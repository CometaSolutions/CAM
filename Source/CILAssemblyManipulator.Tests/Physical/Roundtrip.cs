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
using NUnit.Framework;
using System.IO;
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Tests.Physical
{
   public class RoundtripTest : AbstractCAMTest
   {
      [Test]
      public void TestRoundtripMSCorLib()
      {
         PerformRoundtripTest( MSCorLibLocation );
      }

      private static void PerformRoundtripTest( String fileLocation )
      {
         ModuleReadResult read1;
         using ( var fs = File.OpenRead( fileLocation ) )
         {
            read1 = fs.ReadModule();
         }

         Byte[] written;
         using ( var ms = new MemoryStream() )
         {
            read1.MetaData.WriteModule( ms, read1.Headers, null );
            written = ms.ToArray();
         }

         ModuleReadResult read2;
         using ( var ms = new MemoryStream( written ) )
         {
            read2 = ms.ReadModule();
         }
      }

   }
}
