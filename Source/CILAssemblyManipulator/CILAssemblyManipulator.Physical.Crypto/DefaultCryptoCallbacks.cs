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
   /// <summary>
   /// This class provides skeleton implementation for <see cref="CryptoCallbacks"/>.
   /// </summary>
   public abstract class AbstractCryptoCallbacks<TRSA> : AbstractDisposable, CryptoCallbacks
      where TRSA : IDisposable
   {
      private readonly LocklessInstancePoolForClasses<BlockDigestAlgorithm> _sha1Pool;

      /// <summary>
      /// Creates a new instance of <see cref="AbstractCryptoCallbacks{TRSA}"/>.
      /// </summary>
      public AbstractCryptoCallbacks()
      {
         this._sha1Pool = new LocklessInstancePoolForClasses<BlockDigestAlgorithm>();
      }

      /// <inheritdoc />
      protected override void Dispose( Boolean disposing )
      {
         if ( disposing )
         {
            BlockDigestAlgorithm algo;
            while ( ( algo = this._sha1Pool.TakeInstance() ) != null )
            {
               algo.DisposeSafely();
            }
         }
      }

      /// <inheritdoc />
      public abstract BlockDigestAlgorithm CreateHashAlgorithm( AssemblyHashAlgorithm algorithm );

      /// <inheritdoc />
      public Byte[] CreateRSASignature( AssemblyHashAlgorithm hashAlgorithm, Byte[] contentsHash, RSAParameters rParams, String containerName )
      {
         using ( var rsa = String.IsNullOrEmpty( containerName ) ? this.CreateRSAFromParameters( rParams ) : this.CreateRSAFromCSPContainer( containerName ) )
         {
            return this.DoCreateRSASignature( hashAlgorithm, contentsHash, rParams, rsa );
         }
      }

      /// <inheritdoc />
      public abstract Byte[] ExtractPublicKeyFromCSPContainer( String containerName );

      /// <inheritdoc />
      public Byte[] ComputePublicKeyToken( Byte[] fullPublicKey )
      {
         Byte[] retVal;
         if ( fullPublicKey.IsNullOrEmpty() )
         {
            retVal = fullPublicKey;
         }
         else
         {
            var sha1 = this._sha1Pool.TakeInstance();
            try
            {
               if ( sha1 == null )
               {
                  sha1 = this.CreateHashAlgorithm( AssemblyHashAlgorithm.SHA1 );
               }
               retVal = sha1.ComputeHash( fullPublicKey, 0, fullPublicKey.Length );
               // Public key token is actually last 8 bytes reversed
               retVal = retVal.Skip( retVal.Length - 8 ).ToArray();
               Array.Reverse( retVal );
            }
            finally
            {
               this._sha1Pool.ReturnInstance( sha1 );
            }
         }
         return retVal;
      }

      /// <summary>
      /// This method is called by <see cref="CreateRSASignature"/> when the RSA is needed to be created from CSP container name.
      /// </summary>
      /// <param name="containerName">The CSP container name. Will be non-empty, non-<c>null</c> string.</param>
      /// <returns>A new instance of RSA algorithm.</returns>
      protected abstract TRSA CreateRSAFromCSPContainer( String containerName );

      /// <summary>
      /// This method is called by <see cref="CreateRSASignature"/> when the RSA is needed to be created from <see cref="RSAParameters"/>.
      /// </summary>
      /// <param name="parameters">The RSA parameters.</param>
      /// <returns>A new instance of RSA algorithm.</returns>
      protected abstract TRSA CreateRSAFromParameters( RSAParameters parameters );

      /// <summary>
      /// This method is called by <see cref="CreateRSASignature"/> to actually create signature.
      /// </summary>
      /// <param name="hashAlgorithm">The hash algorithm used to produce the hash.</param>
      /// <param name="contentsHash">The hash that was produced.</param>
      /// <param name="parameters">The <see cref="RSAParameters"/> passed to <see cref="CreateRSASignature"/> method.</param>
      /// <param name="rsa">The instance of RSA algorithm, as created by one of <see cref="CreateRSAFromParameters"/> or <see cref="CreateRSAFromCSPContainer"/> methods.</param>
      /// <returns>A RSA signature of the hash.</returns>
      protected abstract Byte[] DoCreateRSASignature( AssemblyHashAlgorithm hashAlgorithm, Byte[] contentsHash, RSAParameters parameters, TRSA rsa );
   }

   /// <summary>
   /// This class provides default implementation for cryptographic functionality required by CAM.Physical framework.
   /// The support for hash algorithms and for RSA is provided, but support for CSP is not.
   /// </summary>
   public class DefaultCryptoCallbacks : AbstractCryptoCallbacks<RSABlindedAlgorithm>
   {
      private static IDictionary<AssemblyHashAlgorithm, ASN1ObjectIdentifier> ObjIDCache { get; }
      static DefaultCryptoCallbacks()
      {
         ObjIDCache = new Dictionary<AssemblyHashAlgorithm, ASN1ObjectIdentifier>();
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

      /// <summary>
      /// Always throws <see cref="NotSupportedException"/> as CSP is not supported in PCL environment.
      /// </summary>
      /// <param name="containerName">The CSP container name, ignored.</param>
      /// <returns>Never returns value.</returns>
      protected override RSABlindedAlgorithm CreateRSAFromCSPContainer( string containerName )
      {
         throw new NotSupportedException( "CSP is not supported in portable environment." );
      }

      /// <summary>
      /// Creates a new instance of <see cref="RSABlindedAlgorithm"/>.
      /// </summary>
      /// <param name="parameters">Ignored.</param>
      /// <returns>A new instance of <see cref="RSABlindedAlgorithm"/>.</returns>
      protected override RSABlindedAlgorithm CreateRSAFromParameters( RSAParameters parameters )
      {
         return new RSABlindedAlgorithm();
      }

      /// <summary>
      /// Formats hash as ASN.1 DER-encoded value, further encoded with PKCS#1 scheme, and then creates signature of the resulting data using given RSA.
      /// </summary>
      /// <param name="hashAlgorithm">The hash algorithm used to create the hash.</param>
      /// <param name="contentsHash">The hash value.</param>
      /// <param name="parameters"></param>
      /// <param name="rsa"></param>
      /// <returns></returns>
      protected override Byte[] DoCreateRSASignature( AssemblyHashAlgorithm hashAlgorithm, Byte[] contentsHash, RSAParameters parameters, RSABlindedAlgorithm rsa )
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
         rsa.ProcessBlock( data, 0, data.Length, output, 0, new RSAComputedParameters( parameters ) );
         return output;
      }
   }


}
