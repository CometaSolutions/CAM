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
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Structural;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static partial class E_CILStructural
{
   private sealed class StructuralCreationState
   {
      private readonly TypeDefinitionStructure[] _typeDefDescriptions;
      private readonly TypeReferenceStructure[] _typeRefDescriptions;
      private readonly TypeSpecificationStructure[] _typeSpecDescriptions;

      private readonly ModuleReferenceStructure[] _moduleRefs;
      private readonly FileReferenceStructure[] _fileRefs;
      private readonly AssemblyReferenceStructure[] _assemblyRefs;

      internal StructuralCreationState(
         CILMetaData md,
         List<TypeDefinitionStructure> topLevelTypes,
         List<ExportedTypeStructure> eTypeList
         )
      {
         // TypeDefs
         var nestedTypes = new Dictionary<Int32, ISet<Int32>>();
         var enclosingTypes = new Dictionary<Int32, Int32>();
         foreach ( var nc in md.NestedClassDefinitions.TableContents )
         {
            nestedTypes
               .GetOrAdd_NotThreadSafe( nc.EnclosingClass.Index, i => new HashSet<Int32>() )
               .Add( nc.NestedClass.Index );
            enclosingTypes[nc.NestedClass.Index] = nc.EnclosingClass.Index;
         }

         var tDefs = md.TypeDefinitions.TableContents;
         this._typeDefDescriptions = tDefs
            .Select( tDef => new TypeDefinitionStructure( tDef ) )
            .ToArray();
         for ( var i = 0; i < tDefs.Count; ++i )
         {
            if ( !enclosingTypes.ContainsKey( i ) )
            {
               topLevelTypes.Add( this._typeDefDescriptions[i] );
            }
         }

         foreach ( var kvp in nestedTypes )
         {
            this._typeDefDescriptions[kvp.Key].NestedTypes.AddRange( kvp.Value.Select( i => this._typeDefDescriptions[i] ) );
         }
         var typeDefInfosTopLevel = topLevelTypes.ToDictionary( t => Miscellaneous.CombineTypeAndNamespace( t.Name, t.Namespace ), t => t );

         // TypeRef
         this._moduleRefs = md.ModuleReferences.TableContents.Select( mRef => new ModuleReferenceStructure( mRef ) ).ToArray();
         this._assemblyRefs = md.AssemblyReferences.TableContents.Select( aRef => new AssemblyReferenceStructure( aRef ) ).ToArray();
         this._fileRefs = md.FileReferences.TableContents.Select( fRef => new FileReferenceStructure( fRef ) ).ToArray();
         var eTypes = md.ExportedTypes.TableContents;
         eTypeList.AddRange( eTypes.Select( e => new ExportedTypeStructure( e ) ) );
         var exportedTopLevelTypes = new Dictionary<String, ExportedTypeStructure>();
         for ( var i = 0; i < eTypes.Count; ++i )
         {
            var resScope = eTypes[i].Implementation;
            ExportedTypeResolutionScope resScopeInfo;
            var isTopLevel = true;
            switch ( resScope.Table )
            {
               case Tables.File:
                  resScopeInfo = new ExportedTypeResolutionScopeFile()
                  {
                     File = this._fileRefs[resScope.Index]
                  };
                  break;
               case Tables.AssemblyRef:
                  resScopeInfo = new ExportedTypeResolutionScopeAssemblyRef()
                  {
                     AssemblyRef = this._assemblyRefs[resScope.Index]
                  };
                  break;
               case Tables.ExportedType:
                  resScopeInfo = new ExportedTypeResolutionScopeNested()
                  {
                     EnclosingType = eTypeList[resScope.Index]
                  };
                  isTopLevel = false;
                  break;
               default:
                  resScopeInfo = null;
                  break;
            }
            var eType = eTypeList[i];
            if ( resScopeInfo != null )
            {
               eType.ResolutionScope = resScopeInfo;
            }

            if ( isTopLevel )
            {
               exportedTopLevelTypes.Add( Miscellaneous.CombineTypeAndNamespace( eType.Name, eType.Namespace ), eType );
            }

         }
         var tRefs = md.TypeReferences.TableContents;
         this._typeRefDescriptions = tRefs
            .Select( tRef => new TypeReferenceStructure( tRef ) )
            .ToArray();
         for ( var i = 0; i < tRefs.Count; ++i )
         {
            var tRef = tRefs[i];
            var resScopeNullable = tRef.ResolutionScope;
            var info = this._typeRefDescriptions[i];
            TypeRefeferenceResolutionScope resScopeInfo;
            ExportedTypeStructure eType;
            if ( resScopeNullable.HasValue )
            {
               var resScope = resScopeNullable.Value;
               switch ( resScope.Table )
               {
                  case Tables.TypeRef:
                     resScopeInfo = new TypeReferenceResolutionScopeNested()
                     {
                        EnclosingTypeRef = this._typeRefDescriptions[resScope.Index] as TypeReferenceStructure
                     };
                     break;
                  case Tables.Module:
                     resScopeInfo = new TypeReferenceResolutionScopeTypeDef()
                     {
                        TypeDef = typeDefInfosTopLevel[Miscellaneous.CombineTypeAndNamespace( tRef.Name, tRef.Namespace )]
                     };
                     break;
                  case Tables.ModuleRef:
                     resScopeInfo = new TypeReferenceResolutionScopeModuleRef()
                     {
                        ModuleRef = this._moduleRefs[resScope.Index]
                     };
                     break;
                  case Tables.AssemblyRef:
                     var aRef = md.AssemblyReferences.TableContents[resScope.Index];
                     resScopeInfo = new TypeReferenceResolutionScopeAssemblyRef()
                     {
                        AssemblyRef = this._assemblyRefs[resScope.Index]
                     };
                     break;
                  default:
                     resScopeInfo = null;
                     break;
               }
            }
            else if ( exportedTopLevelTypes.TryGetValue( Miscellaneous.CombineTypeAndNamespace( tRef.Name, tRef.Namespace ), out eType ) )
            {
               resScopeInfo = new TypeReferenceResolutionScopeExportedType()
               {
                  ExportedType = eType
               };
            }
            else
            {
               resScopeInfo = null;
            }

            if ( resScopeInfo != null )
            {
               info.ResolutionScope = resScopeInfo;
            }

         }

         // TypeSpecs
         var tSpecs = md.TypeSpecifications.TableContents;
         this._typeSpecDescriptions = tSpecs
            .Select( tSpec => new TypeSpecificationStructure() )
            .ToArray();
         for ( var i = 0; i < tSpecs.Count; ++i )
         {
            var tSpec = tSpecs[i];
            this._typeSpecDescriptions[i].Signature = (TypeStructureSignature) this.CreateStructureSignature( tSpec.Signature );
         }
      }

      public TypeDefinitionStructure[] TypeDefDescriptions
      {
         get
         {
            return this._typeDefDescriptions;
         }
      }

      public TypeReferenceStructure[] TypeRefDescriptions
      {
         get
         {
            return this._typeRefDescriptions;
         }
      }

      public TypeSpecificationStructure[] TypeSpecDescriptions
      {
         get
         {
            return this._typeSpecDescriptions;
         }
      }

      public ModuleReferenceStructure[] ModuleRefs
      {
         get
         {
            return this._moduleRefs;
         }
      }

      public FileReferenceStructure[] FileRefs
      {
         get
         {
            return this._fileRefs;
         }
      }

      public AssemblyReferenceStructure[] AssemblyRefs
      {
         get
         {
            return this._assemblyRefs;
         }
      }
   }

   public static AssemblyStructure CreateStructuralRepresentation( this CILMetaData md )
   {
      var retVal = new AssemblyStructure( md );
      retVal.Modules.Add( retVal.CreateStructuralRepresentation( md ) );
      return retVal;
   }

   public static ModuleStructure CreateStructuralRepresentation( this AssemblyStructure assembly, CILMetaData metaData )
   {
      var module = new ModuleStructure( metaData );
      var state = new StructuralCreationState( metaData, module.TopLevelTypeDefinitions, module.ExportedTypes );
      state.PopulateStructure( assembly, module, metaData );
      return module;
   }

   private static void PopulateStructure(
      this StructuralCreationState state,
      AssemblyStructure assembly,
      ModuleStructure module,
      CILMetaData md
      )
   {
      var tDefList = state.TypeDefDescriptions;

      // Fields
      var fDefs = md.FieldDefinitions.TableContents;
      var fDefList = new List<FieldStructure>( fDefs.Count );
      fDefList.AddRange( fDefs.Select( fDef => new FieldStructure( fDef ) ) );

      // Methods
      var mDefs = md.MethodDefinitions.TableContents;
      var mDefList = new List<MethodStructure>( mDefs.Count );
      mDefList.AddRange( mDefs.Select( mDef => new MethodStructure( mDef ) ) );

      // Parameters
      var paramDefs = md.ParameterDefinitions.TableContents;
      var paramDefList = new List<ParameterStructure>( paramDefs.Count );
      paramDefList.AddRange( paramDefs.Select( paramDef => new ParameterStructure( paramDef ) ) );

      // Properties
      var propDefs = md.PropertyDefinitions.TableContents;
      var propDefList = new List<PropertyStructure>( propDefs.Count );
      propDefList.AddRange( propDefs.Select( propDef =>
      {
         var prop = new PropertyStructure( propDef );
         prop.Signature = (PropertyStructureSignature) state.CreateStructureSignature( propDef.Signature );
         return prop;
      } ) );

      // Events
      var evtDefs = md.EventDefinitions.TableContents;
      var evtDefList = new List<EventStructure>( evtDefs.Count );
      evtDefList.AddRange( evtDefs.Select( evtDef =>
      {
         var evt = new EventStructure( evtDef );
         evt.EventType = state.FromTypeDefOrRefOrSpec( evtDef.EventType );
         return evt;
      } ) );

      // Process types
      var tDefs = md.TypeDefinitions.TableContents;
      for ( var i = 0; i < tDefs.Count; ++i )
      {
         var tDef = tDefs[i];
         var tDefDesc = tDefList[i];

         // Base type
         if ( tDef.BaseType.HasValue )
         {
            tDefDesc.BaseType = state.FromTypeDefOrRefOrSpec( tDef.BaseType.Value );
         }

         tDefDesc.Fields.AddRange( md.GetTypeFieldIndices( i ).Select( fIdx => fDefList[fIdx] ) );

         // Method parameter types and custom modifiers
         tDefDesc.Methods.AddRange( md.GetTypeMethodIndices( i ).Select( mIdx => mDefList[mIdx] ) );
      }

      // Fields
      for ( var i = 0; i < fDefs.Count; ++i )
      {
         fDefList[i].Signature = (FieldStructureSignature) state.CreateStructureSignature( fDefs[i].Signature );
      }

      // Methods
      for ( var i = 0; i < mDefs.Count; ++i )
      {
         var method = mDefList[i];
         method.Signature = (MethodDefinitionStructureSignature) state.CreateStructureSignature( mDefs[i].Signature );
         method.Parameters.AddRange( md.GetMethodParameterIndices( i ).Select( pIdx => paramDefList[pIdx] ) );
      }

      // Properties
      var pMaps = md.PropertyMaps.TableContents;
      for ( var i = 0; i < pMaps.Count; ++i )
      {
         tDefList[pMaps[i].Parent.Index].Properties.AddRange( md.GetTypePropertyIndices( i ).Select( propIdx => propDefList[propIdx] ) );
      }

      // Events
      var eMaps = md.EventMaps.TableContents;
      for ( var i = 0; i < eMaps.Count; ++i )
      {
         tDefList[eMaps[i].Parent.Index].Events.AddRange( md.GetTypeEventIndices( i ).Select( evtIdx => evtDefList[evtIdx] ) );
      }

      // Method Semantics
      foreach ( var semantic in md.MethodSemantics.TableContents )
      {
         StructureWithSemanticsMethods semInfo;
         var asso = semantic.Associaton;
         switch ( asso.Table )
         {
            case Tables.Property:
               semInfo = propDefList[asso.Index];
               break;
            case Tables.Event:
               semInfo = evtDefList[asso.Index];
               break;
            default:
               semInfo = null;
               break;
         }

         if ( semInfo != null )
         {
            semInfo.SemanticMethods.Add( new SemanticMethodInfo( semantic.Attributes, mDefList[semantic.Method.Index] ) );
         }
      }

      // Generic Parameters
      var gArgs = md.GenericParameterDefinitions.TableContents;
      var gArgsList = new List<GenericParameterStructure>( gArgs.Count );
      gArgsList.AddRange( gArgs.Select( gArg =>
      {
         var gArgInfo = new GenericParameterStructure( gArg );
         List<GenericParameterStructure> thisArgs;
         var owner = gArg.Owner;
         switch ( owner.Table )
         {
            case Tables.TypeDef:
               thisArgs = tDefList[owner.Index].GenericParameters;
               break;
            case Tables.MethodDef:
               thisArgs = mDefList[owner.Index].GenericParameters;
               break;
            default:
               thisArgs = null;
               break;
         }
         if ( thisArgs != null )
         {
            thisArgs.Add( gArgInfo );
         }
         return gArgInfo;
      } ) );

      // Generic parameter constraints
      var gArgConstraints = md.GenericParameterConstraintDefinitions.TableContents;
      var gArgConstraintList = new List<GenericParameterConstraintStructure>( gArgConstraints.Count );
      gArgConstraintList.AddRange( gArgConstraints.Select( gConstraint =>
      {
         var gConstraintInfo = new GenericParameterConstraintStructure()
         {
            Constraint = state.FromTypeDefOrRefOrSpec( gConstraint.Constraint )
         };
         gArgsList[gConstraint.Owner.Index].Constraints.Add( gConstraintInfo );
         return gConstraintInfo;
      } ) );

      // Class layout
      foreach ( var layout in md.ClassLayouts.TableContents )
      {
         tDefList[layout.Parent.Index].Layout = new LayoutInfo()
         {
            ClassSize = layout.ClassSize,
            PackingSize = layout.PackingSize
         };
      }

      // Constants
      foreach ( var constant in md.ConstantDefinitions.TableContents )
      {
         var parent = constant.Parent;
         var value = constant.Value;
         switch ( parent.Table )
         {
            case Tables.Field:
               fDefList[parent.Index].ConstantValue = new ConstantStructure( value );
               break;
            case Tables.Parameter:
               paramDefList[parent.Index].ConstantValue = new ConstantStructure( value );
               break;
            case Tables.Property:
               propDefList[parent.Index].ConstantValue = new ConstantStructure( value );
               break;
         }
      }

      // Field layouts 
      foreach ( var layout in md.FieldLayouts.TableContents )
      {
         fDefList[layout.Field.Index].FieldOffset = layout.Offset;
      }

      // Field marshals
      foreach ( var marshal in md.FieldMarshals.TableContents )
      {
         var parent = marshal.Parent;
         switch ( parent.Table )
         {
            case Tables.Field:
               fDefList[parent.Index].MarshalingInfo = marshal.NativeType;
               break;
            case Tables.Parameter:
               paramDefList[parent.Index].MarshalingInfo = marshal.NativeType;
               break;
         }
      }

      // Field RVAs
      foreach ( var rva in md.FieldRVAs.TableContents )
      {
         fDefList[rva.Field.Index].FieldData = rva.Data;
      }
      // Impl maps
      foreach ( var impl in md.MethodImplementationMaps.TableContents )
      {
         var parent = impl.MemberForwarded;
         var invokeInfo = new PInvokeInfo()
         {
            Attributes = impl.Attributes,
            PlatformInvokeName = impl.ImportName,
            PlatformInvokeModule = state.ModuleRefs[impl.ImportScope.Index]
         };

         switch ( parent.Table )
         {
            case Tables.MethodDef:
               mDefList[parent.Index].PInvokeInfo = invokeInfo;
               break;
            case Tables.Field:
               fDefList[parent.Index].PInvokeInfo = invokeInfo;
               break;
         }
      }

      // Interface Impls
      var interfaces = md.InterfaceImplementations.TableContents;
      var interfaceList = new List<InterfaceImplStructure>( interfaces.Count );
      interfaceList.AddRange( interfaces.Select( iFace =>
      {
         var iFaceInfo = new InterfaceImplStructure()
         {
            InterfaceType = state.FromTypeDefOrRefOrSpec( iFace.Interface )
         };
         tDefList[iFace.Class.Index].ImplementedInterfaces.Add( iFaceInfo );
         return iFaceInfo;
      } ) );

      // DeclSecurity
      var security = md.SecurityDefinitions.TableContents;
      var securityList = new List<SecurityStructure>( security.Count );
      securityList.AddRange( security.Select( sec =>
      {
         var secInfo = new SecurityStructure( sec );
         var parent = sec.Parent;
         switch ( parent.Table )
         {
            case Tables.TypeDef:
               tDefList[parent.Index].SecurityInfo.Add( secInfo );
               break;
            case Tables.MethodDef:
               mDefList[parent.Index].SecurityInfo.Add( secInfo );
               break;
            case Tables.Assembly:
               assembly.SecurityInfo.Add( secInfo );
               break;
         }

         return secInfo;
      } ) );

      // ManifestResource
      var resources = md.ManifestResources.TableContents;
      var resourceList = module.ManifestResources;
      resourceList.AddRange( resources.Select( res =>
      {
         var resourceInfo = new ManifestResourceStructure( res );
         var implNullable = res.Implementation;
         ManifestResourceStructureData data;
         if ( implNullable.HasValue )
         {
            var impl = implNullable.Value;
            switch ( impl.Table )
            {
               case Tables.File:
                  data = new ManifestResourceStrucureDataFile()
                  {
                     FileReference = state.FileRefs[impl.Index]
                  };
                  break;
               case Tables.AssemblyRef:
                  data = new ManifestResourceStructureDataAssemblyReference()
                  {
                     AssemblyRef = state.AssemblyRefs[impl.Index]
                  };
                  break;
               default:
                  data = null;
                  break;
            }
         }
         else
         {
            data = new ManifestResourceStructureDataEmbedded()
            {
               Data = res.DataInCurrentFile.CreateBlockCopy()
            };
         }
         if ( data != null )
         {
            resourceInfo.ManifestData = data;
         }

         return resourceInfo;
      } ) );

      // MemberRefs
      var memberRefs = md.MemberReferences.TableContents;
      var memberRefList = new List<MemberReferenceStructure>( memberRefs.Count );
      memberRefList.AddRange( memberRefs.Select( mRef =>
      {
         var mRefInfo = new MemberReferenceStructure( mRef );
         mRefInfo.Signature = state.CreateStructureSignature( mRef.Signature );
         var parent = mRef.DeclaringType;
         MemberReferenceParent parentInfo;
         switch ( parent.Table )
         {
            case Tables.MethodDef:
               parentInfo = new MemberReferenceParentMethodDef()
               {
                  Method = mDefList[parent.Index]
               };
               break;
            case Tables.ModuleRef:
               parentInfo = new MemberReferenceParentModuleRef()
               {
                  ModuleRef = state.ModuleRefs[parent.Index]
               };
               break;
            case Tables.TypeDef:
            case Tables.TypeRef:
            case Tables.TypeSpec:
               parentInfo = new MemberReferenceParentType()
               {
                  Type = state.FromTypeDefOrRefOrSpec( parent )
               };
               break;
            default:
               parentInfo = null;
               break;
         }

         if ( parentInfo != null )
         {
            mRefInfo.Parent = parentInfo;
         }
         return mRefInfo;
      } ) );
      // MethodImpl
      foreach ( var impl in md.MethodImplementations.TableContents )
      {
         tDefList[impl.Class.Index].OverriddenMethods.Add( new OverriddenMethodInfo( FromMethodDefOrMemberRef( mDefList, memberRefList, impl.MethodBody ), FromMethodDefOrMemberRef( mDefList, memberRefList, impl.MethodDeclaration ) ) );
      }
      // StandaloneSig
      var standaloneSigs = md.StandaloneSignatures.TableContents;
      var standaloneSigList = new List<StandaloneSignatureStructure>( standaloneSigs.Count );
      standaloneSigList.AddRange( standaloneSigs.Select( sig => new StandaloneSignatureStructure()
      {
         Signature = state.CreateStructureSignature( sig.Signature )
      } ) );
      // MethodSpec
      var methodSpecs = md.MethodSpecifications.TableContents;
      var methodSpecList = new List<MethodSpecificationStructure>( methodSpecs.Count );
      methodSpecList.AddRange( methodSpecs.Select( mSpec => new MethodSpecificationStructure()
      {
         Signature = (GenericMethodStructureSignature) state.CreateStructureSignature( mSpec.Signature ),
         Method = FromMethodDefOrMemberRef( mDefList, memberRefList, mSpec.Method )
      } ) );
      // Custom Attributes
      foreach ( var ca in md.CustomAttributeDefinitions.TableContents )
      {
         var parent = ca.Parent;
         StructureWithCustomAttributes parentInfo;
         switch ( parent.Table )
         {
            case Tables.TypeDef:
               parentInfo = state.TypeDefDescriptions[parent.Index];
               break;
            case Tables.ExportedType:
               parentInfo = module.ExportedTypes[parent.Index];
               break;
            case Tables.MethodDef:
               parentInfo = mDefList[parent.Index];
               break;
            case Tables.Field:
               parentInfo = fDefList[parent.Index];
               break;
            case Tables.Parameter:
               parentInfo = paramDefList[parent.Index];
               break;
            case Tables.Module:
               parentInfo = module;
               break;
            case Tables.Property:
               parentInfo = propDefList[parent.Index];
               break;
            case Tables.Event:
               parentInfo = evtDefList[parent.Index];
               break;
            case Tables.Assembly:
               parentInfo = assembly;
               break;
            case Tables.GenericParameter:
               parentInfo = gArgsList[parent.Index];
               break;
            case Tables.TypeRef:
               parentInfo = state.TypeRefDescriptions[parent.Index];
               break;
            case Tables.InterfaceImpl:
               parentInfo = interfaceList[parent.Index];
               break;
            case Tables.MemberRef:
               parentInfo = memberRefList[parent.Index];
               break;
            case Tables.DeclSecurity:
               parentInfo = securityList[parent.Index];
               break;
            case Tables.StandaloneSignature:
               parentInfo = standaloneSigList[parent.Index];
               break;
            case Tables.ModuleRef:
               parentInfo = state.ModuleRefs[parent.Index];
               break;
            case Tables.TypeSpec:
               parentInfo = state.TypeSpecDescriptions[parent.Index];
               break;
            case Tables.AssemblyRef:
               parentInfo = state.AssemblyRefs[parent.Index];
               break;
            case Tables.File:
               parentInfo = state.FileRefs[parent.Index];
               break;
            case Tables.ManifestResource:
               parentInfo = resourceList[parent.Index];
               break;
            case Tables.GenericParameterConstraint:
               parentInfo = gArgConstraintList[parent.Index];
               break;
            case Tables.MethodSpec:
               parentInfo = methodSpecList[parent.Index];
               break;
            default:
               parentInfo = null;
               break;
         }
         parentInfo.CustomAttributes.Add( new CustomAttributeStructure()
         {
            Constructor = FromMethodDefOrMemberRef( mDefList, memberRefList, ca.Type ),
            Signature = ca.Signature // TODO clone
         } );
      }

      // IL
      for ( var i = 0; i < mDefs.Count; ++i )
      {
         var il = mDefs[i].IL;
         if ( il != null )
         {
            var ilStructure = new MethodILStructureInfo( il.ExceptionBlocks.Count, il.OpCodes.Count )
            {
               InitLocals = il.InitLocals,
               MaxStackSize = il.MaxStackSize,
               Locals = il.LocalsSignatureIndex.HasValue ? standaloneSigList[il.LocalsSignatureIndex.Value.Index] : null
            };
            mDefList[i].IL = ilStructure;
            ilStructure.ExceptionBlocks.AddRange( il.ExceptionBlocks.Select( e => new MethodExceptionBlockStructure()
            {
               BlockType = e.BlockType,
               TryOffset = e.TryOffset,
               TryLength = e.TryLength,
               HandlerOffset = e.HandlerOffset,
               HandlerLength = e.HandlerLength,
               FilterOffset = e.FilterOffset,
               ExceptionType = e.ExceptionType.HasValue ? state.FromTypeDefOrRefOrSpec( e.ExceptionType.Value ) : null
            } ) );
            ilStructure.OpCodes.AddRange( il.OpCodes.Select<OpCodeInfo, OpCodeStructure>( o =>
            {
               switch ( o.InfoKind )
               {
                  case OpCodeOperandKind.OperandNone:
                     return OpCodeStructureSimple.GetInstanceFor( o.OpCode.Value );
                  case OpCodeOperandKind.OperandToken:
                     return new OpCodeStructureWithReference()
                     {
                        OpCode = o.OpCode,
                        Structure = state.FromILToken( fDefList, mDefList, memberRefList, standaloneSigList, methodSpecList, ( (OpCodeInfoWithToken) o ).Operand )
                     };
                  default:
                     return new OpCodeStructureWrapper()
                     {
                        PhysicalOpCode = o // TODO Clone
                     };
               }
            } ) );
         }
      }
   }

   private static AbstractStructureSignature CreateStructureSignature( this StructuralCreationState state, AbstractSignature signature )
   {
      if ( signature == null )
      {
         return null;
      }
      else
      {
         switch ( signature.SignatureKind )
         {
            case SignatureKind.Field:
               return state.CreateFieldStructureSignature( (FieldSignature) signature );
            case SignatureKind.GenericMethodInstantiation:
               return state.CreateGenericMethodStructureSignature( (GenericMethodSignature) signature );
            case SignatureKind.LocalVariables:
               return state.CreateLocalsStructureSignature( (LocalVariablesSignature) signature );
            case SignatureKind.MethodDefinition:
               return state.CreateMethodDefStructureSignature( (MethodDefinitionSignature) signature );
            case SignatureKind.MethodReference:
               return state.CreateMethodRefStructureSignature( (MethodReferenceSignature) signature );
            case SignatureKind.Property:
               return state.CreatePropertyStructureSignature( (PropertySignature) signature );
            case SignatureKind.Type:
               return state.CreateTypeStructureSignature( (TypeSignature) signature );
            case SignatureKind.RawSignature:
               throw new NotSupportedException( "Raw signatures are not supported in structural layer." );
            default:
               throw new InvalidOperationException( "Invalid physical signature kind: " + signature.SignatureKind + "." );
         }
      }
   }

   private static AbstractTypeStructure FromTypeDefOrRefOrSpec( this StructuralCreationState moduleInfo, TableIndex index )
   {
      switch ( index.Table )
      {
         case Tables.TypeDef:
            return moduleInfo.TypeDefDescriptions[index.Index];
         case Tables.TypeRef:
            return moduleInfo.TypeRefDescriptions[index.Index];
         case Tables.TypeSpec:
            return moduleInfo.TypeSpecDescriptions[index.Index];
         default:
            throw new InvalidOperationException( "Unsupported TypeDef/Ref/Spec: " + index + "." );
      }
   }

   private static MethodDefOrMemberRefStructure FromMethodDefOrMemberRef( List<MethodStructure> mDefList, List<MemberReferenceStructure> mRefList, TableIndex index )
   {
      switch ( index.Table )
      {
         case Tables.MethodDef:
            return mDefList[index.Index];
         case Tables.MemberRef:
            return mRefList[index.Index];
         default:
            return null;
      }
   }

   private static StructurePresentInIL FromILToken( this StructuralCreationState state, List<FieldStructure> fDefList, List<MethodStructure> mDefList, List<MemberReferenceStructure> mRefList, List<StandaloneSignatureStructure> standaloneSigList, List<MethodSpecificationStructure> mSpecList, TableIndex token )
   {
      switch ( token.Table )
      {
         case Tables.TypeDef:
            return state.TypeDefDescriptions[token.Index];
         case Tables.TypeRef:
            return state.TypeRefDescriptions[token.Index];
         case Tables.TypeSpec:
            return state.TypeSpecDescriptions[token.Index];
         case Tables.Field:
            return fDefList[token.Index];
         case Tables.MethodDef:
            return mDefList[token.Index];
         case Tables.MemberRef:
            return mRefList[token.Index];
         case Tables.MethodSpec:
            return mSpecList[token.Index];
         case Tables.StandaloneSignature:
            return standaloneSigList[token.Index];
         default:
            throw new InvalidOperationException( "Invalid IL token: " + token + "." );
      }
   }

   private static TypeStructureSignature CreateTypeStructureSignature( this StructuralCreationState state, TypeSignature sig )
   {
      TypeStructureSignature retVal;
      switch ( sig.TypeSignatureKind )
      {
         case TypeSignatureKind.ClassOrValue:
            var clazz = (ClassOrValueTypeSignature) sig;
            var clazzClone = new ClassOrValueTypeStructureSignature( clazz.GenericArguments.Count )
            {
               IsClass = clazz.IsClass,
               Type = state.FromTypeDefOrRefOrSpec( clazz.Type )
            };
            clazzClone.GenericArguments.AddRange( clazz.GenericArguments.Select( gArg => state.CreateTypeStructureSignature( gArg ) ) );
            retVal = clazzClone;
            break;
         case TypeSignatureKind.ComplexArray:
            var cArray = (ComplexArrayTypeSignature) sig;
            var cClone = new ComplexArrayTypeStructureSignature( cArray.Sizes.Count, cArray.LowerBounds.Count )
            {
               Rank = cArray.Rank,
               ArrayType = state.CreateTypeStructureSignature( cArray.ArrayType )
            };
            cClone.LowerBounds.AddRange( cArray.LowerBounds );
            cClone.Sizes.AddRange( cArray.Sizes );
            retVal = cClone;
            break;
         case TypeSignatureKind.FunctionPointer:
            retVal = new FunctionPointerTypeStructureSignature()
            {
               MethodSignature = state.CreateMethodRefStructureSignature( ( (FunctionPointerTypeSignature) sig ).MethodSignature )
            };
            break;
         case TypeSignatureKind.Pointer:
            var ptr = (PointerTypeSignature) sig;
            var ptrClone = new PointerTypeStructureSignature( ptr.CustomModifiers.Count )
            {
               PointerType = state.CreateTypeStructureSignature( ptr.PointerType )
            };
            state.TransformCustomModsToStructural( ptr.CustomModifiers, ptrClone.CustomModifiers );
            retVal = ptrClone;
            break;
         case TypeSignatureKind.GenericParameter:
            var gParam = (GenericParameterTypeSignature) sig;
            retVal = new GenericParameterTypeStructureSignature()
            {
               GenericParameterIndex = gParam.GenericParameterIndex,
               IsTypeParameter = gParam.IsTypeParameter,
            };
            break;
         case TypeSignatureKind.Simple:
            retVal = SimpleTypeStructureSignature.GetByElement( ( (SimpleTypeSignature) sig ).SimpleType );
            break;
         case TypeSignatureKind.SimpleArray:
            var array = (SimpleArrayTypeSignature) sig;
            var clone = new SimpleArrayTypeStructureSignature( array.CustomModifiers.Count )
            {
               ArrayType = state.CreateTypeStructureSignature( array.ArrayType )
            };
            state.TransformCustomModsToStructural( array.CustomModifiers, clone.CustomModifiers );
            retVal = clone;
            break;
         default:
            throw new NotSupportedException( "Invalid type signature kind: " + sig.TypeSignatureKind );
      }

      return retVal;
   }

   private static void PopulateAbstractMethodStructureSignature( this StructuralCreationState state, AbstractMethodSignature original, AbstractMethodStructureSignature clone )
   {
      clone.GenericArgumentCount = original.GenericArgumentCount;
      clone.SignatureStarter = original.SignatureStarter;
      clone.ReturnType = state.CreateParameterStructureSignature( original.ReturnType );
      clone.Parameters.AddRange( original.Parameters.Select( p => state.CreateParameterStructureSignature( p ) ) );
   }

   private static MethodReferenceStructureSignature CreateMethodRefStructureSignature( this StructuralCreationState state, MethodReferenceSignature methodRef )
   {
      var retVal = new MethodReferenceStructureSignature( methodRef.Parameters.Count, methodRef.VarArgsParameters.Count );
      state.PopulateAbstractMethodStructureSignature( methodRef, retVal );
      retVal.VarArgsParameters.AddRange( methodRef.VarArgsParameters.Select( p => state.CreateParameterStructureSignature( p ) ) );
      return retVal;
   }

   private static MethodDefinitionStructureSignature CreateMethodDefStructureSignature( this StructuralCreationState state, MethodDefinitionSignature methodDef )
   {
      var retVal = new MethodDefinitionStructureSignature( methodDef.Parameters.Count );
      state.PopulateAbstractMethodStructureSignature( methodDef, retVal );
      return retVal;
   }

   private static ParameterStructureSignature CreateParameterStructureSignature( this StructuralCreationState state, ParameterSignature paramSig )
   {
      var retVal = new ParameterStructureSignature( paramSig.CustomModifiers.Count )
      {
         IsByRef = paramSig.IsByRef,
         Type = state.CreateTypeStructureSignature( paramSig.Type )
      };
      state.TransformCustomModsToStructural( paramSig.CustomModifiers, retVal.CustomModifiers );
      return retVal;
   }

   private static GenericMethodStructureSignature CreateGenericMethodStructureSignature( this StructuralCreationState state, GenericMethodSignature gSig )
   {
      var retVal = new GenericMethodStructureSignature( gSig.GenericArguments.Count );
      retVal.GenericArguments.AddRange( gSig.GenericArguments.Select( gArg => state.CreateTypeStructureSignature( gArg ) ) );
      return retVal;
   }

   private static FieldStructureSignature CreateFieldStructureSignature( this StructuralCreationState state, FieldSignature sig )
   {
      var retVal = new FieldStructureSignature( sig.CustomModifiers.Count );
      retVal.Type = state.CreateTypeStructureSignature( sig.Type );
      state.TransformCustomModsToStructural( sig.CustomModifiers, retVal.CustomModifiers );
      return retVal;
   }

   private static LocalVariablesStructureSignature CreateLocalsStructureSignature( this StructuralCreationState state, LocalVariablesSignature locals )
   {
      var retVal = new LocalVariablesStructureSignature( locals.Locals.Count );
      retVal.Locals.AddRange( locals.Locals.Select( l => state.CreateLocalStructureSignature( l ) ) );
      return retVal;
   }

   private static LocalVariableStructureSignature CreateLocalStructureSignature( this StructuralCreationState state, LocalVariableSignature local )
   {
      var retVal = new LocalVariableStructureSignature( local.CustomModifiers.Count )
      {
         IsByRef = local.IsByRef,
         IsPinned = local.IsPinned,
         Type = state.CreateTypeStructureSignature( local.Type )
      };
      state.TransformCustomModsToStructural( local.CustomModifiers, retVal.CustomModifiers );
      return retVal;
   }

   private static PropertyStructureSignature CreatePropertyStructureSignature( this StructuralCreationState state, PropertySignature sig )
   {
      var retVal = new PropertyStructureSignature( sig.CustomModifiers.Count, sig.Parameters.Count )
      {
         HasThis = sig.HasThis,
         PropertyType = state.CreateTypeStructureSignature( sig.PropertyType )
      };
      state.TransformCustomModsToStructural( sig.CustomModifiers, retVal.CustomModifiers );
      retVal.Parameters.AddRange( sig.Parameters.Select( p => state.CreateParameterStructureSignature( p ) ) );
      return retVal;
   }

   private static void TransformCustomModsToStructural( this StructuralCreationState state, List<CustomModifierSignature> original, List<CustomModifierStructureSignature> newMods )
   {
      newMods.AddRange( original.Select( cm => new CustomModifierStructureSignature()
      {
         IsOptional = cm.IsOptional,
         CustomModifierType = state.FromTypeDefOrRefOrSpec( cm.CustomModifierType )
      } ) );
   }
}