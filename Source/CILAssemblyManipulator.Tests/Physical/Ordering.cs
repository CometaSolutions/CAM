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
extern alias CAMPhysical;
extern alias CAMPhysicalR;
using CAMPhysical;
using CAMPhysicalR;
using CAMPhysical::CILAssemblyManipulator.Physical.Meta;
using CAMPhysical::CILAssemblyManipulator.Physical.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CILAssemblyManipulator.Physical;
using NUnit.Framework;
using CommonUtils;
using CILAssemblyManipulator.Tests.Physical;

namespace CILAssemblyManipulator.Tests.Physical
{
   [Category( "CAM.Physical" )]
   public class OrderingTest : AbstractCAMTest
   {

      [Test]
      public void TestNestedClassOrdering()
      {
         const String NS = "TestNamespace";
         const String NESTED_CLASS_NAME = "NestedType";
         const String ENCLOSING_CLASS_NAME = "EnclosingType";
         var md = CAMPhysical::CILAssemblyManipulator.Physical.CILMetaDataFactory.CreateMinimalAssembly( null, null, false );

         // Create some types
         md.TypeDefinitions.TableContents.Add( new TypeDefinition() { Namespace = NS, Name = NESTED_CLASS_NAME } );
         md.TypeDefinitions.TableContents.Add( new TypeDefinition() { Namespace = NS, Name = ENCLOSING_CLASS_NAME } );

         // Add wrong nested-class definition (enclosing type is greater than nested type)
         md.NestedClassDefinitions.TableContents.Add( new NestedClassDefinition()
         {
            NestedClass = new TableIndex( Tables.TypeDef, 0 ),
            EnclosingClass = new TableIndex( Tables.TypeDef, 1 )
         } );

         ReOrderAndValidate( md );

         Assert.AreEqual( 1, md.NestedClassDefinitions.TableContents.Count );
         Assert.AreEqual( 2, md.TypeDefinitions.TableContents.Count );
         Assert.AreEqual( NESTED_CLASS_NAME, md.TypeDefinitions.TableContents[md.NestedClassDefinitions.TableContents[0].NestedClass.Index].Name );
         Assert.AreEqual( ENCLOSING_CLASS_NAME, md.TypeDefinitions.TableContents[md.NestedClassDefinitions.TableContents[0].EnclosingClass.Index].Name );
      }

      [Test]
      public void TestMSCorLibOrdering()
      {
         var md = CILMetaDataIO.ReadModuleFrom( MSCorLibLocation );
         ReOrderAndValidate( md );
      }

      [Test]
      public void TestCAMOrdering()
      {
         var md = CILMetaDataIO.ReadModuleFrom( CAMPhysicalLocation );
         ReOrderAndValidate( md );
      }

      [Test]
      public void TestDuplicateRemovingWithOneDuplicate()
      {
         var md = CAMPhysical::CILAssemblyManipulator.Physical.CILMetaDataFactory.CreateMinimalAssembly( null, null, false );
         md.TypeDefinitions.TableContents.Add( new TypeDefinition() { Namespace = "TestNS", Name = "TestType" } );
         var method = new MethodDefinition() { Name = "TestMethod", IL = new MethodILDefinition(), Signature = new MethodDefinitionSignature( 1 ) };
         md.MethodDefinitions.TableContents.Add( method );
         var typeSpec = new ClassOrValueTypeSignature() { Type = new TableIndex( Tables.TypeDef, 0 ), TypeReferenceKind = TypeReferenceKind.ValueType };

         AddDuplicateRowToMD( md, method, typeSpec );

         ReOrderAndValidate( md );

         Assert.AreEqual( 1, md.TypeSpecifications.TableContents.Count );
         Assert.AreEqual( new TableIndex( Tables.TypeSpec, 0 ), ( (ClassOrValueTypeSignature) md.MethodDefinitions.TableContents[0].Signature.Parameters[0].Type ).Type );
         Assert.AreEqual( new TableIndex( Tables.TypeSpec, 0 ), ( (OpCodeInfoWithTableIndex) md.MethodDefinitions.TableContents[0].IL.OpCodes[0] ).Operand );
      }

      [Test]
      public void TestDuplicateRemovingWithTwoDuplicates()
      {
         var md = CAMPhysical::CILAssemblyManipulator.Physical.CILMetaDataFactory.CreateMinimalAssembly( null, null, false );
         md.TypeDefinitions.TableContents.Add( new TypeDefinition() { Namespace = "TestNS", Name = "TestType" } );
         var method = new MethodDefinition() { Name = "TestMethod", IL = new MethodILDefinition(), Signature = new MethodDefinitionSignature( 1 ) };
         md.MethodDefinitions.TableContents.Add( method );
         var typeSpec = new ClassOrValueTypeSignature() { Type = new TableIndex( Tables.TypeDef, 0 ), TypeReferenceKind = TypeReferenceKind.ValueType };

         var method2 = new MethodDefinition() { Name = "TestMethod2", IL = new MethodILDefinition(), Signature = new MethodDefinitionSignature( 1 ) };
         md.MethodDefinitions.TableContents.Add( method2 );
         var typeSpec2 = new ClassOrValueTypeSignature( 1 ) { Type = new TableIndex( Tables.TypeDef, 0 ), TypeReferenceKind = TypeReferenceKind.Class };

         AddDuplicateRowToMD( md, method, typeSpec );
         AddDuplicateRowToMD( md, method2, typeSpec2 );


         ReOrderAndValidate( md );

         Assert.AreEqual( 2, md.TypeSpecifications.TableContents.Count );
         Assert.AreEqual( new TableIndex( Tables.TypeSpec, 0 ), ( (ClassOrValueTypeSignature) md.MethodDefinitions.TableContents[0].Signature.Parameters[0].Type ).Type );
         Assert.AreEqual( new TableIndex( Tables.TypeSpec, 0 ), ( (OpCodeInfoWithTableIndex) md.MethodDefinitions.TableContents[0].IL.OpCodes[0] ).Operand );
         Assert.AreEqual( new TableIndex( Tables.TypeSpec, 1 ), ( (ClassOrValueTypeSignature) md.MethodDefinitions.TableContents[1].Signature.Parameters[0].Type ).Type );
         Assert.AreEqual( new TableIndex( Tables.TypeSpec, 1 ), ( (OpCodeInfoWithTableIndex) md.MethodDefinitions.TableContents[1].IL.OpCodes[0] ).Operand );
      }

      //[Test]
      //public void TestDuplicateNestedTypeRefs()
      //{
      //   var md = CILMetaDataFactory.CreateMinimalAssembly( null, null, false );

      //   md.AssemblyReferences.TableContents.Add( new AssemblyReference() );

      //   var tRefs = md.TypeReferences.TableContents;
      //   var firstTRef = new TypeReference()
      //   {
      //      Name = "FirstType",
      //      Namespace = "NS",
      //      ResolutionScope = new TableIndex( Tables.AssemblyRef, 0 )
      //   };

      //   var midTRef = new TypeReference()
      //   {
      //      Name = "MidType",
      //      Namespace = "NS",
      //      ResolutionScope = new TableIndex( Tables.AssemblyRef, 0 )
      //   };

      //   tRefs.AddRange( new[]
      //   {
      //      firstTRef,
      //      midTRef,
      //      midTRef,
      //      new TypeReference()
      //      {
      //         Name = "EnclosingType",
      //         Namespace ="NS",
      //         ResolutionScope = new TableIndex(Tables.AssemblyRef, 0)
      //      },
      //      new TypeReference()
      //      {
      //         Name = "EnclosingType",
      //         Namespace ="NS",
      //         ResolutionScope = new TableIndex(Tables.AssemblyRef, 0)
      //      },
      //      new TypeReference()
      //      {
      //         Name = "NestedType",
      //         Namespace = null,
      //         ResolutionScope = new TableIndex(Tables.TypeRef, 4)
      //      },
      //      new TypeReference()
      //      {
      //         Name = "NestedType",
      //         Namespace = null,
      //         ResolutionScope = new TableIndex(Tables.TypeRef, 3)
      //      },
      //   } );

      //   ReOrderAndValidate( md );

      //   Assert.AreEqual( 4, tRefs.Count );
      //   Assert.IsTrue( ReferenceEquals( tRefs[0], firstTRef ) );
      //   Assert.IsTrue( Comparers.TypeReferenceEqualityComparer.Equals( tRefs[1], new TypeReference()
      //   {
      //      Name = "EnclosingType",
      //      Namespace = "NS",
      //      ResolutionScope = new TableIndex( Tables.AssemblyRef, 0 )
      //   } ) );
      //   Assert.IsTrue( Comparers.TypeReferenceEqualityComparer.Equals( tRefs[1], new TypeReference()
      //   {
      //      Name = "NestedType",
      //      Namespace = null,
      //      ResolutionScope = new TableIndex( Tables.TypeRef, 1 )
      //   } ) );
      //   Assert.IsTrue( ReferenceEquals( tRefs[3], midTRef ) );
      //}

      private static void AddDuplicateRowToMD( CILMetaData md, MethodDefinition method, TypeSignature typeSpec )
      {
         var typeSpecIndex = md.TypeSpecifications.GetRowCount();
         var type = new ClassOrValueTypeSignature() { Type = new TableIndex( Tables.TypeSpec, typeSpecIndex ) };
         method.Signature.Parameters.Add( new ParameterSignature() { Type = type } );
         method.Signature.ReturnType = new ParameterSignature() { Type = md.SignatureProvider.GetSimpleTypeSignature( SimpleTypeSignatureKind.Void ) };
         method.IL.OpCodes.Add( new OpCodeInfoWithTableIndex( OpCodeID.Ldtoken, new TableIndex( Tables.TypeSpec, typeSpecIndex + 1 ) ) );

         var typeSpecRow = new TypeSpecification() { Signature = typeSpec };
         md.TypeSpecifications.TableContents.Add( typeSpecRow );
         md.TypeSpecifications.TableContents.Add( typeSpecRow );
      }

      private static void ReOrderAndValidate( CILMetaData md )
      {
         md.ResolveEverything();

         var structure1 = md.CreateStructuralRepresentation();

         // Perform Sort
         var tableIndexTranslationInfo = md.OrderTablesAndRemoveDuplicates();
         /////////////////////// Order

         // 1. TypeDef - enclosing class definition must precede nested class definition
         foreach ( var nc in md.NestedClassDefinitions.TableContents )
         {
            Assert.Less( nc.EnclosingClass.Index, nc.NestedClass.Index );
         }

         // NestedClass - sorted by NestedClass column
         AssertOrderBySingleSimpleColumn( md.NestedClassDefinitions.TableContents, nc =>
         {
            Assert.AreEqual( nc.NestedClass.Table, Tables.TypeDef );
            Assert.AreEqual( nc.EnclosingClass.Table, Tables.TypeDef );
            return nc.NestedClass.Index;
         } );

         // TODO all other tables as well...

         //////////////////////// Integrity
         var structure2 = md.CreateStructuralRepresentation();
         Assert.IsTrue( CILAssemblyManipulator.Structural.AssemblyEquivalenceComparerExact.ExactEqualityComparer.Equals( structure1, structure2 ) );
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