/*
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
         for ( var i = 0; i < 256; ++i )
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
   }
}
