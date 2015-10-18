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
using CILAssemblyManipulator.Physical.Implementation;
using CILAssemblyManipulator.Physical.IO;
using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CILAssemblyManipulator.Physical.IO
{
   public class DefaultReaderFunctionalityProvider : ReaderFunctionalityProvider
   {
      public virtual ReaderFunctionality GetFunctionality( Stream stream, out Stream newStream )
      {
         throw new NotImplementedException();
      }
   }

   public class DefaultReaderFunctionality : ReaderFunctionality
   {
      protected const Int32 CLI_DATADIR_INDEX = 14;

      public void ReadImageInformation(
         StreamHelper stream,
         out PEInformation peInfo,
         out RVAConverter rvaConverter,
         out CLIHeader cliHeader,
         out MetaDataRoot mdRoot
         )
      {
         // Read PE info
         peInfo = stream.NewPEImageInformationFromStream();

         var dataDirs = peInfo.NTHeader.OptionalHeader.DataDirectories;

         if ( dataDirs.Count <= CLI_DATADIR_INDEX )
         {
            throw new BadImageFormatException( "No data directory for CLI header." );
         }

         // Create RVA converter
         rvaConverter = this.CreateRVAConverter( peInfo ) ?? new DefaultRVAConverter( peInfo );

         // Read CLI header
         cliHeader = stream
            .GoToRVA( rvaConverter, dataDirs[CLI_DATADIR_INDEX].RVA )
            .NewCLIHeaderFromStream();

         // Read MD root
         mdRoot = stream
            .GoToRVA( rvaConverter, cliHeader.MetaData.RVA )
            .NewMetaDataRootFromStream();
      }

      public virtual AbstractReaderStreamHandler CreateStreamHandler(
         StreamHelper stream,
         MetaDataStreamHeader header
         )
      {
         throw new NotImplementedException();
      }
      public virtual ReaderRVAHandler CreateRVAHandler(
         StreamHelper stream,
         ImageInformation imageInfo,
         RVAConverter rvaConverter,
         CILMetaData md
         )
      {
         return new DefaultReaderRVAHandler( stream, imageInfo, rvaConverter, md );
      }

      protected virtual RVAConverter CreateRVAConverter(
         PEInformation peInformation
         )
      {
         return new DefaultRVAConverter( peInformation );
      }

   }

   public class DefaultRVAConverter : RVAConverter
   {
      private readonly SectionHeader[] _sections;

      public DefaultRVAConverter( PEInformation peInfo )
      {
         ArgumentValidator.ValidateNotNull( "PE information", peInfo );

         this._sections = peInfo.SectionHeaders.ToArray();
      }

      public Int64 ToOffset( Int64 rva )
      {
         // TODO some kind of interval-map for sections...
         var sections = this._sections;
         var retVal = -1L;
         for ( var i = 0; i < sections.Length; ++i )
         {
            var sec = sections[i];
            if ( sec.VirtualAddress <= rva && rva < (Int64) sec.VirtualAddress + (Int64) Math.Max( sec.VirtualSize, sec.RawDataSize ) )
            {
               retVal = sec.RawDataPointer + ( rva - sec.VirtualAddress );
               break;
            }
         }

         return retVal;
      }

      public Int64 ToRVA( Int64 offset )
      {
         // TODO some kind of interval-map for sections...
         var sections = this._sections;
         var retVal = -1L;
         for ( var i = 0; i < sections.Length; ++i )
         {
            var sec = sections[i];
            if ( sec.RawDataPointer <= offset && offset < (Int64) sec.RawDataPointer + (Int64) sec.RawDataSize )
            {
               retVal = sec.VirtualAddress + ( offset - sec.RawDataPointer );
               break;
            }
         }

         return retVal;
      }
   }

   public abstract class AbstractReaderStreamHandlerImpl : AbstractReaderStreamHandler
   {

      protected AbstractReaderStreamHandlerImpl( StreamHelper stream, Int32 streamSize )
      {
         ArgumentValidator.ValidateNotNull( "Stream", stream );

         this.Stream = stream;
         this.StreamSize = (UInt32) streamSize;
         this.StartingPosition = stream.Stream.Position;
      }

      public abstract String StreamName { get; }

      protected StreamHelper Stream { get; }

      protected Int64 StreamSize { get; }

      protected Int64 StartingPosition { get; }

      protected StreamHelper SetStreamToHeapOffset( Int32 heapOffset )
      {
         var stream = this.Stream;
         stream.Stream.Position = this.StartingPosition + (UInt32) heapOffset;
         return stream;
      }

      protected Boolean CheckHeapOffset( Int32 heapOffset )
      {
         return ( (UInt32) heapOffset ) < this.StreamSize;
      }
   }

   public abstract class AbstractReaderStreamHandlerWithCustomName : AbstractReaderStreamHandlerImpl
   {
      protected AbstractReaderStreamHandlerWithCustomName( StreamHelper stream, Int32 streamSize, String streamName )
         : base( stream, streamSize )
      {
         this.StreamName = streamName;
      }

      public override String StreamName { get; }
   }

   public class DefaultReaderTableStreamHandler : AbstractReaderStreamHandlerWithCustomName, ReaderTableStreamHandler
   {
      private const Int32 TABLE_ARRAY_SIZE = 64;


      public DefaultReaderTableStreamHandler(
         StreamHelper stream,
         Int32 streamSize,
         String tableStreamName,
         MetaDataSerializationSupportProvider mdSerialization
         )
         : base( stream, streamSize, tableStreamName )
      {
         var tableHeader = stream.NewTableStreamHeaderFromStream();
         var thFlags = tableHeader.TableStreamFlags;

         var tableStartPosition = stream.Stream.Position;
         this.TableStreamHeader = tableHeader;
         this.TableSizes = tableHeader.CreateTableSizesArray().ToArrayProxy().CQ;

         if ( mdSerialization == null )
         {
            mdSerialization = new DefaultMetaDataSerializationSupportProvider();
         }

         this.TableSerializationInfo =
            Enumerable.Range( 0, this.TableSizes.Count )
            .Select( table => mdSerialization.CreateTableSerializationInfo( (Tables) table ) )
            .ToArrayProxy()
            .CQ;

         this.TableSerializationSupport =
            this.TableSerializationInfo
            .Select( table => table.CreateSupport( this.TableSizes, thFlags.IsWideBLOB(), thFlags.IsWideGUID(), thFlags.IsWideStrings() ) )
            .ToArrayProxy()
            .CQ;

         this.TableWidths =
            this.TableSerializationSupport
            .Select( table => table.ColumnSerializationSupports.Aggregate( 0, ( curRowBytecount, colInfo ) => curRowBytecount + colInfo.ColumnByteCount ) )
            .ToArrayProxy()
            .CQ;

         this.TableStartOffsets =
            this.TableSizes
            .Select( ( size, idx ) => Tuple.Create( size, idx ) )
            .AggregateIntermediate( tableStartPosition, ( curOffset, tuple ) => curOffset + tuple.Item1 * this.TableWidths[tuple.Item2] )
            .ToArrayProxy()
            .CQ;

      }

      public virtual void PopulateMetaDataStructure(
         CILMetaData md,
         ReaderBLOBStreamHandler blobs,
         ReaderGUIDStreamHandler guids,
         ReaderStringStreamHandler sysStrings,
         ReaderStringStreamHandler userStrings,
         IEnumerable<AbstractReaderStreamHandler> otherStreams,
         List<Int32> methodRVAs,
         List<Int32> fieldRVAs
         )
      {
         var args = new RowReadingArguments( this.Stream, blobs, guids, sysStrings, methodRVAs, fieldRVAs );
         for ( var i = 0; i < this.TableSizes.Count; ++i )
         {
            var table = md.GetByTable( (Tables) i );
            var tableSize = this.TableSizes[i];
            for ( var j = 0; j < tableSize; ++j )
            {
               table.TryAddRow( this.TableSerializationSupport[i].ReadRow( args ) );
            }
         }

      }

      public virtual Object GetRawRowOrNull( Tables table, Int32 idx )
      {
         var tableSizes = this.TableSizes;
         var tableInt = (Int32) table;
         Object retVal;
         if ( tableInt >= 0
            && tableInt < tableSizes.Count
            && idx >= 0
            && tableSizes[tableInt] > idx
            )
         {
            var offset = this.TableStartOffsets[tableInt] + idx * (Int64) this.TableWidths[tableInt];
            var stream = this.Stream;
            stream.Stream.Position = offset;
            retVal = this.TableSerializationSupport[tableInt].ReadRawRow( stream );
         }
         else
         {
            retVal = null;
         }
         return retVal;
      }

      public virtual MetaDataTableStreamHeader ReadHeader()
      {
         return this.TableStreamHeader;
      }

      protected MetaDataTableStreamHeader TableStreamHeader { get; }

      protected ArrayQuery<Int32> TableSizes { get; }

      protected ArrayQuery<Int32> TableWidths { get; }

      protected ArrayQuery<Int64> TableStartOffsets { get; }

      protected ArrayQuery<TableSerializationInfo> TableSerializationInfo { get; }

      protected ArrayQuery<TableSerializationSupport> TableSerializationSupport { get; }

   }

   public abstract class AbstractReaderStreamHandlerImplWithCache<TValue> : AbstractReaderStreamHandlerImpl
   {
      private readonly IDictionary<Int32, TValue> _cache;

      public AbstractReaderStreamHandlerImplWithCache(
         StreamHelper stream,
         Int32 streamSize
         )
         : base( stream, streamSize )
      {
         this._cache = new Dictionary<Int32, TValue>();
      }

      protected TValue GetOrAddValue( Int32 heapOffset )
      {
         return this.CheckHeapOffset( heapOffset ) ?
            this._cache.GetOrAdd_NotThreadSafe( heapOffset, this.ValueFactoryWrapper ) :
            (TValue) (Object) null;
      }

      protected TValue GetValueNoCache( Int32 heapOffset )
      {
         return this.CheckHeapOffset( heapOffset ) ?
            this.ValueFactoryWrapper( heapOffset ) :
            (TValue) (Object) null;
      }

      private TValue ValueFactoryWrapper( Int32 heapOffset )
      {

         try
         {
            return this.ValueFactory( heapOffset );
         }
         catch
         {
            return (TValue) (Object) null;
         }

      }

      protected abstract TValue ValueFactory( Int32 heapOffset );
   }

   public class DefaultReaderGUIDStringStreamHandler : AbstractReaderStreamHandlerImplWithCache<Guid?>, ReaderGUIDStreamHandler
   {
      public DefaultReaderGUIDStringStreamHandler(
         StreamHelper stream,
         Int32 streamSize
         )
         : base( stream, streamSize )
      {
      }

      public override String StreamName
      {
         get
         {
            return MetaDataConstants.GUID_STREAM_NAME;
         }
      }

      public Guid? GetGUID( Int32 heapIndex )
      {
         return this.GetOrAddValue( heapIndex );
      }

      public Guid? GetGUIDNoCache( Int32 heapIndex )
      {
         return this.GetValueNoCache( heapIndex );
      }

      protected override Guid? ValueFactory( Int32 heapOffset )
      {
         if ( heapOffset == 0 || (UInt32) heapOffset >= this.StreamSize - MetaDataConstants.GUID_SIZE )
         {
            return null;
         }
         else
         {
            return new Guid( this.SetStreamToHeapOffset( heapOffset ).ReadAndCreateArray( MetaDataConstants.GUID_SIZE ) );
         }
      }
   }

   public abstract class AbstractReaderStringStreamHandler : AbstractReaderStreamHandlerImplWithCache<String>, ReaderStringStreamHandler
   {
      public AbstractReaderStringStreamHandler(
         StreamHelper stream,
         Int32 streamSize,
         Encoding encoding
         )
         : base( stream, streamSize )
      {
         ArgumentValidator.ValidateNotNull( "Encoding", encoding );

         this.Encoding = encoding;
      }

      public String GetString( Int32 heapIndex )
      {
         return this.GetOrAddValue( heapIndex );
      }

      public String GetStringNoCache( Int32 heapIndex )
      {
         return this.GetValueNoCache( heapIndex );
      }

      protected Encoding Encoding { get; }
   }

   public class DefaultReaderSystemStringStreamHandler : AbstractReaderStringStreamHandler
   {
      public DefaultReaderSystemStringStreamHandler(
         StreamHelper stream,
         Int32 streamSize
         )
         : base( stream, streamSize, MetaDataConstants.SYS_STRING_ENCODING )
      {

      }

      public override String StreamName
      {
         get
         {
            return MetaDataConstants.SYS_STRING_STREAM_NAME;
         }
      }

      protected override String ValueFactory( Int32 heapIndex )
      {
         return this.Encoding.GetString( this.SetStreamToHeapOffset( heapIndex ).ReadUntilZeroAndCreateArray() );
      }
   }

   public class DefaultReaderUserStringsStreamHandler : AbstractReaderStringStreamHandler
   {
      public DefaultReaderUserStringsStreamHandler(
         StreamHelper stream,
         Int32 streamSize
         )
         : base( stream, streamSize, MetaDataConstants.USER_STRING_ENCODING )
      {

      }

      public override String StreamName
      {
         get
         {
            return MetaDataConstants.USER_STRING_STREAM_NAME;
         }
      }

      protected override String ValueFactory( Int32 heapIndex )
      {
         Int32 length;
         String retVal;
         if ( this.SetStreamToHeapOffset( heapIndex ).DecompressUInt32( out length ) && this.Stream.Stream.CanReadNextBytes( length ).IsTrue() )
         {
            retVal = this.Encoding.GetString( this.Stream.ReadAndCreateArray( length ) );
         }
         else
         {
            retVal = null;
         }

         return retVal;
      }
   }

   public class DefaultReaderRVAHandler : ReaderRVAHandler
   {
      public DefaultReaderRVAHandler(
         StreamHelper stream,
         ImageInformation imageInfo,
         RVAConverter rvaConverter,
         CILMetaData md
         )
      {
         ArgumentValidator.ValidateNotNull( "Stream", stream );
         ArgumentValidator.ValidateNotNull( "Image information", imageInfo );
         ArgumentValidator.ValidateNotNull( "RVA converter", rvaConverter );
         ArgumentValidator.ValidateNotNull( "Meta data", md );

         this.Stream = stream;
         this.ImageInfo = imageInfo;
         this.RVAConverter = rvaConverter;
         this.MetaData = md;
      }

      public byte[] ReadConstantValue( int fieldIndex )
      {
         throw new NotImplementedException();
      }

      public byte[] ReadEmbeddedManifestResource( int manifestIndex )
      {
         throw new NotImplementedException();
      }

      public MethodILDefinition ReadIL( int methodIndex )
      {
         throw new NotImplementedException();
      }

      protected StreamHelper Stream { get; }

      protected ImageInformation ImageInfo { get; }

      protected RVAConverter RVAConverter { get; }

      protected CILMetaData MetaData { get; }
   }
}

public static partial class E_CILPhysical
{
   private const Int32 TABLE_ARRAY_SIZE = 64;

   public static Int32[] CreateTableSizesArray( this MetaDataTableStreamHeader tableStreamHeader )
   {
      var tableSizes = new Int32[TABLE_ARRAY_SIZE];
      var present = tableStreamHeader.PresentTablesBitVector;
      var sizeIdx = 0;
      for ( var i = 0; i < TABLE_ARRAY_SIZE; ++i )
      {
         if ( ( ( present >> i ) & 0x1 ) != 0 )
         {
            tableSizes[i] = (Int32) tableStreamHeader.TableSizes[sizeIdx++];
         }
      }

      return tableSizes;
   }
}