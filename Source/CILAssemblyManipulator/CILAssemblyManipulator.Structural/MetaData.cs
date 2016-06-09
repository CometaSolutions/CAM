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
using CILAssemblyManipulator.Physical;
using UtilPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Structural
{
   public abstract class StructureWithCustomAttributes
   {
      private readonly List<CustomAttributeStructure> _customAttributes;

      internal StructureWithCustomAttributes()
      {
         this._customAttributes = new List<CustomAttributeStructure>();
      }

      public List<CustomAttributeStructure> CustomAttributes
      {
         get
         {
            return this._customAttributes;
         }
      }
   }

   public sealed class CustomAttributeStructure
   {
      public MethodDefOrMemberRefStructure Constructor { get; set; }
      public AbstractCustomAttributeSignature Signature { get; set; }
   }

   public sealed class AssemblyStructure : StructureWithCustomAttributes
   {
      private readonly AssemblyInformation _assemblyInfo;
      private readonly List<ModuleStructure> _modules;
      private readonly List<SecurityStructure> _security;

      public AssemblyStructure()
      {
         this._modules = new List<ModuleStructure>();
         this._assemblyInfo = new AssemblyInformation();
         this._security = new List<SecurityStructure>();
      }

      internal AssemblyStructure( CILMetaData md )
         : this()
      {
         ArgumentValidator.ValidateNotNull( "Meta data", md );

         var aDefs = md.AssemblyDefinitions.TableContents;
         if ( aDefs.Count <= 0 )
         {
            throw new InvalidOperationException( "Given physical meta data does not have assembly definitions." );
         }

         var aDef = aDefs[0];
         var aInfo = this._assemblyInfo;
         aDef.AssemblyInformation.DeepCopyContentsTo( aInfo );
         this.Attributes = aDef.Attributes;
         this.HashAlgorithm = aDef.HashAlgorithm;
      }

      public AssemblyInformation AssemblyInfo
      {
         get
         {
            return this._assemblyInfo;
         }
      }

      public AssemblyFlags Attributes { get; set; }
      public AssemblyHashAlgorithm HashAlgorithm { get; set; }
      public List<SecurityStructure> SecurityInfo
      {
         get
         {
            return this._security;
         }
      }
      public List<ModuleStructure> Modules
      {
         get
         {
            return this._modules;
         }
      }
   }

   public sealed class ModuleStructure : StructureWithCustomAttributes
   {
      private readonly List<TypeDefinitionStructure> _topLevelTypeDefs;
      private readonly List<ExportedTypeStructure> _exportedTypes;
      private readonly List<ManifestResourceStructure> _resources;

      public ModuleStructure( Int32 typeDefCount = 0, Int32 exportedTypeCound = 0, Int32 resourceCount = 0 )
      {
         this._topLevelTypeDefs = new List<TypeDefinitionStructure>( typeDefCount );
         this._exportedTypes = new List<ExportedTypeStructure>( exportedTypeCound );
         this._resources = new List<ManifestResourceStructure>( resourceCount );
      }

      internal ModuleStructure( CILMetaData md )
         : this( md.TypeDefinitions.GetRowCount(), md.ExportedTypes.GetRowCount(), md.ManifestResources.GetRowCount() )
      {
         this.Name = md.ModuleDefinitions.TableContents[0].Name;
         this.IsMainModule = md.AssemblyDefinitions.GetRowCount() > 0;
      }

      public String Name { get; set; }
      public Boolean IsMainModule { get; set; }

      public List<TypeDefinitionStructure> TopLevelTypeDefinitions
      {
         get
         {
            return this._topLevelTypeDefs;
         }
      }
      public List<ExportedTypeStructure> ExportedTypes
      {
         get
         {
            return this._exportedTypes;
         }
      }

      public List<ManifestResourceStructure> ManifestResources
      {
         get
         {
            return this._resources;
         }
      }
   }

   public enum TypeStructureKind
   {
      TypeDef,
      TypeRef,
      TypeSpec
   }

   public abstract class AbstractTypeStructure : StructureWithCustomAttributes, StructurePresentInIL
   {
      internal AbstractTypeStructure()
      {

      }

      public abstract TypeStructureKind TypeStructureKind { get; }

      public abstract OpCodeStructureTokenKind StructureTokenKind { get; }
   }

   public sealed class LayoutInfo : IEquatable<LayoutInfo>
   {
      public Int32 ClassSize { get; set; }
      public Int32 PackingSize { get; set; }

      public override Boolean Equals( Object obj )
      {
         return this.Equals( obj as LayoutInfo );
      }

      public override Int32 GetHashCode()
      {
         return ( 17 * 23 + this.ClassSize ) * 23 + this.PackingSize;
      }

      public Boolean Equals( LayoutInfo other )
      {
         return ReferenceEquals( this, other )
            || ( other != null
            && this.ClassSize == other.ClassSize
            && this.PackingSize == other.PackingSize
            );
      }
   }

   public struct OverriddenMethodInfo
   {
      private readonly MethodDefOrMemberRefStructure _methodBody;
      private readonly MethodDefOrMemberRefStructure _methodDeclaration;

      public OverriddenMethodInfo( MethodDefOrMemberRefStructure methodBody, MethodDefOrMemberRefStructure methodDeclaration )
      {
         this._methodBody = methodBody;
         this._methodDeclaration = methodDeclaration;
      }

      public MethodDefOrMemberRefStructure MethodBody
      {
         get
         {
            return this._methodBody;
         }
      }

      public MethodDefOrMemberRefStructure MethodDeclaration
      {
         get
         {
            return this._methodDeclaration;
         }
      }
   }

   public sealed class TypeDefinitionStructure : AbstractTypeStructure
   {
      private readonly List<TypeDefinitionStructure> _nestedTypes;
      private readonly List<FieldStructure> _fields;
      private readonly List<MethodStructure> _methods;
      private readonly List<PropertyStructure> _properties;
      private readonly List<EventStructure> _events;

      private readonly List<InterfaceImplStructure> _interfaces;
      private readonly List<GenericParameterStructure> _genericParameters;
      private readonly List<OverriddenMethodInfo> _overriddenMethods;
      private readonly List<SecurityStructure> _security;

      public TypeDefinitionStructure()
      {
         this._nestedTypes = new List<TypeDefinitionStructure>();
         this._fields = new List<FieldStructure>();
         this._methods = new List<MethodStructure>();
         this._properties = new List<PropertyStructure>();
         this._events = new List<EventStructure>();
         this._interfaces = new List<InterfaceImplStructure>();
         this._genericParameters = new List<GenericParameterStructure>();
         this._overriddenMethods = new List<OverriddenMethodInfo>();
         this._security = new List<SecurityStructure>();
      }

      public TypeDefinitionStructure( TypeDefinition tDef )
         : this()
      {
         this.Namespace = tDef.Namespace;
         this.Name = tDef.Name;
         this.Attributes = tDef.Attributes;
      }

      public override TypeStructureKind TypeStructureKind
      {
         get
         {
            return TypeStructureKind.TypeDef;
         }
      }

      public override OpCodeStructureTokenKind StructureTokenKind
      {
         get
         {
            return OpCodeStructureTokenKind.TypeDef;
         }
      }

      public String Name { get; set; }
      public String Namespace { get; set; }
      public TypeAttributes Attributes { get; set; }
      public AbstractTypeStructure BaseType { get; set; }
      public LayoutInfo Layout { get; set; }
      public List<SecurityStructure> SecurityInfo
      {
         get
         {
            return this._security;
         }
      }

      public List<TypeDefinitionStructure> NestedTypes
      {
         get
         {
            return this._nestedTypes;
         }
      }

      public List<FieldStructure> Fields
      {
         get
         {
            return this._fields;
         }
      }

      public List<MethodStructure> Methods
      {
         get
         {
            return this._methods;
         }
      }

      public List<PropertyStructure> Properties
      {
         get
         {
            return this._properties;
         }
      }

      public List<EventStructure> Events
      {
         get
         {
            return this._events;
         }
      }

      public List<GenericParameterStructure> GenericParameters
      {
         get
         {
            return this._genericParameters;
         }
      }

      public List<InterfaceImplStructure> ImplementedInterfaces
      {
         get
         {
            return this._interfaces;
         }
      }

      public List<OverriddenMethodInfo> OverriddenMethods
      {
         get
         {
            return this._overriddenMethods;
         }
      }

      public override String ToString()
      {
         return Miscellaneous.CombineNamespaceAndType( this.Namespace, this.Name );
      }
   }

   public sealed class TypeReferenceStructure : AbstractTypeStructure
   {
      public TypeReferenceStructure()
      {

      }

      public TypeReferenceStructure( TypeReference tRef )
         : this()
      {
         this.Name = tRef.Name;
         this.Namespace = tRef.Namespace;
      }

      public String Name { get; set; }
      public String Namespace { get; set; }
      public TypeReferenceResolutionScope ResolutionScope { get; set; }

      public override TypeStructureKind TypeStructureKind
      {
         get
         {
            return TypeStructureKind.TypeRef;
         }
      }

      public override OpCodeStructureTokenKind StructureTokenKind
      {
         get
         {
            return OpCodeStructureTokenKind.TypeRef;
         }
      }

      public override String ToString()
      {
         return Miscellaneous.CombineNamespaceAndType( this.Namespace, this.Name );
      }
   }

   public enum TypeRefResolutionScopeKind
   {
      Nested,
      ModuleRef,
      AssemblyRef,
      TypeDef,
      ExportedType
   }

   public abstract class TypeReferenceResolutionScope
   {
      internal TypeReferenceResolutionScope()
      {

      }

      public abstract TypeRefResolutionScopeKind ResolutionScopeKind { get; }
   }

   public sealed class TypeReferenceResolutionScopeTypeDef : TypeReferenceResolutionScope
   {
      public TypeReferenceResolutionScopeTypeDef()
      {

      }

      public TypeDefinitionStructure TypeDef { get; set; }

      public override TypeRefResolutionScopeKind ResolutionScopeKind
      {
         get
         {
            return TypeRefResolutionScopeKind.TypeDef;
         }
      }
   }

   public sealed class TypeReferenceResolutionScopeNested : TypeReferenceResolutionScope
   {
      public TypeReferenceResolutionScopeNested()
      {

      }

      public override TypeRefResolutionScopeKind ResolutionScopeKind
      {
         get
         {
            return TypeRefResolutionScopeKind.Nested;
         }
      }

      public TypeReferenceStructure EnclosingTypeRef { get; set; }
   }

   public sealed class TypeReferenceResolutionScopeModuleRef : TypeReferenceResolutionScope
   {
      public TypeReferenceResolutionScopeModuleRef()
      {

      }

      public override TypeRefResolutionScopeKind ResolutionScopeKind
      {
         get
         {
            return TypeRefResolutionScopeKind.ModuleRef;
         }
      }

      public ModuleReferenceStructure ModuleRef { get; set; }
   }

   public sealed class TypeReferenceResolutionScopeAssemblyRef : TypeReferenceResolutionScope
   {
      public TypeReferenceResolutionScopeAssemblyRef()
      {

      }

      public override TypeRefResolutionScopeKind ResolutionScopeKind
      {
         get
         {
            return TypeRefResolutionScopeKind.AssemblyRef;
         }
      }

      public AssemblyReferenceStructure AssemblyRef { get; set; }
   }

   public sealed class TypeReferenceResolutionScopeExportedType : TypeReferenceResolutionScope
   {

      public ExportedTypeStructure ExportedType { get; set; }

      public override TypeRefResolutionScopeKind ResolutionScopeKind
      {
         get
         {
            return TypeRefResolutionScopeKind.ExportedType;
         }
      }
   }

   public sealed class TypeSpecificationStructure : AbstractTypeStructure
   {
      public TypeSpecificationStructure()
      {
      }

      public override TypeStructureKind TypeStructureKind
      {
         get
         {
            return TypeStructureKind.TypeSpec;
         }
      }

      public override OpCodeStructureTokenKind StructureTokenKind
      {
         get
         {
            return OpCodeStructureTokenKind.TypeSpec;
         }
      }

      public TypeStructureSignature Signature { get; set; }
   }

   public sealed class AssemblyReferenceStructure : StructureWithCustomAttributes
   {
      public AssemblyReferenceStructure( AssemblyReference aRef )
      {
         this.AssemblyRef = new AssemblyInformation();
         aRef.AssemblyInformation.DeepCopyContentsTo( this.AssemblyRef );
         this.Attributes = aRef.Attributes;
         this.HashValue = aRef.HashValue.CreateBlockCopy();
      }

      public AssemblyInformation AssemblyRef { get; set; }
      public AssemblyFlags Attributes { get; set; }
      public Byte[] HashValue { get; set; }
   }

   public sealed class ExportedTypeStructure : StructureWithCustomAttributes
   {
      public ExportedTypeStructure()
      {

      }

      public ExportedTypeStructure( ExportedType eType )
      {
         this.Attributes = eType.Attributes;
         this.TypeDefID = eType.TypeDefinitionIndex;
         this.Name = eType.Name;
         this.Namespace = eType.Namespace;
      }

      public TypeAttributes Attributes { get; set; }
      public Int32 TypeDefID { get; set; }
      public String Name { get; set; }
      public String Namespace { get; set; }
      public ExportedTypeResolutionScope ResolutionScope { get; set; }
   }

   public enum ExportedTypeResolutionScopeKind
   {
      Nested,
      File,
      AssemblyRef
   }

   public abstract class ExportedTypeResolutionScope
   {
      internal ExportedTypeResolutionScope()
      {

      }

      public abstract ExportedTypeResolutionScopeKind ResolutionScopeKind { get; }
   }

   public sealed class ExportedTypeResolutionScopeNested : ExportedTypeResolutionScope
   {
      public ExportedTypeResolutionScopeNested()
      {

      }

      public ExportedTypeStructure EnclosingType { get; set; }

      public override ExportedTypeResolutionScopeKind ResolutionScopeKind
      {
         get
         {
            return ExportedTypeResolutionScopeKind.Nested;
         }
      }
   }

   public sealed class ExportedTypeResolutionScopeFile : ExportedTypeResolutionScope
   {
      public ExportedTypeResolutionScopeFile()
      {

      }

      public FileReferenceStructure File { get; set; }

      public override ExportedTypeResolutionScopeKind ResolutionScopeKind
      {
         get
         {
            return ExportedTypeResolutionScopeKind.File;
         }
      }
   }

   public sealed class ExportedTypeResolutionScopeAssemblyRef : ExportedTypeResolutionScope
   {
      public ExportedTypeResolutionScopeAssemblyRef()
      {

      }

      public AssemblyReferenceStructure AssemblyRef { get; set; }

      public override ExportedTypeResolutionScopeKind ResolutionScopeKind
      {
         get
         {
            return ExportedTypeResolutionScopeKind.AssemblyRef;
         }
      }
   }

   public sealed class ModuleReferenceStructure : StructureWithCustomAttributes
   {
      public ModuleReferenceStructure( ModuleReference modRef )
      {
         this.ModuleName = modRef.ModuleName;
      }

      public String ModuleName { get; set; }
   }

   public sealed class FileReferenceStructure : StructureWithCustomAttributes
   {
      public FileReferenceStructure()
      {

      }

      public FileReferenceStructure( FileReference fRef )
      {
         this.Name = fRef.Name;
         this.Attributes = fRef.Attributes;
         this.HashValue = fRef.HashValue.CreateBlockCopy();
      }

      public String Name { get; set; }
      public CILAssemblyManipulator.Physical.FileAttributes Attributes { get; set; }
      public Byte[] HashValue { get; set; }
   }

   public sealed class FieldStructure : StructureWithCustomAttributes, StructurePresentInIL
   {
      public FieldStructure()
      {

      }

      public FieldStructure( FieldDefinition fDef )
         : this()
      {
         this.Attributes = fDef.Attributes;
         this.Name = fDef.Name;
      }

      public OpCodeStructureTokenKind StructureTokenKind
      {
         get
         {
            return OpCodeStructureTokenKind.FieldDef;
         }
      }

      public String Name { get; set; }
      public FieldAttributes Attributes { get; set; }
      public FieldStructureSignature Signature { get; set; }
      public ConstantStructure? ConstantValue { get; set; }
      public AbstractMarshalingInfo MarshalingInfo { get; set; }
      public Int32? FieldOffset { get; set; }
      public Byte[] FieldData { get; set; }
      public PInvokeInfo PInvokeInfo { get; set; }

      public override String ToString()
      {
         return this.Name;
      }
   }

   public struct ConstantStructure : IEquatable<ConstantStructure>
   {
      private readonly Object _const;

      public ConstantStructure( Object constant )
      {
         this._const = constant;
      }

      public Object Constant
      {
         get
         {
            return this._const;
         }
      }

      public override Boolean Equals( Object obj )
      {
         return obj is ConstantStructure && this.Equals( (ConstantStructure) obj );
      }

      public override Int32 GetHashCode()
      {
         return this._const.GetHashCodeSafe();
      }

      public Boolean Equals( ConstantStructure other )
      {
         return Equals( this._const, other._const );
      }
   }

   public enum MethodReferenceKind
   {
      MethodDef,
      MemberRef
   }

   public abstract class MethodDefOrMemberRefStructure : StructureWithCustomAttributes, StructurePresentInIL
   {
      internal MethodDefOrMemberRefStructure()
      {

      }

      public abstract MethodReferenceKind MethodReferenceKind { get; }

      public abstract OpCodeStructureTokenKind StructureTokenKind { get; }
   }

   public sealed class MethodStructure : MethodDefOrMemberRefStructure
   {
      private readonly List<ParameterStructure> _parameters;
      private readonly List<GenericParameterStructure> _genericParameters;
      private readonly List<SecurityStructure> _security;

      public MethodStructure()
      {
         this._parameters = new List<ParameterStructure>();
         this._genericParameters = new List<GenericParameterStructure>();
         this._security = new List<SecurityStructure>();
      }

      public MethodStructure( MethodDefinition mDef )
         : this()
      {
         this.Attributes = mDef.Attributes;
         this.ImplementationAttributes = mDef.ImplementationAttributes;
         this.Name = mDef.Name;
      }

      public override MethodReferenceKind MethodReferenceKind
      {
         get
         {
            return MethodReferenceKind.MethodDef;
         }
      }

      public override OpCodeStructureTokenKind StructureTokenKind
      {
         get
         {
            return OpCodeStructureTokenKind.MethodDef;
         }
      }

      public MethodAttributes Attributes { get; set; }
      public MethodImplAttributes ImplementationAttributes { get; set; }
      public String Name { get; set; }
      public MethodDefinitionStructureSignature Signature { get; set; }
      public PInvokeInfo PInvokeInfo { get; set; }
      public MethodILStructureInfo IL { get; set; }
      public List<SecurityStructure> SecurityInfo
      {
         get
         {
            return this._security;
         }
      }
      public List<ParameterStructure> Parameters
      {
         get
         {
            return this._parameters;
         }
      }
      public List<GenericParameterStructure> GenericParameters
      {
         get
         {
            return this._genericParameters;
         }
      }

      public override String ToString()
      {
         return this.Name;
      }
   }

   public sealed class ParameterStructure : StructureWithCustomAttributes
   {
      public ParameterStructure()
      {

      }

      public ParameterStructure( ParameterDefinition pDef )
         : this()
      {
         this.Attributes = pDef.Attributes;
         this.Name = pDef.Name;
         this.Sequence = pDef.Sequence;
      }

      public ParameterAttributes Attributes { get; set; }
      public Int32 Sequence { get; set; }
      public String Name { get; set; }
      public ConstantStructure? ConstantValue { get; set; }
      public AbstractMarshalingInfo MarshalingInfo { get; set; }
   }

   public abstract class StructureWithSemanticsMethods : StructureWithCustomAttributes
   {
      private readonly List<SemanticMethodInfo> _semanticMethods;

      internal StructureWithSemanticsMethods()
      {
         this._semanticMethods = new List<SemanticMethodInfo>();
      }

      public List<SemanticMethodInfo> SemanticMethods
      {
         get
         {
            return this._semanticMethods;
         }
      }
   }

   public struct SemanticMethodInfo
   {
      private readonly MethodSemanticsAttributes _attributes;
      private readonly MethodStructure _method;

      public SemanticMethodInfo( MethodSemanticsAttributes attributes, MethodStructure method )
      {
         this._attributes = attributes;
         this._method = method;
      }

      public MethodSemanticsAttributes Attributes
      {
         get
         {
            return this._attributes;
         }
      }

      public MethodStructure Method
      {
         get
         {
            return this._method;
         }
      }
   }

   public sealed class PropertyStructure : StructureWithSemanticsMethods
   {
      public PropertyStructure()
      {

      }

      public PropertyStructure( PropertyDefinition pDef )
      {
         this.Attributes = pDef.Attributes;
         this.Name = pDef.Name;
      }

      public PropertyAttributes Attributes { get; set; }
      public String Name { get; set; }
      public PropertyStructureSignature Signature { get; set; }
      public ConstantStructure? ConstantValue { get; set; }

      public override String ToString()
      {
         return this.Name;
      }
   }

   public sealed class EventStructure : StructureWithSemanticsMethods
   {
      public EventStructure()
      {

      }

      public EventStructure( EventDefinition pDef )
      {
         this.Attributes = pDef.Attributes;
         this.Name = pDef.Name;
      }

      public EventAttributes Attributes { get; set; }
      public String Name { get; set; }
      public AbstractTypeStructure EventType { get; set; }

      public override String ToString()
      {
         return this.Name;
      }
   }

   public sealed class InterfaceImplStructure : StructureWithCustomAttributes
   {

      public AbstractTypeStructure InterfaceType { get; set; }
   }

   public sealed class GenericParameterStructure : StructureWithCustomAttributes
   {
      private readonly List<GenericParameterConstraintStructure> _constraints;

      public GenericParameterStructure()
      {
         this._constraints = new List<GenericParameterConstraintStructure>();
      }

      public GenericParameterStructure( GenericParameterDefinition gParam )
         : this()
      {
         this.Name = gParam.Name;
         this.GenericParameterIndex = gParam.GenericParameterIndex;
         this.Attributes = gParam.Attributes;
      }

      public String Name { get; set; }
      public Int32 GenericParameterIndex { get; set; }
      public GenericParameterAttributes Attributes { get; set; }
      public List<GenericParameterConstraintStructure> Constraints
      {
         get
         {
            return this._constraints;
         }
      }
   }

   public sealed class GenericParameterConstraintStructure : StructureWithCustomAttributes
   {
      public GenericParameterConstraintStructure()
      {

      }

      public AbstractTypeStructure Constraint { get; set; }
   }

   public sealed class PInvokeInfo
   {
      public PInvokeAttributes Attributes { get; set; }
      public String PlatformInvokeName { get; set; }
      public ModuleReferenceStructure PlatformInvokeModule { get; set; }
   }

   public sealed class SecurityStructure : StructureWithCustomAttributes
   {
      private readonly List<AbstractSecurityInformation> _permissionSets;

      public SecurityStructure( Int32 permissionSetCount = 0 )
      {
         this._permissionSets = new List<AbstractSecurityInformation>( permissionSetCount );
      }

      internal SecurityStructure( SecurityDefinition sec )
         : this( sec.PermissionSets.Count )
      {
         this.SecurityAction = sec.Action;
         this._permissionSets.AddRange( sec.PermissionSets ); // TODO Clone
      }

      public SecurityAction SecurityAction { get; set; }

      public List<AbstractSecurityInformation> PermissionSets
      {
         get
         {
            return this._permissionSets;
         }
      }
   }

   public sealed class ManifestResourceStructure : StructureWithCustomAttributes
   {
      public ManifestResourceStructure()
      {

      }

      public ManifestResourceStructure( ManifestResource mRes )
      {
         this.Name = mRes.Name;
         this.Attributes = mRes.Attributes;
         this.Offset = mRes.Implementation.HasValue ? mRes.Offset : 0;
      }

      public ManifestResourceStructureData ManifestData { get; set; }
      public String Name { get; set; }
      public ManifestResourceAttributes Attributes { get; set; }
      public Int32 Offset { get; set; }
   }

   public enum ManifestResourceDataKind
   {
      Embedded,
      File,
      AssemblyRef
   }

   public abstract class ManifestResourceStructureData
   {
      internal ManifestResourceStructureData()
      {

      }

      public abstract ManifestResourceDataKind ManifestResourceDataKind { get; }
   }

   public sealed class ManifestResourceStructureDataEmbedded : ManifestResourceStructureData
   {

      public Byte[] Data { get; set; }

      public override ManifestResourceDataKind ManifestResourceDataKind
      {
         get
         {
            return ManifestResourceDataKind.Embedded;
         }
      }
   }

   public sealed class ManifestResourceStrucureDataFile : ManifestResourceStructureData
   {
      public FileReferenceStructure FileReference { get; set; }

      public override ManifestResourceDataKind ManifestResourceDataKind
      {
         get
         {
            return ManifestResourceDataKind.File;
         }
      }
   }

   public sealed class ManifestResourceStructureDataAssemblyReference : ManifestResourceStructureData
   {
      public AssemblyReferenceStructure AssemblyRef { get; set; }

      public override ManifestResourceDataKind ManifestResourceDataKind
      {
         get
         {
            return ManifestResourceDataKind.AssemblyRef;
         }
      }
   }

   public sealed class MemberReferenceStructure : MethodDefOrMemberRefStructure
   {
      internal MemberReferenceStructure()
      {

      }

      internal MemberReferenceStructure( MemberReference mRef )
         : this()
      {
         this.Name = mRef.Name;
      }

      public override MethodReferenceKind MethodReferenceKind
      {
         get
         {
            return MethodReferenceKind.MemberRef;
         }
      }

      public override OpCodeStructureTokenKind StructureTokenKind
      {
         get
         {
            return OpCodeStructureTokenKind.MemberRef;
         }
      }

      public String Name { get; set; }
      public AbstractStructureSignature Signature { get; set; }
      public MemberReferenceParent Parent { get; set; }

      public override String ToString()
      {
         return Miscellaneous.CombineNamespaceAndType( this.Parent.ToStringSafe(), this.Name );
      }
   }

   public enum MemberReferenceParentKind
   {
      MethodDef,
      ModuleRef,
      Type
   }

   public abstract class MemberReferenceParent
   {
      internal MemberReferenceParent()
      {

      }

      public abstract MemberReferenceParentKind MemberReferenceParentKind { get; }
      public abstract override String ToString();
   }

   public sealed class MemberReferenceParentMethodDef : MemberReferenceParent
   {

      public MethodStructure Method { get; set; }

      public override MemberReferenceParentKind MemberReferenceParentKind
      {
         get
         {
            return MemberReferenceParentKind.MethodDef;
         }
      }

      public override String ToString()
      {
         return this.Method.ToStringSafe();
      }
   }

   public sealed class MemberReferenceParentModuleRef : MemberReferenceParent
   {

      public ModuleReferenceStructure ModuleRef { get; set; }

      public override MemberReferenceParentKind MemberReferenceParentKind
      {
         get
         {
            return MemberReferenceParentKind.ModuleRef;
         }
      }

      public override String ToString()
      {
         return this.ModuleRef.ToStringSafe();
      }
   }

   public sealed class MemberReferenceParentType : MemberReferenceParent
   {

      public AbstractTypeStructure Type { get; set; }

      public override MemberReferenceParentKind MemberReferenceParentKind
      {
         get
         {
            return MemberReferenceParentKind.Type;
         }
      }

      public override String ToString()
      {
         return this.Type.ToStringSafe();
      }
   }

   public sealed class StandaloneSignatureStructure : StructureWithCustomAttributes, StructurePresentInIL
   {
      public AbstractStructureSignature Signature { get; set; }

      public OpCodeStructureTokenKind StructureTokenKind
      {
         get
         {
            return OpCodeStructureTokenKind.StandaloneSignature;
         }
      }
   }

   public sealed class MethodSpecificationStructure : StructureWithCustomAttributes, StructurePresentInIL
   {
      public MethodDefOrMemberRefStructure Method { get; set; }
      public GenericMethodStructureSignature Signature { get; set; }

      public OpCodeStructureTokenKind StructureTokenKind
      {
         get
         {
            return OpCodeStructureTokenKind.MethodSpec;
         }
      }
   }
}
