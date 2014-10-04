/*
 * Copyright 2014 Stanislav Muhametsin. All rights Reserved.
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonUtils
{
   /// <summary>
   /// This class contains useful methods to convert things to and from strings.
   /// </summary>
   public static class StringConversions
   {
      private static readonly Char[] LOOKUP_UC = new Char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
      private static readonly Char[] LOOKUP_LC = new Char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

      /// <summary>
      /// This method creates a textual representation in hexadecimal format of byte array <paramref name="data"/>.
      /// </summary>
      /// <param name="data">Byte array to create textual representation from.</param>
      /// <param name="upperCaseHexes"><c>true</c> if alpha characters in should be in upper case; <c>false</c> otherwise.</param>
      /// <returns><c>null</c> if <paramref name="data"/> is <c>null</c>; otherwise textual representation of the byte array in hexadecimal format.</returns>
      /// <remarks>This is modified version represented on <see href="http://social.msdn.microsoft.com/Forums/en-US/csharpgeneral/thread/3928b8cb-3703-4672-8ccd-33718148d1e3/" /> , Matthew Fraser's and following PZahra's posts.</remarks>
      public static String ByteArray2HexStr( Byte[] data, Boolean upperCaseHexes = false )
      {
         return ByteArray2HexStr( data, 0, data.Length, upperCaseHexes );
      }

      /// <summary>
      /// This method creates a textual representation in hexadecimal format of byte array <paramref name="data"/>.
      /// </summary>
      /// <param name="data">Byte array to create textual representation from.</param>
      /// <param name="offset">Offset in the array to start reading bytes.</param>
      /// <param name="count">How many bytes read from array.</param>
      /// <param name="upperCaseHexes"><c>true</c> if alpha characters in should be in upper case; <c>false</c> otherwise.</param>
      /// <returns><c>null</c> if <paramref name="data"/> is <c>null</c>; otherwise textual representation of the byte array in hexadecimal format.</returns>
      /// <remarks>This is modified version represented on <see href="http://social.msdn.microsoft.com/Forums/en-US/csharpgeneral/thread/3928b8cb-3703-4672-8ccd-33718148d1e3/" /> , Matthew Fraser's and following PZahra's posts.</remarks>
      public static String ByteArray2HexStr( Byte[] data, Int32 offset, Int32 count, Boolean upperCaseHexes )
      {
         String result;
         if ( data == null )
         {
            result = null;
         }
         else
         {
            var lookup = upperCaseHexes ? LOOKUP_UC : LOOKUP_LC;
            Int32 p = 0;
            Char[] c = new Char[count * 2];
            Byte d;
            Int32 i = 0;
            while ( i < count )
            {
               d = data[offset++];
               c[p++] = lookup[d / 0x10];
               c[p++] = lookup[d % 0x10];
               ++i;
            }
            result = new String( c, 0, c.Length );
         }
         return result;
      }

      /// <summary>
      /// This method creates a byte array based on string which is assumed to be in hexadecimal format.
      /// </summary>
      /// <param name="str">The string containing byte array in hexadecimal format.</param>
      /// <param name="offset">The offset in string to start reading characters.</param>
      /// <param name="step">How many characters to skip after each successful single byte read.</param>
      /// <param name="tail">How many characters to leave unread at the end of the string.</param>
      /// <returns>A byte array containing logically same bytes as the given string in hexadecimal format.</returns>
      /// <remarks>This is modified version represented on <see href="http://social.msdn.microsoft.com/Forums/en-US/csharpgeneral/thread/3928b8cb-3703-4672-8ccd-33718148d1e3/" /> , Matthew Fraser's and following PZahra's posts.</remarks>
      public static Byte[] HexStr2ByteArray( String str, Int32 offset = 0, Int32 step = 0, Int32 tail = 0 )
      {
         Byte[] b;
         if ( str != null )
         {
            b = new Byte[( str.Length - offset - tail + step ) / ( 2 + step )];
            Byte c1, c2;
            Int32 l = str.Length - tail;
            Int32 s = step + 1;
            for ( Int32 y = 0, x = offset; x < l; ++y, x += s )
            {
               c1 = (Byte) str[x];
               if ( c1 > 0x60 )
               {
                  c1 -= 0x57;
               }
               else if ( c1 > 0x40 )
               {
                  c1 -= 0x37;
               }
               else
               {
                  c1 -= 0x30;
               }
               c2 = (Byte) str[++x];
               if ( c2 > 0x60 )
               {
                  c2 -= 0x57;
               }
               else if ( c2 > 0x40 )
               {
                  c2 -= 0x37;
               }
               else
               {
                  c2 -= 0x30;
               }
               b[y] = (Byte) ( ( c1 << 4 ) + c2 );
            }
         }
         else
         {
            b = null;
         }
         return b;
      }
   }
}
