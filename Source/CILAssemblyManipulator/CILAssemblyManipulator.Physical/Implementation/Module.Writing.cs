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

namespace CILAssemblyManipulator.Physical.Implementation
{
   internal static class ModuleWriter
   {
      internal const Int32 MIN_FILE_ALIGNMENT = 0x200;
      internal const UInt64 IMAGE_BASE_MULTIPLE = 0x10000; // ECMA-335, p. 279

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
         if ( headers.CLREntryPointIndex.HasValue )
         {
            clrEntryPointToken = TokenUtils.EncodeToken( Tables.MethodDef, headers.CLREntryPointIndex.Value + 1 );
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
         var delaySign = eArgs.DelaySign || ( !useStrongName && !an.PublicKey.IsNullOrEmpty() );
         RSAParameters rParams;
         var signingAlgorithm = AssemblyHashAlgorithm.SHA1;
         if ( useStrongName || delaySign )
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
               if ( an.PublicKey.IsNullOrEmpty() )
               {
                  an.PublicKey = this._context.ExtractPublicKeyFromCSP( strongName.ContainerName );
               }
               pkToProcess = an.PublicKey;
            }
            else
            {
               // Get public key from BLOB
               pkToProcess = strongName.KeyPair.ToArray();
            }

            // Create RSA parameters and process public key so that it will have proper, full format.
            Byte[] pk;
            rParams = CryptoUtils.CreateSigningInformationFromKeyBLOB( pkToProcess, algoOverride, out pk, out signingAlgorithm );
            an.PublicKey = pk;
            snSize = (UInt32) rParams.Modulus.Length;
         }
         else
         {
            rParams = default( RSAParameters );
         }

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
         var methodRVAs = new Dictionary<CILMethodBase, UInt32>( allMethodDefs.Count );


         foreach ( var method in allMethodDefs )
         {
            if ( method.HasILMethodBody() )
            {
               Boolean isTiny;
               var array = new MethodILWriter( this._context, md, method, eArgs.AssemblyMapper )
                  .PerformEmitting( currentOffset, out isTiny );
               if ( !isTiny )
               {
                  sink.SkipToNextAlignment( ref currentOffset, 4 );
               }
               methodRVAs.Add( method, codeSectionVirtualOffset + currentOffset );
               sink.Write( array );
               currentOffset += (UInt32) array.Length;
            }
         }

         // Write padding
         sink.SkipToNextAlignment( ref currentOffset, isPE64 ? 0x10u : 0x04u );

         // Write manifest resources here
         var mRes = this._module.ManifestResources;
         var mResInfo = new Dictionary<String, UInt32>();
         var mResRVA = mRes.Values.Any( mr => mr is EmbeddedManifestResource ) ?
            codeSectionVirtualOffset + currentOffset :
            0u;
         var mResSize = 0u;
         if ( mResRVA > 0u )
         {
            var tmpArray = new Byte[4];
            foreach ( var kvp in mRes )
            {
               if ( kvp.Value is EmbeddedManifestResource )
               {
                  var data = ( (EmbeddedManifestResource) kvp.Value ).Data;
                  if ( data != null && data.Length > 0 )
                  {
                     mResInfo.Add( kvp.Key, mResSize );
                     tmpArray.WriteInt32LEToBytesNoRef( 0, data.Length );
                     sink.Write( tmpArray );
                     sink.Write( data );
                     mResSize += 4 + (UInt32) data.Length;
                  }
               }
            }

            // Write padding
            currentOffset += mResSize;
            sink.SkipToNextAlignment( ref currentOffset, isPE64 ? 0x10u : 0x04u );
         }
         // Finalize & write metadata
         var mdRVA = codeSectionVirtualOffset + currentOffset;
         UInt32 addedToOffsetBeforeMD;
         var mdSize = md.WriteMetaData( sink, mdRVA, eArgs, methodRVAs, mResInfo, out addedToOffsetBeforeMD );
         mdRVA += addedToOffsetBeforeMD;
         currentOffset += mdSize + addedToOffsetBeforeMD;

         // Pad
         sink.SkipToNextAlignment( ref currentOffset, 0x4 );

         // Write debug header if present
         var dbgInfo = eArgs.DebugInformation;
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
            foreach ( var chr in eArgs.ImportDirectoryName )
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
               .WriteInt16LEToBytes( ref idx, eArgs.EntryPointInstruction )
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
         var moduleFlags = eArgs.ModuleFlags;
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

         if ( useStrongName || delaySign )
         {
            if ( !delaySign )
            {
               // Try create RSA first
               var rsaArgs = strongName.ContainerName == null ? new RSACreationEventArgs( rParams ) : new RSACreationEventArgs( strongName.ContainerName );
               this._context.LaunchRSACreationEvent( rsaArgs );
               using ( var rsa = rsaArgs.RSA )
               {
                  var buffer = new Byte[MetaDataConstants.STREAM_COPY_BUFFER_SIZE];
                  Func<Stream> hashStream; Func<Byte[]> hashGetter; IDisposable transform;
                  this._context.LaunchHashStreamEvent( signingAlgorithm, out hashStream, out hashGetter, out transform );

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


                  this._context.LaunchRSASignatureCreationEvent( sigArgs );
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
}
