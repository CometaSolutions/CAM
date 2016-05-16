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
using CommonUtils.Numerics;
using CommonUtils.Numerics.Calculations;

namespace CommonUtils.Numerics.Calculations
{
#pragma warning disable 1591
   [CLSCompliant( false )]
   public static class BigIntegerCalculations
   {
      // For convenience, zero can be represented as one-element array of 0.



      public static BigInteger CreateBigInteger( Int32 sign, UInt32[] bits, Int32 bitsLength )
      {
         return new BigInteger( sign, bits, bitsLength );
      }

      public static Boolean TryGetSmallValue( this BigInteger value, out UInt32 smallValue )
      {
         var retVal = value.IsSmall;
         smallValue = retVal ? value.SmallValue : 0u;
         return retVal;
      }

      [CLSCompliant( false )]
      public static IEnumerable<UInt32> IterateInLittleEndianOrder( this BigInteger value )
      {
         var array = value.GetArrayDirect();
         for ( var i = 0; i < array.Length; ++i )
         {
            yield return array[i];
         }
      }

      [CLSCompliant( false )]
      public static IEnumerable<UInt32> IterateInBigEndianOrder( this BigInteger value )
      {
         var array = value.GetArrayDirect();
         for ( var i = array.Length - 1; i >= 0; --i )
         {
            yield return array[i];
         }
      }

      public static Int32 GetUInt32ArrayLength( this BigInteger value )
      {
         return value.GetArrayDirect().Length;
      }

      public static void Subtract(
         BigInteger left,
         UInt32[] right,
         Int32 rightLength,
         Int32 rightSign,
         ref UInt32[] result,
         ref Int32 resultLength,
         out Int32 resultSign
         )
      {
         var leftArray = left.GetArrayDirect();
         Subtract( leftArray, leftArray.Length, left.Sign, right, rightLength, rightSign, ref result, ref resultLength, out resultSign );
      }

      public static void Subtract(
         UInt32[] left,
         Int32 leftLength,
         Int32 leftSign,
         BigInteger right,
         ref UInt32[] result,
         ref Int32 resultLength,
         out Int32 resultSign
         )
      {
         var rightArray = right.GetArrayDirect();
         Subtract( left, leftLength, leftSign, rightArray, rightArray.Length, right.Sign, ref result, ref resultLength, out resultSign );
      }

      public static void Subtract(
         BigInteger left,
         BigInteger right,
         ref UInt32[] result,
         ref Int32 resultLength,
         out Int32 resultSign
         )
      {
         var leftArray = left.GetArrayDirect();
         var rightArray = right.GetArrayDirect();
         Subtract( leftArray, leftArray.Length, left.Sign, rightArray, rightArray.Length, right.Sign, ref result, ref resultLength, out resultSign );
      }

      public static void Subtract(
         UInt32[] left,
         Int32 leftLength,
         Int32 leftSign,
         UInt32[] right,
         Int32 rightLength,
         Int32 rightSign,
         ref UInt32[] result,
         ref Int32 resultLength,
         out Int32 resultSign
         )
      {
         if ( rightLength <= 1 && GetSmall( right, rightLength ) == 0 )
         {
            // result is left
            CopyBits( left, leftLength, ref result, ref resultLength );
            resultSign = leftSign;
         }
         else if ( leftLength <= 1 && GetSmall( left, leftLength ) == 0 )
         {
            // result is right, but we underflowed
            CopyBits( right, rightLength, ref result, ref resultLength );
            resultSign = -rightSign;
         }
         else if ( leftSign == rightSign )
         {
            DoSubtract( left, leftLength, leftSign, right, rightLength, ref result, ref resultLength, out resultSign );
         }
         else
         {
            DoAdd( left, leftLength, right, rightLength, ref result, ref resultLength );
            resultSign = leftSign;
         }
      }

      private static void DoSubtract(
         UInt32[] left,
         Int32 leftLength,
         Int32 leftSign,
         UInt32[] right,
         Int32 rightLength,
         ref UInt32[] result,
         ref Int32 resultLength,
         out Int32 resultSign
         )
      {
         var compareResult = CompareBits( left, leftLength, right, rightLength );
         if ( compareResult == 0 )
         {
            // Result is zero
            resultLength = 0;
            resultSign = 0;
         }
         else
         {
            UInt32[] greater, smaller;
            Int32 greaterLength, smallerLength;
            if ( compareResult < 0 )
            {
               greater = right;
               greaterLength = rightLength;
               smaller = left;
               smallerLength = leftLength;
            }
            else
            {
               greater = left;
               greaterLength = leftLength;
               smaller = right;
               smallerLength = rightLength;
            }
            DoSubtract( greater, greaterLength, smaller, smallerLength, ref result, ref resultLength );
            resultSign = leftSign * compareResult;
         }
      }

      // This method does result = x - y, assuming that x >= y
      private static void DoSubtract(
         UInt32[] left,
         Int32 leftLength,
         UInt32[] right,
         Int32 rightLength,
         ref UInt32[] result,
         ref Int32 resultLength
         )
      {
         CopyBits( left, leftLength, ref result, ref resultLength );

         var borrow = 0u;
         var xIdx = 0;
         for ( var yIdx = 0; yIdx < rightLength; ++xIdx, ++yIdx )
         {
            borrow = SubtractWithBorrow( ref result[xIdx], left[xIdx], right[yIdx], borrow );
            System.Diagnostics.Debug.Assert( borrow <= 1, "Borrow must be within legal range." );
         }

         if ( borrow != 0 )
         {
            // Need to apply the borrow to the rest of integers
            while ( xIdx < leftLength && unchecked(--result[xIdx++]) == 0 ) ;
         }

         // Trim trailing zeroes
         MinimizeBitsLength( result, ref resultLength );
      }

      public static void Add(
         BigInteger left,
         UInt32[] right,
         Int32 rightLength,
         Int32 rightSign,
         ref UInt32[] result,
         ref Int32 resultLength,
         out Int32 resultSign
         )
      {
         var leftArray = left.GetArrayDirect();
         Add( leftArray, leftArray.Length, left.Sign, right, rightLength, rightSign, ref result, ref resultLength, out resultSign );
      }

      public static void Add(
         UInt32[] left,
         Int32 leftLength,
         Int32 leftSign,
         BigInteger right,
         ref UInt32[] result,
         ref Int32 resultLength,
         out Int32 resultSign
         )
      {
         var rightArray = right.GetArrayDirect();
         Add( left, leftLength, leftSign, rightArray, rightArray.Length, right.Sign, ref result, ref resultLength, out resultSign );
      }

      public static void Add(
         BigInteger left,
         BigInteger right,
         ref UInt32[] result,
         ref Int32 resultLength,
         out Int32 resultSign
         )
      {
         var leftArray = left.GetArrayDirect();
         var rightArray = right.GetArrayDirect();
         Add( leftArray, leftArray.Length, left.Sign, rightArray, rightArray.Length, right.Sign, ref result, ref resultLength, out resultSign );
      }

      public static void Add(
         UInt32[] left,
         Int32 leftLength,
         Int32 leftSign,
         UInt32[] right,
         Int32 rightLength,
         Int32 rightSign,
         ref UInt32[] result,
         ref Int32 resultLength,
         out Int32 resultSign
         )
      {
         if ( rightLength <= 1 && GetSmall( right, rightLength ) == 0 )
         {
            // result is left
            CopyBits( left, leftLength, ref result, ref resultLength );
            resultSign = leftSign;
         }
         else if ( leftLength <= 1 && GetSmall( left, leftLength ) == 0 )
         {
            // result is right
            CopyBits( right, rightLength, ref result, ref resultLength );
            resultSign = rightSign;
         }
         else if ( rightSign == leftSign )
         {
            DoAdd( left, leftLength, right, rightLength, ref result, ref resultLength );
            resultSign = rightSign;
         }
         else
         {
            DoSubtract( left, leftLength, leftSign, right, rightLength, ref result, ref resultLength, out resultSign );
         }
      }

      // This method does result = x + y 
      private static void DoAdd(
         UInt32[] left,
         Int32 leftLength,
         UInt32[] right,
         Int32 rightLength,
         ref UInt32[] result,
         ref Int32 resultLength
         )
      {
         // See which one is greater
         UInt32[] max, min;
         Int32 maxLength, minLength;
         if ( leftLength > rightLength )
         {
            max = left;
            min = right;
            maxLength = leftLength;
            minLength = rightLength;
         }
         else
         {
            max = right;
            min = left;
            maxLength = rightLength;
            minLength = leftLength;
         }

         // Prepare result
         resultLength = ResizeBits( ref result, resultLength, maxLength );

         // Perform addition
         var carry = 0u;
         var resultIdx = 0;
         for ( var minIdx = 0; minIdx < minLength; ++resultIdx, ++minIdx )
         {
            carry = AddWithCarry( ref result[resultIdx], max[resultIdx], min[minIdx], carry );
            System.Diagnostics.Debug.Assert( carry <= 1, "Carry must be within legal range." );
         }

         // Remember to copy whatever is needed from max to result
         if ( minLength < maxLength )
         {
            Array.Copy( max, minLength, result, minLength, maxLength - minLength );
         }

         // Propagate carry
         if ( carry != 0 )
         {
            do
            {
               if ( resultIdx >= resultLength )
               {
                  resultLength = ResizeBits( ref result, resultLength, resultLength + 1 );
                  result[resultIdx] = 1;
                  break;
               }
            } while ( unchecked(++result[resultIdx++]) == 0 );
         }
      }

      public static void Multiply(
         BigInteger left,
         UInt32[] right,
         Int32 rightLength,
         ref UInt32[] result,
         ref Int32 resultLength
         )
      {
         var leftArray = left.GetArrayDirect();
         Multiply( leftArray, leftArray.Length, right, rightLength, ref result, ref resultLength );
      }

      public static void Multiply(
         UInt32[] left,
         Int32 leftLength,
         BigInteger right,
         ref UInt32[] result,
         ref Int32 resultLength
         )
      {
         var rigthArray = right.GetArrayDirect();
         Multiply( left, leftLength, rigthArray, rigthArray.Length, ref result, ref resultLength );
      }

      public static void Multiply(
         BigInteger left,
         BigInteger right,
         ref UInt32[] result,
         ref Int32 resultLength
      )
      {
         var leftArray = left.GetArrayDirect();
         var rightArray = right.GetArrayDirect();
         Multiply( leftArray, leftArray.Length, rightArray, rightArray.Length, ref result, ref resultLength );
      }

      public static void Multiply(
         UInt32[] left,
         Int32 leftLength,
         UInt32[] right,
         Int32 rightLength,
         ref UInt32[] result,
         ref Int32 resultLength
         )
      {
         if ( leftLength <= 1 )
         {
            var small = GetSmall( left, leftLength );
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
         else if ( rightLength <= 1 )
         {
            var small = GetSmall( right, rightLength );
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
            Multiply_Big( left, leftLength, right, rightLength, ref result, ref resultLength );
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
      private static void Multiply_Big( UInt32[] x, Int32 xLength, UInt32[] y, Int32 yLength, ref UInt32[] result, ref Int32 resultLength )
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

      public static void Modulus(
         UInt32[] divident,
         ref Int32 dividentLength,
         BigInteger divisor
         )
      {
         var divisorArray = divisor.GetArrayDirect();
         Modulus( divident, ref dividentLength, divisorArray, divisorArray.Length );
      }

      // Computes divident = divident % divisor
      public static void Modulus(
         UInt32[] divident,
         ref Int32 dividentLength,
         UInt32[] divisor,
         Int32 divisorLength
         )
      {
         UInt32[] dummy = null;
         Int32 dummy2 = -1;
         DivideWithRemainder( divident, ref dividentLength, divisor, divisorLength, ref dummy, ref dummy2, false );
      }

      public static void DivideWithRemainder(
         UInt32[] divident,
         ref Int32 dividentLength,
         BigInteger divisor,
         ref UInt32[] quotient,
         ref Int32 quotientLength,
         Boolean computeQuotient
         )
      {
         var divisorArray = divisor.GetArrayDirect();
         DivideWithRemainder( divident, ref dividentLength, divisorArray, divisorArray.Length, ref quotient, ref quotientLength, computeQuotient );
      }

      // Computes quotient = divident / divisor, if so specified.
      // The modulus is stored in divident, always.
      public static void DivideWithRemainder(
         UInt32[] divident,
         ref Int32 dividentLength,
         UInt32[] divisor,
         Int32 divisorLength,
         ref UInt32[] quotient,
         ref Int32 quotientLength,
         Boolean computeQuotient
         )
      {
         if ( divisorLength <= 1 )
         {
            // Small divisor
            var small = GetSmall( divisor, divisorLength );

            switch ( small )
            {
               case 0:
                  throw new ArithmeticException( "Division by zero." );
               //case 1:
               //   // In this case, quotient is divident, and modulus is zero
               //   if ( computeQuotient )
               //   {
               //      quotientLength = ResizeBits( ref quotient, quotientLength, dividentLength );
               //      Array.Copy( divident, 0, quotient, 0, quotientLength );
               //   }
               //   dividentLength = 0;
               //   break;
               default:
                  UInt32 modulus;
                  if ( computeQuotient )
                  {
                     quotientLength = ResizeBits( ref quotient, quotientLength, dividentLength );
                     Array.Copy( divident, 0, quotient, 0, quotientLength );
                     if ( small > 1 )
                     {
                        modulus = DivideWithRemainder_Small( quotient, ref quotientLength, small, true );
                        MinimizeBitsLength( quotient, ref quotientLength );
                     }
                     else
                     {
                        modulus = 0;
                     }
                  }
                  else
                  {
                     modulus = DivideWithRemainder_Small( divident, ref dividentLength, small, false );
                  }
                  divident[0] = modulus;
                  dividentLength = 1;
                  break;
            }
         }
         else
         {
            // Both divisor and divident are of length 2 or more.
            DivideWithRemainder_Big( divident, ref dividentLength, divisor, divisorLength, ref quotient, ref quotientLength, computeQuotient );
         }
      }

      // Computes divident = divident / divisor, when divisor is UInt32.
      // Returns modulus
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
         if ( assignToDivident )
         {
            MinimizeBitsLength( divident, ref dividentLength );
         }

         return retVal;
      }

      // Calculates divident = divident % divisor (in place), and returns quotient, if specified
      // Assumes the length of divisor is at least 2
      private static void DivideWithRemainder_Big(
         UInt32[] divident,
         ref Int32 dividentLength,
         UInt32[] divisor,
         Int32 divisorLength,
         ref UInt32[] quotient,
         ref Int32 quotientLength,
         Boolean computeQuotient
         )
      {
         if ( dividentLength < divisorLength ) // Divident is definetly smaller than divisor, so the modulus is divident, and quotient is zero
         {
            if ( computeQuotient )
            {
               quotientLength = 0;
            }
         }
         else
         {
            // Find out quotient size. We can't use Compare(UInt32[], Int32, UInt32[], Int32) here, since we want to iterate the array even the size differs.
            var newQuotientLength = dividentLength - divisorLength;
            for ( var i = dividentLength - 1; i >= 0; --i )
            {
               UInt32 x, y;
               if ( i < newQuotientLength )
               {
                  // We've gone all this way with the values being the same - increment the size
                  ++newQuotientLength;
                  break;
               }
               else if ( ( x = divident[i - newQuotientLength] ) != ( y = divisor[i - newQuotientLength] ) )
               {
                  // We've encountered first value that differs
                  if ( x > y )
                  {
                     // Divident is greater than divisor
                     ++newQuotientLength;
                  }
                  break;
               }
            }

            // If quotient size at this point is zero, it means that divident had same amount of integers as divisor, but divident < divisor.
            if ( newQuotientLength == 0 )
            {
               if ( computeQuotient )
               {
                  quotientLength = 0;
               }
            }
            else
            {
               if ( computeQuotient )
               {
                  ResizeBits( ref quotient, quotientLength, newQuotientLength );
               }
               quotientLength = newQuotientLength;
               // Get the highest 32 bits of divisor (x2) to use for the trial divisions
               var divisorHigh = divisor[divisorLength - 1];
               var divisorLow = divisor[divisorLength - 2];
               var shiftLeft = CountHighZeroes( divisorHigh );
               var shiftRight = BigInteger.BITS_32 - shiftLeft;
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
                  for ( Int32 i = quotientLength - 1, dividentMax = dividentLength - 1; i >= 0; --i )
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
                     var q = u64 / divisorHigh;
                     UInt64 remainder = (UInt32) ( u64 % divisorHigh );
                     if ( q > UInt32.MaxValue )
                     {
                        remainder += divisorHigh * ( q - UInt32.MaxValue );
                        q = UInt32.MaxValue;
                     }
                     while ( remainder <= UInt32.MaxValue && q * divisorLow > ToUInt64( (UInt32) remainder, next ) )
                     {
                        --q;
                        remainder += divisorHigh;
                     }

                     // Multiply and subtract. The quotient may be 1 too large - if there is a need to borrow, then we add the divisor and decremant the quotient.
                     if ( q > 0 )
                     {
                        var borrow = 0UL;
                        for ( var j = 0; j < divisorLength; ++j )
                        {
                           borrow += divisor[j] * q;
                           var borrowLow = (UInt32) borrow;
                           borrow >>= BigInteger.BITS_32;
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
                           --q;
                        }

                        dividentMax = dividentIdx - 1;
                     }

                     if ( computeQuotient )
                     {
                        quotient[i] = (UInt32) q;
                     }
                  }
               }
               dividentLength = divisorLength;
               MinimizeBitsLength( divident, ref dividentLength );

               if ( computeQuotient )
               {
                  MinimizeBitsLength( quotient, ref quotientLength );
               }
            }
         }
      }

      private static Int32 CountHighZeroes( UInt32 x )
      {
         return x == 0 ? BigInteger.BITS_32 : ( BigInteger.BITS_32 - BinaryUtils.Log2( x ) - 1 );
      }

      private static UInt32 SubtractWithBorrow( ref UInt32 store, UInt32 first, UInt32 second, UInt32 borrow )
      {
         unchecked
         {
            // Do subtraction using UInt64
            var cur = (UInt64) first - second - borrow;
            store = (UInt32) cur;
            // If we got underflow, the high 32 bits of 'cur' will be FFFFFFFF. Extract them via shift, cast to Int32 (it will become -1), then negate and cast back to UInt32 to obtain 1.
            // If no underflow, this should be 0.
            return (UInt32) ( -( (Int32) ( cur >> BigInteger.BITS_32 ) ) );
         }
      }

      private static UInt32 AddWithCarry( ref UInt32 store, UInt32 first, UInt32 second, UInt32 carry )
      {
         unchecked
         {
            // Do addition using UInt64
            var result = (UInt64) first + second + carry;
            store = (UInt32) result;
            // If we got overflow, the carry will have only its 33th bit set.
            return (UInt32) ( result >> BigInteger.BITS_32 );
         }
      }

      private static UInt32 AddWithCarry( ref UInt32 first, UInt32 second, UInt32 carry )
      {
         return AddWithCarry( ref first, first, second, carry );
      }

      private static UInt32 MultiplyWithCarry( ref UInt32 store, UInt32 first, UInt32 second, UInt32 carry )
      {
         var result = (UInt64) first * second + carry;
         unchecked
         {
            store = (UInt32) result;
            // If we got overflow, carry will have its 33th -> bits set
            return (UInt32) ( result >> BigInteger.BITS_32 );
         }
      }

      private static UInt32 MultiplyAndAddWithCarry( ref UInt32 value, UInt32 first, UInt32 second, UInt32 carry )
      {
         var result = (UInt64) first * second + value + carry;
         unchecked
         {
            value = (UInt32) result;
            return (UInt32) ( result >> BigInteger.BITS_32 );
         }
      }




      public static Int32 ResizeBits( ref UInt32[] bits, Int32 curLength, Int32 newLength )
      {
         // If newLength is '0', still make 1-element array.
         if ( bits == null )
         {
            bits = new UInt32[Math.Max( 1, newLength )];
         }
         else if ( bits.Length < newLength )
         {
            var tmp = bits;
            bits = new UInt32[Math.Max( 1, newLength )];
            // Don't copy *all* elements of the array, since there may be garbage at the end
            Array.Copy( tmp, 0, bits, 0, curLength );
         }
         return newLength;
      }

      public static void MinimizeBitsLength( UInt32[] bits, ref Int32 length )
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
         MinimizeBitsLength( value, ref valueLength );
         resultLength = ResizeBits( ref result, resultLength, valueLength );
         Array.Copy( value, result, valueLength );
      }

      private static UInt32 GetSmall( UInt32[] bits, Int32 bitLength )
      {
         return bitLength > 0 ? bits[0] : 0;
      }

      internal static UInt64 ToUInt64( UInt32 high, UInt32 low )
      {
         return ( ( (UInt64) high ) << BigInteger.BITS_32 ) | low;
      }

      internal static void SwapBits( ref UInt32[] x, ref Int32 xLength, ref UInt32[] y, ref Int32 yLength )
      {
         var tmp = x;
         x = y;
         y = tmp;

         var tmpLength = xLength;
         xLength = yLength;
         yLength = tmpLength;
      }

      internal static void CopyBits( UInt32[] from, Int32 fromLength, ref UInt32[] to, ref Int32 toLength )
      {
         toLength = ResizeBits( ref to, toLength, fromLength );
         Array.Copy( from, 0, to, 0, fromLength );
      }

      // We must return always either -1, 0, or 1, since the result of this method is used in multiplication operations for sign
      internal static Int32 CompareBits( UInt32[] xBits, Int32 xLength, UInt32[] yBits, Int32 yLength )
      {
         var lengthDiff = xLength - yLength;
         Int32 retVal;
         if ( lengthDiff != 0 )
         {
            // Different length of bits - we can return right away
            retVal = lengthDiff < 0 ? -1 : 1;
         }
         else
         {
            retVal = 0;
            for ( --xLength, --yLength; xLength >= 0 && retVal == 0; --xLength, --yLength )
            {
               UInt32 x, y;
               if ( ( x = xBits[xLength] ) != ( y = yBits[yLength] ) )
               {
                  retVal = x < y ? -1 : 1;
               }
            }
         }

         return retVal;
      }
   }
}

public static partial class E_CILPhysical
{
   #region ModPow
   private sealed class ModPowCalculationState
   {
      public ModPowCalculationState(
         UInt32[] value,
         BigInteger modulus
         )
      {
         this.Modulus = modulus;

         // Result starts with '1'
         this.Result = new UInt32[] { 1 };
         this.ResultLength = 1;
         // Value starts with given value
         this.Value = value;
         this.ValueLength = value.Length;
         // Temporary starts as zeroes with value length
         this.Temporary = new UInt32[this.ValueLength];
         this.TemporaryLength = this.Temporary.Length;
      }

      public BigInteger Modulus { get; }

      // Value, Result, and Temporary buffer are all modified and resized by calculations, so they are fields
      public UInt32[] Value;
      public UInt32[] Result;
      public UInt32[] Temporary;
      public Int32 ValueLength;
      public Int32 ResultLength;
      public Int32 TemporaryLength;
   }

   public static BigInteger ModPow( this BigInteger value, BigInteger exponent, BigInteger modulus )
   {
      BigInteger retVal;
      if ( modulus.IsNegative() )
      {
         throw new ArithmeticException( "Modulus must be positive." );
      }
      else if ( modulus.IsOne || value.IsZero )
      {
         retVal = BigInteger.Zero;
      }
      else if ( exponent.IsZero )
      {
         retVal = BigInteger.One;
      }
      else
      {
         var state = new ModPowCalculationState( value.GetValuesArrayCopy(), modulus );

         UInt32 smallExponent;
         if ( exponent.TryGetSmallValue( out smallExponent ) )
         {
            state.ModPow_Small( smallExponent );
         }
         else
         {
            state.ModPow_Big( exponent );
         }
         retVal = BigIntegerCalculations.CreateBigInteger(
            value.IsPositive() ? 1 : ( exponent.IsEven ? 1 : -1 ), // Result is negative for negative values with odd exponents
            state.Result,
            state.ResultLength
            );
      }
      return retVal;
   }

   // Square exponentiation with modulus - optimized to situation where whole exponent is single UInt32
   private static void ModPow_Small(
      this ModPowCalculationState state,
      UInt32 exponent
      )
   {
      while ( exponent != 0 )
      {
         if ( !exponent.IsEven() )
         {
            // Perform extra step for odd exponent
            state.ModPow_Step_Result();
         }

         if ( exponent == 1 )
         {
            // Exit early - the Value-step will not affect result, and we will exit on next condition-check anyway.
            break;
         }

         // Perform step
         state.ModPow_Step_Value();

         // Shift right once
         exponent >>= 1;
      }
   }

   // Square exponentiation with modulus - the given exponent UInt32 is part of bigger number consisting of several UInt32's, and thus does not check for exponent == 0
   private static void ModPow_BigPart(
      this ModPowCalculationState state,
      UInt32 exponent
      )
   {
      // Same as ModPow_Small except that we are always iterating 32 times
      for ( var i = 0; i < BigInteger.BITS_32; ++i )
      {
         if ( !exponent.IsEven() )
         {
            state.ModPow_Step_Result();
         }
         state.ModPow_Step_Value();

         exponent >>= 1;
      }
   }

   private static void ModPow_Big( this ModPowCalculationState state, BigInteger exponent )
   {
      // Iterate all except for last integer
      var len = exponent.GetUInt32ArrayLength();
      var i = 0;
      foreach ( var cur in exponent.IterateInLittleEndianOrder() )
      {
         if ( i < len - 1 )
         {
            // All steps before last
            state.ModPow_BigPart( cur );
         }
         else
         {
            // Last step
            state.ModPow_Small( cur );
         }
         ++i;
      }
   }

   private static void ModPow_Step_Result( this ModPowCalculationState state )
   {
      // result = (result * value) % modulus
      state.ModPow_Step( state.Result, state.ResultLength, state.Value, state.ValueLength, ref state.Result, ref state.ResultLength );
   }

   private static void ModPow_Step_Value( this ModPowCalculationState state )
   {
      // value = (value * value) % modulus
      state.ModPow_Step( state.Value, state.ValueLength, state.Value, state.ValueLength, ref state.Value, ref state.ValueLength );
   }

   private static void ModPow_Step(
      this ModPowCalculationState state,
      UInt32[] x,
      Int32 xLength,
      UInt32[] y,
      Int32 yLength,
      ref UInt32[] result,
      ref Int32 resultLength
      )
   {
      // result = ( x * y ) % modulus
      var tmp = state.Temporary;
      var tmpLength = state.TemporaryLength;

      BigIntegerCalculations.Multiply( x, xLength, y, yLength, ref tmp, ref tmpLength );
      BigIntegerCalculations.Modulus( tmp, ref tmpLength, state.Modulus );
      state.Temporary = result;
      state.TemporaryLength = resultLength;
      result = tmp;
      resultLength = tmpLength;
   }

   #endregion

   #region ExtendedEuclidean

   private sealed class ExtendedEuclideanCalculationState
   {
      public ExtendedEuclideanCalculationState( UInt32[] a, UInt32[] b )
      {
         this.U1 = new UInt32[] { 1 };
         //this.U2 = new UInt32[] { 0 };
         this.U3 = a;

         this.V1 = new UInt32[] { 0 };
         //this.V2 = new UInt32[] { 1 };
         this.V3 = b;

         this.U1Length = this.U1.Length;
         //this.U2Length = this.U2.Length;
         this.U3Length = this.U3.Length;

         this.V1Length = this.V1.Length;
         //this.V2Length = this.V2.Length;
         this.V3Length = this.V3.Length;

         this.Q = this.U3.CreateArrayCopy();
         this.QLength = this.Q.Length;

         this.Temporary = new UInt32[this.V3Length];
         this.TemporaryLength = this.Temporary.Length;
         this.Temporary2 = new UInt32[this.U3Length];
         this.Temporary2Length = this.Temporary2.Length;

         this.U1Sign = this.V1Sign = 1; // this.U2Sign = this.V1Sign = this.V2Sign = 1;
      }

      public UInt32[] U1;
      //public UInt32[] U2;
      public UInt32[] U3;

      public UInt32[] V1;
      //public UInt32[] V2;
      public UInt32[] V3;

      public Int32 U1Length;
      //public Int32 U2Length;
      public Int32 U3Length;

      public Int32 V1Length;
      //public Int32 V2Length;
      public Int32 V3Length;

      public UInt32[] Q;
      public Int32 QLength;

      public UInt32[] Temporary;
      public Int32 TemporaryLength;
      public UInt32[] Temporary2;
      public Int32 Temporary2Length;

      // U3 and V3 are always positive
      public Int32 U1Sign;
      //public Int32 U2Sign;
      public Int32 V1Sign;
      //public Int32 V2Sign;

      public override String ToString()
      {
         return "U3: " + BigIntegerCalculations.CreateBigInteger( 1, this.U3, this.U3Length ) + "\n"
            + "U1 " + BigIntegerCalculations.CreateBigInteger( this.U1Sign, this.U1, this.U1Length ) + "\n"
            + "V3: " + BigIntegerCalculations.CreateBigInteger( 1, this.V3, this.V3Length ) + "\n"
            + "V1: " + BigIntegerCalculations.CreateBigInteger( this.V1Sign, this.V1, this.V1Length );

      }
   }

   // When 'a' and 'b' given, computes ax + by = GCD(a,b)
   // The GCD is returned
   public static BigInteger ExtendedEuclidean( this BigInteger a, BigInteger b, out BigInteger x, out BigInteger y, Boolean computeY = true )
   {
      if ( a.IsNegative() )
      {
         throw new ArithmeticException( "The 'a' parameter to extended Euclidean algorithm must not be negative." );
      }
      else if ( b.IsNegative() )
      {
         throw new ArithmeticException( "The 'b' parameter to extended Euclidean algorithm must not be negative." );
      }
      else
      {
         // U3 and V3 will get assigned so create a copy
         var state = new ExtendedEuclideanCalculationState( a.GetValuesArrayCopy(), b.GetValuesArrayCopy() );

         // We don't keep computing U2 and V2 - the 'y' can be calculated from original equasion 'ax + by = GCD(a,b)' once we have found out the x and GCD
         while ( state.V3Length >= 1 && state.V3[0] != 0 )
         {
            // tmp = u3
            BigIntegerCalculations.CopyBits( state.U3, state.U3Length, ref state.Temporary, ref state.TemporaryLength );
            // tmp = u3 % v3, q = u3 / v3
            BigIntegerCalculations.DivideWithRemainder( state.Temporary, ref state.TemporaryLength, state.V3, state.V3Length, ref state.Q, ref state.QLength, true );
            // u3 = v3
            BigIntegerCalculations.SwapBits( ref state.U3, ref state.U3Length, ref state.V3, ref state.V3Length );
            // v3 = u3 - q * v3 ( = u3 % v3)
            BigIntegerCalculations.SwapBits( ref state.V3, ref state.V3Length, ref state.Temporary, ref state.TemporaryLength );


            //var t1 = state.U1 - q * state.V1;
            //state.U1 = state.V1;
            //state.V1 = t1;

            // Let's do u1 = v1 first
            // We have to save u1
            BigIntegerCalculations.CopyBits( state.U1, state.U1Length, ref state.Temporary, ref state.TemporaryLength );
            var u1Sign = state.U1Sign;
            // Now u1 = v1
            BigIntegerCalculations.SwapBits( ref state.U1, ref state.U1Length, ref state.V1, ref state.V1Length );
            state.U1Sign = state.V1Sign;

            if ( state.V3Length >= 1 && state.V3[0] != 0 )
            {
               // Only calculate v1 if we are going to iterate one more time

               // tmp2 = q * v1
               // Since q is always positive (because u3 and v3 are always positive), the sign of tmp2 is sign of v1
               // Remember that v1 is now in u1
               BigIntegerCalculations.Multiply( state.Q, state.QLength, state.U1, state.U1Length, ref state.Temporary2, ref state.Temporary2Length );
               // v1 = u1 - tmp2 ( = tmp - tmp2 )
               BigIntegerCalculations.Subtract( state.Temporary, state.TemporaryLength, u1Sign, state.Temporary2, state.Temporary2Length, state.V1Sign, ref state.V1, ref state.V1Length, out state.V1Sign );
            }

         }

         x = BigIntegerCalculations.CreateBigInteger( state.U1Sign, state.U1, state.U1Length );
         var retVal = BigIntegerCalculations.CreateBigInteger( 1, state.U3, state.U3Length );

         if ( computeY )
         {

            // ax + by = retVal
            // => y = (retVal - ax) / b
            // => y = (u3 - a * u1) / b
            BigIntegerCalculations.Multiply( a, state.U1, state.U1Length, ref state.Temporary, ref state.TemporaryLength );
            BigIntegerCalculations.Subtract( state.U3, state.U3Length, 1, state.Temporary, state.TemporaryLength, state.U1Sign, ref state.Temporary2, ref state.Temporary2Length, out state.V1Sign );
            BigIntegerCalculations.DivideWithRemainder( state.Temporary2, ref state.Temporary2Length, b, ref state.Temporary, ref state.TemporaryLength, true );
            y = BigIntegerCalculations.CreateBigInteger( state.V1Sign, state.Temporary, state.TemporaryLength );

            // Verify
            System.Diagnostics.Debug.Assert( ( a * x + b * y ).Equals( retVal ) );
         }
         else
         {
            y = default( BigInteger );
         }

         return retVal;
      }
   }

   #endregion
}

#pragma warning restore 1591