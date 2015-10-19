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
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace CILAssemblyManipulator.Physical.IO
{
   public interface ReaderFunctionalityProvider
   {
      ReaderFunctionality GetFunctionality(
         Stream stream,
         out Stream newStream
         );
   }

   public interface ReaderFunctionality
   {
      void ReadImageInformation(
         StreamHelper stream,
         out PEInformation peInfo,
         out RVAConverter rvaConverter,
         out CLIHeader cliHeader,
         out MetaDataRoot mdRoot
         );

      AbstractReaderStreamHandler CreateStreamHandler(
         StreamHelper stream,
         MetaDataStreamHeader header
         );


      ReaderRVAHandler CreateRVAHandler(
         StreamHelper stream,
         ImageInformation imageInfo,
         RVAConverter rvaConverter,
         CILMetaData md
         );
   }

   public interface RVAConverter
   {
      Int64 ToRVA( Int64 offset );

      Int64 ToOffset( Int64 rva );
   }

   public interface AbstractReaderStreamHandler
   {
      String StreamName { get; }
   }

   public interface ReaderTableStreamHandler : AbstractReaderStreamHandler
   {
      MetaDataTableStreamHeader ReadHeader();

      void PopulateMetaDataStructure(
         CILMetaData md,
         ReaderBLOBStreamHandler blobs,
         ReaderGUIDStreamHandler guids,
         ReaderStringStreamHandler sysStrings,
         ReaderStringStreamHandler userStrings,
         IEnumerable<AbstractReaderStreamHandler> otherStreams,
         List<Int32> methodRVAs,
         List<Int32> fieldRVAs
         );

   }

   public interface ReaderBLOBStreamHandler : AbstractReaderStreamHandler
   {
      Byte[] GetBLOB( Int32 heapIndex );

      Int64 GetStreamOffset( Int32 heapIndex, out Int32 blobSize );

      AbstractSignature ReadSignature( Int32 heapIndex, out Boolean wasFieldSig );

      CustomAttributeSignature ReadCASignature( Int32 heapIndex );

      IEnumerable<AbstractSecurityInformation> ReadSecurityInformation( Int32 heapIndex );

      MarshalingInfo ReadMarshalingInfo( Int32 heapIndex );

      Object ReadConstantValue( Int32 heapIndex, SignatureElementTypes constType );

   }

   public interface ReaderGUIDStreamHandler : AbstractReaderStreamHandler
   {
      Guid? GetGUID( Int32 heapIndex );

      Guid? GetGUIDNoCache( Int32 heapIndex );
   }

   public interface ReaderStringStreamHandler : AbstractReaderStreamHandler
   {
      String GetString( Int32 heapIndex );

      String GetStringNoCache( Int32 heapIndex );
   }



   public interface ReaderRVAHandler
   {
      MethodILDefinition ReadIL( Int32 methodIndex );

      Byte[] ReadConstantValue( Int32 fieldIndex );

      Byte[] ReadEmbeddedManifestResource( Int32 manifestIndex );
   }
}

public static partial class E_CILPhysical
{
   public static CILMetaData ReadingProcess(
      this Stream stream,
      ReaderFunctionalityProvider readerProvider,
      out ImageInformation imageInfo
      )
   {
      ArgumentValidator.ValidateNotNull( "Stream", stream );

      Stream newStream;
      var reader = ( readerProvider ?? new DefaultReaderFunctionalityProvider() ).GetFunctionality( stream, out newStream );

      CILMetaData md;
      if ( newStream != null && !ReferenceEquals( stream, newStream ) )
      {
         using ( newStream )
         {
            md = newStream.ReadingProcess( reader, out imageInfo );
         }
      }
      else
      {
         md = stream.ReadingProcess( reader, out imageInfo );
      }

      return md;
   }

   public static CILMetaData ReadingProcess(
      this Stream stream,
      ReaderFunctionality reader,
      out ImageInformation imageInfo
      )
   {
      ArgumentValidator.ValidateNotNull( "Stream", stream );

      if ( reader == null )
      {
         reader = new DefaultReaderFunctionality();
      }

      var helper = new StreamHelper( stream );

      // 1. Read image basic information (PE, sections, CLI header, md root)
      PEInformation peInfo;
      RVAConverter rvaConverter;
      CLIHeader cliHeader;
      MetaDataRoot mdRoot;
      reader.ReadImageInformation( helper, out peInfo, out rvaConverter, out cliHeader, out mdRoot );

      if ( peInfo == null )
      {
         throw new BadImageFormatException( "Not a PE image." );
      }
      else if ( cliHeader == null )
      {
         throw new BadImageFormatException( "Not a managed assembly." );
      }
      else if ( mdRoot == null )
      {
         throw new BadImageFormatException( "Missing meta-data root." );
      }

      if ( rvaConverter == null )
      {
         rvaConverter = new DefaultRVAConverter( peInfo );
      }

      // 2. Create MD streams
      var mdStreamHeaders = mdRoot.StreamHeaders;
      var mdStreams = new AbstractReaderStreamHandler[mdStreamHeaders.Count];
      for ( var i = 0; i < mdStreams.Length; ++i )
      {
         var hdr = mdStreamHeaders[i];
         var mdHelper = helper.NewStreamPortion( rvaConverter.ToOffset( cliHeader.MetaData.RVA ), (UInt32) hdr.Size );
         mdStreams[i] = reader.CreateStreamHandler( mdHelper, hdr ) ?? CreateDefaultHandlerFor( hdr, helper );
      }

      // 3. Create and populate meta-data structure
      var tblMDStream = mdStreams
         .OfType<ReaderTableStreamHandler>()
         .FirstOrDefault();
      if ( tblMDStream == null )
      {
         throw new BadImageFormatException( "No table stream exists." );
      }
      var tblHeader = tblMDStream.ReadHeader();
      var md = CILMetaDataFactory.NewBlankMetaData( tblHeader.CreateTableSizesArray() );
      var blobStream = mdStreams.OfType<ReaderBLOBStreamHandler>().FirstOrDefault();
      var guidStream = mdStreams.OfType<ReaderGUIDStreamHandler>().FirstOrDefault();
      var sysStringStream = mdStreams.OfType<ReaderStringStreamHandler>().FirstOrDefault( s => String.Equals( s.StreamName, MetaDataConstants.SYS_STRING_STREAM_NAME ) );
      var userStringStream = mdStreams.OfType<ReaderStringStreamHandler>().FirstOrDefault( s => String.Equals( s.StreamName, MetaDataConstants.USER_STRING_STREAM_NAME ) );
      var methodRVAs = new List<Int32>( md.MethodDefinitions.RowCount );
      var fieldRVAs = new List<Int32>( md.FieldRVAs.RowCount );
      tblMDStream.PopulateMetaDataStructure(
         md,
         blobStream,
         guidStream,
         sysStringStream,
         userStringStream,
         mdStreams.Where( s => !ReferenceEquals( tblMDStream, s ) && !ReferenceEquals( blobStream, s ) && !ReferenceEquals( guidStream, s ) && !ReferenceEquals( sysStringStream, s ) && !ReferenceEquals( userStringStream, s ) ),
         methodRVAs,
         fieldRVAs
         );

      // 4. Create image information
      var snDD = cliHeader.StrongNameSignature;
      var snOffset = rvaConverter.ToOffset( snDD.RVA );
      imageInfo = new ImageInformation(
         peInfo,
         new CLIInformation(
            cliHeader,
            mdRoot,
            tblHeader,
            snOffset > 0 && snDD.Size > 0 ?
               helper.At( snOffset ).ReadAndCreateArray( checked((Int32) snDD.Size) ).ToArrayProxy().CQ :
               null,
            methodRVAs.Select( rva => (UInt32) rva ).ToArrayProxy().CQ,
            fieldRVAs.Select( rva => (UInt32) rva ).ToArrayProxy().CQ
            )
         );

      // 5. Populate IL, FieldRVA, and ManifestResource data
      var rvaHandler = reader.CreateRVAHandler( helper, imageInfo, rvaConverter, md );
      for ( var i = 0; i < methodRVAs.Count; ++i )
      {
         md.MethodDefinitions.TableContents[i].IL = rvaHandler.ReadIL( i );
      }
      for ( var i = 0; i < fieldRVAs.Count; ++i )
      {
         md.FieldRVAs.TableContents[i].Data = rvaHandler.ReadConstantValue( i );
      }
      var mResources = md.ManifestResources.TableContents;
      for ( var i = 0; i < mResources.Count; ++i )
      {
         var mRes = mResources[i];
         if ( !mRes.Implementation.HasValue )
         {
            rvaHandler.ReadEmbeddedManifestResource( i );
         }
      }

      // We're done
      return md;
   }

   private static AbstractReaderStreamHandler CreateDefaultHandlerFor( MetaDataStreamHeader header, StreamHelper helper )
   {
      throw new NotImplementedException();
   }
}