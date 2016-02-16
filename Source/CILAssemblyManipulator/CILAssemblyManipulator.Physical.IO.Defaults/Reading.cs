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
      /// This method will return <see cref="DefaultReaderFunctionality"/>.
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
         // We are going to do a lot of seeking, so just read whole stream into byte array and use memory stream
         newStream = !( stream is MemoryStream ) && deserializingDataReferences ?
            new MemoryStream( stream.ReadUntilTheEnd(), this.IsMemoryStreamWriteable ) :
            null;
         return new DefaultReaderFunctionality( new TableSerializationInfoCreationArgs( errorHandler ), tableInfoProvider: mdTableInfoProvider );
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

   public class DefaultReaderFunctionality : ReaderFunctionality
   {

      public DefaultReaderFunctionality(
         TableSerializationInfoCreationArgs serializationCreationArgs,
         CILMetaDataTableInformationProvider tableInfoProvider = null
         )
      {
         this.TableInfoProvider = tableInfoProvider ?? DefaultMetaDataTableInformationProvider.CreateDefault();
         this.TableSerializations = serializationCreationArgs.CreateTableSerializationInfos( this.TableInfoProvider ).ToArrayProxy().CQ;
      }

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
         rvaConverter = this.CreateRVAConverter( peInfo ) ?? this.CreateDefaultRVAConverter( peInfo );

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
               return new DefaultReaderTableStreamHandler( stream, startPosition, size, header.Name, this.TableSerializations, mdRoot );
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

      public virtual CILMetaData CreateBlankMetaData( ArrayQuery<Int32> tableSizes )
      {
         return CILMetaDataFactory.NewBlankMetaData(
            sizes: tableSizes.ToArray(),
            tableInfoProvider: this.TableInfoProvider
            );
      }

      public virtual void HandleDataReferences(
         StreamHelper stream,
         ImageInformation imageInfo,
         RVAConverter rvaConverter,
         ReaderMetaDataStreamContainer mdStreamContainer,
         CILMetaData md
         )
      {
         var args = this.CreateRawValueProcessingArgs( stream, imageInfo, rvaConverter, mdStreamContainer, md ) ?? CreateDefaultRawValueProcessingArgs( stream, imageInfo, rvaConverter, mdStreamContainer, md );
         foreach ( var tableSerialization in this.TableSerializations )
         {
            tableSerialization?.ProcessRowForRawValues( args );
         }
      }

      protected virtual RVAConverter CreateRVAConverter(
         PEInformation peInformation
         )
      {
         return this.CreateDefaultRVAConverter( peInformation );
      }

      protected RVAConverter CreateDefaultRVAConverter(
         PEInformation peInformation
         )
      {
         return new DefaultRVAConverter( peInformation.SectionHeaders );
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
         return new RawValueProcessingArgs( stream, imageInfo, rvaConverter, mdStreamContainer, md, new ResizableArray<Byte>( initialSize: 0x1000 ) );
      }

      protected CILMetaDataTableInformationProvider TableInfoProvider { get; }

      protected ArrayQuery<TableSerializationInfo> TableSerializations { get; }

   }

   public abstract class AbstractReaderStreamHandlerWithArray : AbstractReaderStreamHandler
   {
      protected AbstractReaderStreamHandlerWithArray(
         StreamHelper stream,
         Int64 startPosition,
         Int32 streamSize
         )
      {
         ArgumentValidator.ValidateNotNull( "Stream", stream );

         this.Bytes = stream.At( startPosition ).ReadAndCreateArray( streamSize );
         this.StreamSize = streamSize;
         this.StreamSize64 = (UInt32) streamSize;
      }

      public abstract String StreamName { get; }

      public Int32 StreamSize { get; }

      protected Byte[] Bytes { get; }

      protected Int64 StreamSize64 { get; }

      protected virtual Boolean CheckHeapOffset( Int32 heapOffset )
      {
         return ( (UInt32) heapOffset ) < this.StreamSize64;
      }
   }

   public abstract class AbstractReaderStreamHandlerWithArrayAndName : AbstractReaderStreamHandlerWithArray
   {
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

      public override String StreamName { get; }

   }



   public class DefaultReaderTableStreamHandler : AbstractReaderStreamHandlerWithArrayAndName, ReaderTableStreamHandler
   {
      public DefaultReaderTableStreamHandler(
         StreamHelper stream,
         Int64 startPosition,
         Int32 streamSize,
         String tableStreamName,
         ArrayQuery<TableSerializationInfo> tableSerializations,
         MetaDataRoot mdRoot
         )
         : this( stream, startPosition, streamSize, tableStreamName, tableSerializations, ( me, tableSizes ) => new DefaultColumnSerializationSupportCreationArgs( tableSizes, mdRoot.StreamHeaders.ToDictionary_Preserve( sh => sh.Name, sh => (Int32) sh.Size ).ToDictionaryProxy().CQ ) )
      {

      }

      protected DefaultReaderTableStreamHandler(
         StreamHelper stream,
         Int64 startPosition,
         Int32 streamSize,
         String tableStreamName,
         ArrayQuery<TableSerializationInfo> tableSerializations,
         Func<DefaultReaderTableStreamHandler, ArrayQuery<Int32>, DefaultColumnSerializationSupportCreationArgs> creationArgsFunc
         )
         : base( stream, startPosition, streamSize, tableStreamName )
      {
         ArgumentValidator.ValidateAllNotNull( "Table serializations", tableSerializations );

         var array = this.Bytes;
         var idx = 0;
         var tableHeader = array.ReadTableStreamHeader( ref idx );
         var thFlags = tableHeader.TableStreamFlags;

         var tableStartPosition = idx;
         this.TableStreamHeader = tableHeader;
         this.TableSizes = tableHeader.CreateTableSizesArray().ToArrayProxy().CQ;

         tableSerializations = tableSerializations
            .Concat( Enumerable.Repeat<TableSerializationInfo>( null, Math.Max( 0, this.TableSizes.Count - tableSerializations.Count ) ) )
            .ToArrayProxy()
            .CQ;


         this.TableSerializationInfo = tableSerializations;
         var creationArgs = creationArgsFunc( this, this.TableSizes );
         this.TableSerializationSupport =
            this.TableSerializationInfo
            .Select( table => table?.CreateSupport( creationArgs ) )
            .ToArrayProxy()
            .CQ;

         this.TableWidths =
            this.TableSerializationSupport
            .Select( table => table?.ColumnSerializationSupports.Aggregate( 0, ( curRowBytecount, colInfo ) => curRowBytecount + colInfo.ColumnByteCount ) ?? 0 )
            .ToArrayProxy()
            .CQ;

         this.TableStartOffsets =
            this.TableSizes
            .AggregateIntermediate_BeforeAggregation( tableStartPosition, ( curOffset, size, i ) => curOffset + size * this.TableWidths[i] )
            .ToArrayProxy()
            .CQ;

      }

      public ArrayQuery<Int32> TableSizes { get; }

      public virtual DataReferencesInfo PopulateMetaDataStructure(
         CILMetaData md,
         ReaderMetaDataStreamContainer mdStreamContainer
         )
      {
         var rawValueStorage = this.CreateRawValueStorage() ?? this.CreateDefaultRawValueStorage();
         var array = this.Bytes;
         for ( var i = 0; i < this.TableSizes.Count; ++i )
         {
            var rowCount = this.TableSizes[i];
            if ( rowCount > 0 )
            {
               var args = new RowReadingArguments( array, this.TableStartOffsets[i], mdStreamContainer, rawValueStorage, md.SignatureProvider );

               var table = md.GetByTable( i );
               this.TableSerializationSupport[i].ReadRows( table, this.TableSizes[i], args );
            }
         }

         return rawValueStorage.CreateDataReferencesInfo( i => (UInt32) i );
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
            var offset = this.TableStartOffsets[tableInt] + idx * this.TableWidths[tableInt];
            retVal = this.TableSerializationSupport[tableInt].ReadRawRow( this.Bytes, offset );
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

      protected virtual ColumnValueStorage<Int32> CreateRawValueStorage()
      {
         return this.CreateDefaultRawValueStorage();
      }

      protected ColumnValueStorage<Int32> CreateDefaultRawValueStorage()
      {
         return new ColumnValueStorage<Int32>(
            this.TableSizes,
            this.TableSerializationInfo.Select( t => t?.RawValueStorageColumnCount ?? 0 )
            );
      }

      protected MetaDataTableStreamHeader TableStreamHeader { get; }

      protected ArrayQuery<Int32> TableWidths { get; }

      protected ArrayQuery<Int32> TableStartOffsets { get; }

      protected ArrayQuery<TableSerializationInfo> TableSerializationInfo { get; }

      protected ArrayQuery<TableSerializationFunctionality> TableSerializationSupport { get; }

   }

   public abstract class AbstractReaderStreamHandlerWithArrayAndCache<TValue> : AbstractReaderStreamHandlerWithArray
   {
      private readonly IDictionary<Int32, TValue> _cache;

      protected AbstractReaderStreamHandlerWithArrayAndCache(
         StreamHelper stream,
         Int64 startPosition,
         Int32 streamSize
         ) : base( stream, startPosition, streamSize )
      {
         this._cache = new Dictionary<Int32, TValue>();
      }

      protected TValue GetOrAddValue( Int32 heapOffset )
      {
         return this.CheckHeapOffset( heapOffset ) ?
            this._cache.GetOrAdd_NotThreadSafe( heapOffset, this.ValueFactoryWrapper ) :
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

   public abstract class AbstractReaderStringStreamHandler : AbstractReaderStreamHandlerWithArrayAndCache<String>, ReaderStringStreamHandler
   {
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

      public String GetString( Int32 heapIndex )
      {
         return this.GetOrAddValue( heapIndex );
      }

      public abstract StringStreamKind StringStreamKind { get; }

      protected Encoding Encoding { get; }
   }

   public class DefaultReaderGUIDStreamHandler : AbstractReaderStreamHandlerWithArrayAndCache<Guid?>, ReaderGUIDStreamHandler
   {
      public DefaultReaderGUIDStreamHandler(
         StreamHelper stream,
         Int64 startPosition,
         Int32 streamSize
         )
         : base( stream, startPosition, streamSize )
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

      protected override Guid? ValueFactory( Int32 heapOffset )
      {
         if ( heapOffset == 0 || (UInt32) heapOffset > this.StreamSize64 - MetaDataConstants.GUID_SIZE + 1 )
         {
            return null;
         }
         else
         {
            return new Guid( this.Bytes.CreateArrayCopy( heapOffset - 1, MetaDataConstants.GUID_SIZE ) );
         }
      }
   }

   public class DefaultReaderSystemStringStreamHandler : AbstractReaderStringStreamHandler
   {
      public DefaultReaderSystemStringStreamHandler(
         StreamHelper stream,
         Int64 startPosition,
         Int32 streamSize
         )
         : base( stream, startPosition, streamSize, MetaDataConstants.SYS_STRING_ENCODING )
      {

      }

      public override String StreamName
      {
         get
         {
            return MetaDataConstants.SYS_STRING_STREAM_NAME;
         }
      }

      public override StringStreamKind StringStreamKind
      {
         get
         {
            return StringStreamKind.SystemStrings;
         }
      }

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

   public class DefaultReaderUserStringsStreamHandler : AbstractReaderStringStreamHandler
   {
      public DefaultReaderUserStringsStreamHandler(
         StreamHelper stream,
         Int64 startPosition,
         Int32 streamSize
         )
         : base( stream, startPosition, streamSize, MetaDataConstants.USER_STRING_ENCODING )
      {

      }

      public override String StreamName
      {
         get
         {
            return MetaDataConstants.USER_STRING_STREAM_NAME;
         }
      }

      public override StringStreamKind StringStreamKind
      {
         get
         {
            return StringStreamKind.UserStrings;
         }
      }

      protected override String ValueFactory( Int32 heapIndex )
      {
         String retVal;
         if ( heapIndex == 0 )
         {
            retVal = "";
         }
         else
         {
            var array = this.Bytes;

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
         }
         return retVal;
      }
   }

   public class DefaultReaderBLOBStreamHandler : AbstractReaderStreamHandlerWithArrayAndCache<Tuple<Int32, Int32>>, ReaderBLOBStreamHandler
   {
      private readonly IDictionary<KeyValuePair<Int32, ConstantValueType>, Object> _constants;

      public DefaultReaderBLOBStreamHandler(
         StreamHelper stream,
         Int64 startPosition,
         Int32 streamSize
         )
         : base( stream, startPosition, streamSize )
      {
         this._constants = new Dictionary<KeyValuePair<Int32, ConstantValueType>, Object>();
      }

      public override String StreamName
      {
         get
         {
            return MetaDataConstants.BLOB_STREAM_NAME;
         }
      }

      public Byte[] GetBLOBByteArray( Int32 heapIndex )
      {
         Int32 len;
         return this.SetUpBLOBWithLength( ref heapIndex, out len ) ? this.Bytes.CreateArrayCopy( heapIndex, len ) : null;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="heapIndex"></param>
      /// <param name="sigProvider"></param>
      /// <returns></returns>
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

      public Object ReadConstantValue( Int32 heapIndex, SignatureProvider sigProvider, ConstantValueType constType )
      {
         return heapIndex == 0 || heapIndex >= this.StreamSize64 ?
            null :
            this._constants.GetOrAdd_NotThreadSafe(
               new KeyValuePair<Int32, ConstantValueType>( heapIndex, constType ),
               kvp => this.DoReadConstantValue( kvp.Key, kvp.Value )
               );
      }

      public AbstractMarshalingInfo ReadMarshalingInfo( Int32 heapIndex, SignatureProvider sigProvider )
      {
         Int32 max;
         return this.SetUpBLOBWithMax( ref heapIndex, out max ) ? sigProvider.ReadMarshalingInfo( this.Bytes, ref heapIndex, max ) : null;
      }

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

      public void ReadSecurityInformation( Int32 heapIndex, SignatureProvider sigProvider, List<AbstractSecurityInformation> securityInfo )
      {
         Int32 max;
         if ( this.SetUpBLOBWithMax( ref heapIndex, out max ) )
         {
            sigProvider.ReadSecurityInformation( this.Bytes, ref heapIndex, max, securityInfo );
         }
      }

      public TypeSignature ReadTypeSignature( Int32 heapIndex, SignatureProvider sigProvider )
      {
         Int32 max;
         return this.SetUpBLOBWithMax( ref heapIndex, out max ) ? sigProvider.ReadTypeSignature( this.Bytes, ref heapIndex, max ) : null;
      }

      protected Boolean SetUpBLOBWithMax( ref Int32 heapIndex, out Int32 max )
      {
         var retVal = this.SetUpBLOBWithLength( ref heapIndex, out max );
         if ( retVal )
         {
            max += heapIndex;
         }
         return retVal;
      }

      protected Boolean SetUpBLOBWithLength( ref Int32 heapIndex, out Int32 length )
      {
         if ( heapIndex > 0 )
         {
            var tuple = this.GetOrAddValue( heapIndex );

            heapIndex = tuple?.Item1 ?? heapIndex;
            length = tuple?.Item2 ?? 0;
            return tuple != null;
            //return array.TryDecompressUInt32( ref heapIndex, array.Length, out length ) && heapIndex + length <= this.StreamSize;
         }
         else
         {
            length = 0;
            return false;
         }
      }

      protected override Tuple<Int32, Int32> ValueFactory( int heapOffset )
      {
         var array = this.Bytes;
         Int32 length;
         return array.TryDecompressUInt32( ref heapOffset, array.Length, out length ) && heapOffset + length <= this.StreamSize64 ?
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

public static partial class E_CILPhysical
{

   public static StreamHelper GoToRVA( this StreamHelper stream, RVAConverter rvaConverter, Int64 rva )
   {
      stream.Stream.SeekFromBegin( rvaConverter.ToOffset( rva ) );
      return stream;
   }

   //[CLSCompliant( false )]
   //public static UInt32 ToRVANullable( this RVAConverter rvaConverter, Int64? offset )
   //{
   //   return offset.HasValue ? (UInt32) rvaConverter.ToRVA( offset.Value ) : 0;
   //}
}