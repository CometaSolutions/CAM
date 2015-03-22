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
      public void TestNestedClassOrdering()
      {
         const String NS = "TestNamespace";
         const String NESTED_CLASS_NAME = "NestedType";
         const String ENCLOSING_CLASS_NAME = "EnclosingType";
         var md = CILMetaDataFactory.NewMetaData();

         // Create some types
         md.TypeDefinitions.Add( new TypeDefinition() { Namespace = NS, Name = NESTED_CLASS_NAME } );
         md.TypeDefinitions.Add( new TypeDefinition() { Namespace = NS, Name = ENCLOSING_CLASS_NAME } );

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
         Assert.AreEqual( NESTED_CLASS_NAME, md.TypeDefinitions[md.NestedClassDefinitions[0].NestedClass.Index].Name );
         Assert.AreEqual( ENCLOSING_CLASS_NAME, md.TypeDefinitions[md.NestedClassDefinitions[0].EnclosingClass.Index].Name );
      }

      [Test]
      public void TestMSCorLibOrdering()
      {
         var md = ReadFromFile( MSCorLibLocation );
         md.MetaData.OrderTablesAndUpdateSignatures();
         ValidateOrderAndIntegrity( md.MetaData );
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
