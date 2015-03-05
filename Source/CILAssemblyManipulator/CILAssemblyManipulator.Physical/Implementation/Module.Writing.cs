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
      internal const Int32 MIN_FILE_ALIGNMENT = 0x200;
      internal const UInt64 IMAGE_BASE_MULTIPLE = 0x10000; // ECMA-335, p. 279
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


      internal const String CODE_SECTION_NAME = ".text";
      internal const String RESOURCE_SECTION_NAME = ".rsrc";
      internal const String RELOCATION_SECTION_NAME = ".reloc";

      private static readonly Encoding MetaDataStringEncoding = new UTF8Encoding( false, true );

      internal static void WriteModule(
         CILMetaData md,
         HeadersData headers,
         EmittingArguments eArgs,
         Stream sink
         )
      {
         // 1. Check arguments
         ArgumentValidator.ValidateNotNull( "Stream", sink );
         ArgumentValidator.ValidateNotNull( "Emitting arguments", eArgs );

         Boolean isPE64, hasRelocations;
         UInt16 peOptionalHeaderSize;
         UInt32 numSections, iatSize;
         CheckHeaders( headers, out isPE64, out hasRelocations, out numSections, out peOptionalHeaderSize, out iatSize );

         // 2. Initialize variables
         var fAlign = headers.FileAlignment;
         var sAlign = headers.SectionAlignment;
         var importHintName = headers.ImportHintName;
         var imageBase = headers.ImageBase;
         var moduleKind = headers.ModuleKind;
         var strongName = eArgs.StrongName;


         var clrEntryPointToken = 0;
         var entryPoint = headers.CLREntryPointIndex;
         if ( entryPoint.HasValue )
         {
            clrEntryPointToken = TokenUtils.EncodeToken( entryPoint.Value.Table, entryPoint.Value.Index + 1 );
         }

         // 3. Write module
         // Start emitting headers
         // MS-DOS header
         var currentArray = new Byte[HeaderFieldOffsetsAndLengths.DOS_HEADER_AND_PE_SIG.Length];
         Array.Copy( HeaderFieldOffsetsAndLengths.DOS_HEADER_AND_PE_SIG, currentArray, HeaderFieldOffsetsAndLengths.DOS_HEADER_AND_PE_SIG.Length );
         sink.Write( currentArray );

         // PE file header
         currentArray = new Byte[HeaderFieldOffsetsAndLengths.PE_FILE_HEADER_SIZE];
         var characteristics = HeaderFieldPossibleValues.IMAGE_FILE_EXECUTABLE_IMAGE | ( isPE64 ? HeaderFieldPossibleValues.IMAGE_FILE_LARGE_ADDRESS_AWARE : HeaderFieldPossibleValues.IMAGE_FILE_32BIT_MACHINE );
         if ( moduleKind.IsDLL() )
         {
            characteristics |= HeaderFieldPossibleValues.IMAGE_FILE_DLL;
         }
         var idx = 0;
         currentArray
            .WriteUInt16LEToBytes( ref idx, (UInt16) headers.Machine )
            .WriteUInt16LEToBytes( ref idx, (UInt16) numSections )
            .WriteInt32LEToBytes( ref idx, Convert.ToInt32( DateTime.Now.Subtract( new DateTime( 1970, 1, 1, 0, 0, 0 ) ).TotalSeconds ) )
            .Skip( ref idx, 8 )
            .WriteUInt16LEToBytes( ref idx, peOptionalHeaderSize )
            .WriteInt16LEToBytes( ref idx, (Int16) characteristics );
         sink.Write( currentArray );

         // PE optional header + section headers + padding + IAT + CLI header + Strong signature
         var codeSectionVirtualOffset = sAlign;
         // Strong name signature

         var useStrongName = strongName != null;
         var snSize = 0u;
         var snRVA = 0u;
         var snPadding = 0u;
         var thisAssemblyPublicKey = md.AssemblyDefinitions.Count > 0 ?
            md.AssemblyDefinitions[0].AssemblyInformation.PublicKeyOrToken :
            null;
         var delaySign = eArgs.DelaySign || ( !useStrongName && !thisAssemblyPublicKey.IsNullOrEmpty() );
         RSAParameters rParams;
         var signingAlgorithm = AssemblyHashAlgorithm.SHA1;
         var computingHash = useStrongName || delaySign;

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
                  thisAssemblyPublicKey = eArgs.ExtractPublicKeyFromCSP( strongName.ContainerName );
               }
               pkToProcess = thisAssemblyPublicKey;
            }
            else
            {
               // Get public key from BLOB
               pkToProcess = strongName.KeyPair.ToArray();
            }

            // Create RSA parameters and process public key so that it will have proper, full format.
            Byte[] pk;
            rParams = CryptoUtils.CreateSigningInformationFromKeyBLOB( pkToProcess, algoOverride, out pk, out signingAlgorithm );
            thisAssemblyPublicKey = pk;
            snSize = (UInt32) rParams.Modulus.Length;
         }
         else
         {
            rParams = default( RSAParameters );
         }

         var hashStreamArgsForThisHashComputing = new Lazy<HashStreamLoadEventArgs>( () => eArgs.LaunchHashStreamEvent( signingAlgorithm, true ), LazyThreadSafetyMode.None );
         var hashStreamArgsForTokenComputing = signingAlgorithm == AssemblyHashAlgorithm.SHA1 ?
            hashStreamArgsForThisHashComputing :
            new Lazy<HashStreamLoadEventArgs>( () => eArgs.LaunchHashStreamEvent( AssemblyHashAlgorithm.SHA1, false ) );

         if ( useStrongName || delaySign )
         {
            snRVA = codeSectionVirtualOffset + iatSize + HeaderFieldOffsetsAndLengths.CLI_HEADER_SIZE;
            if ( snSize <= 32 )
            {
               // The "Standard Public Key", ECMA-335 p. 116
               // It is replaced by the runtime with 128 bytes key
               snSize = 128;
            }
            snPadding = BitUtils.MultipleOf4( snSize ) - snSize;
         }

         var revisitableOffset = HeaderFieldOffsetsAndLengths.DOS_HEADER_AND_PE_SIG.Length + currentArray.Length;
         var revisitableArraySize = fAlign + iatSize + HeaderFieldOffsetsAndLengths.CLI_HEADER_SIZE - revisitableOffset;
         // Cheat a bit - skip now, and re-visit it after all other emitting is done
         sink.Seek( revisitableArraySize + snSize + snPadding, SeekOrigin.Current );

         // First section
         // Start with method ILs
         // Current offset within section
         var currentOffset = iatSize + snSize + snPadding + HeaderFieldOffsetsAndLengths.CLI_HEADER_SIZE;
         UserStringHeapEmittingInfo usersStrings; IList<UInt32> methodRVAs;
         WriteMethodDefsIL( md, sink, codeSectionVirtualOffset, isPE64, ref currentOffset, out usersStrings, out methodRVAs );

         // Write manifest resources & field RVAs here
         UInt32 mResRVA, mResSize; IList<UInt32?> mResInfo; IList<UInt32> fieldRVAs;
         WriteDataBeforeMD( md, sink, codeSectionVirtualOffset, isPE64, out mResRVA, out mResSize, out mResInfo, out fieldRVAs, ref currentOffset );

         var mdRVA = codeSectionVirtualOffset + currentOffset;
         var mdSize = md.WriteMetaData( sink, mdRVA, eArgs, methodRVAs, mResInfo );
         mdRVA += addedToOffsetBeforeMD;
         currentOffset += mdSize + addedToOffsetBeforeMD;

         // Pad
         sink.SkipToNextAlignment( ref currentOffset, 0x4 );

         // Write debug header if present
         var dbgInfo = headers.DebugInformation;
         var dbgRVA = 0u;
         if ( dbgInfo != null )
         {
            dbgRVA = codeSectionVirtualOffset + currentOffset;
            var dbgData = dbgInfo.DebugData;
            currentArray = new Byte[MetaDataConstants.DEBUG_DD_SIZE + dbgData.Length];
            idx = 0;
            currentArray
               .WriteInt32LEToBytes( ref idx, dbgInfo.Characteristics )
               .WriteInt32LEToBytes( ref idx, dbgInfo.Timestamp )
               .WriteInt16LEToBytes( ref idx, dbgInfo.VersionMajor )
               .WriteInt16LEToBytes( ref idx, dbgInfo.VersionMinor )
               .WriteInt32LEToBytes( ref idx, MetaDataConstants.CODE_VIEW_DEBUG_TYPE )
               .WriteInt32LEToBytes( ref idx, dbgData.Length )
               .WriteUInt32LEToBytes( ref idx, dbgRVA + MetaDataConstants.DEBUG_DD_SIZE )
               .WriteUInt32LEToBytes( ref idx, fAlign + currentOffset + (UInt32) idx + 4 ) // Pointer to data, end Debug Data Directory
               .BlockCopyFrom( ref idx, dbgData );
            sink.Write( currentArray );
            currentOffset += (UInt32) currentArray.Length;
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
            currentArray = new Byte[importDirectorySize];
            idx = 0;
            currentArray
               .WriteUInt32LEToBytes( ref idx, codeSectionVirtualOffset + currentOffset + (UInt32) currentArray.Length ) // RVA of the ILT
               .WriteInt32LEToBytes( ref idx, 0 ) // DateTimeStamp
               .WriteInt32LEToBytes( ref idx, 0 ) // ForwarderChain
               .WriteUInt32LEToBytes( ref idx, codeSectionVirtualOffset + currentOffset + (UInt32) currentArray.Length + HeaderFieldOffsetsAndLengths.ILT_SIZE + HeaderFieldOffsetsAndLengths.HINT_NAME_MIN_SIZE + (UInt32) importHintName.Length + 1 ) // RVA of Import Directory name (mscoree.dll)  
               .WriteUInt32LEToBytes( ref idx, codeSectionVirtualOffset ); // RVA of Import Address Table
            // The rest are zeroes
            sink.Write( currentArray );
            currentOffset += (UInt32) currentArray.Length;

            // ILT
            currentArray = new Byte[HeaderFieldOffsetsAndLengths.ILT_SIZE];
            idx = 0;
            currentArray
               .WriteUInt32LEToBytes( ref idx, codeSectionVirtualOffset + currentOffset + (UInt32) currentArray.Length ); // RVA of the hint/name table
            // The rest are zeroes
            sink.Write( currentArray );
            currentOffset += (UInt32) currentArray.Length;

            // Hint/Name table
            currentArray = new Byte[HeaderFieldOffsetsAndLengths.HINT_NAME_MIN_SIZE + importHintName.Length + 1];
            hnRVA = currentOffset + codeSectionVirtualOffset;
            // Skip first two bytes
            idx = HeaderFieldOffsetsAndLengths.HINT_NAME_MIN_SIZE;
            currentArray.WriteASCIIString( ref idx, importHintName, true );
            sink.Write( currentArray );
            currentOffset += (UInt32) currentArray.Length;

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
            currentArray = new Byte[sizeof( Int16 ) + sizeof( Int32 )];
            idx = 0;
            currentArray
               .WriteInt16LEToBytes( ref idx, headers.EntryPointInstruction )
               .WriteUInt32LEToBytes( ref idx, (UInt32) imageBase + codeSectionVirtualOffset );
            sink.Write( currentArray );
            currentOffset += (UInt32) currentArray.Length;
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

            currentArray = new Byte[HeaderFieldOffsetsAndLengths.RELOC_ARRAY_BASE_SIZE];
            idx = 0;
            currentArray
               .WriteUInt32LEToBytes( ref idx, pageRVA )
               .WriteUInt32LEToBytes( ref idx, HeaderFieldOffsetsAndLengths.RELOC_ARRAY_BASE_SIZE ) // Block size
               .WriteUInt32LEToBytes( ref idx, ( RELOCATION_FIXUP_TYPE << 12 ) + relocRVA - pageRVA ); // Type (high 4 bits) + Offset (lower 12 bits) + dummy entry (16 bits)
            sink.Write( currentArray );
            currentOffset += (UInt32) currentArray.Length;

            relocSectionInfo = new SectionInfo( sink, prevSectionInfo, currentOffset, sAlign, fAlign, true );
            prevSectionInfo = relocSectionInfo;
         }

         // Revisit PE optional header + section headers + padding + IAT + CLI header
         currentArray = new Byte[revisitableArraySize];
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
         currentArray.Skip( ref idx, (Int32) ( fAlign - (UInt32) revisitableOffset - idx ) );

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
         if ( idx != currentArray.Length )
         {
            throw new Exception( "Something went wrong when emitting file headers. Emitted " + idx + " bytes, but was supposed to emit " + currentArray.Length + " bytes." );
         }
#endif
         sink.Seek( revisitableOffset, SeekOrigin.Begin );
         sink.Write( currentArray );

         if ( computingHash )
         {
            if ( !delaySign )
            {
               // Try create RSA first
               var rsaArgs = strongName.ContainerName == null ? new RSACreationEventArgs( rParams ) : new RSACreationEventArgs( strongName.ContainerName );
               eArgs.LaunchRSACreationEvent( rsaArgs );
               using ( var rsa = rsaArgs.RSA )
               {
                  var buffer = new Byte[MetaDataConstants.STREAM_COPY_BUFFER_SIZE];
                  var hashEvtArgs = hashStreamArgsForThisHashComputing.Value;
                  var hashStream = hashEvtArgs.CryptoStream;
                  var hashGetter = hashEvtArgs.HashGetter;
                  var transform = hashEvtArgs.Transform;

                  RSASignatureCreationEventArgs sigArgs;
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
                     sigArgs = new RSASignatureCreationEventArgs( rsa, signingAlgorithm, hashGetter() );
                  }


                  eArgs.LaunchRSASignatureCreationEvent( sigArgs );
                  var strongNameArray = sigArgs.Signature;
                  if ( snSize != strongNameArray.Length )
                  {
                     throw new CryptographicException( "Calculated and actual strong name size differ (calculated: " + snSize + ", actual: " + strongNameArray.Length + ")." );
                  }
                  Array.Reverse( strongNameArray );

                  // Write strong name
                  sink.Seek( snRVA - codeSectionVirtualOffset + fAlign, SeekOrigin.Begin );
                  sink.Write( strongNameArray );
               }
            }

            currentArray = new Byte[8];
            idx = 0;
            currentArray.WriteDataDirectory( ref idx, snRVA, snSize );
            sink.Seek( snDataDirOffset, SeekOrigin.Begin );
            sink.Write( currentArray );
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
         if ( eArgs.FileAlignment < MIN_FILE_ALIGNMENT )
         {
            eArgs.FileAlignment = MIN_FILE_ALIGNMENT;
         }
         else
         {
            eArgs.FileAlignment = FLP2( eArgs.FileAlignment );
         }

         if ( eArgs.ImageBase < IMAGE_BASE_MULTIPLE )
         {
            eArgs.ImageBase = IMAGE_BASE_MULTIPLE;
         }
         else
         {
            eArgs.ImageBase -= eArgs.ImageBase % IMAGE_BASE_MULTIPLE;
         }
         if ( eArgs.SectionAlignment <= eArgs.FileAlignment )
         {
            throw new ArgumentException( "Section alignment " + eArgs.SectionAlignment + " must be greater than file alignment " + eArgs.FileAlignment + "." );
         }

         isPE64 = eArgs.Machine.RequiresPE64();
         hasRelocations = eArgs.Machine.RequiresRelocations();
         numSections = isPE64 ? 1u : 2u; // TODO resource-section
         peOptionalHeaderSize = isPE64 ? HeaderFieldOffsetsAndLengths.PE_OPTIONAL_HEADER_SIZE_64 : HeaderFieldOffsetsAndLengths.PE_OPTIONAL_HEADER_SIZE_32;
         iatSize = hasRelocations ? HeaderFieldOffsetsAndLengths.IAT_SIZE : 0u; // No Import tables if no relocations
         if ( !isPE64 )
         {
            eArgs.ImageBase = CheckValueFor32PE( eArgs.ImageBase );
            eArgs.StackReserve = CheckValueFor32PE( eArgs.StackReserve );
            eArgs.StackCommit = CheckValueFor32PE( eArgs.StackCommit );
            eArgs.HeapReserve = CheckValueFor32PE( eArgs.HeapReserve );
            eArgs.HeapCommit = CheckValueFor32PE( eArgs.HeapCommit );
         }
      }

      private static UInt64 CheckValueFor32PE( UInt64 value )
      {
         return Math.Min( UInt32.MaxValue, value );
      }

      private static UInt16 GetSubSystem( ModuleKind kind, Boolean isCEApp = false )
      {
         return ModuleKind.Windows == kind ? ( isCEApp ? HeaderFieldPossibleValues.IMAGE_SUBSYSTEM_WINDOWS_CE_GUI : HeaderFieldPossibleValues.IMAGE_SUBSYSTEM_WINDOWS_GUI ) : HeaderFieldPossibleValues.IMAGE_SUBSYSTEM_WINDOWS_CUI;
      }

      // From http://my.safaribooksonline.com/book/information-technology-and-software-development/0201914654/power-of-2-boundaries/ch03lev1sec2
      private static UInt32 FLP2( UInt32 x )
      {
         x = x | ( x >> 1 );
         x = x | ( x >> 2 );
         x = x | ( x >> 4 );
         x = x | ( x >> 8 );
         x = x | ( x >> 16 );
         return x - ( x >> 1 );
      }

      private static void WriteMethodDefsIL(
         CILMetaData md,
         Stream sink,
         UInt32 codeSectionVirtualOffset,
         Boolean isPE64,
         ref UInt32 currentOffset,
         out UserStringHeapEmittingInfo usersStrings,
         out IList<UInt32> methodRVAs
         )
      {
         // Create users string heap
         usersStrings = new UserStringHeapEmittingInfo();
         methodRVAs = new List<UInt32>( md.MethodDefinitions.Count );

         for ( var i = 0; i < md.MethodDefinitions.Count; )
         {
            var method = md.MethodDefinitions[i];
            UInt32 thisMethodRVA;
            if ( method.HasILMethodBody() )
            {
               Boolean isTiny;
               var array = new MethodILWriter( md, i, usersStrings )
                  .PerformEmitting( currentOffset, out isTiny );
               if ( !isTiny )
               {
                  sink.SkipToNextAlignment( ref currentOffset, 4 );
               }
               thisMethodRVA = codeSectionVirtualOffset + currentOffset;
               sink.Write( array );
               currentOffset += (UInt32) array.Length;
            }
            else
            {
               thisMethodRVA = 0u;
            }
            methodRVAs.Add( thisMethodRVA );
         }

         // Write padding
         sink.SkipToNextAlignment( ref currentOffset, isPE64 ? 0x10u : 0x04u );
      }

      private static void WriteDataBeforeMD(
         CILMetaData md,
         Stream sink,
         UInt32 codeSectionVirtualOffset,
         Boolean isPE64,
         out UInt32 resourcesRVA,
         out UInt32 resourcesSize,
         out IList<UInt32?> mResInfo,
         out IList<UInt32> fieldRVAs,
         ref UInt32 currentOffset
         )
      {
         // Write manifest resources here
         var mResInfos = md.ManifestResources;
         mResInfo = new List<UInt32?>( mResInfos.Count );
         resourcesSize = 0u;
         var tmpArray = new Byte[4];
         foreach ( var mr in mResInfos )
         {
            if ( mr.Implementation == null )
            {
               var data = mr.DataInCurrentFile;
               if ( data == null )
               {
                  data = Empty<Byte>.Array;
               }
               mResInfo.Add( resourcesSize );
               tmpArray.WriteInt32LEToBytesNoRef( 0, data.Length );
               sink.Write( tmpArray );
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
         var mdFRVAs = md.FieldRVAs;
         fieldRVAs = new List<UInt32>( mdFRVAs.Count );
         var fRVA = 0u;
         foreach ( var fRVAInfo in mdFRVAs )
         {
            fieldRVAs.Add( codeSectionVirtualOffset + fRVA );
            var data = fRVAInfo.Data;
            sink.Write( data );
            fRVA += (UInt32) data.Length;
         }

         if ( fRVA > 0u )
         {
            currentOffset += fRVA;
         }

         sink.SkipToNextAlignment( ref currentOffset, 0x04u );
      }

      //This assumes that sink offset is at multiple of 4.
      private static UInt32 WriteMetaData(
         CILMetaData md,
         Stream sink,
         UInt32 currentRVA,
         HeadersData headers,
         EmittingArguments eArgs,
         UserStringHeapEmittingInfo userStrings,
         IList<UInt32> methodRVAs,
         IList<UInt32> embeddedManifestResourceOffsets,
         Byte[] zeroArray,
         out UInt32 addedToOffsetBeforeMD
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

         BLOBContainer blob; SystemStringHeapEmittingInfo sysStrings; IList<Guid> guids; Object[] heapInfos;
         CreateMDHeaps( md, out blob, out sysStrings, out guids, out heapInfos );

         var hasSysStrings = sysStrings.Accessed;
         var hasUserStrings = userStrings.Accessed;
         var hasGuids = guids.Count > 0;
         var hasBlobs = blob.Accessed;

         if ( hasSysStrings )
         {
            // Store offset to array to streamHeaders
            // This offset, for each stream, tells where to write first field of stream header (offset from metadata root)
            streamHeaders[SYS_STRINGS_IDX] = 8 + BitUtils.MultipleOf4( Consts.SYS_STRING_STREAM_NAME.Length + 1 );
            streamSizes[SYS_STRINGS_IDX] = BitUtils.MultipleOf4( sysStrings.Size );
         }
         if ( hasUserStrings )
         {
            streamHeaders[USER_STRINGS_IDX] = 8 + BitUtils.MultipleOf4( Consts.USER_STRING_STREAM_NAME.Length + 1 );
            streamSizes[USER_STRINGS_IDX] = BitUtils.MultipleOf4( userStrings.Size );
         }
         if ( hasGuids )
         {
            streamHeaders[GUID_IDX] = 8 + BitUtils.MultipleOf4( Consts.GUID_STREAM_NAME.Length + 1 );
            streamSizes[GUID_IDX] = BitUtils.MultipleOf4( ( (UInt32) guids.Count ) * Consts.GUID_SIZE );
         }
         if ( hasBlobs )
         {
            streamHeaders[BLOB_IDX] = 8 + BitUtils.MultipleOf4( Consts.BLOB_STREAM_NAME.Length + 1 );
            streamSizes[BLOB_IDX] = BitUtils.MultipleOf4( blob.Size );
         }

         var wideSysStringIndex = streamSizes[SYS_STRINGS_IDX] > UInt16.MaxValue;
         var wideGUIDIndex = streamSizes[GUID_IDX] > UInt16.MaxValue;
         var wideBLOBIndex = streamSizes[BLOB_IDX] > UInt16.MaxValue;
         var sysStringIndexSize = wideSysStringIndex ? 4 : 2;
         var guidIndexSize = wideGUIDIndex ? 4 : 2;
         var blobIndexSize = wideBLOBIndex ? 4 : 2;

         var tableSizes = new Int32[Consts.AMOUNT_OF_TABLES];
         tableSizes[(Int32) Tables.Module] = md.ModuleDefinitions.Count;
         tableSizes[(Int32) Tables.TypeRef] = md.TypeReferences.Count;
         tableSizes[(Int32) Tables.TypeDef] = md.TypeDefinitions.Count;
         tableSizes[(Int32) Tables.Field] = md.FieldDefinitions.Count;
         tableSizes[(Int32) Tables.MethodDef] = md.MethodDefinitions.Count;
         tableSizes[(Int32) Tables.Parameter] = md.ParameterDefinitions.Count;
         tableSizes[(Int32) Tables.InterfaceImpl] = md.InterfaceImplementations.Count;
         tableSizes[(Int32) Tables.MemberRef] = md.MemberReferences.Count;
         tableSizes[(Int32) Tables.Constant] = md.ConstantDefinitions.Count;
         tableSizes[(Int32) Tables.CustomAttribute] = md.CustomAttributeDefinitions.Count;
         tableSizes[(Int32) Tables.FieldMarshal] = md.FieldMarshals.Count;
         tableSizes[(Int32) Tables.DeclSecurity] = md.SecurityDefinitions.Count;
         tableSizes[(Int32) Tables.ClassLayout] = md.ClassLayouts.Count;
         tableSizes[(Int32) Tables.FieldLayout] = md.FieldLayouts.Count;
         tableSizes[(Int32) Tables.StandaloneSignature] = md.StandaloneSignatures.Count;
         tableSizes[(Int32) Tables.EventMap] = md.EventMaps.Count;
         tableSizes[(Int32) Tables.Event] = md.EventDefinitions.Count;
         tableSizes[(Int32) Tables.PropertyMap] = md.PropertyMaps.Count;
         tableSizes[(Int32) Tables.Property] = md.PropertyDefinitions.Count;
         tableSizes[(Int32) Tables.MethodSemantics] = md.MethodSemantics.Count;
         tableSizes[(Int32) Tables.MethodImpl] = md.MethodImplementations.Count;
         tableSizes[(Int32) Tables.ModuleRef] = md.ModuleReferences.Count;
         tableSizes[(Int32) Tables.TypeSpec] = md.TypeSpecifications.Count;
         tableSizes[(Int32) Tables.ImplMap] = md.MethodImplementationMaps.Count;
         tableSizes[(Int32) Tables.FieldRVA] = md.FieldRVAs.Count;
         tableSizes[(Int32) Tables.Assembly] = md.AssemblyDefinitions.Count;
         tableSizes[(Int32) Tables.AssemblyRef] = md.AssemblyReferences.Count;
         tableSizes[(Int32) Tables.File] = md.FieldDefinitions.Count;
         tableSizes[(Int32) Tables.ExportedType] = md.ExportedTypess.Count;
         tableSizes[(Int32) Tables.ManifestResource] = md.ManifestResources.Count;
         tableSizes[(Int32) Tables.NestedClass] = md.NestedClassDefinitions.Count;
         tableSizes[(Int32) Tables.GenericParameter] = md.GenericParameterDefinitions.Count;
         tableSizes[(Int32) Tables.MethodSpec] = md.MethodSpecifications.Count;
         tableSizes[(Int32) Tables.GenericParameterConstraint] = md.GenericParameterConstraintDefinitions.Count;

         var tableWidths = new Int32[tableSizes.Length];
         for ( var i = 0; i < tableWidths.Length; ++i )
         {
            if ( tableSizes[i] > 0 )
            {
               tableWidths[i] = MetaDataConstants.CalculateTableWidth( (Tables) i, tableSizes, sysStringIndexSize, guidIndexSize, blobIndexSize );
            }
         }

         var tRefWidths = MetaDataConstants.GetCodedTableIndexSizes( tableSizes );

         var versionStringSize4 = BitUtils.MultipleOf4( versionStringSize );
         var mdHeaderSize = 24 + 4 * (UInt32) tableSizes.Count( size => size > 0 );
         streamHeaders[MD_IDX] = 8 + BitUtils.MultipleOf4( Consts.TABLE_STREAM_NAME.Length );
         var mdStreamSize = mdHeaderSize + tableSizes.Select( ( size, idx ) => (UInt32) size * (UInt32) tableWidths[idx] ).Sum();
         var mdStreamSize4 = BitUtils.MultipleOf4( mdStreamSize );
         streamSizes[MD_IDX] = mdStreamSize4;

         var anArray = new Byte[16 // Header start
            + versionStringSize4 // Version string
            + 4 // Header end
            + streamHeaders.Sum() // Stream headers
            + mdHeaderSize
            ];
         // Metadata root
         var offset = 0;
         anArray.WriteUInt32LEToBytes( ref offset, MD_SIGNATURE )
            .WriteUInt16LEToBytes( ref offset, MD_MAJOR )
            .WriteUInt16LEToBytes( ref offset, MD_MINOR )
            .WriteUInt32LEToBytes( ref offset, MD_RESERVED )
            .WriteInt32LEToBytes( ref offset, versionStringSize4 )
            .WriteStringToBytes( ref offset, MetaDataStringEncoding, metaDataVersion )
            .Skip( ref offset, versionStringSize4 - versionStringSize + 1 )
            .WriteUInt16LEToBytes( ref offset, MD_FLAGS )
            .WriteUInt16LEToBytes( ref offset, (UInt16) streamHeaders.Count( stream => stream > 0 ) )
            // #~ header
            .WriteInt32LEToBytes( ref offset, offset + streamHeaders.Sum() )
            .WriteUInt32LEToBytes( ref offset, mdStreamSize4 )
            .WriteStringToBytes( ref offset, MetaDataStringEncoding, Consts.TABLE_STREAM_NAME )
            .Skip( ref offset, 4 - ( offset % 4 ) );

         if ( hasSysStrings )
         {
            // #String header
            anArray.WriteUInt32LEToBytes( ref offset, (UInt32) offset + (UInt32) streamHeaders.Skip( SYS_STRINGS_IDX ).Sum() + streamSizes.Take( SYS_STRINGS_IDX ).Sum() )
               .WriteUInt32LEToBytes( ref offset, streamSizes[SYS_STRINGS_IDX] )
               .WriteStringToBytes( ref offset, MetaDataStringEncoding, Consts.SYS_STRING_STREAM_NAME )
               .Skip( ref offset, 4 - ( offset % 4 ) );
         }

         if ( hasUserStrings )
         {
            // #US header
            anArray.WriteUInt32LEToBytes( ref offset, (UInt32) offset + (UInt32) streamHeaders.Skip( USER_STRINGS_IDX ).Sum() + streamSizes.Take( USER_STRINGS_IDX ).Sum() )
               .WriteUInt32LEToBytes( ref offset, streamSizes[USER_STRINGS_IDX] )
               .WriteStringToBytes( ref offset, MetaDataStringEncoding, Consts.USER_STRING_STREAM_NAME )
               .Skip( ref offset, 4 - ( offset % 4 ) );
         }

         if ( hasGuids )
         {
            // #Guid header
            anArray.WriteUInt32LEToBytes( ref offset, (UInt32) offset + (UInt32) streamHeaders.Skip( GUID_IDX ).Sum() + streamSizes.Take( GUID_IDX ).Sum() )
               .WriteUInt32LEToBytes( ref offset, streamSizes[GUID_IDX] )
               .WriteStringToBytes( ref offset, MetaDataStringEncoding, Consts.GUID_STREAM_NAME )
               .Skip( ref offset, 4 - ( offset % 4 ) );
         }

         if ( hasBlobs )
         {
            // #Blob header
            anArray.WriteUInt32LEToBytes( ref offset, (UInt32) offset + (UInt32) streamHeaders.Skip( BLOB_IDX ).Sum() + streamSizes.Take( BLOB_IDX ).Sum() )
               .WriteUInt32LEToBytes( ref offset, streamSizes[BLOB_IDX] )
               .WriteStringToBytes( ref offset, MetaDataStringEncoding, Consts.BLOB_STREAM_NAME )
               .Skip( ref offset, 4 - ( offset % 4 ) );
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
            .WriteByteToBytes( ref offset, (Byte) ( Convert.ToInt32( wideSysStringIndex ) | ( Convert.ToInt32( wideGUIDIndex ) << 1 ) | ( Convert.ToInt32( wideBLOBIndex ) << 2 ) ) )
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
         if ( offset != anArray.Length )
         {
            throw new Exception( "Debyyg" );
         }
#endif

         // Write the full CLI header
         sink.Write( anArray );

         // Then, write the binary representation of the tables
         // ECMA-335, p. 239
         ForEachRow( md.ModuleDefinitions, tableWidths, sink, ( array, idx, listIdx, blobIdx, module ) => array
            .WriteInt16LEToBytes( ref idx, 0 ) // Generation
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( module.Name ), wideSysStringIndex ) // Name
            .WriteHeapIndex( ref idx, 1, wideGUIDIndex ) // MvId
            .WriteHeapIndex( ref idx, 0, wideGUIDIndex ) // EncId
            .WriteHeapIndex( ref idx, 0, wideGUIDIndex ) // EncBaseId
            );
         // ECMA-335, p. 247
         // TypeRef may contain types which result in duplicate rows - avoid that
         ForEachRow( this._typeRef, tableWidths, sink, ( array, idx, listIdx, blobIdx, typeRef ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.ResolutionScope, MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.ResolutionScope, typeRef.Item1 ), tRefWidths ) // ResolutionScope
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( typeRef.Item2 ), wideSysStringIndex ) // TypeName
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( typeRef.Item3 ), wideSysStringIndex ) // TypeNamespace
            );
         // ECMA-335, p. 243
         ForEachRow( this._typeDef, tableWidths, sink, ( array, idx, listIdx, blobIdx, typeDef ) => array
            .WriteInt32LEToBytes( ref idx, (Int32) ( (CILType) typeDef ).Attributes ) // Flags
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( typeDef.Name ), wideSysStringIndex ) // TypeName
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( typeDef.Namespace ), wideSysStringIndex ) // TypeNamespace
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeDefOrRef, typeDefExtends[listIdx], tRefWidths ) // Extends
            .WriteSimpleTableIndex( ref idx, Tables.Field, this._typeDefFieldAndMethodIndices[listIdx].Item1, tableSizes ) // FieldList
            .WriteSimpleTableIndex( ref idx, Tables.MethodDef, this._typeDefFieldAndMethodIndices[listIdx].Item2, tableSizes ) // MethodList
            );
         // ECMA-335, p. 223
         ForEachRow( this._fieldDef, tableWidths, sink, ( array, idx, listIdx, blobIdx, fDef ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) fDef.Attributes ) // FieldAttributes
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( fDef.Name ), wideSysStringIndex ) // Name
            .WriteHeapIndex( ref idx, blobIdx, wideBLOBIndex ) // Signature
            );
         // ECMA-335, p. 233
         ForEachRow( this._methodDef, tableWidths, sink, ( array, idx, listIdx, blobIdx, mDef ) => array
            .WriteUInt32LEToBytes( ref idx, methodRVAs.GetOrDefault( mDef, 0u ) ) // RVA
            .WriteInt16LEToBytes( ref idx, (Int16) mDef.ImplementationAttributes ) // ImplFlags
            .WriteInt16LEToBytes( ref idx, (Int16) mDef.Attributes ) // Flags
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( mDef.GetName() ), wideSysStringIndex ) // Name
            .WriteHeapIndex( ref idx, blobIdx, wideBLOBIndex ) // Signature
            .WriteSimpleTableIndex( ref idx, Tables.Parameter, this._methodDefParamIndices[listIdx], tableSizes )
            );
         // ECMA-335, p. 240
         ForEachRow( this._paramDef, tableWidths, sink, ( array, idx, listIdx, blobIdx, pDef ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) pDef.Attributes ) // Flags
            .WriteUInt16LEToBytes( ref idx, (UInt16) ( pDef.Position + 1 ) ) // Sequence
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( pDef.Name ), wideSysStringIndex ) // Name
            );
         // ECMA-335, p. 231
         ForEachElement( Tables.InterfaceImpl, this._interfaceImpl, tableWidths, sink, ( array, idx, listIdx, item ) => array
            .WriteSimpleTableIndex( ref idx, Tables.TypeDef, item.Item1, tableSizes ) // Class
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeDefOrRef, item.Item2, tRefWidths ) // Interface
            );
         // ECMA-335, p. 232
         ForEachRow( this._memberRef, tableWidths, sink, ( array, idx, listIdx, blobIdx, mRef ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MemberRefParent, MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.MemberRefParent, this._memberRef._keys[listIdx].Item1 ), tRefWidths ) // Class
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( mRef is CILElementWithSimpleName ? ( (CILElementWithSimpleName) mRef ).Name : ( (CILConstructor) mRef ).GetName() ), wideSysStringIndex ) // Name
            .WriteHeapIndex( ref idx, blobIdx, wideBLOBIndex ) // Signature
            );
         // ECMA-335, p. 216
         ForEachElement( Tables.Constant, this._constants, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteInt16LEToBytes( ref idx, tuple.Item1 )
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasConstant, tuple.Item2, tRefWidths )
            .WriteHeapIndex( ref idx, tuple.Item3, wideBLOBIndex )
            );
         // ECMA-335, p. 216
         ForEachElement( Tables.CustomAttribute, this._customAttributes, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasCustomAttribute, tuple.Item1, tRefWidths ) // Parent
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.CustomAttributeType, tuple.Item2, tRefWidths ) // Type
            .WriteHeapIndex( ref idx, tuple.Item3, wideBLOBIndex )
            );
         // ECMA-335, p.226
         ForEachElement( Tables.FieldMarshal, this._marshals, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasFieldMarshal, tuple.Item1, tRefWidths ) // Parent
            .WriteHeapIndex( ref idx, tuple.Item2, wideBLOBIndex ) // NativeType
            );
         // ECMA-335, p. 218
         ForEachElement( Tables.DeclSecurity, this._declSecurity, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteUInt16LEToBytes( ref idx, tuple.Item1 ) // Action
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasDeclSecurity, tuple.Item2, tRefWidths ) // Parent
            .WriteHeapIndex( ref idx, tuple.Item3, wideBLOBIndex ) // PermissionSet
            );
         // ECMA-335 p. 215
         ForEachElement( Tables.ClassLayout, this._classLayout, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteInt16LEToBytes( ref idx, tuple.Item1 ) // PackingSize
            .WriteInt32LEToBytes( ref idx, tuple.Item2 ) // ClassSize
            .WriteSimpleTableIndex( ref idx, Tables.TypeDef, tuple.Item3, tableSizes ) // Parent
            );
         // ECMA-335 p. 225
         ForEachElement( Tables.FieldLayout, this._fieldLayout, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteInt32LEToBytes( ref idx, tuple.Item1 ) // Offset
            .WriteSimpleTableIndex( ref idx, Tables.Field, tuple.Item2, tableSizes ) // Field
            );
         // ECMA-335 p. 243
         ForEachRow( this._standaloneSig, tableWidths, sink, ( array, idx, listIdx, blobIdx, sig ) => array
            .WriteHeapIndex( ref idx, blobIdx, wideBLOBIndex ) // Signature
            );
         // ECMA-335 p. 220
         ForEachElement( Tables.EventMap, this._eventMap, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteSimpleTableIndex( ref idx, Tables.TypeDef, tuple.Item1, tableSizes ) // Parent
            .WriteSimpleTableIndex( ref idx, Tables.Event, tuple.Item2, tableSizes ) // EventList
            );
         // ECMA-335 p. 221
         ForEachRow( this._eventDef, tableWidths, sink, ( array, idx, listIdx, blobIdx, evt ) => array
            .WriteUInt16LEToBytes( ref idx, (UInt16) evt.Attributes ) // EventFlags
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( evt.Name ), wideSysStringIndex ) // Name
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeDefOrRef, evtTypes[listIdx], tRefWidths ) // EventType
            );
         // ECMA-335 p. 242
         ForEachElement( Tables.PropertyMap, this._propertyMap, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteSimpleTableIndex( ref idx, Tables.TypeDef, tuple.Item1, tableSizes ) // Parent
            .WriteSimpleTableIndex( ref idx, Tables.Property, tuple.Item2, tableSizes ) // PropertyList
            );
         // ECMA-335 p. 242
         ForEachRow( this._propertyDef, tableWidths, sink, ( array, idx, listIdx, blobIdx, prop ) => array
            .WriteUInt16LEToBytes( ref idx, (UInt16) prop.Attributes ) // Flags
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( prop.Name ), wideSysStringIndex ) // Name
            .WriteHeapIndex( ref idx, blobIdx, wideBLOBIndex ) // Type
            );
         // ECMA-335 p. 237
         ForEachElement( Tables.MethodSemantics, this._methodSemantics, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteUInt16LEToBytes( ref idx, tuple.Item1 ) // Semantics
            .WriteSimpleTableIndex( ref idx, Tables.MethodDef, tuple.Item2, tableSizes ) // Method
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasSemantics, tuple.Item3, tRefWidths ) // Association
            );
         // ECMA-335 p. 237
         ForEachElement( Tables.MethodImpl, this._methodImpl, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteSimpleTableIndex( ref idx, Tables.TypeDef, tuple.Item1, tableSizes ) // Class
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MethodDefOrRef, tuple.Item2, tRefWidths ) // MethodBody
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MethodDefOrRef, tuple.Item3, tRefWidths ) // MethodDeclaration
            );
         // ECMA-335, p. 239
         ForEachElement( Tables.ModuleRef, this._moduleRef._list, tableWidths, sink, ( array, idx, listIdx, mRef ) => array
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( mRef ), wideSysStringIndex ) // Name
            );
         // ECMA-335, p. 248
         ForEachRow( this._typeSpec, tableWidths, sink, ( array, idx, listIdx, blobIdx, tSpec ) => array
            .WriteHeapIndex( ref idx, blobIdx, wideBLOBIndex ) // Signature
            );
         // ECMA-335, p. 230
         ForEachElement( Tables.ImplMap, this._implMap, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteUInt16LEToBytes( ref idx, (UInt16) tuple.Item1 ) // PInvokeAttributes
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MemberForwarded, tuple.Item2, tRefWidths ) // MemberForwarded
            .WriteHeapIndex( ref idx, tuple.Item3, wideSysStringIndex ) // Import name
            .WriteSimpleTableIndex( ref idx, Tables.ModuleRef, tuple.Item4, tableSizes ) // Import scope
            );
         // ECMA-335, p. 227
         ForEachElement( Tables.FieldRVA, this._fieldsWithRVA, tableWidths, sink, ( array, idx, listIdx, item ) => array
            .WriteUInt32LEToBytes( ref idx, fRVAs[listIdx] )
            .WriteSimpleTableIndex( ref idx, Tables.Field, this._fieldsWithRVA[listIdx], tableSizes )
            );
         // ECMA-335, p. 211
         ForEachRow( this._assembly, tableWidths, sink, ( array, idx, listIdx, blobIdx, ass ) => array
            .WriteInt32LEToBytes( ref idx, (Int32) ass.HashAlgorithm ) // HashAlgId
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.MajorVersion ) // MajorVersion
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.MinorVersion ) // MinorVersion
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.BuildNumber ) // BuildNumber
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.Revision ) // RevisionNumber
            .WriteInt32LEToBytes( ref idx, (Int32) ( ass.Flags & ~AssemblyFlags.PublicKey ) ) // Flags (Don't set public key flag)
            .WriteHeapIndex( ref idx, this._blob.GetBLOB( this._assemblyName.PublicKey ), wideBLOBIndex ) // PublicKey
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( ass.Name ), wideSysStringIndex ) // Name
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( ass.Culture ), wideSysStringIndex ) // Culture
            );
         // ECMA-335, p. 212
         ForEachElement( Tables.AssemblyRef, this._assemblyRef, tableWidths, sink, ( array, idx, listIdx, ass ) => array
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.MajorVersion ) // MajorVersion
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.MinorVersion ) // MinorVersion
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.BuildNumber ) // BuildNumber
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.Revision ) // RevisionNumber
            .WriteInt32LEToBytes( ref idx, (Int32) ass.Flags ) // Flags
            .WriteHeapIndex( ref idx, this._blob.GetBLOB( ass.PublicKey.IsNullOrEmpty() ? null : ass.PublicKey ), wideBLOBIndex ) // PublicKey
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( ass.Name ), wideSysStringIndex ) // Name
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( ass.Culture ), wideSysStringIndex ) // Culture
            .WriteHeapIndex( ref idx, this._aRefHashes[listIdx], wideBLOBIndex ) // HashValue
            );
         ForEachElement( Tables.File, this._files.Values.OrderBy( t => t.Item1 ).ToList(), tableWidths, sink, ( array, idx, listIdx, file ) => array
            .WriteUInt32LEToBytes( ref idx, (UInt32) file.Item2 )
            .WriteHeapIndex( ref idx, file.Item3, wideSysStringIndex )
            .WriteHeapIndex( ref idx, file.Item4, wideBLOBIndex )
            );
         ForEachElement( Tables.ExportedType, this._exportedTypes, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteUInt32LEToBytes( ref idx, (UInt32) tuple.Item1 ) // TypeAttributes
            .WriteInt32LEToBytes( ref idx, tuple.Item2 ) // TypeDef index in other (!) assembly
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( tuple.Item3 ), wideSysStringIndex ) // TypeName
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( tuple.Item4 ), wideSysStringIndex ) // TypeNamespace
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.Implementation, tuple.Item5, tRefWidths ) // Implementation
            );
         ForEachElement( Tables.ManifestResource, this._module.ManifestResources, tableWidths, sink, ( array, idx, listIdx, kvp ) =>
         {
            UInt32 mOffset;
            Int32 impl = 0;
            var add = embeddedManifestResourceOffsets.TryGetValue( kvp.Key, out mOffset );
            if ( !add )
            {
               add = moduleResRefIndices.ContainsKey( kvp.Key );
               if ( add )
               {
                  add = true;
                  mOffset = 0u;
                  impl = MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.Implementation, moduleResRefIndices[kvp.Key] );
               }
            }

            if ( add )
            {
               array
                  .WriteUInt32LEToBytes( ref idx, mOffset ) // Offset
                  .WriteUInt32LEToBytes( ref idx, (UInt32) kvp.Value.Attributes ) // Flags
                  .WriteHeapIndex( ref idx, this._sysStrings.GetString( kvp.Key ), wideSysStringIndex ) // Name
                  .WriteCodedTableIndex( ref idx, CodedTableIndexKind.Implementation, impl, tRefWidths ); // Implementation
            }
         } );
         // ECMA-335, p. 240
         ForEachElement( Tables.NestedClass, this._nestedClass, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteSimpleTableIndex( ref idx, Tables.TypeDef, tuple.Item1, tableSizes )
            .WriteSimpleTableIndex( ref idx, Tables.TypeDef, tuple.Item2, tableSizes )
            );
         // ECMA-335, p. 228
         ForEachRow( this._gParamDef, tableWidths, sink, ( array, idx, listIdx, blobIdx, gParam ) => array
            .WriteUInt16LEToBytes( ref idx, (UInt16) gParam.GenericParameterPosition ) // Number
            .WriteUInt16LEToBytes( ref idx, (UInt16) gParam.Attributes ) // Flags
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeOrMethodDef, this._gParamOwners[listIdx], tRefWidths ) // Owner
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( gParam.Name ), wideSysStringIndex ) // Name
            );
         // ECMA-335, p. 238
         ForEachRow( this._methodSpec, tableWidths, sink, ( array, idx, listIdx, blobIdx, mSpec ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MethodDefOrRef, MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.MethodDefOrRef, this._methodSpec._keys[listIdx] ), tRefWidths ) // Method
            .WriteHeapIndex( ref idx, blobIdx, wideBLOBIndex ) // Instantiation
            );
         // ECMA-335, p. 229
         ForEachElement( Tables.GenericParameterConstraint, this._gParamConstraints, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteSimpleTableIndex( ref idx, Tables.GenericParameter, tuple.Item1, tableSizes ) // Owner
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeDefOrRef, tuple.Item2, tRefWidths ) // Constraint
            );
         // Padding
         for ( var i = mdStreamSize; i < mdStreamSize4; i++ )
         {
            sink.WriteByte( 0 );
         }

         // #String
         sysStrings.WriteStrings( sink );

         // #US
         userStrings.WriteStrings( sink );

         // #Guid
         foreach ( var guid in guids )
         {
            sink.Write( guid.ToByteArray() );
         }

         // #Blob
         blob.WriteBLOBs( sink );

         var total = (UInt32) anArray.Length + streamSizes.Sum() - mdHeaderSize;

         return total;
      }

      private static void ForEachElement<T>( Tables table, ICollection<T> list, Int32[] tableWidths, Stream sink, Action<Byte[], Int32, Int32, T> writeAction )
      {
         var count = list.Count;
         if ( count > 0 )
         {

            var width = tableWidths[(Int32) table];
            var array = new Byte[width * count];
            var idx = 0;
            var listIdx = 0;
            foreach ( var item in list )
            {
               writeAction( array, idx, listIdx, item );
               idx += width;
               ++listIdx;
            }
            sink.Write( array );
#if DEBUG
            if ( idx != array.Length )
            {
               throw new Exception( "Something went wrong when emitting metadata array: emitted " + idx + " instead of expected " + array.Length + " bytes." );
            }
#endif
         }
      }

      private static void CreateMDHeaps(
         CILMetaData md,
         out BLOBContainer blobsParam,
         out SystemStringHeapEmittingInfo sysStringsParam,
         out IList<Guid> guidsParam,
         out Object[] heapInfos
         )
      {
         var blobs = new BLOBContainer();
         var sysStrings = new SystemStringHeapEmittingInfo();
         var guids = new List<Guid>();
         heapInfos = new Object[Consts.AMOUNT_OF_TABLES];

         var blobHelper = new BinaryHeapEmittingInfo();
         var auxHelper = new BinaryHeapEmittingInfo(); // For writing security BLOBs
         // 0x00 Module
         ProcessTableForHeaps4( Tables.Module, md.ModuleDefinitions, heapInfos, mod => new HeapInfo4( sysStrings.GetOrAddString( mod.Name ), GetGUIDIndex( guids, mod.ModuleGUID ), GetGUIDIndex( guids, mod.EditAndContinueGUID ), GetGUIDIndex( guids, mod.EditAndContinueBaseGUID ) ) );
         // 0x01 TypeRef
         ProcessTableForHeaps2( Tables.TypeRef, md.TypeReferences, heapInfos, tr => new HeapInfo2( sysStrings.GetOrAddString( tr.Name ), sysStrings.GetOrAddString( tr.Namespace ) ) );
         // 0x02 TypeDef
         ProcessTableForHeaps2( Tables.TypeDef, md.TypeDefinitions, heapInfos, td => new HeapInfo2( sysStrings.GetOrAddString( td.Name ), sysStrings.GetOrAddString( td.Namespace ) ) );
         // 0x04 FieldDef
         ProcessTableForHeaps2( Tables.Field, md.FieldDefinitions, heapInfos, f => new HeapInfo2( sysStrings.GetOrAddString( f.Name ), blobs.GetBLOB( blobHelper.CreateFieldSignature( f.Signature ) ) ) );
         // 0x06 MethodDef
         ProcessTableForHeaps2( Tables.MethodDef, md.MethodDefinitions, heapInfos, m => new HeapInfo2( sysStrings.GetOrAddString( m.Name ), blobs.GetBLOB( blobHelper.CreateMethodSignature( m.Signature ) ) ) );
         // 0x08 Parameter
         ProcessTableForHeaps1( Tables.Parameter, md.ParameterDefinitions, heapInfos, p => new HeapInfo1( sysStrings.GetOrAddString( p.Name ) ) );
         // 0x0A MemberRef
         ProcessTableForHeaps2( Tables.MemberRef, md.MemberReferences, heapInfos, m => new HeapInfo2( sysStrings.GetOrAddString( m.Name ), blobs.GetBLOB( blobHelper.CreateMemberRefSignature( m.Signature ) ) ) );
         // 0x0B Constant
         ProcessTableForHeaps1( Tables.Constant, md.ConstantDefinitions, heapInfos, c => new HeapInfo1( blobs.GetBLOB( blobHelper.CreateConstantBytes( c.Value ) ) ) );
         // 0x0C CustomAttribute
         ProcessTableForHeaps1( Tables.CustomAttribute, md.CustomAttributeDefinitions, heapInfos, ca => new HeapInfo1( blobs.GetBLOB( blobHelper.CreateCustomAttributeSignature( ca.Signature ) ) ) );
         // 0x0D FieldMarshal
         ProcessTableForHeaps1( Tables.FieldMarshal, md.FieldMarshals, heapInfos, fm => new HeapInfo1( blobs.GetBLOB( blobHelper.CreateMarshalSpec( fm.NativeType ) ) ) );
         // 0x0E Security definitions
         ProcessTableForHeaps1( Tables.DeclSecurity, md.SecurityDefinitions, heapInfos, sd => new HeapInfo1( blobs.GetBLOB( blobHelper.CreateSecuritySignature( sd, auxHelper ) ) ) );
         // 0x11 Standalone sig
         ProcessTableForHeaps1( Tables.StandaloneSignature, md.StandaloneSignatures, heapInfos, s => new HeapInfo1( blobs.GetBLOB( blobHelper.CreateStandaloneSignature( s.Signature ) ) ) );
         // 0x14 Event
         ProcessTableForHeaps1( Tables.Event, md.EventDefinitions, heapInfos, e => new HeapInfo1( sysStrings.GetOrAddString( e.Name ) ) );
         // 0x17 Property
         ProcessTableForHeaps2( Tables.Property, md.PropertyDefinitions, heapInfos, p => new HeapInfo2( sysStrings.GetOrAddString( p.Name ), blobs.GetBLOB( blobHelper.CreatePropertySignature( p.Signature ) ) ) );
         // 0x1A ModuleRef
         ProcessTableForHeaps1( Tables.ModuleRef, md.ModuleReferences, heapInfos, mr => new HeapInfo1( sysStrings.GetOrAddString( mr.ModuleReference ) ) );
         // 0x1B TypeSpec
         ProcessTableForHeaps1( Tables.TypeSpec, md.TypeSpecifications, heapInfos, t => new HeapInfo1( blobs.GetBLOB( blobHelper.CreateTypeSignature( t.Signature ) ) ) );
         // 0x1C ImplMap
         ProcessTableForHeaps1( Tables.ImplMap, md.MethodImplementationMaps, heapInfos, mim => new HeapInfo1( sysStrings.GetOrAddString( mim.ImportName ) ) );
         // 0x20 Assembly
         ProcessTableForHeaps3( Tables.Assembly, md.AssemblyDefinitions, heapInfos, ad => new HeapInfo3( blobs.GetBLOB( ad.AssemblyInformation.PublicKeyOrToken ), sysStrings.GetOrAddString( ad.AssemblyInformation.Name ), sysStrings.GetOrAddString( ad.AssemblyInformation.Culture ) ) );
         // 0x21 AssemblyRef
         ProcessTableForHeaps4( Tables.AssemblyRef, md.AssemblyReferences, heapInfos, ar => new HeapInfo4( blobs.GetBLOB( ar.AssemblyInformation.PublicKeyOrToken ), sysStrings.GetOrAddString( ar.AssemblyInformation.Name ), sysStrings.GetOrAddString( ar.AssemblyInformation.Culture ), blobs.GetBLOB( ar.HashValue ) ) );
         // 0x26 File
         ProcessTableForHeaps2( Tables.File, md.FileReferences, heapInfos, f => new HeapInfo2( sysStrings.GetOrAddString( f.Name ), blobs.GetBLOB( f.HashValue ) ) );
         // 0x27 ExportedType
         ProcessTableForHeaps2( Tables.ExportedType, md.ExportedTypess, heapInfos, e => new HeapInfo2( sysStrings.GetOrAddString( e.Name ), sysStrings.GetOrAddString( e.Namespace ) ) );
         // 0x28 ManifestResource
         ProcessTableForHeaps1( Tables.ManifestResource, md.ManifestResources, heapInfos, m => new HeapInfo1( sysStrings.GetOrAddString( m.Name ) ) );
         // 0x2A GenericParameter
         ProcessTableForHeaps1( Tables.GenericParameter, md.GenericParameterDefinitions, heapInfos, g => new HeapInfo1( sysStrings.GetOrAddString( g.Name ) ) );
         // 0x2B MethosSpec
         ProcessTableForHeaps1( Tables.MethodSpec, md.MethodSpecifications, heapInfos, m => new HeapInfo1( blobs.GetBLOB( blobHelper.CreateMethodSpecSignature( m.Signature ) ) ) );

         // We're done
         blobsParam = blobs;
         sysStringsParam = sysStrings;
         guidsParam = guids;
      }

      private static void ProcessTableForHeaps1<T>( Tables tableEnum, List<T> table, Object[] heapInfos, Func<T, HeapInfo1> heapInfoExtractor )
      {
         var heapInfoList = new List<HeapInfo1>( table.Count );
         foreach ( var row in table )
         {
            heapInfoList.Add( heapInfoExtractor( row ) );
         }
         heapInfos[(Int32) tableEnum] = heapInfoList;
      }

      private static void ProcessTableForHeaps2<T>( Tables tableEnum, List<T> table, Object[] heapInfos, Func<T, HeapInfo2> heapInfoExtractor )
      {
         var heapInfoList = new List<HeapInfo2>( table.Count );
         foreach ( var row in table )
         {
            heapInfoList.Add( heapInfoExtractor( row ) );
         }
         heapInfos[(Int32) tableEnum] = heapInfoList;
      }

      private static void ProcessTableForHeaps3<T>( Tables tableEnum, List<T> table, Object[] heapInfos, Func<T, HeapInfo3> heapInfoExtractor )
      {
         var heapInfoList = new List<HeapInfo3>( table.Count );
         foreach ( var row in table )
         {
            heapInfoList.Add( heapInfoExtractor( row ) );
         }
         heapInfos[(Int32) tableEnum] = heapInfoList;
      }

      private static void ProcessTableForHeaps4<T>( Tables tableEnum, List<T> table, Object[] heapInfos, Func<T, HeapInfo4> heapInfoExtractor )
      {
         var heapInfoList = new List<HeapInfo4>( table.Count );
         foreach ( var row in table )
         {
            heapInfoList.Add( heapInfoExtractor( row ) );
         }
         heapInfos[(Int32) tableEnum] = heapInfoList;
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

      private static UInt32 GetGUIDIndex( IList<Guid> guids, Guid? guid )
      {
         UInt32 retVal;
         if ( guid.HasValue )
         {
            var idx = guids.IndexOf( guid.Value );
            if ( idx < 0 )
            {
               idx = guids.Count;
               guids.Add( guid.Value );
            }
            retVal = (UInt32) idx;
         }
         else
         {
            retVal = 0u;
         }

         return retVal;
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

   internal sealed class MethodILWriter
   {
      private const UInt32 USER_STRING_MASK = 0x70 << 24;
      private const Int32 MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION = 20;
      private const Int32 METHOD_DATA_SECTION_SIZE = 4;

      private readonly CILMetaData _md;
      private readonly Int32 _methodDefIndex;
      private readonly MethodDefinition _method;
      private readonly MethodILDefinition _methodIL;
      private readonly UserStringHeapEmittingInfo _usersStringHeap;
      private readonly Byte[] _ilCode;
      private readonly Int32[] _stackSizes;
      private Int32 _ilCodeCount;
      private Int32 _currentStack;
      private Int32 _maxStack;

      internal MethodILWriter( CILMetaData md, Int32 idx, UserStringHeapEmittingInfo usersStringHeap )
      {
         this._md = md;
         this._methodDefIndex = idx;
         this._usersStringHeap = usersStringHeap;

         this._method = md.MethodDefinitions[idx];
         this._methodIL = this._method.IL;
         this._ilCode = new Byte[this._methodIL.OpCodes.Sum( oci =>
         {
            var oc = oci.OpCode;
            var ocSize = oc.Size + oc.OperandSize;
            if ( oci.InfoKind == OpCodeOperandKind.OperandSwitch )
            {
               ocSize += ( (OpCodeInfoWithSwitch) oci ).Offsets.Count * sizeof( Int32 );
            }
            return ocSize;
         } )];
         this._stackSizes = new Int32[this._ilCode.Length];
         this._ilCodeCount = 0;
         this._currentStack = 0;
         this._maxStack = 0;
      }

      private void Emit( OpCodeInfo codeInfo )
      {
         var code = codeInfo.OpCode;

         if ( code.Size > 1 )
         {
            this._ilCode.WriteByteToBytes( ref this._ilCodeCount, code.Byte1 );
         }
         this._ilCode.WriteByteToBytes( ref this._ilCodeCount, code.Byte2 );

         var operandType = code.OperandType;
         if ( operandType != OperandType.InlineNone )
         {
            Object methodOrLabelOrManyLabels = null;
            Int32 i32;
            switch ( operandType )
            {
               case OperandType.ShortInlineI:
               case OperandType.ShortInlineVar:
                  this._ilCode.WriteByteToBytes( ref this._ilCodeCount, (Byte) ( (OpCodeInfoWithInt32) codeInfo ).Operand );
                  break;
               case OperandType.ShortInlineBrTarget:
                  i32 = ( (OpCodeInfoWithInt32) codeInfo ).Operand;
                  methodOrLabelOrManyLabels = i32;
                  this._ilCode.WriteByteToBytes( ref this._ilCodeCount, (Byte) i32 );
                  break;
               case OperandType.ShortInlineR:
                  this._ilCode.WriteSingleLEToBytes( ref this._ilCodeCount, (Single) ( (OpCodeInfoWithSingle) codeInfo ).Operand );
                  break;
               case OperandType.InlineBrTarget:
                  i32 = ( (OpCodeInfoWithInt32) codeInfo ).Operand;
                  methodOrLabelOrManyLabels = i32;
                  this._ilCode.WriteInt32LEToBytes( ref this._ilCodeCount, i32 );
                  break;
               case OperandType.InlineI:
                  this._ilCode.WriteInt32LEToBytes( ref this._ilCodeCount, ( (OpCodeInfoWithInt32) codeInfo ).Operand );
                  break;
               case OperandType.InlineVar:
                  this._ilCode.WriteInt16LEToBytes( ref this._ilCodeCount, (Int16) ( (OpCodeInfoWithInt32) codeInfo ).Operand );
                  break;
               case OperandType.InlineR:
                  this._ilCode.WriteDoubleLEToBytes( ref this._ilCodeCount, (Double) ( (OpCodeInfoWithDouble) codeInfo ).Operand );
                  break;
               case OperandType.InlineI8:
                  this._ilCode.WriteInt64LEToBytes( ref this._ilCodeCount, (Int64) ( (OpCodeInfoWithInt64) codeInfo ).Operand );
                  break;
               case OperandType.InlineString:
                  this._ilCode.WriteInt32LEToBytes( ref this._ilCodeCount, (Int32) ( this._usersStringHeap.GetOrAddString( ( (OpCodeInfoWithString) codeInfo ).Operand ) | USER_STRING_MASK ) );
                  break;
               case OperandType.InlineField:
               case OperandType.InlineMethod:
               case OperandType.InlineType:
               case OperandType.InlineTok:
               case OperandType.InlineSig:
                  var tIdx = ( (OpCodeInfoWithToken) codeInfo ).Operand;
                  if ( operandType != OperandType.InlineField )
                  {
                     methodOrLabelOrManyLabels = tIdx;
                  }
                  this._ilCode.WriteInt32LEToBytes( ref this._ilCodeCount, tIdx.CreateTokenForEmittingMandatoryTableIndex() );
                  break;
               case OperandType.InlineSwitch:
                  var offsets = ( (OpCodeInfoWithSwitch) codeInfo ).Offsets;
                  this._ilCode.WriteInt32LEToBytes( ref this._ilCodeCount, offsets.Count );
                  foreach ( var offset in offsets )
                  {
                     this._ilCode.WriteInt32LEToBytes( ref this._ilCodeCount, offset );
                  }
                  methodOrLabelOrManyLabels = offsets;
                  break;
               default:
                  throw new ArgumentException( "Unknown operand type: " + code.OperandType + " for " + code + "." );
            }

            this.UpdateStackSize( code, methodOrLabelOrManyLabels );
         }
      }

      internal Byte[] PerformEmitting( UInt32 currentOffset, out Boolean isTiny )
      {
         // Remember that inner exception blocks must precede outer ones
         var allExceptionBlocksCorrectlyOrdered = this._methodIL.ExceptionBlocks.ToArray();
         Array.Sort(
            allExceptionBlocksCorrectlyOrdered,
            ( item1, item2 ) =>
            {
               // Return -1 if item1 is inner block of item2, 0 if they are same, 1 if item1 is not inner block of item2
               return Object.ReferenceEquals( item1, item2 ) ? 0 :
                  ( item1.TryOffset >= item2.HandlerOffset + item2.HandlerLength
                     || ( item1.TryOffset <= item2.TryOffset && item1.HandlerOffset + item1.HandlerLength > item2.HandlerOffset + item2.HandlerLength ) ?
                  1 :
                  -1
                  );
            } );

         // Setup stack sizes based on exception blocks
         foreach ( var block in allExceptionBlocksCorrectlyOrdered )
         {
            switch ( block.BlockType )
            {
               case ExceptionBlockType.Exception:
                  this._stackSizes[block.HandlerOffset] = 1;
                  break;
               case ExceptionBlockType.Filter:
                  this._stackSizes[block.HandlerOffset] = 1;
                  this._stackSizes[block.FilterOffset] = 1;
                  break;
            }
         }

         // Emit opcodes and operands
         foreach ( var info in this._methodIL.OpCodes )
         {
            this.Emit( info );
         }

         // Create exception blocks with byte offsets
         var exceptionBlocks = new Byte[allExceptionBlocksCorrectlyOrdered.Length][];
         var exceptionFormats = new Boolean[exceptionBlocks.Length];

         // TODO PEVerify doesn't like mixed small and fat blocks at all (however, at least Cecil understands that kind of situation)
         // TODO Apparently, PEVerify doesn't like multiple small blocks either (Cecil still loads code fine)
         // Also, because of exception block ordering, it is easier to do this way.
         var allAreSmall = allExceptionBlocksCorrectlyOrdered.Length <= MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION
            && allExceptionBlocksCorrectlyOrdered.All( excBlock =>
            {
               return excBlock.TryLength <= Byte.MaxValue
                  && excBlock.HandlerLength <= Byte.MaxValue
                  && excBlock.TryOffset <= UInt16.MaxValue
                  && excBlock.HandlerOffset <= UInt16.MaxValue;
            } );

         for ( var i = 0; i < exceptionBlocks.Length; ++i )
         {
            // ECMA-335, pp. 286-287
            var block = allExceptionBlocksCorrectlyOrdered[i];
            Int32 idx = 0;
            Byte[] array;
            var tryOffset = block.TryOffset;
            var tryLength = block.TryLength;
            var handlerOffset = block.HandlerOffset;
            var handlerLength = block.HandlerLength;
            var useSmallFormat = allAreSmall &&
               tryLength <= Byte.MaxValue && handlerLength <= Byte.MaxValue && tryOffset <= UInt16.MaxValue && handlerOffset <= UInt16.MaxValue;
            exceptionFormats[i] = useSmallFormat;
            if ( useSmallFormat )
            {
               array = new Byte[12];
               array.WriteInt16LEToBytes( ref idx, (Int16) block.BlockType )
                  .WriteUInt16LEToBytes( ref idx, (UInt16) tryOffset )
                  .WriteByteToBytes( ref idx, (Byte) tryLength )
                  .WriteUInt16LEToBytes( ref idx, (UInt16) handlerOffset )
                  .WriteByteToBytes( ref idx, (Byte) handlerLength );
            }
            else
            {
               array = new Byte[24];
               array.WriteInt32LEToBytes( ref idx, (Int32) block.BlockType )
                  .WriteInt32LEToBytes( ref idx, tryOffset )
                  .WriteInt32LEToBytes( ref idx, tryLength )
                  .WriteInt32LEToBytes( ref idx, handlerOffset )
                  .WriteInt32LEToBytes( ref idx, handlerLength );
            }

            if ( ExceptionBlockType.Exception == block.BlockType )
            {
               array.WriteInt32LEToBytes( ref idx, block.ExceptionType.CreateTokenForEmittingOptionalTableIndex() );
            }
            else if ( ExceptionBlockType.Filter == block.BlockType )
            {
               array.WriteInt32LEToBytes( ref idx, block.FilterOffset );
            }
            exceptionBlocks[i] = array;
         }

         // Write method header, extra data sections, and IL
         Byte[] result;
         var localSig = this._md.GetLocalsSignatureForMethodOrNull( this._methodDefIndex );
         isTiny = this._ilCodeCount < 64
            && exceptionBlocks.Length == 0
            && this._maxStack <= 8
            && ( localSig == null || localSig.Locals.Count == 0 );
         var resultIndex = 0;
         var hasAnyExc = false;
         var hasSmallExc = false;
         var hasLargExc = false;
         var smallExcCount = 0;
         var largeExcCount = 0;
         var amountToNext4ByteBoundary = 0;
         if ( isTiny )
         {
            // Can use tiny header
            result = new Byte[this._ilCodeCount + 1];
            result[resultIndex++] = (Byte) ( (Int32) MethodHeaderFlags.TinyFormat | ( this._ilCodeCount << 2 ) );
         }
         else
         {
            // Use fat header
            hasAnyExc = exceptionBlocks.Length > 0;
            hasSmallExc = hasAnyExc && exceptionFormats.Any( excFormat => excFormat );
            hasLargExc = hasAnyExc && exceptionFormats.Any( excFormat => !excFormat );
            smallExcCount = hasSmallExc ? exceptionFormats.Count( excFormat => excFormat ) : 0;
            largeExcCount = hasLargExc ? exceptionFormats.Count( excFormat => !excFormat ) : 0;
            var offsetAfterIL = (Int32) ( BitUtils.MultipleOf4( currentOffset ) + 12 + (UInt32) this._ilCodeCount );
            amountToNext4ByteBoundary = BitUtils.MultipleOf4( offsetAfterIL ) - offsetAfterIL;

            result = new Byte[12
               + this._ilCodeCount +
               ( hasAnyExc ? amountToNext4ByteBoundary : 0 ) +
               ( hasSmallExc ? METHOD_DATA_SECTION_SIZE : 0 ) +
               ( hasLargExc ? METHOD_DATA_SECTION_SIZE : 0 ) +
               smallExcCount * 12 +
               ( smallExcCount / MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION ) * METHOD_DATA_SECTION_SIZE + // (Amount of extra section headers ) * section size
               largeExcCount * 24
               ];
            var flags = MethodHeaderFlags.FatFormat;
            if ( hasAnyExc )
            {
               flags |= MethodHeaderFlags.MoreSections;
            }
            if ( this._methodIL.InitLocals )
            {
               flags |= MethodHeaderFlags.InitLocals;
            }

            result.WriteInt16LEToBytes( ref resultIndex, (Int16) ( ( (Int32) flags ) | ( 3 << 12 ) ) )
               .WriteInt16LEToBytes( ref resultIndex, (Int16) this._maxStack )
               .WriteInt32LEToBytes( ref resultIndex, this._ilCodeCount )
               .WriteInt32LEToBytes( ref resultIndex, this._methodIL.LocalsSignatureIndex.CreateTokenForEmittingOptionalTableIndex() );
         }

         Array.Copy( this._ilCode, 0, result, resultIndex, this._ilCodeCount );
         resultIndex += this._ilCodeCount;

         if ( hasAnyExc )
         {
            var processedIndices = new HashSet<Int32>();
            resultIndex += amountToNext4ByteBoundary;
            var flags = MethodDataFlags.ExceptionHandling;
            // First, write fat sections
            if ( hasLargExc )
            {
               // TODO like with small sections, what if too many exception clauses to be fit into DataSize?
               flags |= MethodDataFlags.FatFormat;
               if ( hasSmallExc )
               {
                  flags |= MethodDataFlags.MoreSections;
               }
               result.WriteByteToBytes( ref resultIndex, (Byte) flags )
                  .WriteInt32LEToBytes( ref resultIndex, largeExcCount * 24 + METHOD_DATA_SECTION_SIZE );
               --resultIndex;
               for ( var i = 0; i < exceptionBlocks.Length; ++i )
               {
                  if ( !exceptionFormats[i] && processedIndices.Add( i ) )
                  {
                     var length = exceptionBlocks[i].Length;
                     Array.Copy( exceptionBlocks[i], 0, result, resultIndex, length );
                     resultIndex += length;
                  }
               }
            }
            // Then, write small sections
            // If exception counts * 12 + 4 are > Byte.MaxValue, have to write several sections
            // (Max 20 handlers per section)
            flags = MethodDataFlags.ExceptionHandling;
            if ( hasSmallExc )
            {
               var curSmallIdx = 0;
               while ( smallExcCount > 0 )
               {
                  var amountToBeWritten = Math.Min( smallExcCount, MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION );
                  if ( amountToBeWritten < smallExcCount )
                  {
                     flags |= MethodDataFlags.MoreSections;
                  }
                  else
                  {
                     flags = flags & ~( MethodDataFlags.MoreSections );
                  }

                  result.WriteByteToBytes( ref resultIndex, (Byte) flags )
                     .WriteByteToBytes( ref resultIndex, (Byte) ( amountToBeWritten * 12 + METHOD_DATA_SECTION_SIZE ) )
                     .WriteInt16LEToBytes( ref resultIndex, 0 );
                  var amountActuallyWritten = 0;
                  while ( curSmallIdx < exceptionBlocks.Length && amountActuallyWritten < amountToBeWritten )
                  {
                     if ( exceptionFormats[curSmallIdx] )
                     {
                        var length = exceptionBlocks[curSmallIdx].Length;
                        Array.Copy( exceptionBlocks[curSmallIdx], 0, result, resultIndex, length );
                        resultIndex += length;
                        ++amountActuallyWritten;
                     }
                     ++curSmallIdx;
                  }
                  smallExcCount -= amountToBeWritten;
               }
            }

         }
#if DEBUG
         if ( resultIndex != result.Length )
         {
            throw new Exception( "Something went wrong when emitting method headers and body. Emitted " + resultIndex + " bytes, but was supposed to emit " + result.Length + " bytes." );
         }
#endif
         return result;
      }

      private void UpdateStackSize( OpCode code, Object methodOrLabelOrManyLabels )
      {
         var curStacksize = Math.Max( this._currentStack, this._stackSizes[this._ilCodeCount] );
         if ( FlowControl.Call == code.FlowControl )
         {
            this.UpdateStackSizeForMethod( code, (TableIndex) methodOrLabelOrManyLabels, ref curStacksize );
         }
         else
         {
            curStacksize += code.StackChange;
         }

         // Save max stack here
         this._maxStack = Math.Max( curStacksize, this._maxStack );

         // Copy branch stack size
         if ( curStacksize > 0 )
         {
            switch ( code.OperandType )
            {
               case OperandType.InlineBrTarget:
                  this.UpdateStackSizeAtBranchTarget( (Int32) methodOrLabelOrManyLabels, curStacksize );
                  break;
               case OperandType.ShortInlineBrTarget:
                  this.UpdateStackSizeAtBranchTarget( (Int32) methodOrLabelOrManyLabels, curStacksize );
                  break;
               case OperandType.InlineSwitch:
                  var labels = (Int32[]) methodOrLabelOrManyLabels;
                  for ( var i = 0; i < labels.Length; ++i )
                  {
                     this.UpdateStackSizeAtBranchTarget( labels[i], curStacksize );
                  }
                  break;
            }
         }

         // Set stack to zero if required
         if ( code.UnconditionallyEndsBulkOfCode )
         {
            curStacksize = 0;
         }

         // Save current size for next iteration
         this._currentStack = curStacksize;
      }

      private void UpdateStackSizeForMethod( OpCode code, TableIndex method, ref Int32 curStacksize )
      {
         var sig = this.ResolveSignatureFromTableIndex( method );

         if ( sig != null )
         {
            if ( sig.SignatureStarter.IsHasThis() && OpCodes.Newobj != code )
            {
               // Pop 'this'
               --curStacksize;
            }

            // Pop parameters
            curStacksize -= sig.Parameters.Count;
            var refSig = sig as MethodReferenceSignature;
            if ( refSig != null )
            {
               curStacksize -= refSig.VarArgsParameters.Count;
            }

            if ( OpCodes.Calli == code )
            {
               // Pop function pointer
               --curStacksize;
            }

            var rType = sig.ReturnType.Type;

            // TODO we could check here for stack underflow!

            if ( OpCodes.Newobj == code
               || rType.TypeSignatureKind != TypeSignatureKind.Simple
               || ( (SimpleTypeSignature) rType ).SimpleType != SignatureElementTypes.Void
               )
            {
               // Push return value
               ++curStacksize;
            }
         }
      }

      private AbstractMethodSignature ResolveSignatureFromTableIndex( TableIndex method )
      {
         var mIdx = method.Index;
         switch ( method.Table )
         {
            case Tables.MethodDef:
               return this._md.MethodDefinitions[mIdx].Signature;
            case Tables.MemberRef:
               return this._md.MemberReferences[mIdx].Signature as AbstractMethodSignature;
            case Tables.StandaloneSignature:
               return this._md.StandaloneSignatures[mIdx].Signature as AbstractMethodSignature;
            case Tables.MethodSpec:
               return this.ResolveSignatureFromTableIndex( this._md.MethodSpecifications[mIdx].Method );
            default:
               return null;
         }
      }

      private void UpdateStackSizeAtBranchTarget( Int32 jump, Int32 stackSize )
      {
         var idx = this._ilCodeCount + jump;
         this._stackSizes[idx] = Math.Max( this._stackSizes[idx], stackSize );
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

   internal abstract class StringHeapEmittingInfo
   {
      private readonly IDictionary<String, Tuple<UInt32, Int32>> _strings;
      private readonly Boolean _saveSize;
      private readonly Encoding _encoding;
      private UInt32 _curIndex;
      private Boolean _accessed;

      internal StringHeapEmittingInfo( Encoding encoding, Boolean saveSize )
      {
         this._encoding = encoding;
         this._saveSize = saveSize;
         this._strings = new Dictionary<String, Tuple<UInt32, Int32>>();
         this._curIndex = 1;
         this._accessed = false;
      }

      internal UInt32 GetOrAddString( String str ) //, Boolean acceptNonEmpty )
      {
         if ( !this._accessed )
         {
            this._accessed = true;
         }

         UInt32 result;
         Tuple<UInt32, Int32> strInfo;
         if ( str == null ) // || ( str.Length == 0 && !acceptNonEmpty ) )
         {
            result = 0;
         }
         else if ( this._strings.TryGetValue( str, out strInfo ) )
         {
            result = strInfo.Item1;
         }
         else
         {
            result = this._curIndex;
            this.AddString( str );
         }
         return result;
      }

      internal UInt32 GetString( String str )
      {
         Tuple<UInt32, Int32> result;
         if ( str == null || !this._strings.TryGetValue( str, out result ) )
         {
            return 0;
         }
         return result.Item1;
      }

      internal UInt32 Size
      {
         get
         {
            return this._curIndex;
         }
      }

      internal Boolean Accessed
      {
         get
         {
            return this._accessed;
         }
      }

      internal void WriteStrings( Stream sink )
      {
         if ( this._accessed )
         {
            sink.WriteByte( 0 );
            if ( this._strings.Count > 0 )
            {
               var array = new Byte[BitUtils.MultipleOf4( this._curIndex ) - 1];
               var i = 0;
               foreach ( var kvp in this._strings )
               {
                  if ( this._saveSize )
                  {
                     array.CompressUInt32( ref i, kvp.Value.Item2 );
                  }
                  i += this.Serialize( kvp.Key, array, i );
                  if ( !this._saveSize )
                  {
                     array[i++] = 0;
                  }
               }
               sink.Write( array );
            }
         }
      }

      private void AddString( String str )
      {
         var byteCount = this._encoding.GetByteCount( str ) + 1;
         this._strings.Add( str, Tuple.Create( this._curIndex, byteCount ) );
         this._curIndex += (UInt32) byteCount;
         if ( this._saveSize )
         {
            this._curIndex += BitUtils.GetEncodedUIntSize( byteCount );
         }
      }

      protected virtual Int32 Serialize( String str, Byte[] array, Int32 arrayIndex )
      {
         return this._encoding.GetBytes( str, 0, str.Length, array, arrayIndex );
      }
   }

   internal sealed class UserStringHeapEmittingInfo : StringHeapEmittingInfo
   {
      internal UserStringHeapEmittingInfo()
         : base( MetaDataConstants.USER_STRING_ENCODING, true )
      {

      }

      protected override Int32 Serialize( String str, Byte[] array, Int32 arrayIndex )
      {
         var oldIdx = arrayIndex;
         byte lastByte = 0;
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
         array[arrayIndex++] = lastByte;
         return arrayIndex - oldIdx;
      }
   }

   internal sealed class SystemStringHeapEmittingInfo : StringHeapEmittingInfo
   {
      internal SystemStringHeapEmittingInfo()
         : base( MetaDataConstants.SYS_STRING_ENCODING, false )
      {

      }
   }

   internal class BLOBContainer
   {
      private readonly IDictionary<Byte[], UInt32> _blobIndices;
      private readonly IList<Byte[]> _blobs;
      private UInt32 _curIdx;
      private Boolean _accessed;


      internal BLOBContainer()
      {
         this._blobIndices = new Dictionary<Byte[], UInt32>( ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer );
         this._blobs = new List<Byte[]>();
         this._curIdx = 1;
         this._accessed = false;
      }

      internal UInt32 GetOrAddBLOB( Byte[] blob )
      {
         if ( !this._accessed )
         {
            this._accessed = true;
         }

         if ( blob == null )
         {
            return 0;
         }
         else
         {
            UInt32 result;
            if ( !this._blobIndices.TryGetValue( blob, out result ) )
            {
               result = this._curIdx;
               this._blobIndices.Add( blob, result );
               this._blobs.Add( blob );
               this._curIdx += (UInt32) blob.Length + BitUtils.GetEncodedUIntSize( blob.Length );
            }
            return result;
         }
      }

      internal UInt32 GetBLOB( Byte[] blob )
      {
         return blob == null ? 0 : this._blobIndices[blob];
      }

      internal UInt32 Size
      {
         get
         {
            return this._curIdx;
         }
      }

      internal Boolean Accessed
      {
         get
         {
            return this._accessed;
         }
      }

      internal void WriteBLOBs( Stream sink )
      {
         if ( this._accessed )
         {
            sink.WriteByte( 0 );
            var byteCount = 1u;
            if ( this._blobs.Count > 0 )
            {
               var tmpArray = new Byte[4];
               foreach ( var blob in this._blobs )
               {
                  var i = 0;
                  tmpArray.CompressUInt32( ref i, blob.Length );
                  sink.Write( tmpArray, 0, i );
                  sink.Write( blob );
                  byteCount += (UInt32) i + (UInt32) blob.Length;
               }
            }
            sink.SkipToNextAlignment( ref byteCount, 4 );
         }
      }
   }

   internal sealed class BinaryHeapEmittingInfo
   {

      private const Int32 DEFAULT_BLOCK_SIZE = 512;

      private readonly Int32 _blockSize;
      private readonly IList<Byte[]> _prevBlocks;
      private readonly IList<Int32> _prevBlockSizes;
      private readonly Encoding _stringEncoding;
      private Byte[] _curBlock;
      private Int32 _curCount;
      private Int32 _prevBlockIndex;

      internal BinaryHeapEmittingInfo()
         : this( DEFAULT_BLOCK_SIZE )
      {

      }

      internal BinaryHeapEmittingInfo( Int32 blockSize )
      {
         this._blockSize = Math.Max( blockSize, DEFAULT_BLOCK_SIZE );
         this._prevBlocks = new List<Byte[]>();
         this._prevBlockSizes = new List<Int32>();
         this._curCount = 0;
         this._prevBlockIndex = 0;
         this._curBlock = new Byte[this._blockSize];
         this._stringEncoding = new UTF8Encoding( false, true );
      }

      internal void AddByte( Byte aByte )
      {
         this.EnsureSize( 1 );
         this._curBlock[this._curCount++] = aByte;
      }

      internal void AddSByte( SByte sByte )
      {
         this.EnsureSize( 1 );
         this._curBlock[this._curCount++] = (Byte) sByte;
      }

      internal void AddUncompressedInt16( Int16 val )
      {
         this.EnsureSize( 2 );
         this._curBlock.WriteInt16LEToBytes( ref this._curCount, val );
      }

      internal void AddUncompressedUInt16( UInt16 val )
      {
         this.EnsureSize( 2 );
         this._curBlock.WriteUInt16LEToBytes( ref this._curCount, val );
      }

      internal void AddUncompressedInt32( Int32 value )
      {
         this.EnsureSize( 4 );
         this._curBlock.WriteInt32LEToBytes( ref this._curCount, value );
      }

      internal void AddUncompressedUInt32( UInt32 value )
      {
         this.EnsureSize( 4 );
         this._curBlock.WriteUInt32LEToBytes( ref this._curCount, value );
      }

      internal void AddUncompressedInt64( Int64 value )
      {
         this.EnsureSize( 8 );
         this._curBlock.WriteInt64LEToBytes( ref this._curCount, value );
      }

      internal void AddUncompressedUInt64( UInt64 value )
      {
         this.EnsureSize( 8 );
         this._curBlock.WriteUInt64LEToBytes( ref this._curCount, value );
      }

      internal void AddUncompressedSingle( Single value )
      {
         this.EnsureSize( 4 );
         this._curBlock.WriteSingleLEToBytes( ref this._curCount, value );
      }

      internal void AddUncompressedDouble( Double value )
      {
         this.EnsureSize( 8 );
         this._curBlock.WriteDoubleLEToBytes( ref this._curCount, value );
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
            if ( this._curCount + size > this._blockSize )
            {
               this.AddBytes( this._stringEncoding.GetBytes( str ) );
            }
            else
            {
               this._curCount += this._stringEncoding.GetBytes( str, 0, str.Length, this._curBlock, this._curCount );
            }
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
            if ( this._curCount + size > this._blockSize )
            {
               this.AddBytes( MetaDataConstants.USER_STRING_ENCODING.GetBytes( str ) );
            }
            else
            {
               this._curCount += MetaDataConstants.USER_STRING_ENCODING.GetBytes( str, 0, str.Length, this._curBlock, this._curCount );
            }
         }
      }

      internal void AddBytes( Byte[] bytes )
      {
         if ( bytes != null )
         {
            var written = 0;
            while ( written < bytes.Length )
            {
               if ( this._curCount == this._blockSize )
               {
                  this.EnsureSize( 1u );
               }
               var amountToWrite = Math.Min( bytes.Length - written, this._blockSize - this._curCount );
               this.EnsureSize( (UInt32) amountToWrite );
               Array.Copy( bytes, written, this._curBlock, this._curCount, amountToWrite );
               written += amountToWrite;
               this._curCount += amountToWrite;
            }
         }
      }

      internal void AddTDRSToken( TableIndex token )
      {
         this.AddCompressedUInt32( TokenUtils.EncodeTypeDefOrRefOrSpec( token.CreateTokenForEmittingMandatoryTableIndex() ) );
      }

      internal void AddCompressedUInt32( Int32 value )
      {
         this.EnsureSize( BitUtils.GetEncodedUIntSize( value ) );
         this._curBlock.CompressUInt32( ref this._curCount, value );
      }

      internal void AddCompressedInt32( Int32 value )
      {
         this.EnsureSize( BitUtils.GetEncodedIntSize( value ) );
         this._curBlock.CompressInt32( ref this._curCount, value );
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
         if ( this._curCount == 0 && this._prevBlockIndex == 0 )
         {
            result = Empty<Byte>.Array;
         }
         else
         {
            result = new Byte[this._prevBlockSizes.Sum() + this._curCount];
            var curIdx = 0;
            for ( var i = 0; i < this._prevBlocks.Count; ++i )
            {
               Array.Copy( this._prevBlocks[i], 0, result, curIdx, this._prevBlockSizes[i] );
               curIdx += this._prevBlockSizes[i];
            }
            Array.Copy( this._curBlock, 0, result, curIdx, this._curCount );

            this.Reset();
         }
         return result;
      }

      private void EnsureSize( UInt32 size )
      {
         if ( this._blockSize < this._curCount + size )
         {
            if ( this._prevBlockIndex >= this._prevBlocks.Count )
            {
               this._prevBlocks.Add( this._curBlock );
               this._prevBlockSizes.Add( this._curCount );
            }
            else
            {
               this._prevBlocks[this._prevBlockIndex] = this._curBlock;
               this._prevBlockSizes[this._prevBlockIndex] = this._curCount;
            }
            ++this._prevBlockIndex;
            this._curBlock = this._prevBlockIndex < this._prevBlocks.Count ?
               this._prevBlocks[this._prevBlockIndex] :
               new Byte[this._blockSize];
            this._curCount = 0;
         }
      }

      private void Reset()
      {
         if ( this._prevBlockIndex > 0 )
         {
            this._curBlock = this._prevBlocks[0];
         }
         this._prevBlockIndex = 0;
         this._curCount = 0;
      }

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

   internal static Byte[] CreateFieldSignature( this BinaryHeapEmittingInfo info, FieldSignature sig )
   {
      info.WriteFieldSignature( sig );
      return info.CreateByteArray();
   }

   internal static Byte[] CreateMethodSignature( this BinaryHeapEmittingInfo info, AbstractMethodSignature sig )
   {
      info.WriteMethodSignature( sig );
      return info.CreateByteArray();
   }

   internal static Byte[] CreateMemberRefSignature( this BinaryHeapEmittingInfo info, AbstractSignature sig )
   {
      return sig.SignatureKind == SignatureKind.Field ?
         info.CreateFieldSignature( (FieldSignature) sig ) :
         info.CreateMethodSignature( (MethodReferenceSignature) sig );
   }

   internal static Byte[] CreateConstantBytes( this BinaryHeapEmittingInfo info, Object constant )
   {
      info.WriteConstantValue( constant );
      return info.CreateByteArray();
   }

   internal static Byte[] CreateCustomAttributeSignature( this BinaryHeapEmittingInfo info, AbstractCustomAttributeSignature sig )
   {
      Byte[] retVal;
      if ( sig != null ) // sig.TypedArguments.Count > 0 || sig.NamedArguments.Count > 0 )
      {
         var sigg = sig as CustomAttributeSignature;
         if ( sigg != null )
         {
            info.WriteCustomAttributeSignature( sigg );
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

   internal static Byte[] CreateMarshalSpec( this BinaryHeapEmittingInfo info, MarshalingInfo marshal )
   {
      info.WriteMarshalInfo( marshal );
      return info.CreateByteArray();
   }

   internal static Byte[] CreateSecuritySignature( this BinaryHeapEmittingInfo info, SecurityDefinition security, BinaryHeapEmittingInfo aux )
   {
      info.WriteSecuritySignature( security, aux );
      return info.CreateByteArray();
   }

   internal static Byte[] CreateStandaloneSignature( this BinaryHeapEmittingInfo info, AbstractSignature sig )
   {
      var locals = sig as LocalVariablesSignature;
      if ( locals != null )
      {
         info.WriteLocalsSignature( locals );
      }
      else
      {
         info.WriteMethodSignature( sig as AbstractMethodSignature );
      }
      return info.CreateByteArray();
   }

   internal static Byte[] CreatePropertySignature( this BinaryHeapEmittingInfo info, PropertySignature sig )
   {
      info.WritePropertySignature( sig );
      return info.CreateByteArray();
   }

   internal static Byte[] CreateTypeSignature( this BinaryHeapEmittingInfo info, TypeSignature sig )
   {
      info.WriteTypeSignature( sig );
      return info.CreateByteArray();
   }

   internal static Byte[] CreateMethodSpecSignature( this BinaryHeapEmittingInfo info, GenericMethodSignature sig )
   {
      info.WriteMethodSpecSignature( sig );
      return info.CreateByteArray();
   }

   internal static void WriteFieldSignature( this BinaryHeapEmittingInfo info, FieldSignature sig )
   {
      if ( sig != null )
      {
         info.AddSigStarterByte( SignatureStarters.Field );
         info.WriteCustomModifiers( sig.CustomModifiers );
         info.WriteTypeSignature( sig.Type );
      }
   }

   private static void WriteCustomModifiers( this BinaryHeapEmittingInfo info, IList<CustomModifierSignature> mods )
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

   private static void WriteTypeSignature( this BinaryHeapEmittingInfo info, TypeSignature type )
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
            info.WriteTypeSignature( ptr.Type );
            break;

      }
   }

   private static void WriteMethodSignature( this BinaryHeapEmittingInfo info, AbstractMethodSignature method )
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

   private static void WriteParameterSignature( this BinaryHeapEmittingInfo info, ParameterSignature parameter )
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

   private static void WriteConstantValue( this BinaryHeapEmittingInfo info, Object constant )
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

   private static void WriteCustomAttributeSignature( this BinaryHeapEmittingInfo info, CustomAttributeSignature attrData )
   {
      // Prolog
      info.AddByte( 1 );
      info.AddByte( 0 );

      // Fixed args
      for ( var i = 0; i < attrData.TypedArguments.Count; ++i )
      {
         var arg = attrData.TypedArguments[i];
         info.WriteCustomAttributeFixedArg( arg.Type, arg.Value );
      }

      // Named args
      info.AddUncompressedUInt16( (UInt16) attrData.NamedArguments.Count );
      foreach ( var arg in attrData.NamedArguments )
      {
         info.WriteCustomAttributeNamedArg( arg );
      }
   }

   private static void WriteCustomAttributeFixedArg( this BinaryHeapEmittingInfo info, CustomAttributeArgumentType argType, Object arg )
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
               info.AddUncompressedInt32( ( (Array) arg ).Length );
               argType = ( (CustomAttributeArgumentTypeArray) argType ).ArrayType;

               foreach ( var elem in (Array) arg )
               {
                  info.WriteCustomAttributeFixedArg( argType, elem );
               }
            }
            break;
         case CustomAttributeArgumentTypeKind.Simple:
            switch ( ( (CustomAttributeArgumentSimple) argType ).SimpleType )
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
               case SignatureElementTypes.Type:
                  info.AddCAString( arg == null ? null : Convert.ToString( arg ) );
                  break;
               case SignatureElementTypes.Object:
                  if ( arg == null )
                  {
                     // Nulls are serialized as null strings
                     if ( !CustomAttributeArgumentSimple.String.Equals( argType ) )
                     {
                        argType = CustomAttributeArgumentSimple.String;
                     }
                  }
                  info.WriteCustomAttributeFieldOrPropType( argType );
                  info.WriteCustomAttributeFixedArg( argType, arg );
                  break;
            }
            break;
         case CustomAttributeArgumentTypeKind.TypeString:
            info.AddCAString( ( (CustomAttributeArgumentTypeEnum) argType ).TypeString );
            info.WriteConstantValue( arg );
            break;
      }
   }

   private static void WriteCustomAttributeNamedArg( this BinaryHeapEmittingInfo info, CustomAttributeNamedArgument arg )
   {
      var elem = arg.IsField ? SignatureElementTypes.CA_Field : SignatureElementTypes.CA_Property;
      info.AddSigByte( elem );
      var typedValue = arg.Value;
      info.WriteCustomAttributeFieldOrPropType( typedValue.Type );
      info.AddCAString( arg.Name );
      info.WriteCustomAttributeFixedArg( typedValue.Type, typedValue.Value );
   }


   private static void WriteCustomAttributeFieldOrPropType( this BinaryHeapEmittingInfo info, CustomAttributeArgumentType type )
   {
      switch ( type.ArgumentTypeKind )
      {
         case CustomAttributeArgumentTypeKind.Array:
            info.AddSigByte( SignatureElementTypes.SzArray );
            info.WriteCustomAttributeFieldOrPropType( ( (CustomAttributeArgumentTypeArray) type ).ArrayType );
            break;
         case CustomAttributeArgumentTypeKind.Simple:
            var sigStarter = ( (CustomAttributeArgumentSimple) type ).SimpleType;
            if ( sigStarter == SignatureElementTypes.Object )
            {
               sigStarter = SignatureElementTypes.CA_Boxed;
            }
            info.AddSigByte( ( (CustomAttributeArgumentSimple) type ).SimpleType );
            break;
         case CustomAttributeArgumentTypeKind.TypeString:
            info.AddSigByte( SignatureElementTypes.CA_Enum );
            info.AddCAString( ( (CustomAttributeArgumentTypeEnum) type ).TypeString );
            break;
      }
   }

   private static void WriteMarshalInfo( this BinaryHeapEmittingInfo info, MarshalingInfo marshal )
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

   private static void WriteSecuritySignature( this BinaryHeapEmittingInfo info, SecurityDefinition security, BinaryHeapEmittingInfo aux )
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
            // For some silly reason, the amount of bytes taken to serialize named attributes is stored at this point. Sigh...
            foreach ( var arg in secInfo.NamedArguments )
            {
               aux.WriteCustomAttributeNamedArg( arg );
            }
            // Now write to sec blob
            secInfoBLOB = aux.CreateByteArray();
            // The length of named arguments blob
            info.AddCompressedUInt32( secInfoBLOB.Length + (Int32) BitUtils.GetEncodedUIntSize( secInfo.NamedArguments.Count ) );
            // The amount of named arguments
            info.AddCompressedUInt32( secInfo.NamedArguments.Count );
         }
         else
         {
            secInfoBLOB = ( (RawSecurityInformation) sec ).Bytes;
         }

         info.AddBytes( secInfoBLOB );
      }

   }

   private static void WriteLocalsSignature( this BinaryHeapEmittingInfo info, LocalVariablesSignature sig )
   {
      if ( sig != null )
      {
         info.AddSigStarterByte( SignatureStarters.LocalSignature );
         var locals = sig.Locals;
         info.AddCompressedUInt32( locals.Count );
         foreach ( var local in locals )
         {
            if ( SimpleTypeSignature.TypedByRef.Equals( local.Type ) )
            {
               info.AddSigByte( SignatureElementTypes.TypedByRef );
            }
            else
            {
               info.WriteCustomModifiers( local.CustomModifiers );
               if ( local.IsPinned )
               {
                  info.AddSigByte( SignatureElementTypes.Pinned );
               }

               if ( local.IsByRef )
               {
                  info.AddSigByte( SignatureElementTypes.ByRef );
               }
               info.WriteTypeSignature( local.Type );
            }
         }
      }
   }

   private static void WritePropertySignature( this BinaryHeapEmittingInfo info, PropertySignature sig )
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

   private static void WriteMethodSpecSignature( this BinaryHeapEmittingInfo info, GenericMethodSignature sig )
   {
      info.AddSigStarterByte( SignatureStarters.MethodSpecGenericInst );
      info.AddCompressedUInt32( sig.GenericArguments.Count );
      foreach ( var gArg in sig.GenericArguments )
      {
         info.WriteTypeSignature( gArg );
      }
   }
}