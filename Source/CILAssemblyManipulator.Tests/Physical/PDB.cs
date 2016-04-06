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
extern alias CAMPhysicalP;

using CAMPhysicalP;
using CAMPhysicalP::CILAssemblyManipulator.Physical.PDB;
using CAMPhysicalP::CILAssemblyManipulator.Physical;

using CILAssemblyManipulator.Physical.PDB;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Tests.Physical
{
   [Category( "CAM.Physical" )]
   public class PDBTest : AbstractCAMTest
   {
      [Test]
      public void TestPDB()
      {
         PerformPDBTest( Path.Combine( Path.GetDirectoryName( CILMergeLocation ), "CILAssemblyManipulator.Physical.Core.pdb" ) );
      }

      private static void PerformPDBTest( String file )
      {
         // Test reading and equality to itself
         PDBInstance pdb;
         using ( var fs = File.OpenRead( file ) )
         {
            pdb = fs.ReadPDBInstance();
         }
         Assert.IsTrue( Comparers.PDBInstanceEqualityComparer.Equals( pdb, pdb ), "PDB instance must equal itself." );

         // Test equality to identical PDB instance
         PDBInstance pdb2;
         using ( var fs = File.OpenRead( file ) )
         {
            pdb2 = fs.ReadPDBInstance();
         }
         Assert.IsTrue( Comparers.PDBInstanceEqualityComparer.Equals( pdb, pdb2 ), "Different PDB instances with same content must be equal." );

         // Test writing
         Byte[] bytez;
         using ( var mem = new MemoryStream() )
         {
            pdb.WriteToStream( mem );
            bytez = mem.ToArray();
         }

         // Test that reading the written file results in same PDB
         using ( var mem = new MemoryStream( bytez ) )
         {
            pdb2 = mem.ReadPDBInstance();
         }
         Assert.IsTrue( Comparers.PDBInstanceEqualityComparer.Equals( pdb, pdb2 ), "PDB after writing and reading must still have same content." );
      }
   }
}
