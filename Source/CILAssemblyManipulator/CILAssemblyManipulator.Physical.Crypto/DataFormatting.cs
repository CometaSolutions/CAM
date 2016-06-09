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
using UtilPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilPack.Numerics;

namespace CILAssemblyManipulator.Physical.Crypto
{
   /// <summary>
   /// This interface represents a formatter for data to be encrypted.
   /// </summary>
   public interface DataFormatter
   {
      /// <summary>
      /// Gets the size of the data.
      /// </summary>
      /// <value>The size of the data.</value>
      Int32 DataSize { get; }

      /// <summary>
      /// Creates the data used for signing.
      /// </summary>
      /// <param name="data">The array where to write the data to.</param>
      /// <param name="dataOffset">The offset in <paramref name="data"/> where to start writing.</param>
      /// <returns>The binary data that can be encrypted.</returns>
      void PopulateData( Byte[] data, Int32 dataOffset );
   }

   /// <summary>
   /// The formatter which uses ASN DER encoding to format the hash.
   /// </summary>
   internal sealed class ASNFormatter : DataFormatter
   {
      /// <summary>
      /// Creates a new instance of <see cref="ASNFormatter"/> with given parameters.
      /// </summary>
      /// <param name="hash">The hash.</param>
      /// <param name="hashAlgorithmID">The <see cref="AssemblyHashAlgorithm"/>.</param>
      /// <returns>A new instance of <see cref="ASNFormatter"/> with given parameters.</returns>
      public static ASNFormatter Create( Byte[] hash, ASN1ObjectIdentifier hashAlgorithmID )
      {
         return new ASNFormatter( hash, hashAlgorithmID );
      }

      private ASN1Sequence _sequence;

      private ASNFormatter( Byte[] contents, ASN1ObjectIdentifier hashAlgorithmID )
      {
         // The data to encrypt is SEQUENCE(SEQUENCE(algorithmID, NULL), message)
         this._sequence = new ASN1Sequence(
            new ASN1Sequence(
               hashAlgorithmID,
               ASN1Null.Instance
               ),
            new ASN1OctetString( contents )
            );
         this.DataSize = this._sequence.CalculateLength();
      }

      /// <inheritdoc />
      public Int32 DataSize { get; }

      /// <summary>
      /// Encodes algorithm ID + message using ASN.1 DER encoding to produce the data to be encrypted.
      /// </summary>
      /// <param name="data">The array where to write the data to.</param>
      /// <param name="dataOffset">The offset in <paramref name="data"/> where to start writing.</param>
      /// <returns>The data to be used in RSA encryption.</returns>
      public void PopulateData( Byte[] data, Int32 dataOffset )
      {
         this._sequence.Serialize( data, dataOffset, this.DataSize );
      }
   }

   /// <summary>
   /// The formatter which uses PKCS1 encoding to encode the data.
   /// </summary>
   public class PKCS1Encoder : DataFormatter
   {

      /// <summary>
      /// Creates new instance of <see cref="PKCS1Encoder"/>.
      /// </summary>
      /// <param name="rsaKeySize">The size of RSA key, in bits.</param>
      /// <param name="formatter">The <see cref="DataFormatter"/> actually formatting the data.</param>
      /// <returns>A new instance of <see cref="PKCS1Encoder"/>.</returns>
      public static PKCS1Encoder Create( Int32 rsaKeySize, DataFormatter formatter )
      {
         return new PKCS1Encoder( rsaKeySize, formatter );
      }

      private readonly DataFormatter _formatter;

      private PKCS1Encoder( Int32 rsaKeySize, DataFormatter formatter )
      {
         // TODO Check contents length ( <= dataSize - 10)
         this.DataSize = rsaKeySize / 8;
         this._formatter = formatter;
      }

      /// <inheritdoc />
      public Int32 DataSize { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="data">The array where to write the data to.</param>
      /// <param name="dataOffset">The offset in <paramref name="data"/> where to start writing.</param>
      public void PopulateData( Byte[] data, Int32 dataOffset )
      {
         var actualFormatter = this._formatter;
         var encodedContentsSize = actualFormatter.DataSize;
         Int32 padCount;
         data
            .WriteByteToBytes( ref dataOffset, 0x00 ) // Leading zero
            .WriteByteToBytes( ref dataOffset, 0x01 ) // Type code 1
            .FillWithOffsetAndCount( dataOffset, ( padCount = this.DataSize - dataOffset - encodedContentsSize - 1 ), (Byte) 0xFF ); // Pad with 0xFF
         dataOffset += padCount;
         data.WriteByteToBytes( ref dataOffset, 0x00 ); // Mark end of the padding

         // Write the actual data
         actualFormatter.PopulateData( data, dataOffset );
      }
   }

   internal abstract class ASN1Component
   {
      // TODO this is a hack
      private Int32 _contentLength;
      private Int32 _lengthLength;

      public Int32 CalculateLength()
      {
         var len = this.CalculateContentLength();
         this._contentLength = len;
         this._lengthLength = len.CalculateDERComponentLengthSize();
         return 1 // Tag
            + this._lengthLength // How many bytes to encode length of data
            + len; // The length of data
      }

      public void Serialize( Byte[] array, Int32 idx, Int32 calculatedLength )
      {
         array
            .WriteByteToBytes( ref idx, this.GetTag() )
            .WriteDERComponentLength( ref idx, this._contentLength, this._lengthLength );
         this.SerializeContent( array, idx, calculatedLength );
      }

      protected abstract Int32 CalculateContentLength();
      protected abstract Byte GetTag();

      protected abstract void SerializeContent( Byte[] array, Int32 idx, Int32 calculatedLength );
   }

   internal sealed class ASN1ObjectIdentifier : ASN1Component
   {
      private readonly Lazy<String> _stringValue;
      private readonly Lazy<Byte[]> _bytes;

      public ASN1ObjectIdentifier( IEnumerable<Int64> identifier )
      {
         this.Integers = identifier.Cast<Object>().ToArray();
         this._stringValue = new Lazy<String>( () => String.Join( ",", this.Integers ), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );
         this._bytes = new Lazy<Byte[]>( () => this.Integers.SerializeObjectIdentifier(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );
      }

      protected override Int32 CalculateContentLength()
      {
         return this.Bytes.Length;
      }

      protected override Byte GetTag()
      {
         return 0x06; // OBJECT IDENTIFIER
      }

      protected override void SerializeContent( byte[] array, int idx, int calculatedLength )
      {
         array.BlockCopyFrom( ref idx, this.Bytes );
      }

      public String StringValue
      {
         get
         {
            return this._stringValue.Value;
         }
      }

      // TODO ArrayQuery...
      public Byte[] Bytes
      {
         get
         {
            return this._bytes.Value;
         }
      }

      // TODO ArrayQuery...
      public Object[] Integers { get; }

      public override String ToString()
      {
         return this._stringValue.Value;
      }
   }

   internal sealed class ASN1OctetString : ASN1Component
   {
      private readonly Byte[] _octetString;

      public ASN1OctetString( Byte[] octetString )
      {
         this._octetString = ArgumentValidator.ValidateNotNull( "Octet string", octetString );
      }

      protected override Int32 CalculateContentLength()
      {
         return this._octetString.Length;
      }

      protected override Byte GetTag()
      {
         return 0x04; // OCTET STRING
      }

      protected override void SerializeContent( Byte[] array, Int32 idx, Int32 calculatedLength )
      {
         array.BlockCopyFrom( ref idx, this._octetString );
      }
   }

   internal sealed class ASN1Sequence : ASN1Component
   {
      private readonly ASN1Component[] _sequence;
      private readonly Lazy<Int32[]> _lengths;

      public ASN1Sequence( params ASN1Component[] sequence )
      {
         this._sequence = ArgumentValidator.ValidateNotNull( "ASN1 sequence", sequence );
         this._lengths = new Lazy<Int32[]>( () => this._sequence.Select( s => s.CalculateLength() ).ToArray(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );
      }

      protected override Int32 CalculateContentLength()
      {
         return this._lengths.Value.Sum();
      }

      protected override Byte GetTag()
      {
         return 0x10 | 0x20; // SEQUENCE | CONSTRUCTED
      }

      protected override void SerializeContent( Byte[] array, Int32 idx, Int32 calculatedLength )
      {
         var seq = this._sequence;
         var lengths = this._lengths.Value;
         for ( var i = 0; i < seq.Length; ++i )
         {
            var len = lengths[i];
            seq[i].Serialize( array, idx, len );
            idx += len;
         }
      }


   }

   internal sealed class ASN1Null : ASN1Component
   {
      public static ASN1Null Instance { get; }

      static ASN1Null()
      {
         Instance = new ASN1Null();
      }

      private ASN1Null()
      {

      }

      protected override Int32 CalculateContentLength()
      {
         return 0;
      }

      protected override Byte GetTag()
      {
         return 0x05; // NULL
      }

      protected override void SerializeContent( Byte[] array, Int32 idx, Int32 calculatedLength )
      {
         // Do nothing
      }
   }

   internal static class E_ASN
   {

      internal static Byte[] SerializeObjectIdentifier( this IEnumerable<Object> objID )
      {
         var array = new ResizableArray<Byte>( 0x20 );
         var idx = 0;
         foreach ( var component in objID.TransformObjectIdentifierSequence() )
         {
            if ( component is BigInteger )
            {
               throw new NotImplementedException( "Serializing big integers is not yet implemented" );
            }
            else
            {
               array.SerializeObjectIdentifierComponent( ref idx, Convert.ToInt64( component ) );
            }

         }

         var bytes = array.Array;
         if ( idx < bytes.Length )
         {
            var tmp = 0;
            bytes = bytes.CreateAndBlockCopyTo( ref tmp, idx );
         }
         return bytes;
      }

      private static IEnumerable<Object> TransformObjectIdentifierSequence( this IEnumerable<Object> objID )
      {
         using ( var enumerator = objID.GetEnumerator() )
         {
            if ( !enumerator.MoveNext() )
            {
               throw new InvalidOperationException( "Object identifier must contain at least two integers." );
            }

            var first = enumerator.Current;
            switch ( Type.GetTypeCode( first?.GetType() ) )
            {
               case TypeCode.Byte:
               case TypeCode.SByte:
               case TypeCode.Int16:
               case TypeCode.UInt16:
               case TypeCode.Int32:
               case TypeCode.UInt32:
               case TypeCode.Int64:
               case TypeCode.UInt64:
                  break;
               default:
                  throw new InvalidOperationException( "The first object of object identifier must be on of the primitive integer types." );
            }
            var firstInt = Convert.ToInt64( first ) * 40;

            if ( !enumerator.MoveNext() )
            {
               throw new InvalidOperationException( "Object identifier must contain at least two integers." );
            }
            var next = enumerator.Current;
            if ( next is BigInteger )
            {
               throw new NotImplementedException( "Serializing big integers is not yet implemented" );
            }
            else
            {
               yield return firstInt + Convert.ToInt64( next );
            }


            while ( enumerator.MoveNext() )
            {
               var obj = enumerator.Current;
               yield return obj;
            }
         }
      }

      private static ResizableArray<Byte> SerializeObjectIdentifierComponent( this ResizableArray<Byte> array, ref Int32 idx, Int64 value )
      {
         array
            .EnsureThatCanAdd( idx, BinaryUtils.Calculate7BitEncodingLength( value ) )
            .Array.WriteInt64BEEncoded7Bit( ref idx, value );
         return array;
      }

      internal static Int32 CalculateDERComponentLengthSize( this Int32 length )
      {
         var retVal = 1;
         var uval = unchecked((UInt32) length);
         if ( uval >= 0x80 )
         {
            retVal += ( BinaryUtils.Log2( uval ) / 8 ) + 1;
         }

         return retVal;
      }

      internal static Byte[] WriteDERComponentLength( this Byte[] array, ref Int32 idx, Int32 length, Int32 calculatedSize )
      {
         if ( calculatedSize == 1 )
         {
            array.WriteByteToBytes( ref idx, (Byte) length );
         }
         else
         {
            array.WriteByteToBytes( ref idx, (Byte) ( calculatedSize | 0x80 ) );

            // Integer serialized in big-endian format, except MSB zeroes are not serialized
            // TODO write some new methods to BinaryExtensions of CommonUtils for this kind of scenario (skipping leading/trailing zeroes in integer serialization)
            for ( var i = ( calculatedSize - 1 ) * 8; i >= 0; i -= 8 )
            {
               array.WriteByteToBytes( ref idx, (Byte) ( calculatedSize >> i ) );
            }
         }
         return array;
      }

      internal static IEnumerable<Int64> GetObjectIdentifier( this AssemblyHashAlgorithm hashAlgo )
      {
         switch ( hashAlgo )
         {
            case AssemblyHashAlgorithm.MD5:
               return GetObjectIdentifier_MD5();
            case AssemblyHashAlgorithm.SHA1:
               return GetObjectIdentifier_SHA1();
            case AssemblyHashAlgorithm.SHA256:
               return GetObjectIdentifier_NISTAlgorithm( 1 );
            case AssemblyHashAlgorithm.SHA384:
               return GetObjectIdentifier_NISTAlgorithm( 2 );
            case AssemblyHashAlgorithm.SHA512:
               return GetObjectIdentifier_NISTAlgorithm( 3 );
            case AssemblyHashAlgorithm.None:
            default:
               throw new ArgumentException( "Assembly hash algorithm was not one of the accepted values for RSA signing." );
         }
      }

      private static IEnumerable<Int64> GetObjectIdentifier_MD5()
      {
         // iso(1) member-body(2) US(840) rsadsi(113549) DigestAlgorithm(2) 5
         yield return 1;
         yield return 2;
         yield return 840;
         yield return 113549;
         yield return 2;
         yield return 5;
      }

      private static IEnumerable<Int64> GetObjectIdentifier_SHA1()
      {
         // iso(1) identified-organization(3) oiw(14) secsig(3) algorithms(2) 26
         yield return 1;
         yield return 3;
         yield return 14;
         yield return 3;
         yield return 2;
         yield return 26;
      }

      private static IEnumerable<Int64> GetObjectIdentifier_NISTAlgorithm( Int32 algoID )
      {
         // iso/itu(2) joint-assign(16) us(840) organization(1) gov(101) csor(3) nistalgorithms(4) hashalgs(2)
         yield return 2;
         yield return 16;
         yield return 840;
         yield return 1;
         yield return 101;
         yield return 3;
         yield return 4;
         yield return 2;
         yield return algoID;
      }
   }
}
