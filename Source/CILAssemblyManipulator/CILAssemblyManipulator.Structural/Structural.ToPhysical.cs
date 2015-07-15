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
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static partial class E_CILStructural
{
   private sealed class TableIndexTracker<T>
      where T : class
   {
      private readonly Tables _table;
      private readonly IDictionary<T, Int32> _dic;
      private readonly Action<T, Int32> _afterAddition;

      internal TableIndexTracker( Tables table, Action<T, Int32> afterAddition )
      {
         this._table = table;
         this._dic = new Dictionary<T, Int32>( ReferenceEqualityComparer<T>.ReferenceBasedComparer );
         this._afterAddition = afterAddition;
      }

      public TableIndex Add( T obj )
      {
         if ( obj == null )
         {
            throw new InvalidOperationException( "Trying to add null to table " + this._table + "." );
         }

         var idx = this._dic.Count;
         this._dic.Add( obj, idx );
         this._afterAddition( obj, idx );
         return new TableIndex( this._table, idx );
      }

      public TableIndex Get( T obj )
      {
         TableIndex retVal;
         if ( !this.TryGet( obj, out retVal ) )
         {
            throw new InvalidOperationException( "The " + this._table + " object " + obj + " was not part of this module." );
         }
         return retVal;
      }

      public TableIndex GetOrAdd( T obj )
      {
         Int32 idx;
         return this._dic.TryGetValue( obj, out idx ) ?
            new TableIndex( this._table, idx ) :
            this.Add( obj );
      }

      public Boolean TryGet( T obj, out TableIndex index )
      {
         if ( obj == null )
         {
            throw new InvalidOperationException( "Null reference to table " + this._table + "." );
         }
         Int32 idx;
         var retVal = this._dic.TryGetValue( obj, out idx );
         index = retVal ? new TableIndex( this._table, idx ) : default( TableIndex );
         return retVal;
      }

      public Int32 Count
      {
         get
         {
            return this._dic.Count;
         }
      }

      public IDictionary<T, Int32> Dictionary
      {
         get
         {
            return this._dic;
         }
      }
   }
   private sealed class PhysicalCreationState
   {
      private readonly CILMetaData _md;
      private readonly TableIndexTracker<TypeDefinitionStructure> _typeDefs;
      private readonly TableIndexTracker<FieldStructure> _fieldDefs;
      private readonly TableIndexTracker<MethodStructure> _methodDefs;
      private readonly TableIndexTracker<ParameterStructure> _paramDefs;
      private readonly TableIndexTracker<TypeReferenceStructure> _typeRefs;
      private readonly TableIndexTracker<TypeSpecificationStructure> _typeSpecs;
      private readonly TableIndexTracker<AssemblyReferenceStructure> _assemblyRefs;
      private readonly TableIndexTracker<ModuleReferenceStructure> _moduleRefs;
      private readonly TableIndexTracker<MemberReferenceStructure> _memberRefs;
      private readonly TableIndexTracker<MethodSpecificationStructure> _methodSpecs;
      private readonly TableIndexTracker<StandaloneSignatureStructure> _standaloneSignatures;

      internal PhysicalCreationState( CILMetaData md, ModuleStructure module )
      {
         this._md = md;
         this._typeDefs = new TableIndexTracker<TypeDefinitionStructure>( Tables.TypeDef, ( tDef, idx ) =>
            md.TypeDefinitions.TableContents.Add( new TypeDefinition()
            {
               Attributes = tDef.Attributes,
               FieldList = new TableIndex( Tables.Field, this._fieldDefs.Count ),
               MethodList = new TableIndex( Tables.MethodDef, this._methodDefs.Count ),
               Name = tDef.Name,
               Namespace = tDef.Namespace
            } ) );
         this._fieldDefs = new TableIndexTracker<FieldStructure>( Tables.Field, ( fDef, idx ) =>
            md.FieldDefinitions.TableContents.Add( new FieldDefinition()
            {
               Attributes = fDef.Attributes,
               Name = fDef.Name
            } ) );
         this._methodDefs = new TableIndexTracker<MethodStructure>( Tables.MethodDef, ( mDef, idx ) =>
            md.MethodDefinitions.TableContents.Add( new MethodDefinition()
            {
               Attributes = mDef.Attributes,
               ImplementationAttributes = mDef.ImplementationAttributes,
               Name = mDef.Name,
               ParameterList = new TableIndex( Tables.Parameter, this._paramDefs.Count )
            } ) );
         this._paramDefs = new TableIndexTracker<ParameterStructure>( Tables.Parameter, ( pDef, idx ) =>
            md.ParameterDefinitions.TableContents.Add( new ParameterDefinition()
            {
               Attributes = pDef.Attributes,
               Name = pDef.Name,
               Sequence = pDef.Sequence
            } ) );
         this._typeRefs = new TableIndexTracker<TypeReferenceStructure>( Tables.TypeRef, ( tRef, idx ) =>
         {
            var tRefPhysical = new TypeReference()
            {
               Name = tRef.Name,
               Namespace = tRef.Namespace
            };
            md.TypeReferences.TableContents.Add( tRefPhysical );

            var resScope = tRef.ResolutionScope;
            if ( resScope == null )
            {
               throw new InvalidOperationException( "Missing resolution scope from type reference " + tRef + "." );
            }
            else
            {
               TableIndex? resScopeIdx;
               switch ( resScope.ResolutionScopeKind )
               {
                  case TypeRefResolutionScopeKind.AssemblyRef:
                     resScopeIdx = this._assemblyRefs.GetOrAdd( ( (TypeReferenceResolutionScopeAssemblyRef) resScope ).AssemblyRef );
                     break;
                  case TypeRefResolutionScopeKind.Nested:
                     resScopeIdx = this._typeRefs.GetOrAdd( ( (TypeReferenceResolutionScopeNested) resScope ).EnclosingTypeRef );
                     break;
                  case TypeRefResolutionScopeKind.ModuleRef:
                     resScopeIdx = this._moduleRefs.GetOrAdd( ( (TypeReferenceResolutionScopeModuleRef) resScope ).ModuleRef );
                     break;
                  case TypeRefResolutionScopeKind.ExportedType:
                     resScopeIdx = null;
                     break;
                  case TypeRefResolutionScopeKind.TypeDef:
                     resScopeIdx = this._typeDefs.GetOrAdd( ( (TypeReferenceResolutionScopeTypeDef) resScope ).TypeDef );
                     break;
                  default:
                     throw new InvalidOperationException( "Invalid type reference resolution scope kind: " + resScope.ResolutionScopeKind + "." );
               }
               tRefPhysical.ResolutionScope = resScopeIdx;
            }

         } );

         this._typeSpecs = new TableIndexTracker<TypeSpecificationStructure>( Tables.TypeSpec, ( tSpec, idx ) =>
            md.TypeSpecifications.TableContents.Add( new TypeSpecification()
            {
               Signature = this.CreatePhysicalTypeSignature( tSpec.Signature )
            } ) );

         this._assemblyRefs = new TableIndexTracker<AssemblyReferenceStructure>( Tables.AssemblyRef, ( aRef, idx ) =>
         {
            var aRefPhysical = new AssemblyReference()
            {
               Attributes = aRef.Attributes,
               HashValue = aRef.HashValue
            };
            aRef.AssemblyRef.DeepCopyContentsTo( aRefPhysical.AssemblyInformation );
            md.AssemblyReferences.TableContents.Add( aRefPhysical );
         } );

         this._moduleRefs = new TableIndexTracker<ModuleReferenceStructure>( Tables.ModuleRef, ( mRef, idx ) =>
            md.ModuleReferences.TableContents.Add( new ModuleReference()
            {
               ModuleName = mRef.ModuleName
            } ) );

         this._memberRefs = new TableIndexTracker<MemberReferenceStructure>( Tables.MemberRef, ( mRef, idx ) =>
         {
            var mRefPhysical = new MemberReference()
            {
               Name = mRef.Name,
               Signature = this.CreatePhysicalSignature( mRef.Signature )
            };
            md.MemberReferences.TableContents.Add( mRefPhysical );
            var resScope = mRef.Parent;
            if ( resScope == null )
            {
               throw new InvalidOperationException( "Missing resolution scope from member reference " + mRef + "." );
            }
            else
            {
               TableIndex resScopeIdx;
               switch ( resScope.MemberReferenceParentKind )
               {
                  case MemberReferenceParentKind.Type:
                     resScopeIdx = this.GetTypeDefOrRefOrSpec( ( (MemberReferenceParentType) resScope ).Type );
                     break;
                  case MemberReferenceParentKind.ModuleRef:
                     resScopeIdx = this._moduleRefs.GetOrAdd( ( (MemberReferenceParentModuleRef) resScope ).ModuleRef );
                     break;
                  case MemberReferenceParentKind.MethodDef:
                     resScopeIdx = this._methodDefs.Get( ( (MemberReferenceParentMethodDef) resScope ).Method );
                     break;
                  default:
                     throw new InvalidOperationException( "Invalid member reference resolution scope kind: " + resScope.MemberReferenceParentKind + "." );
               }
               mRefPhysical.DeclaringType = resScopeIdx;
            }
         } );

         this._methodSpecs = new TableIndexTracker<MethodSpecificationStructure>( Tables.MethodSpec, ( mSpec, idx ) =>
            md.MethodSpecifications.TableContents.Add( new MethodSpecification()
            {
               Method = this.GetMethodDefOrMemberRef( mSpec.Method ),
               Signature = this.CreatePhysicalGenericMethodSignature( mSpec.Signature )
            } ) );

         this._standaloneSignatures = new TableIndexTracker<StandaloneSignatureStructure>( Tables.StandaloneSignature, ( sig, idx ) =>
            md.StandaloneSignatures.TableContents.Add( new StandaloneSignature()
            {
               StoreSignatureAsFieldSignature = false,
               Signature = this.CreatePhysicalSignature( sig.Signature )
            } ) );

         this.PopulateStructualTables( module );
      }

      public CILMetaData MetaData
      {
         get
         {
            return this._md;
         }
      }

      public TableIndexTracker<TypeDefinitionStructure> TypeDefs
      {
         get
         {
            return this._typeDefs;
         }
      }

      public TableIndexTracker<TypeReferenceStructure> TypeRefs
      {
         get
         {
            return this._typeRefs;
         }
      }

      public TableIndexTracker<TypeSpecificationStructure> TypeSpecs
      {
         get
         {
            return this._typeSpecs;
         }
      }

      public TableIndexTracker<FieldStructure> FieldDefs
      {
         get
         {
            return this._fieldDefs;
         }
      }

      public TableIndexTracker<MethodStructure> MethodDefs
      {
         get
         {
            return this._methodDefs;
         }
      }

      public TableIndexTracker<ParameterStructure> ParamDefs
      {
         get
         {
            return this._paramDefs;
         }
      }

      public TableIndexTracker<AssemblyReferenceStructure> AssemblyRefs
      {
         get
         {
            return this._assemblyRefs;
         }
      }

      public TableIndexTracker<ModuleReferenceStructure> ModuleRefs
      {
         get
         {
            return this._moduleRefs;
         }
      }

      public TableIndexTracker<MemberReferenceStructure> MemberRefs
      {
         get
         {
            return this._memberRefs;
         }
      }

      public TableIndexTracker<MethodSpecificationStructure> MethodSpecs
      {
         get
         {
            return this._methodSpecs;
         }
      }

      public TableIndexTracker<StandaloneSignatureStructure> StandaloneSignatures
      {
         get
         {
            return this._standaloneSignatures;
         }
      }
   }
   public static CILMetaData[] CreatePhysicalRepresentation( this AssemblyStructure assembly )
   {
      return assembly.Modules
         .Select( m => m.CreatePhysicalRepresentation( assembly ) )
         .ToArray();
   }

   public static CILMetaData CreatePhysicalRepresentation( this ModuleStructure module )
   {
      return module.CreatePhysicalRepresentation( null );
   }

   private static CILMetaData CreatePhysicalRepresentation( this ModuleStructure module, AssemblyStructure assembly )
   {
      var md = CILMetaDataFactory.CreateMinimalModule( module.Name );
      if ( module.IsMainModule && assembly != null )
      {
         var aDef = new AssemblyDefinition()
         {
            Attributes = assembly.Attributes,
            HashAlgorithm = assembly.HashAlgorithm
         };
         assembly.AssemblyInfo.DeepCopyContentsTo( aDef.AssemblyInformation );
      }

      var state = new PhysicalCreationState( md, module );


      return md;
   }

   private static void PopulateStructualTables( this PhysicalCreationState state, ModuleStructure module )
   {
      foreach ( var tDef in module.TopLevelTypeDefinitions )
      {
         state.AddTypeDefRelatedRows( tDef, null );
      }
   }

   private static void AddTypeDefRelatedRows( this PhysicalCreationState state, TypeDefinitionStructure typeDef, TypeDefinitionStructure enclosingType )
   {
      var md = state.MetaData;
      var tDefs = md.TypeDefinitions.TableContents;
      // TypeDef
      var tDefIdx = state.TypeDefs.Add( typeDef );

      // NestedClass
      if ( enclosingType != null )
      {
         md.NestedClassDefinitions.TableContents.Add( new NestedClassDefinition()
         {
            NestedClass = tDefIdx,
            EnclosingClass = state.TypeDefs.Get( enclosingType )
         } );
      }

      // Fields
      foreach ( var field in typeDef.Fields )
      {
         state.FieldDefs.Add( field );
      }

      // Methods and Parameters
      foreach ( var method in typeDef.Methods )
      {
         state.MethodDefs.Add( method );

         foreach ( var param in method.Parameters )
         {
            state.ParamDefs.Add( param );
         }
      }

      // Nested types
      foreach ( var nestedType in typeDef.NestedTypes )
      {
         state.AddTypeDefRelatedRows( nestedType, typeDef );
      }
   }

   private static void FillRestOfTheTables( this PhysicalCreationState state, ModuleStructure module, AssemblyStructure assembly )
   {
      state.AddCustomAttributes( new TableIndex( Tables.Module, 0 ), module );
      if ( module.IsMainModule && assembly != null )
      {
         state.AddCustomAttributes( new TableIndex( Tables.Assembly, 0 ), assembly );
      }

      // TypeDefs
      var md = state.MetaData;
      var tDefs = md.TypeDefinitions.TableContents;
      var gParams = md.GenericParameterDefinitions.TableContents;
      var gConstraints = md.GenericParameterConstraintDefinitions.TableContents;
      var evtMap = md.EventMaps.TableContents;
      var evtDef = md.EventDefinitions.TableContents;
      var propMap = md.PropertyMaps.TableContents;
      var propDef = md.PropertyDefinitions.TableContents;
      var methodSemantics = md.MethodSemantics.TableContents;
      foreach ( var kvp in state.TypeDefs.Dictionary )
      {
         var tDefS = kvp.Key;
         var tDefIdx = new TableIndex( Tables.TypeDef, kvp.Value );
         // Custom attributes
         state.AddCustomAttributes( tDefIdx, tDefS );

         // Base type
         var tDefP = tDefs[tDefIdx.Index];
         if ( tDefS.BaseType != null )
         {
            tDefP.BaseType = state.GetTypeDefOrRefOrSpec( tDefS.BaseType );
         }

         // Generic arguments
         state.AddGenericParameters( tDefS.GenericParameters, tDefIdx );

         // Events
         var evts = tDefS.Events;
         if ( evts.Count > 0 )
         {
            var evtIdx = md.GetNextTableIndexFor( Tables.Event );
            evtMap.Add( new EventMap()
            {
               EventList = evtIdx,
               Parent = tDefIdx
            } );
            evtDef.AddRange( evts.Select( evt =>
            {
               var evtP = new EventDefinition()
               {
                  Attributes = evt.Attributes,
                  EventType = state.GetTypeDefOrRefOrSpec( evt.EventType ),
                  Name = evt.Name
               };
               state.AddCustomAttributes( evtIdx, evt );
               methodSemantics.AddRange( evt.SemanticMethods.Select( sm => new MethodSemantics()
               {
                  Associaton = evtIdx,
                  Attributes = sm.Attributes,
                  Method = state.MethodDefs.Get( sm.Method )
               } ) );
               evtIdx = evtIdx.IncrementIndex();
               return evtP;
            } ) );
         }

         // Properties
         var props = tDefS.Properties;
         if ( props.Count > 0 )
         {
            var propIdx = md.GetNextTableIndexFor( Tables.Property );
            propMap.Add( new PropertyMap()
            {
               PropertyList = propIdx,
               Parent = tDefIdx
            } );
            propDef.AddRange( props.Select( prop =>
            {
               var propP = new PropertyDefinition()
               {
                  Attributes = prop.Attributes,
                  Name = prop.Name,
                  Signature = state.CreatePhysicalPropertySignature( prop.Signature )
               };
               state.AddCustomAttributes( propIdx, prop );
               methodSemantics.AddRange( prop.SemanticMethods.Select( sm => new MethodSemantics()
               {
                  Associaton = propIdx,
                  Attributes = sm.Attributes,
                  Method = state.MethodDefs.Get( sm.Method )
               } ) );
               propIdx = propIdx.IncrementIndex();
               return propP;
            } ) );
         }
         // Interface impl
         // Overridden methods
         // Layout
         // Security
      }

      var mDefs = md.MethodDefinitions.TableContents;
      foreach ( var kvp in state.MethodDefs.Dictionary )
      {
         var mDefS = kvp.Key;
         var mDefIdx = new TableIndex( Tables.MethodDef, kvp.Value );
         var mDefP = mDefs[mDefIdx.Index];
         // Signature
         mDefP.Signature = state.CreatePhysicalMethodDefSignature( mDefS.Signature );
         // Generic arguments
         state.AddGenericParameters( mDefS.GenericParameters, mDefIdx );
         // PInvoke
         // Security
         // Parameters
         // TODO IL


      }
   }

   private static TableIndex GetTypeDefOrRefOrSpec( this PhysicalCreationState state, AbstractTypeStructure type )
   {
      if ( type == null )
      {
         throw new InvalidOperationException( "TypeDef/Ref/Spec was null when it wasn't supposed to be." );
      }

      switch ( type.TypeStructureKind )
      {
         case TypeStructureKind.TypeDef:
            return state.TypeDefs.Get( (TypeDefinitionStructure) type );
         case TypeStructureKind.TypeRef:
            return state.TypeRefs.Get( (TypeReferenceStructure) type );
         case TypeStructureKind.TypeSpec:
            return state.TypeSpecs.Get( (TypeSpecificationStructure) type );
         default:
            throw new InvalidOperationException( "Invalid type structure kind: " + type.TypeStructureKind + "." );
      }
   }

   private static TableIndex GetMethodDefOrMemberRef( this PhysicalCreationState state, MethodDefOrMemberRefStructure mDefOrRef )
   {
      if ( mDefOrRef == null )
      {
         throw new InvalidOperationException( "MethodDef or MemberRef was null when it wasn't supposed to be." );
      }

      switch ( mDefOrRef.MethodReferenceKind )
      {
         case MethodReferenceKind.MethodDef:
            return state.MethodDefs.Get( (MethodStructure) mDefOrRef );
         case MethodReferenceKind.MemberRef:
            return state.MemberRefs.GetOrAdd( (MemberReferenceStructure) mDefOrRef );
         default:
            throw new InvalidOperationException( "Invalid MethodDef or MemberRef kind: " + mDefOrRef.MethodReferenceKind + "." );
      }
   }

   private static void AddCustomAttributes( this PhysicalCreationState state, TableIndex parent, StructureWithCustomAttributes structure )
   {
      state.MetaData.CustomAttributeDefinitions.TableContents.AddRange( structure.CustomAttributes.Select( ca => new CustomAttributeDefinition()
      {
         Parent = parent,
         Signature = ca.Signature, // TODO clone...
         Type = state.GetMethodDefOrMemberRef( ca.Constructor )
      } ) );
   }

   private static void AddGenericParameters( this PhysicalCreationState state, List<GenericParameterStructure> genericParameters, TableIndex owner )
   {
      // Generic arguments
      var md = state.MetaData;
      var gParams = md.GenericParameterDefinitions.TableContents;
      var gConstraints = md.GenericParameterConstraintDefinitions.TableContents;
      gParams.AddRange( genericParameters.Select( g =>
      {
         var gIdx = new TableIndex( Tables.GenericParameter, gParams.Count );
         state.AddCustomAttributes( gIdx, g );
         var gParamP = new GenericParameterDefinition()
         {
            Attributes = g.Attributes,
            GenericParameterIndex = g.GenericParameterIndex,
            Name = g.Name,
            Owner = owner
         };

         gConstraints.AddRange( g.Constraints.Select( gConstraint =>
         {
            state.AddCustomAttributes( new TableIndex( Tables.GenericParameterConstraint, gConstraints.Count ), gConstraint );
            return new GenericParameterConstraintDefinition()
            {
               Constraint = state.GetTypeDefOrRefOrSpec( gConstraint.Constraint ),
               Owner = gIdx
            };
         } ) );

         return gParamP;
      } ) );
   }

   private static AbstractSignature CreatePhysicalSignature( this PhysicalCreationState state, AbstractStructureSignature sig )
   {
      if ( sig == null )
      {
         return null;
      }
      else
      {
         switch ( sig.SignatureKind )
         {
            case StructureSignatureKind.Field:
               return state.CreatePhysicalFieldSignature( (FieldStructureSignature) sig );
            case StructureSignatureKind.GenericMethodInstantiation:
               return state.CreatePhysicalGenericMethodSignature( (GenericMethodStructureSignature) sig );
            case StructureSignatureKind.LocalVariables:
               return state.CreatePhysicalLocalsSignature( (LocalVariablesStructureSignature) sig );
            case StructureSignatureKind.MethodDefinition:
               return state.CreatePhysicalMethodDefSignature( (MethodDefinitionStructureSignature) sig );
            case StructureSignatureKind.MethodReference:
               return state.CreatePhysicalMethodRefSignature( (MethodReferenceStructureSignature) sig );
            case StructureSignatureKind.Property:
               return state.CreatePhysicalPropertySignature( (PropertyStructureSignature) sig );
            case StructureSignatureKind.Type:
               return state.CreatePhysicalTypeSignature( (TypeStructureSignature) sig );
            default:
               throw new InvalidOperationException( "Invalid structure signature kind: " + sig.SignatureKind + "." );
         }
      }
   }

   private static TypeSignature CreatePhysicalTypeSignature( this PhysicalCreationState state, TypeStructureSignature sig )
   {
      if ( sig == null )
      {
         return null;
      }
      else
      {
         switch ( sig.TypeSignatureKind )
         {
            case TypeStructureSignatureKind.ClassOrValue:
               var clazz = (ClassOrValueTypeStructureSignature) sig;
               var clazzP = new ClassOrValueTypeSignature( clazz.GenericArguments.Count )
               {
                  IsClass = clazz.IsClass,
                  Type = state.GetTypeDefOrRefOrSpec( clazz.Type )
               };
               clazzP.GenericArguments.AddRange( clazz.GenericArguments.Select( gArg => state.CreatePhysicalTypeSignature( gArg ) ) );
               return clazzP;
            case TypeStructureSignatureKind.ComplexArray:
               var cArray = (ComplexArrayTypeStructureSignature) sig;
               var cArrayP = new ComplexArrayTypeSignature( cArray.Sizes.Count, cArray.LowerBounds.Count )
               {
                  ArrayType = state.CreatePhysicalTypeSignature( cArray.ArrayType ),
                  Rank = cArray.Rank
               };
               cArrayP.Sizes.AddRange( cArray.Sizes );
               cArrayP.LowerBounds.AddRange( cArray.LowerBounds );
               return cArrayP;
            case TypeStructureSignatureKind.FunctionPointer:
               return new FunctionPointerTypeSignature()
               {
                  MethodSignature = state.CreatePhysicalMethodRefSignature( ( (FunctionPointerTypeStructureSignature) sig ).MethodSignature )
               };
            case TypeStructureSignatureKind.GenericParameter:
               var gParam = (GenericParameterTypeStructureSignature) sig;
               return new GenericParameterTypeSignature()
               {
                  GenericParameterIndex = gParam.GenericParameterIndex,
                  IsTypeParameter = gParam.IsTypeParameter
               };
            case TypeStructureSignatureKind.Pointer:
               var ptr = (PointerTypeStructureSignature) sig;
               var ptrP = new PointerTypeSignature( ptr.CustomModifiers.Count )
               {
                  PointerType = state.CreatePhysicalTypeSignature( ptr.PointerType )
               };
               state.AddPhysicalCustomMods( ptrP.CustomModifiers, ptr.CustomModifiers );
               return ptrP;
            case TypeStructureSignatureKind.Simple:
               return SimpleTypeSignature.GetByElement( ( (SimpleTypeStructureSignature) sig ).SimpleType );
            case TypeStructureSignatureKind.SimpleArray:
               var array = (SimpleArrayTypeStructureSignature) sig;
               var arrayP = new SimpleArrayTypeSignature( array.CustomModifiers.Count )
               {
                  ArrayType = state.CreatePhysicalTypeSignature( array.ArrayType )
               };
               state.AddPhysicalCustomMods( arrayP.CustomModifiers, array.CustomModifiers );
               return arrayP;
            default:
               throw new InvalidOperationException( "Invalid type structure signature kind: " + sig.TypeSignatureKind + "." );
         }
      }
   }

   private static void AddPhysicalCustomMods( this PhysicalCreationState state, List<CustomModifierSignature> physicalMods, IEnumerable<CustomModifierStructureSignature> structureMods )
   {
      physicalMods.AddRange( structureMods.Select( mod => new CustomModifierSignature()
      {
         CustomModifierType = state.GetTypeDefOrRefOrSpec( mod.CustomModifierType ),
         IsOptional = mod.IsOptional
      } ) );
   }

   private static FieldSignature CreatePhysicalFieldSignature( this PhysicalCreationState state, FieldStructureSignature sig )
   {
      FieldSignature retVal;
      if ( sig == null )
      {
         retVal = null;
      }
      else
      {
         retVal = new FieldSignature( sig.CustomModifiers.Count )
         {
            Type = state.CreatePhysicalTypeSignature( sig.Type )
         };
         state.AddPhysicalCustomMods( retVal.CustomModifiers, sig.CustomModifiers );
      }
      return retVal;
   }

   private static void PopulateAbstractPhysicalMethodSignature( this PhysicalCreationState state, AbstractMethodSignature physicalSig, AbstractMethodStructureSignature structureSig )
   {
      physicalSig.SignatureStarter = structureSig.SignatureStarter;
      physicalSig.GenericArgumentCount = structureSig.GenericArgumentCount;
      physicalSig.ReturnType = state.CreatePhysicalParameterSignature( structureSig.ReturnType );
      physicalSig.Parameters.AddRange( structureSig.Parameters.Select( p => state.CreatePhysicalParameterSignature( p ) ) );
   }

   private static MethodDefinitionSignature CreatePhysicalMethodDefSignature( this PhysicalCreationState state, MethodDefinitionStructureSignature sig )
   {
      MethodDefinitionSignature retVal;
      if ( sig == null )
      {
         retVal = null;
      }
      else
      {
         retVal = new MethodDefinitionSignature( sig.Parameters.Count );
         state.PopulateAbstractPhysicalMethodSignature( retVal, sig );
      }
      return retVal;
   }

   private static MethodReferenceSignature CreatePhysicalMethodRefSignature( this PhysicalCreationState state, MethodReferenceStructureSignature sig )
   {
      MethodReferenceSignature retVal;
      if ( sig == null )
      {
         retVal = null;
      }
      else
      {
         retVal = new MethodReferenceSignature( sig.Parameters.Count, sig.VarArgsParameters.Count );
         state.PopulateAbstractPhysicalMethodSignature( retVal, sig );
         retVal.VarArgsParameters.AddRange( sig.VarArgsParameters.Select( p => state.CreatePhysicalParameterSignature( p ) ) );
      }
      return retVal;
   }

   private static void PopulatePhysicalParameterOrLocalSig( this PhysicalCreationState state, ParameterOrLocalVariableSignature physicalSig, ParameterOrLocalVariableStructureSignature structureSig )
   {
      physicalSig.IsByRef = structureSig.IsByRef;
      physicalSig.Type = state.CreatePhysicalTypeSignature( structureSig.Type );
      state.AddPhysicalCustomMods( physicalSig.CustomModifiers, structureSig.CustomModifiers );
   }

   private static ParameterSignature CreatePhysicalParameterSignature( this PhysicalCreationState state, ParameterStructureSignature sig )
   {
      ParameterSignature retVal;
      if ( sig == null )
      {
         retVal = null;
      }
      else
      {
         retVal = new ParameterSignature( sig.CustomModifiers.Count );
         state.PopulatePhysicalParameterOrLocalSig( retVal, sig );
      }
      return retVal;
   }

   private static LocalVariableSignature CreatePhysicalLocalSignature( this PhysicalCreationState state, LocalVariableStructureSignature sig )
   {
      LocalVariableSignature retVal;
      if ( sig == null )
      {
         retVal = null;
      }
      else
      {
         retVal = new LocalVariableSignature( sig.CustomModifiers.Count )
         {
            IsPinned = sig.IsPinned
         };
         state.PopulatePhysicalParameterOrLocalSig( retVal, sig );
      }
      return retVal;
   }

   private static LocalVariablesSignature CreatePhysicalLocalsSignature( this PhysicalCreationState state, LocalVariablesStructureSignature sig )
   {
      LocalVariablesSignature retVal;
      if ( sig == null )
      {
         retVal = null;
      }
      else
      {
         retVal = new LocalVariablesSignature( sig.Locals.Count );
         retVal.Locals.AddRange( sig.Locals.Select( l => state.CreatePhysicalLocalSignature( l ) ) );
      }
      return retVal;
   }

   private static GenericMethodSignature CreatePhysicalGenericMethodSignature( this PhysicalCreationState state, GenericMethodStructureSignature sig )
   {
      GenericMethodSignature retVal;
      if ( sig == null )
      {
         retVal = null;
      }
      else
      {
         retVal = new GenericMethodSignature( sig.GenericArguments.Count );
         retVal.GenericArguments.AddRange( sig.GenericArguments.Select( g => state.CreatePhysicalTypeSignature( g ) ) );
      }
      return retVal;
   }

   private static PropertySignature CreatePhysicalPropertySignature( this PhysicalCreationState state, PropertyStructureSignature sig )
   {
      PropertySignature retVal;
      if ( sig == null )
      {
         retVal = null;
      }
      else
      {
         retVal = new PropertySignature( sig.CustomModifiers.Count, sig.Parameters.Count )
         {
            HasThis = sig.HasThis,
            PropertyType = state.CreatePhysicalTypeSignature( sig.PropertyType )
         };
         state.AddPhysicalCustomMods( retVal.CustomModifiers, sig.CustomModifiers );
         retVal.Parameters.AddRange( sig.Parameters.Select( p => state.CreatePhysicalParameterSignature( p ) ) );
      }
      return retVal;
   }
}