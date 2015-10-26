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
         // We are going to do a lot of seeking, so just read whole stream into byte array and use memory stream
         newStream = new MemoryStream( stream.ReadUntilTheEnd(), this.IsMemoryStreamWriteable );
         return new DefaultReaderFunctionality( this.CreateMDSerialization() );
      }

      protected virtual Boolean IsMemoryStreamWriteable
      {
         get
         {
            return false;
         }
      }

      protected virtual MetaDataSerializationSupportProvider CreateMDSerialization()
      {
         return DefaultMetaDataSerializationSupportProvider.Instance;
      }
   }

   public class DefaultReaderFunctionality : ReaderFunctionality
   {

      public DefaultReaderFunctionality(
         MetaDataSerializationSupportProvider mdSerialization = null
         )
      {
         this.MDSerialization = mdSerialization ?? new DefaultMetaDataSerializationSupportProvider();
         this.TableSerializations = this.MDSerialization.CreateTableSerializationInfos().ToArrayProxy().CQ;
      }

      public virtual void ReadImageInformation(
         StreamHelper stream,
         out PEInformation peInfo,
         out RVAConverter rvaConverter,
         out CLIHeader cliHeader,
         out MetaDataRoot mdRoot
         )
      {
         // Read PE info
         peInfo = stream.NewPEImageInformationFromStream();

         // Create RVA converter
         rvaConverter = this.CreateRVAConverter( peInfo ) ?? new DefaultRVAConverter( peInfo );

         var dataDirs = peInfo.NTHeader.OptionalHeader.DataDirectories;

         var cliDataDirIndex = (Int32) DataDirectories.CLIHeader;

         if ( cliDataDirIndex < dataDirs.Count )
         {
            // Read CLI header
            cliHeader = stream
               .GoToRVA( rvaConverter, dataDirs[cliDataDirIndex].RVA )
               .NewCLIHeaderFromStream();

            // Read MD root
            mdRoot = stream
               .GoToRVA( rvaConverter, cliHeader.MetaData.RVA )
               .NewMetaDataRootFromStream();
         }
         else
         {
            cliHeader = null;
            mdRoot = null;
         }
      }

      public virtual AbstractReaderStreamHandler CreateStreamHandler(
         StreamHelper stream,
         MetaDataStreamHeader header
         )
      {
         var size = (Int32) header.Size;
         switch ( header.Name )
         {
            case MetaDataConstants.TABLE_STREAM_NAME:
            case "#-":
               return new DefaultReaderTableStreamHandler( stream, size, header.Name, this.TableSerializations );
            case MetaDataConstants.BLOB_STREAM_NAME:
               return new DefaultReaderBLOBStreamHandler( stream, size );
            case MetaDataConstants.GUID_STREAM_NAME:
               return new DefaultReaderGUIDStringStreamHandler( stream, size );
            case MetaDataConstants.SYS_STRING_STREAM_NAME:
               return new DefaultReaderSystemStringStreamHandler( stream, size );
            case MetaDataConstants.USER_STRING_STREAM_NAME:
               return new DefaultReaderUserStringsStreamHandler( stream, size );
            default:
               return null;
         }
      }

      public virtual void HandleStoredRawValues(
         StreamHelper stream,
         ImageInformation imageInfo,
         RVAConverter rvaConverter,
         ReaderMetaDataStreamContainer mdStreamContainer,
         CILMetaData md,
         RawValueStorage<Int32> rawValues
         )
      {
         var args = this.CreateRawValueProcessingArgs( stream, imageInfo, rvaConverter, mdStreamContainer, md ) ?? CreateDefaultRawValueProcessingArgs( stream, imageInfo, rvaConverter, mdStreamContainer, md );
         var tableSerializations = this.TableSerializations;
         foreach ( var tableSerialization in this.TableSerializations )
         {
            tableSerialization.ProcessRowForRawValues( args, rawValues );
         }
      }

      protected virtual RVAConverter CreateRVAConverter(
         PEInformation peInformation
         )
      {
         return new DefaultRVAConverter( peInformation );
      }



      protected virtual RawValueProcessingArgs CreateRawValueProcessingArgs(
         StreamHelper stream,
         ImageInformation imageInfo,
         RVAConverter rvaConverter,
         ReaderMetaDataStreamContainer mdStreamContainer,
         CILMetaData md
         )
      {
         return CreateDefaultRawValueProcessingArgs( stream, imageInfo, rvaConverter, mdStreamContainer, md );
      }

      protected static RawValueProcessingArgs CreateDefaultRawValueProcessingArgs(
         StreamHelper stream,
         ImageInformation imageInfo,
         RVAConverter rvaConverter,
         ReaderMetaDataStreamContainer mdStreamContainer,
         CILMetaData md
         )
      {
         return new RawValueProcessingArgs( stream, imageInfo, rvaConverter, mdStreamContainer, md );
      }

      protected MetaDataSerializationSupportProvider MDSerialization { get; }

      protected ArrayQuery<TableSerializationInfo> TableSerializations { get; }

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
         if ( rva > 0 )
         {
            for ( var i = 0; i < sections.Length; ++i )
            {
               var sec = sections[i];
               if ( sec.VirtualAddress <= rva && rva < (Int64) sec.VirtualAddress + (Int64) Math.Max( sec.VirtualSize, sec.RawDataSize ) )
               {
                  retVal = sec.RawDataPointer + ( rva - sec.VirtualAddress );
                  break;
               }
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
         ArrayQuery<TableSerializationInfo> tableSerializations
         )
         : base( stream, streamSize, tableStreamName )
      {
         if ( tableSerializations == null )
         {
            tableSerializations = new DefaultMetaDataSerializationSupportProvider().CreateTableSerializationInfos().ToArrayProxy().CQ;
         }

         var tableHeader = stream.NewTableStreamHeaderFromStream();
         var thFlags = tableHeader.TableStreamFlags;

         var tableStartPosition = stream.Stream.Position;
         this.TableStreamHeader = tableHeader;
         this.TableSizes = tableHeader.CreateTableSizesArray().ToArrayProxy().CQ;

         this.TableSerializationInfo = tableSerializations;
         var supportArgs = new ColumnSerializationSupportCreationArgs( this.TableSizes, thFlags.IsWideBLOB(), thFlags.IsWideGUID(), thFlags.IsWideStrings() );
         this.TableSerializationSupport =
            this.TableSerializationInfo
            .Select( table => table.CreateSupport( supportArgs ) )
            .ToArrayProxy()
            .CQ;

         this.TableWidths =
            this.TableSerializationSupport
            .Select( table => table.ColumnSerializationSupports.Aggregate( 0, ( curRowBytecount, colInfo ) => curRowBytecount + colInfo.ColumnByteCount ) )
            .ToArrayProxy()
            .CQ;

         this.TableStartOffsets =
            this.TableSizes
            .AggregateIntermediate_BeforeAggregation( tableStartPosition, ( curOffset, size, idx ) => curOffset + size * this.TableWidths[idx] )
            .ToArrayProxy()
            .CQ;

      }

      public virtual RawValueStorage<Int32> PopulateMetaDataStructure(
         CILMetaData md,
         ReaderMetaDataStreamContainer mdStreamContainer
         )
      {
         var rawValueStorage = this.CreateRawValueStorage() ?? this.CreateDefaultRawValueStorage();
         var args = new RowReadingArguments( this.Stream, mdStreamContainer, rawValueStorage );
         for ( var i = 0; i < this.TableSizes.Count; ++i )
         {
            var table = md.GetByTable( (Tables) i );
            this.TableSerializationSupport[i].ReadRows( table, this.TableSizes[i], args );
         }

         return rawValueStorage;
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
            retVal = this.TableSerializationSupport[tableInt].ReadRawRow( this.Stream.At( offset ) );
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

      protected virtual RawValueStorage<Int32> CreateRawValueStorage()
      {
         return this.CreateDefaultRawValueStorage();
      }

      protected RawValueStorage<Int32> CreateDefaultRawValueStorage()
      {
         return new RawValueStorage<Int32>(
            this.TableSizes,
            this.TableSerializationInfo.Select( t => t.RawValueStorageColumnCount )
            );
      }

      protected MetaDataTableStreamHeader TableStreamHeader { get; }

      protected ArrayQuery<Int32> TableSizes { get; }

      protected ArrayQuery<Int32> TableWidths { get; }

      protected ArrayQuery<Int64> TableStartOffsets { get; }

      protected ArrayQuery<TableSerializationInfo> TableSerializationInfo { get; }

      protected ArrayQuery<TableSerializationFunctionality> TableSerializationSupport { get; }

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

   public class DefaultReaderBLOBStreamHandler : AbstractReaderStreamHandlerImplWithCache<Byte[]>, ReaderBLOBStreamHandler
   {
      private readonly IDictionary<Int32, Object> _constants;

      public DefaultReaderBLOBStreamHandler(
         StreamHelper stream,
         Int32 streamSize
         )
         : base( stream, streamSize )
      {
         this._constants = new Dictionary<Int32, Object>();
      }

      public override String StreamName
      {
         get
         {
            return MetaDataConstants.BLOB_STREAM_NAME;
         }
      }

      public Byte[] GetBLOB( Int32 heapIndex )
      {
         return this.GetOrAddValue( heapIndex );
      }

      public StreamHelper GetBLOBAsStreamPortion( Int32 heapIndex )
      {
         Int32 size;
         var min = this.GetStreamOffset( heapIndex, out size );
         return min >= 0 && size >= 0 ? this.Stream.NewStreamPortionFromCurrent( size ) : null;
      }

      public Int64 GetStreamOffset( Int32 heapIndex, out Int32 blobSize )
      {
         Int64 retVal;
         if ( this.CheckHeapOffset( heapIndex ) )
         {
            var stream = this.SetStreamToHeapOffset( heapIndex );
            retVal = stream.DecompressUInt32( out blobSize ) ?
               -1L :
               stream.Stream.Position;
         }
         else
         {
            blobSize = -1;
            retVal = -1L;
         }
         return retVal;
      }

      public AbstractCustomAttributeSignature ReadCASignature( Int32 heapIndex )
      {
         AbstractCustomAttributeSignature caSig;
         Int32 blobSize;
         if ( this.GetStreamOffset( heapIndex, out blobSize ) >= 0 )
         {
            if ( blobSize <= 2 )
            {
               // Empty blob
               caSig = new CustomAttributeSignature();
            }
            else
            {
               caSig = new RawCustomAttributeSignature()
               {
                  Bytes = this.GetBLOB( heapIndex )
               };
            }
         }
         else
         {
            caSig = null;
         }
         return caSig;
      }


      public Object ReadConstantValue( Int32 heapIndex, SignatureElementTypes constType )
      {
         return this.GetOrAddCustom(
            this._constants,
            heapIndex,
            h => this.DoReadConstantValue( h, constType )
            );
      }

      public MarshalingInfo ReadMarshalingInfo( Int32 heapIndex )
      {
         return MarshalingInfo.ReadFromBytes( this.GetBLOBAsStreamPortion( heapIndex ) );
      }

      public IEnumerable<AbstractSecurityInformation> ReadSecurityInformation( Int32 heapIndex )
      {
         return AbstractSecurityInformation.ReadSecurityInformation( this.GetBLOBAsStreamPortion( heapIndex ) );
      }

      public AbstractSignature ReadNonTypeSignature( Int32 heapIndex, Boolean methodSigIsDefinition, Boolean handleFieldSigAsLocalsSig, out Boolean fieldSigTransformedToLocalsSig )
      {
         return AbstractNotRawSignature.ReadNonTypeSignature( this.GetBLOBAsStreamPortion( heapIndex ), methodSigIsDefinition, handleFieldSigAsLocalsSig, out fieldSigTransformedToLocalsSig );
      }

      public TypeSignature ReadTypeSignature( Int32 heapIndex )
      {
         return TypeSignature.ReadTypeSignature( this.GetBLOBAsStreamPortion( heapIndex ), true );
      }

      protected override Byte[] ValueFactory( Int32 heapOffset )
      {
         Int32 blobLen;
         var stream = this.SetStreamToHeapOffset( heapOffset );
         return stream.DecompressUInt32( out blobLen )
            && this.Stream.Stream.CanReadNextBytes( blobLen ).IsTrue() ?
            stream.ReadAndCreateArray( blobLen ) :
            null;
      }

      private TValue GetOrAddCustom<TValue>( IDictionary<Int32, TValue> cache, Int32 heapOffset, Func<Int32, TValue> factory )
         where TValue : class
      {
         return this.CheckHeapOffset( heapOffset ) ?
            cache.GetOrAdd_NotThreadSafe( heapOffset, i => this.CustomValueFactory( i, factory ) ) :
            null;
      }

      private TValue CustomValueFactory<TValue>( Int32 heapOffset, Func<Int32, TValue> actualFactory )
         where TValue : class
      {
         try
         {
            return actualFactory( heapOffset );
         }
         catch
         {
            return null;
         }
      }

      private Object DoReadConstantValue( Int32 heapIndex, SignatureElementTypes constType )
      {
         var stream = this.SetStreamToHeapOffset( heapIndex );
         Object retVal;
         Int32 blobSize;
         if ( stream.DecompressUInt32( out blobSize ) )
         {
            var s = stream.Stream;
            switch ( constType )
            {
               case SignatureElementTypes.Boolean:
                  return s.CanReadNextBytes( 1 ).IsTrue() ? (Object) ( stream.ReadByteFromBytes() == 1 ) : null;
               case SignatureElementTypes.Char:
                  return s.CanReadNextBytes( 2 ).IsTrue() ? (Object) Convert.ToChar( stream.ReadUInt16LEFromBytes() ) : null;
               case SignatureElementTypes.I1:
                  return s.CanReadNextBytes( 1 ).IsTrue() ? (Object) stream.ReadSByteFromBytes() : null;
               case SignatureElementTypes.U1:
                  return s.CanReadNextBytes( 1 ).IsTrue() ? (Object) stream.ReadByteFromBytes() : null;
               case SignatureElementTypes.I2:
                  return s.CanReadNextBytes( 2 ).IsTrue() ? (Object) stream.ReadInt16LEFromBytes() : null;
               case SignatureElementTypes.U2:
                  return s.CanReadNextBytes( 2 ).IsTrue() ? (Object) stream.ReadUInt16LEFromBytes() : null;
               case SignatureElementTypes.I4:
                  return s.CanReadNextBytes( 4 ).IsTrue() ? (Object) stream.ReadInt32LEFromBytes() : null;
               case SignatureElementTypes.U4:
                  return s.CanReadNextBytes( 4 ).IsTrue() ? (Object) stream.ReadUInt32LEFromBytes() : null;
               case SignatureElementTypes.I8:
                  return s.CanReadNextBytes( 8 ).IsTrue() ? (Object) stream.ReadInt64LEFromBytes() : null;
               case SignatureElementTypes.U8:
                  return s.CanReadNextBytes( 8 ).IsTrue() ? (Object) stream.ReadUInt64LEFromBytes() : null;
               case SignatureElementTypes.R4:
                  return s.CanReadNextBytes( 4 ).IsTrue() ? (Object) stream.ReadSingleLEFromBytes() : null;
               case SignatureElementTypes.R8:
                  return s.CanReadNextBytes( 8 ).IsTrue() ? (Object) stream.ReadDoubleLEFromBytes() : null;
               case SignatureElementTypes.String:
                  return s.CanReadNextBytes( blobSize ).IsTrue() ? MetaDataConstants.USER_STRING_ENCODING.GetString( stream.ReadAndCreateArray( blobSize ) ) : null;
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

   public partial class DefaultMetaDataSerializationSupportProvider
   {
      [Flags]
      private enum MethodHeaderFlags
      {
         TinyFormat = 0x2,
         FatFormat = 0x3,
         MoreSections = 0x8,
         InitLocals = 0x10
      }

      [Flags]
      private enum MethodDataFlags
      {
         ExceptionHandling = 0x1,
         OptimizeILTable = 0x2,
         FatFormat = 0x40,
         MoreSections = 0x80
      }

      protected virtual MethodILDefinition DeserializeIL(
         RawValueProcessingArgs args,
         Int32 rva
         )
      {
         Int64 offset;
         MethodILDefinition retVal = null;
         if ( rva > 0 && ( offset = args.RVAConverter.ToOffset( rva ) ) > 0 )
         {
            var stream = args.Stream.At( offset );
            var userStrings = args.MDStreamContainer.UserStrings;


            var FORMAT_MASK = 0x00000001;
            var FLAG_MASK = 0x00000FFF;
            var SEC_SIZE_MASK = 0xFFFFFF00u;
            var SEC_FLAG_MASK = 0x000000FFu;
            Byte b;
            if ( stream.TryReadByteFromBytes( out b ) )
            {
               Byte b2;
               if ( ( FORMAT_MASK & b ) == 0 )
               {
                  // Tiny header - no locals, no exceptions, no extra data
                  CreateOpCodes( retVal, stream, b >> 2, userStrings );
                  // Max stack is 8
                  retVal.MaxStackSize = 8;
                  retVal.InitLocals = false;
               }
               else if ( stream.TryReadByteFromBytes( out b2 ) )
               {
                  var starter = ( b << 8 ) | b2;
                  var flags = (MethodHeaderFlags) ( starter & FLAG_MASK );
                  retVal.InitLocals = ( flags & MethodHeaderFlags.InitLocals ) != 0;
                  var headerSize = ( starter >> 12 ) * 4; // Header size is written as amount of integers
                                                          // Read max stack
                  retVal.MaxStackSize = stream.ReadUInt16LEFromBytes();
                  var codeSize = stream.ReadInt32LEFromBytes();
                  retVal.LocalsSignatureIndex = TableIndex.FromOneBasedTokenNullable( stream.ReadInt32LEFromBytes() );

                  if ( headerSize > 12 )
                  {
                     stream.SkipToNextAlignmentInt32();
                  }

                  // Read code
                  if ( CreateOpCodes( retVal, stream, codeSize, userStrings ) )
                  {
                     stream.SkipToNextAlignmentInt32();

                     var excList = new List<MethodExceptionBlock>();
                     if ( ( flags & MethodHeaderFlags.MoreSections ) != 0 )
                     {
                        // Read sections
                        MethodDataFlags secFlags;
                        do
                        {
                           var secHeader = stream.ReadInt32LEFromBytes();
                           secFlags = (MethodDataFlags) ( secHeader & SEC_FLAG_MASK );
                           var secByteSize = ( secHeader & SEC_SIZE_MASK ) >> 8;
                           secByteSize -= 4;
                           var isFat = ( secFlags & MethodDataFlags.FatFormat ) != 0;
                           while ( secByteSize > 0 )
                           {
                              var eType = (ExceptionBlockType) ( isFat ? stream.ReadInt32LEFromBytes() : stream.ReadUInt16LEFromBytes() );
                              retVal.ExceptionBlocks.Add( new MethodExceptionBlock()
                              {
                                 BlockType = eType,
                                 TryOffset = isFat ? stream.ReadInt32LEFromBytes() : stream.ReadUInt16LEFromBytes(),
                                 TryLength = isFat ? stream.ReadInt32LEFromBytes() : stream.ReadByteFromBytes(),
                                 HandlerOffset = isFat ? stream.ReadInt32LEFromBytes() : stream.ReadUInt16LEFromBytes(),
                                 HandlerLength = isFat ? stream.ReadInt32LEFromBytes() : stream.ReadByteFromBytes(),
                                 ExceptionType = eType == ExceptionBlockType.Filter ? (TableIndex?) null : TableIndex.FromOneBasedTokenNullable( stream.ReadInt32LEFromBytes() ),
                                 FilterOffset = eType == ExceptionBlockType.Filter ? stream.ReadInt32LEFromBytes() : 0
                              } );
                              secByteSize -= ( isFat ? 24u : 12u );
                           }
                        } while ( ( secFlags & MethodDataFlags.MoreSections ) != 0 );
                     }
                  }
               }
               else
               {
                  retVal = null;
               }
            }
         }
         return retVal;
      }

      private static Boolean CreateOpCodes(
         MethodILDefinition methodIL,
         StreamHelper stream,
         Int32 codeSize,
         ReaderStringStreamHandler userStrings
         )
      {

         var success = codeSize >= 0;
         if ( codeSize > 0 )
         {
            var opCodes = methodIL.OpCodes;
            var s = stream.Stream;
            var max = s.Position + codeSize;
            while ( s.Position < max && success )
            {
               var curCodeInfo = OpCodeInfo.ReadFromStream(
                  stream,
                  strToken =>
                  {
                     var oldPos = s.Position;
                     var str = userStrings.GetString( TableIndex.FromZeroBasedToken( strToken ).Index );
                     s.Position = oldPos;
                     return str;
                  } );

               if ( curCodeInfo == null )
               {
                  success = false;
               }
               else
               {
                  opCodes.Add( curCodeInfo );
               }
            }
         }

         return success;
      }

      protected virtual Byte[] DeserializeConstantValue(
         RawValueProcessingArgs args,
         FieldRVA row,
         Int32 rva
         )
      {
         Byte[] retVal = null;
         Int64 offset;
         if ( rva > 0 && ( offset = args.RVAConverter.ToOffset( rva ) ) > 0 )
         {
            // Read all field RVA content
            var layoutInfo = args.LayoutInfo;

            var stream = args.Stream.At( offset );
            Int32 size;
            if ( TryCalculateFieldTypeSize( args, row.Field.Index, out size )
               && stream.Stream.CanReadNextBytes( size ).IsTrue()
               )
            {
               // Sometimes there are field RVAs that are unresolvable...
               retVal = stream.ReadAndCreateArray( size );
            }
         }
         return retVal;
      }

      private static Boolean TryCalculateFieldTypeSize(
         RawValueProcessingArgs args,
         Int32 fieldIdx,
         out Int32 size,
         Boolean onlySimpleTypeValid = false
         )
      {
         var md = args.MetaData;
         var fDef = md.FieldDefinitions.TableContents;
         size = 0;
         if ( fieldIdx < fDef.Count )
         {
            var type = fDef[fieldIdx]?.Signature?.Type;
            if ( type != null )
            {
               switch ( type.TypeSignatureKind )
               {
                  case TypeSignatureKind.Simple:
                     switch ( ( (SimpleTypeSignature) type ).SimpleType )
                     {
                        case SignatureElementTypes.Boolean:
                           size = sizeof( Boolean ); // TODO is this actually 1 or 4?
                           break;
                        case SignatureElementTypes.I1:
                        case SignatureElementTypes.U1:
                           size = 1;
                           break;
                        case SignatureElementTypes.I2:
                        case SignatureElementTypes.U2:
                        case SignatureElementTypes.Char:
                           size = 2;
                           break;
                        case SignatureElementTypes.I4:
                        case SignatureElementTypes.U4:
                        case SignatureElementTypes.R4:
                        case SignatureElementTypes.FnPtr:
                        case SignatureElementTypes.Ptr: // I am not 100% sure of this.
                           size = 4;
                           break;
                        case SignatureElementTypes.I8:
                        case SignatureElementTypes.U8:
                        case SignatureElementTypes.R8:
                           size = 8;
                           break;
                     }
                     break;
                  case TypeSignatureKind.ClassOrValue:
                     if ( !onlySimpleTypeValid )
                     {
                        var c = (ClassOrValueTypeSignature) type;

                        var typeIdx = c.Type;
                        if ( typeIdx.Table == Tables.TypeDef )
                        {
                           // Only possible for types defined in this module
                           Int32 enumValueFieldIndex;
                           if ( md.TryGetEnumValueFieldIndex( typeIdx.Index, out enumValueFieldIndex ) )
                           {
                              TryCalculateFieldTypeSize( args, enumValueFieldIndex, out size, true ); // Last parameter true to prevent possible infinite recursion in case of malformed metadata
                           }
                           else
                           {
                              ClassLayout layout;
                              if ( args.LayoutInfo.TryGetValue( typeIdx.Index, out layout ) )
                              {
                                 size = layout.ClassSize;
                              }
                           }

                        }
                     }
                     break;
               }
            }
         }
         return size != 0;
      }

      protected virtual Byte[] DeserializeEmbeddedManifest(
         RawValueProcessingArgs args,
         Int32 offset
         )
      {
         Byte[] retVal = null;
         var stream = args.Stream;
         var rsrcDD = args.ImageInformation.CLIInformation.CLIHeader.Resources;
         Int64 ddOffset;
         if ( rsrcDD.RVA > 0
            && ( ddOffset = args.RVAConverter.ToOffset( rsrcDD.RVA ) ) > 0
            && ( stream = stream.At( ddOffset ) ).Stream.CanReadNextBytes( offset + sizeof( Int32 ) ).IsTrue()
            )
         {
            stream = stream.At( ddOffset + offset );
            var size = stream.ReadInt32LEFromBytes();
            if ( stream.Stream.CanReadNextBytes( size ).IsTrue() )
            {
               retVal = stream.ReadAndCreateArray( size );
            }
         }
         return retVal;
      }
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