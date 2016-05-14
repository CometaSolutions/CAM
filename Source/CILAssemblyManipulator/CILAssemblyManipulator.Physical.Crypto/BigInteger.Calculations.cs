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
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.Crypto
{
#pragma warning disable 1591
   public partial struct BigInteger
#pragma warning restore 1591
   {
      // This method does x = x - y, assuming that x >= y, storing result in-place for x
      private static UInt32[] Subtract( UInt32[] xBits, UInt32[] yBits )
      {
         var borrow = 0u;
         var xIdx = 0;
         for ( var yIdx = 0; yIdx < yBits.Length; ++xIdx, ++yIdx )
         {
            borrow = SubtractWithBorrow( ref xBits[xIdx], yBits[yIdx], borrow );
            System.Diagnostics.Debug.Assert( borrow <= 1, "Borrow must be within legal range." );
         }

         if ( borrow != 0 )
         {
            // Need to apply the borrow to the rest of integers
            while ( xIdx < xBits.Length && unchecked(--xBits[xIdx++]) == 0 ) ;
         }

         return xBits;
      }

      // This method adds xBits to yBits, but not in-place.
      private static UInt32[] Add( UInt32[] xBits, UInt32[] yBits, out Boolean checkReturnValueTrailingZeroes )
      {
         UInt32[] greater, smaller;
         if ( xBits.Length < yBits.Length )
         {
            greater = yBits;
            smaller = xBits;
         }
         else
         {
            greater = xBits;
            smaller = yBits;
         }

         // We don't allow trailing zeroes, so check if we really need to allocate extra integer.
         var max = UInt32.MaxValue;
         if ( xBits.Length == yBits.Length )
         {
            max -= smaller[0];
         }

         checkReturnValueTrailingZeroes = greater[greater.Length - 1] >= max;

         // Because of BE format, we would rather allocate new array right here.
         UInt32[] greaterCopy;
         if ( checkReturnValueTrailingZeroes )
         {
            // We *may* overflow
            greaterCopy = new UInt32[greater.Length + 1];
            greater.CopyTo( greaterCopy, 0 );
         }
         else
         {
            // We definetly will *not* overflow
            greaterCopy = greater.CreateArrayCopy();
         }

         return Add( greaterCopy, smaller );
      }

      // This method does x + y in-place for x. It assumes that x >= y and will have enough room.
      private static UInt32[] Add( UInt32[] xBits, UInt32[] yBits )
      {
         var carry = 0u;
         var xIdx = 0;
         for ( var yIdx = 0; yIdx < yBits.Length; ++xIdx, ++yIdx )
         {
            carry = AddWithCarry( ref xBits[xIdx], yBits[yIdx], carry );
            System.Diagnostics.Debug.Assert( carry <= 1, "Borrow must be within legal range." );
         }

         if ( carry != 0 )
         {
            // Need to apply the carry to the rest of the integers
            while ( xIdx < xBits.Length && unchecked(++xBits[xIdx++]) == 0 ) ;
         }
         return xBits;
      }

      private static UInt32[] Multiply_SmallOrBig( UInt32[] left, UInt32[] right )
      {
         UInt32[] result = null;
         var resultLength = -1;
         Multiply_SmallOrBig( left, left.Length, right, right.Length, ref result, ref resultLength );
         return result;
      }

      private static void Multiply_SmallOrBig(
         UInt32[] left,
         Int32 leftLength,
         UInt32[] right,
         Int32 rightLength,
         ref UInt32[] result,
         ref Int32 resultLength
         )
      {
         if ( AreSmallBits( left ) )
         {
            var small = left[0];
            switch ( small )
            {
               case 0:
                  // Result is always zero
                  SetSmall( ref result, ref resultLength, 0 );
                  break;
               case 1:
                  // Result is always same as value
                  SetAsCopy( ref result, ref resultLength, right, rightLength );
                  break;
               default:
                  // Perform small multiply
                  Multiply_Small( right, rightLength, small, ref result, ref resultLength );
                  break;
            }
         }
         else if ( AreSmallBits( right ) )
         {
            var small = right[0];
            switch ( small )
            {
               case 0:
                  // Result is always zero
                  SetSmall( ref result, ref resultLength, 0 );
                  break;
               case 1:
                  // Result is always same as value
                  SetAsCopy( ref result, ref resultLength, left, leftLength );
                  break;
               default:
                  Multiply_Small( left, leftLength, small, ref result, ref resultLength );
                  break;
            }
         }
         else
         {
            Multiply( left, leftLength, right, rightLength, ref result, ref resultLength );
         }
      }

      // This method computes result = big * small. The 'big' array is unmodified.
      private static void Multiply_Small( UInt32[] big, Int32 bigLength, UInt32 small, ref UInt32[] result, ref Int32 resultLength )
      {
         // First check that result is big enough
         resultLength = ResizeBits( ref result, resultLength, bigLength );

         // Do multiplication
         var carry = 0u;
         for ( var i = 0; i < bigLength; ++i )
         {
            carry = MultiplyWithCarry( ref result[i], big[i], small, carry );
         }

         // Apply carry, and resize if necessary
         if ( carry != 0 )
         {
            resultLength = ResizeBits( ref result, resultLength, resultLength + 1 );
            result[resultLength - 1] = carry;
         }
      }

      // This method computes x * y and returns resulting array
      // The resulting array may have leading zeroes
      private static void Multiply( UInt32[] x, Int32 xLength, UInt32[] y, Int32 yLength, ref UInt32[] result, ref Int32 resultLength )
      {
         resultLength = ResizeBits( ref result, resultLength, xLength + yLength );
         // Clear bits since we are reading from result as well
         Array.Clear( result, 0, resultLength );
         for ( var i = 0; i < xLength; ++i )
         {
            var cur = x[i];
            if ( cur > 0 )
            {
               var carry = 0u;
               var retValIdx = i;
               for ( var j = 0; j < yLength; ++j )
               {
                  carry = MultiplyAndAddWithCarry( ref result[retValIdx++], cur, y[j], carry );
               }
               // Propagate carry
               while ( carry > 0 )
               {
                  carry = AddWithCarry( ref result[retValIdx++], 0, carry );
               }
            }
         }

         MinimizeBitsLength( result, ref resultLength );
      }


      // Computes divident = divident / divisor, when divisor is UInt32.
      // Does not check for zeroes or ones on each of those
      // Returns modulus
      private static UInt32 DivideWithRemainder_Small( UInt32[] divident, UInt32 divisor, Boolean assignToDivident )
      {
         var dividentLength = divident.Length;
         return DivideWithRemainder_Small( divident, ref dividentLength, divisor, assignToDivident );
      }

      private static UInt32 DivideWithRemainder_Small( UInt32[] divident, ref Int32 dividentLength, UInt32 divisor, Boolean assignToDivident )
      {
         var retVal = 0u;
         for ( var i = dividentLength - 1; i >= 0; --i )
         {
            var cur = ToUInt64( retVal, divident[i] );
            if ( assignToDivident )
            {
               divident[i] = unchecked((UInt32) ( cur / divisor ));
            }
            retVal = (UInt32) ( cur % divisor );
         }

         MinimizeBitsLength( divident, ref dividentLength );
         return retVal;
      }

      // Calculates divident = divident % divisor (in place), and returns quotient, if specified
      // Assumes the length of divisor is at least 2
      private static UInt32[] DivideWithRemainder( UInt32[] divident, UInt32[] divisor, Boolean returnQuotient )
      {
         var dividentLength = divident.Length;
         var retVal = DivideWithRemainder( divident, ref dividentLength, divisor, divisor.Length, returnQuotient );
         // Remember trailing zeroes for divident
         for ( var i = dividentLength; i < divident.Length; ++i )
         {
            divident[i] = 0;
         }
         return retVal;
      }

      private static UInt32[] DivideWithRemainder( UInt32[] divident, ref Int32 dividentLength, UInt32[] divisor, Int32 divisorLength, Boolean returnQuotient )
      {
         UInt32[] retVal;
         if ( dividentLength < divisorLength ) // Divident is definetly smaller than divisor, so the modulus is divident, and quotient is zero
         {
            retVal = null;
         }
         else
         {
            // Find out quotient size. We can't use Compare(UInt32[], Int32, UInt32[], Int32) here, since we want to iterate the array even the size differs.
            var quotientSize = dividentLength - divisorLength;
            for ( var i = dividentLength - 1; i >= 0; --i )
            {
               UInt32 x, y;
               if ( i < quotientSize )
               {
                  // We've gone all this way with the values being the same - increment the size
                  ++quotientSize;
                  break;
               }
               else if ( ( x = divident[i - quotientSize] ) != ( y = divisor[i - quotientSize] ) )
               {
                  // We've encountered first value that differs
                  if ( x > y )
                  {
                     // Divident is greater than divisor
                     ++quotientSize;
                  }
                  break;
               }
            }

            // If quotient size at this point is zero, it means that divident had same amount of integers as divisor, but divident < divisor.
            if ( quotientSize == 0 )
            {
               retVal = null;
            }
            else
            {
               retVal = returnQuotient ? new UInt32[quotientSize] : null;
               // Get the highest 32 bits of divisor (x2) to use for the trial divisions
               var divisorHigh = divisor[divisorLength - 1];
               var divisorLow = divisor[divisorLength - 2];
               var shiftLeft = CountHighZeroes( divisorHigh );
               var shiftRight = BITS_32 - shiftLeft;
               if ( shiftLeft > 0 )
               {
                  divisorHigh = ( divisorHigh << shiftLeft ) | ( divisorLow >> shiftRight );
                  divisorLow <<= shiftRight;
                  if ( divisorLength > 2 )
                  {
                     divisorLow |= divisor[divisorLength - 3] >> shiftRight;
                  }
               }

               // Populate modulus (divident) and quotient
               unchecked
               {
                  for ( Int32 i = quotientSize - 1, dividentMax = dividentLength - 1; i >= 0; --i )
                  {
                     // Get the high 32 bits of divident (x2)
                     var dividentIdx = i + divisorLength;
                     var high = ( dividentIdx <= dividentMax ) ? divident[dividentIdx] : 0;
                     var u64 = ToUInt64( high, divident[dividentIdx - 1] );
                     var next = divident[dividentIdx - 2];
                     if ( shiftLeft > 0 )
                     {
                        u64 = ( u64 << shiftLeft ) | ( next >> shiftRight );
                        next <<= shiftRight;
                        if ( dividentIdx >= 3 )
                        {
                           next |= divident[dividentIdx - 3] >> shiftRight;
                        }
                     }

                     // Perform division
                     var quotient = u64 / divisorHigh;
                     UInt64 remainder = (UInt32) ( u64 % divisorHigh );
                     if ( quotient > UInt32.MaxValue )
                     {
                        remainder += divisorHigh * ( quotient - UInt32.MaxValue );
                        quotient = UInt32.MaxValue;
                     }
                     while ( remainder <= UInt32.MaxValue && quotient * divisorLow > ToUInt64( (UInt32) remainder, next ) )
                     {
                        --quotient;
                        remainder += divisorHigh;
                     }

                     // Multiply and subtract. The quotient may be 1 too large - if there is a need to borrow, then we add the divisor and decremant the quotient.
                     if ( quotient > 0 )
                     {
                        var borrow = 0UL;
                        for ( var j = 0; j < divisorLength; ++j )
                        {
                           borrow += divisor[j] * quotient;
                           var borrowLow = (UInt32) borrow;
                           borrow >>= BITS_32;
                           var dividentIndex = i + j;
                           if ( divident[dividentIndex] < borrowLow )
                           {
                              ++borrow;
                           }
                           divident[dividentIndex] -= borrowLow;
                        }

                        if ( high < borrow )
                        {
                           // Add with carry
                           var carry = 0u;
                           for ( var j = 0; j < divisorLength; ++j )
                           {
                              carry = AddWithCarry( ref divident[i + j], divisor[j], carry );
                              System.Diagnostics.Debug.Assert( carry <= 1 );
                           }
                           System.Diagnostics.Debug.Assert( carry == 1 );
                           --quotient;
                        }

                        dividentMax = dividentIdx - 1;
                     }

                     if ( returnQuotient )
                     {
                        retVal[i] = (UInt32) quotient;
                     }
                  }
               }
               dividentLength = divisorLength;
               MinimizeBitsLength( divident, ref dividentLength );

            }
         }



         return retVal;
      }

      private static Int32 CountHighZeroes( UInt32 x )
      {
         return x == 0 ? BITS_32 : ( BITS_32 - BinaryUtils.Log2( x ) - 1 );
      }

      private static UInt32 SubtractWithBorrow( ref UInt32 first, UInt32 second, UInt32 borrow )
      {
         unchecked
         {
            // Do subtraction using UInt64
            var cur = (UInt64) first - second - borrow;
            first = (UInt32) cur;
            // If we got underflow, the high 32 bits of 'cur' will be FFFFFFFF. Extract them via shift, cast to Int32 (it will become -1), then negate and cast back to UInt32 to obtain 1.
            // If no underflow, this should be 0.
            return (UInt32) ( -( (Int32) ( cur >> BITS_32 ) ) );
         }
      }

      private static UInt32 AddWithCarry( ref UInt32 first, UInt32 second, UInt32 carry )
      {
         unchecked
         {
            // Do addition using UInt64
            var result = (UInt64) first + second + carry;
            first = (UInt32) result;
            // If we got overflow, the carry will have only its 33th bit set.
            return (UInt32) ( result >> BITS_32 );
         }
      }

      private static UInt32 MultiplyWithCarry( ref UInt32 store, UInt32 first, UInt32 second, UInt32 carry )
      {
         var result = (UInt64) first * second + carry;
         unchecked
         {
            store = (UInt32) result;
            // If we got overflow, carry will have its 33th -> bits set
            return (UInt32) ( result >> BITS_32 );
         }
      }

      private static UInt32 MultiplyAndAddWithCarry( ref UInt32 value, UInt32 first, UInt32 second, UInt32 carry )
      {
         var result = (UInt64) first * second + value + carry;
         unchecked
         {
            value = (UInt32) result;
            return (UInt32) ( result >> BITS_32 );
         }
      }

      // TODO refactor all the parameters into single ModPowCalculationState object.
      private static void ModPow_Small(
         ref UInt32[] value,
         ref Int32 valueLength,
         UInt32[] modulus,
         ref UInt32[] result,
         ref Int32 resultLength,
         UInt32 exponent,
         ref UInt32[] tmp,
         ref Int32 tmpLength
         )
      {
         while ( exponent != 0 )
         {
            if ( !exponent.IsEven() )
            {
               // Perform extra step for odd exponent
               ModPow_Step( result, resultLength, value, valueLength, modulus, ref result, ref resultLength, ref tmp, ref tmpLength );
            }

            if ( exponent == 1 )
            {
               // Exit early - no point calculating the x^1
               break;
            }

            // Perform step
            ModPow_Step( value, valueLength, value, valueLength, modulus, ref value, ref valueLength, ref tmp, ref tmpLength );

            // Shift right once
            exponent >>= 1;
         }
      }

      private static void ModPow_Small_32(
         ref UInt32[] value,
         ref Int32 valueLength,
         UInt32[] modulus,
         ref UInt32[] result,
         ref Int32 resultLength,
         UInt32 exponent,
         ref UInt32[] tmp,
         ref Int32 tmpLength
         )
      {
         // Same as ModPow_Small except that we are always iterating 32 times
         for ( var i = 0; i < BITS_32; ++i )
         {
            if ( !exponent.IsEven() )
            {
               ModPow_Step( result, resultLength, value, valueLength, modulus, ref result, ref resultLength, ref tmp, ref tmpLength );
            }
            ModPow_Step( value, valueLength, value, valueLength, modulus, ref value, ref valueLength, ref tmp, ref tmpLength );

            exponent >>= 1;
         }
      }

      private static void ModPow(
         UInt32[] value,
         UInt32[] modulus,
         ref UInt32[] result,
         ref Int32 resultLength,
         UInt32[] exponent,
         UInt32[] tmp,
         Int32 tmpLength
         )
      {
         // Iterate all except for last integer
         var valueLength = value.Length;
         for ( var i = 0; i < exponent.Length - 1; ++i )
         {
            ModPow_Small_32( ref value, ref valueLength, modulus, ref result, ref resultLength, exponent[i], ref tmp, ref tmpLength );
         }

         // Last step
         ModPow_Small( ref value, ref valueLength, modulus, ref result, ref resultLength, exponent[exponent.Length - 1], ref tmp, ref tmpLength );
      }

      private static void ModPow_Step(
         UInt32[] x,
         Int32 xLength,
         UInt32[] y,
         Int32 yLength,
         UInt32[] modulus,
         ref UInt32[] result,
         ref Int32 resultLength,
         ref UInt32[] tmp,
         ref Int32 tmpLength
         )
      {
         // result = ( x * y ) % modulus
         Multiply_SmallOrBig( x, xLength, y, yLength, ref tmp, ref tmpLength );
         if ( modulus.Length > 1 )
         {
            // Big modulus.
            DivideWithRemainder( tmp, ref tmpLength, modulus, modulus.Length, false );
         }
         else
         {
            // Small modulus
            result[0] = DivideWithRemainder_Small( tmp, ref tmpLength, modulus[0], false );
         }

         var swap = tmp;
         tmp = result;
         result = swap;

         var swap2 = tmpLength;
         tmpLength = resultLength;
         resultLength = swap2;
      }

      private static Int32 ResizeBits( ref UInt32[] bits, Int32 curLength, Int32 newLength )
      {
         if ( bits == null )
         {
            bits = new UInt32[newLength];
         }
         else if ( bits.Length < newLength )
         {
            var tmp = bits;
            bits = new UInt32[newLength];
            // Don't copy *all* elements of the array, since there may be garbage at the end
            Array.Copy( tmp, 0, bits, 0, curLength );
         }
         return newLength;
      }

      private static void MinimizeBitsLength( UInt32[] bits, ref Int32 length )
      {
         while ( length > 0 && bits[length - 1] == 0 )
         {
            --length;
         }
      }

      private static void SetSmall( ref UInt32[] result, ref Int32 resultLength, UInt32 smallValue )
      {
         resultLength = ResizeBits( ref result, resultLength, 1 );
         result[0] = smallValue;
      }

      private static void SetAsCopy( ref UInt32[] result, ref Int32 resultLength, UInt32[] value, Int32 valueLength )
      {
         resultLength = ResizeBits( ref result, resultLength, valueLength );
         Array.Copy( value, result, valueLength );
      }

   }
}
