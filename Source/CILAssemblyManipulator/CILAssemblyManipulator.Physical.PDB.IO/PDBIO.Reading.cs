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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommonUtils;
using CILAssemblyManipulator.Physical.IO; // Only for compressed integers.

namespace CILAssemblyManipulator.Physical.PDB
{
   /// <summary>
   /// This class holds the method to read <see cref="PDBInstance"/> from <see cref="Stream"/>.
   /// </summary>
   public static partial class PDBIO
   {

      private const UInt32 METHOD_TABLE = 0x06000000;
      private const UInt32 METHOD_TABLE_INDEX_MASK = 0x00FFFFFF;

      internal static Encoding NameEncoding { get; }
      internal static Encoding UTF16 { get; }

      static PDBIO()
      {
         NameEncoding = new UTF8Encoding( false, true );
         UTF16 = new UnicodeEncoding( false, false, true );
      }

      private const String SOURCE_SERVER_STREAM_NAME = "srcsrv";

      /// <summary>
      /// Reads the <see cref="PDBInstance"/> from the given <see cref="Stream"/>.
      /// </summary>
      /// <param name="stream">The <see cref="Stream"/> to read <see cref="PDBInstance"/> from.</param>
      /// <param name="ignoreCaseSourceFileStreamNames">Whether ignore case in source file stream names.</param>
      /// <returns>A new instance of <see cref="PDBInstance"/> containing deserialized information relevant to CLR meta data.</returns>
      public static PDBInstance ReadPDBInstance( this Stream stream, Boolean ignoreCaseSourceFileStreamNames = true )
      {
         var streamHelper = new StreamInfo( stream );
         // Header
         // Skip first 32 bytes (it's just text and magic ints)
         streamHelper.stream.SeekFromCurrent( 32 );
         var array = new Byte[20];
         streamHelper.stream.ReadWholeArray( array );
         var idx = 0;
         var pageSize = array.ReadInt32LEFromBytes( ref idx );
         if ( pageSize <= 0 )
         {
            // Maybe instead just return empty PDBInstance ?
            throw new PDBException( "Page size must be greater than zero, was: " + pageSize + "." );
         }
         array.Skip( ref idx, 8 ); // Skip free page map size and page count

         var directoryByteCount = array.ReadInt32LEFromBytes( ref idx );
         // Ignore last 4 bytes

         // Read directory page offsets
         var directoryPageCount = AmountOfPagesTaken( directoryByteCount, pageSize );
         var directoryPageOffsetPageCount = AmountOfPagesTaken(
            INT_SIZE * directoryPageCount,
            pageSize );
         array = streamHelper.stream.ReadWholeArray( INT_SIZE * directoryPageOffsetPageCount );
         idx = 0;
         var directoryPageOffsets = array.ReadInt32ArrayLEFromBytes( ref idx, directoryPageOffsetPageCount );

         // Directory
         // Read directory pages
         array = streamHelper.ReadPagedData( pageSize, directoryPageOffsets, directoryPageCount * INT_SIZE );
         idx = 0;
         directoryPageOffsets = array.ReadInt32ArrayLEFromBytes( ref idx, directoryPageCount );

         // Read directory data
         array = streamHelper.ReadPagedData( pageSize, directoryPageOffsets, directoryByteCount );
         idx = 0;
         var dataStreamCount = array.ReadInt32LEFromBytes( ref idx );
         var dataStreamSizes = array.ReadInt32ArrayLEFromBytes( ref idx, dataStreamCount );
         var dataStreamPages = new Int32[dataStreamCount][];
         for ( var i = 0; i < dataStreamCount; ++i )
         {
            var dSize = dataStreamSizes[i];
            if ( dSize > 0 )
            {
               dataStreamPages[i] = array.ReadInt32ArrayLEFromBytes( ref idx, AmountOfPagesTaken( dSize, pageSize ) );
            }
         }

         // Read DBI data
         array = streamHelper.ReadPagedData( pageSize, dataStreamPages[STREAM_INDEX_DBI], dataStreamSizes[STREAM_INDEX_DBI] );
         idx = 0;
         DBIHeader dbiHeader; List<DBIModuleInfo> modules; DBIDebugHeader debugHeader;
         LoadDBIStream( array, ref idx, out dbiHeader, out modules, out debugHeader );

         // Make array have the size of biggest stream size to be used, so we wouldn't need to create new array each time for stream
         // TODO this might still be too much in some gigantic function cases...
         array = new Byte[dataStreamSizes.Where( ( siz, i ) => i != 3 && i != dbiHeader.symRecStream && i != dbiHeader.psSymStream && i != dbiHeader.gsSymStream ).Max()];

         // Create result instance
         var instance = new PDBInstance();

         // Read root stream
         streamHelper.ReadPagedData( pageSize, dataStreamPages[STREAM_INDEX_ROOT], dataStreamSizes[STREAM_INDEX_ROOT], array );
         idx = 4; // Skip version
         instance.Timestamp = array.ReadUInt32LEFromBytes( ref idx );
         instance.Age = array.ReadUInt32LEFromBytes( ref idx );
         instance.DebugGUID = array.ReadGUIDFromBytes( ref idx );
         var streamNameIndices = LoadStreamIndices( array, ref idx, ignoreCaseSourceFileStreamNames );

         Int32 namesStreamIdx;
         if ( !streamNameIndices.TryGetValue( NAMES_STREAM_NAME, out namesStreamIdx ) )
         {
            throw new PDBException( "The \"" + NAMES_STREAM_NAME + "\" stream is missing." );
         }
         else if ( namesStreamIdx >= dataStreamCount )
         {
            throw new PDBException( "The \"" + NAMES_STREAM_NAME + "\" stream was referencing to non-existant stream." );
         }

         // Read symbol server data, if any
         Int32 srcStrmIdx;
         if ( streamNameIndices.TryGetValue( SOURCE_SERVER_STREAM_NAME, out srcStrmIdx ) && srcStrmIdx < dataStreamCount )
         {
            streamHelper.ReadPagedData( pageSize, dataStreamPages[srcStrmIdx], dataStreamSizes[srcStrmIdx], array );
            idx = 0;
            instance.SourceServer = array.ReadStringWithEncoding( ref idx, dataStreamSizes[srcStrmIdx], NameEncoding );
         }

         // Read name index.
         streamHelper.ReadPagedData( pageSize, dataStreamPages[namesStreamIdx], dataStreamSizes[namesStreamIdx], array );
         idx = 0;
         /*var nsSig = */
         array.ReadUInt32LEFromBytes( ref idx );
         /*var nsVer = */
         array.ReadInt32LEFromBytes( ref idx );
         var nameIndex = LoadNameIndex( array, ref idx );

         // TEMP BEGIN
         //var kekke = dbiHeader.psSymStream;
         //array = new Byte[1024];
         //streamHelper.ReadPagedData( pageSize, dataStreamPages[kekke], dataStreamSizes[kekke], array );

         // TEMP END
         // Read modules.
         var allSources = new Dictionary<String, PDBSource>();
         foreach ( var module in modules )
         {
            if ( module.stream > 0 && module.stream < dataStreamCount )
            {
               streamHelper.ReadPagedData( pageSize, dataStreamPages[module.stream], dataStreamSizes[module.stream], array );
               instance.Modules.Add( LoadFunctionsFromDBIModule( streamHelper, pageSize, dataStreamPages, dataStreamSizes, array, streamNameIndices, nameIndex, module, allSources ) );
            }
         }

         // Apply token remapping table if exists
         if ( debugHeader != null && debugHeader.snTokenRidMap != UInt16.MinValue && debugHeader.snTokenRidMap != UInt16.MaxValue )
         {
            var tokenRemapSize = dataStreamSizes[debugHeader.snTokenRidMap];
            streamHelper.ReadPagedData( pageSize, dataStreamPages[debugHeader.snTokenRidMap], tokenRemapSize, array );
            idx = 0;
            var tokens = array.ReadUInt32ArrayLEFromBytes( ref idx, tokenRemapSize / INT_SIZE );
            foreach ( var function in instance.Modules.SelectMany( mod => mod.Functions ) )
            {
               function.Token = METHOD_TABLE | tokens[function.Token & METHOD_TABLE_INDEX_MASK];
            }
         }

         return instance;
      }

      private static IDictionary<String, Int32> LoadStreamIndices( Byte[] array, ref Int32 idx, Boolean ignoreCaseSourceFileStreamNames )
      {
         // How many bytes of strings
         var strSize = array.ReadInt32LEFromBytes( ref idx );
         // Save offset where the strings start
         var strStart = idx;
         // Move to data after strings
         idx += strSize;
         // Read amount of names and create result dictionary
         var nameCount = array.ReadInt32LEFromBytes( ref idx );
         var retVal = new Dictionary<String, Int32>( nameCount, ignoreCaseSourceFileStreamNames ? StringComparer.OrdinalIgnoreCase : null );

         // Read max bit set size
         var max = array.ReadInt32LEFromBytes( ref idx );
         // Read present and deleted bit sets
         var present = array.ReadUInt32ArrayLEFromBytes( ref idx, array.ReadInt32LEFromBytes( ref idx ) );
         var deleted = array.ReadUInt32ArrayLEFromBytes( ref idx, array.ReadInt32LEFromBytes( ref idx ) );

         // Read stream names and corresponding indices.
         for ( var i = 0; i < max; ++i )
         {
            if ( present.IsBitSet( i ) && !deleted.IsBitSet( i ) )
            {
               var strIdx = array.ReadInt32LEFromBytes( ref idx );
               var idxValue = array.ReadInt32LEFromBytes( ref idx );
               var oldIdx = idx;
               idx = strStart + strIdx;
               retVal.Add( array.ReadZeroTerminatedStringFromBytes( ref idx, NameEncoding ), idxValue );
               idx = oldIdx;
            }
         }

         if ( retVal.Count != nameCount )
         {
            throw new PDBException( "Name index claimed to have " + nameCount + " entries but had only " + retVal.Count + " entries." );
         }

         return retVal;
      }

      private static IDictionary<Int32, String> LoadNameIndex( Byte[] array, ref Int32 idx )
      {
         // How many bytes of strings
         var strSize = array.ReadInt32LEFromBytes( ref idx );
         // Save offset where strings start
         var strStart = idx;
         // Move to data after strings
         idx += strSize;
         // Read amount of strings
         var nameMaxCount = array.ReadInt32LEFromBytes( ref idx );
         var retVal = new Dictionary<Int32, String>( nameMaxCount );
         for ( var i = 0; i < nameMaxCount; ++i )
         {
            var nameIdx = array.ReadInt32LEFromBytes( ref idx );
            if ( nameIdx != 0 )
            {
               // Read name
               var oldIdx = idx;
               idx = strStart + nameIdx;
               retVal.Add( nameIdx, array.ReadZeroTerminatedStringFromBytes( ref idx, NameEncoding ) );
               idx = oldIdx;
            }
         }

         return retVal;
      }

      private static void LoadDBIStream( Byte[] array, ref Int32 idx, out DBIHeader dbiHeader, out List<DBIModuleInfo> modules, out DBIDebugHeader debugHeader )
      {
         dbiHeader = new DBIHeader( array, ref idx );

         // Read module info
         var max = idx + dbiHeader.moduleInfoSize;
         modules = new List<DBIModuleInfo>();
         while ( idx < max )
         {
            modules.Add( new DBIModuleInfo( array, ref idx, NameEncoding ) );
            Align4( ref idx );
         }

         // Skip following parts: Section Contribution, Section Map, File Info, TSM, EC
         idx += dbiHeader.secConSize + dbiHeader.secMapSize + dbiHeader.fileInfoSize + dbiHeader.tsMapSize + dbiHeader.ecInfoSize;

         // Read debug header if present
         debugHeader = dbiHeader.debugHeaderSize > 0 ? new DBIDebugHeader( array, ref idx ) : null;
      }

      private static PDBModule LoadFunctionsFromDBIModule(
         StreamInfo stream,
         Int32 pageSize,
         Int32[][] streamPages,
         Int32[] streamSizes,
         Byte[] array,
         IDictionary<String, Int32> streamNameIndices,
         IDictionary<Int32, String> nameIndex,
         DBIModuleInfo moduleInfo,
         IDictionary<String, PDBSource> allSources
         )
      {
         var idx = INT_SIZE; // Skip signature
         var thisFuncs = new List<PDBFunctionInfo>();
         var module = new PDBModule()
         {
            Name = moduleInfo.moduleName,
            ObjectName = moduleInfo.objectName
         };
         while ( idx < moduleInfo.symbolByteCount )
         {
            var blockLen = array.ReadUInt16LEFromBytes( ref idx );
            var blockEndIdx = idx + blockLen;
#if DEBUG
            if ( blockEndIdx % 4 != 0 )
            {
               throw new PDBException( "Debyyg" );
            }
#endif
            switch ( array.ReadUInt16LEFromBytes( ref idx ) )
            {
               case SYM_GLOBAL_MANAGED_FUNC:
               case SYM_LOCAL_MANAGED_FUNC:
                  UInt32 addr; UInt16 seg;
                  var func = NewPDBFunction( moduleInfo.moduleName, array, ref idx, blockEndIdx, out addr, out seg );
                  module.Functions.Add( func );
                  thisFuncs.Add( new PDBFunctionInfo( func, addr, seg, 0u ) ); // Function pointer is not used during reading
                  break;
               default:
#if DEBUG
                  throw new PDBException( "Debyyg" );
#else
                  idx = blockEndIdx;
                  break;
#endif
            }
         }

         if ( thisFuncs.Count > 0 )
         {
            // Seek to line information
            idx = moduleInfo.symbolByteCount + moduleInfo.oldLinesByteCount;
            // Sort the functions based on their address and token in order for fast lookup
            thisFuncs.Sort( PDB_FUNC_ADDRESS_AND_TOKEN_BASED );
            // Load PDBSources and PDBLines and modify PDBFunction's lineInfo to contain the information about sources and lines.
            LoadSourcesAndLinesFromModuleStream( stream, pageSize, streamPages, streamSizes, array, idx, idx + moduleInfo.linesByteCount, streamNameIndices, nameIndex, thisFuncs, allSources );
         }
         return module;
      }

      private static void LoadSourcesAndLinesFromModuleStream(
         StreamInfo stream,
         Int32 pageSize,
         Int32[][] streamPages,
         Int32[] streamSizes,
         Byte[] array,
         Int32 idx,
         Int32 max,
         IDictionary<String, Int32> streamNameIndices,
         IDictionary<Int32, String> nameIndex,
         List<PDBFunctionInfo> functions,
         IDictionary<String, PDBSource> allSources
         )
      {
         var sourcesLocal = new Dictionary<Int32, PDBSource>();
         var lines = new List<Tuple<Int32, PDBLine>>[functions.Count];

         while ( idx < max )
         {
            var sym = array.ReadInt32LEFromBytes( ref idx );
            var size = array.ReadInt32LEFromBytes( ref idx );
            var startIdx = idx;
            var endIdx = idx + size;
            switch ( sym )
            {
               case SYM_DEBUG_SOURCE_INFO:
                  while ( idx < endIdx )
                  {
                     var curSrcFileIdx = idx - startIdx;
                     var nameIdx = array.ReadInt32LEFromBytes( ref idx );
                     var thisLen = array.ReadByteFromBytes( ref idx );
                     /*var kind = */
                     array.ReadByteFromBytes( ref idx );

                     var name = nameIndex[nameIdx];
                     var pdbSource = allSources.GetOrAdd_NotThreadSafe( name, theName =>
                     {
                        var src = new PDBSource()
                        {
                           Name = name
                        };

                        Int32 sourceStreamIdx;
                        if ( streamNameIndices.TryGetValue( SOURCE_FILE_PREFIX + name, out sourceStreamIdx ) )
                        {
                           var sourceBytes = stream.ReadPagedData( pageSize, streamPages[sourceStreamIdx], streamSizes[sourceStreamIdx] );
                           var tmpIdx = 0;
                           src.Language = sourceBytes.ReadGUIDFromBytes( ref tmpIdx );
                           src.Vendor = sourceBytes.ReadGUIDFromBytes( ref tmpIdx );
                           src.DocumentType = sourceBytes.ReadGUIDFromBytes( ref tmpIdx );
                           src.HashAlgorithm = sourceBytes.ReadGUIDFromBytes( ref tmpIdx );
                           var hashSize = sourceBytes.ReadInt32LEFromBytes( ref tmpIdx );
                           // Some int32 here...
                           tmpIdx += 4;
                           src.Hash = sourceBytes.CreateAndBlockCopyTo( ref tmpIdx, hashSize );
                        }
                        return src;
                     } );

                     sourcesLocal.Add( curSrcFileIdx, pdbSource );
#if DEBUG
                     if ( thisLen != 0 )
                     {
                        throw new PDBException( "Debyyg" );
                     }
#endif
                     idx += thisLen;
                     Align4( ref idx );
                  }
                  break;
               case SYM_DEBUG_LINE_INFO:

                  var addr = array.ReadUInt32LEFromBytes( ref idx );
                  var section = array.ReadUInt16LEFromBytes( ref idx );
                  var flags = array.ReadUInt16LEFromBytes( ref idx );
                  /*var cod = */
                  array.ReadUInt32LEFromBytes( ref idx );
                  var funcIdx = functions.BinarySearchDeferredEqualityDetection( new PDBFunctionInfo( null, addr, section, 0u ), PDB_FUNC_ADDRESS_AND_TOKEN_BASED );
                  if ( funcIdx >= 0 )
                  {
                     // Skip the functions that already have lines
                     while ( funcIdx < lines.Length && lines[funcIdx] != null && functions[funcIdx].segment == section && functions[funcIdx].address == addr )
                     {
                        ++funcIdx;
                     }
                     if ( funcIdx < lines.Length && functions[funcIdx].segment == section && functions[funcIdx].address == addr )
                     {
                        // We found the correct function index
                        var thisLines = new List<Tuple<Int32, PDBLine>>();
                        // Read line data
                        while ( idx < endIdx )
                        {
                           var srcIdx = array.ReadInt32LEFromBytes( ref idx );
                           var lineCount = array.ReadInt32LEFromBytes( ref idx );
                           // Skip size information
                           idx += INT_SIZE;
                           // Save line and column start indices
                           var lineStartIdx = idx;
                           var columnStartIdx = idx + 8 * lineCount; // Each line is 2 integers
                           // Iterate each line
                           for ( var i = 0; i < lineCount; ++i )
                           {
                              // Reset index after possible column read
                              idx = lineStartIdx + LINE_MULTIPLIER * i;
                              var line = new PDBLine()
                              {
                                 Offset = array.ReadInt32LEFromBytes( ref idx )
                              };
                              var lineFlags = array.ReadUInt32LEFromBytes( ref idx );
                              line.LineStart = (Int32) ( lineFlags & 0x00ffffffu ); // Lower 3 bytes are start line of statement/expression
                              line.LineEnd = line.LineStart + (Int32) ( ( lineFlags & 0x7f000000u ) >> 24 ); // High seven bits is delta of line
                              line.IsStatement = ( lineFlags & 0x80000000u ) == 0; // Highest bit is whether the line is statement
                              if ( ( flags & 1 ) != 0 )
                              {
                                 // Column info present
                                 idx = columnStartIdx + COLUMN_MULTIPLIER * i; // Each column info is two shorts
                                 line.ColumnStart = array.ReadUInt16LEFromBytes( ref idx );
                                 line.ColumnEnd = array.ReadUInt16LEFromBytes( ref idx );
                              }
                              thisLines.Add( Tuple.Create( srcIdx, line ) );
                           }
                        }
                        lines[funcIdx] = thisLines;
                     }
#if DEBUG
                     else
                     {
                        throw new PDBException( "Debyyg" );
                     }
#endif
                  }
#if DEBUG
                  else
                  {
                     throw new PDBException( "Debyyg" );
                  }
#endif
                  break;
               default:
#if DEBUG
                  throw new PDBException( "Debyyg" );
#else
                  break;
#endif
            }
            idx = endIdx;
         }

         // Postprocess line infos
         for ( var i = 0; i < lines.Length; ++i )
         {
            var lineList = lines[i];
            if ( lineList != null )
            {
               foreach ( var tuple in lineList )
               {
                  var line = tuple.Item2;
                  line.Source = sourcesLocal[tuple.Item1];
                  functions[i].function.Lines.Add( line );
               }
            }
         }
      }

      internal static Int32 AmountOfPagesTaken( Int32 byteSize, Int32 pageSize )
      {
         return ( byteSize + pageSize - 1 ) / pageSize;
      }

      private static Stream SeekToPage( this StreamInfo stream, Int32 pageSize, Int32 page, Int32 pageOffset )
      {
         return stream.stream.SeekToPage( stream.begin, pageSize, page, pageOffset );
      }

      private static Byte[] ReadPagedData( this StreamInfo stream, Int32 pageSize, Int32[] pages, Int32 byteCount )
      {
         var array = new Byte[byteCount];
         stream.ReadPagedData( pageSize, pages, byteCount, array );
         return array;
      }

      private static void ReadPagedData( this StreamInfo stream, Int32 pageSize, Int32[] pages, Int32 byteCount, Byte[] arrayToUse )
      {
         var idx = 0;
         foreach ( var page in pages )
         {
            var len = Math.Min( pageSize, ( byteCount - idx ) );
            stream.SeekToPage( pageSize, page, 0 )
               .ReadSpecificAmount( arrayToUse, idx, len );
            idx += len;
         }
      }

      internal static Boolean IsBitSet( this UInt32[] array, Int32 index )
      {
         var arrayIdx = index / 32;
         return arrayIdx < array.Length ?
            ( array[arrayIdx] & ( 1u << ( index % 32 ) ) ) != 0 :
            false;
      }

      private static void Align4( ref Int32 value )
      {
         value += MultipleOf4( value ) - value;
      }

      internal static Int32 MultipleOf4( Int32 value )
      {
         return ( value + 3 ) & ~3;
      }

      internal static String ReadZeroTerminatedUTF16String( this Byte[] array, ref Int32 idx )
      {
         var curIdx = idx;
         while ( curIdx < array.Length && array[curIdx] != 0 )
         {
            curIdx += 2;
         }
         var result = UTF16.GetString( array, idx, curIdx - idx );
         idx = curIdx + 2;
         return result;
      }

      private static PDBLocalScope NewPDBLocalScope( Byte[] array, ref Int32 idx )
      {
         Int32 offset;
         return new PDBLocalScope()
         {
            Offset = ( offset = array.ReadInt32LEFromBytes( ref idx ) ),
            Length = array.ReadInt32LEFromBytes( ref idx ) - offset
         };
      }

      private static PDBSynchronizationPoint NewPDBSynchronizationPoint( Byte[] array, ref Int32 idx )
      {
         return new PDBSynchronizationPoint()
         {
            SyncOffset = array.ReadInt32LEFromBytes( ref idx ),
            ContinuationMethodToken = array.ReadUInt32LEFromBytes( ref idx ),
            ContinuationOffset = array.ReadInt32LEFromBytes( ref idx )
         };
      }

      private static void HandleAsyncOEM( PDBFunction func, Byte[] array, ref Int32 idx )
      {
         var result = new PDBAsyncMethodInfo()
         {
            KickoffMethodToken = array.ReadUInt32LEFromBytes( ref idx ),
            CatchHandlerOffset = array.ReadInt32LEFromBytes( ref idx )
         };
         var syncPointCount = array.ReadInt32LEFromBytes( ref idx );
         for ( var i = 0; i < syncPointCount; ++i )
         {
            result.SynchronizationPoints.Add( NewPDBSynchronizationPoint( array, ref idx ) );
         }
         func.AsyncMethodInfo = result;
      }

      private static PDBSlot NewPDBSlot( Byte[] array, ref Int32 idx )
      {
         return new PDBSlot()
         {
            SlotIndex = array.ReadInt32LEFromBytes( ref idx ),
            TypeToken = array.ReadUInt32LEFromBytes( ref idx ),
            //Address = array.ReadInt32LEFromBytes( ref idx ),
            Flags = (PDBSlotFlags) array.Skip( ref idx, 6 ) // Skip address & segment
               .ReadInt16LEFromBytes( ref idx ),
            Name = array.ReadZeroTerminatedStringFromBytes( ref idx, NameEncoding ),
         };
      }

      private static PDBScope NewPDBScope( Byte[] array, ref Int32 idx, UInt32 funcOffset, Int32 listsStartIdx, out UInt32 address, out UInt16 segment, out Int32 end )
      {
         /*var parent = */
         array.ReadInt32LEFromBytes( ref idx );
         end = array.ReadInt32LEFromBytes( ref idx );
         var length = array.ReadInt32LEFromBytes( ref idx );
         address = array.ReadUInt32LEFromBytes( ref idx );
         segment = array.ReadUInt16LEFromBytes( ref idx );
         var result = new PDBScope()
         {
            Name = array.ReadZeroTerminatedStringFromBytes( ref idx, NameEncoding ),
            Offset = (Int32) ( address - funcOffset ),
            Length = length
         };

         idx = listsStartIdx;
         ReadListsFromBytes( result, array, ref idx, end, funcOffset );
         return result;
      }

      private static PDBFunction NewPDBFunction( String moduleName, Byte[] array, ref Int32 idx, Int32 listsStartIdx, out UInt32 address, out UInt16 segment )
      {
         var result = new PDBFunction();

         /*var parent = */
         array.ReadInt32LEFromBytes( ref idx );
         var end = array.ReadInt32LEFromBytes( ref idx );
         /*var next = */
         array.ReadInt32LEFromBytes( ref idx );
         result.Length = array.ReadInt32LEFromBytes( ref idx );
         /*var debugStart = */
         array.ReadInt32LEFromBytes( ref idx );
         /*var debugEnd = */
         array.ReadInt32LEFromBytes( ref idx );
         result.Token = array.ReadUInt32LEFromBytes( ref idx );
         address = array.ReadUInt32LEFromBytes( ref idx );
         segment = array.ReadUInt16LEFromBytes( ref idx );
         /*var flags = */
         array.ReadByteFromBytes( ref idx );
         /*var returnReg = */
         array.ReadUInt16LEFromBytes( ref idx );
         result.Name = array.ReadZeroTerminatedStringFromBytes( ref idx, NameEncoding );

         idx = listsStartIdx;
         try
         {
            ReadListsFromBytes( result, array, ref idx, end, address );
         }
         catch ( Exception e )
         {
            throw new PDBException( "Error when reading function " + result.Name + " in " + moduleName + ".", e );
         }

         return result;
      }

      private static void ReadListsFromBytes( PDBScopeOrFunction scope, Byte[] array, ref Int32 idx, Int32 max, UInt32 funcOffset )
      {
         UInt16 blockLen;
         while ( idx < max )
         {
            blockLen = array.ReadUInt16LEFromBytes( ref idx );
            var blockEndIdx = idx + blockLen;
#if DEBUG
            if ( blockEndIdx % 4 != 0 )
            {
               throw new PDBException( "Debyyg" );
            }
#endif
            var setIdx = true;
            var sym = array.ReadUInt16LEFromBytes( ref idx );
            switch ( sym )
            {
               case SYM_OEM:
                  if ( scope is PDBFunction )
                  {
                     HandleOEM( (PDBFunction) scope, array, ref idx );
                  }
                  else
                  {
                     throw new PDBException( "OEM not supported for other than functions." );
                  }
                  break;
               case SYM_SCOPE:
                  UInt32 scopeAddress; UInt16 scopeSegment;
                  Int32 end;
                  scope.Scopes.Add( NewPDBScope( array, ref idx, funcOffset, blockEndIdx, out scopeAddress, out scopeSegment, out end ) );
                  //idx = end;
                  setIdx = false;
                  break;
               case SYM_MANAGED_SLOT:
                  scope.Slots.Add( NewPDBSlot( array, ref idx ) );
                  break;
               case SYM_USED_NS:
                  scope.UsedNamespaces.Add( array.ReadZeroTerminatedStringFromBytes( ref idx, NameEncoding ) );
                  break;
               case SYM_MANAGED_CONSTANT:
                  scope.Constants.Add( ReadManagedConstant( array, ref idx ) );
                  break;
               default:
#if DEBUG
                  if ( SYM_END != sym )
                  {
                     throw new PDBException( "Debyyg" );
                  }
#endif
                  break;
            }
            if ( setIdx )
            {
               idx = blockEndIdx;
            }
         }

         if ( idx != max )
         {
            throw new PDBException( "PDB scope or function (" + scope.Name + ") did not end properly." );
         }
         blockLen = array.ReadUInt16LEFromBytes( ref idx );
         if ( SYM_END != array.ReadUInt16LEFromBytes( ref idx ) )
         {
            throw new PDBException( "PDB scope or function (" + scope.Name + ") missing END symbol." );
         }
      }

      private static PDBConstant ReadManagedConstant( Byte[] array, ref Int32 idx )
      {
         return new PDBConstant()
         {
            Token = array.ReadUInt32LEFromBytes( ref idx ),
            Value = ReadPDBConstantValue( array, ref idx ),
            Name = array.ReadZeroTerminatedStringFromBytes( ref idx, PDBIO.NameEncoding )
         };
         //var token = array.ReadUInt32LEFromBytes( ref idx );
         //var startIdx = idx;
         //var value = ReadPDBConstantValue( array, ref idx );
         //var size = idx - startIdx;
         //var name = array.ReadZeroTerminatedStringFromBytes( ref idx, PDBIO.NameEncoding );
         //System.Diagnostics.Debug.WriteLine( "CONSTANT NAMED " + name + " took " + size + " bytes ( " + array.ReadUInt16LEFromBytesNoRef( startIdx ).ToString( "X4" ) + ")." );
         //return new PDBConstant()
         //{
         //   Token = token,
         //   Value = value,
         //   Name = name
         //};
      }

      private static Object ReadPDBConstantValue( Byte[] array, ref Int32 idx )
      {
#if DEBUG
         const bool constBool = false;
         const bool constBool2 = true;
         const char constChar = 'G';
         const byte constByte = 5;
         const sbyte constSByte = -4;
         const sbyte constSByte2 = 100;
         const short constShort = -666;
         const ushort constUShort = 666;
         const int constInt = -1000000;
         const uint constUInt = 1000000;
         const long constLong = -5000000000;
         const ulong constULong = 5000000000;
         const decimal constDecimal = 0.06M;
         const float constFloat = 0.16f;
         const double constDobule = 0.26d;
         const string constStringNull = null;
         System.Diagnostics.Debug.WriteLine(
            constBool
            + " " + constBool2
            + " " + constChar
            + " " + constByte
            + " " + constSByte
            + " " + constSByte2
            + " " + constShort
            + " " + constUShort
            + " " + constInt
            + " " + constUInt
            + " " + constLong
            + " " + constULong
            + " " + constDecimal
            + " " + constFloat
            + " " + constDobule
            + " " + constStringNull
            );
#endif
         // Read the LEAF_ENUM_e value
         Object retVal;
         var valueKind = array.ReadUInt16LEFromBytes( ref idx );
         if ( valueKind <= MAX_PDB_CONSTANT_NUMERIC )
         {
            // As per LEAF_ENUM_e spec, anything less than 0x8000 is value itself
            // This does result in the loss of type information, but that's how CodeView seems to work...
            retVal = (Int32) valueKind;
         }
         else
         {
            switch ( valueKind )
            {
               case PDBIO.CONST_LF_CHAR:
                  retVal = array.ReadSByteFromBytes( ref idx );
                  break;
               case PDBIO.CONST_LF_SHORT:
                  retVal = array.ReadInt16LEFromBytes( ref idx );
                  break;
               case PDBIO.CONST_LF_USHORT:
                  retVal = array.ReadUInt16LEFromBytes( ref idx ); // Notice that 'char' is serialized as ushort!
                  break;
               case PDBIO.CONST_LF_LONG:
                  retVal = array.ReadInt32LEFromBytes( ref idx );
                  break;
               case PDBIO.CONST_LF_ULONG:
                  retVal = array.ReadUInt32LEFromBytes( ref idx );
                  break;
               case PDBIO.CONST_LF_REAL_32:
                  retVal = array.ReadSingleLEFromBytes( ref idx );
                  break;
               case PDBIO.CONST_LF_REAL_64:
                  retVal = array.ReadDoubleLEFromBytes( ref idx );
                  break;
               case PDBIO.CONST_LF_QUADWORD:
                  retVal = array.ReadInt64LEFromBytes( ref idx );
                  break;
               case PDBIO.CONST_LF_UQUADWORD:
                  retVal = array.ReadUInt64LEFromBytes( ref idx );
                  break;
               case PDBIO.CONST_LF_VARSTRING:
                  var len = array.ReadUInt16LEFromBytes( ref idx );
                  retVal = PDBIO.NameEncoding.GetString( array, idx, len );
                  idx += len;
                  break;
               case PDBIO.CONST_LF_DECIMAL:
                  var intArray = array.ReadInt32ArrayLEFromBytes( ref idx, 4 );
                  retVal = new Decimal( intArray[2], intArray[3], intArray[1], intArray[0] < 0, (Byte) ( ( intArray[0] & 0x00FF0000 ) >> 16 ) );
                  break;
               default:
#if DEBUG
                  throw new PDBException( "Debyyg" );
#else
                  retVal = null;
                  break;
#endif
            }
         }
         return retVal;
      }

      private static void HandleOEM( PDBFunction func, Byte[] array, ref Int32 idx )
      {
         var guid = array.ReadGUIDFromBytes( ref idx );
         var typeIdx = array.ReadUInt32LEFromBytes( ref idx );
         if ( guid == GUIDs.MSIL_METADATA_GUID )
         {
            var oemName = array.ReadZeroTerminatedUTF16String( ref idx );
            switch ( oemName )
            {
               case MD_OEM_NAME:
                  HandleMD2OEM( func, array, ref idx );
                  break;
               case ASYNC_METHOD_OEM_NAME:
                  HandleAsyncOEM( func, array, ref idx );
                  break;
               case ENC_OEM_NAME:
                  HandleENCOEM( func, array, ref idx );
                  break;
               default:
#if DEBUG
                  throw new PDBException( "Debyyg" );
#else
                  break;
#endif
            }
         }
         else
         {
            throw new PDBException( "Unknown OEM guid: " + guid + " with type index " + typeIdx + " in " + func.Name + "." );
         }
      }
#pragma warning disable 1591
#if DEBUG

      public
#else
      private
#endif
         static void HandleENCOEM( PDBFunction func, Byte[] array, ref Int32 idx )
      {
         func.ENCID = array.ReadUInt32LEFromBytes( ref idx );
      }

#if DEBUG
      public
#else
      private
#endif
         static void HandleMD2OEM( PDBFunction func, Byte[] array, ref Int32 idx )
      {
         ++idx; // Skip version byte
         var amountOfMDInfos = array.ReadByteFromBytes( ref idx );
         Align4( ref idx );
         while ( amountOfMDInfos > 0 )
         {
            // Save current index
            var oldIdx = idx;
            // Skip version byte, again
            ++idx;
            // Read metadata item kind
            var mdKind = array.ReadByteFromBytes( ref idx );
            // Align
            Align4( ref idx );
            // Read metadata item byte size
            var byteCount = array.ReadInt32LEFromBytes( ref idx );
            switch ( mdKind )
            {
               case MD2_USED_NAMESPACES:
                  // Counts for using namespace -lists of scopes.
                  // Could set the capacities of the using lists of each slot, but at this point the slots may or may not have been created...
                  // So just skip.

                  //var uSize = array.ReadUInt16LEFromBytes( ref idx );
                  //var arrrr = new UInt16[uSize];
                  //for ( var i = 0; i < uSize; ++i )
                  //{
                  //   arrrr[i] = array.ReadUInt16LEFromBytes( ref idx );
                  //}
                  break;
               case MD2_FORWARDING_METHOD_TOKEN:
                  // Forwarding information
                  func.ForwardingMethodToken = array.ReadUInt32LEFromBytes( ref idx );
                  break;
               case MD2_FORWARDING_MODULE_METHOD_TOKEN:
#if DEBUG
                  if ( func.ModuleForwardingMethodToken != 0 )
                  {
                     throw new PDBException( "Debyyg" );
                  }
#endif
                  func.ModuleForwardingMethodToken = array.ReadUInt32LEFromBytes( ref idx );
                  break;
               case MD2_LOCAL_SCOPES:
                  // Local scopes
                  var localSize = array.ReadInt32LEFromBytes( ref idx );
                  func.LocalScopes.Capacity = localSize;
                  for ( var i = 0; i < localSize; ++i )
                  {
                     func.LocalScopes.Add( NewPDBLocalScope( array, ref idx ) );
                  }
                  break;
               case MD2_ITERATOR_CLASS:
                  // Forward iterator class
                  func.IteratorClass = array.ReadZeroTerminatedUTF16String( ref idx );
                  break;
               case MD2_DYNAMICS:
                  // TODO
                  break;
               case MD2_ENC_LOCALS:
                  func.ReadENCLocals( array, ref idx, oldIdx + byteCount );
                  break;
               case MD2_ENC_LAMBDAS:
                  func.ReadENCLambas( array, ref idx, oldIdx + byteCount );
                  break;
               default:
#if DEBUG
                  throw new PDBException( "Debyyg" );
#else
                  break;
#endif
            }
            // Set index always to the size specified in bytes
            idx = oldIdx + byteCount;
            // Remember decrement loop variable
            --amountOfMDInfos;
         }
      }
#pragma warning restore 1591

      private static readonly IComparer<PDBFunctionInfo> PDB_FUNC_ADDRESS_AND_TOKEN_BASED = ComparerFromFunctions.NewComparer<PDBFunctionInfo>( ( x, y ) =>
      {
         var retVal = x.segment.CompareTo( y.segment );
         if ( retVal == 0 )
         {
            retVal = x.address.CompareTo( y.address );
            if ( retVal == 0 && x.function != null && y.function != null )
            {
               retVal = x.function.Token.CompareTo( y.function.Token );
            }
         }
         return retVal;
      } );

      private static void SetAtSpecificIndex<T>( this List<T> list, Int32 index, T value )
      {
         var c = list.Count;
         if ( index < c )
         {
            list[index] = value;
         }
         else if ( index == c )
         {
            list.Add( value );
         }
         else
         {
            list.AddRange( Enumerable.Repeat<T>( default( T ), index - c ) );
            list.Add( value );
         }
      }

      private static void ReadENCLocals( this PDBFunction func, Byte[] array, ref Int32 idx, Int32 maxIdx )
      {
         var locals = func.GetOrCreateENCInfo().LocalSlots;
         var syntaxOffsetBaseline = -1;
         var b = array[idx];
         if ( b == PDBIO.CUSTOM_SYNTAX_OFFSET_BASELINE )
         {
            ++idx;
            syntaxOffsetBaseline = -array.DecompressUInt32( ref idx );
         }
         while ( idx < maxIdx )
         {
            b = array.ReadByteFromBytes( ref idx );
            LocalSlotDebugInfo dbg;
            if ( b == 0 )
            {
               dbg = new LocalSlotDebugInfo();
            }
            else
            {
               var kind = (Byte) ( ( b & 0x3F ) - 1 );
               var hasOrdinal = ( b & ( 1 << 7 ) ) != 0;
               dbg = new LocalSlotDebugInfo()
               {
                  SyntaxOffset = array.DecompressUInt32( ref idx ) + syntaxOffsetBaseline,
                  LocalIndex = hasOrdinal ? array.DecompressUInt32( ref idx ) : 0,
                  SynthesizedKind = kind
               };
            }
            locals.Add( dbg );
         }
      }

      private static void ReadENCLambas( this PDBFunction func, Byte[] array, ref Int32 idx, Int32 maxIdx )
      {
         var enc = func.GetOrCreateENCInfo();
         enc.MethodOrdinal = array.DecompressUInt32( ref idx ) - 1;
         var syntaxOffsetBaseline = -array.DecompressUInt32( ref idx );
         var closureCount = array.DecompressUInt32( ref idx );

         var closures = enc.Closures;
         for ( var i = 0; i < closureCount; ++i )
         {
            closures.Add( new ClosureDebugInfo()
            {
               SyntaxOffset = array.DecompressUInt32( ref idx ) + syntaxOffsetBaseline
            } );
         }

         var lambas = enc.Lambdas;
         while ( idx < maxIdx )
         {
            var dbg = new LambdaDebugInfo()
            {
               SyntaxOffset = array.DecompressUInt32( ref idx ) + syntaxOffsetBaseline,
            };
            if ( idx < maxIdx )
            {
               dbg.ClosureIndex = array.DecompressUInt32( ref idx ) + LAMBDA_MIN_CLOSURE_INDEX;
            }
            lambas.Add( dbg );
         }
      }

      private static EditAndContinueMethodDebugInformation GetOrCreateENCInfo( this PDBFunction func )
      {
         var retVal = func.ENCInfo;
         if ( retVal == null )
         {
            retVal = new EditAndContinueMethodDebugInformation();
            func.ENCInfo = retVal;
         }

         return retVal;
      }
   }
}
