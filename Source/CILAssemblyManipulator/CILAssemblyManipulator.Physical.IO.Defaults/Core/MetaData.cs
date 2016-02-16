﻿/*
* Copyright 2016 Stanislav Muhametsin. All rights Reserved.
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
extern alias CAMPhysicalR;
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;
using CAMPhysical::CILAssemblyManipulator.Physical.Meta;

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.Meta;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TabularMetaData;
using TabularMetaData.Meta;

namespace CILAssemblyManipulator.Physical
{
   internal sealed class CILMetadataImpl : TabularMetaDataWithSchemaImpl, CILMetaData, CAMPhysicalR::CILAssemblyManipulator.Physical.CILMetaData
   {

#pragma warning disable 618

      internal CILMetadataImpl(
         CILMetaDataTableInformationProvider tableInfoProvider,
         Int32[] sizes,
         out MetaDataTableInformation[] infos
         )
         : base( tableInfoProvider ?? DefaultMetaDataTableInformationProvider.CreateDefault(), DefaultMetaDataTableInformationProvider.AMOUNT_OF_FIXED_TABLES, sizes, out infos )
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

         this.OpCodeProvider = tableInfoProvider?.CreateOpCodeProvider() ?? DefaultOpCodeProvider.DefaultInstance;
         this.SignatureProvider = tableInfoProvider?.CreateSignatureProvider() ?? DefaultSignatureProvider.Instance;
         this.ResolvingProvider = ( (CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.CILMetaDataTableInformationProvider) tableInfoProvider )?.CreateResolvingProvider( this ) ?? new CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.DefaultResolvingProvider( this, Empty<Tuple<Tables, CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.MetaDataColumnInformationWithResolvingCapability>>.Enumerable );
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

      public OpCodeProvider OpCodeProvider { get; }

      public SignatureProvider SignatureProvider { get; }

      public CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.ResolvingProvider ResolvingProvider { get; }

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

   /// <summary>
   /// This class provides static methods to create new instances of <see cref="CILMetaData"/>.
   /// </summary>
   public static class CILMetaDataFactory
   {

      /// <summary>
      /// Creates a new, blank, <see cref="CILMetaData"/>.
      /// All of its tables will be empty.
      /// </summary>
      /// <param name="sizes">The optional initial size array for tables. The element at <c>0</c> (which is value of <see cref="Tables.Module"/>) will be the initial capacity for <see cref="CILMetaData.ModuleDefinitions"/> table, and so on.</param>
      /// <param name="tableInfoProvider">The optional <see cref="CILMetaDataTableInformationProvider"/> to customize the tables that resulting <see cref="CILMetaData"/> will have.</param>
      /// <returns>The <see cref="CILMetaData"/> with empty tables.</returns>
      /// <exception cref="FixedTableCreationException">If <paramref name="tableInfoProvider"/> is not <c>null</c>, and returns fixed table with wrong row type.</exception>
      public static CILMetaData NewBlankMetaData( Int32[] sizes = null, CILMetaDataTableInformationProvider tableInfoProvider = null )
      {
         MetaDataTableInformation[] infos;
         return new CILMetadataImpl( tableInfoProvider, sizes, out infos );
      }

      /// <summary>
      /// Creates a new <see cref="CILMetaData"/> with minimal required rows so that it would count as an assembly.
      /// These rows are one row in <see cref="CILMetaData.ModuleDefinitions"/> and one row in <see cref="CILMetaData.AssemblyDefinitions"/> tables.
      /// Additionally, a specific module type will be created to <see cref="CILMetaData.TypeDefinitions"/> table, unless <paramref name="createModuleType"/> will be <c>false</c>.
      /// </summary>
      /// <param name="assemblyName">The name of the assembly.</param>
      /// <param name="moduleName">The name of the module, may be <c>null</c>. In that case, the name of <see cref="ModuleDefinition"/> will be <paramref name="assemblyName"/> concatenated with <c>.dll</c>.</param>
      /// <param name="createModuleType">Whether to create a module type, used to hold the "global" elements.</param>
      /// <param name="tableInfoProvider">The optional <see cref="CILMetaDataTableInformationProvider"/> to customize the tables that resulting <see cref="CILMetaData"/> will have.</param>
      /// <returns>The <see cref="CILMetaData"/> with required information to be count as an assembly.</returns>
      /// <exception cref="FixedTableCreationException">If <paramref name="tableInfoProvider"/> is not <c>null</c>, and returns fixed table with wrong row type.</exception>
      public static CILMetaData CreateMinimalAssembly( String assemblyName, String moduleName, Boolean createModuleType = true, CILMetaDataTableInformationProvider tableInfoProvider = null )
      {
         if ( !String.IsNullOrEmpty( assemblyName ) && String.IsNullOrEmpty( moduleName ) )
         {
            moduleName = assemblyName + ".dll";
         }

         var md = CreateMinimalModule( moduleName, createModuleType, tableInfoProvider );

         var aDef = new AssemblyDefinition();
         aDef.AssemblyInformation.Name = assemblyName;
         md.AssemblyDefinitions.TableContents.Add( aDef );

         return md;
      }

      /// <summary>
      /// Creates a new <see cref="CILMetaData"/> with minimal required rows so that it would count as a module.
      /// This includes one row in the <see cref="CILMetaData.ModuleDefinitions"/>, and also one row in <see cref="CILMetaData.TypeDefinitions"/> table, unless <paramref name="createModuleType"/> is <c>false</c>.
      /// </summary>
      /// <param name="moduleName">The name of the module.</param>
      /// <param name="createModuleType">Whether to create a module type, used to hold the "global" elements.</param>
      /// <param name="tableInfoProvider">The optional <see cref="CILMetaDataTableInformationProvider"/> to customize the tables that resulting <see cref="CILMetaData"/> will have.</param>
      /// <returns>The <see cref="CILMetaData"/> with required information to be count as a module.</returns>
      /// <exception cref="FixedTableCreationException">If <paramref name="tableInfoProvider"/> is not <c>null</c>, and returns fixed table with wrong row type.</exception>
      public static CILMetaData CreateMinimalModule( String moduleName, Boolean createModuleType = true, CILMetaDataTableInformationProvider tableInfoProvider = null )
      {
         var md = CILMetaDataFactory.NewBlankMetaData( tableInfoProvider: tableInfoProvider );

         // Module definition
         md.ModuleDefinitions.TableContents.Add( new ModuleDefinition()
         {
            Name = moduleName,
            ModuleGUID = Guid.NewGuid()
         } );

         if ( createModuleType )
         {
            // Module type
            md.TypeDefinitions.TableContents.Add( new TypeDefinition()
            {
               Name = Miscellaneous.MODULE_TYPE_NAME
            } );
         }

         return md;
      }
   }
}
