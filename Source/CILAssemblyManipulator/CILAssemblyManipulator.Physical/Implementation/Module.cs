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
using CILAssemblyManipulator.Physical.Meta;
using CommonUtils;
using TabularMetaData.Meta;
using TabularMetaData;

namespace CILAssemblyManipulator.Physical.Implementation
{
   internal sealed class CILMetadataImpl : TabularMetaDataWithSchemaImpl, CILMetaData
   {
      private readonly MetaDataTable<ModuleDefinition> _moduleDefinitions;
      private readonly MetaDataTable<TypeReference> _typeReferences;
      private readonly MetaDataTable<TypeDefinition> _typeDefinitions;
      private readonly MetaDataTable<FieldDefinition> _fieldDefinitions;
      private readonly MetaDataTable<MethodDefinition> _methodDefinitions;
      private readonly MetaDataTable<ParameterDefinition> _parameterDefinitions;
      private readonly MetaDataTable<InterfaceImplementation> _interfaceImplementations;
      private readonly MetaDataTable<MemberReference> _memberReferences;
      private readonly MetaDataTable<ConstantDefinition> _constantDefinitions;
      private readonly MetaDataTable<CustomAttributeDefinition> _customAttributeDefinitions;
      private readonly MetaDataTable<FieldMarshal> _fieldMarshals;
      private readonly MetaDataTable<SecurityDefinition> _securityDefinitions;
      private readonly MetaDataTable<ClassLayout> _classLayouts;
      private readonly MetaDataTable<FieldLayout> _fieldLayouts;
      private readonly MetaDataTable<StandaloneSignature> _standaloneSignatures;
      private readonly MetaDataTable<EventMap> _eventMaps;
      private readonly MetaDataTable<EventDefinition> _eventDefinitions;
      private readonly MetaDataTable<PropertyMap> _propertyMaps;
      private readonly MetaDataTable<PropertyDefinition> _propertyDefinitions;
      private readonly MetaDataTable<MethodSemantics> _methodSemantics;
      private readonly MetaDataTable<MethodImplementation> _methodImplementations;
      private readonly MetaDataTable<ModuleReference> _moduleReferences;
      private readonly MetaDataTable<TypeSpecification> _typeSpecifications;
      private readonly MetaDataTable<MethodImplementationMap> _methodImplementationMaps;
      private readonly MetaDataTable<FieldRVA> _fieldRVAs;
      private readonly MetaDataTable<AssemblyDefinition> _assemblyDefinitions;
      private readonly MetaDataTable<AssemblyReference> _assemblyReferences;
      private readonly MetaDataTable<FileReference> _fileReferences;
      private readonly MetaDataTable<ExportedType> _exportedTypes;
      private readonly MetaDataTable<ManifestResource> _manifestResources;
      private readonly MetaDataTable<NestedClassDefinition> _nestedClassDefinitions;
      private readonly MetaDataTable<GenericParameterDefinition> _genericParameterDefinitions;
      private readonly MetaDataTable<MethodSpecification> _methodSpecifications;
      private readonly MetaDataTable<GenericParameterConstraintDefinition> _genericParameterConstraintDefinitions;
      private readonly MetaDataTable<EditAndContinueLog> _editAndContinueLog;
      private readonly MetaDataTable<EditAndContinueMap> _editAndContinueMap;
      private readonly MetaDataTable<FieldDefinitionPointer> _fieldDefinitionPointers;
      private readonly MetaDataTable<MethodDefinitionPointer> _methodDefinitionPointers;
      private readonly MetaDataTable<ParameterDefinitionPointer> _parameterDefinitionPointers;
      private readonly MetaDataTable<EventDefinitionPointer> _eventDefinitionPointers;
      private readonly MetaDataTable<PropertyDefinitionPointer> _propertyDefinitionPointers;
#pragma warning disable 618
      private readonly MetaDataTable<AssemblyDefinitionProcessor> _assemblyDefinitionProcessors;
      private readonly MetaDataTable<AssemblyDefinitionOS> _assemblyDefinitionOSs;
      private readonly MetaDataTable<AssemblyReferenceProcessor> _assemblyReferenceProcessors;
      private readonly MetaDataTable<AssemblyReferenceOS> _assemblyReferenceOSs;

      internal CILMetadataImpl(
         MetaDataTableInformationProvider tableInfoProvider,
         Int32[] sizes,
         out MetaDataTableInformation[] infos
         )
         : base( tableInfoProvider ?? DefaultMetaDataTableInformationProvider.CreateDefault(), Consts.AMOUNT_OF_FIXED_TABLES, sizes, out infos )
      {
         MetaDataTableInformation[] defaultTableInfos = null;
         this._moduleDefinitions = CreateFixedMDTable<ModuleDefinition>( Tables.Module, sizes, infos, ref defaultTableInfos );
         this._typeReferences = CreateFixedMDTable<TypeReference>( Tables.TypeRef, sizes, infos, ref defaultTableInfos );
         this._typeDefinitions = CreateFixedMDTable<TypeDefinition>( Tables.TypeDef, sizes, infos, ref defaultTableInfos );
         this._fieldDefinitionPointers = CreateFixedMDTable<FieldDefinitionPointer>( Tables.FieldPtr, sizes, infos, ref defaultTableInfos );
         this._fieldDefinitions = CreateFixedMDTable<FieldDefinition>( Tables.Field, sizes, infos, ref defaultTableInfos );
         this._methodDefinitionPointers = CreateFixedMDTable<MethodDefinitionPointer>( Tables.MethodPtr, sizes, infos, ref defaultTableInfos );
         this._methodDefinitions = CreateFixedMDTable<MethodDefinition>( Tables.MethodDef, sizes, infos, ref defaultTableInfos );
         this._parameterDefinitionPointers = CreateFixedMDTable<ParameterDefinitionPointer>( Tables.ParameterPtr, sizes, infos, ref defaultTableInfos );
         this._parameterDefinitions = CreateFixedMDTable<ParameterDefinition>( Tables.Parameter, sizes, infos, ref defaultTableInfos );
         this._interfaceImplementations = CreateFixedMDTable<InterfaceImplementation>( Tables.InterfaceImpl, sizes, infos, ref defaultTableInfos );
         this._memberReferences = CreateFixedMDTable<MemberReference>( Tables.MemberRef, sizes, infos, ref defaultTableInfos );
         this._constantDefinitions = CreateFixedMDTable<ConstantDefinition>( Tables.Constant, sizes, infos, ref defaultTableInfos );
         this._customAttributeDefinitions = CreateFixedMDTable<CustomAttributeDefinition>( Tables.CustomAttribute, sizes, infos, ref defaultTableInfos );
         this._fieldMarshals = CreateFixedMDTable<FieldMarshal>( Tables.FieldMarshal, sizes, infos, ref defaultTableInfos );
         this._securityDefinitions = CreateFixedMDTable<SecurityDefinition>( Tables.DeclSecurity, sizes, infos, ref defaultTableInfos );
         this._classLayouts = CreateFixedMDTable<ClassLayout>( Tables.ClassLayout, sizes, infos, ref defaultTableInfos );
         this._fieldLayouts = CreateFixedMDTable<FieldLayout>( Tables.FieldLayout, sizes, infos, ref defaultTableInfos );
         this._standaloneSignatures = CreateFixedMDTable<StandaloneSignature>( Tables.StandaloneSignature, sizes, infos, ref defaultTableInfos );
         this._eventMaps = CreateFixedMDTable<EventMap>( Tables.EventMap, sizes, infos, ref defaultTableInfos );
         this._eventDefinitionPointers = CreateFixedMDTable<EventDefinitionPointer>( Tables.EventPtr, sizes, infos, ref defaultTableInfos );
         this._eventDefinitions = CreateFixedMDTable<EventDefinition>( Tables.Event, sizes, infos, ref defaultTableInfos );
         this._propertyMaps = CreateFixedMDTable<PropertyMap>( Tables.PropertyMap, sizes, infos, ref defaultTableInfos );
         this._propertyDefinitionPointers = CreateFixedMDTable<PropertyDefinitionPointer>( Tables.PropertyPtr, sizes, infos, ref defaultTableInfos );
         this._propertyDefinitions = CreateFixedMDTable<PropertyDefinition>( Tables.Property, sizes, infos, ref defaultTableInfos );
         this._methodSemantics = CreateFixedMDTable<MethodSemantics>( Tables.MethodSemantics, sizes, infos, ref defaultTableInfos );
         this._methodImplementations = CreateFixedMDTable<MethodImplementation>( Tables.MethodImpl, sizes, infos, ref defaultTableInfos );
         this._moduleReferences = CreateFixedMDTable<ModuleReference>( Tables.ModuleRef, sizes, infos, ref defaultTableInfos );
         this._typeSpecifications = CreateFixedMDTable<TypeSpecification>( Tables.TypeSpec, sizes, infos, ref defaultTableInfos );
         this._methodImplementationMaps = CreateFixedMDTable<MethodImplementationMap>( Tables.ImplMap, sizes, infos, ref defaultTableInfos );
         this._fieldRVAs = CreateFixedMDTable<FieldRVA>( Tables.FieldRVA, sizes, infos, ref defaultTableInfos );
         this._editAndContinueLog = CreateFixedMDTable<EditAndContinueLog>( Tables.EncLog, sizes, infos, ref defaultTableInfos );
         this._editAndContinueMap = CreateFixedMDTable<EditAndContinueMap>( Tables.EncMap, sizes, infos, ref defaultTableInfos );
         this._assemblyDefinitions = CreateFixedMDTable<AssemblyDefinition>( Tables.Assembly, sizes, infos, ref defaultTableInfos );
         this._assemblyDefinitionProcessors = CreateFixedMDTable<AssemblyDefinitionProcessor>( Tables.AssemblyProcessor, sizes, infos, ref defaultTableInfos );
         this._assemblyDefinitionOSs = CreateFixedMDTable<AssemblyDefinitionOS>( Tables.AssemblyOS, sizes, infos, ref defaultTableInfos );
         this._assemblyReferences = CreateFixedMDTable<AssemblyReference>( Tables.AssemblyRef, sizes, infos, ref defaultTableInfos );
         this._assemblyReferenceProcessors = CreateFixedMDTable<AssemblyReferenceProcessor>( Tables.AssemblyRefProcessor, sizes, infos, ref defaultTableInfos );
         this._assemblyReferenceOSs = CreateFixedMDTable<AssemblyReferenceOS>( Tables.AssemblyRefOS, sizes, infos, ref defaultTableInfos );
         this._fileReferences = CreateFixedMDTable<FileReference>( Tables.File, sizes, infos, ref defaultTableInfos );
         this._exportedTypes = CreateFixedMDTable<ExportedType>( Tables.ExportedType, sizes, infos, ref defaultTableInfos );
         this._manifestResources = CreateFixedMDTable<ManifestResource>( Tables.ManifestResource, sizes, infos, ref defaultTableInfos );
         this._nestedClassDefinitions = CreateFixedMDTable<NestedClassDefinition>( Tables.NestedClass, sizes, infos, ref defaultTableInfos );
         this._genericParameterDefinitions = CreateFixedMDTable<GenericParameterDefinition>( Tables.GenericParameter, sizes, infos, ref defaultTableInfos );
         this._methodSpecifications = CreateFixedMDTable<MethodSpecification>( Tables.MethodSpec, sizes, infos, ref defaultTableInfos );
         this._genericParameterConstraintDefinitions = CreateFixedMDTable<GenericParameterConstraintDefinition>( Tables.GenericParameterConstraint, sizes, infos, ref defaultTableInfos );
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
            return this._exportedTypes;
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

      public MetaDataTable<ParameterDefinitionPointer> ParameterDefinitionPointers
      {
         get
         {
            return this._parameterDefinitionPointers;
         }
      }

      public MetaDataTable<EventDefinitionPointer> EventDefinitionPointers
      {
         get
         {
            return this._eventDefinitionPointers;
         }
      }

      public MetaDataTable<PropertyDefinitionPointer> PropertyDefinitionPointers
      {
         get
         {
            return this._propertyDefinitionPointers;
         }
      }

#pragma warning disable 618
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

      protected override bool TryGetFixedTable( Int32 index, out MetaDataTable table )
      {
#pragma warning disable 618
         switch ( (Tables) index )
         {
            case Tables.Module:
               table = this._moduleDefinitions;
               break;
            case Tables.TypeRef:
               table = this._typeReferences;
               break;
            case Tables.TypeDef:
               table = this._typeDefinitions;
               break;
            case Tables.FieldPtr:
               table = this._fieldDefinitionPointers;
               break;
            case Tables.Field:
               table = this._fieldDefinitions;
               break;
            case Tables.MethodPtr:
               table = this._methodDefinitionPointers;
               break;
            case Tables.MethodDef:
               table = this._methodDefinitions;
               break;
            case Tables.ParameterPtr:
               table = this._parameterDefinitionPointers;
               break;
            case Tables.Parameter:
               table = this._parameterDefinitions;
               break;
            case Tables.InterfaceImpl:
               table = this._interfaceImplementations;
               break;
            case Tables.MemberRef:
               table = this._memberReferences;
               break;
            case Tables.Constant:
               table = this._constantDefinitions;
               break;
            case Tables.CustomAttribute:
               table = this._customAttributeDefinitions;
               break;
            case Tables.FieldMarshal:
               table = this._fieldMarshals;
               break;
            case Tables.DeclSecurity:
               table = this._securityDefinitions;
               break;
            case Tables.ClassLayout:
               table = this._classLayouts;
               break;
            case Tables.FieldLayout:
               table = this._fieldLayouts;
               break;
            case Tables.StandaloneSignature:
               table = this._standaloneSignatures;
               break;
            case Tables.EventMap:
               table = this._eventMaps;
               break;
            case Tables.EventPtr:
               table = this._eventDefinitionPointers;
               break;
            case Tables.Event:
               table = this._eventDefinitions;
               break;
            case Tables.PropertyMap:
               table = this._propertyMaps;
               break;
            case Tables.PropertyPtr:
               table = this._propertyDefinitionPointers;
               break;
            case Tables.Property:
               table = this._propertyDefinitions;
               break;
            case Tables.MethodSemantics:
               table = this._methodSemantics;
               break;
            case Tables.MethodImpl:
               table = this._methodImplementations;
               break;
            case Tables.ModuleRef:
               table = this._moduleReferences;
               break;
            case Tables.TypeSpec:
               table = this._typeSpecifications;
               break;
            case Tables.ImplMap:
               table = this._methodImplementationMaps;
               break;
            case Tables.FieldRVA:
               table = this._fieldRVAs;
               break;
            case Tables.EncLog:
               table = this._editAndContinueLog;
               break;
            case Tables.EncMap:
               table = this._editAndContinueMap;
               break;
            case Tables.Assembly:
               table = this._assemblyDefinitions;
               break;
            case Tables.AssemblyProcessor:
               table = this._assemblyDefinitionProcessors;
               break;
            case Tables.AssemblyOS:
               table = this._assemblyDefinitionOSs;
               break;
            case Tables.AssemblyRef:
               table = this._assemblyReferences;
               break;
            case Tables.AssemblyRefProcessor:
               table = this._assemblyReferenceProcessors;
               break;
            case Tables.AssemblyRefOS:
               table = this._assemblyReferenceOSs;
               break;
            case Tables.File:
               table = this._fileReferences;
               break;
            case Tables.ExportedType:
               table = this._exportedTypes;
               break;
            case Tables.ManifestResource:
               table = this._manifestResources;
               break;
            case Tables.NestedClass:
               table = this._nestedClassDefinitions;
               break;
            case Tables.GenericParameter:
               table = this._genericParameterDefinitions;
               break;
            case Tables.MethodSpec:
               table = this._methodSpecifications;
               break;
            case Tables.GenericParameterConstraint:
               table = this._genericParameterConstraintDefinitions;
               break;
            default:
               this.TryGetAdditionalTable( index, out table );
               break;
         }
         return table != null;
#pragma warning restore 618
      }

      public override IEnumerable<MetaDataTable> GetFixedTables()
      {
         yield return this._moduleDefinitions;
         yield return this._typeReferences;
         yield return this._typeDefinitions;
         yield return this._fieldDefinitionPointers;
         yield return this._fieldDefinitions;
         yield return this._methodDefinitionPointers;
         yield return this._methodDefinitions;
         yield return this._parameterDefinitionPointers;
         yield return this._parameterDefinitions;
         yield return this._interfaceImplementations;
         yield return this._memberReferences;
         yield return this._constantDefinitions;
         yield return this._customAttributeDefinitions;
         yield return this._fieldMarshals;
         yield return this._securityDefinitions;
         yield return this._classLayouts;
         yield return this._fieldLayouts;
         yield return this._standaloneSignatures;
         yield return this._eventMaps;
         yield return this._eventDefinitionPointers;
         yield return this._eventDefinitions;
         yield return this._propertyMaps;
         yield return this._propertyDefinitionPointers;
         yield return this._propertyDefinitions;
         yield return this._methodSemantics;
         yield return this._methodImplementations;
         yield return this._moduleReferences;
         yield return this._typeSpecifications;
         yield return this._methodImplementationMaps;
         yield return this._fieldRVAs;
         yield return this._editAndContinueLog;
         yield return this._editAndContinueMap;
         yield return this._assemblyDefinitions;
#pragma warning disable 618
         yield return this._assemblyDefinitionProcessors;
         yield return this._assemblyDefinitionOSs;
#pragma warning restore 618
         yield return this._assemblyReferences;
#pragma warning disable 618
         yield return this._assemblyReferenceProcessors;
         yield return this._assemblyReferenceOSs;
#pragma warning restore 618
         yield return this._fileReferences;
         yield return this._exportedTypes;
         yield return this._manifestResources;
         yield return this._nestedClassDefinitions;
         yield return this._genericParameterDefinitions;
         yield return this._methodSpecifications;
         yield return this._genericParameterConstraintDefinitions;
      }

      private static MetaDataTable<TRow> CreateFixedMDTable<TRow>(
         Tables table,
         Int32[] sizes,
         MetaDataTableInformation[] infos,
         ref MetaDataTableInformation[] defaultInfos
         )
         where TRow : class
      {
         return CreateFixedMDTable<TRow>(
            (Int32) table,
            sizes,
            infos,
            ref defaultInfos,
            () => DefaultMetaDataTableInformationProvider.CreateDefault()
            );
      }
   }

}
