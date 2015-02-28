using CommonUtils;
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

   internal static partial class ModuleReader
   {
      internal class BLOBContainer : AbstractHeapContainer
      {

         internal BLOBContainer( Byte[] tmpArray, Stream stream, IDictionary<String, Tuple<Int64, UInt32>> streamSizeInfo )
            : base( tmpArray, stream, streamSizeInfo, Consts.BLOB_STREAM_NAME )
         {
         }

         internal Byte[] GetBLOB( Int32 idx )
         {
            var idxToGive = idx;
            var length = this._bytes.DecompressUInt32( ref idxToGive );
            var result = length <= 0 ?
               Empty<Byte>.Array :
               this._bytes.CreateAndBlockCopyTo( ref idxToGive, length );
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
            var idx = this.ReadHeapIndex( stream );
            this._bytes.DecompressUInt32( ref idx );
            return idx;
         }

         internal Int32 GetBLOBIndex( Stream stream, out Int32 blobSize )
         {
            var idx = this.ReadHeapIndex( stream );
            blobSize = this._bytes.DecompressUInt32( ref idx );
            return idx;
         }

         internal Int32 GetBLOBIndex( Stream stream, out Int32 heapIndex, out Int32 blobSize )
         {
            var idx = this.ReadHeapIndex( stream );
            heapIndex = idx;
            blobSize = this._bytes.DecompressUInt32( ref idx );
            return idx;
         }
      }

      internal abstract class AbstractHeapContainer
      {

         protected readonly Byte[] _tmpArray;
         protected readonly Byte[] _bytes;

         internal AbstractHeapContainer( Byte[] tmpArray, Stream stream, IDictionary<String, Tuple<Int64, UInt32>> streamSizeInfo, String name )
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

      internal abstract class AbstractStringContainer : AbstractHeapContainer
      {
         internal protected readonly IDictionary<Int32, String> _strings;
         internal protected readonly Encoding _encoding;

         protected AbstractStringContainer( Byte[] tmpArray, Stream stream, IDictionary<String, Tuple<Int64, UInt32>> streamSizeInfo, String name, Encoding encoding )
            : base( tmpArray, stream, streamSizeInfo, name )
         {
            this._encoding = encoding;
            this._strings = new Dictionary<Int32, String>();
         }

         internal abstract String GetString( Int32 idx );
      }

      internal class SysStringContainer : AbstractStringContainer
      {
         internal SysStringContainer( Byte[] tmpArray, Stream stream, IDictionary<String, Tuple<Int64, UInt32>> streamSizeInfo )
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

      internal class UserStringContainer : AbstractStringContainer
      {
         internal UserStringContainer( Byte[] tmpArray, Stream stream, IDictionary<String, Tuple<Int64, UInt32>> streamSizeInfo )
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

      internal class GUIDContainer : AbstractHeapContainer
      {

         internal GUIDContainer( Byte[] tmpArray, Stream stream, IDictionary<String, Tuple<Int64, UInt32>> streamSizeInfo )
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

      public static CILMetaData ReadFromStream(
         Stream stream,
         out HeadersData headers
         )
      {

         Byte[] tmpArray = new Byte[8];

         // DOS header, skip to lfa new
         stream.SeekFromBegin( 60 );

         // PE file header
         // Skip to PE file header, and skip magic
         var suuka = stream.ReadU32( tmpArray );
         stream.SeekFromBegin( suuka + 4 );

         // Architecture
         var architecture = (ImageFileMachine) stream.ReadU16( tmpArray );

         // Amount of sections
         var amountOfSections = stream.ReadU16( tmpArray );

         // Skip timestamp, symbol table pointer, number of symbols
         stream.SeekFromCurrent( 12 );

         // Optional header size
         stream.ReadU16( tmpArray );

         // Characteristics
         var characteristics = stream.ReadU16( tmpArray );

         // PE Optional header
         // Skip standard fields and all NT-specific fields until subsystem
         stream.SeekFromCurrent( 68 ); // Value is the same for both pe32 & pe64, since BaseOfData is lacking from pe64

         // Subsystem
         var subsystem = stream.ReadU16( tmpArray );

         // DLL flags
         var dllFlags = (DLLFlags) stream.ReadU16( tmpArray );

         // Skip to debug header
         stream.SeekFromCurrent( architecture.RequiresPE64() ? 88 : 72 ); // PE64 requires 8 bytes for stack reserve & commit sizes, and heap reserve & commit sizes
         var debugDD = new DataDir( stream, tmpArray );

         // Skip to CLI header
         stream.SeekFromCurrent( 56 );

         // CLI header
         var cliDD = new DataDir( stream, tmpArray );

         // Reserved
         stream.SeekFromCurrent( 8 );

         // Read sections
         var sections = new SectionInfo[amountOfSections];
         for ( var i = 0u; i < amountOfSections; ++i )
         {
            // VS2012 evaluates positional arguments from left to right, so creating Tuple should work correctly
            // This is not so in VS2010 ( see http://msdn.microsoft.com/en-us/library/hh678682.aspx )
            stream.ReadWholeArray( tmpArray ); // tmpArray is 8 bytes long
            sections[i] = new SectionInfo(
               tmpArray.ReadZeroTerminatedASCIIStringFromBytes(), // Section name
               stream.ReadU32( tmpArray ), // Virtual size
               stream.ReadU32( tmpArray ), // Virtual address
               stream.ReadU32( tmpArray ), // Raw data size
               stream.ReadU32( tmpArray ) // Raw data pointer
               );
            // Skip number of relocation & line numbers, and section characteristics
            stream.SeekFromCurrent( 16 );
         }

         // CLI header, skip magic and runtime versions
         stream.SeekFromBegin( ResolveRVA( cliDD.rva, sections ) + 8 );

         // Metadata datadirectory
         var mdDD = new DataDir( stream, tmpArray );

         // Module flags
         var moduleFlags = (ModuleFlags) stream.ReadU32( tmpArray );

         // Entrypoint token
         var epToken = (Int32) stream.ReadU32( tmpArray );

         // Resources data directory
         var rsrcDD = new DataDir( stream, tmpArray );

         // Strong name
         //var snDD = new DataDir( stream, tmpArray );

         // Skip code manager table, virtual table fixups, export address table jumps, and managed native header data directories
         //stream.SeekFromCurrent( 32 );

         // Metadata
         stream.SeekFromBegin( ResolveRVA( mdDD.rva, sections ) );
         String mdVersion;
         var retVal = ReadMetadata(
            stream,
            sections,
            out mdVersion
            );

         // TODO
         headers = null;

         return retVal;
      }

      internal static CILMetaData ReadMetadata(
         Stream stream,
         SectionInfo[] sections,
         out String versionStr
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
         versionStr = stream.ReadZeroTerminatedString( versionStrByteLen, utf8 );

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
         SysStringContainer sysStrings = new SysStringContainer( tmpArray, stream, streamDic );
         GUIDContainer guids = new GUIDContainer( tmpArray, stream, streamDic );
         BLOBContainer blobs = new BLOBContainer( tmpArray, stream, streamDic );
         UserStringContainer userStrings = new UserStringContainer( tmpArray, stream, streamDic );

         // Read table stream
         stream.SeekFromBegin( streamDic[Consts.TABLE_STREAM_NAME].Item1
            + 6 // Skip reserved + major & minor versions
            );

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
         var methodDefRVAs = new Int64[tableSizes[(Int32) Tables.MethodDef]];
         var fieldDefRVAs = new Int64[tableSizes[(Int32) Tables.FieldRVA]];
         var typeResolveCache = new CATypeResolveCache();

         for ( var curTable = 0; curTable < Consts.AMOUNT_OF_TABLES; ++curTable )
         {
            switch ( (Tables) curTable )
            {
               // VS2012 evaluates positional arguments from left to right, so creating Tuple inside lambda should work correctly
               // This is not so in VS2010 ( see http://msdn.microsoft.com/en-us/library/hh678682.aspx )
               case Tables.Module:
                  ReadTable( retVal.ModuleDefinitions, curTable, tableSizes, i =>
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
                  ReadTable( retVal.TypeReferences, curTable, tableSizes, i =>
                     new TypeReference()
                     {
                        ResolutionScope = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.ResolutionScope, tRefSizes, tmpArray, false ),
                        Name = sysStrings.ReadSysString( stream ),
                        Namespace = sysStrings.ReadSysString( stream )
                     } );
                  break;
               case Tables.TypeDef:
                  ReadTable( retVal.TypeDefinitions, curTable, tableSizes, i =>
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
                  ReadTable( retVal.FieldDefinitions, curTable, tableSizes, i =>
                     new FieldDefinition()
                     {
                        Attributes = (FieldAttributes) stream.ReadU16( tmpArray ),
                        Name = sysStrings.ReadSysString( stream ),
                        Signature = FieldSignature.ReadFromBytes( blobs.WholeBLOBArray, blobs.GetBLOBIndex( stream ) )
                     } );
                  break;
               case Tables.MethodDef:
                  ReadTable( retVal.MethodDefinitions, curTable, tableSizes, i =>
                  {
                     methodDefRVAs[i] = stream.ReadI32( tmpArray );
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
                  ReadTable( retVal.ParameterDefinitions, curTable, tableSizes, i =>
                     new ParameterDefinition()
                     {
                        Attributes = (ParameterAttributes) stream.ReadU16( tmpArray ),
                        Sequence = stream.ReadU16( tmpArray ),
                        Name = sysStrings.ReadSysString( stream )
                     } );
                  break;
               case Tables.InterfaceImpl:
                  ReadTable( retVal.InterfaceImplementations, curTable, tableSizes, i =>
                     new InterfaceImplementation()
                     {
                        Class = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray ),
                        Interface = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.TypeDefOrRef, tRefSizes, tmpArray ).Value
                     } );
                  break;
               case Tables.MemberRef:
                  ReadTable( retVal.MemberReferences, curTable, tableSizes, i =>
                     new MemberReference()
                     {
                        DeclaringType = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.MemberRefParent, tRefSizes, tmpArray ).Value,
                        Name = sysStrings.ReadSysString( stream ),
                        Signature = ReadMemberRefSignature( blobs.WholeBLOBArray, blobs.GetBLOBIndex( stream ) )
                     } );
                  break;
               case Tables.Constant:
                  ReadTable( retVal.ConstantDefinitions, curTable, tableSizes, i =>
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
                  ReadTable( retVal.CustomAttributeDefinitions, curTable, tableSizes, i =>
                  {
                     var ca = new CustomAttributeDefinition()
                     {
                        Parent = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.HasCustomAttribute, tRefSizes, tmpArray ).Value,
                        Type = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.CustomAttributeType, tRefSizes, tmpArray ).Value,
                     };
                     ca.Signature = ReadCustomAttributeSignature( blobs, stream, retVal, ca.Type, typeResolveCache );
                     return ca;
                  } );
                  break;
               case Tables.FieldMarshal:
                  ReadTable( retVal.FieldMarshals, curTable, tableSizes, i =>
                     new FieldMarshal()
                     {
                        Parent = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.HasFieldMarshal, tRefSizes, tmpArray ).Value,
                        NativeType = MarshalingInfo.ReadFromBytes( blobs.WholeBLOBArray, blobs.GetBLOBIndex( stream ) )
                     } );
                  break;
               case Tables.DeclSecurity:
                  ReadTable( retVal.SecurityDefinitions, curTable, tableSizes, i =>
                  {
                     var sec = new SecurityDefinition()
                     {
                        Action = (SecurityAction) stream.ReadI16( tmpArray ),
                        Parent = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.HasDeclSecurity, tRefSizes, tmpArray ).Value
                     };
                     ReadSecurityBLOB( retVal, blobs, stream, sec, typeResolveCache );
                     return sec;
                  } );
                  break;
               case Tables.ClassLayout:
                  ReadTable( retVal.ClassLayouts, curTable, tableSizes, i =>
                     new ClassLayout()
                     {
                        PackingSize = stream.ReadI16( tmpArray ),
                        ClassSize = stream.ReadI32( tmpArray ),
                        Parent = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray )
                     } );
                  break;
               case Tables.FieldLayout:
                  ReadTable( retVal.FieldLayouts, curTable, tableSizes, i =>
                     new FieldLayout()
                     {
                        Offset = stream.ReadI32( tmpArray ),
                        Field = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.Field, tableSizes, tmpArray )
                     } );
                  break;
               case Tables.StandaloneSignature:
                  ReadTable( retVal.StandaloneSignatures, curTable, tableSizes, i =>
                     new StandaloneSignature()
                     {
                        Signature = ReadStandaloneSignature( blobs.WholeBLOBArray, blobs.GetBLOBIndex( stream ) )
                     } );
                  break;
               case Tables.EventMap:
                  ReadTable( retVal.EventMaps, curTable, tableSizes, i =>
                     new EventMap()
                     {
                        Parent = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray ),
                        EventList = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.Event, tableSizes, tmpArray )
                     } );
                  break;
               case Tables.Event:
                  ReadTable( retVal.EventDefinitions, curTable, tableSizes, i =>
                     new EventDefinition()
                     {
                        Attributes = (EventAttributes) stream.ReadU16( tmpArray ),
                        Name = sysStrings.ReadSysString( stream ),
                        EventType = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.TypeDefOrRef, tRefSizes, tmpArray ).Value
                     } );
                  break;
               case Tables.PropertyMap:
                  ReadTable( retVal.PropertyMaps, curTable, tableSizes, i =>
                     new PropertyMap()
                     {
                        Parent = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray ),
                        PropertyList = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.Property, tableSizes, tmpArray )
                     } );
                  break;
               case Tables.Property:
                  ReadTable( retVal.PropertyDefinitions, curTable, tableSizes, i =>
                     new PropertyDefinition()
                     {
                        Attributes = (PropertyAttributes) stream.ReadU16( tmpArray ),
                        Name = sysStrings.ReadSysString( stream ),
                        Signature = PropertySignature.ReadFromBytes( blobs.WholeBLOBArray, blobs.GetBLOBIndex( stream ) )
                     } );
                  break;
               case Tables.MethodSemantics:
                  ReadTable( retVal.MethodSemantics, curTable, tableSizes, i =>
                     new MethodSemantics()
                     {
                        Attributes = (MethodSemanticsAttributes) stream.ReadU16( tmpArray ),
                        Method = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.MethodDef, tableSizes, tmpArray ),
                        Associaton = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.HasSemantics, tRefSizes, tmpArray ).Value
                     } );
                  break;
               case Tables.MethodImpl:
                  ReadTable( retVal.MethodImplementations, curTable, tableSizes, i =>
                     new MethodImplementation()
                     {
                        Class = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray ),
                        MethodBody = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.MethodDefOrRef, tRefSizes, tmpArray ).Value,
                        MethodDeclaration = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.MethodDefOrRef, tRefSizes, tmpArray ).Value
                     } );
                  break;
               case Tables.ModuleRef:
                  ReadTable( retVal.ModuleReferences, curTable, tableSizes, i =>
                     new ModuleReference()
                     {
                        ModuleRefeference = sysStrings.ReadSysString( stream )
                     } );
                  break;
               case Tables.TypeSpec:
                  ReadTable( retVal.TypeSpecifications, curTable, tableSizes, i =>
                     new TypeSpecification()
                     {
                        Signature = TypeSignature.ReadFromBytes( blobs.WholeBLOBArray, blobs.GetBLOBIndex( stream ) )
                     } );
                  break;
               case Tables.ImplMap:
                  ReadTable( retVal.MethodImplementationMaps, curTable, tableSizes, i =>
                     new MethodImplementationMap()
                     {
                        Attributes = (PInvokeAttributes) stream.ReadU16( tmpArray ),
                        MemberForwarded = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.MemberForwarded, tRefSizes, tmpArray ).Value,
                        ImportName = sysStrings.ReadSysString( stream ),
                        ImportScope = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.ModuleRef, tableSizes, tmpArray )
                     } );
                  break;
               case Tables.FieldRVA:
                  ReadTable( retVal.FieldRVAs, curTable, tableSizes, i =>
                  {
                     fieldDefRVAs[i] = stream.ReadI32( tmpArray );
                     return new FieldRVA()
                     {
                        Field = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.Field, tableSizes, tmpArray )
                     };
                  } );
                  break;
               case Tables.Assembly:
                  ReadTable( retVal.AssemblyDefinitions, curTable, tableSizes, i =>
                  {
                     var assDef = new AssemblyDefinition()
                     {
                        HashAlgorithm = (AssemblyHashAlgorithm) stream.ReadU32( tmpArray ),
                        AssemblyInformation = new AssemblyInformation()
                        {
                           VersionMajor = stream.ReadU16( tmpArray ),
                           VersionMinor = stream.ReadU16( tmpArray ),
                           VersionBuild = stream.ReadU16( tmpArray ),
                           VersionRevision = stream.ReadU16( tmpArray ),
                        },
                        Attributes = (AssemblyFlags) stream.ReadU32( tmpArray )
                     };
                     assDef.AssemblyInformation.PublicKeyOrToken = blobs.ReadBLOB( stream );
                     assDef.AssemblyInformation.Name = sysStrings.ReadSysString( stream );
                     assDef.AssemblyInformation.Culture = sysStrings.ReadSysString( stream );
                     return assDef;
                  } );
                  break;
               case Tables.AssemblyRef:
                  ReadTable( retVal.AssemblyReferences, curTable, tableSizes, i =>
                  {
                     var assRef = new AssemblyReference()
                     {
                        AssemblyInformation = new AssemblyInformation()
                        {
                           VersionMajor = stream.ReadU16( tmpArray ),
                           VersionMinor = stream.ReadU16( tmpArray ),
                           VersionBuild = stream.ReadU16( tmpArray ),
                           VersionRevision = stream.ReadU16( tmpArray )
                        },
                        Attributes = (AssemblyFlags) stream.ReadU32( tmpArray )
                     };
                     assRef.AssemblyInformation.PublicKeyOrToken = blobs.ReadBLOB( stream );
                     assRef.AssemblyInformation.Name = sysStrings.ReadSysString( stream );
                     assRef.AssemblyInformation.Culture = sysStrings.ReadSysString( stream );
                     assRef.HashValue = blobs.ReadBLOB( stream );
                     return assRef;
                  } );
                  break;
               case Tables.File:
                  ReadTable( retVal.FileReferences, curTable, tableSizes, i =>
                     new FileReference()
                     {
                        Attributes = (FileAttributes) stream.ReadU32( tmpArray ),
                        Name = sysStrings.ReadSysString( stream ),
                        HashValue = blobs.ReadBLOB( stream )
                     } );
                  break;
               case Tables.ExportedType:
                  ReadTable( retVal.ExportedTypess, curTable, tableSizes, i =>
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
                  ReadTable( retVal.ManifestResources, curTable, tableSizes, i =>
                     new ManifestResource()
                     {
                        Offset = stream.ReadU32( tmpArray ),
                        Attributes = (ManifestResourceAttributes) stream.ReadU32( tmpArray ),
                        Name = sysStrings.ReadSysString( stream ),
                        Implementation = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.Implementation, tRefSizes, tmpArray, false )
                     } );
                  break;
               case Tables.NestedClass:
                  ReadTable( retVal.NestedClassDefinitions, curTable, tableSizes, i =>
                     new NestedClassDefinition()
                     {
                        NestedClass = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray ),
                        EnclosingClass = MetaDataConstants.ReadSimpleTableIndex( stream, Tables.TypeDef, tableSizes, tmpArray )
                     } );
                  break;
               case Tables.GenericParameter:
                  ReadTable( retVal.GenericParameterDefinitions, curTable, tableSizes, i =>
                     new GenericParameterDefinition()
                     {
                        GenericParameterIndex = stream.ReadI16( tmpArray ),
                        Attributes = (GenericParameterAttributes) stream.ReadU16( tmpArray ),
                        Owner = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.TypeOrMethodDef, tRefSizes, tmpArray ).Value,
                        Name = sysStrings.ReadSysString( stream )
                     } );
                  break;
               case Tables.MethodSpec:
                  ReadTable( retVal.MethodSpecifications, curTable, tableSizes, i =>
                     new MethodSpecification()
                     {
                        Method = MetaDataConstants.ReadCodedTableIndex( stream, CodedTableIndexKind.MethodDefOrRef, tRefSizes, tmpArray ).Value,
                        Signature = GenericMethodSignature.ReadFromBytes( blobs.WholeBLOBArray, blobs.GetBLOBIndex( stream ) )
                     } );
                  break;
               case Tables.GenericParameterConstraint:
                  ReadTable( retVal.GenericParameterConstraintDefinitions, curTable, tableSizes, i =>
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
         for ( var i = 0; i < methodDefRVAs.Length; ++i )
         {
            var rva = methodDefRVAs[i];
            if ( rva != 0 )
            {
               var offset = ResolveRVA( rva, sections );
               if ( offset < stream.Length )
               {
                  stream.SeekFromBegin( offset );
                  retVal.MethodDefinitions[i].IL = ReadMethodILDefinition( stream, userStrings );
               }
            }
         }

         // Read all field RVA content
         for ( var i = 0; i < fieldDefRVAs.Length; ++i )
         {
            var offset = ResolveRVA( fieldDefRVAs[i], sections );
            UInt32 size;
            if (
               TryCalculateFieldTypeSize( retVal, retVal.FieldRVAs[i].Field.Index, out size )
               && offset + size < stream.Length
               )
            {
               // Sometimes there are field RVAs that are unresolvable...
               var bytes = new Byte[size];
               stream.SeekFromBegin( offset );
               stream.ReadWholeArray( bytes );
               retVal.FieldRVAs[i].Data = bytes;
            }

         }
         return retVal;
      }

      private static void ReadTable<T>( IList<T> tableArray, Int32 curTable, Int32[] tableSizes, Func<Int32, T> rowReader )
      {
         // TODO - calculate table width, thus reading whole table into single array
         // Then give array as argument to rowReader
         // However, stream buffers things really well - is this really necessary? Not sure if performance boost will be worth it, and probably not good thing memory-wise if big tables are present.
         var len = tableSizes[curTable];

         for ( var i = 0; i < len; ++i )
         {
            tableArray.Add( rowReader( i ) );
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

      private static Boolean TryCalculateFieldTypeSize( CILMetaData md, Int32 fieldIdx, out UInt32 size, Boolean onlySimpleTypeValid = false )
      {
         var retVal = fieldIdx < md.FieldDefinitions.Count;
         size = 0u;
         if ( retVal )
         {
            var fieldSig = md.FieldDefinitions[fieldIdx].Signature;
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
                     if ( typeIdx.Table == Tables.TypeDef )
                     {
                        // Only possible for types defined in this module
                        TableIndex? extendInfo;
                        var enumValueFieldIndex = GetEnumValueFieldIndex( md, typeIdx.Index, out extendInfo );
                        if ( enumValueFieldIndex >= 0 )
                        {
                           retVal = TryCalculateFieldTypeSize( md, enumValueFieldIndex, out size, true ); // Last parameter true to prevent possible infinite recursion in case of malformed metadata
                        }
                        else if ( extendInfo.Value.Table == Tables.TypeDef )
                        {
                           var cilIdx = md.ClassLayouts.GetReferencingRowsFromOrdered( Tables.TypeDef, typeIdx.Index, row => row.Parent );
                           if ( cilIdx.Any() )
                           {
                              var first = cilIdx.First();
                              retVal = first < md.ClassLayouts.Count;
                              if ( retVal )
                              {
                                 size = (UInt32) md.ClassLayouts[first].ClassSize;
                              }
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
         Int32 tDefIndex,
         out TableIndex? extendInfo
         )
      {
         // Only possible for types defined in this module
         var typeRow = md.TypeDefinitions.GetOrNull( tDefIndex );
         var retVal = -1;
         if ( typeRow != null )
         {
            extendInfo = typeRow.BaseType;
            if ( extendInfo.HasValue )
            {
               var isEnum = IsEnum( md, extendInfo );
               if ( isEnum )
               {
                  // First non-static field of enum type is the field containing enum value
                  var fieldStartIdx = typeRow.FieldList.Index;
                  var fieldEndIdx = tDefIndex + 1 >= md.TypeDefinitions.Count ?
                     md.FieldDefinitions.Count :
                     md.TypeDefinitions[tDefIndex + 1].FieldList.Index;
                  for ( var i = fieldStartIdx; i < fieldEndIdx; ++i )
                  {
                     if ( !md.FieldDefinitions[i].Attributes.IsStatic() )
                     {
                        // We have found non-static field of the enum type -> this field should be primitive and the size thus calculable
                        retVal = i;
                        break;
                     }
                  }
               }
            }
         }
         else
         {
            extendInfo = null;
         }

         return retVal;
      }

      private static Boolean IsEnum( CILMetaData md, TableIndex? tIdx )
      {
         return IsSystemType( md, tIdx, Consts.ENUM_NAMESPACE, Consts.ENUM_TYPENAME );
      }

      private static Boolean IsSystemType( CILMetaData md, TableIndex? tIdx, String systemNS, String systemTN )
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
               result = idx < md.TypeDefinitions.Count;
               if ( result )
               {
                  tn = md.TypeDefinitions[idx].Name;
                  ns = md.TypeDefinitions[idx].Namespace;
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
                  tn = md.TypeReferences[idx].Name;
                  ns = md.TypeReferences[idx].Namespace;
               }
            }

            result = String.Equals( tn, systemTN ) && String.Equals( ns, systemNS );
         }
         return result;
      }

      private static Object ReadConstantValue( BLOBContainer blobContainer, Stream stream, SignatureElementTypes constType )
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

      internal static MethodILDefinition ReadMethodILDefinition( System.IO.Stream stream, UserStringContainer userStrings )
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
            retVal.InitLocals = false;
         }
         else
         {
            stream.SeekFromCurrent( -1 );

            b = stream.ReadU16( tmpArray );
            var flags = (MethodHeaderFlags) ( b & FLAG_MASK );
            retVal.InitLocals = ( flags & MethodHeaderFlags.InitLocals ) != 0;
            var headerSize = ( b >> 12 ) * 4; // Header size is written as amount of integers
            // Skip max stack
            stream.SeekFromCurrent( 2 );
            var codeSize = stream.ReadI32( tmpArray );
            var localSigToken = stream.ReadI32( tmpArray );
            if ( localSigToken != 0 )
            {
               retVal.LocalsSignatureIndex = new TableIndex( localSigToken );
            }

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
                        ExceptionType = eType == ExceptionBlockType.Filter ? (TableIndex?) null : ReadExceptionType( stream, tmpArray ),
                        FilterOffset = eType == ExceptionBlockType.Filter ? stream.ReadI32( tmpArray ) : 0
                     } );
                     secByteSize -= ( isFat ? 24u : 12u );
                  }
               } while ( ( secFlags & MethodDataFlags.MoreSections ) != 0 );
            }
         }

         return retVal;
      }

      private static TableIndex? ReadExceptionType( Stream stream, Byte[] tmpArray )
      {
         var token = stream.ReadI32( tmpArray );
         return token == 0 ? (TableIndex?) null : new TableIndex( token );
      }

      private static void CreateOpCodes( MethodILDefinition methodIL, Stream stream, Int32 codeSize, Byte[] tmpArray, UserStringContainer userStrings )
      {
         var current = 0;
         var opCodes = methodIL.OpCodes;
         while ( current < codeSize )
         {
            var curInstruction = (Int32) stream.ReadByteFromStream();
            ++current;
            if ( curInstruction == OpCode.MAX_ONE_BYTE_INSTRUCTION )
            {
               curInstruction = ( curInstruction << 8 ) | (Int32) stream.ReadByteFromStream();
               ++current;
            }

            var code = OpCodes.Codes[(OpCodeEncoding) curInstruction];

            switch ( code.OperandType )
            {
               case OperandType.InlineNone:
                  opCodes.Add( OpCodes.CodeInfosWithNoOperand[(OpCodeEncoding) curInstruction] );
                  break;
               case OperandType.ShortInlineBrTarget:
               case OperandType.ShortInlineI:
                  current += sizeof( Byte );
                  opCodes.Add( new OpCodeInfoWithInt32( code, (Int32) ( (SByte) stream.ReadByteFromStream() ) ) );
                  break;
               case OperandType.ShortInlineVar:
                  current += sizeof( Byte );
                  opCodes.Add( new OpCodeInfoWithInt32( code, stream.ReadByteFromStream() ) );
                  break;
               case OperandType.ShortInlineR:
                  stream.ReadSpecificAmount( tmpArray, 0, sizeof( Single ) );
                  current += sizeof( Single );
                  opCodes.Add( new OpCodeInfoWithSingle( code, tmpArray.ReadSingleLEFromBytesNoRef( 0 ) ) );
                  break;
               case OperandType.InlineBrTarget:
               case OperandType.InlineI:
                  current += sizeof( Int32 );
                  opCodes.Add( new OpCodeInfoWithInt32( code, stream.ReadI32( tmpArray ) ) );
                  break;
               case OperandType.InlineVar:
                  current += sizeof( Int16 );
                  opCodes.Add( new OpCodeInfoWithInt32( code, stream.ReadU16( tmpArray ) ) );
                  break;
               case OperandType.InlineR:
                  current += sizeof( Double );
                  stream.ReadSpecificAmount( tmpArray, 0, sizeof( Double ) );
                  opCodes.Add( new OpCodeInfoWithDouble( code, tmpArray.ReadDoubleLEFromBytesNoRef( 0 ) ) );
                  break;
               case OperandType.InlineI8:
                  current += sizeof( Int64 );
                  stream.ReadSpecificAmount( tmpArray, 0, sizeof( Int64 ) );
                  opCodes.Add( new OpCodeInfoWithInt64( code, tmpArray.ReadInt64LEFromBytesNoRef( 0 ) ) );
                  break;
               case OperandType.InlineString:
                  current += sizeof( Int32 );
                  opCodes.Add( new OpCodeInfoWithString( code, userStrings.GetString( stream.ReadI32( tmpArray ) & TokenUtils.INDEX_MASK ) ) );
                  break;
               case OperandType.InlineField:
               case OperandType.InlineMethod:
               case OperandType.InlineType:
               case OperandType.InlineTok:
               case OperandType.InlineSig:
                  current += sizeof( Int32 );
                  opCodes.Add( new OpCodeInfoWithToken( code, new TableIndex( stream.ReadI32( tmpArray ) ) ) );
                  break;
               case OperandType.InlineSwitch:
                  var count = stream.ReadI32( tmpArray );
                  current += sizeof( Int32 ) + count * sizeof( Int32 );
                  var info = new OpCodeInfoWithSwitch( code, count );
                  for ( var i = 0; i < count; ++i )
                  {
                     info.Offsets.Add( stream.ReadI32( tmpArray ) );
                  }
                  opCodes.Add( info );
                  break;
               default:
                  throw new ArgumentException( "Unknown operand type: " + code.OperandType + " for " + code + "." );
            }
         }
      }


   }
}
