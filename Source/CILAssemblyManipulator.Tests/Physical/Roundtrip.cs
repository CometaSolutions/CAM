﻿/*
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
extern alias CAMPhysicalR;
extern alias CAMPhysicalM;

using CAMPhysicalIOD;
using CAMPhysicalIOD::CILAssemblyManipulator.Physical;
using CAMPhysicalIOD::CILAssemblyManipulator.Physical.Meta;
using CAMPhysicalIOD::CILAssemblyManipulator.Physical.IO;

using CAMPhysicalIO;
using CAMPhysicalIO::CILAssemblyManipulator.Physical.IO;

using CAMPhysicalR;

using CAMPhysicalM;
using CAMPhysicalM::CILAssemblyManipulator.Physical.MResources;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.IO;

namespace CILAssemblyManipulator.Tests.Physical
{
   [Category( "CAM.Physical" )]
   public class RoundtripTest : AbstractCAMTest
   {

      [Test]
      public void TestRoundtripMSCorLib()
      {
         PerformRoundtripTest( MSCorLibLocation, ValidateAllIsResolved, ValidateAllIsResolved );
      }

      [Test]
      public void TestReadingAndWritingGUIDStream()
      {
         const String ASSEMBLY = "SimpleTestAssembly1";

         var md = CILMetaDataFactory.CreateMinimalAssembly( ASSEMBLY, ASSEMBLY + ".dll" );
         md.ModuleDefinitions.TableContents[0].EditAndContinueGUID = Guid.NewGuid();
         CILMetaData md2;
         using ( var stream = new MemoryStream() )
         {
            md.WriteModule( stream );
            stream.Position = 0;
            md2 = stream.ReadModule();
         }

         Assert.IsTrue( md.AreAllTablesEqual( md2 ) );
      }

      private static void PerformRoundtripTest( String fileLocation, Action<CILMetaData> afterFirstRead, Action<CILMetaData> afterSecondRead )
      {

         var rArgs1 = new ReadingArguments();

         CILMetaData read1;
         using ( var fs = File.OpenRead( fileLocation ) )
         {
            read1 = fs.ReadModule( rArgs1 );
         }

         read1.ResolveEverything();
         //var resManagerList1 = new List<Tuple<String, ResourceManagerEntry>[]>();
         //foreach ( var mr in read1.ManifestResources.TableContents.Where( m => m.IsEmbeddedResource() ) )
         //{
         //   var data = mr.EmbeddedData;
         //   Boolean wasResourceManagerData;
         //   var items = data.ReadResourceManagerEntries( out wasResourceManagerData );
         //   if ( wasResourceManagerData )
         //   {
         //      resManagerList1.Add( items.Select( i =>
         //       {
         //          var idx = i.DataOffset;
         //          return Tuple.Create( i.Name, i.CreateEntry( data ) );
         //       } ).ToArray() );
         //   }
         //}
         if ( afterFirstRead != null )
         {
            afterFirstRead( read1 );
         }

         Byte[] written;
         WritingArguments eArgs = new WritingArguments() { WritingOptions = rArgs1.ImageInformation.CreateWritingOptions() };
         using ( var ms = new MemoryStream() )
         {
            read1.WriteModule( ms, eArgs );
            written = ms.ToArray();
         }

         var rArgs2 = new ReadingArguments();

         CILMetaData read2;
         using ( var ms = new MemoryStream( written ) )
         {
            read2 = ms.ReadModule( rArgs2 );
         }

         read2.ResolveEverything();
         //var resManagerList2 = new List<Tuple<String, ResourceManagerEntry>[]>();
         //foreach ( var mr in read2.ManifestResources.TableContents.Where( m => m.IsEmbeddedResource() ) )
         //{
         //   var data = mr.EmbeddedData;
         //   Boolean wasResourceManagerData;
         //   var items = data.ReadResourceManagerEntries( out wasResourceManagerData );
         //   if ( wasResourceManagerData )
         //   {
         //      resManagerList2.Add( items.Select( i =>
         //      {
         //         var idx = i.DataOffset;
         //         return Tuple.Create( i.Name, i.CreateEntry( data ) );
         //      } ).ToArray() );
         //   }
         //}
         if ( afterSecondRead != null )
         {
            afterSecondRead( read2 );
         }

         // Re-calculate max stack sizes.
         // Sometimes methods have large format, even though they could've had tiny format (but tiny format loses stack size info)
         var read1MDefs = read1.MethodDefinitions.TableContents;
         var read2MDefs = read2.MethodDefinitions.TableContents;
         for ( var i = 0; i < read1MDefs.Count; ++i )
         {
            if ( read1.IsTinyILHeader( i ) && read2.IsTinyILHeader( i ) )
            {
               var il = read1MDefs[i].IL;
               var il2 = read2MDefs[i].IL;
               il2.MaxStackSize = il.MaxStackSize;
               il2.InitLocals = il.InitLocals;
            }
         }

         Assert.IsTrue( read1.AreAllTablesEqual( read2 ) );
         // We don't use public key when emitting module
         //rArgs1.Headers.ModuleFlags = ModuleFlags.ILOnly;
         Assert.IsTrue( CAMPhysicalIO::CILAssemblyManipulator.Physical.Comparers.ImageInformationLogicalEqualityComparer.Equals( rArgs1.ImageInformation, rArgs2.ImageInformation ) );
         //Assert.IsTrue( CommonUtils.ListEqualityComparer<List<Tuple<String, ResourceManagerEntry>[]>, Tuple<String, ResourceManagerEntry>[]>.ListEquality( resManagerList1, resManagerList2, ( xArray, yArray ) => CommonUtils.ArrayEqualityComparer<Tuple<String, ResourceManagerEntry>>.ArrayEquality( xArray, yArray, ( x, y ) => String.Equals( x.Item1, y.Item1 ) && CAMPhysicalM::CILAssemblyManipulator.Physical.Comparers.ResourceManagerEntryEqualityComparer.Equals( x.Item2, y.Item2 ) ) ) );
      }

   }
}
