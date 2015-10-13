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
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO
{
   public class DosHeader
   {
      public DosHeader( Int16 signature, Int32 ntHeadersOffset )
      {
         this.Signature = signature;
         this.NTHeadersOffset = ntHeadersOffset;
      }
      public Int16 Signature { get; }
      public Int32 NTHeadersOffset { get; }

      public static DosHeader FromStream( Stream stream )
      {

      }

   }
}
