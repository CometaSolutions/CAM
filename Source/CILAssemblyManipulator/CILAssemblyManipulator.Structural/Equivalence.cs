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
   public abstract class AbstractAssemblyEquivalenceComparer : IEqualityComparer<AssemblyStructure>
   {
      internal AbstractAssemblyEquivalenceComparer()
      {

      }


      public Boolean Equals( AssemblyStructure x, AssemblyStructure y )
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
               var aInfo = x.AssemblyInfo;
               // Match module structure
               var modulesX = x.Modules;
               var modulesY = y.Modules;
               retVal = modulesX.Count == modulesY.Count;
               if ( retVal && modulesX.Count > 0 )
               {
                  var modulesMatches = new Int32[modulesX.Count];
                  modulesMatches.Fill( -1 );
                  for ( var i = 0; i < modulesX.Count && retVal; ++i )
                  {
                     var moduleX = modulesX[i];
                     var matchingModuleYInfo = modulesMatches
                        .Where( idx => idx == -1 )
                        .Select( ( idx, matchIdx ) => Tuple.Create( matchIdx, this.NewModuleEquivalenceComparer( aInfo, moduleX, modulesY[matchIdx] ) ) )
                        .FirstOrDefault( tuple => tuple.Item2.PerformEquivalenceCheckForModules() );

                     if ( matchingModuleYInfo == null )
                     {
                        retVal = false;
                     }
                     else
                     {
                        var matchingModuleYIndex = matchingModuleYInfo.Item1;
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

      public Int32 GetHashCode( AssemblyStructure obj )
      {
         return obj == null ? 0 : obj.AssemblyInfo.GetHashCode();
      }

      protected abstract ModuleEquivalenceComparer NewModuleEquivalenceComparer( AssemblyInformation assemblyInfo, ModuleStructure x, ModuleStructure y );
   }

   public sealed class AssemblyEquivalenceComparerExact : AbstractAssemblyEquivalenceComparer
   {
      private static readonly IEqualityComparer<AssemblyStructure> DefaultInstance = new AssemblyEquivalenceComparerExact();

      public static IEqualityComparer<AssemblyStructure> ExactEqualityComparer
      {
         get
         {
            return DefaultInstance;
         }
      }

      private AssemblyEquivalenceComparerExact()
      {

      }

      protected override ModuleEquivalenceComparer NewModuleEquivalenceComparer( AssemblyInformation assemblyInfo, ModuleStructure x, ModuleStructure y )
      {
         return new ModuleEquivalenceComparer( assemblyInfo, x, y, null );
      }
   }

   public sealed class AssemblyEquivalenceComparerTokenMatch : AbstractAssemblyEquivalenceComparer, IDisposable
   {

      private readonly Lazy<HashStreamInfo> _publicKeyComputer;

      public AssemblyEquivalenceComparerTokenMatch( CryptoCallbacks cryptoCallbacks )
      {
         this._publicKeyComputer = cryptoCallbacks == null ? null : new Lazy<HashStreamInfo>( () => cryptoCallbacks.CreateHashStream( AssemblyHashAlgorithm.SHA1 ), LazyThreadSafetyMode.None );
      }

      public void Dispose()
      {
         if ( this._publicKeyComputer != null && this._publicKeyComputer.IsValueCreated )
         {
            this._publicKeyComputer.Value.Transform.DisposeSafely();
         }
      }

      protected override ModuleEquivalenceComparer NewModuleEquivalenceComparer( AssemblyInformation assemblyInfo, ModuleStructure x, ModuleStructure y )
      {
         return new ModuleEquivalenceComparer( assemblyInfo, x, y, this._publicKeyComputer );
      }
   }

   /// <summary>
   /// TODO use reference-based dictionary to cache equality results for object (something like IDictionary{Tuple{Object, Object}, Boolean} )
   /// </summary>
   public sealed class ModuleEquivalenceComparer
   {
      private readonly AssemblyInformation _assemblyInfo;
      private readonly ModuleStructure _xModule;
      private readonly ModuleStructure _yModule;
      private readonly IEqualityComparer<ModuleStructure> _moduleComparer;
      private readonly IEqualityComparer<TypeDefinitionStructure> _typeDefComparer;
      private readonly IEqualityComparer<ExportedTypeStructure> _exportedTypeComparer;
      private readonly IEqualityComparer<PropertyStructure> _propertyComparer;
      private readonly IEqualityComparer<EventStructure> _eventComparer;
      private readonly IEqualityComparer<SemanticMethodInfo> _semanticMethodComparer;
      private readonly IEqualityComparer<GenericParameterConstraintStructure> _gConstraintComparer;
      private readonly IEqualityComparer<CustomAttributeStructure> _caComparer;
      private readonly IEqualityComparer<InterfaceImplStructure> _interfaceImplComparer;
      private readonly IEqualityComparer<OverriddenMethodInfo> _overriddenMethodComparer;
      private readonly IEqualityComparer<ManifestResourceStructure> _resourceComparer;
      private readonly IEqualityComparer<MethodExceptionBlockStructure> _methodExceptionComparer;
      private readonly IEqualityComparer<SecurityStructure> _securityComparer;
      private readonly IEqualityComparer<AbstractSecurityInformation> _permissionSetComparer;
      private readonly IEqualityComparer<MethodStructure> _methodComparer;
      private readonly IEqualityComparer<CustomAttributeNamedArgument> _caNamedArgComparer;

      private readonly Lazy<IDictionary<TypeDefinitionStructure, String>> _xTypeDefFullNames;
      private readonly Lazy<IDictionary<TypeDefinitionStructure, String>> _yTypeDefFullNames;
      private readonly Lazy<IDictionary<FieldStructure, TypeDefinitionStructure>> _xFieldDeclaringTypes;
      private readonly Lazy<IDictionary<FieldStructure, TypeDefinitionStructure>> _yFieldDeclaringTypes;
      private readonly Lazy<IDictionary<MethodStructure, TypeDefinitionStructure>> _xMethodDeclaringTypes;
      private readonly Lazy<IDictionary<MethodStructure, TypeDefinitionStructure>> _yMethodDeclaringTypes;

      private readonly Lazy<HashStreamInfo> _publicKeyComputer;

      //public ModuleEquivalenceComparer( ModuleStructure x, ModuleStructure y, CryptoCallbacks cryptoCallbacks )
      //   : this( x, y, cryptoCallbacks == null ? null : new Lazy<HashStreamInfo>( () => cryptoCallbacks.CreateHashStream( AssemblyHashAlgorithm.SHA1 ), LazyThreadSafetyMode.None ) )
      //{

      //}

      internal ModuleEquivalenceComparer( AssemblyInformation assemblyInfo, ModuleStructure x, ModuleStructure y, Lazy<HashStreamInfo> publicKeyComputer )
      {
         ArgumentValidator.ValidateNotNull( "Assembly information", assemblyInfo );
         ArgumentValidator.ValidateNotNull( "First module", x );
         ArgumentValidator.ValidateNotNull( "Second module", y );

         this._assemblyInfo = assemblyInfo;
         this._xModule = x;
         this._yModule = y;
         this._publicKeyComputer = publicKeyComputer;

         this._moduleComparer = ComparerFromFunctions.NewEqualityComparer<ModuleStructure>( this.Equivalence_Module, this.HashCode_Module );
         this._typeDefComparer = ComparerFromFunctions.NewEqualityComparer<TypeDefinitionStructure>( this.Equivalence_TypeDefinition, this.HashCode_TypeDefinition );
         this._interfaceImplComparer = ComparerFromFunctions.NewEqualityComparer<InterfaceImplStructure>( this.Equivalence_InterfaceImpl, this.HashCode_InterfaceImpl );
         this._overriddenMethodComparer = ComparerFromFunctions.NewEqualityComparer<OverriddenMethodInfo>( this.Equivalence_OverriddenMethod, this.HashCode_OverriddenMethod );
         this._exportedTypeComparer = ComparerFromFunctions.NewEqualityComparer<ExportedTypeStructure>( this.Equivalence_ExportedType, this.HashCode_ExportedType );
         this._propertyComparer = ComparerFromFunctions.NewEqualityComparer<PropertyStructure>( this.Equivalence_Property, this.HashCode_Property );
         this._eventComparer = ComparerFromFunctions.NewEqualityComparer<EventStructure>( this.Equivalence_Event, this.HashCode_Event );
         this._semanticMethodComparer = ComparerFromFunctions.NewEqualityComparer<SemanticMethodInfo>( this.Equivalence_SemanticMethod, this.HashCode_SemanticMethod );
         this._gConstraintComparer = ComparerFromFunctions.NewEqualityComparer<GenericParameterConstraintStructure>( this.Equivalence_GenericParameterConstraint, this.HashCode_GenericParameterConstraint );
         this._resourceComparer = ComparerFromFunctions.NewEqualityComparer<ManifestResourceStructure>( this.Equivalence_ManifestResource, this.HashCode_ManifestResource );
         this._methodExceptionComparer = ComparerFromFunctions.NewEqualityComparer<MethodExceptionBlockStructure>( this.Equivalence_MethodException, this.HashCode_MethodException );
         this._securityComparer = ComparerFromFunctions.NewEqualityComparer<SecurityStructure>( this.Equivalence_Security, this.HashCode_Security );
         this._permissionSetComparer = ComparerFromFunctions.NewEqualityComparer<AbstractSecurityInformation>( this.Equivalence_PermissionSet, this.HashCode_PermissionSet );
         this._methodComparer = ComparerFromFunctions.NewEqualityComparer<MethodStructure>( this.Equivalence_Method, this.HashCode_Method );
         this._caNamedArgComparer = ComparerFromFunctions.NewEqualityComparer<CustomAttributeNamedArgument>( this.Equivalence_CANamedArg, this.HashCode_CANamedArg );

         this._caComparer = ComparerFromFunctions.NewEqualityComparer<CustomAttributeStructure>( Equivalence_CustomAttribute, HashCode_CustomAttribute );

         this._xTypeDefFullNames = new Lazy<IDictionary<TypeDefinitionStructure, String>>( () => CreateTypeDefNameDictionary( x ), LazyThreadSafetyMode.None );
         this._yTypeDefFullNames = new Lazy<IDictionary<TypeDefinitionStructure, String>>( () => CreateTypeDefNameDictionary( y ), LazyThreadSafetyMode.None );
         this._xFieldDeclaringTypes = new Lazy<IDictionary<FieldStructure, TypeDefinitionStructure>>( () => CreateDeclaringTypeDictionary<FieldStructure>( x, t => t.Fields ), LazyThreadSafetyMode.None );
         this._yFieldDeclaringTypes = new Lazy<IDictionary<FieldStructure, TypeDefinitionStructure>>( () => CreateDeclaringTypeDictionary<FieldStructure>( y, t => t.Fields ), LazyThreadSafetyMode.None );
         this._xMethodDeclaringTypes = new Lazy<IDictionary<MethodStructure, TypeDefinitionStructure>>( () => CreateDeclaringTypeDictionary<MethodStructure>( x, t => t.Methods ), LazyThreadSafetyMode.None );
         this._yMethodDeclaringTypes = new Lazy<IDictionary<MethodStructure, TypeDefinitionStructure>>( () => CreateDeclaringTypeDictionary<MethodStructure>( y, t => t.Methods ), LazyThreadSafetyMode.None );
      }

      public Boolean PerformEquivalenceCheckForModules()
      {
         return this.Equivalence_Module( this._xModule, this._yModule );
      }

      internal Boolean PerformEquivalenceCheckForAssemblies( AssemblyStructure x, AssemblyStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && ListEqualityComparer<List<ModuleStructure>, ModuleStructure>.IsPermutation( x.Modules, y.Modules, this._moduleComparer )
            && ListEqualityComparer<List<SecurityStructure>, SecurityStructure>.IsPermutation( x.SecurityInfo, y.SecurityInfo, this._securityComparer )
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
         );
      }

      private Boolean Equivalence_Module( ModuleStructure x, ModuleStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.IsMainModule == y.IsMainModule
            && ListEqualityComparer<List<TypeDefinitionStructure>, TypeDefinitionStructure>.IsPermutation( x.TopLevelTypeDefinitions, y.TopLevelTypeDefinitions, this._typeDefComparer )
            && ListEqualityComparer<List<ExportedTypeStructure>, ExportedTypeStructure>.IsPermutation( x.ExportedTypes, y.ExportedTypes, this._exportedTypeComparer )
            && ListEqualityComparer<List<ManifestResourceStructure>, ManifestResourceStructure>.IsPermutation( x.ManifestResources, y.ManifestResources, this._resourceComparer )
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
            );
      }

      private Boolean Equivalence_TypeDefinition( TypeDefinitionStructure x, TypeDefinitionStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && Equivalence_TypeDefinition_NotNull( x, y )
            );
      }

      private Boolean Equivalence_TypeDefinition_NotNull( TypeDefinitionStructure x, TypeDefinitionStructure y )
      {
         List<MethodStructure> ctorsX, methodsX;
         CreateCtorAndMethodLists( x, out ctorsX, out methodsX );
         List<MethodStructure> ctorsY, methodsY;
         CreateCtorAndMethodLists( x, out ctorsY, out methodsY );
         return String.Equals( x.Name, y.Name )
            && String.Equals( x.Namespace, y.Namespace )
            && Equivalence_TypeDefOrRefOrSpec( x.BaseType, y.BaseType )
            && x.Attributes == y.Attributes
            && ListEqualityComparer<List<FieldStructure>, FieldStructure>.ListEquality( x.Fields, y.Fields, this.Equivalence_Field )
            && ListEqualityComparer<List<MethodStructure>, MethodStructure>.IsPermutation( ctorsX, ctorsY, this._methodComparer )
            && ListEqualityComparer<List<MethodStructure>, MethodStructure>.ListEquality( methodsX, methodsY, this.Equivalence_Method )
            && ListEqualityComparer<List<GenericParameterStructure>, GenericParameterStructure>.ListEquality( x.GenericParameters, y.GenericParameters, this.Equivalence_GenericParameter )
            && ListEqualityComparer<List<PropertyStructure>, PropertyStructure>.IsPermutation( x.Properties, y.Properties, this._propertyComparer )
            && ListEqualityComparer<List<EventStructure>, EventStructure>.IsPermutation( x.Events, y.Events, this._eventComparer )
            && ListEqualityComparer<List<InterfaceImplStructure>, InterfaceImplStructure>.IsPermutation( x.ImplementedInterfaces, y.ImplementedInterfaces, this._interfaceImplComparer )
            && ListEqualityComparer<List<SecurityStructure>, SecurityStructure>.IsPermutation( x.SecurityInfo, y.SecurityInfo, this._securityComparer )
            && ListEqualityComparer<List<OverriddenMethodInfo>, OverriddenMethodInfo>.IsPermutation( x.OverriddenMethods, y.OverriddenMethods, this._overriddenMethodComparer )
            && x.Layout.EqualsTypedEquatable( y.Layout )
            && ListEqualityComparer<List<TypeDefinitionStructure>, TypeDefinitionStructure>.IsPermutation( x.NestedTypes, y.NestedTypes, this._typeDefComparer )
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer );
      }

      private void CreateCtorAndMethodLists( TypeDefinitionStructure x, out List<MethodStructure> ctors, out List<MethodStructure> methods )
      {
         ctors = new List<MethodStructure>();
         methods = new List<MethodStructure>();
         foreach ( var method in x.Methods )
         {
            ( ( String.Equals( method.Name, Miscellaneous.INSTANCE_CTOR_NAME ) || String.Equals( method.Name, Miscellaneous.CLASS_CTOR_NAME ) ) ?
               ctors :
               methods ).Add( method );
         }
      }

      private Boolean Equivalence_CustomAttribute( CustomAttributeStructure x, CustomAttributeStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && Equivalence_MethodDefOrMemberRef( x.Constructor, y.Constructor )
            && Equivalence_CASignature( x.Signature, y.Signature )
         );
      }

      private Boolean Equivalence_TypeDefOrRefOrSpec( AbstractTypeStructure x, AbstractTypeStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.TypeStructureKind == y.TypeStructureKind
            && Equivalence_TypeDefOrRefOrSpec_SameKind( x, y )
            );
      }

      private Boolean Equivalence_Field( FieldStructure x, FieldStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.Attributes == y.Attributes
            && Equivalence_Signature_Field( x.Signature, y.Signature )
            && NullableEqualityComparer<ConstantStructure>.Equals( x.ConstantValue, y.ConstantValue )
            && Equivalence_MarshalInfo( x.MarshalingInfo, y.MarshalingInfo )
            && NullableEqualityComparer<Int32>.Equals( x.FieldOffset, y.FieldOffset )
            && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.FieldData, y.FieldData )
            && Equivalence_PInvoke( x.PInvokeInfo, y.PInvokeInfo )
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
            );
      }

      private Boolean Equivalence_Method( MethodStructure x, MethodStructure y )
      {
         return Equivalence_Method_NoIL( x, y )
            && ( ReferenceEquals( x, y )
            || ( x != null && y != null
               && Equivalence_MethodIL( x.IL, y.IL )
               ) );
      }

      private Boolean Equivalence_Method_NoIL( MethodStructure x, MethodStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && Equivalence_Signature_MethodDef( x.Signature, y.Signature )
            && ListEqualityComparer<List<ParameterStructure>, ParameterStructure>.ListEquality( x.Parameters, y.Parameters, this.Equivalence_Parameter )
            && x.Attributes == y.Attributes
            && x.ImplementationAttributes == y.ImplementationAttributes
            && Equivalence_PInvoke( x.PInvokeInfo, y.PInvokeInfo )
            && ListEqualityComparer<List<GenericParameterStructure>, GenericParameterStructure>.ListEquality( x.GenericParameters, y.GenericParameters, this.Equivalence_GenericParameter )
            && ListEqualityComparer<List<SecurityStructure>, SecurityStructure>.IsPermutation( x.SecurityInfo, y.SecurityInfo, this._securityComparer )
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
            );
      }

      private Boolean Equivalence_Parameter( ParameterStructure x, ParameterStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.Sequence == y.Sequence
            && String.Equals( x.Name, y.Name )
            && x.Attributes == y.Attributes
            && NullableEqualityComparer<ConstantStructure>.Equals( x.ConstantValue, y.ConstantValue )
            && Equivalence_MarshalInfo( x.MarshalingInfo, y.MarshalingInfo )
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
            );
      }

      private Boolean Equivalence_InterfaceImpl( InterfaceImplStructure x, InterfaceImplStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && Equivalence_TypeDefOrRefOrSpec( x.InterfaceType, y.InterfaceType )
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
            );
      }

      private Boolean Equivalence_TypeDefOrRefOrSpec_SameKind( AbstractTypeStructure x, AbstractTypeStructure y )
      {
         switch ( x.TypeStructureKind )
         {
            case TypeStructureKind.TypeDef:
               var xDef = (TypeDefinitionStructure) x;
               var yDef = (TypeDefinitionStructure) y;
               return Equivalence_FromDictionary( this._xTypeDefFullNames.Value, this._yTypeDefFullNames.Value, xDef, yDef, ( xName, yName ) => String.Equals( xName, yName ) );
            case TypeStructureKind.TypeRef:
               var xx = (TypeReferenceStructure) x;
               var yy = (TypeReferenceStructure) y;
               return String.Equals( xx.Name, yy.Name )
                  && String.Equals( xx.Namespace, yy.Namespace )
                  && Equivalence_TypeRefResolutionScope( xx.ResolutionScope, yy.ResolutionScope )
                  && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer );
            case TypeStructureKind.TypeSpec:
               var xs = (TypeSpecificationStructure) x;
               var ys = (TypeSpecificationStructure) y;
               return Equivalence_Signature_Type( xs.Signature, ys.Signature )
                  && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer );
            default:
               throw new InvalidOperationException( "Invalid type ref or def or spec: " + x.TypeStructureKind + "." );
         }
      }

      private Boolean Equivalence_MethodDefOrMemberRef( MethodDefOrMemberRefStructure x, MethodDefOrMemberRefStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.MethodReferenceKind == y.MethodReferenceKind
            && Equivalence_MethodDefOrMemberRef_SameKind( x, y )
            );
      }

      private Boolean Equivalence_MethodDefOrMemberRef_SameKind( MethodDefOrMemberRefStructure x, MethodDefOrMemberRefStructure y )
      {
         switch ( x.MethodReferenceKind )
         {
            case MethodReferenceKind.MethodDef:
               var xMDef = (MethodStructure) x;
               var yMDef = (MethodStructure) y;
               return Equivalence_Signature_MethodDef( xMDef.Signature, yMDef.Signature )
                  && Equivalence_FromDictionary( this._xMethodDeclaringTypes.Value, this._yMethodDeclaringTypes.Value, xMDef, yMDef, ( xTDef, yTDef ) => Equivalence_TypeDefOrRefOrSpec( xTDef, yTDef ) );
            case MethodReferenceKind.MemberRef:
               return Equivalence_MemberRef( (MemberReferenceStructure) x, (MemberReferenceStructure) y );
            default:
               throw new InvalidOperationException( "Invalid method def or member ref kind: " + x.MethodReferenceKind + "." );
         }
      }

      private Boolean Equivalence_FromDictionary<T, U>( IDictionary<T, U> xDic, IDictionary<T, U> yDic, T x, T y, Func<U, U, Boolean> equivalence )
      {
         U xU, yU;
         return
            ( xDic.TryGetValue( x, out xU ) ?
               yDic.TryGetValue( y, out yU ) :
               ( yDic.TryGetValue( x, out xU )
                  && xDic.TryGetValue( y, out yU )
               )
            )
           && equivalence( xU, yU );
      }

      private Boolean Equivalence_OverriddenMethod( OverriddenMethodInfo x, OverriddenMethodInfo y )
      {
         return Equivalence_MethodDefOrMemberRef( x.MethodBody, y.MethodBody )
            && Equivalence_MethodDefOrMemberRef( x.MethodDeclaration, y.MethodDeclaration );
      }

      private Boolean Equivalence_ModuleRef( ModuleReferenceStructure x, ModuleReferenceStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.ModuleName, y.ModuleName )
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
            );
      }

      private Boolean Equivalence_AssemblyRef( AssemblyReferenceStructure x, AssemblyReferenceStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && ( x.Attributes == y.Attributes ? Equivalence_AssemblyRef_Precise( x, y ) : Equivalence_AssemblyRef_ComputeTokenIfNeeded( x, y ) )
            && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.HashValue, y.HashValue )
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
            );
      }

      private Boolean Equivalence_AssemblyRef_Precise( AssemblyReferenceStructure x, AssemblyReferenceStructure y )
      {
         return x.AssemblyRef.EqualsTypedEquatable( y.AssemblyRef )
            && x.Attributes == y.Attributes
            && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.HashValue, y.HashValue );
      }

      private Boolean Equivalence_AssemblyRef_ComputeTokenIfNeeded( AssemblyReferenceStructure x, AssemblyReferenceStructure y )
      {
         var retVal = this._publicKeyComputer != null
            && ( x.Attributes & ~AssemblyFlags.PublicKey ) == ( y.Attributes & ~AssemblyFlags.PublicKey );
         if ( retVal )
         {
            var xa = x.AssemblyRef;
            var ya = y.AssemblyRef;
            var xIsFullPublicKey = x.Attributes.IsFullPublicKey();
            var yIsFullPublicKey = y.Attributes.IsFullPublicKey();
            if ( xIsFullPublicKey == yIsFullPublicKey )
            {
               retVal = xa.Equals( ya );
            }
            else
            {
               Equivalence_AssemblyInfo_ComputeTokenIfNeeded( xa, ya, xIsFullPublicKey, yIsFullPublicKey );
            }
         }
         return retVal;
      }

      private Boolean Equivalence_AssemblyInfo_ComputeTokenIfNeeded( AssemblyInformation x, AssemblyInformation y, Boolean xIsFullPublicKey, Boolean yIsFullPublicKey )
      {
         var retVal = xIsFullPublicKey != yIsFullPublicKey && x.Equals( y, false );
         if ( retVal
            && !x.PublicKeyOrToken.IsNullOrEmpty()
            && !y.PublicKeyOrToken.IsNullOrEmpty()
            )
         {
            Byte[] xBytes, yBytes;
            if ( xIsFullPublicKey )
            {
               // Create public key token for x and compare with y
               xBytes = this._publicKeyComputer.Value.ComputePublicKeyToken( x.PublicKeyOrToken );
               yBytes = y.PublicKeyOrToken;
            }
            else
            {
               // Create public key token for y and compare with x
               xBytes = x.PublicKeyOrToken;
               yBytes = this._publicKeyComputer.Value.ComputePublicKeyToken( y.PublicKeyOrToken );
            }
            retVal = ArrayEqualityComparer<Byte>.ArrayEquality( xBytes, yBytes );
         }

         return retVal;
      }

      private Boolean Equivalence_ExportedType( ExportedTypeStructure x, ExportedTypeStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && String.Equals( x.Namespace, y.Namespace )
            && x.Attributes == y.Attributes
            && x.TypeDefID == y.TypeDefID
            && Equivalence_ExportedTypeResolutionScope( x.ResolutionScope, y.ResolutionScope )
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
            );
      }

      private Boolean Equivalence_File( FileReferenceStructure x, FileReferenceStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.Attributes == y.Attributes
            && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.HashValue, y.HashValue )
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
            );
      }

      private Boolean Equivalence_Security( SecurityStructure x, SecurityStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.SecurityAction == y.SecurityAction
            && ListEqualityComparer<List<AbstractSecurityInformation>, AbstractSecurityInformation>.IsPermutation( x.PermissionSets, y.PermissionSets, this._permissionSetComparer )
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
            );
      }

      private Boolean Equivalence_Property( PropertyStructure x, PropertyStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.Attributes == y.Attributes
            && Equivalence_Signature_Property( x.Signature, y.Signature )
            && ListEqualityComparer<List<SemanticMethodInfo>, SemanticMethodInfo>.IsPermutation( x.SemanticMethods, y.SemanticMethods, this._semanticMethodComparer )
            && NullableEqualityComparer<ConstantStructure>.Equals( x.ConstantValue, y.ConstantValue )
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
            );
      }

      private Boolean Equivalence_Event( EventStructure x, EventStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && Equivalence_TypeDefOrRefOrSpec( x.EventType, y.EventType )
            && ListEqualityComparer<List<SemanticMethodInfo>, SemanticMethodInfo>.IsPermutation( x.SemanticMethods, y.SemanticMethods, this._semanticMethodComparer )
            && x.Attributes == y.Attributes
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
            );
      }

      private Boolean Equivalence_SemanticMethod( SemanticMethodInfo x, SemanticMethodInfo y )
      {
         return x.Attributes == y.Attributes
            && Equivalence_Method_NoIL( x.Method, y.Method );
      }

      private Boolean Equivalence_MemberRef( MemberReferenceStructure x, MemberReferenceStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && Equivalence_MemberReferenceParent( x.Parent, y.Parent )
            && Equivalence_Signature( x.Signature, y.Signature )
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
            );
      }

      private Boolean Equivalence_GenericParameter( GenericParameterStructure x, GenericParameterStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.GenericParameterIndex == y.GenericParameterIndex
            && x.Attributes == y.Attributes
            && ListEqualityComparer<List<GenericParameterConstraintStructure>, GenericParameterConstraintStructure>.IsPermutation( x.Constraints, y.Constraints, this._gConstraintComparer )
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
            );
      }

      private Boolean Equivalence_GenericParameterConstraint( GenericParameterConstraintStructure x, GenericParameterConstraintStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && Equivalence_TypeDefOrRefOrSpec( x.Constraint, y.Constraint )
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
            );
      }

      private Boolean Equivalence_ManifestResource( ManifestResourceStructure x, ManifestResourceStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && Equivalence_ManifestResourceData( x.ManifestData, y.ManifestData )
            && x.Attributes == y.Attributes
            && x.Offset == y.Offset
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
            );
      }

      private Boolean Equivalence_StandaloneSignature( StandaloneSignatureStructure x, StandaloneSignatureStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && Equivalence_Signature( x.Signature, y.Signature )
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
            );
      }

      private Boolean Equivalence_MethodSpec( MethodSpecificationStructure x, MethodSpecificationStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && Equivalence_MethodDefOrMemberRef( x.Method, y.Method )
            && Equivalence_Signature_GenericMethod( x.Signature, y.Signature )
            && ListEqualityComparer<List<CustomAttributeStructure>, CustomAttributeStructure>.IsPermutation( x.CustomAttributes, y.CustomAttributes, this._caComparer )
            );
      }

      private Boolean Equivalence_PInvoke( PInvokeInfo x, PInvokeInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.PlatformInvokeName, y.PlatformInvokeName )
            && Equivalence_ModuleRef( x.PlatformInvokeModule, y.PlatformInvokeModule )
            && x.Attributes == y.Attributes
            );
      }

      private Boolean Equivalence_MethodIL( MethodILStructureInfo x, MethodILStructureInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && this.Equivalence_StandaloneSignature( x.Locals, y.Locals )
            && ListEqualityComparer<List<OpCodeStructure>, OpCodeStructure>.ListEquality( x.OpCodes, y.OpCodes, Equivalence_OpCode )
            && ListEqualityComparer<List<MethodExceptionBlockStructure>, MethodExceptionBlockStructure>.IsPermutation( x.ExceptionBlocks, y.ExceptionBlocks, this._methodExceptionComparer )
            && x.InitLocals == y.InitLocals
            && x.MaxStackSize == y.MaxStackSize
            );
      }

      private Boolean Equivalence_MethodException( MethodExceptionBlockStructure x, MethodExceptionBlockStructure y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.BlockType == y.BlockType
            && x.TryOffset == y.TryOffset
            && x.TryLength == y.TryLength
            && x.HandlerOffset == y.HandlerOffset
            && x.HandlerLength == y.HandlerLength
            && x.FilterOffset == y.FilterOffset
            && Equivalence_TypeDefOrRefOrSpec( x.ExceptionType, y.ExceptionType )
            );
      }

      private Boolean Equivalence_OpCode( OpCodeStructure x, OpCodeStructure y )
      {
         var retVal = ReferenceEquals( x, y );
         if ( !retVal && x != null && y != null && x.OpCodeStructureKind == y.OpCodeStructureKind )
         {
            switch ( x.OpCodeStructureKind )
            {
               case OpCodeStructureKind.Simple:
                  retVal = ( (OpCodeStructureSimple) x ).SimpleOpCode == ( (OpCodeStructureSimple) y ).SimpleOpCode;
                  break;
               case OpCodeStructureKind.Wrapper:
                  retVal = Comparers.OpCodeInfoEqualityComparer.Equals( ( (OpCodeStructureWrapper) x ).PhysicalOpCode, ( (OpCodeStructureWrapper) y ).PhysicalOpCode );
                  break;
               case OpCodeStructureKind.WithReference:
                  var xRef = (OpCodeStructureWithReference) x;
                  var yRef = (OpCodeStructureWithReference) y;
                  retVal = xRef.OpCode == yRef.OpCode && Equivalence_ILReference( xRef.Structure, yRef.Structure );
                  break;
            }
         }
         return retVal;
      }

      private Boolean Equivalence_ILReference( StructurePresentInIL x, StructurePresentInIL y )
      {
         var retVal = ReferenceEquals( x, y );
         if ( !retVal && x != null && y != null && x.StructureTokenKind == y.StructureTokenKind )
         {
            switch ( x.StructureTokenKind )
            {
               case OpCodeStructureTokenKind.FieldDef:
                  var xFDef = (FieldStructure) x;
                  var yFDef = (FieldStructure) y;
                  retVal = Equivalence_Field( xFDef, yFDef )
                     && Equivalence_FromDictionary( this._xFieldDeclaringTypes.Value, this._yFieldDeclaringTypes.Value, xFDef, yFDef, ( xTDef, yTDef ) => Equivalence_TypeDefOrRefOrSpec( xTDef, yTDef ) );
                  break;
               case OpCodeStructureTokenKind.MemberRef:
                  retVal = Equivalence_MemberRef( (MemberReferenceStructure) x, (MemberReferenceStructure) y );
                  break;
               case OpCodeStructureTokenKind.MethodDef:
                  var xMDef = (MethodStructure) x;
                  var yMDef = (MethodStructure) y;
                  retVal = Equivalence_Method_NoIL( xMDef, yMDef )
                     && Equivalence_FromDictionary( this._xMethodDeclaringTypes.Value, this._yMethodDeclaringTypes.Value, xMDef, yMDef, ( xTDef, yTDef ) => Equivalence_TypeDefOrRefOrSpec( xTDef, yTDef ) );
                  break;
               case OpCodeStructureTokenKind.MethodSpec:
                  retVal = Equivalence_MethodSpec( (MethodSpecificationStructure) x, (MethodSpecificationStructure) y );
                  break;
               case OpCodeStructureTokenKind.StandaloneSignature:
                  retVal = Equivalence_StandaloneSignature( (StandaloneSignatureStructure) x, (StandaloneSignatureStructure) y );
                  break;
               case OpCodeStructureTokenKind.TypeDef:
               case OpCodeStructureTokenKind.TypeRef:
               case OpCodeStructureTokenKind.TypeSpec:
                  retVal = Equivalence_TypeDefOrRefOrSpec( (AbstractTypeStructure) x, (AbstractTypeStructure) y );
                  break;
            }
         }
         return retVal;
      }

      private Boolean Equivalence_ManifestResourceData( ManifestResourceStructureData x, ManifestResourceStructureData y )
      {
         return ReferenceEquals( x, y )
            || ( x != null & y != null
            && x.ManifestResourceDataKind == y.ManifestResourceDataKind
            && Equivalence_ManifestResourceData_SameKind( x, y )
            );
      }

      private Boolean Equivalence_ManifestResourceData_SameKind( ManifestResourceStructureData x, ManifestResourceStructureData y )
      {
         switch ( x.ManifestResourceDataKind )
         {
            case ManifestResourceDataKind.AssemblyRef:
               return Equivalence_AssemblyRef( ( (ManifestResourceStructureDataAssemblyReference) x ).AssemblyRef, ( (ManifestResourceStructureDataAssemblyReference) y ).AssemblyRef );
            case ManifestResourceDataKind.Embedded:
               return ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( ( (ManifestResourceStructureDataEmbedded) x ).Data, ( (ManifestResourceStructureDataEmbedded) y ).Data );
            case ManifestResourceDataKind.File:
               return Equivalence_File( ( (ManifestResourceStrucureDataFile) x ).FileReference, ( (ManifestResourceStrucureDataFile) y ).FileReference );
            default:
               throw new InvalidOperationException( "Invalid manifest resouce data kind: " + x.ManifestResourceDataKind + "." );
         }
      }

      private Boolean Equivalence_TypeRefResolutionScope( TypeRefeferenceResolutionScope x, TypeRefeferenceResolutionScope y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.ResolutionScopeKind == y.ResolutionScopeKind
            && Equivalence_TypeRefResolutionScope_SameKind( x, y )
            );
      }

      private Boolean Equivalence_TypeRefResolutionScope_SameKind( TypeRefeferenceResolutionScope x, TypeRefeferenceResolutionScope y )
      {
         switch ( x.ResolutionScopeKind )
         {
            case TypeRefResolutionScopeKind.Nested:
               return Equivalence_TypeDefOrRefOrSpec( ( (TypeReferenceResolutionScopeNested) x ).EnclosingTypeRef, ( (TypeReferenceResolutionScopeNested) y ).EnclosingTypeRef );
            case TypeRefResolutionScopeKind.TypeDef:
               return Equivalence_TypeDefOrRefOrSpec( ( (TypeReferenceResolutionScopeTypeDef) x ).TypeDef, ( (TypeReferenceResolutionScopeTypeDef) y ).TypeDef );
            case TypeRefResolutionScopeKind.ModuleRef:
               return Equivalence_ModuleRef( ( (TypeReferenceResolutionScopeModuleRef) x ).ModuleRef, ( (TypeReferenceResolutionScopeModuleRef) y ).ModuleRef );
            case TypeRefResolutionScopeKind.ExportedType:
               return Equivalence_ExportedType( ( (TypeReferenceResolutionScopeExportedType) x ).ExportedType, ( (TypeReferenceResolutionScopeExportedType) y ).ExportedType );
            case TypeRefResolutionScopeKind.AssemblyRef:
               return Equivalence_AssemblyRef( ( (TypeReferenceResolutionScopeAssemblyRef) x ).AssemblyRef, ( (TypeReferenceResolutionScopeAssemblyRef) y ).AssemblyRef );
            default:
               throw new InvalidOperationException( "Invalid type ref resolution scope: " + x.ResolutionScopeKind + "." );
         }
      }

      private Boolean Equivalence_ExportedTypeResolutionScope( ExportedTypeResolutionScope x, ExportedTypeResolutionScope y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.ResolutionScopeKind == y.ResolutionScopeKind
            && Equivalence_ExportedTypeResolutionScope_SameKind( x, y )
            );
      }

      private Boolean Equivalence_ExportedTypeResolutionScope_SameKind( ExportedTypeResolutionScope x, ExportedTypeResolutionScope y )
      {
         switch ( x.ResolutionScopeKind )
         {
            case ExportedTypeResolutionScopeKind.Nested:
               return Equivalence_ExportedType( ( (ExportedTypeResolutionScopeNested) x ).EnclosingType, ( (ExportedTypeResolutionScopeNested) y ).EnclosingType );
            case ExportedTypeResolutionScopeKind.File:
               return Equivalence_File( ( (ExportedTypeResolutionScopeFile) x ).File, ( (ExportedTypeResolutionScopeFile) y ).File );
            case ExportedTypeResolutionScopeKind.AssemblyRef:
               return Equivalence_AssemblyRef( ( (ExportedTypeResolutionScopeAssemblyRef) x ).AssemblyRef, ( (ExportedTypeResolutionScopeAssemblyRef) y ).AssemblyRef );
            default:
               throw new InvalidOperationException( "Invalid exported type resolution scope: " + x.ResolutionScopeKind + "." );
         }
      }

      private Boolean Equivalence_MemberReferenceParent( MemberReferenceParent x, MemberReferenceParent y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.MemberReferenceParentKind == y.MemberReferenceParentKind
            && Equivalence_MemberReferenceParent_SameKind( x, y )
            );
      }

      private Boolean Equivalence_MemberReferenceParent_SameKind( MemberReferenceParent x, MemberReferenceParent y )
      {
         switch ( x.MemberReferenceParentKind )
         {
            case MemberReferenceParentKind.MethodDef:
               return Equivalence_Method_NoIL( ( (MemberReferenceParentMethodDef) x ).Method, ( (MemberReferenceParentMethodDef) y ).Method );
            case MemberReferenceParentKind.ModuleRef:
               return Equivalence_ModuleRef( ( (MemberReferenceParentModuleRef) x ).ModuleRef, ( (MemberReferenceParentModuleRef) y ).ModuleRef );
            case MemberReferenceParentKind.Type:
               return Equivalence_TypeDefOrRefOrSpec( ( (MemberReferenceParentType) x ).Type, ( (MemberReferenceParentType) y ).Type );
            default:
               throw new InvalidOperationException( "Invalid member ref parent: " + x.MemberReferenceParentKind + "." );
         }
      }

      private Boolean Equivalence_MarshalInfo( MarshalingInfo x, MarshalingInfo y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.Value == y.Value
            && x.SafeArrayType == y.SafeArrayType
            && x.ArrayType == y.ArrayType
            && x.IIDParameterIndex == y.IIDParameterIndex
            && x.SizeParameterIndex == y.SizeParameterIndex
            && x.ConstSize == y.ConstSize
            && Equivalence_TypeString( x.SafeArrayUserDefinedType, y.SafeArrayUserDefinedType )
            && Equivalence_TypeString( x.MarshalType, y.MarshalType )
            && String.Equals( x.MarshalCookie, y.MarshalCookie )
            );
      }

      private Boolean Equivalence_PermissionSet( AbstractSecurityInformation x, AbstractSecurityInformation y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.SecurityInformationKind == y.SecurityInformationKind
            && this.Equivalence_PermissionSet_SameKind( x, y )
            );
      }

      private Boolean Equivalence_PermissionSet_SameKind( AbstractSecurityInformation x, AbstractSecurityInformation y )
      {
         if ( this.Equivalence_TypeString( x.SecurityAttributeType, y.SecurityAttributeType ) )
         {
            switch ( x.SecurityInformationKind )
            {
               case SecurityInformationKind.Raw:
                  return ArrayEqualityComparer<Byte>.ArrayEquality( ( (RawSecurityInformation) x ).Bytes, ( (RawSecurityInformation) y ).Bytes );
               case SecurityInformationKind.Resolved:
                  return ListEqualityComparer<List<CustomAttributeNamedArgument>, CustomAttributeNamedArgument>.IsPermutation( ( (SecurityInformation) x ).NamedArguments, ( (SecurityInformation) y ).NamedArguments, this._caNamedArgComparer );
               default:
                  throw new InvalidOperationException( "Invalid security information kind: " + x.SecurityInformationKind + "." );
            }
         }
         else
         {
            return false;
         }
      }

      private Boolean Equivalence_CASignature( AbstractCustomAttributeSignature x, AbstractCustomAttributeSignature y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.CustomAttributeSignatureKind == y.CustomAttributeSignatureKind
            && this.Equivalence_CASignature_SameKind( x, y )
            );
      }

      private Boolean Equivalence_CASignature_SameKind( AbstractCustomAttributeSignature x, AbstractCustomAttributeSignature y )
      {
         switch ( x.CustomAttributeSignatureKind )
         {
            case CustomAttributeSignatureKind.Raw:
               return ArrayEqualityComparer<Byte>.ArrayEquality( ( (RawCustomAttributeSignature) x ).Bytes, ( (RawCustomAttributeSignature) y ).Bytes );
            case CustomAttributeSignatureKind.Resolved:
               var xr = (CustomAttributeSignature) x;
               var yr = (CustomAttributeSignature) y;
               return ListEqualityComparer<List<CustomAttributeTypedArgument>, CustomAttributeTypedArgument>.ListEquality( xr.TypedArguments, yr.TypedArguments, this.Equivalence_CATypedArg )
                  && ListEqualityComparer<List<CustomAttributeNamedArgument>, CustomAttributeNamedArgument>.IsPermutation( xr.NamedArguments, yr.NamedArguments, this._caNamedArgComparer );
            default:
               throw new InvalidOperationException( "Invalid custom attribute signature kind: " + x.CustomAttributeSignatureKind + "." );
         }
      }

      private Boolean Equivalence_CANamedArg( CustomAttributeNamedArgument x, CustomAttributeNamedArgument y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.IsField == y.IsField
            && this.Equivalence_CATypedArg( x.Value, y.Value )
            );
      }

      private Boolean Equivalence_CATypedArg( CustomAttributeTypedArgument x, CustomAttributeTypedArgument y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && Equals( x.Value, y.Value )
            && Equivalence_CATypedArgType( x.Type, y.Type )
            );
      }

      private Boolean Equivalence_CATypedArgType( CustomAttributeArgumentType x, CustomAttributeArgumentType y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.ArgumentTypeKind == y.ArgumentTypeKind
            && this.Equivalence_CATypedArgType_SameKind( x, y )
            );
      }

      private Boolean Equivalence_CATypedArgType_SameKind( CustomAttributeArgumentType x, CustomAttributeArgumentType y )
      {
         switch ( x.ArgumentTypeKind )
         {
            case CustomAttributeArgumentTypeKind.Simple:
               return ( (CustomAttributeArgumentTypeSimple) x ).SimpleType == ( (CustomAttributeArgumentTypeSimple) y ).SimpleType;
            case CustomAttributeArgumentTypeKind.Array:
               return this.Equivalence_CATypedArgType( ( (CustomAttributeArgumentTypeArray) x ).ArrayType, ( (CustomAttributeArgumentTypeArray) y ).ArrayType );
            case CustomAttributeArgumentTypeKind.TypeString:
               return this.Equivalence_TypeString( ( (CustomAttributeArgumentTypeEnum) x ).TypeString, ( (CustomAttributeArgumentTypeEnum) y ).TypeString );
            default:
               throw new InvalidOperationException( "Invalid custom attribute argument type kind: " + x.ArgumentTypeKind + "." );
         }
      }

      private Boolean Equivalence_TypeString( String x, String y )
      {
         return String.Equals( x, y )
            || ( x != null && y != null
            && Equivalence_TypeString_WithParse( x, y )
            );
      }

      private Boolean Equivalence_TypeString_WithParse( String x, String y )
      {
         // This can only produce true if x referes to same type as y, but one has full public key, while other doesn't.
         String xAssembly, yAssembly, xType, yType;
         var xHasAssembly = x.ParseAssemblyQualifiedTypeString( out xType, out xAssembly );
         var yHasAssembly = y.ParseAssemblyQualifiedTypeString( out yType, out yAssembly );

         var retVal = String.Equals( xType, yType );
         if ( retVal )
         {
            AssemblyInformation xAssemblyInfo, yAssemblyInfo;
            Boolean xFullPublicKey, yFullPublicKey;
            if ( xHasAssembly && yHasAssembly )
            {
               retVal = this._publicKeyComputer != null
                  && AssemblyInformation.TryParse( xAssembly, out xAssemblyInfo, out xFullPublicKey )
                  && AssemblyInformation.TryParse( yAssembly, out yAssemblyInfo, out yFullPublicKey )
                  && xFullPublicKey != yFullPublicKey
                  && this.Equivalence_AssemblyInfo_ComputeTokenIfNeeded( xAssemblyInfo, yAssemblyInfo, xFullPublicKey, yFullPublicKey )
                  ;
            }
            else if ( xHasAssembly != yHasAssembly )
            {
               retVal = AssemblyInformation.TryParse( xHasAssembly ? xAssembly : yAssembly, out xAssemblyInfo, out xFullPublicKey )
                  && ( this._assemblyInfo.Equals( xAssemblyInfo ) || ( !xFullPublicKey && this._publicKeyComputer != null && this.Equivalence_AssemblyInfo_ComputeTokenIfNeeded( this._assemblyInfo, xAssemblyInfo, false, xFullPublicKey ) ) );

            }
         }

         return retVal;
      }

      private Boolean Equivalence_Signature( AbstractStructureSignature x, AbstractStructureSignature y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.SignatureKind == y.SignatureKind
            && Equivalence_Signature_SameKind( x, y )
            );
      }

      private Boolean Equivalence_Signature_SameKind( AbstractStructureSignature x, AbstractStructureSignature y )
      {
         switch ( x.SignatureKind )
         {
            case StructureSignatureKind.Field:
               return Equivalence_Signature_Field( (FieldStructureSignature) x, (FieldStructureSignature) y );
            case StructureSignatureKind.GenericMethodInstantiation:
               return Equivalence_Signature_GenericMethod( (GenericMethodStructureSignature) x, (GenericMethodStructureSignature) y );
            case StructureSignatureKind.LocalVariables:
               return Equivalence_Signature_Locals( (LocalVariablesStructureSignature) x, (LocalVariablesStructureSignature) y );
            case StructureSignatureKind.MethodDefinition:
               return Equivalence_Signature_MethodDef( (MethodDefinitionStructureSignature) x, (MethodDefinitionStructureSignature) y );
            case StructureSignatureKind.MethodReference:
               return Equivalence_Signature_MethodRef( (MethodReferenceStructureSignature) x, (MethodReferenceStructureSignature) y );
            case StructureSignatureKind.Property:
               return Equivalence_Signature_Property( (PropertyStructureSignature) x, (PropertyStructureSignature) y );
            case StructureSignatureKind.Type:
               return Equivalence_Signature_Type( (TypeStructureSignature) x, (TypeStructureSignature) y );
            default:
               throw new InvalidOperationException( "Invalid signature kind: " + x.SignatureKind + "." );
         }
      }

      private Boolean Equivalence_Signature_Type( TypeStructureSignature thisSig, TypeStructureSignature otherSig )
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
                        retVal = Equivalence_Signature_Type_ClassOrValue( (ClassOrValueTypeStructureSignature) thisSig, (ClassOrValueTypeStructureSignature) otherSig );
                        break;
                     case TypeStructureSignatureKind.ComplexArray:
                        retVal = Equivalence_Signature_Type_ComplexArray( (ComplexArrayTypeStructureSignature) thisSig, (ComplexArrayTypeStructureSignature) otherSig );
                        break;
                     case TypeStructureSignatureKind.FunctionPointer:
                        retVal = Equivalence_Signature_MethodRef( ( (FunctionPointerTypeStructureSignature) thisSig ).MethodSignature, ( (FunctionPointerTypeStructureSignature) otherSig ).MethodSignature );
                        break;
                     case TypeStructureSignatureKind.GenericParameter:
                        var thisG = (GenericParameterTypeStructureSignature) thisSig;
                        var otherG = (GenericParameterTypeStructureSignature) otherSig;
                        retVal = thisG.GenericParameterIndex == otherG.GenericParameterIndex
                           && thisG.IsTypeParameter == otherG.IsTypeParameter;
                        break;
                     case TypeStructureSignatureKind.Pointer:
                        retVal = Equivalence_Signature_Type_Pointer( (PointerTypeStructureSignature) thisSig, (PointerTypeStructureSignature) otherSig );
                        break;
                     case TypeStructureSignatureKind.Simple:
                        retVal = ( (SimpleTypeStructureSignature) thisSig ).SimpleType == ( (SimpleTypeStructureSignature) otherSig ).SimpleType;
                        break;
                     case TypeStructureSignatureKind.SimpleArray:
                        retVal = Equivalence_Signature_Type_SimpleArray( (SimpleArrayTypeStructureSignature) thisSig, (SimpleArrayTypeStructureSignature) otherSig );
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

      private Boolean Equivalence_Signature_Type_ClassOrValue( ClassOrValueTypeStructureSignature thisSig, ClassOrValueTypeStructureSignature otherSig )
      {
         var thisArgs = thisSig.GenericArguments;
         var otherArgs = otherSig.GenericArguments;
         var retVal = thisSig.IsClass == otherSig.IsClass
            && thisArgs.Count == otherArgs.Count
            && Equivalence_TypeDefOrRefOrSpec( thisSig.Type, otherSig.Type );
         if ( retVal && thisArgs.Count > 0 )
         {
            var i = 0;
            while ( i < thisArgs.Count && Equivalence_Signature_Type( thisArgs[i], otherArgs[i] ) )
            {
               ++i;
            }
            retVal = i == thisArgs.Count;
         }

         return retVal;
      }

      private Boolean Equivalence_Signature_Type_ComplexArray( ComplexArrayTypeStructureSignature thisSig, ComplexArrayTypeStructureSignature otherSig )
      {
         return thisSig.Rank == otherSig.Rank
            && ListEqualityComparer<List<Int32>, Int32>.DefaultListEqualityComparer.Equals( thisSig.LowerBounds, otherSig.LowerBounds )
            && ListEqualityComparer<List<Int32>, Int32>.DefaultListEqualityComparer.Equals( thisSig.Sizes, otherSig.Sizes )
            && Equivalence_Signature_Type( thisSig.ArrayType, otherSig.ArrayType );
      }

      private Boolean Equivalence_Signature_Type_SimpleArray( SimpleArrayTypeStructureSignature thisSig, SimpleArrayTypeStructureSignature otherSig )
      {
         return Equivalence_Signature_Mods( thisSig.CustomModifiers, otherSig.CustomModifiers )
            && Equivalence_Signature_Type( thisSig.ArrayType, otherSig.ArrayType );
      }

      private Boolean Equivalence_Signature_Type_Pointer( PointerTypeStructureSignature thisSig, PointerTypeStructureSignature otherSig )
      {
         return Equivalence_Signature_Mods( thisSig.CustomModifiers, otherSig.CustomModifiers )
            && Equivalence_Signature_Type( thisSig.PointerType, otherSig.PointerType );
      }

      private Boolean Equivalence_Signature_Mods( List<CustomModifierStructureSignature> thisMods, List<CustomModifierStructureSignature> otherMods )
      {
         var retVal = thisMods.Count == otherMods.Count;
         if ( retVal && thisMods.Count > 0 )
         {
            var i = 0;
            while ( i < thisMods.Count && thisMods[i].IsOptional == otherMods[i].IsOptional && Equivalence_TypeDefOrRefOrSpec( thisMods[i].CustomModifierType, otherMods[i].CustomModifierType ) )
            {
               ++i;
            }
            retVal = i == thisMods.Count;
         }
         return retVal;
      }

      private Boolean Equivalence_Signature_Field( FieldStructureSignature thisSig, FieldStructureSignature otherSig )
      {
         return ReferenceEquals( thisSig, otherSig )
            || ( thisSig != null && otherSig != null
            && Equivalence_Signature_Mods( thisSig.CustomModifiers, otherSig.CustomModifiers )
            && Equivalence_Signature_Type( thisSig.Type, otherSig.Type )
            );
      }

      private Boolean Equivalence_Signature_GenericMethod( GenericMethodStructureSignature thisSig, GenericMethodStructureSignature otherSig )
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
                  while ( i < thisArgs.Count && Equivalence_Signature_Type( thisArgs[i], otherArgs[i] ) )
                  {
                     ++i;
                  }
                  retVal = i == thisArgs.Count;
               }
            }
         }
         return retVal;
      }

      private Boolean Equivalence_Signature_MethodDef( MethodDefinitionStructureSignature thisSig, MethodDefinitionStructureSignature otherSig )
      {
         return Equivalence_Signature_AbstractMethodSig( thisSig, otherSig );
      }

      private Boolean Equivalence_Signature_MethodRef( MethodReferenceStructureSignature thisSig, MethodReferenceStructureSignature otherSig )
      {
         return Equivalence_Signature_AbstractMethodSig( thisSig, otherSig )
            && Equivalence_Signature_ParamOrLocalSigs( thisSig.VarArgsParameters, otherSig.VarArgsParameters );
      }

      private Boolean Equivalence_Signature_AbstractMethodSig( AbstractMethodStructureSignature thisSig, AbstractMethodStructureSignature otherSig )
      {
         var retVal = ( thisSig == null ) == ( otherSig == null );
         if ( retVal && thisSig != null )
         {
            var thisParams = thisSig.Parameters;
            var otherParams = otherSig.Parameters;
            retVal = thisSig.SignatureStarter == otherSig.SignatureStarter
               && thisSig.GenericArgumentCount == otherSig.GenericArgumentCount
               && Equivalence_Signature_ParamOrLocalSig( thisSig.ReturnType, otherSig.ReturnType );

            if ( retVal && thisParams.Count > 0 )
            {
               retVal = Equivalence_Signature_ParamOrLocalSigs( thisParams, otherParams );
            }
         }
         return retVal;
      }

      private Boolean Equivalence_Signature_ParamOrLocalSigs<TSig>( List<TSig> thisSigs, List<TSig> otherSigs )
         where TSig : ParameterOrLocalVariableStructureSignature
      {
         var retVal = thisSigs.Count == otherSigs.Count;
         if ( retVal && thisSigs.Count > 0 )
         {
            var i = 0;
            while ( i < thisSigs.Count && Equivalence_Signature_ParamOrLocalSig( thisSigs[i], otherSigs[i] ) )
            {
               ++i;
            }
            retVal = i == thisSigs.Count;
         }
         return retVal;
      }

      private Boolean Equivalence_Signature_ParamOrLocalSig( ParameterOrLocalVariableStructureSignature thisSig, ParameterOrLocalVariableStructureSignature otherSig )
      {
         return thisSig.IsByRef == otherSig.IsByRef
            && Equivalence_Signature_Mods( thisSig.CustomModifiers, otherSig.CustomModifiers )
            && Equivalence_Signature_Type( thisSig.Type, otherSig.Type );
      }

      private Boolean Equivalence_Signature_Property( PropertyStructureSignature thisSig, PropertyStructureSignature otherSig )
      {
         return ReferenceEquals( thisSig, otherSig )
            || ( thisSig != null && otherSig != null
            && thisSig.HasThis == otherSig.HasThis
            && Equivalence_Signature_Type( thisSig.PropertyType, otherSig.PropertyType )
            && Equivalence_Signature_ParamOrLocalSigs( thisSig.Parameters, otherSig.Parameters )
            && Equivalence_Signature_Mods( thisSig.CustomModifiers, otherSig.CustomModifiers )
            );
      }

      private Boolean Equivalence_Signature_Locals( LocalVariablesStructureSignature thisSig, LocalVariablesStructureSignature otherSig )
      {
         var thisLocals = thisSig.Locals;
         var otherLocals = otherSig.Locals;
         var retVal = Equivalence_Signature_ParamOrLocalSigs( thisLocals, otherLocals );
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

      private Int32 HashCode_Module( ModuleStructure x )
      {
         return x == null || x.Name == null ? 0 : x.Name.GetHashCode();
      }

      private Int32 HashCode_TypeDefinition( TypeDefinitionStructure x )
      {
         return x == null ? 0 : ( 17 * 23 + x.Name.GetHashCodeSafe() ) * 23 + x.Namespace.GetHashCodeSafe();
      }

      private Int32 HashCode_CustomAttribute( CustomAttributeStructure x )
      {
         return x == null ? 0 : HashCode_MethodDefOrMemberRef( x.Constructor );
      }

      private Int32 HashCode_Method( MethodStructure x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe() ) * 23 + HashCode_Signature( x.Signature ) );
      }

      private Int32 HashCode_InterfaceImpl( InterfaceImplStructure x )
      {
         return x == null ? 0 : HashCode_TypeDefOrRefOrSpec( x.InterfaceType );
      }

      private Int32 HashCode_TypeDefOrRefOrSpec( AbstractTypeStructure x )
      {
         return x == null ? 0 : HashCode_TypeDefOrRefOrSpec_NotNull( x );
      }

      private Int32 HashCode_TypeDefOrRefOrSpec_NotNull( AbstractTypeStructure x )
      {
         switch ( x.TypeStructureKind )
         {
            case TypeStructureKind.TypeDef:
               return HashCode_TypeDefinition( (TypeDefinitionStructure) x );
            case TypeStructureKind.TypeRef:
               var tRef = (TypeReferenceStructure) x;
               return ( 17 * 23 + tRef.Name.GetHashCodeSafe() ) * 23 + tRef.Namespace.GetHashCodeSafe();
            case TypeStructureKind.TypeSpec:
               return HashCode_Signature( ( (TypeSpecificationStructure) x ).Signature );
            default:
               throw new InvalidOperationException( "Invalid type ref or def or spec: " + x.TypeStructureKind + "." );
         }
      }

      private Int32 HashCode_MethodDefOrMemberRef( MethodDefOrMemberRefStructure x )
      {
         return x == null ? 0 : ( x.MethodReferenceKind == MethodReferenceKind.MethodDef ? HashCode_Method( (MethodStructure) x ) : HashCode_MemberRef( (MemberReferenceStructure) x ) );
      }

      private Int32 HashCode_OverriddenMethod( OverriddenMethodInfo x )
      {
         return ( 17 * 23 + HashCode_MethodDefOrMemberRef( x.MethodBody ) ) * 23 + HashCode_MethodDefOrMemberRef( x.MethodDeclaration );
      }

      private Int32 HashCode_ModuleRef( ModuleReferenceStructure x )
      {
         return x == null ? 0 : x.ModuleName.GetHashCodeSafe();
      }

      private Int32 HashCode_AssemblyRef( AssemblyReferenceStructure x )
      {
         return x == null ? 0 : x.AssemblyRef.GetHashCode();
      }

      private Int32 HashCode_ExportedType( ExportedTypeStructure x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe() ) * 23 + x.Namespace.GetHashCodeSafe() );
      }

      private Int32 HashCode_File( FileReferenceStructure x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe();
      }

      private Int32 HashCode_Security( SecurityStructure x )
      {
         return x == null ? 0 : x.SecurityAction.GetHashCode(); // Can't get hash code over permission set list, as permission set comparison is set comparison, not sequence comparison (since same sequence in different order produces different hash code)
      }

      private Int32 HashCode_Property( PropertyStructure x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe() ) * 23 + HashCode_Signature( x.Signature ) );
      }

      private Int32 HashCode_Event( EventStructure x )
      {
         return x == null ? 0 : ( 17 * 23 + x.Name.GetHashCodeSafe() ) * 23 + HashCode_TypeDefOrRefOrSpec( x.EventType );
      }

      private Int32 HashCode_SemanticMethod( SemanticMethodInfo x )
      {
         return ( 17 * 23 + (Int32) x.Attributes ) * 23 + HashCode_Method( x.Method );
      }

      private Int32 HashCode_MemberRef( MemberReferenceStructure x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe() ) * 23 + HashCode_Signature( x.Signature ) );
      }

      private Int32 HashCode_GenericParameterConstraint( GenericParameterConstraintStructure x )
      {
         return x == null ? 0 : HashCode_TypeDefOrRefOrSpec( x.Constraint );
      }

      private Int32 HashCode_ManifestResource( ManifestResourceStructure x )
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

      private Int32 HashCode_MethodException( MethodExceptionBlockStructure x )
      {
         return x == null ? 0 : ( ( ( 17 * 23 + (Int32) x.BlockType ) * 23 + x.TryOffset ) * 23 + x.TryLength );
      }

      private Int32 HashCode_PermissionSet( AbstractSecurityInformation x )
      {
         if ( x == null )
         {
            return 0;
         }
         else
         {
            switch ( x.SecurityInformationKind )
            {
               case SecurityInformationKind.Raw:
                  return ArrayEqualityComparer<Byte>.GetHashCode( ( (RawSecurityInformation) x ).Bytes );
               case SecurityInformationKind.Resolved:
                  // Can't get list hash code directly over named arguments, as named argument comparison is set comparison, not sequence comparison.
                  return ( (SecurityInformation) x ).NamedArguments.Count;// ListEqualityComparer<List<CustomAttributeNamedArgument>, CustomAttributeNamedArgument>.ListHashCode( ( (SecurityInformation) x ).NamedArguments, this.HashCode_CANamedArg );
               default:
                  throw new InvalidOperationException( "Unrecognized security information kind: " + x.SecurityInformationKind + "." );
            }
         }
      }

      private Int32 HashCode_CANamedArg( CustomAttributeNamedArgument x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe();
      }

      private Int32 HashCode_Signature( AbstractStructureSignature x )
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

      private Int32 HashCode_AbstractMethodSignature( AbstractMethodStructureSignature x )
      {
         return x == null ? 0 : ( ( 17 * 23 + HashCode_ParameterSignature( x.ReturnType ) ) * 23 + ListEqualityComparer<List<ParameterStructureSignature>, ParameterStructureSignature>.ListHashCode( x.Parameters, HashCode_ParameterSignature ) );
      }

      private Int32 HashCode_MethodDefinitionSignature( MethodDefinitionStructureSignature x )
      {
         return HashCode_AbstractMethodSignature( x );
      }

      private Int32 HashCode_MethodReferenceSignature( MethodReferenceStructureSignature x )
      {
         // Ignore varargs when calculating hash code
         return HashCode_AbstractMethodSignature( x );
      }

      private Int32 HashCode_FieldSignature( FieldStructureSignature x )
      {
         return x == null ? 0 : HashCode_TypeSignature( x.Type );
      }

      private Int32 HashCode_PropertySignature( PropertyStructureSignature x )
      {
         return x == null ? 0 : ( ( 17 * 23 + HashCode_TypeSignature( x.PropertyType ) ) * 23 + ListEqualityComparer<List<ParameterStructureSignature>, ParameterStructureSignature>.ListHashCode( x.Parameters, HashCode_ParameterSignature ) );
      }

      private Int32 HashCode_LocalVariablesSignature( LocalVariablesStructureSignature x )
      {
         return x == null ? 0 : ( 17 * 23 + ListEqualityComparer<List<LocalVariableStructureSignature>, LocalVariableStructureSignature>.ListHashCode( x.Locals, HashCode_LocalVariableSignature ) );
      }

      private Int32 HashCode_LocalVariableSignature( LocalVariableStructureSignature x )
      {
         return x == null ? 0 : HashCode_TypeSignature( x.Type );
      }

      private Int32 HashCode_ParameterSignature( ParameterStructureSignature x )
      {
         return x == null ? 0 : HashCode_TypeSignature( x.Type );
      }

      private Int32 HashCode_CustomModifierSignature( CustomModifierStructureSignature x )
      {
         return x == null ? 0 : x.CustomModifierType.GetHashCode();
      }

      private Int32 HashCode_TypeSignature( TypeStructureSignature x )
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

      private Int32 HashCode_SimpleTypeSignature( SimpleTypeStructureSignature x )
      {
         return x == null ? 0 : (Int32) x.SimpleType;
      }

      private Int32 HashCode_ClassOrValueTypeSignature( ClassOrValueTypeStructureSignature x )
      {
         return x == null ? 0 : this.HashCode_TypeDefOrRefOrSpec( x.Type );
      }

      private Int32 HashCode_GenericParameterTypeSignature( GenericParameterTypeStructureSignature x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.GenericParameterIndex.GetHashCode() ) * 23 + x.IsTypeParameter.GetHashCode() );
      }

      private Int32 HashCode_FunctionPointerTypeSignature( FunctionPointerTypeStructureSignature x )
      {
         return x == null ? 0 : HashCode_MethodReferenceSignature( x.MethodSignature );
      }

      private Int32 HashCode_PointerTypeSignature( PointerTypeStructureSignature x )
      {
         return x == null ? 0 : ( 17 * 23 + HashCode_TypeSignature( x.PointerType ) );
      }

      private Int32 HashCode_ComplexArrayTypeSignature( ComplexArrayTypeStructureSignature x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Rank ) * 23 + HashCode_TypeSignature( x.ArrayType ) );
      }

      private Int32 HashCode_SimpleArrayTypeSignature( SimpleArrayTypeStructureSignature x )
      {
         return x == null ? 0 : ( 17 * 41 + HashCode_TypeSignature( x.ArrayType ) );
      }

      private Int32 HashCode_GenericMethodSignature( GenericMethodStructureSignature x )
      {
         return x == null ? 0 : ListEqualityComparer<List<TypeStructureSignature>, TypeStructureSignature>.ListHashCode( x.GenericArguments, HashCode_TypeSignature );
      }

      private static IDictionary<TypeDefinitionStructure, String> CreateTypeDefNameDictionary( ModuleStructure module )
      {
         var retVal = new Dictionary<TypeDefinitionStructure, String>( ReferenceEqualityComparer<TypeDefinitionStructure>.ReferenceBasedComparer );
         foreach ( var type in module.TopLevelTypeDefinitions )
         {
            AddToTypeDefNameDictionary( retVal, null, type );
         }

         return retVal;
      }

      private static void AddToTypeDefNameDictionary( IDictionary<TypeDefinitionStructure, String> dictionary, String currentPrefix, TypeDefinitionStructure currentType )
      {
         var typeString = Miscellaneous.CombineTypeAndNamespace( currentType.Name, currentType.Namespace );
         if ( currentPrefix != null )
         {
            typeString = currentPrefix + typeString;
         }
         dictionary.Add( currentType, typeString );
         foreach ( var nestedType in currentType.NestedTypes )
         {
            AddToTypeDefNameDictionary( dictionary, typeString + Miscellaneous.NESTED_TYPE_SEPARATOR, nestedType );
         }
      }

      private static IDictionary<T, TypeDefinitionStructure> CreateDeclaringTypeDictionary<T>( ModuleStructure module, Func<TypeDefinitionStructure, IEnumerable<T>> extractor )
      {
         var retVal = new Dictionary<T, TypeDefinitionStructure>( ReferenceEqualityComparer<T>.ReferenceBasedComparer );
         foreach ( var tDef in module.TopLevelTypeDefinitions.SelectMany( t => t.AsDepthFirstEnumerable( tt => tt.NestedTypes ) ) )
         {
            foreach ( var element in extractor( tDef ) )
            {
               retVal.Add( element, tDef );
            }
         }

         return retVal;
      }
   }
}
