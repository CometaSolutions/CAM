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
   public struct BigInteger : IComparable<BigInteger>, IEquatable<BigInteger>
   {

      public enum BinaryEndianness
      {
         LittleEndian,
         BigEndian
      }

      #region Static properties

      private static readonly LocklessInstancePoolForClasses<Byte[]> ArraysForNativeIntegers;

      public static BigInteger One { get; }

      public static BigInteger Zero { get; }

      public static BigInteger MinusOne { get; }

      #endregion

      #region Static constructor

      static BigInteger()
      {
         ArraysForNativeIntegers = new LocklessInstancePoolForClasses<Byte[]>();
         One = new BigInteger( 1 );
         Zero = new BigInteger( 0 );
         MinusOne = new BigInteger( -1 );
      }

      #endregion

      #region Instance fields

      private readonly Int32 _sign;
      private readonly UInt32[] _bits;

      #endregion

      #region Instance constructors

      public BigInteger( Int32 intValue )
      {
         var arrays = ArraysForNativeIntegers;
         var array = arrays.TakeInstance();
         if ( array == null )
         {
            array = new Byte[sizeof( Int64 )];
         }

         try
         {
            array.WriteInt32BEToBytesNoRef( 0, Math.Abs( intValue ) );
            ParseFromBinary( array, 0, sizeof( Int32 ), BinaryEndianness.BigEndian, intValue < 0 ? -1 : ( intValue > 0 ? 1 : 0 ), out this._sign, out this._bits );
         }
         finally
         {
            arrays.ReturnInstance( array );
         }
      }

      private BigInteger( Int32 bitLength, Random random )
      {
         throw new NotImplementedException();
      }

      private BigInteger( Int32 sign, UInt32[] bits )
      {
         this._sign = ValidateSign( sign );
         this._bits = sign == 0 ? Empty<UInt32>.Array : bits;
      }

      #endregion

      #region Instance properties

      public Int32 BitLength
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public Int32 ByteLength
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
            return this._sign == 0;
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
         throw new NotImplementedException();
      }

      // Same as Remainder, but always returns positive
      public BigInteger Remainder_Positive( BigInteger other )
      {
         throw new NotImplementedException();
      }

      public BigInteger ModPow( BigInteger exponent, BigInteger modulus )
      {
         throw new NotImplementedException();
      }

      public BigInteger ModInverse( BigInteger x )
      {
         throw new NotImplementedException();
      }

      public Int32 CompareTo( BigInteger other )
      {
         throw new NotImplementedException();
      }

      public void WriteToByteArray( Byte[] array, Int32 offset, BinaryEndianness endianness )
      {
         throw new NotImplementedException();
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
         throw new NotImplementedException();
      }

      public override String ToString()
      {
         String retVal;
         if ( this.IsZero )
         {
            retVal = "0";
         }
         else
         {
            // TODO compile CAM with /checked

            // The largest possible 10^x number suitable in UInt32 is 10^9: 1000000000
            // So we first convert to that, and then we convert the individual ints using the traditional algorithm
            const UInt32 convBase = 1000000000;
            const Int32 convBasePower = 9;
            var thisByteLen = this.ByteLength;
            var thisBits = this._bits;
            var newBits = new UInt32[thisByteLen * 10 / 9 + 2];
            var newBitsIdx = 0;

            for ( var i = thisByteLen - 1; i >= 0; --i )
            {
               var carry = thisBits[i];
               for ( var j = 0; j < newBitsIdx; ++j )
               {
                  var cur = ToUInt64( newBits[j], carry );
                  newBits[j] = (UInt32) ( cur % convBase );
                  carry = (UInt32) ( cur / convBase );
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
            var isNegative = this._sign < 0;
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

         return retVal;
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
         return new BigInteger( actualSign, bits );
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
            bits = ParseBits( array, offset, length, endianness );
            signResult = bits.Length < 1 ? 0 : sign;
         }
      }

      // max is inclusive!!
      public static BigInteger CreateRandomInRange( BigInteger min, BigInteger max, Random random )
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
            if ( max.BitLength / 2 < min.BitLength )
            {
               // The difference is small enough
               retVal = min + CreateRandomInRange( Zero, max - min, random );
            }
            else
            {
               // Slower, but more entropy
               var i = 0;
               Boolean valid;
               do
               {
                  retVal = new BigInteger( max.BitLength, random );
                  valid = retVal >= min && retVal < max;
               } while ( !valid && ( ++i ) < 100 );

               if ( !valid )
               {
                  // Faster, but less entropy
                  retVal = min + new BigInteger( ( max - min ).BitLength - 1, random );
               }
            }
         }

         return retVal;
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

            // Index of int array where we will write
            var idx = 0;
            // Start with the remainder
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
                  bits[idx++] = bytes.ReadUInt16BEFromBytes( ref offset );
                  break;
               case 3:
                  // Three extra bytes
                  bits[idx++] = ( (UInt32) bytes.ReadUInt16BEFromBytes( ref offset ) << 8 ) | bytes.ReadByteFromBytes( ref offset );
                  break;
            }

            // Read the rest of the bytes
            while ( idx < intCount )
            {
               bits[idx++] = bytes.ReadUInt32BEFromBytes( ref offset );
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
            // The index to start writing into int array is the last element of the int array
            // We still iterate the byte array normally
            var idx = intCount - 1;
            while ( idx > 0 )
            {
               bits[idx--] = bytes.ReadUInt32LEFromBytes( ref offset );
            }

            UInt32 lastInt;
            switch ( byteCount % sizeof( UInt32 ) )
            {
               case 0:
                  // One full integer left
                  lastInt = bytes.ReadUInt32LEFromBytes( ref offset );
                  break;
               case 1:
                  // One byte left
                  lastInt = bytes.ReadByteFromBytes( ref offset );
                  break;
               case 2:
                  // Two bytes left
                  lastInt = bytes.ReadUInt16LEFromBytes( ref offset );
                  break;
               default:
                  // Three bytes left
                  lastInt = ( (UInt32) bytes.ReadByteFromBytes( ref offset ) << 8 ) | bytes.ReadUInt16LEFromBytes( ref offset );
                  break;
            }
            bits[0] = lastInt;
         }

         System.Diagnostics.Debug.Assert( offset == max, "All bytes must've been read." );

         return bits;
      }

      private static UInt64 ToUInt64( UInt32 high, UInt32 low )
      {
         return ( ( (UInt64) high ) << ( sizeof( UInt32 ) * 8 ) ) | low;
      }

      #endregion

      #region Operators

      public static BigInteger operator %( BigInteger divident, BigInteger divisor )
      {
         throw new NotImplementedException();
      }

      public static BigInteger operator *( BigInteger left, BigInteger right )
      {
         throw new NotImplementedException();
      }

      public static BigInteger operator -( BigInteger left, BigInteger right )
      {
         throw new NotImplementedException();
      }

      public static BigInteger operator +( BigInteger left, BigInteger right )
      {
         throw new NotImplementedException();
      }

      public static BigInteger operator /( BigInteger left, BigInteger right )
      {
         throw new NotImplementedException();
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

      #endregion
   }

}

#pragma warning restore 1591