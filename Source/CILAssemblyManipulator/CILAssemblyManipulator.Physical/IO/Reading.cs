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
using CILAssemblyManipulator.Physical.IO.Defaults;
using CollectionsWithRoles.API;
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
         Meta.MetaDataTableInformationProvider mdTableInfoProvider,
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
         MetaDataRoot mdRoot,
         Int64 startPosition,
         MetaDataStreamHeader header
         );

      void HandleStoredRawValues(
         StreamHelper stream,
         ImageInformation imageInfo,
         RVAConverter rvaConverter,
         ReaderMetaDataStreamContainer mdStreamContainer,
         CILMetaData md,
         RawValueStorage<Int32> rawValues
         );
   }

   public sealed class RawValueStorage<TValue>
   {
      private readonly ArrayQuery<Int32> _tableSizes;
      private readonly Int32[] _tableColCount;
      private readonly Int32[] _tableStartOffsets;
      private readonly TValue[] _rawValues;
      private Int32 _currentIndex;

      public RawValueStorage(
         ArrayQuery<Int32> tableSizes,
         IEnumerable<Int32> rawColumnInfo
         )
      {
         this._tableSizes = tableSizes;
         this._tableColCount = rawColumnInfo.ToArray();
         this._tableStartOffsets = tableSizes
            .AggregateIntermediate_BeforeAggregation(
               0,
               ( cur, size, idx ) => cur += size * this._tableColCount[idx]
               )
            .ToArray();
         this._rawValues = new TValue[tableSizes.Select( ( size, idx ) => size * this._tableColCount[idx] ).Sum()];
         this._currentIndex = 0;
      }

      public void AddRawValue( TValue rawValue )
      {
         this._rawValues[this._currentIndex++] = rawValue;
      }

      public IEnumerable<TValue> GetAllRawValuesForColumn( Tables table, Int32 columnIndex )
      {
         var size = this._tableSizes[(Int32) table];
         for ( var i = this._tableStartOffsets[(Int32) table]; i < size; ++i )
         {
            yield return this._rawValues[i + columnIndex];
         }
      }

      public IEnumerable<TValue> GetAllRawValuesForRow( Tables table, Int32 rowIndex )
      {
         var size = this._tableColCount[(Int32) table];
         var startOffset = this._tableStartOffsets[(Int32) table] + rowIndex * size;
         for ( var i = 0; i < size; ++i )
         {
            yield return this._rawValues[startOffset];
            ++startOffset;
         }
      }

      public TValue GetRawValue( Tables table, Int32 rowIndex, Int32 columnIndex )
      {
         return this._rawValues[this._tableStartOffsets[(Int32) table] + rowIndex * this._tableColCount[(Int32) table] + columnIndex];
      }

   }

   public interface RVAConverter
   {
      Int64 ToRVA( Int64 offset );

      Int64 ToOffset( Int64 rva );
   }

   public class ReaderMetaDataStreamContainer
   {
      public ReaderMetaDataStreamContainer(
         ReaderBLOBStreamHandler blobs,
         ReaderGUIDStreamHandler guids,
         ReaderStringStreamHandler sysStrings,
         ReaderStringStreamHandler userStrings,
         IEnumerable<AbstractReaderStreamHandler> otherStreams
         )
      {
         this.BLOBs = blobs;
         this.GUIDs = guids;
         this.SystemStrings = sysStrings;
         this.UserStrings = userStrings;
         this.OtherStreams = otherStreams.ToArrayProxy().CQ;
      }

      public ReaderBLOBStreamHandler BLOBs { get; }

      public ReaderGUIDStreamHandler GUIDs { get; }

      public ReaderStringStreamHandler SystemStrings { get; }

      public ReaderStringStreamHandler UserStrings { get; }

      public ArrayQuery<AbstractReaderStreamHandler> OtherStreams { get; }
   }

   public interface AbstractReaderStreamHandler
   {
      String StreamName { get; }
   }

   public interface ReaderTableStreamHandler : AbstractReaderStreamHandler
   {
      MetaDataTableStreamHeader ReadHeader();

      ArrayQuery<Int32> TableSizes { get; }

      RawValueStorage<Int32> PopulateMetaDataStructure(
         CILMetaData md,
         ReaderMetaDataStreamContainer mdStreamContainer
         );

   }

   public interface ReaderBLOBStreamHandler : AbstractReaderStreamHandler
   {
      Byte[] GetBLOB( Int32 heapIndex );

      AbstractSignature ReadNonTypeSignature( Int32 heapIndex, Boolean methodSigIsDefinition, Boolean handleFieldSigAsLocalsSig, out Boolean fieldSigTransformedToLocalsSig );

      TypeSignature ReadTypeSignature( Int32 heapIndex );

      AbstractCustomAttributeSignature ReadCASignature( Int32 heapIndex );

      void ReadSecurityInformation( Int32 heapIndex, List<AbstractSecurityInformation> securityInfo );

      AbstractMarshalingInfo ReadMarshalingInfo( Int32 heapIndex );

      Object ReadConstantValue( Int32 heapIndex, SignatureElementTypes constType );

   }

   public interface ReaderGUIDStreamHandler : AbstractReaderStreamHandler
   {
      Guid? GetGUID( Int32 heapIndex );
   }

   public interface ReaderStringStreamHandler : AbstractReaderStreamHandler
   {
      String GetString( Int32 heapIndex );
   }
}

public static partial class E_CILPhysical
{
   public static CILMetaData ReadMetaDataFromStream(
      this Stream stream,
      ReaderFunctionalityProvider readerProvider,
      CILAssemblyManipulator.Physical.Meta.MetaDataTableInformationProvider tableInfoProvider,
      out ImageInformation imageInfo
      )
   {
      ArgumentValidator.ValidateNotNull( "Stream", stream );

      if ( tableInfoProvider == null )
      {
         tableInfoProvider = CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider.CreateDefault();
      }

      Stream newStream;
      var reader = ( readerProvider ?? new DefaultReaderFunctionalityProvider() ).GetFunctionality( stream, tableInfoProvider, out newStream );

      CILMetaData md;
      if ( newStream != null && !ReferenceEquals( stream, newStream ) )
      {
         using ( newStream )
         {
            md = newStream.ReadMetaDataFromStream( reader, tableInfoProvider, out imageInfo );
         }
      }
      else
      {
         md = stream.ReadMetaDataFromStream( reader, tableInfoProvider, out imageInfo );
      }

      return md;
   }

   public static CILMetaData ReadMetaDataFromStream(
      this Stream stream,
      ReaderFunctionality reader,
      CILAssemblyManipulator.Physical.Meta.MetaDataTableInformationProvider tableInfoProvider,
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
         rvaConverter = new DefaultRVAConverter( peInfo.SectionHeaders );
      }

      // 2. Create MD streams
      var mdStreamHeaders = mdRoot.StreamHeaders;
      var mdStreams = new AbstractReaderStreamHandler[mdStreamHeaders.Count];
      for ( var i = 0; i < mdStreams.Length; ++i )
      {
         var hdr = mdStreamHeaders[i];
         var startPos = rvaConverter.ToOffset( cliHeader.MetaData.RVA ) + hdr.Offset;
         var mdHelper = helper.NewStreamPortion( startPos, (UInt32) hdr.Size );
         mdStreams[i] = reader.CreateStreamHandler( mdHelper, mdRoot, 0, hdr ) ?? CreateDefaultHandlerFor( hdr, helper );
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
      var md = CILMetaDataFactory.NewBlankMetaData( sizes: tblMDStream.TableSizes.ToArray(), tableInfoProvider: tableInfoProvider );
      var blobStream = mdStreams.OfType<ReaderBLOBStreamHandler>().FirstOrDefault();
      var guidStream = mdStreams.OfType<ReaderGUIDStreamHandler>().FirstOrDefault();
      var sysStringStream = mdStreams.OfType<ReaderStringStreamHandler>().FirstOrDefault( s => String.Equals( s.StreamName, MetaDataConstants.SYS_STRING_STREAM_NAME ) );
      var userStringStream = mdStreams.OfType<ReaderStringStreamHandler>().FirstOrDefault( s => String.Equals( s.StreamName, MetaDataConstants.USER_STRING_STREAM_NAME ) );
      var mdStreamContainer = new ReaderMetaDataStreamContainer(
            blobStream,
            guidStream,
            sysStringStream,
            userStringStream,
            mdStreams.Where( s => !ReferenceEquals( tblMDStream, s ) && !ReferenceEquals( blobStream, s ) && !ReferenceEquals( guidStream, s ) && !ReferenceEquals( sysStringStream, s ) && !ReferenceEquals( userStringStream, s ) )
            );

      var rawValueStorage = tblMDStream.PopulateMetaDataStructure(
         md,
         mdStreamContainer
         );

      // 4. Create image information
      var snDD = cliHeader.StrongNameSignature;
      var snOffset = rvaConverter.ToOffset( snDD.RVA );
      imageInfo = new ImageInformation(
         peInfo,
         helper.NewDebugInformationFromStream( peInfo, rvaConverter ),
         new CLIInformation(
            cliHeader,
            mdRoot,
            tblHeader,
            snOffset > 0 && snDD.Size > 0 ?
               helper.At( snOffset ).ReadAndCreateArray( checked((Int32) snDD.Size) ).ToArrayProxy().CQ :
               null,
            rawValueStorage.GetAllRawValuesForColumn( Tables.MethodDef, 0 ).Select( rva => (UInt32) rva ).ToArrayProxy().CQ,
            rawValueStorage.GetAllRawValuesForColumn( Tables.FieldRVA, 0 ).Select( rva => (UInt32) rva ).ToArrayProxy().CQ
            )
         );

      // 5. Populate IL, FieldRVA, and ManifestResource data
      reader.HandleStoredRawValues( helper, imageInfo, rvaConverter, mdStreamContainer, md, rawValueStorage );

      // We're done
      return md;
   }

   private static AbstractReaderStreamHandler CreateDefaultHandlerFor( MetaDataStreamHeader header, StreamHelper helper )
   {
      throw new NotImplementedException( "Creating default handler for stream." );
   }
}