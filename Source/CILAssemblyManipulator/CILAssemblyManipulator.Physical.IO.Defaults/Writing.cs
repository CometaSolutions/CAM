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

   public class DefaultWriterFunctionality : WriterFunctionality
   {

      public DefaultWriterFunctionality(
         CILMetaData md,
         WritingOptions options,
         TableSerializationLogicalFunctionalityCreationArgs serializationCreationArgs
         )
      {
         this.MetaData = md;
         this.WritingOptions = options ?? new WritingOptions();
         this.TableSerializationLogicalFunctionalityCreationArgs = serializationCreationArgs;
      }



      public virtual WritingStatus CreateWritingStatus(
         StrongNameInformation snVars
         )
      {
         var options = this.WritingOptions;
         var peOptions = options.PEOptions;
         var machine = peOptions.Machine ?? ImageFileMachine.I386;
         var sectionsCount = this.GetSectionCount( machine );

         var peDataDirCount = peOptions.NumberOfDataDirectories ?? (Int32) DataDirectories.MaxValue;
         var optionalHeaderKind = machine.GetOptionalHeaderKind();
         var optionalHeaderSize = optionalHeaderKind.GetOptionalHeaderSize( peDataDirCount );
         return this.DoCreateWritingStatus(
            0x80 // DOS header size
            + CAMIOInternals.PE_SIG_AND_FILE_HEADER_SIZE // PE Signature + File header size
            + optionalHeaderSize // Optional header size
            + sectionsCount * 0x28 // Sections
            ,
            machine,
            peOptions.FileAlignment,
            peOptions.SectionAlignment,
            peOptions.ImageBase,
            snVars,
            peDataDirCount,
            sectionsCount
            );
      }

      public virtual IEnumerable<AbstractWriterStreamHandler> CreateMetaDataStreamHandlers(
         WritingStatus status
         )
      {
         yield return new DefaultWriterTableStreamHandler( this.MetaData, this.WritingOptions.CLIOptions.TablesStreamOptions, this.TableSerializationLogicalFunctionalityCreationArgs, (DefaultWritingStatus) status );
         yield return new DefaultWriterSystemStringStreamHandler();
         yield return new DefaultWriterBLOBStreamHandler();
         yield return new DefaultWriterGuidStreamHandler();
         yield return new DefaultWriterUserStringStreamHandler();
      }

      public virtual DataReferencesInfo CalculateImageLayout(
         WritingStatus writingStatus,
         WriterMetaDataStreamContainer mdStreamContainer,
         IEnumerable<AbstractWriterStreamHandler> allStreams,
         out RVAConverter rvaConverter,
         out Int32 mdRootSize
         )
      {
         var dStatus = (DefaultWritingStatus) writingStatus;

         // Get raw value sections (IL, Field RVA, Embedded manifest resources)
         // Enumerate right here, to force e.g. user-string heap initialization for method IL section part
         var rawSections = dStatus.DataReferencesSectionParts.ToArray();

         // MetaData
         Int32 mdSize;
         var mdRoot = this.CreateMDRoot( allStreams.Where( s => s.Accessed ), out mdRootSize, out mdSize );
         writingStatus.MDRoot = mdRoot;

         // Sections
         var sectionLayoutInfos = new SectionLayoutAggregator( this.PopulateSections( dStatus, rawSections, mdRoot, mdSize ) );

         dStatus.SectionLayouts = sectionLayoutInfos;
         var allLayouts = sectionLayoutInfos.LayoutInfos;
         var sections = writingStatus.SectionHeaders;
         for ( var i = 0; i < allLayouts.Count; ++i )
         {
            sections[i] = allLayouts[i].SectionHeader;
         }

         // RVA converter
         rvaConverter = this.CreateRVAConverter( sections );

         // CLI Header
         writingStatus.CLIHeader = this.CreateCLIHeader( dStatus );

         return dStatus.DataReferencesStorage.CreateDataReferencesInfo( i => i );
      }

      public virtual void BeforeMetaData(
         WritingStatus writingStatus,
         Stream stream,
         ResizableArray<Byte> array
         )
      {
         foreach ( var section in ( (DefaultWritingStatus) writingStatus ).SectionLayouts.LayoutInfos )
         {
            var parts = section.PartInfos;
            if ( parts.Count > 0 )
            {
               // Write either whole section, or all parts up until metadata
               var idx = 0;
               foreach ( var partLayout in parts.TakeWhile( p => !( p.Part is SectionPart_MetaData ) ) )
               {
                  // Write to ResizableArray
                  this.WritePart( partLayout, array, stream, writingStatus );
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
                  var pad = (Int32) ( stream.Position.RoundUpI64( writingStatus.FileAlignment ) - stream.Position );
                  array.CurrentMaxCapacity = pad;
                  idx = 0;
                  array.ZeroOut( ref idx, pad );
                  stream.Write( array.Array, pad );
               }
            }
         }
      }

      public virtual void WriteMDRoot(
         WritingStatus writingStatus,
         ResizableArray<Byte> array
         )
      {
         // Array capacity set by writing process
         writingStatus.MDRoot.WriteMetaDataRoot( array );
      }

      public virtual void AfterMetaData(
         WritingStatus writingStatus,
         Stream stream,
         ResizableArray<Byte> array
         )
      {

         var mdEncountered = false;
         foreach ( var section in ( (DefaultWritingStatus) writingStatus ).SectionLayouts.LayoutInfos.SkipWhile( s => !s.PartInfos.Any( p => p.Part is SectionPart_MetaData ) ) )
         {
            var parts = section.PartInfos;
            if ( parts.Count > 0 )
            {

               // Write either whole section, or all parts up until metadata
               foreach ( var partLayout in parts )
               {
                  if ( mdEncountered )
                  {
                     this.WritePart( partLayout, array, stream, writingStatus );
                  }
                  else
                  {
                     if ( partLayout.Part is SectionPart_MetaData )
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

      protected CILMetaData MetaData { get; }

      protected TableSerializationLogicalFunctionalityCreationArgs TableSerializationLogicalFunctionalityCreationArgs { get; }

      protected WritingOptions WritingOptions { get; }

      protected virtual Int32 GetSectionCount( ImageFileMachine machine )
      {
         var retVal = this.GetMinimumSectionCount();
         if ( !machine.RequiresPE64() )
         {
            ++retVal; // Relocs
         }
         return retVal;
      }

      protected virtual Int32 GetMinimumSectionCount()
      {
         return 1;
      }

      protected virtual RVAConverter CreateRVAConverter( IEnumerable<SectionHeader> headers )
      {
         return new DefaultRVAConverter( headers );
      }

      protected virtual IEnumerable<SectionLayoutInfo> PopulateSections(
         DefaultWritingStatus writingStatus,
         IEnumerable<SectionPartWithDataReferenceTargets> rawValueSectionParts,
         MetaDataRoot mdRoot,
         Int32 mdSize
         )
      {
         var encoding = Encoding.UTF8;
         var fAlign = writingStatus.FileAlignment;
         var sAlign = (UInt32) writingStatus.SectionAlignment;
         var curPointer = (UInt32) writingStatus.GetAlignedHeadersSize();
         var curRVA = sAlign;

         foreach ( var layout in this.GetSectionLayouts( writingStatus, mdSize, rawValueSectionParts ) )
         {
            var layoutInfo = new SectionLayoutInfo(
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

      protected virtual MetaDataRoot CreateMDRoot(
         IEnumerable<AbstractWriterStreamHandler> presentStreams,
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
            mdOptions.StorageFlags ?? (StorageFlags) 0,
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


      protected virtual IEnumerable<SectionLayout> GetSectionLayouts(
         WritingStatus writingStatus,
         Int32 mdSize,
         IEnumerable<SectionPartWithDataReferenceTargets> rawValueSectionParts
         )
      {
         // 1. Text section
         yield return new SectionLayout( this.GetTextSectionParts( writingStatus, mdSize, rawValueSectionParts ) )
         {
            Name = ".text",
            Characteristics = SectionHeaderCharacteristics.Memory_Execute | SectionHeaderCharacteristics.Memory_Read | SectionHeaderCharacteristics.Contains_Code
         };

         // 2. Resource section (TODO)

         // 3. Relocation section
         if ( !writingStatus.Machine.RequiresPE64() )
         {
            yield return new SectionLayout( new SectionPart[] { new SectionPart_RelocDirectory( writingStatus.Machine ) } )
            {
               Name = ".reloc",
               Characteristics = SectionHeaderCharacteristics.Memory_Read | SectionHeaderCharacteristics.Memory_Discardable | SectionHeaderCharacteristics.Contains_InitializedData
            };
         }
      }

      protected virtual DefaultWritingStatus DoCreateWritingStatus(
         Int32 headersSize,
         ImageFileMachine machine,
         Int32? fileAlignment,
         Int32? sectionAlignment,
         Int64? imageBase,
         StrongNameInformation strongNameVariables,
         Int32 dataDirCount,
         Int32 sectionsCount
         )
      {
         return new DefaultWritingStatus(
            headersSize,
            machine,
            fileAlignment,
            sectionAlignment,
            imageBase,
            strongNameVariables,
            dataDirCount,
            sectionsCount
            );
      }

      protected virtual IEnumerable<SectionPart> GetTextSectionParts(
         WritingStatus writingStatus,
         Int32 mdSize,
         IEnumerable<SectionPartWithDataReferenceTargets> rawValueSectionParts
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
         foreach ( var rawValueSectionPart in rawValueSectionParts )
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

      protected void WritePart(
         SectionPartInfo partLayout,
         ResizableArray<Byte> array,
         Stream stream,
         WritingStatus writingStatus
         )
      {
         if ( stream.Position != partLayout.Offset - partLayout.PrePadding )
         {
            // TODO better exception type
            throw new BadImageFormatException( "Internal error: stream position for " + partLayout.Part + " was calculated to be " + ( partLayout.Offset - partLayout.PrePadding ) + ", but was " + stream.Position + "." );
         }

         // Write to ResizableArray
         partLayout.Part.WriteData( new SectionPartWritingArgs(
            stream,
            array,
            partLayout.PrePadding,
            partLayout.Size,
            (DefaultWritingStatus) writingStatus
            ) );
      }

      protected virtual CLIHeader CreateCLIHeader(
         DefaultWritingStatus writingStatus
         )
      {
         var partInfos = writingStatus.SectionLayouts.SectionPartInfos;
         var parts = partInfos.Keys;
         var embeddedResources = parts
            .OfType<SectionPartWithDataReferenceTargets>()
            .FirstOrDefault( p => p.RelatedTable == Tables.ManifestResource );
         var snData = parts
            .OfType<SectionPart_StrongNameSignature>()
            .FirstOrDefault();
         var md = parts
            .OfType<SectionPart_MetaData>()
            .FirstOrDefault();
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
               partInfos.GetDataDirectoryForSectionPart( md ),
               flags,
               (UInt32) ep,
               partInfos.GetDataDirectoryForSectionPart( embeddedResources ),
               partInfos.GetDataDirectoryForSectionPart( snData ),
               default( DataDirectory ), // TODO: customize code manager
               default( DataDirectory ), // TODO: customize vtable fixups
               default( DataDirectory ), // TODO: customize exported address table jumps
               default( DataDirectory ) // TODO: customize managed native header
               );
      }

   }

   public class DefaultWritingStatus : WritingStatus
   {
      public DefaultWritingStatus(
         Int32 headersSize,
         ImageFileMachine machine,
         Int32? fileAlignment,
         Int32? sectionAlignment,
         Int64? imageBase,
         StrongNameInformation strongNameVariables,
         Int32 dataDirCount,
         Int32 sectionsCount
         ) : base( headersSize, machine, fileAlignment, sectionAlignment, imageBase, strongNameVariables, dataDirCount, sectionsCount )
      {

      }

      public SectionLayoutAggregator SectionLayouts { get; set; }

      public ColumnValueStorage<Int64> DataReferencesStorage { get; set; }

      public IEnumerable<SectionPartWithDataReferenceTargets> DataReferencesSectionParts { get; set; }
   }

   public class SectionLayoutInfo
   {
      public SectionLayoutInfo(
         SectionLayout layout,
         Int64 sectionStartOffset,
         TRVA sectionStartRVA,
         Int32 fileAlignment,
         ColumnValueStorage<Int64> rawValues
         )
      {
         ArgumentValidator.ValidateNotNull( "Layout", layout );

         var curRVA = sectionStartRVA;
         var curOffset = sectionStartOffset;

         var list = new List<SectionPartInfo>( layout.Parts.Count );
         foreach ( var part in layout.Parts )
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

                  list.Add( new SectionPartInfo( part, prePadding, size, curOffset, curRVA ) );

                  curOffset += (UInt32) size;
                  curRVA += (UInt32) size;
               }
            }
         }

         this.PartInfos = list.ToArrayProxy().CQ;
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

      public ArrayQuery<SectionPartInfo> PartInfos { get; }

      public SectionHeader SectionHeader { get; }
   }

   public sealed class SectionLayoutAggregator
   {

      public SectionLayoutAggregator( IEnumerable<SectionLayoutInfo> layoutInfos )
      {
         this.LayoutInfos = layoutInfos.ToArrayProxy().CQ;
         this.AllSectionParts = this.LayoutInfos.SelectMany( s => s.PartInfos.Select( p => p.Part ) ).ToArrayProxy().CQ;
         this.SectionPartInfos = this.LayoutInfos.SelectMany( s => s.PartInfos ).ToDictionary( i => i.Part, i => i, ReferenceEqualityComparer<SectionPart>.ReferenceBasedComparer ).ToDictionaryProxy().CQ;

      }

      public ArrayQuery<SectionLayoutInfo> LayoutInfos { get; }

      // TODO this might be useless property (consider extension method)
      public ArrayQuery<SectionPart> AllSectionParts { get; }

      public DictionaryQuery<SectionPart, SectionPartInfo> SectionPartInfos { get; }
   }

   public class SectionLayout
   {

      public SectionLayout()
      {
         this.Parts = new List<SectionPart>();
      }

      public SectionLayout( IEnumerable<SectionPart> parts )
         : this()
      {
         this.Parts.AddRange( parts );
      }

      public String Name { get; set; }

      public Byte[] NameBytes { get; set; }

      public SectionHeaderCharacteristics Characteristics { get; set; }

      public List<SectionPart> Parts { get; }
   }

   public interface SectionPart
   {
      Int32 DataAlignment { get; }

      Int32 GetDataSize( Int64 currentOffset, TRVA currentRVA, ColumnValueStorage<Int64> dataRefs );

      void WriteData( SectionPartWritingArgs args );
   }

   public interface SectionPartWithDataReferenceTargets : SectionPart
   {
      Tables RelatedTable { get; }

      Int32 RelatedTableColumnIndex { get; }
   }

   public abstract class SectionPartWithFixedAlignment : SectionPart
   {
      public SectionPartWithFixedAlignment( Int32 alignment, Boolean isPresent )
      {
         if ( alignment <= 0 )
         {
            throw new ArgumentOutOfRangeException( "Alignment" );
         }

         this.DataAlignment = alignment;
         this.Write = isPresent;
      }

      public Int32 DataAlignment { get; }

      public Int32 GetDataSize( Int64 currentOffset, TRVA currentRVA, ColumnValueStorage<Int64> rawValues )
      {
         return this.Write ?
            this.DoGetDataSize( currentOffset, currentRVA, rawValues ) :
            0;
      }

      public abstract void WriteData( SectionPartWritingArgs args );

      protected Boolean Write { get; }

      protected abstract Int32 DoGetDataSize( Int64 currentOffset, TRVA currentRVA, ColumnValueStorage<Int64> rawValues );
   }

   public abstract class SectionPartWriteableToArray : SectionPartWithFixedAlignment
   {
      public SectionPartWriteableToArray( Int32 alignment, Boolean write )
         : base( alignment, write )
      {
      }


      public override void WriteData( SectionPartWritingArgs args )
      {
         var array = args.ArrayHelper;
         var prePadding = args.PrePadding;
         var idx = prePadding;
         var capacity = idx + args.DataLength;
         array.CurrentMaxCapacity = capacity;
         var bytez = array.Array;
         var dummyIdx = 0;

         if ( !this.DoWriteData( args, bytez, ref idx ) )
         {
            bytez.ZeroOut( ref dummyIdx, capacity );
         }
         else
         {
            bytez.ZeroOut( ref dummyIdx, prePadding );
         }

         args.Stream.Write( bytez, capacity );
      }

      protected override Int32 DoGetDataSize( Int64 currentOffset, Int64 currentRVA, ColumnValueStorage<Int64> rawValues )
      {
         return this.DoGetDataSize( currentOffset, currentRVA );
      }

      protected abstract Int32 DoGetDataSize( Int64 currentOffset, TRVA currentRVA );

      protected abstract Boolean DoWriteData( SectionPartWritingArgs args, Byte[] array, ref Int32 idx );
   }

   public abstract class SectionPartWithFixedLength : SectionPartWriteableToArray
   {
      private readonly Int32 _dataSize;

      public SectionPartWithFixedLength( Int32 alignment, Boolean write, Int32 size )
         : base( alignment, write )
      {
         if ( size < 0 )
         {
            throw new ArgumentOutOfRangeException( "Size" );
         }
         this._dataSize = size;
      }

      protected override Int32 DoGetDataSize( Int64 currentOffset, TRVA currentRVA )
      {
         return this._dataSize;
      }
   }

   public abstract class SectionPartWithMultipleItems<TRow, TSizeInfo> : SectionPartWithFixedAlignment, SectionPartWithDataReferenceTargets
      where TRow : class
      where TSizeInfo : struct
   {
      private readonly Int32 _min;
      private readonly Int32 _max;
      private readonly List<TRow> _rows;
      private readonly ArrayProxy<TSizeInfo?> _sizes;

      public SectionPartWithMultipleItems(
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

      public Tables RelatedTable { get; }

      public Int32 RelatedTableColumnIndex { get; }

      protected abstract TSizeInfo? GetSizeInfo( Int32 rowIndex, TRow row, Int64 currentOffset, TRVA currentRVA, Int64 startOffset, TRVA startRVA );

      protected abstract Int32 GetSize( TSizeInfo sizeInfo );

      protected abstract TRVA GetValueForTableStreamFromSize( TRVA currentRVA, TSizeInfo sizeInfo );

      protected abstract TRVA GetValueForTableStreamFromRow( Int32 rowIndex, TRow row );

      protected abstract void WriteData( TRow row, TSizeInfo sizeInfo, Byte[] array );
   }

   public class SectionPart_MethodIL : SectionPartWithMultipleItems<MethodDefinition, SectionPart_MethodIL.MethodSizeInfo>
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

   public class SectionPart_FieldRVA : SectionPartWithMultipleItems<FieldRVA, Int32>
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

   public class SectionPart_EmbeddedManifests : SectionPartWithMultipleItems<ManifestResource, SectionPart_EmbeddedManifests.ManifestSizeInfo>
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

   public class SectionPartInfo
   {
      public SectionPartInfo(
         SectionPart part,
         Int32 prePadding,
         Int32 size,
         Int64 offset,
         TRVA rva
         )
      {
         ArgumentValidator.ValidateNotNull( "Part", part );

         this.Part = part;
         this.PrePadding = prePadding;
         this.Size = size;
         this.Offset = offset;
         this.RVA = rva;
      }

      public SectionPart Part { get; }

      public Int32 PrePadding { get; }

      public Int32 Size { get; }

      public Int64 Offset { get; }

      public TRVA RVA { get; }
   }

   public class SectionPart_CLIHeader : SectionPartWithFixedLength
   {
      internal const Int32 HEADER_SIZE = 0x48;


      public SectionPart_CLIHeader()
         : base( 4, true, HEADER_SIZE )
      {
      }

      protected override Boolean DoWriteData( SectionPartWritingArgs args, Byte[] array, ref Int32 idx )
      {
         var status = args.WritingStatus;
         status.CLIHeader.WriteCLIHeader( array, ref idx );
         status.PEDataDirectories[(Int32) DataDirectories.CLIHeader] = args.GetDataDirectoryForSectionPart( this );

         return true;
      }

   }

   public class SectionPart_StrongNameSignature : SectionPartWithFixedLength
   {
      public SectionPart_StrongNameSignature( StrongNameInformation snVars, ImageFileMachine machine )
         : base( machine.RequiresPE64() ? 0x10 : 0x04, true, snVars?.SignatureSize ?? 0 )
      {

      }

      protected override Boolean DoWriteData( SectionPartWritingArgs args, Byte[] array, ref Int32 idx )
      {
         // Don't write actual signature, since we don't have required information. The strong name signature will be written by WriteMetaData implementation.
         array.ZeroOut( ref idx, args.PrePadding + args.DataLength );
         return true;
      }
   }

   public class SectionPart_MetaData : SectionPartWithFixedLength
   {
      public SectionPart_MetaData( Int32 size )
         : base( 0x04, true, size )
      {

      }

      protected override Boolean DoWriteData( SectionPartWritingArgs args, Byte[] array, ref Int32 idx )
      {
         // This method will never get really called
         throw new NotSupportedException( "This method should not be called." );
      }
   }

   public class SectionPart_ImportAddressTable : SectionPartWithFixedLength
   {
      public SectionPart_ImportAddressTable( ImageFileMachine machine )
         : base( 0x04, !machine.RequiresPE64(), 0x08 )
      {
      }


      protected override Boolean DoWriteData( SectionPartWritingArgs args, Byte[] array, ref Int32 idx )
      {
         var importDir = args.WritingStatus.SectionLayouts.AllSectionParts.OfType<SectionPart_ImportDirectory>().FirstOrDefault();
         var retVal = importDir != null;
         if ( retVal )
         {
            array
               .WriteInt32LEToBytes( ref idx, importDir.CorMainRVA ) // RVA of _CorDll/ExeMain
               .WriteInt32LEToBytes( ref idx, 0 ); // Terminating entry

            args.WritingStatus.PEDataDirectories[(Int32) DataDirectories.ImportAddressTable] = args.GetDataDirectoryForSectionPart( this );
         }
         return retVal;
      }
   }

   public class SectionPart_ImportDirectory : SectionPartWriteableToArray
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

      protected override Int32 DoGetDataSize( Int64 currentOffset, TRVA currentRVA )
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

      protected override Boolean DoWriteData( SectionPartWritingArgs args, Byte[] array, ref Int32 idx )
      {
         var wStatus = args.WritingStatus;
         var addressTable = wStatus.SectionLayouts.AllSectionParts.OfType<SectionPart_ImportAddressTable>().FirstOrDefault();

         var retVal = addressTable != null;
         if ( retVal )
         {
            array
               // Import directory
               .WriteUInt32LEToBytes( ref idx, this._lookupTableRVA )
               .WriteInt32LEToBytes( ref idx, 0 ) // TimeDateStamp
               .WriteInt32LEToBytes( ref idx, 0 ) // ForwarderChain
               .WriteUInt32LEToBytes( ref idx, this._mscoreeRVA ) // Name of module
               .WriteUInt32LEToBytes( ref idx, (UInt32) wStatus.SectionLayouts.SectionPartInfos[addressTable].RVA ) // Address table RVA
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

            wStatus.PEDataDirectories[(Int32) DataDirectories.ImportTable] = args.GetDataDirectoryForSectionPart( this );
         }

         return retVal;
      }
   }

   public class SectionPart_StartupCode : SectionPartWithFixedLength
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

      protected override Boolean DoWriteData( SectionPartWritingArgs args, Byte[] array, ref Int32 idx )
      {
         var sectionLayouts = args.WritingStatus.SectionLayouts;
         var addressTable = sectionLayouts.AllSectionParts.OfType<SectionPart_ImportAddressTable>().FirstOrDefault();
         var retVal = addressTable != null;
         if ( retVal )
         {
            array
               .ZeroOut( ref idx, PADDING ) // Padding - 2 zero bytes
               .WriteUInt16LEToBytes( ref idx, 0x25FF ) // JMP
               .WriteUInt32LEToBytes( ref idx, this._imageBase + (UInt32) ( sectionLayouts.SectionPartInfos[addressTable].RVA ) ); // First entry of address table = RVA of _CorDll/ExeMain

            args.WritingStatus.EntryPointRVA = (Int32) ( sectionLayouts.SectionPartInfos[this].RVA + this.EntryPointOffset );
         }
         return retVal;
      }
   }

   public class SectionPart_RelocDirectory : SectionPartWithFixedLength
   {
      private const Int32 SIZE = 0x0C;
      private const UInt32 RELOCATION_PAGE_MASK = 0x0FFF; // ECMA-335, p. 282
      private const UInt16 RELOCATION_FIXUP_TYPE = 0x3; // ECMA-335, p. 282

      public SectionPart_RelocDirectory( ImageFileMachine machine )
         : base( 0x04, !machine.RequiresPE64(), SIZE )
      {

      }

      protected override Boolean DoWriteData( SectionPartWritingArgs args, Byte[] array, ref Int32 idx )
      {
         var sectionLayouts = args.WritingStatus.SectionLayouts;
         var startupCode = sectionLayouts.AllSectionParts.OfType<SectionPart_StartupCode>().FirstOrDefault();
         var retVal = startupCode != null;
         if ( retVal )
         {
            var rva = (UInt32) ( sectionLayouts.SectionPartInfos[startupCode].RVA + startupCode.EntryPointInstructionAddressOffset );
            array
               .WriteUInt32LEToBytes( ref idx, rva & ( ~RELOCATION_PAGE_MASK ) ) // Page RVA
               .WriteInt32LEToBytes( ref idx, SIZE ) // Block size
               .WriteUInt16LEToBytes( ref idx, (UInt16) ( ( RELOCATION_FIXUP_TYPE << 12 ) | ( rva & RELOCATION_PAGE_MASK ) ) ) // Type (high 4 bits) + Offset (lower 12 bits) + dummy entry (16 bits)
               .WriteUInt16LEToBytes( ref idx, 0 ); // Terminating entry

            args.WritingStatus.PEDataDirectories[(Int32) DataDirectories.BaseRelocationTable] = args.GetDataDirectoryForSectionPart( this );
         }
         return retVal;
      }
   }

   public class SectionPart_DebugDirectory : SectionPartWithFixedLength
   {
      private const Int32 ALIGNMENT = 0x04;
      private const Int32 HEADER_SIZE = 0x1C;

      private readonly WritingOptions_Debug _options;

      public SectionPart_DebugDirectory( WritingOptions_Debug options )
         : base( ALIGNMENT, !( options?.DebugData ).IsNullOrEmpty(), ( HEADER_SIZE + ( options?.DebugData?.Length ?? 0 ) ) )
      {
         this._options = options;
      }

      protected override Boolean DoWriteData( SectionPartWritingArgs args, Byte[] array, ref Int32 idx )
      {
         var dbgData = this._options?.DebugData;
         var retVal = !dbgData.IsNullOrEmpty();
         if ( retVal )
         {
            var parts = args.WritingStatus.SectionLayouts.SectionPartInfos;
            var dbgOptions = this._options;
            var dataOffset = (UInt32) ( parts[this].Offset + HEADER_SIZE );
            var dataRVA = (UInt32) ( parts[this].RVA + HEADER_SIZE );
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
            args.WritingStatus.PEDataDirectories[(Int32) DataDirectories.Debug] = args.GetDataDirectoryForSectionPart( this );
            args.WritingStatus.DebugInformation = debugInfo;
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
               .SelectMany( s => s?.CreateDataReferenceSectionParts( this._md, mdStreams ) ?? Empty<SectionPartWithDataReferenceTargets>.Enumerable );
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
   public static DataDirectory GetDataDirectoryForSectionPart( this SectionPartWritingArgs args, SectionPart part )
   {
      return part == null ? default( DataDirectory ) : args.WritingStatus.SectionLayouts.SectionPartInfos[part].GetDataDirectory();
   }

   public static DataDirectory GetDataDirectory( this SectionPartInfo info )
   {
      return new DataDirectory( (UInt32) info.RVA, (UInt32) info.Size );
   }

   internal static DataDirectory GetDataDirectoryForSectionPart( this DictionaryQuery<SectionPart, SectionPartInfo> partInfos, SectionPart partOrNull )
   {
      SectionPartInfo info;
      return partOrNull == null || !partInfos.TryGetValue( partOrNull, out info ) ?
         default( DataDirectory ) :
         info.GetDataDirectory();
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
}