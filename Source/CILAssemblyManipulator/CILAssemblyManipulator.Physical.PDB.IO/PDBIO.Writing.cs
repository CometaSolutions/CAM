/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
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
 * See the License for the specific _language governing permissions and
 * limitations under the License. 
 */

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// PDB writing attempt.
// I got as far as:
// Emitting module streams correctly
// Emitting source streams correctly
// Emitting root stream, names stream, gsSymStream correctly (gsSymStream emitting might have minor bugs)
// Emitting /src/headerblock stream to be identical to the one where things still worked in original PDB.
// However, the .language element in IL Disassembler just refused to be set to correct value (it defaulted to C++), even after emitting /src/headerblock.
// Maybe there is some stream I am missing (I tried zeroing out all other 'extra' stream from original PDB and .language still worked) or I was doign things in wrong order, I don't know which.
// At that point I decided to just use the COM wrappers to write PDB.
//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using CommonUtils;

//namespace CILAssemblyManipulator.PDB
//{
//   public static partial class PDBIO
//   {
//      private const Int32 GUID_SIZE = 16;
//      private const Int32 FUNCTION_FIXED_SIZE = 37;
//      private const Int32 SLOT_FIXED_SIZE = 16;
//      private const Int32 SCOPE_FIXED_SIZE = 18;
//      private const Int32 DEFAULT_PAGE_SIZE = 0x200;
//      private const Int32 START_PAGE = 3; // Next two pages after the first page are for page allocation bits
//      private const Int32 FIXED_OEM_SIZE = GUID_SIZE + 8; // Block len + sym + GUID + type index
//      private const Int32 BLOCK_LEN_SIZE = SHORT_SIZE;
//      private const Int32 SOURCE_INFO_SIZE = 8; // Source name index, hash length byte (0), hash type byte (0), align to 4 byte boundary
//      private const Int32 LINE_FIXED_SIZE = 12; // Function offset (uint32), function segment (uint16), flags (uint16), unused (int32)
//      private const Int32 LINE_PER_SOURCE_MULTIPLIER = 12; // Source index (int32), line count (int32), byte count (int32)
//      private const Int32 SOURCE_INFO_START_OFFSET = 8; // Two ints before this - SYM_DEBUG_SOURCE_INFO and byte count
//      private const Int32 SYM_PROCREF = 0x1125;
//      private const Int32 SYM_TOKENREF = 0x1129;
//      private const Int32 PROCREF_FIXED_SIZE = 14;
//      private const UInt16 TOKENREF_FIXED_SIZE = 24;
//      private const Int32 MODULE_INFO_FIXED_SIZE = 64;
//      private const Int32 DBI_HEADER_SIZE = 64;
//      private const UInt32 SEC_CON_SIG = 0xF12EBA2D;
//      private const Int32 NAMES_STREAM_IDX = 6;
//      private const Int32 ROOT_STREAM_FIXED_SIZE = 40 + GUID_SIZE;
//      private const Int32 ROOT_STREAM_NAME_MULTIPLIER = 8;
//      private const UInt32 ROOT_STREAM_PDB_VERSION = 0x01312E94;
//      private const UInt64 ROOT_STREAM_END = 0x0132914100000000;
//      private const UInt32 NAME_INDEX_SIG = 0xFEEFFEEF;
//      private const UInt32 NAME_INDEX_VERSION = 0x00000001;
//      private const Int32 NAME_INDEX_FIXED_SIZE = 20;
//      private const Int32 HEADER_FIXED_SIZE = 52;
//      private const Int32 SEC_MAP_FIXED_SIZE = 44;
//      private const Int32 SEC_CON_MULTIPLIER = 28;
//      private const Int32 DBI_DEBUG_HEADER_SIZE = 22;
//      private const UInt32 TO_LOWER_MASK = 0x20202020u;
//      private const Int32 GS_SYM_BUCKETS = 4096;
//      private const UInt32 GS_SYM_HASH_INFO_MULTIPLIER = 12;
//      private const Int32 GS_SYM_REF_MULTIPLIER = 16; // 16, if both PROCREFs and TOKENREFs are stored
//      private const Int32 GS_SYM_HEADER_SIZE = 16;
//      private const String SRC_HEADER_BLOCK_NAME = "/src/headerblock";
//      private const UInt32 SRC_HEADER_BLOCK_SIG = 0x0130E21Bu; // 1998 08 27
//      private const Int32 SRC_HEADER_BLOCK_ENTRY_SIZE = 0x2C;
//      private const UInt32 SRC_HEADER_SRC_TYPE = 0x58;
//      private const UInt32 SRC_HEADER_OTHER_TYPE = 0x65;
//      private const Int32 SRC_HEADER_FIXED_SIZE = 0x50;

//      public static void WriteToStream( this PDBInstance instance, Stream writingStream, Int32 timeStamp = 0 )
//      {
//         // TODO when things stabilize, chop this big method into smaller methods.
//         var stream = new StreamInfo( writingStream );

//         var nameIndex = new Dictionary<String, Int32>();
//         var nameIndexAux = 1;
//         var dataStreams = new List<Tuple<Int32, Int32[]>>( Enumerable.Repeat<Tuple<Int32, Int32[]>>( null, 12 ) ); // First 12 streams reserved
//         var namedDataStreams = new Dictionary<String, Int32>();
//         var dbiHeader = new DBIHeader();

//         // Some fixed stream names and indices
//         namedDataStreams.Add( NAMES_STREAM_NAME, NAMES_STREAM_IDX );
//         namedDataStreams.Add( SRC_HEADER_BLOCK_NAME, 7 );

//         var pageSize = DEFAULT_PAGE_SIZE; // Each source is taking only 88 bytes, so it's gonna be less waste to just write with small page size.
//         var zeroesPageArray = new Byte[pageSize]; // The zero-array to fill page gaps quickly
//         // Seek to the start page.
//         stream.SeekToPage( pageSize, START_PAGE, 0 );

//         Byte[] array = null;
//         Int32 idx, arrayLen, startPage;

//         // Write symbol server data if needed
//         var srcSrv = instance.SourceServer;
//         if ( !String.IsNullOrEmpty( srcSrv ) )
//         {
//            array = NewArrayIfNeeded( array, NAME_ENCODING.GetByteCount( srcSrv ), out idx, out arrayLen );
//            array.WriteStringToBytes( ref idx, NAME_ENCODING, srcSrv );
//            stream.stream.Write( array, arrayLen );
//            namedDataStreams.Add( SOURCE_SERVER_STREAM_NAME, dataStreams.Count );
//            startPage = stream.GetCurrentPage( pageSize );
//            dataStreams.Add( Tuple.Create( arrayLen, stream.PagesFromContinuousStream( pageSize, startPage, zeroesPageArray ) ) );
//         }

//         // Emit all source streams.
//         var sources = instance.Sources.ToArray();
//         UInt32 tmpUInt32;
//         if ( sources.Length > 0 )
//         {
//            var srcInfos = new Tuple<Int32, Int32>[sources.Length];
//            for ( var i = 0; i < sources.Length; ++i )
//            {
//               var src = sources[i];
//               try
//               {
//                  startPage = stream.GetCurrentPage( pageSize );
//                  array = NewArrayIfNeeded( array, GUID_SIZE * 4, out idx, out arrayLen );
//                  // Language, vendor, doctype, hash algorithm, hash
//                  array.WriteGUIDToBytes( ref idx, src.Language )
//                     .WriteGUIDToBytes( ref idx, src.Vendor )
//                     .WriteGUIDToBytes( ref idx, src.DocumentType )
//                     .WriteGUIDToBytes( ref idx, src.HashAlgorithm );
//#if DEBUG
//                  if ( idx != arrayLen )
//                  {
//                     throw new PDBException( "Debyyg" );
//                  }
//#endif
//                  stream.stream.Write( array, arrayLen );
//                  var hash = src.Hash;
//                  if ( hash != null )
//                  {
//                     stream.stream.Write( hash );
//                  }
//                  namedDataStreams.Add( SOURCE_FILE_PREFIX + src.Name, dataStreams.Count );
//                  dataStreams.Add( Tuple.Create( arrayLen + hash.SafeLength(), stream.PagesFromContinuousStream( pageSize, startPage, zeroesPageArray ) ) );

//                  var nIdx = GetNameIndex( nameIndex, ref nameIndexAux, src.Name );
//                  var dummy = GetNameIndex( nameIndex, ref nameIndexAux, "" );
//                  var sIdx = GetNameIndex( nameIndex, ref nameIndexAux, src.Name.ToLowerInvariant() );
//                  srcInfos[i] = Tuple.Create( sIdx, nIdx );
//               }
//               catch ( Exception e )
//               {
//                  throw new PDBException( "Unhandled exception while writing source " + src + ".", e );
//               }
//            }

//            // Emit src/headerblock
//            var hashBucketCount = sources.Length * 2; // Starting count is this, this will guarantee that eventually there always will be enough hash buckets
//            var srcHash = new Tuple<Int32, Int32>[hashBucketCount];
//            foreach ( var srcInfo in srcInfos )
//            {
//               var hIdx = srcInfo.Item1 % hashBucketCount;
//               while ( hIdx < hashBucketCount && srcHash[hIdx] != null )
//               {
//                  ++hIdx;
//               }
//               if ( hIdx == hashBucketCount )
//               {
//                  // Overflow - make hash bucket count smaller
//                  // Find first free bucket
//                  while ( hIdx >= 0 && srcHash[hIdx] != null )
//                  {
//                     --hIdx;
//                  }
//                  if ( hIdx < 0 )
//                  {
//                     // Shouldn't be possible, since hash bucket size was twice as big as source size in the beginning.
//                     throw new PDBException( "Failed to hash source file names?" );
//                  }
//                  // New count to accomodate the overflowing item
//                  hashBucketCount = hIdx + 1;
//                  // Set the item
//                  srcHash[hIdx] = srcInfo;
//                  // Move all buckets which are now beyond the border
//                  ++hIdx;
//                  var curIdx = 0;
//                  while ( hIdx < srcHash.Length && srcHash[hIdx] != null )
//                  {
//                     // There should be enough free entries for all buckets to be moved because of the start size of hash bucket array.
//                     while ( srcHash[curIdx] != null )
//                     {
//                        ++curIdx;
//                     }
//                     srcHash[curIdx] = srcHash[hIdx];
//                     srcHash[hIdx] = null; // Remember to remove the old entry
//                     ++hIdx;
//                  }
//               }
//               else
//               {
//                  srcHash[hIdx] = srcInfo;
//               }
//            }

//            array = NewArrayIfNeeded( array, SRC_HEADER_FIXED_SIZE + AmountOfPagesTaken( hashBucketCount, 32 ) * INT_SIZE + SRC_HEADER_BLOCK_ENTRY_SIZE * sources.Length, out idx, out arrayLen );
//            array
//               .WriteUInt32LEToBytes( ref idx, SRC_HEADER_BLOCK_SIG ) // Starting signature
//               .WriteInt32LEToBytes( ref idx, arrayLen ) // Stream size
//               .ZeroesInt32( ref idx, 14 ) // The following 14 ints are ignored on read
//               .WriteInt32LEToBytes( ref idx, sources.Length ) // Amount of entries
//               .WriteInt32LEToBytes( ref idx, hashBucketCount ) // Amount of hash buckets
//               .WriteInt32LEToBytes( ref idx, AmountOfPagesTaken( hashBucketCount, 32 ) ); // How many int32's hash bucket present bitset takes
//            // Write hash bucket present set
//            tmpUInt32 = 0u;
//            for ( var i = 0; i < hashBucketCount; ++i )
//            {
//               if ( srcHash[i] != null )
//               {
//                  tmpUInt32 |= ( 1u << ( i % 32 ) );
//               }
//               if ( i + 1 == hashBucketCount || ( i != 0 && i % 32 == 0 ) )
//               {
//                  // Write UInt32
//                  array.WriteUInt32LEToBytes( ref idx, tmpUInt32 );
//               }
//            }
//            // Magic zero
//            array.ZeroesInt32( ref idx, 1 );
//            // Write entries
//            foreach ( var srcInfo in srcHash )
//            {
//               if ( srcInfo != null )
//               {
//                  array
//                     .WriteInt32LEToBytes( ref idx, srcInfo.Item1 ) // Search term name index - MSVS seems to want the lowercase version
//                     .WriteInt32LEToBytes( ref idx, SRC_HEADER_BLOCK_ENTRY_SIZE - INT_SIZE ) // The size of the entry
//                     .WriteUInt32LEToBytes( ref idx, SRC_HEADER_BLOCK_SIG ) // Signature, ignored on read
//                     .ZeroesInt32( ref idx, 1 ) // Checksum (?), ignored on read
//                     .WriteUInt32LEToBytes( ref idx, SRC_HEADER_SRC_TYPE ) // Source data format (?)
//                     .WriteInt32LEToBytes( ref idx, srcInfo.Item2 ) // File name index, ignored on read
//                     .ZeroesInt32( ref idx, 1 ) // Seems to be always name index to empty string, ignored on read
//                     .WriteInt32LEToBytes( ref idx, srcInfo.Item2 ) // Stream suffix name index
//                     .WriteUInt32LEToBytes( ref idx, SRC_HEADER_OTHER_TYPE ) // Some other format?
//                     .ZeroesInt32( ref idx, 2 );
//               }
//            }
//#if DEBUG
//            if ( idx != arrayLen )
//            {
//               throw new PDBException( "Debyyg" );
//            }
//#endif
//            // Write to stream
//            startPage = stream.GetCurrentPage( pageSize );
//            stream.stream.Write( array, arrayLen );
//            dataStreams[7] = Tuple.Create( arrayLen, stream.PagesFromContinuousStream( pageSize, startPage, zeroesPageArray ) );
//         }

//         // Write root stream (no more named streams will follow)
//         startPage = stream.GetCurrentPage( pageSize );

//         var strByteCount = namedDataStreams.Keys.Aggregate( 0, ( cur, str ) => cur + NAME_ENCODING.SafeByteCount( str ) );
//         // Amount of int32's needed to write
//         var namedDataStreamsPresentBitSetCount = AmountOfPagesTaken( namedDataStreams.Count, 32 );
//         array = NewArrayIfNeeded(
//            array,
//            ROOT_STREAM_FIXED_SIZE // Minimum size
//            + namedDataStreamsPresentBitSetCount * INT_SIZE // Present bit set length
//            + namedDataStreams.Count * ROOT_STREAM_NAME_MULTIPLIER // How many bytes for each name
//            + strByteCount, // How many bytes for strings
//            out idx,
//            out arrayLen
//            );
//         array
//            .WriteUInt32LEToBytes( ref idx, ROOT_STREAM_PDB_VERSION )
//            .WriteInt32LEToBytes( ref idx, timeStamp ) // Timestamp
//            .WriteUInt32LEToBytes( ref idx, instance.Age )
//            .WriteGUIDToBytes( ref idx, instance.DebugGUID )
//            .WriteInt32LEToBytes( ref idx, strByteCount );
//         var strStartIdx = idx;
//         var afterStrIdx = idx + strByteCount;
//         array
//            .WriteInt32LEToBytes( ref afterStrIdx, namedDataStreams.Count ) // Amount of named streams
//            .WriteInt32LEToBytes( ref afterStrIdx, namedDataStreams.Count ) // Amount of set bits (same as size)
//            .WriteInt32LEToBytes( ref afterStrIdx, namedDataStreamsPresentBitSetCount ); // How many int32's bit set takes
//         // Write set bits
//         for ( var i = 0; i < namedDataStreamsPresentBitSetCount; ++i )
//         {
//            var bits = UInt32.MaxValue;
//            if ( i == namedDataStreamsPresentBitSetCount - 1 )
//            {
//               bits >>= 32 - namedDataStreams.Count % 32;
//            }
//            array.WriteUInt32LEToBytes( ref afterStrIdx, bits );
//         }
//         // Write deleted bits size (0)
//         array.WriteInt32LEToBytes( ref afterStrIdx, 0 );
//         // Write strings and their indices
//         foreach ( var kvp in namedDataStreams )
//         {
//            // First, the index of string in array and its stream index
//            array
//               .WriteInt32LEToBytes( ref afterStrIdx, idx - strStartIdx )
//               .WriteInt32LEToBytes( ref afterStrIdx, kvp.Value );
//            // Then write the string
//            array.WriteZeroTerminatedString( ref idx, kvp.Key );
//         }
//         // Last two ints always same: zero and 0x01329141
//         array.WriteUInt64LEToBytes( ref afterStrIdx, ROOT_STREAM_END );
//#if DEBUG
//         if ( afterStrIdx != arrayLen )
//         {
//            throw new PDBException( "Debyyg" );
//         }
//#endif
//         stream.stream.Write( array, arrayLen );
//         dataStreams[1] = Tuple.Create( arrayLen, stream.PagesFromContinuousStream( pageSize, startPage, zeroesPageArray ) );

//         // Create array for the symbol record stream
//         var symRecStream = new Byte[instance.Modules
//            .SelectMany( m => m.Functions )
//            .Aggregate( 0, ( cur, f ) =>
//            {
//               cur += PROCREF_FIXED_SIZE + NAME_ENCODING.SafeByteCount( f.Name );
//               Align4( ref cur );
//               return cur + TOKENREF_FIXED_SIZE;
//            } )];
//         var symRecIdx = 0;

//         // Then emit all module streams
//         var funcOffset = 0u;
//         var moduleInfos = new List<Tuple<DBIModuleInfo, String[]>>( instance.Modules.Count() );
//         var funcSecContribs = new List<DBISecCon>( instance.Modules.Aggregate( 0, ( cur, m ) => cur + m.Functions.Count ) );
//         var gsSymAux = new Dictionary<UInt32, Object>(); // Key - name hash; Value - int32 or list of int32's.

//         foreach ( var module in instance.Modules )
//         {
//            try
//            {
//               var mInfo = new DBIModuleInfo( module.Name );
//               mInfo.files = (UInt16) module.Functions.SelectMany( f => f.Lines.Keys ).Distinct().Count();

//               dbiHeader.moduleInfoSize += MODULE_INFO_FIXED_SIZE + NAME_ENCODING.SafeByteCount( mInfo.moduleName ) + NAME_ENCODING.SafeByteCount( mInfo.objectName );
//               Align4( ref dbiHeader.moduleInfoSize );

//               startPage = stream.GetCurrentPage( pageSize );
//               // Signature
//               stream.stream.Write( new Byte[] { 4, 0, 0, 0 } );
//               var funcInfos = new List<PDBFunctionInfo>( module.Functions.Count );
//               var totalIdx = 4;
//               // Write symbols
//               foreach ( var func in module.Functions )
//               {
//                  var funcContrib = new DBISecCon();
//                  funcContrib.module = (UInt16) moduleInfos.Count;
//                  funcContrib.section = 1;
//                  funcContrib.size = func.Length;
//                  funcSecContribs.Add( funcContrib );

//                  var funcBlockLen = func.CalculateSymbolByteCount();
//                  // block len + global managed function SYM + fixed size + name + 4 byte boundary
//                  var preludeLength = 4 + FUNCTION_FIXED_SIZE + NAME_ENCODING.SafeByteCount( func.Name );
//                  Align4( ref preludeLength );
//                  array = NewArrayIfNeeded( array, preludeLength + funcBlockLen + 4, out idx, out arrayLen );
//                  var funcPointer = totalIdx;
//                  // Block length + SYM (global managed function) + function fixed data
//                  array.WriteUInt16LEToBytes( ref idx, 0 ) // block length
//                     .WriteUInt16LEToBytes( ref idx, SYM_GLOBAL_MANAGED_FUNC ) // global managed function
//                     .WriteInt32LEToBytes( ref idx, 0 ) // parent
//                     .WriteInt32LEToBytes( ref idx, totalIdx + preludeLength + funcBlockLen ) // function end pointer
//                     .WriteInt32LEToBytes( ref idx, 0 ) // next
//                     .WriteUInt32LEToBytes( ref idx, func.Length ) // length
//                     .WriteInt32LEToBytes( ref idx, 0 ) // debug start
//                     .WriteInt32LEToBytes( ref idx, 0 ) // debug end
//                     .WriteUInt32LEToBytes( ref idx, func.Token ) // token
//                     .WriteUInt32LEToBytes( ref idx, funcOffset ) // address
//                     .WriteUInt16LEToBytes( ref idx, 1 ) // segment
//                     .WriteByteToBytes( ref idx, 0 ) // flags
//                     .WriteUInt16LEToBytes( ref idx, 0 ) // returnReg
//                     .WriteZeroTerminatedString( ref idx, func.Name ) // name
//                     .Align4( ref idx ); // 4-byte border
//                  // Revisit block length
//                  array.WriteBlockLength( 0, idx );

//                  WriteScopeOrFunctionBlocks( func, array, ref idx, totalIdx, funcOffset, funcPointer );
//                  WriteOEM( func, array, ref idx, ref totalIdx, stream );
//                  WriteENDSym( array, ref idx );
//#if DEBUG
//                  if ( idx != arrayLen )
//                  {
//                     throw new PDBException( "Debyyg" );
//                  }
//#endif
//                  funcInfos.Add( new PDBFunctionInfo( func, funcOffset, 1, funcPointer ) );

//                  funcOffset += func.Length;
//                  totalIdx += idx;
//                  stream.stream.Write( array, arrayLen );
//               }

//               mInfo.symbolByteCount = totalIdx;

//               // Write source & line infos
//               var usedSources = module.Functions.SelectMany( f => f.Lines.Keys ).Distinct().ToArray();
//               var sourcesByteCount = usedSources.Length * SOURCE_INFO_SIZE;
//               array = NewArrayIfNeeded(
//                  array,
//                  SOURCE_INFO_START_OFFSET // SYM_DEBUG_SOURCE_INFO (int32) + byte size (int32)
//                  + sourcesByteCount // Source info count
//                  + module.Functions.Count * ( LINE_FIXED_SIZE + 16 ) // Line info per function count, including SYM_DEBUG_LINE_INFO (int32) and block byte size (int32), also (2x->)1x int32s for sym-rec-stream refs
//                  + module.Functions.Aggregate( 0, ( cur, f ) =>
//                     cur
//                     + f.Lines.Count * LINE_PER_SOURCE_MULTIPLIER // For each used source, source index + line count + byte count
//                     + f.Lines.Values.Aggregate( 0, ( cur2, l ) => cur2 + l.Count * LINE_MULTIPLIER + l.Where( ll => ll.ColumnEnd.HasValue && ll.ColumnStart.HasValue ).Count() * COLUMN_MULTIPLIER ) ) // For each line, line info + optional column info
//                  + 4, // One int32 for byte count of symrec stream refs
//                  out idx,
//                  out arrayLen
//                  );

//               // Write source info
//               array
//                  .WriteInt32LEToBytes( ref idx, SYM_DEBUG_SOURCE_INFO ) // Sources
//                  .WriteInt32LEToBytes( ref idx, sourcesByteCount ); // Amount of bytes
//               var dic = new Dictionary<String, Int32>();
//               foreach ( var src in usedSources )
//               {
//                  // Save information about the index of this source within this block
//                  dic.Add( src, idx - SOURCE_INFO_START_OFFSET );

//                  array
//                     .WriteInt32LEToBytes( ref idx, GetNameIndex( nameIndex, ref nameIndexAux, src ) ) // Name of the source
//                     .WriteInt32LEToBytes( ref idx, 0 ); // length-byte (0), kind-byte (0), padding
//               }

//               // Write line info
//               foreach ( var funcI in funcInfos )
//               {
//                  var func = funcI.function;
//                  var lenIdx = idx + 4;
//                  array
//                     .WriteInt32LEToBytes( ref idx, SYM_DEBUG_LINE_INFO ) // Line info
//                     .Skip( ref idx, 4 ) // Byte count of this block excluding sym + this count
//                     .WriteUInt32LEToBytes( ref idx, funcI.address ) // Function address
//                     .WriteUInt16LEToBytes( ref idx, 1 ) // Segment
//                     .WriteUInt16LEToBytes( ref idx, (UInt16) ( func.Lines.Values.SelectMany( ll => ll ).Any( l => l.ColumnStart.HasValue && l.ColumnEnd.HasValue ) ? 1 : 0 ) ) // Flags: 1 if there is even one line present with column information, 0 otherwise
//                     .WriteUInt32LEToBytes( ref idx, func.Length ); // Function length is duplicated here
//                  foreach ( var kvp in func.Lines )
//                  {
//                     var lines = kvp.Value;
//                     var lineByteSize = lines.Count * LINE_MULTIPLIER;
//                     array
//                        .WriteInt32LEToBytes( ref idx, dic[kvp.Key] ) // Source info index within its block
//                        .WriteInt32LEToBytes( ref idx, lines.Count ) // How many lines
//                        .WriteInt32LEToBytes( ref idx, LINE_PER_SOURCE_MULTIPLIER + lineByteSize + lines.Where( ll => ll.ColumnEnd.HasValue && ll.ColumnStart.HasValue ).Count() * COLUMN_MULTIPLIER ); // How many bytes the line info takes, including source info + line count
//                     var colIdx = idx + lineByteSize;
//                     foreach ( var line in lines )
//                     {
//                        // Write line information
//                        // Line-flags: low 3 bytes is line start, 7 bits of highest byte is line delta, and highest bit is whether line is statement
//                        var lineFlags = ( line.LineStart & 0x00ffffffu ) | ( ( ( line.LineEnd - line.LineStart ) << 24 ) & 0x7f000000u );
//                        if ( !line.IsStatement )
//                        {
//                           lineFlags |= 0x80000000u;
//                        }
//                        array
//                           .WriteUInt32LEToBytes( ref idx, line.Offset ) // Line offset
//                           .WriteUInt32LEToBytes( ref idx, lineFlags ); // Line flags
//                        if ( line.ColumnStart.HasValue && line.ColumnEnd.HasValue )
//                        {
//                           // Write column info
//                           array
//                              .WriteUInt16LEToBytes( ref colIdx, line.ColumnStart.Value )
//                              .WriteUInt16LEToBytes( ref colIdx, line.ColumnEnd.Value );
//                        }
//                     }
//                     // Set current index to point after columns
//                     idx = colIdx;
//                  }
//                  // Revisit byte count
//                  array.WriteInt32LEToBytes( ref lenIdx, idx - lenIdx - 4 );
//               }

//               mInfo.linesByteCount = idx;

//               // Write symrec stream refs
//               array
//                  .WriteInt32LEToBytes( ref idx, 8 * funcInfos.Count ); // Byte count for symrec stream refs

//               // Add module info here, before PROCREF & TOKENREF, since they use 1-based module indexing
//               moduleInfos.Add( Tuple.Create( mInfo, usedSources ) );

//               // Then write PROCREF & TOKENREF info
//               foreach ( var fInfo in funcInfos )
//               {
//                  // MSVS uses name-based lookup only with textual token values, therefore no need to write PROCREFs.
//                  // Record offset to module stream
//                  var sRef = symRecIdx;
//                  array
//                     .WriteInt32LEToBytes( ref idx, symRecIdx ); // Pointer to the start of this function's PROCREF

//                  // Write PROCREF to symrec stream
//                  var curSymRecIdx = symRecIdx;
//                  symRecStream
//                     .Skip( ref symRecIdx, 2 ) // Byte count, skip for now
//                     .WriteUInt16LEToBytes( ref symRecIdx, SYM_PROCREF ) // S_PROCREF
//                     .WriteInt32LEToBytes( ref symRecIdx, 0 ) // Checksum (always zero)
//                     .WriteInt32LEToBytes( ref symRecIdx, fInfo.funcPointer ) // Function pointer within the module stream
//                     .WriteUInt16LEToBytes( ref symRecIdx, (UInt16) moduleInfos.Count ); // Module index
//                  var tmp = symRecIdx;
//                  symRecStream.WriteZeroTerminatedString( ref symRecIdx, fInfo.function.Name ); // Function name
//                  // Hash function name
//                  AddToGSSym( gsSymAux, symRecStream, tmp, symRecIdx, sRef );

//                  // Pad to 4-byte boundary
//                  Align4( ref symRecIdx );
//                  // Revisit byte count
//                  symRecStream.WriteUInt16LEToBytes( ref curSymRecIdx, (UInt16) ( symRecIdx - curSymRecIdx - 2 ) );

//                  // Record offset to module stream
//                  sRef = symRecIdx;
//                  array.WriteInt32LEToBytes( ref idx, symRecIdx ); // Pointer to the start of this function's TOKENREF

//                  // Write TOKENREF to symrec stream
//                  //curSymRecIdx = symRecIdx;
//                  symRecStream
//                     .WriteUInt16LEToBytes( ref symRecIdx, TOKENREF_FIXED_SIZE - 2 ) // Byte count, always fixed since token ref fixed size (+ padding)
//                     .WriteUInt16LEToBytes( ref symRecIdx, SYM_TOKENREF ) // S_TOKENREF
//                     .WriteInt32LEToBytes( ref symRecIdx, 0 ) // Checksum (always zero)
//                     .WriteInt32LEToBytes( ref symRecIdx, fInfo.funcPointer ) // Function pointer within the module stream
//                     .WriteUInt16LEToBytes( ref symRecIdx, (UInt16) moduleInfos.Count ); // Module index
//                  tmp = symRecIdx;
//                  symRecStream.WriteZeroTerminatedString( ref symRecIdx, fInfo.function.Token.ToString( "x8" ) ); // Fixed-length hexadecimal token value, with lower-case letters (upper-case won't work).
//                  // Hash tokenref textual name
//                  AddToGSSym( gsSymAux, symRecStream, tmp, symRecIdx, sRef );

//                  Align4( ref symRecIdx );
//               }
//#if DEBUG
//               if ( idx != arrayLen )
//               {
//                  throw new PDBException( "Debyyg" );
//               }
//#endif
//               stream.stream.Write( array, arrayLen );
//               var linesAndSymRecRefsLength = arrayLen;

//               // Write module information to stream
//               mInfo.stream = (UInt16) dataStreams.Count;
//               dataStreams.Add( Tuple.Create( mInfo.symbolByteCount + linesAndSymRecRefsLength, stream.PagesFromContinuousStream( pageSize, startPage, zeroesPageArray ) ) );
//            }
//            catch ( Exception e )
//            {
//               throw new PDBException( "Unhandled exception while writing module " + module + ".", e );
//            }
//         }

//#if DEBUG
//         if ( symRecIdx != symRecStream.Length )
//         {
//            throw new PDBException( "Debyyg" );
//         }
//#endif
//         // Write symrec stream
//         startPage = stream.GetCurrentPage( pageSize );
//         stream.stream.Write( symRecStream );
//         dataStreams[10] = Tuple.Create( symRecStream.Length, stream.PagesFromContinuousStream( pageSize, startPage, zeroesPageArray ) );

//         // Write gSymStream (only if any functions)
//         tmpUInt32 = 0u;
//         if ( gsSymAux.Count > 0 )
//         {
//            startPage = stream.GetCurrentPage( pageSize );
//            var gsSymHashSize = GS_SYM_BUCKETS / 8 + ( gsSymAux.Count + 1 ) * 4;
//            array = NewArrayIfNeeded( array, GS_SYM_HEADER_SIZE + GS_SYM_REF_MULTIPLIER * funcSecContribs.Count + gsSymHashSize, out idx, out arrayLen );
//            array
//               .WriteUInt32LEToBytes( ref idx, UInt32.MaxValue ) // Sig
//               .WriteUInt32LEToBytes( ref idx, 0xF12F091A ) // Version or hash info?
//               .WriteInt32LEToBytes( ref idx, GS_SYM_REF_MULTIPLIER * funcSecContribs.Count ) // Byte count of references to symRecStream
//               .WriteInt32LEToBytes( ref idx, gsSymHashSize ); // Present buckets bitset + offsets
//            // References to symRecStream
//            var gsHashSorted = gsSymAux.Keys.ToArray();
//            Array.Sort( gsHashSorted, Comparer<UInt32>.Default );
//            foreach ( var gsHash in gsHashSorted )
//            {
//               var obj = gsSymAux[gsHash];
//               if ( obj is Int32 ) // Single reference.
//               {
//                  array
//                     .WriteInt32LEToBytes( ref idx, (Int32) obj ) // Reference to symRec (either PROCREF or TOKENREF)
//                     .WriteInt32LEToBytes( ref idx, 1 ); // Section (?)
//               }
//               else // List of references.
//               {
//                  // The list items are in ascending order since whenever a new value is added, it is always greater than prev values.
//                  // Iterate in descending order (not sure if really required, but it is how the values are written by MS writer)
//                  var list = (IList<Int32>) obj;
//                  for ( var i = list.Count; i > 0; --i )
//                  {
//                     array
//                        .WriteInt32LEToBytes( ref idx, list[i - 1] )
//                        .WriteInt32LEToBytes( ref idx, 1 );
//                  }
//               }
//            }
//            // Hash table present buckets bitset
//            var curGSHashIdx = 0;
//            for ( var i = 0; i < GS_SYM_BUCKETS / 32; ++i )
//            {
//               tmpUInt32 = 0u;
//               while ( curGSHashIdx < gsHashSorted.Length && gsHashSorted[curGSHashIdx] < ( i + 1 ) * 32 )
//               {
//                  tmpUInt32 |= ( 1u << (Int32) ( gsHashSorted[curGSHashIdx] % 32 ) );
//                  ++curGSHashIdx;
//               }
//               array.WriteUInt32LEToBytes( ref idx, tmpUInt32 );
//            }
//            // Magic zero
//            array.WriteInt32LEToBytes( ref idx, 0 );
//            // Write counts in each bucket
//            tmpUInt32 = 0u;
//            foreach ( var gsHash in gsHashSorted )
//            {
//               array.WriteUInt32LEToBytes( ref idx, tmpUInt32 );
//               tmpUInt32 += gsSymAux[gsHash] is Int32 ?
//                  GS_SYM_HASH_INFO_MULTIPLIER :
//                  ( GS_SYM_HASH_INFO_MULTIPLIER * (UInt32) ( (List<Int32>) gsSymAux[gsHash] ).Count );
//            }

//#if DEBUG
//            if ( idx != arrayLen )
//            {
//               throw new PDBException( "Debyyg" );
//            }
//#endif
//            stream.stream.Write( array, arrayLen );
//            dataStreams[8] = Tuple.Create( arrayLen, stream.PagesFromContinuousStream( pageSize, startPage, zeroesPageArray ) );
//         }

//         // Write DBI stream
//         dbiHeader.age = instance.Age;
//         var usedFiles = moduleInfos
//            .SelectMany( t => t.Item2 )
//            .ToArray();

//         dbiHeader.secConSize = INT_SIZE + funcSecContribs.Count * SEC_CON_MULTIPLIER;
//         dbiHeader.secMapSize = moduleInfos.Count > 0 ? SEC_MAP_FIXED_SIZE : INT_SIZE;
//         dbiHeader.fileInfoSize = INT_SIZE + INT_SIZE * moduleInfos.Count + usedFiles
//            .Aggregate( 0, ( cur, s ) => cur + INT_SIZE + NAME_ENCODING.SafeByteCount( s ) );
//         Align4( ref dbiHeader.fileInfoSize );
//         dbiHeader.debugHeaderSize = moduleInfos.Count > 0 ? DBI_DEBUG_HEADER_SIZE : 0;
//         dbiHeader.gsSymStream = 8;
//         dbiHeader.psSymStream = 9;
//         dbiHeader.symRecStream = 10;
//         array = NewArrayIfNeeded(
//            array,
//            DBI_HEADER_SIZE + dbiHeader.moduleInfoSize + dbiHeader.fileInfoSize + dbiHeader.secConSize + dbiHeader.secMapSize + dbiHeader.ecInfoSize + dbiHeader.debugHeaderSize,
//            out idx,
//            out arrayLen
//            );
//         // Write module infos
//         dbiHeader.WriteDBIHeader( array, ref idx );
//         foreach ( var mInfo in moduleInfos )
//         {
//            mInfo.Item1.WriteToArray( array, ref idx );
//         }
//         // Section contribution
//         array.WriteUInt32LEToBytes( ref idx, SEC_CON_SIG );
//         tmpUInt32 = 0u;
//         foreach ( var secCon in funcSecContribs )
//         {
//            secCon.offset = tmpUInt32;
//            secCon.WriteToArray( array, ref idx );
//            tmpUInt32 += secCon.size;
//         }
//         if ( tmpUInt32 > 0 )
//         {
//            // Section map, fixed prefix (cSeg, cSegLog, flags, ovl1, grp, frame, iSegName, iClassName, offset)
//            array
//               .BlockCopyFrom( ref idx, new Byte[] { 0x02, 0x00, 0x02, 0x00, 0x0D, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00 } )
//               .WriteUInt32LEToBytes( ref idx, tmpUInt32 ) // Section size
//               .BlockCopyFrom( ref idx, new Byte[] { 0x08, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF } ); // Section map, fixed suffix.
//         }
//         else
//         {
//            array.WriteInt32LEToBytes( ref idx, 0 );
//         }
//         // File info
//         array
//            .WriteUInt16LEToBytes( ref idx, (UInt16) moduleInfos.Count ) // Amount of modules
//            .WriteUInt16LEToBytes( ref idx, (UInt16) usedFiles.Length ); // Amount of file references in modules
//         UInt16 fiIdx = 0;
//         foreach ( var mInfo in moduleInfos )
//         {
//            array.WriteUInt16LEToBytes( ref idx, fiIdx );
//            fiIdx += (UInt16) mInfo.Item2.Length;
//         }
//         foreach ( var mInfo in moduleInfos )
//         {
//            array.WriteUInt16LEToBytes( ref idx, (UInt16) mInfo.Item2.Length );
//         }

//         var fiNameIdx = 0u;
//         foreach ( var uf in usedFiles )
//         {
//            array.WriteUInt32LEToBytes( ref idx, fiNameIdx );
//            fiNameIdx += (UInt32) NAME_ENCODING.SafeByteCount( uf );
//         }
//         foreach ( var uf in usedFiles )
//         {
//            array.WriteZeroTerminatedString( ref idx, uf );
//         }
//         Align4( ref idx );

//         // EC info
//         array.BlockCopyFrom( ref idx, new Byte[] { 0xFE, 0xEF, 0xFE, 0xEF, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } );

//         // Debug header
//         if ( tmpUInt32 > 0 )
//         {
//            new DBIDebugHeader( 11 ).WriteToArray( array, ref idx );
//         }
//#if DEBUG
//         if ( idx != arrayLen )
//         {
//            throw new PDBException( "Debyyg" );
//         }
//#endif
//         // Write DBI stream 
//         startPage = stream.GetCurrentPage( pageSize );
//         stream.stream.Write( array, arrayLen );
//         dataStreams[3] = Tuple.Create( arrayLen, stream.PagesFromContinuousStream( pageSize, startPage, zeroesPageArray ) );

//         // Write section header stream. This is needed to set global RVA to zero.
//         if ( tmpUInt32 > 0 )
//         {
//            array = NewArrayIfNeeded( array, 40, out idx, out arrayLen );
//            array
//               .Zeroes( ref idx, 8 )
//               .WriteUInt32LEToBytes( ref idx, tmpUInt32 )
//               .Zeroes( ref idx, 4 )
//               .WriteUInt32LEToBytes( ref idx, tmpUInt32 )
//               .Zeroes( ref idx, 20 );
//            startPage = stream.GetCurrentPage( pageSize );
//            stream.stream.Write( array, arrayLen );
//            dataStreams[11] = Tuple.Create( arrayLen, stream.PagesFromContinuousStream( pageSize, startPage, zeroesPageArray ) );
//         }

//         // Write name index
//         strByteCount = nameIndexAux;
//         array = NewArrayIfNeeded( array, NAME_INDEX_FIXED_SIZE + strByteCount + INT_SIZE * nameIndex.Count, out idx, out arrayLen );
//         array
//            .WriteUInt32LEToBytes( ref idx, NAME_INDEX_SIG ) // Signature
//            .WriteUInt32LEToBytes( ref idx, NAME_INDEX_VERSION ) // Version
//            .WriteInt32LEToBytes( ref idx, strByteCount ); // String byte count
//         afterStrIdx = idx + strByteCount;
//         array.WriteByteToBytes( ref idx, 0 ); // 1 zero byte to distinguish zero indices
//         array.WriteInt32LEToBytes( ref afterStrIdx, nameIndex.Count ); // Max amount of names
//         foreach ( var kvp in nameIndex )
//         {
//            // Write index information first
//            array
//               .WriteInt32LEToBytes( ref afterStrIdx, kvp.Value );
//            // Write string
//            array.WriteZeroTerminatedString( ref idx, kvp.Key );
//         }
//         array.WriteInt32LEToBytes( ref afterStrIdx, nameIndex.Count ); // Amount of names
//#if DEBUG
//         if ( afterStrIdx != arrayLen )
//         {
//            throw new PDBException( "Debyyg" );
//         }
//#endif
//         // Write name index stream
//         startPage = stream.GetCurrentPage( pageSize );
//         stream.stream.Write( array, arrayLen );
//         dataStreams[6] = Tuple.Create( arrayLen, stream.PagesFromContinuousStream( pageSize, startPage, zeroesPageArray ) );

//         // Write directory
//         array = NewArrayIfNeeded(
//            array,
//            INT_SIZE + INT_SIZE * dataStreams.Count + dataStreams.Aggregate( 0, ( cur, tuple ) => cur + ( tuple == null ? 0 : tuple.Item2.Length * INT_SIZE ) ),
//            out idx,
//            out arrayLen );
//         array.WriteInt32LEToBytes( ref idx, dataStreams.Count );
//         foreach ( var tuple in dataStreams )
//         {
//            array.WriteInt32LEToBytes( ref idx, tuple == null ? 0 : tuple.Item1 );
//         }
//         foreach ( var tuple in dataStreams )
//         {
//            if ( tuple != null && tuple.Item2 != null )
//            {
//               foreach ( var page in tuple.Item2 )
//               {
//                  array.WriteInt32LEToBytes( ref idx, page );
//               }
//            }
//         }
//#if DEBUG
//         if ( idx != arrayLen )
//         {
//            throw new PDBException( "Debyyg" );
//         }
//#endif
//         startPage = stream.GetCurrentPage( pageSize );
//         stream.stream.Write( array, arrayLen );
//         var directoryByteCount = arrayLen;
//         var directoryPages = stream.PagesFromContinuousStream( pageSize, startPage, zeroesPageArray );

//         // Write directory page information
//         array = NewArrayIfNeeded( array, directoryPages.Length * INT_SIZE, out idx, out arrayLen );
//         foreach ( var page in directoryPages )
//         {
//            array.WriteInt32LEToBytes( ref idx, page );
//         }
//         startPage = stream.GetCurrentPage( pageSize );
//         stream.stream.Write( array, arrayLen );
//         directoryPages = stream.PagesFromContinuousStream( pageSize, startPage, zeroesPageArray );

//         // Write PDBHeader
//         stream.SeekToPage( pageSize, 0, 0 );
//         array = NewArrayIfNeeded( array, HEADER_FIXED_SIZE + directoryPages.Length * INT_SIZE, out idx, out arrayLen );
//         if ( arrayLen > pageSize )
//         {
//            throw new PDBException( "Header is larger than pagesize." );
//         }
//         var pagesUsed = directoryPages[directoryPages.Length - 1] + 1;
//         array
//            .BlockCopyFrom( ref idx, new Byte[]
//            {
//              0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0x20, 0x43, 0x2F, 0x43, 0x2B, 0x2B, 0x20, // Microsoft C/C++ 
//              0x4D, 0x53, 0x46, 0x20, 0x37, 0x2E, 0x30, 0x30, 0x0D, 0x0A, 0x1A, 0x44, 0x53, 0x00, 0x00, 0x00  // MSF 7.00...DS...
//            }, 0, 32 )
//            .WriteInt32LEToBytes( ref idx, pageSize ) // Page size
//            .WriteInt32LEToBytes( ref idx, START_PAGE - 1 ) // Free page map
//            .WriteInt32LEToBytes( ref idx, pagesUsed ) // Pages used
//            .WriteInt32LEToBytes( ref idx, directoryByteCount ) // Directory byte count
//            .WriteInt32LEToBytes( ref idx, 0 ); // Magic zero
//         // Write directory page information pages
//         foreach ( var page in directoryPages )
//         {
//            array.WriteInt32LEToBytes( ref idx, page );
//         }
//#if DEBUG
//         if ( idx != arrayLen )
//         {
//            throw new PDBException( "Debyyg" );
//         }
//#endif
//         // Zero out the rest
//         stream.stream.Write( array, arrayLen );
//         stream.PagesFromContinuousStream( pageSize, 0, zeroesPageArray );

//         // DBI refuses to load file if free page map size is zero.
//         // Therefore, write free page map.
//         array = NewArrayIfNeeded( array, pageSize * ( START_PAGE - 1 ), out idx, out arrayLen );
//         var reserved = ( pagesUsed + 1 ) / 2;
//         for ( var i = 0; i < pageSize / INT_SIZE; ++i )
//         {
//            var ui = UInt32.MaxValue;
//            if ( i * 32 < reserved )
//            {
//               ui <<= reserved;
//               reserved -= 32;
//            }
//            array.WriteUInt32LEToBytes( ref idx, ui );
//         }
//         reserved = ( pagesUsed + 1 ) - ( ( pagesUsed + 1 ) / 2 );
//         for ( var i = 0; i < pageSize / INT_SIZE; ++i )
//         {
//            var ui = UInt32.MaxValue;
//            if ( i * 32 < reserved )
//            {
//               ui <<= reserved;
//               reserved -= 32;
//            }
//            array.WriteUInt32LEToBytes( ref idx, ui );
//         }
//         stream.stream.Write( array, arrayLen );
//      }

//      private static void SkipToNextPage( this StreamInfo stream, Int32 pageSize, Byte[] zeroesPageArray )
//      {
//         var remainder = (Int32) ( stream.stream.Position % pageSize );
//         if ( remainder != 0 )
//         {
//            stream.stream.Write( zeroesPageArray, pageSize - remainder );
//         }
//      }

//      private static void FlushPageIfNeeded( Byte[] array, ref Int32 idx, StreamInfo stream, Int32 pageSize )
//      {
//         if ( idx >= pageSize ) // 'idx' points to next free byte
//         {
//            // Have to flush to stream
//            stream.stream.Write( array, 0, idx );
//            // Assumes that delta never gets too big
//            while ( idx > pageSize )
//            {
//               array[idx - pageSize] = array[idx];
//               --idx;
//            }
//         }
//      }

//      private static Int32 GetCurrentPage( this StreamInfo stream, Int32 pageSize )
//      {
//         return ( (Int32) stream.stream.Position ) / pageSize;
//      }

//      private static Int32[] PagesFromContinuousStream( this StreamInfo stream, Int32 pageSize, Int32 startPage, Byte[] zeroesPageArray )
//      {
//         stream.SkipToNextPage( pageSize, zeroesPageArray );
//         return Enumerable.Range( startPage, stream.GetCurrentPage( pageSize ) - startPage ).ToArray();
//      }

//      private static Int32 GetNameIndex( IDictionary<String, Int32> nameIndex, ref Int32 nameIndexAux, String name )
//      {
//         name = name ?? String.Empty;
//         Int32 result;
//         if ( !nameIndex.TryGetValue( name, out result ) )
//         {
//            result = nameIndexAux;
//            nameIndex.Add( name, result );
//            nameIndexAux += NAME_ENCODING.SafeByteCount( name );
//         }
//         return result;
//      }

//      private static void WriteScopeOrFunctionBlocks( PDBScopeOrFunction scope, Byte[] array, ref Int32 idx, Int32 totalIndexAtStartOfFunction, UInt32 functionAddress, Int32 parentPointer )
//      {
//         // Slots
//         foreach ( var slot in scope.Slots )
//         {
//            var lenIdx = idx;
//            array
//               .Skip( ref idx, 2 ) // Block size
//               .WriteUInt16LEToBytes( ref idx, SYM_MANAGED_SLOT ) // MANSLOT
//               .WriteInt32LEToBytes( ref idx, slot.SlotIndex ) // slot index
//               .WriteUInt32LEToBytes( ref idx, slot.TypeToken ) // type token
//               .WriteInt32LEToBytes( ref idx, 0 ) // address
//               .WriteUInt16LEToBytes( ref idx, 0 ) // segment
//               .WriteUInt16LEToBytes( ref idx, (UInt16) slot.Flags ) // flags
//               .WriteZeroTerminatedString( ref idx, slot.Name ) // name
//               .Align4( ref idx ); // 4-byte border
//            // Revisit size
//            array.WriteBlockLength( lenIdx, idx );
//         }

//         // Used namespaces
//         foreach ( var un in scope.UsedNamespaces )
//         {
//            var lenIdx = idx;
//            array
//               .Skip( ref idx, 2 ) // Block size
//               .WriteUInt16LEToBytes( ref idx, SYM_USED_NS ) // UNAMESPACE
//               .WriteZeroTerminatedString( ref idx, un ) // namespace
//               .Align4( ref idx ); // 4-byte border
//            // Revisit size
//            array.WriteBlockLength( lenIdx, idx );
//         }

//         // Scopes
//         foreach ( var innerScope in scope.Scopes )
//         {
//            var lenIdx = idx;
//            array
//               .Skip( ref idx, 2 ) // block size
//               .WriteUInt16LEToBytes( ref idx, SYM_SCOPE ) // BLOCK_32
//               .WriteInt32LEToBytes( ref idx, parentPointer ); // parent
//            var endIdx = idx;
//            array
//               .Skip( ref idx, 4 ) // end pointer
//               .WriteUInt32LEToBytes( ref idx, innerScope.Length ) // length
//               .WriteUInt32LEToBytes( ref idx, functionAddress + innerScope.Offset ) // address
//               .WriteUInt16LEToBytes( ref idx, 1 ) // segment
//               .WriteZeroTerminatedString( ref idx, innerScope.Name ) // name
//               .Align4( ref idx ); // 4-byte border
//            // Revisit size
//            array.WriteBlockLength( lenIdx, idx );
//            // Revisit end pointer
//            array.WriteInt32LEToBytes( ref endIdx, totalIndexAtStartOfFunction + idx + CalculateByteCountFromLists( innerScope ) );
//            // Write inner scope's lists
//            WriteScopeOrFunctionBlocks( innerScope, array, ref idx, totalIndexAtStartOfFunction, functionAddress, parentPointer );
//            // Write END sym
//            WriteENDSym( array, ref idx );
//         }
//      }

//      private static void WriteOEM( PDBFunction func, Byte[] array, ref Int32 idx, ref Int32 totalIndex, StreamInfo stream )
//      {
//         var am = func.AsyncMethodInfo;
//         if ( am != null )
//         {
//            var lenIdx = idx;
//            WriteOEMHeader( array, ref idx, ASYNC_METHOD_OEM_NAME );
//            array
//               .WriteUInt32LEToBytes( ref idx, am.KickoffMethodToken )
//               .WriteInt32LEToBytes( ref idx, am.CatchHandlerOffset )
//               .WriteInt32LEToBytes( ref idx, am.SynchronizationPoints.Count );
//            foreach ( var sp in am.SynchronizationPoints )
//            {
//               array
//                  .WriteInt32LEToBytes( ref idx, sp.SyncOffset )
//                  .WriteUInt32LEToBytes( ref idx, sp.ContinuationMethodToken )
//                  .WriteInt32LEToBytes( ref idx, sp.ContinuationOffset );
//            }
//            array.Align4( ref idx );
//            // Revisit length
//            array.WriteBlockLength( lenIdx, idx );
//         }

//         Byte count = 0;
//         if ( func.UsingCounts.Count > 0 )
//         {
//            ++count;
//         }
//         if ( func.ForwardingMethodToken != 0u )
//         {
//            ++count;
//         }
//         if ( func.ModuleForwardingMethodToken != 0u )
//         {
//            ++count;
//         }
//         if ( func.LocalScopes.Count > 0 )
//         {
//            ++count;
//         }
//         if ( !String.IsNullOrEmpty( func.IteratorClass ) )
//         {
//            ++count;
//         }
//         if ( count > 0 )
//         {
//            var lenIdx = idx;
//            WriteOEMHeader( array, ref idx, MD_OEM_NAME );
//            array
//               .WriteByteToBytes( ref idx, 4 ) // version
//               .WriteByteToBytes( ref idx, count ) // MD item count
//               .Align4( ref idx ); // 4-byte boundary
//            if ( func.UsingCounts.Count > 0 )
//            {
//               // Iterator info
//               var startIdx = idx;
//               Int32 mdLenIdx;
//               array
//                  .WriteOEMItemKind( ref idx, MD2_USED_NAMESPACES, out mdLenIdx )
//                  .WriteUInt16LEToBytes( ref idx, (UInt16) func.UsingCounts.Count );
//               foreach ( var un in func.UsingCounts )
//               {
//                  array.WriteUInt16LEToBytes( ref idx, un );
//               }
//               // Revisit length
//               array
//                  .Align4( ref idx )
//                  .WriteInt32LEToBytes( ref mdLenIdx, idx - startIdx );
//            }
//            if ( func.ForwardingMethodToken != 0u )
//            {
//               // Forwarding method
//               var startIdx = idx;
//               Int32 mdLenIdx;
//               array
//                  .WriteOEMItemKind( ref idx, MD2_FORWARDING_METHOD_TOKEN, out mdLenIdx )
//                  .WriteUInt32LEToBytes( ref idx, func.ForwardingMethodToken )
//                  .Align4( ref idx )
//                  .WriteInt32LEToBytes( ref mdLenIdx, idx - startIdx );// Revisit length
//            }
//            if ( func.ModuleForwardingMethodToken != 0u )
//            {
//               // Forwarding module method
//               var startIdx = idx;
//               Int32 mdLenIdx;
//               array
//                  .WriteOEMItemKind( ref idx, MD2_FORWARDING_MODULE_METHOD_TOKEN, out mdLenIdx )
//                  .WriteUInt32LEToBytes( ref idx, func.ModuleForwardingMethodToken )
//                  .Align4( ref idx )
//                  .WriteInt32LEToBytes( ref mdLenIdx, idx - startIdx ); // Revisit length
//            }
//            if ( func.LocalScopes.Count > 0 )
//            {
//               var startIdx = idx;
//               Int32 mdLenIdx;
//               array
//                  .WriteOEMItemKind( ref idx, MD2_LOCAL_SCOPES, out mdLenIdx )
//                  .WriteInt32LEToBytes( ref idx, func.LocalScopes.Count );
//               foreach ( var ls in func.LocalScopes )
//               {
//                  array
//                     .WriteInt32LEToBytes( ref idx, ls.Offset ) // IL start offset
//                     .WriteInt32LEToBytes( ref idx, ls.Offset + ls.Length ); // IL end offset
//               }
//               // Revisit length
//               array
//                  .Align4( ref idx )
//                  .WriteInt32LEToBytes( ref mdLenIdx, idx - startIdx );
//            }
//            if ( !String.IsNullOrEmpty( func.IteratorClass ) )
//            {
//               var startIdx = idx;
//               Int32 mdLenIdx;
//               array
//                  .WriteOEMItemKind( ref idx, MD2_ITERATOR_CLASS, out mdLenIdx )
//                  .WriteZeroTerminatedString( ref idx, func.IteratorClass )
//                  .Align4( ref idx )
//                  .WriteInt32LEToBytes( ref mdLenIdx, idx - startIdx );
//            }

//            // The size of whole OEM block
//            array.WriteBlockLength( lenIdx, idx );
//         }
//      }

//      private static Byte[] WriteOEMItemKind( this Byte[] array, ref Int32 idx, Byte mdKind, out Int32 lenIdx )
//      {
//         array
//            .WriteByteToBytes( ref idx, 4 ) // version
//            .WriteByteToBytes( ref idx, mdKind ) // md item kind
//            .Align4( ref idx ); // 4-byte boundary
//         lenIdx = idx; // Save length index
//         idx += 4; // Skip the length
//         return array;
//      }

//      private static void WriteOEMHeader( Byte[] array, ref Int32 idx, String oemName )
//      {
//         array
//            .Skip( ref idx, 2 ) // Block byte count
//            .WriteUInt16LEToBytes( ref idx, SYM_OEM ) // OEM sym
//            .WriteGUIDToBytes( ref idx, GUIDs.MSIL_METADATA_GUID ) // MD2 GUID
//            .WriteInt32LEToBytes( ref idx, 0 ) // Type index
//            .WriteZeroTerminatedString( ref idx, oemName, false ); // OEM name
//      }

//      private static void WriteENDSym( Byte[] array, ref Int32 idx )
//      {
//         var lenIdx = idx;
//         array
//            .Skip( ref idx, 2 ) // Length
//            .WriteUInt16LEToBytes( ref idx, SYM_END ) // END
//            .Align4( ref idx );
//         // Revisit length
//         array.WriteUInt16LEToBytes( ref lenIdx, (UInt16) ( idx - lenIdx - 2 ) );
//      }

//      public static Int32 CalculateSymbolByteCount( this PDBFunction func )
//      {
//         // Function byte length begins after the name
//         var result = func.CalculateByteCountFromLists()
//            + CalculateByteCountAsyncMethodInfo( func.AsyncMethodInfo ) // Async OEM
//            + CalculateByteCountMD2Info( func ); // MD2 OEM
//         Align4( ref result ); // 4-byte boundary
//         return result; // No END SYM
//      }

//      private static Int32 CalculateByteCountAsyncMethodInfo( PDBAsyncMethodInfo asyncInfo )
//      {
//         // Async info OEM is needed
//         var result = asyncInfo == null ? 0 : ( FIXED_OEM_SIZE // Required OEM info
//            + UTF16.GetByteCount( ASYNC_METHOD_OEM_NAME ) + 2 // OEM name
//            + INT_SIZE * 3 // Kickoff method token, catch handler offset, sync point count
//            + asyncInfo.SynchronizationPoints.Count * INT_SIZE * 3 ); // For each sync point, sync offset, continuation method token, continuation offset
//         Align4( ref result );
//         return result;
//      }

//      private static Int32 CalculateByteCountMD2Info( PDBFunction func )
//      {
//         var size = 0;
//         if ( !String.IsNullOrEmpty( func.IteratorClass )
//              || func.ForwardingMethodToken != 0u
//              || func.ModuleForwardingMethodToken != 0u
//              || func.LocalScopes.Count > 0
//              || func.UsingCounts.Count > 0
//            )
//         {
//            // MD2 OEM is needed
//            size += FIXED_OEM_SIZE // Required OEM info
//               + UTF16.GetByteCount( MD_OEM_NAME ) + 2 // OEM name
//               + 2; // Version byte, OEM MD2 infos count
//            Align4( ref size );
//            if ( func.UsingCounts.Count > 0 )
//            {
//               size += 2; // version + MD2 kind
//               Align4( ref size ); // 4-byte boundary
//               size += INT_SIZE // OEM entry byte size
//                  + SHORT_SIZE // Used namespace count
//                  + SHORT_SIZE * func.UsingCounts.Count; // Used namespace idx
//               Align4( ref size );
//            }
//            if ( func.ForwardingMethodToken != 0u )
//            {
//               size += 2; // version + MD2 kind
//               Align4( ref size ); // 4-byte boundary
//               size += INT_SIZE // OEM entry byte size
//                  + INT_SIZE; // Token
//            }
//            if ( func.ModuleForwardingMethodToken != 0u )
//            {
//               size += 2; // version + MD2 kind
//               Align4( ref size ); // 4-byte boundary
//               size += INT_SIZE // OEM entry byte size
//                  + INT_SIZE; // Token
//            }
//            if ( func.LocalScopes.Count > 0 )
//            {
//               size += 2; // version + MD2 kind
//               Align4( ref size ); // 4-byte boundary
//               size += INT_SIZE // OEM entry byte size
//                  + INT_SIZE // Local scope count
//                  + INT_SIZE * 2 * func.LocalScopes.Count; // Local scope offset and length
//               Align4( ref size );
//            }
//            if ( !String.IsNullOrEmpty( func.IteratorClass ) )
//            {
//               size += 2; // version + MD2 kind
//               Align4( ref size ); // 4-byte boundary
//               size += INT_SIZE // OEM entry byte size
//                  + NAME_ENCODING.GetByteCount( func.IteratorClass ); // Iterator class name
//               Align4( ref size );
//            }

//         }
//         return size;
//      }

//      public static Int32 CalculateByteCount( this PDBScope scope )
//      {
//         var result = SCOPE_FIXED_SIZE + 4; // BlockLen + SYM
//         result += NAME_ENCODING.SafeByteCount( scope.Name );
//         Align4( ref result );
//         result += scope.CalculateByteCountFromLists();
//         Align4( ref result );
//         return result + 4; // END SYM
//      }

//      private static Int32 CalculateByteCountFromLists( this PDBScopeOrFunction scope )
//      {
//         return scope.Slots.Aggregate( 0, ( cur, slot ) =>
//            {
//               var result = cur + 4 + SLOT_FIXED_SIZE + NAME_ENCODING.SafeByteCount( slot.Name ); // BlockLen + SYM
//               Align4( ref result );
//               return result;
//            } )
//            + scope.UsedNamespaces.Aggregate( 0, ( cur, un ) =>
//            {
//               var result = cur + 4 + NAME_ENCODING.SafeByteCount( un ); // BlockLen + SYM
//               Align4( ref result );
//               return result;
//            } )
//            + scope.Scopes.Aggregate( 0, ( cur, scp ) => cur + scp.CalculateByteCount() );
//      }

//      private static Int32 SafeLength<T>( this T[] array )
//      {
//         return array == null ? 0 : array.Length;
//      }

//      private static Int32 SafeByteCount( this Encoding encoding, String str, Boolean zeroTerminated = true )
//      {
//         var result = zeroTerminated ? 1 : INT_SIZE;
//         if ( str != null )
//         {
//            result += encoding.GetByteCount( str );
//         }
//         return result;
//      }

//      private static void WriteBlockLength( this Byte[] array, Int32 lenIdx, Int32 curIdx )
//      {
//         array.WriteUInt16LEToBytes( ref lenIdx, (UInt16) ( curIdx - lenIdx - 2 ) );
//      }

//      private static Byte[] NewArrayIfNeeded( Byte[] array, Int32 length, out Int32 idx, out Int32 arrayLen )
//      {
//         idx = 0;
//         arrayLen = length;
//         return array == null || array.Length < length ?
//            new Byte[length] :
//            array;
//      }

//      private static Byte[] Zeroes( this Byte[] array, ref Int32 idx, Int32 amount )
//      {
//         while ( amount-- > 0 )
//         {
//            array[idx++] = 0;
//         }
//         return array;
//      }

//      private static Byte[] ZeroesInt32( this Byte[] array, ref Int32 idx, Int32 amount )
//      {
//         while ( amount-- > 0 )
//         {
//            array.WriteInt32LEToBytes( ref idx, 0 );
//         }
//         return array;
//      }

//      // This is algorithm used to compute hashes of names in gsSymStream.
//      // Ported from C to C# and modified a bit, original is available at http://code.google.com/p/pdbparser/source/browse/trunk/symeng/misc.cpp , HashPbCb function.
//      private static UInt32 ComputeHash( Byte[] pb, Int32 startIdx, Int32 count, UInt32 ulMod )
//      {
//         var ulHash = 0u;

//         var cl = count >> 2;
//         // Create new UInt32 array and fill it, play with unsafe blocks only if this becomes a performance problem.
//         var pul = new UInt32[cl];
//         var tmp = 0;
//         while ( tmp < pul.Length )
//         {
//            pul[tmp] = pb.ReadUInt32LEFromBytes( ref startIdx );
//            ++tmp;
//         }

//         // Have to unwind switch- and do-while-stataments that were compact in C.
//         var dcul = cl & 7;
//         switch ( dcul )
//         {
//            case 7: ulHash ^= pul[6]; ulHash ^= pul[5]; ulHash ^= pul[4]; ulHash ^= pul[3]; ulHash ^= pul[2]; ulHash ^= pul[1]; ulHash ^= pul[0]; break;
//            case 6: ulHash ^= pul[5]; ulHash ^= pul[4]; ulHash ^= pul[3]; ulHash ^= pul[2]; ulHash ^= pul[1]; ulHash ^= pul[0]; break;
//            case 5: ulHash ^= pul[4]; ulHash ^= pul[3]; ulHash ^= pul[2]; ulHash ^= pul[1]; ulHash ^= pul[0]; break;
//            case 4: ulHash ^= pul[3]; ulHash ^= pul[2]; ulHash ^= pul[1]; ulHash ^= pul[0]; break;
//            case 3: ulHash ^= pul[2]; ulHash ^= pul[1]; ulHash ^= pul[0]; break;
//            case 2: ulHash ^= pul[1]; ulHash ^= pul[0]; break;
//            case 1: ulHash ^= pul[0]; break;
//         }
//         tmp = dcul;
//         while ( tmp < pul.Length )
//         {
//            ulHash ^= pul[7];
//            ulHash ^= pul[6];
//            ulHash ^= pul[5];
//            ulHash ^= pul[4];
//            ulHash ^= pul[3];
//            ulHash ^= pul[2];
//            ulHash ^= pul[1];
//            ulHash ^= pul[0];
//            tmp += 8;
//         }

//         if ( ( count & 2 ) != 0 )
//         {
//            ulHash ^= pb.ReadUInt16LEFromBytes( ref startIdx );
//         }
//         if ( ( count & 1 ) != 0 )
//         {
//            ulHash ^= pb[startIdx];
//         }

//         ulHash |= TO_LOWER_MASK;
//         ulHash ^= ( ulHash >> 11 );

//         return ( ulHash ^ ( ulHash >> 16 ) ) % ulMod;
//      }

//      private static void AddToGSSym( IDictionary<UInt32, Object> gsSymAux, Byte[] symRecStream, Int32 symRecStart, Int32 symRecEnd, Int32 symRecRef )
//      {
//         ++symRecRef; // Indexing is 1-based
//         var hash = ComputeHash( symRecStream, symRecStart, symRecEnd - symRecStart - 1, GS_SYM_BUCKETS ); // Remember terminating zero
//         Object obj;
//         if ( gsSymAux.TryGetValue( hash, out obj ) )
//         {
//            if ( obj is Int32 )
//            {
//               var tmp = new List<Int32>();
//               tmp.Add( (Int32) obj );
//               tmp.Add( symRecRef );
//               gsSymAux[hash] = tmp;
//            }
//            else
//            {
//               ( (IList<Int32>) obj ).Add( symRecRef );
//            }
//         }
//         else
//         {
//            gsSymAux.Add( hash, symRecRef );
//         }
//      }
//   }
//}
