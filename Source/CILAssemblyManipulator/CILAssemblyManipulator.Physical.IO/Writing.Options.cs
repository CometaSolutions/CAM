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
   public class WritingOptions
   {
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

      public Boolean IsExecutable { get; set; }

      public WritingOptions_PE PEOptions { get; }

      public WritingOptions_CLI CLIOptions { get; }

      public WritingOptions_Debug DebugOptions { get; }
   }

   public class WritingOptions_PE
   {
      public ImageFileMachine? Machine { get; set; }

      public Int32? Timestamp { get; set; }

      public FileHeaderCharacteristics? Characteristics { get; set; }

      public Byte? MajorLinkerVersion { get; set; }

      public Byte? MinorLinkerVersion { get; set; }

      public Int64? ImageBase { get; set; }

      public Int32? SectionAlignment { get; set; }

      public Int32? FileAlignment { get; set; }

      public Int16? MajorOSVersion { get; set; }

      public Int16? MinorOSVersion { get; set; }

      public Int16? MajorUserVersion { get; set; }

      public Int16? MinorUserVersion { get; set; }

      public Int16? MajorSubsystemVersion { get; set; }

      public Int16? MinorSubsystemVersion { get; set; }

      public Int32? Win32VersionValue { get; set; }

      public Subsystem? Subsystem { get; set; }

      public DLLFlags? DLLCharacteristics { get; set; }

      public Int64? StackReserveSize { get; set; }

      public Int64? StackCommitSize { get; set; }

      public Int64? HeapReserveSize { get; set; }

      public Int64? HeapCommitSize { get; set; }

      public Int32? LoaderFlags { get; set; }

      public Int32? NumberOfDataDirectories { get; set; }

      public String ImportDirectoryName { get; set; }

      public String ImportHintName { get; set; }
   }

   public class WritingOptions_CLI
   {
      public WritingOptions_CLI(
         WritingOptions_CLIHeader headerOptions = null,
         WritingOptions_MetaDataRoot mdRootOptions = null,
         WritingOptions_TableStream tablesStreamOptions = null
         )
      {
         this.HeaderOptions = headerOptions ?? new WritingOptions_CLIHeader();
         this.MDRootOptions = mdRootOptions ?? new WritingOptions_MetaDataRoot();
         this.TablesStreamOptions = tablesStreamOptions ?? new WritingOptions_TableStream();
      }

      public WritingOptions_CLIHeader HeaderOptions { get; set; }

      public WritingOptions_MetaDataRoot MDRootOptions { get; set; }

      public WritingOptions_TableStream TablesStreamOptions { get; set; }

   }

   public class WritingOptions_CLIHeader
   {
      public Int16? MajorRuntimeVersion { get; set; }

      public Int16? MinorRuntimeVersion { get; set; }

      public ModuleFlags? ModuleFlags { get; set; }

      public TableIndex? EntryPointToken { get; set; }
   }

   public class WritingOptions_MetaDataRoot
   {
      public Int32? Signature { get; set; }

      public Int16? MajorVersion { get; set; }

      public Int16? MinorVersion { get; set; }

      public Int32? Reserved { get; set; }

      public String VersionString { get; set; }

      public StorageFlags? StorageFlags { get; set; }

      public Byte? Reserved2 { get; set; }
   }

   public class WritingOptions_TableStream
   {
      public Int32? Reserved { get; set; }

      public Byte? HeaderMajorVersion { get; set; }

      public Byte? HeaderMinorVersion { get; set; }

      public Byte? Reserved2 { get; set; }

      public Int32? HeaderExtraData { get; set; }

      public Int64? PresentTablesBitVector { get; set; }

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
            new WritingOptions_CLIHeader()
            {
               EntryPointToken = cliHeader.EntryPointToken,
               MajorRuntimeVersion = (Int16) cliHeader.MajorRuntimeVersion,
               MinorRuntimeVersion = (Int16) cliHeader.MinorRuntimeVersion,
               ModuleFlags = cliHeader.Flags
            },
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
               HeaderExtraData = tableStream.ExtraData,
               HeaderMajorVersion = tableStream.MajorVersion,
               HeaderMinorVersion = tableStream.MinorVersion,
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
