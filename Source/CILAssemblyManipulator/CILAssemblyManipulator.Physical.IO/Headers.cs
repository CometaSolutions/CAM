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
   /// <summary>
   /// This class is the root class to access various information about headers and data references in binary (de)serialization.
   /// </summary>
   public sealed class ImageInformation
   {
      /// <summary>
      /// Creates a new instance of <see cref="ImageInformation"/>, with given <see cref="IO.PEInformation"/>, <see cref="IO.DebugInformation"/>, and <see cref="IO.CLIInformation"/>.
      /// </summary>
      /// <param name="peInfo">The <see cref="IO.PEInformation"/>.</param>
      /// <param name="debugInfo">The <see cref="IO.DebugInformation"/>.</param>
      /// <param name="cliInfo">The <see cref="IO.CLIInformation"/>.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="peInfo"/> or <paramref name="cliInfo"/> is <c>null</c>.</exception>
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

      /// <summary>
      /// Gets the <see cref="IO.PEInformation"/> of this <see cref="ImageInformation"/>.
      /// </summary>
      /// <value>The <see cref="IO.PEInformation"/> of this <see cref="ImageInformation"/>.</value>
      /// <remarks>
      /// This value is never <c>null</c>.
      /// </remarks>
      /// <seealso cref="IO.PEInformation"/>
      public PEInformation PEInformation { get; }

      /// <summary>
      /// Gets the <see cref="IO.DebugInformation"/> of this <see cref="ImageInformation"/>.
      /// </summary>
      /// <value>The <see cref="IO.DebugInformation"/> of this <see cref="ImageInformation"/>.</value>
      /// <remarks>
      /// This value may be <c>null</c>.
      /// </remarks>
      /// <seealso cref="IO.DebugInformation"/>
      public DebugInformation DebugInformation { get; }

      /// <summary>
      /// Gets the <see cref="IO.CLIInformation"/> of this <see cref="ImageInformation"/>.
      /// </summary>
      /// <value>The <see cref="IO.CLIInformation"/> of this <see cref="ImageInformation"/>.</value>
      /// <remarks>
      /// This value is never <c>null</c>.
      /// </remarks>
      /// <seealso cref="IO.CLIInformation"/>
      public CLIInformation CLIInformation { get; }
   }

   #region PE-related

   /// <summary>
   /// This class contains information related to various PE (Portable Executable) headers and values.
   /// </summary>
   /// <seealso cref="ImageInformation"/>
   /// <seealso cref="ImageInformation.PEInformation"/>
   /// <seealso cref="E_CILPhysical.ReadPEInformation"/>
   /// <seealso cref="E_CILPhysical.WritePEinformation"/>
   public sealed class PEInformation
   {
      /// <summary>
      /// Creates a new instance of <see cref="PEInformation"/> with given <see cref="IO.DOSHeader"/>, <see cref="IO.NTHeader"/>, and <see cref="SectionHeader"/>s.
      /// </summary>
      /// <param name="dosHeader">The <see cref="IO.DOSHeader"/>.</param>
      /// <param name="ntHeader">The <see cref="IO.NTHeader"/>.</param>
      /// <param name="sectionHeaders">The <see cref="SectionHeader"/>s.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="dosHeader"/>, <paramref name="ntHeader"/>, or <paramref name="sectionHeaders"/> is <c>null</c>.</exception>
      /// <remarks>
      /// Note that empty array for <paramref name="sectionHeaders"/> is allowed.
      /// </remarks>
      /// <seealso cref="E_CILPhysical.ReadPEInformation"/>
      /// <seealso cref="E_CILPhysical.WritePEinformation"/>
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

      /// <summary>
      /// Gets the <see cref="IO.DOSHeader"/> of this <see cref="PEInformation"/>.
      /// </summary>
      /// <value>The <see cref="IO.DOSHeader"/> of this <see cref="PEInformation"/>.</value>
      /// <remarks>
      /// This value is never <c>null</c>.
      /// </remarks>
      /// <seealso cref="IO.DOSHeader"/>
      public DOSHeader DOSHeader { get; }

      /// <summary>
      /// Gets the <see cref="IO.NTHeader"/> of this <see cref="PEInformation"/>.
      /// </summary>
      /// <value>The <see cref="IO.NTHeader"/> of this <see cref="PEInformation"/>.</value>
      /// <remarks>
      /// This value is never <c>null</c>.
      /// </remarks>
      /// <seealso cref="IO.NTHeader"/>
      public NTHeader NTHeader { get; }

      /// <summary>
      /// Gets the <see cref="SectionHeader"/>s of this <see cref="PEInformation"/>.
      /// </summary>
      /// <value>The <see cref="SectionHeader"/>s of this <see cref="PEInformation"/>.</value>
      /// <remarks>
      /// This value is never <c>null</c>, but it may be empty.
      /// </remarks>
      /// <seealso cref="IO.SectionHeader"/>
      public ArrayQuery<SectionHeader> SectionHeaders { get; }
   }

   /// <summary>
   /// This class contains those fields of DOS header, which are useful in PE image handling.
   /// </summary>
   /// <seealso cref="E_CILPhysical.ReadDOSHeader"/>
   /// <seealso cref="E_CILPhysical.WriteDOSHeader"/>
   public sealed class DOSHeader
   {
      /// <summary>
      /// Creates a new instance of <see cref="DOSHeader"/> with given signature and offset for <see cref="NTHeader"/>.
      /// </summary>
      /// <param name="signature">The signature.</param>
      /// <param name="ntHeadersOffset">The offset where <see cref="NTHeader"/> may be read.</param>
      /// <seealso cref="E_CILPhysical.ReadDOSHeader"/>
      /// <seealso cref="E_CILPhysical.WriteDOSHeader"/>
      [CLSCompliant( false )]
      public DOSHeader( Int16 signature, UInt32 ntHeadersOffset )
      {
         this.Signature = signature;
         this.NTHeaderOffset = ntHeadersOffset;
      }

      /// <summary>
      /// Gets the signature of this <see cref="DOSHeader"/>.
      /// </summary>
      /// <value>The signature of this <see cref="DOSHeader"/>.</value>
      public Int16 Signature { get; }

      /// <summary>
      /// Gets the offset where the <see cref="NTHeader"/> is located.
      /// </summary>
      /// <value>The offset where the <see cref="NTHeader"/> is located.</value>
      /// <seealso cref="NTHeader"/>
      [CLSCompliant( false )]
      public UInt32 NTHeaderOffset { get; }
   }

   /// <summary>
   /// This class contains fields and values of the PE NT header and all the headers it contains.
   /// </summary>
   /// <seealso cref="E_CILPhysical.ReadNTHeader"/>
   /// <seealso cref="E_CILPhysical.WriteNTHeader"/>
   public sealed class NTHeader
   {
      /// <summary>
      /// Creates a new instance of <see cref="NTHeader"/> with given signature, <see cref="IO.FileHeader"/>, and <see cref="IO.OptionalHeader"/>.
      /// </summary>
      /// <param name="signature">The signature.</param>
      /// <param name="fileHeader">The <see cref="IO.FileHeader"/>.</param>
      /// <param name="optionalHeader">The <see cref="IO.OptionalHeader"/>.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="fileHeader"/> or <paramref name="optionalHeader"/> is <c>null</c>.</exception>
      /// <seealso cref="E_CILPhysical.ReadNTHeader"/>
      /// <seealso cref="E_CILPhysical.WriteNTHeader"/>
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

      /// <summary>
      /// Gets the signature of this <see cref="NTHeader"/>.
      /// </summary>
      /// <value>The signature of this <see cref="NTHeader"/>.</value>
      public Int32 Signature { get; }

      /// <summary>
      /// Gets the <see cref="IO.FileHeader"/> of this <see cref="NTHeader"/>.
      /// </summary>
      /// <value>The <see cref="IO.FileHeader"/> of this <see cref="NTHeader"/>.</value>
      /// <remarks>
      /// This value is never <c>null</c>.
      /// </remarks>
      /// <seealso cref="IO.FileHeader"/>
      public FileHeader FileHeader { get; }

      /// <summary>
      /// Gets the <see cref="IO.OptionalHeader"/> of this <see cref="NTHeader"/>.
      /// </summary>
      /// <value>The <see cref="IO.OptionalHeader"/> of this <see cref="NTHeader"/>.</value>
      /// <remarks>
      /// This value is never <c>null</c>.
      /// </remarks>
      /// <seealso cref="IO.OptionalHeader"/>
      public OptionalHeader OptionalHeader { get; }
   }

   /// <summary>
   /// This class contains fields and values of the PE file header.
   /// </summary>
   /// <seealso cref="E_CILPhysical.ReadFileHeader"/>
   /// <seealso cref="E_CILPhysical.WriteFileHeader"/>
   public sealed class FileHeader
   {
      /// <summary>
      /// Creates a new instance of <see cref="FileHeader"/> with given values.
      /// </summary>
      /// <param name="machine">The <see cref="ImageFileMachine"/>.</param>
      /// <param name="numberOfSections">The amount of <see cref="SectionHeader"/>s in <see cref="PEInformation.SectionHeaders"/>.</param>
      /// <param name="timeDateStamp">The timedate stamp as integer.</param>
      /// <param name="pointerToSymbolTable">The pointer to symbol table.</param>
      /// <param name="numberOfSymbols">The amount of symbols.</param>
      /// <param name="optionalHeaderSize">The size of <see cref="OptionalHeader"/>, in bytes.</param>
      /// <param name="characteristics">The <see cref="FileHeaderCharacteristics"/>.</param>
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

      /// <summary>
      /// Gets the <see cref="ImageFileMachine"/> of this <see cref="FileHeader"/>.
      /// </summary>
      /// <value>The <see cref="ImageFileMachine"/> of this <see cref="FileHeader"/>.</value>
      /// <seealso cref="ImageFileMachine"/>
      public ImageFileMachine Machine { get; }

      /// <summary>
      /// Gets the number of section headers.
      /// </summary>
      /// <value>The number of section headers.</value>
      /// <remarks>
      /// This should be the amount of section headers in <see cref="PEInformation.SectionHeaders"/>.
      /// </remarks>
      [CLSCompliant( false )]
      public UInt16 NumberOfSections { get; }

      /// <summary>
      /// Gets the PE time date stamp, as integer.
      /// </summary>
      /// <value>The PE time date stamp, as integer.</value>
      [CLSCompliant( false )]
      public UInt32 TimeDateStamp { get; }

      /// <summary>
      /// Gets the pointer to the symbol table.
      /// </summary>
      /// <value>The pointer to the symbol table.</value>
      [CLSCompliant( false )]
      public UInt32 PointerToSymbolTable { get; }

      /// <summary>
      /// Gets the number of the symbols in symbol table.
      /// </summary>
      /// <value>The number of the symbols in symbol table.</value>
      [CLSCompliant( false )]
      public UInt32 NumberOfSymbols { get; }

      /// <summary>
      /// Gets the size of the <see cref="OptionalHeader"/>, in bytes.
      /// </summary>
      /// <value>The size of the <see cref="OptionalHeader"/>, in bytes.</value>
      [CLSCompliant( false )]
      public UInt16 OptionalHeaderSize { get; }

      /// <summary>
      /// Gets the <see cref="FileHeaderCharacteristics"/> of this <see cref="FileHeader"/>.
      /// </summary>
      /// <value>The <see cref="FileHeaderCharacteristics"/> of this <see cref="FileHeader"/>.</value>
      /// <seealso cref="FileHeaderCharacteristics"/>
      public FileHeaderCharacteristics Characteristics { get; }
   }

   /// <summary>
   /// This class contains fields and values of the PE optional header.
   /// </summary>
   /// <remarks>
   /// This is abstract class. Use <see cref="OptionalHeader32"/> or <see cref="OptionalHeader64"/> for instantation.
   /// </remarks>
   /// <seealso cref="OptionalHeader32"/>
   /// <seealso cref="OptionalHeader64"/>
   /// <seealso cref="E_CILPhysical.ReadOptionalHeader"/>
   /// <seealso cref="E_CILPhysical.WriteOptionalHeader"/>
   public abstract class OptionalHeader
   {

      /// <summary>
      /// Initializes properties of this <see cref="OptionalHeader"/>.
      /// </summary>
      /// <param name="majorLinkerVersion">The value for <see cref="MajorLinkerVersion"/>.</param>
      /// <param name="minorLinkerVersion">The value for <see cref="MinorLinkerVersion"/>.</param>
      /// <param name="sizeOfCode">The value for <see cref="SizeOfCode"/>.</param>
      /// <param name="sizeOfInitializedData">The value for <see cref="SizeOfInitializedData"/>.</param>
      /// <param name="sizeOfUninitializedData">The value of <see cref="SizeOfUninitializedData"/>.</param>
      /// <param name="entryPointRVA">The value for <see cref="EntryPointRVA"/>.</param>
      /// <param name="baseOfCodeRVA">The value for <see cref="BaseOfCodeRVA"/>.</param>
      /// <param name="baseOfDataRVA">The value for <see cref="BaseOfDataRVA"/>.</param>
      /// <param name="imageBase">The value for <see cref="ImageBase"/>.</param>
      /// <param name="sectionAlignment">The value for <see cref="SectionAlignment"/>.</param>
      /// <param name="fileAlignment">The value for <see cref="FileAlignment"/>.</param>
      /// <param name="majorOSVersion">The value for <see cref="MajorOSVersion"/>.</param>
      /// <param name="minorOSVersion">The value for <see cref="MinorOSVersion"/>.</param>
      /// <param name="majorUserVersion">The value for <see cref="MajorUserVersion"/>.</param>
      /// <param name="minorUserVersion">The value for <see cref="MinorUserVersion"/>.</param>
      /// <param name="majorSubsystemVersion">The value for <see cref="MajorSubsystemVersion"/>.</param>
      /// <param name="minorSubsystemVersion">The value for <see cref="MinorSubsystemVersion"/>.</param>
      /// <param name="win32VersionValue">The value for <see cref="Win32VersionValue"/>.</param>
      /// <param name="imageSize">The value for <see cref="ImageSize"/>.</param>
      /// <param name="headerSize">The value for <see cref="HeaderSize"/>.</param>
      /// <param name="fileChecksum">The value for <see cref="FileChecksum"/>.</param>
      /// <param name="subsystem">The value for <see cref="Subsystem"/>.</param>
      /// <param name="dllCharacteristics">The value for <see cref="DLLCharacteristics"/>.</param>
      /// <param name="stackReserveSize">The value for <see cref="StackReserveSize"/>.</param>
      /// <param name="stackCommitSize">The value for <see cref="StackCommitSize"/>.</param>
      /// <param name="heapReserveSize">The value for <see cref="HeapReserveSize"/>.</param>
      /// <param name="heapCommitSize">The value for <see cref="HeapCommitSize"/>.</param>
      /// <param name="loaderFlags">The value for <see cref="LoaderFlags"/>.</param>
      /// <param name="numberOfDataDirectories">The value for <see cref="NumberOfDataDirectories"/>.</param>
      /// <param name="dataDirectories">The value for <see cref="DataDirectories"/></param>
      [CLSCompliant( false )]
      public OptionalHeader(
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
      /// <summary>
      /// Gets the <see cref="IO.OptionalHeaderKind"/> enumeration describing the kind of this optional header.
      /// </summary>
      /// <value>The <see cref="IO.OptionalHeaderKind"/> enumeration describing the kind of this optional header.</value>
      /// <seealso cref="IO.OptionalHeaderKind"/>
      /// <seealso cref="OptionalHeader32"/>
      /// <seealso cref="OptionalHeader64"/>
      public abstract OptionalHeaderKind OptionalHeaderKind { get; }

      /// <summary>
      /// Gets the linker major version.
      /// </summary>
      /// <value>The linker major version.</value>
      public Byte MajorLinkerVersion { get; }

      /// <summary>
      /// Gets the linker minor version.
      /// </summary>
      /// <value>The linker minor version.</value>
      public Byte MinorLinkerVersion { get; }

      /// <summary>
      /// Gets the size of code, in bytes.
      /// </summary>
      /// <value>The size of code, in bytes.</value>
      /// <remarks>
      /// Typically, this value is calculated based on <see cref="PEInformation.SectionHeaders"/>.
      /// </remarks>
      [CLSCompliant( false )]
      public UInt32 SizeOfCode { get; }

      /// <summary>
      /// Gets the size of initialized data, in bytes.
      /// </summary>
      /// <value>The size of initialized data, in bytes.</value>
      /// <remarks>
      /// Typically, this value is calculated based on <see cref="PEInformation.SectionHeaders"/>.
      /// </remarks>
      [CLSCompliant( false )]
      public UInt32 SizeOfInitializedData { get; }

      /// <summary>
      /// Gets the size of uninitialized data, in bytes.
      /// </summary>
      /// <value>The size of uninitialized data, in bytes.</value>
      /// <remarks>
      /// Typically, this value is calculated based on <see cref="PEInformation.SectionHeaders"/>.
      /// </remarks>
      [CLSCompliant( false )]
      public UInt32 SizeOfUninitializedData { get; }

      /// <summary>
      /// Gets the RVA of the entry point code.
      /// </summary>
      /// <value>The RVA of the entry point code.</value>
      [CLSCompliant( false )]
      public TRVA EntryPointRVA { get; }

      /// <summary>
      /// Gets the RVA where the first occurrence of code is.
      /// </summary>
      /// <value>Gets the RVA where the first occurrence of code is.</value>
      /// <remarks>
      /// Typically, this value is calculated based on <see cref="PEInformation.SectionHeaders"/>.
      /// </remarks>
      [CLSCompliant( false )]
      public TRVA BaseOfCodeRVA { get; }

      /// <summary>
      /// Gets the RVA where the first occurrence of data (initialized or uninitialized) is.
      /// </summary>
      /// <value>Gets the RVA where the first occurrence of data (initialized or uninitialized) is.</value>
      /// <remarks>
      /// Typically, this value is calculated based on <see cref="PEInformation.SectionHeaders"/>.
      /// </remarks>
      [CLSCompliant( false )]
      public TRVA BaseOfDataRVA { get; }

      // NT-Specific 

      /// <summary>
      /// Gets the image base address.
      /// </summary>
      /// <value>The image base address.</value>
      /// <remarks>
      /// This value is used in the entry point code, when the "_CorDllMain" or "_CorExeMain" is called.
      /// </remarks>
      [CLSCompliant( false )]
      public UInt64 ImageBase { get; }

      /// <summary>
      /// Gets the section alignment, in bytes.
      /// </summary>
      /// <value>The section alignment, in bytes.</value>
      [CLSCompliant( false )]
      public UInt32 SectionAlignment { get; }

      /// <summary>
      /// Gets the file alignment, in bytes.
      /// </summary>
      /// <value>The file alignment, in bytes.</value>
      [CLSCompliant( false )]
      public UInt32 FileAlignment { get; }

      /// <summary>
      /// Gets the OS major version.
      /// </summary>
      /// <value>The OS major version.</value>
      [CLSCompliant( false )]
      public UInt16 MajorOSVersion { get; }

      /// <summary>
      /// Gets the OS minor version.
      /// </summary>
      /// <value>The OS minor version.</value>
      [CLSCompliant( false )]
      public UInt16 MinorOSVersion { get; }

      /// <summary>
      /// Gets the user-defined major version.
      /// </summary>
      /// <value>The user-defined major version.</value>
      [CLSCompliant( false )]
      public UInt16 MajorUserVersion { get; }

      /// <summary>
      /// Gets the user-defined minor version.
      /// </summary>
      /// <value>The user-defined minor version.</value>
      [CLSCompliant( false )]
      public UInt16 MinorUserVersion { get; }

      /// <summary>
      /// Gets the subsystem major version.
      /// </summary>
      /// <value>The subsystem major version.</value>
      [CLSCompliant( false )]
      public UInt16 MajorSubsystemVersion { get; }

      /// <summary>
      /// Gets the subsystem minor version.
      /// </summary>
      /// <value>The subsystem minor version.</value>
      [CLSCompliant( false )]
      public UInt16 MinorSubsystemVersion { get; }

      /// <summary>
      /// Gets the Win32 version.
      /// </summary>
      /// <value>The Win32 version.</value>
      [CLSCompliant( false )]
      public UInt32 Win32VersionValue { get; }

      /// <summary>
      /// Gets the size of the image when it's been laid out in memory, in bytes.
      /// </summary>
      /// <value>The size of the image when it's been laid out in memory, in bytes.</value>
      /// <remarks>
      /// This value should be aligned with <see cref="SectionAlignment"/>.
      /// </remarks>
      [CLSCompliant( false )]
      public UInt32 ImageSize { get; }

      /// <summary>
      /// Gets the size of the various PE headers when the image has been laid out in memory, in bytes.
      /// </summary>
      /// <value>The size of the various PE headers when the image has been laid out in memory, in bytes.</value>
      /// <remarks>
      /// This value should be aligned with <see cref="SectionAlignment"/>.
      /// </remarks>
      [CLSCompliant( false )]
      public UInt32 HeaderSize { get; }

      /// <summary>
      /// Gets the checksum calculated for the file.
      /// </summary>
      /// <value>The checksum calculated for the file.</value>
      /// <remarks>
      /// This is one of the portions of the file not included when calculating strong name signature (<see cref="CLIInformation.StrongNameSignature"/>).
      /// </remarks>
      [CLSCompliant( false )]
      public UInt32 FileChecksum { get; }

      /// <summary>
      /// Gets the <see cref="IO.Subsystem"/> for this <see cref="OptionalHeader"/>.
      /// </summary><
      /// <value>The <see cref="IO.Subsystem"/> for this <see cref="OptionalHeader"/>.</value>
      /// <seealso cref="IO.Subsystem"/>
      public Subsystem Subsystem { get; }

      /// <summary>
      /// Gest the <see cref="DLLFlags"/> for this <see cref="OptionalHeader"/>.
      /// </summary>
      /// <value>The <see cref="DLLFlags"/> for this <see cref="OptionalHeader"/>.</value>
      /// <seealso cref="DLLFlags"/>
      public DLLFlags DLLCharacteristics { get; }

      /// <summary>
      /// Gets the stack reserve size.
      /// </summary>
      /// <value>The stack reserve size.</value>
      [CLSCompliant( false )]
      public UInt64 StackReserveSize { get; }

      /// <summary>
      /// Gets the stack commit size.
      /// </summary>
      /// <value>The stack commit size.</value>
      [CLSCompliant( false )]
      public UInt64 StackCommitSize { get; }

      /// <summary>
      /// Gets the heap reserve size.
      /// </summary>
      /// <value>The heap reserve size.</value>
      [CLSCompliant( false )]
      public UInt64 HeapReserveSize { get; }

      /// <summary>
      /// Gets the heap commit size.
      /// </summary>
      /// <value>The heap commit size.</value>
      [CLSCompliant( false )]
      public UInt64 HeapCommitSize { get; }

      /// <summary>
      /// Gets the flags for loader.
      /// </summary>
      /// <value>The flags for loader.</value>
      public Int32 LoaderFlags { get; }

      /// <summary>
      /// Gets the number of data directories in <see cref="DataDirectories"/> property.
      /// </summary>
      /// <value>The number of data directories in <see cref="DataDirectories"/> property.</value>
      /// <remarks>
      /// Even though this value *should* indicate the number of elements in <see cref="DataDirectories"/> property, there is no code to enforce or check this.
      /// </remarks>
      [CLSCompliant( false )]
      public UInt32 NumberOfDataDirectories { get; }

      /// <summary>
      /// Gets the data directories of this <see cref="OptionalHeader"/>.
      /// </summary>
      /// <value>The data directories of this <see cref="OptionalHeader"/>.</value>
      /// <remarks>
      /// See <see cref="IO.DataDirectories"/> enumeration interpret the <see cref="DataDirectory"/> at certain index.
      /// </remarks>
      /// <seealso cref="IO.DataDirectories"/>
      /// <seealso cref="DataDirectory"/>
      public ArrayQuery<DataDirectory> DataDirectories { get; }
   }

   /// <summary>
   /// This class represents optional header for 32-bit systems.
   /// </summary>
   public sealed class OptionalHeader32 : OptionalHeader
   {
      /// <summary>
      /// Creates a new instance of <see cref="OptionalHeader32"/> with given values.
      /// </summary>
      /// <param name="majorLinkerVersion">The value for <see cref="OptionalHeader.MajorLinkerVersion"/>.</param>
      /// <param name="minorLinkerVersion">The value for <see cref="OptionalHeader.MinorLinkerVersion"/>.</param>
      /// <param name="sizeOfCode">The value for <see cref="OptionalHeader.SizeOfCode"/>.</param>
      /// <param name="sizeOfInitializedData">The value for <see cref="OptionalHeader.SizeOfInitializedData"/>.</param>
      /// <param name="sizeOfUninitializedData">The value of <see cref="OptionalHeader.SizeOfUninitializedData"/>.</param>
      /// <param name="entryPointRVA">The value for <see cref="OptionalHeader.EntryPointRVA"/>.</param>
      /// <param name="baseOfCodeRVA">The value for <see cref="OptionalHeader.BaseOfCodeRVA"/>.</param>
      /// <param name="baseOfDataRVA">The value for <see cref="OptionalHeader.BaseOfDataRVA"/>.</param>
      /// <param name="imageBase">The value for <see cref="OptionalHeader.ImageBase"/>.</param>
      /// <param name="sectionAlignment">The value for <see cref="OptionalHeader.SectionAlignment"/>.</param>
      /// <param name="fileAlignment">The value for <see cref="OptionalHeader.FileAlignment"/>.</param>
      /// <param name="majorOSVersion">The value for <see cref="OptionalHeader.MajorOSVersion"/>.</param>
      /// <param name="minorOSVersion">The value for <see cref="OptionalHeader.MinorOSVersion"/>.</param>
      /// <param name="majorUserVersion">The value for <see cref="OptionalHeader.MajorUserVersion"/>.</param>
      /// <param name="minorUserVersion">The value for <see cref="OptionalHeader.MinorUserVersion"/>.</param>
      /// <param name="majorSubsystemVersion">The value for <see cref="OptionalHeader.MajorSubsystemVersion"/>.</param>
      /// <param name="minorSubsystemVersion">The value for <see cref="OptionalHeader.MinorSubsystemVersion"/>.</param>
      /// <param name="win32VersionValue">The value for <see cref="OptionalHeader.Win32VersionValue"/>.</param>
      /// <param name="imageSize">The value for <see cref="OptionalHeader.ImageSize"/>.</param>
      /// <param name="headerSize">The value for <see cref="OptionalHeader.HeaderSize"/>.</param>
      /// <param name="fileChecksum">The value for <see cref="OptionalHeader.FileChecksum"/>.</param>
      /// <param name="subsystem">The value for <see cref="OptionalHeader.Subsystem"/>.</param>
      /// <param name="dllCharacteristics">The value for <see cref="OptionalHeader.DLLCharacteristics"/>.</param>
      /// <param name="stackReserveSize">The value for <see cref="OptionalHeader.StackReserveSize"/>.</param>
      /// <param name="stackCommitSize">The value for <see cref="OptionalHeader.StackCommitSize"/>.</param>
      /// <param name="heapReserveSize">The value for <see cref="OptionalHeader.HeapReserveSize"/>.</param>
      /// <param name="heapCommitSize">The value for <see cref="OptionalHeader.HeapCommitSize"/>.</param>
      /// <param name="loaderFlags">The value for <see cref="OptionalHeader.LoaderFlags"/>.</param>
      /// <param name="numberOfDataDirectories">The value for <see cref="OptionalHeader.NumberOfDataDirectories"/>.</param>
      /// <param name="dataDirectories">The value for <see cref="OptionalHeader.DataDirectories"/></param>
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

      /// <summary>
      /// The value <see cref="OptionalHeaderKind.Optional32"/> is returned.
      /// </summary>
      /// <value>The value <see cref="OptionalHeaderKind.Optional32"/></value>
      /// <seealso cref="IO.OptionalHeaderKind"/>
      public override OptionalHeaderKind OptionalHeaderKind
      {
         get
         {
            return OptionalHeaderKind.Optional32;
         }
      }
   }

   /// <summary>
   /// This class represents optional header for 64-bit systems.
   /// </summary>
   public sealed class OptionalHeader64 : OptionalHeader
   {
      /// <summary>
      /// Creates a new instance of <see cref="OptionalHeader64"/> with given values.
      /// </summary>
      /// <param name="majorLinkerVersion">The value for <see cref="OptionalHeader.MajorLinkerVersion"/>.</param>
      /// <param name="minorLinkerVersion">The value for <see cref="OptionalHeader.MinorLinkerVersion"/>.</param>
      /// <param name="sizeOfCode">The value for <see cref="OptionalHeader.SizeOfCode"/>.</param>
      /// <param name="sizeOfInitializedData">The value for <see cref="OptionalHeader.SizeOfInitializedData"/>.</param>
      /// <param name="sizeOfUninitializedData">The value of <see cref="OptionalHeader.SizeOfUninitializedData"/>.</param>
      /// <param name="entryPointRVA">The value for <see cref="OptionalHeader.EntryPointRVA"/>.</param>
      /// <param name="baseOfCodeRVA">The value for <see cref="OptionalHeader.BaseOfCodeRVA"/>.</param>
      /// <param name="imageBase">The value for <see cref="OptionalHeader.ImageBase"/>.</param>
      /// <param name="sectionAlignment">The value for <see cref="OptionalHeader.SectionAlignment"/>.</param>
      /// <param name="fileAlignment">The value for <see cref="OptionalHeader.FileAlignment"/>.</param>
      /// <param name="majorOSVersion">The value for <see cref="OptionalHeader.MajorOSVersion"/>.</param>
      /// <param name="minorOSVersion">The value for <see cref="OptionalHeader.MinorOSVersion"/>.</param>
      /// <param name="majorUserVersion">The value for <see cref="OptionalHeader.MajorUserVersion"/>.</param>
      /// <param name="minorUserVersion">The value for <see cref="OptionalHeader.MinorUserVersion"/>.</param>
      /// <param name="majorSubsystemVersion">The value for <see cref="OptionalHeader.MajorSubsystemVersion"/>.</param>
      /// <param name="minorSubsystemVersion">The value for <see cref="OptionalHeader.MinorSubsystemVersion"/>.</param>
      /// <param name="win32VersionValue">The value for <see cref="OptionalHeader.Win32VersionValue"/>.</param>
      /// <param name="imageSize">The value for <see cref="OptionalHeader.ImageSize"/>.</param>
      /// <param name="headerSize">The value for <see cref="OptionalHeader.HeaderSize"/>.</param>
      /// <param name="fileChecksum">The value for <see cref="OptionalHeader.FileChecksum"/>.</param>
      /// <param name="subsystem">The value for <see cref="OptionalHeader.Subsystem"/>.</param>
      /// <param name="dllCharacteristics">The value for <see cref="OptionalHeader.DLLCharacteristics"/>.</param>
      /// <param name="stackReserveSize">The value for <see cref="OptionalHeader.StackReserveSize"/>.</param>
      /// <param name="stackCommitSize">The value for <see cref="OptionalHeader.StackCommitSize"/>.</param>
      /// <param name="heapReserveSize">The value for <see cref="OptionalHeader.HeapReserveSize"/>.</param>
      /// <param name="heapCommitSize">The value for <see cref="OptionalHeader.HeapCommitSize"/>.</param>
      /// <param name="loaderFlags">The value for <see cref="OptionalHeader.LoaderFlags"/>.</param>
      /// <param name="numberOfDataDirectories">The value for <see cref="OptionalHeader.NumberOfDataDirectories"/>.</param>
      /// <param name="dataDirectories">The value for <see cref="OptionalHeader.DataDirectories"/></param>
      /// <remarks>
      /// The <see cref="OptionalHeader.BaseOfDataRVA"/> will be <c>0</c>.
      /// </remarks>
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

      /// <summary>
      /// The value <see cref="OptionalHeaderKind.Optional64"/> is returned.
      /// </summary>
      /// <value>The value <see cref="OptionalHeaderKind.Optional64"/></value>
      /// <seealso cref="IO.OptionalHeaderKind"/>
      public override OptionalHeaderKind OptionalHeaderKind
      {
         get
         {
            return OptionalHeaderKind.Optional64;
         }
      }
   }

   /// <summary>
   /// This struct represents a single chunk within an image, starting at specific RVA and having a specific size.
   /// </summary>
   /// <seealso cref="E_CILPhysical.ReadDataDirectory"/>
   /// <seealso cref="E_CILPhysical.WriteDataDirectory(DataDirectory, byte[], ref int)"/>
   public struct DataDirectory : IEquatable<DataDirectory>
   {
      /// <summary>
      /// Creates a new instance of <see cref="DataDirectory"/> with given RVA and size, in bytes.
      /// </summary>
      /// <param name="rva">The RVA where the chunk starts.</param>
      /// <param name="size">The size of the chunk, in bytes.</param>
      [CLSCompliant( false )]
      public DataDirectory( TRVA rva, UInt32 size )
      {
         this.RVA = rva;
         this.Size = size;
      }

      /// <summary>
      /// Gets the RVA where this <see cref="DataDirectory"/> starts.
      /// </summary>
      /// <value>The RVA where this <see cref="DataDirectory"/> starts.</value>
      [CLSCompliant( false )]
      public TRVA RVA { get; }

      /// <summary>
      /// Gets the size of this <see cref="DataDirectory"/>, in bytes.
      /// </summary>
      /// <value>The size of this <see cref="DataDirectory"/>, in bytes.</value>
      [CLSCompliant( false )]
      public UInt32 Size { get; }

      /// <summary>
      /// Checks whether given object is <see cref="DataDirectory"/> and that it has the same values as this <see cref="DataDirectory"/>.
      /// </summary>
      /// <param name="obj">The object to check.</param>
      /// <returns><c>true</c> if <paramref name="obj"/> is <see cref="DataDirectory"/> and has same values as this <see cref="DataDirectory"/>; <c>false</c> otherwise.</returns>
      /// <seealso cref="Equals(DataDirectory)"/>
      public override Boolean Equals( Object obj )
      {
         return obj is DataDirectory && this.Equals( (DataDirectory) obj );
      }

      /// <summary>
      /// Computes the hash code for this <see cref="DataDirectory"/>.
      /// </summary>
      /// <returns>The hash code for this <see cref="DataDirectory"/>.</returns>
      public override Int32 GetHashCode()
      {
         return unchecked((Int32) ( ( 17 * 23 + this.RVA ) * 23 + this.Size ));
      }

      /// <summary>
      /// Creates the textual representation of this <see cref="DataDirectory"/>.
      /// </summary>
      /// <returns>The textual representation of this <see cref="DataDirectory"/>.</returns>
      public override String ToString()
      {
         return "[" + this.RVA + ";" + this.Size + "]";
      }

      /// <summary>
      /// Checks whether this <see cref="DataDirectory"/> has same values as given <see cref="DataDirectory"/>.
      /// </summary>
      /// <param name="other">The other <see cref="DataDirectory"/>.</param>
      /// <returns><c>true</c> if this <see cref="DataDirectory"/> has same values as <paramref name="other"/>; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// The following properties are checked for equality:
      /// <list type="bullet">
      /// <item><description><see cref="RVA"/>, and</description></item>
      /// <item><description><see cref="Size"/>.</description></item>
      /// </list>
      /// </remarks>
      public Boolean Equals( DataDirectory other )
      {
         return this.RVA == other.RVA && this.Size == other.Size;
      }

      /// <summary>
      /// Checks whether two <see cref="DataDirectory"/> instances are considered to be equal.
      /// </summary>
      /// <param name="x">The first <see cref="DataDirectory"/>.</param>
      /// <param name="y">The second <see cref="DataDirectory"/>.</param>
      /// <returns>The value of <see cref="Equals(DataDirectory)"/>.</returns>
      /// <seealso cref="Equals(DataDirectory)"/>
      public static Boolean operator ==( DataDirectory x, DataDirectory y )
      {
         return x.Equals( y );
      }

      /// <summary>
      /// Checks whether two <see cref="DataDirectory"/> instances are not considered to be equal.
      /// </summary>
      /// <param name="x">The first <see cref="DataDirectory"/>.</param>
      /// <param name="y">The second <see cref="DataDirectory"/>.</param>
      /// <returns>The inverted value of <see cref="Equals(DataDirectory)"/>.</returns>
      /// <seealso cref="Equals(DataDirectory)"/>
      public static Boolean operator !=( DataDirectory x, DataDirectory y )
      {
         return !( x == y );
      }
   }

   /// <summary>
   /// This enumeration tells the semantic meaning of the <see cref="DataDirectory"/> located at index represented by this enum, in <see cref="OptionalHeader.DataDirectories"/>.
   /// </summary>
   public enum DataDirectories
   {
      /// <summary>
      /// The target of the <see cref="DataDirectory"/> is a table for exported symbols.
      /// </summary>
      ExportTable,

      /// <summary>
      /// The target of the <see cref="DataDirectory"/> is a table for imported symbols.
      /// </summary>
      ImportTable,

      /// <summary>
      /// The target of the <see cref="DataDirectory"/> is a table for resources.
      /// </summary>
      ResourceTable,

      /// <summary>
      /// The target of the <see cref="DataDirectory"/> is a table for exceptions.
      /// </summary>
      ExceptionTable,

      /// <summary>
      /// The target of the <see cref="DataDirectory"/> is a table for certificate.
      /// </summary>
      CertificateTable,

      /// <summary>
      /// The target of the <see cref="DataDirectory"/> is base relocation table.
      /// </summary>
      BaseRelocationTable,

      /// <summary>
      /// The target of the <see cref="DataDirectory"/> is serialized <see cref="DebugInformation"/>.
      /// </summary>
      /// <seealso cref="DebugInformation"/>
      Debug,

      /// <summary>
      /// The target of the <see cref="DataDirectory"/> is serialized copyright information.
      /// </summary>
      Copyright,

      /// <summary>
      /// The target of the <see cref="DataDirectory"/> is globals table.
      /// </summary>
      Globals,

      /// <summary>
      /// The target of the <see cref="DataDirectory"/> is TLS table.
      /// </summary>
      TLSTable,

      /// <summary>
      /// The target of the <see cref="DataDirectory"/> is load configuration table.
      /// </summary>
      LoadConfigTable,

      /// <summary>
      /// The target of the <see cref="DataDirectory"/> is serialized bound imports information.
      /// </summary>
      BoundImport,

      /// <summary>
      /// The target of the <see cref="DataDirectory"/> is address table for imports.
      /// </summary>
      /// <seealso cref="Defaults.SectionPart_ImportAddressTable"/>
      ImportAddressTable,

      /// <summary>
      /// The target of the <see cref="DataDirectory"/> is descriptor for delayed imports.
      /// </summary>
      DelayImportDescriptor,

      /// <summary>
      /// The target of the <see cref="DataDirectory"/> is <see cref="IO.CLIHeader"/>.
      /// </summary>
      /// <seealso cref="IO.CLIHeader"/>
      CLIHeader,

      /// <summary>
      /// This value is reserved for future usage.
      /// </summary>
      Reserved,

      /// <summary>
      /// This value represents the maximum value for <see cref="DataDirectories"/>.
      /// </summary>
      MaxValue
   }

   /// <summary>
   /// This enumeration tells what type instance of <see cref="OptionalHeader"/> really is.
   /// </summary>
   public enum OptionalHeaderKind : short
   {
      /// <summary>
      /// The <see cref="OptionalHeader"/> is of type <see cref="OptionalHeader32"/>.
      /// </summary>
      Optional32 = 0x010B,

      /// <summary>
      /// The <see cref="OptionalHeader"/> is of type <see cref="OptionalHeader64"/>.
      /// </summary>
      Optional64 = 0x020B,

      // OptionalROM = 0x0107
   }

   /// <summary>
   /// This enumeration represents possible values for <see cref="FileHeader.Characteristics"/>.
   /// </summary>
   [Flags]
   public enum FileHeaderCharacteristics : short
   {
      /// <summary>
      /// TODO
      /// </summary>
      RelocsStripped = 0x0001,
      /// <summary>
      /// TODO
      /// </summary>
      ExecutableImage = 0x0002,
      /// <summary>
      /// TODO
      /// </summary>
      LineNumsStripped = 0x0004,
      /// <summary>
      /// TODO
      /// </summary>
      LocalSymsStripped = 0x0008,
      /// <summary>
      /// TODO
      /// </summary>
      AggressiveWSTrim = 0x0010,
      /// <summary>
      /// TODO
      /// </summary>
      LargeAddressAware = 0x0020,
      /// <summary>
      /// TODO
      /// </summary>
      Reserved1 = 0x0040,
      /// <summary>
      /// TODO
      /// </summary>
      BytesReversedLo = 0x0080,
      /// <summary>
      /// TODO
      /// </summary>
      Machine32Bit = 0x0100,
      /// <summary>
      /// TODO
      /// </summary>
      DebugStripped = 0x0200,
      /// <summary>
      /// TODO
      /// </summary>
      RemovableRunFromSwap = 0x0400,
      /// <summary>
      /// TODO
      /// </summary>
      NetRunFromSwap = 0x0800,
      /// <summary>
      /// TODO
      /// </summary>
      System = 0x1000,
      /// <summary>
      /// TODO
      /// </summary>
      Dll = 0x2000,
      /// <summary>
      /// TODO
      /// </summary>
      UPSystemOnly = 0x4000,
      /// <summary>
      /// TODO
      /// </summary>
      BytesReversedHi = unchecked((Int16) 0x8000),
   }

   /// <summary>
   /// This class contains fields and values of the PE section header.
   /// </summary>
   /// <seealso cref="PEInformation.SectionHeaders"/>
   /// <seealso cref="E_CILPhysical.ReadSectionHeader"/>
   /// <seealso cref="E_CILPhysical.WriteSectionHeader"/>
   public sealed class SectionHeader
   {
      private readonly Lazy<String> _name;

      /// <summary>
      /// Creates a new instance of <see cref="SectionHeader"/> with given values.
      /// </summary>
      /// <param name="nameBytes">The value for <see cref="NameBytes"/>.</param>
      /// <param name="virtualSize">The value for <see cref="VirtualSize"/>.</param>
      /// <param name="virtualAddress">The value for <see cref="VirtualAddress"/>.</param>
      /// <param name="rawDataSize">The value for <see cref="RawDataSize"/>.</param>
      /// <param name="rawDataPointer">The value for <see cref="RawDataPointer"/>.</param>
      /// <param name="relocationsPointer">The value for <see cref="RelocationsPointer"/>.</param>
      /// <param name="lineNumbersPointer">The value for <see cref="LineNumbersPointer"/>.</param>
      /// <param name="numberOfRelocations">The value for <see cref="NumberOfRelocations"/>.</param>
      /// <param name="numberOfLineNumbers">The value for <see cref="NumberOfLineNumbers"/>.</param>
      /// <param name="characteristics">The value for <see cref="Characteristics"/>.</param>
      /// <remarks>
      /// If <paramref name="nameBytes"/> is <c>null</c>, an empty array is used instead.
      /// </remarks>
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
         this.NameBytes = nameBytes ?? EmptyArrayProxy<Byte>.Query;
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

      /// <summary>
      /// Gets the bytes that constitute the name for this <see cref="SectionHeader"/>.
      /// </summary>
      /// <value>The bytes that constitute the name for this <see cref="SectionHeader"/>.</value>
      public ArrayQuery<Byte> NameBytes { get; }

      /// <summary>
      /// Gets the name for this <see cref="SectionHeader"/>, in textual format.
      /// </summary>
      /// <value>The name for this <see cref="SectionHeader"/>, in textual format.</value>
      /// <remarks>
      /// This will be lazily created from <see cref="NameBytes"/> property, interpreting bytes as zero-terminated ASCII string.
      /// </remarks>
      public String Name
      {
         get
         {
            return this._name.Value;
         }
      }

      /// <summary>
      /// Gets the size of this section when it has been laid out in memory.
      /// </summary>
      /// <value>The size of this section when it has been laid out in memory.</value>
      /// <remarks>
      /// Typically this is the size of the contents of the section, without the padding up to <see cref="OptionalHeader.FileAlignment"/>.
      /// </remarks>
      [CLSCompliant( false )]
      public TRVA VirtualSize { get; }

      /// <summary>
      /// Gets the address of the first byte in this section when it has been laid out in memory.
      /// </summary>
      /// <value>The address of the first byte in this section when it has been laid out in memory.</value>
      [CLSCompliant( false )]
      public TRVA VirtualAddress { get; }

      /// <summary>
      /// Gets the size of this section in image.
      /// </summary>
      /// <value>The size of this section in image.</value>
      [CLSCompliant( false )]
      public UInt32 RawDataSize { get; }

      /// <summary>
      /// Gets the pointer within image to the data of this section.
      /// </summary><
      /// <value>The pointer within image to the data of this section.</value>
      [CLSCompliant( false )]
      public UInt32 RawDataPointer { get; }

      /// <summary>
      /// Gets the pointer within image to the relocation data of this section.
      /// </summary>
      /// <value>The pointer within image to the relocation data of this section.</value>
      [CLSCompliant( false )]
      public UInt32 RelocationsPointer { get; }

      /// <summary>
      /// Gets the pointer wihin image to the line number data of this section.
      /// </summary>
      /// <value>The pointer wihin image to the line number data of this section.</value>
      [CLSCompliant( false )]
      public UInt32 LineNumbersPointer { get; }

      /// <summary>
      /// Gets the number of relocations in relocation data chunk of this section.
      /// </summary>
      /// <value>The number of relocations in relocation data chunk of this section.</value>
      [CLSCompliant( false )]
      public UInt16 NumberOfRelocations { get; }

      /// <summary>
      /// GEts the number of line numbers in line number data chunk of this section.
      /// </summary>
      /// <value>The number of line numbers in line number data chunk of this section.</value>
      [CLSCompliant( false )]
      public UInt16 NumberOfLineNumbers { get; }

      /// <summary>
      /// Gets the <see cref="SectionHeaderCharacteristics"/> of this <see cref="SectionHeader"/>, describing the data of this section.
      /// </summary>
      /// <value>The <see cref="SectionHeaderCharacteristics"/> of this <see cref="SectionHeader"/>, describing the data of this section.</value>
      /// <seealso cref="SectionHeaderCharacteristics"/>
      public SectionHeaderCharacteristics Characteristics { get; }
   }

   /// <summary>
   /// This enumeration describes information about data and layout of one section represented by <see cref="SectionHeader"/>.
   /// </summary>
   [Flags]
   public enum SectionHeaderCharacteristics : int
   {
      Reserved1 = 0x00000000,
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
      //Unknown = 0,

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

   /// <summary>
   /// This class represents debug data located at <see cref="DataDirectories.Debug"/> in <see cref="OptionalHeader.DataDirectories"/>.
   /// </summary>
   /// <seealso cref="ImageInformation.DebugInformation"/>
   /// <seealso cref="E_CILPhysical.ReadDebugInformation(StreamHelper)"/>
   /// <seealso cref="E_CILPhysical.WriteDebugInformation"/>
   public sealed class DebugInformation
   {
      /// <summary>
      /// Creates a new instance of <see cref="DebugInformation"/> with given values.
      /// </summary>
      /// <param name="characteristics">The value for <see cref="Characteristics"/>.</param>
      /// <param name="timestamp">The value for <see cref="Timestamp"/>.</param>
      /// <param name="versionMajor">The value for <see cref="VersionMajor"/>.</param>
      /// <param name="versionMinor">The value for <see cref="VersionMinor"/>.</param>
      /// <param name="debugType">The value for <see cref="DebugType"/>.</param>
      /// <param name="dataSize">The value for <see cref="DataSize"/>.</param>
      /// <param name="dataRVA">The value for <see cref="DataRVA"/>.</param>
      /// <param name="dataPointer">The value for <see cref="DataPointer"/>.</param>
      /// <param name="data">The value for <see cref="DebugData"/>.</param>
      /// <remarks>
      /// If <paramref name="data"/> is <c>null</c>, an empty array is used instead.
      /// </remarks>
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

      // TODO some of these properties would be good to be enums instead of integers

      /// <summary>
      /// Gets the characteristics of this <see cref="DebugInformation"/>.
      /// </summary>
      /// <value>The characteristics of this <see cref="DebugInformation"/>.</value>
      public Int32 Characteristics { get; }

      /// <summary>
      /// Gets the timestamp of this <see cref="DebugInformation"/> as integer.
      /// </summary>
      /// <value>The timestamp of this <see cref="DebugInformation"/> as integer.</value>
      [CLSCompliant( false )]
      public UInt32 Timestamp { get; }

      /// <summary>
      /// Gets the major version of this <see cref="DebugInformation"/>.
      /// </summary>
      /// <value>The major version of this <see cref="DebugInformation"/>.</value>
      [CLSCompliant( false )]
      public UInt16 VersionMajor { get; }

      /// <summary>
      /// Gets the minor version of this <see cref="DebugInformation"/>.
      /// </summary>
      /// <value>The minor version of this <see cref="DebugInformation"/>.</value>
      [CLSCompliant( false )]
      public UInt16 VersionMinor { get; }

      /// <summary>
      /// Gets the debug type of this <see cref="DebugInformation"/>.
      /// This will affect how the debug data is interpreted.
      /// </summary>
      /// <value>The debug type of this <see cref="DebugInformation"/>.</value>
      public Int32 DebugType { get; }

      /// <summary>
      /// Gets the size of the debug data.
      /// </summary>
      /// <value>The size of the debug data.</value>
      /// <seealso cref="DebugData"/>
      [CLSCompliant( false )]
      public UInt32 DataSize { get; }

      /// <summary>
      /// Gets the RVA where the debug data is located.
      /// </summary>
      /// <value>The RVA where the debug data is located.</value>
      /// <seealso cref="DebugData"/>
      [CLSCompliant( false )]
      public UInt32 DataRVA { get; }

      /// <summary>
      /// Gets the raw pointer in file where the debug data is located.
      /// </summary>
      /// <value>The raw pointer in file where the debug data is located.</value>
      /// <seealso cref="DebugData"/>
      [CLSCompliant( false )]
      public UInt32 DataPointer { get; }

      /// <summary>
      /// Gets the debug data of this <see cref="DebugInformation"/>.
      /// </summary>
      /// <value>The debug data of this <see cref="DebugInformation"/>.</value>
      public ArrayQuery<Byte> DebugData { get; }
   }

   #endregion

   #region CIL-related

   /// <summary>
   /// This class contains CLI-related information of <see cref="ImageInformation"/> obtained when (de)serializing <see cref="CILMetaData"/>.
   /// </summary>
   public sealed class CLIInformation
   {
      [CLSCompliant( false )]
      public CLIInformation(
         CLIHeader cliHeader,
         MetaDataRoot mdRoot,
         MetaDataTableStreamHeader tableStreamHeader,
         ArrayQuery<Byte> strongNameSignature,
         DataReferencesInfo dataRefs
         )
      {
         ArgumentValidator.ValidateNotNull( "CLI header", cliHeader );
         ArgumentValidator.ValidateNotNull( "MetaData root", mdRoot );
         ArgumentValidator.ValidateNotNull( "Table stream header", tableStreamHeader );
         ArgumentValidator.ValidateNotNull( "Data references", dataRefs );

         this.CLIHeader = cliHeader;
         this.MetaDataRoot = mdRoot;
         this.TableStreamHeader = tableStreamHeader;
         this.StrongNameSignature = strongNameSignature;
         this.DataReferences = dataRefs;
      }

      public CLIHeader CLIHeader { get; }

      public MetaDataRoot MetaDataRoot { get; }

      public MetaDataTableStreamHeader TableStreamHeader { get; }

      public ArrayQuery<Byte> StrongNameSignature { get; }

      public DataReferencesInfo DataReferences { get; }
   }

   public sealed class DataReferencesInfo
   {
      public DataReferencesInfo( DictionaryQuery<Tables, DictionaryQuery<Int32, ArrayQuery<Int64>>> dataRefs )
      {
         ArgumentValidator.ValidateNotNull( "Data references", dataRefs );

         this.DataReferences = dataRefs;
      }
      public DictionaryQuery<Tables, DictionaryQuery<Int32, ArrayQuery<Int64>>> DataReferences { get; }
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

         this._versionString = new Lazy<String>( () =>
            {
               var bArray = this.VersionStringBytes.TakeWhile( b => b != 0 ).ToArray();
               return VERSION_ENCODING.GetString( bArray, 0, bArray.Length );
            },
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

   public delegate T ReadElementFromArrayDelegate<T>( Byte[] array, ref Int32 idx );

   public static class CAMIOInternals
   {
      public const Int32 DATA_DIR_SIZE = 0x08;

      public static Byte[] WriteDataDirectory( this Byte[] array, ref Int32 idx, DataDirectory dataDir )
      {
         dataDir.WriteDataDirectory( array, ref idx );
         return array;
      }
   }
}

public static partial class E_CILPhysical
{

   public static StreamHelper GoToRVA( this StreamHelper stream, RVAConverter rvaConverter, Int64 rva )
   {
      stream.Stream.SeekFromBegin( rvaConverter.ToOffset( rva ) );
      return stream;
   }

   #region PE-related

   public static PEInformation ReadPEInformation( this StreamHelper stream )
   {
      // Read DOS header
      var dosHeader = stream.ReadDOSHeader();

      // Read NT header
      stream.Stream.SeekFromBegin( dosHeader.NTHeaderOffset );
      var ntHeader = stream.ReadNTHeader();

      // Read section headers
      var sections = stream.ReadSequentialElements( ntHeader.FileHeader.NumberOfSections, s => s.ReadSectionHeader() );
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
      idx = (Int32) peInfo.DOSHeader.NTHeaderOffset;
      peInfo.NTHeader.WriteNTHeader( bytez, ref idx );

      // Sections
      foreach ( var section in peInfo.SectionHeaders )
      {
         section.WriteSectionHeader( bytez, ref idx );
      }
   }

   public static DOSHeader ReadDOSHeader( this StreamHelper stream )
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

   public static NTHeader ReadNTHeader( this StreamHelper stream )
   {
      return new NTHeader(
         stream.ReadInt32LEFromBytes(),
         stream.ReadFileHeader(),
         stream.ReadOptionalHeader()
         );
   }

   public static void WriteNTHeader( this NTHeader header, Byte[] array, ref Int32 idx )
   {
      array.WriteInt32LEToBytes( ref idx, header.Signature );
      header.FileHeader.WriteFileHeader( array, ref idx );
      header.OptionalHeader.WriteOptionalHeader( array, ref idx );
   }

   public static FileHeader ReadFileHeader( this StreamHelper stream )
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

   public static OptionalHeader ReadOptionalHeader( this StreamHelper stream )
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
      dataDir.WriteDataDirectory( array, ref idx );
      return array;
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

   [CLSCompliant( false )]
   public static ArrayQuery<T> ReadSequentialElements<T>( this Byte[] array, ref Int32 idx, UInt32 elementCount, ReadElementFromArrayDelegate<T> singleElementReader )
   {
      var retVal = new T[elementCount];
      for ( var i = 0u; i < elementCount; ++i )
      {
         retVal[i] = singleElementReader( array, ref idx );
      }
      return CollectionsWithRoles.Implementation.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( retVal ).CQ;
   }

   public static DataDirectory ReadDataDirectory( this StreamHelper stream )
   {
      return new DataDirectory( stream.ReadRVAFromBytes(), stream.ReadUInt32LEFromBytes() );
   }

   public static SectionHeader ReadSectionHeader( this StreamHelper stream )
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
      return machine.GetOptionalHeaderKind() == OptionalHeaderKind.Optional64;
   }

   public static OptionalHeaderKind GetOptionalHeaderKind( this ImageFileMachine machine )
   {
      switch ( machine )
      {
         case ImageFileMachine.AMD64:
         case ImageFileMachine.IA64:
         case ImageFileMachine.ARM_64:
            return OptionalHeaderKind.Optional64;
         default:
            return OptionalHeaderKind.Optional32;
      }
   }

   public static FileHeaderCharacteristics GetDefaultCharacteristics( this ImageFileMachine machine )
   {
      return FileHeaderCharacteristics.ExecutableImage | FileHeaderCharacteristics.LargeAddressAware; //  ( machine.RequiresPE64() ? FileHeaderCharacteristics.ExecutableImage : FileHeaderCharacteristics.Machine32Bit );
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

   public static Int32 GetOptionalHeaderSize( this OptionalHeaderKind kind, Int32 peDataDirectoriesCount )
   {
      return ( ( kind == OptionalHeaderKind.Optional64 ? 0x70 : 0x60 ) + CAMIOInternals.DATA_DIR_SIZE * peDataDirectoriesCount );
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
         TableIndex.FromOneBasedTokenNullable( stream.ReadInt32LEFromBytes() ),
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

   public static MetaDataTableStreamHeader NewTableStreamHeaderFromStream( this Byte[] array, ref Int32 idx )
   {
      UInt64 presentTables;
      TableStreamFlags thFlags;
      return new MetaDataTableStreamHeader(
         array.ReadInt32LEFromBytes( ref idx ),
         array.ReadByteFromBytes( ref idx ),
         array.ReadByteFromBytes( ref idx ),
         ( thFlags = (TableStreamFlags) array.ReadByteFromBytes( ref idx ) ),
         array.ReadByteFromBytes( ref idx ),
         ( presentTables = array.ReadUInt64LEFromBytes( ref idx ) ),
         array.ReadUInt64LEFromBytes( ref idx ),
         array.ReadSequentialElements( ref idx, (UInt32) BinaryUtils.CountBitsSetU64( presentTables ), ( Byte[] a, ref Int32 i ) => a.ReadUInt32LEFromBytes( ref i ) ),
         thFlags.HasExtraData() ? array.ReadInt32LEFromBytes( ref idx ) : (Int32?) null
         );
   }

   public static DebugInformation ReadDebugInformation( this StreamHelper stream )
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
      stream.SkipToNextAlignmentInt32();

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

   public static ArrayQuery<Int64> GetMethodRVAs( this CLIInformation info )
   {
      return info.GetDataReferencesFor( Tables.MethodDef, 0 );
   }

   public static ArrayQuery<Int64> GetFieldRVAs( this CLIInformation info )
   {
      return info.GetDataReferencesFor( Tables.FieldRVA, 0 );
   }

   public static ArrayQuery<Int64> GetDataReferencesFor( this CLIInformation info, Tables table, Int32 colIndex )
   {
      DictionaryQuery<Int32, ArrayQuery<Int64>> dic;
      ArrayQuery<Int64> refs = null;
      if ( info.DataReferences.DataReferences.TryGetValue( table, out dic )
         && dic.TryGetValue( colIndex, out refs ) )
      {

      }
      return refs ?? EmptyArrayProxy<Int64>.Query;
   }

   public static DataReferencesInfo CreateDataReferencesDictionary( this MetaDataTableStreamHeader header, IEnumerable<DataReferenceInfo> refs )
   {
      var cf = CollectionsWithRoles.Implementation.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY;
      var dic = new Dictionary<Tables, DictionaryWithRoles<Int32, ArrayProxy<Int64>, ArrayProxyQuery<Int64>, ArrayQuery<Int64>>>();
      var tSizes = header.CreateTableSizesArray();
      var indices = new Int32[tSizes.Length];
      foreach ( var dataRef in refs )
      {
         var table = dataRef.Table;
         var thisTableDic = dic
            .GetOrAdd_NotThreadSafe( table, t => cf.NewDictionary<Int32, ArrayProxy<Int64>, ArrayProxyQuery<Int64>, ArrayQuery<Int64>>() );
         var cIdx = dataRef.ColumnIndex;
         ArrayProxy<Int64> array;
         if ( !thisTableDic.CQ.TryGetValue( cIdx, out array ) )
         {
            array = cf.NewArrayProxy<Int64>( new Int64[tSizes[(Int32) table]] );
            thisTableDic.Add( cIdx, array );
         }
         array[indices[(Int32) table]++] = dataRef.DataReference;
      }
      return new DataReferencesInfo( cf.NewDictionary<
         Tables,
         DictionaryWithRoles<Int32, ArrayProxy<Int64>, ArrayProxyQuery<Int64>, ArrayQuery<Int64>>,
         DictionaryQueryOfMutables<Int32, ArrayProxy<Int64>, ArrayProxyQuery<Int64>, ArrayQuery<Int64>>,
         DictionaryQuery<Int32, ArrayQuery<Int64>>
         >( dic ).CQ.IQ );
   }

   // Technically, max size is 255, but the bitmask in CLI header can only describe presence of 64 tables
   private const Int32 TABLE_ARRAY_SIZE = 64;

   public static Int32[] CreateTableSizesArray( this MetaDataTableStreamHeader tableStreamHeader )
   {
      var tableSizes = new Int32[TABLE_ARRAY_SIZE];
      var present = tableStreamHeader.PresentTablesBitVector;
      var sizeIdx = 0;
      for ( var i = 0; i < TABLE_ARRAY_SIZE; ++i )
      {
         if ( ( ( present >> i ) & 0x1 ) != 0 )
         {
            tableSizes[i] = (Int32) tableStreamHeader.TableSizes[sizeIdx++];
         }
      }

      return tableSizes;
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
