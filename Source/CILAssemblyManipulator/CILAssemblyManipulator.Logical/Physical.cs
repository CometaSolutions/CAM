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
using System.Linq;
using System.Text;
using CommonUtils;
using CollectionsWithRoles.API;

public static partial class E_CILLogical
{
   private sealed class PhysicalCreationState
   {
      private sealed class TypeDefOrRefOrSpecComparer : IEqualityComparer<CILTypeBase>
      {
         internal static readonly IEqualityComparer<CILTypeBase> Instance = new TypeDefOrRefOrSpecComparer();

         private TypeDefOrRefOrSpecComparer()
         {

         }

         Boolean IEqualityComparer<CILTypeBase>.Equals( CILTypeBase x, CILTypeBase y )
         {
            return x.TypeKind == y.TypeKind
               && (
                  ( x.TypeKind == TypeKind.TypeParameter && Equality( (CILTypeParameter) x, (CILTypeParameter) y ) )
                  || ( x.TypeKind != TypeKind.TypeParameter && x.Equals( y ) )
                  );
         }

         Int32 IEqualityComparer<CILTypeBase>.GetHashCode( CILTypeBase obj )
         {
            return obj.TypeKind == TypeKind.TypeParameter ?
               HashCode( (CILTypeParameter) obj ) :
               obj.GetHashCode();
         }

         private static Boolean Equality( CILTypeParameter x, CILTypeParameter y )
         {
            return x.GenericParameterPosition == y.GenericParameterPosition
               && ( x.DeclaringMethod == null ) == ( y.DeclaringMethod == null );
         }

         private static Int32 HashCode( CILTypeParameter obj )
         {
            return 17 * 23 + obj.GenericParameterPosition;
         }
      }

      private readonly CILModule _module;
      private readonly CILMetaData _md;

      private readonly IDictionary<CILTypeBase, TableIndex> _typeDefOrRefOrSpec;
      private readonly IDictionary<CILTypeBase, TableIndex> _typeDefsAsTypeSpecs;
      private readonly IDictionary<CILField, TableIndex> _fields;
      private readonly IDictionary<CILMethodBase, TableIndex> _methods;
      private readonly IDictionary<CILParameter, TableIndex> _parameters;
      private readonly IDictionary<String, Int32> _moduleRefs;
      private readonly IDictionary<CILAssembly, Int32> _assemblyRefs;

      internal PhysicalCreationState( CILModule module, CILMetaData md )
      {
         this._module = module;
         this._md = md;
         this._typeDefOrRefOrSpec = new Dictionary<CILTypeBase, TableIndex>( TypeDefOrRefOrSpecComparer.Instance );
         this._typeDefsAsTypeSpecs = new Dictionary<CILTypeBase, TableIndex>( TypeDefOrRefOrSpecComparer.Instance );
         this._fields = new Dictionary<CILField, TableIndex>();
         this._methods = new Dictionary<CILMethodBase, TableIndex>();
         this._parameters = new Dictionary<CILParameter, TableIndex>();
         this._moduleRefs = new Dictionary<String, Int32>();
         this._assemblyRefs = new Dictionary<CILAssembly, Int32>();
      }

      public CILMetaData MetaData
      {
         get
         {
            return this._md;
         }
      }

      internal Int32 RecordTypeDef( CILType type )
      {
         var tDefIdx = this._md.TypeDefinitions.RowCount;
         this._typeDefOrRefOrSpec.Add( type, new TableIndex( Tables.TypeDef, tDefIdx ) );
         return tDefIdx;
      }

      internal void RecordFieldDef( CILField field )
      {
         this._fields.Add( field, new TableIndex( Tables.Field, this._md.FieldDefinitions.RowCount ) );
      }

      internal void RecordMethodDef( CILMethodBase method )
      {
         this._methods.Add( method, new TableIndex( Tables.MethodDef, this._md.MethodDefinitions.RowCount ) );
      }

      internal void RecordParamDef( CILParameter param )
      {
         this._parameters.Add( param, new TableIndex( Tables.Parameter, this._md.ParameterDefinitions.RowCount ) );
      }

      internal TableIndex GetTypeDefOrRefOrSpec( CILTypeBase type, Boolean convertTypeDefToTypeSpec = false )
      {
         TableIndex retVal;
         var created = !this._typeDefOrRefOrSpec.TryGetValue( type, out retVal );
         if ( created )
         {
            switch ( type.TypeKind )
            {
               case TypeKind.MethodSignature:
               case TypeKind.TypeParameter:
                  // TypeSpec
                  retVal = this._md.GetNextTableIndexFor( Tables.TypeSpec );
                  break;
               case TypeKind.Type:
                  // TypeRef/Spec (since all TypeDefs should have been already added at this point)
                  retVal = this._md.GetNextTableIndexFor( ( (CILType) type ).IsGenericType() ? Tables.TypeSpec : Tables.TypeRef );
                  break;
               default:
                  throw new NotSupportedException( "Unknown type kind: " + type.TypeKind + "." );
            }
            this._typeDefOrRefOrSpec.Add( type, retVal );

            // Create signatures only after adding table index to avoid potential recursion problems
            switch ( retVal.Table )
            {
               case Tables.TypeSpec:
                  this._md.TypeSpecifications.TableContents.Add( new TypeSpecification()
                  {
                     Signature = this.CreateTypeSignature( type )
                  } );
                  break;
               case Tables.TypeRef:
                  var t = (CILType) type;
                  this._md.TypeReferences.TableContents.Add( new TypeReference()
                  {
                     Name = t.Name,
                     Namespace = t.Namespace,
                     ResolutionScope = this.GetResolutionScope( t )
                  } );
                  break;
               default:
                  throw new InvalidOperationException( "Added unexpected TypeDefOrRefOrSpec: " + retVal + "." );
            }
         }
         else if ( retVal.Table == Tables.TypeDef && convertTypeDefToTypeSpec && type.IsGenericTypeDefinition() )
         {
            retVal = this._typeDefsAsTypeSpecs.GetOrAdd_NotThreadSafe( type, t =>
            {
               created = true;
               return this._md.GetNextTableIndexFor( Tables.TypeSpec );
            } );

            if ( created )
            {
               this._md.TypeSpecifications.TableContents.Add( new TypeSpecification()
               {
                  Signature = this.CreateTypeSignature( type )
               } );
            }
         }

         return retVal;
      }

      internal TableIndex GetFieldDefOrMemberRef( CILField field, Boolean convertTypeDefToTypeSpec = false )
      {
         return this._fields.GetOrAdd_NotThreadSafe( field, f =>
         {
            // Only MemberRef possible here as all field defs should've been added at this point.
            var retVal = this._md.GetNextTableIndexFor( Tables.MemberRef );
            this._md.MemberReferences.TableContents.Add( new MemberReference()
            {
               DeclaringType = this.GetMemberRefDeclaringType( field.DeclaringType, convertTypeDefToTypeSpec ),
               Name = field.Name,
               Signature = this.CreateFieldSignature( field )
            } );
            return retVal;
         } );
      }

      internal TableIndex GetMethodDefOrMemberRefOrMethodSpec( CILMethodBase method, Boolean convertTypeDefToTypeSpec = false )
      {
         var created = false;
         var retVal = this._methods.GetOrAdd_NotThreadSafe( method, m =>
         {
            // Only MemberRef or MethodSpec possible here, since all MethodDefs should've been added at this point.
            created = true;
            return this._md.GetNextTableIndexFor( m.MethodKind == MethodKind.Method && ( (CILMethod) m ).HasGenericArguments() ? Tables.MethodSpec : Tables.MemberRef );
         } );
         if ( created )
         {
            switch ( retVal.Table )
            {
               case Tables.MemberRef:
                  this._md.MemberReferences.TableContents.Add( new MemberReference()
                  {
                     DeclaringType = this.GetMemberRefDeclaringType( method.DeclaringType, convertTypeDefToTypeSpec ),
                     Name = method.GetName(),
                     Signature = this.CreateMethodRefSignature( method )
                  } );
                  break;
               case Tables.MethodSpec:
                  this._md.MethodSpecifications.TableContents.Add( new MethodSpecification()
                  {
                     Method = this.GetMethodDefOrMemberRefOrMethodSpec( ( (CILMethod) method ).GenericDefinition, convertTypeDefToTypeSpec ),
                     Signature = this.CreateGenericMethodSignature( (CILMethod) method )
                  } );
                  break;
               default:
                  throw new InvalidOperationException( "Added unexpected MeMberRefOrMethodSpec: " + retVal.Table + "." );
            }
         }
         return retVal;
      }

      internal TableIndex GetMethodSignatureToken( CILMethodSignature signature, VarArgInstance[] varArgs, Boolean convertTypeDefToTypeSpec )
      {
         throw new NotImplementedException();
      }

      private TableIndex GetMemberRefDeclaringType( CILType type, Boolean convertTypeDefToTypeSpec = false )
      {
         return !this._module.Equals( type.Module ) && this._module.Assembly.Equals( type.Module.Assembly ) ?
            this.GetModuleRef( type.Module.Name ) :
            this.GetTypeDefOrRefOrSpec( type, convertTypeDefToTypeSpec );
      }

      internal TableIndex? GetLocalsIndex( MethodIL il )
      {
         throw new NotImplementedException();
      }

      private TableIndex GetResolutionScope( CILType type )
      {
         return type.DeclaringType == null ?
            ( Equals( type.Module.Assembly, this._module.Assembly ) ?
               this.GetModuleRef( type.Module.Name ) : // ModuleRef
               this.GetAssemblyRef( type.Module.Assembly )  // AssemblyRef
            ) :
            this.GetTypeDefOrRefOrSpec( type.DeclaringType ); // TypeRef
      }

      private TableIndex GetModuleRef( String module )
      {
         return new TableIndex( Tables.ModuleRef, this._moduleRefs.GetOrAdd_NotThreadSafe( module, m =>
         {
            var modRefs = this._md.ModuleReferences.TableContents;
            var retVal = modRefs.Count;
            modRefs.Add( new ModuleReference()
            {
               ModuleName = m
            } );
            return retVal;
         } ) );
      }

      private TableIndex GetAssemblyRef( CILAssembly assembly )
      {
         return new TableIndex( Tables.AssemblyRef, this._assemblyRefs.GetOrAdd_NotThreadSafe( assembly, a =>
         {
            var aRefs = this._md.AssemblyReferences.TableContents;
            var retVal = aRefs.Count;
            var an = assembly.Name;
            var aRef = new AssemblyReference();
            an.AssemblyInformation.DeepCopyContentsTo( aRef.AssemblyInformation );
            if ( !an.PublicKey.IsNullOrEmpty() )
            {
               aRef.Attributes |= AssemblyFlags.PublicKey;
            }
            aRefs.Add( aRef );
            return retVal;
         } ) );
      }
   }

   public static CILMetaData CreatePhysicalRepresentation( this CILModule module )
   {
      var retVal = CILMetaDataFactory.NewBlankMetaData();

      var state = new PhysicalCreationState( module, retVal );
      state.ProcessLogicalForPhysical( module );

      return retVal;
   }

   private static void ProcessLogicalForPhysical( this PhysicalCreationState state, CILModule module )
   {
      var md = state.MetaData;
      md.ModuleDefinitions.TableContents.Add( new ModuleDefinition()
      {
         Name = module.Name,
         ModuleGUID = Guid.NewGuid()
      } );

      state.ProcessLogicalForPhysical( module.ModuleInitializer );
      foreach ( var type in module.DefinedTypes )
      {
         state.ProcessLogicalForPhysical( type );
      }

      state.PostProcessLogicalForPhysical( module.ModuleInitializer );
      foreach ( var type in module.DefinedTypes )
      {
         state.ProcessLogicalForPhysical( type );
      }
   }

   private static void ProcessLogicalForPhysical( this PhysicalCreationState state, CILType typeInModule )
   {
      var md = state.MetaData;

      foreach ( var type in typeInModule.AsDepthFirstEnumerable( t => t.DeclaredNestedTypes ) )
      {
         var thisTypeDefIdx = state.RecordTypeDef( type );
         md.TypeDefinitions.TableContents.Add( new TypeDefinition()
         {
            Name = type.Name,
            Namespace = type.Namespace,
            Attributes = type.Attributes,
            FieldList = new TableIndex( Tables.Field, md.FieldDefinitions.RowCount ),
            MethodList = new TableIndex( Tables.MethodDef, md.MethodDefinitions.RowCount )
         } );

         foreach ( var field in type.DeclaredFields )
         {
            state.RecordFieldDef( field );
            md.FieldDefinitions.TableContents.Add( new FieldDefinition()
            {
               Attributes = field.Attributes,
               Name = field.Name
            } );
         }

         foreach ( var ctor in type.Constructors )
         {
            state.ProcessLogicalForPhysical( ctor );
         }

         foreach ( var method in type.DeclaredMethods )
         {
            state.ProcessLogicalForPhysical( method );
         }

         if ( type.DeclaringType != null )
         {
            md.NestedClassDefinitions.TableContents.Add( new NestedClassDefinition()
            {
               EnclosingClass = state.GetTypeDefOrRefOrSpec( type.DeclaringType ),
               NestedClass = new TableIndex( Tables.TypeDef, thisTypeDefIdx )
            } );
         }
      }
   }

   private static void ProcessLogicalForPhysical( this PhysicalCreationState state, CILMethodBase method )
   {
      var md = state.MetaData;
      var paramDef = md.ParameterDefinitions.TableContents;

      state.RecordMethodDef( method );
      md.MethodDefinitions.TableContents.Add( new MethodDefinition()
      {
         Attributes = method.Attributes,
         ImplementationAttributes = method.ImplementationAttributes,
         Name = method.GetName(),
         ParameterList = new TableIndex( Tables.Parameter, paramDef.Count ),
      } );


      if ( method.MethodKind == MethodKind.Method )
      {
         var retParam = ( (CILMethod) method ).ReturnParameter;
         if ( retParam.CustomAttributeData.Count > 0 || retParam.MarshalingInformation != null )
         {
            // Add row for this return parameter, since it will be refenced by CA table or marshalling info
            state.ProcessLogicalForPhysical( retParam );
         }
      }

      foreach ( var param in method.Parameters )
      {
         state.ProcessLogicalForPhysical( param );
      }
   }

   private static void ProcessLogicalForPhysical( this PhysicalCreationState state, CILParameter param )
   {
      state.RecordParamDef( param );
      state.MetaData.ParameterDefinitions.TableContents.Add( new ParameterDefinition()
      {
         Attributes = param.Attributes,
         Name = param.Name,
         Sequence = param.Position + 1
      } );
   }

   private static void PostProcessLogicalForPhysical( this PhysicalCreationState state, CILType typeInModule )
   {

      var md = state.MetaData;
      foreach ( var type in typeInModule.AsDepthFirstEnumerable( t => t.DeclaredNestedTypes ) )
      {
         // Set base type here, as it may be another type def!
         var bType = type.BaseType;
         if ( bType != null )
         {
            md.TypeDefinitions.TableContents[state.GetTypeDefOrRefOrSpec( type ).Index].BaseType = state.GetTypeDefOrRefOrSpec( bType );
         }

         // Create signatures for fields
         foreach ( var field in type.DeclaredFields )
         {
            md.FieldDefinitions.TableContents[state.GetFieldDefOrMemberRef( field ).Index].Signature = state.CreateFieldSignature( field );
         }

         // Create signatures and IL for methods
         foreach ( var method in type.Constructors.Cast<CILMethodBase>().Concat( type.DeclaredMethods ) )
         {
            var mDef = md.MethodDefinitions.TableContents[state.GetMethodDefOrMemberRefOrMethodSpec( method ).Index];
            mDef.Signature = state.CreateMethodDefSignature( method );
            if ( method.HasILMethodBody() )
            {
               mDef.IL = state.ProcessLogicalForPhysical( method.MethodIL );
            }
         }
      }
   }

   private static MethodILDefinition ProcessLogicalForPhysical( this PhysicalCreationState state, MethodIL logicalIL )
   {
      var physicalIL = new MethodILDefinition( logicalIL.ExceptionBlocks.Count(), logicalIL.OpCodeCount )
      {
         InitLocals = logicalIL.InitLocals,
         LocalsSignatureIndex = state.GetLocalsIndex( logicalIL )
      };

      var logicalToByteOffset = new Int32[logicalIL.OpCodeCount];
      var pOpCodes = physicalIL.OpCodes;
      var branchCodeIndices = new List<Int32>();
      var dynamicBranchInfos = new Dictionary<Int32, Tuple<OpCode, OpCode>>();

      foreach ( var lOpCode in logicalIL.OpCodeInfos )
      {
         OpCodeInfo pOpCode;
         switch ( lOpCode.InfoKind )
         {
            case OpCodeInfoKind.OperandNone:
               pOpCode = OpCodeInfoWithNoOperand.GetInstanceFor( ( (LogicalOpCodeInfoWithNoOperand) lOpCode ).Code );
               break;
            case OpCodeInfoKind.OperandTypeToken:
               var lt = (LogicalOpCodeInfoWithTypeToken) lOpCode;
               pOpCode = new OpCodeInfoWithToken( lt.Code, state.GetTypeDefOrRefOrSpec( lt.ReflectionObject, !lt.UseGenericDefinitionIfPossible ) );
               break;
            case OpCodeInfoKind.OperandFieldToken:
               var ft = (LogicalOpCodeInfoWithFieldToken) lOpCode;
               pOpCode = new OpCodeInfoWithToken( ft.Code, state.GetFieldDefOrMemberRef( ft.ReflectionObject, !ft.UseGenericDefinitionIfPossible ) );
               break;
            case OpCodeInfoKind.OperandMethodToken:
               var lm = (LogicalOpCodeInfoWithMethodToken) lOpCode;
               pOpCode = new OpCodeInfoWithToken( lm.Code, state.GetMethodDefOrMemberRefOrMethodSpec( lm.ReflectionObject, !lm.UseGenericDefinitionIfPossible ) );
               break;
            case OpCodeInfoKind.OperandCtorToken:
               var ct = (LogicalOpCodeInfoWithCtorToken) lOpCode;
               pOpCode = new OpCodeInfoWithToken( ct.Code, state.GetMethodDefOrMemberRefOrMethodSpec( ct.ReflectionObject, !ct.UseGenericDefinitionIfPossible ) );
               break;
            case OpCodeInfoKind.OperandMethodSigToken:
               var lms = (LogicalOpCodeInfoWithMethodSig) lOpCode;
               pOpCode = new OpCodeInfoWithToken( lms.Code, state.GetMethodSignatureToken( lms.ReflectionObject, lms.VarArgs, !lms.UseGenericDefinitionIfPossible ) );
               break;
            case OpCodeInfoKind.NormalOrVirtual:
               var lv = (LogicalOpCodeInfoForNormalOrVirtual) lOpCode;
               pOpCode = new OpCodeInfoWithToken( lv.ReflectionObject.Attributes.IsStatic() ? lv.NormalCode : lv.VirtualCode, state.GetMethodDefOrMemberRefOrMethodSpec( lv.ReflectionObject, true ) );
               break;
            case OpCodeInfoKind.OperandString:
               var s = (LogicalOpCodeInfoWithFixedSizeOperandString) lOpCode;
               pOpCode = new OpCodeInfoWithString( s.Code, s.Operand );
               break;
            case OpCodeInfoKind.OperandUInt16:
               var sh = (LogicalOpCodeInfoWithFixedSizeOperandUInt16) lOpCode;
               pOpCode = new OpCodeInfoWithInt32( sh.Code, sh.Operand );
               break;
            case OpCodeInfoKind.OperandInt32:
               var lg = (LogicalOpCodeInfoWithFixedSizeOperandInt32) lOpCode;
               pOpCode = new OpCodeInfoWithInt32( lg.Code, lg.Operand );
               break;
            case OpCodeInfoKind.OperandInt64:
               var llg = (LogicalOpCodeInfoWithFixedSizeOperandInt64) lOpCode;
               pOpCode = new OpCodeInfoWithInt64( llg.Code, llg.Operand );
               break;
            case OpCodeInfoKind.OperandR4:
               var fs = (LogicalOpCodeInfoWithFixedSizeOperandSingle) lOpCode;
               pOpCode = new OpCodeInfoWithSingle( fs.Code, fs.Operand );
               break;
            case OpCodeInfoKind.OperandR8:
               var fd = (LogicalOpCodeInfoWithFixedSizeOperandDouble) lOpCode;
               pOpCode = new OpCodeInfoWithDouble( fd.Code, fd.Operand );
               break;
            case OpCodeInfoKind.Branch:
               var bl = (LogicalOpCodeInfoForBranch) lOpCode;
               pOpCode = new OpCodeInfoWithInt32( bl.ShortForm, logicalIL.GetLabelOffset( bl.TargetLabel ) );
               dynamicBranchInfos.Add( pOpCodes.Count, Tuple.Create( bl.ShortForm, bl.LongForm ) );
               branchCodeIndices.Add( pOpCodes.Count );
               break;
            case OpCodeInfoKind.Switch:
               var sw = (LogicalOpCodeInfoForSwitch) lOpCode;
               var pSw = new OpCodeInfoWithSwitch( OpCodes.Switch, sw.Labels.Count() );
               pSw.Offsets.AddRange( sw.Labels.Select( l => logicalIL.GetLabelOffset( l ) ) );
               pOpCode = pSw;
               branchCodeIndices.Add( pOpCodes.Count );
               break;
            case OpCodeInfoKind.Leave:
               var ll = (LogicalOpCodeInfoForLeave) lOpCode;
               pOpCode = new OpCodeInfoWithInt32( ll.ShortForm, logicalIL.GetLabelOffset( ll.TargetLabel ) );
               dynamicBranchInfos.Add( pOpCodes.Count, Tuple.Create( ll.ShortForm, ll.LongForm ) );
               branchCodeIndices.Add( pOpCodes.Count );
               break;
            case OpCodeInfoKind.BranchOrLeaveFixed:
               var bf = (LogicalOpCodeInfoForFixedBranchOrLeave) lOpCode;
               pOpCode = new OpCodeInfoWithInt32( bf.Code, logicalIL.GetLabelOffset( bf.TargetLabel ) );
               branchCodeIndices.Add( pOpCodes.Count );
               break;
            default:
               throw new InvalidOperationException( "Invalid logical opcode kind: " + lOpCode.InfoKind + "." );
         }

         pOpCodes.Add( pOpCode );
      }

      // First, walk through each dynamic branch code and decide between short and long notation

      // Then, walk through each branch code and fix logical offset -> physical offset

      return physicalIL;
   }

   private static FieldSignature CreateFieldSignature( this PhysicalCreationState state, CILField field )
   {
      var retVal = new FieldSignature( field.CustomModifiers.Count )
      {
         Type = state.CreateTypeSignature( field.FieldType )
      };
      state.AddCustomMods( field.CustomModifiers, retVal.CustomModifiers );
      return retVal;
   }

   private static void AddCustomMods( this PhysicalCreationState state, ListQuery<CILCustomModifier> logicalMods, List<CustomModifierSignature> physicalMods )
   {
      foreach ( var mod in logicalMods )
      {
         physicalMods.Add( new CustomModifierSignature()
         {
            IsOptional = mod.IsOptional,
            CustomModifierType = state.GetTypeDefOrRefOrSpec( mod.Modifier )
         } );
      }
   }

   private static TypeSignature CreateTypeSignature( this PhysicalCreationState state, CILTypeBase type )
   {
      switch ( type.TypeKind )
      {
         case TypeKind.MethodSignature:
            return new FunctionPointerTypeSignature()
            {
               MethodSignature = state.CreateMethodRefSignature( (CILMethodSignature) type )
            };
         case TypeKind.Type:
            var t = (CILType) type;
            var elKind = t.ElementKind;
            if ( elKind.HasValue )
            {
               switch ( elKind.Value )
               {
                  case ElementKind.Array:
                     var complexInfo = t.ArrayInformation;
                     if ( complexInfo == null )
                     {
                        var szArray = new SimpleArrayTypeSignature( t.CustomAttributeData.Count )
                        {
                           ArrayType = state.CreateTypeSignature( t.ElementType )
                        };
                        //state.AddCustomMods(t.CustomModifiers, szArray.CustomModifiers);
                        return szArray;
                     }
                     else
                     {
                        var cArray = new ComplexArrayTypeSignature( complexInfo.Sizes.Count, complexInfo.LowerBounds.Count )
                        {
                           Rank = complexInfo.Rank,
                           ArrayType = state.CreateTypeSignature( t.ElementType )
                        };
                        cArray.Sizes.AddRange( complexInfo.Sizes );
                        cArray.LowerBounds.AddRange( complexInfo.LowerBounds );
                        return cArray;
                     }
                  case ElementKind.Pointer:
                     return new PointerTypeSignature()
                     {
                        PointerType = state.CreateTypeSignature( t.ElementType )
                     };
                  case ElementKind.Reference:
                     // Just skip through
                     return state.CreateTypeSignature( t.ElementType );
                  default:
                     throw new NotSupportedException( "Unknown element kind: " + elKind.Value + "." );
               }
            }
            else
            {
               var tc = t.TypeCode;
               switch ( tc )
               {
                  case CILTypeCode.Boolean:
                     return SimpleTypeSignature.Boolean;
                  case CILTypeCode.Byte:
                     return SimpleTypeSignature.Byte;
                  case CILTypeCode.Char:
                     return SimpleTypeSignature.Char;
                  case CILTypeCode.Double:
                     return SimpleTypeSignature.Double;
                  case CILTypeCode.Int16:
                     return SimpleTypeSignature.Int16;
                  case CILTypeCode.Int32:
                     return SimpleTypeSignature.Int32;
                  case CILTypeCode.Int64:
                     return SimpleTypeSignature.Int64;
                  case CILTypeCode.IntPtr:
                     return SimpleTypeSignature.IntPtr;
                  case CILTypeCode.SystemObject:
                     return SimpleTypeSignature.Object;
                  case CILTypeCode.SByte:
                     return SimpleTypeSignature.SByte;
                  case CILTypeCode.Single:
                     return SimpleTypeSignature.Single;
                  case CILTypeCode.String:
                     return SimpleTypeSignature.String;
                  case CILTypeCode.TypedByRef:
                     return SimpleTypeSignature.TypedByRef;
                  case CILTypeCode.UInt16:
                     return SimpleTypeSignature.UInt16;
                  case CILTypeCode.UInt32:
                     return SimpleTypeSignature.UInt32;
                  case CILTypeCode.UInt64:
                     return SimpleTypeSignature.UInt64;
                  case CILTypeCode.UIntPtr:
                     return SimpleTypeSignature.UIntPtr;
                  case CILTypeCode.Void:
                     return SimpleTypeSignature.Void;
                  default:
                     var gArgs = t.GenericArguments;
                     var classOrValue = new ClassOrValueTypeSignature( gArgs.Count )
                     {
                        IsClass = !t.IsValueType(),
                        Type = state.GetTypeDefOrRefOrSpec( t )
                     };
                     foreach ( var gArg in gArgs )
                     {
                        classOrValue.GenericArguments.Add( state.CreateTypeSignature( gArg ) );
                     }
                     return classOrValue;
               }
            }
         case TypeKind.TypeParameter:
            var tParam = (CILTypeParameter) type;
            return new GenericParameterTypeSignature()
            {
               GenericParameterIndex = tParam.GenericParameterPosition,
               IsTypeParameter = tParam.DeclaringMethod == null
            };
         default:
            throw new NotSupportedException( "Unknown type kind: " + type.TypeKind + "." );
      }
   }

   private static MethodDefinitionSignature CreateMethodDefSignature( this PhysicalCreationState state, CILMethodBase method )
   {
      var retVal = new MethodDefinitionSignature( method.Parameters.Count );
      state.ProcessMethodDefOrRefSignature( method, retVal );
      return retVal;
   }

   private static MethodReferenceSignature CreateMethodRefSignature( this PhysicalCreationState state, CILMethodBase method )
   {
      var retVal = new MethodReferenceSignature( method.Parameters.Count );
      state.ProcessMethodDefOrRefSignature( method, retVal );
      return retVal;
   }

   private static MethodReferenceSignature CreateMethodRefSignature( this PhysicalCreationState state, CILMethodSignature method )
   {
      var retVal = new MethodReferenceSignature( method.Parameters.Count );
      retVal.GenericArgumentCount = 0;
      retVal.SignatureStarter = (SignatureStarters) method.CallingConvention;
      retVal.ReturnType = state.CreateParameterSignature( method.ReturnParameter );
      retVal.Parameters.AddRange( method.Parameters.Select( p => state.CreateParameterSignature( p ) ) );
      return retVal;
   }

   private static void ProcessMethodDefOrRefSignature( this PhysicalCreationState state, CILMethodBase method, AbstractMethodSignature sig )
   {
      var m = method.MethodKind == MethodKind.Method ? (CILMethod) method : null;
      var isGeneric = m.HasGenericArguments();
      sig.GenericArgumentCount = isGeneric ? 0 : m.GenericArguments.Count;
      sig.SignatureStarter = m.CallingConvention.GetSignatureStarter( method.Attributes.IsStatic(), isGeneric );
      sig.ReturnType = state.CreateParameterSignature( m == null ? null : m.ReturnParameter );
      sig.Parameters.AddRange( method.Parameters.Select( p => state.CreateParameterSignature( p ) ) );
   }

   private static ParameterSignature CreateParameterSignature( this PhysicalCreationState state, CILParameter parameter )
   {
      ParameterSignature retVal;
      if ( parameter == null )
      {
         retVal = new ParameterSignature()
         {
            IsByRef = false,
            Type = SimpleTypeSignature.Void
         };
      }
      else
      {
         retVal = new ParameterSignature( parameter.CustomModifiers.Count )
         {
            IsByRef = parameter.ParameterType.IsByRef(),
            Type = state.CreateTypeSignature( parameter.ParameterType )
         };
         state.AddCustomMods( parameter.CustomModifiers, retVal.CustomModifiers );
      }
      return retVal;
   }

   private static ParameterSignature CreateParameterSignature( this PhysicalCreationState state, CILParameterSignature parameter )
   {
      var retVal = new ParameterSignature( parameter.CustomModifiers.Count )
      {
         IsByRef = parameter.ParameterType.IsByRef(),
         Type = state.CreateTypeSignature( parameter.ParameterType )
      };
      state.AddCustomMods( parameter.CustomModifiers, retVal.CustomModifiers );
      return retVal;
   }

   private static GenericMethodSignature CreateGenericMethodSignature( this PhysicalCreationState state, CILMethod method )
   {
      var retVal = new GenericMethodSignature( method.GenericArguments.Count );
      retVal.GenericArguments.AddRange( method.GenericArguments.Select( g => state.CreateTypeSignature( g ) ) );
      return retVal;
   }
}
