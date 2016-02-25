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
extern alias CAMPhysicalR;
extern alias CAMPhysicalIO;
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;
using CAMPhysical::CILAssemblyManipulator.Physical.Meta;

using CAMPhysicalR;
using CAMPhysicalR::CILAssemblyManipulator.Physical.Resolving;

using CAMPhysicalIO;
using CAMPhysicalIO::CILAssemblyManipulator.Physical.IO;

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
   public class DefaultMetaDataTableInformationProvider : CILMetaDataTableInformationProvider, CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.CILMetaDataTableInformationProvider
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
      private readonly OpCodeProvider _opCodeProvider;
      private readonly SignatureProvider _sigProvider;

      public DefaultMetaDataTableInformationProvider(
         IEnumerable<MetaDataTableInformation> tableInfos = null,
         OpCodeProvider opCodeProvider = null,
         SignatureProvider sigProvider = null
         )
      {
         this._infos = new MetaDataTableInformation[CAMCoreInternals.AMOUNT_OF_TABLES];
         foreach ( var tableInfo in ( tableInfos ?? CreateDefaultTableInformation() ).Where( ti => ti != null ) )
         {
            var tKind = (Int32) tableInfo.TableIndex;
            this._infos[tKind] = tableInfo;
         }
         this._opCodeProvider = opCodeProvider ?? DefaultOpCodeProvider.DefaultInstance;
         this._sigProvider = sigProvider ?? DefaultSignatureProvider.Instance;
      }

      public IEnumerable<MetaDataTableInformation> GetAllSupportedTableInformations()
      {
         foreach ( var tableInfo in this._infos )
         {
            yield return tableInfo;
         }
      }

      public OpCodeProvider CreateOpCodeProvider()
      {
         return this._opCodeProvider;
      }

      public SignatureProvider CreateSignatureProvider()
      {
         return this._sigProvider;
      }

      public CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.ResolvingProvider CreateResolvingProvider( CILMetaData thisMD )
      {
         return new CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.DefaultResolvingProvider(
            thisMD,
            this._infos.SelectMany( i => i?.ColumnsInformationNotGeneric
               .OfType<MetaDataColumnInformationWithResolving>()
               .Where( c => c.ResolvingHandler != null )
               .Select( c => Tuple.Create( (Tables) i.TableIndex, c.ResolvingHandler ) ) ?? Empty<Tuple<Tables, CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.MetaDataColumnInformationWithResolvingCapability>>.Enumerable
               )
            );
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
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.InterfaceImplementationComparer,
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
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.ConstantDefinitionComparer,
            () => new ConstantDefinition(),
            GetConstantColumns(),
            () => new RawConstantDefinition(),
            true
            );

         yield return new MetaDataTableInformation<CustomAttributeDefinition, RawCustomAttributeDefinition>(
            Tables.CustomAttribute,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.CustomAttributeDefinitionEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.CustomAttributeDefinitionComparer,
            () => new CustomAttributeDefinition(),
            GetCustomAttributeColumns(),
            () => new RawCustomAttributeDefinition(),
            true
            );

         yield return new MetaDataTableInformation<FieldMarshal, RawFieldMarshal>(
            Tables.FieldMarshal,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.FieldMarshalEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.FieldMarshalComparer,
            () => new FieldMarshal(),
            GetFieldMarshalColumns(),
            () => new RawFieldMarshal(),
            true
            );

         yield return new MetaDataTableInformation<SecurityDefinition, RawSecurityDefinition>(
            Tables.DeclSecurity,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.SecurityDefinitionEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.SecurityDefinitionComparer,
            () => new SecurityDefinition(),
            GetDeclSecurityColumns(),
            () => new RawSecurityDefinition(),
            true
            );

         yield return new MetaDataTableInformation<ClassLayout, RawClassLayout>(
            Tables.ClassLayout,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ClassLayoutEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.ClassLayoutComparer,
            () => new ClassLayout(),
            GetClassLayoutColumns(),
            () => new RawClassLayout(),
            true
            );

         yield return new MetaDataTableInformation<FieldLayout, RawFieldLayout>(
            Tables.FieldLayout,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.FieldLayoutEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.FieldLayoutComparer,
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
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.MethodSemanticsComparer,
            () => new MethodSemantics(),
            GetMethodSemanticsColumns(),
            () => new RawMethodSemantics(),
            true
            );

         yield return new MetaDataTableInformation<MethodImplementation, RawMethodImplementation>(
            Tables.MethodImpl,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.MethodImplementationEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.MethodImplementationComparer,
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
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.MethodImplementationMapComparer,
            () => new MethodImplementationMap(),
            GetImplMapColumns(),
            () => new RawMethodImplementationMap(),
            true
            );

         yield return new MetaDataTableInformation<FieldRVA, RawFieldRVA>(
            Tables.FieldRVA,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.FieldRVAEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.FieldRVAComparer,
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
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.NestedClassDefinitionComparer,
            () => new NestedClassDefinition(),
            GetNestedClassColumns(),
            () => new RawNestedClassDefinition(),
            true
            );

         yield return new MetaDataTableInformation<GenericParameterDefinition, RawGenericParameterDefinition>(
            Tables.GenericParameter,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.GenericParameterDefinitionEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.GenericParameterDefinitionComparer,
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
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.GenericParameterConstraintDefinitionComparer,
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
         yield return MetaDataColumnInformationFactory.CodedTableIndexNullable<TypeReference, RawTypeReference>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.ResolutionScope, ( r, v ) => { r.ResolutionScope = v; return true; }, row => row.ResolutionScope, ( r, v ) => r.ResolutionScope = v );
         yield return MetaDataColumnInformationFactory.SystemString<TypeReference, RawTypeReference>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.SystemString<TypeReference, RawTypeReference>( ( r, v ) => { r.Namespace = v; return true; }, row => row.Namespace, ( r, v ) => r.Namespace = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<TypeDefinition>> GetTypeDefColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<TypeDefinition, RawTypeDefinition>( ( r, v ) => { r.Attributes = (TypeAttributes) v; return true; }, row => (Int32) row.Attributes, ( r, v ) => r.Attributes = (TypeAttributes) v );
         yield return MetaDataColumnInformationFactory.SystemString<TypeDefinition, RawTypeDefinition>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.SystemString<TypeDefinition, RawTypeDefinition>( ( r, v ) => { r.Namespace = v; return true; }, row => row.Namespace, ( r, v ) => r.Namespace = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndexNullable<TypeDefinition, RawTypeDefinition>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.TypeDefOrRef, ( r, v ) => { r.BaseType = v; return true; }, row => row.BaseType, ( r, v ) => r.BaseType = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<TypeDefinition, RawTypeDefinition>( Tables.Field, ( r, v ) => { r.FieldList = v; return true; }, row => row.FieldList, ( r, v ) => r.FieldList = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<TypeDefinition, RawTypeDefinition>( Tables.MethodDef, ( r, v ) => { r.MethodList = v; return true; }, row => row.MethodList, ( r, v ) => r.MethodList = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<FieldDefinitionPointer>> GetFieldPtrColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<FieldDefinitionPointer, RawFieldDefinitionPointer>( Tables.Field, ( r, v ) => { r.FieldIndex = v; return true; }, row => row.FieldIndex, ( r, v ) => r.FieldIndex = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<FieldDefinition>> GetFieldDefColumns()
      {
         yield return MetaDataColumnInformationFactory.Number16<FieldDefinition, RawFieldDefinition>( ( r, v ) => { r.Attributes = (FieldAttributes) v; return true; }, row => (Int16) row.Attributes, ( r, v ) => r.Attributes = (FieldAttributes) v );
         yield return MetaDataColumnInformationFactory.SystemString<FieldDefinition, RawFieldDefinition>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.BLOBNonTypeSignature<FieldDefinition, RawFieldDefinition, FieldSignature>( ( r, v ) => { r.Signature = v; return true; }, row => row.Signature, ( r, v ) => r.Signature = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MethodDefinitionPointer>> GetMethodPtrColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<MethodDefinitionPointer, RawMethodDefinitionPointer>( Tables.MethodDef, ( r, v ) => { r.MethodIndex = v; return true; }, row => row.MethodIndex, ( r, v ) => r.MethodIndex = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MethodDefinition>> GetMethodDefColumns()
      {
         yield return MetaDataColumnInformationFactory.DataReference<MethodDefinition, RawMethodDefinition, MethodILDefinition>( ( r, v ) => { r.IL = v; return true; }, r => r.IL, ( r, v ) => r.RVA = v, ( args, rva ) =>
         {
            Int64 offset;
            var rArgs = args.RowArgs;
            var mDef = args.Row;
            if ( rva != 0
               && ( offset = rArgs.RVAConverter.ToOffset( rva ) ) > 0
               && mDef.ShouldHaveMethodBody()
               )
            {
               mDef.IL = rArgs.MetaData.OpCodeProvider.DeserializeIL( rArgs.Stream.At( offset ), rArgs.Array, rArgs.MDStreamContainer.UserStrings );
            }
         }, ( md, mdStreamContainer ) => new SectionPartFunctionality_MethodIL( md, mdStreamContainer.UserStrings ), null, null );
         yield return MetaDataColumnInformationFactory.Number16<MethodDefinition, RawMethodDefinition>( ( r, v ) => { r.ImplementationAttributes = (MethodImplAttributes) v; return true; }, row => (Int16) row.ImplementationAttributes, ( r, v ) => r.ImplementationAttributes = (MethodImplAttributes) v );
         yield return MetaDataColumnInformationFactory.Number16<MethodDefinition, RawMethodDefinition>( ( r, v ) => { r.Attributes = (MethodAttributes) v; return true; }, row => (Int16) row.Attributes, ( r, v ) => r.Attributes = (MethodAttributes) v );
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
         yield return MetaDataColumnInformationFactory.Number16<ParameterDefinition, RawParameterDefinition>( ( r, v ) => { r.Attributes = (ParameterAttributes) v; return true; }, row => (Int16) row.Attributes, ( r, v ) => r.Attributes = (ParameterAttributes) v );
         yield return MetaDataColumnInformationFactory.Number16<ParameterDefinition, RawParameterDefinition>( ( r, v ) => { r.Sequence = (UInt16) v; return true; }, row => (Int16) row.Sequence, ( r, v ) => r.Sequence = v );
         yield return MetaDataColumnInformationFactory.SystemString<ParameterDefinition, RawParameterDefinition>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<InterfaceImplementation>> GetInterfaceImplColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<InterfaceImplementation, RawInterfaceImplementation>( Tables.TypeDef, ( r, v ) => { r.Class = v; return true; }, row => row.Class, ( r, v ) => r.Class = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<InterfaceImplementation, RawInterfaceImplementation>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.TypeDefOrRef, ( r, v ) => { r.Interface = v; return true; }, row => row.Interface, ( r, v ) => r.Interface = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MemberReference>> GetMemberRefColumns()
      {
         yield return MetaDataColumnInformationFactory.CodedTableIndex<MemberReference, RawMemberReference>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.MemberRefParent, ( r, v ) => { r.DeclaringType = v; return true; }, row => row.DeclaringType, ( r, v ) => r.DeclaringType = v );
         yield return MetaDataColumnInformationFactory.SystemString<MemberReference, RawMemberReference>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.BLOBNonTypeSignature<MemberReference, RawMemberReference, AbstractSignature>( ( r, v ) => { r.Signature = v; return true; }, row => row.Signature, ( r, v ) => r.Signature = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<ConstantDefinition>> GetConstantColumns()
      {
         yield return MetaDataColumnInformationFactory.Number8<ConstantDefinition, RawConstantDefinition>( ( r, v ) => { r.Type = (ConstantValueType) v; return true; }, row => (Byte) row.Type, ( r, v ) => r.Type = (ConstantValueType) v );
         yield return MetaDataColumnInformationFactory.Number8<ConstantDefinition, RawConstantDefinition>( ( r, v ) => { return true; }, row => 0, ( r, v ) => r.Padding = (Byte) v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<ConstantDefinition, RawConstantDefinition>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.HasConstant, ( r, v ) => { r.Parent = v; return true; }, row => row.Parent, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformationFactory.BLOBCustom<ConstantDefinition, RawConstantDefinition, Object>( ( r, v ) => { r.Value = v; return true; }, r => r.Value, ( r, v ) => r.Value = v, ( args, v, blobs ) => args.Row.Value = blobs.ReadConstantValue( v, args.RowArgs.MetaData.SignatureProvider, args.Row.Type ), args => args.RowArgs.Array.CreateConstantBytes( args.Row.Value, args.Row.Type ), null, null );
      }

      protected static IEnumerable<MetaDataColumnInformation<CustomAttributeDefinition>> GetCustomAttributeColumns()
      {
         yield return MetaDataColumnInformationFactory.CodedTableIndex<CustomAttributeDefinition, RawCustomAttributeDefinition>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.HasCustomAttribute, ( r, v ) => { r.Parent = v; return true; }, row => row.Parent, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<CustomAttributeDefinition, RawCustomAttributeDefinition>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.CustomAttributeType, ( r, v ) => { r.Type = v; return true; }, row => row.Type, ( r, v ) => r.Type = v );
         yield return MetaDataColumnInformationFactory.BLOBCustom<CustomAttributeDefinition, RawCustomAttributeDefinition, AbstractCustomAttributeSignature>( ( r, v ) => { r.Signature = v; return true; }, r => r.Signature, ( r, v ) => r.Signature = v, ( args, v, blobs ) => args.Row.Signature = blobs.ReadCASignature( v, args.RowArgs.MetaData.SignatureProvider ), args => args.RowArgs.Array.CreateCustomAttributeSignature( args.RowArgs.MetaData, args.RowIndex ), CreateCAColumnSpecificCache, ResolveCASignature );
      }

      protected static IEnumerable<MetaDataColumnInformation<FieldMarshal>> GetFieldMarshalColumns()
      {
         yield return MetaDataColumnInformationFactory.CodedTableIndex<FieldMarshal, RawFieldMarshal>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.HasFieldMarshal, ( r, v ) => { r.Parent = v; return true; }, row => row.Parent, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformationFactory.BLOBCustom<FieldMarshal, RawFieldMarshal, AbstractMarshalingInfo>( ( r, v ) => { r.NativeType = v; return true; }, row => row.NativeType, ( r, v ) => r.NativeType = v, ( args, v, blobs ) => args.Row.NativeType = blobs.ReadMarshalingInfo( v, args.RowArgs.MetaData.SignatureProvider ), args => args.RowArgs.Array.CreateMarshalSpec( args.Row.NativeType ), null, null );
      }

      protected static IEnumerable<MetaDataColumnInformation<SecurityDefinition>> GetDeclSecurityColumns()
      {
         yield return MetaDataColumnInformationFactory.Number16<SecurityDefinition, RawSecurityDefinition>( ( r, v ) => { r.Action = (SecurityAction) v; return true; }, row => (Int16) row.Action, ( r, v ) => r.Action = (SecurityAction) v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<SecurityDefinition, RawSecurityDefinition>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.HasSecurity, ( r, v ) => { r.Parent = v; return true; }, row => row.Parent, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformationFactory.BLOBCustom<SecurityDefinition, RawSecurityDefinition, List<AbstractSecurityInformation>>( ( r, v ) => { r.PermissionSets.Clear(); r.PermissionSets.AddRange( v ); return true; }, row => row.PermissionSets, ( r, v ) => r.PermissionSets = v, ( args, v, blobs ) => blobs.ReadSecurityInformation( v, args.RowArgs.MetaData.SignatureProvider, args.Row.PermissionSets ), args => args.RowArgs.Array.CreateSecuritySignature( args.Row.PermissionSets, args.RowArgs.AuxArray, args.RowArgs.MetaData.SignatureProvider ), CreateCAColumnSpecificCache, ResolveSecurityPermissionSets );
      }

      protected static IEnumerable<MetaDataColumnInformation<ClassLayout>> GetClassLayoutColumns()
      {
         yield return MetaDataColumnInformationFactory.Number16<ClassLayout, RawClassLayout>( ( r, v ) => { r.PackingSize = (UInt16) v; return true; }, row => (Int16) row.PackingSize, ( r, v ) => r.PackingSize = v );
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
            args.Row.Signature = blobs.ReadNonTypeSignature( v, args.RowArgs.MetaData.SignatureProvider, false, true, out wasFieldSig );
            args.Row.StoreSignatureAsFieldSignature = wasFieldSig;
         }, args => args.RowArgs.Array.CreateStandaloneSignature( args.Row ), null, null );
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
         yield return MetaDataColumnInformationFactory.Number16<EventDefinition, RawEventDefinition>( ( r, v ) => { r.Attributes = (EventAttributes) v; return true; }, row => (Int16) row.Attributes, ( r, v ) => r.Attributes = (EventAttributes) v );
         yield return MetaDataColumnInformationFactory.SystemString<EventDefinition, RawEventDefinition>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<EventDefinition, RawEventDefinition>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.TypeDefOrRef, ( r, v ) => { r.EventType = v; return true; }, row => row.EventType, ( r, v ) => r.EventType = v );
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
         yield return MetaDataColumnInformationFactory.Number16<PropertyDefinition, RawPropertyDefinition>( ( r, v ) => { r.Attributes = (PropertyAttributes) v; return true; }, row => (Int16) row.Attributes, ( r, v ) => r.Attributes = (PropertyAttributes) v );
         yield return MetaDataColumnInformationFactory.SystemString<PropertyDefinition, RawPropertyDefinition>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.BLOBNonTypeSignature<PropertyDefinition, RawPropertyDefinition, PropertySignature>( ( r, v ) => { r.Signature = v; return true; }, row => row.Signature, ( r, v ) => r.Signature = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MethodSemantics>> GetMethodSemanticsColumns()
      {
         yield return MetaDataColumnInformationFactory.Number16<MethodSemantics, RawMethodSemantics>( ( r, v ) => { r.Attributes = (MethodSemanticsAttributes) v; return true; }, row => (Int16) row.Attributes, ( r, v ) => r.Attributes = (MethodSemanticsAttributes) v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<MethodSemantics, RawMethodSemantics>( Tables.MethodDef, ( r, v ) => { r.Method = v; return true; }, row => row.Method, ( r, v ) => r.Method = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<MethodSemantics, RawMethodSemantics>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.HasSemantics, ( r, v ) => { r.Associaton = v; return true; }, row => row.Associaton, ( r, v ) => r.Associaton = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MethodImplementation>> GetMethodImplColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<MethodImplementation, RawMethodImplementation>( Tables.TypeDef, ( r, v ) => { r.Class = v; return true; }, row => row.Class, ( r, v ) => r.Class = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<MethodImplementation, RawMethodImplementation>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.MethodDefOrRef, ( r, v ) => { r.MethodBody = v; return true; }, row => row.MethodBody, ( r, v ) => r.MethodBody = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<MethodImplementation, RawMethodImplementation>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.MethodDefOrRef, ( r, v ) => { r.MethodDeclaration = v; return true; }, row => row.MethodDeclaration, ( r, v ) => r.MethodDeclaration = v );
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
         yield return MetaDataColumnInformationFactory.Number16<MethodImplementationMap, RawMethodImplementationMap>( ( r, v ) => { r.Attributes = (PInvokeAttributes) v; return true; }, row => (Int16) row.Attributes, ( r, v ) => r.Attributes = (PInvokeAttributes) v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<MethodImplementationMap, RawMethodImplementationMap>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.MemberForwarded, ( r, v ) => { r.MemberForwarded = v; return true; }, row => row.MemberForwarded, ( r, v ) => r.MemberForwarded = v );
         yield return MetaDataColumnInformationFactory.SystemString<MethodImplementationMap, RawMethodImplementationMap>( ( r, v ) => { r.ImportName = v; return true; }, row => row.ImportName, ( r, v ) => r.ImportName = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<MethodImplementationMap, RawMethodImplementationMap>( Tables.ModuleRef, ( r, v ) => { r.ImportScope = v; return true; }, row => row.ImportScope, ( r, v ) => r.ImportScope = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<FieldRVA>> GetFieldRVAColumns()
      {
         yield return MetaDataColumnInformationFactory.DataReference<FieldRVA, RawFieldRVA, Byte[]>( ( r, v ) => { r.Data = v; return true; }, r => r.Data, ( r, v ) => r.RVA = v, ( args, rva ) =>
         {
            var rArgs = args.RowArgs;
            var md = rArgs.MetaData;
            var fRVA = args.Row;
            var stream = rArgs.Stream;
            Int64 offset;
            Int32 size;
            if ( rva > 0
               && ( offset = rArgs.RVAConverter.ToOffset( rva ) ) > 0
               && md.TryCalculateFieldTypeSize( rArgs.LayoutInfo, fRVA.Field.Index, out size )
               && stream.At( offset ).Stream.CanReadNextBytes( size ).IsTrue() // Sometimes there are RVAs which are unresolvable...
               )
            {
               // Read all field RVA content
               args.Row.Data = stream.ReadAndCreateArray( size );
            }
         }, ( md, mdStreamContainer ) => new SectionPart_FieldRVA( md ), null, null );
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
         yield return MetaDataColumnInformationFactory.Number32<AssemblyDefinition, RawAssemblyDefinition>( ( r, v ) => { r.HashAlgorithm = (AssemblyHashAlgorithm) v; return true; }, row => (Int32) row.HashAlgorithm, ( r, v ) => r.HashAlgorithm = (AssemblyHashAlgorithm) v );
         yield return MetaDataColumnInformationFactory.Number16<AssemblyDefinition, RawAssemblyDefinition>( ( r, v ) => { r.AssemblyInformation.VersionMajor = (UInt16) v; return true; }, row => (Int16) row.AssemblyInformation.VersionMajor, ( r, v ) => r.MajorVersion = v );
         yield return MetaDataColumnInformationFactory.Number16<AssemblyDefinition, RawAssemblyDefinition>( ( r, v ) => { r.AssemblyInformation.VersionMinor = (UInt16) v; return true; }, row => (Int16) row.AssemblyInformation.VersionMinor, ( r, v ) => r.MinorVersion = v );
         yield return MetaDataColumnInformationFactory.Number16<AssemblyDefinition, RawAssemblyDefinition>( ( r, v ) => { r.AssemblyInformation.VersionBuild = (UInt16) v; return true; }, row => (Int16) row.AssemblyInformation.VersionBuild, ( r, v ) => r.BuildNumber = v );
         yield return MetaDataColumnInformationFactory.Number16<AssemblyDefinition, RawAssemblyDefinition>( ( r, v ) => { r.AssemblyInformation.VersionRevision = (UInt16) v; return true; }, row => (Int16) row.AssemblyInformation.VersionRevision, ( r, v ) => r.RevisionNumber = v );
         yield return MetaDataColumnInformationFactory.Number32<AssemblyDefinition, RawAssemblyDefinition>( ( r, v ) => { r.Attributes = (AssemblyFlags) v; return true; }, row => (Int32) row.Attributes, ( r, v ) => r.Attributes = (AssemblyFlags) v );
         yield return MetaDataColumnInformationFactory.BLOBCustom<AssemblyDefinition, RawAssemblyDefinition, Byte[]>( ( r, v ) => { r.AssemblyInformation.PublicKeyOrToken = v; return true; }, r => r.AssemblyInformation.PublicKeyOrToken, ( r, v ) => r.PublicKey = v, ( args, v, blobs ) => args.Row.AssemblyInformation.PublicKeyOrToken = blobs.GetBLOBByteArray( v ), args => args.RowArgs.PublicKey?.ToArray(), null, null );
         //{
         //   var pk = args.Row.AssemblyInformation.PublicKeyOrToken;
         //   return pk.IsNullOrEmpty() ?  : pk;
         //} );
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
         yield return MetaDataColumnInformationFactory.Number16<AssemblyReference, RawAssemblyReference>( ( r, v ) => { r.AssemblyInformation.VersionMajor = (UInt16) v; return true; }, row => (Int16) row.AssemblyInformation.VersionMajor, ( r, v ) => r.MajorVersion = v );
         yield return MetaDataColumnInformationFactory.Number16<AssemblyReference, RawAssemblyReference>( ( r, v ) => { r.AssemblyInformation.VersionMinor = (UInt16) v; return true; }, row => (Int16) row.AssemblyInformation.VersionMinor, ( r, v ) => r.MinorVersion = v );
         yield return MetaDataColumnInformationFactory.Number16<AssemblyReference, RawAssemblyReference>( ( r, v ) => { r.AssemblyInformation.VersionBuild = (UInt16) v; return true; }, row => (Int16) row.AssemblyInformation.VersionBuild, ( r, v ) => r.BuildNumber = v );
         yield return MetaDataColumnInformationFactory.Number16<AssemblyReference, RawAssemblyReference>( ( r, v ) => { r.AssemblyInformation.VersionRevision = (UInt16) v; return true; }, row => (Int16) row.AssemblyInformation.VersionRevision, ( r, v ) => r.RevisionNumber = v );
         yield return MetaDataColumnInformationFactory.Number32<AssemblyReference, RawAssemblyReference>( ( r, v ) => { r.Attributes = (AssemblyFlags) v; return true; }, row => (Int32) row.Attributes, ( r, v ) => r.Attributes = (AssemblyFlags) v );
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
         yield return MetaDataColumnInformationFactory.Number32<FileReference, RawFileReference>( ( r, v ) => { r.Attributes = (FileAttributes) v; return true; }, row => (Int32) row.Attributes, ( r, v ) => r.Attributes = (FileAttributes) v );
         yield return MetaDataColumnInformationFactory.SystemString<FileReference, RawFileReference>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.BLOBByteArray<FileReference, RawFileReference>( ( r, v ) => { r.HashValue = v; return true; }, row => row.HashValue, ( r, v ) => r.HashValue = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<ExportedType>> GetExportedTypeColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<ExportedType, RawExportedType>( ( r, v ) => { r.Attributes = (TypeAttributes) v; return true; }, row => (Int32) row.Attributes, ( r, v ) => r.Attributes = (TypeAttributes) v );
         yield return MetaDataColumnInformationFactory.Number32<ExportedType, RawExportedType>( ( r, v ) => { r.TypeDefinitionIndex = v; return true; }, row => row.TypeDefinitionIndex, ( r, v ) => r.TypeDefinitionIndex = v );
         yield return MetaDataColumnInformationFactory.SystemString<ExportedType, RawExportedType>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.SystemString<ExportedType, RawExportedType>( ( r, v ) => { r.Namespace = v; return true; }, row => row.Namespace, ( r, v ) => r.Namespace = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<ExportedType, RawExportedType>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.Implementation, ( r, v ) => { r.Implementation = v; return true; }, row => row.Implementation, ( r, v ) => r.Implementation = v );
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
               var rArgs = args.RowArgs;
               var stream = rArgs.Stream;
               var rsrcDD = rArgs.ImageInformation.CLIInformation.CLIHeader.Resources;
               Int64 ddOffset;
               if ( rsrcDD.RVA > 0
                  && ( ddOffset = rArgs.RVAConverter.ToOffset( rsrcDD.RVA ) ) > 0
                  && ( stream = stream.At( ddOffset ) ).Stream.CanReadNextBytes( offset + sizeof( Int32 ) ).IsTrue()
                  )
               {
                  stream = stream.At( ddOffset + offset );
                  var size = stream.ReadInt32LEFromBytes();
                  if ( stream.Stream.CanReadNextBytes( size ).IsTrue() )
                  {
                     row.EmbeddedData = stream.ReadAndCreateArray( size );
                  }
               }
            }
         },
         ( md, mdStreamContainer ) => new SectionPart_EmbeddedManifests( md ), null, null );
         yield return MetaDataColumnInformationFactory.Number32<ManifestResource, RawManifestResource>( ( r, v ) => { r.Attributes = (ManifestResourceAttributes) v; return true; }, row => (Int32) row.Attributes, ( r, v ) => r.Attributes = (ManifestResourceAttributes) v );
         yield return MetaDataColumnInformationFactory.SystemString<ManifestResource, RawManifestResource>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndexNullable<ManifestResource, RawManifestResource>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.Implementation, ( r, v ) => { r.Implementation = v; return true; }, row => row.Implementation, ( r, v ) => r.Implementation = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<NestedClassDefinition>> GetNestedClassColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<NestedClassDefinition, RawNestedClassDefinition>( Tables.TypeDef, ( r, v ) => { r.NestedClass = v; return true; }, row => row.NestedClass, ( r, v ) => r.NestedClass = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<NestedClassDefinition, RawNestedClassDefinition>( Tables.TypeDef, ( r, v ) => { r.EnclosingClass = v; return true; }, row => row.EnclosingClass, ( r, v ) => r.EnclosingClass = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<GenericParameterDefinition>> GetGenericParamColumns()
      {
         yield return MetaDataColumnInformationFactory.Number16<GenericParameterDefinition, RawGenericParameterDefinition>( ( r, v ) => { r.GenericParameterIndex = (UInt16) v; return true; }, row => (Int16) row.GenericParameterIndex, ( r, v ) => r.GenericParameterIndex = v );
         yield return MetaDataColumnInformationFactory.Number16<GenericParameterDefinition, RawGenericParameterDefinition>( ( r, v ) => { r.Attributes = (GenericParameterAttributes) v; return true; }, row => (Int16) row.Attributes, ( r, v ) => r.Attributes = (GenericParameterAttributes) v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<GenericParameterDefinition, RawGenericParameterDefinition>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.TypeOrMethodDef, ( r, v ) => { r.Owner = v; return true; }, row => row.Owner, ( r, v ) => r.Owner = v );
         yield return MetaDataColumnInformationFactory.SystemString<GenericParameterDefinition, RawGenericParameterDefinition>( ( r, v ) => { r.Name = v; return true; }, row => row.Name, ( r, v ) => r.Name = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MethodSpecification>> GetMethodSpecColumns()
      {
         yield return MetaDataColumnInformationFactory.CodedTableIndex<MethodSpecification, RawMethodSpecification>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.MethodDefOrRef, ( r, v ) => { r.Method = v; return true; }, row => row.Method, ( r, v ) => r.Method = v );
         yield return MetaDataColumnInformationFactory.BLOBNonTypeSignature<MethodSpecification, RawMethodSpecification, GenericMethodSignature>( ( r, v ) => { r.Signature = v; return true; }, row => row.Signature, ( r, v ) => r.Signature = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<GenericParameterConstraintDefinition>> GetGenericParamConstraintColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<GenericParameterConstraintDefinition, RawGenericParameterConstraintDefinition>( Tables.GenericParameter, ( r, v ) => { r.Owner = v; return true; }, row => row.Owner, ( r, v ) => r.Owner = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<GenericParameterConstraintDefinition, RawGenericParameterConstraintDefinition>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.TypeDefOrRef, ( r, v ) => { r.Constraint = v; return true; }, row => row.Constraint, ( r, v ) => r.Constraint = v );
      }


      private static Object CreateCAColumnSpecificCache()
      {
         return new Dictionary<CILMetaData, IDictionary<Int32, CustomAttributeArgumentTypeSimple>>();
      }

      private static Boolean ResolveCASignature(
         CILMetaData md,
         Int32 rowIndex,
         MetaDataResolver resolver,
         Object cache
         )
      {
         var row = md?.CustomAttributeDefinitions?.GetOrNull( rowIndex );
         var sig = row?.Signature as RawCustomAttributeSignature;
         var retVal = sig != null;
         if ( retVal )
         {
            var resolved = TryResolveCustomAttributeSignature( md, sig.Bytes, 0, row.Type, resolver, (IDictionary<CILMetaData, IDictionary<Int32, CustomAttributeArgumentTypeSimple>>) cache );
            retVal = resolved != null;
            if ( retVal )
            {
               row.Signature = resolved;
            }
         }
         return retVal;
      }

      private static Boolean ResolveSecurityPermissionSets(
         CILMetaData md,
         Int32 rowIndex,
         MetaDataResolver resolver,
         Object cache
         )
      {
         var sec = md?.SecurityDefinitions?.GetOrNull( rowIndex );
         var retVal = sec != null;
         if ( retVal )
         {
            var permissions = sec.PermissionSets;
            var actualCache = (IDictionary<CILMetaData, IDictionary<Int32, CustomAttributeArgumentTypeSimple>>) cache;
            for ( var i = 0; i < permissions.Count; ++i )
            {
               var permission = permissions[i] as RawSecurityInformation;
               if ( permission != null )
               {
                  var idx = 0;
                  var bytes = permission.Bytes;
                  var argCount = permission.ArgumentCount;
                  var secInfo = new SecurityInformation( argCount ) { SecurityAttributeType = permission.SecurityAttributeType };
                  var success = true;
                  for ( var j = 0; j < argCount && success; ++j )
                  {
                     var arg = md.SignatureProvider.ReadCANamedArgument( bytes, ref idx, typeStr => ResolveTypeFromFullName( md, typeStr, resolver, actualCache ) );
                     if ( arg == null )
                     {
                        success = false;
                     }
                     else
                     {
                        secInfo.NamedArguments.Add( arg );
                     }
                  }

                  if ( success )
                  {
                     permissions[i] = secInfo;
                  }
                  else
                  {
                     retVal = false;
                  }
               }
            }
         }

         return retVal;
      }

      private static ResolvedCustomAttributeSignature TryResolveCustomAttributeSignature(
         CILMetaData md,
         Byte[] blob,
         Int32 idx,
         TableIndex caTypeTableIndex,
         MetaDataResolver resolver,
         IDictionary<CILMetaData, IDictionary<Int32, CustomAttributeArgumentTypeSimple>> cache
         )
      {

         AbstractMethodSignature ctorSig;
         switch ( caTypeTableIndex.Table )
         {
            case Tables.MethodDef:
               ctorSig = caTypeTableIndex.Index < md.MethodDefinitions.GetRowCount() ?
                  md.MethodDefinitions.TableContents[caTypeTableIndex.Index].Signature :
                  null;
               break;
            case Tables.MemberRef:
               ctorSig = caTypeTableIndex.Index < md.MemberReferences.GetRowCount() ?
                  md.MemberReferences.TableContents[caTypeTableIndex.Index].Signature as AbstractMethodSignature :
                  null;
               break;
            default:
               ctorSig = null;
               break;
         }

         var success = ctorSig != null;
         ResolvedCustomAttributeSignature retVal = null;
         if ( success )
         {
            var startIdx = idx;
            retVal = new ResolvedCustomAttributeSignature( typedArgsCount: ctorSig.Parameters.Count );

            idx += 2; // Skip prolog

            for ( var i = 0; i < ctorSig.Parameters.Count; ++i )
            {
               var caType = md.TypeSignatureToCustomAttributeArgumentType( ctorSig.Parameters[i].Type, tIdx => ResolveCATypeFromTableIndex( md, tIdx, resolver ) );
               if ( caType == null )
               {
                  // We don't know the size of the type -> stop
                  retVal.TypedArguments.Clear();
                  break;
               }
               else
               {
                  retVal.TypedArguments.Add( md.SignatureProvider.ReadCAFixedArgument( blob, ref idx, caType, typeStr => ResolveTypeFromFullName( md, typeStr, resolver, cache ) ) );
               }
            }

            // Check if we had failed to resolve ctor type before.
            success = retVal.TypedArguments.Count == ctorSig.Parameters.Count;
            if ( success )
            {
               var namedCount = blob.ReadUInt16LEFromBytes( ref idx );
               for ( var i = 0; i < namedCount && success; ++i )
               {
                  var caNamedArg = md.SignatureProvider.ReadCANamedArgument( blob, ref idx, typeStr => ResolveTypeFromFullName( md, typeStr, resolver, cache ) );

                  if ( caNamedArg == null )
                  {
                     // We don't know the size of the type -> stop
                     success = false;
                  }
                  else
                  {
                     retVal.NamedArguments.Add( caNamedArg );
                  }
               }
            }
         }
         return success ? retVal : null;
      }

      private static CustomAttributeArgumentTypeEnum ResolveCATypeFromTableIndex(
         CILMetaData md,
         TableIndex tIdx,
         MetaDataResolver resolver
         )
      {
         var typeName = resolver.ResolveTypeNameFromTypeDefOrRefOrSpec( md, tIdx );

         return typeName == null ? null : new CustomAttributeArgumentTypeEnum()
         {
            TypeString = typeName
         };
      }

      private static CustomAttributeArgumentTypeSimple ResolveTypeFromFullName(
         CILMetaData md,
         String typeString,
         MetaDataResolver resolver,
         IDictionary<CILMetaData, IDictionary<Int32, CustomAttributeArgumentTypeSimple>> cache
         )
      {
         CILMetaData otherMD; Int32 typeDefIndex;
         return resolver.TryResolveTypeString( md, typeString, out otherMD, out typeDefIndex ) ?
            cache
               .GetOrAdd_NotThreadSafe( otherMD, omd => new Dictionary<Int32, CustomAttributeArgumentTypeSimple>() )
               .GetOrAdd_NotThreadSafe( typeDefIndex, tDefIdx => ResolveTypeFromTypeDef( otherMD, tDefIdx ) ) :
            null;
      }

      private static CustomAttributeArgumentTypeSimple ResolveTypeFromTypeDef(
         CILMetaData md,
         Int32 index
         )
      {
         CustomAttributeArgumentTypeSimple retVal = null;
         if ( index >= 0 )
         {
            Int32 enumFieldIndex;
            if ( md.TryGetEnumValueFieldIndex( index, out enumFieldIndex ) )
            {
               var sig = md.FieldDefinitions.TableContents[enumFieldIndex].Signature.Type;
               if ( sig != null && sig.TypeSignatureKind == TypeSignatureKind.Simple )
               {
                  retVal = md.SignatureProvider.GetSimpleCATypeOrNull( (CustomAttributeArgumentTypeSimpleKind) ( (SimpleTypeSignature) sig ).SimpleType );
               }
            }
         }

         return retVal;
      }

   }

   public interface MetaDataTableInformationWithSerializationCapability
   {
      TableSerializationLogicalFunctionality CreateTableSerializationInfoNotGeneric( TableSerializationLogicalFunctionalityCreationArgs args );
   }

   public struct TableSerializationLogicalFunctionalityCreationArgs
   {
      public TableSerializationLogicalFunctionalityCreationArgs( EventHandler<SerializationErrorEventArgs> errorHandler )
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

      public TableSerializationLogicalFunctionalityImpl<TRow, TRawRow> CreateTableSerializationInfo( TableSerializationLogicalFunctionalityCreationArgs args )
      {
         return new TableSerializationLogicalFunctionalityImpl<TRow, TRawRow>(
            (Tables) this.TableIndex,
            this.IsSortedForSerialization,
            this.ColumnsInformation.Select( c => ( c as MetaDataColumnInformationWithSerializationCapability<TRow, TRawRow> )?.DefaultColumnSerializationInfoWithRawType ),
            this.RowFactory,
            this._rawRowFactory,
            args
            );
      }

      public Boolean IsSortedForSerialization { get; }

      public TableSerializationLogicalFunctionality CreateTableSerializationInfoNotGeneric( TableSerializationLogicalFunctionalityCreationArgs args )
      {
         return this.CreateTableSerializationInfo( args );
      }
   }

   public static class MetaDataColumnInformationFactory
   {

      //public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue> Constant16<TRow, TRawRow, TValue>(
      //   RowColumnSetterDelegate<TRow, TValue> setter,
      //   RowColumnGetterDelegate<TRow, TValue> getter,
      //   RawRowColumnSetterDelegate<TRawRow> rawSetter,
      //   Func<Int32, TValue> fromInteger,
      //   Func<TValue, Int32> toInteger
      //   )
      //   where TRow : class
      //   where TRawRow : class
      //   where TValue : struct
      //{
      //   return ConstantCustom<TRow, TRawRow, TValue>(
      //      sizeof( Int16 ),
      //      getter,
      //      setter,
      //      () => DefaultColumnSerializationInfoFactory.Constant16<TRow, TRawRow>( rawSetter, ( args, v ) => setter( args.Row, fromInteger( v ) ), r => toInteger( getter( r ) ) )
      //      );
      //}

      //public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue> Constant32<TRow, TRawRow, TValue>(
      //   RowColumnSetterDelegate<TRow, TValue> setter,
      //   RowColumnGetterDelegate<TRow, TValue> getter,
      //   RawRowColumnSetterDelegate<TRawRow> rawSetter,
      //   Func<Int32, TValue> fromInteger,
      //   Func<TValue, Int32> toInteger
      //   )
      //   where TRow : class
      //   where TRawRow : class
      //   where TValue : struct
      //{
      //   return ConstantCustom<TRow, TRawRow, TValue>(
      //      sizeof( Int32 ),
      //      getter,
      //      setter,
      //      () => DefaultColumnSerializationInfoFactory.Constant32<TRow, TRawRow>( rawSetter, ( args, v ) => setter( args.Row, fromInteger( v ) ), r => toInteger( getter( r ) ) )
      //      );
      //}

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, Byte> Number8<TRow, TRawRow>(
         RowColumnSetterDelegate<TRow, Byte> setter,
         RowColumnGetterDelegate<TRow, Byte> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return ConstantCustom(
            sizeof( Byte ),
            getter,
            setter,
            () => DefaultColumnSerializationInfoFactory.Constant8( rawSetter, setter, getter )
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
         return ConstantCustom(
            sizeof( Int16 ),
            getter,
            setter,
            () => DefaultColumnSerializationInfoFactory.Constant16( rawSetter, setter, getter )
            );
      }

      //public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, Int32> Number32_SerializedAs16<TRow, TRawRow>(
      //   RowColumnSetterDelegate<TRow, Int32> setter,
      //   RowColumnGetterDelegate<TRow, Int32> getter,
      //   RawRowColumnSetterDelegate<TRawRow> rawSetter
      //   )
      //   where TRow : class
      //   where TRawRow : class
      //{
      //   return ConstantCustom<TRow, TRawRow, Int32>(
      //      sizeof( Int16 ),
      //      getter,
      //      setter,
      //      () => DefaultColumnSerializationInfoFactory.Constant16<TRow, TRawRow>( rawSetter, ( row, v ) => setter( row, v ), r => getter( r ) )
      //      );
      //}

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, Int32> Number32<TRow, TRawRow>(
         RowColumnSetterDelegate<TRow, Int32> setter,
         RowColumnGetterDelegate<TRow, Int32> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return ConstantCustom(
            sizeof( Int32 ),
            getter,
            setter,
            () => DefaultColumnSerializationInfoFactory.Constant32( rawSetter, setter, getter )
            );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue> ConstantCustom<TRow, TRawRow, TValue>(
         Int32 byteCount,
         RowColumnGetterDelegate<TRow, TValue> getter,
         RowColumnSetterDelegate<TRow, TValue> setter,
         Func<DefaultColumnSerializationInfo<TRow, TRawRow>> serializationInfo
         )
         where TRow : class
         where TRawRow : class
         where TValue : struct
      {
         return new MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue>( getter, setter, new MetaDataColumnDataInformation_FixedSizeConstant( byteCount ), null, null, serializationInfo );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, String> SystemString<TRow, TRawRow>(
         RowColumnSetterDelegate<TRow, String> setter,
         RowColumnGetterDelegate<TRow, String> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return new MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, String>( getter, setter, new MetaDataColumnDataInformation_HeapIndex( MetaDataConstants.SYS_STRING_STREAM_NAME ), null, null, () => DefaultColumnSerializationInfoFactory.SystemString<TRow, TRawRow>( rawSetter, setter, getter ) );
      }

      public static MetaDataColumnInformationForNullables<TRow, TRawRow, Guid> GUID<TRow, TRawRow>(
         RowColumnSetterDelegate<TRow, Guid?> setter,
         RowColumnGetterDelegate<TRow, Guid?> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return new MetaDataColumnInformationForNullables<TRow, TRawRow, Guid>( getter, setter, new MetaDataColumnDataInformation_HeapIndex( MetaDataConstants.GUID_STREAM_NAME ), null, null, () => DefaultColumnSerializationInfoFactory.GUID<TRow, TRawRow>( rawSetter, setter, getter ) );
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
         return new MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TableIndex>( getter, setter, new MetaDataColumnDataInformation_SimpleTableIndex( (Int32) targetTable ), null, null, () => DefaultColumnSerializationInfoFactory.SimpleReference<TRow, TRawRow>( targetTable, rawSetter, setter, getter ) );
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
         return new MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TableIndex>( getter, setter, new MetaDataColumnDataInformation_CodedTableIndex( targetTables ), null, null, () => DefaultColumnSerializationInfoFactory.CodedReference<TRow, TRawRow>( targetTables, rawSetter, ( row, v ) => setter( row, v.GetValueOrDefault() ), r => getter( r ) ) );
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
         return new MetaDataColumnInformationForNullables<TRow, TRawRow, TableIndex>( getter, setter, new MetaDataColumnDataInformation_CodedTableIndex( targetTables ), null, null, () => DefaultColumnSerializationInfoFactory.CodedReference<TRow, TRawRow>( targetTables, rawSetter, setter, getter ) );
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
            ( args, value, blobs ) => setter( args.Row, blobs.ReadTypeSignature( value, args.RowArgs.MetaData.SignatureProvider ) ),
            args => args.RowArgs.Array.CreateAnySignature( getter( args.Row ) ),
            null,
            null
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
               setter( args.Row, blobs.ReadNonTypeSignature( value, args.RowArgs.MetaData.SignatureProvider, isMethodDef, false, out wasFieldSig ) as TSignature );
            },
            args => args.RowArgs.Array.CreateAnySignature( getter( args.Row ) ),
            null,
            null
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
            ( args, heapIndex, blobs ) => setter( args.Row, blobs.GetBLOBByteArray( heapIndex ) ),
            args => getter( args.Row ),
            null,
            null
            );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue> BLOBCustom<TRow, TRawRow, TValue>(
         RowColumnSetterDelegate<TRow, TValue> setter,
         RowColumnGetterDelegate<TRow, TValue> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         Action<ColumnValueArgs<TRow, RowReadingArguments>, Int32, ReaderBLOBStreamHandler> blobReader, // TODO delegat-ize these
         Func<ColumnValueArgs<TRow, RowHeapFillingArguments>, Byte[]> blobCreator,
         ResolverCacheCreatorDelegate resolverCacheCreator,
         ResolverDelegate resolver
         )
         where TRow : class
         where TRawRow : class
      {
         return new MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue>( getter, setter, new MetaDataColumnDataInformation_HeapIndex( MetaDataConstants.BLOB_STREAM_NAME ), resolverCacheCreator, resolver, () => DefaultColumnSerializationInfoFactory.BLOB<TRow, TRawRow>( rawSetter, blobReader, blobCreator ) );
      }

      public static MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue> DataReference<TRow, TRawRow, TValue>(
         RowColumnSetterDelegate<TRow, TValue> setter,
         RowColumnGetterDelegate<TRow, TValue> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnDataReferenceSetterDelegate<TRow> rawValueProcessor,
         DataReferenceColumnSectionPartCreationDelegate<TRow> rawColummnSectionPartCreator,
         ResolverCacheCreatorDelegate resolverCacheCreator,
         ResolverDelegate resolver
         )
         where TRow : class
         where TRawRow : class
      {
         return new MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue>( getter, setter, new MetaDataColumnDataInformation_DataReference(), resolverCacheCreator, resolver, () => DefaultColumnSerializationInfoFactory.DataReferenceColumn<TRow, TRawRow>( rawSetter, rawValueProcessor, rawColummnSectionPartCreator ) );
      }


   }

   //public interface MetaDataColumnInformationWithSerializationCapability
   //{
   //   DefaultColumnSerializationInfo DefaultColumnSerializationInfoNotGeneric { get; }
   //}

   public interface MetaDataColumnInformationWithSerializationCapability<TRow, TRawRow> // : MetaDataColumnInformationWithSerializationCapability
      where TRow : class
      where TRawRow : class
   {
      DefaultColumnSerializationInfo<TRow, TRawRow> DefaultColumnSerializationInfoWithRawType { get; }
   }

   // TODO this interface and MetaDataColumnDataInformation class are obsolete.
   public interface MetaDataColumnInformationWithDataMeaning
   {
      MetaDataColumnDataInformation DataInformation { get; }
   }

   public interface MetaDataColumnInformationWithRawRowType
   {
      Type RawRowType { get; }
   }

   public interface MetaDataColumnInformationWithResolving
   {
      CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.MetaDataColumnInformationWithResolvingCapability ResolvingHandler { get; }
   }

   public sealed class MetaDataColumnInformationWithResolvingCapabilityWithCallbacks : CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.MetaDataColumnInformationWithResolvingCapability
   {
      private readonly ResolverCacheCreatorDelegate _cacheCreator;
      private readonly ResolverDelegate _resolver;

      public MetaDataColumnInformationWithResolvingCapabilityWithCallbacks(
         ResolverCacheCreatorDelegate cacheCreator,
         ResolverDelegate resolver
         )
      {
         ArgumentValidator.ValidateNotNull( "Resolver", resolver );

         this._cacheCreator = cacheCreator;
         this._resolver = resolver;
      }

      public Object CreateCache()
      {
         return this._cacheCreator?.Invoke();
      }

      public Boolean Resolve( CILMetaData md, Int32 rowIndex, MetaDataResolver resolver, Object cache )
      {
         return this._resolver( md, rowIndex, resolver, cache );
      }
   }

   public delegate Object ResolverCacheCreatorDelegate();

   public delegate Boolean ResolverDelegate( CILMetaData md, Int32 rowIndex, MetaDataResolver resolver, Object cache );

   public sealed class MetaDataColumnInformationForClassesOrStructs<TRow, TRawRow, TValue> : MetaDataColumnInformationForClassesOrStructs<TRow, TValue>, MetaDataColumnInformationWithSerializationCapability<TRow, TRawRow>, MetaDataColumnInformationWithDataMeaning, MetaDataColumnInformationWithRawRowType, MetaDataColumnInformationWithResolving
      where TRow : class
      where TRawRow : class
   {

      private readonly Lazy<DefaultColumnSerializationInfo<TRow, TRawRow>> _serializationInfo;

      public MetaDataColumnInformationForClassesOrStructs(
         RowColumnGetterDelegate<TRow, TValue> getter,
         RowColumnSetterDelegate<TRow, TValue> setter,
         MetaDataColumnDataInformation information,
         ResolverCacheCreatorDelegate resolverCacheCreator,
         ResolverDelegate resolver,
         Func<DefaultColumnSerializationInfo<TRow, TRawRow>> defaultSerializationInfo
         )
         : base( getter, setter )
      {
         ArgumentValidator.ValidateNotNull( "Data information", information );

         this.DataInformation = information;
         this.ResolvingHandler = resolver == null ? null : new MetaDataColumnInformationWithResolvingCapabilityWithCallbacks( resolverCacheCreator, resolver );
         this._serializationInfo = defaultSerializationInfo == null ? null : new Lazy<DefaultColumnSerializationInfo<TRow, TRawRow>>( defaultSerializationInfo, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );
      }

      public DefaultColumnSerializationInfo<TRow, TRawRow> DefaultColumnSerializationInfoWithRawType
      {
         get
         {
            return this._serializationInfo?.Value;
         }
      }

      //public DefaultColumnSerializationInfo DefaultColumnSerializationInfoNotGeneric
      //{
      //   get
      //   {
      //      return this.DefaultColumnSerializationInfoWithRawType;
      //   }
      //}

      public Type RawRowType
      {
         get
         {
            return typeof( TRawRow );
         }
      }

      public MetaDataColumnDataInformation DataInformation { get; }

      public CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.MetaDataColumnInformationWithResolvingCapability ResolvingHandler { get; }
   }

   public sealed class MetaDataColumnInformationForNullables<TRow, TRawRow, TValue> : MetaDataColumnInformationForNullables<TRow, TValue>, MetaDataColumnInformationWithSerializationCapability<TRow, TRawRow>, MetaDataColumnInformationWithDataMeaning, MetaDataColumnInformationWithRawRowType, MetaDataColumnInformationWithResolving
      where TRow : class
      where TValue : struct
      where TRawRow : class
   {

      private readonly Lazy<DefaultColumnSerializationInfo<TRow, TRawRow>> _serializationInfo;

      public MetaDataColumnInformationForNullables(
         RowColumnGetterDelegate<TRow, TValue?> getter,
         RowColumnSetterDelegate<TRow, TValue?> setter,
         MetaDataColumnDataInformation information,
         ResolverCacheCreatorDelegate resolverCacheCreator,
         ResolverDelegate resolver,
         Func<DefaultColumnSerializationInfo<TRow, TRawRow>> defaultSerializationInfo
         )
         : base( getter, setter )
      {
         ArgumentValidator.ValidateNotNull( "Data information", information );

         this.DataInformation = information;
         this.ResolvingHandler = resolver == null ? null : new MetaDataColumnInformationWithResolvingCapabilityWithCallbacks( resolverCacheCreator, resolver );
         this._serializationInfo = defaultSerializationInfo == null ? null : new Lazy<DefaultColumnSerializationInfo<TRow, TRawRow>>( defaultSerializationInfo, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );
      }

      public DefaultColumnSerializationInfo<TRow, TRawRow> DefaultColumnSerializationInfoWithRawType
      {
         get
         {
            return this._serializationInfo?.Value;
         }
      }

      //public DefaultColumnSerializationInfo DefaultColumnSerializationInfoNotGeneric
      //{
      //   get
      //   {
      //      return this.DefaultColumnSerializationInfoWithRawType;
      //   }
      //}

      public Type RawRowType
      {
         get
         {
            return typeof( TRawRow );
         }
      }

      public MetaDataColumnDataInformation DataInformation { get; }

      public CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.MetaDataColumnInformationWithResolvingCapability ResolvingHandler { get; }
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