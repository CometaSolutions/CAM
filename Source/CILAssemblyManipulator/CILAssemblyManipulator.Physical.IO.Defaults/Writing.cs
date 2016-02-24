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
      /// This method implements the <see cref="WriterFunctionality.BeforeMetaData"/> by calling <see cref="WritePart"/> for each section part for each section layout in <see cref="DefaultWritingStatus.SectionLayouts"/> until it encounters <see cref="SectionPart_MetaData"/>.
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
               foreach ( var partLayout in parts.TakeWhile( p => !( p.Functionality is SectionPart_MetaData ) ) )
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
      /// This method implements the <see cref="WriterFunctionality.AfterMetaData"/> by calling <see cref="WritePart"/> for each section part for each section layout in <see cref="DefaultWritingStatus.SectionLayouts"/> after first encounter of <see cref="SectionPart_MetaData"/>.
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
         foreach ( var section in dStatus.SectionLayouts.Sections.SkipWhile( s => !s.Parts.Any( p => p.Functionality is SectionPart_MetaData ) ) )
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
                     if ( partLayout.Functionality is SectionPart_MetaData )
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
            yield return new SectionDescription( new SectionPartFunctionality[] { new SectionPart_RelocDirectory( writingStatus.Machine ) } )
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
      /// <item><description><see cref="SectionPart_ImportAddressTable"/>,</description></item>
      /// <item><description><see cref="SectionPart_CLIHeader"/>,</description></item>
      /// <item><description><see cref="SectionPart_StrongNameSignature"/>,</description></item>
      /// <item><description>all <see cref="SectionPartFunctionalityWithDataReferenceTargets"/> in <see cref="DefaultWritingStatus.DataReferencesSectionParts"/>,</description></item>
      /// <item><description><see cref="SectionPart_MetaData"/>,</description></item>
      /// <item><description><see cref="SectionPart_ImportDirectory"/>,</description></item>
      /// <item><description><see cref="SectionPart_StartupCode"/>,</description></item>
      /// <item><description><see cref="SectionPart_DebugDirectory"/>,</description></item>
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
         yield return new SectionPart_ImportAddressTable( machine );

         // 2. CLI Header
         yield return new SectionPart_CLIHeader();

         // 3. Strong name signature
         yield return new SectionPart_StrongNameSignature( writingStatus.StrongNameInformation, machine );

         // 4. Method IL, Field RVAs, Embedded Manifests
         foreach ( var rawValueSectionPart in writingStatus.DataReferencesSectionParts )
         {
            yield return rawValueSectionPart;
         }

         // 5. Meta data
         yield return new SectionPart_MetaData( mdSize );

         // 6. Import directory
         var peOptions = options.PEOptions;
         yield return new SectionPart_ImportDirectory(
            machine,
            peOptions.ImportHintName,
            peOptions.ImportDirectoryName,
            options.IsExecutable
            );

         // 7. Startup code
         yield return new SectionPart_StartupCode( machine, writingStatus.ImageBase );

         // 8. Debug directory (will get filtered away if no debug data)
         yield return new SectionPart_DebugDirectory( options.DebugOptions );
      }

      /// <summary>
      /// This method is called by <see cref="BeforeMetaData"/> and <see cref="AfterMetaData"/> for each <see cref="SectionPart"/> that should be written to stream.
      /// </summary>
      /// <param name="part">The <see cref="SectionPart"/> to write.</param>
      /// <param name="array">The auxiliary byte array to use.</param>
      /// <param name="stream">The stream to write the <paramref name="part"/> to.</param>
      /// <param name="writingStatus">The <see cref="DefaultWritingStatus"/>.</param>
      protected void WritePart(
         SectionPart part,
         ResizableArray<Byte> array,
         Stream stream,
         DefaultWritingStatus writingStatus
         )
      {
         if ( stream.Position != part.Offset - part.PrePadding )
         {
            // TODO better exception type
            throw new BadImageFormatException( "Internal error: stream position for " + part.Functionality + " was calculated to be " + ( part.Offset - part.PrePadding ) + ", but was " + stream.Position + "." );
         }

         // Write to ResizableArray
         part.Functionality.WriteData( new SectionPartWritingArgs(
            stream,
            array,
            part.PrePadding,
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
         var snData = imageSections.GetSectionPartWithFunctionalityOfType<SectionPart_StrongNameSignature>();
         var md = imageSections.GetSectionPartWithFunctionalityOfType<SectionPart_MetaData>();
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
               SectionPart_CLIHeader.HEADER_SIZE,
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

                  list.Add( new SectionPart( part, prePadding, size, curOffset, curRVA ) );

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
      /// <param name="rawValues">The <see cref="ColumnValueStorage{TValue}"/> for data reference columns.</param>
      /// <returns>The value of <see cref="DoGetDataSize"/> if <paramref name="Write"/> is <c>true</c>; <c>0</c> otherwise.</returns>
      public Int32 GetDataSize( Int64 currentOffset, TRVA currentRVA, ColumnValueStorage<Int64> rawValues )
      {
         return this.Write ?
            this.DoGetDataSize( currentOffset, currentRVA, rawValues ) :
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
      /// <param name="rawValues">The <see cref="ColumnValueStorage{TValue}"/> for data reference columns.</param>
      /// <returns>The size of this <see cref="SectionPartFunctionalityWithFixedAlignment"/>, in bytes.</returns>
      protected abstract Int32 DoGetDataSize( Int64 currentOffset, TRVA currentRVA, ColumnValueStorage<Int64> rawValues );
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
         var prePadding = args.PrePadding;
         var idx = prePadding;
         var capacity = idx + args.DataLength;
         array.CurrentMaxCapacity = capacity;
         var bytez = array.Array;
         var dummyIdx = 0;

         if ( this.DoWriteData( args.WritingStatus, bytez, ref idx ) )
         {
            bytez.ZeroOut( ref dummyIdx, prePadding );
         }
         else
         {
            bytez.ZeroOut( ref dummyIdx, capacity );
         }

         args.Stream.Write( bytez, capacity );
      }

      /// <summary>
      /// This method should write the section contents to given array.
      /// </summary>
      /// <param name="wStatus">The <see cref="DefaultWritingStatus"/>.</param>
      /// <param name="array">The array to write data to.</param>
      /// <param name="idx">The index in <paramref name="array"/> where to start writing.</param>
      /// <returns><c>true</c> if writing was successful; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// If return value is <c>false</c>, then the whole section part data will be zeroed out.
      /// </remarks>
      protected abstract Boolean DoWriteData( DefaultWritingStatus wStatus, Byte[] array, ref Int32 idx );
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
         // Write pre-padding first
         var stream = args.Stream;

         var array = args.ArrayHelper;
         var prePadding = args.PrePadding;
         if ( prePadding > 0 )
         {
            var idx = 0;
            array.ZeroOut( ref idx, prePadding );
            stream.Write( array.Array, prePadding );
         }

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
                  stream.Write( bytez, capacity );
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

   public class SectionPart_MethodIL : SectionPartFunctionalityWithDataReferenceTargetsImpl<MethodDefinition, SectionPart_MethodIL.MethodSizeInfo>
   {
      public struct MethodSizeInfo
      {
         public MethodSizeInfo( Int32 prePadding, Int32 byteSize, Int32 ilCodeByteCount, Boolean isTinyHeader, Boolean exceptionSectionsAreLarge )
         {
            this.PrePadding = prePadding;
            this.ByteSize = byteSize;
            this.ILCodeByteCount = ilCodeByteCount;
            this.IsTinyHeader = isTinyHeader;
            this.ExceptionSectionsAreLarge = exceptionSectionsAreLarge;
         }

         public Int32 PrePadding { get; }

         public Int32 ByteSize { get; }

         public Int32 ILCodeByteCount { get; }

         public Boolean IsTinyHeader { get; }

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
      public SectionPart_MethodIL( CILMetaData md, WriterStringStreamHandler userStrings, Int32 columnIndex = 0, Int32 min = 0, Int32 max = -1 )
         : base( 0x04, md.MethodDefinitions, columnIndex, min, max )
      {
         ArgumentValidator.ValidateNotNull( "Meta data", md );
         ArgumentValidator.ValidateNotNull( "User strings", userStrings );

         this._md = md;
         this._stringTokens = md.MethodDefinitions.TableContents
            .Select( m => m?.IL )
            .Where( il => il != null )
            .SelectMany( il => il.OpCodes.OfType<OpCodeInfoWithString>() )
            .ToDictionary_Overwrite( o => o, o => userStrings.RegisterString( o.Operand ), ReferenceEqualityComparer<OpCodeInfoWithString>.ReferenceBasedComparer );
      }

      protected override Int32 GetSize( MethodSizeInfo sizeInfo )
      {
         return sizeInfo.PrePadding + sizeInfo.ByteSize;
      }

      protected override MethodSizeInfo? GetSizeInfo( Int32 rowIndex, MethodDefinition row, Int64 currentOffset, TRVA currentRVA, Int64 startOffset, TRVA startRVA )
      {
         var il = row?.IL;
         return il == null ?
            (MethodSizeInfo?) null :
            this.CalculateByteSizeForMethod( rowIndex, il, currentRVA );
      }

      protected override TRVA GetValueForTableStreamFromSize( TRVA currentRVA, MethodSizeInfo sizeInfo )
      {
         return currentRVA + sizeInfo.PrePadding;
      }

      protected override TRVA GetValueForTableStreamFromRow( Int32 rowIndex, MethodDefinition row )
      {
         return 0;
      }

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

      protected void EmitOpCodeInfo(
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

      protected MethodSizeInfo CalculateByteSizeForMethod(
         Int32 rowIndex,
         MethodILDefinition il,
         TRVA currentRVA
         )
      {
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

   public class SectionPart_FieldRVA : SectionPartFunctionalityWithDataReferenceTargetsImpl<FieldRVA, Int32>
   {
      public SectionPart_FieldRVA( CILMetaData md, Int32 columnIndex = 0, Int32 min = 0, Int32 max = -1 )
         : base( 0x08, md.FieldRVAs, columnIndex, min, max )
      {
      }

      protected override Int32? GetSizeInfo( Int32 rowIndex, FieldRVA row, Int64 currentOffset, TRVA currentRVA, Int64 startOffset, TRVA startRVA )
      {
         return row?.Data?.Length;
      }

      protected override Int32 GetSize( Int32 sizeInfo )
      {
         return sizeInfo;
      }

      protected override TRVA GetValueForTableStreamFromSize( TRVA currentRVA, Int32 sizeInfo )
      {
         return currentRVA;
      }

      protected override TRVA GetValueForTableStreamFromRow( Int32 rowIndex, FieldRVA row )
      {
         return 0;
      }

      protected override void WriteData( FieldRVA row, Int32 sizeInfo, Byte[] array )
      {
         var idx = 0;
         var data = row.Data;
         if ( !data.IsNullOrEmpty() )
         {
            array.BlockCopyFrom( ref idx, row.Data );
         }
      }
   }

   public class SectionPart_EmbeddedManifests : SectionPartFunctionalityWithDataReferenceTargetsImpl<ManifestResource, SectionPart_EmbeddedManifests.ManifestSizeInfo>
   {
      public struct ManifestSizeInfo
      {
         public ManifestSizeInfo( Int32 byteCount, TRVA startRVA, TRVA currentRVA )
         {
            this.ByteCount = byteCount;
            this.PrePadding = (Int32) ( currentRVA.RoundUpI64( ALIGNMENT ) - currentRVA );
            this.Offset = (Int32) ( currentRVA - startRVA + this.PrePadding );
         }

         public Int32 ByteCount { get; }

         public Int32 PrePadding { get; }

         public Int32 Offset { get; }
      }

      private const Int32 ALIGNMENT = 0x08;

      public SectionPart_EmbeddedManifests( CILMetaData md, Int32 columnIndex = 0, Int32 min = 0, Int32 max = -1 )
         : base( ALIGNMENT, md.ManifestResources, columnIndex, min, max )
      {
      }

      protected override ManifestSizeInfo? GetSizeInfo( Int32 rowIndex, ManifestResource row, Int64 currentOffset, TRVA currentRVA, Int64 startOffset, TRVA startRVA )
      {
         ManifestSizeInfo? retVal;
         if ( row.IsEmbeddedResource() )
         {
            retVal = new ManifestSizeInfo(
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

      protected override Int32 GetSize( ManifestSizeInfo sizeInfo )
      {
         return sizeInfo.PrePadding + sizeInfo.ByteCount;
      }

      protected override TRVA GetValueForTableStreamFromSize( TRVA currentRVA, ManifestSizeInfo sizeInfo )
      {
         return sizeInfo.Offset;
      }

      protected override TRVA GetValueForTableStreamFromRow( Int32 rowIndex, ManifestResource row )
      {
         return row.Offset;
      }

      protected override void WriteData( ManifestResource row, ManifestSizeInfo sizeInfo, Byte[] array )
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

   public class SectionPartWritingArgs
   {
      public SectionPartWritingArgs(
         Stream stream,
         ResizableArray<Byte> array,
         Int32 prePadding,
         Int32 dataLength,
         DefaultWritingStatus writingStatus
         )
      {
         ArgumentValidator.ValidateNotNull( "Stream", stream );
         ArgumentValidator.ValidateNotNull( "Array", array );
         ArgumentValidator.ValidateNotNull( "Writing status", writingStatus );

         this.Stream = stream;
         this.ArrayHelper = array;
         this.PrePadding = prePadding;
         this.DataLength = dataLength;
         this.WritingStatus = writingStatus;
      }

      public Stream Stream { get; }

      public ResizableArray<Byte> ArrayHelper { get; }

      public Int32 PrePadding { get; }

      public Int32 DataLength { get; }

      public DefaultWritingStatus WritingStatus { get; }
   }

   public class SectionPart
   {
      public SectionPart(
         SectionPartFunctionality functionality,
         Int32 prePadding,
         Int32 size,
         Int64 offset,
         TRVA rva
         )
      {
         ArgumentValidator.ValidateNotNull( "Functionality", functionality );

         this.Functionality = functionality;
         this.PrePadding = prePadding;
         this.Size = size;
         this.Offset = offset;
         this.RVA = rva;
      }

      public SectionPartFunctionality Functionality { get; }

      public Int32 PrePadding { get; }

      public Int32 Size { get; }

      public Int64 Offset { get; }

      public TRVA RVA { get; }
   }

   public class SectionPart_CLIHeader : SectionPartFunctionalityWithFixedLength
   {
      internal const Int32 HEADER_SIZE = 0x48;


      public SectionPart_CLIHeader()
         : base( 4, true, HEADER_SIZE )
      {
      }

      protected override Boolean DoWriteData( DefaultWritingStatus wStatus, Byte[] array, ref Int32 idx )
      {
         var imageSections = wStatus.SectionLayouts;
         wStatus.CLIHeader.WriteCLIHeader( array, ref idx );
         wStatus.PEDataDirectories[(Int32) DataDirectories.CLIHeader] = imageSections.GetDataDirectoryForSectionPart( this );

         return true;
      }

   }

   public class SectionPart_StrongNameSignature : SectionPartFunctionalityWithFixedLength
   {
      public SectionPart_StrongNameSignature( StrongNameInformation snVars, ImageFileMachine machine )
         : base( machine.RequiresPE64() ? 0x10 : 0x04, true, snVars?.SignatureSize ?? 0 )
      {

      }

      protected override Boolean DoWriteData( DefaultWritingStatus wStatus, Byte[] array, ref Int32 idx )
      {
         // Don't write actual signature, since we don't have required information. The strong name signature will be written by WriteMetaData implementation.
         return false;
         //array.ZeroOut( ref idx, args.PrePadding + args.DataLength );
         //return true;
      }
   }

   public class SectionPart_MetaData : SectionPartFunctionalityWithFixedLength
   {
      public SectionPart_MetaData( Int32 size )
         : base( 0x04, true, size )
      {

      }

      protected override Boolean DoWriteData( DefaultWritingStatus wStatus, Byte[] array, ref Int32 idx )
      {
         // This method will never get really called
         throw new NotSupportedException( "This method should not be called." );
      }
   }

   public class SectionPart_ImportAddressTable : SectionPartFunctionalityWithFixedLength
   {
      public SectionPart_ImportAddressTable( ImageFileMachine machine )
         : base( 0x04, !machine.RequiresPE64(), 0x08 )
      {
      }


      protected override Boolean DoWriteData( DefaultWritingStatus wStatus, Byte[] array, ref Int32 idx )
      {
         var imageSections = wStatus.SectionLayouts;
         var importDir = imageSections.GetSectionPartWithFunctionalityOfType<SectionPart_ImportDirectory>();
         var retVal = importDir != null;
         if ( retVal )
         {
            array
               .WriteInt32LEToBytes( ref idx, ( (SectionPart_ImportDirectory) importDir.Functionality ).CorMainRVA ) // RVA of _CorDll/ExeMain
               .WriteInt32LEToBytes( ref idx, 0 ); // Terminating entry

            wStatus.PEDataDirectories[(Int32) DataDirectories.ImportAddressTable] = imageSections.GetDataDirectoryForSectionPart( this );
         }
         return retVal;
      }
   }

   public class SectionPart_ImportDirectory : SectionPartFunctionalityWriteableToArray
   {

      internal const String HINTNAME_FOR_DLL = "_CorDllMain";
      internal const String HINTNAME_FOR_EXE = "_CorExeMain";

      private readonly String _functionName;
      private readonly String _moduleName;

      private UInt32 _lookupTableRVA;
      private UInt32 _paddingBeforeString;
      private UInt32 _corMainRVA;
      private UInt32 _mscoreeRVA;

      public SectionPart_ImportDirectory( ImageFileMachine machine, String functionName, String moduleName, Boolean isExecutable )
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

      protected override Int32 DoGetDataSize( Int64 currentOffset, TRVA currentRVA, ColumnValueStorage<Int64> rawValues )
      {
         var startRVA = (UInt32) currentRVA.RoundUpI64( this.DataAlignment );
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

      public Int32 CorMainRVA
      {
         get
         {
            return (Int32) this._corMainRVA;
         }
      }

      protected override Boolean DoWriteData( DefaultWritingStatus wStatus, Byte[] array, ref Int32 idx )
      {
         var imageSections = wStatus.SectionLayouts;
         var addressTable = imageSections.GetSectionPartWithFunctionalityOfType<SectionPart_ImportAddressTable>();

         var retVal = addressTable != null;
         if ( retVal )
         {
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

   public class SectionPart_StartupCode : SectionPartFunctionalityWithFixedLength
   {
      private readonly UInt32 _imageBase;

      private const Int32 ALIGNMENT = 0x04;
      private const Int32 PADDING = 2;
      public SectionPart_StartupCode( ImageFileMachine machine, Int64 imageBase )
         : base( ALIGNMENT, !machine.RequiresPE64(), 0x08 )
      {
         this._imageBase = (UInt32) imageBase;
      }

      public Int32 EntryPointOffset
      {
         get
         {
            return PADDING;
         }
      }

      public Int32 EntryPointInstructionAddressOffset
      {
         get
         {
            return this.EntryPointOffset + 2;
         }
      }

      protected override Boolean DoWriteData( DefaultWritingStatus wStatus, Byte[] array, ref Int32 idx )
      {
         var sectionLayouts = wStatus.SectionLayouts;
         var addressTable = sectionLayouts.GetSectionPartWithFunctionalityOfType<SectionPart_ImportAddressTable>();
         var retVal = addressTable != null;
         if ( retVal )
         {
            array
               .ZeroOut( ref idx, PADDING ) // Padding - 2 zero bytes
               .WriteUInt16LEToBytes( ref idx, 0x25FF ) // JMP
               .WriteUInt32LEToBytes( ref idx, this._imageBase + (UInt32) addressTable.RVA ); // First entry of address table = RVA of _CorDll/ExeMain

            wStatus.EntryPointRVA = (Int32) ( sectionLayouts.GetSectionPartFor( this ).RVA + this.EntryPointOffset );
         }
         return retVal;
      }
   }

   public class SectionPart_RelocDirectory : SectionPartFunctionalityWithFixedLength
   {
      private const Int32 SIZE = 0x0C;
      private const UInt32 RELOCATION_PAGE_MASK = 0x0FFF; // ECMA-335, p. 282
      private const UInt16 RELOCATION_FIXUP_TYPE = 0x3; // ECMA-335, p. 282

      public SectionPart_RelocDirectory( ImageFileMachine machine )
         : base( 0x04, !machine.RequiresPE64(), SIZE )
      {

      }

      protected override Boolean DoWriteData( DefaultWritingStatus wStatus, Byte[] array, ref Int32 idx )
      {
         var imageSections = wStatus.SectionLayouts;
         var startupCode = imageSections.GetSectionPartWithFunctionalityOfType<SectionPart_StartupCode>();
         var retVal = startupCode != null;
         if ( retVal )
         {
            var rva = (UInt32) ( startupCode.RVA + ( (SectionPart_StartupCode) startupCode.Functionality ).EntryPointInstructionAddressOffset );
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

   public class SectionPart_DebugDirectory : SectionPartFunctionalityWithFixedLength
   {
      private const Int32 ALIGNMENT = 0x04;
      private const Int32 HEADER_SIZE = 0x1C;

      private readonly WritingOptions_Debug _options;

      public SectionPart_DebugDirectory( WritingOptions_Debug options )
         : base( ALIGNMENT, !( options?.DebugData ).IsNullOrEmpty(), ( HEADER_SIZE + ( options?.DebugData?.Length ?? 0 ) ) )
      {
         this._options = options;
      }

      protected override Boolean DoWriteData( DefaultWritingStatus wStatus, Byte[] array, ref Int32 idx )
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

            debugInfo.WriteDebugInformation( array, ref idx );
            wStatus.PEDataDirectories[(Int32) DataDirectories.Debug] = imageSections.GetDataDirectoryForSectionPart( this );
            wStatus.DebugInformation = debugInfo;
         }
         return retVal;
      }
   }


   public abstract class AbstractWriterStreamHandlerImpl : AbstractWriterStreamHandler
   {
      private readonly UInt32 _startingIndex;
      [CLSCompliant( false )]
      protected UInt32 curIndex;

      internal AbstractWriterStreamHandlerImpl( UInt32 startingIndex )
      {
         this._startingIndex = startingIndex;
         this.curIndex = startingIndex;
      }

      public abstract String StreamName { get; }

      public virtual void WriteStream(
         Stream sink,
         ResizableArray<Byte> array,
         DataReferencesInfo rawValueProvder
         )
      {
         if ( this.Accessed )
         {
            this.DoWriteStream( sink, array );
            var size = this.curIndex;
            var padding = (Int32) ( size.RoundUpU32( 4 ) - size );
            if ( padding > 0 )
            {
               array.CurrentMaxCapacity = padding;
               var idx = 0;
               array.Array.ZeroOut( ref idx, padding );
               sink.Write( array.Array, padding );
            }
         }
      }

      public Int32 StreamSize
      {
         get
         {
            return (Int32) this.curIndex.RoundUpU32( 4 );
         }
      }

      public Boolean Accessed
      {
         get
         {
            return this.curIndex > this._startingIndex;
         }
      }

      protected abstract void DoWriteStream( Stream sink, ResizableArray<Byte> array );
   }

   internal class DefaultWriterBLOBStreamHandler : AbstractWriterStreamHandlerImpl, WriterBLOBStreamHandler
   {
      private readonly IDictionary<Byte[], UInt32> _blobIndices;
      private readonly IList<Byte[]> _blobs;

      internal DefaultWriterBLOBStreamHandler()
         : base( 1 )
      {
         this._blobIndices = new Dictionary<Byte[], UInt32>( ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer );
         this._blobs = new List<Byte[]>();
      }

      public override String StreamName
      {
         get
         {
            return MetaDataConstants.BLOB_STREAM_NAME;
         }
      }

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
               result = this.curIndex;
               this._blobIndices.Add( blob, result );
               this._blobs.Add( blob );
               this.curIndex += (UInt32) blob.Length + (UInt32) BitUtils.GetEncodedUIntSize( blob.Length );
            }
         }

         return (Int32) result;
      }

      protected override void DoWriteStream(
         Stream sink,
         ResizableArray<Byte> array
         )
      {
         sink.WriteByte( 0 );
         var idx = 0;
         if ( this._blobs.Count > 0 )
         {
            foreach ( var blob in this._blobs )
            {
               idx = 0;
               array.AddCompressedUInt32( ref idx, blob.Length );
               sink.Write( array.Array, idx );
               sink.Write( blob );
            }
         }
      }

   }

   public class DefaultWriterGuidStreamHandler : AbstractWriterStreamHandlerImpl, WriterGUIDStreamHandler
   {
      private readonly IDictionary<Guid, UInt32> _guids;

      internal DefaultWriterGuidStreamHandler()
         : base( 0 )
      {
         this._guids = new Dictionary<Guid, UInt32>();
      }

      public override String StreamName
      {
         get
         {
            return MetaDataConstants.GUID_STREAM_NAME;
         }
      }

      public Int32 RegisterGUID( Guid? guid )
      {
         UInt32 result;
         if ( guid.HasValue )
         {
            result = this._guids.GetOrAdd_NotThreadSafe( guid.Value, g =>
            {
               var retVal = (UInt32) this._guids.Count + 1;
               this.curIndex += MetaDataConstants.GUID_SIZE;
               return retVal;
            } );
         }
         else
         {
            result = 0;
         }

         return (Int32) result;
      }

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

   public abstract class AbstractWriterStringStreamHandlerImpl : AbstractWriterStreamHandlerImpl, WriterStringStreamHandler
   {
      private readonly IDictionary<String, KeyValuePair<UInt32, Int32>> _strings;
      private readonly Encoding _encoding;

      internal AbstractWriterStringStreamHandlerImpl( Encoding encoding )
         : base( 1 )
      {
         this._encoding = encoding;
         this._strings = new Dictionary<String, KeyValuePair<UInt32, Int32>>();
      }

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
               result = this.curIndex;
               var byteCount = this.GetByteCountForString( str );
               this._strings.Add( str, new KeyValuePair<UInt32, Int32>( this.curIndex, byteCount ) );
               this.curIndex += (UInt32) byteCount;
            }
         }
         return (Int32) result;
      }

      public abstract StringStreamKind StringStreamKind { get; }

      internal Int32 StringCount
      {
         get
         {
            return this._strings.Count;
         }
      }

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
               this.Serialize( kvp.Key, array );
               sink.Write( array.Array, arrayLen );
            }
         }
      }

      protected Encoding Encoding
      {
         get
         {
            return this._encoding;
         }
      }

      protected abstract Int32 GetByteCountForString( String str );

      protected abstract void Serialize( String str, ResizableArray<Byte> byteArrayHelper );
   }

   public class DefaultWriterUserStringStreamHandler : AbstractWriterStringStreamHandlerImpl
   {
      internal DefaultWriterUserStringStreamHandler()
         : base( MetaDataConstants.USER_STRING_ENCODING )
      {

      }

      public override String StreamName
      {
         get
         {
            return MetaDataConstants.USER_STRING_STREAM_NAME;
         }
      }

      public override StringStreamKind StringStreamKind
      {
         get
         {
            return StringStreamKind.UserStrings;
         }
      }

      protected override Int32 GetByteCountForString( String str )
      {
         var retVal = str.Length * 2 // Each character is 2 bytes
            + 1; // Trailing byte (zero or 1)
         retVal += BitUtils.GetEncodedUIntSize( retVal ); // How many bytes it will take to compress the byte count
         return retVal;
      }

      protected override void Serialize( String str, ResizableArray<Byte> byteArrayHelper )
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

   public class DefaultWriterSystemStringStreamHandler : AbstractWriterStringStreamHandlerImpl
   {
      public DefaultWriterSystemStringStreamHandler()
         : base( MetaDataConstants.SYS_STRING_ENCODING )
      {

      }

      public override String StreamName
      {
         get
         {
            return MetaDataConstants.SYS_STRING_STREAM_NAME;
         }
      }

      public override StringStreamKind StringStreamKind
      {
         get
         {
            return StringStreamKind.SystemStrings;
         }
      }

      protected override Int32 GetByteCountForString( String str )
      {
         return this.Encoding.GetByteCount( str ) // Byte count for string
            + 1; // Trailing zero
      }

      protected override void Serialize( String str, ResizableArray<Byte> byteArrayHelper )
      {
         // Byte array helper has already been set up to hold array size
         var array = byteArrayHelper.Array;
         var byteCount = this.Encoding.GetBytes( str, 0, str.Length, array, 0 );
         // Remember trailing zero
         array[byteCount] = 0;
      }
   }

   public class DefaultWriterTableStreamHandler : WriterTableStreamHandler
   {

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

      public String StreamName
      {
         get
         {
            return MetaDataConstants.TABLE_STREAM_NAME;
         }
      }

      public Int32 StreamSize
      {
         get
         {
            var writeInfo = this._writeDependantInfo;
            return (Int32) ( writeInfo.HeaderSize + writeInfo.ContentSize + writeInfo.PaddingSize );
         }
      }

      public Boolean Accessed
      {
         get
         {
            // Always true, since we need to write table header.
            return true;
         }
      }


      public MetaDataTableStreamHeader FillOtherMDStreams(
         ArrayQuery<Byte> publicKey,
         WriterMetaDataStreamContainer mdStreams,
         ResizableArray<Byte> array
         )
      {
         var retVal = new ColumnValueStorage<Int32>( this.TableSizes, this.TableSerializations.Select( info => info?.MetaDataStreamReferenceColumnCount ?? 0 ) );
         foreach ( var info in this.TableSerializations )
         {
            info?.ExtractMetaDataStreamReferences( this._md, retVal, mdStreams, array, publicKey );
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

         Interlocked.Exchange( ref this._writeDependantInfo, new WriteDependantInfo( this._options, this.TableSizes, this.TableSerializations, mdStreams, retVal, header, this.CreateSerializationCreationArgs( mdStreams ) ) );

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

      protected virtual TableSerializationBinaryFunctionalityCreationArgs CreateSerializationCreationArgs(
         WriterMetaDataStreamContainer mdStreams
         )
      {
         return new TableSerializationBinaryFunctionalityCreationArgs(
            this.TableSizes,
            this.CreateSerializationCreationArgsStreamDictionary( mdStreams ).ToDictionaryProxy().CQ
            );
      }

      protected virtual IDictionary<String, Int32> CreateSerializationCreationArgsStreamDictionary(
         WriterMetaDataStreamContainer mdStreams
         )
      {
         return mdStreams
            .GetAllStreams()
            .ToDictionary_Preserve( s => s.StreamName, s => s.StreamSize );
      }

      public void WriteStream(
         Stream sink,
         ResizableArray<Byte> array,
         DataReferencesInfo rawValueProvder
         )
      {
         var writeInfo = this._writeDependantInfo;

         // Header
         array.CurrentMaxCapacity = (Int32) writeInfo.HeaderSize;
         var headerSize = writeInfo.Header.WriteTableStreamHeader( array );
         sink.Write( array.Array, headerSize );

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
               rawValueProvder.DataReferences.TryGetValue( info.Table, out dataRefs );

               foreach ( var rawValue in info.GetAllRawValues( table, dataRefs, heapIndices ) )
               {
                  var col = cols[valIdx % cols.Count];
                  col.WriteValue( byteArray, arrayIdx, rawValue );
                  arrayIdx += col.ColumnByteCount;
                  ++valIdx;
               }

               sink.Write( byteArray, arrayIdx );

            }

         }

         // Post-padding
         var postPadding = (Int32) writeInfo.PaddingSize;
         array.CurrentMaxCapacity = postPadding;
         var idx = 0;
         array.Array.ZeroOut( ref idx, postPadding );
         sink.Write( array.Array, postPadding );
      }

      protected ArrayQuery<TableSerializationLogicalFunctionality> TableSerializations { get; }

      protected ArrayQuery<Int32> TableSizes { get; }

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

   public static DataDirectory GetDataDirectory( this SectionPart info )
   {
      return info == null ? default( DataDirectory ) : new DataDirectory( (UInt32) info.RVA, (UInt32) info.Size );
   }

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

   public static ArrayQuery<Int32> CreateTableSizeArray( this IEnumerable<TableSerializationLogicalFunctionality> infos, CILMetaData md )
   {
      return infos.Select( info =>
      {
         MetaDataTable tbl;
         return info != null && md.TryGetByTable( (Int32) info.Table, out tbl ) ?
            tbl.GetRowCount() :
            0;
      } ).ToArrayProxy().CQ;
   }

   public static DataDirectory GetDataDirectoryForSectionPart( this ImageSectionsInfo imageSections, SectionPartFunctionality part )
   {
      return part == null ? default( DataDirectory ) : imageSections.GetSectionPartFor( part ).GetDataDirectory();
   }

   public static IEnumerable<SectionPart> GetAllSectionParts( this ImageSectionsInfo imageSections )
   {
      return imageSections.Sections.SelectMany( s => s.Parts );
   }

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

   public static SectionPart GetSectionPartFor( this ImageSectionsInfo imageSections, SectionPartFunctionality functionality )
   {
      return imageSections.GetAllSectionParts().FirstOrDefault( p => ReferenceEquals( p.Functionality, functionality ) );
   }

}