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
extern alias CAMPhysicalIOD;
extern alias CAMPhysicalIO;

using CAMPhysicalIOD;
using CAMPhysicalIOD::CILAssemblyManipulator.Physical;
using CAMPhysicalIOD::CILAssemblyManipulator.Physical.Meta;
using CAMPhysicalIOD::CILAssemblyManipulator.Physical.IO;

using CAMPhysicalIO::CILAssemblyManipulator.Physical.IO;

using CILAssemblyManipulator.Physical;
using CommonUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Tests.Physical
{
   [Category( "CAM.Physical" )]
   public class MiscellaneousTest : AbstractCAMTest
   {
      [Test]
      public void TestTypeDefNamesNoTypes()
      {
         var md = CILMetaDataFactory.NewBlankMetaData();
         Assert.IsEmpty( md.GetTypeDefinitionsFullNames() );
      }

      [Test]
      public void TestTypeDefNamesSingleType()
      {
         const String NAME = "TestType";
         const String NAMESPACE = "TestNamespace";

         var md = CILMetaDataFactory.NewBlankMetaData();
         var tDefs = md.TypeDefinitions.TableContents;

         tDefs.Add( new TypeDefinition()
         {
            Name = NAME,
            Namespace = NAMESPACE
         } );

         var typeStr = md.GetTypeDefinitionsFullNames().Single();
         Assert.AreEqual( typeStr, Miscellaneous.CombineNamespaceAndType( NAMESPACE, NAME ) );
      }

      [Test]
      public void TestTypeDefNamesWithNestedTypes()
      {
         const String NAME_ENCLOSING = "EnclosingType";
         const String NAMESPACE_ENCLOSING = "TestNamespace";

         const String NAME_NESTED = "NestedType";
         const String NAMESPACE_NESTED = "TestNamespace1";


         var md = CILMetaDataFactory.NewBlankMetaData();
         var tDefs = md.TypeDefinitions.TableContents;

         tDefs.Add( new TypeDefinition()
         {
            Name = NAME_ENCLOSING,
            Namespace = NAMESPACE_ENCLOSING
         } );

         tDefs.Add( new TypeDefinition()
         {
            Name = NAME_NESTED,
            Namespace = NAMESPACE_NESTED
         } );

         md.NestedClassDefinitions.TableContents.Add( new NestedClassDefinition()
         {
            EnclosingClass = new TableIndex( Tables.TypeDef, 0 ),
            NestedClass = new TableIndex( Tables.TypeDef, 1 )
         } );

         var generatedArray = md.GetTypeDefinitionsFullNames().ToArray();
         var enclosingTypeStr = Miscellaneous.CombineNamespaceAndType( NAMESPACE_ENCLOSING, NAME_ENCLOSING );
         Assert.IsTrue( ArrayEqualityComparer<String>.ArrayEquality(
            generatedArray,
            new[] { enclosingTypeStr, Miscellaneous.CombineEnclosingAndNestedType( enclosingTypeStr, NAME_NESTED ) }
            ) );
      }

      [Test]
      public void TestTypeDefNamesWithNestedTypes_WrongOrder()
      {
         const String NAME_ENCLOSING = "EnclosingType";
         const String NAMESPACE_ENCLOSING = "TestNamespace";

         const String NAME_NESTED = "NestedType";
         const String NAMESPACE_NESTED = "TestNamespace";


         var md = CILMetaDataFactory.NewBlankMetaData();
         var tDefs = md.TypeDefinitions.TableContents;

         tDefs.Add( new TypeDefinition()
         {
            Name = NAME_NESTED,
            Namespace = NAMESPACE_NESTED
         } );

         tDefs.Add( new TypeDefinition()
         {
            Name = NAME_ENCLOSING,
            Namespace = NAMESPACE_ENCLOSING
         } );

         md.NestedClassDefinitions.TableContents.Add( new NestedClassDefinition()
         {
            EnclosingClass = new TableIndex( Tables.TypeDef, 1 ),
            NestedClass = new TableIndex( Tables.TypeDef, 0 )
         } );


         var generatedArray = md.GetTypeDefinitionsFullNames().ToArray();
         var enclosingTypeStr = Miscellaneous.CombineNamespaceAndType( NAMESPACE_ENCLOSING, NAME_ENCLOSING );
         Assert.IsTrue( ArrayEqualityComparer<String>.ArrayEquality(
            generatedArray,
            new[] { Miscellaneous.CombineEnclosingAndNestedType( enclosingTypeStr, NAME_NESTED ), enclosingTypeStr, }
            ) );
      }

      [Test]
      public void TestTypeDefNamesWithNestedTypes_LongNestedHierarchy()
      {
         const String NAME_ENCLOSING = "EnclosingType";
         const String NAMESPACE_ENCLOSING = "TestNamespace";

         const String NAME_NESTED1 = "NestedType1";
         const String NAME_NESTED2 = "NestedType2";
         const String NAME_NESTED3 = "NestedType3";
         const String NAME_NESTED4 = "NestedType4";


         var md = CILMetaDataFactory.NewBlankMetaData();
         var tDefs = md.TypeDefinitions.TableContents;

         tDefs.Add( new TypeDefinition()
         {
            Name = NAME_NESTED1,
            Namespace = null
         } );

         tDefs.Add( new TypeDefinition()
         {
            Name = NAME_ENCLOSING,
            Namespace = NAMESPACE_ENCLOSING
         } );

         tDefs.Add( new TypeDefinition()
         {
            Name = NAME_NESTED2,
            Namespace = null
         } );
         tDefs.Add( new TypeDefinition()
         {
            Name = NAME_NESTED3,
            Namespace = null
         } );
         tDefs.Add( new TypeDefinition()
         {
            Name = NAME_NESTED4,
            Namespace = null
         } );

         md.NestedClassDefinitions.TableContents.AddRange( new[]
         {
            new NestedClassDefinition()
            {
               EnclosingClass = new TableIndex( Tables.TypeDef, 1 ),
               NestedClass = new TableIndex( Tables.TypeDef, 0 )
            },
            new NestedClassDefinition()
            {
               EnclosingClass = new TableIndex(Tables.TypeDef, 0),
               NestedClass = new TableIndex(Tables.TypeDef, 2)
            },
            new NestedClassDefinition()
            {
               EnclosingClass = new TableIndex(Tables.TypeDef, 2),
               NestedClass = new TableIndex(Tables.TypeDef, 3)
            },
            new NestedClassDefinition()
            {
               EnclosingClass = new TableIndex(Tables.TypeDef, 3),
               NestedClass = new TableIndex(Tables.TypeDef, 4)
            },
         } );


         var generatedArray = md.GetTypeDefinitionsFullNames().ToArray();
         var enclosingTypeStr = Miscellaneous.CombineNamespaceAndType( NAMESPACE_ENCLOSING, NAME_ENCLOSING );
         var nested1Str = Miscellaneous.CombineEnclosingAndNestedType( enclosingTypeStr, NAME_NESTED1 );
         var nested2Str = Miscellaneous.CombineEnclosingAndNestedType( nested1Str, NAME_NESTED2 );
         var nested3Str = Miscellaneous.CombineEnclosingAndNestedType( nested2Str, NAME_NESTED3 );
         var nested4Str = Miscellaneous.CombineEnclosingAndNestedType( nested3Str, NAME_NESTED4 );
         Assert.IsTrue( ArrayEqualityComparer<String>.ArrayEquality(
            generatedArray,
            new[] { nested1Str, enclosingTypeStr, nested2Str, nested3Str, nested4Str }
            ) );
      }

      [Test]
      public void TestTypeDefNamesWithNestedTypes_LongNestedHierarchy2()
      {
         const String NAME_ENCLOSING = "EnclosingType";
         const String NAMESPACE_ENCLOSING = "TestNamespace";

         const String NAME_NESTED1 = "NestedType1";
         const String NAME_NESTED2 = "NestedType2";
         const String NAME_NESTED3 = "NestedType3";
         const String NAME_NESTED4 = "NestedType4";


         var md = CILMetaDataFactory.NewBlankMetaData();
         var tDefs = md.TypeDefinitions.TableContents;

         tDefs.Add( new TypeDefinition()
         {
            Name = NAME_NESTED4,
            Namespace = null
         } );

         tDefs.Add( new TypeDefinition()
         {
            Name = NAME_NESTED1,
            Namespace = null
         } );

         tDefs.Add( new TypeDefinition()
         {
            Name = NAME_NESTED2,
            Namespace = null
         } );

         tDefs.Add( new TypeDefinition()
         {
            Name = NAME_ENCLOSING,
            Namespace = NAMESPACE_ENCLOSING
         } );


         tDefs.Add( new TypeDefinition()
         {
            Name = NAME_NESTED3,
            Namespace = null
         } );


         md.NestedClassDefinitions.TableContents.AddRange( new[]
         {
            new NestedClassDefinition()
            {
               EnclosingClass = new TableIndex( Tables.TypeDef, 3 ),
               NestedClass = new TableIndex( Tables.TypeDef, 1 )
            },
            new NestedClassDefinition()
            {
               EnclosingClass = new TableIndex(Tables.TypeDef, 1),
               NestedClass = new TableIndex(Tables.TypeDef, 2)
            },
            new NestedClassDefinition()
            {
               EnclosingClass = new TableIndex(Tables.TypeDef, 2),
               NestedClass = new TableIndex(Tables.TypeDef, 4)
            },
            new NestedClassDefinition()
            {
               EnclosingClass = new TableIndex(Tables.TypeDef, 4),
               NestedClass = new TableIndex(Tables.TypeDef, 0)
            },
         } );


         var generatedArray = md.GetTypeDefinitionsFullNames().ToArray();
         var enclosingTypeStr = Miscellaneous.CombineNamespaceAndType( NAMESPACE_ENCLOSING, NAME_ENCLOSING );
         var nested1Str = Miscellaneous.CombineEnclosingAndNestedType( enclosingTypeStr, NAME_NESTED1 );
         var nested2Str = Miscellaneous.CombineEnclosingAndNestedType( nested1Str, NAME_NESTED2 );
         var nested3Str = Miscellaneous.CombineEnclosingAndNestedType( nested2Str, NAME_NESTED3 );
         var nested4Str = Miscellaneous.CombineEnclosingAndNestedType( nested3Str, NAME_NESTED4 );
         Assert.IsTrue( ArrayEqualityComparer<String>.ArrayEquality(
            generatedArray,
            new[] { nested4Str, nested1Str, nested2Str, enclosingTypeStr, nested3Str }
            ) );
      }

      [Test]
      public void TestTypeDefNamesWithNestedTypes_WithLoop()
      {
         // Purpose of this test is mainly to verify that calling GetTypeDefinitionsFullNames() won't get stuck into infinite loop or cause stack overflow
         const String NAME_ENCLOSING = "EnclosingType";
         const String NAMESPACE_ENCLOSING = "TestNamespace";

         const String NAME_NESTED = "NestedType";
         const String NAMESPACE_NESTED = "NestedNamespace";


         var md = CILMetaDataFactory.NewBlankMetaData();
         var tDefs = md.TypeDefinitions.TableContents;


         tDefs.Add( new TypeDefinition()
         {
            Name = NAME_ENCLOSING,
            Namespace = NAMESPACE_ENCLOSING
         } );

         tDefs.Add( new TypeDefinition()
         {
            Name = NAME_NESTED,
            Namespace = NAMESPACE_NESTED
         } );


         md.NestedClassDefinitions.TableContents.AddRange( new[]
         {
            new NestedClassDefinition()
            {
               EnclosingClass = new TableIndex( Tables.TypeDef, 0 ),
               NestedClass = new TableIndex( Tables.TypeDef, 1 )
            },
            new NestedClassDefinition()
            {
               EnclosingClass = new TableIndex( Tables.TypeDef, 1 ),
               NestedClass = new TableIndex( Tables.TypeDef, 0 )
            }
         } );


         var generatedArray = md.GetTypeDefinitionsFullNames().ToArray();
         var nestedStr = Miscellaneous.CombineNamespaceAndType( NAMESPACE_NESTED, NAME_NESTED );
         Assert.IsTrue( ArrayEqualityComparer<String>.ArrayEquality(
            generatedArray,
            new[]
            {
               Miscellaneous.CombineEnclosingAndNestedType(nestedStr, NAME_ENCLOSING),
               nestedStr
            } ) );
      }

      [Test]
      public void TestTypeDefNames_MSCorLib()
      {
         PerformNameTestFor( MSCorLib, "System.Runtime.Remoting.Proxies.__TransparentProxy" );
      }

      [Test]
      public void TestTypeDefNames_CAMPhysical()
      {
         PerformNameTestFor( CAMPhysical );
      }


      [Test]
      public void TestTypeDefNames_CAMLogical()
      {
         PerformNameTestFor( CAMLogical );
      }

      [Test]
      public void TestTypeDefNames_CAMStructural()
      {
         PerformNameTestFor( CAMStructural );
      }

      [Test]
      public void TestCapacities()
      {
         var md = CILMetaDataFactory.NewBlankMetaData( sizes: new[] { 1, 2 } );
         Assert.AreEqual( md.ModuleDefinitions.TableContents.Capacity, 1 );
         Assert.AreEqual( md.TypeReferences.TableContents.Capacity, 2 );
      }

      private static void PerformNameTestFor( System.Reflection.Assembly assembly, params String[] namesToRemoveFromCILTypes )
      {
         var md = ReadFromAssembly( assembly, null );
         // Remove <Module> type since native reflection API does not return it.
         var cilTypes = new HashSet<String>( md.GetTypeDefinitionsFullNames() );
         cilTypes.Remove( Miscellaneous.MODULE_TYPE_NAME );

         var srTypes = new HashSet<String>( assembly.GetTypes().Select( t => t.FullName ) );

         // Remove any additional types as well.
         cilTypes.ExceptWith( namesToRemoveFromCILTypes );

         Assert.IsTrue( cilTypes.SetEquals( srTypes ) );

      }
   }
}
