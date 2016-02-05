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
using CILAssemblyManipulator.Physical;
using CommonUtils;
using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;

namespace CILAssemblyManipulator.Physical
{
   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.ModuleDefinitions"/> table at table index <see cref="Tables.Module"/>.
   /// </summary>
   public sealed class ModuleDefinition
   {
      /// <summary>
      /// Gets or sets the generation of this module.
      /// Used in Edit-And-Continue context mostly.
      /// </summary>
      /// <value>The generation of this module.</value>
      public Int16 Generation { get; set; }

      /// <summary>
      /// Gets or sets the name of this module.
      /// Usually ends with <c>.dll</c> or <c>.exe</c>.
      /// </summary>
      /// <value>The name of this module.</value>
      public String Name { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="Guid"/> of this module.
      /// </summary>
      /// <value>The <see cref="Guid"/> of this module.</value>
      public Guid? ModuleGUID { get; set; }

      /// <summary>
      /// Gets or sets the Edit-And-Continue <see cref="Guid"/> of this module.
      /// </summary>
      /// <value>The Edit-And-Continue <see cref="Guid"/> of this module.</value>
      public Guid? EditAndContinueGUID { get; set; }

      /// <summary>
      /// Gets or sets the Edit-And-Continue <see cref="Guid"/> of base module.
      /// </summary>
      /// <value>The Edit-And-Continue <see cref="Guid"/> of base module.</value>
      public Guid? EditAndContinueBaseGUID { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.TypeReferences"/> table at table index <see cref="Tables.TypeRef"/>.
   /// </summary>
   public sealed class TypeReference
   {
      /// <summary>
      /// Gets or sets the resolution scope for this <see cref="TypeReference"/>.
      /// </summary>
      /// <value>The resolution scope for this <see cref="TypeReference"/>.</value>
      /// <remarks>
      /// <para>
      /// According to ECMA-335 spec, the resolution scope should be exactly one of:
      /// <list type="bullet">
      /// <item>
      /// <description><c>null</c>, and there should be row in <see cref="CILMetaData.ExportedTypes"/> table for this row, if this represents a exported type,</description>
      /// </item>
      /// <item>
      /// <description>a <see cref="TableIndex"/> with <see cref="Tables.TypeRef"/> as its <see cref="TableIndex.Table"/> property, if this represents a nested type,</description>
      /// </item>
      /// <item>
      /// <description>a <see cref="TableIndex"/> with <see cref="Tables.ModuleRef"/> as its <see cref="TableIndex.Table"/> property, if this represents type defined in another module but within the same assembly,</description>
      /// </item>
      /// <item>
      /// <description>a <see cref="TableIndex"/> with <see cref="Tables.Module"/> as its <see cref="TableIndex.Table"/> property, if this represents type defined in this module,</description>
      /// </item>
      /// <item>
      /// <description>or a <see cref="TableIndex"/> with <see cref="Tables.AssemblyRef"/> as its <see cref="TableIndex.Table"/> property, if this represents type defined in another assembly.</description>
      /// </item>
      /// </list>
      /// </para>
      /// <para>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.ResolutionScope"/> schema.
      /// </para>
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex? ResolutionScope { get; set; }

      /// <summary>
      /// Gets or sets the name for this <see cref="TypeReference"/>.
      /// </summary>
      /// <value>The name for this <see cref="TypeReference"/>.</value>
      public String Name { get; set; }

      /// <summary>
      /// Gets or sets the namespace for this <see cref="TypeReference"/>.
      /// </summary>
      /// <value>The namespace for this <see cref="TypeReference"/>.</value>
      public String Namespace { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.TypeDefinitions"/> table at table index <see cref="Tables.TypeDef"/>.
   /// </summary>
   public sealed class TypeDefinition
   {
      /// <summary>
      /// Creates a new instance of <see cref="TypeDefinition"/> with <see cref="FieldList"/> and <see cref="MethodList"/> pointing to zeroth element of <see cref="Tables.Field"/> and <see cref="Tables.MethodDef"/>, respectively.
      /// </summary>
      public TypeDefinition()
         : this( 0, 0 )
      {
         // This exists instead of default parameters so that new() -constraint would be possible for rows (if ever needed)
      }

      /// <summary>
      /// Creates a new instance of <see cref="TypeDefinition"/> with <see cref="FieldList"/> and <see cref="MethodList"/> pointing to given elements of <see cref="Tables.Field"/> and <see cref="Tables.MethodDef"/>, respectively.
      /// </summary>
      /// <param name="fieldIndex">The zero-based index for <see cref="FieldList"/>.</param>
      /// <param name="methodIndex">The zero-based index for <see cref="MethodList"/>.</param>
      public TypeDefinition( Int32 fieldIndex, Int32 methodIndex )
      {
         this.FieldList = new TableIndex( Tables.Field, fieldIndex );
         this.MethodList = new TableIndex( Tables.MethodDef, methodIndex );
      }

      /// <summary>
      /// Gets or sets the <see cref="TypeAttributes"/> for this <see cref="TypeDefinition"/>.
      /// </summary>
      /// <value>The <see cref="TypeAttributes"/> for this <see cref="TypeDefinition"/>.</value>
      /// <seealso cref="TypeAttributes"/>
      public TypeAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the name of this <see cref="TypeDefinition"/>.
      /// </summary>
      /// <value>The name of this <see cref="TypeDefinition"/>.</value>
      public String Name { get; set; }

      /// <summary>
      /// Gets or sets the namespace of this <see cref="TypeDefinition"/>.
      /// </summary>
      /// <value>The namespace of this <see cref="TypeDefinition"/>.</value>
      public String Namespace { get; set; }

      /// <summary>
      /// Gets or sets the optional reference to base type of this <see cref="TypeDefinition"/>.
      /// </summary>
      /// <value>The optional reference to base type of this <see cref="TypeDefinition"/>.</value>
      /// <remarks>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.TypeDefOrRef"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex? BaseType { get; set; }

      /// <summary>
      /// Gets or sets the index to the first element of the contiguous run of <see cref="FieldDefinition"/>s owned by this <see cref="TypeDefinition"/>.
      /// </summary>
      /// <value>The index to the contiguous run of <see cref="FieldDefinition"/>s owned by this <see cref="TypeDefinition"/>.</value>
      /// <remarks>
      /// <para>
      /// The contiguous run of <see cref="FieldDefinition"/>s ends at the smaller of the last row in <see cref="CILMetaData.FieldDefinitions"/>, or the run of <see cref="FieldDefinition"/>s started by the next <see cref="TypeDefinition"/>.
      /// </para>
      /// <para>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.Field"/>.
      /// </para>
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex FieldList { get; set; }

      /// <summary>
      /// Gets or sets the index to the first element of the contiguous run of <see cref="MethodDefinition"/>s owned by this <see cref="TypeDefinition"/>.
      /// </summary>
      /// <value>The index to the contiguous run of <see cref="MethodDefinition"/>s owned by this <see cref="TypeDefinition"/>.</value>
      /// <remarks>
      /// <para>
      /// The contiguous run of <see cref="MethodDefinition"/>s ends at the smaller of the last row in <see cref="CILMetaData.MethodDefinitions"/>, or the run of <see cref="MethodDefinition"/>s started by the next <see cref="TypeDefinition"/>.
      /// </para>
      /// <para>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.MethodDef"/>.
      /// </para>
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex MethodList { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.FieldDefinitions"/> table at table index <see cref="Tables.Field"/>.
   /// </summary>
   public sealed class FieldDefinition
   {
      /// <summary>
      /// Gets or sets the <see cref="FieldAttributes"/> of this <see cref="FieldDefinition"/>.
      /// </summary>
      /// <value>The <see cref="FieldAttributes"/> of this <see cref="FieldDefinition"/>.</value>
      /// <seealso cref="FieldAttributes"/>
      public FieldAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the name of this <see cref="FieldDefinition"/>.
      /// </summary>
      /// <value>The name of this <see cref="FieldDefinition"/>.</value>
      public String Name { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="FieldSignature"/> of this <see cref="FieldDefinition"/>.
      /// </summary>
      /// <value>The <see cref="FieldSignature"/> of this <see cref="FieldDefinition"/>.</value>
      /// <seealso cref="FieldSignature"/>
      public FieldSignature Signature { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.MethodDefinitions"/> table at table index <see cref="Tables.MethodDef"/>.
   /// </summary>
   public sealed class MethodDefinition
   {
      /// <summary>
      /// Creates a new instance of <see cref="MethodDefinition"/> with <see cref="ParameterList"/> pointing to zeroth element of <see cref="Tables.Parameter"/>.
      /// </summary>
      public MethodDefinition()
         : this( 0 )
      {
         // This exists instead of default parameters so that new() -constraint would be possible for rows (if ever needed)
      }

      /// <summary>
      /// Creates a new instance of <see cref="MethodDefinition"/> with <see cref="ParameterList"/> pointing to given element of <see cref="Tables.Parameter"/>.
      /// </summary>
      /// <param name="parameterIndex">The zero-based index for <see cref="ParameterList"/>.</param>
      public MethodDefinition( Int32 parameterIndex )
      {
         this.ParameterList = new TableIndex( Tables.Parameter, parameterIndex );
      }

      /// <summary>
      /// Gets or sets the <see cref="MethodILDefinition"/> for this <see cref="MethodDefinition"/>.
      /// </summary>
      /// <value>The <see cref="MethodILDefinition"/> for this <see cref="MethodDefinition"/>.</value>
      /// <seealso cref="MethodILDefinition"/>
      public MethodILDefinition IL { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="MethodImplAttributes"/> for this <see cref="MethodDefinition"/>.
      /// </summary>
      /// <value>The <see cref="MethodImplAttributes"/> for this <see cref="MethodDefinition"/>.</value>
      /// <seealso cref="MethodImplAttributes"/>
      public MethodImplAttributes ImplementationAttributes { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="MethodAttributes"/> for this <see cref="MethodDefinition"/>.
      /// </summary>
      /// <value>The <see cref="MethodAttributes"/> for this <see cref="MethodDefinition"/>.</value>
      /// <seealso cref="MethodAttributes"/>
      public MethodAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the name of this <see cref="MethodDefinition"/>.
      /// </summary>
      /// <value>The name of this <see cref="MethodDefinition"/>.</value>
      public String Name { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="MethodDefinitionSignature"/> of this <see cref="MethodDefinition"/>.
      /// </summary>
      /// <value>The <see cref="MethodDefinitionSignature"/> of this <see cref="MethodDefinition"/>.</value>
      /// <seealso cref="MethodDefinitionSignature"/>
      public MethodDefinitionSignature Signature { get; set; }

      /// <summary>
      /// Gets or sets the index to the first element of the contiguous run of <see cref="ParameterDefinition"/>s owned by this <see cref="MethodDefinition"/>.
      /// </summary>
      /// <value>The index to the contiguous run of <see cref="ParameterDefinition"/>s owned by this <see cref="MethodDefinition"/>.</value>
      /// <remarks>
      /// <para>
      /// The contiguous run of <see cref="ParameterDefinition"/>s ends at the smaller of the last row in <see cref="CILMetaData.ParameterDefinitions"/>, or the run of <see cref="ParameterDefinition"/>s started by the next <see cref="MethodDefinition"/>.
      /// </para>
      /// <para>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.Parameter"/>.
      /// </para>
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex ParameterList { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.ParameterDefinitions"/> table at table index <see cref="Tables.Parameter"/>.
   /// </summary>
   public sealed class ParameterDefinition
   {
      /// <summary>
      /// Gets or sets the <see cref="ParameterAttributes"/> for this <see cref="ParameterDefinition"/>.
      /// </summary>
      /// <value>The <see cref="ParameterAttributes"/> for this <see cref="ParameterDefinition"/>.</value>
      public ParameterAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the index of this <see cref="ParameterDefinition"/> within the parameters of the owning <see cref="MethodDefinition"/>.
      /// </summary>
      /// <value>The index of this <see cref="ParameterDefinition"/> within the parameters of the owning <see cref="MethodDefinition"/>.</value>
      /// <remarks>
      /// According to ECMA-335 spec, this property should have value of <c>0</c> for method's return type, and values of <c>&gt;= 1</c> for method's actual parameters.
      /// </remarks>
      public Int32 Sequence { get; set; }

      /// <summary>
      /// Gets or sets the name for this <see cref="ParameterDefinition"/>.
      /// </summary>
      /// <value>The name for this <see cref="ParameterDefinition"/>.</value>
      public String Name { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.InterfaceImplementations"/> table at table index <see cref="Tables.InterfaceImpl"/>.
   /// </summary>
   public sealed class InterfaceImplementation
   {
      /// <summary>
      /// Creates a new instance of <see cref="InterfaceImplementation"/> with <see cref="Class"/> pointing to zeroth element of <see cref="Tables.TypeDef"/>.
      /// </summary>
      public InterfaceImplementation()
         : this( 0 )
      {
         // This exists instead of default parameters so that new() -constraint would be possible for rows (if ever needed) 
      }

      /// <summary>
      /// Creates a new instance of <see cref="InterfaceImplementation"/> with <see cref="Class"/> pointing to given element of <see cref="Tables.TypeDef"/>.
      /// </summary>
      /// <param name="typeDefIndex">The zero-based index for <see cref="Class"/>.</param>
      public InterfaceImplementation( Int32 typeDefIndex )
      {
         this.Class = new TableIndex( Tables.TypeDef, typeDefIndex );
      }

      /// <summary>
      /// Gets or sets the type that implements or extends an interface.
      /// </summary>
      /// <value>The type that implements or extends an interface.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.TypeDef"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Class { get; set; }

      /// <summary>
      /// Gets or sets the interface that <see cref="Class"/> implements or extends.
      /// </summary>
      /// <value>The interface that <see cref="Class"/> implements or extends.</value>
      /// <remarks>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.TypeDefOrRef"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Interface { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.MemberReferences"/> table at table index <see cref="Tables.MemberRef"/>.
   /// </summary>
   public sealed class MemberReference
   {
      /// <summary>
      /// Gets or sets the declaring type for this <see cref="MemberReference"/>.
      /// </summary>
      /// <value>The declaring type for this <see cref="MemberReference"/>.</value>
      /// <remarks>
      /// <para>
      /// According to ECMA-335 spec, the resolution scope should be exactly one of:
      /// <list type="bullet">
      /// <item>
      /// <description>a <see cref="TableIndex"/> with <see cref="Tables.TypeDef"/> as its <see cref="TableIndex.Table"/> property, if this is a member of a type defined in this module,</description>
      /// </item>
      /// <item>
      /// <description>a <see cref="TableIndex"/> with <see cref="Tables.TypeRef"/> as its <see cref="TableIndex.Table"/> property, if this is a member of a type defined in another module,</description>
      /// </item>
      /// <item>
      /// <description>a <see cref="TableIndex"/> with <see cref="Tables.ModuleRef"/> as its <see cref="TableIndex.Table"/> property, if this is a member of a type defined in another module within same assembly, as a global function or variable,</description>
      /// </item>
      /// <item>
      /// <description>a <see cref="TableIndex"/> with <see cref="Tables.MethodDef"/> as its <see cref="TableIndex.Table"/> property, if this is a call-site signature for a vararg method,</description>
      /// </item>
      /// <item>
      /// <description>or a <see cref="TableIndex"/> with <see cref="Tables.TypeSpec"/> as its <see cref="TableIndex.Table"/> property, if this is a member of a generic type.</description>
      /// </item>
      /// </list>
      /// </para>
      /// <para>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.MemberRefParent"/> schema.
      /// </para>
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex DeclaringType { get; set; }

      /// <summary>
      /// Gets or sets the name for this <see cref="MemberReference"/>.
      /// </summary>
      /// <value>The name for this <see cref="MemberReference"/>.</value>
      public String Name { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="AbstractSignature"/> for this <see cref="MemberReference"/>.
      /// </summary>
      /// <value>The <see cref="AbstractSignature"/> for this <see cref="MemberReference"/>.</value>
      /// <remarks>
      /// This should be either <see cref="MethodReferenceSignature"/> or <see cref="FieldSignature"/>.
      /// </remarks>
      /// <seealso cref="AbstractSignature"/>
      /// <seealso cref="MethodReferenceSignature"/>
      /// <seealso cref="FieldSignature"/>
      public AbstractSignature Signature { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.ConstantDefinitions"/> table at table index <see cref="Tables.Constant"/>.
   /// </summary>
   public sealed class ConstantDefinition
   {
      /// <summary>
      /// Gets or sets the <see cref="ConstantValueType"/> for this <see cref="ConstantDefinition"/>.
      /// </summary>
      /// <value>The <see cref="ConstantValueType"/> for this <see cref="ConstantDefinition"/>.</value>
      /// <seealso cref="ConstantValueType"/>
      public ConstantValueType Type { get; set; }

      /// <summary>
      /// Gets or sets the owner for this <see cref="ConstantDefinition"/>.
      /// </summary>
      /// <value>The owner for this <see cref="ConstantDefinition"/>.</value>
      /// <remarks>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.HasConstant"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Parent { get; set; }

      /// <summary>
      /// Gets or sets the value for this <see cref="ConstantDefinition"/>.
      /// May be <c>null</c>.
      /// </summary>
      /// <value>The value for this <see cref="ConstantDefinition"/>.</value>
      /// <remarks>
      /// If this property is <c>null</c>, then <see cref="Type"/> property should be <see cref="ConstantValueType.Null"/>.
      /// </remarks>
      public Object Value { get; set; }
   }

   /// <summary>
   /// This enumeration represents the kind of <see cref="ConstantDefinition.Value"/>.
   /// </summary>
   /// <remarks>
   /// The values of this enumeration are safe to be casted to <see cref="T:CILAssemblyManipulator.Physical.IO.SignatureElementTypes"/>.
   /// </remarks>
   public enum ConstantValueType : byte
   {
      /// <summary>
      /// The value is of type <see cref="Boolean"/>.
      /// </summary>
      Boolean = 0x02, // Same as SignatureElementTypes.Boolean
      /// <summary>
      /// The value is of type <see cref="Char"/>.
      /// </summary>
      Char,
      /// <summary>
      /// The value is of type <see cref="SByte"/>.
      /// </summary>
      I1,
      /// <summary>
      /// The value is of type <see cref="Byte"/>.
      /// </summary>
      U1,
      /// <summary>
      /// The value is of type <see cref="Int16"/>.
      /// </summary>
      I2,
      /// <summary>
      /// The value is of type <see cref="UInt16"/>.
      /// </summary>
      U2,
      /// <summary>
      /// The value is of type <see cref="Int32"/>.
      /// </summary>
      I4,
      /// <summary>
      /// The value is of type <see cref="UInt32"/>.
      /// </summary>
      U4,
      /// <summary>
      /// The value is of type <see cref="Int64"/>.
      /// </summary>
      I8,
      /// <summary>
      /// The value is of type <see cref="UInt64"/>.
      /// </summary>
      U8,
      /// <summary>
      /// The value is of type <see cref="Single"/>.
      /// </summary>
      R4,
      /// <summary>
      /// The value is of type <see cref="Double"/>.
      /// </summary>
      R8,
      /// <summary>
      /// The value is of type <see cref="String"/>.
      /// </summary>
      String,
      /// <summary>
      /// The value is <c>null</c>.
      /// </summary>
      Null = 0x12, // Same as SignatureElementTypes.Class
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.CustomAttributeDefinitions"/> table at table index <see cref="Tables.CustomAttribute"/>.
   /// </summary>
   public sealed class CustomAttributeDefinition
   {
      /// <summary>
      /// Gets or sets the owner for this <see cref="CustomAttributeDefinition"/>.
      /// </summary>
      /// <value>The owner for this <see cref="CustomAttributeDefinition"/>.</value>
      /// <remarks>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.HasCustomAttribute"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Parent { get; set; }

      /// <summary>
      /// Gets or sets the constructor used to create instance of custom attribute.
      /// </summary>
      /// <value>The constructor used to create instance of custom attribute.</value>
      /// <remarks>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.CustomAttributeType"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Type { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="AbstractCustomAttributeSignature"/> for this <see cref="CustomAttributeDefinition"/>.
      /// </summary>
      /// <value>The <see cref="AbstractCustomAttributeSignature"/> for this <see cref="CustomAttributeDefinition"/>.</value>
      /// <seealso cref="AbstractCustomAttributeSignature"/>
      public AbstractCustomAttributeSignature Signature { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.FieldMarshals"/> table at table index <see cref="Tables.FieldMarshal"/>.
   /// </summary>
   public sealed class FieldMarshal
   {
      /// <summary>
      /// Gets or sets the owner of this <see cref="FieldMarshal"/>.
      /// </summary>
      /// <value>The owner of this <see cref="FieldMarshal"/>.</value>
      /// <remarks>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.HasFieldMarshal"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Parent { get; set; }

      /// <summary>
      /// Gets or sets the marshaling information of this <see cref="FieldMarshal"/>.
      /// </summary>
      /// <value>the marshaling information of this <see cref="FieldMarshal"/>.</value>
      /// <seealso cref="AbstractMarshalingInfo"/>
      public AbstractMarshalingInfo NativeType { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.SecurityDefinitions"/> table at table index <see cref="Tables.DeclSecurity"/>.
   /// </summary>
   public sealed class SecurityDefinition
   {

      /// <summary>
      /// Creates a new instance of <see cref="SecurityDefinition"/> with initial capacity of <see cref="PermissionSets"/> set to <c>0</c>.
      /// </summary>
      public SecurityDefinition()
         : this( 0 )
      {
         // This exists instead of default parameters so that new() -constraint would be possible for rows (if ever needed) 
      }

      /// <summary>
      /// Creates a new instance of <see cref="SecurityDefinition"/> with given initial capacity of <see cref="PermissionSets"/>.
      /// </summary>
      /// <param name="permissionSetsCount">The initial capacity for <see cref="PermissionSets"/>.</param>
      public SecurityDefinition( Int32 permissionSetsCount )
      {
         this.PermissionSets = new List<AbstractSecurityInformation>( permissionSetsCount );
      }

      /// <summary>
      /// Gets or sets the <see cref="SecurityAction"/> associated with this security attribute declaration.
      /// </summary>
      /// <value>The <see cref="SecurityAction"/> associated with this security attribute declaration.</value>
      /// <seealso cref="SecurityAction"/>
      public SecurityAction Action { get; set; }

      /// <summary>
      /// Gets or sets the owner for this <see cref="SecurityDefinition"/>.
      /// </summary>
      /// <value>The owner for this <see cref="SecurityDefinition"/>.</value>
      /// <remarks>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.HasSecurity"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Parent { get; set; }

      /// <summary>
      /// Gets the list of <see cref="AbstractSecurityInformation"/>s that this <see cref="SecurityDefinition"/> has.
      /// </summary>
      /// <value>The list of <see cref="AbstractSecurityInformation"/>s that this <see cref="SecurityDefinition"/> has.</value>
      /// <seealso cref="AbstractSecurityInformation"/>
      public List<AbstractSecurityInformation> PermissionSets { get; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.ClassLayouts"/> table at table index <see cref="Tables.ClassLayout"/>.
   /// </summary>
   public sealed class ClassLayout
   {
      /// <summary>
      /// Creates a new instance of <see cref="ClassLayout"/> with <see cref="Parent"/> pointing to zeroth element of <see cref="Tables.TypeDef"/>.
      /// </summary>
      public ClassLayout()
         : this( 0 )
      {
         // This exists instead of default parameters so that new() -constraint would be possible for rows (if ever needed) 
      }

      /// <summary>
      /// Creates a new instance of <see cref="ClassLayout"/> with <see cref="Parent"/> pointing to given element of <see cref="Tables.TypeDef"/>.
      /// </summary>
      /// <param name="typeDefIndex">The zero-based index for <see cref="Parent"/>.</param>
      public ClassLayout( Int32 typeDefIndex )
      {
         this.Parent = new TableIndex( Tables.TypeDef, typeDefIndex );
      }

      /// <summary>
      /// Gets or sets the packing size for type referenced by <see cref="Parent"/>.
      /// </summary>
      /// <value>The packing size for type referenced by <see cref="Parent"/>.</value>
      public Int32 PackingSize { get; set; }

      /// <summary>
      /// Gets or sets the class size for type referenced by <see cref="Parent"/>.
      /// </summary>
      /// <value>The class size for type referenced by <see cref="Parent"/>.</value>
      public Int32 ClassSize { get; set; }

      /// <summary>
      /// Gets or sets the owner for this <see cref="ClassLayout"/>.
      /// </summary>
      /// <value>The owner for this <see cref="ClassLayout"/>.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.TypeDef"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Parent { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.FieldLayouts"/> table at table index <see cref="Tables.FieldLayout"/>.
   /// </summary>
   public sealed class FieldLayout
   {
      /// <summary>
      /// Creates a new instance of <see cref="FieldLayout"/> with <see cref="Field"/> pointing to zeroth element of <see cref="Tables.Field"/>.
      /// </summary>
      public FieldLayout()
         : this( 0 )
      {
         // This exists instead of default parameters so that new() -constraint would be possible for rows (if ever needed)
      }

      /// <summary>
      /// Creates a new instance of <see cref="FieldLayout"/> with <see cref="Field"/> pointing to given element of <see cref="Tables.Field"/>.
      /// </summary>
      /// <param name="fieldDefIndex">The zero-based index for <see cref="Field"/>.</param>
      public FieldLayout( Int32 fieldDefIndex )
      {
         this.Field = new TableIndex( Tables.Field, fieldDefIndex );
      }

      /// <summary>
      /// Gets or sets the offset of the field referenced by <see cref="Field"/>.
      /// </summary>
      /// <value>The offset of the field referenced by <see cref="Field"/>.</value>
      public Int32 Offset { get; set; }

      /// <summary>
      /// Gets or sets the owner of this <see cref="FieldLayout"/>.
      /// </summary>
      /// <value>The owner of this <see cref="FieldLayout"/>.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.Field"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Field { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.StandaloneSignatures"/> table at table index <see cref="Tables.StandaloneSignature"/>.
   /// </summary>
   public sealed class StandaloneSignature
   {
      /// <summary>
      /// Gets or sets the <see cref="AbstractSignature"/> of this <see cref="StandaloneSignature"/>.
      /// </summary>
      /// <value>The <see cref="AbstractSignature"/> of this <see cref="StandaloneSignature"/>.</value>
      /// <remarks>
      /// This signature should be either <see cref="LocalVariablesSignature"/> or <see cref="MethodReferenceSignature"/>.
      /// </remarks>
      /// <seealso cref="AbstractSignature"/>
      /// <seealso cref="LocalVariablesSignature"/>
      /// <seealso cref="MethodReferenceSignature"/>
      public AbstractSignature Signature { get; set; }

      /// <summary>
      /// Gets or sets the indicator, whether the <see cref="Signature"/> should be serialized with prefix used for field signatures.
      /// </summary>
      /// <remarks>
      /// <para>
      /// This value is not stored in serialized meta data directly.
      /// Indeed, if the meta data files would strictly adher for ECMA-335 spec (more specifically, the signatures referenced by some of these <see cref="Tables.StandaloneSignature"/> rows), this property would not be required.
      /// Here follows some background data.
      /// </para>
      /// <para>
      /// From <see href="https://social.msdn.microsoft.com/Forums/en-US/b4252eab-7aae-4456-9829-2707c8459e13/pinned-fields-in-the-common-language-runtime?forum=netfxtoolsdev"/>:
      /// After messing around further, and noticing that even the C# compiler emits Field signatures in the StandAloneSig table, the signatures seem to relate to PDB debugging symbols.
      /// When you emit symbols with the Debug or Release versions of your code, I'm guessing a StandAloneSig entry is injected and referred to by the PDB file.
      /// If you are in release mode and you generate no PDB info, the StandAloneSig table contains no Field signatures.
      /// One such condition for the emission of such information is constants within the scope of a method body.
      /// Original thread: <see href="http://www.netframeworkdev.com/building-development-diagnostic-tools-for-net/field-signatures-in-standalonesig-table-30658.shtml"/>.
      /// </para>
      /// </remarks>
      public Boolean StoreSignatureAsFieldSignature { get; set; }

   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.EventMaps"/> table at table index <see cref="Tables.EventMap"/>.
   /// </summary>
   public sealed class EventMap
   {
      /// <summary>
      /// Creates a new instance of <see cref="EventMap"/> with <see cref="Parent"/> and <see cref="EventList"/> pointing to zeroth row of <see cref="Tables.TypeDef"/> and <see cref="Tables.Event"/> tables, respectively.
      /// </summary>
      public EventMap()
         : this( 0, 0 )
      {
         // This exists instead of default parameters so that new() -constraint would be possible for rows (if ever needed)
      }

      /// <summary>
      /// Creates a new instance of <see cref="EventMap"/> with <see cref="Parent"/> and <see cref="EventList"/> pointing to given rows of <see cref="Tables.TypeDef"/> and <see cref="Tables.Event"/> tables, respectively.
      /// </summary>
      /// <param name="typeDefIndex">The zero-based index for <see cref="Parent"/>.</param>
      /// <param name="eventDefIndex">The zero-based index for <see cref="EventList"/>.</param>
      public EventMap( Int32 typeDefIndex, Int32 eventDefIndex )
      {
         this.Parent = new TableIndex( Tables.TypeDef, typeDefIndex );
         this.EventList = new TableIndex( Tables.Event, eventDefIndex );
      }

      /// <summary>
      /// Gets or sets the owner of this <see cref="EventMap"/>.
      /// </summary>
      /// <value>The owner of this <see cref="EventMap"/>.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.TypeDef"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Parent { get; set; }

      /// <summary>
      /// Gets or sets the index to the first element of the contiguous run of <see cref="EventDefinition"/>s owned by this <see cref="EventMap"/> (i.e. belonging to type referenced by <see cref="Parent"/>).
      /// </summary>
      /// <value>The index to the contiguous run of <see cref="EventDefinition"/>s owned by this <see cref="EventMap"/>.</value>
      /// <remarks>
      /// <para>
      /// The contiguous run of <see cref="EventDefinition"/>s ends at the smaller of the last row in <see cref="CILMetaData.EventDefinitions"/>, or the run of <see cref="EventDefinition"/>s started by the next <see cref="EventMap"/>.
      /// </para>
      /// <para>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.Event"/>.
      /// </para>
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex EventList { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.EventDefinitions"/> table at table index <see cref="Tables.Event"/>.
   /// </summary>
   public sealed class EventDefinition
   {
      /// <summary>
      /// Gets or sets the <see cref="EventAttributes"/> of this <see cref="EventDefinition"/>.
      /// </summary>
      /// <value>The <see cref="EventAttributes"/> of this <see cref="EventDefinition"/>.</value>
      /// <seealso cref="EventAttributes"/>
      public EventAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the name of this <see cref="EventDefinition"/>.
      /// </summary>
      /// <value>The name of this <see cref="EventDefinition"/>.</value>
      public String Name { get; set; }

      /// <summary>
      /// Gets or sets the type for this <see cref="EventDefinition"/>.
      /// </summary>
      /// <value>The type for this <see cref="EventDefinition"/>.</value>
      /// <remarks>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.TypeDefOrRef"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex EventType { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.PropertyMaps"/> table at table index <see cref="Tables.PropertyMap"/>.
   /// </summary>
   public sealed class PropertyMap
   {
      /// <summary>
      /// Creates a new instance of <see cref="PropertyMap"/> with <see cref="Parent"/> and <see cref="PropertyList"/> pointing to zeroth row of <see cref="Tables.TypeDef"/> and <see cref="Tables.Property"/> tables, respectively.
      /// </summary>
      public PropertyMap()
         : this( 0, 0 )
      {
         // This exists instead of default parameters so that new() -constraint would be possible for rows (if ever needed)
      }

      /// <summary>
      /// Creates a new instance of <see cref="PropertyMap"/> with <see cref="Parent"/> and <see cref="PropertyList"/> pointing to given rows of <see cref="Tables.TypeDef"/> and <see cref="Tables.Property"/> tables, respectively.
      /// </summary>
      /// <param name="typeDefIndex">The zero-based index for <see cref="Parent"/>.</param>
      /// <param name="propertyDefIndex">The zero-based index for <see cref="PropertyList"/>.</param>
      public PropertyMap( Int32 typeDefIndex, Int32 propertyDefIndex )
      {
         this.Parent = new TableIndex( Tables.TypeDef, typeDefIndex );
         this.PropertyList = new TableIndex( Tables.Property, propertyDefIndex );
      }

      /// <summary>
      /// Gets or sets the owner of this <see cref="PropertyMap"/>.
      /// </summary>
      /// <value>The owner of this <see cref="PropertyMap"/>.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.TypeDef"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Parent { get; set; }

      /// <summary>
      /// Gets or sets the index to the first element of the contiguous run of <see cref="PropertyDefinition"/>s owned by this <see cref="PropertyMap"/> (i.e. belonging to type referenced by <see cref="Parent"/>).
      /// </summary>
      /// <value>The index to the contiguous run of <see cref="PropertyDefinition"/>s owned by this <see cref="PropertyMap"/>.</value>
      /// <remarks>
      /// <para>
      /// The contiguous run of <see cref="PropertyDefinition"/>s ends at the smaller of the last row in <see cref="CILMetaData.EventDefinitions"/>, or the run of <see cref="PropertyDefinition"/>s started by the next <see cref="PropertyMap"/>.
      /// </para>
      /// <para>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.Property"/>.
      /// </para>
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex PropertyList { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.PropertyDefinitions"/> table at table index <see cref="Tables.Property"/>.
   /// </summary>
   public sealed class PropertyDefinition
   {
      /// <summary>
      /// Gets or sets the <see cref="PropertyAttributes"/> of this <see cref="PropertyDefinition"/>.
      /// </summary>
      /// <value>The <see cref="PropertyAttributes"/> of this <see cref="PropertyDefinition"/>.</value>
      /// <seealso cref="PropertyAttributes"/>
      public PropertyAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the name of this <see cref="PropertyDefinition"/>.
      /// </summary>
      /// <value>The name of this <see cref="PropertyDefinition"/>.</value>
      public String Name { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="PropertySignature"/> of this <see cref="PropertyDefinition"/>.
      /// </summary>
      /// <value>The <see cref="PropertySignature"/> of this <see cref="PropertyDefinition"/>.</value>
      /// <seealso cref="PropertySignature"/>
      public PropertySignature Signature { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.MethodSemantics"/> table at table index <see cref="Tables.MethodSemantics"/>.
   /// </summary>
   public sealed class MethodSemantics
   {
      /// <summary>
      /// Creates a new instance of <see cref="MethodSemantics"/> with <see cref="Method"/> pointing to zeroth row of <see cref="Tables.MethodDef"/> table.
      /// </summary>
      public MethodSemantics()
         : this( 0 )
      {
         // This exists instead of default parameters so that new() -constraint would be possible for rows (if ever needed)
      }

      /// <summary>
      /// Creates a new instance of <see cref="MethodSemantics"/> with <see cref="Method"/> pointing to given row of <see cref="Tables.MethodDef"/> table.
      /// </summary>
      /// <param name="methodDefIndex">The zero-based index for <see cref="Method"/>.</param>
      public MethodSemantics( Int32 methodDefIndex )
      {
         this.Method = new TableIndex( Tables.MethodDef, methodDefIndex );
      }

      /// <summary>
      /// Gets or sets the <see cref="MethodSemanticsAttributes"/> of this <see cref="MethodSemantics"/>.
      /// </summary>
      /// <value>The <see cref="MethodSemanticsAttributes"/> of this <see cref="MethodSemantics"/>.</value>
      /// <seealso cref="MethodSemanticsAttributes"/>
      public MethodSemanticsAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the associated method for this <see cref="MethodSemantics"/>.
      /// </summary>
      /// <value>The associated method for this <see cref="MethodSemantics"/>.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.MethodDef"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Method { get; set; }

      /// <summary>
      /// Gets or sets the reference to associated element (event or property).
      /// </summary>
      /// <value>The reference to associated element (event or property).</value>
      /// <remarks>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.HasSemantics"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Associaton { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.MethodImplementations"/> table at table index <see cref="Tables.MethodImpl"/>.
   /// </summary>
   public sealed class MethodImplementation
   {
      /// <summary>
      /// Creates a new instance of <see cref="MethodImplementation"/> with <see cref="Class"/> pointing to zeroth row of <see cref="Tables.TypeDef"/> table.
      /// </summary>
      public MethodImplementation()
         : this( 0 )
      {
         // This exists instead of default parameters so that new() -constraint would be possible for rows (if ever needed)
      }

      /// <summary>
      /// Creates a new instance of <see cref="MethodImplementation"/> with <see cref="Class"/> pointing to given row of <see cref="Tables.TypeDef"/> table.
      /// </summary>
      /// <param name="typeDefIndex">The zero-based index for <see cref="Class"/>.</param>
      public MethodImplementation( Int32 typeDefIndex )
      {
         this.Class = new TableIndex( Tables.TypeDef, typeDefIndex );
      }

      /// <summary>
      /// Gets or sets the class for this <see cref="MethodImplementation"/>.
      /// </summary>
      /// <value>The class for this <see cref="MethodImplementation"/>.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.TypeDef"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Class { get; set; }

      /// <summary>
      /// Gets or sets the reference to the method actually implementing <see cref="MethodDeclaration"/>.
      /// </summary>
      /// <value>The reference to the method actually implementing <see cref="MethodDeclaration"/>.</value>
      /// <remarks>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.MethodDefOrRef"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex MethodBody { get; set; }


      /// <summary>
      /// Gets or sets the reference to the method being implemented.
      /// </summary>
      /// <value>The reference to the method being implemented.</value>
      /// <remarks>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.MethodDefOrRef"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex MethodDeclaration { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.ModuleReferences"/> table at table index <see cref="Tables.ModuleRef"/>.
   /// </summary>
   public sealed class ModuleReference
   {
      /// <summary>
      /// Gets or sets the name of the module being referenced.
      /// </summary>
      /// <value>The name of the module being referenced.</value>
      public String ModuleName { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.TypeSpecifications"/> table at table index <see cref="Tables.TypeSpec"/>.
   /// </summary>
   public sealed class TypeSpecification
   {
      /// <summary>
      /// Gets or sets the <see cref="TypeSignature"/> for this <see cref="TypeSpecification"/>.
      /// </summary>
      /// <value>The <see cref="TypeSignature"/> for this <see cref="TypeSpecification"/>.</value>
      public TypeSignature Signature { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.MethodImplementationMaps"/> table at table index <see cref="Tables.ImplMap"/>.
   /// </summary>
   public sealed class MethodImplementationMap
   {
      /// <summary>
      /// Creates a new instance of <see cref="MethodImplementationMap"/> with <see cref="ImportScope"/> pointing to zeroth row of <see cref="Tables.ModuleRef"/> table.
      /// </summary>
      public MethodImplementationMap()
         : this( 0 )
      {
         // This exists instead of default parameters so that new() -constraint would be possible for rows (if ever needed)
      }

      /// <summary>
      /// Creates a new instance of <see cref="MethodImplementationMap"/> with <see cref="ImportScope"/> pointing to given row of <see cref="Tables.ModuleRef"/> table.
      /// </summary>
      /// <param name="moduleRefIndex">The zero-based index for <see cref="ImportScope"/>.</param>
      public MethodImplementationMap( Int32 moduleRefIndex )
      {
         this.ImportScope = new TableIndex( Tables.ModuleRef, moduleRefIndex );
      }

      /// <summary>
      /// Gets or sets the <see cref="PInvokeAttributes"/> for this <see cref="MethodImplementationMap"/>.
      /// </summary>
      public PInvokeAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the reference to the member (field or method) being forwarded for this <see cref="MethodImplementationMap"/>.
      /// </summary>
      /// <value>The reference to the member (field or method) being forwarded for this <see cref="MethodImplementationMap"/>.</value>
      /// <remarks>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.MemberForwarded"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex MemberForwarded { get; set; }

      /// <summary>
      /// Gets or sets the name of the imported function for this <see cref="MethodImplementationMap"/>.
      /// </summary>
      /// <value>The name of the imported function for this <see cref="MethodImplementationMap"/>.</value>
      public String ImportName { get; set; }

      /// <summary>
      /// Gets or sets the class for this <see cref="MethodImplementationMap"/>.
      /// </summary>
      /// <value>The class for this <see cref="MethodImplementationMap"/>.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.ModuleRef"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex ImportScope { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.FieldRVAs"/> table at table index <see cref="Tables.FieldRVA"/>.
   /// </summary>
   public sealed class FieldRVA
   {
      /// <summary>
      /// Creates a new instance of <see cref="FieldRVA"/> with <see cref="Field"/> pointing to zeroth row of <see cref="Tables.Field"/> table.
      /// </summary>
      public FieldRVA()
         : this( 0 )
      {
         // This exists instead of default parameters so that new() -constraint would be possible for rows (if ever needed)
      }

      /// <summary>
      /// Creates a new instance of <see cref="FieldRVA"/> with <see cref="Field"/> pointing to given row of <see cref="Tables.Field"/> table.
      /// </summary>
      /// <param name="fieldDefIndex">The zero-based index for <see cref="Field"/>.</param>
      public FieldRVA( Int32 fieldDefIndex )
      {
         this.Field = new TableIndex( Tables.ModuleRef, fieldDefIndex );
      }

      /// <summary>
      /// Gets or sets the binary data for this <see cref="FieldRVA"/>.
      /// </summary>
      /// <value>The binary data for this <see cref="FieldRVA"/>.</value>
      public Byte[] Data { get; set; }

      /// <summary>
      /// Gets or sets the field for this <see cref="FieldRVA"/>.
      /// </summary>
      /// <value>The field for this <see cref="FieldRVA"/>.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.Field"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Field { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.AssemblyDefinitions"/> table at table index <see cref="Tables.Assembly"/>.
   /// </summary>
   public sealed class AssemblyDefinition
   {

      /// <summary>
      /// Creates a new instance of <see cref="AssemblyDefinition"/>.
      /// </summary>
      public AssemblyDefinition()
      {
         this.AssemblyInformation = new AssemblyInformation();
      }

      /// <summary>
      /// Gets or sets the <see cref="AssemblyFlags"/> for this <see cref="AssemblyDefinition"/>.
      /// </summary>
      /// <value>The <see cref="AssemblyFlags"/> for this <see cref="AssemblyDefinition"/>.</value>
      public AssemblyFlags Attributes { get; set; }

      /// <summary>
      /// Gets the <see cref="Physical.AssemblyInformation"/> for this <see cref="AssemblyDefinition"/>.
      /// This object contains modifiable values like name, version, etc.
      /// </summary>
      /// <value>The <see cref="Physical.AssemblyInformation"/> for this <see cref="AssemblyDefinition"/>.</value>
      public AssemblyInformation AssemblyInformation { get; }

      /// <summary>
      /// Gets or sets the <see cref="AssemblyHashAlgorithm"/> for this <see cref="AssemblyDefinition"/>.
      /// </summary>
      /// <value>The <see cref="AssemblyHashAlgorithm"/> for this <see cref="AssemblyDefinition"/>.</value>
      public AssemblyHashAlgorithm HashAlgorithm { get; set; }

      /// <summary>
      /// Creates a textual representation of this <see cref="AssemblyDefinition"/>.
      /// </summary>
      /// <returns>A textual representation of this <see cref="AssemblyDefinition"/>.</returns>
      /// <remarks>
      /// Calling this method is equivalent of calling <see cref="Physical.AssemblyInformation.ToString(Boolean, Boolean)"/> and specifying to always use culture string and full public key string.
      /// </remarks>
      public override String ToString()
      {
         return this.AssemblyInformation.ToString( true, true );
      }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.AssemblyReferences"/> table at table index <see cref="Tables.AssemblyRef"/>.
   /// </summary>
   public sealed class AssemblyReference
   {

      /// <summary>
      /// Creates a new instance of <see cref="AssemblyReference"/>.
      /// </summary>
      public AssemblyReference()
      {
         this.AssemblyInformation = new AssemblyInformation();
      }

      /// <summary>
      /// Gets or sets the <see cref="AssemblyFlags"/> for this <see cref="AssemblyReference"/>.
      /// </summary>
      /// <value>The <see cref="AssemblyFlags"/> for this <see cref="AssemblyReference"/>.</value>
      public AssemblyFlags Attributes { get; set; }

      /// <summary>
      /// Gets the <see cref="Physical.AssemblyInformation"/> for this <see cref="AssemblyReference"/>.
      /// This object contains modifiable values like name, version, etc.
      /// </summary>
      /// <value>The <see cref="Physical.AssemblyInformation"/> for this <see cref="AssemblyReference"/>.</value>
      public AssemblyInformation AssemblyInformation { get; }

      /// <summary>
      /// Gets or sets the hash value of target assembly contents.
      /// </summary>
      /// <value>The hash value of target assembly contents.</value>
      /// <remarks>
      /// The algorithm for the hash is determined by <see cref="AssemblyDefinition.HashAlgorithm"/> of the <see cref="CILMetaData.AssemblyDefinitions"/>.
      /// </remarks>
      public Byte[] HashValue { get; set; }

      /// <summary>
      /// Creates a textual representation of this <see cref="AssemblyReference"/>.
      /// </summary>
      /// <returns>A textual representation of this <see cref="AssemblyReference"/>.</returns>
      /// <remarks>
      /// Calling this method is equivalent of calling <see cref="Physical.AssemblyInformation.ToString(Boolean, Boolean)"/> and specifying to always use culture string, and use full public key string only if <see cref="Attributes"/> has <see cref="AssemblyFlags.PublicKey"/> flag.
      /// </remarks>
      public override String ToString()
      {
         return this.AssemblyInformation.ToString( true, this.Attributes.IsFullPublicKey() );
      }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.FileReferences"/> table at table index <see cref="Tables.File"/>.
   /// </summary>
   public sealed class FileReference
   {
      /// <summary>
      /// Gets or sets the <see cref="FileAttributes"/> for this <see cref="FileReference"/>.
      /// </summary>
      /// <value>The <see cref="FileAttributes"/> for this <see cref="FileReference"/>.</value>
      public FileAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the name of this <see cref="FileReference"/>.
      /// </summary>
      /// <value>The name of this <see cref="FileReference"/>.</value>
      public String Name { get; set; }

      /// <summary>
      /// Gets or sets the hash value of this <see cref="FileReference"/>.
      /// </summary>
      /// <value>The hash value of this <see cref="FileReference"/>.</value>
      /// <remarks>
      /// The algorithm for the hash is determined by <see cref="AssemblyDefinition.HashAlgorithm"/> of the <see cref="CILMetaData.AssemblyDefinitions"/>.
      /// </remarks>
      public Byte[] HashValue { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.ExportedTypes"/> table at table index <see cref="Tables.ExportedType"/>.
   /// </summary>
   public sealed class ExportedType
   {
      /// <summary>
      /// Gets or sets the <see cref="TypeAttributes"/> for this <see cref="ExportedType"/>.
      /// </summary>
      /// <value>The <see cref="TypeAttributes"/> for this <see cref="ExportedType"/>.</value>
      public TypeAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets a hint for what is the type definition index in another module for this <see cref="ExportedType"/>.
      /// </summary>
      /// <value>A hint for what is the type definition index in another module for this <see cref="ExportedType"/>.</value>
      public Int32 TypeDefinitionIndex { get; set; }

      /// <summary>
      /// Gets or sets the name of this <see cref="ExportedType"/>.
      /// </summary>
      /// <value>The name of this <see cref="ExportedType"/>.</value>
      public String Name { get; set; }

      /// <summary>
      /// Gets or sets the namespace of this <see cref="ExportedType"/>.
      /// </summary>
      /// <value>The namespace of this <see cref="ExportedType"/>.</value>
      public String Namespace { get; set; }

      /// <summary>
      /// Gets or sets the reference to the implementation (another module, enclosing type, or assembly reference) of this <see cref="ExportedType"/>.
      /// </summary>
      /// <value>The reference to the implementation (another module, enclosing type, or assembly reference) of this <see cref="ExportedType"/>.</value>
      /// <remarks>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.Implementation"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Implementation { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.ManifestResources"/> table at table index <see cref="Tables.ManifestResource"/>.
   /// </summary>
   public sealed class ManifestResource
   {
      /// <summary>
      /// Gets or sets the offset in target file where this manifest resource starts, in bytes.
      /// </summary>
      /// <value>The offset in target file where this manifest resource starts, in bytes.</value>
      /// <remarks>
      /// This value is interpreted as unsigned 4-byte integer.
      /// </remarks>
      public Int32 Offset { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="ManifestResourceAttributes"/> of this <see cref="ManifestResource"/>.
      /// </summary>
      /// <value>The <see cref="ManifestResourceAttributes"/> of this <see cref="ManifestResource"/>.</value>
      public ManifestResourceAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the name of this <see cref="ManifestResource"/>.
      /// </summary>
      /// <value>The name of this <see cref="ManifestResource"/>.</value>
      public String Name { get; set; }

      /// <summary>
      /// Gets or sets the optional reference to the location (another module or assembly reference) of this <see cref="ManifestResource"/>.
      /// </summary>
      /// <value>The optional reference to the location (another module or assembly reference) of this <see cref="ManifestResource"/>.</value>
      /// <remarks>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.Implementation"/> schema.
      /// Exceptionally, the table <see cref="Tables.ExportedType"/> should not be used here.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex? Implementation { get; set; }

      /// <summary>
      /// Gets or sets the data that is embedded in the module when (de)serializing.
      /// </summary>
      /// <value>The data that is embedded in the module when (de)serializing.</value>
      /// <remarks>
      /// During (de)serializing process, this will be used only if <see cref="Implementation"/> is <c>null</c>.
      /// </remarks>
      public Byte[] EmbeddedData { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.NestedClassDefinitions"/> table at table index <see cref="Tables.NestedClass"/>.
   /// </summary>
   public sealed class NestedClassDefinition
   {
      /// <summary>
      /// Creates a new instance of <see cref="NestedClassDefinition"/> with <see cref="NestedClass"/> and <see cref="EnclosingClass"/> references pointing to zeroth row of <see cref="Tables.TypeDef"/> table.
      /// </summary>
      public NestedClassDefinition()
         : this( nestedClassIndex: 0, enclosingClassIndex: 0 )
      {
         // This exists instead of default parameters so that new() -constraint would be possible for rows (if ever needed)
      }

      /// <summary>
      /// Creates a new instance of <see cref="NestedClassDefinition"/> with <see cref="NestedClass"/> and <see cref="EnclosingClass"/> references pointing to given rows of <see cref="Tables.TypeDef"/> table.
      /// </summary>
      /// <param name="nestedClassIndex">The zero-based index for <see cref="NestedClass"/>.</param>
      /// <param name="enclosingClassIndex">The zero-based index for <see cref="EnclosingClass"/>.</param>
      public NestedClassDefinition( Int32 nestedClassIndex = 0, Int32 enclosingClassIndex = 0 )
      {
         this.NestedClass = new TableIndex( Tables.TypeDef, nestedClassIndex );
         this.EnclosingClass = new TableIndex( Tables.TypeDef, enclosingClassIndex );
      }

      /// <summary>
      /// Gets or sets the index of the type considered as nested type for this <see cref="NestedClassDefinition"/>.
      /// </summary>
      /// <value>The index of the type considered as nested type for this <see cref="NestedClassDefinition"/>.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.TypeDef"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex NestedClass { get; set; }

      /// <summary>
      /// Gets or sets the index of the type considered as enclosing type for <see cref="NestedClass"/>.
      /// </summary>
      /// <value>The index of the type considered as nested type for <see cref="NestedClass"/>.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.TypeDef"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex EnclosingClass { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.GenericParameterDefinitions"/> table at table index <see cref="Tables.GenericParameter"/>.
   /// </summary>
   public sealed class GenericParameterDefinition
   {
      /// <summary>
      /// Gets or sets the index of this <see cref="GenericParameterDefinition"/> within the declaring type or method.
      /// </summary>
      /// <value>The index of this <see cref="GenericParameterDefinition"/> within the declaring type or method.</value>
      public Int32 GenericParameterIndex { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="GenericParameterAttributes"/> of this <see cref="GenericParameterDefinition"/>.
      /// </summary>
      /// <value>The <see cref="GenericParameterAttributes"/> of this <see cref="GenericParameterDefinition"/>.</value>
      public GenericParameterAttributes Attributes { get; set; }

      /// <summary>
      /// Gets or sets the reference to the owner of this <see cref="GenericParameterDefinition"/>.
      /// </summary>
      /// <value>The reference to the owner of this <see cref="GenericParameterDefinition"/>.</value>
      /// <remarks>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.TypeOrMethodDef"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Owner { get; set; }

      /// <summary>
      /// Gets or sets the name of this <see cref="GenericParameterDefinition"/>.
      /// </summary>
      /// <value>The name of this <see cref="GenericParameterDefinition"/>.</value>
      public String Name { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.MethodSpecifications"/> table at table index <see cref="Tables.MethodSpec"/>.
   /// </summary>
   public sealed class MethodSpecification
   {
      /// <summary>
      /// Gets or sets the method generic definition of this instantiated generic method.
      /// </summary>
      /// <value>The method generic definition of this instantiated generic method.</value>
      /// <remarks>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.MethodDefOrRef"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Method { get; set; }

      /// <summary>
      /// Gets or sets the signature of this instantiated generic method.
      /// </summary>
      /// <value>The signature of this instantiated generic method.</value>
      /// <seealso cref="GenericMethodSignature"/>
      public GenericMethodSignature Signature { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.GenericParameterConstraintDefinitions"/> table at table index <see cref="Tables.GenericParameterConstraint"/>.
   /// </summary>
   public sealed class GenericParameterConstraintDefinition
   {

      /// <summary>
      /// Creates a new instance of <see cref="GenericParameterConstraintDefinition"/> with <see cref="Owner"/> pointing to zeroth row of <see cref="Tables.GenericParameter"/> table.
      /// </summary>
      public GenericParameterConstraintDefinition()
         : this( 0 )
      {
         // This exists instead of default parameters so that new() -constraint would be possible for rows (if ever needed)
      }

      /// <summary>
      /// Creates a new instance of <see cref="GenericParameterConstraintDefinition"/> with <see cref="Owner"/> pointing to given row of <see cref="Tables.GenericParameter"/> table.
      /// </summary>
      /// <param name="genericParamIndex">The zero-based index for <see cref="Owner"/>.</param>
      public GenericParameterConstraintDefinition( Int32 genericParamIndex )
      {
         this.Owner = new TableIndex( Tables.GenericParameter, genericParamIndex );
      }

      /// <summary>
      /// Gets or sets the owner of this <see cref="GenericParameterConstraintDefinition"/>.
      /// </summary>
      /// <value>The owner of  this <see cref="GenericParameterConstraintDefinition"/>.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.GenericParameter"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Owner { get; set; }

      /// <summary>
      /// Gets or sets the constraint for this <see cref="GenericParameterConstraintDefinition"/>.
      /// </summary>
      /// <value>The constraint for this <see cref="GenericParameterConstraintDefinition"/>.</value>
      /// <remarks>
      /// The schema for this table index corresponds to the <see cref="P:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.TypeDefOrRef"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Constraint { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.EditAndContinueLog"/> table at table index <see cref="Tables.EncLog"/>.
   /// </summary>
   public sealed class EditAndContinueLog
   {
      /// <summary>
      /// Gets or sets the raw token for this <see cref="EditAndContinueLog"/>.
      /// </summary>
      /// <value>The raw token for this <see cref="EditAndContinueLog"/>.</value>
      public Int32 Token { get; set; }

      /// <summary>
      /// Gets or sets the function code for this <see cref="EditAndContinueLog"/>.
      /// </summary>
      /// <value>The function code for this <see cref="EditAndContinueLog"/>.</value>
      public Int32 FuncCode { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.EditAndContinueMap"/> table at table index <see cref="Tables.EncMap"/>.
   /// </summary>
   public sealed class EditAndContinueMap
   {
      /// <summary>
      /// Gets or sets the raw token for this <see cref="EditAndContinueMap"/>.
      /// </summary>
      /// <value>The raw token for this <see cref="EditAndContinueMap"/>.</value>
      public Int32 Token { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.FieldDefinitionPointers"/> table at table index <see cref="Tables.FieldPtr"/>.
   /// </summary>
   public sealed class FieldDefinitionPointer
   {
      /// <summary>
      /// Gets or sets the referenced field definition.
      /// </summary>
      /// <value>The the referenced field definition.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.Field"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex FieldIndex { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.MethodDefinitionPointers"/> table at table index <see cref="Tables.MethodPtr"/>.
   /// </summary>
   public sealed class MethodDefinitionPointer
   {
      /// <summary>
      /// Gets or sets the referenced method definition.
      /// </summary>
      /// <value>The the referenced method definition.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.MethodDef"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex MethodIndex { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.ParameterDefinitionPointers"/> table at table index <see cref="Tables.ParameterPtr"/>.
   /// </summary>
   public sealed class ParameterDefinitionPointer
   {
      /// <summary>
      /// Gets or sets the referenced parameter definition.
      /// </summary>
      /// <value>The the referenced parameter definition.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.Parameter"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex ParameterIndex { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.EventDefinitionPointers"/> table at table index <see cref="Tables.EventPtr"/>.
   /// </summary>
   public sealed class EventDefinitionPointer
   {
      /// <summary>
      /// Gets or sets the referenced event definition.
      /// </summary>
      /// <value>The the referenced event definition.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.Event"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex EventIndex { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.PropertyDefinitionPointers"/> table at table index <see cref="Tables.PropertyPtr"/>.
   /// </summary>
   public sealed class PropertyDefinitionPointer
   {
      /// <summary>
      /// Gets or sets the referenced property definition.
      /// </summary>
      /// <value>The the referenced property definition.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.Property"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex PropertyIndex { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.AssemblyDefinitionProcessors"/> table at table index <see cref="Tables.AssemblyProcessor"/>.
   /// </summary>
   [Obsolete( "Rows of these type should no longer be present in CIL meta data file.", false )]
   public sealed class AssemblyDefinitionProcessor
   {
      /// <summary>
      /// Gets or sets the processor kind for this <see cref="AssemblyDefinitionProcessor"/>.
      /// </summary>
      /// <value>The processor kind for this <see cref="AssemblyDefinitionProcessor"/>.</value>
      public Int32 Processor { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.AssemblyDefinitionOSs"/> table at table index <see cref="Tables.AssemblyOS"/>.
   /// </summary>
   [Obsolete( "Rows of these type should no longer be present in CIL meta data file.", false )]
   public sealed class AssemblyDefinitionOS
   {
      /// <summary>
      /// Gets or sets the OS platform ID for this <see cref="AssemblyDefinitionOS"/>.
      /// </summary>
      /// <value>The OS platform ID for this <see cref="AssemblyDefinitionOS"/>.</value>
      public Int32 OSPlatformID { get; set; }

      /// <summary>
      /// Gets or sets the major version of OS for this <see cref="AssemblyDefinitionOS"/>.
      /// </summary>
      /// <value>The major version of OS for this <see cref="AssemblyDefinitionOS"/>.</value>
      public Int32 OSMajorVersion { get; set; }

      /// <summary>
      /// Gets or sets the minor version of OS for this <see cref="AssemblyDefinitionOS"/>.
      /// </summary>
      /// <value>The minor version of OS for this <see cref="AssemblyDefinitionOS"/>.</value>
      public Int32 OSMinorVersion { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.AssemblyReferenceProcessors"/> table at table index <see cref="Tables.AssemblyRefProcessor"/>.
   /// </summary>
   [Obsolete( "Rows of these type should no longer be present in CIL meta data file.", false )]
   public sealed class AssemblyReferenceProcessor
   {
      /// <summary>
      /// Creates a new instance of <see cref="AssemblyReferenceProcessor"/> with <see cref="AssemblyRef"/> pointing to zeroth row of <see cref="Tables.AssemblyRef"/> table.
      /// </summary>
      public AssemblyReferenceProcessor()
         : this( 0 )
      {
         // This exists instead of default parameters so that new() -constraint would be possible for rows (if ever needed)
      }

      /// <summary>
      /// Creates a new instance of <see cref="AssemblyReferenceProcessor"/> with <see cref="AssemblyRef"/> pointing to given row of <see cref="Tables.AssemblyRef"/> table.
      /// </summary>
      /// <param name="assemblyRefIndex">The zero-based index for <see cref="AssemblyRef"/>.</param>
      public AssemblyReferenceProcessor( Int32 assemblyRefIndex )
      {
         this.AssemblyRef = new TableIndex( Tables.AssemblyRef, assemblyRefIndex );
      }

      /// <summary>
      /// Gets or sets the processor kind for this <see cref="AssemblyReferenceProcessor"/>.
      /// </summary>
      /// <value>The processor kind for this <see cref="AssemblyReferenceProcessor"/>.</value>
      public Int32 Processor { get; set; }

      /// <summary>
      /// Gets or sets the referenced assembly reference.
      /// </summary>
      /// <value>The the referenced assembly reference.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.AssemblyRef"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex AssemblyRef { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.AssemblyReferenceOSs"/> table at table index <see cref="Tables.AssemblyRefOS"/>.
   /// </summary>
   [Obsolete( "Rows of these type should no longer be present in CIL meta data file.", false )]
   public sealed class AssemblyReferenceOS
   {
      /// <summary>
      /// Creates a new instance of <see cref="AssemblyReferenceOS"/> with <see cref="AssemblyRef"/> pointing to zeroth row of <see cref="Tables.AssemblyRef"/> table.
      /// </summary>
      public AssemblyReferenceOS()
         : this( 0 )
      {
         // This exists instead of default parameters so that new() -constraint would be possible for rows (if ever needed)
      }

      /// <summary>
      /// Creates a new instance of <see cref="AssemblyReferenceOS"/> with <see cref="AssemblyRef"/> pointing to given row of <see cref="Tables.AssemblyRef"/> table.
      /// </summary>
      /// <param name="assemblyRefIndex">The zero-based index for <see cref="AssemblyRef"/>.</param>
      public AssemblyReferenceOS( Int32 assemblyRefIndex )
      {
         this.AssemblyRef = new TableIndex( Tables.AssemblyRef, assemblyRefIndex );
      }

      /// <summary>
      /// Gets or sets the OS platform ID for this <see cref="AssemblyReferenceOS"/>.
      /// </summary>
      /// <value>The OS platform ID for this <see cref="AssemblyReferenceOS"/>.</value>
      public Int32 OSPlatformID { get; set; }

      /// <summary>
      /// Gets or sets the major version of OS for this <see cref="AssemblyReferenceOS"/>.
      /// </summary>
      /// <value>The major version of OS for this <see cref="AssemblyReferenceOS"/>.</value>
      public Int32 OSMajorVersion { get; set; }

      /// <summary>
      /// Gets or sets the minor version of OS for this <see cref="AssemblyReferenceOS"/>.
      /// </summary>
      /// <value>The minor version of OS for this <see cref="AssemblyReferenceOS"/>.</value>
      public Int32 OSMinorVersion { get; set; }

      /// <summary>
      /// Gets or sets the referenced assembly reference.
      /// </summary>
      /// <value>The the referenced assembly reference.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this table index should always be <see cref="Tables.AssemblyRef"/>. 
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex AssemblyRef { get; set; }

   }

   /// <summary>
   /// This structure represents a reference to a certain row in a certain table in <see cref="CILMetaData"/>.
   /// </summary>
   /// <remarks>
   /// Because how this class is implemented, the maximum value for an <see cref="Index"/> is <c>0x00FFFFFF</c>.
   /// </remarks>
   public struct TableIndex : IEquatable<TableIndex>, IComparable<TableIndex>, IComparable
   {
      private readonly Int32 _token;

      /// <summary>
      /// Creates a new <see cref="TableIndex"/> with given table and zero-based index.
      /// </summary>
      /// <param name="aTable">The <see cref="Tables"/> enumeration.</param>
      /// <param name="anIdx">The zero-based index.</param>
      /// <remarks>
      /// Because how this class is implemented, the <paramref name="anIdx"/> should be in inclusive range <c>[0, 0x00FFFFFF]</c>.
      /// </remarks>
      public TableIndex( Tables aTable, Int32 anIdx )
         : this( ( (Int32) aTable << 24 ) | anIdx )
      {
      }

      private TableIndex( Int32 token )
      {
         this._token = token;
      }

      /// <summary>
      /// Gets the <see cref="Tables"/> value identifying the target table.
      /// </summary>
      /// <value>The <see cref="Tables"/> value identifying the target table.</value>
      public Tables Table
      {
         get
         {
            return (Tables) ( this._token >> 24 );
         }
      }

      /// <summary>
      /// Gets the zero-based index identifying the row in the target table.
      /// </summary>
      /// <value>The zero-based index identifying the row in the target table.</value>
      /// <remarks>
      /// Because how this class is implemented, the returned value will always be in inclusive range <c>[0, 0x00FFFFFF]</c>.
      /// </remarks>
      public Int32 Index
      {
         get
         {
            return this._token & CAMCoreInternals.INDEX_MASK;
         }
      }


#pragma warning disable 1591
      // TODO this property needs to be internalized!!
      public Int32 CombinedValue
      {
         get
         {
            return this._token;
         }
      }
#pragma warning restore 1591

      /// <summary>
      /// Checks whether given object is of type <see cref="TableIndex"/> and is considered to be equal to this <see cref="TableIndex"/>.
      /// </summary>
      /// <param name="obj">The object to check.</param>
      /// <returns><c>true</c> if <paramref name="obj"/> is of type <see cref="TableIndex"/> and is equa lto this <see cref="TableIndex"/>.</returns>
      /// <seealso cref="Equals(TableIndex)"/>
      public override Boolean Equals( Object obj )
      {
         return obj is TableIndex && this.Equals( (TableIndex) obj );
      }

      /// <summary>
      /// Computes the hash code for this <see cref="TableIndex"/>.
      /// </summary>
      /// <returns>The hash code for this <see cref="TableIndex"/>.</returns>
      public override Int32 GetHashCode()
      {
         return this._token;
      }

      /// <summary>
      /// Checks that both <see cref="Table"/> and <see cref="Index"/> match for this <see cref="TableIndex"/> and the one given as parameter.
      /// </summary>
      /// <param name="other">The other <see cref="TableIndex"/>.</param>
      /// <returns><c>true</c> if both <see cref="Table"/> and <see cref="Index"/> match for this <see cref="TableIndex"/> and <paramref name="other"/>; <c>false</c> otherwise.</returns>
      public Boolean Equals( TableIndex other )
      {
         return this._token == other._token;
      }

      /// <summary>
      /// Createss the textual representation of this <see cref="TableIndex"/>, containing the <see cref="Table"/> and <see cref="Index"/>.
      /// </summary>
      /// <returns>The textual representation of this <see cref="TableIndex"/>.</returns>
      public override String ToString()
      {
         return this.Table + "[" + this.Index + "]";
      }

      /// <summary>
      /// Compares this <see cref="TableIndex"/> to the one given as parameter, and returns integer indicating whether this <see cref="TableIndex"/> is less, equal to, or greaer than the parameter.
      /// </summary>
      /// <param name="other">The other <see cref="TableIndex"/>.</param>
      /// <returns>Integer value indicating whether this <see cref="TableIndex"/> is less, equal to, or greater than <paramref name="other"/>.</returns>
      /// <remarks>
      /// Negative value means that this <see cref="TableIndex"/> is less than <paramref name="other"/>, zero means that both are equal, and positive value means that this <see cref="TableIndex"/> is greater than <paramref name="other"/>.
      /// </remarks>
      public Int32 CompareTo( TableIndex other )
      {
         var retVal = this.Table.CompareTo( other.Table );
         if ( retVal == 0 )
         {
            retVal = this.Index.CompareTo( other.Index );
         }

         return retVal;
      }

      Int32 IComparable.CompareTo( Object obj )
      {
         if ( obj == null )
         {
            // This is always 'greater' than null
            return 1;
         }
         else if ( obj is TableIndex )
         {
            return this.CompareTo( (TableIndex) obj );
         }
         else
         {
            throw new ArgumentException( "Given object must be of type " + this.GetType() + " or null." );
         }
      }

      /// <summary>
      /// Compares two <see cref="TableIndex"/>es for equality.
      /// </summary>
      /// <param name="x">The first <see cref="TableIndex"/>.</param>
      /// <param name="y">The second <see cref="TableIndex"/>.</param>
      /// <returns>The result of <see cref="TableIndex.Equals(TableIndex)"/>.</returns>
      public static Boolean operator ==( TableIndex x, TableIndex y )
      {
         return x.Equals( y );
      }

      /// <summary>
      /// Compares two <see cref="TableIndex"/>es for inequality.
      /// </summary>
      /// <param name="x">The first <see cref="TableIndex"/>.</param>
      /// <param name="y">The second <see cref="TableIndex"/>.</param>
      /// <returns>The reversed result of <see cref="operator ==(TableIndex, TableIndex)"/>.</returns>
      public static Boolean operator !=( TableIndex x, TableIndex y )
      {
         return !( x == y );
      }

      /// <summary>
      /// Compares whether the first <see cref="TableIndex"/> is considered to be less than the second.
      /// </summary>
      /// <param name="x">The first <see cref="TableIndex"/>.</param>
      /// <param name="y">The second <see cref="TableIndex"/>.</param>
      /// <returns><c>true</c> if <paramref name="x"/> is considered to be less than <paramref name="y"/>; <c>false</c> otherwise.</returns>
      public static Boolean operator <( TableIndex x, TableIndex y )
      {
         return x.CompareTo( y ) < 0;
      }

      /// <summary>
      /// Compares whether the first <see cref="TableIndex"/> is considered to be greater than the second.
      /// </summary>
      /// <param name="x">The first <see cref="TableIndex"/>.</param>
      /// <param name="y">The second <see cref="TableIndex"/>.</param>
      /// <returns><c>true</c> if <paramref name="x"/> is considered to be greater than <paramref name="y"/>; <c>false</c> otherwise.</returns>
      public static Boolean operator >( TableIndex x, TableIndex y )
      {
         return x.CompareTo( y ) > 0;
      }

      /// <summary>
      /// Compares whether the first <see cref="TableIndex"/> is considered to be less than or equal to the second.
      /// </summary>
      /// <param name="x">The first <see cref="TableIndex"/>.</param>
      /// <param name="y">The second <see cref="TableIndex"/>.</param>
      /// <returns><c>true</c> if <paramref name="x"/> is considered to be less than or equal to <paramref name="y"/>; <c>false</c> otherwise.</returns>
      public static Boolean operator <=( TableIndex x, TableIndex y )
      {
         return !( x > y );
      }

      /// <summary>
      /// Compares whether the first <see cref="TableIndex"/> is considered to be greater than or equal to the second.
      /// </summary>
      /// <param name="x">The first <see cref="TableIndex"/>.</param>
      /// <param name="y">The second <see cref="TableIndex"/>.</param>
      /// <returns><c>true</c> if <paramref name="x"/> is considered to be greater than or equal to <paramref name="y"/>; <c>false</c> otherwise.</returns>
      public static Boolean operator >=( TableIndex x, TableIndex y )
      {
         return !( x < y );
      }

      /// <summary>
      /// Creates a new <see cref="TableIndex"/> from token encoded with index value being one-based (i.e. first element is <c>1</c>, not <c>0</c>).
      /// </summary>
      /// <param name="token">The encoded token.</param>
      /// <returns>A new <see cref="TableIndex"/> with decoded table and index values, or <c>null</c> if <paramref name="token"/> is <c>0</c>.</returns>
      public static TableIndex? FromOneBasedTokenNullable( Int32 token )
      {
         return token == 0 ?
            (TableIndex?) null :
            FromOneBasedToken( token );
      }

      /// <summary>
      /// Creates a new <see cref="TableIndex"/> from token encoded with index value being one-based (i.e. first element is <c>1</c>, not <c>0</c>).
      /// </summary>
      /// <param name="token">The encoded token.</param>
      /// <returns>A new <see cref="TableIndex"/> with decoded table and index values.</returns>
      /// <remarks>
      /// If <paramref name="token"/> is <c>0</c>, then the index of resulting <see cref="TableIndex"/> will be something else.
      /// To support zero values, please use <see cref="FromOneBasedTokenNullable"/> method.
      /// </remarks>
      public static TableIndex FromOneBasedToken( Int32 token )
      {
         return new TableIndex( ( ( token & CAMCoreInternals.INDEX_MASK ) - 1 ) | ( token & ~CAMCoreInternals.INDEX_MASK ) );
      }

      /// <summary>
      /// Creates a new <see cref="TableIndex"/> from token encoded with index value being zero-based (i.e. first element is <c>0</c>).
      /// </summary>
      /// <param name="token">The encoded token.</param>
      /// <returns>A new <see cref="TableIndex"/> with decoded table and index values.</returns>
      public static TableIndex FromZeroBasedToken( Int32 token )
      {
         return new TableIndex( token );
      }
   }

   /// <summary>
   /// This enumeration contains the IDs of all the fixed tables present in <see cref="CILMetaData"/>.
   /// </summary>
   public enum Tables : byte
   {
      /// <summary>
      /// The ID for <see cref="CILMetaData.AssemblyDefinitions"/> table (<c>0x20</c>).
      /// </summary>
      Assembly = 0x20,

      /// <summary>
      /// The ID for <see cref="CILMetaData.AssemblyDefinitionOSs"/> table (<c>0x22</c>).
      /// </summary>
      AssemblyOS = 0x22,

      /// <summary>
      /// The ID for <see cref="CILMetaData.AssemblyDefinitionProcessors"/> table (<c>0x21</c>).
      /// </summary>
      AssemblyProcessor = 0x21,

      /// <summary>
      /// The ID for <see cref="CILMetaData.AssemblyReferences"/> table (<c>0x23</c>).
      /// </summary>
      AssemblyRef = 0x23,

      /// <summary>
      /// The ID for <see cref="CILMetaData.AssemblyReferenceOSs"/> table (<c>0x25</c>).
      /// </summary>
      AssemblyRefOS = 0x25,

      /// <summary>
      /// The ID for <see cref="CILMetaData.AssemblyReferenceProcessors"/> table (<c>0x24</c>).
      /// </summary>
      AssemblyRefProcessor = 0x24,

      /// <summary>
      /// The ID for <see cref="CILMetaData.ClassLayouts"/> table (<c>0x0F</c>).
      /// </summary>
      ClassLayout = 0x0F,

      /// <summary>
      /// The ID for <see cref="CILMetaData.ConstantDefinitions"/> table (<c>0x0B</c>).
      /// </summary>
      Constant = 0x0B,

      /// <summary>
      /// The ID for <see cref="CILMetaData.CustomAttributeDefinitions"/> table (<c>0x0C</c>).
      /// </summary>
      CustomAttribute = 0x0C,

      /// <summary>
      /// The ID for <see cref="CILMetaData.SecurityDefinitions"/> table (<c>0x0E</c>).
      /// </summary>
      DeclSecurity = 0x0E,

      /// <summary>
      /// The ID for <see cref="CILMetaData.EditAndContinueLog"/> table (<c>0x1E</c>).
      /// </summary>
      EncLog = 0x1E,

      /// <summary>
      /// The ID for <see cref="CILMetaData.EditAndContinueMap"/> table (<c>0x1F</c>).
      /// </summary>
      EncMap = 0x1F,

      /// <summary>
      /// The ID for <see cref="CILMetaData.EventMaps"/> table (<c>0x12</c>).
      /// </summary>
      EventMap = 0x12,

      /// <summary>
      /// The ID for <see cref="CILMetaData.EventDefinitions"/> table (<c>0x14</c>).
      /// </summary>
      Event = 0x14,

      /// <summary>
      /// The ID for <see cref="CILMetaData.EventDefinitionPointers"/> table (<c>0x13</c>).
      /// </summary>
      EventPtr = 0x13,

      /// <summary>
      /// The ID for <see cref="CILMetaData.ExportedTypes"/> table (<c>0x27</c>).
      /// </summary>
      ExportedType = 0x27,

      /// <summary>
      /// The ID for <see cref="CILMetaData.FieldDefinitions"/> table (<c>0x04</c>).
      /// </summary>
      Field = 0x04,

      /// <summary>
      /// The ID for <see cref="CILMetaData.FieldLayouts"/> table (<c>0x10</c>).
      /// </summary>
      FieldLayout = 0x10,

      /// <summary>
      /// The ID for <see cref="CILMetaData.FieldMarshals"/> table (<c>0x0D</c>).
      /// </summary>
      FieldMarshal = 0x0D,

      /// <summary>
      /// The ID for <see cref="CILMetaData.FieldDefinitionPointers"/> table (<c>0x03</c>).
      /// </summary>
      FieldPtr = 0x03,

      /// <summary>
      /// The ID for <see cref="CILMetaData.FieldRVAs"/> table (<c>0x1D</c>).
      /// </summary>
      FieldRVA = 0x1D,

      /// <summary>
      /// The ID for <see cref="CILMetaData.FileReferences"/> table (<c>0x26</c>).
      /// </summary>
      File = 0x26,

      /// <summary>
      /// The ID for <see cref="CILMetaData.GenericParameterDefinitions"/> table (<c>0x2A</c>).
      /// </summary>
      GenericParameter = 0x2A,

      /// <summary>
      /// The ID for <see cref="CILMetaData.GenericParameterConstraintDefinitions"/> table (<c>0x2C</c>).
      /// </summary>
      GenericParameterConstraint = 0x2C,

      /// <summary>
      /// The ID for <see cref="CILMetaData.MethodImplementationMaps"/> table (<c>0x1C</c>).
      /// </summary>
      ImplMap = 0x1C,

      /// <summary>
      /// The ID for <see cref="CILMetaData.InterfaceImplementations"/> table (<c>0x09</c>).
      /// </summary>
      InterfaceImpl = 0x09,

      /// <summary>
      /// The ID for <see cref="CILMetaData.ManifestResources"/> table (<c>0x28</c>).
      /// </summary>
      ManifestResource = 0x28,

      /// <summary>
      /// The ID for <see cref="CILMetaData.MemberReferences"/> table (<c>0x0A</c>).
      /// </summary>
      MemberRef = 0x0A,

      /// <summary>
      /// The ID for <see cref="CILMetaData.MethodDefinitions"/> table (<c>0x06</c>).
      /// </summary>
      MethodDef = 0x06,

      /// <summary>
      /// The ID for <see cref="CILMetaData.MethodImplementations"/> table (<c>0x19</c>).
      /// </summary>
      MethodImpl = 0x19,

      /// <summary>
      /// The ID for <see cref="CILMetaData.MethodDefinitionPointers"/> table (<c>0x05</c>).
      /// </summary>
      MethodPtr = 0x05,

      /// <summary>
      /// The ID for <see cref="CILMetaData.MethodSemantics"/> table (<c>0x18</c>).
      /// </summary>
      MethodSemantics = 0x18,

      /// <summary>
      /// The ID for <see cref="CILMetaData.MethodSpecifications"/> table (<c>0x2B</c>).
      /// </summary>
      MethodSpec = 0x2B,

      /// <summary>
      /// The ID for <see cref="CILMetaData.ModuleDefinitions"/> table (<c>0x00</c>).
      /// </summary>
      Module = 0x00,

      /// <summary>
      /// The ID for <see cref="CILMetaData.ModuleReferences"/> table (<c>0x1A</c>).
      /// </summary>
      ModuleRef = 0x1A,

      /// <summary>
      /// The ID for <see cref="CILMetaData.NestedClassDefinitions"/> table (<c>0x29</c>).
      /// </summary>
      NestedClass = 0x29,

      /// <summary>
      /// The ID for <see cref="CILMetaData.ParameterDefinitions"/> table (<c>0x08</c>).
      /// </summary>
      Parameter = 0x08,

      /// <summary>
      /// The ID for <see cref="CILMetaData.ParameterDefinitionPointers"/> table (<c>0x07</c>).
      /// </summary>
      ParameterPtr = 0x07,

      /// <summary>
      /// The ID for <see cref="CILMetaData.PropertyDefinitions"/> table (<c>0x17</c>).
      /// </summary>
      Property = 0x17,

      /// <summary>
      /// The ID for <see cref="CILMetaData.PropertyDefinitionPointers"/> table (<c>0x16</c>).
      /// </summary>
      PropertyPtr = 0x16,

      /// <summary>
      /// The ID for <see cref="CILMetaData.PropertyMaps"/> table (<c>0x15</c>).
      /// </summary>
      PropertyMap = 0x15,

      /// <summary>
      /// The ID for <see cref="CILMetaData.StandaloneSignatures"/> table (<c>0x11</c>).
      /// </summary>
      StandaloneSignature = 0x11,

      /// <summary>
      /// The ID for <see cref="CILMetaData.TypeDefinitions"/> table (<c>0x02</c>).
      /// </summary>
      TypeDef = 0x02,

      /// <summary>
      /// The ID for <see cref="CILMetaData.TypeReferences"/> table (<c>0x01</c>).
      /// </summary>
      TypeRef = 0x01,

      /// <summary>
      /// The ID for <see cref="CILMetaData.TypeSpecifications"/> table (<c>0x1B</c>).
      /// </summary>
      TypeSpec = 0x1B
   }

   /// <summary>
   /// This class contains information about a single target framework (e.g. .NET 4.0, or .NETPortable 4.5 ProfileXY).
   /// </summary>
   /// <remarks>
   /// This class exists mostly because the type <see cref="T:System.Runtime.Versioning.FrameworkName"/> is missing from PCL framework assemblies.
   /// Furthermore, this class does not perform any kind of validation for values of <see cref="Identifier"/>, <see cref="Version"/>, and <see cref="Profile"/>.
   /// </remarks>
   public sealed class TargetFrameworkInfo : IEquatable<TargetFrameworkInfo>
   {
      /// <summary>
      /// Creates a new instance of <see cref="TargetFrameworkInfo"/> with given target framework identifier, version, and profile.
      /// </summary>
      /// <param name="identifier">The target framework identifier. May be <c>null</c>.</param>
      /// <param name="version">The target framework version. May be <c>null</c>.</param>
      /// <param name="profile">The target framework profile. May be <c>null</c>.</param>
      public TargetFrameworkInfo( String identifier, String version, String profile )
      {
         this.Identifier = identifier;
         this.Version = version;
         this.Profile = profile;
      }

      /// <summary>
      /// Gets the the target framework identifier string.
      /// </summary>
      /// <value>The the target framework identifier string.</value>
      public String Identifier { get; }

      /// <summary>
      /// Gets the target framework version string.
      /// </summary>
      /// <value>The target framework version string.</value>
      public String Version { get; }

      /// <summary>
      /// Gets the target framework profile string.
      /// </summary>
      /// <value>The target framework profile string.</value>
      public String Profile { get; }

      /// <summary>
      /// Checks that given object is of type <see cref="TargetFrameworkInfo"/>, and that it contains same data as this <see cref="TargetFrameworkInfo"/>.
      /// </summary>
      /// <param name="obj">The object to check.</param>
      /// <returns><c>true</c>, if <paramref name="obj"/> is of type <see cref="TargetFrameworkInfo"/>, ans contains same data as this <see cref="TargetFrameworkInfo"/>; <c>false</c> otherwise.</returns>
      public override Boolean Equals( Object obj )
      {
         return this.Equals( obj as TargetFrameworkInfo );
      }

      /// <summary>
      /// Computes the hash code for this <see cref="TargetFrameworkInfo"/>.
      /// </summary>
      /// <returns>The hash code for this <see cref="TargetFrameworkInfo"/>.</returns>
      public override Int32 GetHashCode()
      {
         return ( ( 17 * 23 + this.Identifier.GetHashCodeSafe() ) * 23 + this.Version.GetHashCodeSafe() ) * 23 + this.Profile.GetHashCodeSafe();
      }

      /// <summary>
      /// Creates a textual representation of this <see cref="TargetFrameworkInfo"/>.
      /// This will contain <see cref="Identifier"/>, <see cref="Version"/>, and <see cref="Profile"/> in a same format as the <see cref="System.Runtime.Versioning.TargetFrameworkAttribute.TargetFrameworkAttribute(String)"/> constructor accepts it.
      /// </summary>
      /// <returns>A textual representation of this <see cref="TargetFrameworkInfo"/>.</returns>
      public override String ToString()
      {
         var retVal = this.Identifier + SEPARATOR + VERSION_PREFIX + this.Version;
         if ( !String.IsNullOrEmpty( this.Profile ) )
         {
            retVal += SEPARATOR + PROFILE_PREFIX + this.Profile;
         }
         return retVal;
      }

      /// <summary>
      /// Checks that given <see cref="TargetFrameworkInfo"/> is same instance or contains same data as this <see cref="TargetFrameworkInfo"/>.
      /// </summary>
      /// <param name="other">The other <see cref="TargetFrameworkInfo"/>.</param>
      /// <returns><c>true</c> if <paramref name="other"/> is same instance or contains same data as this <see cref="TargetFrameworkInfo"/>; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// The values of <see cref="Identifier"/>, <see cref="Version"/>, and <see cref="Profile"/> are all compared in exact, case-sensitive manner.
      /// </remarks>
      public Boolean Equals( TargetFrameworkInfo other )
      {
         return ReferenceEquals( this, other )
            || ( other != null
            && String.Equals( this.Identifier, other.Identifier )
            && String.Equals( this.Version, other.Version )
            && String.Equals( this.Profile, other.Profile )
            );
      }


      private const String PROFILE_PREFIX = "Profile=";
      private const String VERSION_PREFIX = "Version=";
      private const Char SEPARATOR = ',';

      /// <summary>
      /// Parses the given string into <see cref="TargetFrameworkInfo"/>, or throws an exception if parsing could not be done.
      /// </summary>
      /// <param name="str">The string to parse.</param>
      /// <returns>The parsed <see cref="TargetFrameworkInfo"/>.</returns>
      /// <remarks>
      /// The string should be in same format as returned by <see cref="ToString"/> method.
      /// </remarks>
      /// <exception cref="FormatException">If <paramref name="str"/> could not be parsed into a <see cref="TargetFrameworkInfo"/>. This can happen if <paramref name="str"/> is <c>null</c>, empty, or contains only whitespace characters.</exception>
      public static TargetFrameworkInfo Parse( String str )
      {
         String errorString;
         TargetFrameworkInfo retVal;
         if ( !TryParse( str, out retVal, out errorString ) )
         {
            throw new FormatException( "Failed to parse target framework string, reason: " + errorString + "." );
         }
         return retVal;
      }

      /// <summary>
      /// Tries to parse given string into <see cref="TargetFrameworkInfo"/>.
      /// </summary>
      /// <param name="str">The string to parse.</param>
      /// <param name="fwInfo">This parameter will contain the parsed <see cref="TargetFrameworkInfo"/>, if parsing is successful.</param>
      /// <returns><c>true</c> if parsing is successful; <c>false</c> otherwise.</returns>
      public static Boolean TryParse( String str, out TargetFrameworkInfo fwInfo )
      {
         String errorString;
         return TryParse( str, out fwInfo, out errorString );
      }

      private static Boolean TryParse( String str, out TargetFrameworkInfo fwInfo, out String errorString )
      {
         var retVal = !String.IsNullOrEmpty( str );
         if ( retVal )
         {
            str = str.Trim();
            if ( str.Length > 0 )
            {
               // First, framework name
               var idx = str.IndexOf( SEPARATOR );
               var fwName = idx == -1 ? str : str.Substring( 0, idx );

               String fwVersion = null, fwProfile = null;
               if ( idx > 0 )
               {

                  // Then, framework version
                  idx = str.IndexOf( VERSION_PREFIX, idx, StringComparison.Ordinal );
                  var nextIdx = idx + VERSION_PREFIX.Length;
                  var endIdx = str.IndexOf( SEPARATOR, nextIdx );
                  if ( endIdx == -1 )
                  {
                     endIdx = str.Length;
                  }
                  fwVersion = idx != -1 && nextIdx < str.Length ? str.Substring( nextIdx, endIdx - nextIdx ) : null;

                  // Then, profile
                  if ( idx > 0 )
                  {
                     idx = str.IndexOf( PROFILE_PREFIX, idx, StringComparison.Ordinal );
                     nextIdx = idx + PROFILE_PREFIX.Length;
                     endIdx = str.IndexOf( SEPARATOR, nextIdx );
                     if ( endIdx == -1 )
                     {
                        endIdx = str.Length;
                     }
                     fwProfile = idx != -1 && nextIdx < str.Length ? str.Substring( nextIdx, endIdx - nextIdx ) : null;
                  }
               }

               fwInfo = new TargetFrameworkInfo( fwName, fwVersion, fwProfile );
               errorString = null;
            }
            else
            {
               fwInfo = null;
               errorString = "String contained only whitespace characters";
            }
         }
         else
         {
            fwInfo = null;
            errorString = "Input string was null or empty";
         }

         return retVal;
      }
   }

   /// <summary>
   /// This class encapsulates the information about target framework information (<see cref="Physical.TargetFrameworkInfo"/>) and whether the assembly references to that framework should be marked with <see cref="AssemblyFlags.Retargetable"/> flag.
   /// </summary>
   public sealed class TargetFrameworkInfoWithRetargetabilityInformation
   {
      /// <summary>
      /// Creates a new instance of <see cref="TargetFrameworkInfoWithRetargetabilityInformation"/> with given <see cref="Physical.TargetFrameworkInfo"/> and whether the assembly references to this target framework are retargetable.
      /// </summary>
      /// <param name="targetFramework">The <see cref="Physical.TargetFrameworkInfo"/> representing target framework information.</param>
      /// <param name="assemblyReferencesRetargetable">Whether the assembly references to <paramref name="targetFramework"/> should be tagged with <see cref="AssemblyFlags.Retargetable"/> flag.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="targetFramework"/> is <c>null</c>.</exception>
      public TargetFrameworkInfoWithRetargetabilityInformation(
         TargetFrameworkInfo targetFramework,
         Boolean assemblyReferencesRetargetable
         )
      {
         ArgumentValidator.ValidateNotNull( "Target framework information", targetFramework );

         this.TargetFrameworkInfo = targetFramework;
         this.AreFrameworkAssemblyReferencesRetargetable = assemblyReferencesRetargetable;
      }

      /// <summary>
      /// Gets the <see cref="Physical.TargetFrameworkInfo"/> for represented target framework.
      /// </summary>
      /// <value>The <see cref="Physical.TargetFrameworkInfo"/> for represented target framework.</value>
      public TargetFrameworkInfo TargetFrameworkInfo { get; }

      /// <summary>
      /// Gets the value indicating whether assembly references to represented target framework should be tagged with <see cref="AssemblyFlags.Retargetable"/> flag.
      /// </summary>
      /// <value>The value indicating whether assembly references to represented target framework should be tagged with <see cref="AssemblyFlags.Retargetable"/> flag.</value>
      public Boolean AreFrameworkAssemblyReferencesRetargetable { get; }
   }
}

public static partial class E_CILPhysical
{
   /// <summary>
   /// Checks whether the method is eligible to have method body. See ECMA specification (condition 33 for MethodDef table) for exact condition of methods having method bodies. In addition to that, the <see cref="E_CILPhysical.IsIL"/> must return <c>true</c>.
   /// </summary>
   /// <param name="method">The method to check.</param>
   /// <returns><c>true</c> if the <paramref name="method"/> is non-<c>null</c> and can have IL method body; <c>false</c> otherwise.</returns>
   /// <seealso cref="E_CILPhysical.IsIL"/>
   /// <seealso cref="E_CILPhysical.CanEmitIL"/>
   public static Boolean ShouldHaveMethodBody( this MethodDefinition method )
   {
      return method != null && method.Attributes.CanEmitIL() && method.ImplementationAttributes.IsIL();
   }

   /// <summary>
   /// Changes the index of given <see cref="TableIndex"/>, leaving the table the same.
   /// </summary>
   /// <param name="index">The <see cref="TableIndex"/>.</param>
   /// <param name="newIndex">The new index of the same table.</param>
   /// <returns>A new <see cref="TableIndex"/> with the same table as <paramref name="index"/>, but with index set to <paramref name="newIndex"/>.</returns>
   public static TableIndex ChangeIndex( this TableIndex index, Int32 newIndex )
   {
      return new TableIndex( index.Table, newIndex );
   }

   /// <summary>
   /// Increments the index of given <see cref="TableIndex"/>, optionally specifying by how much to increment.
   /// </summary>
   /// <param name="index">The <see cref="TableIndex"/>.</param>
   /// <param name="amount">The amount to increment. Defaults to <c>1</c>.</param>
   /// <returns>A new <see cref="TableIndex"/> with the same table as <paramref name="index"/>, but with index incremented by given <paramref name="amount"/>.</returns>
   public static TableIndex IncrementIndex( this TableIndex index, Int32 amount = 1 )
   {
      return index.ChangeIndex( index.Index + amount );
   }

   //public static Boolean IsSimpleTypeOfKind( this CustomAttributeArgumentType caType, SignatureElementTypes typeKind )
   //{
   //   return caType.ArgumentTypeKind == CustomAttributeArgumentTypeKind.Simple
   //      && ( (CustomAttributeArgumentTypeSimple) caType ).SimpleType == typeKind;
   //}

   //public static Boolean CanBeReferencedFromIL( this Tables table )
   //{
   //   switch ( table )
   //   {
   //      case Tables.TypeDef:
   //      case Tables.TypeRef:
   //      case Tables.TypeSpec:
   //      case Tables.MethodDef:
   //      case Tables.Field:
   //      case Tables.MemberRef:
   //      case Tables.MethodSpec:
   //      case Tables.StandaloneSignature:
   //         return true;
   //      default:
   //         return false;
   //   }
   //}

   /// <summary>
   /// Checks that this <see cref="ManifestResource"/> can be considered as embedded resource.
   /// </summary>
   /// <param name="resource">The <see cref="ManifestResource"/>.</param>
   /// <returns><c>true</c> if <paramref name="resource"/> can be considered as embedded resource; <c>false</c> otherwise.</returns>
   /// <remarks>
   /// The <see cref="ManifestResource"/> is considered to be embedded resource when it is not <c>null</c> and it's <see cref="ManifestResource.Implementation"/> propery is <c>null</c>.
   /// </remarks>
   public static Boolean IsEmbeddedResource( this ManifestResource resource )
   {
      return resource != null && !resource.Implementation.HasValue;
   }

   /// <summary>
   /// Creates a new <see cref="AssemblyReference"/>, based on this <see cref="AssemblyDefinition"/>.
   /// </summary>
   /// <param name="definition">The <see cref="AssemblyDefinition"/>.</param>
   /// <returns>A new <see cref="AssemblyReference"/> with its <see cref="AssemblyReference.Attributes"/> having <see cref="AssemblyFlags.PublicKey"/> set, if this <see cref="AssemblyDefinition"/> its <see cref="AssemblyInformation.PublicKeyOrToken"/> not null and not empty.</returns>
   /// <remarks>
   /// All the contents of <see cref="AssemblyDefinition"/> is copied so that any subsequent modification to tis <see cref="AssemblyDefinition"/> will not affect the returned <see cref="AssemblyReference"/>.
   /// </remarks>
   public static AssemblyReference AsAssemblyReference( this AssemblyDefinition definition )
   {
      var retVal = new AssemblyReference()
      {
         Attributes = definition.AssemblyInformation.PublicKeyOrToken.IsNullOrEmpty() ? AssemblyFlags.None : AssemblyFlags.PublicKey
      };

      definition.AssemblyInformation.DeepCopyContentsTo( retVal.AssemblyInformation );

      return retVal;

   }
}
