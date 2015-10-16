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

namespace CILAssemblyManipulator.Physical.IO
{
   public sealed class RawModuleDefinition
   {
      public Int32 Generation { get; set; }

      public Int32 Name { get; set; }

      public Int32 ModuleGUID { get; set; }

      public Int32 EditAndContinueGUID { get; set; }

      public Int32 EditAndContinueBaseGUID { get; set; }
   }

   public sealed class RawTypeReference
   {
      public Int32 ResolutionScope { get; set; }

      public Int32 Name { get; set; }

      public Int32 Namespace { get; set; }
   }

   public sealed class RawTypeDefinition
   {
      public TypeAttributes Attributes { get; set; }

      public Int32 Name { get; set; }

      public Int32 Namespace { get; set; }

      public Int32 BaseType { get; set; }

      public Int32 FieldList { get; set; }

      public Int32 MethodList { get; set; }
   }

   public sealed class RawFieldDefinition
   {
      public FieldAttributes Attributes { get; set; }

      public Int32 Name { get; set; }

      public Int32 Signature { get; set; }
   }

   public sealed class RawMethodDefinition
   {
      public Int32 RVA { get; set; }

      public MethodImplAttributes ImplementationAttributes { get; set; }

      public MethodAttributes Attributes { get; set; }

      public Int32 Name { get; set; }

      public Int32 Signature { get; set; }

      public Int32 ParameterList { get; set; }
   }

   public sealed class RawParameterDefinition
   {
      public ParameterAttributes Attributes { get; set; }

      public Int32 Sequence { get; set; }

      public Int32 Name { get; set; }
   }

   public sealed class RawInterfaceImplementation
   {
      public Int32 Class { get; set; }

      public Int32 Interface { get; set; }
   }

   public sealed class RawMemberReference
   {
      public Int32 DeclaringType { get; set; }

      public Int32 Name { get; set; }

      public Int32 Signature { get; set; }
   }

   public sealed class RawConstantDefinition
   {
      public SignatureElementTypes Type { get; set; }

      public Byte Padding { get; set; }

      public Int32 Parent { get; set; }

      public Int32 Value { get; set; }
   }

   public sealed class RawCustomAttributeDefinition
   {
      public Int32 Parent { get; set; }

      public Int32 Type { get; set; }

      public Int32 Signature { get; set; }
   }

   public sealed class RawFieldMarshal
   {
      public Int32 Parent { get; set; }

      public Int32 NativeType { get; set; }
   }

   public sealed class RawSecurityDefinition
   {
      public SecurityAction Action { get; set; }

      public Int32 Parent { get; set; }

      public Int32 PermissionSets { get; set; }
   }

   public sealed class RawClassLayout
   {
      public Int32 PackingSize { get; set; }

      public Int32 ClassSize { get; set; }

      public Int32 Parent { get; set; }
   }

   public sealed class RawFieldLayout
   {
      public Int32 Offset { get; set; }

      public Int32 Field { get; set; }
   }

   public sealed class RawStandaloneSignature
   {
      public Int32 Signature { get; set; }

   }

   public sealed class RawEventMap
   {
      public Int32 Parent { get; set; }

      public Int32 EventList { get; set; }
   }

   public sealed class RawEventDefinition
   {
      public EventAttributes Attributes { get; set; }

      public Int32 Name { get; set; }

      public Int32 EventType { get; set; }
   }

   public sealed class RawPropertyMap
   {
      public Int32 Parent { get; set; }

      public Int32 PropertyList { get; set; }
   }

   public sealed class RawPropertyDefinition
   {
      public PropertyAttributes Attributes { get; set; }

      public Int32 Name { get; set; }

      public Int32 Signature { get; set; }
   }

   public sealed class RawMethodSemantics
   {

      public MethodSemanticsAttributes Attributes { get; set; }

      public Int32 Method { get; set; }

      public Int32 Associaton { get; set; }
   }

   public sealed class RawMethodImplementation
   {
      public Int32 Class { get; set; }

      public Int32 MethodBody { get; set; }

      public Int32 MethodDeclaration { get; set; }
   }

   public sealed class RawModuleReference
   {
      public Int32 ModuleName { get; set; }
   }

   public sealed class RawTypeSpecification
   {
      public Int32 Signature { get; set; }
   }

   public sealed class RawMethodImplementationMap
   {

      public PInvokeAttributes Attributes { get; set; }

      public Int32 MemberForwarded { get; set; }

      public Int32 ImportName { get; set; }

      public Int32 ImportScope { get; set; }
   }

   public sealed class RawFieldRVA
   {
      public Int32 RVA { get; set; }

      public Int32 Field { get; set; }
   }

   public sealed class RawAssemblyDefinition
   {
      public AssemblyHashAlgorithm HashAlgorithm { get; set; }

      public Int32 MajorVersion { get; set; }

      public Int32 MinorVersion { get; set; }

      public Int32 BuildNumber { get; set; }

      public Int32 RevisionNumber { get; set; }

      public AssemblyFlags Attributes { get; set; }

      public Int32 PublicKey { get; set; }

      public Int32 Name { get; set; }

      public Int32 Culture { get; set; }

   }

   public sealed class RawAssemblyReference
   {
      public Int32 MajorVersion { get; set; }

      public Int32 MinorVersion { get; set; }

      public Int32 BuildNumber { get; set; }

      public Int32 RevisionNumber { get; set; }

      public AssemblyFlags Attributes { get; set; }

      public Int32 PublicKeyOrToken { get; set; }

      public Int32 Name { get; set; }

      public Int32 Culture { get; set; }

      public Int32 HashValue { get; set; }

   }

   public sealed class RawFileReference
   {
      public FileAttributes Attributes { get; set; }

      public Int32 Name { get; set; }

      public Int32 HashValue { get; set; }
   }

   public sealed class RawExportedType
   {
      public TypeAttributes Attributes { get; set; }

      public Int32 TypeDefinitionIndex { get; set; }

      public Int32 Name { get; set; }

      public Int32 Namespace { get; set; }

      public Int32 Implementation { get; set; }
   }

   public sealed class RawManifestResource
   {
      public Int32 Offset { get; set; }

      public ManifestResourceAttributes Attributes { get; set; }

      public Int32 Name { get; set; }

      public Int32 Implementation { get; set; }

   }

   public sealed class RawNestedClassDefinition
   {
      public Int32 NestedClass { get; set; }

      public Int32 EnclosingClass { get; set; }
   }

   public sealed class RawGenericParameterDefinition
   {
      public Int32 GenericParameterIndex { get; set; }

      public GenericParameterAttributes Attributes { get; set; }

      public Int32 Owner { get; set; }

      public Int32 Name { get; set; }
   }

   public sealed class RawMethodSpecification
   {
      public Int32 Method { get; set; }

      public Int32 Signature { get; set; }
   }

   public sealed class RawGenericParameterConstraintDefinition
   {
      public Int32 Owner { get; set; }

      public Int32 Constraint { get; set; }
   }

   public sealed class RawEditAndContinueLog
   {
      public Int32 Token { get; set; }

      public Int32 FuncCode { get; set; }
   }

   public sealed class RawEditAndContinueMap
   {
      public Int32 Token { get; set; }
   }

   public sealed class RawFieldDefinitionPointer
   {
      public Int32 FieldIndex { get; set; }
   }

   public sealed class RawMethodDefinitionPointer
   {
      public Int32 MethodIndex { get; set; }
   }

   public sealed class RawParameterDefinitionPointer
   {
      public Int32 ParameterIndex { get; set; }
   }

   public sealed class RawEventDefinitionPointer
   {
      public Int32 EventIndex { get; set; }
   }

   public sealed class RawPropertyDefinitionPointer
   {
      public Int32 PropertyIndex { get; set; }
   }

   public sealed class RawAssemblyDefinitionProcessor
   {
      public Int32 Processor { get; set; }
   }

   public sealed class RawAssemblyDefinitionOS
   {
      public Int32 OSPlatformID { get; set; }

      public Int32 OSMajorVersion { get; set; }

      public Int32 OSMinorVersion { get; set; }
   }

   public sealed class RawAssemblyReferenceProcessor
   {
      public Int32 Processor { get; set; }

      public Int32 AssemblyRef { get; set; }
   }

   public sealed class RawAssemblyReferenceOS
   {
      public Int32 OSPlatformID { get; set; }

      public Int32 OSMajorVersion { get; set; }

      public Int32 OSMinorVersion { get; set; }

      public Int32 AssemblyRef { get; set; }

   }

   public interface MetaDataSerializationSupportProvider
   {
      TableSerializationSupportProvider CreateTableSerializationSupportProvider(
         ArrayQuery<Int32> tableSizes,
         Boolean wideBLOBs,
         Boolean wideGUIDs,
         Boolean wideStrings
         );
   }

   public interface TableSerializationSupportProvider
   {
      TableSerializationSupport CreateSerializationSupport(
         Tables table
         );
   }

   public interface TableSerializationSupport
   {
      Tables Table { get; }

      Object ReadRow( StreamHelper stream,
        ReaderBLOBStreamHandler blobs,
         ReaderGUIDStreamHandler guids,
         ReaderStringStreamHandler sysStrings
         );

      Object ReadRawRow( StreamHelper stream );

      ArrayQuery<ColumnSerializationInfo> ColumnSerializationSupports { get; }
   }

   public interface ColumnSerializationInfo
   {
      String ColumnName { get; }

      ColumnSerializationSupport Serialization { get; }

      void SetRawValue( Object row, Int32 value );
   }

   public interface ColumnSerializationSupport
   {
      Int32 ColumnByteCount { get; }

      Int32 ReadRawValue( StreamHelper stream );

      Object ReadValue(
         StreamHelper stream,
         ReaderBLOBStreamHandler blobs,
         ReaderGUIDStreamHandler guids,
         ReaderStringStreamHandler sysStrings
         );

      void WriteValue( StreamHelper stream, Object value );
   }
}
