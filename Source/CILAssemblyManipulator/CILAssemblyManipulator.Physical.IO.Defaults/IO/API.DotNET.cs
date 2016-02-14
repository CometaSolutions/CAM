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
#if !CAM_PHYSICAL_IS_PORTABLE
extern alias CAMPhysical;
extern alias CAMPhysicalIO;

using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;

using CAMPhysicalIO;
using CAMPhysicalIO::CILAssemblyManipulator.Physical.IO;

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO
{
   public static partial class CILMetaDataIO
   {
      public static CILMetaData ReadModuleFrom( String filePath, ReadingArguments rArgs = null )
      {
         using ( var stream = File.OpenRead( filePath ) )
         {
            return stream.ReadModule( rArgs );
         }
      }
   }
}

public static partial class E_CILPhysical
{
   public static void WriteModuleTo( this CILMetaData module, String filePath, WritingArguments eArgs = null )
   {
      using ( var fs = System.IO.File.Open( filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None ) )
      {
         module.WriteModule( fs, eArgs );
      }
   }
}
#endif