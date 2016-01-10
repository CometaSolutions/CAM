/*
* Copyright 2016 Stanislav Muhametsin. All rights Reserved.
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
using CILAssemblyManipulator.Physical.IO;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
   public static class Comparerz
   {
      private static IEqualityComparer<ImageInformation> _ImageInformationLogicalEqualityComparer = null;

      public static IEqualityComparer<ImageInformation> ImageInformationLogicalEqualityComparer
      {
         get
         {
            var retVal = _ImageInformationLogicalEqualityComparer;
            if ( retVal == null )
            {
               retVal = ComparerFromFunctions.NewEqualityComparer<ImageInformation>( Equality_ImageInformation_Logical, HashCode_HeadersData );
               _ImageInformationLogicalEqualityComparer = retVal;
            }
            return retVal;
         }
      }

      private static Boolean Equality_ImageInformation_Logical( ImageInformation x, ImageInformation y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && Equality_PEInformation_Logical( x.PEInformation, y.PEInformation )
            && Equality_CLIInformation_Logical( x.CLIInformation, y.CLIInformation )
            && Equality_DebugInfo_Logical( x.DebugInformation, y.DebugInformation )
            );
      }

      private static Boolean Equality_PEInformation_Logical( PEInformation x, PEInformation y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && Equality_DOSHeader_Logical( x.DOSHeader, y.DOSHeader )
            && Equality_NTHeader_Logical( x.NTHeader, y.NTHeader )
            );
      }

      private static Boolean Equality_DOSHeader_Logical( DOSHeader x, DOSHeader y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Signature == y.Signature
            );
      }

      private static Boolean Equality_NTHeader_Logical( NTHeader x, NTHeader y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Signature == y.Signature
            && Equality_FileHeader_Logical( x.FileHeader, y.FileHeader )
            && Equality_OptionalHeader_Logical( x.OptionalHeader, y.OptionalHeader )
            );
      }

      private static Boolean Equality_FileHeader_Logical( FileHeader x, FileHeader y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Machine == y.Machine
            && x.TimeDateStamp == y.TimeDateStamp
            && x.Characteristics == y.Characteristics
            );
      }

      private static Boolean Equality_OptionalHeader_Logical( OptionalHeader x, OptionalHeader y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.MajorLinkerVersion == y.MajorLinkerVersion
            && x.MinorLinkerVersion == y.MinorLinkerVersion
            && x.ImageBase == y.ImageBase
            && x.SectionAlignment == y.SectionAlignment
            && x.FileAlignment == y.FileAlignment
            && x.MajorOSVersion == y.MajorOSVersion
            && x.MinorOSVersion == y.MinorOSVersion
            && x.MajorUserVersion == y.MajorUserVersion
            && x.MinorUserVersion == y.MinorUserVersion
            && x.MajorSubsystemVersion == y.MajorSubsystemVersion
            && x.MinorSubsystemVersion == y.MinorSubsystemVersion
            && x.Win32VersionValue == y.Win32VersionValue
            && x.Subsystem == y.Subsystem
            && x.DLLCharacteristics == y.DLLCharacteristics
            && x.StackReserveSize == y.StackReserveSize
            && x.StackCommitSize == y.StackCommitSize
            && x.HeapReserveSize == y.HeapReserveSize
            && x.HeapCommitSize == y.HeapCommitSize
            && x.LoaderFlags == y.LoaderFlags
            && x.NumberOfDataDirectories == y.NumberOfDataDirectories
            );
      }

      private static Boolean Equality_CLIInformation_Logical( CLIInformation x, CLIInformation y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && Equality_CLIHeader_Logical( x.CLIHeader, y.CLIHeader )
            && Equality_MDRoot_Logical( x.MetaDataRoot, y.MetaDataRoot )
            && Equality_MDTableStreamHeader( x.TableStreamHeader, y.TableStreamHeader )
            && x.FieldRVAs.Count == y.FieldRVAs.Count
            && x.MethodRVAs.Count == y.MethodRVAs.Count
            );
      }

      private static Boolean Equality_CLIHeader_Logical( CLIHeader x, CLIHeader y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.MajorRuntimeVersion == y.MajorRuntimeVersion
            && x.MinorRuntimeVersion == y.MinorRuntimeVersion
            && x.Flags == y.Flags
            && x.EntryPointToken == y.EntryPointToken
            );
      }

      private static Boolean Equality_MDRoot_Logical( MetaDataRoot x, MetaDataRoot y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Signature == y.Signature
            && x.MajorVersion == y.MajorVersion
            && x.MinorVersion == y.MinorVersion
            && x.Reserved == y.Reserved
            && x.VersionStringBytes.ArrayQueryEquality( y.VersionStringBytes )
            && x.StorageFlags == y.StorageFlags
            && x.Reserved2 == y.Reserved2
            );
      }

      private static Boolean Equality_MDTableStreamHeader( MetaDataTableStreamHeader x, MetaDataTableStreamHeader y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Reserved == y.Reserved
            && x.MajorVersion == y.MajorVersion
            && x.MinorVersion == y.MinorVersion
            && x.Reserved2 == y.Reserved2
            && NullableEqualityComparer<Int32>.Equals( x.ExtraData, y.ExtraData )
            );
      }

      private static Boolean Equality_DebugInfo_Logical( DebugInformation x, DebugInformation y )
      {
         return Object.ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Timestamp == y.Timestamp
            && x.Characteristics == y.Characteristics
            && x.DebugType == y.DebugType
            && x.VersionMajor == y.VersionMajor
            && x.VersionMinor == y.VersionMinor
            && x.DebugData.ArrayQueryEquality( y.DebugData )
            );
      }

      private static Int32 HashCode_HeadersData( ImageInformation x )
      {
         return x == null ? 0 : ( ( 17 * 23 + (Int32) x.PEInformation.NTHeader.FileHeader.Machine ) * 23 + x.CLIInformation.MetaDataRoot.VersionStringBytes.ArrayQueryHashCode() );
      }
   }
}
