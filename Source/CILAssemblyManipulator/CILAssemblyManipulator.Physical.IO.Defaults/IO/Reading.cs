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
extern alias CAMPhysical;
extern alias CAMPhysicalIO;

using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;
using CAMPhysical::CILAssemblyManipulator.Physical.Meta;

using CAMPhysicalIO;
using CAMPhysicalIO::CILAssemblyManipulator.Physical.IO;

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.IO;
using CILAssemblyManipulator.Physical.Meta;
using CILAssemblyManipulator.Physical.IO.Defaults;
using CollectionsWithRoles.API;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TabularMetaData.Meta;


public static partial class E_CILPhysical
{
   /// <summary>
   /// This is extension method to read <see cref="CILMetaData"/> from <see cref="Stream"/> using this <see cref="ReaderFunctionalityProvider"/>.
   /// It takes into account the possible new stream created by <see cref="ReaderFunctionalityProvider.GetFunctionality"/> method.
   /// </summary>
   /// <param name="readerProvider">This <see cref="ReaderFunctionalityProvider"/>.</param>
   /// <param name="stream">The <see cref="Stream"/> to read <see cref="CILMetaData"/> from.</param>
   /// <param name="tableInfoProvider">The <see cref="CILMetaDataTableInformationProvider"/> to use when creating a new instance of <see cref="CILMetaData"/> with <see cref="CILMetaDataFactory.NewBlankMetaData"/>.</param>
   /// <param name="errorHandler">The callback to handle errors during deserialization.</param>
   /// <param name="deserializeDataReferences">Whether to deserialize data references (e.g. <see cref="MethodDefinition.IL"/>).</param>
   /// <param name="imageInfo">This parameter will hold the <see cref="ImageInformation"/> read from the <see cref="Stream"/>.</param>
   /// <returns>An instance of <see cref="CILMetaData"/> with its data read from the <paramref name="stream"/>.</returns>
   /// <exception cref="BadImageFormatException">If the structure of the image represented by <see cref="Stream"/> is invalid (e.g. missing PE header or CLI header, etc).</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="stream"/> is <c>null</c>.</exception>
   /// <exception cref="NullReferenceException">If this <see cref="ReaderFunctionalityProvider"/> is <c>null</c>.</exception>
   /// <remarks>
   /// This method is used by <see cref="CILMetaDataIO.ReadModule"/> to actually perform deserialization, and thus is rarely needed to be used directly.
   /// Instead, use <see cref="CILMetaDataIO.ReadModule"/> or any of the classes implementing <see cref="T:CILAssemblyManipulator.Physical.CILMetaDataLoader"/>.
   /// </remarks>
   /// <seealso cref="ReaderFunctionalityProvider"/>
   /// <seealso cref="CILMetaDataIO.ReadModule"/>
   /// <seealso cref="T:CILAssemblyManipulator.Physical.CILMetaDataLoader"/>
   /// <seealso cref="ReadMetaDataFromStream(ReaderFunctionality, Stream, CILMetaDataTableInformationProvider, EventHandler{SerializationErrorEventArgs}, bool, out ImageInformation, out ColumnValueStorage{int}, out RVAConverter)"/>
   public static CILMetaData ReadMetaDataFromStream(
      this ReaderFunctionalityProvider readerProvider,
      Stream stream,
      CILMetaDataTableInformationProvider tableInfoProvider,
      EventHandler<SerializationErrorEventArgs> errorHandler,
      Boolean deserializeDataReferences,
      out ImageInformation imageInfo
      )
   {
      if ( readerProvider == null )
      {
         throw new NullReferenceException();
      }

      if ( tableInfoProvider == null )
      {
         tableInfoProvider = DefaultMetaDataTableInformationProvider.CreateDefault();
      }

      Stream newStream;
      var reader = readerProvider.GetFunctionality( ArgumentValidator.ValidateNotNullAndReturn( "Stream", stream ), tableInfoProvider, errorHandler, out newStream ) ?? new DefaultReaderFunctionality( new TableSerializationInfoCreationArgs( errorHandler ) );

      CILMetaData md;
      if ( newStream != null && !ReferenceEquals( stream, newStream ) )
      {
         using ( newStream )
         {
            md = reader.ReadMetaDataFromStream( newStream, tableInfoProvider, errorHandler, deserializeDataReferences, out imageInfo );
         }
      }
      else
      {
         md = reader.ReadMetaDataFromStream( stream, tableInfoProvider, errorHandler, deserializeDataReferences, out imageInfo );
      }

      return md;
   }

   /// <summary>
   /// This is extension method to read <see cref="CILMetaData"/> from <see cref="Stream"/> using this <see cref="ReaderFunctionality"/>.
   /// </summary>
   /// <param name="reader">This <see cref="ReaderFunctionality"/>.</param>
   /// <param name="stream">The <see cref="Stream"/> to read <see cref="CILMetaData"/> from.</param>
   /// <param name="tableInfoProvider">The <see cref="CILMetaDataTableInformationProvider"/> to use when creating a new instance of <see cref="CILMetaData"/> with <see cref="CILMetaDataFactory.NewBlankMetaData"/>.</param>
   /// <param name="errorHandler">The callback to handle errors during deserialization.</param>
   /// <param name="deserializeDataReferences">Whether to deserialize data references (e.g. <see cref="MethodDefinition.IL"/>).</param>
   /// <param name="imageInfo">This parameter will hold the <see cref="ImageInformation"/> read from the <see cref="Stream"/>.</param>
   ///// <param name="dataReferences">This parameter will hold the <see cref="RawValueStorage{TValue}"/> returned by <see cref="ReaderTableStreamHandler.PopulateMetaDataStructure"/>.</param>
   ///// <param name="rvaConverter">This parameter will hold the <see cref="RVAConverter"/> obtained with <see cref="ReaderFunctionality.ReadImageInformation"/>, or <see cref="DefaultRVAConverter"/> if that method did not obtain rva converter.</param>
   /// <returns>An instance of <see cref="CILMetaData"/> with its data read from the <paramref name="stream"/>.</returns>
   /// <exception cref="BadImageFormatException">If the structure of the image represented by <see cref="Stream"/> is invalid (e.g. missing PE header or CLI header, etc).</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="stream"/> is <c>null</c>.</exception>
   /// <exception cref="NullReferenceException">If this <see cref="ReaderFunctionality"/> is <c>null</c>.</exception>
   /// <remarks>
   /// This method is used by <see cref="ReadMetaDataFromStream(ReaderFunctionalityProvider, Stream, CILMetaDataTableInformationProvider, EventHandler{SerializationErrorEventArgs}, bool, out ImageInformation)"/>, which in turn is used by <see cref="CILMetaDataIO.ReadModule"/> to actually perform deserialization.
   /// Thus, this method is rarely needed to be used directly.
   /// Instead, use <see cref="CILMetaDataIO.ReadModule"/> or any of the classes implementing <see cref="CILMetaDataLoader"/>.
   /// </remarks>
   public static CILMetaData ReadMetaDataFromStream(
      this ReaderFunctionality reader,
      Stream stream,
      CILMetaDataTableInformationProvider tableInfoProvider,
      EventHandler<SerializationErrorEventArgs> errorHandler,
      Boolean deserializeDataReferences,
      out ImageInformation imageInfo
      //out ColumnValueStorage<Int32> dataReferences
      //out RVAConverter rvaConverter
      )
   {
      if ( reader == null )
      {
         throw new NullReferenceException();
         //reader = new DefaultReaderFunctionality( new TableSerializationInfoCreationArgs( errorHandler ) );
      }

      ArgumentValidator.ValidateNotNull( "Stream", stream );

      var helper = new StreamHelper( stream );

      // 1. Read image basic information (PE, sections, CLI header, md root)
      PEInformation peInfo;
      CLIHeader cliHeader;
      MetaDataRoot mdRoot;
      RVAConverter rvaConverter;
      reader.ReadImageInformation( helper, out peInfo, out rvaConverter, out cliHeader, out mdRoot );

      if ( peInfo == null )
      {
         throw new BadImageFormatException( "Not a PE image." );
      }
      else if ( cliHeader == null )
      {
         throw new BadImageFormatException( "Missing CLI header." );
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

      var dataReferences = tblMDStream.PopulateMetaDataStructure(
         md,
         mdStreamContainer
         );

      // 4. Create image information
      var snDD = cliHeader.StrongNameSignature;
      var snOffset = rvaConverter.ToOffset( snDD.RVA );
      imageInfo = new ImageInformation(
         peInfo,
         helper.ReadDebugInformation( peInfo, rvaConverter ),
         new CLIInformation(
            cliHeader,
            mdRoot,
            tblHeader,
            snOffset > 0 && snDD.Size > 0 ?
               helper.At( snOffset ).ReadAndCreateArray( checked((Int32) snDD.Size) ).ToArrayProxy().CQ :
               null,
             dataReferences
            )
         );

      // 5. Populate IL, FieldRVA, and ManifestResource data
      if ( deserializeDataReferences )
      {
         reader.HandleDataReferences( helper, imageInfo, rvaConverter, mdStreamContainer, md );
      }

      // We're done
      return md;
   }

   private static AbstractReaderStreamHandler CreateDefaultHandlerFor( MetaDataStreamHeader header, StreamHelper helper )
   {
      throw new NotImplementedException( "Creating default handler for stream." );
   }

   private static DebugInformation ReadDebugInformation( this StreamHelper stream, PEInformation peInfo, RVAConverter rvaConverter )
   {
      var dataDirs = peInfo.NTHeader.OptionalHeader.DataDirectories;
      DataDirectory debugDD;
      var debugDDIdx = (Int32) DataDirectories.Debug;
      return dataDirs.Count > debugDDIdx
         && ( debugDD = dataDirs[debugDDIdx] ).RVA > 0 ?
         stream
            .At( rvaConverter.ToOffset( debugDD.RVA ) )
            .ReadDebugInformation() :
         null;
   }
}