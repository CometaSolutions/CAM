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

namespace CILAssemblyManipulator.Physical.Meta
{
   public interface MetaDataTableInformationProvider
   {
      MetaDataTableInformation GetTableInformation( Tables table );
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

      //public void SetTableInformation( MetaDataTableInformation tableInfo )
      //{
      //   if ( tableInfo != null )
      //   {
      //      this._infos[(Int32) tableInfo.TableKind] = tableInfo;
      //   }
      //}

      protected static IEnumerable<MetaDataTableInformation> CreateDefaultTableInformation()
      {
         yield return new MetaDataTableInformation<ModuleDefinition>(
            Tables.Module,
            Comparers.ModuleDefinitionEqualityComparer,
            null,
            GetModuleDefColumns()
            );

         yield return new MetaDataTableInformation<TypeReference>(
            Tables.TypeRef,
            Comparers.TypeReferenceEqualityComparer,
            null,
            GetTypeRefColumns()
            );

         yield return new MetaDataTableInformation<TypeDefinition>(
            Tables.TypeDef,
            Comparers.TypeDefinitionEqualityComparer,
            null,
            GetTypeDefColumns()
            );

         yield return new MetaDataTableInformation<FieldDefinitionPointer>(
            Tables.FieldPtr,
            Comparers.FieldDefinitionPointerEqualityComparer,
            null,
            GetFieldPtrColumns()
            );

         yield return new MetaDataTableInformation<FieldDefinition>(
            Tables.Field,
            Comparers.FieldDefinitionEqualityComparer,
            null,
            GetFieldDefColumns()
            );

         yield return new MetaDataTableInformation<MethodDefinitionPointer>(
            Tables.MethodPtr,
            Comparers.MethodDefinitionPointerEqualityComparer,
            null,
            GetMethodPtrColumns()
            );

         yield return new MetaDataTableInformation<MethodDefinition>(
            Tables.MethodDef,
            Comparers.MethodDefinitionEqualityComparer,
            null,
            GetMethodDefColumns()
            );

         yield return new MetaDataTableInformation<ParameterDefinitionPointer>(
            Tables.ParameterPtr,
            Comparers.ParameterDefinitionPointerEqualityComparer,
            null,
            GetParamPtrColumns()
            );

         yield return new MetaDataTableInformation<ParameterDefinition>(
            Tables.Parameter,
            Comparers.ParameterDefinitionEqualityComparer,
            null,
            GetParamColumns()
            );

         yield return new MetaDataTableInformation<InterfaceImplementation>(
            Tables.InterfaceImpl,
            Comparers.InterfaceImplementationEqualityComparer,
            Comparers.InterfaceImplementationComparer,
            GetInterfaceImplColumns()
            );

         yield return new MetaDataTableInformation<MemberReference>(
            Tables.MemberRef,
            Comparers.MemberReferenceEqualityComparer,
            null,
            GetMemberRefColumns()
            );

         yield return new MetaDataTableInformation<ConstantDefinition>(
            Tables.Constant,
            Comparers.ConstantDefinitionEqualityComparer,
            Comparers.ConstantDefinitionComparer,
            GetConstantColumns()
            );

         yield return new MetaDataTableInformation<CustomAttributeDefinition>(
            Tables.CustomAttribute,
            Comparers.CustomAttributeDefinitionEqualityComparer,
            Comparers.CustomAttributeDefinitionComparer,
            GetCustomAttributeColumns()
            );

         yield return new MetaDataTableInformation<FieldMarshal>(
            Tables.FieldMarshal,
            Comparers.FieldMarshalEqualityComparer,
            Comparers.FieldMarshalComparer,
            GetFieldMarshalColumns()
            );

         yield return new MetaDataTableInformation<SecurityDefinition>(
            Tables.DeclSecurity,
            Comparers.SecurityDefinitionEqualityComparer,
            Comparers.SecurityDefinitionComparer,
            GetDeclSecurityColumns()
            );

         yield return new MetaDataTableInformation<ClassLayout>(
            Tables.ClassLayout,
            Comparers.ClassLayoutEqualityComparer,
            Comparers.ClassLayoutComparer,
            GetClassLayoutColumns()
            );

         yield return new MetaDataTableInformation<FieldLayout>(
            Tables.FieldLayout,
            Comparers.FieldLayoutEqualityComparer,
            Comparers.FieldLayoutComparer,
            GetFieldLayoutColumns()
            );

         yield return new MetaDataTableInformation<StandaloneSignature>(
            Tables.StandaloneSignature,
            Comparers.StandaloneSignatureEqualityComparer,
            null,
            GetStandaloneSigColumns()
            );

         yield return new MetaDataTableInformation<EventMap>(
            Tables.EventMap,
            Comparers.EventMapEqualityComparer,
            null,
            GetEventMapColumns()
            );

         yield return new MetaDataTableInformation<EventDefinitionPointer>(
            Tables.EventPtr,
            Comparers.EventDefinitionPointerEqualityComparer,
            null,
            GetEventPtrColumns()
            );

         yield return new MetaDataTableInformation<EventDefinition>(
            Tables.Event,
            Comparers.EventDefinitionEqualityComparer,
            null,
            GetEventDefColumns()
            );

         yield return new MetaDataTableInformation<PropertyMap>(
            Tables.PropertyMap,
            Comparers.PropertyMapEqualityComparer,
            null,
            GetPropertyMapColumns()
            );

         yield return new MetaDataTableInformation<PropertyDefinitionPointer>(
            Tables.PropertyPtr,
            Comparers.PropertyDefinitionPointerEqualityComparer,
            null,
            GetPropertyPtrColumns()
            );

         yield return new MetaDataTableInformation<PropertyDefinition>(
            Tables.Property,
            Comparers.PropertyDefinitionEqualityComparer,
            null,
            GetPropertyDefColumns()
            );

         yield return new MetaDataTableInformation<MethodSemantics>(
            Tables.MethodSemantics,
            Comparers.MethodSemanticsEqualityComparer,
            Comparers.MethodSemanticsComparer,
            GetMethodSemanticsColumns()
            );

         yield return new MetaDataTableInformation<MethodImplementation>(
            Tables.MethodImpl,
            Comparers.MethodImplementationEqualityComparer,
            Comparers.MethodImplementationComparer,
            GetMethodImplColumns()
            );

         yield return new MetaDataTableInformation<ModuleReference>(
            Tables.ModuleRef,
            Comparers.ModuleReferenceEqualityComparer,
            null,
            GetModuleRefColumns()
            );

         yield return new MetaDataTableInformation<TypeSpecification>(
            Tables.TypeSpec,
            Comparers.TypeSpecificationEqualityComparer,
            null,
            GetTypeSpecColumns()
            );

         yield return new MetaDataTableInformation<MethodImplementationMap>(
            Tables.ImplMap,
            Comparers.MethodImplementationMapEqualityComparer,
            Comparers.MethodImplementationMapComparer,
            GetImplMapColumns()
            );

         yield return new MetaDataTableInformation<FieldRVA>(
            Tables.FieldRVA,
            Comparers.FieldRVAEqualityComparer,
            Comparers.FieldRVAComparer,
            GetFieldRVAColumns()
            );

         yield return new MetaDataTableInformation<EditAndContinueLog>(
            Tables.EncLog,
            Comparers.EditAndContinueLogEqualityComparer,
            null,
            GetENCLogColumns()
            );

         yield return new MetaDataTableInformation<EditAndContinueMap>(
            Tables.EncMap,
            Comparers.EditAndContinueMapEqualityComparer,
            null,
            GetENCMapColumns()
            );

         yield return new MetaDataTableInformation<AssemblyDefinition>(
            Tables.Assembly,
            Comparers.AssemblyDefinitionEqualityComparer,
            null,
            GetAssemblyDefColumns()
            );

#pragma warning disable 618

         yield return new MetaDataTableInformation<AssemblyDefinitionProcessor>(
            Tables.AssemblyProcessor,
            Comparers.AssemblyDefinitionProcessorEqualityComparer,
            null,
            GetAssemblyDefProcessorColumns()
            );

         yield return new MetaDataTableInformation<AssemblyDefinitionOS>(
            Tables.AssemblyOS,
            Comparers.AssemblyDefinitionOSEqualityComparer,
            null,
            GetAssemblyDefOSColumns()
            );

#pragma warning restore 618

         yield return new MetaDataTableInformation<AssemblyReference>(
            Tables.AssemblyRef,
            Comparers.AssemblyReferenceEqualityComparer,
            null,
            GetAssemblyRefColumns()
            );

#pragma warning disable 618

         yield return new MetaDataTableInformation<AssemblyReferenceProcessor>(
            Tables.AssemblyRefProcessor,
            Comparers.AssemblyReferenceProcessorEqualityComparer,
            null,
            GetAssemblyRefProcessorColumns()
            );

         yield return new MetaDataTableInformation<AssemblyReferenceOS>(
            Tables.AssemblyRefOS,
            Comparers.AssemblyReferenceOSEqualityComparer,
            null,
            GetAssemblyRefOSColumns()
            );

#pragma warning restore 618

         yield return new MetaDataTableInformation<FileReference>(
            Tables.File,
            Comparers.FileReferenceEqualityComparer,
            null,
            GetFileColumns()
            );

         yield return new MetaDataTableInformation<ExportedType>(
            Tables.ExportedType,
            Comparers.ExportedTypeEqualityComparer,
            null,
            GetExportedTypeColumns()
            );

         yield return new MetaDataTableInformation<ManifestResource>(
            Tables.ManifestResource,
            Comparers.ManifestResourceEqualityComparer,
            null,
            GetManifestResourceColumns()
            );

         yield return new MetaDataTableInformation<NestedClassDefinition>(
            Tables.NestedClass,
            Comparers.NestedClassDefinitionEqualityComparer,
            Comparers.NestedClassDefinitionComparer,
            GetNestedClassColumns()
            );

         yield return new MetaDataTableInformation<GenericParameterDefinition>(
            Tables.GenericParameter,
            Comparers.GenericParameterDefinitionEqualityComparer,
            Comparers.GenericParameterDefinitionComparer,
            GetGenericParamColumns()
            );

         yield return new MetaDataTableInformation<MethodSpecification>(
            Tables.MethodSpec,
            Comparers.MethodSpecificationEqualityComparer,
            null,
            GetMethodSpecColumns()
            );

         yield return new MetaDataTableInformation<GenericParameterConstraintDefinition>(
            Tables.GenericParameterConstraint,
            Comparers.GenericParameterConstraintDefinitionEqualityComparer,
            Comparers.GenericParameterConstraintDefinitionComparer,
            GetGenericParamConstraintColumns()
            );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<ModuleDefinition>> GetModuleDefColumns()
      {
         yield return MetaDataColumnFunctionality.Constant16<ModuleDefinition, Int16>( nameof( ModuleDefinition.Generation ), ( r, v ) => r.Generation = v, row => row.Generation );
         yield return MetaDataColumnFunctionality.SystemString<ModuleDefinition>( nameof( ModuleDefinition.Name ), ( r, v ) => r.Name = v, row => row.Name );
         yield return MetaDataColumnFunctionality.GUID<ModuleDefinition>( nameof( ModuleDefinition.ModuleGUID ), ( r, v ) => r.ModuleGUID = v, row => row.ModuleGUID );
         yield return MetaDataColumnFunctionality.GUID<ModuleDefinition>( nameof( ModuleDefinition.EditAndContinueGUID ), ( r, v ) => r.EditAndContinueGUID = v, row => row.EditAndContinueGUID );
         yield return MetaDataColumnFunctionality.GUID<ModuleDefinition>( nameof( ModuleDefinition.EditAndContinueBaseGUID ), ( r, v ) => r.EditAndContinueBaseGUID = v, row => row.EditAndContinueBaseGUID );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<TypeReference>> GetTypeRefColumns()
      {
         yield return MetaDataColumnFunctionality.CodedTableIndexNullable<TypeReference>( nameof( TypeReference.ResolutionScope ), ResolutionScope, ( r, v ) => r.ResolutionScope = v, row => row.ResolutionScope );
         yield return MetaDataColumnFunctionality.SystemString<TypeReference>( nameof( TypeReference.Name ), ( r, v ) => r.Name = v, row => row.Name );
         yield return MetaDataColumnFunctionality.SystemString<TypeReference>( nameof( TypeReference.Namespace ), ( r, v ) => r.Namespace = v, row => row.Namespace );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<TypeDefinition>> GetTypeDefColumns()
      {
         yield return MetaDataColumnFunctionality.Constant32<TypeDefinition, TypeAttributes>( nameof( TypeDefinition.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes );
         yield return MetaDataColumnFunctionality.SystemString<TypeDefinition>( nameof( TypeDefinition.Name ), ( r, v ) => r.Name = v, row => row.Name );
         yield return MetaDataColumnFunctionality.SystemString<TypeDefinition>( nameof( TypeDefinition.Namespace ), ( r, v ) => r.Namespace = v, row => row.Namespace );
         yield return MetaDataColumnFunctionality.CodedTableIndexNullable<TypeDefinition>( nameof( TypeDefinition.BaseType ), TypeDefOrRef, ( r, v ) => r.BaseType = v, row => row.BaseType );
         yield return MetaDataColumnFunctionality.SimpleTableIndex<TypeDefinition>( nameof( TypeDefinition.FieldList ), Tables.Field, ( r, v ) => r.FieldList = v, row => row.FieldList );
         yield return MetaDataColumnFunctionality.SimpleTableIndex<TypeDefinition>( nameof( TypeDefinition.MethodList ), Tables.MethodDef, ( r, v ) => r.MethodList = v, row => row.MethodList );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<FieldDefinitionPointer>> GetFieldPtrColumns()
      {
         yield return MetaDataColumnFunctionality.SimpleTableIndex<FieldDefinitionPointer>( nameof( FieldDefinitionPointer.FieldIndex ), Tables.Field, ( r, v ) => r.FieldIndex = v, row => row.FieldIndex );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<FieldDefinition>> GetFieldDefColumns()
      {
         yield return MetaDataColumnFunctionality.Constant16<FieldDefinition, FieldAttributes>( nameof( FieldDefinition.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes );
         yield return MetaDataColumnFunctionality.SystemString<FieldDefinition>( nameof( FieldDefinition.Name ), ( r, v ) => r.Name = v, row => row.Name );
         yield return MetaDataColumnFunctionality.BLOB<FieldDefinition, FieldSignature>( nameof( FieldDefinition.Signature ), ( r, v ) => r.Signature = v, row => row.Signature );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<MethodDefinitionPointer>> GetMethodPtrColumns()
      {
         yield return MetaDataColumnFunctionality.SimpleTableIndex<MethodDefinitionPointer>( nameof( MethodDefinitionPointer.MethodIndex ), Tables.MethodDef, ( r, v ) => r.MethodIndex = v, row => row.MethodIndex );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<MethodDefinition>> GetMethodDefColumns()
      {
         yield return MetaDataColumnFunctionality.DataReferenceForClasses<MethodDefinition, MethodILDefinition>( nameof( MethodDefinition.IL ), ( r, v ) => r.IL = v, r => r.IL );
         yield return MetaDataColumnFunctionality.Constant16<MethodDefinition, MethodImplAttributes>( nameof( MethodDefinition.ImplementationAttributes ), ( r, v ) => r.ImplementationAttributes = v, row => row.ImplementationAttributes );
         yield return MetaDataColumnFunctionality.Constant16<MethodDefinition, MethodAttributes>( nameof( MethodDefinition.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes );
         yield return MetaDataColumnFunctionality.SystemString<MethodDefinition>( nameof( MethodDefinition.Name ), ( r, v ) => r.Name = v, row => row.Name );
         yield return MetaDataColumnFunctionality.BLOB<MethodDefinition, MethodDefinitionSignature>( nameof( MethodDefinition.Signature ), ( r, v ) => r.Signature = v, row => row.Signature );
         yield return MetaDataColumnFunctionality.SimpleTableIndex<MethodDefinition>( nameof( MethodDefinition.ParameterList ), Tables.Parameter, ( r, v ) => r.ParameterList = v, row => row.ParameterList );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<ParameterDefinitionPointer>> GetParamPtrColumns()
      {
         yield return MetaDataColumnFunctionality.SimpleTableIndex<ParameterDefinitionPointer>( nameof( ParameterDefinitionPointer.ParameterIndex ), Tables.Parameter, ( r, v ) => r.ParameterIndex = v, row => row.ParameterIndex );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<ParameterDefinition>> GetParamColumns()
      {
         yield return MetaDataColumnFunctionality.Constant16<ParameterDefinition, ParameterAttributes>( nameof( ParameterDefinition.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes );
         yield return MetaDataColumnFunctionality.Constant16<ParameterDefinition, Int32>( nameof( ParameterDefinition.Sequence ), ( r, v ) => r.Sequence = v, row => row.Sequence );
         yield return MetaDataColumnFunctionality.SystemString<ParameterDefinition>( nameof( ParameterDefinition.Name ), ( r, v ) => r.Name = v, row => row.Name );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<InterfaceImplementation>> GetInterfaceImplColumns()
      {
         yield return MetaDataColumnFunctionality.SimpleTableIndex<InterfaceImplementation>( nameof( InterfaceImplementation.Class ), Tables.TypeDef, ( r, v ) => r.Class = v, row => row.Class );
         yield return MetaDataColumnFunctionality.CodedTableIndex<InterfaceImplementation>( nameof( InterfaceImplementation.Interface ), TypeDefOrRef, ( r, v ) => r.Interface = v, row => row.Interface );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<MemberReference>> GetMemberRefColumns()
      {
         yield return MetaDataColumnFunctionality.CodedTableIndex<MemberReference>( nameof( MemberReference.DeclaringType ), MemberRefParent, ( r, v ) => r.DeclaringType = v, row => row.DeclaringType );
         yield return MetaDataColumnFunctionality.SystemString<MemberReference>( nameof( MemberReference.Name ), ( r, v ) => r.Name = v, row => row.Name );
         yield return MetaDataColumnFunctionality.BLOB<MemberReference, AbstractSignature>( nameof( MemberReference.Signature ), ( r, v ) => r.Signature = v, row => row.Signature );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<ConstantDefinition>> GetConstantColumns()
      {
         yield return MetaDataColumnFunctionality.Constant8<ConstantDefinition, SignatureElementTypes>( nameof( ConstantDefinition.Type ), ( r, v ) => r.Type = v, row => row.Type );
         yield return MetaDataColumnFunctionality.Constant8<ConstantDefinition, Byte>( "Padding", ( r, v ) => { }, row => 0 );
         yield return MetaDataColumnFunctionality.CodedTableIndex<ConstantDefinition>( nameof( ConstantDefinition.Parent ), HasConstant, ( r, v ) => r.Parent = v, row => row.Parent );
         yield return MetaDataColumnFunctionality.BLOB<ConstantDefinition, Object>( nameof( ConstantDefinition.Value ), ( r, v ) => r.Value = v, r => r.Value );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<CustomAttributeDefinition>> GetCustomAttributeColumns()
      {
         yield return MetaDataColumnFunctionality.CodedTableIndex<CustomAttributeDefinition>( nameof( CustomAttributeDefinition.Parent ), HasCustomAttribute, ( r, v ) => r.Parent = v, row => row.Parent );
         yield return MetaDataColumnFunctionality.CodedTableIndex<CustomAttributeDefinition>( nameof( CustomAttributeDefinition.Type ), CustomAttributeType, ( r, v ) => r.Type = v, row => row.Type );
         yield return MetaDataColumnFunctionality.BLOB<CustomAttributeDefinition, AbstractCustomAttributeSignature>( nameof( CustomAttributeDefinition.Signature ), ( r, v ) => r.Signature = v, r => r.Signature );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<FieldMarshal>> GetFieldMarshalColumns()
      {
         yield return MetaDataColumnFunctionality.CodedTableIndex<FieldMarshal>( nameof( FieldMarshal.Parent ), HasFieldMarshal, ( r, v ) => r.Parent = v, row => row.Parent );
         yield return MetaDataColumnFunctionality.BLOB<FieldMarshal, AbstractMarshalingInfo>( nameof( FieldMarshal.NativeType ), ( r, v ) => r.NativeType = v, row => row.NativeType );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<SecurityDefinition>> GetDeclSecurityColumns()
      {
         yield return MetaDataColumnFunctionality.Constant16<SecurityDefinition, SecurityAction>( nameof( SecurityDefinition.Action ), ( r, v ) => r.Action = v, row => row.Action );
         yield return MetaDataColumnFunctionality.CodedTableIndex<SecurityDefinition>( nameof( SecurityDefinition.Parent ), HasSecurity, ( r, v ) => r.Parent = v, row => row.Parent );
         yield return MetaDataColumnFunctionality.BLOB<SecurityDefinition, List<AbstractSecurityInformation>>( nameof( SecurityDefinition.PermissionSets ), ( r, v ) => { r.PermissionSets.Clear(); r.PermissionSets.AddRange( v ); }, row => row.PermissionSets );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<ClassLayout>> GetClassLayoutColumns()
      {
         yield return MetaDataColumnFunctionality.Constant16<ClassLayout, Int32>( nameof( ClassLayout.PackingSize ), ( r, v ) => r.PackingSize = v, row => row.PackingSize );
         yield return MetaDataColumnFunctionality.Constant32<ClassLayout, Int32>( nameof( ClassLayout.ClassSize ), ( r, v ) => r.ClassSize = v, row => row.ClassSize );
         yield return MetaDataColumnFunctionality.SimpleTableIndex<ClassLayout>( nameof( ClassLayout.Parent ), Tables.TypeDef, ( r, v ) => r.Parent = v, row => row.Parent );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<FieldLayout>> GetFieldLayoutColumns()
      {
         yield return MetaDataColumnFunctionality.Constant32<FieldLayout, Int32>( nameof( FieldLayout.Offset ), ( r, v ) => r.Offset = v, row => row.Offset );
         yield return MetaDataColumnFunctionality.SimpleTableIndex<FieldLayout>( nameof( FieldLayout.Field ), Tables.Field, ( r, v ) => r.Field = v, row => row.Field );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<StandaloneSignature>> GetStandaloneSigColumns()
      {
         yield return MetaDataColumnFunctionality.BLOB<StandaloneSignature, AbstractSignature>( nameof( StandaloneSignature.Signature ), ( r, v ) => r.Signature = v, r => r.Signature );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<EventMap>> GetEventMapColumns()
      {
         yield return MetaDataColumnFunctionality.SimpleTableIndex<EventMap>( nameof( EventMap.Parent ), Tables.TypeDef, ( r, v ) => r.Parent = v, row => row.Parent );
         yield return MetaDataColumnFunctionality.SimpleTableIndex<EventMap>( nameof( EventMap.EventList ), Tables.Event, ( r, v ) => r.EventList = v, row => row.EventList );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<EventDefinitionPointer>> GetEventPtrColumns()
      {
         yield return MetaDataColumnFunctionality.SimpleTableIndex<EventDefinitionPointer>( nameof( EventDefinitionPointer.EventIndex ), Tables.Event, ( r, v ) => r.EventIndex = v, row => row.EventIndex );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<EventDefinition>> GetEventDefColumns()
      {
         yield return MetaDataColumnFunctionality.Constant16<EventDefinition, EventAttributes>( nameof( EventDefinition.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes );
         yield return MetaDataColumnFunctionality.SystemString<EventDefinition>( nameof( EventDefinition.Name ), ( r, v ) => r.Name = v, row => row.Name );
         yield return MetaDataColumnFunctionality.CodedTableIndex<EventDefinition>( nameof( EventDefinition.EventType ), TypeDefOrRef, ( r, v ) => r.EventType = v, row => row.EventType );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<PropertyMap>> GetPropertyMapColumns()
      {
         yield return MetaDataColumnFunctionality.SimpleTableIndex<PropertyMap>( nameof( PropertyMap.Parent ), Tables.TypeDef, ( r, v ) => r.Parent = v, row => row.Parent );
         yield return MetaDataColumnFunctionality.SimpleTableIndex<PropertyMap>( nameof( PropertyMap.PropertyList ), Tables.Property, ( r, v ) => r.PropertyList = v, row => row.PropertyList );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<PropertyDefinitionPointer>> GetPropertyPtrColumns()
      {
         yield return MetaDataColumnFunctionality.SimpleTableIndex<PropertyDefinitionPointer>( nameof( PropertyDefinitionPointer.PropertyIndex ), Tables.Property, ( r, v ) => r.PropertyIndex = v, row => row.PropertyIndex );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<PropertyDefinition>> GetPropertyDefColumns()
      {
         yield return MetaDataColumnFunctionality.Constant16<PropertyDefinition, PropertyAttributes>( nameof( PropertyDefinition.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes );
         yield return MetaDataColumnFunctionality.SystemString<PropertyDefinition>( nameof( PropertyDefinition.Name ), ( r, v ) => r.Name = v, row => row.Name );
         yield return MetaDataColumnFunctionality.BLOB<PropertyDefinition, PropertySignature>( nameof( PropertyDefinition.Signature ), ( r, v ) => r.Signature = v, row => row.Signature );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<MethodSemantics>> GetMethodSemanticsColumns()
      {
         yield return MetaDataColumnFunctionality.Constant16<MethodSemantics, MethodSemanticsAttributes>( nameof( MethodSemantics.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes );
         yield return MetaDataColumnFunctionality.SimpleTableIndex<MethodSemantics>( nameof( MethodSemantics.Method ), Tables.MethodDef, ( r, v ) => r.Method = v, row => row.Method );
         yield return MetaDataColumnFunctionality.CodedTableIndex<MethodSemantics>( nameof( MethodSemantics.Associaton ), HasSemantics, ( r, v ) => r.Associaton = v, row => row.Associaton );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<MethodImplementation>> GetMethodImplColumns()
      {
         yield return MetaDataColumnFunctionality.SimpleTableIndex<MethodImplementation>( nameof( MethodImplementation.Class ), Tables.TypeDef, ( r, v ) => r.Class = v, row => row.Class );
         yield return MetaDataColumnFunctionality.CodedTableIndex<MethodImplementation>( nameof( MethodImplementation.MethodBody ), MethodDefOrRef, ( r, v ) => r.MethodBody = v, row => row.MethodBody );
         yield return MetaDataColumnFunctionality.CodedTableIndex<MethodImplementation>( nameof( MethodImplementation.MethodDeclaration ), MethodDefOrRef, ( r, v ) => r.MethodDeclaration = v, row => row.MethodDeclaration );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<ModuleReference>> GetModuleRefColumns()
      {
         yield return MetaDataColumnFunctionality.SystemString<ModuleReference>( nameof( ModuleReference.ModuleName ), ( r, v ) => r.ModuleName = v, row => row.ModuleName );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<TypeSpecification>> GetTypeSpecColumns()
      {
         yield return MetaDataColumnFunctionality.BLOB<TypeSpecification, TypeSignature>( nameof( TypeSpecification.Signature ), ( r, v ) => r.Signature = v, row => row.Signature );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<MethodImplementationMap>> GetImplMapColumns()
      {
         yield return MetaDataColumnFunctionality.Constant16<MethodImplementationMap, PInvokeAttributes>( nameof( MethodImplementationMap.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes );
         yield return MetaDataColumnFunctionality.CodedTableIndex<MethodImplementationMap>( nameof( MethodImplementationMap.MemberForwarded ), MemberForwarded, ( r, v ) => r.MemberForwarded = v, row => row.MemberForwarded );
         yield return MetaDataColumnFunctionality.SystemString<MethodImplementationMap>( nameof( MethodImplementationMap.ImportName ), ( r, v ) => r.ImportName = v, row => row.ImportName );
         yield return MetaDataColumnFunctionality.SimpleTableIndex<MethodImplementationMap>( nameof( MethodImplementationMap.ImportScope ), Tables.ModuleRef, ( r, v ) => r.ImportScope = v, row => row.ImportScope );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<FieldRVA>> GetFieldRVAColumns()
      {
         yield return MetaDataColumnFunctionality.DataReferenceForClasses<FieldRVA, Byte[]>( nameof( FieldRVA.Data ), ( r, v ) => r.Data = v, r => r.Data );
         yield return MetaDataColumnFunctionality.SimpleTableIndex<FieldRVA>( nameof( FieldRVA.Field ), Tables.Field, ( r, v ) => r.Field = v, row => row.Field );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<EditAndContinueLog>> GetENCLogColumns()
      {
         yield return MetaDataColumnFunctionality.Constant32<EditAndContinueLog, Int32>( nameof( EditAndContinueLog.Token ), ( r, v ) => r.Token = v, row => row.Token );
         yield return MetaDataColumnFunctionality.Constant32<EditAndContinueLog, Int32>( nameof( EditAndContinueLog.FuncCode ), ( r, v ) => r.FuncCode = v, row => row.FuncCode );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<EditAndContinueMap>> GetENCMapColumns()
      {
         yield return MetaDataColumnFunctionality.Constant32<EditAndContinueMap, Int32>( nameof( EditAndContinueMap.Token ), ( r, v ) => r.Token = v, row => row.Token );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<AssemblyDefinition>> GetAssemblyDefColumns()
      {
         yield return MetaDataColumnFunctionality.Constant32<AssemblyDefinition, AssemblyHashAlgorithm>( nameof( AssemblyDefinition.HashAlgorithm ), ( r, v ) => r.HashAlgorithm = v, row => row.HashAlgorithm );
         yield return MetaDataColumnFunctionality.Constant16<AssemblyDefinition, Int32>( nameof( AssemblyInformation.VersionMajor ), ( r, v ) => r.AssemblyInformation.VersionMajor = v, row => row.AssemblyInformation.VersionMajor );
         yield return MetaDataColumnFunctionality.Constant16<AssemblyDefinition, Int32>( nameof( AssemblyInformation.VersionMinor ), ( r, v ) => r.AssemblyInformation.VersionMinor = v, row => row.AssemblyInformation.VersionMinor );
         yield return MetaDataColumnFunctionality.Constant16<AssemblyDefinition, Int32>( nameof( AssemblyInformation.VersionBuild ), ( r, v ) => r.AssemblyInformation.VersionBuild = v, row => row.AssemblyInformation.VersionBuild );
         yield return MetaDataColumnFunctionality.Constant16<AssemblyDefinition, Int32>( nameof( AssemblyInformation.VersionRevision ), ( r, v ) => r.AssemblyInformation.VersionRevision = v, row => row.AssemblyInformation.VersionRevision );
         yield return MetaDataColumnFunctionality.Constant32<AssemblyDefinition, AssemblyFlags>( nameof( AssemblyDefinition.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes );
         yield return MetaDataColumnFunctionality.BLOB<AssemblyDefinition, Byte[]>( nameof( AssemblyInformation.PublicKeyOrToken ), ( r, v ) => r.AssemblyInformation.PublicKeyOrToken = v, r => r.AssemblyInformation.PublicKeyOrToken );
         yield return MetaDataColumnFunctionality.SystemString<AssemblyDefinition>( nameof( AssemblyInformation.Name ), ( r, v ) => r.AssemblyInformation.Name = v, row => row.AssemblyInformation.Name );
         yield return MetaDataColumnFunctionality.SystemString<AssemblyDefinition>( nameof( AssemblyInformation.Culture ), ( r, v ) => r.AssemblyInformation.Culture = v, row => row.AssemblyInformation.Culture );
      }
#pragma warning disable 618
      protected static IEnumerable<MetaDataColumnFunctionality<AssemblyDefinitionProcessor>> GetAssemblyDefProcessorColumns()
      {
         yield return MetaDataColumnFunctionality.Constant32<AssemblyDefinitionProcessor, Int32>( nameof( AssemblyDefinitionProcessor.Processor ), ( r, v ) => r.Processor = v, row => row.Processor );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<AssemblyDefinitionOS>> GetAssemblyDefOSColumns()
      {
         yield return MetaDataColumnFunctionality.Constant32<AssemblyDefinitionOS, Int32>( nameof( AssemblyDefinitionOS.OSPlatformID ), ( r, v ) => r.OSPlatformID = v, row => row.OSPlatformID );
         yield return MetaDataColumnFunctionality.Constant32<AssemblyDefinitionOS, Int32>( nameof( AssemblyDefinitionOS.OSMajorVersion ), ( r, v ) => r.OSMajorVersion = v, row => row.OSMajorVersion );
         yield return MetaDataColumnFunctionality.Constant32<AssemblyDefinitionOS, Int32>( nameof( AssemblyDefinitionOS.OSMinorVersion ), ( r, v ) => r.OSMinorVersion = v, row => row.OSMinorVersion );
      }
#pragma warning restore 618

      protected static IEnumerable<MetaDataColumnFunctionality<AssemblyReference>> GetAssemblyRefColumns()
      {
         yield return MetaDataColumnFunctionality.Constant16<AssemblyReference, Int32>( nameof( AssemblyInformation.VersionMajor ), ( r, v ) => r.AssemblyInformation.VersionMajor = v, row => row.AssemblyInformation.VersionMajor );
         yield return MetaDataColumnFunctionality.Constant16<AssemblyReference, Int32>( nameof( AssemblyInformation.VersionMinor ), ( r, v ) => r.AssemblyInformation.VersionMinor = v, row => row.AssemblyInformation.VersionMinor );
         yield return MetaDataColumnFunctionality.Constant16<AssemblyReference, Int32>( nameof( AssemblyInformation.VersionBuild ), ( r, v ) => r.AssemblyInformation.VersionBuild = v, row => row.AssemblyInformation.VersionBuild );
         yield return MetaDataColumnFunctionality.Constant16<AssemblyReference, Int32>( nameof( AssemblyInformation.VersionRevision ), ( r, v ) => r.AssemblyInformation.VersionRevision = v, row => row.AssemblyInformation.VersionRevision );
         yield return MetaDataColumnFunctionality.Constant32<AssemblyReference, AssemblyFlags>( nameof( AssemblyDefinition.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes );
         yield return MetaDataColumnFunctionality.BLOB<AssemblyReference, Byte[]>( nameof( AssemblyInformation.PublicKeyOrToken ), ( r, v ) => r.AssemblyInformation.PublicKeyOrToken = v, r => r.AssemblyInformation.PublicKeyOrToken );
         yield return MetaDataColumnFunctionality.SystemString<AssemblyReference>( nameof( AssemblyInformation.Name ), ( r, v ) => r.AssemblyInformation.Name = v, row => row.AssemblyInformation.Name );
         yield return MetaDataColumnFunctionality.SystemString<AssemblyReference>( nameof( AssemblyInformation.Culture ), ( r, v ) => r.AssemblyInformation.Culture = v, row => row.AssemblyInformation.Culture );
         yield return MetaDataColumnFunctionality.BLOB<AssemblyReference, Byte[]>( nameof( AssemblyReference.HashValue ), ( r, v ) => r.HashValue = v, row => row.HashValue );
      }

#pragma warning disable 618
      protected static IEnumerable<MetaDataColumnFunctionality<AssemblyReferenceProcessor>> GetAssemblyRefProcessorColumns()
      {
         yield return MetaDataColumnFunctionality.Constant32<AssemblyReferenceProcessor, Int32>( nameof( AssemblyReferenceProcessor.Processor ), ( r, v ) => r.Processor = v, row => row.Processor );
         yield return MetaDataColumnFunctionality.SimpleTableIndex<AssemblyReferenceProcessor>( nameof( AssemblyReferenceProcessor.AssemblyRef ), Tables.AssemblyRef, ( r, v ) => r.AssemblyRef = v, row => row.AssemblyRef );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<AssemblyReferenceOS>> GetAssemblyRefOSColumns()
      {
         yield return MetaDataColumnFunctionality.Constant32<AssemblyReferenceOS, Int32>( nameof( AssemblyReferenceOS.OSPlatformID ), ( r, v ) => r.OSPlatformID = v, row => row.OSPlatformID );
         yield return MetaDataColumnFunctionality.Constant32<AssemblyReferenceOS, Int32>( nameof( AssemblyReferenceOS.OSMajorVersion ), ( r, v ) => r.OSMajorVersion = v, row => row.OSMajorVersion );
         yield return MetaDataColumnFunctionality.Constant32<AssemblyReferenceOS, Int32>( nameof( AssemblyReferenceOS.OSMinorVersion ), ( r, v ) => r.OSMinorVersion = v, row => row.OSMinorVersion );
         yield return MetaDataColumnFunctionality.SimpleTableIndex<AssemblyReferenceOS>( nameof( AssemblyReferenceOS.AssemblyRef ), Tables.AssemblyRef, ( r, v ) => r.AssemblyRef = v, row => row.AssemblyRef );
      }
#pragma warning restore 618

      protected static IEnumerable<MetaDataColumnFunctionality<FileReference>> GetFileColumns()
      {
         yield return MetaDataColumnFunctionality.Constant32<FileReference, FileAttributes>( nameof( FileReference.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes );
         yield return MetaDataColumnFunctionality.SystemString<FileReference>( nameof( FileReference.Name ), ( r, v ) => r.Name = v, row => row.Name );
         yield return MetaDataColumnFunctionality.BLOB<FileReference, Byte[]>( nameof( FileReference.HashValue ), ( r, v ) => r.HashValue = v, row => row.HashValue );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<ExportedType>> GetExportedTypeColumns()
      {
         yield return MetaDataColumnFunctionality.Constant32<ExportedType, TypeAttributes>( nameof( ExportedType.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes );
         yield return MetaDataColumnFunctionality.Constant32<ExportedType, Int32>( nameof( ExportedType.TypeDefinitionIndex ), ( r, v ) => r.TypeDefinitionIndex = v, row => row.TypeDefinitionIndex );
         yield return MetaDataColumnFunctionality.SystemString<ExportedType>( nameof( ExportedType.Name ), ( r, v ) => r.Name = v, row => row.Name );
         yield return MetaDataColumnFunctionality.SystemString<ExportedType>( nameof( ExportedType.Namespace ), ( r, v ) => r.Namespace = v, row => row.Namespace );
         yield return MetaDataColumnFunctionality.CodedTableIndex<ExportedType>( nameof( ExportedType.Implementation ), Implementation, ( r, v ) => r.Implementation = v, row => row.Implementation );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<ManifestResource>> GetManifestResourceColumns()
      {
         yield return MetaDataColumnFunctionality.DataReferenceForStructs<ManifestResource, Int32>( nameof( ManifestResource.Offset ), ( r, v ) => r.Offset = v, r => r.Offset );
         yield return MetaDataColumnFunctionality.Constant32<ManifestResource, ManifestResourceAttributes>( nameof( ManifestResource.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes );
         yield return MetaDataColumnFunctionality.SystemString<ManifestResource>( nameof( ManifestResource.Name ), ( r, v ) => r.Name = v, row => row.Name );
         yield return MetaDataColumnFunctionality.CodedTableIndexNullable<ManifestResource>( nameof( ManifestResource.Implementation ), Implementation, ( r, v ) => r.Implementation = v, row => row.Implementation );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<NestedClassDefinition>> GetNestedClassColumns()
      {
         yield return MetaDataColumnFunctionality.SimpleTableIndex<NestedClassDefinition>( nameof( NestedClassDefinition.NestedClass ), Tables.TypeDef, ( r, v ) => r.NestedClass = v, row => row.NestedClass );
         yield return MetaDataColumnFunctionality.SimpleTableIndex<NestedClassDefinition>( nameof( NestedClassDefinition.EnclosingClass ), Tables.TypeDef, ( r, v ) => r.EnclosingClass = v, row => row.EnclosingClass );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<GenericParameterDefinition>> GetGenericParamColumns()
      {
         yield return MetaDataColumnFunctionality.Constant16<GenericParameterDefinition, Int32>( nameof( GenericParameterDefinition.GenericParameterIndex ), ( r, v ) => r.GenericParameterIndex = v, row => row.GenericParameterIndex );
         yield return MetaDataColumnFunctionality.Constant16<GenericParameterDefinition, GenericParameterAttributes>( nameof( GenericParameterDefinition.Attributes ), ( r, v ) => r.Attributes = v, row => row.Attributes );
         yield return MetaDataColumnFunctionality.CodedTableIndex<GenericParameterDefinition>( nameof( GenericParameterDefinition.Owner ), TypeOrMethodDef, ( r, v ) => r.Owner = v, row => row.Owner );
         yield return MetaDataColumnFunctionality.SystemString<GenericParameterDefinition>( nameof( GenericParameterDefinition.Name ), ( r, v ) => r.Name = v, row => row.Name );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<MethodSpecification>> GetMethodSpecColumns()
      {
         yield return MetaDataColumnFunctionality.CodedTableIndex<MethodSpecification>( nameof( MethodSpecification.Method ), MethodDefOrRef, ( r, v ) => r.Method = v, row => row.Method );
         yield return MetaDataColumnFunctionality.BLOB<MethodSpecification, GenericMethodSignature>( nameof( MethodSpecification.Signature ), ( r, v ) => r.Signature = v, row => row.Signature );
      }

      protected static IEnumerable<MetaDataColumnFunctionality<GenericParameterConstraintDefinition>> GetGenericParamConstraintColumns()
      {
         yield return MetaDataColumnFunctionality.SimpleTableIndex<GenericParameterConstraintDefinition>( nameof( GenericParameterConstraintDefinition.Owner ), Tables.GenericParameter, ( r, v ) => r.Owner = v, row => row.Owner );
         yield return MetaDataColumnFunctionality.CodedTableIndex<GenericParameterConstraintDefinition>( nameof( GenericParameterConstraintDefinition.Constraint ), TypeDefOrRef, ( r, v ) => r.Constraint = v, row => row.Constraint );
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

      public abstract ArrayQuery<MetaDataColumnFunctionality> ColumnsFunctionalityNotGeneric { get; }

      public abstract MetaDataTable CreateMetaDataTableNotGeneric( Int32 capacity );
   }

   public sealed class MetaDataTableInformation<TRow> : MetaDataTableInformation
      where TRow : class
   {
      public MetaDataTableInformation(
         Tables tableKind,
         IEqualityComparer<TRow> equalityComparer,
         IComparer<TRow> comparer,
         IEnumerable<MetaDataColumnFunctionality<TRow>> columns
         )
         : base( tableKind, new EqualityComparerWrapper<TRow>( equalityComparer ), comparer == null ? null : new ComparerWrapper<TRow>( comparer ) )
      {
         this.EqualityComparer = equalityComparer;
         this.Comparer = comparer;


         ArgumentValidator.ValidateNotNull( "Columns", columns );

         this.ColumnsFunctionality = columns.ToArrayProxy().CQ;

         if ( this.ColumnsFunctionality.Count <= 0 )
         {
            throw new ArgumentException( "Table must have at least one column." );
         }
      }

      public IEqualityComparer<TRow> EqualityComparer { get; }

      public IComparer<TRow> Comparer { get; }

      public MetaDataTable<TRow> CreateMetaDataTable( Int32 capacity )
      {
         return new Implementation.MetaDataTableImpl<TRow>( this, capacity );
      }

      public ArrayQuery<MetaDataColumnFunctionality<TRow>> ColumnsFunctionality { get; }

      public override MetaDataTable CreateMetaDataTableNotGeneric( Int32 capacity )
      {
         return this.CreateMetaDataTable( capacity );
      }

      public override ArrayQuery<MetaDataColumnFunctionality> ColumnsFunctionalityNotGeneric
      {
         get
         {
            return this.ColumnsFunctionality;
         }
      }
   }

   public abstract class MetaDataColumnFunctionality
   {
      internal MetaDataColumnFunctionality(
         MetaDataColumnInformation information
         )
      {
         ArgumentValidator.ValidateNotNull( "Information", information );

         this.Information = information;
      }

      public MetaDataColumnInformation Information { get; }

      public abstract Object GetterNotGeneric( Object row );

      public abstract void SetterNotGeneric( Object row, Object value );

      public abstract Type RowType { get; }

      public abstract Type ValueType { get; }

      public static MetaDataColumnFunctionalityForStructs<TRow, TValue> Constant8<TRow, TValue>(
         String columnName,
         Action<TRow, TValue> setter,
         Func<TRow, TValue> getter
         )
         where TRow : class
         where TValue : struct
      {
         return ConstantCustom<TRow, TValue>( columnName, sizeof( Byte ), getter, setter );
      }
      public static MetaDataColumnFunctionalityForStructs<TRow, TValue> Constant16<TRow, TValue>(
         String columnName,
         Action<TRow, TValue> setter,
         Func<TRow, TValue> getter
         )
         where TRow : class
         where TValue : struct
      {
         return ConstantCustom<TRow, TValue>( columnName, sizeof( Int16 ), getter, setter );
      }

      public static MetaDataColumnFunctionalityForStructs<TRow, TValue> Constant32<TRow, TValue>(
         String columnName,
         Action<TRow, TValue> setter,
         Func<TRow, TValue> getter
         )
         where TRow : class
         where TValue : struct
      {
         return ConstantCustom<TRow, TValue>( columnName, sizeof( Int32 ), getter, setter );
      }

      public static MetaDataColumnFunctionalityForStructs<TRow, TValue> ConstantCustom<TRow, TValue>(
         String columnName,
         Int32 byteCount,
         Func<TRow, TValue> getter,
         Action<TRow, TValue> setter
         )
         where TRow : class
         where TValue : struct
      {
         return new MetaDataColumnFunctionalityForStructs<TRow, TValue>( new MetaDataColumnInformation_FixedSizeConstant( columnName, byteCount ), getter, setter );
      }

      public static MetaDataColumnFunctionalityForClasses<TRow, String> SystemString<TRow>(
         String columnName,
         Action<TRow, String> setter,
         Func<TRow, String> getter
         )
         where TRow : class
      {
         return new MetaDataColumnFunctionalityForClasses<TRow, String>( new MetaDataColumnInformation_HeapIndex( columnName, IO.Defaults.MetaDataConstants.SYS_STRING_STREAM_NAME ), getter, setter );
      }

      public static MetaDataColumnFunctionalityForNullables<TRow, Guid> GUID<TRow>(
         String columnName,
         Action<TRow, Guid?> setter,
         Func<TRow, Guid?> getter
         )
         where TRow : class
      {
         return new MetaDataColumnFunctionalityForNullables<TRow, Guid>( new MetaDataColumnInformation_HeapIndex( columnName, IO.Defaults.MetaDataConstants.GUID_STREAM_NAME ), getter, setter );
      }

      public static MetaDataColumnFunctionalityForStructs<TRow, TableIndex> SimpleTableIndex<TRow>(
         String columnName,
         Tables targetTable,
         Action<TRow, TableIndex> setter,
         Func<TRow, TableIndex> getter
         )
         where TRow : class
      {
         return new MetaDataColumnFunctionalityForStructs<TRow, TableIndex>( new MetaDataColumnInformation_SimpleTableIndex( columnName, targetTable ), getter, setter );
      }

      public static MetaDataColumnFunctionalityForStructs<TRow, TableIndex> CodedTableIndex<TRow>(
         String columnName,
         ArrayQuery<Tables?> targetTables,
         Action<TRow, TableIndex> setter,
         Func<TRow, TableIndex> getter
         )
         where TRow : class
      {
         return new MetaDataColumnFunctionalityForStructs<TRow, TableIndex>( new MetaDataColumnInformation_CodedTableIndex( columnName, targetTables ), getter, setter );
      }

      public static MetaDataColumnFunctionalityForNullables<TRow, TableIndex> CodedTableIndexNullable<TRow>(
         String columnName,
         ArrayQuery<Tables?> targetTables,
         Action<TRow, TableIndex?> setter,
         Func<TRow, TableIndex?> getter
         )
         where TRow : class
      {
         return new MetaDataColumnFunctionalityForNullables<TRow, TableIndex>( new MetaDataColumnInformation_CodedTableIndex( columnName, targetTables ), getter, setter );
      }

      public static MetaDataColumnFunctionalityForClasses<TRow, TValue> BLOB<TRow, TValue>(
         String columnName,
         Action<TRow, TValue> setter,
         Func<TRow, TValue> getter
         )
         where TRow : class
         where TValue : class
      {
         return new MetaDataColumnFunctionalityForClasses<TRow, TValue>( new MetaDataColumnInformation_HeapIndex( columnName, IO.Defaults.MetaDataConstants.BLOB_STREAM_NAME ), getter, setter );
      }

      public static MetaDataColumnFunctionalityForClasses<TRow, TValue> DataReferenceForClasses<TRow, TValue>(
         String columnName,
         Action<TRow, TValue> setter,
         Func<TRow, TValue> getter
         )
         where TRow : class
         where TValue : class
      {
         return new MetaDataColumnFunctionalityForClasses<TRow, TValue>( new MetaDataColumnInformation_DataReference( columnName ), getter, setter );
      }

      public static MetaDataColumnFunctionalityForStructs<TRow, TValue> DataReferenceForStructs<TRow, TValue>(
         String columnName,
         Action<TRow, TValue> setter,
         Func<TRow, TValue> getter
         )
         where TRow : class
         where TValue : struct
      {
         return new MetaDataColumnFunctionalityForStructs<TRow, TValue>( new MetaDataColumnInformation_DataReference( columnName ), getter, setter );
      }

   }

   public abstract class MetaDataColumnFunctionality<TRow> : MetaDataColumnFunctionality
      where TRow : class
   {


      public MetaDataColumnFunctionality(
         MetaDataColumnInformation information
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

      public override Type RowType
      {
         get
         {
            return typeof( TRow );
         }
      }
   }

   public sealed class MetaDataColumnFunctionalityForClasses<TRow, TValue> : MetaDataColumnFunctionality<TRow>
      where TRow : class
      where TValue : class
   {

      private readonly Func<TRow, TValue> _getter;
      private readonly Action<TRow, TValue> _setter;

      public MetaDataColumnFunctionalityForClasses(
         MetaDataColumnInformation information,
         Func<TRow, TValue> getter,
         Action<TRow, TValue> setter
         )
         : base( information )
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

   public sealed class MetaDataColumnFunctionalityForStructs<TRow, TValue> : MetaDataColumnFunctionality<TRow>
      where TRow : class
      where TValue : struct
   {

      private readonly Func<TRow, TValue> _getter;
      private readonly Action<TRow, TValue> _setter;

      public MetaDataColumnFunctionalityForStructs(
         MetaDataColumnInformation information,
         Func<TRow, TValue> getter,
         Action<TRow, TValue> setter
         )
         : base( information )
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

   public sealed class MetaDataColumnFunctionalityForNullables<TRow, TValue> : MetaDataColumnFunctionality<TRow>
      where TRow : class
      where TValue : struct
   {

      private readonly Func<TRow, TValue?> _getter;
      private readonly Action<TRow, TValue?> _setter;

      public MetaDataColumnFunctionalityForNullables(
         MetaDataColumnInformation information,
         Func<TRow, TValue?> getter,
         Action<TRow, TValue?> setter
         )
         : base( information )
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
            return typeof( TValue );
         }
      }
   }

   public abstract class MetaDataColumnInformation
   {
      internal MetaDataColumnInformation(
         String columnName
         )
      {
         ArgumentValidator.ValidateNotEmpty( "Column name", columnName );

         this.Name = columnName;
      }

      public String Name { get; }

      public abstract MetaDataColumnInformationKind ColumnKind { get; }
   }

   public sealed class MetaDataColumnInformation_FixedSizeConstant : MetaDataColumnInformation
   {
      internal MetaDataColumnInformation_FixedSizeConstant(
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

   public sealed class MetaDataColumnInformation_SimpleTableIndex : MetaDataColumnInformation
   {
      public MetaDataColumnInformation_SimpleTableIndex(
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

   public sealed class MetaDataColumnInformation_CodedTableIndex : MetaDataColumnInformation
   {
      public MetaDataColumnInformation_CodedTableIndex(
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

   public sealed class MetaDataColumnInformation_HeapIndex : MetaDataColumnInformation
   {
      public MetaDataColumnInformation_HeapIndex(
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

   public sealed class MetaDataColumnInformation_DataReference : MetaDataColumnInformation
   {
      public MetaDataColumnInformation_DataReference(
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

   public abstract class ColumnSerializationFunctionality<TRow>
   {
      public abstract Object DeserializeNotGeneric( ColumnDeserializationArgs args, Int32 value );

      public abstract Int32 RawGetterNotGeneric( Object rawRow );

      public abstract void RawSetterNotGeneric( Object rawRow, Int32 value );
   }

   public struct ColumnDeserializationArgs
   {

   }
}
