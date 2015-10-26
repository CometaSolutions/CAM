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
using CILAssemblyManipulator.Physical.Implementation;
using CILAssemblyManipulator.Physical.IO;
using CollectionsWithRoles.API;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO
{
   public interface WriterFunctionalityProvider
   {
      WriterFunctionality GetFunctionality(
         CILMetaData md,
         WritingOptions options,
         out CILMetaData newMD,
         out Stream newStream
         );
   }

   public interface WriterFunctionality
   {
      IEnumerable<AbstractWriterStreamHandler> CreateStreamHandlers();

      RawValueStorage<Int64> CreateRawValuesBeforeMDStreams(
         Stream stream,
         ResizableArray<Byte> array,
         WriterMetaDataStreamContainer mdStreams,
         WritingStatus writingStatus
         );

      IEnumerable<SectionHeader> CreateSections(
         WritingStatus writingStatus,
         out RVAConverter rvaConverter
         );

      void FinalizeWritingStatus(
         WritingStatus writingStatus
         );

   }

   public class WriterMetaDataStreamContainer
   {
      public WriterMetaDataStreamContainer(
         WriterBLOBStreamHandler blobs,
         WriterGUIDStreamHandler guids,
         WriterStringStreamHandler sysStrings,
         WriterStringStreamHandler userStrings,
         IEnumerable<AbstractWriterStreamHandler> otherStreams
         )
      {
         this.BLOBs = blobs;
         this.GUIDs = guids;
         this.SystemStrings = sysStrings;
         this.UserStrings = userStrings;
         this.OtherStreams = otherStreams.ToArrayProxy().CQ;
      }

      public WriterBLOBStreamHandler BLOBs { get; }

      public WriterGUIDStreamHandler GUIDs { get; }

      public WriterStringStreamHandler SystemStrings { get; }

      public WriterStringStreamHandler UserStrings { get; }

      public ArrayQuery<AbstractWriterStreamHandler> OtherStreams { get; }
   }


   public interface AbstractWriterStreamHandler
   {
      String StreamName { get; }

      void WriteStream(
         Stream sink,
         ResizableArray<Byte> array,
         RawValueStorage<Int64> rawValuesBeforeStreams,
         RVAConverter rvaConverter
         );

      /// <summary>
      /// This should be max UInt32.Value
      /// </summary>
      Int64 CurrentSize { get; }

      Boolean Accessed { get; }
   }

   public interface WriterTableStreamHandler : AbstractWriterStreamHandler
   {
      RawValueStorage<Int32> FillHeaps(
         RawValueStorage<Int64> rawValuesBeforeStreams,
         ArrayQuery<Byte> thisAssemblyPublicKeyIfPresentNull,
         WriterMetaDataStreamContainer mdStreams,
         ResizableArray<Byte> array
         );
   }

   public interface WriterBLOBStreamHandler : AbstractWriterStreamHandler
   {
      Int32 RegisterBLOB( Byte[] blob );
   }

   public interface WriterStringStreamHandler : AbstractWriterStreamHandler
   {
      Int32 RegisterString( String systemString );
   }

   public interface WriterGUIDStreamHandler : AbstractWriterStreamHandler
   {
      Int32 RegisterGUID( Guid? guid );
   }

   public interface WriterCustomStreamHandler : AbstractWriterStreamHandler
   {
   }

   public class WritingStatus
   {

   }
}


public static partial class E_CILPhysical
{
   public static ImageInformation WriteMetaDataFromStream(
      this Stream stream,
      CILMetaData md,
      WriterFunctionalityProvider writerProvider,
      WritingOptions options
      )
   {
   }

   public static ImageInformation WriteMetaDataFromStream(
      this Stream stream,
      CILMetaData md,
      WriterFunctionality writer,
      WritingOptions options
      )
   {
      // Check arguments
      ArgumentValidator.ValidateNotNull( "Stream", stream );
      ArgumentValidator.ValidateNotNull( "Meta data", md );

      if ( options == null )
      {
         options = new WritingOptions();
      }

      if ( writer == null )
      {
         writer = new DefaultWriterFunctionality( md, options );
      }

      var status = new WritingStatus();

      // 1. Create streams
      var mdStreams = writer.CreateStreamHandlers().ToArrayProxy().CQ;
      var tblMDStream = mdStreams
         .OfType<WriterTableStreamHandler>()
         .FirstOrDefault() ?? new DefaultWriterTableStreamHandler( md, options.TableStreamOptions, DefaultMetaDataSerializationSupportProvider.Instance.CreateTableSerializationInfos().ToArrayProxy().CQ );

      var blobStream = mdStreams.OfType<WriterBLOBStreamHandler>().FirstOrDefault();
      var guidStream = mdStreams.OfType<WriterGUIDStreamHandler>().FirstOrDefault();
      var sysStringStream = mdStreams.OfType<WriterStringStreamHandler>().FirstOrDefault( s => String.Equals( s.StreamName, MetaDataConstants.SYS_STRING_STREAM_NAME ) );
      var userStringStream = mdStreams.OfType<WriterStringStreamHandler>().FirstOrDefault( s => String.Equals( s.StreamName, MetaDataConstants.USER_STRING_STREAM_NAME ) );
      var mdStreamContainer = new WriterMetaDataStreamContainer(
            blobStream,
            guidStream,
            sysStringStream,
            userStringStream,
            mdStreams.Where( s => !ReferenceEquals( tblMDStream, s ) && !ReferenceEquals( blobStream, s ) && !ReferenceEquals( guidStream, s ) && !ReferenceEquals( sysStringStream, s ) && !ReferenceEquals( userStringStream, s ) )
            );

      // 2. Position stream at file alignment, and write raw values (IL, constants, resources)
      stream.Position = options.PEOptions.FileAlignment ?? 0x200;
      var array = new ResizableArray<Byte>();
      var rawValues = writer.CreateRawValuesBeforeMDStreams( stream, array, mdStreamContainer, status );

      // 3. Populate heaps
      tblMDStream.FillHeaps( rawValues, null, mdStreamContainer, array );

      // 4. Create sections
      RVAConverter rvaConverter;
      writer.CreateSections( status, out rvaConverter );

      // 5. Write meta data
      foreach ( var mdStream in mdStreams )
      {
         mdStream.WriteStream( stream, array, rawValues, rvaConverter );
      }

      // 6. Finalize writing status
      writer.FinalizeWritingStatus( status );

      // Create image information
   }

   public static Boolean IsWide( this AbstractWriterStreamHandler stream )
   {
      return stream.CurrentSize > UInt16.MaxValue;
   }
}