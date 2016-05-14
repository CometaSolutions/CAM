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
      private static UInt32[] Add( UInt32[] xBits, UInt32[] yBits, out Boolean checkReturnValueLeadingZeroes )
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

         // We don't allow leading zeroes, so check if we really need to allocate extra integer.
         var max = UInt32.MaxValue;
         if ( xBits.Length == yBits.Length )
         {
            max -= smaller[smaller.Length - 1];
         }

         checkReturnValueLeadingZeroes = greater[0] >= max;

         // Because of BE format, we would rather allocate new array right here.
         UInt32[] greaterCopy;
         if ( checkReturnValueLeadingZeroes )
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
         if ( AreSmallBits( left ) )
         {
            return Multiply_Small( right.CreateArrayCopy(), left[0] );
         }
         else if ( AreSmallBits( right ) )
         {
            return Multiply_Small( left.CreateArrayCopy(), right[0] );
         }
         else
         {
            return Multiply( left, right );
         }
      }

      // This method computes big = big * y in-place for big.
      // It resizes array if necessary, and returns it
      private static UInt32[] Multiply_Small( UInt32[] big, UInt32 small )
      {
         var carry = 0u;
         for ( var i = 0; i < big.Length; ++i )
         {
            carry = MultiplyWithCarry( ref big[i], small, carry );
         }

         if ( carry != 0 )
         {
            var tmp = big;
            big = new UInt32[big.Length + 1];
            tmp.CopyTo( big, 0 );
            big[big.Length - 1] = carry;
         }

         return big;
      }

      // This method computes x * y and returns resulting array
      // The resulting array may have leading zeroes
      private static UInt32[] Multiply( UInt32[] x, UInt32[] y )
      {
         var retVal = new UInt32[x.Length + y.Length];
         for ( var i = 0; i < x.Length; ++i )
         {
            var cur = x[i];
            if ( cur > 0 )
            {
               var carry = 0u;
               var retValIdx = i;
               for ( var j = 0; j < y.Length; ++j )
               {
                  carry = MultiplyAndAddWithCarry( ref retVal[retValIdx++], cur, y[j], carry );
               }
               // Propagate carry
               while ( carry > 0 )
               {
                  carry = AddWithCarry( ref retVal[retValIdx++], 0, carry );
               }
            }
         }

         return retVal;
      }


      // Computes divident = divident / divisor, when divisor is UInt32.
      // Does not check for zeroes or ones on each of those
      // Returns modulus
      private static UInt32 DivideWithRemainder_Small( UInt32[] divident, UInt32 divisor, Boolean assignToDivident )
      {
         var retVal = 0u;
         for ( var i = divident.Length - 1; i >= 0; --i )
         {
            var cur = ToUInt64( retVal, divident[i] );
            if ( assignToDivident )
            {
               divident[i] = unchecked((UInt32) ( cur / divisor ));
            }
            retVal = (UInt32) ( cur % divisor );
         }

         return retVal;
      }

      // Calculates divident = divident % divisor (in place), and returns quotient, if specified
      // Assumes the length of divisor is at least 2
      private static UInt32[] DivideWithRemainder( UInt32[] divident, UInt32[] divisor, Boolean returnQuotient )
      {
         var dividentLength = divident.Length;
         var divisorLength = divisor.Length;
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

               // Remember trailing zeroes for divident
               for ( var i = divisorLength; i < dividentLength; ++i )
               {
                  divident[i] = 0;
               }
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

      private static UInt32 MultiplyWithCarry( ref UInt32 first, UInt32 second, UInt32 carry )
      {
         var result = (UInt64) first * second + carry;
         unchecked
         {
            first = (UInt32) result;
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

      private static void ModPow_Small( ref UInt32[] value, UInt32[] modulus, ref UInt32[] result, UInt32 exponent )
      {
         while ( exponent != 0 )
         {
            if ( !exponent.IsEven() )
            {
               // Perform extra step for odd exponent
               ModPow_Step( result, value, modulus, ref result );
            }

            if ( exponent == 1 )
            {
               // Exit early - no point calculating the x^1
               break;
            }

            // Perform step
            ModPow_Step( value, value, modulus, ref value );

            // Shift right once
            exponent >>= 1;
         }
      }

      private static void ModPow_Small_32( ref UInt32[] value, UInt32[] modulus, ref UInt32[] result, UInt32 exponent )
      {
         // Same as ModPow_Small except that we are always iterating 32 times
         for ( var i = 0; i < BITS_32; ++i )
         {
            if ( !exponent.IsEven() )
            {
               ModPow_Step( result, value, modulus, ref result );
            }
            ModPow_Step( value, value, modulus, ref value );

            exponent >>= 1;
         }
      }

      private static void ModPow( UInt32[] value, UInt32[] modulus, ref UInt32[] result, UInt32[] exponent )
      {
         // Iterate all except for last byte
         for ( var i = 0; i < exponent.Length - 1; ++i )
         {
            ModPow_Small_32( ref value, modulus, ref result, exponent[i] );
         }

         // Last step
         ModPow_Small( ref value, modulus, ref result, exponent[exponent.Length - 1] );
      }

      private static void ModPow_Step( UInt32[] x, UInt32[] y, UInt32[] modulus, ref UInt32[] result )
      {
         // result = ( x * y ) % modulus
         result = Multiply_SmallOrBig( x, y ); // TODO this is not the most efficient thing to do right now, as x or y will be copied (we could use temp array here to avoid extra allocations)
         if ( modulus.Length > 1 )
         {
            // Big modulus.
            // TODO this is also not the most efficient thing to do right now...
            if ( result[result.Length - 1] == 0 )
            {
               result = new BigInteger( 1, result, true )._bits;
            }
            DivideWithRemainder( result, modulus, false );
         }
         else
         {
            // Small modulus
            result[0] = DivideWithRemainder_Small( result, modulus[0], false );
            // Clear array for trailing zeroes
            Array.Clear( result, 1, result.Length - 1 );
         }

         if ( result[result.Length - 1] == 0 )
         {
            result = new BigInteger( 1, result, true )._bits;
         }
      }

   }
}
