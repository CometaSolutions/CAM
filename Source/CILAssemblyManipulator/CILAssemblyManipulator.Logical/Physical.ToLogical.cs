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
using System.Threading;

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

      private readonly Lazy<CILModule> _associatedMSCorLib;

      internal LogicalCreationState(
         CILModule module,
         CILMetaData md,
         Func<String, CILModule> moduleRefResolver,
         Func<AssemblyReference, CILAssembly> assemblyRefResolver,
         CILModule msCorLibOverride
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

         this._associatedMSCorLib = new Lazy<CILModule>( () => this.ResolveMSCorLibModule( msCorLibOverride ), LazyThreadSafetyMode.None );
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

      public CILModule AssociatedMSCorLibModule
      {
         get
         {
            return this._associatedMSCorLib.Value;
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
               return this.ResolveSimpleType( ( (SimpleTypeSignature) sig ).SimpleType );
            case TypeSignatureKind.SimpleArray:
               return this.ResolveTypeSignature( ( (SimpleArrayTypeSignature) sig ).ArrayType, contextType, contextMethod ).MakeArrayType();
            default:
               throw new InvalidOperationException( "Unrecognized type signature kind: " + sig.TypeSignatureKind + "." );
         }
      }

      internal CILTypeBase ResolveParamSignature( ParameterOrLocalVariableSignature sig, CILType contextType, CILMethodBase contextMethod )
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

      internal CILElementTokenizableInILCode ResolveMemberRef( Int32 index, CILType contextType, CILMethodBase contextMethod, Boolean? shouldBeMethod = null )
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
         CILElementTokenizableInILCode retVal;
         switch ( sig.SignatureKind )
         {
            case SignatureKind.Field:
               var fTypeSig = ( (FieldSignature) sig ).Type;
               retVal = cilDeclType.DeclaredFields.FirstOrDefault( f =>
                  !f.Attributes.IsCompilerControlled()
                  && String.Equals( f.Name, mRef.Name )
                  && this.MatchCILTypeToSignature( f.FieldType, fTypeSig )
                  );
               break;
            case SignatureKind.MethodDefinition:
            case SignatureKind.MethodReference:
               wasMethod = true;
               var mSig = (AbstractMethodSignature) mRef.Signature;
               var isCtor = String.Equals( mRef.Name, Miscellaneous.CLASS_CTOR_NAME ) || String.Equals( mRef.Name, Miscellaneous.INSTANCE_CTOR_NAME );
               var declIsGeneric = cilDeclType.IsGenericType();
               var declTypeToUse = declIsGeneric ? cilDeclType.GenericDefinition : cilDeclType;
               var method = ( isCtor ? (IEnumerable<CILMethodBase>) declTypeToUse.Constructors : declTypeToUse.DeclaredMethods.Where( m => String.Equals( mRef.Name, m.Name ) ) )
                  .FirstOrDefault( m => this.MatchCILMethodParametersToSignature( m, mSig ) );
               if ( method != null && declIsGeneric )
               {
                  method.ChangeDeclaringTypeUT( cilDeclType.GenericArguments.ToArray() );
               }
               retVal = method;
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

      internal CILMethodBase ResolveMethodDefOrMemberRef( TableIndex index, CILType contextType, CILMethodBase contextMethod )
      {
         switch ( index.Table )
         {
            case Tables.MethodDef:
               return this.GetMethodDef( index.Index );
            case Tables.MemberRef:
               return (CILMethodBase) this.ResolveMemberRef( index.Index, contextType, contextMethod, true );
            default:
               throw new InvalidOperationException( "Unsupported method def or member ref index: " + index + "." );
         }
      }

      internal CILType ResolveSimpleType( SignatureElementTypes sigType )
      {
         return this._simpleTypes.GetOrAdd_NotThreadSafe( sigType, st =>
         {
            String typeStr;
            switch ( sigType )
            {
               case SignatureElementTypes.Boolean:
                  typeStr = Consts.BOOLEAN;
                  break;
               case SignatureElementTypes.Char:
                  typeStr = Consts.CHAR;
                  break;
               case SignatureElementTypes.I1:
                  typeStr = Consts.SBYTE;
                  break;
               case SignatureElementTypes.U1:
                  typeStr = Consts.BYTE;
                  break;
               case SignatureElementTypes.I2:
                  typeStr = Consts.INT16;
                  break;
               case SignatureElementTypes.U2:
                  typeStr = Consts.UINT16;
                  break;
               case SignatureElementTypes.I4:
                  typeStr = Consts.INT32;
                  break;
               case SignatureElementTypes.U4:
                  typeStr = Consts.UINT32;
                  break;
               case SignatureElementTypes.I8:
                  typeStr = Consts.INT64;
                  break;
               case SignatureElementTypes.U8:
                  typeStr = Consts.UINT64;
                  break;
               case SignatureElementTypes.I:
                  typeStr = Consts.INT_PTR;
                  break;
               case SignatureElementTypes.U:
                  typeStr = Consts.UINT_PTR;
                  break;
               case SignatureElementTypes.R4:
                  typeStr = Consts.SINGLE;
                  break;
               case SignatureElementTypes.R8:
                  typeStr = Consts.DOUBLE;
                  break;
               case SignatureElementTypes.String:
                  typeStr = Consts.STRING;
                  break;
               case SignatureElementTypes.Void:
                  typeStr = Consts.VOID;
                  break;
               case SignatureElementTypes.Object:
                  typeStr = Consts.OBJECT;
                  break;
               case SignatureElementTypes.TypedByRef:
                  typeStr = Consts.TYPED_BY_REF;
                  break;
               case SignatureElementTypes.Type:
                  typeStr = Consts.TYPE;
                  break;
               default:
                  throw new InvalidOperationException( "Unsupported primitive type: " + sigType + "." );
            }

            return this._associatedMSCorLib.Value.GetTypeByName( typeStr, true );
         } );
      }

      internal CILElementTokenizableInILCode ResolveToken( TableIndex index, CILType contextType, CILMethodBase contextMethod )
      {
         switch ( index.Table )
         {
            case Tables.TypeDef:
               return this.GetTypeDef( index.Index );
            case Tables.TypeRef:
               return this.ResolveTypeRef( index.Index );
            case Tables.TypeSpec:
               return this.ResolveTypeSpec( index.Index, contextType, contextMethod );
            case Tables.MethodDef:
               return this.GetMethodDef( index.Index );
            case Tables.Field:
               return this.GetFieldDef( index.Index );
            case Tables.MemberRef:
               return this.ResolveMemberRef( index.Index, contextType, contextMethod );
            case Tables.MethodSpec:
               var mSpec = this._md.MethodSpecifications.TableContents[index.Index];
               var retVal = this.ResolveMethodDefOrMemberRef( mSpec.Method, contextType, contextMethod ) as CILMethod;
               if ( retVal == null )
               {
                  throw new InvalidOperationException( "Token resolved to constructor with generic arguments (" + index + ")." );
               }
               else
               {
                  retVal = retVal.MakeGenericMethod( mSpec.Signature.GenericArguments.Select( arg => this.ResolveTypeSignature( arg, contextType, contextMethod ) ).ToArray() );
               }
               return retVal;
            case Tables.StandaloneSignature:
               throw new NotImplementedException( "StandaloneSignature as token." );
            default:
               throw new InvalidOperationException( "Unsupported token: " + index + "." );
         }
      }

      internal IEnumerable<Tuple<CILTypeBase, Boolean>> ResolveLocalsSignature( Int32 index, CILType contextType, CILMethodBase contextMethod )
      {
         // Same as type specs: can't cache, but to optimize, should cache if context variables are not used
         var sig = this._md.StandaloneSignatures.TableContents[index].Signature;
         switch ( sig.SignatureKind )
         {
            case SignatureKind.LocalVariables:
               var lSig = (LocalVariablesSignature) sig;
               foreach ( var local in lSig.Locals )
               {
                  yield return Tuple.Create( this.ResolveParamSignature( local, contextType, contextMethod ), local.IsPinned );
               }
               break;
            default:
               throw new InvalidOperationException( "Unsupported local variable signature: " + sig.SignatureKind + "." );
         }
      }

      internal CILAssembly ResolveAssemblyRef( Int32 index )
      {
         return this._assemblyRefs.GetOrAdd_NotThreadSafe( index, i => this._assemblyRefResolver( this._md.AssemblyReferences.TableContents[i] ) );
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
                  retVal = this.ResolveAssemblyRef( resScope.Index )
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
               var sigArgs = clazz.GenericArguments;
               var cilArgs = cilType.GenericArguments;
               var retVal = type != null
                  && !clazz.IsClass == type.IsValueType()
                  && sigArgs.Count == cilArgs.Count;
               if ( retVal )
               {
                  var typeTable = clazz.Type;
                  var cilTypeToUse = cilArgs.Count > 0 ?
                     cilType.GenericDefinition :
                     cilType;
                  switch ( typeTable.Table )
                  {
                     case Tables.TypeDef:
                        retVal = cilTypeToUse.Equals( this.GetTypeDef( typeTable.Index ) );
                        break;
                     case Tables.TypeRef:
                        retVal = this.MatchTypeRefs( cilTypeToUse, typeTable.Index );
                        break;
                     case Tables.TypeSpec:
                        retVal = cilType.GenericDefinition != null && this.MatchCILTypeToSignature( cilType.GenericDefinition, this._md.TypeSpecifications.TableContents[typeTable.Index].Signature );
                        break;
                     default:
                        retVal = false;
                        break;
                  }
                  if ( retVal && sigArgs.Count > 0 )
                  {
                     retVal = cilArgs.Where( ( g, i ) => this.MatchCILTypeToSignature( g, sigArgs[i] ) ).Count() == cilArgs.Count;
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

      private CILModule ResolveMSCorLibModule( CILModule msCorLibOverride )
      {
         // Try find "System.Object"
         var testTypeString = Consts.OBJECT;

         var suitableModule = this.GetSuitableMSCorLibModules( msCorLibOverride ).FirstOrDefault( m => m.GetTypeByName( testTypeString, false ) != null );

         if ( suitableModule == null )
         {
            throw new InvalidOperationException( "Failed to resolve mscorlib module." );
         }

         return suitableModule;
      }

      private IEnumerable<CILModule> GetSuitableMSCorLibModules( CILModule msCorLibOverride )
      {
         if ( msCorLibOverride != null )
         {
            yield return msCorLibOverride;
         }

         yield return this._module;

         var aRefs = this._md.AssemblyReferences.TableContents;
         var aRefList = new List<Int32>( Enumerable.Range( 0, aRefs.Count )
            .Where( aRef => aRefs[aRef] != null )
            );

         aRefList.Sort( ( xIdx, yIdx ) =>
         {
            var x = aRefs[xIdx].AssemblyInformation;
            var y = aRefs[yIdx].AssemblyInformation;

            Int32 retVal;
            if ( x.Equals( y ) )
            {
               retVal = 0;
            }
            else
            {
               retVal = StringComparer.Ordinal.Compare( x.Name, y.Name );
               if ( retVal == 0 )
               {
                  // Compare by-version, newest first
                  retVal = x.VersionMajor.CompareTo( y.VersionMajor );
                  if ( retVal == 0 )
                  {
                     retVal = x.VersionMinor.CompareTo( y.VersionMinor );
                     if ( retVal == 0 )
                     {
                        retVal = x.VersionBuild.CompareTo( y.VersionBuild );
                        if ( retVal == 0 )
                        {
                           retVal = x.VersionRevision.CompareTo( y.VersionRevision );
                        }
                     }
                  }
               }
               else if ( !String.IsNullOrEmpty( x.Name ) )
               {
                  // MSCorLib < System.Runtime < anything else
                  if ( retVal > 0 )
                  {
                     switch ( x.Name )
                     {
                        case Consts.MSCORLIB_NAME:
                        case Consts.NEW_MSCORLIB_NAME:
                           retVal = -1;
                           break;
                     }
                  }
                  else
                  {
                     switch ( y.Name )
                     {
                        case Consts.MSCORLIB_NAME:
                        case Consts.NEW_MSCORLIB_NAME:
                           retVal = 1;
                           break;
                     }
                  }
               }
            }

            return retVal;
         } );

         foreach ( var aRefIdx in aRefList )
         {
            yield return this.ResolveAssemblyRef( aRefIdx ).MainModule;
         }

         if ( msCorLibOverride == null )
         {
            yield return typeof( Object ).Assembly.NewWrapper( this._module.ReflectionContext ).MainModule;
         }
      }
   }

   public static CILAssembly CreateLogicalRepresentation(
      this CILReflectionContext ctx,
      CILMetaData metaData,
      Func<String, CILMetaData> moduleResolver,
      Func<CILMetaData, CILAssemblyName, CILAssembly> customAssemblyResolver = null,
      CILModule msCorLibOverride = null
      )
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
      var assemblyRefResolver = new Func<CILMetaData, CILAssemblyName, CILAssembly>( ( thisMD, aName ) =>
      {
         CILAssembly resolvedAss = null;
         if ( customAssemblyResolver != null )
         {
            resolvedAss = customAssemblyResolver( thisMD, aName );
         }
         if ( resolvedAss == null )
         {
            resolvedAss = ( (CILAssemblyManipulator.Logical.Implementation.CILReflectionContextImpl) ctx ).LaunchAssemblyRefResolveEvent( new AssemblyRefResolveFromLoadedAssemblyEventArgs( aName, ctx ) );
         }

         if ( resolvedAss == null )
         {
            throw new InvalidOperationException( "Failed to resolve " + aName + "." );
         }

         return resolvedAss;
      } );
      var mainModuleState = retVal.AddModule( modList[0].Name ).CreateLogicalCreationState( metaData, moduleRefResolver, assemblyRefResolver, msCorLibOverride );
      allModuleStates[mainModuleState.Module.Name] = mainModuleState;

      foreach ( var module in metaData.FileReferences.TableContents.Where( f => f.Attributes.ContainsMetadata() ) )
      {
         var name = module.Name;
         var moduleMD = moduleResolver( name );
         if ( moduleMD == null )
         {
            throw new InvalidOperationException( "Failed to resolve module \"" + moduleMD + "\"." );
         }

         var cilModule = retVal.AddModule( name );
         allModuleStates[name] = cilModule.CreateLogicalCreationState( moduleMD, moduleRefResolver, assemblyRefResolver, msCorLibOverride );
      }

      // Process rest of the stuff now because of possible cross-module references (we have to have instances of CILModule existing at this point)
      foreach ( var state in allModuleStates.Values )
      {
         state.ProcessLogicalModule( state.Module );
      }

      return retVal;
   }

   private static LogicalCreationState CreateLogicalCreationState(
      this CILModule module,
      CILMetaData metaData,
      Func<String, CILModule> moduleRefResolver,
      Func<CILMetaData, CILAssemblyName, CILAssembly> customAssemblyResolver,
      CILModule msCorLibOverride
      )
   {
      // The module name is set by all places calling this method, so don't set that
      var state = new LogicalCreationState( module, metaData, moduleRefResolver, aRef => customAssemblyResolver( metaData, new CILAssemblyName( aRef.AssemblyInformation, aRef.Attributes.IsFullPublicKey() ) ), msCorLibOverride );

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

      module.AssociatedMSCorLibModule = state.AssociatedMSCorLibModule;

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

   private static void ProcessLogicalModule( this LogicalCreationState state, CILModule module )
   {
      var md = state.MetaData;

      // TODO Associated MSCorLib

      // Generic arguments
      state.ProcessGenericParameters();

      // Type definitions
      state.ProcessTablesAfterStructureIsCreated();

      // TODO ExportedType

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

   private static void ProcessTablesAfterStructureIsCreated( this LogicalCreationState state )
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

      // Create method IL
      for ( var i = 0; i < md.MethodDefinitions.TableContents.Count; ++i )
      {
         state.CreateLogicalIL( i );
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
               var cilMethodDecl = state.ResolveMethodDefOrMemberRef( impl.MethodDeclaration, type, cilMethodBody ) as CILMethod;
               if ( cilMethodDecl != null )
               {
                  cilMethodBody.AddOverriddenMethods( cilMethodDecl );
               }
            }
         }
      }

      // DeclSecurity
      var secDefs = md.SecurityDefinitions.TableContents;
      for ( var i = 0; i < secDefs.Count; ++i )
      {
         var sec = secDefs[i];
         var parent = sec.Parent;
         CILElementWithSecurityInformation cilParent;
         switch ( parent.Table )
         {
            case Tables.TypeDef:
               cilParent = state.GetTypeDef( parent.Index );
               break;
            case Tables.MethodDef:
               cilParent = state.GetMethodDef( parent.Index );
               break;
            case Tables.Assembly:
               cilParent = null;
               break;
            default:
               throw new InvalidOperationException( "Unsupported security information parent: " + parent + "." );
         }

         if ( cilParent != null )
         {
            var secIdx = new TableIndex( Tables.DeclSecurity, i );
            foreach ( var permission in sec.PermissionSets )
            {
               IList<CustomAttributeNamedArgument> namedArgs;
               switch ( permission.SecurityInformationKind )
               {
                  case SecurityInformationKind.Resolved:
                     namedArgs = ( (SecurityInformation) permission ).NamedArguments;
                     break;
                  case SecurityInformationKind.Raw:
                     throw new InvalidOperationException( "Unresolved security information at " + secIdx + "." );
                  default:
                     throw new InvalidOperationException( "Unsupported security information kind: " + permission.SecurityInformationKind + "." );
               }

               var declType = state.ResolveTypeString( permission.SecurityAttributeType );
               var cilPermission = cilParent.AddDeclarativeSecurity( sec.Action, declType );
               cilPermission.NamedArguments.AddRange( namedArgs.Select( arg => state.CreateCANamedArg( declType, secIdx, arg ) ) );
            }
         }
      }

      // Manifest resources
      foreach ( var mResource in md.ManifestResources.TableContents )
      {
         var name = mResource.Name;
         if ( state.Module.ManifestResources.ContainsKey( name ) )
         {
            throw new InvalidOperationException( "Duplicate manifest resource: \"" + name + "\"." );
         }
         else
         {
            var implNullable = mResource.Implementation;
            AbstractLogicalManifestResource resource;
            if ( implNullable.HasValue )
            {
               var impl = implNullable.Value;
               switch ( impl.Table )
               {
                  case Tables.File:
                     var file = md.FileReferences.TableContents[impl.Index];
                     resource = new FileManifestResource( mResource.Attributes, file.Name, file.HashValue.CreateBlockCopy() );
                     break;
                  case Tables.AssemblyRef:
                     resource = new AssemblyManifestResource( mResource.Attributes, state.ResolveAssemblyRef( impl.Index ) );
                     break;
                  default:
                     throw new InvalidOperationException( "Unsupported manifest resource implementation: " + impl + "." );
               }
            }
            else
            {
               resource = new EmbeddedManifestResource( mResource.Attributes, mResource.DataInCurrentFile );
            }

            state.Module.ManifestResources.Add( name, resource );
         }
      }

      // Custom attributes
      var customAttrs = md.CustomAttributeDefinitions.TableContents;
      for ( var i = 0; i < customAttrs.Count; ++i )
      {
         var ca = customAttrs[i];
         var owner = ca.Parent;
         CILCustomAttributeContainer cilOwner;
         switch ( owner.Table )
         {
            case Tables.TypeDef:
               cilOwner = state.GetTypeDef( owner.Index );
               break;
            case Tables.MethodDef:
               cilOwner = state.GetMethodDef( owner.Index );
               break;
            case Tables.Field:
               cilOwner = state.GetFieldDef( owner.Index );
               break;
            case Tables.Parameter:
               cilOwner = state.GetParameterDef( owner.Index );
               break;
            case Tables.Module:
               cilOwner = state.Module;
               break;
            case Tables.Property:
               cilOwner = allProperties[owner.Index];
               break;
            case Tables.Event:
               cilOwner = allEvents[owner.Index];
               break;
            case Tables.Assembly:
               cilOwner = state.Module.Assembly;
               break;
            case Tables.GenericParameter:
               cilOwner = state.GetTypeParameter( owner.Index );
               break;
            case Tables.TypeRef:
            case Tables.InterfaceImpl:
            case Tables.MemberRef:
            case Tables.DeclSecurity:
            case Tables.StandaloneSignature:
            case Tables.ModuleRef:
            case Tables.TypeSpec:
            case Tables.AssemblyRef:
            case Tables.File:
            case Tables.ExportedType:
            case Tables.ManifestResource:
            case Tables.GenericParameterConstraint:
            case Tables.MethodSpec:
               // CAM.Logical does not expose adding custom attributes to these
               cilOwner = null;
               break;
            default:
               throw new InvalidOperationException( "Unrecognized custom attribute parent: " + owner + "." );
         }

         if ( cilOwner != null )
         {
            state.AddCustomAttributeTo( cilOwner, i );
         }
      }
   }

   private static void AddCustomAttributeTo( this LogicalCreationState state, CILCustomAttributeContainer owner, Int32 caIdx )
   {
      var ca = state.MetaData.CustomAttributeDefinitions.TableContents[caIdx];
      var ctor = state.ResolveMethodDefOrMemberRef( ca.Parent, null, null ) as CILConstructor;
      if ( ctor != null )
      {
         var sig = ca.Signature;
         if ( sig.CustomAttributeSignatureKind == CustomAttributeSignatureKind.Resolved )
         {
            var caSig = (CustomAttributeSignature) sig;
            var declType = ctor.DeclaringType;
            var caTableIdx = new TableIndex( Tables.CustomAttribute, caIdx );
            owner.AddCustomAttribute( ctor, caSig.TypedArguments.Select( arg => state.CreateCATypedArg( arg ) ), caSig.NamedArguments.Select( arg => state.CreateCANamedArg( declType, caTableIdx, arg ) ) );
         }
         else
         {
            throw new InvalidOperationException( "Custom attribute signature at zero-based index " + caIdx + " must be resolved." );
         }
      }
   }

   private static CILCustomAttributeTypedArgument CreateCATypedArg( this LogicalCreationState state, CustomAttributeTypedArgument sig )
   {
      return CILCustomAttributeFactory.NewTypedArgument( state.ResolveCAType( sig.Type ), sig.Value );
   }

   private static CILCustomAttributeNamedArgument CreateCANamedArg( this LogicalCreationState state, CILType declType, TableIndex caIdx, CustomAttributeNamedArgument sig )
   {
      var namedElement = sig.IsField ?
         (CILElementForNamedCustomAttribute) declType.GetBaseTypeChain().SelectMany( t => t.DeclaredFields ).FirstOrDefault( f => String.Equals( f.Name, sig.Name ) ) :
         declType.GetBaseTypeChain().SelectMany( t => t.DeclaredProperties ).FirstOrDefault( p => String.Equals( p.Name, sig.Name ) );
      if ( namedElement == null )
      {
         throw new InvalidOperationException( "Failed to resolve " + ( sig.IsField ? "field" : "property" ) + " named \"" + sig.Name + "\" at " + caIdx + "." );
      }

      return CILCustomAttributeFactory.NewNamedArgument( namedElement, state.CreateCATypedArg( sig.Value ) );
   }

   private static CILType ResolveCAType( this LogicalCreationState state, CustomAttributeArgumentType sigType )
   {
      switch ( sigType.ArgumentTypeKind )
      {
         case CustomAttributeArgumentTypeKind.Simple:
            return state.ResolveSimpleType( ( (CustomAttributeArgumentTypeSimple) sigType ).SimpleType );
         case CustomAttributeArgumentTypeKind.Array:
            return state.ResolveCAType( ( (CustomAttributeArgumentTypeArray) sigType ).ArrayType ).MakeArrayType();
         case CustomAttributeArgumentTypeKind.TypeString:
            return state.ResolveTypeString( ( (CustomAttributeArgumentTypeEnum) sigType ).TypeString );
         default:
            throw new InvalidOperationException( "Unsupported custom attribute argument type: " + sigType.ArgumentTypeKind + "." );
      }
   }

   private static void CreateLogicalIL( this LogicalCreationState state, Int32 mDefIdx )
   {
      var physicalIL = state.MetaData.MethodDefinitions.TableContents[mDefIdx].IL;
      if ( physicalIL != null )
      {
         var method = state.GetMethodDef( mDefIdx );
         var contextType = method.DeclaringType;
         var retVal = method.MethodIL;
         var physOpCodes = physicalIL.OpCodes;
         var logicalByteOffsets = new Dictionary<Int32, Int32>( physOpCodes.Count );
         var labelByteOffsets = new Dictionary<ILLabel, Int32>();
         var curByteOffset = 0;
         for ( var codeIdx = 0; codeIdx < physOpCodes.Count; ++codeIdx )
         {
            var code = physOpCodes[codeIdx];
            logicalByteOffsets.Add( curByteOffset, codeIdx );
            curByteOffset += code.GetTotalByteCount();
            LogicalOpCodeInfo logicalCode;
            switch ( code.InfoKind )
            {
               case OpCodeOperandKind.OperandInteger:
                  var physIntCode = (OpCodeInfoWithInt32) code;
                  switch ( code.OpCode.OperandType )
                  {
                     case OperandType.InlineBrTarget:
                     case OperandType.ShortInlineBrTarget:
                        var label = retVal.DefineLabel();
                        logicalCode = new LogicalOpCodeInfoForFixedBranchOrLeave( code.OpCode, label );
                        labelByteOffsets.Add( label, curByteOffset + physIntCode.Operand );
                        break;
                     case OperandType.InlineI:
                     case OperandType.InlineVar:
                        logicalCode = new LogicalOpCodeInfoWithFixedSizeOperandInt32( code.OpCode, physIntCode.Operand );
                        break;
                     case OperandType.ShortInlineI:
                     case OperandType.ShortInlineVar:
                        logicalCode = new LogicalOpCodeInfoWithFixedSizeOperandUInt16( code.OpCode, (Int16) physIntCode.Operand );
                        break;
                     default:
                        throw new InvalidOperationException( "Unsupported op code for physical op code with integer: " + code.OpCode + "." );
                  }
                  break;
               case OpCodeOperandKind.OperandInteger64:
                  logicalCode = new LogicalOpCodeInfoWithFixedSizeOperandInt64( code.OpCode, ( (OpCodeInfoWithInt64) code ).Operand );
                  break;
               case OpCodeOperandKind.OperandNone:
                  logicalCode = LogicalOpCodeInfoWithNoOperand.GetInstanceFor( code.OpCode );
                  break;
               case OpCodeOperandKind.OperandR4:
                  logicalCode = new LogicalOpCodeInfoWithFixedSizeOperandSingle( code.OpCode, ( (OpCodeInfoWithSingle) code ).Operand );
                  break;
               case OpCodeOperandKind.OperandR8:
                  logicalCode = new LogicalOpCodeInfoWithFixedSizeOperandDouble( code.OpCode, ( (OpCodeInfoWithDouble) code ).Operand );
                  break;
               case OpCodeOperandKind.OperandString:
                  logicalCode = new LogicalOpCodeInfoWithFixedSizeOperandString( code.OpCode, ( (OpCodeInfoWithString) code ).Operand );
                  break;
               case OpCodeOperandKind.OperandSwitch:
                  var physSwitch = (OpCodeInfoWithSwitch) code;
                  var labels = retVal.DefineLabels( physSwitch.Offsets.Count );
                  logicalCode = new LogicalOpCodeInfoForSwitch( labels );
                  for ( var i = 0; i < labels.Length; ++i )
                  {
                     labelByteOffsets.Add( labels[i], curByteOffset + physSwitch.Offsets[i] );
                  }
                  break;
               case OpCodeOperandKind.OperandToken:
                  var token = ( (OpCodeInfoWithToken) code ).Operand;
                  var resolved = state.ResolveToken( token, contextType, method );
                  switch ( resolved.ElementTypeKind )
                  {
                     case CILElementWithinILCode.Field:
                        logicalCode = new LogicalOpCodeInfoWithFieldToken( code.OpCode, (CILField) resolved, token.Table == Tables.Field );
                        break;
                     case CILElementWithinILCode.Method:
                        var useGDef = token.Table == Tables.MethodDef || ( token.Table == Tables.MemberRef && state.MetaData.MemberReferences.TableContents[token.Index].DeclaringType.Table == Tables.TypeDef );
                        if ( resolved is CILMethod )
                        {
                           logicalCode = new LogicalOpCodeInfoWithMethodToken( code.OpCode, (CILMethod) resolved, useGDef );
                        }
                        else
                        {
                           logicalCode = new LogicalOpCodeInfoWithCtorToken( code.OpCode, (CILConstructor) resolved, useGDef );
                        }
                        break;
                     case CILElementWithinILCode.Type:
                        logicalCode = new LogicalOpCodeInfoWithTypeToken( code.OpCode, (CILTypeBase) resolved, token.Table == Tables.TypeDef );
                        break;
                     default:
                        throw new InvalidOperationException( "Unrecognized tokenizable element kind: " + resolved.ElementTypeKind + "." );
                  }
                  break;
               default:
                  throw new InvalidOperationException( "Unsupported physical op code kind: " + code.InfoKind + "." );
            }

            retVal.Add( logicalCode );
         }

         // Mark labels
         var labelLogicalOffsets = new Dictionary<Int32, ILLabel>( labelByteOffsets.Count );
         foreach ( var kvp in labelByteOffsets )
         {
            var codeOffset = MapByteOffsetToCodeOffset( logicalByteOffsets, kvp.Value );
            retVal.MarkLabel( kvp.Key, codeOffset );
            labelLogicalOffsets.Add( codeOffset, kvp.Key );
         }

         // Create locals
         var localsIdx = physicalIL.LocalsSignatureIndex;
         if ( localsIdx.HasValue )
         {
            foreach ( var local in state.ResolveLocalsSignature( physicalIL.LocalsSignatureIndex.Value.Index, contextType, method ) )
            {
               retVal.DeclareLocal( local.Item1, local.Item2 );
            }
         }

         retVal.InitLocals = physicalIL.InitLocals;
         // Create exception blocks
         foreach ( var excBlock in physicalIL.ExceptionBlocks )
         {
            var tryOffset = MapByteOffsetToCodeOffset( logicalByteOffsets, excBlock.TryOffset );
            var handlerOffset = MapByteOffsetToCodeOffset( logicalByteOffsets, excBlock.HandlerOffset );
            retVal.AddExceptionBlockInfo( new ExceptionBlockInfo(
               tryOffset,
               MapByteOffsetToCodeOffset( logicalByteOffsets, excBlock.TryOffset + excBlock.TryLength ) - tryOffset,
               handlerOffset,
               MapByteOffsetToCodeOffset( logicalByteOffsets, excBlock.HandlerOffset + excBlock.HandlerLength ) - handlerOffset,
               excBlock.ExceptionType.HasValue ? state.ResolveTypeDefOrRefOrSpec( excBlock.ExceptionType.Value, contextType, method ) : null,
               MapByteOffsetToCodeOffset( logicalByteOffsets, excBlock.FilterOffset ),
               excBlock.BlockType
               ) );
         }
      }
   }

   private static Int32 MapByteOffsetToCodeOffset( IDictionary<Int32, Int32> logicalByteOffsets, Int32 byteOffset )
   {
      Int32 codeOffset;
      if ( !logicalByteOffsets.TryGetValue( byteOffset, out codeOffset ) )
      {
         //throw new InvalidOperationException( "Branch offset does not point to the start of the op code." );
         // Instead of throwing, just set to -1. Because stuff in reference assemblies folder tends to have branches which go outside the actual code...
         codeOffset = -1;
      }
      return codeOffset;
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
