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

using CILAssemblyManipulator.Physical.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO
{
   /// <summary>
   /// This class contains options and other classes used to fine-grain the output of headers and other low-level data during serialization process.
   /// </summary>
   /// <seealso cref="WritingArguments.WritingOptions"/>
   public class WritingOptions
   {
      /// <summary>
      /// Creates a new instance of <see cref="WritingOptions"/>, with optional <see cref="WritingOptions_PE"/>, <see cref="WritingOptions_CLI"/>, and <see cref="WritingOptions_Debug"/> options.
      /// </summary>
      /// <param name="peOptions">The <c>PE</c>-related options. If <c>null</c>, a new instance of <see cref="WritingOptions_PE"/> will be created.</param>
      /// <param name="cliOptions">The <c>CLI</c>-related options. If <c>null</c>, a new instance of <see cref="WritingOptions_CLI"/> will be created.</param>
      /// <param name="debugOptions">The debug-related options. If <c>null</c>, a new instance of <see cref="WritingOptions_Debug"/> will be created.</param>
      public WritingOptions(
         WritingOptions_PE peOptions = null,
         WritingOptions_CLI cliOptions = null,
         WritingOptions_Debug debugOptions = null
         )
      {
         this.PEOptions = peOptions ?? new WritingOptions_PE();
         this.CLIOptions = cliOptions ?? new WritingOptions_CLI();
         this.DebugOptions = debugOptions ?? new WritingOptions_Debug();
      }

      /// <summary>
      /// Gets or sets the value indicating whether this image is executable. This will affect <see cref="OptionalHeader.DLLCharacteristics"/> flags for all images, and the name of the imported module in import directory for <c>PE32</c> images.
      /// </summary>
      /// <value>The value indicating whether this image is executable.</value>
      public Boolean IsExecutable { get; set; }

      /// <summary>
      /// Gets the <see cref="WritingOptions_PE"/> object. Will never be null.
      /// </summary>
      /// <value>The <see cref="WritingOptions_PE"/> object.</value>
      public WritingOptions_PE PEOptions { get; }

      /// <summary>
      /// Gets the <see cref="WritingOptions_CLI"/> object. Will never be null.
      /// </summary>
      /// <value>The <see cref="WritingOptions_CLI"/> object.</value>
      public WritingOptions_CLI CLIOptions { get; }

      /// <summary>
      /// Gets the <see cref="WritingOptions_Debug"/> object. Will never be null.
      /// </summary>
      /// <value>The <see cref="WritingOptions_Debug"/> object.</value>
      public WritingOptions_Debug DebugOptions { get; }
   }

   /// <summary>
   /// This class contains properties controlling various values of <see cref="OptionalHeader"/> and <see cref="FileHeader"/> in <see cref="M:E_CILPhysical.WriteMetaDataToStream(WriterFunctionality, System.IO.Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, Crypto.CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/> method.
   /// Additionally, the <see cref="ImportDirectoryName"/> and <see cref="ImportHintName"/> control import directory content, if the import directory is present.
   /// </summary>
   /// <seealso cref="WritingOptions"/>
   /// <seealso cref="WritingArguments.WritingOptions"/>
   public class WritingOptions_PE
   {
      /// <summary>
      /// Gets or sets the value for <see cref="FileHeader.Machine"/> property.
      /// </summary>
      /// <value>The value for <see cref="FileHeader.Machine"/> property.</value>
      /// <remarks>
      /// By default, the value of <see cref="ImageFileMachine.I386"/> will be used.
      /// </remarks>
      /// <seealso cref="FileHeader.Machine"/>
      /// <seealso cref="ImageFileMachine"/>
      public ImageFileMachine? Machine { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="FileHeader.TimeDateStamp"/> property.
      /// </summary>
      /// <value>The value for <see cref="FileHeader.TimeDateStamp"/> property.</value>
      /// <remarks>
      /// By default, the automatically calculated value will be used.
      /// </remarks>
      /// <seealso cref="FileHeader.TimeDateStamp"/>
      public Int32? Timestamp { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="FileHeader.Characteristics"/> property.
      /// </summary>
      /// <value>The value for <see cref="FileHeader.Characteristics"/> property.</value>
      /// <remarks>
      /// By default, the combination of <see cref="FileHeaderCharacteristics.ExecutableImage"/> and <see cref="FileHeaderCharacteristics.LargeAddressAware" /> values will be used.
      /// </remarks>
      /// <seealso cref="FileHeader.Characteristics"/>
      /// <seealso cref="FileHeaderCharacteristics"/>
      public FileHeaderCharacteristics? Characteristics { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.MajorLinkerVersion"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.MajorLinkerVersion"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x0B</c> will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.MajorLinkerVersion"/>
      public Byte? MajorLinkerVersion { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.MinorLinkerVersion"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.MinorLinkerVersion"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x00</c> will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.MinorLinkerVersion"/>
      public Byte? MinorLinkerVersion { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.ImageBase"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.ImageBase"/> property.</value>
      /// <remarks>
      /// By default, the automatically calculated value will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.ImageBase"/>
      public Int64? ImageBase { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.SectionAlignment"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.SectionAlignment"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x2000</c> will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.SectionAlignment"/>
      public Int32? SectionAlignment { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.FileAlignment"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.FileAlignment"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x200</c> will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.FileAlignment"/>
      public Int32? FileAlignment { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.MajorOSVersion"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.MajorOSVersion"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x04</c> will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.MajorOSVersion"/>
      public Int16? MajorOSVersion { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.MinorOSVersion"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.MinorOSVersion"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x00</c> will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.MinorOSVersion"/>
      public Int16? MinorOSVersion { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.MajorUserVersion"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.MajorUserVersion"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x00</c> will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.MajorUserVersion"/>
      public Int16? MajorUserVersion { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.MinorUserVersion"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.MinorUserVersion"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x00</c> will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.MinorUserVersion"/>
      public Int16? MinorUserVersion { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.MajorSubsystemVersion"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.MajorSubsystemVersion"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x04</c> will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.MajorSubsystemVersion"/>
      public Int16? MajorSubsystemVersion { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.MinorSubsystemVersion"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.MinorSubsystemVersion"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x00</c> will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.MinorSubsystemVersion"/>
      public Int16? MinorSubsystemVersion { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.Win32VersionValue"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.Win32VersionValue"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x00</c> will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.Win32VersionValue"/>
      public Int32? Win32VersionValue { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.Subsystem"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.Subsystem"/> property.</value>
      /// <remarks>
      /// By default, the value of <see cref="Subsystem.WindowsConsole"/> will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.Subsystem"/>
      /// <seealso cref="IO.Subsystem"/>
      public Subsystem? Subsystem { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.DLLCharacteristics"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.DLLCharacteristics"/> property.</value>
      /// <remarks>
      /// By default, the automatically calculated value will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.DLLCharacteristics"/>
      /// <seealso cref="DLLFlags"/>
      public DLLFlags? DLLCharacteristics { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.StackReserveSize"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.StackReserveSize"/> property.</value>
      /// <remarks>
      /// By default, the automatically calculated value will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.StackReserveSize"/>
      public Int64? StackReserveSize { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.StackCommitSize"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.StackCommitSize"/> property.</value>
      /// <remarks>
      /// By default, the automatically calculated value will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.StackCommitSize"/>
      public Int64? StackCommitSize { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.HeapReserveSize"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.HeapReserveSize"/> property.</value>
      /// <remarks>
      /// By default, the automatically calculated value will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.HeapReserveSize"/>
      public Int64? HeapReserveSize { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.HeapCommitSize"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.HeapCommitSize"/> property.</value>
      /// <remarks>
      /// By default, the automatically calculated value will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.HeapCommitSize"/>
      public Int64? HeapCommitSize { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="OptionalHeader.LoaderFlags"/> property.
      /// </summary>
      /// <value>The value for <see cref="OptionalHeader.LoaderFlags"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x00000000</c> will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.LoaderFlags"/>
      public Int32? LoaderFlags { get; set; }

      /// <summary>
      /// Gets or sets the number of elements for <see cref="OptionalHeader.DataDirectories"/> property.
      /// </summary>
      /// <value>The number of elements for <see cref="OptionalHeader.LoaderFlags"/> property.</value>
      /// <remarks>
      /// By default, the value of <see cref="DataDirectories.MaxValue"/> will be used.
      /// </remarks>
      /// <seealso cref="OptionalHeader.LoaderFlags"/>
      public Int32? NumberOfDataDirectories { get; set; }

      /// <summary>
      /// Gets or sets the textual name of the imported module for import directory, if present.
      /// </summary>
      /// <value>The textual name of the imported module for import directory, if present.</value>
      /// <remarks>
      /// By default, the value of <c>"mscoree.dll"</c> will be used.
      /// This value will not be used at all if the import directory is not present.
      /// </remarks>
      public String ImportDirectoryName { get; set; }

      /// <summary>
      /// Gets or sets the textual name of the imported function for import directory, if present.
      /// </summary>
      /// <value>The textual name of the imported function for import directory, if present.</value>
      /// <remarks>
      /// By default, the value of  <c>"_CorExeMain"</c> will be used when <see cref="WritingOptions.IsExecutable"/> is <c>true</c>, and <c>"_CorDllMain"</c> will be used when <see cref="WritingOptions.IsExecutable"/> is <c>false</c>.
      /// This value will not be used at all if the import directory is not present.
      /// </remarks>
      public String ImportHintName { get; set; }
   }

   /// <summary>
   /// This class contains properties controlling various values of <see cref="CLIHeader"/>, <see cref="MetaDataRoot"/>, and <see cref="MetaDataTableStreamHeader"/> in <see cref="M:E_CILPhysical.WriteMetaDataToStream(WriterFunctionality, System.IO.Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, Crypto.CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/> method.
   /// </summary>
   /// <seealso cref="WritingOptions"/>
   /// <seealso cref="WritingArguments.WritingOptions"/>
   public class WritingOptions_CLI
   {
      /// <summary>
      /// Creates a new instance of <see cref="WritingOptions_CLI"/>, with optional <see cref="WritingOptions_CLIHeader"/>, <see cref="WritingOptions_MetaDataRoot"/>, and <see cref="WritingOptions_TableStream"/> options.
      /// </summary>
      /// <param name="headerOptions">The options for <see cref="CLIHeader"/>. If <c>null</c>, a new instance of <see cref="WritingOptions_CLIHeader"/> will be created.</param>
      /// <param name="mdRootOptions">The options for <see cref="MetaDataRoot"/>. If <c>null</c>, a new instance of <see cref="WritingOptions_MetaDataRoot"/> will be created.</param>
      /// <param name="tableStreamOptions">The options for <see cref="MetaDataTableStreamHeader"/>. If <c>null</c>, a new instance of <see cref="WritingOptions_TableStream"/> will be created.</param>
      public WritingOptions_CLI(
         WritingOptions_CLIHeader headerOptions = null,
         WritingOptions_MetaDataRoot mdRootOptions = null,
         WritingOptions_TableStream tableStreamOptions = null
         )
      {
         this.HeaderOptions = headerOptions ?? new WritingOptions_CLIHeader();
         this.MDRootOptions = mdRootOptions ?? new WritingOptions_MetaDataRoot();
         this.TablesStreamOptions = tableStreamOptions ?? new WritingOptions_TableStream();
      }

      /// <summary>
      /// Gets the <see cref="WritingOptions_CLIHeader"/> object. Will never be null.
      /// </summary>
      /// <value>The <see cref="WritingOptions_CLIHeader"/> object.</value>
      public WritingOptions_CLIHeader HeaderOptions { get; }

      /// <summary>
      /// Gets the <see cref="WritingOptions_MetaDataRoot"/> object. Will never be null.
      /// </summary>
      /// <value>The <see cref="WritingOptions_MetaDataRoot"/> object.</value>
      public WritingOptions_MetaDataRoot MDRootOptions { get; }

      /// <summary>
      /// Gets the <see cref="WritingOptions_TableStream"/> object. Will never be null.
      /// </summary>
      /// <value>The <see cref="WritingOptions_TableStream"/> object.</value>
      public WritingOptions_TableStream TablesStreamOptions { get; }

   }

   /// <summary>
   /// This class contains properties controlling various values of <see cref="CLIHeader"/> in <see cref="M:E_CILPhysical.WriteMetaDataToStream(WriterFunctionality, System.IO.Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, Crypto.CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/> method.
   /// </summary>
   /// <seealso cref="WritingOptions_CLI"/>
   /// <seealso cref="WritingArguments.WritingOptions"/>
   public class WritingOptions_CLIHeader
   {
      /// <summary>
      /// Gets or sets the value for <see cref="CLIHeader.MajorRuntimeVersion"/> property.
      /// </summary>
      /// <value>The value for <see cref="CLIHeader.MajorRuntimeVersion"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x0002</c> will be used.
      /// </remarks>
      /// <seealso cref="CLIHeader.MajorRuntimeVersion"/>
      public Int16? MajorRuntimeVersion { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="CLIHeader.MinorRuntimeVersion"/> property.
      /// </summary>
      /// <value>The value for <see cref="CLIHeader.MinorRuntimeVersion"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x0005</c> will be used.
      /// </remarks>
      /// <seealso cref="CLIHeader.MinorRuntimeVersion"/>
      public Int16? MinorRuntimeVersion { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="CLIHeader.Flags"/> property.
      /// </summary>
      /// <value>The value for <see cref="CLIHeader.Flags"/> property.</value>
      /// <remarks>
      /// By default, the automatically calculated value will be used.
      /// </remarks>
      /// <seealso cref="CLIHeader.Flags"/>
      public ModuleFlags? ModuleFlags { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="CLIHeader.EntryPointToken"/> property, as <see cref="TableIndex"/> reference to managed code.
      /// </summary>
      /// <value>The value for <see cref="CLIHeader.EntryPointToken"/> property, as <see cref="TableIndex"/> reference to managed code.</value>
      /// <remarks>
      /// By default, the <c>null</c> value will be used.
      /// The <see cref="TableIndex.Table"/> property should be either <see cref="Tables.MethodDef"/> or <see cref="Tables.File"/>.
      /// If both <see cref="ManagedEntryPointToken"/> and <see cref="UnmanagedEntryPointToken"/> are set, then the <see cref="ManagedEntryPointToken"/> takes precedence.
      /// </remarks>
      /// <seealso cref="CLIHeader.EntryPointToken"/>
      public TableIndex? ManagedEntryPointToken { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="CLIHeader.EntryPointToken"/> property, as RVA to unmanaged code.
      /// </summary>
      /// <value>The value for <see cref="CLIHeader.EntryPointToken"/> property, as RVA to unmanaged code.</value>
      /// <remarks>
      /// By default, the <c>null</c> value will be used.
      /// If both <see cref="ManagedEntryPointToken"/> and <see cref="UnmanagedEntryPointToken"/> are set, then the <see cref="ManagedEntryPointToken"/> takes precedence.
      /// </remarks>
      /// <seealso cref="CLIHeader.EntryPointToken"/>
      public Int32? UnmanagedEntryPointToken { get; set; }
   }

   /// <summary>
   /// This class contains properties controlling various values of <see cref="MetaDataRoot"/> in <see cref="M:E_CILPhysical.WriteMetaDataToStream(WriterFunctionality, System.IO.Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, Crypto.CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/> method.
   /// </summary>
   /// <seealso cref="WritingOptions_CLI"/>
   /// <seealso cref="WritingArguments.WritingOptions"/>
   public class WritingOptions_MetaDataRoot
   {
      /// <summary>
      /// Gets or sets the value for <see cref="MetaDataRoot.Signature"/> property.
      /// </summary>
      /// <value>The value for <see cref="MetaDataRoot.Signature"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x424A5342</c> will be used.
      /// </remarks>
      /// <seealso cref="MetaDataRoot.Signature"/>
      public Int32? Signature { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="MetaDataRoot.MajorVersion"/> property.
      /// </summary>
      /// <value>The value for <see cref="MetaDataRoot.MajorVersion"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x0001</c> will be used.
      /// </remarks>
      /// <seealso cref="MetaDataRoot.MajorVersion"/>
      public Int16? MajorVersion { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="MetaDataRoot.MinorVersion"/> property.
      /// </summary>
      /// <value>The value for <see cref="MetaDataRoot.MinorVersion"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x0001</c> will be used.
      /// </remarks>
      /// <seealso cref="MetaDataRoot.MinorVersion"/>
      public Int16? MinorVersion { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="MetaDataRoot.Reserved"/> property.
      /// </summary>
      /// <value>The value for <see cref="MetaDataRoot.Reserved"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x00000000</c> will be used.
      /// </remarks>
      /// <seealso cref="MetaDataRoot.Reserved"/>
      public Int32? Reserved { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="MetaDataRoot.VersionString"/> property.
      /// </summary>
      /// <value>The value for <see cref="MetaDataRoot.VersionString"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>"v4.0.30319"</c> will be used.
      /// </remarks>
      /// <seealso cref="MetaDataRoot.VersionString"/>
      public String VersionString { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="MetaDataRoot.StorageFlags"/> property.
      /// </summary>
      /// <value>The value for <see cref="MetaDataRoot.StorageFlags"/> property.</value>
      /// <remarks>
      /// By default, the value of <see cref="IO.StorageFlags.Normal"/> is used.
      /// Note that setting this to something else than that will make module load fail on most environments.
      /// </remarks>
      /// <seealso cref="MetaDataRoot.StorageFlags"/>
      /// <seealso cref="IO.StorageFlags"/>
      public StorageFlags? StorageFlags { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="MetaDataRoot.Reserved2"/> property.
      /// </summary>
      /// <value>The value for <see cref="MetaDataRoot.Reserved2"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x00</c> will be used.
      /// </remarks>
      /// <seealso cref="MetaDataRoot.Reserved2"/>
      public Byte? Reserved2 { get; set; }
   }

   /// <summary>
   /// This class contains properties controlling various values of <see cref="MetaDataTableStreamHeader"/> in <see cref="M:E_CILPhysical.WriteMetaDataToStream(WriterFunctionality, System.IO.Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, Crypto.CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/> method.
   /// </summary>
   /// <seealso cref="WritingOptions_CLI"/>
   /// <seealso cref="WritingArguments.WritingOptions"/>
   public class WritingOptions_TableStream
   {
      /// <summary>
      /// Gets or sets the value for <see cref="MetaDataTableStreamHeader.Reserved"/> property.
      /// </summary>
      /// <value>The value for <see cref="MetaDataTableStreamHeader.Reserved"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x00000000</c> will be used.
      /// </remarks>
      /// <seealso cref="MetaDataTableStreamHeader.Reserved"/>
      public Int32? Reserved { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="MetaDataTableStreamHeader.MajorVersion"/> property.
      /// </summary>
      /// <value>The value for <see cref="MetaDataTableStreamHeader.MajorVersion"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x02</c> will be used.
      /// </remarks>
      /// <seealso cref="MetaDataTableStreamHeader.MajorVersion"/>
      public Byte? MajorVersion { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="MetaDataTableStreamHeader.MinorVersion"/> property.
      /// </summary>
      /// <value>The value for <see cref="MetaDataTableStreamHeader.MinorVersion"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x00</c> will be used.
      /// </remarks>
      /// <seealso cref="MetaDataTableStreamHeader.MinorVersion"/>
      public Byte? MinorVersion { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="MetaDataTableStreamHeader.Reserved2"/> property.
      /// </summary>
      /// <value>The value for <see cref="MetaDataTableStreamHeader.Reserved2"/> property.</value>
      /// <remarks>
      /// By default, the value of <c>0x01</c> will be used.
      /// </remarks>
      /// <seealso cref="MetaDataTableStreamHeader.Reserved2"/>
      public Byte? Reserved2 { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="MetaDataTableStreamHeader.ExtraData"/> property.
      /// </summary>
      /// <value>The value for <see cref="MetaDataTableStreamHeader.ExtraData"/> property.</value>
      /// <remarks>
      /// By default, the <c>null</c> value will be used.
      /// </remarks>
      /// <seealso cref="MetaDataTableStreamHeader.ExtraData"/>
      public Int32? ExtraData { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="MetaDataTableStreamHeader.PresentTablesBitVector"/> property.
      /// </summary>
      /// <value>The value for <see cref="MetaDataTableStreamHeader.PresentTablesBitVector"/> property.</value>
      /// <remarks>
      /// By default, the automatically calculated value will be used.
      /// </remarks>
      /// <seealso cref="MetaDataTableStreamHeader.PresentTablesBitVector"/>
      public Int64? PresentTablesBitVector { get; set; }

      /// <summary>
      /// Gets or sets the value for <see cref="MetaDataTableStreamHeader.SortedTablesBitVector"/> property.
      /// </summary>
      /// <value>The value for <see cref="MetaDataTableStreamHeader.SortedTablesBitVector"/> property.</value>
      /// <remarks>
      /// By default, the automatically calculated value will be used.
      /// </remarks>
      /// <seealso cref="MetaDataTableStreamHeader.SortedTablesBitVector"/>
      public Int64? SortedTablesBitVector { get; set; }

      // TODO ENC, HasDeleted
   }

   /// <summary>
   /// This class contains information about the debug directory of PE files.
   /// </summary>
   /// <seealso href="http://msdn.microsoft.com/en-us/library/windows/desktop/ms680307%28v=vs.85%29.aspx"/>
   public class WritingOptions_Debug
   {
      /// <summary>
      /// Gets or sets the characteristics field of the debug directory.
      /// </summary>
      /// <value>The characteristics field of the debug directory.</value>
      public Int32 Characteristics { get; set; }

      /// <summary>
      /// Gets or sets the timestamp field of the debug directory.
      /// </summary>
      /// <value>The timestamp field of the debug directory.</value>
      public Int32 Timestamp { get; set; }

      /// <summary>
      /// Gets or sets the major version of the debug directory.
      /// </summary>
      /// <value>The major version of the debug directory.</value>
      public Int16 MajorVersion { get; set; }

      /// <summary>
      /// Gets or sets the minor version of the debug directory.
      /// </summary>
      /// <value>The minor version of the debug directory.</value>
      public Int16 MinorVersion { get; set; }

      /// <summary>
      /// Gets or sets the type field of the debug directory.
      /// </summary>
      /// <value>The field of the debug directory.</value>
      /// <remarks>In most cases, this should be <c>CodeView</c> debug type (<c>2</c>).</remarks>
      public Int32 DebugType { get; set; }

      /// <summary>
      /// Gets or sets the binary data of the debug directory.
      /// </summary>
      /// <value>The binary data of the debug directory.</value>
      /// <remarks>The debug header will not be written, if this is <c>null</c>.</remarks>
      public Byte[] DebugData { get; set; }
   }

}

public static partial class E_CILPhysical
{
   /// <summary>
   /// This is extension method to help create a new <see cref="WritingOptions"/> with all of its contents from given <see cref="ImageInformation"/>.
   /// </summary>
   /// <param name="imageInformation">The <see cref="ImageInformation"/> to use when populating various properties of the writing option objects.</param>
   /// <returns>A new <see cref="WritingOptions"/> object with options set from this <see cref="ImageInformation"/>.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="ImageInformation"/> is <c>null</c>.</exception>
   /// <remarks>
   /// The following properties are not set:
   /// <list type="bullet">
   /// <item><description><see cref="WritingOptions_TableStream.PresentTablesBitVector"/>, and</description></item>
   /// <item><description><see cref="WritingOptions_TableStream.SortedTablesBitVector"/>.</description></item>
   /// </list>
   /// </remarks>
   public static WritingOptions CreateWritingOptions( this ImageInformation imageInformation )
   {
      var peInfo = imageInformation.PEInformation;
      var dosHeader = peInfo.DOSHeader;
      var ntHeader = peInfo.NTHeader;
      var fileHeader = ntHeader.FileHeader;
      var optionalHeader = ntHeader.OptionalHeader;
      var debugInfo = imageInformation.DebugInformation;
      var cliInfo = imageInformation.CLIInformation;
      var cliHeader = cliInfo.CLIHeader;
      var mdRoot = cliInfo.MetaDataRoot;
      var tableStream = cliInfo.TableStreamHeader;
      var cliOptions = new WritingOptions_CLIHeader()
      {
         MajorRuntimeVersion = (Int16) cliHeader.MajorRuntimeVersion,
         MinorRuntimeVersion = (Int16) cliHeader.MinorRuntimeVersion,
         ModuleFlags = cliHeader.Flags
      };
      var epInt = (Int32) cliHeader.EntryPointToken;

      if ( cliHeader.Flags.IsNativeEntryPoint() )
      {
         cliOptions.UnmanagedEntryPointToken = epInt;
      }
      else
      {
         cliOptions.ManagedEntryPointToken = TableIndex.FromOneBasedTokenNullable( (Int32) cliHeader.EntryPointToken );
      }

      return new WritingOptions(
         new WritingOptions_PE()
         {
            Characteristics = fileHeader.Characteristics,
            DLLCharacteristics = optionalHeader.DLLCharacteristics,
            FileAlignment = (Int32) optionalHeader.FileAlignment,
            HeapCommitSize = (Int64) optionalHeader.HeapCommitSize,
            HeapReserveSize = (Int64) optionalHeader.HeapReserveSize,
            ImageBase = (Int64) optionalHeader.ImageBase,
            //ImportDirectoryName = ...,
            //ImportHintName = ...,
            LoaderFlags = optionalHeader.LoaderFlags,
            Machine = fileHeader.Machine,
            MajorLinkerVersion = optionalHeader.MajorLinkerVersion,
            MinorLinkerVersion = optionalHeader.MinorLinkerVersion,
            MajorOSVersion = (Int16) optionalHeader.MajorOSVersion,
            MinorOSVersion = (Int16) optionalHeader.MinorOSVersion,
            MajorSubsystemVersion = (Int16) optionalHeader.MajorSubsystemVersion,
            MinorSubsystemVersion = (Int16) optionalHeader.MinorSubsystemVersion,
            MajorUserVersion = (Int16) optionalHeader.MajorUserVersion,
            MinorUserVersion = (Int16) optionalHeader.MinorUserVersion,
            NumberOfDataDirectories = (Int32) optionalHeader.NumberOfDataDirectories,
            SectionAlignment = (Int32) optionalHeader.SectionAlignment,
            StackCommitSize = (Int64) optionalHeader.StackCommitSize,
            StackReserveSize = (Int64) optionalHeader.StackReserveSize,
            Subsystem = optionalHeader.Subsystem,
            Timestamp = (Int32) fileHeader.TimeDateStamp,
            Win32VersionValue = (Int32) optionalHeader.Win32VersionValue
         },
         new WritingOptions_CLI(
            cliOptions,
            new WritingOptions_MetaDataRoot()
            {
               MajorVersion = (Int16) mdRoot.MajorVersion,
               MinorVersion = (Int16) mdRoot.MinorVersion,
               Reserved = mdRoot.Reserved,
               Reserved2 = mdRoot.Reserved2,
               Signature = mdRoot.Signature,
               StorageFlags = mdRoot.StorageFlags,
               VersionString = mdRoot.VersionString
            },
            new WritingOptions_TableStream()
            {
               ExtraData = tableStream.ExtraData,
               MajorVersion = tableStream.MajorVersion,
               MinorVersion = tableStream.MinorVersion,
               Reserved = tableStream.Reserved,
               Reserved2 = tableStream.Reserved2
            }
         ),
         debugInfo == null ? null : new WritingOptions_Debug()
         {
            Characteristics = debugInfo.Characteristics,
            DebugData = debugInfo.DebugData.ToArray(),
            DebugType = debugInfo.DebugType,
            MajorVersion = (Int16) debugInfo.VersionMajor,
            MinorVersion = (Int16) debugInfo.VersionMinor,
            Timestamp = (Int32) debugInfo.Timestamp
         } )
      {
         IsExecutable = !fileHeader.Characteristics.IsDLL()
      };
   }

}
