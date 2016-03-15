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
      private const UInt16 SYM_GLOBAL_MANAGED_FUNC = 0x112a;
      private const UInt16 SYM_LOCAL_MANAGED_FUNC = 0x112b;
      private const UInt16 SYM_MANAGED_SLOT = 0x1120;
      private const UInt16 SYM_MANAGED_CONSTANT = 0x112d;
      private const UInt16 SYM_USED_NS = 0x1124;
      private const UInt16 SYM_SCOPE = 0x1103;
      private const UInt16 SYM_END = 0x0006;
      private const UInt16 SYM_OEM = 0x0404;
      private const Int32 SYM_DEBUG_SOURCE_INFO = 0xF4;
      private const Int32 SYM_DEBUG_LINE_INFO = 0xF2;

      private const Byte MD2_USED_NAMESPACES = 0;
      private const Byte MD2_FORWARDING_METHOD_TOKEN = 1;
      private const Byte MD2_FORWARDING_MODULE_METHOD_TOKEN = 2;
      private const Byte MD2_LOCAL_SCOPES = 3;
      private const Byte MD2_ITERATOR_CLASS = 4;
      private const Byte MD2_YET_UNKNOWN = 6;
      private const Byte MD2_YET_UNKNOWN_2 = 7;

      private const Int32 LINE_MULTIPLIER = 8; // Offset (int32), flags (uint32)
      private const Int32 COLUMN_MULTIPLIER = 4; // Column start (uint16), column end (uint16)

      private const Int32 INT_SIZE = sizeof( Int32 );
      private const Int32 SHORT_SIZE = sizeof( Int16 );

      private const Int32 EC_INFO_SIZE = 0x19;

      internal const String SOURCE_FILE_PREFIX = "/src/files/";
      private const String NAMES_STREAM_NAME = "/names";

      private class StreamInfo
      {
         internal Int64 begin;
         internal Stream stream;

         internal StreamInfo( Stream stream )
         {
            ArgumentValidator.ValidateNotNull( "Stream", stream );

            this.stream = stream;
            this.begin = stream.Position;
         }
      }

      private sealed class DBIModuleInfo
      {
         internal Int32 opened;
         internal DBISecCon section;
         internal UInt16 flags;
         internal UInt16 stream;
         internal Int32 symbolByteCount;
         internal Int32 oldLinesByteCount;
         internal Int32 linesByteCount;
         internal UInt16 files;
         internal UInt32 offsets;
         internal Int32 sourceIdx;
         internal Int32 compilerIdx;
         internal String moduleName;
         internal String objectName;

         internal DBIModuleInfo( String modName )
         {
            this.moduleName = modName ?? String.Empty;
            this.section = new DBISecCon();
         }

         internal DBIModuleInfo( Byte[] array, ref Int32 idx, Encoding encoding )
         {
            this.opened = array.ReadInt32LEFromBytes( ref idx );
            this.section = new DBISecCon( array, ref idx );
            this.flags = array.ReadUInt16LEFromBytes( ref idx );
            this.stream = array.ReadUInt16LEFromBytes( ref idx );
            this.symbolByteCount = array.ReadInt32LEFromBytes( ref idx );
            this.oldLinesByteCount = array.ReadInt32LEFromBytes( ref idx );
            this.linesByteCount = array.ReadInt32LEFromBytes( ref idx );
            this.files = array.ReadUInt16LEFromBytes( ref idx );
            idx += 2;
            this.offsets = array.ReadUInt32LEFromBytes( ref idx );
            this.sourceIdx = array.ReadInt32LEFromBytes( ref idx );
            this.compilerIdx = array.ReadInt32LEFromBytes( ref idx );
            this.moduleName = array.ReadZeroTerminatedStringFromBytes( ref idx, encoding );
            this.objectName = array.ReadZeroTerminatedStringFromBytes( ref idx, encoding );
         }

         internal void WriteToArray( Byte[] array, ref Int32 idx )
         {
            array
               .WriteInt32LEToBytes( ref idx, this.opened );
            this.section.WriteToArray( array, ref idx );
            array
               .WriteUInt16LEToBytes( ref idx, this.flags )
               .WriteUInt16LEToBytes( ref idx, this.stream )
               .WriteInt32LEToBytes( ref idx, this.symbolByteCount )
               .WriteInt32LEToBytes( ref idx, this.oldLinesByteCount )
               .WriteInt32LEToBytes( ref idx, this.linesByteCount )
               .WriteUInt16LEToBytes( ref idx, this.files )
               .WriteInt16LEToBytes( ref idx, 0 ) // Pad
               .WriteUInt32LEToBytes( ref idx, this.offsets )
               .WriteInt32LEToBytes( ref idx, this.sourceIdx )
               .WriteInt32LEToBytes( ref idx, this.compilerIdx )
               .WriteZeroTerminatedString( ref idx, this.moduleName )
               .WriteZeroTerminatedString( ref idx, this.objectName ?? String.Empty )
               .Align4( ref idx );
         }

#if DEBUG
         public override String ToString()
         {
            return this.moduleName + " (" + this.objectName + ")";
         }
#endif

      }

      private sealed class DBISecCon
      {
         internal UInt16 section;
         internal UInt32 offset;
         internal UInt32 size;
         internal UInt32 flags;
         internal UInt16 module;
         internal UInt32 dataCRC;
         internal UInt32 relocCRC;

         internal DBISecCon()
         {
            // Initialize default values (for DBI module info)
            this.module = UInt16.MaxValue;
            this.section = UInt16.MaxValue;
            this.size = UInt32.MaxValue;
         }

         internal DBISecCon( Byte[] array, ref Int32 idx )
         {
            this.section = array.ReadUInt16LEFromBytes( ref idx );
            idx += 2;
            this.offset = array.ReadUInt32LEFromBytes( ref idx );
            this.size = array.ReadUInt32LEFromBytes( ref idx );
            this.flags = array.ReadUInt32LEFromBytes( ref idx );
            this.module = array.ReadUInt16LEFromBytes( ref idx );
            idx += 2;
            this.dataCRC = array.ReadUInt32LEFromBytes( ref idx );
            this.relocCRC = array.ReadUInt32LEFromBytes( ref idx );
         }

         internal void WriteToArray( Byte[] array, ref Int32 idx )
         {
            array
               .WriteUInt16LEToBytes( ref idx, this.section )
               .WriteInt16LEToBytes( ref idx, 0 ) // Pad
               .WriteUInt32LEToBytes( ref idx, this.offset )
               .WriteUInt32LEToBytes( ref idx, this.size )
               .WriteUInt32LEToBytes( ref idx, this.flags )
               .WriteUInt16LEToBytes( ref idx, this.module )
               .WriteInt16LEToBytes( ref idx, 0 ) // Pad
               .WriteUInt32LEToBytes( ref idx, this.dataCRC )
               .WriteUInt32LEToBytes( ref idx, this.relocCRC );
         }
      }

      private sealed class DBIHeader
      {
         internal Int32 signature;
         internal Int32 version;
         internal UInt32 age;
         internal UInt16 gsSymStream;
         internal UInt16 gsSymStreamVersion;
         internal UInt16 psSymStream;
         internal UInt16 psSymStreamVersion;
         internal UInt16 symRecStream;
         internal UInt16 symRecStreamVersion;
         internal Int32 moduleInfoSize;
         internal Int32 secConSize;
         internal Int32 secMapSize;
         internal Int32 fileInfoSize;
         internal Int32 tsMapSize;
         internal Int32 mfcIndex;
         internal Int32 debugHeaderSize;
         internal Int32 ecInfoSize;
         internal UInt16 flags;
         internal UInt16 machine;
         internal Int32 reserved;

         internal DBIHeader()
         {
            // Set versions and other stuff to their defaults
            this.version = 0x01310977;
            this.gsSymStreamVersion = 0x8B00;
            this.psSymStreamVersion = 0xC6FA;
            this.symRecStreamVersion = 0x47E8;
            this.ecInfoSize = EC_INFO_SIZE;
            this.signature = -1;
            this.machine = 0xC0EE; // Pure IL assembly.
         }

         internal DBIHeader( Byte[] array, ref Int32 idx )
         {
            this.signature = array.ReadInt32LEFromBytes( ref idx );
            this.version = array.ReadInt32LEFromBytes( ref idx );
            this.age = array.ReadUInt32LEFromBytes( ref idx );
            this.gsSymStream = array.ReadUInt16LEFromBytes( ref idx );
            this.gsSymStreamVersion = array.ReadUInt16LEFromBytes( ref idx );
            this.psSymStream = array.ReadUInt16LEFromBytes( ref idx );
            this.psSymStreamVersion = array.ReadUInt16LEFromBytes( ref idx );
            this.symRecStream = array.ReadUInt16LEFromBytes( ref idx );
            this.symRecStreamVersion = array.ReadUInt16LEFromBytes( ref idx );
            this.moduleInfoSize = array.ReadInt32LEFromBytes( ref idx );
            this.secConSize = array.ReadInt32LEFromBytes( ref idx );
            this.secMapSize = array.ReadInt32LEFromBytes( ref idx );
            this.fileInfoSize = array.ReadInt32LEFromBytes( ref idx );
            this.tsMapSize = array.ReadInt32LEFromBytes( ref idx );
            this.mfcIndex = array.ReadInt32LEFromBytes( ref idx );
            this.debugHeaderSize = array.ReadInt32LEFromBytes( ref idx );
            this.ecInfoSize = array.ReadInt32LEFromBytes( ref idx );
            this.flags = array.ReadUInt16LEFromBytes( ref idx );
            this.machine = array.ReadUInt16LEFromBytes( ref idx );
            this.reserved = array.ReadInt32LEFromBytes( ref idx );
         }

         internal void WriteDBIHeader( Byte[] array, ref Int32 idx )
         {
            array
               .WriteInt32LEToBytes( ref idx, this.signature )
               .WriteInt32LEToBytes( ref idx, this.version )
               .WriteUInt32LEToBytes( ref idx, this.age )
               .WriteUInt16LEToBytes( ref idx, this.gsSymStream )
               .WriteUInt16LEToBytes( ref idx, this.gsSymStreamVersion )
               .WriteUInt16LEToBytes( ref idx, this.psSymStream )
               .WriteUInt16LEToBytes( ref idx, this.psSymStreamVersion )
               .WriteUInt16LEToBytes( ref idx, this.symRecStream )
               .WriteUInt16LEToBytes( ref idx, this.symRecStreamVersion )
               .WriteInt32LEToBytes( ref idx, this.moduleInfoSize )
               .WriteInt32LEToBytes( ref idx, this.secConSize )
               .WriteInt32LEToBytes( ref idx, this.secMapSize )
               .WriteInt32LEToBytes( ref idx, this.fileInfoSize )
               .WriteInt32LEToBytes( ref idx, this.tsMapSize )
               .WriteInt32LEToBytes( ref idx, this.mfcIndex )
               .WriteInt32LEToBytes( ref idx, this.debugHeaderSize )
               .WriteInt32LEToBytes( ref idx, this.ecInfoSize )
               .WriteUInt16LEToBytes( ref idx, this.flags )
               .WriteUInt16LEToBytes( ref idx, this.machine )
               .WriteInt32LEToBytes( ref idx, this.reserved );
         }
      }

      private sealed class DBIDebugHeader
      {
         internal UInt16 snFPO;
         internal UInt16 snException;
         internal UInt16 snFixup;
         internal UInt16 snOmapToSrc;
         internal UInt16 snOmapFromSrc;
         internal UInt16 snSectionHeader;
         internal UInt16 snTokenRidMap;
         internal UInt16 snXData;
         internal UInt16 snPData;
         internal UInt16 snNewFPO;
         internal UInt16 snSectionHeaderOriginal;

         internal DBIDebugHeader( UInt16 snSecHdr )
         {
            this.snFPO = UInt16.MaxValue;
            this.snException = UInt16.MaxValue;
            this.snFixup = UInt16.MaxValue;
            this.snOmapToSrc = UInt16.MaxValue;
            this.snOmapFromSrc = UInt16.MaxValue;
            this.snSectionHeader = snSecHdr;
            this.snTokenRidMap = UInt16.MaxValue;
            this.snXData = UInt16.MaxValue;
            this.snPData = UInt16.MaxValue;
            this.snNewFPO = UInt16.MaxValue;
            this.snSectionHeaderOriginal = UInt16.MaxValue;
         }

         internal DBIDebugHeader( Byte[] array, ref Int32 idx )
         {
            this.snFPO = array.ReadUInt16LEFromBytes( ref idx );
            this.snException = array.ReadUInt16LEFromBytes( ref idx );
            this.snFixup = array.ReadUInt16LEFromBytes( ref idx );
            this.snOmapToSrc = array.ReadUInt16LEFromBytes( ref idx );
            this.snOmapFromSrc = array.ReadUInt16LEFromBytes( ref idx );
            this.snSectionHeader = array.ReadUInt16LEFromBytes( ref idx );
            this.snTokenRidMap = array.ReadUInt16LEFromBytes( ref idx );
            this.snXData = array.ReadUInt16LEFromBytes( ref idx );
            this.snPData = array.ReadUInt16LEFromBytes( ref idx );
            this.snNewFPO = array.ReadUInt16LEFromBytes( ref idx );
            this.snSectionHeaderOriginal = array.ReadUInt16LEFromBytes( ref idx );
         }

         internal void WriteToArray( Byte[] array, ref Int32 idx )
         {
            array
               .WriteUInt16LEToBytes( ref idx, this.snFPO )
               .WriteUInt16LEToBytes( ref idx, this.snException )
               .WriteUInt16LEToBytes( ref idx, this.snFixup )
               .WriteUInt16LEToBytes( ref idx, this.snOmapToSrc )
               .WriteUInt16LEToBytes( ref idx, this.snOmapFromSrc )
               .WriteUInt16LEToBytes( ref idx, this.snSectionHeader )
               .WriteUInt16LEToBytes( ref idx, this.snTokenRidMap )
               .WriteUInt16LEToBytes( ref idx, this.snXData )
               .WriteUInt16LEToBytes( ref idx, this.snPData )
               .WriteUInt16LEToBytes( ref idx, this.snNewFPO )
               .WriteUInt16LEToBytes( ref idx, this.snSectionHeaderOriginal );
         }
      }

      private struct PDBFunctionInfo
      {
         internal readonly PDBFunction function;
         internal readonly Int32 address;
         internal readonly UInt16 segment;
         internal readonly Int32 funcPointer;

         internal PDBFunctionInfo( PDBFunction func, Int32 addr, UInt16 seg, Int32 funcPtr )
         {
            this.function = func;
            this.address = addr;
            this.segment = seg;
            this.funcPointer = funcPtr;
         }

         public override String ToString()
         {
            return this.function + " at " + this.address + " in segment " + this.segment;
         }
      }

      private static Byte[] WriteZeroTerminatedString( this Byte[] array, ref Int32 idx, String str, Boolean useUTF8 = true )
      {
         array.WriteStringToBytes( ref idx, useUTF8 ? NameEncoding : UTF16, str );
         array.WriteByteToBytes( ref idx, 0 );
         if ( !useUTF8 )
         {
            array.WriteByteToBytes( ref idx, 0 );
         }
         return array;
      }

      private static Byte[] Align4( this Byte[] array, ref Int32 idx )
      {
         while ( idx % 4 != 0 )
         {
            array[idx++] = 0;
         }
         return array;
      }

   }
}
