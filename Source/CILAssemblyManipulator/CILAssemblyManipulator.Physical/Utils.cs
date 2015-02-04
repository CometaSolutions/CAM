using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
   using TRVA = UInt32;

   internal static class TokenUtils
   {
      internal const Int32 INDEX_MASK = 0x00FFFFF;

      private const Int32 TYPE_DEF = 0;
      private const Int32 TYPE_REF = 1;
      private const Int32 TYPE_SPEC = 2;
      private const Int32 TABLE_EXTRACTOR_MASK = 0x3;
      private const Int32 TYPE_DEF_MASK = ( (Byte) Tables.TypeDef ) << 24; // 0x2000000;
      private const Int32 TYPE_REF_MASK = ( (Byte) Tables.TypeRef ) << 24; // 0x1000000;
      private const Int32 TYPE_SPEC_MASK = ( (Byte) Tables.TypeSpec ) << 24; // 0x1B000000;

      internal static Tuple<Tables, Int32> DecodeToken( Int32 token )
      {
         Tables table;
         Int32 index;
         DecodeToken( token, out table, out index );
         return Tuple.Create( table, index );
      }

      internal static Tables DecodeTokenTable( Int32 token )
      {
         return (Tables) ( unchecked( (UInt32) token ) >> 24 );
      }

      internal static void DecodeToken( Int32 token, out Tables table, out Int32 index )
      {
         table = (Tables) ( unchecked( (UInt32) token ) >> 24 );
         index = token & INDEX_MASK;
      }

      internal static void DecodeTokenZeroBased( Int32 token, out Tables table, out Int32 index )
      {
         table = (Tables) ( unchecked( (UInt32) token ) >> 24 );
         index = ( token & INDEX_MASK ) - 1;
      }

      internal static Int32 EncodeToken( Tuple<Tables, Int32> tableRef )
      {
         return EncodeToken( tableRef.Item1, tableRef.Item2 );
      }

      internal static Int32 EncodeToken( Tables table, Int32 index )
      {
         return EncodeToken( (Int32) table, index );
      }

      internal static Int32 EncodeToken( Int32 table, Int32 index )
      {
         return ( ( table ) << 24 ) | index;
      }

      internal static Int32 DecodeTypeDefOrRefOrSpec( Byte[] array, ref Int32 offset )
      {
         var decodedValue = array.DecompressUInt32( ref offset );
         switch ( decodedValue & TABLE_EXTRACTOR_MASK )
         {
            case TYPE_DEF:
               decodedValue = TYPE_DEF_MASK | ( decodedValue >> 2 );
               break;
            case TYPE_REF:
               decodedValue = TYPE_REF_MASK | ( decodedValue >> 2 );
               break;
            case TYPE_SPEC:
               decodedValue = TYPE_SPEC_MASK | ( decodedValue >> 2 );
               break;
            default:
               throw new ArgumentException( "Token table resolved to not supported: " + (Tables) ( decodedValue & TABLE_EXTRACTOR_MASK ) + "." );
         }
         return decodedValue;
      }

      internal static Int32 EncodeTypeDefOrRefOrSpec( Int32 token )
      {
         Int32 encodedValue;
         switch ( unchecked( (UInt32) token ) >> 24 )
         {
            case (UInt32) Tables.TypeDef:
               encodedValue = ( ( INDEX_MASK & token ) << 2 ) | TYPE_DEF;
               break;
            case (UInt32) Tables.TypeRef:
               encodedValue = ( ( INDEX_MASK & token ) << 2 ) | TYPE_REF;
               break;
            case (UInt32) Tables.TypeSpec:
               encodedValue = ( ( INDEX_MASK & token ) << 2 ) | TYPE_SPEC;
               break;
            default:
               throw new ArgumentException( "Token must reference one of the following tables: " + String.Join( ", ", Tables.TypeDef, Tables.TypeRef, Tables.TypeSpec ) + "." );
         }
         return encodedValue;
      }
   }

   // TODO maybe move this to API utils?
   internal static class BitUtils
   {
      private enum EncodingForcePolicy : byte { DontForce, Force2Byte, Force4Byte }

      internal static String ReadLenPrefixedUTF8String( this Byte[] caBLOB, ref Int32 idx )
      {
         var tmp = caBLOB[idx];
         String result;
         if ( tmp != 0xFF )
         {
            var len = caBLOB.DecompressUInt32( ref idx );
            result = MetaDataConstants.SYS_STRING_ENCODING.GetString( caBLOB, idx, len );
            idx += len;
         }
         else
         {
            ++idx;
            result = null;
         }
         return result;
      }

      internal static String ReadZeroTerminatedStringFromBytes( this Byte[] array, Encoding encoding )
      {
         Int32 idx = 0;
         return array.ReadZeroTerminatedStringFromBytes( ref idx, encoding );
      }

      internal static Byte[] WriteHeapIndex( this Byte[] array, ref Int32 idx, UInt32 heapIndex, Boolean wideIndex )
      {
         return wideIndex ? array.WriteUInt32LEToBytes( ref idx, heapIndex ) : array.WriteUInt16LEToBytes( ref idx, (UInt16) heapIndex );
      }

      internal static Byte[] WriteCodedTableIndex( this Byte[] array, ref Int32 idx, CodedTableIndexKind codedKind, Int32 codedIndex, IDictionary<CodedTableIndexKind, Boolean> wideIndices )
      {
         return wideIndices[codedKind] ? array.WriteInt32LEToBytes( ref idx, codedIndex ) : array.WriteUInt16LEToBytes( ref idx, (UInt16) codedIndex );
      }

      internal static Byte[] WriteSimpleTableIndex( this Byte[] array, ref Int32 idx, Tables table, Int32 tableIndex, UInt32[] tableSizes )
      {
         return tableSizes[(Int32) table] > UInt16.MaxValue ? array.WriteInt32LEToBytes( ref idx, tableIndex ) : array.WriteUInt16LEToBytes( ref idx, (UInt16) tableIndex );
      }

      internal static String ReadZeroTerminatedASCIIStringFromBytes( this Byte[] array )
      {
         var length = array.Length;
         var charBuf = new Char[length];
         var amountRead = 0;
         while ( amountRead < length )
         {
            var b = array[amountRead];
            if ( b == 0 )
            {
               break;
            }
            charBuf[amountRead++] = (Char) b;
         }
         return new String( charBuf, 0, amountRead );
      }

      public static Byte[] WriteDataDirectory( this Byte[] array, ref Int32 idx, UInt32 addr, UInt32 size )
      {
         return array
            .WriteUInt32LEToBytes( ref idx, addr )
            .WriteUInt32LEToBytes( ref idx, size );
      }

      public static Byte[] WriteZeroDataDirectory( this Byte[] array, ref Int32 idx )
      {
         // Assume array was already zeroes
         idx += 8;
         return array;
      }

      // ECMA-335, p. 281
      public static Byte[] WriteSectionInfo( this Byte[] array, ref Int32 idx, SectionInfo secInfo, String secName, UInt32 characteristics )
      {
         if ( secInfo.virtualSize > 0 )
         {
            return array
               .WriteASCIIString( ref idx, secName, false ) // Name
               .Skip( ref idx, 8 - secName.Length ) // Zero padding
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

      public static Byte[] SkipToNextAlignment( this Byte[] array, ref Int32 idx, Int32 alignment )
      {
         idx += MultipleOf( alignment, idx ) - idx;
         return array;
      }

      public static UInt32 SkipToNextAlignment( this Stream sink, ref UInt32 idx, UInt32 alignment )
      {
         var amountToSkip = MultipleOf( alignment, idx ) - idx;
         // Instead of skipping, actually fill with zeroes
         sink.Write( new Byte[amountToSkip] );
         //sink.Seek( amountToSkip, SeekOrigin.Current );
         idx += amountToSkip;
         return amountToSkip;
      }



      private const UInt32 UINT_FOUR_BYTES_MASK = 0xC0;
      private const UInt32 UINT_TWO_BYTES_MASK = 0x80;
      private const Int32 UINT_TWO_BYTES_DECODE_MASK = 0x3F;
      private const Int32 UINT_FOUR_BYTES_DECODE_MASK = 0x1F;

      private const Int32 UINT_ONE_BYTE_MAX = 0x7F;
      private const Int32 UINT_TWO_BYTES_MAX = 0x3FFF;
      private const Int32 UINT_FOUR_BYTES_MAX = 0x1FFFFFFF;

      private const Int32 INT_ONE_BYTE_MIN = -0x40;
      private const Int32 INT_ONE_BYTE_MAX = 0x3F;
      private const Int32 INT_TWO_BYTES_MIN = -0x2000;
      private const Int32 INT_TWO_BYTES_MAX = 0x1FFF;
      private const Int32 INT_FOUR_BYTES_MIN = -0x10000000;
      private const Int32 INT_FOUR_BYTES_MAX = 0xFFFFFFF;

      private const Int32 INT_ONE_BYTE_ENCODE_MASK = 0x40;
      private const Int32 INT_TWO_BYTES_ENCODE_MASK = 0x2000;
      private const Int32 INT_FOUR_BYTES_ENCODE_MASK = 0x10000000;

      private const UInt32 ONE = 1;
      private const Int32 COMPLEMENT_MASK_ONE_BYTE = unchecked( (Int32) 0xFFFFFFC0 );
      private const Int32 COMPLEMENT_MASK_TWO_BYTES = unchecked( (Int32) 0xFFFFE000 );
      private const Int32 COMPLEMENT_MASK_FOUR_BYTES = unchecked( (Int32) 0xF0000000 );

      public static Int32 DecompressUInt32( this Byte[] array, ref Int32 offset )
      {
         Int32 first = array[offset];
         ++offset;
         Int32 result;
         if ( ( UINT_FOUR_BYTES_MASK & first ) == UINT_FOUR_BYTES_MASK )
         {
            result = ( ( first & UINT_FOUR_BYTES_DECODE_MASK ) << 24 ) | ( ( (Int32) array[offset] ) << 16 ) | ( ( (Int32) array[offset + 1] ) << 8 ) | array[offset + 2];
            offset += 3;
         }
         else if ( ( UINT_TWO_BYTES_MASK & first ) == UINT_TWO_BYTES_MASK )
         {
            result = ( ( first & UINT_TWO_BYTES_DECODE_MASK ) << 8 ) | (Int32) array[offset];
            ++offset;
         }
         else
         {
            result = first;
         }
         return result;
      }

      public static Int32 DecompressInt32( this Byte[] array, ref Int32 offset )
      {
         var oldOffset = offset;
         var decodedUInt = array.DecompressUInt32( ref offset );
         var bytesRead = offset - oldOffset;
         if ( bytesRead == 1 )
         {
            // Value is one-bit left rotated, 7-bit 2-complement number
            // If LSB is 1 -> then the value is negative
            if ( ( decodedUInt & ONE ) == ONE )
            {
               decodedUInt = ( decodedUInt >> 1 ) | COMPLEMENT_MASK_ONE_BYTE;
            }
            else
            {
               decodedUInt = decodedUInt >> 1;
            }
         }
         else if ( bytesRead == 2 )
         {
            if ( ( decodedUInt & ONE ) == ONE )
            {
               decodedUInt = ( decodedUInt >> 1 ) | COMPLEMENT_MASK_TWO_BYTES;
            }
            else
            {
               decodedUInt = decodedUInt >> 1;
            }
         }
         else // if ( bytesRead == 4 )
         {
            if ( ( decodedUInt & ONE ) == ONE )
            {
               decodedUInt = ( decodedUInt >> 1 ) | COMPLEMENT_MASK_FOUR_BYTES;
            }
            else
            {
               decodedUInt = decodedUInt >> 1;
            }
         }

         return unchecked( (Int32) decodedUInt );
      }

      public static void CompressUInt32( this Byte[] array, ref Int32 offset, Int32 value )
      {
         EncodeUInt32( array, ref offset, value, EncodingForcePolicy.DontForce );
      }

      public static void CompressInt32( this  Byte[] array, ref Int32 offset, Int32 value )
      {
         Int32 uValue = value;

         EncodingForcePolicy policy;
         if ( value >= INT_ONE_BYTE_MIN && value <= INT_ONE_BYTE_MAX )
         {
            policy = EncodingForcePolicy.DontForce;
            if ( value < 0 )
            {
               // Represent the value as 7-bit 2-complement number
               uValue = ( INT_ONE_BYTE_ENCODE_MASK | ( uValue & INT_ONE_BYTE_MAX ) );
            }
            // Rotate value 1 bit left
            uValue = ( ( uValue << 1 ) | ( uValue >> 6 ) ) & UINT_ONE_BYTE_MAX;
         }
         else if ( value >= INT_TWO_BYTES_MIN && value <= INT_TWO_BYTES_MAX )
         {
            policy = EncodingForcePolicy.Force2Byte;
            if ( value < 0 )
            {
               // Represent the value as 14-bit 2-complement number
               uValue = ( INT_TWO_BYTES_ENCODE_MASK | ( uValue & INT_TWO_BYTES_MAX ) );
            }
            // Rotate value 1 bit left
            uValue = ( ( uValue << 1 ) | ( uValue >> 13 ) ) & UINT_TWO_BYTES_MAX;
         }
         else if ( value >= INT_FOUR_BYTES_MIN || value <= INT_FOUR_BYTES_MAX )
         {
            policy = EncodingForcePolicy.Force4Byte;
            if ( value < 0 )
            {
               // Represent the value as 28-bit 2-complement number
               uValue = ( INT_FOUR_BYTES_ENCODE_MASK | ( uValue & INT_FOUR_BYTES_MAX ) );
            }

            // Rotate value 1 bit left
            uValue = ( ( uValue << 1 ) | ( uValue >> 28 ) ) & UINT_FOUR_BYTES_MAX;
         }
         else
         {
            throw new ArgumentException( "Int32 value " + value + "was too big or too small to be encoded (should be between " + INT_FOUR_BYTES_MIN + " and " + INT_FOUR_BYTES_MAX + ")." );
         }
         EncodeUInt32( array, ref offset, uValue, policy );
      }

      private static void EncodeUInt32( Byte[] array, ref Int32 offset, Int32 value, EncodingForcePolicy forcePolicy )
      {
         if ( forcePolicy == EncodingForcePolicy.DontForce && value <= UINT_ONE_BYTE_MAX )
         {
            array[offset++] = (Byte) value;
         }
         else if ( forcePolicy != EncodingForcePolicy.Force4Byte && value <= UINT_TWO_BYTES_MAX )
         {
#if __MonoCS__
#pragma warning disable 675
#endif
            array[offset++] = (Byte) ( ( value >> 8 ) | UINT_TWO_BYTES_MASK );
#if __MonoCS__
#pragma warning restore 675
#endif
            array[offset++] = (Byte) ( value & Byte.MaxValue );
         }
         else if ( value <= UINT_FOUR_BYTES_MAX )
         {
#if __MonoCS__
#pragma warning disable 675
#endif
            array[offset++] = (Byte) ( ( value >> 24 ) | UINT_FOUR_BYTES_MASK );
#if __MonoCS__
#pragma warning restore 675
#endif
            array[offset++] = (Byte) ( value >> 16 );
            array[offset++] = (Byte) ( value >> 8 );
            array[offset++] = (Byte) ( value & Byte.MaxValue );
         }
         else
         {
            throw new ArgumentException( "UInt32 value " + value + " was too big to be encoded (max value is " + UINT_FOUR_BYTES_MAX + ")." );
         }
      }

      internal static UInt32 GetEncodedIntSize( Int32 value )
      {
         if ( value >= INT_ONE_BYTE_MIN && value <= INT_ONE_BYTE_MAX )
         {
            return 1;
         }
         else if ( value >= INT_TWO_BYTES_MIN && value <= INT_TWO_BYTES_MAX )
         {
            return 2;
         }
         else if ( value >= INT_FOUR_BYTES_MIN || value <= INT_FOUR_BYTES_MAX )
         {
            return 4;
         }
         else
         {
            throw new ArgumentException( "Int32 value " + value + "was too big or too small to be encoded (should be between " + INT_FOUR_BYTES_MIN + " and " + INT_FOUR_BYTES_MAX + ")." );
         }
      }

      internal static UInt32 GetEncodedUIntSize( Int32 value )
      {
         if ( value < 0 )
         {
            throw new ArgumentException( "Number " + value + " is not UInt32." );
         }

         if ( value <= UINT_ONE_BYTE_MAX )
         {
            return 1;
         }
         else if ( value <= UINT_TWO_BYTES_MAX )
         {
            return 2;
         }
         else if ( value <= UINT_FOUR_BYTES_MAX )
         {
            return 4;
         }
         else
         {
            throw new ArgumentException( "Int32 value " + value + "was too big or too small to be encoded (should be between " + INT_FOUR_BYTES_MIN + " and " + INT_FOUR_BYTES_MAX + ")." );
         }
      }

      internal static Int32 MultipleOf4( Int32 value )
      {
         return MultipleOf( 4, value );
      }

      internal static UInt32 MultipleOf4( UInt32 value )
      {
         return MultipleOf( 4, value );
      }

      // Assumes "multiple" is power of 2
      internal static Int32 MultipleOf( Int32 multiple, Int32 value )
      {
         --multiple;
         return ( value + multiple ) & ~multiple;
         //return value % multiple == 0 ? value : ( multiple * ( ( value / multiple ) + 1 ) );
      }

      // Assumes "multiple" is power of 2
      internal static UInt32 MultipleOf( UInt32 multiple, UInt32 value )
      {
         --multiple;
         return ( value + multiple ) & ~multiple;
         //return value % multiple == 0 ? value : ( multiple * ( ( value / multiple ) + 1 ) );
      }

      //      // Mofidied from http://stackoverflow.com/questions/1068541/how-to-convert-a-value-type-to-byte-in-c
      //      internal static Byte[] ObjectToByteArray( Object value )
      //      {
      //#if WINDOWS_PHONE_APP
      //#else
      //         var rawsize = Marshal.SizeOf( value );
      //#endif
      //         var rawdata = new byte[rawsize];
      //         var handle =
      //             GCHandle.Alloc( rawdata,
      //             GCHandleType.Pinned );
      //         try
      //         {
      //#if WINDOWS_PHONE_APP
      //#else
      //            Marshal.StructureToPtr( value,
      //                handle.AddrOfPinnedObject(),
      //                false );
      //#endif
      //         }
      //         finally
      //         {
      //            handle.Free();
      //         }
      //         return rawdata;
      //      }

      internal static UInt32 Sum( this IEnumerable<UInt32> enumerable )
      {
         var total = 0u;
         checked
         {
            foreach ( var item in enumerable )
            {
               total += item;
            }
         }
         return total;
      }
   }

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
         var dic2 = new Dictionary<Tables, Func<UInt32[], Int32, Int32, Int32, Int32>>( Consts.AMOUNT_OF_TABLES );
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

         TableIndex? retVal;
         if ( CheckRowIndex( indexKind, throwOnNull, idx ) )
         {
            var possibleTables = GetTablesForCodedIndex( indexKind );
            var rowIdx = ( idx >> possibleTables.Item3 );
            if ( CheckRowIndex( indexKind, throwOnNull, rowIdx ) )
            {
               retVal = new TableIndex( possibleTables.Item1[possibleTables.Item2 & idx].Value, (Int32) rowIdx - 1 );
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

      private static Boolean CheckRowIndex( CodedTableIndexKind indexKind, Boolean throwOnNull, UInt32 index )
      {
         var retVal = index > 0;
         if ( !retVal )
         {
            if ( throwOnNull )
            {
               throw new BadImageFormatException( "Found null coded table index when shouldn't (table reference kind: " + indexKind + ")." );
            }
         }

         return retVal;
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

   internal static class StreamExtensions
   {

      /// <summary>
      /// Using specified auxiliary array, reads a <see cref="UInt64"/> from <see cref="Stream"/>.
      /// </summary>
      /// <param name="stream">The <see cref="Stream"/>.</param>
      /// <param name="i64Array">The auxiliary array, must be at least 8 bytes long.</param>
      /// <returns>The <see cref="UInt64"/> read from current position of the <paramref name="stream"/>.</returns>
      /// <remarks>
      /// See <see cref="E_CommonUtils.ReadSpecificAmount(Stream, Byte[], Int32, Int32)"/> for more exceptions.
      /// </remarks>
      internal static UInt64 ReadU64( this Stream stream, Byte[] i64Array )
      {
         stream.ReadSpecificAmount( i64Array, 0, 8 );
         var dummy = 0;
         return i64Array.ReadUInt64LEFromBytes( ref dummy );
      }

      /// <summary>
      /// Using specified auxiliary array, reads a <see cref="UInt32"/> from <see cref="Stream"/>.
      /// </summary>
      /// <param name="stream">The <see cref="Stream"/>.</param>
      /// <param name="i32Array">The auxiliary array, must be at least 4 bytes long.</param>
      /// <returns>The <see cref="UInt32"/> read from current position of the <paramref name="stream"/>.</returns>
      /// <remarks>
      /// See <see cref="E_CommonUtils.ReadSpecificAmount(Stream, Byte[], Int32, Int32)"/> for more exceptions.
      /// </remarks>
      internal static UInt32 ReadU32( this Stream stream, Byte[] i32Array )
      {
         stream.ReadSpecificAmount( i32Array, 0, 4 );
         return (UInt32) i32Array.ReadInt32LEFromBytesNoRef( 0 );
      }

      /// <summary>
      /// Using specified auxiliary array, reads a <see cref="UInt16"/> from <see cref="Stream"/>.
      /// </summary>
      /// <param name="stream">The <see cref="Stream"/>.</param>
      /// <param name="i16Array">The auxiliary array, must be at least 2 bytes long.</param>
      /// <returns>The <see cref="UInt16"/> read from current position of the <paramref name="stream"/>.</returns>
      /// <remarks>
      /// See <see cref="E_CommonUtils.ReadSpecificAmount(Stream, Byte[], Int32, Int32)"/> for more exceptions.
      /// </remarks>
      internal static UInt16 ReadU16( this Stream stream, Byte[] i16Array )
      {
         stream.ReadSpecificAmount( i16Array, 0, 2 );
         var dummy = 0;
         return i16Array.ReadUInt16LEFromBytes( ref dummy );
      }

      internal static String ReadZeroTerminatedString( this Stream stream, UInt32 length, Encoding encoding )
      {
         var buf = new Byte[length];
         stream.ReadWholeArray( buf );
         return buf.ReadZeroTerminatedStringFromBytes( encoding );
      }


      internal static String ReadZeroTerminatedASCIIString( this Stream stream, UInt32 length )
      {
         var buf = new Byte[length];
         stream.ReadWholeArray( buf );
         return buf.ReadZeroTerminatedASCIIStringFromBytes();
      }

      internal static String ReadAlignedASCIIString( this Stream stream, Int32 maxLength )
      {
         var bytesRead = 0;
         var charBufSize = 0;
         var charBuf = new Char[maxLength];
         while ( bytesRead < maxLength )
         {
            var b = stream.ReadByteFromStream();
            if ( b == 0 )
            {
               ++bytesRead;
               if ( bytesRead % 4 == 0 )
               {
                  break;
               }
            }
            else
            {
               charBuf[bytesRead++] = (char) b;
               ++charBufSize;
            }
         }
         return new String( charBuf, 0, charBufSize );
      }

      internal static UInt32 ReadHeapIndex( this Stream stream, Boolean wideIndex, Byte[] tmpArray )
      {
         return wideIndex ? stream.ReadU32( tmpArray ) : stream.ReadU16( tmpArray );
      }
   }

   internal static class Consts
   {
      internal const String TABLE_STREAM_NAME = "#~";
      internal const String SYS_STRING_STREAM_NAME = "#Strings";
      internal const String USER_STRING_STREAM_NAME = "#US";
      internal const String GUID_STREAM_NAME = "#GUID";
      internal const String BLOB_STREAM_NAME = "#Blob";

      internal const String MSCORLIB_NAME = "mscorlib";
      internal const String NEW_MSCORLIB_NAME = "System.Runtime";

      internal const Int32 TWO_BYTE_SIZE = 2;
      internal const Int32 FOUR_BYTE_SIZE = 4;
      internal const Int32 GUID_SIZE = 16;

      internal const Int32 AMOUNT_OF_TABLES = 0x2D; // Enum.GetValues( typeof( Tables ) ).Length;


      internal const String MULTICAST_DELEGATE = "System.MulticastDelegate";
      internal const String NULLABLE = "System.Nullable`1";
      internal const String LAZY = "System.Lazy`1";
      internal const String METHOD_INFO = "System.Reflection.MethodInfo";
      internal const String CTOR_INFO = "System.Reflection.ConstructorInfo";
      internal const String FIELD_INFO = "System.Reflection.FieldInfo";
      internal const String BOOLEAN = "System.Boolean";
      internal const String CHAR = "System.Char";
      internal const String SBYTE = "System.SByte";
      internal const String BYTE = "System.Byte";
      internal const String INT16 = "System.Int16";
      internal const String UINT16 = "System.UInt16";
      internal const String INT32 = "System.Int32";
      internal const String UINT32 = "System.UInt32";
      internal const String INT64 = "System.Int64";
      internal const String UINT64 = "System.UInt64";
      internal const String SINGLE = "System.Single";
      internal const String DOUBLE = "System.Double";
      internal const String DECIMAL = "System.Decimal";
      internal const String DATETIME = "System.DateTime";
      internal const String VOID = "System.Void";
      internal const String OBJECT = "System.Object";
      internal const String INT_PTR = "System.IntPtr";
      internal const String UINT_PTR = "System.UIntPtr";
      internal const String STRING = "System.String";
      internal const String TYPE = "System.Type";
      internal const String VALUE_TYPE = "System.ValueType";
      internal const String ENUM = "System.Enum";
      internal const String SECURITY_ATTR = "System.Security.Permissions.SecurityAttribute";
      internal const String SECURITY_ACTION = "System.Security.Permissions.SecurityAction";
      internal const String PERMISSION_SET = "System.Security.Permissions.PermissionSetAttribute";
      internal const String PERMISSION_SET_XML_PROP = "XML";
   }

   internal struct SectionInfo
   {
      internal readonly UInt32 virtualSize;
      internal readonly UInt32 virtualAddress;
      internal readonly UInt32 rawSize;
      internal readonly UInt32 rawPointer;

      internal SectionInfo( Stream sink, SectionInfo? prevSection, UInt32 bytesWrittenInThisSection, UInt32 sectionAlignment, UInt32 fileAlignment, Boolean actuallyPad )
      {
         this.virtualSize = bytesWrittenInThisSection;
         this.virtualAddress = prevSection.HasValue ? ( prevSection.Value.virtualAddress + BitUtils.MultipleOf( sectionAlignment, prevSection.Value.virtualSize ) ) : sectionAlignment;
         this.rawPointer = prevSection.HasValue ? ( prevSection.Value.rawPointer + prevSection.Value.rawSize ) : fileAlignment; // prevSection.rawSize should always be multiple of file alignment
         this.rawSize = BitUtils.MultipleOf( fileAlignment, bytesWrittenInThisSection );
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
