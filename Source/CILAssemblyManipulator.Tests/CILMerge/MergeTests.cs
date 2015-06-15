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
using CILMerge;
using Microsoft.Win32;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Tests.CILMerge
{
   public class MergeTests : AbstractCAMTest
   {

      [Test]
      public void TestMergingCILMerge()
      {
         this.PerformTest( new CILMergeOptionsImpl()
         {
            OutPath = Path.Combine( Path.GetDirectoryName( CILMergeLocation ), "CILMergeMerged.dll" ),
            Closed = true,
            Union = true,
            InputAssemblies = new[] { CILMergeLocation },
            XmlDocs = true,
            DoLogging = true
         } );
      }

      [Test]
      public void TestMergingCILMergeMSBuild()
      {
         this.PerformTest( new CILMergeOptionsImpl()
         {
            InputAssemblies = new[] { CILMergeLocation },
            OutPath = Path.Combine( Path.GetDirectoryName( CILMergeLocation ), "CILMergeMerged.dll" ),
            Union = true,
            Closed = true,
            Internalize = true,
            UseFullPublicKeyForRefs = true,
            XmlDocs = true,
            DoLogging = true
         } );
      }

      private void PerformTest( CILMergeOptions options )
      {
         if ( !Path.IsPathRooted( options.OutPath ) )
         {
            options.OutPath = Path.GetFullPath( options.OutPath );
         }

         var outFile = options.OutPath;

         new CILMerger( options, new ConsoleCILMergeLogCallback() )
            .PerformMerge();


         RunPEVerify( outFile, !String.IsNullOrEmpty( options.KeyFile ) );
      }


   }
}
