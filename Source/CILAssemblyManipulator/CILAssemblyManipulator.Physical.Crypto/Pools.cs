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

namespace CILAssemblyManipulator.Physical.Crypto
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="TInstance"></typeparam>
   public class InstancePoolUser<TInstance>
      where TInstance : class
   {

      /// <summary>
      /// 
      /// </summary>
      /// <param name="factory"></param>
      public InstancePoolUser( Func<TInstance> factory )
      {
         this.Factory = ArgumentValidator.ValidateNotNull( "Factory", factory );
         this.Pool = new LocklessInstancePoolForClasses<TInstance>();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="action"></param>
      protected void UseInstance( Action<TInstance> action )
      {
         var instance = this.Pool.TakeInstance();
         try
         {
            if ( instance == null )
            {
               instance = this.Factory();
            }
            action( instance );
         }
         finally
         {
            this.Pool.ReturnInstance( instance );
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TResult"></typeparam>
      /// <param name="function"></param>
      /// <returns></returns>
      protected TResult UseInstance<TResult>( Func<TInstance, TResult> function )
      {
         var instance = this.Pool.TakeInstance();
         try
         {
            if ( instance == null )
            {
               instance = this.Factory();
            }
            return function( instance );
         }
         finally
         {
            this.Pool.ReturnInstance( instance );
         }
      }

      /// <summary>
      /// 
      /// </summary>
      protected LocklessInstancePoolForClasses<TInstance> Pool { get; }

      /// <summary>
      /// 
      /// </summary>
      protected Func<TInstance> Factory { get; }
   }

   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="THashAlgorithm"></typeparam>
   public class HashAlgorithmInstancePoolUser<THashAlgorithm> : InstancePoolUser<THashAlgorithm>
      where THashAlgorithm : class, BlockDigestAlgorithm
   {
      private readonly LocklessInstancePoolForClasses<Byte[]> _byteArrays = new LocklessInstancePoolForClasses<Byte[]>();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="factory"></param>
      public HashAlgorithmInstancePoolUser( Func<THashAlgorithm> factory )
         : base( factory )
      {
         this._byteArrays = new LocklessInstancePoolForClasses<Byte[]>();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="contents"></param>
      /// <param name="digestCount"></param>
      /// <returns></returns>
      public IEnumerable<Byte> ComputeHash( Byte[] contents, Int32 digestCount )
      {
         var algorithm = this.Pool.TakeInstance();
         try
         {
            if ( algorithm == null )
            {
               algorithm = this.Factory();
            }
            var bytes = this._byteArrays.TakeInstance();
            try
            {
               if ( bytes == null )
               {
                  bytes = new Byte[algorithm.DigestByteCount];
               }
               algorithm.ProcessBlock( contents );
               algorithm.WriteDigest( bytes );
               var max = Math.Min( digestCount, bytes.Length );
               for ( var i = 0; i < max; ++i )
               {
                  yield return bytes[i];
               }
            }
            finally
            {
               this._byteArrays.ReturnInstance( bytes );
            }
         }
         finally
         {
            this.Pool.ReturnInstance( algorithm );
         }
         //return this.UseInstance( algorithm => this.ComputeHash( algorithm, algorithmUser ) );
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="contents"></param>
      /// <param name="digestCount"></param>
      /// <returns></returns>
      public IEnumerable<Byte> ComputeHashReverse( Byte[] contents, Int32 digestCount )
      {
         var algorithm = this.Pool.TakeInstance();
         try
         {
            if ( algorithm == null )
            {
               algorithm = this.Factory();
            }
            var bytes = this._byteArrays.TakeInstance();
            try
            {
               if ( bytes == null )
               {
                  bytes = new Byte[algorithm.DigestByteCount];
               }
               algorithm.ProcessBlock( contents );
               algorithm.WriteDigest( bytes );
               var max = Math.Min( digestCount, bytes.Length );
               for ( var i = 0; i < max; ++i )
               {
                  yield return bytes[bytes.Length - i - 1];
               }
            }
            finally
            {
               this._byteArrays.ReturnInstance( bytes );
            }
         }
         finally
         {
            this.Pool.ReturnInstance( algorithm );
         }
         //return this.UseInstance( algorithm => this.ComputeHash( algorithm, algorithmUser ) );
      }

      //private IEnumerable<Byte> ComputeHash(THashAlgorithm algorithm, Action<THashAlgorithm> algorithmUser )
      //{
      //   var bytes = this._byteArrays.TakeInstance();
      //   try
      //   {
      //      if ( bytes == null )
      //      {
      //         bytes = new Byte[algorithm.DigestByteCount];
      //      }
      //      algorithmUser( algorithm );
      //      algorithm.WriteDigest( bytes );
      //      // Public key token is actually last 8 bytes reversed
      //      for ( var i = 0; i < 8; ++i )
      //      {
      //         yield return bytes[bytes.Length - i - 1];
      //      }
      //   }
      //   finally
      //   {
      //      this._byteArrays.ReturnInstance( bytes );
      //   }
      //}
   }

   /// <summary>
   /// 
   /// </summary>
   public static class HashAlgorithmPool
   {

      /// <summary>
      /// 
      /// </summary>
      [CLSCompliant( false )]
      public static HashAlgorithmInstancePoolUser<SHA1_128> SHA1 { get; }

      /// <summary>
      /// 
      /// </summary>
      [CLSCompliant( false )]
      public static HashAlgorithmInstancePoolUser<SHA2_256> SHA256 { get; }

      /// <summary>
      /// 
      /// </summary>
      [CLSCompliant( false )]
      public static HashAlgorithmInstancePoolUser<SHA2_512> SHA512 { get; }

      static HashAlgorithmPool()
      {
         SHA1 = new HashAlgorithmInstancePoolUser<SHA1_128>( () => new SHA1_128() );
         SHA256 = new HashAlgorithmInstancePoolUser<SHA2_256>( () => new SHA2_256() );
         SHA512 = new HashAlgorithmInstancePoolUser<SHA2_512>( () => new SHA2_512() );
      }
   }
}

#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{
   /// <summary>
   /// 
   /// </summary>
   /// <param name="instancePoolUser"></param>
   /// <param name="fullPublicKey"></param>
   /// <returns></returns>
   [CLSCompliant( false )]
   public static IEnumerable<Byte> EnumeratePublicKeyToken( this HashAlgorithmInstancePoolUser<SHA1_128> instancePoolUser, Byte[] fullPublicKey )
   {
      // Public key token is actually last 8 bytes reversed
      return instancePoolUser.ComputeHashReverse( fullPublicKey, 8 );
   }
}