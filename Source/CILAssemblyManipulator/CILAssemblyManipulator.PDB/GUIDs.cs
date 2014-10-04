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

namespace CILAssemblyManipulator.PDB
{
   internal static class GUIDs
   {
      public static readonly Guid DOCTYPE_TEXT =
         new Guid( 0x5A869D0B, 0x6611, 0x11D3, 189, 0xBD, 0, 0, 0xF8, 0x08, 0x49, 0xBD );
      internal static readonly Guid MSIL_METADATA_GUID =
         new Guid( unchecked( (Int32) 0xC6EA3FC9 ), 0x59B3, 0x49D6, 0xBC, 0x25, 0x09, 0x02, 0xBB, 0xAB, 0xB4, 0x60 );

      // TODO lots of others for various languages etc.
   }
}
