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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtils;
using System.Threading;
using System.IO;
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.Meta;
using CILAssemblyManipulator.Physical.IO;
using CILAssemblyManipulator.Physical.IO.Defaults;
using CollectionsWithRoles.API;
using TabularMetaData;

namespace CILAssemblyManipulator.Physical.IO.Defaults
{

   using TRVA = Int64;

   /// <summary>
   /// This class provides default implementation for <see cref="WriterFunctionalityProvider"/>.
   /// </summary>
   public class DefaultWriterFunctionalityProvider : WriterFunctionalityProvider
   {
      /// <summary>
      /// This method implements <see cref="WriterFunctionalityProvider.GetFunctionality"/>, and will return <see cref="DefaultWriterFunctionality"/>.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/>.</param>
      /// <param name="options">The <see cref="WritingOptions"/>.</param>
      /// <param name="errorHandler">The error handler callback.</param>
      /// <param name="newMD">This will be <c>null</c>.</param>
      /// <param name="newStream">This will be <c>null</c>.</param>
      /// <returns>A new instance of <see cref="DefaultWriterFunctionality"/>.</returns>
      public virtual WriterFunctionality GetFunctionality(
         CILMetaData md,
         WritingOptions options,
         EventHandler<SerializationErrorEventArgs> errorHandler,
         out CILMetaData newMD,
         out Stream newStream
         )
      {
         newMD = null;
         newStream = null;
         return new DefaultWriterFunctionality( md, options, new TableSerializationLogicalFunctionalityCreationArgs( errorHandler ) );
      }
   }

   /// <summary>
   /// This class provides default implementation for <see cref="WriterFunctionality"/>.
   /// </summary>
   public class DefaultWriterFunctionality : WriterFunctionality
   {

      /// <summary>
      /// Creates a new instance of <see cref="DefaultWriterFunctionality"/> with given parameters.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/>.</param>
      /// <param name="options">The <see cref="CAMPhysicalIO::CILAssemblyManipulator.Physical.IO.WritingOptions"/>.</param>
      /// <param name="serializationCreationArgs">The <see cref="TableSerializationLogicalFunctionalityCreationArgs"/> for table stream.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="md"/> is <c>null</c>.</exception>
      public DefaultWriterFunctionality(
         CILMetaData md,
         WritingOptions options,
         TableSerializationLogicalFunctionalityCreationArgs serializationCreationArgs
         )
      {
         ArgumentValidator.ValidateNotNull( "Meta data", md );

         this.MetaData = md;
         this.WritingOptions = options ?? new WritingOptions();
         this.SerializationCreationArgs = serializationCreationArgs;
      }

      /// <summary>
      /// This method implements the <see cref="WriterFunctionality.CreateWritingStatus"/> by delegating creation to <see cref="DoCreateWritingStatus"/> method.
      /// </summary>
      /// <param name="snVars">The <see cref="StrongNameInformation"/>. May be <c>null</c>.</param>
      /// <returns>A new instance of <see cref="DefaultWritingStatus"/>.</returns>
      /// <remarks>
      /// A number of methods will cast given <see cref="WritingStatus"/> into <see cref="DefaultWritingStatus"/>, namely:
      /// <list type="bullet">
      /// <item><description><see cref="CreateMetaDataStreamHandlers"/>,</description></item>
      /// <item><description><see cref="CalculateImageLayout"/>,</description></item>
      /// <item><description><see cref="BeforeMetaData"/>, and</description></item>
      /// <item><description><see cref="AfterMetaData"/>.</description></item>
      /// </list>
      /// </remarks>
      public virtual WritingStatus CreateWritingStatus(
         StrongNameInformation snVars
         )
      {
         var peOptions = this.WritingOptions.PEOptions;
         var machine = peOptions.Machine ?? ImageFileMachine.I386;

         var peDataDirCount = peOptions.NumberOfDataDirectories ?? (Int32) DataDirectories.MaxValue;

         return this.DoCreateWritingStatus(
            machine,
            peOptions.FileAlignment,
            peOptions.SectionAlignment,
            peOptions.ImageBase,
            snVars,
            peDataDirCount
            );
      }

      /// <summary>
      /// This method implements the <see cref="WriterFunctionality.CreateMetaDataStreamHandlers"/> by returning five meta data stream handlers.
      /// </summary>
      /// <param name="status">The <see cref="WritingStatus"/> created by <see cref="CreateWritingStatus"/> method.</param>
      /// <returns>Five meta data stream handlers.</returns>
      /// <remarks>
      /// The returned meta data stream handlers are the following, in this order:
      /// <list type="number">
      /// <item><description><see cref="DefaultWriterTableStreamHandler"/>,</description></item>
      /// <item><description><see cref="DefaultWriterSystemStringStreamHandler"/>,</description></item>
      /// <item><description><see cref="DefaultWriterBLOBStreamHandler"/>,</description></item>
      /// <item><description><see cref="DefaultWriterGuidStreamHandler"/>, and</description></item>
      /// <item><description><see cref="DefaultWriterUserStringStreamHandler"/>.</description></item>
      /// </list>
      /// </remarks>
      public virtual IEnumerable<AbstractWriterStreamHandler> CreateMetaDataStreamHandlers(
         WritingStatus status
         )
      {
         yield return new DefaultWriterTableStreamHandler( this.MetaData, this.WritingOptions.CLIOptions.TablesStreamOptions, this.SerializationCreationArgs, (DefaultWritingStatus) status );
         yield return new DefaultWriterSystemStringStreamHandler();
         yield return new DefaultWriterBLOBStreamHandler();
         yield return new DefaultWriterGuidStreamHandler();
         yield return new DefaultWriterUserStringStreamHandler();
      }

      /// <summary>
      /// This method implements the <see cref="WriterFunctionality.CalculateImageLayout"/>.
      /// </summary>
      /// <param name="writingStatus">The <see cref="WritingStatus"/>, will be casted to <see cref="DefaultWritingStatus"/>.</param>
      /// <param name="presentStreams">The information about present streams.</param>
      /// <param name="rvaConverter">This parameter will hold the <see cref="RVAConverter"/> to use.</param>
      /// <param name="mdRootSize">This parameter will hold the meta data root byte size.</param>
      /// <returns></returns>
      /// <remarks>
      /// This method works as follows.
      /// <list type="number">
      /// <item><description>The <see cref="MetaDataRoot"/> is created with <see cref="CreateMDRoot"/> method, and then the <see cref="WritingStatus.MDRoot"/> property is assigned. Also the <paramref name="mdRootSize"/> is assigned then too.</description></item>
      /// <item><description>The section layout is calculated with <see cref="CreateSectionLayouts"/> method.</description></item>
      /// <item><description>The <see cref="ImageSectionsInfo"/> is created and populated, and <see cref="DefaultWritingStatus.SectionLayouts"/> along with <see cref="WritingStatus.SectionHeaders"/> are assigned.</description></item>
      /// <item><description>The <paramref name="rvaConverter"/> is assigned to the result of <see cref="CreateRVAConverter"/> method.</description></item>
      /// <item><description>The <see cref="CLIHeader"/> is created with <see cref="CreateCLIHeader"/> methodand <see cref="WritingStatus.CLIHeader"/> property is assigned.</description></item>
      /// <item><description>Finally, the <see cref="DataReferencesInfo"/> is created from <see cref="DefaultWritingStatus.DataReferencesStorage"/> by <see cref="E_CILPhysical.CreateDataReferencesInfo"/> method.</description></item>
      /// </list>
      /// </remarks>
      public virtual DataReferencesInfo CalculateImageLayout(
         WritingStatus writingStatus,
         IEnumerable<StreamHandlerInfo> presentStreams,
         out RVAConverter rvaConverter,
         out Int32 mdRootSize
         )
      {
         var dStatus = (DefaultWritingStatus) writingStatus;

         // MetaData
         Int32 mdSize;
         var mdRoot = this.CreateMDRoot( presentStreams, out mdRootSize, out mdSize );
         writingStatus.MDRoot = mdRoot;

         // Sections
         var sectionLayoutInfos = new ImageSectionsInfo( this.CreateSectionLayouts( dStatus, mdRoot, mdSize ) );

         dStatus.SectionLayouts = sectionLayoutInfos;
         var sectionHeaders = sectionLayoutInfos.Sections.Select( s => s.SectionHeader ).ToArray();
         dStatus.SectionHeaders = sectionHeaders;

         // RVA converter
         rvaConverter = this.CreateRVAConverter( sectionHeaders );

         // CLI Header
         writingStatus.CLIHeader = this.CreateCLIHeader( dStatus );

         return dStatus.DataReferencesStorage.CreateDataReferencesInfo( i => i );
      }

      /// <summary>
      /// This method implements the <see cref="WriterFunctionality.BeforeMetaData"/> by calling <see cref="WritePart"/> for each section part for each section layout in <see cref="DefaultWritingStatus.SectionLayouts"/> until it encounters <see cref="SectionPartFunctionality_MetaData"/>.
      /// </summary>
      /// <param name="writingStatus">The <see cref="WritingStatus"/>, will be casted to <see cref="DefaultWritingStatus"/>.</param>
      /// <param name="stream">The stream where to write, positioned after headers.</param>
      /// <param name="array">The auxiliary array to use.</param>
      public virtual void BeforeMetaData(
         WritingStatus writingStatus,
         Stream stream,
         ResizableArray<Byte> array
         )
      {
         var dStatus = (DefaultWritingStatus) writingStatus;
         foreach ( var section in dStatus.SectionLayouts.Sections )
         {
            var parts = section.Parts;
            if ( parts.Count > 0 )
            {
               // Write either whole section, or all parts up until metadata
               var idx = 0;
               foreach ( var partLayout in parts.TakeWhile( p => !( p.Functionality is SectionPartFunctionality_MetaData ) ) )
               {
                  // Write to ResizableArray
                  this.WritePart( partLayout, array, stream, dStatus );
                  ++idx;
               }

               if ( idx < parts.Count )
               {
                  // We encountered the md-part
                  break;
               }
               else
               {
                  // We've written the whole section - pad with zeroes
                  var pad = (Int32) ( stream.Position.RoundUpI64( dStatus.FileAlignment ) - stream.Position );
                  array.CurrentMaxCapacity = pad;
                  idx = 0;
                  array.ZeroOut( ref idx, pad );
                  stream.Write( array.Array, pad );
               }
            }
         }
      }


      /// <summary>
      /// This method implements the <see cref="WriterFunctionality.WriteMDRoot"/> by calling <see cref="CAMPhysicalIO::E_CILPhysical.WriteMetaDataRoot"/> on <see cref="WritingStatus.MDRoot"/>.
      /// </summary>
      /// <param name="writingStatus">The <see cref="WritingStatus"/>.</param>
      /// <param name="array">The auxiliary byte array to use.</param>
      public virtual void WriteMDRoot(
         WritingStatus writingStatus,
         ResizableArray<Byte> array
         )
      {
         // Array capacity set by writing process
         writingStatus.MDRoot.WriteMetaDataRoot( array );
      }

      /// <summary>
      /// This method implements the <see cref="WriterFunctionality.AfterMetaData"/> by calling <see cref="WritePart"/> for each section part for each section layout in <see cref="DefaultWritingStatus.SectionLayouts"/> after first encounter of <see cref="SectionPartFunctionality_MetaData"/>.
      /// </summary>
      /// <param name="writingStatus">The <see cref="WritingStatus"/>, will be casted to <see cref="DefaultWritingStatus"/>.</param>
      /// <param name="stream">The stream where to write, positioned right after meta data.</param>
      /// <param name="array">The auxiliary array to use.</param>
      public virtual void AfterMetaData(
         WritingStatus writingStatus,
         Stream stream,
         ResizableArray<Byte> array
         )
      {
         var dStatus = (DefaultWritingStatus) writingStatus;
         var mdEncountered = false;
         foreach ( var section in dStatus.SectionLayouts.Sections.SkipWhile( s => !s.Parts.Any( p => p.Functionality is SectionPartFunctionality_MetaData ) ) )
         {
            var parts = section.Parts;
            if ( parts.Count > 0 )
            {

               // Write either whole section, or all parts up until metadata
               foreach ( var partLayout in parts )
               {
                  if ( mdEncountered )
                  {
                     this.WritePart( partLayout, array, stream, dStatus );
                  }
                  else
                  {
                     if ( partLayout.Functionality is SectionPartFunctionality_MetaData )
                     {
                        mdEncountered = true;
                     }
                  }
               }

               // We've written the whole section - pad with zeroes
               var pad = (Int32) ( stream.Position.RoundUpI64( writingStatus.FileAlignment ) - stream.Position );
               array.CurrentMaxCapacity = pad;
               var idx = 0;
               array.ZeroOut( ref idx, pad );
               stream.Write( array.Array, pad );
            }
         }
      }

      /// <summary>
      /// This method implements the <see cref="WriterFunctionality.WritePEInformation"/> by calling <see cref="CAMPhysicalIO::E_CILPhysical.WritePEinformation"/> on given <see cref="PEInformation"/>.
      /// </summary>
      /// <param name="writingStatus">The <see cref="WritingStatus"/>.</param>
      /// <param name="stream">The stream to write PE information to.</param>
      /// <param name="array">The auxiliary byte array to use.</param>
      /// <param name="peInfo">The <see cref="PEInformation"/> to write.</param>
      public virtual void WritePEInformation(
         WritingStatus writingStatus,
         Stream stream,
         ResizableArray<Byte> array,
         PEInformation peInfo
         )
      {
         // PE information
         var headersSize = writingStatus.HeadersSizeUnaligned;
         array.CurrentMaxCapacity = headersSize;
         peInfo.WritePEinformation( array.Array );

         stream.Position = 0;
         stream.Write( array.Array, headersSize );
      }

      /// <summary>
      /// Gets the <see cref="CILMetaData"/> being serialized.
      /// </summary>
      /// <value>The <see cref="CILMetaData"/> being serialized.</value>
      protected CILMetaData MetaData { get; }

      /// <summary>
      /// Gets the <see cref="TableSerializationLogicalFunctionalityCreationArgs"/> used by table stream.
      /// </summary>
      /// <value>The <see cref="TableSerializationLogicalFunctionalityCreationArgs"/> used by table stream.</value>
      protected TableSerializationLogicalFunctionalityCreationArgs SerializationCreationArgs { get; }

      /// <summary>
      /// Gets the <see cref="CAMPhysicalIO::CILAssemblyManipulator.Physical.IO.WritingOptions"/> supplied to this <see cref="DefaultWriterFunctionality"/>.
      /// </summary>
      /// <value>The <see cref="CAMPhysicalIO::CILAssemblyManipulator.Physical.IO.WritingOptions"/> supplied to this <see cref="DefaultWriterFunctionality"/>.</value>
      /// <remarks>
      /// This value is never <c>null</c>.
      /// </remarks>
      protected WritingOptions WritingOptions { get; }

      /// <summary>
      /// This method is called by <see cref="CalculateImageLayout"/> after the section layout has been done.
      /// Creates an instance of <see cref="DefaultRVAConverter"/> with given <see cref="SectionHeader"/>s.
      /// </summary>
      /// <param name="headers">All the <see cref="SectionHeader"/>s of the image being emitted.</param>
      /// <returns>An instance of <see cref="DefaultRVAConverter"/>.</returns>
      protected virtual RVAConverter CreateRVAConverter( IEnumerable<SectionHeader> headers )
      {
         return new DefaultRVAConverter( headers );
      }

      /// <summary>
      /// This method is called by <see cref="CalculateImageLayout"/> in order to produce enumerable of <see cref="SectionLayout"/>s, one for each section.
      /// It does so by calling <see cref="CreateSectionDescriptions"/>, and then building <see cref="SectionLayout"/> for each returned <see cref="SectionDescription"/>.
      /// </summary>
      /// <param name="writingStatus">The <see cref="DefaultWritingStatus"/>, with its <see cref="DefaultWritingStatus.DataReferencesStorage"/> and <see cref="DefaultWritingStatus.DataReferencesSectionParts"/> values set by table stream.</param>
      /// <param name="mdRoot">The <see cref="MetaDataRoot"/> created by <see cref="CreateMDRoot"/> method.</param>
      /// <param name="mdSize">The calculated size of the meta data.</param>
      /// <returns>An enumerable of <see cref="SectionLayout"/>s, one for each sections.</returns>
      /// <seealso cref="SectionLayout"/>
      protected virtual IEnumerable<SectionLayout> CreateSectionLayouts(
         DefaultWritingStatus writingStatus,
         MetaDataRoot mdRoot,
         Int32 mdSize
         )
      {
         // It's important to call 'ToArray()' here, so we won't iterate layouts twice (since there is foreach loop later)
         var sectionLayouts = this.CreateSectionDescriptions( writingStatus, mdSize ).ToArray();
         var sectionsCount = sectionLayouts.Length;
         var optionalHeaderSize = writingStatus.Machine.GetOptionalHeaderKind().GetOptionalHeaderSize( writingStatus.PEDataDirectories.Length );
         var headersSize = 0x80 // DOS header size
            + CAMIOInternals.PE_SIG_AND_FILE_HEADER_SIZE // PE Signature + File header size
            + optionalHeaderSize // Optional header size
            + sectionsCount * 0x28; // Sections
         writingStatus.HeadersSizeUnaligned = headersSize;

         var fAlign = writingStatus.FileAlignment;
         var sAlign = (UInt32) writingStatus.SectionAlignment;
         var curPointer = (UInt32) headersSize.GetAlignedHeadersSize( fAlign );
         var curRVA = sAlign;
         foreach ( var layout in sectionLayouts )
         {
            var layoutInfo = new SectionLayout(
               layout,
               curPointer,
               curRVA,
               fAlign,
               writingStatus.DataReferencesStorage
               );
            var hdr = layoutInfo.SectionHeader;
            if ( hdr.VirtualSize > 0 )
            {
               curRVA = ( curRVA + hdr.VirtualSize ).RoundUpU32( sAlign );
               curPointer += hdr.RawDataSize;

               yield return layoutInfo;
            }
         }
      }

      /// <summary>
      /// This method is called by <see cref="CalculateImageLayout"/> in order to create <see cref="MetaDataRoot"/> describing meta data properties.
      /// </summary>
      /// <param name="presentStreams">Description of all meta data streams, that will be present.</param>
      /// <param name="mdRootSize">This parameter should have the size of the <see cref="MetaDataRoot"/>, in bytes.</param>
      /// <param name="mdSize">This parameter should have the size of whole meta data (root, and all streams), in bytes.</param>
      /// <returns>An instance of <see cref="MetaDataRoot"/>.</returns>
      protected virtual MetaDataRoot CreateMDRoot(
         IEnumerable<StreamHandlerInfo> presentStreams,
         out Int32 mdRootSize,
         out Int32 mdSize
         )
      {
         var mdOptions = this.WritingOptions.CLIOptions.MDRootOptions;
         var mdVersionBytes = MetaDataRoot.GetVersionStringBytes( mdOptions.VersionString );
         var streamNamesBytes = presentStreams
            .Select( mds => Tuple.Create( mds.StreamName.CreateASCIIBytes( 4 ), (UInt32) mds.StreamSize ) )
            .ToArray();
         var streamOffset = (UInt32) ( 0x14 + mdVersionBytes.Count + streamNamesBytes.Sum( sh => 0x08 + sh.Item1.Count ) );
         mdRootSize = (Int32) streamOffset;

         var retVal = new MetaDataRoot(
            mdOptions.Signature ?? 0x424A5342,
            (UInt16) ( mdOptions.MajorVersion ?? 0x0001 ),
            (UInt16) ( mdOptions.MinorVersion ?? 0x0001 ),
            mdOptions.Reserved ?? 0x00000000,
            (UInt32) mdVersionBytes.Count,
            mdVersionBytes,
            mdOptions.StorageFlags ?? 0,
            mdOptions.Reserved2 ?? 0,
            (UInt16) streamNamesBytes.Length,
            streamNamesBytes
               .Select( b =>
               {
                  var streamSize = b.Item2;
                  var hdr = new MetaDataStreamHeader( streamOffset, streamSize, b.Item1 );
                  streamOffset += streamSize;
                  return hdr;
               } )
               .ToArrayProxy().CQ
             );
         mdSize = (Int32) streamOffset;
         return retVal;
      }


      /// <summary>
      /// This method is called by <see cref="CreateSectionLayouts"/> in order to produce enumerable of <see cref="SectionDescription"/>s, one for each section.
      /// </summary>
      /// <param name="writingStatus">The <see cref="DefaultWritingStatus"/>.</param>
      /// <param name="mdSize">The size of the meta data, as calculated previously by <see cref="CreateMDRoot"/> method.</param>
      /// <returns>An enumerable of <see cref="SectionDescription"/>s, one for each </returns>
      /// <remarks>
      /// When customizing the sections of the image being emitted, this is the method that should be overridden.
      /// </remarks>
      /// <seealso cref="SectionDescription"/>
      protected virtual IEnumerable<SectionDescription> CreateSectionDescriptions(
         DefaultWritingStatus writingStatus,
         Int32 mdSize
         )
      {
         // 1. Text section
         yield return new SectionDescription( this.GetTextSectionParts( writingStatus, mdSize ) )
         {
            Name = ".text",
            Characteristics = SectionHeaderCharacteristics.Memory_Execute | SectionHeaderCharacteristics.Memory_Read | SectionHeaderCharacteristics.Contains_Code
         };

         // 2. Resource section (TODO)

         // 3. Relocation section
         if ( !writingStatus.Machine.RequiresPE64() )
         {
            yield return new SectionDescription( new SectionPartFunctionality[] { new SectionPartFunctionality_RelocDirectory( writingStatus.Machine ) } )
            {
               Name = ".reloc",
               Characteristics = SectionHeaderCharacteristics.Memory_Read | SectionHeaderCharacteristics.Memory_Discardable | SectionHeaderCharacteristics.Contains_InitializedData
            };
         }
      }

      /// <summary>
      /// This method is called by <see cref="CreateWritingStatus"/>, after doing some processing for parameters, which should be common for all scenarios.
      /// </summary>
      /// <param name="machine">The <see cref="ImageFileMachine"/> of the image being emitted.</param>
      /// <param name="fileAlignment">The file alignment.</param>
      /// <param name="sectionAlignment">The section alignment.</param>
      /// <param name="imageBase">The image base address.</param>
      /// <param name="strongNameVariables">The <see cref="StrongNameInformation"/>, or <c>null</c>.</param>
      /// <param name="dataDirCount">The amount of PE data directories in optional header.</param>
      /// <returns>A new instance of <see cref="DefaultWritingStatus"/>.</returns>
      protected virtual DefaultWritingStatus DoCreateWritingStatus(
         ImageFileMachine machine,
         Int32? fileAlignment,
         Int32? sectionAlignment,
         Int64? imageBase,
         StrongNameInformation strongNameVariables,
         Int32 dataDirCount
         )
      {
         return new DefaultWritingStatus(
            machine,
            fileAlignment,
            sectionAlignment,
            imageBase,
            strongNameVariables,
            dataDirCount
            );
      }

      /// <summary>
      /// This method is called by <see cref="CreateSectionDescriptions"/> when the <see cref="SectionPartFunctionality"/>s for <c>.text</c> section of the image are being collected.
      /// </summary>
      /// <param name="writingStatus">The <see cref="DefaultWritingStatus"/>.</param>
      /// <param name="mdSize">The meta data size, in bytes.</param>
      /// <returns>An enumerable of <see cref="SectionPartFunctionality"/>s making up the <c>.text</c> section.</returns>
      /// <remarks>
      /// <para>
      /// More specifically, the returned <see cref="SectionPartFunctionality"/>s are these, in the following order:
      /// <list type="number">
      /// <item><description><see cref="SectionPartFunctionality_ImportAddressTable"/>,</description></item>
      /// <item><description><see cref="SectionPartFunctionality_CLIHeader"/>,</description></item>
      /// <item><description><see cref="SectionPartFunctionality_StrongNameSignature"/>,</description></item>
      /// <item><description>all <see cref="SectionPartFunctionalityWithDataReferenceTargets"/> in <see cref="DefaultWritingStatus.DataReferencesSectionParts"/>,</description></item>
      /// <item><description><see cref="SectionPartFunctionality_MetaData"/>,</description></item>
      /// <item><description><see cref="SectionPartFunctionality_ImportDirectory"/>,</description></item>
      /// <item><description><see cref="SectionPartFunctionality_StartupCode"/>,</description></item>
      /// <item><description><see cref="SectionPartFunctionality_DebugDirectory"/>,</description></item>
      /// </list>
      /// </para>
      /// <para>
      /// Finally, note that when building <see cref="SectionLayout"/>s in order to get final <see cref="SectionHeader"/>s, the <see cref="SectionPartFunctionality"/>s with zero size are discarded.
      /// So not all <see cref="SectionPartFunctionality"/>s returned by this method are always included in the final image.
      /// </para>
      /// </remarks>
      protected virtual IEnumerable<SectionPartFunctionality> GetTextSectionParts(
         DefaultWritingStatus writingStatus,
         Int32 mdSize
         )
      {
         var options = this.WritingOptions;
         var machine = writingStatus.Machine;

         // 1. IAT
         yield return new SectionPartFunctionality_ImportAddressTable( machine );

         // 2. CLI Header
         yield return new SectionPartFunctionality_CLIHeader();

         // 3. Strong name signature
         yield return new SectionPartFunctionality_StrongNameSignature( writingStatus.StrongNameInformation, machine );

         // 4. Method IL, Field RVAs, Embedded Manifests
         foreach ( var rawValueSectionPart in writingStatus.DataReferencesSectionParts )
         {
            yield return rawValueSectionPart;
         }

         // 5. Meta data
         yield return new SectionPartFunctionality_MetaData( mdSize );

         // 6. Import directory
         var peOptions = options.PEOptions;
         yield return new SectionPartFunctionality_ImportDirectory(
            machine,
            peOptions.ImportHintName,
            peOptions.ImportDirectoryName,
            options.IsExecutable
            );

         // 7. Startup code
         yield return new SectionPartFunctionality_StartupCode( machine );

         // 8. Debug directory (will get filtered away if no debug data)
         yield return new SectionPartFunctionality_DebugDirectory( options.DebugOptions );
      }

      /// <summary>
      /// This method is called by <see cref="BeforeMetaData"/> and <see cref="AfterMetaData"/> for each <see cref="SectionPart"/> that should be written to stream.
      /// </summary>
      /// <param name="part">The <see cref="SectionPart"/> to write.</param>
      /// <param name="array">The auxiliary byte array to use.</param>
      /// <param name="stream">The stream to write the <paramref name="part"/> to.</param>
      /// <param name="writingStatus">The <see cref="DefaultWritingStatus"/>.</param>
      /// <remarks>
      /// This method assumes that the stream is positioned at the end of the previous <see cref="SectionPart"/>.
      /// </remarks>
      protected void WritePart(
         SectionPart part,
         ResizableArray<Byte> array,
         Stream stream,
         DefaultWritingStatus writingStatus
         )
      {
         var partOffset = part.Offset;
         var prePadding = checked((Int32) ( partOffset - stream.Position ));
         if ( prePadding < 0 || stream.Position != partOffset - prePadding )
         {
            // TODO better exception type
            throw new InvalidOperationException( "Internal error: stream position for " + part.Functionality + " was calculated to be " + ( partOffset - prePadding ) + ", but was " + stream.Position + "." );
         }

         // Write pre-padding
         if ( prePadding > 0 )
         {
            var idx = 0;
            array.CurrentMaxCapacity = prePadding;
            array.ZeroOut( ref idx, prePadding );
            stream.Write( array.Array, prePadding );
         }

         // Write actual contents
         var size = part.Size;
         var cur = 0;
         part.Functionality.WriteData( new SectionPartWritingArgs(
            bytesToWrite =>
            {
               if ( bytesToWrite > 0 )
               {
                  if ( cur + bytesToWrite > size )
                  {
                     // TODO better exception type
                     throw new InvalidOperationException( "Internal error: " + part.Functionality + " tried to write " + bytesToWrite + " bytes, which would have been bigger than its size of " + size + " bytes." );
                  }
                  stream.Write( array.Array, bytesToWrite );
                  cur += bytesToWrite;
               }
            },
            array,
            part.Size,
            writingStatus
            ) );
      }

      /// <summary>
      /// This method is called by <see cref="CalculateImageLayout"/> in order to create <see cref="CLIHeader"/>.
      /// </summary>
      /// <param name="writingStatus">The <see cref="DefaultWritingStatus"/>.</param>
      /// <returns>A new instance of <see cref="CLIHeader"/>.</returns>
      protected virtual CLIHeader CreateCLIHeader(
         DefaultWritingStatus writingStatus
         )
      {
         var imageSections = writingStatus.SectionLayouts;
         var embeddedResources = imageSections.GetSectionPartWithFunctionalityOfType<SectionPartFunctionalityWithDataReferenceTargets>( f => f.RelatedTable == Tables.ManifestResource );
         var snData = imageSections.GetSectionPartWithFunctionalityOfType<SectionPartFunctionality_StrongNameSignature>();
         var md = imageSections.GetSectionPartWithFunctionalityOfType<SectionPartFunctionality_MetaData>();
         var options = this.WritingOptions.CLIOptions.HeaderOptions;
         var flags = options.ModuleFlags ?? ModuleFlags.ILOnly;
         if ( writingStatus.StrongNameInformation != null )
         {
            flags |= ModuleFlags.StrongNameSigned;
         }
         var managedEP = options.ManagedEntryPointToken;
         Int32 ep;
         if ( managedEP.HasValue )
         {
            ep = managedEP.Value.GetOneBasedToken();
         }
         else
         {
            var nativeEP = options.UnmanagedEntryPointToken;
            if ( nativeEP.HasValue )
            {
               ep = nativeEP.Value;
               flags |= ModuleFlags.NativeEntrypoint;
            }
            else
            {
               ep = 0;
            }
         }

         return new CLIHeader(
               SectionPartFunctionality_CLIHeader.HEADER_SIZE,
               (UInt16) ( options.MajorRuntimeVersion ?? 2 ),
               (UInt16) ( options.MinorRuntimeVersion ?? 5 ),
               md.GetDataDirectory(),
               flags,
               (UInt32) ep,
               embeddedResources.GetDataDirectory(),
               snData.GetDataDirectory(),
               default( DataDirectory ), // TODO: customize code manager
               default( DataDirectory ), // TODO: customize vtable fixups
               default( DataDirectory ), // TODO: customize exported address table jumps
               default( DataDirectory ) // TODO: customize managed native header
               );
      }

   }

   /// <summary>
   /// This class specializes <see cref="WritingStatus"/> to hold some additional information regarding section layout and data reference columns.
   /// </summary>
   public class DefaultWritingStatus : WritingStatus
   {
      /// <summary>
      /// Creates a new instance of <see cref="DefaultWritingStatus"/> with given parameters.
      /// </summary>
      /// <param name="machine"></param>
      /// <param name="fileAlignment"></param>
      /// <param name="sectionAlignment"></param>
      /// <param name="imageBase"></param>
      /// <param name="strongNameVariables"></param>
      /// <param name="dataDirCount"></param>
      public DefaultWritingStatus(
         ImageFileMachine machine,
         Int32? fileAlignment,
         Int32? sectionAlignment,
         Int64? imageBase,
         StrongNameInformation strongNameVariables,
         Int32 dataDirCount
         ) : base( machine, fileAlignment, sectionAlignment, imageBase, strongNameVariables, dataDirCount )
      {

      }

      /// <summary>
      /// Gets or sets the <see cref="ImageSectionsInfo"/> containing information about all the sections of the image being emitted.
      /// </summary>
      /// <value>The <see cref="ImageSectionsInfo"/> containing information about all the sections of the image being emitted.</value>
      /// <remarks>
      /// This property is set by <see cref="DefaultWriterFunctionality.CalculateImageLayout"/> method.
      /// </remarks>
      public ImageSectionsInfo SectionLayouts { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="ColumnValueStorage{TValue}"/> holding values for all data reference columns in <see cref="CILMetaData"/> being written.
      /// </summary>
      /// <value>The <see cref="ColumnValueStorage{TValue}"/> holding values for all data reference columns in <see cref="CILMetaData"/> being written.</value>
      /// <remarks>
      /// This property is set by <see cref="DefaultWriterTableStreamHandler.FillOtherMDStreams"/> method.
      /// </remarks>
      public ColumnValueStorage<Int64> DataReferencesStorage { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="SectionPartFunctionalityWithDataReferenceTargets"/> objects in order to write the data for the data reference columns in <see cref="CILMetaData"/>  being written.
      /// </summary>
      /// <value>The <see cref="SectionPartFunctionalityWithDataReferenceTargets"/> objects in order to write the data for the data reference columns in <see cref="CILMetaData"/>  being written.</value>
      /// <remarks>
      /// This property is set by <see cref="DefaultWriterTableStreamHandler.FillOtherMDStreams"/> method.
      /// </remarks>
      public ArrayQuery<SectionPartFunctionalityWithDataReferenceTargets> DataReferencesSectionParts { get; set; }
   }

   /// <summary>
   /// This class contains the final information about single section, with complete <see cref="CAMPhysicalIO::CILAssemblyManipulator.Physical.IO.SectionHeader"/> and an array of <see cref="SectionPart"/>s describing the byte size and offset of each continuous chunk in the section.
   /// Unlike <see cref="SectionDescription"/>, the offsets and RVAs are calculated at this stage.
   /// </summary>
   public class SectionLayout
   {
      /// <summary>
      /// Creates a new <see cref="SectionLayout"/> from given <see cref="SectionDescription"/>, and using given parameters to calculate the data for <see cref="SectionHeader"/>.
      /// </summary>
      /// <param name="layout">The <see cref="SectionDescription"/> object, containing the <see cref="SectionPartFunctionality"/>s constituting the section.</param>
      /// <param name="sectionStartOffset">The absolute offset, in bytes, where this section starts.</param>
      /// <param name="sectionStartRVA">The RVA where this section starts.</param>
      /// <param name="fileAlignment">The file alignment.</param>
      /// <param name="rawValues">The <see cref="ColumnValueStorage{TValue}"/> for data reference columns of <see cref="CILMetaData"/>. This constructor will call <see cref="SectionPartFunctionality.GetDataSize"/>, where this <see cref="ColumnValueStorage{TValue}"/> is populated.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="layout"/> is <c>null</c>.</exception>
      public SectionLayout(
         SectionDescription layout,
         Int64 sectionStartOffset,
         TRVA sectionStartRVA,
         Int32 fileAlignment,
         ColumnValueStorage<Int64> rawValues
         )
      {
         ArgumentValidator.ValidateNotNull( "Layout", layout );

         var curRVA = sectionStartRVA;
         var curOffset = sectionStartOffset;

         var list = new List<SectionPart>( layout.Functionalities.Count );
         foreach ( var part in layout.Functionalities )
         {
            var includePart = part != null;
            if ( includePart )
            {
               var prePadding = (Int32) ( curRVA.RoundUpI64( (UInt32) part.DataAlignment ) - curRVA );
               var size = part.GetDataSize( curOffset + prePadding, curRVA + prePadding, rawValues );
               includePart = size != 0;
               if ( includePart )
               {
                  curOffset += prePadding;
                  curRVA += prePadding;

                  list.Add( new SectionPart( part, size, curOffset, curRVA ) );

                  curOffset += (UInt32) size;
                  curRVA += (UInt32) size;
               }
            }
         }

         this.Parts = list.ToArrayProxy().CQ;
         var nameBytes = layout.NameBytes;
         var virtualSize = (UInt32) ( curOffset - sectionStartOffset );
         this.SectionHeader = new SectionHeader(
            nameBytes.IsNullOrEmpty() ? layout.Name.CreateASCIIBytes( 0, 0x08, 0x08 ) : nameBytes.ToArrayProxy().CQ,
            virtualSize,
            (UInt32) sectionStartRVA,
            virtualSize.RoundUpU32( (UInt32) fileAlignment ),
            (UInt32) sectionStartOffset,
            0,
            0,
            0,
            0,
            layout.Characteristics
         );

      }

      /// <summary>
      /// Gets the array of <see cref="SectionPart"/>s making up this section.
      /// </summary>
      /// <value>The array of <see cref="SectionPart"/>s making up this section.</value>
      public ArrayQuery<SectionPart> Parts { get; }

      /// <summary>
      /// Gets the constructed <see cref="CAMPhysicalIO::CILAssemblyManipulator.Physical.IO.SectionHeader"/>.
      /// </summary>
      /// <value>The constructed <see cref="CAMPhysicalIO::CILAssemblyManipulator.Physical.IO.SectionHeader"/>.</value>
      public SectionHeader SectionHeader { get; }
   }

   /// <summary>
   /// This class contains information about all the sections that will be written to image.
   /// </summary>
   public sealed class ImageSectionsInfo
   {

      /// <summary>
      /// Creates a new <see cref="ImageSectionsInfo"/> with given <see cref="SectionLayout"/>s
      /// </summary>
      /// <param name="layoutInfos">The array of <see cref="SectionLayout"/>s, one each for section.</param>
      public ImageSectionsInfo( IEnumerable<SectionLayout> layoutInfos )
      {
         this.Sections = layoutInfos.ToArrayProxy().CQ;
      }

      /// <summary>
      /// Gets the array of <see cref="SectionLayout"/>s, making up all sections of the image.
      /// </summary>
      /// <value>The array of <see cref="SectionLayout"/>s, making up all sections of the image.</value>
      public ArrayQuery<SectionLayout> Sections { get; }
   }

   /// <summary>
   /// This class represents abstract description of the section and what it may contain.
   /// The concrete offsets and RVAs are not known at this stage.
   /// </summary>
   public class SectionDescription
   {

      /// <summary>
      /// Creates a new instance of <see cref="SectionDescription"/> with empty <see cref="Functionalities"/> list.
      /// </summary>
      public SectionDescription()
      {
         this.Functionalities = new List<SectionPartFunctionality>();
      }

      /// <summary>
      /// Creates a new instance of <see cref="SectionDescription"/>, and fills the <see cref="Functionalities"/> list with given enumerable of <see cref="SectionPartFunctionality"/>.
      /// </summary>
      /// <param name="parts">The enumerable of <see cref="SectionPartFunctionality"/> objects. May be <c>null</c>. If not <c>null</c>, the single <c>null</c> elements will be filtered out.</param>
      public SectionDescription( IEnumerable<SectionPartFunctionality> parts )
         : this()
      {
         this.Functionalities.AddRange( parts?.Where( p => p != null ) ?? Empty<SectionPartFunctionality>.Enumerable );
      }

      /// <summary>
      /// Gets the textual name of the section.
      /// </summary>
      /// <value>The textual name of the section.</value>
      /// <remarks>
      /// When creating concrete <see cref="SectionLayout"/>, the non-<c>null</c> and non-empty <see cref="NameBytes"/> takes precedence over this property.
      /// </remarks>
      public String Name { get; set; }

      /// <summary>
      /// Gets the name bytes of the section.
      /// </summary>
      /// <value>The name bytes of the section.</value>
      /// <remarks>
      /// When creating concrete <see cref="SectionLayout"/>, this property takes precedence over <see cref="Name"/>, if this is non-<c>null</c> and non-empty.
      /// </remarks>
      public Byte[] NameBytes { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="SectionHeaderCharacteristics"/> of this section.
      /// </summary>
      /// <value>The <see cref="SectionHeaderCharacteristics"/> of this section.</value>
      public SectionHeaderCharacteristics Characteristics { get; set; }

      /// <summary>
      /// Gets the modifiable list of all <see cref="SectionPartFunctionality"/> objects that may be part of the actual written section.
      /// </summary>
      /// <value>The modifiable list of all <see cref="SectionPartFunctionality"/> objects that may be part of the actual written section.</value>
      public List<SectionPartFunctionality> Functionalities { get; }
   }

   /// <summary>
   /// This interface should be implemented by all objects intending to participate in writing any data that is located in some image section.
   /// </summary>
   public interface SectionPartFunctionality
   {
      /// <summary>
      /// Gets the alignment that this section part should be aligned as.
      /// </summary>
      /// <value>The alignment that this section part should be aligned as.</value>
      /// <remarks>
      /// This value should be power of two, although this is currently not enforced at run-time.
      /// </remarks>
      Int32 DataAlignment { get; }

      // TODO this method should be something like this
      // SectionPartWriter CreateWriter( Int64 currentOffset, TRVA currentRVA, ColumnValueStorage<Int64> dataRefs, out Int32 dataSize);
      // And SectionPartWriter is interface containing single method:
      // void WriteData( SectionPartWritingArgs args );

      /// <summary>
      /// Computes the size of this section part, in bytes.
      /// </summary>
      /// <param name="currentOffset">The current offset of the image.</param>
      /// <param name="currentRVA">The current RVA of the image.</param>
      /// <param name="dataRefs">The <see cref="ColumnValueStorage{TValue}"/> for data reference columns of <see cref="CILMetaData"/>.</param>
      /// <returns>The size of this section part, in bytes.</returns>
      /// <remarks>
      /// If the returned size is <c>0</c>, this <see cref="SectionPartFunctionality"/> will not participate in writing data, i.e. its <see cref="WriteData"/> method will not be called.
      /// </remarks>
      Int32 GetDataSize( Int64 currentOffset, TRVA currentRVA, ColumnValueStorage<Int64> dataRefs );

      /// <summary>
      /// This method should write the whatever data there is.
      /// </summary>
      /// <param name="args">The <see cref="SectionPartWritingArgs"/> containing required information to write the data.</param>
      void WriteData( SectionPartWritingArgs args );
   }

   /// <summary>
   /// This interface further specializes <see cref="SectionPartFunctionality"/> for section parts containing data, which is referenced by data reference columns of <see cref="CILMetaData"/> (e.g. <see cref="MethodDefinition.IL"/>).
   /// </summary>
   public interface SectionPartFunctionalityWithDataReferenceTargets : SectionPartFunctionality
   {
      /// <summary>
      /// Gets the table ID of the related table, as <see cref="Tables"/> enumeration.
      /// </summary>
      /// <value>The table ID of the related table, as <see cref="Tables"/> enumeration.</value>
      Tables RelatedTable { get; }

      /// <summary>
      /// Gets the column index within the table for data reference column.
      /// </summary>
      /// <value>The column index within the table for data reference column.</value>
      Int32 RelatedTableColumnIndex { get; }
   }

   /// <summary>
   /// This class implements <see cref="SectionPartFunctionality"/> with fixed alignment, and possibility to opt out from calculating data size.
   /// </summary>
   public abstract class SectionPartFunctionalityWithFixedAlignment : SectionPartFunctionality
   {
      /// <summary>
      /// Creates new instance of <see cref="SectionPartFunctionalityWithFixedAlignment"/> with given alignment and flag indicating whether this <see cref="SectionPartFunctionalityWithFixedAlignment"/> should be present in the final image.
      /// </summary>
      /// <param name="alignment">The data alignment, should be power of two and greater than zero.</param>
      /// <param name="isPresent">Whether this <see cref="SectionPartFunctionalityWithFixedAlignment"/> is present in final image.</param>
      /// <exception cref="ArgumentOutOfRangeException">If <paramref name="alignment"/> is <c>0</c> or less.</exception>
      public SectionPartFunctionalityWithFixedAlignment( Int32 alignment, Boolean isPresent )
      {
         if ( alignment <= 0 )
         {
            throw new ArgumentOutOfRangeException( "Alignment" );
         }

         this.DataAlignment = alignment;
         this.Write = isPresent;
      }

      /// <summary>
      /// Gets the data alignment supplied to this <see cref="SectionPartFunctionalityWithFixedAlignment"/>.
      /// </summary>
      /// <value>The data alignment supplied to this <see cref="SectionPartFunctionalityWithFixedAlignment"/>.</value>
      public Int32 DataAlignment { get; }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionality.GetDataSize"/> by first checking the <see cref="Write"/> property, and then invoking <see cref="DoGetDataSize"/>, if the property is <c>true</c>.
      /// </summary>
      /// <param name="currentOffset">The current offset.</param>
      /// <param name="currentRVA">The current RVA.</param>
      /// <param name="dataReferences">The <see cref="ColumnValueStorage{TValue}"/> for data reference columns.</param>
      /// <returns>The value of <see cref="DoGetDataSize"/> if <paramref name="Write"/> is <c>true</c>; <c>0</c> otherwise.</returns>
      public Int32 GetDataSize( Int64 currentOffset, TRVA currentRVA, ColumnValueStorage<Int64> dataReferences )
      {
         return this.Write ?
            this.DoGetDataSize( currentOffset, currentRVA, dataReferences ) :
            0;
      }

      /// <summary>
      /// Performs writing.
      /// </summary>
      /// <param name="args">The <see cref="SectionPartWritingArgs"/>.</param>
      public abstract void WriteData( SectionPartWritingArgs args );

      /// <summary>
      /// Gets the presence flag supplied to this <see cref="SectionPartFunctionalityWithFixedAlignment"/>.
      /// </summary>
      /// <value>The presence flag supplied to this <see cref="SectionPartFunctionalityWithFixedAlignment"/>.</value>
      protected Boolean Write { get; }

      /// <summary>
      /// Performs actual data size calculation.
      /// </summary>
      /// <param name="currentOffset">The current offset.</param>
      /// <param name="currentRVA">The current RVA.</param>
      /// <param name="dataReferences">The <see cref="ColumnValueStorage{TValue}"/> for data reference columns.</param>
      /// <returns>The size of this <see cref="SectionPartFunctionalityWithFixedAlignment"/>, in bytes.</returns>
      protected abstract Int32 DoGetDataSize( Int64 currentOffset, TRVA currentRVA, ColumnValueStorage<Int64> dataReferences );
   }

   /// <summary>
   /// This class specializes <see cref="SectionPartFunctionalityWithFixedAlignment"/> for section part functionalities having small enough data content so that it can be written into byte array in single go.
   /// </summary>
   public abstract class SectionPartFunctionalityWriteableToArray : SectionPartFunctionalityWithFixedAlignment
   {
      /// <summary>
      /// Creates new instance of <see cref="SectionPartFunctionalityWriteableToArray"/> with given alignment and flag indicating whether this <see cref="SectionPartFunctionalityWriteableToArray"/> should be present in the final image.
      /// </summary>
      /// <param name="alignment">The data alignment, should be power of two and greater than zero.</param>
      /// <param name="isPresent">Whether this <see cref="SectionPartFunctionalityWriteableToArray"/> is present in final image.</param>
      /// <exception cref="ArgumentOutOfRangeException">If <paramref name="alignment"/> is <c>0</c> or less.</exception>
      public SectionPartFunctionalityWriteableToArray( Int32 alignment, Boolean isPresent )
         : base( alignment, isPresent )
      {
      }

      /// <summary>
      /// Sets up the <see cref="SectionPartWritingArgs.ArrayHelper"/>, and calls <see cref="DoWriteData"/> to write the data to array.
      /// Then writes the array contents to <see cref="SectionPartWritingArgs.Stream"/>.
      /// </summary>
      /// <param name="args">The <see cref="SectionPartWritingArgs"/>.</param>
      public override void WriteData( SectionPartWritingArgs args )
      {
         var array = args.ArrayHelper;
         var size = args.PartSize;
         array.CurrentMaxCapacity = size;
         var bytez = array.Array;
         if ( !this.DoWriteData( args.WritingStatus, bytez ) )
         {
            var idx = 0;
            bytez.ZeroOut( ref idx, size );
         }
         args.WriteCallback( size );
      }

      /// <summary>
      /// This method should write the section contents to given array, starting at index <c>0</c>.
      /// </summary>
      /// <param name="wStatus">The <see cref="DefaultWritingStatus"/>.</param>
      /// <param name="array">The array to write data to.</param>
      /// <returns><c>true</c> if writing was successful; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// If return value is <c>false</c>, then the whole section part data will be zeroed out.
      /// </remarks>
      protected abstract Boolean DoWriteData( DefaultWritingStatus wStatus, Byte[] array );
   }

   /// <summary>
   /// This class specializes <see cref="SectionPartFunctionalityWriteableToArray"/> when the data size of the section part is known at construction time.
   /// </summary>
   public abstract class SectionPartFunctionalityWithFixedLength : SectionPartFunctionalityWriteableToArray
   {

      /// <summary>
      /// Creates new instance of <see cref="SectionPartFunctionalityWriteableToArray"/> with given alignment and flag indicating whether this <see cref="SectionPartFunctionalityWriteableToArray"/> should be present in the final image, in addition to pre-calculated data size.
      /// </summary>
      /// <param name="alignment">The data alignment, should be power of two and greater than zero.</param>
      /// <param name="isPresent">Whether this <see cref="SectionPartFunctionalityWriteableToArray"/> is present in final image.</param>
      /// <param name="size">The size, in bytes, of this section part, if present.</param>
      /// <exception cref="ArgumentOutOfRangeException">If <paramref name="alignment"/> is <c>0</c> or less.</exception>
      public SectionPartFunctionalityWithFixedLength( Int32 alignment, Boolean isPresent, Int32 size )
         : base( alignment, isPresent )
      {
         if ( size < 0 )
         {
            throw new ArgumentOutOfRangeException( "Size" );
         }
         this.DataSize = size;
      }

      /// <summary>
      /// Gets the size of data, in bytes.
      /// </summary>
      /// <value>The size of data, in bytes.</value>
      protected Int32 DataSize { get; }

      /// <summary>
      /// Returns the value of <see cref="DataSize"/>.
      /// </summary>
      /// <param name="currentOffset">The current offset.</param>
      /// <param name="currentRVA">The current RVA.</param>
      /// <param name="rawValues">The <see cref="ColumnValueStorage{TValue}"/> for data reference columns.</param>
      /// <returns>The value of <see cref="DataSize"/>.</returns>
      protected override Int32 DoGetDataSize( Int64 currentOffset, TRVA currentRVA, ColumnValueStorage<Int64> rawValues )
      {
         return this.DataSize;
      }
   }

   /// <summary>
   /// This class specializes <see cref="SectionPartFunctionalityWithFixedAlignment"/> and implements <see cref="SectionPartFunctionalityWithDataReferenceTargets"/>, defining simple abstract methods that should be implemented to actually write data for data reference column values.
   /// </summary>
   /// <typeparam name="TRow">The type of the row.</typeparam>
   /// <typeparam name="TSizeInfo">The type describing information about the size of the data of a single column value.</typeparam>
   public abstract class SectionPartFunctionalityWithDataReferenceTargetsImpl<TRow, TSizeInfo> : SectionPartFunctionalityWithFixedAlignment, SectionPartFunctionalityWithDataReferenceTargets
      where TRow : class
      where TSizeInfo : struct
   {
      private readonly Int32 _min;
      private readonly Int32 _max;
      private readonly List<TRow> _rows;
      private readonly ArrayProxy<TSizeInfo?> _sizes;

      /// <summary>
      /// Creates a new instance of <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}"/> with given parameters.
      /// </summary>
      /// <param name="alignment">The data alignment.</param>
      /// <param name="table">The <see cref="MetaDataTable{TRow}"/> containing rows with data reference columns.</param>
      /// <param name="columnIndex">The index of the data reference column that will be used.</param>
      /// <param name="min">The minimum index of the rows, inclusive. Use <c>-1</c> to include rows from the beginning.</param>
      /// <param name="max">The maximum index of the rows, exclusive. Use <c>-1</c> to include rows until the end.</param>
      /// <exception cref="ArgumentOutOfRangeException">If <paramref name="alignment"/> is <c>0</c> or less.</exception>
      /// <exception cref="ArgumentNullException">If <paramref name="table"/> is <c>null</c>.</exception>
      public SectionPartFunctionalityWithDataReferenceTargetsImpl(
         Int32 alignment,
         MetaDataTable<TRow> table,
         Int32 columnIndex,
         Int32 min,
         Int32 max
         )
         : base( alignment, table.GetRowCount() > 0 )
      {
         ArgumentValidator.ValidateNotNull( "Table", table );

         var rows = table.TableContents;

         if ( min <= 0 )
         {
            min = 0;
         }
         var count = rows.Count;
         if ( max >= count )
         {
            max = count;
         }

         if ( max < min )
         {
            if ( max < 0 )
            {
               max = count;
            }
            else
            {
               max = min;
            }
         }

         var cf = CollectionsWithRoles.Implementation.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY;

         var range = max - min;
         this._rows = rows;
         this.RelatedTable = (Tables) table.GetTableIndex();
         this.RelatedTableColumnIndex = columnIndex;
         this._min = min;
         this._max = max;
         this._sizes = cf.NewArrayProxy( new TSizeInfo?[range] );
      }

      /// <summary>
      /// Computes the data size by iterating rows and using <see cref="GetSizeInfo"/>, <see cref="GetValueForTableStreamFromSize"/>, <see cref="GetValueForTableStreamFromRow"/>, and <see cref="GetSize"/> methods.
      /// </summary>
      /// <param name="currentOffset">The current offset.</param>
      /// <param name="currentRVA">The current RVA.</param>
      /// <param name="rawValues">The <see cref="ColumnValueStorage{TValue}"/> for data reference columns.</param>
      /// <returns>The size of this <see cref="SectionPartFunctionalityWithFixedAlignment"/>, in bytes.</returns>
      protected override Int32 DoGetDataSize( Int64 currentOffset, TRVA currentRVA, ColumnValueStorage<Int64> rawValues )
      {
         var startOffset = currentOffset;
         var startRVA = currentRVA;
         var rows = this._rows;
         var sizesArray = this._sizes.Array;
         for ( var i = this._min; i < this._max; ++i )
         {
            // Calculate size
            var row = this._rows[i];
            var sizeInfoNullable = this.GetSizeInfo( i, row, currentOffset, currentRVA, startOffset, startRVA );
            var arrayIdx = i - this._min;

            // Save size and RVA information
            sizesArray[arrayIdx] = sizeInfoNullable;
            var hasValue = sizeInfoNullable.HasValue;
            rawValues.SetRawValue(
               this.RelatedTable,
               i,
               this.RelatedTableColumnIndex,
               hasValue ? this.GetValueForTableStreamFromSize( currentRVA, sizeInfoNullable.Value ) : this.GetValueForTableStreamFromRow( i, row )
               );

            // Update offset + rva
            if ( hasValue )
            {
               var size = (UInt32) this.GetSize( sizeInfoNullable.Value );
               currentOffset += size;
               currentRVA += size;
            }
         }

         return (Int32) ( currentOffset - startOffset );
      }

      /// <summary>
      /// Writes all the data of the rows using <see cref="WriteData(TRow, TSizeInfo, byte[])"/> method to write data of the single row.
      /// </summary>
      /// <param name="args">The <see cref="SectionPartWritingArgs"/>.</param>
      public override void WriteData( SectionPartWritingArgs args )
      {
         var array = args.ArrayHelper;
         var writeCB = args.WriteCallback;

         // Write contents
         var sizesArray = this._sizes.Array;
         for ( var i = this._min; i < this._max; ++i )
         {
            var sizeInfoNullable = sizesArray[i - this._min];
            if ( sizeInfoNullable.HasValue )
            {
               var sizeInfo = sizeInfoNullable.Value;
               var capacity = this.GetSize( sizeInfo );
               if ( capacity > 0 )
               {
                  array.CurrentMaxCapacity = capacity;
                  var bytez = array.Array;
                  this.WriteData( this._rows[i], sizeInfo, bytez );
                  writeCB( capacity );
               }
            }
         }
      }

      /// <inheritdoc />
      public Tables RelatedTable { get; }

      /// <inheritdoc />
      public Int32 RelatedTableColumnIndex { get; }

      /// <summary>
      /// This method should be implemented by subclasses to extract size information for single data reference column of a single row.
      /// </summary>
      /// <param name="rowIndex">The index of the row.</param>
      /// <param name="row">The row instance.</param>
      /// <param name="currentOffset">Current offset.</param>
      /// <param name="currentRVA">Current RVA.</param>
      /// <param name="startOffset">Offset at the start of this section part.</param>
      /// <param name="startRVA">RVA at the start of this section part.</param>
      /// <returns>Size information object for this row and column, or <c>null</c> if this row does not have data that should be written to this section part.</returns>
      protected abstract TSizeInfo? GetSizeInfo( Int32 rowIndex, TRow row, Int64 currentOffset, TRVA currentRVA, Int64 startOffset, TRVA startRVA );

      /// <summary>
      /// Given the size information object, this should extract the actual size as integer.
      /// </summary>
      /// <param name="sizeInfo">The size information object.</param>
      /// <returns>The size, in bytes, of the data.</returns>
      protected abstract Int32 GetSize( TSizeInfo sizeInfo );

      /// <summary>
      /// Gets the value to be written to table stream from size information object, if such was supplied by <see cref="GetSizeInfo"/> method.
      /// </summary>
      /// <param name="currentRVA">Current RVA.</param>
      /// <param name="sizeInfo">The size information object.</param>
      /// <returns>The value to be written to table stream.</returns>
      protected abstract TRVA GetValueForTableStreamFromSize( TRVA currentRVA, TSizeInfo sizeInfo );

      /// <summary>
      /// Gets the value to be written to table stream from row, if <see cref="GetSizeInfo"/> method returned <c>null</c>.
      /// </summary>
      /// <param name="rowIndex">The row index.</param>
      /// <param name="row">The row instance.</param>
      /// <returns>The value to be written to table stream.</returns>
      protected abstract TRVA GetValueForTableStreamFromRow( Int32 rowIndex, TRow row );

      /// <summary>
      /// This method should write the data of the data reference column to given array.
      /// </summary>
      /// <param name="row">The row instance.</param>
      /// <param name="sizeInfo">The size information object for this row, as returned by <see cref="GetSizeInfo"/>.</param>
      /// <param name="array">The array to write data to. The writing should begin at index <c>0</c>.</param>
      protected abstract void WriteData( TRow row, TSizeInfo sizeInfo, Byte[] array );
   }

   /// <summary>
   /// This class specializes <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}"/> to implement writing <see cref="MethodILDefinition"/>s.
   /// </summary>
   public class SectionPartFunctionality_MethodIL : SectionPartFunctionalityWithDataReferenceTargetsImpl<MethodDefinition, SectionPartFunctionality_MethodIL.MethodSizeInfo>
   {
      /// <summary>
      /// This struct contains information about the size of the serialized <see cref="MethodILDefinition"/>.
      /// </summary>
      public struct MethodSizeInfo
      {
         /// <summary>
         /// Creates a new instance of <see cref="MethodSizeInfo"/> with given parameters.
         /// </summary>
         /// <param name="prePadding">The padding before actual content starts.</param>
         /// <param name="byteSize">The size of the content, in bytes.</param>
         /// <param name="ilCodeByteCount">The size of the IL.</param>
         /// <param name="isTinyHeader">Whether the header before IL is tiny.</param>
         /// <param name="exceptionSectionsAreLarge">Whether the exception sections are large.</param>
         public MethodSizeInfo( Int32 prePadding, Int32 byteSize, Int32 ilCodeByteCount, Boolean isTinyHeader, Boolean exceptionSectionsAreLarge )
         {
            this.PrePadding = prePadding;
            this.ByteSize = byteSize;
            this.ILCodeByteCount = ilCodeByteCount;
            this.IsTinyHeader = isTinyHeader;
            this.ExceptionSectionsAreLarge = exceptionSectionsAreLarge;
         }

         /// <summary>
         /// Gets the amount of zero padding before actual content starts.
         /// </summary>
         /// <value>The amount of zero padding before actual content starts.</value>
         public Int32 PrePadding { get; }

         /// <summary>
         /// Gets the size of the content, in bytes.
         /// </summary>
         /// <value>The size of the content, in bytes.</value>
         public Int32 ByteSize { get; }

         /// <summary>
         /// Gets the size of the IL code, in bytes.
         /// </summary>
         /// <value>The size of the IL code, in bytes.</value>
         public Int32 ILCodeByteCount { get; }

         /// <summary>
         /// Gets the value indicating whether the header before IL code is tiny.
         /// </summary>
         /// <value>The value indicating whether the header before IL code is tiny.</value>
         public Boolean IsTinyHeader { get; }

         /// <summary>
         /// Gets the value indicating whether the exception sections after IL are large.
         /// </summary>
         /// <value>The value indicating whether the exception sections after IL are large.</value>
         public Boolean ExceptionSectionsAreLarge { get; }
      }

      private const Int32 METHOD_DATA_SECTION_HEADER_SIZE = 4;
      private const Int32 SMALL_EXC_BLOCK_SIZE = 12;
      private const Int32 LARGE_EXC_BLOCK_SIZE = 24;
      internal const Int32 MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION = ( Byte.MaxValue - METHOD_DATA_SECTION_HEADER_SIZE ) / SMALL_EXC_BLOCK_SIZE; // 20
      private const Int32 MAX_LARGE_EXC_HANDLERS_IN_ONE_SECTION = ( 0x00FFFFFF - METHOD_DATA_SECTION_HEADER_SIZE ) / LARGE_EXC_BLOCK_SIZE; // 699050
      private const Int32 FAT_HEADER_SIZE = 12;

      private readonly CILMetaData _md;
      private readonly IDictionary<OpCodeInfoWithString, Int32> _stringTokens;

      /// <summary>
      /// Creates a new instance of <see cref="SectionPartFunctionality_MethodIL"/>.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/> to use to obtain <see cref="MethodILDefinition"/>s from <see cref="CILMetaData.MethodDefinitions"/>.</param>
      /// <param name="userStrings">The <see cref="WriterStringStreamHandler"/> to get indices for string values of <see cref="OpCodeInfoWithString"/>s.</param>
      /// <param name="columnIndex">The column index of <see cref="MethodDefinition"/>. Should be left at zero.</param>
      /// <param name="min">The minimum index to start reading contents of <see cref="CILMetaData.MethodDefinitions"/>, inclusive. Use <c>-1</c> or <c>0</c> to start reading from beginning.</param>
      /// <param name="max">The maximum index to end reading contents of <see cref="CILMetaData.MethodDefinitions"/>, exclusive. Use <c>-1</c> to read until the end.</param>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="md"/> or <paramref name="userStrings"/> is <c>null</c>.</exception>
      public SectionPartFunctionality_MethodIL(
         CILMetaData md,
         WriterStringStreamHandler userStrings,
         Int32 columnIndex = 0,
         Int32 min = 0,
         Int32 max = -1
         )
         : base( 0x04, ArgumentValidator.ValidateNotNull( "Meta data", md ).MethodDefinitions, columnIndex, min, max )
      {
         ArgumentValidator.ValidateNotNull( "User strings", userStrings );

         this._md = md;
         this._stringTokens = md.MethodDefinitions.TableContents
            .Select( m => m?.IL )
            .Where( il => il != null )
            .SelectMany( il => il.OpCodes.OfType<OpCodeInfoWithString>() )
            .ToDictionary_Overwrite( o => o, o => userStrings.RegisterString( o.Operand ), ReferenceEqualityComparer<OpCodeInfoWithString>.ReferenceBasedComparer );
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}.GetSize"/> method.
      /// </summary>
      /// <param name="sizeInfo">The <see cref="MethodSizeInfo"/>.</param>
      /// <returns>The sum of <see cref="MethodSizeInfo.PrePadding"/> and <see cref="MethodSizeInfo.ByteSize"/>.</returns>
      protected override Int32 GetSize( MethodSizeInfo sizeInfo )
      {
         return sizeInfo.PrePadding + sizeInfo.ByteSize;
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}.GetSizeInfo"/> method.
      /// </summary>
      /// <param name="rowIndex">The row index.</param>
      /// <param name="row">The <see cref="MethodDefinition"/> row.</param>
      /// <param name="currentOffset">The current offset.</param>
      /// <param name="currentRVA">The current RVA.</param>
      /// <param name="startOffset">The start offset of this section part.</param>
      /// <param name="startRVA">The start RVA of this section part.</param>
      /// <returns>The result of <see cref="CalculateSizeForMethodIL"/>, if <paramref name="row"/> is not <c>null</c> and its <see cref="MethodDefinition.IL"/> property is not <c>null</c>. Otherwise returns <c>null</c>.</returns>
      protected override MethodSizeInfo? GetSizeInfo( Int32 rowIndex, MethodDefinition row, Int64 currentOffset, TRVA currentRVA, Int64 startOffset, TRVA startRVA )
      {
         var il = row?.IL;
         return il == null ?
            (MethodSizeInfo?) null :
            this.CalculateSizeForMethodIL( rowIndex, currentRVA );
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}.GetValueForTableStreamFromSize"/> method.
      /// </summary>
      /// <param name="currentRVA">The current RVA.</param>
      /// <param name="sizeInfo">The size information about this <see cref="MethodILDefinition"/>.</param>
      /// <returns>The sum of <paramref name="currentRVA"/> and <see cref="MethodSizeInfo.PrePadding"/> of <paramref name="sizeInfo"/>.</returns>
      protected override TRVA GetValueForTableStreamFromSize( TRVA currentRVA, MethodSizeInfo sizeInfo )
      {
         return currentRVA + sizeInfo.PrePadding;
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}.GetValueForTableStreamFromRow"/> method.
      /// </summary>
      /// <param name="rowIndex">The row index.</param>
      /// <param name="row">The <see cref="MethodDefinition"/> row.</param>
      /// <returns>The value <c>0</c>, since this method is called when row or <see cref="MethodDefinition.IL"/> is <c>null</c>.</returns>
      protected override TRVA GetValueForTableStreamFromRow( Int32 rowIndex, MethodDefinition row )
      {
         return 0;
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}.WriteData"/> method.
      /// </summary>
      /// <param name="row">The <see cref="MethodDefinition"/> row.</param>
      /// <param name="sizeInfo">The <see cref="MethodSizeInfo"/> returned by <see cref="GetSizeInfo"/>.</param>
      /// <param name="array">The array to write data to.</param>
      protected override void WriteData( MethodDefinition row, MethodSizeInfo sizeInfo, Byte[] array )
      {
         var idx = 0;
         var il = row.IL;
         var exceptionBlocks = il.ExceptionBlocks;
         var hasAnyExceptions = exceptionBlocks.Count > 0;
         var prePadding = sizeInfo.PrePadding;
         // Header
         if ( sizeInfo.IsTinyHeader )
         {
            // Tiny header
            array.WriteByteToBytes( ref idx, (Byte) ( (Int32) MethodHeaderFlags.TinyFormat | ( sizeInfo.ILCodeByteCount << 2 ) ) );
         }
         else
         {
            // Fat header 
            var flags = MethodHeaderFlags.FatFormat;
            if ( hasAnyExceptions )
            {
               flags |= MethodHeaderFlags.MoreSections;
            }
            if ( il.InitLocals )
            {
               flags |= MethodHeaderFlags.InitLocals;
            }

            array
               .ZeroOut( ref idx, prePadding )
               .WriteInt16LEToBytes( ref idx, (Int16) ( ( (Int32) flags ) | ( 3 << 12 ) ) )
               .WriteUInt16LEToBytes( ref idx, (UInt16) il.MaxStackSize )
               .WriteInt32LEToBytes( ref idx, sizeInfo.ILCodeByteCount )
               .WriteInt32LEToBytes( ref idx, il.LocalsSignatureIndex.GetOneBasedToken() );
         }


         // Emit IL code
         foreach ( var info in il.OpCodes )
         {
            EmitOpCodeInfo( info, array, ref idx );
         }

         // Emit exception block infos
         if ( hasAnyExceptions )
         {
            var exceptionSectionsAreLarge = sizeInfo.ExceptionSectionsAreLarge;
            var processedIndices = new HashSet<Int32>();
            array.ZeroOut( ref idx, ( idx - prePadding ).RoundUpI32( 4 ) - ( idx - prePadding ) );
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

               array.WriteInt32LEToBytes( ref idx, ( ( amountToBeWritten * excBlockSize + METHOD_DATA_SECTION_HEADER_SIZE ) << 8 ) | (Byte) flags );

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
      }

      /// <summary>
      /// This method is used by <see cref="WriteData"/>, in order to write single <see cref="OpCodeInfo"/> to byte array.
      /// </summary>
      /// <param name="codeInfo">The <see cref="OpCodeInfo"/> to write.</param>
      /// <param name="array">The array to write <paramref name="codeInfo"/> to.</param>
      /// <param name="idx">The index in <paramref name="array"/> where to start writing.</param>
      /// <exception cref="NullReferenceException"></exception>
      private void EmitOpCodeInfo(
         OpCodeInfo codeInfo,
         Byte[] array,
         ref Int32 idx
      )
      {
         const Int32 USER_STRING_MASK = 0x70 << 24;
         var code = this._md.OpCodeProvider.GetCodeFor( codeInfo.OpCodeID );

         if ( code.Size == 1 )
         {
            array.WriteByteToBytes( ref idx, (Byte) code.OpCodeID );
         }
         else
         {
            // N.B.! Big-endian! Everywhere else everything is little-endian.
            array.WriteUInt16BEToBytes( ref idx, (UInt16) code.OpCodeID );
         }

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
                  Int32 token;
                  this._stringTokens.TryGetValue( (OpCodeInfoWithString) codeInfo, out token );
                  array.WriteInt32LEToBytes( ref idx, token | USER_STRING_MASK );
                  break;
               case OperandType.InlineField:
               case OperandType.InlineMethod:
               case OperandType.InlineType:
               case OperandType.InlineToken:
               case OperandType.InlineSignature:
                  var tIdx = ( (OpCodeInfoWithTableIndex) codeInfo ).Operand;
                  array.WriteInt32LEToBytes( ref idx, tIdx.GetOneBasedToken() );
                  break;
               case OperandType.InlineSwitch:
                  var offsets = ( (OpCodeInfoWithIntegers) codeInfo ).Operand;
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

      /// <summary>
      /// This method is used by <see cref="GetSizeInfo"/> to return a <see cref="MethodSizeInfo"/> object describing various properties related to the size of serialized <see cref="MethodILDefinition"/>.
      /// </summary>
      /// <param name="rowIndex">The index in <see cref="CILMetaData.MethodDefinitions"/> to search for <see cref="MethodILDefinition"/>.</param>
      /// <param name="currentRVA">The current RVA.</param>
      /// <returns>A <see cref="MethodSizeInfo"/> describing the size of serialized <see cref="MethodILDefinition"/>.</returns>
      /// <exception cref="InvalidOperationException">If <paramref name="rowIndex"/> was invalid or the <see cref="MethodDefinition"/> at <paramref name="rowIndex"/> did not have a <see cref="MethodILDefinition"/>.</exception>
      protected MethodSizeInfo CalculateSizeForMethodIL(
         Int32 rowIndex,
         TRVA currentRVA
         )
      {
         var il = this._md.MethodDefinitions.GetOrNull( rowIndex )?.IL;

         if ( il == null )
         {
            throw new InvalidOperationException( "Either row index was invalid or method does not have IL." );
         }
         Int32 ilCodeByteCount; Boolean hasAnyExc, allAreSmall;
         var isTinyHeader = this._md.IsTinyILHeader( rowIndex, out ilCodeByteCount, out hasAnyExc, out allAreSmall );

         var arraySize = ilCodeByteCount;
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
               var excCount = il.ExceptionBlocks.Count;
               // Skip to next boundary of 4
               arraySize = arraySize.RoundUpI32( 4 );
               var excBlockSize = allAreSmall ? SMALL_EXC_BLOCK_SIZE : LARGE_EXC_BLOCK_SIZE;
               var maxExcHandlersInOnSection = allAreSmall ? MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION : MAX_LARGE_EXC_HANDLERS_IN_ONE_SECTION;
               arraySize += BinaryUtils.AmountOfPagesTaken( excCount, maxExcHandlersInOnSection ) * METHOD_DATA_SECTION_HEADER_SIZE +
                  excCount * excBlockSize;
            }
         }

         var exceptionSectionsAreLarge = hasAnyExc && !allAreSmall;

         Int32 prePadding;
         if ( isTinyHeader )
         {
            prePadding = 0;
         }
         else
         {
            // Non-tiny headers must start at 4-byte boundary
            prePadding = (Int32) ( currentRVA.RoundUpI64( 4 ) - currentRVA );
         }

         return new MethodSizeInfo( prePadding, arraySize, ilCodeByteCount, isTinyHeader, exceptionSectionsAreLarge );
      }
   }

   /// <summary>
   /// This class specializes <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}"/> to implement writing the <see cref="FieldRVA.Data"/> for <see cref="FieldRVA"/>s.
   /// </summary>
   public class SectionPartFunctionality_FieldRVA : SectionPartFunctionalityWithDataReferenceTargetsImpl<FieldRVA, Int32>
   {
      /// <summary>
      /// Creates a new instance of <see cref="SectionPartFunctionality_FieldRVA"/>.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/> to use to obtain <see cref="FieldRVA"/>s from <see cref="CILMetaData.FieldRVAs"/>.</param>
      /// <param name="columnIndex">The column index of <see cref="FieldRVA"/>. Should be left at zero.</param>
      /// <param name="min">The minimum index to start reading contents of <see cref="CILMetaData.FieldRVAs"/>, inclusive. Use <c>-1</c> or <c>0</c> to start reading from beginning.</param>
      /// <param name="max">The maximum index to end reading contents of <see cref="CILMetaData.FieldRVAs"/>, exclusive. Use <c>-1</c> to read until the end.</param>
      /// <exception cref="ArgumentNullException">If the <paramref name="md"/> is <c>null</c>.</exception>
      public SectionPartFunctionality_FieldRVA( CILMetaData md, Int32 columnIndex = 0, Int32 min = 0, Int32 max = -1 )
         : base( 0x08, md.FieldRVAs, columnIndex, min, max )
      {
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}.GetSizeInfo"/> method.
      /// </summary>
      /// <param name="rowIndex">The row index.</param>
      /// <param name="row">The <see cref="FieldRVA"/> row.</param>
      /// <param name="currentOffset">The current offset.</param>
      /// <param name="currentRVA">The current RVA.</param>
      /// <param name="startOffset">The start offset of this section part.</param>
      /// <param name="startRVA">The start RVA of this section part.</param>
      /// <returns>The length of <see cref="FieldRVA.Data"/>, if <paramref name="row"/> and <see cref="FieldRVA.Data"/> is not <c>null</c>. Otherwise, returns <c>null</c>.</returns>
      protected override Int32? GetSizeInfo( Int32 rowIndex, FieldRVA row, Int64 currentOffset, TRVA currentRVA, Int64 startOffset, TRVA startRVA )
      {
         return row?.Data?.Length;
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}.GetSize"/> method.
      /// </summary>
      /// <param name="sizeInfo">The length of <see cref="FieldRVA.Data"/>.</param>
      /// <returns>The <paramref name="sizeInfo"/>, as the length of the <see cref="FieldRVA.Data"/> is the directly the size of the data written to image.</returns>
      protected override Int32 GetSize( Int32 sizeInfo )
      {
         return sizeInfo;
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}.GetValueForTableStreamFromSize"/> method.
      /// </summary>
      /// <param name="currentRVA">The current RVA.</param>
      /// <param name="sizeInfo">The length of <see cref="FieldRVA.Data"/>.</param>
      /// <returns>The <paramref name="currentRVA"/>, as the <see cref="FieldRVA.Data"/> is will be written directly to the image.</returns>
      protected override TRVA GetValueForTableStreamFromSize( TRVA currentRVA, Int32 sizeInfo )
      {
         return currentRVA;
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}.GetValueForTableStreamFromRow"/> method.
      /// </summary>
      /// <param name="rowIndex">The row index.</param>
      /// <param name="row">The <see cref="FieldRVA"/> row.</param>
      /// <returns>The value <c>0</c>, since this method is called when row or <see cref="FieldRVA.Data"/> is <c>null</c>.</returns>
      protected override TRVA GetValueForTableStreamFromRow( Int32 rowIndex, FieldRVA row )
      {
         return 0;
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}.WriteData"/> method.
      /// </summary>
      /// <param name="row">The <see cref="FieldRVA"/> row.</param>
      /// <param name="sizeInfo">The length of <see cref="FieldRVA.Data"/>.</param>
      /// <param name="array">The array to write data to.</param>
      /// <remarks>
      /// This method copies the <see cref="FieldRVA.Data"/> array directly to given <paramref name="array"/>.
      /// </remarks>
      protected override void WriteData( FieldRVA row, Int32 sizeInfo, Byte[] array )
      {
         if ( sizeInfo > 0 )
         {
            var idx = 0;
            array.BlockCopyFrom( ref idx, row.Data );
         }
      }
   }

   /// <summary>
   /// This class specializes <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}"/> to implement writing the <see cref="ManifestResource.EmbeddedData"/> for embedded <see cref="ManifestResource"/>s.
   /// </summary>
   public class SectionPartFunctionality_EmbeddedManifests : SectionPartFunctionalityWithDataReferenceTargetsImpl<ManifestResource, SectionPartFunctionality_EmbeddedManifests.ManifestResourceSizeInfo>
   {
      /// <summary>
      /// This struct contains information about the size of the serialized <see cref="ManifestResource.EmbeddedData"/> of embedded <see cref="ManifestResource"/>.
      /// </summary>
      public struct ManifestResourceSizeInfo
      {
         /// <summary>
         /// Create a new instance of <see cref="ManifestResourceSizeInfo"/>.
         /// </summary>
         /// <param name="byteCount">The size of the data to write.</param>
         /// <param name="startRVA">The RVA of the start of this section part.</param>
         /// <param name="currentRVA">The current RVA.</param>
         public ManifestResourceSizeInfo( Int32 byteCount, TRVA startRVA, TRVA currentRVA )
         {
            this.ByteCount = byteCount;
            this.PrePadding = (Int32) ( currentRVA.RoundUpI64( ALIGNMENT ) - currentRVA );
            this.Offset = (Int32) ( currentRVA - startRVA + this.PrePadding );
         }

         /// <summary>
         /// Gets the size, in bytes, of the data to write.
         /// </summary>
         /// <value>The size, in bytes, of the data to write.</value>
         public Int32 ByteCount { get; }

         /// <summary>
         /// Gets the size, in bytes, of zero padding before the data.
         /// </summary>
         /// <value>The size, in bytes, of zero padding before the data.</value>
         public Int32 PrePadding { get; }

         /// <summary>
         /// Gets the offset of data from the start of the section.
         /// </summary>
         /// <value>The offset of data from the start of the section.</value>
         public Int32 Offset { get; }
      }

      private const Int32 ALIGNMENT = 0x08;

      /// <summary>
      /// Creates a new instance of <see cref="SectionPartFunctionality_EmbeddedManifests"/>.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/> to use to obtain <see cref="ManifestResource"/>s from <see cref="CILMetaData.ManifestResources"/>.</param>
      /// <param name="columnIndex">The column index of <see cref="ManifestResource"/>. Should be left at zero.</param>
      /// <remarks>
      /// Embedded manifests must be written as continous chunk, therefore the are no <c>min</c> or <c>max</c> parameters, like in <see cref="SectionPartFunctionality_MethodIL"/> or <see cref="SectionPartFunctionality_FieldRVA"/> constructors.
      /// </remarks>
      public SectionPartFunctionality_EmbeddedManifests( CILMetaData md, Int32 columnIndex = 0 )
         : base( ALIGNMENT, md.ManifestResources, columnIndex, 0, -1 )
      {
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}.GetSizeInfo"/> method.
      /// </summary>
      /// <param name="rowIndex">The row index.</param>
      /// <param name="row">The <see cref="ManifestResource"/> row.</param>
      /// <param name="currentOffset">The current offset.</param>
      /// <param name="currentRVA">The current RVA.</param>
      /// <param name="startOffset">The start offset of this section part.</param>
      /// <param name="startRVA">The start RVA of this section part.</param>
      /// <returns>The <see cref="ManifestResourceSizeInfo"/>, if <see cref="CAMPhysical::E_CILPhysical.IsEmbeddedResource"/> method returns <c>true</c> for <paramref name="row"/>. Otherwise, returns <c>null</c>.</returns>
      protected override ManifestResourceSizeInfo? GetSizeInfo( Int32 rowIndex, ManifestResource row, Int64 currentOffset, TRVA currentRVA, Int64 startOffset, TRVA startRVA )
      {
         ManifestResourceSizeInfo? retVal;
         if ( row.IsEmbeddedResource() )
         {
            retVal = new ManifestResourceSizeInfo(
               sizeof( Int32 ) + row.EmbeddedData.GetLengthOrDefault(),
               startRVA,
               currentRVA
               );
         }
         else
         {
            retVal = null;
         }

         return retVal;
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}.GetSize"/> method.
      /// </summary>
      /// <param name="sizeInfo">The <see cref="ManifestResourceSizeInfo"/>.</param>
      /// <returns>The sum of <see cref="ManifestResourceSizeInfo.PrePadding"/> and <see cref="ManifestResourceSizeInfo.ByteCount"/> of <paramref name="sizeInfo"/>.</returns>
      protected override Int32 GetSize( ManifestResourceSizeInfo sizeInfo )
      {
         return sizeInfo.PrePadding + sizeInfo.ByteCount;
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}.GetValueForTableStreamFromSize"/> method.
      /// </summary>
      /// <param name="currentRVA">The current RVA.</param>
      /// <param name="sizeInfo">The <see cref="ManifestResourceSizeInfo"/>.</param>
      /// <returns>The <see cref="ManifestResourceSizeInfo.Offset"/> of <paramref name="sizeInfo"/>, as the value written to table stream for embedded manifest resources is offset from the RVA of <see cref="CLIHeader.Resources"/> data directory.</returns>
      protected override TRVA GetValueForTableStreamFromSize( TRVA currentRVA, ManifestResourceSizeInfo sizeInfo )
      {
         return sizeInfo.Offset;
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}.GetValueForTableStreamFromRow"/> method.
      /// </summary>
      /// <param name="rowIndex">The row index.</param>
      /// <param name="row">The <see cref="ManifestResource"/> row.</param>
      /// <returns>The value of <see cref="ManifestResource.Offset"/> of <paramref name="row"/>, as the value written to table stream foor non-embedded manifest resources is directly the specified offset.</returns>
      protected override TRVA GetValueForTableStreamFromRow( Int32 rowIndex, ManifestResource row )
      {
         return row.Offset;
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWithDataReferenceTargetsImpl{TRow, TSizeInfo}.WriteData"/> method.
      /// </summary>
      /// <param name="row">The <see cref="ManifestResource"/> row.</param>
      /// <param name="sizeInfo">The <see cref="ManifestResourceSizeInfo"/>.</param>
      /// <param name="array">The array to write data to.</param>
      /// <remarks>
      /// This method first zeroes out the <see cref="ManifestResourceSizeInfo.PrePadding"/> amount of bytes, then writes the length of <see cref="ManifestResource.EmbeddedData"/> array, and then copies that array to given byte <paramref name="array"/>.
      /// </remarks>
      protected override void WriteData( ManifestResource row, ManifestResourceSizeInfo sizeInfo, Byte[] array )
      {
         var data = row.EmbeddedData;
         var idx = 0;
         array
            .ZeroOut( ref idx, sizeInfo.PrePadding )
            .WriteInt32LEToBytes( ref idx, data.GetLengthOrDefault() );
         if ( !data.IsNullOrEmpty() )
         {
            array.BlockCopyFrom( ref idx, data );
         }
      }
   }

   /// <summary>
   /// This class combines information required to write a single <see cref="SectionPartFunctionality"/> in its <see cref="SectionPartFunctionality.WriteData"/> method.
   /// </summary>
   /// <remarks>
   /// The typical usecase for using this class is to write data to the array of <see cref="ArrayHelper"/>, and then calling <see cref="WriteCallback"/> specifying the amount of bytes to write from the array of <see cref="ArrayHelper"/> to the stream.
   /// </remarks>
   public class SectionPartWritingArgs
   {
      /// <summary>
      /// Creates a new instance of <see cref="SectionPartWritingArgs"/> with given parameters.
      /// </summary>
      /// <param name="writeCallback">The callback to use when it is needed to write specific amount of bytes from <paramref name="array"/> to stream.</param>
      /// <param name="array">The auxiliary array to use to write data to.</param>
      /// <param name="partSize">The size, in bytes, of this section part.</param>
      /// <param name="writingStatus">The <see cref="DefaultWritingStatus"/>.</param>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="writeCallback"/>, <paramref name="array"/>, or <paramref name="writingStatus"/> is <c>null</c>.</exception>
      public SectionPartWritingArgs(
         Action<Int32> writeCallback,
         ResizableArray<Byte> array,
         Int32 partSize,
         DefaultWritingStatus writingStatus
         )
      {
         ArgumentValidator.ValidateNotNull( "Write callback", writeCallback );
         ArgumentValidator.ValidateNotNull( "Array", array );
         ArgumentValidator.ValidateNotNull( "Writing status", writingStatus );

         this.WriteCallback = writeCallback;
         this.ArrayHelper = array;
         this.PartSize = partSize;
         this.WritingStatus = writingStatus;
      }

      /// <summary>
      /// Gets the callback which will use the array of <see cref="ArrayHelper"/> to write specific amount of bytes to the stream.
      /// </summary>
      /// <value>The callback which will use the array of <see cref="ArrayHelper"/> to write specific amount of bytes to the stream.</value>
      /// <remarks>
      /// This callback provides more controlled way of writing data as compared to revealing the actual <see cref="Stream"/> object.
      /// The callback will throw <see cref="InvalidOperationException"/> if the total byte count for this section part would exceed the <see cref="PartSize"/>.
      /// </remarks>
      public Action<Int32> WriteCallback { get; }

      /// <summary>
      /// Gets the auxiliary array to write the data to.
      /// </summary>
      /// <value>The auxiliary array to write the data to.</value>
      public ResizableArray<Byte> ArrayHelper { get; }

      /// <summary>
      /// Gets the size of this section part, in bytes.
      /// </summary>
      /// <value>The size of this section part, in bytes.</value>
      public Int32 PartSize { get; } // TODO maybe replace this with public SectionPart Part { get; } ?

      /// <summary>
      /// Gets the <see cref="DefaultWritingStatus"/>.
      /// </summary>
      /// <value>The <see cref="DefaultWritingStatus"/>.</value>
      public DefaultWritingStatus WritingStatus { get; }
   }

   /// <summary>
   /// This class represents the concrete information about some continous part of the image section.
   /// Unlike <see cref="SectionPartFunctionality"/>, the size, offset, and RVA of this section part have already been calculated.
   /// </summary>
   public class SectionPart
   {
      /// <summary>
      /// Creates a new instance of <see cref="SectionPart"/>, representing contents of given <see cref="SectionPartFunctionality"/>.
      /// </summary>
      /// <param name="functionality">The <see cref="SectionPartFunctionality"/>.</param>
      /// <param name="size">The size of this section part, in bytes.</param>
      /// <param name="offset">The offset of this section part, in bytes.</param>
      /// <param name="rva">The RVA of this section part, in bytes.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="functionality"/> is <c>null</c>.</exception>
      public SectionPart(
         SectionPartFunctionality functionality,
         Int32 size,
         Int64 offset,
         TRVA rva
         )
      {
         ArgumentValidator.ValidateNotNull( "Functionality", functionality );

         this.Functionality = functionality;
         this.Size = size;
         this.Offset = offset;
         this.RVA = rva;
      }

      /// <summary>
      /// Gets the <see cref="SectionPartFunctionality"/> that this <see cref="SectionPart"/> represents.
      /// </summary>
      /// <value>The <see cref="SectionPartFunctionality"/> that this <see cref="SectionPart"/> represents.</value>
      public SectionPartFunctionality Functionality { get; }

      /// <summary>
      /// Gets the size of this <see cref="SectionPart"/>, in bytes.
      /// </summary>
      /// <value>The size of this <see cref="SectionPart"/>, in bytes.</value>
      public Int32 Size { get; }

      /// <summary>
      /// Gets the offset of this <see cref="SectionPart"/>, in bytes.
      /// </summary>
      /// <value>The offset of this <see cref="SectionPart"/>, in bytes.</value>
      public Int64 Offset { get; }

      /// <summary>
      /// Gets the RVA of this <see cref="SectionPart"/>.
      /// </summary>
      /// <value>The RVA of this <see cref="SectionPart"/>.</value>
      public TRVA RVA { get; }
   }

   /// <summary>
   /// This class specializes <see cref="SectionPartFunctionalityWithFixedLength"/> in order to write <see cref="CLIHeader"/>.
   /// </summary>
   public class SectionPartFunctionality_CLIHeader : SectionPartFunctionalityWithFixedLength
   {
      internal const Int32 HEADER_SIZE = 0x48;

      /// <summary>
      /// Creates a new instance of <see cref="SectionPartFunctionality_CLIHeader"/>.
      /// </summary>
      public SectionPartFunctionality_CLIHeader()
         : base( 4, true, HEADER_SIZE )
      {
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWriteableToArray.DoWriteData"/> method so that the <see cref="CLIHeader"/> is written to given array, and the <see cref="WritingStatus.PEDataDirectories"/> array is updated to contain the <see cref="DataDirectory"/> at index of <see cref="DataDirectories.CLIHeader"/> for the written <see cref="CLIHeader"/>.
      /// </summary>
      /// <param name="wStatus">The <see cref="DefaultWritingStatus"/>.</param>
      /// <param name="array">The byte array to write <see cref="CLIHeader"/> to.</param>
      /// <returns>Always returns value of <c>true</c>.</returns>
      /// <remarks>
      /// The instance of <see cref="CLIHeader"/> to write is obtained through <see cref="WritingStatus.CLIHeader"/> of given <paramref name="wStatus"/>.
      /// </remarks>
      /// <seealso cref="CAMPhysicalIO::E_CILPhysical.WriteCLIHeader"/>
      protected override Boolean DoWriteData( DefaultWritingStatus wStatus, Byte[] array )
      {
         var imageSections = wStatus.SectionLayouts;
         var idx = 0;
         wStatus.CLIHeader.WriteCLIHeader( array, ref idx );
         wStatus.PEDataDirectories[(Int32) DataDirectories.CLIHeader] = imageSections.GetDataDirectoryForSectionPart( this );

         return true;
      }

   }

   /// <summary>
   /// This class specializes <see cref="SectionPartFunctionalityWithFixedLength"/> in order to write strong name signature.
   /// </summary>
   public class SectionPartFunctionality_StrongNameSignature : SectionPartFunctionalityWithFixedLength
   {
      /// <summary>
      /// Creates a new instance of <see cref="SectionPartFunctionality_StrongNameSignature"/>.
      /// </summary>
      /// <param name="snVars">The <see cref="StrongNameInformation"/>. If <c>null</c>, then this section part will not be written to image.</param>
      /// <param name="machine">The <see cref="ImageFileMachine"/> of the image being written. This is required to calculate the alignment for this section part.</param>
      public SectionPartFunctionality_StrongNameSignature( StrongNameInformation snVars, ImageFileMachine machine )
         : base( machine.RequiresPE64() ? 0x10 : 0x04, snVars != null, snVars?.SignatureSize ?? 0 )
      {
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWriteableToArray.DoWriteData"/> method, but always just returns <c>false</c>.
      /// </summary>
      /// <param name="wStatus">The <see cref="DefaultWritingStatus"/>.</param>
      /// <param name="array">The byte array to use.</param>
      /// <returns>Always returns <c>false</c>.</returns>
      /// <remarks>
      /// By returng <c>false</c>, the <see cref="SectionPartFunctionalityWriteableToArray.WriteData"/> method will zero out the contents.
      /// This is useful in this case, as the actual strong name signature will be written by <see cref="CAMPhysicalIO::E_CILPhysical.WriteMetaDataToStream(WriterFunctionality, Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, Crypto.CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/> method, if needed.
      /// </remarks>
      protected override Boolean DoWriteData( DefaultWritingStatus wStatus, Byte[] array )
      {
         // Don't write actual signature, since we don't have required information. The strong name signature will be written by WriteMetaData implementation.
         return false;
      }
   }

   /// <summary>
   /// This class specializes <see cref="SectionPartFunctionalityWithFixedAlignment"/>, but only acts as a placeholder, since the actual meta data contents will be written by <see cref="CAMPhysicalIO::E_CILPhysical.WriteMetaDataToStream(WriterFunctionality, Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, Crypto.CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/> method.
   /// </summary>
   public class SectionPartFunctionality_MetaData : SectionPartFunctionalityWithFixedAlignment
   {
      private readonly Int32 _size;

      /// <summary>
      /// Creates a new instance of <see cref="SectionPartFunctionality_MetaData"/> with given meta data size, in bytes.
      /// </summary>
      /// <param name="size">The meta data size, in bytes.</param>
      public SectionPartFunctionality_MetaData( Int32 size )
         : base( 0x04, true )
      {
         this._size = size;
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWithFixedAlignment.DoGetDataSize"/> method by returning the meta data size passed as argument to the <see cref="SectionPartFunctionality_MetaData(Int32)"/> constructor.
      /// </summary>
      /// <param name="currentOffset">The current offset.</param>
      /// <param name="currentRVA">The current RVA.</param>
      /// <param name="dataReferences">The <see cref="ColumnValueStorage{TValue}"/> for data reference columns.</param>
      /// <returns>The meta data size passed as argument to the <see cref="SectionPartFunctionality_MetaData(Int32)"/> constructor.</returns>
      protected override Int32 DoGetDataSize( Int64 currentOffset, TRVA currentRVA, ColumnValueStorage<Int64> dataReferences )
      {
         return this._size;
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWithFixedAlignment.WriteData"/> method, but always throws the <see cref="NotSupportedException"/>, since this method is not supposed to be called on this object.
      /// </summary>
      /// <param name="args">The <see cref="SectionPartWritingArgs"/>.</param>
      /// <remarks>
      /// The <see cref="DefaultWriterFunctionality"/> will know never to call this method on this object.
      /// </remarks>
      public override void WriteData( SectionPartWritingArgs args )
      {
         throw new NotSupportedException( "This method should not be called." );
      }
   }

   /// <summary>
   /// This class specializes <see cref="SectionPartFunctionalityWithFixedLength"/> in order to write import address table for managed images.
   /// </summary>
   public class SectionPartFunctionality_ImportAddressTable : SectionPartFunctionalityWithFixedLength
   {
      /// <summary>
      /// Creates a new instance of <see cref="SectionPartFunctionality_ImportAddressTable"/> for given <see cref="ImageFileMachine"/>.
      /// </summary>
      /// <param name="machine">The <see cref="ImageFileMachine"/> of the image being emitted.</param>
      /// <remarks>
      /// The section part will not be present in the final image, if the <see cref="CAMPhysicalIO::E_CILPhysical.RequiresPE64"/> will return <c>true</c> for given <paramref name="machine"/>.
      /// </remarks>
      public SectionPartFunctionality_ImportAddressTable( ImageFileMachine machine )
         : base( 0x04, !machine.RequiresPE64(), 0x08 )
      {
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWriteableToArray.DoWriteData"/> method in order to write import address table, and the <see cref="WritingStatus.PEDataDirectories"/> array is updated to contain the <see cref="DataDirectory"/> at index of <see cref="DataDirectories.ImportAddressTable"/> for the written import data directory.
      /// </summary>
      /// <param name="wStatus">The <see cref="DefaultWritingStatus"/>.</param>
      /// <param name="array">The byte array to write import address table to.</param>
      /// <returns>The value <c>true</c> if any data was written, <c>false</c> otherwise.</returns>
      /// <remarks>
      /// In order to obtain the RVA of import name, this method will search for <see cref="SectionPart"/> that is related to <see cref="SectionPartFunctionality_ImportDirectory"/> from <see cref="ImageSectionsInfo"/> of the supplied <see cref="DefaultWritingStatus"/>.
      /// </remarks>
      protected override Boolean DoWriteData( DefaultWritingStatus wStatus, Byte[] array )
      {
         var imageSections = wStatus.SectionLayouts;
         var importDir = imageSections.GetSectionPartWithFunctionalityOfType<SectionPartFunctionality_ImportDirectory>();
         var retVal = importDir != null;
         if ( retVal )
         {
            var idx = 0;
            array
               .WriteInt32LEToBytes( ref idx, ( (SectionPartFunctionality_ImportDirectory) importDir.Functionality ).CorMainRVA ) // RVA of _CorDll/ExeMain
               .WriteInt32LEToBytes( ref idx, 0 ); // Terminating entry

            wStatus.PEDataDirectories[(Int32) DataDirectories.ImportAddressTable] = imageSections.GetDataDirectoryForSectionPart( this );
         }
         return retVal;
      }
   }

   /// <summary>
   /// This class specializes <see cref="SectionPartFunctionalityWriteableToArray"/> in order to write import directory for managed images.
   /// </summary>
   public class SectionPartFunctionality_ImportDirectory : SectionPartFunctionalityWriteableToArray
   {

      internal const String HINTNAME_FOR_DLL = "_CorDllMain";
      internal const String HINTNAME_FOR_EXE = "_CorExeMain";

      private readonly String _functionName;
      private readonly String _moduleName;

      private UInt32 _lookupTableRVA;
      private UInt32 _paddingBeforeString;
      private UInt32 _corMainRVA;
      private UInt32 _mscoreeRVA;

      /// <summary>
      /// Creates a new instance of <see cref="SectionPartFunctionality_ImportDirectory"/> for given <see cref="ImageFileMachine"/> and given imported module and function names.
      /// </summary>
      /// <param name="machine">The <see cref="ImageFileMachine"/> of the image being emitted.</param>
      /// <param name="functionName">The name of the function to import. Will default to <c>"_CorExeMain"</c> if <paramref name="isExecutable"/> is <c>true</c>, and to <c>"_CorDllMain"</c> if <paramref name="isExectuable"/> is <c>false</c>.</param>
      /// <param name="moduleName">The name of the module to import. Will default to <c>"mscoree.dll"</c> if not given.</param>
      /// <param name="isExecutable">Whether this image is executable.</param>
      /// <remarks>
      /// The section part will not be present in the final image, if the <see cref="CAMPhysicalIO::E_CILPhysical.RequiresPE64"/> will return <c>true</c> for given <paramref name="machine"/>.
      /// </remarks>
      public SectionPartFunctionality_ImportDirectory( ImageFileMachine machine, String functionName, String moduleName, Boolean isExecutable )
         : base( 0x04, !machine.RequiresPE64() )
      {
         if ( String.IsNullOrEmpty( moduleName ) )
         {
            moduleName = "mscoree.dll";
         }

         if ( String.IsNullOrEmpty( functionName ) )
         {
            functionName = isExecutable ? HINTNAME_FOR_EXE : HINTNAME_FOR_DLL;
         }

         this._moduleName = moduleName;
         this._functionName = functionName;


      }

      /// <summary>
      /// This method implements <see cref="SectionPartFunctionalityWithFixedAlignment.DoGetDataSize"/> by calculating the size of this import directory, in bytes.
      /// </summary>
      /// <param name="currentOffset">The current offset.</param>
      /// <param name="currentRVA">The current RVA.</param>
      /// <param name="dataReferences">The <see cref="ColumnValueStorage{TValue}"/> for data reference columns.</param>
      /// <returns>The size of this import directory, in bytes.</returns>
      protected override Int32 DoGetDataSize( Int64 currentOffset, TRVA currentRVA, ColumnValueStorage<Int64> dataReferences )
      {
         var startRVA = (UInt32) currentRVA;
         var len = 0x28u; // Import directory actual size

         this._lookupTableRVA = startRVA + len;

         len += 0x08; // Chunk size
         var endRVA = startRVA + len;

         // Padding before strings
         this._paddingBeforeString = endRVA.RoundUpU32( 0x10 ) - endRVA;
         len += this._paddingBeforeString;

         // Hint + _CorDll/ExeMain string
         this._corMainRVA = startRVA + len;
         len += 2 + (UInt32) this._functionName.Length + 1;

         // mscoree string
         this._mscoreeRVA = startRVA + len;
         len += (UInt32) this._moduleName.Length + 1; // 0xC

         // Last byte
         len++;

         return (Int32) len;
      }

      /// <summary>
      /// Returns the RVA of the imported function name (typically <c>"_CorExeMain"</c> or <c>"_CorDllMain"</c>).
      /// </summary>
      /// <value>The RVA of the imported function name.</value>
      public Int32 CorMainRVA
      {
         get
         {
            return (Int32) this._corMainRVA;
         }
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWriteableToArray.DoWriteData"/> method by writing the contents of this import directory, and the <see cref="WritingStatus.PEDataDirectories"/> array is updated to contain the <see cref="DataDirectory"/> at index of <see cref="DataDirectories.ImportTable"/> for the written import directory.
      /// </summary>
      /// <param name="wStatus">The <see cref="DefaultWritingStatus"/>.</param>
      /// <param name="array">The byte array to write import directory to.</param>
      /// <returns>The value <c>true</c> if any data was written, <c>false</c> otherwise.</returns>
      /// <remarks>
      /// In order to obtain the RVA of import address table, this method will search for <see cref="SectionPart"/> that is related to <see cref="SectionPartFunctionality_ImportAddressTable"/> from <see cref="ImageSectionsInfo"/> of the supplied <see cref="DefaultWritingStatus"/>.
      /// </remarks>
      protected override Boolean DoWriteData( DefaultWritingStatus wStatus, Byte[] array )
      {
         var imageSections = wStatus.SectionLayouts;
         var addressTable = imageSections.GetSectionPartWithFunctionalityOfType<SectionPartFunctionality_ImportAddressTable>();

         var retVal = addressTable != null;
         if ( retVal )
         {
            var idx = 0;
            array
               // Import directory
               .WriteUInt32LEToBytes( ref idx, this._lookupTableRVA )
               .WriteInt32LEToBytes( ref idx, 0 ) // TimeDateStamp
               .WriteInt32LEToBytes( ref idx, 0 ) // ForwarderChain
               .WriteUInt32LEToBytes( ref idx, this._mscoreeRVA ) // Name of module
               .WriteUInt32LEToBytes( ref idx, (UInt32) addressTable.RVA ) // Address table RVA
               .WriteInt64LEToBytes( ref idx, 0 ) // ?
               .WriteInt64LEToBytes( ref idx, 0 ) // ?
               .WriteInt32LEToBytes( ref idx, 0 ) // ?

               // Import lookup table
               .WriteUInt32LEToBytes( ref idx, this._corMainRVA ) // 1st and only entry - _CorDll/ExeMain
               .WriteInt32LEToBytes( ref idx, 0 ) // 2nd entry - zeroes

               // Padding before entries
               .ZeroOut( ref idx, (Int32) this._paddingBeforeString )

               // Function data: _CorDll/ExeMain
               .WriteInt16LEToBytes( ref idx, 0 ) // Hint
               .WriteASCIIString( ref idx, this._functionName, true )

               // Module data: mscoree.dll
               .WriteASCIIString( ref idx, this._moduleName, true )
               .WriteByteToBytes( ref idx, 0 );

            wStatus.PEDataDirectories[(Int32) DataDirectories.ImportTable] = imageSections.GetDataDirectoryForSectionPart( this );
         }

         return retVal;
      }
   }

   /// <summary>
   /// This class specializes <see cref="SectionPartFunctionalityWithFixedLength"/> in order to write native startup code stub for managed images.
   /// </summary>
   public class SectionPartFunctionality_StartupCode : SectionPartFunctionalityWithFixedLength
   {

      private const Int32 ALIGNMENT = 0x04;
      private const Int32 PADDING = 2;

      /// <summary>
      /// Creates a new instance for <see cref="SectionPartFunctionality_StartupCode"/> for given <see cref="ImageFileMachine"/>.
      /// </summary>
      /// <param name="machine">The <see cref="ImageFileMachine"/> of the image being emitted.</param>
      /// <remarks>
      /// The section part will not be present in the final image, if the <see cref="CAMPhysicalIO::E_CILPhysical.RequiresPE64"/> will return <c>true</c> for given <paramref name="machine"/>.
      /// </remarks>
      public SectionPartFunctionality_StartupCode( ImageFileMachine machine )
         : base( ALIGNMENT, !machine.RequiresPE64(), 0x08 )
      {
      }

      /// <summary>
      /// Returns the offset within this section part, where the address of the called function is.
      /// </summary>
      /// <value>The offset within this section part, where the address of the called function is.</value>
      /// <remarks>
      /// This value is used by <see cref="SectionPartFunctionality_RelocDirectory"/> in order to keep track of the relocatable addresses.
      /// </remarks>
      public Int32 EntryPointInstructionAddressOffset
      {
         get
         {
            return PADDING + 2;
         }
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWriteableToArray.DoWriteData"/> method by writing the contents of this native startup code stub, and the <see cref="WritingStatus.EntryPointRVA"/> array is updated to be the RVA of this startup code.
      /// </summary>
      /// <param name="wStatus">The <see cref="DefaultWritingStatus"/>.</param>
      /// <param name="array">The byte array to write native startup code stub to.</param>
      /// <returns>The value <c>true</c> if any data was written, <c>false</c> otherwise.</returns>
      /// <remarks>
      /// In order to obtain the RVA of import address table, this method will search for <see cref="SectionPart"/> that is related to <see cref="SectionPartFunctionality_ImportAddressTable"/> from <see cref="ImageSectionsInfo"/> of the supplied <see cref="DefaultWritingStatus"/>.
      /// </remarks>
      protected override Boolean DoWriteData( DefaultWritingStatus wStatus, Byte[] array )
      {
         var sectionLayouts = wStatus.SectionLayouts;
         var addressTable = sectionLayouts.GetSectionPartWithFunctionalityOfType<SectionPartFunctionality_ImportAddressTable>();
         var retVal = addressTable != null;
         if ( retVal )
         {
            var idx = 0;
            array
               .ZeroOut( ref idx, PADDING ) // Padding - 2 zero bytes
               .WriteUInt16LEToBytes( ref idx, 0x25FF ) // JMP
               .WriteUInt32LEToBytes( ref idx, (UInt32) ( wStatus.ImageBase + addressTable.RVA ) ); // First entry of address table = RVA of _CorDll/ExeMain

            wStatus.EntryPointRVA = (Int32) ( sectionLayouts.GetSectionPartFor( this ).RVA + PADDING );
         }
         return retVal;
      }
   }

   /// <summary>
   /// This class specializes <see cref="SectionPartFunctionalityWithFixedLength"/> in order to write relocation directory for managed images.
   /// </summary>
   public class SectionPartFunctionality_RelocDirectory : SectionPartFunctionalityWithFixedLength
   {
      private const Int32 SIZE = 0x0C;
      private const UInt32 RELOCATION_PAGE_MASK = 0x0FFF; // ECMA-335, p. 282
      private const UInt16 RELOCATION_FIXUP_TYPE = 0x3; // ECMA-335, p. 282

      /// <summary>
      /// Creates a new instance for <see cref="SectionPartFunctionality_RelocDirectory"/> for given <see cref="ImageFileMachine"/>.
      /// </summary>
      /// <param name="machine">The <see cref="ImageFileMachine"/> of the image being emitted.</param>
      /// <remarks>
      /// The section part will not be present in the final image, if the <see cref="CAMPhysicalIO::E_CILPhysical.RequiresPE64"/> will return <c>true</c> for given <paramref name="machine"/>.
      /// </remarks>
      public SectionPartFunctionality_RelocDirectory( ImageFileMachine machine )
         : base( 0x04, !machine.RequiresPE64(), SIZE )
      {

      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWriteableToArray.DoWriteData"/> method by writing the contents of this relocation directory, and the <see cref="WritingStatus.PEDataDirectories"/> array is updated to contain the <see cref="DataDirectory"/> at index of <see cref="DataDirectories.BaseRelocationTable"/> for the written relocation directory.
      /// </summary>
      /// <param name="wStatus">The <see cref="DefaultWritingStatus"/>.</param>
      /// <param name="array">The byte array to write relocation directory to.</param>
      /// <returns>The value <c>true</c> if any data was written, <c>false</c> otherwise.</returns>
      /// <remarks>
      /// In order to obtain the RVA of used address of the native code startup stub, this method will search for <see cref="SectionPart"/> that is related to <see cref="SectionPartFunctionality_StartupCode"/> from <see cref="ImageSectionsInfo"/> of the supplied <see cref="DefaultWritingStatus"/>.
      /// </remarks>
      protected override Boolean DoWriteData( DefaultWritingStatus wStatus, Byte[] array )
      {
         var imageSections = wStatus.SectionLayouts;
         var startupCode = imageSections.GetSectionPartWithFunctionalityOfType<SectionPartFunctionality_StartupCode>();
         var retVal = startupCode != null;
         if ( retVal )
         {
            var idx = 0;
            var rva = (UInt32) ( startupCode.RVA + ( (SectionPartFunctionality_StartupCode) startupCode.Functionality ).EntryPointInstructionAddressOffset );
            array
               .WriteUInt32LEToBytes( ref idx, rva & ( ~RELOCATION_PAGE_MASK ) ) // Page RVA
               .WriteInt32LEToBytes( ref idx, SIZE ) // Block size
               .WriteUInt16LEToBytes( ref idx, (UInt16) ( ( RELOCATION_FIXUP_TYPE << 12 ) | ( rva & RELOCATION_PAGE_MASK ) ) ) // Type (high 4 bits) + Offset (lower 12 bits) + dummy entry (16 bits)
               .WriteUInt16LEToBytes( ref idx, 0 ); // Terminating entry

            wStatus.PEDataDirectories[(Int32) DataDirectories.BaseRelocationTable] = imageSections.GetDataDirectoryForSectionPart( this );
         }
         return retVal;
      }
   }

   /// <summary>
   /// This class specializes <see cref="SectionPartFunctionalityWithFixedLength"/> in order to write debug information.
   /// </summary>
   /// <seealso cref="DebugInformation"/>
   public class SectionPartFunctionality_DebugDirectory : SectionPartFunctionalityWithFixedLength
   {
      private const Int32 ALIGNMENT = 0x04;
      private const Int32 HEADER_SIZE = 0x1C;

      private readonly WritingOptions_Debug _options;

      /// <summary>
      /// Creates a new <see cref="SectionPartFunctionality_DebugDirectory"/> from given <see cref="WritingOptions_Debug"/>.
      /// </summary>
      /// <param name="options">The <see cref="WritingOptions_Debug"/>. May be <c>null</c>.</param>
      /// <remarks>
      /// The section part will not be present in the final image, if the <paramref name="options"/> is <c>null</c>, or its <see cref="WritingOptions_Debug.DebugData"/> is <c>null</c> or empty.
      /// </remarks>
      public SectionPartFunctionality_DebugDirectory( WritingOptions_Debug options )
         : base( ALIGNMENT, !( options?.DebugData ).IsNullOrEmpty(), ( HEADER_SIZE + ( options?.DebugData?.Length ?? 0 ) ) )
      {
         this._options = options;
      }

      /// <summary>
      /// Implements the <see cref="SectionPartFunctionalityWriteableToArray.DoWriteData"/> method by writing the contents of this relocation directory, and the <see cref="WritingStatus.PEDataDirectories"/> array is updated to contain the <see cref="DataDirectory"/> at index of <see cref="DataDirectories.Debug"/> for the written debug information.
      /// </summary>
      /// <param name="wStatus">The <see cref="DefaultWritingStatus"/>.</param>
      /// <param name="array">The byte array to write debug information to.</param>
      /// <returns>The value <c>true</c> if any data was written, <c>false</c> otherwise.</returns>
      protected override Boolean DoWriteData( DefaultWritingStatus wStatus, Byte[] array )
      {
         var dbgData = this._options?.DebugData;
         var retVal = !dbgData.IsNullOrEmpty();
         if ( retVal )
         {
            var imageSections = wStatus.SectionLayouts;
            var thisPart = imageSections.GetSectionPartFor( this );
            var dbgOptions = this._options;
            var dataOffset = (UInt32) ( thisPart.Offset + HEADER_SIZE );
            var dataRVA = (UInt32) ( thisPart.RVA + HEADER_SIZE );
            var debugInfo = new DebugInformation(
               dbgOptions.Characteristics,
               (UInt32) dbgOptions.Timestamp,
               (UInt16) dbgOptions.MajorVersion,
               (UInt16) dbgOptions.MinorVersion,
               dbgOptions.DebugType,
               (UInt32) dbgData.Length,
               dataRVA,
               dataOffset,
               dbgData.ToArrayProxy().CQ
               );
            var idx = 0;
            debugInfo.WriteDebugInformation( array, ref idx );
            wStatus.PEDataDirectories[(Int32) DataDirectories.Debug] = thisPart.GetDataDirectory();
            wStatus.DebugInformation = debugInfo;
         }
         return retVal;
      }
   }

   /// <summary>
   /// This class provides implementation of the <see cref="AbstractWriterStreamHandler"/> suitable for most meta data stream handlers participating in writing process.
   /// </summary>
   public abstract class AbstractWriterStreamHandlerImpl : AbstractWriterStreamHandler
   {
      private readonly UInt32 _startingIndex;

      /// <summary>
      /// Creates a new <see cref="AbstractWriterStreamHandlerImpl"/> with given index as index within the meta data stream, where to start writing data.
      /// </summary>
      /// <param name="startingIndex">The index within the meta data stream, where to start writing data.</param>
      /// <remarks>
      /// The value of <paramref name="startingIndex"/> will be used to determine whether this stream has been accessed, in <see cref="Accessed"/> property.
      /// </remarks>
      [CLSCompliant( false )]
      protected AbstractWriterStreamHandlerImpl( UInt32 startingIndex )
      {
         this._startingIndex = startingIndex;
         this.CurrentSize = startingIndex;
      }

      /// <summary>
      /// This class leaves out the implementation of <see cref="AbstractMetaDataStreamHandler.StreamName"/> to the subclasses.
      /// </summary>
      /// <value>The name of this <see cref="AbstractWriterStreamHandlerImpl"/>.</value>
      public abstract String StreamName { get; }

      /// <summary>
      /// This method implements the <see cref="AbstractWriterStreamHandler.WriteStream"/> method.
      /// </summary>
      /// <param name="stream">The stream where to write the contents to.</param>
      /// <param name="array">The auxiliary byte array to use.</param>
      /// <param name="dataReferences">The <see cref="DataReferencesInfo"/> containing values of the data reference columns.</param>
      public virtual void WriteStream(
         Stream stream,
         ResizableArray<Byte> array,
         DataReferencesInfo dataReferences
         )
      {
         if ( this.Accessed )
         {
            this.DoWriteStream( stream, array );
            var size = this.CurrentSize;
            var padding = (Int32) ( size.RoundUpU32( 4 ) - size );
            if ( padding > 0 )
            {
               array.CurrentMaxCapacity = padding;
               var idx = 0;
               array.Array.ZeroOut( ref idx, padding );
               stream.Write( array.Array, padding );
            }
         }
      }

      /// <summary>
      /// Gets the size of the stream.
      /// </summary>
      /// <value>The size of the stream.</value>
      /// <remarks>
      /// The returned size will always be aligned up by <c>4</c>.
      /// </remarks>
      public Int32 StreamSize
      {
         get
         {
            return (Int32) this.CurrentSize.RoundUpU32( 4 );
         }
      }

      /// <summary>
      /// Gets the value indicating whether this stream has anything stored in it.
      /// </summary>
      /// <value>The value indicating whether this stream has anything stored in it.</value>
      /// <remarks>
      /// The check is performed by comparing <see cref="CurrentSize"/> to the starting index given to the <see cref="AbstractWriterStreamHandlerImpl(UInt32)"/> constructor.
      /// If the <see cref="CurrentSize"/> is larger, this stream is considered to have something in storage.
      /// </remarks>
      public Boolean Accessed
      {
         get
         {
            return this.CurrentSize > this._startingIndex;
         }
      }

      /// <summary>
      /// Gets or sets the current size of this stream.
      /// </summary>
      /// <value>The current size of this stream.</value>
      /// <remarks>
      /// Subclasses should update this value as appropriate.
      /// </remarks>
      [CLSCompliant( false )]
      protected UInt32 CurrentSize { get; set; }

      /// <summary>
      /// This is abstract method to perform actual writing of this meta data stream.
      /// </summary>
      /// <param name="stream">The <see cref="Stream"/> to write contents to.</param>
      /// <param name="array">The auxiliary array to use.</param>
      protected abstract void DoWriteStream( Stream stream, ResizableArray<Byte> array );
   }

   /// <summary>
   /// This class provides default implementation for <see cref="WriterBLOBStreamHandler"/>.
   /// </summary>
   public class DefaultWriterBLOBStreamHandler : AbstractWriterStreamHandlerImpl, WriterBLOBStreamHandler
   {
      private readonly IDictionary<Byte[], UInt32> _blobIndices;
      private readonly IList<Byte[]> _blobs;

      /// <summary>
      /// Creates a new instance of <see cref="DefaultWriterBLOBStreamHandler"/>.
      /// </summary>
      public DefaultWriterBLOBStreamHandler()
         : base( 1 )
      {
         this._blobIndices = new Dictionary<Byte[], UInt32>( ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer );
         this._blobs = new List<Byte[]>();
      }

      /// <summary>
      /// Gets the name of the BLOB stream, which is always <c>"#Blobs"</c>.
      /// </summary>
      /// <value>The string value <c>"#Blobs"</c>.</value>
      public override String StreamName
      {
         get
         {
            return MetaDataConstants.BLOB_STREAM_NAME;
         }
      }

      /// <summary>
      /// This method implements <see cref="WriterBLOBStreamHandler.RegisterBLOB"/>.
      /// </summary>
      /// <param name="blob">The BLOB, as byte array, that should be stored in this BLOB stream.</param>
      /// <returns>The index within this stream where the given <paramref name="blob"/> is located.</returns>
      public Int32 RegisterBLOB( Byte[] blob )
      {
         UInt32 result;
         if ( blob == null )
         {
            result = 0;
         }
         else
         {
            if ( !this._blobIndices.TryGetValue( blob, out result ) )
            {
               result = this.CurrentSize;
               this._blobIndices.Add( blob, result );
               this._blobs.Add( blob );
               this.CurrentSize += (UInt32) blob.Length + (UInt32) BitUtils.GetEncodedUIntSize( blob.Length );
            }
         }

         return (Int32) result;
      }

      /// <summary>
      /// Implements the <see cref="AbstractWriterStreamHandlerImpl.DoWriteStream"/> with code that will write all registered BLOBs to the given stream.
      /// </summary>
      /// <param name="stream">The <see cref="Stream"/> to write this BLOB stream to.</param>
      /// <param name="array">The auxiliary array to use.</param>
      protected override void DoWriteStream(
         Stream stream,
         ResizableArray<Byte> array
         )
      {
         stream.WriteByte( 0 );
         var idx = 0;
         if ( this._blobs.Count > 0 )
         {
            foreach ( var blob in this._blobs )
            {
               idx = 0;
               array.AddCompressedUInt32( ref idx, blob.Length );
               stream.Write( array.Array, idx );
               stream.Write( blob );
            }
         }
      }

   }

   /// <summary>
   /// This class provides default implementation for <see cref="WriterGUIDStreamHandler"/>.
   /// </summary>
   public class DefaultWriterGuidStreamHandler : AbstractWriterStreamHandlerImpl, WriterGUIDStreamHandler
   {
      private readonly IDictionary<Guid, UInt32> _guids;

      /// <summary>
      /// Creates a new instance of <see cref="DefaultWriterGuidStreamHandler"/>.
      /// </summary>
      public DefaultWriterGuidStreamHandler()
         : base( 0 )
      {
         this._guids = new Dictionary<Guid, UInt32>();
      }

      /// <summary>
      /// Gets the name of the GUID stream, which is always <c>"#GUID"</c>.
      /// </summary>
      /// <value>The string value <c>"#GUID"</c>.</value>
      public override String StreamName
      {
         get
         {
            return MetaDataConstants.GUID_STREAM_NAME;
         }
      }

      /// <summary>
      /// This method implements <see cref="WriterGUIDStreamHandler.RegisterGUID"/>.
      /// </summary>
      /// <param name="guid">The nullable <see cref="Guid"/> that should be stored in this GUID stream.</param>
      /// <returns>The index within this stream where the given <paramref name="guid"/> is located.</returns>
      public Int32 RegisterGUID( Guid? guid )
      {
         UInt32 result;
         if ( guid.HasValue )
         {
            result = this._guids.GetOrAdd_NotThreadSafe( guid.Value, g =>
            {
               var retVal = (UInt32) this._guids.Count + 1;
               this.CurrentSize += MetaDataConstants.GUID_SIZE;
               return retVal;
            } );
         }
         else
         {
            result = 0;
         }

         return (Int32) result;
      }

      /// <summary>
      /// Implements the <see cref="AbstractWriterStreamHandlerImpl.DoWriteStream"/> with code that will write all registered GUIDs to the given stream.
      /// </summary>
      /// <param name="stream">The <see cref="Stream"/> to write this GUID stream to.</param>
      /// <param name="array">The auxiliary array to use.</param>
      protected override void DoWriteStream(
         Stream sink,
         ResizableArray<Byte> array
         )
      {
         foreach ( var kvp in this._guids )
         {
            sink.Write( kvp.Key.ToByteArray() );
         }

      }
   }

   /// <summary>
   /// This class provides default implementation for <see cref="WriterStringStreamHandler"/>.
   /// </summary>
   public abstract class DefaultWriterStringStreamHandlerImpl : AbstractWriterStreamHandlerImpl, WriterStringStreamHandler
   {
      private readonly IDictionary<String, KeyValuePair<UInt32, Int32>> _strings;

      /// <summary>
      /// Creates a new instance of <see cref="DefaultWriterStringStreamHandlerImpl"/> with given <see cref="System.Text.Encoding"/>.
      /// </summary>
      /// <param name="encoding">The encoding that this <see cref="DefaultWriterStringStreamHandlerImpl"/> is deemed to serialize strings with.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="encoding"/> is <c>null</c>.</exception>
      protected DefaultWriterStringStreamHandlerImpl( Encoding encoding )
         : base( 1 )
      {
         ArgumentValidator.ValidateNotNull( "Encoding", encoding );
         this.Encoding = encoding;
         this._strings = new Dictionary<String, KeyValuePair<UInt32, Int32>>();
      }

      /// <summary>
      /// This method implements <see cref="WriterStringStreamHandler.RegisterString"/>.
      /// </summary>
      /// <param name="str">The string that should be stored in this string stream.</param>
      /// <returns>The index within this stream where the given <paramref name="str"/> is located.</returns>
      /// <remarks>
      /// This method will use the <see cref="GetByteCountForString"/> to obtain the actual byte count for a given string.
      /// </remarks>
      public Int32 RegisterString( String str )
      {
         UInt32 result;
         if ( str == null )
         {
            result = 0;
         }
         else
         {
            KeyValuePair<UInt32, Int32> strInfo;
            if ( this._strings.TryGetValue( str, out strInfo ) )
            {
               result = strInfo.Key;
            }
            else
            {
               result = this.CurrentSize;
               var byteCount = this.GetByteCountForString( str );
               this._strings.Add( str, new KeyValuePair<UInt32, Int32>( this.CurrentSize, byteCount ) );
               this.CurrentSize += (UInt32) byteCount;
            }
         }
         return (Int32) result;
      }

      /// <summary>
      /// Leaves the implementation of the <see cref="AbstractStringStreamHandler.StringStreamKind"/> property to subclasses.
      /// </summary>
      /// <value>The <see cref="CAMPhysicalIO::CILAssemblyManipulator.Physical.IO.StringStreamKind"/> of this string stream.</value>
      public abstract StringStreamKind StringStreamKind { get; }

      /// <summary>
      /// Implements the <see cref="AbstractWriterStreamHandlerImpl.DoWriteStream"/> with code that will write all registered strings to the given stream.
      /// </summary>
      /// <param name="stream">The <see cref="Stream"/> to write this string stream to.</param>
      /// <param name="array">The auxiliary array to use.</param>
      /// <remarks>
      /// This method will use <see cref="Serialize"/> to delegate the writing of the string to byte array.
      /// </remarks>
      protected override void DoWriteStream(
         Stream sink,
         ResizableArray<Byte> array
         )
      {
         sink.WriteByte( 0 );
         if ( this._strings.Count > 0 )
         {
            foreach ( var kvp in this._strings )
            {
               var arrayLen = kvp.Value.Value;
               array.CurrentMaxCapacity = arrayLen;
               this.Serialize( kvp.Key, array.Array );
               sink.Write( array.Array, arrayLen );
            }
         }
      }

      /// <summary>
      /// Gets the <see cref="System.Text.Encoding"/> specified to this string stream.
      /// </summary>
      /// <value>The <see cref="System.Text.Encoding"/> specified to this string stream.</value>
      protected Encoding Encoding { get; }

      /// <summary>
      /// This method should calculate the byte count for given string.
      /// </summary>
      /// <param name="str">The given string. Is guaranteed to be non-<c>null</c>.</param>
      /// <returns>The byte count for <paramref name="str"/>.</returns>
      protected abstract Int32 GetByteCountForString( String str );

      /// <summary>
      /// This method should write the given string to given byte array.
      /// </summary>
      /// <param name="str">The string to serialize. Is guaranteed to be non-<c>null</c>.</param>
      /// <param name="array">The byte array to serialize string to. The array will always be at least the size of what <see cref="GetByteCountForString"/> returned for given <paramref name="str"/>.</param>
      protected abstract void Serialize( String str, Byte[] array );
   }

   /// <summary>
   /// This class specializes the <see cref="DefaultWriterStringStreamHandlerImpl"/> to provide default implementation for user string (<c>"#US"</c>) meta data stream.
   /// </summary>
   public class DefaultWriterUserStringStreamHandler : DefaultWriterStringStreamHandlerImpl
   {
      /// <summary>
      /// Creates a new instance of <see cref="DefaultWriterUserStringStreamHandler"/>.
      /// </summary>
      public DefaultWriterUserStringStreamHandler()
         : base( MetaDataConstants.USER_STRING_ENCODING )
      {

      }

      /// <summary>
      /// Gets the name of this string stream, which is always <c>"#US"</c>.
      /// </summary>
      /// <value>The string value <c>"#US"</c>.</value>
      public override String StreamName
      {
         get
         {
            return MetaDataConstants.USER_STRING_STREAM_NAME;
         }
      }

      /// <summary>
      /// Returns the <see cref="StringStreamKind.UserStrings"/>.
      /// </summary>
      /// <value>The <see cref="StringStreamKind.UserStrings"/>.</value>
      public override StringStreamKind StringStreamKind
      {
         get
         {
            return StringStreamKind.UserStrings;
         }
      }

      /// <summary>
      /// Implements the <see cref="DefaultWriterStringStreamHandlerImpl.GetByteCountForString"/> method.
      /// </summary>
      /// <param name="str">The string.</param>
      /// <returns>The byte count for user string, as specified in ECMA-335 standard.</returns>
      protected override Int32 GetByteCountForString( String str )
      {
         var retVal = str.Length * 2 // Each character is 2 bytes
            + 1; // Trailing byte (zero or 1)
         retVal += BitUtils.GetEncodedUIntSize( retVal ); // How many bytes it will take to compress the byte count
         return retVal;
      }

      /// <summary>
      /// Implements the <see cref="DefaultWriterStringStreamHandlerImpl.Serialize"/> method.
      /// </summary>
      /// <param name="str">The string.</param>
      /// <param name="array">The byte array to write <paramref name="str"/> to.</param>
      protected override void Serialize( String str, Byte[] array )
      {
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

   /// <summary>
   /// This class specializes the <see cref="DefaultWriterStringStreamHandlerImpl"/> to provide default implementation for system string (<c>"#Strings"</c>) meta data stream.
   /// </summary>
   public class DefaultWriterSystemStringStreamHandler : DefaultWriterStringStreamHandlerImpl
   {
      /// <summary>
      /// Creates a new instance of <see cref="DefaultWriterSystemStringStreamHandler"/>.
      /// </summary>
      public DefaultWriterSystemStringStreamHandler()
         : base( MetaDataConstants.SYS_STRING_ENCODING )
      {

      }

      /// <summary>
      /// Gets the name of this string stream, which is always <c>"#Strings"</c>.
      /// </summary>
      /// <value>The string value <c>"#Strings"</c>.</value>
      public override String StreamName
      {
         get
         {
            return MetaDataConstants.SYS_STRING_STREAM_NAME;
         }
      }

      /// <summary>
      /// Returns the <see cref="StringStreamKind.SystemStrings"/>.
      /// </summary>
      /// <value>The <see cref="StringStreamKind.SystemStrings"/>.</value>
      public override StringStreamKind StringStreamKind
      {
         get
         {
            return StringStreamKind.SystemStrings;
         }
      }

      /// <summary>
      /// Implements the <see cref="DefaultWriterStringStreamHandlerImpl.GetByteCountForString"/> method.
      /// </summary>
      /// <param name="str">The string.</param>
      /// <returns>The byte count for user string, as specified in ECMA-335 standard.</returns>
      protected override Int32 GetByteCountForString( String str )
      {
         return this.Encoding.GetByteCount( str ) // Byte count for string
            + 1; // Trailing zero
      }

      /// <summary>
      /// Implements the <see cref="DefaultWriterStringStreamHandlerImpl.Serialize"/> method.
      /// </summary>
      /// <param name="str">The string.</param>
      /// <param name="array">The byte array to write <paramref name="str"/> to.</param>
      protected override void Serialize( String str, Byte[] array )
      {
         // Byte array helper has already been set up to hold array size
         var byteCount = this.Encoding.GetBytes( str, 0, str.Length, array, 0 );
         // Remember trailing zero
         array[byteCount] = 0;
      }
   }

   /// <summary>
   /// This class provides default implementation for <see cref="WriterTableStreamHandler"/>.
   /// The serialization functionality is by default delegated further to <see cref="TableSerializationLogicalFunctionality"/> and <see cref="TableSerializationBinaryFunctionality"/> instances.
   /// </summary>
   public class DefaultWriterTableStreamHandler : WriterTableStreamHandler
   {
      // TODO make this class public and store it to DefaultWritingStatus.
      private sealed class WriteDependantInfo
      {
         internal WriteDependantInfo(
            WritingOptions_TableStream writingOptions,
            ArrayQuery<Int32> tableSizes,
            ArrayQuery<TableSerializationLogicalFunctionality> infos,
            WriterMetaDataStreamContainer mdStreams,
            ColumnValueStorage<Int32> heapIndices,
            MetaDataTableStreamHeader header,
            TableSerializationBinaryFunctionalityCreationArgs creationArgs
            )
         {

            var presentTables = header.TableSizes.Count( s => s > 0 );
            var hdrSize = 24 + 4 * presentTables;
            if ( writingOptions.ExtraData.HasValue )
            {
               hdrSize += 4;
            }

            this.HeapIndices = heapIndices;
            this.Serialization = infos.Select( info => info?.CreateBinaryFunctionality( creationArgs ) ).ToArrayProxy().CQ;
            this.HeaderSize = (UInt32) hdrSize;
            this.ContentSize = tableSizes.Select( ( size, idx ) => (UInt32) size * (UInt32) ( this.Serialization[idx]?.ColumnSerializationSupports?.Sum( c => c.ColumnByteCount ) ?? 0 ) ).Sum();
            var totalSize = ( this.HeaderSize + this.ContentSize ).RoundUpU32( 4 );
            this.PaddingSize = totalSize - this.HeaderSize - this.ContentSize;
            this.Header = header;
         }

         public ColumnValueStorage<Int32> HeapIndices { get; }


         public ArrayQuery<TableSerializationBinaryFunctionality> Serialization { get; }

         public UInt32 HeaderSize { get; }

         public UInt32 ContentSize { get; }

         public UInt32 PaddingSize { get; }

         public MetaDataTableStreamHeader Header { get; }

      }

      private readonly CILMetaData _md;
      private readonly WritingOptions_TableStream _options;
      private WriteDependantInfo _writeDependantInfo;

      /// <summary>
      /// Creates a new instance of <see cref="DefaultWriterTableStreamHandler"/> with given parameters.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/> containing tables to serialize.</param>
      /// <param name="options">The <see cref="WritingOptions_TableStream"/> for this <see cref="DefaultWriterTableStreamHandler"/>.</param>
      /// <param name="serializationCreationArgs">The <see cref="TableSerializationLogicalFunctionalityCreationArgs"/> to use when creating <see cref="TableSerializationLogicalFunctionality"/> objects.</param>
      /// <param name="writingStatus">The <see cref="DefaultWritingStatus"/>.</param>
      public DefaultWriterTableStreamHandler(
         CILMetaData md,
         WritingOptions_TableStream options,
         TableSerializationLogicalFunctionalityCreationArgs serializationCreationArgs,
         DefaultWritingStatus writingStatus
         )
      {
         ArgumentValidator.ValidateNotNull( "Meta data", md );

         this._md = md;
         this.TableSerializations = serializationCreationArgs.CreateTableSerializationInfos( md.GetAllTables().Select( t => t.TableInformationNotGeneric ) ).ToArrayProxy().CQ; ;
         this.TableSizes = this.TableSerializations.CreateTableSizeArray( md );
         this.WritingStatus = writingStatus;
         this._options = options ?? new WritingOptions_TableStream();
      }

      /// <summary>
      /// Returns the <c>"#~"</c> string.
      /// </summary>
      /// <value>The <c>"#~"</c> string.</value>
      public String StreamName
      {
         get
         {
            return MetaDataConstants.TABLE_STREAM_NAME;
         }
      }

      /// <summary>
      /// Returns the size of this table stream.
      /// </summary>
      /// <value>The size of this table stream.</value>
      /// <remarks>
      /// This property will return <c>-1</c> when the size has not yet been computed.
      /// The size is computed by <see cref="FillOtherMDStreams"/> method.
      /// </remarks>
      public Int32 StreamSize
      {
         get
         {
            var writeInfo = this._writeDependantInfo;
            return writeInfo == null ? -1 : (Int32) ( writeInfo.HeaderSize + writeInfo.ContentSize + writeInfo.PaddingSize );
         }
      }

      /// <summary>
      /// Returns the <c>true</c> value.
      /// </summary>
      /// <value>The <c>true</c> value.</value>
      public Boolean Accessed
      {
         get
         {
            // Always true, since we need to write table header.
            return true;
         }
      }

      /// <summary>
      /// Implements the <see cref="WriterTableStreamHandler.FillOtherMDStreams"/> method.
      /// </summary>
      /// <param name="publicKey">The public key to use instead of <see cref="AssemblyInformation.PublicKeyOrToken"/> of the <see cref="AssemblyDefinition"/> row.</param>
      /// <param name="mdStreams">The <see cref="WriterMetaDataStreamContainer"/> object containing other <see cref="AbstractWriterStreamHandler"/>s returned by <see cref="WriterFunctionality.CreateMetaDataStreamHandlers"/> method.</param>
      /// <param name="array">The auxiliary byte <see cref="ResizableArray{T}"/>.</param>
      /// <returns>A new instance of <see cref="MetaDataTableStreamHeader"/> describing the header for this meta-data.</returns>
      /// <remarks>
      /// This method will use <see cref="TableSerializationLogicalFunctionality.ExtractMetaDataStreamReferences"/> method to extract and store references to other meta data streams.
      /// Then the <see cref="TableSerializationBinaryFunctionality"/> objects will be built, and table stream size will become queryable via <see cref="StreamSize"/> property.
      /// Finally, the <see cref="DefaultWritingStatus.DataReferencesStorage"/> and <see cref="DefaultWritingStatus.DataReferencesSectionParts"/> properties are set.
      /// </remarks>
      public MetaDataTableStreamHeader FillOtherMDStreams(
         ArrayQuery<Byte> publicKey,
         WriterMetaDataStreamContainer mdStreams,
         ResizableArray<Byte> array
         )
      {
         var mdStreamsRefs = new ColumnValueStorage<Int32>( this.TableSizes, this.TableSerializations.Select( info => info?.MetaDataStreamReferenceColumnCount ?? 0 ) );
         foreach ( var info in this.TableSerializations )
         {
            info?.ExtractMetaDataStreamReferences( this._md, mdStreamsRefs, mdStreams, array, publicKey );
         }

         // Create table stream header
         var options = this._options;
         var header = new MetaDataTableStreamHeader(
            options.Reserved ?? 0,
            options.MajorVersion ?? 2,
            options.MinorVersion ?? 0,
            CreateTableStreamFlags( mdStreams ),
            options.Reserved2 ?? 1,
            (UInt64) ( options.PresentTablesBitVector ?? this.GetPresentTablesBitVector() ),
            (UInt64) ( options.SortedTablesBitVector ?? this.GetSortedTablesBitVector() ),
            this.TableSizes.Select( s => (UInt32) s ).Where( s => s > 0 ).ToArrayProxy().CQ,
            options.ExtraData
            );

         Interlocked.Exchange( ref this._writeDependantInfo, new WriteDependantInfo( this._options, this.TableSizes, this.TableSerializations, mdStreams, mdStreamsRefs, header, this.CreateSerializationCreationArgs( mdStreams ) ) );

         // Set values for writing status
         var status = this.WritingStatus;
         if ( status != null )
         {
            status.DataReferencesStorage = new ColumnValueStorage<Int64>( this.TableSizes, this.TableSerializations.Select( s => s?.DataReferenceColumnCount ?? 0 ) );
            status.DataReferencesSectionParts = this.TableSerializations
               .SelectMany( s => s?.CreateDataReferenceSectionParts( this._md, mdStreams ) ?? Empty<SectionPartFunctionalityWithDataReferenceTargets>.Enumerable )
               .ToArrayProxy().CQ;
         }

         return header;
      }

      /// <summary>
      /// Creates a new <see cref="TableSerializationBinaryFunctionalityCreationArgs"/> to be used to create <see cref="TableSerializationBinaryFunctionality"/> objects in <see cref="FillOtherMDStreams"/> method.
      /// </summary>
      /// <param name="mdStreams">The <see cref="WriterMetaDataStreamContainer"/>.</param>
      /// <returns>A new instance of <see cref="TableSerializationBinaryFunctionalityCreationArgs"/>.</returns>
      protected virtual TableSerializationBinaryFunctionalityCreationArgs CreateSerializationCreationArgs(
         WriterMetaDataStreamContainer mdStreams
         )
      {
         return new TableSerializationBinaryFunctionalityCreationArgs(
            this.TableSizes,
            this.CreateSerializationCreationArgsStreamDictionary( mdStreams ).ToDictionaryProxy().CQ
            );
      }

      /// <summary>
      /// Creates a dictionary to use for <see cref="TableSerializationBinaryFunctionalityCreationArgs"/>.
      /// </summary>
      /// <param name="mdStreams">The <see cref="WriterMetaDataStreamContainer"/>.</param>
      /// <returns>The dictionary created from all streams in given <paramref name="mdStreams"/> with <see cref="E_CommonUtils.ToDictionary_Preserve"/> method.</returns>
      protected virtual IDictionary<String, Int32> CreateSerializationCreationArgsStreamDictionary(
         WriterMetaDataStreamContainer mdStreams
         )
      {
         return mdStreams
            .GetAllStreams()
            .ToDictionary_Preserve( s => s.StreamName, s => s.StreamSize );
      }

      /// <summary>
      /// Implements the <see cref="AbstractWriterStreamHandler.WriteStream"/> method by writing the full table stream.
      /// </summary>
      /// <param name="stream">The <see cref="Stream"/> to write this table stream to.</param>
      /// <param name="array">The auxiliary provider to use.</param>
      /// <param name="dataReferences">The <see cref="DataReferencesInfo"/> containing all the data reference column values.</param>
      public void WriteStream(
         Stream stream,
         ResizableArray<Byte> array,
         DataReferencesInfo dataReferences
         )
      {
         var writeInfo = this._writeDependantInfo;

         // Header
         array.CurrentMaxCapacity = (Int32) writeInfo.HeaderSize;
         var headerSize = writeInfo.Header.WriteTableStreamHeader( array );
         stream.Write( array.Array, headerSize );

         // Rows
         var heapIndices = writeInfo.HeapIndices;
         var tableSizes = this.TableSizes;
         foreach ( var info in this.TableSerializations )
         {
            MetaDataTable table;
            if ( info != null
               && this._md.TryGetByTable( (Int32) info.Table, out table )
               && table.GetRowCount() > 0
               )
            {
               var support = writeInfo.Serialization[(Int32) info.Table];
               var cols = support.ColumnSerializationSupports;
               array.CurrentMaxCapacity = cols.Sum( c => c.ColumnByteCount ) * tableSizes[(Int32) info.Table];
               var byteArray = array.Array;
               var valIdx = 0;
               var arrayIdx = 0;
               ArrayQuery<ArrayQuery<Int64>> dataRefs;
               dataReferences.DataReferences.TryGetValue( info.Table, out dataRefs );

               foreach ( var rawValue in info.GetAllRawValues( table, dataRefs, heapIndices ) )
               {
                  var col = cols[valIdx % cols.Count];
                  col.WriteValue( byteArray, arrayIdx, rawValue );
                  arrayIdx += col.ColumnByteCount;
                  ++valIdx;
               }

               stream.Write( byteArray, arrayIdx );

            }

         }

         // Post-padding
         var postPadding = (Int32) writeInfo.PaddingSize;
         array.CurrentMaxCapacity = postPadding;
         var idx = 0;
         array.Array.ZeroOut( ref idx, postPadding );
         stream.Write( array.Array, postPadding );
      }

      /// <summary>
      /// Gets the array of all <see cref="TableSerializationLogicalFunctionality"/> objects.
      /// </summary>
      /// <value>The array of all <see cref="TableSerializationLogicalFunctionality"/> objects.</value>
      protected ArrayQuery<TableSerializationLogicalFunctionality> TableSerializations { get; }

      /// <summary>
      /// Gets the table size array.
      /// </summary>
      /// <value>The table size array.</value>
      protected ArrayQuery<Int32> TableSizes { get; }

      /// <summary>
      /// Gets the <see cref="DefaultWritingStatus"/> provided to this table stream.
      /// </summary>
      /// <value>The <see cref="DefaultWritingStatus"/> provided to this table stream.</value>
      protected DefaultWritingStatus WritingStatus { get; }

      private TableStreamFlags CreateTableStreamFlags( WriterMetaDataStreamContainer streams )
      {
         var retVal = (TableStreamFlags) 0;
         if ( streams.SystemStrings.IsWide() )
         {
            retVal |= TableStreamFlags.WideStrings;
         }
         if ( streams.GUIDs.IsWide() )
         {
            retVal |= TableStreamFlags.WideGUID;
         }
         if ( streams.BLOBs.IsWide() )
         {
            retVal |= TableStreamFlags.WideBLOB;
         }

         if ( this._options.ExtraData.HasValue )
         {
            retVal |= TableStreamFlags.ExtraData;
         }

         return retVal;
      }

      private Int64 GetPresentTablesBitVector()
      {
         var validBitvector = 0UL;
         var tableSizes = this.TableSizes;
         for ( var i = tableSizes.Count - 1; i >= 0; --i )
         {
            validBitvector = validBitvector << 1;
            if ( tableSizes[i] > 0 )
            {
               validBitvector |= 1;
            }
         }

         return (Int64) validBitvector;
      }

      private Int64 GetSortedTablesBitVector()
      {
         var sortedBitvector = 0UL;
         var tableSerializations = this.TableSerializations;
         for ( var i = tableSerializations.Count - 1; i >= 0; --i )
         {
            sortedBitvector = sortedBitvector << 1;
            if ( tableSerializations[i]?.IsSorted ?? false )
            {
               sortedBitvector |= 1;
            }
         }

         return (Int64) sortedBitvector;
      }
   }
}

public static partial class E_CILPhysical
{
   /// <summary>
   /// Gets the <see cref="DataDirectory"/> object for this <see cref="SectionPart"/>.
   /// </summary>
   /// <param name="part">The <see cref="SectionPart"/>.</param>
   /// <returns>The <see cref="DataDirectory"/> for <see cref="SectionPart"/>, or default value of <see cref="DataDirectory"/> if this <see cref="SectionPart"/> is <c>null</c>.</returns>
   public static DataDirectory GetDataDirectory( this SectionPart part )
   {
      return part == null ? default( DataDirectory ) : new DataDirectory( (UInt32) part.RVA, (UInt32) part.Size );
   }

   /// <summary>
   /// Returns value indicating whether the indices to this <see cref="AbstractWriterStreamHandler"/> are considered to be wide.
   /// </summary>
   /// <param name="stream">The <see cref="AbstractWriterStreamHandler"/>.</param>
   /// <returns><c>true</c> if indices to this <see cref="AbstractWriterStreamHandler"/> are wide (4 byte long).</returns>
   public static Boolean IsWide( this AbstractWriterStreamHandler stream )
   {
      return stream.StreamSize.IsWideMDStreamSize();
   }

   internal static Boolean IsWideMDStreamSize( this Int32 size )
   {
      return ( (UInt32) size ) > UInt16.MaxValue;
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

   internal static ArrayQuery<Int32> CreateTableSizeArray( this IEnumerable<TableSerializationLogicalFunctionality> infos, CILMetaData md )
   {
      return infos.Select( info =>
      {
         MetaDataTable tbl;
         return info != null && md.TryGetByTable( (Int32) info.Table, out tbl ) ?
            tbl.GetRowCount() :
            0;
      } ).ToArrayProxy().CQ;
   }

   /// <summary>
   /// Helper method to get <see cref="DataDirectory"/> for given <see cref="SectionPartFunctionality"/>.
   /// </summary>
   /// <param name="imageSections">This <see cref="ImageSectionsInfo"/>.</param>
   /// <param name="part">The <see cref="SectionPartFunctinality"/>. May be <c>null</c>.</param>
   /// <returns>The <see cref="DataDirectory"/> for given <paramref name="part"/>, or default value of <see cref="DataDirectory"/> if <paramref name="part"/> is <c>null</c> or not found.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="ImageSectionsInfo"/> is <c>null</c>.</exception>
   public static DataDirectory GetDataDirectoryForSectionPart( this ImageSectionsInfo imageSections, SectionPartFunctionality part )
   {
      return part == null ? default( DataDirectory ) : imageSections.GetSectionPartFor( part ).GetDataDirectory();
   }

   /// <summary>
   /// Helper method to return all <see cref="SectionPart"/>s of all <see cref="SectionLayout"/>s in this <see cref="ImageSectionsInfo"/>.
   /// </summary>
   /// <param name="imageSections">The <see cref="ImageSectionsInfo"/>.</param>
   /// <returns>Enumerable of all <see cref="SectionPart"/>s of all <see cref="SectionLayout"/>s in this <see cref="ImageSectionsInfo"/>.</returns>
   /// <exception cref="NullReferenceException">If <see cref="ImageSectionsInfo"/> is <c>null</c>.</exception>
   public static IEnumerable<SectionPart> GetAllSectionParts( this ImageSectionsInfo imageSections )
   {
      return imageSections.Sections.SelectMany( s => s.Parts );
   }

   /// <summary>
   /// Helper method to find the <see cref="SectionPart"/> which would have its <see cref="SectionPart.Functionality"/> object of given type, and pass optional additional checks.
   /// </summary>
   /// <typeparam name="TFunctionality">The type that <see cref="SectionPart.Functionality"/> must be.</typeparam>
   /// <param name="imageSections">The <see cref="ImageSectionsInfo"/>.</param>
   /// <param name="additionalCheck">The optional additional check for <see cref="SectionPartFunctionality"/>.</param>
   /// <returns>The first suitable <see cref="SectionPart"/>, or <c>null</c> if no such parts are found.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="ImageSectionsInfo"/> is <c>null</c>.</exception>
   public static SectionPart GetSectionPartWithFunctionalityOfType<TFunctionality>( this ImageSectionsInfo imageSections, Func<TFunctionality, Boolean> additionalCheck = null )
      where TFunctionality : class, SectionPartFunctionality
   {
      var parts = imageSections.GetAllSectionParts();
      return additionalCheck == null ?
         parts.FirstOrDefault( p => p.Functionality is TFunctionality ) :
         parts.FirstOrDefault( p =>
         {
            var f = p.Functionality as TFunctionality;
            return f != null && additionalCheck( f );
         } );

   }

   /// <summary>
   /// Helper method to find <see cref="SectionPart"/> which <see cref="SectionPart.Functionality"/> would be same reference as given <see cref="SectionPartFunctionality"/>.
   /// </summary>
   /// <param name="imageSections">The <see cref="ImageSectionsInfo"/>.</param>
   /// <param name="functionality">The <see cref="SectionPartFunctionality"/> to match.</param>
   /// <returns>The first suitable <see cref="SectionPart"/>, or <c>null</c> if no such parts are found.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="ImageSectionsInfo"/> is <c>null</c>.</exception>
   public static SectionPart GetSectionPartFor( this ImageSectionsInfo imageSections, SectionPartFunctionality functionality )
   {
      return imageSections.GetAllSectionParts().FirstOrDefault( p => ReferenceEquals( p.Functionality, functionality ) );
   }

}