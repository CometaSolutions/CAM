/*
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
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;

using CILAssemblyManipulator.Physical.IO;
using CollectionsWithRoles.API;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace CILAssemblyManipulator.Physical
{
   public static class Comparers
   {
      /// <summary>
      /// Gets the <see cref="IEqualityComparer{T}"/> to use when comparing equality of two <see cref="ImageInformation"/> on a logical level.
      /// This means that some properties will be left out of comparison.
      /// </summary>
      /// <value>The <see cref="IEqualityComparer{T}"/> to use when comparing equality of two <see cref="ImageInformation"/> on a logical level</value>
      /// <remarks>
      /// The properties left out of comparison are:
      /// <list type="bullet">
      /// <item><description><see cref="PEInformation.SectionHeaders"/>,</description></item>
      /// <item><description><see cref="DOSHeader.NTHeaderOffset"/>,</description></item>
      /// <item><description><see cref="FileHeader.NumberOfSections"/>,</description></item>
      /// <item><description><see cref="FileHeader.PointerToSymbolTable"/>,</description></item>
      /// <item><description><see cref="FileHeader.NumberOfSymbols"/>,</description></item>
      /// <item><description><see cref="FileHeader.OptionalHeaderSize"/>,</description></item>
      /// <item><description><see cref="OptionalHeader.OptionalHeaderKind"/>,</description></item>
      /// <item><description><see cref="OptionalHeader.SizeOfCode"/>,</description></item>
      /// <item><description><see cref="OptionalHeader.SizeOfInitializedData"/>,</description></item>
      /// <item><description><see cref="OptionalHeader.SizeOfUninitializedData"/>,</description></item>
      /// <item><description><see cref="OptionalHeader.EntryPointRVA"/>,</description></item>
      /// <item><description><see cref="OptionalHeader.BaseOfCodeRVA"/>,</description></item>
      /// <item><description><see cref="OptionalHeader.BaseOfDataRVA"/>,</description></item>
      /// <item><description><see cref="OptionalHeader.ImageSize"/>,</description></item>
      /// <item><description><see cref="OptionalHeader.HeaderSize"/>,</description></item>
      /// <item><description><see cref="OptionalHeader.FileChecksum"/>,</description></item>
      /// <item><description><see cref="OptionalHeader.DataDirectories"/>,</description></item>
      /// <item><description><see cref="CLIInformation.StrongNameSignature"/>,</description></item>
      /// <item><description><see cref="CLIInformation.MethodRVAs"/> (only size is compared, not contents),</description></item>
      /// <item><description><see cref="CLIInformation.FieldRVAs"/> (only size is compared, not contents),</description></item>
      /// <item><description><see cref="CLIHeader.HeaderSize"/>,</description></item>
      /// <item><description><see cref="CLIHeader.MetaData"/>,</description></item>
      /// <item><description><see cref="CLIHeader.Resources"/>,</description></item>
      /// <item><description><see cref="CLIHeader.StrongNameSignature"/>,</description></item>
      /// <item><description><see cref="CLIHeader.CodeManagerTable"/>,</description></item>
      /// <item><description><see cref="CLIHeader.VTableFixups"/>,</description></item>
      /// <item><description><see cref="CLIHeader.ExportAddressTableJumps"/>,</description></item>
      /// <item><description><see cref="CLIHeader.ManagedNativeHeader"/>,</description></item>
      /// <item><description><see cref="MetaDataRoot.VersionStringLength"/>,</description></item>
      /// <item><description><see cref="MetaDataRoot.VersionString"/> (constructed from <see cref="MetaDataRoot.VersionStringBytes"/>),</description></item>
      /// <item><description><see cref="MetaDataRoot.NumberOfStreams"/>,</description></item>
      /// <item><description><see cref="MetaDataRoot.StreamHeaders"/>,</description></item>
      /// <item><description><see cref="MetaDataTableStreamHeader.TableStreamFlags"/>,</description></item>
      /// <item><description><see cref="MetaDataTableStreamHeader.PresentTablesBitVector"/>,</description></item>
      /// <item><description><see cref="MetaDataTableStreamHeader.SortedTablesBitVector"/>,</description></item>
      /// <item><description><see cref="MetaDataTableStreamHeader.TableSizes"/>,</description></item>
      /// <item><description><see cref="DebugInformation.DataRVA"/>,</description></item>
      /// <item><description><see cref="DebugInformation.DataPointer"/>,</description></item>
      /// <item><description><see cref="DebugInformation.DataSize"/>, and</description></item>
      /// <item><description><see cref="DebugInformation.DataRVA"/>.</description></item>
      /// </list>
      /// </remarks>
      public static IEqualityComparer<ImageInformation> ImageInformationLogicalEqualityComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="ClassLayout"/> according to ECMA-335 standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="ClassLayout"/> according to ECMA-335 standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="TableIndex.Index"/> property of <see cref="ClassLayout.Parent"/> to perform comparison.
      /// </remarks>
      public static IComparer<ClassLayout> ClassLayoutComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="ConstantDefinition"/> according to ECMA-335 standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="ConstantDefinition"/> according to ECMA-335 standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="HasConstantComparer"/> for <see cref="ConstantDefinition.Parent"/> property to perform comparison.
      /// </remarks>
      public static IComparer<ConstantDefinition> ConstantDefinitionComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="CustomAttributeDefinition"/> according to ECMA-335 standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="CustomAttributeDefinition"/> according to ECMA-335 standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="HasCustomAttributeComparer"/> for <see cref="CustomAttributeDefinition.Parent"/> property to perform comparison.
      /// </remarks>
      public static IComparer<CustomAttributeDefinition> CustomAttributeDefinitionComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="CustomAttributeDefinition"/> according to ECMA-335 standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="CustomAttributeDefinition"/> according to ECMA-335 standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="HasCustomAttributeComparer"/> for <see cref="CustomAttributeDefinition.Parent"/> property to perform comparison.
      /// </remarks>
      public static IComparer<SecurityDefinition> SecurityDefinitionComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="FieldLayout"/> according to ECMA-335 standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="FieldLayout"/> according to ECMA-335 standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="TableIndex.Index"/> property of <see cref="FieldLayout.Field"/> to perform comparison.
      /// </remarks>
      public static IComparer<FieldLayout> FieldLayoutComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="FieldMarshal"/> according to ECMA-335 standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="FieldMarshal"/> according to ECMA-335 standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="HasFieldMarshalComparer"/> for <see cref="FieldMarshal.Parent"/> property to perform comparison.
      /// </remarks>
      public static IComparer<FieldMarshal> FieldMarshalComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="FieldRVA"/> according to ECMA-335 standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="FieldRVA"/> according to ECMA-335 standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="TableIndex.Index"/> property of <see cref="FieldRVA.Field"/> to perform comparison.
      /// </remarks>
      public static IComparer<FieldRVA> FieldRVAComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="GenericParameterDefinition"/> according to ECMA-335 standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="GenericParameterDefinition"/> according to ECMA-335 standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="TypeOrMethodDefComparer"/> for <see cref="GenericParameterDefinition.Owner"/> property as primary key, and <see cref="GenericParameterDefinition.GenericParameterIndex"/> as secondary key to perform comparison.
      /// </remarks>
      public static IComparer<GenericParameterDefinition> GenericParameterDefinitionComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="GenericParameterConstraintDefinition"/> according to ECMA-335 standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="GenericParameterConstraintDefinition"/> according to ECMA-335 standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="TableIndex.Index"/> property of <see cref="GenericParameterConstraintDefinition.Owner"/> to perform comparison.
      /// </remarks>
      public static IComparer<GenericParameterConstraintDefinition> GenericParameterConstraintDefinitionComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="MethodImplementationMap"/> according to ECMA-335 standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="MethodImplementationMap"/> according to ECMA-335 standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="MemberForwardedComparer"/> for <see cref="MethodImplementationMap.MemberForwarded"/> property to perform comparison.
      /// </remarks>
      public static IComparer<MethodImplementationMap> MethodImplementationMapComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="InterfaceImplementation"/> according to ECMA-335 standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="InterfaceImplementation"/> according to ECMA-335 standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="TableIndex.Index"/> property of the <see cref="InterfaceImplementation.Class"/> property as primary key, and the <see cref="TableIndex.Index"/> property of the <see cref="InterfaceImplementation.Interface"/> as secondary key to perform comparison.
      /// </remarks>
      public static IComparer<InterfaceImplementation> InterfaceImplementationComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="MethodImplementation"/> according to ECMA-335 standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="MethodImplementation"/> according to ECMA-335 standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="TableIndex.Index"/> property of <see cref="MethodImplementation.Class"/> to perform comparison.
      /// </remarks>
      public static IComparer<MethodImplementation> MethodImplementationComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="MethodSemantics"/> according to ECMA-335 standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="MethodSemantics"/> according to ECMA-335 standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="HasSemanticsComparer"/> for <see cref="MethodSemantics.Associaton"/> property to perform comparison.
      /// </remarks>
      public static IComparer<MethodSemantics> MethodSemanticsComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="NestedClassDefinition"/> according to ECMA-335 standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="NestedClassDefinition"/> according to ECMA-335 standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="TableIndex.Index"/> property of <see cref="NestedClassDefinition.NestedClass"/> to perform comparison.
      /// </remarks>
      public static IComparer<NestedClassDefinition> NestedClassDefinitionComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="TableIndex"/> properties, possible tables for which are defined in <see cref="Meta.DefaultMetaDataTableInformationProvider.HasConstant"/>.
      /// </summary>
      /// <seealso cref="CodedTableIndexComparer"/>
      /// <seealso cref="Meta.DefaultMetaDataTableInformationProvider.HasConstant"/>
      public static CodedTableIndexComparer HasConstantComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="TableIndex"/> properties, possible tables for which are defined in <see cref="Meta.DefaultMetaDataTableInformationProvider.HasCustomAttribute"/>.
      /// </summary>
      /// <seealso cref="CodedTableIndexComparer"/>
      /// <seealso cref="Meta.DefaultMetaDataTableInformationProvider.HasCustomAttribute"/>
      public static CodedTableIndexComparer HasCustomAttributeComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="TableIndex"/> properties, possible tables for which are defined in <see cref="Meta.DefaultMetaDataTableInformationProvider.HasFieldMarshal"/>.
      /// </summary>
      /// <seealso cref="CodedTableIndexComparer"/>
      /// <seealso cref="Meta.DefaultMetaDataTableInformationProvider.HasFieldMarshal"/>
      public static CodedTableIndexComparer HasFieldMarshalComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="TableIndex"/> properties, possible tables for which are defined in <see cref="Meta.DefaultMetaDataTableInformationProvider.HasSecurity"/>.
      /// </summary>
      /// <seealso cref="CodedTableIndexComparer"/>
      /// <seealso cref="Meta.DefaultMetaDataTableInformationProvider.HasSecurity"/>
      public static CodedTableIndexComparer HasDeclSecurityComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="TableIndex"/> properties, possible tables for which are defined in <see cref="Meta.DefaultMetaDataTableInformationProvider.HasSemantics"/>.
      /// </summary>
      /// <seealso cref="CodedTableIndexComparer"/>
      /// <seealso cref="Meta.DefaultMetaDataTableInformationProvider.HasSemantics"/>
      public static CodedTableIndexComparer HasSemanticsComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="TableIndex"/> properties, possible tables for which are defined in <see cref="Meta.DefaultMetaDataTableInformationProvider.MemberForwarded"/>.
      /// </summary>
      /// <seealso cref="CodedTableIndexComparer"/>
      /// <seealso cref="Meta.DefaultMetaDataTableInformationProvider.MemberForwarded"/>
      public static CodedTableIndexComparer MemberForwardedComparer { get; }

      /// <summary>
      /// Gets the ordering comparer for <see cref="TableIndex"/> properties, possible tables for which are defined in <see cref="Meta.DefaultMetaDataTableInformationProvider.TypeOrMethodDef"/>.
      /// </summary>
      /// <seealso cref="CodedTableIndexComparer"/>
      /// <seealso cref="Meta.DefaultMetaDataTableInformationProvider.TypeOrMethodDef"/>
      public static CodedTableIndexComparer TypeOrMethodDefComparer { get; }

      static Comparers()
      {
         ImageInformationLogicalEqualityComparer = ComparerFromFunctions.NewEqualityComparer<ImageInformation>( Equality_ImageInformation_Logical, HashCode_HeadersData );

         ClassLayoutComparer = ComparerFromFunctions.NewComparerWithNullStrategy<ClassLayout>( Comparison_ClassLayout, NullSorting.NullsLast );
         ConstantDefinitionComparer = ComparerFromFunctions.NewComparerWithNullStrategy<ConstantDefinition>( Comparison_ConstantDefinition, NullSorting.NullsLast );
         CustomAttributeDefinitionComparer = ComparerFromFunctions.NewComparerWithNullStrategy<CustomAttributeDefinition>( Comparison_CustomAttributeDefinition, NullSorting.NullsLast );
         SecurityDefinitionComparer = ComparerFromFunctions.NewComparerWithNullStrategy<SecurityDefinition>( Comparison_SecurityDefinition, NullSorting.NullsLast );
         FieldLayoutComparer = ComparerFromFunctions.NewComparerWithNullStrategy<FieldLayout>( Comparison_FieldLayout, NullSorting.NullsLast );
         FieldMarshalComparer = ComparerFromFunctions.NewComparerWithNullStrategy<FieldMarshal>( Comparison_FieldMarshal, NullSorting.NullsLast );
         FieldRVAComparer = ComparerFromFunctions.NewComparerWithNullStrategy<FieldRVA>( Comparison_FieldRVA, NullSorting.NullsLast );
         GenericParameterDefinitionComparer = ComparerFromFunctions.NewComparerWithNullStrategy<GenericParameterDefinition>( Comparison_GenericParameterDefinition, NullSorting.NullsLast );
         GenericParameterConstraintDefinitionComparer = ComparerFromFunctions.NewComparerWithNullStrategy<GenericParameterConstraintDefinition>( Comparison_GenericParameterConstraintDefinition, NullSorting.NullsLast );
         MethodImplementationMapComparer = ComparerFromFunctions.NewComparerWithNullStrategy<MethodImplementationMap>( Comparison_MethodImplementationMap, NullSorting.NullsLast );
         InterfaceImplementationComparer = ComparerFromFunctions.NewComparerWithNullStrategy<InterfaceImplementation>( Comparison_InterfaceImplementation, NullSorting.NullsLast );
         MethodImplementationComparer = ComparerFromFunctions.NewComparerWithNullStrategy<MethodImplementation>( Comparison_MethodImplementation, NullSorting.NullsLast );
         MethodSemanticsComparer = ComparerFromFunctions.NewComparerWithNullStrategy<MethodSemantics>( Comparison_MethodSemantics, NullSorting.NullsLast );
         NestedClassDefinitionComparer = ComparerFromFunctions.NewComparerWithNullStrategy<NestedClassDefinition>( Comparison_NestedClassDefinition, NullSorting.NullsLast );

         HasConstantComparer = new CodedTableIndexComparer( Meta.DefaultMetaDataTableInformationProvider.HasConstant );
         HasCustomAttributeComparer = new CodedTableIndexComparer( Meta.DefaultMetaDataTableInformationProvider.HasCustomAttribute );
         HasFieldMarshalComparer = new CodedTableIndexComparer( Meta.DefaultMetaDataTableInformationProvider.HasFieldMarshal );
         HasDeclSecurityComparer = new CodedTableIndexComparer( Meta.DefaultMetaDataTableInformationProvider.HasSecurity );
         HasSemanticsComparer = new CodedTableIndexComparer( Meta.DefaultMetaDataTableInformationProvider.HasSemantics );
         MemberForwardedComparer = new CodedTableIndexComparer( Meta.DefaultMetaDataTableInformationProvider.MemberForwarded );
         TypeOrMethodDefComparer = new CodedTableIndexComparer( Meta.DefaultMetaDataTableInformationProvider.TypeOrMethodDef );
      }

      private static Boolean Equality_ImageInformation_Logical( ImageInformation x, ImageInformation y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && Equality_PEInformation_Logical( x.PEInformation, y.PEInformation )
            && Equality_CLIInformation_Logical( x.CLIInformation, y.CLIInformation )
            && Equality_DebugInfo_Logical( x.DebugInformation, y.DebugInformation )
            );
      }

      private static Boolean Equality_PEInformation_Logical( PEInformation x, PEInformation y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && Equality_DOSHeader_Logical( x.DOSHeader, y.DOSHeader )
            && Equality_NTHeader_Logical( x.NTHeader, y.NTHeader )
            );
      }

      private static Boolean Equality_DOSHeader_Logical( DOSHeader x, DOSHeader y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Signature == y.Signature
            );
      }

      private static Boolean Equality_NTHeader_Logical( NTHeader x, NTHeader y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Signature == y.Signature
            && Equality_FileHeader_Logical( x.FileHeader, y.FileHeader )
            && Equality_OptionalHeader_Logical( x.OptionalHeader, y.OptionalHeader )
            );
      }

      private static Boolean Equality_FileHeader_Logical( FileHeader x, FileHeader y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Machine == y.Machine
            && x.TimeDateStamp == y.TimeDateStamp
            && x.Characteristics == y.Characteristics
            );
      }

      private static Boolean Equality_OptionalHeader_Logical( OptionalHeader x, OptionalHeader y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.MajorLinkerVersion == y.MajorLinkerVersion
            && x.MinorLinkerVersion == y.MinorLinkerVersion
            && x.ImageBase == y.ImageBase
            && x.SectionAlignment == y.SectionAlignment
            && x.FileAlignment == y.FileAlignment
            && x.MajorOSVersion == y.MajorOSVersion
            && x.MinorOSVersion == y.MinorOSVersion
            && x.MajorUserVersion == y.MajorUserVersion
            && x.MinorUserVersion == y.MinorUserVersion
            && x.MajorSubsystemVersion == y.MajorSubsystemVersion
            && x.MinorSubsystemVersion == y.MinorSubsystemVersion
            && x.Win32VersionValue == y.Win32VersionValue
            && x.Subsystem == y.Subsystem
            && x.DLLCharacteristics == y.DLLCharacteristics
            && x.StackReserveSize == y.StackReserveSize
            && x.StackCommitSize == y.StackCommitSize
            && x.HeapReserveSize == y.HeapReserveSize
            && x.HeapCommitSize == y.HeapCommitSize
            && x.LoaderFlags == y.LoaderFlags
            && x.NumberOfDataDirectories == y.NumberOfDataDirectories
            );
      }

      private static Boolean Equality_CLIInformation_Logical( CLIInformation x, CLIInformation y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && Equality_CLIHeader_Logical( x.CLIHeader, y.CLIHeader )
            && Equality_MDRoot_Logical( x.MetaDataRoot, y.MetaDataRoot )
            && Equality_MDTableStreamHeader( x.TableStreamHeader, y.TableStreamHeader )
            && Equality_DataReferences_Logical( x.DataReferences, y.DataReferences )
            );
      }

      private static Boolean Equality_DataReferences_Logical( DataReferencesInfo x, DataReferencesInfo y )
      {
         // TODO SequenceEquality -methods to DictionaryEqualityComparer
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.DataReferences.DictionaryQueryEquality( y.DataReferences, ( xDataRefs, yDataRefs ) => xDataRefs.DictionaryQueryEquality( yDataRefs, ( xColRefs, yColRefs ) => xColRefs.Count == yColRefs.Count ) )
            );
      }

      private static Boolean Equality_CLIHeader_Logical( CLIHeader x, CLIHeader y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.MajorRuntimeVersion == y.MajorRuntimeVersion
            && x.MinorRuntimeVersion == y.MinorRuntimeVersion
            && x.Flags == y.Flags
            && x.EntryPointToken == y.EntryPointToken
            );
      }

      private static Boolean Equality_MDRoot_Logical( MetaDataRoot x, MetaDataRoot y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Signature == y.Signature
            && x.MajorVersion == y.MajorVersion
            && x.MinorVersion == y.MinorVersion
            && x.Reserved == y.Reserved
            && x.VersionStringBytes.ArrayQueryEquality( y.VersionStringBytes )
            && x.StorageFlags == y.StorageFlags
            && x.Reserved2 == y.Reserved2
            );
      }

      private static Boolean Equality_MDTableStreamHeader( MetaDataTableStreamHeader x, MetaDataTableStreamHeader y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Reserved == y.Reserved
            && x.MajorVersion == y.MajorVersion
            && x.MinorVersion == y.MinorVersion
            && x.Reserved2 == y.Reserved2
            && NullableEqualityComparer<Int32>.Equals( x.ExtraData, y.ExtraData )
            );
      }

      private static Boolean Equality_DebugInfo_Logical( DebugInformation x, DebugInformation y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Timestamp == y.Timestamp
            && x.Characteristics == y.Characteristics
            && x.DebugType == y.DebugType
            && x.VersionMajor == y.VersionMajor
            && x.VersionMinor == y.VersionMinor
            && x.DebugData.ArrayQueryEquality( y.DebugData )
            );
      }

      private static Int32 HashCode_HeadersData( ImageInformation x )
      {
         return x == null ? 0 : ( ( 17 * 23 + (Int32) x.PEInformation.NTHeader.FileHeader.Machine ) * 23 + x.CLIInformation.MetaDataRoot.VersionStringBytes.ArrayQueryHashCode() );
      }

      private static Int32 Comparison_ClassLayout( ClassLayout x, ClassLayout y )
      {
         // Parent (simple index) is primary key
         return x.Parent.Index.CompareTo( y.Parent.Index );
      }

      private static Int32 Comparison_ConstantDefinition( ConstantDefinition x, ConstantDefinition y )
      {
         // Parent (coded index) is primary key
         return HasConstantComparer.Compare( x.Parent, y.Parent );
      }

      private static Int32 Comparison_CustomAttributeDefinition( CustomAttributeDefinition x, CustomAttributeDefinition y )
      {
         // Parent (coded index) is primary key
         return HasCustomAttributeComparer.Compare( x.Parent, y.Parent );
      }

      private static Int32 Comparison_SecurityDefinition( SecurityDefinition x, SecurityDefinition y )
      {
         // Parent (coded index) is primary key
         return HasDeclSecurityComparer.Compare( x.Parent, y.Parent );
      }

      private static Int32 Comparison_FieldLayout( FieldLayout x, FieldLayout y )
      {
         // Field (simple index) is primary key
         return x.Field.Index.CompareTo( y.Field.Index );
      }

      private static Int32 Comparison_FieldMarshal( FieldMarshal x, FieldMarshal y )
      {
         // Parent (coded index) is primary key
         return HasFieldMarshalComparer.Compare( x.Parent, y.Parent );
      }

      private static Int32 Comparison_FieldRVA( FieldRVA x, FieldRVA y )
      {
         // Field (simple index) is primary key
         return x.Field.Index.CompareTo( y.Field.Index );
      }

      private static Int32 Comparison_GenericParameterDefinition( GenericParameterDefinition x, GenericParameterDefinition y )
      {
         // Owner (coded index) is primary key, Sequence is secondary key
         var retVal = TypeOrMethodDefComparer.Compare( x.Owner, y.Owner );
         if ( retVal == 0 )
         {
            retVal = x.GenericParameterIndex.CompareTo( y.GenericParameterIndex );
         }
         return retVal;
      }

      private static Int32 Comparison_GenericParameterConstraintDefinition( GenericParameterConstraintDefinition x, GenericParameterConstraintDefinition y )
      {
         // Owner (simple index) is primary key
         return x.Owner.Index.CompareTo( y.Owner.Index );
      }

      private static Int32 Comparison_MethodImplementationMap( MethodImplementationMap x, MethodImplementationMap y )
      {
         // MemberForwarded (coded index) is primary key
         return MemberForwardedComparer.Compare( x.MemberForwarded, y.MemberForwarded );
      }

      private static Int32 Comparison_InterfaceImplementation( InterfaceImplementation x, InterfaceImplementation y )
      {
         // Primary key 'Class', secondary key 'Interface'
         var retVal = x.Class.Index.CompareTo( y.Class.Index );
         if ( retVal == 0 )
         {
            retVal = x.Interface.Index.CompareTo( y.Interface.Index );
         }
         return retVal;
      }

      private static Int32 Comparison_MethodImplementation( MethodImplementation x, MethodImplementation y )
      {
         // Class (simple index) is primary key
         return x.Class.Index.CompareTo( y.Class.Index );
      }

      private static Int32 Comparison_MethodSemantics( MethodSemantics x, MethodSemantics y )
      {
         // Associaton (coded index) is primary key
         return HasSemanticsComparer.Compare( x.Associaton, y.Associaton );
      }

      private static Int32 Comparison_NestedClassDefinition( NestedClassDefinition x, NestedClassDefinition y )
      {
         // Sort by 'NestedClass' table index
         return x.NestedClass.Index.CompareTo( y.NestedClass.Index );
      }
   }

   /// <summary>
   /// This class provides functionality of comparing <see cref="TableIndex"/> structs, when the <see cref="TableIndex.Table"/> property is one of the pre-defined options.
   /// </summary>
   /// <remarks>
   /// When specifying the pre-defined tables, the order is important.
   /// The <see cref="TableIndex"/> objects with same <see cref="TableIndex.Index"/> will be ordered in such way, that their <see cref="TableIndex.Table"/> will be in same order as specified in constructor for this class.
   /// </remarks>
   public sealed class CodedTableIndexComparer : IComparer<TableIndex>, IComparer
   {
      private readonly Int32[] _tableOrderArray;

      /// <summary>
      /// Creates a new instance of <see cref="CodedTableIndexComparer"/> with given pre-defined tables.
      /// </summary>
      /// <param name="possibleTables">The possible values for <see cref="TableIndex.Table"/>, or <c>null</c> if value at that index is not in use.</param>
      public CodedTableIndexComparer( IEnumerable<Tables?> possibleTables )
         : this( possibleTables.Select( t => t.HasValue ? (Int32) t.Value : (Int32?) null ) )
      {

      }

      /// <summary>
      /// Creates a new instance of <see cref="CodedTableIndexComparer"/> with given pre-defined tables, as integers.
      /// </summary>
      /// <param name="possibleTables">The possible values for <see cref="TableIndex.Table"/> as integers, or <c>null</c> if value at that index is not in use.</param>
      public CodedTableIndexComparer( IEnumerable<Int32?> possibleTables )
      {
         this._tableOrderArray = new Int32[CAMCoreInternals.AMOUNT_OF_TABLES];
         PopulateTableOrderArray( this._tableOrderArray, possibleTables );
      }

      /// <summary>
      /// Compares the two instances of <see cref="TableIndex"/> so that <see cref="TableIndex.Index"/> acts are primary key, and <see cref="TableIndex.Table"/> acts as secondary key according to the pre-defined tables given to this <see cref="CodedTableIndexComparer"/>.
      /// </summary>
      /// <param name="x">The first <see cref="TableIndex"/>.</param>
      /// <param name="y">The second <see cref="TableIndex"/>.</param>
      /// <returns>Negative value if <paramref name="x"/> is considered less than <paramref name="y"/>, <c>0</c> if <paramref name="x"/> and <paramref name="y"/> are considered equal, or positive value if <paramref name="x"/> is considered to be greater than <paramref name="y"/>.</returns>
      /// <remarks>
      /// For example, consider that this <see cref="CodedTableIndexComparer"/> was created with following tables: <see cref="Tables.Property"/> and <see cref="Tables.Event"/>, in that order.
      /// Then the table index to <c>9</c>th row of <see cref="Tables.Event"/> would be considered less than table index to <c>10</c>th row of <see cref="Tables.Property"/>, because the numerical index value is less.
      /// However, the table index to <c>9</c>th row of <see cref="Tables.Event"/> would be considered greater than table index to <c>9</c>th row of <see cref="Tables.Property"/> table, becase the numerical index values are same, but the <see cref="Tables.Property"/> was specified before <see cref="Tables.Event"/>.
      /// </remarks>
      public Int32 Compare( TableIndex x, TableIndex y )
      {
         var retVal = x.Index.CompareTo( y.Index );
         if ( retVal == 0 )
         {
            retVal = this._tableOrderArray[(Int32) x.Table].CompareTo( this._tableOrderArray[(Int32) y.Table] );
         }
         return retVal;
      }

      /// <summary>
      /// Compares two <see cref="TableIndex"/> instances as objects, taking into account <c>null</c> vales.
      /// </summary>
      /// <param name="x">The first <see cref="TableIndex"/> or <c>null</c>.</param>
      /// <param name="y">The second <see cref="TableIndex"/> or <c>null</c>.</param>
      /// <returns>
      /// If both <paramref name="x"/> and <paramref name="y"/> are of type <see cref="TableIndex"/>, the value of <see cref="Compare(TableIndex, TableIndex)"/>.
      /// Otherwise, if one of them is <c>null</c>, returns value so that <c>null</c>s are always before non-<c>null</c>s.
      /// If both are non-<c>null</c> and one or both are not <see cref="TableIndex"/>, then an exception is thrown.
      /// </returns>
      /// <exception cref="ArgumentException">If <paramref name="x"/> and <paramref name="y"/> are not <c>null</c> and their type is not <see cref="TableIndex"/>.</exception>
      Int32 IComparer.Compare( object x, object y )
      {
         Int32 retVal;
         if ( x == null )
         {
            retVal = y == null ? 0 : -1;
         }
         else if ( y == null )
         {
            retVal = 1;
         }
         else if ( x is TableIndex && y is TableIndex )
         {
            retVal = this.Compare( (TableIndex) x, (TableIndex) y );
         }
         else
         {
            throw new ArgumentException( "Given object must be of type " + typeof( TableIndex ) + " or null." );
         }
         return retVal;
      }

      private static void PopulateTableOrderArray( Int32[] array, IEnumerable<Int32?> tablesInOrder )
      {
         var i = 0;
         foreach ( var table in tablesInOrder )
         {
            if ( table.HasValue )
            {
               array[table.Value] = i;
            }
            ++i;
         }
      }
   }
}
