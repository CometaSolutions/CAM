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
using CILAssemblyManipulator.Structural;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Structural
{
   public abstract class StructuralElementWithCustomAttributes
   {
      private readonly List<CustomAttributeStructure> _customAttributes;

      internal StructuralElementWithCustomAttributes()
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

   public sealed class AssemblyStructureInfo : StructuralElementWithCustomAttributes
   {
      private readonly List<ModuleStructureInfo> _modules;

      public AssemblyStructureInfo()
      {
         this._modules = new List<ModuleStructureInfo>();
      }

      public AssemblyStructureInfo( CILMetaData md )
         : this()
      {
         var aDef = md.AssemblyDefinitions.TableContents[0];
         var aInfo = new AssemblyInformation();
         aDef.AssemblyInformation.DeepCopyContentsTo( aInfo );
         this.AssemblyInfo = aInfo;
         this.Attributes = aDef.Attributes;
         this.HashAlgorithm = aDef.HashAlgorithm;
         this.Modules.Add( this.NewModuleFromPhysical( md ) );
      }

      public AssemblyInformation AssemblyInfo { get; set; }
      public AssemblyFlags Attributes { get; set; }
      public AssemblyHashAlgorithm HashAlgorithm { get; set; }
      public SecurityStructuralInfo SecurityInfo { get; set; }
      public List<ModuleStructureInfo> Modules
      {
         get
         {
            return this._modules;
         }
      }
   }

   public sealed class ModuleStructureInfo : StructuralElementWithCustomAttributes
   {
      private readonly List<TypeDefDescription> _topLevelTypeDefs;
      private readonly List<ExportedTypeStructureInfo> _exportedTypes;
      private readonly List<ManifestResourceStructuralInfo> _resources;

      public ModuleStructureInfo()
      {
         this._topLevelTypeDefs = new List<TypeDefDescription>();
         this._exportedTypes = new List<ExportedTypeStructureInfo>();
         this._resources = new List<ManifestResourceStructuralInfo>();
      }

      internal ModuleStructureInfo( CILMetaData md )
         : this()
      {
         this.Name = md.ModuleDefinitions.TableContents[0].Name;
         this.IsMainModule = md.AssemblyDefinitions.RowCount > 0;
      }

      public String Name { get; set; }
      public Boolean IsMainModule { get; set; }

      public List<TypeDefDescription> TopLevelTypeDefinitions
      {
         get
         {
            return this._topLevelTypeDefs;
         }
      }
      public List<ExportedTypeStructureInfo> ExportedTypes
      {
         get
         {
            return this._exportedTypes;
         }
      }

      public List<ManifestResourceStructuralInfo> ManifestResources
      {
         get
         {
            return this._resources;
         }
      }
   }

   public enum TypeDescriptionKind
   {
      TypeDef,
      TypeRef,
      TypeSpec
   }

   public abstract class AbstractTypeDescription : StructuralElementWithCustomAttributes
   {
      internal AbstractTypeDescription()
      {

      }

      public abstract TypeDescriptionKind TypeDescriptionKind { get; }
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

   public sealed class TypeDefDescription : AbstractTypeDescription
   {
      private readonly List<TypeDefDescription> _nestedTypes;
      private readonly List<FieldStructureInfo> _fields;
      private readonly List<MethodStructureInfo> _methods;
      private readonly List<PropertyStructuralInfo> _properties;
      private readonly List<EventStructuralInfo> _events;

      private readonly List<InterfaceImplStructuralInfo> _interfaces;
      private readonly List<GenericParameterStructuralInfo> _genericParameters;
      private readonly List<OverriddenMethodInfo> _overriddenMethods;

      internal TypeDefDescription()
      {
         this._nestedTypes = new List<TypeDefDescription>();
         this._fields = new List<FieldStructureInfo>();
         this._methods = new List<MethodStructureInfo>();
         this._properties = new List<PropertyStructuralInfo>();
         this._events = new List<EventStructuralInfo>();
         this._interfaces = new List<InterfaceImplStructuralInfo>();
         this._genericParameters = new List<GenericParameterStructuralInfo>();
         this._overriddenMethods = new List<OverriddenMethodInfo>();
      }

      internal TypeDefDescription( TypeDefinition tDef )
         : this()
      {
         this.Namespace = tDef.Namespace;
         this.Name = tDef.Name;
         this.Attributes = tDef.Attributes;
      }

      public override TypeDescriptionKind TypeDescriptionKind
      {
         get
         {
            return TypeDescriptionKind.TypeDef;
         }
      }

      public String Name { get; set; }
      public String Namespace { get; set; }
      public TypeAttributes Attributes { get; set; }
      public AbstractTypeDescription BaseType { get; set; }
      public LayoutInfo Layout { get; set; }
      public SecurityStructuralInfo SecurityInfo { get; set; }

      public List<TypeDefDescription> NestedTypes
      {
         get
         {
            return this._nestedTypes;
         }
      }

      public List<FieldStructureInfo> Fields
      {
         get
         {
            return this._fields;
         }
      }

      public List<MethodStructureInfo> Methods
      {
         get
         {
            return this._methods;
         }
      }

      public List<PropertyStructuralInfo> Properties
      {
         get
         {
            return this._properties;
         }
      }

      public List<EventStructuralInfo> Events
      {
         get
         {
            return this._events;
         }
      }

      public List<GenericParameterStructuralInfo> GenericParameters
      {
         get
         {
            return this._genericParameters;
         }
      }

      public List<InterfaceImplStructuralInfo> ImplementedInterfaces
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
         return Miscellaneous.CombineTypeAndNamespace( this.Name, this.Namespace );
      }
   }

   public sealed class TypeRefDescription : AbstractTypeDescription
   {
      public TypeRefDescription()
      {

      }

      public TypeRefDescription( TypeReference tRef )
         : this()
      {
         this.Name = tRef.Name;
         this.Namespace = tRef.Namespace;
      }

      public String Name { get; set; }
      public String Namespace { get; set; }
      public TypeRefResolutionScope ResolutionScope { get; set; }

      public override TypeDescriptionKind TypeDescriptionKind
      {
         get
         {
            return TypeDescriptionKind.TypeRef;
         }
      }

      public override String ToString()
      {
         return Miscellaneous.CombineTypeAndNamespace( this.Name, this.Namespace );
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

   public abstract class TypeRefResolutionScope
   {
      internal TypeRefResolutionScope()
      {

      }

      public abstract TypeRefResolutionScopeKind ResolutionScopeKind { get; }
   }

   public sealed class TypeRefResolutionScopeTypeDef : TypeRefResolutionScope
   {
      internal TypeRefResolutionScopeTypeDef()
      {

      }

      public TypeDefDescription TypeDef { get; set; }

      public override TypeRefResolutionScopeKind ResolutionScopeKind
      {
         get
         {
            return TypeRefResolutionScopeKind.TypeDef;
         }
      }
   }

   public sealed class TypeRefResolutionScopeNested : TypeRefResolutionScope
   {
      internal TypeRefResolutionScopeNested()
      {

      }

      public override TypeRefResolutionScopeKind ResolutionScopeKind
      {
         get
         {
            return TypeRefResolutionScopeKind.Nested;
         }
      }

      public TypeRefDescription EnclosingTypeRef { get; set; }
   }

   public sealed class TypeRefResolutionScopeModuleRef : TypeRefResolutionScope
   {
      internal TypeRefResolutionScopeModuleRef()
      {

      }

      public override TypeRefResolutionScopeKind ResolutionScopeKind
      {
         get
         {
            return TypeRefResolutionScopeKind.ModuleRef;
         }
      }

      public ModuleRefStructureInfo ModuleRef { get; set; }
   }

   public sealed class TypeRefResolutionScopeAssemblyRef : TypeRefResolutionScope
   {
      internal TypeRefResolutionScopeAssemblyRef()
      {

      }

      public override TypeRefResolutionScopeKind ResolutionScopeKind
      {
         get
         {
            return TypeRefResolutionScopeKind.AssemblyRef;
         }
      }

      public AssemblyRefStructureInfo AssemblyRef { get; set; }
   }

   public sealed class TypeRefResolutionScopeExportedType : TypeRefResolutionScope
   {

      public ExportedTypeStructureInfo ExportedType { get; set; }

      public override TypeRefResolutionScopeKind ResolutionScopeKind
      {
         get
         {
            return TypeRefResolutionScopeKind.ExportedType;
         }
      }
   }

   public sealed class TypeSpecDescription : AbstractTypeDescription
   {
      internal TypeSpecDescription()
      {
      }

      public override TypeDescriptionKind TypeDescriptionKind
      {
         get
         {
            return TypeDescriptionKind.TypeSpec;
         }
      }

      public TypeStructureSignature Signature { get; set; }
   }

   public sealed class AssemblyRefStructureInfo : StructuralElementWithCustomAttributes
   {
      internal AssemblyRefStructureInfo( AssemblyReference aRef )
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

   public sealed class ExportedTypeStructureInfo : StructuralElementWithCustomAttributes
   {
      internal ExportedTypeStructureInfo()
      {

      }

      internal ExportedTypeStructureInfo( ExportedType eType )
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
      internal ExportedTypeResolutionScopeNested()
      {

      }

      public ExportedTypeStructureInfo EnclosingType { get; set; }

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
      internal ExportedTypeResolutionScopeFile()
      {

      }

      public FileReferenceStructureInfo File { get; set; }

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
      internal ExportedTypeResolutionScopeAssemblyRef()
      {

      }

      public AssemblyRefStructureInfo AssemblyRef { get; set; }

      public override ExportedTypeResolutionScopeKind ResolutionScopeKind
      {
         get
         {
            return ExportedTypeResolutionScopeKind.AssemblyRef;
         }
      }
   }

   public sealed class ModuleRefStructureInfo : StructuralElementWithCustomAttributes
   {
      internal ModuleRefStructureInfo( ModuleReference modRef )
      {
         this.ModuleName = modRef.ModuleName;
      }

      public String ModuleName { get; set; }
   }

   public sealed class FileReferenceStructureInfo : StructuralElementWithCustomAttributes
   {
      internal FileReferenceStructureInfo()
      {

      }

      internal FileReferenceStructureInfo( FileReference fRef )
      {
         this.Name = fRef.Name;
         this.Attributes = fRef.Attributes;
         this.HashValue = fRef.HashValue.CreateBlockCopy();
      }

      public String Name { get; set; }
      public CILAssemblyManipulator.Physical.FileAttributes Attributes { get; set; }
      public Byte[] HashValue { get; set; }
   }

   public sealed class FieldStructureInfo : StructuralElementWithCustomAttributes
   {
      internal FieldStructureInfo()
      {

      }

      internal FieldStructureInfo( FieldDefinition fDef )
         : this()
      {
         this.Attributes = fDef.Attributes;
         this.Name = fDef.Name;
      }

      public String Name { get; set; }
      public FieldAttributes Attributes { get; set; }
      public FieldStructureSignature Signature { get; set; }
      public Object ConstantValue { get; set; }
      public MarshalingInfo MarshalingInfo { get; set; }
      public Int32 FieldOffset { get; set; }
      public Byte[] FieldData { get; set; }
      public PInvokeInfo PInvokeInfo { get; set; }

      public override String ToString()
      {
         return this.Name;
      }
   }

   public enum MethodRefKind
   {
      MethodDef,
      MemberRef
   }

   public abstract class MethodDefOrMemberRefStructure : StructuralElementWithCustomAttributes
   {
      internal MethodDefOrMemberRefStructure()
      {

      }

      public abstract MethodRefKind MethodRefKind { get; }
   }

   public sealed class MethodStructureInfo : MethodDefOrMemberRefStructure
   {
      private readonly List<ParameterStructureInfo> _parameters;
      private readonly List<GenericParameterStructuralInfo> _genericParameters;

      internal MethodStructureInfo()
      {
         this._parameters = new List<ParameterStructureInfo>();
         this._genericParameters = new List<GenericParameterStructuralInfo>();
      }

      internal MethodStructureInfo( MethodDefinition mDef )
         : this()
      {
         this.Attributes = mDef.Attributes;
         this.ImplementationAttributes = mDef.ImplementationAttributes;
         this.Name = mDef.Name;
      }

      public override MethodRefKind MethodRefKind
      {
         get
         {
            return MethodRefKind.MethodDef;
         }
      }

      public MethodAttributes Attributes { get; set; }
      public MethodImplAttributes ImplementationAttributes { get; set; }
      public String Name { get; set; }
      public MethodDefinitionStructureSignature Signature { get; set; }
      public PInvokeInfo PInvokeInfo { get; set; }
      public MethodILStructureInfo IL { get; set; }
      public SecurityStructuralInfo SecurityInfo { get; set; }
      public List<ParameterStructureInfo> Parameters
      {
         get
         {
            return this._parameters;
         }
      }
      public List<GenericParameterStructuralInfo> GenericParameters
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

   public sealed class MethodILStructureInfo
   {

   }

   public sealed class ParameterStructureInfo : StructuralElementWithCustomAttributes
   {
      internal ParameterStructureInfo()
      {

      }

      internal ParameterStructureInfo( ParameterDefinition pDef )
         : this()
      {
         this.Attributes = pDef.Attributes;
         this.Name = pDef.Name;
         this.Sequence = pDef.Sequence;
      }

      public ParameterAttributes Attributes { get; set; }
      public Int32 Sequence { get; set; }
      public String Name { get; set; }
      public Object ConstantValue { get; set; }
      public MarshalingInfo MarshalingInfo { get; set; }
   }

   public abstract class StructuralInfoWithSemanticsMethods : StructuralElementWithCustomAttributes
   {
      private readonly List<Tuple<MethodSemanticsAttributes, MethodStructureInfo>> _semanticMethods;

      internal StructuralInfoWithSemanticsMethods()
      {
         this._semanticMethods = new List<Tuple<MethodSemanticsAttributes, MethodStructureInfo>>();
      }

      public List<Tuple<MethodSemanticsAttributes, MethodStructureInfo>> SemanticMethods
      {
         get
         {
            return this._semanticMethods;
         }
      }
   }

   public sealed class PropertyStructuralInfo : StructuralInfoWithSemanticsMethods
   {
      internal PropertyStructuralInfo()
      {

      }

      internal PropertyStructuralInfo( PropertyDefinition pDef )
      {
         this.Attributes = pDef.Attributes;
         this.Name = pDef.Name;
      }

      public PropertyAttributes Attributes { get; set; }
      public String Name { get; set; }
      public PropertyStructureSignature Signature { get; set; }
      public Object ConstantValue { get; set; }

      public override String ToString()
      {
         return this.Name;
      }
   }

   public sealed class EventStructuralInfo : StructuralInfoWithSemanticsMethods
   {
      internal EventStructuralInfo()
      {

      }

      internal EventStructuralInfo( EventDefinition pDef )
      {
         this.Attributes = pDef.Attributes;
         this.Name = pDef.Name;
      }

      public EventAttributes Attributes { get; set; }
      public String Name { get; set; }
      public AbstractTypeDescription EventType { get; set; }

      public override String ToString()
      {
         return this.Name;
      }
   }

   public sealed class InterfaceImplStructuralInfo : StructuralElementWithCustomAttributes
   {

      public AbstractTypeDescription InterfaceType { get; set; }
   }

   public sealed class GenericParameterStructuralInfo : StructuralElementWithCustomAttributes
   {
      private readonly List<GenericParameterConstraintStructuralInfo> _constraints;

      public GenericParameterStructuralInfo()
      {
         this._constraints = new List<GenericParameterConstraintStructuralInfo>();
      }

      public GenericParameterStructuralInfo( GenericParameterDefinition gParam )
         : this()
      {
         this.Name = gParam.Name;
         this.GenericParameterIndex = gParam.GenericParameterIndex;
         this.Attributes = gParam.Attributes;
      }

      public String Name { get; set; }
      public Int32 GenericParameterIndex { get; set; }
      public GenericParameterAttributes Attributes { get; set; }
      public List<GenericParameterConstraintStructuralInfo> Constraints
      {
         get
         {
            return this._constraints;
         }
      }
   }

   public sealed class GenericParameterConstraintStructuralInfo : StructuralElementWithCustomAttributes
   {
      public GenericParameterConstraintStructuralInfo()
      {

      }

      public AbstractTypeDescription Constraint { get; set; }
   }

   public sealed class PInvokeInfo : IEquatable<PInvokeInfo>
   {
      public PInvokeAttributes Attributes { get; set; }
      public String PlatformInvokeName { get; set; }
      public String PlatformInvokeModuleName { get; set; }

      public override Boolean Equals( Object obj )
      {
         return this.Equals( obj as PInvokeInfo );
      }

      public override Int32 GetHashCode()
      {
         return base.GetHashCode();
      }

      public Boolean Equals( PInvokeInfo other )
      {
         return ReferenceEquals( this, other )
            || ( other != null
            && String.Equals( this.PlatformInvokeName, other.PlatformInvokeName )
            && String.Equals( this.PlatformInvokeModuleName, other.PlatformInvokeModuleName )
            && this.Attributes == other.Attributes
            );
      }
   }

   public sealed class SecurityStructuralInfo : StructuralElementWithCustomAttributes
   {
      private readonly List<AbstractSecurityInformation> _permissionSets;

      public SecurityStructuralInfo( Int32 permissionSetCount = 0 )
      {
         this._permissionSets = new List<AbstractSecurityInformation>( permissionSetCount );
      }

      public SecurityStructuralInfo( SecurityDefinition secDef )
         : this( secDef.PermissionSets.Count )
      {
         this.SecurityAction = secDef.Action;
         this._permissionSets.AddRange( secDef.PermissionSets );
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

   public sealed class ManifestResourceStructuralInfo : StructuralElementWithCustomAttributes
   {
      internal ManifestResourceStructuralInfo()
      {

      }

      internal ManifestResourceStructuralInfo( ManifestResource mRes )
      {
         this.Name = mRes.Name;
         this.Attributes = mRes.Attributes;
         this.Offset = mRes.Offset;
      }

      public ManifestResourceData ManifestData { get; set; }
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

   public abstract class ManifestResourceData
   {
      internal ManifestResourceData()
      {

      }

      public abstract ManifestResourceDataKind ManifestResourceDataKind { get; }
   }

   public sealed class ManifestResourceDataEmbedded : ManifestResourceData
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

   public sealed class ManifestResourceDataFile : ManifestResourceData
   {
      public FileReferenceStructureInfo FileReference { get; set; }

      public override ManifestResourceDataKind ManifestResourceDataKind
      {
         get
         {
            return ManifestResourceDataKind.Embedded;
         }
      }
   }

   public sealed class ManifestResourceDataAssemblyRef : ManifestResourceData
   {
      public AssemblyRefStructureInfo AssemblyRef { get; set; }

      public override ManifestResourceDataKind ManifestResourceDataKind
      {
         get
         {
            return ManifestResourceDataKind.Embedded;
         }
      }
   }

   public sealed class MemberReferenceStructuralInfo : MethodDefOrMemberRefStructure
   {
      internal MemberReferenceStructuralInfo()
      {

      }

      internal MemberReferenceStructuralInfo( MemberReference mRef )
         : this()
      {
         this.Name = mRef.Name;
      }

      public override MethodRefKind MethodRefKind
      {
         get
         {
            return MethodRefKind.MemberRef;
         }
      }

      public String Name { get; set; }
      public AbstractStructureSignature Signature { get; set; }
      public MemberReferenceParent Parent { get; set; }
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
   }

   public sealed class MemberReferenceParentMethodDef : MemberReferenceParent
   {

      public MethodStructureInfo Method { get; set; }

      public override MemberReferenceParentKind MemberReferenceParentKind
      {
         get
         {
            return MemberReferenceParentKind.MethodDef;
         }
      }
   }

   public sealed class MemberReferenceParentModuleRef : MemberReferenceParent
   {

      public ModuleRefStructureInfo ModuleRef { get; set; }

      public override MemberReferenceParentKind MemberReferenceParentKind
      {
         get
         {
            return MemberReferenceParentKind.ModuleRef;
         }
      }
   }

   public sealed class MemberReferenceParentType : MemberReferenceParent
   {

      public AbstractTypeDescription Type { get; set; }

      public override MemberReferenceParentKind MemberReferenceParentKind
      {
         get
         {
            return MemberReferenceParentKind.Type;
         }
      }
   }

   public sealed class StandaloneSignatureStructure : StructuralElementWithCustomAttributes
   {
      internal StandaloneSignatureStructure()
      {

      }

      public AbstractStructureSignature Signature { get; set; }
   }

   public sealed class MethodSpecificationStructure : StructuralElementWithCustomAttributes
   {
      internal MethodSpecificationStructure()
      {

      }

      public MethodDefOrMemberRefStructure Method { get; set; }
      public GenericMethodStructureSignature Signature { get; set; }
   }
}

public static partial class E_CILTests
{
   private sealed class StructuralCreationState
   {
      private readonly TypeDefDescription[] _typeDefDescriptions;
      private readonly TypeRefDescription[] _typeRefDescriptions;
      private readonly TypeSpecDescription[] _typeSpecDescriptions;
      private readonly ExportedTypeStructureInfo[] _exportedTypes;

      private readonly ModuleRefStructureInfo[] _moduleRefs;
      private readonly FileReferenceStructureInfo[] _fileRefs;
      private readonly AssemblyRefStructureInfo[] _assemblyRefs;

      private readonly ISet<Int32> _topLevelTypes;

      internal StructuralCreationState( CILMetaData md )
      {
         // TypeDefs
         var nestedTypes = new Dictionary<Int32, ISet<Int32>>();
         var enclosingTypes = new Dictionary<Int32, Int32>();
         foreach ( var nc in md.NestedClassDefinitions.TableContents )
         {
            nestedTypes
               .GetOrAdd_NotThreadSafe( nc.EnclosingClass.Index, i => new HashSet<Int32>() )
               .Add( nc.NestedClass.Index );
            enclosingTypes[nc.NestedClass.Index] = nc.EnclosingClass.Index;
         }

         var tDefs = md.TypeDefinitions.TableContents;
         var tlTypes = new HashSet<Int32>( Enumerable.Range( 0, tDefs.Count ) );
         tlTypes.ExceptWith( nestedTypes.Values.SelectMany( v => v ) );
         this._typeDefDescriptions = tDefs
            .Select( tDef => new TypeDefDescription( tDef ) )
            .ToArray();
         this._topLevelTypes = tlTypes;
         foreach ( var kvp in nestedTypes )
         {
            this._typeDefDescriptions[kvp.Key].NestedTypes.AddRange( kvp.Value.Select( i => this._typeDefDescriptions[i] ) );
         }
         var typeDefInfosTopLevel = tlTypes.ToDictionary( t => Miscellaneous.CombineTypeAndNamespace( tDefs[t].Name, tDefs[t].Namespace ), t => this._typeDefDescriptions[t] );

         // TypeRef
         this._moduleRefs = md.ModuleReferences.TableContents.Select( mRef => new ModuleRefStructureInfo( mRef ) ).ToArray();
         this._assemblyRefs = md.AssemblyReferences.TableContents.Select( aRef => new AssemblyRefStructureInfo( aRef ) ).ToArray();
         this._fileRefs = md.FileReferences.TableContents.Select( fRef => new FileReferenceStructureInfo( fRef ) ).ToArray();
         var eTypes = md.ExportedTypes.TableContents;
         var eTypeList = eTypes.Select( eType => new ExportedTypeStructureInfo( eType ) ).ToArray();
         this._exportedTypes = eTypeList;
         var exportedTopLevelTypes = new Dictionary<String, ExportedTypeStructureInfo>();
         for ( var i = 0; i < eTypes.Count; ++i )
         {
            var resScope = eTypes[i].Implementation;
            ExportedTypeResolutionScope resScopeInfo;
            var isTopLevel = true;
            switch ( resScope.Table )
            {
               case Tables.File:
                  resScopeInfo = new ExportedTypeResolutionScopeFile()
                  {
                     File = this._fileRefs[resScope.Index]
                  };
                  break;
               case Tables.AssemblyRef:
                  resScopeInfo = new ExportedTypeResolutionScopeAssemblyRef()
                  {
                     AssemblyRef = this._assemblyRefs[resScope.Index]
                  };
                  break;
               case Tables.ExportedType:
                  resScopeInfo = new ExportedTypeResolutionScopeNested()
                  {
                     EnclosingType = eTypeList[resScope.Index]
                  };
                  isTopLevel = false;
                  break;
               default:
                  resScopeInfo = null;
                  break;
            }
            var eType = eTypeList[i];
            if ( resScopeInfo != null )
            {
               eType.ResolutionScope = resScopeInfo;
            }

            if ( isTopLevel )
            {
               exportedTopLevelTypes.Add( Miscellaneous.CombineTypeAndNamespace( eType.Name, eType.Namespace ), eType );
            }

         }
         var tRefs = md.TypeReferences.TableContents;
         this._typeRefDescriptions = tRefs
            .Select( tRef => new TypeRefDescription( tRef ) )
            .ToArray();
         for ( var i = 0; i < tRefs.Count; ++i )
         {
            var tRef = tRefs[i];
            var resScopeNullable = tRef.ResolutionScope;
            var info = this._typeRefDescriptions[i];
            TypeRefResolutionScope resScopeInfo;
            ExportedTypeStructureInfo eType;
            if ( resScopeNullable.HasValue )
            {
               var resScope = resScopeNullable.Value;
               switch ( resScope.Table )
               {
                  case Tables.TypeRef:
                     resScopeInfo = new TypeRefResolutionScopeNested()
                     {
                        EnclosingTypeRef = this._typeRefDescriptions[resScope.Index] as TypeRefDescription
                     };
                     break;
                  case Tables.Module:
                     resScopeInfo = new TypeRefResolutionScopeTypeDef()
                     {
                        TypeDef = typeDefInfosTopLevel[Miscellaneous.CombineTypeAndNamespace( tRef.Name, tRef.Namespace )]
                     };
                     break;
                  case Tables.ModuleRef:
                     resScopeInfo = new TypeRefResolutionScopeModuleRef()
                     {
                        ModuleRef = this._moduleRefs[resScope.Index]
                     };
                     break;
                  case Tables.AssemblyRef:
                     var aRef = md.AssemblyReferences.TableContents[resScope.Index];
                     resScopeInfo = new TypeRefResolutionScopeAssemblyRef()
                     {
                        AssemblyRef = this._assemblyRefs[resScope.Index]
                     };
                     break;
                  default:
                     resScopeInfo = null;
                     break;
               }
            }
            else if ( exportedTopLevelTypes.TryGetValue( Miscellaneous.CombineTypeAndNamespace( tRef.Name, tRef.Namespace ), out eType ) )
            {
               resScopeInfo = new TypeRefResolutionScopeExportedType()
               {
                  ExportedType = eType
               };
            }
            else
            {
               resScopeInfo = null;
            }

            if ( resScopeInfo != null )
            {
               info.ResolutionScope = resScopeInfo;
            }

         }

         // TypeSpecs
         var tSpecs = md.TypeSpecifications.TableContents;
         this._typeSpecDescriptions = tSpecs
            .Select( tSpec => new TypeSpecDescription() )
            .ToArray();
         for ( var i = 0; i < tSpecs.Count; ++i )
         {
            var tSpec = tSpecs[i];
            this._typeSpecDescriptions[i].Signature = (TypeStructureSignature) this.CreateStructureSignature( tSpec.Signature );
         }
      }

      public TypeDefDescription[] TypeDefDescriptions
      {
         get
         {
            return this._typeDefDescriptions;
         }
      }

      public TypeRefDescription[] TypeRefDescriptions
      {
         get
         {
            return this._typeRefDescriptions;
         }
      }

      public TypeSpecDescription[] TypeSpecDescriptions
      {
         get
         {
            return this._typeSpecDescriptions;
         }
      }

      public ModuleRefStructureInfo[] ModuleRefs
      {
         get
         {
            return this._moduleRefs;
         }
      }

      public FileReferenceStructureInfo[] FileRefs
      {
         get
         {
            return this._fileRefs;
         }
      }

      public AssemblyRefStructureInfo[] AssemblyRefs
      {
         get
         {
            return this._assemblyRefs;
         }
      }

      public ExportedTypeStructureInfo[] ExportedTypes
      {
         get
         {
            return this._exportedTypes;
         }
      }

      public ISet<Int32> TopLevelTypes
      {
         get
         {
            return this._topLevelTypes;
         }
      }
   }

   public static ModuleStructureInfo NewModuleFromPhysical( this AssemblyStructureInfo assembly, CILMetaData metaData )
   {
      var state = new StructuralCreationState( metaData );
      var module = new ModuleStructureInfo( metaData );
      var resources = state.PopulateStructure( assembly, module, metaData );

      module.TopLevelTypeDefinitions.AddRange( state.TopLevelTypes.Select( idx => state.TypeDefDescriptions[idx] ) );
      module.ExportedTypes.AddRange( state.ExportedTypes );
      module.ManifestResources.AddRange( resources );
      return module;
   }

   private static List<ManifestResourceStructuralInfo> PopulateStructure(
      this StructuralCreationState state,
      AssemblyStructureInfo assembly,
      ModuleStructureInfo module,
      CILMetaData md
      )
   {
      var tDefList = state.TypeDefDescriptions;

      // Fields
      var fDefs = md.FieldDefinitions.TableContents;
      var fDefList = new List<FieldStructureInfo>( fDefs.Count );
      fDefList.AddRange( fDefs.Select( fDef => new FieldStructureInfo( fDef ) ) );

      // Methods
      var mDefs = md.MethodDefinitions.TableContents;
      var mDefList = new List<MethodStructureInfo>( mDefs.Count );
      mDefList.AddRange( mDefs.Select( mDef => new MethodStructureInfo( mDef ) ) );

      // Parameters
      var paramDefs = md.ParameterDefinitions.TableContents;
      var paramDefList = new List<ParameterStructureInfo>( paramDefs.Count );
      paramDefList.AddRange( paramDefs.Select( paramDef => new ParameterStructureInfo( paramDef ) ) );

      // Properties
      var propDefs = md.PropertyDefinitions.TableContents;
      var propDefList = new List<PropertyStructuralInfo>( propDefs.Count );
      propDefList.AddRange( propDefs.Select( propDef =>
      {
         var prop = new PropertyStructuralInfo( propDef );
         prop.Signature = (PropertyStructureSignature) state.CreateStructureSignature( propDef.Signature );
         return prop;
      } ) );

      // Events
      var evtDefs = md.EventDefinitions.TableContents;
      var evtDefList = new List<EventStructuralInfo>( evtDefs.Count );
      evtDefList.AddRange( evtDefs.Select( evtDef =>
      {
         var evt = new EventStructuralInfo( evtDef );
         evt.EventType = state.FromTypeDefOrRefOrSpec( evtDef.EventType );
         return evt;
      } ) );

      // Process types
      var tDefs = md.TypeDefinitions.TableContents;
      for ( var i = 0; i < tDefs.Count; ++i )
      {
         var tDef = tDefs[i];
         var tDefDesc = tDefList[i];

         // Base type
         if ( tDef.BaseType.HasValue )
         {
            tDefDesc.BaseType = state.FromTypeDefOrRefOrSpec( tDef.BaseType.Value );
         }

         tDefDesc.Fields.AddRange( md.GetTypeFieldIndices( i ).Select( fIdx => fDefList[fIdx] ) );

         // Method parameter types and custom modifiers
         tDefDesc.Methods.AddRange( md.GetTypeMethodIndices( i ).Select( mIdx => mDefList[mIdx] ) );
      }

      for ( var i = 0; i < fDefs.Count; ++i )
      {
         fDefList[i].Signature = (FieldStructureSignature) state.CreateStructureSignature( fDefs[i].Signature );
      }

      for ( var i = 0; i < mDefs.Count; ++i )
      {
         var method = mDefList[i];
         method.Signature = (MethodDefinitionStructureSignature) state.CreateStructureSignature( mDefs[i].Signature );
         method.Parameters.AddRange( md.GetMethodParameterIndices( i ).Select( pIdx => paramDefList[pIdx] ) );
      }

      // Properties
      var pMaps = md.PropertyMaps.TableContents;
      for ( var i = 0; i < pMaps.Count; ++i )
      {
         tDefList[pMaps[i].Parent.Index].Properties.AddRange( md.GetTypePropertyIndices( i ).Select( propIdx => propDefList[propIdx] ) );
      }

      // Events
      var eMaps = md.EventMaps.TableContents;
      for ( var i = 0; i < eMaps.Count; ++i )
      {
         tDefList[eMaps[i].Parent.Index].Events.AddRange( md.GetTypeEventIndices( i ).Select( evtIdx => evtDefList[evtIdx] ) );
      }

      // Method Semantics
      foreach ( var semantic in md.MethodSemantics.TableContents )
      {
         StructuralInfoWithSemanticsMethods semInfo;
         var asso = semantic.Associaton;
         switch ( asso.Table )
         {
            case Tables.Property:
               semInfo = propDefList[asso.Index];
               break;
            case Tables.Event:
               semInfo = evtDefList[asso.Index];
               break;
            default:
               semInfo = null;
               break;
         }

         if ( semInfo != null )
         {
            semInfo.SemanticMethods.Add( Tuple.Create( semantic.Attributes, mDefList[semantic.Method.Index] ) );
         }
      }

      // Generic Parameters
      var gArgs = md.GenericParameterDefinitions.TableContents;
      var gArgsList = new List<GenericParameterStructuralInfo>( gArgs.Count );
      gArgsList.AddRange( gArgs.Select( gArg =>
      {
         var gArgInfo = new GenericParameterStructuralInfo( gArg );
         List<GenericParameterStructuralInfo> thisArgs;
         var owner = gArg.Owner;
         switch ( owner.Table )
         {
            case Tables.TypeDef:
               thisArgs = tDefList[owner.Index].GenericParameters;
               break;
            case Tables.MethodDef:
               thisArgs = mDefList[owner.Index].GenericParameters;
               break;
            default:
               thisArgs = null;
               break;
         }
         if ( thisArgs != null )
         {
            thisArgs.Add( gArgInfo );
         }
         return gArgInfo;
      } ) );

      // Generic parameter constraints
      var gArgConstraints = md.GenericParameterConstraintDefinitions.TableContents;
      var gArgConstraintList = new List<GenericParameterConstraintStructuralInfo>( gArgConstraints.Count );
      gArgConstraintList.AddRange( gArgConstraints.Select( gConstraint =>
      {
         var gConstraintInfo = new GenericParameterConstraintStructuralInfo()
         {
            Constraint = state.FromTypeDefOrRefOrSpec( gConstraint.Constraint )
         };
         gArgsList[gConstraint.Owner.Index].Constraints.Add( gConstraintInfo );
         return gConstraintInfo;
      } ) );

      // Class layout
      foreach ( var layout in md.ClassLayouts.TableContents )
      {
         tDefList[layout.Parent.Index].Layout = new LayoutInfo()
         {
            ClassSize = layout.ClassSize,
            PackingSize = layout.PackingSize
         };
      }

      // Constants
      foreach ( var constant in md.ConstantDefinitions.TableContents )
      {
         var parent = constant.Parent;
         var value = constant.Value;
         switch ( parent.Table )
         {
            case Tables.Field:
               fDefList[parent.Index].ConstantValue = value;
               break;
            case Tables.Parameter:
               paramDefList[parent.Index].ConstantValue = value;
               break;
            case Tables.Property:
               propDefList[parent.Index].ConstantValue = value;
               break;
         }
      }

      // Field layouts 
      foreach ( var layout in md.FieldLayouts.TableContents )
      {
         fDefList[layout.Field.Index].FieldOffset = layout.Offset;
      }

      // Field marshals
      foreach ( var marshal in md.FieldMarshals.TableContents )
      {
         var parent = marshal.Parent;
         switch ( parent.Table )
         {
            case Tables.Field:
               fDefList[parent.Index].MarshalingInfo = marshal.NativeType;
               break;
            case Tables.Parameter:
               paramDefList[parent.Index].MarshalingInfo = marshal.NativeType;
               break;
         }
      }

      // Field RVAs
      foreach ( var rva in md.FieldRVAs.TableContents )
      {
         fDefList[rva.Field.Index].FieldData = rva.Data;
      }
      // Impl maps
      foreach ( var impl in md.MethodImplementationMaps.TableContents )
      {
         var parent = impl.MemberForwarded;
         var invokeInfo = new PInvokeInfo()
         {
            Attributes = impl.Attributes,
            PlatformInvokeName = impl.ImportName,
            PlatformInvokeModuleName = md.ModuleReferences.TableContents[impl.ImportScope.Index].ModuleName
         };

         switch ( parent.Table )
         {
            case Tables.MethodDef:
               mDefList[parent.Index].PInvokeInfo = invokeInfo;
               break;
            case Tables.Field:
               fDefList[parent.Index].PInvokeInfo = invokeInfo;
               break;
         }
      }

      // Interface Impls
      var interfaces = md.InterfaceImplementations.TableContents;
      var interfaceList = new List<InterfaceImplStructuralInfo>( interfaces.Count );
      interfaceList.AddRange( interfaces.Select( iFace =>
      {
         var iFaceInfo = new InterfaceImplStructuralInfo()
         {
            InterfaceType = state.FromTypeDefOrRefOrSpec( iFace.Interface )
         };
         tDefList[iFace.Class.Index].ImplementedInterfaces.Add( iFaceInfo );
         return iFaceInfo;
      } ) );

      // DeclSecurity
      var security = md.SecurityDefinitions.TableContents;
      var securityList = new List<SecurityStructuralInfo>( security.Count );
      securityList.AddRange( security.Select( sec =>
      {
         var secInfo = new SecurityStructuralInfo( sec );
         var parent = sec.Parent;
         switch ( parent.Table )
         {
            case Tables.TypeDef:
               tDefList[parent.Index].SecurityInfo = secInfo;
               break;
            case Tables.MethodDef:
               mDefList[parent.Index].SecurityInfo = secInfo;
               break;
            case Tables.Assembly:
               assembly.SecurityInfo = secInfo;
               break;
         }
         return secInfo;
      } ) );

      // ManifestResource
      var resources = md.ManifestResources.TableContents;
      var resourceList = new List<ManifestResourceStructuralInfo>( resources.Count );
      resourceList.AddRange( resources.Select( res =>
      {
         var resourceInfo = new ManifestResourceStructuralInfo( res );
         var implNullable = res.Implementation;
         ManifestResourceData data;
         if ( implNullable.HasValue )
         {
            var impl = implNullable.Value;
            switch ( impl.Table )
            {
               case Tables.File:
                  data = new ManifestResourceDataFile()
                  {
                     FileReference = state.FileRefs[impl.Index]
                  };
                  break;
               case Tables.AssemblyRef:
                  data = new ManifestResourceDataAssemblyRef()
                  {
                     AssemblyRef = state.AssemblyRefs[impl.Index]
                  };
                  break;
               default:
                  data = null;
                  break;
            }
         }
         else
         {
            data = new ManifestResourceDataEmbedded()
            {
               Data = res.DataInCurrentFile.CreateBlockCopy()
            };
         }
         if ( data != null )
         {
            resourceInfo.ManifestData = data;
         }

         return resourceInfo;
      } ) );

      // MemberRefs
      var memberRefs = md.MemberReferences.TableContents;
      var memberRefList = new List<MemberReferenceStructuralInfo>( memberRefs.Count );
      memberRefList.AddRange( memberRefs.Select( mRef =>
      {
         var mRefInfo = new MemberReferenceStructuralInfo( mRef );
         mRefInfo.Signature = state.CreateStructureSignature( mRef.Signature );
         var parent = mRef.DeclaringType;
         MemberReferenceParent parentInfo;
         switch ( parent.Table )
         {
            case Tables.MethodDef:
               parentInfo = new MemberReferenceParentMethodDef()
               {
                  Method = mDefList[parent.Index]
               };
               break;
            case Tables.ModuleRef:
               parentInfo = new MemberReferenceParentModuleRef()
               {
                  ModuleRef = state.ModuleRefs[parent.Index]
               };
               break;
            case Tables.TypeDef:
            case Tables.TypeRef:
            case Tables.TypeSpec:
               parentInfo = new MemberReferenceParentType()
               {
                  Type = state.FromTypeDefOrRefOrSpec( parent )
               };
               break;
            default:
               parentInfo = null;
               break;
         }

         if ( parentInfo != null )
         {
            mRefInfo.Parent = parentInfo;
         }
         return mRefInfo;
      } ) );
      // MethodImpl
      foreach ( var impl in md.MethodImplementations.TableContents )
      {
         tDefList[impl.Class.Index].OverriddenMethods.Add( new OverriddenMethodInfo( FromMethodDefOrMemberRef( mDefList, memberRefList, impl.MethodBody ), FromMethodDefOrMemberRef( mDefList, memberRefList, impl.MethodDeclaration ) ) );
      }
      // StandaloneSig
      var standaloneSigs = md.StandaloneSignatures.TableContents;
      var standaloneSigList = new List<StandaloneSignatureStructure>( standaloneSigs.Count );
      standaloneSigList.AddRange( standaloneSigs.Select( sig => new StandaloneSignatureStructure()
      {
         Signature = state.CreateStructureSignature( sig.Signature )
      } ) );
      // MethodSpec
      var methodSpecs = md.MethodSpecifications.TableContents;
      var methodSpecList = new List<MethodSpecificationStructure>( methodSpecs.Count );
      methodSpecList.AddRange( methodSpecs.Select( mSpec => new MethodSpecificationStructure()
      {
         Signature = (GenericMethodStructureSignature) state.CreateStructureSignature( mSpec.Signature ),
         Method = FromMethodDefOrMemberRef( mDefList, memberRefList, mSpec.Method )
      } ) );
      // Custom Attributes
      foreach ( var ca in md.CustomAttributeDefinitions.TableContents )
      {
         var parent = ca.Parent;
         StructuralElementWithCustomAttributes parentInfo;
         switch ( parent.Table )
         {
            case Tables.TypeDef:
               parentInfo = state.TypeDefDescriptions[parent.Index];
               break;
            case Tables.ExportedType:
               parentInfo = state.ExportedTypes[parent.Index];
               break;
            case Tables.MethodDef:
               parentInfo = mDefList[parent.Index];
               break;
            case Tables.Field:
               parentInfo = fDefList[parent.Index];
               break;
            case Tables.Parameter:
               parentInfo = paramDefList[parent.Index];
               break;
            case Tables.Module:
               parentInfo = module;
               break;
            case Tables.Property:
               parentInfo = propDefList[parent.Index];
               break;
            case Tables.Event:
               parentInfo = evtDefList[parent.Index];
               break;
            case Tables.Assembly:
               parentInfo = assembly;
               break;
            case Tables.GenericParameter:
               parentInfo = gArgsList[parent.Index];
               break;
            case Tables.TypeRef:
               parentInfo = state.TypeRefDescriptions[parent.Index];
               break;
            case Tables.InterfaceImpl:
               parentInfo = interfaceList[parent.Index];
               break;
            case Tables.MemberRef:
               parentInfo = memberRefList[parent.Index];
               break;
            case Tables.DeclSecurity:
               parentInfo = securityList[parent.Index];
               break;
            case Tables.StandaloneSignature:
               parentInfo = standaloneSigList[parent.Index];
               break;
            case Tables.ModuleRef:
               parentInfo = state.ModuleRefs[parent.Index];
               break;
            case Tables.TypeSpec:
               parentInfo = state.TypeSpecDescriptions[parent.Index];
               break;
            case Tables.AssemblyRef:
               parentInfo = state.AssemblyRefs[parent.Index];
               break;
            case Tables.File:
               parentInfo = state.FileRefs[parent.Index];
               break;
            case Tables.ManifestResource:
               parentInfo = resourceList[parent.Index];
               break;
            case Tables.GenericParameterConstraint:
               parentInfo = gArgConstraintList[parent.Index];
               break;
            case Tables.MethodSpec:
               parentInfo = methodSpecList[parent.Index];
               break;
            default:
               parentInfo = null;
               break;
         }
         parentInfo.CustomAttributes.Add( new CustomAttributeStructure()
         {
            Constructor = FromMethodDefOrMemberRef( mDefList, memberRefList, ca.Type ),
            Signature = ca.Signature // TODO clone
         } );
      }
      // IL

      return resourceList;
   }

   private static AbstractStructureSignature CreateStructureSignature( this StructuralCreationState state, AbstractSignature signature )
   {
      if ( signature == null )
      {
         return null;
      }
      else
      {
         switch ( signature.SignatureKind )
         {
            case SignatureKind.Field:
               return state.CloneFieldSignature( (FieldSignature) signature );
            case SignatureKind.GenericMethodInstantiation:
               return state.CloneGenericMethodSignature( (GenericMethodSignature) signature );
            case SignatureKind.LocalVariables:
               return state.CloneLocalsSignature( (LocalVariablesSignature) signature );
            case SignatureKind.MethodDefinition:
               return state.CloneMethodDefSignature( (MethodDefinitionSignature) signature );
            case SignatureKind.MethodReference:
               return state.CloneMethodRefSignature( (MethodReferenceSignature) signature );
            case SignatureKind.Property:
               return state.ClonePropertySignature( (PropertySignature) signature );
            case SignatureKind.Type:
               return state.CloneTypeSignature( (TypeSignature) signature );
            case SignatureKind.RawSignature:
               throw new NotSupportedException( "Raw signatures are not supported in structural layer." );
            default:
               throw new InvalidOperationException( "Invalid physical signature kind: " + signature.SignatureKind + "." );
         }
      }
   }

   private static AbstractTypeDescription FromTypeDefOrRefOrSpec( this StructuralCreationState moduleInfo, TableIndex index )
   {
      switch ( index.Table )
      {
         case Tables.TypeDef:
            return moduleInfo.TypeDefDescriptions[index.Index];
         case Tables.TypeRef:
            return moduleInfo.TypeRefDescriptions[index.Index];
         case Tables.TypeSpec:
            return moduleInfo.TypeSpecDescriptions[index.Index];
         default:
            throw new InvalidOperationException( "Unsupported TypeDef/Ref/Spec: " + index + "." );
      }
   }

   private static MethodDefOrMemberRefStructure FromMethodDefOrMemberRef( List<MethodStructureInfo> mDefList, List<MemberReferenceStructuralInfo> mRefList, TableIndex index )
   {
      switch ( index.Table )
      {
         case Tables.MethodDef:
            return mDefList[index.Index];
         case Tables.MemberRef:
            return mRefList[index.Index];
         default:
            return null;
      }
   }

   private static TypeStructureSignature CloneTypeSignature( this StructuralCreationState state, TypeSignature sig )
   {
      TypeStructureSignature retVal;
      switch ( sig.TypeSignatureKind )
      {
         case TypeSignatureKind.ClassOrValue:
            var clazz = (ClassOrValueTypeSignature) sig;
            var clazzClone = new ClassOrValueTypeStructureSignature( clazz.GenericArguments.Count )
            {
               IsClass = clazz.IsClass,
               Type = state.FromTypeDefOrRefOrSpec( clazz.Type )
            };
            clazzClone.GenericArguments.AddRange( clazz.GenericArguments.Select( gArg => state.CloneTypeSignature( gArg ) ) );
            retVal = clazzClone;
            break;
         case TypeSignatureKind.ComplexArray:
            var cArray = (ComplexArrayTypeSignature) sig;
            var cClone = new ComplexArrayTypeStructureSignature( cArray.Sizes.Count, cArray.LowerBounds.Count )
            {
               Rank = cArray.Rank,
               ArrayType = state.CloneTypeSignature( cArray.ArrayType )
            };
            cClone.LowerBounds.AddRange( cArray.LowerBounds );
            cClone.Sizes.AddRange( cArray.Sizes );
            retVal = cClone;
            break;
         case TypeSignatureKind.FunctionPointer:
            retVal = new FunctionPointerTypeStructureSignature()
            {
               MethodSignature = state.CloneMethodRefSignature( ( (FunctionPointerTypeSignature) sig ).MethodSignature )
            };
            break;
         case TypeSignatureKind.Pointer:
            var ptr = (PointerTypeSignature) sig;
            var ptrClone = new PointerTypeStructureSignature( ptr.CustomModifiers.Count )
            {
               PointerType = state.CloneTypeSignature( ptr.PointerType )
            };
            state.CloneCustomMods( ptr.CustomModifiers, ptrClone.CustomModifiers );
            retVal = ptrClone;
            break;
         case TypeSignatureKind.GenericParameter:
            var gParam = (GenericParameterTypeSignature) sig;
            retVal = new GenericParameterTypeStructureSignature()
            {
               GenericParameterIndex = gParam.GenericParameterIndex,
               IsTypeParameter = gParam.IsTypeParameter,
            };
            break;
         case TypeSignatureKind.Simple:
            retVal = SimpleTypeStructureSignature.GetByElement( ( (SimpleTypeSignature) sig ).SimpleType );
            break;
         case TypeSignatureKind.SimpleArray:
            var array = (SimpleArrayTypeSignature) sig;
            var clone = new SimpleArrayTypeStructureSignature( array.CustomModifiers.Count )
            {
               ArrayType = state.CloneTypeSignature( array.ArrayType )
            };
            state.CloneCustomMods( array.CustomModifiers, clone.CustomModifiers );
            retVal = clone;
            break;
         default:
            throw new NotSupportedException( "Invalid type signature kind: " + sig.TypeSignatureKind );
      }

      return retVal;
   }

   private static void PopulateAbstractMethodSignature( this StructuralCreationState state, AbstractMethodSignature original, AbstractMethodStructureSignature clone )
   {
      clone.GenericArgumentCount = original.GenericArgumentCount;
      clone.SignatureStarter = original.SignatureStarter;
      clone.ReturnType = state.CloneParameterSignature( original.ReturnType );
      clone.Parameters.AddRange( original.Parameters.Select( p => state.CloneParameterSignature( p ) ) );
   }

   private static MethodReferenceStructureSignature CloneMethodRefSignature( this StructuralCreationState state, MethodReferenceSignature methodRef )
   {
      var retVal = new MethodReferenceStructureSignature( methodRef.Parameters.Count, methodRef.VarArgsParameters.Count );
      state.PopulateAbstractMethodSignature( methodRef, retVal );
      retVal.VarArgsParameters.AddRange( methodRef.VarArgsParameters.Select( p => state.CloneParameterSignature( p ) ) );
      return retVal;
   }

   private static MethodDefinitionStructureSignature CloneMethodDefSignature( this StructuralCreationState state, MethodDefinitionSignature methodDef )
   {
      var retVal = new MethodDefinitionStructureSignature( methodDef.Parameters.Count );
      state.PopulateAbstractMethodSignature( methodDef, retVal );
      return retVal;
   }

   private static ParameterStructureSignature CloneParameterSignature( this StructuralCreationState state, ParameterSignature paramSig )
   {
      var retVal = new ParameterStructureSignature( paramSig.CustomModifiers.Count )
      {
         IsByRef = paramSig.IsByRef,
         Type = state.CloneTypeSignature( paramSig.Type )
      };
      state.CloneCustomMods( paramSig.CustomModifiers, retVal.CustomModifiers );
      return retVal;
   }

   private static GenericMethodStructureSignature CloneGenericMethodSignature( this StructuralCreationState state, GenericMethodSignature gSig )
   {
      var retVal = new GenericMethodStructureSignature( gSig.GenericArguments.Count );
      retVal.GenericArguments.AddRange( gSig.GenericArguments.Select( gArg => state.CloneTypeSignature( gArg ) ) );
      return retVal;
   }

   private static FieldStructureSignature CloneFieldSignature( this StructuralCreationState state, FieldSignature sig )
   {
      var retVal = new FieldStructureSignature( sig.CustomModifiers.Count );
      retVal.Type = state.CloneTypeSignature( sig.Type );
      state.CloneCustomMods( sig.CustomModifiers, retVal.CustomModifiers );
      return retVal;
   }

   private static LocalVariablesStructureSignature CloneLocalsSignature( this StructuralCreationState state, LocalVariablesSignature locals )
   {
      var retVal = new LocalVariablesStructureSignature( locals.Locals.Count );
      retVal.Locals.AddRange( locals.Locals.Select( l => state.CloneLocalSignature( l ) ) );
      return retVal;
   }

   private static LocalVariableStructureSignature CloneLocalSignature( this StructuralCreationState state, LocalVariableSignature local )
   {
      var retVal = new LocalVariableStructureSignature( local.CustomModifiers.Count )
      {
         IsByRef = local.IsByRef,
         IsPinned = local.IsPinned,
         Type = state.CloneTypeSignature( local.Type )
      };
      state.CloneCustomMods( local.CustomModifiers, retVal.CustomModifiers );
      return retVal;
   }

   private static PropertyStructureSignature ClonePropertySignature( this StructuralCreationState state, PropertySignature sig )
   {
      var retVal = new PropertyStructureSignature( sig.CustomModifiers.Count, sig.Parameters.Count )
      {
         HasThis = sig.HasThis,
         PropertyType = state.CloneTypeSignature( sig.PropertyType )
      };
      state.CloneCustomMods( sig.CustomModifiers, retVal.CustomModifiers );
      retVal.Parameters.AddRange( sig.Parameters.Select( p => state.CloneParameterSignature( p ) ) );
      return retVal;
   }

   private static void CloneCustomMods( this StructuralCreationState state, List<CustomModifierSignature> original, List<CustomModifierStructureSignature> newMods )
   {
      newMods.AddRange( original.Select( cm => new CustomModifierStructureSignature()
      {
         IsOptional = cm.IsOptional,
         CustomModifierType = state.FromTypeDefOrRefOrSpec( cm.CustomModifierType )
      } ) );
   }
}