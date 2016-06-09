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
   /// This class provides skeleton implementation for <see cref="CryptoCallbacks"/>.
   /// </summary>
   public abstract class AbstractCryptoCallbacks<TRSA> : AbstractDisposable, CryptoCallbacks
   {
      /// <summary>
      /// Creates a new instance of <see cref="AbstractCryptoCallbacks{TRSA}"/>.
      /// </summary>
      public AbstractCryptoCallbacks()
      {

      }

      /// <inheritdoc />
      protected override void Dispose( Boolean disposing )
      {
         //if ( disposing )
         //{
         //   BlockDigestAlgorithm algo;
         //   while ( ( algo = this._sha1Pool.TakeInstance() ) != null )
         //   {
         //      algo.DisposeSafely();
         //   }
         //}
      }

      /// <inheritdoc />
      public abstract BlockDigestAlgorithm CreateHashAlgorithm( AssemblyHashAlgorithm algorithm );

      /// <inheritdoc />
      public Byte[] CreateSignature( Byte[] contentsHash, KeyBLOBParsingResult parsingResult, String containerName )
      {
         var rsaInfo = parsingResult as RSAKeyBLOBParsingResult;
         return rsaInfo == null ?
            this.CreateNonRSASignature( contentsHash, parsingResult, containerName ) :
            this.CreateRSASignature( contentsHash, rsaInfo, containerName );
      }

      /// <inheritdoc />
      public abstract Byte[] ExtractPublicKeyFromCSPContainer( String containerName );

      /// <inheritdoc />
      public abstract IEnumerable<Byte> EnumeratePublicKeyToken( Byte[] fullPublicKey );

      /// <inheritdoc />
      public virtual KeyBLOBParsingResult TryParseKeyBLOB( Byte[] keyBLOB, AssemblyHashAlgorithm? hashAlgorithmOverride )
      {
         RSAParameters rParams; Byte[] publicKey; AssemblyHashAlgorithm signingAlgorithm; String errorMessage;
         var success = TryCreateSigningInformationFromKeyBLOB( keyBLOB, hashAlgorithmOverride, out publicKey, out signingAlgorithm, out rParams, out errorMessage );
         return success ? new RSAKeyBLOBParsingResult( rParams, publicKey, signingAlgorithm ) : new RSAKeyBLOBParsingResult( errorMessage );
      }

      /// <summary>
      /// Creates a strong-name signature using RSA algorithm.
      /// </summary>
      /// <param name="contentsHash">The generated hash.</param>
      /// <param name="parsingResult">The optional parsed <see cref="RSAKeyBLOBParsingResult"/>.</param>
      /// <param name="containerName">The optional CSP container name.</param>
      /// <returns>The strong-name signature created with RSA algorithm.</returns>
      protected Byte[] CreateRSASignature( Byte[] contentsHash, RSAKeyBLOBParsingResult parsingResult, String containerName )
      {
         var rParams = parsingResult.RSAParameters;
         var rsa = String.IsNullOrEmpty( containerName ) ? this.CreateRSAFromParameters( rParams ) : this.CreateRSAFromCSPContainer( containerName );
         try
         {
            return this.DoCreateRSASignature( parsingResult.HashAlgorithm, contentsHash, rParams, rsa );
         }
         finally
         {
            ( rsa as IDisposable )?.DisposeSafely();
         }
      }

      /// <summary>
      /// This method is called by <see cref="CreateSignature"/> when the RSA is needed to be created from CSP container name.
      /// </summary>
      /// <param name="containerName">The CSP container name. Will be non-empty, non-<c>null</c> string.</param>
      /// <returns>A new instance of RSA algorithm.</returns>
      protected abstract TRSA CreateRSAFromCSPContainer( String containerName );

      /// <summary>
      /// This method is called by <see cref="CreateSignature"/> when the RSA is needed to be created from <see cref="RSAParameters"/>.
      /// </summary>
      /// <param name="parameters">The RSA parameters.</param>
      /// <returns>A new instance of RSA algorithm.</returns>
      protected abstract TRSA CreateRSAFromParameters( RSAParameters parameters );

      /// <summary>
      /// This method is called by <see cref="CreateSignature"/> to actually create signature.
      /// </summary>
      /// <param name="hashAlgorithm">The hash algorithm used to produce the hash.</param>
      /// <param name="contentsHash">The hash that was produced.</param>
      /// <param name="parameters">The <see cref="RSAParameters"/> passed to <see cref="CreateSignature"/> method.</param>
      /// <param name="rsa">The instance of RSA algorithm, as created by one of <see cref="CreateRSAFromParameters"/> or <see cref="CreateRSAFromCSPContainer"/> methods.</param>
      /// <returns>A RSA signature of the hash.</returns>
      protected abstract Byte[] DoCreateRSASignature( AssemblyHashAlgorithm hashAlgorithm, Byte[] contentsHash, RSAParameters parameters, TRSA rsa );

      /// <summary>
      /// Subclasses may override this method to handle creation of signature which is done in some other algorithm than RSA.
      /// </summary>
      /// <param name="contentsHash">The generated hash.</param>
      /// <param name="parsingResult">The optional parsed <see cref="RSAKeyBLOBParsingResult"/>.</param>
      /// <param name="containerName">The optional CSP container name.</param>
      /// <returns>The strong-name signature.</returns>
      protected virtual Byte[] CreateNonRSASignature( Byte[] contentsHash, KeyBLOBParsingResult parsingResult, String containerName )
      {
         throw new NotSupportedException( "Not supported parsing info: " + parsingResult );
      }


      private const UInt32 RSA1 = 0x31415352;
      private const UInt32 RSA2 = 0x32415352;
      private const Int32 CAPI_HEADER_SIZE = 12;
      private const Byte PRIVATE_KEY = 0x07;
      private const Byte PUBLIC_KEY = 0x06;

      // Most info from http://msdn.microsoft.com/en-us/library/cc250013.aspx 
      private static Boolean TryCreateRSAParametersFromCapiBLOB( Byte[] blob, Int32 offset, out Int32 pkLen, out Int32 algID, out RSAParameters result, out String errorString )
      {
         pkLen = 0;
         algID = 0;
         var startOffset = offset;
         errorString = null;
         var retVal = false;
         Byte[] exp, mod, p, q, dp, dq, iq, d;
         exp = mod = p = q = dp = dq = iq = d = null;
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

                     // Public exponent
                     const Int32 EXP_LEN = 4;
                     exp = blob.CreateAndBlockCopyTo( ref offset, EXP_LEN );
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

                     // Modulus
                     mod = blob.CreateAndBlockCopyTo( ref offset, byteLength );
                     pkLen = offset - startOffset;

                     if ( isPrivateKey )
                     {

                        // prime1
                        p = blob.CreateAndBlockCopyTo( ref offset, byteHalfLength );

                        // prime2
                        q = blob.CreateAndBlockCopyTo( ref offset, byteHalfLength );

                        // exponent1
                        dp = blob.CreateAndBlockCopyTo( ref offset, byteHalfLength );

                        // exponent2
                        dq = blob.CreateAndBlockCopyTo( ref offset, byteHalfLength );

                        // coefficient
                        iq = blob.CreateAndBlockCopyTo( ref offset, byteHalfLength );

                        // private exponent
                        d = blob.CreateAndBlockCopyTo( ref offset, byteLength );
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

         result = exp == null || mod == null ?
            default( RSAParameters ) :
            new RSAParameters( BinaryEndianness.LittleEndian, mod, exp, d, p, q, dp, dq, iq );

         return retVal;
      }

      private static Boolean TryCreateSigningInformationFromKeyBLOB( Byte[] blob, AssemblyHashAlgorithm? signingAlgorithm, out Byte[] publicKey, out AssemblyHashAlgorithm actualSigningAlgorithm, out RSAParameters rsaParameters, out String errorString )
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

   /// <summary>
   /// This class represents parsed key BLOB which contained RSA key information.
   /// </summary>
   public class RSAKeyBLOBParsingResult : KeyBLOBParsingResult
   {
      /// <summary>
      /// Creates new instance of <see cref="RSAKeyBLOBParsingResult"/> with given error message.
      /// </summary>
      /// <param name="errorMessage">The error message. If none is specified, then it will be <c>"Unknown error"</c>.</param>
      public RSAKeyBLOBParsingResult( String errorMessage ) :
         base( 0, null, AssemblyHashAlgorithm.None, errorMessage ?? "Unknown error" )
      {

      }

      /// <summary>
      /// Creates new instance of <see cref="RSAKeyBLOBParsingResult"/> with given parameters.
      /// </summary>
      /// <param name="parameters">The <see cref="Crypto.RSAParameters"/>.</param>
      /// <param name="publicKey">The public key to use when emitting.</param>
      /// <param name="hashAlgorithm">The hash algorithm to use.</param>
      public RSAKeyBLOBParsingResult( RSAParameters parameters, Byte[] publicKey, AssemblyHashAlgorithm hashAlgorithm )
         : base( parameters.Modulus.Length, publicKey, hashAlgorithm, null )
      {
         this.RSAParameters = parameters;
      }

      /// <summary>
      /// Gets the parsed <see cref="Crypto.RSAParameters"/>.
      /// </summary>
      /// <value>The parsed <see cref="Crypto.RSAParameters"/>.</value>
      public RSAParameters RSAParameters { get; }


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

   /// <summary>
   /// This class provides default implementation for cryptographic functionality required by CAM.Physical framework.
   /// The support for hash algorithms and for RSA is provided, but support for CSP is not.
   /// </summary>
   public class DefaultCryptoCallbacks : AbstractCryptoCallbacks<CipherAlgorithm<RSAComputingParameters>>
   {
      private static IDictionary<AssemblyHashAlgorithm, ASN1ObjectIdentifier> ObjIDCache { get; }

      /// <summary>
      /// Gets the default instance of <see cref="DefaultCryptoCallbacks"/>, which uses normal RSA algorithm instead of blinded one.
      /// </summary>
      public static DefaultCryptoCallbacks NonBlindedInstance { get; }

      static DefaultCryptoCallbacks()
      {
         ObjIDCache = new Dictionary<AssemblyHashAlgorithm, ASN1ObjectIdentifier>();
         NonBlindedInstance = new DefaultCryptoCallbacks();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="randomToUse"></param>
      /// <returns></returns>
      /// <remarks>
      /// Please note that if <paramref name="randomToUse"/> is not <c>null</c>, it will *not* be disposed when the returned <see cref="DefaultCryptoCallbacks"/> is disposed.
      /// </remarks>
      public static DefaultCryptoCallbacks CreateDefaultInstance( Random randomToUse = null )
      {
         return new DefaultCryptoCallbacks( () => new RSABlindedAlgorithm( RSAAlgorithm.DefaultInstance, randomToUse ) );
      }

      private readonly Func<CipherAlgorithm<RSAComputingParameters>> _rsaFactory;

      /// <summary>
      /// Creates a new instance of <see cref="DefaultCryptoCallbacks"/> with optional given <see cref="CipherAlgorithm{TParameters}"/>, usually <see cref="RSAAlgorithm"/>.
      /// </summary>
      /// <param name="algorithmFactory">The callback to create RSA algorithm. If none is given, then such callback is created, that <see cref="RSAAlgorithm.DefaultInstance"/> is used.</param>
      public DefaultCryptoCallbacks( Func<CipherAlgorithm<RSAComputingParameters>> algorithmFactory = null )
      {
         if ( algorithmFactory == null )
         {
            algorithmFactory = () => RSAAlgorithm.DefaultInstance;
         }
         this._rsaFactory = algorithmFactory;
      }

      /// <summary>
      /// This will create an instance of <see cref="BlockDigestAlgorithm"/> by following rules:
      /// <list type="table">
      /// <listheader>
      /// <term>The <see cref="AssemblyHashAlgorithm"/> value</term>
      /// <term>The return value type</term>
      /// </listheader>
      /// <item>
      /// <term><see cref="AssemblyHashAlgorithm.MD5"/></term>
      /// <term><see cref="MD5"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="AssemblyHashAlgorithm.SHA1"/></term>
      /// <term><see cref="SHA1_128"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="AssemblyHashAlgorithm.SHA256"/></term>
      /// <term><see cref="SHA2_256"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="AssemblyHashAlgorithm.SHA384"/></term>
      /// <term><see cref="SHA2_384"/></term>
      /// </item>
      /// <item>
      /// <term><see cref="AssemblyHashAlgorithm.SHA512"/></term>
      /// <term><see cref="SHA2_512"/></term>
      /// </item>
      /// </list>
      /// </summary>
      /// <param name="algorithm"></param>
      /// <returns></returns>
      public override BlockDigestAlgorithm CreateHashAlgorithm( AssemblyHashAlgorithm algorithm )
      {
         switch ( algorithm )
         {
            case AssemblyHashAlgorithm.MD5:
               return new MD5();
            case AssemblyHashAlgorithm.SHA1:
               return new SHA1_128();
            case AssemblyHashAlgorithm.SHA256:
               return new SHA2_256();
            case AssemblyHashAlgorithm.SHA384:
               return new SHA2_384();
            case AssemblyHashAlgorithm.SHA512:
               return new SHA2_512();
            case AssemblyHashAlgorithm.None:
               throw new InvalidOperationException( "Tried to create hash stream with no hash algorithm" );
            default:
               throw new ArgumentException( "Unknown hash algorithm: " + algorithm + "." );
         }
      }

      /// <summary>
      /// Always throws <see cref="NotSupportedException"/> as CSP is not supported in PCL environment.
      /// </summary>
      /// <param name="containerName">The CSP container name, ignored.</param>
      /// <returns>Never returns value.</returns>
      public override Byte[] ExtractPublicKeyFromCSPContainer( String containerName )
      {
         throw new NotSupportedException( "CSP is not supported in portable environment." );
      }

      /// <inheritdoc />
      public override IEnumerable<Byte> EnumeratePublicKeyToken( Byte[] fullPublicKey )
      {
         return HashAlgorithmPool.SHA1.EnumeratePublicKeyToken( fullPublicKey );
      }

      /// <summary>
      /// Always throws <see cref="NotSupportedException"/> as CSP is not supported in PCL environment.
      /// </summary>
      /// <param name="containerName">The CSP container name, ignored.</param>
      /// <returns>Never returns value.</returns>
      protected override CipherAlgorithm<RSAComputingParameters> CreateRSAFromCSPContainer( string containerName )
      {
         throw new NotSupportedException( "CSP is not supported in portable environment." );
      }

      /// <summary>
      /// Creates a new instance of <see cref="RSABlindedAlgorithm"/>.
      /// </summary>
      /// <param name="parameters">Ignored.</param>
      /// <returns>A new instance of <see cref="RSABlindedAlgorithm"/>.</returns>
      protected override CipherAlgorithm<RSAComputingParameters> CreateRSAFromParameters( RSAParameters parameters )
      {
         return this._rsaFactory();
      }

      /// <summary>
      /// Formats hash as ASN.1 DER-encoded value, further encoded with PKCS#1 scheme, and then creates signature of the resulting data using given RSA.
      /// </summary>
      /// <param name="hashAlgorithm">The hash algorithm used to create the hash.</param>
      /// <param name="contentsHash">The hash value.</param>
      /// <param name="parameters"></param>
      /// <param name="rsa"></param>
      /// <returns></returns>
      protected override Byte[] DoCreateRSASignature( AssemblyHashAlgorithm hashAlgorithm, Byte[] contentsHash, RSAParameters parameters, CipherAlgorithm<RSAComputingParameters> rsa )
      {
         // 1. Format data
         var pkcs = PKCS1Encoder.Create(
            parameters.Modulus.Length * 8,
            ASNFormatter.Create(
               contentsHash,
               ObjIDCache.GetOrAdd_NotThreadSafe( hashAlgorithm, algo => new ASN1ObjectIdentifier( algo.GetObjectIdentifier() ) )
            )
         );
         var data = new Byte[pkcs.DataSize];
         pkcs.PopulateData( data, 0 );

         // 2. Create signature
         var output = new Byte[pkcs.DataSize];
         // The input data is treated as big-endian number, but the actual signature stored in the StrongNameSignature sector is little-endian
         rsa.ProcessBlock( data, 0, data.Length, output, 0, new RSAComputingParameters( parameters, BinaryEndianness.BigEndian, BinaryEndianness.LittleEndian ) );
         return output;
      }
   }



}
