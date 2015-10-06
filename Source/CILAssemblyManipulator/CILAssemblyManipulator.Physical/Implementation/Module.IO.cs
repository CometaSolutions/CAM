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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.Implementation
{
   using System.IO;
   using TRVA = System.Int32;

   internal static class MetaDataConstants
   {

      internal static readonly Encoding SYS_STRING_ENCODING = new UTF8Encoding( false, false );
      internal static readonly Encoding USER_STRING_ENCODING = new UnicodeEncoding( false, false, false );

      private const UInt32 TAGMASK_MASK = 0xFFFFFFFF;

      internal const Int32 DEBUG_DD_SIZE = 28;

      internal const String TABLE_STREAM_NAME = "#~";
      internal const String SYS_STRING_STREAM_NAME = "#Strings";
      internal const String USER_STRING_STREAM_NAME = "#US";
      internal const String GUID_STREAM_NAME = "#GUID";
      internal const String BLOB_STREAM_NAME = "#Blob";

      internal const Int32 TWO_BYTE_SIZE = 2;
      internal const Int32 FOUR_BYTE_SIZE = 4;
      internal const Int32 GUID_SIZE = 16;

      internal const String PERMISSION_SET = "System.Security.Permissions.PermissionSetAttribute";
      internal const String PERMISSION_SET_XML_PROP = "XML";

      internal const Byte DECL_SECURITY_HEADER = 0x2E; // '.'

      // ECMA-335, pp. 274-276
      private static readonly Tables?[] TYPE_DEF_OR_REF_ARRAY = new Tables?[] { Tables.TypeDef, Tables.TypeRef, Tables.TypeSpec };
      private static readonly Tables?[] HAS_CONSTANT_ARRAY = new Tables?[] { Tables.Field, Tables.Parameter, Tables.Property };
      private static readonly Tables?[] HAS_CUSTOM_ATTRIBUTE_ARRAY = new Tables?[] { Tables.MethodDef, Tables.Field, Tables.TypeRef, Tables.TypeDef, Tables.Parameter,
            Tables.InterfaceImpl, Tables.MemberRef, Tables.Module, Tables.DeclSecurity, Tables.Property, Tables.Event,
            Tables.StandaloneSignature, Tables.ModuleRef, Tables.TypeSpec, Tables.Assembly, Tables.AssemblyRef, Tables.File,
            Tables.ExportedType, Tables.ManifestResource, Tables.GenericParameter, Tables.GenericParameterConstraint, Tables.MethodSpec };
      private static readonly Tables?[] HAS_FIELD_MARSHAL_ARRAY = new Tables?[] { Tables.Field, Tables.Parameter };
      private static readonly Tables?[] HAS_DECL_SECURITY_ARRAY = new Tables?[] { Tables.TypeDef, Tables.MethodDef, Tables.Assembly };
      private static readonly Tables?[] MEMBER_REF_PARENT_ARRAY = new Tables?[] { Tables.TypeDef, Tables.TypeRef, Tables.ModuleRef, Tables.MethodDef, Tables.TypeSpec };
      private static readonly Tables?[] HAS_SEMANTICS_ARRAY = new Tables?[] { Tables.Event, Tables.Property };
      private static readonly Tables?[] METHOD_DEF_OR_REF_ARRAY = new Tables?[] { Tables.MethodDef, Tables.MemberRef };
      private static readonly Tables?[] MEMBER_FORWARDED_ARRAY = new Tables?[] { Tables.Field, Tables.MethodDef };
      private static readonly Tables?[] IMPLEMENTATION_ARRAY = new Tables?[] { Tables.File, Tables.AssemblyRef, Tables.ExportedType };
      private static readonly Tables?[] CUSTOM_ATTRIBUTE_TYPE_ARRAY = new Tables?[] { null, null, Tables.MethodDef, Tables.MemberRef, null };
      private static readonly Tables?[] RESOLUTION_SCOPE_ARRAY = new Tables?[] { Tables.Module, Tables.ModuleRef, Tables.AssemblyRef, Tables.TypeRef };
      private static readonly Tables?[] TYPE_OR_METHOD_DEF_ARRAY = new Tables?[] { Tables.TypeDef, Tables.MethodDef };
      internal static readonly Tuple<Tables?[], UInt32, Int32> TYPE_DEF_OR_REF = Tuple.Create( TYPE_DEF_OR_REF_ARRAY, GetTagBitMask( TYPE_DEF_OR_REF_ARRAY ), GetTagBitSize( TYPE_DEF_OR_REF_ARRAY ) );
      internal static readonly Tuple<Tables?[], UInt32, Int32> HAS_CONSTANT = Tuple.Create( HAS_CONSTANT_ARRAY, GetTagBitMask( HAS_CONSTANT_ARRAY ), GetTagBitSize( HAS_CONSTANT_ARRAY ) );
      internal static readonly Tuple<Tables?[], UInt32, Int32> HAS_CUSTOM_ATTRIBUTE = Tuple.Create( HAS_CUSTOM_ATTRIBUTE_ARRAY, GetTagBitMask( HAS_CUSTOM_ATTRIBUTE_ARRAY ), GetTagBitSize( HAS_CUSTOM_ATTRIBUTE_ARRAY ) );
      internal static readonly Tuple<Tables?[], UInt32, Int32> HAS_FIELD_MARSHAL = Tuple.Create( HAS_FIELD_MARSHAL_ARRAY, GetTagBitMask( HAS_FIELD_MARSHAL_ARRAY ), GetTagBitSize( HAS_FIELD_MARSHAL_ARRAY ) );
      internal static readonly Tuple<Tables?[], UInt32, Int32> HAS_DECL_SECURITY = Tuple.Create( HAS_DECL_SECURITY_ARRAY, GetTagBitMask( HAS_DECL_SECURITY_ARRAY ), GetTagBitSize( HAS_DECL_SECURITY_ARRAY ) );
      internal static readonly Tuple<Tables?[], UInt32, Int32> MEMBER_REF_PARENT = Tuple.Create( MEMBER_REF_PARENT_ARRAY, GetTagBitMask( MEMBER_REF_PARENT_ARRAY ), GetTagBitSize( MEMBER_REF_PARENT_ARRAY ) );
      internal static readonly Tuple<Tables?[], UInt32, Int32> HAS_SEMANTICS = Tuple.Create( HAS_SEMANTICS_ARRAY, GetTagBitMask( HAS_SEMANTICS_ARRAY ), GetTagBitSize( HAS_SEMANTICS_ARRAY ) );
      internal static readonly Tuple<Tables?[], UInt32, Int32> METHOD_DEF_OR_REF = Tuple.Create( METHOD_DEF_OR_REF_ARRAY, GetTagBitMask( METHOD_DEF_OR_REF_ARRAY ), GetTagBitSize( METHOD_DEF_OR_REF_ARRAY ) );
      internal static readonly Tuple<Tables?[], UInt32, Int32> MEMBER_FORWARDED = Tuple.Create( MEMBER_FORWARDED_ARRAY, GetTagBitMask( MEMBER_FORWARDED_ARRAY ), GetTagBitSize( MEMBER_FORWARDED_ARRAY ) );
      internal static readonly Tuple<Tables?[], UInt32, Int32> IMPLEMENTATION = Tuple.Create( IMPLEMENTATION_ARRAY, GetTagBitMask( IMPLEMENTATION_ARRAY ), GetTagBitSize( IMPLEMENTATION_ARRAY ) );
      internal static readonly Tuple<Tables?[], UInt32, Int32> CUSTOM_ATTRIBUTE_TYPE = Tuple.Create( CUSTOM_ATTRIBUTE_TYPE_ARRAY, GetTagBitMask( CUSTOM_ATTRIBUTE_TYPE_ARRAY ), GetTagBitSize( CUSTOM_ATTRIBUTE_TYPE_ARRAY ) );
      internal static readonly Tuple<Tables?[], UInt32, Int32> RESOLUTION_SCOPE = Tuple.Create( RESOLUTION_SCOPE_ARRAY, GetTagBitMask( RESOLUTION_SCOPE_ARRAY ), GetTagBitSize( RESOLUTION_SCOPE_ARRAY ) );
      internal static readonly Tuple<Tables?[], UInt32, Int32> TYPE_OR_METHOD_DEF = Tuple.Create( TYPE_OR_METHOD_DEF_ARRAY, GetTagBitMask( TYPE_OR_METHOD_DEF_ARRAY ), GetTagBitSize( TYPE_OR_METHOD_DEF_ARRAY ) );

      private static readonly IDictionary<Tables, Func<Int32[], Int32, Int32, Int32, Int32>> TABLE_WIDTH_CALCULATOR;
      static MetaDataConstants()
      {
         TABLE_WIDTH_CALCULATOR = new Dictionary<Tables, Func<Int32[], Int32, Int32, Int32, Int32>>( Consts.AMOUNT_OF_TABLES )
         {
            { Tables.Module, ( tableSizes, strWidth, guidWidth, blobWidth ) => TWO_BYTE_SIZE + strWidth + guidWidth + guidWidth + guidWidth },
            { Tables.TypeRef, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeCoded( tableSizes, RESOLUTION_SCOPE ) + strWidth + strWidth },
            { Tables.TypeDef, ( tableSizes, strWidth, guidWidth, blobWidth ) => FOUR_BYTE_SIZE + strWidth + strWidth + GetTableIndexSizeCoded( tableSizes, TYPE_DEF_OR_REF ) + GetTableIndexSizeSimple( tableSizes, Tables.Field ) + GetTableIndexSizeSimple( tableSizes, Tables.MethodDef ) },
            { Tables.FieldPtr, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeSimple( tableSizes, Tables.Field ) },
            { Tables.Field, ( tableSizes, strWidth, guidWidth, blobWidth ) => TWO_BYTE_SIZE + strWidth + blobWidth },
            { Tables.MethodPtr, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeSimple( tableSizes, Tables.MethodDef ) },
            { Tables.MethodDef, ( tableSizes, strWidth, guidWidth, blobWidth ) => sizeof( TRVA ) + TWO_BYTE_SIZE + TWO_BYTE_SIZE + strWidth + blobWidth + GetTableIndexSizeSimple( tableSizes, Tables.Parameter ) },
            { Tables.ParameterPtr, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeSimple( tableSizes, Tables.Parameter ) },
            { Tables.Parameter, ( tableSizes, strWidth, guidWidth, blobWidth ) => TWO_BYTE_SIZE + TWO_BYTE_SIZE + strWidth },
            { Tables.InterfaceImpl, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeSimple( tableSizes, Tables.TypeDef ) + GetTableIndexSizeCoded( tableSizes, TYPE_DEF_OR_REF ) },
            { Tables.MemberRef, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeCoded( tableSizes, MEMBER_REF_PARENT ) + strWidth + blobWidth },
            { Tables.Constant, ( tableSizes, strWidth, guidWidth, blobWidth ) => TWO_BYTE_SIZE + GetTableIndexSizeCoded( tableSizes, HAS_CONSTANT ) + blobWidth },
            { Tables.CustomAttribute, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeCoded( tableSizes, HAS_CUSTOM_ATTRIBUTE ) + GetTableIndexSizeCoded( tableSizes, CUSTOM_ATTRIBUTE_TYPE ) + blobWidth },
            { Tables.FieldMarshal, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeCoded( tableSizes, HAS_FIELD_MARSHAL ) + blobWidth },
            { Tables.DeclSecurity, ( tableSizes, strWidth, guidWidth, blobWidth ) => TWO_BYTE_SIZE + GetTableIndexSizeCoded( tableSizes, HAS_DECL_SECURITY ) + blobWidth },
            { Tables.ClassLayout, ( tableSizes, strWidth, guidWidth, blobWidth ) => TWO_BYTE_SIZE + FOUR_BYTE_SIZE + GetTableIndexSizeSimple( tableSizes, Tables.TypeDef ) },
            { Tables.FieldLayout, ( tableSizes, strWidth, guidWidth, blobWidth ) => FOUR_BYTE_SIZE + GetTableIndexSizeSimple( tableSizes, Tables.Field ) },
            { Tables.StandaloneSignature, ( tableSizes, strWidth, guidWidth, blobWidth ) => blobWidth },
            { Tables.EventMap, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeSimple( tableSizes, Tables.TypeDef ) + GetTableIndexSizeSimple( tableSizes, Tables.Event ) },
            { Tables.EventPtr, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeSimple( tableSizes, Tables.Event ) },
            { Tables.Event, ( tableSizes, strWidth, guidWidth, blobWidth ) => TWO_BYTE_SIZE + strWidth + GetTableIndexSizeCoded( tableSizes, TYPE_DEF_OR_REF ) },
            { Tables.PropertyMap, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeSimple( tableSizes, Tables.TypeDef ) + GetTableIndexSizeSimple( tableSizes, Tables.Property ) },
            { Tables.PropertyPtr, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeSimple( tableSizes, Tables.Property ) },
            { Tables.Property, ( tableSizes, strWidth, guidWidth, blobWidth ) => TWO_BYTE_SIZE + strWidth + blobWidth },
            { Tables.MethodSemantics, ( tableSizes, strWidth, guidWidth, blobWidth ) => TWO_BYTE_SIZE + GetTableIndexSizeSimple( tableSizes, Tables.MethodDef ) + GetTableIndexSizeCoded( tableSizes, HAS_SEMANTICS ) },
            { Tables.MethodImpl, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeSimple( tableSizes, Tables.TypeDef ) + GetTableIndexSizeCoded( tableSizes, METHOD_DEF_OR_REF ) + GetTableIndexSizeCoded( tableSizes, METHOD_DEF_OR_REF ) },
            { Tables.ModuleRef, ( tableSizes, strWidth, guidWidth, blobWidth ) => strWidth },
            { Tables.TypeSpec, ( tableSizes, strWidth, guidWidth, blobWidth ) => blobWidth },
            { Tables.ImplMap, ( tableSizes, strWidth, guidWidth, blobWidth ) => TWO_BYTE_SIZE + GetTableIndexSizeCoded( tableSizes, MEMBER_FORWARDED ) + strWidth + GetTableIndexSizeSimple( tableSizes, Tables.ModuleRef ) },
            { Tables.FieldRVA, ( tableSizes, strWidth, guidWidth, blobWidth ) => sizeof( TRVA ) + GetTableIndexSizeSimple( tableSizes, Tables.Field ) },
            { Tables.EncLog, ( tableSizes, strWidth, guidWidth, blobWidth ) => FOUR_BYTE_SIZE * 2 },
            { Tables.EncMap, ( tableSizes, strWidth, guidWidth, blobWidth ) => FOUR_BYTE_SIZE },
            { Tables.Assembly, ( tableSizes, strWidth, guidWidth, blobWidth ) => FOUR_BYTE_SIZE + TWO_BYTE_SIZE * 4 + FOUR_BYTE_SIZE + blobWidth + strWidth + strWidth },
            { Tables.AssemblyProcessor, ( tableSizes, strWidth, guidWidth, blobWidth ) => FOUR_BYTE_SIZE },
            { Tables.AssemblyOS, ( tableSizes, strWidth, guidWidth, blobWidth ) => FOUR_BYTE_SIZE * 3 },
            { Tables.AssemblyRef, ( tableSizes, strWidth, guidWidth, blobWidth ) => TWO_BYTE_SIZE * 4 + FOUR_BYTE_SIZE + blobWidth + strWidth + strWidth + blobWidth },
            { Tables.AssemblyRefProcessor, ( tableSizes, strWidth, guidWidth, blobWidth ) => FOUR_BYTE_SIZE + GetTableIndexSizeSimple( tableSizes, Tables.AssemblyRef ) },
            { Tables.AssemblyRefOS, ( tableSizes, strWidth, guidWidth, blobWidth ) => FOUR_BYTE_SIZE * 3 + GetTableIndexSizeSimple( tableSizes, Tables.AssemblyRef ) },
            { Tables.File, ( tableSizes, strWidth, guidWidth, blobWidth ) => FOUR_BYTE_SIZE + strWidth + blobWidth },
            { Tables.ExportedType, ( tableSizes, strWidth, guidWidth, blobWidth ) => FOUR_BYTE_SIZE + FOUR_BYTE_SIZE + strWidth + strWidth + GetTableIndexSizeCoded( tableSizes, IMPLEMENTATION ) },
            { Tables.ManifestResource, ( tableSizes, strWidth, guidWidth, blobWidth ) => FOUR_BYTE_SIZE + FOUR_BYTE_SIZE + strWidth + GetTableIndexSizeCoded( tableSizes, IMPLEMENTATION ) },
            { Tables.NestedClass, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeSimple( tableSizes, Tables.TypeDef ) + GetTableIndexSizeSimple( tableSizes, Tables.TypeDef ) },
            { Tables.GenericParameter, ( tableSizes, strWidth, guidWidth, blobWidth ) => TWO_BYTE_SIZE + TWO_BYTE_SIZE + GetTableIndexSizeCoded( tableSizes, TYPE_OR_METHOD_DEF ) + strWidth },
            { Tables.MethodSpec, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeCoded( tableSizes, METHOD_DEF_OR_REF ) + blobWidth },
            { Tables.GenericParameterConstraint, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeSimple( tableSizes, Tables.GenericParameter ) + GetTableIndexSizeCoded( tableSizes, TYPE_DEF_OR_REF ) },
         };
      }

      private static Int32 GetTableIndexSizeSimple( Int32[] tableSizes, Tables referencedTable )
      {
         return tableSizes[(Int32) referencedTable] > UInt16.MaxValue ? 4 : 2;
      }

      private static Int32 GetTableIndexSizeCoded( Int32[] tableSizes, Tuple<Tables?[], UInt32, Int32> referencedTables )
      {
         Int32 max = 0;
         var array = referencedTables.Item1;
         for ( var i = 0; i < array.Length; ++i )
         {
            if ( array[i].HasValue )
            {
               max = Math.Max( max, tableSizes[(Int32) array[i].Value] );
            }
         }
         return max < ( UInt16.MaxValue >> referencedTables.Item3 ) ? 2 : 4;
      }

      private static Int32 GetTagBitSize( Tables?[] tables )
      {
         // If this would be executed really frequently, one could use lookup-table method from
         // http://graphics.stanford.edu/~seander/bithacks.html#IntegerLogLookup 
         return Convert.ToInt32( Math.Ceiling( Math.Log( tables.Length, 2.0d ) ) );
      }

      private static UInt32 GetTagBitMask( Tables?[] tables )
      {
         return ( TAGMASK_MASK >> ( 32 - GetTagBitSize( tables ) ) );
      }

      internal static IDictionary<CodedTableIndexKind, Boolean> GetCodedTableIndexSizes( Int32[] tableSizes )
      {
         var tRefWidths = new Dictionary<CodedTableIndexKind, Boolean>();
         tRefWidths.Add( CodedTableIndexKind.TypeDefOrRef, MetaDataConstants.GetTableIndexSizeCoded( tableSizes, TYPE_DEF_OR_REF ) == sizeof( Int32 ) );
         tRefWidths.Add( CodedTableIndexKind.HasConstant, MetaDataConstants.GetTableIndexSizeCoded( tableSizes, HAS_CONSTANT ) == sizeof( Int32 ) );
         tRefWidths.Add( CodedTableIndexKind.HasCustomAttribute, MetaDataConstants.GetTableIndexSizeCoded( tableSizes, HAS_CUSTOM_ATTRIBUTE ) == sizeof( Int32 ) );
         tRefWidths.Add( CodedTableIndexKind.HasFieldMarshal, MetaDataConstants.GetTableIndexSizeCoded( tableSizes, HAS_FIELD_MARSHAL ) == sizeof( Int32 ) );
         tRefWidths.Add( CodedTableIndexKind.HasDeclSecurity, MetaDataConstants.GetTableIndexSizeCoded( tableSizes, HAS_DECL_SECURITY ) == sizeof( Int32 ) );
         tRefWidths.Add( CodedTableIndexKind.MemberRefParent, MetaDataConstants.GetTableIndexSizeCoded( tableSizes, MEMBER_REF_PARENT ) == sizeof( Int32 ) );
         tRefWidths.Add( CodedTableIndexKind.HasSemantics, MetaDataConstants.GetTableIndexSizeCoded( tableSizes, HAS_SEMANTICS ) == sizeof( Int32 ) );
         tRefWidths.Add( CodedTableIndexKind.MethodDefOrRef, MetaDataConstants.GetTableIndexSizeCoded( tableSizes, METHOD_DEF_OR_REF ) == sizeof( Int32 ) );
         tRefWidths.Add( CodedTableIndexKind.MemberForwarded, MetaDataConstants.GetTableIndexSizeCoded( tableSizes, MEMBER_FORWARDED ) == sizeof( Int32 ) );
         tRefWidths.Add( CodedTableIndexKind.Implementation, MetaDataConstants.GetTableIndexSizeCoded( tableSizes, IMPLEMENTATION ) == sizeof( Int32 ) );
         tRefWidths.Add( CodedTableIndexKind.CustomAttributeType, MetaDataConstants.GetTableIndexSizeCoded( tableSizes, CUSTOM_ATTRIBUTE_TYPE ) == sizeof( Int32 ) );
         tRefWidths.Add( CodedTableIndexKind.ResolutionScope, MetaDataConstants.GetTableIndexSizeCoded( tableSizes, RESOLUTION_SCOPE ) == sizeof( Int32 ) );
         tRefWidths.Add( CodedTableIndexKind.TypeOrMethodDef, MetaDataConstants.GetTableIndexSizeCoded( tableSizes, TYPE_OR_METHOD_DEF ) == sizeof( Int32 ) );
         return tRefWidths;
      }

      internal static Int32 CalculateTableWidth( Tables table, Int32[] tableSizes, Boolean sysStringIsWide, Boolean guidIsWide, Boolean blobIsWide )
      {
         return MetaDataConstants.TABLE_WIDTH_CALCULATOR[table](
            tableSizes,
            sysStringIsWide ? FOUR_BYTE_SIZE : TWO_BYTE_SIZE,
            guidIsWide ? FOUR_BYTE_SIZE : TWO_BYTE_SIZE,
            blobIsWide ? FOUR_BYTE_SIZE : TWO_BYTE_SIZE
            );
      }

      internal static Int32 GetCodedTableIndex( CodedTableIndexKind indexKind, TableIndex? tIdx )
      {
         Int32 retVal;
         if ( tIdx.HasValue )
         {
            var tIdxValue = tIdx.Value;
            var possibleTables = GetTablesForCodedIndex( indexKind );
            retVal = ( ( tIdxValue.Index + 1 ) << possibleTables.Item3 ) | Array.IndexOf( possibleTables.Item1, tIdxValue.Table );
         }
         else
         {
            retVal = 0;
         }

         return retVal;
      }

      internal static TableIndex? ReadCodedTableIndex( System.IO.Stream stream, CodedTableIndexKind indexKind, IDictionary<CodedTableIndexKind, Boolean> tRefSizes, Byte[] tmpArray )
      {
         var idx = tRefSizes[indexKind] ? stream.ReadU32( tmpArray ) : stream.ReadU16( tmpArray );

         TableIndex? retVal;
         if ( idx > 0 )
         {
            var possibleTables = GetTablesForCodedIndex( indexKind );
            var rowIdx = ( idx >> possibleTables.Item3 );
            var tableIndex = possibleTables.Item2 & idx;
            if ( rowIdx > 0 && tableIndex < possibleTables.Item1.Length )
            {
               var tableNullable = possibleTables.Item1[tableIndex];
               if ( tableNullable.HasValue )
               {
                  retVal = new TableIndex( tableNullable.Value, (Int32) rowIdx - 1 );
               }
               else
               {
                  retVal = null;
               }
            }
            else
            {
               retVal = null;
            }
         }
         else
         {
            retVal = null;
         }

         return retVal;
      }


      // Zero-based
      internal static TableIndex ReadSimpleTableIndex( System.IO.Stream stream, Tables targetTable, Int32[] tableSizes, Byte[] tmpArray )
      {
         return new TableIndex( targetTable, ( (Int32) ( tableSizes[(Int32) targetTable] > UInt16.MaxValue ? stream.ReadU32( tmpArray ) : stream.ReadU16( tmpArray ) ) ) - 1 );
      }

      private static Tuple<Tables?[], UInt32, Int32> GetTablesForCodedIndex( CodedTableIndexKind indexKind )
      {
         Tuple<Tables?[], UInt32, Int32> possibleTables;
         switch ( indexKind )
         {
            case CodedTableIndexKind.CustomAttributeType:
               possibleTables = CUSTOM_ATTRIBUTE_TYPE;
               break;
            case CodedTableIndexKind.HasConstant:
               possibleTables = HAS_CONSTANT;
               break;
            case CodedTableIndexKind.HasCustomAttribute:
               possibleTables = HAS_CUSTOM_ATTRIBUTE;
               break;
            case CodedTableIndexKind.HasDeclSecurity:
               possibleTables = HAS_DECL_SECURITY;
               break;
            case CodedTableIndexKind.HasFieldMarshal:
               possibleTables = HAS_FIELD_MARSHAL;
               break;
            case CodedTableIndexKind.HasSemantics:
               possibleTables = HAS_SEMANTICS;
               break;
            case CodedTableIndexKind.Implementation:
               possibleTables = IMPLEMENTATION;
               break;
            case CodedTableIndexKind.MemberForwarded:
               possibleTables = MEMBER_FORWARDED;
               break;
            case CodedTableIndexKind.MemberRefParent:
               possibleTables = MEMBER_REF_PARENT;
               break;
            case CodedTableIndexKind.MethodDefOrRef:
               possibleTables = METHOD_DEF_OR_REF;
               break;
            case CodedTableIndexKind.ResolutionScope:
               possibleTables = RESOLUTION_SCOPE;
               break;
            case CodedTableIndexKind.TypeDefOrRef:
               possibleTables = TYPE_DEF_OR_REF;
               break;
            case CodedTableIndexKind.TypeOrMethodDef:
               possibleTables = TYPE_OR_METHOD_DEF;
               break;
            default:
               throw new ArgumentException( "Unknown code table index kind: " + indexKind );
         }
         return possibleTables;
      }

      internal static Byte[] WriteDataDirectory( this Byte[] array, ref Int32 idx, UInt32 addr, UInt32 size )
      {
         return array
            .WriteUInt32LEToBytes( ref idx, addr )
            .WriteUInt32LEToBytes( ref idx, size );
      }

      internal static Byte[] WriteZeroDataDirectory( this Byte[] array, ref Int32 idx )
      {
         return array.ZeroOut( ref idx, 8 );
      }

      // ECMA-335, p. 281
      internal static Byte[] WriteSectionInfo( this Byte[] array, ref Int32 idx, SectionInfo secInfo, String secName, UInt32 characteristics )
      {
         if ( secInfo.virtualSize > 0 )
         {
            return array
               .WriteASCIIString( ref idx, secName, false ) // Name
               .ZeroOut( ref idx, 8 - secName.Length ) // Zero padding
               .WriteUInt32LEToBytes( ref idx, secInfo.virtualSize ) // VirtualSize
               .WriteUInt32LEToBytes( ref idx, secInfo.virtualAddress ) // VirtualAddress
               .WriteUInt32LEToBytes( ref idx, secInfo.rawSize ) // SizeOfRawData
               .WriteUInt32LEToBytes( ref idx, secInfo.rawPointer ) // PointerToRawData
               .WriteUInt32LEToBytes( ref idx, 0 ) // PointerToRelocations
               .WriteUInt32LEToBytes( ref idx, 0 ) // PointerToLinenumbers
               .WriteUInt16LEToBytes( ref idx, 0 ) // NumberOfRelocations
               .WriteUInt16LEToBytes( ref idx, 0 ) // NumberOfLinenumbers
               .WriteUInt32LEToBytes( ref idx, characteristics ); // Characteristics
         }
         else
         {
            return array;
         }
      }
   }

   // ECMA-335, pp. 274-276
   internal enum CodedTableIndexKind
   {
      TypeDefOrRef,
      HasConstant,
      HasCustomAttribute,
      HasFieldMarshal,
      HasDeclSecurity,
      MemberRefParent,
      HasSemantics,
      MethodDefOrRef,
      MemberForwarded,
      Implementation,
      CustomAttributeType,
      ResolutionScope,
      TypeOrMethodDef
   }

   internal struct SectionInfo
   {
      internal readonly UInt32 virtualSize;
      internal readonly UInt32 virtualAddress;
      internal readonly UInt32 rawSize;
      internal readonly UInt32 rawPointer;

      internal SectionInfo( UInt32 virtualSize, UInt32 virtualAddress, UInt32 rawSize, UInt32 rawPointer )
      {
         this.virtualSize = virtualSize;
         this.virtualAddress = virtualAddress;
         this.rawSize = rawSize;
         this.rawPointer = rawPointer;
      }

      internal SectionInfo( Stream sink, SectionInfo? prevSection, UInt32 bytesWrittenInThisSection, UInt32 sectionAlignment, UInt32 fileAlignment, Boolean actuallyPad )
         : this(
         bytesWrittenInThisSection,
         prevSection.HasValue ? ( prevSection.Value.virtualAddress + BitUtils.MultipleOf( sectionAlignment, prevSection.Value.virtualSize ) ) : sectionAlignment,
          BitUtils.MultipleOf( fileAlignment, bytesWrittenInThisSection ),
         prevSection.HasValue ? ( prevSection.Value.rawPointer + prevSection.Value.rawSize ) : fileAlignment // prevSection.rawSize should always be multiple of file alignment
         )
      {
         if ( actuallyPad )
         {
            for ( var i = this.virtualSize; i < this.rawSize; ++i )
            {
               sink.WriteByte( 0 );
            }
         }
         else
         {
            sink.Seek( this.rawSize - this.virtualSize, SeekOrigin.Current );
         }
      }
   }

}
