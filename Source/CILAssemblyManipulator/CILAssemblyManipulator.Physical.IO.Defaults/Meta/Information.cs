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
using CAMPhysicalR::CILAssemblyManipulator.Physical.Meta;

using CAMPhysicalIO;
using CAMPhysicalIO::CILAssemblyManipulator.Physical.IO;
using CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta;

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
   /// <summary>
   /// This class acts as a factory for <see cref="MetaDataTableInformationProvider"/>s to use in CAM.Physical framework.
   /// </summary>
   public static class CILMetaDataTableInformationProviderFactory
   {
      internal const Int32 AMOUNT_OF_FIXED_TABLES = 0x2D;


      private static MetaDataTableInformationProviderWithArray CreateInstance( IEnumerable<MetaDataTableInformation> tableInfos )
      {
         var retVal = new MetaDataTableInformationProviderWithArray( tableInfos, CAMCoreInternals.AMOUNT_OF_TABLES );

         retVal.RegisterFunctionality<CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider>( () => DefaultOpCodeProvider.DefaultInstance );
         retVal.RegisterFunctionality<SignatureProvider>( () => DefaultSignatureProvider.DefaultInstance );
         retVal.RegisterFunctionality<ResolvingProviderProvider>( () => md => new DefaultResolvingProvider(
            md,
            retVal.TableInformationArray.SelectMany( i => i?.ColumnsInformationNotGeneric
               .Select( c => c?.GetFunctionality<MetaDataColumnInformationWithResolvingCapability>() )
               .Where( info => info != null )
               .Select( info => Tuple.Create( (Tables) i.TableIndex, info ) ) ?? Empty<Tuple<Tables, MetaDataColumnInformationWithResolvingCapability>>.Enumerable
               )
            ) );
         return retVal;
      }

      /// <summary>
      /// This helper method creates the default instance of <see cref="MetaDataTableInformationProvider"/>.
      /// </summary>
      /// <returns>The new instance of <see cref="MetaDataTableInformationProvider"/>.</returns>
      public static MetaDataTableInformationProvider CreateDefault()
      {
         return CreateInstance( CreateFixedTableInformations() );
      }

      /// <summary>
      /// This helper method creates the default instance of <see cref="MetaDataTableInformationProvider"/> with given <see cref="MetaDataTableInformation"/> objects for any additional tables.
      /// </summary>
      /// <param name="tableInfos">The enumerable of <see cref="MetaDataTableInformation"/>. May be <c>null</c> or contain <c>null</c> values. The order does not matter.</param>
      /// <returns>The new instance of <see cref="MetaDataTableInformationProvider"/> containing <see cref="MetaDataTableInformation"/> for additional tables.</returns>
      /// <remarks>
      /// The <see cref="MetaDataTableInformation.TableIndex"/> of the given <paramref name="tableInfos"/> should be greater than maximum value of <see cref="Tables"/> enumeration.
      /// </remarks>
      public static MetaDataTableInformationProvider CreateWithAdditionalTables( IEnumerable<MetaDataTableInformation> tableInfos )
      {
         return CreateInstance( CreateFixedTableInformations().Concat( tableInfos?.Where( t => (Int32) ( t?.TableIndex ?? 0 ) > AMOUNT_OF_FIXED_TABLES ) ?? Empty<MetaDataTableInformation>.Enumerable ) );
      }

      /// <summary>
      /// This helper method creates the default instance of <see cref="MetaDataTableInformationProvider"/> with given <see cref="MetaDataTableInformation"/> objects for all tables.
      /// </summary>
      /// <param name="tableInfos">The enumerable of <see cref="MetaDataTableInformation"/>. May be <c>null</c> or containg <c>null</c> values. The order does not matter.</param>
      /// <returns>The new instance of <see cref="MetaDataTableInformationProvider"/> containing <see cref="MetaDataTableInformation"/> for all tables, including the fixed ones of <see cref="CILMetaData"/>.</returns>
      public static MetaDataTableInformationProvider CreateWithExactTables( IEnumerable<MetaDataTableInformation> tableInfos )
      {
         return CreateInstance( tableInfos );
      }

      /// <summary>
      /// Creates a new instance of <see cref="MetaDataTableInformation{TRow}"/> with required functionalities registered.
      /// </summary>
      /// <typeparam name="TRow">The type of normal row.</typeparam>
      /// <typeparam name="TRawRow">The type of raw row.</typeparam>
      /// <param name="tableKind">The table ID, as value of <see cref="Tables"/> enumeration.</param>
      /// <param name="equalityComparer">The equality comparer for rows.</param>
      /// <param name="comparer">The optional comparer for rows.</param>
      /// <param name="rowFactory">The callbackto create blank normal row.</param>
      /// <param name="columns">The enumerable of <see cref="MetaDataColumnInformation{TRow}"/>s, should be created with methods of <see cref="MetaDataColumnInformationFactory"/>.</param>
      /// <param name="rawRowFactory">The callback to create blank raw row.</param>
      /// <param name="isSorted">Whether this table is sorted, will affect <see cref="MetaDataTableStreamHeader.SortedTablesBitVector"/> value.</param>
      /// <returns>A new instance of <see cref="MetaDataTableInformation{TRow}"/> with required functionalities registered.</returns>
      /// <remarks>
      /// The functionalities that are registered by this method are as follows:
      /// <list type="table">
      /// <listheader>
      /// <term>Functionality type</term>
      /// <term>Description</term>
      /// </listheader>
      /// <item>
      /// <term><see cref="MetaDataTableInformationWithSerializationCapabilityDelegate"/></term>
      /// <term>Always registered, contain callback to create <see cref="TableSerializationLogicalFunctionality"/>.</term>
      /// </item>
      /// </list>
      /// All of these functionalities are accessible from the created <see cref="MetaDataTableInformation{TRow}"/> with methods and extension methods of <see cref="ExtensionByCompositionProvider{TFunctionality}"/>.
      /// </remarks>
      public static MetaDataTableInformation<TRow> CreateSingleTableInfo<TRow, TRawRow>(
         Tables tableKind,
         IEqualityComparer<TRow> equalityComparer,
         IComparer<TRow> comparer,
         Func<TRow> rowFactory,
         IEnumerable<MetaDataColumnInformation<TRow>> columns,
         Func<TRawRow> rawRowFactory,
         Boolean isSorted
         )
         where TRow : class
         where TRawRow : class
      {
         ArgumentValidator.ValidateNotNull( "Raw row factory", rawRowFactory );
         var retVal = new MetaDataTableInformation<TRow>( (Int32) tableKind, equalityComparer, comparer, rowFactory, columns );
         retVal.RegisterFunctionalityDirect<MetaDataTableInformationWithSerializationCapabilityDelegate>( args => new TableSerializationLogicalFunctionalityImpl<TRow, TRawRow>(
            (Tables) retVal.TableIndex,
            isSorted,
            retVal.ColumnsInformation.Select( c => c.GetFunctionality<DefaultColumnSerializationInfo<TRow, TRawRow>>() ),
            retVal.CreateRow,
            rawRowFactory,
            args
            ) );

         return retVal;
      }

      /// <summary>
      /// This is helper static method to generate enumerable of <see cref="MetaDataTableInformation"/>s for all fixed tables of <see cref="CILMetaData"/>.
      /// </summary>
      /// <returns>Enumerable of <see cref="MetaDataTableInformation"/>s for all fixed tables of <see cref="CILMetaData"/>.</returns>
      /// <remarks>
      /// The returned <see cref="MetaDataTableInformation"/>s are actually instances of <see cref="MetaDataTableInformation{TRow}"/>, with their generic argument depending on the type of the row of the fixed table.
      /// </remarks>
      /// <seealso cref="GetModuleDefColumns"/>
      /// <seealso cref="GetTypeRefColumns"/>
      /// <seealso cref="GetTypeDefColumns"/>
      /// <seealso cref="GetFieldPtrColumns"/>
      /// <seealso cref="GetFieldDefColumns"/>
      /// <seealso cref="GetMethodPtrColumns"/>
      /// <seealso cref="GetMethodDefColumns"/>
      /// <seealso cref="GetParamPtrColumns"/>
      /// <seealso cref="GetParamColumns"/>
      /// <seealso cref="GetInterfaceImplColumns"/>
      /// <seealso cref="GetMemberRefColumns"/>
      /// <seealso cref="GetConstantColumns"/>
      /// <seealso cref="GetCustomAttributeColumns"/>
      /// <seealso cref="GetFieldMarshalColumns"/>
      /// <seealso cref="GetDeclSecurityColumns"/>
      /// <seealso cref="GetClassLayoutColumns"/>
      /// <seealso cref="GetFieldLayoutColumns"/>
      /// <seealso cref="GetStandaloneSigColumns"/>
      /// <seealso cref="GetEventMapColumns"/>
      /// <seealso cref="GetEventPtrColumns"/>
      /// <seealso cref="GetEventDefColumns"/>
      /// <seealso cref="GetPropertyMapColumns"/>
      /// <seealso cref="GetPropertyPtrColumns"/>
      /// <seealso cref="GetPropertyDefColumns"/>
      /// <seealso cref="GetMethodSemanticsColumns"/>
      /// <seealso cref="GetMethodImplColumns"/>
      /// <seealso cref="GetModuleRefColumns"/>
      /// <seealso cref="GetTypeSpecColumns"/>
      /// <seealso cref="GetImplMapColumns"/>
      /// <seealso cref="GetFieldRVAColumns"/>
      /// <seealso cref="GetENCLogColumns"/>
      /// <seealso cref="GetENCMapColumns"/>
      /// <seealso cref="GetAssemblyDefColumns"/>
      /// <seealso cref="GetAssemblyDefProcessorColumns"/>
      /// <seealso cref="GetAssemblyDefOSColumns"/>
      /// <seealso cref="GetAssemblyRefColumns"/>
      /// <seealso cref="GetAssemblyRefProcessorColumns"/>
      /// <seealso cref="GetAssemblyRefOSColumns"/>
      /// <seealso cref="GetFileColumns"/>
      /// <seealso cref="GetExportedTypeColumns"/>
      /// <seealso cref="GetManifestResourceColumns"/>
      /// <seealso cref="GetNestedClassColumns"/>
      /// <seealso cref="GetGenericParamColumns"/>
      /// <seealso cref="GetMethodSpecColumns"/>
      /// <seealso cref="GetGenericParamConstraintColumns"/>
      public static IEnumerable<MetaDataTableInformation> CreateFixedTableInformations()
      {
         yield return CreateSingleTableInfo<ModuleDefinition, RawModuleDefinition>(
            Tables.Module,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ModuleDefinitionEqualityComparer,
            null,
            () => new ModuleDefinition(),
            GetModuleDefColumns(),
            () => new RawModuleDefinition(),
            false
            );

         yield return CreateSingleTableInfo<TypeReference, RawTypeReference>(
            Tables.TypeRef,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.TypeReferenceEqualityComparer,
            null,
            () => new TypeReference(),
            GetTypeRefColumns(),
            () => new RawTypeReference(),
            false
            );

         yield return CreateSingleTableInfo<TypeDefinition, RawTypeDefinition>(
            Tables.TypeDef,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.TypeDefinitionEqualityComparer,
            null,
            () => new TypeDefinition(),
            GetTypeDefColumns(),
            () => new RawTypeDefinition(),
            false
            );

         yield return CreateSingleTableInfo<FieldDefinitionPointer, RawFieldDefinitionPointer>(
            Tables.FieldPtr,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.FieldDefinitionPointerEqualityComparer,
            null,
            () => new FieldDefinitionPointer(),
            GetFieldPtrColumns(),
            () => new RawFieldDefinitionPointer(),
            false
            );

         yield return CreateSingleTableInfo<FieldDefinition, RawFieldDefinition>(
            Tables.Field,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.FieldDefinitionEqualityComparer,
            null,
            () => new FieldDefinition(),
            GetFieldDefColumns(),
            () => new RawFieldDefinition(),
            false
            );

         yield return CreateSingleTableInfo<MethodDefinitionPointer, RawMethodDefinitionPointer>(
            Tables.MethodPtr,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.MethodDefinitionPointerEqualityComparer,
            null,
            () => new MethodDefinitionPointer(),
            GetMethodPtrColumns(),
            () => new RawMethodDefinitionPointer(),
            false
            );

         yield return CreateSingleTableInfo<MethodDefinition, RawMethodDefinition>(
            Tables.MethodDef,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.MethodDefinitionEqualityComparer,
            null,
            () => new MethodDefinition(),
            GetMethodDefColumns(),
            () => new RawMethodDefinition(),
            false
            );

         yield return CreateSingleTableInfo<ParameterDefinitionPointer, RawParameterDefinitionPointer>(
            Tables.ParameterPtr,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ParameterDefinitionPointerEqualityComparer,
            null,
            () => new ParameterDefinitionPointer(),
            GetParamPtrColumns(),
            () => new RawParameterDefinitionPointer(),
            false
            );

         yield return CreateSingleTableInfo<ParameterDefinition, RawParameterDefinition>(
            Tables.Parameter,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ParameterDefinitionEqualityComparer,
            null,
            () => new ParameterDefinition(),
            GetParamColumns(),
            () => new RawParameterDefinition(),
            false
            );

         yield return CreateSingleTableInfo<InterfaceImplementation, RawInterfaceImplementation>(
            Tables.InterfaceImpl,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.InterfaceImplementationEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.InterfaceImplementationComparer,
            () => new InterfaceImplementation(),
            GetInterfaceImplColumns(),
            () => new RawInterfaceImplementation(),
            true
            );

         yield return CreateSingleTableInfo<MemberReference, RawMemberReference>(
            Tables.MemberRef,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.MemberReferenceEqualityComparer,
            null,
            () => new MemberReference(),
            GetMemberRefColumns(),
            () => new RawMemberReference(),
            false
            );

         yield return CreateSingleTableInfo<ConstantDefinition, RawConstantDefinition>(
            Tables.Constant,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ConstantDefinitionEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.ConstantDefinitionComparer,
            () => new ConstantDefinition(),
            GetConstantColumns(),
            () => new RawConstantDefinition(),
            true
            );

         yield return CreateSingleTableInfo<CustomAttributeDefinition, RawCustomAttributeDefinition>(
            Tables.CustomAttribute,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.CustomAttributeDefinitionEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.CustomAttributeDefinitionComparer,
            () => new CustomAttributeDefinition(),
            GetCustomAttributeColumns(),
            () => new RawCustomAttributeDefinition(),
            true
            );

         yield return CreateSingleTableInfo<FieldMarshal, RawFieldMarshal>(
            Tables.FieldMarshal,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.FieldMarshalEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.FieldMarshalComparer,
            () => new FieldMarshal(),
            GetFieldMarshalColumns(),
            () => new RawFieldMarshal(),
            true
            );

         yield return CreateSingleTableInfo<SecurityDefinition, RawSecurityDefinition>(
            Tables.DeclSecurity,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.SecurityDefinitionEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.SecurityDefinitionComparer,
            () => new SecurityDefinition(),
            GetDeclSecurityColumns(),
            () => new RawSecurityDefinition(),
            true
            );

         yield return CreateSingleTableInfo<ClassLayout, RawClassLayout>(
            Tables.ClassLayout,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ClassLayoutEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.ClassLayoutComparer,
            () => new ClassLayout(),
            GetClassLayoutColumns(),
            () => new RawClassLayout(),
            true
            );

         yield return CreateSingleTableInfo<FieldLayout, RawFieldLayout>(
            Tables.FieldLayout,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.FieldLayoutEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.FieldLayoutComparer,
            () => new FieldLayout(),
            GetFieldLayoutColumns(),
            () => new RawFieldLayout(),
            true
            );

         yield return CreateSingleTableInfo<StandaloneSignature, RawStandaloneSignature>(
            Tables.StandaloneSignature,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.StandaloneSignatureEqualityComparer,
            null,
            () => new StandaloneSignature(),
            GetStandaloneSigColumns(),
            () => new RawStandaloneSignature(),
            false
            );

         yield return CreateSingleTableInfo<EventMap, RawEventMap>(
            Tables.EventMap,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.EventMapEqualityComparer,
            null,
            () => new EventMap(),
            GetEventMapColumns(),
            () => new RawEventMap(),
            true
            );

         yield return CreateSingleTableInfo<EventDefinitionPointer, RawEventDefinitionPointer>(
            Tables.EventPtr,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.EventDefinitionPointerEqualityComparer,
            null,
            () => new EventDefinitionPointer(),
            GetEventPtrColumns(),
            () => new RawEventDefinitionPointer(),
            false
            );

         yield return CreateSingleTableInfo<EventDefinition, RawEventDefinition>(
            Tables.Event,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.EventDefinitionEqualityComparer,
            null,
            () => new EventDefinition(),
            GetEventDefColumns(),
            () => new RawEventDefinition(),
            false
            );

         yield return CreateSingleTableInfo<PropertyMap, RawPropertyMap>(
            Tables.PropertyMap,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.PropertyMapEqualityComparer,
            null,
            () => new PropertyMap(),
            GetPropertyMapColumns(),
            () => new RawPropertyMap(),
            true
            );

         yield return CreateSingleTableInfo<PropertyDefinitionPointer, RawPropertyDefinitionPointer>(
            Tables.PropertyPtr,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.PropertyDefinitionPointerEqualityComparer,
            null,
            () => new PropertyDefinitionPointer(),
            GetPropertyPtrColumns(),
            () => new RawPropertyDefinitionPointer(),
            false
            );

         yield return CreateSingleTableInfo<PropertyDefinition, RawPropertyDefinition>(
            Tables.Property,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.PropertyDefinitionEqualityComparer,
            null,
            () => new PropertyDefinition(),
            GetPropertyDefColumns(),
            () => new RawPropertyDefinition(),
            false
            );

         yield return CreateSingleTableInfo<MethodSemantics, RawMethodSemantics>(
            Tables.MethodSemantics,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.MethodSemanticsEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.MethodSemanticsComparer,
            () => new MethodSemantics(),
            GetMethodSemanticsColumns(),
            () => new RawMethodSemantics(),
            true
            );

         yield return CreateSingleTableInfo<MethodImplementation, RawMethodImplementation>(
            Tables.MethodImpl,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.MethodImplementationEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.MethodImplementationComparer,
            () => new MethodImplementation(),
            GetMethodImplColumns(),
            () => new RawMethodImplementation(),
            true
            );

         yield return CreateSingleTableInfo<ModuleReference, RawModuleReference>(
            Tables.ModuleRef,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ModuleReferenceEqualityComparer,
            null,
            () => new ModuleReference(),
            GetModuleRefColumns(),
            () => new RawModuleReference(),
            false
            );

         yield return CreateSingleTableInfo<TypeSpecification, RawTypeSpecification>(
            Tables.TypeSpec,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.TypeSpecificationEqualityComparer,
            null,
            () => new TypeSpecification(),
            GetTypeSpecColumns(),
            () => new RawTypeSpecification(),
            false
            );

         yield return CreateSingleTableInfo<MethodImplementationMap, RawMethodImplementationMap>(
            Tables.ImplMap,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.MethodImplementationMapEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.MethodImplementationMapComparer,
            () => new MethodImplementationMap(),
            GetImplMapColumns(),
            () => new RawMethodImplementationMap(),
            true
            );

         yield return CreateSingleTableInfo<FieldRVA, RawFieldRVA>(
            Tables.FieldRVA,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.FieldRVAEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.FieldRVAComparer,
            () => new FieldRVA(),
            GetFieldRVAColumns(),
            () => new RawFieldRVA(),
            true
            );

         yield return CreateSingleTableInfo<EditAndContinueLog, RawEditAndContinueLog>(
            Tables.EncLog,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.EditAndContinueLogEqualityComparer,
            null,
            () => new EditAndContinueLog(),
            GetENCLogColumns(),
            () => new RawEditAndContinueLog(),
            false
            );

         yield return CreateSingleTableInfo<EditAndContinueMap, RawEditAndContinueMap>(
            Tables.EncMap,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.EditAndContinueMapEqualityComparer,
            null,
            () => new EditAndContinueMap(),
            GetENCMapColumns(),
            () => new RawEditAndContinueMap(),
            false
            );

         yield return CreateSingleTableInfo<AssemblyDefinition, RawAssemblyDefinition>(
            Tables.Assembly,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.AssemblyDefinitionEqualityComparer,
            null,
            () => new AssemblyDefinition(),
            GetAssemblyDefColumns(),
            () => new RawAssemblyDefinition(),
            false
            );

#pragma warning disable 618

         yield return CreateSingleTableInfo<AssemblyDefinitionProcessor, RawAssemblyDefinitionProcessor>(
            Tables.AssemblyProcessor,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.AssemblyDefinitionProcessorEqualityComparer,
            null,
            () => new AssemblyDefinitionProcessor(),
            GetAssemblyDefProcessorColumns(),
            () => new RawAssemblyDefinitionProcessor(),
            false
            );

         yield return CreateSingleTableInfo<AssemblyDefinitionOS, RawAssemblyDefinitionOS>(
            Tables.AssemblyOS,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.AssemblyDefinitionOSEqualityComparer,
            null,
            () => new AssemblyDefinitionOS(),
            GetAssemblyDefOSColumns(),
            () => new RawAssemblyDefinitionOS(),
            false
            );

#pragma warning restore 618

         yield return CreateSingleTableInfo<AssemblyReference, RawAssemblyReference>(
            Tables.AssemblyRef,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.AssemblyReferenceEqualityComparer,
            null,
            () => new AssemblyReference(),
            GetAssemblyRefColumns(),
            () => new RawAssemblyReference(),
            false
            );

#pragma warning disable 618

         yield return CreateSingleTableInfo<AssemblyReferenceProcessor, RawAssemblyReferenceProcessor>(
            Tables.AssemblyRefProcessor,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.AssemblyReferenceProcessorEqualityComparer,
            null,
            () => new AssemblyReferenceProcessor(),
            GetAssemblyRefProcessorColumns(),
            () => new RawAssemblyReferenceProcessor(),
            false
            );

         yield return CreateSingleTableInfo<AssemblyReferenceOS, RawAssemblyReferenceOS>(
            Tables.AssemblyRefOS,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.AssemblyReferenceOSEqualityComparer,
            null,
            () => new AssemblyReferenceOS(),
            GetAssemblyRefOSColumns(),
            () => new RawAssemblyReferenceOS(),
            false
            );

#pragma warning restore 618

         yield return CreateSingleTableInfo<FileReference, RawFileReference>(
            Tables.File,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.FileReferenceEqualityComparer,
            null,
            () => new FileReference(),
            GetFileColumns(),
            () => new RawFileReference(),
            false
            );

         yield return CreateSingleTableInfo<ExportedType, RawExportedType>(
            Tables.ExportedType,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ExportedTypeEqualityComparer,
            null,
            () => new ExportedType(),
            GetExportedTypeColumns(),
            () => new RawExportedType(),
            false
            );

         yield return CreateSingleTableInfo<ManifestResource, RawManifestResource>(
            Tables.ManifestResource,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.ManifestResourceEqualityComparer,
            null,
            () => new ManifestResource(),
            GetManifestResourceColumns(),
            () => new RawManifestResource(),
            false
            );

         yield return CreateSingleTableInfo<NestedClassDefinition, RawNestedClassDefinition>(
            Tables.NestedClass,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.NestedClassDefinitionEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.NestedClassDefinitionComparer,
            () => new NestedClassDefinition(),
            GetNestedClassColumns(),
            () => new RawNestedClassDefinition(),
            true
            );

         yield return CreateSingleTableInfo<GenericParameterDefinition, RawGenericParameterDefinition>(
            Tables.GenericParameter,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.GenericParameterDefinitionEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.GenericParameterDefinitionComparer,
            () => new GenericParameterDefinition(),
            GetGenericParamColumns(),
            () => new RawGenericParameterDefinition(),
            true
            );

         yield return CreateSingleTableInfo<MethodSpecification, RawMethodSpecification>(
            Tables.MethodSpec,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.MethodSpecificationEqualityComparer,
            null,
            () => new MethodSpecification(),
            GetMethodSpecColumns(),
            () => new RawMethodSpecification(),
            false
            );

         yield return CreateSingleTableInfo<GenericParameterConstraintDefinition, RawGenericParameterConstraintDefinition>(
            Tables.GenericParameterConstraint,
            CAMPhysical::CILAssemblyManipulator.Physical.Comparers.GenericParameterConstraintDefinitionEqualityComparer,
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.GenericParameterConstraintDefinitionComparer,
            () => new GenericParameterConstraintDefinition(),
            GetGenericParamConstraintColumns(),
            () => new RawGenericParameterConstraintDefinition(),
            true
            );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.Module"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.Module"/> table.</returns>
      /// <seealso cref="ModuleDefinition"/>
      /// <seealso cref="RawModuleDefinition"/>
      public static IEnumerable<MetaDataColumnInformation<ModuleDefinition>> GetModuleDefColumns()
      {
         yield return MetaDataColumnInformationFactory.Number16<ModuleDefinition, RawModuleDefinition>( row => row.Generation, ( r, v ) => { r.Generation = v; return true; }, ( r, v ) => r.Generation = v );
         yield return MetaDataColumnInformationFactory.SystemString<ModuleDefinition, RawModuleDefinition>( row => row.Name, ( r, v ) => { r.Name = v; return true; }, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.GUID<ModuleDefinition, RawModuleDefinition>( row => row.ModuleGUID, ( r, v ) => { r.ModuleGUID = v; return true; }, ( r, v ) => r.ModuleGUID = v );
         yield return MetaDataColumnInformationFactory.GUID<ModuleDefinition, RawModuleDefinition>( row => row.EditAndContinueGUID, ( r, v ) => { r.EditAndContinueGUID = v; return true; }, ( r, v ) => r.EditAndContinueGUID = v );
         yield return MetaDataColumnInformationFactory.GUID<ModuleDefinition, RawModuleDefinition>( row => row.EditAndContinueBaseGUID, ( r, v ) => { r.EditAndContinueBaseGUID = v; return true; }, ( r, v ) => r.EditAndContinueBaseGUID = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.TypeRef"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.TypeRef"/> table.</returns>
      /// <seealso cref="TypeReference"/>
      /// <seealso cref="RawTypeReference"/>
      public static IEnumerable<MetaDataColumnInformation<TypeReference>> GetTypeRefColumns()
      {
         yield return MetaDataColumnInformationFactory.CodedTableIndexNullable<TypeReference, RawTypeReference>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.ResolutionScope, row => row.ResolutionScope, ( r, v ) => { r.ResolutionScope = v; return true; }, ( r, v ) => r.ResolutionScope = v );
         yield return MetaDataColumnInformationFactory.SystemString<TypeReference, RawTypeReference>( row => row.Name, ( r, v ) => { r.Name = v; return true; }, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.SystemString<TypeReference, RawTypeReference>( row => row.Namespace, ( r, v ) => { r.Namespace = v; return true; }, ( r, v ) => r.Namespace = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.TypeDef"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.TypeDef"/> table.</returns>
      /// <seealso cref="TypeDefinition"/>
      /// <seealso cref="RawTypeDefinition"/>
      public static IEnumerable<MetaDataColumnInformation<TypeDefinition>> GetTypeDefColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<TypeDefinition, RawTypeDefinition>( row => (Int32) row.Attributes, ( r, v ) => { r.Attributes = (TypeAttributes) v; return true; }, ( r, v ) => r.Attributes = (TypeAttributes) v );
         yield return MetaDataColumnInformationFactory.SystemString<TypeDefinition, RawTypeDefinition>( row => row.Name, ( r, v ) => { r.Name = v; return true; }, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.SystemString<TypeDefinition, RawTypeDefinition>( row => row.Namespace, ( r, v ) => { r.Namespace = v; return true; }, ( r, v ) => r.Namespace = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndexNullable<TypeDefinition, RawTypeDefinition>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.TypeDefOrRef, row => row.BaseType, ( r, v ) => { r.BaseType = v; return true; }, ( r, v ) => r.BaseType = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<TypeDefinition, RawTypeDefinition>( Tables.Field, row => row.FieldList, ( r, v ) => { r.FieldList = v; return true; }, ( r, v ) => r.FieldList = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<TypeDefinition, RawTypeDefinition>( Tables.MethodDef, row => row.MethodList, ( r, v ) => { r.MethodList = v; return true; }, ( r, v ) => r.MethodList = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.FieldPtr"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.FieldPtr"/> table.</returns>
      /// <seealso cref="FieldDefinitionPointer"/>
      /// <seealso cref="RawFieldDefinitionPointer"/>
      public static IEnumerable<MetaDataColumnInformation<FieldDefinitionPointer>> GetFieldPtrColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<FieldDefinitionPointer, RawFieldDefinitionPointer>( Tables.Field, row => row.FieldIndex, ( r, v ) => { r.FieldIndex = v; return true; }, ( r, v ) => r.FieldIndex = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.Field"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.Field"/> table.</returns>
      /// <seealso cref="FieldDefinition"/>
      /// <seealso cref="RawFieldDefinition"/>
      public static IEnumerable<MetaDataColumnInformation<FieldDefinition>> GetFieldDefColumns()
      {
         yield return MetaDataColumnInformationFactory.Number16<FieldDefinition, RawFieldDefinition>( row => (Int16) row.Attributes, ( r, v ) => { r.Attributes = (FieldAttributes) v; return true; }, ( r, v ) => r.Attributes = (FieldAttributes) v );
         yield return MetaDataColumnInformationFactory.SystemString<FieldDefinition, RawFieldDefinition>( row => row.Name, ( r, v ) => { r.Name = v; return true; }, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.BLOBNonTypeSignature<FieldDefinition, RawFieldDefinition, FieldSignature>( row => row.Signature, ( r, v ) => { r.Signature = v; return true; }, ( r, v ) => r.Signature = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.MethodPtr"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.MethodPtr"/> table.</returns>
      /// <seealso cref="MethodDefinitionPointer"/>
      /// <seealso cref="RawMethodDefinitionPointer"/>
      public static IEnumerable<MetaDataColumnInformation<MethodDefinitionPointer>> GetMethodPtrColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<MethodDefinitionPointer, RawMethodDefinitionPointer>( Tables.MethodDef, row => row.MethodIndex, ( r, v ) => { r.MethodIndex = v; return true; }, ( r, v ) => r.MethodIndex = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.MethodDef"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.MethodDef"/> table.</returns>
      /// <seealso cref="MethodDefinition"/>
      /// <seealso cref="RawMethodDefinition"/>
      public static IEnumerable<MetaDataColumnInformation<MethodDefinition>> GetMethodDefColumns()
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
         yield return MetaDataColumnInformationFactory.Number16<MethodDefinition, RawMethodDefinition>( row => (Int16) row.ImplementationAttributes, ( r, v ) => { r.ImplementationAttributes = (MethodImplAttributes) v; return true; }, ( r, v ) => r.ImplementationAttributes = (MethodImplAttributes) v );
         yield return MetaDataColumnInformationFactory.Number16<MethodDefinition, RawMethodDefinition>( row => (Int16) row.Attributes, ( r, v ) => { r.Attributes = (MethodAttributes) v; return true; }, ( r, v ) => r.Attributes = (MethodAttributes) v );
         yield return MetaDataColumnInformationFactory.SystemString<MethodDefinition, RawMethodDefinition>( row => row.Name, ( r, v ) => { r.Name = v; return true; }, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.BLOBNonTypeSignature<MethodDefinition, RawMethodDefinition, MethodDefinitionSignature>( row => row.Signature, ( r, v ) => { r.Signature = v; return true; }, ( r, v ) => r.Signature = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<MethodDefinition, RawMethodDefinition>( Tables.Parameter, row => row.ParameterList, ( r, v ) => { r.ParameterList = v; return true; }, ( r, v ) => r.ParameterList = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.ParameterPtr"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.ParameterPtr"/> table.</returns>
      /// <seealso cref="ParameterDefinitionPointer"/>
      /// <seealso cref="RawParameterDefinitionPointer"/>
      public static IEnumerable<MetaDataColumnInformation<ParameterDefinitionPointer>> GetParamPtrColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<ParameterDefinitionPointer, RawParameterDefinitionPointer>( Tables.Parameter, row => row.ParameterIndex, ( r, v ) => { r.ParameterIndex = v; return true; }, ( r, v ) => r.ParameterIndex = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.Parameter"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.Parameter"/> table.</returns>
      /// <seealso cref="ParameterDefinition"/>
      /// <seealso cref="RawParameterDefinition"/>
      public static IEnumerable<MetaDataColumnInformation<ParameterDefinition>> GetParamColumns()
      {
         yield return MetaDataColumnInformationFactory.Number16<ParameterDefinition, RawParameterDefinition>( row => (Int16) row.Attributes, ( r, v ) => { r.Attributes = (ParameterAttributes) v; return true; }, ( r, v ) => r.Attributes = (ParameterAttributes) v );
         yield return MetaDataColumnInformationFactory.Number16<ParameterDefinition, RawParameterDefinition>( row => (Int16) row.Sequence, ( r, v ) => { r.Sequence = (UInt16) v; return true; }, ( r, v ) => r.Sequence = v );
         yield return MetaDataColumnInformationFactory.SystemString<ParameterDefinition, RawParameterDefinition>( row => row.Name, ( r, v ) => { r.Name = v; return true; }, ( r, v ) => r.Name = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.InterfaceImpl"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.InterfaceImpl"/> table.</returns>
      /// <seealso cref="InterfaceImplementation"/>
      /// <seealso cref="RawInterfaceImplementation"/>
      public static IEnumerable<MetaDataColumnInformation<InterfaceImplementation>> GetInterfaceImplColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<InterfaceImplementation, RawInterfaceImplementation>( Tables.TypeDef, row => row.Class, ( r, v ) => { r.Class = v; return true; }, ( r, v ) => r.Class = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<InterfaceImplementation, RawInterfaceImplementation>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.TypeDefOrRef, row => row.Interface, ( r, v ) => { r.Interface = v; return true; }, ( r, v ) => r.Interface = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.MemberRef"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.MemberRef"/> table.</returns>
      /// <seealso cref="MemberReference"/>
      /// <seealso cref="RawMemberReference"/>
      public static IEnumerable<MetaDataColumnInformation<MemberReference>> GetMemberRefColumns()
      {
         yield return MetaDataColumnInformationFactory.CodedTableIndex<MemberReference, RawMemberReference>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.MemberRefParent, row => row.DeclaringType, ( r, v ) => { r.DeclaringType = v; return true; }, ( r, v ) => r.DeclaringType = v );
         yield return MetaDataColumnInformationFactory.SystemString<MemberReference, RawMemberReference>( row => row.Name, ( r, v ) => { r.Name = v; return true; }, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.BLOBNonTypeSignature<MemberReference, RawMemberReference, AbstractSignature>( row => row.Signature, ( r, v ) => { r.Signature = v; return true; }, ( r, v ) => r.Signature = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.Constant"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.Constant"/> table.</returns>
      /// <seealso cref="ConstantDefinition"/>
      /// <seealso cref="RawConstantDefinition"/>
      public static IEnumerable<MetaDataColumnInformation<ConstantDefinition>> GetConstantColumns()
      {
         yield return MetaDataColumnInformationFactory.Number8<ConstantDefinition, RawConstantDefinition>( row => (Byte) row.Type, ( r, v ) => { r.Type = (ConstantValueType) v; return true; }, ( r, v ) => r.Type = (ConstantValueType) v );
         yield return MetaDataColumnInformationFactory.Number8<ConstantDefinition, RawConstantDefinition>( row => 0, ( r, v ) => { return true; }, ( r, v ) => r.Padding = (Byte) v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<ConstantDefinition, RawConstantDefinition>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.HasConstant, row => row.Parent, ( r, v ) => { r.Parent = v; return true; }, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformationFactory.BLOBCustom<ConstantDefinition, RawConstantDefinition, Object>( ( r, v ) => { r.Value = v; return true; }, r => r.Value, ( r, v ) => r.Value = v, ( args, v, blobs ) => args.Row.Value = blobs.ReadConstantValue( v, args.RowArgs.MetaData.SignatureProvider, args.Row.Type ), args => args.RowArgs.Array.CreateConstantBytes( args.Row.Value, args.Row.Type ), null, null );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.CustomAttribute"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.CustomAttribute"/> table.</returns>
      /// <seealso cref="CustomAttributeDefinition"/>
      /// <seealso cref="RawCustomAttributeDefinition"/>
      public static IEnumerable<MetaDataColumnInformation<CustomAttributeDefinition>> GetCustomAttributeColumns()
      {
         yield return MetaDataColumnInformationFactory.CodedTableIndex<CustomAttributeDefinition, RawCustomAttributeDefinition>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.HasCustomAttribute, row => row.Parent, ( r, v ) => { r.Parent = v; return true; }, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<CustomAttributeDefinition, RawCustomAttributeDefinition>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.CustomAttributeType, row => row.Type, ( r, v ) => { r.Type = v; return true; }, ( r, v ) => r.Type = v );
         yield return MetaDataColumnInformationFactory.BLOBCustom<CustomAttributeDefinition, RawCustomAttributeDefinition, AbstractCustomAttributeSignature>( ( r, v ) => { r.Signature = v; return true; }, r => r.Signature, ( r, v ) => r.Signature = v, ( args, v, blobs ) => args.Row.Signature = blobs.ReadCASignature( v, args.RowArgs.MetaData.SignatureProvider ), args => args.RowArgs.Array.CreateCustomAttributeSignature( args.RowArgs.MetaData, args.RowIndex ), CreateCAColumnSpecificCache, ResolveCASignature );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.FieldMarshal"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.FieldMarshal"/> table.</returns>
      /// <seealso cref="FieldMarshal"/>
      /// <seealso cref="RawFieldMarshal"/>
      public static IEnumerable<MetaDataColumnInformation<FieldMarshal>> GetFieldMarshalColumns()
      {
         yield return MetaDataColumnInformationFactory.CodedTableIndex<FieldMarshal, RawFieldMarshal>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.HasFieldMarshal, row => row.Parent, ( r, v ) => { r.Parent = v; return true; }, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformationFactory.BLOBCustom<FieldMarshal, RawFieldMarshal, AbstractMarshalingInfo>( ( r, v ) => { r.NativeType = v; return true; }, row => row.NativeType, ( r, v ) => r.NativeType = v, ( args, v, blobs ) => args.Row.NativeType = blobs.ReadMarshalingInfo( v, args.RowArgs.MetaData.SignatureProvider ), args => args.RowArgs.Array.CreateMarshalSpec( args.Row.NativeType ), null, null );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.DeclSecurity"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.DeclSecurity"/> table.</returns>
      /// <seealso cref="SecurityDefinition"/>
      /// <seealso cref="RawSecurityDefinition"/>
      public static IEnumerable<MetaDataColumnInformation<SecurityDefinition>> GetDeclSecurityColumns()
      {
         yield return MetaDataColumnInformationFactory.Number16<SecurityDefinition, RawSecurityDefinition>( row => (Int16) row.Action, ( r, v ) => { r.Action = (SecurityAction) v; return true; }, ( r, v ) => r.Action = (SecurityAction) v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<SecurityDefinition, RawSecurityDefinition>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.HasSecurity, row => row.Parent, ( r, v ) => { r.Parent = v; return true; }, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformationFactory.BLOBCustom<SecurityDefinition, RawSecurityDefinition, List<AbstractSecurityInformation>>( ( r, v ) => { r.PermissionSets.Clear(); r.PermissionSets.AddRange( v ); return true; }, row => row.PermissionSets, ( r, v ) => r.PermissionSets = v, ( args, v, blobs ) => blobs.ReadSecurityInformation( v, args.RowArgs.MetaData.SignatureProvider, args.Row.PermissionSets ), args => args.RowArgs.Array.CreateSecuritySignature( args.Row.PermissionSets, args.RowArgs.AuxArray, args.RowArgs.MetaData.SignatureProvider ), CreateCAColumnSpecificCache, ResolveSecurityPermissionSets );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.ClassLayout"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.ClassLayout"/> table.</returns>
      /// <seealso cref="ClassLayout"/>
      /// <seealso cref="RawClassLayout"/>
      public static IEnumerable<MetaDataColumnInformation<ClassLayout>> GetClassLayoutColumns()
      {
         yield return MetaDataColumnInformationFactory.Number16<ClassLayout, RawClassLayout>( row => (Int16) row.PackingSize, ( r, v ) => { r.PackingSize = (UInt16) v; return true; }, ( r, v ) => r.PackingSize = v );
         yield return MetaDataColumnInformationFactory.Number32<ClassLayout, RawClassLayout>( row => row.ClassSize, ( r, v ) => { r.ClassSize = v; return true; }, ( r, v ) => r.ClassSize = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<ClassLayout, RawClassLayout>( Tables.TypeDef, row => row.Parent, ( r, v ) => { r.Parent = v; return true; }, ( r, v ) => r.Parent = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.FieldLayout"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.FieldLayout"/> table.</returns>
      /// <seealso cref="FieldLayout"/>
      /// <seealso cref="RawFieldLayout"/>
      public static IEnumerable<MetaDataColumnInformation<FieldLayout>> GetFieldLayoutColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<FieldLayout, RawFieldLayout>( row => row.Offset, ( r, v ) => { r.Offset = v; return true; }, ( r, v ) => r.Offset = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<FieldLayout, RawFieldLayout>( Tables.Field, row => row.Field, ( r, v ) => { r.Field = v; return true; }, ( r, v ) => r.Field = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.StandaloneSignature"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.StandaloneSignature"/> table.</returns>
      /// <seealso cref="StandaloneSignature"/>
      /// <seealso cref="RawStandaloneSignature"/>
      public static IEnumerable<MetaDataColumnInformation<StandaloneSignature>> GetStandaloneSigColumns()
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

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.EventMap"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.EventMap"/> table.</returns>
      /// <seealso cref="EventMap"/>
      /// <seealso cref="RawEventMap"/>
      public static IEnumerable<MetaDataColumnInformation<EventMap>> GetEventMapColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<EventMap, RawEventMap>( Tables.TypeDef, row => row.Parent, ( r, v ) => { r.Parent = v; return true; }, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<EventMap, RawEventMap>( Tables.Event, row => row.EventList, ( r, v ) => { r.EventList = v; return true; }, ( r, v ) => r.EventList = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.EventPtr"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.EventPtr"/> table.</returns>
      /// <seealso cref="EventDefinitionPointer"/>
      /// <seealso cref="RawEventDefinitionPointer"/>
      public static IEnumerable<MetaDataColumnInformation<EventDefinitionPointer>> GetEventPtrColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<EventDefinitionPointer, RawEventDefinitionPointer>( Tables.Event, row => row.EventIndex, ( r, v ) => { r.EventIndex = v; return true; }, ( r, v ) => r.EventIndex = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.Event"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.Event"/> table.</returns>
      /// <seealso cref="EventDefinition"/>
      /// <seealso cref="RawEventDefinition"/>
      public static IEnumerable<MetaDataColumnInformation<EventDefinition>> GetEventDefColumns()
      {
         yield return MetaDataColumnInformationFactory.Number16<EventDefinition, RawEventDefinition>( row => (Int16) row.Attributes, ( r, v ) => { r.Attributes = (EventAttributes) v; return true; }, ( r, v ) => r.Attributes = (EventAttributes) v );
         yield return MetaDataColumnInformationFactory.SystemString<EventDefinition, RawEventDefinition>( row => row.Name, ( r, v ) => { r.Name = v; return true; }, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<EventDefinition, RawEventDefinition>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.TypeDefOrRef, row => row.EventType, ( r, v ) => { r.EventType = v; return true; }, ( r, v ) => r.EventType = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.PropertyMap"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.PropertyMap"/> table.</returns>
      /// <seealso cref="PropertyMap"/>
      /// <seealso cref="RawPropertyMap"/>
      public static IEnumerable<MetaDataColumnInformation<PropertyMap>> GetPropertyMapColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<PropertyMap, RawPropertyMap>( Tables.TypeDef, row => row.Parent, ( r, v ) => { r.Parent = v; return true; }, ( r, v ) => r.Parent = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<PropertyMap, RawPropertyMap>( Tables.Property, row => row.PropertyList, ( r, v ) => { r.PropertyList = v; return true; }, ( r, v ) => r.PropertyList = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.PropertyPtr"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.PropertyPtr"/> table.</returns>
      /// <seealso cref="PropertyDefinitionPointer"/>
      /// <seealso cref="RawPropertyDefinitionPointer"/>
      public static IEnumerable<MetaDataColumnInformation<PropertyDefinitionPointer>> GetPropertyPtrColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<PropertyDefinitionPointer, RawPropertyDefinitionPointer>( Tables.Property, row => row.PropertyIndex, ( r, v ) => { r.PropertyIndex = v; return true; }, ( r, v ) => r.PropertyIndex = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.Property"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.Property"/> table.</returns>
      /// <seealso cref="PropertyDefinition"/>
      /// <seealso cref="RawPropertyDefinition"/>
      public static IEnumerable<MetaDataColumnInformation<PropertyDefinition>> GetPropertyDefColumns()
      {
         yield return MetaDataColumnInformationFactory.Number16<PropertyDefinition, RawPropertyDefinition>( row => (Int16) row.Attributes, ( r, v ) => { r.Attributes = (PropertyAttributes) v; return true; }, ( r, v ) => r.Attributes = (PropertyAttributes) v );
         yield return MetaDataColumnInformationFactory.SystemString<PropertyDefinition, RawPropertyDefinition>( row => row.Name, ( r, v ) => { r.Name = v; return true; }, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.BLOBNonTypeSignature<PropertyDefinition, RawPropertyDefinition, PropertySignature>( row => row.Signature, ( r, v ) => { r.Signature = v; return true; }, ( r, v ) => r.Signature = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.MethodSemantics"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.MethodSemantics"/> table.</returns>
      /// <seealso cref="MethodSemantics"/>
      /// <seealso cref="RawMethodSemantics"/>
      public static IEnumerable<MetaDataColumnInformation<MethodSemantics>> GetMethodSemanticsColumns()
      {
         yield return MetaDataColumnInformationFactory.Number16<MethodSemantics, RawMethodSemantics>( row => (Int16) row.Attributes, ( r, v ) => { r.Attributes = (MethodSemanticsAttributes) v; return true; }, ( r, v ) => r.Attributes = (MethodSemanticsAttributes) v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<MethodSemantics, RawMethodSemantics>( Tables.MethodDef, row => row.Method, ( r, v ) => { r.Method = v; return true; }, ( r, v ) => r.Method = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<MethodSemantics, RawMethodSemantics>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.HasSemantics, row => row.Associaton, ( r, v ) => { r.Associaton = v; return true; }, ( r, v ) => r.Associaton = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.MethodImpl"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.MethodImpl"/> table.</returns>
      /// <seealso cref="MethodImplementation"/>
      /// <seealso cref="RawMethodImplementation"/>
      public static IEnumerable<MetaDataColumnInformation<MethodImplementation>> GetMethodImplColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<MethodImplementation, RawMethodImplementation>( Tables.TypeDef, row => row.Class, ( r, v ) => { r.Class = v; return true; }, ( r, v ) => r.Class = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<MethodImplementation, RawMethodImplementation>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.MethodDefOrRef, row => row.MethodBody, ( r, v ) => { r.MethodBody = v; return true; }, ( r, v ) => r.MethodBody = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<MethodImplementation, RawMethodImplementation>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.MethodDefOrRef, row => row.MethodDeclaration, ( r, v ) => { r.MethodDeclaration = v; return true; }, ( r, v ) => r.MethodDeclaration = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.ModuleRef"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.ModuleRef"/> table.</returns>
      /// <seealso cref="ModuleReference"/>
      /// <seealso cref="RawModuleReference"/>
      public static IEnumerable<MetaDataColumnInformation<ModuleReference>> GetModuleRefColumns()
      {
         yield return MetaDataColumnInformationFactory.SystemString<ModuleReference, RawModuleReference>( row => row.ModuleName, ( r, v ) => { r.ModuleName = v; return true; }, ( r, v ) => r.ModuleName = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.TypeSpec"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.TypeSpec"/> table.</returns>
      /// <seealso cref="TypeSpecification"/>
      /// <seealso cref="RawTypeSpecification"/>
      public static IEnumerable<MetaDataColumnInformation<TypeSpecification>> GetTypeSpecColumns()
      {
         yield return MetaDataColumnInformationFactory.BLOBTypeSignature<TypeSpecification, RawTypeSpecification>( row => row.Signature, ( r, v ) => { r.Signature = v; return true; }, ( r, v ) => r.Signature = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.ImplMap"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.ImplMap"/> table.</returns>
      /// <seealso cref="MethodImplementationMap"/>
      /// <seealso cref="RawMethodImplementationMap"/>
      public static IEnumerable<MetaDataColumnInformation<MethodImplementationMap>> GetImplMapColumns()
      {
         yield return MetaDataColumnInformationFactory.Number16<MethodImplementationMap, RawMethodImplementationMap>( row => (Int16) row.Attributes, ( r, v ) => { r.Attributes = (PInvokeAttributes) v; return true; }, ( r, v ) => r.Attributes = (PInvokeAttributes) v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<MethodImplementationMap, RawMethodImplementationMap>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.MemberForwarded, row => row.MemberForwarded, ( r, v ) => { r.MemberForwarded = v; return true; }, ( r, v ) => r.MemberForwarded = v );
         yield return MetaDataColumnInformationFactory.SystemString<MethodImplementationMap, RawMethodImplementationMap>( row => row.ImportName, ( r, v ) => { r.ImportName = v; return true; }, ( r, v ) => r.ImportName = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<MethodImplementationMap, RawMethodImplementationMap>( Tables.ModuleRef, row => row.ImportScope, ( r, v ) => { r.ImportScope = v; return true; }, ( r, v ) => r.ImportScope = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.FieldRVA"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.FieldRVA"/> table.</returns>
      /// <seealso cref="FieldRVA"/>
      /// <seealso cref="RawFieldRVA"/>
      public static IEnumerable<MetaDataColumnInformation<FieldRVA>> GetFieldRVAColumns()
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
         }, ( md, mdStreamContainer ) => new SectionPartFunctionality_FieldRVA( md ), null, null );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<FieldRVA, RawFieldRVA>( Tables.Field, row => row.Field, ( r, v ) => { r.Field = v; return true; }, ( r, v ) => r.Field = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.EncLog"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.EncLog"/> table.</returns>
      /// <seealso cref="EditAndContinueLog"/>
      /// <seealso cref="RawEditAndContinueLog"/>
      public static IEnumerable<MetaDataColumnInformation<EditAndContinueLog>> GetENCLogColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<EditAndContinueLog, RawEditAndContinueLog>( row => row.Token, ( r, v ) => { r.Token = v; return true; }, ( r, v ) => r.Token = v );
         yield return MetaDataColumnInformationFactory.Number32<EditAndContinueLog, RawEditAndContinueLog>( row => row.FuncCode, ( r, v ) => { r.FuncCode = v; return true; }, ( r, v ) => r.FuncCode = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.EncMap"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.EncMap"/> table.</returns>
      /// <seealso cref="EditAndContinueMap"/>
      /// <seealso cref="RawEditAndContinueMap"/>
      public static IEnumerable<MetaDataColumnInformation<EditAndContinueMap>> GetENCMapColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<EditAndContinueMap, RawEditAndContinueMap>( row => row.Token, ( r, v ) => { r.Token = v; return true; }, ( r, v ) => r.Token = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.Assembly"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.Assembly"/> table.</returns>
      /// <seealso cref="AssemblyDefinition"/>
      /// <seealso cref="RawAssemblyDefinition"/>
      public static IEnumerable<MetaDataColumnInformation<AssemblyDefinition>> GetAssemblyDefColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<AssemblyDefinition, RawAssemblyDefinition>( row => (Int32) row.HashAlgorithm, ( r, v ) => { r.HashAlgorithm = (AssemblyHashAlgorithm) v; return true; }, ( r, v ) => r.HashAlgorithm = (AssemblyHashAlgorithm) v );
         yield return MetaDataColumnInformationFactory.Number16<AssemblyDefinition, RawAssemblyDefinition>( row => (Int16) row.AssemblyInformation.VersionMajor, ( r, v ) => { r.AssemblyInformation.VersionMajor = (UInt16) v; return true; }, ( r, v ) => r.MajorVersion = v );
         yield return MetaDataColumnInformationFactory.Number16<AssemblyDefinition, RawAssemblyDefinition>( row => (Int16) row.AssemblyInformation.VersionMinor, ( r, v ) => { r.AssemblyInformation.VersionMinor = (UInt16) v; return true; }, ( r, v ) => r.MinorVersion = v );
         yield return MetaDataColumnInformationFactory.Number16<AssemblyDefinition, RawAssemblyDefinition>( row => (Int16) row.AssemblyInformation.VersionBuild, ( r, v ) => { r.AssemblyInformation.VersionBuild = (UInt16) v; return true; }, ( r, v ) => r.BuildNumber = v );
         yield return MetaDataColumnInformationFactory.Number16<AssemblyDefinition, RawAssemblyDefinition>( row => (Int16) row.AssemblyInformation.VersionRevision, ( r, v ) => { r.AssemblyInformation.VersionRevision = (UInt16) v; return true; }, ( r, v ) => r.RevisionNumber = v );
         yield return MetaDataColumnInformationFactory.Number32<AssemblyDefinition, RawAssemblyDefinition>( row => (Int32) row.Attributes, ( r, v ) => { r.Attributes = (AssemblyFlags) v; return true; }, ( r, v ) => r.Attributes = (AssemblyFlags) v );
         yield return MetaDataColumnInformationFactory.BLOBCustom<AssemblyDefinition, RawAssemblyDefinition, Byte[]>( ( r, v ) => { r.AssemblyInformation.PublicKeyOrToken = v; return true; }, r => r.AssemblyInformation.PublicKeyOrToken, ( r, v ) => r.PublicKey = v, ( args, v, blobs ) => args.Row.AssemblyInformation.PublicKeyOrToken = blobs.GetBLOBByteArray( v ), args => args.RowArgs.PublicKey?.ToArray(), null, null );
         //{
         //   var pk = args.Row.AssemblyInformation.PublicKeyOrToken;
         //   return pk.IsNullOrEmpty() ?  : pk;
         //} );
         yield return MetaDataColumnInformationFactory.SystemString<AssemblyDefinition, RawAssemblyDefinition>( row => row.AssemblyInformation.Name, ( r, v ) => { r.AssemblyInformation.Name = v; return true; }, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.SystemString<AssemblyDefinition, RawAssemblyDefinition>( row => row.AssemblyInformation.Culture, ( r, v ) => { r.AssemblyInformation.Culture = v; return true; }, ( r, v ) => r.Culture = v );
      }
#pragma warning disable 618

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.AssemblyProcessor"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.AssemblyProcessor"/> table.</returns>
      /// <seealso cref="AssemblyDefinitionProcessor"/>
      /// <seealso cref="RawAssemblyDefinitionProcessor"/>
      public static IEnumerable<MetaDataColumnInformation<AssemblyDefinitionProcessor>> GetAssemblyDefProcessorColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<AssemblyDefinitionProcessor, RawAssemblyDefinitionProcessor>( row => row.Processor, ( r, v ) => { r.Processor = v; return true; }, ( r, v ) => r.Processor = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.AssemblyOS"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.AssemblyOS"/> table.</returns>
      /// <seealso cref="AssemblyDefinitionOS"/>
      /// <seealso cref="RawAssemblyDefinitionOS"/>
      public static IEnumerable<MetaDataColumnInformation<AssemblyDefinitionOS>> GetAssemblyDefOSColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<AssemblyDefinitionOS, RawAssemblyDefinitionOS>( row => row.OSPlatformID, ( r, v ) => { r.OSPlatformID = v; return true; }, ( r, v ) => r.OSPlatformID = v );
         yield return MetaDataColumnInformationFactory.Number32<AssemblyDefinitionOS, RawAssemblyDefinitionOS>( row => row.OSMajorVersion, ( r, v ) => { r.OSMajorVersion = v; return true; }, ( r, v ) => r.OSMajorVersion = v );
         yield return MetaDataColumnInformationFactory.Number32<AssemblyDefinitionOS, RawAssemblyDefinitionOS>( row => row.OSMinorVersion, ( r, v ) => { r.OSMinorVersion = v; return true; }, ( r, v ) => r.OSMinorVersion = v );
      }

#pragma warning restore 618

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.AssemblyRef"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.AssemblyRef"/> table.</returns>
      /// <seealso cref="AssemblyReference"/>
      /// <seealso cref="RawAssemblyReference"/>
      public static IEnumerable<MetaDataColumnInformation<AssemblyReference>> GetAssemblyRefColumns()
      {
         yield return MetaDataColumnInformationFactory.Number16<AssemblyReference, RawAssemblyReference>( row => (Int16) row.AssemblyInformation.VersionMajor, ( r, v ) => { r.AssemblyInformation.VersionMajor = (UInt16) v; return true; }, ( r, v ) => r.MajorVersion = v );
         yield return MetaDataColumnInformationFactory.Number16<AssemblyReference, RawAssemblyReference>( row => (Int16) row.AssemblyInformation.VersionMinor, ( r, v ) => { r.AssemblyInformation.VersionMinor = (UInt16) v; return true; }, ( r, v ) => r.MinorVersion = v );
         yield return MetaDataColumnInformationFactory.Number16<AssemblyReference, RawAssemblyReference>( row => (Int16) row.AssemblyInformation.VersionBuild, ( r, v ) => { r.AssemblyInformation.VersionBuild = (UInt16) v; return true; }, ( r, v ) => r.BuildNumber = v );
         yield return MetaDataColumnInformationFactory.Number16<AssemblyReference, RawAssemblyReference>( row => (Int16) row.AssemblyInformation.VersionRevision, ( r, v ) => { r.AssemblyInformation.VersionRevision = (UInt16) v; return true; }, ( r, v ) => r.RevisionNumber = v );
         yield return MetaDataColumnInformationFactory.Number32<AssemblyReference, RawAssemblyReference>( row => (Int32) row.Attributes, ( r, v ) => { r.Attributes = (AssemblyFlags) v; return true; }, ( r, v ) => r.Attributes = (AssemblyFlags) v );
         yield return MetaDataColumnInformationFactory.BLOBByteArray<AssemblyReference, RawAssemblyReference>( r => r.AssemblyInformation.PublicKeyOrToken, ( r, v ) => { r.AssemblyInformation.PublicKeyOrToken = v; return true; }, ( r, v ) => r.PublicKeyOrToken = v );
         yield return MetaDataColumnInformationFactory.SystemString<AssemblyReference, RawAssemblyReference>( row => row.AssemblyInformation.Name, ( r, v ) => { r.AssemblyInformation.Name = v; return true; }, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.SystemString<AssemblyReference, RawAssemblyReference>( row => row.AssemblyInformation.Culture, ( r, v ) => { r.AssemblyInformation.Culture = v; return true; }, ( r, v ) => r.Culture = v );
         yield return MetaDataColumnInformationFactory.BLOBByteArray<AssemblyReference, RawAssemblyReference>( row => row.HashValue, ( r, v ) => { r.HashValue = v; return true; }, ( r, v ) => r.HashValue = v );
      }

#pragma warning disable 618

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.AssemblyRefProcessor"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.AssemblyRefProcessor"/> table.</returns>
      /// <seealso cref="AssemblyReferenceProcessor"/>
      /// <seealso cref="RawAssemblyReferenceProcessor"/>
      public static IEnumerable<MetaDataColumnInformation<AssemblyReferenceProcessor>> GetAssemblyRefProcessorColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<AssemblyReferenceProcessor, RawAssemblyReferenceProcessor>( row => row.Processor, ( r, v ) => { r.Processor = v; return true; }, ( r, v ) => r.Processor = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<AssemblyReferenceProcessor, RawAssemblyReferenceProcessor>( Tables.AssemblyRef, row => row.AssemblyRef, ( r, v ) => { r.AssemblyRef = v; return true; }, ( r, v ) => r.AssemblyRef = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.AssemblyRefOS"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.AssemblyRefOS"/> table.</returns>
      /// <seealso cref="AssemblyReferenceOS"/>
      /// <seealso cref="RawAssemblyReferenceOS"/>
      public static IEnumerable<MetaDataColumnInformation<AssemblyReferenceOS>> GetAssemblyRefOSColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<AssemblyReferenceOS, RawAssemblyReferenceOS>( row => row.OSPlatformID, ( r, v ) => { r.OSPlatformID = v; return true; }, ( r, v ) => r.OSPlatformID = v );
         yield return MetaDataColumnInformationFactory.Number32<AssemblyReferenceOS, RawAssemblyReferenceOS>( row => row.OSMajorVersion, ( r, v ) => { r.OSMajorVersion = v; return true; }, ( r, v ) => r.OSMajorVersion = v );
         yield return MetaDataColumnInformationFactory.Number32<AssemblyReferenceOS, RawAssemblyReferenceOS>( row => row.OSMinorVersion, ( r, v ) => { r.OSMinorVersion = v; return true; }, ( r, v ) => r.OSMinorVersion = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<AssemblyReferenceOS, RawAssemblyReferenceOS>( Tables.AssemblyRef, row => row.AssemblyRef, ( r, v ) => { r.AssemblyRef = v; return true; }, ( r, v ) => r.AssemblyRef = v );
      }

#pragma warning restore 618

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.File"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.File"/> table.</returns>
      /// <seealso cref="FileReference"/>
      /// <seealso cref="RawFileReference"/>
      public static IEnumerable<MetaDataColumnInformation<FileReference>> GetFileColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<FileReference, RawFileReference>( row => (Int32) row.Attributes, ( r, v ) => { r.Attributes = (FileAttributes) v; return true; }, ( r, v ) => r.Attributes = (FileAttributes) v );
         yield return MetaDataColumnInformationFactory.SystemString<FileReference, RawFileReference>( row => row.Name, ( r, v ) => { r.Name = v; return true; }, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.BLOBByteArray<FileReference, RawFileReference>( row => row.HashValue, ( r, v ) => { r.HashValue = v; return true; }, ( r, v ) => r.HashValue = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.ExportedType"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.ExportedType"/> table.</returns>
      /// <seealso cref="ExportedType"/>
      /// <seealso cref="RawExportedType"/>
      public static IEnumerable<MetaDataColumnInformation<ExportedType>> GetExportedTypeColumns()
      {
         yield return MetaDataColumnInformationFactory.Number32<ExportedType, RawExportedType>( row => (Int32) row.Attributes, ( r, v ) => { r.Attributes = (TypeAttributes) v; return true; }, ( r, v ) => r.Attributes = (TypeAttributes) v );
         yield return MetaDataColumnInformationFactory.Number32<ExportedType, RawExportedType>( row => row.TypeDefinitionIndex, ( r, v ) => { r.TypeDefinitionIndex = v; return true; }, ( r, v ) => r.TypeDefinitionIndex = v );
         yield return MetaDataColumnInformationFactory.SystemString<ExportedType, RawExportedType>( row => row.Name, ( r, v ) => { r.Name = v; return true; }, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.SystemString<ExportedType, RawExportedType>( row => row.Namespace, ( r, v ) => { r.Namespace = v; return true; }, ( r, v ) => r.Namespace = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<ExportedType, RawExportedType>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.Implementation, row => row.Implementation, ( r, v ) => { r.Implementation = v; return true; }, ( r, v ) => r.Implementation = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.ManifestResource"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.ManifestResource"/> table.</returns>
      /// <seealso cref="ManifestResource"/>
      /// <seealso cref="RawManifestResource"/>
      public static IEnumerable<MetaDataColumnInformation<ManifestResource>> GetManifestResourceColumns()
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
         ( md, mdStreamContainer ) => new SectionPartFunctionality_EmbeddedManifests( md ), null, null );
         yield return MetaDataColumnInformationFactory.Number32<ManifestResource, RawManifestResource>( row => (Int32) row.Attributes, ( r, v ) => { r.Attributes = (ManifestResourceAttributes) v; return true; }, ( r, v ) => r.Attributes = (ManifestResourceAttributes) v );
         yield return MetaDataColumnInformationFactory.SystemString<ManifestResource, RawManifestResource>( row => row.Name, ( r, v ) => { r.Name = v; return true; }, ( r, v ) => r.Name = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndexNullable<ManifestResource, RawManifestResource>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.Implementation, row => row.Implementation, ( r, v ) => { r.Implementation = v; return true; }, ( r, v ) => r.Implementation = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.NestedClass"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.NestedClass"/> table.</returns>
      /// <seealso cref="NestedClassDefinition"/>
      /// <seealso cref="RawNestedClassDefinition"/>
      public static IEnumerable<MetaDataColumnInformation<NestedClassDefinition>> GetNestedClassColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<NestedClassDefinition, RawNestedClassDefinition>( Tables.TypeDef, row => row.NestedClass, ( r, v ) => { r.NestedClass = v; return true; }, ( r, v ) => r.NestedClass = v );
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<NestedClassDefinition, RawNestedClassDefinition>( Tables.TypeDef, row => row.EnclosingClass, ( r, v ) => { r.EnclosingClass = v; return true; }, ( r, v ) => r.EnclosingClass = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.GenericParameter"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.GenericParameter"/> table.</returns>
      /// <seealso cref="GenericParameterDefinition"/>
      /// <seealso cref="RawGenericParameterDefinition"/>
      public static IEnumerable<MetaDataColumnInformation<GenericParameterDefinition>> GetGenericParamColumns()
      {
         yield return MetaDataColumnInformationFactory.Number16<GenericParameterDefinition, RawGenericParameterDefinition>( row => (Int16) row.GenericParameterIndex, ( r, v ) => { r.GenericParameterIndex = (UInt16) v; return true; }, ( r, v ) => r.GenericParameterIndex = v );
         yield return MetaDataColumnInformationFactory.Number16<GenericParameterDefinition, RawGenericParameterDefinition>( row => (Int16) row.Attributes, ( r, v ) => { r.Attributes = (GenericParameterAttributes) v; return true; }, ( r, v ) => r.Attributes = (GenericParameterAttributes) v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<GenericParameterDefinition, RawGenericParameterDefinition>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.TypeOrMethodDef, row => row.Owner, ( r, v ) => { r.Owner = v; return true; }, ( r, v ) => r.Owner = v );
         yield return MetaDataColumnInformationFactory.SystemString<GenericParameterDefinition, RawGenericParameterDefinition>( row => row.Name, ( r, v ) => { r.Name = v; return true; }, ( r, v ) => r.Name = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.MethodSpec"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.MethodSpec"/> table.</returns>
      /// <seealso cref="MethodSpecification"/>
      /// <seealso cref="RawMethodSpecification"/>
      public static IEnumerable<MetaDataColumnInformation<MethodSpecification>> GetMethodSpecColumns()
      {
         yield return MetaDataColumnInformationFactory.CodedTableIndex<MethodSpecification, RawMethodSpecification>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.MethodDefOrRef, row => row.Method, ( r, v ) => { r.Method = v; return true; }, ( r, v ) => r.Method = v );
         yield return MetaDataColumnInformationFactory.BLOBNonTypeSignature<MethodSpecification, RawMethodSpecification, GenericMethodSignature>( row => row.Signature, ( r, v ) => { r.Signature = v; return true; }, ( r, v ) => r.Signature = v );
      }

      /// <summary>
      /// Returns the enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.GenericParameterConstraint"/> table.
      /// </summary>
      /// <returns>The enumerable of <see cref="MetaDataColumnInformation{TRow, TValue}"/>s for <see cref="Tables.GenericParameterConstraint"/> table.</returns>
      /// <seealso cref="GenericParameterConstraintDefinition"/>
      /// <seealso cref="RawGenericParameterConstraintDefinition"/>
      public static IEnumerable<MetaDataColumnInformation<GenericParameterConstraintDefinition>> GetGenericParamConstraintColumns()
      {
         yield return MetaDataColumnInformationFactory.SimpleTableIndex<GenericParameterConstraintDefinition, RawGenericParameterConstraintDefinition>( Tables.GenericParameter, row => row.Owner, ( r, v ) => { r.Owner = v; return true; }, ( r, v ) => r.Owner = v );
         yield return MetaDataColumnInformationFactory.CodedTableIndex<GenericParameterConstraintDefinition, RawGenericParameterConstraintDefinition>( CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.TypeDefOrRef, row => row.Constraint, ( r, v ) => { r.Constraint = v; return true; }, ( r, v ) => r.Constraint = v );
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

   /// <summary>
   /// This delegate a way to create <see cref="TableSerializationLogicalFunctionality"/> without the knowledge of generic arguments of <see cref="MetaDataTableInformation{TRow}"/>.
   /// </summary>
   /// <remarks>
   /// This delegate is accessible through (extension) methods for <see cref="ExtensionByCompositionProvider{TFunctionality}"/> of <see cref="MetaDataTableInformation"/>.
   /// </remarks>
   public delegate TableSerializationLogicalFunctionality MetaDataTableInformationWithSerializationCapabilityDelegate( TableSerializationLogicalFunctionalityCreationArgs args );

   /// <summary>
   /// This struct defines all data that is used to create <see cref="TableSerializationLogicalFunctionality"/> by <see cref="MetaDataTableInformationWithSerializationCapabilityDelegate"/>.
   /// </summary>
   public struct TableSerializationLogicalFunctionalityCreationArgs
   {
      /// <summary>
      /// Creates a new <see cref="TableSerializationLogicalFunctionalityCreationArgs"/> with given parameters.
      /// </summary>
      /// <param name="errorHandler">The error handler, as specified in <see cref="CAMPhysicalIO::E_CILPhysical.WriteMetaDataToStream(WriterFunctionality, System.IO.Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, Crypto.CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/> or <see cref="CAMPhysicalIO::E_CILPhysical.ReadMetaDataFromStream(ReaderFunctionality, System.IO.Stream, EventHandler{SerializationErrorEventArgs}, bool, out ImageInformation)"/> method.</param>
      public TableSerializationLogicalFunctionalityCreationArgs( EventHandler<SerializationErrorEventArgs> errorHandler )
      {
         this.ErrorHandler = errorHandler;
      }

      /// <summary>
      /// Gets the callback for handling errors during (de)serialization process.
      /// </summary>
      /// <value>The callback for handling errors during (de)serialization process.</value>
      /// <remarks>
      /// This value may be <c>null</c>.
      /// </remarks>
      public EventHandler<SerializationErrorEventArgs> ErrorHandler { get; }
   }

   /// <summary>
   /// This static class should be used to create information about columns for <see cref="MetaDataTableInformation{TRow}"/> constructor.
   /// </summary>
   public static class MetaDataColumnInformationFactory
   {

      /// <summary>
      /// This method creates the appropriate <see cref="MetaDataColumnInformation{TRow, TValue}"/> for column, which is serialized using one byte.
      /// </summary>
      /// <typeparam name="TRow">The type of the row.</typeparam>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <param name="getter">The callback to get the byte value from row.</param>
      /// <param name="setter">The callback to set the byte value on row.</param>
      /// <param name="rawSetter">The callback to set the byte value on raw row.</param>
      /// <returns>A new instance of <see cref="MetaDataColumnInformation{TRow, TValue}"/> with given information.</returns>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="getter"/> or <paramref name="setter"/> is <c>null</c>.</exception>
      /// <seealso cref="ConstantCustom"/>
      /// <seealso cref="GenericColumn"/>
      public static MetaDataColumnInformation<TRow, Byte> Number8<TRow, TRawRow>(
         RowColumnGetterDelegate<TRow, Byte> getter,
         RowColumnSetterDelegate<TRow, Byte> setter,
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

      /// <summary>
      /// This method creates the appropriate <see cref="MetaDataColumnInformation{TRow, TValue}"/> for column, which is serialized using two bytes.
      /// </summary>
      /// <typeparam name="TRow">The type of the row.</typeparam>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <param name="getter">The callback to get the (unsigned) short value from row.</param>
      /// <param name="setter">The callback to set the (unsigned) short value on row.</param>
      /// <param name="rawSetter">The callback to set the (unsigned) short value on raw row.</param>
      /// <returns>A new instance of <see cref="MetaDataColumnInformation{TRow, TValue}"/> with given information.</returns>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="getter"/> or <paramref name="setter"/> is <c>null</c>.</exception>
      /// <seealso cref="ConstantCustom"/>
      /// <seealso cref="GenericColumn"/>
      public static MetaDataColumnInformation<TRow, Int16> Number16<TRow, TRawRow>(
         RowColumnGetterDelegate<TRow, Int16> getter,
         RowColumnSetterDelegate<TRow, Int16> setter,
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

      /// <summary>
      /// This method creates the appropriate <see cref="MetaDataColumnInformation{TRow, TValue}"/> for column, which is serialized using four bytes.
      /// </summary>
      /// <typeparam name="TRow">The type of the row.</typeparam>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <param name="getter">The callback to get the (unsigned) int value from row.</param>
      /// <param name="setter">The callback to set the (unsigned) int value on row.</param>
      /// <param name="rawSetter">The callback to set the (unsigned) int value on raw row.</param>
      /// <returns>A new instance of <see cref="MetaDataColumnInformation{TRow, TValue}"/> with given information.</returns>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="getter"/> or <paramref name="setter"/> is <c>null</c>.</exception>
      /// <seealso cref="ConstantCustom"/>
      /// <seealso cref="GenericColumn"/>
      public static MetaDataColumnInformation<TRow, Int32> Number32<TRow, TRawRow>(
         RowColumnGetterDelegate<TRow, Int32> getter,
         RowColumnSetterDelegate<TRow, Int32> setter,
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

      /// <summary>
      /// This method creates the appropriate <see cref="MetaDataColumnInformation{TRow, TValue}"/> for column, which is serialized using constant amount of bytes.
      /// </summary>
      /// <typeparam name="TRow">The type of the row.</typeparam>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <typeparam name="TValue">The type of the value.</typeparam>
      /// <param name="byteCount">The amount of bytes to use when serializing.</param>
      /// <param name="setter">The callback to set the value on row.</param>
      /// <param name="getter">The callback to get the value from row.</param>
      /// <param name="serializationInfo">The callback to create <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/>.</param>
      /// <returns>A new instance of <see cref="MetaDataColumnInformation{TRow, TValue}"/> with given information.</returns>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="getter"/> or <paramref name="setter"/> is <c>null</c>.</exception>
      /// <seealso cref="GenericColumn"/>
      public static MetaDataColumnInformation<TRow, TValue> ConstantCustom<TRow, TRawRow, TValue>(
         Int32 byteCount,
         RowColumnGetterDelegate<TRow, TValue> getter,
         RowColumnSetterDelegate<TRow, TValue> setter,
         Func<DefaultColumnSerializationInfo<TRow, TRawRow>> serializationInfo
         )
         where TRow : class
         where TRawRow : class
         where TValue : struct
      {
         return GenericColumn(
            getter,
            setter,
            null,
            null,
            serializationInfo
            );
      }

      /// <summary>
      /// This method creates the appropriate <see cref="MetaDataColumnInformation{TRow, TValue}"/> for column, which is serialized as reference to system string meta data stream.
      /// </summary>
      /// <typeparam name="TRow">The type of the row.</typeparam>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <param name="setter">The callback to set the value on row.</param>
      /// <param name="getter">The callback to get the value from row.</param>
      /// <param name="rawSetter">The callback to set the value on raw row.</param>
      /// <returns>A new instance of <see cref="MetaDataColumnInformation{TRow, TValue}"/> with given information.</returns>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="getter"/> or <paramref name="setter"/> is <c>null</c>.</exception>
      /// <seealso cref="GenericColumn"/>
      public static MetaDataColumnInformation<TRow, String> SystemString<TRow, TRawRow>(
         RowColumnGetterDelegate<TRow, String> getter,
         RowColumnSetterDelegate<TRow, String> setter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return GenericColumn(
            getter,
            setter,
            null,
            null,
            () => DefaultColumnSerializationInfoFactory.SystemString( rawSetter, setter, getter )
            );
      }

      /// <summary>
      /// This method creates the appropriate <see cref="MetaDataColumnInformation{TRow, TValue}"/> for column, which is serialized as reference to GUID meta data stream.
      /// </summary>
      /// <typeparam name="TRow">The type of the row.</typeparam>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <param name="getter">The callback to get the value from row.</param>
      /// <param name="setter">The callback to set the value on row.</param>
      /// <param name="rawSetter">The callback to set the value on raw row.</param>
      /// <returns>A new instance of <see cref="MetaDataColumnInformation{TRow, TValue}"/> with given information.</returns>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="getter"/> or <paramref name="setter"/> is <c>null</c>.</exception>
      /// <seealso cref="GenericColumn"/>
      public static MetaDataColumnInformation<TRow, Guid?> GUID<TRow, TRawRow>(
         RowColumnGetterDelegate<TRow, Guid?> getter,
         RowColumnSetterDelegate<TRow, Guid?> setter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return GenericColumn(
            getter,
            setter,
            null,
            null,
            () => DefaultColumnSerializationInfoFactory.GUID( rawSetter, setter, getter )
            );
      }

      /// <summary>
      /// This method creates the appropriate <see cref="MetaDataColumnInformation{TRow, TValue}"/> for column, which is <see cref="TableIndex"/> into one pre-defined table, and serialized using variable amount of bytes.
      /// </summary>
      /// <typeparam name="TRow">The type of the row.</typeparam>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <param name="targetTable">The table ID where the table indices point to, as <see cref="Tables"/> enumeration.</param>
      /// <param name="getter">The callback to get the value from row.</param>
      /// <param name="setter">The callback to set the value on row.</param>
      /// <param name="rawSetter">The callback to set the value on raw row.</param>
      /// <returns>A new instance of <see cref="MetaDataColumnInformation{TRow, TValue}"/> with given information.</returns>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="getter"/>, <paramref name="setter"/>, or <paramref name="rawSetter"/> is <c>null</c>.</exception>
      /// <seealso cref="GenericColumn"/>
      public static MetaDataColumnInformation<TRow, TableIndex> SimpleTableIndex<TRow, TRawRow>(
         Tables targetTable,
         RowColumnGetterDelegate<TRow, TableIndex> getter,
         RowColumnSetterDelegate<TRow, TableIndex> setter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return GenericColumn(
            getter,
            setter,
            null,
            null,
            () => DefaultColumnSerializationInfoFactory.SimpleReference( targetTable, rawSetter, setter, getter )
            );
      }

      /// <summary>
      /// This method creates the appropriate <see cref="MetaDataColumnInformation{TRow, TValue}"/> for column, which is <see cref="TableIndex"/> into several possible pre-defined tables, and serialized using variable amount of bytes.
      /// </summary>
      /// <typeparam name="TRow">The type of the row.</typeparam>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <param name="targetTables">The table IDs where the table indices can point to, as array of nullable integer values of <see cref="Tables"/> enumeration.</param>
      /// <param name="getter">The callback to get the value from row.</param>
      /// <param name="setter">The callback to set the value on row.</param>
      /// <param name="rawSetter">The callback to set the value on raw row.</param>
      /// <returns>A new instance of <see cref="MetaDataColumnInformation{TRow, TValue}"/> with given information.</returns>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="targetTables"/>, <paramref name="getter"/>, <paramref name="setter"/>, or <paramref name="rawSetter"/> is <c>null</c>.</exception>
      /// <seealso cref="GenericColumn"/>
      /// <seealso cref="CAMPhysicalIO::CILAssemblyManipulator.Physical.CodedTableIndexComparer"/>
      public static MetaDataColumnInformation<TRow, TableIndex> CodedTableIndex<TRow, TRawRow>(
         ArrayQuery<Int32?> targetTables,
         RowColumnGetterDelegate<TRow, TableIndex> getter,
         RowColumnSetterDelegate<TRow, TableIndex> setter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return GenericColumn(
            getter,
            setter,
            null,
            null,
            () => DefaultColumnSerializationInfoFactory.CodedReference<TRow, TRawRow>( targetTables, rawSetter, ( row, v ) => setter( row, v.GetValueOrDefault() ), r => getter( r ) )
            );
      }

      /// <summary>
      /// This method creates the appropriate <see cref="MetaDataColumnInformation{TRow, TValue}"/> for column, which is nullable <see cref="TableIndex"/> into several possible pre-defined tables, and serialized using variable amount of bytes.
      /// </summary>
      /// <typeparam name="TRow">The type of the row.</typeparam>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <param name="targetTables">The table IDs where the table indices can point to, as array of nullable integer values of <see cref="Tables"/> enumeration.</param>
      /// <param name="getter">The callback to get the value from row.</param>
      /// <param name="setter">The callback to set the value on row.</param>
      /// <param name="rawSetter">The callback to set the value on raw row.</param>
      /// <returns>A new instance of <see cref="MetaDataColumnInformation{TRow, TValue}"/> with given information.</returns>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="targetTables"/>, <paramref name="getter"/>, <paramref name="setter"/>, or <paramref name="rawSetter"/> is <c>null</c>.</exception>
      /// <seealso cref="GenericColumn"/>
      /// <seealso cref="CAMPhysicalIO::CILAssemblyManipulator.Physical.CodedTableIndexComparer"/>
      public static MetaDataColumnInformation<TRow, TableIndex?> CodedTableIndexNullable<TRow, TRawRow>(
         ArrayQuery<Int32?> targetTables,
         RowColumnGetterDelegate<TRow, TableIndex?> getter,
         RowColumnSetterDelegate<TRow, TableIndex?> setter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter
         )
         where TRow : class
         where TRawRow : class
      {
         return GenericColumn(
            getter,
            setter,
            null,
            null,
            () => DefaultColumnSerializationInfoFactory.CodedReference( targetTables, rawSetter, setter, getter )
            );
      }

      /// <summary>
      /// This method creates the appropriate <see cref="MetaDataColumnInformation{TRow, TValue}"/> for column, which is <see cref="TypeSignature"/> and serialized as index to BLOB meta data stream.
      /// </summary>
      /// <typeparam name="TRow">The type of the row.</typeparam>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <param name="getter">The callback to get the value from row.</param>
      /// <param name="setter">The callback to set the value on row.</param>
      /// <param name="rawSetter">The callback to set the value on raw row.</param>
      /// <returns>A new instance of <see cref="MetaDataColumnInformation{TRow, TValue}"/> with given information.</returns>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="getter"/>, <paramref name="setter"/>, or <paramref name="rawSetter"/> is <c>null</c>.</exception>
      /// <seealso cref="BLOBCustom"/>
      /// <seealso cref="GenericColumn"/>
      public static MetaDataColumnInformation<TRow, TypeSignature> BLOBTypeSignature<TRow, TRawRow>(
         RowColumnGetterDelegate<TRow, TypeSignature> getter,
         RowColumnSetterDelegate<TRow, TypeSignature> setter,
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

      /// <summary>
      /// This method creates the appropriate <see cref="MetaDataColumnInformation{TRow, TValue}"/> for column, which is subclass of <see cref="AbstractSignature"/>, but not of <see cref="TypeSignature"/>, and is serialized as index to BLOB meta data stream.
      /// </summary>
      /// <typeparam name="TRow">The type of the row.</typeparam>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <typeparam name="TSignature">The type of the signature.</typeparam>
      /// <param name="getter">The callback to get the value from row.</param>
      /// <param name="setter">The callback to set the value on row.</param>
      /// <param name="rawSetter">The callback to set the value on raw row.</param>
      /// <returns>A new instance of <see cref="MetaDataColumnInformation{TRow, TValue}"/> with given information.</returns>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="getter"/>, <paramref name="setter"/>, or <paramref name="rawSetter"/> is <c>null</c>.</exception>
      /// <seealso cref="BLOBCustom"/>
      /// <seealso cref="GenericColumn"/>
      public static MetaDataColumnInformation<TRow, TSignature> BLOBNonTypeSignature<TRow, TRawRow, TSignature>(
         RowColumnGetterDelegate<TRow, TSignature> getter,
         RowColumnSetterDelegate<TRow, TSignature> setter,
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

      /// <summary>
      /// This method creates the appropriate <see cref="MetaDataColumnInformation{TRow, TValue}"/> for column, which is byte array, and is serialized as index to BLOB meta data stream.
      /// </summary>
      /// <typeparam name="TRow">The type of the row.</typeparam>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <param name="getter">The callback to get the value from row.</param>
      /// <param name="setter">The callback to set the value on row.</param>
      /// <param name="rawSetter">The callback to set the value on raw row.</param>
      /// <returns>A new instance of <see cref="MetaDataColumnInformation{TRow, TValue}"/> with given information.</returns>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="getter"/>, <paramref name="setter"/>, or <paramref name="rawSetter"/> is <c>null</c>.</exception>
      /// <seealso cref="BLOBCustom"/>
      /// <seealso cref="GenericColumn"/>
      public static MetaDataColumnInformation<TRow, Byte[]> BLOBByteArray<TRow, TRawRow>(
         RowColumnGetterDelegate<TRow, Byte[]> getter,
         RowColumnSetterDelegate<TRow, Byte[]> setter,
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

      /// <summary>
      /// This method creates the appropriate <see cref="MetaDataColumnInformation{TRow, TValue}"/> for column, which is of given type, and is serialized as index to BLOB meta data stream.
      /// </summary>
      /// <typeparam name="TRow">The type of the row.</typeparam>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <typeparam name="TValue">The type of the colum nvalue.</typeparam>
      /// <param name="getter">The callback to get the value from row.</param>
      /// <param name="setter">The callback to set the value on row.</param>
      /// <param name="rawSetter">The callback to set the value on raw row.</param>
      /// <param name="blobReader">The callback to read the value from <see cref="ReaderBLOBStreamHandler"/>.</param>
      /// <param name="blobCreator">The callback to create a byte array from the column value.</param>
      /// <param name="resolver">The optional callback to resolve the column value.</param>
      /// <param name="resolverCacheCreator">The optional callback to create a cache object for resolving, in a scope of single <see cref="CILMetaData"/>.</param>
      /// <returns>A new instance of <see cref="MetaDataColumnInformation{TRow, TValue}"/> with given information.</returns>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="getter"/>, <paramref name="setter"/>, <paramref name="rawSetter"/>, <paramref name="blobReader"/>, or <paramref name="blobCreator"/> is <c>null</c>.</exception>
      /// <seealso cref="GenericColumn"/>
      public static MetaDataColumnInformation<TRow, TValue> BLOBCustom<TRow, TRawRow, TValue>(
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
         return GenericColumn(
            getter,
            setter,
            resolverCacheCreator,
            resolver,
            () => DefaultColumnSerializationInfoFactory.BLOB( rawSetter, blobReader, blobCreator )
            );
      }

      /// <summary>
      /// This method creates the appropriate <see cref="MetaDataColumnInformation{TRow, TValue}"/> for column, which is of given type, and is serialized as four byte integer, which is a reference to data outside the meta data chunk of the image. 
      /// </summary>
      /// <typeparam name="TRow">The type of the row.</typeparam>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <typeparam name="TValue">The type of the colum nvalue.</typeparam>
      /// <param name="getter">The callback to get the value from row.</param>
      /// <param name="setter">The callback to set the value on row.</param>
      /// <param name="rawSetter">The callback to set the value on raw row.</param>
      /// <param name="dataReferenceSetter">The callback to deserialize and set the value from given data reference.</param>
      /// <param name="dataReferenceTargetSectionCreator">The callback to create <see cref="SectionPartFunctionalityWithDataReferenceTargets"/> to store serialized value of the column.</param>
      /// <param name="resolver">The optional callback to resolve the column value.</param>
      /// <param name="resolverCacheCreator">The optional callback to create a cache object for resolving, in a scope of single <see cref="CILMetaData"/>.</param>
      /// <returns>A new instance of <see cref="MetaDataColumnInformation{TRow, TValue}"/> with given information.</returns>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="getter"/>, <paramref name="setter"/>, <paramref name="rawSetter"/>, <paramref name="dataReferenceSetter"/>, or <paramref name="dataReferenceTargetSectionCreator"/> is <c>null</c>.</exception>
      /// <seealso cref="GenericColumn"/>
      public static MetaDataColumnInformation<TRow, TValue> DataReference<TRow, TRawRow, TValue>(
         RowColumnSetterDelegate<TRow, TValue> setter,
         RowColumnGetterDelegate<TRow, TValue> getter,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnDataReferenceSetterDelegate<TRow> dataReferenceSetter,
         DataReferenceColumnSectionPartCreationDelegate<TRow> dataReferenceTargetSectionCreator,
         ResolverCacheCreatorDelegate resolverCacheCreator,
         ResolverDelegate resolver
         )
         where TRow : class
         where TRawRow : class
      {
         return GenericColumn(
            getter,
            setter,
            resolverCacheCreator,
            resolver,
            () => DefaultColumnSerializationInfoFactory.DataReferenceColumn( rawSetter, dataReferenceSetter, dataReferenceTargetSectionCreator )
            );
      }

      /// <summary>
      /// This is method to create a generic column useable with IO and resolving capabilities within CAM.Physical framework.
      /// </summary>
      /// <typeparam name="TRow">The type of the row.</typeparam>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <typeparam name="TValue">The type of the column value.</typeparam>
      /// <param name="getter">The callback to get the value.</param>
      /// <param name="setter">The callback to set the value.</param>
      /// <param name="resolverCacheCreator">The callback to create meta data -wide cache for resolving value of this column. May be <c>null</c> if the column value is not resolvable value or does not use a cache.</param>
      /// <param name="resolver">The callback to resolve value from its initial to final form. May be <c>null</c> if the column value is not a resolvable value.</param>
      /// <param name="defaultSerializationInfo">The callback to create <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/> object for this column.</param>
      /// <returns>A new instance of <see cref="MetaDataColumnInformation{TRow, TValue}"/>, with additional functionalities registered.</returns>
      /// <remarks>
      /// The functionalities that are registered by this method are as follows:
      /// <list type="table">
      /// <listheader>
      /// <term>Functionality type</term>
      /// <term>Description</term>
      /// </listheader>
      /// <item>
      /// <term><see cref="MetaDataColumnInformationWithRawRowType"/></term>
      /// <term>Always registered, will describe the type of the raw row.</term>
      /// </item>
      /// <item>
      /// <term><see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/></term>
      /// <term>Always registered, will contain functionality related to serializing the column values to byte stream.</term>
      /// </item>
      /// <item>
      /// <term><see cref="CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.MetaDataColumnInformationWithResolvingCapability"/></term>
      /// <term>Only registered if <paramref name="resolver"/> is not <c>null</c>, will contain functionality for resolving the initially unresolved values. One such example is <see cref="CustomAttributeDefinition.Signature"/>, which is always initially <see cref="RawCustomAttributeSignature"/> for non-empty custom signature BLOBs, and will be transformed to <see cref="ResolvedCustomAttributeSignature"/> by <paramref name="resolver"/> callback.</term>
      /// </item>
      /// </list>
      /// All of these functionalities are accessible from the created <see cref="MetaDataColumnInformation{TRow, TValue}"/> with methods and extension methods of <see cref="ExtensionByCompositionProvider{TFunctionality}"/>.
      /// </remarks>
      /// <seealso cref="CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.MetaDataColumnInformationWithResolvingCapability"/>
      public static MetaDataColumnInformation<TRow, TValue> GenericColumn<TRow, TRawRow, TValue>(
         RowColumnGetterDelegate<TRow, TValue> getter,
         RowColumnSetterDelegate<TRow, TValue> setter,
         ResolverCacheCreatorDelegate resolverCacheCreator,
         ResolverDelegate resolver,
         Func<DefaultColumnSerializationInfo<TRow, TRawRow>> defaultSerializationInfo
         )
         where TRow : class
         where TRawRow : class
      {
         var retVal = new MetaDataColumnInformation<TRow, TValue>( getter, setter );

         retVal.RegisterFunctionalityDirect<MetaDataColumnInformationWithRawRowType>( new MetaDataColumnInformationWithRawRowType<TRawRow>() );
         retVal.RegisterFunctionalityDirect<CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.MetaDataColumnInformationWithResolvingCapability>( resolver == null ? null : new MetaDataColumnInformationWithResolvingCapabilityWithCallbacks( resolverCacheCreator, resolver ) );
         retVal.RegisterFunctionality( defaultSerializationInfo );

         return retVal;
      }

      private class MetaDataColumnInformationWithRawRowType<TRawRow> : MetaDataColumnInformationWithRawRowType
         where TRawRow : class
      {
         public MetaDataColumnInformationWithRawRowType()
         {

         }

         public Type RawRowType
         {
            get
            {
               return typeof( TRawRow );
            }
         }
      }

   }

   /// <summary>
   /// This interface is the functionality that will be present in <see cref="MetaDataColumnInformation{TRow, TValue}"/>s created by methods of <see cref="MetaDataColumnInformationFactory"/> class.
   /// </summary>
   /// <seealso cref="ExtensionByCompositionProvider{TFunctionality}"/>
   public interface MetaDataColumnInformationWithRawRowType
   {
      /// <summary>
      /// Gets the type of the raw row that this column belongs to.
      /// </summary>
      /// <value>The type of the raw row that this column belongs to.</value>
      Type RawRowType { get; }
   }

   /// <summary>
   /// This class implements the <see cref="CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.MetaDataColumnInformationWithResolvingCapability"/> interface by delegating the implementation of the methods of that interface to the callbacks it receives in constructor.
   /// </summary>
   public sealed class MetaDataColumnInformationWithResolvingCapabilityWithCallbacks : CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.MetaDataColumnInformationWithResolvingCapability
   {
      private readonly ResolverCacheCreatorDelegate _cacheCreator;
      private readonly ResolverDelegate _resolver;

      /// <summary>
      /// Creates a new instance of <see cref="MetaDataColumnInformationWithResolvingCapabilityWithCallbacks"/> with given callbacks.
      /// </summary>
      /// <param name="cacheCreator">The callback for <see cref="CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.MetaDataColumnInformationWithResolvingCapability.CreateCache"/> method. May be <c>null</c>.</param>
      /// <param name="resolver">The callback for <see cref="CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.MetaDataColumnInformationWithResolvingCapability.Resolve"/> method.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="resolver"/> is <c>null</c>.</exception>
      public MetaDataColumnInformationWithResolvingCapabilityWithCallbacks(
         ResolverCacheCreatorDelegate cacheCreator,
         ResolverDelegate resolver
         )
      {
         ArgumentValidator.ValidateNotNull( "Resolver", resolver );

         this._cacheCreator = cacheCreator;
         this._resolver = resolver;
      }

      /// <summary>
      /// Implements the <see cref="CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.MetaDataColumnInformationWithResolvingCapability.CreateCache"/> method by invoking the callback provided to constructor.
      /// </summary>
      /// <returns>The cache created by the callback, or <c>null</c> if no callback was supplied.</returns>
      public Object CreateCache()
      {
         return this._cacheCreator?.Invoke();
      }

      /// <summary>
      /// Implements the <see cref="CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.MetaDataColumnInformationWithResolvingCapability.Resolve"/> method by invoking the callback provided to constructor.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/>.</param>
      /// <param name="rowIndex">The row index.</param>
      /// <param name="resolver">The <see cref="MetaDataResolver"/> to use.</param>
      /// <param name="cache">The cache created by <see cref="CreateCache"/> method.</param>
      /// <returns><c>true</c> if resolving succeeded; <c>false</c> otherwise.</returns>
      public Boolean Resolve( CILMetaData md, Int32 rowIndex, MetaDataResolver resolver, Object cache )
      {
         return this._resolver( md, rowIndex, resolver, cache );
      }
   }

   /// <summary>
   /// This delegate is used by <see cref="MetaDataColumnInformationWithResolvingCapabilityWithCallbacks"/> to represent signature of <see cref="CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.MetaDataColumnInformationWithResolvingCapability.CreateCache"/> method.
   /// </summary>
   /// <returns>The cache to use in scope of single instance of <see cref="CILMetaData"/>.</returns>
   public delegate Object ResolverCacheCreatorDelegate();

   /// <summary>
   /// This delegate is used by <see cref="MetaDataColumnInformationWithResolvingCapabilityWithCallbacks"/> to represent signature of <see cref="CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.MetaDataColumnInformationWithResolvingCapability.Resolve"/> method.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="rowIndex">The row index.</param>
   /// <param name="resolver">The <see cref="MetaDataResolver"/> to use.</param>
   /// <param name="cache">The cache created by <see cref="CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.MetaDataColumnInformationWithResolvingCapability.CreateCache"/> method.</param>
   /// <returns><c>true</c> if resolving succeeded; <c>false</c> otherwise.</returns>
   public delegate Boolean ResolverDelegate( CILMetaData md, Int32 rowIndex, MetaDataResolver resolver, Object cache );



   //public abstract class MetaDataColumnDataInformation
   //{
   //   internal MetaDataColumnDataInformation()
   //   {
   //   }

   //   public abstract MetaDataColumnInformationKind ColumnKind { get; }
   //}

   //public sealed class MetaDataColumnDataInformation_FixedSizeConstant : MetaDataColumnDataInformation
   //{
   //   public MetaDataColumnDataInformation_FixedSizeConstant(
   //      Int32 byteSize
   //      )
   //      : base()
   //   {
   //      this.FixedSize = byteSize;
   //   }


   //   public Int32 FixedSize { get; }

   //   public override MetaDataColumnInformationKind ColumnKind
   //   {
   //      get
   //      {
   //         return MetaDataColumnInformationKind.FixedSizeConstant;
   //      }
   //   }
   //}

   //public sealed class MetaDataColumnDataInformation_SimpleTableIndex : MetaDataColumnDataInformation
   //{
   //   public MetaDataColumnDataInformation_SimpleTableIndex(
   //      Int32 targetTable
   //      )
   //      : base()
   //   {
   //      this.TargetTable = targetTable;
   //   }

   //   public Int32 TargetTable { get; }

   //   public override MetaDataColumnInformationKind ColumnKind
   //   {
   //      get
   //      {
   //         return MetaDataColumnInformationKind.SimpleTableIndex;
   //      }
   //   }
   //}

   //public sealed class MetaDataColumnDataInformation_CodedTableIndex : MetaDataColumnDataInformation
   //{
   //   public MetaDataColumnDataInformation_CodedTableIndex(
   //      IEnumerable<Int32?> targetTables
   //      )
   //      : base()
   //   {

   //      this.TargetTables = ( targetTables ?? Empty<Int32?>.Enumerable ).ToArrayProxy().CQ;
   //   }

   //   public ArrayQuery<Int32?> TargetTables { get; }

   //   public override MetaDataColumnInformationKind ColumnKind
   //   {
   //      get
   //      {
   //         return MetaDataColumnInformationKind.CodedTableIndex;
   //      }
   //   }
   //}

   //public sealed class MetaDataColumnDataInformation_HeapIndex : MetaDataColumnDataInformation
   //{
   //   public MetaDataColumnDataInformation_HeapIndex(
   //      String heapName
   //      )
   //      : base()
   //   {
   //      ArgumentValidator.ValidateNotEmpty( "Heap name", heapName );

   //      this.HeapName = heapName;
   //   }

   //   public String HeapName { get; }

   //   public override MetaDataColumnInformationKind ColumnKind
   //   {
   //      get
   //      {
   //         return MetaDataColumnInformationKind.HeapIndex;
   //      }
   //   }
   //}

   //public sealed class MetaDataColumnDataInformation_DataReference : MetaDataColumnDataInformation
   //{
   //   public MetaDataColumnDataInformation_DataReference(
   //      )
   //      : base()
   //   {

   //   }

   //   public override MetaDataColumnInformationKind ColumnKind
   //   {
   //      get
   //      {
   //         return MetaDataColumnInformationKind.DataReference;
   //      }
   //   }
   //}

   //public enum MetaDataColumnInformationKind
   //{
   //   FixedSizeConstant,
   //   SimpleTableIndex,
   //   CodedTableIndex,
   //   HeapIndex,
   //   DataReference
   //}

}