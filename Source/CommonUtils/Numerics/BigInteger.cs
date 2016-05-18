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

namespace CommonUtils.Numerics
{
#pragma warning disable 1591

   public enum BinaryEndianness
   {
      LittleEndian,
      BigEndian
   }

   public partial struct BigInteger : IComparable<BigInteger>, IEquatable<BigInteger>
   {
      internal const Int32 BITS_BYTE = 8;
      internal const Int32 BITS_32 = BigInteger.BITS_BYTE * sizeof( Int32 );
      private const Int32 BYTES_32 = BITS_32 / BITS_BYTE;

      #region Static properties

      public static BigInteger One { get; }

      public static BigInteger Zero { get; }

      public static BigInteger MinusOne { get; }

      #endregion

      #region Static constructor

      static BigInteger()
      {
         One = new BigInteger( 1 );
         Zero = new BigInteger( 0 );
         MinusOne = new BigInteger( -1 );
      }

      #endregion

      #region Instance fields

      private readonly Int32 _sign;
      private readonly UInt32[] _bits; // Bits are in LE order, makes some arithmetic operations easier

      #endregion

      #region Instance constructors

      public BigInteger( Int32 intValue )
      {
         if ( intValue == 0 )
         {
            this = Zero;
         }
         else
         {
            UInt32 u32;
            Int32 sign;

            if ( intValue < 0 )
            {
               sign = -1;
               u32 = unchecked((UInt32) ( -intValue ));
            }
            else
            {
               sign = 1;
               u32 = (UInt32) intValue;
            }

            this._sign = sign;
            this._bits = new UInt32[] { u32 };
         }
      }

      public BigInteger( Int64 intValue )
      {
         if ( intValue == 0 )
         {
            this = Zero;
         }
         else
         {
            UInt64 u64;
            Int32 sign;
            if ( intValue < 0 )
            {
               sign = -1;
               u64 = unchecked((UInt64) ( -intValue ));
            }
            else
            {
               sign = 1;
               u64 = (UInt64) intValue;
            }

            this._sign = sign;
            var hi = (UInt32) ( u64 >> BITS_32 );
            var lo = unchecked((UInt32) u64);
            this._bits = hi == 0 ?
               new UInt32[] { lo } :
               new UInt32[] { lo, hi };
         }
      }

      [CLSCompliant( false )]
      public BigInteger( UInt32 value )
         : this( 1, value )
      {

      }

      private BigInteger( Int32 sign, UInt32 value )
      {
         if ( value == 0 )
         {
            this = Zero;
         }
         else
         {
            this._sign = sign;
            this._bits = new UInt32[] { value };
         }
      }

      [CLSCompliant( false )]
      public BigInteger( UInt64 intValue )
      {
         if ( intValue == 0 )
         {
            this = Zero;
         }
         else
         {
            var hi = (UInt32) ( intValue >> BITS_32 );
            var lo = unchecked((UInt32) intValue);
            this._sign = 1;
            this._bits = hi == 0 ?
               new UInt32[] { lo } :
               new UInt32[] { lo, hi };
         }
      }


      private BigInteger( Int32 sign, UInt32[] bits, Boolean checkBitsForLeadingZeroes )
      {
         if ( checkBitsForLeadingZeroes )
         {
            this._bits = CheckForTrailingZeroes( bits, sign, out this._sign );
         }
         else
         {
            this._sign = ValidateSign( sign, bits );
            this._bits = this._sign == 0 ? Empty<UInt32>.Array : bits;
         }

         // Check for sign and leading zeroes in debug mode
         System.Diagnostics.Debug.Assert(
            ( this._bits.IsNullOrEmpty() && this._sign == 0 )
            || ( !this._bits.IsNullOrEmpty() && this._bits[this._bits.Length - 1] != 0 )
            );
      }

      internal BigInteger( Int32 sign, UInt32[] bits, Int32 bitsLength )
      {
         if ( bitsLength == 0 || ( bitsLength == 1 && ( bits == null || bits[0] == 0 ) ) )
         {
            sign = 0;
         }

         this._sign = ValidateSign( sign, bits );
         this._bits = this._sign == 0 ?
            Empty<UInt32>.Array :
            ( bitsLength == bits.Length ?
               bits :
               bits.CreateArrayCopy( bitsLength )
            );

         // Check for sign and leading zeroes in debug mode
         System.Diagnostics.Debug.Assert(
            ( this._bits.IsNullOrEmpty() && this._sign == 0 )
            || ( !this._bits.IsNullOrEmpty() && this._bits[this._bits.Length - 1] != 0 )
            );
      }

      #endregion

      #region Instance properties

      public Int32 BitLength
      {
         get
         {
            return CalculateBitLength( this._bits );
         }
      }

      internal Int32 BitsArrayLength
      {
         get
         {
            return this._bits.Length;
         }
      }

      public Boolean IsZero
      {
         get
         {
            return this.Sign == 0;
         }
      }

      public Boolean IsOne
      {
         get
         {
            return this.Sign == 1 && this._bits[0] == 1;
         }
      }

      public Boolean IsMinusOne
      {
         get
         {
            return this.Sign == -1 && this._bits[0] == 1;
         }
      }

      public Int32 Sign
      {
         get
         {
            return this._sign;
         }
      }

      public Boolean IsPowerOfTwo
      {
         get
         {
            return this.Sign == 1
               && this._bits[this._bits.Length - 1].IsPowerOfTwo()
               && this._bits.Take( this._bits.Length - 1 ).All( b => b == 0 );
         }
      }

      public Boolean IsEven
      {
         get
         {
            return this._bits.IsNullOrEmpty() || this._bits[0].IsEven();
         }
      }


      internal Boolean IsSmall
      {
         get
         {
            return AreSmallBits( this._bits );
         }
      }

      internal UInt32 SmallValue
      {
         get
         {
            return this._bits?[0] ?? 0;
         }
      }

      #endregion

      #region Instance methods

      public BigInteger Remainder( BigInteger divisor )
      {
         return this % divisor;
      }

      public BigInteger Subtract( BigInteger other )
      {
         return this - other;
      }

      public BigInteger Add( BigInteger other )
      {
         return this + other;
      }

      public BigInteger Multiply( BigInteger other )
      {
         return this * other;
      }

      public BigInteger Divide( BigInteger divisor )
      {
         return this / divisor;
      }

      public BigInteger DivideRemainder( BigInteger divisor, out BigInteger remainder )
      {
         BigInteger modulus = default( BigInteger ), quotient = default( BigInteger );
         PerformDivMod( this, divisor, true, true, ref modulus, ref quotient );
         remainder = modulus;
         return quotient;
      }

      //// TODO move these two methods to Calculations.BigIntegerCalculations, and make this._bits internal!

      //[CLSCompliant( false )]
      //public UInt32[] GetValuesArrayCopy()
      //{
      //   return this._bits.CreateArrayCopy();
      //}

      //[CLSCompliant( false )]
      //public void WriteToValuesArray( ref UInt32[] array, ref Int32 arrayLength )
      //{
      //   var bits = this._bits;
      //   arrayLength = Calculations.BigIntegerCalculations.ResizeBits( ref array, arrayLength, bits.Length );
      //   Array.Copy( bits, 0, array, 0, bits.Length );
      //}

      internal UInt32[] GetArrayDirect()
      {
         return this._bits ?? Empty<UInt32>.Array;
      }

      public BigInteger Absolute()
      {
         return this.IsNegative() ? -this : this;
      }

      public Int32 CompareTo( BigInteger other )
      {
         var retVal = this.Sign.CompareTo( other.Sign );
         if ( retVal == 0 && this.Sign != 0 )
         {
            retVal = Compare( this._bits, other._bits );
         }
         return retVal;
      }

      public void WriteToByteArray( Byte[] array, Int32 offset, Int32 count, BinaryEndianness endianness, Boolean includeSign = true )
      {
         array.CheckArrayArguments( offset, count );

         if ( count > 0 )
         {
            if ( this.IsZero )
            {
               if ( includeSign )
               {
                  array[offset] = 0;
               }
            }
            else
            {
               switch ( endianness )
               {
                  case BinaryEndianness.LittleEndian:
                     this.WriteBitsLE( array, offset, count, includeSign );
                     break;
                  case BinaryEndianness.BigEndian:
                     this.WriteBitsBE( array, offset, count, includeSign );
                     break;
                  default:
                     throw new ArgumentException( "Unrecognized endianness enum: " + endianness );
               }
            }
         }
      }

      public Boolean Equals( BigInteger another )
      {
         return this.CompareTo( another ) == 0;
      }

      public override Boolean Equals( Object obj )
      {
         return obj is BigInteger && this.CompareTo( (BigInteger) obj ) == 0;
      }

      public override Int32 GetHashCode()
      {
         var retVal = ArrayEqualityComparer<UInt32>.GetHashCode( this._bits );
         var sign = this._sign;
         if ( sign != 0 )
         {
            retVal = unchecked(retVal * sign);
         }
         return retVal;
      }

      public override String ToString()
      {
         return ToString( this._sign, this._bits, this._bits.Length );
      }

      internal static String ToString( Int32 sign, UInt32[] bits, Int32 bitsLength )
      {
         UInt32 smallValue;
         String retVal;
         var isNegative = sign < 0;
         if ( Calculations.BigIntegerCalculations.TryGetSmallValue( bits, ref bitsLength, out smallValue ) )
         {
            retVal = smallValue.ToString();
            if ( isNegative )
            {
               retVal = "-" + retVal;
            }
         }
         else
         {
            // The largest possible 10^x number suitable in UInt32 is 10^9: 1000000000
            // So we first convert to that, and then we convert the individual ints using the traditional algorithm
            const UInt32 convBase = 1000000000;
            const Int32 convBasePower = 9;
            try
            {
               var newBits = new UInt32[bitsLength * 10 / 9 + 2];
               var newBitsIdx = 0;

               for ( var i = bitsLength - 1; i >= 0; --i )
               {
                  var carry = bits[i];
                  for ( var j = 0; j < newBitsIdx; ++j )
                  {
                     var cur = Calculations.BigIntegerCalculations.ToUInt64( newBits[j], carry );
                     unchecked
                     {
                        newBits[j] = (UInt32) ( cur % convBase );
                        carry = (UInt32) ( cur / convBase );
                     }
                  }

                  if ( carry != 0 )
                  {
                     newBits[newBitsIdx++] = carry % convBase;
                     carry /= convBase;
                     if ( carry != 0 )
                     {
                        newBits[newBitsIdx++] = carry;
                     }
                  }
               }

               // Each integer in newBits takes at most 9 characters in base10
               var charCount = newBitsIdx * convBasePower;
               if ( isNegative )
               {
                  // Space for '-'
                  ++charCount;
               }

               // Create char array and populate it using traditional algorithm
               var chars = new Char[charCount];
               var charIdx = charCount;
               for ( var i = 0; i < newBitsIdx; ++i )
               {
                  var cur = newBits[i];
                  // For last iteration of newBits array, don't produce extra zeroes.
                  for ( var j = convBasePower - 1; j >= 0 && ( i < newBitsIdx - 1 || cur != 0 ); --j )
                  {
                     chars[--charIdx] = (Char) ( cur % 10 + '0' );
                     cur /= 10;
                  }
               }

               // Set minus sign, if needed
               if ( isNegative )
               {
                  chars[--charIdx] = '-';
               }

               retVal = new String( chars, charIdx, charCount - charIdx );
            }
            catch ( OverflowException oe )
            {
               throw new FormatException( "This integer is too big for .ToString()", oe );
            }
         }

         return retVal;
      }

      internal Int32 GetSerializedByteCount( Boolean includeSign )
      {
         return BinaryUtils.AmountOfPagesTaken(
            includeSign || this.Sign < 0 ? ( this.BitLength + 1 ) : this.BitLength, // Amount of bits
            BITS_BYTE ); // Bits per byte
      }


      #endregion

      #region Static methods

      public static BigInteger? ParseFromBinaryOrNull( Byte[] array, BinaryEndianness endianness, Int32 sign )
      {
         return array == null ? (BigInteger?) null : ParseFromBinary( array, endianness, sign );
      }

      public static BigInteger? ParseFromBinaryOrNull( Byte[] array, Int32 offset, Int32 length, BinaryEndianness endianness, Int32 sign )
      {
         return array == null ? (BigInteger?) null : ParseFromBinary( array, offset, length, endianness, sign );
      }

      public static BigInteger ParseFromBinary( Byte[] array, BinaryEndianness endianness, Int32 sign )
      {
         return ParseFromBinary( array, 0, array.Length, endianness, sign );
      }

      public static BigInteger ParseFromBinary( Byte[] array, Int32 offset, Int32 length, BinaryEndianness endianness, Int32 sign )
      {
         UInt32[] bits; Int32 actualSign;
         ParseFromBinary( array, offset, length, endianness, sign, out actualSign, out bits );
         return new BigInteger( actualSign, bits, false );
      }

      private static void ParseFromBinary( Byte[] array, Int32 offset, Int32 length, BinaryEndianness endianness, Int32 sign, out Int32 signResult, out UInt32[] bits )
      {
         if ( ValidateSign( sign ) == 0 )
         {
            signResult = 0;
            bits = Empty<UInt32>.Array;
         }
         else
         {
            if ( sign < 0 )
            {
               throw new NotImplementedException( "Implement two-complement deserialization" );
            }

            bits = ParseBits( array, offset, length, endianness );
            signResult = bits.Length < 1 ? 0 : sign;
         }
      }

      public static BigInteger Remainder( BigInteger divident, BigInteger divisor )
      {
         return divident % divisor;
      }

      public static BigInteger Subtract( BigInteger left, BigInteger right )
      {
         return left - right;
      }

      public static BigInteger Add( BigInteger left, BigInteger right )
      {
         return left + right;
      }

      public static BigInteger Multiply( BigInteger left, BigInteger right )
      {
         return left * right;
      }

      public static BigInteger Divide( BigInteger left, BigInteger right )
      {
         return left / right;
      }

      private static Int32 ValidateSign( Int32 sign, UInt32[] bits )
      {
         return bits.IsNullOrEmpty() ? 0 : ValidateSign( sign );
      }

      private static Int32 ValidateSign( Int32 sign )
      {
         if ( sign < -1 || sign > 1 )
         {
            throw new FormatException( "The sign only accepts 3 values: -1, 0, and 1." );
         }
         return sign;
      }

      private static UInt32[] ParseBits(
         Byte[] bytes,
         Int32 offset,
         Int32 count,
         BinaryEndianness endianness
         )
      {
         bytes.CheckArrayArguments( offset, count );
         var max = offset + count;
         UInt32[] bits;
         switch ( endianness )
         {
            case BinaryEndianness.BigEndian:
               bits = ParseBitsBE( bytes, offset, max );
               break;
            case BinaryEndianness.LittleEndian:
               bits = ParseBitsLE( bytes, offset, max );
               break;
            default:
               throw new ArgumentException( "Unrecognized endianness enum: " + endianness );
         }

         return bits;
      }

      private static UInt32[] ParseBitsBE(
         Byte[] bytes,
         Int32 offset,
         Int32 max
         )
      {
         // Skip leading zeroes.
         while ( offset < max && bytes[offset] == 0 )
         {
            ++offset;
         }

         var byteCount = max - offset;

         UInt32[] bits;
         if ( byteCount < 1 )
         {
            bits = Empty<UInt32>.Array;
         }
         else
         {
            var intCount = BinaryUtils.AmountOfPagesTaken( byteCount, sizeof( UInt32 ) );
            bits = new UInt32[intCount];
            // The index to start writing into int array is the last element of the int array
            // We still iterate the byte array normally
            var idx = intCount - 1;
            UInt32 firstInt;
            switch ( byteCount % sizeof( UInt32 ) )
            {
               case 0:
                  // One full integer left
                  firstInt = bytes.ReadUInt32BEFromBytes( ref offset );
                  break;
               case 1:
                  // One byte left
                  firstInt = bytes.ReadByteFromBytes( ref offset );
                  break;
               case 2:
                  // Two bytes left
                  firstInt = bytes.ReadUInt16BEFromBytes( ref offset );
                  break;
               default:
                  // Three bytes left
                  firstInt = ( (UInt32) bytes.ReadUInt16BEFromBytes( ref offset ) << 8 ) | bytes.ReadByteFromBytes( ref offset );
                  break;
            }
            bits[idx--] = firstInt;

            while ( idx >= 0 )
            {
               bits[idx--] = bytes.ReadUInt32BEFromBytes( ref offset );
            }


         }

         System.Diagnostics.Debug.Assert( offset == max, "All bytes must've been read." );

         return bits;
      }

      private static UInt32[] ParseBitsLE(
         Byte[] bytes,
         Int32 offset,
         Int32 max
         )
      {
         // Skip leading (in LE format) zeroes
         while ( offset < max && bytes[max - 1] == 0 )
         {
            --max;
         }

         var byteCount = max - offset;

         UInt32[] bits;
         if ( byteCount < 1 )
         {
            bits = Empty<UInt32>.Array;
         }
         else
         {
            var intCount = BinaryUtils.AmountOfPagesTaken( byteCount, sizeof( UInt32 ) );
            bits = new UInt32[intCount];
            // Index of int array where we will write
            var idx = 0;
            // Read the bytes
            while ( idx < intCount - 1 )
            {
               bits[idx++] = bytes.ReadUInt32LEFromBytes( ref offset );
            }

            // Read last integer
            switch ( byteCount % sizeof( UInt32 ) )
            {
               case 0:
                  // The amount of bytes is even - start from first integer
                  break;
               case 1:
                  // One extra byte
                  bits[idx++] = bytes.ReadByteFromBytes( ref offset );
                  break;
               case 2:
                  // Two extra bytes
                  bits[idx++] = bytes.ReadUInt16LEFromBytes( ref offset );
                  break;
               case 3:
                  // Three extra bytes
                  bits[idx++] = ( (UInt32) bytes.ReadUInt16LEFromBytes( ref offset ) ) | ( (UInt32) bytes.ReadByteFromBytes( ref offset ) << 16 );
                  break;
            }


         }

         System.Diagnostics.Debug.Assert( offset == max, "All bytes must've been read." );

         return bits;
      }

      // This method assumes this is not zero
      private void WriteBitsLE( Byte[] array, Int32 offset, Int32 count, Boolean includeSign )
      {
         var sign = this.Sign;
         if ( sign < 0 )
         {
            throw new NotImplementedException( "Implement two-complement LE serialization." );
         }

         var bits = this._bits;
         // Write whole integers as much as we can first
         var bytesForWholeIntegers = count - offset;
         var wholeIntegerCount = Math.Min( this.BitsArrayLength - 1, ( count - offset ) / BYTES_32 );
         var bitsIdx = 0;
         var isTwoComplement = includeSign && sign < 0;
         var carry = isTwoComplement;
         for ( ; bitsIdx < wholeIntegerCount; ++bitsIdx )
         {
            var cur = bits[bitsIdx];
            if ( isTwoComplement )
            {
               MakeTwoComplement( ref cur, ref carry );
            }

            array.WriteUInt32LEToBytes( ref offset, cur );
         }

         // Write last integer
         var lastInteger = bits[bitsIdx];
         if ( isTwoComplement )
         {
            MakeTwoComplement( ref lastInteger, ref carry );
         }
         var roomForLastInteger = count - offset;
         var needExtraByte = includeSign && (
            carry // We have all bits set in all integers, and this is negative value
            || ( sign > 0 && ( lastInteger & 0x80000000u ) != 0 ) // This is positive value, and highest bit is set
            );
         if ( needExtraByte )
         {
            --roomForLastInteger;
         }
         if ( roomForLastInteger > 0 )
         {
            var lastIntegerBytes = roomForLastInteger % BYTES_32;
            switch ( lastIntegerBytes )
            {
               case 0:
                  // Room to write full integer
                  array.WriteUInt32LEToBytes( ref offset, lastInteger );
                  lastIntegerBytes = 4;
                  break;
               case 1:
                  // Room for one byte - write highermost byte
                  throw new NotImplementedException();
               case 2:
                  // Room for two bytes - write two highermost bytes
                  throw new NotImplementedException();
               default:
                  // Room for three bytes - write three highermost bytes
                  throw new NotImplementedException();
            }
            roomForLastInteger -= lastIntegerBytes;
         }

         // Write sign, if needed
         if ( roomForLastInteger > 0 && includeSign )
         {
            Int32 newByte;
            if ( needExtraByte )
            {
               newByte = sign > 0 ? 0x00 : 0xFF;
               array[offset] = (Byte) newByte;
            }
            else if ( sign < 0 )
            {
               var oldByte = array[offset];
               newByte = oldByte | 0x80;
               array[offset] = (Byte) newByte;
            }
         }
      }

      private void MakeTwoComplement( ref UInt32 current, ref Boolean carry )
      {
         current = ~current;
         if ( carry )
         {
            carry = unchecked(++current) == 0;
         }
      }

      private void WriteBitsBE( Byte[] array, Int32 offset, Int32 count, Boolean includeSign )
      {
         throw new NotImplementedException( "Implement BE serialization." );
      }

      private static Int32 CalculateBitLength( UInt32[] bits )
      {
         Int32 retVal;
         if ( bits == null )
         {
            retVal = 0;
         }
         else
         {
            // This class never has trailing zeroes
            retVal = BITS_32 * ( bits.Length - 1 ) // Amount of bits in other integers
               + BinaryUtils.Log2( bits[bits.Length - 1] ); // Amount of bits in last integer
         }
         return retVal;
      }

      // We must return always either -1, 0, or 1, since the result of this method is used in multiplication operations for sign
      private static Int32 Compare( UInt32[] xBits, UInt32[] yBits )
      {
         return Calculations.BigIntegerCalculations.CompareBits( xBits, xBits.Length, yBits, yBits.Length );
      }


      private static UInt32[] CheckForTrailingZeroes( UInt32[] bits, Int32 sign, out Int32 signResult )
      {
         var i = CheckForTrailingZeroes( bits );

         if ( i <= 0 )
         {
            bits = Empty<UInt32>.Array;
            signResult = 0;
         }
         else
         {
            signResult = sign;
            if ( i < bits.Length - 1 )
            {
               var tmp = bits;
               bits = new UInt32[i + 1];
               Array.Copy( tmp, 0, bits, 0, i + 1 );
            }
         }

         return bits;
      }

      private static Int32 CheckForTrailingZeroes( UInt32[] bits )
      {
         var i = bits.Length - 1;
         while ( i >= 0 && bits[i] == 0 )
         {
            --i;
         }
         return Math.Max( 0, i );
      }


      private static void PerformDivMod(
         BigInteger divident,
         BigInteger divisor,
         Boolean computeModulus,
         Boolean computeQuotient,
         ref BigInteger modulusResult,
         ref BigInteger quotientResult
         )
      {
         if ( divisor.IsZero )
         {
            throw new ArithmeticException( "Division by zero." );
         }
         else if ( divident.IsZero )
         {
            if ( computeQuotient )
            {
               quotientResult = divident;
            }
            if ( computeModulus )
            {
               modulusResult = divident;
            }
         }
         else if ( divisor.IsOne )
         {
            if ( computeQuotient )
            {
               quotientResult = divident;
            }
            if ( computeModulus )
            {
               modulusResult = Zero;
            }
         }
         else if ( divisor.IsMinusOne )
         {
            if ( computeQuotient )
            {
               quotientResult = -divident;
            }
            if ( computeModulus )
            {
               modulusResult = Zero;
            }
         }
         else
         {
            var dividentBits = divident._bits.CreateArrayCopy();
            Int32 dividentLength = dividentBits.Length;
            var sign = divident.Sign;
            //if ( computeModulus && !computeQuotient )
            //{
            //   Calculations.BigIntegerCalculations.Modulus( dividentBits, ref dividentLength, divisor );
            //   modulusResult = new BigInteger( sign, dividentBits, dividentLength );
            //}
            //else
            //{
            UInt32[] quotient = null;
            Int32 quotientLength = -1;
            Int32 quotientSign = -1;
            Calculations.BigIntegerCalculations.DivideWithRemainder( dividentBits, ref dividentLength, sign, divisor._bits, divisor._bits.Length, divisor.Sign, ref quotient, ref quotientLength, ref quotientSign, computeQuotient );
            if ( computeModulus )
            {
               modulusResult = new BigInteger( sign, dividentBits, dividentLength );
            }
            if ( computeQuotient )
            {
               quotientResult = new BigInteger( quotientSign, quotient, quotientLength );
            }

            //}
         }
      }

      private static Boolean AreSmallBits( UInt32[] bits )
      {
         return bits == null || bits.Length <= 1;
      }

      #endregion

      #region Operators

      public static BigInteger operator +( BigInteger left, BigInteger right )
      {
         BigInteger retVal;
         if ( right.IsZero )
         {
            retVal = left;
         }
         else if ( left.IsZero )
         {
            retVal = right;
         }
         else
         {
            UInt32[] resultBits = null;
            var resultLength = -1;
            Int32 resultSign;
            Calculations.BigIntegerCalculations.Add( left._bits, left._bits.Length, left.Sign, right._bits, right._bits.Length, right.Sign, ref resultBits, ref resultLength, out resultSign );
            retVal = new BigInteger( resultSign, resultBits, resultLength );
         }

         return retVal;
      }

      public static BigInteger operator -( BigInteger left, BigInteger right )
      {
         BigInteger retVal;
         if ( right.IsZero )
         {
            retVal = left;
         }
         else if ( left.IsZero )
         {
            retVal = -right;
         }
         else
         {
            UInt32[] resultBits = null;
            var resultLength = -1;
            Int32 resultSign;
            Calculations.BigIntegerCalculations.Subtract( left._bits, left._bits.Length, left.Sign, right._bits, right._bits.Length, right.Sign, ref resultBits, ref resultLength, out resultSign );
            retVal = new BigInteger( resultSign, resultBits, resultLength );
         }
         return retVal;
      }

      public static BigInteger operator -( BigInteger x )
      {
         return new BigInteger( -x.Sign, x._bits, false );
      }

      public static BigInteger operator %( BigInteger divident, BigInteger divisor )
      {
         BigInteger modulus = default( BigInteger ), quotient = default( BigInteger );
         PerformDivMod( divident, divisor, true, false, ref modulus, ref quotient );
         return modulus;
      }

      public static BigInteger operator /( BigInteger divident, BigInteger divisor )
      {
         BigInteger modulus = default( BigInteger ), quotient = default( BigInteger );
         PerformDivMod( divident, divisor, false, true, ref modulus, ref quotient );
         return quotient;
      }

      public static BigInteger operator *( BigInteger left, BigInteger right )
      {
         BigInteger retVal;
         if ( left.IsZero || right.IsZero )
         {
            retVal = Zero;
         }
         else if ( left.IsOne )
         {
            retVal = right;
         }
         else if ( left.IsMinusOne )
         {
            retVal = -right;
         }
         else if ( right.IsOne )
         {
            retVal = left;
         }
         else if ( right.IsMinusOne )
         {
            retVal = -left;
         }
         else
         {
            UInt32[] resultBits = null;
            var resultLength = -1;
            Int32 resultSign;
            Calculations.BigIntegerCalculations.Multiply( left._bits, left._bits.Length, left.Sign, right._bits, right._bits.Length, right.Sign, ref resultBits, ref resultLength, out resultSign );
            retVal = new BigInteger(
               resultSign,
               resultBits,
               resultLength
               );
         }
         return retVal;
      }

      public static Boolean operator >( BigInteger left, BigInteger right )
      {
         return left.CompareTo( right ) > 0;
      }

      public static Boolean operator <( BigInteger left, BigInteger right )
      {
         return left.CompareTo( right ) < 0;
      }

      public static Boolean operator >=( BigInteger left, BigInteger right )
      {
         return left.CompareTo( right ) >= 0;
      }

      public static Boolean operator <=( BigInteger left, BigInteger right )
      {
         return left.CompareTo( right ) <= 0;
      }

      public static Boolean operator ==( BigInteger left, BigInteger right )
      {
         return left.CompareTo( right ) == 0;
      }

      public static Boolean operator !=( BigInteger left, BigInteger right )
      {
         return left.CompareTo( right ) != 0;
      }

      #endregion

      #region Casts

      public static implicit operator BigInteger( Int32 value )
      {
         return new BigInteger( value );
      }

      [CLSCompliant( false )]
      public static implicit operator BigInteger( UInt32 value )
      {
         return new BigInteger( value );
      }

      public static implicit operator BigInteger( Int64 value )
      {
         return new BigInteger( value );
      }

      [CLSCompliant( false )]
      public static implicit operator BigInteger( UInt64 value )
      {
         return new BigInteger( value );
      }

      #endregion
   }

}

public static partial class E_CILPhysical
{


   // Same as Remainder, but always returns positive
   public static BigInteger Remainder_Positive( this BigInteger divident, BigInteger divisor )
   {
      if ( divisor.IsNegative() )
      {
         throw new ArgumentException( "Divisor must be positive." );
      }
      var retVal = divident % divisor;
      return retVal.Sign >= 0 ? retVal : ( retVal + divisor );
   }

   public static BigInteger ModInverse( this BigInteger value, BigInteger modulus )
   {
      if ( modulus.IsNegative() )
      {
         throw new ArithmeticException( "Modulus must be positive." );
      }

      BigInteger x, y;
      var gcd = ( value % modulus ).ExtendedEuclidean( modulus, out x, out y, false );
      if ( !gcd.IsOne )
      {
         throw new ArithmeticException( "The modular multiplicative inverse does not exist." );
      }

      if ( x.IsNegative() )
      {
         x += modulus;
      }

      return x;
   }

   internal static Boolean IsNegative( this BigInteger integer )
   {
      return integer.Sign < 0;
   }

   internal static Boolean IsPositive( this BigInteger integer )
   {
      return integer.Sign > 0;
   }

   public static Byte[] ToByteArray( this BigInteger integer, BinaryEndianness endianness, Boolean includeSign = true )
   {
      var retVal = new Byte[integer.GetSerializedByteCount( includeSign )];
      integer.WriteToByteArray( retVal, endianness, includeSign );
      return retVal;
   }

   public static void WriteToByteArray( this BigInteger integer, Byte[] array, BinaryEndianness endianness, Boolean includeSign = true )
   {
      integer.WriteToByteArray( array, 0, array.Length, endianness, includeSign );
   }

   public static BigInteger NextBigInt( this Random random, BigInteger min, BigInteger max )
   {
      // Compare only once
      var cmp = min.CompareTo( max );
      BigInteger retVal;
      if ( cmp > 0 )
      {
         throw new ArgumentException( "Max value should be at least as min value." );
      }
      else if ( cmp == 0 )
      {
         retVal = min;
      }
      else
      {
         retVal = min + CreateRandom( ( max - min ).BitLength - 1, random );
      }

      System.Diagnostics.Debug.Assert( retVal >= min && retVal <= max, "Returned random integer must be in correct range." );

      return retVal;
   }

   private static BigInteger CreateRandom( Int32 bitLength, Random random )
   {
      // Create and populate bytes
      var byteCount = BinaryUtils.AmountOfPagesTaken( bitLength, 8 );
      var bytes = new Byte[byteCount];
      random.NextBytes( bytes );

      // Strip excess bits
      var excessBits = byteCount * 8 - bitLength;
      bytes[0] &= (Byte) ( Byte.MaxValue >> excessBits );

      // Create BigInteger
      return BigInteger.ParseFromBinary( bytes, BinaryEndianness.BigEndian, 1 );
   }
}

#pragma warning restore 1591