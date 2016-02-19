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
using CollectionsWithRoles.API;
using CommonUtils;
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
   public sealed class RawInterfaceImplementation
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="ParameterDefinition.Class"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ParameterDefinition.Class"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Class { get; set; }

      /// <summary>
      /// Gets or sets the raw version of <see cref="ParameterDefinition.Interface"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ParameterDefinition.Interface"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Interface { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="MemberReference"/>.
   /// </summary>
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
      /// Gets or sets the raw version of <see cref="ConstantDefinition.Padding"/>.
      /// </summary>
      /// <value>The raw version of <see cref="ConstantDefinition.Padding"/>.</value>
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
   public sealed class RawFieldRVA
   {
      /// <summary>
      /// Gets or sets the raw version of <see cref="FieldRVA.RVA"/>.
      /// </summary>
      /// <value>The raw version of <see cref="FieldRVA.RVA"/>.</value>
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
      /// Gets or sets the raw version of <see cref="GenericParameterDefinitionGenericParameterDefinitionName"/>.
      /// </summary>
      /// <value>The raw version of <see cref="XXX.Name"/>.</value>
      /// <remarks>
      /// Modifying this value has no effect on the actual rows read by <see cref="ReaderTableStreamHandler"/>.
      /// </remarks>
      public Int32 Name { get; set; }
   }

   /// <summary>
   /// This is raw row type for <see cref="MethodSpecification"/>.
   /// </summary>
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
   /// The <see cref="TableSerializationInfo"/> is agnostic to the byte size of a single row, that is what <see cref="TableSerializationFunctionality"/> is for, obtaineable by <see cref="CreateSupport"/> method.
   /// </summary>
   public interface TableSerializationInfo
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

      TableSerializationFunctionality CreateSupport(
         DefaultColumnSerializationSupportCreationArgs args
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
      /// <param name="args">The <see cref="RawValueProcessingArgs"/>.</param>
      /// <remarks>
      /// This method is used when deserializing (reading) a module.
      /// </remarks>
      void PopulateDataReferences(
         RawValueProcessingArgs args
         );

      /// <summary>
      /// This method creates a <see cref="SectionPartWithDataReferenceTargets"/> object for each of the data reference columns.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/>.</param>
      /// <param name="mdStreamContainer">The <see cref="WriterMetaDataStreamContainer"/>.</param>
      /// <returns>An enumeraboe of <see cref="SectionPartWithDataReferenceTargets"/>, one for each data reference column.</returns>
      /// <remarks>
      /// This method is used when serializing (writing) a module.
      /// </remarks>
      IEnumerable<SectionPartWithDataReferenceTargets> CreateDataReferenceSectionParts(
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

   public class DefaultColumnSerializationSupportCreationArgs
   {

      public DefaultColumnSerializationSupportCreationArgs(
         ArrayQuery<Int32> tableSizes,
         DictionaryQuery<String, Int32> streamSizes
         )
      {
         ArgumentValidator.ValidateNotNull( "Table sizes", tableSizes );

         this.TableSizes = tableSizes;
         this.StreamSizes = streamSizes;
      }

      public ArrayQuery<Int32> TableSizes { get; }

      public DictionaryQuery<String, Int32> StreamSizes { get; }

      public Boolean IsWide( String heapName )
      {
         Int32 streamSize;
         return heapName != null
            && this.StreamSizes.TryGetValue( heapName, out streamSize )
            && streamSize.IsWideMDStreamSize();
      }

   }

   public interface TableSerializationFunctionality
   {
      TableSerializationInfo TableSerializationInfo { get; }

      Object ReadRawRow( Byte[] array, Int32 idx );

      void ReadRows( MetaDataTable table, Int32 tableRowCount, RowReadingArguments args );

      ArrayQuery<ColumnSerializationFunctionality> ColumnSerializationSupports { get; }
   }

   public class RowReadingArguments
   {
      public RowReadingArguments(
         Byte[] array,
         Int32 index,
         ReaderMetaDataStreamContainer mdStreamContainer,
         ColumnValueStorage<Int32> rawValueStorage,
         SignatureProvider sigProvider
         )
      {
         ArgumentValidator.ValidateNotNull( "Array", array );
         ArgumentValidator.ValidateNotNull( "Meta data stream container", mdStreamContainer );
         ArgumentValidator.ValidateNotNull( "Raw value storage", rawValueStorage );
         ArgumentValidator.ValidateNotNull( "Signature provider", sigProvider );

         this.Array = array;
         this.Index = index;
         this.MDStreamContainer = mdStreamContainer;
         this.RawValueStorage = rawValueStorage;
         this.SignatureProvider = sigProvider;
      }

      public Byte[] Array { get; }

      public Int32 Index { get; }

      public ReaderMetaDataStreamContainer MDStreamContainer { get; }

      public ColumnValueStorage<Int32> RawValueStorage { get; }

      public SignatureProvider SignatureProvider { get; }
   }

   public class RawValueProcessingArgs
   {

      private readonly Lazy<DictionaryQuery<Int32, ClassLayout>> _layoutInfo;
      public RawValueProcessingArgs(
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

      public StreamHelper Stream { get; }
      public ImageInformation ImageInformation { get; }
      public RVAConverter RVAConverter { get; }

      public ReaderMetaDataStreamContainer MDStreamContainer { get; }

      public CILMetaData MetaData { get; }

      public ResizableArray<Byte> Array { get; }

      public DictionaryQuery<Int32, ClassLayout> LayoutInfo
      {
         get
         {
            return this._layoutInfo.Value;
         }
      }

   }

   public class RowHeapFillingArguments
   {
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

      public ResizableArray<Byte> Array { get; }

      public WriterMetaDataStreamContainer MDStreamContainer { get; }

      public ArrayQuery<Byte> PublicKey { get; }

      public CILMetaData MetaData { get; }

      public ResizableArray<Byte> AuxArray { get; }
   }

   public interface ColumnSerializationFunctionality
   {

      Int32 ColumnByteCount { get; }

      Int32 ReadRawValue( Byte[] array, ref Int32 idx );

      void WriteValue( Byte[] bytes, Int32 idx, Int32 value );
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

public static partial class E_CILPhysical
{
   public static ArrayQuery<Int32> CreateTableSizeArray( this IEnumerable<TableSerializationInfo> infos, CILMetaData md )
   {
      return infos.Select( info =>
      {
         MetaDataTable tbl;
         return info != null && md.TryGetByTable( (Int32) info.Table, out tbl ) ?
            tbl.GetRowCount() :
            0;
      } ).ToArrayProxy().CQ;
   }

   public static IEnumerable<TableSerializationInfo> CreateTableSerializationInfos( this TableSerializationInfoCreationArgs serializationCreationArgs, CILMetaData md )
   {
      return serializationCreationArgs.CreateTableSerializationInfos( md.GetAllTables().Select( t => t.TableInformationNotGeneric ) );
   }

   public static IEnumerable<TableSerializationInfo> CreateTableSerializationInfos( this TableSerializationInfoCreationArgs serializationCreationArgs, MetaDataTableInformationProvider tableInfoProvider )
   {
      return serializationCreationArgs.CreateTableSerializationInfos( tableInfoProvider.GetAllSupportedTableInformations() );
   }

   private static IEnumerable<TableSerializationInfo> CreateTableSerializationInfos(
      this TableSerializationInfoCreationArgs serializationCreationArgs,
      IEnumerable<MetaDataTableInformation> tableInfos
      )
   {
      var tableInfoDic = tableInfos
         .Where( ti => ti != null )
         .ToDictionary_Overwrite(
            info => (Int32) info.TableIndex,
            info => ( info as MetaDataTableInformationWithSerializationCapability )?.CreateTableSerializationInfoNotGeneric( serializationCreationArgs )
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
   /// This is extension method to create a new instance of <see cref="DataReferencesInfo"/> from this <see cref="MetaDataTableStreamHeader"/> with given enumerable of <see cref="DataReferenceInfo"/>s.
   /// </summary>
   /// <param name="valueStorage">The <see cref="MetaDataTableStreamHeader"/>.</param>
   /// <returns>A new instance of <see cref="DataReferencesInfo"/> with information extracted from given enumerable of <see cref="DataReferencesInfo"/>s.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="MetaDataTableStreamHeader"/> is <c>null</c>.</exception>
   public static DataReferencesInfo CreateDataReferencesInfo<T>( this ColumnValueStorage<T> valueStorage, Func<T, Int64> converter )
   {

      var cf = CollectionsWithRoles.Implementation.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY;
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
            // TODO: to CommonUtils: public static T[] FillFromEnumerable<T>(this IEnumerable<T>, Int32 size, SizeMismatchStrategy = Ignore | ThrowIfArraySmaller | ThrowIfArrayGreater )
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
}