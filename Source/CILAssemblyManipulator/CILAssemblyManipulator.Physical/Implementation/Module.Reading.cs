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
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.Implementation
{


   internal struct DataDir
   {
      internal UInt32 rva;
      internal UInt32 size;

      internal DataDir( Stream stream, Byte[] tmpArray )
      {
         this.rva = stream.ReadU32( tmpArray );
         this.size = stream.ReadU32( tmpArray );
      }
   }

   internal static class ModuleReader
   {
      internal class BLOBHeapReader : AbstractHeapReader
      {

         internal BLOBHeapReader( Byte[] tmpArray, Stream stream, IDictionary<String, Tuple<Int64, UInt32>> streamSizeInfo )
            : base( tmpArray, stream, streamSizeInfo, Consts.BLOB_STREAM_NAME )
         {
         }

         internal Byte[] GetBLOB( Int32 idx )
         {
            var length = this._bytes.DecompressUInt32( ref idx );
            var result = length <= 0 ?
               Empty<Byte>.Array :
               this._bytes.CreateAndBlockCopyTo( ref idx, length );
            return result;
         }

         internal Byte[] ReadBLOB( Stream stream )
         {
            return this.GetBLOB( this.ReadHeapIndex( stream ) );
         }

         // Useful when reading signatures - don't need to create array for each.
         internal Byte[] WholeBLOBArray
         {
            get
            {
               return this._bytes;
            }
         }

         internal Int32 GetBLOBIndex( Stream stream )
         {
            Int32 heapIndex, blobSize;
            return this.GetBLOBIndex( stream, out heapIndex, out blobSize );
         }

         internal Int32 GetBLOBIndex( Stream stream, out Int32 blobSize )
         {
            Int32 heapIndex;
            return this.GetBLOBIndex( stream, out heapIndex, out blobSize );
         }

         internal Int32 GetBLOBIndex( Stream stream, out Int32 heapIndex, out Int32 blobSize )
         {
            var idx = this.ReadHeapIndex( stream );
            heapIndex = idx;
            blobSize = this._bytes.DecompressUInt32( ref idx );
            return idx;
         }
      }

      internal abstract class AbstractHeapReader
      {

         protected readonly Byte[] _tmpArray;
         protected readonly Byte[] _bytes;

         internal AbstractHeapReader( Byte[] tmpArray, Stream stream, IDictionary<String, Tuple<Int64, UInt32>> streamSizeInfo, String name )
         {
            this._tmpArray = tmpArray;
            Tuple<Int64, UInt32> tuple;
            if ( streamSizeInfo.TryGetValue( name, out tuple ) )
            {
               stream.SeekFromBegin( tuple.Item1 );
               this._bytes = new Byte[tuple.Item2];
               stream.ReadWholeArray( this._bytes );
            }
         }

         public Boolean IsWideIndex { get; set; }

         internal Int32 ReadHeapIndex( Stream stream )
         {
            return this.IsWideIndex ? stream.ReadI32( this._tmpArray ) : stream.ReadU16( this._tmpArray );
         }

      }

      internal abstract class AbstractStringHeapReader : AbstractHeapReader
      {
         internal protected readonly IDictionary<Int32, String> _strings;
         internal protected readonly Encoding _encoding;

         protected AbstractStringHeapReader( Byte[] tmpArray, Stream stream, IDictionary<String, Tuple<Int64, UInt32>> streamSizeInfo, String name, Encoding encoding )
            : base( tmpArray, stream, streamSizeInfo, name )
         {
            this._encoding = encoding;
            this._strings = new Dictionary<Int32, String>();
         }

         internal abstract String GetString( Int32 idx );
      }

      internal class SysStringHeapReader : AbstractStringHeapReader
      {
         internal SysStringHeapReader( Byte[] tmpArray, Stream stream, IDictionary<String, Tuple<Int64, UInt32>> streamSizeInfo )
            : base( tmpArray, stream, streamSizeInfo, Consts.SYS_STRING_STREAM_NAME, MetaDataConstants.SYS_STRING_ENCODING )
         {

         }

         internal override String GetString( Int32 idx )
         {
            // Don't need to be threadsafe - sysstrings are all read during meta-data reading
            String result;
            if ( !this._strings.TryGetValue( idx, out result ) )
            {
               if ( idx == 0 )
               {
                  result = null;
               }
               else
               {
                  var max = idx;
                  while ( max < this._bytes.Length && this._bytes[max] != 0 )
                  {
                     ++max;
                  }
                  result = this._encoding.GetString( this._bytes, idx, max - idx );
               }
               this._strings[idx] = result;
            }
            return result;
         }

         internal String ReadSysString( Stream stream )
         {
            return this.GetString( this.ReadHeapIndex( stream ) );
         }
      }

      internal class UserStringHeapReader : AbstractStringHeapReader
      {
         internal UserStringHeapReader( Byte[] tmpArray, Stream stream, IDictionary<String, Tuple<Int64, UInt32>> streamSizeInfo )
            : base( tmpArray, stream, streamSizeInfo, Consts.USER_STRING_STREAM_NAME, MetaDataConstants.USER_STRING_ENCODING )
         {

         }

         internal override String GetString( Int32 idx )
         {
            String result;
            if ( !this._strings.TryGetValue( idx, out result ) )
            {

               // User strings
               var arrayIdx = idx;
               var length = this._bytes.DecompressUInt32( ref arrayIdx ) - 1;
               if ( length == -1 )
               {
                  result = "";
               }
               else
               {
                  result = this._encoding.GetString( this._bytes, arrayIdx, length );
               }
               this._strings[idx] = result;
            }
            return result;
         }

      }

      internal class GUIDHeapReader : AbstractHeapReader
      {

         internal GUIDHeapReader( Byte[] tmpArray, Stream stream, IDictionary<String, Tuple<Int64, UInt32>> streamSizeInfo )
            : base( tmpArray, stream, streamSizeInfo, Consts.GUID_STREAM_NAME )
         {
         }

         internal Guid? GetGUID( Int32 idx )
         {
            if ( idx == 0 )
            {
               return null;
            }
            else
            {
               var array = new Byte[Consts.GUID_SIZE];
               Buffer.BlockCopy( this._bytes, ( idx - 1 ) % Consts.GUID_SIZE, array, 0, Consts.GUID_SIZE );
               return new Guid( array );
            }
         }

         internal Guid? ReadGUID( Stream stream )
         {
            return this.GetGUID( this.ReadHeapIndex( stream ) );
         }
      }

      private const Byte WIDE_SYS_STRING_FLAG = 0x01;
      private const Byte WIDE_GUID_FLAG = 0x02;
      private const Byte WIDE_BLOB_FLAG = 0x04;

      internal static CILMetaData ReadFromStream(
         Stream stream,
         ReadingArguments rArgs
         )
      {
         if ( rArgs == null )
         {
            rArgs = new ReadingArguments( false );
         }

         var headers = rArgs.Headers;

         var headersWereSet = headers != null;
         if ( !headersWereSet )
         {
            headers = new HeadersData( false );
         }

         Byte[] tmpArray = new Byte[8];

         // DOS header, skip to lfa new
         stream.SeekFromBegin( 60 );

         // PE file header
         // Skip to PE file header, and skip magic
         var lfaNew = stream.ReadU32( tmpArray );
         stream.SeekFromBegin( lfaNew + 4 );

         // Architecture
         var architecture = (ImageFileMachine) stream.ReadU16( tmpArray );
         headers.Machine = architecture;

         // Amount of sections
         var amountOfSections = stream.ReadU16( tmpArray );

         // Skip timestamp, symbol table pointer, number of symbols, optional header size
         stream.SeekFromCurrent( 14 );

         // Characteristics
         var characteristics = stream.ReadU16( tmpArray );

         // PE Optional header
         // Skip magic
         stream.SeekFromCurrent( 2 );
         headers.LinkerMajor = stream.ReadByteFromStream();
         headers.LinkerMinor = stream.ReadByteFromStream();

         // Skip sizes (x3)
         stream.SeekFromCurrent( 12 );
         var nativeEPRVA = stream.ReadU32( tmpArray );

         // Skip base of code, and base of data (base of data is not stored in pe64 files)
         var isPE64 = architecture.RequiresPE64();
         stream.SeekFromCurrent( isPE64 ? 4 : 8 );

         headers.ImageBase = isPE64 ? stream.ReadU64( tmpArray ) : stream.ReadU32( tmpArray );
         headers.SectionAlignment = stream.ReadU32( tmpArray );
         headers.FileAlignment = stream.ReadU32( tmpArray );
         headers.OSMajor = stream.ReadU16( tmpArray );
         headers.OSMinor = stream.ReadU16( tmpArray );
         headers.UserMajor = stream.ReadU16( tmpArray );
         headers.UserMinor = stream.ReadU16( tmpArray );
         headers.SubSysMajor = stream.ReadU16( tmpArray );
         headers.SubSysMinor = stream.ReadU16( tmpArray );

         // Skip reserved, image size, header size, file checksum
         stream.SeekFromCurrent( 16 );

         // Subsystem
         var subsystem = stream.ReadU16( tmpArray );

         // DLL flags
         var dllFlags = (DLLFlags) stream.ReadU16( tmpArray );
         headers.HighEntropyVA = dllFlags.HasFlag( DLLFlags.HighEntropyVA );

         // Stack reserve, stack commit, heap reserve, heap commit
         headers.StackReserve = isPE64 ? stream.ReadU64( tmpArray ) : stream.ReadU32( tmpArray );
         headers.StackCommit = isPE64 ? stream.ReadU64( tmpArray ) : stream.ReadU32( tmpArray );
         headers.HeapReserve = isPE64 ? stream.ReadU64( tmpArray ) : stream.ReadU32( tmpArray );
         headers.HeapCommit = isPE64 ? stream.ReadU64( tmpArray ) : stream.ReadU32( tmpArray );

         // Skip to import directory
         stream.SeekFromCurrent( 16 );
         var importDD = new DataDir( stream, tmpArray );

         // Skip to debug header
         stream.SeekFromCurrent( 32 );
         var debugDD = new DataDir( stream, tmpArray );

         // Skip to IAT header
         stream.SeekFromCurrent( 40 );
         var iatDD = new DataDir( stream, tmpArray );

         // Skip to CLI header
         stream.SeekFromCurrent( 8 );
         var cliDD = new DataDir( stream, tmpArray );

         // Reserved
         stream.SeekFromCurrent( 8 );

         // Read sections
         var sections = new SectionInfo[amountOfSections];
         for ( var i = 0u; i < amountOfSections; ++i )
         {
            // Outdated comment but still relevant for actual bug related to positional arguments:
            // VS2012 evaluates positional arguments from left to right, so creating Tuple should work correctly
            // This is not so in VS2010 ( see http://msdn.microsoft.com/en-us/library/hh678682.aspx )
            stream.ReadWholeArray( tmpArray ); // tmpArray is 8 bytes long
            sections[i] = new SectionInfo(
               //tmpArray.ReadZeroTerminatedASCIIStringFromBytes(), // Section name
               stream.ReadU32( tmpArray ), // Virtual size
               stream.ReadU32( tmpArray ), // Virtual address
               stream.ReadU32( tmpArray ), // Raw data size
               stream.ReadU32( tmpArray ) // Raw data pointer
               );
            // Skip number of relocation & line numbers, and section characteristics
            stream.SeekFromCurrent( 16 );
         }

         if ( headersWereSet )
         {

            if ( importDD.rva > 0 )
            {
               // Read Import table
               stream.SeekFromBegin( ResolveRVA( importDD.rva, sections ) + 12 );
               var importRVA = stream.ReadU32( tmpArray );
               stream.SeekFromBegin( ResolveRVA( importRVA, sections ) );
               headers.ImportDirectoryName = stream.ReadZeroTerminatedASCIIString();
            }

            if ( iatDD.rva > 0 )
            {
               // Read IAT for hint name
               stream.SeekFromBegin( ResolveRVA( iatDD.rva, sections ) );
               var hnRVA = stream.ReadU32( tmpArray );
               stream.SeekFromBegin( ResolveRVA( hnRVA, sections ) + 2 );
               headers.ImportHintName = stream.ReadZeroTerminatedASCIIString();
            }

            if ( nativeEPRVA > 0 )
            {
               stream.SeekFromBegin( ResolveRVA( nativeEPRVA, sections ) );
               headers.EntryPointInstruction = stream.ReadI16( tmpArray );
            }
         }

         // CLI header, skip magic
         stream.SeekFromBegin( ResolveRVA( cliDD.rva, sections ) + 4 );
         headers.CLIMajor = stream.ReadU16( tmpArray );
         headers.CLIMinor = stream.ReadU16( tmpArray );

         // Metadata datadirectory
         var mdDD = new DataDir( stream, tmpArray );

         // Module flags
         headers.ModuleFlags = (ModuleFlags) stream.ReadU32( tmpArray );

         // Entrypoint token
         headers.CLREntryPointIndex = TableIndex.FromOneBasedTokenNullable( stream.ReadI32( tmpArray ) );

         // Resources data directory
         var rsrcDD = new DataDir( stream, tmpArray );

         // Strong name
         var snDD = new DataDir( stream, tmpArray );

         // Skip code manager table, virtual table fixups, export address table jumps, and managed native header data directories
         //stream.SeekFromCurrent( 32 );

         // Metadata
         stream.SeekFromBegin( ResolveRVA( mdDD.rva, sections ) );
         var retVal = ReadMetadata(
            stream,
            sections,
            rsrcDD,
            rArgs,
            headers
            );

         if ( headersWereSet )
         {
            // Read debug info
            if ( debugDD.rva > 0 )
            {
               stream.SeekFromBegin( ResolveRVA( debugDD.rva, sections ) );
               var dbg = new DebugInformation( false )
               {
                  Characteristics = stream.ReadI32( tmpArray ),
                  Timestamp = stream.ReadI32( tmpArray ),
                  VersionMajor = stream.ReadI16( tmpArray ),
                  VersionMinor = stream.ReadI16( tmpArray ),
                  DebugType = stream.ReadI32( tmpArray )
               };
               var data = new Byte[stream.ReadI32( tmpArray )];
               var dataRVA = stream.ReadU32( tmpArray );
               var dataPtr = stream.ReadU32( tmpArray );
               stream.SeekFromBegin( dataPtr );
               stream.ReadWholeArray( data );
               dbg.DebugData = data;
               headers.DebugInformation = dbg;
            }

            // Read strong name contents
            if ( snDD.rva > 0 )
            {
               stream.SeekFromBegin( ResolveRVA( snDD.rva, sections ) );
               var array = new Byte[snDD.size];
               stream.ReadWholeArray( array );
               rArgs.StrongNameHashValue = array;
            }
         }

         return retVal;
      }

      internal static CILMetaData ReadMetadata(
         Stream stream,
         SectionInfo[] sections,
         DataDir rsrcDD,
         ReadingArguments rArgs,
         HeadersData headers
         )
      {
         var mdRoot = stream.Position;

         // Prepare variables
         var utf8 = MetaDataConstants.SYS_STRING_ENCODING;
         var tmpArray = new Byte[8];

         // Skip signature, major & minor version, and reserved
         stream.SeekFromCurrent( 12 );

         // Read version string
         var versionStrByteLen = stream.ReadU32( tmpArray );
         headers.MetaDataVersion = stream.ReadZeroTerminatedString( versionStrByteLen, utf8 );

         // Skip flags
         stream.SeekFromCurrent( 2 );

         // Amount of streams
         var amountOfStreams = stream.ReadU16( tmpArray );

         // Stream headers
         var streamDic = new Dictionary<String, Tuple<Int64, UInt32>>();
         //var totalRead = 12 // Sig, major & minor version, reserved
         //   + versionStrByteLen // Version string
         //   + 4; // Flags, amount of streams
         for ( var i = 0; i < amountOfStreams; ++i )
         {
            var offset = stream.ReadU32( tmpArray );
            var size = stream.ReadU32( tmpArray );
            //UInt32 streamStringBytesLen;
            streamDic.Add( stream.ReadAlignedASCIIString( 32 ), Tuple.Create( mdRoot + offset, size ) );
            //totalRead += streamStringBytesLen + 8;
         }

         // Read all streams except table stream
         SysStringHeapReader sysStrings = new SysStringHeapReader( tmpArray, stream, streamDic );
         GUIDHeapReader guids = new GUIDHeapReader( tmpArray, stream, streamDic );
         BLOBHeapReader blobs = new BLOBHeapReader( tmpArray, stream, streamDic );
         UserStringHeapReader userStrings = new UserStringHeapReader( tmpArray, stream, streamDic );

         // Read table stream
         stream.SeekFromBegin( streamDic[Consts.TABLE_STREAM_NAME].Item1
            + 4 // Skip reserved
            );
         headers.TableHeapMajor = stream.ReadByteFromStream();
         headers.TableHeapMinor = stream.ReadByteFromStream();

         // Stream index sizes
         var b = stream.ReadByteFromStream();
         if ( ( b & WIDE_SYS_STRING_FLAG ) != 0 )
         {
            sysStrings.IsWideIndex = true;
         }
         if ( ( b & WIDE_GUID_FLAG ) != 0 )
         {
            guids.IsWideIndex = true;
         }
         if ( ( b & WIDE_BLOB_FLAG ) != 0 )
         {
            blobs.IsWideIndex = true;
         }

         stream.SeekFromCurrent( 1 ); // Skip reserved

         // Present tables
         var presentTableMask = stream.ReadU64( tmpArray );

         stream.SeekFromCurrent( 8 ); // Skip sorted

         // Table row count
         var tableSizes = new Int32[Consts.AMOUNT_OF_TABLES];
         for ( var i = 0; i < Consts.AMOUNT_OF_TABLES; ++i )
         {
            if ( ( (UInt32) ( presentTableMask >> i ) ) % 2 == 1 )
            {
               tableSizes[i] = stream.ReadI32( tmpArray );
            }
            //else
            //{
            //   size = 0;
            //}
         }

         // Read actual tables
         var retVal = new CILMetadataImpl( tableSizes );
         var tRefSizes = MetaDataConstants.GetCodedTableIndexSizes( tableSizes );
         var methodDefRVAs = rArgs.MethodRVAs;
         methodDefRVAs.Capacity = tableSizes[(Int32) Tables.MethodDef];

         var fieldDefRVAs = rArgs.FieldRVAs;
         fieldDefRVAs.Capacity = tableSizes[(Int32) Tables.FieldRVA];

         // Try resolve purely local custom attributes and security blobs by creating new resolver but not registering to an event
         //var resolver = new MetaDataResolver();

         for ( var curTable = 0; curTable < Consts.AMOUNT_OF_TABLES; ++curTable )
         {
            switch ( (Tables) curTable )
            {
               // VS2012 evaluates positional arguments from left to right, so creating Tuple inside lambda should work correctly
               // This is not so in VS2010 ( see http://msdn.microsoft.com/en-us/library/hh678682.aspx )
               case Tables.Module:
                  ReadTable( retVal.ModuleDefinitions, tableSizes, i =>
                     new ModuleDefinition()
                     {
                        Generation = stream.ReadI16( tmpArray ),
                        Name = sysStrings.ReadSysString( stream ),
                        ModuleGUID = guids.ReadGUID( stream ),
                        EditAndContinueGUID = guids.ReadGUID( stream ),
                        EditAndContinueBaseGUID = guids.ReadGUID( stream )
                     }
                  );
                  break;
               case Tables.TypeRef:
                  ReadTable( retVal.TypeReferences, tableSizes, i =>
                     new TypeReference()
                     {
                        ResolutionScope = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.ResolutionScope, tRefSizes, tmpArray, false ),
                        Name = sysStrings.ReadSysString( stream ),
                        Namespace = sysStrings.ReadSysString( stream )
                     } );
                  break;
               case Tables.TypeDef:
                  ReadTable( retVal.TypeDefinitions, tableSizes, i =>
                     new TypeDefinition()
                     {
                        Attributes = (TypeAttributes) stream.ReadU32( tmpArray ),
                        Name = sysStrings.ReadSysString( stream ),
                        Namespace = sysStrings.ReadSysString( stream ),
                        BaseType = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.TypeDefOrRef, tRefSizes, tmpArray, false ),
                        FieldList = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.Field, tableSizes, tmpArray ),
                        MethodList = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.MethodDef, tableSizes, tmpArray )
                     } );
                  break;
               case Tables.Field:
                  ReadTable( retVal.FieldDefinitions, tableSizes, i =>
                     new FieldDefinition()
                     {
                        Attributes = (FieldAttributes) stream.ReadU16( tmpArray ),
                        Name = sysStrings.ReadSysString( stream ),
                        Signature = FieldSignature.ReadFromBytes( blobs.WholeBLOBArray, blobs.GetBLOBIndex( stream ) )
                     } );
                  break;
               case Tables.MethodDef:
                  ReadTable( retVal.MethodDefinitions, tableSizes, i =>
                  {
                     methodDefRVAs.Add( stream.ReadI32( tmpArray ) );
                     return new MethodDefinition()
                     {
                        ImplementationAttributes = (MethodImplAttributes) stream.ReadU16( tmpArray ),
                        Attributes = (MethodAttributes) stream.ReadU16( tmpArray ),
                        Name = sysStrings.ReadSysString( stream ),
                        Signature = MethodDefinitionSignature.ReadFromBytes( blobs.WholeBLOBArray, blobs.GetBLOBIndex( stream ) ),
                        ParameterList = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.Parameter, tableSizes, tmpArray )
                     };
                  } );
                  break;
               case Tables.Parameter:
                  ReadTable( retVal.ParameterDefinitions, tableSizes, i =>
                     new ParameterDefinition()
                     {
                        Attributes = (ParameterAttributes) stream.ReadU16( tmpArray ),
                        Sequence = stream.ReadU16( tmpArray ),
                        Name = sysStrings.ReadSysString( stream )
                     } );
                  break;
               case Tables.InterfaceImpl:
                  ReadTable( retVal.InterfaceImplementations, tableSizes, i =>
                     new InterfaceImplementation()
                     {
                        Class = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray ),
                        Interface = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.TypeDefOrRef, tRefSizes, tmpArray ).Value
                     } );
                  break;
               case Tables.MemberRef:
                  ReadTable( retVal.MemberReferences, tableSizes, i =>
                     new MemberReference()
                     {
                        DeclaringType = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.MemberRefParent, tRefSizes, tmpArray ).Value,
                        Name = sysStrings.ReadSysString( stream ),
                        Signature = ReadMemberRefSignature( blobs.WholeBLOBArray, blobs.GetBLOBIndex( stream ) )
                     } );
                  break;
               case Tables.Constant:
                  ReadTable( retVal.ConstantDefinitions, tableSizes, i =>
                  {
                     var constType = (SignatureElementTypes) stream.ReadU16( tmpArray );
                     return new ConstantDefinition()
                     {
                        Type = constType,
                        Parent = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.HasConstant, tRefSizes, tmpArray ).Value,
                        Value = ReadConstantValue( blobs, stream, constType )
                     };
                  } );
                  break;
               case Tables.CustomAttribute:
                  ReadTable( retVal.CustomAttributeDefinitions, tableSizes, i =>
                  {
                     var caDef = new CustomAttributeDefinition()
                     {
                        Parent = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.HasCustomAttribute, tRefSizes, tmpArray ).Value,
                        Type = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.CustomAttributeType, tRefSizes, tmpArray ).Value
                     };
                     Int32 caBlobIndex, caBlobSize;
                     var bArrayIdx = blobs.GetBLOBIndex( stream, out caBlobIndex, out caBlobSize );
                     AbstractCustomAttributeSignature caSig;
                     if ( caBlobSize <= 2 )
                     {
                        // Empty blob
                        caSig = new CustomAttributeSignature();
                     }
                     else
                     {
                        //caSig = resolver.TryResolveCustomAttributeSignature( retVal, blobs.WholeBLOBArray, bArrayIdx, caDef.Type );
                        //if ( caSig == null )
                        //{
                        // Resolving failed
                        caSig = new RawCustomAttributeSignature() { Bytes = blobs.GetBLOB( caBlobIndex ) };
                        //}
                     }
                     caDef.Signature = caSig;
                     return caDef;
                  } );
                  break;
               case Tables.FieldMarshal:
                  ReadTable( retVal.FieldMarshals, tableSizes, i =>
                     new FieldMarshal()
                     {
                        Parent = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.HasFieldMarshal, tRefSizes, tmpArray ).Value,
                        NativeType = MarshalingInfo.ReadFromBytes( blobs.WholeBLOBArray, blobs.GetBLOBIndex( stream ) )
                     } );
                  break;
               case Tables.DeclSecurity:
                  ReadTable( retVal.SecurityDefinitions, tableSizes, i =>
                  {
                     var sec = new SecurityDefinition()
                     {
                        Action = (SecurityAction) stream.ReadI16( tmpArray ),
                        Parent = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.HasDeclSecurity, tRefSizes, tmpArray ).Value
                     };
                     ReadSecurityBLOB( retVal, blobs, stream, sec );
                     return sec;
                  } );
                  break;
               case Tables.ClassLayout:
                  ReadTable( retVal.ClassLayouts, tableSizes, i =>
                     new ClassLayout()
                     {
                        PackingSize = stream.ReadI16( tmpArray ),
                        ClassSize = stream.ReadI32( tmpArray ),
                        Parent = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray )
                     } );
                  break;
               case Tables.FieldLayout:
                  ReadTable( retVal.FieldLayouts, tableSizes, i =>
                     new FieldLayout()
                     {
                        Offset = stream.ReadI32( tmpArray ),
                        Field = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.Field, tableSizes, tmpArray )
                     } );
                  break;
               case Tables.StandaloneSignature:
                  ReadTable( retVal.StandaloneSignatures, tableSizes, i =>
                  {
                     Boolean wasFieldSig;
                     var sig = ReadStandaloneSignature( blobs, stream, out wasFieldSig );
                     return new StandaloneSignature()
                     {
                        Signature = sig,
                        StoreSignatureAsFieldSignature = wasFieldSig
                     };
                  } );
                  break;
               case Tables.EventMap:
                  ReadTable( retVal.EventMaps, tableSizes, i =>
                     new EventMap()
                     {
                        Parent = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray ),
                        EventList = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.Event, tableSizes, tmpArray )
                     } );
                  break;
               case Tables.Event:
                  ReadTable( retVal.EventDefinitions, tableSizes, i =>
                     new EventDefinition()
                     {
                        Attributes = (EventAttributes) stream.ReadU16( tmpArray ),
                        Name = sysStrings.ReadSysString( stream ),
                        EventType = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.TypeDefOrRef, tRefSizes, tmpArray ).Value
                     } );
                  break;
               case Tables.PropertyMap:
                  ReadTable( retVal.PropertyMaps, tableSizes, i =>
                     new PropertyMap()
                     {
                        Parent = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray ),
                        PropertyList = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.Property, tableSizes, tmpArray )
                     } );
                  break;
               case Tables.Property:
                  ReadTable( retVal.PropertyDefinitions, tableSizes, i =>
                     new PropertyDefinition()
                     {
                        Attributes = (PropertyAttributes) stream.ReadU16( tmpArray ),
                        Name = sysStrings.ReadSysString( stream ),
                        Signature = PropertySignature.ReadFromBytes( blobs.WholeBLOBArray, blobs.GetBLOBIndex( stream ) )
                     } );
                  break;
               case Tables.MethodSemantics:
                  ReadTable( retVal.MethodSemantics, tableSizes, i =>
                     new MethodSemantics()
                     {
                        Attributes = (MethodSemanticsAttributes) stream.ReadU16( tmpArray ),
                        Method = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.MethodDef, tableSizes, tmpArray ),
                        Associaton = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.HasSemantics, tRefSizes, tmpArray ).Value
                     } );
                  break;
               case Tables.MethodImpl:
                  ReadTable( retVal.MethodImplementations, tableSizes, i =>
                     new MethodImplementation()
                     {
                        Class = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray ),
                        MethodBody = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.MethodDefOrRef, tRefSizes, tmpArray ).Value,
                        MethodDeclaration = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.MethodDefOrRef, tRefSizes, tmpArray ).Value
                     } );
                  break;
               case Tables.ModuleRef:
                  ReadTable( retVal.ModuleReferences, tableSizes, i =>
                     new ModuleReference()
                     {
                        ModuleName = sysStrings.ReadSysString( stream )
                     } );
                  break;
               case Tables.TypeSpec:
                  ReadTable( retVal.TypeSpecifications, tableSizes, i =>
                     new TypeSpecification()
                     {
                        Signature = TypeSignature.ReadFromBytes( blobs.WholeBLOBArray, blobs.GetBLOBIndex( stream ) )
                     } );
                  break;
               case Tables.ImplMap:
                  ReadTable( retVal.MethodImplementationMaps, tableSizes, i =>
                     new MethodImplementationMap()
                     {
                        Attributes = (PInvokeAttributes) stream.ReadU16( tmpArray ),
                        MemberForwarded = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.MemberForwarded, tRefSizes, tmpArray ).Value,
                        ImportName = sysStrings.ReadSysString( stream ),
                        ImportScope = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.ModuleRef, tableSizes, tmpArray )
                     } );
                  break;
               case Tables.FieldRVA:
                  ReadTable( retVal.FieldRVAs, tableSizes, i =>
                  {
                     fieldDefRVAs.Add( stream.ReadI32( tmpArray ) );
                     return new FieldRVA()
                     {
                        Field = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.Field, tableSizes, tmpArray )
                     };
                  } );
                  break;
               case Tables.Assembly:
                  ReadTable( retVal.AssemblyDefinitions, tableSizes, i =>
                  {
                     var assDef = new AssemblyDefinition()
                     {
                        HashAlgorithm = (AssemblyHashAlgorithm) stream.ReadU32( tmpArray )
                     };
                     var assInfo = assDef.AssemblyInformation;
                     assInfo.VersionMajor = stream.ReadU16( tmpArray );
                     assInfo.VersionMinor = stream.ReadU16( tmpArray );
                     assInfo.VersionBuild = stream.ReadU16( tmpArray );
                     assInfo.VersionRevision = stream.ReadU16( tmpArray );
                     assDef.Attributes = (AssemblyFlags) stream.ReadU32( tmpArray );
                     assInfo.PublicKeyOrToken = blobs.ReadBLOB( stream );
                     assInfo.Name = sysStrings.ReadSysString( stream );
                     assInfo.Culture = sysStrings.ReadSysString( stream );
                     return assDef;
                  } );
                  break;
               case Tables.AssemblyRef:
                  ReadTable( retVal.AssemblyReferences, tableSizes, i =>
                  {
                     var assRef = new AssemblyReference();
                     var assInfo = assRef.AssemblyInformation;
                     assInfo.VersionMajor = stream.ReadU16( tmpArray );
                     assInfo.VersionMinor = stream.ReadU16( tmpArray );
                     assInfo.VersionBuild = stream.ReadU16( tmpArray );
                     assInfo.VersionRevision = stream.ReadU16( tmpArray );
                     assRef.Attributes = (AssemblyFlags) stream.ReadI32( tmpArray );
                     assInfo.PublicKeyOrToken = blobs.ReadBLOB( stream );
                     assInfo.Name = sysStrings.ReadSysString( stream );
                     assInfo.Culture = sysStrings.ReadSysString( stream );
                     assRef.HashValue = blobs.ReadBLOB( stream );
                     return assRef;
                  } );
                  break;
               case Tables.File:
                  ReadTable( retVal.FileReferences, tableSizes, i =>
                     new FileReference()
                     {
                        Attributes = (FileAttributes) stream.ReadU32( tmpArray ),
                        Name = sysStrings.ReadSysString( stream ),
                        HashValue = blobs.ReadBLOB( stream )
                     } );
                  break;
               case Tables.ExportedType:
                  ReadTable( retVal.ExportedTypes, tableSizes, i =>
                     new ExportedType()
                     {
                        Attributes = (TypeAttributes) stream.ReadU32( tmpArray ),
                        TypeDefinitionIndex = stream.ReadI32( tmpArray ),
                        Name = sysStrings.ReadSysString( stream ),
                        Namespace = sysStrings.ReadSysString( stream ),
                        Implementation = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.Implementation, tRefSizes, tmpArray ).Value
                     } );
                  break;
               case Tables.ManifestResource:
                  ReadTable( retVal.ManifestResources, tableSizes, i =>
                     new ManifestResource()
                     {
                        Offset = (Int32) stream.ReadU32( tmpArray ),
                        Attributes = (ManifestResourceAttributes) stream.ReadU32( tmpArray ),
                        Name = sysStrings.ReadSysString( stream ),
                        Implementation = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.Implementation, tRefSizes, tmpArray, false )
                     } );
                  break;
               case Tables.NestedClass:
                  ReadTable( retVal.NestedClassDefinitions, tableSizes, i =>
                     new NestedClassDefinition()
                     {
                        NestedClass = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray ),
                        EnclosingClass = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray )
                     } );
                  break;
               case Tables.GenericParameter:
                  ReadTable( retVal.GenericParameterDefinitions, tableSizes, i =>
                     new GenericParameterDefinition()
                     {
                        GenericParameterIndex = stream.ReadI16( tmpArray ),
                        Attributes = (GenericParameterAttributes) stream.ReadU16( tmpArray ),
                        Owner = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.TypeOrMethodDef, tRefSizes, tmpArray ).Value,
                        Name = sysStrings.ReadSysString( stream )
                     } );
                  break;
               case Tables.MethodSpec:
                  ReadTable( retVal.MethodSpecifications, tableSizes, i =>
                     new MethodSpecification()
                     {
                        Method = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.MethodDefOrRef, tRefSizes, tmpArray ).Value,
                        Signature = GenericMethodSignature.ReadFromBytes( blobs.WholeBLOBArray, blobs.GetBLOBIndex( stream ) )
                     } );
                  break;
               case Tables.GenericParameterConstraint:
                  ReadTable( retVal.GenericParameterConstraintDefinitions, tableSizes, i =>
                     new GenericParameterConstraintDefinition()
                     {
                        Owner = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.GenericParameter, tableSizes, tmpArray ),
                        Constraint = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.TypeDefOrRef, tRefSizes, tmpArray ).Value
                     } );
                  break;
               case Tables.FieldPtr:
               case Tables.MethodPtr:
               case Tables.ParameterPtr:
               case Tables.EventPtr:
               case Tables.PropertyPtr:
               case Tables.EncLog:
               case Tables.EncMap:
               case Tables.AssemblyProcessor:
               case Tables.AssemblyOS:
               case Tables.AssemblyRefProcessor:
               case Tables.AssemblyRefOS:
                  // Skip (TODO skip stream too... but would need to know table row size for that)
                  break;
               default:
                  throw new BadImageFormatException( "Unknown table: " + curTable );
            }
         }

         // Read all IL code
         for ( var i = 0; i < methodDefRVAs.Count; ++i )
         {
            var rva = (UInt32) methodDefRVAs[i];
            if ( rva != 0 )
            {
               var offset = ResolveRVA( rva, sections );
               if ( offset < stream.Length )
               {
                  stream.SeekFromBegin( offset );
                  retVal.MethodDefinitions.TableContents[i].IL = ReadMethodILDefinition( stream, userStrings );
               }
            }
         }

         // Read all field RVA content
         var layoutInfo = new Lazy<IDictionary<Int32, ClassLayout>>( () =>
         {
            var dic = new Dictionary<Int32, ClassLayout>();
            foreach ( var layout in retVal.ClassLayouts.TableContents )
            {
               dic[layout.Parent.Index] = layout;
            }
            return dic;
         }, System.Threading.LazyThreadSafetyMode.None );
         for ( var i = 0; i < fieldDefRVAs.Count; ++i )
         {
            var fRVA = retVal.FieldRVAs.TableContents[i];
            var offset = ResolveRVA( (UInt32) fieldDefRVAs[i], sections );
            UInt32 size;
            if (
               TryCalculateFieldTypeSize( retVal, layoutInfo, fRVA.Field.Index, out size )
               && offset + size < stream.Length
               )
            {
               // Sometimes there are field RVAs that are unresolvable...
               var bytes = new Byte[size];
               stream.SeekFromBegin( offset );
               stream.ReadWholeArray( bytes );
               fRVA.Data = bytes;
            }

         }

         // Read all raw manifest resources
         var hasEmbeddedResources = rsrcDD.rva > 0 && rsrcDD.size > 0;
         if ( hasEmbeddedResources )
         {
            var rsrcOffset = ResolveRVA( rsrcDD.rva, sections );
            var rsrcSize = rsrcDD.size;
            var rsrcOffsets = rArgs.EmbeddedManifestResourceOffsets;
            foreach ( var mRes in retVal.ManifestResources.TableContents )
            {
               Int32? offsetToAdd;
               if ( mRes.IsEmbeddedResource() && (UInt32) mRes.Offset < rsrcSize )
               {
                  // Read embedded resource
                  offsetToAdd = mRes.Offset;
                  stream.SeekFromBegin( rsrcOffset + (UInt32) offsetToAdd );
                  var length = stream.ReadU32( tmpArray );
                  var data = new Byte[length];
                  stream.ReadWholeArray( data );
                  mRes.DataInCurrentFile = data;
               }
               else
               {
                  offsetToAdd = null;
               }
               rsrcOffsets.Add( offsetToAdd );
            }
         }

         return retVal;
      }

      private static void ReadTable<T>( MetaDataTable<T> tableArray, Int32[] tableSizes, Func<Int32, T> rowReader )
         where T : class
      {
         // TODO - calculate table width, thus reading whole table into single array
         // Then give array as argument to rowReader
         // However, stream buffers things really well - is this really necessary? Not sure if performance boost will be worth it, and probably not good thing memory-wise if big tables are present.
         var len = tableSizes[(Int32) tableArray.TableKind];
         var list = tableArray.TableContents;

         for ( var i = 0; i < len; ++i )
         {
            list.Add( rowReader( i ) );
         }
      }


      private static Int64 ResolveRVA( Int64 rva, SectionInfo[] sections )
      {
         for ( var i = 0; i < sections.Length; ++i )
         {
            var sec = sections[i];
            if ( sec.virtualAddress <= rva && rva < (Int64) ( sec.virtualAddress + sec.virtualSize ) )
            {
               return (Int64) sec.rawPointer + ( rva - sec.virtualAddress );
            }
         }
         throw new ArgumentException( "Could not resolve RVA " + rva + "." );
      }

      private static Boolean TryCalculateFieldTypeSize( CILMetaData md, Lazy<IDictionary<Int32, ClassLayout>> classLayoutInfo, Int32 fieldIdx, out UInt32 size, Boolean onlySimpleTypeValid = false )
      {
         var fDef = md.FieldDefinitions.TableContents;
         var retVal = fieldIdx < fDef.Count;
         size = 0u;
         if ( retVal )
         {
            var fieldSig = fDef[fieldIdx].Signature;
            var type = fieldSig.Type;
            retVal = false;
            switch ( type.TypeSignatureKind )
            {
               case TypeSignatureKind.Simple:
                  retVal = true;
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
                     default:
                        retVal = false;
                        break;
                  }
                  break;
               case TypeSignatureKind.ClassOrValue:
                  retVal = !onlySimpleTypeValid;
                  if ( retVal )
                  {
                     var c = (ClassOrValueTypeSignature) type;

                     var typeIdx = c.Type;
                     retVal = typeIdx.Table == Tables.TypeDef;
                     if ( retVal )
                     {
                        // Only possible for types defined in this module
                        var enumValueFieldIndex = GetEnumValueFieldIndex( md, typeIdx.Index );
                        if ( enumValueFieldIndex >= 0 )
                        {
                           retVal = TryCalculateFieldTypeSize( md, classLayoutInfo, enumValueFieldIndex, out size, true ); // Last parameter true to prevent possible infinite recursion in case of malformed metadata
                        }
                        else
                        {
                           ClassLayout layout;
                           if ( classLayoutInfo.Value.TryGetValue( typeIdx.Index, out layout ) )
                           {
                              size = (UInt32) layout.ClassSize;
                           }
                        }

                     }
                  }
                  break;
            }
         }
         return retVal;
      }

      internal static Int32 GetEnumValueFieldIndex(
         CILMetaData md,
         Int32 tDefIndex
         )
      {
         // Only possible for types defined in this module
         var typeRow = md.TypeDefinitions.GetOrNull( tDefIndex );
         var retVal = -1;
         if ( typeRow != null )
         {
            var extendInfo = typeRow.BaseType;
            if ( extendInfo.HasValue )
            {
               var isEnum = IsEnum( md, extendInfo );
               if ( isEnum )
               {
                  // First non-static field of enum type is the field containing enum value
                  var fieldStartIdx = typeRow.FieldList.Index;
                  var tDefs = md.TypeDefinitions.TableContents;
                  var fDefs = md.FieldDefinitions.TableContents;
                  var fieldEndIdx = tDefIndex + 1 >= tDefs.Count ?
                     fDefs.Count :
                     tDefs[tDefIndex + 1].FieldList.Index;
                  for ( var i = fieldStartIdx; i < fieldEndIdx; ++i )
                  {
                     if ( !fDefs[i].Attributes.IsStatic() )
                     {
                        // We have found non-static field of the enum type -> this field should be primitive and the size thus calculable
                        retVal = i;
                        break;
                     }
                  }
               }
            }
         }

         return retVal;
      }

      private static Boolean IsEnum( CILMetaData md, TableIndex? tIdx )
      {
         return IsSystemType( md, tIdx, Consts.ENUM_NAMESPACE, Consts.ENUM_TYPENAME );
      }

      internal static Boolean IsSystemType( CILMetaData md, TableIndex? tIdx, String systemNS, String systemTN )
      {
         var result = tIdx.HasValue && tIdx.Value.Table != Tables.TypeSpec;

         if ( result )
         {
            var tIdxValue = tIdx.Value;
            var table = tIdxValue.Table;
            var idx = tIdxValue.Index;

            String tn = null, ns = null;
            if ( table == Tables.TypeDef )
            {
               var tDefs = md.TypeDefinitions.TableContents;
               result = idx < tDefs.Count;
               if ( result )
               {
                  tn = tDefs[idx].Name;
                  ns = tDefs[idx].Namespace;
               }
            }
            else if ( table == Tables.TypeRef )
            {
               var tRef = md.TypeReferences.GetOrNull( idx );
               result = tRef != null
                  && tRef.ResolutionScope.HasValue
                  && tRef.ResolutionScope.Value.Table == Tables.AssemblyRef; // TODO check for 'mscorlib', except that sometimes it may be System.Runtime ...
               if ( result )
               {
                  tn = tRef.Name;
                  ns = tRef.Namespace;
               }
            }
            if ( result )
            {
               result = String.Equals( tn, systemTN ) && String.Equals( ns, systemNS );
            }
         }
         return result;
      }

      private static Object ReadConstantValue( BLOBHeapReader blobContainer, Stream stream, SignatureElementTypes constType )
      {
         var blob = blobContainer.WholeBLOBArray;
         Int32 blobSize;
         var idx = blobContainer.GetBLOBIndex( stream, out blobSize );
         switch ( constType )
         {
            case SignatureElementTypes.Boolean:
               return blob.ReadByteFromBytes( ref idx ) == 1;
            case SignatureElementTypes.Char:
               return Convert.ToChar( blob.ReadUInt16LEFromBytes( ref idx ) );
            case SignatureElementTypes.I1:
               return blob.ReadSByteFromBytes( ref idx );
            case SignatureElementTypes.U1:
               return blob.ReadByteFromBytes( ref idx );
            case SignatureElementTypes.I2:
               return blob.ReadInt16LEFromBytes( ref idx );
            case SignatureElementTypes.U2:
               return blob.ReadUInt16LEFromBytes( ref idx );
            case SignatureElementTypes.I4:
               return blob.ReadInt32LEFromBytes( ref idx );
            case SignatureElementTypes.U4:
               return blob.ReadUInt32LEFromBytes( ref idx );
            case SignatureElementTypes.I8:
               return blob.ReadInt64LEFromBytes( ref idx );
            case SignatureElementTypes.U8:
               return blob.ReadUInt64LEFromBytes( ref idx );
            case SignatureElementTypes.R4:
               return blob.ReadSingleLEFromBytes( ref idx );
            case SignatureElementTypes.R8:
               return blob.ReadDoubleLEFromBytes( ref idx );
            case SignatureElementTypes.String:
               return MetaDataConstants.USER_STRING_ENCODING.GetString( blob, idx, blobSize );
            default:
               //idx = blob.ReadInt32LEFromBytesNoRef( 0 );
               //if ( idx != 0 )
               //{
               //   throw new BadImageFormatException( "Other const types than primitives should be serialized as zero int32's." );
               //}
               return null;
         }
      }

      [Flags]
      internal enum MethodHeaderFlags
      {
         TinyFormat = 0x2,
         FatFormat = 0x3,
         MoreSections = 0x8,
         InitLocals = 0x10
      }

      [Flags]
      internal enum MethodDataFlags
      {
         ExceptionHandling = 0x1,
         OptimizeILTable = 0x2,
         FatFormat = 0x40,
         MoreSections = 0x80
      }

      internal static MethodILDefinition ReadMethodILDefinition( System.IO.Stream stream, UserStringHeapReader userStrings )
      {
         var FORMAT_MASK = 0x00000001;
         var FLAG_MASK = 0x00000FFF;
         var SEC_SIZE_MASK = 0xFFFFFF00u;
         var SEC_FLAG_MASK = 0x000000FFu;


         var b = (Int32) stream.ReadByteFromStream();
         var retVal = new MethodILDefinition();
         var tmpArray = new Byte[8];
         if ( ( FORMAT_MASK & b ) == 0 )
         {
            // Tiny header - no locals, no exceptions, no extra data
            CreateOpCodes( retVal, stream, b >> 2, tmpArray, userStrings );
            // Max stack is 8
            retVal.MaxStackSize = 8;
            retVal.InitLocals = false;
         }
         else
         {
            stream.SeekFromCurrent( -1 );

            b = stream.ReadU16( tmpArray );
            var flags = (MethodHeaderFlags) ( b & FLAG_MASK );
            retVal.InitLocals = ( flags & MethodHeaderFlags.InitLocals ) != 0;
            var headerSize = ( b >> 12 ) * 4; // Header size is written as amount of integers
            // Read max stack
            retVal.MaxStackSize = stream.ReadU16( tmpArray );
            var codeSize = stream.ReadI32( tmpArray );
            retVal.LocalsSignatureIndex = TableIndex.FromOneBasedTokenNullable( stream.ReadI32( tmpArray ) );

            if ( headerSize != 12 )
            {
               stream.SeekFromCurrent( BitUtils.MultipleOf4( headerSize - 12 ) );
            }

            // Read code
            CreateOpCodes( retVal, stream, codeSize, tmpArray, userStrings );

            stream.SeekFromCurrent( BitUtils.MultipleOf4( codeSize ) - codeSize );

            var excList = new List<MethodExceptionBlock>();
            if ( ( flags & MethodHeaderFlags.MoreSections ) != 0 )
            {
               // Read sections
               MethodDataFlags secFlags;
               do
               {
                  var secHeader = stream.ReadU32( tmpArray );
                  secFlags = (MethodDataFlags) ( secHeader & SEC_FLAG_MASK );
                  var secByteSize = ( secHeader & SEC_SIZE_MASK ) >> 8;
                  secByteSize -= 4;
                  var isFat = ( secFlags & MethodDataFlags.FatFormat ) != 0;
                  while ( secByteSize > 0 )
                  {
                     var eType = (ExceptionBlockType) ( isFat ? stream.ReadU32( tmpArray ) : stream.ReadU16( tmpArray ) );
                     retVal.ExceptionBlocks.Add( new MethodExceptionBlock()
                     {
                        BlockType = eType,
                        TryOffset = isFat ? stream.ReadI32( tmpArray ) : stream.ReadU16( tmpArray ),
                        TryLength = isFat ? stream.ReadI32( tmpArray ) : stream.ReadByteFromStream(),
                        HandlerOffset = isFat ? stream.ReadI32( tmpArray ) : stream.ReadU16( tmpArray ),
                        HandlerLength = isFat ? stream.ReadI32( tmpArray ) : stream.ReadByteFromStream(),
                        ExceptionType = eType == ExceptionBlockType.Filter ? (TableIndex?) null : TableIndex.FromOneBasedTokenNullable( stream.ReadI32( tmpArray ) ),
                        FilterOffset = eType == ExceptionBlockType.Filter ? stream.ReadI32( tmpArray ) : 0
                     } );
                     secByteSize -= ( isFat ? 24u : 12u );
                  }
               } while ( ( secFlags & MethodDataFlags.MoreSections ) != 0 );
            }
         }

         return retVal;
      }

      private static void CreateOpCodes( MethodILDefinition methodIL, Stream stream, Int32 codeSize, Byte[] tmpArray, UserStringHeapReader userStrings )
      {
         var current = 0;
         var opCodes = methodIL.OpCodes;
         while ( current < codeSize )
         {
            Int32 bytesRead;
            opCodes.Add( OpCodeInfo.ReadFromStream(
               stream,
               tmpArray,
               strToken => userStrings.GetString( TableIndex.FromZeroBasedToken( strToken ).Index ),
               out bytesRead )
               );
            current += bytesRead;
         }
      }

      private static AbstractSignature ReadMemberRefSignature( Byte[] bytes, Int32 idx )
      {
         return (SignatureStarters) bytes[idx] == SignatureStarters.Field ?
            (AbstractSignature) FieldSignature.ReadFromBytesWithRef( bytes, ref idx ) :
            MethodReferenceSignature.ReadFromBytes( bytes, ref idx );
      }

      private static AbstractSignature ReadStandaloneSignature( BLOBHeapReader blob, Stream stream, out Boolean wasFieldSig )
      {
         Int32 heapIndex, blobSize;
         var idx = blob.GetBLOBIndex( stream, out heapIndex, out blobSize );
         var bytes = blob.WholeBLOBArray;

         var sigStarter = (SignatureStarters) bytes[idx];
         wasFieldSig = sigStarter == SignatureStarters.Field;
         AbstractSignature retVal;
         if ( sigStarter == SignatureStarters.LocalSignature )
         {
            retVal = LocalVariablesSignature.ReadFromBytes( bytes, ref idx );
         }
         else if ( wasFieldSig )
         {
            // Read as local signature instead of field signature, since we might encounter pinned etc stuff
            ++idx;
            retVal = new LocalVariablesSignature( 1 );
            ( (LocalVariablesSignature) retVal ).Locals.Add( LocalVariableSignature.ReadFromBytes( bytes, ref idx ) );
         }
         else
         {
            // ??
            retVal = new RawSignature() { Bytes = blob.GetBLOB( heapIndex ) };
         }

         return retVal;

         //return (SignatureStarters) bytes[idx] == SignatureStarters.LocalSignature ?
         //   (AbstractSignature) LocalVariablesSignature.ReadFromBytes( bytes, ref idx ) :
         //   ( (SignatureStarters) bytes[idx] == SignatureStarters.Field ?
         //      (AbstractSignature)   new RawSignature() { Bytes = blob.GetBLOB( heapIndex ) } : // We could parse field signature but it sometimes may contain stuff like Pinned etc, which would just mess it up
         //      MethodReferenceSignature.ReadFromBytes( bytes, ref idx ) );
      }

      private static void ReadSecurityBLOB(
         CILMetaData md,
         BLOBHeapReader blobs,
         Stream stream,
         SecurityDefinition declSecurity
         )
      {
         Int32 blobSize;
         var bIdx = blobs.GetBLOBIndex( stream, out blobSize );
         if ( blobSize > 0 )
         {
            var blob = blobs.WholeBLOBArray;

            if ( blob[bIdx] == MetaDataConstants.DECL_SECURITY_HEADER )
            {
               // New (.NET 2.0+) security spec
               ++bIdx;
               // Amount of security attributes
               var attrCount = blob.DecompressUInt32( ref bIdx );
               for ( var j = 0; j < attrCount; ++j )
               {
                  var secType = blob.ReadLenPrefixedUTF8String( ref bIdx );
                  // There is an amount of remaining bytes here
                  var attributeByteCount = blob.DecompressUInt32( ref bIdx );
                  var copyStart = bIdx;
                  // Now, amount of named args
                  var argCount = blob.DecompressUInt32( ref bIdx );
                  var bytesToCopy = attributeByteCount - ( bIdx - copyStart );
                  AbstractSecurityInformation secInfo;
                  if ( argCount <= 0 )
                  {
                     secInfo = new SecurityInformation()
                     {
                        SecurityAttributeType = secType
                     };
                     bIdx += bytesToCopy;
                  }
                  else
                  {
                     secInfo = new RawSecurityInformation()
                     {
                        SecurityAttributeType = secType,
                        ArgumentCount = argCount,
                        Bytes = blob.CreateAndBlockCopyTo( ref bIdx, bytesToCopy )
                     };
                  }
                  declSecurity.PermissionSets.Add( secInfo );
               }
            }
            else
            {
               // Old (.NET 1.x) security spec
               // Create a single SecurityInformation with PermissionSetAttribute type and XML property argument containing the XML of the blob
               var secInfo = new SecurityInformation( 1 )
               {
                  SecurityAttributeType = Consts.PERMISSION_SET
               };
               secInfo.NamedArguments.Add( new CustomAttributeNamedArgument()
               {
                  IsField = false,
                  Name = Consts.PERMISSION_SET_XML_PROP,
                  Value = new CustomAttributeTypedArgument()
                  {
                     Type = CustomAttributeArgumentTypeSimple.String,
                     Value = MetaDataConstants.USER_STRING_ENCODING.GetString( blob, bIdx, blobSize )
                  }
               } );
               declSecurity.PermissionSets.Add( secInfo );
            }
         }
      }
   }
}
