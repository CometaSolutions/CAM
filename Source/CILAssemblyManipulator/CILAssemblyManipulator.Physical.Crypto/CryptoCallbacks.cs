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
      /// <param name="hashAlgorithm">The hash algorithm to use when creating signature.</param>
      /// <param name="contentsHash">The binary contents of the content from which to create signature from.</param>
      /// <param name="rParams">The RSA parameters. Use <c>default(RSAParameters)</c> if RSA parameters are from CSP container name.</param>
      /// <param name="containerName">The container name. Use <c>null</c> or empty string to use <paramref name="rParams"/> for RSA.</param>
      /// <returns>The cryptographic signature created with <paramref name="hashAlgorithm"/> algorithm from <paramref name="contentsHash"/>.</returns>
      Byte[] CreateRSASignature( AssemblyHashAlgorithm hashAlgorithm, Byte[] contentsHash, RSAParameters rParams, String containerName );

      /// <summary>
      /// This method will compute public key token for a given full public key.
      /// </summary>
      /// <param name="fullPublicKey">The full public key.</param>
      /// <returns>The value of <paramref name="fullPublicKey"/>, if <paramref name="fullPublicKey"/> is <c>null</c> or empty, or the public key token of the <paramref name="fullPublicKey"/>.</returns>
      Byte[] ComputePublicKeyToken( Byte[] fullPublicKey );
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
                     const Int32 EXP_LEN = 4;
                     var exp = blob.CreateAndBlockCopyTo( ref offset, EXP_LEN );
                     var tmp = 0;
                     // Trim exponent
                     while ( exp[EXP_LEN - 1 - tmp] == 0 )
                     {
                        ++tmp;
                     }
                     if ( tmp != 0 )
                     {
                        exp = exp.Take( EXP_LEN - tmp ).ToArray();
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
   /// <param name="rParams">The RSA parameters. Use <c>default(RSAParameters)</c> if RSA parameters are from CSP container name.</param>
   /// <param name="containerName">The container name. Use <c>null</c> or empty string to use <paramref name="rParams"/> for RSA.</param>
   /// <param name="hashAlgorithm">The algorithm name to use when creating signature.</param>
   /// <param name="contentsHash">The binary data to create signature from.</param>
   /// <returns>Non-<c>null</c> signature bytes.</returns>
   /// <exception cref="ArgumentNullException">If return value of <see cref="CryptoCallbacks.CreateRSASignature"/> is <c>null</c>.</exception>
   /// <exception cref="NullReferenceException">If this <see cref="CryptoCallbacks"/> is <c>null</c>.</exception>
   public static Byte[] CreateRSASignatureAndCheck( this CryptoCallbacks callbacks, AssemblyHashAlgorithm hashAlgorithm, Byte[] contentsHash, RSAParameters rParams, String containerName )
   {
      return ArgumentValidator.ValidateNotNull( "RSA signature", callbacks.CreateRSASignature( hashAlgorithm, contentsHash, rParams, containerName ) );
   }

}