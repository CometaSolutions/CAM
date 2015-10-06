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
         CILMetaData md,
         EmittingArguments eArgs,
         Stream sink
         )
      {
         // 1. Check arguments
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


         var byteArrayHelper = new ByteArrayHelper();

         // 3. Write module
         // Start emitting headers
         // MS-DOS header
         var curArrayLen = HeaderFieldOffsetsAndLengths.DOS_HEADER_AND_PE_SIG.Length;
         byteArrayHelper.EnsureSize( curArrayLen );
         var currentArray = byteArrayHelper.Array;
         Array.Copy( HeaderFieldOffsetsAndLengths.DOS_HEADER_AND_PE_SIG, currentArray, curArrayLen );
         sink.Write( currentArray, curArrayLen );

         // PE file header
         curArrayLen = HeaderFieldOffsetsAndLengths.PE_FILE_HEADER_SIZE;
         byteArrayHelper.EnsureSize( curArrayLen );
         currentArray = byteArrayHelper.Array;
         var characteristics = HeaderFieldPossibleValues.IMAGE_FILE_EXECUTABLE_IMAGE | ( isPE64 ? HeaderFieldPossibleValues.IMAGE_FILE_LARGE_ADDRESS_AWARE : HeaderFieldPossibleValues.IMAGE_FILE_32BIT_MACHINE );
         if ( isDLL )
         {
            characteristics |= HeaderFieldPossibleValues.IMAGE_FILE_DLL;
         }
         var idx = 0;
         currentArray
            .WriteUInt16LEToBytes( ref idx, (UInt16) headers.Machine )
            .WriteUInt16LEToBytes( ref idx, (UInt16) numSections )
            .WriteInt32LEToBytes( ref idx, Convert.ToInt32( DateTime.Now.Subtract( new DateTime( 1970, 1, 1, 0, 0, 0 ) ).TotalSeconds ) )
            .ZeroOut( ref idx, 8 )
            .WriteUInt16LEToBytes( ref idx, peOptionalHeaderSize )
            .WriteInt16LEToBytes( ref idx, (Int16) characteristics );
         sink.Write( currentArray, curArrayLen );

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
         var currentOffset = iatSize + snSize + snPadding + HeaderFieldOffsetsAndLengths.CLI_HEADER_SIZE;
         UserStringHeapWriter usersStrings;
         WriteMethodDefsIL( md, sink, codeSectionVirtualOffset, isPE64, byteArrayHelper, eArgs.MethodRVAs, ref currentOffset, out usersStrings );

         // Write manifest resources & field RVAs here
         UInt32 mResRVA, mResSize;
         WriteDataBeforeMD( md, sink, codeSectionVirtualOffset, isPE64, byteArrayHelper, eArgs, out mResRVA, out mResSize, ref currentOffset );

         // Write metadata streams (tables & heaps)
         var mdRVA = codeSectionVirtualOffset + currentOffset;
         var mdSize = WriteMetaData( md, sink, headers, eArgs, usersStrings, byteArrayHelper, thisAssemblyPublicKey );
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
            curArrayLen = MetaDataConstants.DEBUG_DD_SIZE + dbgData.Length;
            byteArrayHelper.EnsureSize( curArrayLen );
            currentArray = byteArrayHelper.Array;
            idx = 0;
            currentArray
               .WriteInt32LEToBytes( ref idx, dbgInfo.Characteristics )
               .WriteInt32LEToBytes( ref idx, dbgInfo.Timestamp )
               .WriteInt16LEToBytes( ref idx, dbgInfo.VersionMajor )
               .WriteInt16LEToBytes( ref idx, dbgInfo.VersionMinor )
               .WriteInt32LEToBytes( ref idx, dbgInfo.DebugType )
               .WriteInt32LEToBytes( ref idx, dbgData.Length )
               .WriteUInt32LEToBytes( ref idx, dbgRVA + MetaDataConstants.DEBUG_DD_SIZE )
               .WriteUInt32LEToBytes( ref idx, fAlign + currentOffset + (UInt32) idx + 4 ) // Pointer to data, end Debug Data Directory
               .BlockCopyFrom( ref idx, dbgData );
            sink.Write( currentArray, curArrayLen );
            currentOffset += (UInt32) curArrayLen;
            sink.SkipToNextAlignment( ref currentOffset, 0x4 );
         }


         var entryPointCodeRVA = 0u;
         var importDirectoryRVA = 0u;
         var importDirectorySize = 0u;
         var hnRVA = 0u;
         if ( hasRelocations )
         {
            // TODO write all of these in a single array
            // Import Directory
            // First, the table
            importDirectoryRVA = codeSectionVirtualOffset + currentOffset;
            importDirectorySize = HeaderFieldOffsetsAndLengths.IMPORT_DIRECTORY_SIZE;
            curArrayLen = HeaderFieldOffsetsAndLengths.IMPORT_DIRECTORY_SIZE;
            byteArrayHelper.EnsureSize( curArrayLen );
            currentArray = byteArrayHelper.Array;
            idx = 0;
            currentArray
               .WriteUInt32LEToBytes( ref idx, codeSectionVirtualOffset + currentOffset + (UInt32) curArrayLen ) // RVA of the ILT
               .WriteInt32LEToBytes( ref idx, 0 ) // DateTimeStamp
               .WriteInt32LEToBytes( ref idx, 0 ) // ForwarderChain
               .WriteUInt32LEToBytes( ref idx, codeSectionVirtualOffset + currentOffset + (UInt32) curArrayLen + HeaderFieldOffsetsAndLengths.ILT_SIZE + HeaderFieldOffsetsAndLengths.HINT_NAME_MIN_SIZE + (UInt32) importHintName.Length + 1 ) // RVA of Import Directory name (mscoree.dll)  
               .WriteUInt32LEToBytes( ref idx, codeSectionVirtualOffset ) // RVA of Import Address Table
               .ZeroOut( ref idx, curArrayLen - idx ); // The rest are zeroes

            sink.Write( currentArray, curArrayLen );
            currentOffset += (UInt32) curArrayLen;

            // ILT
            curArrayLen = HeaderFieldOffsetsAndLengths.ILT_SIZE;
            byteArrayHelper.EnsureSize( curArrayLen );
            currentArray = byteArrayHelper.Array;
            idx = 0;
            currentArray
               .WriteUInt32LEToBytes( ref idx, codeSectionVirtualOffset + currentOffset + (UInt32) curArrayLen ) // RVA of the hint/name table
               .ZeroOut( ref idx, curArrayLen - idx ); // The rest are zeroes
            sink.Write( currentArray, curArrayLen );
            currentOffset += (UInt32) curArrayLen;

            // Hint/Name table
            curArrayLen = HeaderFieldOffsetsAndLengths.HINT_NAME_MIN_SIZE + importHintName.Length + 1;
            byteArrayHelper.EnsureSize( curArrayLen );
            currentArray = byteArrayHelper.Array;
            hnRVA = currentOffset + codeSectionVirtualOffset;
            // Skip first two bytes
            idx = 0;
            currentArray.ZeroOut( ref idx, HeaderFieldOffsetsAndLengths.HINT_NAME_MIN_SIZE );
            currentArray.WriteASCIIString( ref idx, importHintName, true );
            sink.Write( currentArray, curArrayLen );
            currentOffset += (UInt32) curArrayLen;

            // Import DirectoryName
            foreach ( var chr in headers.ImportDirectoryName )
            {
               sink.WriteByte( (Byte) chr ); // TODO properly ASCII-encoded string
               ++currentOffset;
            }
            sink.WriteByte( 0 ); // String-terminating null
            ++currentOffset;

            // Then, a zero int
            // TODO investigate if this is really needed...
            sink.SeekFromCurrent( sizeof( Int32 ) );
            currentOffset += sizeof( Int32 );

            // Then, a PE entrypoint
            entryPointCodeRVA = currentOffset + codeSectionVirtualOffset;
            curArrayLen = sizeof( Int16 ) + sizeof( Int32 );
            byteArrayHelper.EnsureSize( curArrayLen );
            currentArray = byteArrayHelper.Array;
            idx = 0;
            currentArray
               .WriteInt16LEToBytes( ref idx, headers.EntryPointInstruction )
               .WriteUInt32LEToBytes( ref idx, (UInt32) imageBase + codeSectionVirtualOffset );
            sink.Write( currentArray, curArrayLen );
            currentOffset += (UInt32) curArrayLen;
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

            curArrayLen = HeaderFieldOffsetsAndLengths.RELOC_ARRAY_BASE_SIZE;
            byteArrayHelper.EnsureSize( curArrayLen );
            currentArray = byteArrayHelper.Array;
            idx = 0;
            currentArray
               .WriteUInt32LEToBytes( ref idx, pageRVA )
               .WriteUInt32LEToBytes( ref idx, HeaderFieldOffsetsAndLengths.RELOC_ARRAY_BASE_SIZE ) // Block size
               .WriteUInt32LEToBytes( ref idx, ( RELOCATION_FIXUP_TYPE << 12 ) + relocRVA - pageRVA ); // Type (high 4 bits) + Offset (lower 12 bits) + dummy entry (16 bits)
            sink.Write( currentArray, curArrayLen );
            currentOffset += (UInt32) curArrayLen;

            relocSectionInfo = new SectionInfo( sink, prevSectionInfo, currentOffset, sAlign, fAlign, true );
            prevSectionInfo = relocSectionInfo;
         }

         // Revisit PE optional header + section headers + padding + IAT + CLI header
         curArrayLen = revisitableArraySize;
         byteArrayHelper.EnsureSize( curArrayLen );
         currentArray = byteArrayHelper.Array;
         idx = 0;
         // PE optional header, ECMA-335 pp. 279-281
         // Standard fields
         currentArray
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
            currentArray.WriteUInt32LEToBytes( ref idx, hasResourceSection ? rsrcSectionInfo.virtualAddress : relocSectionInfo.virtualAddress ); // Base of data
         }
         // WinNT-specific fields
         var dllFlags = DLLFlags.TerminalServerAware | DLLFlags.NXCompatible | DLLFlags.NoSEH | DLLFlags.DynamicBase;
         if ( headers.HighEntropyVA )
         {
            dllFlags |= DLLFlags.HighEntropyVA;
         }
         ( isPE64 ? currentArray.WriteUInt64LEToBytes( ref idx, imageBase ) : currentArray.WriteUInt32LEToBytes( ref idx, (UInt32) imageBase ) )
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
            .WriteUInt16LEToBytes( ref idx, GetSubSystem( moduleKind ) ) // SubSystem
            .WriteUInt16LEToBytes( ref idx, (UInt16) dllFlags ); // DLL Characteristics
         if ( isPE64 )
         {
            currentArray
               .WriteUInt64LEToBytes( ref idx, headers.StackReserve ) // Stack Reserve Size
               .WriteUInt64LEToBytes( ref idx, headers.StackCommit ) // Stack Commit Size
               .WriteUInt64LEToBytes( ref idx, headers.HeapReserve ) // Heap Reserve Size
               .WriteUInt64LEToBytes( ref idx, headers.HeapCommit ); // Heap Commit Size
         }
         else
         {
            currentArray
               .WriteUInt32LEToBytes( ref idx, (UInt32) headers.StackReserve ) // Stack Reserve Size
               .WriteUInt32LEToBytes( ref idx, (UInt32) headers.StackCommit ) // Stack Commit Size
               .WriteUInt32LEToBytes( ref idx, (UInt32) headers.HeapReserve ) // Heap Reserve Size
               .WriteUInt32LEToBytes( ref idx, (UInt32) headers.HeapCommit ); // Heap Commit Size
         }
         currentArray
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
         currentArray.ZeroOut( ref idx, (Int32) ( fAlign - (UInt32) revisitableOffset - idx ) );

         // Write IAT if needed
         if ( hasRelocations )
         {
            currentArray
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
         currentArray
            .WriteUInt32LEToBytes( ref idx, HeaderFieldOffsetsAndLengths.CLI_HEADER_SIZE ) // Cb
            .WriteUInt16LEToBytes( ref idx, headers.CLIMajor ) // MajorRuntimeVersion
            .WriteUInt16LEToBytes( ref idx, headers.CLIMinor ) // MinorRuntimeVersion
            .WriteDataDirectory( ref idx, mdRVA, mdSize ) // MetaData
            .WriteInt32LEToBytes( ref idx, (Int32) moduleFlags ) // Flags
            .WriteInt32LEToBytes( ref idx, clrEntryPointToken ) // EntryPointToken
            .WriteDataDirectory( ref idx, mResRVA, mResSize ); // Resources
         var snDataDirOffset = revisitableOffset + idx;
         currentArray
            .WriteDataDirectory( ref idx, snRVA, snSize ) // StrongNameSignature
            .WriteZeroDataDirectory( ref idx ) // CodeManagerTable
            .WriteZeroDataDirectory( ref idx ) // VTableFixups
            .WriteZeroDataDirectory( ref idx ) // ExportAddressTableJumps
            .WriteZeroDataDirectory( ref idx ); // ManagedNativeHeader
#if DEBUG
         if ( idx != curArrayLen )
         {
            throw new Exception( "Something went wrong when emitting file headers. Emitted " + idx + " bytes, but was supposed to emit " + curArrayLen + " bytes." );
         }
#endif
         sink.Seek( revisitableOffset, SeekOrigin.Begin );
         sink.Write( currentArray, curArrayLen );

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

            curArrayLen = 8;
            byteArrayHelper.EnsureSize( curArrayLen );
            currentArray = byteArrayHelper.Array;
            idx = 0;
            currentArray.WriteDataDirectory( ref idx, snRVA, snSize );
            sink.Seek( snDataDirOffset, SeekOrigin.Begin );
            sink.Write( currentArray, curArrayLen );
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
         //if ( eArgs.FileAlignment < MIN_FILE_ALIGNMENT )
         //{
         //   eArgs.FileAlignment = MIN_FILE_ALIGNMENT;
         //}
         //else
         //{
         //   eArgs.FileAlignment = BinaryUtils.FLP2( eArgs.FileAlignment );
         //}

         //if ( eArgs.ImageBase < IMAGE_BASE_MULTIPLE )
         //{
         //   eArgs.ImageBase = IMAGE_BASE_MULTIPLE;
         //}
         //else
         //{

         //   eArgs.ImageBase -= eArgs.ImageBase % IMAGE_BASE_MULTIPLE;
         //}
         //if ( eArgs.SectionAlignment <= eArgs.FileAlignment )
         //{
         //   throw new ArgumentException( "Section alignment " + eArgs.SectionAlignment + " must be greater than file alignment " + eArgs.FileAlignment + "." );
         //}

         var machineEnum = eArgs.Machine;
         isPE64 = machineEnum.RequiresPE64();
         hasRelocations = machineEnum.RequiresRelocations();
         numSections = isPE64 ? 1u : 2u; // TODO win32-resource-section
         peOptionalHeaderSize = isPE64 ? HeaderFieldOffsetsAndLengths.PE_OPTIONAL_HEADER_SIZE_64 : HeaderFieldOffsetsAndLengths.PE_OPTIONAL_HEADER_SIZE_32;
         iatSize = hasRelocations ? HeaderFieldOffsetsAndLengths.IAT_SIZE : 0u; // No Import tables if no relocations
         //if ( !isPE64 )
         //{
         //   eArgs.ImageBase = CheckValueFor32PE( eArgs.ImageBase );
         //   eArgs.StackReserve = CheckValueFor32PE( eArgs.StackReserve );
         //   eArgs.StackCommit = CheckValueFor32PE( eArgs.StackCommit );
         //   eArgs.HeapReserve = CheckValueFor32PE( eArgs.HeapReserve );
         //   eArgs.HeapCommit = CheckValueFor32PE( eArgs.HeapCommit );
         //}
         //machine = (UInt16) machineEnum;
      }

      private static UInt64 CheckValueFor32PE( UInt64 value )
      {
         return Math.Min( UInt32.MaxValue, value );
      }

      private static UInt16 GetSubSystem( ModuleKind kind, Boolean isCEApp = false )
      {
         return ModuleKind.Windows == kind ? ( isCEApp ? HeaderFieldPossibleValues.IMAGE_SUBSYSTEM_WINDOWS_CE_GUI : HeaderFieldPossibleValues.IMAGE_SUBSYSTEM_WINDOWS_GUI ) : HeaderFieldPossibleValues.IMAGE_SUBSYSTEM_WINDOWS_CUI;
      }



      private static void WriteMethodDefsIL(
         CILMetaData md,
         Stream sink,
         UInt32 codeSectionVirtualOffset,
         Boolean isPE64,
         ByteArrayHelper byteArrayHelper,
         List<Int32> methodRVAs,
         ref UInt32 currentOffset,
         out UserStringHeapWriter usersStrings
         )
      {
         // Create users string heap
         usersStrings = new UserStringHeapWriter();
         var mDefs = md.MethodDefinitions.TableContents;
         methodRVAs.Clear();
         methodRVAs.Capacity = mDefs.Count;

         for ( var i = 0; i < mDefs.Count; ++i )
         {
            var method = mDefs[i];
            UInt32 thisMethodRVA;
            var il = method.IL;
            if ( il != null )
            {
               Boolean isTinyHeader;
               var methodILByteCount = EmitMethodIL( md, i, usersStrings, byteArrayHelper, out isTinyHeader );
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
         Stream sink,
         UInt32 codeSectionVirtualOffset,
         Boolean isPE64,
         ByteArrayHelper byteArrayHelper,
         EmittingArguments eArgs,
         out UInt32 resourcesRVA,
         out UInt32 resourcesSize,
         ref UInt32 currentOffset
         )
      {
         // Write manifest resources here
         var mResInfos = md.ManifestResources.TableContents;
         var mResInfo = eArgs.EmbeddedManifestResourceOffsets;
         mResInfo.Clear();
         mResInfo.Capacity = mResInfos.Count;

         resourcesSize = 0u;
         var arrayLen = sizeof( Int32 );
         byteArrayHelper.EnsureSize( arrayLen );
         var array = byteArrayHelper.Array;
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
               array.WriteInt32LEToBytesNoRef( 0, data.Length );
               sink.Write( array, arrayLen );
               sink.Write( data );
               resourcesSize += 4 + (UInt32) data.Length;
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

         // Write field RVAs here
         var mdFRVAs = md.FieldRVAs.TableContents;
         var fieldRVAs = eArgs.FieldRVAs;
         fieldRVAs.Clear();
         fieldRVAs.Capacity = mdFRVAs.Count;
         foreach ( var fRVAInfo in mdFRVAs )
         {
            fieldRVAs.Add( (Int32) ( codeSectionVirtualOffset + currentOffset ) );
            var data = fRVAInfo.Data;
            sink.Write( data );
            currentOffset += (UInt32) data.Length;
         }

         sink.SkipToNextAlignment( ref currentOffset, 0x04u );
      }

      private const Int32 METHOD_DATA_SECTION_HEADER_SIZE = 4;
      private const Int32 SMALL_EXC_BLOCK_SIZE = 12;
      private const Int32 LARGE_EXC_BLOCK_SIZE = 24;
      private const Int32 MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION = ( Byte.MaxValue - METHOD_DATA_SECTION_HEADER_SIZE ) / SMALL_EXC_BLOCK_SIZE; // 20
      private const Int32 MAX_LARGE_EXC_HANDLERS_IN_ONE_SECTION = ( 0x00FFFFFF - METHOD_DATA_SECTION_HEADER_SIZE ) / LARGE_EXC_BLOCK_SIZE; // 699050
      private const Int32 FAT_HEADER_SIZE = 12;

      internal static Int32 EmitMethodIL(
         CILMetaData md,
         Int32 methodIndex,
         UserStringHeapWriter usersStringHeap,
         ByteArrayHelper bytes,
         out Boolean isTinyHeader
         )
      {
         var il = md.MethodDefinitions.TableContents[methodIndex].IL;
         var locals = md.GetLocalsSignatureForMethodOrNull( methodIndex );
         Boolean exceptionSectionsAreLarge; Int32 wholeMethodByteCount;
         var ilCodeByteCount = CalculateByteSizeForMethod( il, locals, out isTinyHeader, out exceptionSectionsAreLarge, out wholeMethodByteCount );
         var exceptionBlocks = il.ExceptionBlocks;
         var hasAnyExceptions = exceptionBlocks.Count > 0;

         bytes.EnsureSize( wholeMethodByteCount );
         var array = bytes.Array;
         var idx = 0;

         // Header
         if ( isTinyHeader )
         {
            // Tiny header - one byte
            array.WriteByteToBytes( ref idx, (Byte) ( (Int32) MethodHeaderFlags.TinyFormat | ( ilCodeByteCount << 2 ) ) );
         }
         else
         {
            // Fat header - 12 bytes
            var flags = MethodHeaderFlags.FatFormat;
            if ( hasAnyExceptions )
            {
               flags |= MethodHeaderFlags.MoreSections;
            }
            if ( il.InitLocals )
            {
               flags |= MethodHeaderFlags.InitLocals;
            }

            array.WriteInt16LEToBytes( ref idx, (Int16) ( ( (Int32) flags ) | ( 3 << 12 ) ) )
               .WriteUInt16LEToBytes( ref idx, (UInt16) il.MaxStackSize )
               .WriteInt32LEToBytes( ref idx, ilCodeByteCount )
               .WriteInt32LEToBytes( ref idx, il.LocalsSignatureIndex.GetOneBasedToken() );
         }


         // Emit IL code
         foreach ( var info in il.OpCodes )
         {
            EmitOpCodeInfo( info, array, ref idx, usersStringHeap );
         }

         // Emit exception block infos
         if ( hasAnyExceptions )
         {
            var processedIndices = new HashSet<Int32>();
            array.ZeroOut( ref idx, BitUtils.MultipleOf4( idx ) - idx );
            var flags = MethodDataFlags.ExceptionHandling;
            if ( exceptionSectionsAreLarge )
            {
               flags |= MethodDataFlags.FatFormat;
            }
            var excCount = exceptionBlocks.Count;
            var maxExceptionHandlersInOneSections = exceptionSectionsAreLarge ? MAX_LARGE_EXC_HANDLERS_IN_ONE_SECTION : MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION;
            var excBlockSize = exceptionSectionsAreLarge ? LARGE_EXC_BLOCK_SIZE : SMALL_EXC_BLOCK_SIZE;
            var curExcIndex = 0;
            while ( excCount > 0 )
            {
               var amountToBeWritten = Math.Min( excCount, maxExceptionHandlersInOneSections );
               if ( amountToBeWritten < excCount )
               {
                  flags |= MethodDataFlags.MoreSections;
               }
               else
               {
                  flags = flags & ~( MethodDataFlags.MoreSections );
               }

               array.WriteByteToBytes( ref idx, (Byte) flags )
                  .WriteInt32LEToBytes( ref idx, amountToBeWritten * excBlockSize + METHOD_DATA_SECTION_HEADER_SIZE );
               --idx;

               // Subtract this here since amountToBeWritten will change
               excCount -= amountToBeWritten;

               if ( exceptionSectionsAreLarge )
               {
                  while ( amountToBeWritten > 0 )
                  {
                     // Write large exc
                     var block = exceptionBlocks[curExcIndex];
                     array.WriteInt32LEToBytes( ref idx, (Int32) block.BlockType )
                     .WriteInt32LEToBytes( ref idx, block.TryOffset )
                     .WriteInt32LEToBytes( ref idx, block.TryLength )
                     .WriteInt32LEToBytes( ref idx, block.HandlerOffset )
                     .WriteInt32LEToBytes( ref idx, block.HandlerLength )
                     .WriteInt32LEToBytes( ref idx, block.BlockType != ExceptionBlockType.Filter ? block.ExceptionType.GetOneBasedToken() : block.FilterOffset );
                     ++curExcIndex;
                     --amountToBeWritten;
                  }
               }
               else
               {
                  while ( amountToBeWritten > 0 )
                  {
                     var block = exceptionBlocks[curExcIndex];
                     // Write small exception
                     array.WriteInt16LEToBytes( ref idx, (Int16) block.BlockType )
                        .WriteUInt16LEToBytes( ref idx, (UInt16) block.TryOffset )
                        .WriteByteToBytes( ref idx, (Byte) block.TryLength )
                        .WriteUInt16LEToBytes( ref idx, (UInt16) block.HandlerOffset )
                        .WriteByteToBytes( ref idx, (Byte) block.HandlerLength )
                        .WriteInt32LEToBytes( ref idx, block.BlockType != ExceptionBlockType.Filter ? block.ExceptionType.GetOneBasedToken() : block.FilterOffset );
                     ++curExcIndex;
                     --amountToBeWritten;
                  }
               }

            }
         }

#if DEBUG
         if ( idx != wholeMethodByteCount )
         {
            throw new Exception( "Something went wrong when emitting method headers and body. Emitted " + idx + " bytes, but was supposed to emit " + wholeMethodByteCount + " bytes." );
         }
#endif

         return idx;
      }

      private static void EmitOpCodeInfo( OpCodeInfo codeInfo, Byte[] array, ref Int32 idx, UserStringHeapWriter usersStrings )
      {
         const UInt32 USER_STRING_MASK = 0x70 << 24;

         var code = codeInfo.OpCode;

         if ( code.Size > 1 )
         {
            array.WriteByteToBytes( ref idx, code.Byte1 );
         }
         array.WriteByteToBytes( ref idx, code.Byte2 );

         var operandType = code.OperandType;
         if ( operandType != OperandType.InlineNone )
         {
            Int32 i32;
            switch ( operandType )
            {
               case OperandType.ShortInlineI:
               case OperandType.ShortInlineVar:
                  array.WriteByteToBytes( ref idx, (Byte) ( (OpCodeInfoWithInt32) codeInfo ).Operand );
                  break;
               case OperandType.ShortInlineBrTarget:
                  i32 = ( (OpCodeInfoWithInt32) codeInfo ).Operand;
                  array.WriteByteToBytes( ref idx, (Byte) i32 );
                  break;
               case OperandType.ShortInlineR:
                  array.WriteSingleLEToBytes( ref idx, (Single) ( (OpCodeInfoWithSingle) codeInfo ).Operand );
                  break;
               case OperandType.InlineBrTarget:
                  i32 = ( (OpCodeInfoWithInt32) codeInfo ).Operand;
                  array.WriteInt32LEToBytes( ref idx, i32 );
                  break;
               case OperandType.InlineI:
                  array.WriteInt32LEToBytes( ref idx, ( (OpCodeInfoWithInt32) codeInfo ).Operand );
                  break;
               case OperandType.InlineVar:
                  array.WriteInt16LEToBytes( ref idx, (Int16) ( (OpCodeInfoWithInt32) codeInfo ).Operand );
                  break;
               case OperandType.InlineR:
                  array.WriteDoubleLEToBytes( ref idx, (Double) ( (OpCodeInfoWithDouble) codeInfo ).Operand );
                  break;
               case OperandType.InlineI8:
                  array.WriteInt64LEToBytes( ref idx, (Int64) ( (OpCodeInfoWithInt64) codeInfo ).Operand );
                  break;
               case OperandType.InlineString:
                  array.WriteInt32LEToBytes( ref idx, (Int32) ( usersStrings.GetOrAddString( ( (OpCodeInfoWithString) codeInfo ).Operand ) | USER_STRING_MASK ) );
                  break;
               case OperandType.InlineField:
               case OperandType.InlineMethod:
               case OperandType.InlineType:
               case OperandType.InlineTok:
               case OperandType.InlineSig:
                  var tIdx = ( (OpCodeInfoWithToken) codeInfo ).Operand;
                  array.WriteInt32LEToBytes( ref idx, tIdx.OneBasedToken );
                  break;
               case OperandType.InlineSwitch:
                  var offsets = ( (OpCodeInfoWithSwitch) codeInfo ).Offsets;
                  array.WriteInt32LEToBytes( ref idx, offsets.Count );
                  foreach ( var offset in offsets )
                  {
                     array.WriteInt32LEToBytes( ref idx, offset );
                  }
                  break;
               default:
                  throw new ArgumentException( "Unknown operand type: " + code.OperandType + " for " + code + "." );
            }
         }
      }

      private static Int32 CalculateByteSizeForMethod(
         MethodILDefinition methodIL,
         LocalVariablesSignature localSig,
         out Boolean isTinyHeader,
         out Boolean exceptionSectionsAreLarge,
         out Int32 wholeMethodByteCount
         )
      {
         // Start by calculating the size of just IL code
         var arraySize = methodIL.OpCodes.Sum( oci => oci.GetTotalByteCount() );
         var ilCodeByteCount = arraySize;

         // Then calculate the size of headers and other stuff
         var exceptionBlocks = methodIL.ExceptionBlocks;
         // PEVerify doesn't like mixed small and fat blocks at all (however, at least Cecil understands that kind of situation)
         // Apparently, PEVerify doesn't like multiple small blocks either (Cecil still loads code fine)
         // So to use small exception blocks at all, all the blocks must be small, and there must be a limited amount of them
         var allAreSmall = exceptionBlocks.Count <= MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION
            && exceptionBlocks.All( excBlock =>
            {
               return excBlock.TryLength <= Byte.MaxValue
                  && excBlock.HandlerLength <= Byte.MaxValue
                  && excBlock.TryOffset <= UInt16.MaxValue
                  && excBlock.HandlerOffset <= UInt16.MaxValue;
            } );

         var maxStack = methodIL.MaxStackSize;

         var excCount = exceptionBlocks.Count;
         var hasAnyExc = excCount > 0;
         isTinyHeader = arraySize < 64
            && !hasAnyExc
            && maxStack <= 8
            && ( localSig == null || localSig.Locals.Count == 0 );

         if ( isTinyHeader )
         {
            // Can use tiny header
            ++arraySize;
         }
         else
         {
            // Use fat header
            arraySize += FAT_HEADER_SIZE;
            if ( hasAnyExc )
            {
               // Skip to next boundary of 4
               arraySize = BitUtils.MultipleOf4( arraySize );
               var excBlockSize = allAreSmall ? SMALL_EXC_BLOCK_SIZE : LARGE_EXC_BLOCK_SIZE;
               var maxExcHandlersInOnSection = allAreSmall ? MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION : MAX_LARGE_EXC_HANDLERS_IN_ONE_SECTION;
               arraySize += BinaryUtils.AmountOfPagesTaken( excCount, maxExcHandlersInOnSection ) * METHOD_DATA_SECTION_HEADER_SIZE +
                  excCount * excBlockSize;
            }
         }

         exceptionSectionsAreLarge = hasAnyExc && !allAreSmall;

         wholeMethodByteCount = arraySize;

         return ilCodeByteCount;

         //bytes.EnsureSize( arraySize );
         //this._array = bytes.Array;
         //this._arrayIndex = 0;
      }

      //This assumes that sink offset is at multiple of 4.
      private static UInt32 WriteMetaData(
         CILMetaData md,
         Stream sink,
         HeadersData headers,
         EmittingArguments eArgs,
         UserStringHeapWriter userStrings,
         ByteArrayHelper byteArrayHelper,
         Byte[] thisAssemblyPublicKey
         )
      {
         // Actual meta-data
         var metaDataVersion = headers.MetaDataVersion;
         var versionStringSize = MetaDataStringEncoding.GetByteCount( metaDataVersion ) + 1;
         if ( versionStringSize > MD_MAX_VERSION_LENGTH )
         {
            throw new ArgumentException( "Metadata version must be at maximum " + MD_MAX_VERSION_LENGTH + " bytes long after encoding it using " + MetaDataStringEncoding + "." );
         }

         // Then write the data to the byte sink
         // ECMA-335, pp. 271-272
         var streamHeaders = new Int32[HEAP_COUNT];
         var streamSizes = new UInt32[HEAP_COUNT];

         BLOBHeapWriter blobs; SystemStringHeapWriter sysStrings; GUIDHeapWriter guids; Object[] heapInfos;
         CreateMDHeaps( md, thisAssemblyPublicKey, byteArrayHelper, out blobs, out sysStrings, out guids, out heapInfos );

         var hasSysStrings = sysStrings.Accessed;
         var hasUserStrings = userStrings.Accessed;
         var hasBlobs = blobs.Accessed;
         var hasGuids = guids.Accessed;

         if ( hasSysStrings )
         {
            // Store offset to array to streamHeaders
            // This offset, for each stream, tells where to write first field of stream header (offset from metadata root)
            streamHeaders[SYS_STRINGS_IDX] = 8 + BitUtils.MultipleOf4( MetaDataConstants.SYS_STRING_STREAM_NAME.Length + 1 );
            streamSizes[SYS_STRINGS_IDX] = sysStrings.Size;
         }
         if ( hasUserStrings )
         {
            streamHeaders[USER_STRINGS_IDX] = 8 + BitUtils.MultipleOf4( MetaDataConstants.USER_STRING_STREAM_NAME.Length + 1 );
            streamSizes[USER_STRINGS_IDX] = userStrings.Size;
         }
         if ( hasGuids )
         {
            streamHeaders[GUID_IDX] = 8 + BitUtils.MultipleOf4( MetaDataConstants.GUID_STREAM_NAME.Length + 1 );
            streamSizes[GUID_IDX] = guids.Size;
         }
         if ( hasBlobs )
         {
            streamHeaders[BLOB_IDX] = 8 + BitUtils.MultipleOf4( MetaDataConstants.BLOB_STREAM_NAME.Length + 1 );
            streamSizes[BLOB_IDX] = blobs.Size;
         }

         var tableSizes = new Int32[Consts.AMOUNT_OF_TABLES];
         for ( var i = 0; i < Consts.AMOUNT_OF_TABLES; ++i )
         {
            MetaDataTable tbl;
            if ( md.TryGetByTable( (Tables) i, out tbl ) )
            {
               tableSizes[i] = tbl.RowCount;
            }
         }

         var tableWidths = new Int32[tableSizes.Length];
         for ( var i = 0; i < tableWidths.Length; ++i )
         {
            if ( tableSizes[i] > 0 )
            {
               tableWidths[i] = MetaDataConstants.CalculateTableWidth(
                  (Tables) i,
                  tableSizes,
                  sysStrings.IsWide,
                  guids.IsWide,
                  blobs.IsWide
                  );
            }
         }

         var tRefWidths = MetaDataConstants.GetCodedTableIndexSizes( tableSizes );

         var versionStringSize4 = BitUtils.MultipleOf4( versionStringSize );
         var tableStreamHeaderSize = 24 + 4 * tableSizes.Count( size => size > 0 );
         streamHeaders[MD_IDX] = 8 + BitUtils.MultipleOf4( MetaDataConstants.TABLE_STREAM_NAME.Length );
         var tableStreamSize = (UInt32) tableStreamHeaderSize + tableSizes.Select( ( size, idx ) => (UInt32) size * (UInt32) tableWidths[idx] ).Sum();
         var tableStreamSize4 = BitUtils.MultipleOf4( tableStreamSize );
         streamSizes[MD_IDX] = tableStreamSize4;

         var arrayLen = 16 // Header start
            + versionStringSize4 // Version string
            + 4 // Header end
            + streamHeaders.Sum() // Stream headers
            + tableStreamHeaderSize; // Table stream header
         byteArrayHelper.EnsureSize( arrayLen );
         var anArray = byteArrayHelper.Array;

         // Metadata root
         var offset = 0;
         anArray.WriteUInt32LEToBytes( ref offset, MD_SIGNATURE )
            .WriteUInt16LEToBytes( ref offset, MD_MAJOR )
            .WriteUInt16LEToBytes( ref offset, MD_MINOR )
            .WriteUInt32LEToBytes( ref offset, MD_RESERVED )
            .WriteInt32LEToBytes( ref offset, versionStringSize4 )
            .WriteStringToBytes( ref offset, MetaDataStringEncoding, metaDataVersion )
            .ZeroOut( ref offset, versionStringSize4 - versionStringSize + 1 )
            .WriteUInt16LEToBytes( ref offset, MD_FLAGS )
            .WriteUInt16LEToBytes( ref offset, (UInt16) streamHeaders.Count( stream => stream > 0 ) );
         var curStreamOffset = (UInt32) ( arrayLen - tableStreamHeaderSize ); // Table stream starts immediately after MD root

         // #~ header
         anArray.WriteUInt32LEToBytes( ref offset, curStreamOffset )
            .WriteUInt32LEToBytes( ref offset, tableStreamSize4 )
            .WriteStringToBytes( ref offset, MetaDataStringEncoding, MetaDataConstants.TABLE_STREAM_NAME )
            .ZeroOut( ref offset, 4 - ( offset % 4 ) );
         curStreamOffset += tableStreamSize4;

         if ( hasSysStrings )
         {
            // #String header
            var size = streamSizes[SYS_STRINGS_IDX];
            anArray.WriteUInt32LEToBytes( ref offset, curStreamOffset )
               .WriteUInt32LEToBytes( ref offset, size )
               .WriteStringToBytes( ref offset, MetaDataStringEncoding, MetaDataConstants.SYS_STRING_STREAM_NAME )
               .ZeroOut( ref offset, 4 - ( offset % 4 ) );
            curStreamOffset += size;
         }

         if ( hasUserStrings )
         {
            // #US header
            var size = streamSizes[USER_STRINGS_IDX];
            anArray.WriteUInt32LEToBytes( ref offset, curStreamOffset )
               .WriteUInt32LEToBytes( ref offset, size )
               .WriteStringToBytes( ref offset, MetaDataStringEncoding, MetaDataConstants.USER_STRING_STREAM_NAME )
               .ZeroOut( ref offset, 4 - ( offset % 4 ) );
            curStreamOffset += size;
         }

         if ( hasGuids )
         {
            // #Guid header
            var size = streamSizes[GUID_IDX];
            anArray.WriteUInt32LEToBytes( ref offset, curStreamOffset )
               .WriteUInt32LEToBytes( ref offset, size )
               .WriteStringToBytes( ref offset, MetaDataStringEncoding, MetaDataConstants.GUID_STREAM_NAME )
               .ZeroOut( ref offset, 4 - ( offset % 4 ) );
            curStreamOffset += size;
         }

         if ( hasBlobs )
         {
            // #Blob header
            var size = streamSizes[BLOB_IDX];
            anArray.WriteUInt32LEToBytes( ref offset, curStreamOffset )
               .WriteUInt32LEToBytes( ref offset, size )
               .WriteStringToBytes( ref offset, MetaDataStringEncoding, MetaDataConstants.BLOB_STREAM_NAME )
               .ZeroOut( ref offset, 4 - ( offset % 4 ) );
            curStreamOffset += size;
         }

         // Write the end of the header
         // Header (ECMA-335, p. 273)
         var validBitvector = 0L;
         for ( var i = tableSizes.Length - 1; i >= 0; --i )
         {
            validBitvector = validBitvector << 1;
            if ( tableSizes[i] > 0 )
            {
               validBitvector |= 1;
            }
         }
         anArray.WriteInt32LEToBytes( ref offset, TABLE_STREAM_RESERVED )
            .WriteByteToBytes( ref offset, headers.TableHeapMajor )
            .WriteByteToBytes( ref offset, headers.TableHeapMinor )
            .WriteByteToBytes( ref offset, (Byte) ( Convert.ToInt32( sysStrings.IsWide ) | ( Convert.ToInt32( guids.IsWide ) << 1 ) | ( Convert.ToInt32( blobs.IsWide ) << 2 ) ) )
            .WriteByteToBytes( ref offset, TABLE_STREAM_RESERVED_2 )
            .WriteInt64LEToBytes( ref offset, validBitvector )
            .WriteInt64LEToBytes( ref offset, SORTED_TABLES );
         for ( var i = 0; i < tableSizes.Length; ++i )
         {
            if ( tableSizes[i] > 0 )
            {
               anArray.WriteInt32LEToBytes( ref offset, tableSizes[i] );
            }
         }

#if DEBUG
         if ( offset != arrayLen )
         {
            throw new Exception( "Debyyg" );
         }
#endif

         // Write the MD header + table stream header
         sink.Write( anArray, arrayLen );

         // Table stream tables start right here
         // ECMA-335, p. 239
         ForEachElement<ModuleDefinition, HeapInfo4>( md.ModuleDefinitions, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, module, heapInfo ) => array
            .WriteInt16LEToBytes( ref idx, module.Generation ) // Generation
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            .WriteHeapIndex( ref idx, guids, heapInfo.Heap2 ) // MvId
            .WriteHeapIndex( ref idx, guids, heapInfo.Heap3 ) // EncId
            .WriteHeapIndex( ref idx, guids, heapInfo.Heap4 ) // EncBaseId
            );
         // ECMA-335, p. 247
         // TypeRef may contain types which result in duplicate rows - avoid that
         ForEachElement<TypeReference, HeapInfo2>( md.TypeReferences, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, typeRef, heapInfo ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.ResolutionScope, typeRef.ResolutionScope, tRefWidths ) // ResolutionScope
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // TypeName
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap2 ) // TypeNamespace
            );
         // ECMA-335, p. 243
         ForEachElement<TypeDefinition, HeapInfo2>( md.TypeDefinitions, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, typeDef, heapInfo ) => array
            .WriteInt32LEToBytes( ref idx, (Int32) typeDef.Attributes ) // Flags
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // TypeName
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap2 ) // TypeNamespace
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeDefOrRef, typeDef.BaseType, tRefWidths ) // Extends
            .WriteSimpleTableIndex( ref idx, typeDef.FieldList, tableSizes ) // FieldList
            .WriteSimpleTableIndex( ref idx, typeDef.MethodList, tableSizes ) // MethodList
            );
         // ECMA-335, p. 223
         ForEachElement<FieldDefinition, HeapInfo2>( md.FieldDefinitions, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, fDef, heapInfo ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) fDef.Attributes ) // FieldAttributes
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap2 ) // Signature
            );
         // ECMA-335, p. 233
         ForEachElement<MethodDefinition, HeapInfo2>( md.MethodDefinitions, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, mDef, heapInfo ) => array
            .WriteUInt32LEToBytes( ref idx, (UInt32) eArgs.MethodRVAs[listIdx] ) // RVA
            .WriteInt16LEToBytes( ref idx, (Int16) mDef.ImplementationAttributes ) // ImplFlags
            .WriteInt16LEToBytes( ref idx, (Int16) mDef.Attributes ) // Flags
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap2 ) // Signature
            .WriteSimpleTableIndex( ref idx, mDef.ParameterList, tableSizes ) // ParamList
            );
         // ECMA-335, p. 240
         ForEachElement<ParameterDefinition, HeapInfo1>( md.ParameterDefinitions, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, pDef, heapInfo ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) pDef.Attributes ) // Flags
            .WriteUInt16LEToBytes( ref idx, (UInt16) pDef.Sequence ) // Sequence
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            );
         // ECMA-335, p. 231
         ForEachElement( md.InterfaceImplementations, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, item ) => array
            .WriteSimpleTableIndex( ref idx, item.Class, tableSizes ) // Class
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeDefOrRef, item.Interface, tRefWidths ) // Interface
            );
         // ECMA-335, p. 232
         ForEachElement<MemberReference, HeapInfo2>( md.MemberReferences, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, mRef, heapInfo ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MemberRefParent, mRef.DeclaringType, tRefWidths ) // Class
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap2 ) // Signature
            );
         // ECMA-335, p. 216
         ForEachElement<ConstantDefinition, HeapInfo1>( md.ConstantDefinitions, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, constant, heapInfo ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) constant.Type ) // Type
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasConstant, constant.Parent, tRefWidths ) // Parent
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap1 ) // Value
            );
         // ECMA-335, p. 216
         ForEachElement<CustomAttributeDefinition, HeapInfo1>( md.CustomAttributeDefinitions, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, ca, heapInfo ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasCustomAttribute, ca.Parent, tRefWidths ) // Parent
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.CustomAttributeType, ca.Type, tRefWidths ) // Type
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap1 )
            );
         // ECMA-335, p.226
         ForEachElement<FieldMarshal, HeapInfo1>( md.FieldMarshals, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, fm, heapInfo ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasFieldMarshal, fm.Parent, tRefWidths ) // Parent
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap1 ) // NativeType
            );
         // ECMA-335, p. 218
         ForEachElement<SecurityDefinition, HeapInfo1>( md.SecurityDefinitions, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, sec, heapInfo ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) sec.Action ) // Action
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasDeclSecurity, sec.Parent, tRefWidths ) // Parent
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap1 ) // PermissionSet
            );
         // ECMA-335 p. 215
         ForEachElement( md.ClassLayouts, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, cl ) => array
            .WriteUInt16LEToBytes( ref idx, (UInt16) cl.PackingSize ) // PackingSize
            .WriteInt32LEToBytes( ref idx, cl.ClassSize ) // ClassSize
            .WriteSimpleTableIndex( ref idx, cl.Parent, tableSizes ) // Parent
            );
         // ECMA-335 p. 225
         ForEachElement( md.FieldLayouts, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, fl ) => array
            .WriteInt32LEToBytes( ref idx, fl.Offset ) // Offset
            .WriteSimpleTableIndex( ref idx, fl.Field, tableSizes ) // Field
            );
         // ECMA-335 p. 243
         ForEachElement<StandaloneSignature, HeapInfo1>( md.StandaloneSignatures, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, sig, heapInfo ) => array
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap1 ) // Signature
            );
         // ECMA-335 p. 220
         ForEachElement( md.EventMaps, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, em ) => array
            .WriteSimpleTableIndex( ref idx, em.Parent, tableSizes ) // Parent
            .WriteSimpleTableIndex( ref idx, em.EventList, tableSizes ) // EventList
            );
         // ECMA-335 p. 221
         ForEachElement<EventDefinition, HeapInfo1>( md.EventDefinitions, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, evt, heapInfo ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) evt.Attributes ) // EventFlags
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeDefOrRef, evt.EventType, tRefWidths ) // EventType
            );
         // ECMA-335 p. 242
         ForEachElement( md.PropertyMaps, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, pm ) => array
            .WriteSimpleTableIndex( ref idx, pm.Parent, tableSizes ) // Parent
            .WriteSimpleTableIndex( ref idx, pm.PropertyList, tableSizes ) // PropertyList
            );
         // ECMA-335 p. 242
         ForEachElement<PropertyDefinition, HeapInfo2>( md.PropertyDefinitions, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, prop, heapInfo ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) prop.Attributes ) // Flags
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap2 ) // Type
            );
         // ECMA-335 p. 237
         ForEachElement( md.MethodSemantics, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, ms ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) ms.Attributes ) // Semantics
            .WriteSimpleTableIndex( ref idx, ms.Method, tableSizes ) // Method
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasSemantics, ms.Associaton, tRefWidths ) // Association
            );
         // ECMA-335 p. 237
         ForEachElement( md.MethodImplementations, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, mi ) => array
            .WriteSimpleTableIndex( ref idx, mi.Class, tableSizes ) // Class
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MethodDefOrRef, mi.MethodBody, tRefWidths ) // MethodBody
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MethodDefOrRef, mi.MethodDeclaration, tRefWidths ) // MethodDeclaration
            );
         // ECMA-335, p. 239
         ForEachElement<ModuleReference, HeapInfo1>( md.ModuleReferences, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, modRef, heapInfo ) => array
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            );
         // ECMA-335, p. 248
         ForEachElement<TypeSpecification, HeapInfo1>( md.TypeSpecifications, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, tSpec, heapInfo ) => array
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap1 ) // Signature
            );
         // ECMA-335, p. 230
         ForEachElement<MethodImplementationMap, HeapInfo1>( md.MethodImplementationMaps, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, mim, heapInfo ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) mim.Attributes ) // PInvokeAttributes
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MemberForwarded, mim.MemberForwarded, tRefWidths ) // MemberForwarded
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Import name
            .WriteSimpleTableIndex( ref idx, mim.ImportScope, tableSizes ) // Import scope
            );
         // ECMA-335, p. 227
         ForEachElement( md.FieldRVAs, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, fRVA ) => array
            .WriteUInt32LEToBytes( ref idx, (UInt32) eArgs.FieldRVAs[listIdx] )
            .WriteSimpleTableIndex( ref idx, fRVA.Field, tableSizes )
            );
         // ECMA-335, p. 211
         ForEachElement<AssemblyDefinition, HeapInfo3>( md.AssemblyDefinitions, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, ass, heapInfo ) => array
            .WriteInt32LEToBytes( ref idx, (Int32) ass.HashAlgorithm ) // HashAlgId
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.AssemblyInformation.VersionMajor ) // MajorVersion
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.AssemblyInformation.VersionMinor ) // MinorVersion
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.AssemblyInformation.VersionBuild ) // BuildNumber
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.AssemblyInformation.VersionRevision ) // RevisionNumber
            .WriteInt32LEToBytes( ref idx, (Int32) ass.Attributes ) // Flags
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap1 ) // PublicKey
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap2 ) // Name
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap3 ) // Culture
            );
         // ECMA-335, p. 212
         ForEachElement<AssemblyReference, HeapInfo4>( md.AssemblyReferences, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, assRef, heapInfo ) => array
            .WriteUInt16LEToBytes( ref idx, (UInt16) assRef.AssemblyInformation.VersionMajor ) // MajorVersion
            .WriteUInt16LEToBytes( ref idx, (UInt16) assRef.AssemblyInformation.VersionMinor ) // MinorVersion
            .WriteUInt16LEToBytes( ref idx, (UInt16) assRef.AssemblyInformation.VersionBuild ) // BuildNumber
            .WriteUInt16LEToBytes( ref idx, (UInt16) assRef.AssemblyInformation.VersionRevision ) // RevisionNumber
            .WriteInt32LEToBytes( ref idx, (Int32) assRef.Attributes ) // Flags
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap1 ) // PublicKey
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap2 ) // Name
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap3 ) // Culture
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap4 ) // HashValue
            );
         ForEachElement<FileReference, HeapInfo2>( md.FileReferences, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, file, heapInfo ) => array
            .WriteInt32LEToBytes( ref idx, (Int32) file.Attributes ) // Flags
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap2 ) // HashValue
            );
         ForEachElement<ExportedType, HeapInfo2>( md.ExportedTypes, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, eType, heapInfo ) => array
            .WriteInt32LEToBytes( ref idx, (Int32) eType.Attributes ) // TypeAttributes
            .WriteInt32LEToBytes( ref idx, eType.TypeDefinitionIndex ) // TypeDef index in other (!) assembly
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // TypeName
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap2 ) // TypeNamespace
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.Implementation, eType.Implementation, tRefWidths ) // Implementation
            );
         ForEachElement<ManifestResource, HeapInfo1>( md.ManifestResources, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, mRes, heapInfo ) => array
            .WriteUInt32LEToBytes( ref idx, (UInt32) ( eArgs.EmbeddedManifestResourceOffsets[listIdx] ?? mRes.Offset ) ) // Offset
            .WriteInt32LEToBytes( ref idx, (Int32) mRes.Attributes ) // Flags
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.Implementation, mRes.Implementation, tRefWidths ) // Implementation
            );
         // ECMA-335, p. 240
         ForEachElement( md.NestedClassDefinitions, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, nc ) => array
            .WriteSimpleTableIndex( ref idx, nc.NestedClass, tableSizes ) // NestedClass
            .WriteSimpleTableIndex( ref idx, nc.EnclosingClass, tableSizes ) // EnclosingClass
            );
         // ECMA-335, p. 228
         ForEachElement<GenericParameterDefinition, HeapInfo1>( md.GenericParameterDefinitions, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, gParam, heapInfo ) => array
            .WriteUInt16LEToBytes( ref idx, (UInt16) gParam.GenericParameterIndex ) // Number
            .WriteInt16LEToBytes( ref idx, (Int16) gParam.Attributes ) // Flags
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeOrMethodDef, gParam.Owner, tRefWidths ) // Owner
            .WriteHeapIndex( ref idx, sysStrings, heapInfo.Heap1 ) // Name
            );
         // ECMA-335, p. 238
         ForEachElement<MethodSpecification, HeapInfo1>( md.MethodSpecifications, tableWidths, sink, heapInfos, byteArrayHelper, ( array, idx, listIdx, mSpec, heapInfo ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MethodDefOrRef, mSpec.Method, tRefWidths ) // Method
            .WriteHeapIndex( ref idx, blobs, heapInfo.Heap1 ) // Instantiation
            );
         // ECMA-335, p. 229
         ForEachElement( md.GenericParameterConstraintDefinitions, tableWidths, sink, byteArrayHelper, ( array, idx, listIdx, gConstraint ) => array
            .WriteSimpleTableIndex( ref idx, gConstraint.Owner, tableSizes ) // Owner
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeDefOrRef, gConstraint.Constraint, tRefWidths ) // Constraint
            );
         // Padding
         for ( var i = tableStreamSize; i < tableStreamSize4; i++ )
         {
            sink.WriteByte( 0 );
         }

         // Done with tables, write other heaps
         sysStrings.WriteHeap( sink, byteArrayHelper );
         userStrings.WriteHeap( sink, byteArrayHelper );
         guids.WriteHeap( sink, byteArrayHelper );
         blobs.WriteHeap( sink, byteArrayHelper );

         return curStreamOffset;
      }

      private static void ForEachElement<T, U>(
         MetaDataTable<T> mdTable,
         Int32[] tableWidths,
         Stream sink,
         Object[] heapInfos,
         ByteArrayHelper byteArrayHelper,
         Action<Byte[], Int32, Int32, T, U> writeAction
         )
         where T : class
      {
         var list = mdTable.TableContents;
         var count = list.Count;
         if ( count > 0 )
         {
            var tableEnum = mdTable.TableKind;
            Int32 width;
            var arrayLen = CheckArrayForTableEmitting( tableEnum, count, tableWidths, byteArrayHelper, out width );
            var idx = 0;
            var heapInfoList = (List<U>) heapInfos[(Int32) tableEnum];
            var array = byteArrayHelper.Array;
            for ( var i = 0; i < count; ++i )
            {
               writeAction( array, idx, i, list[i], heapInfoList[i] );
               idx += width;
            }
            sink.Write( array, arrayLen );
#if DEBUG
            if ( idx != arrayLen )
            {
               throw new Exception( "Something went wrong when emitting metadata array: emitted " + idx + " instead of expected " + arrayLen + " bytes." );
            }
#endif
         }
      }

      private static void ForEachElement<T>(
         MetaDataTable<T> mdTable,
         Int32[] tableWidths,
         Stream sink,
         ByteArrayHelper byteArrayHelper,
         Action<Byte[], Int32, Int32, T> writeAction
         )
         where T : class
      {
         var list = mdTable.TableContents;
         var count = list.Count;
         if ( count > 0 )
         {
            var tableEnum = mdTable.TableKind;
            Int32 width;
            var arrayLen = CheckArrayForTableEmitting( tableEnum, count, tableWidths, byteArrayHelper, out width );
            var idx = 0;
            var array = byteArrayHelper.Array;
            for ( var i = 0; i < count; ++i )
            {
               writeAction( array, idx, i, list[i] );
               idx += width;
            }
            sink.Write( array, arrayLen );
#if DEBUG
            if ( idx != arrayLen )
            {
               throw new Exception( "Something went wrong when emitting metadata array: emitted " + idx + " instead of expected " + arrayLen + " bytes." );
            }
#endif
         }
      }

      private static Int32 CheckArrayForTableEmitting( Tables tableEnum, Int32 rowCount, Int32[] tableWidths, ByteArrayHelper byteArrayHelper, out Int32 width )
      {
         width = tableWidths[(Int32) tableEnum];
         var arrayLen = width * rowCount;
         byteArrayHelper.EnsureSize( arrayLen );
         return arrayLen;
      }

      private static void CreateMDHeaps(
         CILMetaData md,
         Byte[] thisAssemblyPublicKey,
         ByteArrayHelper byteArrayHelper,
         out BLOBHeapWriter blobsParam,
         out SystemStringHeapWriter sysStringsParam,
         out GUIDHeapWriter guidsParam,
         out Object[] heapInfos
         )
      {
         var blobs = new BLOBHeapWriter();
         var sysStrings = new SystemStringHeapWriter();
         var guids = new GUIDHeapWriter();
         heapInfos = new Object[Consts.AMOUNT_OF_TABLES];

         var auxHelper = new ByteArrayHelper(); // For writing security BLOBs
         // 0x00 Module
         ProcessTableForHeaps4( md.ModuleDefinitions, heapInfos, mod => new HeapInfo4( sysStrings.GetOrAddString( mod.Name ), guids.GetOrAddGUID( mod.ModuleGUID ), guids.GetOrAddGUID( mod.EditAndContinueGUID ), guids.GetOrAddGUID( mod.EditAndContinueBaseGUID ) ) );
         // 0x01 TypeRef
         ProcessTableForHeaps2( md.TypeReferences, heapInfos, tr => new HeapInfo2( sysStrings.GetOrAddString( tr.Name ), sysStrings.GetOrAddString( tr.Namespace ) ) );
         // 0x02 TypeDef
         ProcessTableForHeaps2( md.TypeDefinitions, heapInfos, td => new HeapInfo2( sysStrings.GetOrAddString( td.Name ), sysStrings.GetOrAddString( td.Namespace ) ) );
         // 0x04 FieldDef
         ProcessTableForHeaps2( md.FieldDefinitions, heapInfos, f => new HeapInfo2( sysStrings.GetOrAddString( f.Name ), blobs.GetOrAddBLOB( byteArrayHelper.CreateFieldSignature( f.Signature ) ) ) );
         // 0x06 MethodDef
         ProcessTableForHeaps2( md.MethodDefinitions, heapInfos, m => new HeapInfo2( sysStrings.GetOrAddString( m.Name ), blobs.GetOrAddBLOB( byteArrayHelper.CreateMethodSignature( m.Signature ) ) ) );
         // 0x08 Parameter
         ProcessTableForHeaps1( md.ParameterDefinitions, heapInfos, ( p, idx ) => new HeapInfo1( sysStrings.GetOrAddString( p.Name ) ) );
         // 0x0A MemberRef
         ProcessTableForHeaps2( md.MemberReferences, heapInfos, m => new HeapInfo2( sysStrings.GetOrAddString( m.Name ), blobs.GetOrAddBLOB( byteArrayHelper.CreateMemberRefSignature( m.Signature ) ) ) );
         // 0x0B Constant
         ProcessTableForHeaps1( md.ConstantDefinitions, heapInfos, ( c, idx ) => new HeapInfo1( blobs.GetOrAddBLOB( byteArrayHelper.CreateConstantBytes( c.Value ) ) ) );
         // 0x0C CustomAttribute
         ProcessTableForHeaps1( md.CustomAttributeDefinitions, heapInfos, ( ca, idx ) => new HeapInfo1( blobs.GetOrAddBLOB( byteArrayHelper.CreateCustomAttributeSignature( md, idx ) ) ) );
         // 0x0D FieldMarshal
         ProcessTableForHeaps1( md.FieldMarshals, heapInfos, ( fm, idx ) => new HeapInfo1( blobs.GetOrAddBLOB( byteArrayHelper.CreateMarshalSpec( fm.NativeType ) ) ) );
         // 0x0E Security definitions
         ProcessTableForHeaps1( md.SecurityDefinitions, heapInfos, ( sd, idx ) => new HeapInfo1( blobs.GetOrAddBLOB( byteArrayHelper.CreateSecuritySignature( sd, auxHelper ) ) ) );
         // 0x11 Standalone sig
         ProcessTableForHeaps1( md.StandaloneSignatures, heapInfos, ( s, idx ) => new HeapInfo1( blobs.GetOrAddBLOB( byteArrayHelper.CreateStandaloneSignature( s ) ) ) );
         // 0x14 Event
         ProcessTableForHeaps1( md.EventDefinitions, heapInfos, ( e, idx ) => new HeapInfo1( sysStrings.GetOrAddString( e.Name ) ) );
         // 0x17 Property
         ProcessTableForHeaps2( md.PropertyDefinitions, heapInfos, p => new HeapInfo2( sysStrings.GetOrAddString( p.Name ), blobs.GetOrAddBLOB( byteArrayHelper.CreatePropertySignature( p.Signature ) ) ) );
         // 0x1A ModuleRef
         ProcessTableForHeaps1( md.ModuleReferences, heapInfos, ( mr, idx ) => new HeapInfo1( sysStrings.GetOrAddString( mr.ModuleName ) ) );
         // 0x1B TypeSpec
         ProcessTableForHeaps1( md.TypeSpecifications, heapInfos, ( t, idx ) => new HeapInfo1( blobs.GetOrAddBLOB( byteArrayHelper.CreateTypeSignature( t.Signature ) ) ) );
         // 0x1C ImplMap
         ProcessTableForHeaps1( md.MethodImplementationMaps, heapInfos, ( mim, idx ) => new HeapInfo1( sysStrings.GetOrAddString( mim.ImportName ) ) );
         // 0x20 Assembly
         ProcessTableForHeaps3( md.AssemblyDefinitions, heapInfos, ad =>
         {
            var pk = ad.AssemblyInformation.PublicKeyOrToken;
            return new HeapInfo3( blobs.GetOrAddBLOB( pk.IsNullOrEmpty() ? thisAssemblyPublicKey : pk ), sysStrings.GetOrAddString( ad.AssemblyInformation.Name ), sysStrings.GetOrAddString( ad.AssemblyInformation.Culture ) );
         } );
         // 0x21 AssemblyRef
         ProcessTableForHeaps4( md.AssemblyReferences, heapInfos, ar => new HeapInfo4( blobs.GetOrAddBLOB( ar.AssemblyInformation.PublicKeyOrToken ), sysStrings.GetOrAddString( ar.AssemblyInformation.Name ), sysStrings.GetOrAddString( ar.AssemblyInformation.Culture ), blobs.GetOrAddBLOB( ar.HashValue ) ) );
         // 0x26 File
         ProcessTableForHeaps2( md.FileReferences, heapInfos, f => new HeapInfo2( sysStrings.GetOrAddString( f.Name ), blobs.GetOrAddBLOB( f.HashValue ) ) );
         // 0x27 ExportedType
         ProcessTableForHeaps2( md.ExportedTypes, heapInfos, e => new HeapInfo2( sysStrings.GetOrAddString( e.Name ), sysStrings.GetOrAddString( e.Namespace ) ) );
         // 0x28 ManifestResource
         ProcessTableForHeaps1( md.ManifestResources, heapInfos, ( m, idx ) => new HeapInfo1( sysStrings.GetOrAddString( m.Name ) ) );
         // 0x2A GenericParameter
         ProcessTableForHeaps1( md.GenericParameterDefinitions, heapInfos, ( g, idx ) => new HeapInfo1( sysStrings.GetOrAddString( g.Name ) ) );
         // 0x2B MethosSpec
         ProcessTableForHeaps1( md.MethodSpecifications, heapInfos, ( m, idx ) => new HeapInfo1( blobs.GetOrAddBLOB( byteArrayHelper.CreateMethodSpecSignature( m.Signature ) ) ) );

         // We're done
         blobs.SetIsWideIndex();
         sysStrings.SetIsWideIndex();
         guids.SetIsWideIndex();

         blobsParam = blobs;
         sysStringsParam = sysStrings;
         guidsParam = guids;
      }

      private static void ProcessTableForHeaps1<T>( MetaDataTable<T> table, Object[] heapInfos, Func<T, Int32, HeapInfo1> heapInfoExtractor )
         where T : class
      {
         var list = table.TableContents;
         var heapInfoList = new List<HeapInfo1>( list.Count );
         for ( var i = 0; i < list.Count; ++i )
         {
            heapInfoList.Add( heapInfoExtractor( list[i], i ) );
         }
         heapInfos[(Int32) table.TableKind] = heapInfoList;
      }

      private static void ProcessTableForHeaps2<T>( MetaDataTable<T> table, Object[] heapInfos, Func<T, HeapInfo2> heapInfoExtractor )
         where T : class
      {
         var list = table.TableContents;
         var heapInfoList = new List<HeapInfo2>( list.Count );
         foreach ( var row in list )
         {
            heapInfoList.Add( heapInfoExtractor( row ) );
         }
         heapInfos[(Int32) table.TableKind] = heapInfoList;
      }

      private static void ProcessTableForHeaps3<T>( MetaDataTable<T> table, Object[] heapInfos, Func<T, HeapInfo3> heapInfoExtractor )
         where T : class
      {
         var list = table.TableContents;
         var heapInfoList = new List<HeapInfo3>( list.Count );
         foreach ( var row in list )
         {
            heapInfoList.Add( heapInfoExtractor( row ) );
         }
         heapInfos[(Int32) table.TableKind] = heapInfoList;
      }

      private static void ProcessTableForHeaps4<T>( MetaDataTable<T> table, Object[] heapInfos, Func<T, HeapInfo4> heapInfoExtractor )
         where T : class
      {
         var list = table.TableContents;
         var heapInfoList = new List<HeapInfo4>( list.Count );
         foreach ( var row in list )
         {
            heapInfoList.Add( heapInfoExtractor( row ) );
         }
         heapInfos[(Int32) table.TableKind] = heapInfoList;
      }


      private struct HeapInfo1
      {
         internal readonly UInt32 Heap1;

         internal HeapInfo1( UInt32 heap1 )
         {
            this.Heap1 = heap1;
         }
      }

      private struct HeapInfo2
      {
         internal readonly UInt32 Heap1;
         internal readonly UInt32 Heap2;

         internal HeapInfo2( UInt32 heap1, UInt32 heap2 )
         {
            this.Heap1 = heap1;
            this.Heap2 = heap2;
         }
      }

      private struct HeapInfo3
      {
         internal readonly UInt32 Heap1;
         internal readonly UInt32 Heap2;
         internal readonly UInt32 Heap3;

         internal HeapInfo3( UInt32 heap1, UInt32 heap2, UInt32 heap3 )
         {
            this.Heap1 = heap1;
            this.Heap2 = heap2;
            this.Heap3 = heap3;
         }
      }

      private struct HeapInfo4
      {
         internal readonly UInt32 Heap1;
         internal readonly UInt32 Heap2;
         internal readonly UInt32 Heap3;
         internal readonly UInt32 Heap4;

         internal HeapInfo4( UInt32 heap1, UInt32 heap2, UInt32 heap3, UInt32 heap4 )
         {
            this.Heap1 = heap1;
            this.Heap2 = heap2;
            this.Heap3 = heap3;
            this.Heap4 = heap4;
         }
      }

      internal static Byte[] WriteCodedTableIndex( this Byte[] array, ref Int32 idx, CodedTableIndexKind codedKind, TableIndex? tIdx, IDictionary<CodedTableIndexKind, Boolean> wideIndices )
      {
         return wideIndices[codedKind] ? array.WriteInt32LEToBytes( ref idx, MetaDataConstants.GetCodedTableIndex( codedKind, tIdx ) ) : array.WriteUInt16LEToBytes( ref idx, (UInt16) MetaDataConstants.GetCodedTableIndex( codedKind, tIdx ) );
      }

      internal static Byte[] WriteSimpleTableIndex( this Byte[] array, ref Int32 idx, TableIndex tIdx, Int32[] tableSizes )
      {
         return tableSizes[(Int32) tIdx.Table] > UInt16.MaxValue ? array.WriteInt32LEToBytes( ref idx, ( tIdx.Index + 1 ) ) : array.WriteUInt16LEToBytes( ref idx, (UInt16) ( tIdx.Index + 1 ) );
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

      #region PE Optional Header, Sub system
      public const UInt16 IMAGE_SUBSYSTEM_WINDOWS_CUI = 0x03;
      public const UInt16 IMAGE_SUBSYSTEM_WINDOWS_GUI = 0x02;
      public const UInt16 IMAGE_SUBSYSTEM_WINDOWS_CE_GUI = 0x09;
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

      #region Base Relocation Table Entries
      public const Byte IMAGE_REL_BASED_HIGHLOW = 0x3;
      #endregion

      #region CIL Header
      public const Int32 COMIMAGE_FLAGS_ILONLY = 0x00000001;
      public const Int32 COMIMAGE_FLAGS_32BITREQUIRED = 0x00000002;
      public const Int32 COMIMAGE_FLAGS_STRONGNAMESIGNED = 0x00000008;
      public const Int32 COMIMAGE_FLAGS_NATIVE_ENTRYPOINT = 0x00000010;
      public const Int32 COMIMAGE_FLAGS_TRACKDEBUGDATA = 0x00010000;
      public const Int32 COMIMAGE_FLAGS_32BITPREFERRED = 0x00020000;
      #endregion

      #region VTable fixup
      public const Int16 COR_VTABLE_32BIT = 0x0001;
      public const Int16 COR_VTABLE_64BIT = 0x0002;
      public const Int16 COR_VTABLE_FROM_UNMANAGED = 0x0004;
      public const Int16 COR_VTABLE_CALL_MOST_DERIVED = 0x0010;
      #endregion

      #region Metadata Table Stream Header
      public const Byte STRING_STREAM_SIZE_OVER_2BYTES = 0x01;
      public const Byte GUID_STREAM_SIZE_OVER_2BYTES = 0x02;
      public const Byte BLOB_STREAM_SIZE_OVER_2BYTES = 0x04;
      #endregion
   }

   internal static class HeaderFieldOffsetsAndLengths
   {
      #region MS-DOS header

      internal static readonly Byte[] DOS_HEADER_AND_PE_SIG = new Byte[] {
         0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00,
         0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 ,/* lfanew begin */ 0x80, 0x00, 0x00, 0x00, /* lfanew end */
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

   internal abstract class AbstractHeapWriter
   {
      protected UInt32 _curIndex;
      protected Boolean _isWide;
      private Boolean _accessed;

      internal AbstractHeapWriter( UInt32 startingIndex = 1 )
      {
         this._curIndex = startingIndex;
      }

      public void SetIsWideIndex()
      {
         if ( this.Size > UInt16.MaxValue )
         {
            this._isWide = true;
         }
      }

      public Boolean IsWide
      {
         get
         {
            return this._isWide;
         }
      }

      public UInt32 Size
      {
         get
         {
            return BitUtils.MultipleOf4( this._curIndex );
         }
      }

      public Boolean Accessed
      {
         get
         {
            return this._accessed;
         }
      }

      public abstract void WriteHeap( Stream stream, ByteArrayHelper byteArrayHelper );

      protected void CheckAccessed()
      {
         if ( !this._accessed )
         {
            this._accessed = true;
         }
      }


   }

   internal abstract class AbstractStringHeapWriter : AbstractHeapWriter
   {
      private readonly IDictionary<String, KeyValuePair<UInt32, Int32>> _strings;
      private readonly Encoding _encoding;

      internal AbstractStringHeapWriter( Encoding encoding )
      {
         this._encoding = encoding;
         this._strings = new Dictionary<String, KeyValuePair<UInt32, Int32>>();
      }

      internal UInt32 GetOrAddString( String str )
      {
         UInt32 result;
         if ( str == null )
         {
            result = 0;
         }
         else
         {
            this.CheckAccessed();

            KeyValuePair<UInt32, Int32> strInfo;
            if ( this._strings.TryGetValue( str, out strInfo ) )
            {
               result = strInfo.Key;
            }
            else
            {
               result = this._curIndex;
               this.AddString( str );
            }
         }
         return result;
      }

      internal Int32 StringCount
      {
         get
         {
            return this._strings.Count;
         }
      }

      public override void WriteHeap( Stream stream, ByteArrayHelper byteArrayHelper )
      {
         if ( this.Accessed )
         {
            stream.WriteByte( 0 );
            if ( this._strings.Count > 0 )
            {
               foreach ( var kvp in this._strings )
               {
                  var arrayLen = kvp.Value.Value;
                  byteArrayHelper.EnsureSize( arrayLen );
                  this.Serialize( kvp.Key, byteArrayHelper );
                  stream.Write( byteArrayHelper.Array, arrayLen );
               }
            }
            var tmp = this._curIndex;
            stream.SkipToNextAlignment( ref this._curIndex, 4 );
         }
      }

      private void AddString( String str )
      {
         var byteCount = this.GetByteCountForString( str );
         this._strings.Add( str, new KeyValuePair<UInt32, Int32>( this._curIndex, byteCount ) );
         this._curIndex += (UInt32) byteCount;
      }

      protected Encoding Encoding
      {
         get
         {
            return this._encoding;
         }
      }

      protected abstract Int32 GetByteCountForString( String str );

      protected abstract void Serialize( String str, ByteArrayHelper byteArrayHelper );
   }

   internal sealed class UserStringHeapWriter : AbstractStringHeapWriter
   {
      internal UserStringHeapWriter()
         : base( MetaDataConstants.USER_STRING_ENCODING )
      {

      }


      protected override Int32 GetByteCountForString( String str )
      {
         var retVal = str.Length * 2 // Each character is 2 bytes
            + 1; // Trailing byte (zero or 1)
         retVal += BitUtils.GetEncodedUIntSize( retVal ); // How many bytes it will take to compress the byte count
         return retVal;
      }

      protected override void Serialize( String str, ByteArrayHelper byteArrayHelper )
      {
         // Byte array helper has already been set up to hold array size
         var array = byteArrayHelper.Array;
         // Byte count
         var arrayIndex = 0;
         array.CompressUInt32( ref arrayIndex, str.Length * 2 + 1 );

         // Actual string
         Byte lastByte = 0;
         for ( var i = 0; i < str.Length; ++i )
         {
            var chr = str[i];
            array.WriteUInt16LEToBytes( ref arrayIndex, chr );
            // ECMA-335, p. 272
            if ( lastByte == 0 &&
             ( chr > 0x7E
                  || ( chr <= 0x2D
                     && ( ( chr >= 0x01 && chr <= 0x08 )
                        || ( chr >= 0x0E && chr <= 0x1F )
                        || chr == 0x27 || chr == 0x2D ) )
                  ) )
            {
               lastByte = 1;
            }
         }
         // Trailing byte (zero or 1)
         array[arrayIndex++] = lastByte;
      }
   }

   internal sealed class SystemStringHeapWriter : AbstractStringHeapWriter
   {
      internal SystemStringHeapWriter()
         : base( MetaDataConstants.SYS_STRING_ENCODING )
      {

      }

      protected override Int32 GetByteCountForString( String str )
      {
         return this.Encoding.GetByteCount( str ) // Byte count for string
            + 1; // Trailing zero
      }

      protected override void Serialize( String str, ByteArrayHelper byteArrayHelper )
      {
         // Byte array helper has already been set up to hold array size
         var array = byteArrayHelper.Array;
         var byteCount = this.Encoding.GetBytes( str, 0, str.Length, array, 0 );
         // Remember trailing zero
         array[byteCount] = 0;
      }
   }

   internal class BLOBHeapWriter : AbstractHeapWriter
   {
      private readonly IDictionary<Byte[], UInt32> _blobIndices;
      private readonly IList<Byte[]> _blobs;

      internal BLOBHeapWriter()
      {
         this._blobIndices = new Dictionary<Byte[], UInt32>( ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer );
         this._blobs = new List<Byte[]>();
      }

      internal UInt32 GetOrAddBLOB( Byte[] blob )
      {
         UInt32 result;
         if ( blob == null )
         {
            result = 0;
         }
         else
         {
            this.CheckAccessed();
            if ( !this._blobIndices.TryGetValue( blob, out result ) )
            {
               result = this._curIndex;
               this._blobIndices.Add( blob, result );
               this._blobs.Add( blob );
               this._curIndex += (UInt32) blob.Length + (UInt32) BitUtils.GetEncodedUIntSize( blob.Length );
            }
         }

         return result;
      }

      public override void WriteHeap( Stream stream, ByteArrayHelper byteArrayHelper )
      {
         if ( this.Accessed )
         {
            stream.WriteByte( 0 );
            if ( this._blobs.Count > 0 )
            {
               byteArrayHelper.EnsureSize( 4 );
               var array = byteArrayHelper.Array;
               foreach ( var blob in this._blobs )
               {
                  var i = 0;
                  array.CompressUInt32( ref i, blob.Length );
                  stream.Write( array, i );
                  stream.Write( blob );
               }
            }
            var tmp = this._curIndex;
            stream.SkipToNextAlignment( ref tmp, 4 );
         }
      }
   }

   internal sealed class GUIDHeapWriter : AbstractHeapWriter
   {
      private readonly IDictionary<Guid, UInt32> _guids;

      internal GUIDHeapWriter()
         : base( 0 )
      {
         this._guids = new Dictionary<Guid, UInt32>();
      }

      internal UInt32 GetOrAddGUID( Guid? guid )
      {
         UInt32 result;
         if ( guid.HasValue )
         {
            this.CheckAccessed();

            result = this._guids.GetOrAdd_NotThreadSafe( guid.Value, g =>
            {
               var retVal = (UInt32) this._guids.Count + 1;
               this._curIndex += MetaDataConstants.GUID_SIZE;
               return retVal;
            } );
         }
         else
         {
            result = 0;
         }

         return result;
      }

      public override void WriteHeap( Stream stream, ByteArrayHelper byteArrayHelper )
      {
         if ( this.Accessed )
         {
            byteArrayHelper.EnsureSize( MetaDataConstants.GUID_SIZE );

            foreach ( var kvp in this._guids )
            {
               stream.Write( kvp.Key.ToByteArray() );
            }
         }
      }
   }

   internal sealed class ByteArrayHelper
   {

      private const Int32 DEFAULT_INITIAL_SIZE = 512;

      //private readonly Int32 _blockSize;
      //private readonly IList<Byte[]> _prevBlocks;
      //private readonly IList<Int32> _prevBlockSizes;

      private Byte[] _bytes;
      private readonly Encoding _stringEncoding;
      //private Byte[] _curBlock;
      private Int32 _curCount;
      //private Int32 _prevBlockIndex;

      internal ByteArrayHelper( Int32 initialSize = DEFAULT_INITIAL_SIZE )
      {
         this._bytes = new Byte[initialSize];
         this._curCount = 0;
         this._stringEncoding = new UTF8Encoding( false, true );
      }

      internal void AddByte( Byte aByte )
      {
         this.EnsureSize( 1 );
         this._bytes[this._curCount++] = aByte;
      }

      internal void AddSByte( SByte sByte )
      {
         this.EnsureSize( 1 );
         this._bytes[this._curCount++] = (Byte) sByte;
      }

      internal void AddUncompressedInt16( Int16 val )
      {
         this.EnsureSize( 2 );
         this._bytes.WriteInt16LEToBytes( ref this._curCount, val );
      }

      internal void AddUncompressedUInt16( UInt16 val )
      {
         this.EnsureSize( 2 );
         this._bytes.WriteUInt16LEToBytes( ref this._curCount, val );
      }

      internal void AddUncompressedInt32( Int32 value )
      {
         this.EnsureSize( 4 );
         this._bytes.WriteInt32LEToBytes( ref this._curCount, value );
      }

      internal void AddUncompressedUInt32( UInt32 value )
      {
         this.EnsureSize( 4 );
         this._bytes.WriteUInt32LEToBytes( ref this._curCount, value );
      }

      internal void AddUncompressedInt64( Int64 value )
      {
         this.EnsureSize( 8 );
         this._bytes.WriteInt64LEToBytes( ref this._curCount, value );
      }

      internal void AddUncompressedUInt64( UInt64 value )
      {
         this.EnsureSize( 8 );
         this._bytes.WriteUInt64LEToBytes( ref this._curCount, value );
      }

      internal void AddUncompressedSingle( Single value )
      {
         this.EnsureSize( 4 );
         this._bytes.WriteSingleLEToBytes( ref this._curCount, value );
      }

      internal void AddUncompressedDouble( Double value )
      {
         this.EnsureSize( 8 );
         this._bytes.WriteDoubleLEToBytes( ref this._curCount, value );
      }

      internal void AddCAString( String str )
      {
         if ( str == null )
         {
            this.AddByte( 0xFF );
         }
         else
         {
            var size = this._stringEncoding.GetByteCount( str );
            this.AddCompressedUInt32( size );
            this.EnsureSize( size );
            this._curCount += this._stringEncoding.GetBytes( str, 0, str.Length, this._bytes, this._curCount );
         }
      }

      //internal void AddTypeString( String typeString )
      //{
      //   this.AddCAString( typeString ); // Utils.CreateTypeString( type, moduleBeingEmitted, true ) );
      //}

      internal void AddNormalString( String str )
      {
         // This is used only when storing string in constants table. The string must be stored using user string encoding (UTF16).
         if ( str == null )
         {
            this.AddByte( 0x00 );
         }
         else
         {
            var size = MetaDataConstants.USER_STRING_ENCODING.GetByteCount( str );
            this.EnsureSize( size );
            this._curCount += MetaDataConstants.USER_STRING_ENCODING.GetBytes( str, 0, str.Length, this._bytes, this._curCount );
         }
      }

      internal void AddBytes( Byte[] bytes )
      {
         if ( !bytes.IsNullOrEmpty() )
         {
            this.EnsureSize( bytes.Length );
            System.Array.Copy( bytes, 0, this._bytes, this._curCount, bytes.Length );
            this._curCount += bytes.Length;
         }
      }

      internal void AddTDRSToken( TableIndex token )
      {
         this.AddCompressedUInt32( TableIndex.EncodeTypeDefOrRefOrSpec( token.OneBasedToken ) );
      }

      internal void AddCompressedUInt32( Int32 value )
      {
         this.EnsureSize( (Int32) BitUtils.GetEncodedUIntSize( value ) );
         this._bytes.CompressUInt32( ref this._curCount, value );
      }

      internal void AddCompressedInt32( Int32 value )
      {
         this.EnsureSize( BitUtils.GetEncodedIntSize( value ) );
         this._bytes.CompressInt32( ref this._curCount, value );
      }

      internal void AddSigByte( SignatureElementTypes sigType )
      {
         this.AddByte( (Byte) sigType );
      }

      internal void AddSigStarterByte( SignatureStarters sigStarter )
      {
         this.AddByte( (Byte) sigStarter );
      }

      internal Byte[] CreateByteArray()
      {
         Byte[] result;
         if ( this._curCount == 0 )
         {
            result = Empty<Byte>.Array;
         }
         else
         {
            result = new Byte[this._curCount];
            System.Array.Copy( this._bytes, 0, result, 0, result.Length );
            this._curCount = 0;
         }
         return result;
      }

      internal void EnsureSize( Int32 size ) // , SizeKind = (Absolute | Incremental [default] ) 
      {
         if ( this._bytes.Length < this._curCount + size )
         {
            var oldArray = this._bytes;
            this._bytes = new Byte[BinaryUtils.CLP2( (UInt32) this._curCount + (UInt32) size )];
            if ( this._curCount > 0 )
            {
               System.Array.Copy( oldArray, 0, this._bytes, 0, oldArray.Length );
            }
         }
      }

      internal Int32 CurCount
      {
         get
         {
            return this._curCount;
         }
      }

      internal Byte[] Array
      {
         get
         {
            return this._bytes;
         }
      }
      //private void Reset()
      //{
      //   if ( this._prevBlockIndex > 0 )
      //   {
      //      this._curBlock = this._prevBlocks[0];
      //   }
      //   this._prevBlockIndex = 0;
      //   this._curCount = 0;
      //}

   }

   //// Maybe move to UtilPack ?
   //internal static class Testing
   //{
   //   public static Int32 MostEfficientCount<T, U>( this T enumerable )
   //      where T : IEnumerable<U>
   //   {
   //      var array = enumerable as Array;
   //      if ( array != null )
   //      {
   //         return array.Length;
   //      }
   //      else
   //      {
   //          var collection = enumerable as ICollection<U>;
   //          return collection != null ?
   //             collection.Count :
   //             enumerable.Count();
   //      }
   //   }
   //}


}

public static partial class E_CILPhysical
{
   private static readonly ClassOrValueTypeSignature DummyClassOrValueTypeSignature = new ClassOrValueTypeSignature();

   internal static Byte[] WriteHeapIndex( this Byte[] array, ref Int32 idx, AbstractHeapWriter writer, UInt32 heapIndex )
   {
      return writer.IsWide ? array.WriteUInt32LEToBytes( ref idx, heapIndex ) : array.WriteUInt16LEToBytes( ref idx, (UInt16) heapIndex );
   }

   internal static Byte[] CreateFieldSignature( this ByteArrayHelper info, FieldSignature sig )
   {
      info.WriteFieldSignature( sig );
      return info.CreateByteArray();
   }

   internal static Byte[] CreateMethodSignature( this ByteArrayHelper info, AbstractMethodSignature sig )
   {
      info.WriteMethodSignature( sig );
      return info.CreateByteArray();
   }

   internal static Byte[] CreateMemberRefSignature( this ByteArrayHelper info, AbstractSignature sig )
   {
      return sig.SignatureKind == SignatureKind.Field ?
         info.CreateFieldSignature( (FieldSignature) sig ) :
         info.CreateMethodSignature( (MethodReferenceSignature) sig );
   }

   internal static Byte[] CreateConstantBytes( this ByteArrayHelper info, Object constant )
   {
      info.WriteConstantValue( constant );
      return info.CreateByteArray();
   }

   internal static Byte[] CreateCustomAttributeSignature( this ByteArrayHelper info, CILMetaData md, Int32 caIdx )
   {
      var sig = md.CustomAttributeDefinitions.TableContents[caIdx].Signature;
      Byte[] retVal;
      if ( sig != null ) // sig.TypedArguments.Count > 0 || sig.NamedArguments.Count > 0 )
      {
         var sigg = sig as CustomAttributeSignature;
         if ( sigg != null )
         {
            info.WriteCustomAttributeSignature( md, caIdx );
            retVal = info.CreateByteArray();
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

   internal static Byte[] CreateMarshalSpec( this ByteArrayHelper info, MarshalingInfo marshal )
   {
      info.WriteMarshalInfo( marshal );
      return info.CreateByteArray();
   }

   internal static Byte[] CreateSecuritySignature( this ByteArrayHelper info, SecurityDefinition security, ByteArrayHelper aux )
   {
      info.WriteSecuritySignature( security, aux );
      return info.CreateByteArray();
   }

   internal static Byte[] CreateStandaloneSignature( this ByteArrayHelper info, StandaloneSignature standaloneSig )
   {
      var sig = standaloneSig.Signature;
      var locals = sig as LocalVariablesSignature;
      if ( locals != null )
      {
         if ( standaloneSig.StoreSignatureAsFieldSignature && locals.Locals.Count > 0 )
         {
            info.AddSigStarterByte( SignatureStarters.Field );
            info.WriteLocalSignature( locals.Locals[0] );
         }
         else
         {
            info.WriteLocalsSignature( locals );
         }
      }
      else
      {
         var raw = sig as RawSignature;
         if ( raw != null )
         {
            info.AddBytes( raw.Bytes );
         }
         else
         {
            info.WriteMethodSignature( sig as AbstractMethodSignature );
         }
      }

      return info.CurCount == 0 ? null : info.CreateByteArray();
   }

   internal static Byte[] CreatePropertySignature( this ByteArrayHelper info, PropertySignature sig )
   {
      info.WritePropertySignature( sig );
      return info.CreateByteArray();
   }

   internal static Byte[] CreateTypeSignature( this ByteArrayHelper info, TypeSignature sig )
   {
      info.WriteTypeSignature( sig );
      return info.CreateByteArray();
   }

   internal static Byte[] CreateMethodSpecSignature( this ByteArrayHelper info, GenericMethodSignature sig )
   {
      info.WriteMethodSpecSignature( sig );
      return info.CreateByteArray();
   }

   internal static void WriteFieldSignature( this ByteArrayHelper info, FieldSignature sig )
   {
      if ( sig != null )
      {
         info.AddSigStarterByte( SignatureStarters.Field );
         info.WriteCustomModifiers( sig.CustomModifiers );
         info.WriteTypeSignature( sig.Type );
      }
   }

   private static void WriteCustomModifiers( this ByteArrayHelper info, IList<CustomModifierSignature> mods )
   {
      if ( mods.Count > 0 )
      {
         foreach ( var mod in mods )
         {
            info.AddSigByte( mod.IsOptional ? SignatureElementTypes.CModOpt : SignatureElementTypes.CModReqd );
            info.AddTDRSToken( mod.CustomModifierType );
         }
      }
   }

   private static void WriteTypeSignature( this ByteArrayHelper info, TypeSignature type )
   {
      switch ( type.TypeSignatureKind )
      {
         case TypeSignatureKind.Simple:
            info.AddSigByte( ( (SimpleTypeSignature) type ).SimpleType );
            break;
         case TypeSignatureKind.SimpleArray:
            info.AddSigByte( SignatureElementTypes.SzArray );
            var szArray = (SimpleArrayTypeSignature) type;
            info.WriteCustomModifiers( szArray.CustomModifiers );
            info.WriteTypeSignature( szArray.ArrayType );
            break;
         case TypeSignatureKind.ComplexArray:
            info.AddSigByte( SignatureElementTypes.Array );
            var array = (ComplexArrayTypeSignature) type;
            info.WriteTypeSignature( array.ArrayType );
            info.AddCompressedUInt32( array.Rank );
            info.AddCompressedUInt32( array.Sizes.Count );
            foreach ( var size in array.Sizes )
            {
               info.AddCompressedUInt32( size );
            }
            info.AddCompressedUInt32( array.LowerBounds.Count );
            foreach ( var lobo in array.LowerBounds )
            {
               info.AddCompressedInt32( lobo );
            }
            break;
         case TypeSignatureKind.ClassOrValue:
            var clazz = (ClassOrValueTypeSignature) type;
            var gArgs = clazz.GenericArguments;
            var isGenericType = gArgs.Count > 0;
            if ( isGenericType )
            {
               info.AddSigByte( SignatureElementTypes.GenericInst );
            }
            info.AddSigByte( clazz.IsClass ? SignatureElementTypes.Class : SignatureElementTypes.ValueType );
            info.AddTDRSToken( clazz.Type );
            if ( isGenericType )
            {
               info.AddCompressedUInt32( gArgs.Count );
               foreach ( var gArg in gArgs )
               {
                  info.WriteTypeSignature( gArg );
               }
            }
            break;
         case TypeSignatureKind.GenericParameter:
            var gParam = (GenericParameterTypeSignature) type;
            info.AddSigByte( gParam.IsTypeParameter ? SignatureElementTypes.Var : SignatureElementTypes.MVar );
            info.AddCompressedUInt32( gParam.GenericParameterIndex );
            break;
         case TypeSignatureKind.FunctionPointer:
            info.AddSigByte( SignatureElementTypes.FnPtr );
            info.WriteMethodSignature( ( (FunctionPointerTypeSignature) type ).MethodSignature );
            throw new NotImplementedException();
         case TypeSignatureKind.Pointer:
            info.AddSigByte( SignatureElementTypes.Ptr );
            var ptr = (PointerTypeSignature) type;
            info.WriteCustomModifiers( ptr.CustomModifiers );
            info.WriteTypeSignature( ptr.PointerType );
            break;

      }
   }

   private static void WriteMethodSignature( this ByteArrayHelper info, AbstractMethodSignature method )
   {
      if ( method != null )
      {
         var starter = method.SignatureStarter;
         info.AddSigStarterByte( method.SignatureStarter );

         if ( starter.IsGeneric() )
         {
            info.AddCompressedUInt32( method.GenericArgumentCount );
         }

         info.AddCompressedUInt32( method.Parameters.Count );

         info.WriteParameterSignature( method.ReturnType );

         foreach ( var param in method.Parameters )
         {
            info.WriteParameterSignature( param );
         }

         if ( method.SignatureKind == SignatureKind.MethodReference )
         {
            var mRef = (MethodReferenceSignature) method;
            if ( mRef.VarArgsParameters.Count > 0 )
            {
               info.AddSigByte( SignatureElementTypes.Sentinel );
               foreach ( var v in mRef.VarArgsParameters )
               {
                  info.WriteParameterSignature( v );
               }
            }
         }
      }
   }

   private static void WriteParameterSignature( this ByteArrayHelper info, ParameterSignature parameter )
   {
      info.WriteCustomModifiers( parameter.CustomModifiers );
      if ( SimpleTypeSignature.TypedByRef.Equals( parameter.Type ) )
      {
         info.AddSigByte( SignatureElementTypes.TypedByRef );
      }
      else
      {
         if ( parameter.IsByRef )
         {
            info.AddSigByte( SignatureElementTypes.ByRef );
         }

         info.WriteTypeSignature( parameter.Type );
      }
   }

   private static void WriteConstantValue( this ByteArrayHelper info, Object constant )
   {
      if ( constant == null )
      {
         info.AddUncompressedInt32( 0 );
      }
      else
      {
         switch ( Type.GetTypeCode( constant.GetType() ) )
         {
            case TypeCode.Boolean:
               info.AddByte( Convert.ToBoolean( constant ) ? (Byte) 1 : (Byte) 0 );
               break;
            case TypeCode.SByte:
               info.AddSByte( Convert.ToSByte( constant ) );
               break;
            case TypeCode.Byte:
               info.AddByte( Convert.ToByte( constant ) );
               break;
            case TypeCode.Char:
               info.AddUncompressedUInt16( Convert.ToUInt16( Convert.ToChar( constant ) ) );
               break;
            case TypeCode.Int16:
               info.AddUncompressedInt16( Convert.ToInt16( constant ) );
               break;
            case TypeCode.UInt16:
               info.AddUncompressedUInt16( Convert.ToUInt16( constant ) );
               break;
            case TypeCode.Int32:
               info.AddUncompressedInt32( Convert.ToInt32( constant ) );
               break;
            case TypeCode.UInt32:
               info.AddUncompressedUInt32( Convert.ToUInt32( constant ) );
               break;
            case TypeCode.Int64:
               info.AddUncompressedInt64( Convert.ToInt64( constant ) );
               break;
            case TypeCode.UInt64:
               info.AddUncompressedUInt64( Convert.ToUInt64( constant ) );
               break;
            case TypeCode.Single:
               info.AddUncompressedSingle( Convert.ToSingle( constant ) );
               break;
            case TypeCode.Double:
               info.AddUncompressedDouble( Convert.ToDouble( constant ) );
               break;
            case TypeCode.String:
               info.AddNormalString( Convert.ToString( constant ) );
               break;
            default:
               info.AddUncompressedInt32( 0 );
               break;
         }
      }
   }

   private static void WriteCustomAttributeSignature( this ByteArrayHelper info, CILMetaData md, Int32 idx )
   {
      var ca = md.CustomAttributeDefinitions.TableContents[idx];
      var attrData = ca.Signature as CustomAttributeSignature;

      var ctor = ca.Type;
      var sig = ctor.Table == Tables.MethodDef ?
         md.MethodDefinitions.TableContents[ctor.Index].Signature :
         md.MemberReferences.TableContents[ctor.Index].Signature as AbstractMethodSignature;

      if ( sig == null )
      {
         throw new InvalidOperationException( "Custom attribute constructor signature was null (custom attribute at index " + idx + ", ctor: " + ctor + ")." );
      }
      else if ( sig.Parameters.Count != attrData.TypedArguments.Count )
      {
         throw new InvalidOperationException( "Custom attribute constructor has different amount of parameters than supplied custom attribute data (custom attribute at index " + idx + ", ctor: " + ctor + ")." );
      }


      // Prolog
      info.AddByte( 1 );
      info.AddByte( 0 );

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
            throw new InvalidOperationException( "Failed to resolve custom attribute type for constructor parameter (custom attribute at index " + idx + ", ctor: " + ctor + ", param: " + i + ")." );
         }
         info.WriteCustomAttributeFixedArg( caType, arg.Value );
      }

      // Named args
      info.AddUncompressedUInt16( (UInt16) attrData.NamedArguments.Count );
      foreach ( var arg in attrData.NamedArguments )
      {
         info.WriteCustomAttributeNamedArg( arg );
      }
   }

   private static void WriteCustomAttributeFixedArg( this ByteArrayHelper info, CustomAttributeArgumentType argType, Object arg )
   {
      switch ( argType.ArgumentTypeKind )
      {
         case CustomAttributeArgumentTypeKind.Array:
            if ( arg == null )
            {
               info.AddUncompressedInt32( unchecked( (Int32) 0xFFFFFFFF ) );
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

               info.AddUncompressedInt32( array.Length );
               foreach ( var elem in array )
               {
                  info.WriteCustomAttributeFixedArg( argType, elem );
               }
            }
            break;
         case CustomAttributeArgumentTypeKind.Simple:
            switch ( ( (CustomAttributeArgumentTypeSimple) argType ).SimpleType )
            {
               case SignatureElementTypes.Boolean:
                  info.AddByte( Convert.ToBoolean( arg ) ? (Byte) 1 : (Byte) 0 );
                  break;
               case SignatureElementTypes.I1:
                  info.AddSByte( Convert.ToSByte( arg ) );
                  break;
               case SignatureElementTypes.U1:
                  info.AddByte( Convert.ToByte( arg ) );
                  break;
               case SignatureElementTypes.Char:
                  info.AddUncompressedUInt16( Convert.ToUInt16( Convert.ToChar( arg ) ) );
                  break;
               case SignatureElementTypes.I2:
                  info.AddUncompressedInt16( Convert.ToInt16( arg ) );
                  break;
               case SignatureElementTypes.U2:
                  info.AddUncompressedUInt16( Convert.ToUInt16( arg ) );
                  break;
               case SignatureElementTypes.I4:
                  info.AddUncompressedInt32( Convert.ToInt32( arg ) );
                  break;
               case SignatureElementTypes.U4:
                  info.AddUncompressedUInt32( Convert.ToUInt32( arg ) );
                  break;
               case SignatureElementTypes.I8:
                  info.AddUncompressedInt64( Convert.ToInt64( arg ) );
                  break;
               case SignatureElementTypes.U8:
                  info.AddUncompressedUInt64( Convert.ToUInt64( arg ) );
                  break;
               case SignatureElementTypes.R4:
                  info.AddUncompressedSingle( Convert.ToSingle( arg ) );
                  break;
               case SignatureElementTypes.R8:
                  info.AddUncompressedDouble( Convert.ToDouble( arg ) );
                  break;
               case SignatureElementTypes.String:
                  info.AddCAString( arg == null ? null : Convert.ToString( arg ) );
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
                  info.AddCAString( typeStr );
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
                  info.WriteCustomAttributeFieldOrPropType( ref argType, ref arg );
                  info.WriteCustomAttributeFixedArg( argType, arg );
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
            info.WriteConstantValue( valueToWrite );
            break;
      }
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

   private static void WriteCustomAttributeNamedArg( this ByteArrayHelper info, CustomAttributeNamedArgument arg )
   {
      var elem = arg.IsField ? SignatureElementTypes.CA_Field : SignatureElementTypes.CA_Property;
      info.AddSigByte( elem );
      var typedValueValue = arg.Value.Value;
      var caType = arg.FieldOrPropertyType;
      info.WriteCustomAttributeFieldOrPropType( ref caType, ref typedValueValue );
      info.AddCAString( arg.Name );
      info.WriteCustomAttributeFixedArg( caType, typedValueValue );
   }

   private static void WriteCustomAttributeFieldOrPropType( this ByteArrayHelper info, ref CustomAttributeArgumentType type, ref Object value, Boolean processEnumTypeAndValue = true )
   {
      if ( type == null )
      {
         throw new InvalidOperationException( "Custom attribute signature typed argument type was null." );
      }

      switch ( type.ArgumentTypeKind )
      {
         case CustomAttributeArgumentTypeKind.Array:
            info.AddSigByte( SignatureElementTypes.SzArray );
            var arrayType = ( (CustomAttributeArgumentTypeArray) type ).ArrayType;
            Object dummy = null;
            info.WriteCustomAttributeFieldOrPropType( ref arrayType, ref dummy, false );
            break;
         case CustomAttributeArgumentTypeKind.Simple:
            var sigStarter = ( (CustomAttributeArgumentTypeSimple) type ).SimpleType;
            if ( sigStarter == SignatureElementTypes.Object )
            {
               sigStarter = SignatureElementTypes.CA_Boxed;
            }
            info.AddSigByte( sigStarter );
            break;
         case CustomAttributeArgumentTypeKind.TypeString:
            info.AddSigByte( SignatureElementTypes.CA_Enum );
            info.AddCAString( ( (CustomAttributeArgumentTypeEnum) type ).TypeString );
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
   }

   private static void WriteMarshalInfo( this ByteArrayHelper info, MarshalingInfo marshal )
   {
      if ( marshal != null )
      {
         info.AddCompressedUInt32( (Int32) marshal.Value );
         if ( !marshal.Value.IsNativeInstric() )
         {
            // Apparently Microsoft's implementation differs from ECMA-335 standard:
            // there the index of first parameter is 1, here all indices are zero-based.
            switch ( (UnmanagedType) marshal.Value )
            {
               case UnmanagedType.ByValTStr:
                  info.AddCompressedUInt32( marshal.ConstSize );
                  break;
               case UnmanagedType.IUnknown:
               case UnmanagedType.IDispatch:
                  if ( marshal.IIDParameterIndex >= 0 )
                  {
                     info.AddCompressedUInt32( marshal.IIDParameterIndex );
                  }
                  break;
               case UnmanagedType.SafeArray:
                  if ( marshal.SafeArrayType != VarEnum.VT_EMPTY )
                  {
                     info.AddCompressedUInt32( (Int32) marshal.SafeArrayType );
                     if ( VarEnum.VT_USERDEFINED == marshal.SafeArrayType )
                     {
                        info.AddCAString( marshal.SafeArrayUserDefinedType );
                     }
                  }
                  break;
               case UnmanagedType.ByValArray:
                  info.AddCompressedUInt32( marshal.ConstSize );
                  if ( marshal.ArrayType != MarshalingInfo.NATIVE_TYPE_MAX )
                  {
                     info.AddCompressedUInt32( (Int32) marshal.ArrayType );
                  }
                  break;
               case UnmanagedType.LPArray:
                  info.AddCompressedUInt32( (Int32) marshal.ArrayType );
                  var hasSize = marshal.SizeParameterIndex != MarshalingInfo.NO_INDEX;
                  info.AddCompressedUInt32( hasSize ? marshal.SizeParameterIndex : 0 );
                  if ( marshal.ConstSize != MarshalingInfo.NO_INDEX )
                  {
                     info.AddCompressedUInt32( marshal.ConstSize );
                     info.AddCompressedUInt32( hasSize ? 1 : 0 ); // Indicate whether size-parameter was specified
                  }
                  break;
               case UnmanagedType.CustomMarshaler:
                  // For some reason, there are two compressed ints at this point
                  info.AddCompressedUInt32( 0 );
                  info.AddCompressedUInt32( 0 );
                  info.AddCAString( marshal.MarshalType );
                  info.AddCAString( marshal.MarshalCookie ?? "" );
                  break;
               default:
                  break;
            }
         }
      }
   }

   private static void WriteSecuritySignature( this ByteArrayHelper info, SecurityDefinition security, ByteArrayHelper aux )
   {
      // TODO currently only newer format, .NET 1 format not supported for writing
      info.AddByte( MetaDataConstants.DECL_SECURITY_HEADER );
      var permissions = security.PermissionSets;
      info.AddCompressedUInt32( permissions.Count );
      foreach ( var sec in permissions )
      {
         info.AddCAString( sec.SecurityAttributeType );
         var secInfo = sec as SecurityInformation;
         Byte[] secInfoBLOB;
         if ( secInfo != null )
         {
            // Store arguments in separate bytes
            foreach ( var arg in secInfo.NamedArguments )
            {
               aux.WriteCustomAttributeNamedArg( arg );
            }
            // Now write to sec blob
            secInfoBLOB = aux.CreateByteArray();
            // The length of named arguments blob
            info.AddCompressedUInt32( secInfoBLOB.Length + BitUtils.GetEncodedUIntSize( secInfo.NamedArguments.Count ) );
            // The amount of named arguments
            info.AddCompressedUInt32( secInfo.NamedArguments.Count );
         }
         else
         {
            var rawBytes = ( (RawSecurityInformation) sec ).Bytes;
            info.AddCompressedUInt32( rawBytes.Length );
            secInfoBLOB = ( (RawSecurityInformation) sec ).Bytes;
         }

         info.AddBytes( secInfoBLOB );
      }

   }

   private static void WriteLocalsSignature( this ByteArrayHelper info, LocalVariablesSignature sig )
   {
      if ( sig != null )
      {
         info.AddSigStarterByte( SignatureStarters.LocalSignature );
         var locals = sig.Locals;
         info.AddCompressedUInt32( locals.Count );
         foreach ( var local in locals )
         {
            info.WriteLocalSignature( local );
         }
      }
   }

   private static void WriteLocalSignature( this ByteArrayHelper info, LocalVariableSignature sig )
   {
      if ( SimpleTypeSignature.TypedByRef.Equals( sig.Type ) )
      {
         info.AddSigByte( SignatureElementTypes.TypedByRef );
      }
      else
      {
         info.WriteCustomModifiers( sig.CustomModifiers );
         if ( sig.IsPinned )
         {
            info.AddSigByte( SignatureElementTypes.Pinned );
         }

         if ( sig.IsByRef )
         {
            info.AddSigByte( SignatureElementTypes.ByRef );
         }
         info.WriteTypeSignature( sig.Type );
      }
   }

   private static void WritePropertySignature( this ByteArrayHelper info, PropertySignature sig )
   {
      if ( sig != null )
      {
         var starter = SignatureStarters.Property;
         if ( sig.HasThis )
         {
            starter |= SignatureStarters.HasThis;
         }
         info.AddSigStarterByte( starter );
         info.AddCompressedUInt32( sig.Parameters.Count );
         info.WriteCustomModifiers( sig.CustomModifiers );
         info.WriteTypeSignature( sig.PropertyType );

         foreach ( var param in sig.Parameters )
         {
            info.WriteParameterSignature( param );
         }
      }
   }

   private static void WriteMethodSpecSignature( this ByteArrayHelper info, GenericMethodSignature sig )
   {
      info.AddSigStarterByte( SignatureStarters.MethodSpecGenericInst );
      info.AddCompressedUInt32( sig.GenericArguments.Count );
      foreach ( var gArg in sig.GenericArguments )
      {
         info.WriteTypeSignature( gArg );
      }
   }


}