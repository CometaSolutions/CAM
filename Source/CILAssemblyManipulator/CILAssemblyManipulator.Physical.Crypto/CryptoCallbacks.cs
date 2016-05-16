/*
 * Copyright 2015 Stanislav Muhametsin. All rights Reserved.
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
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.Crypto;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommonUtils.Numerics;

namespace CILAssemblyManipulator.Physical.Crypto
{
   /// <summary>
   /// This interface encapsulates all the cryptographical methods needed by CAM.Physical.
   /// </summary>
   public interface CryptoCallbacks : IDisposable
   {
      /// <summary>
      /// Given a container name, extracts public key bytes from it.
      /// </summary>
      /// <param name="containerName">The container name.</param>
      /// <returns>The public key bytes for given container name.</returns>
      /// <exception cref="NotSupportedException">If container names are not supported on this platform.</exception>
      Byte[] ExtractPublicKeyFromCSPContainer( String containerName );

      /// <summary>
      /// Given an <see cref="AssemblyHashAlgorithm"/>, creates a new <see cref="BlockDigestAlgorithm"/> struct to be used for cryptographic purposes (e.g. computing public key token, or calculating storng name signature).
      /// </summary>
      /// <param name="algorithm">The <see cref="AssemblyHashAlgorithm"/> enumeration.</param>
      /// <returns>A <see cref="BlockDigestAlgorithm"/> for given <paramref name="algorithm"/>.</returns>
      /// <seealso cref="BlockDigestAlgorithm"/>
      BlockDigestAlgorithm CreateHashAlgorithm( AssemblyHashAlgorithm algorithm );

      /// <summary>
      /// Computes a signature from a hash bytes, using given algorithm.
      /// </summary>
      /// <param name="contentsHash">The binary contents of the content from which to create signature from.</param>
      /// <param name="parsingResult">The parsing result from <see cref="TryParseKeyBLOB"/>.</param>
      /// <param name="containerName">The container name. Use <c>null</c> or empty string to use <paramref name="parsingResult"/> for RSA.</param>
      /// <returns>The cryptographic signature created from <paramref name="contentsHash"/>.</returns>
      Byte[] CreateSignature( Byte[] contentsHash, KeyBLOBParsingResult parsingResult, String containerName );

      /// <summary>
      /// This method will compute public key token for a given full public key.
      /// </summary>
      /// <param name="fullPublicKey">The full public key.</param>
      /// <returns>The value of <paramref name="fullPublicKey"/>, if <paramref name="fullPublicKey"/> is <c>null</c> or empty, or the public key token of the <paramref name="fullPublicKey"/>.</returns>
      Byte[] ComputePublicKeyToken( Byte[] fullPublicKey );

      /// <summary>
      /// Tries to parse public and/or private key information from given key BLOB.
      /// </summary>
      /// <param name="keyBLOB">The key BLOB.</param>
      /// <param name="hashAlgorithmOverride">The optional override to the hash algorithm specified in <paramref name="keyBLOB"/>.</param>
      /// <returns>Instance of <see cref="KeyBLOBParsingResult"/> if parsing was successul; <c>null</c> otherwise.</returns>
      KeyBLOBParsingResult TryParseKeyBLOB( Byte[] keyBLOB, AssemblyHashAlgorithm? hashAlgorithmOverride );
   }

   /// <summary>
   /// This class represents the result of parsing a cryptographic key(-pair) BLOB.
   /// </summary>
   public class KeyBLOBParsingResult
   {

      /// <summary>
      /// Creates a new instance of <see cref="KeyBLOBParsingResult"/> with given parameters.
      /// </summary>
      /// <param name="snSize">The size of the strong-name signature in the image, in bytes.</param>
      /// <param name="publicKey">The public key to use in emitting.</param>
      /// <param name="hashAlgorithm">The hash algorithm to use.</param>
      /// <param name="errorMessage">The error message, if key BLOB parsing failed.</param>
      public KeyBLOBParsingResult( Int32 snSize, Byte[] publicKey, AssemblyHashAlgorithm hashAlgorithm, String errorMessage )
      {
         this.StrongNameSize = snSize;
         this.PublicKey = publicKey;
         this.ErrorMessage = errorMessage;
         this.HashAlgorithm = hashAlgorithm;
      }

      /// <summary>
      /// Gets the size of the strong-name signature in the image, in bytes.
      /// </summary>
      /// <value>The size of the strong-name signature in the image, in bytes.</value>
      public Int32 StrongNameSize { get; }

      /// <summary>
      /// Gets the public key to use in emitting.
      /// </summary>
      /// <value>The public key to use in emitting.</value>
      public Byte[] PublicKey { get; }

      /// <summary>
      /// Gets the error message if key BLOB parsing failed.
      /// </summary>
      /// <value>The error message if key BLOB parsing failed.</value>
      public String ErrorMessage { get; }

      /// <summary>
      /// Gets the hash algorithm to use when computing hash.
      /// </summary>
      /// <value>The hash algorithm to use when computing hash.</value>
      public AssemblyHashAlgorithm HashAlgorithm { get; }
   }

   /// <summary>
   /// This is identical to System.Security.Cryptography.RSAParameters struct, which is missing from PCL.
   /// </summary>
   public struct RSAParameters
   {
      /// <summary>
      /// Creates a new instance of <see cref="RSAParameters"/> with public data.
      /// </summary>
      /// <param name="numberEndianness">The endianness of the binary numbers.</param>
      /// <param name="modulus">The modulus as binary number.</param>
      /// <param name="publicExponent">The exponent as binary number.</param>
      /// <remarks>
      /// This data should be sufficient to encrypt, but not to decrypt operations.
      /// </remarks>
      /// <exception cref="ArgumentNullException">If either of <paramref name="modulus"/> or <paramref name="publicExponent"/> is <c>null</c>.</exception>
      public RSAParameters(
         BinaryEndianness numberEndianness,
         Byte[] modulus,
         Byte[] publicExponent
         ) : this( numberEndianness, modulus, publicExponent, null, null, null, null, null, null )
      {

      }

      /// <summary>
      /// Creates a new instance of <see cref="RSAParameters"/> with public and private data.
      /// </summary>
      /// <param name="numberEndianness">The endianness of the binary numbers.</param>
      /// <param name="modulus">The modulus as binary number.</param>
      /// <param name="publicExponent">The exponent as binary number.</param>
      /// <param name="privateExponent">The private exponent as binary number.</param>
      /// <param name="p">The first prime as binary number (used t.</param>
      /// <param name="q">The second prime as binary number.</param>
      /// <param name="dp">The helper value for <paramref name="p"/> to be used in Chinese Remainder Theorem calculation.</param>
      /// <param name="dq">The helper value for <paramref name="q"/> to be used in Chinese Remainder Theorem calculation.</param>
      /// <param name="inverseQ">Another helper value for <paramref name="q"/> to be used in Chinese Remainder Theorem calculation.</param>
      /// <remarks>
      /// Assuming none of the parameters is <c>null</c>, this data should be sufficient to both encrypt and decrypt operations.
      /// </remarks>
      /// <exception cref="ArgumentNullException">If either of <paramref name="modulus"/> or <paramref name="publicExponent"/> is <c>null</c>.</exception>
      public RSAParameters(
         BinaryEndianness numberEndianness,
         Byte[] modulus,
         Byte[] publicExponent,
         Byte[] privateExponent,
         Byte[] p,
         Byte[] q,
         Byte[] dp,
         Byte[] dq,
         Byte[] inverseQ
         )
      {
         this.NumberEndianness = numberEndianness;
         this.Modulus = ArgumentValidator.ValidateNotNull( "Modulus", modulus );
         this.Exponent = ArgumentValidator.ValidateNotNull( "Public exponent", publicExponent );
         this.D = privateExponent;
         this.P = p;
         this.Q = q;
         this.DP = dp;
         this.DQ = dq;
         this.InverseQ = inverseQ;
      }
      /// <summary>
      /// Represents the <c>D</c> parameter (private exponent) for the RSA algorithm.
      /// </summary>
      public Byte[] D { get; }

      /// <summary>
      /// Represents the <c>DP</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] DP { get; }

      /// <summary>
      /// Represents the <c>DQ</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] DQ { get; }

      /// <summary>
      /// Represents the <c>Exponent</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] Exponent { get; }

      /// <summary>
      /// Represents the <c>InverseQ</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] InverseQ { get; }

      /// <summary>
      /// Represents the <c>Modulus</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] Modulus { get; }

      /// <summary>
      /// Represents the <c>P</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] P { get; }

      /// <summary>
      /// Represents the <c>Q</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] Q { get; }

      /// <summary>
      /// Represents the endianness of the numbers for the RSA algorithm.
      /// </summary>
      /// <value>The endianness of the numbers for the RSA algorithm.</value>
      public BinaryEndianness NumberEndianness { get; }
   }
}

// This class will get its documentation from CAM.Physical Core.
#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{

   /// <summary>
   /// Helper method to call <see cref="CryptoCallbacks.ExtractPublicKeyFromCSPContainer"/> method and check that return value is not <c>null</c> or empty.
   /// </summary>
   /// <param name="callbacks">This <see cref="CryptoCallbacks"/>.</param>
   /// <param name="containerName">The container name.</param>
   /// <returns>The non-<c>null</c> and non-empty public key for given container name.</returns>
   /// <exception cref="ArgumentException">If the return value of <see cref="CryptoCallbacks.ExtractPublicKeyFromCSPContainer"/> method is <c>null</c> or empty.</exception>
   /// <exception cref="NullReferenceException">If this <see cref="CryptoCallbacks"/> is <c>null</c>.</exception>
   public static Byte[] ExtractPublicKeyFromCSPContainerAndCheck( this CryptoCallbacks callbacks, String containerName )
   {
      var retVal = callbacks.ExtractPublicKeyFromCSPContainer( containerName );
      if ( retVal.IsNullOrEmpty() )
      {
         throw new ArgumentException( "The public key of CSP \"" + containerName + "\" could not be resolved." );
      }
      return retVal;
   }

   ///// <summary>
   ///// Helper method to call <see cref="CryptoCallbacks.CreateRSASignature"/> method and check that the return value is not <c>null</c>.
   ///// </summary>
   ///// <param name="callbacks">This <see cref="CryptoCallbacks"/>.</param>
   ///// <param name="rParams">The RSA parameters. Use <c>default(RSAParameters)</c> if RSA parameters are from CSP container name.</param>
   ///// <param name="containerName">The container name. Use <c>null</c> or empty string to use <paramref name="rParams"/> for RSA.</param>
   ///// <param name="hashAlgorithm">The algorithm name to use when creating signature.</param>
   ///// <param name="contentsHash">The binary data to create signature from.</param>
   ///// <param name="signatureEndianness">This parameter should contain the endianness of the binary number produced by RSA algorithm.</param>
   ///// <returns>Non-<c>null</c> signature bytes.</returns>
   ///// <exception cref="ArgumentNullException">If return value of <see cref="CryptoCallbacks.CreateRSASignature"/> is <c>null</c>.</exception>
   ///// <exception cref="NullReferenceException">If this <see cref="CryptoCallbacks"/> is <c>null</c>.</exception>
   //public static Byte[] CreateRSASignatureAndCheck( this CryptoCallbacks callbacks, AssemblyHashAlgorithm hashAlgorithm, Byte[] contentsHash, RSAParameters rParams, String containerName, out BinaryEndianness signatureEndianness )
   //{
   //   return ArgumentValidator.ValidateNotNull( "RSA signature", callbacks.CreateRSASignature( hashAlgorithm, contentsHash, rParams, containerName, out signatureEndianness ) );
   //}

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

   /// <summary>
   /// Helper method to check whether <see cref="KeyBLOBParsingResult"/> represents succeeded key BLOB parsing operation.
   /// </summary>
   /// <param name="result">The <see cref="KeyBLOBParsingResult"/>.</param>
   /// <returns><c>true</c> if the <see cref="KeyBLOBParsingResult"/> is not <c>null</c> and its <see cref="KeyBLOBParsingResult.ErrorMessage"/> is not <c>null</c>; <c>false</c> otherwise.</returns>
   public static Boolean ParsingSucceeded( this KeyBLOBParsingResult result )
   {
      return result != null && result.ErrorMessage == null;
   }
}