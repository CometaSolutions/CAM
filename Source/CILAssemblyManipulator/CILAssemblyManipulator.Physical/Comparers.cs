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
using CILAssemblyManipulator.Physical.IO;
using CollectionsWithRoles.API;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
   public static class Comparers
   {
      // Metadata and metadata row comparers
#pragma warning disable 618
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
      private static IEqualityComparer<AssemblyDefinitionProcessor> _AssemblyDefinitionProcessorEqualityComparer = null;
      private static IEqualityComparer<AssemblyDefinitionOS> _AssemblyDefinitionOSEqualityComparer = null;
      private static IEqualityComparer<AssemblyReference> _AssemblyReferenceEqualityComparer = null;
      private static IEqualityComparer<AssemblyReferenceProcessor> _AssemblyReferenceProcessorEqualityComparer = null;
      private static IEqualityComparer<AssemblyReferenceOS> _AssemblyReferenceOSEqualityComparer = null;
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
#pragma warning restore 618

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
      private static IEqualityComparer<AbstractSecurityInformation> _AbstractSecurityInformationEqualityComparer = null;
      private static IEqualityComparer<MarshalingInfo> _MarshalingInfoEqualityComparer = null;

      private static IComparer<ClassLayout> _ClassLayoutComparer = null;
      private static IComparer<ConstantDefinition> _ConstantDefinitionComparer = null;
      private static IComparer<CustomAttributeDefinition> _CustomAttributeDefinitionComparer = null;
      private static IComparer<SecurityDefinition> _SecurityDefinitionComparer = null;
      private static IComparer<FieldLayout> _FieldLayoutComparer = null;
      private static IComparer<FieldMarshal> _FieldMarshalComparer = null;
      private static IComparer<FieldRVA> _FieldRVAComparer = null;
      private static IComparer<GenericParameterDefinition> _GenericParameterDefinitionComparer = null;
      private static IComparer<GenericParameterConstraintDefinition> _GenericParameterConstraintDefinitionComparer = null;
      private static IComparer<MethodImplementationMap> _MethodImplementationMapComparer = null;
      private static IComparer<InterfaceImplementation> _InterfaceImplementationComparer = null;
      private static IComparer<MethodImplementation> _MethodImplementationComparer = null;
      private static IComparer<MethodSemantics> _MethodSemanticsComparer = null;
      private static IComparer<NestedClassDefinition> _NestedClassDefinitionComparer = null;

      private static IComparer<TableIndex> _HasConstantComparer = null;
      private static IComparer<TableIndex> _HasCustomAttributeComparer = null;
      private static IComparer<TableIndex> _HasFieldMarshallComparer = null;
      private static IComparer<TableIndex> _HasDeclSecurityComparer = null;
      private static IComparer<TableIndex> _HasSemanticsComparer = null;
      private static IComparer<TableIndex> _MemberForwardedComparer = null;
      private static IComparer<TableIndex> _TypeOrMethodDefComparer = null;

      private static IEqualityComparer<Object> _CAValueEqualityComparer = ComparerFromFunctions.NewEqualityComparer<Object>( Equality_CustomAttributeValue, x => { throw new NotSupportedException(); } );

      private static IEqualityComparer<ImageInformation> _ImageInformationLogicalEqualityComparer = null;

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
      /// N.B.: Does not check for IL equality!
      /// </summary>
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


      public static IEqualityComparer<MarshalingInfo> MarshalingInfoEqualityComparer
      {
         get
         {
            var retVal = _MarshalingInfoEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<MarshalingInfo>( Equality_MarshalingInfo, HashCode_MarshalingInfo );
               _MarshalingInfoEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      public static IEqualityComparer<ImageInformation> ImageInformationLogicalEqualityComparer
      {
         get
         {
            var retVal = _ImageInformationLogicalEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<ImageInformation>( Equality_ImageInformation_Logical, HashCode_HeadersData );
               _ImageInformationLogicalEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<ClassLayout> ClassLayoutComparer
      {
         get
         {
            var retVal = _ClassLayoutComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<ClassLayout>( Comparison_ClassLayout );
               _ClassLayoutComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<ConstantDefinition> ConstantDefinitionComparer
      {
         get
         {
            var retVal = _ConstantDefinitionComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<ConstantDefinition>( Comparison_ConstantDefinition );
               _ConstantDefinitionComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<CustomAttributeDefinition> CustomAttributeDefinitionComparer
      {
         get
         {
            var retVal = _CustomAttributeDefinitionComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<CustomAttributeDefinition>( Comparison_CustomAttributeDefinition );
               _CustomAttributeDefinitionComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<SecurityDefinition> SecurityDefinitionComparer
      {
         get
         {
            var retVal = _SecurityDefinitionComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<SecurityDefinition>( Comparison_SecurityDefinition );
               _SecurityDefinitionComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<FieldLayout> FieldLayoutComparer
      {
         get
         {
            var retVal = _FieldLayoutComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<FieldLayout>( Comparison_FieldLayout );
               _FieldLayoutComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<FieldMarshal> FieldMarshalComparer
      {
         get
         {
            var retVal = _FieldMarshalComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<FieldMarshal>( Comparison_FieldMarshal );
               _FieldMarshalComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<FieldRVA> FieldRVAComparer
      {
         get
         {
            var retVal = _FieldRVAComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<FieldRVA>( Comparison_FieldRVA );
               _FieldRVAComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<GenericParameterDefinition> GenericParameterDefinitionComparer
      {
         get
         {
            var retVal = _GenericParameterDefinitionComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<GenericParameterDefinition>( Comparison_GenericParameterDefinition );
               _GenericParameterDefinitionComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<GenericParameterConstraintDefinition> GenericParameterConstraintDefinitionComparer
      {
         get
         {
            var retVal = _GenericParameterConstraintDefinitionComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<GenericParameterConstraintDefinition>( Comparison_GenericParameterConstraintDefinition );
               _GenericParameterConstraintDefinitionComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<MethodImplementationMap> MethodImplementationMapComparer
      {
         get
         {
            var retVal = _MethodImplementationMapComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<MethodImplementationMap>( Comparison_MethodImplementationMap );
               _MethodImplementationMapComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<InterfaceImplementation> InterfaceImplementationComparer
      {
         get
         {
            var retVal = _InterfaceImplementationComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<InterfaceImplementation>( Comparison_InterfaceImplementation );
               _InterfaceImplementationComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<MethodImplementation> MethodImplementationComparer
      {
         get
         {
            var retVal = _MethodImplementationComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<MethodImplementation>( Comparison_MethodImplementation );
               _MethodImplementationComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<MethodSemantics> MethodSemanticsComparer
      {
         get
         {
            var retVal = _MethodSemanticsComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<MethodSemantics>( Comparison_MethodSemantics );
               _MethodSemanticsComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<NestedClassDefinition> NestedClassDefinitionComparer
      {
         get
         {
            var retVal = _NestedClassDefinitionComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<NestedClassDefinition>( Comparison_NestedClassDefinition );
               _NestedClassDefinitionComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<TableIndex> HasConstantComparer
      {
         get
         {
            var retVal = _HasConstantComparer;
            if ( retVal == null )
            {
               var tableOrderArray = CreateTableOrderArray( Tables.Field, Tables.Parameter, Tables.Property );
               retVal = ComparerFromFunctions.NewComparer<TableIndex>( ( x, y ) => x.CompareTo( y, tableOrderArray ) );
               _HasConstantComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<TableIndex> HasCustomAttributeComparer
      {
         get
         {
            var retVal = _HasCustomAttributeComparer;
            if ( retVal == null )
            {
               var tableOrderArray = CreateTableOrderArray( Tables.MethodDef, Tables.Field, Tables.TypeRef, Tables.TypeDef, Tables.Parameter, Tables.InterfaceImpl, Tables.MemberRef, Tables.Module, Tables.DeclSecurity, Tables.Property, Tables.Event, Tables.StandaloneSignature, Tables.ModuleRef, Tables.TypeSpec, Tables.Assembly, Tables.AssemblyRef, Tables.File, Tables.ExportedType, Tables.ManifestResource, Tables.GenericParameter, Tables.GenericParameterConstraint, Tables.MethodSpec );
               retVal = ComparerFromFunctions.NewComparer<TableIndex>( ( x, y ) => x.CompareTo( y, tableOrderArray ) );
            }
            return retVal;
         }
      }

      public static IComparer<TableIndex> HasFieldMarshallComparer
      {
         get
         {
            var retVal = _HasFieldMarshallComparer;
            if ( retVal == null )
            {
               var tableOrderArray = CreateTableOrderArray( Tables.Field, Tables.Parameter );
               retVal = ComparerFromFunctions.NewComparer<TableIndex>( ( x, y ) => x.CompareTo( y, tableOrderArray ) );
            }
            return retVal;
         }
      }

      public static IComparer<TableIndex> HasDeclSecurityComparer
      {
         get
         {
            var retVal = _HasDeclSecurityComparer;
            if ( retVal == null )
            {
               var tableOrderArray = CreateTableOrderArray( Tables.TypeDef, Tables.MethodDef, Tables.Assembly );
               retVal = ComparerFromFunctions.NewComparer<TableIndex>( ( x, y ) => x.CompareTo( y, tableOrderArray ) );
            }
            return retVal;
         }
      }

      public static IComparer<TableIndex> HasSemanticsComparer
      {
         get
         {
            var retVal = _HasSemanticsComparer;
            if ( retVal == null )
            {
               var tableOrderArray = CreateTableOrderArray( Tables.Event, Tables.Property );
               retVal = ComparerFromFunctions.NewComparer<TableIndex>( ( x, y ) => x.CompareTo( y, tableOrderArray ) );
            }
            return retVal;
         }
      }

      public static IComparer<TableIndex> MemberForwardedComparer
      {
         get
         {
            var retVal = _MemberForwardedComparer;
            if ( retVal == null )
            {
               var tableOrderArray = CreateTableOrderArray( Tables.Field, Tables.MethodDef );
               retVal = ComparerFromFunctions.NewComparer<TableIndex>( ( x, y ) => x.CompareTo( y, tableOrderArray ) );
            }
            return retVal;
         }
      }

      public static IComparer<TableIndex> TypeOrMethodDefComparer
      {
         get
         {
            var retVal = _TypeOrMethodDefComparer;
            if ( retVal == null )
            {
               var tableOrderArray = CreateTableOrderArray( Tables.TypeDef, Tables.MethodDef );
               retVal = ComparerFromFunctions.NewComparer<TableIndex>( ( x, y ) => x.CompareTo( y, tableOrderArray ) );
            }
            return retVal;
         }
      }

      private static Boolean Equality_MetaData( CILMetaData x, CILMetaData y )
      {
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
               case OpCodeOperandKind.OperandSwitch:
                  retVal = ListEqualityComparer<List<Int32>, Int32>.ListEquality( ( (OpCodeInfoWithSwitch) x ).Offsets, ( (OpCodeInfoWithSwitch) y ).Offsets );
                  break;
               case OpCodeOperandKind.OperandToken:
                  retVal = ( (OpCodeInfoWithToken) x ).Operand == ( (OpCodeInfoWithToken) y ).Operand;
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
            && x.SignatureStarter == y.SignatureStarter
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
         //&& Equality_CustomAttributeArgumentType( x.Type, y.Type )
         );
      }

      private static Boolean Equality_CustomAttributeValue( Object x, Object y )
      {
         return Object.Equals( x, y ) || ( x as Array ).ArraysDeepEqualUntyped( y as Array, _CAValueEqualityComparer );
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

      private static Boolean Equality_MarshalingInfo( MarshalingInfo x, MarshalingInfo y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( x != null && y != null
            && x.ArrayType == y.ArrayType
            && x.ConstSize == y.ConstSize
            && x.IIDParameterIndex == y.IIDParameterIndex
            && String.Equals( x.MarshalCookie, y.MarshalCookie )
            && String.Equals( x.MarshalType, y.MarshalType )
            && x.SafeArrayType == y.SafeArrayType
            && x.SafeArrayUserDefinedType == y.SafeArrayUserDefinedType
            && x.SizeParameterIndex == y.SizeParameterIndex
            && x.Value == y.Value
         );
      }

      private static Boolean Equality_ImageInformation_Logical( ImageInformation x, ImageInformation y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && Equality_PEInformation_Logical( x.PEInformation, y.PEInformation )
            && Equality_CLIInformation_Logical( x.CLIInformation, y.CLIInformation )
            && Equality_DebugInfo_Logical( x.DebugInformation, y.DebugInformation )
            );



         //&& x.Machine == y.Machine
         //&& String.Equals( x.MetaDataVersion, y.MetaDataVersion )
         //&& String.Equals( x.ImportHintName, y.ImportHintName )
         //&& x.ModuleFlags == y.ModuleFlags
         //&& x.DLLFlags == y.DLLFlags
         //&& NullableEqualityComparer<TableIndex>.DefaultComparer.Equals( x.CLREntryPointIndex, y.CLREntryPointIndex )
         //&& Equality_DebugInfo( x.DebugInformation, y.DebugInformation )
         //&& x.ImageBase == y.ImageBase
         //&& x.FileAlignment == y.FileAlignment
         //&& x.SectionAlignment == y.SectionAlignment
         //&& x.StackReserve == y.StackReserve
         //&& x.StackCommit == y.StackCommit
         //&& x.HeapReserve == y.HeapReserve
         //&& x.HeapCommit == y.HeapCommit
         //&& x.DLLFlags == y.DLLFlags
         //&& String.Equals( x.ImportDirectoryName, y.ImportDirectoryName )
         ////&& x.EntryPointInstruction == y.EntryPointInstruction
         //&& x.LinkerMajor == y.LinkerMajor
         //&& x.LinkerMinor == y.LinkerMinor
         //&& x.OSMajor == y.OSMajor
         //&& x.OSMinor == y.OSMinor
         //&& x.SubSysMajor == y.SubSysMajor
         //&& x.SubSysMinor == y.SubSysMinor
         //&& x.CLIMajor == y.CLIMajor
         //&& x.CLIMinor == y.CLIMinor
         //&& x.TableHeapMajor == y.TableHeapMajor
         //&& x.TableHeapMinor == y.TableHeapMinor
         //);
      }

      private static Boolean Equality_PEInformation_Logical( PEInformation x, PEInformation y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && Equality_DOSHeader_Logical( x.DOSHeader, y.DOSHeader )
            && Equality_NTHeader_Logical( x.NTHeader, y.NTHeader )
            //&& x.SectionHeaders.ArrayQueryEquality( y.SectionHeaders, Equality_SectionHeader_Logical )
            );
      }

      private static Boolean Equality_DOSHeader_Logical( DOSHeader x, DOSHeader y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Signature == y.Signature
            );
      }

      private static Boolean Equality_NTHeader_Logical( NTHeader x, NTHeader y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Signature == y.Signature
            && Equality_FileHeader_Logical( x.FileHeader, y.FileHeader )
            && Equality_OptionalHeader_Logical( x.OptionalHeader, y.OptionalHeader )
            );
      }

      private static Boolean Equality_FileHeader_Logical( FileHeader x, FileHeader y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Machine == y.Machine
            && x.TimeDateStamp == y.TimeDateStamp
            && x.Characteristics == y.Characteristics
            );
      }

      private static Boolean Equality_OptionalHeader_Logical( OptionalHeader x, OptionalHeader y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.MajorLinkerVersion == y.MajorLinkerVersion
            && x.MinorLinkerVersion == y.MinorLinkerVersion
            && x.ImageBase == y.ImageBase
            && x.SectionAlignment == y.SectionAlignment
            && x.FileAlignment == y.FileAlignment
            && x.MajorOSVersion == y.MajorOSVersion
            && x.MinorOSVersion == y.MinorOSVersion
            && x.MajorUserVersion == y.MajorUserVersion
            && x.MinorUserVersion == y.MinorUserVersion
            && x.MajorSubsystemVersion == y.MajorSubsystemVersion
            && x.MinorSubsystemVersion == y.MinorSubsystemVersion
            && x.Win32VersionValue == y.Win32VersionValue
            && x.Subsystem == y.Subsystem
            && x.DLLCharacteristics == y.DLLCharacteristics
            && x.StackReserveSize == y.StackReserveSize
            && x.StackCommitSize == y.StackCommitSize
            && x.HeapReserveSize == y.HeapReserveSize
            && x.HeapCommitSize == y.HeapCommitSize
            && x.LoaderFlags == y.LoaderFlags
            && x.NumberOfDataDirectories == y.NumberOfDataDirectories
            );
      }

      //private static Boolean Equality_SectionHeader_Logical( SectionHeader x, SectionHeader y )
      //{

      //}

      private static Boolean Equality_CLIInformation_Logical( CLIInformation x, CLIInformation y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && Equality_CLIHeader_Logical( x.CLIHeader, y.CLIHeader )
            && Equality_MDRoot_Logical( x.MetaDataRoot, y.MetaDataRoot )
            && Equality_MDTableStreamHeader( x.TableStreamHeader, y.TableStreamHeader )
            && x.FieldRVAs.Count == y.FieldRVAs.Count
            && x.MethodRVAs.Count == y.MethodRVAs.Count
            );
      }

      private static Boolean Equality_CLIHeader_Logical( CLIHeader x, CLIHeader y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.MajorRuntimeVersion == y.MajorRuntimeVersion
            && x.MinorRuntimeVersion == y.MinorRuntimeVersion
            && x.Flags == y.Flags
            && x.EntryPointToken == y.EntryPointToken
            );
      }

      private static Boolean Equality_MDRoot_Logical( MetaDataRoot x, MetaDataRoot y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Signature == y.Signature
            && x.MajorVersion == y.MajorVersion
            && x.MinorVersion == y.MinorVersion
            && x.Reserved == y.Reserved
            && x.VersionStringBytes.ArrayQueryEquality( y.VersionStringBytes )
            && x.StorageFlags == y.StorageFlags
            && x.Reserved2 == y.Reserved2
            );
      }

      private static Boolean Equality_MDTableStreamHeader( MetaDataTableStreamHeader x, MetaDataTableStreamHeader y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Reserved == y.Reserved
            && x.MajorVersion == y.MajorVersion
            && x.MinorVersion == y.MinorVersion
            && x.Reserved2 == y.Reserved2
            && NullableEqualityComparer<Int32>.Equals( x.ExtraData, y.ExtraData )
            );
      }

      private static Boolean Equality_DebugInfo_Logical( DebugInformation x, DebugInformation y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Timestamp == y.Timestamp
            && x.Characteristics == y.Characteristics
            && x.DebugType == y.DebugType
            && x.VersionMajor == y.VersionMajor
            && x.VersionMinor == y.VersionMinor
            && x.DebugData.ArrayQueryEquality( y.DebugData )
            );
      }

      private static Int32 HashCode_MetaData( CILMetaData x )
      {
         return x == null ?
            0 :
            ArrayEqualityComparer<Int32>.DefaultArrayEqualityComparer.GetHashCode( new[] { x.TypeDefinitions.RowCount, x.MethodDefinitions.RowCount, x.ParameterDefinitions.RowCount, x.FieldDefinitions.RowCount } );
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
                  case OpCodeOperandKind.OperandToken:
                     operandHashCode = ( (OpCodeInfoWithToken) x ).Operand.GetHashCode();
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

      private static Int32 HashCode_MarshalingInfo( MarshalingInfo x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.MarshalType.GetHashCodeSafe( 1 ) ) * 23 + (Int32) x.Value );
      }

      private static Int32 HashCode_HeadersData( ImageInformation x )
      {
         return x == null ? 0 : ( ( 17 * 23 + (Int32) x.PEInformation.NTHeader.FileHeader.Machine ) * 23 + SequenceEqualityComparer<ArrayQuery<Byte>, Byte>.GetHashCode( x.CLIInformation.MetaDataRoot.VersionStringBytes ) );
      }

      private static Int32 Comparison_ClassLayout( ClassLayout x, ClassLayout y )
      {
         // Parent (simple index) is primary key
         return x.Parent.Index.CompareTo( y.Parent.Index );
      }

      private static Int32 Comparison_ConstantDefinition( ConstantDefinition x, ConstantDefinition y )
      {
         // Parent (coded index) is primary key
         return HasConstantComparer.Compare( x.Parent, y.Parent );
      }

      private static Int32 Comparison_CustomAttributeDefinition( CustomAttributeDefinition x, CustomAttributeDefinition y )
      {
         // Parent (coded index) is primary key
         return HasCustomAttributeComparer.Compare( x.Parent, y.Parent );
      }

      private static Int32 Comparison_SecurityDefinition( SecurityDefinition x, SecurityDefinition y )
      {
         // Parent (coded index) is primary key
         return HasDeclSecurityComparer.Compare( x.Parent, y.Parent );
      }

      private static Int32 Comparison_FieldLayout( FieldLayout x, FieldLayout y )
      {
         // Field (simple index) is primary key
         return x.Field.Index.CompareTo( y.Field.Index );
      }

      private static Int32 Comparison_FieldMarshal( FieldMarshal x, FieldMarshal y )
      {
         // Parent (coded index) is primary key
         return HasFieldMarshallComparer.Compare( x.Parent, y.Parent );
      }

      private static Int32 Comparison_FieldRVA( FieldRVA x, FieldRVA y )
      {
         // Field (simple index) is primary key
         return x.Field.Index.CompareTo( y.Field.Index );
      }

      private static Int32 Comparison_GenericParameterDefinition( GenericParameterDefinition x, GenericParameterDefinition y )
      {
         // Owner (coded index) is primary key, Sequence is secondary key
         var retVal = TypeOrMethodDefComparer.Compare( x.Owner, y.Owner );
         if ( retVal == 0 )
         {
            retVal = x.GenericParameterIndex.CompareTo( y.GenericParameterIndex );
         }
         return retVal;
      }

      private static Int32 Comparison_GenericParameterConstraintDefinition( GenericParameterConstraintDefinition x, GenericParameterConstraintDefinition y )
      {
         // Owner (simple index) is primary key
         return x.Owner.Index.CompareTo( y.Owner.Index );
      }

      private static Int32 Comparison_MethodImplementationMap( MethodImplementationMap x, MethodImplementationMap y )
      {
         // MemberForwarded (coded index) is primary key
         return MemberForwardedComparer.Compare( x.MemberForwarded, y.MemberForwarded );
      }

      private static Int32 Comparison_InterfaceImplementation( InterfaceImplementation x, InterfaceImplementation y )
      {
         // Primary key 'Class', secondary key 'Interface'
         var retVal = x.Class.Index.CompareTo( y.Class.Index );
         if ( retVal == 0 )
         {
            retVal = x.Interface.Index.CompareTo( y.Interface.Index );
         }
         return retVal;
      }

      private static Int32 Comparison_MethodImplementation( MethodImplementation x, MethodImplementation y )
      {
         // Class (simple index) is primary key
         return x.Class.Index.CompareTo( y.Class.Index );
      }

      private static Int32 Comparison_MethodSemantics( MethodSemantics x, MethodSemantics y )
      {
         // Associaton (coded index) is primary key
         return HasSemanticsComparer.Compare( x.Associaton, y.Associaton );
      }

      private static Int32 Comparison_NestedClassDefinition( NestedClassDefinition x, NestedClassDefinition y )
      {
         // Sort by 'NestedClass' table index
         return x.NestedClass.Index.CompareTo( y.NestedClass.Index );
      }

      private static Int32[] CreateTableOrderArray( params Tables[] tablesInOrder )
      {
         var retVal = new Int32[Consts.AMOUNT_OF_TABLES];
         for ( var i = 0; i < tablesInOrder.Length; ++i )
         {
            retVal[(Int32) tablesInOrder[i]] = i;
         }
         return retVal;
      }

      private static Boolean ArrayQueryEquality<T>( this ArrayQuery<T> x, ArrayQuery<T> y, Equality<T> equality = null )
      {
         // TODO make equality classes to CWR
         return SequenceEqualityComparer<ArrayQuery<T>, T>.SequenceEquality( x, y, equality );
      }
   }
}
