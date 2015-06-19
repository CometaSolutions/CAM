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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CILAssemblyManipulator.Logical;
using CILAssemblyManipulator.Logical.Implementation;
using CollectionsWithRoles.API;
using CommonUtils;
using CILAssemblyManipulator.Physical;

public static partial class E_CIL
{
   private static Func<CILModule, String, Stream> THROW_INVALID_OPERATION = ( mod, str ) =>
      {
         throw new InvalidOperationException( "Callback to load files from assembly's file table was not specified when this assembly was loaded." );
      };
   private static IEqualityComparer<CILAssemblyName> ASSEMBLY_NAME_COMPARER = ComparerFromFunctions.NewEqualityComparer<CILAssemblyName>( ( x, y ) => String.Equals( x.Name, y.Name ), x => x.Name.GetHashCode() );

   /// <summary>
   /// Creates a new <see cref="CILMethodSignature"/> which has all its information specified from the parameters of this method.
   /// </summary>
   /// <param name="ctx">The current <see cref="CILReflectionContext"/>.</param>
   /// <param name="currentModule">The current <see cref="CILModule"/>.</param>
   /// <param name="callingConventions">The <see cref="UnmanagedCallingConventions"/> for the method signature.</param>
   /// <param name="returnType">The return type for the method signature.</param>
   /// <param name="paramTypes">The types of the parameters.</param>
   /// <returns>A new <see cref="CILMethodSignature"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="ctx"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="currentModule"/>, <paramref name="returnType"/> or any of the types within <paramref name="paramTypes"/> is <c>null</c>.</exception>
   /// <seealso cref="CILMethodSignature"/>
   public static CILMethodSignature NewMethodSignature( this CILReflectionContext ctx, CILModule currentModule, UnmanagedCallingConventions callingConventions, CILTypeBase returnType, params CILTypeBase[] paramTypes )
   {
      return NewMethodSignature( ctx, currentModule, callingConventions, returnType, null, paramTypes.Select( pt => Tuple.Create( (CILCustomModifier[]) null, pt ) ).ToArray() );
   }

   /// <summary>
   /// Creates a new <see cref="CILMethodSignature"/> which has all its information specified from the parameters of this method.
   /// </summary>
   /// <param name="ctx">The current <see cref="CILReflectionContext"/>.</param>
   /// <param name="currentModule">The current <see cref="CILModule"/>.</param>
   /// <param name="callingConventions">The <see cref="UnmanagedCallingConventions"/> for the method signature.</param>
   /// <param name="returnType">The return type for the method signature.</param>
   /// <param name="returnParamMods">The <see cref="CILCustomModifier"/>s for the method signature. May be <c>null</c> if no modifiers should be used.</param>
   /// <param name="parameters">The parameter information for the method signature. Each element is a tuple containing <see cref="CILCustomModifier"/>s and type for the parameter. Custom modifiers array may be <c>null</c> if no modifiers should be used.</param>
   /// <returns>A new <see cref="CILMethodSignature"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="ctx"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="currentModule"/>, <paramref name="returnType"/> or any of the types within <paramref name="parameters"/> is <c>null</c>.</exception>
   /// <seealso cref="CILMethodSignature"/>
   public static CILMethodSignature NewMethodSignature( this CILReflectionContext ctx, CILModule currentModule, UnmanagedCallingConventions callingConventions, CILTypeBase returnType, CILCustomModifier[] returnParamMods, params Tuple<CILCustomModifier[], CILTypeBase>[] parameters )
   {
      if ( ctx == null )
      {
         // Throw nullref explicitly for consistency (since it is 'this' parameter)
         // Because CILMethodSignatureImpl ctor throws ArgumentNullException
         throw new NullReferenceException();
      }
      var cctx = (CILReflectionContextImpl) ctx;
      return new CILMethodSignatureImpl( ctx, currentModule, callingConventions, returnParamMods == null ? null : cctx.CollectionsFactory.NewListProxyFromParams( returnParamMods ), returnType, parameters.Select( t => Tuple.Create( t.Item1 == null ? null : cctx.CollectionsFactory.NewListProxyFromParams( t.Item1 ), t.Item2 ) ).ToList(), null );
   }

   /// <summary>
   /// Creates a new, blank instance of <see cref="CILAssembly"/>.
   /// </summary>
   /// <param name="ctx">The current reflection context.</param>
   /// <param name="name">The name of the assembly.</param>
   /// <returns>New instance of <see cref="CILAssembly"/> with specified <paramref name="name"/> which will have no modules.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="ctx"/> is <c>null</c>.</exception>
   public static CILAssembly NewBlankAssembly( this CILReflectionContext ctx, String name )
   {
      ArgumentValidator.ValidateNotNull( "Reflection context", ctx );

      var ass = ( (CILReflectionContextImpl) ctx ).Cache.NewBlankAssembly();
      ass.Name.Name = name;
      return ass;
   }

   ///// <summary>
   ///// Loads a new instance of <see cref="CILAssembly"/> from the contents of the <paramref name="stream"/>.
   ///// </summary>
   ///// <param name="ctx">The current reflection context.</param>
   ///// <param name="stream">The stream to load assembly from.</param>
   ///// <param name="eArgs">The <see cref="EmittingArguments"/> to be used during loading. After successful assembly load, this will contain some additional information about the loaded assembly.</param>
   ///// <returns>A new instance of <see cref="CILAssembly"/> from the contents of the <paramref name="stream"/>.</returns>
   ///// <exception cref="ArgumentNullException">If <paramref name="ctx"/> or <paramref name="stream"/> or <paramref name="eArgs"/> is <c>null</c>.</exception>
   ///// <exception cref="BadImageFormatException">If the headers or metadata section of the stream has invalid values, or stream contents are otherwise malformed.</exception>
   //public static CILAssembly LoadAssembly( this CILReflectionContext ctx, Stream stream, EmittingArguments eArgs )
   //{
   //   var result = LoadAssembly( (CILReflectionContextImpl) ctx, stream, eArgs, null, null, null );
   //   if ( !eArgs.LazyLoad )
   //   {
   //      ForceLoadAssembly( result );
   //   }
   //   return result;
   //}

   ///// <summary>
   ///// Loads a new instance of <see cref="CILModule"/> with a contents from <paramref name="stream"/>.
   ///// Useful for multi-module assemblies when one doesn't want to load whole assembly.
   ///// </summary>
   ///// <param name="ctx">The current <see cref="CILReflectionContext"/>.</param>
   ///// <param name="stream">The stream to load module from.</param>
   ///// <param name="eArgs">The <see cref="EmittingArguments"/> during loading. After successful module load, the additional information about the module will be available in this object.</param>
   ///// <returns>A new instance of <see cref="CILModule"/> form the contents of the <paramref name="stream"/>.</returns>
   ///// <exception cref="ArgumentNullException">If <paramref name="ctx"/> or <paramref name="stream"/> or <paramref name="eArgs"/> is <c>null</c>.</exception>
   ///// <exception cref="BadImageFormatException">If the headers or metadata section of the stream has invalid values, or stream contents are otherwise malformed.</exception>
   ///// <remarks>
   ///// If the module returned by this method is main module, that is, its <c>Assembly</c> table has exactly one row, then the <see cref="EmittingArguments.ModuleAssemblyLoader"/> callback of <paramref name="eArgs"/> will never be used.
   ///// Therefore it is ok to pass <c>null</c> as the callback to <paramref name="eArgs"/> if the caller is certain the module will always be the main module.
   ///// However, when the module is not the main module, the callback will be used.
   ///// If in that situation the callback is <c>null</c>, <see cref="InvalidOperationException"/> is thrown.
   ///// The callback may be invoked only by accessing the <see cref="CILModule.Assembly"/> property of the returned <see cref="CILModule"/>.
   ///// </remarks>
   //public static CILModule LoadModule( this CILReflectionContext ctx, Stream stream, EmittingArguments eArgs )
   //{
   //   ModuleReader mr = null; MetaDataReader md = null;
   //   var ownerAssembly = eArgs.ModuleAssemblyLoader;
   //   var result = LoadModule( (CILReflectionContextImpl) ctx, stream, eArgs, mod =>
   //   {
   //      CILAssemblyImpl ass;
   //      if ( md.assembly.Length == 1 )
   //      {
   //         ass = LoadAssembly( (CILReflectionContextImpl) ctx, null, eArgs, mr, md, mod );
   //      }
   //      else
   //      {
   //         if ( ownerAssembly == null )
   //         {
   //            throw new InvalidOperationException( "The callback to get assembly of this module was not provided." );
   //         }
   //         ass = (CILAssemblyImpl) ownerAssembly( mod );
   //         var mods = ass.InternalModules;
   //         lock ( mods.Lock )
   //         {
   //            if ( !mods.Value.CQ.Contains( mod ) )
   //            {
   //               mods.Value.Add( mod );
   //            }
   //         }
   //      }
   //      return ass;
   //   }, out mr, out md );

   //   if ( !eArgs.LazyLoad )
   //   {
   //      ForceLoadModule( result );
   //   }
   //   return result;
   //}

   //private static CILModule LoadModule( CILReflectionContextImpl cctx, Stream stream, EmittingArguments eArgs, Func<CILModule, CILAssembly> modOwnerLoader, out ModuleReader moduleReaderOut, out MetaDataReader mdOut )
   //{
   //   ArgumentValidator.ValidateNotNull( "Reflection context", cctx );
   //   ArgumentValidator.ValidateNotNull( "Stream", stream );
   //   ArgumentValidator.ValidateNotNull( "Emitting arguments", eArgs );

   //   DLLFlags dllFlags; TargetRuntime targetRuntime;
   //   ModuleReader moduleReader;
   //   MetaDataReader md;
   //   IDictionary<String, ManifestResource> mResources;
   //   try
   //   {
   //      moduleReaderOut = new ModuleReader( cctx, stream, eArgs, out targetRuntime, out dllFlags, out mdOut, out mResources );
   //      moduleReader = moduleReaderOut;
   //      md = mdOut;
   //      eArgs.AssemblyRefs.Clear();
   //      foreach ( var aRef in md.assemblyRef )
   //      {
   //         eArgs.AssemblyRefs.Add( new CILAssemblyName( aRef.Item7, aRef.Item1, aRef.Item2, aRef.Item3, aRef.Item4, AssemblyHashAlgorithm.None, aRef.Item5, aRef.Item6, aRef.Item8 ) );
   //      }
   //   }
   //   catch ( Exception x )
   //   {
   //      if ( x is BadImageFormatException )
   //      {
   //         throw;
   //      }
   //      else
   //      {
   //         throw new BadImageFormatException( "Exception when loading assembly (" + x + ").", x );
   //      }
   //   }

   //   var retVal = cctx.Cache.NewModule( mID => new CILModuleImpl(
   //      cctx,
   //      mID,
   //      new LazyWithLock<ListProxy<CILCustomAttribute>>( () =>
   //      {
   //         // Force evaluation of module types ( to get stuff populated in module reader)
   //         var dummy = cctx.Cache.ResolveModuleID( mID ).DefinedTypes;
   //         return moduleReader.ReadModuleCustomAttributes( mID );
   //      } ),
   //      () => modOwnerLoader( cctx.Cache.ResolveModuleID( mID ) ),
   //      md.module[0].Item2,
   //      () => moduleReader.GetModuleInitializer(),
   //      () => moduleReader.CreateLogicalStructure(),
   //      () => moduleReader._mscorLibRef.Value,
   //      mResources
   //      ) );
   //   moduleReader.SetThisModule( retVal );

   //   eArgs.SetCLREntryPoint( () =>
   //   {
   //      CILMethod epMethod;
   //      if ( moduleReader.HasEntryPoint() )
   //      {
   //         // Initialize logical structures first
   //         var dummy = retVal.DefinedTypes;
   //         // Get the CILMethod
   //         epMethod = moduleReader.GetEntryPoint();
   //      }
   //      else
   //      {
   //         epMethod = null;
   //      }
   //      return epMethod;
   //   } );
   //   return retVal;
   //}

   //private static CILAssemblyImpl LoadAssembly( CILReflectionContextImpl cctx, Stream stream, EmittingArguments eArgs, ModuleReader existingModuleReader, MetaDataReader existingMD, CILModule existingModule )
   //{
   //   ArgumentValidator.ValidateNotNull( "Reflection context", cctx );
   //   ArgumentValidator.ValidateNotNull( "Emitting arguments", eArgs );

   //   var fileStreamOpener = eArgs.FileStreamOpener ?? THROW_INVALID_OPERATION;

   //   var moduleReader = existingModuleReader;
   //   var md = existingMD;
   //   var thisModule = existingModule;

   //   CILAssemblyImpl result = null;
   //   result = (CILAssemblyImpl) cctx.Cache.NewAssembly( id => new CILAssemblyImpl(
   //      cctx,
   //      id,
   //      new LazyWithLock<ListProxy<CILCustomAttribute>>( () =>
   //      {
   //         // Force evaluation of module types ( to get stuff populated in module reader)
   //         var dummy = thisModule.DefinedTypes;
   //         return moduleReader.ReadAssemblyCustomAttributes( id );
   //      } ),
   //      () =>
   //      {
   //         var aRow = md.assembly[0];
   //         var aFlags = aRow.Item6;
   //         if ( !aRow.Item7.IsNullOrEmpty() )
   //         {
   //            aFlags |= AssemblyFlags.PublicKey;
   //         }
   //         return new CILAssemblyName( aRow.Item8, aRow.Item2, aRow.Item3, aRow.Item4, aRow.Item5, aRow.Item1, aFlags, aRow.Item7, aRow.Item9 );
   //      },
   //      () =>
   //      {
   //         var list = new List<CILModule>();
   //         list.Add( thisModule );
   //         list.AddRange( md.file
   //            .Where( f => f.Item1.ContainsMetadata() )
   //            .Select( f =>
   //            {
   //               using ( var strm = fileStreamOpener( thisModule, f.Item2 ) )
   //               {
   //                  ModuleReader mRdr;
   //                  MetaDataReader mdRdr;
   //                  return LoadModule( cctx, strm, eArgs, mod => result, out mRdr, out mdRdr );
   //               }
   //            } ) );
   //         return cctx.CollectionsFactory.NewListProxy( list );
   //      },
   //      () => cctx.CollectionsFactory.NewDictionaryProxy( md.exportedType
   //            .Where( eRow => eRow.Item1.IsTypeForwarder() && eRow.Item5.table == Tables.AssemblyRef || eRow.Item5.table == Tables.ExportedType )
   //            .Select( eRow => moduleReader.ResolveExportedType( eRow.Item5, (TypeAttributes) eRow.Item1, eRow.Item4, eRow.Item3 ) )
   //            .GroupBy( tf => Tuple.Create( tf.Name, tf.Namespace ) )
   //            .ToDictionary( g => g.Key, g => g.First() ) ),
   //      () => thisModule
   //      ) );

   //   if ( thisModule == null )
   //   {
   //      thisModule = LoadModule( cctx, stream, eArgs, mod => result, out moduleReader, out md );
   //   }

   //   if ( md.assembly.Length != 1 )
   //   {
   //      throw new BadImageFormatException( "Assembly table had " + ( md.assembly.Length == 0 ? "too few" : "too many" ) + " rows, exactly one expected, but had " + md.assembly.Length + "." );
   //   }

   //   return result;
   //}

   ///// <summary>
   ///// Creates a new <see cref="EmittingAssemblyMapper"/> to be used to redirect references from currently loaded runtime to other runtime.
   ///// The assemblies of other runtime should be located in given <paramref name="assemblyDirectory"/>.
   ///// </summary>
   ///// <param name="ctx">The <see cref="CILReflectionContext"/>.</param>
   ///// <param name="thisRuntimeRoot">The directory containing assemblies of currently loaded runtime.</param>
   ///// <param name="assemblyDirectory">The directory where moniker assemblies reside. This should have its last character as the directory separator of the current operating system.</param>
   ///// <param name="streamOpener">The callback to open a file given a specific path.</param>
   ///// <param name="monikerInfo">The information about framework moniker.</param>
   ///// <returns>A new <see cref="EmittingAssemblyMapper"/> that will redirect references from currently loaded runtime to other runtime.</returns>
   ///// <exception cref="ArgumentNullException">If <paramref name="thisRuntimeRoot"/>, <paramref name="assemblyDirectory"/>, <paramref name="monikerInfo"/> or <paramref name="streamOpener"/> is <c>null</c>.</exception>
   ///// <exception cref="NullReferenceException">If <paramref name="ctx"/> is <c>null</c>.</exception>
   //public static EmittingAssemblyMapper CreateMapperForFrameworkMoniker( this CILReflectionContext ctx, String thisRuntimeRoot, String assemblyDirectory, Func<String, Stream> streamOpener, FrameworkMonikerInfo monikerInfo )
   //{
   //   if ( ctx == null )
   //   {
   //      throw new NullReferenceException();
   //   }

   //   ArgumentValidator.ValidateNotNull( "This runtime root directory", thisRuntimeRoot );
   //   ArgumentValidator.ValidateNotNull( "Moniker information.", monikerInfo );

   //   var loader = new AssemblyLoaderFromBasePath( ctx, assemblyDirectory, streamOpener, monikerInfo.MsCorLibAssembly );
   //   return new CachingEmittingAssemblyMapper( monikerInfo.Assemblies.ToDictionary(
   //      kvp => new CILAssemblyName( kvp.Key, kvp.Value.Item1.Major, kvp.Value.Item1.Minor, kvp.Value.Item1.Build, kvp.Value.Item1.Revision ),
   //      kvp => new Lazy<CILAssembly>( () => loader.LoadFromStreamFunction( assemblyDirectory + "\\" + kvp.Key + ".dll" ) ),
   //      ASSEMBLY_NAME_COMPARER ),
   //      monikerInfo.Assemblies.ToDictionary(
   //         kvp => new CILAssemblyName( kvp.Key, kvp.Value.Item1.Major, kvp.Value.Item1.Minor, kvp.Value.Item1.Build, kvp.Value.Item1.Revision ),
   //         kvp => new Lazy<CILAssembly>( () => loader.LoadFromStreamFunction( thisRuntimeRoot + "\\" + kvp.Key + ".dll" ) ),
   //         ASSEMBLY_NAME_COMPARER )
   //      );
   //}

   //private static void ForceLoadAssembly( CILAssembly assembly )
   //{
   //   var dummy1 = assembly.Name;
   //   var dummy2 = assembly.CustomAttributeData;
   //   var dummy3 = assembly.ForwardedTypeInfos;
   //   foreach ( var mod in assembly.Modules )
   //   {
   //      ForceLoadModule( mod );
   //   }
   //}

   //private static void ForceLoadModule( CILModule module )
   //{
   //   var dummy1 = module.Assembly;
   //   var dummy2 = module.AssociatedMSCorLibModule;
   //   var dummy3 = module.CustomAttributeData;
   //   var dummy4 = module.Name;
   //   var dummy5 = module.ModuleInitializer;
   //   foreach ( var type in module.DefinedTypes )
   //   {
   //      ForceLoadType( type );
   //   }

   //}

   //private static void ForceLoadType( CILType t )
   //{
   //   var sucka = t.Name;
   //   sucka = t.Namespace;
   //   var sucka1 = t.CustomAttributeData;
   //   var sucka2 = t.ElementKind;
   //   var sucka3 = t.ElementType;
   //   var sucka31 = t.DeclaredInterfaces;
   //   var sucka32 = t.Layout;
   //   var sucka33 = t.TypeCode;
   //   var sucka34 = t.Module;
   //   foreach ( var g in t.GenericArguments )
   //   {
   //      var sucka4 = ( (CILTypeParameter) g ).GenericParameterConstraints;
   //      if ( g is CILTypeParameter )
   //      {
   //         var sucka41 = ( (CILTypeParameter) g ).CustomAttributeData;
   //      }
   //   }
   //   foreach ( var f in t.DeclaredFields )
   //   {
   //      var sucka5 = f.FieldType;
   //      if ( f.Name == "LunarMonthLen" )
   //      {

   //      }
   //      var sucka6 = f.DeclaringType;
   //      var sucka61 = f.CustomAttributeData;
   //      var sucka62 = f.ConstantValue;
   //      var sucka63 = f.MarshalingInformation;
   //   }
   //   var sucka7 = t.DeclaringType;
   //   var sucka8 = t.ArrayInformation;
   //   var sucka9 = t.BaseType;
   //   foreach ( var c in t.Constructors )
   //   {
   //      foreach ( var cp in c.Parameters )
   //      {
   //         ForceLoadParameter( cp );
   //      }
   //      var sucka91 = c.CustomAttributeData;
   //      var sucka92 = c.MethodIL;
   //      var sucka93 = c.DeclaringType;
   //   }

   //   foreach ( var m in t.DeclaredMethods )
   //   {
   //      var sucka12 = m.Name;
   //      foreach ( var mp in m.Parameters )
   //      {
   //         ForceLoadParameter( mp );
   //      }
   //      ForceLoadParameter( m.ReturnParameter );

   //      var sucka121 = m.CustomAttributeData;
   //      var sucka122 = m.OverriddenMethods;
   //      var sucka123 = m.MethodIL;
   //      var sucka124 = m.GenericArguments;
   //      var sucka125 = m.GenericDefinition;
   //      var sucka126 = m.IsTrueDefinition;
   //      var sucka127 = m.ImplementationAttributes;
   //      var sucka128 = m.DeclaringType;
   //   }

   //   foreach ( var p in t.DeclaredProperties )
   //   {
   //      var sucka17 = p.ConstantValue;
   //      var sucka18 = p.GetMethod;
   //      var sucka19 = p.SetMethod;
   //      var sucka24 = p.CustomAttributeData;
   //   }
   //   foreach ( var e in t.DeclaredEvents )
   //   {
   //      var sucka20 = e.AddMethod;
   //      var sucka21 = e.RemoveMethod;
   //      var sucka22 = e.RaiseMethod;
   //      var sucka23 = e.OtherMethods;
   //      var sucka25 = e.CustomAttributeData;
   //   }
   //}

   //private static void ForceLoadParameter( CILParameter param )
   //{
   //   var sucka13 = param.ParameterType;
   //   var sucka14 = param.Name;
   //   var sucka141 = param.CustomAttributeData;
   //   var sucka142 = param.ConstantValue;
   //   var sucka143 = param.Method;
   //   var sucka144 = param.CustomModifiers;
   //   var sucka145 = param.MarshalingInformation;
   //}
}