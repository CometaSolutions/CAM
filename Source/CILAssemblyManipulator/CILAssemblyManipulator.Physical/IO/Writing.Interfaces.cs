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
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.Implementation;
using CILAssemblyManipulator.Physical.IO;
using CollectionsWithRoles.API;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO
{
   public interface WriterFunctionalityProvider
   {
      WriterFunctionality GetFunctionality(
         CILMetaData md,
         WritingOptions options,
         out CILMetaData newMD,
         out Stream newStream
         );
   }

   public interface WriterFunctionality
   {
      IEnumerable<AbstractWriterStreamHandler> CreateStreamHandlers();

      Int32 GetSectionCount( ImageFileMachine machine );

      RawValueProvider PopulateSections(
         WritingStatus writingStatus,
         WriterMetaDataStreamContainer mdStreamContainer,
         ArrayQuery<AbstractWriterStreamHandler> allMDStreams,
         SectionHeader[] sections,
         out RVAConverter rvaConverter,
         out MetaDataRoot mdRoot,
         out Int32 mdRootSize,
         out Int32 mdSize
         );

      void BeforeMetaData(
         Stream stream,
         ResizableArray<Byte> array,
         ArrayQuery<SectionHeader> sections,
         WritingStatus writingStatus,
         RVAConverter rvaConverter,
         MetaDataRoot mdRoot,
         out CLIHeader cliHeader
         );

      void WriteMDRoot(
         MetaDataRoot mdRoot,
         ResizableArray<Byte> array
         );

      void AfterMetaData(
         Stream stream,
         ResizableArray<Byte> array,
         ArrayQuery<SectionHeader> sections,
         WritingStatus writingStatus,
         RVAConverter rvaConverter
         );

      void WritePEInformation(
         Stream stream,
         ResizableArray<Byte> array,
         WritingStatus writingStatus,
         PEInformation peInfo
         );
   }

   public interface RawValueProvider
   {
      Int32 GetRawValueFor( Tables table, Int32 rowIndex, Int32 columnIndex );

      IEnumerable<Int32> GetRawValuesFor( Tables table, Int32 columnIndex );
   }

   public class WriterMetaDataStreamContainer
   {
      public WriterMetaDataStreamContainer(
         WriterBLOBStreamHandler blobs,
         WriterGUIDStreamHandler guids,
         WriterStringStreamHandler sysStrings,
         WriterStringStreamHandler userStrings,
         IEnumerable<AbstractWriterStreamHandler> otherStreams
         )
      {
         this.BLOBs = blobs;
         this.GUIDs = guids;
         this.SystemStrings = sysStrings;
         this.UserStrings = userStrings;
         this.OtherStreams = otherStreams.ToArrayProxy().CQ;
      }

      public WriterBLOBStreamHandler BLOBs { get; }

      public WriterGUIDStreamHandler GUIDs { get; }

      public WriterStringStreamHandler SystemStrings { get; }

      public WriterStringStreamHandler UserStrings { get; }

      public ArrayQuery<AbstractWriterStreamHandler> OtherStreams { get; }
   }


   public interface AbstractWriterStreamHandler
   {
      String StreamName { get; }

      void WriteStream(
         Stream sink,
         ResizableArray<Byte> array,
         RawValueProvider rawValueProvder,
         RVAConverter rvaConverter
         );

      Int32 CurrentSize { get; }

      Boolean Accessed { get; }
   }

   public interface WriterTableStreamHandler : AbstractWriterStreamHandler
   {
      RawValueStorage<Int32> FillHeaps(
         ArrayQuery<Byte> thisAssemblyPublicKeyIfPresentNull,
         WriterMetaDataStreamContainer mdStreams,
         ResizableArray<Byte> array,
         out MetaDataTableStreamHeader header
         );
   }

   public interface WriterBLOBStreamHandler : AbstractWriterStreamHandler
   {
      Int32 RegisterBLOB( Byte[] blob );
   }

   public interface WriterStringStreamHandler : AbstractWriterStreamHandler
   {
      Int32 RegisterString( String systemString );
   }

   public interface WriterGUIDStreamHandler : AbstractWriterStreamHandler
   {
      Int32 RegisterGUID( Guid? guid );
   }

   public interface WriterCustomStreamHandler : AbstractWriterStreamHandler
   {
   }

   public class WritingStatus
   {
      public WritingStatus(
         Int32 headersSize,
         ImageFileMachine machine,
         Int32 fileAlignment,
         Int64? imageBase,
         StrongNameVariables strongNameVariables,
         Int32? dataDirCount
         )
      {
         this.HeadersSizeUnaligned = headersSize;
         this.HeadersSize = headersSize.RoundUpI32( fileAlignment );
         this.Machine = machine;
         this.FileAlignment = fileAlignment;
         this.ImageBase = imageBase ?? ( machine.RequiresPE64() ? 0x0000000140000000 : 0x0000000000400000 );
         this.StrongNameVariables = strongNameVariables;
         this.PEDataDirectories = new List<DataDirectory>( Enumerable.Repeat<DataDirectory>( default( DataDirectory ), dataDirCount ?? (Int32) DataDirectories.MaxValue ) );
      }

      public Int32 HeadersSize { get; }

      public Int32 HeadersSizeUnaligned { get; }

      public ImageFileMachine Machine { get; }

      public Int32 FileAlignment { get; }

      public Int64 ImageBase { get; }

      public Int32 SectionAlignment { get; set; }

      public StrongNameVariables StrongNameVariables { get; }

      public List<DataDirectory> PEDataDirectories { get; }

      //public Int64? OffsetAfterInitialRawValues { get; set; }

      public Int64? EntryPointOffset { get; set; }

      //public Tuple<Int64, Int32> EmbeddedManifestResourcesInfo { get; set; }

      public DebugInformation DebugInformation { get; set; }

   }

   public class StrongNameVariables
   {
      public Int32 SignatureSize { get; set; }

      //public Int32 SignaturePaddingSize { get; set; }

      public AssemblyHashAlgorithm HashAlgorithm { get; set; }

      public Byte[] PublicKey { get; set; }

      public String ContainerName { get; set; }
   }
}


public static partial class E_CILPhysical
{

   public static ImageInformation WriteMetaDataToStream(
      this Stream stream,
      CILMetaData md,
      WriterFunctionalityProvider writerProvider,
      WritingOptions options,
      StrongNameKeyPair sn,
      Boolean delaySign,
      CryptoCallbacks cryptoCallbacks,
      AssemblyHashAlgorithm? snAlgorithmOverride
      )
   {
      ArgumentValidator.ValidateNotNull( "Stream", stream );
      ArgumentValidator.ValidateNotNull( "Meta data", md );

      CILMetaData newMD; Stream newStream;
      var writer = ( writerProvider ?? new DefaultWriterFunctionalityProvider() ).GetFunctionality( md, options, out newMD, out newStream );
      if ( newMD != null )
      {
         md = newMD;
      }

      ImageInformation retVal;
      if ( newStream == null )
      {
         retVal = stream.WriteMetaDataToStream( md, writer, options, sn, delaySign, cryptoCallbacks, snAlgorithmOverride );
      }
      else
      {
         using ( newStream )
         {
            retVal = newStream.WriteMetaDataToStream( md, writer, options, sn, delaySign, cryptoCallbacks, snAlgorithmOverride );
            newStream.Position = 0;
            newStream.CopyTo( stream );
         }
      }

      return retVal;
   }

   public static ImageInformation WriteMetaDataToStream(
      this Stream stream,
      CILMetaData md,
      WriterFunctionality writer,
      WritingOptions options,
      StrongNameKeyPair sn,
      Boolean delaySign,
      CryptoCallbacks cryptoCallbacks,
      AssemblyHashAlgorithm? snAlgorithmOverride
      )
   {
      // Check arguments
      ArgumentValidator.ValidateNotNull( "Stream", stream );
      ArgumentValidator.ValidateNotNull( "Meta data", md );

      var cf = CollectionsWithRoles.Implementation.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY;

      if ( options == null )
      {
         options = new WritingOptions();
      }

      if ( writer == null )
      {
         writer = new DefaultWriterFunctionality( md, options );
      }

      // Prepare strong name
      RSAParameters rParams;
      var snVars = md.PrepareStrongNameVariables( sn, delaySign, cryptoCallbacks, snAlgorithmOverride, out rParams );

      // 1. Create streams
      var mdStreams = writer.CreateStreamHandlers().ToArrayProxy().CQ;
      var tblMDStream = mdStreams
         .OfType<WriterTableStreamHandler>()
         .FirstOrDefault() ?? new DefaultWriterTableStreamHandler( md, options.CLIOptions.TablesStreamOptions, DefaultMetaDataSerializationSupportProvider.Instance.CreateTableSerializationInfos().ToArrayProxy().CQ );

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

      // 2. Populate streams
      var array = new ResizableArray<Byte>();
      MetaDataTableStreamHeader thHeader;
      tblMDStream.FillHeaps( snVars?.PublicKey?.ToArrayProxy()?.CQ, mdStreamContainer, array, out thHeader );

      // 3. Create WritingStatus (TODO maybe let writer create it?)
      var peOptions = options.PEOptions;
      var fAlign = peOptions.FileAlignment ?? 0x200;
      var machine = peOptions.Machine ?? ImageFileMachine.I386;
      var sectionsArray = new SectionHeader[writer.GetSectionCount( machine )];
      var status = new WritingStatus(
         0x80 // DOS header size
         + 0x04 // PE Signature
         + 0x18 // File header size
         + machine.GetOptionalHeaderSize() // Optional header size
         + sectionsArray.Length * 0x28 // Sections
         ,
         machine,
         fAlign,
         peOptions.ImageBase,
         snVars,
         peOptions.NumberOfDataDirectories
         );

      // 4. Create sections and some headers
      status.SectionAlignment = peOptions.SectionAlignment ?? 0x2000;
      RVAConverter rvaConverter; MetaDataRoot mdRoot; Int32 mdRootSize; Int32 mdSize;
      var rawValueProvider = writer.PopulateSections(
         status,
         mdStreamContainer,
         mdStreams,
         sectionsArray,
         out rvaConverter,
         out mdRoot,
         out mdRootSize,
         out mdSize
         );

      // 5. Position stream after headers, and write whatever is needed before meta data
      var sections = cf.NewArrayProxy( sectionsArray ).CQ;
      CLIHeader cliHeader;
      var headersSize = status.HeadersSize;
      stream.Position = headersSize;
      writer.BeforeMetaData( stream, array, sections, status, rvaConverter, mdRoot, out cliHeader );

      if ( cliHeader == null )
      {
         throw new InvalidOperationException( "Writer failed to create CLI header." );
      }

      // 6. Write meta data
      stream.SeekFromBegin( rvaConverter.ToOffset( (UInt32) cliHeader.MetaData.RVA ) );
      array.CurrentMaxCapacity = mdRootSize;
      writer.WriteMDRoot( mdRoot, array );
      stream.Write( array.Array, mdRootSize );
      foreach ( var mds in mdStreams.Where( mds => mds.Accessed ) )
      {
         mds.WriteStream( stream, array, rawValueProvider, rvaConverter );
      }

      // 7. Write whatever is needed after meta data
      writer.AfterMetaData( stream, array, sections, status, rvaConverter );

      // 8. Create and write image information
      var cliOptions = options.CLIOptions;
      var snSignature = new Byte[snVars?.SignatureSize ?? 0];
      var cliHeaderOptions = cliOptions.HeaderOptions;
      var thOptions = cliOptions.TablesStreamOptions;
      var peStartOffset = 0x00000080u;
      var imageInfo = new ImageInformation(
         new PEInformation(
            new DOSHeader( 0x5A4D, peStartOffset ),
            new NTHeader( 0x00004550,
               new FileHeader(
                  machine, // Machine
                  (UInt16) sections.Count, // Number of sections
                  (UInt32) ( peOptions.Timestamp ?? CreateNewPETimestamp() ), // Timestamp
                  0, // Pointer to symbol table
                  0, // Number of symbols
                  (UInt16) machine.GetOptionalHeaderSize(),
                  ( peOptions.Characteristics ?? machine.GetDefaultCharacteristics() ).ProcessCharacteristics( options.IsExecutable )
                  ),
               machine.CreateOptionalHeader(
                  peOptions,
                  status,
                  rvaConverter,
                  sections,
                  (UInt32) headersSize
                  )
               ),
            sections
            ),
         status.DebugInformation,
         new CLIInformation(
            cliHeader,
            mdRoot,
            thHeader,
            cf.NewArrayProxy( snSignature ).CQ,
            null,
            null
            )
         );
      writer.WritePEInformation( stream, array, status, imageInfo.PEInformation );

      // 9. Compute strong name signature, if needed
      CreateStrongNameSignature( stream, snVars, delaySign, cryptoCallbacks, rParams, cliHeader, rvaConverter, snSignature );

      return imageInfo;
   }

   private static StrongNameVariables PrepareStrongNameVariables(
      this CILMetaData md,
      StrongNameKeyPair strongName,
      Boolean delaySign,
      CryptoCallbacks cryptoCallbacks,
      AssemblyHashAlgorithm? algoOverride,
      out RSAParameters rParams
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
         throw new InvalidOperationException( "Assembly should be strong-named, but the crypto callbacks are not provided." );
      }

      StrongNameVariables retVal;

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
         var containerName = strongName?.ContainerName;
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

         retVal = new StrongNameVariables()
         {
            HashAlgorithm = signingAlgorithm,
            PublicKey = thisAssemblyPublicKey,
            SignatureSize = snSize,
            ContainerName = containerName
         };
      }
      else
      {
         retVal = null;
         rParams = default( RSAParameters );
      }

      return retVal;
   }

   private static void CreateStrongNameSignature(
      Stream stream,
      StrongNameVariables snVars,
      Boolean delaySign,
      CryptoCallbacks cryptoCallbacks,
      RSAParameters rParams,
      CLIHeader cliHeader,
      RVAConverter rvaConverter,
      Byte[] snSignatureArray
      )
   {
      if ( snVars != null && !delaySign )
      {
         var containerName = snVars.ContainerName;
         using ( var rsa = ( containerName == null ? cryptoCallbacks.CreateRSAFromParameters( rParams ) : cryptoCallbacks.CreateRSAFromCSPContainer( containerName ) ) )
         {
            var algo = snVars.HashAlgorithm;
            var snSize = snVars.SignatureSize;
            var buffer = new Byte[0x2000]; // 2x typical windows page size
            var hashEvtArgs = cryptoCallbacks.CreateHashStreamAndCheck( algo, true, true, false, true );
            var hashStream = hashEvtArgs.CryptoStream;
            var hashGetter = hashEvtArgs.HashGetter;
            var transform = hashEvtArgs.Transform;
            var sigOffset = rvaConverter.ToOffset( cliHeader.StrongNameSignature.RVA );

            Byte[] strongNameArray;
            using ( var tf = transform )
            {
               using ( var cryptoStream = hashStream() )
               {
                  // Calculate hash of required parts of file (ECMA-335, p.117)
                  // TODO: Skip Certificate Table and PE Header File Checksum fields
                  stream.Seek( 0, SeekOrigin.Begin );
                  stream.CopyStreamPart( cryptoStream, buffer, sigOffset );

                  stream.Seek( snSize, SeekOrigin.Current );
                  stream.CopyStream( cryptoStream, buffer );
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
            var idx = 0;
            snSignatureArray.BlockCopyFrom( ref idx, strongNameArray );
         }
      }
   }


   public static Boolean IsWide( this AbstractWriterStreamHandler stream )
   {
      return stream.CurrentSize > UInt16.MaxValue;
   }

   internal static ArrayQuery<Byte> CreateASCIIBytes( this String str, Int32 align, Int32 minLen = 0, Int32 maxLen = -1 )
   {
      Byte[] bytez;
      if ( String.IsNullOrEmpty( str ) )
      {
         bytez = new Byte[Math.Max( align, minLen )];
      }
      else
      {
         var byteArrayLen = ( str.Length + 1 ).RoundUpI32( align );
         bytez = new Byte[maxLen >= 0 ? Math.Max( maxLen, byteArrayLen ) : byteArrayLen];
         var idx = 0;
         while ( idx < bytez.Length && idx < str.Length )
         {
            bytez[idx] = (Byte) str[idx];
            ++idx;
         }
      }
      return CollectionsWithRoles.Implementation.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( bytez ).CQ;
   }

   private static Int32 CreateNewPETimestamp()
   {
      return (Int32) ( DateTime.UtcNow - new DateTime( 1870, 1, 1, 0, 0, 0, DateTimeKind.Utc ) ).TotalSeconds;
   }

   private static UInt16 GetOptionalHeaderSize( this ImageFileMachine machine )
   {
      return (UInt16) ( machine.RequiresPE64() ?
         0xF0 :
         0xE0 );
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
      this ImageFileMachine machine,
      WritingOptions_PE options,
      WritingStatus writingStatus,
      RVAConverter rvaConverter,
      ArrayQuery<SectionHeader> sections,
      UInt32 headersSize
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
      const Subsystem subsystem = Subsystem.WindowsGUI;
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
         var isCode = chars.HasFlag( SectionHeaderCharacteristics.Contains_Code );
         var isInitData = chars.HasFlag( SectionHeaderCharacteristics.Contains_InitializedData );
         var isUninitData = chars.HasFlag( SectionHeaderCharacteristics.Contains_UninitializedData );
         var curSize = section.RawDataSize;

         if ( isCode )
         {
            if ( codeBase == 0u )
            {
               codeBase = imageSize;
            }
            codeSize += curSize;
         }
         if ( isInitData )
         {
            if ( dataBase == 0u )
            {
               dataBase = imageSize;
            }
            initDataSize += curSize;
         }
         if ( isUninitData )
         {
            if ( dataBase == 0u )
            {
               dataBase = imageSize;
            }
            uninitDataSize += curSize;
         }

         imageSize += curSize.RoundUpU32( sAlign );
      }

      var ep = rvaConverter.ToRVANullable( writingStatus.EntryPointOffset );
      var imageBase = writingStatus.ImageBase;

      if ( machine.RequiresPE64() )
      {
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
      }
      else
      {
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
}