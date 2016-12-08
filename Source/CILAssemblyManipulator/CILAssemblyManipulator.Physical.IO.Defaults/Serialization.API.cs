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
extern alias CAMPhysical;
extern alias CAMPhysicalIO;
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;
using CAMPhysical::CILAssemblyManipulator.Physical.Meta;

using CAMPhysicalIO;
using CAMPhysicalIO::CILAssemblyManipulator.Physical.IO;

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.IO.Defaults;
using CILAssemblyManipulator.Physical.Meta;
using UtilPack.CollectionsWithRoles;
using UtilPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TabularMetaData;
using TabularMetaData.Meta;

namespace CILAssemblyManipulator.Physical.IO.Defaults
{
   /// <summary>
   /// This is raw row type for <see cref="ModuleDefinition"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawModuleDefinition
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="ModuleDefinition.Generation"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ModuleDefinition.Generation"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Generation { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="ModuleDefinition.Name"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ModuleDefinition.Name"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Name { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="ModuleDefinition.ModuleGUID"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ModuleDefinition.ModuleGUID"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 ModuleGUID { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="ModuleDefinition.EditAndContinueGUID"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ModuleDefinition.EditAndContinueGUID"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 EditAndContinueGUID { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="ModuleDefinition.EditAndContinueBaseGUID"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ModuleDefinition.EditAndContinueBaseGUID"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 EditAndContinueBaseGUID { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="TypeReference"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawTypeReference
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="TypeReference.ResolutionScope"/>.
      /// </summary>
      /// <value>The raw version of <see cref="TypeReference.ResolutionScope"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 ResolutionScope { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="TypeReference.ResolutionScope"/>.
      /// </summary>
      /// <value>The raw version of <see cref="TypeReference.ResolutionScope"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Name { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="TypeReference.ResolutionScope"/>.
      /// </summary>
      /// <value>The raw version of <see cref="TypeReference.ResolutionScope"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Namespace { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="TypeDefinition"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawTypeDefinition
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="TypeDefinition.Attributes"/>.
      /// </summary>
      /// <value>The raw version of <see cref="TypeDefinition.Attributes"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public TypeAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="TypeDefinition.Name"/>.
      /// </summary>
      /// <value>The raw version of <see cref="TypeDefinition.Name"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Name { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="TypeDefinition.Namespace"/>.
      /// </summary>
      /// <value>The raw version of <see cref="TypeDefinition.Namespace"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Namespace { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="TypeDefinition.BaseType"/>.
      /// </summary>
      /// <value>The raw version of <see cref="TypeDefinition.BaseType"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 BaseType { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="TypeDefinition.FieldList"/>.
      /// </summary>
      /// <value>The raw version of <see cref="TypeDefinition.FieldList"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 FieldList { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="TypeDefinition.MethodList"/>.
      /// </summary>
      /// <value>The raw version of <see cref="TypeDefinition.MethodList"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 MethodList { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="FieldDefinition"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawFieldDefinition
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="FieldDefinition.Attributes"/>.
      /// </summary>
      /// <value>The raw version of <see cref="FieldDefinition.Attributes"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public FieldAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="FieldDefinition.Name"/>.
      /// </summary>
      /// <value>The raw version of <see cref="FieldDefinition.Name"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Name { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="FieldDefinition.Signature"/>.
      /// </summary>
      /// <value>The raw version of <see cref="FieldDefinition.Signature"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Signature { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="MethodDefinition"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawMethodDefinition
   {
      /// <summary>
      /// Gets or sets the RVA of <see cref="MethodDefinition.IL"/>.
      /// </summary>
      /// <value>The RVA of <see cref="MethodDefinition.IL"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 RVA { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="MethodDefinition.ImplementationAttributes"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MethodDefinition.ImplementationAttributes"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public MethodImplAttributes ImplementationAttributes { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="MethodDefinition.Attributes"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MethodDefinition.Attributes"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public MethodAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="MethodDefinition.Name"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MethodDefinition.Name"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Name { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="MethodDefinition.Signature"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MethodDefinition.Signature"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Signature { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="MethodDefinition.ParameterList"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MethodDefinition.ParameterList"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 ParameterList { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="ParameterDefinition"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawParameterDefinition
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="ParameterDefinition.Attributes"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ParameterDefinition.Attributes"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public ParameterAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="ParameterDefinition.Sequence"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ParameterDefinition.Sequence"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Sequence { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="ParameterDefinition.Name"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ParameterDefinition.Name"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Name { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="InterfaceImplementation"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawInterfaceImplementation
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="InterfaceImplementation.Class"/>.
      /// </summary>
      /// <value>The raw version of <see cref="InterfaceImplementation.Class"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Class { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="InterfaceImplementation.Interface"/>.
      /// </summary>
      /// <value>The raw version of <see cref="InterfaceImplementation.Interface"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Interface { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="MemberReference"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawMemberReference
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="MemberReference.DeclaringType"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MemberReference.DeclaringType"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 DeclaringType { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="MemberReference.Name"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MemberReference.Name"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Name { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="MemberReference.Signature"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MemberReference.Signature"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Signature { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="ConstantDefinition"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawConstantDefinition
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="ConstantDefinition.Type"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ConstantDefinition.Type"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public ConstantValueType Type { get; set; }

      /// <summary>
      /// Gets or sets the padding value, which is not present in normal <see cref="ConstantDefinition"/> row.
      /// </summary>
      /// <value>The padding value, which is not present in normal <see cref="ConstantDefinition"/> row.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Byte Padding { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="ConstantDefinition.Parent"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ConstantDefinition.Parent"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Parent { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="ConstantDefinition.Value"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ConstantDefinition.Value"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Value { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="CustomAttributeDefinition"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawCustomAttributeDefinition
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="CustomAttributeDefinition.Parent"/>.
      /// </summary>
      /// <value>The raw version of <see cref="CustomAttributeDefinition.Parent"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Parent { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="CustomAttributeDefinition.Type"/>.
      /// </summary>
      /// <value>The raw version of <see cref="CustomAttributeDefinition.Type"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Type { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="CustomAttributeDefinition.Signature"/>.
      /// </summary>
      /// <value>The raw version of <see cref="CustomAttributeDefinition.Signature"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Signature { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="FieldMarshal"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawFieldMarshal
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="FieldMarshal.Parent"/>.
      /// </summary>
      /// <value>The raw version of <see cref="FieldMarshal.Parent"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Parent { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="FieldMarshal.NativeType"/>.
      /// </summary>
      /// <value>The raw version of <see cref="FieldMarshal.NativeType"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 NativeType { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="SecurityDefinition"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawSecurityDefinition
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="SecurityDefinition.Action"/>.
      /// </summary>
      /// <value>The raw version of <see cref="SecurityDefinition.Action"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public SecurityAction Action { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="SecurityDefinition.Parent"/>.
      /// </summary>
      /// <value>The raw version of <see cref="SecurityDefinition.Parent"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Parent { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="SecurityDefinition.PermissionSets"/>.
      /// </summary>
      /// <value>The raw version of <see cref="SecurityDefinition.PermissionSets"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 PermissionSets { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="ClassLayout"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawClassLayout
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="ClassLayout.PackingSize"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ClassLayout.PackingSize"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 PackingSize { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="ClassLayout.ClassSize"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ClassLayout.ClassSize"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 ClassSize { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="ClassLayout.Parent"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ClassLayout.Parent"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Parent { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="FieldLayout"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawFieldLayout
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="FieldLayout.Offset"/>.
      /// </summary>
      /// <value>The raw version of <see cref="FieldLayout.Offset"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Offset { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="FieldLayout.Field"/>.
      /// </summary>
      /// <value>The raw version of <see cref="FieldLayout.Field"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Field { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="StandaloneSignature"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawStandaloneSignature
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="StandaloneSignature.Signature"/>.
      /// </summary>
      /// <value>The raw version of <see cref="StandaloneSignature.Signature"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Signature { get; set; }

   }

   /// <summary>
   /// This is raw row type for <see cref="EventMap"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawEventMap
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="EventMap.Parent"/>.
      /// </summary>
      /// <value>The raw version of <see cref="EventMap.Parent"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Parent { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="EventMap.EventList"/>.
      /// </summary>
      /// <value>The raw version of <see cref="EventMap.EventList"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 EventList { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="EventDefinition"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawEventDefinition
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="EventDefinition.Attributes"/>.
      /// </summary>
      /// <value>The raw version of <see cref="EventDefinition.Attributes"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public EventAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="EventDefinition.Name"/>.
      /// </summary>
      /// <value>The raw version of <see cref="EventDefinition.Name"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Name { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="EventDefinition.EventType"/>.
      /// </summary>
      /// <value>The raw version of <see cref="EventDefinition.EventType"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 EventType { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="PropertyMap"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawPropertyMap
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="PropertyMap.Parent"/>.
      /// </summary>
      /// <value>The raw version of <see cref="PropertyMap.Parent"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Parent { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="PropertyMap.PropertyList"/>.
      /// </summary>
      /// <value>The raw version of <see cref="PropertyMap.PropertyList"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 PropertyList { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="PropertyDefinition"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawPropertyDefinition
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="PropertyDefinition.Attributes"/>.
      /// </summary>
      /// <value>The raw version of <see cref="PropertyDefinition.Attributes"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public PropertyAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="PropertyDefinition.Name"/>.
      /// </summary>
      /// <value>The raw version of <see cref="PropertyDefinition.Name"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Name { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="PropertyDefinition.Signature"/>.
      /// </summary>
      /// <value>The raw version of <see cref="PropertyDefinition.Signature"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Signature { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="MethodSemantics"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawMethodSemantics
   {

      /// <summary>
      /// Gets or sets the raw version of <see cref="MethodSemantics.Attributes"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MethodSemantics.Attributes"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public MethodSemanticsAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="MethodSemantics.Method"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MethodSemantics.Method"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Method { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="MethodSemantics.Associaton"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MethodSemantics.Associaton"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Associaton { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="MethodImplementation"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawMethodImplementation
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="MethodImplementation.Class"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MethodImplementation.Class"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Class { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="MethodImplementation.MethodBody"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MethodImplementation.MethodBody"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 MethodBody { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="MethodImplementation.MethodDeclaration"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MethodImplementation.MethodDeclaration"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 MethodDeclaration { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="ModuleReference"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawModuleReference
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="ModuleReference.ModuleName"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ModuleReference.ModuleName"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 ModuleName { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="TypeSpecification"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawTypeSpecification
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="TypeSpecification.Signature"/>.
      /// </summary>
      /// <value>The raw version of <see cref="TypeSpecification.Signature"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Signature { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="MethodImplementationMap"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawMethodImplementationMap
   {

      /// <summary>
      /// Gets or sets the raw version of <see cref="MethodImplementationMap.Attributes"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MethodImplementationMap.Attributes"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public PInvokeAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="MethodImplementationMap.MemberForwarded"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MethodImplementationMap.MemberForwarded"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 MemberForwarded { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="MethodImplementationMap.ImportName"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MethodImplementationMap.ImportName"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 ImportName { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="MethodImplementationMap.ImportScope"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MethodImplementationMap.ImportScope"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 ImportScope { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="FieldRVA"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawFieldRVA
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="FieldRVA.Data"/>.
      /// </summary>
      /// <value>The raw version of <see cref="FieldRVA.Data"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 RVA { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="FieldRVA.Field"/>.
      /// </summary>
      /// <value>The raw version of <see cref="FieldRVA.Field"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Field { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="AssemblyDefinition"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawAssemblyDefinition
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyDefinition.HashAlgorithm"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyDefinition.HashAlgorithm"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public AssemblyHashAlgorithm HashAlgorithm { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyInformation.VersionMajor"/> of <see cref="AssemblyDefinition.AssemblyInformation"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyInformation.VersionMajor"/> of <see cref="AssemblyDefinition.AssemblyInformation"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 MajorVersion { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyInformation.VersionMinor"/> of <see cref="AssemblyDefinition.AssemblyInformation"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyInformation.VersionMinor"/> of <see cref="AssemblyDefinition.AssemblyInformation"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 MinorVersion { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyInformation.VersionBuild"/> of <see cref="AssemblyDefinition.AssemblyInformation"/>.
      /// </summary>
      /// <value>The raw version of<see cref="AssemblyInformation.VersionBuild"/> of <see cref="AssemblyDefinition.AssemblyInformation"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 BuildNumber { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyInformation.VersionRevision"/> of <see cref="AssemblyDefinition.AssemblyInformation"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyInformation.VersionRevision"/> of <see cref="AssemblyDefinition.AssemblyInformation"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 RevisionNumber { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyDefinition.Attributes"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyDefinition.Attributes"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public AssemblyFlags Attributes { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyInformation.PublicKeyOrToken"/> of <see cref="AssemblyDefinition.AssemblyInformation"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyInformation.PublicKeyOrToken"/> of <see cref="AssemblyDefinition.AssemblyInformation"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 PublicKey { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyInformation.Name"/> of <see cref="AssemblyDefinition.AssemblyInformation"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyInformation.Name"/> of <see cref="AssemblyDefinition.AssemblyInformation"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Name { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyInformation.Culture"/> of <see cref="AssemblyDefinition.AssemblyInformation"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyInformation.Culture"/> of <see cref="AssemblyDefinition.AssemblyInformation"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Culture { get; set; }

   }

   /// <summary>
   /// This is raw row type for <see cref="AssemblyReference"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawAssemblyReference
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyInformation.VersionMajor"/> of <see cref="AssemblyReference.AssemblyInformation"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyInformation.VersionMajor"/> of <see cref="AssemblyReference.AssemblyInformation"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 MajorVersion { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyInformation.VersionMinor"/> of <see cref="AssemblyReference.AssemblyInformation"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyInformation.VersionMinor"/> of <see cref="AssemblyReference.AssemblyInformation"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 MinorVersion { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyInformation.VersionBuild"/> of <see cref="AssemblyReference.AssemblyInformation"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyInformation.VersionBuild"/> of <see cref="AssemblyReference.AssemblyInformation"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 BuildNumber { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyInformation.VersionRevision"/> of <see cref="AssemblyReference.AssemblyInformation"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyInformation.VersionRevision"/> of <see cref="AssemblyReference.AssemblyInformation"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 RevisionNumber { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyReference.Attributes"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyReference.Attributes"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public AssemblyFlags Attributes { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyInformation.PublicKeyOrToken"/> of <see cref="AssemblyReference.AssemblyInformation"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyInformation.PublicKeyOrToken"/> of <see cref="AssemblyReference.AssemblyInformation"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 PublicKeyOrToken { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyInformation.Name"/> of <see cref="AssemblyReference.AssemblyInformation"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyInformation.Name"/> of <see cref="AssemblyReference.AssemblyInformation"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Name { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyInformation.Culture"/> of <see cref="AssemblyReference.AssemblyInformation"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyInformation.Culture"/> of <see cref="AssemblyReference.AssemblyInformation"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Culture { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyReference.HashValue"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyReference.HashValue"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 HashValue { get; set; }

   }

   /// <summary>
   /// This is raw row type for <see cref="FileReference"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawFileReference
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="FileReference.Attributes"/>.
      /// </summary>
      /// <value>The raw version of <see cref="FileReference.Attributes"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public CAMPhysical::CILAssemblyManipulator.Physical.FileAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="FileReference.Name"/>.
      /// </summary>
      /// <value>The raw version of <see cref="FileReference.Name"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Name { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="FileReference.HashValue"/>.
      /// </summary>
      /// <value>The raw version of <see cref="FileReference.HashValue"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 HashValue { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="ExportedType"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawExportedType
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="ExportedType.Attributes"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ExportedType.Attributes"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public TypeAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="ExportedType.TypeDefinitionIndex"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ExportedType.TypeDefinitionIndex"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 TypeDefinitionIndex { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="ExportedType.Name"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ExportedType.Name"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Name { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="ExportedType.Namespace"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ExportedType.Namespace"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Namespace { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="ExportedType.Implementation"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ExportedType.Implementation"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Implementation { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="ManifestResource"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawManifestResource
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="ManifestResource.Offset"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ManifestResource.Offset"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Offset { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="ManifestResource.Attributes"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ManifestResource.Attributes"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public ManifestResourceAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="ManifestResource.Name"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ManifestResource.Name"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Name { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="ManifestResource.Implementation"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ManifestResource.Implementation"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Implementation { get; set; }

   }

   /// <summary>
   /// This is raw row type for <see cref="NestedClassDefinition"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawNestedClassDefinition
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="NestedClassDefinition.NestedClass"/>.
      /// </summary>
      /// <value>The raw version of <see cref="NestedClassDefinition.NestedClass"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 NestedClass { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="NestedClassDefinition.EnclosingClass"/>.
      /// </summary>
      /// <value>The raw version of <see cref="NestedClassDefinition.EnclosingClass"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 EnclosingClass { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="GenericParameterDefinition"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawGenericParameterDefinition
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="GenericParameterDefinition.GenericParameterIndex"/>.
      /// </summary>
      /// <value>The raw version of <see cref="GenericParameterDefinition.GenericParameterIndex"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 GenericParameterIndex { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="GenericParameterDefinition.Attributes"/>.
      /// </summary>
      /// <value>The raw version of <see cref="GenericParameterDefinition.Attributes"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public GenericParameterAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="GenericParameterDefinition.Owner"/>.
      /// </summary>
      /// <value>The raw version of <see cref="GenericParameterDefinition.Owner"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Owner { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="GenericParameterDefinition.Name"/>.
      /// </summary>
      /// <value>The raw version of <see cref="GenericParameterDefinition.Name"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Name { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="MethodSpecification"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawMethodSpecification
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="MethodSpecification.Method"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MethodSpecification.Method"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Method { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="MethodSpecification.Signature"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MethodSpecification.Signature"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Signature { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="GenericParameterConstraintDefinition"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawGenericParameterConstraintDefinition
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="GenericParameterConstraintDefinition.Owner"/>.
      /// </summary>
      /// <value>The raw version of <see cref="GenericParameterConstraintDefinition.Owner"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Owner { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="GenericParameterConstraintDefinition.Constraint"/>.
      /// </summary>
      /// <value>The raw version of <see cref="GenericParameterConstraintDefinition.Constraint"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Constraint { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="EditAndContinueLog"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawEditAndContinueLog
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="EditAndContinueLog.Token"/>.
      /// </summary>
      /// <value>The raw version of <see cref="EditAndContinueLog.Token"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Token { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="EditAndContinueLog.FuncCode"/>.
      /// </summary>
      /// <value>The raw version of <see cref="EditAndContinueLog.FuncCode"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 FuncCode { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="EditAndContinueMap"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawEditAndContinueMap
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="EditAndContinueMap.Token"/>.
      /// </summary>
      /// <value>The raw version of <see cref="EditAndContinueMap.Token"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Token { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="FieldDefinitionPointer"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawFieldDefinitionPointer
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="FieldDefinitionPointer.FieldIndex"/>.
      /// </summary>
      /// <value>The raw version of <see cref="FieldDefinitionPointer.FieldIndex"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 FieldIndex { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="MethodDefinitionPointer"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawMethodDefinitionPointer
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="MethodDefinitionPointer.MethodIndex"/>.
      /// </summary>
      /// <value>The raw version of <see cref="MethodDefinitionPointer.MethodIndex"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 MethodIndex { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="ParameterDefinitionPointer"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawParameterDefinitionPointer
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="ParameterDefinitionPointer.ParameterIndex"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ParameterDefinitionPointer.ParameterIndex"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 ParameterIndex { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="EventDefinitionPointer"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawEventDefinitionPointer
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="EventDefinitionPointer.EventIndex"/>.
      /// </summary>
      /// <value>The raw version of <see cref="EventDefinitionPointer.EventIndex"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 EventIndex { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="PropertyDefinitionPointer"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawPropertyDefinitionPointer
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="PropertyDefinitionPointer.PropertyIndex"/>.
      /// </summary>
      /// <value>The raw version of <see cref="PropertyDefinitionPointer.PropertyIndex"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 PropertyIndex { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="AssemblyDefinitionProcessor"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawAssemblyDefinitionProcessor
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyDefinitionProcessor.Processor"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyDefinitionProcessor.Processor"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Processor { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="AssemblyDefinitionOS"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawAssemblyDefinitionOS
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyDefinitionOS.OSPlatformID"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyDefinitionOS.OSPlatformID"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 OSPlatformID { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyDefinitionOS.OSMajorVersion"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyDefinitionOS.OSMajorVersion"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 OSMajorVersion { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyDefinitionOS.OSMinorVersion"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyDefinitionOS.OSMinorVersion"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 OSMinorVersion { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="AssemblyReferenceProcessor"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawAssemblyReferenceProcessor
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyReferenceProcessor.Processor"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyReferenceProcessor.Processor"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Processor { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyReferenceProcessor.AssemblyRef"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyReferenceProcessor.AssemblyRef"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 AssemblyRef { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="AssemblyReferenceOS"/>.
   /// </summary>
   /// <remarks>
   /// Typically these objects are acquired through <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
   /// </remarks>
   public sealed class RawAssemblyReferenceOS
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyReferenceOS.OSPlatformID"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyReferenceOS.OSPlatformID"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 OSPlatformID { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyReferenceOS.OSMajorVersion"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyReferenceOS.OSMajorVersion"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 OSMajorVersion { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyReferenceOS.OSMinorVersion"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyReferenceOS.OSMinorVersion"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 OSMinorVersion { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="AssemblyReferenceOS.AssemblyRef"/>.
      /// </summary>
      /// <value>The raw version of <see cref="AssemblyReferenceOS.AssemblyRef"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 AssemblyRef { get; set; }

   }

   /// <summary>
   /// This interface contains methods relevant to (de)serialization of rows of a single table.
   /// The <see cref="TableSerializationLogicalFunctionality"/> is agnostic to the byte size of a single row, that is what <see cref="TableSerializationBinaryFunctionality"/> is for, obtaineable by <see cref="CreateBinaryFunctionality"/> method.
   /// </summary>
   /// <remarks>
   /// This interface is implemented by <see cref="TableSerializationLogicalFunctionalityImpl{TRow, TRawRow}"/>.
   /// </remarks>
   public interface TableSerializationLogicalFunctionality
   {
      /// <summary>
      /// Gets the table ID as <see cref="Tables"/> enumeration.
      /// </summary>
      /// <value>The table ID as <see cref="Tables"/> enumeration.</value>
      Tables Table { get; }

      /// <summary>
      /// Gets the value indicating whether this table is considered to be sorted.
      /// This value will affect <see cref="MetaDataTableStreamHeader.SortedTablesBitVector"/> when writing module.
      /// </summary>
      /// <value>The value indicating whether this table is considered to be sorted.</value>
      Boolean IsSorted { get; }

      /// <summary>
      /// Creates the <see cref="TableSerializationBinaryFunctionality"/> object, which can be queried about column byte sizes.
      /// </summary>
      /// <param name="args">The <see cref="TableSerializationBinaryFunctionalityCreationArgs"/>.</param>
      /// <returns>A new instance of <see cref="TableSerializationBinaryFunctionality"/>.</returns>
      TableSerializationBinaryFunctionality CreateBinaryFunctionality(
         TableSerializationBinaryFunctionalityCreationArgs args
         );

      /// <summary>
      /// Gets the number of data reference columns in this table.
      /// </summary>
      /// <value>The number of data reference columns in this table.</value>
      Int32 DataReferenceColumnCount { get; }

      /// <summary>
      /// Gets the number of meta data stream ('heap') reference columns in this table.
      /// </summary>
      /// <value>The number of meta data stream ('heap') reference columns in this table.</value>
      Int32 MetaDataStreamReferenceColumnCount { get; }

      /// <summary>
      /// Given the data references, sets all data reference values to their deserialized version (e.g. <see cref="MethodDefinition.IL"/>).
      /// </summary>
      /// <param name="args">The <see cref="DataReferencesProcessingArgs"/>.</param>
      /// <remarks>
      /// This method is used when deserializing (reading) a module.
      /// </remarks>
      void PopulateDataReferences(
         DataReferencesProcessingArgs args
         );

      /// <summary>
      /// This method creates a <see cref="SectionPartFunctionalityWithDataReferenceTargets"/> object for each of the data reference columns.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/>.</param>
      /// <param name="mdStreamContainer">The <see cref="WriterMetaDataStreamContainer"/>.</param>
      /// <returns>An enumeraboe of <see cref="SectionPartFunctionalityWithDataReferenceTargets"/>, one for each data reference column.</returns>
      /// <remarks>
      /// This method is used when serializing (writing) a module.
      /// </remarks>
      IEnumerable<SectionPartFunctionalityWithDataReferenceTargets> CreateDataReferenceSectionParts(
         CILMetaData md,
         WriterMetaDataStreamContainer mdStreamContainer
         );

      /// <summary>
      /// This method extracts and stores all meta data stream reference values to given <see cref="ColumnValueStorage{TValue}"/>.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/>.</param>
      /// <param name="storage">The <see cref="ColumnValueStorage{TValue}"/> where to store the meta data stream reference values.</param>
      /// <param name="mdStreamContainer">The <see cref="WriterMetaDataStreamContainer"/>.</param>
      /// <param name="array">The auxiliary byte array.</param>
      /// <param name="publicKey">The processed and actual public key of the assembly. May be <c>null</c>.</param>
      /// <remarks>
      /// This method is used when serializing (writing) a module.
      /// </remarks>
      void ExtractMetaDataStreamReferences(
         CILMetaData md,
         ColumnValueStorage<Int32> storage,
         WriterMetaDataStreamContainer mdStreamContainer,
         ResizableArray<Byte> array,
         ArrayQuery<Byte> publicKey
         );

      /// <summary>
      /// This method returns all the columns of all the rows of this table, as integers.
      /// </summary>
      /// <param name="table">The <see cref="MetaDataTable"/>.</param>
      /// <param name="dataReferences">The data references, from <see cref="DataReferencesInfo.DataReferences"/>.</param>
      /// <param name="heapIndices">The meta data stream references, after they been populated by <see cref="ExtractMetaDataStreamReferences"/> method.</param>
      /// <returns>An enumerable of all columns of all the rows of this table, as integers and in correct order.</returns>
      /// <remarks>
      /// This method is used when serializing (writing) a module.
      /// </remarks>
      IEnumerable<Int32> GetAllRawValues(
         MetaDataTable table,
         ArrayQuery<ArrayQuery<Int64>> dataReferences,
         ColumnValueStorage<Int32> heapIndices
         );
   }

   /// <summary>
   /// This class can be used to store values, e.g. data references.
   /// The raw value storage has a pre-set capacity, which can not changed.
   /// </summary>
   /// <typeparam name="TValue">The type of the values to store.</typeparam>
   public sealed class ColumnValueStorage<TValue>
   {
      private readonly Int32[] _tableColCount;
      private readonly Int32[] _tableStartOffsets;
      private readonly TValue[] _rawValues;
      private Int32 _currentIndex;

      /// <summary>
      /// Creates a new instance of <see cref="ColumnValueStorage{TValue}"/> with given information about table sizes and raw value column count for each table.
      /// </summary>
      /// <param name="tableSizes">The table size array. The index of the array is value of <see cref="Tables"/> enumeration, and the value in that array is the size of that table. So if <see cref="Tables.Module"/> would have 1 element, the element at index <c>0</c> (value of <see cref="Tables.Module"/>) would be <c>1</c>.</param>
      /// <param name="rawColumnInfo">The count of raw value columns for each table. The index of the array is value of <see cref="Tables"/> enumeration, and the value in that array is the raw column count. Since <see cref="Tables.MethodDef"/> has one raw value column (the method IL RVA), the element at index <c>6</c> (value of <see cref="Tables.MethodDef"/>) would be <c>1</c>.</param>
      public ColumnValueStorage(
         ArrayQuery<Int32> tableSizes,
         IEnumerable<Int32> rawColumnInfo
         )
      {
         this.TableSizes = tableSizes;
         this._tableColCount = rawColumnInfo.ToArray();
         this._tableStartOffsets = tableSizes
            .AggregateIntermediate_BeforeAggregation(
               0,
               ( cur, size, idx ) => cur += size * this._tableColCount[idx]
               )
            .ToArray();
         this._rawValues = new TValue[tableSizes.Select( ( size, idx ) => size * this._tableColCount[idx] ).Sum()];
         this._currentIndex = 0;
      }

      /// <summary>
      /// Appends raw value to the end of the list of the raw values.
      /// </summary>
      /// <param name="rawValue">The raw value to append.</param>
      /// <exception cref="IndexOutOfRangeException">If this <see cref="ColumnValueStorage{TValue}"/> has already been filled.</exception>
      public void AddRawValue( TValue rawValue )
      {
         this._rawValues[this._currentIndex++] = rawValue;
      }

      /// <summary>
      /// Gets all raw values for a given column in a given table.
      /// </summary>
      /// <param name="table">The <see cref="Tables"/> value.</param>
      /// <param name="columnIndex">The raw column index among all the raw columns in <paramref name="table"/>.</param>
      /// <returns>Enumerable of all raw values for given column.</returns>
      public IEnumerable<TValue> GetAllRawValuesForColumn( Tables table, Int32 columnIndex )
      {
         var colCount = this._tableColCount[(Int32) table];
         var start = this._tableStartOffsets[(Int32) table] + columnIndex;
         var max = start + this.TableSizes[(Int32) table] * colCount;
         for ( var i = start; i < max; i += colCount )
         {
            yield return this._rawValues[i];
         }
      }

      /// <summary>
      /// Gets all raw values for a given row in a given table.
      /// </summary>
      /// <param name="table">The <see cref="Tables"/> value.</param>
      /// <param name="rowIndex">The zero-based index of the row.</param>
      /// <returns>Enumerable of all raw values for given row.</returns>
      public IEnumerable<TValue> GetAllRawValuesForRow( Tables table, Int32 rowIndex )
      {
         var size = this._tableColCount[(Int32) table];
         var startOffset = this._tableStartOffsets[(Int32) table] + rowIndex * size;
         for ( var i = 0; i < size; ++i )
         {
            yield return this._rawValues[startOffset];
            ++startOffset;
         }
      }

      /// <summary>
      /// Gets raw value for a given row in a given table, at a given column index.
      /// </summary>
      /// <param name="table">The <see cref="Tables"/> value.</param>
      /// <param name="rowIndex">The zero-based row index.</param>
      /// <param name="columnIndex">The zero-based column index amongst all raw value columns in the table.</param>
      /// <returns>The value previously stored at specified table, row, and column.</returns>
      public TValue GetRawValue( Tables table, Int32 rowIndex, Int32 columnIndex )
      {
         return this._rawValues[this.GetArrayIndex( (Int32) table, rowIndex, columnIndex )];
      }

      /// <summary>
      /// Sets raw value for a given row in a given table, at a given column index.
      /// </summary>
      /// <param name="table">The <see cref="Tables"/> value.</param>
      /// <param name="rowIndex">The zero-based row index.</param>
      /// <param name="columnIndex">The zero-based column index amongst all raw value columns in the table.</param>
      /// <param name="value">The value to set.</param>
      public void SetRawValue( Tables table, Int32 rowIndex, Int32 columnIndex, TValue value )
      {
         this._rawValues[this.GetArrayIndex( (Int32) table, rowIndex, columnIndex )] = value;
      }

      /// <summary>
      /// Gets the enumerable representing tables which have at least one storable column value specified.
      /// </summary>
      /// <returns>The enumerable representing tables which have at least one storable column value specified.</returns>
      public IEnumerable<Tables> GetPresentTables()
      {
         var tableColCount = this._tableColCount;
         for ( var i = 0; i < tableColCount.Length; ++i )
         {
            if ( tableColCount[i] > 0 )
            {
               yield return (Tables) i;
            }
         }
      }

      /// <summary>
      /// Gets the amount of stored column values for a specific table.
      /// </summary>
      /// <param name="table">The table.</param>
      /// <returns>The amount of stored column values for a specific table.</returns>
      public Int32 GetStoredColumnsCount( Tables table )
      {
         return this._tableColCount[(Int32) table];
      }

      /// <summary>
      /// Gets the table size array.
      /// </summary>
      /// <value>The table size array.</value>
      public ArrayQuery<Int32> TableSizes { get; }

      private Int32 GetArrayIndex( Int32 table, Int32 row, Int32 col )
      {
         return this._tableStartOffsets[table] + row * this._tableColCount[table] + col;
      }

   }

   /// <summary>
   /// This class contains all information required to create a <see cref="TableSerializationBinaryFunctionality"/> object with <see cref="TableSerializationLogicalFunctionality.CreateBinaryFunctionality"/> method.
   /// </summary>
   public class TableSerializationBinaryFunctionalityCreationArgs
   {
      /// <summary>
      /// Creates a new instance of <see cref="TableSerializationBinaryFunctionalityCreationArgs"/> with given size information about tables and meta data streams.
      /// </summary>
      /// <param name="tableSizes">The table size array, where each element is the number of rows in table.</param>
      /// <param name="streamSizes">The meta data stream size dictionary, where key is the name of the stream and value the size of the stream in bytes.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="tableSizes"/> or <paramref name="streamSizes"/> is <c>null</c>.</exception>
      public TableSerializationBinaryFunctionalityCreationArgs(
         ArrayQuery<Int32> tableSizes,
         DictionaryQuery<String, Int32> streamSizes
         )
      {
         ArgumentValidator.ValidateNotNull( "Table sizes", tableSizes );
         ArgumentValidator.ValidateNotNull( "Stream sizes", streamSizes );

         this.TableSizes = tableSizes;
         this.StreamSizes = streamSizes;
      }

      /// <summary>
      /// Gets the table size array.
      /// </summary>
      /// <value>The table size array.</value>
      public ArrayQuery<Int32> TableSizes { get; }

      /// <summary>
      /// Gets the meta data stream size dictionary.
      /// </summary>
      /// <value>The meta data stream size dictionary.</value>
      /// <seealso cref="E_CILPhysical.IsWide(TableSerializationBinaryFunctionalityCreationArgs, String)"/>
      public DictionaryQuery<String, Int32> StreamSizes { get; }

   }

   /// <summary>
   /// This interface provides methods to actually read table rows from binary data.
   /// </summary>
   public interface TableSerializationBinaryFunctionality
   {
      /// <summary>
      /// Gets the <see cref="TableSerializationLogicalFunctionality"/> that this <see cref="TableSerializationBinaryFunctionality"/> was created from.
      /// </summary>
      /// <value>The <see cref="TableSerializationLogicalFunctionality"/> that this <see cref="TableSerializationBinaryFunctionality"/> was created from.</value>
      TableSerializationLogicalFunctionality LogicalFunctionality { get; }

      /// <summary>
      /// Reads the raw row from given byte array, starting at given index.
      /// </summary>
      /// <param name="array">The byte array.</param>
      /// <param name="idx">The index in the <paramref name="array"/> where to start reading row columns.</param>
      /// <returns>A raw row object.</returns>
      /// <remarks>
      /// <para>
      /// The type of the returned object depends on the <see cref="TableSerializationLogicalFunctionality.Table"/> as follows:
      /// <list type="table">
      /// <listheader>
      /// <term>The value of <see cref="TableSerializationLogicalFunctionality.Table"/> of this <see cref="LogicalFunctionality"/></term>
      /// <term>The type of the returned object</term>
      /// </listheader>
      /// <item>
      /// <term><see cref="Tables.Module"/></term>
      /// <term><see cref="RawModuleDefinition"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.TypeRef"/></term>
      /// <term><see cref="RawTypeReference"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.TypeDef"/></term>
      /// <term><see cref="RawTypeDefinition"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.FieldPtr"/></term>
      /// <term><see cref="RawFieldDefinitionPointer"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.Field"/></term>
      /// <term><see cref="RawFieldDefinition"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.MethodPtr"/></term>
      /// <term><see cref="RawMethodDefinitionPointer"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.MethodDef"/></term>
      /// <term><see cref="RawMethodDefinition"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.ParameterPtr"/></term>
      /// <term><see cref="RawParameterDefinitionPointer"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.Parameter"/></term>
      /// <term><see cref="RawParameterDefinition"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.InterfaceImpl"/></term>
      /// <term><see cref="RawInterfaceImplementation"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.MemberRef"/></term>
      /// <term><see cref="RawMemberReference"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.Constant"/></term>
      /// <term><see cref="RawConstantDefinition"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.CustomAttribute"/></term>
      /// <term><see cref="RawCustomAttributeDefinition"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.FieldMarshal"/></term>
      /// <term><see cref="RawFieldMarshal"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.DeclSecurity"/></term>
      /// <term><see cref="RawSecurityDefinition"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.ClassLayout"/></term>
      /// <term><see cref="RawClassLayout"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.FieldLayout"/></term>
      /// <term><see cref="RawFieldLayout"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.StandaloneSignature"/></term>
      /// <term><see cref="RawStandaloneSignature"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.EventMap"/></term>
      /// <term><see cref="RawEventMap"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.EventPtr"/></term>
      /// <term><see cref="RawEventDefinitionPointer"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.Event"/></term>
      /// <term><see cref="RawEventDefinition"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.PropertyMap"/></term>
      /// <term><see cref="RawPropertyMap"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.PropertyPtr"/></term>
      /// <term><see cref="RawPropertyDefinitionPointer"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.Property"/></term>
      /// <term><see cref="RawPropertyDefinition"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.MethodSemantics"/></term>
      /// <term><see cref="RawMethodSemantics"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.MethodImpl"/></term>
      /// <term><see cref="RawMethodImplementation"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.ModuleRef"/></term>
      /// <term><see cref="RawModuleReference"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.TypeSpec"/></term>
      /// <term><see cref="RawTypeSpecification"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.ImplMap"/></term>
      /// <term><see cref="RawMethodImplementationMap"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.FieldRVA"/></term>
      /// <term><see cref="RawFieldRVA"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.EncLog"/></term>
      /// <term><see cref="RawEditAndContinueLog"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.EncMap"/></term>
      /// <term><see cref="RawEditAndContinueMap"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.Assembly"/></term>
      /// <term><see cref="RawAssemblyDefinition"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.AssemblyProcessor"/></term>
      /// <term><see cref="RawAssemblyDefinitionProcessor"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.AssemblyOS"/></term>
      /// <term><see cref="RawAssemblyDefinitionOS"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.AssemblyRef"/></term>
      /// <term><see cref="RawAssemblyReference"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.AssemblyRefProcessor"/></term>
      /// <term><see cref="RawAssemblyReferenceProcessor"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.AssemblyRefOS"/></term>
      /// <term><see cref="RawAssemblyReferenceOS"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.File"/></term>
      /// <term><see cref="RawFileReference"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.ExportedType"/></term>
      /// <term><see cref="RawExportedType"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.ManifestResource"/></term>
      /// <term><see cref="RawManifestResource"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.NestedClass"/></term>
      /// <term><see cref="RawNestedClassDefinition"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.GenericParameter"/></term>
      /// <term><see cref="RawGenericParameterDefinition"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.MethodSpec"/></term>
      /// <term><see cref="RawMethodSpecification"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="Tables.GenericParameterConstraint"/></term>
      /// <term><see cref="RawGenericParameterConstraintDefinition"/></term>
      /// </item>
      /// <item>
      /// <term>Any other value</term>
      /// <term>Depends on the raw row creation callback passed to <see cref="CILMetaDataTableInformationProviderFactory.CreateSingleTableInfo"/></term>
      /// </item>
      /// </list>
      /// </para>
      /// <para>
      /// This method is not used directly by neither serialization (writing) nor deserialization (reading) process.
      /// It is provided to be used if meta data needs to be examined before actually reading the table stream.
      /// </para>
      /// </remarks>
      Object ReadRawRow( Byte[] array, Int32 idx );

      /// <summary>
      /// Reads the given amount of rows into given <see cref="MetaDataTable"/>.
      /// </summary>
      /// <param name="table">The <see cref="MetaDataTable"/> to read rows into.</param>
      /// <param name="rowsToRead">The amount of rows to read.</param>
      /// <param name="array">The byte array to read rows from.</param>
      /// <param name="index">The offset in <paramref name="array"/> where to start reading rows.</param>
      /// <param name="args">The <see cref="RowReadingArguments"/> object, further specifying additional information for deserializing column values.</param>
      void ReadRows(
         MetaDataTable table,
         Int32 rowsToRead,
         Byte[] array,
         Int32 index,
         RowReadingArguments args
         );

      /// <summary>
      /// Gets the array of <see cref="ColumnSerializationBinaryFunctionality"/> objects for each of the column of this table.
      /// </summary>
      /// <value>The array of <see cref="ColumnSerializationBinaryFunctionality"/> objects for each of the column of this table.</value>
      ArrayQuery<ColumnSerializationBinaryFunctionality> ColumnSerializationSupports { get; }
   }

   /// <summary>
   /// This class contains required information to read one normal column value (not a data reference) of a single table row.
   /// </summary>
   public class RowReadingArguments
   {
      /// <summary>
      /// Creates new instance of <see cref="RowReadingArguments"/> with given meta data stream container, data references, and signature provider.
      /// </summary>
      /// <param name="mdStreamContainer">The <see cref="ReaderMetaDataStreamContainer"/>.</param>
      /// <param name="dataReferences">The data references stored into <see cref="ColumnValueStorage{TValue}"/>.</param>
      /// <param name="md">The <see cref="CILMetaData"/> being processed.</param>
      /// <exception cref="ArgumentNullException">If any of <paramref name="mdStreamContainer"/>, <paramref name="dataReferences"/>, or <paramref name="md"/> is <c>null</c>.</exception>
      public RowReadingArguments(
         ReaderMetaDataStreamContainer mdStreamContainer,
         ColumnValueStorage<Int32> dataReferences,
         CILMetaData md
         )
      {
         ArgumentValidator.ValidateNotNull( "Meta data stream container", mdStreamContainer );
         ArgumentValidator.ValidateNotNull( "Data references", dataReferences );
         ArgumentValidator.ValidateNotNull( "Meta data", md );

         this.MDStreamContainer = mdStreamContainer;
         this.DataReferencesStorage = dataReferences;
         this.MetaData = md;
      }

      /// <summary>
      /// Gets the <see cref="ReaderMetaDataStreamContainer"/> containing all meta data streams except table stream.
      /// </summary>
      /// <value>The <see cref="ReaderMetaDataStreamContainer"/> containing all meta data streams except table stream.</value>
      public ReaderMetaDataStreamContainer MDStreamContainer { get; }

      /// <summary>
      /// Gets the data references stored in <see cref="ColumnValueStorage{TValue}"/>.
      /// </summary>
      /// <value>The data references stored in <see cref="ColumnValueStorage{TValue}"/>.</value>
      public ColumnValueStorage<Int32> DataReferencesStorage { get; }

      /// <summary>
      /// Gets the <see cref="CILMetaData"/> being processed.
      /// </summary>
      /// <value>The <see cref="CILMetaData"/> being processed.</value>
      public CILMetaData MetaData { get; }
   }

   /// <summary>
   /// This class contains required information to read one data reference value (e.g. <see cref="MethodDefinition.IL"/>) of a single table row.
   /// </summary>
   public class DataReferencesProcessingArgs
   {

      private readonly Lazy<DictionaryQuery<Int32, ClassLayout>> _layoutInfo;

      /// <summary>
      /// Creates new instance of <see cref="DataReferencesProcessingArgs"/> with given values.
      /// </summary>
      /// <param name="stream">The stream to read data from.</param>
      /// <param name="imageInformation">The <see cref="CAMPhysicalIO::CILAssemblyManipulator.Physical.IO.ImageInformation"/>.</param>
      /// <param name="rvaConverter">The <see cref="CAMPhysicalIO::CILAssemblyManipulator.Physical.IO.RVAConverter"/>.</param>
      /// <param name="mdStreamContainer">The <see cref="ReaderMetaDataStreamContainer"/>.</param>
      /// <param name="md">The <see cref="CILMetaData"/> being processed.</param>
      /// <param name="array">The auxiliary array to use (to avoid too much dynamic memory allocation).</param>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="stream"/>, <paramref name="imageInformation"/>, <paramref name="rvaConverter"/>, <paramref name="mdStreamContainer"/>, <paramref name="md"/>, or <paramref name="array"/> is <c>null</c>.</exception>
      public DataReferencesProcessingArgs(
         StreamHelper stream,
         ImageInformation imageInformation,
         RVAConverter rvaConverter,
         ReaderMetaDataStreamContainer mdStreamContainer,
         CILMetaData md,
         ResizableArray<Byte> array
         )
      {
         ArgumentValidator.ValidateNotNull( "Stream", stream );
         ArgumentValidator.ValidateNotNull( "Image information", imageInformation );
         ArgumentValidator.ValidateNotNull( "RVA converter", rvaConverter );
         ArgumentValidator.ValidateNotNull( "Meta data stream container", mdStreamContainer );
         ArgumentValidator.ValidateNotNull( "Meta data", md );
         ArgumentValidator.ValidateNotNull( "Array", array );

         this.Stream = stream;
         this.ImageInformation = imageInformation;
         this.RVAConverter = rvaConverter;
         this.MDStreamContainer = mdStreamContainer;
         this.MetaData = md;
         this.Array = array;
         this._layoutInfo = new Lazy<DictionaryQuery<Int32, ClassLayout>>(
            () => md.ClassLayouts.TableContents
            .ToDictionary_Overwrite( l => l.Parent.Index, l => l )
            .ToDictionaryProxy().CQ,
            System.Threading.LazyThreadSafetyMode.None );
      }

      /// <summary>
      /// Gets the stream containing data, as <see cref="StreamHelper"/>.
      /// </summary>
      /// <value>The stream containing data, as <see cref="StreamHelper"/>.</value>
      public StreamHelper Stream { get; }

      /// <summary>
      /// Gets the <see cref="CAMPhysicalIO::CILAssemblyManipulator.Physical.IO.ImageInformation"/> containing information about the image, including data references.
      /// </summary>
      /// <value>The <see cref="CAMPhysicalIO::CILAssemblyManipulator.Physical.IO.ImageInformation"/> containing information about the image, including data references.</value>
      public ImageInformation ImageInformation { get; }

      /// <summary>
      /// Gets the <see cref="CAMPhysicalIO::CILAssemblyManipulator.Physical.IO.RVAConverter"/> to be used when converting RVAs to absolute stream offsets.
      /// </summary>
      /// <value>The <see cref="CAMPhysicalIO::CILAssemblyManipulator.Physical.IO.RVAConverter"/> to be used when converting RVAs to absolute stream offsets.</value>
      public RVAConverter RVAConverter { get; }

      /// <summary>
      /// Gets the <see cref="ReaderMetaDataStreamContainer"/> containing all meta data streams except table stream.
      /// </summary>
      /// <value>The <see cref="ReaderMetaDataStreamContainer"/> containing all meta data streams except table stream.</value>
      public ReaderMetaDataStreamContainer MDStreamContainer { get; }

      /// <summary>
      /// Gets the <see cref="CILMetaData"/> being processed.
      /// </summary>
      /// <value>The <see cref="CILMetaData"/> being processed.</value>
      public CILMetaData MetaData { get; }

      /// <summary>
      /// Gets the auxiliary byte array.
      /// </summary>
      /// <value>The auxiliary byte array.</value>
      public ResizableArray<Byte> Array { get; }

      /// <summary>
      /// Gets the information about class layouts.
      /// </summary>
      /// <value>The information about class layouts.</value>
      /// <remarks>
      /// <para>
      /// This value is lazily constructed from <see cref="MetaData"/>.
      /// </para>
      /// <para>
      /// This information is used by <see cref="E_CILPhysical.TryCalculateFieldTypeSize(CILMetaData, DictionaryQuery{int, ClassLayout}, int, out int)"/> method.
      /// </para>
      /// </remarks>
      /// <seealso cref="E_CILPhysical.TryCalculateFieldTypeSize(CILMetaData, DictionaryQuery{int, ClassLayout}, int, out int)"/>
      public DictionaryQuery<Int32, ClassLayout> LayoutInfo
      {
         get
         {
            return this._layoutInfo.Value;
         }
      }

   }

   /// <summary>
   /// This class contains information required to get a reference to meta data stream for e.g. string or blob value of a single row.
   /// </summary>
   public class RowHeapFillingArguments
   {
      /// <summary>
      /// Creates a new instance of <see cref="RowHeapFillingArguments"/> with given meta data stream container, auxiliary array, the public key of the assembly being serialized, and the meta data being serialized.
      /// </summary>
      /// <param name="mdStreamContainer">The <see cref="WriterMetaDataStreamContainer"/>.</param>
      /// <param name="array">The auxiliary byte array, to help avoid extra memory allocation.</param>
      /// <param name="publicKey">The public key of the assembly being serialized, either from <see cref="AssemblyDefinition"/> or <see cref="StrongNameKeyPair"/>.</param>
      /// <param name="metaData">The <see cref="CILMetaData"/> being serialized.</param>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="mdStreamContainer"/>, <paramref name="array"/>, or <paramref name="metaData"/> is <c>null</c>.</exception>
      public RowHeapFillingArguments(
         WriterMetaDataStreamContainer mdStreamContainer,
         ResizableArray<Byte> array,
         ArrayQuery<Byte> publicKey,
         CILMetaData metaData
         )
      {
         ArgumentValidator.ValidateNotNull( "Meta data stream container", mdStreamContainer );
         ArgumentValidator.ValidateNotNull( "Byte array", array );
         ArgumentValidator.ValidateNotNull( "Meta data", metaData );

         this.MDStreamContainer = mdStreamContainer;
         this.Array = array;
         this.PublicKey = publicKey;
         this.MetaData = metaData;
         this.AuxArray = new ResizableArray<Byte>();
      }

      /// <summary>
      /// Gets the auxiliary byte array.
      /// </summary>
      /// <value>The auxiliary byte array.</value>
      public ResizableArray<Byte> Array { get; }

      /// <summary>
      /// Gets the <see cref="WriterMetaDataStreamContainer"/> containing all meta data streams except table stream.
      /// </summary>
      /// <value>The <see cref="WriterMetaDataStreamContainer"/> containing all meta data streams except table stream.</value>
      public WriterMetaDataStreamContainer MDStreamContainer { get; }

      /// <summary>
      /// Gets the public key of the assembly being serialized.
      /// May be <c>null</c>.
      /// </summary>
      /// <value>The public key of the assembly being serialized.</value>
      public ArrayQuery<Byte> PublicKey { get; }

      /// <summary>
      /// Gets the <see cref="CILMetaData"/> being serialized.
      /// </summary>
      /// <value>The <see cref="CILMetaData"/> being serialized.</value>
      public CILMetaData MetaData { get; }

      /// <summary>
      /// This is additional auxiliary byte array.
      /// </summary>
      /// <value>The additional auxiliary byte array.</value>
      public ResizableArray<Byte> AuxArray { get; }
   }

   /// <summary>
   /// This interface contains functionality directly interfacing with binary data, for one column of one table.
   /// </summary>
   /// <seealso cref="ColumnSerializationSupport_Constant8"/>
   /// <seealso cref="ColumnSerializationSupport_Constant16"/>
   /// <seealso cref="ColumnSerializationSupport_Constant32"/>
   public interface ColumnSerializationBinaryFunctionality
   {
      /// <summary>
      /// Gets the amount of bytes that one value of this column will use when serialized to binary data.
      /// </summary>
      /// <value>The amount of bytes that one value of this column will use when serialized to binary data.</value>
      Int32 ColumnByteCount { get; }

      /// <summary>
      /// Reads the column value, as integer, from given byte array.
      /// </summary>
      /// <param name="array">The byte array.</param>
      /// <param name="idx">The index to start reading in <paramref name="array"/>.</param>
      /// <returns>A column value as integer.</returns>
      Int32 ReadRawValue( Byte[] array, Int32 idx );

      /// <summary>
      /// Writes the column value, s integer, to given byte array.
      /// </summary>
      /// <param name="array">The byte array.</param>
      /// <param name="idx">The index to start writing in <paramref name="array"/>.</param>
      /// <param name="value">The value to write.</param>
      void WriteValue( Byte[] array, Int32 idx, Int32 value );
   }

   internal static class MetaDataConstants
   {

      internal static readonly Encoding SYS_STRING_ENCODING = new UTF8Encoding( false, false );
      internal static readonly Encoding USER_STRING_ENCODING = new UnicodeEncoding( false, false, false );

      internal const String TABLE_STREAM_NAME = "#~";
      internal const String SYS_STRING_STREAM_NAME = "#Strings";
      internal const String USER_STRING_STREAM_NAME = "#US";
      internal const String GUID_STREAM_NAME = "#GUID";
      internal const String BLOB_STREAM_NAME = "#Blob";

      internal const Int32 GUID_SIZE = 16;
   }
}

#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{


   /// <summary>
   /// Given a enumerable of <see cref="MetaDataTableInformation"/>s, creates a enumerable of <see cref="TableSerializationLogicalFunctionality"/> objects constructed with information in this <see cref="TableSerializationLogicalFunctionalityCreationArgs"/>.
   /// </summary>
   /// <param name="serializationCreationArgs">The <see cref="TableSerializationLogicalFunctionalityCreationArgs"/>.</param>
   /// <param name="tableInfos">The enumerable of <see cref="MetaDataTableInformation"/>s.</param>
   /// <returns>An enumerable of created <see cref="TableSerializationLogicalFunctionalityCreationInfo"/> objects.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="tableInfos"/> is <c>null</c>.</exception>
   /// <seealso cref="MetaDataTableInformationWithSerializationCapabilityDelegate"/>
   public static IEnumerable<TableSerializationLogicalFunctionalityCreationInfo> CreateTableSerializationInfos(
      this TableSerializationLogicalFunctionalityCreationArgs serializationCreationArgs,
      IEnumerable<MetaDataTableInformation> tableInfos
      )
   {
      ArgumentValidator.ValidateNotNull( "Table infos", tableInfos );

      var tableInfoDic = tableInfos
         .Where( ti => ti != null )
         .ToDictionary_Overwrite(
            info => (Int32) info.TableIndex,
            info => info.GetFunctionality<MetaDataTableInformationWithSerializationCapabilityDelegate>()?.Invoke( serializationCreationArgs )
         );
      var curMax = 0;
      foreach ( var kvp in tableInfoDic.OrderBy( kvp => kvp.Key ) )
      {
         var cur = kvp.Key;
         while ( curMax < cur )
         {
            yield return null;
            ++curMax;
         }
         yield return kvp.Value;
         ++curMax;
      }
   }

   /// <summary>
   /// This is extension method to create a new instance of <see cref="DataReferencesInfo"/> from this <see cref="ColumnValueStorage{TValue}"/> with given conversion callback.
   /// </summary>
   /// <param name="valueStorage">The <see cref="ColumnValueStorage{TValue}"/>.</param>
   /// <param name="converter">The callback to convert values into <see cref="Int64"/>.</param>
   /// <returns>A new instance of <see cref="DataReferencesInfo"/> with information extracted from given <see cref="ColumnValueStorage{TValue}"/>.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="ColumnValueStorage{TValue}"/> is <c>null</c>.</exception>
   public static DataReferencesInfo CreateDataReferencesInfo<T>( this ColumnValueStorage<T> valueStorage, Func<T, Int64> converter )
   {

      var cf = UtilPack.CollectionsWithRoles.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY;
      var dic = new Dictionary<
         Tables,
         ArrayQuery<ArrayQuery<Int64>>
         >();
      foreach ( var tbl in valueStorage.GetPresentTables() )
      {
         var dataRefsColCount = valueStorage.GetStoredColumnsCount( tbl );
         var arr = new ArrayQuery<Int64>[dataRefsColCount];
         var tSize = valueStorage.TableSizes[(Int32) tbl];
         for ( var i = 0; i < dataRefsColCount; ++i )
         {
            // TODO: to CommonUtils: public static U[] FillFromEnumerable<T, U>(this IEnumerable<T>, Func<T, U> converter, Int32 size, SizeMismatchStrategy = Ignore | ThrowIfArraySmaller | ThrowIfArrayGreater )
            var values = new Int64[tSize];
            var idx = 0;
            foreach ( var val in valueStorage.GetAllRawValuesForColumn( tbl, i ) )
            {
               values[idx++] = converter( val );
            }
            arr[i] = cf.NewArrayProxy( values ).CQ;
         }
         dic.Add( tbl, cf.NewArrayProxy( arr ).CQ );
      }

      return new DataReferencesInfo( cf.NewDictionaryProxy( dic ).CQ );
   }

   /// <summary>
   /// Helper extension method to check whether meta data stream with given name is considered to be wide, i.e. the references into that stream require 4-byte integer instead of 2-byte integer.
   /// </summary>
   /// <param name="args">The <see cref="TableSerializationBinaryFunctionalityCreationArgs"/>.</param>
   /// <param name="streamName">The name of the meta data stream.</param>
   /// <returns><c>true</c> if the references into meta data stream with given name require 4-byte integer; <c>false</c> otherwise.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="args"/> is <c>null</c>.</exception>
   public static Boolean IsWide( this TableSerializationBinaryFunctionalityCreationArgs args, String streamName )
   {
      Boolean success;
      return streamName == null ?
         false :
         args.StreamSizes.TryGetValue( streamName, out success ).IsWideMDStreamSize();
   }
}