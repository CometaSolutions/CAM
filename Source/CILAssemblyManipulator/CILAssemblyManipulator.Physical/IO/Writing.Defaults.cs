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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtils;
using CILAssemblyManipulator.Physical.Implementation;
using System.Threading;
using System.IO;
using CILAssemblyManipulator.Physical;
using CollectionsWithRoles.API;

namespace CILAssemblyManipulator.Physical.IO
{
   public class DefaultWriterFunctionalityProvider : WriterFunctionalityProvider
   {
      public virtual WriterFunctionality GetFunctionality(
         CILMetaData md,
         WritingOptions headers,
         out CILMetaData newMD,
         out Stream newStream
         )
      {
         newMD = null;
         newStream = null;
         return new DefaultWriterFunctionality( md, headers, this.CreateMDSerialization() );
      }

      protected virtual MetaDataSerializationSupportProvider CreateMDSerialization()
      {
         return DefaultMetaDataSerializationSupportProvider.Instance;
      }
   }

   public class DefaultWriterFunctionality : WriterFunctionality
   {


      private readonly WritingOptions _headers;

      public DefaultWriterFunctionality(
         CILMetaData md,
         WritingOptions headers,
         MetaDataSerializationSupportProvider mdSerialization = null
         )
      {
         this.MetaData = md;
         this._headers = headers ?? new WritingOptions();
         this.MDSerialization = mdSerialization ?? DefaultMetaDataSerializationSupportProvider.Instance;
         this.TableSerializations = this.MDSerialization.CreateTableSerializationInfos().ToArrayProxy().CQ;
         this.TableSizes = this.TableSerializations.CreateTableSizeArray( md );
      }

      public virtual IEnumerable<AbstractWriterStreamHandler> CreateStreamHandlers()
      {
         yield return new DefaultWriterTableStreamHandler( this.MetaData, this._headers.CLIOptions.TablesStreamOptions, this.TableSerializations );
         yield return new DefaultWriterSystemStringStreamHandler();
         yield return new DefaultWriterBLOBStreamHandler();
         yield return new DefaultWriterGuidStreamHandler();
         yield return new DefaultWriterUserStringStreamHandler();
      }

      public virtual RawValueStorage<Int64> CreateRawValuesBeforeMDStreams(
         Stream stream,
         ResizableArray<Byte> array,
         WriterMetaDataStreamContainer mdStreams,
         WritingStatus writingStatus
         )
      {
         var retVal = this.CreateRawValueStorage() ?? CreateDefaultRawValueStorage();
         foreach ( var info in this.TableSerializations )
         {
            info.ExtractTableRawValues( this.MetaData, retVal, stream, array, mdStreams );
         }

         return retVal;
      }

      public virtual IEnumerable<SectionHeader> CreateSections(
         WritingStatus writingStatus,
         out RVAConverter rvaConverter
         )
      {

      }

      public virtual void FinalizeWritingStatus(
         WritingStatus writingStatus
         )
      {

      }

      protected CILMetaData MetaData { get; }

      protected MetaDataSerializationSupportProvider MDSerialization { get; }

      protected ArrayQuery<TableSerializationInfo> TableSerializations { get; }

      protected ArrayQuery<Int32> TableSizes { get; }

      protected virtual RawValueStorage<Int64> CreateRawValueStorage()
      {
         return this.CreateDefaultRawValueStorage();
      }

      protected RawValueStorage<Int64> CreateDefaultRawValueStorage()
      {
         return new RawValueStorage<Int64>(
            this.TableSizes,
            this.TableSerializations.Select( t => t.RawValueStorageColumnCount )
            );
      }
   }

   public partial class DefaultMetaDataSerializationSupportProvider
   {
      private const Int32 METHOD_DATA_SECTION_HEADER_SIZE = 4;
      private const Int32 SMALL_EXC_BLOCK_SIZE = 12;
      private const Int32 LARGE_EXC_BLOCK_SIZE = 24;
      private const Int32 MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION = ( Byte.MaxValue - METHOD_DATA_SECTION_HEADER_SIZE ) / SMALL_EXC_BLOCK_SIZE; // 20
      private const Int32 MAX_LARGE_EXC_HANDLERS_IN_ONE_SECTION = ( 0x00FFFFFF - METHOD_DATA_SECTION_HEADER_SIZE ) / LARGE_EXC_BLOCK_SIZE; // 699050
      private const Int32 FAT_HEADER_SIZE = 12;

      protected virtual Int32 WriteMethodIL(
         RowRawValueExtractionArguments args,
         MethodILDefinition il
         )
      {
         return this.WriteMethodILToArray( args.Array, args.MDStreamContainer.UserStrings, args.MetaData, il, args.CurrentStreamPosition );
      }

      protected virtual Int32 WriteMethodILToArray(
         ResizableArray<Byte> sink,
         WriterStringStreamHandler userStrings,
         CILMetaData md,
         MethodILDefinition il,
         Int64 currentStreamPosition
         )
      {
         var lIdx = il.LocalsSignatureIndex;
         var locals = lIdx.HasValue && lIdx.Value.Table == Tables.StandaloneSignature ?
            md.StandaloneSignatures.TableContents[lIdx.Value.Index].Signature as LocalVariablesSignature :
            null;
         Boolean isTinyHeader; Boolean exceptionSectionsAreLarge; Int32 wholeMethodByteCount; Int32 idx;
         var ilCodeByteCount = CalculateByteSizeForMethod(
            il,
            locals,
            currentStreamPosition,
            out isTinyHeader,
            out exceptionSectionsAreLarge,
            out wholeMethodByteCount,
            out idx
            );
         var exceptionBlocks = il.ExceptionBlocks;
         var hasAnyExceptions = exceptionBlocks.Count > 0;

         sink.CurrentMaxCapacity = wholeMethodByteCount;
         var array = sink.Array;

         // Header
         if ( isTinyHeader )
         {
            // Tiny header - one byte
            array.WriteByteToBytes( ref idx, (Byte) ( (Int32) MethodHeaderFlags.TinyFormat | ( ilCodeByteCount << 2 ) ) );
         }
         else
         {
            // Fat header - 12 bytes
            var flags = MethodHeaderFlags.FatFormat;
            if ( hasAnyExceptions )
            {
               flags |= MethodHeaderFlags.MoreSections;
            }
            if ( il.InitLocals )
            {
               flags |= MethodHeaderFlags.InitLocals;
            }

            array.WriteInt16LEToBytes( ref idx, (Int16) ( ( (Int32) flags ) | ( 3 << 12 ) ) )
               .WriteUInt16LEToBytes( ref idx, (UInt16) il.MaxStackSize )
               .WriteInt32LEToBytes( ref idx, ilCodeByteCount )
               .WriteInt32LEToBytes( ref idx, il.LocalsSignatureIndex.GetOneBasedToken() );
         }


         // Emit IL code
         foreach ( var info in il.OpCodes )
         {
            EmitOpCodeInfo( info, array, ref idx, userStrings );
         }

         // Emit exception block infos
         if ( hasAnyExceptions )
         {
            var processedIndices = new HashSet<Int32>();
            array.ZeroOut( ref idx, BitUtils.MultipleOf4( idx ) - idx );
            var flags = MethodDataFlags.ExceptionHandling;
            if ( exceptionSectionsAreLarge )
            {
               flags |= MethodDataFlags.FatFormat;
            }
            var excCount = exceptionBlocks.Count;
            var maxExceptionHandlersInOneSections = exceptionSectionsAreLarge ? MAX_LARGE_EXC_HANDLERS_IN_ONE_SECTION : MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION;
            var excBlockSize = exceptionSectionsAreLarge ? LARGE_EXC_BLOCK_SIZE : SMALL_EXC_BLOCK_SIZE;
            var curExcIndex = 0;
            while ( excCount > 0 )
            {
               var amountToBeWritten = Math.Min( excCount, maxExceptionHandlersInOneSections );
               if ( amountToBeWritten < excCount )
               {
                  flags |= MethodDataFlags.MoreSections;
               }
               else
               {
                  flags = flags & ~( MethodDataFlags.MoreSections );
               }

               array.WriteByteToBytes( ref idx, (Byte) flags )
                  .WriteInt32LEToBytes( ref idx, amountToBeWritten * excBlockSize + METHOD_DATA_SECTION_HEADER_SIZE );
               --idx;

               // Subtract this here since amountToBeWritten will change
               excCount -= amountToBeWritten;

               if ( exceptionSectionsAreLarge )
               {
                  while ( amountToBeWritten > 0 )
                  {
                     // Write large exc
                     var block = exceptionBlocks[curExcIndex];
                     array.WriteInt32LEToBytes( ref idx, (Int32) block.BlockType )
                     .WriteInt32LEToBytes( ref idx, block.TryOffset )
                     .WriteInt32LEToBytes( ref idx, block.TryLength )
                     .WriteInt32LEToBytes( ref idx, block.HandlerOffset )
                     .WriteInt32LEToBytes( ref idx, block.HandlerLength )
                     .WriteInt32LEToBytes( ref idx, block.BlockType != ExceptionBlockType.Filter ? block.ExceptionType.GetOneBasedToken() : block.FilterOffset );
                     ++curExcIndex;
                     --amountToBeWritten;
                  }
               }
               else
               {
                  while ( amountToBeWritten > 0 )
                  {
                     var block = exceptionBlocks[curExcIndex];
                     // Write small exception
                     array.WriteInt16LEToBytes( ref idx, (Int16) block.BlockType )
                        .WriteUInt16LEToBytes( ref idx, (UInt16) block.TryOffset )
                        .WriteByteToBytes( ref idx, (Byte) block.TryLength )
                        .WriteUInt16LEToBytes( ref idx, (UInt16) block.HandlerOffset )
                        .WriteByteToBytes( ref idx, (Byte) block.HandlerLength )
                        .WriteInt32LEToBytes( ref idx, block.BlockType != ExceptionBlockType.Filter ? block.ExceptionType.GetOneBasedToken() : block.FilterOffset );
                     ++curExcIndex;
                     --amountToBeWritten;
                  }
               }

            }

         }

#if DEBUG
         if ( idx != wholeMethodByteCount )
         {
            throw new Exception( "Something went wrong when emitting method headers and body. Emitted " + idx + " bytes, but was supposed to emit " + wholeMethodByteCount + " bytes." );
         }
#endif

         return idx;
      }

      protected static void EmitOpCodeInfo(
         OpCodeInfo codeInfo,
         Byte[] array,
         ref Int32 idx,
         WriterStringStreamHandler usersStrings
         )
      {
         const Int32 USER_STRING_MASK = 0x70 << 24;

         var code = codeInfo.OpCode;

         if ( code.Size > 1 )
         {
            array.WriteByteToBytes( ref idx, code.Byte1 );
         }
         array.WriteByteToBytes( ref idx, code.Byte2 );

         var operandType = code.OperandType;
         if ( operandType != OperandType.InlineNone )
         {
            Int32 i32;
            switch ( operandType )
            {
               case OperandType.ShortInlineI:
               case OperandType.ShortInlineVar:
                  array.WriteByteToBytes( ref idx, (Byte) ( (OpCodeInfoWithInt32) codeInfo ).Operand );
                  break;
               case OperandType.ShortInlineBrTarget:
                  i32 = ( (OpCodeInfoWithInt32) codeInfo ).Operand;
                  array.WriteByteToBytes( ref idx, (Byte) i32 );
                  break;
               case OperandType.ShortInlineR:
                  array.WriteSingleLEToBytes( ref idx, (Single) ( (OpCodeInfoWithSingle) codeInfo ).Operand );
                  break;
               case OperandType.InlineBrTarget:
                  i32 = ( (OpCodeInfoWithInt32) codeInfo ).Operand;
                  array.WriteInt32LEToBytes( ref idx, i32 );
                  break;
               case OperandType.InlineI:
                  array.WriteInt32LEToBytes( ref idx, ( (OpCodeInfoWithInt32) codeInfo ).Operand );
                  break;
               case OperandType.InlineVar:
                  array.WriteInt16LEToBytes( ref idx, (Int16) ( (OpCodeInfoWithInt32) codeInfo ).Operand );
                  break;
               case OperandType.InlineR:
                  array.WriteDoubleLEToBytes( ref idx, (Double) ( (OpCodeInfoWithDouble) codeInfo ).Operand );
                  break;
               case OperandType.InlineI8:
                  array.WriteInt64LEToBytes( ref idx, (Int64) ( (OpCodeInfoWithInt64) codeInfo ).Operand );
                  break;
               case OperandType.InlineString:
                  array.WriteInt32LEToBytes( ref idx, usersStrings.RegisterString( ( (OpCodeInfoWithString) codeInfo ).Operand ) | USER_STRING_MASK );
                  break;
               case OperandType.InlineField:
               case OperandType.InlineMethod:
               case OperandType.InlineType:
               case OperandType.InlineTok:
               case OperandType.InlineSig:
                  var tIdx = ( (OpCodeInfoWithToken) codeInfo ).Operand;
                  array.WriteInt32LEToBytes( ref idx, tIdx.OneBasedToken );
                  break;
               case OperandType.InlineSwitch:
                  var offsets = ( (OpCodeInfoWithSwitch) codeInfo ).Offsets;
                  array.WriteInt32LEToBytes( ref idx, offsets.Count );
                  foreach ( var offset in offsets )
                  {
                     array.WriteInt32LEToBytes( ref idx, offset );
                  }
                  break;
               default:
                  throw new ArgumentException( "Unknown operand type: " + code.OperandType + " for " + code + "." );
            }
         }
      }

      protected static Int32 CalculateByteSizeForMethod(
         MethodILDefinition methodIL,
         LocalVariablesSignature localSig,
         Int64 currentStreamPosition,
         out Boolean isTinyHeader,
         out Boolean exceptionSectionsAreLarge,
         out Int32 wholeMethodByteCount,
         out Int32 startIndex
         )
      {
         // Start by calculating the size of just IL code
         var arraySize = methodIL.OpCodes.Sum( oci => oci.GetTotalByteCount() );
         var ilCodeByteCount = arraySize;

         // Then calculate the size of headers and other stuff
         var exceptionBlocks = methodIL.ExceptionBlocks;
         // PEVerify doesn't like mixed small and fat blocks at all (however, at least Cecil understands that kind of situation)
         // Apparently, PEVerify doesn't like multiple small blocks either (Cecil still loads code fine)
         // So to use small exception blocks at all, all the blocks must be small, and there must be a limited amount of them
         var allAreSmall = exceptionBlocks.Count <= MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION
            && exceptionBlocks.All( excBlock =>
            {
               return excBlock.TryLength <= Byte.MaxValue
                  && excBlock.HandlerLength <= Byte.MaxValue
                  && excBlock.TryOffset <= UInt16.MaxValue
                  && excBlock.HandlerOffset <= UInt16.MaxValue;
            } );

         var maxStack = methodIL.MaxStackSize;

         var excCount = exceptionBlocks.Count;
         var hasAnyExc = excCount > 0;
         isTinyHeader = arraySize < 64
            && !hasAnyExc
            && maxStack <= 8
            && ( localSig == null || localSig.Locals.Count == 0 );

         if ( isTinyHeader )
         {
            // Can use tiny header
            ++arraySize;
         }
         else
         {
            // Use fat header
            arraySize += FAT_HEADER_SIZE;
            if ( hasAnyExc )
            {
               // Skip to next boundary of 4
               arraySize = BitUtils.MultipleOf4( arraySize );
               var excBlockSize = allAreSmall ? SMALL_EXC_BLOCK_SIZE : LARGE_EXC_BLOCK_SIZE;
               var maxExcHandlersInOnSection = allAreSmall ? MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION : MAX_LARGE_EXC_HANDLERS_IN_ONE_SECTION;
               arraySize += BinaryUtils.AmountOfPagesTaken( excCount, maxExcHandlersInOnSection ) * METHOD_DATA_SECTION_HEADER_SIZE +
                  excCount * excBlockSize;
            }
         }

         exceptionSectionsAreLarge = hasAnyExc && !allAreSmall;

         wholeMethodByteCount = arraySize;

         if ( !isTinyHeader )
         {
            // Non-tiny headers must start at 4-byte boundary
            startIndex = (Int32) ( currentStreamPosition.RoundUpI64( 4 ) - currentStreamPosition );
            wholeMethodByteCount += startIndex;
         }
         else
         {
            startIndex = 0;
         }

         return ilCodeByteCount;
      }

      protected virtual Int32 WriteConstant(
         RowRawValueExtractionArguments args,
         Byte[] data
         )
      {
         return this.WriteConstantToArray( args.Array, data );
      }

      protected virtual Int32 WriteConstantToArray(
         ResizableArray<Byte> array,
         Byte[] data
         )
      {
         var idx = 0;
         array.WriteArray( ref idx, data );
         return idx;
      }

      protected virtual Int32 WriteEmbeddedManifestResoruce(
         RowRawValueExtractionArguments args,
         Byte[] data
         )
      {
         return this.WriteEmbeddedManifestResourceToArray( args.Array, data );
      }

      public virtual Int32 WriteEmbeddedManifestResourceToArray(
         ResizableArray<Byte> sink,
         Byte[] resource
         )
      {
         var idx = 0;
         sink.CurrentMaxCapacity = resource.Length + sizeof( Int32 );
         sink
            .WriteInt32LEToBytes( ref idx, resource.Length )
            .WriteArray( ref idx, resource );
         return idx;
      }
   }



   public abstract class AbstractWriterStreamHandlerImpl : AbstractWriterStreamHandler
   {
      private readonly UInt32 _startingIndex;
      [CLSCompliant( false )]
      protected UInt32 curIndex;

      internal AbstractWriterStreamHandlerImpl( UInt32 startingIndex )
      {
         this._startingIndex = startingIndex;
         this.curIndex = startingIndex;
      }

      public abstract String StreamName { get; }

      public virtual void WriteStream(
         Stream sink,
         ResizableArray<Byte> array,
         RawValueStorage<Int64> rawValuesBeforeStreams,
         RVAConverter rvaConverter
         )
      {
         this.DoWriteStream( sink, array );
      }

      public Int64 CurrentSize
      {
         get
         {
            return (Int64) BitUtils.MultipleOf4( this.curIndex );
         }
      }

      public Boolean Accessed
      {
         get
         {
            return this.curIndex > this._startingIndex;
         }
      }

      protected abstract void DoWriteStream( Stream sink, ResizableArray<Byte> array );
   }

   internal class DefaultWriterBLOBStreamHandler : AbstractWriterStreamHandlerImpl, WriterBLOBStreamHandler
   {
      private readonly IDictionary<Byte[], UInt32> _blobIndices;
      private readonly IList<Byte[]> _blobs;

      internal DefaultWriterBLOBStreamHandler()
         : base( 1 )
      {
         this._blobIndices = new Dictionary<Byte[], UInt32>( ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer );
         this._blobs = new List<Byte[]>();
      }

      public override String StreamName
      {
         get
         {
            return MetaDataConstants.BLOB_STREAM_NAME;
         }
      }

      public Int32 RegisterBLOB( Byte[] blob )
      {
         UInt32 result;
         if ( blob == null )
         {
            result = 0;
         }
         else
         {
            if ( !this._blobIndices.TryGetValue( blob, out result ) )
            {
               result = this.curIndex;
               this._blobIndices.Add( blob, result );
               this._blobs.Add( blob );
               this.curIndex += (UInt32) blob.Length + (UInt32) BitUtils.GetEncodedUIntSize( blob.Length );
            }
         }

         return (Int32) result;
      }

      protected override void DoWriteStream(
         Stream sink,
         ResizableArray<Byte> array
         )
      {
         if ( this.Accessed )
         {
            sink.WriteByte( 0 );
            var idx = 0;
            if ( this._blobs.Count > 0 )
            {
               foreach ( var blob in this._blobs )
               {
                  idx = 0;
                  array.AddCompressedUInt32( ref idx, blob.Length );
                  sink.Write( array.Array, idx );
                  sink.Write( blob );
               }
            }

            var tmp = this.curIndex;
            sink.SkipToNextAlignment( ref tmp, 4 );
         }
      }
   }

   public class DefaultWriterGuidStreamHandler : AbstractWriterStreamHandlerImpl, WriterGUIDStreamHandler
   {
      private readonly IDictionary<Guid, UInt32> _guids;

      internal DefaultWriterGuidStreamHandler()
         : base( 0 )
      {
         this._guids = new Dictionary<Guid, UInt32>();
      }

      public override String StreamName
      {
         get
         {
            return MetaDataConstants.GUID_STREAM_NAME;
         }
      }

      public Int32 RegisterGUID( Guid? guid )
      {
         UInt32 result;
         if ( guid.HasValue )
         {
            result = this._guids.GetOrAdd_NotThreadSafe( guid.Value, g =>
            {
               var retVal = (UInt32) this._guids.Count + 1;
               this.curIndex += MetaDataConstants.GUID_SIZE;
               return retVal;
            } );
         }
         else
         {
            result = 0;
         }

         return (Int32) result;
      }

      protected override void DoWriteStream(
         Stream sink,
         ResizableArray<Byte> array
         )
      {
         if ( this.Accessed )
         {
            foreach ( var kvp in this._guids )
            {
               sink.Write( kvp.Key.ToByteArray() );
            }
         }
      }
   }

   public abstract class AbstractWriterStringStreamHandlerImpl : AbstractWriterStreamHandlerImpl, WriterStringStreamHandler
   {
      private readonly IDictionary<String, KeyValuePair<UInt32, Int32>> _strings;
      private readonly Encoding _encoding;

      internal AbstractWriterStringStreamHandlerImpl( Encoding encoding )
         : base( 1 )
      {
         this._encoding = encoding;
         this._strings = new Dictionary<String, KeyValuePair<UInt32, Int32>>();
      }

      public Int32 RegisterString( String str )
      {
         UInt32 result;
         if ( str == null )
         {
            result = 0;
         }
         else
         {
            KeyValuePair<UInt32, Int32> strInfo;
            if ( this._strings.TryGetValue( str, out strInfo ) )
            {
               result = strInfo.Key;
            }
            else
            {
               result = this.curIndex;
               this.AddString( str );
            }
         }
         return (Int32) result;
      }

      internal Int32 StringCount
      {
         get
         {
            return this._strings.Count;
         }
      }

      protected override void DoWriteStream(
         Stream sink,
         ResizableArray<Byte> array
         )
      {
         if ( this.Accessed )
         {
            sink.WriteByte( 0 );
            if ( this._strings.Count > 0 )
            {
               foreach ( var kvp in this._strings )
               {
                  var arrayLen = kvp.Value.Value;
                  array.CurrentMaxCapacity = arrayLen;
                  this.Serialize( kvp.Key, array );
                  sink.Write( array.Array, arrayLen );
               }
            }

            var tmp = this.curIndex;
            sink.SkipToNextAlignment( ref tmp, 4 );
         }
      }

      private void AddString( String str )
      {
         var byteCount = this.GetByteCountForString( str );
         this._strings.Add( str, new KeyValuePair<UInt32, Int32>( this.curIndex, byteCount ) );
         this.curIndex += (UInt32) byteCount;
      }

      protected Encoding Encoding
      {
         get
         {
            return this._encoding;
         }
      }

      protected abstract Int32 GetByteCountForString( String str );

      protected abstract void Serialize( String str, ResizableArray<Byte> byteArrayHelper );
   }

   public class DefaultWriterUserStringStreamHandler : AbstractWriterStringStreamHandlerImpl
   {
      internal DefaultWriterUserStringStreamHandler()
         : base( MetaDataConstants.USER_STRING_ENCODING )
      {

      }

      public override String StreamName
      {
         get
         {
            return MetaDataConstants.USER_STRING_STREAM_NAME;
         }
      }

      protected override Int32 GetByteCountForString( String str )
      {
         var retVal = str.Length * 2 // Each character is 2 bytes
            + 1; // Trailing byte (zero or 1)
         retVal += BitUtils.GetEncodedUIntSize( retVal ); // How many bytes it will take to compress the byte count
         return retVal;
      }

      protected override void Serialize( String str, ResizableArray<Byte> byteArrayHelper )
      {
         // Byte array helper has already been set up to hold array size
         var array = byteArrayHelper.Array;
         // Byte count
         var arrayIndex = 0;
         array.CompressUInt32( ref arrayIndex, str.Length * 2 + 1 );

         // Actual string
         Byte lastByte = 0;
         for ( var i = 0; i < str.Length; ++i )
         {
            var chr = str[i];
            array.WriteUInt16LEToBytes( ref arrayIndex, chr );
            // ECMA-335, p. 272
            if ( lastByte == 0 &&
             ( chr > 0x7E
                  || ( chr <= 0x2D
                     && ( ( chr >= 0x01 && chr <= 0x08 )
                        || ( chr >= 0x0E && chr <= 0x1F )
                        || chr == 0x27 || chr == 0x2D ) )
                  ) )
            {
               lastByte = 1;
            }
         }
         // Trailing byte (zero or 1)
         array[arrayIndex++] = lastByte;
      }


   }

   public class DefaultWriterSystemStringStreamHandler : AbstractWriterStringStreamHandlerImpl
   {
      public DefaultWriterSystemStringStreamHandler()
         : base( MetaDataConstants.SYS_STRING_ENCODING )
      {

      }

      public override String StreamName
      {
         get
         {
            return MetaDataConstants.SYS_STRING_STREAM_NAME;
         }
      }

      protected override Int32 GetByteCountForString( String str )
      {
         return this.Encoding.GetByteCount( str ) // Byte count for string
            + 1; // Trailing zero
      }

      protected override void Serialize( String str, ResizableArray<Byte> byteArrayHelper )
      {
         // Byte array helper has already been set up to hold array size
         var array = byteArrayHelper.Array;
         var byteCount = this.Encoding.GetBytes( str, 0, str.Length, array, 0 );
         // Remember trailing zero
         array[byteCount] = 0;
      }
   }

   public class DefaultWriterTableStreamHandler : WriterTableStreamHandler
   {
      private const Int32 TABLE_STREAM_RESERVED = 0;
      private const Byte TABLE_STREAM_RESERVED_2 = 1;
      private const Int64 SORTED_TABLES = 0x16003325FA00;

      private sealed class WriteDependantInfo
      {
         internal WriteDependantInfo(
            WritingOptions_TableStream writingOptions,
            ArrayQuery<Int32> tableSizes,
            ArrayQuery<TableSerializationInfo> infos,
            WriterMetaDataStreamContainer mdStreams,
            RawValueStorage<Int32> heapIndices
            )
         {

            var presentTables = 0;
            for ( var i = 0; i < tableSizes.Count; ++i )
            {
               if ( tableSizes[i] > 0 )
               {
                  ++presentTables;
               }
            }
            var hdrSize = 24 + 4 * presentTables;
            if ( writingOptions.HeaderExtraData.HasValue )
            {
               hdrSize += 4;
            }

            this.HeapIndices = heapIndices;
            this.ColumnSerializationSupportCreationArgs = new ColumnSerializationSupportCreationArgs( tableSizes, mdStreams.BLOBs.IsWide(), mdStreams.GUIDs.IsWide(), mdStreams.SystemStrings.IsWide() );
            this.Serialization = infos.Select( info => info.CreateSupport( this.ColumnSerializationSupportCreationArgs ) ).ToArrayProxy().CQ;
            this.HeaderSize = (UInt32) hdrSize;
            this.ContentSize = tableSizes.Select( ( size, idx ) => (UInt32) size * (UInt32) this.Serialization[idx].ColumnSerializationSupports.Sum( c => c.ColumnByteCount ) ).Sum();
            var totalSize = ( this.HeaderSize + this.ContentSize ).RoundUpU32( 4 );
            this.PaddingSize = totalSize - this.HeaderSize - this.ContentSize;
         }

         public RawValueStorage<Int32> HeapIndices { get; }

         public ColumnSerializationSupportCreationArgs ColumnSerializationSupportCreationArgs { get; }

         public ArrayQuery<TableSerializationFunctionality> Serialization { get; }

         public UInt32 HeaderSize { get; }

         public UInt32 ContentSize { get; }

         public UInt32 PaddingSize { get; }



      }

      private readonly CILMetaData _md;
      private readonly WritingOptions_TableStream _writingData;
      private WriteDependantInfo _writeDependantInfo;

      public DefaultWriterTableStreamHandler(
         CILMetaData md,
         WritingOptions_TableStream writingData,
         ArrayQuery<TableSerializationInfo> tableSerializations
         )
      {
         ArgumentValidator.ValidateNotNull( "Meta data", md );
         ArgumentValidator.ValidateNotNull( "Table serialization info", tableSerializations );

         this._md = md;
         this.TableSerializations = tableSerializations;
         this.TableSizes = tableSerializations.CreateTableSizeArray( md );
         this._writingData = writingData ?? new WritingOptions_TableStream();
      }

      public String StreamName
      {
         get
         {
            return MetaDataConstants.TABLE_STREAM_NAME;
         }
      }

      public Int64 CurrentSize
      {
         get
         {
            var writeInfo = this._writeDependantInfo;
            return writeInfo.HeaderSize + writeInfo.ContentSize + writeInfo.PaddingSize;
         }
      }

      public Boolean Accessed
      {
         get
         {
            // Always true, since we need to write table header.
            return true;
         }
      }


      public RawValueStorage<Int32> FillHeaps(
         RawValueStorage<Int64> rawValuesBeforeStreams,
         ArrayQuery<Byte> thisAssemblyPublicKeyIfPresentNull,
         WriterMetaDataStreamContainer mdStreams,
         ResizableArray<Byte> array
         )
      {
         var retVal = new RawValueStorage<Int32>( this.TableSizes, this.TableSerializations.Select( info => info.HeapValueColumnCount ) );
         foreach ( var info in this.TableSerializations )
         {
            info.ExtractTableHeapValues( this._md, retVal, mdStreams, array, thisAssemblyPublicKeyIfPresentNull );
         }

         Interlocked.Exchange( ref this._writeDependantInfo, new WriteDependantInfo( this._writingData, this.TableSizes, this.TableSerializations, mdStreams, retVal ) );

         return retVal;
      }

      public void WriteStream(
         Stream sink,
         ResizableArray<Byte> array,
         RawValueStorage<Int64> rawValuesBeforeStreams,
         RVAConverter rvaConverter
         )
      {
         var writeInfo = this._writeDependantInfo;

         // Header
         array.CurrentMaxCapacity = (Int32) writeInfo.HeaderSize;
         var headerSize = this.WriteTableHeader( array );
         sink.Write( array.Array, headerSize );

         // Rows
         var heapIndices = writeInfo.HeapIndices;
         var tableSizes = this.TableSizes;
         foreach ( var info in this.TableSerializations )
         {
            MetaDataTable table;
            if ( this._md.TryGetByTable( info.Table, out table ) && table.RowCount > 0 )
            {
               var support = writeInfo.Serialization[(Int32) info.Table];
               var cols = support.ColumnSerializationSupports;
               array.CurrentMaxCapacity = cols.Sum( c => c.ColumnByteCount ) * tableSizes[(Int32) info.Table];
               var byteArray = array.Array;
               var valIdx = 0;
               var arrayIdx = 0;
               foreach ( var rawValue in info.GetAllRawValues( table, rawValuesBeforeStreams, heapIndices, rvaConverter ) )
               {
                  var col = cols[valIdx % cols.Count];
                  col.WriteValue( byteArray, arrayIdx, rawValue );
                  arrayIdx += col.ColumnByteCount;
                  ++valIdx;
               }

               sink.Write( byteArray, arrayIdx );
            }

         }
      }

      protected ArrayQuery<TableSerializationInfo> TableSerializations { get; }

      protected ArrayQuery<Int32> TableSizes { get; }

      private Int32 WriteTableHeader(
         ResizableArray<Byte> byteArray
         )
      {
         var mdStreamInfo = this._writeDependantInfo.ColumnSerializationSupportCreationArgs;
         var validBitvector = 0L;
         var tableSizes = this.TableSizes;
         for ( var i = this.TableSizes.Count - 1; i >= 0; --i )
         {
            validBitvector = validBitvector << 1;
            if ( tableSizes[i] > 0 )
            {
               validBitvector |= 1;
            }
         }

         var thFlags = (TableStreamFlags) 0;
         if ( mdStreamInfo.IsWide( HeapIndexKind.String ) )
         {
            thFlags |= TableStreamFlags.WideStrings;
         }
         if ( mdStreamInfo.IsWide( HeapIndexKind.GUID ) )
         {
            thFlags |= TableStreamFlags.WideGUID;
         }
         if ( mdStreamInfo.IsWide( HeapIndexKind.BLOB ) )
         {
            thFlags |= TableStreamFlags.WideBLOB;
         }
         var headers = this._writingData;
         var extraData = headers.HeaderExtraData;
         if ( extraData.HasValue )
         {
            thFlags |= TableStreamFlags.ExtraData;
         }

         var idx = 0;
         var array = byteArray.Array;
         array
             .WriteInt32LEToBytes( ref idx, TABLE_STREAM_RESERVED )
            .WriteByteToBytes( ref idx, headers.HeaderMajorVersion ?? 2 )
            .WriteByteToBytes( ref idx, headers.HeaderMinorVersion ?? 0 )
            .WriteByteToBytes( ref idx, (Byte) thFlags )
            .WriteByteToBytes( ref idx, TABLE_STREAM_RESERVED_2 )
            .WriteInt64LEToBytes( ref idx, validBitvector )
            .WriteInt64LEToBytes( ref idx, SORTED_TABLES );

         for ( var i = 0; i < tableSizes.Count; ++i )
         {
            if ( tableSizes[i] > 0 )
            {
               array.WriteInt32LEToBytes( ref idx, tableSizes[i] );
            }
         }

         if ( extraData.HasValue )
         {
            array.WriteInt32LEToBytes( ref idx, extraData.Value );
         }

         return idx;
      }
   }
}