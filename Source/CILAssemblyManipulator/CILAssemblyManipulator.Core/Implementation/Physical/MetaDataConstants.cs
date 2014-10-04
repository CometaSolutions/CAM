/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
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
using System.Text;
using CILAssemblyManipulator.API;
using TRVA = System.UInt32;

namespace CILAssemblyManipulator.Implementation.Physical
{
   internal static class MetaDataConstants
   {

      internal static readonly Encoding SYS_STRING_ENCODING = new UTF8Encoding( false, true );
      internal static readonly Encoding USER_STRING_ENCODING = new UnicodeEncoding( false, false, true );

      private const UInt32 TAGMASK_MASK = 0xFFFFFFFF;

      internal const Int32 DEBUG_DD_SIZE = 28;
      internal const Int32 CODE_VIEW_DEBUG_TYPE = 2;

      internal const Byte DECL_SECURITY_HEADER = 0x2E; // '.'
      internal const Int32 STREAM_COPY_BUFFER_SIZE = 0x2000; // 2x typical windows page size

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

      private static readonly IDictionary<Tables, Func<UInt32[], Int32, Int32, Int32, Int32>> TABLE_WIDTH_CALCULATOR;
      internal static readonly IDictionary<CILTypeCode, SignatureElementTypes> TYPECODE_MAPPING_SIMPLE = new Dictionary<CILTypeCode, SignatureElementTypes>()
      {
         { CILTypeCode.Boolean, SignatureElementTypes.Boolean },
         { CILTypeCode.Char, SignatureElementTypes.Char },
         { CILTypeCode.SByte, SignatureElementTypes.I1 },
         { CILTypeCode.Byte, SignatureElementTypes.U1 },
         { CILTypeCode.Int16, SignatureElementTypes.I2 },
         { CILTypeCode.UInt16, SignatureElementTypes.U2 },
         { CILTypeCode.Int32, SignatureElementTypes.I4 },
         { CILTypeCode.UInt32, SignatureElementTypes.U4 },
         { CILTypeCode.Int64, SignatureElementTypes.I8 },
         { CILTypeCode.UInt64, SignatureElementTypes.U8 },
         { CILTypeCode.Single, SignatureElementTypes.R4 },
         { CILTypeCode.Double, SignatureElementTypes.R8 },
         { CILTypeCode.String, SignatureElementTypes.String },
      };

      internal static readonly IDictionary<CILTypeCode, SignatureElementTypes> TYPECODE_MAPPING_FULL = new Dictionary<CILTypeCode, SignatureElementTypes>()
      {
         { CILTypeCode.Boolean, SignatureElementTypes.Boolean },
         { CILTypeCode.Char, SignatureElementTypes.Char },
         { CILTypeCode.SByte, SignatureElementTypes.I1 },
         { CILTypeCode.Byte, SignatureElementTypes.U1 },
         { CILTypeCode.Int16, SignatureElementTypes.I2 },
         { CILTypeCode.UInt16, SignatureElementTypes.U2 },
         { CILTypeCode.Int32, SignatureElementTypes.I4 },
         { CILTypeCode.UInt32, SignatureElementTypes.U4 },
         { CILTypeCode.Int64, SignatureElementTypes.I8 },
         { CILTypeCode.UInt64, SignatureElementTypes.U8 },
         { CILTypeCode.Single, SignatureElementTypes.R4 },
         { CILTypeCode.Double, SignatureElementTypes.R8 },
         { CILTypeCode.String, SignatureElementTypes.String },
         { CILTypeCode.Void, SignatureElementTypes.Void },
         { CILTypeCode.TypedByRef, SignatureElementTypes.TypedByRef },
         { CILTypeCode.IntPtr, SignatureElementTypes.I },
         { CILTypeCode.UIntPtr, SignatureElementTypes.U },
         { CILTypeCode.SystemObject, SignatureElementTypes.Object },
      };

      static MetaDataConstants()
      {
         var dic2 = new Dictionary<Tables, Func<UInt32[], Int32, Int32, Int32, Int32>>( TablesUtils.AMOUNT_OF_TABLES );
         dic2.Add( Tables.Module, ( tableSizes, strWidth, guidWidth, blobWidth ) => Consts.TWO_BYTE_SIZE + strWidth + guidWidth + guidWidth + guidWidth );
         dic2.Add( Tables.TypeRef, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeCoded( tableSizes, RESOLUTION_SCOPE ) + strWidth + strWidth );
         dic2.Add( Tables.TypeDef, ( tableSizes, strWidth, guidWidth, blobWidth ) => Consts.FOUR_BYTE_SIZE + strWidth + strWidth + GetTableIndexSizeCoded( tableSizes, TYPE_DEF_OR_REF ) + GetTableIndexSizeSimple( tableSizes, Tables.Field ) + GetTableIndexSizeSimple( tableSizes, Tables.MethodDef ) );
         dic2.Add( Tables.Field, ( tableSizes, strWidth, guidWidth, blobWidth ) => Consts.TWO_BYTE_SIZE + strWidth + blobWidth );
         dic2.Add( Tables.MethodDef, ( tableSizes, strWidth, guidWidth, blobWidth ) => sizeof( TRVA ) + Consts.TWO_BYTE_SIZE + Consts.TWO_BYTE_SIZE + strWidth + blobWidth + GetTableIndexSizeSimple( tableSizes, Tables.Parameter ) );
         dic2.Add( Tables.Parameter, ( tableSizes, strWidth, guidWidth, blobWidth ) => Consts.TWO_BYTE_SIZE + Consts.TWO_BYTE_SIZE + strWidth );
         dic2.Add( Tables.InterfaceImpl, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeSimple( tableSizes, Tables.TypeDef ) + GetTableIndexSizeCoded( tableSizes, TYPE_DEF_OR_REF ) );
         dic2.Add( Tables.MemberRef, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeCoded( tableSizes, MEMBER_REF_PARENT ) + strWidth + blobWidth );
         dic2.Add( Tables.Constant, ( tableSizes, strWidth, guidWidth, blobWidth ) => Consts.TWO_BYTE_SIZE + GetTableIndexSizeCoded( tableSizes, HAS_CONSTANT ) + blobWidth );
         dic2.Add( Tables.CustomAttribute, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeCoded( tableSizes, HAS_CUSTOM_ATTRIBUTE ) + GetTableIndexSizeCoded( tableSizes, CUSTOM_ATTRIBUTE_TYPE ) + blobWidth );
         dic2.Add( Tables.FieldMarshal, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeCoded( tableSizes, HAS_FIELD_MARSHAL ) + blobWidth );
         dic2.Add( Tables.DeclSecurity, ( tableSizes, strWidth, guidWidth, blobWidth ) => Consts.TWO_BYTE_SIZE + GetTableIndexSizeCoded( tableSizes, HAS_DECL_SECURITY ) + blobWidth );
         dic2.Add( Tables.ClassLayout, ( tableSizes, strWidth, guidWidth, blobWidth ) => Consts.TWO_BYTE_SIZE + Consts.FOUR_BYTE_SIZE + GetTableIndexSizeSimple( tableSizes, Tables.TypeDef ) );
         dic2.Add( Tables.FieldLayout, ( tableSizes, strWidth, guidWidth, blobWidth ) => Consts.FOUR_BYTE_SIZE + GetTableIndexSizeSimple( tableSizes, Tables.Field ) );
         dic2.Add( Tables.StandaloneSignature, ( tableSizes, strWidth, guidWidth, blobWidth ) => blobWidth );
         dic2.Add( Tables.EventMap, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeSimple( tableSizes, Tables.TypeDef ) + GetTableIndexSizeSimple( tableSizes, Tables.Event ) );
         dic2.Add( Tables.Event, ( tableSizes, strWidth, guidWidth, blobWidth ) => Consts.TWO_BYTE_SIZE + strWidth + GetTableIndexSizeCoded( tableSizes, TYPE_DEF_OR_REF ) );
         dic2.Add( Tables.PropertyMap, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeSimple( tableSizes, Tables.TypeDef ) + GetTableIndexSizeSimple( tableSizes, Tables.Property ) );
         dic2.Add( Tables.Property, ( tableSizes, strWidth, guidWidth, blobWidth ) => Consts.TWO_BYTE_SIZE + strWidth + blobWidth );
         dic2.Add( Tables.MethodSemantics, ( tableSizes, strWidth, guidWidth, blobWidth ) => Consts.TWO_BYTE_SIZE + GetTableIndexSizeSimple( tableSizes, Tables.MethodDef ) + GetTableIndexSizeCoded( tableSizes, HAS_SEMANTICS ) );
         dic2.Add( Tables.MethodImpl, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeSimple( tableSizes, Tables.TypeDef ) + GetTableIndexSizeCoded( tableSizes, METHOD_DEF_OR_REF ) + GetTableIndexSizeCoded( tableSizes, METHOD_DEF_OR_REF ) );
         dic2.Add( Tables.ModuleRef, ( tableSizes, strWidth, guidWidth, blobWidth ) => strWidth );
         dic2.Add( Tables.TypeSpec, ( tableSizes, strWidth, guidWidth, blobWidth ) => blobWidth );
         dic2.Add( Tables.ImplMap, ( tableSizes, strWidth, guidWidth, blobWidth ) => Consts.TWO_BYTE_SIZE + GetTableIndexSizeCoded( tableSizes, MEMBER_FORWARDED ) + strWidth + GetTableIndexSizeSimple( tableSizes, Tables.ModuleRef ) );
         dic2.Add( Tables.FieldRVA, ( tableSizes, strWidth, guidWidth, blobWidth ) => sizeof( TRVA ) + GetTableIndexSizeSimple( tableSizes, Tables.Field ) );
         dic2.Add( Tables.Assembly, ( tableSizes, strWidth, guidWidth, blobWidth ) => Consts.FOUR_BYTE_SIZE + Consts.TWO_BYTE_SIZE * 4 + Consts.FOUR_BYTE_SIZE + blobWidth + strWidth + strWidth );
         dic2.Add( Tables.AssemblyRef, ( tableSizes, strWidth, guidWidth, blobWidth ) => Consts.TWO_BYTE_SIZE * 4 + Consts.FOUR_BYTE_SIZE + blobWidth + strWidth + strWidth + blobWidth );
         dic2.Add( Tables.File, ( tableSizes, strWidth, guidWidth, blobWidth ) => Consts.FOUR_BYTE_SIZE + strWidth + blobWidth );
         dic2.Add( Tables.ExportedType, ( tableSizes, strWidth, guidWidth, blobWidth ) => Consts.FOUR_BYTE_SIZE + Consts.FOUR_BYTE_SIZE + strWidth + strWidth + GetTableIndexSizeCoded( tableSizes, IMPLEMENTATION ) );
         dic2.Add( Tables.ManifestResource, ( tableSizes, strWidth, guidWidth, blobWidth ) => Consts.FOUR_BYTE_SIZE + Consts.FOUR_BYTE_SIZE + strWidth + GetTableIndexSizeCoded( tableSizes, IMPLEMENTATION ) );
         dic2.Add( Tables.NestedClass, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeSimple( tableSizes, Tables.TypeDef ) + GetTableIndexSizeSimple( tableSizes, Tables.TypeDef ) );
         dic2.Add( Tables.GenericParameter, ( tableSizes, strWidth, guidWidth, blobWidth ) => Consts.TWO_BYTE_SIZE + Consts.TWO_BYTE_SIZE + GetTableIndexSizeCoded( tableSizes, TYPE_OR_METHOD_DEF ) + strWidth );
         dic2.Add( Tables.MethodSpec, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeCoded( tableSizes, METHOD_DEF_OR_REF ) + blobWidth );
         dic2.Add( Tables.GenericParameterConstraint, ( tableSizes, strWidth, guidWidth, blobWidth ) => GetTableIndexSizeSimple( tableSizes, Tables.GenericParameter ) + GetTableIndexSizeCoded( tableSizes, TYPE_DEF_OR_REF ) );

         TABLE_WIDTH_CALCULATOR = dic2;
      }

      private static Int32 GetTableIndexSizeSimple( UInt32[] tableSizes, Tables referencedTable )
      {
         return tableSizes[(Int32) referencedTable] > UInt16.MaxValue ? 4 : 2;
      }

      private static Int32 GetTableIndexSizeCoded( UInt32[] tableSizes, Tuple<Tables?[], UInt32, Int32> referencedTables )
      {
         UInt32 max = 0;
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

      internal static IDictionary<CodedTableIndexKind, Boolean> GetCodedTableIndexSizes( UInt32[] tableSizes )
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

      internal static Int32 CalculateTableWidth( Tables table, UInt32[] tableSizes, Int32 sysStringIndexSize, Int32 guidIndexSize, Int32 blobIndexSize )
      {
         return MetaDataConstants.TABLE_WIDTH_CALCULATOR[table]( tableSizes, sysStringIndexSize, guidIndexSize, blobIndexSize );
      }

      internal static Int32 GetCodedTableIndex( CodedTableIndexKind indexKind, Int32 token )
      {
         Tables table;
         Int32 idx;
         TokenUtils.DecodeToken( token, out table, out idx );
         var possibleTables = GetTablesForCodedIndex( indexKind );
         return ( idx << possibleTables.Item3 ) | Array.IndexOf( possibleTables.Item1, table );
      }

      internal static TableIndex? ReadCodedTableIndex( System.IO.Stream stream, CodedTableIndexKind indexKind, IDictionary<CodedTableIndexKind, Boolean> tRefSizes, Byte[] tmpArray, Boolean throwOnNull = true )
      {
         var idx = tRefSizes[indexKind] ? stream.ReadU32( tmpArray ) : stream.ReadU16( tmpArray );
         if ( idx == 0 )
         {
            if ( throwOnNull )
            {
               throw new BadImageFormatException( "Found null coded table index when shouldn't (table reference kind: " + indexKind + ")." );
            }
            else
            {
               return null;
            }
         }
         else
         {
            var possibleTables = GetTablesForCodedIndex( indexKind );
            return new TableIndex( possibleTables.Item1[possibleTables.Item2 & idx].Value, (Int32) ( idx >> possibleTables.Item3 ) - 1 );
         }
      }

      // Zero-based
      internal static TableIndex ReadSimpleTableIndex( System.IO.Stream stream, Tables targetTable, UInt32[] tableSizes, Byte[] tmpArray )
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
   }

   internal struct TableIndex
   {
      internal readonly Tables table;
      internal readonly Int32 idx; // Zero-based
      internal TableIndex( Tables aTable, Int32 anIdx )
      {
         if ( anIdx < 0 )
         {
            throw new BadImageFormatException( "Simple index to table " + aTable + " was null." );
         }
         this.table = aTable;
         this.idx = anIdx;
      }

      internal TableIndex( Int32 token )
      {
         TokenUtils.DecodeTokenZeroBased( token, out this.table, out this.idx );
         if ( idx < 0 )
         {
            throw new BadImageFormatException( "Token had zero as index (" + this + ")." );
         }
      }

      public override string ToString()
      {
         return this.table + "[" + this.idx + "]";
      }
   }
}