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
using CILAssemblyManipulator.Logical;
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Structural;
using CILAssemblyManipulator.Tests.Logical;
using CommonUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CILAssemblyManipulator.Tests.Logical
{

   public class LogicalInteropTest : AbstractCAMTest
   {

      //[Test]
      public void TestPhysicalInteropCAMPhysicalAssembly()
      {
         PerformRoundtripTest( CAMPhysicalLocation );
      }

      private static void PerformRoundtripTest( String mdLocation )
      {
         CILMetaData md;
         using ( var fs = File.OpenRead( mdLocation ) )
         {
            md = fs.ReadModule();
         }
         PerformTest( ctx =>
         {
            var mdLoader = new CILMetaDataLoaderNotThreadSafeForFiles();
            var loader = new CILAssemblyLoaderNotThreadSafe( ctx, mdLoader );
            var logical = loader.LoadAssemblyFrom( mdLocation );
            var physicalLoaded = mdLoader.GetOrLoadMetaData( mdLocation );
            var physicalCreated = logical.MainModule.CreatePhysicalRepresentation();

            var structure1 = new AssemblyStructureInfo( physicalLoaded );
            var structure2 = new AssemblyStructureInfo( physicalLoaded );

            Assert.IsTrue( AssemblyEquivalenceComparer.EqualityComparer.Equals( structure1, structure2 ) );
            Console.WriteLine( "Hmz" );
         } );
      }

      private static void PerformTest( Action<CILReflectionContext> test )
      {
         using ( var ctx = DotNETReflectionContext.CreateDotNETContext() )
         {
            test( ctx );
         }
      }
   }
}

