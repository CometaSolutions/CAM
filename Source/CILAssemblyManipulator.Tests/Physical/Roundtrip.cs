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
         PerformRoundtripTest( MSCorLibLocation, ValidateAllIsResolved, ValidateAllIsResolved );
      }

      private static void PerformRoundtripTest( String fileLocation, Action<CILMetaData> afterFirstRead, Action<CILMetaData> afterSecondRead )
      {
         var resolver = new MetaDataResolver();

         var rArgs1 = new ReadingArguments();
         CILMetaData read1;
         using ( var fs = File.OpenRead( fileLocation ) )
         {
            read1 = fs.ReadModule( rArgs1 );
         }

         resolver.ResolveEverything( read1 );
         if ( afterFirstRead != null )
         {
            afterFirstRead( read1 );
         }

         Byte[] written;
         using ( var ms = new MemoryStream() )
         {
            read1.WriteModule( ms, new EmittingArguments() { Headers = rArgs1.Headers } );
            written = ms.ToArray();
         }

         var rArgs2 = new ReadingArguments();
         CILMetaData read2;
         using ( var ms = new MemoryStream( written ) )
         {
            read2 = ms.ReadModule( rArgs2 );
         }

         resolver.ResolveEverything( read2 );
         if ( afterSecondRead != null )
         {
            afterSecondRead( read2 );
         }

         Assert.IsTrue( Comparers.MetaDataComparer.Equals( read1, read2 ) );
         Assert.IsTrue( Comparers.HeadersEqualityComparer.Equals( rArgs1.Headers, rArgs2.Headers ) );
      }

   }
}
