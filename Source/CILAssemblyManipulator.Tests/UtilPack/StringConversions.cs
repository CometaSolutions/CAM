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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CommonUtils;


namespace CILAssemblyManipulator.Tests.UtilPack
{
   public class StringConversionTest
   {
      [Test]
      public void TestHexConversion()
      {
         const Int32 max = 50;
         var random = new Random();
         for ( var i = 0; i < max; ++i )
         {
            RunSingleHexConversionTest( random, random.Next( 1, 20 ) );
         }
      }

      [Test]
      public void TestBase64Encoding()
      {
         const Int32 max = 50;
         var random = new Random();
         for ( var i = 0; i < max; ++i )
         {
            RunSingleEncodingTest( random, random.Next( 1, 100 ), null );
         }
      }

      private static void RunSingleHexConversionTest( Random random, Int32 amountOfInts )
      {
         var bytez = new Byte[amountOfInts * 4];
         var idx = 0;
         while ( amountOfInts > 0 )
         {
            bytez.WriteInt32LEToBytes( ref idx, random.Next( Int32.MinValue, Int32.MaxValue ) );
            --amountOfInts;
         }

         var hexString = bytez.CreateHexString();
         Assert.IsTrue( ArrayEqualityComparer<Byte>.ArrayEquality( bytez, hexString.CreateHexBytes() ), "Original array was: " + String.Join( "", bytez.Select( b => b.ToString( "X2" ) ) ) + ", generated hex string was: " + hexString + "." );
      }

      private static void RunSingleEncodingTest( Random random, Int32 amountOfBytes, Char[] encodeTable )
      {
         if ( encodeTable == null )
         {
            encodeTable = StringConversions.CreateBase64EncodeLookupTable( true );
         }
         var decodeTable = StringConversions.CreateDecodeLookupTable( encodeTable );

         var intCount = BinaryUtils.AmountOfPagesTaken( amountOfBytes, 4 );
         var bytez = new Byte[intCount * 4];
         var idx = 0;
         while ( intCount > 0 )
         {
            bytez.WriteInt32LEToBytes( ref idx, random.Next( Int32.MinValue, Int32.MaxValue ) );
            --intCount;
         }
         idx = 0;
         bytez = bytez.CreateAndBlockCopyTo( ref idx, amountOfBytes );
         var encoded = bytez.EncodeBinary( 0, amountOfBytes, encodeTable );
         Assert.IsTrue( ArrayEqualityComparer<Byte>.ArrayEquality( bytez, encoded.DecodeBinary( BinaryUtils.Log2( (UInt32) encodeTable.Length ), decodeTable ) ), "Original array was: " + String.Join( "", bytez.Select( b => b.ToString( "X2" ) ) ) + ", generated string was: " + encoded + "." );
      }
   }
}
