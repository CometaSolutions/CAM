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
      /// <param name="containterName">The container name.</param>
      /// <returns>The public key bytes for given container name.</returns>
      /// <exception cref="NotSupportedException">If container names are not supported on this platform.</exception>
      Byte[] ExtractPublicKeyFromCSPContainer( String containterName );

      /// <summary>
      /// Given an <see cref="AssemblyHashAlgorithm"/>, creates a new <see cref="HashStreamInfo"/> struct to be used for cryptographic purposes (e.g. computing public key token, or calculating storng name signature).
      /// </summary>
      /// <param name="algorithm">The <see cref="AssemblyHashAlgorithm"/> enumeration.</param>
      /// <returns>A <see cref="BlockHashAlgorithm"/> for given <paramref name="algorithm"/>.</returns>
      /// <seealso cref="BlockHashAlgorithm"/>
      BlockHashAlgorithm CreateHashAlgorithm( AssemblyHashAlgorithm algorithm );

      /// <summary>
      /// Creates a new RSA service provider for a given container name.
      /// </summary>
      /// <param name="containerName">The container name.</param>
      /// <returns>The RSA for given container name.</returns>
      /// <remarks>
      /// Usually return value is of type <see cref="T:System.Security.Cryptography.RSACryptoServiceProvider"/>, which is missing from PCL.
      /// </remarks>
      IDisposable CreateRSAFromCSPContainer( String containerName );

      /// <summary>
      /// Creates a new RSA service provider with given <see cref="RSAParameters"/>.
      /// </summary>
      /// <param name="parameters">The <see cref="RSAParameters"/>.</param>
      /// <returns>The RSA for given <see cref="RSAParameters"/>.</returns>
      /// <remarks>
      /// Usually return value is of type <see cref="T:System.Security.Cryptography.RSA"/>, which is missing from PCL.
      /// </remarks>
      IDisposable CreateRSAFromParameters( RSAParameters parameters );

      /// <summary>
      /// Given a RSA service provider (usually obtained from <see cref="CreateRSAFromCSPContainer"/> or <see cref="CreateRSAFromParameters"/>), computes a signature from a hash bytes, using given algorithm.
      /// </summary>
      /// <param name="rsa">The RSA, should be of type <see cref="T:System.Security.Cryptography.AsymmetricAlgorithm"/>.</param>
      /// <param name="hashAlgorithmName">The name of the algorithm to use when creating signature.</param>
      /// <param name="contentsHash">The binary contents of the content from which to create signature from.</param>
      /// <returns>The cryptographic signature created with <paramref name="hashAlgorithmName"/> algorithm from <paramref name="contentsHash"/>.</returns>
      Byte[] CreateRSASignature( IDisposable rsa, String hashAlgorithmName, Byte[] contentsHash );

      /// <summary>
      /// This method will compute public key token for a given full public key.
      /// </summary>
      /// <param name="fullPublicKey">The full public key.</param>
      /// <returns>The value of <paramref name="fullPublicKey"/>, if <paramref name="fullPublicKey"/> is <c>null</c> or empty, or the public key token of the <paramref name="fullPublicKey"/>.</returns>
      Byte[] ComputePublicKeyToken( Byte[] fullPublicKey );
   }

   /// <summary>
   /// This struct encapsulates information about hash algorithm, and some additional cryptographic callbacks and objects, so that they are accessible in PCL.
   /// </summary>
   public struct HashStreamInfo
   {
      /// <summary>
      /// Creates a new instance of <see cref="HashStreamInfo"/>, binding together <see cref="AssemblyHashAlgorithm"/>, a callback to create <see cref="T:System.Security.Cryptography.CryptoStream"/>, a <see cref="T:System.Security.Cryptography.HashAlgorithm"/> transform object, callback to get the hash from transform object, and callback to compute hash from given content, using the transform object.
      /// All this is done in such way that it is possible to use this class in PCL.
      /// </summary>
      /// <param name="algorithm">The <see cref="AssemblyHashAlgorithm"/>.</param>
      /// <param name="cryptoStream">The callback to create <see cref="T:System.Security.Cryptography.CryptoStream"/>.</param>
      /// <param name="transform">The <see cref="T:System.Security.Cryptography.HashAlgorithm"/> object.</param>
      /// <param name="hashGetter">The callback to get hash bytes from <paramref name="transform"/>.</param>
      /// <param name="hashComputer">The callback to compute hash bytes from given contents, using <paramref name="transform"/>.</param>
      public HashStreamInfo(
         AssemblyHashAlgorithm algorithm,
         Func<Stream> cryptoStream,
         IDisposable transform,
         Func<Byte[]> hashGetter,
         Func<Byte[], Byte[]> hashComputer )
      {
         this.Algorithm = algorithm;
         this.CryptoStream = cryptoStream;
         this.HashGetter = hashGetter;
         this.HashComputer = hashComputer;
         this.Transform = transform;
      }

      /// <summary>
      /// Gets the <see cref="AssemblyHashAlgorithm"/> of this <see cref="HashStreamInfo"/>.
      /// </summary>
      /// <value>The <see cref="AssemblyHashAlgorithm"/> of this <see cref="HashStreamInfo"/>.</value>
      public AssemblyHashAlgorithm Algorithm { get; }

      /// <summary>
      /// Gets the callback to create <see cref="T:System.Security.Cryptography.CryptoStream"/> with <see cref="Transform"/> object.
      /// </summary>
      /// <value>The callback to create <see cref="T:System.Security.Cryptography.CryptoStream"/> with <see cref="Transform"/> object.</value>
      public Func<Stream> CryptoStream { get; }

      /// <summary>
      /// Gets the callback to get hash bytes from <see cref="Transform"/>.
      /// </summary>
      /// <value>The callback to get hash bytes from <see cref="Transform"/>.</value>
      public Func<Byte[]> HashGetter { get; }

      /// <summary>
      /// Gets the callback to compute hash for given content using <see cref="Transform"/>.
      /// </summary>
      /// <value>The callback to compute hash for given content using <see cref="Transform"/>.</value>
      public Func<Byte[], Byte[]> HashComputer { get; }

      /// <summary>
      /// Gets the <see cref="T:System.Security.Cryptography.HashAlgorithm"/> transform object.
      /// </summary>
      /// <value>The <see cref="T:System.Security.Cryptography.HashAlgorithm"/> transform object.</value>
      public IDisposable Transform { get; }
   }

   /// <summary>
   /// This is identical to System.Security.Cryptography.RSAParameters struct, which is missing from PCL.
   /// </summary>
   public struct RSAParameters
   {

      /// <summary>
      /// Represents the <c>D</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] D;

      /// <summary>
      /// Represents the <c>DP</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] DP;

      /// <summary>
      /// Represents the <c>DQ</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] DQ;

      /// <summary>
      /// Represents the <c>Exponent</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] Exponent;

      /// <summary>
      /// Represents the <c>InverseQ</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] InverseQ;

      /// <summary>
      /// Represents the <c>Modulus</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] Modulus;

      /// <summary>
      /// Represents the <c>P</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] P;

      /// <summary>
      /// Represents the <c>Q</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] Q;
   }

#pragma warning disable 1591
   // Most info from http://msdn.microsoft.com/en-us/library/cc250013.aspx 
   // Note: This class will become internal when merging CAM.Physical DLL.
   public static class CryptoUtils
   {
      private const UInt32 RSA1 = 0x31415352;
      private const UInt32 RSA2 = 0x32415352;
      private const Int32 CAPI_HEADER_SIZE = 12;
      private const Byte PRIVATE_KEY = 0x07;
      private const Byte PUBLIC_KEY = 0x06;

      internal static Boolean TryCreateRSAParametersFromCapiBLOB( Byte[] blob, Int32 offset, out Int32 pkLen, out Int32 algID, out RSAParameters result, out String errorString )
      {
         pkLen = 0;
         algID = 0;
         var startOffset = offset;
         errorString = null;
         var retVal = false;
         result = new RSAParameters();
         if ( startOffset + CAPI_HEADER_SIZE < blob.Length )
         {
            try
            {
               var firstByte = blob[offset++];
               if ( ( firstByte == PRIVATE_KEY || firstByte == PUBLIC_KEY ) // Check whether this is private or public key blob
                   && blob.ReadByteFromBytes( ref offset ) == 0x02 // Version (0x02)
                  )
               {
                  blob.Skip( ref offset, 2 ); // Skip reserved (short, should be zero)
                  algID = blob.ReadInt32LEFromBytes( ref offset ); // alg-id (should be either 0x0000a400 for RSA_KEYX or 0x00002400 for RSA_SIGN)
                  var keyType = blob.ReadUInt32LEFromBytes( ref offset );
                  if ( keyType == RSA2 // RSA2
                      || keyType == RSA1 ) // RSA1
                  {
                     var isPrivateKey = firstByte == PRIVATE_KEY;


                     var bitLength = blob.ReadInt32LEFromBytes( ref offset );
                     var byteLength = bitLength >> 3;
                     var byteHalfLength = byteLength >> 1;

                     // Exponent
                     var exp = blob.CreateAndBlockCopyTo( ref offset, 4 );
                     //if ( BitConverter.IsLittleEndian )
                     //{
                     Array.Reverse( exp );
                     //}
                     var tmp = 0;
                     // Trim exponent
                     while ( exp[tmp] == 0 )
                     {
                        ++tmp;
                     }
                     if ( tmp != 0 )
                     {
                        exp = exp.Skip( tmp ).ToArray();
                     }
                     result.Exponent = exp;

                     // Modulus
                     result.Modulus = blob.CreateAndBlockCopyTo( ref offset, byteLength );
                     pkLen = offset - startOffset;

                     if ( isPrivateKey )
                     {

                        // prime1
                        result.P = blob.CreateAndBlockCopyTo( ref offset, byteHalfLength );

                        // prime2
                        result.Q = blob.CreateAndBlockCopyTo( ref offset, byteHalfLength );

                        // exponent1
                        result.DP = blob.CreateAndBlockCopyTo( ref offset, byteHalfLength );

                        // exponent2
                        result.DQ = blob.CreateAndBlockCopyTo( ref offset, byteHalfLength );

                        // coefficient
                        result.InverseQ = blob.CreateAndBlockCopyTo( ref offset, byteHalfLength );

                        // private exponent
                        result.D = blob.CreateAndBlockCopyTo( ref offset, byteLength );
                     }

                     //if ( BitConverter.IsLittleEndian )
                     //{
                     // Reverse arrays, since they are stored in big-endian format in BLOB
                     Array.Reverse( result.Modulus );
                     //if ( isPrivateKey )
                     //{
                     Array.Reverse( result.P );
                     Array.Reverse( result.Q );
                     Array.Reverse( result.DP );
                     Array.Reverse( result.DQ );
                     Array.Reverse( result.InverseQ );
                     Array.Reverse( result.D );
                     //}
                     //}

                     retVal = true;
                  }
                  else
                  {
                     errorString = "Invalid key type: " + keyType;
                  }
               }
               else
               {
                  errorString = "Invalid BLOB header.";
               }
            }
            catch
            {
               errorString = "Invalid BLOB";
            }
         }

         return retVal;
      }

      public static Boolean TryCreateSigningInformationFromKeyBLOB( Byte[] blob, AssemblyHashAlgorithm? signingAlgorithm, out Byte[] publicKey, out AssemblyHashAlgorithm actualSigningAlgorithm, out RSAParameters rsaParameters, out String errorString )
      {
         // There might be actual key after a header, if first byte is zero.
         var hasHeader = blob[0] == 0x00;
         Int32 pkLen, algID;
         var retVal = TryCreateRSAParametersFromCapiBLOB( blob, hasHeader ? CAPI_HEADER_SIZE : 0, out pkLen, out algID, out rsaParameters, out errorString );

         if ( retVal )
         {

            publicKey = new Byte[pkLen + CAPI_HEADER_SIZE];
            var idx = 0;
            if ( hasHeader )
            {
               // Just copy from blob
               publicKey.BlockCopyFrom( ref idx, blob, 0, publicKey.Length );
               idx = 4;
               if ( signingAlgorithm.HasValue )
               {
                  actualSigningAlgorithm = signingAlgorithm.Value;
                  publicKey.WriteInt32LEToBytes( ref idx, (Int32) actualSigningAlgorithm );
               }
               else
               {
                  actualSigningAlgorithm = (AssemblyHashAlgorithm) publicKey.ReadInt32LEFromBytes( ref idx );
               }
            }
            else
            {
               // Write public key, including header
               // Write header explicitly. ALG-ID, followed by AssemblyHashAlgorithmID, followed by the size of the PK
               actualSigningAlgorithm = signingAlgorithm ?? AssemblyHashAlgorithm.SHA1;// Defaults to SHA1
               publicKey
                  .WriteInt32LEToBytes( ref idx, algID )
                  .WriteInt32LEToBytes( ref idx, (Int32) actualSigningAlgorithm )
                  .WriteInt32LEToBytes( ref idx, pkLen )
                  .BlockCopyFrom( ref idx, blob, 0, pkLen );
            }
            // Mark PK actually being PK
            publicKey[CAPI_HEADER_SIZE] = PUBLIC_KEY;

            // Set public key algorithm to RSA1
            idx = 20;
            publicKey.WriteUInt32LEToBytes( ref idx, RSA1 );
         }
         else
         {
            publicKey = null;
            actualSigningAlgorithm = AssemblyHashAlgorithm.SHA1;
         }

         return retVal;
      }
   }


}

// This class will get its documentation from CAM.Physical Core.
#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{

   ///// <summary>
   ///// Helper method to create a new <see cref="HashStreamInfo"/> struct, and check that its properties are not <c>null</c>.
   ///// </summary>
   ///// <param name="callbacks">This <see cref="CryptoCallbacks"/>.</param>
   ///// <param name="algorithm">The <see cref="AssemblyHashAlgorithm"/>.</param>
   ///// <param name="checkCryptoStream">Whether to check <see cref="HashStreamInfo.CryptoStream"/> property for <c>null</c> value.</param>
   ///// <param name="checkHashGetter">Whether to check <see cref="HashStreamInfo.HashGetter"/> property for <c>null</c> value.</param>
   ///// <param name="checkComputeHash">Whether to check <see cref="HashStreamInfo.HashComputer"/> property for <c>null</c> value.</param>
   ///// <param name="checkTransform">Whether to check <see cref="HashStreamInfo.Transform"/> property for <c>null</c> value.</param>
   ///// <returns>The <see cref="HashStreamInfo"/> returned by <see cref="CryptoCallbacks.CreateHashStream"/> method.</returns>
   ///// <exception cref="ArgumentNullException">If any of the boolean parameters is <c>true</c>, and the corresponding property of <see cref="HashStreamInfo"/> is <c>null</c>.</exception>
   ///// <exception cref="NullReferenceException">If this <see cref="CryptoCallbacks"/> is <c>null</c>.</exception>
   //public static HashStreamInfo CreateHashStreamAndCheck(
   //      this CryptoCallbacks callbacks,
   //      AssemblyHashAlgorithm algorithm,
   //      Boolean checkCryptoStream,
   //      Boolean checkHashGetter,
   //      Boolean checkComputeHash,
   //      Boolean checkTransform
   //      )
   //{
   //   var retVal = callbacks.CreateHashStream( algorithm );
   //   if ( checkCryptoStream )
   //   {
   //      ArgumentValidator.ValidateNotNull( "Crypto stream", retVal.CryptoStream );
   //   }
   //   if ( checkHashGetter )
   //   {
   //      ArgumentValidator.ValidateNotNull( "Hash getter", retVal.HashGetter );
   //   }
   //   if ( checkComputeHash )
   //   {
   //      ArgumentValidator.ValidateNotNull( "Hash computer", retVal.HashComputer );
   //   }
   //   if ( checkTransform )
   //   {
   //      ArgumentValidator.ValidateNotNull( "Transform", retVal.Transform );
   //   }
   //   return retVal;
   //}

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

   /// <summary>
   /// Helper method to call <see cref="CryptoCallbacks.CreateRSASignature"/> method and check that the return value is not <c>null</c>.
   /// </summary>
   /// <param name="callbacks">This <see cref="CryptoCallbacks"/>.</param>
   /// <param name="rsa">The RSA object returned by <see cref="CryptoCallbacks.CreateRSAFromCSPContainer"/> or <see cref="CryptoCallbacks.CreateRSAFromParameters"/>.</param>
   /// <param name="algorithmName">The algorithm name to use when creating signature.</param>
   /// <param name="contentsHash">The binary data to create signature from.</param>
   /// <returns>Non-<c>null</c> signature bytes.</returns>
   /// <exception cref="ArgumentNullException">If return value of <see cref="CryptoCallbacks.CreateRSASignature"/> is <c>null</c>.</exception>
   /// <exception cref="NullReferenceException">If this <see cref="CryptoCallbacks"/> is <c>null</c>.</exception>
   public static Byte[] CreateRSASignatureAndCheck( this CryptoCallbacks callbacks, IDisposable rsa, String algorithmName, Byte[] contentsHash )
   {
      var retVal = callbacks.CreateRSASignature( rsa, algorithmName, contentsHash );
      ArgumentValidator.ValidateNotNull( "RSA signature", retVal );
      return retVal;
   }

   ///// <summary>
   ///// Helper method to compute public key token for a given full public key.
   ///// </summary>
   ///// <param name="streamInfo">The <see cref="HashStreamInfo"/> object created by <see cref="CryptoCallbacks"/>.</param>
   ///// <param name="fullPublicKey">The full public key.</param>
   ///// <returns>The value of <paramref name="fullPublicKey"/>, if <paramref name="fullPublicKey"/> is <c>null</c> or empty, or the public key token of the <paramref name="fullPublicKey"/>.</returns>
   ///// <exception cref="InvalidOperationException">If <see cref="HashStreamInfo.Algorithm"/> is not <see cref="AssemblyHashAlgorithm.SHA1"/>.</exception>
   ///// <exception cref="ArgumentNullException">If <see cref="HashStreamInfo.Transform"/> or <see cref="HashStreamInfo.HashComputer"/> is <c>null</c>.</exception>
   //public static Byte[] ComputePublicKeyToken( this HashStreamInfo streamInfo, Byte[] fullPublicKey )
   //{
   //   Byte[] retVal;
   //   if ( fullPublicKey.IsNullOrEmpty() )
   //   {
   //      retVal = fullPublicKey;
   //   }
   //   else
   //   {
   //      if ( streamInfo.Algorithm != AssemblyHashAlgorithm.SHA1 )
   //      {
   //         throw new InvalidOperationException( "Hash algorithm must be " + AssemblyHashAlgorithm.SHA1 + "." );
   //      }
   //      ArgumentValidator.ValidateNotNull( "Transform", streamInfo.Transform );
   //      ArgumentValidator.ValidateNotNull( "Hash computer", streamInfo.HashComputer );

   //      retVal = streamInfo.HashComputer( fullPublicKey );
   //      retVal = retVal.Skip( retVal.Length - 8 ).Reverse().ToArray();
   //   }
   //   return retVal;
   //}


}