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
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;

using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CILAssemblyManipulator.Physical.IO.Defaults;
using CILAssemblyManipulator.Physical.IO;
using TabularMetaData.Meta;

namespace CILAssemblyManipulator.Physical.Meta
{
   public class DefaultMetaDataTableInformationProvider : MetaDataTableInformationProvider
   {

      public const Int32 AMOUNT_OF_FIXED_TABLES = 0x2D; // Enum.GetValues( typeof( Tables ) ).Length;

      // Don't cache the instance in case the provider will become mutable at some point.

      //private static MetaDataTableInformationProvider _Instance = new DefaultMetaDataTableInformationProvider();

      //public static MetaDataTableInformationProvider DefaultInstance
      //{
      //   get
      //   {
      //      return _Instance;
      //   }
      //}

      private readonly MetaDataTableInformation[] _infos;

      public DefaultMetaDataTableInformationProvider(
         IEnumerable<MetaDataTableInformation> tableInfos = null
         )
      {
         this._infos = new MetaDataTableInformation[CAMCoreInternals.AMOUNT_OF_TABLES];
         foreach ( var tableInfo in ( tableInfos ?? CreateDefaultTableInformation() ).Where( ti => ti != null ) )
         {
            var tKind = (Int32) tableInfo.TableIndex;
            this._infos[tKind] = tableInfo;
         }
      }

      public IEnumerable<MetaDataTableInformation> GetAllSupportedTableInformations()
      {
         foreach ( var tableInfo in this._infos )
         {
            yield return tableInfo;
         }
      }

      public static DefaultMetaDataTableInformationProvider CreateDefault()
      {
         return new DefaultMetaDataTableInformationProvider();
      }

      public static DefaultMetaDataTableInformationProvider CreateWithAdditionalTables( IEnumerable<MetaDataTableInformation> tableInfos )
      {
         ArgumentValidator.ValidateNotNull( "Additional table infos", tableInfos );
         return new DefaultMetaDataTableInformationProvider( CreateDefaultTableInformation().Concat( tableInfos.Where( t => (Int32) t.TableIndex > AMOUNT_OF_FIXED_TABLES ) ) );
      }

      public static DefaultMetaDataTableInformationProvider CreateWithExactTables( IEnumerable<MetaDataTableInformation> tableInfos )
      {
         return new DefaultMetaDataTableInformationProvider( tableInfos );
      }

      protected static IEnumerable<MetaDataTableInformation> CreateDefaultTableInformation()
      {
         yield return new MetaDataTableInformation<ModuleDefinition, RawModuleDefinition>(
            Tables.Module,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ModuleDefinitionEqualityComparer,
            null,
            () => new ModuleDefinition(),
            GetModuleDefColumns(),
            () => new RawModuleDefinition(),
            false
            );

         yield return new MetaDataTableInformation<TypeReference, RawTypeReference>(
            Tables.TypeRef,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.TypeReferenceEqualityComparer,
            null,
            () => new TypeReference(),
            GetTypeRefColumns(),
            () => new RawTypeReference(),
            false
            );

         yield return new MetaDataTableInformation<TypeDefinition, RawTypeDefinition>(
            Tables.TypeDef,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.TypeDefinitionEqualityComparer,
            null,
            () => new TypeDefinition(),
            GetTypeDefColumns(),
            () => new RawTypeDefinition(),
            false
            );

         yield return new MetaDataTableInformation<FieldDefinitionPointer, RawFieldDefinitionPointer>(
            Tables.FieldPtr,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.FieldDefinitionPointerEqualityComparer,
            null,
            () => new FieldDefinitionPointer(),
            GetFieldPtrColumns(),
            () => new RawFieldDefinitionPointer(),
            false
            );

         yield return new MetaDataTableInformation<FieldDefinition, RawFieldDefinition>(
            Tables.Field,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.FieldDefinitionEqualityComparer,
            null,
            () => new FieldDefinition(),
            GetFieldDefColumns(),
            () => new RawFieldDefinition(),
            false
            );

         yield return new MetaDataTableInformation<MethodDefinitionPointer, RawMethodDefinitionPointer>(
            Tables.MethodPtr,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.MethodDefinitionPointerEqualityComparer,
            null,
            () => new MethodDefinitionPointer(),
            GetMethodPtrColumns(),
            () => new RawMethodDefinitionPointer(),
            false
            );

         yield return new MetaDataTableInformation<MethodDefinition, RawMethodDefinition>(
            Tables.MethodDef,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.MethodDefinitionEqualityComparer,
            null,
            () => new MethodDefinition(),
            GetMethodDefColumns(),
            () => new RawMethodDefinition(),
            false
            );

         yield return new MetaDataTableInformation<ParameterDefinitionPointer, RawParameterDefinitionPointer>(
            Tables.ParameterPtr,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ParameterDefinitionPointerEqualityComparer,
            null,
            () => new ParameterDefinitionPointer(),
            GetParamPtrColumns(),
            () => new RawParameterDefinitionPointer(),
            false
            );

         yield return new MetaDataTableInformation<ParameterDefinition, RawParameterDefinition>(
            Tables.Parameter,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ParameterDefinitionEqualityComparer,
            null,
            () => new ParameterDefinition(),
            GetParamColumns(),
            () => new RawParameterDefinition(),
            false
            );

         yield return new MetaDataTableInformation<InterfaceImplementation, RawInterfaceImplementation>(
            Tables.InterfaceImpl,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.InterfaceImplementationEqualityComparer,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.InterfaceImplementationComparer,
            () => new InterfaceImplementation(),
            GetInterfaceImplColumns(),
            () => new RawInterfaceImplementation(),
            true
            );

         yield return new MetaDataTableInformation<MemberReference, RawMemberReference>(
            Tables.MemberRef,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.MemberReferenceEqualityComparer,
            null,
            () => new MemberReference(),
            GetMemberRefColumns(),
            () => new RawMemberReference(),
            false
            );

         yield return new MetaDataTableInformation<ConstantDefinition, RawConstantDefinition>(
            Tables.Constant,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ConstantDefinitionEqualityComparer,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ConstantDefinitionComparer,
            () => new ConstantDefinition(),
            GetConstantColumns(),
            () => new RawConstantDefinition(),
            true
            );

         yield return new MetaDataTableInformation<CustomAttributeDefinition, RawCustomAttributeDefinition>(
            Tables.CustomAttribute,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.CustomAttributeDefinitionEqualityComparer,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.CustomAttributeDefinitionComparer,
            () => new CustomAttributeDefinition(),
            GetCustomAttributeColumns(),
            () => new RawCustomAttributeDefinition(),
            true
            );

         yield return new MetaDataTableInformation<FieldMarshal, RawFieldMarshal>(
            Tables.FieldMarshal,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.FieldMarshalEqualityComparer,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.FieldMarshalComparer,
            () => new FieldMarshal(),
            GetFieldMarshalColumns(),
            () => new RawFieldMarshal(),
            true
            );

         yield return new MetaDataTableInformation<SecurityDefinition, RawSecurityDefinition>(
            Tables.DeclSecurity,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.SecurityDefinitionEqualityComparer,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.SecurityDefinitionComparer,
            () => new SecurityDefinition(),
            GetDeclSecurityColumns(),
            () => new RawSecurityDefinition(),
            true
            );

         yield return new MetaDataTableInformation<ClassLayout, RawClassLayout>(
            Tables.ClassLayout,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ClassLayoutEqualityComparer,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ClassLayoutComparer,
            () => new ClassLayout(),
            GetClassLayoutColumns(),
            () => new RawClassLayout(),
            true
            );

         yield return new MetaDataTableInformation<FieldLayout, RawFieldLayout>(
            Tables.FieldLayout,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.FieldLayoutEqualityComparer,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.FieldLayoutComparer,
            () => new FieldLayout(),
            GetFieldLayoutColumns(),
            () => new RawFieldLayout(),
            true
            );

         yield return new MetaDataTableInformation<StandaloneSignature, RawStandaloneSignature>(
            Tables.StandaloneSignature,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.StandaloneSignatureEqualityComparer,
            null,
            () => new StandaloneSignature(),
            GetStandaloneSigColumns(),
            () => new RawStandaloneSignature(),
            false
            );

         yield return new MetaDataTableInformation<EventMap, RawEventMap>(
            Tables.EventMap,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.EventMapEqualityComparer,
            null,
            () => new EventMap(),
            GetEventMapColumns(),
            () => new RawEventMap(),
            true
            );

         yield return new MetaDataTableInformation<EventDefinitionPointer, RawEventDefinitionPointer>(
            Tables.EventPtr,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.EventDefinitionPointerEqualityComparer,
            null,
            () => new EventDefinitionPointer(),
            GetEventPtrColumns(),
            () => new RawEventDefinitionPointer(),
            false
            );

         yield return new MetaDataTableInformation<EventDefinition, RawEventDefinition>(
            Tables.Event,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.EventDefinitionEqualityComparer,
            null,
            () => new EventDefinition(),
            GetEventDefColumns(),
            () => new RawEventDefinition(),
            false
            );

         yield return new MetaDataTableInformation<PropertyMap, RawPropertyMap>(
            Tables.PropertyMap,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.PropertyMapEqualityComparer,
            null,
            () => new PropertyMap(),
            GetPropertyMapColumns(),
            () => new RawPropertyMap(),
            true
            );

         yield return new MetaDataTableInformation<PropertyDefinitionPointer, RawPropertyDefinitionPointer>(
            Tables.PropertyPtr,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.PropertyDefinitionPointerEqualityComparer,
            null,
            () => new PropertyDefinitionPointer(),
            GetPropertyPtrColumns(),
            () => new RawPropertyDefinitionPointer(),
            false
            );

         yield return new MetaDataTableInformation<PropertyDefinition, RawPropertyDefinition>(
            Tables.Property,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.PropertyDefinitionEqualityComparer,
            null,
            () => new PropertyDefinition(),
            GetPropertyDefColumns(),
            () => new RawPropertyDefinition(),
            false
            );

         yield return new MetaDataTableInformation<MethodSemantics, RawMethodSemantics>(
            Tables.MethodSemantics,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.MethodSemanticsEqualityComparer,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.MethodSemanticsComparer,
            () => new MethodSemantics(),
            GetMethodSemanticsColumns(),
            () => new RawMethodSemantics(),
            true
            );

         yield return new MetaDataTableInformation<MethodImplementation, RawMethodImplementation>(
            Tables.MethodImpl,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.MethodImplementationEqualityComparer,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.MethodImplementationComparer,
            () => new MethodImplementation(),
            GetMethodImplColumns(),
            () => new RawMethodImplementation(),
            true
            );

         yield return new MetaDataTableInformation<ModuleReference, RawModuleReference>(
            Tables.ModuleRef,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ModuleReferenceEqualityComparer,
            null,
            () => new ModuleReference(),
            GetModuleRefColumns(),
            () => new RawModuleReference(),
            false
            );

         yield return new MetaDataTableInformation<TypeSpecification, RawTypeSpecification>(
            Tables.TypeSpec,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.TypeSpecificationEqualityComparer,
            null,
            () => new TypeSpecification(),
            GetTypeSpecColumns(),
            () => new RawTypeSpecification(),
            false
            );

         yield return new MetaDataTableInformation<MethodImplementationMap, RawMethodImplementationMap>(
            Tables.ImplMap,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.MethodImplementationMapEqualityComparer,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.MethodImplementationMapComparer,
            () => new MethodImplementationMap(),
            GetImplMapColumns(),
            () => new RawMethodImplementationMap(),
            true
            );

         yield return new MetaDataTableInformation<FieldRVA, RawFieldRVA>(
            Tables.FieldRVA,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.FieldRVAEqualityComparer,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.FieldRVAComparer,
            () => new FieldRVA(),
            GetFieldRVAColumns(),
            () => new RawFieldRVA(),
            true
            );

         yield return new MetaDataTableInformation<EditAndContinueLog, RawEditAndContinueLog>(
            Tables.EncLog,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.EditAndContinueLogEqualityComparer,
            null,
            () => new EditAndContinueLog(),
            GetENCLogColumns(),
            () => new RawEditAndContinueLog(),
            false
            );

         yield return new MetaDataTableInformation<EditAndContinueMap, RawEditAndContinueMap>(
            Tables.EncMap,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.EditAndContinueMapEqualityComparer,
            null,
            () => new EditAndContinueMap(),
            GetENCMapColumns(),
            () => new RawEditAndContinueMap(),
            false
            );

         yield return new MetaDataTableInformation<AssemblyDefinition, RawAssemblyDefinition>(
            Tables.Assembly,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.AssemblyDefinitionEqualityComparer,
            null,
            () => new AssemblyDefinition(),
            GetAssemblyDefColumns(),
            () => new RawAssemblyDefinition(),
            false
            );

#pragma warning disable 618

         yield return new MetaDataTableInformation<AssemblyDefinitionProcessor, RawAssemblyDefinitionProcessor>(
            Tables.AssemblyProcessor,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.AssemblyDefinitionProcessorEqualityComparer,
            null,
            () => new AssemblyDefinitionProcessor(),
            GetAssemblyDefProcessorColumns(),
            () => new RawAssemblyDefinitionProcessor(),
            false
            );

         yield return new MetaDataTableInformation<AssemblyDefinitionOS, RawAssemblyDefinitionOS>(
            Tables.AssemblyOS,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.AssemblyDefinitionOSEqualityComparer,
            null,
            () => new AssemblyDefinitionOS(),
            GetAssemblyDefOSColumns(),
            () => new RawAssemblyDefinitionOS(),
            false
            );

#pragma warning restore 618

         yield return new MetaDataTableInformation<AssemblyReference, RawAssemblyReference>(
            Tables.AssemblyRef,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.AssemblyReferenceEqualityComparer,
            null,
            () => new AssemblyReference(),
            GetAssemblyRefColumns(),
            () => new RawAssemblyReference(),
            false
            );

#pragma warning disable 618

         yield return new MetaDataTableInformation<AssemblyReferenceProcessor, RawAssemblyReferenceProcessor>(
            Tables.AssemblyRefProcessor,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.AssemblyReferenceProcessorEqualityComparer,
            null,
            () => new AssemblyReferenceProcessor(),
            GetAssemblyRefProcessorColumns(),
            () => new RawAssemblyReferenceProcessor(),
            false
            );

         yield return new MetaDataTableInformation<AssemblyReferenceOS, RawAssemblyReferenceOS>(
            Tables.AssemblyRefOS,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.AssemblyReferenceOSEqualityComparer,
            null,
            () => new AssemblyReferenceOS(),
            GetAssemblyRefOSColumns(),
            () => new RawAssemblyReferenceOS(),
            false
            );

#pragma warning restore 618

         yield return new MetaDataTableInformation<FileReference, RawFileReference>(
            Tables.File,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.FileReferenceEqualityComparer,
            null,
            () => new FileReference(),
            GetFileColumns(),
            () => new RawFileReference(),
            false
            );

         yield return new MetaDataTableInformation<ExportedType, RawExportedType>(
            Tables.ExportedType,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ExportedTypeEqualityComparer,
            null,
            () => new ExportedType(),
            GetExportedTypeColumns(),
            () => new RawExportedType(),
            false
            );

         yield return new MetaDataTableInformation<ManifestResource, RawManifestResource>(
            Tables.ManifestResource,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ManifestResourceEqualityComparer,
            null,
            () => new ManifestResource(),
            GetManifestResourceColumns(),
            () => new RawManifestResource(),
            false
            );

         yield return new MetaDataTableInformation<NestedClassDefinition, RawNestedClassDefinition>(
            Tables.NestedClass,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.NestedClassDefinitionEqualityComparer,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.NestedClassDefinitionComparer,
            () => new NestedClassDefinition(),
            GetNestedClassColumns(),
            () => new RawNestedClassDefinition(),
            true
            );

         yield return new MetaDataTableInformation<GenericParameterDefinition, RawGenericParameterDefinition>(
            Tables.GenericParameter,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.GenericParameterDefinitionEqualityComparer,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.GenericParameterDefinitionComparer,
            () => new GenericParameterDefinition(),
            GetGenericParamColumns(),
            () => new RawGenericParameterDefinition(),
            true
            );

         yield return new MetaDataTableInformation<MethodSpecification, RawMethodSpecification>(
            Tables.MethodSpec,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.MethodSpecificationEqualityComparer,
            null,
            () => new MethodSpecification(),
            GetMethodSpecColumns(),
            () => new RawMethodSpecification(),
            false
            );

         yield return new MetaDataTableInformation<GenericParameterConstraintDefinition, RawGenericParameterConstraintDefinition>(
            Tables.GenericParameterConstraint,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.GenericParameterConstraintDefinitionEqualityComparer,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.GenericParameterConstraintDefinitionComparer,
            () => new GenericParameterConstraintDefinition(),
            GetGenericParamConstraintColumns(),
            () => new RawGenericParameterConstraintDefinition(),
            true
            );
      }

      protected static IEnumerable<MetaDataColumnInformation<ModuleDefinition>> GetModuleDefColumns()
      {
         yield return MetaDataColumnInformationFactory.Number16<ModuleDefinition, RawModuleDefinition>( ( r, v ) => { r.Generation = v; return true; }, row => row.Generation, ( r, v ) => r.Generation = v );
         yield return MetaDataColumnInformationFactory.SystemString<ModuleDefinition, RawModuleDefinition>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.GUID<ModuleDefinition, RawModuleDefinition>( ( r, v ) => { r.ModuleGUID = v; return true; }, row => row.ModuleGUID, ( r, v ) => r.ModuleGUID = v );
         yield return MetaDataColumnInformationFactory.GUID<ModuleDefinition, RawModuleDefinition>( ( r, v ) => { r.EditAndContinueGUID = v; return true; }, row => row.EditAndContinueGUID, ( r, v ) => r.EditAndContinueGUID = v );
         yield return MetaDataColumnInformationFactory.GUID<ModuleDefinition, RawModuleDefinition>( ( r, v ) => { r.EditAndContinueBaseGUID = v; return true; }, row => row.EditAndContinueBaseGUID, ( r, v ) => r.EditAndContinueBaseGUID = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<TypeReference>> GetTypeRefColumns()
      {
         yield return MetaDataColumnInformationFactory.CodedTableIndexNullable<TypeReference, RawTypeReference>( TableIndexSchemas.ResolutionScope, ( r, v ) => { r.ResolutionScope = v; return true; }, row => row.ResolutionScope, ( r, v ) => r.ResolutionScope = v );
         yield return MetaDataColumnInformationFactory.SystemString<TypeReference, RawTypeReference>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.SystemString<TypeReference, RawTypeReference>( ( r, v ) => { r.Namespace = v; return true; }, row => row.Namespace, ( r, v ) => r.Namespace = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<TypeDefinition>> GetTypeDefColumns()
      {
         yield return MetaDataColumnInformationFactory.Constant32<TypeDefinition, RawTypeDefinition, TypeAttributes>( ( r, v ) => { r.Attributes = v; return true; }, row => row.Attributes, ( r, v ) => r.Attributes = (TypeAttributes) v, i => (TypeAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformationFactory.SystemString<TypeDefinition, RawTypeDefinition>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.SystemString<TypeDefinition, RawTypeDefinition>( ( r, v ) => { r.Namespace = v; return true; }, row => row.Namespace, ( r, v ) => r.Namespace = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndexNullable<TypeDefinition, RawTypeDefinition>( TableIndexSchemas.TypeDefOrRef, ( r, v ) => { r.BaseType = v; return true; }, row => row.BaseType, ( r, v ) => r.BaseType = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<TypeDefinition, RawTypeDefinition>( Tables.Field, ( r, v ) => { r.FieldList = v; return true; }, row => row.FieldList, ( r, v ) => r.FieldList = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<TypeDefinition, RawTypeDefinition>( Tables.MethodDef, ( r, v ) => { r.MethodList = v; return true; }, row => row.MethodList, ( r, v ) => r.MethodList = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<FieldDefinitionPointer>> GetFieldPtrColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<FieldDefinitionPointer, RawFieldDefinitionPointer>( Tables.Field, ( r, v ) => { r.FieldIndex = v; return true; }, row => row.FieldIndex, ( r, v ) => r.FieldIndex = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<FieldDefinition>> GetFieldDefColumns()
      {
         yield return MetaDataColumnInformationFactory.Constant16<FieldDefinition, RawFieldDefinition, FieldAttributes>( ( r, v ) => { r.Attributes = v; return true; }, row => row.Attributes, ( r, v ) => r.Attributes = (FieldAttributes) v, i => (FieldAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformationFactory.SystemString<FieldDefinition, RawFieldDefinition>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.BLOBNonTypeSignature<FieldDefinition, RawFieldDefinition, FieldSignature>( ( r, v ) => { r.Signature = v; return true; }, row => row.Signature, ( r, v ) => r.Signature = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MethodDefinitionPointer>> GetMethodPtrColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<MethodDefinitionPointer, RawMethodDefinitionPointer>( Tables.MethodDef, ( r, v ) => { r.MethodIndex = v; return true; }, row => row.MethodIndex, ( r, v ) => r.MethodIndex = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MethodDefinition>> GetMethodDefColumns()
      {
         yield return MetaDataColumnInformationFactory.DataReference<MethodDefinition, RawMethodDefinition, MethodILDefinition>( ( r, v ) => { r.IL = v; return true; }, r => r.IL, ( r, v ) => r.RVA = v, ( args, v ) => args.Row.IL = DefaultMetaDataSerializationSupportProvider.DeserializeIL( args.RowArgs, v, args.Row ), ( md, mdStreamContainer ) => new SectionPart_MethodIL( md, mdStreamContainer.UserStrings ) );
         yield return MetaDataColumnInformationFactory.Constant16<MethodDefinition, RawMethodDefinition, MethodImplAttributes>( ( r, v ) => { r.ImplementationAttributes = v; return true; }, row => row.ImplementationAttributes, ( r, v ) => r.ImplementationAttributes = (MethodImplAttributes) v, i => (MethodImplAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformationFactory.Constant16<MethodDefinition, RawMethodDefinition, MethodAttributes>( ( r, v ) => { r.Attributes = v; return true; }, row => row.Attributes, ( r, v ) => r.Attributes = (MethodAttributes) v, i => (MethodAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformationFactory.SystemString<MethodDefinition, RawMethodDefinition>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.BLOBNonTypeSignature<MethodDefinition, RawMethodDefinition, MethodDefinitionSignature>( ( r, v ) => { r.Signature = v; return true; }, row => row.Signature, ( r, v ) => r.Signature = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<MethodDefinition, RawMethodDefinition>( Tables.Parameter, ( r, v ) => { r.ParameterList = v; return true; }, row => row.ParameterList, ( r, v ) => r.ParameterList = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<ParameterDefinitionPointer>> GetParamPtrColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<ParameterDefinitionPointer, RawParameterDefinitionPointer>( Tables.Parameter, ( r, v ) => { r.ParameterIndex = v; return true; }, row => row.ParameterIndex, ( r, v ) => r.ParameterIndex = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<ParameterDefinition>> GetParamColumns()
      {
         yield return MetaDataColumnInformationFactory.Constant16<ParameterDefinition, RawParameterDefinition, ParameterAttributes>( ( r, v ) => { r.Attributes = v; return true; }, row => row.Attributes, ( r, v ) => r.Attributes = (ParameterAttributes) v, i => (ParameterAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformationFactory.Number32_SerializedAs16<ParameterDefinition, RawParameterDefinition>( ( r, v ) => { r.Sequence = v; return true; }, row => row.Sequence, ( r, v ) => r.Sequence = v );
         yield return MetaDataColumnInformationFactory.SystemString<ParameterDefinition, RawParameterDefinition>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<InterfaceImplementation>> GetInterfaceImplColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<InterfaceImplementation, RawInterfaceImplementation>( Tables.TypeDef, ( r, v ) => { r.Class = v; return true; }, row => row.Class, ( r, v ) => r.Class = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<InterfaceImplementation, RawInterfaceImplementation>( TableIndexSchemas.TypeDefOrRef, ( r, v ) => { r.Interface = v; return true; }, row => row.Interface, ( r, v ) => r.Interface = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MemberReference>> GetMemberRefColumns()
      {
         yield return MetaDataColumnInformationFactory.CodedTableIndex<MemberReference, RawMemberReference>( TableIndexSchemas.MemberRefParent, ( r, v ) => { r.DeclaringType = v; return true; }, row => row.DeclaringType, ( r, v ) => r.DeclaringType = v );
         yield return MetaDataColumnInformationFactory.SystemString<MemberReference, RawMemberReference>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.BLOBNonTypeSignature<MemberReference, RawMemberReference, AbstractSignature>( ( r, v ) => { r.Signature = v; return true; }, row => row.Signature, ( r, v ) => r.Signature = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<ConstantDefinition>> GetConstantColumns()
      {
         yield return MetaDataColumnInformationFactory.Constant8<ConstantDefinition, RawConstantDefinition, SignatureElementTypes>( ( r, v ) => { r.Type = v; return true; }, row => row.Type, ( r, v ) => r.Type = (SignatureElementTypes) v, i => (SignatureElementTypes) i, v => (Int32) v );
         yield return MetaDataColumnInformationFactory.Number8<ConstantDefinition, RawConstantDefinition>( ( r, v ) => { return true; }, row => 0, ( r, v ) => r.Padding = (Byte) v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<ConstantDefinition, RawConstantDefinition>( TableIndexSchemas.HasConstant, ( r, v ) => { r.Parent = v; return true; }, row => row.Parent, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformationFactory.BLOBCustom<ConstantDefinition, RawConstantDefinition, Object>( ( r, v ) => { r.Value = v; return true; }, r => r.Value, ( r, v ) => r.Value = v, ( args, v, blobs ) => args.Row.Value = blobs.ReadConstantValue( v, args.Row.Type ), args => args.RowArgs.Array.CreateConstantBytes( args.Row.Value, args.Row.Type ) );
      }

      protected static IEnumerable<MetaDataColumnInformation<CustomAttributeDefinition>> GetCustomAttributeColumns()
      {
         yield return MetaDataColumnInformationFactory.CodedTableIndex<CustomAttributeDefinition, RawCustomAttributeDefinition>( TableIndexSchemas.HasCustomAttribute, ( r, v ) => { r.Parent = v; return true; }, row => row.Parent, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<CustomAttributeDefinition, RawCustomAttributeDefinition>( TableIndexSchemas.CustomAttributeType, ( r, v ) => { r.Type = v; return true; }, row => row.Type, ( r, v ) => r.Type = v );
         yield return MetaDataColumnInformationFactory.BLOBCustom<CustomAttributeDefinition, RawCustomAttributeDefinition, AbstractCustomAttributeSignature>( ( r, v ) => { r.Signature = v; return true; }, r => r.Signature, ( r, v ) => r.Signature = v, ( args, v, blobs ) => args.Row.Signature = blobs.ReadCASignature( v ), args => args.RowArgs.Array.CreateCustomAttributeSignature( args.RowArgs.MetaData, args.RowIndex ) );
      }

      protected static IEnumerable<MetaDataColumnInformation<FieldMarshal>> GetFieldMarshalColumns()
      {
         yield return MetaDataColumnInformationFactory.CodedTableIndex<FieldMarshal, RawFieldMarshal>( TableIndexSchemas.HasFieldMarshal, ( r, v ) => { r.Parent = v; return true; }, row => row.Parent, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformationFactory.BLOBCustom<FieldMarshal, RawFieldMarshal, AbstractMarshalingInfo>( ( r, v ) => { r.NativeType = v; return true; }, row => row.NativeType, ( r, v ) => r.NativeType = v, ( args, v, blobs ) => args.Row.NativeType = blobs.ReadMarshalingInfo( v ), args => args.RowArgs.Array.CreateMarshalSpec( args.Row.NativeType ) );
      }

      protected static IEnumerable<MetaDataColumnInformation<SecurityDefinition>> GetDeclSecurityColumns()
      {
         yield return MetaDataColumnInformationFactory.Constant16<SecurityDefinition, RawSecurityDefinition, SecurityAction>( ( r, v ) => { r.Action = v; return true; }, row => row.Action, ( r, v ) => r.Action = (SecurityAction) v, i => (SecurityAction) i, v => (Int32) v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<SecurityDefinition, RawSecurityDefinition>( TableIndexSchemas.HasSecurity, ( r, v ) => { r.Parent = v; return true; }, row => row.Parent, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformationFactory.BLOBCustom<SecurityDefinition, RawSecurityDefinition, List<AbstractSecurityInformation>>( ( r, v ) => { r.PermissionSets.Clear(); r.PermissionSets.AddRange( v ); return true; }, row => row.PermissionSets, ( r, v ) => r.PermissionSets = v, ( args, v, blobs ) => blobs.ReadSecurityInformation( v, args.Row.PermissionSets ), args => args.RowArgs.Array.CreateSecuritySignature( args.Row.PermissionSets, args.RowArgs.AuxArray ) );
      }

      protected static IEnumerable<MetaDataColumnInformation<ClassLayout>> GetClassLayoutColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32_SerializedAs16<ClassLayout, RawClassLayout>( ( r, v ) => { r.PackingSize = v; return true; }, row => row.PackingSize, ( r, v ) => r.PackingSize = v );
         yield return MetaDataColumnInformationFactory.Number32<ClassLayout, RawClassLayout>( ( r, v ) => { r.ClassSize = v; return true; }, row => row.ClassSize, ( r, v ) => r.ClassSize = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<ClassLayout, RawClassLayout>( Tables.TypeDef, ( r, v ) => { r.Parent = v; return true; }, row => row.Parent, ( r, v ) => r.Parent = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<FieldLayout>> GetFieldLayoutColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<FieldLayout, RawFieldLayout>( ( r, v ) => { r.Offset = v; return true; }, row => row.Offset, ( r, v ) => r.Offset = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<FieldLayout, RawFieldLayout>( Tables.Field, ( r, v ) => { r.Field = v; return true; }, row => row.Field, ( r, v ) => r.Field = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<StandaloneSignature>> GetStandaloneSigColumns()
      {
         yield return MetaDataColumnInformationFactory.BLOBCustom<StandaloneSignature, RawStandaloneSignature, AbstractSignature>( ( r, v ) =>
        {
           r.Signature = v;
           return true;
        }, r => r.Signature, ( r, v ) => r.Signature = v, ( args, v, blobs ) =>
        {
           Boolean wasFieldSig;
           args.Row.Signature = blobs.ReadNonTypeSignature( v, false, true, out wasFieldSig );
           args.Row.StoreSignatureAsFieldSignature = wasFieldSig;
        }, args => args.RowArgs.Array.CreateStandaloneSignature( args.Row ) );
      }

      protected static IEnumerable<MetaDataColumnInformation<EventMap>> GetEventMapColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<EventMap, RawEventMap>( Tables.TypeDef, ( r, v ) => { r.Parent = v; return true; }, row => row.Parent, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<EventMap, RawEventMap>( Tables.Event, ( r, v ) => { r.EventList = v; return true; }, row => row.EventList, ( r, v ) => r.EventList = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<EventDefinitionPointer>> GetEventPtrColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<EventDefinitionPointer, RawEventDefinitionPointer>( Tables.Event, ( r, v ) => { r.EventIndex = v; return true; }, row => row.EventIndex, ( r, v ) => r.EventIndex = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<EventDefinition>> GetEventDefColumns()
      {
         yield return MetaDataColumnInformationFactory.Constant16<EventDefinition, RawEventDefinition, EventAttributes>( ( r, v ) => { r.Attributes = v; return true; }, row => row.Attributes, ( r, v ) => r.Attributes = (EventAttributes) v, i => (EventAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformationFactory.SystemString<EventDefinition, RawEventDefinition>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<EventDefinition, RawEventDefinition>( TableIndexSchemas.TypeDefOrRef, ( r, v ) => { r.EventType = v; return true; }, row => row.EventType, ( r, v ) => r.EventType = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<PropertyMap>> GetPropertyMapColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<PropertyMap, RawPropertyMap>( Tables.TypeDef, ( r, v ) => { r.Parent = v; return true; }, row => row.Parent, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<PropertyMap, RawPropertyMap>( Tables.Property, ( r, v ) => { r.PropertyList = v; return true; }, row => row.PropertyList, ( r, v ) => r.PropertyList = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<PropertyDefinitionPointer>> GetPropertyPtrColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<PropertyDefinitionPointer, RawPropertyDefinitionPointer>( Tables.Property, ( r, v ) => { r.PropertyIndex = v; return true; }, row => row.PropertyIndex, ( r, v ) => r.PropertyIndex = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<PropertyDefinition>> GetPropertyDefColumns()
      {
         yield return MetaDataColumnInformationFactory.Constant16<PropertyDefinition, RawPropertyDefinition, PropertyAttributes>( ( r, v ) => { r.Attributes = v; return true; }, row => row.Attributes, ( r, v ) => r.Attributes = (PropertyAttributes) v, i => (PropertyAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformationFactory.SystemString<PropertyDefinition, RawPropertyDefinition>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.BLOBNonTypeSignature<PropertyDefinition, RawPropertyDefinition, PropertySignature>( ( r, v ) => { r.Signature = v; return true; }, row => row.Signature, ( r, v ) => r.Signature = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MethodSemantics>> GetMethodSemanticsColumns()
      {
         yield return MetaDataColumnInformationFactory.Constant16<MethodSemantics, RawMethodSemantics, MethodSemanticsAttributes>( ( r, v ) => { r.Attributes = v; return true; }, row => row.Attributes, ( r, v ) => r.Attributes = (MethodSemanticsAttributes) v, i => (MethodSemanticsAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<MethodSemantics, RawMethodSemantics>( Tables.MethodDef, ( r, v ) => { r.Method = v; return true; }, row => row.Method, ( r, v ) => r.Method = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<MethodSemantics, RawMethodSemantics>( TableIndexSchemas.HasSemantics, ( r, v ) => { r.Associaton = v; return true; }, row => row.Associaton, ( r, v ) => r.Associaton = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MethodImplementation>> GetMethodImplColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<MethodImplementation, RawMethodImplementation>( Tables.TypeDef, ( r, v ) => { r.Class = v; return true; }, row => row.Class, ( r, v ) => r.Class = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<MethodImplementation, RawMethodImplementation>( TableIndexSchemas.MethodDefOrRef, ( r, v ) => { r.MethodBody = v; return true; }, row => row.MethodBody, ( r, v ) => r.MethodBody = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<MethodImplementation, RawMethodImplementation>( TableIndexSchemas.MethodDefOrRef, ( r, v ) => { r.MethodDeclaration = v; return true; }, row => row.MethodDeclaration, ( r, v ) => r.MethodDeclaration = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<ModuleReference>> GetModuleRefColumns()
      {
         yield return MetaDataColumnInformationFactory.SystemString<ModuleReference, RawModuleReference>( ( r, v ) => { r.ModuleName = v; return true; }, row => row.ModuleName, ( r, v ) => r.ModuleName = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<TypeSpecification>> GetTypeSpecColumns()
      {
         yield return MetaDataColumnInformationFactory.BLOBTypeSignature<TypeSpecification, RawTypeSpecification>( ( r, v ) => { r.Signature = v; return true; }, row => row.Signature, ( r, v ) => r.Signature = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MethodImplementationMap>> GetImplMapColumns()
      {
         yield return MetaDataColumnInformationFactory.Constant16<MethodImplementationMap, RawMethodImplementationMap, PInvokeAttributes>( ( r, v ) => { r.Attributes = v; return true; }, row => row.Attributes, ( r, v ) => r.Attributes = (PInvokeAttributes) v, i => (PInvokeAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<MethodImplementationMap, RawMethodImplementationMap>( TableIndexSchemas.MemberForwarded, ( r, v ) => { r.MemberForwarded = v; return true; }, row => row.MemberForwarded, ( r, v ) => r.MemberForwarded = v );
         yield return MetaDataColumnInformationFactory.SystemString<MethodImplementationMap, RawMethodImplementationMap>( ( r, v ) => { r.ImportName = v; return true; }, row => row.ImportName, ( r, v ) => r.ImportName = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<MethodImplementationMap, RawMethodImplementationMap>( Tables.ModuleRef, ( r, v ) => { r.ImportScope = v; return true; }, row => row.ImportScope, ( r, v ) => r.ImportScope = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<FieldRVA>> GetFieldRVAColumns()
      {
         yield return MetaDataColumnInformationFactory.DataReference<FieldRVA, RawFieldRVA, Byte[]>( ( r, v ) => { r.Data = v; return true; }, r => r.Data, ( r, v ) => r.RVA = v, ( args, rva ) => args.Row.Data = DefaultMetaDataSerializationSupportProvider.DeserializeConstantValue( args.RowArgs, args.Row, rva ), ( md, mdStreamContainer ) => new SectionPart_FieldRVA( md ) );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<FieldRVA, RawFieldRVA>( Tables.Field, ( r, v ) => { r.Field = v; return true; }, row => row.Field, ( r, v ) => r.Field = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<EditAndContinueLog>> GetENCLogColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<EditAndContinueLog, RawEditAndContinueLog>( ( r, v ) => { r.Token = v; return true; }, row => row.Token, ( r, v ) => r.Token = v );
         yield return MetaDataColumnInformationFactory.Number32<EditAndContinueLog, RawEditAndContinueLog>( ( r, v ) => { r.FuncCode = v; return true; }, row => row.FuncCode, ( r, v ) => r.FuncCode = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<EditAndContinueMap>> GetENCMapColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<EditAndContinueMap, RawEditAndContinueMap>( ( r, v ) => { r.Token = v; return true; }, row => row.Token, ( r, v ) => r.Token = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<AssemblyDefinition>> GetAssemblyDefColumns()
      {
         yield return MetaDataColumnInformationFactory.Constant32<AssemblyDefinition, RawAssemblyDefinition, AssemblyHashAlgorithm>( ( r, v ) => { r.HashAlgorithm = v; return true; }, row => row.HashAlgorithm, ( r, v ) => r.HashAlgorithm = (AssemblyHashAlgorithm) v, i => (AssemblyHashAlgorithm) i, v => (Int32) v );
         yield return MetaDataColumnInformationFactory.Number32_SerializedAs16<AssemblyDefinition, RawAssemblyDefinition>( ( r, v ) => { r.AssemblyInformation.VersionMajor = v; return true; }, row => row.AssemblyInformation.VersionMajor, ( r, v ) => r.MajorVersion = v );
         yield return MetaDataColumnInformationFactory.Number32_SerializedAs16<AssemblyDefinition, RawAssemblyDefinition>( ( r, v ) => { r.AssemblyInformation.VersionMinor = v; return true; }, row => row.AssemblyInformation.VersionMinor, ( r, v ) => r.MinorVersion = v );
         yield return MetaDataColumnInformationFactory.Number32_SerializedAs16<AssemblyDefinition, RawAssemblyDefinition>( ( r, v ) => { r.AssemblyInformation.VersionBuild = v; return true; }, row => row.AssemblyInformation.VersionBuild, ( r, v ) => r.BuildNumber = v );
         yield return MetaDataColumnInformationFactory.Number32_SerializedAs16<AssemblyDefinition, RawAssemblyDefinition>( ( r, v ) => { r.AssemblyInformation.VersionRevision = v; return true; }, row => row.AssemblyInformation.VersionRevision, ( r, v ) => r.RevisionNumber = v );
         yield return MetaDataColumnInformationFactory.Constant32<AssemblyDefinition, RawAssemblyDefinition, AssemblyFlags>( ( r, v ) => { r.Attributes = v; return true; }, row => row.Attributes, ( r, v ) => r.Attributes = (AssemblyFlags) v, i => (AssemblyFlags) i, v => (Int32) v );
         yield return MetaDataColumnInformationFactory.BLOBCustom<AssemblyDefinition, RawAssemblyDefinition, Byte[]>( ( r, v ) => { r.AssemblyInformation.PublicKeyOrToken = v; return true; }, r => r.AssemblyInformation.PublicKeyOrToken, ( r, v ) => r.PublicKey = v, ( args, v, blobs ) => args.Row.AssemblyInformation.PublicKeyOrToken = blobs.GetBLOB( v ), args =>
{
   var pk = args.Row.AssemblyInformation.PublicKeyOrToken;
   return pk.IsNullOrEmpty() ? args.RowArgs.ThisAssemblyPublicKeyIfPresentNull?.ToArray() : pk;
} );
         yield return MetaDataColumnInformationFactory.SystemString<AssemblyDefinition, RawAssemblyDefinition>( ( r, v ) => { r.AssemblyInformation.Name = v; return true; }, row => row.AssemblyInformation.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.SystemString<AssemblyDefinition, RawAssemblyDefinition>( ( r, v ) => { r.AssemblyInformation.Culture = v; return true; }, row => row.AssemblyInformation.Culture, ( r, v ) => r.Culture = v );
      }
#pragma warning disable 618
      protected static IEnumerable<MetaDataColumnInformation<AssemblyDefinitionProcessor>> GetAssemblyDefProcessorColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<AssemblyDefinitionProcessor, RawAssemblyDefinitionProcessor>( ( r, v ) => { r.Processor = v; return true; }, row => row.Processor, ( r, v ) => r.Processor = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<AssemblyDefinitionOS>> GetAssemblyDefOSColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<AssemblyDefinitionOS, RawAssemblyDefinitionOS>( ( r, v ) => { r.OSPlatformID = v; return true; }, row => row.OSPlatformID, ( r, v ) => r.OSPlatformID = v );
         yield return MetaDataColumnInformationFactory.Number32<AssemblyDefinitionOS, RawAssemblyDefinitionOS>( ( r, v ) => { r.OSMajorVersion = v; return true; }, row => row.OSMajorVersion, ( r, v ) => r.OSMajorVersion = v );
         yield return MetaDataColumnInformationFactory.Number32<AssemblyDefinitionOS, RawAssemblyDefinitionOS>( ( r, v ) => { r.OSMinorVersion = v; return true; }, row => row.OSMinorVersion, ( r, v ) => r.OSMinorVersion = v );
      }
#pragma warning restore 618

      protected static IEnumerable<MetaDataColumnInformation<AssemblyReference>> GetAssemblyRefColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32_SerializedAs16<AssemblyReference, RawAssemblyReference>( ( r, v ) => { r.AssemblyInformation.VersionMajor = v; return true; }, row => row.AssemblyInformation.VersionMajor, ( r, v ) => r.MajorVersion = v );
         yield return MetaDataColumnInformationFactory.Number32_SerializedAs16<AssemblyReference, RawAssemblyReference>( ( r, v ) => { r.AssemblyInformation.VersionMinor = v; return true; }, row => row.AssemblyInformation.VersionMinor, ( r, v ) => r.MinorVersion = v );
         yield return MetaDataColumnInformationFactory.Number32_SerializedAs16<AssemblyReference, RawAssemblyReference>( ( r, v ) => { r.AssemblyInformation.VersionBuild = v; return true; }, row => row.AssemblyInformation.VersionBuild, ( r, v ) => r.BuildNumber = v );
         yield return MetaDataColumnInformationFactory.Number32_SerializedAs16<AssemblyReference, RawAssemblyReference>( ( r, v ) => { r.AssemblyInformation.VersionRevision = v; return true; }, row => row.AssemblyInformation.VersionRevision, ( r, v ) => r.RevisionNumber = v );
         yield return MetaDataColumnInformationFactory.Constant32<AssemblyReference, RawAssemblyReference, AssemblyFlags>( ( r, v ) => { r.Attributes = v; return true; }, row => row.Attributes, ( r, v ) => r.Attributes = (AssemblyFlags) v, i => (AssemblyFlags) i, v => (Int32) v );
         yield return MetaDataColumnInformationFactory.BLOBByteArray<AssemblyReference, RawAssemblyReference>( ( r, v ) => { r.AssemblyInformation.PublicKeyOrToken = v; return true; }, r => r.AssemblyInformation.PublicKeyOrToken, ( r, v ) => r.PublicKeyOrToken = v );
         yield return MetaDataColumnInformationFactory.SystemString<AssemblyReference, RawAssemblyReference>( ( r, v ) => { r.AssemblyInformation.Name = v; return true; }, row => row.AssemblyInformation.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.SystemString<AssemblyReference, RawAssemblyReference>( ( r, v ) => { r.AssemblyInformation.Culture = v; return true; }, row => row.AssemblyInformation.Culture, ( r, v ) => r.Culture = v );
         yield return MetaDataColumnInformationFactory.BLOBByteArray<AssemblyReference, RawAssemblyReference>( ( r, v ) => { r.HashValue = v; return true; }, row => row.HashValue, ( r, v ) => r.HashValue = v );
      }

#pragma warning disable 618
      protected static IEnumerable<MetaDataColumnInformation<AssemblyReferenceProcessor>> GetAssemblyRefProcessorColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<AssemblyReferenceProcessor, RawAssemblyReferenceProcessor>( ( r, v ) => { r.Processor = v; return true; }, row => row.Processor, ( r, v ) => r.Processor = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<AssemblyReferenceProcessor, RawAssemblyReferenceProcessor>( Tables.AssemblyRef, ( r, v ) => { r.AssemblyRef = v; return true; }, row => row.AssemblyRef, ( r, v ) => r.AssemblyRef = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<AssemblyReferenceOS>> GetAssemblyRefOSColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<AssemblyReferenceOS, RawAssemblyReferenceOS>( ( r, v ) => { r.OSPlatformID = v; return true; }, row => row.OSPlatformID, ( r, v ) => r.OSPlatformID = v );
         yield return MetaDataColumnInformationFactory.Number32<AssemblyReferenceOS, RawAssemblyReferenceOS>( ( r, v ) => { r.OSMajorVersion = v; return true; }, row => row.OSMajorVersion, ( r, v ) => r.OSMajorVersion = v );
         yield return MetaDataColumnInformationFactory.Number32<AssemblyReferenceOS, RawAssemblyReferenceOS>( ( r, v ) => { r.OSMinorVersion = v; return true; }, row => row.OSMinorVersion, ( r, v ) => r.OSMinorVersion = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<AssemblyReferenceOS, RawAssemblyReferenceOS>( Tables.AssemblyRef, ( r, v ) => { r.AssemblyRef = v; return true; }, row => row.AssemblyRef, ( r, v ) => r.AssemblyRef = v );
      }
#pragma warning restore 618

      protected static IEnumerable<MetaDataColumnInformation<FileReference>> GetFileColumns()
      {
         yield return MetaDataColumnInformationFactory.Constant32<FileReference, RawFileReference, FileAttributes>( ( r, v ) => { r.Attributes = v; return true; }, row => row.Attributes, ( r, v ) => r.Attributes = (FileAttributes) v, i => (FileAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformationFactory.SystemString<FileReference, RawFileReference>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.BLOBByteArray<FileReference, RawFileReference>( ( r, v ) => { r.HashValue = v; return true; }, row => row.HashValue, ( r, v ) => r.HashValue = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<ExportedType>> GetExportedTypeColumns()
      {
         yield return MetaDataColumnInformationFactory.Constant32<ExportedType, RawExportedType, TypeAttributes>( ( r, v ) => { r.Attributes = v; return true; }, row => row.Attributes, ( r, v ) => r.Attributes = (TypeAttributes) v, i => (TypeAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformationFactory.Number32<ExportedType, RawExportedType>( ( r, v ) => { r.TypeDefinitionIndex = v; return true; }, row => row.TypeDefinitionIndex, ( r, v ) => r.TypeDefinitionIndex = v );
         yield return MetaDataColumnInformationFactory.SystemString<ExportedType, RawExportedType>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.SystemString<ExportedType, RawExportedType>( ( r, v ) => { r.Namespace = v; return true; }, row => row.Namespace, ( r, v ) => r.Namespace = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<ExportedType, RawExportedType>( TableIndexSchemas.Implementation, ( r, v ) => { r.Implementation = v; return true; }, row => row.Implementation, ( r, v ) => r.Implementation = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<ManifestResource>> GetManifestResourceColumns()
      {
         yield return MetaDataColumnInformationFactory.DataReference<ManifestResource, RawManifestResource, Int32>( ( r, v ) =>
        {
           r.Offset = v; return true;
        }, r => r.Offset, ( r, v ) => r.Offset = v, ( args, offset ) =>
{
   var row = args.Row;
   row.Offset = offset;
   if ( !row.Implementation.HasValue )
   {
      row.DataInCurrentFile = DefaultMetaDataSerializationSupportProvider.DeserializeEmbeddedManifest( args.RowArgs, offset );
   }
},
         ( md, mdStreamContainer ) => new SectionPart_EmbeddedManifests( md ) );
         yield return MetaDataColumnInformationFactory.Constant32<ManifestResource, RawManifestResource, ManifestResourceAttributes>( ( r, v ) => { r.Attributes = v; return true; }, row => row.Attributes, ( r, v ) => r.Attributes = (ManifestResourceAttributes) v, i => (ManifestResourceAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformationFactory.SystemString<ManifestResource, RawManifestResource>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndexNullable<ManifestResource, RawManifestResource>( TableIndexSchemas.Implementation, ( r, v ) => { r.Implementation = v; return true; }, row => row.Implementation, ( r, v ) => r.Implementation = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<NestedClassDefinition>> GetNestedClassColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<NestedClassDefinition, RawNestedClassDefinition>( Tables.TypeDef, ( r, v ) => { r.NestedClass = v; return true; }, row => row.NestedClass, ( r, v ) => r.NestedClass = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<NestedClassDefinition, RawNestedClassDefinition>( Tables.TypeDef, ( r, v ) => { r.EnclosingClass = v; return true; }, row => row.EnclosingClass, ( r, v ) => r.EnclosingClass = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<GenericParameterDefinition>> GetGenericParamColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32_SerializedAs16<GenericParameterDefinition, RawGenericParameterDefinition>( ( r, v ) => { r.GenericParameterIndex = v; return true; }, row => row.GenericParameterIndex, ( r, v ) => r.GenericParameterIndex = v );
         yield return MetaDataColumnInformationFactory.Constant16<GenericParameterDefinition, RawGenericParameterDefinition, GenericParameterAttributes>( ( r, v ) => { r.Attributes = v; return true; }, row => row.Attributes, ( r, v ) => r.Attributes = (GenericParameterAttributes) v, i => (GenericParameterAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<GenericParameterDefinition, RawGenericParameterDefinition>( TableIndexSchemas.TypeOrMethodDef, ( r, v ) => { r.Owner = v; return true; }, row => row.Owner, ( r, v ) => r.Owner = v );
         yield return MetaDataColumnInformationFactory.SystemString<GenericParameterDefinition, RawGenericParameterDefinition>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MethodSpecification>> GetMethodSpecColumns()
      {
         yield return MetaDataColumnInformationFactory.CodedTableIndex<MethodSpecification, RawMethodSpecification>( TableIndexSchemas.MethodDefOrRef, ( r, v ) => { r.Method = v; return true; }, row => row.Method, ( r, v ) => r.Method = v );
         yield return MetaDataColumnInformationFactory.BLOBNonTypeSignature<MethodSpecification, RawMethodSpecification, GenericMethodSignature>( ( r, v ) => { r.Signature = v; return true; }, row => row.Signature, ( r, v ) => r.Signature = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<GenericParameterConstraintDefinition>> GetGenericParamConstraintColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<GenericParameterConstraintDefinition, RawGenericParameterConstraintDefinition>( Tables.GenericParameter, ( r, v ) => { r.Owner = v; return true; }, row => row.Owner, ( r, v ) => r.Owner = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<GenericParameterConstraintDefinition, RawGenericParameterConstraintDefinition>( TableIndexSchemas.TypeDefOrRef, ( r, v ) => { r.Constraint = v; return true; }, row => row.Constraint, ( r, v ) => r.Constraint = v );
      }

   }

   public interface MetaDataTableInformationWithSerializationCapability
   {
      TableSerializationInfo CreateTableSerializationInfoNotGeneric( TableSerializationInfoCreationArgs args );
   }

   public struct TableSerializationInfoCreationArgs
   {
      public TableSerializationInfoCreationArgs( EventHandler<SerializationErrorEventArgs> errorHandler )
      {
         this.ErrorHandler = errorHandler;
      }

      public EventHandler<SerializationErrorEventArgs> ErrorHandler { get; }
   }

   public sealed class MetaDataTableInformation<TRow, TRawRow> : MetaDataTableInformation<TRow>, MetaDataTableInformationWithSerializationCapability
      where TRow : class
      where TRawRow : class
   {
      private readonly Func<TRawRow> _rawRowFactory;

      public MetaDataTableInformation(
         Tables tableKind,
         IEqualityComparer<TRow> equalityComparer,
         IComparer<TRow> comparer,
         Func<TRow> rowFactory,
         IEnumerable<MetaDataColumnInformation<TRow>> columns,
         Func<TRawRow> rawRowFactory,
         Boolean isSorted
         )
         : base( (Int32) tableKind, equalityComparer, comparer, rowFactory, columns )
      {
         ArgumentValidator.ValidateNotNull( "Raw row factory", rawRowFactory );

         this._rawRowFactory = rawRowFactory;
         this.IsSortedForSerialization = isSorted;
      }


      public TRawRow CreateRawRow()
      {
         return this._rawRowFactory();
      }

      public DefaultTableSerializationInfo<TRawRow, TRow> CreateTableSerializationInfo( TableSerializationInfoCreationArgs args )
      {
         return new DefaultTableSerializationInfo<TRawRow, TRow>(
            (Tables) this.TableIndex,
            this.IsSortedForSerialization,
            this.ColumnsInformation.Select( c => ( c as MetaDataColumnInformationWithSerializationCapability<TRow, TRawRow> )?.DefaultColumnSerializationInfoWithRawType ),
            this.RowFactory,
            this._rawRowFactory,
            args
            );
      }

      public Boolean IsSortedForSerialization { get; }

      public TableSerializationInfo CreateTableSerializationInfoNotGeneric( TableSerializationInfoCreationArgs args )
      {
         return this.CreateTableSerializationInfo( args );
      }
   }

   public static class MetaDataColumnInformationFactory
   {

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue> Constant8<TRow, TRawRow, TValue>(
         RowColumnSetterDelegate<TRow, TValue> setter,
         RowColumnGetterDelegate<TRow, TValue> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         Func<Int32, TValue> fromInteger,
         Func<TValue, Int32> toInteger
         )
         where TRow : class
         where TRawRow : class
         where TValue : struct
      {
         return ConstantCustom<TRow, TRawRow, TValue>(
            sizeof( Byte ),
            getter,
            setter,
            () => DefaultColumnSerializationInfoFactory.Constant8<TRawRow, TRow>( rawSetter, ( args, v ) => setter( args.Row, fromInteger( v ) ), r => toInteger( getter( r ) ) )
            );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue> Constant16<TRow, TRawRow, TValue>(
         RowColumnSetterDelegate<TRow, TValue> setter,
         RowColumnGetterDelegate<TRow, TValue> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         Func<Int32, TValue> fromInteger,
         Func<TValue, Int32> toInteger
         )
         where TRow : class
         where TRawRow : class
         where TValue : struct
      {
         return ConstantCustom<TRow, TRawRow, TValue>(
            sizeof( Int16 ),
            getter,
            setter,
            () => DefaultColumnSerializationInfoFactory.Constant16<TRawRow, TRow>( rawSetter, ( args, v ) => setter( args.Row, fromInteger( v ) ), r => toInteger( getter( r ) ) )
            );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue> Constant32<TRow, TRawRow, TValue>(
         RowColumnSetterDelegate<TRow, TValue> setter,
         RowColumnGetterDelegate<TRow, TValue> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         Func<Int32, TValue> fromInteger,
         Func<TValue, Int32> toInteger
         )
         where TRow : class
         where TRawRow : class
         where TValue : struct
      {
         return ConstantCustom<TRow, TRawRow, TValue>(
            sizeof( Int32 ),
            getter,
            setter,
            () => DefaultColumnSerializationInfoFactory.Constant32<TRawRow, TRow>( rawSetter, ( args, v ) => setter( args.Row, fromInteger( v ) ), r => toInteger( getter( r ) ) )
            );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, Byte> Number8<TRow, TRawRow>(
         RowColumnSetterDelegate<TRow, Byte> setter,
         RowColumnGetterDelegate<TRow, Byte> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return ConstantCustom<TRow, TRawRow, Byte>(
            sizeof( Byte ),
            getter,
            setter,
            () => DefaultColumnSerializationInfoFactory.Constant8<TRawRow, TRow>( rawSetter, ( args, v ) => setter( args.Row, (Byte) v ), r => getter( r ) )
            );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, Int16> Number16<TRow, TRawRow>(
         RowColumnSetterDelegate<TRow, Int16> setter,
         RowColumnGetterDelegate<TRow, Int16> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return ConstantCustom<TRow, TRawRow, Int16>(
            sizeof( Int16 ),
            getter,
            setter,
            () => DefaultColumnSerializationInfoFactory.Constant16<TRawRow, TRow>( rawSetter, ( args, v ) => setter( args.Row, (Int16) v ), r => getter( r ) )
            );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, Int32> Number32_SerializedAs16<TRow, TRawRow>(
         RowColumnSetterDelegate<TRow, Int32> setter,
         RowColumnGetterDelegate<TRow, Int32> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return ConstantCustom<TRow, TRawRow, Int32>(
            sizeof( Int16 ),
            getter,
            setter,
            () => DefaultColumnSerializationInfoFactory.Constant16<TRawRow, TRow>( rawSetter, ( args, v ) => setter( args.Row, v ), r => getter( r ) )
            );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, Int32> Number32<TRow, TRawRow>(
         RowColumnSetterDelegate<TRow, Int32> setter,
         RowColumnGetterDelegate<TRow, Int32> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return ConstantCustom<TRow, TRawRow, Int32>(
            sizeof( Int32 ),
            getter,
            setter,
            () => DefaultColumnSerializationInfoFactory.Constant32<TRawRow, TRow>( rawSetter, ( args, v ) => setter( args.Row, v ), r => getter( r ) )
            );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue> ConstantCustom<TRow, TRawRow, TValue>(
         Int32 byteCount,
         RowColumnGetterDelegate<TRow, TValue> getter,
         RowColumnSetterDelegate<TRow, TValue> setter,
         Func<DefaultColumnSerializationInfo<TRawRow, TRow>> serializationInfo
         )
         where TRow : class
         where TRawRow : class
         where TValue : struct
      {
         return new MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue>( getter, setter, new MetaDataColumnDataInformation_FixedSizeConstant( byteCount ), serializationInfo );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, String> SystemString<TRow, TRawRow>(
         RowColumnSetterDelegate<TRow, String> setter,
         RowColumnGetterDelegate<TRow, String> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return new MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, String>( getter, setter, new MetaDataColumnDataInformation_HeapIndex( MetaDataConstants.SYS_STRING_STREAM_NAME ), () => DefaultColumnSerializationInfoFactory.SystemString<TRawRow, TRow>( rawSetter, ( args, v ) => setter( args.Row, v ), getter ) );
      }

      public static MetaDataColumnInformationForNullables<TRow, TRawRow, Guid> GUID<TRow, TRawRow>(
         RowColumnSetterDelegate<TRow, Guid?> setter,
         RowColumnGetterDelegate<TRow, Guid?> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return new MetaDataColumnInformationForNullables<TRow, TRawRow, Guid>( getter, setter, new MetaDataColumnDataInformation_HeapIndex( MetaDataConstants.GUID_STREAM_NAME ), () => DefaultColumnSerializationInfoFactory.GUID<TRawRow, TRow>( rawSetter, ( args, v ) => setter( args.Row, v ), getter ) );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TableIndex> SimpleTableIndex<TRow, TRawRow>(
         Tables targetTable,
         RowColumnSetterDelegate<TRow, TableIndex> setter,
         RowColumnGetterDelegate<TRow, TableIndex> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return new MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TableIndex>( getter, setter, new MetaDataColumnDataInformation_SimpleTableIndex( (Int32) targetTable ), () => DefaultColumnSerializationInfoFactory.SimpleReference<TRawRow, TRow>( targetTable, rawSetter, ( args, v ) => setter( args.Row, v ), getter ) );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TableIndex> CodedTableIndex<TRow, TRawRow>(
         ArrayQuery<Int32?> targetTables,
         RowColumnSetterDelegate<TRow, TableIndex> setter,
         RowColumnGetterDelegate<TRow, TableIndex> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return new MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TableIndex>( getter, setter, new MetaDataColumnDataInformation_CodedTableIndex( targetTables ), () => DefaultColumnSerializationInfoFactory.CodedReference<TRawRow, TRow>( targetTables, rawSetter, ( args, v ) => setter( args.Row, v.GetValueOrDefault() ), r => getter( r ) ) );
      }

      public static MetaDataColumnInformationForNullables<TRow, TRawRow, TableIndex> CodedTableIndexNullable<TRow, TRawRow>(
         ArrayQuery<Int32?> targetTables,
         RowColumnSetterDelegate<TRow, TableIndex?> setter,
         RowColumnGetterDelegate<TRow, TableIndex?> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return new MetaDataColumnInformationForNullables<TRow, TRawRow, TableIndex>( getter, setter, new MetaDataColumnDataInformation_CodedTableIndex( targetTables ), () => DefaultColumnSerializationInfoFactory.CodedReference<TRawRow, TRow>( targetTables, rawSetter, ( args, v ) => setter( args.Row, v ), getter ) );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TypeSignature> BLOBTypeSignature<TRow, TRawRow>(
         RowColumnSetterDelegate<TRow, TypeSignature> setter,
         RowColumnGetterDelegate<TRow, TypeSignature> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return BLOBCustom(
            setter,
            getter,
            rawSetter,
            ( args, value, blobs ) => setter( args.Row, blobs.ReadTypeSignature( value ) ),
            args => args.RowArgs.Array.CreateAnySignature( getter( args.Row ) )
            );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TSignature> BLOBNonTypeSignature<TRow, TRawRow, TSignature>(
         RowColumnSetterDelegate<TRow, TSignature> setter,
         RowColumnGetterDelegate<TRow, TSignature> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
         where TSignature : AbstractSignature
      {
         var isMethodDef = Equals( typeof( MethodDefinitionSignature ), typeof( TSignature ) );
         return BLOBCustom(
            setter,
            getter,
            rawSetter,
            ( args, value, blobs ) =>
            {
               Boolean wasFieldSig;
               setter( args.Row, blobs.ReadNonTypeSignature( value, isMethodDef, false, out wasFieldSig ) as TSignature );
            },
            args => args.RowArgs.Array.CreateAnySignature( getter( args.Row ) )
            );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, Byte[]> BLOBByteArray<TRow, TRawRow>(
         RowColumnSetterDelegate<TRow, Byte[]> setter,
         RowColumnGetterDelegate<TRow, Byte[]> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return BLOBCustom(
            setter,
            getter,
            rawSetter,
            ( args, heapIndex, blobs ) => setter( args.Row, blobs.GetBLOB( heapIndex ) ),
            args => getter( args.Row )
            );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue> BLOBCustom<TRow, TRawRow, TValue>(
         RowColumnSetterDelegate<TRow, TValue> setter,
         RowColumnGetterDelegate<TRow, TValue> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         Action<ColumnFunctionalityArgs<TRow, RowReadingArguments>, Int32, ReaderBLOBStreamHandler> blobReader, // TODO delegat-ize these
         Func<ColumnFunctionalityArgs<TRow, RowHeapFillingArguments>, Byte[]> blobCreator
         )
         where TRow : class
         where TRawRow : class
      {
         return new MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue>( getter, setter, new MetaDataColumnDataInformation_HeapIndex( MetaDataConstants.BLOB_STREAM_NAME ), () => DefaultColumnSerializationInfoFactory.BLOBCustom<TRawRow, TRow>( rawSetter, blobReader, blobCreator ) );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue> DataReference<TRow, TRawRow, TValue>(
         RowColumnSetterDelegate<TRow, TValue> setter,
         RowColumnGetterDelegate<TRow, TValue> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowRawColumnSetterDelegate<TRow> rawValueProcessor,
         RawColumnSectionPartCreationDelegte<TRow> rawColummnSectionPartCreator
         )
         where TRow : class
         where TRawRow : class
      {
         return new MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue>( getter, setter, new MetaDataColumnDataInformation_DataReference(), () => DefaultColumnSerializationInfoFactory.RawValueStorageColumn<TRawRow, TRow>( rawSetter, rawValueProcessor, rawColummnSectionPartCreator ) );
      }


   }

   public interface MetaDataColumnInformationWithSerializationCapability
   {
      DefaultColumnSerializationInfo DefaultColumnSerializationInfoNotGeneric { get; }
   }

   public interface MetaDataColumnInformationWithSerializationCapability<TRow, TRawRow> : MetaDataColumnInformationWithSerializationCapability
      where TRow : class
      where TRawRow : class
   {
      DefaultColumnSerializationInfo<TRawRow, TRow> DefaultColumnSerializationInfoWithRawType { get; }
   }

   public interface MetaDataColumnInformationWithDataMeaning
   {
      MetaDataColumnDataInformation DataInformation { get; }
   }

   public interface MetaDataColumnInformationWithRawRowType
   {
      Type RawRowType { get; }
   }


   public sealed class MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue> : MetaDataColumnInformationForClassesOrStructs<TRow, TValue>, MetaDataColumnInformationWithSerializationCapability<TRow, TRawRow>, MetaDataColumnInformationWithDataMeaning, MetaDataColumnInformationWithRawRowType
      where TRow : class
      where TRawRow : class
   {

      private readonly Lazy<DefaultColumnSerializationInfo<TRawRow, TRow>> _serializationInfo;

      public MetaDataColumnInformationForClassesOrStructs(
         RowColumnGetterDelegate<TRow, TValue> getter,
         RowColumnSetterDelegate<TRow, TValue> setter,
         MetaDataColumnDataInformation information,
         Func<DefaultColumnSerializationInfo<TRawRow, TRow>> defaultSerializationInfo
         )
         : base( getter, setter )
      {
         ArgumentValidator.ValidateNotNull( "Data information", information );

         this.DataInformation = information;
         // TODO check for null or not?
         this._serializationInfo = defaultSerializationInfo == null ? null : new Lazy<DefaultColumnSerializationInfo<TRawRow, TRow>>( defaultSerializationInfo, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );
      }

      public DefaultColumnSerializationInfo<TRawRow, TRow> DefaultColumnSerializationInfoWithRawType
      {
         get
         {
            return this._serializationInfo?.Value;
         }
      }

      public DefaultColumnSerializationInfo DefaultColumnSerializationInfoNotGeneric
      {
         get
         {
            return this.DefaultColumnSerializationInfoWithRawType;
         }
      }

      public Type RawRowType
      {
         get
         {
            return typeof( TRawRow );
         }
      }

      public MetaDataColumnDataInformation DataInformation { get; }
   }

   public sealed class MetaDataColumnInformationForNullables<TRow, TRawRow, TValue> : MetaDataColumnInformationForNullables<TRow, TValue>, MetaDataColumnInformationWithSerializationCapability<TRow, TRawRow>, MetaDataColumnInformationWithDataMeaning, MetaDataColumnInformationWithRawRowType
      where TRow : class
      where TValue : struct
      where TRawRow : class
   {

      private readonly Lazy<DefaultColumnSerializationInfo<TRawRow, TRow>> _serializationInfo;

      public MetaDataColumnInformationForNullables(
         RowColumnGetterDelegate<TRow, TValue?> getter,
         RowColumnSetterDelegate<TRow, TValue?> setter,
         MetaDataColumnDataInformation information,
         Func<DefaultColumnSerializationInfo<TRawRow, TRow>> defaultSerializationInfo
         )
         : base( getter, setter )
      {
         ArgumentValidator.ValidateNotNull( "Data information", information );

         this.DataInformation = information;
         // TODO check for null or not?
         this._serializationInfo = defaultSerializationInfo == null ? null : new Lazy<DefaultColumnSerializationInfo<TRawRow, TRow>>( defaultSerializationInfo, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );
      }

      public DefaultColumnSerializationInfo<TRawRow, TRow> DefaultColumnSerializationInfoWithRawType
      {
         get
         {
            return this._serializationInfo?.Value;
         }
      }

      public DefaultColumnSerializationInfo DefaultColumnSerializationInfoNotGeneric
      {
         get
         {
            return this.DefaultColumnSerializationInfoWithRawType;
         }
      }

      public Type RawRowType
      {
         get
         {
            return typeof( TRawRow );
         }
      }

      public MetaDataColumnDataInformation DataInformation { get; }
   }

   public abstract class MetaDataColumnDataInformation
   {
      internal MetaDataColumnDataInformation()
      {
      }

      public abstract MetaDataColumnInformationKind ColumnKind { get; }
   }

   public sealed class MetaDataColumnDataInformation_FixedSizeConstant : MetaDataColumnDataInformation
   {
      public MetaDataColumnDataInformation_FixedSizeConstant(
         Int32 byteSize
         )
         : base()
      {
         this.FixedSize = byteSize;
      }


      public Int32 FixedSize { get; }

      public override MetaDataColumnInformationKind ColumnKind
      {
         get
         {
            return MetaDataColumnInformationKind.FixedSizeConstant;
         }
      }
   }

   public sealed class MetaDataColumnDataInformation_SimpleTableIndex : MetaDataColumnDataInformation
   {
      public MetaDataColumnDataInformation_SimpleTableIndex(
         Int32 targetTable
         )
         : base()
      {
         this.TargetTable = targetTable;
      }

      public Int32 TargetTable { get; }

      public override MetaDataColumnInformationKind ColumnKind
      {
         get
         {
            return MetaDataColumnInformationKind.SimpleTableIndex;
         }
      }
   }

   public sealed class MetaDataColumnDataInformation_CodedTableIndex : MetaDataColumnDataInformation
   {
      public MetaDataColumnDataInformation_CodedTableIndex(
         IEnumerable<Int32?> targetTables
         )
         : base()
      {

         this.TargetTables = ( targetTables ?? Empty<Int32?>.Enumerable ).ToArrayProxy().CQ;
      }

      public ArrayQuery<Int32?> TargetTables { get; }

      public override MetaDataColumnInformationKind ColumnKind
      {
         get
         {
            return MetaDataColumnInformationKind.CodedTableIndex;
         }
      }
   }

   public sealed class MetaDataColumnDataInformation_HeapIndex : MetaDataColumnDataInformation
   {
      public MetaDataColumnDataInformation_HeapIndex(
         String heapName
         )
         : base()
      {
         ArgumentValidator.ValidateNotEmpty( "Heap name", heapName );

         this.HeapName = heapName;
      }

      public String HeapName { get; }

      public override MetaDataColumnInformationKind ColumnKind
      {
         get
         {
            return MetaDataColumnInformationKind.HeapIndex;
         }
      }
   }

   public sealed class MetaDataColumnDataInformation_DataReference : MetaDataColumnDataInformation
   {
      public MetaDataColumnDataInformation_DataReference(
         )
         : base()
      {

      }

      public override MetaDataColumnInformationKind ColumnKind
      {
         get
         {
            return MetaDataColumnInformationKind.DataReference;
         }
      }
   }

   public enum MetaDataColumnInformationKind
   {
      FixedSizeConstant,
      SimpleTableIndex,
      CodedTableIndex,
      HeapIndex,
      DataReference
   }

}
