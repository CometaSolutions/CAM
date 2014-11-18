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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CILAssemblyManipulator.API;

namespace CILAssemblyManipulator.Implementation.Physical
{
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

      // Mofidied from http://stackoverflow.com/questions/1068541/how-to-convert-a-value-type-to-byte-in-c
      internal static Byte[] ObjectToByteArray( Object value )
      {
#if WINDOWS_PHONE_APP
#else
         var rawsize = Marshal.SizeOf( value );
#endif
         var rawdata = new byte[rawsize];
         var handle =
             GCHandle.Alloc( rawdata,
             GCHandleType.Pinned );
         try
         {
#if WINDOWS_PHONE_APP
#else
            Marshal.StructureToPtr( value,
                handle.AddrOfPinnedObject(),
                false );
#endif
         }
         finally
         {
            handle.Free();
         }
         return rawdata;
      }

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
}