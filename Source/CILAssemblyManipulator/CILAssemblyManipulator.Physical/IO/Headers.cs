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
         DebugInformation debugInfo,
         CLIInformation cliInfo
         )
      {
         ArgumentValidator.ValidateNotNull( "PE information", peInfo );
         ArgumentValidator.ValidateNotNull( "CLI information", cliInfo );

         this.PEInformation = peInfo;
         this.DebugInformation = debugInfo;
         this.CLIInformation = cliInfo;
      }

      public PEInformation PEInformation { get; }

      public DebugInformation DebugInformation { get; }

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
         0u, // base of data
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
      Reserved,
      MaxValue
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

   /// <summary>
   /// This enumerable contains values for possible target platforms when emitting <see cref="CILMetaData"/>.
   /// </summary>
   /// <remarks>This enumeration has same values as <c>System.Reflection.ImageFileMachine</c> enumeration, and more. It will end up as 'Machine' field in <see cref="IO.FileHeader"/>.</remarks>
   public enum ImageFileMachine : short
   {
      Unknown = 0,
      /// <summary>
      /// Targets Intel 32-bit processor.
      /// </summary>
      I386 = 0x014C,
      R3000 = 0x0162,
      R4000 = 0x0166,
      R10000 = 0x0168,
      WCE_MIPS_v2 = 0x0169,
      AlphaAXP = 0x0184,
      SH3 = 0x01A2,
      SH3DSP = 0x01A3,
      SH3E = 0x01A4,
      SH4 = 0x01A6,
      SH5 = 0x01A8,
      ARM = 0x01C0,
      ARMThumb = 0x01C2,
      /// <summary>
      /// Targets ARM processor.
      /// </summary>
      ARMv7 = 0x01C4,
      ARM_AM33 = 0x01D3,
      PowerPC = 0x01F0,
      PowerPC_FP = 0x01F1,
      /// <summary>
      /// Targets Intel 64-bit processor.
      /// </summary>
      IA64 = 0x0200,
      MIPS_16 = 0x0266,
      ALPHA64 = 0x0284,
      MIPS_FPU = 0x0366,
      MIPS_FPU_16 = 0x0466,
      Infineon_Tricore = 0x0520,
      Infineon_CEF = 0x0CEF,
      EBC = 0x0EBC,
      /// <summary>
      /// Targets AMD 64-bit processor.
      /// </summary>
      AMD64 = unchecked((Int16) 0x8664),
      M32R = unchecked((Int16) 0x9041),
      ARM_64 = unchecked((Int16) 0xAA64),
      CEE = unchecked((Int16) 0xC0EE),

   }

   public enum Subsystem : short
   {
      Native = 0x0001,
      WindowsGUI = 0x0002,
      WindowsConsole = 0x0003,
      OS2Console = 0x0005,
      PosixConsole = 0x0007,
      NativeWin9XDriver = 0x0008,
      WinCE = 0x0009,
      EFIApplication = 0x000A,
      EFIBootDriver = 0x000B,
      EFIRuntimeDriver = 0x000C,
      EFIROM = 0x000D,
      XBox = 0x000E,
      WindowsBootApplication = 0x0010
   }

   [Flags]
   public enum DLLFlags : short
   {
      Reserved1 = 0x0001,
      Reserved2 = 0x0002,
      Reserved3 = 0x0004,
      Reserved4 = 0x0008,
      Reserved5 = 0x0010,
      HighEntropyVA = 0x0020,
      DynamicBase = 0x0040,
      ForceIntegroty = 0x0080,
      NXCompatible = 0x0100,
      NoIsolation = 0x0200,
      NoSEH = 0x0400,
      NoBind = 0x0800,
      AppContainer = 0x1000,
      WdmDriver = 0x2000,
      GuardControlFlow = 0x4000,
      TerminalServerAware = unchecked((Int16) 0x8000),
   }

   public sealed class DebugInformation
   {
      [CLSCompliant( false )]
      public DebugInformation(
         Int32 characteristics,
         UInt32 timestamp,
         UInt16 versionMajor,
         UInt16 versionMinor,
         Int32 debugType,
         UInt32 dataSize,
         UInt32 dataRVA,
         UInt32 dataPointer,
         ArrayQuery<Byte> data
         )
      {
         this.Characteristics = characteristics;
         this.Timestamp = timestamp;
         this.VersionMajor = versionMajor;
         this.VersionMinor = versionMinor;
         this.DebugType = debugType;
         this.DataSize = dataSize;
         this.DataRVA = dataRVA;
         this.DataPointer = dataPointer;
         this.DebugData = data ?? EmptyArrayProxy<Byte>.Query;
      }

      public Int32 Characteristics { get; }

      [CLSCompliant( false )]
      public UInt32 Timestamp { get; }

      [CLSCompliant( false )]
      public UInt16 VersionMajor { get; }

      [CLSCompliant( false )]
      public UInt16 VersionMinor { get; }

      public Int32 DebugType { get; }

      [CLSCompliant( false )]
      public UInt32 DataSize { get; }

      [CLSCompliant( false )]
      public UInt32 DataRVA { get; }

      [CLSCompliant( false )]
      public UInt32 DataPointer { get; }

      public ArrayQuery<Byte> DebugData { get; }
   }

   #endregion

   #region CIL-related

   public sealed class CLIInformation
   {
      [CLSCompliant( false )]
      public CLIInformation(
         CLIHeader cliHeader,
         MetaDataRoot mdRoot,
         MetaDataTableStreamHeader tableStreamHeader,
         ArrayQuery<Byte> strongNameSignature,
         ArrayQuery<TRVA> methodRVAs,
         ArrayQuery<TRVA> fieldRVAs
         )
      {
         ArgumentValidator.ValidateNotNull( "CLI header", cliHeader );
         ArgumentValidator.ValidateNotNull( "MetaData root", mdRoot );
         ArgumentValidator.ValidateNotNull( "Table stream header", tableStreamHeader );
         ArgumentValidator.ValidateNotNull( "Method RVAs", methodRVAs );
         ArgumentValidator.ValidateNotNull( "Field RVAs", fieldRVAs );

         this.CLIHeader = cliHeader;
         this.MetaDataRoot = mdRoot;
         this.TableStreamHeader = tableStreamHeader;
         this.StrongNameSignature = strongNameSignature;
         this.MethodRVAs = methodRVAs;
         this.FieldRVAs = fieldRVAs;
      }

      public CLIHeader CLIHeader { get; }

      public MetaDataRoot MetaDataRoot { get; }

      public MetaDataTableStreamHeader TableStreamHeader { get; }

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

      //public static Encoding VersionStringEncoding
      //{
      //   get
      //   {
      //      return VERSION_ENCODING;
      //   }
      //}

      public static Int32 GetVersionStringByteCount( String versionString )
      {
         ArgumentValidator.ValidateNotNull( "Version string", versionString );
         return ( VERSION_ENCODING.GetByteCount( versionString ) + 1 ).RoundUpI32( 4 );
      }

      public static ArrayQuery<Byte> GetVersionStringBytes( String versionString )
      {
         if ( String.IsNullOrEmpty( versionString ) )
         {
            versionString = "v4.0.30319";
         }
         var bytez = new Byte[GetVersionStringByteCount( versionString )];
         VERSION_ENCODING.GetBytes( versionString, 0, versionString.Length, bytez, 0 );
         return bytez.ToArrayProxy().CQ;
      }

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

   public sealed class MetaDataStreamHeader
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

   public sealed class MetaDataTableStreamHeader
   {
      [CLSCompliant( false )]
      public MetaDataTableStreamHeader(
         Int32 reserved,
         Byte majorVersion,
         Byte minorVersion,
         TableStreamFlags tableStreamFlags,
         Byte reserved2,
         UInt64 presentTablesBitVector,
         UInt64 sortedTablesBitVector,
         ArrayQuery<UInt32> tableSizes,
         Int32? extraData
         )
      {
         ArgumentValidator.ValidateNotNull( "Table sizes", tableSizes );

         this.Reserved = reserved;
         this.MajorVersion = majorVersion;
         this.MinorVersion = minorVersion;
         this.TableStreamFlags = tableStreamFlags;
         this.Reserved2 = reserved2;
         this.PresentTablesBitVector = presentTablesBitVector;
         this.SortedTablesBitVector = sortedTablesBitVector;
         this.TableSizes = tableSizes;
         this.ExtraData = extraData;
      }

      public Int32 Reserved { get; }

      public Byte MajorVersion { get; }

      public Byte MinorVersion { get; }

      public TableStreamFlags TableStreamFlags { get; }

      public Byte Reserved2 { get; }

      [CLSCompliant( false )]
      public UInt64 PresentTablesBitVector { get; }

      [CLSCompliant( false )]
      public UInt64 SortedTablesBitVector { get; }

      [CLSCompliant( false )]
      public ArrayQuery<UInt32> TableSizes { get; }

      public Int32? ExtraData { get; }
   }

   [Flags]
   public enum TableStreamFlags : byte
   {
      WideStrings = 0x01,
      WideGUID = 0x02,
      WideBLOB = 0x04,
      Padding = 0x08,
      Reserved = 0x10,
      DeltaOnly = 0x20,
      ExtraData = 0x40,
      HasDelete = 0x80,
   }

   /// <summary>
   /// This enumeration contains values for what kind of code is contained within the module when emitting <see cref="CILMetaData"/>.
   /// </summary>
   /// <remarks>
   /// This enumeration partly overlaps <c>System.ReflectionPortableExecutableKinds</c> in its purpose.
   /// The value will end up as 'Flags' field in CLI header.
   /// </remarks>
   [Flags]
   public enum ModuleFlags : int
   {
      /// <summary>
      /// The module contains IL code only.
      /// </summary>
      ILOnly = 0x00000001,
      /// <summary>
      /// The module will load into 32-bit process only (the 64-bit processes won't be able to load it).
      /// </summary>
      Required32Bit = 0x00000002,
      /// <summary>
      /// Obsolete flag.
      /// </summary>
      [Obsolete( "This flag should no longer be used." )]
      ILLibrary = 0x00000004,
      /// <summary>
      /// This module is signed with the strong name.
      /// </summary>
      StrongNameSigned = 0x00000008,
      /// <summary>
      /// This module's entry point is an unmanaged method.
      /// </summary>
      NativeEntrypoint = 0x00000010,
      /// <summary>
      /// Some flag related to debugging modules in WinDbg, maybe?
      /// </summary>
      TrackDebugData = 0x00010000,
      /// <summary>
      /// If this module is not a class library, the process it starts will run as 32-bit process even in 64-bit OS.
      /// </summary>
      /// <remarks>
      /// <para>
      /// Taken from <see href="http://stackoverflow.com/questions/12066638/what-is-the-purpose-of-the-prefer-32-bit-setting-in-visual-studio-2012-and-how"/>, Lex Li's answer:
      /// <list type="bullet">
      /// <item><description>If the process runs on a 32-bit Windows system, it runs as a 32-bit process. IL is compiled to x86 machine code.</description></item>
      /// <item><description>If the process runs on a 64-bit Windows system, it runs as a 32-bit process. IL is compiled to x86 machine code.</description></item>
      /// <item><description>If the process runs on an ARM Windows system, it runs as a 32-bit process. IL is compiled to ARM machine code.</description></item>
      /// </list>
      /// </para>
      /// <para>
      /// Please note that if this flag is specified when emitting a <see cref="CILMetaData"/>, the flag <see cref="Required32Bit"/> should be set as well.
      /// </para>
      /// </remarks>
      Preferred32Bit = 0x00020000
   }

   [Flags]
   public enum MethodHeaderFlags
   {
      TinyFormat = 0x2,
      FatFormat = 0x3,
      MoreSections = 0x8,
      InitLocals = 0x10
   }

   [Flags]
   public enum MethodDataFlags
   {
      ExceptionHandling = 0x1,
      OptimizeILTable = 0x2,
      FatFormat = 0x40,
      MoreSections = 0x80
   }

   #endregion
}

public static partial class E_CILPhysical
{

   public static StreamHelper GoToRVA( this StreamHelper stream, RVAConverter rvaConverter, Int64 rva )
   {
      stream.Stream.SeekFromBegin( rvaConverter.ToOffset( rva ) );
      return stream;
   }

   #region PE-related

   public static PEInformation NewPEImageInformationFromStream( this StreamHelper stream )
   {
      // Read DOS header
      var dosHeader = stream.NewDOSHeaderFromStream();

      // Read NT header
      stream.Stream.SeekFromBegin( dosHeader.NTHeaderOffset );
      var ntHeader = stream.NewNTHeaderFromStream();

      // Read section headers
      var sections = stream.ReadSequentialElements( ntHeader.FileHeader.NumberOfSections, s => s.NewSectionHeaderFromStream() );
      return new PEInformation(
         dosHeader,
         ntHeader,
         sections
         );
   }

   public static void WritePEinformation( this PEInformation peInfo, ResizableArray<Byte> array )
   {
      var bytez = array.Array;
      var idx = 0;

      // DOS header
      peInfo.DOSHeader.WriteDOSHeader( bytez, ref idx );

      // NT Header
      peInfo.NTHeader.WriteNTHeader( bytez, ref idx );

      // Sections
      foreach ( var section in peInfo.SectionHeaders )
      {
         section.WriteSectionHeader( bytez, ref idx );
      }
   }

   public static DOSHeader NewDOSHeaderFromStream( this StreamHelper stream )
   {
      return new DOSHeader(
         stream.ReadInt16LEFromBytes(),
         stream.Skip( 0x3A ).ReadUInt32LEFromBytes()
         );
   }

   public static void WriteDOSHeader( this DOSHeader header, Byte[] array, ref Int32 idx )
   {
      array
         .WriteInt16LEToBytes( ref idx, header.Signature )
         .BlockCopyFrom( ref idx, new Byte[]
         {
            0x90, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00,
            0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
         } )
         .WriteUInt32LEToBytes( ref idx, header.NTHeaderOffset )
         .BlockCopyFrom( ref idx, new Byte[]
         {
            0x0E, 0x1F, 0xBA, 0x0E, 0x00, 0xB4, 0x09, 0xCD, 0x21, 0xB8, 0x01, 0x4C, 0xCD, 0x21, 0x54, 0x68,
            0x69, 0x73, 0x20, 0x70, 0x72, 0x6F, 0x67, 0x72, 0x61, 0x6D, 0x20, 0x63, 0x61, 0x6E, 0x6E, 0x6F, // is program canno
            0x74, 0x20, 0x62, 0x65, 0x20, 0x72, 0x75, 0x6E, 0x20, 0x69, 0x6E, 0x20, 0x44, 0x4F, 0x53, 0x20, // t be run in DOS 
            0x6D, 0x6F, 0x64, 0x65, 0x2E, 0x0D, 0x0D, 0x0A, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mode....$.......
         } );
   }

   public static NTHeader NewNTHeaderFromStream( this StreamHelper stream )
   {
      return new NTHeader(
         stream.ReadInt32LEFromBytes(),
         stream.NewFileHeaderFromStream(),
         stream.NewOptionalHeaderFromStream()
         );
   }

   public static void WriteNTHeader( this NTHeader header, Byte[] array, ref Int32 idx )
   {
      array.WriteInt32LEToBytes( ref idx, header.Signature );
      header.FileHeader.WriteFileHeader( array, ref idx );
      header.OptionalHeader.WriteOptionalHeader( array, ref idx );
   }

   public static FileHeader NewFileHeaderFromStream( this StreamHelper stream )
   {
      return new FileHeader(
         (ImageFileMachine) stream.ReadInt16LEFromBytes(), // Machine
         stream.ReadUInt16LEFromBytes(), // Number of sections
         stream.ReadUInt32LEFromBytes(), // Timestamp
         stream.ReadUInt32LEFromBytes(), // Pointer to symbol table
         stream.ReadUInt32LEFromBytes(), // Number of symbols
         stream.ReadUInt16LEFromBytes(), // Optional header size
         (FileHeaderCharacteristics) stream.ReadInt16LEFromBytes() // Characteristics
         );
   }

   public static void WriteFileHeader( this FileHeader header, Byte[] array, ref Int32 idx )
   {
      array
         .WriteInt16LEToBytes( ref idx, (Int16) header.Machine )
         .WriteUInt16LEToBytes( ref idx, header.NumberOfSections )
         .WriteUInt32LEToBytes( ref idx, header.TimeDateStamp )
         .WriteUInt32LEToBytes( ref idx, header.PointerToSymbolTable )
         .WriteUInt32LEToBytes( ref idx, header.NumberOfSymbols )
         .WriteUInt16LEToBytes( ref idx, header.OptionalHeaderSize )
         .WriteInt16LEToBytes( ref idx, (Int16) header.Characteristics );
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

   public static void WriteOptionalHeader( this OptionalHeader header, Byte[] array, ref Int32 idx )
   {
      var peKind = header.OptionalHeaderKind;
      Boolean isPE32;
      switch ( peKind )
      {
         case OptionalHeaderKind.Optional32:
            isPE32 = true;
            break;
         case OptionalHeaderKind.Optional64:
            isPE32 = false;
            break;
         default:
            throw new NotSupportedException( "Optional header kind not supported: " + peKind + "." );
      }

      array
         .WriteInt16LEToBytes( ref idx, (Int16) peKind )
         .WriteByteToBytes( ref idx, header.MajorLinkerVersion )
         .WriteByteToBytes( ref idx, header.MinorLinkerVersion )
         .WriteUInt32LEToBytes( ref idx, header.SizeOfCode )
         .WriteUInt32LEToBytes( ref idx, header.SizeOfInitializedData )
         .WriteUInt32LEToBytes( ref idx, header.SizeOfUninitializedData )
         .WriteUInt32LEToBytes( ref idx, header.EntryPointRVA )
         .WriteUInt32LEToBytes( ref idx, header.BaseOfCodeRVA );
      if ( isPE32 )
      {
         array.WriteUInt32LEToBytes( ref idx, header.BaseOfDataRVA );
      }
      ( isPE32 ? array.WriteUInt32LEToBytes( ref idx, (UInt32) header.ImageBase ) : array.WriteUInt64LEToBytes( ref idx, header.ImageBase ) )
         .WriteUInt32LEToBytes( ref idx, header.SectionAlignment )
         .WriteUInt32LEToBytes( ref idx, header.FileAlignment )
         .WriteUInt16LEToBytes( ref idx, header.MajorOSVersion )
         .WriteUInt16LEToBytes( ref idx, header.MinorOSVersion )
         .WriteUInt16LEToBytes( ref idx, header.MajorUserVersion )
         .WriteUInt16LEToBytes( ref idx, header.MinorUserVersion )
         .WriteUInt16LEToBytes( ref idx, header.MajorSubsystemVersion )
         .WriteUInt16LEToBytes( ref idx, header.MinorSubsystemVersion )
         .WriteUInt32LEToBytes( ref idx, header.Win32VersionValue )
         .WriteUInt32LEToBytes( ref idx, header.ImageSize )
         .WriteUInt32LEToBytes( ref idx, header.HeaderSize )
         .WriteUInt32LEToBytes( ref idx, header.FileChecksum )
         .WriteInt16LEToBytes( ref idx, (Int16) header.Subsystem )
         .WriteInt16LEToBytes( ref idx, (Int16) header.DLLCharacteristics );
      if ( isPE32 )
      {
         array
            .WriteUInt32LEToBytes( ref idx, (UInt32) header.StackReserveSize )
            .WriteUInt32LEToBytes( ref idx, (UInt32) header.StackCommitSize )
            .WriteUInt32LEToBytes( ref idx, (UInt32) header.HeapReserveSize )
            .WriteUInt32LEToBytes( ref idx, (UInt32) header.HeapCommitSize );
      }
      else
      {
         array
            .WriteUInt64LEToBytes( ref idx, header.StackReserveSize )
            .WriteUInt64LEToBytes( ref idx, header.StackCommitSize )
            .WriteUInt64LEToBytes( ref idx, header.HeapReserveSize )
            .WriteUInt64LEToBytes( ref idx, header.HeapCommitSize );
      }
      array
         .WriteInt32LEToBytes( ref idx, header.LoaderFlags )
         .WriteUInt32LEToBytes( ref idx, header.NumberOfDataDirectories );
      foreach ( var datadir in header.DataDirectories )
      {
         datadir.WriteDataDirectory( array, ref idx );
      }
   }

   public static void WriteDataDirectory( this DataDirectory dataDir, Byte[] array, ref Int32 idx )
   {
      array
         .WriteUInt32LEToBytes( ref idx, dataDir.RVA )
         .WriteUInt32LEToBytes( ref idx, dataDir.Size );
   }

   private static Byte[] WriteDataDirectory( this Byte[] array, ref Int32 idx, DataDirectory dataDir )
   {
      return array
         .WriteUInt32LEToBytes( ref idx, dataDir.RVA )
         .WriteUInt32LEToBytes( ref idx, dataDir.Size );
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

   public static void WriteSectionHeader( this SectionHeader section, Byte[] array, ref Int32 idx )
   {
      array
         .WriteBytesEnumerable( ref idx, section.NameBytes )
         .WriteUInt32LEToBytes( ref idx, section.VirtualSize )
         .WriteUInt32LEToBytes( ref idx, section.VirtualAddress )
         .WriteUInt32LEToBytes( ref idx, section.RawDataSize )
         .WriteUInt32LEToBytes( ref idx, section.RawDataPointer )
         .WriteUInt32LEToBytes( ref idx, section.RelocationsPointer )
         .WriteUInt32LEToBytes( ref idx, section.LineNumbersPointer )
         .WriteUInt16LEToBytes( ref idx, section.NumberOfRelocations )
         .WriteUInt16LEToBytes( ref idx, section.NumberOfLineNumbers )
         .WriteInt32LEToBytes( ref idx, (Int32) section.Characteristics );
   }

   /// <summary>
   /// Checks whether this target platform requires PE64 header.
   /// </summary>
   /// <param name="machine">The <see cref="ImageFileMachine"/>.</param>
   /// <returns><c>true</c> if <paramref name="machine"/> represents a target platform requiring PE64 header; <c>false</c> otherwise.</returns>
   public static Boolean RequiresPE64( this ImageFileMachine machine )
   {
      switch ( machine )
      {
         case ImageFileMachine.AMD64:
         case ImageFileMachine.IA64:
         case ImageFileMachine.ARM_64:
            return true;
         default:
            return false;
      }
   }

   public static FileHeaderCharacteristics GetDefaultCharacteristics( this ImageFileMachine machine )
   {
      return FileHeaderCharacteristics.ExecutableImage | ( machine.RequiresPE64() ? FileHeaderCharacteristics.ExecutableImage : FileHeaderCharacteristics.Machine32Bit );
   }

   public static Boolean IsDLL( this FileHeaderCharacteristics characteristics )
   {
      return ( characteristics & FileHeaderCharacteristics.Dll ) != 0;
   }

   ///// <summary>
   ///// Checks whether emitted module requires relocation section.
   ///// </summary>
   ///// <param name="machine">The <see cref="ImageFileMachine"/>.</param>
   ///// <returns><c>true</c> if <paramref name="machine"/> represents a target platform which requires relocation section in emitted file; <c>false</c> otherwise.</returns>
   //public static Boolean RequiresRelocations( this ImageFileMachine machine )
   //{
   //   return ImageFileMachine.I386 == machine;
   //}

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

   public static void WriteCLIHeader( this CLIHeader header, Byte[] array, ref Int32 idx )
   {
      array
         .WriteUInt32LEToBytes( ref idx, header.HeaderSize )
         .WriteUInt16LEToBytes( ref idx, header.MajorRuntimeVersion )
         .WriteUInt16LEToBytes( ref idx, header.MinorRuntimeVersion )
         .WriteDataDirectory( ref idx, header.MetaData )
         .WriteInt32LEToBytes( ref idx, (Int32) header.Flags )
         .WriteInt32LEToBytes( ref idx, header.EntryPointToken.GetOneBasedToken() )
         .WriteDataDirectory( ref idx, header.Resources )
         .WriteDataDirectory( ref idx, header.StrongNameSignature )
         .WriteDataDirectory( ref idx, header.CodeManagerTable )
         .WriteDataDirectory( ref idx, header.VTableFixups )
         .WriteDataDirectory( ref idx, header.ExportAddressTableJumps )
         .WriteDataDirectory( ref idx, header.ManagedNativeHeader );
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

   public static Int32 WriteMetaDataRoot( this MetaDataRoot header, ResizableArray<Byte> array )
   {
      var bytez = array.Array;
      var idx = 0;
      bytez
         .WriteInt32LEToBytes( ref idx, header.Signature )
         .WriteUInt16LEToBytes( ref idx, header.MajorVersion )
         .WriteUInt16LEToBytes( ref idx, header.MinorVersion )
         .WriteInt32LEToBytes( ref idx, header.Reserved )
         .WriteUInt32LEToBytes( ref idx, header.VersionStringLength )
         .BlockCopyFrom( ref idx, header.VersionStringBytes.ToArray() )
         .WriteByteToBytes( ref idx, (Byte) header.StorageFlags )
         .WriteByteToBytes( ref idx, header.Reserved2 )
         .WriteUInt16LEToBytes( ref idx, header.NumberOfStreams );
      foreach ( var hdr in header.StreamHeaders )
      {
         bytez
            .WriteUInt32LEToBytes( ref idx, hdr.Offset )
            .WriteUInt32LEToBytes( ref idx, hdr.Size )
            .WriteBytesEnumerable( ref idx, hdr.NameBytes );
      }

      return idx;
   }

   public static MetaDataTableStreamHeader NewTableStreamHeaderFromStream( this StreamHelper stream )
   {
      UInt64 presentTables;
      TableStreamFlags thFlags;
      return new MetaDataTableStreamHeader(
         stream.ReadInt32LEFromBytes(),
         stream.ReadByteFromBytes(),
         stream.ReadByteFromBytes(),
         ( thFlags = (TableStreamFlags) stream.ReadByteFromBytes() ),
         stream.ReadByteFromBytes(),
         ( presentTables = stream.ReadUInt64LEFromBytes() ),
         stream.ReadUInt64LEFromBytes(),
         stream.ReadSequentialElements( (UInt32) BinaryUtils.CountBitsSetU64( presentTables ), s => s.ReadUInt32LEFromBytes() ),
         thFlags.HasExtraData() ? stream.ReadInt32LEFromBytes() : (Int32?) null
         );
   }

   public static DebugInformation NewDebugInformationFromStream( this StreamHelper stream, PEInformation peInfo, RVAConverter rvaConverter )
   {
      var dataDirs = peInfo.NTHeader.OptionalHeader.DataDirectories;
      DataDirectory debugDD;
      var debugDDIdx = (Int32) DataDirectories.Debug;
      return dataDirs.Count > debugDDIdx
         && ( debugDD = dataDirs[debugDDIdx] ).RVA > 0 ?
         stream
            .At( rvaConverter.ToOffset( debugDD.RVA ) )
            .NewDebugInformationFromStream() :
         null;
   }

   public static DebugInformation NewDebugInformationFromStream( this StreamHelper stream )
   {
      UInt32 dataSize, dataPtr;
      return new DebugInformation(
         stream.ReadInt32LEFromBytes(), // Characteristics
         stream.ReadUInt32LEFromBytes(), // Timestamp
         stream.ReadUInt16LEFromBytes(), // Major version
         stream.ReadUInt16LEFromBytes(), // Minor version
         stream.ReadInt32LEFromBytes(), // Debug type
         ( dataSize = stream.ReadUInt32LEFromBytes() ),
         stream.ReadUInt32LEFromBytes(),
         ( dataPtr = stream.ReadUInt32LEFromBytes() ),
         stream.At( dataPtr ).ReadAndCreateArray( (Int32) dataSize ).ToArrayProxy().CQ
         );
   }

   public static void WriteDebugInformation( this DebugInformation debugInfo, Byte[] array, ref Int32 idx )
   {
      array
         .WriteInt32LEToBytes( ref idx, debugInfo.Characteristics )
         .WriteUInt32LEToBytes( ref idx, debugInfo.Timestamp )
         .WriteUInt16LEToBytes( ref idx, debugInfo.VersionMajor )
         .WriteUInt16LEToBytes( ref idx, debugInfo.VersionMinor )
         .WriteInt32LEToBytes( ref idx, debugInfo.DebugType )
         .WriteUInt32LEToBytes( ref idx, debugInfo.DataSize )
         .WriteUInt32LEToBytes( ref idx, debugInfo.DataRVA )
         .WriteUInt32LEToBytes( ref idx, debugInfo.DataPointer )
         .WriteBytesEnumerable( ref idx, debugInfo.DebugData );
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
      stream.SkipToNextAlignment( 4 );

      return CollectionsWithRoles.Implementation.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( bytez ).CQ;

   }


   /// <summary>
   /// Checks whether given <see cref="ModuleFlags"/> has its <see cref="ModuleFlags.ILOnly"/> flag set.
   /// </summary>
   /// <param name="mFlags">The <see cref="ModuleFlags"/>.</param>
   /// <returns><c>true</c> if <paramref name="mFlags"/> has <see cref="ModuleFlags.ILOnly"/> flag set; <c>false</c> otherwise.</returns>
   public static Boolean IsILOnly( this ModuleFlags mFlags )
   {
      return ( mFlags & ModuleFlags.ILOnly ) != 0;
   }

   public static Boolean IsWideStrings( this TableStreamFlags flags )
   {
      return ( flags & TableStreamFlags.WideStrings ) != 0;
   }

   public static Boolean IsWideGUID( this TableStreamFlags flags )
   {
      return ( flags & TableStreamFlags.WideGUID ) != 0;
   }

   public static Boolean IsWideBLOB( this TableStreamFlags flags )
   {
      return ( flags & TableStreamFlags.WideBLOB ) != 0;
   }

   public static Boolean HasPadding( this TableStreamFlags flags )
   {
      return ( flags & TableStreamFlags.Padding ) != 0;
   }

   public static Boolean IsDeltaOnly( this TableStreamFlags flags )
   {
      return ( flags & TableStreamFlags.DeltaOnly ) != 0;
   }

   public static Boolean HasExtraData( this TableStreamFlags flags )
   {
      return ( flags & TableStreamFlags.ExtraData ) != 0;
   }

   public static Boolean HasDelete( this TableStreamFlags flags )
   {
      return ( flags & TableStreamFlags.HasDelete ) != 0;
   }

   #endregion

   private static Byte[] WriteBytesEnumerable( this Byte[] bytez, ref Int32 idx, IEnumerable<Byte> enumerable )
   {
      foreach ( var b in enumerable )
      {
         bytez[idx++] = b;
      }
      return bytez;
   }
}
