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
   public class StructuralEquivalenceTest : AbstractCAMTest
   {
      [Test]
      public void TestPhysicalInteropWithCAMPhysical()
      {
         PerformEquivalenceTest( CAMPhysicalLocation );
      }

      [Test]
      public void TestPhysicalInteropWithCAMLogical()
      {
         PerformEquivalenceTest( CAMLogicalLocation );
      }

      [Test]
      public void TestPhysicalInteropWithMSCorLib()
      {
         PerformEquivalenceTest( MSCorLibLocation );
      }

      private static void PerformEquivalenceTest( String mdLocation )
      {
         CILMetaData md;
         using ( var fs = File.OpenRead( mdLocation ) )
         {
            md = fs.ReadModule();
         }
         var structure1 = md.CreateStructuralRepresentation();
         var structure2 = md.CreateStructuralRepresentation();

         Assert.IsTrue( AssemblyEquivalenceComparerExact.ExactEqualityComparer.Equals( structure1, structure2 ) );
      }
   }
}
