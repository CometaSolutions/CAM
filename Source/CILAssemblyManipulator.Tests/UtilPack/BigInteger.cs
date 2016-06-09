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
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilPack.Numerics;

namespace CILAssemblyManipulator.Tests.UtilPack
{
   [Category( "UtilPack" )]
   public class BigIntegerTests
   {
      [Test]
      public void TestBigIntegerStatics()
      {
         Assert.AreEqual( BigInteger.Zero.ToString(), "0" );
         Assert.AreEqual( BigInteger.One.ToString(), "1" );
         Assert.AreEqual( BigInteger.MinusOne.ToString(), "-1" );

         Assert.IsTrue( BigInteger.Zero.IsZero );
         Assert.IsFalse( BigInteger.Zero.IsOne );
         Assert.IsFalse( BigInteger.Zero.IsMinusOne );
         Assert.IsTrue( BigInteger.Zero.IsEven );

         Assert.IsTrue( BigInteger.One.IsOne );
         Assert.IsFalse( BigInteger.One.IsZero );
         Assert.IsFalse( BigInteger.One.IsMinusOne );
         Assert.IsFalse( BigInteger.One.IsEven );

         Assert.IsTrue( BigInteger.MinusOne.IsMinusOne );
         Assert.IsFalse( BigInteger.MinusOne.IsZero );
         Assert.IsFalse( BigInteger.MinusOne.IsOne );
         Assert.IsFalse( BigInteger.MinusOne.IsEven );
      }

      [Test]
      public void TestBigIntegerWithInt64()
      {
         PerformInt64Test( Int32.MaxValue );
         PerformInt64Test( UInt32.MaxValue );
         PerformInt64Test( Int32.MinValue );
         PerformInt64Test( Int64.MaxValue );
         PerformInt64Test( Int64.MinValue );
      }

      [Test]
      public void TestModulo()
      {
         var dividentSmall = Int64.MaxValue;
         var divident = new BigInteger( dividentSmall );
         Assert.Throws<ArithmeticException>( () => { var dummy = divident % 0; } );
         Assert.AreEqual( divident % 1, BigInteger.Zero );
         var r = new Random();
         var divisor = r.Next();
         Assert.AreEqual( divident % divisor, new BigInteger( dividentSmall % divisor ) );
         var divisor2 = DateTime.Now.Ticks;
         Assert.AreEqual( divident % divisor2, new BigInteger( dividentSmall % divisor2 ) );
      }

      [Test]
      public void TestDefault()
      {
         var bigint = default( BigInteger );
         Assert.IsTrue( bigint.IsZero );
         Assert.IsTrue( bigint.IsEven );
         Assert.IsFalse( bigint.IsPowerOfTwo );
         Assert.IsFalse( bigint.IsOne );
         Assert.IsFalse( bigint.IsMinusOne );
         Assert.AreEqual( bigint.Sign, 0 );
         Assert.AreEqual( bigint.BitLength, 0 );

         Assert.AreEqual( bigint / 5, bigint );
         Assert.AreEqual( bigint * 5, bigint );
         Assert.AreEqual( bigint % 5, bigint );
         Assert.AreEqual( bigint + 5, new BigInteger( 5 ) );
         Assert.AreEqual( bigint - 5, new BigInteger( -5 ) );
      }

      private static void PerformInt64Test( Int64 value )
      {
         var bigInt = new BigInteger( value );
         Assert.AreEqual( bigInt.ToString(), value.ToString() );
         bigInt = new BigInteger( -value );
         Assert.AreEqual( bigInt.ToString(), ( -value ).ToString() );
      }
   }
}
