///*
// * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
// *
// * Licensed  under the  Apache License,  Version 2.0  (the "License");
// * you may not use  this file  except in  compliance with the License.
// * You may obtain a copy of the License at
// *
// *   http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed  under the  License is distributed on an "AS IS" BASIS,
// * WITHOUT  WARRANTIES OR CONDITIONS  OF ANY KIND, either  express  or
// * implied.
// *
// * See the License for the specific language governing permissions and
// * limitations under the License. 
// */
//using System;
//using System.IO;
//using System.Text;
//using CILAssemblyManipulator.Logical.Implementation.Physical;


//internal static class StreamExtensions
//{

//   /// <summary>
//   /// Using specified auxiliary array, reads a <see cref="UInt64"/> from <see cref="Stream"/>.
//   /// </summary>
//   /// <param name="stream">The <see cref="Stream"/>.</param>
//   /// <param name="i64Array">The auxiliary array, must be at least 8 bytes long.</param>
//   /// <returns>The <see cref="UInt64"/> read from current position of the <paramref name="stream"/>.</returns>
//   /// <remarks>
//   /// See <see cref="E_CommonUtils.ReadSpecificAmount(Stream, Byte[], Int32, Int32)"/> for more exceptions.
//   /// </remarks>
//   internal static UInt64 ReadU64( this Stream stream, Byte[] i64Array )
//   {
//      stream.ReadSpecificAmount( i64Array, 0, 8 );
//      var dummy = 0;
//      return i64Array.ReadUInt64LEFromBytes( ref dummy );
//   }

//   /// <summary>
//   /// Using specified auxiliary array, reads a <see cref="UInt32"/> from <see cref="Stream"/>.
//   /// </summary>
//   /// <param name="stream">The <see cref="Stream"/>.</param>
//   /// <param name="i32Array">The auxiliary array, must be at least 4 bytes long.</param>
//   /// <returns>The <see cref="UInt32"/> read from current position of the <paramref name="stream"/>.</returns>
//   /// <remarks>
//   /// See <see cref="E_CommonUtils.ReadSpecificAmount(Stream, Byte[], Int32, Int32)"/> for more exceptions.
//   /// </remarks>
//   internal static UInt32 ReadU32( this Stream stream, Byte[] i32Array )
//   {
//      stream.ReadSpecificAmount( i32Array, 0, 4 );
//      return (UInt32) i32Array.ReadInt32LEFromBytesNoRef( 0 );
//   }

//   /// <summary>
//   /// Using specified auxiliary array, reads a <see cref="UInt16"/> from <see cref="Stream"/>.
//   /// </summary>
//   /// <param name="stream">The <see cref="Stream"/>.</param>
//   /// <param name="i16Array">The auxiliary array, must be at least 2 bytes long.</param>
//   /// <returns>The <see cref="UInt16"/> read from current position of the <paramref name="stream"/>.</returns>
//   /// <remarks>
//   /// See <see cref="E_CommonUtils.ReadSpecificAmount(Stream, Byte[], Int32, Int32)"/> for more exceptions.
//   /// </remarks>
//   internal static UInt16 ReadU16( this Stream stream, Byte[] i16Array )
//   {
//      stream.ReadSpecificAmount( i16Array, 0, 2 );
//      var dummy = 0;
//      return i16Array.ReadUInt16LEFromBytes( ref dummy );
//   }

//   internal static String ReadZeroTerminatedString( this Stream stream, UInt32 length, Encoding encoding )
//   {
//      var buf = new Byte[length];
//      stream.ReadWholeArray( buf );
//      return buf.ReadZeroTerminatedStringFromBytes( encoding );
//   }


//   internal static String ReadZeroTerminatedASCIIString( this Stream stream, UInt32 length )
//   {
//      var buf = new Byte[length];
//      stream.ReadWholeArray( buf );
//      return buf.ReadZeroTerminatedASCIIStringFromBytes();
//   }

//   internal static String ReadAlignedASCIIString( this Stream stream, Int32 maxLength )
//   {
//      var bytesRead = 0;
//      var charBufSize = 0;
//      var charBuf = new Char[maxLength];
//      while ( bytesRead < maxLength )
//      {
//         var b = stream.ReadByteFromStream();
//         if ( b == 0 )
//         {
//            ++bytesRead;
//            if ( bytesRead % 4 == 0 )
//            {
//               break;
//            }
//         }
//         else
//         {
//            charBuf[bytesRead++] = (char) b;
//            ++charBufSize;
//         }
//      }
//      return new String( charBuf, 0, charBufSize );
//   }

//   internal static UInt32 ReadHeapIndex( this Stream stream, Boolean wideIndex, Byte[] tmpArray )
//   {
//      return wideIndex ? stream.ReadU32( tmpArray ) : stream.ReadU16( tmpArray );
//   }
//}