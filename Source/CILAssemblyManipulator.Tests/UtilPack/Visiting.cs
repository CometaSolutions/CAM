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
         var visitor = new TypeBasedVisitor<Object, Int32>();
         using ( var a = visitor.CreateVertexInfoFactory( typeof( A ) ) )
         {
         }
         Int32 edgeID;
         using ( var b = visitor.CreateVertexInfoFactory( typeof( B ) ) )
         {
            edgeID = b.CreatePropertyEdge<Object, Int32, B>( nameof( B.A ), thisEdgeID => ( el, cb ) => cb.VisitSimpleEdge( el.A, thisEdgeID ) ).ID;
         }

         var acceptor = new EqualityComparisonAcceptor<Object>(
            visitor,
            TopMostTypeVisitingStrategy.Never
            );
         acceptor.RegisterEqualityAcceptor( ( A x, A y ) => String.Equals( x.StringValue, y.StringValue ) );
         acceptor.RegisterEqualityAcceptor( ( B x, B y ) => x.Int32Value == y.Int32Value );
         acceptor.RegisterEqualityComparisonTransition_Simple( edgeID, ( B el ) => el.A );

         var first = new B()
         {
            Int32Value = 5,
            A = new A()
            {
               StringValue = "Test"
            }
         };
         var second = new B()
         {
            Int32Value = 5,
            A = new A()
            {
               StringValue = "Test"
            }
         };

         var firstResult = acceptor.Accept( first, second );
         var secondResult = acceptor.Accept( first, second );

         Assert.IsTrue( firstResult );
         Assert.AreEqual( firstResult, secondResult );
      }


      private static Boolean TestEquality<T>( Object element, ObjectGraphEqualityContext<Object> context, Equality<T> equality )
         where T : class
      {
         var fromCtx = context.GetCurrentElement();
         T fromCtxTyped;
         return ReferenceEquals( element, fromCtx )
         || ( element != null && ( fromCtxTyped = fromCtx as T ) != null
         && equality( (T) element, fromCtxTyped ) );

      }
   }


}
