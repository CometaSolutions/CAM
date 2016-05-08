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
using CILAssemblyManipulator.Physical.Crypto;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CILAssemblyManipulator.Physical.Crypto
{

   /// <summary>
   /// This interface represents an algorithm which ciphers or deciphers blocks of data.
   /// </summary>
   /// <typeparam name="TParameters">The algorithm-specific parameters.</typeparam>
   public interface CipherAlgorithm<TParameters>
   {
      /// <summary>
      /// Processes the data block.
      /// </summary>
      /// <param name="input">The array where to read the data from.</param>
      /// <param name="inOffset">The offset in <paramref name="input"/> where to start reading data from.</param>
      /// <param name="inCount">The amount of bytes to read from <paramref name="input"/>.</param>
      /// <param name="output">The array where to write cipher or plaintext data.</param>
      /// <param name="outOffset">The offset in <paramref name="output"/> where to start writing data to.</param>
      /// <param name="parameters">The algorithm-specific parameters.</param>
      void ProcessBlock( Byte[] input, Int32 inOffset, Int32 inCount, Byte[] output, Int32 outOffset, TParameters parameters );
   }

   /// <summary>
   /// This class represents the RSA algorithm logic operating with API of <see cref="CipherAlgorithm{TParameters}"/>.
   /// </summary>
   public class RSAAlgorithm : CipherAlgorithm<RSAComputedParameters>
   {
      /// <summary>
      /// The <see cref="RSAAlgorithm"/> itself is stateless, and this instance should be used as default instance.
      /// </summary>
      public static RSAAlgorithm DefaultInstance { get; }

      static RSAAlgorithm()
      {
         DefaultInstance = new RSAAlgorithm();
      }

      /// <inheritdoc/>
      public void ProcessBlock( Byte[] input, Int32 inOffset, Int32 inCount, Byte[] output, Int32 outOffset, RSAComputedParameters parameters )
      {
         this.ConvertOutput(
            output,
            outOffset,
            this.ProcessInteger(
               this.ConvertInput(
                  input,
                  inOffset,
                  inCount,
                  parameters
                  ),
               parameters
               )
            );
      }

      /// <summary>
      /// This method is the actual implementation of RSA algorithm.
      /// It uses the values of <see cref="RSAComputedParameters"/> and the Chinese Remainder Theorem to compute an integer value, given another integer value and the <see cref="RSAComputedParameters"/>.
      /// </summary>
      /// <param name="input"></param>
      /// <param name="parameters"></param>
      /// <returns></returns>
      public BigInteger ProcessInteger( BigInteger input, RSAComputedParameters parameters )
      {
         var p = parameters.P;
         var q = parameters.Q;
         var dp = parameters.DP;
         var dq = parameters.DQ;
         BigInteger retVal;
         if ( p.HasValue && q.HasValue && dp.HasValue && dq.HasValue )
         {
            // Decryption

            // mP = ((input Mod p) ^ dP)) Mod p
            var mp = ( input % p.Value ).ModPow( dp.Value, p.Value );

            // mQ = ((input Mod q) ^ dQ)) Mod q
            var mq = ( input % q.Value ).ModPow( dq.Value, q.Value );

            // h = qInv * (mP - mQ) Mod p
            var h = ( parameters.InverseQ.Value * ( mp - mq ) ).Remainder_Positive( p.Value );

            // m = h * q + mQ
            retVal = ( h * q.Value ) + mq;
         }
         else
         {
            // Encryption

            // c = (i ^ e) Mod m
            retVal = input.ModPow( parameters.Exponent, parameters.Modulus );
         }

         return retVal;
      }

      /// <summary>
      /// Converts the input binary data to <see cref="BigInteger"/> processable by <see cref="ProcessInteger"/> method.
      /// </summary>
      /// <param name="input">The binary data.</param>
      /// <param name="inOffset">The offset in <paramref name="input"/> where to start reading data form.</param>
      /// <param name="inCount">The amount of bytes to read from <paramref name="input"/>.</param>
      /// <param name="parameters">The <see cref="RSAComputedParameters"/>.</param>
      /// <returns>The integer acting as input for RSA algorithm implemented by <see cref="ProcessInteger"/> method.</returns>
      /// <exception cref="ArgumentException">If the integer readable from binary data is too large. The value should be less than <see cref="RSAComputedParameters.Modulus"/>.</exception>
      public BigInteger ConvertInput( Byte[] input, Int32 inOffset, Int32 inCount, RSAComputedParameters parameters )
      {
         if ( inCount > BinaryUtils.AmountOfPagesTaken( parameters.Modulus.BitLength, 8 ) )
         {
            throw new ArgumentException( "Input too large." );
         }

         var retVal = BigInteger.ParseFromBinary( input, inOffset, inCount, BigInteger.BinaryEndianness.BigEndian, 1 );
         if ( retVal >= parameters.Modulus )
         {
            throw new ArgumentException( "Input too large." );
         }

         return retVal;
      }

      /// <summary>
      /// Converts the integer produced by <see cref="ProcessInteger"/> into binary output.
      /// </summary>
      /// <param name="output">The array to write data to.</param>
      /// <param name="outOffset">The offset in <paramref name="output"/> to start writing data to.</param>
      /// <param name="result">The <see cref="BigInteger"/> produced by <see cref="ProcessInteger"/> method.</param>
      public void ConvertOutput( Byte[] output, Int32 outOffset, BigInteger result )
      {
         result.WriteToByteArray( output, outOffset, BigInteger.BinaryEndianness.BigEndian );
      }
   }

   /// <summary>
   /// This class represents the blinded RSA algorithm, suitable to use when creating signatures.
   /// </summary>
   public sealed class RSABlindedAlgorithm : AbstractDisposable, CipherAlgorithm<RSAComputedParameters>
   {
      private readonly RSAAlgorithm _actualEncryptor;
      private readonly Random _random;

      /// <summary>
      /// Creates a new instance of <see cref="RSABlindedAlgorithm"/> with given parameters.
      /// </summary>
      /// <param name="actualEncryptor">Optional actual RSA algorithm. If <c>null</c>, the <see cref="RSAAlgorithm.DefaultInstance"/> will be used.</param>
      /// <param name="random">The <see cref="Random"/> to use when generating random value for blinding. If <c>null</c>, the <see cref="SecureRandom"/> will be used, with automatically seeded <see cref="DigestRandomGenerator"/> using <see cref="SHA2_512"/>.</param>
      public RSABlindedAlgorithm( RSAAlgorithm actualEncryptor = null, Random random = null )
      {
         this._actualEncryptor = actualEncryptor ?? RSAAlgorithm.DefaultInstance;
         this._random = random ?? new SecureRandom( DigestRandomGenerator.CreateAndSeedWithDefaultLogic( new SHA2_512() ) );
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="input"></param>
      /// <param name="inOffset"></param>
      /// <param name="inCount"></param>
      /// <param name="output"></param>
      /// <param name="outOffset"></param>
      /// <param name="parameters"></param>
      public void ProcessBlock( Byte[] input, Int32 inOffset, Int32 inCount, Byte[] output, Int32 outOffset, RSAComputedParameters parameters )
      {
         var inputInt = this._actualEncryptor.ConvertInput( input, inOffset, inCount, parameters );
         var m = parameters.Modulus;
         var e = parameters.Exponent;
         var r = BigInteger.CreateRandomInRange( 1, m - 1, this._random );
         // Compute output s' of i' = (i * r ^ e) Mod m
         var blindedOutput = this._actualEncryptor.ProcessInteger( ( inputInt * r.ModPow( e, m ) ).Remainder_Positive( m ), parameters );
         // Signature s = (s' * r ^ -1) Mod m
         var outputInt = ( blindedOutput * r.ModInverse( m ) ).Remainder_Positive( m );

         // Timing attack check
         if ( inputInt != outputInt.ModPow( e, m ) )
         {
            throw new InvalidOperationException( "Invalid decryption/signing detected!" );
         }

         this._actualEncryptor.ConvertOutput( output, outOffset, outputInt );
      }

      /// <summary>
      /// Currently, this method does nothing.
      /// </summary>
      /// <param name="disposing">Whether we are disposing from a <see cref="AbstractDisposable.Dispose()"/> call.</param>
      protected override void Dispose( Boolean disposing )
      {
         // Nothing to do
      }

   }

   internal class SecureRandom : Random
   {
      private readonly RandomGenerator _generator;
      private readonly Byte[] _intBytes;

      public SecureRandom( RandomGenerator generator )
         : base( 0 )
      {
         this._generator = ArgumentValidator.ValidateNotNull( "Generator", generator );
         this._intBytes = new Byte[sizeof( Int64 )];
      }

      public override Int32 Next()
      {
         // The spec is to return non-negative integer.
         return this.NextInt32() & Int32.MaxValue;
      }

      public override Int32 Next( Int32 maxValue )
      {
         // The spec is to return non-negative integer lesser than maxValue
         Int32 retVal;
         if ( maxValue < 2 )
         {
            if ( maxValue < 0 )
            {
               throw new ArgumentOutOfRangeException( "maxValue", "should be at least zero." );
            }
            else
            {
               // Return 0 for 0 and 1 max value
               retVal = 0;
            }
         }
         else
         {
            retVal = ( this.NextInt32() & Int32.MaxValue ) % maxValue;
         }

         return retVal;
      }

      public override Int32 Next( Int32 minValue, Int32 maxValue )
      {
         // TODO once whole CAM compiles in checked mode, the checks here will also be checked.

         // Here both minValue and maxValue can be negative
         Int32 retVal;
         if ( maxValue < minValue )
         {
            throw new ArgumentException( "Max value should be at least as min value." );
         }
         else if ( maxValue == minValue || maxValue == minValue + 1 )
         {
            retVal = minValue;
         }
         else
         {
            // Diff will be always at least 2, since previous if-conditions filter out all other options.
            var diff = (Int64) maxValue - (Int64) minValue;
            if ( diff <= Int32.MaxValue )
            {
               retVal = minValue + this.Next( (Int32) diff );
            }
            else
            {
               retVal = (Int32) ( (Int64) minValue + ( this.NextInt64() & Int64.MaxValue ) % diff );
            }
         }
         return retVal;
      }

      public override void NextBytes( Byte[] buffer )
      {
         this.NextBytes( buffer, 0, buffer.Length );
      }

      public void NextBytes( Byte[] buffer, Int32 offset, Int32 length )
      {
         this._generator.NextBytes( buffer, offset, length );
      }

      public override Double NextDouble()
      {
         const Double scale = Int64.MaxValue;
         return Convert.ToDouble( (UInt64) this.NextInt64() ) / scale;
      }

      public Int32 NextInt32()
      {
         this.NextBytes( this._intBytes, 0, sizeof( Int32 ) );
         return this._intBytes.ReadInt32BEFromBytesNoRef( 0 ); // Endianness shouldn't matter here since bytes are random.
      }

      public Int64 NextInt64()
      {
         this.NextBytes( this._intBytes, 0, sizeof( Int64 ) );
         return this._intBytes.ReadInt64BEFromBytesNoRef( 0 );
      }
   }

   internal interface RandomGenerator
   {
      void AddSeedMaterial( Byte[] material, Int32 offset, Int32 count );
      void AddSeedMaterial( Int64 materialValue );
      void NextBytes( Byte[] array, Int32 offset, Int32 count );
   }

   internal abstract class AbstractRandomGenerator : RandomGenerator
   {

      protected AbstractRandomGenerator()
      {
         this.ArrayForLong = new Byte[sizeof( Int64 )];
      }

      public void AddSeedMaterial( Int64 materialValue )
      {
         this.ArrayForLong.WriteInt64LEToBytesNoRef( 0, materialValue );
         this.AddSeedMaterial( this.ArrayForLong, 0, sizeof( Int64 ) );
      }

      public abstract void AddSeedMaterial( Byte[] material, Int32 offset, Int32 count );
      public abstract void NextBytes( Byte[] array, Int32 offset, Int32 count );

      // Do *not* use this inside AddSeedMaterial method!
      protected Byte[] ArrayForLong { get; }
   }

   internal class DigestRandomGenerator : AbstractRandomGenerator
   {
      private const Int32 DEFAULT_SEED_CYCLE_COUNT = 10;

      private readonly Int32 _seedCycleCount;
      private readonly Byte[] _seed;
      private readonly Byte[] _state;

      private Int64 _stateCounter;
      private Int64 _seedCounter;

      public DigestRandomGenerator(
         BlockDigestAlgorithm algorithm,
         Int32 seedCycleCount = DEFAULT_SEED_CYCLE_COUNT
         )
      {
         this.Algorithm = ArgumentValidator.ValidateNotNull( "Algorithm", algorithm );
         this._seed = new Byte[this.Algorithm.DigestByteCount];
         this._state = new Byte[this.Algorithm.DigestByteCount];

         this._stateCounter = 0; // When state is first time generated, the state counter will be increased to 1
         this._seedCounter = 0;
         this._seedCycleCount = seedCycleCount;
      }

      public override void AddSeedMaterial( Byte[] material, Int32 offset, Int32 count )
      {
         this.Algorithm.ProcessBlock( material, offset, count );
         this.Algorithm.ProcessBlock( this._seed );
         this.Algorithm.WriteDigest( this._seed );
      }

      public override void NextBytes( Byte[] array, Int32 offset, Int32 count )
      {
         var state = this._state;
         do
         {
            this.PopulateState();
            Array.Copy( state, 0, array, offset, Math.Min( count, state.Length ) );
         } while ( ( count -= state.Length ) > 0 );
      }

      protected BlockDigestAlgorithm Algorithm { get; }

      private void PopulateState()
      {
         var newStateCounter = this.AlgorithmProcessInt64( Interlocked.Increment( ref this._stateCounter ) );
         this.Algorithm.ProcessBlock( this._state );
         this.Algorithm.ProcessBlock( this._seed );
         this.Algorithm.WriteDigest( this._state );

         if ( newStateCounter % this._seedCycleCount == 0 )
         {
            this.PopulateSeed();
         }
      }

      private void PopulateSeed()
      {
         this.Algorithm.ProcessBlock( this._seed );
         this.AlgorithmProcessInt64( Interlocked.Increment( ref this._seedCounter ) );
         this.Algorithm.WriteDigest( this._seed );
      }

      private Int64 AlgorithmProcessInt64( Int64 val )
      {
         this.ArrayForLong.WriteInt64LEToBytesNoRef( 0, val );
         this.Algorithm.ProcessBlock( this.ArrayForLong );
         return val;
      }

      public static DigestRandomGenerator CreateAndSeedWithDefaultLogic( BlockDigestAlgorithm algorithm, Int32 seedCycleCount = DEFAULT_SEED_CYCLE_COUNT )
      {

         var retVal = new DigestRandomGenerator( algorithm, seedCycleCount );
         // Use Guid as random source (should be version 4)
         retVal.AddSeedMaterial( Guid.NewGuid().ToByteArray() );

         // Use current ticks
         retVal.AddSeedMaterial( DateTime.Now.Ticks );

         // Use Guid again
         retVal.AddSeedMaterial( Guid.NewGuid().ToByteArray() );
         return retVal;
      }

   }

   /// <summary>
   /// This class encapsulates the same data that is stored in <see cref="RSAParameters"/>, but the binary array objects are now parsed into <see cref="BigInteger"/>s.
   /// </summary>
   public struct RSAComputedParameters
   {
      /// <summary>
      /// Creates a new instance of <see cref="RSAComputedParameters"/> from given <see cref="RSAParameters"/>.
      /// </summary>
      /// <param name="rParams">The <see cref="RSAParameters"/>.</param>
      public RSAComputedParameters( RSAParameters rParams )
      {
         this.Exponent = BigInteger.ParseFromBinary( rParams.Exponent, BigInteger.BinaryEndianness.BigEndian, 1 );
         this.Modulus = BigInteger.ParseFromBinary( rParams.Modulus, BigInteger.BinaryEndianness.BigEndian, 1 );

         this.D = BigInteger.ParseFromBinaryOrNull( rParams.D, BigInteger.BinaryEndianness.BigEndian, 1 );
         this.DP = BigInteger.ParseFromBinaryOrNull( rParams.DP, BigInteger.BinaryEndianness.BigEndian, 1 );
         this.DQ = BigInteger.ParseFromBinaryOrNull( rParams.DQ, BigInteger.BinaryEndianness.BigEndian, 1 );
         this.InverseQ = BigInteger.ParseFromBinaryOrNull( rParams.InverseQ, BigInteger.BinaryEndianness.BigEndian, 1 );
         this.P = BigInteger.ParseFromBinaryOrNull( rParams.P, BigInteger.BinaryEndianness.BigEndian, 1 );
         this.Q = BigInteger.ParseFromBinaryOrNull( rParams.Q, BigInteger.BinaryEndianness.BigEndian, 1 );
      }

      /// <summary>
      /// Gets the public exponent.
      /// </summary>
      /// <value>The public exponent.</value>
      public BigInteger Exponent { get; }

      /// <summary>
      /// Gets the (public) modulus.
      /// </summary>
      /// <value>The (public) modulus.</value>
      public BigInteger Modulus { get; }

      /// <summary>
      /// Gets the private exponent.
      /// </summary>
      /// <value>The private exponent.</value>
      public BigInteger? D { get; }

      /// <summary>
      /// Gets the factor of <see cref="D"/> related to <see cref="P"/> such that <c>dp = d mod (p - 1)</c>.
      /// </summary>
      /// <value>The factor of <see cref="D"/> related to <see cref="P"/>.</value>
      public BigInteger? DP { get; }

      /// <summary>
      /// Gets the factor of <see cref="D"/> related to <see cref="Q"/> such that <c>dq = d mod (q - 1)</c>.
      /// </summary>
      /// <value>The factor of <see cref="D"/> related to <see cref="Q"/>.</value>
      public BigInteger? DQ { get; }

      /// <summary>
      /// Gets the inverse of <see cref="Q"/> so that <c>(qinv * q) mod p = 1</c>.
      /// </summary>
      /// <value>The inverse of <see cref="Q"/> modulo <see cref="P"/>.</value>
      public BigInteger? InverseQ { get; }

      /// <summary>
      /// Gets the first prime number of the private key.
      /// </summary>
      /// <value>The first prime number of the private key.</value>
      public BigInteger? P { get; }

      /// <summary>
      /// Gets the second prime number of the private key.
      /// </summary>
      /// <value>The second prime number of the private key.</value>
      public BigInteger? Q { get; }

   }


}

#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{
   internal static void AddSeedMaterial( this RandomGenerator generator, Byte[] array )
   {
      generator.AddSeedMaterial( array, 0, array.Length );
   }

   internal static void NextBytes( this RandomGenerator generator, Byte[] array )
   {
      generator.NextBytes( array, 0, array.Length );
   }
}