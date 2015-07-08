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

      private static readonly IEqualityComparer<LocalBuilder> LocalsEqualityComparer = ComparerFromFunctions.NewEqualityComparer<LocalBuilder>(
         ( x, y ) => x.LocalIndex == y.LocalIndex && x.LocalType.Equals( y.LocalType ) && x.IsPinned == y.IsPinned,
         x => x.LocalType.GetHashCode()
         );

      private readonly CILModule _module;
      private readonly CILMetaData _md;

      private readonly IDictionary<CILTypeBase, TableIndex> _typeDefOrRefOrSpec;
      private readonly IDictionary<CILTypeBase, TableIndex> _typeDefsAsTypeSpecs;
      private readonly IDictionary<CILField, TableIndex> _fields;
      private readonly IDictionary<CILMethodBase, TableIndex> _methods;
      private readonly IDictionary<CILParameter, TableIndex> _parameters;
      private readonly IDictionary<String, TableIndex> _moduleRefs;
      private readonly IDictionary<CILAssembly, TableIndex> _assemblyRefs;
      private readonly IDictionary<String, TableIndex> _fileRefs;
      private readonly IDictionary<IList<LocalBuilder>, TableIndex> _locals;

      internal PhysicalCreationState( CILModule module, CILMetaData md )
      {
         this._module = module;
         this._md = md;
         this._typeDefOrRefOrSpec = new Dictionary<CILTypeBase, TableIndex>( TypeDefOrRefOrSpecComparer.Instance );
         this._typeDefsAsTypeSpecs = new Dictionary<CILTypeBase, TableIndex>( TypeDefOrRefOrSpecComparer.Instance );
         this._fields = new Dictionary<CILField, TableIndex>();
         this._methods = new Dictionary<CILMethodBase, TableIndex>();
         this._parameters = new Dictionary<CILParameter, TableIndex>();
         this._moduleRefs = new Dictionary<String, TableIndex>();
         this._assemblyRefs = new Dictionary<CILAssembly, TableIndex>();
         this._fileRefs = new Dictionary<String, TableIndex>();
         this._locals = new Dictionary<IList<LocalBuilder>, TableIndex>( ListEqualityComparer<IList<LocalBuilder>, LocalBuilder>.NewListEqualityComparer( LocalsEqualityComparer ) );
      }

      public CILMetaData MetaData
      {
         get
         {
            return this._md;
         }
      }

      public CILModule LogicalModule
      {
         get
         {
            return this._module;
         }
      }

      internal TableIndex RecordTypeDef( CILType type )
      {
         var retVal = this._md.GetNextTableIndexFor( Tables.TypeDef );
         this._typeDefOrRefOrSpec.Add( type, retVal );
         return retVal;
      }

      internal TableIndex RecordFieldDef( CILField field )
      {
         var retVal = this._md.GetNextTableIndexFor( Tables.Field );
         this._fields.Add( field, retVal );
         return retVal;
      }

      internal TableIndex RecordMethodDef( CILMethodBase method )
      {
         var retVal = this._md.GetNextTableIndexFor( Tables.MethodDef );
         this._methods.Add( method, retVal );
         return retVal;
      }

      internal TableIndex RecordParamDef( CILParameter param )
      {
         var retVal = this._md.GetNextTableIndexFor( Tables.Parameter );
         this._parameters.Add( param, retVal );
         return retVal;
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

      internal TableIndex GetMethodDefOrMemberRefOrMethodSpec( CILMethodBase method, Boolean useMemberRefForGenericMethodDefs = true, Boolean convertTypeDefToTypeSpec = false )
      {
         var created = false;
         var retVal = this._methods.GetOrAdd_NotThreadSafe( method, m =>
         {
            // Only MemberRef or MethodSpec possible here, since all MethodDefs should've been added at this point.
            created = true;
            Tables table = Tables.MemberRef;
            if ( m.MethodKind == MethodKind.Method )
            {
               var mm = (CILMethod) m;
               if ( mm.HasGenericArguments()
                  && ( !mm.IsGenericMethodDefinition() || !useMemberRefForGenericMethodDefs ) )
               {
                  table = Tables.MethodSpec;
               }
            }
            return this._md.GetNextTableIndexFor( table );
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
                     Method = this.GetMethodDefOrMemberRefOrMethodSpec( ( (CILMethod) method ).GenericDefinition, true, convertTypeDefToTypeSpec ),
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

      internal TableIndex? GetLocalsIndex( MethodIL il )
      {
         var locals = ( (CILAssemblyManipulator.Logical.Implementation.MethodILImpl) il ).LocalsList;
         return locals.Count == 0 ?
            (TableIndex?) null :
            this._locals.GetOrAdd_NotThreadSafe( locals, l =>
            {
               var retVal = this._md.GetNextTableIndexFor( Tables.StandaloneSignature );
               this._md.StandaloneSignatures.TableContents.Add( new StandaloneSignature()
               {
                  StoreSignatureAsFieldSignature = false,
                  Signature = this.CreateLocalsSignature( l )
               } );
               return retVal;
            } );
      }

      internal Boolean TryGetParamIndex( CILParameter param, out TableIndex paramIdx )
      {
         return this._parameters.TryGetValue( param, out paramIdx );
      }

      internal TableIndex GetModuleRef( String module )
      {
         return this._moduleRefs.GetOrAdd_NotThreadSafe( module, m =>
         {
            var retVal = this._md.GetNextTableIndexFor( Tables.ModuleRef );
            this._md.ModuleReferences.TableContents.Add( new ModuleReference()
            {
               ModuleName = m
            } );
            return retVal;
         } );
      }

      internal TableIndex GetFileRef( String fileName, FileAttributes attrs, Byte[] hashValue )
      {
         return this._fileRefs.GetOrAdd_NotThreadSafe( fileName, fn =>
         {
            var retVal = this._md.GetNextTableIndexFor( Tables.File );
            this._md.FileReferences.TableContents.Add( new FileReference()
            {
               Attributes = attrs,
               HashValue = hashValue,
               Name = fn
            } );
            return retVal;
         } );
      }

      internal TableIndex GetAssemblyRef( CILAssembly assembly )
      {
         return this._assemblyRefs.GetOrAdd_NotThreadSafe( assembly, a =>
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
            return new TableIndex( Tables.AssemblyRef, retVal );
         } );
      }

      private TableIndex GetMemberRefDeclaringType( CILType type, Boolean convertTypeDefToTypeSpec = false )
      {
         return !this._module.Equals( type.Module ) && this._module.Assembly.Equals( type.Module.Assembly ) ?
            this.GetModuleRef( type.Module.Name ) :
            this.GetTypeDefOrRefOrSpec( type, convertTypeDefToTypeSpec );
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




   }

   private sealed class PhysicalILCreationState
   {
      private readonly PhysicalCreationState _moduleState;
      private readonly MethodIL _logicalIL;
      private readonly MethodILDefinition _physicalIL;
      private readonly Int32[] _opCodeInfoByteOffsets;
      private readonly IList<Int32> _dynamicOpCodeInfos;

      internal PhysicalILCreationState( PhysicalCreationState moduleState, MethodIL logicalIL, MethodILDefinition physicalIL )
      {
         this._moduleState = moduleState;
         this._logicalIL = logicalIL;
         this._physicalIL = physicalIL;
         this._opCodeInfoByteOffsets = new Int32[logicalIL.OpCodeCount];
         this._dynamicOpCodeInfos = new List<Int32>();
      }

      public PhysicalCreationState ModuleState
      {
         get
         {
            return this._moduleState;
         }
      }

      public MethodIL LogicalIL
      {
         get
         {
            return this._logicalIL;
         }
      }

      public MethodILDefinition PhysicalIL
      {
         get
         {
            return this._physicalIL;
         }
      }

      public Int32[] OpCodeInfoByteOffsets
      {
         get
         {
            return this._opCodeInfoByteOffsets;
         }
      }

      public IList<Int32> DynamicOpCodeInfos
      {
         get
         {
            return this._dynamicOpCodeInfos;
         }
      }

   }

   public static CILMetaData CreatePhysicalRepresentation( this CILModule module, Boolean orderAndRemoveDuplicates = true )
   {
      var retVal = CILMetaDataFactory.NewBlankMetaData();

      var state = new PhysicalCreationState( module, retVal );
      state.ProcessLogicalForPhysical( module );
      state.PostProcessLogicalForPhysical( module );

      if ( orderAndRemoveDuplicates )
      {
         retVal.OrderTablesAndRemoveDuplicates();
      }

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

      var kek = module.GetAllTypes().ToArray();

      foreach ( var type in module.GetAllTypes() )
      {
         state.ProcessLogicalForPhysical( type );
      }
   }

   private static void ProcessLogicalForPhysical( this PhysicalCreationState state, CILType type )
   {
      var md = state.MetaData;

      var thisTypeDefIdx = state.RecordTypeDef( type );
      md.TypeDefinitions.TableContents.Add( new TypeDefinition()
      {
         Name = type.Name,
         Namespace = type.Namespace,
         Attributes = type.Attributes,
         FieldList = md.GetNextTableIndexFor( Tables.Field ),
         MethodList = md.GetNextTableIndexFor( Tables.MethodDef )
      } );

      foreach ( var field in type.DeclaredFields )
      {
         state.ProcessLogicalForPhysical( field );
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
            NestedClass = thisTypeDefIdx
         } );
      }
   }

   private static void ProcessLogicalForPhysical( this PhysicalCreationState state, CILField field )
   {
      var fieldIdx = state.RecordFieldDef( field );
      var md = state.MetaData;
      md.FieldDefinitions.TableContents.Add( new FieldDefinition()
      {
         Attributes = field.Attributes,
         Name = field.Name
      } );

      if ( field.Attributes.HasDefault() )
      {
         state.AddToConstantTable( fieldIdx, field.ConstantValue );
      }

      // TODO Alternatively could just that .DeclaringType.Attributes.IsExplicitLayout()... ?
      var offset = field.FieldOffset;
      if ( offset != CILAssemblyManipulator.Logical.Implementation.CILFieldImpl.NO_OFFSET )
      {
         md.FieldLayouts.TableContents.Add( new FieldLayout()
         {
            Field = fieldIdx,
            Offset = offset
         } );
      }
      var initialValue = field.InitialValue;
      if ( initialValue != null )
      {
         md.FieldRVAs.TableContents.Add( new FieldRVA()
         {
            Data = initialValue,
            Field = fieldIdx
         } );
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
         ParameterList = md.GetNextTableIndexFor( Tables.Parameter )
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
      var paramIdx = state.RecordParamDef( param );
      var md = state.MetaData;
      md.ParameterDefinitions.TableContents.Add( new ParameterDefinition()
      {
         Attributes = param.Attributes,
         Name = param.Name,
         Sequence = param.Position + 1
      } );

      if ( param.Attributes.HasDefault() )
      {
         state.AddToConstantTable( paramIdx, param.ConstantValue );
      }
   }

   private static void AddToConstantTable( this PhysicalCreationState state, TableIndex parent, Object constant )
   {
      SignatureElementTypes sig;
      if ( constant == null )
      {
         sig = SignatureElementTypes.Class;
      }
      else
      {
         var tc = Type.GetTypeCode( constant.GetType() );

         switch ( tc )
         {
            case TypeCode.Boolean:
               sig = SignatureElementTypes.Boolean;
               break;
            case TypeCode.Char:
               sig = SignatureElementTypes.Char;
               break;
            case TypeCode.SByte:
               sig = SignatureElementTypes.I1;
               break;
            case TypeCode.Byte:
               sig = SignatureElementTypes.U1;
               break;
            case TypeCode.Int16:
               sig = SignatureElementTypes.I2;
               break;
            case TypeCode.UInt16:
               sig = SignatureElementTypes.U2;
               break;
            case TypeCode.Int32:
               sig = SignatureElementTypes.I4;
               break;
            case TypeCode.UInt32:
               sig = SignatureElementTypes.U4;
               break;
            case TypeCode.Int64:
               sig = SignatureElementTypes.I8;
               break;
            case TypeCode.UInt64:
               sig = SignatureElementTypes.U8;
               break;
            case TypeCode.Single:
               sig = SignatureElementTypes.R4;
               break;
            case TypeCode.Double:
               sig = SignatureElementTypes.R8;
               break;
            case TypeCode.String:
               sig = SignatureElementTypes.String;
               break;
            default:
               throw new InvalidOperationException( "Constant of type " + constant.GetType() + " is not supported." );
         }
      }

      state.MetaData.ConstantDefinitions.TableContents.Add( new ConstantDefinition()
      {
         Parent = parent,
         Type = sig,
         Value = constant
      } );
   }

   private static void PostProcessLogicalForPhysical( this PhysicalCreationState state, CILModule module )
   {
      // Custom attributes
      state.AddToCustomAttributeTable( new TableIndex( Tables.Module, 0 ), module );

      // Assembly definition, if main module
      if ( module.Equals( module.Assembly.MainModule ) )
      {
         state.PostProcessLogicalForPhysical( module.Assembly );
      }

      // Rest of the type-related tables
      foreach ( var type in module.GetAllTypes() )
      {
         state.PostProcessLogicalForPhysical( type );
      }

      // Only now calculate max stack sizes, as the calculation will need signatures created in post-processing types
      var md = state.MetaData;
      var mDefs = md.MethodDefinitions.TableContents;
      for ( var mIdx = 0; mIdx < mDefs.Count; ++mIdx )
      {
         var mDef = mDefs[mIdx];
         if ( mDef.IL != null )
         {
            mDef.IL.MaxStackSize = md.CalculateStackSize( mIdx );
         }
      }

      // Manifest resources
      md.ManifestResources.TableContents.AddRange( module.ManifestResources.Select( kvp =>
      {
         var name = kvp.Key;
         var res = kvp.Value;

         var retVal = new ManifestResource()
         {
            Attributes = res.Attributes,
            Name = name
         };
         switch ( res.ManifestResourceKind )
         {
            case ManifestResourceKind.Embedded:
               retVal.DataInCurrentFile = ( (EmbeddedManifestResource) res ).Data;
               break;
            case ManifestResourceKind.AnotherFile:
               var fRes = (FileManifestResource) res;
               retVal.Implementation = state.GetFileRef( fRes.FileName, FileAttributes.ContainsNoMetadata, fRes.Hash );
               break;
            case ManifestResourceKind.AnotherAssembly:
               var aRes = (AssemblyManifestResource) res;
               retVal.Implementation = state.GetAssemblyRef( aRes.Assembly );
               // TODO offset!!
               retVal.Offset = -1;
               break;
         }
         return retVal;
      } ) );
   }

   private static void PostProcessLogicalForPhysical( this PhysicalCreationState state, CILAssembly assembly )
   {
      var md = state.MetaData;
      var aDefIdx = md.GetNextTableIndexFor( Tables.Assembly );
      var an = assembly.Name;
      var aDef = new AssemblyDefinition()
      {
         Attributes = AssemblyFlags.PublicKey,
         HashAlgorithm = an.HashAlgorithm
      };
      var aInfo = aDef.AssemblyInformation;
      aInfo.Name = an.Name;
      aInfo.Culture = an.Culture;
      aInfo.VersionMajor = an.MajorVersion;
      aInfo.VersionMinor = an.MinorVersion;
      aInfo.VersionBuild = an.BuildNumber;
      aInfo.VersionRevision = an.Revision;
      aInfo.PublicKeyOrToken = an.PublicKey;
      md.AssemblyDefinitions.TableContents.Add( aDef );

      state.AddToCustomAttributeTable( aDefIdx, assembly );
      foreach ( var otherMod in assembly.Modules.Where( m => !m.Equals( assembly.MainModule ) ) )
      {
         // TODO hash value!
         state.GetFileRef( otherMod.Name, FileAttributes.ContainsMetadata, null );
      }

      // TODO ExportedType
   }

   private static void PostProcessLogicalForPhysical( this PhysicalCreationState state, CILType type )
   {
      var md = state.MetaData;
      var typeIdx = state.GetTypeDefOrRefOrSpec( type );

      // Set base type here, as it may be another type def!
      var bType = type.BaseType;
      if ( bType != null )
      {
         md.TypeDefinitions.TableContents[typeIdx.Index].BaseType = state.GetTypeDefOrRefOrSpec( bType );
      }

      // Process generic arguments
      state.ProcessLogicalForPhysicalGArgs( type );

      // Create signatures for methods
      foreach ( var method in type.GetAllConstructorsAndMethods() )
      {
         state.PostProcessLogicalForPhysical( method );
      }

      // Create signatures for fields
      foreach ( var field in type.DeclaredFields )
      {
         md.FieldDefinitions.TableContents[state.GetFieldDefOrMemberRef( field ).Index].Signature = state.CreateFieldSignature( field );
      }

      // Events first as Association table index in MethodSemantics is sorted with having Event first
      foreach ( var evt in type.DeclaredEvents )
      {
         state.PostProcessLogicalForPhysical( evt );
      }

      foreach ( var property in type.DeclaredProperties )
      {
         state.PostProcessLogicalForPhysical( property );
      }

      state.AddToCustomAttributeTable( typeIdx, type );
      state.AddToSecurityTable( typeIdx, type );
      var layout = type.Layout;
      if ( layout.HasValue )
      {
         md.ClassLayouts.TableContents.Add( new ClassLayout()
         {
            ClassSize = layout.Value.Size,
            PackingSize = layout.Value.Pack,
            Parent = typeIdx
         } );
      }
      md.InterfaceImplementations.TableContents.AddRange( type.DeclaredInterfaces.Select( iFace => new InterfaceImplementation()
      {
         Class = typeIdx,
         Interface = state.GetTypeDefOrRefOrSpec( iFace )
      } ) );

   }

   private static void PostProcessLogicalForPhysical( this PhysicalCreationState state, CILField field )
   {
      var md = state.MetaData;
      var fieldIdx = state.GetFieldDefOrMemberRef( field );
      md.FieldDefinitions.TableContents[fieldIdx.Index].Signature = state.CreateFieldSignature( field );

      state.AddToCustomAttributeTable( fieldIdx, field );
      state.AddToMarshalTable( fieldIdx, field );
   }

   private static void PostProcessLogicalForPhysical( this PhysicalCreationState state, CILMethodBase method )
   {

      var md = state.MetaData;
      var methodIdx = state.GetMethodDefOrMemberRefOrMethodSpec( method );
      var mDef = md.MethodDefinitions.TableContents[methodIdx.Index];
      mDef.Signature = state.CreateMethodDefSignature( method );

      if ( method.HasILMethodBody() )
      {
         mDef.IL = state.ProcessLogicalForPhysical( method.MethodIL );
      }

      state.AddToCustomAttributeTable( methodIdx, method );
      state.AddToSecurityTable( methodIdx, method );

      foreach ( var param in method.GetAllParameters() )
      {
         TableIndex paramIdx;
         if ( state.TryGetParamIndex( param, out paramIdx ) )
         {
            state.AddToCustomAttributeTable( paramIdx, param );
            state.AddToMarshalTable( paramIdx, param );
         }
      }

      if ( method.MethodKind == MethodKind.Method )
      {
         // Create generic arguments for the method
         var m = (CILMethod) method;
         state.ProcessLogicalForPhysicalGArgs( m );

         var typeDefIdx = state.GetTypeDefOrRefOrSpec( method.DeclaringType );
         md.MethodImplementations.TableContents.AddRange( m.OverriddenMethods.Select( om => new MethodImplementation()
         {
            Class = typeDefIdx,
            MethodBody = methodIdx,
            MethodDeclaration = state.GetMethodDefOrMemberRefOrMethodSpec( om.GetTrueGenericDefinition() )
         } ) );

         if ( !String.IsNullOrEmpty( m.PlatformInvokeName ) && !String.IsNullOrEmpty( m.PlatformInvokeModuleName ) )
         {
            md.MethodImplementationMaps.TableContents.Add( new MethodImplementationMap()
            {
               Attributes = m.PlatformInvokeAttributes,
               ImportName = m.PlatformInvokeName,
               ImportScope = state.GetModuleRef( m.PlatformInvokeModuleName ),
               MemberForwarded = methodIdx
            } );
         }
      }
   }

   private static void ProcessLogicalForPhysicalGArgs(
      this PhysicalCreationState state,
      CILElementWithGenericArguments<Object> element
      )
   {
      var md = state.MetaData;
      var gArgOwner = element is CILType ?
            state.GetTypeDefOrRefOrSpec( element as CILType ) :
            state.GetMethodDefOrMemberRefOrMethodSpec( element as CILMethod );
      foreach ( CILTypeParameter gArg in element.GenericArguments )
      {
         var gArgIndex = md.GetNextTableIndexFor( Tables.GenericParameter );

         md.GenericParameterDefinitions.TableContents.Add( new GenericParameterDefinition()
         {
            Attributes = gArg.Attributes,
            GenericParameterIndex = gArg.GenericParameterPosition,
            Name = gArg.Name,
            Owner = gArgOwner
         } );
         state.AddToCustomAttributeTable( gArgIndex, gArg );

         var constraints = md.GenericParameterConstraintDefinitions.TableContents;
         foreach ( var constraint in gArg.GenericParameterConstraints )
         {
            constraints.Add( new GenericParameterConstraintDefinition()
            {
               Owner = gArgIndex,
               Constraint = state.GetTypeDefOrRefOrSpec( constraint, true )
            } );
         }
      }
   }

   private static MethodILDefinition ProcessLogicalForPhysical(
      this PhysicalCreationState state,
      MethodIL logicalIL
      )
   {
      var physicalIL = new MethodILDefinition( logicalIL.ExceptionBlocks.Count(), logicalIL.OpCodeCount )
      {
         InitLocals = logicalIL.InitLocals,
         LocalsSignatureIndex = state.GetLocalsIndex( logicalIL )
      };

      var ilState = new PhysicalILCreationState( state, logicalIL, physicalIL );
      var byteOffsets = ilState.OpCodeInfoByteOffsets;
      var dynamicBranchInfos = ilState.DynamicOpCodeInfos;
      var pOpCodes = physicalIL.OpCodes;
      var branchCodeIndices = new List<Int32>();

      // I have a gut feeling that jumps fitting into SByte are much more common than the ones that would require longer, Int32 format
      // Therefore whenever encountering dynamic branch or leave, use short form as a guess
      // The costly algorithm to re-adjust byte offsets should arise less frequently that way
      var curByteOffset = 0;
      for ( var i = 0; i < logicalIL.OpCodeCount; ++i )
      {
         var lOpCode = logicalIL.GetOpCodeInfo( i );
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
               pOpCode = new OpCodeInfoWithToken( lm.Code, state.GetMethodDefOrMemberRefOrMethodSpec( lm.ReflectionObject, false, !lm.UseGenericDefinitionIfPossible ) );
               break;
            case OpCodeInfoKind.OperandCtorToken:
               var ct = (LogicalOpCodeInfoWithCtorToken) lOpCode;
               pOpCode = new OpCodeInfoWithToken( ct.Code, state.GetMethodDefOrMemberRefOrMethodSpec( ct.ReflectionObject, false, !ct.UseGenericDefinitionIfPossible ) );
               break;
            case OpCodeInfoKind.OperandMethodSigToken:
               var lms = (LogicalOpCodeInfoWithMethodSig) lOpCode;
               pOpCode = new OpCodeInfoWithToken( lms.Code, state.GetMethodSignatureToken( lms.ReflectionObject, lms.VarArgs, !lms.UseGenericDefinitionIfPossible ) );
               break;
            case OpCodeInfoKind.NormalOrVirtual:
               var lv = (LogicalOpCodeInfoForNormalOrVirtual) lOpCode;
               pOpCode = new OpCodeInfoWithToken( lv.ReflectionObject.Attributes.IsStatic() ? lv.NormalCode : lv.VirtualCode, state.GetMethodDefOrMemberRefOrMethodSpec( lv.ReflectionObject, false, true ) );
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
               dynamicBranchInfos.Add( pOpCodes.Count );
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
               dynamicBranchInfos.Add( pOpCodes.Count );
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

         byteOffsets[i] = curByteOffset;
         curByteOffset += pOpCode.GetTotalByteCount();
      }

      // Walk dynamic branch codes, and if offset is larger than SByte, then re-adjust as needed all the previous jumps that jump over this
      for ( var i = 0; i < dynamicBranchInfos.Count; ++i )
      {
         var opCodeOffset = dynamicBranchInfos[i];
         var codeInfo = (OpCodeInfoWithInt32) pOpCodes[opCodeOffset];
         var physicalOffset = ilState.TransformLogicalOffsetToPhysicalOffset( opCodeOffset, codeInfo.OpCode.GetTotalByteCount(), codeInfo.Operand );
         if ( !codeInfo.Operand.IsShortJump() )
         {
            // Have to use long form
            var newForm = ( (LogicalOpCodeInfoForBranchingControlFlow) logicalIL.GetOpCodeInfo( opCodeOffset ) ).LongForm;
            pOpCodes[opCodeOffset] = new OpCodeInfoWithInt32( newForm, codeInfo.Operand );

            // Fix byte offsets and recursively check all previous jumps that jump over this
            ilState.UpdateAllByteOffsetsFollowing( opCodeOffset, newForm.GetTotalByteCount() - codeInfo.OpCode.GetTotalByteCount() );
            ilState.AfterDynamicChangedToLongForm( opCodeOffset );
         }
      }

      // Then, walk through each branch code and fix logical absolute offset -> physical relative offset
      foreach ( var i in branchCodeIndices )
      {
         var codeInfo = pOpCodes[i];
         if ( codeInfo.InfoKind == OpCodeOperandKind.OperandSwitch )
         {
            var switchInfo = (OpCodeInfoWithSwitch) codeInfo;
            var switchByteCount = switchInfo.GetTotalByteCount();
            var targetList = switchInfo.Offsets;
            for ( var j = 0; j < targetList.Count; ++j )
            {
               targetList[j] = ilState.TransformLogicalOffsetToPhysicalOffset( i, switchByteCount, targetList[j] );
            }
         }
         else
         {
            var codeInfoBranch = (OpCodeInfoWithInt32) codeInfo;
            var physicalOffset = ilState.TransformLogicalOffsetToPhysicalOffset( i, codeInfo.OpCode.GetTotalByteCount(), codeInfoBranch.Operand );

            if ( codeInfoBranch.OpCode.OperandType == OperandType.ShortInlineBrTarget
               && !physicalOffset.IsShortJump()
               )
            {
               throw new InvalidOperationException( "Tried to use one-byte branch instruction for offset of amount " + physicalOffset + "." );
            }
            else
            {
               codeInfoBranch.Operand = physicalOffset;
            }
         }
      }

      // Once the byte offset are done, create exception block infos, and place them in correct order
      physicalIL.ExceptionBlocks.AddRange( logicalIL.ExceptionBlocks.Select( e => ilState.TransformLogicalExceptionBlockToPhysicalExceptionBlock( e ) ) );
      physicalIL.SortExceptionBlocks();



      return physicalIL;
   }

   private static Int32 TransformLogicalOffsetToPhysicalOffset(
      this PhysicalILCreationState state,
      Int32 currentLogicalOffset,
      Int32 currentCodeByteCount,
      Int32 targetLogicalOffset
      )
   {
      return state.OpCodeInfoByteOffsets[targetLogicalOffset] - ( state.OpCodeInfoByteOffsets[currentLogicalOffset] + currentCodeByteCount );
   }

   private static void AfterDynamicChangedToLongForm(
      this PhysicalILCreationState state,
      Int32 startingOpCodeIndex
      )
   {
      var dynIndices = state.DynamicOpCodeInfos;
      var currentOpCodeIndex = dynIndices[startingOpCodeIndex];
      var pOpCodes = state.PhysicalIL.OpCodes;
      for ( var idx = 0; idx < startingOpCodeIndex; ++idx )
      {
         var currentDynamicIndex = state.DynamicOpCodeInfos[idx];
         var dynamicJump = (LogicalOpCodeInfoForBranchingControlFlow) state.LogicalIL.GetOpCodeInfo( currentDynamicIndex );
         var codeInfo = (OpCodeInfoWithInt32) pOpCodes[currentDynamicIndex];
         if ( codeInfo.Operand > currentOpCodeIndex && dynamicJump.ShortForm == codeInfo.OpCode )
         {
            // Short jump over the changed offset, see if we need to change this as well
            var physicalOffset = state.TransformLogicalOffsetToPhysicalOffset( currentDynamicIndex, codeInfo.OpCode.GetTotalByteCount(), codeInfo.Operand );

            if ( !physicalOffset.IsShortJump() )
            {
               // Have to use long form
               var newForm = dynamicJump.LongForm;
               pOpCodes[currentDynamicIndex] = new OpCodeInfoWithInt32( newForm, codeInfo.Operand );
               // Modify all byte offsets following this.
               state.UpdateAllByteOffsetsFollowing( currentDynamicIndex, newForm.GetTotalByteCount() - dynamicJump.ShortForm.GetTotalByteCount() );
               // Re-check dynamic jumps between start and this.
               state.AfterDynamicChangedToLongForm( idx );
            }
         }
      }
   }

   private static Boolean IsShortJump( this Int32 jump )
   {
      return jump >= SByte.MinValue && jump <= SByte.MaxValue;
   }

   private static void UpdateAllByteOffsetsFollowing( this PhysicalILCreationState state, Int32 opCodeOffset, Int32 byteDelta )
   {
      var byteOffsets = state.OpCodeInfoByteOffsets;
      for ( var i = opCodeOffset + 1; i < byteOffsets.Length; ++i )
      {
         byteOffsets[i] += byteDelta;
      }
   }

   private static MethodExceptionBlock TransformLogicalExceptionBlockToPhysicalExceptionBlock(
      this PhysicalILCreationState state,
      ExceptionBlockInfo block
      )
   {
      var retVal = new MethodExceptionBlock()
      {
         BlockType = block.BlockType,
         ExceptionType = block.ExceptionType == null ? (TableIndex?) null : state.ModuleState.GetTypeDefOrRefOrSpec( block.ExceptionType, true ),
         FilterOffset = block.FilterOffset < 0 ? 0 : state.OpCodeInfoByteOffsets[block.FilterOffset],
         HandlerOffset = block.HandlerOffset < 0 ? 0 : state.OpCodeInfoByteOffsets[block.HandlerOffset],
         TryOffset = state.OpCodeInfoByteOffsets[block.TryOffset]
      };
      if ( block.HandlerOffset >= 0 )
      {
         retVal.HandlerLength = state.OpCodeInfoByteOffsets[block.HandlerOffset + block.HandlerLength] - retVal.HandlerOffset;
      }
      retVal.TryLength = state.OpCodeInfoByteOffsets[block.TryOffset + block.TryLength] - retVal.TryOffset;
      return retVal;
   }

   private static void PostProcessLogicalForPhysical(
      this PhysicalCreationState state,
      CILProperty property
      )
   {
      var typeDefIndex = state.GetTypeDefOrRefOrSpec( property.DeclaringType );
      var md = state.MetaData;

      // Add to PropertyMap
      var propIdx = md.GetNextTableIndexFor( Tables.Property );
      md.PropertyMaps.TableContents.Add( new PropertyMap()
      {
         Parent = typeDefIndex,
         PropertyList = propIdx
      } );

      // Add to Property
      md.PropertyDefinitions.TableContents.Add( new PropertyDefinition()
      {
         Attributes = property.Attributes,
         Name = property.Name,
         Signature = state.CreatePropertySignature( property )
      } );

      // Add to method semantics
      md.MethodSemantics.TableContents.AddRange( property.GetSemanticMethods().Select( method => new MethodSemantics()
      {
         Associaton = typeDefIndex,
         Attributes = method.Item1,
         Method = state.GetMethodDefOrMemberRefOrMethodSpec( method.Item2 )
      } ) );

      // Add to Constant
      if ( property.Attributes.HasDefault() )
      {
         state.AddToConstantTable( propIdx, property.ConstantValue );
      }

      // Add custom attributes
      state.AddToCustomAttributeTable( propIdx, property );
   }

   private static void PostProcessLogicalForPhysical(
      this PhysicalCreationState state,
      CILEvent evt
   )
   {
      var typeDefIndex = state.GetTypeDefOrRefOrSpec( evt.DeclaringType );
      var md = state.MetaData;

      // Add to EventMap
      var evtIdx = md.GetNextTableIndexFor( Tables.Event );
      md.EventMaps.TableContents.Add( new EventMap()
      {
         Parent = typeDefIndex,
         EventList = evtIdx
      } );

      // Add to Event
      md.EventDefinitions.TableContents.Add( new EventDefinition()
      {
         Attributes = evt.Attributes,
         EventType = state.GetTypeDefOrRefOrSpec( evt.EventHandlerType ),
         Name = evt.Name
      } );

      // Add to method semantics
      md.MethodSemantics.TableContents.AddRange( evt.GetSemanticMethods().Select( method => new MethodSemantics()
      {
         Associaton = typeDefIndex,
         Attributes = method.Item1,
         Method = state.GetMethodDefOrMemberRefOrMethodSpec( method.Item2 )
      } ) );

      // Add custom attributes
      state.AddToCustomAttributeTable( evtIdx, evt );
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
      sig.GenericArgumentCount = isGeneric ? m.GenericArguments.Count : 0;
      sig.SignatureStarter = method.CallingConvention.GetSignatureStarter( method.Attributes.IsStatic(), isGeneric );
      sig.ReturnType = m == null ?
         new ParameterSignature()
         {
            IsByRef = false,
            Type = SimpleTypeSignature.Void
         } :
         state.CreateParameterSignature( m.ReturnParameter );
      sig.Parameters.AddRange( method.Parameters.Select( p => state.CreateParameterSignature( p ) ) );
   }

   private static ParameterSignature CreateParameterSignature( this PhysicalCreationState state, CILParameter parameter )
   {
      var retVal = new ParameterSignature( parameter.CustomModifiers.Count )
      {
         IsByRef = parameter.ParameterType.IsByRef(),
         Type = state.CreateTypeSignature( parameter.ParameterType )
      };
      state.AddCustomMods( parameter.CustomModifiers, retVal.CustomModifiers );

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

   private static PropertySignature CreatePropertySignature( this PhysicalCreationState state, CILProperty property )
   {
      var indexParameters = property.GetIndexParameters();

      var retVal = new PropertySignature( property.CustomModifiers.Count, indexParameters.Count() )
      {
         HasThis = property.GetSemanticMethods().Any( m => !m.Item2.Attributes.IsStatic() ),
         PropertyType = state.CreateTypeSignature( property.GetPropertyType() )
      };
      state.AddCustomMods( property.CustomModifiers, retVal.CustomModifiers );
      retVal.Parameters.AddRange( indexParameters.Select( p => state.CreateParameterSignature( p ) ) );
      return retVal;
   }

   private static void AddToCustomAttributeTable( this PhysicalCreationState state, TableIndex parent, CILCustomAttributeContainer attributeContainer )
   {
      state.MetaData.CustomAttributeDefinitions.TableContents.AddRange( attributeContainer.CustomAttributeData.Select( ca => new CustomAttributeDefinition()
      {
         Parent = parent,
         Signature = state.CreateCASignature( ca ),
         Type = state.GetMethodDefOrMemberRefOrMethodSpec( ca.Constructor )
      } ) );
   }

   private static void AddToMarshalTable( this PhysicalCreationState state, TableIndex parent, CILElementWithMarshalingInfo element )
   {
      if ( element.MarshalingInformation != null )
      {
         state.MetaData.FieldMarshals.TableContents.Add( new FieldMarshal()
         {
            NativeType = state.CreatePhysicalMarshalingInfo( element.MarshalingInformation ),
            Parent = parent
         } );
      }
   }

   private static void AddToSecurityTable( this PhysicalCreationState state, TableIndex parent, CILElementWithSecurityInformation element )
   {
      state.MetaData.SecurityDefinitions.TableContents.AddRange( element.DeclarativeSecurity.Select( kvp =>
      {
         var action = kvp.Key;
         var permissionSets = kvp.Value;
         var retVal = new SecurityDefinition( permissionSets.Count )
         {
            Action = action,
            Parent = parent
         };
         retVal.PermissionSets.AddRange( permissionSets.Select( ps => state.CreatePhysicalSecurityInfo( ps ) ) );
         return retVal;
      } ) );
   }

   private static CustomAttributeSignature CreateCASignature( this PhysicalCreationState state, CILCustomAttribute attribute )
   {
      var retVal = new CustomAttributeSignature( attribute.ConstructorArguments.Count, attribute.NamedArguments.Count );
      retVal.TypedArguments.AddRange( attribute.ConstructorArguments.Select( arg => state.CreateCATypedArg( arg ) ) );
      retVal.NamedArguments.AddRange( attribute.NamedArguments.Select( arg => state.CreateCANamedArg( arg ) ) );
      return retVal;
   }

   private static CustomAttributeTypedArgument CreateCATypedArg( this PhysicalCreationState state, CILCustomAttributeTypedArgument arg )
   {
      var retVal = new CustomAttributeTypedArgument()
      {
         Type = state.CreateCAType( arg.ArgumentType )
      };

      var value = arg.Value;
      state.ProcessCATypedArgValue( retVal.Type, ref value );
      retVal.Value = value;
      return retVal;
   }

   private static Boolean ProcessCATypedArgValue( this PhysicalCreationState state, CustomAttributeArgumentType type, ref Object value )
   {
      var retVal = false;
      if ( value != null )
      {
         switch ( type.ArgumentTypeKind )
         {
            case CustomAttributeArgumentTypeKind.Array:
               type = ( (CustomAttributeArgumentTypeArray) type ).ArrayType;
               var array = (Array) value;
               for ( var i = 0; i < array.Length; ++i )
               {
                  var cur = array.GetValue( i );
                  if ( state.ProcessCATypedArgValue( type, ref cur ) )
                  {
                     array.SetValue( cur, i );
                  }
               }
               break;
            case CustomAttributeArgumentTypeKind.Simple:
               if ( ( (CustomAttributeArgumentTypeSimple) type ).SimpleType == SignatureElementTypes.Type )
               {
                  // Convert System.Type or CILType to string
                  CILType valueType;
                  if ( value is Type )
                  {
                     valueType = ( (Type) value ).NewWrapperAsType( state.LogicalModule.ReflectionContext );
                  }
                  else
                  {
                     valueType = value as CILType;
                  }

                  if ( valueType == null )
                  {
                     // If string was specified directly, just let it pass
                     if ( !( value is String ) )
                     {
                        throw new InvalidOperationException( "Custom attribute argument type was System.Type but the value was of type " + value.GetType() );
                     }
                  }
                  else
                  {
                     value = state.CreateCATypeString( valueType );
                  }
               }
               break;
         }
      }

      return retVal;
   }

   private static CustomAttributeNamedArgument CreateCANamedArg( this PhysicalCreationState state, CILCustomAttributeNamedArgument arg )
   {
      return new CustomAttributeNamedArgument()
      {
         IsField = arg.NamedMember is CILField,
         Name = arg.NamedMember.Name,
         Value = state.CreateCATypedArg( arg.TypedValue )
      };
   }

   private static CustomAttributeArgumentType CreateCAType( this PhysicalCreationState state, CILType type )
   {
      if ( type == null )
      {
         return CustomAttributeArgumentTypeSimple.String;
      }
      else if ( type.IsEnum() )
      {
         return new CustomAttributeArgumentTypeEnum()
         {
            TypeString = state.CreateCATypeString( type )
         };
      }
      else
      {
         switch ( type.TypeCode )
         {
            case CILTypeCode.Boolean:
               return CustomAttributeArgumentTypeSimple.Boolean;
            case CILTypeCode.Char:
               return CustomAttributeArgumentTypeSimple.Char;
            case CILTypeCode.SByte:
               return CustomAttributeArgumentTypeSimple.SByte;
            case CILTypeCode.Byte:
               return CustomAttributeArgumentTypeSimple.Byte;
            case CILTypeCode.Int16:
               return CustomAttributeArgumentTypeSimple.Int16;
            case CILTypeCode.UInt16:
               return CustomAttributeArgumentTypeSimple.UInt16;
            case CILTypeCode.Int32:
               return CustomAttributeArgumentTypeSimple.Int32;
            case CILTypeCode.UInt32:
               return CustomAttributeArgumentTypeSimple.UInt32;
            case CILTypeCode.Int64:
               return CustomAttributeArgumentTypeSimple.Int64;
            case CILTypeCode.UInt64:
               return CustomAttributeArgumentTypeSimple.UInt64;
            case CILTypeCode.Single:
               return CustomAttributeArgumentTypeSimple.Single;
            case CILTypeCode.Double:
               return CustomAttributeArgumentTypeSimple.Double;
            case CILTypeCode.String:
               return CustomAttributeArgumentTypeSimple.String;
            case CILTypeCode.Type:
               return CustomAttributeArgumentTypeSimple.Type;
            default:
               var valid = type.IsVectorArray();
               CILType elType = null;
               if ( valid )
               {
                  elType = type.ElementType as CILType;
                  valid = elType != null;
               }

               if ( valid )
               {
                  return new CustomAttributeArgumentTypeArray()
                  {
                     ArrayType = state.CreateCAType( elType )
                  };
               }
               else
               {
                  throw new InvalidOperationException( "The custom attribute argument type " + type + " is not valid." );
               }
         }
      }
   }

   private static String CreateCATypeString( this PhysicalCreationState state, CILType type )
   {
      return type == null ?
         null :
         ( type.Module.Assembly.Equals( state.LogicalModule.Assembly ) ?
               type.GetFullName() :
               type.GetAssemblyQualifiedName() // TODO maybe do some kind of "IsSystemLibrary" check?
         );
   }

   private static MarshalingInfo CreatePhysicalMarshalingInfo( this PhysicalCreationState state, LogicalMarshalingInfo marshalingInfo )
   {
      return new MarshalingInfo(
         marshalingInfo.Value,
         marshalingInfo.SafeArrayType,
         state.CreateCATypeString( marshalingInfo.SafeArrayUserDefinedType ),
         marshalingInfo.IIDParameterIndex,
         marshalingInfo.ArrayType,
         marshalingInfo.SizeParameterIndex,
         marshalingInfo.ConstSize,
         marshalingInfo.MarshalType ?? state.CreateCATypeString( marshalingInfo.MarshalTypeRef ),
         marshalingInfo.MarshalCookie );

   }

   private static SecurityInformation CreatePhysicalSecurityInfo( this PhysicalCreationState state, LogicalSecurityInformation securityInfo )
   {
      var retVal = new SecurityInformation( securityInfo.NamedArguments.Count )
      {
         SecurityAttributeType = state.CreateCATypeString( securityInfo.SecurityAttributeType )
      };
      retVal.NamedArguments.AddRange( securityInfo.NamedArguments.Select( arg => state.CreateCANamedArg( arg ) ) );
      return retVal;
   }

   private static LocalVariablesSignature CreateLocalsSignature( this PhysicalCreationState state, IList<LocalBuilder> locals )
   {
      var retVal = new LocalVariablesSignature( locals.Count );
      retVal.Locals.AddRange( locals.Select( lb => new LocalVariableSignature()
      {
         IsPinned = lb.IsPinned,
         IsByRef = lb.LocalType.IsByRef(),
         Type = state.CreateTypeSignature( lb.LocalType.IsByRef() ? lb.LocalType.GetElementType() : lb.LocalType )
      } ) );
      return retVal;
   }

}
