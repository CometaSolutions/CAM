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
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO
{


   public sealed class RawModuleDefinition
   {
      [CLSCompliant( false )]
      public UInt16 Generation { get; }

      [CLSCompliant( false )]
      public UInt32 Name { get; }

      [CLSCompliant( false )]
      public UInt32 ModuleGUID { get; }

      [CLSCompliant( false )]
      public UInt32 EditAndContinueGUID { get; }

      [CLSCompliant( false )]
      public UInt32 EditAndContinueBaseGUID { get; }
   }

   public sealed class RawTypeReference
   {
      [CLSCompliant( false )]
      public UInt32 ResolutionScope { get; set; }

      [CLSCompliant( false )]
      public UInt32 Name { get; set; }

      [CLSCompliant( false )]
      public UInt32 Namespace { get; set; }
   }

   public sealed class RawTypeDefinition
   {
      public TypeAttributes Attributes { get; set; }

      [CLSCompliant( false )]
      public UInt32 Name { get; set; }

      [CLSCompliant( false )]
      public UInt32 Namespace { get; set; }

      [CLSCompliant( false )]
      public UInt32 BaseType { get; set; }

      [CLSCompliant( false )]
      public UInt32 FieldList { get; set; }

      [CLSCompliant( false )]
      public UInt32 MethodList { get; set; }
   }

   public sealed class RawFieldDefinition
   {
      public FieldAttributes Attributes { get; set; }

      [CLSCompliant( false )]
      public UInt32 Name { get; set; }

      [CLSCompliant( false )]
      public UInt32 Signature { get; set; }
   }

   public sealed class RawMethodDefinition
   {
      [CLSCompliant( false )]
      public UInt32 RVA { get; set; }

      public MethodImplAttributes ImplementationAttributes { get; set; }

      public MethodAttributes Attributes { get; set; }

      [CLSCompliant( false )]
      public UInt32 Name { get; set; }

      [CLSCompliant( false )]
      public UInt32 Signature { get; set; }

      [CLSCompliant( false )]
      public UInt32 ParameterList { get; set; }
   }

   public sealed class RawParameterDefinition
   {
      public ParameterAttributes Attributes { get; set; }

      [CLSCompliant( false )]
      public UInt16 Sequence { get; set; }

      [CLSCompliant( false )]
      public UInt32 Name { get; set; }
   }

   public sealed class RawInterfaceImplementation
   {
      [CLSCompliant( false )]
      public UInt32 Class { get; set; }

      [CLSCompliant( false )]
      public UInt32 Interface { get; set; }
   }

   public sealed class RawMemberReference
   {
      [CLSCompliant( false )]
      public UInt32 DeclaringType { get; set; }

      [CLSCompliant( false )]
      public UInt32 Name { get; set; }

      [CLSCompliant( false )]
      public UInt32 Signature { get; set; }
   }

   public sealed class RawConstantDefinition
   {
      public SignatureElementTypes Type { get; set; }

      public Byte Padding { get; set; }

      [CLSCompliant( false )]
      public UInt32 Parent { get; set; }

      [CLSCompliant( false )]
      public UInt32 Value { get; set; }
   }

   public sealed class RawCustomAttributeDefinition
   {
      [CLSCompliant( false )]
      public UInt32 Parent { get; set; }

      [CLSCompliant( false )]
      public UInt32 Type { get; set; }

      [CLSCompliant( false )]
      public UInt32 Signature { get; set; }
   }

   public sealed class RawFieldMarshal
   {
      [CLSCompliant( false )]
      public UInt32 Parent { get; set; }

      [CLSCompliant( false )]
      public UInt32 NativeType { get; set; }
   }

   public sealed class RawSecurityDefinition
   {
      public SecurityAction Action { get; set; }

      [CLSCompliant( false )]
      public UInt32 Parent { get; set; }

      [CLSCompliant( false )]
      public UInt32 PermissionSets { get; set; }
   }

   public sealed class RawClassLayout
   {
      [CLSCompliant( false )]
      public UInt16 PackingSize { get; set; }

      [CLSCompliant( false )]
      public UInt32 ClassSize { get; set; }

      [CLSCompliant( false )]
      public UInt32 Parent { get; set; }
   }

   public sealed class RawFieldLayout
   {
      [CLSCompliant( false )]
      public UInt32 Offset { get; set; }

      [CLSCompliant( false )]
      public UInt32 Field { get; set; }
   }

   public sealed class RawStandaloneSignature
   {
      [CLSCompliant( false )]
      public UInt32 Signature { get; set; }

   }

   public sealed class RawEventMap
   {
      [CLSCompliant( false )]
      public UInt32 Parent { get; set; }

      [CLSCompliant( false )]
      public UInt32 EventList { get; set; }
   }

   public sealed class RawEventDefinition
   {
      public EventAttributes Attributes { get; set; }

      [CLSCompliant( false )]
      public UInt32 Name { get; set; }

      [CLSCompliant( false )]
      public UInt32 EventType { get; set; }
   }

   public sealed class RawPropertyMap
   {
      [CLSCompliant( false )]
      public UInt32 Parent { get; set; }

      [CLSCompliant( false )]
      public UInt32 PropertyList { get; set; }
   }

   public sealed class RawPropertyDefinition
   {
      public PropertyAttributes Attributes { get; set; }

      [CLSCompliant( false )]
      public UInt32 Name { get; set; }

      [CLSCompliant( false )]
      public UInt32 Signature { get; set; }
   }

   public sealed class RawMethodSemantics
   {

      public MethodSemanticsAttributes Attributes { get; set; }

      [CLSCompliant( false )]
      public UInt32 Method { get; set; }

      [CLSCompliant( false )]
      public UInt32 Associaton { get; set; }
   }

   public sealed class RawMethodImplementation
   {
      [CLSCompliant( false )]
      public UInt32 Class { get; set; }

      [CLSCompliant( false )]
      public UInt32 MethodBody { get; set; }

      [CLSCompliant( false )]
      public UInt32 MethodDeclaration { get; set; }
   }

   public sealed class RawModuleReference
   {
      [CLSCompliant( false )]
      public UInt32 ModuleName { get; set; }
   }

   public sealed class RawTypeSpecification
   {
      [CLSCompliant( false )]
      public UInt32 Signature { get; set; }
   }

   public sealed class RawMethodImplementationMap
   {

      public PInvokeAttributes Attributes { get; set; }

      [CLSCompliant( false )]
      public UInt32 MemberForwarded { get; set; }

      [CLSCompliant( false )]
      public UInt32 ImportName { get; set; }

      [CLSCompliant( false )]
      public UInt32 ImportScope { get; set; }
   }

   public sealed class RawFieldRVA
   {
      [CLSCompliant( false )]
      public UInt32 RVA { get; set; }

      [CLSCompliant( false )]
      public UInt32 Field { get; set; }
   }

   public sealed class RawAssemblyDefinition
   {
      public AssemblyHashAlgorithm HashAlgorithm { get; set; }

      [CLSCompliant( false )]
      public UInt16 MajorVersion { get; set; }

      [CLSCompliant( false )]
      public UInt16 MinorVersion { get; set; }

      [CLSCompliant( false )]
      public UInt16 BuildNumber { get; set; }

      [CLSCompliant( false )]
      public UInt16 RevisionNumber { get; set; }

      public AssemblyFlags Attributes { get; set; }

      [CLSCompliant( false )]
      public UInt32 PublicKey { get; set; }

      [CLSCompliant( false )]
      public UInt32 Name { get; set; }

      [CLSCompliant( false )]
      public UInt32 Culture { get; set; }

   }

   public sealed class RawAssemblyReference
   {
      [CLSCompliant( false )]
      public UInt16 MajorVersion { get; set; }

      [CLSCompliant( false )]
      public UInt16 MinorVersion { get; set; }

      [CLSCompliant( false )]
      public UInt16 BuildNumber { get; set; }

      [CLSCompliant( false )]
      public UInt16 RevisionNumber { get; set; }

      public AssemblyFlags Attributes { get; set; }

      [CLSCompliant( false )]
      public UInt32 PublicKeyOrToken { get; set; }

      [CLSCompliant( false )]
      public UInt32 Name { get; set; }

      [CLSCompliant( false )]
      public UInt32 Culture { get; set; }

      [CLSCompliant( false )]
      public UInt32 HashValue { get; set; }

   }

   public sealed class RawFileReference
   {
      public FileAttributes Attributes { get; set; }

      [CLSCompliant( false )]
      public UInt32 Name { get; set; }

      [CLSCompliant( false )]
      public UInt32 HashValue { get; set; }
   }

   public sealed class RawExportedType
   {
      public TypeAttributes Attributes { get; set; }

      [CLSCompliant( false )]
      public UInt32 TypeDefinitionIndex { get; set; }

      [CLSCompliant( false )]
      public UInt32 Name { get; set; }

      [CLSCompliant( false )]
      public UInt32 Namespace { get; set; }

      [CLSCompliant( false )]
      public UInt32 Implementation { get; set; }
   }

   public sealed class RawManifestResource
   {
      [CLSCompliant( false )]
      public UInt32 Offset { get; set; }

      public ManifestResourceAttributes Attributes { get; set; }

      [CLSCompliant( false )]
      public UInt32 Name { get; set; }

      [CLSCompliant( false )]
      public UInt32 Implementation { get; set; }

   }

   public sealed class RawNestedClassDefinition
   {
      [CLSCompliant( false )]
      public UInt32 NestedClass { get; set; }

      [CLSCompliant( false )]
      public UInt32 EnclosingClass { get; set; }
   }

   public sealed class RawGenericParameterDefinition
   {
      [CLSCompliant( false )]
      public UInt16 GenericParameterIndex { get; set; }

      public GenericParameterAttributes Attributes { get; set; }

      [CLSCompliant( false )]
      public UInt32 Owner { get; set; }

      [CLSCompliant( false )]
      public UInt32 Name { get; set; }
   }

   public sealed class RawMethodSpecification
   {
      [CLSCompliant( false )]
      public UInt32 Method { get; set; }

      [CLSCompliant( false )]
      public UInt32 Signature { get; set; }
   }

   public sealed class RawGenericParameterConstraintDefinition
   {
      [CLSCompliant( false )]
      public UInt32 Owner { get; set; }

      [CLSCompliant( false )]
      public UInt32 Constraint { get; set; }
   }

   public sealed class RawEditAndContinueLog
   {
      [CLSCompliant( false )]
      public UInt32 Token { get; set; }

      [CLSCompliant( false )]
      public UInt32 FuncCode { get; set; }
   }

   public sealed class RawEditAndContinueMap
   {
      [CLSCompliant( false )]
      public UInt32 Token { get; set; }
   }

   public sealed class RawFieldDefinitionPointer
   {
      [CLSCompliant( false )]
      public UInt32 FieldIndex { get; set; }
   }

   public sealed class RawMethodDefinitionPointer
   {
      [CLSCompliant( false )]
      public UInt32 MethodIndex { get; set; }
   }

   public sealed class RawParameterDefinitionPointer
   {
      [CLSCompliant( false )]
      public UInt32 ParameterIndex { get; set; }
   }

   public sealed class RawEventDefinitionPointer
   {
      [CLSCompliant( false )]
      public UInt32 EventIndex { get; set; }
   }

   public sealed class RawPropertyDefinitionPointer
   {
      [CLSCompliant( false )]
      public UInt32 PropertyIndex { get; set; }
   }

   public sealed class RawAssemblyDefinitionProcessor
   {
      [CLSCompliant( false )]
      public UInt32 Processor { get; set; }
   }

   public sealed class RawAssemblyDefinitionOS
   {
      [CLSCompliant( false )]
      public UInt32 OSPlatformID { get; set; }

      [CLSCompliant( false )]
      public UInt32 OSMajorVersion { get; set; }

      [CLSCompliant( false )]
      public UInt32 OSMinorVersion { get; set; }
   }

   public sealed class RawAssemblyReferenceProcessor
   {
      [CLSCompliant( false )]
      public UInt32 Processor { get; set; }

      [CLSCompliant( false )]
      public UInt32 AssemblyRef { get; set; }
   }

   public sealed class RawAssemblyReferenceOS
   {
      [CLSCompliant( false )]
      public UInt32 OSPlatformID { get; set; }

      [CLSCompliant( false )]
      public UInt32 OSMajorVersion { get; set; }

      [CLSCompliant( false )]
      public UInt32 OSMinorVersion { get; set; }

      [CLSCompliant( false )]
      public UInt32 AssemblyRef { get; set; }

   }
}
