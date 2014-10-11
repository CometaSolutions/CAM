/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
 *
 * Licensed  under the  Apache License,  Version 2.0  (the "License");
 * you may not use  this file  except in  compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed  under the  License is distributed on an "AS IS" BASIS,
 * WITHOUT  WARRANTIES OR CONDITIONS  OF ANY KIND, either  express  or
 * implied.
 *
 * See the License for the specific language governing permissions and
 * limitations under the License. 
 */
using CILAssemblyManipulator.API;
using CommonUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Implementation
{
   internal class CachingEmittingAssemblyMapper : EmittingAssemblyMapper
   {

      private readonly IDictionary<CILAssemblyName, Lazy<CILAssembly>> _assemblies;
      private readonly IDictionary<CILAssemblyName, Lazy<CILAssembly>> _runtimeAssemblies;
      private readonly ConcurrentDictionary<CILType, CILType> _typeCache;
      private readonly ConcurrentDictionary<CILTypeParameter, CILTypeParameter> _typeParameterCache;
      private readonly ConcurrentDictionary<CILConstructor, CILConstructor> _ctorCache;
      private readonly ConcurrentDictionary<CILMethod, CILMethod> _methodCache;
      private readonly ConcurrentDictionary<CILField, CILField> _fieldCache;

      internal CachingEmittingAssemblyMapper( IDictionary<CILAssemblyName, Lazy<CILAssembly>> monikers, IDictionary<CILAssemblyName, Lazy<CILAssembly>> runtime )
      {
         this._assemblies = monikers;
         this._runtimeAssemblies = runtime; // monikers.Keys.ToDictionary( key => key, key => new Lazy<CILAssembly>( () => System.Reflection.Assembly.Load( key.Name ).NewWrapper( ctx ), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication ) );
         this._typeCache = new ConcurrentDictionary<CILType, CILType>();
         this._typeParameterCache = new ConcurrentDictionary<CILTypeParameter, CILTypeParameter>();
         this._ctorCache = new ConcurrentDictionary<CILConstructor, CILConstructor>();
         this._methodCache = new ConcurrentDictionary<CILMethod, CILMethod>();
         this._fieldCache = new ConcurrentDictionary<CILField, CILField>();
      }

      #region EmittingAssemblyMapper Members

      public CILTypeBase MapTypeBase( CILTypeBase type )
      {
         return TypeKind.Type == type.TypeKind ? (CILTypeBase) this.MapType( (CILType) type ) : this.MapTypeParameter( (CILTypeParameter) type );
      }

      public CILMethodBase MapMethodBase( CILMethodBase method )
      {
         return MethodKind.Method == method.MethodKind ? (CILMethodBase) this.MapMethod( (CILMethod) method ) : this.MapConstructor( (CILConstructor) method );
      }

      public CILField MapField( CILField field )
      {
         return this._fieldCache.GetOrAdd( field, fieldArg =>
         {
            var mapped = this.MapType( fieldArg.DeclaringType );
            var retVal = mapped.Equals( fieldArg.DeclaringType ) ? fieldArg : mapped.DeclaredFields.FirstOrDefault( f => f.Name == fieldArg.Name && MatchParameterTypes( fieldArg.FieldType, f.FieldType ) );
            if ( retVal == null )
            {
               throw new InvalidOperationException( "Failed to map field " + fieldArg + "." );
            }
            return retVal;
         } );
      }

      public Boolean IsMapped( CILAssemblyName an )
      {
         return this._assemblies.ContainsKey( an );
      }

      public Boolean TryGetMappedAssembly( CILAssemblyName name, out CILAssembly assembly )
      {
         Lazy<CILAssembly> lazy;
         var retVal = this._assemblies.TryGetValue( name, out lazy );
         assembly = retVal ? lazy.Value : null;
         return retVal;
      }

      #endregion

      private Boolean MatchMethodAttrsAndParameters( CILMethodBase original, CILMethodBase mapped )
      {
         return original.Attributes.IsStatic() == mapped.Attributes.IsStatic()
            && original.Parameters.Count == mapped.Parameters.Count
            && original.Parameters.Where( ( p, i ) => MatchParameterTypes( p.ParameterType, mapped.Parameters[i].ParameterType ) ).Count() == original.Parameters.Count;
      }


      private CILTypeParameter MapTypeParameter( CILTypeParameter tParam )
      {
         return this._typeParameterCache.GetOrAdd( tParam, tParamArg =>
         {
            try
            {
               if ( tParamArg.DeclaringMethod != null )
               {
                  return (CILTypeParameter) this.MapMethod( tParamArg.DeclaringMethod ).GenericArguments[tParamArg.GenericParameterPosition];
               }
               else
               {
                  return (CILTypeParameter) this.MapType( tParamArg.DeclaringType ).GenericArguments[tParamArg.GenericParameterPosition];
               }
            }
            catch ( IndexOutOfRangeException )
            {
               throw new InvalidOperationException( "Failed to map " + tParam + " of declaring type " + tParam.DeclaringType + ( tParam.DeclaringMethod == null ? "" : ( " and of declaring method " + tParam.DeclaringMethod + " " ) ) + "." );
            }
         } );
      }

      private CILType MapType( CILType type )
      {
         return this._typeCache.GetOrAdd( type, typeArg =>
         {
            CILType result;
            var elKind = typeArg.ElementKind;
            if ( elKind.HasValue )
            {
               result = (CILType) this.MapTypeBase( typeArg.ElementType ).MakeElementType( elKind.Value, typeArg.ArrayInformation ); ;
            }
            else
            {
               var gArgs = typeArg.GenericArguments.Count > 0 && !typeArg.IsGenericDefinition() ? typeArg.GenericArguments : null;
               var typeArgToUse = typeArg;
               if ( gArgs != null )
               {
                  typeArgToUse = typeArgToUse.GenericDefinition;
               }

               if ( typeArgToUse.DeclaringType != null )
               {
                  result = this.MapType( typeArgToUse.DeclaringType ).DeclaredNestedTypes.FirstOrDefault( t => String.Equals( t.Name, typeArgToUse.Name ) );
               }
               else
               {
                  Lazy<CILAssembly> targetAss;
                  var keyAss = typeArgToUse.Module.Assembly;
                  var key = keyAss.Name;
                  if ( this._assemblies.TryGetValue( key, out targetAss ) )
                  {
                     var mod = targetAss.Value.MainModule; // TODO other modules than MainModule
                     var typeArgToUseName = Utils.NamespaceAndTypeName( typeArgToUse.Namespace, typeArgToUse.Name );
                     result = mod.GetTypeByName( typeArgToUseName, false );
                     if ( result == null )
                     {
                        TypeForwardingInfo tf;

                        // Have to iterate all assemblies to find type forwarder
                        // Loading assembly + reading type forward info doesn't cause generation module logical structure -> should be fast.
                        foreach ( var tAss in this._runtimeAssemblies.Values )
                        {
                           if ( tAss.Value.TryGetTypeForwarder( typeArgToUse.Name, typeArgToUse.Namespace, out tf ) )
                           {
                              // TODO other modules than MainModule
                              result = this._assemblies[tAss.Value.Name].Value.MainModule.GetTypeByName( typeArgToUseName, false );
                              if ( result != null )
                              {
                                 break;
                              }
                           }
                        }


                     }
                  }
                  else
                  {
                     result = typeArgToUse;
                  }
               }

               if ( result == null )
               {
                  throw new InvalidOperationException( "Failed to map type " + typeArgToUse + "." );
               }

               if ( gArgs != null )
               {
                  result = result.MakeGenericType( gArgs.Select( gArg => this.MapTypeBase( gArg ) ).ToArray() );
               }
            }
            return result;
         } );
      }

      private CILConstructor MapConstructor( CILConstructor ctor )
      {
         return this._ctorCache.GetOrAdd( ctor, ctorArg =>
         {
            var mapped = this.MapType( ctorArg.DeclaringType );
            var retVal = mapped.Equals( ctorArg.DeclaringType ) ? ctorArg : mapped.Constructors.FirstOrDefault( c => this.MatchMethodAttrsAndParameters( ctorArg, c ) );
            if ( retVal == null )
            {
               throw new InvalidOperationException( "Failed to map constructor " + ctor + "." );
            }
            return retVal;
         } );
      }

      private CILMethod MapMethod( CILMethod method )
      {
         return this._methodCache.GetOrAdd( method, methodArg =>
         {
            var gArgs = method.GenericArguments.Count > 0 && !method.IsGenericDefinition() ? method.GenericArguments : null;
            var methodToUse = methodArg;
            if ( gArgs != null )
            {
               methodToUse = methodToUse.GenericDefinition;
            }
            var mapped = this.MapType( methodArg.DeclaringType );
            var result = mapped.Equals( methodToUse.DeclaringType ) ?
               methodToUse :
               mapped.DeclaredMethods.FirstOrDefault(
                  m => String.Equals( m.Name, methodToUse.Name )
                     && m.GenericArguments.Count == ( gArgs == null ? 0 : gArgs.Count )
                     && MatchParameterTypes( methodToUse.ReturnParameter.ParameterType, m.ReturnParameter.ParameterType )
                     && this.MatchMethodAttrsAndParameters( methodToUse, m ) );

            if ( result == null )
            {
               throw new InvalidOperationException( "Failed to map method " + methodToUse + "." );
            }

            if ( gArgs != null )
            {
               result = result.MakeGenericMethod( gArgs.Select( gArg => this.MapTypeBase( gArg ) ).ToArray() );
            }

            return result;
         } );
      }

      private static String GetTypeString( CILType type )
      {
         return ( type.Namespace == null || type.Namespace.Length == 0 ? "" : ( type.Namespace + "." ) ) + type.Name;
      }

      private static Boolean MatchParameterTypes( CILTypeBase type1, CILTypeBase type2 )
      {
         // TODO if tParam -> match also decl method
         // TODO we here assume that portable assemblies won't contain method signature types
         if ( TypeKind.MethodSignature == type1.TypeKind || TypeKind.MethodSignature == type2.TypeKind )
         {
            return false;
         }
         return type1 == null && type2 == null || ( type1 != null && type2 != null && type1.TypeKind == type2.TypeKind
            && ( type1.TypeKind == TypeKind.Type ?
                ( GetTypeString( (CILType) type1 ) == GetTypeString( (CILType) type2 ) ) :
                ( ( (CILTypeParameter) type1 ).GenericParameterPosition == ( (CILTypeParameter) type2 ).GenericParameterPosition
                  && MatchParameterTypes( ( (CILTypeParameter) type1 ).DeclaringType, ( (CILTypeParameter) type2 ).DeclaringType ) ) ) );
      }
   }
   internal class AssemblyLoaderFromBasePath
   {
      private readonly CILReflectionContext _ctx;
      private readonly ConcurrentDictionary<String, CILAssembly> _loadedAssemblies;
      private readonly String _portableLibDirectory;
      private readonly Func<String, Stream> _streamOpener;
      private readonly EmittingArguments _loadArgs;

      internal AssemblyLoaderFromBasePath( CILReflectionContext ctx, String fwLibDirectory, Func<String, Stream> streamOpener, String corLibName )
      {
         ArgumentValidator.ValidateNotNull( "Reflection context", ctx );
         ArgumentValidator.ValidateNotNull( "Assembly directory", fwLibDirectory );
         ArgumentValidator.ValidateNotNull( "Stream opener callback", streamOpener );

         if ( corLibName == null )
         {
            corLibName = Consts.MSCORLIB_NAME;
         }

         this._ctx = ctx;
         this._loadedAssemblies = new ConcurrentDictionary<String, CILAssembly>();
         this._portableLibDirectory = fwLibDirectory; // fwLibDirectory.EndsWith( "\\" ) ? fwLibDirectory : ( fwLibDirectory + "\\" );
         this._streamOpener = streamOpener;
         this._loadArgs = EmittingArguments.CreateForLoadingModule( this.LoadFunction );
         this._loadArgs.CorLibName = corLibName;
      }

      private CILAssembly LoadFunction( CILModule module, CILAssemblyName aName )
      {
         return this._loadedAssemblies.GetOrAdd( this._portableLibDirectory + aName.Name + ".dll", pathArg =>
         {
            using ( var stream = this._streamOpener( pathArg ) )
            {
               return module.ReflectionContext.LoadAssembly( stream, this._loadArgs );
            }
         } );
      }

      internal CILAssembly LoadFromStreamFunction( String str )
      {
         return this._loadedAssemblies.GetOrAdd( str, pathArg =>
         {
            using ( var stream = this._streamOpener( str ) )
            {
               return this._ctx.LoadAssembly( stream, this._loadArgs );
            }
         } );
      }
   }
}