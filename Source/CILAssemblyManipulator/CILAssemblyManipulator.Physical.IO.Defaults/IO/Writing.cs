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
extern alias CAMPhysical;
extern alias CAMPhysicalIO;

using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;

using CAMPhysicalIO;
using CAMPhysicalIO::CILAssemblyManipulator.Physical.IO;

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.IO;
using CILAssemblyManipulator.Physical.Crypto;
using CILAssemblyManipulator.Physical.IO.Defaults;
using CollectionsWithRoles.API;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CILAssemblyManipulator.Physical.Meta;

public static partial class E_CILPhysical
{


   /// <summary>
   /// This is extension method to write given <see cref="CILMetaData"/> to <see cref="Stream"/> using this <see cref="WriterFunctionalityProvider"/>.
   /// It takes into account the possible new stream, and the possible new meta data created by <see cref="WriterFunctionalityProvider.GetFunctionality"/> method.
   /// </summary>
   /// <param name="writerProvider">This <see cref="WriterFunctionalityProvider"/>.</param>
   /// <param name="stream">The <see cref="Stream"/> to write <see cref="CILMetaData"/> to.</param>
   /// <param name="md">The <see cref="CILMetaData"/> to write.</param>
   /// <param name="options">The <see cref="WritingOptions"/> object to control header values. May be <c>null</c>.</param>
   /// <param name="sn">The <see cref="StrongNameKeyPair"/> to use, if the <see cref="CILMetaData"/> should be strong-name signed.</param>
   /// <param name="delaySign">Setting this to <c>true</c> will leave the room for strong-name signature, but will not compute it.</param>
   /// <param name="cryptoCallbacks">The <see cref="CryptoCallbacks"/> to use, if the <see cref="CILMetaData"/> should be strong-name signed.</param>
   /// <param name="snAlgorithmOverride">Overrides the signing algorithm, which originally is deduceable from the key BLOB of the <see cref="StrongNameKeyPair"/>.</param>
   /// <param name="errorHandler">The callback to handle errors during serialization.</param>
   /// <returns>An instance of <see cref="ImageInformation"/> containing information about the headers and other values created during serialization.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="stream"/> or <paramref name="md"/> is <c>null</c>.</exception>
   /// <exception cref="NullReferenceException">If this <see cref="WriterFunctionalityProvider"/> is <c>null</c>.</exception>
   /// <remarks>
   /// This method is used by <see cref="E_CILPhysical.WriteModule"/> to actually perform serialization, and thus is rarely needed to be used directly.
   /// Instead, use <see cref="E_CILPhysical.WriteModule"/>.
   /// </remarks>
   /// <seealso cref="WriterFunctionalityProvider"/>
   /// <seealso cref="E_CILPhysical.WriteModule"/>
   /// <seealso cref="WriteMetaDataToStream(WriterFunctionality, Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/>
   public static ImageInformation WriteMetaDataToStream(
      this WriterFunctionalityProvider writerProvider,
      Stream stream,
      CILMetaData md,
      WritingOptions options,
      StrongNameKeyPair sn,
      Boolean delaySign,
      CryptoCallbacks cryptoCallbacks,
      AssemblyHashAlgorithm? snAlgorithmOverride,
      EventHandler<SerializationErrorEventArgs> errorHandler
      )
   {
      if ( writerProvider == null )
      {
         throw new NullReferenceException();
      }

      ArgumentValidator.ValidateNotNull( "Stream", stream );
      ArgumentValidator.ValidateNotNull( "Meta data", md );

      if ( options == null )
      {
         options = new WritingOptions();
      }

      CILMetaData newMD; Stream newStream;
      var writer = writerProvider.GetFunctionality( md, options, errorHandler, out newMD, out newStream ) ?? new DefaultWriterFunctionality( md, options, new TableSerializationInfoCreationArgs( errorHandler ) );
      if ( newMD != null )
      {
         md = newMD;
      }

      ImageInformation retVal;
      if ( newStream == null )
      {
         retVal = writer.WriteMetaDataToStream( stream, md, options, sn, delaySign, cryptoCallbacks, snAlgorithmOverride, errorHandler );
      }
      else
      {
         using ( newStream )
         {
            retVal = writer.WriteMetaDataToStream( stream, md, options, sn, delaySign, cryptoCallbacks, snAlgorithmOverride, errorHandler );
            newStream.Position = 0;
            newStream.CopyTo( stream );
         }
      }

      return retVal;
   }

   /// <summary>
   /// This is extension method to write given <see cref="CILMetaData"/> to <see cref="Stream"/> using this <see cref="WriterFunctionality"/>.
   /// </summary>
   /// <param name="writer">This <see cref="WriterFunctionality"/>.</param>
   /// <param name="stream">The <see cref="Stream"/> to write <see cref="CILMetaData"/> to.</param>
   /// <param name="md">The <see cref="CILMetaData"/> to write.</param>
   /// <param name="options">The <see cref="WritingOptions"/> object to control header values. May be <c>null</c>.</param>
   /// <param name="sn">The <see cref="StrongNameKeyPair"/> to use, if the <see cref="CILMetaData"/> should be strong-name signed.</param>
   /// <param name="delaySign">Setting this to <c>true</c> will leave the room for strong-name signature, but will not compute it.</param>
   /// <param name="cryptoCallbacks">The <see cref="CryptoCallbacks"/> to use, if the <see cref="CILMetaData"/> should be strong-name signed.</param>
   /// <param name="snAlgorithmOverride">Overrides the signing algorithm, which originally is deduceable from the key BLOB of the <see cref="StrongNameKeyPair"/>.</param>
   /// <param name="errorHandler">The callback to handle errors during serialization.</param>
   /// <returns>An instance of <see cref="ImageInformation"/> containing information about the headers and other values created during serialization.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="stream"/> or <paramref name="md"/> is <c>null</c>.</exception>
   /// <exception cref="NullReferenceException">If this <see cref="WriterFunctionality"/> is <c>null</c>.</exception>
   /// <exception cref="InvalidOperationException">If this <see cref="WriterFunctionality"/> returns invalid values.</exception>
   /// <remarks>
   /// This method is used by <see cref="E_CILPhysical.WriteModule"/> to actually perform serialization, and thus is rarely needed to be used directly.
   /// Instead, use <see cref="E_CILPhysical.WriteModule"/>.
   /// </remarks>
   /// <seealso cref="WriterFunctionality"/>
   /// <seealso cref="E_CILPhysical.WriteModule"/>
   public static ImageInformation WriteMetaDataToStream(
      this WriterFunctionality writer,
      Stream stream,
      CILMetaData md,
      WritingOptions options,
      StrongNameKeyPair sn,
      Boolean delaySign,
      CryptoCallbacks cryptoCallbacks,
      AssemblyHashAlgorithm? snAlgorithmOverride,
      EventHandler<SerializationErrorEventArgs> errorHandler
      )
   {
      // Check arguments
      if ( writer == null )
      {
         throw new NullReferenceException();
         //writer = new DefaultWriterFunctionality( md, options, new TableSerializationInfoCreationArgs( errorHandler ) );
      }

      ArgumentValidator.ValidateNotNull( "Stream", stream );
      ArgumentValidator.ValidateNotNull( "Meta data", md );
      if ( options == null )
      {
         options = new WritingOptions();
      }

      // 1. Create WritingStatus
      // Prepare strong name
      RSAParameters rParams; String snContainerName;
      var status = writer.CreateWritingStatus( md.PrepareStrongNameVariables( sn, ref delaySign, cryptoCallbacks, snAlgorithmOverride, out rParams, out snContainerName ) );
      if ( status == null )
      {
         throw new InvalidOperationException( "Writer failed to create writing status object." );
      }
      var snVars = status.StrongNameInformation;

      // 2. Create streams
      var mdStreams = writer.CreateMetaDataStreamHandlers().ToArrayProxy().CQ;
      var tblMDStream = mdStreams
         .OfType<WriterTableStreamHandler>()
         .FirstOrDefault() ?? new DefaultWriterTableStreamHandler( md, options.CLIOptions.TablesStreamOptions, DefaultMetaDataSerializationSupportProvider.Instance.CreateTableSerializationInfos( md, new TableSerializationInfoCreationArgs( errorHandler ) ).ToArrayProxy().CQ );

      var blobStream = mdStreams.OfType<WriterBLOBStreamHandler>().FirstOrDefault();
      var guidStream = mdStreams.OfType<WriterGUIDStreamHandler>().FirstOrDefault();
      var sysStringStream = mdStreams.OfType<WriterStringStreamHandler>().FirstOrDefault( s => String.Equals( s.StreamName, MetaDataConstants.SYS_STRING_STREAM_NAME ) );
      var userStringStream = mdStreams.OfType<WriterStringStreamHandler>().FirstOrDefault( s => String.Equals( s.StreamName, MetaDataConstants.USER_STRING_STREAM_NAME ) );
      var mdStreamContainer = new WriterMetaDataStreamContainer(
            blobStream,
            guidStream,
            sysStringStream,
            userStringStream,
            mdStreams.Where( s => !ReferenceEquals( tblMDStream, s ) && !ReferenceEquals( blobStream, s ) && !ReferenceEquals( guidStream, s ) && !ReferenceEquals( sysStringStream, s ) && !ReferenceEquals( userStringStream, s ) )
            );

      // 3. Populate streams
      var array = new ResizableArray<Byte>( initialSize: 0x1000 );
      var thHeader = tblMDStream.FillOtherMDStreams( snVars?.PublicKey?.ToArrayProxy()?.CQ, mdStreamContainer, array );
      if ( thHeader == null )
      {
         throw new InvalidOperationException( "Writer failed to create meta data table header." );
      }

      // 4. Create sections and some headers
      RVAConverter rvaConverter; Int32 mdRootSize;
      var dataRefs = writer.CalculateImageLayout(
         status,
         mdStreamContainer,
         mdStreams,
         out rvaConverter,
         out mdRootSize
         );

      if ( rvaConverter == null )
      {
         rvaConverter = new DefaultRVAConverter( status.SectionHeaders );
      }

      // 5. Position stream after headers, and write whatever is needed before meta data
      var headersSize = status.GetAlignedHeadersSize();
      stream.Position = headersSize;
      writer.BeforeMetaData( status, stream, array );

      var cliHeader = status.CLIHeader;
      if ( cliHeader == null )
      {
         throw new InvalidOperationException( "Writer failed to create CLI header." );
      }

      // 6. Write meta data
      stream.SeekFromBegin( rvaConverter.ToOffset( (UInt32) cliHeader.MetaData.RVA ) );
      array.CurrentMaxCapacity = mdRootSize;
      writer.WriteMDRoot( status, array );
      stream.Write( array.Array, mdRootSize );
      foreach ( var mds in mdStreams.Where( mds => mds.Accessed ) )
      {
         mds.WriteStream( stream, array, dataRefs );
      }

      // 7. Write whatever is needed after meta data
      writer.AfterMetaData( status, stream, array );

      // 8. Create and write image information
      var cliOptions = options.CLIOptions;
      var snSignature = snVars == null ? null : new Byte[snVars.SignatureSize];
      var cliHeaderOptions = cliOptions.HeaderOptions;
      var thOptions = cliOptions.TablesStreamOptions;
      var machine = status.Machine;
      var peOptions = options.PEOptions;
      var optionalHeaderKind = machine.GetOptionalHeaderKind();
      var optionalHeaderSize = optionalHeaderKind.GetOptionalHeaderSize( status.PEDataDirectories.Length );
      var sections = status.SectionHeaders.ToArrayProxy().CQ;
      var imageInfo = new ImageInformation(
         new PEInformation(
            new DOSHeader( 0x5A4D, 0x00000080u ),
            new NTHeader( 0x00004550,
               new FileHeader(
                  machine, // Machine
                  (UInt16) sections.Count, // Number of sections
                  (UInt32) ( peOptions.Timestamp ?? CreateNewPETimestamp() ), // Timestamp
                  0, // Pointer to symbol table
                  0, // Number of symbols
                  (UInt16) optionalHeaderSize,
                  ( peOptions.Characteristics ?? ( FileHeaderCharacteristics.ExecutableImage | FileHeaderCharacteristics.LargeAddressAware ) ).ProcessCharacteristics( options.IsExecutable )
                  ),
               peOptions.CreateOptionalHeader(
                  status,
                  sections,
                  (UInt32) headersSize,
                  optionalHeaderKind
                  )
               ),
            sections
            ),
         status.DebugInformation,
         new CLIInformation(
            cliHeader,
            status.MDRoot,
            thHeader,
            snSignature?.ToArrayProxy()?.CQ,
            dataRefs
            )
         );


      writer.WritePEInformation( status, stream, array, imageInfo.PEInformation );

      // 9. Compute strong name signature, if needed
      CreateStrongNameSignature(
         stream,
         snVars,
         delaySign,
         cryptoCallbacks,
         rParams,
         snContainerName,
         cliHeader,
         rvaConverter,
         snSignature,
         imageInfo.PEInformation,
         status.HeadersSizeUnaligned
         );

      return imageInfo;
   }

   private static StrongNameInformation PrepareStrongNameVariables(
      this CILMetaData md,
      StrongNameKeyPair strongName,
      ref Boolean delaySign,
      CryptoCallbacks cryptoCallbacks,
      AssemblyHashAlgorithm? algoOverride,
      out RSAParameters rParams,
      out String containerName
      )
   {
      var useStrongName = strongName != null;
      var snSize = 0;
      var aDefs = md.AssemblyDefinitions.TableContents;
      var thisAssemblyPublicKey = aDefs.Count > 0 ?
         aDefs[0].AssemblyInformation.PublicKeyOrToken.CreateArrayCopy() :
         null;

      if ( !delaySign )
      {
         delaySign = !useStrongName && !thisAssemblyPublicKey.IsNullOrEmpty();
      }
      var signingAlgorithm = AssemblyHashAlgorithm.SHA1;
      var computingHash = useStrongName || delaySign;

      if ( useStrongName && cryptoCallbacks == null )
      {
#if CAM_PHYSICAL_IS_PORTABLE
         throw new ArgumentException( "Assembly strong name was provided, but the crypto callbacks were not." );
#else
         cryptoCallbacks = new CryptoCallbacksDotNET();
#endif
      }

      StrongNameInformation retVal;

      if ( computingHash )
      {
         //// Set appropriate module flags
         //headers.ModuleFlags |= ModuleFlags.StrongNameSigned;

         // Check algorithm override
         var algoOverrideWasInvalid = algoOverride.HasValue && ( algoOverride.Value == AssemblyHashAlgorithm.MD5 || algoOverride.Value == AssemblyHashAlgorithm.None );
         if ( algoOverrideWasInvalid )
         {
            algoOverride = AssemblyHashAlgorithm.SHA1;
         }

         Byte[] pkToProcess;
         containerName = strongName?.ContainerName;
         if ( ( useStrongName && containerName != null ) || ( !useStrongName && delaySign ) )
         {
            if ( thisAssemblyPublicKey.IsNullOrEmpty() )
            {
               thisAssemblyPublicKey = cryptoCallbacks.ExtractPublicKeyFromCSPContainerAndCheck( containerName );
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
            snSize = rParams.Modulus.Length;
         }
         else if ( thisAssemblyPublicKey != null && thisAssemblyPublicKey.Length == 16 ) // The "Standard Public Key", ECMA-335 p. 116
         {
            // TODO throw instead (but some tests will fail then...)
            snSize = 0x100;
         }
         else
         {
            throw new CryptographicException( errorString );
         }

         retVal = new StrongNameInformation(
            signingAlgorithm,
            snSize,
            thisAssemblyPublicKey
            );
      }
      else
      {
         retVal = null;
         rParams = default( RSAParameters );
         containerName = null;
      }

      return retVal;
   }

   private static void CreateStrongNameSignature(
      Stream stream,
      StrongNameInformation snVars,
      Boolean delaySign,
      CryptoCallbacks cryptoCallbacks,
      RSAParameters rParams,
      String containerName,
      CLIHeader cliHeader,
      RVAConverter rvaConverter,
      Byte[] snSignatureArray,
      PEInformation imageInfo,
      Int32 headersSizeUnaligned
      )
   {
      if ( snVars != null && !delaySign )
      {
         using ( var rsa = ( containerName == null ? cryptoCallbacks.CreateRSAFromParameters( rParams ) : cryptoCallbacks.CreateRSAFromCSPContainer( containerName ) ) )
         {
            var algo = snVars.HashAlgorithm;
            var snSize = snVars.SignatureSize;
            var buffer = new Byte[0x8000];
            var hashEvtArgs = cryptoCallbacks.CreateHashStreamAndCheck( algo, true, true, false, true );
            var hashStream = hashEvtArgs.CryptoStream;
            var hashGetter = hashEvtArgs.HashGetter;
            var transform = hashEvtArgs.Transform;
            var sigOffset = rvaConverter.ToOffset( cliHeader.StrongNameSignature.RVA );
            Int32 idx;

            Byte[] strongNameArray;
            using ( var tf = transform )
            {
               using ( var cryptoStream = hashStream() )
               {
                  // TODO: WriterFunctionality should have method:
                  // IEnumerable<Tuple<Int64, Int64>> GetRangesSkippedInStrongNameSignatureCalculation(ImageInformation imageInfo);

                  // Calculate hash of required parts of file (ECMA-335, p.117)
                  // Read all headers first DOS header (start of file to the NT headers)
                  stream.SeekFromBegin( 0 );
                  var hdrArray = new Byte[headersSizeUnaligned];
                  stream.ReadSpecificAmount( hdrArray, 0, hdrArray.Length );

                  // Hash the checksum entry + authenticode as zeroes
                  const Int32 peCheckSumOffsetWithinOptionalHeader = 0x40;
                  var ntHeaderStart = (Int32) imageInfo.DOSHeader.NTHeaderOffset;
                  idx = ntHeaderStart
                     + DefaultWriterFunctionality.PE_SIG_AND_FILE_HEADER_SIZE // NT header signature + file header size
                     + peCheckSumOffsetWithinOptionalHeader; // Offset of PE checksum entry.
                  hdrArray.WriteInt32LEToBytes( ref idx, 0 );

                  var optionalHeaderSizeWithoutDataDirs = imageInfo.NTHeader.FileHeader.OptionalHeaderSize - CAMIOInternals.DATA_DIR_SIZE * imageInfo.NTHeader.OptionalHeader.DataDirectories.Count;

                  idx = ntHeaderStart
                     + DefaultWriterFunctionality.PE_SIG_AND_FILE_HEADER_SIZE // NT header signature + file header size
                     + optionalHeaderSizeWithoutDataDirs
                     + 4 * CAMIOInternals.DATA_DIR_SIZE; // Authenticode is 5th data directory, and optionalHeaderSize includes all data directories
                  hdrArray.WriteDataDirectory( ref idx, default( DataDirectory ) );
                  // Hash the correctly zeroed-out header data
                  cryptoStream.Write( hdrArray );

                  // Now, calculate hash for all sections, except we have to skip our own strong name signature hash part
                  foreach ( var section in imageInfo.SectionHeaders )
                  {
                     var min = section.RawDataPointer;
                     var max = min + section.RawDataSize;
                     stream.SeekFromBegin( min );
                     if ( min <= sigOffset && max >= sigOffset )
                     {
                        // Strong name signature is in this section
                        stream.CopyStreamPart( cryptoStream, buffer, sigOffset - min );
                        stream.SeekFromCurrent( snSize );
                        stream.CopyStreamPart( cryptoStream, buffer, max - sigOffset - snSize );
                     }
                     else
                     {
                        stream.CopyStreamPart( cryptoStream, buffer, max - min );
                     }
                  }
               }

               strongNameArray = cryptoCallbacks.CreateRSASignatureAndCheck( rsa, algo.GetAlgorithmName(), hashGetter() );
            }


            if ( snSize != strongNameArray.Length )
            {
               throw new CryptographicException( "Calculated and actual strong name size differ (calculated: " + snSize + ", actual: " + strongNameArray.Length + ")." );
            }
            Array.Reverse( strongNameArray );

            // Write strong name
            stream.Seek( sigOffset, SeekOrigin.Begin );
            stream.Write( strongNameArray );
            idx = 0;
            snSignatureArray.BlockCopyFrom( ref idx, strongNameArray );
         }
      }
   }

   private static Int32 CreateNewPETimestamp()
   {
      return (Int32) ( DateTime.UtcNow - new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc ) ).TotalSeconds;
   }

   private static FileHeaderCharacteristics ProcessCharacteristics(
      this FileHeaderCharacteristics characteristics,
      Boolean isExecutable
      )
   {
      return isExecutable ?
         ( characteristics & ~FileHeaderCharacteristics.Dll ) :
         ( characteristics | FileHeaderCharacteristics.Dll );
   }

   private static OptionalHeader CreateOptionalHeader(
      this WritingOptions_PE options,
      WritingStatus writingStatus,
      ArrayQuery<SectionHeader> sections,
      UInt32 headersSize,
      OptionalHeaderKind kind
      )
   {
      const Byte linkerMajor = 0x0B;
      const Byte linkerMinor = 0x00;
      const Int16 osMajor = 0x04;
      const Int16 osMinor = 0x00;
      const Int16 userMajor = 0x0000;
      const Int16 userMinor = 0x0000;
      const Int16 subsystemMajor = 0x0004;
      const Int16 subsystemMinor = 0x0000;
      const Subsystem subsystem = Subsystem.WindowsConsole;
      const DLLFlags dllFlags = DLLFlags.DynamicBase | DLLFlags.NXCompatible | DLLFlags.NoSEH | DLLFlags.TerminalServerAware;

      // Calculate various sizes in one iteration of sections
      var sAlign = (UInt32) writingStatus.SectionAlignment;
      var fAlign = (UInt32) writingStatus.FileAlignment;
      var imageSize = headersSize.RoundUpU32( sAlign );
      var dataBase = 0u;
      var codeBase = 0u;
      var codeSize = 0u;
      var initDataSize = 0u;
      var uninitDataSize = 0u;
      foreach ( var section in sections )
      {
         var chars = section.Characteristics;
         var curSize = section.RawDataSize;

         if ( chars.HasFlag( SectionHeaderCharacteristics.Contains_Code ) )
         {
            if ( codeBase == 0u )
            {
               codeBase = imageSize;
            }
            codeSize += curSize;
         }
         if ( chars.HasFlag( SectionHeaderCharacteristics.Contains_InitializedData ) )
         {
            if ( dataBase == 0u )
            {
               dataBase = imageSize;
            }
            initDataSize += curSize;
         }
         if ( chars.HasFlag( SectionHeaderCharacteristics.Contains_UninitializedData ) )
         {
            if ( dataBase == 0u )
            {
               dataBase = imageSize;
            }
            uninitDataSize += curSize;
         }

         imageSize += curSize.RoundUpU32( sAlign );
      }

      var ep = (UInt32) writingStatus.EntryPointRVA.GetValueOrDefault();
      var imageBase = writingStatus.ImageBase;

      switch ( kind )
      {
         case OptionalHeaderKind.Optional32:
            return new OptionalHeader32(
               options.MajorLinkerVersion ?? linkerMajor,
               options.MinorLinkerVersion ?? linkerMinor,
               codeSize,
               initDataSize,
               uninitDataSize,
               ep,
               codeBase,
               dataBase,
               (UInt32) imageBase,
               sAlign,
               fAlign,
               (UInt16) ( options.MajorOSVersion ?? osMajor ),
               (UInt16) ( options.MinorOSVersion ?? osMinor ),
               (UInt16) ( options.MajorUserVersion ?? userMajor ),
               (UInt16) ( options.MinorUserVersion ?? userMinor ),
               (UInt16) ( options.MajorSubsystemVersion ?? subsystemMajor ),
               (UInt16) ( options.MinorSubsystemVersion ?? subsystemMinor ),
               (UInt32) ( options.Win32VersionValue ?? 0x00000000 ),
               imageSize,
               headersSize,
               0x00000000,
               options.Subsystem ?? subsystem,
               options.DLLCharacteristics ?? dllFlags,
               (UInt32) ( options.StackReserveSize ?? 0x00100000 ),
               (UInt32) ( options.StackCommitSize ?? 0x00001000 ),
               (UInt32) ( options.HeapReserveSize ?? 0x00100000 ),
               (UInt32) ( options.HeapCommitSize ?? 0x00001000 ),
               options.LoaderFlags ?? 0x00000000,
               (UInt32) ( options.NumberOfDataDirectories ?? (Int32) DataDirectories.MaxValue ),
               writingStatus.PEDataDirectories.ToArrayProxy().CQ
               );
         case OptionalHeaderKind.Optional64:
            return new OptionalHeader64(
               options.MajorLinkerVersion ?? linkerMajor,
               options.MinorLinkerVersion ?? linkerMinor,
               codeSize,
               initDataSize,
               uninitDataSize,
               ep,
               codeBase,
               (UInt64) imageBase,
               sAlign,
               fAlign,
               (UInt16) ( options.MajorOSVersion ?? osMajor ),
               (UInt16) ( options.MinorOSVersion ?? osMinor ),
               (UInt16) ( options.MajorUserVersion ?? userMajor ),
               (UInt16) ( options.MinorUserVersion ?? userMinor ),
               (UInt16) ( options.MajorSubsystemVersion ?? subsystemMajor ),
               (UInt16) ( options.MinorSubsystemVersion ?? subsystemMinor ),
               (UInt32) ( options.Win32VersionValue ?? 0x00000000 ),
               imageSize,
               headersSize,
               0x00000000,
               options.Subsystem ?? subsystem,
               options.DLLCharacteristics ?? dllFlags,
               (UInt64) ( options.StackReserveSize ?? 0x0000000000400000 ),
               (UInt64) ( options.StackCommitSize ?? 0x0000000000004000 ),
               (UInt64) ( options.HeapReserveSize ?? 0x0000000000100000 ),
               (UInt64) ( options.HeapCommitSize ?? 0x0000000000002000 ),
               options.LoaderFlags ?? 0x00000000,
               (UInt32) ( options.NumberOfDataDirectories ?? (Int32) DataDirectories.MaxValue ),
               writingStatus.PEDataDirectories.ToArrayProxy().CQ
               );
         default:
            throw new ArgumentException( "Unsupported optional header kind: " + kind + "." );
      }

   }

   internal static Int32 SetCapacityAndAlign( this ResizableArray<Byte> array, Int64 streamPosition, Int32 dataSize, Int32 dataAlignmnet )
   {
      var paddingBefore = (Int32) ( streamPosition.RoundUpI64( dataAlignmnet ) - streamPosition );
      array.CurrentMaxCapacity = paddingBefore + dataSize;
      return paddingBefore;
   }

   internal static void SkipAlignedData( this Stream stream, Int32 dataSize, Int32 dataAlignment )
   {
      stream.Position = stream.Position.RoundUpI64( dataAlignment ) + dataSize;
   }

   /// <summary>
   /// Creates an <see cref="IEnumerable{T}"/> to enumerate all the streams of this <see cref="MetaDataStreamContainer{TAbstractStream, TBLOBStream, TGUIDStream, TStringStream}"/>.
   /// </summary>
   /// <typeparam name="TAbstractStream">The type of the abstract meta data stream.</typeparam>
   /// <typeparam name="TBLOBStream">The type of the BLOB meta data stream.</typeparam>
   /// <typeparam name="TGUIDStream">The type of the GUID meta data stream.</typeparam>
   /// <typeparam name="TStringStream">The type of the various string meta data streams.</typeparam>
   /// <param name="mdStreams">This <see cref="MetaDataStreamContainer{TAbstractStream, TBLOBStream, TGUIDStream, TStringStream}"/>.</param>
   /// <returns>An enumerable to enumerate all of the streams of this <see cref="MetaDataStreamContainer{TAbstractStream, TBLOBStream, TGUIDStream, TStringStream}"/>.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="MetaDataStreamContainer{TAbstractStream, TBLOBStream, TGUIDStream, TStringStream}"/> is <c>null</c>.</exception>
   public static IEnumerable<TAbstractStream> GetAllStreams<TAbstractStream, TBLOBStream, TGUIDStream, TStringStream>( this MetaDataStreamContainer<TAbstractStream, TBLOBStream, TGUIDStream, TStringStream> mdStreams )
      where TAbstractStream : AbstractMetaDataStreamHandler
      where TBLOBStream : TAbstractStream
      where TGUIDStream : TAbstractStream
      where TStringStream : TAbstractStream
   {
      yield return mdStreams.BLOBs;
      yield return mdStreams.GUIDs;
      yield return mdStreams.SystemStrings;
      yield return mdStreams.UserStrings;
      foreach ( var os in mdStreams.OtherStreams )
      {
         yield return os;
      }
   }

   /// <summary>
   /// This is helper method to get the aligned size of the DOS and NT headers of the PE image.
   /// </summary>
   /// <param name="status">This <see cref="WritingStatus"/>.</param>
   /// <returns>The aligned size of the DOS and NT headers, which will be <see cref="WritingStatus.HeadersSizeUnaligned"/> aligned to <see cref="WritingStatus.FileAlignment"/>.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="WritingStatus"/> is <c>null</c>.</exception>
   public static Int32 GetAlignedHeadersSize( this WritingStatus status )
   {
      return status.HeadersSizeUnaligned.RoundUpI32( status.FileAlignment );
   }
}