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

namespace CILAssemblyManipulator.Physical.PDB
{
   public static partial class PDBIO
   {

      private const UInt32 METHOD_TABLE = 0x06000000;
      private const UInt32 METHOD_TABLE_INDEX_MASK = 0x00FFFFFF;

      private static readonly Encoding NAME_ENCODING = new UTF8Encoding( false, true );
      private static readonly Encoding UTF16 = new UnicodeEncoding( false, false, true );

      private const String SOURCE_SERVER_STREAM_NAME = "srcsrv";

      private const String MD_OEM_NAME = "MD2";
      private const String ASYNC_METHOD_OEM_NAME = "asyncMethodInfo";
      private const String ENC_OEM_NAME = "ENC";

      public static PDBInstance FromStream( Stream readingStream, Boolean ignoreCaseSourceFileStreamNames = true )
      {
         var stream = new StreamInfo( readingStream );
         // Header
         // Skip first 32 bytes (it's just text and magic ints)
         stream.stream.SeekFromCurrent( 32 );
         var array = new Byte[20];
         stream.stream.ReadWholeArray( array );
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
         array = stream.stream.ReadWholeArray( INT_SIZE * directoryPageOffsetPageCount );
         idx = 0;
         var directoryPageOffsets = array.ReadInt32ArrayLEFromBytes( ref idx, directoryPageOffsetPageCount );

         // Directory
         // Read directory pages
         array = stream.ReadPagedData( pageSize, directoryPageOffsets, directoryPageCount * INT_SIZE );
         idx = 0;
         directoryPageOffsets = array.ReadInt32ArrayLEFromBytes( ref idx, directoryPageCount );

         // Read directory data
         array = stream.ReadPagedData( pageSize, directoryPageOffsets, directoryByteCount );
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
         array = stream.ReadPagedData( pageSize, dataStreamPages[3], dataStreamSizes[3] );
         idx = 0;
         DBIHeader dbiHeader; DBIModuleInfo[] modules; DBIDebugHeader debugHeader;
         LoadDBIStream( array, ref idx, out dbiHeader, out modules, out debugHeader );

         // Make array have the size of biggest stream size to be used, so we wouldn't need to create new array each time for stream
         // TODO this might still be too much in some gigantic function cases...
         array = new Byte[dataStreamSizes.Where( ( siz, i ) => i != 3 && i != dbiHeader.symRecStream && i != dbiHeader.psSymStream && i != dbiHeader.gsSymStream ).Max()];

         // Create result instance
         var instance = new PDBInstance();

         // Read root stream
         stream.ReadPagedData( pageSize, dataStreamPages[1], dataStreamSizes[1], array );
         idx = 8; // Skip version & timestamp
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
            stream.ReadPagedData( pageSize, dataStreamPages[srcStrmIdx], dataStreamSizes[srcStrmIdx], array );
            idx = 0;
            instance.SourceServer = array.ReadStringWithEncoding( ref idx, dataStreamSizes[srcStrmIdx], NAME_ENCODING );
         }

         // Read name index.
         stream.ReadPagedData( pageSize, dataStreamPages[namesStreamIdx], dataStreamSizes[namesStreamIdx], array );
         idx = 0;
         /*var nsSig = */
         array.ReadUInt32LEFromBytes( ref idx );
         /*var nsVer = */
         array.ReadInt32LEFromBytes( ref idx );
         var nameIndex = LoadNameIndex( array, ref idx );

         // TEMP BEGIN
         //stream.ReadPagedData( pageSize, dataStreamPages[7], dataStreamSizes[7], array );
         //idx = 4;
         //var max = array.ReadInt32LEFromBytes( ref idx );
         //idx = 0x58;
         //var srcHdrStrs = new List<String[]>();
         //while ( idx < max )
         //{
         //   var sIdx = array.ReadInt32LEFromBytes( ref idx );
         //   var str1 = nameIndex[sIdx];
         //   var max2 = array.ReadInt32LEFromBytes( ref idx );
         //   if ( max2 != 0x28 )
         //   {

         //   }
         //   max2 += idx - 4;
         //   idx += 8;
         //   if ( array.ReadInt32LEFromBytes( ref idx ) != 0x58 )
         //   {

         //   }
         //   var strs = new List<String>();
         //   strs.Add( str1 );
         //   while ( idx < max2 )
         //   {
         //      var curStr = nameIndex[array.ReadInt32LEFromBytes( ref idx )];
         //      strs.Add( curStr );
         //      if ( String.Equals( curStr, strs[0] ) )
         //      {
         //         if ( array.ReadInt32LEFromBytes( ref idx ) != 0x65 )
         //         {

         //         }
         //         idx = max2;
         //         break;
         //      }
         //   }
         //   srcHdrStrs.Add( new String[] { "" + sIdx + " - " + ( sIdx % 52 ) + ": " + str1 } );// strs.ToArray() );
         //}
         //var srcHdrStr = String.Join( "\n", srcHdrStrs.Select( s => String.Join( "\n", s ) ) );

         // TEMP END
         // Read modules.
         foreach ( var module in modules )
         {
            if ( module.stream > 0 && module.stream < dataStreamCount )
            {
               stream.ReadPagedData( pageSize, dataStreamPages[module.stream], dataStreamSizes[module.stream], array );
               LoadFunctionsFromDBIModule( stream, pageSize, dataStreamPages, dataStreamSizes, array, streamNameIndices, nameIndex, module, instance );
            }
         }

         // Apply token remapping table if exists
         if ( debugHeader != null && debugHeader.snTokenRidMap != UInt16.MinValue && debugHeader.snTokenRidMap != UInt16.MaxValue )
         {
            var tokenRemapSize = dataStreamSizes[debugHeader.snTokenRidMap];
            stream.ReadPagedData( pageSize, dataStreamPages[debugHeader.snTokenRidMap], tokenRemapSize, array );
            idx = 0;
            var tokens = array.ReadUInt32ArrayLEFromBytes( ref idx, tokenRemapSize / INT_SIZE );
            foreach ( var function in instance.Modules.Values.SelectMany( mod => mod.Functions ) )
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
               retVal.Add( array.ReadZeroTerminatedStringFromBytes( ref idx, NAME_ENCODING ), idxValue );
               idx = oldIdx;
            }
         }

         if ( retVal.Count != nameCount )
         {
            throw new PDBException( "Name index claimed to have " + max + " entries but had only " + retVal.Count + " entries." );
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
               retVal.Add( nameIdx, array.ReadZeroTerminatedStringFromBytes( ref idx, NAME_ENCODING ) );
               idx = oldIdx;
            }
         }

         return retVal;
      }

      private static void LoadDBIStream( Byte[] array, ref Int32 idx, out DBIHeader dbiHeader, out DBIModuleInfo[] modules, out DBIDebugHeader debugHeader )
      {
         dbiHeader = new DBIHeader( array, ref idx );

         // Read module info
         var max = idx + dbiHeader.moduleInfoSize;
         var list = new List<DBIModuleInfo>();
         while ( idx < max )
         {
            list.Add( new DBIModuleInfo( array, ref idx, NAME_ENCODING ) );
            Align4( ref idx );
         }
         modules = list.ToArray();

         // Skip following parts: Section Contribution, Section Map, File Info, TSM, EC
         idx += dbiHeader.secConSize + dbiHeader.secMapSize + dbiHeader.fileInfoSize + dbiHeader.tsMapSize + dbiHeader.ecInfoSize;

         // Read debug header if present
         debugHeader = dbiHeader.debugHeaderSize > 0 ? new DBIDebugHeader( array, ref idx ) : null;
      }

      private static void LoadFunctionsFromDBIModule(
         StreamInfo stream,
         Int32 pageSize,
         Int32[][] streamPages,
         Int32[] streamSizes,
         Byte[] array,
         IDictionary<String, Int32> streamNameIndices,
         IDictionary<Int32, String> nameIndex,
         DBIModuleInfo moduleInfo,
         PDBInstance instance
         )
      {
         var idx = INT_SIZE; // Skip signature
         var thisFuncs = new List<PDBFunctionInfo>();
         var module = instance.Modules.GetOrAdd_NotThreadSafe( moduleInfo.moduleName, n => new PDBModule() );
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
                  Int32 addr; UInt16 seg;
                  var func = NewPDBFunction( moduleInfo.moduleName, array, ref idx, blockEndIdx, out addr, out seg );
                  module.Functions.Add( func );
                  thisFuncs.Add( new PDBFunctionInfo( func, addr, seg, -1 ) ); // Function pointer is not used during reading
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
            LoadSourcesAndLinesFromModuleStream( stream, pageSize, streamPages, streamSizes, array, idx, idx + moduleInfo.linesByteCount, streamNameIndices, nameIndex, instance, thisFuncs );
         }
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
         PDBInstance instance,
         List<PDBFunctionInfo> functions
         )
      {
         var sourcesLocal = new Dictionary<Int32, Tuple<String, PDBSource>>();
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
                     var pdbSource = instance.Sources.GetOrAdd_NotThreadSafe( name, sourceName =>
                     {
                        var source = new PDBSource();
                        Int32 sourceStreamIdx;
                        if ( streamNameIndices.TryGetValue( SOURCE_FILE_PREFIX + sourceName, out sourceStreamIdx ) )
                        {
                           var sourceBytes = stream.ReadPagedData( pageSize, streamPages[sourceStreamIdx], streamSizes[sourceStreamIdx] );
                           var tmpIdx = 0;
                           source.Language = sourceBytes.ReadGUIDFromBytes( ref tmpIdx );
                           source.Vendor = sourceBytes.ReadGUIDFromBytes( ref tmpIdx );
                           source.DocumentType = sourceBytes.ReadGUIDFromBytes( ref tmpIdx );
                           source.HashAlgorithm = sourceBytes.ReadGUIDFromBytes( ref tmpIdx );
                           source.Hash = sourceBytes.CreateAndBlockCopyTo( ref tmpIdx, sourceBytes.Length - tmpIdx );
                        }
                        return source;
                     } );

                     sourcesLocal.Add( curSrcFileIdx, Tuple.Create( name, pdbSource ) );
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

                  var addr = array.ReadInt32LEFromBytes( ref idx );
                  var section = array.ReadUInt16LEFromBytes( ref idx );
                  var flags = array.ReadUInt16LEFromBytes( ref idx );
                  /*var cod = */
                  array.ReadUInt32LEFromBytes( ref idx );
                  var funcIdx = functions.BinarySearchDeferredEqualityDetection( new PDBFunctionInfo( null, addr, section, -1 ), PDB_FUNC_ADDRESS_AND_TOKEN_BASED );
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
               foreach ( var line in lineList )
               {
                  functions[i].function.Lines.GetOrAdd_NotThreadSafe( sourcesLocal[line.Item1].Item1, ni => new List<PDBLine>() )
                     .Add( line.Item2 );
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
         stream.stream.SeekFromBegin( stream.begin + page * pageSize + pageOffset );
         return stream.stream;
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

      private static PDBAsyncMethodInfo NewPDBAsyncMethodInfo( Byte[] array, ref Int32 idx )
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
         return result;
      }

      private static PDBSlot NewPDBSlot( Byte[] array, ref Int32 idx )
      {
         var slot = new PDBSlot();
         slot.SlotIndex = array.ReadInt32LEFromBytes( ref idx );
         slot.TypeToken = array.ReadUInt32LEFromBytes( ref idx );
         slot.Address = array.ReadInt32LEFromBytes( ref idx );
         /*var segment = */
         array.ReadUInt16LEFromBytes( ref idx );
         slot.Flags = (PDBSlotFlags) array.ReadUInt16LEFromBytes( ref idx );
         slot.Name = array.ReadZeroTerminatedStringFromBytes( ref idx, NAME_ENCODING );
         return slot;
      }

      private static PDBScope NewPDBScope( Byte[] array, ref Int32 idx, Int32 funcOffset, Int32 listsStartIdx, out Int32 address, out UInt16 segment, out Int32 end )
      {
         /*var parent = */
         array.ReadInt32LEFromBytes( ref idx );
         end = array.ReadInt32LEFromBytes( ref idx );
         var length = array.ReadInt32LEFromBytes( ref idx );
         address = array.ReadInt32LEFromBytes( ref idx );
         segment = array.ReadUInt16LEFromBytes( ref idx );
         var result = new PDBScope()
         {
            Name = array.ReadZeroTerminatedStringFromBytes( ref idx, NAME_ENCODING ),
            Offset = address - funcOffset,
            Length = length
         };

         idx = listsStartIdx;
         ReadListsFromBytes( result, array, ref idx, end, funcOffset );
         return result;
      }

      private static PDBFunction NewPDBFunction( String moduleName, Byte[] array, ref Int32 idx, Int32 listsStartIdx, out Int32 address, out UInt16 segment )
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
         address = array.ReadInt32LEFromBytes( ref idx );
         segment = array.ReadUInt16LEFromBytes( ref idx );
         /*var flags = */
         array.ReadByteFromBytes( ref idx );
         /*var returnReg = */
         array.ReadUInt16LEFromBytes( ref idx );
         result.Name = array.ReadZeroTerminatedStringFromBytes( ref idx, NAME_ENCODING );

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

      private static void ReadListsFromBytes( PDBScopeOrFunction scope, Byte[] array, ref Int32 idx, Int32 max, Int32 funcOffset )
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
            switch ( array.ReadUInt16LEFromBytes( ref idx ) )
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
                  Int32 scopeAddress; UInt16 scopeSegment;
                  Int32 end;
                  scope.Scopes.Add( NewPDBScope( array, ref idx, funcOffset, blockEndIdx, out scopeAddress, out scopeSegment, out end ) );
                  idx = end;
                  setIdx = false;
                  break;
               case SYM_MANAGED_SLOT:
                  scope.Slots.Add( NewPDBSlot( array, ref idx ) );
                  break;
               case SYM_USED_NS:
                  scope.UsedNamespaces.Add( array.ReadZeroTerminatedStringFromBytes( ref idx, NAME_ENCODING ) );
                  break;
               case SYM_MANAGED_CONSTANT:
                  // TODO
                  break;
               default:
#if DEBUG
                  idx -= 2;
                  var sym = array.ReadUInt16LEFromBytes( ref idx );
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
                  ReadMD2OEM( func, array, ref idx );
                  break;
               case ASYNC_METHOD_OEM_NAME:
                  func.AsyncMethodInfo = NewPDBAsyncMethodInfo( array, ref idx );
                  break;
               case ENC_OEM_NAME:
#if DEBUG
                  if ( func.ENCID != 0 )
                  {
                     throw new PDBException( "Debyyg" );
                  }
#endif
                  func.ENCID = array.ReadUInt32LEFromBytes( ref idx );
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

      private static void ReadMD2OEM( PDBFunction func, Byte[] array, ref Int32 idx )
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
                  // Using info
                  var uSize = array.ReadUInt16LEFromBytes( ref idx );
                  func.UsingCounts.Capacity = uSize;
                  for ( UInt16 i = 0; i < uSize; ++i )
                  {
                     func.UsingCounts.Add( array.ReadUInt16LEFromBytes( ref idx ) );
                  }
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
                  func.IteratorClass = array.ReadZeroTerminatedStringFromBytes( ref idx, NAME_ENCODING );
                  break;
               case MD2_YET_UNKNOWN:
                  break;
               case MD2_YET_UNKNOWN_2:
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
   }
}
