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
using CollectionsWithRoles.API;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TabularMetaData;

namespace CILAssemblyManipulator.Physical
{
   /// <summary>
   /// This class contains all <see cref="IEqualityComparer{T}"/>s and <see cref="IComparer{T}"/>s for various types present in this assembly.
   /// </summary>
   /// <remarks>
   /// All the equality comparers are <c>exact</c> in a sense that all properties of the objects being compared must match precisely in order for a equality comparer to return value <c>true</c>.
   /// </remarks>
   public static class Comparers
   {
      // Metadata and metadata row comparers
      private static IEqualityComparer<CILMetaData> _MetaDataEqualityComparer = null;
      private static IEqualityComparer<ModuleDefinition> _ModuleDefinitionEqualityComparer = null;
      private static IEqualityComparer<TypeReference> _TypeReferenceEqualityComparer = null;
      private static IEqualityComparer<TypeDefinition> _TypeDefinitionEqualityComparer = null;
      private static IEqualityComparer<FieldDefinitionPointer> _FieldDefinitionPointerEqualityComparer = null;
      private static IEqualityComparer<FieldDefinition> _FieldDefinitionEqualityComparer = null;
      private static IEqualityComparer<MethodDefinitionPointer> _MethodDefinitionPointerEqualityComparer = null;
      private static IEqualityComparer<MethodDefinition> _MethodDefinitionEqualityComparer = null;
      private static IEqualityComparer<ParameterDefinitionPointer> _ParameterDefinitionPointerEqualityComparer = null;
      private static IEqualityComparer<ParameterDefinition> _ParameterDefinitionEqualityComparer = null;
      private static IEqualityComparer<InterfaceImplementation> _InterfaceImplementationEqualityComparer = null;
      private static IEqualityComparer<MemberReference> _MemberReferenceEqualityComparer = null;
      private static IEqualityComparer<ConstantDefinition> _ConstantDefinitionEqualityComparer = null;
      private static IEqualityComparer<CustomAttributeDefinition> _CustomAttributeDefinitionEqualityComparer = null;
      private static IEqualityComparer<FieldMarshal> _FieldMarshalEqualityComparer = null;
      private static IEqualityComparer<SecurityDefinition> _SecurityDefinitionEqualityComparer = null;
      private static IEqualityComparer<ClassLayout> _ClassLayoutEqualityComparer = null;
      private static IEqualityComparer<FieldLayout> _FieldLayoutEqualityComparer = null;
      private static IEqualityComparer<StandaloneSignature> _StandaloneSignatureEqualityComparer = null;
      private static IEqualityComparer<EventMap> _EventMapEqualityComparer = null;
      private static IEqualityComparer<EventDefinitionPointer> _EventDefinitionPointerEqualityComparer = null;
      private static IEqualityComparer<EventDefinition> _EventDefinitionEqualityComparer = null;
      private static IEqualityComparer<PropertyMap> _PropertyMapEqualityComparer = null;
      private static IEqualityComparer<PropertyDefinitionPointer> _PropertyDefinitionPointerEqualityComparer = null;
      private static IEqualityComparer<PropertyDefinition> _PropertyDefinitionEqualityComparer = null;
      private static IEqualityComparer<MethodSemantics> _MethodSemanticsEqualityComparer = null;
      private static IEqualityComparer<MethodImplementation> _MethodImplementationEqualityComparer = null;
      private static IEqualityComparer<ModuleReference> _ModuleReferenceEqualityComparer = null;
      private static IEqualityComparer<TypeSpecification> _TypeSpecificationEqualityComparer = null;
      private static IEqualityComparer<MethodImplementationMap> _MethodImplementationMapEqualityComparer = null;
      private static IEqualityComparer<FieldRVA> _FieldRVAEqualityComparer = null;
      private static IEqualityComparer<EditAndContinueLog> _EditAndContinueLogEqualityComparer = null;
      private static IEqualityComparer<EditAndContinueMap> _EditAndContinueMapEqualityComparer = null;
      private static IEqualityComparer<AssemblyDefinition> _AssemblyDefinitionEqualityComparer = null;
      private static IEqualityComparer<AssemblyReference> _AssemblyReferenceEqualityComparer = null;
#pragma warning disable 618
      private static IEqualityComparer<AssemblyDefinitionProcessor> _AssemblyDefinitionProcessorEqualityComparer = null;
      private static IEqualityComparer<AssemblyDefinitionOS> _AssemblyDefinitionOSEqualityComparer = null;
      private static IEqualityComparer<AssemblyReferenceProcessor> _AssemblyReferenceProcessorEqualityComparer = null;
      private static IEqualityComparer<AssemblyReferenceOS> _AssemblyReferenceOSEqualityComparer = null;
#pragma warning restore 618
      private static IEqualityComparer<FileReference> _FileReferenceEqualityComparer = null;
      private static IEqualityComparer<ExportedType> _ExportedTypeEqualityComparer = null;
      private static IEqualityComparer<ManifestResource> _ManifestResourceEqualityComparer = null;
      private static IEqualityComparer<NestedClassDefinition> _NestedClassDefinitionEqualityComparer = null;
      private static IEqualityComparer<GenericParameterDefinition> _GenericParameterDefinitionEqualityComparer = null;
      private static IEqualityComparer<MethodSpecification> _MethodSpecificationEqualityComparer = null;
      private static IEqualityComparer<GenericParameterConstraintDefinition> _GenericParameterConstraintDefinitionEqualityComparer = null;
      private static IEqualityComparer<MethodILDefinition> _MethodILDefinitionEqualityComparer = null;
      private static IEqualityComparer<MethodExceptionBlock> _MethodExceptionBlockEqualityComparer = null;
      private static IEqualityComparer<OpCodeInfo> _OpCodeInfoEqualityComparer = null;

      private static IEqualityComparer<AssemblyInformation> _AssemblyInformationEqualityComparer = null;
      private static IEqualityComparer<AbstractSignature> _AbstractSignatureEqualityComparer = null;
      private static IEqualityComparer<RawSignature> _RawSignatureEqualityComparer = null;
      private static IEqualityComparer<AbstractMethodSignature> _AbstractMethodSignatureEqualityComparer = null;
      private static IEqualityComparer<AbstractMethodSignature> _AbstractMethodSignatureEqualityComparer_IgnoreKind = null;
      private static IEqualityComparer<MethodDefinitionSignature> _MethodDefinitionSignatureEqualityComparer = null;
      private static IEqualityComparer<MethodReferenceSignature> _MethodReferenceSignatureEqualityComparer = null;
      private static IEqualityComparer<FieldSignature> _FieldSignatureEqualityComparer = null;
      private static IEqualityComparer<PropertySignature> _PropertySignatureEqualityComparer = null;
      private static IEqualityComparer<LocalVariablesSignature> _LocalVariablesSignatureEqualityComparer = null;
      private static IEqualityComparer<LocalVariableSignature> _LocalVariableSignatureEqualityComparer = null;
      private static IEqualityComparer<ParameterSignature> _ParameterSignatureEqualityComparer = null;
      private static IEqualityComparer<CustomModifierSignature> _CustomModifierSignatureEqualityComparer = null;
      private static IEqualityComparer<TypeSignature> _TypeSignatureEqualityComparer = null;
      private static IEqualityComparer<GenericMethodSignature> _GenericMethodSignatureEqualityComparer = null;
      private static IEqualityComparer<AbstractCustomAttributeSignature> _AbstractCustomAttributeSignatureEqualityComparer = null;
      private static IEqualityComparer<CustomAttributeTypedArgument> _CustomAttributeTypedArgumentEqualityComparer = null;
      private static IEqualityComparer<CustomAttributeNamedArgument> _CustomAttributeNamedArgumentEqualityComparer = null;
      private static IEqualityComparer<CustomAttributeArgumentType> _CustomAttributeArgumentTypeEqualityComparer = null;
      private static IEqualityComparer<AbstractSecurityInformation> _AbstractSecurityInformationEqualityComparer = null;
      private static IEqualityComparer<AbstractMarshalingInfo> _MarshalingInfoEqualityComparer = null;

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="CILMetaData"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="CILMetaData"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="CILMetaData"/> are considered to be equal by this equality comparer when all the tables of the <see cref="CILMetaData"/> are considered equal by their <see cref="TabularMetaData.Meta.MetaDataTableInformation{TRow}.EqualityComparer"/>.
      /// </remarks>
      public static IEqualityComparer<CILMetaData> MetaDataComparer
      {
         get
         {
            var retVal = _MetaDataEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<CILMetaData>( Equality_MetaData, HashCode_MetaData );
               _MetaDataEqualityComparer = retVal;
            }

            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="ModuleDefinition"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="ModuleDefinition"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="ModuleDefinition"/> are considered to be equal by this equality comparer when all the properties of the <see cref="ModuleDefinition"/> are equal.
      /// </remarks>
      public static IEqualityComparer<ModuleDefinition> ModuleDefinitionEqualityComparer
      {
         get
         {
            var retVal = _ModuleDefinitionEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<ModuleDefinition>( Equality_ModuleDefinition, HashCode_ModuleDefinition );
               _ModuleDefinitionEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="TypeReference"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="TypeReference"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="TypeReference"/> are considered to be equal by this equality comparer when all the properties of the <see cref="TypeReference"/> are equal.
      /// </remarks>
      public static IEqualityComparer<TypeReference> TypeReferenceEqualityComparer
      {
         get
         {
            var retVal = _TypeReferenceEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<TypeReference>( Equality_TypeReference, HashCode_TypeReference );
               _TypeReferenceEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="TypeDefinition"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="TypeDefinition"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="TypeDefinition"/> are considered to be equal by this equality comparer when all the properties of the <see cref="TypeDefinition"/> are equal.
      /// </remarks>
      public static IEqualityComparer<TypeDefinition> TypeDefinitionEqualityComparer
      {
         get
         {
            var retVal = _TypeDefinitionEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<TypeDefinition>( Equality_TypeDefinition, HashCode_TypeDefinition );
               _TypeDefinitionEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="FieldDefinitionPointer"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="FieldDefinitionPointer"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="FieldDefinitionPointer"/> are considered to be equal by this equality comparer when all the properties of the <see cref="FieldDefinitionPointer"/> are equal.
      /// </remarks>
      public static IEqualityComparer<FieldDefinitionPointer> FieldDefinitionPointerEqualityComparer
      {
         get
         {
            var retVal = _FieldDefinitionPointerEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<FieldDefinitionPointer>( Equality_FieldDefinitionPointer, HashCode_FieldDefinitionPointer );
               _FieldDefinitionPointerEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="FieldDefinition"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="FieldDefinition"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="FieldDefinition"/> are considered to be equal by this equality comparer when all the properties of the <see cref="FieldDefinition"/> are equal.
      /// </remarks>
      public static IEqualityComparer<FieldDefinition> FieldDefinitionEqualityComparer
      {
         get
         {
            var retVal = _FieldDefinitionEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<FieldDefinition>( Equality_FieldDefinition, HashCode_FieldDefinition );
               _FieldDefinitionEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="MethodDefinitionPointer"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="MethodDefinitionPointer"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="MethodDefinitionPointer"/> are considered to be equal by this equality comparer when all the properties of the <see cref="MethodDefinitionPointer"/> are equal.
      /// </remarks>
      public static IEqualityComparer<MethodDefinitionPointer> MethodDefinitionPointerEqualityComparer
      {
         get
         {
            var retVal = _MethodDefinitionPointerEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<MethodDefinitionPointer>( Equality_MethodDefinitionPointer, HashCode_MethodDefinitionPointer );
               _MethodDefinitionPointerEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="MethodDefinition"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="MethodDefinition"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="MethodDefinition"/> are considered to be equal by this equality comparer when all the properties of the <see cref="MethodDefinition"/> are equal.
      /// This also includes comparing the <see cref="MethodDefinition.IL"/> using <see cref="MethodILDefinitionEqualityComparer"/>.
      /// </remarks>
      public static IEqualityComparer<MethodDefinition> MethodDefinitionEqualityComparer
      {
         get
         {
            var retVal = _MethodDefinitionEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<MethodDefinition>( Equality_MethodDefinition, HashCode_MethodDefinition );
               _MethodDefinitionEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="ParameterDefinitionPointer"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="ParameterDefinitionPointer"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="ParameterDefinitionPointer"/> are considered to be equal by this equality comparer when all the properties of the <see cref="ParameterDefinitionPointer"/> are equal.
      /// </remarks>
      public static IEqualityComparer<ParameterDefinitionPointer> ParameterDefinitionPointerEqualityComparer
      {
         get
         {
            var retVal = _ParameterDefinitionPointerEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<ParameterDefinitionPointer>( Equality_ParameterDefinitionPointer, HashCode_ParameterDefinitionPointer );
               _ParameterDefinitionPointerEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="ParameterDefinition"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="ParameterDefinition"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="ParameterDefinition"/> are considered to be equal by this equality comparer when all the properties of the <see cref="ParameterDefinition"/> are equal.
      /// </remarks>
      public static IEqualityComparer<ParameterDefinition> ParameterDefinitionEqualityComparer
      {
         get
         {
            var retVal = _ParameterDefinitionEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<ParameterDefinition>( Equality_ParameterDefinition, HashCode_ParameterDefinition );
               _ParameterDefinitionEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="InterfaceImplementation"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="InterfaceImplementation"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="InterfaceImplementation"/> are considered to be equal by this equality comparer when all the properties of the <see cref="InterfaceImplementation"/> are equal.
      /// </remarks>
      public static IEqualityComparer<InterfaceImplementation> InterfaceImplementationEqualityComparer
      {
         get
         {
            var retVal = _InterfaceImplementationEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<InterfaceImplementation>( Equality_InterfaceImplementation, HashCode_InterfaceImplementation );
               _InterfaceImplementationEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="MemberReference"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="MemberReference"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="MemberReference"/> are considered to be equal by this equality comparer when all the properties of the <see cref="MemberReference"/> are equal.
      /// </remarks>
      public static IEqualityComparer<MemberReference> MemberReferenceEqualityComparer
      {
         get
         {
            var retVal = _MemberReferenceEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<MemberReference>( Equality_MemberReference, HashCode_MemberReference );
               _MemberReferenceEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="ConstantDefinition"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="ConstantDefinition"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="ConstantDefinition"/> are considered to be equal by this equality comparer when all the properties of the <see cref="ConstantDefinition"/> are equal.
      /// </remarks>
      public static IEqualityComparer<ConstantDefinition> ConstantDefinitionEqualityComparer
      {
         get
         {
            var retVal = _ConstantDefinitionEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<ConstantDefinition>( Equality_ConstantDefinition, HashCode_ConstantDefinition );
               _ConstantDefinitionEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="CustomAttributeDefinition"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="CustomAttributeDefinition"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="CustomAttributeDefinition"/> are considered to be equal by this equality comparer when all the properties of the <see cref="CustomAttributeDefinition"/> are equal.
      /// </remarks>
      /// <seealso cref="CustomAttributeTypedArgumentEqualityComparer"/>
      /// <see cref="CustomAttributeNamedArgumentEqualityComparer"/>
      public static IEqualityComparer<CustomAttributeDefinition> CustomAttributeDefinitionEqualityComparer
      {
         get
         {
            var retVal = _CustomAttributeDefinitionEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<CustomAttributeDefinition>( Equality_CustomAttributeDefinition, HashCode_CustomAttributeDefinition );
               _CustomAttributeDefinitionEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="FieldMarshal"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="FieldMarshal"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="FieldMarshal"/> are considered to be equal by this equality comparer when all the properties of the <see cref="FieldMarshal"/> are equal.
      /// </remarks>
      public static IEqualityComparer<FieldMarshal> FieldMarshalEqualityComparer
      {
         get
         {
            var retVal = _FieldMarshalEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<FieldMarshal>( Equality_FieldMarshal, HashCode_FieldMarshal );
               _FieldMarshalEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="SecurityDefinition"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="SecurityDefinition"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="SecurityDefinition"/> are considered to be equal by this equality comparer when all the properties of the <see cref="SecurityDefinition"/> are equal.
      /// The values of <see cref="SecurityDefinition.PermissionSets"/> are compared using <see cref="AbstractSecurityInformationEqualityComparer"/>.
      /// </remarks>
      public static IEqualityComparer<SecurityDefinition> SecurityDefinitionEqualityComparer
      {
         get
         {
            var retVal = _SecurityDefinitionEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<SecurityDefinition>( Equality_SecurityDefinition, HashCode_SecurityDefinition );
               _SecurityDefinitionEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="ClassLayout"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="ClassLayout"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="ClassLayout"/> are considered to be equal by this equality comparer when all the properties of the <see cref="ClassLayout"/> are equal.
      /// </remarks>
      public static IEqualityComparer<ClassLayout> ClassLayoutEqualityComparer
      {
         get
         {
            var retVal = _ClassLayoutEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<ClassLayout>( Equality_ClassLayout, HashCode_ClassLayout );
               _ClassLayoutEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="FieldLayout"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="FieldLayout"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="FieldLayout"/> are considered to be equal by this equality comparer when all the properties of the <see cref="FieldLayout"/> are equal.
      /// </remarks>
      public static IEqualityComparer<FieldLayout> FieldLayoutEqualityComparer
      {
         get
         {
            var retVal = _FieldLayoutEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<FieldLayout>( Equality_FieldLayout, HashCode_FieldLayout );
               _FieldLayoutEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="StandaloneSignature"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="StandaloneSignature"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="StandaloneSignature"/> are considered to be equal by this equality comparer when all the properties of the <see cref="StandaloneSignature"/> are equal.
      /// </remarks>
      public static IEqualityComparer<StandaloneSignature> StandaloneSignatureEqualityComparer
      {
         get
         {
            var retVal = _StandaloneSignatureEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<StandaloneSignature>( Equality_StandaloneSignature, HashCode_StandaloneSignature );
               _StandaloneSignatureEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="EventMap"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="EventMap"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="EventMap"/> are considered to be equal by this equality comparer when all the properties of the <see cref="EventMap"/> are equal.
      /// </remarks>
      public static IEqualityComparer<EventMap> EventMapEqualityComparer
      {
         get
         {
            var retVal = _EventMapEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<EventMap>( Equality_EventMap, HashCode_EventMap );
               _EventMapEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="EventDefinitionPointer"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="EventDefinitionPointer"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="EventDefinitionPointer"/> are considered to be equal by this equality comparer when all the properties of the <see cref="EventDefinitionPointer"/> are equal.
      /// </remarks>
      public static IEqualityComparer<EventDefinitionPointer> EventDefinitionPointerEqualityComparer
      {
         get
         {
            var retVal = _EventDefinitionPointerEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<EventDefinitionPointer>( Equality_EventDefinitionPointer, HashCode_EventDefinitionPointer );
               _EventDefinitionPointerEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="EventDefinition"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="EventDefinition"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="EventDefinition"/> are considered to be equal by this equality comparer when all the properties of the <see cref="EventDefinition"/> are equal.
      /// </remarks>
      public static IEqualityComparer<EventDefinition> EventDefinitionEqualityComparer
      {
         get
         {
            var retVal = _EventDefinitionEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<EventDefinition>( Equality_EventDefinition, HashCode_EventDefinition );
               _EventDefinitionEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="PropertyMap"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="PropertyMap"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="PropertyMap"/> are considered to be equal by this equality comparer when all the properties of the <see cref="PropertyMap"/> are equal.
      /// </remarks>
      public static IEqualityComparer<PropertyMap> PropertyMapEqualityComparer
      {
         get
         {
            var retVal = _PropertyMapEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<PropertyMap>( Equality_PropertyMap, HashCode_PropertyMap );
               _PropertyMapEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="PropertyDefinitionPointer"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="PropertyDefinitionPointer"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="PropertyDefinitionPointer"/> are considered to be equal by this equality comparer when all the properties of the <see cref="PropertyDefinitionPointer"/> are equal.
      /// </remarks>
      public static IEqualityComparer<PropertyDefinitionPointer> PropertyDefinitionPointerEqualityComparer
      {
         get
         {
            var retVal = _PropertyDefinitionPointerEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<PropertyDefinitionPointer>( Equality_PropertyDefinitionPointer, HashCode_PropertyDefinitionPointer );
               _PropertyDefinitionPointerEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="PropertyDefinition"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="PropertyDefinition"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="PropertyDefinition"/> are considered to be equal by this equality comparer when all the properties of the <see cref="PropertyDefinition"/> are equal.
      /// </remarks>
      public static IEqualityComparer<PropertyDefinition> PropertyDefinitionEqualityComparer
      {
         get
         {
            var retVal = _PropertyDefinitionEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<PropertyDefinition>( Equality_PropertyDefinition, HashCode_PropertyDefinition );
               _PropertyDefinitionEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="MethodSemantics"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="MethodSemantics"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="MethodSemantics"/> are considered to be equal by this equality comparer when all the properties of the <see cref="MethodSemantics"/> are equal.
      /// </remarks>
      public static IEqualityComparer<MethodSemantics> MethodSemanticsEqualityComparer
      {
         get
         {
            var retVal = _MethodSemanticsEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<MethodSemantics>( Equality_MethodSemantics, HashCode_MethodSemantics );
               _MethodSemanticsEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="MethodImplementation"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="MethodImplementation"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="MethodImplementation"/> are considered to be equal by this equality comparer when all the properties of the <see cref="MethodImplementation"/> are equal.
      /// </remarks>
      public static IEqualityComparer<MethodImplementation> MethodImplementationEqualityComparer
      {
         get
         {
            var retVal = _MethodImplementationEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<MethodImplementation>( Equality_MethodImplementation, HashCode_MethodImplementation );
               _MethodImplementationEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="ModuleReference"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="ModuleReference"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="ModuleReference"/> are considered to be equal by this equality comparer when all the properties of the <see cref="ModuleReference"/> are equal.
      /// </remarks>
      public static IEqualityComparer<ModuleReference> ModuleReferenceEqualityComparer
      {
         get
         {
            var retVal = _ModuleReferenceEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<ModuleReference>( Equality_ModuleReference, HashCode_ModuleReference );
               _ModuleReferenceEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="TypeSpecification"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="TypeSpecification"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="TypeSpecification"/> are considered to be equal by this equality comparer when all the properties of the <see cref="TypeSpecification"/> are equal.
      /// </remarks>
      public static IEqualityComparer<TypeSpecification> TypeSpecificationEqualityComparer
      {
         get
         {
            var retVal = _TypeSpecificationEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<TypeSpecification>( Equality_TypeSpecification, HashCode_TypeSpecification );
               _TypeSpecificationEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="MethodImplementationMap"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="MethodImplementationMap"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="MethodImplementationMap"/> are considered to be equal by this equality comparer when all the properties of the <see cref="MethodImplementationMap"/> are equal.
      /// </remarks>
      public static IEqualityComparer<MethodImplementationMap> MethodImplementationMapEqualityComparer
      {
         get
         {
            var retVal = _MethodImplementationMapEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<MethodImplementationMap>( Equality_MethodImplementationMap, HashCode_MethodImplementationMap );
               _MethodImplementationMapEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="FieldRVA"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="FieldRVA"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="FieldRVA"/> are considered to be equal by this equality comparer when all the properties of the <see cref="FieldRVA"/> are equal.
      /// </remarks>
      public static IEqualityComparer<FieldRVA> FieldRVAEqualityComparer
      {
         get
         {
            var retVal = _FieldRVAEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<FieldRVA>( Equality_FieldRVA, HashCode_FieldRVA );
               _FieldRVAEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="EditAndContinueLog"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="EditAndContinueLog"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="EditAndContinueLog"/> are considered to be equal by this equality comparer when all the properties of the <see cref="EditAndContinueLog"/> are equal.
      /// </remarks>
      public static IEqualityComparer<EditAndContinueLog> EditAndContinueLogEqualityComparer
      {
         get
         {
            var retVal = _EditAndContinueLogEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<EditAndContinueLog>( Equality_EditAndContinueLog, HashCode_EditAndContinueLog );
               _EditAndContinueLogEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="EditAndContinueMap"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="EditAndContinueMap"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="EditAndContinueMap"/> are considered to be equal by this equality comparer when all the properties of the <see cref="EditAndContinueMap"/> are equal.
      /// </remarks>
      public static IEqualityComparer<EditAndContinueMap> EditAndContinueMapEqualityComparer
      {
         get
         {
            var retVal = _EditAndContinueMapEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<EditAndContinueMap>( Equality_EditAndContinueMap, HashCode_EditAndContinueMap );
               _EditAndContinueMapEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="AssemblyDefinition"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="AssemblyDefinition"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="AssemblyDefinition"/> are considered to be equal by this equality comparer when all the properties of the <see cref="AssemblyDefinition"/> are equal.
      /// </remarks>
      public static IEqualityComparer<AssemblyDefinition> AssemblyDefinitionEqualityComparer
      {
         get
         {
            var retVal = _AssemblyDefinitionEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<AssemblyDefinition>( Equality_AssemblyDefinition, HashCode_AssemblyDefinition );
               _AssemblyDefinitionEqualityComparer = retVal;
            }
            return retVal;
         }
      }

#pragma warning disable 618

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="AssemblyDefinitionProcessor"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="AssemblyDefinitionProcessor"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="AssemblyDefinitionProcessor"/> are considered to be equal by this equality comparer when all the properties of the <see cref="AssemblyDefinitionProcessor"/> are equal.
      /// </remarks>
      public static IEqualityComparer<AssemblyDefinitionProcessor> AssemblyDefinitionProcessorEqualityComparer
      {
         get
         {
            var retVal = _AssemblyDefinitionProcessorEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<AssemblyDefinitionProcessor>( Equality_AssemblyDefinitionProcessor, HashCode_AssemblyDefinitionProcessor );
               _AssemblyDefinitionProcessorEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="AssemblyDefinitionOS"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="AssemblyDefinitionOS"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="AssemblyDefinitionOS"/> are considered to be equal by this equality comparer when all the properties of the <see cref="AssemblyDefinitionOS"/> are equal.
      /// </remarks>
      public static IEqualityComparer<AssemblyDefinitionOS> AssemblyDefinitionOSEqualityComparer
      {
         get
         {
            var retVal = _AssemblyDefinitionOSEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<AssemblyDefinitionOS>( Equality_AssemblyDefinitionOS, HashCode_AssemblyDefinitionOS );
               _AssemblyDefinitionOSEqualityComparer = retVal;
            }
            return retVal;
         }
      }

#pragma warning restore 618

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="AssemblyReference"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="AssemblyReference"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="AssemblyReference"/> are considered to be equal by this equality comparer when all the properties of the <see cref="AssemblyReference"/> are equal.
      /// </remarks>
      public static IEqualityComparer<AssemblyReference> AssemblyReferenceEqualityComparer
      {
         get
         {
            var retVal = _AssemblyReferenceEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<AssemblyReference>( Equality_AssemblyReference, HashCode_AssemblyReference );
               _AssemblyReferenceEqualityComparer = retVal;
            }
            return retVal;
         }
      }

#pragma warning disable 618

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="AssemblyReferenceProcessor"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="AssemblyReferenceProcessor"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="AssemblyReferenceProcessor"/> are considered to be equal by this equality comparer when all the properties of the <see cref="AssemblyReferenceProcessor"/> are equal.
      /// </remarks>
      public static IEqualityComparer<AssemblyReferenceProcessor> AssemblyReferenceProcessorEqualityComparer
      {
         get
         {
            var retVal = _AssemblyReferenceProcessorEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<AssemblyReferenceProcessor>( Equality_AssemblyReferenceProcessor, HashCode_AssemblyReferenceProcessor );
               _AssemblyReferenceProcessorEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="AssemblyReferenceOS"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="AssemblyReferenceOS"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="AssemblyReferenceOS"/> are considered to be equal by this equality comparer when all the properties of the <see cref="AssemblyReferenceOS"/> are equal.
      /// </remarks>
      public static IEqualityComparer<AssemblyReferenceOS> AssemblyReferenceOSEqualityComparer
      {
         get
         {
            var retVal = _AssemblyReferenceOSEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<AssemblyReferenceOS>( Equality_AssemblyReferenceOS, HashCode_AssemblyReferenceOS );
               _AssemblyReferenceOSEqualityComparer = retVal;
            }
            return retVal;
         }
      }

#pragma warning restore 618

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="FileReference"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="FileReference"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="FileReference"/> are considered to be equal by this equality comparer when all the properties of the <see cref="FileReference"/> are equal.
      /// </remarks>
      public static IEqualityComparer<FileReference> FileReferenceEqualityComparer
      {
         get
         {
            var retVal = _FileReferenceEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<FileReference>( Equality_FileReference, HashCode_FileReference );
               _FileReferenceEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="ExportedType"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="ExportedType"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="ExportedType"/> are considered to be equal by this equality comparer when all the properties of the <see cref="ExportedType"/> are equal.
      /// </remarks>
      public static IEqualityComparer<ExportedType> ExportedTypeEqualityComparer
      {
         get
         {
            var retVal = _ExportedTypeEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<ExportedType>( Equality_ExportedType, HashCode_ExportedType );
               _ExportedTypeEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="ManifestResource"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="ManifestResource"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="ManifestResource"/> are considered to be equal by this equality comparer when all the properties of the <see cref="ManifestResource"/> are equal.
      /// </remarks>
      public static IEqualityComparer<ManifestResource> ManifestResourceEqualityComparer
      {
         get
         {
            var retVal = _ManifestResourceEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<ManifestResource>( Equality_ManifestResource, HashCode_ManifestResource );
               _ManifestResourceEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="NestedClassDefinition"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="NestedClassDefinition"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="NestedClassDefinition"/> are considered to be equal by this equality comparer when all the properties of the <see cref="NestedClassDefinition"/> are equal.
      /// </remarks>
      public static IEqualityComparer<NestedClassDefinition> NestedClassDefinitionEqualityComparer
      {
         get
         {
            var retVal = _NestedClassDefinitionEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<NestedClassDefinition>( Equality_NestedClassDefinition, HashCode_NestedClassDefinition );
               _NestedClassDefinitionEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="GenericParameterDefinition"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="GenericParameterDefinition"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="GenericParameterDefinition"/> are considered to be equal by this equality comparer when all the properties of the <see cref="GenericParameterDefinition"/> are equal.
      /// </remarks>
      public static IEqualityComparer<GenericParameterDefinition> GenericParameterDefinitionEqualityComparer
      {
         get
         {
            var retVal = _GenericParameterDefinitionEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<GenericParameterDefinition>( Equality_GenericParameterDefinition, HashCode_GenericParameterDefinition );
               _GenericParameterDefinitionEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="MethodSpecification"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="MethodSpecification"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="MethodSpecification"/> are considered to be equal by this equality comparer when all the properties of the <see cref="MethodSpecification"/> are equal.
      /// </remarks>
      public static IEqualityComparer<MethodSpecification> MethodSpecificationEqualityComparer
      {
         get
         {
            var retVal = _MethodSpecificationEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<MethodSpecification>( Equality_MethodSpecification, HashCode_MethodSpecification );
               _MethodSpecificationEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="GenericParameterConstraintDefinition"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="GenericParameterConstraintDefinition"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="GenericParameterConstraintDefinition"/> are considered to be equal by this equality comparer when all the properties of the <see cref="GenericParameterConstraintDefinition"/> are equal.
      /// </remarks>
      public static IEqualityComparer<GenericParameterConstraintDefinition> GenericParameterConstraintDefinitionEqualityComparer
      {
         get
         {
            var retVal = _GenericParameterConstraintDefinitionEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<GenericParameterConstraintDefinition>( Equality_GenericParameterConstraintDefinition, HashCode_GenericParameterConstraintDefinition );
               _GenericParameterConstraintDefinitionEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="MethodILDefinition"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="MethodILDefinition"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="MethodILDefinition"/> are considered to be equal by this equality comparer when all the properties of the <see cref="MethodILDefinition"/> are equal.
      /// The op-codes are compared using <see cref="OpCodeInfoEqualityComparer"/> and exception blocks using <see cref="MethodExceptionBlockEqualityComparer"/>.
      /// </remarks>
      /// <seealso cref="OpCodeInfoEqualityComparer"/>
      /// <seealso cref="MethodExceptionBlockEqualityComparer"/>
      public static IEqualityComparer<MethodILDefinition> MethodILDefinitionEqualityComparer
      {
         get
         {
            var retVal = _MethodILDefinitionEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<MethodILDefinition>( Equality_MethodILDefinition, HashCode_MethodILDefinition );
               _MethodILDefinitionEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="MethodExceptionBlock"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="MethodExceptionBlock"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="MethodExceptionBlock"/> are considered to be equal by this equality comparer when all the properties of the <see cref="MethodExceptionBlock"/> are equal.
      /// </remarks>
      public static IEqualityComparer<MethodExceptionBlock> MethodExceptionBlockEqualityComparer
      {
         get
         {
            var retVal = _MethodExceptionBlockEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<MethodExceptionBlock>( Equality_MethodExceptionBlock, HashCode_MethodExceptionBlock );
               _MethodExceptionBlockEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="OpCodeInfo"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="OpCodeInfo"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="OpCodeInfo"/> are considered to be equal by this equality comparer when their <see cref="OpCodeInfo.InfoKind"/> match (i.e. they are of the same type) and all the content of the corresponding type deriving from <see cref="OpCodeInfo"/> is equal between the two instances.
      /// </remarks>
      public static IEqualityComparer<OpCodeInfo> OpCodeInfoEqualityComparer
      {
         get
         {
            var retVal = _OpCodeInfoEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<OpCodeInfo>( Equality_OpCodeInfo, HashCode_OpCodeInfo );
               _OpCodeInfoEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="AssemblyInformation"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="AssemblyInformation"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="AssemblyInformation"/> are considered to be equal by this equality comparer when <see cref="AssemblyInformation.Equals(AssemblyInformation)"/> returns <c>true</c>.
      /// </remarks>
      public static IEqualityComparer<AssemblyInformation> AssemblyInformationEqualityComparer
      {
         get
         {
            var retVal = _AssemblyInformationEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<AssemblyInformation>( Equality_AssemblyInformation, HashCode_AssemblyInformation );
               _AssemblyInformationEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="AbstractSignature"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="AbstractSignature"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="AbstractSignature"/> are considered to be equal by this equality comparer when their <see cref="AbstractSignature.SignatureKind"/> match (i.e. they are of the same type) and all the content of the corresponding type deriving from <see cref="AbstractSignature"/> is equal between the two instances.
      /// </remarks>
      public static IEqualityComparer<AbstractSignature> AbstractSignatureEqualityComparer
      {
         get
         {
            var retVal = _AbstractSignatureEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<AbstractSignature>( Equality_AbstractSignature, HashCode_AbstractSignature );
               _AbstractSignatureEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="RawSignature"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="RawSignature"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="RawSignature"/> are considered to be equal by this equality comparer when their <see cref="RawSignature.Bytes"/> arrays are equal.
      /// </remarks>
      public static IEqualityComparer<RawSignature> RawSignatureEqualityComparer
      {
         get
         {
            var retVal = _RawSignatureEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<RawSignature>( Equality_RawSignature, HashCode_RawSignature );
               _RawSignatureEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="AbstractMethodSignature"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="AbstractMethodSignature"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="AbstractMethodSignature"/> are considered to be equal by this equality comparer when their <see cref="AbstractSignature.SignatureKind"/> match (i.e. they are of the same type) and all the content of the corresponding type deriving from <see cref="AbstractMethodSignature"/> is equal between the two instances.
      /// </remarks>
      public static IEqualityComparer<AbstractMethodSignature> AbstractMethodSignatureEqualityComparer
      {
         get
         {
            var retVal = _AbstractMethodSignatureEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<AbstractMethodSignature>( Equality_AbstractMethodSignature, HashCode_AbstractMethodSignature );
               _AbstractMethodSignatureEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="AbstractMethodSignature"/> are equal in a scope of <see cref="AbstractMethodSignature"/> type, i.e. allowing different types of <see cref="AbstractMethodSignature"/> to mach, excluding <see cref="MethodReferenceSignature.VarArgsParameters"/>.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="AbstractMethodSignature"/> are equal in a scope of <see cref="AbstractMethodSignature"/> type.</value>
      /// <remarks>
      /// Two instances of <see cref="AbstractMethodSignature"/> are considered to be equal by this equality comparer when their <see cref="AbstractMethodSignature.GenericArgumentCount"/>, <see cref="AbstractMethodSignature.Parameters"/>, <see cref="AbstractMethodSignature.ReturnType"/>, and <see cref="AbstractMethodSignature.MethodSignatureInformation"/> are equal between the two instances.
      /// </remarks>
      public static IEqualityComparer<AbstractMethodSignature> AbstractMethodSignatureEqualityComparer_IgnoreKind
      {
         get
         {
            var retVal = _AbstractMethodSignatureEqualityComparer_IgnoreKind;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<AbstractMethodSignature>( Equality_AbstractMethodSignature_IgnoreKind, HashCode_AbstractMethodSignature );
               _AbstractMethodSignatureEqualityComparer_IgnoreKind = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="MethodDefinitionSignature"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="MethodDefinitionSignature"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="MethodDefinitionSignature"/> are considered to be equal by this equality comparer when all the properties of the <see cref="MethodDefinitionSignature"/> are equal.
      /// </remarks>
      public static IEqualityComparer<MethodDefinitionSignature> MethodDefinitionSignatureEqualityComparer
      {
         get
         {
            var retVal = _MethodDefinitionSignatureEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<MethodDefinitionSignature>( Equality_MethodDefinitionSignature, HashCode_MethodDefinitionSignature );
               _MethodDefinitionSignatureEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="MethodReferenceSignature"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="MethodReferenceSignature"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="MethodReferenceSignature"/> are considered to be equal by this equality comparer when all the properties of the <see cref="MethodReferenceSignature"/> are equal.
      /// </remarks>
      public static IEqualityComparer<MethodReferenceSignature> MethodReferenceSignatureEqualityComparer
      {
         get
         {
            var retVal = _MethodReferenceSignatureEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<MethodReferenceSignature>( Equality_MethodReferenceSignature, HashCode_MethodReferenceSignature );
               _MethodReferenceSignatureEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="FieldSignature"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="FieldSignature"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="FieldSignature"/> are considered to be equal by this equality comparer when all the properties of the <see cref="FieldSignature"/> are equal.
      /// </remarks>
      public static IEqualityComparer<FieldSignature> FieldSignatureEqualityComparer
      {
         get
         {
            var retVal = _FieldSignatureEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<FieldSignature>( Equality_FieldSignature, HashCode_FieldSignature );
               _FieldSignatureEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="PropertySignature"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="PropertySignature"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="PropertySignature"/> are considered to be equal by this equality comparer when all the properties of the <see cref="PropertySignature"/> are equal.
      /// </remarks>
      public static IEqualityComparer<PropertySignature> PropertySignatureEqualityComparer
      {
         get
         {
            var retVal = _PropertySignatureEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<PropertySignature>( Equality_PropertySignature, HashCode_PropertySignature );
               _PropertySignatureEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="LocalVariablesSignature"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="LocalVariablesSignature"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="LocalVariablesSignature"/> are considered to be equal by this equality comparer when all the properties of the <see cref="LocalVariablesSignature"/> are equal.
      /// </remarks>
      public static IEqualityComparer<LocalVariablesSignature> LocalVariablesSignatureEqualityComparer
      {
         get
         {
            var retVal = _LocalVariablesSignatureEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<LocalVariablesSignature>( Equality_LocalVariablesSignature, HashCode_LocalVariablesSignature );
               _LocalVariablesSignatureEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="LocalVariableSignature"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="LocalVariableSignature"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="LocalVariableSignature"/> are considered to be equal by this equality comparer when all the properties of the <see cref="LocalVariableSignature"/> are equal.
      /// </remarks>
      public static IEqualityComparer<LocalVariableSignature> LocalVariableSignatureEqualityComparer
      {
         get
         {
            var retVal = _LocalVariableSignatureEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<LocalVariableSignature>( Equality_LocalVariableSignature, HashCode_LocalVariableSignature );
               _LocalVariableSignatureEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="ParameterSignature"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="ParameterSignature"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="ParameterSignature"/> are considered to be equal by this equality comparer when all the properties of the <see cref="ParameterSignature"/> are equal.
      /// </remarks>
      public static IEqualityComparer<ParameterSignature> ParameterSignatureEqualityComparer
      {
         get
         {
            var retVal = _ParameterSignatureEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<ParameterSignature>( Equality_ParameterSignature, HashCode_ParameterSignature );
               _ParameterSignatureEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="CustomModifierSignature"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="CustomModifierSignature"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="CustomModifierSignature"/> are considered to be equal by this equality comparer when all the properties of the <see cref="CustomModifierSignature"/> are equal.
      /// </remarks>
      public static IEqualityComparer<CustomModifierSignature> CustomModifierSignatureEqualityComparer
      {
         get
         {
            var retVal = _CustomModifierSignatureEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<CustomModifierSignature>( Equality_CustomModifierSignature, HashCode_CustomModifierSignature );
               _CustomModifierSignatureEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="TypeSignature"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="TypeSignature"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="TypeSignature"/> are considered to be equal by this equality comparer when their <see cref="TypeSignature.TypeSignatureKind"/> match (i.e. they are of the same type) and all the content of the corresponding type deriving from <see cref="TypeSignature"/> is equal between the two instances.
      /// </remarks>
      public static IEqualityComparer<TypeSignature> TypeSignatureEqualityComparer
      {
         get
         {
            var retVal = _TypeSignatureEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<TypeSignature>( Equality_TypeSignature, HashCode_TypeSignature );
               _TypeSignatureEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="GenericMethodSignature"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="GenericMethodSignature"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="GenericMethodSignature"/> are considered to be equal by this equality comparer when all the properties of the <see cref="GenericMethodSignature"/> are equal.
      /// </remarks>
      public static IEqualityComparer<GenericMethodSignature> GenericMethodSignatureEqualityComparer
      {
         get
         {
            var retVal = _GenericMethodSignatureEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<GenericMethodSignature>( Equality_GenericMethodSignature, HashCode_GenericMethodSignature );
               _GenericMethodSignatureEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="AbstractCustomAttributeSignature"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="AbstractCustomAttributeSignature"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="AbstractCustomAttributeSignature"/> are considered to be equal by this equality comparer when their <see cref="AbstractCustomAttributeSignature.CustomAttributeSignatureKind"/> match (i.e. they are of the same type) and all the content of the corresponding type deriving from <see cref="AbstractCustomAttributeSignature"/> is equal between the two instances.
      /// </remarks>
      public static IEqualityComparer<AbstractCustomAttributeSignature> AbstractCustomAttributeSignatureEqualityComparer
      {
         get
         {
            var retVal = _AbstractCustomAttributeSignatureEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<AbstractCustomAttributeSignature>( Equality_AbstractCustomAttributeSignature, HashCode_AbstractCustomAttributeSignature );
               _AbstractCustomAttributeSignatureEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="CustomAttributeTypedArgument"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="CustomAttributeTypedArgument"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="CustomAttributeTypedArgument"/> are considered to be equal by this equality comparer when all the properties of the <see cref="CustomAttributeTypedArgument"/> are equal.
      /// The <see cref="CustomAttributeTypedArgument.Value"/> property is equaled using <see cref="Object.Equals(Object, Object)"/> method, and arrays are equaled recursively using <see cref="E_CommonUtils.ArraysDeepEqualUntyped"/> method.
      /// </remarks>
      public static IEqualityComparer<CustomAttributeTypedArgument> CustomAttributeTypedArgumentEqualityComparer
      {
         get
         {
            var retVal = _CustomAttributeTypedArgumentEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<CustomAttributeTypedArgument>( Equality_CustomAttributeTypedArgument, HashCode_CustomAttributeTypedArgument );
               _CustomAttributeTypedArgumentEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="CustomAttributeNamedArgument"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="CustomAttributeNamedArgument"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="CustomAttributeNamedArgument"/> are considered to be equal by this equality comparer when all the properties of the <see cref="CustomAttributeNamedArgument"/> are equal.
      /// </remarks>
      /// <seealso cref="CustomAttributeTypedArgumentEqualityComparer"/>
      /// <seealso cref="CustomAttributeArgumentTypeEqualityComparer"/>
      public static IEqualityComparer<CustomAttributeNamedArgument> CustomAttributeNamedArgumentEqualityComparer
      {
         get
         {
            var retVal = _CustomAttributeNamedArgumentEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<CustomAttributeNamedArgument>( Equality_CustomAttributeNamedArgument, HashCode_CustomAttributeNamedArgument );
               _CustomAttributeNamedArgumentEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="CustomAttributeArgumentType"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="CustomAttributeArgumentType"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="CustomAttributeArgumentType"/> are considered to be equal by this equality comparer when their <see cref="CustomAttributeArgumentType.ArgumentTypeKind"/> match (i.e. they are of the same type) and all the content of the corresponding type deriving from <see cref="CustomAttributeArgumentType"/> is equal between the two instances.
      /// </remarks>
      public static IEqualityComparer<CustomAttributeArgumentType> CustomAttributeArgumentTypeEqualityComparer
      {
         get
         {
            var retVal = _CustomAttributeArgumentTypeEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<CustomAttributeArgumentType>( Equality_CustomAttributeArgumentType, HashCode_CustomAttributeArgumentType );
               _CustomAttributeArgumentTypeEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="AbstractSecurityInformation"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="AbstractSecurityInformation"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="AbstractSecurityInformation"/> are considered to be equal by this equality comparer when their <see cref="AbstractSecurityInformation.SecurityInformationKind"/> match (i.e. they are of the same type) and all the content of the corresponding type deriving from <see cref="AbstractSecurityInformation"/> is equal between the two instances.
      /// </remarks>
      public static IEqualityComparer<AbstractSecurityInformation> AbstractSecurityInformationEqualityComparer
      {
         get
         {
            var retVal = _AbstractSecurityInformationEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<AbstractSecurityInformation>( Equality_AbstractSecurityInformation, HashCode_AbstractSecurityInformation );
               _AbstractSecurityInformationEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the equality comparer to check whether two instances of <see cref="AbstractMarshalingInfo"/> are exactly equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="AbstractMarshalingInfo"/> are exactly equal.</value>
      /// <remarks>
      /// Two instances of <see cref="AbstractMarshalingInfo"/> are considered to be equal by this equality comparer when their <see cref="AbstractMarshalingInfo.MarshalingInfoKind"/> match (i.e. they are of the same type) and all the content of the corresponding type deriving from <see cref="AbstractMarshalingInfo"/> is equal between the two instances.
      /// </remarks>
      public static IEqualityComparer<AbstractMarshalingInfo> MarshalingInfoEqualityComparer
      {
         get
         {
            var retVal = _MarshalingInfoEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<AbstractMarshalingInfo>( Equality_MarshalingInfo, HashCode_MarshalingInfo );
               _MarshalingInfoEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      private static Boolean Equality_MetaData( CILMetaData x, CILMetaData y )
      {
         // TODO: Table infos actually use these comparers to perform equality & hash code operations
         // Instead, this method should actually use the comparers of table infos
         // Comparers of table infos should be the ones that are currently here.
         // Alternatively, consider merging this class and DefaultMetaDataTableInformationProvider .
#pragma warning disable 618
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && ListEqualityComparer<List<ModuleDefinition>, ModuleDefinition>.Equals( x.ModuleDefinitions.TableContents, y.ModuleDefinitions.TableContents, ModuleDefinitionEqualityComparer )
            && ListEqualityComparer<List<TypeReference>, TypeReference>.Equals( x.TypeReferences.TableContents, y.TypeReferences.TableContents, TypeReferenceEqualityComparer )
            && ListEqualityComparer<List<TypeDefinition>, TypeDefinition>.Equals( x.TypeDefinitions.TableContents, y.TypeDefinitions.TableContents, TypeDefinitionEqualityComparer )
            && ListEqualityComparer<List<FieldDefinitionPointer>, FieldDefinitionPointer>.Equals( x.FieldDefinitionPointers.TableContents, y.FieldDefinitionPointers.TableContents, FieldDefinitionPointerEqualityComparer )
            && ListEqualityComparer<List<FieldDefinition>, FieldDefinition>.Equals( x.FieldDefinitions.TableContents, y.FieldDefinitions.TableContents, FieldDefinitionEqualityComparer )
            && ListEqualityComparer<List<MethodDefinitionPointer>, MethodDefinitionPointer>.Equals( x.MethodDefinitionPointers.TableContents, y.MethodDefinitionPointers.TableContents, MethodDefinitionPointerEqualityComparer )
            && ListEqualityComparer<List<MethodDefinition>, MethodDefinition>.Equals( x.MethodDefinitions.TableContents, y.MethodDefinitions.TableContents, MethodDefinitionEqualityComparer )
            && ListEqualityComparer<List<ParameterDefinitionPointer>, ParameterDefinitionPointer>.Equals( x.ParameterDefinitionPointers.TableContents, y.ParameterDefinitionPointers.TableContents, ParameterDefinitionPointerEqualityComparer )
            && ListEqualityComparer<List<ParameterDefinition>, ParameterDefinition>.Equals( x.ParameterDefinitions.TableContents, y.ParameterDefinitions.TableContents, ParameterDefinitionEqualityComparer )
            && ListEqualityComparer<List<InterfaceImplementation>, InterfaceImplementation>.Equals( x.InterfaceImplementations.TableContents, y.InterfaceImplementations.TableContents, InterfaceImplementationEqualityComparer )
            && ListEqualityComparer<List<MemberReference>, MemberReference>.Equals( x.MemberReferences.TableContents, y.MemberReferences.TableContents, MemberReferenceEqualityComparer )
            && ListEqualityComparer<List<ConstantDefinition>, ConstantDefinition>.Equals( x.ConstantDefinitions.TableContents, y.ConstantDefinitions.TableContents, ConstantDefinitionEqualityComparer )
            && ListEqualityComparer<List<CustomAttributeDefinition>, CustomAttributeDefinition>.Equals( x.CustomAttributeDefinitions.TableContents, y.CustomAttributeDefinitions.TableContents, CustomAttributeDefinitionEqualityComparer )
            && ListEqualityComparer<List<FieldMarshal>, FieldMarshal>.Equals( x.FieldMarshals.TableContents, y.FieldMarshals.TableContents, FieldMarshalEqualityComparer )
            && ListEqualityComparer<List<SecurityDefinition>, SecurityDefinition>.Equals( x.SecurityDefinitions.TableContents, y.SecurityDefinitions.TableContents, SecurityDefinitionEqualityComparer )
            && ListEqualityComparer<List<ClassLayout>, ClassLayout>.Equals( x.ClassLayouts.TableContents, y.ClassLayouts.TableContents, ClassLayoutEqualityComparer )
            && ListEqualityComparer<List<FieldLayout>, FieldLayout>.Equals( x.FieldLayouts.TableContents, y.FieldLayouts.TableContents, FieldLayoutEqualityComparer )
            && ListEqualityComparer<List<StandaloneSignature>, StandaloneSignature>.Equals( x.StandaloneSignatures.TableContents, y.StandaloneSignatures.TableContents, StandaloneSignatureEqualityComparer )
            && ListEqualityComparer<List<EventMap>, EventMap>.Equals( x.EventMaps.TableContents, y.EventMaps.TableContents, EventMapEqualityComparer )
            && ListEqualityComparer<List<EventDefinitionPointer>, EventDefinitionPointer>.Equals( x.EventDefinitionPointers.TableContents, y.EventDefinitionPointers.TableContents, EventDefinitionPointerEqualityComparer )
            && ListEqualityComparer<List<EventDefinition>, EventDefinition>.Equals( x.EventDefinitions.TableContents, y.EventDefinitions.TableContents, EventDefinitionEqualityComparer )
            && ListEqualityComparer<List<PropertyMap>, PropertyMap>.Equals( x.PropertyMaps.TableContents, y.PropertyMaps.TableContents, PropertyMapEqualityComparer )
            && ListEqualityComparer<List<PropertyDefinitionPointer>, PropertyDefinitionPointer>.Equals( x.PropertyDefinitionPointers.TableContents, y.PropertyDefinitionPointers.TableContents, PropertyDefinitionPointerEqualityComparer )
            && ListEqualityComparer<List<PropertyDefinition>, PropertyDefinition>.Equals( x.PropertyDefinitions.TableContents, y.PropertyDefinitions.TableContents, PropertyDefinitionEqualityComparer )
            && ListEqualityComparer<List<MethodSemantics>, MethodSemantics>.Equals( x.MethodSemantics.TableContents, y.MethodSemantics.TableContents, MethodSemanticsEqualityComparer )
            && ListEqualityComparer<List<MethodImplementation>, MethodImplementation>.Equals( x.MethodImplementations.TableContents, y.MethodImplementations.TableContents, MethodImplementationEqualityComparer )
            && ListEqualityComparer<List<ModuleReference>, ModuleReference>.Equals( x.ModuleReferences.TableContents, y.ModuleReferences.TableContents, ModuleReferenceEqualityComparer )
            && ListEqualityComparer<List<TypeSpecification>, TypeSpecification>.Equals( x.TypeSpecifications.TableContents, y.TypeSpecifications.TableContents, TypeSpecificationEqualityComparer )
            && ListEqualityComparer<List<MethodImplementationMap>, MethodImplementationMap>.Equals( x.MethodImplementationMaps.TableContents, y.MethodImplementationMaps.TableContents, MethodImplementationMapEqualityComparer )
            && ListEqualityComparer<List<FieldRVA>, FieldRVA>.Equals( x.FieldRVAs.TableContents, y.FieldRVAs.TableContents, FieldRVAEqualityComparer )
            && ListEqualityComparer<List<EditAndContinueLog>, EditAndContinueLog>.Equals( x.EditAndContinueLog.TableContents, y.EditAndContinueLog.TableContents, EditAndContinueLogEqualityComparer )
            && ListEqualityComparer<List<EditAndContinueMap>, EditAndContinueMap>.Equals( x.EditAndContinueMap.TableContents, y.EditAndContinueMap.TableContents, EditAndContinueMapEqualityComparer )
            && ListEqualityComparer<List<AssemblyDefinition>, AssemblyDefinition>.Equals( x.AssemblyDefinitions.TableContents, y.AssemblyDefinitions.TableContents, AssemblyDefinitionEqualityComparer )
            && ListEqualityComparer<List<AssemblyDefinitionProcessor>, AssemblyDefinitionProcessor>.Equals( x.AssemblyDefinitionProcessors.TableContents, y.AssemblyDefinitionProcessors.TableContents, AssemblyDefinitionProcessorEqualityComparer )
            && ListEqualityComparer<List<AssemblyDefinitionOS>, AssemblyDefinitionOS>.Equals( x.AssemblyDefinitionOSs.TableContents, y.AssemblyDefinitionOSs.TableContents, AssemblyDefinitionOSEqualityComparer )
            && ListEqualityComparer<List<AssemblyReference>, AssemblyReference>.Equals( x.AssemblyReferences.TableContents, y.AssemblyReferences.TableContents, AssemblyReferenceEqualityComparer )
            && ListEqualityComparer<List<AssemblyReferenceProcessor>, AssemblyReferenceProcessor>.Equals( x.AssemblyReferenceProcessors.TableContents, y.AssemblyReferenceProcessors.TableContents, AssemblyReferenceProcessorEqualityComparer )
            && ListEqualityComparer<List<AssemblyReferenceOS>, AssemblyReferenceOS>.Equals( x.AssemblyReferenceOSs.TableContents, y.AssemblyReferenceOSs.TableContents, AssemblyReferenceOSEqualityComparer )
            && ListEqualityComparer<List<FileReference>, FileReference>.Equals( x.FileReferences.TableContents, y.FileReferences.TableContents, FileReferenceEqualityComparer )
            && ListEqualityComparer<List<ExportedType>, ExportedType>.Equals( x.ExportedTypes.TableContents, y.ExportedTypes.TableContents, ExportedTypeEqualityComparer )
            && ListEqualityComparer<List<ManifestResource>, ManifestResource>.Equals( x.ManifestResources.TableContents, y.ManifestResources.TableContents, ManifestResourceEqualityComparer )
            && ListEqualityComparer<List<NestedClassDefinition>, NestedClassDefinition>.Equals( x.NestedClassDefinitions.TableContents, y.NestedClassDefinitions.TableContents, NestedClassDefinitionEqualityComparer )
            && ListEqualityComparer<List<GenericParameterDefinition>, GenericParameterDefinition>.Equals( x.GenericParameterDefinitions.TableContents, y.GenericParameterDefinitions.TableContents, GenericParameterDefinitionEqualityComparer )
            && ListEqualityComparer<List<MethodSpecification>, MethodSpecification>.Equals( x.MethodSpecifications.TableContents, y.MethodSpecifications.TableContents, MethodSpecificationEqualityComparer )
            && ListEqualityComparer<List<GenericParameterConstraintDefinition>, GenericParameterConstraintDefinition>.Equals( x.GenericParameterConstraintDefinitions.TableContents, y.GenericParameterConstraintDefinitions.TableContents, GenericParameterConstraintDefinitionEqualityComparer )
            && SequenceEqualityComparer<IEnumerable<MetaDataTable>, MetaDataTable>.SequenceEquality(
               x.GetAdditionalTables(),
               y.GetAdditionalTables(),
               ( xa, ya ) => SequenceEqualityComparer<IEnumerable<Object>, Object>.SequenceEquality(
                  xa.TableContentsNotGeneric.Cast<Object>(),
                  ya.TableContentsNotGeneric.Cast<Object>(),
                  xa.TableInformationNotGeneric.EqualityComparerNotGeneric.Equals
               ) )
            );
#pragma warning restore 618
      }

      private static Boolean Equality_ModuleDefinition( ModuleDefinition x, ModuleDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.Generation == y.Generation
            && NullableEqualityComparer<Guid>.Equals( x.ModuleGUID, y.ModuleGUID )
            && NullableEqualityComparer<Guid>.Equals( x.EditAndContinueGUID, y.EditAndContinueGUID )
            && NullableEqualityComparer<Guid>.Equals( x.EditAndContinueBaseGUID, y.EditAndContinueBaseGUID )
            );
      }

      private static Boolean Equality_TypeReference( TypeReference x, TypeReference y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && String.Equals( x.Namespace, y.Namespace )
            && NullableEqualityComparer<TableIndex>.Equals( x.ResolutionScope, y.ResolutionScope )
            );
      }

      private static Boolean Equality_TypeDefinition( TypeDefinition x, TypeDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && String.Equals( x.Namespace, y.Namespace )
            && x.Attributes == y.Attributes
            && NullableEqualityComparer<TableIndex>.Equals( x.BaseType, y.BaseType )
            && x.FieldList.Equals( y.FieldList )
            && x.MethodList.Equals( y.MethodList )
            );
      }

      private static Boolean Equality_FieldDefinitionPointer( FieldDefinitionPointer x, FieldDefinitionPointer y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null && x.FieldIndex.Equals( y.FieldIndex ) );
      }


      private static Boolean Equality_FieldDefinition( FieldDefinition x, FieldDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.Attributes == y.Attributes
            && FieldSignatureEqualityComparer.Equals( x.Signature, y.Signature )
            );
      }

      private static Boolean Equality_MethodDefinitionPointer( MethodDefinitionPointer x, MethodDefinitionPointer y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null && x.MethodIndex.Equals( y.MethodIndex ) );
      }

      private static Boolean Equality_MethodDefinition( MethodDefinition x, MethodDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.Attributes == y.Attributes
            && x.ImplementationAttributes == y.ImplementationAttributes
            && x.ParameterList == y.ParameterList
            && MethodDefinitionSignatureEqualityComparer.Equals( x.Signature, y.Signature )
            && Equality_MethodILDefinition( x.IL, y.IL )
            );
      }

      private static Boolean Equality_ParameterDefinitionPointer( ParameterDefinitionPointer x, ParameterDefinitionPointer y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null && x.ParameterIndex.Equals( y.ParameterIndex ) );
      }

      private static Boolean Equality_ParameterDefinition( ParameterDefinition x, ParameterDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Sequence == y.Sequence
            && String.Equals( x.Name, y.Name )
            && x.Attributes == y.Attributes
             );
      }

      private static Boolean Equality_InterfaceImplementation( InterfaceImplementation x, InterfaceImplementation y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Class == y.Class
             && x.Interface == y.Interface
             );
      }

      private static Boolean Equality_MemberReference( MemberReference x, MemberReference y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && String.Equals( x.Name, y.Name )
             && x.DeclaringType == y.DeclaringType
             && AbstractSignatureEqualityComparer.Equals( x.Signature, y.Signature )
             );
      }

      private static Boolean Equality_ConstantDefinition( ConstantDefinition x, ConstantDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Type == y.Type
             && x.Parent == y.Parent
             && Object.Equals( x.Value, y.Value )
             );
      }

      private static Boolean Equality_CustomAttributeDefinition( CustomAttributeDefinition x, CustomAttributeDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Parent == y.Parent
             && x.Type == y.Type
             && AbstractCustomAttributeSignatureEqualityComparer.Equals( x.Signature, y.Signature )
             );
      }

      private static Boolean Equality_FieldMarshal( FieldMarshal x, FieldMarshal y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Parent == y.Parent
             && MarshalingInfoEqualityComparer.Equals( x.NativeType, y.NativeType )
             );
      }

      private static Boolean Equality_SecurityDefinition( SecurityDefinition x, SecurityDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Parent == y.Parent
             && x.Action == y.Action
             && ListEqualityComparer<List<AbstractSecurityInformation>, AbstractSecurityInformation>.Equals( x.PermissionSets, y.PermissionSets, AbstractSecurityInformationEqualityComparer )
             );
      }

      private static Boolean Equality_ClassLayout( ClassLayout x, ClassLayout y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Parent == y.Parent
             && x.PackingSize == y.PackingSize
             && x.ClassSize == y.ClassSize
             );
      }

      private static Boolean Equality_FieldLayout( FieldLayout x, FieldLayout y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Field == y.Field
             && x.Offset == y.Offset
             );
      }

      private static Boolean Equality_StandaloneSignature( StandaloneSignature x, StandaloneSignature y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.StoreSignatureAsFieldSignature == y.StoreSignatureAsFieldSignature
             && AbstractSignatureEqualityComparer.Equals( x.Signature, y.Signature )
             );
      }

      private static Boolean Equality_EventMap( EventMap x, EventMap y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Parent == y.Parent
             && x.EventList == y.EventList
             );
      }

      private static Boolean Equality_EventDefinitionPointer( EventDefinitionPointer x, EventDefinitionPointer y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null && x.EventIndex.Equals( y.EventIndex ) );
      }

      private static Boolean Equality_EventDefinition( EventDefinition x, EventDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && String.Equals( x.Name, y.Name )
             && x.Attributes == y.Attributes
             && x.EventType == y.EventType
             );
      }

      private static Boolean Equality_PropertyMap( PropertyMap x, PropertyMap y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Parent == y.Parent
             && x.PropertyList == y.PropertyList
             );
      }

      private static Boolean Equality_PropertyDefinitionPointer( PropertyDefinitionPointer x, PropertyDefinitionPointer y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null && x.PropertyIndex.Equals( y.PropertyIndex ) );
      }

      private static Boolean Equality_PropertyDefinition( PropertyDefinition x, PropertyDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && String.Equals( x.Name, y.Name )
             && x.Attributes == y.Attributes
             && PropertySignatureEqualityComparer.Equals( x.Signature, y.Signature )
             );
      }

      private static Boolean Equality_MethodSemantics( MethodSemantics x, MethodSemantics y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Attributes == y.Attributes
             && x.Method == y.Method
             && x.Associaton == y.Associaton
             );
      }

      private static Boolean Equality_MethodImplementation( MethodImplementation x, MethodImplementation y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Class == y.Class
             && x.MethodBody == y.MethodBody
             && x.MethodDeclaration == y.MethodDeclaration
             );
      }

      private static Boolean Equality_ModuleReference( ModuleReference x, ModuleReference y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && String.Equals( x.ModuleName, y.ModuleName )
             );
      }

      private static Boolean Equality_TypeSpecification( TypeSpecification x, TypeSpecification y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && TypeSignatureEqualityComparer.Equals( x.Signature, y.Signature )
             );
      }

      private static Boolean Equality_MethodImplementationMap( MethodImplementationMap x, MethodImplementationMap y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && String.Equals( x.ImportName, y.ImportName )
             && x.MemberForwarded == y.MemberForwarded
             && x.Attributes == y.Attributes
             && x.ImportScope == y.ImportScope
             );
      }

      private static Boolean Equality_FieldRVA( FieldRVA x, FieldRVA y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Field == y.Field
             && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.Data, y.Data )
             );
      }

      private static Boolean Equality_EditAndContinueLog( EditAndContinueLog x, EditAndContinueLog y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Token == y.Token
            && x.FuncCode == y.FuncCode
            );
      }

      private static Boolean Equality_EditAndContinueMap( EditAndContinueMap x, EditAndContinueMap y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Token == y.Token
            );
      }

      private static Boolean Equality_AssemblyDefinition( AssemblyDefinition x, AssemblyDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.AssemblyInformation.Equals( y.AssemblyInformation )
             && x.Attributes == y.Attributes
             && x.HashAlgorithm == y.HashAlgorithm
             );
      }

#pragma warning disable 618

      private static Boolean Equality_AssemblyDefinitionProcessor( AssemblyDefinitionProcessor x, AssemblyDefinitionProcessor y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Processor == y.Processor
            );
      }

      private static Boolean Equality_AssemblyDefinitionOS( AssemblyDefinitionOS x, AssemblyDefinitionOS y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.OSPlatformID == y.OSPlatformID
            && x.OSMajorVersion == y.OSMajorVersion
            && x.OSMinorVersion == y.OSMinorVersion
            );
      }

#pragma warning restore 618

      private static Boolean Equality_AssemblyReference( AssemblyReference x, AssemblyReference y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.AssemblyInformation.Equals( y.AssemblyInformation )
             && x.Attributes == y.Attributes
             && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.HashValue, y.HashValue )
             );
      }

#pragma warning disable 618

      private static Boolean Equality_AssemblyReferenceProcessor( AssemblyReferenceProcessor x, AssemblyReferenceProcessor y )
      {
         return Object.ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.Processor == y.Processor
            && x.AssemblyRef.Equals( y.AssemblyRef )
            );
      }

      private static Boolean Equality_AssemblyReferenceOS( AssemblyReferenceOS x, AssemblyReferenceOS y )
      {
         return Object.ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.OSPlatformID == y.OSPlatformID
            && x.OSMajorVersion == y.OSMajorVersion
            && x.OSMinorVersion == y.OSMinorVersion
            && x.AssemblyRef == y.AssemblyRef
            );
      }

#pragma warning restore 618

      private static Boolean Equality_FileReference( FileReference x, FileReference y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && String.Equals( x.Name, y.Name )
             && x.Attributes == y.Attributes
             && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.HashValue, y.HashValue )
             );
      }

      private static Boolean Equality_ExportedType( ExportedType x, ExportedType y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && String.Equals( x.Name, y.Name )
             && String.Equals( x.Namespace, y.Namespace )
             && x.Attributes == y.Attributes
             && x.Implementation == y.Implementation
             && x.TypeDefinitionIndex == y.TypeDefinitionIndex
             );
      }

      private static Boolean Equality_ManifestResource( ManifestResource x, ManifestResource y )
      {
         var retVal = Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && String.Equals( x.Name, y.Name )
             && NullableEqualityComparer<TableIndex>.Equals( x.Implementation, y.Implementation )
             && ( !x.Implementation.HasValue || x.Offset == y.Offset )
             && x.Attributes == y.Attributes
             && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.DataInCurrentFile, y.DataInCurrentFile )
             );

         return retVal;
      }

      private static Boolean Equality_NestedClassDefinition( NestedClassDefinition x, NestedClassDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.NestedClass == y.NestedClass
             && x.EnclosingClass == y.EnclosingClass
             );
      }

      private static Boolean Equality_GenericParameterDefinition( GenericParameterDefinition x, GenericParameterDefinition y )
      {
         return ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.GenericParameterIndex == y.GenericParameterIndex
             && String.Equals( x.Name, y.Name )
             && x.Owner == y.Owner
             && x.Attributes == y.Attributes
             );
      }

      private static Boolean Equality_MethodSpecification( MethodSpecification x, MethodSpecification y )
      {
         return ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Method == y.Method
             && GenericMethodSignatureEqualityComparer.Equals( x.Signature, y.Signature )
             );
      }

      private static Boolean Equality_GenericParameterConstraintDefinition( GenericParameterConstraintDefinition x, GenericParameterConstraintDefinition y )
      {
         return ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Owner == y.Owner
             && x.Constraint == y.Constraint
             );
      }

      private static Boolean Equality_MethodILDefinition( MethodILDefinition x, MethodILDefinition y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && NullableEqualityComparer<TableIndex>.Equals( x.LocalsSignatureIndex, y.LocalsSignatureIndex )
            && ListEqualityComparer<List<OpCodeInfo>, OpCodeInfo>.ListEquality( x.OpCodes, y.OpCodes, Equality_OpCodeInfo )
            && ListEqualityComparer<List<MethodExceptionBlock>, MethodExceptionBlock>.ListEquality( x.ExceptionBlocks, y.ExceptionBlocks, Equality_MethodExceptionBlock )
            && x.InitLocals == y.InitLocals
            && x.MaxStackSize == y.MaxStackSize
            );
      }

      private static Boolean Equality_MethodExceptionBlock( MethodExceptionBlock x, MethodExceptionBlock y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.BlockType == y.BlockType
            && x.TryOffset == y.TryOffset
            && x.TryLength == y.TryLength
            && x.HandlerOffset == y.HandlerOffset
            && x.HandlerLength == y.HandlerLength
            && NullableEqualityComparer<TableIndex>.Equals( x.ExceptionType, y.ExceptionType )
            && x.FilterOffset == y.FilterOffset
            );
      }

      private static Boolean Equality_OpCodeInfo( OpCodeInfo x, OpCodeInfo y )
      {
         var retVal = Object.ReferenceEquals( x, y );
         if ( !retVal && x != null && y != null && x.OpCode == y.OpCode && x.InfoKind == y.InfoKind )
         {
            switch ( x.InfoKind )
            {
               case OpCodeOperandKind.OperandInteger:
                  retVal = ( (OpCodeInfoWithInt32) x ).Operand == ( (OpCodeInfoWithInt32) y ).Operand;
                  break;
               case OpCodeOperandKind.OperandInteger64:
                  retVal = ( (OpCodeInfoWithInt64) x ).Operand == ( (OpCodeInfoWithInt64) y ).Operand;
                  break;
               case OpCodeOperandKind.OperandNone:
                  retVal = true;
                  break;
               case OpCodeOperandKind.OperandR4:
                  // Use .Equals in order for NaN's to work more intuitively
                  retVal = ( (OpCodeInfoWithSingle) x ).Operand.Equals( ( (OpCodeInfoWithSingle) y ).Operand );
                  break;
               case OpCodeOperandKind.OperandR8:
                  // Use .Equals in order for NaN's to work more intuitively
                  retVal = ( (OpCodeInfoWithDouble) x ).Operand.Equals( ( (OpCodeInfoWithDouble) y ).Operand );
                  break;
               case OpCodeOperandKind.OperandString:
                  retVal = String.Equals( ( (OpCodeInfoWithString) x ).Operand, ( (OpCodeInfoWithString) y ).Operand );
                  break;
               case OpCodeOperandKind.OperandIntegerList:
                  retVal = ListEqualityComparer<List<Int32>, Int32>.ListEquality( ( (OpCodeInfoWithIntegers) x ).Operand, ( (OpCodeInfoWithIntegers) y ).Operand );
                  break;
               case OpCodeOperandKind.OperandTableIndex:
                  retVal = ( (OpCodeInfoWithTableIndex) x ).Operand == ( (OpCodeInfoWithTableIndex) y ).Operand;
                  break;
            }
         }
         return retVal;
      }

      private static Boolean Equality_AssemblyInformation( AssemblyInformation x, AssemblyInformation y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && x.Equals( y ) );
      }

      private static Boolean Equality_AbstractSignature( AbstractSignature x, AbstractSignature y )
      {
         var retVal = Object.ReferenceEquals( x, y );
         if ( !retVal && x != null && y != null && x.SignatureKind == y.SignatureKind )
         {
            switch ( x.SignatureKind )
            {
               case SignatureKind.Field:
                  retVal = FieldSignatureEqualityComparer.Equals( x as FieldSignature, y as FieldSignature );
                  break;
               case SignatureKind.GenericMethodInstantiation:
                  retVal = GenericMethodSignatureEqualityComparer.Equals( x as GenericMethodSignature, y as GenericMethodSignature );
                  break;
               case SignatureKind.LocalVariables:
                  retVal = LocalVariablesSignatureEqualityComparer.Equals( x as LocalVariablesSignature, y as LocalVariablesSignature );
                  break;
               case SignatureKind.MethodDefinition:
                  retVal = MethodDefinitionSignatureEqualityComparer.Equals( x as MethodDefinitionSignature, y as MethodDefinitionSignature );
                  break;
               case SignatureKind.MethodReference:
                  retVal = MethodReferenceSignatureEqualityComparer.Equals( x as MethodReferenceSignature, y as MethodReferenceSignature );
                  break;
               case SignatureKind.Property:
                  retVal = PropertySignatureEqualityComparer.Equals( x as PropertySignature, y as PropertySignature );
                  break;
               case SignatureKind.Type:
                  retVal = TypeSignatureEqualityComparer.Equals( x as TypeSignature, y as TypeSignature );
                  break;
               case SignatureKind.RawSignature:
                  retVal = RawSignatureEqualityComparer.Equals( x as RawSignature, y as RawSignature );
                  break;
            }
         }
         return retVal;
      }

      private static Boolean Equality_RawSignature( RawSignature x, RawSignature y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( x != null && y != null
            && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.Bytes, y.Bytes )
         );
      }

      private static Boolean Equality_AbstractMethodSignature( AbstractMethodSignature x, AbstractMethodSignature y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( x != null && y != null
            && x.SignatureKind == y.SignatureKind
            && ( ( x as MethodDefinitionSignature == null ) ?
               MethodReferenceSignatureEqualityComparer.Equals( x as MethodReferenceSignature, y as MethodReferenceSignature ) :
               MethodDefinitionSignatureEqualityComparer.Equals( x as MethodDefinitionSignature, y as MethodDefinitionSignature )
               )
         );
      }

      private static Boolean Equality_AbstractMethodSignature_IgnoreKind( AbstractMethodSignature x, AbstractMethodSignature y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( Equality_AbstractMethodSignature_NoReferenceEquals( x, y )
            && (
               x.SignatureKind != y.SignatureKind
               || x.SignatureKind != SignatureKind.MethodReference
               || Equality_ParameterSignatures( ( (MethodReferenceSignature) x ).VarArgsParameters, ( (MethodReferenceSignature) y ).VarArgsParameters )
               )
            );
      }

      private static Boolean Equality_ParameterSignatures( List<ParameterSignature> x, List<ParameterSignature> y )
      {
         return ListEqualityComparer<List<ParameterSignature>, ParameterSignature>.Equals( x, y, ParameterSignatureEqualityComparer );
      }

      private static Boolean Equality_AbstractMethodSignature_NoReferenceEquals( AbstractMethodSignature x, AbstractMethodSignature y )
      {
         return
            ( x != null && y != null
            && x.MethodSignatureInformation == y.MethodSignatureInformation
            && x.GenericArgumentCount == y.GenericArgumentCount
            && ParameterSignatureEqualityComparer.Equals( x.ReturnType, y.ReturnType )
            && Equality_ParameterSignatures( x.Parameters, y.Parameters )
            );
      }

      private static Boolean Equality_MethodDefinitionSignature( MethodDefinitionSignature x, MethodDefinitionSignature y )
      {
         return Object.ReferenceEquals( x, y ) || Equality_AbstractMethodSignature_NoReferenceEquals( x, y );
      }

      private static Boolean Equality_MethodReferenceSignature( MethodReferenceSignature x, MethodReferenceSignature y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( Equality_AbstractMethodSignature_NoReferenceEquals( x, y )
            && Equality_ParameterSignatures( x.VarArgsParameters, y.VarArgsParameters )
         );
      }

      private static Boolean Equality_CustomModifierSignatures( List<CustomModifierSignature> x, List<CustomModifierSignature> y )
      {
         return ListEqualityComparer<List<CustomModifierSignature>, CustomModifierSignature>.Equals( x, y, CustomModifierSignatureEqualityComparer );
      }

      private static Boolean Equality_FieldSignature( FieldSignature x, FieldSignature y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( x != null && y != null
            && TypeSignatureEqualityComparer.Equals( x.Type, y.Type )
            && Equality_CustomModifierSignatures( x.CustomModifiers, y.CustomModifiers )
         );
      }

      private static Boolean Equality_PropertySignature( PropertySignature x, PropertySignature y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( x != null && y != null
            && x.HasThis == y.HasThis
            && TypeSignatureEqualityComparer.Equals( x.PropertyType, y.PropertyType )
            && Equality_ParameterSignatures( x.Parameters, y.Parameters )
            && Equality_CustomModifierSignatures( x.CustomModifiers, y.CustomModifiers )
         );
      }

      private static Boolean Equality_ParameterOrLocalVariableSignature_NoReferenceEquals( ParameterOrLocalVariableSignature x, ParameterOrLocalVariableSignature y )
      {
         return x != null && y != null
            && TypeSignatureEqualityComparer.Equals( x.Type, y.Type )
            && x.IsByRef == y.IsByRef
            && Equality_CustomModifierSignatures( x.CustomModifiers, y.CustomModifiers );
      }

      private static Boolean Equality_LocalVariablesSignature( LocalVariablesSignature x, LocalVariablesSignature y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( x != null && y != null
            && ListEqualityComparer<List<LocalVariableSignature>, LocalVariableSignature>.Equals( x.Locals, y.Locals, LocalVariableSignatureEqualityComparer )
         );
      }

      private static Boolean Equality_LocalVariableSignature( LocalVariableSignature x, LocalVariableSignature y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( Equality_ParameterOrLocalVariableSignature_NoReferenceEquals( x, y )
            && x.IsPinned == y.IsPinned
         );
      }

      private static Boolean Equality_ParameterSignature( ParameterSignature x, ParameterSignature y )
      {
         return Object.ReferenceEquals( x, y ) || Equality_ParameterOrLocalVariableSignature_NoReferenceEquals( x, y );
      }

      private static Boolean Equality_CustomModifierSignature( CustomModifierSignature x, CustomModifierSignature y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( x != null && y != null
            && x.CustomModifierType == y.CustomModifierType
            && x.IsOptional == y.IsOptional
         );
      }

      private static Boolean Equality_TypeSignature( TypeSignature x, TypeSignature y )
      {
         var retVal = Object.ReferenceEquals( x, y );
         if ( !retVal && x != null && y != null && x.TypeSignatureKind == y.TypeSignatureKind )
         {
            switch ( x.TypeSignatureKind )
            {
               case TypeSignatureKind.Simple:
                  retVal = Equality_SimpleTypeSignature_NoReferenceEquals( x as SimpleTypeSignature, y as SimpleTypeSignature );
                  break;
               case TypeSignatureKind.ClassOrValue:
                  retVal = Equality_ClassOrValueTypeSignature_NoReferenceEquals( x as ClassOrValueTypeSignature, y as ClassOrValueTypeSignature );
                  break;
               case TypeSignatureKind.GenericParameter:
                  retVal = Equality_GenericParameterTypeSignature_NoReferenceEquals( x as GenericParameterTypeSignature, y as GenericParameterTypeSignature );
                  break;
               case TypeSignatureKind.FunctionPointer:
                  retVal = Equality_FunctionPointerTypeSignature_NoReferenceEquals( x as FunctionPointerTypeSignature, y as FunctionPointerTypeSignature );
                  break;
               case TypeSignatureKind.Pointer:
                  retVal = Equality_PointerTypeSignature_NoReferenceEquals( x as PointerTypeSignature, y as PointerTypeSignature );
                  break;
               case TypeSignatureKind.ComplexArray:
                  retVal = Equality_ComplexArrayTypeSignature_NoReferenceEquals( x as ComplexArrayTypeSignature, y as ComplexArrayTypeSignature );
                  break;
               case TypeSignatureKind.SimpleArray:
                  retVal = Equality_SimpleArrayTypeSignature_NoReferenceEquals( x as SimpleArrayTypeSignature, y as SimpleArrayTypeSignature );
                  break;
            }
         }

         return retVal;
      }

      private static Boolean Equality_SimpleTypeSignature_NoReferenceEquals( SimpleTypeSignature x, SimpleTypeSignature y )
      {
         return x != null && y != null
            && x.SimpleType == y.SimpleType;
      }

      private static Boolean Equality_ClassOrValueTypeSignature_NoReferenceEquals( ClassOrValueTypeSignature x, ClassOrValueTypeSignature y )
      {
         return x != null && y != null
            && x.Type == y.Type
            && x.IsClass == y.IsClass
            && ListEqualityComparer<List<TypeSignature>, TypeSignature>.Equals( x.GenericArguments, y.GenericArguments, TypeSignatureEqualityComparer );
      }

      private static Boolean Equality_GenericParameterTypeSignature_NoReferenceEquals( GenericParameterTypeSignature x, GenericParameterTypeSignature y )
      {
         return x != null && y != null
            && x.GenericParameterIndex == y.GenericParameterIndex
            && x.IsTypeParameter == y.IsTypeParameter;
      }

      private static Boolean Equality_FunctionPointerTypeSignature_NoReferenceEquals( FunctionPointerTypeSignature x, FunctionPointerTypeSignature y )
      {
         return x != null && y != null
            && MethodReferenceSignatureEqualityComparer.Equals( x.MethodSignature, y.MethodSignature );
      }

      private static Boolean Equality_PointerTypeSignature_NoReferenceEquals( PointerTypeSignature x, PointerTypeSignature y )
      {
         return x != null && y != null
            && TypeSignatureEqualityComparer.Equals( x.PointerType, y.PointerType )
            && Equality_CustomModifierSignatures( x.CustomModifiers, y.CustomModifiers );
      }

      private static Boolean Equality_AbstractArrayTypeSignature_NoReferenceEquals( AbstractArrayTypeSignature x, AbstractArrayTypeSignature y )
      {
         return x != null && y != null && TypeSignatureEqualityComparer.Equals( x.ArrayType, y.ArrayType );
      }

      private static Boolean Equality_ComplexArrayTypeSignature_NoReferenceEquals( ComplexArrayTypeSignature x, ComplexArrayTypeSignature y )
      {
         return Equality_AbstractArrayTypeSignature_NoReferenceEquals( x, y )
            && x.Rank == y.Rank
            && ListEqualityComparer<List<Int32>, Int32>.DefaultListEqualityComparer.Equals( x.Sizes, y.Sizes )
            && ListEqualityComparer<List<Int32>, Int32>.DefaultListEqualityComparer.Equals( x.LowerBounds, y.LowerBounds );
      }

      private static Boolean Equality_SimpleArrayTypeSignature_NoReferenceEquals( SimpleArrayTypeSignature x, SimpleArrayTypeSignature y )
      {
         return Equality_AbstractArrayTypeSignature_NoReferenceEquals( x, y )
            && Equality_CustomModifierSignatures( x.CustomModifiers, y.CustomModifiers );
      }

      private static Boolean Equality_GenericMethodSignature( GenericMethodSignature x, GenericMethodSignature y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( x != null && y != null
            && ListEqualityComparer<List<TypeSignature>, TypeSignature>.Equals( x.GenericArguments, y.GenericArguments, TypeSignatureEqualityComparer )
         );
      }

      private static Boolean Equality_AbstractCustomAttributeSignature( AbstractCustomAttributeSignature x, AbstractCustomAttributeSignature y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( x is CustomAttributeSignature ?
            Equality_CustomAttributeSignature_NoReferenceEquals( x as CustomAttributeSignature, y as CustomAttributeSignature ) :
            Equality_RawCustomAttributeSignature_NoReferenceEquals( x as RawCustomAttributeSignature, y as RawCustomAttributeSignature )
         );
      }

      private static Boolean Equality_RawCustomAttributeSignature_NoReferenceEquals( RawCustomAttributeSignature x, RawCustomAttributeSignature y )
      {
         return x != null && y != null
            && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.Bytes, y.Bytes );
      }

      private static Boolean Equality_CustomAttributeSignature_NoReferenceEquals( CustomAttributeSignature x, CustomAttributeSignature y )
      {
         return x != null && y != null
            && ListEqualityComparer<List<CustomAttributeTypedArgument>, CustomAttributeTypedArgument>.Equals( x.TypedArguments, y.TypedArguments, CustomAttributeTypedArgumentEqualityComparer )
            // TODO should we use set-comparer for named args? since order most likely shouldn't matter...
            && ListEqualityComparer<List<CustomAttributeNamedArgument>, CustomAttributeNamedArgument>.Equals( x.NamedArguments, y.NamedArguments, CustomAttributeNamedArgumentEqualityComparer );
      }

      private static Boolean Equality_CustomAttributeTypedArgument( CustomAttributeTypedArgument x, CustomAttributeTypedArgument y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( x != null && y != null
            && Equality_CustomAttributeValue( x.Value, y.Value )
         );
      }

      private static Boolean Equality_CustomAttributeValue( Object x, Object y )
      {
         return Object.Equals( x, y ) || ( x as Array ).ArraysDeepEqualUntyped( y as Array, Equality_CustomAttributeValue );
      }

      private static Boolean Equality_CustomAttributeNamedArgument( CustomAttributeNamedArgument x, CustomAttributeNamedArgument y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( x != null && y != null
            && x.IsField == y.IsField
            && String.Equals( x.Name, y.Name )
            && Equality_CustomAttributeArgumentType( x.FieldOrPropertyType, y.FieldOrPropertyType )
            && Equality_CustomAttributeTypedArgument( x.Value, y.Value ) // Optimize a bit - don't use CustomAttributeTypedArgumentEqualityComparer property
         );
      }

      private static Boolean Equality_CustomAttributeArgumentType( CustomAttributeArgumentType x, CustomAttributeArgumentType y )
      {
         var retVal = Object.ReferenceEquals( x, y );
         if ( !retVal
            && x != null
            && y != null
            && x.ArgumentTypeKind == y.ArgumentTypeKind
            )
         {
            switch ( x.ArgumentTypeKind )
            {
               case CustomAttributeArgumentTypeKind.Simple:
                  retVal = Equality_CustomAttributeArgumentSimple_NoReferenceEquals( x as CustomAttributeArgumentTypeSimple, y as CustomAttributeArgumentTypeSimple );
                  break;
               case CustomAttributeArgumentTypeKind.TypeString:
                  retVal = Equality_CustomAttributeArgumentTypeEnum_NoReferenceEquals( x as CustomAttributeArgumentTypeEnum, y as CustomAttributeArgumentTypeEnum );
                  break;
               case CustomAttributeArgumentTypeKind.Array:
                  retVal = Equality_CustomAttributeArgumentTypeArray_NoReferenceEquals( x as CustomAttributeArgumentTypeArray, y as CustomAttributeArgumentTypeArray );
                  break;
            }
         }

         return retVal;
      }

      private static Boolean Equality_CustomAttributeArgumentSimple_NoReferenceEquals( CustomAttributeArgumentTypeSimple x, CustomAttributeArgumentTypeSimple y )
      {
         return x != null && y != null && x.SimpleType == y.SimpleType;
      }

      private static Boolean Equality_CustomAttributeArgumentTypeEnum_NoReferenceEquals( CustomAttributeArgumentTypeEnum x, CustomAttributeArgumentTypeEnum y )
      {
         return x != null && y != null && String.Equals( x.TypeString, y.TypeString );
      }

      private static Boolean Equality_CustomAttributeArgumentTypeArray_NoReferenceEquals( CustomAttributeArgumentTypeArray x, CustomAttributeArgumentTypeArray y )
      {
         return x != null && y != null && Equality_CustomAttributeArgumentType( x.ArrayType, y.ArrayType );
      }

      private static Boolean Equality_AbstractSecurityInformation( AbstractSecurityInformation x, AbstractSecurityInformation y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( x is SecurityInformation ?
            Equality_SecurityInformation_NoReferenceEquals( x as SecurityInformation, y as SecurityInformation ) :
            Equality_RawSecurityInformation_NoReferenceEquals( x as RawSecurityInformation, y as RawSecurityInformation )
         );
      }

      private static Boolean Equality_AbstractSecurityInformation_NoReferenceEquals( AbstractSecurityInformation x, AbstractSecurityInformation y )
      {
         return x != null && y != null
            && String.Equals( x.SecurityAttributeType, y.SecurityAttributeType );
      }

      private static Boolean Equality_RawSecurityInformation_NoReferenceEquals( RawSecurityInformation x, RawSecurityInformation y )
      {
         return Equality_AbstractSecurityInformation_NoReferenceEquals( x, y )
            && x.ArgumentCount == y.ArgumentCount
            && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.Bytes, y.Bytes );
      }

      private static Boolean Equality_SecurityInformation_NoReferenceEquals( SecurityInformation x, SecurityInformation y )
      {
         return Equality_AbstractSecurityInformation_NoReferenceEquals( x, y )
            && ListEqualityComparer<List<CustomAttributeNamedArgument>, CustomAttributeNamedArgument>.Equals( x.NamedArguments, y.NamedArguments, CustomAttributeNamedArgumentEqualityComparer );
      }

      private static Boolean Equality_MarshalingInfo( AbstractMarshalingInfo x, AbstractMarshalingInfo y )
      {
         var retVal = Object.ReferenceEquals( x, y );
         if ( !retVal
            && x != null
            && y != null
            && x.MarshalingInfoKind == y.MarshalingInfoKind
            && x.Value == y.Value
            )
         {
            switch ( x.MarshalingInfoKind )
            {
               case MarshalingInfoKind.Simple:
                  retVal = true;
                  break;
               case MarshalingInfoKind.FixedLengthString:
                  retVal = Equality_MarshalingInfo_FixedLengthString_NoReferenceEquals( (FixedLengthStringMarshalingInfo) x, (FixedLengthStringMarshalingInfo) y );
                  break;
               case MarshalingInfoKind.FixedLengthArray:
                  retVal = Equality_MarshalingInfo_FixedLengthArray_NoReferenceEquals( (FixedLengthArrayMarshalingInfo) x, (FixedLengthArrayMarshalingInfo) y );
                  break;
               case MarshalingInfoKind.SafeArray:
                  retVal = Equality_MarshalingInfo_SafeArray_NoReferenceEquals( (SafeArrayMarshalingInfo) x, (SafeArrayMarshalingInfo) y );
                  break;
               case MarshalingInfoKind.Array:
                  retVal = Equality_MarshalingInfo_Array_NoReferenceEquals( (ArrayMarshalingInfo) x, (ArrayMarshalingInfo) y );
                  break;
               case MarshalingInfoKind.Interface:
                  retVal = Equality_MarshalingInfo_Interface_NoReferenceEquals( (InterfaceMarshalingInfo) x, (InterfaceMarshalingInfo) y );
                  break;
               case MarshalingInfoKind.Custom:
                  retVal = Equality_MarshalingInfo_Custom_NoReferenceEquals( (CustomMarshalingInfo) x, (CustomMarshalingInfo) y );
                  break;
               case MarshalingInfoKind.Raw:
                  retVal = Equality_MarshalingInfo_Raw_NoReferenceEquals( (RawMarshalingInfo) x, (RawMarshalingInfo) y );
                  break;
            }
         }

         return retVal;
      }

      private static Boolean Equality_MarshalingInfo_FixedLengthString_NoReferenceEquals( FixedLengthStringMarshalingInfo x, FixedLengthStringMarshalingInfo y )
      {
         return x.Size == y.Size;
      }

      private static Boolean Equality_MarshalingInfo_FixedLengthArray_NoReferenceEquals( FixedLengthArrayMarshalingInfo x, FixedLengthArrayMarshalingInfo y )
      {
         return x.Size == y.Size
            && x.ElementType == y.ElementType;
      }

      private static Boolean Equality_MarshalingInfo_SafeArray_NoReferenceEquals( SafeArrayMarshalingInfo x, SafeArrayMarshalingInfo y )
      {
         return x.ElementType == y.ElementType
            && String.Equals( x.UserDefinedType, y.UserDefinedType );
      }

      private static Boolean Equality_MarshalingInfo_Array_NoReferenceEquals( ArrayMarshalingInfo x, ArrayMarshalingInfo y )
      {
         return x.ElementType == y.ElementType
            && x.SizeParameterIndex == y.SizeParameterIndex
            && x.Size == y.Size
            && x.Flags == y.Flags;
      }

      private static Boolean Equality_MarshalingInfo_Interface_NoReferenceEquals( InterfaceMarshalingInfo x, InterfaceMarshalingInfo y )
      {
         return x.IIDParameterIndex == y.IIDParameterIndex;
      }

      private static Boolean Equality_MarshalingInfo_Custom_NoReferenceEquals( CustomMarshalingInfo x, CustomMarshalingInfo y )
      {
         return String.Equals( x.CustomMarshalerTypeName, y.CustomMarshalerTypeName )
            && String.Equals( x.MarshalCookie, y.MarshalCookie )
            && String.Equals( x.GUIDString, y.GUIDString )
            && String.Equals( x.NativeTypeName, y.NativeTypeName );
      }

      private static Boolean Equality_MarshalingInfo_Raw_NoReferenceEquals( RawMarshalingInfo x, RawMarshalingInfo y )
      {
         return ArrayEqualityComparer<Byte>.ArrayEquality( x.Bytes, y.Bytes );
      }



      private static Int32 HashCode_MetaData( CILMetaData x )
      {
         return x == null ?
            0 :
            ArrayEqualityComparer<Int32>.DefaultArrayEqualityComparer.GetHashCode( new[] { x.TypeDefinitions.GetRowCount(), x.MethodDefinitions.GetRowCount(), x.ParameterDefinitions.GetRowCount(), x.FieldDefinitions.GetRowCount() } );
      }

      private static Int32 HashCode_ModuleDefinition( ModuleDefinition x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe( 1 );
      }

      private static Int32 HashCode_TypeReference( TypeReference x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + x.Namespace.GetHashCodeSafe( 1 ) );
      }

      private static Int32 HashCode_TypeDefinition( TypeDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + x.Namespace.GetHashCodeSafe( 1 ) );
      }

      private static Int32 HashCode_FieldDefinitionPointer( FieldDefinitionPointer x )
      {
         return x == null ? 0 : x.FieldIndex.GetHashCode();
      }

      private static Int32 HashCode_FieldDefinition( FieldDefinition x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe( 1 ); // TODO might need to include something else to hashcode?
      }

      private static Int32 HashCode_MethodDefinitionPointer( MethodDefinitionPointer x )
      {
         return x == null ? 0 : x.MethodIndex.GetHashCode();
      }

      private static Int32 HashCode_MethodDefinition( MethodDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + x.Signature.Parameters.Count );
      }

      private static Int32 HashCode_ParameterDefinitionPointer( ParameterDefinitionPointer x )
      {
         return x == null ? 0 : x.ParameterIndex.GetHashCode();
      }

      private static Int32 HashCode_ParameterDefinition( ParameterDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + x.Sequence );
      }

      private static Int32 HashCode_InterfaceImplementation( InterfaceImplementation x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Class.GetHashCode() ) * 23 + x.Interface.GetHashCode() );
      }

      private static Int32 HashCode_MemberReference( MemberReference x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + x.DeclaringType.GetHashCode() );
      }

      private static Int32 HashCode_ConstantDefinition( ConstantDefinition x )
      {
         return x == null ? 0 : x.Parent.GetHashCode();
      }

      private static Int32 HashCode_CustomAttributeDefinition( CustomAttributeDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Parent.GetHashCode() ) * 23 + x.Type.GetHashCode() );
      }

      private static Int32 HashCode_FieldMarshal( FieldMarshal x )
      {
         return x == null ? 0 : x.Parent.GetHashCode();
      }

      private static Int32 HashCode_SecurityDefinition( SecurityDefinition x )
      {
         return x == null ? 0 : x.Parent.GetHashCode();
      }

      private static Int32 HashCode_ClassLayout( ClassLayout x )
      {
         return x == null ? 0 : x.Parent.GetHashCode();
      }

      private static Int32 HashCode_FieldLayout( FieldLayout x )
      {
         return x == null ? 0 : x.Field.GetHashCode();
      }

      private static Int32 HashCode_StandaloneSignature( StandaloneSignature x )
      {
         return x == null ? 0 : HashCode_AbstractSignature( x.Signature );
      }

      private static Int32 HashCode_EventMap( EventMap x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Parent.GetHashCode() ) * 23 + x.EventList.GetHashCode() );
      }

      private static Int32 HashCode_EventDefinitionPointer( EventDefinitionPointer x )
      {
         return x == null ? 0 : x.EventIndex.GetHashCode();
      }

      private static Int32 HashCode_EventDefinition( EventDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + x.EventType.GetHashCode() );
      }

      private static Int32 HashCode_PropertyMap( PropertyMap x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Parent.GetHashCode() ) * 23 + x.PropertyList.GetHashCode() );
      }

      private static Int32 HashCode_PropertyDefinitionPointer( PropertyDefinitionPointer x )
      {
         return x == null ? 0 : x.PropertyIndex.GetHashCode();
      }

      private static Int32 HashCode_PropertyDefinition( PropertyDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + HashCode_PropertySignature( x.Signature ) );
      }

      private static Int32 HashCode_MethodSemantics( MethodSemantics x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Method.GetHashCode() ) * 23 + x.Associaton.GetHashCode() );
      }

      private static Int32 HashCode_MethodImplementation( MethodImplementation x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Class.GetHashCode() ) * 23 + x.MethodBody.GetHashCode() );
      }

      private static Int32 HashCode_ModuleReference( ModuleReference x )
      {
         return x == null ? 0 : x.ModuleName.GetHashCodeSafe( 1 );
      }

      private static Int32 HashCode_TypeSpecification( TypeSpecification x )
      {
         return x == null ? 0 : HashCode_TypeSignature( x.Signature );
      }

      private static Int32 HashCode_MethodImplementationMap( MethodImplementationMap x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.ImportName.GetHashCodeSafe( 1 ) ) * 23 + x.MemberForwarded.GetHashCode() );
      }

      private static Int32 HashCode_FieldRVA( FieldRVA x )
      {
         return x == null ? 0 : x.Field.GetHashCode();
      }

      private static Int32 HashCode_EditAndContinueLog( EditAndContinueLog x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Token ) * 23 + x.FuncCode );
      }

      private static Int32 HashCode_EditAndContinueMap( EditAndContinueMap x )
      {
         return x == null ? 0 : ( 17 * 23 + x.Token );
      }

      private static Int32 HashCode_AssemblyDefinition( AssemblyDefinition x )
      {
         return x == null ? 0 : x.AssemblyInformation.GetHashCodeSafe( 1 );
      }

#pragma warning disable 618

      private static Int32 HashCode_AssemblyDefinitionProcessor( AssemblyDefinitionProcessor x )
      {
         return x == null ? 0 : ( 17 * 23 + x.Processor );
      }

      private static Int32 HashCode_AssemblyDefinitionOS( AssemblyDefinitionOS x )
      {
         return x == null ? 0 : ( ( ( 17 * 23 + x.OSPlatformID ) * 23 + x.OSMajorVersion ) * 23 + x.OSMinorVersion );
      }

#pragma warning restore 618

      private static Int32 HashCode_AssemblyReference( AssemblyReference x )
      {
         return x == null ? 0 : x.AssemblyInformation.GetHashCodeSafe( 1 );
      }

#pragma warning disable 618
      private static Int32 HashCode_AssemblyReferenceProcessor( AssemblyReferenceProcessor x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Processor ) * 23 + x.AssemblyRef.GetHashCode() );
      }

      private static Int32 HashCode_AssemblyReferenceOS( AssemblyReferenceOS x )
      {
         return x == null ? 0 : ( ( ( ( 17 * 23 + x.OSPlatformID ) * 23 + x.OSMajorVersion ) * 23 + x.OSMinorVersion ) * 23 + x.AssemblyRef.GetHashCode() );
      }
#pragma warning restore 618

      private static Int32 HashCode_FileReference( FileReference x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe( 1 );
      }

      private static Int32 HashCode_ExportedType( ExportedType x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe( 1 );
      }

      private static Int32 HashCode_ManifestResource( ManifestResource x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe( 1 );
      }

      private static Int32 HashCode_NestedClassDefinition( NestedClassDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.NestedClass.GetHashCode() ) * 23 + x.EnclosingClass.GetHashCode() );
      }

      private static Int32 HashCode_GenericParameterDefinition( GenericParameterDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + x.GenericParameterIndex );
      }

      private static Int32 HashCode_MethodSpecification( MethodSpecification x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Method.GetHashCode() ) * 23 + HashCode_GenericMethodSignature( x.Signature ) );
      }

      private static Int32 HashCode_GenericParameterConstraintDefinition( GenericParameterConstraintDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Owner.GetHashCode() ) * 23 + x.Constraint.GetHashCode() );
      }

      private static Int32 HashCode_MethodILDefinition( MethodILDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.LocalsSignatureIndex.GetHashCodeSafe() ) * 23 + SequenceEqualityComparer<IEnumerable<OpCodeInfo>, OpCodeInfo>.SequenceHashCode( x.OpCodes.Take( 10 ), HashCode_OpCodeInfo ) );
      }

      private static Int32 HashCode_MethodExceptionBlock( MethodExceptionBlock x )
      {
         return x == null ? 0 : ( ( ( 17 * 23 + (Int32) x.BlockType ) * 23 + x.TryOffset ) * 23 + x.TryLength );
      }

      private static Int32 HashCode_OpCodeInfo( OpCodeInfo x )
      {
         Int32 retVal;
         if ( x == null )
         {
            retVal = 0;
         }
         else
         {
            retVal = 17 * 23 + x.OpCode.GetHashCode();
            var infoKind = x.InfoKind;
            if ( infoKind != OpCodeOperandKind.OperandNone )
            {
               Int32 operandHashCode;
               switch ( x.InfoKind )
               {
                  case OpCodeOperandKind.OperandInteger:
                     operandHashCode = ( (OpCodeInfoWithInt32) x ).Operand;
                     break;
                  case OpCodeOperandKind.OperandInteger64:
                     operandHashCode = ( (OpCodeInfoWithInt64) x ).Operand.GetHashCode();
                     break;
                  case OpCodeOperandKind.OperandR4:
                     operandHashCode = ( (OpCodeInfoWithSingle) x ).Operand.GetHashCode();
                     break;
                  case OpCodeOperandKind.OperandR8:
                     operandHashCode = ( (OpCodeInfoWithDouble) x ).Operand.GetHashCode();
                     break;
                  case OpCodeOperandKind.OperandString:
                     operandHashCode = ( (OpCodeInfoWithString) x ).Operand.GetHashCodeSafe();
                     break;
                  case OpCodeOperandKind.OperandTableIndex:
                     operandHashCode = ( (OpCodeInfoWithTableIndex) x ).Operand.GetHashCode();
                     break;
                  default:
                     operandHashCode = 0;
                     break;
               }
               retVal = retVal * 23 + operandHashCode;
            }
         }

         return retVal;
      }

      private static Int32 HashCode_AssemblyInformation( AssemblyInformation x )
      {
         return x.GetHashCodeSafe();
      }

      private static Int32 HashCode_AbstractSignature( AbstractSignature x )
      {
         if ( x == null )
         {
            return 0;
         }
         else
         {
            switch ( x.SignatureKind )
            {
               case SignatureKind.Field:
                  return HashCode_FieldSignature( x as FieldSignature );
               case SignatureKind.GenericMethodInstantiation:
                  return HashCode_GenericMethodSignature( x as GenericMethodSignature );
               case SignatureKind.LocalVariables:
                  return HashCode_LocalVariablesSignature( x as LocalVariablesSignature );
               case SignatureKind.MethodDefinition:
                  return HashCode_MethodDefinitionSignature( x as MethodDefinitionSignature );
               case SignatureKind.MethodReference:
                  return HashCode_MethodReferenceSignature( x as MethodReferenceSignature );
               case SignatureKind.Property:
                  return HashCode_PropertySignature( x as PropertySignature );
               case SignatureKind.RawSignature:
                  return HashCode_RawSignature( x as RawSignature );
               case SignatureKind.Type:
                  return HashCode_TypeSignature( x as TypeSignature );
               default:
                  return 0;
            }
         }

      }

      private static Int32 HashCode_RawSignature( RawSignature x )
      {
         return ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.GetHashCode( x.Bytes );
      }

      private static Int32 HashCode_AbstractMethodSignature( AbstractMethodSignature x )
      {
         return x == null ? 0 : ( ( 17 * 23 + HashCode_ParameterSignature( x.ReturnType ) ) * 23 + ListEqualityComparer<List<ParameterSignature>, ParameterSignature>.GetHashCode( x.Parameters, ParameterSignatureEqualityComparer ) );
      }

      private static Int32 HashCode_MethodDefinitionSignature( MethodDefinitionSignature x )
      {
         return HashCode_AbstractMethodSignature( x );
      }

      private static Int32 HashCode_MethodReferenceSignature( MethodReferenceSignature x )
      {
         // Ignore varargs when calculating hash code
         return HashCode_AbstractMethodSignature( x );
      }

      private static Int32 HashCode_FieldSignature( FieldSignature x )
      {
         return x == null ? 0 : HashCode_TypeSignature( x.Type );
      }

      private static Int32 HashCode_PropertySignature( PropertySignature x )
      {
         return x == null ? 0 : ( ( 17 * 23 + HashCode_TypeSignature( x.PropertyType ) ) * 23 + ListEqualityComparer<List<ParameterSignature>, ParameterSignature>.GetHashCode( x.Parameters, ParameterSignatureEqualityComparer ) );
      }

      private static Int32 HashCode_LocalVariablesSignature( LocalVariablesSignature x )
      {
         return x == null ? 0 : ( 17 * 23 + ListEqualityComparer<List<LocalVariableSignature>, LocalVariableSignature>.GetHashCode( x.Locals, LocalVariableSignatureEqualityComparer ) );
      }

      private static Int32 HashCode_LocalVariableSignature( LocalVariableSignature x )
      {
         return x == null ? 0 : HashCode_TypeSignature( x.Type );
      }

      private static Int32 HashCode_ParameterSignature( ParameterSignature x )
      {
         return x == null ? 0 : HashCode_TypeSignature( x.Type );
      }

      private static Int32 HashCode_CustomModifierSignature( CustomModifierSignature x )
      {
         return x == null ? 0 : x.CustomModifierType.GetHashCode();
      }

      private static Int32 HashCode_TypeSignature( TypeSignature x )
      {
         if ( x == null )
         {
            return 0;
         }
         else
         {
            switch ( x.TypeSignatureKind )
            {
               case TypeSignatureKind.Simple:
                  return HashCode_SimpleTypeSignature( x as SimpleTypeSignature );
               case TypeSignatureKind.ClassOrValue:
                  return HashCode_ClassOrValueTypeSignature( x as ClassOrValueTypeSignature );
               case TypeSignatureKind.GenericParameter:
                  return HashCode_GenericParameterTypeSignature( x as GenericParameterTypeSignature );
               case TypeSignatureKind.FunctionPointer:
                  return HashCode_FunctionPointerTypeSignature( x as FunctionPointerTypeSignature );
               case TypeSignatureKind.Pointer:
                  return HashCode_PointerTypeSignature( x as PointerTypeSignature );
               case TypeSignatureKind.ComplexArray:
                  return HashCode_ComplexArrayTypeSignature( x as ComplexArrayTypeSignature );
               case TypeSignatureKind.SimpleArray:
                  return HashCode_SimpleArrayTypeSignature( x as SimpleArrayTypeSignature );
               default:
                  return 0;
            }
         }
      }

      private static Int32 HashCode_SimpleTypeSignature( SimpleTypeSignature x )
      {
         return x == null ? 0 : (Int32) x.SimpleType;
      }

      private static Int32 HashCode_ClassOrValueTypeSignature( ClassOrValueTypeSignature x )
      {
         return x == null ? 0 : x.Type.GetHashCode();
      }

      private static Int32 HashCode_GenericParameterTypeSignature( GenericParameterTypeSignature x )
      {
         return x == null ? 0 : x.GenericParameterIndex.GetHashCode();
      }

      private static Int32 HashCode_FunctionPointerTypeSignature( FunctionPointerTypeSignature x )
      {
         return x == null ? 0 : HashCode_MethodReferenceSignature( x.MethodSignature );
      }

      private static Int32 HashCode_PointerTypeSignature( PointerTypeSignature x )
      {
         return x == null ? 0 : ( 17 * 23 + HashCode_TypeSignature( x.PointerType ) );
      }

      private static Int32 HashCode_ComplexArrayTypeSignature( ComplexArrayTypeSignature x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Rank ) * 23 + HashCode_TypeSignature( x.ArrayType ) );
      }

      private static Int32 HashCode_SimpleArrayTypeSignature( SimpleArrayTypeSignature x )
      {
         return x == null ? 0 : ( 17 * 41 + HashCode_TypeSignature( x.ArrayType ) );
      }

      private static Int32 HashCode_GenericMethodSignature( GenericMethodSignature x )
      {
         return x == null ? 0 : ListEqualityComparer<List<TypeSignature>, TypeSignature>.GetHashCode( x.GenericArguments, TypeSignatureEqualityComparer );
      }

      private static Int32 HashCode_AbstractCustomAttributeSignature( AbstractCustomAttributeSignature x )
      {
         return x == null ? 0 : ( x is CustomAttributeSignature ? HashCode_CustomAttributeSignature( x as CustomAttributeSignature ) : HashCode_RawCustomAttributeSignature( x as RawCustomAttributeSignature ) );
      }

      private static Int32 HashCode_RawCustomAttributeSignature( RawCustomAttributeSignature x )
      {
         return x == null ? 0 : ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.GetHashCode( x.Bytes );
      }

      private static Int32 HashCode_CustomAttributeSignature( CustomAttributeSignature x )
      {
         return x == null ? 0 : ListEqualityComparer<List<CustomAttributeTypedArgument>, CustomAttributeTypedArgument>.GetHashCode( x.TypedArguments, CustomAttributeTypedArgumentEqualityComparer );
      }

      private static Int32 HashCode_CustomAttributeTypedArgument( CustomAttributeTypedArgument x )
      {
         return x == null ? 0 : HashCode_CustomAttributeValue( x.Value );
      }

      private static Int32 HashCode_CustomAttributeValue( Object x )
      {
         return x == null ? 0 : ( x is Array ? HashCode_Array( x as Array ) : x.GetHashCode() );
      }

      private static Int32 HashCode_Array( Array x )
      {
         Int32 retVal;
         if ( x == null )
         {
            retVal = 0;
         }
         else
         {
            retVal = 17;
            var max = x.Length;
            if ( max > 0 )
            {
               unchecked
               {
                  for ( var i = 0; i < max; ++i )
                  {
                     retVal = retVal * 23 + HashCode_CustomAttributeValue( x.GetValue( i ) );
                  }
               }
            }
         }

         return retVal;
      }

      private static Int32 HashCode_CustomAttributeNamedArgument( CustomAttributeNamedArgument x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + HashCode_CustomAttributeTypedArgument( x.Value ) );
      }

      private static Int32 HashCode_CustomAttributeArgumentType( CustomAttributeArgumentType x )
      {
         Int32 retVal;
         if ( x == null )
         {
            retVal = 0;
         }
         else
         {
            var s = x as CustomAttributeArgumentTypeSimple;
            if ( s != null )
            {
               retVal = s.SimpleType.GetHashCode();
            }
            else
            {
               var e = x as CustomAttributeArgumentTypeEnum;
               retVal = e != null ? e.TypeString.GetHashCodeSafe() : x.ArgumentTypeKind.GetHashCode();
            }
         }
         return retVal;
      }

      private static Int32 HashCode_AbstractSecurityInformation( AbstractSecurityInformation x )
      {
         return x == null ? 0 : ( x is SecurityInformation ? HashCode_SecurityInformation( x as SecurityInformation ) : HashCode_RawSecurityInformation( x as RawSecurityInformation ) );
      }

      private static Int32 HashCode_RawSecurityInformation( RawSecurityInformation x )
      {
         return x == null ? 0 : ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.GetHashCode( x.Bytes );
      }

      private static Int32 HashCode_SecurityInformation( SecurityInformation x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.SecurityAttributeType.GetHashCodeSafe( 1 ) ) * 23 + ListEqualityComparer<List<CustomAttributeNamedArgument>, CustomAttributeNamedArgument>.GetHashCode( x.NamedArguments, CustomAttributeNamedArgumentEqualityComparer ) );
      }

      private static Int32 HashCode_MarshalingInfo( AbstractMarshalingInfo x )
      {
         return x == null ? 0 : ( ( 17 * 23 + (Int32) x.MarshalingInfoKind ) * 23 + (Int32) x.Value );
      }
   }
}
