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
      /// Get the ordering comparer for <see cref="ClassLayout"/> according to ECMA-335 serialization standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="ClassLayout"/> according to ECMA-335 serialization standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="TableIndex.Index"/> property of <see cref="ClassLayout.Parent"/> to perform comparison.
      /// </remarks>
      public static IComparer<ClassLayout> ClassLayoutComparer { get; }

      /// <summary>
      /// Get the ordering comparer for <see cref="ConstantDefinition"/> according to ECMA-335 serialization standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="ConstantDefinition"/> according to ECMA-335 serialization standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="HasConstantComparer"/> for <see cref="ConstantDefinition.Parent"/> property to perform comparison.
      /// </remarks>
      public static IComparer<ConstantDefinition> ConstantDefinitionComparer { get; }

      /// <summary>
      /// Get the ordering comparer for <see cref="CustomAttributeDefinition"/> according to ECMA-335 serialization standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="CustomAttributeDefinition"/> according to ECMA-335 serialization standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="HasCustomAttributeComparer"/> for <see cref="CustomAttributeDefinition.Parent"/> property to perform comparison.
      /// </remarks>
      public static IComparer<CustomAttributeDefinition> CustomAttributeDefinitionComparer { get; }

      /// <summary>
      /// Get the ordering comparer for <see cref="CustomAttributeDefinition"/> according to ECMA-335 serialization standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="CustomAttributeDefinition"/> according to ECMA-335 serialization standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="HasCustomAttributeComparer"/> for <see cref="CustomAttributeDefinition.Parent"/> property to perform comparison.
      /// </remarks>
      public static IComparer<SecurityDefinition> SecurityDefinitionComparer { get; }
      public static IComparer<FieldLayout> FieldLayoutComparer { get; }
      public static IComparer<FieldMarshal> FieldMarshalComparer { get; }
      public static IComparer<FieldRVA> FieldRVAComparer { get; }
      public static IComparer<GenericParameterDefinition> GenericParameterDefinitionComparer { get; }
      public static IComparer<GenericParameterConstraintDefinition> GenericParameterConstraintDefinitionComparer { get; }
      public static IComparer<MethodImplementationMap> MethodImplementationMapComparer { get; }
      public static IComparer<InterfaceImplementation> InterfaceImplementationComparer { get; }
      public static IComparer<MethodImplementation> MethodImplementationComparer { get; }
      public static IComparer<MethodSemantics> MethodSemanticsComparer { get; }
      public static IComparer<NestedClassDefinition> NestedClassDefinitionComparer { get; }

      public static CodedTableIndexComparer HasConstantComparer { get; }
      public static CodedTableIndexComparer HasCustomAttributeComparer { get; }
      public static CodedTableIndexComparer HasFieldMarshalComparer { get; }
      public static CodedTableIndexComparer HasDeclSecurityComparer { get; }
      public static CodedTableIndexComparer HasSemanticsComparer { get; }
      public static CodedTableIndexComparer MemberForwardedComparer { get; }
      public static CodedTableIndexComparer TypeOrMethodDefComparer { get; }

      static Comparers()
      {
         ImageInformationLogicalEqualityComparer = ComparerFromFunctions.NewEqualityComparer<ImageInformation>( Equality_ImageInformation_Logical, HashCode_HeadersData );

         ClassLayoutComparer = ComparerFromFunctions.NewComparer<ClassLayout>( Comparison_ClassLayout );
         ConstantDefinitionComparer = ComparerFromFunctions.NewComparer<ConstantDefinition>( Comparison_ConstantDefinition );
         CustomAttributeDefinitionComparer = ComparerFromFunctions.NewComparer<CustomAttributeDefinition>( Comparison_CustomAttributeDefinition );
         SecurityDefinitionComparer = ComparerFromFunctions.NewComparer<SecurityDefinition>( Comparison_SecurityDefinition );
         FieldLayoutComparer = ComparerFromFunctions.NewComparer<FieldLayout>( Comparison_FieldLayout );
         FieldMarshalComparer = ComparerFromFunctions.NewComparer<FieldMarshal>( Comparison_FieldMarshal );
         FieldRVAComparer = ComparerFromFunctions.NewComparer<FieldRVA>( Comparison_FieldRVA );
         GenericParameterDefinitionComparer = ComparerFromFunctions.NewComparer<GenericParameterDefinition>( Comparison_GenericParameterDefinition );
         GenericParameterConstraintDefinitionComparer = ComparerFromFunctions.NewComparer<GenericParameterConstraintDefinition>( Comparison_GenericParameterConstraintDefinition );
         MethodImplementationMapComparer = ComparerFromFunctions.NewComparer<MethodImplementationMap>( Comparison_MethodImplementationMap );
         InterfaceImplementationComparer = ComparerFromFunctions.NewComparer<InterfaceImplementation>( Comparison_InterfaceImplementation );
         MethodImplementationComparer = ComparerFromFunctions.NewComparer<MethodImplementation>( Comparison_MethodImplementation );
         MethodSemanticsComparer = ComparerFromFunctions.NewComparer<MethodSemantics>( Comparison_MethodSemantics );
         NestedClassDefinitionComparer = ComparerFromFunctions.NewComparer<NestedClassDefinition>( Comparison_NestedClassDefinition );

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
            && x.FieldRVAs.Count == y.FieldRVAs.Count
            && x.MethodRVAs.Count == y.MethodRVAs.Count
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

   public sealed class CodedTableIndexComparer : IComparer<TableIndex>, System.Collections.IComparer
   {
      private readonly Int32[] _tableOrderArray;

      public CodedTableIndexComparer( IEnumerable<Int32?> possibleTables )
      {
         this._tableOrderArray = new Int32[CAMCoreInternals.AMOUNT_OF_TABLES];
         PopulateTableOrderArray( this._tableOrderArray, possibleTables );
      }

      public Int32 Compare( TableIndex x, TableIndex y )
      {
         var retVal = x.Index.CompareTo( y.Index );
         if ( retVal == 0 )
         {
            retVal = this._tableOrderArray[(Int32) x.Table].CompareTo( this._tableOrderArray[(Int32) y.Table] );
         }
         return retVal;
      }

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
