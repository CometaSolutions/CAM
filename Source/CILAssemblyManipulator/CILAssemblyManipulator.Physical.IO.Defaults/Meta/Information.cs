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

using UtilPack.CollectionsWithRoles;
using UtilPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CILAssemblyManipulator.Physical.IO.Defaults;
using CILAssemblyManipulator.Physical.IO;
using TabularMetaData.Meta;
using TabularMetaData;

namespace CILAssemblyManipulator.Physical.Meta
{
   /// <summary>
   /// This class acts as a factory for <see cref="MetaDataTableInformationProvider"/>s to use in CAM.Physical framework.
   /// </summary>
   public static class CILMetaDataTableInformationProviderFactory
   {
      internal const Int32 AMOUNT_OF_FIXED_TABLES = 0x2D;

      /// <summary>
      /// Gets the default <see cref="MetaDataTableInformationProvider"/> suitable to understand CIL 2.0 metadata files.
      /// </summary>
      /// <value>The default <see cref="MetaDataTableInformationProvider"/> suitable to understand CIL 2.0 metadata files.</value>
      public static MetaDataTableInformationProvider DefaultInstance { get; }

      static CILMetaDataTableInformationProviderFactory()
      {
         DefaultInstance = CreateInstance( null, null, CreateFixedTableInformations() );
      }


      private static MetaDataTableInformationProviderWithArray CreateInstance(
         SignatureProvider sigProvider,
         CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider opCodeProvider,
         IEnumerable<MetaDataTableInformation> tableInfos
         )
      {
         var retVal = new MetaDataTableInformationProviderWithArray( tableInfos, CAMCoreInternals.AMOUNT_OF_TABLES );

         CheckProviders( ref sigProvider, ref opCodeProvider );

         retVal.RegisterFunctionalityDirect<CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider>( opCodeProvider );
         retVal.RegisterFunctionalityDirect<SignatureProvider>( sigProvider );
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

      private static void CheckProviders(
         ref SignatureProvider sigProvider,
         ref CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider opCodeProvider
         )
      {
         if ( sigProvider == null )
         {
            sigProvider = DefaultSignatureProvider.DefaultInstance;
         }
         if ( opCodeProvider == null )
         {
            opCodeProvider = DefaultOpCodeProvider.DefaultInstance;
         }
      }

      ///// <summary>
      ///// This helper method creates the default instance of <see cref="MetaDataTableInformationProvider"/>.
      ///// </summary>
      ///// <param name="sigProvider">The optional <see cref="SignatureProvider"/> to use. If not supplied (is <c>null</c>), then <see cref="DefaultSignatureProvider.DefaultInstance"/> will be used.</param>
      ///// <param name="opCodeProvider">The optional <see cref="CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider"/> to use. If not supplied (is <c>null</c>), then <see cref="DefaultOpCodeProvider.DefaultInstance"/> will be used.</param>
      ///// <returns>The new instance of <see cref="MetaDataTableInformationProvider"/>.</returns>
      //public static MetaDataTableInformationProvider CreateDefault(
      //   SignatureProvider sigProvider = null,
      //   CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider opCodeProvider = null
      //   )
      //{
      //   return CreateInstance( sigProvider, opCodeProvider, CreateFixedTableInformations( sigProvider, opCodeProvider ) );
      //}

      /// <summary>
      /// This helper method creates the default instance of <see cref="MetaDataTableInformationProvider"/> with given <see cref="MetaDataTableInformation"/> objects for any additional tables.
      /// </summary>
      /// <param name="tableInfos">The enumerable of <see cref="MetaDataTableInformation"/>. May be <c>null</c> or contain <c>null</c> values. The order does not matter.</param>
      /// <param name="sigProvider">The optional <see cref="SignatureProvider"/> to use. If not supplied (is <c>null</c>), then <see cref="DefaultSignatureProvider.DefaultInstance"/> will be used.</param>
      /// <param name="opCodeProvider">The optional <see cref="CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider"/> to use. If not supplied (is <c>null</c>), then <see cref="DefaultOpCodeProvider.DefaultInstance"/> will be used.</param>
      /// <returns>The new instance of <see cref="MetaDataTableInformationProvider"/> containing <see cref="MetaDataTableInformation"/> for additional tables.</returns>
      /// <remarks>
      /// The <see cref="MetaDataTableInformation.TableIndex"/> of the given <paramref name="tableInfos"/> should be greater than maximum value of <see cref="Tables"/> enumeration.
      /// </remarks>
      public static MetaDataTableInformationProvider CreateWithAdditionalTables(
         IEnumerable<MetaDataTableInformation> tableInfos,
         SignatureProvider sigProvider = null,
         CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider opCodeProvider = null
         )
      {
         return CreateInstance( sigProvider, opCodeProvider, CreateFixedTableInformations( sigProvider, opCodeProvider ).Concat( tableInfos?.Where( t => (Int32) ( t?.TableIndex ?? 0 ) > AMOUNT_OF_FIXED_TABLES ) ?? Empty<MetaDataTableInformation>.Enumerable ) );
      }

      /// <summary>
      /// This helper method creates the default instance of <see cref="MetaDataTableInformationProvider"/> with given <see cref="MetaDataTableInformation"/> objects for all tables.
      /// </summary>
      /// <param name="tableInfos">The enumerable of <see cref="MetaDataTableInformation"/>. May be <c>null</c> or containg <c>null</c> values. The order does not matter.</param>
      /// <param name="sigProvider">The optional <see cref="SignatureProvider"/> to use. If not supplied (is <c>null</c>), then <see cref="DefaultSignatureProvider.DefaultInstance"/> will be used.</param>
      /// <param name="opCodeProvider">The optional <see cref="CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider"/> to use. If not supplied (is <c>null</c>), then <see cref="DefaultOpCodeProvider.DefaultInstance"/> will be used.</param>
      /// <returns>The new instance of <see cref="MetaDataTableInformationProvider"/> containing <see cref="MetaDataTableInformation"/> for all tables, including the fixed ones of <see cref="CILMetaData"/>.</returns>
      public static MetaDataTableInformationProvider CreateWithExactTables(
         IEnumerable<MetaDataTableInformation> tableInfos,
         SignatureProvider sigProvider = null,
         CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider opCodeProvider = null
         )
      {
         return CreateInstance( sigProvider, opCodeProvider, tableInfos );
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
      /// All of these functionalities are accessible from the created <see cref="MetaDataTableInformation{TRow}"/> with methods and extension methods of <see cref="UtilPack.Extension.SelfDescribingExtensionByCompositionProvider{TFunctionality}"/>.
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
      public static IEnumerable<MetaDataTableInformation> CreateFixedTableInformations(
         SignatureProvider sigProvider = null,
         CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider opCodeProvider = null
         )
      {
         CheckProviders( ref sigProvider, ref opCodeProvider );

         yield return CreateSingleTableInfo<ModuleDefinition, RawModuleDefinition>(
            Tables.Module,
            ComparerFromFunctions.NewEqualityComparer<ModuleDefinition>( TableRowComparisons.Equality_ModuleDefinition, TableRowComparisons.HashCode_ModuleDefinition ),
            null,
            () => new ModuleDefinition(),
            GetModuleDefColumns(),
            () => new RawModuleDefinition(),
            false
            );

         yield return CreateSingleTableInfo<TypeReference, RawTypeReference>(
            Tables.TypeRef,
            ComparerFromFunctions.NewEqualityComparer<TypeReference>( TableRowComparisons.Equality_TypeReference, TableRowComparisons.HashCode_TypeReference ),
            null,
            () => new TypeReference(),
            GetTypeRefColumns(),
            () => new RawTypeReference(),
            false
            );

         yield return CreateSingleTableInfo<TypeDefinition, RawTypeDefinition>(
            Tables.TypeDef,
            ComparerFromFunctions.NewEqualityComparer<TypeDefinition>( TableRowComparisons.Equality_TypeDefinition, TableRowComparisons.HashCode_TypeDefinition ),
            null,
            () => new TypeDefinition(),
            GetTypeDefColumns(),
            () => new RawTypeDefinition(),
            false
            );

         yield return CreateSingleTableInfo<FieldDefinitionPointer, RawFieldDefinitionPointer>(
            Tables.FieldPtr,
            ComparerFromFunctions.NewEqualityComparer<FieldDefinitionPointer>( TableRowComparisons.Equality_FieldDefinitionPointer, TableRowComparisons.HashCode_FieldDefinitionPointer ),
            null,
            () => new FieldDefinitionPointer(),
            GetFieldPtrColumns(),
            () => new RawFieldDefinitionPointer(),
            false
            );

         yield return CreateSingleTableInfo<FieldDefinition, RawFieldDefinition>(
            Tables.Field,
            ComparerFromFunctions.NewEqualityComparer<FieldDefinition>( ( x, y ) => TableRowComparisons.Equality_FieldDefinition( sigProvider, x, y ), TableRowComparisons.HashCode_FieldDefinition ),
            null,
            () => new FieldDefinition(),
            GetFieldDefColumns(),
            () => new RawFieldDefinition(),
            false
            );

         yield return CreateSingleTableInfo<MethodDefinitionPointer, RawMethodDefinitionPointer>(
            Tables.MethodPtr,
            ComparerFromFunctions.NewEqualityComparer<MethodDefinitionPointer>( TableRowComparisons.Equality_MethodDefinitionPointer, TableRowComparisons.HashCode_MethodDefinitionPointer ),
            null,
            () => new MethodDefinitionPointer(),
            GetMethodPtrColumns(),
            () => new RawMethodDefinitionPointer(),
            false
            );

         yield return CreateSingleTableInfo<MethodDefinition, RawMethodDefinition>(
            Tables.MethodDef,
            ComparerFromFunctions.NewEqualityComparer<MethodDefinition>( ( x, y ) => TableRowComparisons.Equality_MethodDefinition( sigProvider, opCodeProvider, x, y ), TableRowComparisons.HashCode_MethodDefinition ),
            null,
            () => new MethodDefinition(),
            GetMethodDefColumns(),
            () => new RawMethodDefinition(),
            false
            );

         yield return CreateSingleTableInfo<ParameterDefinitionPointer, RawParameterDefinitionPointer>(
            Tables.ParameterPtr,
            ComparerFromFunctions.NewEqualityComparer<ParameterDefinitionPointer>( TableRowComparisons.Equality_ParameterDefinitionPointer, TableRowComparisons.HashCode_ParameterDefinitionPointer ),
            null,
            () => new ParameterDefinitionPointer(),
            GetParamPtrColumns(),
            () => new RawParameterDefinitionPointer(),
            false
            );

         yield return CreateSingleTableInfo<ParameterDefinition, RawParameterDefinition>(
            Tables.Parameter,
            ComparerFromFunctions.NewEqualityComparer<ParameterDefinition>( TableRowComparisons.Equality_ParameterDefinition, TableRowComparisons.HashCode_ParameterDefinition ),
            null,
            () => new ParameterDefinition(),
            GetParamColumns(),
            () => new RawParameterDefinition(),
            false
            );

         yield return CreateSingleTableInfo<InterfaceImplementation, RawInterfaceImplementation>(
            Tables.InterfaceImpl,
            ComparerFromFunctions.NewEqualityComparer<InterfaceImplementation>( TableRowComparisons.Equality_InterfaceImplementation, TableRowComparisons.HashCode_InterfaceImplementation ),
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.InterfaceImplementationComparer,
            () => new InterfaceImplementation(),
            GetInterfaceImplColumns(),
            () => new RawInterfaceImplementation(),
            true
            );

         yield return CreateSingleTableInfo<MemberReference, RawMemberReference>(
            Tables.MemberRef,
            ComparerFromFunctions.NewEqualityComparer<MemberReference>( ( x, y ) => TableRowComparisons.Equality_MemberReference( sigProvider, x, y ), TableRowComparisons.HashCode_MemberReference ),
            null,
            () => new MemberReference(),
            GetMemberRefColumns(),
            () => new RawMemberReference(),
            false
            );

         yield return CreateSingleTableInfo<ConstantDefinition, RawConstantDefinition>(
            Tables.Constant,
            ComparerFromFunctions.NewEqualityComparer<ConstantDefinition>( TableRowComparisons.Equality_ConstantDefinition, TableRowComparisons.HashCode_ConstantDefinition ),
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.ConstantDefinitionComparer,
            () => new ConstantDefinition(),
            GetConstantColumns(),
            () => new RawConstantDefinition(),
            true
            );

         yield return CreateSingleTableInfo<CustomAttributeDefinition, RawCustomAttributeDefinition>(
            Tables.CustomAttribute,
            ComparerFromFunctions.NewEqualityComparer<CustomAttributeDefinition>( ( x, y ) => TableRowComparisons.Equality_CustomAttributeDefinition( sigProvider, x, y ), TableRowComparisons.HashCode_CustomAttributeDefinition ),
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.CustomAttributeDefinitionComparer,
            () => new CustomAttributeDefinition(),
            GetCustomAttributeColumns(),
            () => new RawCustomAttributeDefinition(),
            true
            );

         yield return CreateSingleTableInfo<FieldMarshal, RawFieldMarshal>(
            Tables.FieldMarshal,
            ComparerFromFunctions.NewEqualityComparer<FieldMarshal>( ( x, y ) => TableRowComparisons.Equality_FieldMarshal( sigProvider, x, y ), TableRowComparisons.HashCode_FieldMarshal ),
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.FieldMarshalComparer,
            () => new FieldMarshal(),
            GetFieldMarshalColumns(),
            () => new RawFieldMarshal(),
            true
            );

         yield return CreateSingleTableInfo<SecurityDefinition, RawSecurityDefinition>(
            Tables.DeclSecurity,
            ComparerFromFunctions.NewEqualityComparer<SecurityDefinition>( ( x, y ) => TableRowComparisons.Equality_SecurityDefinition( sigProvider, x, y ), TableRowComparisons.HashCode_SecurityDefinition ),
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.SecurityDefinitionComparer,
            () => new SecurityDefinition(),
            GetDeclSecurityColumns(),
            () => new RawSecurityDefinition(),
            true
            );

         yield return CreateSingleTableInfo<ClassLayout, RawClassLayout>(
            Tables.ClassLayout,
            ComparerFromFunctions.NewEqualityComparer<ClassLayout>( TableRowComparisons.Equality_ClassLayout, TableRowComparisons.HashCode_ClassLayout ),
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.ClassLayoutComparer,
            () => new ClassLayout(),
            GetClassLayoutColumns(),
            () => new RawClassLayout(),
            true
            );

         yield return CreateSingleTableInfo<FieldLayout, RawFieldLayout>(
            Tables.FieldLayout,
            ComparerFromFunctions.NewEqualityComparer<FieldLayout>( TableRowComparisons.Equality_FieldLayout, TableRowComparisons.HashCode_FieldLayout ),
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.FieldLayoutComparer,
            () => new FieldLayout(),
            GetFieldLayoutColumns(),
            () => new RawFieldLayout(),
            true
            );

         yield return CreateSingleTableInfo<StandaloneSignature, RawStandaloneSignature>(
            Tables.StandaloneSignature,
            ComparerFromFunctions.NewEqualityComparer<StandaloneSignature>( ( x, y ) => TableRowComparisons.Equality_StandaloneSignature( sigProvider, x, y ), x => TableRowComparisons.HashCode_StandaloneSignature( sigProvider, x ) ),
            null,
            () => new StandaloneSignature(),
            GetStandaloneSigColumns(),
            () => new RawStandaloneSignature(),
            false
            );

         yield return CreateSingleTableInfo<EventMap, RawEventMap>(
            Tables.EventMap,
            ComparerFromFunctions.NewEqualityComparer<EventMap>( TableRowComparisons.Equality_EventMap, TableRowComparisons.HashCode_EventMap ),
            null,
            () => new EventMap(),
            GetEventMapColumns(),
            () => new RawEventMap(),
            true
            );

         yield return CreateSingleTableInfo<EventDefinitionPointer, RawEventDefinitionPointer>(
            Tables.EventPtr,
            ComparerFromFunctions.NewEqualityComparer<EventDefinitionPointer>( TableRowComparisons.Equality_EventDefinitionPointer, TableRowComparisons.HashCode_EventDefinitionPointer ),
            null,
            () => new EventDefinitionPointer(),
            GetEventPtrColumns(),
            () => new RawEventDefinitionPointer(),
            false
            );

         yield return CreateSingleTableInfo<EventDefinition, RawEventDefinition>(
            Tables.Event,
            ComparerFromFunctions.NewEqualityComparer<EventDefinition>( TableRowComparisons.Equality_EventDefinition, TableRowComparisons.HashCode_EventDefinition ),
            null,
            () => new EventDefinition(),
            GetEventDefColumns(),
            () => new RawEventDefinition(),
            false
            );

         yield return CreateSingleTableInfo<PropertyMap, RawPropertyMap>(
            Tables.PropertyMap,
            ComparerFromFunctions.NewEqualityComparer<PropertyMap>( TableRowComparisons.Equality_PropertyMap, TableRowComparisons.HashCode_PropertyMap ),
            null,
            () => new PropertyMap(),
            GetPropertyMapColumns(),
            () => new RawPropertyMap(),
            true
            );

         yield return CreateSingleTableInfo<PropertyDefinitionPointer, RawPropertyDefinitionPointer>(
            Tables.PropertyPtr,
            ComparerFromFunctions.NewEqualityComparer<PropertyDefinitionPointer>( TableRowComparisons.Equality_PropertyDefinitionPointer, TableRowComparisons.HashCode_PropertyDefinitionPointer ),
            null,
            () => new PropertyDefinitionPointer(),
            GetPropertyPtrColumns(),
            () => new RawPropertyDefinitionPointer(),
            false
            );

         yield return CreateSingleTableInfo<PropertyDefinition, RawPropertyDefinition>(
            Tables.Property,
            ComparerFromFunctions.NewEqualityComparer<PropertyDefinition>( ( x, y ) => TableRowComparisons.Equality_PropertyDefinition( sigProvider, x, y ), x => TableRowComparisons.HashCode_PropertyDefinition( sigProvider, x ) ),
            null,
            () => new PropertyDefinition(),
            GetPropertyDefColumns(),
            () => new RawPropertyDefinition(),
            false
            );

         yield return CreateSingleTableInfo<MethodSemantics, RawMethodSemantics>(
            Tables.MethodSemantics,
            ComparerFromFunctions.NewEqualityComparer<MethodSemantics>( TableRowComparisons.Equality_MethodSemantics, TableRowComparisons.HashCode_MethodSemantics ),
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.MethodSemanticsComparer,
            () => new MethodSemantics(),
            GetMethodSemanticsColumns(),
            () => new RawMethodSemantics(),
            true
            );

         yield return CreateSingleTableInfo<MethodImplementation, RawMethodImplementation>(
            Tables.MethodImpl,
            ComparerFromFunctions.NewEqualityComparer<MethodImplementation>( TableRowComparisons.Equality_MethodImplementation, TableRowComparisons.HashCode_MethodImplementation ),
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.MethodImplementationComparer,
            () => new MethodImplementation(),
            GetMethodImplColumns(),
            () => new RawMethodImplementation(),
            true
            );

         yield return CreateSingleTableInfo<ModuleReference, RawModuleReference>(
            Tables.ModuleRef,
            ComparerFromFunctions.NewEqualityComparer<ModuleReference>( TableRowComparisons.Equality_ModuleReference, TableRowComparisons.HashCode_ModuleReference ),
            null,
            () => new ModuleReference(),
            GetModuleRefColumns(),
            () => new RawModuleReference(),
            false
            );

         yield return CreateSingleTableInfo<TypeSpecification, RawTypeSpecification>(
            Tables.TypeSpec,
            ComparerFromFunctions.NewEqualityComparer<TypeSpecification>( ( x, y ) => TableRowComparisons.Equality_TypeSpecification( sigProvider, x, y ), x => TableRowComparisons.HashCode_TypeSpecification( sigProvider, x ) ),
            null,
            () => new TypeSpecification(),
            GetTypeSpecColumns(),
            () => new RawTypeSpecification(),
            false
            );

         yield return CreateSingleTableInfo<MethodImplementationMap, RawMethodImplementationMap>(
            Tables.ImplMap,
            ComparerFromFunctions.NewEqualityComparer<MethodImplementationMap>( TableRowComparisons.Equality_MethodImplementationMap, TableRowComparisons.HashCode_MethodImplementationMap ),
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.MethodImplementationMapComparer,
            () => new MethodImplementationMap(),
            GetImplMapColumns(),
            () => new RawMethodImplementationMap(),
            true
            );

         yield return CreateSingleTableInfo<FieldRVA, RawFieldRVA>(
            Tables.FieldRVA,
            ComparerFromFunctions.NewEqualityComparer<FieldRVA>( TableRowComparisons.Equality_FieldRVA, TableRowComparisons.HashCode_FieldRVA ),
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.FieldRVAComparer,
            () => new FieldRVA(),
            GetFieldRVAColumns(),
            () => new RawFieldRVA(),
            true
            );

         yield return CreateSingleTableInfo<EditAndContinueLog, RawEditAndContinueLog>(
            Tables.EncLog,
            ComparerFromFunctions.NewEqualityComparer<EditAndContinueLog>( TableRowComparisons.Equality_EditAndContinueLog, TableRowComparisons.HashCode_EditAndContinueLog ),
            null,
            () => new EditAndContinueLog(),
            GetENCLogColumns(),
            () => new RawEditAndContinueLog(),
            false
            );

         yield return CreateSingleTableInfo<EditAndContinueMap, RawEditAndContinueMap>(
            Tables.EncMap,
            ComparerFromFunctions.NewEqualityComparer<EditAndContinueMap>( TableRowComparisons.Equality_EditAndContinueMap, TableRowComparisons.HashCode_EditAndContinueMap ),
            null,
            () => new EditAndContinueMap(),
            GetENCMapColumns(),
            () => new RawEditAndContinueMap(),
            false
            );

         yield return CreateSingleTableInfo<AssemblyDefinition, RawAssemblyDefinition>(
            Tables.Assembly,
            ComparerFromFunctions.NewEqualityComparer<AssemblyDefinition>( TableRowComparisons.Equality_AssemblyDefinition, TableRowComparisons.HashCode_AssemblyDefinition ),
            null,
            () => new AssemblyDefinition(),
            GetAssemblyDefColumns(),
            () => new RawAssemblyDefinition(),
            false
            );

#pragma warning disable 618

         yield return CreateSingleTableInfo<AssemblyDefinitionProcessor, RawAssemblyDefinitionProcessor>(
            Tables.AssemblyProcessor,
            ComparerFromFunctions.NewEqualityComparer<AssemblyDefinitionProcessor>( TableRowComparisons.Equality_AssemblyDefinitionProcessor, TableRowComparisons.HashCode_AssemblyDefinitionProcessor ),
            null,
            () => new AssemblyDefinitionProcessor(),
            GetAssemblyDefProcessorColumns(),
            () => new RawAssemblyDefinitionProcessor(),
            false
            );

         yield return CreateSingleTableInfo<AssemblyDefinitionOS, RawAssemblyDefinitionOS>(
            Tables.AssemblyOS,
            ComparerFromFunctions.NewEqualityComparer<AssemblyDefinitionOS>( TableRowComparisons.Equality_AssemblyDefinitionOS, TableRowComparisons.HashCode_AssemblyDefinitionOS ),
            null,
            () => new AssemblyDefinitionOS(),
            GetAssemblyDefOSColumns(),
            () => new RawAssemblyDefinitionOS(),
            false
            );

#pragma warning restore 618

         yield return CreateSingleTableInfo<AssemblyReference, RawAssemblyReference>(
            Tables.AssemblyRef,
            ComparerFromFunctions.NewEqualityComparer<AssemblyReference>( TableRowComparisons.Equality_AssemblyReference, TableRowComparisons.HashCode_AssemblyReference ),
            null,
            () => new AssemblyReference(),
            GetAssemblyRefColumns(),
            () => new RawAssemblyReference(),
            false
            );

#pragma warning disable 618

         yield return CreateSingleTableInfo<AssemblyReferenceProcessor, RawAssemblyReferenceProcessor>(
            Tables.AssemblyRefProcessor,
            ComparerFromFunctions.NewEqualityComparer<AssemblyReferenceProcessor>( TableRowComparisons.Equality_AssemblyReferenceProcessor, TableRowComparisons.HashCode_AssemblyReferenceProcessor ),
            null,
            () => new AssemblyReferenceProcessor(),
            GetAssemblyRefProcessorColumns(),
            () => new RawAssemblyReferenceProcessor(),
            false
            );

         yield return CreateSingleTableInfo<AssemblyReferenceOS, RawAssemblyReferenceOS>(
            Tables.AssemblyRefOS,
            ComparerFromFunctions.NewEqualityComparer<AssemblyReferenceOS>( TableRowComparisons.Equality_AssemblyReferenceOS, TableRowComparisons.HashCode_AssemblyReferenceOS ),
            null,
            () => new AssemblyReferenceOS(),
            GetAssemblyRefOSColumns(),
            () => new RawAssemblyReferenceOS(),
            false
            );

#pragma warning restore 618

         yield return CreateSingleTableInfo<FileReference, RawFileReference>(
            Tables.File,
            ComparerFromFunctions.NewEqualityComparer<FileReference>( TableRowComparisons.Equality_FileReference, TableRowComparisons.HashCode_FileReference ),
            null,
            () => new FileReference(),
            GetFileColumns(),
            () => new RawFileReference(),
            false
            );

         yield return CreateSingleTableInfo<ExportedType, RawExportedType>(
            Tables.ExportedType,
            ComparerFromFunctions.NewEqualityComparer<ExportedType>( TableRowComparisons.Equality_ExportedType, TableRowComparisons.HashCode_ExportedType ),
            null,
            () => new ExportedType(),
            GetExportedTypeColumns(),
            () => new RawExportedType(),
            false
            );

         yield return CreateSingleTableInfo<ManifestResource, RawManifestResource>(
            Tables.ManifestResource,
            ComparerFromFunctions.NewEqualityComparer<ManifestResource>( TableRowComparisons.Equality_ManifestResource, TableRowComparisons.HashCode_ManifestResource ),
            null,
            () => new ManifestResource(),
            GetManifestResourceColumns(),
            () => new RawManifestResource(),
            false
            );

         yield return CreateSingleTableInfo<NestedClassDefinition, RawNestedClassDefinition>(
            Tables.NestedClass,
            ComparerFromFunctions.NewEqualityComparer<NestedClassDefinition>( TableRowComparisons.Equality_NestedClassDefinition, TableRowComparisons.HashCode_NestedClassDefinition ),
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.NestedClassDefinitionComparer,
            () => new NestedClassDefinition(),
            GetNestedClassColumns(),
            () => new RawNestedClassDefinition(),
            true
            );

         yield return CreateSingleTableInfo<GenericParameterDefinition, RawGenericParameterDefinition>(
            Tables.GenericParameter,
            ComparerFromFunctions.NewEqualityComparer<GenericParameterDefinition>( TableRowComparisons.Equality_GenericParameterDefinition, TableRowComparisons.HashCode_GenericParameterDefinition ),
            CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.GenericParameterDefinitionComparer,
            () => new GenericParameterDefinition(),
            GetGenericParamColumns(),
            () => new RawGenericParameterDefinition(),
            true
            );

         yield return CreateSingleTableInfo<MethodSpecification, RawMethodSpecification>(
            Tables.MethodSpec,
            ComparerFromFunctions.NewEqualityComparer<MethodSpecification>( ( x, y ) => TableRowComparisons.Equality_MethodSpecification( sigProvider, x, y ), x => TableRowComparisons.HashCode_MethodSpecification( sigProvider, x ) ),
            null,
            () => new MethodSpecification(),
            GetMethodSpecColumns(),
            () => new RawMethodSpecification(),
            false
            );

         yield return CreateSingleTableInfo<GenericParameterConstraintDefinition, RawGenericParameterConstraintDefinition>(
            Tables.GenericParameterConstraint,
            ComparerFromFunctions.NewEqualityComparer<GenericParameterConstraintDefinition>( TableRowComparisons.Equality_GenericParameterConstraintDefinition, TableRowComparisons.HashCode_GenericParameterConstraintDefinition ),
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
               mDef.IL = rArgs.MetaData.GetOpCodeProvider().DeserializeIL( rArgs.Stream.At( offset ), rArgs.Array, rArgs.MDStreamContainer.UserStrings );
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
         yield return MetaDataColumnInformationFactory.BLOBCustom<ConstantDefinition, RawConstantDefinition, Object>( ( r, v ) => { r.Value = v; return true; }, r => r.Value, ( r, v ) => r.Value = v, ( args, v, blobs ) => args.Row.Value = blobs.ReadConstantValue( v, args.RowArgs.MetaData.GetSignatureProvider(), args.Row.Type ), args => args.RowArgs.Array.CreateConstantBytes( args.Row.Value, args.Row.Type ), null, null );
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
         yield return MetaDataColumnInformationFactory.BLOBCustom<CustomAttributeDefinition, RawCustomAttributeDefinition, AbstractCustomAttributeSignature>( ( r, v ) => { r.Signature = v; return true; }, r => r.Signature, ( r, v ) => r.Signature = v, ( args, v, blobs ) => args.Row.Signature = blobs.ReadCASignature( v, args.RowArgs.MetaData.GetSignatureProvider() ), args => args.RowArgs.Array.CreateCustomAttributeSignature( args.RowArgs.MetaData, args.RowIndex ), CreateCAColumnSpecificCache, ResolveCASignature );
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
         yield return MetaDataColumnInformationFactory.BLOBCustom<FieldMarshal, RawFieldMarshal, AbstractMarshalingInfo>( ( r, v ) => { r.NativeType = v; return true; }, row => row.NativeType, ( r, v ) => r.NativeType = v, ( args, v, blobs ) => args.Row.NativeType = blobs.ReadMarshalingInfo( v, args.RowArgs.MetaData.GetSignatureProvider() ), args => args.RowArgs.Array.CreateMarshalSpec( args.Row.NativeType ), null, null );
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
         yield return MetaDataColumnInformationFactory.BLOBCustom<SecurityDefinition, RawSecurityDefinition, List<AbstractSecurityInformation>>( ( r, v ) => { r.PermissionSets.Clear(); r.PermissionSets.AddRange( v ); return true; }, row => row.PermissionSets, ( r, v ) => r.PermissionSets = v, ( args, v, blobs ) => blobs.ReadSecurityInformation( v, args.RowArgs.MetaData.GetSignatureProvider(), args.Row.PermissionSets ), args => args.RowArgs.Array.CreateSecuritySignature( args.Row.PermissionSets, args.RowArgs.AuxArray, args.RowArgs.MetaData.GetSignatureProvider() ), CreateCAColumnSpecificCache, ResolveSecurityPermissionSets );
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
            args.Row.Signature = blobs.ReadNonTypeSignature( v, args.RowArgs.MetaData.GetSignatureProvider(), false, true, out wasFieldSig );
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
                     var arg = md.GetSignatureProvider().ReadCANamedArgument( bytes, ref idx, typeStr => ResolveTypeFromFullName( md, typeStr, resolver, actualCache ) );
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
            var sp = md.GetSignatureProvider();

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
                  retVal.TypedArguments.Add( sp.ReadCAFixedArgument( blob, ref idx, caType, typeStr => ResolveTypeFromFullName( md, typeStr, resolver, cache ) ) );
               }
            }

            // Check if we had failed to resolve ctor type before.
            success = retVal.TypedArguments.Count == ctorSig.Parameters.Count;
            if ( success )
            {
               var namedCount = blob.ReadUInt16LEFromBytes( ref idx );
               for ( var i = 0; i < namedCount && success; ++i )
               {
                  var caNamedArg = sp.ReadCANamedArgument( blob, ref idx, typeStr => ResolveTypeFromFullName( md, typeStr, resolver, cache ) );

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
                  retVal = md.GetSignatureProvider().GetSimpleCATypeOrNull( (CustomAttributeArgumentTypeSimpleKind) ( (SimpleTypeSignature) sig ).SimpleType );
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
   /// This delegate is accessible through (extension) methods for <see cref="UtilPack.Extension.SelfDescribingExtensionByCompositionProvider{TFunctionality}"/> of <see cref="MetaDataTableInformation"/>.
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
            ( args, value, blobs ) => setter( args.Row, blobs.ReadTypeSignature( value, args.RowArgs.MetaData.GetSignatureProvider() ) ),
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
               setter( args.Row, blobs.ReadNonTypeSignature( value, args.RowArgs.MetaData.GetSignatureProvider(), isMethodDef, false, out wasFieldSig ) as TSignature );
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
      /// All of these functionalities are accessible from the created <see cref="MetaDataColumnInformation{TRow, TValue}"/> with methods and extension methods of <see cref="UtilPack.Extension.SelfDescribingExtensionByCompositionProvider{TFunctionality}"/>.
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
   /// <seealso cref="UtilPack.Extension.SelfDescribingExtensionByCompositionProvider{TFunctionality}"/>
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







   /// <summary>
   /// This class contains all <see cref="IEqualityComparer{T}"/>s and <see cref="IComparer{T}"/>s for various types present in this assembly.
   /// </summary>
   /// <remarks>
   /// All the equality comparers are <c>exact</c> in a sense that all properties of the objects being compared must match precisely in order for a equality comparer to return value <c>true</c>.
   /// </remarks>
   internal static class TableRowComparisons
   {

      //      internal static Boolean Equality_MetaData( CILMetaData x, CILMetaData y )
      //      {
      //         // TODO: Table infos actually use these comparers to perform equality & hash code operations
      //         // Instead, this method should actually use the comparers of table infos
      //         // Comparers of table infos should be the ones that are currently here.
      //         // Alternatively, consider merging this class and DefaultMetaDataTableInformationProvider .
      //#pragma warning disable 618
      //         return Object.ReferenceEquals( x, y ) ||
      //            ( x != null && y != null
      //            && ListEqualityComparer<List<ModuleDefinition>, ModuleDefinition>.Equals( x.ModuleDefinitions.TableContents, y.ModuleDefinitions.TableContents, ModuleDefinitionEqualityComparer )
      //            && ListEqualityComparer<List<TypeReference>, TypeReference>.Equals( x.TypeReferences.TableContents, y.TypeReferences.TableContents, TypeReferenceEqualityComparer )
      //            && ListEqualityComparer<List<TypeDefinition>, TypeDefinition>.Equals( x.TypeDefinitions.TableContents, y.TypeDefinitions.TableContents, TypeDefinitionEqualityComparer )
      //            && ListEqualityComparer<List<FieldDefinitionPointer>, FieldDefinitionPointer>.Equals( x.FieldDefinitionPointers.TableContents, y.FieldDefinitionPointers.TableContents, FieldDefinitionPointerEqualityComparer )
      //            && ListEqualityComparer<List<FieldDefinition>, FieldDefinition>.Equals( x.FieldDefinitions.TableContents, y.FieldDefinitions.TableContents, FieldDefinitionEqualityComparer )
      //            && ListEqualityComparer<List<MethodDefinitionPointer>, MethodDefinitionPointer>.Equals( x.MethodDefinitionPointers.TableContents, y.MethodDefinitionPointers.TableContents, MethodDefinitionPointerEqualityComparer )
      //            && ListEqualityComparer<List<MethodDefinition>, MethodDefinition>.Equals( x.MethodDefinitions.TableContents, y.MethodDefinitions.TableContents, MethodDefinitionEqualityComparer )
      //            && ListEqualityComparer<List<ParameterDefinitionPointer>, ParameterDefinitionPointer>.Equals( x.ParameterDefinitionPointers.TableContents, y.ParameterDefinitionPointers.TableContents, ParameterDefinitionPointerEqualityComparer )
      //            && ListEqualityComparer<List<ParameterDefinition>, ParameterDefinition>.Equals( x.ParameterDefinitions.TableContents, y.ParameterDefinitions.TableContents, ParameterDefinitionEqualityComparer )
      //            && ListEqualityComparer<List<InterfaceImplementation>, InterfaceImplementation>.Equals( x.InterfaceImplementations.TableContents, y.InterfaceImplementations.TableContents, InterfaceImplementationEqualityComparer )
      //            && ListEqualityComparer<List<MemberReference>, MemberReference>.Equals( x.MemberReferences.TableContents, y.MemberReferences.TableContents, MemberReferenceEqualityComparer )
      //            && ListEqualityComparer<List<ConstantDefinition>, ConstantDefinition>.Equals( x.ConstantDefinitions.TableContents, y.ConstantDefinitions.TableContents, ConstantDefinitionEqualityComparer )
      //            && ListEqualityComparer<List<CustomAttributeDefinition>, CustomAttributeDefinition>.Equals( x.CustomAttributeDefinitions.TableContents, y.CustomAttributeDefinitions.TableContents, CustomAttributeDefinitionEqualityComparer )
      //            && ListEqualityComparer<List<FieldMarshal>, FieldMarshal>.Equals( x.FieldMarshals.TableContents, y.FieldMarshals.TableContents, FieldMarshalEqualityComparer )
      //            && ListEqualityComparer<List<SecurityDefinition>, SecurityDefinition>.Equals( x.SecurityDefinitions.TableContents, y.SecurityDefinitions.TableContents, SecurityDefinitionEqualityComparer )
      //            && ListEqualityComparer<List<ClassLayout>, ClassLayout>.Equals( x.ClassLayouts.TableContents, y.ClassLayouts.TableContents, ClassLayoutEqualityComparer )
      //            && ListEqualityComparer<List<FieldLayout>, FieldLayout>.Equals( x.FieldLayouts.TableContents, y.FieldLayouts.TableContents, FieldLayoutEqualityComparer )
      //            && ListEqualityComparer<List<StandaloneSignature>, StandaloneSignature>.Equals( x.StandaloneSignatures.TableContents, y.StandaloneSignatures.TableContents, StandaloneSignatureEqualityComparer )
      //            && ListEqualityComparer<List<EventMap>, EventMap>.Equals( x.EventMaps.TableContents, y.EventMaps.TableContents, EventMapEqualityComparer )
      //            && ListEqualityComparer<List<EventDefinitionPointer>, EventDefinitionPointer>.Equals( x.EventDefinitionPointers.TableContents, y.EventDefinitionPointers.TableContents, EventDefinitionPointerEqualityComparer )
      //            && ListEqualityComparer<List<EventDefinition>, EventDefinition>.Equals( x.EventDefinitions.TableContents, y.EventDefinitions.TableContents, EventDefinitionEqualityComparer )
      //            && ListEqualityComparer<List<PropertyMap>, PropertyMap>.Equals( x.PropertyMaps.TableContents, y.PropertyMaps.TableContents, PropertyMapEqualityComparer )
      //            && ListEqualityComparer<List<PropertyDefinitionPointer>, PropertyDefinitionPointer>.Equals( x.PropertyDefinitionPointers.TableContents, y.PropertyDefinitionPointers.TableContents, PropertyDefinitionPointerEqualityComparer )
      //            && ListEqualityComparer<List<PropertyDefinition>, PropertyDefinition>.Equals( x.PropertyDefinitions.TableContents, y.PropertyDefinitions.TableContents, PropertyDefinitionEqualityComparer )
      //            && ListEqualityComparer<List<MethodSemantics>, MethodSemantics>.Equals( x.MethodSemantics.TableContents, y.MethodSemantics.TableContents, MethodSemanticsEqualityComparer )
      //            && ListEqualityComparer<List<MethodImplementation>, MethodImplementation>.Equals( x.MethodImplementations.TableContents, y.MethodImplementations.TableContents, MethodImplementationEqualityComparer )
      //            && ListEqualityComparer<List<ModuleReference>, ModuleReference>.Equals( x.ModuleReferences.TableContents, y.ModuleReferences.TableContents, ModuleReferenceEqualityComparer )
      //            && ListEqualityComparer<List<TypeSpecification>, TypeSpecification>.Equals( x.TypeSpecifications.TableContents, y.TypeSpecifications.TableContents, TypeSpecificationEqualityComparer )
      //            && ListEqualityComparer<List<MethodImplementationMap>, MethodImplementationMap>.Equals( x.MethodImplementationMaps.TableContents, y.MethodImplementationMaps.TableContents, MethodImplementationMapEqualityComparer )
      //            && ListEqualityComparer<List<FieldRVA>, FieldRVA>.Equals( x.FieldRVAs.TableContents, y.FieldRVAs.TableContents, FieldRVAEqualityComparer )
      //            && ListEqualityComparer<List<EditAndContinueLog>, EditAndContinueLog>.Equals( x.EditAndContinueLog.TableContents, y.EditAndContinueLog.TableContents, EditAndContinueLogEqualityComparer )
      //            && ListEqualityComparer<List<EditAndContinueMap>, EditAndContinueMap>.Equals( x.EditAndContinueMap.TableContents, y.EditAndContinueMap.TableContents, EditAndContinueMapEqualityComparer )
      //            && ListEqualityComparer<List<AssemblyDefinition>, AssemblyDefinition>.Equals( x.AssemblyDefinitions.TableContents, y.AssemblyDefinitions.TableContents, AssemblyDefinitionEqualityComparer )
      //            && ListEqualityComparer<List<AssemblyDefinitionProcessor>, AssemblyDefinitionProcessor>.Equals( x.AssemblyDefinitionProcessors.TableContents, y.AssemblyDefinitionProcessors.TableContents, AssemblyDefinitionProcessorEqualityComparer )
      //            && ListEqualityComparer<List<AssemblyDefinitionOS>, AssemblyDefinitionOS>.Equals( x.AssemblyDefinitionOSs.TableContents, y.AssemblyDefinitionOSs.TableContents, AssemblyDefinitionOSEqualityComparer )
      //            && ListEqualityComparer<List<AssemblyReference>, AssemblyReference>.Equals( x.AssemblyReferences.TableContents, y.AssemblyReferences.TableContents, AssemblyReferenceEqualityComparer )
      //            && ListEqualityComparer<List<AssemblyReferenceProcessor>, AssemblyReferenceProcessor>.Equals( x.AssemblyReferenceProcessors.TableContents, y.AssemblyReferenceProcessors.TableContents, AssemblyReferenceProcessorEqualityComparer )
      //            && ListEqualityComparer<List<AssemblyReferenceOS>, AssemblyReferenceOS>.Equals( x.AssemblyReferenceOSs.TableContents, y.AssemblyReferenceOSs.TableContents, AssemblyReferenceOSEqualityComparer )
      //            && ListEqualityComparer<List<FileReference>, FileReference>.Equals( x.FileReferences.TableContents, y.FileReferences.TableContents, FileReferenceEqualityComparer )
      //            && ListEqualityComparer<List<ExportedType>, ExportedType>.Equals( x.ExportedTypes.TableContents, y.ExportedTypes.TableContents, ExportedTypeEqualityComparer )
      //            && ListEqualityComparer<List<ManifestResource>, ManifestResource>.Equals( x.ManifestResources.TableContents, y.ManifestResources.TableContents, ManifestResourceEqualityComparer )
      //            && ListEqualityComparer<List<NestedClassDefinition>, NestedClassDefinition>.Equals( x.NestedClassDefinitions.TableContents, y.NestedClassDefinitions.TableContents, NestedClassDefinitionEqualityComparer )
      //            && ListEqualityComparer<List<GenericParameterDefinition>, GenericParameterDefinition>.Equals( x.GenericParameterDefinitions.TableContents, y.GenericParameterDefinitions.TableContents, GenericParameterDefinitionEqualityComparer )
      //            && ListEqualityComparer<List<MethodSpecification>, MethodSpecification>.Equals( x.MethodSpecifications.TableContents, y.MethodSpecifications.TableContents, MethodSpecificationEqualityComparer )
      //            && ListEqualityComparer<List<GenericParameterConstraintDefinition>, GenericParameterConstraintDefinition>.Equals( x.GenericParameterConstraintDefinitions.TableContents, y.GenericParameterConstraintDefinitions.TableContents, GenericParameterConstraintDefinitionEqualityComparer )
      //            && SequenceEqualityComparer<IEnumerable<MetaDataTable>, MetaDataTable>.SequenceEquality(
      //               x.GetAdditionalTables(),
      //               y.GetAdditionalTables(),
      //               ( xa, ya ) => SequenceEqualityComparer<IEnumerable<Object>, Object>.SequenceEquality(
      //                  xa.TableContentsNotGeneric.Cast<Object>(),
      //                  ya.TableContentsNotGeneric.Cast<Object>(),
      //                  xa.TableInformationNotGeneric.EqualityComparerNotGeneric.Equals
      //               ) )
      //            );
      //#pragma warning restore 618
      //      }

      internal static Boolean Equality_ModuleDefinition( ModuleDefinition x, ModuleDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.Generation == y.Generation
            && x.ModuleGUID.EqualsTypedEquatable( y.ModuleGUID )
            && x.EditAndContinueGUID.EqualsTypedEquatable( y.EditAndContinueGUID )
            && x.EditAndContinueBaseGUID.EqualsTypedEquatable( y.EditAndContinueBaseGUID )
            );
      }

      internal static Boolean Equality_TypeReference( TypeReference x, TypeReference y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && String.Equals( x.Namespace, y.Namespace )
            && x.ResolutionScope.EqualsTypedEquatable( y.ResolutionScope )
            );
      }

      internal static Boolean Equality_TypeDefinition( TypeDefinition x, TypeDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && String.Equals( x.Namespace, y.Namespace )
            && x.Attributes == y.Attributes
            && x.BaseType.EqualsTypedEquatable( y.BaseType )
            && x.FieldList.Equals( y.FieldList )
            && x.MethodList.Equals( y.MethodList )
            );
      }

      internal static Boolean Equality_FieldDefinitionPointer( FieldDefinitionPointer x, FieldDefinitionPointer y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null && x.FieldIndex.Equals( y.FieldIndex ) );
      }


      internal static Boolean Equality_FieldDefinition( SignatureProvider sigProvider, FieldDefinition x, FieldDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.Attributes == y.Attributes
            && sigProvider.SignatureEquality( x.Signature, y.Signature )
            );
      }

      internal static Boolean Equality_MethodDefinitionPointer( MethodDefinitionPointer x, MethodDefinitionPointer y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null && x.MethodIndex.Equals( y.MethodIndex ) );
      }

      internal static Boolean Equality_MethodDefinition( SignatureProvider sigProvider, CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider opCodeProvider, MethodDefinition x, MethodDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.Attributes == y.Attributes
            && x.ImplementationAttributes == y.ImplementationAttributes
            && x.ParameterList == y.ParameterList
            && sigProvider.SignatureEquality( x.Signature, y.Signature )
            && Equality_MethodILDefinition( x.IL, y.IL )
            );
      }

      internal static Boolean Equality_ParameterDefinitionPointer( ParameterDefinitionPointer x, ParameterDefinitionPointer y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null && x.ParameterIndex.Equals( y.ParameterIndex ) );
      }

      internal static Boolean Equality_ParameterDefinition( ParameterDefinition x, ParameterDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Sequence == y.Sequence
            && String.Equals( x.Name, y.Name )
            && x.Attributes == y.Attributes
             );
      }

      internal static Boolean Equality_InterfaceImplementation( InterfaceImplementation x, InterfaceImplementation y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Class == y.Class
             && x.Interface == y.Interface
             );
      }

      internal static Boolean Equality_MemberReference( SignatureProvider sigProvider, MemberReference x, MemberReference y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && String.Equals( x.Name, y.Name )
             && x.DeclaringType == y.DeclaringType
             && sigProvider.SignatureEquality( x.Signature, y.Signature )
             );
      }

      internal static Boolean Equality_ConstantDefinition( ConstantDefinition x, ConstantDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Type == y.Type
             && x.Parent == y.Parent
             && Object.Equals( x.Value, y.Value )
             );
      }

      internal static Boolean Equality_CustomAttributeDefinition( SignatureProvider sigProvider, CustomAttributeDefinition x, CustomAttributeDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Parent == y.Parent
             && x.Type == y.Type
             && Equality_AbstractCustomAttributeSignature( x.Signature, y.Signature ) // TODO use sigProvider to compare custom attribute signatures.
             );
      }

      internal static Boolean Equality_FieldMarshal( SignatureProvider sigProvider, FieldMarshal x, FieldMarshal y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Parent == y.Parent
             && Equality_MarshalingInfo( x.NativeType, y.NativeType ) // TODO use sigProvider to compare marshal signatures.
             );
      }

      internal static Boolean Equality_SecurityDefinition( SignatureProvider sigProvider, SecurityDefinition x, SecurityDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Parent == y.Parent
             && x.Action == y.Action
             && ListEqualityComparer<List<AbstractSecurityInformation>, AbstractSecurityInformation>.ListEquality( x.PermissionSets, y.PermissionSets, Equality_AbstractSecurityInformation )
             );
      }

      internal static Boolean Equality_ClassLayout( ClassLayout x, ClassLayout y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Parent == y.Parent
             && x.PackingSize == y.PackingSize
             && x.ClassSize == y.ClassSize
             );
      }

      internal static Boolean Equality_FieldLayout( FieldLayout x, FieldLayout y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Field == y.Field
             && x.Offset == y.Offset
             );
      }

      internal static Boolean Equality_StandaloneSignature( SignatureProvider sigProvider, StandaloneSignature x, StandaloneSignature y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.StoreSignatureAsFieldSignature == y.StoreSignatureAsFieldSignature
             && sigProvider.SignatureEquality( x.Signature, y.Signature )
             );
      }

      internal static Boolean Equality_EventMap( EventMap x, EventMap y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Parent == y.Parent
             && x.EventList == y.EventList
             );
      }

      internal static Boolean Equality_EventDefinitionPointer( EventDefinitionPointer x, EventDefinitionPointer y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null && x.EventIndex.Equals( y.EventIndex ) );
      }

      internal static Boolean Equality_EventDefinition( EventDefinition x, EventDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && String.Equals( x.Name, y.Name )
             && x.Attributes == y.Attributes
             && x.EventType == y.EventType
             );
      }

      internal static Boolean Equality_PropertyMap( PropertyMap x, PropertyMap y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Parent == y.Parent
             && x.PropertyList == y.PropertyList
             );
      }

      internal static Boolean Equality_PropertyDefinitionPointer( PropertyDefinitionPointer x, PropertyDefinitionPointer y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null && x.PropertyIndex.Equals( y.PropertyIndex ) );
      }

      internal static Boolean Equality_PropertyDefinition( SignatureProvider sigProvider, PropertyDefinition x, PropertyDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && String.Equals( x.Name, y.Name )
             && x.Attributes == y.Attributes
             && sigProvider.SignatureEquality( x.Signature, y.Signature )
             );
      }

      internal static Boolean Equality_MethodSemantics( MethodSemantics x, MethodSemantics y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Attributes == y.Attributes
             && x.Method == y.Method
             && x.Associaton == y.Associaton
             );
      }

      internal static Boolean Equality_MethodImplementation( MethodImplementation x, MethodImplementation y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Class == y.Class
             && x.MethodBody == y.MethodBody
             && x.MethodDeclaration == y.MethodDeclaration
             );
      }

      internal static Boolean Equality_ModuleReference( ModuleReference x, ModuleReference y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && String.Equals( x.ModuleName, y.ModuleName )
             );
      }

      internal static Boolean Equality_TypeSpecification( SignatureProvider sigProvider, TypeSpecification x, TypeSpecification y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && sigProvider.SignatureEquality( x.Signature, y.Signature )
             );
      }

      internal static Boolean Equality_MethodImplementationMap( MethodImplementationMap x, MethodImplementationMap y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && String.Equals( x.ImportName, y.ImportName )
             && x.MemberForwarded == y.MemberForwarded
             && x.Attributes == y.Attributes
             && x.ImportScope == y.ImportScope
             );
      }

      internal static Boolean Equality_FieldRVA( FieldRVA x, FieldRVA y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Field == y.Field
             && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.Data, y.Data )
             );
      }

      internal static Boolean Equality_EditAndContinueLog( EditAndContinueLog x, EditAndContinueLog y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Token == y.Token
            && x.FuncCode == y.FuncCode
            );
      }

      internal static Boolean Equality_EditAndContinueMap( EditAndContinueMap x, EditAndContinueMap y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Token == y.Token
            );
      }

      internal static Boolean Equality_AssemblyDefinition( AssemblyDefinition x, AssemblyDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.AssemblyInformation.Equals( y.AssemblyInformation )
             && x.Attributes == y.Attributes
             && x.HashAlgorithm == y.HashAlgorithm
             );
      }

#pragma warning disable 618

      internal static Boolean Equality_AssemblyDefinitionProcessor( AssemblyDefinitionProcessor x, AssemblyDefinitionProcessor y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Processor == y.Processor
            );
      }

      internal static Boolean Equality_AssemblyDefinitionOS( AssemblyDefinitionOS x, AssemblyDefinitionOS y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.OSPlatformID == y.OSPlatformID
            && x.OSMajorVersion == y.OSMajorVersion
            && x.OSMinorVersion == y.OSMinorVersion
            );
      }

#pragma warning restore 618

      internal static Boolean Equality_AssemblyReference( AssemblyReference x, AssemblyReference y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.AssemblyInformation.Equals( y.AssemblyInformation )
             && x.Attributes == y.Attributes
             && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.HashValue, y.HashValue )
             );
      }

#pragma warning disable 618

      internal static Boolean Equality_AssemblyReferenceProcessor( AssemblyReferenceProcessor x, AssemblyReferenceProcessor y )
      {
         return Object.ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.Processor == y.Processor
            && x.AssemblyRef.Equals( y.AssemblyRef )
            );
      }

      internal static Boolean Equality_AssemblyReferenceOS( AssemblyReferenceOS x, AssemblyReferenceOS y )
      {
         return Object.ReferenceEquals( x, y )
            || ( x != null && y != null
            && x.OSPlatformID == y.OSPlatformID
            && x.OSMajorVersion == y.OSMajorVersion
            && x.OSMinorVersion == y.OSMinorVersion
            && x.AssemblyRef == y.AssemblyRef
            );
      }

#pragma warning restore 618

      internal static Boolean Equality_FileReference( FileReference x, FileReference y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && String.Equals( x.Name, y.Name )
             && x.Attributes == y.Attributes
             && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.HashValue, y.HashValue )
             );
      }

      internal static Boolean Equality_ExportedType( ExportedType x, ExportedType y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && String.Equals( x.Name, y.Name )
             && String.Equals( x.Namespace, y.Namespace )
             && x.Attributes == y.Attributes
             && x.Implementation == y.Implementation
             && x.TypeDefinitionIndex == y.TypeDefinitionIndex
             );
      }

      internal static Boolean Equality_ManifestResource( ManifestResource x, ManifestResource y )
      {
         var retVal = Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && String.Equals( x.Name, y.Name )
             && x.Implementation.EqualsTypedEquatable( y.Implementation )
             && ( !x.Implementation.HasValue || x.Offset == y.Offset )
             && x.Attributes == y.Attributes
             && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.EmbeddedData, y.EmbeddedData )
             );

         return retVal;
      }

      internal static Boolean Equality_NestedClassDefinition( NestedClassDefinition x, NestedClassDefinition y )
      {
         return Object.ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.NestedClass == y.NestedClass
             && x.EnclosingClass == y.EnclosingClass
             );
      }

      internal static Boolean Equality_GenericParameterDefinition( GenericParameterDefinition x, GenericParameterDefinition y )
      {
         return ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.GenericParameterIndex == y.GenericParameterIndex
             && String.Equals( x.Name, y.Name )
             && x.Owner == y.Owner
             && x.Attributes == y.Attributes
             );
      }

      internal static Boolean Equality_MethodSpecification( SignatureProvider sigProvider, MethodSpecification x, MethodSpecification y )
      {
         return ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Method == y.Method
             && sigProvider.SignatureEquality( x.Signature, y.Signature )
             );
      }

      internal static Boolean Equality_GenericParameterConstraintDefinition( GenericParameterConstraintDefinition x, GenericParameterConstraintDefinition y )
      {
         return ReferenceEquals( x, y ) ||
             ( x != null && y != null
             && x.Owner == y.Owner
             && x.Constraint == y.Constraint
             );
      }

      internal static Boolean Equality_MethodILDefinition( MethodILDefinition x, MethodILDefinition y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.LocalsSignatureIndex.EqualsTypedEquatable( y.LocalsSignatureIndex )
            && ListEqualityComparer<List<OpCodeInfo>, OpCodeInfo>.ListEquality( x.OpCodes, y.OpCodes, Equality_OpCodeInfo )
            && ListEqualityComparer<List<MethodExceptionBlock>, MethodExceptionBlock>.ListEquality( x.ExceptionBlocks, y.ExceptionBlocks, Equality_MethodExceptionBlock )
            && x.InitLocals == y.InitLocals
            && x.MaxStackSize == y.MaxStackSize
            );
      }

      internal static Boolean Equality_MethodExceptionBlock( MethodExceptionBlock x, MethodExceptionBlock y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.BlockType == y.BlockType
            && x.TryOffset == y.TryOffset
            && x.TryLength == y.TryLength
            && x.HandlerOffset == y.HandlerOffset
            && x.HandlerLength == y.HandlerLength
            && x.ExceptionType.EqualsTypedEquatable( y.ExceptionType )
            && x.FilterOffset == y.FilterOffset
            );
      }

      internal static Boolean Equality_OpCodeInfo( OpCodeInfo x, OpCodeInfo y )
      {
         var retVal = Object.ReferenceEquals( x, y );
         if ( !retVal && x != null && y != null && x.OpCodeID == y.OpCodeID && x.InfoKind == y.InfoKind )
         {
            switch ( x.InfoKind )
            {
               case OpCodeInfoKind.OperandInteger:
                  retVal = ( (IOpCodeInfoWithOperand<Int32>) x ).Operand == ( (IOpCodeInfoWithOperand<Int32>) y ).Operand;
                  break;
               case OpCodeInfoKind.OperandInteger64:
                  retVal = ( (IOpCodeInfoWithOperand<Int64>) x ).Operand == ( (IOpCodeInfoWithOperand<Int64>) y ).Operand;
                  break;
               case OpCodeInfoKind.OperandNone:
                  retVal = true;
                  break;
               case OpCodeInfoKind.OperandR4:
                  // Use .Equals in order for NaN's to work more intuitively
                  retVal = ( (IOpCodeInfoWithOperand<Single>) x ).Operand.Equals( ( (IOpCodeInfoWithOperand<Single>) y ).Operand );
                  break;
               case OpCodeInfoKind.OperandR8:
                  // Use .Equals in order for NaN's to work more intuitively
                  retVal = ( (IOpCodeInfoWithOperand<Double>) x ).Operand.Equals( ( (IOpCodeInfoWithOperand<Double>) y ).Operand );
                  break;
               case OpCodeInfoKind.OperandString:
                  retVal = String.Equals( ( (IOpCodeInfoWithOperand<String>) x ).Operand, ( (IOpCodeInfoWithOperand<String>) y ).Operand );
                  break;
               case OpCodeInfoKind.OperandIntegerList:
                  retVal = ListEqualityComparer<List<Int32>, Int32>.ListEquality( ( (IOpCodeInfoWithOperand<List<Int32>>) x ).Operand, ( (IOpCodeInfoWithOperand<List<Int32>>) y ).Operand );
                  break;
               case OpCodeInfoKind.OperandTableIndex:
                  retVal = ( (IOpCodeInfoWithOperand<TableIndex>) x ).Operand == ( (IOpCodeInfoWithOperand<TableIndex>) y ).Operand;
                  break;
            }
         }
         return retVal;
      }

      internal static Boolean Equality_AssemblyInformation( AssemblyInformation x, AssemblyInformation y )
      {
         return ReferenceEquals( x, y )
            || ( x != null && x.Equals( y ) );
      }


      //internal static Boolean Equality_AbstractMethodSignature_IgnoreKind( AbstractMethodSignature x, AbstractMethodSignature y )
      //{
      //   return Object.ReferenceEquals( x, y ) ||
      //      ( Equality_AbstractMethodSignature_NoReferenceEquals( x, y )
      //      && (
      //         x.SignatureKind != y.SignatureKind
      //         || x.SignatureKind != SignatureKind.MethodReference
      //         || Equality_ParameterSignatures( ( (MethodReferenceSignature) x ).VarArgsParameters, ( (MethodReferenceSignature) y ).VarArgsParameters )
      //         )
      //      );
      //}


      internal static Boolean Equality_AbstractCustomAttributeSignature( AbstractCustomAttributeSignature x, AbstractCustomAttributeSignature y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( x is ResolvedCustomAttributeSignature ?
            Equality_CustomAttributeSignature_NoReferenceEquals( x as ResolvedCustomAttributeSignature, y as ResolvedCustomAttributeSignature ) :
            Equality_RawCustomAttributeSignature_NoReferenceEquals( x as RawCustomAttributeSignature, y as RawCustomAttributeSignature )
         );
      }

      internal static Boolean Equality_RawCustomAttributeSignature_NoReferenceEquals( RawCustomAttributeSignature x, RawCustomAttributeSignature y )
      {
         return x != null && y != null
            && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.Bytes, y.Bytes );
      }

      internal static Boolean Equality_CustomAttributeSignature_NoReferenceEquals( ResolvedCustomAttributeSignature x, ResolvedCustomAttributeSignature y )
      {
         return x != null && y != null
            && ListEqualityComparer<List<CustomAttributeTypedArgument>, CustomAttributeTypedArgument>.ListEquality( x.TypedArguments, y.TypedArguments, Equality_CustomAttributeTypedArgument )
            // TODO should we use set-comparer for named args? since order most likely shouldn't matter...
            && ListEqualityComparer<List<CustomAttributeNamedArgument>, CustomAttributeNamedArgument>.ListEquality( x.NamedArguments, y.NamedArguments, Equality_CustomAttributeNamedArgument );
      }

      internal static Boolean Equality_CustomAttributeTypedArgument( CustomAttributeTypedArgument x, CustomAttributeTypedArgument y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( x != null && y != null
            && Equality_CustomAttributeValue( x.Value, y.Value )
         );
      }

      internal static Boolean Equality_CustomAttributeValue( Object x, Object y )
      {
         return Object.Equals( x, y ) || ( x as Array ).ArraysDeepEqualUntyped( y as Array, Equality_CustomAttributeValue );
      }

      internal static Boolean Equality_CustomAttributeNamedArgument( CustomAttributeNamedArgument x, CustomAttributeNamedArgument y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( x != null && y != null
            && x.TargetKind == y.TargetKind
            && String.Equals( x.Name, y.Name )
            && Equality_CustomAttributeArgumentType( x.FieldOrPropertyType, y.FieldOrPropertyType )
            && Equality_CustomAttributeTypedArgument( x.Value, y.Value ) // Optimize a bit - don't use CustomAttributeTypedArgumentEqualityComparer property
         );
      }

      internal static Boolean Equality_CustomAttributeArgumentType( CustomAttributeArgumentType x, CustomAttributeArgumentType y )
      {
         var retVal = Object.ReferenceEquals( x, y );
         if ( !retVal
            && x != null
            && y != null
            && x.ArgumentTypeKind == y.ArgumentTypeKind
            )
         {
            switch ( x.ArgumentTypeKind )
            {
               case CustomAttributeArgumentTypeKind.Simple:
                  retVal = Equality_CustomAttributeArgumentSimple_NoReferenceEquals( x as CustomAttributeArgumentTypeSimple, y as CustomAttributeArgumentTypeSimple );
                  break;
               case CustomAttributeArgumentTypeKind.Enum:
                  retVal = Equality_CustomAttributeArgumentTypeEnum_NoReferenceEquals( x as CustomAttributeArgumentTypeEnum, y as CustomAttributeArgumentTypeEnum );
                  break;
               case CustomAttributeArgumentTypeKind.Array:
                  retVal = Equality_CustomAttributeArgumentTypeArray_NoReferenceEquals( x as CustomAttributeArgumentTypeArray, y as CustomAttributeArgumentTypeArray );
                  break;
            }
         }

         return retVal;
      }

      internal static Boolean Equality_CustomAttributeArgumentSimple_NoReferenceEquals( CustomAttributeArgumentTypeSimple x, CustomAttributeArgumentTypeSimple y )
      {
         return x != null && y != null && x.SimpleType == y.SimpleType;
      }

      internal static Boolean Equality_CustomAttributeArgumentTypeEnum_NoReferenceEquals( CustomAttributeArgumentTypeEnum x, CustomAttributeArgumentTypeEnum y )
      {
         return x != null && y != null && String.Equals( x.TypeString, y.TypeString );
      }

      internal static Boolean Equality_CustomAttributeArgumentTypeArray_NoReferenceEquals( CustomAttributeArgumentTypeArray x, CustomAttributeArgumentTypeArray y )
      {
         return x != null && y != null && Equality_CustomAttributeArgumentType( x.ArrayType, y.ArrayType );
      }

      internal static Boolean Equality_AbstractSecurityInformation( AbstractSecurityInformation x, AbstractSecurityInformation y )
      {
         return Object.ReferenceEquals( x, y ) ||
         ( x is SecurityInformation ?
            Equality_SecurityInformation_NoReferenceEquals( x as SecurityInformation, y as SecurityInformation ) :
            Equality_RawSecurityInformation_NoReferenceEquals( x as RawSecurityInformation, y as RawSecurityInformation )
         );
      }

      internal static Boolean Equality_AbstractSecurityInformation_NoReferenceEquals( AbstractSecurityInformation x, AbstractSecurityInformation y )
      {
         return x != null && y != null
            && String.Equals( x.SecurityAttributeType, y.SecurityAttributeType );
      }

      internal static Boolean Equality_RawSecurityInformation_NoReferenceEquals( RawSecurityInformation x, RawSecurityInformation y )
      {
         return Equality_AbstractSecurityInformation_NoReferenceEquals( x, y )
            && x.ArgumentCount == y.ArgumentCount
            && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.Bytes, y.Bytes );
      }

      internal static Boolean Equality_SecurityInformation_NoReferenceEquals( SecurityInformation x, SecurityInformation y )
      {
         return Equality_AbstractSecurityInformation_NoReferenceEquals( x, y )
            && ListEqualityComparer<List<CustomAttributeNamedArgument>, CustomAttributeNamedArgument>.ListEquality( x.NamedArguments, y.NamedArguments, Equality_CustomAttributeNamedArgument );
      }

      internal static Boolean Equality_MarshalingInfo( AbstractMarshalingInfo x, AbstractMarshalingInfo y )
      {
         var retVal = Object.ReferenceEquals( x, y );
         if ( !retVal
            && x != null
            && y != null
            && x.MarshalingInfoKind == y.MarshalingInfoKind
            && x.Value == y.Value
            )
         {
            switch ( x.MarshalingInfoKind )
            {
               case MarshalingInfoKind.Simple:
                  retVal = true;
                  break;
               case MarshalingInfoKind.FixedLengthString:
                  retVal = Equality_MarshalingInfo_FixedLengthString_NoReferenceEquals( (FixedLengthStringMarshalingInfo) x, (FixedLengthStringMarshalingInfo) y );
                  break;
               case MarshalingInfoKind.FixedLengthArray:
                  retVal = Equality_MarshalingInfo_FixedLengthArray_NoReferenceEquals( (FixedLengthArrayMarshalingInfo) x, (FixedLengthArrayMarshalingInfo) y );
                  break;
               case MarshalingInfoKind.SafeArray:
                  retVal = Equality_MarshalingInfo_SafeArray_NoReferenceEquals( (SafeArrayMarshalingInfo) x, (SafeArrayMarshalingInfo) y );
                  break;
               case MarshalingInfoKind.Array:
                  retVal = Equality_MarshalingInfo_Array_NoReferenceEquals( (ArrayMarshalingInfo) x, (ArrayMarshalingInfo) y );
                  break;
               case MarshalingInfoKind.Interface:
                  retVal = Equality_MarshalingInfo_Interface_NoReferenceEquals( (InterfaceMarshalingInfo) x, (InterfaceMarshalingInfo) y );
                  break;
               case MarshalingInfoKind.Custom:
                  retVal = Equality_MarshalingInfo_Custom_NoReferenceEquals( (CustomMarshalingInfo) x, (CustomMarshalingInfo) y );
                  break;
               case MarshalingInfoKind.Raw:
                  retVal = Equality_MarshalingInfo_Raw_NoReferenceEquals( (RawMarshalingInfo) x, (RawMarshalingInfo) y );
                  break;
            }
         }

         return retVal;
      }

      internal static Boolean Equality_MarshalingInfo_FixedLengthString_NoReferenceEquals( FixedLengthStringMarshalingInfo x, FixedLengthStringMarshalingInfo y )
      {
         return x.Size == y.Size;
      }

      internal static Boolean Equality_MarshalingInfo_FixedLengthArray_NoReferenceEquals( FixedLengthArrayMarshalingInfo x, FixedLengthArrayMarshalingInfo y )
      {
         return x.Size == y.Size
            && x.ElementType == y.ElementType;
      }

      internal static Boolean Equality_MarshalingInfo_SafeArray_NoReferenceEquals( SafeArrayMarshalingInfo x, SafeArrayMarshalingInfo y )
      {
         return x.ElementType == y.ElementType
            && String.Equals( x.UserDefinedType, y.UserDefinedType );
      }

      internal static Boolean Equality_MarshalingInfo_Array_NoReferenceEquals( ArrayMarshalingInfo x, ArrayMarshalingInfo y )
      {
         return x.ElementType == y.ElementType
            && x.SizeParameterIndex == y.SizeParameterIndex
            && x.Size == y.Size
            && x.SizeParameterMultiplier == y.SizeParameterMultiplier;
      }

      internal static Boolean Equality_MarshalingInfo_Interface_NoReferenceEquals( InterfaceMarshalingInfo x, InterfaceMarshalingInfo y )
      {
         return x.IIDParameterIndex == y.IIDParameterIndex;
      }

      internal static Boolean Equality_MarshalingInfo_Custom_NoReferenceEquals( CustomMarshalingInfo x, CustomMarshalingInfo y )
      {
         return String.Equals( x.CustomMarshalerTypeName, y.CustomMarshalerTypeName )
            && String.Equals( x.MarshalCookie, y.MarshalCookie )
            && String.Equals( x.GUIDString, y.GUIDString )
            && String.Equals( x.NativeTypeName, y.NativeTypeName );
      }

      internal static Boolean Equality_MarshalingInfo_Raw_NoReferenceEquals( RawMarshalingInfo x, RawMarshalingInfo y )
      {
         return ArrayEqualityComparer<Byte>.ArrayEquality( x.Bytes, y.Bytes );
      }



      internal static Int32 HashCode_MetaData( CILMetaData x )
      {
         return x == null ?
            0 :
            ArrayEqualityComparer<Int32>.DefaultArrayEqualityComparer.GetHashCode( new[] { x.TypeDefinitions.GetRowCount(), x.MethodDefinitions.GetRowCount(), x.ParameterDefinitions.GetRowCount(), x.FieldDefinitions.GetRowCount() } );
      }

      internal static Int32 HashCode_ModuleDefinition( ModuleDefinition x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe( 1 );
      }

      internal static Int32 HashCode_TypeReference( TypeReference x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + x.Namespace.GetHashCodeSafe( 1 ) );
      }

      internal static Int32 HashCode_TypeDefinition( TypeDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + x.Namespace.GetHashCodeSafe( 1 ) );
      }

      internal static Int32 HashCode_FieldDefinitionPointer( FieldDefinitionPointer x )
      {
         return x == null ? 0 : x.FieldIndex.GetHashCode();
      }

      internal static Int32 HashCode_FieldDefinition( FieldDefinition x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe( 1 ); // TODO might need to include something else to hashcode?
      }

      internal static Int32 HashCode_MethodDefinitionPointer( MethodDefinitionPointer x )
      {
         return x == null ? 0 : x.MethodIndex.GetHashCode();
      }

      internal static Int32 HashCode_MethodDefinition( MethodDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + x.Signature.Parameters.Count );
      }

      internal static Int32 HashCode_ParameterDefinitionPointer( ParameterDefinitionPointer x )
      {
         return x == null ? 0 : x.ParameterIndex.GetHashCode();
      }

      internal static Int32 HashCode_ParameterDefinition( ParameterDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + x.Sequence );
      }

      internal static Int32 HashCode_InterfaceImplementation( InterfaceImplementation x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Class.GetHashCode() ) * 23 + x.Interface.GetHashCode() );
      }

      internal static Int32 HashCode_MemberReference( MemberReference x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + x.DeclaringType.GetHashCode() );
      }

      internal static Int32 HashCode_ConstantDefinition( ConstantDefinition x )
      {
         return x == null ? 0 : x.Parent.GetHashCode();
      }

      internal static Int32 HashCode_CustomAttributeDefinition( CustomAttributeDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Parent.GetHashCode() ) * 23 + x.Type.GetHashCode() );
      }

      internal static Int32 HashCode_FieldMarshal( FieldMarshal x )
      {
         return x == null ? 0 : x.Parent.GetHashCode();
      }

      internal static Int32 HashCode_SecurityDefinition( SecurityDefinition x )
      {
         return x == null ? 0 : x.Parent.GetHashCode();
      }

      internal static Int32 HashCode_ClassLayout( ClassLayout x )
      {
         return x == null ? 0 : x.Parent.GetHashCode();
      }

      internal static Int32 HashCode_FieldLayout( FieldLayout x )
      {
         return x == null ? 0 : x.Field.GetHashCode();
      }

      internal static Int32 HashCode_StandaloneSignature( SignatureProvider sigProvider, StandaloneSignature x )
      {
         return sigProvider.SignatureHashCode( x?.Signature );
      }

      internal static Int32 HashCode_EventMap( EventMap x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Parent.GetHashCode() ) * 23 + x.EventList.GetHashCode() );
      }

      internal static Int32 HashCode_EventDefinitionPointer( EventDefinitionPointer x )
      {
         return x == null ? 0 : x.EventIndex.GetHashCode();
      }

      internal static Int32 HashCode_EventDefinition( EventDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + x.EventType.GetHashCode() );
      }

      internal static Int32 HashCode_PropertyMap( PropertyMap x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Parent.GetHashCode() ) * 23 + x.PropertyList.GetHashCode() );
      }

      internal static Int32 HashCode_PropertyDefinitionPointer( PropertyDefinitionPointer x )
      {
         return x == null ? 0 : x.PropertyIndex.GetHashCode();
      }

      internal static Int32 HashCode_PropertyDefinition( SignatureProvider sigProvider, PropertyDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + sigProvider.SignatureHashCode( x.Signature ) );
      }

      internal static Int32 HashCode_MethodSemantics( MethodSemantics x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Method.GetHashCode() ) * 23 + x.Associaton.GetHashCode() );
      }

      internal static Int32 HashCode_MethodImplementation( MethodImplementation x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Class.GetHashCode() ) * 23 + x.MethodBody.GetHashCode() );
      }

      internal static Int32 HashCode_ModuleReference( ModuleReference x )
      {
         return x == null ? 0 : x.ModuleName.GetHashCodeSafe( 1 );
      }

      internal static Int32 HashCode_TypeSpecification( SignatureProvider sigProvider, TypeSpecification x )
      {
         return sigProvider.SignatureHashCode( x?.Signature );
      }

      internal static Int32 HashCode_MethodImplementationMap( MethodImplementationMap x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.ImportName.GetHashCodeSafe( 1 ) ) * 23 + x.MemberForwarded.GetHashCode() );
      }

      internal static Int32 HashCode_FieldRVA( FieldRVA x )
      {
         return x == null ? 0 : x.Field.GetHashCode();
      }

      internal static Int32 HashCode_EditAndContinueLog( EditAndContinueLog x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Token ) * 23 + x.FuncCode );
      }

      internal static Int32 HashCode_EditAndContinueMap( EditAndContinueMap x )
      {
         return x == null ? 0 : ( 17 * 23 + x.Token );
      }

      internal static Int32 HashCode_AssemblyDefinition( AssemblyDefinition x )
      {
         return x == null ? 0 : x.AssemblyInformation.GetHashCodeSafe( 1 );
      }

#pragma warning disable 618

      internal static Int32 HashCode_AssemblyDefinitionProcessor( AssemblyDefinitionProcessor x )
      {
         return x == null ? 0 : ( 17 * 23 + x.Processor );
      }

      internal static Int32 HashCode_AssemblyDefinitionOS( AssemblyDefinitionOS x )
      {
         return x == null ? 0 : ( ( ( 17 * 23 + x.OSPlatformID ) * 23 + x.OSMajorVersion ) * 23 + x.OSMinorVersion );
      }

#pragma warning restore 618

      internal static Int32 HashCode_AssemblyReference( AssemblyReference x )
      {
         return x == null ? 0 : x.AssemblyInformation.GetHashCodeSafe( 1 );
      }

#pragma warning disable 618
      internal static Int32 HashCode_AssemblyReferenceProcessor( AssemblyReferenceProcessor x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Processor ) * 23 + x.AssemblyRef.GetHashCode() );
      }

      internal static Int32 HashCode_AssemblyReferenceOS( AssemblyReferenceOS x )
      {
         return x == null ? 0 : ( ( ( ( 17 * 23 + x.OSPlatformID ) * 23 + x.OSMajorVersion ) * 23 + x.OSMinorVersion ) * 23 + x.AssemblyRef.GetHashCode() );
      }
#pragma warning restore 618

      internal static Int32 HashCode_FileReference( FileReference x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe( 1 );
      }

      internal static Int32 HashCode_ExportedType( ExportedType x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe( 1 );
      }

      internal static Int32 HashCode_ManifestResource( ManifestResource x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe( 1 );
      }

      internal static Int32 HashCode_NestedClassDefinition( NestedClassDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.NestedClass.GetHashCode() ) * 23 + x.EnclosingClass.GetHashCode() );
      }

      internal static Int32 HashCode_GenericParameterDefinition( GenericParameterDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + x.GenericParameterIndex );
      }

      internal static Int32 HashCode_MethodSpecification( SignatureProvider sigProvider, MethodSpecification x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Method.GetHashCode() ) * 23 + sigProvider.SignatureHashCode( x.Signature ) );
      }

      internal static Int32 HashCode_GenericParameterConstraintDefinition( GenericParameterConstraintDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Owner.GetHashCode() ) * 23 + x.Constraint.GetHashCode() );
      }

      internal static Int32 HashCode_MethodILDefinition( MethodILDefinition x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.LocalsSignatureIndex.GetHashCodeSafe() ) * 23 + SequenceEqualityComparer<IEnumerable<OpCodeInfo>, OpCodeInfo>.SequenceHashCode( x.OpCodes.Take( 10 ), HashCode_OpCodeInfo ) );
      }

      internal static Int32 HashCode_MethodExceptionBlock( MethodExceptionBlock x )
      {
         return x == null ? 0 : ( ( ( 17 * 23 + (Int32) x.BlockType ) * 23 + x.TryOffset ) * 23 + x.TryLength );
      }

      internal static Int32 HashCode_OpCodeInfo( OpCodeInfo x )
      {
         Int32 retVal;
         if ( x == null )
         {
            retVal = 0;
         }
         else
         {
            retVal = 17 * 23 + x.OpCodeID.GetHashCode();
            var infoKind = x.InfoKind;
            if ( infoKind != OpCodeInfoKind.OperandNone )
            {
               Int32 operandHashCode;
               switch ( infoKind )
               {
                  case OpCodeInfoKind.OperandInteger:
                     operandHashCode = ( (IOpCodeInfoWithOperand<Int32>) x ).Operand;
                     break;
                  case OpCodeInfoKind.OperandInteger64:
                     operandHashCode = ( (IOpCodeInfoWithOperand<Int64>) x ).Operand.GetHashCode();
                     break;
                  case OpCodeInfoKind.OperandR4:
                     operandHashCode = ( (IOpCodeInfoWithOperand<Single>) x ).Operand.GetHashCode();
                     break;
                  case OpCodeInfoKind.OperandR8:
                     operandHashCode = ( (IOpCodeInfoWithOperand<Double>) x ).Operand.GetHashCode();
                     break;
                  case OpCodeInfoKind.OperandString:
                     operandHashCode = ( (IOpCodeInfoWithOperand<String>) x ).Operand.GetHashCodeSafe();
                     break;
                  case OpCodeInfoKind.OperandTableIndex:
                     operandHashCode = ( (IOpCodeInfoWithOperand<TableIndex>) x ).Operand.GetHashCode();
                     break;
                  default:
                     operandHashCode = 0;
                     break;
               }
               retVal = retVal * 23 + operandHashCode;
            }
         }

         return retVal;
      }

      internal static Int32 HashCode_AssemblyInformation( AssemblyInformation x )
      {
         return x.GetHashCodeSafe();
      }


      internal static Int32 HashCode_AbstractCustomAttributeSignature( AbstractCustomAttributeSignature x )
      {
         return x == null ? 0 : ( x is ResolvedCustomAttributeSignature ? HashCode_CustomAttributeSignature( x as ResolvedCustomAttributeSignature ) : HashCode_RawCustomAttributeSignature( x as RawCustomAttributeSignature ) );
      }

      internal static Int32 HashCode_RawCustomAttributeSignature( RawCustomAttributeSignature x )
      {
         return x == null ? 0 : ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.GetHashCode( x.Bytes );
      }

      internal static Int32 HashCode_CustomAttributeSignature( ResolvedCustomAttributeSignature x )
      {
         return x == null ? 0 : ListEqualityComparer<List<CustomAttributeTypedArgument>, CustomAttributeTypedArgument>.ListHashCode( x.TypedArguments, HashCode_CustomAttributeTypedArgument );
      }

      internal static Int32 HashCode_CustomAttributeTypedArgument( CustomAttributeTypedArgument x )
      {
         return x == null ? 0 : HashCode_CustomAttributeValue( x.Value );
      }

      internal static Int32 HashCode_CustomAttributeValue( Object x )
      {
         return x == null ? 0 : ( x is Array ? HashCode_Array( x as Array ) : x.GetHashCode() );
      }

      internal static Int32 HashCode_Array( Array x )
      {
         Int32 retVal;
         if ( x == null )
         {
            retVal = 0;
         }
         else
         {
            retVal = 17;
            var max = x.Length;
            if ( max > 0 )
            {
               unchecked
               {
                  for ( var i = 0; i < max; ++i )
                  {
                     retVal = retVal * 23 + HashCode_CustomAttributeValue( x.GetValue( i ) );
                  }
               }
            }
         }

         return retVal;
      }

      internal static Int32 HashCode_CustomAttributeNamedArgument( CustomAttributeNamedArgument x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + HashCode_CustomAttributeTypedArgument( x.Value ) );
      }

      internal static Int32 HashCode_CustomAttributeArgumentType( CustomAttributeArgumentType x )
      {
         Int32 retVal;
         if ( x == null )
         {
            retVal = 0;
         }
         else
         {
            var s = x as CustomAttributeArgumentTypeSimple;
            if ( s != null )
            {
               retVal = s.SimpleType.GetHashCode();
            }
            else
            {
               var e = x as CustomAttributeArgumentTypeEnum;
               retVal = e != null ? e.TypeString.GetHashCodeSafe() : x.ArgumentTypeKind.GetHashCode();
            }
         }
         return retVal;
      }

      internal static Int32 HashCode_AbstractSecurityInformation( AbstractSecurityInformation x )
      {
         return x == null ? 0 : ( x is SecurityInformation ? HashCode_SecurityInformation( x as SecurityInformation ) : HashCode_RawSecurityInformation( x as RawSecurityInformation ) );
      }

      internal static Int32 HashCode_RawSecurityInformation( RawSecurityInformation x )
      {
         return x == null ? 0 : ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.GetHashCode( x.Bytes );
      }

      internal static Int32 HashCode_SecurityInformation( SecurityInformation x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.SecurityAttributeType.GetHashCodeSafe( 1 ) ) * 23 + ListEqualityComparer<List<CustomAttributeNamedArgument>, CustomAttributeNamedArgument>.ListHashCode( x.NamedArguments, HashCode_CustomAttributeNamedArgument ) );
      }

      internal static Int32 HashCode_MarshalingInfo( AbstractMarshalingInfo x )
      {
         return x == null ? 0 : ( ( 17 * 23 + (Int32) x.MarshalingInfoKind ) * 23 + (Int32) x.Value );
      }
   }


}