﻿/*
 * Copyright 2014 Stanislav Muhametsin. All rights Reserved.
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

namespace CommonUtils
{
   /// <summary>
   /// This class provides utility methods related to binary operations, which are not sensible to create as extension methods.
   /// </summary>
   public static class BinaryUtils
   {
      // From http://graphics.stanford.edu/~seander/bithacks.html#IntegerLogLookup

      // The log base 2 of an integer is the same as the position of the highest bit set (or most significant bit set, MSB).

      private static readonly Int32[] LOG_TABLE_256;

      static BinaryUtils()
      {
         var arr = new Int32[256];
         for ( var i = 2; i < 256; ++i )
         {
            arr[i] = 1 + arr[i / 2];
         }
         arr[0] = -1;
         LOG_TABLE_256 = arr;
      }

      /// <summary>
      /// Rotates given <paramref name="value"/> left <paramref name="shift"/> amount of bytes.
      /// </summary>
      /// <param name="value">The value to rotate to left.</param>
      /// <param name="shift">The amount to bits to rotate.</param>
      /// <returns>The rotated value.</returns>
      public static Int32 RotateLeft32( Int32 value, Int32 shift )
      {
         return (Int32) RotateLeft32( (UInt32) value, shift );
      }

      /// <summary>
      /// Rotates given <paramref name="value"/> left <paramref name="shift"/> amount of bytes.
      /// </summary>
      /// <param name="value">The value to rotate to left.</param>
      /// <param name="shift">The amount to bits to rotate.</param>
      /// <returns>The rotated value.</returns>
      [CLSCompliant( false )]
      public static UInt32 RotateLeft32( UInt32 value, Int32 shift )
      {
         return ( value << shift ) | ( value >> ( sizeof( UInt32 ) * 8 - shift ) );
      }

      /// <summary>
      /// Rotates given <paramref name="value"/> right <paramref name="shift"/> amount of bytes.
      /// </summary>
      /// <param name="value">The value to rotate to right.</param>
      /// <param name="shift">The amount to bits to rotate.</param>
      /// <returns>The rotated value.</returns>
      public static Int32 RotateRight32( Int32 value, Int32 shift )
      {
         return (Int32) RotateRight32( (UInt32) value, shift );
      }

      /// <summary>
      /// Rotates given <paramref name="value"/> right <paramref name="shift"/> amount of bytes.
      /// </summary>
      /// <param name="value">The value to rotate to right.</param>
      /// <param name="shift">The amount to bits to rotate.</param>
      /// <returns>The rotated value.</returns>
      [CLSCompliant( false )]
      public static UInt32 RotateRight32( UInt32 value, Int32 shift )
      {
         return ( value >> shift ) | ( value << ( sizeof( UInt32 ) * 8 - shift ) );
      }

      /// <summary>
      /// Rotates given <paramref name="value"/> left <paramref name="shift"/> amount of bytes.
      /// </summary>
      /// <param name="value">The value to rotate to left.</param>
      /// <param name="shift">The amount to bits to rotate.</param>
      /// <returns>The rotated value.</returns>
      public static Int64 RotateLeft64( Int64 value, Int32 shift )
      {
         return (Int64) RotateLeft64( (UInt64) value, shift );
      }

      /// <summary>
      /// Rotates given <paramref name="value"/> left <paramref name="shift"/> amount of bytes.
      /// </summary>
      /// <param name="value">The value to rotate to left.</param>
      /// <param name="shift">The amount to bits to rotate.</param>
      /// <returns>The rotated value.</returns>
      [CLSCompliant( false )]
      public static UInt64 RotateLeft64( UInt64 value, Int32 shift )
      {
         return ( value << shift ) | ( value >> ( sizeof( UInt64 ) * 8 - shift ) );
      }

      /// <summary>
      /// Rotates given <paramref name="value"/> right <paramref name="shift"/> amount of bytes.
      /// </summary>
      /// <param name="value">The value to rotate to right.</param>
      /// <param name="shift">The amount to bits to rotate.</param>
      /// <returns>The rotated value.</returns>
      public static Int64 RotateRight64( Int64 value, Int32 shift )
      {
         return (Int64) RotateRight64( (UInt64) value, shift );
      }

      /// <summary>
      /// Rotates given <paramref name="value"/> right <paramref name="shift"/> amount of bytes.
      /// </summary>
      /// <param name="value">The value to rotate to right.</param>
      /// <param name="shift">The amount to bits to rotate.</param>
      /// <returns>The rotated value.</returns>
      [CLSCompliant( false )]
      public static UInt64 RotateRight64( UInt64 value, Int32 shift )
      {
         return ( value >> shift ) | ( value << ( sizeof( UInt64 ) * 8 - shift ) );
      }

      /// <summary>
      /// Given amount of data and page size, calculates amount of pages the data will take.
      /// </summary>
      /// <param name="totalSize">The total size of the data.</param>
      /// <param name="pageSize">The size of a single page.</param>
      /// <returns>The amount of pages the data will take.</returns>
      /// <remarks>
      /// More specifically, this method will return <c>( <paramref name="totalSize" /> + <paramref name="pageSize" /> - 1 ) / <paramref name="pageSize" /></c>
      /// </remarks>
      public static Int32 AmountOfPagesTaken( Int32 totalSize, Int32 pageSize )
      {
         return ( totalSize + pageSize - 1 ) / pageSize;
      }

      /// <summary>
      /// Returns the log base 2 of a given <paramref name="value"/>.
      /// </summary>
      /// <param name="value">The value.</param>
      /// <returns>Log base 2 of <paramref name="value"/>.</returns>
      /// <remarks>
      /// The return value is also the position of the MSB set.
      /// The algorithm is from <see href="http://graphics.stanford.edu/~seander/bithacks.html#IntegerLogLookup"/> .
      /// </remarks>
      [CLSCompliant( false )]
      public static Int32 Log2( UInt32 value )
      {
         UInt32 tt;

         if ( ( tt = value >> 24 ) != 0u )
         {
            return 24 + LOG_TABLE_256[tt];
         }
         else if ( ( tt = value >> 16 ) != 0u )
         {
            return 16 + LOG_TABLE_256[tt];
         }
         else if ( ( tt = value >> 8 ) != 0u )
         {
            return 8 + LOG_TABLE_256[tt];
         }
         else
         {
            return LOG_TABLE_256[value];
         }
      }

      /// <summary>
      /// Returns the log base 2 of a given <paramref name="value"/>.
      /// </summary>
      /// <param name="value">The value.</param>
      /// <returns>Log base 2 of <paramref name="value"/>.</returns>
      /// <remarks>
      /// The return value is also the position of the MSB set.
      /// The algorithm uses <see cref="Log2(UInt32)"/> method to calculate return value.
      /// </remarks>
      [CLSCompliant( false )]
      public static Int32 Log2( UInt64 value )
      {
         var highest = Log2( (UInt32) ( value >> sizeof( UInt32 ) ) );
         if ( highest == 0 )
         {
            highest = Log2( (UInt32) value );
         }
         else
         {
            highest += 32;
         }
         return highest;
      }

      /// <summary>
      /// Computes greatest common denominator without recursion.
      /// </summary>
      /// <param name="x">The first number.</param>
      /// <param name="y">The second number.</param>
      /// <returns>The greatest common denominator of <paramref name="x"/> and <paramref name="y"/>.</returns>
      public static Int32 GCD( Int32 x, Int32 y )
      {
         // Unwind recursion
         while ( y != 0 )
         {
            var tmp = y;
            y = x % y;
            x = tmp;
         }

         return x;
      }

      /// <summary>
      /// Computes greatest common denominator without recursion, for <see cref="Int64"/>.
      /// </summary>
      /// <param name="x">The first number.</param>
      /// <param name="y">The second number.</param>
      /// <returns>The greatest common denominator of <paramref name="x"/> and <paramref name="y"/>.</returns>
      public static Int64 GCD64( Int64 x, Int64 y )
      {
         // Unwind recursion
         while ( y != 0 )
         {
            var tmp = y;
            y = x % y;
            x = tmp;
         }

         return x;
      }

      /// <summary>
      /// Computes least common multiplier (by using greatest common denominator).
      /// </summary>
      /// <param name="x">The first number.</param>
      /// <param name="y">The second number.</param>
      /// <returns>The least common multiplier of <paramref name="x"/> and <paramref name="y"/>.</returns>
      public static Int32 LCM( Int32 x, Int32 y )
      {
         return ( x * y ) / GCD( x, y );
      }

      /// <summary>
      /// Computes least common multiplier (by using greatest common denominator), for <see cref="Int64"/>.
      /// </summary>
      /// <param name="x">The first number.</param>
      /// <param name="y">The second number.</param>
      /// <returns>The least common multiplier of <paramref name="x"/> and <paramref name="y"/>.</returns>
      public static Int64 LCM64( Int64 x, Int64 y )
      {
         return ( x * y ) / GCD64( x, y );
      }

      /// <summary>
      /// Returns greatest power of 2 less than or equal to given number.
      /// </summary>
      /// <param name="x">The number.</param>
      /// <returns>The greatest power of 2 less than or equal to <paramref name="x"/>.</returns>
      [CLSCompliant( false )]
      public static UInt32 FLP2( UInt32 x )
      {
         return ( 1u << Log2( x ) );
      }

      /// <summary>
      /// Returns greatest power of 2 less than or equal to given 64-bit number.
      /// </summary>
      /// <param name="x">The number.</param>
      /// <returns>The greatest power of 2 less than or equal to <paramref name="x"/>.</returns>
      [CLSCompliant( false )]
      public static UInt64 FLP264( UInt64 x )
      {
         return ( 1u << Log2( x ) );
      }

      /// <summary>
      /// Returns least power of 2 greater than or equal to given number.
      /// </summary>
      /// <param name="x">The number.</param>
      /// <returns>The least power of 2 greater than or equal to <paramref name="x"/>.</returns>
      [CLSCompliant( false )]
      public static UInt32 CLP2( UInt32 x )
      {
         return x == 0 ? 0 : ( 1u << ( 1 + Log2( x - 1 ) ) );
      }

      /// <summary>
      /// Returns least power of 2 greater than or equal to given 64-bit number.
      /// </summary>
      /// <param name="x">The number.</param>
      /// <returns>The least power of 2 greater than or equal to <paramref name="x"/>.</returns>
      [CLSCompliant( false )]
      public static UInt64 CLP264( UInt64 x )
      {
         return x == 0 ? 0 : ( 1ul << ( 1 + Log2( x - 1 ) ) );
      }

      /// <summary>
      /// Rounds given value up to next alignment, which should be a power of two.
      /// </summary>
      /// <param name="value">The value.</param>
      /// <param name="multiple">The alignment.</param>
      /// <returns>Value rounded up to next alignment.</returns>
      /// <remarks>
      /// Will return incorrect results if <paramref name="multiple"/> is zero.
      /// </remarks>
      public static Int32 RoundUpI32( Int32 value, Int32 multiple )
      {
         return ( multiple - 1 + value ) & ~( multiple - 1 );
      }

      /// <summary>
      /// Rounds given value up to next alignment, which should be a power of two.
      /// </summary>
      /// <param name="value">The value.</param>
      /// <param name="multiple">The alignment.</param>
      /// <returns>Value rounded up to next alignment.</returns>
      /// <remarks>
      /// Will return incorrect results if <paramref name="multiple"/> is zero.
      /// </remarks>
      [CLSCompliant( false )]
      public static UInt32 RoundUpU32( UInt32 value, UInt32 multiple )
      {
         return ( multiple - 1 + value ) & ~( multiple - 1 );
      }

      /// <summary>
      /// Rounds given value up to next alignment, which should be a power of two.
      /// </summary>
      /// <param name="value">The value.</param>
      /// <param name="multiple">The alignment.</param>
      /// <returns>Value rounded up to next alignment.</returns>
      /// <remarks>
      /// Will return incorrect results if <paramref name="multiple"/> is zero.
      /// </remarks>
      public static Int64 RoundUpI64( Int64 value, Int32 multiple )
      {
         return ( multiple - 1 + value ) & ~( multiple - 1 );
      }

      /// <summary>
      /// Rounds given value up to next alignment, which should be a power of two.
      /// </summary>
      /// <param name="value">The value.</param>
      /// <param name="multiple">The alignment.</param>
      /// <returns>Value rounded up to next alignment.</returns>
      /// <remarks>
      /// Will return incorrect results if <paramref name="multiple"/> is zero.
      /// </remarks>
      [CLSCompliant( false )]
      public static UInt64 RoundUpU64( UInt64 value, UInt32 multiple )
      {
         return ( multiple - 1 + value ) & ~( multiple - 1 );
      }

      /// <summary>
      /// This method counts how many bits are set in a given value.
      /// </summary>
      /// <param name="value">The value to count bits set.</param>
      /// <returns>How many bits are set in a given value.</returns>
      /// <remarks>
      /// This algorithm is from <see href="https://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel"/>.
      /// </remarks>
      public static Int32 CountBitsSetI32( Int32 value )
      {
         return (Int32) CountBitsSetU32( (UInt32) value );
      }

      /// <summary>
      /// This method counts how many bits are set in a given value.
      /// </summary>
      /// <param name="value">The value to count bits set.</param>
      /// <returns>How many bits are set in a given value.</returns>
      /// <remarks>
      /// This algorithm is from <see href="https://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel"/>.
      /// </remarks>
      [CLSCompliant( false )]
      public static UInt32 CountBitsSetU32( UInt32 value )
      {
         value = value - ( ( value >> 1 ) & 0x55555555u );
         value = ( value & 0x33333333u ) + ( ( value >> 2 ) & 0x33333333u );
         return ( ( value + ( value >> 4 ) & 0x0F0F0F0Fu ) * 0x01010101u ) >> 24;
      }

      /// <summary>
      /// This method counts how many bits are set in a given value.
      /// </summary>
      /// <param name="value">The value to count bits set.</param>
      /// <returns>How many bits are set in a given value.</returns>
      /// <remarks>
      /// This algorithm is from <see href="https://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel"/>.
      /// </remarks>
      public static Int32 CountBitsSetI64( Int64 value )
      {
         return (Int32) CountBitsSetU64( (UInt64) value );
      }

      /// <summary>
      /// This method counts how many bits are set in a given value.
      /// </summary>
      /// <param name="value">The value to count bits set.</param>
      /// <returns>How many bits are set in a given value.</returns>
      /// <remarks>
      /// This algorithm is from <see href="https://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel"/>.
      /// </remarks>
      [CLSCompliant( false )]
      public static UInt32 CountBitsSetU64( UInt64 value )
      {
         value = value - ( ( value >> 1 ) & 0x5555555555555555UL );
         value = ( value & 0x3333333333333333UL ) + ( ( value >> 2 ) & 0x3333333333333333UL );
         return (UInt32) ( ( ( value + ( value >> 4 ) & 0x0F0F0F0F0F0F0F0FUL ) * 0x0101010101010101UL ) >> 56 );
      }
   }
}
