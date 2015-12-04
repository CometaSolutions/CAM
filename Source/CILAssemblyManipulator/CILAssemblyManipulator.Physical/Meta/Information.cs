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
using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CILAssemblyManipulator.Physical.IO.Defaults;
using CILAssemblyManipulator.Physical.IO;

namespace CILAssemblyManipulator.Physical.Meta
{
   public interface MetaDataTableInformationProvider
   {
      MetaDataTableInformation GetTableInformation( Tables table );

      IEnumerable<MetaDataTableInformation> GetAllSupportedTableInformations();
   }

   public class DefaultMetaDataTableInformationProvider : MetaDataTableInformationProvider
   {
      // Don't cache the instance in case the provider will become mutable at some point.

      //private static MetaDataTableInformationProvider _Instance = new DefaultMetaDataTableInformationProvider();

      //public static MetaDataTableInformationProvider DefaultInstance
      //{
      //   get
      //   {
      //      return _Instance;
      //   }
      //}

      // ECMA-335, pp. 274-276
      public static readonly ArrayQuery<Tables?> TypeDefOrRef = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.TypeDef, Tables.TypeRef, Tables.TypeSpec } ).CQ;
      public static readonly ArrayQuery<Tables?> HasConstant = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.Field, Tables.Parameter, Tables.Property } ).CQ;
      public static readonly ArrayQuery<Tables?> HasCustomAttribute = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.MethodDef, Tables.Field, Tables.TypeRef, Tables.TypeDef, Tables.Parameter,
            Tables.InterfaceImpl, Tables.MemberRef, Tables.Module, Tables.DeclSecurity, Tables.Property, Tables.Event,
            Tables.StandaloneSignature, Tables.ModuleRef, Tables.TypeSpec, Tables.Assembly, Tables.AssemblyRef, Tables.File,
            Tables.ExportedType, Tables.ManifestResource, Tables.GenericParameter, Tables.GenericParameterConstraint, Tables.MethodSpec } ).CQ;
      public static readonly ArrayQuery<Tables?> HasFieldMarshal = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.Field, Tables.Parameter } ).CQ;
      public static readonly ArrayQuery<Tables?> HasSecurity = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.TypeDef, Tables.MethodDef, Tables.Assembly } ).CQ;
      public static readonly ArrayQuery<Tables?> MemberRefParent = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.TypeDef, Tables.TypeRef, Tables.ModuleRef, Tables.MethodDef, Tables.TypeSpec } ).CQ;
      public static readonly ArrayQuery<Tables?> HasSemantics = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.Event, Tables.Property } ).CQ;
      public static readonly ArrayQuery<Tables?> MethodDefOrRef = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.MethodDef, Tables.MemberRef } ).CQ;
      public static readonly ArrayQuery<Tables?> MemberForwarded = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.Field, Tables.MethodDef } ).CQ;
      public static readonly ArrayQuery<Tables?> Implementation = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.File, Tables.AssemblyRef, Tables.ExportedType } ).CQ;
      public static readonly ArrayQuery<Tables?> CustomAttributeType = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { null, null, Tables.MethodDef, Tables.MemberRef, null } ).CQ;
      public static readonly ArrayQuery<Tables?> ResolutionScope = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.Module, Tables.ModuleRef, Tables.AssemblyRef, Tables.TypeRef } ).CQ;
      public static readonly ArrayQuery<Tables?> TypeOrMethodDef = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.TypeDef, Tables.MethodDef } ).CQ;


      private readonly MetaDataTableInformation[] _infos;

      public DefaultMetaDataTableInformationProvider(
         IEnumerable<MetaDataTableInformation> tableInfos = null
         )
      {
         this._infos = new MetaDataTableInformation[Byte.MaxValue + 1];
         foreach ( var tableInfo in tableInfos ?? CreateDefaultTableInformation() )
         {
            this._infos[(Int32) tableInfo.TableKind] = tableInfo;
         }

      }

      public MetaDataTableInformation GetTableInformation( Tables table )
      {
         return this._infos[(Int32) table];
      }

      public IEnumerable<MetaDataTableInformation> GetAllSupportedTableInformations()
      {
         return this._infos
            .Where( i => i != null );
      }

      //public void SetTableInformation( MetaDataTableInformation tableInfo )
      //{
      //   if ( tableInfo != null )
      //   {
      //      this._infos[(Int32) tableInfo.TableKind] = tableInfo;
      //   }
      //}

      protected static IEnumerable<MetaDataTableInformation> CreateDefaultTableInformation()
      {
         yield return new MetaDataTableInformation<ModuleDefinition, RawModuleDefinition>(
            Tables.Module,
            Comparers.ModuleDefinitionEqualityComparer,
            null,
            GetModuleDefColumns(),
            () => new ModuleDefinition(),
            () => new RawModuleDefinition()
            );

         yield return new MetaDataTableInformation<TypeReference, RawTypeReference>(
            Tables.TypeRef,
            Comparers.TypeReferenceEqualityComparer,
            null,
            GetTypeRefColumns(),
            () => new TypeReference(),
            () => new RawTypeReference()
            );

         yield return new MetaDataTableInformation<TypeDefinition, RawTypeDefinition>(
            Tables.TypeDef,
            Comparers.TypeDefinitionEqualityComparer,
            null,
            GetTypeDefColumns(),
            () => new TypeDefinition(),
            () => new RawTypeDefinition()
            );

         yield return new MetaDataTableInformation<FieldDefinitionPointer, RawFieldDefinitionPointer>(
            Tables.FieldPtr,
            Comparers.FieldDefinitionPointerEqualityComparer,
            null,
            GetFieldPtrColumns(),
            () => new FieldDefinitionPointer(),
            () => new RawFieldDefinitionPointer()
            );

         yield return new MetaDataTableInformation<FieldDefinition, RawFieldDefinition>(
            Tables.Field,
            Comparers.FieldDefinitionEqualityComparer,
            null,
            GetFieldDefColumns(),
            () => new FieldDefinition(),
            () => new RawFieldDefinition()
            );

         yield return new MetaDataTableInformation<MethodDefinitionPointer, RawMethodDefinitionPointer>(
            Tables.MethodPtr,
            Comparers.MethodDefinitionPointerEqualityComparer,
            null,
            GetMethodPtrColumns(),
            () => new MethodDefinitionPointer(),
            () => new RawMethodDefinitionPointer()
            );

         yield return new MetaDataTableInformation<MethodDefinition, RawMethodDefinition>(
            Tables.MethodDef,
            Comparers.MethodDefinitionEqualityComparer,
            null,
            GetMethodDefColumns(),
            () => new MethodDefinition(),
            () => new RawMethodDefinition()
            );

         yield return new MetaDataTableInformation<ParameterDefinitionPointer, RawParameterDefinitionPointer>(
            Tables.ParameterPtr,
            Comparers.ParameterDefinitionPointerEqualityComparer,
            null,
            GetParamPtrColumns(),
            () => new ParameterDefinitionPointer(),
            () => new RawParameterDefinitionPointer()
            );

         yield return new MetaDataTableInformation<ParameterDefinition, RawParameterDefinition>(
            Tables.Parameter,
            Comparers.ParameterDefinitionEqualityComparer,
            null,
            GetParamColumns(),
            () => new ParameterDefinition(),
            () => new RawParameterDefinition()
            );

         yield return new MetaDataTableInformation<InterfaceImplementation, RawInterfaceImplementation>(
            Tables.InterfaceImpl,
            Comparers.InterfaceImplementationEqualityComparer,
            Comparers.InterfaceImplementationComparer,
            GetInterfaceImplColumns(),
            () => new InterfaceImplementation(),
            () => new RawInterfaceImplementation()
            );

         yield return new MetaDataTableInformation<MemberReference, RawMemberReference>(
            Tables.MemberRef,
            Comparers.MemberReferenceEqualityComparer,
            null,
            GetMemberRefColumns(),
            () => new MemberReference(),
            () => new RawMemberReference()
            );

         yield return new MetaDataTableInformation<ConstantDefinition, RawConstantDefinition>(
            Tables.Constant,
            Comparers.ConstantDefinitionEqualityComparer,
            Comparers.ConstantDefinitionComparer,
            GetConstantColumns(),
            () => new ConstantDefinition(),
            () => new RawConstantDefinition()
            );

         yield return new MetaDataTableInformation<CustomAttributeDefinition, RawCustomAttributeDefinition>(
            Tables.CustomAttribute,
            Comparers.CustomAttributeDefinitionEqualityComparer,
            Comparers.CustomAttributeDefinitionComparer,
            GetCustomAttributeColumns(),
            () => new CustomAttributeDefinition(),
            () => new RawCustomAttributeDefinition()
            );

         yield return new MetaDataTableInformation<FieldMarshal, RawFieldMarshal>(
            Tables.FieldMarshal,
            Comparers.FieldMarshalEqualityComparer,
            Comparers.FieldMarshalComparer,
            GetFieldMarshalColumns(),
            () => new FieldMarshal(),
            () => new RawFieldMarshal()
            );

         yield return new MetaDataTableInformation<SecurityDefinition, RawSecurityDefinition>(
            Tables.DeclSecurity,
            Comparers.SecurityDefinitionEqualityComparer,
            Comparers.SecurityDefinitionComparer,
            GetDeclSecurityColumns(),
            () => new SecurityDefinition(),
            () => new RawSecurityDefinition()
            );

         yield return new MetaDataTableInformation<ClassLayout, RawClassLayout>(
            Tables.ClassLayout,
            Comparers.ClassLayoutEqualityComparer,
            Comparers.ClassLayoutComparer,
            GetClassLayoutColumns(),
            () => new ClassLayout(),
            () => new RawClassLayout()
            );

         yield return new MetaDataTableInformation<FieldLayout, RawFieldLayout>(
            Tables.FieldLayout,
            Comparers.FieldLayoutEqualityComparer,
            Comparers.FieldLayoutComparer,
            GetFieldLayoutColumns(),
            () => new FieldLayout(),
            () => new RawFieldLayout()
            );

         yield return new MetaDataTableInformation<StandaloneSignature, RawStandaloneSignature>(
            Tables.StandaloneSignature,
            Comparers.StandaloneSignatureEqualityComparer,
            null,
            GetStandaloneSigColumns(),
            () => new StandaloneSignature(),
            () => new RawStandaloneSignature()
            );

         yield return new MetaDataTableInformation<EventMap, RawEventMap>(
            Tables.EventMap,
            Comparers.EventMapEqualityComparer,
            null,
            GetEventMapColumns(),
            () => new EventMap(),
            () => new RawEventMap()
            );

         yield return new MetaDataTableInformation<EventDefinitionPointer, RawEventDefinitionPointer>(
            Tables.EventPtr,
            Comparers.EventDefinitionPointerEqualityComparer,
            null,
            GetEventPtrColumns(),
            () => new EventDefinitionPointer(),
            () => new RawEventDefinitionPointer()
            );

         yield return new MetaDataTableInformation<EventDefinition, RawEventDefinition>(
            Tables.Event,
            Comparers.EventDefinitionEqualityComparer,
            null,
            GetEventDefColumns(),
            () => new EventDefinition(),
            () => new RawEventDefinition()
            );

         yield return new MetaDataTableInformation<PropertyMap, RawPropertyMap>(
            Tables.PropertyMap,
            Comparers.PropertyMapEqualityComparer,
            null,
            GetPropertyMapColumns(),
            () => new PropertyMap(),
            () => new RawPropertyMap()
            );

         yield return new MetaDataTableInformation<PropertyDefinitionPointer, RawPropertyDefinitionPointer>(
            Tables.PropertyPtr,
            Comparers.PropertyDefinitionPointerEqualityComparer,
            null,
            GetPropertyPtrColumns(),
            () => new PropertyDefinitionPointer(),
            () => new RawPropertyDefinitionPointer()
            );

         yield return new MetaDataTableInformation<PropertyDefinition, RawPropertyDefinition>(
            Tables.Property,
            Comparers.PropertyDefinitionEqualityComparer,
            null,
            GetPropertyDefColumns(),
            () => new PropertyDefinition(),
            () => new RawPropertyDefinition()
            );

         yield return new MetaDataTableInformation<MethodSemantics, RawMethodSemantics>(
            Tables.MethodSemantics,
            Comparers.MethodSemanticsEqualityComparer,
            Comparers.MethodSemanticsComparer,
            GetMethodSemanticsColumns(),
            () => new MethodSemantics(),
            () => new RawMethodSemantics()
            );

         yield return new MetaDataTableInformation<MethodImplementation, RawMethodImplementation>(
            Tables.MethodImpl,
            Comparers.MethodImplementationEqualityComparer,
            Comparers.MethodImplementationComparer,
            GetMethodImplColumns(),
            () => new MethodImplementation(),
            () => new RawMethodImplementation()
            );

         yield return new MetaDataTableInformation<ModuleReference, RawModuleReference>(
            Tables.ModuleRef,
            Comparers.ModuleReferenceEqualityComparer,
            null,
            GetModuleRefColumns(),
            () => new ModuleReference(),
            () => new RawModuleReference()
            );

         yield return new MetaDataTableInformation<TypeSpecification, RawTypeSpecification>(
            Tables.TypeSpec,
            Comparers.TypeSpecificationEqualityComparer,
            null,
            GetTypeSpecColumns(),
            () => new TypeSpecification(),
            () => new RawTypeSpecification()
            );

         yield return new MetaDataTableInformation<MethodImplementationMap, RawMethodImplementationMap>(
            Tables.ImplMap,
            Comparers.MethodImplementationMapEqualityComparer,
            Comparers.MethodImplementationMapComparer,
            GetImplMapColumns(),
            () => new MethodImplementationMap(),
            () => new RawMethodImplementationMap()
            );

         yield return new MetaDataTableInformation<FieldRVA, RawFieldRVA>(
            Tables.FieldRVA,
            Comparers.FieldRVAEqualityComparer,
            Comparers.FieldRVAComparer,
            GetFieldRVAColumns(),
            () => new FieldRVA(),
            () => new RawFieldRVA()
            );

         yield return new MetaDataTableInformation<EditAndContinueLog, RawEditAndContinueLog>(
            Tables.EncLog,
            Comparers.EditAndContinueLogEqualityComparer,
            null,
            GetENCLogColumns(),
            () => new EditAndContinueLog(),
            () => new RawEditAndContinueLog()
            );

         yield return new MetaDataTableInformation<EditAndContinueMap, RawEditAndContinueMap>(
            Tables.EncMap,
            Comparers.EditAndContinueMapEqualityComparer,
            null,
            GetENCMapColumns(),
            () => new EditAndContinueMap(),
            () => new RawEditAndContinueMap()
            );

         yield return new MetaDataTableInformation<AssemblyDefinition, RawAssemblyDefinition>(
            Tables.Assembly,
            Comparers.AssemblyDefinitionEqualityComparer,
            null,
            GetAssemblyDefColumns(),
            () => new AssemblyDefinition(),
            () => new RawAssemblyDefinition()
            );

#pragma warning disable 618

         yield return new MetaDataTableInformation<AssemblyDefinitionProcessor, RawAssemblyDefinitionProcessor>(
            Tables.AssemblyProcessor,
            Comparers.AssemblyDefinitionProcessorEqualityComparer,
            null,
            GetAssemblyDefProcessorColumns(),
            () => new AssemblyDefinitionProcessor(),
            () => new RawAssemblyDefinitionProcessor()
            );

         yield return new MetaDataTableInformation<AssemblyDefinitionOS, RawAssemblyDefinitionOS>(
            Tables.AssemblyOS,
            Comparers.AssemblyDefinitionOSEqualityComparer,
            null,
            GetAssemblyDefOSColumns(),
            () => new AssemblyDefinitionOS(),
            () => new RawAssemblyDefinitionOS()
            );

#pragma warning restore 618

         yield return new MetaDataTableInformation<AssemblyReference, RawAssemblyReference>(
            Tables.AssemblyRef,
            Comparers.AssemblyReferenceEqualityComparer,
            null,
            GetAssemblyRefColumns(),
            () => new AssemblyReference(),
            () => new RawAssemblyReference()
            );

#pragma warning disable 618

         yield return new MetaDataTableInformation<AssemblyReferenceProcessor, RawAssemblyReferenceProcessor>(
            Tables.AssemblyRefProcessor,
            Comparers.AssemblyReferenceProcessorEqualityComparer,
            null,
            GetAssemblyRefProcessorColumns(),
            () => new AssemblyReferenceProcessor(),
            () => new RawAssemblyReferenceProcessor()
            );

         yield return new MetaDataTableInformation<AssemblyReferenceOS, RawAssemblyReferenceOS>(
            Tables.AssemblyRefOS,
            Comparers.AssemblyReferenceOSEqualityComparer,
            null,
            GetAssemblyRefOSColumns(),
            () => new AssemblyReferenceOS(),
            () => new RawAssemblyReferenceOS()
            );

#pragma warning restore 618

         yield return new MetaDataTableInformation<FileReference, RawFileReference>(
            Tables.File,
            Comparers.FileReferenceEqualityComparer,
            null,
            GetFileColumns(),
            () => new FileReference(),
            () => new RawFileReference()
            );

         yield return new MetaDataTableInformation<ExportedType, RawExportedType>(
            Tables.ExportedType,
            Comparers.ExportedTypeEqualityComparer,
            null,
            GetExportedTypeColumns(),
            () => new ExportedType(),
            () => new RawExportedType()
            );

         yield return new MetaDataTableInformation<ManifestResource, RawManifestResource>(
            Tables.ManifestResource,
            Comparers.ManifestResourceEqualityComparer,
            null,
            GetManifestResourceColumns(),
            () => new ManifestResource(),
            () => new RawManifestResource()
            );

         yield return new MetaDataTableInformation<NestedClassDefinition, RawNestedClassDefinition>(
            Tables.NestedClass,
            Comparers.NestedClassDefinitionEqualityComparer,
            Comparers.NestedClassDefinitionComparer,
            GetNestedClassColumns(),
            () => new NestedClassDefinition(),
            () => new RawNestedClassDefinition()
            );

         yield return new MetaDataTableInformation<GenericParameterDefinition, RawGenericParameterDefinition>(
            Tables.GenericParameter,
            Comparers.GenericParameterDefinitionEqualityComparer,
            Comparers.GenericParameterDefinitionComparer,
            GetGenericParamColumns(),
            () => new GenericParameterDefinition(),
            () => new RawGenericParameterDefinition()
            );

         yield return new MetaDataTableInformation<MethodSpecification, RawMethodSpecification>(
            Tables.MethodSpec,
            Comparers.MethodSpecificationEqualityComparer,
            null,
            GetMethodSpecColumns(),
            () => new MethodSpecification(),
            () => new RawMethodSpecification()
            );

         yield return new MetaDataTableInformation<GenericParameterConstraintDefinition, RawGenericParameterConstraintDefinition>(
            Tables.GenericParameterConstraint,
            Comparers.GenericParameterConstraintDefinitionEqualityComparer,
            Comparers.GenericParameterConstraintDefinitionComparer,
            GetGenericParamConstraintColumns(),
            () => new GenericParameterConstraintDefinition(),
            () => new RawGenericParameterConstraintDefinition()
            );
      }

      protected static IEnumerable<MetaDataColumnInformation<ModuleDefinition, RawModuleDefinition>> GetModuleDefColumns()
      {
         yield return MetaDataColumnInformation.Number16<ModuleDefinition, RawModuleDefinition>( nameof( ModuleDefinition.Generation ), ( r, v ) => r.Generation = v, row => row.Generation, ( r, v ) => r.Generation = v );
         yield return MetaDataColumnInformation.SystemString<ModuleDefinition, RawModuleDefinition>( nameof( ModuleDefinition.Name ), ( r, v ) => r.Name = v, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformation.GUID<ModuleDefinition, RawModuleDefinition>( nameof( ModuleDefinition.ModuleGUID ), ( r, v ) => r.ModuleGUID = v, row => row.ModuleGUID, ( r, v ) => r.ModuleGUID = v );
         yield return MetaDataColumnInformation.GUID<ModuleDefinition, RawModuleDefinition>( nameof( ModuleDefinition.EditAndContinueGUID ), ( r, v ) => r.EditAndContinueGUID = v, row => row.EditAndContinueGUID, ( r, v ) => r.EditAndContinueGUID = v );
         yield return MetaDataColumnInformation.GUID<ModuleDefinition, RawModuleDefinition>( nameof( ModuleDefinition.EditAndContinueBaseGUID ), ( r, v ) => r.EditAndContinueBaseGUID = v, row => row.EditAndContinueBaseGUID, ( r, v ) => r.EditAndContinueBaseGUID = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<TypeReference, RawTypeReference>> GetTypeRefColumns()
      {
         yield return MetaDataColumnInformation.CodedTableIndexNullable<TypeReference, RawTypeReference>( nameof( TypeReference.ResolutionScope ), ResolutionScope, ( r, v ) => r.ResolutionScope = v, row => row.ResolutionScope, ( r, v ) => r.ResolutionScope = v );
         yield return MetaDataColumnInformation.SystemString<TypeReference, RawTypeReference>( nameof( TypeReference.Name ), ( r, v ) => r.Name = v, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformation.SystemString<TypeReference, RawTypeReference>( nameof( TypeReference.Namespace ), ( r, v ) => r.Namespace = v, row => row.Namespace, ( r, v ) => r.Namespace = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<TypeDefinition, RawTypeDefinition>> GetTypeDefColumns()
      {
         yield return MetaDataColumnInformation.Constant32<TypeDefinition, RawTypeDefinition, TypeAttributes>( nameof( TypeDefinition.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes, ( r, v ) => r.Attributes = (TypeAttributes) v, i => (TypeAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformation.SystemString<TypeDefinition, RawTypeDefinition>( nameof( TypeDefinition.Name ), ( r, v ) => r.Name = v, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformation.SystemString<TypeDefinition, RawTypeDefinition>( nameof( TypeDefinition.Namespace ), ( r, v ) => r.Namespace = v, row => row.Namespace, ( r, v ) => r.Namespace = v );
         yield return MetaDataColumnInformation.CodedTableIndexNullable<TypeDefinition, RawTypeDefinition>( nameof( TypeDefinition.BaseType ), TypeDefOrRef, ( r, v ) => r.BaseType = v, row => row.BaseType, ( r, v ) => r.BaseType = v );
         yield return MetaDataColumnInformation.SimpleTableIndex<TypeDefinition, RawTypeDefinition>( nameof( TypeDefinition.FieldList ), Tables.Field, ( r, v ) => r.FieldList = v, row => row.FieldList, ( r, v ) => r.FieldList = v );
         yield return MetaDataColumnInformation.SimpleTableIndex<TypeDefinition, RawTypeDefinition>( nameof( TypeDefinition.MethodList ), Tables.MethodDef, ( r, v ) => r.MethodList = v, row => row.MethodList, ( r, v ) => r.MethodList = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<FieldDefinitionPointer, RawFieldDefinitionPointer>> GetFieldPtrColumns()
      {
         yield return MetaDataColumnInformation.SimpleTableIndex<FieldDefinitionPointer, RawFieldDefinitionPointer>( nameof( FieldDefinitionPointer.FieldIndex ), Tables.Field, ( r, v ) => r.FieldIndex = v, row => row.FieldIndex, ( r, v ) => r.FieldIndex = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<FieldDefinition, RawFieldDefinition>> GetFieldDefColumns()
      {
         yield return MetaDataColumnInformation.Constant16<FieldDefinition, RawFieldDefinition, FieldAttributes>( nameof( FieldDefinition.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes, ( r, v ) => r.Attributes = (FieldAttributes) v, i => (FieldAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformation.SystemString<FieldDefinition, RawFieldDefinition>( nameof( FieldDefinition.Name ), ( r, v ) => r.Name = v, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformation.BLOBNonTypeSignature<FieldDefinition, RawFieldDefinition, FieldSignature>( nameof( FieldDefinition.Signature ), ( r, v ) => r.Signature = v, row => row.Signature, ( r, v ) => r.Signature = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MethodDefinitionPointer, RawMethodDefinitionPointer>> GetMethodPtrColumns()
      {
         yield return MetaDataColumnInformation.SimpleTableIndex<MethodDefinitionPointer, RawMethodDefinitionPointer>( nameof( MethodDefinitionPointer.MethodIndex ), Tables.MethodDef, ( r, v ) => r.MethodIndex = v, row => row.MethodIndex, ( r, v ) => r.MethodIndex = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MethodDefinition, RawMethodDefinition>> GetMethodDefColumns()
      {
         yield return MetaDataColumnInformation.DataReferenceForClasses<MethodDefinition, RawMethodDefinition, MethodILDefinition>( nameof( MethodDefinition.IL ), ( r, v ) => r.IL = v, r => r.IL, ( r, v ) => r.RVA = v, ( args, v ) => args.Row.IL = DefaultMetaDataSerializationSupportProvider.DeserializeIL( args.RowArgs, v, args.Row ), ( md, mdStreamContainer ) => new SectionPart_MethodIL( md, mdStreamContainer.UserStrings ) );
         yield return MetaDataColumnInformation.Constant16<MethodDefinition, RawMethodDefinition, MethodImplAttributes>( nameof( MethodDefinition.ImplementationAttributes ), ( r, v ) => r.ImplementationAttributes = v, row => row.ImplementationAttributes, ( r, v ) => r.ImplementationAttributes = (MethodImplAttributes) v, i => (MethodImplAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformation.Constant16<MethodDefinition, RawMethodDefinition, MethodAttributes>( nameof( MethodDefinition.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes, ( r, v ) => r.Attributes = (MethodAttributes) v, i => (MethodAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformation.SystemString<MethodDefinition, RawMethodDefinition>( nameof( MethodDefinition.Name ), ( r, v ) => r.Name = v, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformation.BLOBNonTypeSignature<MethodDefinition, RawMethodDefinition, MethodDefinitionSignature>( nameof( MethodDefinition.Signature ), ( r, v ) => r.Signature = v, row => row.Signature, ( r, v ) => r.Signature = v );
         yield return MetaDataColumnInformation.SimpleTableIndex<MethodDefinition, RawMethodDefinition>( nameof( MethodDefinition.ParameterList ), Tables.Parameter, ( r, v ) => r.ParameterList = v, row => row.ParameterList, ( r, v ) => r.ParameterList = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<ParameterDefinitionPointer, RawParameterDefinitionPointer>> GetParamPtrColumns()
      {
         yield return MetaDataColumnInformation.SimpleTableIndex<ParameterDefinitionPointer, RawParameterDefinitionPointer>( nameof( ParameterDefinitionPointer.ParameterIndex ), Tables.Parameter, ( r, v ) => r.ParameterIndex = v, row => row.ParameterIndex, ( r, v ) => r.ParameterIndex = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<ParameterDefinition, RawParameterDefinition>> GetParamColumns()
      {
         yield return MetaDataColumnInformation.Constant16<ParameterDefinition, RawParameterDefinition, ParameterAttributes>( nameof( ParameterDefinition.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes, ( r, v ) => r.Attributes = (ParameterAttributes) v, i => (ParameterAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformation.Number32_SerializedAs16<ParameterDefinition, RawParameterDefinition>( nameof( ParameterDefinition.Sequence ), ( r, v ) => r.Sequence = v, row => row.Sequence, ( r, v ) => r.Sequence = v );
         yield return MetaDataColumnInformation.SystemString<ParameterDefinition, RawParameterDefinition>( nameof( ParameterDefinition.Name ), ( r, v ) => r.Name = v, row => row.Name, ( r, v ) => r.Name = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<InterfaceImplementation, RawInterfaceImplementation>> GetInterfaceImplColumns()
      {
         yield return MetaDataColumnInformation.SimpleTableIndex<InterfaceImplementation, RawInterfaceImplementation>( nameof( InterfaceImplementation.Class ), Tables.TypeDef, ( r, v ) => r.Class = v, row => row.Class, ( r, v ) => r.Class = v );
         yield return MetaDataColumnInformation.CodedTableIndex<InterfaceImplementation, RawInterfaceImplementation>( nameof( InterfaceImplementation.Interface ), TypeDefOrRef, ( r, v ) => r.Interface = v, row => row.Interface, ( r, v ) => r.Interface = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MemberReference, RawMemberReference>> GetMemberRefColumns()
      {
         yield return MetaDataColumnInformation.CodedTableIndex<MemberReference, RawMemberReference>( nameof( MemberReference.DeclaringType ), MemberRefParent, ( r, v ) => r.DeclaringType = v, row => row.DeclaringType, ( r, v ) => r.DeclaringType = v );
         yield return MetaDataColumnInformation.SystemString<MemberReference, RawMemberReference>( nameof( MemberReference.Name ), ( r, v ) => r.Name = v, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformation.BLOBNonTypeSignature<MemberReference, RawMemberReference, AbstractSignature>( nameof( MemberReference.Signature ), ( r, v ) => r.Signature = v, row => row.Signature, ( r, v ) => r.Signature = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<ConstantDefinition, RawConstantDefinition>> GetConstantColumns()
      {
         yield return MetaDataColumnInformation.Constant8<ConstantDefinition, RawConstantDefinition, SignatureElementTypes>( nameof( ConstantDefinition.Type ), ( r, v ) => r.Type = v, row => row.Type, ( r, v ) => r.Type = (SignatureElementTypes) v, i => (SignatureElementTypes) i, v => (Int32) v );
         yield return MetaDataColumnInformation.Number8<ConstantDefinition, RawConstantDefinition>( "Padding", ( r, v ) => { }, row => 0, ( r, v ) => r.Padding = (Byte) v );
         yield return MetaDataColumnInformation.CodedTableIndex<ConstantDefinition, RawConstantDefinition>( nameof( ConstantDefinition.Parent ), HasConstant, ( r, v ) => r.Parent = v, row => row.Parent, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformation.BLOBCustom<ConstantDefinition, RawConstantDefinition, Object>( nameof( ConstantDefinition.Value ), ( r, v ) => r.Value = v, r => r.Value, ( r, v ) => r.Value = v, ( args, v, blobs ) => args.Row.Value = blobs.ReadConstantValue( v, args.Row.Type ), args => args.RowArgs.Array.CreateConstantBytes( args.Row.Value, args.Row.Type ) );
      }

      protected static IEnumerable<MetaDataColumnInformation<CustomAttributeDefinition, RawCustomAttributeDefinition>> GetCustomAttributeColumns()
      {
         yield return MetaDataColumnInformation.CodedTableIndex<CustomAttributeDefinition, RawCustomAttributeDefinition>( nameof( CustomAttributeDefinition.Parent ), HasCustomAttribute, ( r, v ) => r.Parent = v, row => row.Parent, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformation.CodedTableIndex<CustomAttributeDefinition, RawCustomAttributeDefinition>( nameof( CustomAttributeDefinition.Type ), CustomAttributeType, ( r, v ) => r.Type = v, row => row.Type, ( r, v ) => r.Type = v );
         yield return MetaDataColumnInformation.BLOBCustom<CustomAttributeDefinition, RawCustomAttributeDefinition, AbstractCustomAttributeSignature>( nameof( CustomAttributeDefinition.Signature ), ( r, v ) => r.Signature = v, r => r.Signature, ( r, v ) => r.Signature = v, ( args, v, blobs ) => args.Row.Signature = blobs.ReadCASignature( v ), args => args.RowArgs.Array.CreateCustomAttributeSignature( args.RowArgs.MetaData, args.RowIndex ) );
      }

      protected static IEnumerable<MetaDataColumnInformation<FieldMarshal, RawFieldMarshal>> GetFieldMarshalColumns()
      {
         yield return MetaDataColumnInformation.CodedTableIndex<FieldMarshal, RawFieldMarshal>( nameof( FieldMarshal.Parent ), HasFieldMarshal, ( r, v ) => r.Parent = v, row => row.Parent, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformation.BLOBCustom<FieldMarshal, RawFieldMarshal, AbstractMarshalingInfo>( nameof( FieldMarshal.NativeType ), ( r, v ) => r.NativeType = v, row => row.NativeType, ( r, v ) => r.NativeType = v, ( args, v, blobs ) => args.Row.NativeType = blobs.ReadMarshalingInfo( v ), args => args.RowArgs.Array.CreateMarshalSpec( args.Row.NativeType ) );
      }

      protected static IEnumerable<MetaDataColumnInformation<SecurityDefinition, RawSecurityDefinition>> GetDeclSecurityColumns()
      {
         yield return MetaDataColumnInformation.Constant16<SecurityDefinition, RawSecurityDefinition, SecurityAction>( nameof( SecurityDefinition.Action ), ( r, v ) => r.Action = v, row => row.Action, ( r, v ) => r.Action = (SecurityAction) v, i => (SecurityAction) i, v => (Int32) v );
         yield return MetaDataColumnInformation.CodedTableIndex<SecurityDefinition, RawSecurityDefinition>( nameof( SecurityDefinition.Parent ), HasSecurity, ( r, v ) => r.Parent = v, row => row.Parent, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformation.BLOBCustom<SecurityDefinition, RawSecurityDefinition, List<AbstractSecurityInformation>>( nameof( SecurityDefinition.PermissionSets ), ( r, v ) => { r.PermissionSets.Clear(); r.PermissionSets.AddRange( v ); }, row => row.PermissionSets, ( r, v ) => r.PermissionSets = v, ( args, v, blobs ) => blobs.ReadSecurityInformation( v, args.Row.PermissionSets ), args => args.RowArgs.Array.CreateSecuritySignature( args.Row.PermissionSets, args.RowArgs.AuxArray ) );
      }

      protected static IEnumerable<MetaDataColumnInformation<ClassLayout, RawClassLayout>> GetClassLayoutColumns()
      {
         yield return MetaDataColumnInformation.Number32_SerializedAs16<ClassLayout, RawClassLayout>( nameof( ClassLayout.PackingSize ), ( r, v ) => r.PackingSize = v, row => row.PackingSize, ( r, v ) => r.PackingSize = v );
         yield return MetaDataColumnInformation.Number32<ClassLayout, RawClassLayout>( nameof( ClassLayout.ClassSize ), ( r, v ) => r.ClassSize = v, row => row.ClassSize, ( r, v ) => r.ClassSize = v );
         yield return MetaDataColumnInformation.SimpleTableIndex<ClassLayout, RawClassLayout>( nameof( ClassLayout.Parent ), Tables.TypeDef, ( r, v ) => r.Parent = v, row => row.Parent, ( r, v ) => r.Parent = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<FieldLayout, RawFieldLayout>> GetFieldLayoutColumns()
      {
         yield return MetaDataColumnInformation.Number32<FieldLayout, RawFieldLayout>( nameof( FieldLayout.Offset ), ( r, v ) => r.Offset = v, row => row.Offset, ( r, v ) => r.Offset = v );
         yield return MetaDataColumnInformation.SimpleTableIndex<FieldLayout, RawFieldLayout>( nameof( FieldLayout.Field ), Tables.Field, ( r, v ) => r.Field = v, row => row.Field, ( r, v ) => r.Field = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<StandaloneSignature, RawStandaloneSignature>> GetStandaloneSigColumns()
      {
         yield return MetaDataColumnInformation.BLOBCustom<StandaloneSignature, RawStandaloneSignature, AbstractSignature>( nameof( StandaloneSignature.Signature ), ( r, v ) => r.Signature = v, r => r.Signature, ( r, v ) => r.Signature = v, ( args, v, blobs ) =>
         {
            Boolean wasFieldSig;
            args.Row.Signature = blobs.ReadNonTypeSignature( v, false, true, out wasFieldSig );
            args.Row.StoreSignatureAsFieldSignature = wasFieldSig;
         }, args => args.RowArgs.Array.CreateStandaloneSignature( args.Row ) );
      }

      protected static IEnumerable<MetaDataColumnInformation<EventMap, RawEventMap>> GetEventMapColumns()
      {
         yield return MetaDataColumnInformation.SimpleTableIndex<EventMap, RawEventMap>( nameof( EventMap.Parent ), Tables.TypeDef, ( r, v ) => r.Parent = v, row => row.Parent, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformation.SimpleTableIndex<EventMap, RawEventMap>( nameof( EventMap.EventList ), Tables.Event, ( r, v ) => r.EventList = v, row => row.EventList, ( r, v ) => r.EventList = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<EventDefinitionPointer, RawEventDefinitionPointer>> GetEventPtrColumns()
      {
         yield return MetaDataColumnInformation.SimpleTableIndex<EventDefinitionPointer, RawEventDefinitionPointer>( nameof( EventDefinitionPointer.EventIndex ), Tables.Event, ( r, v ) => r.EventIndex = v, row => row.EventIndex, ( r, v ) => r.EventIndex = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<EventDefinition, RawEventDefinition>> GetEventDefColumns()
      {
         yield return MetaDataColumnInformation.Constant16<EventDefinition, RawEventDefinition, EventAttributes>( nameof( EventDefinition.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes, ( r, v ) => r.Attributes = (EventAttributes) v, i => (EventAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformation.SystemString<EventDefinition, RawEventDefinition>( nameof( EventDefinition.Name ), ( r, v ) => r.Name = v, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformation.CodedTableIndex<EventDefinition, RawEventDefinition>( nameof( EventDefinition.EventType ), TypeDefOrRef, ( r, v ) => r.EventType = v, row => row.EventType, ( r, v ) => r.EventType = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<PropertyMap, RawPropertyMap>> GetPropertyMapColumns()
      {
         yield return MetaDataColumnInformation.SimpleTableIndex<PropertyMap, RawPropertyMap>( nameof( PropertyMap.Parent ), Tables.TypeDef, ( r, v ) => r.Parent = v, row => row.Parent, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformation.SimpleTableIndex<PropertyMap, RawPropertyMap>( nameof( PropertyMap.PropertyList ), Tables.Property, ( r, v ) => r.PropertyList = v, row => row.PropertyList, ( r, v ) => r.PropertyList = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<PropertyDefinitionPointer, RawPropertyDefinitionPointer>> GetPropertyPtrColumns()
      {
         yield return MetaDataColumnInformation.SimpleTableIndex<PropertyDefinitionPointer, RawPropertyDefinitionPointer>( nameof( PropertyDefinitionPointer.PropertyIndex ), Tables.Property, ( r, v ) => r.PropertyIndex = v, row => row.PropertyIndex, ( r, v ) => r.PropertyIndex = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<PropertyDefinition, RawPropertyDefinition>> GetPropertyDefColumns()
      {
         yield return MetaDataColumnInformation.Constant16<PropertyDefinition, RawPropertyDefinition, PropertyAttributes>( nameof( PropertyDefinition.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes, ( r, v ) => r.Attributes = (PropertyAttributes) v, i => (PropertyAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformation.SystemString<PropertyDefinition, RawPropertyDefinition>( nameof( PropertyDefinition.Name ), ( r, v ) => r.Name = v, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformation.BLOBNonTypeSignature<PropertyDefinition, RawPropertyDefinition, PropertySignature>( nameof( PropertyDefinition.Signature ), ( r, v ) => r.Signature = v, row => row.Signature, ( r, v ) => r.Signature = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MethodSemantics, RawMethodSemantics>> GetMethodSemanticsColumns()
      {
         yield return MetaDataColumnInformation.Constant16<MethodSemantics, RawMethodSemantics, MethodSemanticsAttributes>( nameof( MethodSemantics.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes, ( r, v ) => r.Attributes = (MethodSemanticsAttributes) v, i => (MethodSemanticsAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformation.SimpleTableIndex<MethodSemantics, RawMethodSemantics>( nameof( MethodSemantics.Method ), Tables.MethodDef, ( r, v ) => r.Method = v, row => row.Method, ( r, v ) => r.Method = v );
         yield return MetaDataColumnInformation.CodedTableIndex<MethodSemantics, RawMethodSemantics>( nameof( MethodSemantics.Associaton ), HasSemantics, ( r, v ) => r.Associaton = v, row => row.Associaton, ( r, v ) => r.Associaton = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MethodImplementation, RawMethodImplementation>> GetMethodImplColumns()
      {
         yield return MetaDataColumnInformation.SimpleTableIndex<MethodImplementation, RawMethodImplementation>( nameof( MethodImplementation.Class ), Tables.TypeDef, ( r, v ) => r.Class = v, row => row.Class, ( r, v ) => r.Class = v );
         yield return MetaDataColumnInformation.CodedTableIndex<MethodImplementation, RawMethodImplementation>( nameof( MethodImplementation.MethodBody ), MethodDefOrRef, ( r, v ) => r.MethodBody = v, row => row.MethodBody, ( r, v ) => r.MethodBody = v );
         yield return MetaDataColumnInformation.CodedTableIndex<MethodImplementation, RawMethodImplementation>( nameof( MethodImplementation.MethodDeclaration ), MethodDefOrRef, ( r, v ) => r.MethodDeclaration = v, row => row.MethodDeclaration, ( r, v ) => r.MethodDeclaration = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<ModuleReference, RawModuleReference>> GetModuleRefColumns()
      {
         yield return MetaDataColumnInformation.SystemString<ModuleReference, RawModuleReference>( nameof( ModuleReference.ModuleName ), ( r, v ) => r.ModuleName = v, row => row.ModuleName, ( r, v ) => r.ModuleName = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<TypeSpecification, RawTypeSpecification>> GetTypeSpecColumns()
      {
         yield return MetaDataColumnInformation.BLOBTypeSignature<TypeSpecification, RawTypeSpecification>( nameof( TypeSpecification.Signature ), ( r, v ) => r.Signature = v, row => row.Signature, ( r, v ) => r.Signature = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MethodImplementationMap, RawMethodImplementationMap>> GetImplMapColumns()
      {
         yield return MetaDataColumnInformation.Constant16<MethodImplementationMap, RawMethodImplementationMap, PInvokeAttributes>( nameof( MethodImplementationMap.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes, ( r, v ) => r.Attributes = (PInvokeAttributes) v, i => (PInvokeAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformation.CodedTableIndex<MethodImplementationMap, RawMethodImplementationMap>( nameof( MethodImplementationMap.MemberForwarded ), MemberForwarded, ( r, v ) => r.MemberForwarded = v, row => row.MemberForwarded, ( r, v ) => r.MemberForwarded = v );
         yield return MetaDataColumnInformation.SystemString<MethodImplementationMap, RawMethodImplementationMap>( nameof( MethodImplementationMap.ImportName ), ( r, v ) => r.ImportName = v, row => row.ImportName, ( r, v ) => r.ImportName = v );
         yield return MetaDataColumnInformation.SimpleTableIndex<MethodImplementationMap, RawMethodImplementationMap>( nameof( MethodImplementationMap.ImportScope ), Tables.ModuleRef, ( r, v ) => r.ImportScope = v, row => row.ImportScope, ( r, v ) => r.ImportScope = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<FieldRVA, RawFieldRVA>> GetFieldRVAColumns()
      {
         yield return MetaDataColumnInformation.DataReferenceForClasses<FieldRVA, RawFieldRVA, Byte[]>( nameof( FieldRVA.Data ), ( r, v ) => r.Data = v, r => r.Data, ( r, v ) => r.RVA = v, ( args, rva ) => args.Row.Data = DefaultMetaDataSerializationSupportProvider.DeserializeConstantValue( args.RowArgs, args.Row, rva ), ( md, mdStreamContainer ) => new SectionPart_FieldRVA( md ) );
         yield return MetaDataColumnInformation.SimpleTableIndex<FieldRVA, RawFieldRVA>( nameof( FieldRVA.Field ), Tables.Field, ( r, v ) => r.Field = v, row => row.Field, ( r, v ) => r.Field = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<EditAndContinueLog, RawEditAndContinueLog>> GetENCLogColumns()
      {
         yield return MetaDataColumnInformation.Number32<EditAndContinueLog, RawEditAndContinueLog>( nameof( EditAndContinueLog.Token ), ( r, v ) => r.Token = v, row => row.Token, ( r, v ) => r.Token = v );
         yield return MetaDataColumnInformation.Number32<EditAndContinueLog, RawEditAndContinueLog>( nameof( EditAndContinueLog.FuncCode ), ( r, v ) => r.FuncCode = v, row => row.FuncCode, ( r, v ) => r.FuncCode = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<EditAndContinueMap, RawEditAndContinueMap>> GetENCMapColumns()
      {
         yield return MetaDataColumnInformation.Number32<EditAndContinueMap, RawEditAndContinueMap>( nameof( EditAndContinueMap.Token ), ( r, v ) => r.Token = v, row => row.Token, ( r, v ) => r.Token = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<AssemblyDefinition, RawAssemblyDefinition>> GetAssemblyDefColumns()
      {
         yield return MetaDataColumnInformation.Constant32<AssemblyDefinition, RawAssemblyDefinition, AssemblyHashAlgorithm>( nameof( AssemblyDefinition.HashAlgorithm ), ( r, v ) => r.HashAlgorithm = v, row => row.HashAlgorithm, ( r, v ) => r.HashAlgorithm = (AssemblyHashAlgorithm) v, i => (AssemblyHashAlgorithm) i, v => (Int32) v );
         yield return MetaDataColumnInformation.Number32_SerializedAs16<AssemblyDefinition, RawAssemblyDefinition>( nameof( AssemblyInformation.VersionMajor ), ( r, v ) => r.AssemblyInformation.VersionMajor = v, row => row.AssemblyInformation.VersionMajor, ( r, v ) => r.MajorVersion = v );
         yield return MetaDataColumnInformation.Number32_SerializedAs16<AssemblyDefinition, RawAssemblyDefinition>( nameof( AssemblyInformation.VersionMinor ), ( r, v ) => r.AssemblyInformation.VersionMinor = v, row => row.AssemblyInformation.VersionMinor, ( r, v ) => r.MinorVersion = v );
         yield return MetaDataColumnInformation.Number32_SerializedAs16<AssemblyDefinition, RawAssemblyDefinition>( nameof( AssemblyInformation.VersionBuild ), ( r, v ) => r.AssemblyInformation.VersionBuild = v, row => row.AssemblyInformation.VersionBuild, ( r, v ) => r.BuildNumber = v );
         yield return MetaDataColumnInformation.Number32_SerializedAs16<AssemblyDefinition, RawAssemblyDefinition>( nameof( AssemblyInformation.VersionRevision ), ( r, v ) => r.AssemblyInformation.VersionRevision = v, row => row.AssemblyInformation.VersionRevision, ( r, v ) => r.RevisionNumber = v );
         yield return MetaDataColumnInformation.Constant32<AssemblyDefinition, RawAssemblyDefinition, AssemblyFlags>( nameof( AssemblyDefinition.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes, ( r, v ) => r.Attributes = (AssemblyFlags) v, i => (AssemblyFlags) i, v => (Int32) v );
         yield return MetaDataColumnInformation.BLOBCustom<AssemblyDefinition, RawAssemblyDefinition, Byte[]>( nameof( AssemblyInformation.PublicKeyOrToken ), ( r, v ) => r.AssemblyInformation.PublicKeyOrToken = v, r => r.AssemblyInformation.PublicKeyOrToken, ( r, v ) => r.PublicKey = v, ( args, v, blobs ) => args.Row.AssemblyInformation.PublicKeyOrToken = blobs.GetBLOB( v ), args =>
         {
            var pk = args.Row.AssemblyInformation.PublicKeyOrToken;
            return pk.IsNullOrEmpty() ? args.RowArgs.ThisAssemblyPublicKeyIfPresentNull?.ToArray() : pk;
         } );
         yield return MetaDataColumnInformation.SystemString<AssemblyDefinition, RawAssemblyDefinition>( nameof( AssemblyInformation.Name ), ( r, v ) => r.AssemblyInformation.Name = v, row => row.AssemblyInformation.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformation.SystemString<AssemblyDefinition, RawAssemblyDefinition>( nameof( AssemblyInformation.Culture ), ( r, v ) => r.AssemblyInformation.Culture = v, row => row.AssemblyInformation.Culture, ( r, v ) => r.Culture = v );
      }
#pragma warning disable 618
      protected static IEnumerable<MetaDataColumnInformation<AssemblyDefinitionProcessor, RawAssemblyDefinitionProcessor>> GetAssemblyDefProcessorColumns()
      {
         yield return MetaDataColumnInformation.Number32<AssemblyDefinitionProcessor, RawAssemblyDefinitionProcessor>( nameof( AssemblyDefinitionProcessor.Processor ), ( r, v ) => r.Processor = v, row => row.Processor, ( r, v ) => r.Processor = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<AssemblyDefinitionOS, RawAssemblyDefinitionOS>> GetAssemblyDefOSColumns()
      {
         yield return MetaDataColumnInformation.Number32<AssemblyDefinitionOS, RawAssemblyDefinitionOS>( nameof( AssemblyDefinitionOS.OSPlatformID ), ( r, v ) => r.OSPlatformID = v, row => row.OSPlatformID, ( r, v ) => r.OSPlatformID = v );
         yield return MetaDataColumnInformation.Number32<AssemblyDefinitionOS, RawAssemblyDefinitionOS>( nameof( AssemblyDefinitionOS.OSMajorVersion ), ( r, v ) => r.OSMajorVersion = v, row => row.OSMajorVersion, ( r, v ) => r.OSMajorVersion = v );
         yield return MetaDataColumnInformation.Number32<AssemblyDefinitionOS, RawAssemblyDefinitionOS>( nameof( AssemblyDefinitionOS.OSMinorVersion ), ( r, v ) => r.OSMinorVersion = v, row => row.OSMinorVersion, ( r, v ) => r.OSMinorVersion = v );
      }
#pragma warning restore 618

      protected static IEnumerable<MetaDataColumnInformation<AssemblyReference, RawAssemblyReference>> GetAssemblyRefColumns()
      {
         yield return MetaDataColumnInformation.Number32_SerializedAs16<AssemblyReference, RawAssemblyReference>( nameof( AssemblyInformation.VersionMajor ), ( r, v ) => r.AssemblyInformation.VersionMajor = v, row => row.AssemblyInformation.VersionMajor, ( r, v ) => r.MajorVersion = v );
         yield return MetaDataColumnInformation.Number32_SerializedAs16<AssemblyReference, RawAssemblyReference>( nameof( AssemblyInformation.VersionMinor ), ( r, v ) => r.AssemblyInformation.VersionMinor = v, row => row.AssemblyInformation.VersionMinor, ( r, v ) => r.MinorVersion = v );
         yield return MetaDataColumnInformation.Number32_SerializedAs16<AssemblyReference, RawAssemblyReference>( nameof( AssemblyInformation.VersionBuild ), ( r, v ) => r.AssemblyInformation.VersionBuild = v, row => row.AssemblyInformation.VersionBuild, ( r, v ) => r.BuildNumber = v );
         yield return MetaDataColumnInformation.Number32_SerializedAs16<AssemblyReference, RawAssemblyReference>( nameof( AssemblyInformation.VersionRevision ), ( r, v ) => r.AssemblyInformation.VersionRevision = v, row => row.AssemblyInformation.VersionRevision, ( r, v ) => r.RevisionNumber = v );
         yield return MetaDataColumnInformation.Constant32<AssemblyReference, RawAssemblyReference, AssemblyFlags>( nameof( AssemblyDefinition.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes, ( r, v ) => r.Attributes = (AssemblyFlags) v, i => (AssemblyFlags) i, v => (Int32) v );
         yield return MetaDataColumnInformation.BLOBByteArray<AssemblyReference, RawAssemblyReference>( nameof( AssemblyInformation.PublicKeyOrToken ), ( r, v ) => r.AssemblyInformation.PublicKeyOrToken = v, r => r.AssemblyInformation.PublicKeyOrToken, ( r, v ) => r.PublicKeyOrToken = v );
         yield return MetaDataColumnInformation.SystemString<AssemblyReference, RawAssemblyReference>( nameof( AssemblyInformation.Name ), ( r, v ) => r.AssemblyInformation.Name = v, row => row.AssemblyInformation.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformation.SystemString<AssemblyReference, RawAssemblyReference>( nameof( AssemblyInformation.Culture ), ( r, v ) => r.AssemblyInformation.Culture = v, row => row.AssemblyInformation.Culture, ( r, v ) => r.Culture = v );
         yield return MetaDataColumnInformation.BLOBByteArray<AssemblyReference, RawAssemblyReference>( nameof( AssemblyReference.HashValue ), ( r, v ) => r.HashValue = v, row => row.HashValue, ( r, v ) => r.HashValue = v );
      }

#pragma warning disable 618
      protected static IEnumerable<MetaDataColumnInformation<AssemblyReferenceProcessor, RawAssemblyReferenceProcessor>> GetAssemblyRefProcessorColumns()
      {
         yield return MetaDataColumnInformation.Number32<AssemblyReferenceProcessor, RawAssemblyReferenceProcessor>( nameof( AssemblyReferenceProcessor.Processor ), ( r, v ) => r.Processor = v, row => row.Processor, ( r, v ) => r.Processor = v );
         yield return MetaDataColumnInformation.SimpleTableIndex<AssemblyReferenceProcessor, RawAssemblyReferenceProcessor>( nameof( AssemblyReferenceProcessor.AssemblyRef ), Tables.AssemblyRef, ( r, v ) => r.AssemblyRef = v, row => row.AssemblyRef, ( r, v ) => r.AssemblyRef = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<AssemblyReferenceOS, RawAssemblyReferenceOS>> GetAssemblyRefOSColumns()
      {
         yield return MetaDataColumnInformation.Number32<AssemblyReferenceOS, RawAssemblyReferenceOS>( nameof( AssemblyReferenceOS.OSPlatformID ), ( r, v ) => r.OSPlatformID = v, row => row.OSPlatformID, ( r, v ) => r.OSPlatformID = v );
         yield return MetaDataColumnInformation.Number32<AssemblyReferenceOS, RawAssemblyReferenceOS>( nameof( AssemblyReferenceOS.OSMajorVersion ), ( r, v ) => r.OSMajorVersion = v, row => row.OSMajorVersion, ( r, v ) => r.OSMajorVersion = v );
         yield return MetaDataColumnInformation.Number32<AssemblyReferenceOS, RawAssemblyReferenceOS>( nameof( AssemblyReferenceOS.OSMinorVersion ), ( r, v ) => r.OSMinorVersion = v, row => row.OSMinorVersion, ( r, v ) => r.OSMinorVersion = v );
         yield return MetaDataColumnInformation.SimpleTableIndex<AssemblyReferenceOS, RawAssemblyReferenceOS>( nameof( AssemblyReferenceOS.AssemblyRef ), Tables.AssemblyRef, ( r, v ) => r.AssemblyRef = v, row => row.AssemblyRef, ( r, v ) => r.AssemblyRef = v );
      }
#pragma warning restore 618

      protected static IEnumerable<MetaDataColumnInformation<FileReference, RawFileReference>> GetFileColumns()
      {
         yield return MetaDataColumnInformation.Constant32<FileReference, RawFileReference, FileAttributes>( nameof( FileReference.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes, ( r, v ) => r.Attributes = (FileAttributes) v, i => (FileAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformation.SystemString<FileReference, RawFileReference>( nameof( FileReference.Name ), ( r, v ) => r.Name = v, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformation.BLOBByteArray<FileReference, RawFileReference>( nameof( FileReference.HashValue ), ( r, v ) => r.HashValue = v, row => row.HashValue, ( r, v ) => r.HashValue = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<ExportedType, RawExportedType>> GetExportedTypeColumns()
      {
         yield return MetaDataColumnInformation.Constant32<ExportedType, RawExportedType, TypeAttributes>( nameof( ExportedType.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes, ( r, v ) => r.Attributes = (TypeAttributes) v, i => (TypeAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformation.Number32<ExportedType, RawExportedType>( nameof( ExportedType.TypeDefinitionIndex ), ( r, v ) => r.TypeDefinitionIndex = v, row => row.TypeDefinitionIndex, ( r, v ) => r.TypeDefinitionIndex = v );
         yield return MetaDataColumnInformation.SystemString<ExportedType, RawExportedType>( nameof( ExportedType.Name ), ( r, v ) => r.Name = v, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformation.SystemString<ExportedType, RawExportedType>( nameof( ExportedType.Namespace ), ( r, v ) => r.Namespace = v, row => row.Namespace, ( r, v ) => r.Namespace = v );
         yield return MetaDataColumnInformation.CodedTableIndex<ExportedType, RawExportedType>( nameof( ExportedType.Implementation ), Implementation, ( r, v ) => r.Implementation = v, row => row.Implementation, ( r, v ) => r.Implementation = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<ManifestResource, RawManifestResource>> GetManifestResourceColumns()
      {
         yield return MetaDataColumnInformation.DataReferenceForStructs<ManifestResource, RawManifestResource, Int32>( nameof( ManifestResource.Offset ), ( r, v ) => r.Offset = v, r => r.Offset, ( r, v ) => r.Offset = v, ( args, offset ) =>
         {
            var row = args.Row;
            row.Offset = offset;
            if ( !row.Implementation.HasValue )
            {
               row.DataInCurrentFile = DefaultMetaDataSerializationSupportProvider.DeserializeEmbeddedManifest( args.RowArgs, offset );
            }
         },
         ( md, mdStreamContainer ) => new SectionPart_EmbeddedManifests( md ) );
         yield return MetaDataColumnInformation.Constant32<ManifestResource, RawManifestResource, ManifestResourceAttributes>( nameof( ManifestResource.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes, ( r, v ) => r.Attributes = (ManifestResourceAttributes) v, i => (ManifestResourceAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformation.SystemString<ManifestResource, RawManifestResource>( nameof( ManifestResource.Name ), ( r, v ) => r.Name = v, row => row.Name, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformation.CodedTableIndexNullable<ManifestResource, RawManifestResource>( nameof( ManifestResource.Implementation ), Implementation, ( r, v ) => r.Implementation = v, row => row.Implementation, ( r, v ) => r.Implementation = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<NestedClassDefinition, RawNestedClassDefinition>> GetNestedClassColumns()
      {
         yield return MetaDataColumnInformation.SimpleTableIndex<NestedClassDefinition, RawNestedClassDefinition>( nameof( NestedClassDefinition.NestedClass ), Tables.TypeDef, ( r, v ) => r.NestedClass = v, row => row.NestedClass, ( r, v ) => r.NestedClass = v );
         yield return MetaDataColumnInformation.SimpleTableIndex<NestedClassDefinition, RawNestedClassDefinition>( nameof( NestedClassDefinition.EnclosingClass ), Tables.TypeDef, ( r, v ) => r.EnclosingClass = v, row => row.EnclosingClass, ( r, v ) => r.EnclosingClass = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<GenericParameterDefinition, RawGenericParameterDefinition>> GetGenericParamColumns()
      {
         yield return MetaDataColumnInformation.Number32_SerializedAs16<GenericParameterDefinition, RawGenericParameterDefinition>( nameof( GenericParameterDefinition.GenericParameterIndex ), ( r, v ) => r.GenericParameterIndex = v, row => row.GenericParameterIndex, ( r, v ) => r.GenericParameterIndex = v );
         yield return MetaDataColumnInformation.Constant16<GenericParameterDefinition, RawGenericParameterDefinition, GenericParameterAttributes>( nameof( GenericParameterDefinition.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes, ( r, v ) => r.Attributes = (GenericParameterAttributes) v, i => (GenericParameterAttributes) i, v => (Int32) v );
         yield return MetaDataColumnInformation.CodedTableIndex<GenericParameterDefinition, RawGenericParameterDefinition>( nameof( GenericParameterDefinition.Owner ), TypeOrMethodDef, ( r, v ) => r.Owner = v, row => row.Owner, ( r, v ) => r.Owner = v );
         yield return MetaDataColumnInformation.SystemString<GenericParameterDefinition, RawGenericParameterDefinition>( nameof( GenericParameterDefinition.Name ), ( r, v ) => r.Name = v, row => row.Name, ( r, v ) => r.Name = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<MethodSpecification, RawMethodSpecification>> GetMethodSpecColumns()
      {
         yield return MetaDataColumnInformation.CodedTableIndex<MethodSpecification, RawMethodSpecification>( nameof( MethodSpecification.Method ), MethodDefOrRef, ( r, v ) => r.Method = v, row => row.Method, ( r, v ) => r.Method = v );
         yield return MetaDataColumnInformation.BLOBNonTypeSignature<MethodSpecification, RawMethodSpecification, GenericMethodSignature>( nameof( MethodSpecification.Signature ), ( r, v ) => r.Signature = v, row => row.Signature, ( r, v ) => r.Signature = v );
      }

      protected static IEnumerable<MetaDataColumnInformation<GenericParameterConstraintDefinition, RawGenericParameterConstraintDefinition>> GetGenericParamConstraintColumns()
      {
         yield return MetaDataColumnInformation.SimpleTableIndex<GenericParameterConstraintDefinition, RawGenericParameterConstraintDefinition>( nameof( GenericParameterConstraintDefinition.Owner ), Tables.GenericParameter, ( r, v ) => r.Owner = v, row => row.Owner, ( r, v ) => r.Owner = v );
         yield return MetaDataColumnInformation.CodedTableIndex<GenericParameterConstraintDefinition, RawGenericParameterConstraintDefinition>( nameof( GenericParameterConstraintDefinition.Constraint ), TypeDefOrRef, ( r, v ) => r.Constraint = v, row => row.Constraint, ( r, v ) => r.Constraint = v );
      }

   }

   public abstract class MetaDataTableInformation
   {
      internal MetaDataTableInformation(
         Tables tableKind,
         System.Collections.IEqualityComparer equalityComparer,
         System.Collections.IComparer comparer
         )
      {
         ArgumentValidator.ValidateNotNull( "Equality comparer", equalityComparer );

         this.TableKind = tableKind;
         this.EqualityComparerNotGeneric = equalityComparer;
         this.ComparerNotGeneric = comparer;

      }

      public Tables TableKind { get; }

      public System.Collections.IEqualityComparer EqualityComparerNotGeneric { get; }

      public System.Collections.IComparer ComparerNotGeneric { get; }

      public abstract ArrayQuery<MetaDataColumnInformation> ColumnsInformationNotGeneric { get; }

      public abstract MetaDataTable CreateMetaDataTableNotGeneric( Int32 capacity );

      public abstract Object CreateRowNotGeneric();

      public abstract TableSerializationInfo CreateTableSerializationInfoNotGeneric();
   }

   public abstract class MetaDataTableInformation<TRow> : MetaDataTableInformation
      where TRow : class
   {
      internal MetaDataTableInformation(
         Tables tableKind,
         IEqualityComparer<TRow> equalityComparer,
         IComparer<TRow> comparer,
         Func<TRow> rowFactory
         )
         : base( tableKind, new EqualityComparerWrapper<TRow>( equalityComparer ), comparer == null ? null : new ComparerWrapper<TRow>( comparer ) )
      {
         ArgumentValidator.ValidateNotNull( "Row factory", rowFactory );

         this.EqualityComparer = equalityComparer;
         this.Comparer = comparer;
         this.RowFactory = rowFactory;
      }

      public IEqualityComparer<TRow> EqualityComparer { get; }

      public IComparer<TRow> Comparer { get; }

      public MetaDataTable<TRow> CreateMetaDataTable( Int32 capacity )
      {
         return new Implementation.MetaDataTableImpl<TRow>( this, capacity );
      }

      public TRow CreateRow()
      {
         return this.RowFactory();
      }

      public override Object CreateRowNotGeneric()
      {
         return this.CreateRow();
      }

      public abstract ArrayQuery<MetaDataColumnInformation<TRow>> ColumnsInformation { get; }

      public override MetaDataTable CreateMetaDataTableNotGeneric( Int32 capacity )
      {
         return this.CreateMetaDataTable( capacity );
      }

      public override ArrayQuery<MetaDataColumnInformation> ColumnsInformationNotGeneric
      {
         get
         {
            return this.ColumnsInformation;
         }
      }

      protected Func<TRow> RowFactory { get; }
   }

   public sealed class MetaDataTableInformation<TRow, TRawRow> : MetaDataTableInformation<TRow>
      where TRow : class
      where TRawRow : class
   {
      private readonly Func<TRawRow> _rawRowFactory;

      public MetaDataTableInformation(
         Tables tableKind,
         IEqualityComparer<TRow> equalityComparer,
         IComparer<TRow> comparer,
         IEnumerable<MetaDataColumnInformation<TRow, TRawRow>> columns,
         Func<TRow> rowFactory,
         Func<TRawRow> rawRowFactory
         )
         : base( tableKind, equalityComparer, comparer, rowFactory )
      {

         ArgumentValidator.ValidateNotNull( "Columns", columns );
         ArgumentValidator.ValidateNotNull( "Raw row factory", rawRowFactory );

         this._rawRowFactory = rawRowFactory;
         this.ColumnsInformationWithRawType = columns.ToArrayProxy().CQ;

         if ( this.ColumnsInformationWithRawType.Count <= 0 )
         {
            throw new ArgumentException( "Table must have at least one column." );
         }
      }

      public ArrayQuery<MetaDataColumnInformation<TRow, TRawRow>> ColumnsInformationWithRawType { get; }

      public TRawRow CreateRawRow()
      {
         return this._rawRowFactory();
      }

      public override ArrayQuery<MetaDataColumnInformation<TRow>> ColumnsInformation
      {
         get
         {
            return this.ColumnsInformationWithRawType;
         }
      }

      public DefaultTableSerializationInfo<TRawRow, TRow> CreateTableSerializationInfo()
      {
         return new DefaultTableSerializationInfo<TRawRow, TRow>(
            this.TableKind,
            this.ColumnsInformationWithRawType.Select( c => c.DefaultColumnSerializationInfoWithRawType ),
            this.RowFactory,
            this._rawRowFactory
            );
      }

      public override TableSerializationInfo CreateTableSerializationInfoNotGeneric()
      {
         return this.CreateTableSerializationInfo();
      }
   }

   public abstract class MetaDataColumnInformation
   {
      internal MetaDataColumnInformation(
         MetaDataColumnDataInformation information
         )
      {
         ArgumentValidator.ValidateNotNull( "Information", information );

         this.DataInformation = information;
      }

      public MetaDataColumnDataInformation DataInformation { get; }

      public abstract Object GetterNotGeneric( Object row );

      public abstract void SetterNotGeneric( Object row, Object value );

      public abstract Type RowType { get; }

      public abstract Type RawRowType { get; }

      public abstract Type ValueType { get; }

      public abstract DefaultColumnSerializationInfo DefaultColumnSerializationInfoNotGeneric { get; }

      public static MetaDataColumnInformationForStructs<TRow, TRawRow, TValue> Constant8<TRow, TRawRow, TValue>(
         String columnName,
         Action<TRow, TValue> setter,
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
            columnName,
            sizeof( Byte ),
            getter,
            setter,
            () => DefaultColumnSerializationInfoFactory.Constant8<TRawRow, TRow>( columnName, rawSetter, ( args, v ) => setter( args.Row, fromInteger( v ) ), r => toInteger( getter( r ) ) )
            );
      }

      public static MetaDataColumnInformationForStructs<TRow, TRawRow, TValue> Constant16<TRow, TRawRow, TValue>(
         String columnName,
         Action<TRow, TValue> setter,
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
            columnName,
            sizeof( Int16 ),
            getter,
            setter,
            () => DefaultColumnSerializationInfoFactory.Constant16<TRawRow, TRow>( columnName, rawSetter, ( args, v ) => setter( args.Row, fromInteger( v ) ), r => toInteger( getter( r ) ) )
            );
      }

      public static MetaDataColumnInformationForStructs<TRow, TRawRow, TValue> Constant32<TRow, TRawRow, TValue>(
         String columnName,
         Action<TRow, TValue> setter,
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
            columnName,
            sizeof( Int32 ),
            getter,
            setter,
            () => DefaultColumnSerializationInfoFactory.Constant32<TRawRow, TRow>( columnName, rawSetter, ( args, v ) => setter( args.Row, fromInteger( v ) ), r => toInteger( getter( r ) ) )
            );
      }

      public static MetaDataColumnInformationForStructs<TRow, TRawRow, Byte> Number8<TRow, TRawRow>(
         String columnName,
         Action<TRow, Byte> setter,
         RowColumnGetterDelegate<TRow, Byte> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return ConstantCustom<TRow, TRawRow, Byte>(
            columnName,
            sizeof( Byte ),
            getter,
            setter,
            () => DefaultColumnSerializationInfoFactory.Constant8<TRawRow, TRow>( columnName, rawSetter, ( args, v ) => setter( args.Row, (Byte) v ), r => getter( r ) )
            );
      }

      public static MetaDataColumnInformationForStructs<TRow, TRawRow, Int16> Number16<TRow, TRawRow>(
         String columnName,
         Action<TRow, Int16> setter,
         RowColumnGetterDelegate<TRow, Int16> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return ConstantCustom<TRow, TRawRow, Int16>(
            columnName,
            sizeof( Int16 ),
            getter,
            setter,
            () => DefaultColumnSerializationInfoFactory.Constant16<TRawRow, TRow>( columnName, rawSetter, ( args, v ) => setter( args.Row, (Int16) v ), r => getter( r ) )
            );
      }

      public static MetaDataColumnInformationForStructs<TRow, TRawRow, Int32> Number32_SerializedAs16<TRow, TRawRow>(
         String columnName,
         Action<TRow, Int32> setter,
         RowColumnGetterDelegate<TRow, Int32> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return ConstantCustom<TRow, TRawRow, Int32>(
            columnName,
            sizeof( Int16 ),
            getter,
            setter,
            () => DefaultColumnSerializationInfoFactory.Constant16<TRawRow, TRow>( columnName, rawSetter, ( args, v ) => setter( args.Row, v ), r => getter( r ) )
            );
      }

      public static MetaDataColumnInformationForStructs<TRow, TRawRow, Int32> Number32<TRow, TRawRow>(
         String columnName,
         Action<TRow, Int32> setter,
         RowColumnGetterDelegate<TRow, Int32> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return ConstantCustom<TRow, TRawRow, Int32>(
            columnName,
            sizeof( int ),
            getter,
            setter,
            () => DefaultColumnSerializationInfoFactory.Constant32<TRawRow, TRow>( columnName, rawSetter, ( args, v ) => setter( args.Row, v ), r => getter( r ) )
            );
      }

      public static MetaDataColumnInformationForStructs<TRow, TRawRow, TValue> ConstantCustom<TRow, TRawRow, TValue>(
         String columnName,
         Int32 byteCount,
         RowColumnGetterDelegate<TRow, TValue> getter,
         Action<TRow, TValue> setter,
         Func<DefaultColumnSerializationInfo<TRawRow, TRow>> serializationInfo
         )
         where TRow : class
         where TRawRow : class
         where TValue : struct
      {
         return new MetaDataColumnInformationForStructs<TRow, TRawRow, TValue>( new MetaDataColumnDataInformation_FixedSizeConstant( columnName, byteCount ), serializationInfo, getter, setter );
      }

      public static MetaDataColumnInformationForClasses<TRow, TRawRow, String> SystemString<TRow, TRawRow>(
         String columnName,
         Action<TRow, String> setter,
         RowColumnGetterDelegate<TRow, String> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return new MetaDataColumnInformationForClasses<TRow, TRawRow, String>( new MetaDataColumnDataInformation_HeapIndex( columnName, MetaDataConstants.SYS_STRING_STREAM_NAME ), () => DefaultColumnSerializationInfoFactory.SystemString<TRawRow, TRow>( columnName, rawSetter, ( args, v ) => setter( args.Row, v ), getter ), getter, setter );
      }

      public static MetaDataColumnInformationForNullables<TRow, TRawRow, Guid> GUID<TRow, TRawRow>(
         String columnName,
         Action<TRow, Guid?> setter,
         RowColumnGetterDelegate<TRow, Guid?> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return new MetaDataColumnInformationForNullables<TRow, TRawRow, Guid>( new MetaDataColumnDataInformation_HeapIndex( columnName, MetaDataConstants.GUID_STREAM_NAME ), () => DefaultColumnSerializationInfoFactory.GUID<TRawRow, TRow>( columnName, rawSetter, ( args, v ) => setter( args.Row, v ), getter ), getter, setter );
      }

      public static MetaDataColumnInformationForStructs<TRow, TRawRow, TableIndex> SimpleTableIndex<TRow, TRawRow>(
         String columnName,
         Tables targetTable,
         Action<TRow, TableIndex> setter,
         RowColumnGetterDelegate<TRow, TableIndex> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return new MetaDataColumnInformationForStructs<TRow, TRawRow, TableIndex>( new MetaDataColumnDataInformation_SimpleTableIndex( columnName, targetTable ), () => DefaultColumnSerializationInfoFactory.SimpleReference<TRawRow, TRow>( columnName, targetTable, rawSetter, ( args, v ) => setter( args.Row, v ), getter ), getter, setter );
      }

      public static MetaDataColumnInformationForStructs<TRow, TRawRow, TableIndex> CodedTableIndex<TRow, TRawRow>(
         String columnName,
         ArrayQuery<Tables?> targetTables,
         Action<TRow, TableIndex> setter,
         RowColumnGetterDelegate<TRow, TableIndex> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return new MetaDataColumnInformationForStructs<TRow, TRawRow, TableIndex>( new MetaDataColumnDataInformation_CodedTableIndex( columnName, targetTables ), () => DefaultColumnSerializationInfoFactory.CodedReference<TRawRow, TRow>( columnName, targetTables, rawSetter, ( args, v ) => setter( args.Row, v.GetValueOrDefault() ), r => getter( r ) ), getter, setter );
      }

      public static MetaDataColumnInformationForNullables<TRow, TRawRow, TableIndex> CodedTableIndexNullable<TRow, TRawRow>(
         String columnName,
         ArrayQuery<Tables?> targetTables,
         Action<TRow, TableIndex?> setter,
         RowColumnGetterDelegate<TRow, TableIndex?> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return new MetaDataColumnInformationForNullables<TRow, TRawRow, TableIndex>( new MetaDataColumnDataInformation_CodedTableIndex( columnName, targetTables ), () => DefaultColumnSerializationInfoFactory.CodedReference<TRawRow, TRow>( columnName, targetTables, rawSetter, ( args, v ) => setter( args.Row, v ), getter ), getter, setter );
      }

      public static MetaDataColumnInformationForClasses<TRow, TRawRow, TypeSignature> BLOBTypeSignature<TRow, TRawRow>(
         String columnName,
         Action<TRow, TypeSignature> setter,
         RowColumnGetterDelegate<TRow, TypeSignature> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return BLOBCustom(
            columnName,
            setter,
            getter,
            rawSetter,
            ( args, value, blobs ) => setter( args.Row, blobs.ReadTypeSignature( value ) ),
            args => args.RowArgs.Array.CreateAnySignature( getter( args.Row ) )
            );
      }

      public static MetaDataColumnInformationForClasses<TRow, TRawRow, TSignature> BLOBNonTypeSignature<TRow, TRawRow, TSignature>(
         String columnName,
         Action<TRow, TSignature> setter,
         RowColumnGetterDelegate<TRow, TSignature> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
         where TSignature : AbstractSignature
      {
         var isMethodDef = Equals( typeof( MethodDefinitionSignature ), typeof( TSignature ) );
         return BLOBCustom(
            columnName,
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

      public static MetaDataColumnInformationForClasses<TRow, TRawRow, Byte[]> BLOBByteArray<TRow, TRawRow>(
         String columnName,
         Action<TRow, Byte[]> setter,
         RowColumnGetterDelegate<TRow, Byte[]> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return BLOBCustom(
            columnName,
            setter,
            getter,
            rawSetter,
            ( args, heapIndex, blobs ) => setter( args.Row, blobs.GetBLOB( heapIndex ) ),
            args => getter( args.Row )
            );
      }

      public static MetaDataColumnInformationForClasses<TRow, TRawRow, TValue> BLOBCustom<TRow, TRawRow, TValue>(
         String columnName,
         Action<TRow, TValue> setter,
         RowColumnGetterDelegate<TRow, TValue> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         Action<ColumnFunctionalityArgs<TRow, RowReadingArguments>, Int32, ReaderBLOBStreamHandler> blobReader, // TODO delegat-ize these
         Func<ColumnFunctionalityArgs<TRow, RowHeapFillingArguments>, Byte[]> blobCreator
         )
         where TRow : class
         where TRawRow : class
         where TValue : class
      {
         return new MetaDataColumnInformationForClasses<TRow, TRawRow, TValue>( new MetaDataColumnDataInformation_HeapIndex( columnName, MetaDataConstants.BLOB_STREAM_NAME ), () => DefaultColumnSerializationInfoFactory.BLOBCustom<TRawRow, TRow>( columnName, rawSetter, blobReader, blobCreator ), getter, setter );
      }

      public static MetaDataColumnInformationForClasses<TRow, TRawRow, TValue> DataReferenceForClasses<TRow, TRawRow, TValue>(
         String columnName,
         Action<TRow, TValue> setter,
         RowColumnGetterDelegate<TRow, TValue> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowRawColumnSetterDelegate<TRow> rawValueProcessor,
         RawColumnSectionPartCreationDelegte<TRow> rawColummnSectionPartCreator
         )
         where TRow : class
         where TRawRow : class
         where TValue : class
      {
         return new MetaDataColumnInformationForClasses<TRow, TRawRow, TValue>( new MetaDataColumnDataInformation_DataReference( columnName ), () => DefaultColumnSerializationInfoFactory.RawValueStorageColumn<TRawRow, TRow>( columnName, rawSetter, rawValueProcessor, rawColummnSectionPartCreator ), getter, setter );
      }

      public static MetaDataColumnInformationForStructs<TRow, TRawRow, TValue> DataReferenceForStructs<TRow, TRawRow, TValue>(
         String columnName,
         Action<TRow, TValue> setter,
         RowColumnGetterDelegate<TRow, TValue> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowRawColumnSetterDelegate<TRow> rawValueProcessor,
         RawColumnSectionPartCreationDelegte<TRow> rawColummnSectionPartCreator
         )
         where TRow : class
         where TRawRow : class
         where TValue : struct
      {
         return new MetaDataColumnInformationForStructs<TRow, TRawRow, TValue>( new MetaDataColumnDataInformation_DataReference( columnName ), () => DefaultColumnSerializationInfoFactory.RawValueStorageColumn<TRawRow, TRow>( columnName, rawSetter, rawValueProcessor, rawColummnSectionPartCreator ), getter, setter );
      }

   }

   public abstract class MetaDataColumnInformation<TRow> : MetaDataColumnInformation
      where TRow : class
   {


      internal MetaDataColumnInformation(
         MetaDataColumnDataInformation information
         )
         : base( information )
      {
      }

      public abstract Object Getter( TRow row );

      public abstract void Setter( TRow row, Object value );

      public override Object GetterNotGeneric( Object row )
      {
         return this.Getter( row as TRow );
      }

      public override void SetterNotGeneric( Object row, Object value )
      {
         this.Setter( row as TRow, value );
      }

      public abstract DefaultColumnSerializationInfo<TRow> DefaultColumnSerializationInfo { get; }

      public override DefaultColumnSerializationInfo DefaultColumnSerializationInfoNotGeneric
      {
         get
         {
            return this.DefaultColumnSerializationInfo;
         }
      }


      public override Type RowType
      {
         get
         {
            return typeof( TRow );
         }
      }
   }

   public abstract class MetaDataColumnInformation<TRow, TRawRow> : MetaDataColumnInformation<TRow>
      where TRow : class
      where TRawRow : class
   {

      private readonly Lazy<DefaultColumnSerializationInfo<TRawRow, TRow>> _serializationInfo;

      internal MetaDataColumnInformation(
         MetaDataColumnDataInformation information,
         Func<DefaultColumnSerializationInfo<TRawRow, TRow>> defaultSerializationInfo
         )
         : base( information )
      {
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

      public override DefaultColumnSerializationInfo<TRow> DefaultColumnSerializationInfo
      {
         get
         {
            return this.DefaultColumnSerializationInfoWithRawType;
         }
      }

      public override Type RawRowType
      {
         get
         {
            return typeof( TRawRow );
         }
      }
   }

   public sealed class MetaDataColumnInformationForClasses<TRow, TRawRow, TValue> : MetaDataColumnInformation<TRow, TRawRow>
      where TRow : class
      where TRawRow : class
      where TValue : class
   {

      private readonly RowColumnGetterDelegate<TRow, TValue> _getter;
      private readonly Action<TRow, TValue> _setter;

      public MetaDataColumnInformationForClasses(
         MetaDataColumnDataInformation information,
         Func<DefaultColumnSerializationInfo<TRawRow, TRow>> defaultSerializationInfo,
         RowColumnGetterDelegate<TRow, TValue> getter,
         Action<TRow, TValue> setter
         )
         : base( information, defaultSerializationInfo )
      {
         ArgumentValidator.ValidateNotNull( "Column value getter", getter );
         ArgumentValidator.ValidateNotNull( "Column value setter", setter );

         this._getter = getter;
         this._setter = setter;
      }

      public override Object Getter( TRow row )
      {
         return row == null ? null : this._getter( row );
      }

      public override void Setter( TRow row, Object value )
      {
         if ( row != null )
         {
            this._setter( row, value as TValue );
         }
      }

      public override Type ValueType
      {
         get
         {
            return typeof( TValue );
         }
      }
   }

   public sealed class MetaDataColumnInformationForStructs<TRow, TRawRow, TValue> : MetaDataColumnInformation<TRow, TRawRow>
      where TRow : class
      where TRawRow : class
      where TValue : struct
   {

      private readonly RowColumnGetterDelegate<TRow, TValue> _getter;
      private readonly Action<TRow, TValue> _setter;

      public MetaDataColumnInformationForStructs(
         MetaDataColumnDataInformation information,
         Func<DefaultColumnSerializationInfo<TRawRow, TRow>> defaultSerializationInfo,
         RowColumnGetterDelegate<TRow, TValue> getter,
         Action<TRow, TValue> setter
         )
         : base( information, defaultSerializationInfo )
      {
         ArgumentValidator.ValidateNotNull( "Column value getter", getter );
         ArgumentValidator.ValidateNotNull( "Column value setter", setter );

         this._getter = getter;
         this._setter = setter;
      }

      public override Object Getter( TRow row )
      {
         return row == null ? null : (Object) this._getter( row );
      }

      public override void Setter( TRow row, Object value )
      {
         if ( row != null && value is TValue )
         {
            this._setter( row, (TValue) value );
         }
      }

      public override Type ValueType
      {
         get
         {
            return typeof( TValue );
         }
      }
   }

   public sealed class MetaDataColumnInformationForNullables<TRow, TRawRow, TValue> : MetaDataColumnInformation<TRow, TRawRow>
      where TRow : class
      where TRawRow : class
      where TValue : struct
   {

      private readonly RowColumnGetterDelegate<TRow, TValue?> _getter;
      private readonly Action<TRow, TValue?> _setter;

      public MetaDataColumnInformationForNullables(
         MetaDataColumnDataInformation information,
         Func<DefaultColumnSerializationInfo<TRawRow, TRow>> defaultSerializationInfo,
         RowColumnGetterDelegate<TRow, TValue?> getter,
         Action<TRow, TValue?> setter
         )
         : base( information, defaultSerializationInfo )
      {
         ArgumentValidator.ValidateNotNull( "Column value getter", getter );
         ArgumentValidator.ValidateNotNull( "Column value setter", setter );

         this._getter = getter;
         this._setter = setter;
      }

      public override Object Getter( TRow row )
      {
         return row == null ? null : (Object) this._getter( row );
      }

      public override void Setter( TRow row, Object value )
      {
         // https://msdn.microsoft.com/en-us/library/ms366789.aspx
         // "is X" returns true when something is of type X?
         if ( row != null )
         {
            TValue? val = value == null ? null : (TValue?) value;
            this._setter( row, val );
         }
      }

      public override Type ValueType
      {
         get
         {
            return typeof( TValue? );
         }
      }
   }

   public abstract class MetaDataColumnDataInformation
   {
      internal MetaDataColumnDataInformation(
         String columnName
         )
      {
         ArgumentValidator.ValidateNotEmpty( "Column name", columnName );

         this.Name = columnName;
      }

      public String Name { get; }

      public abstract MetaDataColumnInformationKind ColumnKind { get; }
   }

   public sealed class MetaDataColumnDataInformation_FixedSizeConstant : MetaDataColumnDataInformation
   {
      internal MetaDataColumnDataInformation_FixedSizeConstant(
         String columnName,
         Int32 byteSize
         )
         : base( columnName )
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
         String columnName,
         Tables targetTable
         )
         : base( columnName )
      {
         this.TargetTable = targetTable;
      }

      public Tables TargetTable { get; }

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
         String columnName,
         IEnumerable<Tables?> targetTables
         )
         : base( columnName )
      {

         this.TargetTables = ( targetTables ?? Empty<Tables?>.Enumerable ).ToArrayProxy().CQ;
      }

      public ArrayQuery<Tables?> TargetTables { get; }

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
         String columnName,
         String heapName
         )
         : base( columnName )
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
         String columnName
         )
         : base( columnName )
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
