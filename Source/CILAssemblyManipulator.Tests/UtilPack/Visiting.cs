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
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilPack;
using UtilPack.Visiting;

namespace CILAssemblyManipulator.Tests.UtilPack
{
   public class VisitingTest
   {
      private class A
      {
         public String StringValue { get; set; }
      }

      private class B
      {
         public Int32 Int32Value { get; set; }

         public A A { get; set; }
      }

      [Test]
      public void TestEqualityWithCachingTypeBasedAcceptor()
      {
         Int32 edgeID;
         var visitor = this.CreateVisitor( out edgeID );
         var acceptorSetup = AcceptorFactory.NewEqualityComparisonAcceptor( visitor, TopMostTypeVisitingStrategy.Never );
         acceptorSetup.RegisterEqualityAcceptor( ( A x, A y ) => String.Equals( x.StringValue, y.StringValue ) );
         acceptorSetup.RegisterEqualityAcceptor( ( B x, B y ) => x.Int32Value == y.Int32Value );
         acceptorSetup.RegisterEqualityComparisonTransition_Simple( edgeID, ( B el ) => el.A );



         var first = this.CreateB1();
         var second = this.CreateB1();

         var acceptor = acceptorSetup.Acceptor;

         var firstResult = acceptor.Accept( first, second );
         var secondResult = acceptor.Accept( first, second );

         Assert.IsTrue( firstResult );
         Assert.AreEqual( firstResult, secondResult );

         first = this.CreateB2();

         firstResult = acceptor.Accept( first, second );
         secondResult = acceptor.Accept( first, second );

         Assert.IsFalse( firstResult );
         Assert.AreEqual( firstResult, secondResult );
      }

      [Test]
      public void TestHashCodeWithAcceptor()
      {
         Int32 edgeID;
         var visitor = this.CreateVisitor( out edgeID );
         var acceptor = AcceptorFactory.NewHashCodeComputationAcceptor( visitor );
         acceptor.RegisterHashCodeComputer( ( A x ) => x.StringValue.GetHashCodeSafe() );
         acceptor.RegisterHashCodeComputer( ( B x, AcceptVertexExplicitCallbackWithResultDelegate<Object, Int32> cb ) => x.Int32Value * 2 + cb( x.A ) );

         var b = this.CreateB1();
         var hashFromAcceptor = acceptor.Acceptor.Accept( b );
         var manualHash = b.Int32Value * 2 + b.A.StringValue.GetHashCodeSafe();

         Assert.AreEqual( hashFromAcceptor, manualHash );

      }

      private TypeBasedVisitor<Object, Int32> CreateVisitor( out Int32 edgeID )
      {
         var visitor = new TypeBasedVisitor<Object, Int32>();
         using ( var a = visitor.CreateVertexInfoFactory( typeof( A ) ) )
         {
         }
         using ( var b = visitor.CreateVertexInfoFactory( typeof( B ) ) )
         {
            edgeID = b.CreatePropertyEdge<Object, Int32, B>( nameof( B.A ), thisEdgeID => ( el, cb ) => cb.VisitSimpleEdge( el.A, thisEdgeID ) ).ID;
         }
         return visitor;
      }

      private B CreateB1()
      {
         return new B()
         {
            Int32Value = 5,
            A = new A()
            {
               StringValue = "Test"
            }
         };
      }

      private B CreateB2()
      {
         return new B()
         {
            Int32Value = 5
         };
      }
   }


}
