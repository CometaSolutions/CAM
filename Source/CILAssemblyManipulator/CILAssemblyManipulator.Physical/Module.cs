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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
   public interface CILModuleData
   {
      HeadersData Headers { get; }
      CILMetaData MetaData { get; }
   }

   public interface HeadersData
   {

   }

   public interface CILMetaData
   {
      IList<ModuleDefinition> ModuleDefinitions { get; set; }

      IList<TypeReference> TypeReferences { get; set; }

      IList<TypeDefinition> TypeDefinitions { get; set; }

      IList<FieldDefinition> FieldDefinitions { get; set; }

      IList<MethodDefinition> MethodDefinitions { get; set; }

      IList<ParameterDefinition> ParameterDefinitions { get; set; }

      IList<InterfaceImplementation> InterfaceImplementations { get; set; }

      IList<MemberReference> MemberReferences { get; set; }

      IList<ConstantDefinition> ConstantDefinitions { get; set; }

      IList<CustomAttributeDefinition> CustomAttributeDefinitions { get; set; }

      IList<FieldMarshal> FieldMarshals { get; set; }

      IList<SecurityDefinition> SecurityDefinitions { get; set; }

      IList<ClassLayout> ClassLayouts { get; set; }

      IList<FieldLayout> FieldLayouts { get; set; }

      IList<StandaloneSignature> StandaloneSignatures { get; set; }

      IList<EventMap> EventMaps { get; set; }

      IList<EventDefinition> EventDefinitions { get; set; }

      IList<PropertyMap> PropertyMaps { get; set; }

      IList<PropertyDefinition> PropertyDefinitions { get; set; }

      IList<MethodSemantics> MethodSemanticss { get; set; }

      IList<MethodImplementation> MethodImplementations { get; set; }

      IList<ModuleReference> ModuleReferences { get; set; }

      IList<TypeSpecification> TypeSpecifications { get; set; }

      IList<MethodImplementationMap> MethodImplementationMaps { get; set; }

      IList<FieldRVA> FieldRVAs { get; set; }

      IList<AssemblyDefinition> AssemblyDefinitions { get; set; }

      IList<AssemblyReference> AssemblyReferences { get; set; }

      IList<FileReference> FileReferences { get; set; }

      IList<ExportedTypes> ExportedTypess { get; set; }

      IList<ManifestResource> ManifestResources { get; set; }

      IList<NestedClassDefinition> NestedClassDefinitions { get; set; }

      IList<GenericParameterDefinition> GenericParameterDefinitions { get; set; }

      IList<MethodSpecification> MethodSpecifications { get; set; }

      IList<GenericParameterConstraintDefinition> GenericParameterConstraintDefinitions { get; set; }
   }
}
