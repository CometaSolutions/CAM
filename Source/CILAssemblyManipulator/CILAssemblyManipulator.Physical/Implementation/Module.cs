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
using CommonUtils;

namespace CILAssemblyManipulator.Physical.Implementation
{
   internal sealed class CILMetadataImpl : CILMetaData
   {
      private static readonly Int32[] EMPTY_SIZES = Enumerable.Repeat( 0, Consts.AMOUNT_OF_TABLES ).ToArray();

      private readonly MetaDataTableImpl<ModuleDefinition> _moduleDefinitions;
      private readonly MetaDataTableImpl<TypeReference> _typeReferences;
      private readonly MetaDataTableImpl<TypeDefinition> _typeDefinitions;
      private readonly MetaDataTableImpl<FieldDefinition> _fieldDefinitions;
      private readonly MetaDataTableImpl<MethodDefinition> _methodDefinitions;
      private readonly MetaDataTableImpl<ParameterDefinition> _parameterDefinitions;
      private readonly MetaDataTableImpl<InterfaceImplementation> _interfaceImplementations;
      private readonly MetaDataTableImpl<MemberReference> _memberReferences;
      private readonly MetaDataTableImpl<ConstantDefinition> _constantDefinitions;
      private readonly MetaDataTableImpl<CustomAttributeDefinition> _customAttributeDefinitions;
      private readonly MetaDataTableImpl<FieldMarshal> _fieldMarshals;
      private readonly MetaDataTableImpl<SecurityDefinition> _securityDefinitions;
      private readonly MetaDataTableImpl<ClassLayout> _classLayouts;
      private readonly MetaDataTableImpl<FieldLayout> _fieldLayouts;
      private readonly MetaDataTableImpl<StandaloneSignature> _standaloneSignatures;
      private readonly MetaDataTableImpl<EventMap> _eventMaps;
      private readonly MetaDataTableImpl<EventDefinition> _eventDefinitions;
      private readonly MetaDataTableImpl<PropertyMap> _propertyMaps;
      private readonly MetaDataTableImpl<PropertyDefinition> _propertyDefinitions;
      private readonly MetaDataTableImpl<MethodSemantics> _methodSemantics;
      private readonly MetaDataTableImpl<MethodImplementation> _methodImplementations;
      private readonly MetaDataTableImpl<ModuleReference> _moduleReferences;
      private readonly MetaDataTableImpl<TypeSpecification> _typeSpecifications;
      private readonly MetaDataTableImpl<MethodImplementationMap> _methodImplementationMaps;
      private readonly MetaDataTableImpl<FieldRVA> _fieldRVAs;
      private readonly MetaDataTableImpl<AssemblyDefinition> _assemblyDefinitions;
      private readonly MetaDataTableImpl<AssemblyReference> _assemblyReferences;
      private readonly MetaDataTableImpl<FileReference> _fileReferences;
      private readonly MetaDataTableImpl<ExportedType> _exportedTypess;
      private readonly MetaDataTableImpl<ManifestResource> _manifestResources;
      private readonly MetaDataTableImpl<NestedClassDefinition> _nestedClassDefinitions;
      private readonly MetaDataTableImpl<GenericParameterDefinition> _genericParameterDefinitions;
      private readonly MetaDataTableImpl<MethodSpecification> _methodSpecifications;
      private readonly MetaDataTableImpl<GenericParameterConstraintDefinition> _genericParameterConstraintDefinitions;
      private readonly MetaDataTableImpl<EditAndContinueLog> _editAndContinueLog;
      private readonly MetaDataTableImpl<EditAndContinueMap> _editAndContinueMap;
#pragma warning disable 618
      private readonly MetaDataTableImpl<FieldDefinitionPointer> _fieldDefinitionPointers;
      private readonly MetaDataTableImpl<MethodDefinitionPointer> _methodDefinitionPointers;
      private readonly MetaDataTableImpl<ParameterPointer> _parameterDefinitionPointers;
      private readonly MetaDataTableImpl<EventPointer> _eventPointers;
      private readonly MetaDataTableImpl<PropertyPointer> _propertyPointers;
      private readonly MetaDataTableImpl<AssemblyDefinitionProcessor> _assemblyDefinitionProcessors;
      private readonly MetaDataTableImpl<AssemblyDefinitionOS> _assemblyDefinitionOSs;
      private readonly MetaDataTableImpl<AssemblyReferenceProcessor> _assemblyReferenceProcessors;
      private readonly MetaDataTableImpl<AssemblyReferenceOS> _assemblyReferenceOSs;

      internal CILMetadataImpl()
         : this( EMPTY_SIZES )
      {

      }

      internal CILMetadataImpl( Int32[] sizes )
      {
         this._moduleDefinitions = new MetaDataTableImpl<ModuleDefinition>( Tables.Module, sizes[(Int32) Tables.Module] );
         this._typeReferences = new MetaDataTableImpl<TypeReference>( Tables.TypeRef, sizes[(Int32) Tables.TypeRef] );
         this._typeDefinitions = new MetaDataTableImpl<TypeDefinition>( Tables.TypeDef, sizes[(Int32) Tables.TypeDef] );
         this._fieldDefinitionPointers = new MetaDataTableImpl<FieldDefinitionPointer>( Tables.FieldPtr, sizes[(Int32) Tables.FieldPtr] );
         this._fieldDefinitions = new MetaDataTableImpl<FieldDefinition>( Tables.Field, sizes[(Int32) Tables.Field] );
         this._methodDefinitionPointers = new MetaDataTableImpl<MethodDefinitionPointer>( Tables.MethodPtr, sizes[(Int32) Tables.MethodPtr] );
         this._methodDefinitions = new MetaDataTableImpl<MethodDefinition>( Tables.MethodDef, sizes[(Int32) Tables.MethodDef] );
         this._parameterDefinitionPointers = new MetaDataTableImpl<ParameterPointer>( Tables.ParameterPtr, sizes[(Int32) Tables.ParameterPtr] );
         this._parameterDefinitions = new MetaDataTableImpl<ParameterDefinition>( Tables.Parameter, sizes[(Int32) Tables.Parameter] );
         this._interfaceImplementations = new MetaDataTableImpl<InterfaceImplementation>( Tables.InterfaceImpl, sizes[(Int32) Tables.InterfaceImpl] );
         this._memberReferences = new MetaDataTableImpl<MemberReference>( Tables.MemberRef, sizes[(Int32) Tables.MemberRef] );
         this._constantDefinitions = new MetaDataTableImpl<ConstantDefinition>( Tables.Constant, sizes[(Int32) Tables.Constant] );
         this._customAttributeDefinitions = new MetaDataTableImpl<CustomAttributeDefinition>( Tables.CustomAttribute, sizes[(Int32) Tables.CustomAttribute] );
         this._fieldMarshals = new MetaDataTableImpl<FieldMarshal>( Tables.FieldMarshal, sizes[(Int32) Tables.FieldMarshal] );
         this._securityDefinitions = new MetaDataTableImpl<SecurityDefinition>( Tables.DeclSecurity, sizes[(Int32) Tables.DeclSecurity] );
         this._classLayouts = new MetaDataTableImpl<ClassLayout>( Tables.ClassLayout, sizes[(Int32) Tables.ClassLayout] );
         this._fieldLayouts = new MetaDataTableImpl<FieldLayout>( Tables.FieldLayout, sizes[(Int32) Tables.FieldLayout] );
         this._standaloneSignatures = new MetaDataTableImpl<StandaloneSignature>( Tables.StandaloneSignature, sizes[(Int32) Tables.StandaloneSignature] );
         this._eventMaps = new MetaDataTableImpl<EventMap>( Tables.EventMap, sizes[(Int32) Tables.EventMap] );
         this._eventPointers = new MetaDataTableImpl<EventPointer>( Tables.EventPtr, sizes[(Int32) Tables.EventPtr] );
         this._eventDefinitions = new MetaDataTableImpl<EventDefinition>( Tables.Event, sizes[(Int32) Tables.Event] );
         this._propertyMaps = new MetaDataTableImpl<PropertyMap>( Tables.PropertyMap, sizes[(Int32) Tables.PropertyMap] );
         this._propertyPointers = new MetaDataTableImpl<PropertyPointer>( Tables.PropertyPtr, sizes[(Int32) Tables.PropertyPtr] );
         this._propertyDefinitions = new MetaDataTableImpl<PropertyDefinition>( Tables.Property, sizes[(Int32) Tables.Property] );
         this._methodSemantics = new MetaDataTableImpl<MethodSemantics>( Tables.MethodSemantics, sizes[(Int32) Tables.MethodSemantics] );
         this._methodImplementations = new MetaDataTableImpl<MethodImplementation>( Tables.MethodImpl, sizes[(Int32) Tables.MethodImpl] );
         this._moduleReferences = new MetaDataTableImpl<ModuleReference>( Tables.ModuleRef, sizes[(Int32) Tables.ModuleRef] );
         this._typeSpecifications = new MetaDataTableImpl<TypeSpecification>( Tables.TypeSpec, sizes[(Int32) Tables.TypeSpec] );
         this._methodImplementationMaps = new MetaDataTableImpl<MethodImplementationMap>( Tables.ImplMap, sizes[(Int32) Tables.ImplMap] );
         this._fieldRVAs = new MetaDataTableImpl<FieldRVA>( Tables.FieldRVA, sizes[(Int32) Tables.FieldRVA] );
         this._editAndContinueLog = new MetaDataTableImpl<EditAndContinueLog>( Tables.EncLog, sizes[(Int32) Tables.EncLog] );
         this._editAndContinueMap = new MetaDataTableImpl<EditAndContinueMap>( Tables.EncMap, sizes[(Int32) Tables.EncMap] );
         this._assemblyDefinitions = new MetaDataTableImpl<AssemblyDefinition>( Tables.Assembly, sizes[(Int32) Tables.Assembly] );
         this._assemblyDefinitionProcessors = new MetaDataTableImpl<AssemblyDefinitionProcessor>( Tables.AssemblyProcessor, sizes[(Int32) Tables.AssemblyProcessor] );
         this._assemblyDefinitionOSs = new MetaDataTableImpl<AssemblyDefinitionOS>( Tables.AssemblyOS, sizes[(Int32) Tables.AssemblyOS] );
         this._assemblyReferences = new MetaDataTableImpl<AssemblyReference>( Tables.AssemblyRef, sizes[(Int32) Tables.AssemblyRef] );
         this._assemblyReferenceProcessors = new MetaDataTableImpl<AssemblyReferenceProcessor>( Tables.AssemblyRefProcessor, sizes[(Int32) Tables.AssemblyRefProcessor] );
         this._assemblyReferenceOSs = new MetaDataTableImpl<AssemblyReferenceOS>( Tables.AssemblyRefOS, sizes[(Int32) Tables.AssemblyRefOS] );
         this._fileReferences = new MetaDataTableImpl<FileReference>( Tables.File, sizes[(Int32) Tables.File] );
         this._exportedTypess = new MetaDataTableImpl<ExportedType>( Tables.ExportedType, sizes[(Int32) Tables.ExportedType] );
         this._manifestResources = new MetaDataTableImpl<ManifestResource>( Tables.ManifestResource, sizes[(Int32) Tables.ManifestResource] );
         this._nestedClassDefinitions = new MetaDataTableImpl<NestedClassDefinition>( Tables.NestedClass, sizes[(Int32) Tables.NestedClass] );
         this._genericParameterDefinitions = new MetaDataTableImpl<GenericParameterDefinition>( Tables.GenericParameter, sizes[(Int32) Tables.GenericParameter] );
         this._methodSpecifications = new MetaDataTableImpl<MethodSpecification>( Tables.MethodSpec, sizes[(Int32) Tables.MethodSpec] );
         this._genericParameterConstraintDefinitions = new MetaDataTableImpl<GenericParameterConstraintDefinition>( Tables.GenericParameterConstraint, sizes[(Int32) Tables.GenericParameterConstraint] );
      }
#pragma warning restore 618

      public MetaDataTable<ModuleDefinition> ModuleDefinitions
      {
         get
         {
            return this._moduleDefinitions;
         }
      }

      public MetaDataTable<TypeReference> TypeReferences
      {
         get
         {
            return this._typeReferences;
         }
      }

      public MetaDataTable<TypeDefinition> TypeDefinitions
      {
         get
         {
            return this._typeDefinitions;
         }
      }

      public MetaDataTable<FieldDefinition> FieldDefinitions
      {
         get
         {
            return this._fieldDefinitions;
         }
      }

      public MetaDataTable<MethodDefinition> MethodDefinitions
      {
         get
         {
            return this._methodDefinitions;
         }
      }

      public MetaDataTable<ParameterDefinition> ParameterDefinitions
      {
         get
         {
            return this._parameterDefinitions;
         }
      }

      public MetaDataTable<InterfaceImplementation> InterfaceImplementations
      {
         get
         {
            return this._interfaceImplementations;
         }
      }

      public MetaDataTable<MemberReference> MemberReferences
      {
         get
         {
            return this._memberReferences;
         }
      }

      public MetaDataTable<ConstantDefinition> ConstantDefinitions
      {
         get
         {
            return this._constantDefinitions;
         }
      }

      public MetaDataTable<CustomAttributeDefinition> CustomAttributeDefinitions
      {
         get
         {
            return this._customAttributeDefinitions;
         }
      }

      public MetaDataTable<FieldMarshal> FieldMarshals
      {
         get
         {
            return this._fieldMarshals;
         }
      }

      public MetaDataTable<SecurityDefinition> SecurityDefinitions
      {
         get
         {
            return this._securityDefinitions;
         }
      }

      public MetaDataTable<ClassLayout> ClassLayouts
      {
         get
         {
            return this._classLayouts;
         }
      }

      public MetaDataTable<FieldLayout> FieldLayouts
      {
         get
         {
            return this._fieldLayouts;
         }
      }

      public MetaDataTable<StandaloneSignature> StandaloneSignatures
      {
         get
         {
            return this._standaloneSignatures;
         }
      }

      public MetaDataTable<EventMap> EventMaps
      {
         get
         {
            return this._eventMaps;
         }
      }

      public MetaDataTable<EventDefinition> EventDefinitions
      {
         get
         {
            return this._eventDefinitions;
         }
      }

      public MetaDataTable<PropertyMap> PropertyMaps
      {
         get
         {
            return this._propertyMaps;
         }
      }

      public MetaDataTable<PropertyDefinition> PropertyDefinitions
      {
         get
         {
            return this._propertyDefinitions;
         }
      }

      public MetaDataTable<MethodSemantics> MethodSemantics
      {
         get
         {
            return this._methodSemantics;
         }
      }

      public MetaDataTable<MethodImplementation> MethodImplementations
      {
         get
         {
            return this._methodImplementations;
         }
      }

      public MetaDataTable<ModuleReference> ModuleReferences
      {
         get
         {
            return this._moduleReferences;
         }
      }

      public MetaDataTable<TypeSpecification> TypeSpecifications
      {
         get
         {
            return this._typeSpecifications;
         }
      }

      public MetaDataTable<MethodImplementationMap> MethodImplementationMaps
      {
         get
         {
            return this._methodImplementationMaps;
         }
      }

      public MetaDataTable<FieldRVA> FieldRVAs
      {
         get
         {
            return this._fieldRVAs;
         }
      }

      public MetaDataTable<AssemblyDefinition> AssemblyDefinitions
      {
         get
         {
            return this._assemblyDefinitions;
         }
      }

      public MetaDataTable<AssemblyReference> AssemblyReferences
      {
         get
         {
            return this._assemblyReferences;
         }
      }

      public MetaDataTable<FileReference> FileReferences
      {
         get
         {
            return this._fileReferences;
         }
      }

      public MetaDataTable<ExportedType> ExportedTypes
      {
         get
         {
            return this._exportedTypess;
         }
      }

      public MetaDataTable<ManifestResource> ManifestResources
      {
         get
         {
            return this._manifestResources;
         }
      }

      public MetaDataTable<NestedClassDefinition> NestedClassDefinitions
      {
         get
         {
            return this._nestedClassDefinitions;
         }
      }

      public MetaDataTable<GenericParameterDefinition> GenericParameterDefinitions
      {
         get
         {
            return this._genericParameterDefinitions;
         }
      }

      public MetaDataTable<MethodSpecification> MethodSpecifications
      {
         get
         {
            return this._methodSpecifications;
         }
      }

      public MetaDataTable<GenericParameterConstraintDefinition> GenericParameterConstraintDefinitions
      {
         get
         {
            return this._genericParameterConstraintDefinitions;
         }
      }

      public MetaDataTable<EditAndContinueLog> EditAndContinueLog
      {
         get
         {
            return this._editAndContinueLog;
         }
      }

      public MetaDataTable<EditAndContinueMap> EditAndContinueMap
      {
         get
         {
            return this._editAndContinueMap;
         }
      }

#pragma warning disable 618
      public MetaDataTable<FieldDefinitionPointer> FieldDefinitionPointers
      {
         get
         {
            return this._fieldDefinitionPointers;
         }
      }

      public MetaDataTable<MethodDefinitionPointer> MethodDefinitionPointers
      {
         get
         {
            return this._methodDefinitionPointers;
         }
      }

      public MetaDataTable<ParameterPointer> ParameterDefinitionPointers
      {
         get
         {
            return this._parameterDefinitionPointers;
         }
      }

      public MetaDataTable<EventPointer> EventPointers
      {
         get
         {
            return this._eventPointers;
         }
      }

      public MetaDataTable<PropertyPointer> PropertyPointers
      {
         get
         {
            return this._propertyPointers;
         }
      }

      public MetaDataTable<AssemblyDefinitionProcessor> AssemblyDefinitionProcessors
      {
         get
         {
            return this._assemblyDefinitionProcessors;
         }
      }

      public MetaDataTable<AssemblyDefinitionOS> AssemblyDefinitionOSs
      {
         get
         {
            return this._assemblyDefinitionOSs;
         }
      }

      public MetaDataTable<AssemblyReferenceProcessor> AssemblyReferenceProcessors
      {
         get
         {
            return this._assemblyReferenceProcessors;
         }
      }

      public MetaDataTable<AssemblyReferenceOS> AssemblyReferenceOSs
      {
         get
         {
            return this._assemblyReferenceOSs;
         }
      }
#pragma warning restore 618
   }

   internal sealed class MetaDataTableImpl<TRow> : MetaDataTable<TRow>
      where TRow : class
   {
      private readonly Tables _tableKind;
      private readonly List<TRow> _table;

      internal MetaDataTableImpl( Tables tableKind, Int32 tableRowCapacity )
      {
         this._tableKind = tableKind;
         this._table = new List<TRow>( tableRowCapacity );
      }

      public List<TRow> TableContents
      {
         get
         {
            return this._table;
         }
      }

      public Tables TableKind
      {
         get
         {
            return this._tableKind;
         }
      }

      public Int32 RowCount
      {
         get
         {
            return this._table.Count;
         }
      }

      public IEnumerable<Object> TableContentsAsEnumerable
      {
         get
         {
            return this._table;
         }
      }


      public Object GetRowAt( Int32 idx )
      {
         return this._table[idx];
      }

      public override String ToString()
      {
         return this._tableKind + ", row count: " + this._table.Count + ".";
      }
   }
}
