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
 * See the License for the specific language governing permissions and
 * limitations under the License. 
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CILAssemblyManipulator.API;
using TRVA = System.UInt32;

namespace CILAssemblyManipulator.Implementation.Physical
{
   internal class MetaDataReader
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
      }

      // TODO: 2 Classes: non-thread-safe SysStringContainer, and thread-safe UserStringContainer
      internal abstract class AbstractStringContainer<TDic>
         where TDic : IDictionary<Int32, String>
      {
         internal protected readonly TDic _strings;
         internal protected readonly Encoding _encoding;
         internal protected readonly Byte[] _bytes;
         protected AbstractStringContainer( Stream stream, UInt32 size, Encoding encoding, TDic strings )
         {
            this._bytes = new Byte[size];
            stream.ReadWholeArray( this._bytes );
            this._encoding = encoding;
            this._strings = strings;
         }

         internal abstract String GetString( Int32 idx );
      }

      internal class SysStringContainer : AbstractStringContainer<IDictionary<Int32, String>>
      {
         internal SysStringContainer( Stream stream, UInt32 size )
            : base( stream, size, MetaDataConstants.SYS_STRING_ENCODING, new Dictionary<Int32, String>() )
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

      internal class UserStringContainer : AbstractStringContainer<ConcurrentDictionary<Int32, String>>
      {
         internal UserStringContainer( Stream stream, UInt32 size )
            : base( stream, size, MetaDataConstants.USER_STRING_ENCODING, new ConcurrentDictionary<Int32, String>() )
         {

         }

         internal override String GetString( Int32 idx )
         {
            return this._strings.GetOrAdd( idx, idxArg =>
            {
               // User strings
               var arrayIdx = idxArg;
               var length = this._bytes.DecompressUInt32( ref arrayIdx ) - 1;
               String result;
               if ( length == -1 )
               {
                  result = "";
               }
               else
               {
                  result = this._encoding.GetString( this._bytes, arrayIdx, length );
               }
               return result;
            } );
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

      // TODO consider changing tuples into structs. Memory consumption will be better, but, will there be problems in getting continous memory for array?
      internal readonly Tuple<UInt16, String, Guid?, Guid?, Guid?>[] module;
      internal readonly Tuple<TableIndex?, String, String>[] typeRef;
      internal readonly Tuple<TypeAttributes, String, String, TableIndex?, TableIndex, TableIndex>[] typeDef;
      internal readonly Tuple<FieldAttributes, String, Byte[]>[] field;
      internal readonly Tuple<TRVA, MethodImplAttributes, MethodAttributes, String, Byte[], TableIndex>[] methodDef;
      internal readonly Tuple<ParameterAttributes, UInt16, String>[] param;
      internal readonly Tuple<TableIndex, TableIndex>[] interfaceImpl;
      internal readonly Tuple<TableIndex, String, Byte[]>[] memberRef;
      internal readonly Tuple<UInt16, TableIndex, Byte[]>[] constant;
      internal readonly Tuple<TableIndex, TableIndex, Byte[]>[] customAttribute;
      internal readonly Tuple<TableIndex, Byte[]>[] fieldMarshal;
      internal readonly Tuple<UInt16, TableIndex, Byte[]>[] declSecurity;
      internal readonly Tuple<UInt16, UInt32, TableIndex>[] classLayout;
      internal readonly Tuple<UInt32, TableIndex>[] fieldLayout;
      internal readonly Byte[][] standaloneSig;
      internal readonly Tuple<TableIndex, TableIndex>[] eventMap;
      internal readonly Tuple<EventAttributes, String, TableIndex>[] events;
      internal readonly Tuple<TableIndex, TableIndex>[] propertyMap;
      internal readonly Tuple<PropertyAttributes, String, Byte[]>[] property;
      internal readonly Tuple<MethodSemanticsAttributes, TableIndex, TableIndex>[] methodSemantics;
      internal readonly Tuple<TableIndex, TableIndex, TableIndex>[] methodImpl;
      internal readonly String[] moduleRef;
      internal readonly Byte[][] typeSpec;
      internal readonly Tuple<PInvokeAttributes, TableIndex, String, TableIndex>[] implMap;
      internal readonly Tuple<TRVA, TableIndex>[] fieldRVA;
      internal readonly Tuple<AssemblyHashAlgorithm, UInt16, UInt16, UInt16, UInt16, AssemblyFlags, Byte[], String, String>[] assembly;
      internal readonly Tuple<UInt16, UInt16, UInt16, UInt16, AssemblyFlags, Byte[], String, String, Byte[]>[] assemblyRef;
      internal readonly Tuple<FileAttributes, String, Byte[]>[] file;
      internal readonly Tuple<TypeAttributes, UInt32, String, String, TableIndex>[] exportedType;
      internal readonly Tuple<UInt32, ManifestResourceAttributes, String, TableIndex?>[] manifestResource;
      internal readonly Tuple<TableIndex, TableIndex>[] nestedClass;
      internal readonly Tuple<UInt16, GenericParameterAttributes, TableIndex, String>[] genericParam;
      internal readonly Tuple<TableIndex, Byte[]>[] methodSpec;
      internal readonly Tuple<TableIndex, TableIndex>[] genericParamConstraint;

      internal readonly UserStringContainer userStrings;

      internal MetaDataReader( Stream stream, out String versionStr )
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
         foreach ( var kvp in streamDic )
         {
            stream.SeekFromBegin( mdRoot + kvp.Value.Item1 );
            switch ( kvp.Key )
            {
               case Consts.SYS_STRING_STREAM_NAME:
                  sysStrings = new SysStringContainer( stream, kvp.Value.Item2 );
                  break;
               case Consts.USER_STRING_STREAM_NAME:
                  this.userStrings = new UserStringContainer( stream, kvp.Value.Item2 );
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
         var tableSizes = Enumerable.Range( 0, TablesUtils.AMOUNT_OF_TABLES )
            .Select(
            val =>
            {
               UInt32 size;
               if ( ( (UInt32) ( presentTableMask >> val ) ) % 2 == 1 )
               {
                  size = stream.ReadU32( tmpArray );
               }
               else
               {
                  size = 0;
               }
               return size;
            } ).ToArray();

         // Read actual tables
         var tRefSizes = MetaDataConstants.GetCodedTableIndexSizes( tableSizes );
         foreach ( var curTable in Enumerable.Range( 0, TablesUtils.AMOUNT_OF_TABLES ) )
         {
            switch ( (Tables) curTable )
            {
               // VS2012 evaluates positional arguments from left to right, so creating Tuple inside lambda should work correctly
               // This is not so in VS2010 ( see http://msdn.microsoft.com/en-us/library/hh678682.aspx )
               case Tables.Module:
                  ReadTable( ref this.module, curTable, tableSizes, () =>
                     Tuple.Create(
                        stream.ReadU16( tmpArray ),
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        ReadGUID( stream, guids, streamWidths, tmpArray ),
                        ReadGUID( stream, guids, streamWidths, tmpArray ),
                        ReadGUID( stream, guids, streamWidths, tmpArray ) )
                  );
                  break;
               case Tables.TypeRef:
                  ReadTable( ref this.typeRef, curTable, tableSizes, () =>
                     Tuple.Create(
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.ResolutionScope, tRefSizes, tmpArray, false ),
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray )
                        ) );
                  break;
               case Tables.TypeDef:
                  ReadTable( ref this.typeDef, curTable, tableSizes, () =>
                     Tuple.Create(
                        (TypeAttributes) stream.ReadU32( tmpArray ),
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        ReadSysString( stream, sysStrings, streamWidths, tmpArray ),
                        MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.TypeDefOrRef, tRefSizes, tmpArray, false ),
                        MetaDataConstants.ReadSimpleTableIndex( stream, Tables.Field, tableSizes, tmpArray ),
                        MetaDataConstants.ReadSimpleTableIndex( stream, Tables.MethodDef, tableSizes, tmpArray )
                     ) );
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

      private static void ReadTable<T>( ref T[] tableArray, Int32 curTable, UInt32[] tableSizes, Func<T> rowReader )
      {
         // TODO - calculate table width, thus reading whole table into single array
         // Then give array as argument to rowReader
         // However, stream buffers things really well - is this really necessary? Not sure if performance boost will be worth it, and probably not good thing memory-wise if big tables are present.
         var len = tableSizes[curTable];
         tableArray = new T[len];
         for ( UInt32 i = 0; i < len; ++i )
         {
            tableArray[i] = rowReader();
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

   internal class Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>
   {
      private readonly T1 _item1;
      private readonly T2 _item2;
      private readonly T3 _item3;
      private readonly T4 _item4;
      private readonly T5 _item5;
      private readonly T6 _item6;
      private readonly T7 _item7;
      private readonly T8 _item8;
      private readonly T9 _item9;

      public Tuple( T1 i1, T2 i2, T3 i3, T4 i4, T5 i5, T6 i6, T7 i7, T8 i8, T9 i9 )
      {
         this._item1 = i1;
         this._item2 = i2;
         this._item3 = i3;
         this._item4 = i4;
         this._item5 = i5;
         this._item6 = i6;
         this._item7 = i7;
         this._item8 = i8;
         this._item9 = i9;
      }

      public T1 Item1
      {
         get
         {
            return this._item1;
         }
      }

      public T2 Item2
      {
         get
         {
            return this._item2;
         }
      }

      public T3 Item3
      {
         get
         {
            return this._item3;
         }
      }

      public T4 Item4
      {
         get
         {
            return this._item4;
         }
      }

      public T5 Item5
      {
         get
         {
            return this._item5;
         }
      }

      public T6 Item6
      {
         get
         {
            return this._item6;
         }
      }

      public T7 Item7
      {
         get
         {
            return this._item7;
         }
      }

      public T8 Item8
      {
         get
         {
            return this._item8;
         }
      }

      public T9 Item9
      {
         get
         {
            return this._item9;
         }
      }
   }

   internal static class Tuples
   {
      internal static Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>( T1 i1, T2 i2, T3 i3, T4 i4, T5 i5, T6 i6, T7 i7, T8 i8, T9 i9 )
      {
         return new Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>( i1, i2, i3, i4, i5, i6, i7, i8, i9 );
      }
   }
}