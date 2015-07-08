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

         this._typeDefs = PopulateWithNulls<CILType>( tDefCount );
         this._fieldDefs = PopulateWithNulls<CILField>( md.FieldDefinitions.RowCount );
         this._methodDefs = PopulateWithNulls<CILMethodBase>( md.MethodDefinitions.RowCount );
         this._parameterDefs = PopulateWithNulls<CILParameter>( md.ParameterDefinitions.RowCount );
         this._typeParameters = PopulateWithNulls<CILTypeParameter>( md.GenericParameterDefinitions.RowCount );
         this._typeRefs = PopulateWithNulls<CILType>( md.TypeReferences.RowCount );
         this._moduleRefs = PopulateWithNulls<CILModule>( md.ModuleReferences.RowCount );
         this._assemblyRefs = PopulateWithNulls<CILAssembly>( md.AssemblyReferences.RowCount );

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

      internal CILParameter GetParameterDef( Int32 parameterIndex )
      {
         return this._parameterDefs[parameterIndex];
      }

      internal CILField GetFieldDef( Int32 fieldDefIndex )
      {
         return this._fieldDefs[fieldDefIndex];
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
               return this.ResolveTypeSpec( index.Index, contextType, contextMethod );
            default:
               throw new InvalidOperationException( "Unexpected TypeDef/Ref/Spec: " + index + "." );
         }
      }

      internal CILTypeBase ResolveTypeSignature( TypeSignature sig, CILType contextType, CILMethodBase contextMethod )
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
               if ( gSig.IsTypeParameter )
               {
                  if ( contextType == null )
                  {
                     throw new InvalidOperationException( "Type generic parameter signature present, but no type is currently in context." );
                  }
                  else
                  {
                     return contextType.GenericArguments[gSig.GenericParameterIndex];
                  }
               }
               else
               {
                  if ( contextMethod == null )
                  {
                     throw new InvalidOperationException( "Method generic parameter signature present, but no method is currently in context." );
                  }
                  else
                  {
                     var m = contextMethod as CILMethod;
                     if ( m == null )
                     {
                        throw new InvalidOperationException( "Method generic parameter signature present, but method does not have generic parameters." );
                     }
                     else
                     {
                        return m.GenericArguments[gSig.GenericParameterIndex];
                     }
                  }
               }
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

      internal CILTypeBase ResolveParamSignature( ParameterSignature sig, CILType contextType, CILMethodBase contextMethod )
      {
         var retVal = this.ResolveTypeSignature( sig.Type, contextType, contextMethod );
         if ( sig.IsByRef )
         {
            retVal = retVal.MakeByRefType();
         }

         return retVal;
      }

      internal CILType ResolveTypeString( String typeString )
      {
         CILType retVal;
         if ( String.IsNullOrEmpty( typeString ) )
         {
            retVal = null;
         }
         else
         {
            throw new NotImplementedException();
         }

         return retVal;
      }

      internal void AddCustomModifiers( CILElementWithCustomModifiers element, List<CustomModifierSignature> mods, CILType contextType, CILMethodBase contextMethod )
      {
         foreach ( var mod in mods )
         {
            element.AddCustomModifier( this.ResolveTypeDefOrRefOrSpec( mod.CustomModifierType, contextType, contextMethod ) as CILType, mod.IsOptional );
         }
      }

      internal Object ResolveMemberRef( Int32 index, CILType contextType, CILMethodBase contextMethod, Boolean? shouldBeMethod = null )
      {
         var mRef = this._md.MemberReferences.TableContents[index];
         var declType = mRef.DeclaringType;
         CILType cilDeclType;
         switch ( declType.Table )
         {
            case Tables.TypeDef:
               cilDeclType = this.GetTypeDef( declType.Index );
               break;
            case Tables.TypeRef:
               cilDeclType = this.ResolveTypeRef( declType.Index );
               break;
            case Tables.TypeSpec:
               cilDeclType = this.ResolveTypeSpec( declType.Index, contextType, contextMethod ) as CILType;
               break;
            case Tables.MethodDef:
               throw new NotImplementedException( "References to global methods/fields in other modules." );
            case Tables.ModuleRef:
               cilDeclType = this.ResolveModuleRef( declType.Index ).ModuleInitializer;
               break;
            default:
               throw new InvalidOperationException( "Unsupported member ref declaring type: " + declType + "." );
         }

         var sig = mRef.Signature;
         var wasMethod = false;
         Object retVal;
         switch ( sig.SignatureKind )
         {
            case SignatureKind.Field:
               retVal = cilDeclType.DeclaredFields.FirstOrDefault( f => !f.Attributes.IsCompilerControlled() && String.Equals( f.Name, mRef.Name ) );
               break;
            case SignatureKind.MethodDefinition:
            case SignatureKind.MethodReference:
               wasMethod = true;
               var mSig = (AbstractMethodSignature) mRef.Signature;
               var isCtor = String.Equals( mRef.Name, Miscellaneous.CLASS_CTOR_NAME ) || String.Equals( mRef.Name, Miscellaneous.INSTANCE_CTOR_NAME );
               retVal = ( isCtor ? (IEnumerable<CILMethodBase>) cilDeclType.Constructors : cilDeclType.DeclaredMethods.Where( m => String.Equals( mRef.Name, m.Name ) ) )
                  .FirstOrDefault( m => this.MatchCILMethodParametersToSignature( m, mSig ) );
               break;
            default:
               throw new InvalidOperationException( "Unsupported member ref signature: " + sig.SignatureKind + "." );
         }

         if ( retVal == null )
         {
            throw new InvalidOperationException( "Failed to resolve member ref at zero-based index " + index + "." );
         }
         else if ( shouldBeMethod.IsTrue() && !wasMethod )
         {
            throw new InvalidOperationException( "Expected method from member reference at zero-based index " + index + "." );
         }

         return retVal;
      }

      private CILTypeBase ResolveTypeSpec( Int32 tSpecIdx, CILType contextType, CILMethodBase contextMethod )
      {
         // TypeSpecs can't be cached, as they might resolve to different instances of logical types because of context type/method.
         // TODO : resolving type signature should indicate whether it had contextual elements (any GenericParameterTypeSignature's).
         // If not, then this type spec is cacheable.
         return this.ResolveTypeSignature( this._md.TypeSpecifications.TableContents[tSpecIdx].Signature, contextType, contextMethod );
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
                  // TODO type forwarding!!
                  retVal = this._assemblyRefs
                     .GetOrAdd_NotThreadSafe( resScope.Index, i => this._assemblyRefResolver( this._md.AssemblyReferences.TableContents[i] ) )
                     .MainModule // TODO probably should seek whole ExportedTypes table!
                     .GetTypeByName( LogicalUtils.CombineTypeAndNamespace( tRef.Name, tRef.Namespace ) );
                  break;
               case Tables.Module:
                  this._topLevelTypesByName.TryGetValue( Tuple.Create( tRef.Namespace, tRef.Name ), out retVal );
                  break;
               case Tables.ModuleRef:
                  retVal = this.ResolveModuleRef( resScope.Index ).GetTypeByName( LogicalUtils.CombineTypeAndNamespace( tRef.Name, tRef.Namespace ) );
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

      private CILModule ResolveModuleRef( Int32 idx )
      {
         return this._moduleRefs.GetOrAdd_NotThreadSafe( idx, i => this._moduleRefResolver( this._md.ModuleReferences.TableContents[i].ModuleName ) );
      }

      private Boolean MatchCILMethodParametersToSignature( CILMethodBase method, AbstractMethodSignature sig )
      {
         // Name matching should've been already done by code calling this method
         var m = method as CILMethod;
         return this.MatchCILMethodParametersToSignature( method.Parameters.Count, method.Parameters, m == null ? null : m.ReturnParameter, sig );
      }

      private Boolean MatchCILMethodParametersToSignature( CILMethodSignature method, AbstractMethodSignature sig )
      {
         return this.MatchCILMethodParametersToSignature( method.Parameters.Count, method.Parameters, method.ReturnParameter, sig );
      }

      private Boolean MatchCILMethodParametersToSignature( Int32 cilParamCount, IEnumerable<CILParameterBase<Object>> cilParams, CILParameterBase<Object> retParam, AbstractMethodSignature sig )
      {
         // Name matching should've been already done by code calling this method
         var sigParams = sig.Parameters;
         var retVal = cilParamCount == sigParams.Count
            && cilParams.Where( ( p, pIdx ) => this.MatchCILParameterToSignature( p, sigParams[pIdx] ) ).Count() == cilParamCount;

         if ( retVal && retParam != null )
         {
            retVal = this.MatchCILParameterToSignature( retParam, sig.ReturnType );
         }

         return retVal;
      }

      private Boolean MatchCILParameterToSignature( CILParameterBase<Object> param, ParameterSignature sig )
      {
         var pType = param.ParameterType;
         return sig.IsByRef == pType.IsByRef()
            && this.MatchCILTypeToSignature( sig.IsByRef ? pType.GetElementType() : pType, sig.Type );
      }

      private Boolean MatchCILTypeToSignature( CILTypeBase type, TypeSignature sig )
      {
         CILType cilType;
         switch ( sig.TypeSignatureKind )
         {
            case TypeSignatureKind.ClassOrValue:
               cilType = type as CILType;
               var clazz = (ClassOrValueTypeSignature) sig;
               var retVal = type != null
                  && !clazz.IsClass == type.IsValueType();
               if ( retVal )
               {
                  var typeTable = clazz.Type;
                  switch ( typeTable.Table )
                  {
                     case Tables.TypeDef:
                        retVal = cilType.Equals( this.GetTypeDef( typeTable.Index ) );
                        break;
                     case Tables.TypeRef:
                        retVal = this.MatchTypeRefs( cilType, typeTable.Index );
                        break;
                     case Tables.TypeSpec:
                        retVal = cilType.GenericDefinition != null && this.MatchCILTypeToSignature( cilType.GenericDefinition, this._md.TypeSpecifications.TableContents[typeTable.Index].Signature );
                        break;
                     default:
                        retVal = false;
                        break;
                  }
                  var sigArgs = clazz.GenericArguments;
                  if ( retVal && sigArgs.Count > 0 )
                  {
                     var cilArgs = cilType.GenericArguments;
                     retVal = cilArgs.Count == sigArgs.Count
                        && cilArgs.Where( ( g, i ) => this.MatchCILTypeToSignature( g, sigArgs[i] ) ).Count() == cilArgs.Count;
                  }
               }
               return retVal;
            case TypeSignatureKind.ComplexArray:
               cilType = type as CILType;
               var sigType = (ComplexArrayTypeSignature) sig;
               return cilType != null
                  && this.MatchComplexArrayInfo( cilType.ArrayInformation, sigType )
                  && this.MatchCILTypeToSignature( cilType.ElementType, sigType.ArrayType );
            case TypeSignatureKind.FunctionPointer:
               return type.TypeKind == TypeKind.MethodSignature
                  && this.MatchCILMethodParametersToSignature( (CILMethodSignature) type, ( (FunctionPointerTypeSignature) sig ).MethodSignature );
            case TypeSignatureKind.GenericParameter:
               var gParam = type as CILTypeParameter;
               var gSig = (GenericParameterTypeSignature) sig;
               return gParam != null
                  && gParam.GenericParameterPosition == gSig.GenericParameterIndex
                  && ( gParam.DeclaringMethod == null ) == ( gSig.IsTypeParameter );
            case TypeSignatureKind.Pointer:
               return type.IsPointerType()
                  && this.MatchCILTypeToSignature( type.GetElementType(), ( (PointerTypeSignature) sig ).PointerType );
            case TypeSignatureKind.Simple:
               var tc = type.GetTypeCode( CILTypeCode.Empty );
               switch ( ( (SimpleTypeSignature) sig ).SimpleType )
               {
                  case SignatureElementTypes.Boolean:
                     return tc == CILTypeCode.Boolean;
                  case SignatureElementTypes.Char:
                     return tc == CILTypeCode.Char;
                  case SignatureElementTypes.I1:
                     return tc == CILTypeCode.SByte;
                  case SignatureElementTypes.U1:
                     return tc == CILTypeCode.Byte;
                  case SignatureElementTypes.I2:
                     return tc == CILTypeCode.Int16;
                  case SignatureElementTypes.U2:
                     return tc == CILTypeCode.UInt16;
                  case SignatureElementTypes.I4:
                     return tc == CILTypeCode.Int32;
                  case SignatureElementTypes.U4:
                     return tc == CILTypeCode.UInt32;
                  case SignatureElementTypes.I8:
                     return tc == CILTypeCode.Int64;
                  case SignatureElementTypes.U8:
                     return tc == CILTypeCode.UInt64;
                  case SignatureElementTypes.R4:
                     return tc == CILTypeCode.Single;
                  case SignatureElementTypes.R8:
                     return tc == CILTypeCode.Double;
                  case SignatureElementTypes.I:
                     return tc == CILTypeCode.IntPtr;
                  case SignatureElementTypes.U:
                     return tc == CILTypeCode.UIntPtr;
                  case SignatureElementTypes.Object:
                     return tc == CILTypeCode.SystemObject;
                  case SignatureElementTypes.String:
                     return tc == CILTypeCode.String;
                  case SignatureElementTypes.Void:
                     return tc == CILTypeCode.Void;
                  case SignatureElementTypes.TypedByRef:
                     return tc == CILTypeCode.TypedByRef;
                  default:
                     throw new InvalidOperationException( "Unrecognized simple type signature: " + ( (SimpleTypeSignature) sig ).SimpleType + "." );
               }
            case TypeSignatureKind.SimpleArray:
               return ( type as CILType ).IsVectorArray()
                  && this.MatchCILTypeToSignature( type.GetElementType(), ( (SimpleArrayTypeSignature) sig ).ArrayType );
            default:
               throw new InvalidOperationException( "Unrecognized type signature kind: " + sig.TypeSignatureKind + "." );
         }
      }

      private Boolean MatchTypeRefs( CILType typeRefFromOtherModule, Int32 thisTypeRefIndex )
      {
         // TODO: If no match, and this type ref represents retargetable type ref, perform textual match
         // Also! need to look into type forwarding infos as well.
         return typeRefFromOtherModule.Equals( this.ResolveTypeRef( thisTypeRefIndex ) );
      }

      private Boolean MatchComplexArrayInfo( GeneralArrayInfo cilInfo, ComplexArrayTypeSignature sig )
      {
         return cilInfo != null
            && cilInfo.Rank == sig.Rank
            && MatchArrayQueryAndList( cilInfo.LowerBounds, sig.LowerBounds )
            && MatchArrayQueryAndList( cilInfo.Sizes, sig.Sizes );
      }

      private static Boolean MatchArrayQueryAndList( CollectionsWithRoles.API.ArrayQuery<Int32> array, List<Int32> list )
      {
         return array.Count == list.Count
            && array.Where( ( i, idx ) => idx == list[i] ).Count() == array.Count;
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
         state.ProcessNewlyCreatedType( 0, module.ModuleInitializer );

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
         state.ProcessNewlyCreatedType( typeDefIndex, retVal );
      }
   }

   private static void ProcessNewlyCreatedType( this LogicalCreationState state, Int32 typeDefIndex, CILType typeDef )
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
         cilMethod.ImplementationAttributes = method.ImplementationAttributes;

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
      var md = state.MetaData;
      // TypeDef table
      var tDefs = md.TypeDefinitions.TableContents;
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

         // Field types and custom modifiers
         foreach ( var fIdx in md.GetTypeFieldIndices( i ) )
         {
            var fSig = md.FieldDefinitions.TableContents[fIdx].Signature;
            var field = state.GetFieldDef( fIdx );
            field.FieldType = state.ResolveTypeSignature( fSig.Type, cilType, null );
            state.AddCustomModifiers( field, fSig.CustomModifiers, cilType, null );
         }

         // Method parameter types and custom modifiers
         foreach ( var mIdx in md.GetTypeMethodIndices( i ) )
         {
            var mSig = md.MethodDefinitions.TableContents[mIdx].Signature;
            var methodBase = state.GetMethodDef( mIdx );
            for ( var pIdx = 0; pIdx < mSig.Parameters.Count; ++pIdx )
            {
               var pSig = mSig.Parameters[pIdx];
               var cilParam = methodBase.Parameters[pIdx];
               cilParam.ParameterType = state.ResolveParamSignature( pSig, cilType, methodBase );
               state.AddCustomModifiers( cilParam, pSig.CustomModifiers, cilType, methodBase );
            }
            var method = methodBase as CILMethod;
            if ( method != null )
            {
               var pSig = mSig.ReturnType;
               var cilParam = method.ReturnParameter;
               cilParam.ParameterType = state.ResolveParamSignature( pSig, cilType, methodBase );
               state.AddCustomModifiers( cilParam, pSig.CustomModifiers, cilType, methodBase );
            }
         }
      }

      // Class layout table
      foreach ( var layout in md.ClassLayouts.TableContents )
      {
         state.GetTypeDef( layout.Parent.Index ).Layout = new LogicalClassLayout( layout.ClassSize, layout.PackingSize );
      }

      // Properties
      var pMaps = md.PropertyMaps.TableContents;
      var allProperties = PopulateWithNulls<CILProperty>( md.PropertyDefinitions.RowCount );
      for ( var i = 0; i < pMaps.Count; ++i )
      {
         var type = state.GetTypeDef( pMaps[i].Parent.Index );
         foreach ( var propIdx in md.GetTypePropertyIndices( i ) )
         {
            var prop = md.PropertyDefinitions.TableContents[propIdx];
            var cilProp = type.AddProperty( prop.Name, prop.Attributes );
            state.AddCustomModifiers( cilProp, prop.Signature.CustomModifiers, type, null );
            allProperties[propIdx] = cilProp;
         }
      }

      // Events
      var eMaps = md.EventMaps.TableContents;
      var allEvents = PopulateWithNulls<CILEvent>( md.EventDefinitions.RowCount );
      for ( var i = 0; i < eMaps.Count; ++i )
      {
         var type = state.GetTypeDef( eMaps[i].Parent.Index );
         foreach ( var evtIdx in md.GetTypeEventIndices( i ) )
         {
            var evt = md.EventDefinitions.TableContents[evtIdx];
            var cilEvt = type.AddEvent( evt.Name, evt.Attributes, state.ResolveTypeDefOrRefOrSpec( evt.EventType, type, null ) );
            allEvents[evtIdx] = cilEvt;
         }
      }

      // Method semantics
      foreach ( var semantics in md.MethodSemantics.TableContents )
      {
         var method = (CILMethod) state.GetMethodDef( semantics.Method.Index );
         var asso = semantics.Associaton;
         switch ( semantics.Attributes )
         {
            case MethodSemanticsAttributes.Getter:
               allProperties[asso.Index].GetMethod = method;
               break;
            case MethodSemanticsAttributes.Setter:
               allProperties[asso.Index].SetMethod = method;
               break;
            case MethodSemanticsAttributes.AddOn:
               allEvents[asso.Index].AddMethod = method;
               break;
            case MethodSemanticsAttributes.RemoveOn:
               allEvents[asso.Index].RemoveMethod = method;
               break;
            case MethodSemanticsAttributes.Fire:
               allEvents[asso.Index].RaiseMethod = method;
               break;
            case MethodSemanticsAttributes.Other:
               allEvents[asso.Index].AddOtherMethods( method );
               break;
            default:
               throw new InvalidOperationException( "Unrecognized semantics attributes: " + semantics.Attributes + "." );
         }
      }

      // Constants
      foreach ( var constant in md.ConstantDefinitions.TableContents )
      {
         CILElementWithConstant element;
         var parent = constant.Parent;
         switch ( parent.Table )
         {
            case Tables.Parameter:
               element = state.GetParameterDef( parent.Index );
               break;
            case Tables.Field:
               element = state.GetFieldDef( parent.Index );
               break;
            case Tables.Property:
               element = allProperties[parent.Index];
               break;
            default:
               throw new InvalidOperationException( "Unrecognized constant parent: " + parent + "." );
         }

         if ( element != null )
         {
            element.ConstantValue = constant.Value;
         }
      }

      // Field layouts
      foreach ( var layout in md.FieldLayouts.TableContents )
      {
         state.GetFieldDef( layout.Field.Index ).FieldOffset = layout.Offset;
      }

      // Field marshals
      foreach ( var marshal in md.FieldMarshals.TableContents )
      {
         CILElementWithMarshalingInfo element;
         var parent = marshal.Parent;
         switch ( parent.Table )
         {
            case Tables.Field:
               element = state.GetFieldDef( parent.Index );
               break;
            case Tables.Parameter:
               element = state.GetParameterDef( parent.Index );
               break;
            default:
               throw new InvalidOperationException( "Unrecognized field marshal parent: " + parent + "." );
         }

         if ( element != null )
         {
            var marshalInfo = marshal.NativeType;
            element.MarshalingInformation = new LogicalMarshalingInfo(
               marshalInfo.Value,
               marshalInfo.SafeArrayType,
               state.ResolveTypeString( marshalInfo.SafeArrayUserDefinedType ),
               marshalInfo.IIDParameterIndex,
               marshalInfo.ArrayType,
               marshalInfo.SizeParameterIndex,
               marshalInfo.ConstSize,
               marshalInfo.MarshalType,
               s => state.ResolveTypeString( s ),
               marshalInfo.MarshalCookie
               );
         }
      }

      // Field RVAs
      foreach ( var rva in md.FieldRVAs.TableContents )
      {
         state.GetFieldDef( rva.Field.Index ).InitialValue = rva.Data;
      }

      // Implementation Maps
      foreach ( var impl in md.MethodImplementationMaps.TableContents )
      {
         var member = impl.MemberForwarded;
         CILMethod method;
         switch ( member.Table )
         {
            case Tables.MethodDef:
               method = state.GetMethodDef( member.Index ) as CILMethod;
               break;
            case Tables.Field:
               // Not implemented
               method = null;
               break;
            default:
               throw new InvalidOperationException( "Unrecognized implementation map member: " + member + "." );
         }

         if ( method != null )
         {
            method.PlatformInvokeAttributes = impl.Attributes;
            method.PlatformInvokeName = impl.ImportName;
            method.PlatformInvokeModuleName = md.ModuleReferences.TableContents[impl.ImportScope.Index].ModuleName;
         }
      }

      // InterfaceImpl
      foreach ( var impl in md.InterfaceImplementations.TableContents )
      {
         var type = state.GetTypeDef( impl.Class.Index );
         var iFace = state.ResolveTypeDefOrRefOrSpec( impl.Interface, type, null ) as CILType;
         if ( iFace != null )
         {
            type.AddDeclaredInterfaces( iFace );
         }
      }

      // MethodImpl
      foreach ( var impl in md.MethodImplementations.TableContents )
      {
         var methodBody = impl.MethodBody;
         if ( methodBody.Table == Tables.MethodDef )
         {
            // Overriding base type methods not supported (yet)
            var type = state.GetTypeDef( impl.Class.Index );
            var cilMethodBody = state.GetMethodDef( methodBody.Index ) as CILMethod;
            if ( cilMethodBody != null )
            {
               var methodDecl = impl.MethodDeclaration;
               CILMethod cilMethodDecl;
               switch ( methodDecl.Table )
               {
                  case Tables.MethodDef:
                     cilMethodDecl = state.GetMethodDef( methodDecl.Index ) as CILMethod;
                     break;
                  case Tables.MemberRef:
                     cilMethodDecl = state.ResolveMemberRef( methodDecl.Index, type, cilMethodBody, true ) as CILMethod;
                     break;
                  default:
                     throw new InvalidOperationException( "Unsupported method implementation declaration: " + methodDecl + "." );
               }

               cilMethodBody.AddOverriddenMethods( cilMethodDecl );
            }
         }
      }

      // TODO method IL.
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

   public static List<T> PopulateWithNulls<T>( Int32 count )
      where T : class
   {
      var retVal = new List<T>( count );
      retVal.AddRange( Enumerable.Repeat<T>( null, count ) );
      return retVal;
   }
}
