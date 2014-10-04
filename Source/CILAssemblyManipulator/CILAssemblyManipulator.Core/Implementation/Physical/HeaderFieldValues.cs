/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
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

namespace CILAssemblyManipulator.Implementation.Physical
{
   internal static class HeaderFieldPossibleValues
   {
      #region PE Header, Characteristics
      public const Int16 IMAGE_FILE_RELOCS_STRIPPED = 0x0001;
      public const Int16 IMAGE_FILE_EXECUTABLE_IMAGE = 0x0002;
      public const Int16 IMAGE_FILE_32BIT_MACHINE = 0x0100;
      public const Int16 IMAGE_FILE_DLL = 0x2000;
      public const Int16 IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x0020;
      #endregion

      #region PE Optional Header, Magic
      public const Int16 PE32 = 0x010B;
      public const Int16 PE64 = 0x020B;
      #endregion

      #region PE Optional Header, Sub system
      public const UInt16 IMAGE_SUBSYSTEM_WINDOWS_CUI = 0x03;
      public const UInt16 IMAGE_SUBSYSTEM_WINDOWS_GUI = 0x02;
      public const UInt16 IMAGE_SUBSYSTEM_WINDOWS_CE_GUI = 0x09;
      #endregion

      #region Section Header, Characteristics
      public const UInt32 SCALE_INDEX = 0x0000001;
      public const UInt32 TYPE_NO_PAD = 0x0000008;
      public const UInt32 CONTAINS_CODE = 0x00000020;
      public const UInt32 CONTAINS_INITIALIZED_DATA = 0x00000040;
      public const UInt32 CONTAINS_UNINITIALIZED_DATA = 0x00000080;
      public const UInt32 LNK_OTHER = 0x0000100;
      public const UInt32 LNK_INFO = 0x000200;
      public const UInt32 LNK_REMOVE = 0x0000800;
      public const UInt32 LNK_COM_DATA = 0x00001000;
      public const UInt32 GP_REL = 0x00008000;
      public const UInt32 MEM_PURGEABLE = 0x00020000;
      public const UInt32 MEM_LOCKED = 0x00040000;
      public const UInt32 MEM_PRELOAD = 0x00080000;
      public const UInt32 ALIGN_1_BYTE = 0x00100000;
      public const UInt32 ALIGN_2_BYTES = 0x00200000;
      public const UInt32 ALIGN_4_BYTES = 0x00300000;
      public const UInt32 ALIGN_8_BYTES = 0x00400000;
      public const UInt32 ALIGN_16_BYTES = 0x00500000;
      public const UInt32 ALIGN_32_BYTES = 0x00600000;
      public const UInt32 ALIGN_64_BYTES = 0x00700000;
      public const UInt32 ALIGN_128_BYTES = 0x00800000;
      public const UInt32 ALIGN_256_BYTES = 0x00900000;
      public const UInt32 ALIGN_512_BYTES = 0x00a00000;
      public const UInt32 ALIGN_1024_BYTES = 0x00b00000;
      public const UInt32 ALIGN_2048_BYTES = 0x00c00000;
      public const UInt32 ALIGN_4096_BYTES = 0x00d00000;
      public const UInt32 ALIGN_8192_BYTES = 0x00e00000;
      public const UInt32 LNK_AND_RELOC_OVERFLOW = 0x01000000;
      public const UInt32 MEM_DISCARDABLE = 0x02000000;
      public const UInt32 MEM_NOT_CACHED = 0x04000000;
      public const UInt32 MEM_NOT_PAGED = 0x08000000;
      public const UInt32 MEM_SHARED = 0x10000000;
      public const UInt32 MEM_EXECUTE = 0x20000000;
      public const UInt32 MEM_READ = 0x40000000;
      public const UInt32 MEM_WRITE = 0x80000000;
      #endregion

      #region Base Relocation Table Entries
      public const Byte IMAGE_REL_BASED_HIGHLOW = 0x3;
      #endregion

      #region CIL Header
      public const Int32 COMIMAGE_FLAGS_ILONLY = 0x00000001;
      public const Int32 COMIMAGE_FLAGS_32BITREQUIRED = 0x00000002;
      public const Int32 COMIMAGE_FLAGS_STRONGNAMESIGNED = 0x00000008;
      public const Int32 COMIMAGE_FLAGS_NATIVE_ENTRYPOINT = 0x00000010;
      public const Int32 COMIMAGE_FLAGS_TRACKDEBUGDATA = 0x00010000;
      public const Int32 COMIMAGE_FLAGS_32BITPREFERRED = 0x00020000;
      #endregion

      #region VTable fixup
      public const Int16 COR_VTABLE_32BIT = 0x0001;
      public const Int16 COR_VTABLE_64BIT = 0x0002;
      public const Int16 COR_VTABLE_FROM_UNMANAGED = 0x0004;
      public const Int16 COR_VTABLE_CALL_MOST_DERIVED = 0x0010;
      #endregion

      #region Metadata Table Stream Header
      public const Byte STRING_STREAM_SIZE_OVER_2BYTES = 0x01;
      public const Byte GUID_STREAM_SIZE_OVER_2BYTES = 0x02;
      public const Byte BLOB_STREAM_SIZE_OVER_2BYTES = 0x04;
      #endregion
   }

   internal static class HeaderFieldOffsetsAndLengths
   {
      #region MS-DOS header

      internal static readonly Byte[] DOS_HEADER_AND_PE_SIG = new Byte[] {
         0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00,
         0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 ,/* lfanew begin */ 0x80, 0x00, 0x00, 0x00, /* lfanew end */
         0x0E, 0x1F, 0xBA, 0x0E, 0x00, 0xB4, 0x09, 0xCD, 0x21, 0xB8, 0x01, 0x4C, 0xCD, 0x21, 0x54, 0x68,
         0x69, 0x73, 0x20, 0x70, 0x72, 0x6F, 0x67, 0x72, 0x61, 0x6D, 0x20, 0x63, 0x61, 0x6E, 0x6E, 0x6F, // is program canno
         0x74, 0x20, 0x62, 0x65, 0x20, 0x72, 0x75, 0x6E, 0x20, 0x69, 0x6E, 0x20, 0x44, 0x4F, 0x53, 0x20, // t be run in DOS 
         0x6D, 0x6F, 0x64, 0x65, 0x2E, 0x0D, 0x0D, 0x0A, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mode....$.......
         0x50, 0x45, 0x00, 0x00 // PE\0\0
      };

      #endregion

      #region PE file header

      internal const Int32 PE_FILE_HEADER_SIZE = 0x14;
      internal const Int32 NUM_SEC_OFFSET = 2;
      internal const Int32 TIMESTAMP_OFFSET = 4;
      internal const Int32 PE_OPTIONAL_HEADER_SIZE_OFFSET = 16;
      internal const Int32 PE_CHARACTERISTICS_OFFSET = 18;

      #endregion

      #region PE optional header
      internal const UInt16 PE_OPTIONAL_HEADER_SIZE_32 = 0x00E0;
      internal const UInt16 PE_OPTIONAL_HEADER_SIZE_64 = PE_OPTIONAL_HEADER_SIZE_32 + 0x0010;
      internal const UInt32 NUMBER_OF_DATA_DIRS = 0x10;
      #endregion

      #region Section header
      internal const Int32 SECTION_HEADER_SIZE = 0x28;
      #endregion

      #region IAT
      internal const UInt32 IAT_SIZE = 8;
      #endregion

      #region CIL Header
      internal const UInt32 CLI_HEADER_SIZE = 0x48;
      internal const Int32 CLI_HEADER_MAJOR_RUNTIME_VERSION = 0x2;
      internal const Int32 CLI_HEADER_MINOR_RUNTIME_VERSION = 0x5;
      #endregion

      #region Import Directory
      internal const Int32 HINT_NAME_MIN_SIZE = 2;
      internal const Int32 IMPORT_DIRECTORY_SIZE = 40;
      #endregion

      #region ILT
      internal const Int32 ILT_SIZE = 8;
      #endregion

      #region Relocation Section
      internal const Int32 RELOC_ARRAY_BASE_SIZE = 12;
      #endregion
   }
}