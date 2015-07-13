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
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CILAssemblyManipulator.Structural
{
   public sealed class AssemblyEquivalenceComparer : IEqualityComparer<AssemblyStructureInfo>
   {
      private static readonly AssemblyEquivalenceComparer Instance = new AssemblyEquivalenceComparer();

      private AssemblyEquivalenceComparer()
      {

      }

      public static IEqualityComparer<AssemblyStructureInfo> EqualityComparer
      {
         get
         {
            return Instance;
         }
      }

      Boolean IEqualityComparer<AssemblyStructureInfo>.Equals( AssemblyStructureInfo x, AssemblyStructureInfo y )
      {
         var retVal = ReferenceEquals( x, y );
         if ( !retVal )
         {
            retVal = x != null && y != null
            && x.AssemblyInfo.EqualsTypedEquatable( y.AssemblyInfo )
            && x.Attributes == y.Attributes
            && x.HashAlgorithm == y.HashAlgorithm;
            if ( retVal )
            {
               // Match module structure
               var modulesX = x.Modules;
               var modulesY = y.Modules;
               retVal = modulesX.Count == modulesY.Count;
               if ( retVal )
               {
                  var modulesMatches = new Int32[modulesX.Count];
                  modulesMatches.Fill( -1 );
                  for ( var i = 0; i < modulesX.Count && retVal; ++i )
                  {
                     var moduleX = modulesX[i];
                     var matchingModuleYInfo = modulesMatches
                        .Where( idx => idx == -1 )
                        .Select( ( idx, matchIdx ) => Tuple.Create( matchIdx, new ModuleComparer( moduleX, modulesY[matchIdx] ) ) )
                        .FirstOrDefault( tuple => tuple.Item2.PerformEquivalenceCheckForModules() );
                     var matchingModuleYIndex = matchingModuleYInfo.Item1;

                     if ( matchingModuleYInfo == null )
                     {
                        retVal = false;
                     }
                     else
                     {
                        if ( moduleX.IsMainModule )
                        {
                           var moduleY = modulesY[matchingModuleYIndex];
                           retVal = matchingModuleYInfo.Item2.PerformEquivalenceCheckForAssemblies( x, y );
                        }

                        if ( retVal )
                        {
                           modulesMatches[i] = matchingModuleYIndex;
                        }
                     }
                  }
               }
            }
         }

         return retVal;
      }

      Int32 IEqualityComparer<AssemblyStructureInfo>.GetHashCode( AssemblyStructureInfo obj )
      {
         return obj == null || obj.AssemblyInfo == null ? 0 : obj.AssemblyInfo.GetHashCode();
      }
   }

   public sealed class ModuleComparer
   {
      private readonly ModuleStructureInfo _xModule;
      private readonly ModuleStructureInfo _yModule;
      private readonly IEqualityComparer<ModuleStructureInfo> _moduleComparer;
      private readonly IEqualityComparer<TypeDefDescription> _typeDefComparer;
      private readonly IEqualityComparer<ExportedTypeStructureInfo> _exportedTypeComparer;
      private readonly IEqualityComparer<PropertyStructuralInfo> _propertyComparer;
      private readonly IEqualityComparer<EventStructuralInfo> _eventComparer;
      private readonly IEqualityComparer<GenericParameterConstraintStructuralInfo> _gConstraintComparer;
      private readonly IEqualityComparer<List<CustomAttributeStructure>> _caComparer;
      private readonly IEqualityComparer<InterfaceImplStructuralInfo> _interfaceImplComparer;
      private readonly IEqualityComparer<OverriddenMethodInfo> _overriddenMethodComparer;
      private readonly IEqualityComparer<ManifestResourceStructuralInfo> _resourceComparer;

      private readonly Lazy<IDictionary<AbstractTypeDescription, String>> _xTypeDefFullNames;
      private readonly Lazy<IDictionary<AbstractTypeDescription, String>> _yTypeDefFullNames;

      public ModuleComparer( ModuleStructureInfo x, ModuleStructureInfo y )
      {
         this._xModule = x;
         this._yModule = y;

         this._moduleComparer = ComparerFromFunctions.NewEqualityComparer<ModuleStructureInfo>( this.Equality_Module, this.HashCode_Module );
         this._typeDefComparer = ComparerFromFunctions.NewEqualityComparer<TypeDefDescription>( this.Equality_TypeDefinition, this.HashCode_TypeDefinition );
         this._interfaceImplComparer = ComparerFromFunctions.NewEqualityComparer<InterfaceImplStructuralInfo>( this.Equality_InterfaceImpl, this.HashCode_InterfaceImpl );
         this._overriddenMethodComparer = ComparerFromFunctions.NewEqualityComparer<OverriddenMethodInfo>( this.Equality_OverriddenMethod, this.HashCode_OverriddenMethod );
         this._exportedTypeComparer = ComparerFromFunctions.NewEqualityComparer<ExportedTypeStructureInfo>( this.Equality_ExportedType, this.HashCode_ExportedType );
         this._propertyComparer = ComparerFromFunctions.NewEqualityComparer<PropertyStructuralInfo>( this.Equality_Property, this.HashCode_Property );
         this._eventComparer = ComparerFromFunctions.NewEqualityComparer<EventStructuralInfo>( this.Equality_Event, this.HashCode_Event );
         this._gConstraintComparer = ComparerFromFunctions.NewEqualityComparer<GenericParameterConstraintStructuralInfo>( this.Equality_GenericParameterConstraint, this.HashCode_GenericParameterConstraint );
         this._resourceComparer = ComparerFromFunctions.NewEqualityComparer<ManifestResourceStructuralInfo>( this.Equality_ManifestResource, this.HashCode_ManifestResource );

         this._caComparer = ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.NewListEqualityComparer( ComparerFromFunctions.NewEqualityComparer<CustomAttributeStructure>( Equality_CustomAttribute, HashCode_CustomAttribute ) );

         this._xTypeDefFullNames = new Lazy<IDictionary<AbstractTypeDescription, String>>( () => CreateTypeDefNameDictionary( x ), LazyThreadSafetyMode.None );
         this._yTypeDefFullNames = new Lazy<IDictionary<AbstractTypeDescription, String>>( () => CreateTypeDefNameDictionary( y ), LazyThreadSafetyMode.None );
      }

      public Boolean PerformEquivalenceCheckForModules()
      {
         return this.Equality_Module( this._xModule, this._yModule );
      }

      internal Boolean PerformEquivalenceCheckForAssemblies( AssemblyStructureInfo x, AssemblyStructureInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && Equality_Security( x.SecurityInfo, y.SecurityInfo )
            && new HashSet<ModuleStructureInfo>( x.Modules, this._moduleComparer ).SetEquals( y.Modules )
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
         );
      }

      private Boolean Equality_Module( ModuleStructureInfo x, ModuleStructureInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.IsMainModule == y.IsMainModule
            && new HashSet<TypeDefDescription>( x.TopLevelTypeDefinitions, this._typeDefComparer ).SetEquals( y.TopLevelTypeDefinitions )
            && new HashSet<ExportedTypeStructureInfo>( x.ExportedTypes, this._exportedTypeComparer ).SetEquals( y.ExportedTypes )
            && new HashSet<ManifestResourceStructuralInfo>( x.ManifestResources, this._resourceComparer ).SetEquals( y.ManifestResources )
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_TypeDefinition( TypeDefDescription x, TypeDefDescription y )
      {
         return ReferenceEquals( x, y )
            || ( x != null
            && String.Equals( x.Name, y.Name )
            && String.Equals( x.Namespace, y.Namespace )
            && Equality_TypeDefOrRefOrSpec( x.BaseType, y.BaseType )
            && x.Attributes == y.Attributes
            && ListEqualityComparer<List<FieldStructureInfo>, FieldStructureInfo>.ListEquality( x.Fields, y.Fields, this.Equality_Field )
            && ListEqualityComparer<List<MethodStructureInfo>, MethodStructureInfo>.ListEquality( x.Methods, y.Methods, this.Equality_Method )
            && ListEqualityComparer<List<GenericParameterStructuralInfo>, GenericParameterStructuralInfo>.ListEquality( x.GenericParameters, y.GenericParameters, this.Equality_GenericParameter )
            && new HashSet<PropertyStructuralInfo>( x.Properties, this._propertyComparer ).SetEquals( y.Properties )
            && new HashSet<EventStructuralInfo>( x.Events, this._eventComparer ).SetEquals( y.Events )
            && new HashSet<InterfaceImplStructuralInfo>( x.ImplementedInterfaces, this._interfaceImplComparer ).SetEquals( y.ImplementedInterfaces )
            && Equality_Security( x.SecurityInfo, y.SecurityInfo )
            && new HashSet<OverriddenMethodInfo>( x.OverriddenMethods, this._overriddenMethodComparer ).SetEquals( y.OverriddenMethods )
            && x.Layout.EqualsTypedEquatable( y.Layout )
            && new HashSet<TypeDefDescription>( x.NestedTypes, this._typeDefComparer ).SetEquals( y.NestedTypes )
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_CustomAttribute( CustomAttributeStructure x, CustomAttributeStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && Equality_MethodDefOrMemberRef( x.Constructor, y.Constructor )
            && Comparers.AbstractCustomAttributeSignatureEqualityComparer.Equals( x.Signature, y.Signature )
         );
      }

      private Boolean Equality_TypeDefOrRefOrSpec( AbstractTypeDescription x, AbstractTypeDescription y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.TypeDescriptionKind == y.TypeDescriptionKind
            && Equality_TypeDefOrRefOrSpec_SameKind( x, y )
            );
      }

      private Boolean Equality_Field( FieldStructureInfo x, FieldStructureInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.Attributes == y.Attributes
            && Equality_Signature_Field( x.Signature, y.Signature )
            && Equals( x.ConstantValue, y.ConstantValue )
            && Comparers.MarshalingInfoEqualityComparer.Equals( x.MarshalingInfo, y.MarshalingInfo )
            && x.FieldOffset == y.FieldOffset
            && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.FieldData, y.FieldData )
            && x.PInvokeInfo.EqualsTypedEquatable( y.PInvokeInfo )
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_Method( MethodStructureInfo x, MethodStructureInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && Equality_Signature_MethodDef( x.Signature, y.Signature )
            && ListEqualityComparer<List<ParameterStructureInfo>, ParameterStructureInfo>.ListEquality( x.Parameters, y.Parameters, this.Equality_Parameter )
            && Equality_MethodIL( x.IL, y.IL )
            && x.Attributes == y.Attributes
            && x.ImplementationAttributes == y.ImplementationAttributes
            && x.PInvokeInfo.EqualsTypedEquatable( y.PInvokeInfo )
            && ListEqualityComparer<List<GenericParameterStructuralInfo>, GenericParameterStructuralInfo>.ListEquality( x.GenericParameters, y.GenericParameters, this.Equality_GenericParameter )
            && Equality_Security( x.SecurityInfo, y.SecurityInfo )
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_Parameter( ParameterStructureInfo x, ParameterStructureInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.Sequence == y.Sequence
            && String.Equals( x.Name, y.Name )
            && x.Attributes == y.Attributes
            && Comparers.MarshalingInfoEqualityComparer.Equals( x.MarshalingInfo, y.MarshalingInfo )
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_InterfaceImpl( InterfaceImplStructuralInfo x, InterfaceImplStructuralInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && Equality_TypeDefOrRefOrSpec( x.InterfaceType, y.InterfaceType )
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_TypeDefOrRefOrSpec_SameKind( AbstractTypeDescription x, AbstractTypeDescription y )
      {
         switch ( x.TypeDescriptionKind )
         {
            case TypeDescriptionKind.TypeDef:
               String xName; String yName;
               return ( this._xTypeDefFullNames.TryGetValue( x, out xName ) ?
                  this._yTypeDefFullNames.TryGetValue( y, out yName ) :
                  ( this._yTypeDefFullNames.TryGetValue( x, out xName )
                     && this._xTypeDefFullNames.TryGetValue( y, out yName )
                  ) ) && String.Equals( xName, yName );
            //if ( !this._typeDefFullNames1.TryGetValue( x, out xName ) )
            //{
            //   return this._typeDefFullNames2.TryGetValue( x, out xName )
            //      && this._typeDefFullNames1.TryGetValue( y, out yName )
            //      && String.Equals( xName, yName );
            //}
            //else
            //{
            //   return this._typeDefFullNames2.TryGetValue( y, out yName )
            //      && String.Equals( xName, yName );
            //}
            case TypeDescriptionKind.TypeRef:
               var xx = (TypeRefDescription) x;
               var yy = (TypeRefDescription) y;
               return String.Equals( xx.Name, yy.Name )
                  && String.Equals( xx.Namespace, yy.Namespace )
                  && Equality_TypeRefResolutionScope( xx.ResolutionScope, yy.ResolutionScope )
                  && this._caComparer.Equals( xx.CustomAttributes, yy.CustomAttributes );
            case TypeDescriptionKind.TypeSpec:
               var xs = (TypeSpecDescription) x;
               var ys = (TypeSpecDescription) y;
               return Equality_Signature_Type( xs.Signature, ys.Signature )
                  && this._caComparer.Equals( xs.CustomAttributes, ys.CustomAttributes );
            default:
               throw new InvalidOperationException( "Invalid type ref or def or spec: " + x.TypeDescriptionKind + "." );
         }
      }

      private Boolean Equality_MethodDefOrMemberRef( MethodDefOrMemberRefStructure x, MethodDefOrMemberRefStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.MethodRefKind == y.MethodRefKind
            && Equality_MethodDefOrMemberRef_SameKind( x, y )
            );
      }

      private Boolean Equality_MethodDefOrMemberRef_SameKind( MethodDefOrMemberRefStructure x, MethodDefOrMemberRefStructure y )
      {
         switch ( x.MethodRefKind )
         {
            case MethodRefKind.MethodDef:
               return Equality_Method( (MethodStructureInfo) x, (MethodStructureInfo) y );
            case MethodRefKind.MemberRef:
               return Equality_MemberRef( (MemberReferenceStructuralInfo) x, (MemberReferenceStructuralInfo) y );
            default:
               throw new InvalidOperationException( "Invalid method def or member ref kind: " + x.MethodRefKind + "." );
         }
      }

      private Boolean Equality_OverriddenMethod( OverriddenMethodInfo x, OverriddenMethodInfo y )
      {
         return Equality_MethodDefOrMemberRef( x.MethodBody, y.MethodBody )
            && Equality_MethodDefOrMemberRef( x.MethodDeclaration, y.MethodDeclaration );
      }

      private Boolean Equality_ModuleRef( ModuleRefStructureInfo x, ModuleRefStructureInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.ModuleName, y.ModuleName )
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_AssemblyRef( AssemblyRefStructureInfo x, AssemblyRefStructureInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.AssemblyRef.EqualsTypedEquatable( y.AssemblyRef )
            && x.Attributes == y.Attributes
            && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.HashValue, y.HashValue )
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_ExportedType( ExportedTypeStructureInfo x, ExportedTypeStructureInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && String.Equals( x.Namespace, y.Namespace )
            && x.Attributes == y.Attributes
            && x.TypeDefID == y.TypeDefID
            && Equality_ExportedTypeResolutionScope( x.ResolutionScope, y.ResolutionScope )
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_File( FileReferenceStructureInfo x, FileReferenceStructureInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.Attributes == y.Attributes
            && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.HashValue, y.HashValue )
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_Security( SecurityStructuralInfo x, SecurityStructuralInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.SecurityAction == y.SecurityAction
            && ListEqualityComparer<List<AbstractSecurityInformation>, AbstractSecurityInformation>.NewListEqualityComparer( Comparers.AbstractSecurityInformationEqualityComparer ).Equals( x.PermissionSets, y.PermissionSets )
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_Property( PropertyStructuralInfo x, PropertyStructuralInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.Attributes == y.Attributes
            && Equality_Signature_Property( x.Signature, y.Signature )
            && Equals( x.ConstantValue, y.ConstantValue )
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_Event( EventStructuralInfo x, EventStructuralInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && Equality_TypeDefOrRefOrSpec( x.EventType, y.EventType )
            && x.Attributes == y.Attributes
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_MemberRef( MemberReferenceStructuralInfo x, MemberReferenceStructuralInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && Equality_MemberReferenceParent( x.Parent, y.Parent )
            && Equality_Signature( x.Signature, y.Signature )
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_GenericParameter( GenericParameterStructuralInfo x, GenericParameterStructuralInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.GenericParameterIndex == y.GenericParameterIndex
            && x.Attributes == y.Attributes
            && new HashSet<GenericParameterConstraintStructuralInfo>( x.Constraints, this._gConstraintComparer ).SetEquals( y.Constraints )
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_GenericParameterConstraint( GenericParameterConstraintStructuralInfo x, GenericParameterConstraintStructuralInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && Equality_TypeDefOrRefOrSpec( x.Constraint, y.Constraint )
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_ManifestResource( ManifestResourceStructuralInfo x, ManifestResourceStructuralInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && Equality_ManifestResourceData( x.ManifestData, y.ManifestData )
            && x.Attributes == y.Attributes
            && x.Offset == y.Offset
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_StandaloneSignature( StandaloneSignatureStructure x, StandaloneSignatureStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && Equality_Signature( x.Signature, y.Signature )
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_MethodSpec( MethodSpecificationStructure x, MethodSpecificationStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && Equality_MethodDefOrMemberRef( x.Method, y.Method )
            && Equality_Signature_GenericMethod( x.Signature, y.Signature )
            && this._caComparer.Equals( x.CustomAttributes, y.CustomAttributes )
            );
      }

      private Boolean Equality_ManifestResourceData( ManifestResourceData x, ManifestResourceData y )
      {
         return ReferenceEquals( x, y )
            || ( x != null & y != null
            && x.ManifestResourceDataKind == y.ManifestResourceDataKind
            && Equality_ManifestResourceData_SameKind( x, y )
            );
      }

      private Boolean Equality_MethodIL( MethodILStructureInfo x, MethodILStructureInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null );
      }

      private Boolean Equality_ManifestResourceData_SameKind( ManifestResourceData x, ManifestResourceData y )
      {
         switch ( x.ManifestResourceDataKind )
         {
            case ManifestResourceDataKind.AssemblyRef:
               return Equality_AssemblyRef( ( (ManifestResourceDataAssemblyRef) x ).AssemblyRef, ( (ManifestResourceDataAssemblyRef) y ).AssemblyRef );
            case ManifestResourceDataKind.Embedded:
               return ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( ( (ManifestResourceDataEmbedded) x ).Data, ( (ManifestResourceDataEmbedded) y ).Data );
            case ManifestResourceDataKind.File:
               return Equality_File( ( (ManifestResourceDataFile) x ).FileReference, ( (ManifestResourceDataFile) y ).FileReference );
            default:
               throw new InvalidOperationException( "Invalid manifest resouce data kind: " + x.ManifestResourceDataKind + "." );
         }
      }

      private Boolean Equality_TypeRefResolutionScope( TypeRefResolutionScope x, TypeRefResolutionScope y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.ResolutionScopeKind == y.ResolutionScopeKind
            && Equality_TypeRefResolutionScope_SameKind( x, y )
            );
      }

      private Boolean Equality_TypeRefResolutionScope_SameKind( TypeRefResolutionScope x, TypeRefResolutionScope y )
      {
         switch ( x.ResolutionScopeKind )
         {
            case TypeRefResolutionScopeKind.Nested:
               return Equality_TypeDefOrRefOrSpec( ( (TypeRefResolutionScopeNested) x ).EnclosingTypeRef, ( (TypeRefResolutionScopeNested) y ).EnclosingTypeRef );
            case TypeRefResolutionScopeKind.TypeDef:
               return Equality_TypeDefOrRefOrSpec( ( (TypeRefResolutionScopeTypeDef) x ).TypeDef, ( (TypeRefResolutionScopeTypeDef) y ).TypeDef );
            case TypeRefResolutionScopeKind.ModuleRef:
               return Equality_ModuleRef( ( (TypeRefResolutionScopeModuleRef) x ).ModuleRef, ( (TypeRefResolutionScopeModuleRef) y ).ModuleRef );
            case TypeRefResolutionScopeKind.ExportedType:
               return Equality_ExportedType( ( (TypeRefResolutionScopeExportedType) x ).ExportedType, ( (TypeRefResolutionScopeExportedType) y ).ExportedType );
            case TypeRefResolutionScopeKind.AssemblyRef:
               return Equality_AssemblyRef( ( (TypeRefResolutionScopeAssemblyRef) x ).AssemblyRef, ( (TypeRefResolutionScopeAssemblyRef) y ).AssemblyRef );
            default:
               throw new InvalidOperationException( "Invalid type ref resolution scope: " + x.ResolutionScopeKind + "." );
         }
      }

      private Boolean Equality_ExportedTypeResolutionScope( ExportedTypeResolutionScope x, ExportedTypeResolutionScope y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.ResolutionScopeKind == y.ResolutionScopeKind
            && Equality_ExportedTypeResolutionScope_SameKind( x, y )
            );
      }

      private Boolean Equality_ExportedTypeResolutionScope_SameKind( ExportedTypeResolutionScope x, ExportedTypeResolutionScope y )
      {
         switch ( x.ResolutionScopeKind )
         {
            case ExportedTypeResolutionScopeKind.Nested:
               return Equality_ExportedType( ( (ExportedTypeResolutionScopeNested) x ).EnclosingType, ( (ExportedTypeResolutionScopeNested) y ).EnclosingType );
            case ExportedTypeResolutionScopeKind.File:
               return Equality_File( ( (ExportedTypeResolutionScopeFile) x ).File, ( (ExportedTypeResolutionScopeFile) y ).File );
            case ExportedTypeResolutionScopeKind.AssemblyRef:
               return Equality_AssemblyRef( ( (ExportedTypeResolutionScopeAssemblyRef) x ).AssemblyRef, ( (ExportedTypeResolutionScopeAssemblyRef) y ).AssemblyRef );
            default:
               throw new InvalidOperationException( "Invalid exported type resolution scope: " + x.ResolutionScopeKind + "." );
         }
      }

      private Boolean Equality_MemberReferenceParent( MemberReferenceParent x, MemberReferenceParent y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.MemberReferenceParentKind == y.MemberReferenceParentKind
            && Equality_MemberReferenceParent_SameKind( x, y )
            );
      }

      private Boolean Equality_MemberReferenceParent_SameKind( MemberReferenceParent x, MemberReferenceParent y )
      {
         switch ( x.MemberReferenceParentKind )
         {
            case MemberReferenceParentKind.MethodDef:
               return Equality_Method( ( (MemberReferenceParentMethodDef) x ).Method, ( (MemberReferenceParentMethodDef) y ).Method );
            case MemberReferenceParentKind.ModuleRef:
               return Equality_ModuleRef( ( (MemberReferenceParentModuleRef) x ).ModuleRef, ( (MemberReferenceParentModuleRef) y ).ModuleRef );
            case MemberReferenceParentKind.Type:
               return Equality_TypeDefOrRefOrSpec( ( (MemberReferenceParentType) x ).Type, ( (MemberReferenceParentType) y ).Type );
            default:
               throw new InvalidOperationException( "Invalid member ref parent: " + x.MemberReferenceParentKind + "." );
         }
      }

      private Boolean Equality_Signature( AbstractStructureSignature x, AbstractStructureSignature y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.SignatureKind == y.SignatureKind
            && Equality_Signature_SameKind( x, y )
            );
      }

      private Boolean Equality_Signature_SameKind( AbstractStructureSignature x, AbstractStructureSignature y )
      {
         switch ( x.SignatureKind )
         {
            case StructureSignatureKind.Field:
               return Equality_Signature_Field( (FieldStructureSignature) x, (FieldStructureSignature) y );
            case StructureSignatureKind.GenericMethodInstantiation:
               return Equality_Signature_GenericMethod( (GenericMethodStructureSignature) x, (GenericMethodStructureSignature) y );
            case StructureSignatureKind.LocalVariables:
               return Equality_Signature_Locals( (LocalVariablesStructureSignature) x, (LocalVariablesStructureSignature) y );
            case StructureSignatureKind.MethodDefinition:
               return Equality_Signature_MethodDef( (MethodDefinitionStructureSignature) x, (MethodDefinitionStructureSignature) y );
            case StructureSignatureKind.MethodReference:
               return Equality_Signature_MethodRef( (MethodReferenceStructureSignature) x, (MethodReferenceStructureSignature) y );
            case StructureSignatureKind.Property:
               return Equality_Signature_Property( (PropertyStructureSignature) x, (PropertyStructureSignature) y );
            case StructureSignatureKind.Type:
               return Equality_Signature_Type( (TypeStructureSignature) x, (TypeStructureSignature) y );
            default:
               throw new InvalidOperationException( "Invalid signature kind: " + x.SignatureKind + "." );
         }
      }

      private Boolean Equality_Signature_Type( TypeStructureSignature thisSig, TypeStructureSignature otherSig )
      {
         var retVal = ReferenceEquals( thisSig, otherSig );
         if ( !retVal )
         {
            retVal = thisSig != null && otherSig != null;
            if ( retVal )
            {
               retVal = thisSig.TypeSignatureKind == otherSig.TypeSignatureKind;
               if ( retVal )
               {
                  switch ( thisSig.TypeSignatureKind )
                  {
                     case TypeStructureSignatureKind.ClassOrValue:
                        retVal = Equality_Signature_Type_ClassOrValue( (ClassOrValueTypeStructureSignature) thisSig, (ClassOrValueTypeStructureSignature) otherSig );
                        break;
                     case TypeStructureSignatureKind.ComplexArray:
                        retVal = Equality_Signature_Type_ComplexArray( (ComplexArrayTypeStructureSignature) thisSig, (ComplexArrayTypeStructureSignature) otherSig );
                        break;
                     case TypeStructureSignatureKind.FunctionPointer:
                        retVal = Equality_Signature_MethodRef( ( (FunctionPointerTypeStructureSignature) thisSig ).MethodSignature, ( (FunctionPointerTypeStructureSignature) otherSig ).MethodSignature );
                        break;
                     case TypeStructureSignatureKind.GenericParameter:
                        var thisG = (GenericParameterTypeStructureSignature) thisSig;
                        var otherG = (GenericParameterTypeStructureSignature) otherSig;
                        retVal = thisG.GenericParameterIndex == otherG.GenericParameterIndex
                           && thisG.IsTypeParameter == otherG.IsTypeParameter;
                        break;
                     case TypeStructureSignatureKind.Pointer:
                        retVal = Equality_Signature_Type_Pointer( (PointerTypeStructureSignature) thisSig, (PointerTypeStructureSignature) otherSig );
                        break;
                     case TypeStructureSignatureKind.Simple:
                        retVal = ( (SimpleTypeStructureSignature) thisSig ).SimpleType == ( (SimpleTypeStructureSignature) otherSig ).SimpleType;
                        break;
                     case TypeStructureSignatureKind.SimpleArray:
                        retVal = Equality_Signature_Type_SimpleArray( (SimpleArrayTypeStructureSignature) thisSig, (SimpleArrayTypeStructureSignature) otherSig );
                        break;
                     default:
                        retVal = false;
                        break;
                  }
               }
            }
         }
         return retVal;
      }

      private Boolean Equality_Signature_Type_ClassOrValue( ClassOrValueTypeStructureSignature thisSig, ClassOrValueTypeStructureSignature otherSig )
      {
         var thisArgs = thisSig.GenericArguments;
         var otherArgs = thisSig.GenericArguments;
         var retVal = thisSig.IsClass == otherSig.IsClass
            && thisArgs.Count == otherArgs.Count
            && Equality_TypeDefOrRefOrSpec( thisSig.Type, otherSig.Type );
         if ( retVal && thisArgs.Count > 0 )
         {
            var i = 0;
            while ( i < thisArgs.Count && Equality_Signature_Type( thisArgs[i], otherArgs[i] ) )
            {
               ++i;
            }
            retVal = i == thisArgs.Count;
         }

         return retVal;
      }

      private Boolean Equality_Signature_Type_ComplexArray( ComplexArrayTypeStructureSignature thisSig, ComplexArrayTypeStructureSignature otherSig )
      {
         return thisSig.Rank == otherSig.Rank
            && ListEqualityComparer<List<Int32>, Int32>.DefaultListEqualityComparer.Equals( thisSig.LowerBounds, otherSig.LowerBounds )
            && ListEqualityComparer<List<Int32>, Int32>.DefaultListEqualityComparer.Equals( thisSig.Sizes, otherSig.Sizes )
            && Equality_Signature_Type( thisSig.ArrayType, otherSig.ArrayType );
      }

      private Boolean Equality_Signature_Type_SimpleArray( SimpleArrayTypeStructureSignature thisSig, SimpleArrayTypeStructureSignature otherSig )
      {
         return Equality_Signature_Mods( thisSig.CustomModifiers, otherSig.CustomModifiers )
            && Equality_Signature_Type( thisSig.ArrayType, otherSig.ArrayType );
      }

      private Boolean Equality_Signature_Type_Pointer( PointerTypeStructureSignature thisSig, PointerTypeStructureSignature otherSig )
      {
         return Equality_Signature_Mods( thisSig.CustomModifiers, otherSig.CustomModifiers )
            && Equality_Signature_Type( thisSig.PointerType, otherSig.PointerType );
      }

      private Boolean Equality_Signature_Mods( List<CustomModifierStructureSignature> thisMods, List<CustomModifierStructureSignature> otherMods )
      {
         var retVal = thisMods.Count == otherMods.Count;
         if ( retVal && thisMods.Count > 0 )
         {
            var i = 0;
            while ( i < thisMods.Count && thisMods[i].IsOptional == otherMods[i].IsOptional && Equality_TypeDefOrRefOrSpec( thisMods[i].CustomModifierType, otherMods[i].CustomModifierType ) )
            {
               ++i;
            }
            retVal = i == thisMods.Count;
         }
         return retVal;
      }

      private Boolean Equality_Signature_Field( FieldStructureSignature thisSig, FieldStructureSignature otherSig )
      {
         return ReferenceEquals( thisSig, otherSig )
            || ( thisSig != null && otherSig != null
            && Equality_Signature_Mods( thisSig.CustomModifiers, otherSig.CustomModifiers )
            && Equality_Signature_Type( thisSig.Type, otherSig.Type )
            );
      }

      private Boolean Equality_Signature_GenericMethod( GenericMethodStructureSignature thisSig, GenericMethodStructureSignature otherSig )
      {
         var retVal = ReferenceEquals( thisSig, otherSig );
         if ( !retVal )
         {
            retVal = thisSig != null && otherSig != null;
            if ( retVal )
            {
               var thisArgs = thisSig.GenericArguments;
               var otherArgs = otherSig.GenericArguments;
               retVal = thisArgs.Count == otherArgs.Count;
               if ( retVal && thisArgs.Count > 0 )
               {
                  var i = 0;
                  while ( i < thisArgs.Count && Equality_Signature_Type( thisArgs[i], otherArgs[i] ) )
                  {
                     ++i;
                  }
                  retVal = i == thisArgs.Count;
               }
            }
         }
         return retVal;
      }

      private Boolean Equality_Signature_MethodDef( MethodDefinitionStructureSignature thisSig, MethodDefinitionStructureSignature otherSig )
      {
         return Equality_Signature_AbstractMethodSig( thisSig, otherSig );
      }

      private Boolean Equality_Signature_MethodRef( MethodReferenceStructureSignature thisSig, MethodReferenceStructureSignature otherSig )
      {
         return Equality_Signature_AbstractMethodSig( thisSig, otherSig )
            && Equality_Signature_ParamOrLocalSigs( thisSig.VarArgsParameters, otherSig.VarArgsParameters );
      }

      private Boolean Equality_Signature_AbstractMethodSig( AbstractMethodStructureSignature thisSig, AbstractMethodStructureSignature otherSig )
      {
         var retVal = ( thisSig == null ) == ( otherSig == null );
         if ( retVal && thisSig != null )
         {
            var thisParams = thisSig.Parameters;
            var otherParams = otherSig.Parameters;
            retVal = thisSig.SignatureStarter == otherSig.SignatureStarter
               && thisSig.GenericArgumentCount == otherSig.GenericArgumentCount
               && Equality_Signature_ParamOrLocalSig( thisSig.ReturnType, otherSig.ReturnType );

            if ( retVal && thisParams.Count > 0 )
            {
               retVal = Equality_Signature_ParamOrLocalSigs( thisSig.Parameters, otherSig.Parameters );
            }
         }
         return retVal;
      }

      private Boolean Equality_Signature_ParamOrLocalSigs<TSig>( List<TSig> thisSigs, List<TSig> otherSigs )
         where TSig : ParameterOrLocalVariableStructureSignature
      {
         var retVal = thisSigs.Count == otherSigs.Count;
         if ( retVal && thisSigs.Count > 0 )
         {
            var i = 0;
            while ( i < thisSigs.Count && Equality_Signature_ParamOrLocalSig( thisSigs[i], otherSigs[i] ) )
            {
               ++i;
            }
            retVal = i == thisSigs.Count;
         }
         return retVal;
      }

      private Boolean Equality_Signature_ParamOrLocalSig( ParameterOrLocalVariableStructureSignature thisSig, ParameterOrLocalVariableStructureSignature otherSig )
      {
         return thisSig.IsByRef == otherSig.IsByRef
            && Equality_Signature_Mods( thisSig.CustomModifiers, otherSig.CustomModifiers )
            && Equality_Signature_Type( thisSig.Type, otherSig.Type );
      }

      private Boolean Equality_Signature_Property( PropertyStructureSignature thisSig, PropertyStructureSignature otherSig )
      {
         return ReferenceEquals( thisSig, otherSig )
            || ( thisSig != null && otherSig != null
            && thisSig.HasThis == otherSig.HasThis
            && Equality_Signature_Type( thisSig.PropertyType, otherSig.PropertyType )
            && Equality_Signature_ParamOrLocalSigs( thisSig.Parameters, otherSig.Parameters )
            && Equality_Signature_Mods( thisSig.CustomModifiers, otherSig.CustomModifiers )
            );
      }

      private Boolean Equality_Signature_Locals( LocalVariablesStructureSignature thisSig, LocalVariablesStructureSignature otherSig )
      {
         var thisLocals = thisSig.Locals;
         var otherLocals = otherSig.Locals;
         var retVal = Equality_Signature_ParamOrLocalSigs( thisLocals, otherLocals );
         if ( retVal && thisLocals.Count > 0 )
         {
            var i = 0;
            while ( i < thisLocals.Count && thisLocals[i].IsPinned == otherLocals[i].IsPinned )
            {
               ++i;
            }
            retVal = i == thisLocals.Count;
         }

         return retVal;
      }

      private Int32 HashCode_Module( ModuleStructureInfo x )
      {
         return x == null || x.Name == null ? 0 : x.Name.GetHashCode();
      }

      private Int32 HashCode_TypeDefinition( TypeDefDescription x )
      {
         return x == null ? 0 : ( 17 * 23 + x.Name.GetHashCodeSafe() ) * 23 + x.Namespace.GetHashCodeSafe();
      }

      private Int32 HashCode_CustomAttribute( CustomAttributeStructure x )
      {
         return x == null ? 0 : HashCode_MethodDefOrMemberRef( x.Constructor );
      }

      private Int32 HashCode_Method( MethodStructureInfo x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe() ) * 23 + HashCode_Signature( x.Signature ) );
      }

      private Int32 HashCode_InterfaceImpl( InterfaceImplStructuralInfo x )
      {
         return x == null ? 0 : HashCode_TypeDefOrRefOrSpec( x.InterfaceType );
      }

      private Int32 HashCode_TypeDefOrRefOrSpec( AbstractTypeDescription x )
      {
         return x == null ? 0 : HashCode_TypeDefOrRefOrSpec_NotNull( x );
      }

      private Int32 HashCode_TypeDefOrRefOrSpec_NotNull( AbstractTypeDescription x )
      {
         switch ( x.TypeDescriptionKind )
         {
            case TypeDescriptionKind.TypeDef:
               return HashCode_TypeDefinition( (TypeDefDescription) x );
            case TypeDescriptionKind.TypeRef:
               var tRef = (TypeRefDescription) x;
               return ( 17 * 23 + tRef.Name.GetHashCodeSafe() ) * 23 + tRef.Namespace.GetHashCodeSafe();
            case TypeDescriptionKind.TypeSpec:
               return HashCode_Signature( ( (TypeSpecDescription) x ).Signature );
            default:
               throw new InvalidOperationException( "Invalid type ref or def or spec: " + x.TypeDescriptionKind + "." );
         }
      }

      private Int32 HashCode_MethodDefOrMemberRef( MethodDefOrMemberRefStructure x )
      {
         return x == null ? 0 : ( x.MethodRefKind == MethodRefKind.MethodDef ? HashCode_Method( (MethodStructureInfo) x ) : HashCode_MemberRef( (MemberReferenceStructuralInfo) x ) );
      }

      private Int32 HashCode_OverriddenMethod( OverriddenMethodInfo x )
      {
         return ( 17 * 23 + HashCode_MethodDefOrMemberRef( x.MethodBody ) ) * 23 + HashCode_MethodDefOrMemberRef( x.MethodDeclaration );
      }

      private Int32 HashCode_ModuleRef( ModuleRefStructureInfo x )
      {
         return x == null ? 0 : x.ModuleName.GetHashCodeSafe();
      }

      private Int32 HashCode_AssemblyRef( AssemblyRefStructureInfo x )
      {
         return x == null ? 0 : x.AssemblyRef.GetHashCode();
      }

      private Int32 HashCode_ExportedType( ExportedTypeStructureInfo x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe() ) * 23 + x.Namespace.GetHashCodeSafe() );
      }

      private Int32 HashCode_File( FileReferenceStructureInfo x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe();
      }

      private Int32 HashCode_Security( SecurityStructuralInfo x )
      {
         return x == null ? 0 : ListEqualityComparer<List<AbstractSecurityInformation>, AbstractSecurityInformation>.DefaultListEqualityComparer.GetHashCode( x.PermissionSets );
      }

      private Int32 HashCode_Property( PropertyStructuralInfo x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe() ) * 23 + HashCode_Signature( x.Signature ) );
      }

      private Int32 HashCode_Event( EventStructuralInfo x )
      {
         return x == null ? 0 : ( 17 * 23 + x.Name.GetHashCodeSafe() ) * 23 + HashCode_TypeDefOrRefOrSpec( x.EventType );
      }

      private Int32 HashCode_MemberRef( MemberReferenceStructuralInfo x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe() ) * 23 + HashCode_Signature( x.Signature ) );
      }

      private Int32 HashCode_GenericParameterConstraint( GenericParameterConstraintStructuralInfo x )
      {
         return x == null ? 0 : HashCode_TypeDefOrRefOrSpec( x.Constraint );
      }

      private Int32 HashCode_ManifestResource( ManifestResourceStructuralInfo x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe();
      }

      private Int32 HashCode_StandaloneSignature( StandaloneSignatureStructure x )
      {
         return x == null ? 0 : HashCode_Signature( x.Signature );
      }

      private Int32 HashCode_MethodSpec( MethodSpecificationStructure x )
      {
         return x == null ? 0 : ( ( 17 * 23 + HashCode_MethodDefOrMemberRef( x.Method ) ) * 23 + HashCode_Signature( x.Signature ) );
      }

      private static Int32 HashCode_Signature( AbstractStructureSignature x )
      {
         if ( x == null )
         {
            return 0;
         }
         else
         {
            switch ( x.SignatureKind )
            {
               case StructureSignatureKind.Field:
                  return HashCode_FieldSignature( x as FieldStructureSignature );
               case StructureSignatureKind.GenericMethodInstantiation:
                  return HashCode_GenericMethodSignature( x as GenericMethodStructureSignature );
               case StructureSignatureKind.LocalVariables:
                  return HashCode_LocalVariablesSignature( x as LocalVariablesStructureSignature );
               case StructureSignatureKind.MethodDefinition:
                  return HashCode_MethodDefinitionSignature( x as MethodDefinitionStructureSignature );
               case StructureSignatureKind.MethodReference:
                  return HashCode_MethodReferenceSignature( x as MethodReferenceStructureSignature );
               case StructureSignatureKind.Property:
                  return HashCode_PropertySignature( x as PropertyStructureSignature );
               case StructureSignatureKind.Type:
                  return HashCode_TypeSignature( x as TypeStructureSignature );
               default:
                  return 0;
            }
         }

      }

      private static Int32 HashCode_AbstractMethodSignature( AbstractMethodStructureSignature x )
      {
         return x == null ? 0 : ( ( 17 * 23 + HashCode_ParameterSignature( x.ReturnType ) ) * 23 + ListEqualityComparer<List<ParameterStructureSignature>, ParameterStructureSignature>.GetHashCode( x.Parameters, ParameterSignatureEqualityComparer ) );
      }

      private static Int32 HashCode_MethodDefinitionSignature( MethodDefinitionStructureSignature x )
      {
         return HashCode_AbstractMethodSignature( x );
      }

      private static Int32 HashCode_MethodReferenceSignature( MethodReferenceStructureSignature x )
      {
         // Ignore varargs when calculating hash code
         return HashCode_AbstractMethodSignature( x );
      }

      private static Int32 HashCode_FieldSignature( FieldStructureSignature x )
      {
         return x == null ? 0 : HashCode_TypeSignature( x.Type );
      }

      private static Int32 HashCode_PropertySignature( PropertyStructureSignature x )
      {
         return x == null ? 0 : ( ( 17 * 23 + HashCode_TypeSignature( x.PropertyType ) ) * 23 + ListEqualityComparer<List<ParameterStructureSignature>, ParameterStructureSignature>.GetHashCode( x.Parameters, ParameterSignatureEqualityComparer ) );
      }

      private static Int32 HashCode_LocalVariablesSignature( LocalVariablesSignature x )
      {
         return x == null ? 0 : ( 17 * 23 + ListEqualityComparer<List<LocalVariableStructureSignature>, LocalVariableStructureSignature>.GetHashCode( x.Locals, LocalVariableSignatureEqualityComparer ) );
      }

      private static Int32 HashCode_LocalVariableSignature( LocalVariableStructureSignature x )
      {
         return x == null ? 0 : HashCode_TypeSignature( x.Type );
      }

      private static Int32 HashCode_ParameterSignature( ParameterStructureSignature x )
      {
         return x == null ? 0 : HashCode_TypeSignature( x.Type );
      }

      private static Int32 HashCode_CustomModifierSignature( CustomModifierStructureSignature x )
      {
         return x == null ? 0 : x.CustomModifierType.GetHashCode();
      }

      private static Int32 HashCode_TypeSignature( TypeStructureSignature x )
      {
         if ( x == null )
         {
            return 0;
         }
         else
         {
            switch ( x.TypeSignatureKind )
            {
               case TypeStructureSignatureKind.Simple:
                  return HashCode_SimpleTypeSignature( x as SimpleTypeStructureSignature );
               case TypeStructureSignatureKind.ClassOrValue:
                  return HashCode_ClassOrValueTypeSignature( x as ClassOrValueTypeStructureSignature );
               case TypeStructureSignatureKind.GenericParameter:
                  return HashCode_GenericParameterTypeSignature( x as GenericParameterTypeStructureSignature );
               case TypeStructureSignatureKind.FunctionPointer:
                  return HashCode_FunctionPointerTypeSignature( x as FunctionPointerTypeStructureSignature );
               case TypeStructureSignatureKind.Pointer:
                  return HashCode_PointerTypeSignature( x as PointerTypeStructureSignature );
               case TypeStructureSignatureKind.ComplexArray:
                  return HashCode_ComplexArrayTypeSignature( x as ComplexArrayTypeStructureSignature );
               case TypeStructureSignatureKind.SimpleArray:
                  return HashCode_SimpleArrayTypeSignature( x as SimpleArrayTypeStructureSignature );
               default:
                  return 0;
            }
         }
      }

      private static Int32 HashCode_SimpleTypeSignature( SimpleTypeStructureSignature x )
      {
         return x == null ? 0 : (Int32) x.SimpleType;
      }

      private static Int32 HashCode_ClassOrValueTypeSignature( ClassOrValueTypeStructureSignature x )
      {
         return x == null ? 0 : x.Type.GetHashCode();
      }

      private static Int32 HashCode_GenericParameterTypeSignature( GenericParameterTypeStructureSignature x )
      {
         return x == null ? 0 : x.GenericParameterIndex.GetHashCode();
      }

      private static Int32 HashCode_FunctionPointerTypeSignature( FunctionPointerTypeStructureSignature x )
      {
         return x == null ? 0 : HashCode_MethodReferenceSignature( x.MethodSignature );
      }

      private static Int32 HashCode_PointerTypeSignature( PointerTypeStructureSignature x )
      {
         return x == null ? 0 : ( 17 * 23 + HashCode_TypeSignature( x.PointerType ) );
      }

      private static Int32 HashCode_ComplexArrayTypeSignature( ComplexArrayTypeStructureSignature x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Rank ) * 23 + HashCode_TypeSignature( x.ArrayType ) );
      }

      private static Int32 HashCode_SimpleArrayTypeSignature( SimpleArrayTypeStructureSignature x )
      {
         return x == null ? 0 : ( 17 * 41 + HashCode_TypeSignature( x.ArrayType ) );
      }

      private static Int32 HashCode_GenericMethodSignature( GenericMethodStructureSignature x )
      {
         return x == null ? 0 : ListEqualityComparer<List<TypeStructureSignature>, TypeStructureSignature>.ListHashCode( x.GenericArguments, HashCode_TypeSignature );
      }

      private static IDictionary<String, TypeDefDescription> CreateTypeDefNameDictionary( ModuleStructureInfo module )
      {

      }

   }
}
