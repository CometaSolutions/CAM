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
         out RVAConverter rvaConverter
         );

      void FinalizeWritingStatus(
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

      /// <summary>
      /// This should be max UInt32.Value
      /// </summary>
      Int64 CurrentSize { get; }

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
      public WritingStatus( StrongNameVariables strongNameVariables )
      {
         this.StrongNameVariables = strongNameVariables;
         this.PEDataDirectories = new List<DataDirectory>( Enumerable.Repeat<DataDirectory>( default( DataDirectory ), (Int32) DataDirectories.MaxValue ) );
      }

      public StrongNameVariables StrongNameVariables { get; }

      public List<DataDirectory> PEDataDirectories { get; }

      public DataDirectory? MetaData { get; set; }

      public DataDirectory? ManifestResources { get; set; }

      public DataDirectory? StrongNameSignature { get; set; }

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
      stream.Position = peOptions?.FileAlignment ?? 0x200;
      var machine = peOptions?.Machine ?? ImageFileMachine.I386;

      var status = new WritingStatus( snVars );
      var array = new ResizableArray<Byte>();
      var rawValues = writer.CreateRawValuesBeforeMDStreams( stream, array, mdStreamContainer, status );

      // 3. Populate heaps
      tblMDStream.FillHeaps( rawValues, null, mdStreamContainer, array );

      // 4. Create sections
      RVAConverter rvaConverter;
      var sections = writer.CreateSections( status, out rvaConverter ).ToArray();

      // 5. Write meta data
      foreach ( var mdStream in mdStreams )
      {
         mdStream.WriteStream( stream, array, rawValues, rvaConverter );
      }

      // 6. Finalize writing status
      writer.FinalizeWritingStatus( status );

      // Create image information
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
            SignaturePaddingSize = BitUtils.MultipleOf4( snSize ) - snSize
         };
      }
      else
      {
         retVal = null;
         rParams = default( RSAParameters );
      }

      return retVal;
   }

   public static Boolean IsWide( this AbstractWriterStreamHandler stream )
   {
      return stream.CurrentSize > UInt16.MaxValue;
   }
}