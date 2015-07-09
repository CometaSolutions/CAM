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
   [Ignore]
   public class MergeTests : AbstractCAMTest
   {
      private class TestCILMergeLogCallback : CILMergeLogCallback
      {
         private Boolean _warningsEncountered;
         private Boolean _errorsEncountered;

         public TestCILMergeLogCallback()
         {

         }


         public void Log( MessageLevel mLevel, String formatString, Object[] args )
         {
            if ( mLevel == MessageLevel.Warning && !this._warningsEncountered )
            {
               this._warningsEncountered = true;
            }
            else if ( mLevel == MessageLevel.Error && !this._errorsEncountered )
            {
               this._errorsEncountered = true;
            }
            ConsoleCILMergeLogCallback.Instance.Log( mLevel, formatString, args );

         }

         public Boolean WarningsEncountered
         {
            get
            {
               return this._warningsEncountered;
            }
         }

         public Boolean ErrorsEncountered
         {
            get
            {
               return this._errorsEncountered;
            }
         }
      }

      //[Test]
      public void TestMergingCILMergeSimple()
      {
         this.PerformTest( new CILMergeOptionsImpl()
         {
            InputAssemblies = new[] { CILMergeLocation },
            OutPath = Path.Combine( Path.GetDirectoryName( CILMergeLocation ), "CILMergeMerged.dll" ),
            Closed = true,
            Union = true,
            NoDebug = true
         } );
      }

      //[Test]
      public void TestMergingCILMergeMoreFeatures()
      {
         this.PerformTest( new CILMergeOptionsImpl()
         {
            InputAssemblies = new[] { CILMergeLocation },
            OutPath = Path.Combine( Path.GetDirectoryName( CILMergeLocation ), "CILMergeMerged.dll" ),
            Union = true,
            Closed = true,
            Internalize = true,
            UseFullPublicKeyForRefs = true,
            XmlDocs = true
         } );
      }

      [Test]
      public void TestMergingCILMergeMSBuildTask()
      {
         var baseDir = Path.GetFullPath( Path.Combine( CILMergeLocation, "..", "..", "..", "..", ".." ) );

         var outDir = Path.Combine( baseDir, "Output", "Release", "dotNET" );

         this.PerformTest( new CILMergeOptionsImpl()
         {
            InputAssemblies = new[] { Path.Combine( baseDir, "Source", "CILMerge", "CILMerge.MSBuild", "obj", "Release", "CILMerge.MSBuild.dll" ) },
            LibPaths = new[] { outDir },
            OutPath = Path.Combine( outDir, "CILMerge.MSBuild.dll" ),
            Union = true,
            Closed = true,
            Internalize = true,
            UseFullPublicKeyForRefs = true,
            XmlDocs = true,
            KeyFile = Path.Combine( baseDir, "Keys", "CILMerge.snk" )
         } );
      }

      [Test]
      public void TestMergingUtilPack()
      {
         var baseDir = Path.GetFullPath( Path.Combine( CILMergeLocation, "..", "..", "..", "..", ".." ) );

         var outDir = Path.Combine( baseDir, "Output", "Release", "SL" );
         this.PerformTest( new CILMergeOptionsImpl()
         {
            InputAssemblies = new[]
            {
               Path.Combine( baseDir, "Source", "UtilPack", "obj", "Release_SL", "UtilPack.dll" ),
               Path.Combine(outDir, "CommonUtils.dll"),
               Path.Combine(outDir, "CollectionsWithRoles.dll")
            },
            LibPaths = new[] { outDir },
            OutPath = Path.Combine( outDir, "UtilPack.dll" ),
            Union = true,
            Closed = true,
            Internalize = true,
            UseFullPublicKeyForRefs = true,
            XmlDocs = true,
            HighEntropyVA = true,
            KeyFile = Path.Combine( baseDir, "Keys", "UtilPack.snk" )
         } );
      }

      //[Test]
      public void TestMergingCAMPhysical()
      {
         var baseDir = Path.GetFullPath( Path.Combine( CILMergeLocation, "..", "..", "..", "..", ".." ) );

         var outDir = Path.Combine( baseDir, "Output", "Debug", "dotNET" );
         var outDirSL = Path.Combine( baseDir, "Output", "Debug", "SL" );

         this.PerformTest( new CILMergeOptionsImpl()
         {
            InputAssemblies = new[]
            {
               Path.Combine( baseDir, "Source", "CILAssemblyManipulator", "CILAssemblyManipulator.Physical", "obj", "Debug_Portable", "CILAssemblyManipulator.Physical.dll" ),
               Path.Combine( outDirSL, "CILAssemblyManipulator.MResources.dll" ),
               Path.Combine( outDirSL, "CILAssemblyManipulator.PDB.dll" )
            },
            LibPaths = new[] { outDirSL },
            OutPath = Path.Combine( outDirSL, "CILAssemblyManipulator.Physical.Testing.dll" ),
            Union = true,
            UseFullPublicKeyForRefs = true,
            XmlDocs = true,
            HighEntropyVA = true,
            KeyFile = Path.Combine( baseDir, "Keys", "CAM.snk" )
         } );
      }

      private void PerformTest( CILMergeOptions options )
      {
         if ( !Path.IsPathRooted( options.OutPath ) )
         {
            options.OutPath = Path.GetFullPath( options.OutPath );
         }

         options.DoLogging = true;

         var outFile = options.OutPath;
         var logCallback = new TestCILMergeLogCallback();

         new CILMerger( options, logCallback )
            .PerformMerge();

         Assert.IsFalse( logCallback.WarningsEncountered, "Warnings encountered during merge." );
         Assert.IsFalse( logCallback.ErrorsEncountered, "Errors encountered during merge." );

         RunPEVerify( outFile, !String.IsNullOrEmpty( options.KeyFile ) );
      }


   }
}
