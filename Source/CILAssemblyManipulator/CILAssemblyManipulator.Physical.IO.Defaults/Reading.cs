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

using CILAssemblyManipulator.Physical.IO;
using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TabularMetaData.Meta;
using CILAssemblyManipulator.Physical.Meta;

namespace CILAssemblyManipulator.Physical.IO.Defaults
{
   /// <summary>
   /// This class provides default implementation for <see cref="ReaderFunctionalityProvider"/>.
   /// </summary>
   public class DefaultReaderFunctionalityProvider : ReaderFunctionalityProvider
   {
      /// <summary>
      /// This method implements <see cref="ReaderFunctionalityProvider.GetFunctionality"/>, and will return <see cref="DefaultReaderFunctionality"/>.
      /// </summary>
      /// <param name="stream">The <see cref="Stream"/>.</param>
      /// <param name="mdTableInfoProvider">The <see cref="CILMetaDataTableInformationProvider"/>.</param>
      /// <param name="errorHandler">The error handler callback.</param>
      /// <param name="deserializingDataReferences">Whether data references are deserialized.</param>
      /// <param name="newStream">This parameter will hold a new <see cref="MemoryStream"/> if <paramref name="stream"/> is not already a <see cref="MemoryStream"/>, and <paramref name="deserializingDataReferences"/> is <c>true</c>. Otherwise, this will be <c>null</c>.</param>
      /// <returns>A new instance of <see cref="DefaultReaderFunctionality"/>.</returns>
      public virtual ReaderFunctionality GetFunctionality(
         Stream stream,
         CILMetaDataTableInformationProvider mdTableInfoProvider,
         EventHandler<SerializationErrorEventArgs> errorHandler,
         Boolean deserializingDataReferences,
         out Stream newStream
         )
      {
         newStream = !( stream is MemoryStream ) && deserializingDataReferences ?
            new MemoryStream( stream.ReadUntilTheEnd(), this.IsMemoryStreamWriteable ) :
            null;
         return new DefaultReaderFunctionality( new TableSerializationLogicalFunctionalityCreationArgs( errorHandler ), tableInfoProvider: mdTableInfoProvider );
      }

      /// <summary>
      /// Gets the value indicating whether the memory stream created by <see cref="GetFunctionality"/> method is writeable.
      /// </summary>
      /// <value>The value indicating whether the memory stream created by <see cref="GetFunctionality"/> method is writeable.</value>
      /// <remarks>
      /// By default, this method returns <c>false</c>.
      /// Subclasses may override for customized behaviour.
      /// </remarks>
      protected virtual Boolean IsMemoryStreamWriteable
      {
         get
         {
            return false;
         }
      }
   }

   /// <summary>
   /// This class provides default implementation for <see cref="ReaderFunctionality"/>.
   /// </summary>
   public class DefaultReaderFunctionality : ReaderFunctionality
   {

      /// <summary>
      /// Creates a new instance of <see cref="DefaultReaderFunctionality"/> with given <see cref="TableSerializationLogicalFunctionalityCreationArgs"/> and optional <see cref="CILMetaDataTableInformationProvider"/>.
      /// </summary>
      /// <param name="serializationCreationArgs">The <see cref="TableSerializationLogicalFunctionalityCreationArgs"/> for table stream.</param>
      /// <param name="tableInfoProvider">The optional <see cref="CILMetaDataTableInformationProvider"/>. If not supplied, the result of <see cref="DefaultMetaDataTableInformationProvider.CreateDefault"/> will be used.</param>
      public DefaultReaderFunctionality(
         TableSerializationLogicalFunctionalityCreationArgs serializationCreationArgs,
         CILMetaDataTableInformationProvider tableInfoProvider = null
         )
      {
         this.SerializationCreationArgs = serializationCreationArgs;
         this.TableInfoProvider = tableInfoProvider ?? DefaultMetaDataTableInformationProvider.CreateDefault();
      }

      /// <inheritdoc />
      public virtual Boolean ReadImageInformation(
         StreamHelper stream,
         out PEInformation peInfo,
         out RVAConverter rvaConverter,
         out CLIHeader cliHeader,
         out MetaDataRoot mdRoot
         )
      {
         // Read PE info
         peInfo = stream.ReadPEInformation();

         // Create RVA converter
         rvaConverter = this.CreateRVAConverter( peInfo ) ?? CreateDefaultRVAConverter( peInfo );

         var cliDDRVA = peInfo.NTHeader.OptionalHeader.DataDirectories.GetOrDefault( (Int32) DataDirectories.CLIHeader ).RVA;

         var retVal = cliDDRVA > 0;
         if ( retVal )
         {
            // Read CLI header
            cliHeader = stream
               .GoToRVA( rvaConverter, cliDDRVA )
               .ReadCLIHeader();

            // Read MD root
            mdRoot = stream
               .GoToRVA( rvaConverter, cliHeader.MetaData.RVA )
               .ReadMetaDataRoot();
         }
         else
         {
            cliHeader = null;
            mdRoot = null;
         }

         return retVal;
      }

      /// <summary>
      /// This method implements <see cref="ReaderFunctionality.CreateStreamHandler"/> by creating an appropriate <see cref="AbstractReaderStreamHandler"/> based on the <see cref="MetaDataStreamHeader.Name"/> of the given <see cref="MetaDataStreamHeader"/>.
      /// </summary>
      /// <param name="stream">The stream containing data.</param>
      /// <param name="mdRoot">The <see cref="MetaDataRoot"/>.</param>
      /// <param name="startPosition">The start position of the stream.</param>
      /// <param name="header">The <see cref="MetaDataStreamHeader"/>.</param>
      /// <returns>Appropriate <see cref="AbstractReaderStreamHandler"/> based on the <see cref="MetaDataStreamHeader.Name"/> of the <paramref name="header"/>.</returns>
      /// <remarks>
      /// The logic is as follows:
      /// <list type="table">
      /// <listheader>
      /// <term>The value of <see cref="MetaDataStreamHeader.Name"/></term>
      /// <term>The type of the returned <see cref="AbstractReaderStreamHandler"/></term>
      /// </listheader>
      /// <item>
      /// <term><c>"#~"</c> or <c>"#-"</c></term>
      /// <term><see cref="DefaultReaderTableStreamHandler"/></term>
      /// </item>
      /// <item>
      /// <term><c>"#Blob"</c></term>
      /// <term><see cref="DefaultReaderBLOBStreamHandler"/></term>
      /// </item>
      /// <item>
      /// <term><c>"#GUID"</c></term>
      /// <term><see cref="DefaultReaderGUIDStreamHandler"/></term>
      /// </item>
      /// <item>
      /// <term><c>"#Strings"</c></term>
      /// <term><see cref="DefaultReaderSystemStringStreamHandler"/></term>
      /// </item>
      /// <item>
      /// <term><c>"#US"</c></term>
      /// <term><see cref="DefaultReaderUserStringsStreamHandler"/></term>
      /// </item>
      /// <item>
      /// <term>Anything else</term>
      /// <term>A <c>null</c> value.</term>
      /// </item>
      /// </list>
      /// </remarks>
      public virtual AbstractReaderStreamHandler CreateStreamHandler(
         StreamHelper stream,
         MetaDataRoot mdRoot,
         Int64 startPosition,
         MetaDataStreamHeader header
         )
      {
         var size = (Int32) header.Size;
         switch ( header.Name )
         {
            case MetaDataConstants.TABLE_STREAM_NAME:
            case "#-":
               return new DefaultReaderTableStreamHandler( stream, startPosition, size, header.Name, this.TableInfoProvider, this.SerializationCreationArgs, mdRoot );
            case MetaDataConstants.BLOB_STREAM_NAME:
               return new DefaultReaderBLOBStreamHandler( stream, startPosition, size );
            case MetaDataConstants.GUID_STREAM_NAME:
               return new DefaultReaderGUIDStreamHandler( stream, startPosition, size );
            case MetaDataConstants.SYS_STRING_STREAM_NAME:
               return new DefaultReaderSystemStringStreamHandler( stream, startPosition, size );
            case MetaDataConstants.USER_STRING_STREAM_NAME:
               return new DefaultReaderUserStringsStreamHandler( stream, startPosition, size );
            default:
               return null;
         }
      }

      /// <summary>
      /// This method implements <see cref="ReaderFunctionality.CreateBlankMetaData"/> by calling <see cref="CILMetaDataFactory.NewBlankMetaData"/> and passing given table sizes and <see cref="TableInfoProvider"/> as arguments.
      /// </summary>
      /// <param name="tableSizes">The table size array.</param>
      /// <returns>A new, blank instance of <see cref="CILMetaData"/>.</returns>
      public virtual CILMetaData CreateBlankMetaData( ArrayQuery<Int32> tableSizes )
      {
         return CILMetaDataFactory.NewBlankMetaData(
            sizes: tableSizes.ToArray(),
            tableInfoProvider: this.TableInfoProvider
            );
      }


      /// <summary>
      /// This method is called by <see cref="ReadImageInformation"/> in order to create a new <see cref="RVAConverter"/>.
      /// </summary>
      /// <param name="peInformation">The deserialized <see cref="PEInformation"/>.</param>
      /// <returns>An instance of <see cref="DefaultRVAConverter"/>.</returns>
      /// <remarks>
      /// Subclasses may override this to return something else.
      /// By default, this returns result of <see cref="CreateDefaultRVAConverter"/>.
      /// </remarks>
      protected virtual RVAConverter CreateRVAConverter(
         PEInformation peInformation
         )
      {
         return CreateDefaultRVAConverter( peInformation );
      }

      /// <summary>
      /// This static method is called by <see cref="CreateRVAConverter"/>.
      /// It returns a new instance of <see cref="DefaultRVAConverter"/>.
      /// </summary>
      /// <param name="peInformation">The deserialized <see cref="PEInformation"/>.</param>
      /// <returns>A new instance of <see cref="DefaultRVAConverter"/>.</returns>
      protected static RVAConverter CreateDefaultRVAConverter(
         PEInformation peInformation
         )
      {
         return new DefaultRVAConverter( peInformation.SectionHeaders );
      }


      /// <summary>
      /// Gets the <see cref="CILMetaDataTableInformationProvider"/> given to this <see cref="DefaultReaderFunctionality"/>.
      /// </summary>
      /// <value>The <see cref="CILMetaDataTableInformationProvider"/> given to this <see cref="DefaultReaderFunctionality"/>.</value>
      protected CILMetaDataTableInformationProvider TableInfoProvider { get; }

      /// <summary>
      /// Gets the <see cref="TableSerializationLogicalFunctionalityCreationArgs"/> passed to this <see cref="DefaultReaderFunctionality"/>.
      /// </summary>
      /// <value>The <see cref="TableSerializationLogicalFunctionalityCreationArgs"/> passed to this <see cref="DefaultReaderFunctionality"/>.</value>
      protected TableSerializationLogicalFunctionalityCreationArgs SerializationCreationArgs { get; }

   }

   /// <summary>
   /// This class implements <see cref="AbstractReaderStreamHandler"/> so that all the contents of the meta data stream is accessible from a single byte array.
   /// </summary>
   public abstract class AbstractReaderStreamHandlerWithArray : AbstractReaderStreamHandler
   {
      /// <summary>
      /// Initializes a new instance of <see cref="AbstractReaderStreamHandlerWithArray"/> with the contents read from the given stream.
      /// </summary>
      /// <param name="stream">The stream, as <see cref="StreamHelper"/>.</param>
      /// <param name="startPosition">The start position of the <paramref name="stream"/> where this meta data stream begins.</param>
      /// <param name="streamSize">The size of this meta data stream, in bytes.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="stream"/> is <c>null</c>.</exception>
      protected AbstractReaderStreamHandlerWithArray(
         StreamHelper stream,
         Int64 startPosition,
         Int32 streamSize
         )
      {
         ArgumentValidator.ValidateNotNull( "Stream", stream );

         this.Bytes = stream.At( startPosition ).ReadAndCreateArray( streamSize );
         this.StreamSizeU32 = (UInt32) streamSize;
      }

      /// <summary>
      /// Leaves the implementation of the <see cref="AbstractMetaDataStreamHandler.StreamName"/> to subclasses.
      /// </summary>
      public abstract String StreamName { get; }

      /// <summary>
      /// Implements the <see cref="AbstractMetaDataStreamHandler.StreamSize"/> as downcast of <see cref="StreamSizeU32"/>.
      /// </summary>
      /// <value>The size of the stream, downcasted from <see cref="StreamSizeU32"/>.</value>
      public Int32 StreamSize
      {
         get
         {
            return (Int32) this.StreamSizeU32;
         }
      }

      /// <summary>
      /// Gets the contents of the stream as byte array.
      /// </summary>
      /// <value>The contents of the stream as byte array.</value>
      protected Byte[] Bytes { get; }

      /// <summary>
      /// Gets the size of this stream, as <see cref="UInt32"/>.
      /// </summary>
      /// <value>The size of this stream, as <see cref="UInt32"/>.</value>
      [CLSCompliant( false )]
      protected UInt32 StreamSizeU32 { get; }
   }

   /// <summary>
   /// This class extends the <see cref="AbstractReaderStreamHandlerWithArray"/> so that it also stores the name of the stream into a field.
   /// </summary>
   public abstract class AbstractReaderStreamHandlerWithArrayAndName : AbstractReaderStreamHandlerWithArray
   {
      /// <summary>
      /// Initializes a new instance of <see cref="AbstractReaderStreamHandlerWithArrayAndName"/> with given stream and name.
      /// </summary>
      /// <param name="stream">The stream, as <see cref="StreamHelper"/>.</param>
      /// <param name="startPosition">The start position of the <paramref name="stream"/> where this meta data stream begins.</param>
      /// <param name="streamSize">The size of this meta data stream, in bytes.</param>
      /// <param name="streamName">The textual name of this stream.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="stream"/> is <c>null</c>.</exception>
      protected AbstractReaderStreamHandlerWithArrayAndName(
         StreamHelper stream,
         Int64 startPosition,
         Int32 streamSize,
         String streamName
         )
         : base( stream, startPosition, streamSize )
      {

         this.StreamName = streamName;
      }

      /// <summary>
      /// Gets the stored name of this <see cref="AbstractReaderStreamHandlerWithArrayAndName"/>.
      /// </summary>
      /// <value>The stored name of this <see cref="AbstractReaderStreamHandlerWithArrayAndName"/>.</value>
      public override String StreamName { get; }

   }

   /// <summary>
   /// This class provides default implementation for <see cref="ReaderTableStreamHandler"/>.
   /// It will use the <see cref="MetaDataTableInformationWithSerializationCapability.CreateTableSerializationInfoNotGeneric"/> to create <see cref="TableSerializationLogicalFunctionality"/> for each table.
   /// These <see cref="TableSerializationLogicalFunctionality"/> objects will be used by this class to deserialize values for rows of tables of <see cref="CILMetaData"/>.
   /// </summary>
   /// <seealso cref="ReaderTableStreamHandler"/>
   /// <seealso cref="ReaderFunctionality"/>
   public class DefaultReaderTableStreamHandler : AbstractReaderStreamHandlerWithArrayAndName, ReaderTableStreamHandler
   {
      /// <summary>
      /// Creates a new instance of <see cref="DefaultReaderTableStreamHandler"/> with given stream, stream name, serialization functionality, and meta data root.
      /// </summary>
      /// <param name="stream">The stream, as <see cref="StreamHelper"/>.</param>
      /// <param name="startPosition">Position in <paramref name="stream"/>, where this table stream starts.</param>
      /// <param name="streamSize">The size of this table stream, in bytes.</param>
      /// <param name="tableStreamName">The name of this table stream.</param>
      /// <param name="tableInfoProvider">The <see cref="CILMetaDataTableInformationProvider"/> to use when creating <see cref="TableSerializationLogicalFunctionality"/>s.</param>
      /// <param name="serializationCreationArgs">The <see cref="TableSerializationLogicalFunctionalityCreationArgs"/> to use when creating <see cref="TableSerializationLogicalFunctionality"/>s.</param>
      /// <param name="mdRoot">The <see cref="MetaDataRoot"/> holding information about the other streams, used when calculating the size of single table row, in bytes.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="stream"/> or <paramref name="tableInfoProvider"/> is <c>null</c>.</exception>
      public DefaultReaderTableStreamHandler(
         StreamHelper stream,
         Int64 startPosition,
         Int32 streamSize,
         String tableStreamName,
         CILMetaDataTableInformationProvider tableInfoProvider,
         TableSerializationLogicalFunctionalityCreationArgs serializationCreationArgs,
         MetaDataRoot mdRoot
         )
         : base( stream, startPosition, streamSize, tableStreamName )
      {
         ArgumentValidator.ValidateNotNull( "Table info provider", tableInfoProvider );

         var array = this.Bytes;
         var idx = 0;
         var tableHeader = array.ReadTableStreamHeader( ref idx );
         var thFlags = tableHeader.TableStreamFlags;

         var tableStartPosition = idx;
         this.TableStreamHeader = tableHeader;
         this.TableSizes = tableHeader.CreateTableSizesArray().ToArrayProxy().CQ;

         var tableSerializationsArray = serializationCreationArgs.CreateTableSerializationInfos( tableInfoProvider.GetAllSupportedTableInformations() ).ToArray();
         this.TableSerializationInfos = tableSerializationsArray
            .Concat( Enumerable.Repeat<TableSerializationLogicalFunctionality>( null, Math.Max( 0, this.TableSizes.Count - tableSerializationsArray.Length ) ) )
            .ToArrayProxy()
            .CQ;

         var creationArgs = new TableSerializationBinaryFunctionalityCreationArgs( this.TableSizes, mdRoot.StreamHeaders.ToDictionary_Preserve( sh => sh.Name, sh => (Int32) sh.Size ).ToDictionaryProxy().CQ );
         this.TableSerializationFunctionalities =
            this.TableSerializationInfos
            .Select( table => table?.CreateBinaryFunctionality( creationArgs ) )
            .ToArrayProxy()
            .CQ;

         this.TableWidths =
            this.TableSerializationFunctionalities
            .Select( table => table?.ColumnSerializationSupports.Aggregate( 0, ( curRowBytecount, colInfo ) => curRowBytecount + colInfo.ColumnByteCount ) ?? 0 )
            .ToArrayProxy()
            .CQ;

         this.TableStartOffsets =
            this.TableSizes
            .AggregateIntermediate_BeforeAggregation( tableStartPosition, ( curOffset, size, i ) => curOffset + size * this.TableWidths[i] )
            .ToArrayProxy()
            .CQ;

      }

      /// <summary>
      /// This method implements <see cref="ReaderTableStreamHandler.PopulateMetaDataStructure"/> by calling <see cref="TableSerializationBinaryFunctionality.ReadRows"/> for each <see cref="TableSerializationBinaryFunctionality"/> in <see cref="TableSerializationFunctionalities"/> array.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/> to populate</param>
      /// <param name="mdStreamContainer">The <see cref="ReaderMetaDataStreamContainer"/> containing other meta data streams.</param>
      /// <returns>A <see cref="DataReferencesInfo"/> object containing information about data references.</returns>
      /// <seealso cref="TableSerializationFunctionalities"/>
      /// <seealso cref="TableSerializationBinaryFunctionality.ReadRows"/>
      public virtual DataReferencesInfo PopulateMetaDataStructure(
         CILMetaData md,
         ReaderMetaDataStreamContainer mdStreamContainer
         )
      {
         var rawValueStorage = this.CreateDataReferencesStorage() ?? this.CreateDefaultDataReferencesStorage();
         var array = this.Bytes;
         for ( var i = 0; i < this.TableSizes.Count; ++i )
         {
            var rowCount = this.TableSizes[i];
            if ( rowCount > 0 )
            {
               var args = new RowReadingArguments( mdStreamContainer, rawValueStorage, md );

               var table = md.GetByTable( i );
               this.TableSerializationFunctionalities[i].ReadRows( table, this.TableSizes[i], array, this.TableStartOffsets[i], args );
            }
         }

         return rawValueStorage.CreateDataReferencesInfo( i => (UInt32) i );
      }

      /// <summary>
      /// This method implements <see cref="ReaderFunctionality.HandleDataReferences"/> by calling <see cref="TableSerializationLogicalFunctionality.PopulateDataReferences"/> for each serialization info in <see cref="TableSerializationInfos"/>.
      /// </summary>
      /// <param name="stream">The <see cref="StreamHelper"/>.</param>
      /// <param name="imageInfo">The <see cref="ImageInformation"/> containing the data references.</param>
      /// <param name="rvaConverter">The <see cref="RVAConverter"/>.</param>
      /// <param name="mdStreamContainer">The <see cref="ReaderMetaDataStreamContainer"/>.</param>
      /// <param name="md">The <see cref="CILMetaData"/>.</param>
      /// <seealso cref="TableSerializationInfos"/>
      /// <seealso cref="TableSerializationLogicalFunctionality.PopulateDataReferences"/>
      public virtual void HandleDataReferences(
         StreamHelper stream,
         ImageInformation imageInfo,
         RVAConverter rvaConverter,
         ReaderMetaDataStreamContainer mdStreamContainer,
         CILMetaData md
         )
      {
         var args = this.CreateDataReferencesProcessingArgs( stream, imageInfo, rvaConverter, mdStreamContainer, md ) ?? CreateDefaultDataReferencesProcessingArgs( stream, imageInfo, rvaConverter, mdStreamContainer, md );
         foreach ( var tableSerialization in this.TableSerializationInfos )
         {
            tableSerialization?.PopulateDataReferences( args );
         }
      }

      /// <summary>
      /// Reads a raw table row directly from this stream, using <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method.
      /// </summary>
      /// <param name="table">The table ID as <see cref="Tables"/> enumeration.</param>
      /// <param name="rowIndex">The zero-based row index.</param>
      /// <returns>A raw row.</returns>
      /// <seealso cref="TableSerializationFunctionalities"/>
      /// <seealso cref="TableSerializationBinaryFunctionality.ReadRawRow"/>
      public virtual Object GetRawRowOrNull( Tables table, Int32 rowIndex )
      {
         var tableSizes = this.TableSizes;
         var tableInt = (Int32) table;
         Object retVal;
         if ( tableInt >= 0
            && tableInt < tableSizes.Count
            && rowIndex >= 0
            && tableSizes[tableInt] > rowIndex
            )
         {
            var offset = this.TableStartOffsets[tableInt] + rowIndex * this.TableWidths[tableInt];
            retVal = this.TableSerializationFunctionalities[tableInt].ReadRawRow( this.Bytes, offset );
         }
         else
         {
            retVal = null;
         }
         return retVal;
      }

      /// <summary>
      /// Implements <see cref="ReaderTableStreamHandler.ReadHeader"/> by returns the <see cref="MetaDataTableStreamHeader"/>, which was read already in constructor.
      /// </summary>
      /// <returns>The <see cref="MetaDataTableStreamHeader"/>.</returns>
      public virtual MetaDataTableStreamHeader ReadHeader()
      {
         return this.TableStreamHeader;
      }

      /// <summary>
      /// This method is called by <see cref="PopulateMetaDataStructure"/> to create storage for data references.
      /// Sublcasses may override this method.
      /// </summary>
      /// <returns>This implementation returns the value of <see cref="CreateDefaultDataReferencesStorage"/>.</returns>
      /// <remarks>
      /// If this method returns <c>null</c>, then the <see cref="PopulateMetaDataStructure"/> will use the result of <see cref="CreateDefaultDataReferencesStorage"/>.
      /// </remarks>
      protected virtual ColumnValueStorage<Int32> CreateDataReferencesStorage()
      {
         return this.CreateDefaultDataReferencesStorage();
      }

      /// <summary>
      /// This method is called by <see cref="CreateDataReferencesStorage"/>.
      /// It returns an instance of <see cref="ColumnValueStorage{TValue}"/>.
      /// </summary>
      /// <returns>An instance of <see cref="ColumnValueStorage{TValue}"/>.</returns>
      protected ColumnValueStorage<Int32> CreateDefaultDataReferencesStorage()
      {
         return new ColumnValueStorage<Int32>(
            this.TableSizes,
            this.TableSerializationInfos.Select( t => t?.DataReferenceColumnCount ?? 0 )
            );
      }

      /// <summary>
      /// This method is called by <see cref="HandleDataReferences"/>, and should return <see cref="DataReferencesProcessingArgs"/> to be used in <see cref="TableSerializationLogicalFunctionality.PopulateDataReferences"/>.
      /// </summary>
      /// <param name="stream">The <see cref="StreamHelper"/>.</param>
      /// <param name="imageInfo">The <see cref="ImageInformation"/>.</param>
      /// <param name="rvaConverter">The <see cref="RVAConverter"/>.</param>
      /// <param name="mdStreamContainer">The <see cref="ReaderMetaDataStreamContainer"/>.</param>
      /// <param name="md">The <see cref="CILMetaData"/>.</param>
      /// <returns>An instance of <see cref="DataReferencesProcessingArgs"/>.</returns>
      /// <remarks>
      /// Subclasses may override this to return something else.
      /// By default, this returns result of <see cref="CreateDefaultDataReferencesProcessingArgs"/>.
      /// </remarks>
      protected virtual DataReferencesProcessingArgs CreateDataReferencesProcessingArgs(
         StreamHelper stream,
         ImageInformation imageInfo,
         RVAConverter rvaConverter,
         ReaderMetaDataStreamContainer mdStreamContainer,
         CILMetaData md
         )
      {
         return CreateDefaultDataReferencesProcessingArgs( stream, imageInfo, rvaConverter, mdStreamContainer, md );
      }

      /// <summary>
      /// This static method is called by <see cref="CreateDataReferencesProcessingArgs"/>.
      /// It returns a new instance of <see cref="DataReferencesProcessingArgs"/>.
      /// </summary>
      /// <param name="stream">The <see cref="StreamHelper"/>.</param>
      /// <param name="imageInfo">The <see cref="ImageInformation"/>.</param>
      /// <param name="rvaConverter">The <see cref="RVAConverter"/>.</param>
      /// <param name="mdStreamContainer">The <see cref="ReaderMetaDataStreamContainer"/>.</param>
      /// <param name="md">The <see cref="CILMetaData"/>.</param>
      /// <returns>An instance of <see cref="DataReferencesProcessingArgs"/>.</returns>
      protected static DataReferencesProcessingArgs CreateDefaultDataReferencesProcessingArgs(
         StreamHelper stream,
         ImageInformation imageInfo,
         RVAConverter rvaConverter,
         ReaderMetaDataStreamContainer mdStreamContainer,
         CILMetaData md
         )
      {
         return new DataReferencesProcessingArgs( stream, imageInfo, rvaConverter, mdStreamContainer, md, new ResizableArray<Byte>( initialSize: 0x1000 ) );
      }

      /// <summary>
      /// Gets the <see cref="MetaDataTableStreamHeader"/> read in constructor.
      /// </summary>
      /// <value>The <see cref="MetaDataTableStreamHeader"/> read in constructor.</value>
      protected MetaDataTableStreamHeader TableStreamHeader { get; }

      /// <summary>
      /// Gets the table size array.
      /// </summary>
      /// <value>The table size array.</value>
      /// <remarks>
      /// The table ID as <see cref="Tables"/> enumeration integer value acts as index, and the array element is the table row count.
      /// </remarks>
      protected ArrayQuery<Int32> TableSizes { get; }

      /// <summary>
      /// Gets the table width array.
      /// </summary>
      /// <value>The table width array.</value>
      /// <remarks>
      /// The table ID as <see cref="Tables"/> enumeration integer value acts as index, and the array element is the byte count for one row.
      /// </remarks>
      protected ArrayQuery<Int32> TableWidths { get; }

      /// <summary>
      /// Gets the table start offset array.
      /// </summary>
      /// <value>The table start offset array.</value>
      /// <remarks>
      /// The table ID as <see cref="Tables"/> enumeration integer value acts as index, and the array element is the byte offset in <see cref="AbstractReaderStreamHandlerWithArray.Bytes"/> where the table rows start.
      /// </remarks>
      protected ArrayQuery<Int32> TableStartOffsets { get; }

      /// <summary>
      /// Gets the array holding <see cref="TableSerializationLogicalFunctionality"/> objects for each table.
      /// </summary>
      /// <value>The array holding <see cref="TableSerializationLogicalFunctionality"/> objects for each table.</value>
      /// <remarks>
      /// The table ID as <see cref="Tables"/> enumeration integer value acts as index, and the array element is the <see cref="TableSerializationLogicalFunctionality"/> object (potentially <c>null</c>) for that table.
      /// </remarks>
      /// <seealso cref="TableSerializationLogicalFunctionality"/>
      protected ArrayQuery<TableSerializationLogicalFunctionality> TableSerializationInfos { get; }

      /// <summary>
      /// Gets the array holding the <see cref="TableSerializationBinaryFunctionality"/> objects for each table.
      /// </summary>
      /// <value>The array holding the <see cref="TableSerializationBinaryFunctionality"/> objects for each table.</value>
      /// <remarks>
      /// The table ID as <see cref="Tables"/> enumeration integer value acts as index, and the array element is the <see cref="TableSerializationBinaryFunctionality"/> object (potentially <c>null</c>) for that table.
      /// </remarks>
      /// <seealso cref="TableSerializationBinaryFunctionality"/>
      protected ArrayQuery<TableSerializationBinaryFunctionality> TableSerializationFunctionalities { get; }

   }

   /// <summary>
   /// This class specializes <see cref="AbstractReaderStreamHandlerWithArray"/> by adding a cache for values at given heap indices.
   /// </summary>
   /// <typeparam name="TValue">The type of values to cache.</typeparam>
   public abstract class AbstractReaderStreamHandlerWithArrayAndCache<TValue> : AbstractReaderStreamHandlerWithArray
   {
      private readonly IDictionary<Int32, TValue> _cache;

      /// <summary>
      /// Initializes this instance of <see cref="AbstractReaderStreamHandlerWithArrayAndCache{TValue}"/> with given stream.
      /// </summary>
      /// <param name="stream">The stream, as <see cref="StreamHelper"/>.</param>
      /// <param name="startPosition">The position in <paramref name="stream"/> where the contents of this meta data stream start.</param>
      /// <param name="streamSize">The size, in bytes, of the contents of this meta data stream.</param>
      protected AbstractReaderStreamHandlerWithArrayAndCache(
         StreamHelper stream,
         Int64 startPosition,
         Int32 streamSize
         ) : base( stream, startPosition, streamSize )
      {
         this._cache = new Dictionary<Int32, TValue>();
      }

      /// <summary>
      /// Subclasses should use this method to retrieve the value from cache or deserialize it from stream.
      /// </summary>
      /// <param name="heapOffset">The heap offset.</param>
      /// <returns>Cached or deserialzied value.</returns>
      /// <remarks>
      /// The <see cref="ValueFactory"/> method will be called if the value is not already cached.
      /// The given <paramref name="heapOffset"/> will be directly passed to it.
      /// This method is not threadsafe.
      /// </remarks>
      /// <seealso cref="ValueFactory"/>
      protected TValue GetOrAddValue( Int32 heapOffset )
      {
         if ( heapOffset == 0 )
         {
            return this.ZeroValue();
         }
         else
         {
            return this.CheckHeapOffset( heapOffset ) ?
               this._cache.GetOrAdd_NotThreadSafe( heapOffset, this.ValueFactory ) :
               (TValue) (Object) null;
         }
      }

      /// <summary>
      /// Subclasses should implement this method to return value to be cached.
      /// </summary>
      /// <param name="heapOffset">The heap offset, as it was given to <seealso cref="GetOrAddValue"/>.</param>
      /// <returns>Deserialized value.</returns>
      protected abstract TValue ValueFactory( Int32 heapOffset );

      /// <summary>
      /// This method is called by <see cref="GetOrAddValue"/> before accessing cache, to check whether given heap offset is of meaningful value.
      /// </summary>
      /// <param name="heapOffset">The heap offset, as it was given to <see cref="GetOrAddValue"/>.</param>
      /// <returns><c>true</c> if <paramref name="heapOffset"/> was meaningful value; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// This implementation just checks whether the value of <paramref name="heapOffset"/> as <see cref="UInt32"/> is smaller than <see cref="AbstractReaderStreamHandlerWithArray.StreamSizeU32"/>.
      /// Subclasses may override this method.
      /// </remarks>
      protected virtual Boolean CheckHeapOffset( Int32 heapOffset )
      {
         return ( (UInt32) heapOffset ) < this.StreamSizeU32;
      }

      /// <summary>
      /// This method is always called by <see cref="GetOrAddValue"/> if the given heap offset was zero.
      /// </summary>
      /// <returns>The value for zero heap offset. By default, it is <c>null</c>.</returns>
      protected virtual TValue ZeroValue()
      {
         return (TValue) (Object) null;
      }
   }

   /// <summary>
   /// This class subclasses the <see cref="AbstractReaderStreamHandlerWithArrayAndCache{TValue}"/> and implements <see cref="ReaderStringStreamHandler"/>, leaving the exact deserialization of the strings as abstract methods.
   /// </summary>
   public abstract class AbstractReaderStringStreamHandler : AbstractReaderStreamHandlerWithArrayAndCache<String>, ReaderStringStreamHandler
   {
      /// <summary>
      /// Initialzies a new instance of <see cref="AbstractReaderStringStreamHandler"/> with given stream and string encoding.
      /// </summary>
      /// <param name="stream">The stream, as <see cref="StreamHelper"/>.</param>
      /// <param name="startPosition">The position in <paramref name="stream"/> where the contents of this meta data stream start.</param>
      /// <param name="streamSize">The size, in bytes, of the contents of this meta data stream.</param>
      /// <param name="encoding">The <see cref="Encoding"/> for the strings.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="stream"/> or <paramref name="encoding"/> is <c>null</c>.</exception>
      public AbstractReaderStringStreamHandler(
         StreamHelper stream,
         Int64 startPosition,
         Int32 streamSize,
         Encoding encoding
         )
         : base( stream, startPosition, streamSize )
      {
         ArgumentValidator.ValidateNotNull( "Encoding", encoding );

         this.Encoding = encoding;
      }

      /// <summary>
      /// This method implements <see cref="ReaderStringStreamHandler.GetString"/> by calling <see cref="AbstractReaderStreamHandlerWithArrayAndCache{TValue}.GetOrAddValue"/>.
      /// </summary>
      /// <param name="heapIndex">The index of the string in this meta data stream.</param>
      /// <returns>Deserialized string, or <c>null</c>.</returns>
      public String GetString( Int32 heapIndex )
      {
         return this.GetOrAddValue( heapIndex );
      }

      /// <summary>
      /// Leaves the actual implementation of <see cref="AbstractStringStreamHandler.StringStreamKind"/> to subclasses.
      /// </summary>
      /// <value>The <see cref="Defaults.StringStreamKind"/> of this <see cref="AbstractReaderStringStreamHandler"/>.</value>
      public abstract StringStreamKind StringStreamKind { get; }

      /// <summary>
      /// Gets the <see cref="System.Text.Encoding"/> to be used when deserializing strings.
      /// </summary>
      /// <value>The <see cref="System.Text.Encoding"/> to be used when deserializing strings.</value>
      protected Encoding Encoding { get; }
   }

   /// <summary>
   /// This class subclasses the <see cref="AbstractReaderStreamHandlerWithArrayAndCache{TValue}"/> and implements <see cref="ReaderGUIDStreamHandler"/>, providing full implementation for <c>"#GUID"</c> stream.
   /// </summary>
   public class DefaultReaderGUIDStreamHandler : AbstractReaderStreamHandlerWithArrayAndCache<Guid?>, ReaderGUIDStreamHandler
   {
      /// <summary>
      /// Creates a new instance of <see cref="DefaultReaderGUIDStreamHandler"/> with given stream.
      /// </summary>
      /// <param name="stream">The stream, as <see cref="StreamHelper"/>.</param>
      /// <param name="startPosition">The position in <paramref name="stream"/> where the contents of this meta data stream start.</param>
      /// <param name="streamSize">The size, in bytes, of the contents of this meta data stream.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="stream"/> is <c>null</c>.</exception>
      public DefaultReaderGUIDStreamHandler(
         StreamHelper stream,
         Int64 startPosition,
         Int32 streamSize
         )
         : base( stream, startPosition, streamSize )
      {
      }

      /// <summary>
      /// Implements the <see cref="AbstractMetaDataStreamHandler.StreamName"/> by returning <c>"#GUID"</c>.
      /// </summary>
      /// <value>The <c>"#GUID"</c>.</value>
      public override String StreamName
      {
         get
         {
            return MetaDataConstants.GUID_STREAM_NAME;
         }
      }

      /// <summary>
      /// This method implements <see cref="ReaderGUIDStreamHandler.GetGUID"/> by calling <see cref="AbstractReaderStreamHandlerWithArrayAndCache{TValue}.GetOrAddValue"/>.
      /// </summary>
      /// <param name="heapIndex">The index of the <see cref="Guid"/> in this meta data stream.</param>
      /// <returns>Deserialized <see cref="Guid"/>, or <c>null</c>.</returns>
      public Guid? GetGUID( Int32 heapIndex )
      {
         return this.GetOrAddValue( heapIndex );
      }

      /// <summary>
      /// This methid implements <see cref="AbstractReaderStreamHandlerWithArrayAndCache{TValue}.ValueFactory"/> by creating <see cref="Guid"/> using <see cref="Guid(byte[])"/> constructor.
      /// </summary>
      /// <param name="heapOffset">The index of the <see cref="Guid"/> in this meta data stream.</param>
      /// <returns>Deserialized <see cref="Guid"/>, or <c>null</c>.</returns>
      protected override Guid? ValueFactory( Int32 heapOffset )
      {
         return new Guid( this.Bytes.CreateArrayCopy( heapOffset - 1, MetaDataConstants.GUID_SIZE ) );
      }

      /// <summary>
      /// Checks whether the given heap offset is small enough to leave room to reading whole <see cref="Guid"/>.
      /// </summary>
      /// <param name="heapOffset">The given heap offset.</param>
      /// <returns><c>true</c> if heap offset is valid; <c>false</c> otherwise.</returns>
      protected override Boolean CheckHeapOffset( Int32 heapOffset )
      {
         return (UInt32) heapOffset <= this.StreamSizeU32 - MetaDataConstants.GUID_SIZE + 1;
      }
   }

   /// <summary>
   /// This class subclasses <see cref="AbstractReaderStringStreamHandler"/> and provides full implementation for <c>"#Strings"</c> stream.
   /// </summary>
   public class DefaultReaderSystemStringStreamHandler : AbstractReaderStringStreamHandler
   {
      /// <summary>
      /// Creates a new instance of <see cref="DefaultReaderSystemStringStreamHandler"/> with given stream.
      /// </summary>
      /// <param name="stream">The stream, as <see cref="StreamHelper"/>.</param>
      /// <param name="startPosition">The position in <paramref name="stream"/> where the contents of this meta data stream start.</param>
      /// <param name="streamSize">The size, in bytes, of the contents of this meta data stream.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="stream"/> is <c>null</c>.</exception>
      public DefaultReaderSystemStringStreamHandler(
         StreamHelper stream,
         Int64 startPosition,
         Int32 streamSize
         )
         : base( stream, startPosition, streamSize, MetaDataConstants.SYS_STRING_ENCODING )
      {

      }

      /// <summary>
      /// Implements the <see cref="AbstractMetaDataStreamHandler.StreamName"/> by returning <c>"#Strings"</c>.
      /// </summary>
      /// <value>The <c>"#Strings"</c>.</value>
      public override String StreamName
      {
         get
         {
            return MetaDataConstants.SYS_STRING_STREAM_NAME;
         }
      }

      /// <summary>
      /// Implements the <see cref="AbstractReaderStringStreamHandler.StringStreamKind"/> by returning <see cref="StringStreamKind.SystemStrings"/>.
      /// </summary>
      /// <value>The <see cref="StringStreamKind.SystemStrings"/>.</value>
      public override StringStreamKind StringStreamKind
      {
         get
         {
            return StringStreamKind.SystemStrings;
         }
      }

      /// <summary>
      /// Implements the <see cref="AbstractReaderStreamHandlerWithArrayAndCache{TValue}.ValueFactory"/> by deserializing string as zero-terminated UTF-8 -encoded string.
      /// </summary>
      /// <param name="heapIndex">The index of the string in this meta data stream.</param>
      /// <returns>Deserialized string.</returns>
      protected override String ValueFactory( Int32 heapIndex )
      {
         var start = heapIndex;
         var array = this.Bytes;
         while ( array[heapIndex] != 0 )
         {
            ++heapIndex;
         }
         return heapIndex == start ?
            String.Empty :
            this.Encoding.GetString( this.Bytes, start, heapIndex - start );
      }
   }

   /// <summary>
   /// This class subclasses <see cref="AbstractReaderStringStreamHandler"/> and provides full implementation for <c>"#US"</c> stream.
   /// </summary>
   public class DefaultReaderUserStringsStreamHandler : AbstractReaderStringStreamHandler
   {
      /// <summary>
      /// Creates a new instance of <see cref="DefaultReaderUserStringsStreamHandler"/> with given stream.
      /// </summary>
      /// <param name="stream">The stream, as <see cref="StreamHelper"/>.</param>
      /// <param name="startPosition">The position in <paramref name="stream"/> where the contents of this meta data stream start.</param>
      /// <param name="streamSize">The size, in bytes, of the contents of this meta data stream.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="stream"/> is <c>null</c>.</exception>
      public DefaultReaderUserStringsStreamHandler(
         StreamHelper stream,
         Int64 startPosition,
         Int32 streamSize
         )
         : base( stream, startPosition, streamSize, MetaDataConstants.USER_STRING_ENCODING )
      {

      }

      /// <summary>
      /// Implements the <see cref="AbstractMetaDataStreamHandler.StreamName"/> by returning <c>"#US"</c>.
      /// </summary>
      /// <value>The <c>"#US"</c>.</value>
      public override String StreamName
      {
         get
         {
            return MetaDataConstants.USER_STRING_STREAM_NAME;
         }
      }

      /// <summary>
      /// Implements the <see cref="AbstractReaderStringStreamHandler.StringStreamKind"/> by returning <see cref="StringStreamKind.UserStrings"/>.
      /// </summary>
      /// <value>The <see cref="StringStreamKind.UserStrings"/>.</value>
      public override StringStreamKind StringStreamKind
      {
         get
         {
            return StringStreamKind.UserStrings;
         }
      }

      /// <summary>
      /// Implements the <see cref="AbstractReaderStreamHandlerWithArrayAndCache{TValue}.ValueFactory"/> by deserializing string as length-prefixed UTF-16 -encoded string.
      /// </summary>
      /// <param name="heapIndex">The index of the string in this meta data stream.</param>
      /// <returns>Deserialized string, or <c>null</c>.</returns>
      protected override String ValueFactory( Int32 heapIndex )
      {
         var array = this.Bytes;
         String retVal;
         Int32 length;
         if ( array.TryDecompressUInt32( ref heapIndex, array.Length, out length ) && heapIndex <= array.Length - length )
         {
            if ( length > 1 )
            {
               retVal = this.Encoding.GetString( array, heapIndex, length - 1 );
            }
            else
            {
               retVal = "";
            }
         }
         else
         {
            retVal = null;
         }

         return retVal;
      }

      /// <summary>
      /// Returns empty string, as user strings can not have <c>null</c>.
      /// </summary>
      /// <returns>Empty string.</returns>
      protected override String ZeroValue()
      {
         return "";
      }
   }

   /// <summary>
   /// This class subclasses <see cref="AbstractReaderStreamHandlerWithArrayAndCache{TValue}"/> and provides full implementation for <c>"#Blob"</c> stream.
   /// </summary>
   public class DefaultReaderBLOBStreamHandler : AbstractReaderStreamHandlerWithArrayAndCache<Tuple<Int32, Int32>>, ReaderBLOBStreamHandler
   {
      private readonly IDictionary<KeyValuePair<Int32, ConstantValueType>, Object> _constants;

      /// <summary>
      /// Creates a new instance of <see cref="DefaultReaderBLOBStreamHandler"/> with given stream.
      /// </summary>
      /// <param name="stream">The stream, as <see cref="StreamHelper"/>.</param>
      /// <param name="startPosition">The position in <paramref name="stream"/> where the contents of this meta data stream start.</param>
      /// <param name="streamSize">The size, in bytes, of the contents of this meta data stream.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="stream"/> is <c>null</c>.</exception>
      public DefaultReaderBLOBStreamHandler(
         StreamHelper stream,
         Int64 startPosition,
         Int32 streamSize
         )
         : base( stream, startPosition, streamSize )
      {
         this._constants = new Dictionary<KeyValuePair<Int32, ConstantValueType>, Object>();
      }

      /// <summary>
      /// Implements the <see cref="AbstractMetaDataStreamHandler.StreamName"/> by returning <c>"#Blob"</c>.
      /// </summary>
      /// <value>The <c>"#Blob"</c>.</value>
      public override String StreamName
      {
         get
         {
            return MetaDataConstants.BLOB_STREAM_NAME;
         }
      }

      /// <summary>
      /// Implements the <see cref="ReaderBLOBStreamHandler.GetBLOBByteArray"/>.
      /// </summary>
      /// <param name="heapIndex">The index to this meta data stream.</param>
      /// <returns>A new byte array, or <c>null</c>.</returns>
      public Byte[] GetBLOBByteArray( Int32 heapIndex )
      {
         Int32 len;
         return this.SetUpBLOBWithLength( ref heapIndex, out len ) ? this.Bytes.CreateArrayCopy( heapIndex, len ) : null;
      }

      /// <summary>
      /// Implements the <see cref="ReaderBLOBStreamHandler.ReadCASignature"/>.
      /// </summary>
      /// <param name="heapIndex">The index to this meta data stream.</param>
      /// <param name="sigProvider">The <see cref="SignatureProvider"/>.</param>
      /// <returns>A new <see cref="RawCustomAttributeSignature"/> or <see cref="ResolvedCustomAttributeSignature"/>, or <c>null</c>.</returns>
      /// <remarks>
      /// Because reading custom attribute signatures may require resolving, the only case when this method does returns an instance of <see cref="ResolvedCustomAttributeSignature"/> is when the signature BLOB represents empty custom attribute signature.
      /// Whenever there is a non-empty signature, then an instance of <see cref="RawCustomAttributeSignature"/> is returned.
      /// </remarks>
      public AbstractCustomAttributeSignature ReadCASignature( Int32 heapIndex, SignatureProvider sigProvider )
      {
         AbstractCustomAttributeSignature caSig;
         Int32 blobSize;
         if ( this.SetUpBLOBWithLength( ref heapIndex, out blobSize ) )
         {
            if ( blobSize <= 2 )
            {
               // Empty blob
               caSig = new ResolvedCustomAttributeSignature();
            }
            else
            {
               caSig = new RawCustomAttributeSignature()
               {
                  Bytes = this.Bytes.CreateArrayCopy( heapIndex, blobSize )
               };
            }
         }
         else
         {
            caSig = null;
         }
         return caSig;
      }

      /// <summary>
      /// Implements the <see cref="ReaderBLOBStreamHandler.ReadConstantValue"/>.
      /// </summary>
      /// <param name="heapIndex">The index to this meta data stream.</param>
      /// <param name="sigProvider">The <see cref="SignatureProvider"/>.</param>
      /// <param name="constType">The kind of the constant.</param>
      /// <returns>A constant value, possibly <c>null</c>.</returns>
      public Object ReadConstantValue( Int32 heapIndex, SignatureProvider sigProvider, ConstantValueType constType )
      {
         return heapIndex == 0 || (UInt32) heapIndex >= this.StreamSizeU32 ?
            null :
            this._constants.GetOrAdd_NotThreadSafe(
               new KeyValuePair<Int32, ConstantValueType>( heapIndex, constType ),
               kvp => this.DoReadConstantValue( kvp.Key, kvp.Value )
               );
      }

      /// <summary>
      /// Implements the <see cref="ReaderBLOBStreamHandler.ReadMarshalingInfo"/>.
      /// </summary>
      /// <param name="heapIndex">The index to this meta data stream.</param>
      /// <param name="sigProvider">The <see cref="SignatureProvider"/>.</param>
      /// <returns>A new instance of <see cref="AbstractMarshalingInfo"/>, or <c>null</c>.</returns>
      public AbstractMarshalingInfo ReadMarshalingInfo( Int32 heapIndex, SignatureProvider sigProvider )
      {
         Int32 max;
         return this.SetUpBLOBWithMax( ref heapIndex, out max ) ? sigProvider.ReadMarshalingInfo( this.Bytes, ref heapIndex, max ) : null;
      }

      /// <summary>
      /// Implements the <see cref="ReaderBLOBStreamHandler.ReadNonTypeSignature"/>.
      /// </summary>
      /// <param name="streamIndex">The zero-based stream index.</param>
      /// <param name="sigProvider">The <see cref="SignatureProvider"/> to use when deserializing the signature.</param>
      /// <param name="methodSigIsDefinition">Whether the method signature, if any, should be a <see cref="MethodDefinitionSignature"/>.</param>
      /// <param name="handleFieldSigAsLocalsSig">Whether to handle the field signature as <see cref="LocalVariablesSignature"/>.</param>
      /// <param name="fieldSigTransformedToLocalsSig">If <paramref name="handleFieldSigAsLocalsSig"/> is <c>true</c> and the signature started with field prefix, this will be <c>true</c>. Otherwise, this will be <c>false</c>.</param>
      /// <returns>A new instance of <see cref="AbstractSignature"/>, or <c>null</c>.</returns>
      public AbstractSignature ReadNonTypeSignature( Int32 heapIndex, SignatureProvider sigProvider, bool methodSigIsDefinition, bool handleFieldSigAsLocalsSig, out bool fieldSigTransformedToLocalsSig )
      {
         Int32 max;
         AbstractSignature retVal;
         if ( this.SetUpBLOBWithMax( ref heapIndex, out max ) )
         {
            retVal = sigProvider.ReadNonTypeSignature(
               this.Bytes,
               ref heapIndex,
               max,
               methodSigIsDefinition,
               handleFieldSigAsLocalsSig,
               out fieldSigTransformedToLocalsSig
               );
         }
         else
         {
            fieldSigTransformedToLocalsSig = false;
            retVal = null;
         }

         return retVal;
      }

      /// <summary>
      /// Implements the <see cref="ReaderBLOBStreamHandler.ReadSecurityInformation"/>.
      /// </summary>
      /// <param name="streamIndex">The zero-based stream index.</param>
      /// <param name="sigProvider">The <see cref="SignatureProvider"/> to use when deserializing the signature.</param>
      /// <param name="securityInfo">The list of <see cref="AbstractSecurityInformation"/>s to populate.</param>
      public void ReadSecurityInformation( Int32 heapIndex, SignatureProvider sigProvider, List<AbstractSecurityInformation> securityInfo )
      {
         Int32 max;
         if ( this.SetUpBLOBWithMax( ref heapIndex, out max ) )
         {
            sigProvider.ReadSecurityInformation( this.Bytes, ref heapIndex, max, securityInfo );
         }
      }

      /// <summary>
      /// Implements the <see cref="ReaderBLOBStreamHandler.ReadTypeSignature"/>.
      /// </summary>
      /// <param name="streamIndex">The zero-based stream index.</param>
      /// <param name="sigProvider">The <see cref="SignatureProvider"/> to use when deserializing the signature.</param>
      /// <returns>A new instance of <see cref="TypeSignature"/>, or <c>null</c>.</returns>
      public TypeSignature ReadTypeSignature( Int32 heapIndex, SignatureProvider sigProvider )
      {
         Int32 max;
         return this.SetUpBLOBWithMax( ref heapIndex, out max ) ? sigProvider.ReadTypeSignature( this.Bytes, ref heapIndex, max ) : null;
      }

      /// <summary>
      /// This method is used to set up reading the BLOB from <see cref="AbstractReaderStreamHandlerWithArray.Bytes"/>.
      /// </summary>
      /// <param name="heapIndex">The given meta data stream index. This will be modified so that the BLOB can be read from <see cref="AbstractReaderStreamHandlerWithArray.Bytes"/> at this index.</param>
      /// <param name="max">The exclusive max value for the BLOB.</param>
      /// <returns><c>true</c> if BLOB length was read successfully and it is ok to read; <c>false</c> otherwise.</returns>
      protected Boolean SetUpBLOBWithMax( ref Int32 heapIndex, out Int32 max )
      {
         var retVal = this.SetUpBLOBWithLength( ref heapIndex, out max );
         if ( retVal )
         {
            max += heapIndex;
         }
         return retVal;
      }

      /// <summary>
      /// This method is used to set up reading the BLOB from <see cref="AbstractReaderStreamHandlerWithArray.Bytes"/>.
      /// </summary>
      /// <param name="heapIndex">The given meta data stream index. This will be modified so that the BLOB can be read from <see cref="AbstractReaderStreamHandlerWithArray.Bytes"/> at this index.</param>
      /// <param name="length">The length of BLOB, in bytes.</param>
      /// <returns><c>true</c> if BLOB length was read successfully and it is ok to read; <c>false</c> otherwise.</returns>
      protected Boolean SetUpBLOBWithLength( ref Int32 heapIndex, out Int32 length )
      {
         if ( heapIndex > 0 )
         {
            var tuple = this.GetOrAddValue( heapIndex );

            heapIndex = tuple?.Item1 ?? heapIndex;
            length = tuple?.Item2 ?? 0;
            return tuple != null;
         }
         else
         {
            length = 0;
            return false;
         }
      }

      /// <summary>
      /// This method implements <see cref="AbstractReaderStreamHandlerWithArrayAndCache{TValue}.ValueFactory"/> so that it caches the start index and length of the BLOB at given meta data stream index.
      /// </summary>
      /// <param name="heapOffset">The meta data stream index.</param>
      /// <returns>Tuple with start index and length, or <c>null</c>.</returns>
      protected override Tuple<Int32, Int32> ValueFactory( int heapOffset )
      {
         var array = this.Bytes;
         Int32 length;
         return array.TryDecompressUInt32( ref heapOffset, array.Length, out length ) && (UInt32) heapOffset + (UInt32) length <= this.StreamSizeU32 ?
            Tuple.Create( heapOffset, length ) :
            null;
      }

      private Object DoReadConstantValue( Int32 heapIndex, ConstantValueType constType )
      {

         Object retVal;
         Int32 blobSize;
         if ( this.SetUpBLOBWithLength( ref heapIndex, out blobSize ) )
         {
            var array = this.Bytes;
            switch ( constType )
            {
               case ConstantValueType.Boolean:
                  return blobSize >= 1 ? (Object) ( array[heapIndex] == 1 ) : null;
               case ConstantValueType.Char:
                  return blobSize >= 2 ? (Object) Convert.ToChar( array.ReadUInt16LEFromBytes( ref heapIndex ) ) : null;
               case ConstantValueType.I1:
                  return blobSize >= 1 ? (Object) array.ReadSByteFromBytes( ref heapIndex ) : null;
               case ConstantValueType.U1:
                  return blobSize >= 1 ? (Object) array.ReadByteFromBytes( ref heapIndex ) : null;
               case ConstantValueType.I2:
                  return blobSize >= 2 ? (Object) array.ReadInt16LEFromBytes( ref heapIndex ) : null;
               case ConstantValueType.U2:
                  return blobSize >= 2 ? (Object) array.ReadUInt16LEFromBytes( ref heapIndex ) : null;
               case ConstantValueType.I4:
                  return blobSize >= 4 ? (Object) array.ReadInt32LEFromBytes( ref heapIndex ) : null;
               case ConstantValueType.U4:
                  return blobSize >= 4 ? (Object) array.ReadUInt32LEFromBytes( ref heapIndex ) : null;
               case ConstantValueType.I8:
                  return blobSize >= 8 ? (Object) array.ReadInt64LEFromBytes( ref heapIndex ) : null;
               case ConstantValueType.U8:
                  return blobSize >= 8 ? (Object) array.ReadUInt64LEFromBytes( ref heapIndex ) : null;
               case ConstantValueType.R4:
                  return blobSize >= 4 ? (Object) array.ReadSingleLEFromBytes( ref heapIndex ) : null;
               case ConstantValueType.R8:
                  return blobSize >= 8 ? (Object) array.ReadDoubleLEFromBytes( ref heapIndex ) : null;
               case ConstantValueType.String:
                  return MetaDataConstants.USER_STRING_ENCODING.GetString( array, heapIndex, blobSize );
               default:
                  return null;
            }
         }
         else
         {
            retVal = null;
         }

         return retVal;
      }
   }
}