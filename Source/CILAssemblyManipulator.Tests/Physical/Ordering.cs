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
using CILAssemblyManipulator.Physical;
using NUnit.Framework;

namespace CILAssemblyManipulator.Tests.Physical
{
   public class OrderingTest : AbstractCAMTest
   {

      [Test]
      public void TestOrdering()
      {
         const String NS = "TestNamespace";
         const String TYPE_1 = "TestType1";
         const String TYPE_2 = "TestType2";
         var md = CILMetaDataFactory.NewMetaData();

         // Create some types
         md.TypeDefinitions.Add( new TypeDefinition() { Namespace = NS, Name = TYPE_1 } );
         md.TypeDefinitions.Add( new TypeDefinition() { Namespace = NS, Name = TYPE_2 } );

         // Add wrong nested-class definition (enclosing type is greater than nested type)
         md.NestedClassDefinitions.Add( new NestedClassDefinition()
         {
            NestedClass = new TableIndex( Tables.TypeDef, 0 ),
            EnclosingClass = new TableIndex( Tables.TypeDef, 1 )
         } );

         md.OrderTablesAndUpdateSignatures();

         ValidateOrderAndIntegrity( md );

         Assert.AreEqual( 1, md.NestedClassDefinitions.Count );
         Assert.AreEqual( 2, md.TypeDefinitions.Count );
         Assert.AreEqual( TYPE_1, md.TypeDefinitions[md.NestedClassDefinitions[0].NestedClass.Index].Name );
         Assert.AreEqual( TYPE_2, md.TypeDefinitions[md.NestedClassDefinitions[0].EnclosingClass.Index].Name );
      }


      private static void ValidateOrderAndIntegrity( CILMetaData md )
      {
         // 1. TypeDef - enclosing class definition must precede nested class definition
         foreach ( var nc in md.NestedClassDefinitions )
         {
            Assert.Less( nc.EnclosingClass.Index, nc.NestedClass.Index );
         }

         // NestedClass - sorted by NestedClass column
         AssertOrderBySingleSimpleColumn( md.NestedClassDefinitions, nc =>
         {
            Assert.AreEqual( nc.NestedClass.Table, Tables.TypeDef );
            Assert.AreEqual( nc.EnclosingClass.Table, Tables.TypeDef );
            return nc.NestedClass.Index;
         } );
      }

      private static void AssertOrderBySingleSimpleColumn<T>( List<T> table, Func<T, Int32> pkExtractor )
      {
         for ( var i = 1; i < table.Count; ++i )
         {
            Assert.Less( pkExtractor( table[i - 1] ), pkExtractor( table[i] ) );
         }
      }
   }
}
