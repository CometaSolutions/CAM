/*
 * Copyright 2015 Stanislav Muhametsin. All rights Reserved.
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
using CILAssemblyManipulator.Logical;
using CILAssemblyManipulator.Physical;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public static partial class E_CILLogical
{
   private sealed class LogicalCreationState
   {
      private readonly CILModule _module;
      private readonly CILMetaData _md;
      private readonly Func<String, CILModule> _moduleRefResolver;
      private readonly Func<AssemblyReference, CILAssembly> _assemblyRefResolver;

      private readonly List<CILType> _typeDefs;
      private readonly List<CILField> _fieldDefs;
      private readonly List<CILMethodBase> _methodDefs;
      private readonly List<CILParameter> _parameterDefs;
      private readonly List<CILTypeParameter> _typeParameters;
      private readonly List<CILModule> _moduleRefs;
      private readonly List<CILAssembly> _assemblyRefs;

      private readonly ISet<Int32> _topLevelTypes;
      private readonly IDictionary<Int32, IList<Int32>> _nestedTypes;

      private readonly IDictionary<SignatureElementTypes, CILType> _simpleTypes;
      private readonly List<CILType> _typeRefs;
      private readonly IDictionary<Tuple<String, String>, CILType> _topLevelTypesByName;



      internal LogicalCreationState(
         CILModule module,
         CILMetaData md,
         Func<String, CILModule> moduleRefResolver,
         Func<AssemblyReference, CILAssembly> assemblyRefResolver
         )
      {
         this._module = module;
         this._md = md;
         this._moduleRefResolver = moduleRefResolver;
         this._assemblyRefResolver = assemblyRefResolver;

         var tDefCount = md.TypeDefinitions.RowCount;
         var fDefCount = md.FieldDefinitions.RowCount;
         var mDefCount = md.MethodDefinitions.RowCount;
         var pDefCount = md.ParameterDefinitions.RowCount;
         var tParamCount = md.GenericParameterDefinitions.RowCount;
         var tRefCount = md.TypeReferences.RowCount;
         var modRefCount = md.ModuleReferences.RowCount;
         var aRefCount = md.AssemblyReferences.RowCount;

         this._typeDefs = new List<CILType>( tDefCount );
         this._fieldDefs = new List<CILField>( fDefCount );
         this._methodDefs = new List<CILMethodBase>( mDefCount );
         this._parameterDefs = new List<CILParameter>( pDefCount );
         this._typeParameters = new List<CILTypeParameter>( tParamCount );
         this._typeRefs = new List<CILType>( tRefCount );
         this._moduleRefs = new List<CILModule>( modRefCount );
         this._assemblyRefs = new List<CILAssembly>( aRefCount );

         this._typeDefs.AddRange( Enumerable.Repeat<CILType>( null, tDefCount ) );
         this._fieldDefs.AddRange( Enumerable.Repeat<CILField>( null, fDefCount ) );
         this._methodDefs.AddRange( Enumerable.Repeat<CILMethodBase>( null, mDefCount ) );
         this._parameterDefs.AddRange( Enumerable.Repeat<CILParameter>( null, pDefCount ) );
         this._typeParameters.AddRange( Enumerable.Repeat<CILTypeParameter>( null, tParamCount ) );
         this._typeRefs.AddRange( Enumerable.Repeat<CILType>( null, tRefCount ) );
         this._moduleRefs.AddRange( Enumerable.Repeat<CILModule>( null, modRefCount ) );
         this._assemblyRefs.AddRange( Enumerable.Repeat<CILAssembly>( null, aRefCount ) );

         var nestedTypes = new Dictionary<Int32, IList<Int32>>();
         foreach ( var nc in md.NestedClassDefinitions.TableContents )
         {
            nestedTypes
               .GetOrAdd_NotThreadSafe( nc.EnclosingClass.Index, i => new List<Int32>() )
               .Add( nc.NestedClass.Index );
         }
         var tlTypes = new HashSet<Int32>( Enumerable.Range( 0, tDefCount ) );
         tlTypes.ExceptWith( nestedTypes.Values.SelectMany( v => v ) );

         this._topLevelTypes = tlTypes;
         this._nestedTypes = nestedTypes;

         this._simpleTypes = new Dictionary<SignatureElementTypes, CILType>();
         this._topLevelTypesByName = new Dictionary<Tuple<String, String>, CILType>();
      }

      public CILModule Module
      {
         get
         {
            return this._module;
         }
      }

      public CILMetaData MetaData
      {
         get
         {
            return this._md;
         }
      }

      public IDictionary<Int32, IList<Int32>> NestedTypes
      {
         get
         {
            return this._nestedTypes;
         }
      }

      public ISet<Int32> TopLevelTypes
      {
         get
         {
            return this._topLevelTypes;
         }
      }

      internal CILType GetTypeDef( Int32 typeDefIndex )
      {
         return this._typeDefs[typeDefIndex];
      }

      internal CILMethodBase GetMethodDef( Int32 methodDefIndex )
      {
         return this._methodDefs[methodDefIndex];
      }

      internal CILTypeParameter GetTypeParameter( Int32 tParamIndex )
      {
         return this._typeParameters[tParamIndex];
      }

      internal void RecordTypeDef( CILType type, Int32 typeDefIndex )
      {
         this._typeDefs[typeDefIndex] = type;

         if ( this._topLevelTypes.Contains( typeDefIndex ) )
         {
            this._topLevelTypesByName[Tuple.Create( type.Namespace, type.Name )] = type;
         }
      }

      internal void RecordFieldDef( CILField field, Int32 fieldDefIndex )
      {
         this._fieldDefs[fieldDefIndex] = field;
      }

      internal void RecordMethodDef( CILMethodBase method, Int32 methodDefIndex )
      {
         this._methodDefs[methodDefIndex] = method;
      }

      internal void RecordParameter( CILParameter parameter, Int32 paramDefIndex )
      {
         this._parameterDefs[paramDefIndex] = parameter;
      }

      internal void RecordTypeParameter( CILTypeParameter parameter, Int32 gParamIndex )
      {
         this._typeParameters[gParamIndex] = parameter;
      }

      internal CILTypeBase ResolveTypeDefOrRefOrSpec( TableIndex index, CILType contextType, CILMethodBase contextMethod )
      {
         switch ( index.Table )
         {
            case Tables.TypeDef:
               return this._typeDefs[index.Index];
            case Tables.TypeRef:
               return this._typeRefs.GetOrAdd_NotThreadSafe( index.Index, i => this.ResolveTypeRef( i ) );
            case Tables.TypeSpec:
               return this.ResolveTypeSignature( this._md.TypeSpecifications.TableContents[index.Index].Signature, contextType, contextMethod );
            default:
               throw new InvalidOperationException( "Unexpected TypeDef/Ref/Spec: " + index + "." );
         }
      }

      private CILType ResolveTypeRef( Int32 tRefIdx )
      {
         var tRef = this._md.TypeReferences.TableContents[tRefIdx];
         var resScopeNullable = tRef.ResolutionScope;
         CILType retVal;
         if ( resScopeNullable.HasValue )
         {
            var resScope = resScopeNullable.Value;
            switch ( resScope.Table )
            {
               case Tables.TypeRef:
                  retVal = this.ResolveTypeRef( resScope.Index ).DeclaredNestedTypes.FirstOrDefault( n => String.Equals( n.Namespace, tRef.Namespace ) && String.Equals( n.Name, tRef.Name ) );
                  break;
               case Tables.AssemblyRef:
                  retVal = this._assemblyRefs.GetOrAdd_NotThreadSafe( resScope.Index, i => this._assemblyRefResolver( this._md.AssemblyReferences.TableContents[i] ) )
                     .MainModule // TODO probably should seek whole ExportedTypes table!
                     .GetTypeByName( LogicalUtils.CombineTypeAndNamespace( tRef.Name, tRef.Namespace ) );
                  break;
               case Tables.Module:
                  this._topLevelTypesByName.TryGetValue( Tuple.Create( tRef.Namespace, tRef.Name ), out retVal );
                  break;
               case Tables.ModuleRef:
                  retVal = this._moduleRefs.GetOrAdd_NotThreadSafe( resScope.Index, i => this._moduleRefResolver( this._md.ModuleReferences.TableContents[i].ModuleName ) )
                     .GetTypeByName( LogicalUtils.CombineTypeAndNamespace( tRef.Name, tRef.Namespace ) );
                  break;
               default:
                  throw new InvalidOperationException( "Unexpected TypeRef resolution scope: " + resScope + "." );
            }
         }
         else
         {
            throw new NotImplementedException( "Null resolution scope." );
         }

         return retVal;
      }

      private CILTypeBase ResolveTypeSignature( TypeSignature sig, CILType contextType, CILMethodBase contextMethod )
      {
         switch ( sig.TypeSignatureKind )
         {
            case TypeSignatureKind.ClassOrValue:
               var clazz = (ClassOrValueTypeSignature) sig;
               var cilClazz = this.ResolveTypeDefOrRefOrSpec( clazz.Type, contextType, contextMethod );
               if ( clazz.GenericArguments.Count > 0 )
               {
                  cilClazz = ( (CILType) cilClazz ).MakeGenericType( clazz.GenericArguments.Select( arg => this.ResolveTypeSignature( arg, contextType, contextMethod ) ).ToArray() );
               }
               return cilClazz;
            case TypeSignatureKind.ComplexArray:
               var array = (ComplexArrayTypeSignature) sig;
               return this.ResolveTypeSignature( array.ArrayType, contextType, contextMethod ).MakeArrayType( array.Rank, array.Sizes.ToArray(), array.LowerBounds.ToArray() );
            case TypeSignatureKind.FunctionPointer:
               var fn = ( (FunctionPointerTypeSignature) sig ).MethodSignature;
               return this._module.ReflectionContext.NewMethodSignature(
                  this._module,
                  (UnmanagedCallingConventions) fn.SignatureStarter,
                  this.ResolveParamSignature( fn.ReturnType, contextType, contextMethod ),
                  fn.ReturnType.CustomModifiers.Select( cm => CILCustomModifierFactory.CreateModifier( cm.IsOptional, (CILType) this.ResolveTypeDefOrRefOrSpec( cm.CustomModifierType, contextType, contextMethod ) ) ).ToArray(),
                  fn.Parameters.Select( p => Tuple.Create( p.CustomModifiers.Select( cm => CILCustomModifierFactory.CreateModifier( cm.IsOptional, (CILType) this.ResolveTypeDefOrRefOrSpec( cm.CustomModifierType, contextType, contextMethod ) ) ).ToArray(), this.ResolveParamSignature( p, contextType, contextMethod ) ) ).ToArray()
                  );
            case TypeSignatureKind.GenericParameter:
               var gSig = (GenericParameterTypeSignature) sig;
               return gSig.IsTypeParameter ? contextType.GenericArguments[gSig.GenericParameterIndex] : ( (CILMethod) contextMethod ).GenericArguments[gSig.GenericParameterIndex];
            case TypeSignatureKind.Pointer:
               return this.ResolveTypeSignature( ( (PointerTypeSignature) sig ).PointerType, contextType, contextMethod ).MakePointerType();
            case TypeSignatureKind.Simple:
               return this._simpleTypes.GetOrAdd_NotThreadSafe( ( (SimpleTypeSignature) sig ).SimpleType, st =>
               {
                  throw new NotImplementedException();
               } );
            case TypeSignatureKind.SimpleArray:
               return this.ResolveTypeSignature( ( (SimpleArrayTypeSignature) sig ).ArrayType, contextType, contextMethod ).MakeArrayType();
            default:
               throw new InvalidOperationException( "Unrecognized type signature kind: " + sig.TypeSignatureKind + "." );
         }
      }

      private CILTypeBase ResolveParamSignature( ParameterSignature sig, CILType contextType, CILMethodBase contextMethod )
      {
         var retVal = this.ResolveTypeSignature( sig.Type, contextType, contextMethod );
         if ( sig.IsByRef )
         {
            retVal = retVal.MakeByRefType();
         }

         return retVal;
      }
   }

   public static CILAssembly CreateLogicalRepresentation( this CILReflectionContext ctx, CILMetaData metaData, Func<String, Stream> streamOpenCallback )
   {

      var aDefList = metaData.AssemblyDefinitions.TableContents;
      if ( aDefList.Count <= 0 )
      {
         throw new InvalidOperationException( "The physical metadata does not contain assembly information." );
      }
      var aDef = aDefList[0];
      var retVal = ctx.NewBlankAssembly( aDef.AssemblyInformation.Name );

      var an = retVal.Name;
      aDef.AssemblyInformation.DeepCopyContentsTo( an.AssemblyInformation );
      an.HashAlgorithm = aDef.HashAlgorithm;
      an.Flags = aDef.Attributes;

      var modList = metaData.ModuleDefinitions.TableContents;
      if ( modList.Count <= 0 )
      {
         throw new InvalidOperationException( "The physical metadata does not contain module information." );
      }

      // Adding first module will make it main module (TODO this is a bit un-intuitive...)
      var allModuleStates = new Dictionary<String, LogicalCreationState>();
      var moduleRefResolver = new Func<String, CILModule>( modName =>
      {
         LogicalCreationState retModule;
         if ( !allModuleStates.TryGetValue( modName, out retModule ) )
         {
            throw new InvalidOperationException( "No module named \"" + modName + "\" exists in this assembly." );
         }
         return retModule.Module;
      } );
      var assemblyRefResolver = new Func<AssemblyReference, CILAssembly>( aRef =>
      {
         throw new NotImplementedException();
      } );
      var mainModuleState = retVal.AddModule( modList[0].Name ).CreateLogicalCreationState( metaData, moduleRefResolver, assemblyRefResolver );
      allModuleStates[mainModuleState.Module.Name] = mainModuleState;

      foreach ( var module in metaData.FileReferences.TableContents.Where( f => f.Attributes.ContainsMetadata() ) )
      {
         CILMetaData moduleMD = null;
         var name = module.Name;
         using ( var stream = streamOpenCallback( name ) )
         {
            moduleMD = stream.ReadModule();
         }
         var cilModule = retVal.AddModule( name );
         allModuleStates[name] = cilModule.CreateLogicalCreationState( moduleMD, moduleRefResolver, assemblyRefResolver );
      }

      // Process rest of the stuff now because of possible cross-module references (we have to have instances of CILModule existing at this point)
      foreach ( var state in allModuleStates.Values )
      {
         state.ProcessLogicalModule( state.Module );
      }
      mainModuleState.PopulateLogicalAssembly( retVal );

      return retVal;
   }

   private static LogicalCreationState CreateLogicalCreationState(
      this CILModule module,
      CILMetaData metaData,
      Func<String, CILModule> moduleRefResolver,
      Func<AssemblyReference, CILAssembly> assemblyRefResolver
      )
   {
      // The module name is set by all places calling this method, so don't set that
      var state = new LogicalCreationState( module, metaData, moduleRefResolver, assemblyRefResolver );

      // TODO
      module.AssociatedMSCorLibModule = null;

      var tDefs = metaData.TypeDefinitions.TableContents;
      if ( tDefs.Count > 0 )
      {
         state.ProcessTypeDef( 0, module.ModuleInitializer );

         for ( var i = 1; i < tDefs.Count; ++i )
         {
            if ( state.TopLevelTypes.Contains( i ) )
            {
               state.CreateLogicalType( module, i );
            }
         }
      }
      return state;
   }

   private static void CreateLogicalType( this LogicalCreationState state, CILElementCapableOfDefiningType owner, Int32 typeDefIndex )
   {

      var md = state.MetaData;
      var tDef = md.TypeDefinitions.TableContents[typeDefIndex];

      CILTypeCode tc;

      Int32 enumValueFieldIndex;
      if ( md.IsEnum( new TableIndex( Tables.TypeDef, typeDefIndex ) ) && md.TryGetEnumValueFieldIndex( typeDefIndex, out enumValueFieldIndex ) )
      {
         tc = ResolveTypeCodeFromEnumType( md.FieldDefinitions.TableContents[enumValueFieldIndex].Signature.Type );
      }
      else
      {
         tc = ResolveTypeCodeTextual( tDef.Namespace, tDef.Name );
      }

      var retVal = state.GetTypeDef( typeDefIndex );
      if ( retVal == null )
      {
         retVal = owner.AddType( tDef.Name, tDef.Attributes, tc );
         retVal.Namespace = tDef.Namespace;
         state.ProcessTypeDef( typeDefIndex, retVal );
      }
   }

   private static void ProcessTypeDef( this LogicalCreationState state, Int32 typeDefIndex, CILType typeDef )
   {

      state.RecordTypeDef( typeDef, typeDefIndex );

      // Fields
      var md = state.MetaData;
      var fieldDefs = md.FieldDefinitions.TableContents;
      foreach ( var fIdx in md.GetTypeFieldIndices( typeDefIndex ) )
      {
         var field = fieldDefs[fIdx];
         state.RecordFieldDef( typeDef.AddField( field.Name, null, field.Attributes ), fIdx );
      }

      // Methods and parameters
      var methodDefs = md.MethodDefinitions.TableContents;
      var paramDefs = md.ParameterDefinitions.TableContents;
      foreach ( var mIdx in md.GetTypeMethodIndices( typeDefIndex ) )
      {
         var method = methodDefs[mIdx];
         var name = method.Name;
         var isCtor = String.Equals( name, Miscellaneous.INSTANCE_CTOR_NAME ) || String.Equals( name, Miscellaneous.CLASS_CTOR_NAME );
         var cilMethod = isCtor ?
            (CILMethodBase) typeDef.AddConstructor( method.Attributes, method.Signature.SignatureStarter.GetCallingConventionFromSignature() ) :
            typeDef.AddMethod( name, method.Attributes, method.Signature.SignatureStarter.GetCallingConventionFromSignature() );
         state.RecordMethodDef( cilMethod, mIdx );

         foreach ( var pIdx in md.GetMethodParameterIndices( mIdx ).OrderBy( p => paramDefs[p].Sequence ) )
         {
            var param = paramDefs[pIdx];
            CILParameter cilParam;
            if ( param.Sequence > 0 )
            {
               cilParam = cilMethod.AddParameter( param.Name, param.Attributes, null );
            }
            else
            {
               cilParam = isCtor ? null : ( (CILMethod) cilMethod ).ReturnParameter;
            }
            state.RecordParameter( cilParam, pIdx );
         }
      }

      // Nested types
      IList<Int32> nestedList;
      if ( state.NestedTypes.TryGetValue( typeDefIndex, out nestedList ) )
      {
         foreach ( var nested in nestedList )
         {
            state.CreateLogicalType( typeDef, nested );
         }
      }
   }

   private static CILTypeCode ResolveTypeCodeFromEnumType( TypeSignature typeSig )
   {
      if ( typeSig.TypeSignatureKind == TypeSignatureKind.Simple )
      {
         switch ( ( (SimpleTypeSignature) typeSig ).SimpleType )
         {
            case SignatureElementTypes.Char:
               return CILTypeCode.Char;
            case SignatureElementTypes.I1:
               return CILTypeCode.SByte;
            case SignatureElementTypes.U1:
               return CILTypeCode.Byte;
            case SignatureElementTypes.I2:
               return CILTypeCode.Int16;
            case SignatureElementTypes.U2:
               return CILTypeCode.UInt16;
            case SignatureElementTypes.I4:
               return CILTypeCode.Int32;
            case SignatureElementTypes.U4:
               return CILTypeCode.UInt64;
            case SignatureElementTypes.I8:
               return CILTypeCode.Int64;
            case SignatureElementTypes.U8:
               return CILTypeCode.UInt64;
            default:
               return CILTypeCode.Object;
         }
      }
      else
      {
         return CILTypeCode.Object;
      }
   }

   private static CILTypeCode ResolveTypeCodeTextual( String ns, String tn )
   {
      if ( String.Equals( ns, "System" ) )
      {
         switch ( tn )
         {
            case "Boolean":
               return CILTypeCode.Boolean;
            case "Char":
               return CILTypeCode.Char;
            case "SByte":
               return CILTypeCode.SByte;
            case "Byte":
               return CILTypeCode.Byte;
            case "Int16":
               return CILTypeCode.Int16;
            case "UInt16":
               return CILTypeCode.UInt16;
            case "Int32":
               return CILTypeCode.Int32;
            case "UInt32":
               return CILTypeCode.UInt32;
            case "Int64":
               return CILTypeCode.Int64;
            case "UInt64":
               return CILTypeCode.UInt64;
            case "Single":
               return CILTypeCode.Single;
            case "Double":
               return CILTypeCode.Double;
            case "String":
               return CILTypeCode.String;
            case "Decimal":
               return CILTypeCode.Decimal;
            case "DateTime":
               return CILTypeCode.DateTime;
            case "Void":
               return CILTypeCode.Void;
            case "ValueType":
               return CILTypeCode.Value;
            case "Enum":
               return CILTypeCode.Enum;
            case "TypedReference":
               return CILTypeCode.TypedByRef;
            case "IntPtr":
               return CILTypeCode.IntPtr;
            case "UIntPtr":
               return CILTypeCode.UIntPtr;
            case "Object":
               return CILTypeCode.SystemObject;
            case "Type":
               return CILTypeCode.Type;
            default:
               return CILTypeCode.Object;
         }
      }
      else
      {
         return CILTypeCode.Object;
      }
   }

   private static void PopulateLogicalAssembly( this LogicalCreationState state, CILAssembly assembly )
   {
      // TODO custom attributes
   }

   private static void ProcessLogicalModule( this LogicalCreationState state, CILModule module )
   {
      var md = state.MetaData;

      // TODO Associated MSCorLib

      // Generic arguments
      state.ProcessGenericParameters();

      // Type definitions
      state.ProcessTypeDefs();

      // TODO fieldDefs, methodDefs, rest tables?

   }

   private static void ProcessGenericParameters( this LogicalCreationState state )
   {
      var md = state.MetaData;

      var gParamInfos = new Dictionary<CILElementWithGenericArguments<Object>, List<Int32>>();
      var gParamTable = md.GenericParameterDefinitions.TableContents;
      for ( var i = 0; i < gParamTable.Count; ++i )
      {
         var gParam = gParamTable[i];
         var parent = gParam.Owner;
         CILElementWithGenericArguments<Object> cilParent;
         switch ( parent.Table )
         {
            case Tables.TypeDef:
               cilParent = state.GetTypeDef( parent.Index );
               break;
            case Tables.MethodDef:
               cilParent = state.GetMethodDef( parent.Index ) as CILMethod;
               break;
            default:
               throw new InvalidOperationException( "Unrecognized generic parameter owner: " + parent + "." );
         }
         if ( cilParent != null )
         {
            gParamInfos
               .GetOrAdd_NotThreadSafe( cilParent, p => new List<Int32>() )
               .Add( i );
         }
      }

      foreach ( var gParamInfo in gParamInfos )
      {
         var parent = gParamInfo.Key;
         var gParams = gParamInfo.Value;
         gParams.Sort( ( x, y ) => gParamTable[x].GenericParameterIndex.CompareTo( gParamTable[y].GenericParameterIndex ) );
         var cilGParams = parent.DefineGenericParameters( gParams.Select( g => gParamTable[g].Name ).ToArray() );
         for ( var i = 0; i < cilGParams.Length; ++i )
         {
            var cilGParam = cilGParams[i];
            var gParam = gParams[i];
            state.RecordTypeParameter( cilGParam, gParam );
            cilGParam.Attributes = gParamTable[gParam].Attributes;
         }
      }

      // Generic parameter constraints
      foreach ( var gParamConstraint in md.GenericParameterConstraintDefinitions.TableContents )
      {
         var typeParam = state
            .GetTypeParameter( gParamConstraint.Owner.Index );
         typeParam.AddGenericParameterConstraints( state.ResolveTypeDefOrRefOrSpec( gParamConstraint.Constraint, typeParam.DeclaringType, typeParam.DeclaringMethod ) );
      }
   }

   private static void ProcessTypeDefs( this LogicalCreationState state )
   {
      var tDefs = state.MetaData.TypeDefinitions.TableContents;
      for ( var i = 0; i < tDefs.Count; ++i )
      {
         var tDef = tDefs[i];
         var cilType = state.GetTypeDef( i );

         // Base type
         if ( tDef.BaseType.HasValue )
         {
            cilType.BaseType = state.ResolveTypeDefOrRefOrSpec( tDef.BaseType.Value, cilType, null ) as CILType;
         }
         else
         {
            cilType.BaseType = null;
         }
      }
   }

   private static T GetOrAdd_NotThreadSafe<T>( this List<T> list, Int32 index, Func<Int32, T> factory )
      where T : class
   {
      var retVal = list[index];
      if ( retVal == null )
      {
         retVal = factory( index );
         list[index] = retVal;
      }
      return retVal;
   }
}
