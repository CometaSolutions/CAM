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
      private static IEqualityComparer<CILMetaData> _MetaDataEqualityComparer = null;
      private static IEqualityComparer<ModuleDefinition> _ModuleDefinitionEqualityComparer = null;
      private static IEqualityComparer<TypeReference> _TypeReferenceEqualityComparer = null;
      private static IEqualityComparer<TypeDefinition> _TypeDefinitionEqualityComparer = null;
      private static IEqualityComparer<FieldDefinition> _FieldDefinitionEqualityComparer = null;
      private static IEqualityComparer<MethodDefinition> _MethodDefinitionEqualityComparer = null;
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
      private static IEqualityComparer<EventDefinition> _EventDefinitionEqualityComparer = null;
      private static IEqualityComparer<PropertyMap> _PropertyMapEqualityComparer = null;
      private static IEqualityComparer<PropertyDefinition> _PropertyDefinitionEqualityComparer = null;
      private static IEqualityComparer<MethodSemantics> _MethodSemanticsEqualityComparer = null;
      private static IEqualityComparer<MethodImplementation> _MethodImplementationEqualityComparer = null;
      private static IEqualityComparer<ModuleReference> _ModuleReferenceEqualityComparer = null;
      private static IEqualityComparer<TypeSpecification> _TypeSpecificationEqualityComparer = null;
      private static IEqualityComparer<MethodImplementationMap> _MethodImplementationMapEqualityComparer = null;
      private static IEqualityComparer<FieldRVA> _FieldRVAEqualityComparer = null;
      private static IEqualityComparer<AssemblyDefinition> _AssemblyDefinitionEqualityComparer = null;
      private static IEqualityComparer<AssemblyReference> _AssemblyReferenceEqualityComparer = null;
      private static IEqualityComparer<FileReference> _FileReferenceEqualityComparer = null;
      private static IEqualityComparer<ExportedType> _ExportedTypeEqualityComparer = null;
      private static IEqualityComparer<ManifestResource> _ManifestResourceEqualityComparer = null;
      private static IEqualityComparer<NestedClassDefinition> _NestedClassDefinitionEqualityComparer = null;
      private static IEqualityComparer<GenericParameterDefinition> _GenericParameterDefinitionEqualityComparer = null;
      private static IEqualityComparer<MethodSpecification> _MethodSpecificationEqualityComparer = null;
      private static IEqualityComparer<GenericParameterConstraintDefinition> _GenericParameterConstraintDefinitionEqualityComparer = null;

      private static IEqualityComparer<AbstractSignature> _AbstractSignatureEqualityComparer = null;
      private static IEqualityComparer<RawSignature> _RawSignatureEqualityComparer = null;
      private static IEqualityComparer<AbstractMethodSignature> _AbstractMethodSignatureEqualityComparer = null;
      private static IEqualityComparer<MethodDefinitionSignature> _MethodDefinitionSignatureEqualityComparer = null;
      private static IEqualityComparer<MethodReferenceSignature> _MethodReferenceSignatureEqualityComparer = null;
      private static IEqualityComparer<FieldSignature> _FieldSignatureEqualityComparer = null;
      private static IEqualityComparer<PropertySignature> _PropertySignatureEqualityComparer = null;
      private static IEqualityComparer<LocalVariablesSignature> _LocalVariablesSignatureEqualityComparer = null;
      private static IEqualityComparer<LocalVariableSignature> _LocalVariableSignatureEqualityComparer = null;
      private static IEqualityComparer<ParameterSignature> _ParameterSignatureEqualityComparer = null;
      private static IEqualityComparer<CustomModifierSignature> _CustomModifierSignatureEqualityComparer = null;
      private static IEqualityComparer<TypeSignature> _TypeSignatureEqualityComparer = null;
      //private static IEqualityComparer<SimpleTypeSignature> _SimpleTypeSignatureEqualityComparer = null;
      //private static IEqualityComparer<ClassOrValueTypeSignature> _ClassOrValueTypeSignatureEqualityComparer = null;
      //private static IEqualityComparer<GenericParameterTypeSignature> _GenericParameterTypeSignatureEqualityComparer = null;
      //private static IEqualityComparer<FunctionPointerTypeSignature> _FunctionPointerTypeSignatureEqualityComparer = null;
      //private static IEqualityComparer<PointerTypeSignature> _PointerTypeSignatureEqualityComparer = null;
      //private static IEqualityComparer<ComplexArrayTypeSignature> _ComplexArrayTypeSignatureEqualityComparer = null;
      //private static IEqualityComparer<SimpleArrayTypeSignature> _SimpleArrayTypeSignatureEqualityComparer = null;
      private static IEqualityComparer<GenericMethodSignature> _GenericMethodSignatureEqualityComparer = null;
      private static IEqualityComparer<AbstractCustomAttributeSignature> _AbstractCustomAttributeSignatureEqualityComparer = null;
      //private static IEqualityComparer<RawCustomAttributeSignature> _RawCustomAttributeSignatureEqualityComparer = null;
      //private static IEqualityComparer<CustomAttributeSignature> _CustomAttributeSignatureEqualityComparer = null;
      private static IEqualityComparer<CustomAttributeTypedArgument> _CustomAttributeTypedArgumentEqualityComparer = null;
      private static IEqualityComparer<CustomAttributeNamedArgument> _CustomAttributeNamedArgumentEqualityComparer = null;
      //private static IEqualityComparer<CustomAttributeArgumentType> _CustomAttributeArgumentTypeEqualityComparer = null;
      //private static IEqualityComparer<CustomAttributeArgumentSimple> _CustomAttributeArgumentSimpleEqualityComparer = null;
      //private static IEqualityComparer<CustomAttributeArgumentTypeEnum> _CustomAttributeArgumentTypeEnumEqualityComparer = null;
      //private static IEqualityComparer<CustomAttributeArgumentTypeArray> _CustomAttributeArgumentTypeArrayEqualityComparer = null;
      private static IEqualityComparer<AbstractSecurityInformation> _AbstractSecurityInformationEqualityComparer = null;
      //private static IEqualityComparer<RawSecurityInformation> _RawSecurityInformationEqualityComparer = null;
      //private static IEqualityComparer<SecurityInformation> _SecurityInformationEqualityComparer = null;
      private static IEqualityComparer<MarshalingInfo> _MarshalingInfoEqualityComparer = null;

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

      //public static IEqualityComparer<SimpleTypeSignature> SimpleTypeSignatureEqualityComparer
      //{
      //   get
      //   {
      //      var retVal = _SimpleTypeSignatureEqualityComparer;
      //      if ( retVal == null )
      //      {
      //         retVal = ComparerFromFunctions.NewEqualityComparer<SimpleTypeSignature>( Equality_SimpleTypeSignature, HashCode_SimpleTypeSignature );
      //         _SimpleTypeSignatureEqualityComparer = retVal;
      //      }
      //      return retVal;
      //   }
      //}

      //public static IEqualityComparer<ClassOrValueTypeSignature> ClassOrValueTypeSignatureEqualityComparer
      //{
      //   get
      //   {
      //      var retVal = _ClassOrValueTypeSignatureEqualityComparer;
      //      if ( retVal == null )
      //      {
      //         retVal = ComparerFromFunctions.NewEqualityComparer<ClassOrValueTypeSignature>( Equality_ClassOrValueTypeSignature, HashCode_ClassOrValueTypeSignature );
      //         _ClassOrValueTypeSignatureEqualityComparer = retVal;
      //      }
      //      return retVal;
      //   }
      //}

      //public static IEqualityComparer<GenericParameterTypeSignature> GenericParameterTypeSignatureEqualityComparer
      //{
      //   get
      //   {
      //      var retVal = _GenericParameterTypeSignatureEqualityComparer;
      //      if ( retVal == null )
      //      {
      //         retVal = ComparerFromFunctions.NewEqualityComparer<GenericParameterTypeSignature>( Equality_GenericParameterTypeSignature, HashCode_GenericParameterTypeSignature );
      //         _GenericParameterTypeSignatureEqualityComparer = retVal;
      //      }
      //      return retVal;
      //   }
      //}

      //public static IEqualityComparer<FunctionPointerTypeSignature> FunctionPointerTypeSignatureEqualityComparer
      //{
      //   get
      //   {
      //      var retVal = _FunctionPointerTypeSignatureEqualityComparer;
      //      if ( retVal == null )
      //      {
      //         retVal = ComparerFromFunctions.NewEqualityComparer<FunctionPointerTypeSignature>( Equality_FunctionPointerTypeSignature, HashCode_FunctionPointerTypeSignature );
      //         _FunctionPointerTypeSignatureEqualityComparer = retVal;
      //      }
      //      return retVal;
      //   }
      //}

      //public static IEqualityComparer<PointerTypeSignature> PointerTypeSignatureEqualityComparer
      //{
      //   get
      //   {
      //      var retVal = _PointerTypeSignatureEqualityComparer;
      //      if ( retVal == null )
      //      {
      //         retVal = ComparerFromFunctions.NewEqualityComparer<PointerTypeSignature>( Equality_PointerTypeSignature, HashCode_PointerTypeSignature );
      //         _PointerTypeSignatureEqualityComparer = retVal;
      //      }
      //      return retVal;
      //   }
      //}

      //public static IEqualityComparer<ComplexArrayTypeSignature> ComplexArrayTypeSignatureEqualityComparer
      //{
      //   get
      //   {
      //      var retVal = _ComplexArrayTypeSignatureEqualityComparer;
      //      if ( retVal == null )
      //      {
      //         retVal = ComparerFromFunctions.NewEqualityComparer<ComplexArrayTypeSignature>( Equality_ComplexArrayTypeSignature, HashCode_ComplexArrayTypeSignature );
      //         _ComplexArrayTypeSignatureEqualityComparer = retVal;
      //      }
      //      return retVal;
      //   }
      //}

      //public static IEqualityComparer<SimpleArrayTypeSignature> SimpleArrayTypeSignatureEqualityComparer
      //{
      //   get
      //   {
      //      var retVal = _SimpleArrayTypeSignatureEqualityComparer;
      //      if ( retVal == null )
      //      {
      //         retVal = ComparerFromFunctions.NewEqualityComparer<SimpleArrayTypeSignature>( Equality_SimpleArrayTypeSignature, HashCode_SimpleArrayTypeSignature );
      //         _SimpleArrayTypeSignatureEqualityComparer = retVal;
      //      }
      //      return retVal;
      //   }
      //}

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

      //public static IEqualityComparer<RawCustomAttributeSignature> RawCustomAttributeSignatureEqualityComparer
      //{
      //   get
      //   {
      //      var retVal = _RawCustomAttributeSignatureEqualityComparer;
      //      if ( retVal == null )
      //      {
      //         retVal = ComparerFromFunctions.NewEqualityComparer<RawCustomAttributeSignature>( Equality_RawCustomAttributeSignature, HashCode_RawCustomAttributeSignature );
      //         _RawCustomAttributeSignatureEqualityComparer = retVal;
      //      }
      //      return retVal;
      //   }
      //}

      //public static IEqualityComparer<CustomAttributeSignature> CustomAttributeSignatureEqualityComparer
      //{
      //   get
      //   {
      //      var retVal = _CustomAttributeSignatureEqualityComparer;
      //      if ( retVal == null )
      //      {
      //         retVal = ComparerFromFunctions.NewEqualityComparer<CustomAttributeSignature>( Equality_CustomAttributeSignature, HashCode_CustomAttributeSignature );
      //         _CustomAttributeSignatureEqualityComparer = retVal;
      //      }
      //      return retVal;
      //   }
      //}

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

      //public static IEqualityComparer<CustomAttributeArgumentType> CustomAttributeArgumentTypeEqualityComparer
      //{
      //   get
      //   {
      //      var retVal = _CustomAttributeArgumentTypeEqualityComparer;
      //      if ( retVal == null )
      //      {
      //         retVal = ComparerFromFunctions.NewEqualityComparer<CustomAttributeArgumentType>( Equality_CustomAttributeArgumentType, HashCode_CustomAttributeArgumentType );
      //         _CustomAttributeArgumentTypeEqualityComparer = retVal;
      //      }
      //      return retVal;
      //   }
      //}

      //public static IEqualityComparer<CustomAttributeArgumentSimple> CustomAttributeArgumentSimpleEqualityComparer
      //{
      //   get
      //   {
      //      var retVal = _CustomAttributeArgumentSimpleEqualityComparer;
      //      if ( retVal == null )
      //      {
      //         retVal = ComparerFromFunctions.NewEqualityComparer<CustomAttributeArgumentSimple>( Equality_CustomAttributeArgumentSimple, HashCode_CustomAttributeArgumentSimple );
      //         _CustomAttributeArgumentSimpleEqualityComparer = retVal;
      //      }
      //      return retVal;
      //   }
      //}

      //public static IEqualityComparer<CustomAttributeArgumentTypeEnum> CustomAttributeArgumentTypeEnumEqualityComparer
      //{
      //   get
      //   {
      //      var retVal = _CustomAttributeArgumentTypeEnumEqualityComparer;
      //      if ( retVal == null )
      //      {
      //         retVal = ComparerFromFunctions.NewEqualityComparer<CustomAttributeArgumentTypeEnum>( Equality_CustomAttributeArgumentTypeEnum, HashCode_CustomAttributeArgumentTypeEnum );
      //         _CustomAttributeArgumentTypeEnumEqualityComparer = retVal;
      //      }
      //      return retVal;
      //   }
      //}

      //public static IEqualityComparer<CustomAttributeArgumentTypeArray> CustomAttributeArgumentTypeArrayEqualityComparer
      //{
      //   get
      //   {
      //      var retVal = _CustomAttributeArgumentTypeArrayEqualityComparer;
      //      if ( retVal == null )
      //      {
      //         retVal = ComparerFromFunctions.NewEqualityComparer<CustomAttributeArgumentTypeArray>( Equality_CustomAttributeArgumentTypeArray, HashCode_CustomAttributeArgumentTypeArray );
      //         _CustomAttributeArgumentTypeArrayEqualityComparer = retVal;
      //      }
      //      return retVal;
      //   }
      //}

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

      //public static IEqualityComparer<RawSecurityInformation> RawSecurityInformationEqualityComparer
      //{
      //   get
      //   {
      //      var retVal = _RawSecurityInformationEqualityComparer;
      //      if ( retVal == null )
      //      {
      //         retVal = ComparerFromFunctions.NewEqualityComparer<RawSecurityInformation>( Equality_RawSecurityInformation, HashCode_RawSecurityInformation );
      //         _RawSecurityInformationEqualityComparer = retVal;
      //      }
      //      return retVal;
      //   }
      //}

      //public static IEqualityComparer<SecurityInformation> SecurityInformationEqualityComparer
      //{
      //   get
      //   {
      //      var retVal = _SecurityInformationEqualityComparer;
      //      if ( retVal == null )
      //      {
      //         retVal = ComparerFromFunctions.NewEqualityComparer<SecurityInformation>( Equality_SecurityInformation, HashCode_SecurityInformation );
      //         _SecurityInformationEqualityComparer = retVal;
      //      }
      //      return retVal;
      //   }
      //}

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


      private static Boolean Equality_MetaData( CILMetaData x, CILMetaData y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && ListEqualityComparer<List<ModuleDefinition>, ModuleDefinition>.Equals( x.ModuleDefinitions, y.ModuleDefinitions, ModuleDefinitionEqualityComparer )
            && ListEqualityComparer<List<TypeReference>, TypeReference>.Equals( x.TypeReferences, y.TypeReferences, TypeReferenceEqualityComparer )
            && ListEqualityComparer<List<TypeDefinition>, TypeDefinition>.Equals( x.TypeDefinitions, y.TypeDefinitions, TypeDefinitionEqualityComparer )
            && ListEqualityComparer<List<FieldDefinition>, FieldDefinition>.Equals( x.FieldDefinitions, y.FieldDefinitions, FieldDefinitionEqualityComparer )
            && ListEqualityComparer<List<MethodDefinition>, MethodDefinition>.Equals( x.MethodDefinitions, y.MethodDefinitions, MethodDefinitionEqualityComparer )
            && ListEqualityComparer<List<ParameterDefinition>, ParameterDefinition>.Equals( x.ParameterDefinitions, y.ParameterDefinitions, ParameterDefinitionEqualityComparer )
            && ListEqualityComparer<List<InterfaceImplementation>, InterfaceImplementation>.Equals( x.InterfaceImplementations, y.InterfaceImplementations, InterfaceImplementationEqualityComparer )
            && ListEqualityComparer<List<MemberReference>, MemberReference>.Equals( x.MemberReferences, y.MemberReferences, MemberReferenceEqualityComparer )
            && ListEqualityComparer<List<ConstantDefinition>, ConstantDefinition>.Equals( x.ConstantDefinitions, y.ConstantDefinitions, ConstantDefinitionEqualityComparer )
            && ListEqualityComparer<List<CustomAttributeDefinition>, CustomAttributeDefinition>.Equals( x.CustomAttributeDefinitions, y.CustomAttributeDefinitions, CustomAttributeDefinitionEqualityComparer )
            && ListEqualityComparer<List<FieldMarshal>, FieldMarshal>.Equals( x.FieldMarshals, y.FieldMarshals, FieldMarshalEqualityComparer )
            && ListEqualityComparer<List<SecurityDefinition>, SecurityDefinition>.Equals( x.SecurityDefinitions, y.SecurityDefinitions, SecurityDefinitionEqualityComparer )
            && ListEqualityComparer<List<ClassLayout>, ClassLayout>.Equals( x.ClassLayouts, y.ClassLayouts, ClassLayoutEqualityComparer )
            && ListEqualityComparer<List<FieldLayout>, FieldLayout>.Equals( x.FieldLayouts, y.FieldLayouts, FieldLayoutEqualityComparer )
            && ListEqualityComparer<List<StandaloneSignature>, StandaloneSignature>.Equals( x.StandaloneSignatures, y.StandaloneSignatures, StandaloneSignatureEqualityComparer )
            && ListEqualityComparer<List<EventMap>, EventMap>.Equals( x.EventMaps, y.EventMaps, EventMapEqualityComparer )
            && ListEqualityComparer<List<EventDefinition>, EventDefinition>.Equals( x.EventDefinitions, y.EventDefinitions, EventDefinitionEqualityComparer )
            && ListEqualityComparer<List<PropertyMap>, PropertyMap>.Equals( x.PropertyMaps, y.PropertyMaps, PropertyMapEqualityComparer )
            && ListEqualityComparer<List<PropertyDefinition>, PropertyDefinition>.Equals( x.PropertyDefinitions, y.PropertyDefinitions, PropertyDefinitionEqualityComparer )
            && ListEqualityComparer<List<MethodSemantics>, MethodSemantics>.Equals( x.MethodSemantics, y.MethodSemantics, MethodSemanticsEqualityComparer )
            && ListEqualityComparer<List<MethodImplementation>, MethodImplementation>.Equals( x.MethodImplementations, y.MethodImplementations, MethodImplementationEqualityComparer )
            && ListEqualityComparer<List<ModuleReference>, ModuleReference>.Equals( x.ModuleReferences, y.ModuleReferences, ModuleReferenceEqualityComparer )
            && ListEqualityComparer<List<TypeSpecification>, TypeSpecification>.Equals( x.TypeSpecifications, y.TypeSpecifications, TypeSpecificationEqualityComparer )
            && ListEqualityComparer<List<MethodImplementationMap>, MethodImplementationMap>.Equals( x.MethodImplementationMaps, y.MethodImplementationMaps, MethodImplementationMapEqualityComparer )
            && ListEqualityComparer<List<FieldRVA>, FieldRVA>.Equals( x.FieldRVAs, y.FieldRVAs, FieldRVAEqualityComparer )
            && ListEqualityComparer<List<AssemblyDefinition>, AssemblyDefinition>.Equals( x.AssemblyDefinitions, y.AssemblyDefinitions, AssemblyDefinitionEqualityComparer )
            && ListEqualityComparer<List<AssemblyReference>, AssemblyReference>.Equals( x.AssemblyReferences, y.AssemblyReferences, AssemblyReferenceEqualityComparer )
            && ListEqualityComparer<List<FileReference>, FileReference>.Equals( x.FileReferences, y.FileReferences, FileReferenceEqualityComparer )
            && ListEqualityComparer<List<ExportedType>, ExportedType>.Equals( x.ExportedTypes, y.ExportedTypes, ExportedTypeEqualityComparer )
            && ListEqualityComparer<List<ManifestResource>, ManifestResource>.Equals( x.ManifestResources, y.ManifestResources, ManifestResourceEqualityComparer )
            && ListEqualityComparer<List<NestedClassDefinition>, NestedClassDefinition>.Equals( x.NestedClassDefinitions, y.NestedClassDefinitions, NestedClassDefinitionEqualityComparer )
            && ListEqualityComparer<List<GenericParameterDefinition>, GenericParameterDefinition>.Equals( x.GenericParameterDefinitions, y.GenericParameterDefinitions, GenericParameterDefinitionEqualityComparer )
            && ListEqualityComparer<List<MethodSpecification>, MethodSpecification>.Equals( x.MethodSpecifications, y.MethodSpecifications, MethodSpecificationEqualityComparer )
            && ListEqualityComparer<List<GenericParameterConstraintDefinition>, GenericParameterConstraintDefinition>.Equals( x.GenericParameterConstraintDefinitions, y.GenericParameterConstraintDefinitions, GenericParameterConstraintDefinitionEqualityComparer )
            );
      }

      private static Boolean Equality_ModuleDefinition( ModuleDefinition x, ModuleDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.Generation == y.Generation
            && NullableEqualityComparer<Guid>.DefaultComparer.Equals( x.ModuleGUID, y.ModuleGUID )
            && NullableEqualityComparer<Guid>.DefaultComparer.Equals( x.EditAndContinueGUID, y.EditAndContinueGUID )
            && NullableEqualityComparer<Guid>.DefaultComparer.Equals( x.EditAndContinueBaseGUID, y.EditAndContinueBaseGUID )
            );
      }

      private static Boolean Equality_TypeReference( TypeReference x, TypeReference y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && String.Equals( x.Namespace, y.Namespace )
            && NullableEqualityComparer<TableIndex>.DefaultComparer.Equals( x.ResolutionScope, y.ResolutionScope )
            );
      }

      private static Boolean Equality_TypeDefinition( TypeDefinition x, TypeDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && String.Equals( x.Namespace, y.Namespace )
            && x.Attributes == y.Attributes
            && NullableEqualityComparer<TableIndex>.DefaultComparer.Equals( x.BaseType, y.BaseType )
            && x.FieldList.Equals( y.FieldList )
            && x.MethodList.Equals( y.MethodList )
            );
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

      private static Boolean Equality_MethodDefinition( MethodDefinition x, MethodDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.Attributes == y.Attributes
            && x.ImplementationAttributes == y.ImplementationAttributes
            && x.ParameterList == y.ParameterList
            && MethodDefinitionSignatureEqualityComparer.Equals( x.Signature, y.Signature )
            );
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
         var retVal = Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Field == y.Field
             && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.Data, y.Data )
             );
         return retVal;
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

      private static Boolean Equality_AssemblyReference( AssemblyReference x, AssemblyReference y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.AssemblyInformation.Equals( y.AssemblyInformation )
             && x.Attributes == y.Attributes
             && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.HashValue, y.HashValue )
             );
      }

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
             && NullableEqualityComparer<TableIndex>.DefaultComparer.Equals( x.Implementation, y.Implementation )
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
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.GenericParameterIndex == y.GenericParameterIndex
             && String.Equals( x.Name, y.Name )
             && x.Owner == y.Owner
             && x.Attributes == y.Attributes
             );
      }

      private static Boolean Equality_MethodSpecification( MethodSpecification x, MethodSpecification y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Method == y.Method
             && GenericMethodSignatureEqualityComparer.Equals( x.Signature, y.Signature )
             );
      }

      private static Boolean Equality_GenericParameterConstraintDefinition( GenericParameterConstraintDefinition x, GenericParameterConstraintDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Owner == y.Owner
             && x.Constraint == y.Constraint
             );
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
            && TypeSignatureEqualityComparer.Equals( x.Type, y.Type )
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
            && ListEqualityComparer<List<Int32>, Int32>.DefaultArrayEqualityComparer.Equals( x.Sizes, y.Sizes )
            && ListEqualityComparer<List<Int32>, Int32>.DefaultArrayEqualityComparer.Equals( x.LowerBounds, y.LowerBounds );
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
            && Equality_CustomAttributeArgumentType( x.Type, y.Type )
         );
      }

      private static Boolean Equality_CustomAttributeValue( Object x, Object y )
      {
         return Object.Equals( x, y ) || Equality_Arrays_NoReferenceEquals( x as Array, y as Array );
      }

      private static Boolean Equality_Arrays_NoReferenceEquals( Array x, Array y )
      {
         // TODO move this method to utilpack
         var retVal = x != null && y != null && x.Length == y.Length; // Arrays only supported for custom attribute values, and only simple arrays, so checking for multiple dimensions is not required here
         if ( retVal )
         {
            var max = x.Length;
            for ( var i = 0; i < max; ++i )
            {
               if ( !Equality_CustomAttributeValue( x.GetValue( i ), y.GetValue( i ) ) )
               {
                  retVal = false;
                  break;
               }
            }
         }

         return retVal;
      }

      private static Boolean Equality_CustomAttributeNamedArgument( CustomAttributeNamedArgument x, CustomAttributeNamedArgument y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( x != null && y != null
            && x.IsField == y.IsField
            && String.Equals( x.Name, y.Name )
            && Equality_CustomAttributeTypedArgument( x.Value, y.Value ) // Optimize a bit - don't use CustomAttributeTypedArgumentEqualityComparer property
         );
      }

      private static Boolean Equality_CustomAttributeArgumentType( CustomAttributeArgumentType x, CustomAttributeArgumentType y )
      {
         var retVal = Object.ReferenceEquals( x, y );
         if ( !retVal && x != null && y != null && x.ArgumentTypeKind == y.ArgumentTypeKind )
         {
            switch ( x.ArgumentTypeKind )
            {
               case CustomAttributeArgumentTypeKind.Simple:
                  retVal = Equality_CustomAttributeArgumentSimple_NoReferenceEquals( x as CustomAttributeArgumentSimple, y as CustomAttributeArgumentSimple );
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

      private static Boolean Equality_CustomAttributeArgumentSimple_NoReferenceEquals( CustomAttributeArgumentSimple x, CustomAttributeArgumentSimple y )
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


      private static Int32 HashCode_MetaData( CILMetaData x )
      {
         return x == null ?
            0 :
            ArrayEqualityComparer<Int32>.DefaultArrayEqualityComparer.GetHashCode( new[] { x.TypeDefinitions.Count, x.MethodDefinitions.Count, x.ParameterDefinitions.Count, x.FieldDefinitions.Count } );
      }

      private static Int32 HashCode_ModuleDefinition( ModuleDefinition x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe( 1 );
      }

      private static Int32 HashCode_TypeReference( TypeReference x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe( 1 );
      }

      private static Int32 HashCode_TypeDefinition( TypeDefinition x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe( 1 );
      }

      private static Int32 HashCode_FieldDefinition( FieldDefinition x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe( 1 ); // TODO might need to include something else to hashcode?
      }

      private static Int32 HashCode_MethodDefinition( MethodDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + x.ParameterList.GetHashCode() );
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

      private static Int32 HashCode_EventDefinition( EventDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + x.EventType.GetHashCode() );
      }

      private static Int32 HashCode_PropertyMap( PropertyMap x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Parent.GetHashCode() ) * 23 + x.PropertyList.GetHashCode() );
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

      private static Int32 HashCode_AssemblyDefinition( AssemblyDefinition x )
      {
         return x == null ? 0 : x.AssemblyInformation.GetHashCodeSafe( 1 );
      }

      private static Int32 HashCode_AssemblyReference( AssemblyReference x )
      {
         return x == null ? 0 : x.AssemblyInformation.GetHashCodeSafe( 1 );
      }

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
         return x == null ? 0 : ( 17 * 23 + HashCode_TypeSignature( x.Type ) );
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

      //private static Int32 HashCode_CustomAttributeArgumentType( CustomAttributeArgumentType x )
      //{
      //   throw new NotImplementedException();
      //}

      //private static Int32 HashCode_CustomAttributeArgumentSimple( CustomAttributeArgumentSimple x )
      //{
      //   throw new NotImplementedException();
      //}

      //private static Int32 HashCode_CustomAttributeArgumentTypeEnum( CustomAttributeArgumentTypeEnum x )
      //{
      //   throw new NotImplementedException();
      //}

      //private static Int32 HashCode_CustomAttributeArgumentTypeArray( CustomAttributeArgumentTypeArray x )
      //{
      //   throw new NotImplementedException();
      //}

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


   }
}
