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
 * See the License for the specific _language governing permissions and
 * limitations under the License. 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.PDB
{
   internal static class GUIDs
   {
      // From https://github.com/dotnet/coreclr/blob/master/src/inc/corsym.idl

      //   module LanguageType
      //   {

      //const LPSTR C = "{63a08714-fc37-11d2-904c-00c04fa302a1}";
      //   const LPSTR CPlusPlus = "{3a12d0b7-c26c-11d0-b442-00a0244a1dd2}";
      //   const LPSTR CSharp = "{3f5162f8-07c6-11d3-9053-00c04fa302a1}";
      //   const LPSTR Basic = "{3a12d0b8-c26c-11d0-b442-00a0244a1dd2}";
      //   const LPSTR Java = "{3a12d0b4-c26c-11d0-b442-00a0244a1dd2}";
      //   const LPSTR Cobol = "{af046cd1-d0e1-11d2-977c-00a0c9b4d50c}";
      //   const LPSTR Pascal = "{af046cd2-d0e1-11d2-977c-00a0c9b4d50c}";
      //   const LPSTR ILAssembly = "{af046cd3-d0e1-11d2-977c-00a0c9b4d50c}";
      //   const LPSTR JScript = "{3a12d0b6-c26c-11d0-b442-00a0244a1dd2}";
      //   const LPSTR SMC = "{0d9b9f7b-6611-11d3-bd2a-0000f80849bd}";
      //   const LPSTR MCPlusPlus = "{4b35fde8-07c6-11d3-9053-00c04fa302a1}";
      //}

      //cpp_quote("EXTERN_GUID(CorSym_SourceHash_MD5,  0x406ea660, 0x64cf, 0x4c82, 0xb6, 0xf0, 0x42, 0xd4, 0x81, 0x72, 0xa7, 0x99);")
      //cpp_quote("EXTERN_GUID(CorSym_SourceHash_SHA1, 0xff1816ec, 0xaa5e, 0x4d10, 0x87, 0xf7, 0x6f, 0x49, 0x63, 0x83, 0x34, 0x60);")


      internal static readonly Guid MSIL_METADATA_GUID =
         new Guid( unchecked((Int32) 0xC6EA3FC9), 0x59B3, 0x49D6, 0xBC, 0x25, 0x09, 0x02, 0xBB, 0xAB, 0xB4, 0x60 );

      // TODO lots of others for various languages etc.
   }
}
