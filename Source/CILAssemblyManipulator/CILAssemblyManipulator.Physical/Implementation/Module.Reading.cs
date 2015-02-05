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
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.Implementation
{
   internal static class ModuleReader
   {
      internal class BLOBContainer
      {
         private static readonly Byte[] EMPTY_ARRAY = new Byte[0];

         private readonly Byte[] _bytes;
         private readonly IDictionary<UInt32, Byte[]> _blobs;

         internal BLOBContainer( Stream stream, UInt32 size )
         {
            this._bytes = new Byte[size];
            stream.ReadWholeArray( this._bytes );
            this._blobs = new Dictionary<UInt32, Byte[]>();
         }

         internal Byte[] GetBLOB( UInt32 idx )
         {
            Byte[] result;
            if ( !this._blobs.TryGetValue( idx, out result ) )
            {
               var idxToGive = (Int32) idx;
               var length = this._bytes.DecompressUInt32( ref idxToGive );
               if ( length == 0 )
               {
                  // There might be no more bytes after this
                  result = EMPTY_ARRAY;
               }
               else
               {
                  result = this._bytes.CreateAndBlockCopyTo( ref idxToGive, length );
               }
               this._blobs.Add( idx, result );
            }
            return result;
         }

         // Useful when reading signatures - don't need to create array for each.
         internal Byte[] WholeBLOBStream
         {
            get
            {
               return this._bytes;
            }
         }
      }

      internal abstract class AbstractStringContainer
      {
         internal protected readonly IDictionary<Int32, String> _strings;
         internal protected readonly Encoding _encoding;
         internal protected readonly Byte[] _bytes;
         protected AbstractStringContainer( Stream stream, UInt32 size, Encoding encoding )
         {
            this._bytes = new Byte[size];
            stream.ReadWholeArray( this._bytes );
            this._encoding = encoding;
            this._strings = new Dictionary<Int32, String>();
         }

         internal abstract String GetString( Int32 idx );
      }

      internal class SysStringContainer : AbstractStringContainer
      {
         internal SysStringContainer( Stream stream, UInt32 size )
            : base( stream, size, MetaDataConstants.SYS_STRING_ENCODING )
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
      }

      internal class UserStringContainer : AbstractStringContainer
      {
         internal UserStringContainer( Stream stream, UInt32 size )
            : base( stream, size, MetaDataConstants.USER_STRING_ENCODING )
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

      internal class GUIDContainer
      {
         private readonly Byte[] _bytes;

         internal GUIDContainer( Stream stream, UInt32 size )
         {
            this._bytes = new Byte[size];
            stream.ReadWholeArray( this._bytes );
         }

         internal Guid? GetGUID( UInt32 idx )
         {
            if ( idx == 0 )
            {
               return null;
            }
            else
            {
               var array = new Byte[Consts.GUID_SIZE];
               Buffer.BlockCopy( this._bytes, ( (Int32) idx - 1 ) % Consts.GUID_SIZE, array, 0, Consts.GUID_SIZE );
               return new Guid( array );
            }
         }
      }

      private const Byte WIDE_SYS_STRING_FLAG = 0x01;
      private const Byte WIDE_GUID_FLAG = 0x02;
      private const Byte WIDE_BLOB_FLAG = 0x04;
      private const Int32 SYS_STR_WIDTH_INDEX = 0;
      private const Int32 GUID_WIDTH_INDEX = 1;
      private const Int32 BLOB_WIDTH_INDEX = 2;
      private const Int32 MAX_WIDTH_INDEX = 3;

      internal CILMetaData ReadMetadata( Stream stream, out String versionStr )
      {
         var mdRoot = stream.Position;

         // Prepare variables
         var utf8 = MetaDataConstants.SYS_STRING_ENCODING;
         var tmpArray = new Byte[8];

         // Skip signature, major & minor version, and reserved
         stream.SeekFromCurrent( 12 );

         // Read version string
         var versionStrByteLen = stream.ReadU32( tmpArray );
         versionStr = stream.ReadZeroTerminatedString( versionStrByteLen, utf8 );

         // Skip flags
         stream.SeekFromCurrent( 2 );

         // Amount of streams
         var amountOfStreams = stream.ReadU16( tmpArray );

         // Stream headers
         var streamDic = new Dictionary<String, Tuple<UInt32, UInt32>>();
         //var totalRead = 12 // Sig, major & minor version, reserved
         //   + versionStrByteLen // Version string
         //   + 4; // Flags, amount of streams
         for ( var i = 0; i < amountOfStreams; ++i )
         {
            var offset = stream.ReadU32( tmpArray );
            var size = stream.ReadU32( tmpArray );
            //UInt32 streamStringBytesLen;
            streamDic.Add( stream.ReadAlignedASCIIString( 32 ), Tuple.Create( offset, size ) );
            //totalRead += streamStringBytesLen + 8;
         }

         // Read all streams except table stream
         SysStringContainer sysStrings = null;
         GUIDContainer guids = null;
         BLOBContainer blobs = null;
         UserStringContainer userStrings = null;
         foreach ( var kvp in streamDic )
         {
            stream.SeekFromBegin( mdRoot + kvp.Value.Item1 );
            switch ( kvp.Key )
            {
               case Consts.SYS_STRING_STREAM_NAME:
                  sysStrings = new SysStringContainer( stream, kvp.Value.Item2 );
                  break;
               case Consts.USER_STRING_STREAM_NAME:
                  userStrings = new UserStringContainer( stream, kvp.Value.Item2 );
                  break;
               case Consts.GUID_STREAM_NAME:
                  guids = new GUIDContainer( stream, kvp.Value.Item2 );
                  break;
               case Consts.BLOB_STREAM_NAME:
                  blobs = new BLOBContainer( stream, kvp.Value.Item2 );
                  break;
            }
         }

         // Read table stream
         stream.SeekFromBegin( mdRoot + streamDic[Consts.TABLE_STREAM_NAME].Item1
            + 6 // Skip reserved + major & minor versions
            );

         // Stream index sizes
         var b = stream.ReadByteFromStream();
         var streamWidths = new Boolean[MAX_WIDTH_INDEX];
         streamWidths[SYS_STR_WIDTH_INDEX] = ( b & WIDE_SYS_STRING_FLAG ) != 0;
         streamWidths[GUID_WIDTH_INDEX] = ( b & WIDE_GUID_FLAG ) != 0;
         streamWidths[BLOB_WIDTH_INDEX] = ( b & WIDE_BLOB_FLAG ) != 0;

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
         for ( var curTable = 0; curTable < Consts.AMOUNT_OF_TABLES; ++curTable )
         {
            switch ( (Tables) curTable )
            {
               // VS2012 evaluates positional arguments from left to right, so creating Tuple inside lambda should work correctly
               // This is not so in VS2010 ( see http://msdn.microsoft.com/en-us/library/hh678682.aspx )
               case Tables.Module:
                  ReadTable( retVal.ModuleDefinitions, curTable, tableSizes, () =>
                     new ModuleDefinition()
                     {
                        Generation = stream.ReadI16( tmpArray ),
                        Name = ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        ModuleGUID = ReadGUID( stream, guids, streamWidths, tmpArray ),
                        EditAndContinueGUID = ReadGUID( stream, guids, streamWidths, tmpArray ),
                        EditAndContinueBaseGUID = ReadGUID( stream, guids, streamWidths, tmpArray )
                     }
                  );
                  break;
               case Tables.TypeRef:
                  ReadTable( retVal.TypeReferences, curTable, tableSizes, () =>
                     new TypeReference()
                     {
                        ResolutionScope = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.ResolutionScope, tRefSizes, tmpArray, false ),
                        Name = ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        Namespace = ReadSysString( stream, sysStrings, streamWidths, tmpArray )
                     } );
                  break;
               case Tables.TypeDef:
                  ReadTable( retVal.TypeDefinitions, curTable, tableSizes, () =>
                     new TypeDefinition()
                     {
                        Attributes = (TypeAttributes) stream.ReadU32( tmpArray ),
                        Name = ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        Namespace = ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        BaseType = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.TypeDefOrRef, tRefSizes, tmpArray, false ),
                        FieldList = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.Field, tableSizes, tmpArray ),
                        MethodList = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.MethodDef, tableSizes, tmpArray )
                     } );
                  break;
               case Tables.Field:
                  ReadTable( ref this.field, curTable, tableSizes, () =>
                     Tuple.Create(
                        (FieldAttributes) stream.ReadU16( tmpArray ),
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        ReadBLOB( stream, blobs, streamWidths, tmpArray )
                     ) );
                  break;
               case Tables.MethodDef:
                  ReadTable( ref this.methodDef, curTable, tableSizes, () =>
                     Tuple.Create(
                        stream.ReadU32( tmpArray ),
                        (MethodImplAttributes) stream.ReadU16( tmpArray ),
                        (MethodAttributes) stream.ReadU16( tmpArray ),
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        ReadBLOB( stream, blobs, streamWidths, tmpArray ),
                        MetaDataConstants.ReadSimpleTableIndex( stream, Tables.Parameter, tableSizes, tmpArray )
                     ) );
                  break;
               case Tables.Parameter:
                  ReadTable( ref this.param, curTable, tableSizes, () =>
                     Tuple.Create(
                        (ParameterAttributes) stream.ReadU16( tmpArray ),
                        stream.ReadU16( tmpArray ),
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray )
                     ) );
                  break;
               case Tables.InterfaceImpl:
                  ReadTable( ref this.interfaceImpl, curTable, tableSizes, () =>
                     Tuple.Create(
                        MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray ),
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.TypeDefOrRef, tRefSizes, tmpArray ).Value
                     ) );
                  break;
               case Tables.MemberRef:
                  ReadTable( ref this.memberRef, curTable, tableSizes, () =>
                     Tuple.Create(
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.MemberRefParent, tRefSizes, tmpArray ).Value,
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        ReadBLOB( stream, blobs, streamWidths, tmpArray )
                     ) );
                  break;
               case Tables.Constant:
                  ReadTable( ref this.constant, curTable, tableSizes, () =>
                     Tuple.Create(
                        stream.ReadU16( tmpArray ),
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.HasConstant, tRefSizes, tmpArray ).Value,
                        ReadBLOB( stream, blobs, streamWidths, tmpArray )
                     ) );
                  break;
               case Tables.CustomAttribute:
                  ReadTable( ref this.customAttribute, curTable, tableSizes, () =>
                     Tuple.Create(
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.HasCustomAttribute, tRefSizes, tmpArray ).Value,
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.CustomAttributeType, tRefSizes, tmpArray ).Value,
                        ReadBLOB( stream, blobs, streamWidths, tmpArray )
                     ) );
                  break;
               case Tables.FieldMarshal:
                  ReadTable( ref this.fieldMarshal, curTable, tableSizes, () =>
                     Tuple.Create(
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.HasFieldMarshal, tRefSizes, tmpArray ).Value,
                        ReadBLOB( stream, blobs, streamWidths, tmpArray )
                     ) );
                  break;
               case Tables.DeclSecurity:
                  ReadTable( ref this.declSecurity, curTable, tableSizes, () =>
                     Tuple.Create(
                        stream.ReadU16( tmpArray ),
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.HasDeclSecurity, tRefSizes, tmpArray ).Value,
                        ReadBLOB( stream, blobs, streamWidths, tmpArray )
                     ) );
                  break;
               case Tables.ClassLayout:
                  ReadTable( ref this.classLayout, curTable, tableSizes, () =>
                     Tuple.Create(
                        stream.ReadU16( tmpArray ),
                        stream.ReadU32( tmpArray ),
                        MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray )
                     ) );
                  break;
               case Tables.FieldLayout:
                  ReadTable( ref this.fieldLayout, curTable, tableSizes, () =>
                     Tuple.Create(
                        stream.ReadU32( tmpArray ),
                        MetaDataConstants.ReadSimpleTableIndex( stream, Tables.Field, tableSizes, tmpArray )
                     ) );
                  break;
               case Tables.StandaloneSignature:
                  ReadTable( ref this.standaloneSig, curTable, tableSizes, () => ReadBLOB( stream, blobs, streamWidths, tmpArray ) );
                  break;
               case Tables.EventMap:
                  ReadTable( ref this.eventMap, curTable, tableSizes, () =>
                     Tuple.Create(
                        MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray ),
                        MetaDataConstants.ReadSimpleTableIndex( stream, Tables.Event, tableSizes, tmpArray )
                     ) );
                  break;
               case Tables.Event:
                  ReadTable( ref this.events, curTable, tableSizes, () =>
                     Tuple.Create(
                        (EventAttributes) stream.ReadU16( tmpArray ),
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.TypeDefOrRef, tRefSizes, tmpArray ).Value
                     ) );
                  break;
               case Tables.PropertyMap:
                  ReadTable( ref this.propertyMap, curTable, tableSizes, () =>
                     Tuple.Create(
                        MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray ),
                        MetaDataConstants.ReadSimpleTableIndex( stream, Tables.Property, tableSizes, tmpArray )
                     ) );
                  break;
               case Tables.Property:
                  ReadTable( ref this.property, curTable, tableSizes, () =>
                     Tuple.Create(
                        (PropertyAttributes) stream.ReadU16( tmpArray ),
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        ReadBLOB( stream, blobs, streamWidths, tmpArray )
                     ) );
                  break;
               case Tables.MethodSemantics:
                  ReadTable( ref this.methodSemantics, curTable, tableSizes, () =>
                     Tuple.Create(
                        (MethodSemanticsAttributes) stream.ReadU16( tmpArray ),
                        MetaDataConstants.ReadSimpleTableIndex( stream, Tables.MethodDef, tableSizes, tmpArray ),
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.HasSemantics, tRefSizes, tmpArray ).Value
                     ) );
                  break;
               case Tables.MethodImpl:
                  ReadTable( ref this.methodImpl, curTable, tableSizes, () =>
                     Tuple.Create(
                        MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray ),
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.MethodDefOrRef, tRefSizes, tmpArray ).Value,
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.MethodDefOrRef, tRefSizes, tmpArray ).Value
                     ) );
                  break;
               case Tables.ModuleRef:
                  ReadTable( ref this.moduleRef, curTable, tableSizes, () => ReadSysString( stream, sysStrings, streamWidths, tmpArray ) );
                  break;
               case Tables.TypeSpec:
                  ReadTable( ref this.typeSpec, curTable, tableSizes, () => ReadBLOB( stream, blobs, streamWidths, tmpArray ) );
                  break;
               case Tables.ImplMap:
                  ReadTable( ref this.implMap, curTable, tableSizes, () =>
                     Tuple.Create(
                        (PInvokeAttributes) stream.ReadU16( tmpArray ),
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.MemberForwarded, tRefSizes, tmpArray ).Value,
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        MetaDataConstants.ReadSimpleTableIndex( stream, Tables.ModuleRef, tableSizes, tmpArray )
                     ) );
                  break;
               case Tables.FieldRVA:
                  ReadTable( ref this.fieldRVA, curTable, tableSizes, () =>
                     Tuple.Create(
                        stream.ReadU32( tmpArray ),
                        MetaDataConstants.ReadSimpleTableIndex( stream, Tables.Field, tableSizes, tmpArray )
                     ) );
                  break;
               case Tables.Assembly:
                  ReadTable( ref this.assembly, curTable, tableSizes, () =>
                     Tuples.Create(
                        (AssemblyHashAlgorithm) stream.ReadU32( tmpArray ),
                        stream.ReadU16( tmpArray ),
                        stream.ReadU16( tmpArray ),
                        stream.ReadU16( tmpArray ),
                        stream.ReadU16( tmpArray ),
                        (AssemblyFlags) stream.ReadU32( tmpArray ),
                        ReadBLOB( stream, blobs, streamWidths, tmpArray ),
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray )
                     ) );
                  break;
               case Tables.AssemblyRef:
                  ReadTable( ref this.assemblyRef, curTable, tableSizes, () =>
                     Tuples.Create(
                        stream.ReadU16( tmpArray ),
                        stream.ReadU16( tmpArray ),
                        stream.ReadU16( tmpArray ),
                        stream.ReadU16( tmpArray ),
                        (AssemblyFlags) stream.ReadU32( tmpArray ),
                        ReadBLOB( stream, blobs, streamWidths, tmpArray ),
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        ReadBLOB( stream, blobs, streamWidths, tmpArray )
                     ) );
                  break;
               case Tables.File:
                  ReadTable( ref this.file, curTable, tableSizes, () =>
                     Tuple.Create(
                        (FileAttributes) stream.ReadU32( tmpArray ),
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        ReadBLOB( stream, blobs, streamWidths, tmpArray )
                     ) );
                  break;
               case Tables.ExportedType:
                  ReadTable( ref this.exportedType, curTable, tableSizes, () =>
                     Tuple.Create(
                        (TypeAttributes) stream.ReadU32( tmpArray ),
                        stream.ReadU32( tmpArray ),
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.Implementation, tRefSizes, tmpArray ).Value
                     ) );
                  break;
               case Tables.ManifestResource:
                  ReadTable( ref this.manifestResource, curTable, tableSizes, () =>
                     Tuple.Create(
                        stream.ReadU32( tmpArray ),
                        (ManifestResourceAttributes) stream.ReadU32( tmpArray ),
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.Implementation, tRefSizes, tmpArray, false )
                     ) );
                  break;
               case Tables.NestedClass:
                  ReadTable( ref this.nestedClass, curTable, tableSizes, () =>
                     Tuple.Create(
                        MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray ),
                        MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray )
                     ) );
                  break;
               case Tables.GenericParameter:
                  ReadTable( ref this.genericParam, curTable, tableSizes, () =>
                     Tuple.Create(
                        stream.ReadU16( tmpArray ),
                        (GenericParameterAttributes) stream.ReadU16( tmpArray ),
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.TypeOrMethodDef, tRefSizes, tmpArray ).Value,
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray )
                     ) );
                  break;
               case Tables.MethodSpec:
                  ReadTable( ref this.methodSpec, curTable, tableSizes, () =>
                     Tuple.Create(
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.MethodDefOrRef, tRefSizes, tmpArray ).Value,
                        ReadBLOB( stream, blobs, streamWidths, tmpArray )
                     ) );
                  break;
               case Tables.GenericParameterConstraint:
                  ReadTable( ref this.genericParamConstraint, curTable, tableSizes, () =>
                     Tuple.Create(
                        MetaDataConstants.ReadSimpleTableIndex( stream, Tables.GenericParameter, tableSizes, tmpArray ),
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.TypeDefOrRef, tRefSizes, tmpArray ).Value
                     ) );
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
                  // Skip
                  break;
               default:
                  throw new BadImageFormatException( "Unknown table: " + curTable );
            }
         }

      }

      private static void ReadTable<T>( IList<T> tableArray, Int32 curTable, Int32[] tableSizes, Func<T> rowReader )
      {
         // TODO - calculate table width, thus reading whole table into single array
         // Then give array as argument to rowReader
         // However, stream buffers things really well - is this really necessary? Not sure if performance boost will be worth it, and probably not good thing memory-wise if big tables are present.
         var len = tableSizes[curTable];
         for ( var i = 0; i < len; ++i )
         {
            tableArray.Add( rowReader() );
         }
      }

      private static String ReadSysString( Stream stream, SysStringContainer sysStrings, Boolean[] streamWidths, Byte[] tmpArray )
      {
         return sysStrings.GetString( (Int32) stream.ReadHeapIndex( streamWidths[SYS_STR_WIDTH_INDEX], tmpArray ) );
      }

      private static Guid? ReadGUID( Stream stream, GUIDContainer guids, Boolean[] streamWidths, Byte[] tmpArray )
      {
         return guids.GetGUID( stream.ReadHeapIndex( streamWidths[GUID_WIDTH_INDEX], tmpArray ) );
      }

      private static Byte[] ReadBLOB( Stream stream, BLOBContainer blobs, Boolean[] streamWidths, Byte[] tmpArray )
      {
         return blobs.GetBLOB( stream.ReadHeapIndex( streamWidths[BLOB_WIDTH_INDEX], tmpArray ) );
      }
   }
}
