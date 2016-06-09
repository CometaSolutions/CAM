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
using UtilPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UtilPack.Numerics;
using UtilPack.Numerics.Calculations;

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
   public class RSAAlgorithm : CipherAlgorithm<RSAComputingParameters>
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
      public void ProcessBlock( Byte[] input, Int32 inOffset, Int32 inCount, Byte[] output, Int32 outOffset, RSAComputingParameters parameters )
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
               ),
            parameters.OutputEndianness
            );
      }

      /// <summary>
      /// This method is the actual implementation of RSA algorithm.
      /// It uses the values of <see cref="RSAComputingParameters"/> and the Chinese Remainder Theorem to compute an integer value, given another integer value and the <see cref="RSAComputingParameters"/>.
      /// </summary>
      /// <param name="input"></param>
      /// <param name="parameters"></param>
      /// <returns></returns>
      public BigInteger ProcessInteger( BigInteger input, RSAComputingParameters parameters )
      {
         return parameters.RunRSAAlgorithm( input );
         //var p = parameters.P;
         //var q = parameters.Q;
         //var dp = parameters.DP;
         //var dq = parameters.DQ;
         //BigInteger retVal;
         //if ( p.HasValue && q.HasValue && dp.HasValue && dq.HasValue )
         //{
         //   parameters.RunRSAAlgorithm( input );
         //   // Decryption

         //   // mP = ((input Mod p) ^ dP)) Mod p
         //   var mp = ( input % p.Value ).ModPow( dp.Value, p.Value );

         //   // mQ = ((input Mod q) ^ dQ)) Mod q
         //   var mq = ( input % q.Value ).ModPow( dq.Value, q.Value );

         //   // h = qInv * (mP - mQ) Mod p
         //   var h = ( parameters.InverseQ.Value * ( mp - mq ) ).Remainder_Positive( p.Value );

         //   // m = h * q + mQ
         //   retVal = ( h * q.Value ) + mq;
         //}
         //else
         //{
         //   // Encryption

         //   // c = (i ^ e) Mod m
         //   retVal = input.ModPow( parameters.Exponent, parameters.Modulus );
         //}

         //return retVal;
      }

      /// <summary>
      /// Converts the input binary data to <see cref="BigInteger"/> processable by <see cref="ProcessInteger"/> method.
      /// </summary>
      /// <param name="input">The binary data.</param>
      /// <param name="inOffset">The offset in <paramref name="input"/> where to start reading data form.</param>
      /// <param name="inCount">The amount of bytes to read from <paramref name="input"/>.</param>
      /// <param name="parameters">The <see cref="RSAComputingParameters"/>.</param>
      /// <returns>The integer acting as input for RSA algorithm implemented by <see cref="ProcessInteger"/> method.</returns>
      /// <exception cref="ArgumentException">If the integer readable from binary data is too large. The value should be less than <see cref="RSAComputingParameters.Modulus"/>.</exception>
      public BigInteger ConvertInput( Byte[] input, Int32 inOffset, Int32 inCount, RSAComputingParameters parameters )
      {
         if ( inCount > BinaryUtils.AmountOfPagesTaken( parameters.Modulus.BitLength, 8 ) )
         {
            throw new ArgumentException( "Input too large." );
         }

         var retVal = BigInteger.ParseFromBinary( input, inOffset, inCount, parameters.InputEndianness, 1 );
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
      /// <param name="resultEndianness">The endianness of the serialization of the <paramref name="result"/>.</param>
      public void ConvertOutput( Byte[] output, Int32 outOffset, BigInteger result, BinaryEndianness resultEndianness )
      {
         var resultByteCount = BinaryUtils.AmountOfPagesTaken( result.BitLength, 8 );
         result.WriteToByteArray( output, outOffset, Math.Min( resultByteCount, output.Length - outOffset ), resultEndianness, includeSign: false );
      }
   }

   /// <summary>
   /// This class represents the blinded RSA algorithm, suitable to use when the actual RSA algorithm should not know the input nor the output.
   /// </summary>
   public sealed class RSABlindedAlgorithm : AbstractDisposable, CipherAlgorithm<RSAComputingParameters>
   {
      private readonly RSAAlgorithm _actualEncryptor;
      private readonly Random _random;
      private readonly Boolean _randomCreatedByConstructor;

      /// <summary>
      /// Creates a new instance of <see cref="RSABlindedAlgorithm"/> with given parameters.
      /// </summary>
      /// <param name="actualEncryptor">Optional actual RSA algorithm. If <c>null</c>, the <see cref="RSAAlgorithm.DefaultInstance"/> will be used.</param>
      /// <param name="random">The <see cref="Random"/> to use when generating random value for blinding. If <c>null</c>, the <see cref="SecureRandom"/> will be used, with automatically seeded <see cref="DigestRandomGenerator"/> using <see cref="SHA2_512"/>.</param>
      /// <remarks>
      /// If given <paramref name="random"/> is not <c>null</c>, it will *not* be disposed when this <see cref="RSABlindedAlgorithm"/> is disposed.
      /// </remarks>
      public RSABlindedAlgorithm( RSAAlgorithm actualEncryptor = null, Random random = null )
      {
         if ( actualEncryptor != null && !Equals( actualEncryptor.GetType(), typeof( RSAAlgorithm ) ) )
         {
            this._actualEncryptor = actualEncryptor;
         }
         this._random = random ?? new SecureRandom( DigestRandomGenerator.CreateAndSeedWithDefaultLogic( new SHA2_512() ) );
         this._randomCreatedByConstructor = random == null;
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
      public void ProcessBlock( Byte[] input, Int32 inOffset, Int32 inCount, Byte[] output, Int32 outOffset, RSAComputingParameters parameters )
      {
         var encryptor = this._actualEncryptor ?? RSAAlgorithm.DefaultInstance;
         var inputInt = encryptor.ConvertInput( input, inOffset, inCount, parameters );
         var m = parameters.Modulus;
         var random = this._random.NextBigInt( 1, m - 1 );
         var outputInt = parameters.RunRSAAlgorithm_Blinded( inputInt, random, this._actualEncryptor );

         //var e = parameters.Exponent;


         //// Compute output s' of i' = (i * r ^ e) Mod m
         //var blindedOutput = this._actualEncryptor.ProcessInteger( ( inputInt * random.ModPow( e, m ) ).Remainder_Positive( m ), parameters );
         //// Signature s = (s' * r ^ -1) Mod m
         //var outputInt = ( blindedOutput * random.ModInverse( m ) ).Remainder_Positive( m );

         //// Timing attack check
         //if ( inputInt != outputInt.ModPow( e, m ) )
         //{
         //   throw new InvalidOperationException( "Invalid decryption/signing detected!" );
         //}

         //var testing = parameters.RunRSAAlgorithm_Blinded( inputInt, random, this._actualEncryptor );

         encryptor.ConvertOutput( output, outOffset, outputInt, parameters.OutputEndianness );
      }

      /// <inheritdoc />
      protected override void Dispose( Boolean disposing )
      {
         if ( this._randomCreatedByConstructor )
         {
            ( this._random as IDisposable )?.DisposeSafely();
         }
      }

   }

   internal class SecureRandom : Random, IDisposable
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

      public void Dispose()
      {
         this._generator.DisposeSafely();
         Array.Clear( this._intBytes, 0, this._intBytes.Length );
      }

      ~SecureRandom()
      {
         try
         {
            this.Dispose();
         }
         catch
         {
            // Ignore
         }
      }
   }

   internal interface RandomGenerator : IDisposable
   {
      void AddSeedMaterial( Byte[] material, Int32 offset, Int32 count );
      void AddSeedMaterial( Int64 materialValue );
      void NextBytes( Byte[] array, Int32 offset, Int32 count );
   }

   internal abstract class AbstractRandomGenerator : AbstractDisposable, RandomGenerator
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
            offset += state.Length;
         } while ( count - offset > 0 );
      }

      protected override void Dispose( Boolean disposing )
      {
         this.Algorithm.Reset();
         Array.Clear( this._state, 0, this._state.Length );
         Array.Clear( this._seed, 0, this._seed.Length );
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
   public class RSAComputingParameters
   {
      /// <summary>
      /// Creates a new instance of <see cref="RSAComputingParameters"/> from given <see cref="RSAParameters"/>.
      /// </summary>
      /// <param name="rParams">The <see cref="RSAParameters"/>.</param>
      /// <param name="inputEndianness">The endianness of the input number.</param>
      /// <param name="outputEndianness">The endianness of the output number.</param>
      public RSAComputingParameters( RSAParameters rParams, BinaryEndianness inputEndianness, BinaryEndianness outputEndianness )
      {
         this.Exponent = BigInteger.ParseFromBinary( rParams.Exponent, rParams.NumberEndianness, 1 );
         this.Modulus = BigInteger.ParseFromBinary( rParams.Modulus, rParams.NumberEndianness, 1 );

         this.D = BigInteger.ParseFromBinaryOrNull( rParams.D, rParams.NumberEndianness, 1 );
         this.DP = BigInteger.ParseFromBinaryOrNull( rParams.DP, rParams.NumberEndianness, 1 );
         this.DQ = BigInteger.ParseFromBinaryOrNull( rParams.DQ, rParams.NumberEndianness, 1 );
         this.InverseQ = BigInteger.ParseFromBinaryOrNull( rParams.InverseQ, rParams.NumberEndianness, 1 );
         this.P = BigInteger.ParseFromBinaryOrNull( rParams.P, rParams.NumberEndianness, 1 );
         this.Q = BigInteger.ParseFromBinaryOrNull( rParams.Q, rParams.NumberEndianness, 1 );
         this.InputEndianness = inputEndianness;
         this.OutputEndianness = outputEndianness;
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

      /// <summary>
      /// Gets the endianness of the input number.
      /// </summary>
      /// <value>The endianness of the input number.</value>
      public BinaryEndianness InputEndianness { get; }

      /// <summary>
      /// Gets the endianness of the output number.
      /// </summary>
      /// <value>The endianness of the output number.</value>
      public BinaryEndianness OutputEndianness { get; }

   }


}

#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{
   /// <summary>
   /// Changes the number endianness of this <see cref="RSAParameters"/>.
   /// </summary>
   /// <param name="rParams">The <see cref="RSAParameters"/>.</param>
   /// <param name="newEndianness">The desired number endianness of the return value.</param>
   /// <param name="inPlace">If reversing is needed, this parameter controls whether to reverse arrays in-place. If <c>true</c>, then the byte arrays of the original <see cref="RSAParameters"/> will be reversed as well.</param>
   /// <returns>The new <see cref="RSAParameters"/> with given number endianness.</returns>
   public static RSAParameters ChangeNumberEndianness( this RSAParameters rParams, BinaryEndianness newEndianness, Boolean inPlace = false )
   {
      RSAParameters retVal;
      if ( rParams.NumberEndianness == newEndianness )
      {
         // Nothing to do
         retVal = rParams;
      }
      else
      {
         // Save arrays
         var mod = rParams.Modulus;
         var exp = rParams.Exponent;
         var d = rParams.D;
         var p = rParams.P;
         var q = rParams.Q;
         var dp = rParams.DP;
         var dq = rParams.DQ;
         var iq = rParams.InverseQ;

         // Create copy if needed
         if ( !inPlace )
         {
            mod = mod.CreateArrayCopy();
            exp = exp.CreateArrayCopy();
            d = d.CreateArrayCopy();
            p = p.CreateArrayCopy();
            q = q.CreateArrayCopy();
            dp = dp.CreateArrayCopy();
            dq = dq.CreateArrayCopy();
            iq = iq.CreateArrayCopy();
         }

         // Reverse
         Array.Reverse( mod );
         Array.Reverse( exp );
         if ( d != null )
         {
            Array.Reverse( d );
         }
         if ( p != null )
         {
            Array.Reverse( p );
         }
         if ( q != null )
         {
            Array.Reverse( q );
         }
         if ( dp != null )
         {
            Array.Reverse( dp );
         }
         if ( dq != null )
         {
            Array.Reverse( dq );
         }
         if ( iq != null )
         {
            Array.Reverse( iq );
         }


         retVal = new RSAParameters( newEndianness, mod, exp, d, p, q, dp, dq, iq );
      }
      return retVal;
   }

   #region RSA Algorithm

   // TODO make this class public and extendable
   private sealed class RSAComputationState
   {
      public RSAComputationState( RSAComputingParameters rsaParams, BigInteger input )
      {
         this.Modulus = rsaParams.Modulus;
         this.Exponent = rsaParams.Exponent;

         this.P = rsaParams.P ?? default( BigInteger );
         this.DP = rsaParams.DP ?? default( BigInteger );
         this.Q = rsaParams.Q ?? default( BigInteger );
         this.DQ = rsaParams.DQ ?? default( BigInteger );
         this.InverseQ = rsaParams.InverseQ ?? default( BigInteger );

         this.Temporary = new ModifiableBigInteger();

         this.Input = new ModifiableBigInteger( input );
         this.MP = new ModifiableBigInteger();
         this.MQ = new ModifiableBigInteger();
         this.Result = new ModifiableBigInteger();
      }

      public BigInteger Modulus;
      public BigInteger Exponent;

      public ModifiableBigInteger Input;

      public ModifiableBigInteger Result;

      public BigInteger P;
      public BigInteger DP;
      public BigInteger Q;
      public BigInteger DQ;
      public BigInteger InverseQ;

      public ModifiableBigInteger Temporary;

      public ModifiableBigInteger MP;
      public ModifiableBigInteger MQ;

      public Boolean HasPrivateKey
      {
         get
         {
            return !this.P.IsZero
               && !this.DP.IsZero
               && !this.Q.IsZero
               && !this.DQ.IsZero
               && !this.InverseQ.IsZero;
         }
      }
   }

   private sealed class RSABlindedComputationState
   {
      public RSABlindedComputationState( RSAComputingParameters rsaParams, BigInteger input, BigInteger random )
      {
         this.RSAState = new RSAComputationState( rsaParams, input );
         this.ModPowState = new ModPowCalculationState( random.AsModifiable(), rsaParams.Modulus );
         this.Temporary = new ModifiableBigInteger();
      }

      public RSAComputationState RSAState { get; }

      public ModPowCalculationState ModPowState { get; }

      public ModifiableBigInteger Temporary;
   }

   /// <summary>
   /// 
   /// </summary>
   /// <param name="rsaParams"></param>
   /// <param name="input"></param>
   /// <returns></returns>
   public static BigInteger RunRSAAlgorithm( this RSAComputingParameters rsaParams, BigInteger input )
   {
      var state = new RSAComputationState( rsaParams, input );
      if ( state.HasPrivateKey )
      {
         state.RunRSAAlgorithm_Decrypt();
      }
      else
      {
         state.RunRSAAlgorithm_Encrypt();
      }
      var retVal = state.Result.CreateBigInteger();
      return retVal;
   }

   /// <summary>
   /// 
   /// </summary>
   /// <param name="rsaParams"></param>
   /// <param name="input"></param>
   /// <param name="random"></param>
   /// <param name="actualRSA"></param>
   /// <returns></returns>
   public static BigInteger RunRSAAlgorithm_Blinded( this RSAComputingParameters rsaParams, BigInteger input, BigInteger random, RSAAlgorithm actualRSA )
   {
      var state = new RSABlindedComputationState( rsaParams, input, random );
      if ( !state.RSAState.HasPrivateKey )
      {
         throw new InvalidOperationException( "Blinded RSA algorithm requires private key." );
      }

      // Compute another input i' = (i * r ^ e) Mod m
      var mpState = state.ModPowState;
      var modulus = state.RSAState.Modulus;
      // random.ModPow(exponent, modulus)
      mpState.ModPow( state.RSAState.Exponent );
      // input * random.ModPow(exponent, modulus)
      BigIntegerCalculations.Multiply( input, mpState.Result, state.Temporary );
      // (input * random.ModPow(exponent, modulus).Remainder_Positive( modulus )
      BigIntegerCalculations.Modulus_Positive( state.Temporary, modulus, mpState.Result );
      // Pass i' to actual RSA algorithm
      if ( actualRSA == null )
      {
         // We can use RSA algorithm directly
         // TODO this still initializes Input of RSAState twice, this could and should be optimized.
         state.Temporary.CopyBits( state.RSAState.Input, true );
         state.RSAState.RunRSAAlgorithm_Decrypt();
         BigIntegerCalculations.Swap( ref state.RSAState.Result, ref state.Temporary );
      }
      else
      {
         // We use the actualRSA object, which operates on BigIntegers.
         var actualRSAOutput = actualRSA.ProcessInteger( state.Temporary.CreateBigInteger(), rsaParams );
         state.Temporary.InitializeBig( actualRSAOutput );
      }

      // state.Temporary now has output from actual RSA
      // But first, calculate random.ModInverse( modulus )
      mpState.Result.InitializeBig( random );
      mpState.Temporary.InitializeBig( modulus );
      var eeState = new ExtendedEuclideanCalculationState( mpState.Result, mpState.Temporary );
      eeState.RunExtendedEuclideanAlgorithm();
      // gcd is in u3
      UInt32 gcd;
      if ( !eeState.U3.TryGetSmallValue( out gcd ) || gcd != 1 )
      {
         // Shouldn't be possible?
         throw new ArithmeticException( "ModInverse failed in RSA blinded algorithm" );
      }
      // mod inverse is in u1.
      // blindedOutput * random.ModInverse( modulus )
      BigIntegerCalculations.Multiply( state.Temporary, eeState.U1, eeState.Temporary );
      // (blindedOutput * random.ModInverse( modulus ) ).Remainder_Positive( modulus )
      BigIntegerCalculations.Modulus_Positive( eeState.Temporary, modulus, state.Temporary );

      // This is our result
      return eeState.Temporary.CreateBigInteger();
      //BigIntegerCalculations.SwapBits( ref eeState.Temporary, ref eeState.TemporaryLength, ref state.RSAState.Result, ref state.RSAState.ResultLength );
   }

   private static void RunRSAAlgorithm_Decrypt( this RSAComputationState state )
   {
      // mP = ((input Mod p) ^ dP)) Mod p
      // var mp = ( input % p ).ModPow( dp, p );
      state.Input.CopyBits( state.Temporary, true );
      BigIntegerCalculations.Modulus( state.Temporary, state.P ); // temp = input % p
      var modPowState = new ModPowCalculationState( state.Temporary, state.P );
      modPowState.ModPow( state.DP );
      BigIntegerCalculations.CopyBits( modPowState.Result, state.MP, true );

      // mQ = ((input Mod q) ^ dQ)) Mod q
      // var mq = ( input % q ).ModPow( dq, q );
      // Input is no longer needed after this, so do modulus right into it
      BigIntegerCalculations.Modulus( state.Input, state.Q );
      modPowState.Reset( state.Input, state.Q );
      modPowState.ModPow( state.DQ );
      BigIntegerCalculations.CopyBits( modPowState.Result, state.MQ, true );

      // h = qInv * (mP - mQ) Mod p
      // var h = ( parameters.InverseQ * ( mp - mq ) ).Remainder_Positive( p );
      BigIntegerCalculations.Subtract( state.MP, state.MQ, state.Input );
      BigIntegerCalculations.Multiply( state.InverseQ, state.Input, state.Temporary );
      BigIntegerCalculations.Modulus_Positive( state.Temporary, state.P, state.Input );

      // m = h * q + mQ
      // retVal = ( h * q ) + mq;
      BigIntegerCalculations.Multiply( state.Temporary, state.Q, state.Input );
      BigIntegerCalculations.Add( state.Input, state.MQ, state.Result );
   }

   private static void RunRSAAlgorithm_Encrypt( this RSAComputationState state )
   {
      // Encryption
      var modState = new ModPowCalculationState( state.Input, state.Modulus );

      // c = (i ^ e) Mod m
      //retVal = input.ModPow( parameters.Exponent, parameters.Modulus );
      modState.ModPow( state.Exponent );
      state.Result = modState.Result;
   }


   #endregion

   internal static void AddSeedMaterial( this RandomGenerator generator, Byte[] array )
   {
      generator.AddSeedMaterial( array, 0, array.Length );
   }

   internal static void NextBytes( this RandomGenerator generator, Byte[] array )
   {
      generator.NextBytes( array, 0, array.Length );
   }
}