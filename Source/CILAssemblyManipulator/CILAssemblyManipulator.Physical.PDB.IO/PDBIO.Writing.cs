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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommonUtils;
using CILAssemblyManipulator.Physical.PDB;
using TStreamInfo = System.Tuple<System.Int32, System.Int32[]>;
using TSourceHeaderInfo = System.Tuple<System.Int32, System.Int32, System.Int32, CILAssemblyManipulator.Physical.PDB.PDBSource>;
using TWriteAction = System.Tuple<System.Int32, System.Action<System.Byte[], System.Int32>>;

#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{
   private const UInt16 TOKENREF_FIXED_SIZE = 24;
   private const Int32 GS_SYM_BUCKETS = 4096;

   private const Int32 START_PAGE = 3; // Next two pages after the first page are for page allocation bits

   private sealed class PDBNameIndex
   {
      private Int32 _index;

      public PDBNameIndex( Int32 startingIndex )
      {
         this._index = startingIndex;
         this.NameIndex = new Dictionary<String, Int32>();
      }

      public Dictionary<String, Int32> NameIndex { get; }

      public Int32 CurrentIndexValue
      {
         get
         {
            return this._index;
         }
      }

      public Int32 GetNameIndex( String name )
      {
         return this.NameIndex.GetOrAdd_NotThreadSafe( name ?? String.Empty, str =>
         {
            var result = this._index;

            this._index += PDBIO.NameEncoding.SafeByteCount( name, true );
            return result;
         } );
      }
   }

   private sealed class SymbolRecStreamRefInfo
   {
      private readonly PDBWritingState _state;

      private Int32 _refCount;

      public SymbolRecStreamRefInfo( PDBWritingState state )
      {
         ArgumentValidator.ValidateNotNull( "State", state );

         this._state = state;
         this.NamedReferences = new Dictionary<UInt32, List<Int32>>();
      }

      public Dictionary<UInt32, List<Int32>> NamedReferences { get; }

      public Int32 ReferenceCount
      {
         get
         {
            return this._refCount;
         }
      }

      //public void RecordSymRecReference( Int32 referenceOffset )
      //{
      //   this.DoRecordSymRecReference( referenceOffset );
      //}

      public void RecordNamedSymcRecReference( Int32 nameStart, Int32 referenceOffset )
      {
         var hash = ComputeHash( this._state.SymRecStream, nameStart, this._state.SymRecIndex - nameStart - 1, GS_SYM_BUCKETS ); // Remember terminating zero
         ++referenceOffset; // Indexing is 1-based
         this.NamedReferences
            .GetOrAdd_NotThreadSafe( hash, h => new List<Int32>() )
            .Add( referenceOffset );
         ++this._refCount;
      }

      //private Int32 DoRecordSymRecReference( Int32 referenceOffset )
      //{
      //   ++referenceOffset; // Indexing is 1-based
      //   var list = this.References;
      //   list.Add( referenceOffset );
      //   return list.Count;
      //}

      // This is algorithm used to compute hashes of names in gsSymStream.
      // Ported from C to C# and modified a bit, original is available at http://code.google.com/p/pdbparser/source/browse/trunk/symeng/misc.cpp , HashPbCb function.
      private static UInt32 ComputeHash( Byte[] pb, Int32 startIdx, Int32 count, UInt32 ulMod )
      {
         var ulHash = 0u;

         var cl = count >> 2;
         // Create new UInt32 array and fill it, play with unsafe blocks only if this becomes a performance problem.
         var pul = new UInt32[cl];
         var tmp = 0;
         while ( tmp < pul.Length )
         {
            pul[tmp] = pb.ReadUInt32LEFromBytes( ref startIdx );
            ++tmp;
         }

         // Have to unwind switch- and do-while-stataments that were compact in C.
         var dcul = cl & 7;
         switch ( dcul )
         {
            case 7: ulHash ^= pul[6]; ulHash ^= pul[5]; ulHash ^= pul[4]; ulHash ^= pul[3]; ulHash ^= pul[2]; ulHash ^= pul[1]; ulHash ^= pul[0]; break;
            case 6: ulHash ^= pul[5]; ulHash ^= pul[4]; ulHash ^= pul[3]; ulHash ^= pul[2]; ulHash ^= pul[1]; ulHash ^= pul[0]; break;
            case 5: ulHash ^= pul[4]; ulHash ^= pul[3]; ulHash ^= pul[2]; ulHash ^= pul[1]; ulHash ^= pul[0]; break;
            case 4: ulHash ^= pul[3]; ulHash ^= pul[2]; ulHash ^= pul[1]; ulHash ^= pul[0]; break;
            case 3: ulHash ^= pul[2]; ulHash ^= pul[1]; ulHash ^= pul[0]; break;
            case 2: ulHash ^= pul[1]; ulHash ^= pul[0]; break;
            case 1: ulHash ^= pul[0]; break;
         }
         tmp = dcul;
         while ( tmp < pul.Length )
         {
            ulHash ^= pul[7];
            ulHash ^= pul[6];
            ulHash ^= pul[5];
            ulHash ^= pul[4];
            ulHash ^= pul[3];
            ulHash ^= pul[2];
            ulHash ^= pul[1];
            ulHash ^= pul[0];
            tmp += 8;
         }

         if ( ( count & 2 ) != 0 )
         {
            ulHash ^= pb.ReadUInt16LEFromBytes( ref startIdx );
         }
         if ( ( count & 1 ) != 0 )
         {
            ulHash ^= pb[startIdx];
         }

         const UInt32 TO_LOWER_MASK = 0x20202020u;
         ulHash |= TO_LOWER_MASK;
         ulHash ^= ( ulHash >> 11 );

         return ( ulHash ^ ( ulHash >> 16 ) ) % ulMod;
      }
   }

   private sealed class PDBWritingState
   {

      public PDBWritingState( PDBInstance pdb, StreamHelper stream, Int32 pageSize, Int32? epToken )
      {
         this.PDB = pdb;
         this.Stream = stream;
         this.StreamStart = stream.Stream.Position;
         this.PageSize = pageSize;

         this.NameIndex = new PDBNameIndex( 1 );
         this.DataStreams = new List<TStreamInfo>();
         this.DataStreamNames = new Dictionary<String, Int32>();
         this.ZeroesArray = new Byte[this.PageSize];

         // Create array for the symbol record stream
         const Int32 PUBLIC_SIZE = 0x20;
         this.SymRecStream = new Byte[
            ( epToken.HasValue ? PUBLIC_SIZE : 0 )
            + pdb.Modules
            .SelectMany( m => m.Functions )
            .Aggregate( 0, ( cur, f ) =>
            {
               cur += 14 + PDBIO.NameEncoding.SafeByteCount( f.Name, true );
               Align4( ref cur );
               return cur + TOKENREF_FIXED_SIZE;
            } )];
         this.SymRecIndex = 0;
         this.Globals = new SymbolRecStreamRefInfo( this );
         this.Publics = new SymbolRecStreamRefInfo( this );

         if ( epToken.HasValue )
         {
            this.SymRecStream
               .WriteUInt16LEToBytes( ref this.SymRecIndex, PUBLIC_SIZE - 0x02 ) // Always same size since string is always same
               .WriteUInt16LEToBytes( ref this.SymRecIndex, PDBIO.SYM_PUBLIC ) // S_PUB32 struct
               .WriteInt32LEToBytes( ref this.SymRecIndex, 0 ) // Flags - 0
               .WriteInt32LEToBytes( ref this.SymRecIndex, epToken.Value ) // Token
               .WriteUInt16LEToBytes( ref this.SymRecIndex, 0 ) // Segment - 1
               .WriteZeroTerminatedString( ref this.SymRecIndex, "COM+_Entry_Point" );
            Align4( ref this.SymRecIndex );
            this.Publics.RecordNamedSymcRecReference( 0x0E, 0 );
         }
      }

      public PDBInstance PDB { get; }

      public StreamHelper Stream { get; }

      public Int64 StreamStart { get; }

      public Int32 PageSize { get; }

      public PDBNameIndex NameIndex { get; }

      public List<TStreamInfo> DataStreams { get; }

      public Dictionary<String, Int32> DataStreamNames { get; }

      public Byte[] ZeroesArray { get; }

      public Byte[] SymRecStream { get; }

      public Int32 SymRecIndex;

      public SymbolRecStreamRefInfo Globals { get; }

      public SymbolRecStreamRefInfo Publics { get; }

      public Int32 GetNameIndex( String name )
      {
         return this.NameIndex.GetNameIndex( name );
      }
   }

   /// <summary>
   /// Serializes this <see cref="PDBInstance"/> to a stream in PDB format.
   /// </summary>
   /// <param name="instance"></param>
   /// <param name="stream">The stream to write <see cref="PDBInstance"/> to.</param>
   /// <param name="entrypointToken">Optional one-based token of entrypoint method.</param>
   /// <exception cref="ArgumentNullException">If <paramref name="stream"/> is <c>null</c>.</exception>
   /// <exception cref="NullReferenceException">If this <see cref="PDBInstance"/> is <c>null</c>.</exception>
   public static void WriteToStream( this PDBInstance instance, Stream stream, Int32? entrypointToken )
   {
      // https://github.com/Microsoft/microsoft-pdb is good place to start
      var state = new PDBWritingState( ArgumentValidator.ValidateNotNullReference( instance ), new StreamHelper( stream ), 0x200, entrypointToken );

      // Remember to start writing from start page (3 first pages are for bookkeeping)
      state.SeekToPage( START_PAGE );

      // Write IPI and TPI streams first, as they always have fixed content, since managed stuff does not use it.
      state.WriteIPIStream();
      state.WriteTPIStream();

      // Then, write all source streams
      var sourceInfo = state.WritePDBSources();

      // Then, write /src/headerblock used to search for file names
      state.WriteSourceHeaderBlock( sourceInfo );

      // TODO /LinkInfo ? 
      // TODO Symbol server info!

      // DBI stream next.
      // This will also write all module streams, global record stream, public record stream, and symbol record stream.
      state.WriteDBIStream();

      // Name index next
      state.WriteNameIndex();

      // Then, write root stream - we won't have any more named streams
      state.WriteRootStream();

      // Now finalize the writing (directory, PDB header, free page map)
      state.FinalizePDBWriting();

      // We're done
   }

   private const Int32 GUID_SIZE = 16;

   private static IDictionary<PDBSource, Int32> WritePDBSources( this PDBWritingState state, Int32 startingIndex = 8 )
   {
      var streams = state.DataStreams;
      var pad = startingIndex - streams.Count;
      if ( pad > 0 )
      {
         streams.AddRange( Enumerable.Repeat<TStreamInfo>( null, pad ) );
      }
      Int32 hashLen;
      return state.PDB.Modules
         .SelectMany( m => m.Functions )
         .SelectMany( f => f.Lines )
         .Select( l => l.Source )
         .ToDictionary_Preserve(
            src => src,
            src => state.AddNewNamedStream(
            PDBIO.SOURCE_FILE_PREFIX + src.Name.ToLowerInvariant(), // Use to-lower conversion so that .language etc elements in IL would appear correctly.
            GUID_SIZE * 4 + ( hashLen = src.Hash?.Length ?? 0 ) + ( hashLen > 0 ? 8 : 4 ),
            ( array, idx ) =>
            {
               array
                  .WriteGUIDToBytes( ref idx, src.Language )
                  .WriteGUIDToBytes( ref idx, src.Vendor )
                  .WriteGUIDToBytes( ref idx, src.DocumentType )
                  .WriteGUIDToBytes( ref idx, src.HashAlgorithm )
                  .WriteInt32LEToBytes( ref idx, hashLen );
               if ( hashLen > 0 )
               {
                  array
                     .WriteInt32LEToBytes( ref idx, 0 ) // flags? Embedded source length?
                     .BlockCopyFrom( ref idx, src.Hash );

               }
            } ),
            equalityComparer: ReferenceEqualityComparer<PDBSource>.ReferenceBasedComparer
         );
   }

   private static void WriteSourceHeaderBlock( this PDBWritingState state, IDictionary<PDBSource, Int32> sources, Int32 streamIndex = 7 )
   {
      // Compute the hashes
      var hashKeyExtractor = new Func<TSourceHeaderInfo, Int32>( info => info.Item1 );
      var hashFunction = new Func<TSourceHeaderInfo[], TSourceHeaderInfo, Int32?>( ( sourceInfos, curSource ) =>
      {
         var len = sourceInfos.Length;
         var hIdx = hashKeyExtractor( curSource ) % len;
         while ( hIdx < len && sourceInfos[hIdx] != null )
         {
            ++hIdx;
         }
         return hIdx < len ? hIdx : (Int32?) null;
      } );

      var srcHash = new TSourceHeaderInfo[sources.Count * 2];

      foreach ( var src in sources.Keys )
      {
         // The order we get these names from name index is important!!!
         // We have to get normal index *first*, and then object file name index, and then virtual file name index.
         // Only in this order, querying the sources by e.g. ILDasm tool will work (e.g. the .language element in IL code will be correct).
         var srcNI = state.GetNameIndex( src.Name );
         var objNI = state.GetNameIndex( "" );
         var virtualNI = state.GetNameIndex( src.Name.ToLowerInvariant() );
         var srcInfo = Tuple.Create(
            virtualNI,
            objNI,
            srcNI,
            src
            );
         var hashBucketCount = srcHash.Length;
         var hIdx = hashFunction( srcHash, srcInfo );
         if ( hIdx.HasValue )
         {
            srcHash[hIdx.Value] = srcInfo;
         }
         else
         {
            // Need to fully re-hash whole thing here!!!
            var oldHashSize = hashBucketCount;
            Boolean success;
            TSourceHeaderInfo[] newHash;
            do
            {
               var newHashSize = oldHashSize * 2; // + BUCKETS_INCREASE;
               success = true;
               newHash = new TSourceHeaderInfo[newHashSize];
               foreach ( var info in srcHash.ConcatSingle( srcInfo ).Where( h => h != null ) )
               {
                  var curHash = hashFunction( newHash, info );
                  if ( curHash.HasValue )
                  {
                     newHash[curHash.Value] = info;
                  }
                  else
                  {
                     success = false;
                     break;
                  }
               }
               oldHashSize = newHashSize;
            } while ( !success );
            srcHash = newHash;
         }
      }

      // Write the source header block
      const Int32 SRC_HEADER_BLOCK_ENTRY_SIZE = 0x2C;
      var srcHashSize = srcHash.Length;
      var srcCount = sources.Count;
      var streamSize = 0x50 // Header size + magic int size
         + BinaryUtils.AmountOfPagesTaken( srcHashSize, 32 ) * sizeof( Int32 ) // Amount of integers for present bit vector
         + SRC_HEADER_BLOCK_ENTRY_SIZE * srcCount;
      state.AddNewIndexedStream(
         streamIndex,
         streamSize,
         ( array, idx ) =>
         {
            // langapi/pdb/pdb.h, SrcHeaderBlock and SrcHeaderOut structs
            const Int32 SRC_HEADER_VERSION = 19980827;
            array
               .WriteInt32LEToBytes( ref idx, SRC_HEADER_VERSION ) // Signature
               .WriteInt32LEToBytes( ref idx, streamSize ) // Stream size
               .WriteInt64LEToBytes( ref idx, DateTime.Now.Ticks ) // Timestamp ticks
               .WriteUInt32LEToBytes( ref idx, state.PDB.Age ) // PDB age
               .ZeroOut( ref idx, 44 ) // 44 bytes of zero padding
               .WriteInt32LEToBytes( ref idx, srcCount ) // Amount of sources
               .WriteInt32LEToBytes( ref idx, srcHashSize ) // Amount of hash buckets
               .WriteInt32LEToBytes( ref idx, 1 ); // Magic?

            // Present hashes bit vector
            var tmpUInt32 = 0u;
            for ( var i = 0; i < srcHashSize; ++i )
            {
               if ( srcHash[i] != null )
               {
                  tmpUInt32 |= ( 1u << ( i % 32 ) );
               }
               if ( i + 1 == srcHashSize || ( i != 0 && i % 32 == 0 ) )
               {
                  // Write UInt32
                  array.WriteUInt32LEToBytes( ref idx, tmpUInt32 );
               }
            }

            // Magic zero
            array.WriteInt32LEToBytes( ref idx, 0 );

            // Write hash info
            foreach ( var hashInfo in srcHash.Where( h => h != null ) )
            {

               array
                  .WriteInt32LEToBytes( ref idx, hashKeyExtractor( hashInfo ) ) // Hash key (lower-case string)
                  .WriteInt32LEToBytes( ref idx, SRC_HEADER_BLOCK_ENTRY_SIZE - sizeof( Int32 ) ) // The size of the rest of the record
                  .WriteInt32LEToBytes( ref idx, SRC_HEADER_VERSION ) // Version
                  .WriteInt32LEToBytes( ref idx, 0 ) // CRC of the data
                  .WriteInt32LEToBytes( ref idx, state.DataStreams[sources[hashInfo.Item4]].Item1 ) // Source stream byte count
                  .WriteInt32LEToBytes( ref idx, hashInfo.Item3 ) // source file name index
                  .WriteInt32LEToBytes( ref idx, hashInfo.Item2 ) // object file name index
                  .WriteInt32LEToBytes( ref idx, hashInfo.Item1 ) // virtual file name index
                  .WriteByteToBytes( ref idx, 0x65 ) // Compression algorithm
                  .WriteByteToBytes( ref idx, 0x00 ) // grFlags (1 if virtual)
                  .WriteInt16LEToBytes( ref idx, 0 ) // Pad to 4-byte size
                  .WriteInt64LEToBytes( ref idx, 0 ); // Reserved
            }

         },
         "/src/headerblock"
         );

   }

   private static void WriteTPIStream( this PDBWritingState state )
   {
      state.WriteTPIOrIPIStream( PDBIO.STREAM_INDEX_TPI );
   }

   private static void WriteIPIStream( this PDBWritingState state )
   {
      state.WriteTPIOrIPIStream( PDBIO.STREAM_INDEX_IPI );
   }

   private static void WriteTPIOrIPIStream( this PDBWritingState state, Int32 streamIndex )
   {
      const Int32 TPI_HEADER_SIZE = 0x38;
      const Int32 VERSION_80 = 20040203;
      const Int32 TI_LOWEST = 0x1000;
      const Int32 TI_HIGHEST = TI_LOWEST;
      const Int16 SN = -1;
      const Int16 SN_PAD = SN;
      const Int32 HASH_KEY_SIZE = sizeof( Int32 );
      const Int32 HASH_BUCKET_COUNT = 0x0003FFFF;
      const Int32 HASH_VALUES_OFFSET = 0;
      const Int32 HASH_VALUES_COUNT = -1;
      const Int32 TI_PAIRS_OFFSET = 0;
      const Int32 TI_PAIRS_COUNT = -1;
      const Int32 HASH_HEAD_LIST_OFFSET = 0;
      const Int32 HASH_HEAD_LIST_COUNT = -1;

      state.AddNewIndexedStream(
         streamIndex,
         TPI_HEADER_SIZE,
         ( array, idx ) =>
          array
             // HDR struct
             .WriteInt32LEToBytes( ref idx, VERSION_80 ) // Version
             .WriteInt32LEToBytes( ref idx, TPI_HEADER_SIZE ) // Size of this header, including version
             .WriteInt32LEToBytes( ref idx, TI_LOWEST ) // Lowest TI
             .WriteInt32LEToBytes( ref idx, TI_HIGHEST ) // Highest TI + 1
             .WriteInt32LEToBytes( ref idx, 0 ) // Byte size of REC stream, is always empty

             // TpiHash struct
             .WriteInt16LEToBytes( ref idx, SN ) // Main hash stream
             .WriteInt16LEToBytes( ref idx, SN_PAD ) // Auxiliary hash data, if needed
             .WriteInt32LEToBytes( ref idx, HASH_KEY_SIZE ) // Byte count for each hash key
             .WriteUInt32LEToBytes( ref idx, HASH_BUCKET_COUNT ) // Count of hash buckets
             .WriteInt32LEToBytes( ref idx, HASH_VALUES_OFFSET ) // Byte offset of hash values
             .WriteInt32LEToBytes( ref idx, HASH_VALUES_COUNT ) // Byte count of hash values
             .WriteInt32LEToBytes( ref idx, TI_PAIRS_OFFSET ) // Byte offset of (TI,OFF) pairs
             .WriteInt32LEToBytes( ref idx, TI_PAIRS_COUNT ) // Byte count of (TI,OFF) pairs
             .WriteInt32LEToBytes( ref idx, HASH_HEAD_LIST_OFFSET ) // Byte offset of hash head list
             .WriteInt32LEToBytes( ref idx, HASH_HEAD_LIST_COUNT ) // Byte count of hash head list
      );
   }

   private static void WriteRootStream( this PDBWritingState state )
   {
      var namedDataStreams = state.DataStreamNames;
      var strByteCount = namedDataStreams.Keys.Aggregate( 0, ( cur, str ) => cur + PDBIO.NameEncoding.SafeByteCount( str, true ) );
      // Amount of int32's needed to write
      var namedDataStreamsPresentBitSetCount = BinaryUtils.AmountOfPagesTaken( namedDataStreams.Count, 32 );
      // Root stream is unnamed, and always at index 1
      state.AddNewIndexedStream(
         PDBIO.STREAM_INDEX_ROOT,
         40 + GUID_SIZE // Minimum size
         + namedDataStreamsPresentBitSetCount * sizeof( Int32 ) // Present bit set length
         + namedDataStreams.Count * 8 // How many bytes for each name
         + strByteCount, // How many bytes for strings
         ( array, idx ) =>
         {
            var instance = state.PDB;
            array
               .WriteUInt32LEToBytes( ref idx, 0x01312E94 ) // 2000 04 04
               .WriteUInt32LEToBytes( ref idx, instance.Timestamp ) // Timestamp
               .WriteUInt32LEToBytes( ref idx, instance.Age )
               .WriteGUIDToBytes( ref idx, instance.DebugGUID )
               .WriteInt32LEToBytes( ref idx, strByteCount );
            var strStartIdx = idx;
            var afterStrIdx = idx + strByteCount;
            array
               .WriteInt32LEToBytes( ref afterStrIdx, namedDataStreams.Count ) // Amount of named streams
               .WriteInt32LEToBytes( ref afterStrIdx, namedDataStreams.Count ) // Amount of set bits (same as size)
               .WriteInt32LEToBytes( ref afterStrIdx, namedDataStreamsPresentBitSetCount ); // How many int32's bit set takes

            // Write set bits
            for ( var i = 0; i < namedDataStreamsPresentBitSetCount; ++i )
            {
               var bits = UInt32.MaxValue;
               if ( i == namedDataStreamsPresentBitSetCount - 1 )
               {
                  bits >>= 32 - namedDataStreams.Count % 32;
               }
               array.WriteUInt32LEToBytes( ref afterStrIdx, bits );
            }
            // Write deleted bits size (0)
            array.WriteInt32LEToBytes( ref afterStrIdx, 0 );
            // Write strings and their indices
            foreach ( var kvp in namedDataStreams )
            {
               // First, the index of string in array and its stream index
               array
                  .WriteInt32LEToBytes( ref afterStrIdx, idx - strStartIdx )
                  .WriteInt32LEToBytes( ref afterStrIdx, kvp.Value );
               // Then write the string
               array.WriteZeroTerminatedString( ref idx, kvp.Key );
            }

            array
               .WriteInt32LEToBytes( ref afterStrIdx, 0 ) // Magic (?) zero
               .WriteInt32LEToBytes( ref afterStrIdx, 20140516 ); // Some kind of version...
         } );

   }

   private static void WriteDBIStream( this PDBWritingState state )
   {
      var instance = state.PDB;
      var funcSecContribs = new List<PDBIO.DBISecCon>( instance.Modules.Aggregate( 0, ( cur, m ) => cur + m.Functions.Count ) );
      var moduleInfos = new List<Tuple<PDBIO.DBIModuleInfo, String[]>>();
      var dbiHeader = new PDBIO.DBIHeader();

      // 1. Write all module streams
      // This will populate SymbolRecord and GlobalSymbol streams
      foreach ( var module in instance.Modules )
      {
         state.WritePDBModule( module, dbiHeader, moduleInfos, funcSecContribs );
      }

      // 2. Write GlobalSymbols stream
      dbiHeader.gsSymStream = (UInt16) state.WriteGlobalSymbolStream();

      // 3. Write PublicSymbols stream
      dbiHeader.psSymStream = (UInt16) state.WritePublicSymbolStream();

      // 4. Write SymbolRecord stream
      dbiHeader.symRecStream = (UInt16) state.WriteSymbolRecordStream();

      // 5. Write actual DBI stream
      dbiHeader.age = instance.Age;
      var usedFiles = moduleInfos
         .SelectMany( t => t.Item2 )
         .ToArray();
      var moduleFileNameIndex = new PDBNameIndex( 0 );
      foreach ( var fn in usedFiles )
      {
         moduleFileNameIndex.GetNameIndex( fn );
      }

      dbiHeader.secConSize = INT_SIZE + funcSecContribs.Count * 28; // SectionContribution size is 28 bytes
      dbiHeader.secMapSize = moduleInfos.Count > 0 ? 44 : INT_SIZE; // Section map size is 44 bytes
      dbiHeader.fileInfoSize = INT_SIZE
         + INT_SIZE * moduleInfos.Count
         + INT_SIZE * usedFiles.Length
         + moduleFileNameIndex.CurrentIndexValue;
      Align4( ref dbiHeader.fileInfoSize );
      dbiHeader.debugHeaderSize = moduleInfos.Count > 0 ? 22 : 0; // Debug header size is 22 bytes

      var secConSize = 0u;
      var secConStream = state.DataStreams.Count;
      state.AddNewIndexedStream(
         PDBIO.STREAM_INDEX_DBI,
         64 // DBI header size
         + dbiHeader.moduleInfoSize // Module infos
         + dbiHeader.fileInfoSize // File infos
         + dbiHeader.secConSize // Section contributions
         + dbiHeader.secMapSize // Section map sizes
         + dbiHeader.ecInfoSize // EC info
         + dbiHeader.debugHeaderSize, // Debug header
         ( array, idx ) =>
         {
            // Write DBI header
            dbiHeader.WriteDBIHeader( array, ref idx );
            // Write module infos
            foreach ( var mInfo in moduleInfos )
            {
               mInfo.Item1.WriteToArray( array, ref idx );
            }
            // Section contribution
            array.WriteUInt32LEToBytes( ref idx, 0xF12EBA2D ); // Signature
            foreach ( var secCon in funcSecContribs )
            {
               secCon.offset = secConSize;
               secCon.WriteToArray( array, ref idx );
               secConSize += secCon.size;
            }
            if ( secConSize > 0 )
            {
               // Section map, fixed prefix (cSeg, cSegLog, flags, ovl1, grp, frame, iSegName, iClassName, offset)
               array
                  .BlockCopyFrom( ref idx, new Byte[] { 0x02, 0x00, 0x02, 0x00, 0x0D, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00 } )
                  .WriteUInt32LEToBytes( ref idx, secConSize ) // Section size
                  .BlockCopyFrom( ref idx, new Byte[] { 0x08, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF } ); // Section map, fixed suffix.
            }
            else
            {
               array.WriteInt32LEToBytes( ref idx, 0 );
            }
            // File info
            array
               .WriteUInt16LEToBytes( ref idx, (UInt16) moduleInfos.Count ) // Amount of modules
               .WriteUInt16LEToBytes( ref idx, (UInt16) usedFiles.Length ); // Amount of file references in modules
            UInt16 fiIdx = 0;
            foreach ( var mInfo in moduleInfos )
            {
               array.WriteUInt16LEToBytes( ref idx, fiIdx );
               fiIdx += (UInt16) mInfo.Item2.Length;
            }
            foreach ( var mInfo in moduleInfos )
            {
               array.WriteUInt16LEToBytes( ref idx, (UInt16) mInfo.Item2.Length );
            }

            foreach ( var uf in usedFiles )
            {
               array.WriteInt32LEToBytes( ref idx, moduleFileNameIndex.GetNameIndex( uf ) );
            }
            foreach ( var uf in moduleFileNameIndex.NameIndex.Keys ) // TODO currently this works, since we are iterating in same order as adding. This might change some day....!!!
            {
               array.WriteZeroTerminatedString( ref idx, uf );
            }
            Align4( ref idx );

            // EC info
            array.BlockCopyFrom( ref idx, new Byte[] { 0xFE, 0xEF, 0xFE, 0xEF, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } );

            // Debug header
            if ( dbiHeader.debugHeaderSize > 0 )
            {
               new PDBIO.DBIDebugHeader( (UInt16) secConStream ).WriteToArray( array, ref idx );
            }
         } );

      // Write section header stream. This is needed to set global RVA to zero.
      if ( secConSize > 0 )
      {
         state.AddNewIndexedStream(
            secConStream,
            40,
            ( array, idx ) => array
               .ZeroOut( ref idx, 8 )
               .WriteUInt32LEToBytes( ref idx, secConSize )
               .ZeroOut( ref idx, 4 )
               .WriteUInt32LEToBytes( ref idx, secConSize )
               .ZeroOut( ref idx, 20 )
            );
      }
   }

   private static void WriteNameIndex( this PDBWritingState state )
   {
      // Write name index
      var nameIdx = state.NameIndex;
      var strByteCount = nameIdx.CurrentIndexValue;
      var nameIndex = nameIdx.NameIndex;
      var strCount = nameIndex.Count;
      state.AddNewIndexedStream(
         6,
         25 // Fixed size
         + strByteCount // Serialized strings
         + INT_SIZE * strCount, // Each string is prepended by its 'id'
         ( array, idx ) =>
         {
            array
               .WriteUInt32LEToBytes( ref idx, 0xEFFEEFFE ) // Signature
               .WriteUInt32LEToBytes( ref idx, 0x00000001 ) // Version
               .WriteInt32LEToBytes( ref idx, strByteCount ); // String byte count
            var afterStrIdx = idx + strByteCount;
            array.WriteByteToBytes( ref idx, 0 ); // 1 zero byte to distinguish zero indices
            array.WriteInt32LEToBytes( ref afterStrIdx, nameIndex.Count + 1 ); // Max amount of names
            foreach ( var kvp in nameIndex )
            {
               // Write index information first
               array
                  .WriteInt32LEToBytes( ref afterStrIdx, kvp.Value );
               // Write string
               array.WriteZeroTerminatedString( ref idx, kvp.Key );
            }
            array
               .WriteInt32LEToBytes( ref afterStrIdx, 0 ) // Magic zero
               .WriteInt32LEToBytes( ref afterStrIdx, nameIndex.Count ); // Amount of names
         },
         PDBIO.NAMES_STREAM_NAME
         );
   }

   private static void FinalizePDBWriting( this PDBWritingState state )
   {
      // Write directory first
      Int32 directoryByteCount;
      var directoryPages = state.WriteDirectory( out directoryByteCount );

      // Then, write PDB header to the beginning of the stream
      state.SeekToPage( 0 );
      var pagesUsed = state.WritePDBHeader( directoryByteCount, directoryPages );

      // Then, free page map (DBI will refuse to load the file if this is not present)
      state.WriteFreePageMap( pagesUsed );
   }

   private static Int32[] WriteDirectory( this PDBWritingState state, out Int32 directoryByteCount )
   {
      // Write directory
      var dataStreams = state.DataStreams;
      directoryByteCount = INT_SIZE // Count of data streams
         + INT_SIZE * dataStreams.Count // The size of each data stream
         + dataStreams.Aggregate( 0, ( cur, tuple ) => cur + ( tuple == null ? 0 : tuple.Item2.Length * INT_SIZE ) ); // The size of page info for each data stream

      var directoryPages = state.WritePagedData( new TWriteAction( directoryByteCount, ( array, idx ) =>
      {
         array.WriteInt32LEToBytes( ref idx, dataStreams.Count );
         foreach ( var tuple in dataStreams )
         {
            array.WriteInt32LEToBytes( ref idx, tuple == null ? 0 : tuple.Item1 );
         }
         foreach ( var tuple in dataStreams )
         {
            if ( tuple != null )
            {
               foreach ( var page in tuple.Item2 )
               {
                  array.WriteInt32LEToBytes( ref idx, page );
               }
            }
         }
      } ) ).Item2;


      // Write directory page information
      directoryPages = state.WritePagedData( new TWriteAction( directoryPages.Length * INT_SIZE, ( array, idx ) =>
      {
         foreach ( var page in directoryPages )
         {
            array.WriteInt32LEToBytes( ref idx, page );
         }
      } ) ).Item2;

      return directoryPages;
   }

   private static Int32 WritePDBHeader( this PDBWritingState state, Int32 directoryByteCount, Int32[] directoryPages )
   {
      var pagesUsed = directoryPages[directoryPages.Length - 1] + 1;

      var headerSize = 52 // Header fixed size
         + directoryPages.Length * INT_SIZE; // Directory page sizes
      var pageSize = state.PageSize;
      if ( headerSize > pageSize )
      {
         throw new PDBException( "Header is larger than pagesize." );
      }
      state.WritePagedData( new TWriteAction( headerSize, ( array, idx ) =>
      {
         array
         .BlockCopyFrom( ref idx, new Byte[]
         {
                    0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0x20, 0x43, 0x2F, 0x43, 0x2B, 0x2B, 0x20, // Microsoft C/C++ 
                    0x4D, 0x53, 0x46, 0x20, 0x37, 0x2E, 0x30, 0x30, 0x0D, 0x0A, 0x1A, 0x44, 0x53, 0x00, 0x00, 0x00  // MSF 7.00...DS...
         }, 0, 32 )
         .WriteInt32LEToBytes( ref idx, pageSize ) // Page size
         .WriteInt32LEToBytes( ref idx, START_PAGE - 1 ) // Free page map
         .WriteInt32LEToBytes( ref idx, pagesUsed ) // Pages used
         .WriteInt32LEToBytes( ref idx, directoryByteCount ) // Directory byte count
         .WriteInt32LEToBytes( ref idx, 0 ); // Magic zero

         // Write directory page information pages
         foreach ( var page in directoryPages )
         {
            array.WriteInt32LEToBytes( ref idx, page );
         }
      } ) );

      return pagesUsed;
   }

   private static void WriteFreePageMap( this PDBWritingState state, Int32 pagesUsed )
   {
      var pageSize = state.PageSize;
      state.WritePagedData( new TWriteAction( pageSize * ( START_PAGE - 1 ), ( array, idx ) =>
      {
         var reserved = ( pagesUsed + 1 ) / 2;
         var max = pageSize / INT_SIZE;
         for ( var i = 0; i < max; ++i )
         {
            var ui = UInt32.MaxValue;
            if ( i * 32 < reserved )
            {
               ui <<= reserved;
               reserved -= 32;
            }
            array.WriteUInt32LEToBytes( ref idx, ui );
         }
         reserved = ( pagesUsed + 1 ) - ( ( pagesUsed + 1 ) / 2 );
         for ( var i = 0; i < max; ++i )
         {
            var ui = UInt32.MaxValue;
            if ( i * 32 < reserved )
            {
               ui <<= reserved;
               reserved -= 32;
            }
            array.WriteUInt32LEToBytes( ref idx, ui );
         }
      } ) );
   }

   private static void WritePDBModule(
      this PDBWritingState state,
      PDBModule module,
      PDBIO.DBIHeader dbiHeader,
      List<Tuple<PDBIO.DBIModuleInfo, String[]>> moduleInfos,
      List<PDBIO.DBISecCon> funcSecContribs
      )
   {
      var mInfo = new PDBIO.DBIModuleInfo( module.Name, module.ObjectName );
      var sourceFileNames = new HashSet<String>( module.Functions
         .SelectMany( f => f.Lines.Select( l => l.Source.Name ) )
         ).ToArray();

      mInfo.files = (UInt16) sourceFileNames.Length;
      dbiHeader.moduleInfoSize += 64 // Module info fixed size
         + PDBIO.NameEncoding.SafeByteCount( mInfo.moduleName, true )
         + PDBIO.NameEncoding.SafeByteCount( mInfo.objectName, true );
      Align4( ref dbiHeader.moduleInfoSize );

      var streamIndex = state.DataStreams.Count;
      mInfo.stream = (UInt16) streamIndex;

      state.AddNewIndexedStreamInPieces( streamIndex, state.GetWriteActionsForModule( module, moduleInfos.Count, mInfo, sourceFileNames, funcSecContribs ) );

      // Add after writing stream
      moduleInfos.Add( Tuple.Create( mInfo, sourceFileNames ) );
   }


   private static IEnumerable<TWriteAction> GetWriteActionsForModule(
      this PDBWritingState state,
      PDBModule module,
      Int32 moduleIndex,
      PDBIO.DBIModuleInfo mInfo,
      String[] sourceFileNames,
      List<PDBIO.DBISecCon> funcSecContribs
      )
   {
      // 1. Signature
      const Int32 SIG_SIZE = sizeof( Int32 );
      yield return new TWriteAction( SIG_SIZE, ( array, idx ) => array.WriteInt32LEToBytes( ref idx, 4 ) );

      // 2. Functions
      var funcOffset = 0u;
      UInt32 funcPointer = SIG_SIZE;
      var funcInfos = new List<PDBIO.PDBFunctionInfo>( module.Functions.Count );

      foreach ( var func in module.Functions )
      {
         var preludeLength = 4
            + 37 // block len + global managed function SYM + fixed size + name + 4 byte boundary
            + PDBIO.NameEncoding.SafeByteCount( func.Name, true );
         Align4( ref preludeLength );

         List<Int32> usedNSCount;
         var funcBlockLen = func.CalculateSymbolByteCount( out usedNSCount );
         var size = preludeLength + funcBlockLen;
         Align4( ref size );
         yield return new TWriteAction( size, ( array, idx ) =>
         {
            func.WritePDBFunction( moduleIndex, array, preludeLength, funcBlockLen, usedNSCount, funcPointer, funcOffset ); // Subtract 4 from block length, since we need to point to END_SYM instead of position after END_SYM
            var funcLen = (UInt32) func.Length;
            funcSecContribs.Add( new PDBIO.DBISecCon()
            {
               module = (UInt16) moduleIndex,
               section = 1,
               size = funcLen
            } );
            funcInfos.Add( new PDBIO.PDBFunctionInfo( func, funcOffset, 1, funcPointer ) );
         } );
         funcOffset += (UInt32) func.Length;
         funcPointer += (UInt32) size;
      }

      mInfo.symbolByteCount = (Int32) funcPointer;

      // 3. Sources
      var sourceIndices = new Dictionary<String, Int32>();
      var sourcesByteCount = sourceFileNames.Length * 8;
      const Int32 SOURCE_INFO_START_PADDING = 8;
      var sourcesTotalByteCount = sourcesByteCount > 0 ? ( SOURCE_INFO_START_PADDING + sourcesByteCount ) : 0;
      if ( sourcesTotalByteCount > 0 )
      {
         yield return new TWriteAction( sourcesTotalByteCount, ( array, idx ) =>
         {
            array
               .WriteInt32LEToBytes( ref idx, PDBIO.SYM_DEBUG_SOURCE_INFO ) // Sources
               .WriteInt32LEToBytes( ref idx, sourcesByteCount ); // Amount of bytes
            foreach ( var src in sourceFileNames )
            {
               // Save information about the index of this source within this block
               sourceIndices.Add( src, idx - SOURCE_INFO_START_PADDING );

               array
                  .WriteInt32LEToBytes( ref idx, state.GetNameIndex( src ) ) // Name of the source
                  .WriteInt32LEToBytes( ref idx, 0 ); // length-byte (0), kind-byte (0), padding
            }
         } );
      }

      // 4. Lines
      var linesByteCount = 0;
      foreach ( var funcI in funcInfos )
      {
         var curWriteInfo = WritePDBFunctionLines( funcI.function, funcI.address, sourceIndices );
         if ( curWriteInfo != null )
         {
            yield return curWriteInfo;
            linesByteCount += curWriteInfo.Item1;
         }
      }

      mInfo.linesByteCount = sourcesTotalByteCount + linesByteCount;

      // 5. Write SymRec stream refs, and update GSSym at the same time
      yield return new TWriteAction( INT_SIZE, ( array, idx ) => array.WriteInt32LEToBytes( ref idx, 8 * funcInfos.Count ) );

      // PROCREF & TOKENREF use 1-based module indexing, so increment module index here
      ++moduleIndex;

      // Then write PROCREF & TOKENREF info
      var symRecStream = state.SymRecStream;
      var globals = state.Globals;
      foreach ( var fInfo in funcInfos )
      {
         // Record offset to module stream
         yield return new TWriteAction( INT_SIZE * 2, ( array, idx ) =>
         {
            var sRef = state.SymRecIndex;
            array.WriteInt32LEToBytes( ref idx, sRef ); // Pointer to the start of this function's PROCREF

            // Write PROCREF to symrec stream
            symRecStream
               .Skip( ref state.SymRecIndex, 2 ) // Byte count, skip for now
               .WriteUInt16LEToBytes( ref state.SymRecIndex, 0x1125 ) // S_PROCREF
               .WriteInt32LEToBytes( ref state.SymRecIndex, 0 ) // Checksum (always zero)
               .WriteUInt32LEToBytes( ref state.SymRecIndex, fInfo.funcPointer ) // Function pointer within the module stream
               .WriteUInt16LEToBytes( ref state.SymRecIndex, (UInt16) moduleIndex ); // Module index
            var tmp = state.SymRecIndex;
            symRecStream.WriteZeroTerminatedString( ref state.SymRecIndex, fInfo.function.Name ); // Function name

            // Hash function name
            globals.RecordNamedSymcRecReference( tmp, sRef );

            // Pad to 4-byte boundary
            Align4( ref state.SymRecIndex );
            // Revisit byte count
            symRecStream.WriteUInt16LEToBytes( ref sRef, (UInt16) ( state.SymRecIndex - sRef - 2 ) );

            // Record offset to module stream
            sRef = state.SymRecIndex;
            array.WriteInt32LEToBytes( ref idx, state.SymRecIndex ); // Pointer to the start of this function's TOKENREF

            // Write TOKENREF to symrec stream
            symRecStream
                .WriteUInt16LEToBytes( ref state.SymRecIndex, TOKENREF_FIXED_SIZE - 2 ) // Byte count, always fixed since token ref fixed size (+ padding)
                .WriteUInt16LEToBytes( ref state.SymRecIndex, 0x1129 ) // S_TOKENREF
                .WriteInt32LEToBytes( ref state.SymRecIndex, 0 ) // Checksum (always zero)
                .WriteUInt32LEToBytes( ref state.SymRecIndex, fInfo.funcPointer ) // Function pointer within the module stream
                .WriteUInt16LEToBytes( ref state.SymRecIndex, (UInt16) moduleIndex ); // Module index
            tmp = state.SymRecIndex;
            symRecStream.WriteZeroTerminatedString( ref state.SymRecIndex, fInfo.function.Token.ToString( "x8" ) ); // Fixed-length hexadecimal token value, with lower-case letters (upper-case won't work).

            // Hash tokenref textual name
            globals.RecordNamedSymcRecReference( tmp, sRef );

            Align4( ref state.SymRecIndex );
         } );
      }
   }

   private static TWriteAction WritePDBFunctionLines(
      PDBFunction func,
      UInt32 address,
      Dictionary<String, Int32> sourceIndices
      )
   {
      var lineInfo = new Dictionary<String, List<PDBLine>>();
      foreach ( var line in func.Lines )
      {
         lineInfo
            .GetOrAdd_NotThreadSafe( line.Source.Name, s => new List<PDBLine>() )
            .Add( line );
      }

      const Int32 LINE_PER_SOURCE_MULTIPLIER = 12; // Source index (int32), line count (int32), byte count (int32)
      var lineSize = 20 // Line info including SYM_DEBUG_LINE_INFO (int32) and block byte size (int32), also (2x->)1x int32s for sym-rec-stream refs
         + lineInfo.Count * LINE_PER_SOURCE_MULTIPLIER // For each used source, source index + line count + byte count
         + lineInfo.Values.Aggregate( 0, ( cur, l ) => cur + l.Count * PDBIO.LINE_MULTIPLIER + l.Where( ll => ll.ColumnEnd.HasValue && ll.ColumnStart.HasValue ).Count() * PDBIO.COLUMN_MULTIPLIER ); // For each line, line info + optional column info
      return lineInfo.Count > 0 ? new TWriteAction(
         lineSize,
         ( array, idx ) =>
         {
            var lenIdx = idx + 4;
            array
               .WriteInt32LEToBytes( ref idx, PDBIO.SYM_DEBUG_LINE_INFO ) // Line info
               .Skip( ref idx, 4 ) // Byte count of this block excluding sym + this count
               .WriteUInt32LEToBytes( ref idx, address ) // Function address
               .WriteUInt16LEToBytes( ref idx, 1 ) // Segment
               .WriteUInt16LEToBytes( ref idx, (UInt16) ( func.Lines.Any( l => l.ColumnStart.HasValue && l.ColumnEnd.HasValue ) ? 1 : 0 ) ) // Flags: 1 if there is even one line present with column information, 0 otherwise
               .WriteInt32LEToBytes( ref idx, func.Length ); // Function length is duplicated here
            foreach ( var kvp in lineInfo )
            {
               var lines = kvp.Value;
               var lineByteSize = lines.Count * PDBIO.LINE_MULTIPLIER;
               array
                  .WriteInt32LEToBytes( ref idx, sourceIndices[kvp.Key] ) // Source info index within its block
                  .WriteInt32LEToBytes( ref idx, lines.Count ) // How many lines
                  .WriteInt32LEToBytes( ref idx, LINE_PER_SOURCE_MULTIPLIER + lineByteSize + lines.Where( ll => ll.ColumnEnd.HasValue && ll.ColumnStart.HasValue ).Count() * PDBIO.COLUMN_MULTIPLIER ); // How many bytes the line info takes, including source info + line count
               var colIdx = idx + lineByteSize;
               foreach ( var line in lines )
               {
                  // Write line information
                  // Line-flags: low 3 bytes is line start, 7 bits of highest byte is line delta, and highest bit is whether line is statement
                  var lineFlags = ( (UInt32) line.LineStart & 0x00ffffffu ) | ( ( (UInt32) ( line.LineEnd - line.LineStart ) << 24 ) & 0x7f000000u );
                  if ( !line.IsStatement )
                  {
                     lineFlags |= 0x80000000u;
                  }
                  array
                     .WriteInt32LEToBytes( ref idx, line.Offset ) // Line offset
                     .WriteUInt32LEToBytes( ref idx, lineFlags ); // Line flags
                  if ( line.ColumnStart.HasValue && line.ColumnEnd.HasValue )
                  {
                     // Write column info
                     array
                     .WriteUInt16LEToBytes( ref colIdx, line.ColumnStart.Value )
                     .WriteUInt16LEToBytes( ref colIdx, line.ColumnEnd.Value );
                  }
               }
               // Set current index to point after columns
               idx = colIdx;
            }
            // Revisit byte count
            array.WriteInt32LEToBytes( ref lenIdx, idx - lenIdx - 4 );
         } ) : null;
   }

   private static void WritePDBFunction(
      this PDBFunction func,
      Int32 moduleIndex,
      Byte[] array,
      Int32 preludeLength,
      Int32 funcBlockLen,
      List<Int32> usedNSCount,
      UInt32 funcPointer,
      UInt32 funcOffset
      )
   {
      var idx = 2; // Skip block length
      // Block length + SYM (global managed function) + function fixed data
      array
         .WriteUInt16LEToBytes( ref idx, PDBIO.SYM_GLOBAL_MANAGED_FUNC ) // global managed function
         .WriteInt32LEToBytes( ref idx, 0 ); // parent
      var endIdx = idx;
      idx += 4;
      array
         //.WriteUInt32LEToBytes( ref idx, funcPointer + (UInt32) preludeLength + (UInt32) funcBlockLen ) // function end pointer
         .WriteInt32LEToBytes( ref idx, 0 ) // next
         .WriteUInt32LEToBytes( ref idx, (UInt32) func.Length ) // length
         .WriteInt32LEToBytes( ref idx, 0 ) // debug start
         .WriteInt32LEToBytes( ref idx, 0 ) // debug end
         .WriteUInt32LEToBytes( ref idx, func.Token ) // token
         .WriteUInt32LEToBytes( ref idx, funcOffset ) // address
         .WriteUInt16LEToBytes( ref idx, 1 ) // segment
         .WriteByteToBytes( ref idx, 0 ) // flags
         .WriteUInt16LEToBytes( ref idx, 0 ) // returnReg
         .WriteZeroTerminatedString( ref idx, func.Name ) // name
         .Align4( ref idx ); // 4-byte border

      // Revisit block length
      array.WriteBlockLength( 0, idx );

      WriteScopeOrFunctionBlocks( func, array, ref idx, funcPointer, funcOffset, funcPointer );
      WriteOEM( func, array, ref idx, usedNSCount );
      array.Align4( ref idx );
      array.WriteInt32LEToBytes( ref endIdx, (Int32) funcPointer + idx );
      WriteENDSym( array, ref idx );
   }

   private static void WriteScopeOrFunctionBlocks( PDBScopeOrFunction scope, Byte[] array, ref Int32 idx, UInt32 funcPointer, UInt32 functionAddress, UInt32 parentPointer )
   {
      var startIdx = idx;

      // Used namespaces
      foreach ( var un in scope.UsedNamespaces )
      {
         var lenIdx = idx;
         array
            .Skip( ref idx, 2 ) // Block size
            .WriteUInt16LEToBytes( ref idx, PDBIO.SYM_USED_NS ) // UNAMESPACE
            .WriteZeroTerminatedString( ref idx, un ) // namespace
            .Align4( ref idx ); // 4-byte border
                                // Revisit size
         array.WriteBlockLength( lenIdx, idx );
      }

      // Slots
      foreach ( var slot in scope.Slots )
      {
         var lenIdx = idx;
         array
            .Skip( ref idx, 2 ) // Block size
            .WriteUInt16LEToBytes( ref idx, PDBIO.SYM_MANAGED_SLOT ) // MANSLOT
            .WriteInt32LEToBytes( ref idx, slot.SlotIndex ) // slot index
            .WriteUInt32LEToBytes( ref idx, slot.TypeToken ) // type token
            .WriteInt32LEToBytes( ref idx, 0 ) // address
            .WriteUInt16LEToBytes( ref idx, 0 ) // segment
            .WriteUInt16LEToBytes( ref idx, (UInt16) slot.Flags ) // flags
            .WriteZeroTerminatedString( ref idx, slot.Name ) // name
            .Align4( ref idx ); // 4-byte border
                                // Revisit size
         array.WriteBlockLength( lenIdx, idx );
      }

      // Constants
      foreach ( var constant in scope.Constants )
      {
         WritePDBConstant( constant, array, ref idx );
      }

      // Scopes
      foreach ( var innerScope in scope.Scopes )
      {
         var lenIdx = idx;
         var thisPointer = (UInt32) ( funcPointer + ( idx - startIdx ) );
         array
            .Skip( ref idx, 2 ) // block size
            .WriteUInt16LEToBytes( ref idx, PDBIO.SYM_SCOPE ) // BLOCK_32
            .WriteUInt32LEToBytes( ref idx, parentPointer ); // parent
         var endIdx = idx;
         array
            .Skip( ref idx, 4 ) // end pointer
            .WriteInt32LEToBytes( ref idx, innerScope.Length ) // length
            .WriteUInt32LEToBytes( ref idx, functionAddress + (UInt32) innerScope.Offset ) // address
            .WriteUInt16LEToBytes( ref idx, 1 ) // segment
            .WriteZeroTerminatedString( ref idx, innerScope.Name ) // name
            .Align4( ref idx ); // 4-byte border
                                // Revisit size
         array.WriteBlockLength( lenIdx, idx );
         // Revisit end pointer
         //array.WriteUInt32LEToBytes( ref endIdx, funcPointer + (UInt32) idx + (UInt32) CalculateByteCountFromLists( innerScope ) );
         // Write inner scope's lists
         WriteScopeOrFunctionBlocks( innerScope, array, ref idx, funcPointer, functionAddress, thisPointer );
         // Write END sym
         array.Align4( ref idx );
         array.WriteInt32LEToBytes( ref endIdx, (Int32) funcPointer + idx );
         WriteENDSym( array, ref idx );
      }


   }

   private static void WritePDBConstant( PDBConstant constant, Byte[] array, ref Int32 idx )
   {
      // Remember block length start
      var startIdx = idx;
      idx += sizeof( UInt16 );

      // SYM_MANAGED_CONSTANT
      array.WriteUInt16LEToBytes( ref idx, PDBIO.SYM_MANAGED_CONSTANT );

      // Token
      array.WriteUInt32LEToBytes( ref idx, constant.Token );

      // Value
      var val = constant.Value;
      if ( val == null )
      {
         array.WriteUInt16LEToBytes( ref idx, PDBIO.CONST_LF_LONG )
            .WriteInt32LEToBytes( ref idx, 0 );
      }
      else
      {
         switch ( Type.GetTypeCode( val.GetType() ) )
         {
            case TypeCode.Boolean:
               array.WriteUInt16LEToBytes( ref idx, (UInt16) ( (Boolean) val ? 1 : 0 ) );
               break;
            case TypeCode.Char:
               array
                  .WriteUInt16LEToBytes( ref idx, PDBIO.CONST_LF_USHORT )
                  .WriteUInt16LEToBytes( ref idx, (UInt16) (Char) val );
               break;
            case TypeCode.SByte:
               var i1 = (SByte) val;
               if ( i1 < 0 )
               {
                  array
                     .WriteUInt16LEToBytes( ref idx, PDBIO.CONST_LF_CHAR )
                     .WriteSByteToBytes( ref idx, i1 );
               }
               else
               {
                  array.WriteUInt16LEToBytes( ref idx, (UInt16) i1 );
               }
               break;
            case TypeCode.Byte:
               array.WriteUInt16LEToBytes( ref idx, (Byte) val );
               break;
            case TypeCode.Int16:
               var i16 = (Int16) val;
               //if ( i16 < 0 || i16 > PDBIO.MAX_PDB_CONSTANT_NUMERIC )
               //{
               array
                  .WriteUInt16LEToBytes( ref idx, PDBIO.CONST_LF_SHORT );

               //}
               array.WriteInt16LEToBytes( ref idx, i16 );
               break;
            case TypeCode.UInt16:
               var u16 = (UInt16) val;
               //if ( u16 > PDBIO.MAX_PDB_CONSTANT_NUMERIC )
               //{
               array.WriteUInt16LEToBytes( ref idx, PDBIO.CONST_LF_USHORT );
               //}
               array.WriteUInt16LEToBytes( ref idx, u16 );
               break;
            case TypeCode.Int32:
               var i32 = (Int32) val;
               //if ( i32 < 0 || i32 > PDBIO.MAX_PDB_CONSTANT_NUMERIC )
               //{
               array.WriteUInt16LEToBytes( ref idx, PDBIO.CONST_LF_LONG )
                  .WriteInt32LEToBytes( ref idx, i32 );
               //}
               //else
               //{
               //array.WriteUInt16LEToBytes( ref idx, (UInt16) i32 );
               //}
               break;
            case TypeCode.UInt32:
               var u32 = (UInt32) val;
               //if ( u32 > PDBIO.MAX_PDB_CONSTANT_NUMERIC )
               //{
               array.WriteUInt16LEToBytes( ref idx, PDBIO.CONST_LF_ULONG )
                  .WriteUInt32LEToBytes( ref idx, u32 );
               //}
               //else
               //{
               //   array.WriteUInt16LEToBytes( ref idx, (UInt16) u32 );
               //}
               break;
            case TypeCode.Int64:
               var i64 = (Int64) val;
               //if ( i64 < 0 || i64 > PDBIO.MAX_PDB_CONSTANT_NUMERIC )
               //{
               array.WriteUInt16LEToBytes( ref idx, PDBIO.CONST_LF_QUADWORD )
                  .WriteInt64LEToBytes( ref idx, i64 );
               //}
               //else
               //{
               //   array.WriteUInt16LEToBytes( ref idx, (UInt16) i64 );
               //}
               break;
            case TypeCode.UInt64:
               var u64 = (UInt64) val;
               //if ( u64 > PDBIO.MAX_PDB_CONSTANT_NUMERIC )
               //{
               array.WriteUInt16LEToBytes( ref idx, PDBIO.CONST_LF_UQUADWORD )
                  .WriteUInt64LEToBytes( ref idx, u64 );
               //}
               //else
               //{
               //   array.WriteUInt16LEToBytes( ref idx, (UInt16) u64 );
               //}
               break;
            case TypeCode.Single:
               array.WriteUInt16LEToBytes( ref idx, PDBIO.CONST_LF_REAL_32 )
                  .WriteSingleLEToBytes( ref idx, (Single) val );
               break;
            case TypeCode.Double:
               array.WriteUInt16LEToBytes( ref idx, PDBIO.CONST_LF_REAL_64 )
                  .WriteDoubleLEToBytes( ref idx, (Double) val );
               break;
            case TypeCode.String:
               var str = (String) val;
               array.WriteUInt16LEToBytes( ref idx, PDBIO.CONST_LF_VARSTRING )
                  .WriteUInt16LEToBytes( ref idx, (UInt16) ( str?.Length ?? 0 ) )
                  .WriteStringToBytes( ref idx, PDBIO.NameEncoding, str );
               break;
            case TypeCode.Decimal:
               var decimalBits = Decimal.GetBits( (Decimal) val );
               array
                  .WriteUInt16LEToBytes( ref idx, PDBIO.CONST_LF_DECIMAL )
                  .WriteInt32LEToBytes( ref idx, decimalBits[3] )
                  .WriteInt32LEToBytes( ref idx, decimalBits[2] )
                  .WriteInt32LEToBytes( ref idx, decimalBits[0] )
                  .WriteInt32LEToBytes( ref idx, decimalBits[1] );
               break;
         }
      }

      // Name
      array.WriteZeroTerminatedString( ref idx, constant.Name )
         .Align4( ref idx ); // Align

      // Revisit length
      array.WriteBlockLength( startIdx, idx );
   }

   private static void WriteOEM( PDBFunction func, Byte[] array, ref Int32 idx, List<Int32> usedNSCount )
   {
      WriteOEMAsyncMethod( func, array, ref idx );
      WriteOEMENCID( func, array, ref idx );
      WriteOEMMD2( func, array, ref idx, usedNSCount );
   }

#pragma warning disable 1591
#if DEBUG

   public
#else
      private
#endif
      static void WriteOEMAsyncMethod( PDBFunction func, Byte[] array, ref Int32 idx )
   {
      var am = func.AsyncMethodInfo;
      if ( am != null )
      {
         var lenIdx = idx;
         WriteOEMHeader( array, ref idx, PDBIO.ASYNC_METHOD_OEM_NAME );
         array
            .WriteUInt32LEToBytes( ref idx, am.KickoffMethodToken )
            .WriteInt32LEToBytes( ref idx, am.CatchHandlerOffset )
            .WriteInt32LEToBytes( ref idx, am.SynchronizationPoints.Count );
         foreach ( var sp in am.SynchronizationPoints )
         {
            array
               .WriteInt32LEToBytes( ref idx, sp.SyncOffset )
               .WriteUInt32LEToBytes( ref idx, sp.ContinuationMethodToken )
               .WriteInt32LEToBytes( ref idx, sp.ContinuationOffset );
         }
         array.Align4( ref idx );

         // Revisit length
         array.WriteBlockLength( lenIdx, idx );
      }
   }

#if DEBUG

   public
#else
      private
#endif
      static void WriteOEMENCID( PDBFunction func, Byte[] array, ref Int32 idx )
   {
      if ( func.ENCID != 0 )
      {
         var lenIdx = idx;
         WriteOEMHeader( array, ref idx, PDBIO.ENC_OEM_NAME );

         array.WriteUInt32LEToBytes( ref idx, func.ENCID );
         array.Align4( ref idx );

         array.WriteBlockLength( lenIdx, idx );

      }
   }

#if DEBUG

   public
#else
      private
#endif
      static void WriteOEMMD2( PDBFunction func, Byte[] array, ref Int32 idx, List<Int32> usedNSCount )
   {
      Byte count = 0;
      var hasNS = ( usedNSCount?.Count ?? 0 ) > 0;
      if ( hasNS )
      {
         ++count;
      }
      if ( func.ForwardingMethodToken != 0u )
      {
         ++count;
      }
      if ( func.ModuleForwardingMethodToken != 0u )
      {
         ++count;
      }
      if ( func.LocalScopes.Count > 0 )
      {
         ++count;
      }
      if ( !String.IsNullOrEmpty( func.IteratorClass ) )
      {
         ++count;
      }
      if ( count > 0 )
      {
         var lenIdx = idx;
         WriteOEMHeader( array, ref idx, PDBIO.MD_OEM_NAME );

         array
            .WriteByteToBytes( ref idx, 4 ) // version
            .WriteByteToBytes( ref idx, count ) // MD item count
            .Align4( ref idx ); // 4-byte boundary
         if ( hasNS )
         {
            // Used namespaces info
            var startIdx = idx;
            Int32 mdLenIdx;
            array
               .WriteOEMItemKind( ref idx, PDBIO.MD2_USED_NAMESPACES, out mdLenIdx )
               .WriteUInt16LEToBytes( ref idx, (UInt16) usedNSCount.Count );
            foreach ( var un in usedNSCount )
            {
               array.WriteUInt16LEToBytes( ref idx, (UInt16) un );
            }
            // Revisit length
            array
               .Align4( ref idx )
               .WriteInt32LEToBytes( ref mdLenIdx, idx - startIdx );
         }
         if ( func.ForwardingMethodToken != 0u )
         {
            // Forwarding method
            var startIdx = idx;
            Int32 mdLenIdx;
            array
               .WriteOEMItemKind( ref idx, PDBIO.MD2_FORWARDING_METHOD_TOKEN, out mdLenIdx )
               .WriteUInt32LEToBytes( ref idx, func.ForwardingMethodToken )
               .Align4( ref idx )
               .WriteInt32LEToBytes( ref mdLenIdx, idx - startIdx );// Revisit length
         }
         if ( func.ModuleForwardingMethodToken != 0u )
         {
            // Forwarding module method
            var startIdx = idx;
            Int32 mdLenIdx;
            array
               .WriteOEMItemKind( ref idx, PDBIO.MD2_FORWARDING_MODULE_METHOD_TOKEN, out mdLenIdx )
               .WriteUInt32LEToBytes( ref idx, func.ModuleForwardingMethodToken )
               .Align4( ref idx )
               .WriteInt32LEToBytes( ref mdLenIdx, idx - startIdx ); // Revisit length
         }
         if ( func.LocalScopes.Count > 0 )
         {
            var startIdx = idx;
            Int32 mdLenIdx;
            array
               .WriteOEMItemKind( ref idx, PDBIO.MD2_LOCAL_SCOPES, out mdLenIdx )
               .WriteInt32LEToBytes( ref idx, func.LocalScopes.Count );
            foreach ( var ls in func.LocalScopes )
            {
               array
                  .WriteInt32LEToBytes( ref idx, ls.Offset ) // IL start offset
                  .WriteInt32LEToBytes( ref idx, ls.Offset + ls.Length ); // IL end offset
            }
            // Revisit length
            array
               .Align4( ref idx )
               .WriteInt32LEToBytes( ref mdLenIdx, idx - startIdx );
         }
         if ( !String.IsNullOrEmpty( func.IteratorClass ) )
         {
            // Iterator info
            var startIdx = idx;
            Int32 mdLenIdx;
            array
               .WriteOEMItemKind( ref idx, PDBIO.MD2_ITERATOR_CLASS, out mdLenIdx )
               .WriteZeroTerminatedString( ref idx, func.IteratorClass, false )
               .Align4( ref idx )
               .WriteInt32LEToBytes( ref mdLenIdx, idx - startIdx );
         }

         // The size of whole OEM block
         array.WriteBlockLength( lenIdx, idx );
      }
   }

   private static Byte[] WriteOEMItemKind( this Byte[] array, ref Int32 idx, Byte mdKind, out Int32 lenIdx )
   {
      array
         .WriteByteToBytes( ref idx, 4 ) // version
         .WriteByteToBytes( ref idx, mdKind ) // md item kind
         .Align4( ref idx ); // 4-byte boundary
      lenIdx = idx; // Save length index
      idx += 4; // Skip the length
      return array;
   }

   private static void WriteOEMHeader( Byte[] array, ref Int32 idx, String oemName )
   {
      array
         .Skip( ref idx, 2 ) // Block byte count
         .WriteUInt16LEToBytes( ref idx, PDBIO.SYM_OEM ) // OEM sym
         .WriteGUIDToBytes( ref idx, GUIDs.MSIL_METADATA_GUID ) // MD2 GUID
         .WriteInt32LEToBytes( ref idx, 0 ) // Type index
         .WriteZeroTerminatedString( ref idx, oemName, false ); // OEM name
   }

   private static void WriteENDSym( Byte[] array, ref Int32 idx )
   {
      var lenIdx = idx;
      array
         .Skip( ref idx, 2 ) // Length
         .WriteUInt16LEToBytes( ref idx, PDBIO.SYM_END ) // END
         .Align4( ref idx );
      // Revisit length
      array.WriteUInt16LEToBytes( ref lenIdx, (UInt16) ( idx - lenIdx - 2 ) );
   }

   private static void AddENDSym( ref Int32 idx )
   {
      Align4( ref idx );
      idx += 4; // Length + END
      Align4( ref idx );
   }

   private static Int32 WriteSymbolRecordStream( this PDBWritingState state )
   {
      System.Diagnostics.Debug.Assert( state.SymRecIndex == state.SymRecStream.Length, "The symbol record stream should've been written completely." );
      var streamIndex = state.DataStreams.Count;
      state.AddNewIndexedStream(
         streamIndex,
         state.SymRecStream.Length,
         ( array, idx ) => array.BlockCopyFrom( ref idx, state.SymRecStream )
         );
      return streamIndex;
   }

   private static Int32 WriteGlobalSymbolStream(
      this PDBWritingState state
      )
   {
      return state.WriteGSIStream(
         state.Globals,
         null,
         null
         );
   }

   private static Int32 WritePublicSymbolStream( this PDBWritingState state )
   {
      var suffixSize = state.Publics.ReferenceCount > 0 ? 4 : 0;
      return state.WriteGSIStream(
         state.Publics,
         gsiStreamSize => new TWriteAction( 28, ( array, idx ) =>
         {
            array
            .WriteInt32LEToBytes( ref idx, gsiStreamSize )
            .WriteInt32LEToBytes( ref idx, suffixSize )
            .ZeroOut( ref idx, 20 ); // Rest is just padding, maybe?
         } ),
         gsiStreamSize => new TWriteAction( suffixSize, ( array, idx ) =>
         {
            // Magic zero as suffix
            array.WriteInt32LEToBytes( ref idx, 0 );
         } )
         );
   }

   private static Int32 WriteGSIStream(
      this PDBWritingState state,
      SymbolRecStreamRefInfo gsiStream,
      Func<Int32, TWriteAction> prefixWriterCreator,
      Func<Int32, TWriteAction> suffixWriterCreator
      )
   {
      var streamIndex = state.DataStreams.Count;
      var namedRefs = gsiStream.NamedReferences;
      var gsRefSize = 8 * gsiStream.ReferenceCount; // Each ref is two ints

      var gsSymHashSize = gsRefSize > 0 ? ( GS_SYM_BUCKETS / 8 + ( namedRefs.Count + 1 ) * 4 ) : 0;
      var gsiStreamSize = 16 // Header size
         + gsRefSize // References size
         + gsSymHashSize; // Hash table size
      state.AddNewIndexedStreamInPieces(
         streamIndex,
         new[]
         {
            prefixWriterCreator?.Invoke(gsiStreamSize),
            new TWriteAction( gsiStreamSize, ( array, idx ) =>
            {
               array
               .WriteUInt32LEToBytes( ref idx, UInt32.MaxValue ) // Signature (gsi.h, GSIHashHdr struct)
               .WriteUInt32LEToBytes( ref idx, 0xF12F091A ) // Version ( gsi.h, GSIHashSCImpvV70 )
               .WriteInt32LEToBytes( ref idx, gsRefSize ) // Byte count of references to symRecStream
               .WriteInt32LEToBytes( ref idx, gsSymHashSize ); // Present buckets bitset + offsets

               // References to symRecStream
               var gsHashSorted = namedRefs.Keys.ToArray();
               Array.Sort( gsHashSorted, Comparer<UInt32>.Default );
               foreach ( var gsHash in gsHashSorted )
               {
                  var list = namedRefs[gsHash];

                  // The list items are in ascending order since whenever a new value is added, it is always greater than prev values.
                  // Iterate in descending order (not sure if really required, but it is how the values are written by MS writer)
                  for ( var i = list.Count; i > 0; --i )
                  {
                     array
                        .WriteInt32LEToBytes( ref idx, list[i - 1] ) // Reference
                        .WriteInt32LEToBytes( ref idx, 1 ); // Segment maybe?
                  }
               }

               if ( gsSymHashSize > 0 )
               {
                  // Hash table present buckets bitset
                  var curGSHashIdx = 0;
                  var tmpUInt32 = 0u;
                  for ( var i = 0; i < GS_SYM_BUCKETS / 32; ++i )
                  {
                     tmpUInt32 = 0u;
                     while ( curGSHashIdx < gsHashSorted.Length && gsHashSorted[curGSHashIdx] < ( i + 1 ) * 32 )
                     {
                        tmpUInt32 |= ( 1u << (Int32) ( gsHashSorted[curGSHashIdx] % 32 ) );
                        ++curGSHashIdx;
                     }
                     array.WriteUInt32LEToBytes( ref idx, tmpUInt32 );
                  }
                  
                  // Magic zero
                  array.WriteInt32LEToBytes( ref idx, 0 );

                  // Write counts in each bucket
                  tmpUInt32 = 0u;
                  foreach ( var gsHash in gsHashSorted )
                  {
                     array.WriteUInt32LEToBytes( ref idx, tmpUInt32 );
                     tmpUInt32 += 12u * (UInt32) namedRefs[gsHash].Count;
                  }
               }
            }),
            suffixWriterCreator?.Invoke(gsiStreamSize)
         } );

      return streamIndex;
   }

   private static Int32 SafeByteCount( this Encoding encoding, String str, Boolean zeroTerminated ) // = true )
   {
      var result = zeroTerminated ? 1 : sizeof( Int32 );
      if ( str != null )
      {
         result += encoding.GetByteCount( str );
      }
      return result;
   }

   private static Int32 AddNewNamedStream( this PDBWritingState state, String streamName, Int32 streamSize, Action<Byte[], Int32> write )
   {
      var retVal = state.DataStreams.Count;
      state.AddNewIndexedStream( retVal, streamSize, write, streamName );
      return retVal;
   }

   private static void AddNewIndexedStream( this PDBWritingState state, Int32 streamIndex, Int32 streamSize, Action<Byte[], Int32> write, String streamName = null )
   {
      state.AddNewIndexedStreamInPieces( streamIndex, Tuple.Create( streamSize, write ).Singleton(), streamName );
   }

   private static void AddNewIndexedStreamInPieces( this PDBWritingState state, Int32 streamIndex, IEnumerable<TWriteAction> writes, String streamName = null )
   {
      var streamInfo = state.WritePagedData( writes );

      var dataStreams = state.DataStreams;
      var count = dataStreams.Count;
      if ( streamIndex < count )
      {
         dataStreams[streamIndex] = streamInfo;
      }
      else
      {
         if ( streamIndex > count )
         {
            dataStreams.AddRange( Enumerable.Repeat<TStreamInfo>( null, streamIndex - count ) );
         }
         dataStreams.Add( streamInfo );
      }

      if ( streamName != null )
      {
         state.DataStreamNames[streamName] = streamIndex;
      }
   }

   private static TStreamInfo WritePagedData( this PDBWritingState state, TWriteAction writes )
   {
      return state.WritePagedData( writes.Singleton() );
   }

   private static TStreamInfo WritePagedData( this PDBWritingState state, IEnumerable<TWriteAction> writes )
   {

      var streamSize = 0;
      var startPage = state.GetCurrentPage();
      foreach ( var write in writes )
      {
         if ( write != null )
         {
            var curSize = write.Item1;
            if ( curSize > 0 )
            {
               var array = state.Stream.Buffer.SetCapacityAndReturnArray( curSize );

               write.Item2( array, 0 );
               state.Stream.Stream.Write( array, curSize );

               streamSize += curSize;
            }
         }
      }
      return Tuple.Create( streamSize, state.PagesFromContinuousStream( startPage ) );
   }



   private static Int32 GetCurrentPage( this PDBWritingState state )
   {
      return ( (Int32) state.Stream.Stream.Position ) / state.PageSize;
   }

   private static void SkipToNextPage( this PDBWritingState state )
   {
      var stream = state.Stream.Stream;
      var remainder = (Int32) ( stream.Position % state.PageSize );
      if ( remainder != 0 )
      {
         stream.Write( state.ZeroesArray, state.PageSize - remainder );
      }
   }

   private static Int32[] PagesFromContinuousStream( this PDBWritingState state, Int32 startPage )
   {
      state.SkipToNextPage();
      return Enumerable.Range( startPage, state.GetCurrentPage() - startPage ).ToArray();
   }

   private const Int32 SLOT_FIXED_SIZE = 16;
   private const Int32 SCOPE_FIXED_SIZE = 18;

   private const Int32 FIXED_OEM_SIZE = GUID_SIZE + 8; // Block len + sym + GUID + type index
   private const Int32 INT_SIZE = sizeof( Int32 );
   private const Int32 SHORT_SIZE = sizeof( Int16 );

   private static Int32 CalculateSymbolByteCount( this PDBFunction func, out List<Int32> usedNSCount )
   {
      // Function byte length begins after the name
      usedNSCount = new List<Int32>();
      var result = func.CalculateByteCountFromLists( usedNSCount )
         + CalculateByteCountAsyncMethodInfo( func.AsyncMethodInfo ) // Async OEM
         + CalculateByteCountMD2Info( func, ref usedNSCount ) // MD2 OEM
         + CalculateByteCountENCInfo( func ); // ENC OEM
      AddENDSym( ref result ); // END
      return result;
   }
#if DEBUG
   public
#else
      private
#endif
      static Int32 GetFixedOEMSize( String oemName )
   {
      return FIXED_OEM_SIZE // Required OEM info
         + PDBIO.UTF16.GetByteCount( oemName ) + 2; // OEM name + zero double-byte
   }

#if DEBUG

   public
#else
      private
#endif
      static Int32 CalculateByteCountAsyncMethodInfo( PDBAsyncMethodInfo asyncInfo )
   {
      // Async info OEM is needed
      var result = asyncInfo == null ? 0 : ( GetFixedOEMSize( PDBIO.ASYNC_METHOD_OEM_NAME )
         + INT_SIZE * 3 // Kickoff method token, catch handler offset, sync point count
         + asyncInfo.SynchronizationPoints.Count * INT_SIZE * 3 ); // For each sync point, sync offset, continuation method token, continuation offset
      Align4( ref result );
      return result;
   }

#if DEBUG

   public
#else
      private
#endif
      static Int32 CalculateByteCountENCInfo( PDBFunction func )
   {
      var size = 0;
      if ( func.ENCID != 0u )
      {
         size += GetFixedOEMSize( PDBIO.ENC_OEM_NAME ); // OEM name
         size += sizeof( UInt32 );
         Align4( ref size );
      }

      return size;
   }

#if DEBUG

   public
#else
      private
#endif
      static Int32 CalculateByteCountMD2Info( PDBFunction func, ref List<Int32> usedNSCount )
   {
      var size = 0;
      var hasUsedNS = usedNSCount != null && usedNSCount.Any( c => c > 0 );
      if ( !hasUsedNS )
      {
         usedNSCount = null;
      }
      if ( !String.IsNullOrEmpty( func.IteratorClass )
           || func.ForwardingMethodToken != 0u
           || func.ModuleForwardingMethodToken != 0u
           || func.LocalScopes.Count > 0
           || hasUsedNS
         )
      {
         // MD2 OEM is needed
         size += GetFixedOEMSize( PDBIO.MD_OEM_NAME ) // OEM name
            + 2; // Version byte, OEM MD2 infos count
         Align4( ref size );
         if ( hasUsedNS )
         {
            size += 2; // version + MD2 kind
            Align4( ref size ); // 4-byte boundary
            size += INT_SIZE // OEM entry byte size
               + SHORT_SIZE // Used namespace count
               + SHORT_SIZE * usedNSCount.Count; // Used namespace idx
            Align4( ref size );
         }
         if ( func.ForwardingMethodToken != 0u )
         {
            size += 2; // version + MD2 kind
            Align4( ref size ); // 4-byte boundary
            size += INT_SIZE // OEM entry byte size
               + INT_SIZE; // Token
         }
         if ( func.ModuleForwardingMethodToken != 0u )
         {
            size += 2; // version + MD2 kind
            Align4( ref size ); // 4-byte boundary
            size += INT_SIZE // OEM entry byte size
               + INT_SIZE; // Token
         }
         if ( func.LocalScopes.Count > 0 )
         {
            size += 2; // version + MD2 kind
            Align4( ref size ); // 4-byte boundary
            size += INT_SIZE // OEM entry byte size
               + INT_SIZE // Local scope count
               + INT_SIZE * 2 * func.LocalScopes.Count; // Local scope offset and length
            Align4( ref size );
         }
         if ( !String.IsNullOrEmpty( func.IteratorClass ) )
         {
            size += 2; // version + MD2 kind
            Align4( ref size ); // 4-byte boundary
            size += INT_SIZE // OEM entry byte size
               + PDBIO.UTF16.GetByteCount( func.IteratorClass )// Iterator class name
               + 2; // Zero padding
            Align4( ref size );
         }

      }
      return size;
   }

   private static Int32 CalculateByteCount( this PDBScope scope, List<Int32> usedNSCount )
   {
      var result = SCOPE_FIXED_SIZE + 4; // BlockLen + SYM
      result += PDBIO.NameEncoding.SafeByteCount( scope.Name, true );
      Align4( ref result );
      result += scope.CalculateByteCountFromLists( usedNSCount );
      //Align4( ref result );
      AddENDSym( ref result );
      return result;
   }



   private static Int32 CalculateByteCount( this PDBConstant constant )
   {
      var retVal = sizeof( UInt16 ) // Block length
         + sizeof( UInt16 ) // SYM_MANAGED_CONSTANT
         + sizeof( Int32 ) // Token size
         + sizeof( UInt16 ); // Size of LEAF_ENUM_e
      var val = constant.Value;
      if ( val == null )
      {
         // Null (string) = LF_LONG + zero
         retVal += 4;
      }
      else
      {
         switch ( Type.GetTypeCode( val.GetType() ) )
         {
            case TypeCode.Boolean:
               // Booleans require no more information than the prefix
               break;
            case TypeCode.Char:
               // Chars are serialized as LF_SHORTs
               retVal += sizeof( UInt16 );
               break;
            case TypeCode.SByte:
               // Positive SBytes are serialized right away, negative SBytes are serialized as LF_CHAR + value
               if ( (SByte) val < 0 )
               {
                  retVal += sizeof( SByte );
               }
               break;
            case TypeCode.Byte:
               // Bytes are serialized right away, since they are never greater than 0x8001
               break;
            case TypeCode.Int16:
               // Positive Int16s which are less than 0x8000 are serialized right away, negative Int16s are serialized as LF_SHORT + value
               var i16 = (Int16) val;
               //if ( i16 < 0 || i16 > PDBIO.MAX_PDB_CONSTANT_NUMERIC )
               //{
               retVal += sizeof( Int16 );
               //}
               break;
            case TypeCode.UInt16:
               //if ( (UInt16) val > PDBIO.MAX_PDB_CONSTANT_NUMERIC )
               //{
               retVal += sizeof( UInt16 );
               //}
               break;
            case TypeCode.Int32:
               var i32 = (Int32) val;
               //if ( i32 < 0 || i32 > PDBIO.MAX_PDB_CONSTANT_NUMERIC )
               //{
               retVal += sizeof( Int32 );
               //}
               break;
            case TypeCode.UInt32:
               //if ( (UInt32) val > PDBIO.MAX_PDB_CONSTANT_NUMERIC )
               //{
               retVal += sizeof( UInt32 );
               //}
               break;
            case TypeCode.Int64:
               var i64 = (Int64) val;
               //if ( i64 < 0 || i64 > PDBIO.MAX_PDB_CONSTANT_NUMERIC )
               //{
               retVal += sizeof( Int64 );
               //}
               break;
            case TypeCode.UInt64:
               //if ( (UInt64) val > PDBIO.MAX_PDB_CONSTANT_NUMERIC )
               //{
               retVal += sizeof( UInt64 );
               //}
               break;
            case TypeCode.Single:
               retVal += sizeof( Single );
               break;
            case TypeCode.Double:
               retVal += sizeof( Double );
               break;
            case TypeCode.String:
               retVal += sizeof( UInt16 ) + PDBIO.NameEncoding.SafeByteCount( (String) val, false );
               break;
            case TypeCode.Decimal:
               retVal += sizeof( Int32 ) * 4;
               break;

         }
      }

      retVal += PDBIO.NameEncoding.SafeByteCount( constant.Name, true );
      Align4( ref retVal );

      return retVal;
   }

   private static Int32 CalculateByteCountFromLists( this PDBScopeOrFunction scope )
   {
      return scope.CalculateByteCountFromLists( null );
   }

#if DEBUG

   public
#else
      private
#endif
      static Int32 CalculateByteCountFromLists( this PDBScopeOrFunction scope, List<Int32> usedNSCount )
   {
      if ( usedNSCount != null )
      {
         usedNSCount.Add( scope.UsedNamespaces.Count );
      }
      var retVal = scope.Slots.Aggregate( 0, ( cur, slot ) =>
         {
            var result = cur + 4 + SLOT_FIXED_SIZE + PDBIO.NameEncoding.SafeByteCount( slot.Name, true ); // BlockLen + SYM
            Align4( ref result );
            return result;
         } )
         + scope.UsedNamespaces.Aggregate( 0, ( cur, un ) =>
         {
            var result = cur + 4 + PDBIO.NameEncoding.SafeByteCount( un, true ); // BlockLen + SYM
            Align4( ref result );
            return result;
         } )
         + scope.Scopes.Aggregate( 0, ( cur, scp ) => cur + scp.CalculateByteCount( usedNSCount ) )
         + scope.Constants.Aggregate( 0, ( cur, constant ) => cur + constant.CalculateByteCount() );
      return retVal;
   }

   private static void Align4( ref Int32 number )
   {
      number = number.RoundUpI32( 4 );
   }

   private static void WriteBlockLength( this Byte[] array, Int32 lenIdx, Int32 curIdx )
   {
      array.WriteUInt16LEToBytes( ref lenIdx, (UInt16) ( curIdx - lenIdx - 2 ) );
   }

   private static void SeekToPage( this PDBWritingState state, Int32 page, Int32 pageOffset = 0 )
   {
      state.Stream.Stream.SeekToPage( state.StreamStart, state.PageSize, page, pageOffset );
   }
}


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
