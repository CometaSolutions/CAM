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

      public virtual Int32 GetSectionCount( ImageFileMachine machine )
      {
         var retVal = this.GetMinimumSectionCount();
         if ( !machine.RequiresPE64() )
         {
            ++retVal; // Relocs
         }
         return retVal;
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

      public virtual void PopulateSections(
         WritingStatus writingStatus,
         IEnumerable<AbstractWriterStreamHandler> presentStreams,
         SectionHeader[] sections,
         out RVAConverter rvaConverter,
         out CLIHeader cliHeader,
         out MetaDataRoot mdRoot,
         out Int32 mdRootSize,
         out Int32 mdSize
         )
      {
         // MetaData
         mdRoot = this.CreateMDRoot( presentStreams, out mdRootSize, out mdSize );

         // Sections
         this.PopulateSections( writingStatus, presentStreams, mdRoot, mdSize, sections );

         // RVA converter
         rvaConverter = this.CreateRVAConverter( sections );

         // CLI Header
         var cliHeaderOptions = this._headers.CLIOptions.HeaderOptions;
         var mResInfo = writingStatus.EmbeddedManifestResourcesInfo;
         var snVars = writingStatus.StrongNameVariables;
         var cliHeaderSize = this.GetCLIHeaderSize();
         cliHeader = new CLIHeader(
               (UInt32) cliHeaderSize,
               (UInt16) ( cliHeaderOptions.MajorRuntimeVersion ?? 2 ),
               (UInt16) ( cliHeaderOptions.MinorRuntimeVersion ?? 5 ),
               new DataDirectory( (UInt32) rvaConverter.ToRVA( this.GetMetaDataOffset( writingStatus, cliHeaderSize ) ), (UInt32) mdSize ),
               cliHeaderOptions.ModuleFlags ?? ModuleFlags.ILOnly,
               cliHeaderOptions.EntryPointToken,
               mResInfo == null ? default( DataDirectory ) : new DataDirectory( (UInt32) rvaConverter.ToRVA( mResInfo.Item1 ), (UInt32) mResInfo.Item2 ),
               snVars == null ? default( DataDirectory ) : new DataDirectory( rvaConverter.ToRVANullable( this.GetStrongNameOffset( writingStatus, cliHeaderSize ) ), (UInt32) ( writingStatus.StrongNameVariables?.SignatureSize + writingStatus.StrongNameVariables?.SignaturePaddingSize ).GetValueOrDefault() ),
               default( DataDirectory ), // TODO: customize code manager
               default( DataDirectory ), // TODO: customize vtable fixups
               default( DataDirectory ), // TODO: customize exported address table jumps
               default( DataDirectory ) // TODO: customize managed native header
               );
      }

      public virtual void BeforeMetaData(
         Stream stream,
         ArrayQuery<SectionHeader> sections,
         WritingStatus writingStatus,
         RVAConverter rvaConverter,
         MetaDataRoot mdRoot,
         CLIHeader cliHeader
         )
      {
         // TODO: Write CLI header, and skip amount of bytes required for strong name signature
      }

      public virtual void WriteMDRoot(
         MetaDataRoot mdRoot,
         ResizableArray<Byte> array
         )
      {
         // Array capacity set by writing process
         mdRoot.WriteMetaDataRoot( array );
      }

      public virtual void AfterMetaData(
         Stream stream,
         ArrayQuery<SectionHeader> sections,
         WritingStatus writingStatus,
         RVAConverter rvaConverter
         )
      {
         // TODO: debug directory, startup stub, reloc section
      }

      protected CILMetaData MetaData { get; }

      protected MetaDataSerializationSupportProvider MDSerialization { get; }

      protected ArrayQuery<TableSerializationInfo> TableSerializations { get; }

      protected ArrayQuery<Int32> TableSizes { get; }

      protected virtual Int32 GetMinimumSectionCount()
      {
         return 1;
      }

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

      protected virtual RVAConverter CreateRVAConverter( IEnumerable<SectionHeader> headers )
      {
         return new DefaultRVAConverter( headers );
      }

      protected virtual void PopulateSections(
         WritingStatus writingStatus,
         IEnumerable<AbstractWriterStreamHandler> presentStreams,
         MetaDataRoot mdRoot,
         Int32 mdSize,
         SectionHeader[] sections
         )
      {
         var encoding = Encoding.UTF8;
         var snVars = writingStatus.StrongNameVariables;
         // 1st section - .text
         var virtualSize = (UInt32) (
            writingStatus.OffsetAfterInitialRawValues.Value
            + this.GetCLIHeaderSize()
            + mdSize
            + ( snVars == null ? 0 : ( snVars.SignatureSize + snVars.SignaturePaddingSize ) )
            + this.GetImportDirectorySize( writingStatus )
            + this.GetDebugDirectorySize()
            - writingStatus.InitialOffset
            );
         var fAlign = (UInt32) writingStatus.FileAlignment;
         var sAlign = (UInt32) writingStatus.SectionAlignment;
         var curVA = sAlign;
         var curPointer = (UInt32) writingStatus.InitialOffset;
         var curRawSize = virtualSize.RoundUpU32( fAlign );
         sections[0] = new SectionHeader(
            encoding.GetBytes( ".text" ).ToArrayProxy().CQ,
            virtualSize,
            curVA,
            curRawSize,
            curPointer,
            0,
            0,
            0,
            0,
            SectionHeaderCharacteristics.Memory_Execute | SectionHeaderCharacteristics.Memory_Read | SectionHeaderCharacteristics.Contains_Code
            );
         curVA = ( curVA + virtualSize ).RoundUpU32( sAlign );
         curPointer += curRawSize;

         // 2nd section - .rsrc
         // TODO

         // 3rd section - .reloc
         if ( !writingStatus.Machine.RequiresPE64() )
         {
            virtualSize = (UInt32) this.GetRelocSectionSize();
            curRawSize = virtualSize.RoundUpU32( fAlign );
            sections[1] = new SectionHeader(
               encoding.GetBytes( ".reloc" ).ToArrayProxy().CQ,
               virtualSize,
               curVA,
               curRawSize,
               curPointer,
               0,
               0,
               0,
               0,
               SectionHeaderCharacteristics.Memory_Read | SectionHeaderCharacteristics.Memory_Discardable | SectionHeaderCharacteristics.Contains_InitializedData
               );
            curVA = ( curVA + virtualSize ).RoundUpU32( sAlign );
            curPointer += curRawSize;
         }
      }

      protected virtual Int32 GetCLIHeaderSize()
      {
         return 0x48;
      }

      protected virtual Int32 GetStreamHeaderSize( AbstractWriterStreamHandler stream )
      {
         return 0x08 + stream.StreamName.Length.RoundUpI32( 4 );
      }

      protected virtual Int32 GetImportDirectorySize(
         WritingStatus writingStatus
         )
      {
         var options = this._headers;
         return writingStatus.Machine.RequiresPE64() ?
            0 :
            ( 0x3C + GetZeroTerminatedASCIIByteSize( options.PEOptions.ImportDirectoryName ) + GetZeroTerminatedASCIIByteSize( options.PEOptions.ImportHintName ) );
      }

      protected virtual Int32 GetDebugDirectorySize()
      {
         // TODO
         return 0;
      }

      protected virtual Int32 GetRelocSectionSize()
      {
         return 0x0C;
      }

      protected Int32 GetZeroTerminatedASCIIByteSize( String str )
      {
         return String.IsNullOrEmpty( str ) ? 1 : ( str.Length + 1 );
      }

      protected virtual MetaDataRoot CreateMDRoot(
         IEnumerable<AbstractWriterStreamHandler> presentStreams,
         out Int32 mdRootSize,
         out Int32 mdSize
         )
      {
         var mdOptions = this._headers.CLIOptions.MDRootOptions;
         var mdVersionBytes = MetaDataRoot.GetVersionStringBytes( mdOptions.VersionString );
         var streamNamesBytes = presentStreams
            .Select( mds => Tuple.Create( mds.StreamName.CreateASCIIBytes( 4 ), (UInt32) mds.CurrentSize ) )
            .ToArray();
         var streamOffset = (UInt32) ( 0x14 + mdVersionBytes.Count + streamNamesBytes.Sum( sh => 0x08 + sh.Item1.Count ) );
         mdRootSize = (Int32) streamOffset;

         var retVal = new MetaDataRoot(
            mdOptions.Signature ?? 0x424A5342,
            (UInt16) ( mdOptions.MajorVersion ?? 0x0001 ),
            (UInt16) ( mdOptions.MinorVersion ?? 0x0001 ),
            mdOptions.Reserved ?? 0x00000000,
            (UInt32) mdVersionBytes.Count,
            mdVersionBytes,
            mdOptions.StorageFlags ?? (StorageFlags) 0,
            mdOptions.Reserved2 ?? 0,
            (UInt16) streamNamesBytes.Length,
            streamNamesBytes
               .Select( b =>
               {
                  var streamSize = b.Item2;
                  var hdr = new MetaDataStreamHeader( streamOffset, streamSize, b.Item1 );
                  streamOffset += streamSize;
                  return hdr;
               } )
               .ToArrayProxy().CQ
             );
         mdSize = (Int32) streamOffset;
         return retVal;
      }

      protected virtual Int64 GetStrongNameOffset( WritingStatus status, Int32 cliHeaderSize )
      {
         // Write strong name signature right after CLI header
         return status.OffsetAfterInitialRawValues.Value + cliHeaderSize;
      }

      protected virtual Int64 GetMetaDataOffset( WritingStatus status, Int32 cliHeaderSize )
      {
         // Write meta data right after strong name signature
         var snVars = status.StrongNameVariables;
         return this.GetStrongNameOffset( status, cliHeaderSize ) + ( snVars?.SignatureSize + snVars?.SignaturePaddingSize ).GetValueOrDefault();
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
         if ( this.Accessed )
         {
            this.DoWriteStream( sink, array );
         }
      }

      public Int32 CurrentSize
      {
         get
         {
            return (Int32) this.curIndex.RoundUpU32( 4 );
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
         foreach ( var kvp in this._guids )
         {
            sink.Write( kvp.Key.ToByteArray() );
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
               var byteCount = this.GetByteCountForString( str );
               this._strings.Add( str, new KeyValuePair<UInt32, Int32>( this.curIndex, byteCount ) );
               this.curIndex += (UInt32) byteCount;
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
      private const Int64 SORTED_TABLES = 0x16003325FA00;

      private sealed class WriteDependantInfo
      {
         internal WriteDependantInfo(
            WritingOptions_TableStream writingOptions,
            ArrayQuery<Int32> tableSizes,
            ArrayQuery<TableSerializationInfo> infos,
            WriterMetaDataStreamContainer mdStreams,
            RawValueStorage<Int32> heapIndices,
            MetaDataTableStreamHeader header
            )
         {

            var presentTables = header.TableSizes.Count( s => s > 0 );
            var hdrSize = 24 + 4 * presentTables;
            if ( writingOptions.HeaderExtraData.HasValue )
            {
               hdrSize += 4;
            }

            this.HeapIndices = heapIndices;
            var args = new ColumnSerializationSupportCreationArgs( tableSizes, mdStreams.BLOBs.IsWide(), mdStreams.GUIDs.IsWide(), mdStreams.SystemStrings.IsWide() );
            this.Serialization = infos.Select( info => info.CreateSupport( args ) ).ToArrayProxy().CQ;
            this.HeaderSize = (UInt32) hdrSize;
            this.ContentSize = tableSizes.Select( ( size, idx ) => (UInt32) size * (UInt32) this.Serialization[idx].ColumnSerializationSupports.Sum( c => c.ColumnByteCount ) ).Sum();
            var totalSize = ( this.HeaderSize + this.ContentSize ).RoundUpU32( 4 );
            this.PaddingSize = totalSize - this.HeaderSize - this.ContentSize;
            this.Header = header;
         }

         public RawValueStorage<Int32> HeapIndices { get; }


         public ArrayQuery<TableSerializationFunctionality> Serialization { get; }

         public UInt32 HeaderSize { get; }

         public UInt32 ContentSize { get; }

         public UInt32 PaddingSize { get; }

         public MetaDataTableStreamHeader Header { get; }

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

      public Int32 CurrentSize
      {
         get
         {
            var writeInfo = this._writeDependantInfo;
            return (Int32) ( writeInfo.HeaderSize + writeInfo.ContentSize + writeInfo.PaddingSize );
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
         ResizableArray<Byte> array,
         out MetaDataTableStreamHeader header
         )
      {
         var retVal = new RawValueStorage<Int32>( this.TableSizes, this.TableSerializations.Select( info => info.HeapValueColumnCount ) );
         foreach ( var info in this.TableSerializations )
         {
            info.ExtractTableHeapValues( this._md, retVal, mdStreams, array, thisAssemblyPublicKeyIfPresentNull );
         }

         // Create table stream header
         var options = this._writingData;
         header = new MetaDataTableStreamHeader(
            options.Reserved ?? 0,
            options.HeaderMajorVersion ?? 2,
            options.HeaderMinorVersion ?? 0,
            CreateTableStreamFlags( mdStreams ),
            options.Reserved2 ?? 1,
            this.GetPresentTablesBitVector(),
            SORTED_TABLES, // TODO customize this
            this.TableSizes.Select( s => (UInt32) s ).ToArrayProxy().CQ,
            options.HeaderExtraData
            );

         Interlocked.Exchange( ref this._writeDependantInfo, new WriteDependantInfo( this._writingData, this.TableSizes, this.TableSerializations, mdStreams, retVal, header ) );

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
         var headerSize = WriteTableHeader( array, writeInfo.Header );
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

      private static Int32 WriteTableHeader(
         ResizableArray<Byte> byteArray,
         MetaDataTableStreamHeader header
         )
      {
         var idx = 0;
         var array = byteArray.Array;
         array
            .WriteInt32LEToBytes( ref idx, header.Reserved )
            .WriteByteToBytes( ref idx, header.MajorVersion )
            .WriteByteToBytes( ref idx, header.MinorVersion )
            .WriteByteToBytes( ref idx, (Byte) header.TableStreamFlags )
            .WriteByteToBytes( ref idx, header.Reserved2 )
            .WriteUInt64LEToBytes( ref idx, header.PresentTablesBitVector )
            .WriteUInt64LEToBytes( ref idx, header.SortedTablesBitVector );

         var tableSizes = header.TableSizes;
         for ( var i = 0; i < tableSizes.Count; ++i )
         {
            if ( tableSizes[i] > 0 )
            {
               array.WriteUInt32LEToBytes( ref idx, tableSizes[i] );
            }
         }
         var extraData = header.ExtraData;
         if ( extraData.HasValue )
         {
            array.WriteInt32LEToBytes( ref idx, extraData.Value );
         }

         return idx;
      }

      private TableStreamFlags CreateTableStreamFlags( WriterMetaDataStreamContainer streams )
      {
         var retVal = (TableStreamFlags) 0;
         if ( streams.SystemStrings.IsWide() )
         {
            retVal |= TableStreamFlags.WideStrings;
         }
         if ( streams.GUIDs.IsWide() )
         {
            retVal |= TableStreamFlags.WideGUID;
         }
         if ( streams.BLOBs.IsWide() )
         {
            retVal |= TableStreamFlags.WideBLOB;
         }

         if ( this._writingData.HeaderExtraData.HasValue )
         {
            retVal |= TableStreamFlags.ExtraData;
         }

         return retVal;
      }

      private UInt64 GetPresentTablesBitVector()
      {
         var validBitvector = 0UL;
         var tableSizes = this.TableSizes;
         for ( var i = this.TableSizes.Count - 1; i >= 0; --i )
         {
            validBitvector = validBitvector << 1;
            if ( tableSizes[i] > 0 )
            {
               validBitvector |= 1;
            }
         }

         return validBitvector;
      }
   }
}