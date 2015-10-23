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
using CommonUtils;
using System.Threading;
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.Implementation;
using CILAssemblyManipulator.Physical.IO;

namespace CILAssemblyManipulator.Physical.Implementation
{
   internal static class ModuleWriter
   {
      //internal const Int32 MIN_FILE_ALIGNMENT = 0x200;
      //internal const UInt64 IMAGE_BASE_MULTIPLE = 0x10000; // ECMA-335, p. 279
      internal const UInt32 RELOCATION_PAGE_SIZE = 0x1000; // ECMA-335, p. 282
      internal const UInt32 RELOCATION_FIXUP_TYPE = 0x3; // ECMA-335, p. 282
      //internal const UInt64 DEFAULT_STACK_RESERVE = 0x100000; // ECMA-335, p. 280
      //internal const UInt64 DEFAULT_STACK_COMMIT = 0x1000; // ECMA-335, p. 280
      //internal const UInt64 DEFAULT_HEAP_RESERVE = 0x100000; // ECMA-335, p. 280
      //internal const UInt64 DEFAULT_HEAP_COMMIT = 0x1000; // ECMA-335, p. 280

      private const Int32 MD_MAX_VERSION_LENGTH = 255;
      private const Int32 MD_IDX = 0;
      private const Int32 SYS_STRINGS_IDX = 1;
      private const Int32 USER_STRINGS_IDX = 2;
      private const Int32 GUID_IDX = 3;
      private const Int32 BLOB_IDX = 4;
      private const Int32 HEAP_COUNT = 5;

      private const UInt32 MD_SIGNATURE = 0x424A5342;
      private const UInt16 MD_MAJOR = 1;
      private const UInt16 MD_MINOR = 1;
      private const UInt32 MD_RESERVED = 0;
      private const UInt16 MD_FLAGS = 0;

      private const Int32 TABLE_STREAM_RESERVED = 0;
      private const Byte TABLE_STREAM_RESERVED_2 = 1;

      // ECMA-335, p. 210
      //private static readonly ISet<Tables> SORTED_TABLES = new HashSet<Tables>(
      //   new Tables[] { Tables.ClassLayout, Tables.Constant, Tables.CustomAttribute, Tables.DeclSecurity, Tables.FieldLayout, Tables.FieldMarshal, Tables.FieldRVA, Tables.GenericParameter, Tables.GenericParameterConstraint, Tables.ImplMap, Tables.InterfaceImpl, Tables.MethodImpl, Tables.MethodSemantics, Tables.NestedClass }
      //   );
      private const Int64 SORTED_TABLES = 0x16003325FA00;


      internal const String CODE_SECTION_NAME = ".text";
      internal const String RESOURCE_SECTION_NAME = ".rsrc";
      internal const String RELOCATION_SECTION_NAME = ".reloc";

      private static readonly Encoding MetaDataStringEncoding = new UTF8Encoding( false, true );

      internal static void WriteToStream(
         WriterFunctionality writer,
         CILMetaData md,
         EmittingArguments eArgs,
         Stream sink
         )
      {
         // 1. Check arguments
         ArgumentValidator.ValidateNotNull( "Writer", writer );
         ArgumentValidator.ValidateNotNull( "Stream", sink );

         if ( eArgs == null )
         {
            eArgs = new EmittingArguments();
         }

         var headers = eArgs.Headers;

         if ( headers == null )
         {
            headers = new HeadersData( true );
         }

         Boolean isPE64, hasRelocations;
         UInt16 peOptionalHeaderSize;
         UInt32 numSections, iatSize;
         CheckHeaders( headers, out isPE64, out hasRelocations, out numSections, out peOptionalHeaderSize, out iatSize );

         var moduleKind = eArgs.ModuleKind;
         var isDLL = moduleKind.IsDLL();

         // 2. Initialize variables
         var fAlign = headers.FileAlignment;
         var sAlign = headers.SectionAlignment;
         var importHintName = headers.ImportHintName;
         if ( String.IsNullOrEmpty( importHintName ) )
         {
            importHintName = isDLL ? HeadersData.HINTNAME_FOR_DLL : HeadersData.HINTNAME_FOR_EXE;
         }
         var imageBase = headers.ImageBase;
         var strongName = eArgs.StrongName;


         var clrEntryPointToken = 0;
         var entryPoint = headers.CLREntryPointIndex;
         if ( entryPoint.HasValue )
         {
            clrEntryPointToken = entryPoint.Value.OneBasedToken;
         }



         // 3. Write module
         // Start emitting headers
         // MS-DOS header
         sink.Write( HeaderFieldOffsetsAndLengths.DOS_HEADER_AND_PE_SIG );

         // PE file header
         var byteArrayHelper = new ResizableArray<Byte>( HeaderFieldOffsetsAndLengths.PE_FILE_HEADER_SIZE );
         var characteristics = HeaderFieldPossibleValues.IMAGE_FILE_EXECUTABLE_IMAGE | ( isPE64 ? HeaderFieldPossibleValues.IMAGE_FILE_LARGE_ADDRESS_AWARE : HeaderFieldPossibleValues.IMAGE_FILE_32BIT_MACHINE );
         if ( isDLL )
         {
            characteristics |= HeaderFieldPossibleValues.IMAGE_FILE_DLL;
         }
         var idx = 0;
         byteArrayHelper
            .WriteUInt16LEToBytes( ref idx, (UInt16) headers.Machine )
            .WriteUInt16LEToBytes( ref idx, (UInt16) numSections )
            .WriteInt32LEToBytes( ref idx, Convert.ToInt32( DateTime.Now.Subtract( new DateTime( 1970, 1, 1, 0, 0, 0 ) ).TotalSeconds ) )
            .ZeroOut( ref idx, 8 )
            .WriteUInt16LEToBytes( ref idx, peOptionalHeaderSize )
            .WriteInt16LEToBytes( ref idx, (Int16) characteristics );
         sink.Write( byteArrayHelper.Array, idx );

         // PE optional header + section headers + padding + IAT + CLI header + Strong signature
         var codeSectionVirtualOffset = sAlign;
         // Strong name signature

         var useStrongName = strongName != null;
         var snSize = 0u;
         var snRVA = 0u;
         var snPadding = 0u;
         var aDefs = md.AssemblyDefinitions.TableContents;
         var thisAssemblyPublicKey = aDefs.Count > 0 ?
            aDefs[0].AssemblyInformation.PublicKeyOrToken :
            null;

         var delaySign = eArgs.DelaySign || ( !useStrongName && !thisAssemblyPublicKey.IsNullOrEmpty() );
         RSAParameters rParams;
         var signingAlgorithm = AssemblyHashAlgorithm.SHA1;
         var computingHash = useStrongName || delaySign;

         var cryptoCallbacks = eArgs.CryptoCallbacks;
         if ( useStrongName && cryptoCallbacks == null )
         {
            throw new InvalidOperationException( "Assembly should be strong-named, but the crypto callbacks are not provided." );
         }

         if ( computingHash )
         {
            // Set appropriate module flags
            headers.ModuleFlags |= ModuleFlags.StrongNameSigned;

            // Check algorithm override
            var algoOverride = eArgs.SigningAlgorithm;
            var algoOverrideWasInvalid = algoOverride.HasValue && ( algoOverride.Value == AssemblyHashAlgorithm.MD5 || algoOverride.Value == AssemblyHashAlgorithm.None );
            if ( algoOverrideWasInvalid )
            {
               algoOverride = AssemblyHashAlgorithm.SHA1;
            }

            Byte[] pkToProcess;
            if ( ( useStrongName && strongName.ContainerName != null ) || ( !useStrongName && delaySign ) )
            {
               if ( thisAssemblyPublicKey.IsNullOrEmpty() )
               {
                  thisAssemblyPublicKey = cryptoCallbacks.ExtractPublicKeyFromCSPContainerAndCheck( strongName.ContainerName );
               }
               pkToProcess = thisAssemblyPublicKey;
            }
            else
            {
               // Get public key from BLOB
               pkToProcess = strongName.KeyPair.ToArray();
            }

            // Create RSA parameters and process public key so that it will have proper, full format.
            Byte[] pk; String errorString;
            if ( CryptoUtils.TryCreateSigningInformationFromKeyBLOB( pkToProcess, algoOverride, out pk, out signingAlgorithm, out rParams, out errorString ) )
            {
               thisAssemblyPublicKey = pk;
               snSize = (UInt32) rParams.Modulus.Length;
            }
            else if ( thisAssemblyPublicKey != null && thisAssemblyPublicKey.Length == 16 ) // The "Standard Public Key", ECMA-335 p. 116
            {
               // TODO investigate this.
               snSize = 0x100;
            }
            else
            {
               throw new CryptographicException( errorString );
            }
         }
         else
         {
            rParams = default( RSAParameters );
         }

         //var hashStreamArgsForTokenComputing = signingAlgorithm == AssemblyHashAlgorithm.SHA1 ?
         //   hashStreamArgsForThisHashComputing :
         //   new Lazy<HashStreamLoadEventArgs>( () => eArgs.LaunchHashStreamEvent( AssemblyHashAlgorithm.SHA1, false ) );

         if ( useStrongName || delaySign )
         {
            snRVA = codeSectionVirtualOffset + iatSize + HeaderFieldOffsetsAndLengths.CLI_HEADER_SIZE;
            snPadding = BitUtils.MultipleOf4( snSize ) - snSize;
         }

         var revisitableOffset = HeaderFieldOffsetsAndLengths.DOS_HEADER_AND_PE_SIG.Length + HeaderFieldOffsetsAndLengths.PE_FILE_HEADER_SIZE;
         var revisitableArraySize = (Int32) ( fAlign + iatSize + HeaderFieldOffsetsAndLengths.CLI_HEADER_SIZE - revisitableOffset );
         // Cheat a bit - skip now, and re-visit it after all other emitting is done
         sink.Seek( revisitableArraySize + snSize + snPadding, SeekOrigin.Current );

         // First section
         // Start with method ILs
         // Current offset within section
         var writeData = new WritingData( md );
         var currentOffset = iatSize + snSize + snPadding + HeaderFieldOffsetsAndLengths.CLI_HEADER_SIZE;
         var mdStreamHandlers = writer.CreateStreamHandlers( writeData ).ToList();
         var usersStrings = mdStreamHandlers.FirstOfTypeOrAddDefault<AbstractWriterStreamHandler, WriterStringStreamHandler>(
            -1,
            mds => String.Equals( mds.StreamName, MetaDataConstants.USER_STRING_STREAM_NAME ),
            () => new DefaultWriterUserStringStreamHandler()
            );

         WriteMethodDefsIL(
            md,
            sink,
            codeSectionVirtualOffset,
            isPE64,
            byteArrayHelper,
            writeData.MethodRVAs,
            ref currentOffset,
            writer.CreateILHandler(),
            usersStrings
            );

         // Write manifest resources & field RVAs here
         UInt32 mResRVA, mResSize;
         WriteDataBeforeMD(
            md,
            writer.CreateManifestResourceHandler(),
            writer.CreateConstantsHandler(),
            sink,
            codeSectionVirtualOffset,
            isPE64,
            byteArrayHelper,
            writeData,
            out mResRVA,
            out mResSize,
            ref currentOffset
            );

         eArgs.MethodRVAs.Clear();
         eArgs.MethodRVAs.AddRange( writeData.MethodRVAs );
         eArgs.FieldRVAs.Clear();
         eArgs.FieldRVAs.AddRange( writeData.FieldRVAs );
         eArgs.EmbeddedManifestResourceOffsets.Clear();
         eArgs.EmbeddedManifestResourceOffsets.AddRange( writeData.EmbeddedManifestResourceOffsets );

         // Write metadata streams (tables & heaps)
         var mdRVA = codeSectionVirtualOffset + currentOffset;
         var mdSize = WriteMetaData(
            mdStreamHandlers,
            writeData,
            md,
            sink,
            headers,
            //eArgs,
            //usersStrings,
            byteArrayHelper,
            thisAssemblyPublicKey
            );
         currentOffset += mdSize;

         // Pad
         sink.SkipToNextAlignment( ref currentOffset, 0x4 );

         // Write debug header if present
         var dbgInfo = headers.DebugInformation;
         var dbgRVA = 0u;
         if ( dbgInfo != null )
         {
            dbgRVA = codeSectionVirtualOffset + currentOffset;
            var dbgData = dbgInfo.DebugData;
            byteArrayHelper.CurrentMaxCapacity = MetaDataConstants.DEBUG_DD_SIZE + dbgData.Length;
            idx = 0;
            byteArrayHelper.Array
               .WriteInt32LEToBytes( ref idx, dbgInfo.Characteristics )
               .WriteInt32LEToBytes( ref idx, dbgInfo.Timestamp )
               .WriteInt16LEToBytes( ref idx, dbgInfo.VersionMajor )
               .WriteInt16LEToBytes( ref idx, dbgInfo.VersionMinor )
               .WriteInt32LEToBytes( ref idx, dbgInfo.DebugType )
               .WriteInt32LEToBytes( ref idx, dbgData.Length )
               .WriteUInt32LEToBytes( ref idx, dbgRVA + MetaDataConstants.DEBUG_DD_SIZE )
               .WriteUInt32LEToBytes( ref idx, fAlign + currentOffset + (UInt32) idx + 4 ) // Pointer to data, end Debug Data Directory
               .BlockCopyFrom( ref idx, dbgData );
            sink.Write( byteArrayHelper.Array, idx );
            currentOffset += (UInt32) idx;
            sink.SkipToNextAlignment( ref currentOffset, 0x4 );
         }


         var entryPointCodeRVA = 0u;
         var importDirectoryRVA = 0u;
         var importDirectorySize = 0u;
         var hnRVA = 0u;
         Byte[] array;
         if ( hasRelocations )
         {
            var importDirectoryName = headers.ImportDirectoryName;
            importDirectoryRVA = codeSectionVirtualOffset + currentOffset;
            importDirectorySize = HeaderFieldOffsetsAndLengths.IMPORT_DIRECTORY_SIZE;
            var relocSize =
               HeaderFieldOffsetsAndLengths.IMPORT_DIRECTORY_SIZE // Import directory
               + HeaderFieldOffsetsAndLengths.ILT_SIZE // ILT
               + HeaderFieldOffsetsAndLengths.HINT_NAME_MIN_SIZE + (UInt32) importHintName.Length + 1 // Hint/name table
               + (UInt32) importDirectoryName.Length + 1 // Import directory name
               + 4 // Zero integer
               + 6 // PE entry point (jmp + operand)
               ;
            byteArrayHelper.CurrentMaxCapacity = (Int32) relocSize;
            hnRVA = importDirectoryRVA + HeaderFieldOffsetsAndLengths.IMPORT_DIRECTORY_SIZE + HeaderFieldOffsetsAndLengths.ILT_SIZE;
            array = byteArrayHelper.Array;
            idx = 0;
            entryPointCodeRVA = importDirectoryRVA + relocSize - 6;

            array
               // Import directory
               .WriteUInt32LEToBytes( ref idx, codeSectionVirtualOffset + currentOffset + (UInt32) HeaderFieldOffsetsAndLengths.IMPORT_DIRECTORY_SIZE ) // RVA of the ILT
               .WriteInt32LEToBytes( ref idx, 0 ) // DateTimeStamp
               .WriteInt32LEToBytes( ref idx, 0 ) // ForwarderChain
               .WriteUInt32LEToBytes( ref idx, codeSectionVirtualOffset + currentOffset + (UInt32) HeaderFieldOffsetsAndLengths.IMPORT_DIRECTORY_SIZE + HeaderFieldOffsetsAndLengths.ILT_SIZE + HeaderFieldOffsetsAndLengths.HINT_NAME_MIN_SIZE + (UInt32) importHintName.Length + 1 ) // RVA of Import Directory name (mscoree.dll)  
               .WriteUInt32LEToBytes( ref idx, codeSectionVirtualOffset ) // RVA of Import Address Table
               .ZeroOut( ref idx, 20 ) // The rest is zeroes

               // ILT
               .WriteUInt32LEToBytes( ref idx, codeSectionVirtualOffset + currentOffset + (UInt32) HeaderFieldOffsetsAndLengths.IMPORT_DIRECTORY_SIZE + (UInt32) HeaderFieldOffsetsAndLengths.ILT_SIZE )
               .ZeroOut( ref idx, 6 ) // Next entry is zeroes, and also zero the 2 first two bytes of hint/name table

               // Hint/Name table
               .WriteASCIIString( ref idx, importHintName, true )

               // Import Directory 
               .WriteASCIIString( ref idx, headers.ImportDirectoryName, true ) // Name

               // Zero integer (TODO investigate what is this)
               .ZeroOut( ref idx, 4 )

               // PE entrypoint
               .WriteInt16LEToBytes( ref idx, 0x25FF ) // jmp
               .WriteUInt32LEToBytes( ref idx, (UInt32) imageBase + codeSectionVirtualOffset ); // RVA of _CorDllMain/_CorExeMain in mscoree.dll

            sink.Write( array, idx );
            currentOffset += relocSize;
            System.Diagnostics.Debug.Assert( idx == relocSize );
         }

         // TODO Win32 resources section
         var hasResourceSection = false;

         var textSectionInfo = new SectionInfo( sink, null, currentOffset, sAlign, fAlign, !hasRelocations && !hasResourceSection );
         var prevSectionInfo = textSectionInfo;

         // TODO Win32 resources section
         var rsrcSectionInfo = new SectionInfo();

         // Final section - relocation section
         var relocSectionInfo = new SectionInfo();
         if ( hasRelocations )
         {
            // Need to build relocation fixup for the argument of the entry point
            currentOffset = 0;
            var relocRVA = entryPointCodeRVA + 2;
            var pageRVA = relocRVA & ~( RELOCATION_PAGE_SIZE - 1 );

            byteArrayHelper.CurrentMaxCapacity = HeaderFieldOffsetsAndLengths.RELOC_ARRAY_BASE_SIZE;
            array = byteArrayHelper.Array;
            idx = 0;
            array
               .WriteUInt32LEToBytes( ref idx, pageRVA )
               .WriteUInt32LEToBytes( ref idx, HeaderFieldOffsetsAndLengths.RELOC_ARRAY_BASE_SIZE ) // Block size
               .WriteUInt32LEToBytes( ref idx, ( RELOCATION_FIXUP_TYPE << 12 ) + relocRVA - pageRVA ); // Type (high 4 bits) + Offset (lower 12 bits) + dummy entry (16 bits)
            sink.Write( array, idx );
            currentOffset += (UInt32) idx;

            relocSectionInfo = new SectionInfo( sink, prevSectionInfo, currentOffset, sAlign, fAlign, true );
            prevSectionInfo = relocSectionInfo;
         }

         // Revisit PE optional header + section headers + padding + IAT + CLI header
         byteArrayHelper.CurrentMaxCapacity = revisitableArraySize;
         array = byteArrayHelper.Array;
         idx = 0;
         // PE optional header, ECMA-335 pp. 279-281
         // Standard fields
         array
            .WriteInt16LEToBytes( ref idx, isPE64 ? HeaderFieldPossibleValues.PE64 : HeaderFieldPossibleValues.PE32 ) // Magic
            .WriteByteToBytes( ref idx, headers.LinkerMajor ) // Linker major version
            .WriteByteToBytes( ref idx, headers.LinkerMinor ) // Linker minor version
            .WriteUInt32LEToBytes( ref idx, textSectionInfo.rawSize ) // Code size
            .WriteUInt32LEToBytes( ref idx, relocSectionInfo.rawSize + rsrcSectionInfo.rawSize ) // Initialized data size
            .WriteUInt32LEToBytes( ref idx, 0 ) // Unitialized data size
            .WriteUInt32LEToBytes( ref idx, entryPointCodeRVA ) // Entry point RVA
            .WriteUInt32LEToBytes( ref idx, textSectionInfo.virtualAddress ); // Base of code
         if ( !isPE64 )
         {
            array.WriteUInt32LEToBytes( ref idx, hasResourceSection ? rsrcSectionInfo.virtualAddress : relocSectionInfo.virtualAddress ); // Base of data
         }
         // WinNT-specific fields
         ( isPE64 ? array.WriteUInt64LEToBytes( ref idx, imageBase ) : array.WriteUInt32LEToBytes( ref idx, (UInt32) imageBase ) )
            .WriteUInt32LEToBytes( ref idx, sAlign ) // Section alignment
            .WriteUInt32LEToBytes( ref idx, fAlign ) // File alignment
            .WriteUInt16LEToBytes( ref idx, headers.OSMajor ) // OS Major
            .WriteUInt16LEToBytes( ref idx, headers.OSMinor ) // OS Minor
            .WriteUInt16LEToBytes( ref idx, headers.UserMajor ) // User Major
            .WriteUInt16LEToBytes( ref idx, headers.UserMinor ) // User Minor
            .WriteUInt16LEToBytes( ref idx, headers.SubSysMajor ) // SubSys Major
            .WriteUInt16LEToBytes( ref idx, headers.SubSysMinor ) // SubSys Minor
            .WriteUInt32LEToBytes( ref idx, 0 ) // Reserved
            .WriteUInt32LEToBytes( ref idx, prevSectionInfo.virtualAddress + BitUtils.MultipleOf( sAlign, prevSectionInfo.virtualSize ) ) // Image Size
            .WriteUInt32LEToBytes( ref idx, textSectionInfo.rawPointer ) // Header Size
            .WriteUInt32LEToBytes( ref idx, 0 ) // File Checksum
            .WriteUInt16LEToBytes( ref idx, (UInt16) ( headers.Subsystem ?? GetSubSystem( moduleKind ) ) ) // SubSystem
            .WriteUInt16LEToBytes( ref idx, (UInt16) headers.DLLFlags ); // DLL Characteristics
         if ( isPE64 )
         {
            array
               .WriteUInt64LEToBytes( ref idx, headers.StackReserve ) // Stack Reserve Size
               .WriteUInt64LEToBytes( ref idx, headers.StackCommit ) // Stack Commit Size
               .WriteUInt64LEToBytes( ref idx, headers.HeapReserve ) // Heap Reserve Size
               .WriteUInt64LEToBytes( ref idx, headers.HeapCommit ); // Heap Commit Size
         }
         else
         {
            array
               .WriteUInt32LEToBytes( ref idx, (UInt32) headers.StackReserve ) // Stack Reserve Size
               .WriteUInt32LEToBytes( ref idx, (UInt32) headers.StackCommit ) // Stack Commit Size
               .WriteUInt32LEToBytes( ref idx, (UInt32) headers.HeapReserve ) // Heap Reserve Size
               .WriteUInt32LEToBytes( ref idx, (UInt32) headers.HeapCommit ); // Heap Commit Size
         }
         array
            .WriteUInt32LEToBytes( ref idx, 0 ) // Loader Flags
            .WriteUInt32LEToBytes( ref idx, HeaderFieldOffsetsAndLengths.NUMBER_OF_DATA_DIRS )
            // Data Directories
            .WriteZeroDataDirectory( ref idx ) // Export Table
            .WriteDataDirectory( ref idx, importDirectoryRVA, importDirectorySize ) // Import Table
            .WriteDataDirectory( ref idx, rsrcSectionInfo.virtualAddress, rsrcSectionInfo.virtualSize ) // Resource Table
            .WriteZeroDataDirectory( ref idx ) // Exception Table
            .WriteZeroDataDirectory( ref idx ) // Certificate Table
            .WriteDataDirectory( ref idx, relocSectionInfo.virtualAddress, relocSectionInfo.virtualSize ) // BaseRelocationTable
            .WriteDataDirectory( ref idx, dbgRVA > 0u ? dbgRVA : 0u, dbgRVA > 0u ? MetaDataConstants.DEBUG_DD_SIZE : 0u ) // Debug Table
            .WriteZeroDataDirectory( ref idx ) // Copyright Table
            .WriteZeroDataDirectory( ref idx ) // Global Ptr
            .WriteZeroDataDirectory( ref idx ) // TLS Table
            .WriteZeroDataDirectory( ref idx ) // Load Config Table
            .WriteZeroDataDirectory( ref idx ) // Bound Import
            .WriteDataDirectory( ref idx, iatSize == 0 ? 0 : codeSectionVirtualOffset, iatSize == 0 ? 0 : iatSize ) // IAT
            .WriteZeroDataDirectory( ref idx ) // Delay Import Descriptor
            .WriteDataDirectory( ref idx, codeSectionVirtualOffset + iatSize, HeaderFieldOffsetsAndLengths.CLI_HEADER_SIZE ) // CLI Header
            .WriteZeroDataDirectory( ref idx ) // Reserved

            // Section headers
            .WriteSectionInfo( ref idx, textSectionInfo, CODE_SECTION_NAME, HeaderFieldPossibleValues.MEM_READ | HeaderFieldPossibleValues.MEM_EXECUTE | HeaderFieldPossibleValues.CONTAINS_CODE )
            .WriteSectionInfo( ref idx, rsrcSectionInfo, RESOURCE_SECTION_NAME, HeaderFieldPossibleValues.MEM_READ | HeaderFieldPossibleValues.CONTAINS_INITIALIZED_DATA )
            .WriteSectionInfo( ref idx, relocSectionInfo, RELOCATION_SECTION_NAME, HeaderFieldPossibleValues.MEM_READ | HeaderFieldPossibleValues.MEM_DISCARDABLE | HeaderFieldPossibleValues.CONTAINS_INITIALIZED_DATA );
         var headersSize = (UInt32) ( revisitableOffset + idx );

         // Skip to beginning of .text section
         array.ZeroOut( ref idx, (Int32) ( fAlign - (UInt32) revisitableOffset - idx ) );

         // Write IAT if needed
         if ( hasRelocations )
         {
            array
               .WriteUInt32LEToBytes( ref idx, hnRVA )
               .WriteUInt32LEToBytes( ref idx, 0 );
         }

         // CLI Header, ECMA-335, p. 283
         // At the moment, the 32BitRequired flag must be specified as well, if 32BitPreferred flag is specified.
         // This is for backwards compatibility.
         // Actually, since CorFlags lets specify Preferred32Bit separately, allow this to do too.
         var moduleFlags = headers.ModuleFlags;
         //if ( moduleFlags.HasFlag( ModuleFlags.Preferred32Bit ) )
         //{
         //   moduleFlags |= ModuleFlags.Required32Bit;
         //}
         array
            .WriteUInt32LEToBytes( ref idx, HeaderFieldOffsetsAndLengths.CLI_HEADER_SIZE ) // Cb
            .WriteUInt16LEToBytes( ref idx, headers.CLIMajor ) // MajorRuntimeVersion
            .WriteUInt16LEToBytes( ref idx, headers.CLIMinor ) // MinorRuntimeVersion
            .WriteDataDirectory( ref idx, mdRVA, mdSize ) // MetaData
            .WriteInt32LEToBytes( ref idx, (Int32) moduleFlags ) // Flags
            .WriteInt32LEToBytes( ref idx, clrEntryPointToken ) // EntryPointToken
            .WriteDataDirectory( ref idx, mResRVA, mResSize ); // Resources
         var snDataDirOffset = revisitableOffset + idx;
         array
            .WriteDataDirectory( ref idx, snRVA, snSize ) // StrongNameSignature
            .WriteZeroDataDirectory( ref idx ) // CodeManagerTable
            .WriteZeroDataDirectory( ref idx ) // VTableFixups
            .WriteZeroDataDirectory( ref idx ) // ExportAddressTableJumps
            .WriteZeroDataDirectory( ref idx ); // ManagedNativeHeader
#if DEBUG
         if ( idx != revisitableArraySize )
         {
            throw new Exception( "Something went wrong when emitting file headers. Emitted " + idx + " bytes, but was supposed to emit " + revisitableArraySize + " bytes." );
         }
#endif
         sink.Seek( revisitableOffset, SeekOrigin.Begin );
         sink.Write( array, idx );

         if ( computingHash )
         {
            if ( !delaySign )
            {

               using ( var rsa = ( strongName.ContainerName == null ? cryptoCallbacks.CreateRSAFromParameters( rParams ) : cryptoCallbacks.CreateRSAFromCSPContainer( strongName.ContainerName ) ) )
               {
                  var buffer = new Byte[0x2000]; // 2x typical windows page size
                  var hashEvtArgs = cryptoCallbacks.CreateHashStreamAndCheck( signingAlgorithm, true, true, false, true );
                  var hashStream = hashEvtArgs.CryptoStream;
                  var hashGetter = hashEvtArgs.HashGetter;
                  var transform = hashEvtArgs.Transform;

                  Byte[] strongNameArray;
                  using ( var tf = transform )
                  {
                     using ( var cryptoStream = hashStream() )
                     {
                        // Calculate hash of required parts of file (ECMA-335, p.117)
                        sink.Seek( 0, SeekOrigin.Begin );
                        sink.CopyStreamPart( cryptoStream, buffer, headersSize );

                        sink.Seek( fAlign, SeekOrigin.Begin );
                        sink.CopyStreamPart( cryptoStream, buffer, snRVA - codeSectionVirtualOffset );

                        sink.Seek( snSize + snPadding, SeekOrigin.Current );
                        sink.CopyStream( cryptoStream, buffer );
                     }

                     strongNameArray = cryptoCallbacks.CreateRSASignatureAndCheck( rsa, signingAlgorithm.GetAlgorithmName(), hashGetter() );
                  }


                  if ( snSize != strongNameArray.Length )
                  {
                     throw new CryptographicException( "Calculated and actual strong name size differ (calculated: " + snSize + ", actual: " + strongNameArray.Length + ")." );
                  }
                  Array.Reverse( strongNameArray );

                  // Write strong name
                  sink.Seek( snRVA - codeSectionVirtualOffset + fAlign, SeekOrigin.Begin );
                  sink.Write( strongNameArray );

                  eArgs.StrongNameHashValue = strongNameArray;
               }
            }

            byteArrayHelper.CurrentMaxCapacity = 8;
            idx = 0;
            array = byteArrayHelper.Array;
            array.WriteDataDirectory( ref idx, snRVA, snSize );
            sink.Seek( snDataDirOffset, SeekOrigin.Begin );
            sink.Write( array, 8 );
         }
      }

      private static void CheckHeaders(
         HeadersData eArgs,
         out Boolean isPE64,
         out Boolean hasRelocations,
         out UInt32 numSections,
         out UInt16 peOptionalHeaderSize,
         out UInt32 iatSize
         )
      {
         var machineEnum = eArgs.Machine;
         isPE64 = machineEnum.RequiresPE64();
         hasRelocations = machineEnum.RequiresRelocations();
         numSections = isPE64 ? 1u : 2u; // TODO win32-resource-section
         peOptionalHeaderSize = isPE64 ? HeaderFieldOffsetsAndLengths.PE_OPTIONAL_HEADER_SIZE_64 : HeaderFieldOffsetsAndLengths.PE_OPTIONAL_HEADER_SIZE_32;
         iatSize = hasRelocations ? HeaderFieldOffsetsAndLengths.IAT_SIZE : 0u; // No Import tables if no relocations
      }

      private static UInt64 CheckValueFor32PE( UInt64 value )
      {
         return Math.Min( UInt32.MaxValue, value );
      }

      private static Subsystem GetSubSystem( ModuleKind kind )
      {
         return ModuleKind.Windows == kind ? Subsystem.WindowsGUI : Subsystem.WindowsConsole;
      }



      private static void WriteMethodDefsIL(
         CILMetaData md,
         Stream sink,
         UInt32 codeSectionVirtualOffset,
         Boolean isPE64,
         ResizableArray<Byte> byteArrayHelper,
         List<Int32> methodRVAs,
         ref UInt32 currentOffset,
         WriterILHandler ilHandler,
         WriterStringStreamHandler userStrings
         )
      {
         // Create users string heap
         var mDefs = md.MethodDefinitions.TableContents;
         //methodRVAs.Clear();
         //methodRVAs.Capacity = mDefs.Count;

         for ( var i = 0; i < mDefs.Count; ++i )
         {
            var method = mDefs[i];
            UInt32 thisMethodRVA;
            var il = method.IL;
            if ( il != null )
            {
               Boolean isTinyHeader;
               var methodILByteCount = ilHandler.WriteMethodIL( byteArrayHelper, il, userStrings, out isTinyHeader );
               if ( !isTinyHeader )
               {
                  sink.SkipToNextAlignment( ref currentOffset, 4 );
               }

               sink.Write( byteArrayHelper.Array, methodILByteCount );
               thisMethodRVA = codeSectionVirtualOffset + currentOffset;
               currentOffset += (UInt32) methodILByteCount;
            }
            else
            {
               thisMethodRVA = 0u;
            }
            methodRVAs.Add( (Int32) thisMethodRVA );
         }

         // Write padding
         sink.SkipToNextAlignment( ref currentOffset, isPE64 ? 0x10u : 0x04u );

      }

      private static void WriteDataBeforeMD(
         CILMetaData md,
         WriterManifestResourceHandler resourceHandler,
         WriterConstantsHandler constsHandler,
         Stream sink,
         UInt32 codeSectionVirtualOffset,
         Boolean isPE64,
         ResizableArray<Byte> byteArrayHelper,
         WritingData writingData,
         out UInt32 resourcesRVA,
         out UInt32 resourcesSize,
         ref UInt32 currentOffset
         )
      {
         // Write manifest resources here
         var mResInfos = md.ManifestResources.TableContents;
         var mResInfo = writingData.EmbeddedManifestResourceOffsets;
         //mResInfo.Clear();
         //mResInfo.Capacity = mResInfos.Count;

         resourcesSize = 0u;
         foreach ( var mr in mResInfos )
         {
            if ( mr.IsEmbeddedResource() )
            {
               var data = mr.DataInCurrentFile;
               if ( data == null )
               {
                  data = Empty<Byte>.Array;
               }
               mResInfo.Add( (Int32) resourcesSize );
               var bytesWritten = resourceHandler.WriteEmbeddedManifestResource( byteArrayHelper, data );
               if ( bytesWritten > 0 )
               {
                  sink.Write( byteArrayHelper.Array, bytesWritten );
                  resourcesSize += (UInt32) bytesWritten;
               }
            }
            else
            {
               mResInfo.Add( null );
            }
         }

         if ( resourcesSize > 0 )
         {
            resourcesRVA = codeSectionVirtualOffset + currentOffset;
            // Write padding
            currentOffset += resourcesSize;
            sink.SkipToNextAlignment( ref currentOffset, isPE64 ? 0x10u : 0x04u );
         }
         else
         {
            resourcesRVA = 0u;
         }

         // Write constants here
         var mdFRVAs = md.FieldRVAs.TableContents;
         var fieldRVAs = writingData.FieldRVAs;
         //fieldRVAs.Clear();
         //fieldRVAs.Capacity = mdFRVAs.Count;
         foreach ( var fRVAInfo in mdFRVAs )
         {
            fieldRVAs.Add( (Int32) ( codeSectionVirtualOffset + currentOffset ) );

            var bytesWritten = constsHandler.WriteConstant( byteArrayHelper, fRVAInfo.Data );
            if ( bytesWritten > 0 )
            {
               sink.Write( byteArrayHelper.Array, bytesWritten );
               currentOffset += (UInt32) bytesWritten;
            }
         }

         sink.SkipToNextAlignment( ref currentOffset, 0x04u );
      }

      //This assumes that sink offset is at multiple of 4.
      private static UInt32 WriteMetaData(
         IList<AbstractWriterStreamHandler> streamHandlers,
         WritingData imageData,
         CILMetaData md,
         Stream sink,
         HeadersData headers,
         ResizableArray<Byte> byteArrayHelper,
         Byte[] thisAssemblyPublicKey
         )
      {
         var metaDataVersion = headers.MetaDataVersion;
         var versionStringSize = MetaDataStringEncoding.GetByteCount( metaDataVersion ) + 1;
         if ( versionStringSize > MD_MAX_VERSION_LENGTH )
         {
            throw new ArgumentException( "Metadata version must be at maximum " + MD_MAX_VERSION_LENGTH + " bytes long after encoding it using " + MetaDataStringEncoding + "." );
         }

         // ECMA-335, pp. 271-272
         var tableStream = streamHandlers.FirstOfTypeOrAddDefault<AbstractWriterStreamHandler, WriterTableStreamHandler>(
            0,
            null,
            () => new DefaultWriterTableStreamHandler( md, imageData, headers )
            );
         var blobs = streamHandlers.FirstOfTypeOrAddDefault<AbstractWriterStreamHandler, WriterBLOBStreamHandler>(
            -1,
            mds => String.Equals( MetaDataConstants.BLOB_STREAM_NAME, mds.StreamName ),
            () => new DefaultWriterBLOBStreamHandler()
            );
         var sysStrings = streamHandlers.FirstOfTypeOrAddDefault<AbstractWriterStreamHandler, WriterStringStreamHandler>(
            -1,
            mds => String.Equals( MetaDataConstants.SYS_STRING_STREAM_NAME, mds.StreamName ),
            () => new DefaultWriterSystemStringStreamHandler()
            );
         var guids = streamHandlers.FirstOfTypeOrAddDefault<AbstractWriterStreamHandler, WriterGUIDStreamHandler>(
            -1,
            mds => String.Equals( MetaDataConstants.GUID_STREAM_NAME, mds.StreamName ),
            () => new DefaultWriterGuidStreamHandler()
            );

         tableStream.FillHeaps(
            thisAssemblyPublicKey,
            blobs,
            sysStrings,
            guids,
            streamHandlers.Where( sh => !ReferenceEquals( sh, tableStream ) && !ReferenceEquals( sh, blobs ) && !ReferenceEquals( sh, sysStrings ) && !ReferenceEquals( sh, guids ) )
            );

         var versionStringSize4 = BitUtils.MultipleOf4( versionStringSize );
         var presentStreams = streamHandlers
            .Where( mds => mds.Accessed )
            .ToArray();

         var mdDirSize = 16 // Header start
            + versionStringSize4 // Version string
            + 4 // Header end
            + presentStreams.Select( mds => 8 + BitUtils.MultipleOf4( mds.StreamName.Length + 1 ) ).Sum(); // Stream headers
         var offset = 0;
         byteArrayHelper.CurrentMaxCapacity = mdDirSize;
         var array = byteArrayHelper.Array;
         // Write metadata root
         array.WriteUInt32LEToBytes( ref offset, MD_SIGNATURE ) // Signature
            .WriteUInt16LEToBytes( ref offset, MD_MAJOR ) // MD Major version
            .WriteUInt16LEToBytes( ref offset, MD_MINOR ) // MD Minor version
            .WriteUInt32LEToBytes( ref offset, MD_RESERVED ) // Reserved
            .WriteInt32LEToBytes( ref offset, versionStringSize4 ) // Version string length
            .WriteStringToBytes( ref offset, MetaDataStringEncoding, metaDataVersion ) // Version string
            .ZeroOut( ref offset, versionStringSize4 - versionStringSize + 1 ) // Padding
            .WriteByteToBytes( ref offset, 0 ) // TODO: StorageFlags to headers
            .WriteByteToBytes( ref offset, 0 ) // TODO: Reserved2
            .WriteUInt16LEToBytes( ref offset, (UInt16) presentStreams.Length ); // Amount of stream headers

         // Write stream headers
         var curMDStreamOffset = (UInt32) mdDirSize;
         foreach ( var stream in presentStreams )
         {
            var strmSize = (UInt32) stream.CurrentSize;
            array
               .WriteUInt32LEToBytes( ref offset, curMDStreamOffset )
               .WriteUInt32LEToBytes( ref offset, strmSize )
               .WriteStringToBytes( ref offset, MetaDataStringEncoding, stream.StreamName )
               .ZeroOut( ref offset, 4 - ( offset % 4 ) );
            curMDStreamOffset += strmSize;
         }

         // Write header
         System.Diagnostics.Debug.Assert( offset == mdDirSize );
         sink.Write( array, offset );

         // Write all streams
         foreach ( var stream in presentStreams )
         {
            stream.WriteStream( sink );
         }

         return curMDStreamOffset;
      }


   }

   internal static class HeaderFieldPossibleValues
   {
      #region PE Header, Characteristics
      public const Int16 IMAGE_FILE_RELOCS_STRIPPED = 0x0001;
      public const Int16 IMAGE_FILE_EXECUTABLE_IMAGE = 0x0002;
      public const Int16 IMAGE_FILE_32BIT_MACHINE = 0x0100;
      public const Int16 IMAGE_FILE_DLL = 0x2000;
      public const Int16 IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x0020;
      #endregion

      #region PE Optional Header, Magic
      public const Int16 PE32 = 0x010B;
      public const Int16 PE64 = 0x020B;
      #endregion

      #region Section Header, Characteristics
      public const UInt32 SCALE_INDEX = 0x0000001;
      public const UInt32 TYPE_NO_PAD = 0x0000008;
      public const UInt32 CONTAINS_CODE = 0x00000020;
      public const UInt32 CONTAINS_INITIALIZED_DATA = 0x00000040;
      public const UInt32 CONTAINS_UNINITIALIZED_DATA = 0x00000080;
      public const UInt32 LNK_OTHER = 0x0000100;
      public const UInt32 LNK_INFO = 0x000200;
      public const UInt32 LNK_REMOVE = 0x0000800;
      public const UInt32 LNK_COM_DATA = 0x00001000;
      public const UInt32 GP_REL = 0x00008000;
      public const UInt32 MEM_PURGEABLE = 0x00020000;
      public const UInt32 MEM_LOCKED = 0x00040000;
      public const UInt32 MEM_PRELOAD = 0x00080000;
      public const UInt32 ALIGN_1_BYTE = 0x00100000;
      public const UInt32 ALIGN_2_BYTES = 0x00200000;
      public const UInt32 ALIGN_4_BYTES = 0x00300000;
      public const UInt32 ALIGN_8_BYTES = 0x00400000;
      public const UInt32 ALIGN_16_BYTES = 0x00500000;
      public const UInt32 ALIGN_32_BYTES = 0x00600000;
      public const UInt32 ALIGN_64_BYTES = 0x00700000;
      public const UInt32 ALIGN_128_BYTES = 0x00800000;
      public const UInt32 ALIGN_256_BYTES = 0x00900000;
      public const UInt32 ALIGN_512_BYTES = 0x00a00000;
      public const UInt32 ALIGN_1024_BYTES = 0x00b00000;
      public const UInt32 ALIGN_2048_BYTES = 0x00c00000;
      public const UInt32 ALIGN_4096_BYTES = 0x00d00000;
      public const UInt32 ALIGN_8192_BYTES = 0x00e00000;
      public const UInt32 LNK_AND_RELOC_OVERFLOW = 0x01000000;
      public const UInt32 MEM_DISCARDABLE = 0x02000000;
      public const UInt32 MEM_NOT_CACHED = 0x04000000;
      public const UInt32 MEM_NOT_PAGED = 0x08000000;
      public const UInt32 MEM_SHARED = 0x10000000;
      public const UInt32 MEM_EXECUTE = 0x20000000;
      public const UInt32 MEM_READ = 0x40000000;
      public const UInt32 MEM_WRITE = 0x80000000;
      #endregion

      #region VTable fixup
      public const Int16 COR_VTABLE_32BIT = 0x0001;
      public const Int16 COR_VTABLE_64BIT = 0x0002;
      public const Int16 COR_VTABLE_FROM_UNMANAGED = 0x0004;
      public const Int16 COR_VTABLE_CALL_MOST_DERIVED = 0x0010;
      #endregion

   }

   internal static class HeaderFieldOffsetsAndLengths
   {
      #region MS-DOS header

      internal static readonly Byte[] DOS_HEADER_AND_PE_SIG = new Byte[] {
         0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00,
         0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, /* lfanew begin */ 0x80, 0x00, 0x00, 0x00, /* lfanew end */
         0x0E, 0x1F, 0xBA, 0x0E, 0x00, 0xB4, 0x09, 0xCD, 0x21, 0xB8, 0x01, 0x4C, 0xCD, 0x21, 0x54, 0x68,
         0x69, 0x73, 0x20, 0x70, 0x72, 0x6F, 0x67, 0x72, 0x61, 0x6D, 0x20, 0x63, 0x61, 0x6E, 0x6E, 0x6F, // is program canno
         0x74, 0x20, 0x62, 0x65, 0x20, 0x72, 0x75, 0x6E, 0x20, 0x69, 0x6E, 0x20, 0x44, 0x4F, 0x53, 0x20, // t be run in DOS 
         0x6D, 0x6F, 0x64, 0x65, 0x2E, 0x0D, 0x0D, 0x0A, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mode....$.......
         0x50, 0x45, 0x00, 0x00 // PE\0\0
      };

      #endregion

      #region PE file header

      internal const Int32 PE_FILE_HEADER_SIZE = 0x14;
      internal const Int32 NUM_SEC_OFFSET = 2;
      internal const Int32 TIMESTAMP_OFFSET = 4;
      internal const Int32 PE_OPTIONAL_HEADER_SIZE_OFFSET = 16;
      internal const Int32 PE_CHARACTERISTICS_OFFSET = 18;

      #endregion

      #region PE optional header
      internal const UInt16 PE_OPTIONAL_HEADER_SIZE_32 = 0x00E0;
      internal const UInt16 PE_OPTIONAL_HEADER_SIZE_64 = PE_OPTIONAL_HEADER_SIZE_32 + 0x0010;
      internal const UInt32 NUMBER_OF_DATA_DIRS = 0x10;
      #endregion

      #region Section header
      internal const Int32 SECTION_HEADER_SIZE = 0x28;
      #endregion

      #region IAT
      internal const UInt32 IAT_SIZE = 8;
      #endregion

      #region CIL Header
      internal const UInt32 CLI_HEADER_SIZE = 0x48;
      internal const Int32 CLI_HEADER_MAJOR_RUNTIME_VERSION = 0x2;
      internal const Int32 CLI_HEADER_MINOR_RUNTIME_VERSION = 0x5;
      #endregion

      #region Import Directory
      internal const Int32 HINT_NAME_MIN_SIZE = 2;
      internal const Int32 IMPORT_DIRECTORY_SIZE = 40;
      #endregion

      #region ILT
      internal const Int32 ILT_SIZE = 8;
      #endregion

      #region Relocation Section
      internal const Int32 RELOC_ARRAY_BASE_SIZE = 12;
      #endregion
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

}

public static partial class E_CILPhysical
{
   internal static Byte[] CreateFieldSignature( this ResizableArray<Byte> info, FieldSignature sig )
   {
      var idx = 0;
      info.WriteFieldSignature( ref idx, sig );
      return info.Array.CreateArrayCopy( idx );
   }

   internal static Byte[] CreateMethodSignature( this ResizableArray<Byte> info, AbstractMethodSignature sig )
   {
      var idx = 0;
      info.WriteMethodSignature( ref idx, sig );
      return info.Array.CreateArrayCopy( idx );
   }

   internal static Byte[] CreateMemberRefSignature( this ResizableArray<Byte> info, AbstractSignature sig )
   {
      return sig.SignatureKind == SignatureKind.Field ?
         info.CreateFieldSignature( (FieldSignature) sig ) :
         info.CreateMethodSignature( (MethodReferenceSignature) sig );
   }

   internal static Byte[] CreateConstantBytes( this ResizableArray<Byte> info, Object constant )
   {
      var idx = 0;
      info.WriteConstantValue( ref idx, constant );
      return info.Array.CreateArrayCopy( idx );
   }

   internal static Byte[] CreateCustomAttributeSignature( this ResizableArray<Byte> info, CILMetaData md, Int32 caIdx )
   {
      var sig = md.CustomAttributeDefinitions.TableContents[caIdx].Signature;
      Byte[] retVal;
      if ( sig != null ) // sig.TypedArguments.Count > 0 || sig.NamedArguments.Count > 0 )
      {
         var sigg = sig as CustomAttributeSignature;
         if ( sigg != null )
         {
            var idx = 0;
            info.WriteCustomAttributeSignature( ref idx, md, caIdx );
            retVal = info.Array.CreateArrayCopy( idx );
         }
         else
         {
            retVal = ( (RawCustomAttributeSignature) sig ).Bytes;
         }
      }
      else
      {
         // Signature missing
         retVal = null;
      }
      return retVal;
   }

   internal static Byte[] CreateMarshalSpec( this ResizableArray<Byte> info, MarshalingInfo marshal )
   {
      var idx = 0;
      info.WriteMarshalInfo( ref idx, marshal );
      return info.Array.CreateArrayCopy( idx );
   }

   internal static Byte[] CreateSecuritySignature( this ResizableArray<Byte> info, SecurityDefinition security, ResizableArray<Byte> aux )
   {
      var idx = 0;
      info.WriteSecuritySignature( ref idx, security, aux );
      return info.Array.CreateArrayCopy( idx );
   }

   internal static Byte[] CreateStandaloneSignature( this ResizableArray<Byte> info, StandaloneSignature standaloneSig )
   {
      var sig = standaloneSig.Signature;
      var locals = sig as LocalVariablesSignature;
      var idx = 0;
      if ( locals != null )
      {
         if ( standaloneSig.StoreSignatureAsFieldSignature && locals.Locals.Count > 0 )
         {
            info
               .AddSigStarterByte( ref idx, SignatureStarters.Field )
               .WriteLocalSignature( ref idx, locals.Locals[0] );
         }
         else
         {
            info.WriteLocalsSignature( ref idx, locals );
         }
      }
      else
      {
         var raw = sig as RawSignature;
         if ( raw != null )
         {
            info.WriteArray( ref idx, raw.Bytes );
         }
         else
         {
            info.WriteMethodSignature( ref idx, sig as AbstractMethodSignature );
         }
      }

      return idx == 0 ? null : info.Array.CreateArrayCopy( idx );
   }

   internal static Byte[] CreatePropertySignature( this ResizableArray<Byte> info, PropertySignature sig )
   {
      var idx = 0;
      info.WritePropertySignature( ref idx, sig );
      return info.Array.CreateArrayCopy( idx );
   }

   internal static Byte[] CreateTypeSignature( this ResizableArray<Byte> info, TypeSignature sig )
   {
      var idx = 0;
      info.WriteTypeSignature( ref idx, sig );
      return info.Array.CreateArrayCopy( idx );
   }

   internal static Byte[] CreateMethodSpecSignature( this ResizableArray<Byte> info, GenericMethodSignature sig )
   {
      var idx = 0;
      info.WriteMethodSpecSignature( ref idx, sig );
      return info.Array.CreateArrayCopy( idx );
   }

   private static void WriteFieldSignature( this ResizableArray<Byte> info, ref Int32 idx, FieldSignature sig )
   {
      if ( sig != null )
      {
         info
            .AddSigStarterByte( ref idx, SignatureStarters.Field )
            .WriteCustomModifiers( ref idx, sig.CustomModifiers )
            .WriteTypeSignature( ref idx, sig.Type );
      }
   }

   private static ResizableArray<Byte> WriteCustomModifiers( this ResizableArray<Byte> info, ref Int32 idx, IList<CustomModifierSignature> mods )
   {
      if ( mods.Count > 0 )
      {
         foreach ( var mod in mods )
         {
            info
               .AddSigByte( ref idx, mod.IsOptional ? SignatureElementTypes.CModOpt : SignatureElementTypes.CModReqd )
               .AddTDRSToken( ref idx, mod.CustomModifierType );
         }
      }
      return info;
   }

   private static ResizableArray<Byte> WriteTypeSignature( this ResizableArray<Byte> info, ref Int32 idx, TypeSignature type )
   {
      switch ( type.TypeSignatureKind )
      {
         case TypeSignatureKind.Simple:
            info.AddSigByte( ref idx, ( (SimpleTypeSignature) type ).SimpleType );
            break;
         case TypeSignatureKind.SimpleArray:
            var szArray = (SimpleArrayTypeSignature) type;
            info
               .AddSigByte( ref idx, SignatureElementTypes.SzArray )
               .WriteCustomModifiers( ref idx, szArray.CustomModifiers )
               .WriteTypeSignature( ref idx, szArray.ArrayType );
            break;
         case TypeSignatureKind.ComplexArray:
            var array = (ComplexArrayTypeSignature) type;
            info
               .AddSigByte( ref idx, SignatureElementTypes.Array )
               .WriteTypeSignature( ref idx, array.ArrayType )
               .AddCompressedUInt32( ref idx, array.Rank )
               .AddCompressedUInt32( ref idx, array.Sizes.Count );
            foreach ( var size in array.Sizes )
            {
               info.AddCompressedUInt32( ref idx, size );
            }
            info.AddCompressedUInt32( ref idx, array.LowerBounds.Count );
            foreach ( var lobo in array.LowerBounds )
            {
               info.AddCompressedInt32( ref idx, lobo );
            }
            break;
         case TypeSignatureKind.ClassOrValue:
            var clazz = (ClassOrValueTypeSignature) type;
            var gArgs = clazz.GenericArguments;
            var isGenericType = gArgs.Count > 0;
            if ( isGenericType )
            {
               info.AddSigByte( ref idx, SignatureElementTypes.GenericInst );
            }
            info
               .AddSigByte( ref idx, clazz.IsClass ? SignatureElementTypes.Class : SignatureElementTypes.ValueType )
               .AddTDRSToken( ref idx, clazz.Type );
            if ( isGenericType )
            {
               info.AddCompressedUInt32( ref idx, gArgs.Count );
               foreach ( var gArg in gArgs )
               {
                  info.WriteTypeSignature( ref idx, gArg );
               }
            }
            break;
         case TypeSignatureKind.GenericParameter:
            var gParam = (GenericParameterTypeSignature) type;
            info
               .AddSigByte( ref idx, gParam.IsTypeParameter ? SignatureElementTypes.Var : SignatureElementTypes.MVar )
               .AddCompressedUInt32( ref idx, gParam.GenericParameterIndex );
            break;
         case TypeSignatureKind.FunctionPointer:
            info
               .AddSigByte( ref idx, SignatureElementTypes.FnPtr )
               .WriteMethodSignature( ref idx, ( (FunctionPointerTypeSignature) type ).MethodSignature );
            break;
         case TypeSignatureKind.Pointer:
            var ptr = (PointerTypeSignature) type;
            info
               .AddSigByte( ref idx, SignatureElementTypes.Ptr )
               .WriteCustomModifiers( ref idx, ptr.CustomModifiers )
               .WriteTypeSignature( ref idx, ptr.PointerType );
            break;

      }
      return info;
   }

   private static ResizableArray<Byte> WriteMethodSignature( this ResizableArray<Byte> info, ref Int32 idx, AbstractMethodSignature method )
   {
      if ( method != null )
      {
         var starter = method.SignatureStarter;
         info.AddSigStarterByte( ref idx, method.SignatureStarter );

         if ( starter.IsGeneric() )
         {
            info.AddCompressedUInt32( ref idx, method.GenericArgumentCount );
         }

         info
            .AddCompressedUInt32( ref idx, method.Parameters.Count )
            .WriteParameterSignature( ref idx, method.ReturnType );

         foreach ( var param in method.Parameters )
         {
            info.WriteParameterSignature( ref idx, param );
         }

         if ( method.SignatureKind == SignatureKind.MethodReference )
         {
            var mRef = (MethodReferenceSignature) method;
            if ( mRef.VarArgsParameters.Count > 0 )
            {
               info.AddSigByte( ref idx, SignatureElementTypes.Sentinel );
               foreach ( var v in mRef.VarArgsParameters )
               {
                  info.WriteParameterSignature( ref idx, v );
               }
            }
         }
      }
      return info;
   }

   private static ResizableArray<Byte> WriteParameterSignature( this ResizableArray<Byte> info, ref Int32 idx, ParameterSignature parameter )
   {
      info
         .WriteCustomModifiers( ref idx, parameter.CustomModifiers );
      if ( SimpleTypeSignature.TypedByRef.Equals( parameter.Type ) )
      {
         info.AddSigByte( ref idx, SignatureElementTypes.TypedByRef );
      }
      else
      {
         if ( parameter.IsByRef )
         {
            info.AddSigByte( ref idx, SignatureElementTypes.ByRef );
         }

         info.WriteTypeSignature( ref idx, parameter.Type );
      }
      return info;
   }

   private static void WriteConstantValue( this ResizableArray<Byte> info, ref Int32 idx, Object constant )
   {
      if ( constant == null )
      {
         info.WriteInt32LEToBytes( ref idx, 0 );
      }
      else
      {
         switch ( Type.GetTypeCode( constant.GetType() ) )
         {
            case TypeCode.Boolean:
               info.WriteByteToBytes( ref idx, Convert.ToBoolean( constant ) ? (Byte) 1 : (Byte) 0 );
               break;
            case TypeCode.SByte:
               info.WriteSByteToBytes( ref idx, Convert.ToSByte( constant ) );
               break;
            case TypeCode.Byte:
               info.WriteByteToBytes( ref idx, Convert.ToByte( constant ) );
               break;
            case TypeCode.Char:
               info.WriteUInt16LEToBytes( ref idx, Convert.ToUInt16( Convert.ToChar( constant ) ) );
               break;
            case TypeCode.Int16:
               info.WriteInt16LEToBytes( ref idx, Convert.ToInt16( constant ) );
               break;
            case TypeCode.UInt16:
               info.WriteUInt16LEToBytes( ref idx, Convert.ToUInt16( constant ) );
               break;
            case TypeCode.Int32:
               info.WriteInt32LEToBytes( ref idx, Convert.ToInt32( constant ) );
               break;
            case TypeCode.UInt32:
               info.WriteUInt32LEToBytes( ref idx, Convert.ToUInt32( constant ) );
               break;
            case TypeCode.Int64:
               info.WriteInt64LEToBytes( ref idx, Convert.ToInt64( constant ) );
               break;
            case TypeCode.UInt64:
               info.WriteUInt64LEToBytes( ref idx, Convert.ToUInt64( constant ) );
               break;
            case TypeCode.Single:
               info.WriteSingleLEToBytes( ref idx, Convert.ToSingle( constant ) );
               break;
            case TypeCode.Double:
               info.WriteDoubleLEToBytes( ref idx, Convert.ToDouble( constant ) );
               break;
            case TypeCode.String:
               var str = Convert.ToString( constant );
               if ( str == null )
               {
                  info.WriteByteToBytes( ref idx, 0x00 );
               }
               else
               {
                  var size = MetaDataConstants.USER_STRING_ENCODING.GetByteCount( str );
                  info.EnsureThatCanAdd( idx, size );
                  idx += MetaDataConstants.USER_STRING_ENCODING.GetBytes( str, 0, str.Length, info.Array, idx );
               }
               break;
            default:
               info.WriteInt32LEToBytes( ref idx, 0 );
               break;
         }
      }
   }

   private static void WriteCustomAttributeSignature( this ResizableArray<Byte> info, ref Int32 idx, CILMetaData md, Int32 caIdx )
   {
      var ca = md.CustomAttributeDefinitions.TableContents[caIdx];
      var attrData = ca.Signature as CustomAttributeSignature;

      var ctor = ca.Type;
      var sig = ctor.Table == Tables.MethodDef ?
         md.MethodDefinitions.TableContents[ctor.Index].Signature :
         md.MemberReferences.TableContents[ctor.Index].Signature as AbstractMethodSignature;

      if ( sig == null )
      {
         throw new InvalidOperationException( "Custom attribute constructor signature was null (custom attribute at index " + caIdx + ", ctor: " + ctor + ")." );
      }
      else if ( sig.Parameters.Count != attrData.TypedArguments.Count )
      {
         throw new InvalidOperationException( "Custom attribute constructor has different amount of parameters than supplied custom attribute data (custom attribute at index " + caIdx + ", ctor: " + ctor + ")." );
      }


      // Prolog
      info
         .WriteByteToBytes( ref idx, 1 )
         .WriteByteToBytes( ref idx, 0 );

      // Fixed args
      for ( var i = 0; i < attrData.TypedArguments.Count; ++i )
      {
         var arg = attrData.TypedArguments[i];
         var caType = md.ResolveCACtorType( sig.Parameters[i].Type, tIdx => new CustomAttributeArgumentTypeEnum()
         {
            TypeString = "" // Type string doesn't matter, as values will be serialized directly...
         } );

         if ( caType == null )
         {
            // TODO some kind of warning system instead of throwing
            throw new InvalidOperationException( "Failed to resolve custom attribute type for constructor parameter (custom attribute at index " + caIdx + ", ctor: " + ctor + ", param: " + i + ")." );
         }
         info.WriteCustomAttributeFixedArg( ref idx, caType, arg.Value );
      }

      // Named args
      info.WriteUInt16LEToBytes( ref idx, (UInt16) attrData.NamedArguments.Count );
      foreach ( var arg in attrData.NamedArguments )
      {
         info.WriteCustomAttributeNamedArg( ref idx, arg );
      }
   }

   private static ResizableArray<Byte> WriteCustomAttributeFixedArg( this ResizableArray<Byte> info, ref Int32 idx, CustomAttributeArgumentType argType, Object arg )
   {
      switch ( argType.ArgumentTypeKind )
      {
         case CustomAttributeArgumentTypeKind.Array:
            if ( arg == null )
            {
               info.WriteInt32LEToBytes( ref idx, unchecked((Int32) 0xFFFFFFFF) );
            }
            else
            {
               var isDirectArray = arg is Array;
               Array array;
               if ( isDirectArray )
               {
                  array = (Array) arg;
                  argType = ( (CustomAttributeArgumentTypeArray) argType ).ArrayType;
               }
               else
               {
                  var indirectArray = (CustomAttributeValue_Array) arg;
                  array = indirectArray.Array;
                  argType = indirectArray.ArrayElementType;
               }

               info.WriteInt32LEToBytes( ref idx, array.Length );
               foreach ( var elem in array )
               {
                  info.WriteCustomAttributeFixedArg( ref idx, argType, elem );
               }
            }
            break;
         case CustomAttributeArgumentTypeKind.Simple:
            switch ( ( (CustomAttributeArgumentTypeSimple) argType ).SimpleType )
            {
               case SignatureElementTypes.Boolean:
                  info.WriteByteToBytes( ref idx, Convert.ToBoolean( arg ) ? (Byte) 1 : (Byte) 0 );
                  break;
               case SignatureElementTypes.I1:
                  info.WriteSByteToBytes( ref idx, Convert.ToSByte( arg ) );
                  break;
               case SignatureElementTypes.U1:
                  info.WriteByteToBytes( ref idx, Convert.ToByte( arg ) );
                  break;
               case SignatureElementTypes.Char:
                  info.WriteUInt16LEToBytes( ref idx, Convert.ToUInt16( Convert.ToChar( arg ) ) );
                  break;
               case SignatureElementTypes.I2:
                  info.WriteInt16LEToBytes( ref idx, Convert.ToInt16( arg ) );
                  break;
               case SignatureElementTypes.U2:
                  info.WriteUInt16LEToBytes( ref idx, Convert.ToUInt16( arg ) );
                  break;
               case SignatureElementTypes.I4:
                  info.WriteInt32LEToBytes( ref idx, Convert.ToInt32( arg ) );
                  break;
               case SignatureElementTypes.U4:
                  info.WriteUInt32LEToBytes( ref idx, Convert.ToUInt32( arg ) );
                  break;
               case SignatureElementTypes.I8:
                  info.WriteInt64LEToBytes( ref idx, Convert.ToInt64( arg ) );
                  break;
               case SignatureElementTypes.U8:
                  info.WriteUInt64LEToBytes( ref idx, Convert.ToUInt64( arg ) );
                  break;
               case SignatureElementTypes.R4:
                  info.WriteSingleLEToBytes( ref idx, Convert.ToSingle( arg ) );
                  break;
               case SignatureElementTypes.R8:
                  info.WriteDoubleLEToBytes( ref idx, Convert.ToDouble( arg ) );
                  break;
               case SignatureElementTypes.String:
                  info.AddCAString( ref idx, arg == null ? null : Convert.ToString( arg ) );
                  break;
               case SignatureElementTypes.Type:
                  String typeStr;
                  if ( arg != null )
                  {
                     if ( arg is CustomAttributeValue_TypeReference )
                     {
                        typeStr = ( (CustomAttributeValue_TypeReference) arg ).TypeString;
                     }
                     else if ( arg is Type )
                     {
                        typeStr = ( (Type) arg ).AssemblyQualifiedName;
                     }
                     else
                     {
                        typeStr = Convert.ToString( arg );
                     }
                  }
                  else
                  {
                     typeStr = null;
                  }
                  info.AddCAString( ref idx, typeStr );
                  break;
               case SignatureElementTypes.Object:
                  if ( arg == null )
                  {
                     // Nulls are serialized as null strings
                     if ( !CustomAttributeArgumentTypeSimple.String.Equals( argType ) )
                     {
                        argType = CustomAttributeArgumentTypeSimple.String;
                     }
                  }
                  else
                  {
                     argType = ResolveBoxedCAType( arg );
                  }
                  info
                     .WriteCustomAttributeFieldOrPropType( ref idx, ref argType, ref arg )
                     .WriteCustomAttributeFixedArg( ref idx, argType, arg );
                  break;
            }
            break;
         case CustomAttributeArgumentTypeKind.TypeString:
            if ( arg == null )
            {
               throw new InvalidOperationException( "Tried to serialize null as enum." );
            }
            // TODO check for invalid types (bool, char, single, double, string, any other non-primitive)
            var valueToWrite = arg is CustomAttributeValue_EnumReference ? ( (CustomAttributeValue_EnumReference) arg ).EnumValue : arg;
            info.WriteConstantValue( ref idx, valueToWrite );
            break;
      }

      return info;
   }

   private static CustomAttributeArgumentType ResolveBoxedCAType( Object arg, Boolean isWithinArray = false )
   {
      var argType = arg.GetType();
      if ( argType.IsEnum )
      {
         return new CustomAttributeArgumentTypeEnum()
         {
            TypeString = argType.AssemblyQualifiedName
         };
      }
      else
      {
         switch ( Type.GetTypeCode( argType ) )
         {
            case TypeCode.Boolean:
               return CustomAttributeArgumentTypeSimple.Boolean;
            case TypeCode.Char:
               return CustomAttributeArgumentTypeSimple.Char;
            case TypeCode.SByte:
               return CustomAttributeArgumentTypeSimple.SByte;
            case TypeCode.Byte:
               return CustomAttributeArgumentTypeSimple.Byte;
            case TypeCode.Int16:
               return CustomAttributeArgumentTypeSimple.Int16;
            case TypeCode.UInt16:
               return CustomAttributeArgumentTypeSimple.UInt16;
            case TypeCode.Int32:
               return CustomAttributeArgumentTypeSimple.Int32;
            case TypeCode.UInt32:
               return CustomAttributeArgumentTypeSimple.UInt32;
            case TypeCode.Int64:
               return CustomAttributeArgumentTypeSimple.Int64;
            case TypeCode.UInt64:
               return CustomAttributeArgumentTypeSimple.UInt64;
            case TypeCode.Single:
               return CustomAttributeArgumentTypeSimple.Single;
            case TypeCode.Double:
               return CustomAttributeArgumentTypeSimple.Double;
            case TypeCode.String:
               return CustomAttributeArgumentTypeSimple.String;
            case TypeCode.Object:
               if ( argType.IsArray )
               {
                  return isWithinArray ?
                     (CustomAttributeArgumentType) CustomAttributeArgumentTypeSimple.Object :
                     new CustomAttributeArgumentTypeArray()
                     {
                        ArrayType = ResolveBoxedCAType( argType.GetElementType(), true )
                     };
               }
               else
               {
                  // Check for enum reference
                  if ( Equals( typeof( CustomAttributeValue_EnumReference ), argType ) )
                  {
                     return new CustomAttributeArgumentTypeEnum()
                     {
                        TypeString = ( (CustomAttributeValue_EnumReference) arg ).EnumType
                     };
                  }
                  // System.Type or System.Object or CustomAttributeTypeReference
                  else if ( Equals( typeof( CustomAttributeValue_TypeReference ), argType ) || Equals( typeof( Type ), argType ) )
                  {
                     return CustomAttributeArgumentTypeSimple.Type;
                  }
                  else if ( isWithinArray && Equals( typeof( Object ), argType ) )
                  {
                     return CustomAttributeArgumentTypeSimple.Object;
                  }
                  else
                  {
                     throw new InvalidOperationException( "Failed to deduce custom attribute type for " + argType + "." );
                  }
               }
            default:
               throw new InvalidOperationException( "Failed to deduce custom attribute type for " + argType + "." );
         }
      }
   }

   private static ResizableArray<Byte> WriteCustomAttributeNamedArg( this ResizableArray<Byte> info, ref Int32 idx, CustomAttributeNamedArgument arg )
   {
      var elem = arg.IsField ? SignatureElementTypes.CA_Field : SignatureElementTypes.CA_Property;
      var typedValueValue = arg.Value.Value;
      var caType = arg.FieldOrPropertyType;
      return info
         .AddSigByte( ref idx, elem )
         .WriteCustomAttributeFieldOrPropType( ref idx, ref caType, ref typedValueValue )
         .AddCAString( ref idx, arg.Name )
         .WriteCustomAttributeFixedArg( ref idx, caType, typedValueValue );
   }

   private static ResizableArray<Byte> WriteCustomAttributeFieldOrPropType( this ResizableArray<Byte> info, ref Int32 idx, ref CustomAttributeArgumentType type, ref Object value, Boolean processEnumTypeAndValue = true )
   {
      if ( type == null )
      {
         throw new InvalidOperationException( "Custom attribute signature typed argument type was null." );
      }

      switch ( type.ArgumentTypeKind )
      {
         case CustomAttributeArgumentTypeKind.Array:
            var arrayType = ( (CustomAttributeArgumentTypeArray) type ).ArrayType;
            Object dummy = null;
            info
               .AddSigByte( ref idx, SignatureElementTypes.SzArray )
               .WriteCustomAttributeFieldOrPropType( ref idx, ref arrayType, ref dummy, false );
            break;
         case CustomAttributeArgumentTypeKind.Simple:
            var sigStarter = ( (CustomAttributeArgumentTypeSimple) type ).SimpleType;
            if ( sigStarter == SignatureElementTypes.Object )
            {
               sigStarter = SignatureElementTypes.CA_Boxed;
            }
            info.AddSigByte( ref idx, sigStarter );
            break;
         case CustomAttributeArgumentTypeKind.TypeString:
            info
               .AddSigByte( ref idx, SignatureElementTypes.CA_Enum )
               .AddCAString( ref idx, ( (CustomAttributeArgumentTypeEnum) type ).TypeString );
            if ( processEnumTypeAndValue )
            {
               if ( value == null )
               {
                  throw new InvalidOperationException( "Tried to serialize null as enum." );
               }
               else
               {
                  if ( value is CustomAttributeValue_EnumReference )
                  {
                     value = ( (CustomAttributeValue_EnumReference) value ).EnumValue;
                  }

                  switch ( Type.GetTypeCode( value.GetType() ) )
                  {
                     //case TypeCode.Boolean:
                     //   type = CustomAttributeArgumentTypeSimple.Boolean;
                     //   break;
                     //case TypeCode.Char:
                     //   type = CustomAttributeArgumentTypeSimple.Char;
                     //   break;
                     case TypeCode.SByte:
                        type = CustomAttributeArgumentTypeSimple.SByte;
                        break;
                     case TypeCode.Byte:
                        type = CustomAttributeArgumentTypeSimple.Byte;
                        break;
                     case TypeCode.Int16:
                        type = CustomAttributeArgumentTypeSimple.Int16;
                        break;
                     case TypeCode.UInt16:
                        type = CustomAttributeArgumentTypeSimple.UInt16;
                        break;
                     case TypeCode.Int32:
                        type = CustomAttributeArgumentTypeSimple.Int32;
                        break;
                     case TypeCode.UInt32:
                        type = CustomAttributeArgumentTypeSimple.UInt32;
                        break;
                     case TypeCode.Int64:
                        type = CustomAttributeArgumentTypeSimple.Int64;
                        break;
                     case TypeCode.UInt64:
                        type = CustomAttributeArgumentTypeSimple.UInt64;
                        break;
                     //case TypeCode.Single:
                     //   type = CustomAttributeArgumentTypeSimple.Single;
                     //   break;
                     //case TypeCode.Double:
                     //   type = CustomAttributeArgumentTypeSimple.Double;
                     //   break;
                     //case TypeCode.String:
                     //   type = CustomAttributeArgumentTypeSimple.String;
                     //break;
                     default:
                        throw new NotSupportedException( "The custom attribute type was marked to be enum, but the actual value's type was: " + value.GetType() + "." );
                  }
               }
            }
            break;
      }

      return info;
   }

   private static ResizableArray<Byte> WriteMarshalInfo( this ResizableArray<Byte> info, ref Int32 idx, MarshalingInfo marshal )
   {
      if ( marshal != null )
      {
         info.AddCompressedUInt32( ref idx, (Int32) marshal.Value );
         if ( !marshal.Value.IsNativeInstric() )
         {
            // Apparently Microsoft's implementation differs from ECMA-335 standard:
            // there the index of first parameter is 1, here all indices are zero-based.
            switch ( (UnmanagedType) marshal.Value )
            {
               case UnmanagedType.ByValTStr:
                  info.AddCompressedUInt32( ref idx, marshal.ConstSize );
                  break;
               case UnmanagedType.IUnknown:
               case UnmanagedType.IDispatch:
                  if ( marshal.IIDParameterIndex >= 0 )
                  {
                     info.AddCompressedUInt32( ref idx, marshal.IIDParameterIndex );
                  }
                  break;
               case UnmanagedType.SafeArray:
                  if ( marshal.SafeArrayType != VarEnum.VT_EMPTY )
                  {
                     info.AddCompressedUInt32( ref idx, (Int32) marshal.SafeArrayType );
                     if ( VarEnum.VT_USERDEFINED == marshal.SafeArrayType )
                     {
                        info.AddCAString( ref idx, marshal.SafeArrayUserDefinedType );
                     }
                  }
                  break;
               case UnmanagedType.ByValArray:
                  info.AddCompressedUInt32( ref idx, marshal.ConstSize );
                  if ( marshal.ArrayType != MarshalingInfo.NATIVE_TYPE_MAX )
                  {
                     info.AddCompressedUInt32( ref idx, (Int32) marshal.ArrayType );
                  }
                  break;
               case UnmanagedType.LPArray:
                  var hasSize = marshal.SizeParameterIndex != MarshalingInfo.NO_INDEX;
                  info
                     .AddCompressedUInt32( ref idx, (Int32) marshal.ArrayType )
                     .AddCompressedUInt32( ref idx, hasSize ? marshal.SizeParameterIndex : 0 );
                  if ( marshal.ConstSize != MarshalingInfo.NO_INDEX )
                  {
                     info
                        .AddCompressedUInt32( ref idx, marshal.ConstSize )
                        .AddCompressedUInt32( ref idx, hasSize ? 1 : 0 ); // Indicate whether size-parameter was specified
                  }
                  break;
               case UnmanagedType.CustomMarshaler:
                  // For some reason, there are two compressed ints at this point
                  info
                     .AddCompressedUInt32( ref idx, 0 )
                     .AddCompressedUInt32( ref idx, 0 )
                     .AddCAString( ref idx, marshal.MarshalType )
                     .AddCAString( ref idx, marshal.MarshalCookie ?? "" );
                  break;
               default:
                  break;
            }
         }
      }

      return info;
   }

   private static ResizableArray<Byte> WriteSecuritySignature(
      this ResizableArray<Byte> info,
      ref Int32 idx,
      SecurityDefinition security,
      ResizableArray<Byte> aux
      )
   {
      // TODO currently only newer format, .NET 1 format not supported for writing
      var permissions = security.PermissionSets;
      info
         .WriteByteToBytes( ref idx, MetaDataConstants.DECL_SECURITY_HEADER )
         .AddCompressedUInt32( ref idx, permissions.Count );
      foreach ( var sec in permissions )
      {
         info.AddCAString( ref idx, sec.SecurityAttributeType );
         var secInfo = sec as SecurityInformation;
         Byte[] secInfoBLOB;
         if ( secInfo != null )
         {
            // Store arguments in separate bytes
            var auxIdx = 0;
            foreach ( var arg in secInfo.NamedArguments )
            {
               aux.WriteCustomAttributeNamedArg( ref auxIdx, arg );
            }
            // Now write to sec blob
            secInfoBLOB = aux.Array.CreateArrayCopy( auxIdx );
            // The length of named arguments blob
            info
               .AddCompressedUInt32( ref idx, secInfoBLOB.Length + BitUtils.GetEncodedUIntSize( secInfo.NamedArguments.Count ) )
            // The amount of named arguments
               .AddCompressedUInt32( ref idx, secInfo.NamedArguments.Count );
         }
         else
         {
            secInfoBLOB = ( (RawSecurityInformation) sec ).Bytes;
            info.AddCompressedUInt32( ref idx, secInfoBLOB.Length );
         }

         info.WriteArray( ref idx, secInfoBLOB );
      }

      return info;
   }

   private static ResizableArray<Byte> WriteLocalsSignature(
      this ResizableArray<Byte> info,
      ref Int32 idx,
      LocalVariablesSignature sig
      )
   {
      if ( sig != null )
      {
         var locals = sig.Locals;
         info
            .AddSigStarterByte( ref idx, SignatureStarters.LocalSignature )
            .AddCompressedUInt32( ref idx, locals.Count );
         foreach ( var local in locals )
         {
            info.WriteLocalSignature( ref idx, local );
         }
      }
      return info;
   }

   private static ResizableArray<Byte> WriteLocalSignature(
      this ResizableArray<Byte> info,
      ref Int32 idx,
      LocalVariableSignature sig
      )
   {
      if ( SimpleTypeSignature.TypedByRef.Equals( sig.Type ) )
      {
         info.AddSigByte( ref idx, SignatureElementTypes.TypedByRef );
      }
      else
      {
         info.WriteCustomModifiers( ref idx, sig.CustomModifiers );
         if ( sig.IsPinned )
         {
            info.AddSigByte( ref idx, SignatureElementTypes.Pinned );
         }

         if ( sig.IsByRef )
         {
            info.AddSigByte( ref idx, SignatureElementTypes.ByRef );
         }
         info.WriteTypeSignature( ref idx, sig.Type );
      }

      return info;
   }

   private static ResizableArray<Byte> WritePropertySignature(
      this ResizableArray<Byte> info,
      ref Int32 idx,
      PropertySignature sig
      )
   {
      if ( sig != null )
      {
         var starter = SignatureStarters.Property;
         if ( sig.HasThis )
         {
            starter |= SignatureStarters.HasThis;
         }
         info
            .AddSigStarterByte( ref idx, starter )
            .AddCompressedUInt32( ref idx, sig.Parameters.Count )
            .WriteCustomModifiers( ref idx, sig.CustomModifiers )
            .WriteTypeSignature( ref idx, sig.PropertyType );

         foreach ( var param in sig.Parameters )
         {
            info.WriteParameterSignature( ref idx, param );
         }
      }
      return info;
   }

   private static ResizableArray<Byte> WriteMethodSpecSignature(
      this ResizableArray<Byte> info,
      ref Int32 idx,
      GenericMethodSignature sig
      )
   {
      info
         .AddSigStarterByte( ref idx, SignatureStarters.MethodSpecGenericInst )
         .AddCompressedUInt32( ref idx, sig.GenericArguments.Count );
      foreach ( var gArg in sig.GenericArguments )
      {
         info.WriteTypeSignature( ref idx, gArg );
      }
      return info;
   }

   private static ResizableArray<Byte> AddSigStarterByte( this ResizableArray<Byte> info, ref Int32 idx, SignatureStarters starter )
   {
      return info.WriteByteToBytes( ref idx, (Byte) starter );
   }

   private static ResizableArray<Byte> AddSigByte( this ResizableArray<Byte> info, ref Int32 idx, SignatureElementTypes sig )
   {
      return info.WriteByteToBytes( ref idx, (Byte) sig );
   }

   private static ResizableArray<Byte> AddTDRSToken( this ResizableArray<Byte> info, ref Int32 idx, TableIndex token )
   {
      return info.AddCompressedUInt32( ref idx, TableIndex.EncodeTypeDefOrRefOrSpec( token.OneBasedToken ) );
   }

   internal static ResizableArray<Byte> AddCompressedUInt32( this ResizableArray<Byte> info, ref Int32 idx, Int32 value )
   {
      info.EnsureThatCanAdd( idx, (Int32) BitUtils.GetEncodedUIntSize( value ) )
         .Array.CompressUInt32( ref idx, value );
      return info;
   }

   internal static ResizableArray<Byte> AddCompressedInt32( this ResizableArray<Byte> info, ref Int32 idx, Int32 value )
   {
      info.EnsureThatCanAdd( idx, (Int32) BitUtils.GetEncodedIntSize( value ) )
         .Array.CompressInt32( ref idx, value );
      return info;
   }

   private static ResizableArray<Byte> AddCAString( this ResizableArray<Byte> info, ref Int32 idx, String str )
   {
      if ( str == null )
      {
         info.WriteByteToBytes( ref idx, 0xFF );
      }
      else
      {
         var encoding = MetaDataConstants.SYS_STRING_ENCODING;
         var size = encoding.GetByteCount( str );
         info
            .AddCompressedUInt32( ref idx, size )
            .EnsureThatCanAdd( idx, size );
         idx += encoding.GetBytes( str, 0, str.Length, info.Array, idx );
      }
      return info;
   }

   internal static U FirstOfTypeOrAddDefault<T, U>( this IList<T> list, Int32 insertIdx, Func<U, Boolean> additionalFilter, Func<U> defaultFactory )
      where U : T
   {
      var correctTyped = list.OfType<U>();
      if ( additionalFilter != null )
      {
         correctTyped = correctTyped.Where( additionalFilter );
      }
      var retVal = correctTyped.FirstOrDefault();
      if ( retVal == null )
      {
         retVal = defaultFactory();
         if ( insertIdx >= 0 )
         {
            list.Insert( insertIdx, retVal );
         }
         else
         {
            list.Add( retVal );
         }
      }

      return retVal;
   }
}