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
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.IO;
using CILAssemblyManipulator.Structural;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Tests.Structural
{
   [Category( "CAM.Structural" )]
   public class StructuralInteropTest : AbstractCAMTest
   {
      [Test]
      public void TestPhysicalInteropWithCAMPhysical()
      {
         PerformInteropTest( CAMPhysicalLocation );
      }

      [Test]
      public void TestPhysicalInteropWithCAMLogical()
      {
         PerformInteropTest( CAMLogicalLocation );
      }

      [Test]
      public void TestPhysicalInteropWithMSCorLib()
      {
         PerformInteropTest( MSCorLibLocation );
      }

      private static void PerformInteropTest( String mdLocation )
      {
         CILMetaData md;
         using ( var fs = File.OpenRead( mdLocation ) )
         {
            md = fs.ReadModule();
         }
         var structure1 = md.CreateStructuralRepresentation();
         Assert.IsTrue( AssemblyEquivalenceComparerExact.ExactEqualityComparer.Equals( structure1, structure1 ), "Assembly structure must equal itself." );

         var md2 = structure1.CreatePhysicalRepresentation()[0];
         var structure2 = md2.CreateStructuralRepresentation();

         //var md1MemberRefs = CreateMemberRefStrings( md );
         //var md2MemberRefs = CreateMemberRefStrings( md2 );
         //var set = new HashSet<String>( md1MemberRefs );
         //set.ExceptWith( md2MemberRefs );


         Assert.IsTrue( AssemblyEquivalenceComparerExact.ExactEqualityComparer.Equals( structure1, structure2 ), "Another assembly structure made from physical module made from original assembly structure must equal original assembly structure." );
      }

      //private static String[] CreateMemberRefStrings( CILMetaData md )
      //{
      //   var tRefStrings = CreateTypeRefStrings( md );
      //   return md.MemberReferences.TableContents
      //      .Select( mRef =>
      //      {
      //         String declTypeStr = null;
      //         var declType = mRef.DeclaringType;
      //         switch ( declType.Table )
      //         {
      //            case Tables.TypeRef:
      //               declTypeStr = tRefStrings[declType.Index];
      //               break;
      //            case Tables.TypeDef:
      //               declTypeStr = Miscellaneous.CombineNamespaceAndType( md.TypeDefinitions.TableContents[declType.Index].Namespace, md.TypeDefinitions.TableContents[declType.Index].Name );
      //               break;
      //         }
      //         return declTypeStr + ": " + mRef.Name;
      //      } )
      //      .ToArray();
      //}

      //private static String[] CreateTypeRefStrings( CILMetaData md )
      //{
      //   return md.TypeReferences.TableContents
      //      .Select( tRef => CreateTypeRefString( md, tRef ) )
      //      .ToArray();
      //}

      //private static String CreateTypeRefString( CILMetaData md, TypeReference tRef )
      //{
      //   var resScope = tRef.ResolutionScope.Value;
      //   String resScopeStr = null;
      //   switch ( resScope.Table )
      //   {
      //      case Tables.ModuleRef:
      //         resScopeStr = "{" + md.ModuleReferences.TableContents[resScope.Index].ModuleName + "}";
      //         break;
      //      case Tables.Module:
      //         resScopeStr = "$This$";
      //         break;
      //      case Tables.AssemblyRef:
      //         resScopeStr = "[" + md.AssemblyReferences.TableContents[resScope.Index] + "]";
      //         break;
      //      case Tables.NestedClass:
      //         resScopeStr = CreateTypeRefString( md, md.TypeReferences.TableContents[resScope.Index] ) + "+";
      //         break;
      //   }

      //   return resScopeStr + Miscellaneous.CombineNamespaceAndType( tRef.Namespace, tRef.Name );
      //}
   }
}
