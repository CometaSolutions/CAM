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
using CILAssemblyManipulator.Physical.IO;
using CollectionsWithRoles.API;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using TRVA = System.UInt32;

namespace CILAssemblyManipulator.Physical.IO
{
   public sealed class ImageInformation
   {
      public ImageInformation(
         PEInformation peInfo,
         CLIInformation cliInfo
         )
      {
         ArgumentValidator.ValidateNotNull( "PE information", peInfo );
         ArgumentValidator.ValidateNotNull( "CLI information", cliInfo );

         this.PEInformation = peInfo;
         this.CLIInformation = cliInfo;
      }

      public PEInformation PEInformation { get; }

      public CLIInformation CLIInformation { get; }
   }

   #region PE-related

   public sealed class PEInformation
   {
      public PEInformation(
         DOSHeader dosHeader,
         NTHeader ntHeader,
         ArrayQuery<SectionHeader> sectionHeaders
         )
      {
         ArgumentValidator.ValidateNotNull( "DOS header", dosHeader );
         ArgumentValidator.ValidateNotNull( "NT header", ntHeader );
         ArgumentValidator.ValidateNotNull( "Section headers", sectionHeaders );

         this.DOSHeader = dosHeader;
         this.NTHeader = ntHeader;
         this.SectionHeaders = sectionHeaders;
      }
      public DOSHeader DOSHeader { get; }
      public NTHeader NTHeader { get; }
      public ArrayQuery<SectionHeader> SectionHeaders { get; }
   }

   public sealed class DOSHeader
   {
      [CLSCompliant( false )]
      public DOSHeader( Int16 signature, UInt32 ntHeadersOffset )
      {
         this.Signature = signature;
         this.NTHeaderOffset = ntHeadersOffset;
      }
      public Int16 Signature { get; }

      [CLSCompliant( false )]
      public UInt32 NTHeaderOffset { get; }
   }

   public sealed class NTHeader
   {
      public NTHeader(
         Int32 signature,
         FileHeader fileHeader,
         OptionalHeader optionalHeader
         )
      {
         ArgumentValidator.ValidateNotNull( "File header", fileHeader );
         ArgumentValidator.ValidateNotNull( "Optional header", optionalHeader );

         this.Signature = signature;
         this.FileHeader = fileHeader;
         this.OptionalHeader = optionalHeader;
      }

      public Int32 Signature { get; }

      public FileHeader FileHeader { get; }

      public OptionalHeader OptionalHeader { get; }
   }

   public sealed class FileHeader
   {
      [CLSCompliant( false )]
      public FileHeader(
         ImageFileMachine machine,
         UInt16 numberOfSections,
         UInt32 timeDateStamp,
         UInt32 pointerToSymbolTable,
         UInt32 numberOfSymbols,
         UInt16 optionalHeaderSize,
         FileHeaderCharacteristics characteristics
         )
      {
         this.Machine = machine;
         this.NumberOfSections = numberOfSections;
         this.TimeDateStamp = timeDateStamp;
         this.PointerToSymbolTable = pointerToSymbolTable;
         this.NumberOfSymbols = numberOfSymbols;
         this.OptionalHeaderSize = optionalHeaderSize;
         this.Characteristics = characteristics;
      }

      public ImageFileMachine Machine { get; }

      [CLSCompliant( false )]
      public UInt16 NumberOfSections { get; }

      [CLSCompliant( false )]
      public UInt32 TimeDateStamp { get; }

      [CLSCompliant( false )]
      public UInt32 PointerToSymbolTable { get; }

      [CLSCompliant( false )]
      public UInt32 NumberOfSymbols { get; }

      [CLSCompliant( false )]
      public UInt16 OptionalHeaderSize { get; }

      public FileHeaderCharacteristics Characteristics { get; }
   }


   public abstract class OptionalHeader
   {
      internal OptionalHeader(
         Byte majorLinkerVersion,
         Byte minorLinkerVersion,
         UInt32 sizeOfCode,
         UInt32 sizeOfInitializedData,
         UInt32 sizeOfUninitializedData,
         TRVA entryPointRVA,
         TRVA baseOfCodeRVA,
         TRVA baseOfDataRVA,
         UInt64 imageBase,
         UInt32 sectionAlignment,
         UInt32 fileAlignment,
         UInt16 majorOSVersion,
         UInt16 minorOSVersion,
         UInt16 majorUserVersion,
         UInt16 minorUserVersion,
         UInt16 majorSubsystemVersion,
         UInt16 minorSubsystemVersion,
         UInt32 win32VersionValue,
         UInt32 imageSize,
         UInt32 headerSize,
         UInt32 fileChecksum,
         Subsystem subsystem,
         DLLFlags dllCharacteristics,
         UInt64 stackReserveSize,
         UInt64 stackCommitSize,
         UInt64 heapReserveSize,
         UInt64 heapCommitSize,
         Int32 loaderFlags,
         UInt32 numberOfDataDirectories,
         ArrayQuery<DataDirectory> dataDirectories
         )
      {
         this.MajorLinkerVersion = majorLinkerVersion;
         this.MinorLinkerVersion = minorLinkerVersion;
         this.SizeOfCode = sizeOfCode;
         this.SizeOfInitializedData = sizeOfInitializedData;
         this.SizeOfUninitializedData = sizeOfUninitializedData;
         this.EntryPointRVA = entryPointRVA;
         this.BaseOfCodeRVA = baseOfCodeRVA;
         this.BaseOfDataRVA = baseOfDataRVA;
         this.ImageBase = imageBase;
         this.SectionAlignment = sectionAlignment;
         this.FileAlignment = fileAlignment;
         this.MajorOSVersion = majorOSVersion;
         this.MinorOSVersion = minorOSVersion;
         this.MajorUserVersion = majorUserVersion;
         this.MinorUserVersion = minorUserVersion;
         this.MajorSubsystemVersion = majorSubsystemVersion;
         this.MinorSubsystemVersion = minorSubsystemVersion;
         this.Win32VersionValue = win32VersionValue;
         this.ImageSize = imageSize;
         this.HeaderSize = headerSize;
         this.FileChecksum = fileChecksum;
         this.Subsystem = subsystem;
         this.DLLCharacteristics = dllCharacteristics;
         this.StackReserveSize = stackReserveSize;
         this.StackCommitSize = stackCommitSize;
         this.HeapReserveSize = heapReserveSize;
         this.HeapCommitSize = heapCommitSize;
         this.LoaderFlags = loaderFlags;
         this.NumberOfDataDirectories = numberOfDataDirectories;
         this.DataDirectories = dataDirectories ?? EmptyArrayProxy<DataDirectory>.Query;
      }

      // Standard
      public abstract OptionalHeaderKind OptionalHeaderKind { get; }

      public Byte MajorLinkerVersion { get; }

      public Byte MinorLinkerVersion { get; }

      [CLSCompliant( false )]
      public UInt32 SizeOfCode { get; }

      [CLSCompliant( false )]
      public UInt32 SizeOfInitializedData { get; }

      [CLSCompliant( false )]
      public UInt32 SizeOfUninitializedData { get; }

      [CLSCompliant( false )]
      public TRVA EntryPointRVA { get; }

      [CLSCompliant( false )]
      public TRVA BaseOfCodeRVA { get; }

      [CLSCompliant( false )]
      public TRVA BaseOfDataRVA { get; }

      // NT-Specific 
      [CLSCompliant( false )]
      public UInt64 ImageBase { get; }

      [CLSCompliant( false )]
      public UInt32 SectionAlignment { get; }

      [CLSCompliant( false )]
      public UInt32 FileAlignment { get; }

      [CLSCompliant( false )]
      public UInt16 MajorOSVersion { get; }

      [CLSCompliant( false )]
      public UInt16 MinorOSVersion { get; }

      [CLSCompliant( false )]
      public UInt16 MajorUserVersion { get; }

      [CLSCompliant( false )]
      public UInt16 MinorUserVersion { get; }

      [CLSCompliant( false )]
      public UInt16 MajorSubsystemVersion { get; }

      [CLSCompliant( false )]
      public UInt16 MinorSubsystemVersion { get; }

      [CLSCompliant( false )]
      public UInt32 Win32VersionValue { get; }

      [CLSCompliant( false )]
      public UInt32 ImageSize { get; }

      [CLSCompliant( false )]
      public UInt32 HeaderSize { get; }

      [CLSCompliant( false )]
      public UInt32 FileChecksum { get; }

      public Subsystem Subsystem { get; }

      public DLLFlags DLLCharacteristics { get; }

      [CLSCompliant( false )]
      public UInt64 StackReserveSize { get; }

      [CLSCompliant( false )]
      public UInt64 StackCommitSize { get; }

      [CLSCompliant( false )]
      public UInt64 HeapReserveSize { get; }

      [CLSCompliant( false )]
      public UInt64 HeapCommitSize { get; }

      public Int32 LoaderFlags { get; }

      [CLSCompliant( false )]
      public UInt32 NumberOfDataDirectories { get; }

      public ArrayQuery<DataDirectory> DataDirectories { get; }
   }

   public sealed class OptionalHeader32 : OptionalHeader
   {
      [CLSCompliant( false )]
      public OptionalHeader32(
         Byte majorLinkerVersion,
         Byte minorLinkerVersion,
         UInt32 sizeOfCode,
         UInt32 sizeOfInitializedData,
         UInt32 sizeOfUninitializedData,
         TRVA entryPointRVA,
         TRVA baseOfCodeRVA,
         TRVA baseOfDataRVA,
         UInt32 imageBase,
         UInt32 sectionAlignment,
         UInt32 fileAlignment,
         UInt16 majorOSVersion,
         UInt16 minorOSVersion,
         UInt16 majorUserVersion,
         UInt16 minorUserVersion,
         UInt16 majorSubsystemVersion,
         UInt16 minorSubsystemVersion,
         UInt32 win32VersionValue,
         UInt32 imageSize,
         UInt32 headerSize,
         UInt32 fileChecksum,
         Subsystem subsystem,
         DLLFlags dllCharacteristics,
         UInt32 stackReserveSize,
         UInt32 stackCommitSize,
         UInt32 heapReserveSize,
         UInt32 heapCommitSize,
         Int32 loaderFlags,
         UInt32 numberOfDataDirectories,
         ArrayQuery<DataDirectory> dataDirectories
         )
         : base(
         majorLinkerVersion,
         minorLinkerVersion,
         sizeOfCode,
         sizeOfInitializedData,
         sizeOfUninitializedData,
         entryPointRVA,
         baseOfCodeRVA,
         baseOfDataRVA,
         imageBase,
         sectionAlignment,
         fileAlignment,
         majorOSVersion,
         minorOSVersion,
         majorUserVersion,
         minorUserVersion,
         majorSubsystemVersion,
         minorSubsystemVersion,
         win32VersionValue,
         imageSize,
         headerSize,
         fileChecksum,
         subsystem,
         dllCharacteristics,
         stackReserveSize,
         stackCommitSize,
         heapReserveSize,
         heapCommitSize,
         loaderFlags,
         numberOfDataDirectories,
         dataDirectories
         )
      {
      }

      public override OptionalHeaderKind OptionalHeaderKind
      {
         get
         {
            return OptionalHeaderKind.Optional32;
         }
      }
   }

   public sealed class OptionalHeader64 : OptionalHeader
   {
      [CLSCompliant( false )]
      public OptionalHeader64(
         Byte majorLinkerVersion,
         Byte minorLinkerVersion,
         UInt32 sizeOfCode,
         UInt32 sizeOfInitializedData,
         UInt32 sizeOfUninitializedData,
         TRVA entryPointRVA,
         TRVA baseOfCodeRVA,
         UInt64 imageBase,
         UInt32 sectionAlignment,
         UInt32 fileAlignment,
         UInt16 majorOSVersion,
         UInt16 minorOSVersion,
         UInt16 majorUserVersion,
         UInt16 minorUserVersion,
         UInt16 majorSubsystemVersion,
         UInt16 minorSubsystemVersion,
         UInt32 win32VersionValue,
         UInt32 imageSize,
         UInt32 headerSize,
         UInt32 fileChecksum,
         Subsystem subsystem,
         DLLFlags dllCharacteristics,
         UInt64 stackReserveSize,
         UInt64 stackCommitSize,
         UInt64 heapReserveSize,
         UInt64 heapCommitSize,
         Int32 loaderFlags,
         UInt32 numberOfDataDirectories,
         ArrayQuery<DataDirectory> dataDirectories
         )
         : base(
         majorLinkerVersion,
         minorLinkerVersion,
         sizeOfCode,
         sizeOfInitializedData,
         sizeOfUninitializedData,
         entryPointRVA,
         baseOfCodeRVA,
         0u,
         imageBase,
         sectionAlignment,
         fileAlignment,
         majorOSVersion,
         minorOSVersion,
         majorUserVersion,
         minorUserVersion,
         majorSubsystemVersion,
         minorSubsystemVersion,
         win32VersionValue,
         imageSize,
         headerSize,
         fileChecksum,
         subsystem,
         dllCharacteristics,
         stackReserveSize,
         stackCommitSize,
         heapReserveSize,
         heapCommitSize,
         loaderFlags,
         numberOfDataDirectories,
         dataDirectories
         )
      {
      }

      public override OptionalHeaderKind OptionalHeaderKind
      {
         get
         {
            return OptionalHeaderKind.Optional64;
         }
      }
   }

   public struct DataDirectory : IEquatable<DataDirectory>
   {
      [CLSCompliant( false )]
      public DataDirectory( TRVA rva, UInt32 size )
      {
         this.RVA = rva;
         this.Size = size;
      }
      [CLSCompliant( false )]
      public TRVA RVA { get; }

      [CLSCompliant( false )]
      public UInt32 Size { get; }

      public override Boolean Equals( Object obj )
      {
         return obj is DataDirectory && this.Equals( (DataDirectory) obj );
      }

      public override Int32 GetHashCode()
      {
         return unchecked((Int32) ( ( 17 * 23 + this.RVA ) * 23 + this.Size ));
      }

      public Boolean Equals( DataDirectory other )
      {
         return this.RVA == other.RVA && this.Size == other.Size;
      }

      public static Boolean operator ==( DataDirectory x, DataDirectory y )
      {
         return x.Equals( y );
      }

      public static Boolean operator !=( DataDirectory x, DataDirectory y )
      {
         return !( x == y );
      }
   }

   public enum DataDirectories
   {
      ExportTable,
      ImportTable,
      ResourceTable,
      ExceptionTable,
      CertificateTable,
      BaseRelocationTable,
      Debug,
      Copyright,
      Globals,
      TLSTable,
      LoadConfigTable,
      BoundImport,
      ImportAddressTable,
      DelayImportDescriptor,
      CLIHeader,
      Reserved
   }

   public enum OptionalHeaderKind : short
   {
      Optional32 = 0x010B,
      Optional64 = 0x020B,
      // OptionalROM = 0x0107
   }

   [Flags]
   public enum FileHeaderCharacteristics : short
   {
      RelocsStripped = 0x0001,
      ExecutableImage = 0x0002,
      LineNumsStripped = 0x0004,
      LocalSymsStripped = 0x0008,
      AggressiveWSTrim = 0x0010,
      LargeAddressAware = 0x0020,
      Reserved1 = 0x0040,
      BytesReversedLo = 0x0080,
      Machine32Bit = 0x0100,
      DebugStripped = 0x0200,
      RemovableRunFromSwap = 0x0400,
      NetRunFromSwap = 0x0800,
      System = 0x1000,
      Dll = 0x2000,
      UPSystemOnly = 0x4000,
      BytesReversedHi = unchecked((Int16) 0x8000),
   }

   public sealed class SectionHeader
   {
      private readonly Lazy<String> _name;

      [CLSCompliant( false )]
      public SectionHeader(
         ArrayQuery<Byte> nameBytes,
         TRVA virtualSize,
         TRVA virtualAddress,
         UInt32 rawDataSize,
         UInt32 rawDataPointer,
         UInt32 relocationsPointer,
         UInt32 lineNumbersPointer,
         UInt16 numberOfRelocations,
         UInt16 numberOfLineNumbers,
         SectionHeaderCharacteristics characteristics
         )
      {
         this.NameBytes = nameBytes;
         this.VirtualSize = virtualSize;
         this.VirtualAddress = virtualAddress;
         this.RawDataSize = rawDataSize;
         this.RawDataPointer = rawDataPointer;
         this.RelocationsPointer = relocationsPointer;
         this.LineNumbersPointer = lineNumbersPointer;
         this.NumberOfRelocations = numberOfRelocations;
         this.NumberOfLineNumbers = numberOfLineNumbers;
         this.Characteristics = characteristics;

         this._name = new Lazy<String>( () =>
            new String( this.NameBytes.TakeWhile( b => b != 0 ).Select( b => (Char) b ).ToArray() ),
            LazyThreadSafetyMode.ExecutionAndPublication
            );
      }

      public ArrayQuery<Byte> NameBytes { get; }
      public String Name
      {
         get
         {
            return this._name.Value;
         }
      }

      [CLSCompliant( false )]
      public TRVA VirtualSize { get; }

      [CLSCompliant( false )]
      public TRVA VirtualAddress { get; }

      [CLSCompliant( false )]
      public UInt32 RawDataSize { get; }

      [CLSCompliant( false )]
      public UInt32 RawDataPointer { get; }

      [CLSCompliant( false )]
      public UInt32 RelocationsPointer { get; }

      [CLSCompliant( false )]
      public UInt32 LineNumbersPointer { get; }

      [CLSCompliant( false )]
      public UInt16 NumberOfRelocations { get; }

      [CLSCompliant( false )]
      public UInt16 NumberOfLineNumbers { get; }

      public SectionHeaderCharacteristics Characteristics { get; }
   }

   [Flags]
   public enum SectionHeaderCharacteristics : int
   {
      //Reserved1 = 0x00000000,
      Reserved2 = 0x00000001,
      Reserved3 = 0x00000002,
      Reserved4 = 0x00000004,
      Type_NoPad = 0x00000008,
      Reserved5 = 0x00000010,
      Contains_Code = 0x00000020,
      Contains_InitializedData = 0x00000040,
      Contains_UninitializedData = 0x00000080,
      Link_Other = 0x00000100,
      Link_Info = 0x00000200,
      Link_Reserved = 0x00000400,
      Link_Remove = 0x00000800,
      Link_COMDAT = 0x00001000,
      Reserved6 = 0x00002000,
      NoDeferredSpeculativeExceptions = 0x00004000,
      GPRelative = 0x00008000,
      Reserved7 = 0x00010000,
      Memory_Purgeable = 0x00020000,
      Memory_Locked = 0x00040000,
      Memory_PreLoad = 0x00080000,
      Align_1Bytes = 0x00100000,
      Align_2Bytes = 0x00200000,
      Align_4Bytes = 0x00300000,
      Align_8Bytes = 0x00400000,
      Align_16Bytes = 0x00500000,
      Align_32Bytes = 0x00600000,
      Align_64Bytes = 0x00700000,
      Align_128Bytes = 0x00800000,
      Align_256Bytes = 0x00900000,
      Align_512Bytes = 0x00A00000,
      Align_1028Bytes = 0x00B00000,
      Align_2048Bytes = 0x00C00000,
      Align_4096Bytes = 0x00D00000,
      Align_8192Bytes = 0x00E00000,
      Link_NoRelocationsOverflow = 0x01000000,
      Memory_Discardable = 0x02000000,
      Memory_NotCached = 0x04000000,
      Memory_NotPaged = 0x08000000,
      Memory_Shared = 0x10000000,
      Memory_Execute = 0x20000000,
      Memory_Read = 0x40000000,
      Memory_Write = unchecked((Int32) 0x80000000),
   }

   #endregion

   #region CIL-related

   public sealed class CLIInformation
   {
      [CLSCompliant( false )]
      public CLIInformation(
         CLIHeader cliHeader,
         MetaDataRoot mdRoot,
         ArrayQuery<Byte> strongNameSignature,
         ArrayQuery<TRVA> methodRVAs,
         ArrayQuery<TRVA> fieldRVAs
         )
      {
         ArgumentValidator.ValidateNotNull( "CLI header", cliHeader );
         ArgumentValidator.ValidateNotNull( "MetaData root", mdRoot );
         ArgumentValidator.ValidateNotNull( "Method RVAs", methodRVAs );
         ArgumentValidator.ValidateNotNull( "Field RVAs", fieldRVAs );

         this.CLIHeader = cliHeader;
         this.MetaDataRoot = mdRoot;
         this.StrongNameSignature = strongNameSignature;
         this.MethodRVAs = methodRVAs;
         this.FieldRVAs = fieldRVAs;
      }

      public CLIHeader CLIHeader { get; }

      public MetaDataRoot MetaDataRoot { get; }

      public ArrayQuery<Byte> StrongNameSignature { get; }

      [CLSCompliant( false )]
      public ArrayQuery<TRVA> MethodRVAs { get; }

      [CLSCompliant( false )]
      public ArrayQuery<TRVA> FieldRVAs { get; }
   }

   public sealed class CLIHeader
   {
      [CLSCompliant( false )]
      public CLIHeader(
         UInt32 headerSize,
         UInt16 majorRuntimeVersion,
         UInt16 minorRuntimeVersion,
         DataDirectory metaData,
         ModuleFlags flags,
         TableIndex? entryPointToken,
         DataDirectory resources,
         DataDirectory strongNameSignature,
         DataDirectory codeManagerTable,
         DataDirectory vTableFixups,
         DataDirectory exportAddressTableJumps,
         DataDirectory managedNativeHeader
         )
      {
         this.HeaderSize = headerSize;
         this.MajorRuntimeVersion = majorRuntimeVersion;
         this.MinorRuntimeVersion = minorRuntimeVersion;
         this.MetaData = metaData;
         this.Flags = flags;
         this.EntryPointToken = entryPointToken;
         this.Resources = resources;
         this.StrongNameSignature = strongNameSignature;
         this.CodeManagerTable = codeManagerTable;
         this.VTableFixups = vTableFixups;
         this.ExportAddressTableJumps = exportAddressTableJumps;
         this.ManagedNativeHeader = managedNativeHeader;
      }

      [CLSCompliant( false )]
      public UInt32 HeaderSize { get; }

      [CLSCompliant( false )]
      public UInt16 MajorRuntimeVersion { get; }

      [CLSCompliant( false )]
      public UInt16 MinorRuntimeVersion { get; }

      public DataDirectory MetaData { get; }

      public ModuleFlags Flags { get; }

      public TableIndex? EntryPointToken { get; }

      public DataDirectory Resources { get; }

      public DataDirectory StrongNameSignature { get; }

      public DataDirectory CodeManagerTable { get; }

      public DataDirectory VTableFixups { get; }

      public DataDirectory ExportAddressTableJumps { get; }

      public DataDirectory ManagedNativeHeader { get; }
   }

   public sealed class MetaDataRoot
   {
      private static readonly Encoding VERSION_ENCODING = new UTF8Encoding( false, false );

      private readonly Lazy<String> _versionString;

      [CLSCompliant( false )]
      public MetaDataRoot(
         Int32 signature,
         UInt16 majorVersion,
         UInt16 minorVersion,
         Int32 reserved,
         UInt32 versionStringLength,
         ArrayQuery<Byte> versionStringBytes,
         StorageFlags storageFlags,
         Byte reserved2,
         UInt16 numberOfStreams,
         ArrayQuery<MetaDataStreamHeader> streamHeaders
         )
      {
         ArgumentValidator.ValidateNotNull( "Version string bytes", versionStringBytes );
         ArgumentValidator.ValidateNotNull( "Stream headers", streamHeaders );

         this.Signature = signature;
         this.MajorVersion = majorVersion;
         this.MinorVersion = minorVersion;
         this.Reserved = reserved;
         this.VersionStringLength = versionStringLength;
         this.VersionStringBytes = versionStringBytes;
         this.StorageFlags = storageFlags;
         this.Reserved2 = reserved2;
         this.NumberOfStreams = numberOfStreams;
         this.StreamHeaders = streamHeaders;

         this._versionString = new Lazy<String>(
            () => VERSION_ENCODING.GetString( this.VersionStringBytes.TakeWhile( b => b != 0 ).ToArray() ),
            LazyThreadSafetyMode.ExecutionAndPublication
            );
      }

      public Int32 Signature { get; }

      [CLSCompliant( false )]
      public UInt16 MajorVersion { get; }

      [CLSCompliant( false )]
      public UInt16 MinorVersion { get; }

      public Int32 Reserved { get; }

      [CLSCompliant( false )]
      public UInt32 VersionStringLength { get; }

      public ArrayQuery<Byte> VersionStringBytes { get; }

      public String VersionString
      {
         get
         {
            return this._versionString.Value;
         }
      }

      public StorageFlags StorageFlags { get; }

      public Byte Reserved2 { get; }

      [CLSCompliant( false )]
      public UInt16 NumberOfStreams { get; }

      public ArrayQuery<MetaDataStreamHeader> StreamHeaders { get; }
   }

   public enum StorageFlags : byte
   {
      Normal = 0,
      ExtraData = 1
   }

   public struct MetaDataStreamHeader
   {
      private readonly Lazy<String> _nameString;

      [CLSCompliant( false )]
      public MetaDataStreamHeader(
         UInt32 offset,
         UInt32 size,
         ArrayQuery<Byte> nameBytes
         )
      {
         ArgumentValidator.ValidateNotNull( "Name bytes", nameBytes );

         this.Offset = offset;
         this.Size = size;
         this.NameBytes = nameBytes;

         this._nameString = new Lazy<String>(
            () => new String( nameBytes.TakeWhile( b => b != 0 ).Select( b => (Char) b ).ToArray() ),
            LazyThreadSafetyMode.ExecutionAndPublication );
      }

      [CLSCompliant( false )]
      public UInt32 Offset { get; }

      [CLSCompliant( false )]
      public UInt32 Size { get; }

      public ArrayQuery<Byte> NameBytes { get; }

      public String Name
      {
         get
         {
            return this._nameString.Value;
         }
      }
   }

   #endregion
}

public static partial class E_CILPhysical
{
   #region PE-related

   public static PEInformation NewPEImageInformationFromStream( this StreamHelper stream )
   {
      // Read DOS header
      var dosHeader = stream.NewDOSHeaderFromStream();

      // Read NT header
      stream.Stream.SeekFromBegin( dosHeader.NTHeaderOffset );
      var ntHeader = stream.NewNTHeaderFromStream();

      // Read section headers
      stream.Stream.SeekFromBegin( dosHeader.NTHeaderOffset + ntHeader.FileHeader.OptionalHeaderSize );
      var sections = stream.ReadSequentialElements( ntHeader.FileHeader.NumberOfSections, s => s.NewSectionHeaderFromStream() );
      return new PEInformation(
         dosHeader,
         ntHeader,
         sections
         );
   }

   public static DOSHeader NewDOSHeaderFromStream( this StreamHelper stream )
   {
      return new DOSHeader(
         stream.ReadInt16LEFromBytes(),
         stream.Skip( 0x3A ).ReadUInt32LEFromBytes()
         );
   }

   public static NTHeader NewNTHeaderFromStream( this StreamHelper stream )
   {
      return new NTHeader(
         stream.ReadInt32LEFromBytes(),
         stream.NewFileHeaderFromStream(),
         stream.NewOptionalHeaderFromStream()
         );
   }

   public static FileHeader NewFileHeaderFromStream( this StreamHelper stream )
   {
      return new FileHeader(
         (ImageFileMachine) stream.ReadInt16LEFromBytes(),
         stream.ReadUInt16LEFromBytes(),
         stream.ReadUInt32LEFromBytes(),
         stream.ReadUInt32LEFromBytes(),
         stream.ReadUInt32LEFromBytes(),
         stream.ReadUInt16LEFromBytes(),
         (FileHeaderCharacteristics) stream.ReadInt16LEFromBytes()
         );
   }

   public static OptionalHeader NewOptionalHeaderFromStream( this StreamHelper stream )
   {
      var kind = (OptionalHeaderKind) stream.ReadInt16LEFromBytes();
      UInt32 ddDirCount;
      switch ( kind )
      {
         case OptionalHeaderKind.Optional32:
            return new OptionalHeader32(
               stream.ReadByteFromBytes(),
               stream.ReadByteFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadRVAFromBytes(),
               stream.ReadRVAFromBytes(),
               stream.ReadRVAFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadUInt16LEFromBytes(),
               stream.ReadUInt16LEFromBytes(),
               stream.ReadUInt16LEFromBytes(),
               stream.ReadUInt16LEFromBytes(),
               stream.ReadUInt16LEFromBytes(),
               stream.ReadUInt16LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               (Subsystem) stream.ReadInt16LEFromBytes(),
               (DLLFlags) stream.ReadInt16LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadInt32LEFromBytes(),
               ( ddDirCount = stream.ReadUInt32LEFromBytes() ),
               stream.ReadSequentialElements( ddDirCount, s => s.ReadDataDirectory() )
               );
         case OptionalHeaderKind.Optional64:
            return new OptionalHeader64(
               stream.ReadByteFromBytes(),
               stream.ReadByteFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadRVAFromBytes(),
               stream.ReadRVAFromBytes(),
               stream.ReadUInt64LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadUInt16LEFromBytes(),
               stream.ReadUInt16LEFromBytes(),
               stream.ReadUInt16LEFromBytes(),
               stream.ReadUInt16LEFromBytes(),
               stream.ReadUInt16LEFromBytes(),
               stream.ReadUInt16LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               stream.ReadUInt32LEFromBytes(),
               (Subsystem) stream.ReadInt16LEFromBytes(),
               (DLLFlags) stream.ReadInt16LEFromBytes(),
               stream.ReadUInt64LEFromBytes(),
               stream.ReadUInt64LEFromBytes(),
               stream.ReadUInt64LEFromBytes(),
               stream.ReadUInt64LEFromBytes(),
               stream.ReadInt32LEFromBytes(),
               ( ddDirCount = stream.ReadUInt32LEFromBytes() ),
               stream.ReadSequentialElements( ddDirCount, s => s.ReadDataDirectory() )
               );
         default:
            throw new BadImageFormatException( "Invalid optional header kind: " + kind + "." );
      }
   }

   [CLSCompliant( false )]
   public static TRVA ReadRVAFromBytes( this StreamHelper stream )
   {
      return stream.ReadUInt32LEFromBytes();
   }

   [CLSCompliant( false )]
   public static ArrayQuery<T> ReadSequentialElements<T>( this StreamHelper stream, UInt32 elementCount, Func<StreamHelper, T> singleElementReader )
   {
      var array = new T[elementCount];
      for ( var i = 0u; i < elementCount; ++i )
      {
         array[i] = singleElementReader( stream );
      }
      return CollectionsWithRoles.Implementation.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( array ).CQ;
   }

   public static DataDirectory ReadDataDirectory( this StreamHelper stream )
   {
      return new DataDirectory( stream.ReadRVAFromBytes(), stream.ReadUInt32LEFromBytes() );
   }

   public static SectionHeader NewSectionHeaderFromStream( this StreamHelper stream )
   {
      return new SectionHeader(
         CollectionsWithRoles.Implementation.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( stream.ReadAndCreateArray( 8 ) ).CQ,
         stream.ReadRVAFromBytes(),
         stream.ReadRVAFromBytes(),
         stream.ReadUInt32LEFromBytes(),
         stream.ReadUInt32LEFromBytes(),
         stream.ReadUInt32LEFromBytes(),
         stream.ReadUInt32LEFromBytes(),
         stream.ReadUInt16LEFromBytes(),
         stream.ReadUInt16LEFromBytes(),
         (SectionHeaderCharacteristics) stream.ReadInt32LEFromBytes()
         );
   }

   #endregion

   #region CIL-related

   public static CLIHeader NewCLIHeaderFromStream( this StreamHelper stream )
   {
      return new CLIHeader(
         stream.ReadUInt32LEFromBytes(),
         stream.ReadUInt16LEFromBytes(),
         stream.ReadUInt16LEFromBytes(),
         stream.ReadDataDirectory(),
         (ModuleFlags) stream.ReadInt32LEFromBytes(),
         TableIndex.FromOneBasedToken( stream.ReadInt32LEFromBytes() ),
         stream.ReadDataDirectory(),
         stream.ReadDataDirectory(),
         stream.ReadDataDirectory(),
         stream.ReadDataDirectory(),
         stream.ReadDataDirectory(),
         stream.ReadDataDirectory()
         );
   }

   public static MetaDataRoot NewMetaDataRootFromStream( this StreamHelper stream )
   {
      UInt32 strLen;
      UInt16 streamCount;
      return new MetaDataRoot(
         stream.ReadInt32LEFromBytes(),
         stream.ReadUInt16LEFromBytes(),
         stream.ReadUInt16LEFromBytes(),
         stream.ReadInt32LEFromBytes(),
         ( strLen = stream.ReadUInt32LEFromBytes() ),
         stream.ReadAndCreateArrayQuery( strLen ),
         (StorageFlags) stream.ReadByteFromBytes(),
         stream.ReadByteFromBytes(),
         ( streamCount = stream.ReadUInt16LEFromBytes() ),
         stream.ReadSequentialElements( streamCount, s => s.NewMetaDataStreamHeaderFromStream() )
         );
   }

   public static MetaDataStreamHeader NewMetaDataStreamHeaderFromStream( this StreamHelper stream )
   {
      return new MetaDataStreamHeader(
         stream.ReadUInt32LEFromBytes(),
         stream.ReadUInt32LEFromBytes(),
         stream.ReadMDStreamHeaderName( 32 )
         );
   }

   private static ArrayQuery<Byte> ReadAndCreateArrayQuery( this StreamHelper stream, UInt32 len )
   {
      return CollectionsWithRoles.Implementation.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( stream.ReadAndCreateArray( (Int32) len ) ).CQ;
   }

   private static ArrayQuery<Byte> ReadMDStreamHeaderName( this StreamHelper stream, Int32 maxLength )
   {
      var bytez = Enumerable
         .Range( 0, maxLength )
         .Select( i => stream.ReadByteFromBytes() )
         .TakeWhile( b => b != 0 )
         .ToArray();

      // Skip to next 4-byte boundary
      stream.

   }

   #endregion
}
