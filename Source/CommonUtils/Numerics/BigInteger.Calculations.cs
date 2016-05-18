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

      // TODO in-place subtract/add/multiply

      //public static BigInteger CreateBigInteger( Int32 sign, UInt32[] bits, Int32 bitsLength )
      //{
      //   return new BigInteger( sign, bits, bitsLength );
      //}

      public static Boolean TryGetSmallValue( this BigInteger value, out UInt32 smallValue )
      {
         var retVal = value.IsSmall;
         smallValue = retVal ? value.SmallValue : 0u;
         return retVal;
      }

      public static Boolean TryGetSmallValue( this ModifiableBigInteger bigInt, out UInt32 smallValue )
      {
         return TryGetSmallValue( bigInt._bits, ref bigInt._length, out smallValue );
      }

      public static Boolean IsZero( this ModifiableBigInteger bigInt )
      {
         return IsZero( bigInt._bits, ref bigInt._length );
      }

      public static Boolean IsEven( this ModifiableBigInteger bigInt )
      {
         return IsEven( bigInt._bits );
      }

      public static ModifiableBigInteger AsModifiable( this BigInteger bigInt )
      {
         return new ModifiableBigInteger( bigInt );
      }

      internal static Boolean TryGetSmallValue( UInt32[] bits, ref Int32 bitsLength, out UInt32 smallValue )
      {
         MinimizeBitsLength( bits, ref bitsLength );
         var retVal = bitsLength <= 1;
         smallValue = retVal ? GetSmall( bits, bitsLength ) : 0u;
         return retVal;
      }

      public static Boolean IsZero( UInt32[] bits, ref Int32 bitsLength )
      {
         UInt32 smallValue;
         return TryGetSmallValue( bits, ref bitsLength, out smallValue ) && smallValue == 0;
      }

      //public static void SetSmallValue( this ModifiableBigInteger bigInt, UInt32 smallValue, Int32 sign )
      //{
      //   SetSmallValue( ref bigInt._bits, ref bigInt._length, smallValue );
      //   if ( smallValue == 0 )
      //   {
      //      bigInt._sign = 0;
      //   }
      //}

      private static void SetSmallValue( ref UInt32[] bits, ref Int32 bitsLength, UInt32 smallValue )
      {
         if ( smallValue == 0 )
         {
            bitsLength = 0;
         }
         else
         {
            bitsLength = ResizeBits( ref bits, bitsLength, 1 );
            bits[0] = smallValue;
         }
      }

      private static Boolean IsEven( UInt32[] bits )
      {
         return bits.IsNullOrEmpty() || bits[0].IsEven();
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
         ModifiableBigInteger right,
         ModifiableBigInteger result
         )
      {
         var leftArray = left.GetArrayDirect();
         Subtract( leftArray, leftArray.Length, left.Sign, right._bits, right._length, right._sign, ref result._bits, ref result._length, out result._sign );
      }

      public static void Subtract(
         ModifiableBigInteger left,
         BigInteger right,
         ModifiableBigInteger result
         )
      {
         var rightArray = right.GetArrayDirect();
         Subtract( left._bits, left._length, left._sign, rightArray, rightArray.Length, right.Sign, ref result._bits, ref result._length, out result._sign );
      }

      public static void Subtract(
         BigInteger left,
         BigInteger right,
         ModifiableBigInteger result
         )
      {
         var leftArray = left.GetArrayDirect();
         var rightArray = right.GetArrayDirect();
         Subtract( leftArray, leftArray.Length, left.Sign, rightArray, rightArray.Length, right.Sign, ref result._bits, ref result._length, out result._sign );
      }

      public static void Subtract(
         ModifiableBigInteger left,
         ModifiableBigInteger right,
         ModifiableBigInteger result
         )
      {
         Subtract( left._bits, left._length, left._sign, right._bits, right._length, right._sign, ref result._bits, ref result._length, out result._sign );
      }

      internal static void Subtract(
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
         if ( IsZero( right, ref rightLength ) )
         {
            // result is left
            CopyBits( left, leftLength, ref result, ref resultLength );
            resultSign = leftSign;
         }
         else if ( IsZero( left, ref leftLength ) )
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
         ModifiableBigInteger right,
         ModifiableBigInteger result
         )
      {
         var leftArray = left.GetArrayDirect();
         Add( leftArray, leftArray.Length, left.Sign, right._bits, right._length, right._sign, ref result._bits, ref result._length, out result._sign );
      }

      public static void Add(
         ModifiableBigInteger left,
         BigInteger right,
         ModifiableBigInteger result
         )
      {
         var rightArray = right.GetArrayDirect();
         Add( left._bits, left._length, left._sign, rightArray, rightArray.Length, right.Sign, ref result._bits, ref result._length, out result._sign );
      }

      public static void Add(
         BigInteger left,
         BigInteger right,
         ModifiableBigInteger result
         )
      {
         var leftArray = left.GetArrayDirect();
         var rightArray = right.GetArrayDirect();
         Add( leftArray, leftArray.Length, left.Sign, rightArray, rightArray.Length, right.Sign, ref result._bits, ref result._length, out result._sign );
      }

      public static void Add(
         ModifiableBigInteger left,
         ModifiableBigInteger right,
         ModifiableBigInteger result
         )
      {
         Add( left._bits, left._length, left._sign, right._bits, right._length, right._sign, ref result._bits, ref result._length, out result._sign );
      }

      internal static void Add(
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
         if ( IsZero( right, ref rightLength ) )
         {
            // result is left
            CopyBits( left, leftLength, ref result, ref resultLength );
            resultSign = leftSign;
         }
         else if ( IsZero( left, ref leftLength ) )
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
         ModifiableBigInteger right,
         ModifiableBigInteger result
         )
      {
         var leftArray = left.GetArrayDirect();
         Multiply( leftArray, leftArray.Length, left.Sign, right._bits, right._length, right._sign, ref result._bits, ref result._length, out result._sign );
      }

      public static void Multiply(
         ModifiableBigInteger left,
         BigInteger right,
         ModifiableBigInteger result
         )
      {
         var rigthArray = right.GetArrayDirect();
         Multiply( left._bits, left._length, left._sign, rigthArray, rigthArray.Length, right.Sign, ref result._bits, ref result._length, out result._sign );
      }

      public static void Multiply(
         BigInteger left,
         BigInteger right,
         ModifiableBigInteger result
      )
      {
         var leftArray = left.GetArrayDirect();
         var rightArray = right.GetArrayDirect();
         Multiply( leftArray, leftArray.Length, left.Sign, rightArray, rightArray.Length, right.Sign, ref result._bits, ref result._length, out result._sign );
      }

      public static void Multiply(
         ModifiableBigInteger left,
         ModifiableBigInteger right,
         ModifiableBigInteger result
         )
      {
         Multiply( left._bits, left._length, left._sign, right._bits, right._length, right._sign, ref result._bits, ref result._length, out result._sign );
      }

      internal static void Multiply(
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
         if ( leftLength <= 1 )
         {
            var small = GetSmall( left, leftLength );
            switch ( small )
            {
               case 0:
                  // Result is always zero
                  SetSmall( ref result, ref resultLength, 0 );
                  resultSign = 0;
                  break;
               case 1:
                  // Result is always same as value
                  SetAsCopy( ref result, ref resultLength, right, rightLength );
                  resultSign = rightSign;
                  break;
               default:
                  // Perform small multiply
                  Multiply_Small( right, rightLength, rightSign, small, leftSign, ref result, ref resultLength, out resultSign );
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
                  resultSign = 0;
                  break;
               case 1:
                  // Result is always same as value
                  SetAsCopy( ref result, ref resultLength, left, leftLength );
                  resultSign = leftSign;
                  break;
               default:
                  Multiply_Small( left, leftLength, leftSign, small, rightSign, ref result, ref resultLength, out resultSign );
                  break;
            }
         }
         else
         {
            Multiply_Big( left, leftLength, right, rightLength, ref result, ref resultLength );
            resultSign = leftSign * rightSign;
         }
      }

      // This method computes result = big * small. The 'big' array is unmodified.
      // Don't call this when small is zero.
      private static void Multiply_Small( UInt32[] big, Int32 bigLength, Int32 bigSign, UInt32 small, Int32 smallSign, ref UInt32[] result, ref Int32 resultLength, out Int32 resultSign )
      {
         UInt32 bigSmall;
         if ( TryGetSmallValue( big, ref bigLength, out bigSmall ) && bigSmall <= 1 )
         {
            if ( bigSmall == 0 )
            {
               SetSmallValue( ref result, ref resultLength, 0 );
               resultSign = 0;
            }
            else
            {
               // big is '1'
               SetSmallValue( ref result, ref resultLength, small );
               resultSign = bigSign * smallSign;
            }
         }
         else
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

            resultSign = bigSign * smallSign;
         }
      }

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
         ModifiableBigInteger divident,
         BigInteger divisor
         )
      {
         var divisorArray = divisor.GetArrayDirect();
         Modulus( divident._bits, ref divident._length, divident._sign, divisorArray, divisorArray.Length, divisor.Sign );
      }

      // Computes divident = divident % divisor
      public static void Modulus(
         ModifiableBigInteger divident,
         ModifiableBigInteger divisor
         )
      {
         Modulus( divident._bits, ref divident._length, divident._sign, divisor._bits, divisor._length, divisor._sign );
      }

      public static void Modulus_Positive(
         ModifiableBigInteger divident,
         BigInteger divisor,
         ModifiableBigInteger temp
         )
      {
         var divisorBits = divisor.GetArrayDirect();
         Modulus_Positive( ref divident._bits, ref divident._length, ref divident._sign, divisorBits, divisorBits.Length, divisor.Sign, ref temp._bits, ref temp._length );
      }

      public static void Modulus_Positive(
         ModifiableBigInteger divident,
         ModifiableBigInteger divisor,
         ModifiableBigInteger temp
         )
      {
         Modulus_Positive( ref divident._bits, ref divident._length, ref divident._sign, divisor._bits, divisor._length, divisor._sign, ref temp._bits, ref temp._length );
      }

      private static void Modulus(
         UInt32[] divident,
         ref Int32 dividentLength,
         Int32 dividentSign,
         UInt32[] divisor,
         Int32 divisorLength,
         Int32 divisorSign
         )
      {
         UInt32[] dummy = null;
         Int32 dummy2 = -1;
         DivideWithRemainder( divident, ref dividentLength, dividentSign, divisor, divisorLength, divisorSign, ref dummy, ref dummy2, ref dummy2, false );
      }

      private static void Modulus_Positive(
         ref UInt32[] divident,
         ref Int32 dividentLength,
         ref Int32 dividentSign,
         UInt32[] divisor,
         Int32 divisorLength,
         Int32 divisorSign,
         ref UInt32[] tmp,
         ref Int32 tmpLength
         )
      {
         Modulus( divident, ref dividentLength, dividentSign, divisor, divisorLength, divisorSign );
         // Modulus will be negative if divident was negative
         if ( dividentSign < 0 )
         {
            Add( divident, dividentLength, dividentSign, divisor, divisorLength, 1, ref tmp, ref tmpLength, out dividentSign );
            SwapBits( ref divident, ref dividentLength, ref tmp, ref tmpLength );
         }
      }

      public static void DivideWithRemainder(
         ModifiableBigInteger divident,
         BigInteger divisor,
         ModifiableBigInteger quotient,
         Boolean computeQuotient
         )
      {
         var divisorArray = divisor.GetArrayDirect();
         if ( computeQuotient )
         {
            DivideWithRemainder( divident._bits, ref divident._length, divident._sign, divisorArray, divisorArray.Length, divisor.Sign, ref quotient._bits, ref quotient._length, ref quotient._sign, true );
         }
         else
         {
            UInt32[] dummy = null;
            Int32 dummy2 = -1;
            DivideWithRemainder( divident._bits, ref divident._length, divident._sign, divisorArray, divisorArray.Length, divisor.Sign, ref dummy, ref dummy2, ref dummy2, false );
         }
      }

      public static void DivideWithRemainder(
         ModifiableBigInteger divident,
         ModifiableBigInteger divisor,
         ModifiableBigInteger quotient,
         Boolean computeQuotient
         )
      {
         if ( computeQuotient )
         {
            DivideWithRemainder( divident._bits, ref divident._length, divisor._sign, divisor._bits, divisor._length, divisor._sign, ref quotient._bits, ref quotient._length, ref quotient._sign, true );
         }
         else
         {
            UInt32[] dummy = null;
            Int32 dummy2 = -1;
            DivideWithRemainder( divident._bits, ref divident._length, divident._sign, divisor._bits, divisor._length, divisor._sign, ref dummy, ref dummy2, ref dummy2, false );
         }
      }

      // Computes quotient = divident / divisor, if so specified.
      // The modulus is stored in divident, always.
      internal static void DivideWithRemainder(
         UInt32[] divident,
         ref Int32 dividentLength,
         Int32 dividentSign,
         UInt32[] divisor,
         Int32 divisorLength,
         Int32 divisorSign,
         ref UInt32[] quotient,
         ref Int32 quotientLength,
         ref Int32 quotientSign,
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

         if ( computeQuotient )
         {
            quotientSign = dividentSign * divisorSign;
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
               // Do long division, like in school.

               if ( computeQuotient )
               {
                  ResizeBits( ref quotient, quotientLength, newQuotientLength );
               }
               quotientLength = newQuotientLength;
               // Get the highest 32 bits of divisor (x2) to use for the trial divisions (since we iterate in BE order)
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




      internal static Int32 ResizeBits( ref UInt32[] bits, Int32 curLength, Int32 newLength )
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

      internal static void MinimizeBitsLength( UInt32[] bits, ref Int32 length )
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

      public static void Swap( ref ModifiableBigInteger x, ref ModifiableBigInteger y )
      {
         var tmp = x;
         x = y;
         y = tmp;
      }

      private static void SwapBits( ref UInt32[] x, ref Int32 xLength, ref UInt32[] y, ref Int32 yLength )
      {
         var tmp = x;
         x = y;
         y = tmp;

         var tmpLength = xLength;
         xLength = yLength;
         yLength = tmpLength;
      }

      public static void CopyBits( this ModifiableBigInteger from, ModifiableBigInteger to, Boolean copySign )
      {
         CopyBits( from._bits, from._length, ref to._bits, ref to._length );
         if ( copySign )
         {
            to.Sign = from.Sign;
         }
      }

      private static void CopyBits( UInt32[] from, Int32 fromLength, ref UInt32[] to, ref Int32 toLength )
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

   public sealed class ModifiableBigInteger
   {
      internal UInt32[] _bits;
      internal Int32 _length;
      internal Int32 _sign;

      public ModifiableBigInteger()
         : this( 0 )
      {

      }

      public ModifiableBigInteger( BigInteger initial )
      {
         this.InitializeBig( initial );
      }

      public ModifiableBigInteger( Int32 bitsSize )
      {
         this._bits = new UInt32[bitsSize];
         this._length = bitsSize;
      }

      public void InitializeBig( BigInteger value )
      {
         var bits = value.GetArrayDirect();
         this._length = Calculations.BigIntegerCalculations.ResizeBits( ref this._bits, this._length, bits.Length );
         Array.Copy( bits, 0, this._bits, 0, bits.Length );
         this._sign = value.Sign;
      }

      public void InitializeSmall( Int32 small )
      {
         this._length = Calculations.BigIntegerCalculations.ResizeBits( ref this._bits, this._length, 1 );
         this._bits[0] = small < 0 ? unchecked((UInt32) ( -small )) : (UInt32) small;
         this._sign = small == 0 ? 0 : ( small < 0 ? -1 : 1 );
      }

      public BigInteger CreateBigInteger()
      {
         return new BigInteger( this._sign, this._bits.CreateArrayCopy( this._length ), this._length );
      }

      public Int32 BitsArrayLength
      {
         get
         {
            return this._length;
         }
      }

      public Int32 Sign
      {
         get
         {
            return this._sign;
         }
         set
         {
            this._sign = value;
         }
      }

      [CLSCompliant( false )]
      public UInt32[] Bits
      {
         get
         {
            return this._bits;
         }
      }

      public override String ToString()
      {
         return BigInteger.ToString( this._sign, this._bits, this._length );
      }
   }


   public sealed class ModPowCalculationState
   {
      public ModPowCalculationState(
         ModifiableBigInteger value,
         BigInteger modulus
         )
      {
         this.Reset( value, modulus );
      }

      public void Reset( ModifiableBigInteger value, BigInteger modulus )
      {
         this.Modulus = modulus;

         // Result starts with '1'
         this.Result = new ModifiableBigInteger( 1 );
         this.Result.InitializeSmall( 1 );
         // Value starts with given value
         this.Value = value;
         // Temporary starts as zeroes with value length
         this.Temporary = new ModifiableBigInteger( value.BitsArrayLength );
      }

      public BigInteger Modulus;

      // Value, Result, and Temporary buffer are all modified and resized by calculations, so they are fields
      public ModifiableBigInteger Value;
      public ModifiableBigInteger Result;
      public ModifiableBigInteger Temporary;
   }

   public class ExtendedEuclideanCalculationState
   {


      public ExtendedEuclideanCalculationState( ModifiableBigInteger a, ModifiableBigInteger b )
      {
         // U1 starts with 1
         this.U1 = new ModifiableBigInteger( 1 );
         this.U1.InitializeSmall( 1 );
         // U3 starts with 'a'
         this.U3 = a;

         // V1 start with zero
         this.V1 = new ModifiableBigInteger( 1 );
         // V3 starts with 'b'
         this.V3 = b;
         this.Q = new ModifiableBigInteger();

         this.Temporary = new ModifiableBigInteger( this.V3.BitsArrayLength );
         this.Temporary2 = new ModifiableBigInteger( this.U3.BitsArrayLength );
      }

      public ModifiableBigInteger U1;
      public ModifiableBigInteger U3;

      public ModifiableBigInteger V1;
      public ModifiableBigInteger V3;
      public ModifiableBigInteger Q;

      public ModifiableBigInteger Temporary;
      public ModifiableBigInteger Temporary2;
   }
}

public static partial class E_CILPhysical
{
   #region ModPow

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
         var state = new ModPowCalculationState( value.AsModifiable(), modulus );

         UInt32 smallExponent;
         if ( exponent.TryGetSmallValue( out smallExponent ) )
         {
            state.ModPow_Small( smallExponent );
         }
         else
         {
            state.ModPow_Big( exponent );
         }
         // Result is negative for negative values with odd exponents
         System.Diagnostics.Debug.Assert( ( value.IsPositive() ? 1 : ( exponent.IsEven ? 1 : -1 ) ) == state.Result.Sign );
         retVal = state.Result.CreateBigInteger();
      }
      return retVal;
   }

   public static void ModPow( this ModPowCalculationState state, BigInteger exponent )
   {
      state.ModPow( exponent.AsModifiable() );
   }

   [CLSCompliant( false )]
   public static void ModPow( this ModPowCalculationState state, ModifiableBigInteger exponent )
   {
      var modulus = state.Modulus;
      if ( modulus.IsNegative() )
      {
         throw new ArithmeticException( "Modulus must be positive." );
      }
      else if ( modulus.IsOne || state.Value.IsZero() )
      {
         // Result is zero
         state.Result.InitializeSmall( 0 );
      }
      else if ( exponent.IsZero() )
      {
         // Result is one
         state.Result.InitializeSmall( 1 );
         state.Result.Sign = 1;
      }
      else
      {
         UInt32 smallExponent;
         if ( exponent.TryGetSmallValue( out smallExponent ) )
         {
            state.ModPow_Small( smallExponent );
         }
         else
         {
            state.ModPow_Big( exponent.Bits, exponent.BitsArrayLength );
         }
         //resultSign = valueSign < 0 && !BigIntegerCalculations.IsEven( exponent ) ? -1 : 1; // Result is negative for negative values with odd exponents
      }
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

   private static void ModPow_Big( this ModPowCalculationState state, UInt32[] exponent, Int32 exponentLength )
   {
      // Iterate all except for last integer
      for ( var i = 0; i < exponentLength - 1; ++i )
      {
         state.ModPow_BigPart( exponent[i] );
      }

      state.ModPow_Small( exponent[exponentLength - 1] );
   }

   private static void ModPow_Step_Result( this ModPowCalculationState state )
   {
      // result = (result * value) % modulus
      state.ModPow_Step( state.Result, state.Value, ref state.Result );
   }

   private static void ModPow_Step_Value( this ModPowCalculationState state )
   {
      // value = (value * value) % modulus
      state.ModPow_Step( state.Value, state.Value, ref state.Value );
   }

   private static void ModPow_Step(
      this ModPowCalculationState state,
      ModifiableBigInteger x,
      ModifiableBigInteger y,
      ref ModifiableBigInteger result
      )
   {
      // result = ( x * y ) % modulus
      BigIntegerCalculations.Multiply( x, y, state.Temporary );
      BigIntegerCalculations.Modulus( state.Temporary, state.Modulus );
      BigIntegerCalculations.Swap( ref state.Temporary, ref result );
   }

   #endregion

   #region ExtendedEuclidean



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
         var state = new ExtendedEuclideanCalculationState( a.AsModifiable(), b.AsModifiable() );

         state.RunExtendedEuclideanAlgorithm();

         x = state.U1.CreateBigInteger();
         var retVal = state.U3.CreateBigInteger();

         if ( computeY )
         {
            state.ComputeYAfterRunningAlgorithm( a, b );
            y = state.Temporary.CreateBigInteger(); // BigIntegerCalculations.CreateBigInteger( state.V1Sign, state.Temporary, state.TemporaryLength );

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

   // After this, x will be in U1 and gcd will be in U3
   // The y is computable from a, b, x, and gcd.
   public static void RunExtendedEuclideanAlgorithm( this ExtendedEuclideanCalculationState state )
   {
      // We don't keep computing U2 and V2 - the 'y' can be calculated from original equasion 'ax + by = GCD(a,b)' once we have found out the x and GCD
      while ( !state.V3.IsZero() )
      {
         // tmp = u3
         state.U3.CopyBits( state.Temporary, true );
         // tmp = u3 % v3, q = u3 / v3
         BigIntegerCalculations.DivideWithRemainder( state.Temporary, state.V3, state.Q, true );
         // u3 = v3
         BigIntegerCalculations.Swap( ref state.U3, ref state.V3 );
         // v3 = u3 - q * v3 ( = u3 % v3)
         BigIntegerCalculations.Swap( ref state.V3, ref state.Temporary );


         //var t1 = state.U1 - q * state.V1;
         //state.U1 = state.V1;
         //state.V1 = t1;

         // Let's do u1 = v1 first
         // We have to save u1
         state.U1.CopyBits( state.Temporary, true );

         // Now u1 = v1
         BigIntegerCalculations.Swap( ref state.U1, ref state.V1 );

         if ( !state.V3.IsZero() )
         {
            // Only calculate v1 if we are going to iterate one more time
            // TODO move this into beginning of loop under if (state.Q != null) condition, since no point repeating loop condition here...

            // tmp2 = q * v1
            // Since q is always positive (because u3 and v3 are always positive), the sign of tmp2 is sign of v1
            // Remember that v1 is now in u1
            BigIntegerCalculations.Multiply( state.Q, state.U1, state.Temporary2 );
            // v1 = u1 - tmp2 ( = tmp - tmp2 )
            BigIntegerCalculations.Subtract( state.Temporary, state.Temporary2, state.V1 );
         }

      }
   }

   // After this, y will be in state.Temporary, and its sign will be in state.V1Sign
   // Assumes that the state is not modified after RunExtendedEuclideanAlgorithm method!
   public static void ComputeYAfterRunningAlgorithm( this ExtendedEuclideanCalculationState state, BigInteger a, BigInteger b )
   {
      // ax + by = retVal
      // => y = (retVal - ax) / b
      // => y = (u3 - a * u1) / b
      BigIntegerCalculations.Multiply( a, state.U1, state.Temporary );
      BigIntegerCalculations.Subtract( state.U3, state.Temporary, state.Temporary2 );
      BigIntegerCalculations.DivideWithRemainder( state.Temporary2, b, state.Temporary, true );
   }

   #endregion
}

#pragma warning restore 1591