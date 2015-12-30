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
using CILAssemblyManipulator.Physical.IO;
using CILAssemblyManipulator.Physical.Crypto;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.Crypto
{
   public interface CryptoCallbacks
   {
      Byte[] ExtractPublicKeyFromCSPContainer( String containterName );
      HashStreamInfo CreateHashStream( AssemblyHashAlgorithm algorithm );
      IDisposable CreateRSAFromCSPContainer( String containerName );
      IDisposable CreateRSAFromParameters( RSAParameters parameters );
      Byte[] CreateRSASignature( IDisposable rsa, String hashAlgorithmName, Byte[] contentsHash );
   }

   /// <summary>
   /// The event argument class used by <see cref="CILReflectionContext.HashStreamLoadEvent"/> event.
   /// </summary>
   public struct HashStreamInfo
   {
      private readonly AssemblyHashAlgorithm _algorithm;
      private readonly Func<Stream> _cryptoStream;
      private readonly Func<Byte[]> _hashGetter;
      private readonly Func<Byte[], Byte[]> _hashComputer;
      private readonly IDisposable _transform;

      public HashStreamInfo( AssemblyHashAlgorithm algorithm, Func<Stream> cryptoStream, Func<Byte[]> hashGetter, Func<Byte[], Byte[]> hashComputer, IDisposable transform )
      {
         this._algorithm = algorithm;
         this._cryptoStream = cryptoStream;
         this._hashGetter = hashGetter;
         this._hashComputer = hashComputer;
         this._transform = transform;
      }

      public AssemblyHashAlgorithm Algorithm
      {
         get
         {
            return this._algorithm;
         }
      }

      public Func<Stream> CryptoStream
      {
         get
         {
            return this._cryptoStream;
         }
      }

      public Func<Byte[]> HashGetter
      {
         get
         {
            return this._hashGetter;
         }
      }

      public Func<Byte[], Byte[]> ComputeHash
      {
         get
         {
            return this._hashComputer;
         }
      }

      public IDisposable Transform
      {
         get
         {
            return this._transform;
         }
      }
   }

   /// <summary>
   /// This is identical to System.Security.Cryptography.RSAParameters struct.
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

   // Most info from http://msdn.microsoft.com/en-us/library/cc250013.aspx 
   internal static class CryptoUtils
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

      internal static Boolean TryCreateSigningInformationFromKeyBLOB( Byte[] blob, AssemblyHashAlgorithm? signingAlgorithm, out Byte[] publicKey, out AssemblyHashAlgorithm actualSigningAlgorithm, out RSAParameters rsaParameters, out String errorString )
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

public static partial class E_CILPhysical
{
   public static HashStreamInfo CreateHashStreamAndCheck(
      this CryptoCallbacks callbacks,
      AssemblyHashAlgorithm algorithm,
      Boolean checkCryptoStream,
      Boolean checkHashGetter,
      Boolean checkComputeHash,
      Boolean checkTransform
      )
   {
      var retVal = callbacks.CreateHashStream( algorithm );
      if ( checkCryptoStream )
      {
         ArgumentValidator.ValidateNotNull( "Crypto stream", retVal.CryptoStream );
      }
      if ( checkHashGetter )
      {
         ArgumentValidator.ValidateNotNull( "Hash getter", retVal.HashGetter );
      }
      if ( checkComputeHash )
      {
         ArgumentValidator.ValidateNotNull( "Hash computer", retVal.ComputeHash );
      }
      if ( checkTransform )
      {
         ArgumentValidator.ValidateNotNull( "Transform", retVal.Transform );
      }
      return retVal;
   }

   public static Byte[] ExtractPublicKeyFromCSPContainerAndCheck( this CryptoCallbacks callbacks, String containerName )
   {
      var retVal = callbacks.ExtractPublicKeyFromCSPContainer( containerName );
      if ( retVal.IsNullOrEmpty() )
      {
         throw new InvalidOperationException( "The public key of CSP \"" + containerName + "\" could not be resolved." );
      }
      return retVal;
   }

   public static Byte[] CreateRSASignatureAndCheck( this CryptoCallbacks callbacks, IDisposable rsa, String algorithmName, Byte[] contentsHash )
   {
      var retVal = callbacks.CreateRSASignature( rsa, algorithmName, contentsHash );
      ArgumentValidator.ValidateNotNull( "RSA signature", retVal );
      return retVal;
   }

   public static Byte[] ComputePublicKeyToken( this HashStreamInfo streamInfo, Byte[] fullPublicKey )
   {
      Byte[] retVal;
      if ( fullPublicKey.IsNullOrEmpty() )
      {
         retVal = fullPublicKey;
      }
      else
      {
         if ( streamInfo.Algorithm != AssemblyHashAlgorithm.SHA1 )
         {
            throw new InvalidOperationException( "Hash algorithm must be " + AssemblyHashAlgorithm.SHA1 + "." );
         }
         ArgumentValidator.ValidateNotNull( "Transform", streamInfo.Transform );
         ArgumentValidator.ValidateNotNull( "Hash computer", streamInfo.ComputeHash );

         retVal = streamInfo.ComputeHash( fullPublicKey );
         retVal = retVal.Skip( retVal.Length - 8 ).Reverse().ToArray();
      }
      return retVal;
   }

   public static Byte[] CreatePublicKeyFromStrongName( this CryptoCallbacks eArgs, StrongNameKeyPair strongName, AssemblyHashAlgorithm? algorithmOverride = null )
   {
      Byte[] retVal;
      if ( strongName == null )
      {
         retVal = null;
      }
      else
      {
         var container = strongName.ContainerName;
         if ( container == null )
         {
            String errorString;
            AssemblyHashAlgorithm signingAlgorithm;
            RSAParameters rParams;
            CryptoUtils.TryCreateSigningInformationFromKeyBLOB(
               strongName.KeyPair.ToArray(),
               algorithmOverride,
               out retVal,
               out signingAlgorithm,
               out rParams,
               out errorString
               );
         }
         else
         {
            retVal = eArgs.ExtractPublicKeyFromCSPContainer( container );
         }
      }

      return retVal;
   }

   public static Boolean IsMatch( this AssemblyDefinition aDef, AssemblyReference aRef, HashStreamInfo? hashStreamInfo )
   {
      return aDef.IsMatch( new AssemblyInformationForResolving( aRef ), aRef.Attributes.IsRetargetable(), hashStreamInfo );
   }

   public static Boolean IsMatch( this AssemblyDefinition aDef, AssemblyInformationForResolving? aRef, Boolean isRetargetable, HashStreamInfo? hashStreamInfo )
   {
      var retVal = aDef != null
         && aRef != null;
      if ( retVal )
      {
         var aReff = aRef.Value;
         var defInfo = aDef.AssemblyInformation;
         var refInfo = aReff.AssemblyInformation;
         if ( isRetargetable )
         {
            retVal = String.Equals( defInfo.Name, refInfo.Name );
         }
         else
         {
            var defPK = defInfo.PublicKeyOrToken;
            var refPK = refInfo.PublicKeyOrToken;
            retVal = defPK.IsNullOrEmpty() == refPK.IsNullOrEmpty()
               && defInfo.Equals( refInfo, aReff.IsFullPublicKey )
               && ( aReff.IsFullPublicKey || ( hashStreamInfo.HasValue && ArrayEqualityComparer<Byte>.ArrayEquality( hashStreamInfo.Value.ComputePublicKeyToken( defPK ), refPK ) ) );
         }
      }

      return retVal;
   }
}