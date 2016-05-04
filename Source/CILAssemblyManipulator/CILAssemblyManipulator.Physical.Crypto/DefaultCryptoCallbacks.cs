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
   public abstract class AbstractCryptoCallbacks : AbstractDisposable, CryptoCallbacks
   {
      private readonly LocklessInstancePoolForClasses<BlockHashAlgorithm> _sha1Pool;

      /// <summary>
      /// Creates a new instance of <see cref="AbstractCryptoCallbacks"/>.
      /// </summary>
      public AbstractCryptoCallbacks()
      {
         this._sha1Pool = new LocklessInstancePoolForClasses<BlockHashAlgorithm>();
      }

      /// <inheritdoc />
      protected override void Dispose( Boolean disposing )
      {
         if ( disposing )
         {
            BlockHashAlgorithm algo;
            while ( ( algo = this._sha1Pool.TakeInstance() ) != null )
            {
               algo.DisposeSafely();
            }
         }
      }

      /// <inheritdoc />
      public abstract BlockHashAlgorithm CreateHashAlgorithm( AssemblyHashAlgorithm algorithm );

      /// <inheritdoc />
      public abstract Byte[] CreateRSASignature( AssemblyHashAlgorithm hashAlgorithm, Byte[] contentsHash, RSAParameters rParams, String containerName );

      /// <inheritdoc />
      public abstract Byte[] ExtractPublicKeyFromCSPContainer( String containterName );

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


   }

   internal class DefaultCryptoCallbacks : AbstractCryptoCallbacks
   {
      public override BlockHashAlgorithm CreateHashAlgorithm( AssemblyHashAlgorithm algorithm )
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

      public override Byte[] ExtractPublicKeyFromCSPContainer( String containterName )
      {
         throw new NotSupportedException( "CSP is not supported in portable environment." );
      }

      public override Byte[] CreateRSASignature( AssemblyHashAlgorithm hashAlgorithm, Byte[] contentsHash, RSAParameters rParams, String containerName )
      {
         throw new NotImplementedException();
      }
   }


}
