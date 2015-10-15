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

namespace CILAssemblyManipulator.Physical.IO
{
   public class DefaultWriterFunctionalityProvider : WriterFunctionalityProvider
   {
      public virtual WriterFunctionality GetFunctionality(
         CILMetaData md,
         HeadersData headers,
         out CILMetaData newMD
         )
      {
         newMD = null;
         return new DefaultWriterFunctionality( md, headers );
      }
   }

   public class DefaultWriterFunctionality : WriterFunctionality
   {
      private readonly HeadersData _headers;

      public DefaultWriterFunctionality(
         CILMetaData md,
         HeadersData headers
         )
      {
         this.MetaData = md;
         this._headers = headers;
      }

      public virtual WriterConstantsHandler CreateConstantsHandler()
      {
         return new DefaultWriterConstantsHandler();
      }

      public virtual WriterILHandler CreateILHandler()
      {
         return new DefaultWriterILHandler( this.MetaData );
      }

      public virtual WriterManifestResourceHandler CreateManifestResourceHandler()
      {
         return new DefaultWriterManifestResourceHandler();
      }

      public virtual IEnumerable<AbstractWriterStreamHandler> CreateStreamHandlers( WritingData writingData )
      {
         yield return new DefaultWriterTableStreamHandler( this.MetaData, writingData, this._headers );
         yield return new DefaultWriterSystemStringStreamHandler();
         yield return new DefaultWriterBLOBStreamHandler();
         yield return new DefaultWriterGuidStreamHandler();
         yield return new DefaultWriterUserStringStreamHandler();
      }

      protected CILMetaData MetaData { get; }
   }
   public class DefaultWriterILHandler : WriterILHandler
   {
      private const Int32 METHOD_DATA_SECTION_HEADER_SIZE = 4;
      private const Int32 SMALL_EXC_BLOCK_SIZE = 12;
      private const Int32 LARGE_EXC_BLOCK_SIZE = 24;
      private const Int32 MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION = ( Byte.MaxValue - METHOD_DATA_SECTION_HEADER_SIZE ) / SMALL_EXC_BLOCK_SIZE; // 20
      private const Int32 MAX_LARGE_EXC_HANDLERS_IN_ONE_SECTION = ( 0x00FFFFFF - METHOD_DATA_SECTION_HEADER_SIZE ) / LARGE_EXC_BLOCK_SIZE; // 699050
      private const Int32 FAT_HEADER_SIZE = 12;

      private readonly CILMetaData _md;

      public DefaultWriterILHandler( CILMetaData md )
      {
         ArgumentValidator.ValidateNotNull( "Meta data", md );

         this._md = md;
      }
      public virtual Int32 WriteMethodIL(
         ResizableArray<Byte> sink,
         MethodILDefinition il,
         WriterStringStreamHandler userStrings,
         out Boolean isTinyHeader
         )
      {
         var md = this._md;
         var lIdx = il.LocalsSignatureIndex;
         var locals = lIdx.HasValue && lIdx.Value.Table == Tables.StandaloneSignature ?
            md.StandaloneSignatures.TableContents[lIdx.Value.Index].Signature as LocalVariablesSignature :
            null;
         Boolean exceptionSectionsAreLarge; Int32 wholeMethodByteCount;
         var ilCodeByteCount = CalculateByteSizeForMethod( il, locals, out isTinyHeader, out exceptionSectionsAreLarge, out wholeMethodByteCount );
         var exceptionBlocks = il.ExceptionBlocks;
         var hasAnyExceptions = exceptionBlocks.Count > 0;

         sink.CurrentMaxCapacity = wholeMethodByteCount;
         var array = sink.Array;
         var idx = 0;

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
         out Boolean isTinyHeader,
         out Boolean exceptionSectionsAreLarge,
         out Int32 wholeMethodByteCount
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

         return ilCodeByteCount;

         //bytes.EnsureSize( arraySize );
         //this._array = bytes.Array;
         //this._arrayIndex = 0;
      }
   }

   public class DefaultWriterManifestResourceHandler : WriterManifestResourceHandler
   {
      public virtual Int32 WriteEmbeddedManifestResource( ResizableArray<Byte> sink, Byte[] resource )
      {
         var idx = 0;
         sink.CurrentMaxCapacity = resource.Length + sizeof( Int32 );
         sink
            .WriteInt32LEToBytes( ref idx, resource.Length )
            .WriteArray( ref idx, resource );
         return idx;
      }
   }

   public class DefaultWriterConstantsHandler : WriterConstantsHandler
   {
      public virtual Int32 WriteConstant( ResizableArray<Byte> sink, Byte[] constant )
      {
         var idx = 0;
         sink.WriteArray( ref idx, constant );
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

      public abstract void WriteStream( Stream sink );

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

      public override void WriteStream( Stream sink )
      {
         if ( this.Accessed )
         {
            sink.WriteByte( 0 );
            var helper = new ResizableArray<Byte>( 4 );
            var idx = 0;
            if ( this._blobs.Count > 0 )
            {
               foreach ( var blob in this._blobs )
               {
                  idx = 0;
                  helper.AddCompressedUInt32( ref idx, blob.Length );
                  sink.Write( helper.Array, idx );
                  sink.Write( blob );
               }
            }

            var tmp = this.curIndex;
            sink.SkipToNextAlignment( ref tmp, 4 );
         }
      }
   }

   public class DefaultWriterGuidStreamHandler : AbstractWriterStreamHandlerImpl, WriterGuidStreamHandler
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

      public override void WriteStream( Stream sink )
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

      public override void WriteStream( Stream sink )
      {
         if ( this.Accessed )
         {
            sink.WriteByte( 0 );
            if ( this._strings.Count > 0 )
            {
               var byteArrayHelper = new ResizableArray<Byte>();
               foreach ( var kvp in this._strings )
               {
                  var arrayLen = kvp.Value.Value;
                  byteArrayHelper.CurrentMaxCapacity = arrayLen;
                  this.Serialize( kvp.Key, byteArrayHelper );
                  sink.Write( byteArrayHelper.Array, arrayLen );
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
            CILMetaData md,
            HeadersData headers,
            Object[] heapInfos,
            Boolean sysStringWide,
            Boolean blobWide,
            Boolean guidWide
            )
         {
            var tableSizes = new Int32[Consts.AMOUNT_OF_TABLES];
            for ( var i = 0; i < Consts.AMOUNT_OF_TABLES; ++i )
            {
               MetaDataTable tbl;
               if ( md.TryGetByTable( (Tables) i, out tbl ) )
               {
                  tableSizes[i] = tbl.RowCount;
               }
            }

            var tableWidths = new Int32[tableSizes.Length];
            var presentTables = 0;
            for ( var i = 0; i < tableWidths.Length; ++i )
            {
               if ( tableSizes[i] > 0 )
               {
                  ++presentTables;
                  tableWidths[i] = MetaDataConstants.CalculateTableWidth(
                     (Tables) i,
                     tableSizes,
                     sysStringWide,
                     guidWide,
                     blobWide
                     );
               }
            }
            var hdrSize = 24 + 4 * presentTables;
            if ( headers.TablesHeaderExtraData.HasValue )
            {
               hdrSize += 4;
            }

            this.HeapInfos = heapInfos;
            this.TableSizes = tableSizes;
            this.TableWidths = tableWidths;
            this.SystemStringIsWide = sysStringWide;
            this.BLOBIsWide = blobWide;
            this.GUIDIsWide = guidWide;
            this.HeaderSize = (UInt32) hdrSize;
            this.ContentSize = tableSizes.Select( ( size, idx ) => (UInt32) size * (UInt32) tableWidths[idx] ).Sum();
            var totalSize = BitUtils.MultipleOf4( this.HeaderSize + this.ContentSize );
            this.PaddingSize = totalSize - this.HeaderSize - this.ContentSize;
         }

         public UInt32 HeaderSize { get; }

         public UInt32 ContentSize { get; }

         public UInt32 PaddingSize { get; }

         public Object[] HeapInfos { get; }

         public Int32[] TableSizes { get; }

         public Int32[] TableWidths { get; }

         public Boolean SystemStringIsWide { get; }

         public Boolean BLOBIsWide { get; }

         public Boolean GUIDIsWide { get; }
      }

      private readonly CILMetaData _md;
      private readonly WritingData _writingData;
      private readonly HeadersData _headers;
      private WriteDependantInfo _writeDependantInfo;

      public DefaultWriterTableStreamHandler(
         CILMetaData md,
         WritingData writingData,
         HeadersData headers
         )
      {
         ArgumentValidator.ValidateNotNull( "Meta data", md );

         this._md = md;
         this._writingData = writingData;
         this._headers = headers;
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


      public void FillHeaps(
         Byte[] thisAssemblyPublicKeyIfPresentNull,
         WriterBLOBStreamHandler blobs,
         WriterStringStreamHandler sysStrings,
         WriterGuidStreamHandler guids,
         IEnumerable<AbstractWriterStreamHandler> otherStreams
         )
      {
         var byteArrayHelper = new ResizableArray<Byte>();
         var md = this._md;
         var heapInfos = new Object[Consts.AMOUNT_OF_TABLES];
         var auxHelper = new ResizableArray<Byte>(); // For writing security BLOBs
                                                     // 0x00 Module
         ProcessTableForHeaps4( md.ModuleDefinitions, heapInfos, mod => new HeapInfo4( sysStrings.RegisterString( mod.Name ), guids.RegisterGUID( mod.ModuleGUID ), guids.RegisterGUID( mod.EditAndContinueGUID ), guids.RegisterGUID( mod.EditAndContinueBaseGUID ) ) );
         // 0x01 TypeRef
         ProcessTableForHeaps2( md.TypeReferences, heapInfos, tr => new HeapInfo2( sysStrings.RegisterString( tr.Name ), sysStrings.RegisterString( tr.Namespace ) ) );
         // 0x02 TypeDef
         ProcessTableForHeaps2( md.TypeDefinitions, heapInfos, td => new HeapInfo2( sysStrings.RegisterString( td.Name ), sysStrings.RegisterString( td.Namespace ) ) );
         // 0x04 FieldDef
         ProcessTableForHeaps2( md.FieldDefinitions, heapInfos, f => new HeapInfo2( sysStrings.RegisterString( f.Name ), blobs.RegisterBLOB( byteArrayHelper.CreateFieldSignature( f.Signature ) ) ) );
         // 0x06 MethodDef
         ProcessTableForHeaps2( md.MethodDefinitions, heapInfos, m => new HeapInfo2( sysStrings.RegisterString( m.Name ), blobs.RegisterBLOB( byteArrayHelper.CreateMethodSignature( m.Signature ) ) ) );
         // 0x08 Parameter
         ProcessTableForHeaps1( md.ParameterDefinitions, heapInfos, ( p, idx ) => new HeapInfo1( sysStrings.RegisterString( p.Name ) ) );
         // 0x0A MemberRef
         ProcessTableForHeaps2( md.MemberReferences, heapInfos, m => new HeapInfo2( sysStrings.RegisterString( m.Name ), blobs.RegisterBLOB( byteArrayHelper.CreateMemberRefSignature( m.Signature ) ) ) );
         // 0x0B Constant
         ProcessTableForHeaps1( md.ConstantDefinitions, heapInfos, ( c, idx ) => new HeapInfo1( blobs.RegisterBLOB( byteArrayHelper.CreateConstantBytes( c.Value ) ) ) );
         // 0x0C CustomAttribute
         ProcessTableForHeaps1( md.CustomAttributeDefinitions, heapInfos, ( ca, idx ) => new HeapInfo1( blobs.RegisterBLOB( byteArrayHelper.CreateCustomAttributeSignature( md, idx ) ) ) );
         // 0x0D FieldMarshal
         ProcessTableForHeaps1( md.FieldMarshals, heapInfos, ( fm, idx ) => new HeapInfo1( blobs.RegisterBLOB( byteArrayHelper.CreateMarshalSpec( fm.NativeType ) ) ) );
         // 0x0E Security definitions
         ProcessTableForHeaps1( md.SecurityDefinitions, heapInfos, ( sd, idx ) => new HeapInfo1( blobs.RegisterBLOB( byteArrayHelper.CreateSecuritySignature( sd, auxHelper ) ) ) );
         // 0x11 Standalone sig
         ProcessTableForHeaps1( md.StandaloneSignatures, heapInfos, ( s, idx ) => new HeapInfo1( blobs.RegisterBLOB( byteArrayHelper.CreateStandaloneSignature( s ) ) ) );
         // 0x14 Event
         ProcessTableForHeaps1( md.EventDefinitions, heapInfos, ( e, idx ) => new HeapInfo1( sysStrings.RegisterString( e.Name ) ) );
         // 0x17 Property
         ProcessTableForHeaps2( md.PropertyDefinitions, heapInfos, p => new HeapInfo2( sysStrings.RegisterString( p.Name ), blobs.RegisterBLOB( byteArrayHelper.CreatePropertySignature( p.Signature ) ) ) );
         // 0x1A ModuleRef
         ProcessTableForHeaps1( md.ModuleReferences, heapInfos, ( mr, idx ) => new HeapInfo1( sysStrings.RegisterString( mr.ModuleName ) ) );
         // 0x1B TypeSpec
         ProcessTableForHeaps1( md.TypeSpecifications, heapInfos, ( t, idx ) => new HeapInfo1( blobs.RegisterBLOB( byteArrayHelper.CreateTypeSignature( t.Signature ) ) ) );
         // 0x1C ImplMap
         ProcessTableForHeaps1( md.MethodImplementationMaps, heapInfos, ( mim, idx ) => new HeapInfo1( sysStrings.RegisterString( mim.ImportName ) ) );
         // 0x20 Assembly
         ProcessTableForHeaps3( md.AssemblyDefinitions, heapInfos, ad =>
         {
            var pk = ad.AssemblyInformation.PublicKeyOrToken;
            return new HeapInfo3( blobs.RegisterBLOB( pk.IsNullOrEmpty() ? thisAssemblyPublicKeyIfPresentNull : pk ), sysStrings.RegisterString( ad.AssemblyInformation.Name ), sysStrings.RegisterString( ad.AssemblyInformation.Culture ) );
         } );
         // 0x21 AssemblyRef
         ProcessTableForHeaps4( md.AssemblyReferences, heapInfos, ar => new HeapInfo4( blobs.RegisterBLOB( ar.AssemblyInformation.PublicKeyOrToken ), sysStrings.RegisterString( ar.AssemblyInformation.Name ), sysStrings.RegisterString( ar.AssemblyInformation.Culture ), blobs.RegisterBLOB( ar.HashValue ) ) );
         // 0x26 File
         ProcessTableForHeaps2( md.FileReferences, heapInfos, f => new HeapInfo2( sysStrings.RegisterString( f.Name ), blobs.RegisterBLOB( f.HashValue ) ) );
         // 0x27 ExportedType
         ProcessTableForHeaps2( md.ExportedTypes, heapInfos, e => new HeapInfo2( sysStrings.RegisterString( e.Name ), sysStrings.RegisterString( e.Namespace ) ) );
         // 0x28 ManifestResource
         ProcessTableForHeaps1( md.ManifestResources, heapInfos, ( m, idx ) => new HeapInfo1( sysStrings.RegisterString( m.Name ) ) );
         // 0x2A GenericParameter
         ProcessTableForHeaps1( md.GenericParameterDefinitions, heapInfos, ( g, idx ) => new HeapInfo1( sysStrings.RegisterString( g.Name ) ) );
         // 0x2B MethosSpec
         ProcessTableForHeaps1( md.MethodSpecifications, heapInfos, ( m, idx ) => new HeapInfo1( blobs.RegisterBLOB( byteArrayHelper.CreateMethodSpecSignature( m.Signature ) ) ) );


         Interlocked.Exchange( ref this._writeDependantInfo, new WriteDependantInfo( md, this._headers, heapInfos, sysStrings.IsWide(), blobs.IsWide(), guids.IsWide() ) );
      }

      public void WriteStream( Stream sink )
      {
         var writeInfo = this._writeDependantInfo;

         // Header
         var byteArrayHelper = this.WriteTableHeader( writeInfo );
         sink.Write( byteArrayHelper.Array, (Int32) writeInfo.HeaderSize );

         // Rows
         this.WriteTableRows( sink, writeInfo, byteArrayHelper );
      }

      private static void ProcessTableForHeaps1<T>( MetaDataTable<T> table, Object[] heapInfos, Func<T, Int32, HeapInfo1> heapInfoExtractor )
         where T : class
      {
         var list = table.TableContents;
         var heapInfoList = new List<HeapInfo1>( list.Count );
         for ( var i = 0; i < list.Count; ++i )
         {
            heapInfoList.Add( heapInfoExtractor( list[i], i ) );
         }
         heapInfos[(Int32) table.TableKind] = heapInfoList;
      }

      private static void ProcessTableForHeaps2<T>( MetaDataTable<T> table, Object[] heapInfos, Func<T, HeapInfo2> heapInfoExtractor )
         where T : class
      {
         var list = table.TableContents;
         var heapInfoList = new List<HeapInfo2>( list.Count );
         foreach ( var row in list )
         {
            heapInfoList.Add( heapInfoExtractor( row ) );
         }
         heapInfos[(Int32) table.TableKind] = heapInfoList;
      }

      private static void ProcessTableForHeaps3<T>( MetaDataTable<T> table, Object[] heapInfos, Func<T, HeapInfo3> heapInfoExtractor )
         where T : class
      {
         var list = table.TableContents;
         var heapInfoList = new List<HeapInfo3>( list.Count );
         foreach ( var row in list )
         {
            heapInfoList.Add( heapInfoExtractor( row ) );
         }
         heapInfos[(Int32) table.TableKind] = heapInfoList;
      }

      private static void ProcessTableForHeaps4<T>( MetaDataTable<T> table, Object[] heapInfos, Func<T, HeapInfo4> heapInfoExtractor )
         where T : class
      {
         var list = table.TableContents;
         var heapInfoList = new List<HeapInfo4>( list.Count );
         foreach ( var row in list )
         {
            heapInfoList.Add( heapInfoExtractor( row ) );
         }
         heapInfos[(Int32) table.TableKind] = heapInfoList;
      }



      private struct HeapInfo1
      {
         internal readonly Int32 Heap1;

         internal HeapInfo1( Int32 heap1 )
         {
            this.Heap1 = heap1;
         }
      }

      private struct HeapInfo2
      {
         internal readonly Int32 Heap1;
         internal readonly Int32 Heap2;

         internal HeapInfo2( Int32 heap1, Int32 heap2 )
         {
            this.Heap1 = heap1;
            this.Heap2 = heap2;
         }
      }

      private struct HeapInfo3
      {
         internal readonly Int32 Heap1;
         internal readonly Int32 Heap2;
         internal readonly Int32 Heap3;

         internal HeapInfo3( Int32 heap1, Int32 heap2, Int32 heap3 )
         {
            this.Heap1 = heap1;
            this.Heap2 = heap2;
            this.Heap3 = heap3;
         }
      }

      private struct HeapInfo4
      {
         internal readonly Int32 Heap1;
         internal readonly Int32 Heap2;
         internal readonly Int32 Heap3;
         internal readonly Int32 Heap4;

         internal HeapInfo4( Int32 heap1, Int32 heap2, Int32 heap3, Int32 heap4 )
         {
            this.Heap1 = heap1;
            this.Heap2 = heap2;
            this.Heap3 = heap3;
            this.Heap4 = heap4;
         }
      }

      private ResizableArray<Byte> WriteTableHeader(
         WriteDependantInfo writeInfo
         )
      {
         var byteArray = new ResizableArray<Byte>( (Int32) writeInfo.HeaderSize );
         var validBitvector = 0L;
         var presentTables = 0;
         var tableSizes = writeInfo.TableSizes;
         for ( var i = Consts.AMOUNT_OF_TABLES - 1; i >= 0; --i )
         {
            validBitvector = validBitvector << 1;
            if ( tableSizes[i] > 0 )
            {
               validBitvector |= 1;
               ++presentTables;
            }
         }

         var tableStreamHeaderSize = 24 + 4 * presentTables;
         var thFlags = (TableStreamFlags) 0;
         if ( writeInfo.SystemStringIsWide )
         {
            thFlags |= TableStreamFlags.WideStrings;
         }
         if ( writeInfo.GUIDIsWide )
         {
            thFlags |= TableStreamFlags.WideGUID;
         }
         if ( writeInfo.BLOBIsWide )
         {
            thFlags |= TableStreamFlags.WideBLOB;
         }
         var headers = this._headers;
         var extraData = headers.TablesHeaderExtraData;
         if ( extraData.HasValue )
         {
            thFlags |= TableStreamFlags.ExtraData;
            tableStreamHeaderSize += 4;
         }

         var idx = 0;
         var array = byteArray.Array;
         array
             .WriteInt32LEToBytes( ref idx, TABLE_STREAM_RESERVED )
            .WriteByteToBytes( ref idx, headers.TableHeapMajor )
            .WriteByteToBytes( ref idx, headers.TableHeapMinor )
            .WriteByteToBytes( ref idx, (Byte) thFlags )
            .WriteByteToBytes( ref idx, TABLE_STREAM_RESERVED_2 )
            .WriteInt64LEToBytes( ref idx, validBitvector )
            .WriteInt64LEToBytes( ref idx, SORTED_TABLES );

         for ( var i = 0; i < tableSizes.Length; ++i )
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

         return byteArray;
      }

      private void WriteTableRows(
         Stream sink,
         WriteDependantInfo writeInfo,
         ResizableArray<Byte> byteArrayHelper
         )
      {
         var md = this._md;
         var tableSizes = writeInfo.TableSizes;
         var tableWidths = writeInfo.TableWidths;
         var tRefWidths = MetaDataConstants.GetCodedTableIndexSizes( tableSizes );
         var sysStrings = writeInfo.SystemStringIsWide;
         var guids = writeInfo.GUIDIsWide;
         var blobs = writeInfo.BLOBIsWide;
         var data = this._writingData;

#pragma warning disable 618
         // ECMA-335, p. 239
         ForEachElement<ModuleDefinition, HeapInfo4>( md.ModuleDefinitions, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, module, heapInfo ) => array
            .WriteInt16LEToBytes( ref idx, module.Generation ) // Generation
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            .WriteHeapIndex( ref idx, guids, heapInfo.Heap2 ) // MvId
            .WriteHeapIndex( ref idx, guids, heapInfo.Heap3 ) // EncId
            .WriteHeapIndex( ref idx, guids, heapInfo.Heap4 ) // EncBaseId
            );
         // ECMA-335, p. 247
         // TypeRef may contain types which result in duplicate rows - avoid that
         ForEachElement<TypeReference, HeapInfo2>( md.TypeReferences, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, typeRef, heapInfo ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.ResolutionScope, typeRef.ResolutionScope, tRefWidths ) // ResolutionScope
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // TypeName
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap2 ) // TypeNamespace
            );
         // ECMA-335, p. 243
         ForEachElement<TypeDefinition, HeapInfo2>( md.TypeDefinitions, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, typeDef, heapInfo ) => array
            .WriteInt32LEToBytes( ref idx, (Int32) typeDef.Attributes ) // Flags
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // TypeName
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap2 ) // TypeNamespace
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeDefOrRef, typeDef.BaseType, tRefWidths ) // Extends
            .WriteSimpleTableIndex( ref idx, typeDef.FieldList, tableSizes, Tables.Field ) // FieldList
            .WriteSimpleTableIndex( ref idx, typeDef.MethodList, tableSizes, Tables.MethodDef ) // MethodList
            );
         ForEachElement( md.FieldDefinitionPointers, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, item ) => array
             .WriteSimpleTableIndex( ref idx, item.FieldIndex, tableSizes, Tables.Field ) // Field
         );
         // ECMA-335, p. 223
         ForEachElement<FieldDefinition, HeapInfo2>( md.FieldDefinitions, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, fDef, heapInfo ) => array
           .WriteInt16LEToBytes( ref idx, (Int16) fDef.Attributes ) // FieldAttributes
           .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
           .WriteHeapIndex( ref idx, blobs, heapInfo.Heap2 ) // Signature
            );
         ForEachElement( md.MethodDefinitionPointers, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, item ) => array
             .WriteSimpleTableIndex( ref idx, item.MethodIndex, tableSizes, Tables.MethodDef ) // Method
         );
         // ECMA-335, p. 233
         ForEachElement<MethodDefinition, HeapInfo2>( md.MethodDefinitions, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, mDef, heapInfo ) => array
            .WriteInt32LEToBytes( ref idx, data.MethodRVAs[listIdx] ) // RVA
            .WriteInt16LEToBytes( ref idx, (Int16) mDef.ImplementationAttributes ) // ImplFlags
            .WriteInt16LEToBytes( ref idx, (Int16) mDef.Attributes ) // Flags
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap2 ) // Signature
            .WriteSimpleTableIndex( ref idx, mDef.ParameterList, tableSizes, Tables.Parameter ) // ParamList
            );
         ForEachElement( md.ParameterDefinitionPointers, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, item ) => array
             .WriteSimpleTableIndex( ref idx, item.ParameterIndex, tableSizes, Tables.Parameter ) // Parameter
         );
         // ECMA-335, p. 240
         ForEachElement<ParameterDefinition, HeapInfo1>( md.ParameterDefinitions, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, pDef, heapInfo ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) pDef.Attributes ) // Flags
            .WriteUInt16LEToBytes( ref idx, (UInt16) pDef.Sequence ) // Sequence
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            );
         // ECMA-335, p. 231
         ForEachElement( md.InterfaceImplementations, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, item ) => array
            .WriteSimpleTableIndex( ref idx, item.Class, tableSizes, Tables.TypeDef ) // Class
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeDefOrRef, item.Interface, tRefWidths ) // Interface
            );
         // ECMA-335, p. 232
         ForEachElement<MemberReference, HeapInfo2>( md.MemberReferences, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, mRef, heapInfo ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MemberRefParent, mRef.DeclaringType, tRefWidths ) // Class
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap2 ) // Signature
            );
         // ECMA-335, p. 216
         ForEachElement<ConstantDefinition, HeapInfo1>( md.ConstantDefinitions, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, constant, heapInfo ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) constant.Type ) // Type
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasConstant, constant.Parent, tRefWidths ) // Parent
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap1 ) // Value
            );
         // ECMA-335, p. 216
         ForEachElement<CustomAttributeDefinition, HeapInfo1>( md.CustomAttributeDefinitions, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, ca, heapInfo ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasCustomAttribute, ca.Parent, tRefWidths ) // Parent
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.CustomAttributeType, ca.Type, tRefWidths ) // Type
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap1 )
            );
         // ECMA-335, p.226
         ForEachElement<FieldMarshal, HeapInfo1>( md.FieldMarshals, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, fm, heapInfo ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasFieldMarshal, fm.Parent, tRefWidths ) // Parent
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap1 ) // NativeType
            );
         // ECMA-335, p. 218
         ForEachElement<SecurityDefinition, HeapInfo1>( md.SecurityDefinitions, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, sec, heapInfo ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) sec.Action ) // Action
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasDeclSecurity, sec.Parent, tRefWidths ) // Parent
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap1 ) // PermissionSet
            );
         // ECMA-335 p. 215
         ForEachElement( md.ClassLayouts, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, cl ) => array
            .WriteUInt16LEToBytes( ref idx, (UInt16) cl.PackingSize ) // PackingSize
            .WriteInt32LEToBytes( ref idx, cl.ClassSize ) // ClassSize
            .WriteSimpleTableIndex( ref idx, cl.Parent, tableSizes, Tables.TypeDef ) // Parent
            );
         // ECMA-335 p. 225
         ForEachElement( md.FieldLayouts, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, fl ) => array
            .WriteInt32LEToBytes( ref idx, fl.Offset ) // Offset
            .WriteSimpleTableIndex( ref idx, fl.Field, tableSizes, Tables.Field ) // Field
            );
         // ECMA-335 p. 243
         ForEachElement<StandaloneSignature, HeapInfo1>( md.StandaloneSignatures, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, sig, heapInfo ) => array
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap1 ) // Signature
            );
         // ECMA-335 p. 220
         ForEachElement( md.EventMaps, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, em ) => array
            .WriteSimpleTableIndex( ref idx, em.Parent, tableSizes, Tables.TypeDef ) // Parent
            .WriteSimpleTableIndex( ref idx, em.EventList, tableSizes, Tables.Event ) // EventList
            );
         ForEachElement( md.EventDefinitionPointers, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, item ) => array
             .WriteSimpleTableIndex( ref idx, item.EventIndex, tableSizes, Tables.Event ) // Event
         );
         // ECMA-335 p. 221
         ForEachElement<EventDefinition, HeapInfo1>( md.EventDefinitions, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, evt, heapInfo ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) evt.Attributes ) // EventFlags
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeDefOrRef, evt.EventType, tRefWidths ) // EventType
            );
         // ECMA-335 p. 242
         ForEachElement( md.PropertyMaps, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, pm ) => array
            .WriteSimpleTableIndex( ref idx, pm.Parent, tableSizes, Tables.TypeDef ) // Parent
            .WriteSimpleTableIndex( ref idx, pm.PropertyList, tableSizes, Tables.Property ) // PropertyList
            );
         ForEachElement( md.PropertyDefinitionPointers, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, item ) => array
            .WriteSimpleTableIndex( ref idx, item.PropertyIndex, tableSizes, Tables.Property ) // Property
         );
         // ECMA-335 p. 242
         ForEachElement<PropertyDefinition, HeapInfo2>( md.PropertyDefinitions, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, prop, heapInfo ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) prop.Attributes ) // Flags
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap2 ) // Type
            );
         // ECMA-335 p. 237
         ForEachElement( md.MethodSemantics, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, ms ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) ms.Attributes ) // Semantics
            .WriteSimpleTableIndex( ref idx, ms.Method, tableSizes, Tables.MethodDef ) // Method
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasSemantics, ms.Associaton, tRefWidths ) // Association
            );
         // ECMA-335 p. 237
         ForEachElement( md.MethodImplementations, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, mi ) => array
            .WriteSimpleTableIndex( ref idx, mi.Class, tableSizes, Tables.TypeDef ) // Class
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MethodDefOrRef, mi.MethodBody, tRefWidths ) // MethodBody
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MethodDefOrRef, mi.MethodDeclaration, tRefWidths ) // MethodDeclaration
            );
         // ECMA-335, p. 239
         ForEachElement<ModuleReference, HeapInfo1>( md.ModuleReferences, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, modRef, heapInfo ) => array
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            );
         // ECMA-335, p. 248
         ForEachElement<TypeSpecification, HeapInfo1>( md.TypeSpecifications, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, tSpec, heapInfo ) => array
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap1 ) // Signature
            );
         // ECMA-335, p. 230
         ForEachElement<MethodImplementationMap, HeapInfo1>( md.MethodImplementationMaps, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, mim, heapInfo ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) mim.Attributes ) // PInvokeAttributes
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MemberForwarded, mim.MemberForwarded, tRefWidths ) // MemberForwarded
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Import name
            .WriteSimpleTableIndex( ref idx, mim.ImportScope, tableSizes, Tables.ModuleRef ) // Import scope
            );
         // ECMA-335, p. 227
         ForEachElement( md.FieldRVAs, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, fRVA ) => array
            .WriteInt32LEToBytes( ref idx, data.FieldRVAs[listIdx] ) // RVA
            .WriteSimpleTableIndex( ref idx, fRVA.Field, tableSizes, Tables.Field ) // Field
            );
         ForEachElement( md.EditAndContinueLog, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, item ) => array
            .WriteInt32LEToBytes( ref idx, item.Token )
            .WriteInt32LEToBytes( ref idx, item.FuncCode )
         );
         ForEachElement( md.EditAndContinueMap, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, item ) => array
             .WriteInt32LEToBytes( ref idx, item.Token )
         );
         // ECMA-335, p. 211
         ForEachElement<AssemblyDefinition, HeapInfo3>( md.AssemblyDefinitions, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, ass, heapInfo ) => array
            .WriteInt32LEToBytes( ref idx, (Int32) ass.HashAlgorithm ) // HashAlgId
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.AssemblyInformation.VersionMajor ) // MajorVersion
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.AssemblyInformation.VersionMinor ) // MinorVersion
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.AssemblyInformation.VersionBuild ) // BuildNumber
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.AssemblyInformation.VersionRevision ) // RevisionNumber
            .WriteInt32LEToBytes( ref idx, (Int32) ass.Attributes ) // Flags
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap1 ) // PublicKey
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap2 ) // Name
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap3 ) // Culture
            );
         ForEachElement( md.AssemblyDefinitionProcessors, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, item ) => array
             .WriteInt32LEToBytes( ref idx, item.Processor )
         );
         ForEachElement( md.AssemblyDefinitionOSs, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, item ) => array
             .WriteInt32LEToBytes( ref idx, item.OSPlatformID )
             .WriteInt32LEToBytes( ref idx, item.OSMajorVersion )
             .WriteInt32LEToBytes( ref idx, item.OSMinorVersion )
         );
         // ECMA-335, p. 212
         ForEachElement<AssemblyReference, HeapInfo4>( md.AssemblyReferences, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, assRef, heapInfo ) => array
            .WriteUInt16LEToBytes( ref idx, (UInt16) assRef.AssemblyInformation.VersionMajor ) // MajorVersion
            .WriteUInt16LEToBytes( ref idx, (UInt16) assRef.AssemblyInformation.VersionMinor ) // MinorVersion
            .WriteUInt16LEToBytes( ref idx, (UInt16) assRef.AssemblyInformation.VersionBuild ) // BuildNumber
            .WriteUInt16LEToBytes( ref idx, (UInt16) assRef.AssemblyInformation.VersionRevision ) // RevisionNumber
            .WriteInt32LEToBytes( ref idx, (Int32) assRef.Attributes ) // Flags
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap1 ) // PublicKey
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap2 ) // Name
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap3 ) // Culture
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap4 ) // HashValue
            );
         ForEachElement( md.AssemblyReferenceProcessors, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, item ) => array
             .WriteInt32LEToBytes( ref idx, item.Processor )
             .WriteSimpleTableIndex( ref idx, item.AssemblyRef, tableSizes, Tables.AssemblyRef )
         );
         ForEachElement( md.AssemblyReferenceOSs, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, item ) => array
             .WriteInt32LEToBytes( ref idx, item.OSPlatformID )
             .WriteInt32LEToBytes( ref idx, item.OSMajorVersion )
             .WriteInt32LEToBytes( ref idx, item.OSMinorVersion )
             .WriteSimpleTableIndex( ref idx, item.AssemblyRef, tableSizes, Tables.AssemblyRef )
         );
         ForEachElement<FileReference, HeapInfo2>( md.FileReferences, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, file, heapInfo ) => array
            .WriteInt32LEToBytes( ref idx, (Int32) file.Attributes ) // Flags
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap2 ) // HashValue
            );
         ForEachElement<ExportedType, HeapInfo2>( md.ExportedTypes, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, eType, heapInfo ) => array
            .WriteInt32LEToBytes( ref idx, (Int32) eType.Attributes ) // TypeAttributes
            .WriteInt32LEToBytes( ref idx, eType.TypeDefinitionIndex ) // TypeDef index in other (!) assembly
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // TypeName
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap2 ) // TypeNamespace
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.Implementation, eType.Implementation, tRefWidths ) // Implementation
            );
         ForEachElement<ManifestResource, HeapInfo1>( md.ManifestResources, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, mRes, heapInfo ) => array
           .WriteInt32LEToBytes( ref idx, data.EmbeddedManifestResourceOffsets[listIdx] ?? mRes.Offset ) // Offset
           .WriteInt32LEToBytes( ref idx, (Int32) mRes.Attributes ) // Flags
           .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
           .WriteCodedTableIndex( ref idx, CodedTableIndexKind.Implementation, mRes.Implementation, tRefWidths ) // Implementation
            );
         // ECMA-335, p. 240
         ForEachElement( md.NestedClassDefinitions, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, nc ) => array
            .WriteSimpleTableIndex( ref idx, nc.NestedClass, tableSizes, Tables.TypeDef ) // NestedClass
            .WriteSimpleTableIndex( ref idx, nc.EnclosingClass, tableSizes, Tables.TypeDef ) // EnclosingClass
            );
         // ECMA-335, p. 228
         ForEachElement<GenericParameterDefinition, HeapInfo1>( md.GenericParameterDefinitions, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, gParam, heapInfo ) => array
            .WriteUInt16LEToBytes( ref idx, (UInt16) gParam.GenericParameterIndex ) // Number
            .WriteInt16LEToBytes( ref idx, (Int16) gParam.Attributes ) // Flags
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeOrMethodDef, gParam.Owner, tRefWidths ) // Owner
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            );
         // ECMA-335, p. 238
         ForEachElement<MethodSpecification, HeapInfo1>( md.MethodSpecifications, writeInfo, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, mSpec, heapInfo ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MethodDefOrRef, mSpec.Method, tRefWidths ) // Method
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap1 ) // Instantiation
            );
         // ECMA-335, p. 229
         ForEachElement( md.GenericParameterConstraintDefinitions, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, gConstraint ) => array
            .WriteSimpleTableIndex( ref idx, gConstraint.Owner, tableSizes, Tables.GenericParameter ) // Owner
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeDefOrRef, gConstraint.Constraint, tRefWidths ) // Constraint
            );
#pragma warning restore 618

         // Write padding to align to 4
         for ( var i = 0; i < writeInfo.PaddingSize; ++i )
         {
            sink.WriteByte( 0 );
         }
      }

      private void ForEachElement<T, U>(
         MetaDataTable<T> mdTable,
         WriteDependantInfo writeInfo,
         Int32[] tableWidths,
         Stream sink,
         ResizableArray<Byte> byteArrayHelper,
         Action<Byte[], Int32, Int32, T, U> writeAction
         )
         where T : class
      {
         var list = mdTable.TableContents;
         var count = list.Count;
         if ( count > 0 )
         {
            var tableEnum = mdTable.TableKind;
            Int32 width;
            var arrayLen = CheckArrayForTableEmitting( tableEnum, count, tableWidths, byteArrayHelper, out width );
            var idx = 0;
            var heapInfoList = (List<U>) writeInfo.HeapInfos[(Int32) tableEnum];
            var array = byteArrayHelper.Array;
            for ( var i = 0; i < count; ++i )
            {
               writeAction( array, idx, i, list[i], heapInfoList[i] );
               idx += width;
            }
            sink.Write( array, arrayLen );
#if DEBUG
            if ( idx != arrayLen )
            {
               throw new Exception( "Something went wrong when emitting metadata array: emitted " + idx + " instead of expected " + arrayLen + " bytes." );
            }
#endif
         }
      }

      private static void ForEachElement<T>(
         MetaDataTable<T> mdTable,
         Int32[] tableWidths,
         Stream sink,
         ResizableArray<Byte> byteArrayHelper,
         Action<Byte[], Int32, Int32, T> writeAction
         )
         where T : class
      {
         var list = mdTable.TableContents;
         var count = list.Count;
         if ( count > 0 )
         {
            var tableEnum = mdTable.TableKind;
            Int32 width;
            var arrayLen = CheckArrayForTableEmitting( tableEnum, count, tableWidths, byteArrayHelper, out width );
            var idx = 0;
            var array = byteArrayHelper.Array;
            for ( var i = 0; i < count; ++i )
            {
               writeAction( array, idx, i, list[i] );
               idx += width;
            }
            sink.Write( array, arrayLen );
#if DEBUG
            if ( idx != arrayLen )
            {
               throw new Exception( "Something went wrong when emitting metadata array: emitted " + idx + " instead of expected " + arrayLen + " bytes." );
            }
#endif
         }
      }

      private static Int32 CheckArrayForTableEmitting(
         Tables tableEnum,
         Int32 rowCount,
         Int32[] tableWidths,
         ResizableArray<Byte> byteArrayHelper,
         out Int32 width
         )
      {
         width = tableWidths[(Int32) tableEnum];
         var arrayLen = width * rowCount;
         byteArrayHelper.CurrentMaxCapacity = arrayLen;
         return arrayLen;
      }

   }
}

public static partial class E_CILPhysical
{
   internal static Byte[] WriteHeapIndex( this Byte[] array, ref Int32 idx, Boolean isWide, Int32 heapIndex )
   {
      return isWide ? array.WriteInt32LEToBytes( ref idx, heapIndex ) : array.WriteUInt16LEToBytes( ref idx, (UInt16) heapIndex );
   }

   internal static Byte[] WriteCodedTableIndex( this Byte[] array, ref Int32 idx, CodedTableIndexKind codedKind, TableIndex? tIdx, IDictionary<CodedTableIndexKind, Boolean> wideIndices )
   {
      return wideIndices[codedKind] ? array.WriteInt32LEToBytes( ref idx, MetaDataConstants.GetCodedTableIndex( codedKind, tIdx ) ) : array.WriteUInt16LEToBytes( ref idx, (UInt16) MetaDataConstants.GetCodedTableIndex( codedKind, tIdx ) );
   }

   internal static Byte[] WriteSimpleTableIndex( this Byte[] array, ref Int32 idx, TableIndex tIdx, Int32[] tableSizes, Tables presumedTable )
   {
      return tableSizes[(Int32) presumedTable] > UInt16.MaxValue ? array.WriteInt32LEToBytes( ref idx, ( tIdx.Index + 1 ) ) : array.WriteUInt16LEToBytes( ref idx, (UInt16) ( tIdx.Index + 1 ) );
   }
}