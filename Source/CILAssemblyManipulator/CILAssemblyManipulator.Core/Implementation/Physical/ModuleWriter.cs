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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CILAssemblyManipulator.API;
using CILAssemblyManipulator.Implementation;
using CommonUtils;

namespace CILAssemblyManipulator.Implementation.Physical
{
   internal class ModuleWriter
   {
      private const Int32 USER_STRING_MASK = 0x70 << 24;
      internal const Int32 MIN_FILE_ALIGNMENT = 0x200;
      internal const Int32 DEFAULT_SECTION_ALIGNMENT = 0x2000;
      internal const String DEFAULT_IMPORT_DIRECTORY_NAME = "mscoree.dll";
      internal const String DEFAULT_DLL_HINT_NAME = "_CorDllMain";
      internal const String DEFAULT_EXE_HINT_NAME = "_CorExeMain";
      internal const Int16 DEFAULT_ENTRY_POINT_INSTRUCTION = 0x25FF;
      internal const UInt64 DEFAULT_IMAGE_BASE = 0x00400000;
      internal const UInt64 IMAGE_BASE_MULTIPLE = 0x10000; // ECMA-335, p. 279
      internal const UInt32 RELOCATION_PAGE_SIZE = 0x1000; // ECMA-335, p. 282
      internal const UInt32 RELOCATION_FIXUP_TYPE = 0x3; // ECMA-335, p. 282
      internal const UInt64 DEFAULT_STACK_RESERVE = 0x100000; // ECMA-335, p. 280
      internal const UInt64 DEFAULT_STACK_COMMIT = 0x1000; // ECMA-335, p. 280
      internal const UInt64 DEFAULT_HEAP_RESERVE = 0x100000; // ECMA-335, p. 280
      internal const UInt64 DEFAULT_HEAP_COMMIT = 0x1000; // ECMA-335, p. 280
      internal const Byte DEFAULT_LINKER_MAJOR = 0x0B;
      internal const Byte DEFAULT_LINKER_MINOR = 0x00;
      internal const UInt16 DEFAULT_OS_MAJOR = 0x04;
      internal const UInt16 DEFAULT_OS_MINOR = 0x00;
      internal const UInt16 DEFAULT_USER_MAJOR = 0x00;
      internal const UInt16 DEFAULT_USER_MINOR = 0x00;
      internal const UInt16 DEFAULT_SUBSYS_MAJOR = 0x04;
      internal const UInt16 DEFAULT_SUBSYS_MINOR = 0x00;
      internal const String CODE_SECTION_NAME = ".text";
      internal const String RESOURCE_SECTION_NAME = ".rsrc";
      internal const String RELOCATION_SECTION_NAME = ".reloc";
      private const Int32 DEBUG_DATA_FIXED_SIZE = 24;
      private const UInt32 DEBUG_DATA_DIR_SIG = 0x53445352; // RSDS



      private static readonly DateTime PE_HEADER_START_TIME = new DateTime( 1970, 1, 1, 0, 0, 0 );

      private readonly CILReflectionContextImpl _context;
      private readonly CILModule _module;

      internal ModuleWriter( CILModule module )
      {
         ArgumentValidator.ValidateNotNull( "Module", module );

         this._context = (CILReflectionContextImpl) module.ReflectionContext;
         this._module = module;
      }

      public void PerformEmitting(
         Stream sink,
         EmittingArguments eArgs
         )
      {
         ArgumentValidator.ValidateNotNull( "Stream", sink );
         ArgumentValidator.ValidateNotNull( "Emitting arguments", eArgs );

         Boolean isPE64, hasRelocations;
         UInt16 peOptionalHeaderSize;
         UInt32 numSections, iatSize;
         CheckEmittingArgs( eArgs, out isPE64, out hasRelocations, out numSections, out peOptionalHeaderSize, out iatSize );

         var fAlign = eArgs.FileAlignment;
         var sAlign = eArgs.SectionAlignment;
         var importHintName = eArgs.ImportHintName;
         var imageBase = eArgs.ImageBase;
         var moduleKind = eArgs.ModuleKind;
         var strongName = eArgs.StrongName;

         IList<CILMethodBase> allMethodDefs;
         CILAssemblyName an;
         using ( var md = new MetaDataWriter( eArgs, this._context, this._module, eArgs.AssemblyMapper, out allMethodDefs, out an ) )
         {
            var clrEntryPointToken = 0;
            if ( eArgs.CLREntryPoint != null )
            {
               var listIdx = allMethodDefs.IndexOf( eArgs.CLREntryPoint );
               if ( listIdx < 0 )
               {
                  throw new ArgumentException( "Entry point method " + eArgs.CLREntryPoint + " is not from this module (" + this._module.Name + ")." );
               }
               clrEntryPointToken = TokenUtils.EncodeToken( Tables.MethodDef, listIdx + 1 );
            }

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
               .WriteUInt16LEToBytes( ref idx, (UInt16) eArgs.Machine )
               .WriteUInt16LEToBytes( ref idx, (UInt16) numSections )
               .WriteInt32LEToBytes( ref idx, Convert.ToInt32( DateTime.Now.Subtract( PE_HEADER_START_TIME ).TotalSeconds ) )
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
               eArgs.ModuleFlags |= ModuleFlags.StrongNameSigned;

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
               .WriteByteToBytes( ref idx, eArgs.LinkerMajor ) // Linker major version
               .WriteByteToBytes( ref idx, eArgs.LinkerMinor ) // Linker minor version
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
            if ( eArgs.HighEntropyVA )
            {
               dllFlags |= DLLFlags.HighEntropyVA;
            }
            ( isPE64 ? currentArray.WriteUInt64LEToBytes( ref idx, imageBase ) : currentArray.WriteUInt32LEToBytes( ref idx, (UInt32) imageBase ) )
               .WriteUInt32LEToBytes( ref idx, sAlign ) // Section alignment
               .WriteUInt32LEToBytes( ref idx, fAlign ) // File alignment
               .WriteUInt16LEToBytes( ref idx, eArgs.OSMajor ) // OS Major
               .WriteUInt16LEToBytes( ref idx, eArgs.OSMinor ) // OS Minor
               .WriteUInt16LEToBytes( ref idx, eArgs.UserMajor ) // User Major
               .WriteUInt16LEToBytes( ref idx, eArgs.UserMinor ) // User Minor
               .WriteUInt16LEToBytes( ref idx, eArgs.SubSysMajor ) // SubSys Major
               .WriteUInt16LEToBytes( ref idx, eArgs.SubSysMinor ) // SubSys Minor
               .WriteUInt32LEToBytes( ref idx, 0 ) // Reserved
               .WriteUInt32LEToBytes( ref idx, prevSectionInfo.virtualAddress + BitUtils.MultipleOf( sAlign, prevSectionInfo.virtualSize ) ) // Image Size
               .WriteUInt32LEToBytes( ref idx, textSectionInfo.rawPointer ) // Header Size
               .WriteUInt32LEToBytes( ref idx, 0 ) // File Checksum
               .WriteUInt16LEToBytes( ref idx, GetSubSystem( moduleKind ) ) // SubSystem
               .WriteUInt16LEToBytes( ref idx, (UInt16) dllFlags ); // DLL Characteristics
            if ( isPE64 )
            {
               currentArray
                  .WriteUInt64LEToBytes( ref idx, eArgs.StackReserve ) // Stack Reserve Size
                  .WriteUInt64LEToBytes( ref idx, eArgs.StackCommit ) // Stack Commit Size
                  .WriteUInt64LEToBytes( ref idx, eArgs.HeapReserve ) // Heap Reserve Size
                  .WriteUInt64LEToBytes( ref idx, eArgs.HeapCommit ); // Heap Commit Size
            }
            else
            {
               currentArray
                  .WriteUInt32LEToBytes( ref idx, (UInt32) eArgs.StackReserve ) // Stack Reserve Size
                  .WriteUInt32LEToBytes( ref idx, (UInt32) eArgs.StackCommit ) // Stack Commit Size
                  .WriteUInt32LEToBytes( ref idx, (UInt32) eArgs.HeapReserve ) // Heap Reserve Size
                  .WriteUInt32LEToBytes( ref idx, (UInt32) eArgs.HeapCommit ); // Heap Commit Size
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
               .WriteUInt16LEToBytes( ref idx, eArgs.CLIMajor ) // MajorRuntimeVersion
               .WriteUInt16LEToBytes( ref idx, eArgs.CLIMinor ) // MinorRuntimeVersion
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
      }

      private static void CheckEmittingArgs(
         EmittingArguments eArgs,
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

   internal struct SectionInfo
   {
      internal readonly UInt32 virtualSize;
      internal readonly UInt32 virtualAddress;
      internal readonly UInt32 rawSize;
      internal readonly UInt32 rawPointer;

      internal SectionInfo( Stream sink, SectionInfo? prevSection, UInt32 bytesWrittenInThisSection, UInt32 sectionAlignment, UInt32 fileAlignment, Boolean actuallyPad )
      {
         this.virtualSize = bytesWrittenInThisSection;
         this.virtualAddress = prevSection.HasValue ? ( prevSection.Value.virtualAddress + BitUtils.MultipleOf( sectionAlignment, prevSection.Value.virtualSize ) ) : sectionAlignment;
         this.rawPointer = prevSection.HasValue ? ( prevSection.Value.rawPointer + prevSection.Value.rawSize ) : fileAlignment; // prevSection.rawSize should always be multiple of file alignment
         this.rawSize = BitUtils.MultipleOf( fileAlignment, bytesWrittenInThisSection );
         if ( actuallyPad )
         {
            for ( var i = this.virtualSize; i < this.rawSize; ++i )
            {
               sink.WriteByte( 0 );
            }
         }
         else
         {
            sink.Seek( this.rawSize - this.virtualSize, SeekOrigin.Current );
         }
      }
   }
}