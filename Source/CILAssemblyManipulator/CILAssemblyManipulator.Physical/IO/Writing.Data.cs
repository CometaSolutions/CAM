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
         WritingOptions_CLI cliOptions = null
         )
      {
         this.PEOptions = peOptions ?? new WritingOptions_PE();
         this.CLIOptions = cliOptions ?? new WritingOptions_CLI();
      }

      public Boolean IsExecutable { get; set; }

      public WritingOptions_PE PEOptions { get; }

      public WritingOptions_CLI CLIOptions { get; }
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

      public Int64? HeapReserverSize { get; set; }

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

      // TODO ENC, HasDeleted
   }

   public class WritingData
   {

      public WritingData( CILMetaData md )
      {
         this.MethodRVAs = new List<Int32>( md.MethodDefinitions.RowCount );
         this.FieldRVAs = new List<Int32>( md.FieldRVAs.RowCount );
         this.EmbeddedManifestResourceOffsets = new List<Int32?>( md.ManifestResources.RowCount );
      }
      // PE Header 
      // Sections
      // MD Header
      // W32 Resources
      // Relocation data

      public List<Int32> MethodRVAs { get; }

      public List<Int32> FieldRVAs { get; }

      public List<Int32?> EmbeddedManifestResourceOffsets { get; }
   }

}
