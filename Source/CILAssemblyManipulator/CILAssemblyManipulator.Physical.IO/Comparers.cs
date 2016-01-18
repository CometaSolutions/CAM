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


namespace CILAssemblyManipulator.Physical
{
   public static class Comparers
   {
      private static IEqualityComparer<ImageInformation> _ImageInformationLogicalEqualityComparer = null;

      private static IComparer<ClassLayout> _ClassLayoutComparer = null;
      private static IComparer<ConstantDefinition> _ConstantDefinitionComparer = null;
      private static IComparer<CustomAttributeDefinition> _CustomAttributeDefinitionComparer = null;
      private static IComparer<SecurityDefinition> _SecurityDefinitionComparer = null;
      private static IComparer<FieldLayout> _FieldLayoutComparer = null;
      private static IComparer<FieldMarshal> _FieldMarshalComparer = null;
      private static IComparer<FieldRVA> _FieldRVAComparer = null;
      private static IComparer<GenericParameterDefinition> _GenericParameterDefinitionComparer = null;
      private static IComparer<GenericParameterConstraintDefinition> _GenericParameterConstraintDefinitionComparer = null;
      private static IComparer<MethodImplementationMap> _MethodImplementationMapComparer = null;
      private static IComparer<InterfaceImplementation> _InterfaceImplementationComparer = null;
      private static IComparer<MethodImplementation> _MethodImplementationComparer = null;
      private static IComparer<MethodSemantics> _MethodSemanticsComparer = null;
      private static IComparer<NestedClassDefinition> _NestedClassDefinitionComparer = null;

      private static IComparer<TableIndex> _HasConstantComparer = null;
      private static IComparer<TableIndex> _HasCustomAttributeComparer = null;
      private static IComparer<TableIndex> _HasFieldMarshalComparer = null;
      private static IComparer<TableIndex> _HasDeclSecurityComparer = null;
      private static IComparer<TableIndex> _HasSemanticsComparer = null;
      private static IComparer<TableIndex> _MemberForwardedComparer = null;
      private static IComparer<TableIndex> _TypeOrMethodDefComparer = null;

      public static IEqualityComparer<ImageInformation> ImageInformationLogicalEqualityComparer
      {
         get
         {
            var retVal = _ImageInformationLogicalEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<ImageInformation>( Equality_ImageInformation_Logical, HashCode_HeadersData );
               _ImageInformationLogicalEqualityComparer = retVal;
            }
            return retVal;
         }
      }


      /// <summary>
      /// Get the ordering comparer for <see cref="ClassLayout"/> according to ECMA-335 serialization standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="ClassLayout"/> according to ECMA-335 serialization standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="TableIndex.Index"/> property of <see cref="ClassLayout.Parent"/> to perform comparison.
      /// </remarks>
      public static IComparer<ClassLayout> ClassLayoutComparer
      {
         get
         {
            var retVal = _ClassLayoutComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<ClassLayout>( Comparison_ClassLayout );
               _ClassLayoutComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the ordering comparer for <see cref="ConstantDefinition"/> according to ECMA-335 serialization standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="ConstantDefinition"/> according to ECMA-335 serialization standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="HasConstantComparer"/> for <see cref="ConstantDefinition.Parent"/> property to perform comparison.
      /// </remarks>
      public static IComparer<ConstantDefinition> ConstantDefinitionComparer
      {
         get
         {
            var retVal = _ConstantDefinitionComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<ConstantDefinition>( Comparison_ConstantDefinition );
               _ConstantDefinitionComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the ordering comparer for <see cref="CustomAttributeDefinition"/> according to ECMA-335 serialization standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="CustomAttributeDefinition"/> according to ECMA-335 serialization standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="HasCustomAttributeComparer"/> for <see cref="CustomAttributeDefinition.Parent"/> property to perform comparison.
      /// </remarks>
      public static IComparer<CustomAttributeDefinition> CustomAttributeDefinitionComparer
      {
         get
         {
            var retVal = _CustomAttributeDefinitionComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<CustomAttributeDefinition>( Comparison_CustomAttributeDefinition );
               _CustomAttributeDefinitionComparer = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Get the ordering comparer for <see cref="CustomAttributeDefinition"/> according to ECMA-335 serialization standard.
      /// </summary>
      /// <value>The ordering comparer to for <see cref="CustomAttributeDefinition"/> according to ECMA-335 serialization standard.</value>
      /// <remarks>
      /// This comparer will use the <see cref="HasCustomAttributeComparer"/> for <see cref="CustomAttributeDefinition.Parent"/> property to perform comparison.
      /// </remarks>
      public static IComparer<SecurityDefinition> SecurityDefinitionComparer
      {
         get
         {
            var retVal = _SecurityDefinitionComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<SecurityDefinition>( Comparison_SecurityDefinition );
               _SecurityDefinitionComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<FieldLayout> FieldLayoutComparer
      {
         get
         {
            var retVal = _FieldLayoutComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<FieldLayout>( Comparison_FieldLayout );
               _FieldLayoutComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<FieldMarshal> FieldMarshalComparer
      {
         get
         {
            var retVal = _FieldMarshalComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<FieldMarshal>( Comparison_FieldMarshal );
               _FieldMarshalComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<FieldRVA> FieldRVAComparer
      {
         get
         {
            var retVal = _FieldRVAComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<FieldRVA>( Comparison_FieldRVA );
               _FieldRVAComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<GenericParameterDefinition> GenericParameterDefinitionComparer
      {
         get
         {
            var retVal = _GenericParameterDefinitionComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<GenericParameterDefinition>( Comparison_GenericParameterDefinition );
               _GenericParameterDefinitionComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<GenericParameterConstraintDefinition> GenericParameterConstraintDefinitionComparer
      {
         get
         {
            var retVal = _GenericParameterConstraintDefinitionComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<GenericParameterConstraintDefinition>( Comparison_GenericParameterConstraintDefinition );
               _GenericParameterConstraintDefinitionComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<MethodImplementationMap> MethodImplementationMapComparer
      {
         get
         {
            var retVal = _MethodImplementationMapComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<MethodImplementationMap>( Comparison_MethodImplementationMap );
               _MethodImplementationMapComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<InterfaceImplementation> InterfaceImplementationComparer
      {
         get
         {
            var retVal = _InterfaceImplementationComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<InterfaceImplementation>( Comparison_InterfaceImplementation );
               _InterfaceImplementationComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<MethodImplementation> MethodImplementationComparer
      {
         get
         {
            var retVal = _MethodImplementationComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<MethodImplementation>( Comparison_MethodImplementation );
               _MethodImplementationComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<MethodSemantics> MethodSemanticsComparer
      {
         get
         {
            var retVal = _MethodSemanticsComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<MethodSemantics>( Comparison_MethodSemantics );
               _MethodSemanticsComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<NestedClassDefinition> NestedClassDefinitionComparer
      {
         get
         {
            var retVal = _NestedClassDefinitionComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewComparer<NestedClassDefinition>( Comparison_NestedClassDefinition );
               _NestedClassDefinitionComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<TableIndex> HasConstantComparer
      {
         get
         {
            var retVal = _HasConstantComparer;
            if ( retVal == null )
            {
               var tableOrderArray = CreateTableOrderArray( Meta.DefaultMetaDataTableInformationProvider.HasConstant );
               retVal = ComparerFromFunctions.NewComparer<TableIndex>( ( x, y ) => CompareWithTableOrderArray( x, y, tableOrderArray ) );
               _HasConstantComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<TableIndex> HasCustomAttributeComparer
      {
         get
         {
            var retVal = _HasCustomAttributeComparer;
            if ( retVal == null )
            {
               var tableOrderArray = CreateTableOrderArray( Meta.DefaultMetaDataTableInformationProvider.HasCustomAttribute );
               retVal = ComparerFromFunctions.NewComparer<TableIndex>( ( x, y ) => CompareWithTableOrderArray( x, y, tableOrderArray ) );
               _HasCustomAttributeComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<TableIndex> HasFieldMarshalComparer
      {
         get
         {
            var retVal = _HasFieldMarshalComparer;
            if ( retVal == null )
            {
               var tableOrderArray = CreateTableOrderArray( Meta.DefaultMetaDataTableInformationProvider.HasFieldMarshal );
               retVal = ComparerFromFunctions.NewComparer<TableIndex>( ( x, y ) => CompareWithTableOrderArray( x, y, tableOrderArray ) );
               _HasFieldMarshalComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<TableIndex> HasDeclSecurityComparer
      {
         get
         {
            var retVal = _HasDeclSecurityComparer;
            if ( retVal == null )
            {
               var tableOrderArray = CreateTableOrderArray( Meta.DefaultMetaDataTableInformationProvider.HasSecurity );
               retVal = ComparerFromFunctions.NewComparer<TableIndex>( ( x, y ) => CompareWithTableOrderArray( x, y, tableOrderArray ) );
               _HasDeclSecurityComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<TableIndex> HasSemanticsComparer
      {
         get
         {
            var retVal = _HasSemanticsComparer;
            if ( retVal == null )
            {
               var tableOrderArray = CreateTableOrderArray( Meta.DefaultMetaDataTableInformationProvider.HasSemantics );
               retVal = ComparerFromFunctions.NewComparer<TableIndex>( ( x, y ) => CompareWithTableOrderArray( x, y, tableOrderArray ) );
               _HasSemanticsComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<TableIndex> MemberForwardedComparer
      {
         get
         {
            var retVal = _MemberForwardedComparer;
            if ( retVal == null )
            {
               var tableOrderArray = CreateTableOrderArray( Meta.DefaultMetaDataTableInformationProvider.MemberForwarded );
               retVal = ComparerFromFunctions.NewComparer<TableIndex>( ( x, y ) => CompareWithTableOrderArray( x, y, tableOrderArray ) );
               _MemberForwardedComparer = retVal;
            }
            return retVal;
         }
      }

      public static IComparer<TableIndex> TypeOrMethodDefComparer
      {
         get
         {
            var retVal = _TypeOrMethodDefComparer;
            if ( retVal == null )
            {
               var tableOrderArray = CreateTableOrderArray( Meta.DefaultMetaDataTableInformationProvider.TypeOrMethodDef );
               retVal = ComparerFromFunctions.NewComparer<TableIndex>( ( x, y ) => CompareWithTableOrderArray( x, y, tableOrderArray ) );
               _TypeOrMethodDefComparer = retVal;
            }
            return retVal;
         }
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

      private static Int32[] CreateTableOrderArray( ArrayQuery<Int32?> tablesInOrder )
      {
         var retVal = new Int32[CAMCoreInternals.AMOUNT_OF_TABLES];
         for ( var i = 0; i < tablesInOrder.Count; ++i )
         {
            var cur = tablesInOrder[i];
            if ( cur.HasValue )
            {
               retVal[cur.Value] = i;
            }
         }
         return retVal;
      }

      private static Int32 CompareWithTableOrderArray( TableIndex x, TableIndex y, Int32[] tableOrderArray )
      {
         var retVal = x.Index.CompareTo( y.Index );
         if ( retVal == 0 )
         {
            retVal = tableOrderArray[(Int32) x.Table].CompareTo( tableOrderArray[(Int32) y.Table] );
         }
         return retVal;
      }
   }
}
