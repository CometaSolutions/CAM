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
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CommonUtils
{
   /// <summary>
   /// This class combines together the stream and the accessible buffer to read data into.
   /// </summary>
   public class StreamHelper
   {

      /// <summary>
      /// Creates a new instance of <see cref="StreamHelper"/> for given stream.
      /// </summary>
      /// <param name="stream">The <see cref="System.IO.Stream"/>.</param>
      /// <param name="bufferLength">The buffer length, default value of <c>8</c> is enough to read all primitives.</param>
      public StreamHelper( Stream stream, Int32 bufferLength = 8 )
      {
         ArgumentValidator.ValidateNotNull( "Stream", stream );

         this.Stream = stream;
         this.Buffer = new Byte[bufferLength];
      }

      /// <summary>
      /// Gets the <see cref="System.IO.Stream"/>.
      /// </summary>
      public Stream Stream { get; }

      /// <summary>
      /// Gets the buffer to read data to.
      /// </summary>
      public Byte[] Buffer { get; }
   }

   /// <summary>
   /// Represents a read-only portion of stream, where read and seek operations are only allowed to be within given boundaries.
   /// </summary>
   public class StreamPortion : Stream
   {

      private readonly Stream _stream;

      /// <summary>
      /// Creates a new <see cref="StreamPortion"/> for given stream, and with given minimum and maximum boundaries (inclusive).
      /// </summary>
      /// <param name="stream">The <see cref="Stream"/>.</param>
      /// <param name="min">The minimum boundary, inclusive.</param>
      /// <param name="max">The maximum boundary, exclusive.</param>
      public StreamPortion(
         Stream stream,
         Int64 min,
         Int64 max
         )
      {
         ArgumentValidator.ValidateNotNull( "Stream", stream );

         if ( min < 0 || max < 0 )
         {
            throw new ArgumentException( "Min and max boundaries must be at least zero." );
         }
         else if ( min > max )
         {
            throw new ArgumentException( "Max boundary must be at least min boundary." );
         }

         var otherPortion = stream as StreamPortion;
         this._stream = otherPortion == null ? stream : otherPortion._stream;
         this.MinPosition = min;
         this.MaxPosition = max;
      }

      /// <summary>
      /// Gets the minimum positon for this <see cref="StreamPortion"/>, inclusive.
      /// </summary>
      public Int64 MinPosition { get; }

      /// <summary>
      /// Gets the maximum position for this <see cref="StreamPortion"/>, inclusive.
      /// </summary>
      public Int64 MaxPosition { get; }


      /// <inheritdoc />
      public override Boolean CanRead
      {
         get
         {
            return this._stream.CanRead;
         }
      }

      /// <inheritdoc />
      public override Boolean CanSeek
      {
         get
         {
            return this._stream.CanSeek;
         }
      }

      /// <inheritdoc />
      public override Boolean CanWrite
      {
         get
         {
            return false;
         }
      }

      /// <inheritdoc />
      public override Int64 Length
      {
         get
         {
            return this.MinPosition + this.MaxPosition + 1;
         }
      }

      /// <inheritdoc />
      public override Int64 Position
      {
         get
         {
            return this._stream.Position;
         }
         set
         {
            this.CheckPosition( value, 0 );
            this._stream.Position = value;
         }
      }

      /// <inheritdoc />
      public override void Flush()
      {
         throw new NotSupportedException();
      }

      /// <inheritdoc />
      public override Int32 Read( Byte[] buffer, Int32 offset, Int32 count )
      {
         this.CheckPosition( this.GetCurrentPosition(), count );
         return this._stream.Read( buffer, offset, count );
      }

      /// <inheritdoc />
      public override Int64 Seek( Int64 offset, SeekOrigin origin )
      {
         Int64 newOffset;
         switch ( origin )
         {
            case SeekOrigin.Begin:
               newOffset = offset;
               break;
            case SeekOrigin.Current:
               newOffset = this.Position + offset;
               break;
            case SeekOrigin.End:
               newOffset = this.Length + offset;
               break;
            default:
               ;
               throw new ArgumentException( "Invalid seek origin: " + origin + "." );
         }

         this.CheckPosition( newOffset, 0 );

         return this._stream.Seek( offset, origin );
      }

      /// <inheritdoc />
      public override void SetLength( Int64 value )
      {
         this._stream.SetLength( value );
      }

      /// <inheritdoc />
      public override void Write( Byte[] buffer, Int32 offset, Int32 count )
      {
         throw new NotSupportedException();
      }

      /// <summary>
      /// Gets the current position for this stream. Default implementation uses <see cref="Stream.Position"/> property.
      /// </summary>
      /// <returns>Current position for this stream.</returns>
      public virtual Int64 GetCurrentPosition()
      {
         return this._stream.Position;
      }

      /// <summary>
      /// Throws <see cref="NotSupportedException"/> if given position is invalid for this <see cref="StreamPortion"/>.
      /// </summary>
      /// <param name="position">The position to check.</param>
      /// <param name="count">The amount of elements to read/write.</param>
      protected void CheckPosition( Int64 position, Int32 count )
      {
         if ( position < this.MinPosition || position + count > this.MaxPosition )
         {
            throw new NotSupportedException( "New offset " + position + " is out of bounds for this stream portion." );
         }
      }
   }
}

public static partial class E_CommonUtils
{
   /// <summary>
   /// Tries to find out whether it is possible to read next given amount of bytes from stream.
   /// Will take into account that <paramref name="stream"/> may be <see cref="StreamPortion"/>.
   /// </summary>
   /// <param name="stream">The <see cref="Stream"/>.</param>
   /// <param name="byteCount">The bytes to read.</param>
   /// <returns><c>true</c>, if next <paramref name="byteCount"/> bytes can certainly be read from stream; <c>false</c> if they certainly can not; and <c>null</c> if it is undeterminate whether the bytes can be read.</returns>
   public static Boolean? CanReadNextBytes( this Stream stream, Int32 byteCount )
   {
      StreamPortion s;
      if ( stream.CanSeek )
      {
         return stream.Position + byteCount <= stream.Length;
      }
      else if ( ( s = stream as StreamPortion ) != null )
      {
         return s.GetCurrentPosition() + byteCount <= s.MaxPosition;
      }
      else
      {
         return null;
      }
   }

   /// <summary>
   /// Creates a new <see cref="StreamHelper"/> over a portion of stream, starting at current offset, and able to read next <paramref name="byteCount"/> bytes.
   /// </summary>
   /// <param name="helper">This stream helper.</param>
   /// <param name="byteCount">The amount of bytes that the resulting stream helper will be able to read, starting from inclusive current offset.</param>
   /// <returns>A new <see cref="StreamHelper"/> that is able to read only given portion of bytes.</returns>
   /// <seealso cref="StreamPortion"/>
   public static StreamHelper NewStreamPortionFromCurrent( this StreamHelper helper, Int64 byteCount )
   {
      return helper.NewStreamPortion( helper.Stream.Position, byteCount );
   }

   /// <summary>
   /// Creates a new <see cref="StreamHelper"/> over a portion of stream, starting at given offset, and able to read next <paramref name="byteCount"/> bytes.
   /// </summary>
   /// <param name="helper">This stream helper.</param>
   /// <param name="offset">The inclusive offset at which reading bytes is eligible.</param>
   /// <param name="byteCount">The amount of bytes that the resulting stream helper will be able to read, starting from inclusive given offset.</param>
   /// <returns>A new <see cref="StreamHelper"/> that is able to read only given portion of bytes.</returns>
   /// <seealso cref="StreamPortion"/>
   public static StreamHelper NewStreamPortion( this StreamHelper helper, Int64 offset, Int64 byteCount )
   {
      var stream = helper.Stream;
      return new StreamHelper( new StreamPortion( stream, offset, offset + Math.Max( 1, byteCount ) - 1 ) );
   }

   /// <summary>
   /// Reads one byte a time from a given <see cref="StreamHelper"/> until a zero byte is encountered, and creates an array containing bytes read.
   /// </summary>
   /// <param name="helper">The <see cref="StreamHelper"/>.</param>
   /// <param name="includeZeroByte">Whether the value byte, if encountered, should be included in the resulting array.</param>
   /// <returns>The array containing bytes read.</returns>
   public static Byte[] ReadUntilZeroAndCreateArray( this StreamHelper helper, Boolean includeZeroByte = false )
   {
      return helper.ReadUntilAndCreateArray( 0, includeZeroByte );
   }

   /// <summary>
   /// Reads one byte a time from a given <see cref="StreamHelper"/> until a given byte is encountered, and creates an array containing bytes read.
   /// </summary>
   /// <param name="helper">The <see cref="StreamHelper"/>.</param>
   /// <param name="value">The value to encounter.</param>
   /// <param name="includeValueByte">Whether the value byte, if encountered, should be included in the resulting array.</param>
   /// <returns>The array containing bytes read.</returns>
   public static Byte[] ReadUntilAndCreateArray( this StreamHelper helper, Byte value, Boolean includeValueByte = false )
   {
      var startPosition = helper.Stream.Position;
      var endPosition = helper.ReadUntil( value ).Stream.Position;
      var len = endPosition - startPosition;
      if ( len > 0 && !includeValueByte )
      {
         --len;
      }

      return len == 0 ? Empty<Byte>.Array : helper.At( startPosition ).ReadAndCreateArray( checked((Int32) len) );
   }

   /// <summary>
   /// Reads one byte at a time from a given <see cref="StreamHelper"/> until a zero byte value is encountered.
   /// </summary>
   /// <param name="helper">The <see cref="StreamHelper"/>.</param>
   /// <returns></returns>
   public static StreamHelper ReadUntilZero( this StreamHelper helper )
   {
      return helper.ReadUntil( 0 );
   }

   /// <summary>
   /// Reads one byte at a time from a given <see cref="StreamHelper"/> until a given byte value is encountered.
   /// </summary>
   /// <param name="helper">The <see cref="StreamHelper"/>.</param>
   /// <param name="value">The value to encounter.</param>
   /// <returns></returns>
   public static StreamHelper ReadUntil( this StreamHelper helper, Byte value )
   {
      while ( helper.Stream.ReadByteFromStream() != value ) ;
      return helper;
   }

   /// <summary>
   /// Sets the position of a <see cref="StreamHelper.Stream"/> at given position, and returns the <see cref="StreamHelper"/>.
   /// </summary>
   /// <param name="helper">The <see cref="StreamHelper"/>.</param>
   /// <param name="position">The new position for <see cref="StreamHelper.Stream"/>.</param>
   /// <returns>The <paramref name="helper"/>.</returns>
   public static StreamHelper At( this StreamHelper helper, Int64 position )
   {
      helper.Stream.Position = position;
      return helper;
   }


   /// <summary>
   /// This method will advance the position of <see cref="StreamHelper.Stream"/> to next alignment.
   /// </summary>
   /// <param name="helper">The <see cref="StreamHelper"/>.</param>
   /// <param name="alignment">The alignment.</param>
   /// <returns>The <paramref name="helper"/>.</returns>
   /// <remarks>
   /// Assumes that <paramref name="alignment"/> is a power of two.
   /// Will return incorrect results if <paramref name="alignment"/> is zero.
   /// </remarks>
   public static StreamHelper SkipToNextAlignment( this StreamHelper helper, Int32 alignment )
   {
      var stream = helper.Stream;
      var oldPos = stream.Position;
      var newPos = BinaryUtils.RoundUpI64( oldPos, alignment );
      if ( newPos > oldPos )
      {
         stream.SeekFromCurrent( newPos - oldPos );
      }
      return helper;
   }


   /// <summary>
   /// Skips the given amount of bytes from current offset of the <see cref="StreamHelper.Stream"/>, and returns <see cref="StreamHelper"/>.
   /// </summary>
   /// <param name="helper">The <see cref="StreamHelper"/>.</param>
   /// <param name="amount">The amount of bytes to skip.</param>
   /// <returns>The <paramref name="helper"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="helper"/> is <c>null</c>.</exception>
   public static StreamHelper Skip( this StreamHelper helper, Int64 amount )
   {
      helper.Stream.SeekFromCurrent( amount );
      return helper;
   }
   private static Byte[] ReadAndReturnArray( this StreamHelper helper, Int32 amount )
   {
      var retVal = helper.Buffer;
      helper.Stream.ReadSpecificAmount( retVal, 0, amount );
      return retVal;
   }

   /// <summary>
   /// Creates a new byte array with given size, reads it completely from stream, and returns the array.
   /// </summary>
   /// <param name="helper">The <see cref="StreamHelper"/>.</param>
   /// <param name="amount">The amount of bytes to read, and subsequentially, how big the returned array will be.</param>
   /// <returns>The byte array with contents read from stream.</returns>
   public static Byte[] ReadAndCreateArray( this StreamHelper helper, Int32 amount )
   {
      Byte[] bytez;
      if ( amount == 0 )
      {
         bytez = Empty<Byte>.Array;
      }
      else
      {
         bytez = new Byte[amount];
         helper.Stream.ReadWholeArray( bytez );
      }
      return bytez;
   }

   /// <summary>
   /// Reads a single byte at specified index in byte array.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <returns>The byte at specified index.</returns>
   public static Byte ReadByteFromBytes( this StreamHelper array )
   {
      return array.ReadAndReturnArray( sizeof( Byte ) )[0];
   }

   /// <summary>
   /// Tries to read a single byte from the stream.
   /// </summary>
   /// <param name="stream">The <see cref="StreamHelper"/>.</param>
   /// <param name="value">This wil hold the byte value that was read from the stream, if read operation was successful.</param>
   /// <returns><c>true</c> if read operation was successful; <c>false</c> otherwise.</returns>
   public static Boolean TryReadByteFromBytes( this StreamHelper stream, out Byte value )
   {
      var s = stream.Stream;
      var sp = s as StreamPortion;
      Int32 b;
      if ( ( sp == null || sp.GetCurrentPosition() < sp.MaxPosition ) && s.Read( stream.Buffer, 0, 1 ) > 0 )
      {
         b = stream.Buffer[0];
      }
      else
      {
         b = -1;
      }
      value = (Byte) b;
      return b >= 0;
   }

   /// <summary>
   /// Reads a single byte as <see cref="SByte"/> at specified index in byte array.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <returns>The byte at specified index casted to <see cref="SByte"/>.</returns>
   [CLSCompliant( false )]
   public static SByte ReadSByteFromBytes( this StreamHelper array )
   {
      return (SByte) array.ReadByteFromBytes();
   }

   /// <summary>
   /// Sets a single byte in byte array at specified offset to given value, and increments the offset.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="aByte">The value to set.</param>
   /// <returns>The <paramref name="array"/>.</returns>
   public static StreamHelper WriteByteToBytes( this StreamHelper array, Byte aByte )
   {
      var bytez = array.Buffer;
      bytez[0] = aByte;
      array.Stream.Write( bytez, sizeof( Byte ) );
      return array;
   }

   /// <summary>
   /// Sets a single byte in byte array at specified offset to given value, and increments the offset.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="sByte">The value to set. Even though it is integer, it is interpreted as signed byte.</param>
   /// <returns>The <paramref name="array"/>.</returns>
   [CLSCompliant( false )]
   public static StreamHelper WriteSByteToBytes( this StreamHelper array, SByte sByte )
   {
      return array.WriteByteToBytes( (Byte) sByte );
   }

   #region Little-Endian Conversions

   /// <summary>
   /// Reads <see cref="Int16"/> starting at specified index in byte array using little-endian decoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <returns>The decoded <see cref="Int16"/>.</returns>
   public static Int16 ReadInt16LEFromBytes( this StreamHelper array )
   {
      var bytez = array.ReadAndReturnArray( sizeof( Int16 ) );
      return (Int16) ( ( bytez[1] << 8 ) | bytez[0] );
   }

   /// <summary>
   /// Reads <see cref="UInt16"/> starting at specified index in byte array using little-endian decoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <returns>The decoded <see cref="UInt16"/>.</returns>
   [CLSCompliant( false )]
   public static UInt16 ReadUInt16LEFromBytes( this StreamHelper array )
   {
      return (UInt16) array.ReadInt16LEFromBytes();
   }


   /// <summary>
   /// Reads <see cref="Int32"/> starting at specified index in byte array using little-endian decoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <returns>The decoded <see cref="Int32"/>.</returns>
   public static Int32 ReadInt32LEFromBytes( this StreamHelper array )
   {
      var bytez = array.ReadAndReturnArray( sizeof( Int32 ) );
      return ( bytez[3] << 24 ) | ( bytez[2] << 16 ) | ( bytez[1] << 8 ) | bytez[0];
   }


   /// <summary>
   /// Reads <see cref="UInt32"/> starting at specified index in byte array using little-endian decoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <returns>The decoded <see cref="UInt32"/>.</returns>
   [CLSCompliant( false )]
   public static UInt32 ReadUInt32LEFromBytes( this StreamHelper array )
   {
      return (UInt32) array.ReadInt32LEFromBytes();
   }

   /// <summary>
   /// Reads <see cref="Int64"/> starting at specified index in byte array using little-endian decoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <returns>The decoded <see cref="Int64"/>.</returns>
   public static Int64 ReadInt64LEFromBytes( this StreamHelper array )
   {
      var bytez = array.ReadAndReturnArray( sizeof( Int64 ) );
      return ( ( (Int64) ( ( bytez[7] << 24 ) | ( bytez[6] << 16 ) | ( bytez[5] << 8 ) | bytez[4] ) ) << 32 )
         | ( (UInt32) ( ( bytez[3] << 24 ) | ( bytez[2] << 16 ) | ( bytez[1] << 8 ) | bytez[0] ) );
   }

   /// <summary>
   /// Reads <see cref="Int64"/> starting at specified index in byte array using little-endian decoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <returns>The decoded <see cref="Int64"/>.</returns>
   [CLSCompliant( false )]
   public static UInt64 ReadUInt64LEFromBytes( this StreamHelper array )
   {
      return (UInt64) array.ReadInt64LEFromBytes();
   }

   /// <summary>
   /// Reads Int32 bits starting at specified index in byte array in little-endian order and changes value to <see cref="Single"/>.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <returns>The decoded <see cref="Single"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="array"/> is <c>null</c>.</exception>
   public static Single ReadSingleLEFromBytes( this StreamHelper array )
   {
      var bytez = array.ReadAndReturnArray( sizeof( Single ) );
      if ( BitConverter.IsLittleEndian )
      {
         return BitConverter.ToSingle( bytez, 0 );
      }
      else
      {
         // Read big-endian Int32, get bytes for it, and convert back to single
         return BitConverter.ToSingle( BitConverter.GetBytes( bytez.ReadInt32BEFromBytesNoRef( 0 ) ), 0 );

      }
   }

   /// <summary>
   /// Reads Int64 bits starting at specified index in byte array in little-endian order and changes value to <see cref="Double"/>.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <returns>The decoded <see cref="Double"/>.</returns>
   public static Double ReadDoubleLEFromBytes( this StreamHelper array )
   {
      return BitConverter.Int64BitsToDouble( array.ReadInt64LEFromBytes() );
   }

   /// <summary>
   /// Reads given amount of integers from byte array starting at given offset, and returns the integers as array.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="intArrayLen">The amount of integers to read.</param>
   /// <returns>The integer array.</returns>
   public static Int32[] ReadInt32ArrayLEFromBytes( this StreamHelper array, Int32 intArrayLen )
   {
      var result = new Int32[intArrayLen];
      for ( var i = 0; i < result.Length; ++i )
      {
         result[i] = array.ReadInt32LEFromBytes();
      }
      return result;
   }

   /// <summary>
   /// Reads given amount of unsigned integers from byte array starting at given offset, and returns the unsigned integers as array.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="intArrayLen">The amount of unsigned integers to read.</param>
   /// <returns>The unsigned integer array.</returns>
   [CLSCompliant( false )]
   public static UInt32[] ReadUInt32ArrayLEFromBytes( this StreamHelper array, Int32 intArrayLen )
   {
      var result = new UInt32[intArrayLen];
      for ( var i = 0; i < result.Length; ++i )
      {
         result[i] = array.ReadUInt32LEFromBytes();
      }
      return result;
   }

   /// <summary>
   /// Writes a given <see cref="Int16"/> in byte array starting at specified offset, using little-endian encoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="value">The <see cref="Int16"/> value to write.</param>
   /// <returns>The <paramref name="array"/>.</returns>
   public static StreamHelper WriteInt16LEToBytes( this StreamHelper array, Int16 value )
   {
      var bytez = array.Buffer;
      bytez.WriteInt16LEToBytesNoRef( 0, value );
      array.Stream.Write( bytez, sizeof( Int16 ) );
      return array;
   }

   /// <summary>
   /// Writes a given <see cref="UInt16"/> in byte array starting at specified offset, using little-endian encoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="value">The <see cref="UInt16"/> value to write.</param>
   /// <returns>The <paramref name="array"/>.</returns>
   [CLSCompliant( false )]
   public static StreamHelper WriteUInt16LEToBytes( this StreamHelper array, UInt16 value )
   {
      return array.WriteInt16LEToBytes( (Int16) value );
   }

   /// <summary>
   /// Writes a given <see cref="Int32"/> in byte array starting at specified offset, using little-endian encoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="value">The <see cref="Int32"/> value to write.</param>
   /// <returns>The <paramref name="array"/>.</returns>
   public static StreamHelper WriteInt32LEToBytes( this StreamHelper array, Int32 value )
   {
      var bytez = array.Buffer;
      bytez.WriteInt32LEToBytesNoRef( 0, value );
      array.Stream.Write( bytez, sizeof( Int32 ) );
      return array;
   }

   /// <summary>
   /// Writes a given <see cref="UInt32"/> in byte array starting at specified offset, using little-endian encoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="value">The <see cref="UInt32"/> value to write.</param>
   /// <returns>The <paramref name="array"/>.</returns>
   [CLSCompliant( false )]
   public static StreamHelper WriteUInt32LEToBytes( this StreamHelper array, UInt32 value )
   {
      return array.WriteInt32LEToBytes( (Int32) value );
   }

   /// <summary>
   /// Writes a given <see cref="Int64"/> in byte array starting at specified offset, using little-endian encoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="value">The <see cref="Int64"/> value to write.</param>
   /// <returns>The <paramref name="array"/>.</returns>
   public static StreamHelper WriteInt64LEToBytes( this StreamHelper array, Int64 value )
   {
      var bytez = array.Buffer;
      bytez.WriteInt64LEToBytesNoRef( 0, value );
      array.Stream.Write( bytez, sizeof( Int64 ) );
      return array;
   }

   /// <summary>
   /// Writes a given <see cref="UInt64"/> in byte array starting at specified offset, using little-endian encoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="value">The <see cref="UInt64"/> value to write.</param>
   /// <returns>The <paramref name="array"/>.</returns>
   [CLSCompliant( false )]
   public static StreamHelper WriteUInt64LEToBytes( this StreamHelper array, UInt64 value )
   {
      return array.WriteInt64LEToBytes( (Int64) value );
   }

   /// <summary>
   /// Writes Int32 bits of given <see cref="Single"/> value in little-endian orger to given array starting at specified offset.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="value">The <see cref="Single"/> value to write.</param>
   /// <returns>The <paramref name="array"/>.</returns>
   public static StreamHelper WriteSingleLEToBytes( this StreamHelper array, Single value )
   {
      var bytez = array.Buffer;
      bytez.WriteSingleLEToBytesNoRef( 0, value );
      array.Stream.Write( bytez, sizeof( Single ) );
      return array;
   }

   /// <summary>
   /// Writes Int64 bits of given <see cref="Double"/> value in little-endian order to given array starting at specified offset.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="value">The <see cref="Double"/> value to write.</param>
   /// <returns>The <paramref name="array"/>.</returns>
   public static StreamHelper WriteDoubleLEToBytes( this StreamHelper array, Double value )
   {
      var bytez = array.Buffer;
      bytez.WriteDoubleLEToBytesNoRef( 0, value );
      array.Stream.Write( bytez, sizeof( Double ) );
      return array;
   }

   #endregion


   #region Big-Endian Conversions

   /// <summary>
   /// Reads <see cref="Int16"/> starting at specified index in byte array using big-endian decoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <returns>The decoded <see cref="Int16"/>.</returns>
   public static Int16 ReadInt16BEFromBytes( this StreamHelper array )
   {
      var bytez = array.ReadAndReturnArray( sizeof( Int16 ) );
      return (Int16) ( ( bytez[0] << 8 ) | bytez[1] );
   }

   /// <summary>
   /// Reads <see cref="UInt16"/> starting at specified index in byte array using big-endian decoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <returns>The decoded <see cref="UInt16"/>.</returns>
   [CLSCompliant( false )]
   public static UInt16 ReadUInt16BEFromBytes( this StreamHelper array )
   {
      return (UInt16) array.ReadInt16BEFromBytes();
   }

   /// <summary>
   /// Reads <see cref="Int32"/> starting at specified index in byte array using big-endian decoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <returns>The decoded <see cref="Int32"/>.</returns>
   public static Int32 ReadInt32BEFromBytes( this StreamHelper array )
   {
      var bytez = array.ReadAndReturnArray( sizeof( Int32 ) );
      return ( bytez[0] << 24 ) | ( bytez[1] << 16 ) | ( bytez[2] << 8 ) | bytez[3];
   }

   /// <summary>
   /// Reads <see cref="UInt32"/> starting at specified index in byte array using big-endian decoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <returns>The decoded <see cref="UInt32"/>.</returns>
   [CLSCompliant( false )]
   public static UInt32 ReadUInt32BEFromBytes( this StreamHelper array )
   {
      return (UInt32) array.ReadInt32BEFromBytes();
   }

   /// <summary>
   /// Reads <see cref="Int64"/> starting at specified index in byte array using big-endian decoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <returns>The decoded <see cref="Int64"/>.</returns>
   public static Int64 ReadInt64BEFromBytes( this StreamHelper array )
   {
      var bytez = array.ReadAndReturnArray( sizeof( Int64 ) );
      return ( ( (Int64) ( ( bytez[0] << 24 ) | ( bytez[1] << 16 ) | ( bytez[2] << 8 ) | bytez[3] ) ) << 32 )
         | ( (UInt32) ( ( bytez[4] << 24 ) | ( bytez[5] << 16 ) | ( bytez[6] << 8 ) | bytez[7] ) );
   }

   /// <summary>
   /// Reads <see cref="Int64"/> starting at specified index in byte array using big-endian decoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <returns>The decoded <see cref="Int64"/>.</returns>
   [CLSCompliant( false )]
   public static UInt64 ReadUInt64BEFromBytes( this StreamHelper array )
   {
      return (UInt64) array.ReadInt64BEFromBytes();
   }

   /// <summary>
   /// Reads Int32 bits starting at specified index in byte array in big-endian order and changes value to <see cref="Single"/>.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <returns>The decoded <see cref="Single"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="array"/> is <c>null</c>.</exception>
   public static Single ReadSingleBEFromBytes( this StreamHelper array )
   {
      var bytez = array.ReadAndReturnArray( sizeof( Single ) );
      if ( BitConverter.IsLittleEndian )
      {
         // Read little-endian Int32, get bytes for it, and convert back to single
         return BitConverter.ToSingle( BitConverter.GetBytes( bytez.ReadInt32LEFromBytesNoRef( 0 ) ), 0 );
      }
      else
      {
         return BitConverter.ToSingle( bytez, 0 );
      }
   }

   /// <summary>
   /// Reads Int64 bits starting at specified index in byte array in big-endian order and changes value to <see cref="Double"/>.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <returns>The decoded <see cref="Double"/>.</returns>
   public static Double ReadDoubleBEFromBytes( this StreamHelper array )
   {
      return BitConverter.Int64BitsToDouble( array.ReadInt64BEFromBytes() );
   }

   /// <summary>
   /// Reads given amount of integers from byte array starting at given offset, and returns the integers as array.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="intArrayLen">The amount of integers to read.</param>
   /// <returns>The integer array.</returns>
   public static Int32[] ReadInt32ArrayBEFromBytes( this StreamHelper array, Int32 intArrayLen )
   {
      var result = new Int32[intArrayLen];
      for ( var i = 0; i < result.Length; ++i )
      {
         result[i] = array.ReadInt32BEFromBytes();
      }
      return result;
   }

   /// <summary>
   /// Reads given amount of unsigned integers from byte array starting at given offset, and returns the unsigned integers as array.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="intArrayLen">The amount of unsigned integers to read.</param>
   /// <returns>The unsigned integer array.</returns>
   [CLSCompliant( false )]
   public static UInt32[] ReadUInt32ArrayBEFromBytes( this StreamHelper array, Int32 intArrayLen )
   {
      var result = new UInt32[intArrayLen];
      for ( var i = 0; i < result.Length; ++i )
      {
         result[i] = array.ReadUInt32BEFromBytes();
      }
      return result;
   }

   /// <summary>
   /// Writes a given <see cref="Int16"/> in byte array starting at specified offset, using big-endian encoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="value">The <see cref="Int16"/> value to write.</param>
   /// <returns>The <paramref name="array"/>.</returns>
   public static StreamHelper WriteInt16BEToBytes( this StreamHelper array, Int16 value )
   {
      var bytez = array.Buffer;
      bytez.WriteInt16BEToBytesNoRef( 0, value );
      array.Stream.Write( bytez, sizeof( Int16 ) );
      return array;
   }

   /// <summary>
   /// Writes a given <see cref="UInt16"/> in byte array starting at specified offset, using big-endian encoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="value">The <see cref="UInt16"/> value to write.</param>
   /// <returns>The <paramref name="array"/>.</returns>
   [CLSCompliant( false )]
   public static StreamHelper WriteUInt16BEToBytes( this StreamHelper array, UInt16 value )
   {
      return array.WriteInt16BEToBytes( (Int16) value );
   }

   /// <summary>
   /// Writes a given <see cref="Int32"/> in byte array starting at specified offset, using big-endian encoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="value">The <see cref="Int32"/> value to write.</param>
   /// <returns>The <paramref name="array"/>.</returns>
   public static StreamHelper WriteInt32BEToBytes( this StreamHelper array, Int32 value )
   {
      var bytez = array.Buffer;
      bytez.WriteInt32BEToBytesNoRef( 0, value );
      array.Stream.Write( bytez, sizeof( Int32 ) );
      return array;
   }

   /// <summary>
   /// Writes a given <see cref="UInt32"/> in byte array starting at specified offset, using big-endian encoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="value">The <see cref="UInt32"/> value to write.</param>
   /// <returns>The <paramref name="array"/>.</returns>
   [CLSCompliant( false )]
   public static StreamHelper WriteUInt32BEToBytes( this StreamHelper array, UInt32 value )
   {
      return array.WriteInt32BEToBytes( (Int32) value );
   }

   /// <summary>
   /// Writes a given <see cref="Int64"/> in byte array starting at specified offset, using big-endian encoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="value">The <see cref="Int64"/> value to write.</param>
   /// <returns>The <paramref name="array"/>.</returns>
   public static StreamHelper WriteInt64BEToBytes( this StreamHelper array, Int64 value )
   {
      var bytez = array.Buffer;
      bytez.WriteInt64BEToBytesNoRef( 0, value );
      array.Stream.Write( bytez, sizeof( Int64 ) );
      return array;
   }

   /// <summary>
   /// Writes a given <see cref="UInt64"/> in byte array starting at specified offset, using big-endian encoding.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="value">The <see cref="UInt64"/> value to write.</param>
   /// <returns>The <paramref name="array"/>.</returns>
   [CLSCompliant( false )]
   public static StreamHelper WriteUInt64BEToBytes( this StreamHelper array, UInt64 value )
   {
      return array.WriteInt64BEToBytes( (Int64) value );
   }

   /// <summary>
   /// Writes Int32 bits of given <see cref="Single"/> value in big-endian orger to given array starting at specified offset.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="value">The <see cref="Single"/> value to write.</param>
   /// <returns>The <paramref name="array"/>.</returns>
   public static StreamHelper WriteSingleBEToBytes( this StreamHelper array, Single value )
   {
      var bytez = array.Buffer;
      bytez.WriteSingleBEToBytesNoRef( 0, value );
      array.Stream.Write( bytez, sizeof( Single ) );
      return array;
   }

   /// <summary>
   /// Writes Int64 bits of given <see cref="Double"/> value in big-endian order to given array starting at specified offset.
   /// </summary>
   /// <param name="array">The byte array.</param>
   /// <param name="value">The <see cref="Double"/> value to write.</param>
   /// <returns>The <paramref name="array"/>.</returns>
   public static StreamHelper WriteDoubleBEToBytes( this StreamHelper array, Double value )
   {
      var bytez = array.Buffer;
      bytez.WriteDoubleBEToBytesNoRef( 0, value );
      array.Stream.Write( bytez, sizeof( Double ) );
      return array;
   }

   #endregion

}