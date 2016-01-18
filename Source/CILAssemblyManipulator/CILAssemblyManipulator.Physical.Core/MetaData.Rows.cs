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
      /// The schema for this table index corresponds to the <see cref="TableIndexSchemas.ResolutionScope"/> schema.
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
      /// The schema for this table index corresponds to the <see cref="TableIndexSchemas.TypeDefOrRef"/> schema.
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
      /// The schema for this table index corresponds to the <see cref="TableIndexSchemas.TypeDefOrRef"/> schema.
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
      /// The schema for this table index corresponds to the <see cref="TableIndexSchemas.MemberRefParent"/> schema.
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
      /// The schema for this table index corresponds to the <see cref="TableIndexSchemas.HasConstant"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Parent { get; set; }

      /// <summary>
      /// Gets or sets the value for this <see cref="ConstantDefinition"/>.
      /// May be <c>null</c>.
      /// </summary>
      /// <value>The value for this <see cref="ConstantDefinition"/>.</value>
      /// <remarks>
      /// If this property is <c>null</c>, then <see cref="Type"/> property should be <see cref="SignatureElementTypes.Class"/>.
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
      Boolean = 0x02, // Same as SignatureElementTypes.Boolean
      Char,
      I1,
      U1,
      I2,
      U2,
      I4,
      U4,
      I8,
      U8,
      R4,
      R8,
      String,
      Class = 0x12, // Same as SignatureElementTypes.Class
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
      /// The schema for this table index corresponds to the <see cref="TableIndexSchemas.HasCustomAttribute"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Parent { get; set; }

      /// <summary>
      /// Gets or sets the constructor used to create instance of custom attribute.
      /// </summary>
      /// <value>The constructor used to create instance of custom attribute.</value>
      /// <remarks>
      /// The schema for this table index corresponds to the <see cref="TableIndexSchemas.CustomAttributeType"/> schema.
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
      /// The schema for this table index corresponds to the <see cref="TableIndexSchemas.HasFieldMarshal"/> schema.
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
      private readonly List<AbstractSecurityInformation> _permissionSets;

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
         this._permissionSets = new List<AbstractSecurityInformation>( permissionSetsCount );
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
      /// The schema for this table index corresponds to the <see cref="TableIndexSchemas.HasSecurity"/> schema.
      /// </remarks>
      /// <seealso cref="TableIndex"/>
      public TableIndex Parent { get; set; }

      /// <summary>
      /// Gets the list of <see cref="AbstractSecurityInformation"/>s that this <see cref="SecurityDefinition"/> has.
      /// </summary>
      /// <value>The list of <see cref="AbstractSecurityInformation"/>s that this <see cref="SecurityDefinition"/> has.</value>
      /// <seealso cref="AbstractSecurityInformation"/>
      public List<AbstractSecurityInformation> PermissionSets
      {
         get
         {
            return this._permissionSets;
         }
      }
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
      /// Gets or sets the indicator, whether the <see cref="Signature"/> should be serialized with <see cref="MethodSignatureInformation.Field"/> prefix.
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
      /// The schema for this table index corresponds to the <see cref="TableIndexSchemas.TypeDefOrRef"/> schema.
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
      /// The schema for this table index corresponds to the <see cref="TableIndexSchemas.HasSemantics"/> schema.
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
      /// <param name="methodDefIndex">The zero-based index for <see cref="Class"/>.</param>
      public MethodImplementation( Int32 typeDefIndex )
      {
         this.Class = new TableIndex( Tables.TypeDef, typeDefIndex );
      }

      public TableIndex Class { get; set; }
      public TableIndex MethodBody { get; set; }
      public TableIndex MethodDeclaration { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.ModuleReferences"/> table at table index <see cref="Tables.ModuleRef"/>.
   /// </summary>
   public sealed class ModuleReference
   {
      public String ModuleName { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.TypeSpecifications"/> table at table index <see cref="Tables.TypeSpec"/>.
   /// </summary>
   public sealed class TypeSpecification
   {
      public TypeSignature Signature { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.MethodImplementationMaps"/> table at table index <see cref="Tables.ImplMap"/>.
   /// </summary>
   public sealed class MethodImplementationMap
   {
      public MethodImplementationMap()
      {
         this.ImportScope = new TableIndex( Tables.ModuleRef, 0 );
      }

      public PInvokeAttributes Attributes { get; set; }
      public TableIndex MemberForwarded { get; set; }
      public String ImportName { get; set; }
      public TableIndex ImportScope { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.FieldRVAs"/> table at table index <see cref="Tables.FieldRVA"/>.
   /// </summary>
   public sealed class FieldRVA
   {
      public FieldRVA()
      {
         this.Field = new TableIndex( Tables.Field, 0 );
      }

      public Byte[] Data { get; set; }
      public TableIndex Field { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.AssemblyDefinitions"/> table at table index <see cref="Tables.Assembly"/>.
   /// </summary>
   public sealed class AssemblyDefinition
   {
      private readonly AssemblyInformation _assemblyInfo;

      public AssemblyDefinition()
      {
         this._assemblyInfo = new AssemblyInformation();
      }

      public AssemblyFlags Attributes { get; set; }

      public AssemblyInformation AssemblyInformation
      {
         get
         {
            return this._assemblyInfo;
         }
      }

      public AssemblyHashAlgorithm HashAlgorithm { get; set; }

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
      private readonly AssemblyInformation _assemblyInfo;

      public AssemblyReference()
      {
         this._assemblyInfo = new AssemblyInformation();
      }

      public AssemblyFlags Attributes { get; set; }

      public AssemblyInformation AssemblyInformation
      {
         get
         {
            return this._assemblyInfo;
         }
      }

      public Byte[] HashValue { get; set; }

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
      public FileAttributes Attributes { get; set; }
      public String Name { get; set; }
      public Byte[] HashValue { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.ExportedTypes"/> table at table index <see cref="Tables.ExportedType"/>.
   /// </summary>
   public sealed class ExportedType
   {
      public TypeAttributes Attributes { get; set; }
      public Int32 TypeDefinitionIndex { get; set; }
      public String Name { get; set; }
      public String Namespace { get; set; }
      public TableIndex Implementation { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.ManifestResources"/> table at table index <see cref="Tables.ManifestResource"/>.
   /// </summary>
   public sealed class ManifestResource
   {
      /// <summary>
      /// This value is interpreted as unsigned 4-byte integer.
      /// </summary>
      public Int32 Offset { get; set; }
      public ManifestResourceAttributes Attributes { get; set; }
      public String Name { get; set; }
      public TableIndex? Implementation { get; set; }

      // This will be used only if Implementation is null
      public Byte[] DataInCurrentFile { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.NestedClassDefinitions"/> table at table index <see cref="Tables.NestedClass"/>.
   /// </summary>
   public sealed class NestedClassDefinition
   {
      public NestedClassDefinition()
      {
         this.NestedClass = new TableIndex( Tables.TypeDef, 0 );
         this.EnclosingClass = new TableIndex( Tables.TypeDef, 0 );
      }

      public TableIndex NestedClass { get; set; }
      public TableIndex EnclosingClass { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.GenericParameterDefinitions"/> table at table index <see cref="Tables.GenericParameter"/>.
   /// </summary>
   public sealed class GenericParameterDefinition
   {
      public Int32 GenericParameterIndex { get; set; }
      public GenericParameterAttributes Attributes { get; set; }
      public TableIndex Owner { get; set; }
      public String Name { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.MethodSpecifications"/> table at table index <see cref="Tables.MethodSpec"/>.
   /// </summary>
   public sealed class MethodSpecification
   {
      public TableIndex Method { get; set; }
      public GenericMethodSignature Signature { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.GenericParameterConstraintDefinitions"/> table at table index <see cref="Tables.GenericParameterConstraint"/>.
   /// </summary>
   public sealed class GenericParameterConstraintDefinition
   {
      public GenericParameterConstraintDefinition()
      {
         this.Owner = new TableIndex( Tables.GenericParameter, 0 );
      }

      public TableIndex Owner { get; set; }
      public TableIndex Constraint { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.EditAndContinueLog"/> table at table index <see cref="Tables.EncLog"/>.
   /// </summary>
   public sealed class EditAndContinueLog
   {
      public Int32 Token { get; set; }
      public Int32 FuncCode { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.EditAndContinueMap"/> table at table index <see cref="Tables.EncMap"/>.
   /// </summary>
   public sealed class EditAndContinueMap
   {
      public Int32 Token { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.FieldDefinitionPointers"/> table at table index <see cref="Tables.FieldPtr"/>.
   /// </summary>
   public sealed class FieldDefinitionPointer
   {
      public TableIndex FieldIndex { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.MethodDefinitionPointers"/> table at table index <see cref="Tables.MethodPtr"/>.
   /// </summary>
   public sealed class MethodDefinitionPointer
   {
      public TableIndex MethodIndex { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.ParameterDefinitionPointers"/> table at table index <see cref="Tables.ParameterPtr"/>.
   /// </summary>
   public sealed class ParameterDefinitionPointer
   {
      public TableIndex ParameterIndex { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.EventDefinitionPointers"/> table at table index <see cref="Tables.EventPtr"/>.
   /// </summary>
   public sealed class EventDefinitionPointer
   {
      public TableIndex EventIndex { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.PropertyDefinitionPointers"/> table at table index <see cref="Tables.PropertyPtr"/>.
   /// </summary>
   public sealed class PropertyDefinitionPointer
   {
      public TableIndex PropertyIndex { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.AssemblyDefinitionProcessors"/> table at table index <see cref="Tables.AssemblyProcessor"/>.
   /// </summary>
   [Obsolete( "Rows of these type should no longer be present in CIL meta data file.", false )]
   public sealed class AssemblyDefinitionProcessor
   {
      public Int32 Processor { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.AssemblyDefinitionOSs"/> table at table index <see cref="Tables.AssemblyOS"/>.
   /// </summary>
   [Obsolete( "Rows of these type should no longer be present in CIL meta data file.", false )]
   public sealed class AssemblyDefinitionOS
   {
      public Int32 OSPlatformID { get; set; }

      public Int32 OSMajorVersion { get; set; }

      public Int32 OSMinorVersion { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.AssemblyReferenceProcessors"/> table at table index <see cref="Tables.AssemblyRefProcessor"/>.
   /// </summary>
   [Obsolete( "Rows of these type should no longer be present in CIL meta data file.", false )]
   public sealed class AssemblyReferenceProcessor
   {
      public Int32 Processor { get; set; }

      public TableIndex AssemblyRef { get; set; }
   }

   /// <summary>
   /// This is type for rows of <see cref="CILMetaData.AssemblyReferenceOSs"/> table at table index <see cref="Tables.AssemblyRefOS"/>.
   /// </summary>
   [Obsolete( "Rows of these type should no longer be present in CIL meta data file.", false )]
   public sealed class AssemblyReferenceOS
   {
      public Int32 OSPlatformID { get; set; }

      public Int32 OSMajorVersion { get; set; }

      public Int32 OSMinorVersion { get; set; }

      public TableIndex AssemblyRef { get; set; }

   }

   public struct TableIndex : IEquatable<TableIndex>, IComparable<TableIndex>, IComparable
   {
      private readonly Int32 _token;

      // index is zero-based
      public TableIndex( Tables aTable, Int32 anIdx )
         : this( ( (Int32) aTable << 24 ) | anIdx )
      {
      }

      private TableIndex( Int32 token )
      {
         this._token = token;
      }

      public Tables Table
      {
         get
         {
            return (Tables) ( this._token >> 24 );
         }
      }

      /// <summary>
      /// This index is zero-based.
      /// </summary>
      public Int32 Index
      {
         get
         {
            return this._token & CAMCoreInternals.INDEX_MASK;
         }
      }

      public Int32 ZeroBasedToken
      {
         get
         {
            return this._token;
         }
      }

      public Int32 OneBasedToken
      {
         get
         {
            return ( ( this._token & CAMCoreInternals.INDEX_MASK ) + 1 ) | ( this._token & ~CAMCoreInternals.INDEX_MASK );
         }
      }

      public override Boolean Equals( Object obj )
      {
         return obj is TableIndex && this.Equals( (TableIndex) obj );
      }

      public override Int32 GetHashCode()
      {
         return this._token;
      }

      public Boolean Equals( TableIndex other )
      {
         return this._token == other._token;
      }

      public override String ToString()
      {
         return this.Table + "[" + this.Index + "]";
      }

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

      public static Boolean operator ==( TableIndex x, TableIndex y )
      {
         return x.Equals( y );
      }

      public static Boolean operator !=( TableIndex x, TableIndex y )
      {
         return !( x == y );
      }

      public static Boolean operator <( TableIndex x, TableIndex y )
      {
         return x.CompareTo( y ) < 0;
      }

      public static Boolean operator >( TableIndex x, TableIndex y )
      {
         return x.CompareTo( y ) > 0;
      }

      public static Boolean operator <=( TableIndex x, TableIndex y )
      {
         return !( x > y );
      }

      public static Boolean operator >=( TableIndex x, TableIndex y )
      {
         return !( x < y );
      }

      public static TableIndex? FromOneBasedTokenNullable( Int32 token )
      {
         return token == 0 ?
            (TableIndex?) null :
            FromOneBasedToken( token );
      }

      public static TableIndex FromOneBasedToken( Int32 token )
      {
         return new TableIndex( ( ( token & CAMCoreInternals.INDEX_MASK ) - 1 ) | ( token & ~CAMCoreInternals.INDEX_MASK ) );
      }

      public static TableIndex FromZeroBasedToken( Int32 token )
      {
         return new TableIndex( token );
      }
   }

   public enum Tables : byte
   {
      Assembly = 0x20,
      AssemblyOS = 0x22,
      AssemblyProcessor = 0x21,
      AssemblyRef = 0x23,
      AssemblyRefOS = 0x25,
      AssemblyRefProcessor = 0x24,
      ClassLayout = 0x0F,
      Constant = 0x0B,
      CustomAttribute = 0x0C,
      DeclSecurity = 0x0E,
      EncLog = 0x1E,
      EncMap = 0x1F,
      EventMap = 0x12,
      Event = 0x14,
      EventPtr = 0x13,
      ExportedType = 0x27,
      Field = 0x04,
      FieldLayout = 0x10,
      FieldMarshal = 0x0D,
      FieldPtr = 0x03,
      FieldRVA = 0x1D,
      File = 0x26,
      GenericParameter = 0x2A,
      GenericParameterConstraint = 0x2C,
      ImplMap = 0x1C,
      InterfaceImpl = 0x09,
      ManifestResource = 0x28,
      MemberRef = 0x0A,
      MethodDef = 0x06,
      MethodImpl = 0x19,
      MethodPtr = 0x05,
      MethodSemantics = 0x18,
      MethodSpec = 0x2B,
      Module = 0x00,
      ModuleRef = 0x1A,
      NestedClass = 0x29,
      Parameter = 0x08,
      ParameterPtr = 0x07,
      Property = 0x17,
      PropertyPtr = 0x16,
      PropertyMap = 0x15,
      StandaloneSignature = 0x11,
      TypeDef = 0x02,
      TypeRef = 0x01,
      TypeSpec = 0x1B
   }

   // System.Runtime.Versioning.FrameworkName is amazingly missing from all PCL framework assemblies.
   public sealed class TargetFrameworkInfo : IEquatable<TargetFrameworkInfo>
   {
      private readonly String _fwName;
      private readonly String _fwVersion;
      private readonly String _fwProfile;
      private readonly Boolean _assemblyRefsRetargetable;

      public TargetFrameworkInfo( String name, String version, String profile )
      {
         this._fwName = name;
         this._fwVersion = version;
         this._fwProfile = profile;
         // TODO better
         this._assemblyRefsRetargetable = String.Equals( this._fwName, ".NETPortable" );
      }

      public String Identifier
      {
         get
         {
            return this._fwName;
         }
      }

      public String Version
      {
         get
         {
            return this._fwVersion;
         }
      }

      public String Profile
      {
         get
         {
            return this._fwProfile;
         }
      }

      public Boolean AreFrameworkAssemblyReferencesRetargetable
      {
         get
         {
            return this._assemblyRefsRetargetable;
         }
      }

      public override Boolean Equals( Object obj )
      {
         return this.Equals( obj as TargetFrameworkInfo );
      }

      public override Int32 GetHashCode()
      {
         return ( ( 17 * 23 + this._fwName.GetHashCodeSafe() ) * 23 + this._fwVersion.GetHashCodeSafe() ) * 23 + this._fwProfile.GetHashCodeSafe();
      }

      public override String ToString()
      {
         var retVal = this._fwName + SEPARATOR + VERSION_PREFIX + this._fwVersion;
         if ( !String.IsNullOrEmpty( this._fwProfile ) )
         {
            retVal += SEPARATOR + PROFILE_PREFIX + this._fwProfile;
         }
         return retVal;
      }

      public Boolean Equals( TargetFrameworkInfo other )
      {
         return ReferenceEquals( this, other )
            || ( other != null
            && String.Equals( this._fwName, other._fwName )
            && String.Equals( this._fwVersion, other._fwVersion )
            && String.Equals( this._fwProfile, other._fwProfile )
            && this._assemblyRefsRetargetable == other._assemblyRefsRetargetable
            );
      }


      private const String PROFILE_PREFIX = "Profile=";
      private const String VERSION_PREFIX = "Version=";
      private const Char SEPARATOR = ',';

      public static Boolean TryParse( String str, out TargetFrameworkInfo fwInfo )
      {
         var retVal = !String.IsNullOrEmpty( str );
         if ( retVal )
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
         }
         else
         {
            fwInfo = null;
         }

         return retVal;
      }
   }
}

public static partial class E_CILPhysical
{
   /// <summary>
   /// Checks whether the method is eligible to have method body. See ECMA specification (condition 33 for MethodDef table) for exact condition of methods having method bodies. In addition to that, the <see cref="E_CIL.IsIL"/> must return <c>true</c>.
   /// </summary>
   /// <param name="method">The method to check.</param>
   /// <returns><c>true</c> if the <paramref name="method"/> is non-<c>null</c> and can have IL method body; <c>false</c> otherwise.</returns>
   /// <seealso cref="E_CIL.IsIL"/>
   /// <seealso cref="E_CIL.CanEmitIL"/>
   public static Boolean ShouldHaveMethodBody( this MethodDefinition method )
   {
      return method != null && method.Attributes.CanEmitIL() && method.ImplementationAttributes.IsIL();
   }

   public static TableIndex ChangeIndex( this TableIndex index, Int32 newIndex )
   {
      return new TableIndex( index.Table, newIndex );
   }

   public static TableIndex IncrementIndex( this TableIndex index, Int32 amount = 1 )
   {
      return index.ChangeIndex( index.Index + amount );
   }

   //public static Boolean IsSimpleTypeOfKind( this CustomAttributeArgumentType caType, SignatureElementTypes typeKind )
   //{
   //   return caType.ArgumentTypeKind == CustomAttributeArgumentTypeKind.Simple
   //      && ( (CustomAttributeArgumentTypeSimple) caType ).SimpleType == typeKind;
   //}

   public static Boolean CanBeReferencedFromIL( this Tables table )
   {
      switch ( table )
      {
         case Tables.TypeDef:
         case Tables.TypeRef:
         case Tables.TypeSpec:
         case Tables.MethodDef:
         case Tables.Field:
         case Tables.MemberRef:
         case Tables.MethodSpec:
         case Tables.StandaloneSignature:
            return true;
         default:
            return false;
      }
   }

   public static Boolean IsEmbeddedResource( this ManifestResource resource )
   {
      return resource != null && !resource.Implementation.HasValue;
   }

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
