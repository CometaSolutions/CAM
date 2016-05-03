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
   public interface BlockTransform
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="data"></param>
      /// <param name="offset"></param>
      /// <param name="count"></param>
      void TransformBlock( Byte[] data, Int32 offset, Int32 count );

      /// <summary>
      /// 
      /// </summary>
      /// <param name="data"></param>
      /// <param name="offset"></param>
      /// <param name="count"></param>
      /// <returns></returns>
      Byte[] TransformFinalBlock( Byte[] data, Int32 offset, Int32 count );
   }

   /// <summary>
   /// 
   /// </summary>
   public abstract class AbstractBlockTransform : BlockTransform
   {

      private readonly Byte[] _block;
      private UInt64 _count;
      private Boolean _stateResetDone;

      /// <summary>
      /// 
      /// </summary>
      protected AbstractBlockTransform( Int32 blockByteCount )
      {
         this._stateResetDone = false;
         this._block = new Byte[blockByteCount];
      }

      /// <inheritdoc />
      public void TransformBlock( Byte[] data, Int32 offset, Int32 count )
      {
         if ( !this._stateResetDone )
         {
            this.ResetState( false );
            this._stateResetDone = true;
         }

         this.HashBlock( data, offset, count );
      }

      /// <inheritdoc />
      public Byte[] TransformFinalBlock( Byte[] data, Int32 offset, Int32 count )
      {
         if ( data != null )
         {
            this.HashBlock( data, offset, count );
         }
         var retVal = this.HashEnd();
         this.Reset();
         return retVal;
      }

      private void Reset()
      {
         Array.Clear( this._block, 0, this._block.Length );
         this._count = 0UL;
         this._stateResetDone = false;
         this.ResetState( true );
      }

      private void HashBlock( Byte[] data, Int32 offset, Int32 count )
      {
         var block = this._block;
         var blockSize = block.Length;
         var blockOffset = (Int32) ( this._count & (UInt64) ( blockSize - 1 ) );
         this._count += (UInt32) count;

         // 1. Transform the previous data first
         if ( blockOffset > 0 && blockOffset + count >= blockSize )
         {
            var remainder = blockSize - blockOffset;
            data.BlockCopyTo( ref offset, block, blockOffset, remainder );
            count -= remainder;
            blockOffset = 0;
            this.DoTransform( block );
         }

         // 2. Transform block at a time
         while ( count >= blockSize )
         {
            data.BlockCopyTo( ref offset, block, 0, blockSize );
            count -= blockSize;
            this.DoTransform( block );
         }

         // 3. Copy in the remaining data
         if ( count > 0 )
         {
            data.BlockCopyTo( ref offset, block, blockOffset, count );
         }
      }

      private Byte[] HashEnd()
      {
         // We will write 9 more bytes (byte + ulong)
         // Round up by block size
         var count = this._count;
         var data = new Byte[( count + (UInt64) this.CountIncreaseForHashEnd ).RoundUpU64( (UInt32) this._block.Length ) - count];
         // Write value 128 at the beginning, and amount of written bits at the end of the data
         var idx = 0;
         data.WriteByteToBytes( ref idx, 128 )
            .WriteUInt64BEToBytesNoRef( data.Length - 8, count * 8 );

         // Hash the data
         this.HashBlock( data, 0, data.Length );

         // Transform state integers into byte array
         var hash = new Byte[this.HashByteCount];
         this.PopulateHash( hash );
         return hash;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="block"></param>
      protected abstract void DoTransform( Byte[] block );

      /// <summary>
      /// 
      /// </summary>
      protected abstract Int32 CountIncreaseForHashEnd { get; }

      /// <summary>
      /// 
      /// </summary>
      protected abstract Int32 HashByteCount { get; }

      /// <summary>
      /// 
      /// </summary>
      protected abstract void PopulateHash( Byte[] hash );

      /// <summary>
      /// 
      /// </summary>
      /// <param name="isHashDone"></param>
      protected abstract void ResetState( Boolean isHashDone );
   }

   /// <summary>
   /// 
   /// </summary>
   [CLSCompliant( false )]
   public abstract class SHA32BitWord : AbstractBlockTransform
   {
      private const Int32 BLOCK_SIZE = 0x40;

      private readonly UInt32[] _x;
      private readonly UInt32[] _state;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="expandedBlockSize"></param>
      /// <param name="stateSize"></param>
      protected SHA32BitWord( Int32 expandedBlockSize, Int32 stateSize )
         : base( BLOCK_SIZE )
      {
         this._x = new UInt32[expandedBlockSize];
         this._state = new UInt32[stateSize];
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="isHashDone"></param>
      protected override void ResetState( Boolean isHashDone )
      {
         // Clear X buffer
         if ( isHashDone )
         {
            Array.Clear( this._x, 0, this._x.Length );
         }

         // Reset the state to its initial values
         this.ResetStateIntegers( this._state );
      }

      /// <summary>
      /// 
      /// </summary>
      protected override Int32 CountIncreaseForHashEnd
      {
         get
         {
            return 9;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      protected override Int32 HashByteCount
      {
         get
         {
            return this._state.Length * sizeof( UInt32 );
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected override void PopulateHash( Byte[] hash )
      {
         var idx = 0;
         var state = this._state;
         for ( var i = 0; i < state.Length; ++i )
         {
            hash.WriteUInt32BEToBytes( ref idx, state[i] );
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="block"></param>
      protected override void DoTransform( Byte[] block )
      {
         // 1. Write data to X
         var x = this._x;
         var i = 0;
         var idx = 0;
         for ( ; i < BLOCK_SIZE / sizeof( UInt32 ); ++i )
         {
            x[i] = block.ReadUInt32BEFromBytes( ref idx );
         }

         // 2. Expand X
         for ( ; i < x.Length; ++i )
         {
            x[i] = this.Expand( x, i );
         }

         // 3. Do the actual transform
         this.DoTransformAfterExpanding( x, this._state );
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="state"></param>
      protected abstract void ResetStateIntegers( UInt32[] state );

      /// <summary>
      /// 
      /// </summary>
      /// <param name="x"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      protected abstract UInt32 Expand( UInt32[] x, Int32 idx );

      /// <summary>
      /// 
      /// </summary>
      /// <param name="x"></param>
      /// <param name="state"></param>
      protected abstract void DoTransformAfterExpanding( UInt32[] x, UInt32[] state );

   }

   /// <summary>
   /// TODO
   /// </summary>
   [CLSCompliant( false )]
   public sealed class SHA128 : SHA32BitWord
   {

      /// <summary>
      /// 
      /// </summary>
      public SHA128()
         : base( 0x50, 0x05 )
      {
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="state"></param>
      protected override void ResetStateIntegers( UInt32[] state )
      {
         // Initial state of SHA1
         state[0] = 0x67452301;
         state[1] = 0xefcdab89;
         state[2] = 0x98badcfe;
         state[3] = 0x10325476;
         state[4] = 0xc3d2e1f0;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="x"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      protected override UInt32 Expand( UInt32[] x, Int32 idx )
      {
         var t = x[idx - 3] ^ x[idx - 8] ^ x[idx - 14] ^ x[idx - 16];
         return t.RotateLeft( 1 );
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="x"></param>
      /// <param name="state"></param>
      protected override void DoTransformAfterExpanding( UInt32[] x, UInt32[] state )
      {
         // Consts
         const UInt32 Y1 = 0x5a827999;
         const UInt32 Y2 = 0x6ed9eba1;
         const UInt32 Y3 = 0x8f1bbcdc;
         const UInt32 Y4 = 0xca62c1d6;

         // Prepare variables
         var h1 = state[0];
         var h2 = state[1];
         var h3 = state[2];
         var h4 = state[3];
         var h5 = state[4];

         // Phase 1
         var i = 0;
         for ( ; i < 20; i += 5 )
         {
            h5 += h1.RotateLeft( 5 ) + F( h2, h3, h4 ) + x[i] + Y1;
            h2 = h2.RotateLeft( 30 );

            h4 += h5.RotateLeft( 5 ) + F( h1, h2, h3 ) + x[i + 1] + Y1;
            h1 = h1.RotateLeft( 30 );

            h3 += h4.RotateLeft( 5 ) + F( h5, h1, h2 ) + x[i + 2] + Y1;
            h5 = h5.RotateLeft( 30 );

            h2 += h3.RotateLeft( 5 ) + F( h4, h5, h1 ) + x[i + 3] + Y1;
            h4 = h4.RotateLeft( 30 );

            h1 += h2.RotateLeft( 5 ) + F( h3, h4, h5 ) + x[i + 4] + Y1;
            h3 = h3.RotateLeft( 30 );
         }

         // Phase 2
         for ( ; i < 40; i += 5 )
         {
            h5 += h1.RotateLeft( 5 ) + H( h2, h3, h4 ) + x[i] + Y2;
            h2 = h2.RotateLeft( 30 );

            h4 += h5.RotateLeft( 5 ) + H( h1, h2, h3 ) + x[i + 1] + Y2;
            h1 = h1.RotateLeft( 30 );

            h3 += h4.RotateLeft( 5 ) + H( h5, h1, h2 ) + x[i + 2] + Y2;
            h5 = h5.RotateLeft( 30 );

            h2 += h3.RotateLeft( 5 ) + H( h4, h5, h1 ) + x[i + 3] + Y2;
            h4 = h4.RotateLeft( 30 );

            h1 += h2.RotateLeft( 5 ) + H( h3, h4, h5 ) + x[i + 4] + Y2;
            h3 = h3.RotateLeft( 30 );
         }

         // Phase 3
         for ( ; i < 60; i += 5 )
         {
            h5 += h1.RotateLeft( 5 ) + G( h2, h3, h4 ) + x[i] + Y3;
            h2 = h2.RotateLeft( 30 );

            h4 += h5.RotateLeft( 5 ) + G( h1, h2, h3 ) + x[i + 1] + Y3;
            h1 = h1.RotateLeft( 30 );

            h3 += h4.RotateLeft( 5 ) + G( h5, h1, h2 ) + x[i + 2] + Y3;
            h5 = h5.RotateLeft( 30 );

            h2 += h3.RotateLeft( 5 ) + G( h4, h5, h1 ) + x[i + 3] + Y3;
            h4 = h4.RotateLeft( 30 );

            h1 += h2.RotateLeft( 5 ) + G( h3, h4, h5 ) + x[i + 4] + Y3;
            h3 = h3.RotateLeft( 30 );
         }

         // Phase 4
         for ( ; i < 80; i += 5 )
         {
            h5 += h1.RotateLeft( 5 ) + H( h2, h3, h4 ) + x[i] + Y4;
            h2 = h2.RotateLeft( 30 );

            h4 += h5.RotateLeft( 5 ) + H( h1, h2, h3 ) + x[i + 1] + Y4;
            h1 = h1.RotateLeft( 30 );

            h3 += h4.RotateLeft( 5 ) + H( h5, h1, h2 ) + x[i + 2] + Y4;
            h5 = h5.RotateLeft( 30 );

            h2 += h3.RotateLeft( 5 ) + H( h4, h5, h1 ) + x[i + 3] + Y4;
            h4 = h4.RotateLeft( 30 );

            h1 += h2.RotateLeft( 5 ) + H( h3, h4, h5 ) + x[i + 4] + Y4;
            h3 = h3.RotateLeft( 30 );
         }

         // Update state
         state[0] += h1;
         state[1] += h2;
         state[2] += h3;
         state[3] += h4;
         state[4] += h5;

      }

      private static UInt32 F( UInt32 u, UInt32 v, UInt32 w )
      {
         return ( u & v ) | ( ~u & w );
      }

      private static UInt32 H( UInt32 u, UInt32 v, UInt32 w )
      {
         return u ^ v ^ w;
      }

      private static UInt32 G( UInt32 u, UInt32 v, UInt32 w )
      {
         return ( u & v ) | ( u & w ) | ( v & w );
      }
   }

   /// <summary>
   /// 
   /// </summary>
   [CLSCompliant( false )]
   public sealed class SHA256 : SHA32BitWord
   {
      /// <summary>
      /// 
      /// </summary>
      public SHA256()
         : base( 0x40, 0x08 )
      {

      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="state"></param>
      protected override void ResetStateIntegers( UInt32[] state )
      {
         // SHA-256 initial hash value: The first 32 bits of the fractional parts of the square roots of the first eight prime numbers
         state[0] = 0x6a09e667;
         state[1] = 0xbb67ae85;
         state[2] = 0x3c6ef372;
         state[3] = 0xa54ff53a;
         state[4] = 0x510e527f;
         state[5] = 0x9b05688c;
         state[6] = 0x1f83d9ab;
         state[7] = 0x5be0cd19;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="x"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      protected override UInt32 Expand( UInt32[] x, Int32 idx )
      {
         return Theta1( x[idx - 2] ) + x[idx - 7] + Theta0( x[idx - 15] ) + x[idx - 16];
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="x"></param>
      /// <param name="state"></param>
      protected override void DoTransformAfterExpanding( UInt32[] x, UInt32[] state )
      {
         // Prepare variables
         var h1 = state[0];
         var h2 = state[1];
         var h3 = state[2];
         var h4 = state[3];
         var h5 = state[4];
         var h6 = state[5];
         var h7 = state[6];
         var h8 = state[7];

         for ( var i = 0; i < 64; )
         {

         }

         // Update state
         state[0] += h1;
         state[1] += h2;
         state[2] += h3;
         state[3] += h4;
         state[4] += h5;
         state[5] += h6;
         state[6] += h7;
         state[7] += h8;
      }

      private static UInt32 Theta0( UInt32 val )
      {
         return val.RotateRight( 7 ) ^ val.RotateRight( 18 ) ^ ( val >> 3 );
      }

      private static UInt32 Theta1( UInt32 val )
      {
         return val.RotateRight( 17 ) ^ val.RotateRight( 19 ) ^ ( val >> 10 );
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
   /// <param name="transform"></param>
   /// <param name="array"></param>
   /// <param name="offset"></param>
   /// <param name="count"></param>
   /// <returns></returns>
   public static Byte[] ComputeHash( this BlockTransform transform, Byte[] array, Int32 offset, Int32 count )
   {
      transform.TransformBlock( array, offset, count );
      return transform.TransformFinalBlock( null, 0, 0 );
   }
}