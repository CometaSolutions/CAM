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
#pragma warning disable 618

      internal CILMetadataImpl(
         MetaDataTableInformationProvider tableInfoProvider,
         Int32[] sizes,
         out MetaDataTableInformation[] infos
         )
         : base( tableInfoProvider ?? DefaultMetaDataTableInformationProvider.CreateDefault(), Consts.AMOUNT_OF_FIXED_TABLES, sizes, out infos )
      {
         MetaDataTableInformation[] defaultTableInfos = null;
         this.ModuleDefinitions = CreateFixedMDTable<ModuleDefinition>( Tables.Module, sizes, infos, ref defaultTableInfos );
         this.TypeReferences = CreateFixedMDTable<TypeReference>( Tables.TypeRef, sizes, infos, ref defaultTableInfos );
         this.TypeDefinitions = CreateFixedMDTable<TypeDefinition>( Tables.TypeDef, sizes, infos, ref defaultTableInfos );
         this.FieldDefinitionPointers = CreateFixedMDTable<FieldDefinitionPointer>( Tables.FieldPtr, sizes, infos, ref defaultTableInfos );
         this.FieldDefinitions = CreateFixedMDTable<FieldDefinition>( Tables.Field, sizes, infos, ref defaultTableInfos );
         this.MethodDefinitionPointers = CreateFixedMDTable<MethodDefinitionPointer>( Tables.MethodPtr, sizes, infos, ref defaultTableInfos );
         this.MethodDefinitions = CreateFixedMDTable<MethodDefinition>( Tables.MethodDef, sizes, infos, ref defaultTableInfos );
         this.ParameterDefinitionPointers = CreateFixedMDTable<ParameterDefinitionPointer>( Tables.ParameterPtr, sizes, infos, ref defaultTableInfos );
         this.ParameterDefinitions = CreateFixedMDTable<ParameterDefinition>( Tables.Parameter, sizes, infos, ref defaultTableInfos );
         this.InterfaceImplementations = CreateFixedMDTable<InterfaceImplementation>( Tables.InterfaceImpl, sizes, infos, ref defaultTableInfos );
         this.MemberReferences = CreateFixedMDTable<MemberReference>( Tables.MemberRef, sizes, infos, ref defaultTableInfos );
         this.ConstantDefinitions = CreateFixedMDTable<ConstantDefinition>( Tables.Constant, sizes, infos, ref defaultTableInfos );
         this.CustomAttributeDefinitions = CreateFixedMDTable<CustomAttributeDefinition>( Tables.CustomAttribute, sizes, infos, ref defaultTableInfos );
         this.FieldMarshals = CreateFixedMDTable<FieldMarshal>( Tables.FieldMarshal, sizes, infos, ref defaultTableInfos );
         this.SecurityDefinitions = CreateFixedMDTable<SecurityDefinition>( Tables.DeclSecurity, sizes, infos, ref defaultTableInfos );
         this.ClassLayouts = CreateFixedMDTable<ClassLayout>( Tables.ClassLayout, sizes, infos, ref defaultTableInfos );
         this.FieldLayouts = CreateFixedMDTable<FieldLayout>( Tables.FieldLayout, sizes, infos, ref defaultTableInfos );
         this.StandaloneSignatures = CreateFixedMDTable<StandaloneSignature>( Tables.StandaloneSignature, sizes, infos, ref defaultTableInfos );
         this.EventMaps = CreateFixedMDTable<EventMap>( Tables.EventMap, sizes, infos, ref defaultTableInfos );
         this.EventDefinitionPointers = CreateFixedMDTable<EventDefinitionPointer>( Tables.EventPtr, sizes, infos, ref defaultTableInfos );
         this.EventDefinitions = CreateFixedMDTable<EventDefinition>( Tables.Event, sizes, infos, ref defaultTableInfos );
         this.PropertyMaps = CreateFixedMDTable<PropertyMap>( Tables.PropertyMap, sizes, infos, ref defaultTableInfos );
         this.PropertyDefinitionPointers = CreateFixedMDTable<PropertyDefinitionPointer>( Tables.PropertyPtr, sizes, infos, ref defaultTableInfos );
         this.PropertyDefinitions = CreateFixedMDTable<PropertyDefinition>( Tables.Property, sizes, infos, ref defaultTableInfos );
         this.MethodSemantics = CreateFixedMDTable<MethodSemantics>( Tables.MethodSemantics, sizes, infos, ref defaultTableInfos );
         this.MethodImplementations = CreateFixedMDTable<MethodImplementation>( Tables.MethodImpl, sizes, infos, ref defaultTableInfos );
         this.ModuleReferences = CreateFixedMDTable<ModuleReference>( Tables.ModuleRef, sizes, infos, ref defaultTableInfos );
         this.TypeSpecifications = CreateFixedMDTable<TypeSpecification>( Tables.TypeSpec, sizes, infos, ref defaultTableInfos );
         this.MethodImplementationMaps = CreateFixedMDTable<MethodImplementationMap>( Tables.ImplMap, sizes, infos, ref defaultTableInfos );
         this.FieldRVAs = CreateFixedMDTable<FieldRVA>( Tables.FieldRVA, sizes, infos, ref defaultTableInfos );
         this.EditAndContinueLog = CreateFixedMDTable<EditAndContinueLog>( Tables.EncLog, sizes, infos, ref defaultTableInfos );
         this.EditAndContinueMap = CreateFixedMDTable<EditAndContinueMap>( Tables.EncMap, sizes, infos, ref defaultTableInfos );
         this.AssemblyDefinitions = CreateFixedMDTable<AssemblyDefinition>( Tables.Assembly, sizes, infos, ref defaultTableInfos );
         this.AssemblyDefinitionProcessors = CreateFixedMDTable<AssemblyDefinitionProcessor>( Tables.AssemblyProcessor, sizes, infos, ref defaultTableInfos );
         this.AssemblyDefinitionOSs = CreateFixedMDTable<AssemblyDefinitionOS>( Tables.AssemblyOS, sizes, infos, ref defaultTableInfos );
         this.AssemblyReferences = CreateFixedMDTable<AssemblyReference>( Tables.AssemblyRef, sizes, infos, ref defaultTableInfos );
         this.AssemblyReferenceProcessors = CreateFixedMDTable<AssemblyReferenceProcessor>( Tables.AssemblyRefProcessor, sizes, infos, ref defaultTableInfos );
         this.AssemblyReferenceOSs = CreateFixedMDTable<AssemblyReferenceOS>( Tables.AssemblyRefOS, sizes, infos, ref defaultTableInfos );
         this.FileReferences = CreateFixedMDTable<FileReference>( Tables.File, sizes, infos, ref defaultTableInfos );
         this.ExportedTypes = CreateFixedMDTable<ExportedType>( Tables.ExportedType, sizes, infos, ref defaultTableInfos );
         this.ManifestResources = CreateFixedMDTable<ManifestResource>( Tables.ManifestResource, sizes, infos, ref defaultTableInfos );
         this.NestedClassDefinitions = CreateFixedMDTable<NestedClassDefinition>( Tables.NestedClass, sizes, infos, ref defaultTableInfos );
         this.GenericParameterDefinitions = CreateFixedMDTable<GenericParameterDefinition>( Tables.GenericParameter, sizes, infos, ref defaultTableInfos );
         this.MethodSpecifications = CreateFixedMDTable<MethodSpecification>( Tables.MethodSpec, sizes, infos, ref defaultTableInfos );
         this.GenericParameterConstraintDefinitions = CreateFixedMDTable<GenericParameterConstraintDefinition>( Tables.GenericParameterConstraint, sizes, infos, ref defaultTableInfos );
      }
#pragma warning restore 618

      public MetaDataTable<ModuleDefinition> ModuleDefinitions { get; }

      public MetaDataTable<TypeReference> TypeReferences { get; }

      public MetaDataTable<TypeDefinition> TypeDefinitions { get; }

      public MetaDataTable<FieldDefinition> FieldDefinitions { get; }

      public MetaDataTable<MethodDefinition> MethodDefinitions { get; }

      public MetaDataTable<ParameterDefinition> ParameterDefinitions { get; }

      public MetaDataTable<InterfaceImplementation> InterfaceImplementations { get; }

      public MetaDataTable<MemberReference> MemberReferences { get; }

      public MetaDataTable<ConstantDefinition> ConstantDefinitions { get; }

      public MetaDataTable<CustomAttributeDefinition> CustomAttributeDefinitions { get; }

      public MetaDataTable<FieldMarshal> FieldMarshals { get; }

      public MetaDataTable<SecurityDefinition> SecurityDefinitions { get; }

      public MetaDataTable<ClassLayout> ClassLayouts { get; }

      public MetaDataTable<FieldLayout> FieldLayouts { get; }

      public MetaDataTable<StandaloneSignature> StandaloneSignatures { get; }

      public MetaDataTable<EventMap> EventMaps { get; }

      public MetaDataTable<EventDefinition> EventDefinitions { get; }

      public MetaDataTable<PropertyMap> PropertyMaps { get; }

      public MetaDataTable<PropertyDefinition> PropertyDefinitions { get; }

      public MetaDataTable<MethodSemantics> MethodSemantics { get; }

      public MetaDataTable<MethodImplementation> MethodImplementations { get; }

      public MetaDataTable<ModuleReference> ModuleReferences { get; }

      public MetaDataTable<TypeSpecification> TypeSpecifications { get; }

      public MetaDataTable<MethodImplementationMap> MethodImplementationMaps { get; }

      public MetaDataTable<FieldRVA> FieldRVAs { get; }

      public MetaDataTable<AssemblyDefinition> AssemblyDefinitions { get; }

      public MetaDataTable<AssemblyReference> AssemblyReferences { get; }

      public MetaDataTable<FileReference> FileReferences { get; }

      public MetaDataTable<ExportedType> ExportedTypes { get; }

      public MetaDataTable<ManifestResource> ManifestResources { get; }

      public MetaDataTable<NestedClassDefinition> NestedClassDefinitions { get; }

      public MetaDataTable<GenericParameterDefinition> GenericParameterDefinitions { get; }

      public MetaDataTable<MethodSpecification> MethodSpecifications { get; }

      public MetaDataTable<GenericParameterConstraintDefinition> GenericParameterConstraintDefinitions { get; }

      public MetaDataTable<EditAndContinueLog> EditAndContinueLog { get; }

      public MetaDataTable<EditAndContinueMap> EditAndContinueMap { get; }

      public MetaDataTable<FieldDefinitionPointer> FieldDefinitionPointers { get; }

      public MetaDataTable<MethodDefinitionPointer> MethodDefinitionPointers { get; }

      public MetaDataTable<ParameterDefinitionPointer> ParameterDefinitionPointers { get; }

      public MetaDataTable<EventDefinitionPointer> EventDefinitionPointers { get; }

      public MetaDataTable<PropertyDefinitionPointer> PropertyDefinitionPointers { get; }

#pragma warning disable 618
      public MetaDataTable<AssemblyDefinitionProcessor> AssemblyDefinitionProcessors { get; }

      public MetaDataTable<AssemblyDefinitionOS> AssemblyDefinitionOSs { get; }

      public MetaDataTable<AssemblyReferenceProcessor> AssemblyReferenceProcessors { get; }

      public MetaDataTable<AssemblyReferenceOS> AssemblyReferenceOSs { get; }
#pragma warning restore 618

      protected override Boolean TryGetFixedTable( Int32 index, out MetaDataTable table )
      {
#pragma warning disable 618
         switch ( (Tables) index )
         {
            case Tables.Module:
               table = this.ModuleDefinitions;
               break;
            case Tables.TypeRef:
               table = this.TypeReferences;
               break;
            case Tables.TypeDef:
               table = this.TypeDefinitions;
               break;
            case Tables.FieldPtr:
               table = this.FieldDefinitionPointers;
               break;
            case Tables.Field:
               table = this.FieldDefinitions;
               break;
            case Tables.MethodPtr:
               table = this.MethodDefinitionPointers;
               break;
            case Tables.MethodDef:
               table = this.MethodDefinitions;
               break;
            case Tables.ParameterPtr:
               table = this.ParameterDefinitionPointers;
               break;
            case Tables.Parameter:
               table = this.ParameterDefinitions;
               break;
            case Tables.InterfaceImpl:
               table = this.InterfaceImplementations;
               break;
            case Tables.MemberRef:
               table = this.MemberReferences;
               break;
            case Tables.Constant:
               table = this.ConstantDefinitions;
               break;
            case Tables.CustomAttribute:
               table = this.CustomAttributeDefinitions;
               break;
            case Tables.FieldMarshal:
               table = this.FieldMarshals;
               break;
            case Tables.DeclSecurity:
               table = this.SecurityDefinitions;
               break;
            case Tables.ClassLayout:
               table = this.ClassLayouts;
               break;
            case Tables.FieldLayout:
               table = this.FieldLayouts;
               break;
            case Tables.StandaloneSignature:
               table = this.StandaloneSignatures;
               break;
            case Tables.EventMap:
               table = this.EventMaps;
               break;
            case Tables.EventPtr:
               table = this.EventDefinitionPointers;
               break;
            case Tables.Event:
               table = this.EventDefinitions;
               break;
            case Tables.PropertyMap:
               table = this.PropertyMaps;
               break;
            case Tables.PropertyPtr:
               table = this.PropertyDefinitionPointers;
               break;
            case Tables.Property:
               table = this.PropertyDefinitions;
               break;
            case Tables.MethodSemantics:
               table = this.MethodSemantics;
               break;
            case Tables.MethodImpl:
               table = this.MethodImplementations;
               break;
            case Tables.ModuleRef:
               table = this.ModuleReferences;
               break;
            case Tables.TypeSpec:
               table = this.TypeSpecifications;
               break;
            case Tables.ImplMap:
               table = this.MethodImplementationMaps;
               break;
            case Tables.FieldRVA:
               table = this.FieldRVAs;
               break;
            case Tables.EncLog:
               table = this.EditAndContinueLog;
               break;
            case Tables.EncMap:
               table = this.EditAndContinueMap;
               break;
            case Tables.Assembly:
               table = this.AssemblyDefinitions;
               break;
            case Tables.AssemblyProcessor:
               table = this.AssemblyDefinitionProcessors;
               break;
            case Tables.AssemblyOS:
               table = this.AssemblyDefinitionOSs;
               break;
            case Tables.AssemblyRef:
               table = this.AssemblyReferences;
               break;
            case Tables.AssemblyRefProcessor:
               table = this.AssemblyReferenceProcessors;
               break;
            case Tables.AssemblyRefOS:
               table = this.AssemblyReferenceOSs;
               break;
            case Tables.File:
               table = this.FileReferences;
               break;
            case Tables.ExportedType:
               table = this.ExportedTypes;
               break;
            case Tables.ManifestResource:
               table = this.ManifestResources;
               break;
            case Tables.NestedClass:
               table = this.NestedClassDefinitions;
               break;
            case Tables.GenericParameter:
               table = this.GenericParameterDefinitions;
               break;
            case Tables.MethodSpec:
               table = this.MethodSpecifications;
               break;
            case Tables.GenericParameterConstraint:
               table = this.GenericParameterConstraintDefinitions;
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
         yield return this.ModuleDefinitions;
         yield return this.TypeReferences;
         yield return this.TypeDefinitions;
         yield return this.FieldDefinitionPointers;
         yield return this.FieldDefinitions;
         yield return this.MethodDefinitionPointers;
         yield return this.MethodDefinitions;
         yield return this.ParameterDefinitionPointers;
         yield return this.ParameterDefinitions;
         yield return this.InterfaceImplementations;
         yield return this.MemberReferences;
         yield return this.ConstantDefinitions;
         yield return this.CustomAttributeDefinitions;
         yield return this.FieldMarshals;
         yield return this.SecurityDefinitions;
         yield return this.ClassLayouts;
         yield return this.FieldLayouts;
         yield return this.StandaloneSignatures;
         yield return this.EventMaps;
         yield return this.EventDefinitionPointers;
         yield return this.EventDefinitions;
         yield return this.PropertyMaps;
         yield return this.PropertyDefinitionPointers;
         yield return this.PropertyDefinitions;
         yield return this.MethodSemantics;
         yield return this.MethodImplementations;
         yield return this.ModuleReferences;
         yield return this.TypeSpecifications;
         yield return this.MethodImplementationMaps;
         yield return this.FieldRVAs;
         yield return this.EditAndContinueLog;
         yield return this.EditAndContinueMap;
         yield return this.AssemblyDefinitions;
#pragma warning disable 618
         yield return this.AssemblyDefinitionProcessors;
         yield return this.AssemblyDefinitionOSs;
#pragma warning restore 618
         yield return this.AssemblyReferences;
#pragma warning disable 618
         yield return this.AssemblyReferenceProcessors;
         yield return this.AssemblyReferenceOSs;
#pragma warning restore 618
         yield return this.FileReferences;
         yield return this.ExportedTypes;
         yield return this.ManifestResources;
         yield return this.NestedClassDefinitions;
         yield return this.GenericParameterDefinitions;
         yield return this.MethodSpecifications;
         yield return this.GenericParameterConstraintDefinitions;
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
            DefaultMetaDataTableInformationProvider.CreateDefault
            );
      }
   }

}
