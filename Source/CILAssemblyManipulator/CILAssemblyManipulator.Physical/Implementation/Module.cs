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

namespace CILAssemblyManipulator.Physical.Implementation
{
   internal sealed class CILMetadataImpl : CILMetaData
   {
      private static readonly Int32[] EMPTY_SIZES = Enumerable.Repeat( 0, Consts.AMOUNT_OF_TABLES ).ToArray();

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
      private readonly MetaDataTable<ExportedType> _exportedTypess;
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
      private readonly MetaDataTable<EventDefinitionPointer> _eventPointers;
      private readonly MetaDataTable<PropertyDefinitionPointer> _propertyPointers;
#pragma warning disable 618
      private readonly MetaDataTable<AssemblyDefinitionProcessor> _assemblyDefinitionProcessors;
      private readonly MetaDataTable<AssemblyDefinitionOS> _assemblyDefinitionOSs;
      private readonly MetaDataTable<AssemblyReferenceProcessor> _assemblyReferenceProcessors;
      private readonly MetaDataTable<AssemblyReferenceOS> _assemblyReferenceOSs;

      private readonly MetaDataTable[] _additionalTables;

      internal CILMetadataImpl(
         MetaDataTableInformationProvider tableInfoProvider,
         Int32[] sizes = null
         )
      {
         if ( tableInfoProvider == null )
         {
            tableInfoProvider = DefaultMetaDataTableInformationProvider.CreateDefault();
         }

         if ( sizes.IsNullOrEmpty() )
         {
            sizes = EMPTY_SIZES;
         }
         else if ( sizes.Length < Consts.AMOUNT_OF_TABLES )
         {
            var newSizes = new Int32[Consts.AMOUNT_OF_TABLES];
            Array.Copy( sizes, newSizes, sizes.Length );
            sizes = newSizes;
         }

         DefaultMetaDataTableInformationProvider defaultProvider = null;

         this._moduleDefinitions = CreateFixedMDTable<ModuleDefinition>( Tables.Module, sizes, tableInfoProvider, ref defaultProvider );
         this._typeReferences = CreateFixedMDTable<TypeReference>( Tables.TypeRef, sizes, tableInfoProvider, ref defaultProvider );
         this._typeDefinitions = CreateFixedMDTable<TypeDefinition>( Tables.TypeDef, sizes, tableInfoProvider, ref defaultProvider );
         this._fieldDefinitionPointers = CreateFixedMDTable<FieldDefinitionPointer>( Tables.FieldPtr, sizes, tableInfoProvider, ref defaultProvider );
         this._fieldDefinitions = CreateFixedMDTable<FieldDefinition>( Tables.Field, sizes, tableInfoProvider, ref defaultProvider );
         this._methodDefinitionPointers = CreateFixedMDTable<MethodDefinitionPointer>( Tables.MethodPtr, sizes, tableInfoProvider, ref defaultProvider );
         this._methodDefinitions = CreateFixedMDTable<MethodDefinition>( Tables.MethodDef, sizes, tableInfoProvider, ref defaultProvider );
         this._parameterDefinitionPointers = CreateFixedMDTable<ParameterDefinitionPointer>( Tables.ParameterPtr, sizes, tableInfoProvider, ref defaultProvider );
         this._parameterDefinitions = CreateFixedMDTable<ParameterDefinition>( Tables.Parameter, sizes, tableInfoProvider, ref defaultProvider );
         this._interfaceImplementations = CreateFixedMDTable<InterfaceImplementation>( Tables.InterfaceImpl, sizes, tableInfoProvider, ref defaultProvider );
         this._memberReferences = CreateFixedMDTable<MemberReference>( Tables.MemberRef, sizes, tableInfoProvider, ref defaultProvider );
         this._constantDefinitions = CreateFixedMDTable<ConstantDefinition>( Tables.Constant, sizes, tableInfoProvider, ref defaultProvider );
         this._customAttributeDefinitions = CreateFixedMDTable<CustomAttributeDefinition>( Tables.CustomAttribute, sizes, tableInfoProvider, ref defaultProvider );
         this._fieldMarshals = CreateFixedMDTable<FieldMarshal>( Tables.FieldMarshal, sizes, tableInfoProvider, ref defaultProvider );
         this._securityDefinitions = CreateFixedMDTable<SecurityDefinition>( Tables.DeclSecurity, sizes, tableInfoProvider, ref defaultProvider );
         this._classLayouts = CreateFixedMDTable<ClassLayout>( Tables.ClassLayout, sizes, tableInfoProvider, ref defaultProvider );
         this._fieldLayouts = CreateFixedMDTable<FieldLayout>( Tables.FieldLayout, sizes, tableInfoProvider, ref defaultProvider );
         this._standaloneSignatures = CreateFixedMDTable<StandaloneSignature>( Tables.StandaloneSignature, sizes, tableInfoProvider, ref defaultProvider );
         this._eventMaps = CreateFixedMDTable<EventMap>( Tables.EventMap, sizes, tableInfoProvider, ref defaultProvider );
         this._eventPointers = CreateFixedMDTable<EventDefinitionPointer>( Tables.EventPtr, sizes, tableInfoProvider, ref defaultProvider );
         this._eventDefinitions = CreateFixedMDTable<EventDefinition>( Tables.Event, sizes, tableInfoProvider, ref defaultProvider );
         this._propertyMaps = CreateFixedMDTable<PropertyMap>( Tables.PropertyMap, sizes, tableInfoProvider, ref defaultProvider );
         this._propertyPointers = CreateFixedMDTable<PropertyDefinitionPointer>( Tables.PropertyPtr, sizes, tableInfoProvider, ref defaultProvider );
         this._propertyDefinitions = CreateFixedMDTable<PropertyDefinition>( Tables.Property, sizes, tableInfoProvider, ref defaultProvider );
         this._methodSemantics = CreateFixedMDTable<MethodSemantics>( Tables.MethodSemantics, sizes, tableInfoProvider, ref defaultProvider );
         this._methodImplementations = CreateFixedMDTable<MethodImplementation>( Tables.MethodImpl, sizes, tableInfoProvider, ref defaultProvider );
         this._moduleReferences = CreateFixedMDTable<ModuleReference>( Tables.ModuleRef, sizes, tableInfoProvider, ref defaultProvider );
         this._typeSpecifications = CreateFixedMDTable<TypeSpecification>( Tables.TypeSpec, sizes, tableInfoProvider, ref defaultProvider );
         this._methodImplementationMaps = CreateFixedMDTable<MethodImplementationMap>( Tables.ImplMap, sizes, tableInfoProvider, ref defaultProvider );
         this._fieldRVAs = CreateFixedMDTable<FieldRVA>( Tables.FieldRVA, sizes, tableInfoProvider, ref defaultProvider );
         this._editAndContinueLog = CreateFixedMDTable<EditAndContinueLog>( Tables.EncLog, sizes, tableInfoProvider, ref defaultProvider );
         this._editAndContinueMap = CreateFixedMDTable<EditAndContinueMap>( Tables.EncMap, sizes, tableInfoProvider, ref defaultProvider );
         this._assemblyDefinitions = CreateFixedMDTable<AssemblyDefinition>( Tables.Assembly, sizes, tableInfoProvider, ref defaultProvider );
         this._assemblyDefinitionProcessors = CreateFixedMDTable<AssemblyDefinitionProcessor>( Tables.AssemblyProcessor, sizes, tableInfoProvider, ref defaultProvider );
         this._assemblyDefinitionOSs = CreateFixedMDTable<AssemblyDefinitionOS>( Tables.AssemblyOS, sizes, tableInfoProvider, ref defaultProvider );
         this._assemblyReferences = CreateFixedMDTable<AssemblyReference>( Tables.AssemblyRef, sizes, tableInfoProvider, ref defaultProvider );
         this._assemblyReferenceProcessors = CreateFixedMDTable<AssemblyReferenceProcessor>( Tables.AssemblyRefProcessor, sizes, tableInfoProvider, ref defaultProvider );
         this._assemblyReferenceOSs = CreateFixedMDTable<AssemblyReferenceOS>( Tables.AssemblyRefOS, sizes, tableInfoProvider, ref defaultProvider );
         this._fileReferences = CreateFixedMDTable<FileReference>( Tables.File, sizes, tableInfoProvider, ref defaultProvider );
         this._exportedTypess = CreateFixedMDTable<ExportedType>( Tables.ExportedType, sizes, tableInfoProvider, ref defaultProvider );
         this._manifestResources = CreateFixedMDTable<ManifestResource>( Tables.ManifestResource, sizes, tableInfoProvider, ref defaultProvider );
         this._nestedClassDefinitions = CreateFixedMDTable<NestedClassDefinition>( Tables.NestedClass, sizes, tableInfoProvider, ref defaultProvider );
         this._genericParameterDefinitions = CreateFixedMDTable<GenericParameterDefinition>( Tables.GenericParameter, sizes, tableInfoProvider, ref defaultProvider );
         this._methodSpecifications = CreateFixedMDTable<MethodSpecification>( Tables.MethodSpec, sizes, tableInfoProvider, ref defaultProvider );
         this._genericParameterConstraintDefinitions = CreateFixedMDTable<GenericParameterConstraintDefinition>( Tables.GenericParameterConstraint, sizes, tableInfoProvider, ref defaultProvider );

         // Populate additional tables

         if ( tableInfoProvider.HasAdditionalTables )
         {
            this._additionalTables = new MetaDataTable[Byte.MaxValue + 1 - Consts.AMOUNT_OF_TABLES];
            for ( var i = 0; i < this._additionalTables.Length; ++i )
            {
               var tableValue = i + Consts.AMOUNT_OF_TABLES;
               var capacity = tableValue < sizes.Length ? sizes[tableValue] : 0;
               this._additionalTables[i] = tableInfoProvider.GetTableInformation( (Tables) tableValue )?.CreateMetaDataTableNotGeneric( capacity );
            }
         }
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
            return this._eventPointers;
         }
      }

      public MetaDataTable<PropertyDefinitionPointer> PropertyDefinitionPointers
      {
         get
         {
            return this._propertyPointers;
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

      public MetaDataTable GetAdditionalTable( Int32 table )
      {
         var additionalTables = this._additionalTables;
         MetaDataTable retVal;
         if ( additionalTables != null )
         {
            var additionalIndex = ( (Byte) table ) - Consts.AMOUNT_OF_TABLES;
            retVal = additionalIndex < 0 ? null : additionalTables[additionalIndex];
         }
         else
         {
            retVal = null;
         }
         return retVal;
      }

      private static MetaDataTable<TRow> CreateFixedMDTable<TRow>(
         Tables table,
         Int32[] sizes,
         MetaDataTableInformationProvider provider,
         ref DefaultMetaDataTableInformationProvider defaultProvider
         )
         where TRow : class
      {
         var info = provider.GetTableInformation( table );
         if ( info == null )
         {
            if ( defaultProvider == null )
            {
               defaultProvider = DefaultMetaDataTableInformationProvider.CreateDefault();
            }
            info = defaultProvider.GetTableInformation( table );
         }

         return (MetaDataTable<TRow>) info.CreateMetaDataTableNotGeneric( sizes[(Int32) table] );
      }
   }

   internal sealed class MetaDataTableImpl<TRow> : MetaDataTable<TRow>
      where TRow : class
   {
      private readonly List<TRow> _table;

      internal MetaDataTableImpl(
         MetaDataTableInformation<TRow> tableInfo,
         Int32 tableRowCapacity
         )
      {
         ArgumentValidator.ValidateNotNull( "Table information", tableInfo );

         this.TableInformation = tableInfo;
         this.TableKind = tableInfo.TableKind;
         this._table = new List<TRow>( Math.Max( 0, tableRowCapacity ) );
      }

      public List<TRow> TableContents
      {
         get
         {
            return this._table;
         }
      }

      public Tables TableKind { get; }

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

      public MetaDataTableInformation<TRow> TableInformation { get; }
      public MetaDataTableInformation TableInformationNotGeneric
      {
         get
         {
            return this.TableInformation;
         }
      }

      public Object GetRowAt( Int32 idx )
      {
         return this._table[idx];
      }

      public Boolean TryAddRow( Object row )
      {
         var rowTyped = row as TRow;
         var retVal = rowTyped != null;
         if ( retVal )
         {
            this._table.Add( rowTyped );
         }
         return retVal;
      }

      public override String ToString()
      {
         return this.TableKind + ", row count: " + this._table.Count + ".";
      }
   }
}
