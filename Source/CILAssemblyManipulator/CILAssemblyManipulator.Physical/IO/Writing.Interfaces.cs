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

      RawValueStorage<Int64> CreateRawValuesBeforeMDStreams(
         Stream stream,
         ResizableArray<Byte> array,
         WriterMetaDataStreamContainer mdStreams,
         WritingStatus writingStatus
         );

      IEnumerable<SectionHeader> CreateSections(
         WritingStatus writingStatus,
         IEnumerable<AbstractWriterStreamHandler> allStreams,
         out RVAConverter rvaConverter
         );

      void FinalizeStream(
         Stream stream,
         ArrayQuery<SectionHeader> sections,
         WritingStatus writingStatus
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
         RawValueStorage<Int64> rawValuesBeforeStreams,
         RVAConverter rvaConverter
         );

      Int32 CurrentSize { get; }

      Boolean Accessed { get; }
   }

   public interface WriterTableStreamHandler : AbstractWriterStreamHandler
   {
      RawValueStorage<Int32> FillHeaps(
         RawValueStorage<Int64> rawValuesBeforeStreams,
         ArrayQuery<Byte> thisAssemblyPublicKeyIfPresentNull,
         WriterMetaDataStreamContainer mdStreams,
         ResizableArray<Byte> array
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
         Int32 initialOffset,
         ImageFileMachine machine,
         Int32 fileAlignment,
         StrongNameVariables strongNameVariables
         )
      {
         this.InitialOffset = initialOffset;
         this.Machine = machine;
         this.FileAlignment = fileAlignment;
         this.StrongNameVariables = strongNameVariables;
         this.PEDataDirectories = new List<DataDirectory>( Enumerable.Repeat<DataDirectory>( default( DataDirectory ), (Int32) DataDirectories.MaxValue ) );
      }

      public Int32 InitialOffset { get; }

      public ImageFileMachine Machine { get; }

      public Int32 FileAlignment { get; }

      public StrongNameVariables StrongNameVariables { get; }

      public List<DataDirectory> PEDataDirectories { get; }

      public Int64? OffsetAfterInitialRawValues { get; set; }

      public Int64? StrongNameSignatureOffset { get; set; }

      public DataDirectory? MetaData { get; set; }

      public DataDirectory? ManifestResources { get; set; }

      //public DataDirectory? StrongNameSignature { get; set; }

      public DataDirectory? CodeManagerTable { get; set; }

      public DataDirectory? VTableFixups { get; set; }

      public DataDirectory? ExportAddressTableJumps { get; set; }

      public DataDirectory? ManagedNativeHeader { get; set; }

   }

   public class StrongNameVariables
   {
      public Int32 SignatureSize { get; set; }

      public Int32 SignaturePaddingSize { get; set; }

      public AssemblyHashAlgorithm HashAlgorithm { get; set; }

      public Byte[] PublicKey { get; set; }

      public String ContainerName { get; set; }
   }
}


public static partial class E_CILPhysical
{

   public static ImageInformation WriteMetaDataFromStream(
      this Stream stream,
      CILMetaData md,
      WriterFunctionalityProvider writerProvider,
      WritingOptions options
      )
   {
   }

   public static ImageInformation WriteMetaDataFromStream(
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

      // 2. Position stream at file alignment, and write raw values (IL, constants, resources, relocs, etc)
      var peOptions = options.PEOptions;
      var initialOffset = peOptions.FileAlignment ?? 0x200;
      stream.Position = initialOffset;
      var machine = peOptions.Machine ?? ImageFileMachine.I386;

      var status = new WritingStatus( initialOffset, machine, initialOffset, snVars );
      var array = new ResizableArray<Byte>();
      var rawValues = writer.CreateRawValuesBeforeMDStreams( stream, array, mdStreamContainer, status );
      status.OffsetAfterInitialRawValues = stream.Position;

      // 3. Populate heaps
      tblMDStream.FillHeaps( rawValues, null, mdStreamContainer, array );

      // 4. Create sections
      RVAConverter rvaConverter;
      var sections = writer.CreateSections( status, mdStreams, out rvaConverter ).ToArrayProxy().CQ;

      // 5. Write meta data
      foreach ( var mdStream in mdStreams )
      {
         mdStream.WriteStream( stream, array, rawValues, rvaConverter );
      }

      // 6. Finalize writing status
      writer.FinalizeStream( stream, sections, status );

      // 7. Create and write image information

      // 8. Compute strong name signature, if needed
      Byte[] snSignature;
      if ( CreateStrongNameSignature( stream, snVars, delaySign, cryptoCallbacks, rParams, status, out snSignature ) )
      {
         stream.Seek( status.StrongNameSignatureOffset.Value, SeekOrigin.Begin );
         stream.Write( snSignature );
      }
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
            // TODO investigate this.
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
            SignaturePaddingSize = BitUtils.MultipleOf4( snSize ) - snSize,
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

   private static Boolean CreateStrongNameSignature(
      Stream stream,
      StrongNameVariables snVars,
      Boolean delaySign,
      CryptoCallbacks cryptoCallbacks,
      RSAParameters rParams,
      WritingStatus writingStatus,
      out Byte[] snSignature
      )
   {
      if ( snVars != null )
      {
         if ( delaySign )
         {
            snSignature = new Byte[snVars.SignatureSize + snVars.SignaturePaddingSize];
         }
         else
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
               var sigOffset = writingStatus.StrongNameSignatureOffset.Value;

               Byte[] strongNameArray;
               using ( var tf = transform )
               {
                  using ( var cryptoStream = hashStream() )
                  {
                     // Calculate hash of required parts of file (ECMA-335, p.117)
                     // TODO: Skip Certificate Table and PE Header File Checksum fields
                     stream.Seek( 0, SeekOrigin.Begin );
                     stream.CopyStreamPart( cryptoStream, buffer, sigOffset );

                     stream.Seek( snSize + snVars.SignaturePaddingSize, SeekOrigin.Current );
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
               stream.Seek( writingStatus.StrongNameSignatureOffset.Value, SeekOrigin.Begin );
               stream.Write( strongNameArray );
               snSignature = strongNameArray;
            }
         }
      }
      else
      {
         snSignature = null;
      }

      return snSignature != null;
   }


   public static Boolean IsWide( this AbstractWriterStreamHandler stream )
   {
      return stream.CurrentSize > UInt16.MaxValue;
   }
}