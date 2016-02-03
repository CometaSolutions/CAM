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
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;

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

namespace CILAssemblyManipulator.Physical.IO
{
   /// <summary>
   /// This interface is the 'root' interface which controls what happens when writing <see cref="CILMetaData"/> with <see cref="E_CILPhysical.WriteModule"/> method.
   /// To customize the serialization process, it is possible to set <see cref="WritingArguments.WriterFunctionalityProvider"/> property to your own instances of <see cref="WriterFunctionalityProvider"/>.
   /// </summary>
   /// <seealso cref="WriterFunctionality"/>
   /// <seealso cref="WritingArguments.ReaderFunctionalityProvider"/>
   /// <seealso cref="E_CILPhysical.WriteMetaDataToStream(WriterFunctionalityProvider, Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/>
   /// <seealso cref="DefaultWriterFunctionalityProvider"/>
   public interface WriterFunctionalityProvider
   {
      /// <summary>
      /// Creates a new <see cref="WriterFunctionality"/> to be used to write <see cref="CILMetaData"/> to <see cref="Stream"/>.
      /// Optionally, specifies a new <see cref="Stream"/> and/or a new <see cref="CILMetaData"/> to use.
      /// </summary>
      /// <param name="md">The original <see cref="CILMetaData"/>.</param>
      /// <param name="options">The <see cref="WritingOptions"/> being used.</param>
      /// <param name="errorHandler">The error handler callback.</param>
      /// <param name="newMD">Optional new <see cref="CILMetaData"/> to use instead of <paramref name="md"/> in the further serialization process.</param>
      /// <param name="newStream">Optional new <see cref="Stream"/> to use instead of <paramref name="stream"/> in the further sererialization process.</param>
      /// <returns>The <see cref="WriterFunctionality"/> to use for actual deserialization.</returns>
      /// <seealso cref="WriterFunctionality"/>
      /// <seealso cref="IOArguments.ErrorHandler"/>
      /// <seealso cref="E_CILPhysical.WriteMetaDataToStream(WriterFunctionality, Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/>
      WriterFunctionality GetFunctionality(
         CILMetaData md,
         WritingOptions options,
         EventHandler<SerializationErrorEventArgs> errorHandler,
         out CILMetaData newMD,
         out Stream newStream
         );
   }

   /// <summary>
   /// This interface provides core functionality to be used when serializing <see cref="CILMetaData"/> to <see cref="Stream"/>.
   /// The instances of this interface are created via <see cref="WriterFunctionalityProvider.GetFunctionality"/> method, and the instances of <see cref="WriterFunctionalityProvider"/> may be customized by setting <see cref="WritingArguments.WriterFunctionalityProvider"/> property.
   /// </summary>
   /// <remarks>
   /// The <see cref="E_CILPhysical.WriteMetaDataToStream(WriterFunctionality, Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/> method will call the methods of this interface in the following order:
   /// <list type="number">
   /// <item><description><see cref="CreateMetaDataStreamHandlers"/>,</description></item>
   /// <item><description><see cref="GetSectionCount"/>,</description></item>
   /// <item><description><see cref="PopulateSections"/>,</description></item>
   /// <item><description><see cref="BeforeMetaData"/>,</description></item>
   /// <item><description><see cref="WriteMDRoot"/>,</description></item>
   /// <item><description><see cref="AfterMetaData"/>, and</description></item>
   /// <item><description><see cref="WritePEInformation"/>.</description></item>
   /// </list>
   /// </remarks>
   /// <seealso cref="WriterFunctionalityProvider"/>
   /// <seealso cref="DefaultWriterFunctionality"/>
   /// <seealso cref="WriterFunctionalityProvider.GetFunctionality"/>
   /// <seealso cref="E_CILPhysical.WriteMetaDataToStream(WriterFunctionality, Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/>
   /// <seealso cref="WritingArguments.WriterFunctionalityProvider"/>
   public interface WriterFunctionality
   {
      IEnumerable<AbstractWriterStreamHandler> CreateMetaDataStreamHandlers();

      Int32 GetSectionCount( ImageFileMachine machine );

      RawValueStorage<Int64> PopulateSections(
         WritingStatus writingStatus,
         WriterMetaDataStreamContainer mdStreamContainer,
         ArrayQuery<AbstractWriterStreamHandler> allMDStreams,
         SectionHeader[] sections,
         out RVAConverter rvaConverter,
         out MetaDataRoot mdRoot,
         out Int32 mdRootSize
         );

      void BeforeMetaData(
         WritingStatus writingStatus,
         Stream stream,
         ResizableArray<Byte> array,
         ArrayQuery<SectionHeader> sections,
         MetaDataRoot mdRoot,
         out CLIHeader cliHeader
         );

      void WriteMDRoot(
         MetaDataRoot mdRoot,
         ResizableArray<Byte> array
         );

      void AfterMetaData(
         WritingStatus writingStatus,
         Stream stream,
         ResizableArray<Byte> array,
         ArrayQuery<SectionHeader> sections
         );

      void WritePEInformation(
         WritingStatus writingStatus,
         Stream stream,
         ResizableArray<Byte> array,
         PEInformation peInfo
         );
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
         RawValueStorage<Int64> rawValueProvder
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

   public class WritingStatus
   {
      public const Int32 DEFAULT_FILE_ALIGNMENT = 0x200;
      public const Int32 DEFAULT_SECTION_ALIGNMENT = 0x2000;

      public WritingStatus(
         Int32 headersSize,
         ImageFileMachine machine,
         Int32 fileAlignment,
         Int32 sectionAlignment,
         Int64? imageBase,
         StrongNameVariables strongNameVariables,
         Int32 dataDirCount
         )
      {
         fileAlignment = CheckAlignment( fileAlignment, DEFAULT_FILE_ALIGNMENT );
         sectionAlignment = CheckAlignment( sectionAlignment, DEFAULT_SECTION_ALIGNMENT );
         this.HeadersSizeUnaligned = headersSize;
         this.HeadersSize = headersSize.RoundUpI32( fileAlignment );
         this.Machine = machine;
         this.FileAlignment = fileAlignment;
         this.SectionAlignment = sectionAlignment;
         this.ImageBase = imageBase ?? ( machine.RequiresPE64() ? 0x0000000140000000 : 0x0000000000400000 );
         this.StrongNameVariables = strongNameVariables;
         this.PEDataDirectories = Enumerable.Repeat<DataDirectory>( default( DataDirectory ), dataDirCount ).ToArrayProxy();
      }

      public Int32 HeadersSize { get; }

      public Int32 HeadersSizeUnaligned { get; }

      public ImageFileMachine Machine { get; }

      public Int32 FileAlignment { get; }

      public Int64 ImageBase { get; }

      public Int32 SectionAlignment { get; }

      public StrongNameVariables StrongNameVariables { get; }

      public ArrayProxy<DataDirectory> PEDataDirectories { get; }

      public Int32? EntryPointRVA { get; set; }

      public DebugInformation DebugInformation { get; set; }

      public static Int32 CheckAlignment( Int32 alignment, Int32 defaultAlignment )
      {
         // TODO reset all bits following MSB set bit in alignment
         return alignment == 0 ? defaultAlignment : alignment;
      }
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
   private const Int32 PE_SIG_AND_FILE_HEADER_SIZE = 0x18; // PE signature + file header
   private const Int32 DATA_DIR_SIZE = 0x08;

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

      var cf = CollectionsWithRoles.Implementation.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY;

      if ( options == null )
      {
         options = new WritingOptions();
      }

      // Prepare strong name
      RSAParameters rParams;
      var snVars = md.PrepareStrongNameVariables( sn, ref delaySign, cryptoCallbacks, snAlgorithmOverride, out rParams );

      // 1. Create streams
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

      // 2. Populate streams
      var array = new ResizableArray<Byte>( initialSize: 0x1000 );
      MetaDataTableStreamHeader thHeader;
      tblMDStream.FillHeaps( snVars?.PublicKey?.ToArrayProxy()?.CQ, mdStreamContainer, array, out thHeader );

      // 3. Create WritingStatus (TODO maybe let writer create it?)
      var peOptions = options.PEOptions;
      var machine = peOptions.Machine ?? ImageFileMachine.I386;
      var sectionsArray = new SectionHeader[writer.GetSectionCount( machine )];

      var peDataDirCount = peOptions.NumberOfDataDirectories ?? (Int32) DataDirectories.MaxValue;
      var optionalHeaderKind = machine.GetOptionalHeaderKind();
      var optionalHeaderSize = optionalHeaderKind.GetOptionalHeaderSize( peDataDirCount );
      var status = new WritingStatus(
         0x80 // DOS header size
         + PE_SIG_AND_FILE_HEADER_SIZE // PE Signature + File header size
         + optionalHeaderSize // Optional header size
         + sectionsArray.Length * 0x28 // Sections
         ,
         machine,
         peOptions.FileAlignment ?? WritingStatus.DEFAULT_FILE_ALIGNMENT,
         peOptions.SectionAlignment ?? WritingStatus.DEFAULT_SECTION_ALIGNMENT,
         peOptions.ImageBase,
         snVars,
         peDataDirCount
         );

      // 4. Create sections and some headers
      RVAConverter rvaConverter; MetaDataRoot mdRoot; Int32 mdRootSize;
      var rawValueProvider = writer.PopulateSections(
         status,
         mdStreamContainer,
         mdStreams,
         sectionsArray,
         out rvaConverter,
         out mdRoot,
         out mdRootSize
         );

      // 5. Position stream after headers, and write whatever is needed before meta data
      var sections = cf.NewArrayProxy( sectionsArray ).CQ;
      CLIHeader cliHeader;
      var headersSize = status.HeadersSize;
      stream.Position = headersSize;
      writer.BeforeMetaData( status, stream, array, sections, mdRoot, out cliHeader );

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
         mds.WriteStream( stream, array, rawValueProvider );
      }

      // 7. Write whatever is needed after meta data
      writer.AfterMetaData( status, stream, array, sections );

      // 8. Create and write image information
      var cliOptions = options.CLIOptions;
      var snSignature = snVars == null ? null : new Byte[snVars.SignatureSize];
      var cliHeaderOptions = cliOptions.HeaderOptions;
      var thOptions = cliOptions.TablesStreamOptions;
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
                  optionalHeaderSize,
                  ( peOptions.Characteristics ?? machine.GetDefaultCharacteristics() ).ProcessCharacteristics( options.IsExecutable )
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
            mdRoot,
            thHeader,
            snSignature == null ? null : cf.NewArrayProxy( snSignature ).CQ,
            rawValueProvider.GetAllRawValuesForColumn( Tables.MethodDef, 0 ).Select( r => (UInt32) r ).ToArrayProxy().CQ,
            rawValueProvider.GetAllRawValuesForColumn( Tables.FieldRVA, 0 ).Select( r => (UInt32) r ).ToArrayProxy().CQ
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
         cliHeader,
         rvaConverter,
         snSignature,
         imageInfo.PEInformation,
         status.HeadersSizeUnaligned
         );

      return imageInfo;
   }

   private static StrongNameVariables PrepareStrongNameVariables(
      this CILMetaData md,
      StrongNameKeyPair strongName,
      ref Boolean delaySign,
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
#if CAM_PHYSICAL_IS_PORTABLE
         throw new ArgumentException( "Assembly strong name was provided, but the crypto callbacks were not." );
#else
         cryptoCallbacks = new CryptoCallbacksDotNET();
#endif
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
      Byte[] snSignatureArray,
      PEInformation imageInfo,
      Int32 headersSizeUnaligned
      )
   {
      if ( snVars != null && !delaySign )
      {
         var containerName = snVars.ContainerName;
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
                  // Calculate hash of required parts of file (ECMA-335, p.117)
                  // Read all headers first DOS header (start of file to the NT headers)
                  stream.SeekFromBegin( 0 );
                  var hdrArray = new Byte[headersSizeUnaligned];
                  stream.ReadSpecificAmount( hdrArray, 0, hdrArray.Length );

                  // Hash the checksum entry + authenticode as zeroes
                  const Int32 peCheckSumOffsetWithinOptionalHeader = 0x40;
                  var ntHeaderStart = (Int32) imageInfo.DOSHeader.NTHeaderOffset;
                  idx = ntHeaderStart
                     + PE_SIG_AND_FILE_HEADER_SIZE // NT header signature + file header size
                     + peCheckSumOffsetWithinOptionalHeader; // Offset of PE checksum entry.
                  hdrArray.WriteInt32LEToBytes( ref idx, 0 );

                  var optionalHeaderSizeWithoutDataDirs = imageInfo.NTHeader.FileHeader.OptionalHeaderSize - DATA_DIR_SIZE * imageInfo.NTHeader.OptionalHeader.DataDirectories.Count;

                  idx = ntHeaderStart
                     + PE_SIG_AND_FILE_HEADER_SIZE // NT header signature + file header size
                     + optionalHeaderSizeWithoutDataDirs
                     + 4 * DATA_DIR_SIZE; // Authenticode is 5th data directory, and optionalHeaderSize includes all data directories
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
      return (Int32) ( DateTime.UtcNow - new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc ) ).TotalSeconds;
   }

   private static UInt16 GetOptionalHeaderSize( this OptionalHeaderKind kind, Int32 peDataDirectoriesCount )
   {
      return (UInt16) ( ( kind == OptionalHeaderKind.Optional64 ? 0x70 : 0x60 ) + DATA_DIR_SIZE * peDataDirectoriesCount );
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
               writingStatus.PEDataDirectories.CQ.ToArrayProxy().CQ
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
               writingStatus.PEDataDirectories.CQ.ToArrayProxy().CQ
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

   public static IEnumerable<AbstractWriterStreamHandler> GetAllStreams( this WriterMetaDataStreamContainer mdStreams )
   {
      yield return mdStreams.SystemStrings;
      yield return mdStreams.BLOBs;
      yield return mdStreams.GUIDs;
      yield return mdStreams.UserStrings;
      foreach ( var os in mdStreams.OtherStreams )
      {
         yield return os;
      }
   }
}