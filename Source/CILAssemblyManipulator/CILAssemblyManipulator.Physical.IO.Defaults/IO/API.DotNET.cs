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
      /// <summary>
      /// This method will read the <see cref="CILMetaData"/> from given file path.
      /// </summary>
      /// <param name="filePath">The file path.</param>
      /// <param name="args">The optional <see cref="ReadingArguments"/>.</param>
      /// <returns>The <see cref="CILMetaData"/> read from given file path.</returns>
      /// <seealso cref="ReadModule"/>
      public static CILMetaData ReadModuleFrom( String filePath, ReadingArguments args = null )
      {
         using ( var stream = File.Open( filePath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
         {
            return stream.ReadModule( args );
         }
      }
   }
}

public static partial class E_CILPhysical
{
   /// <summary>
   /// This method will write the given <see cref="CILMetaData"/> to file located in given file path.
   /// </summary>
   /// <param name="module">The <see cref="CILMetaData"/>.</param>
   /// <param name="filePath">The file path to write the <paramref name="module"/> to.</param>
   /// <param name="args">The optional <see cref="WritingArguments"/>.</param>
   /// <seealso cref="WriteModule"/>
   /// <remarks>
   /// If the file at given <paramref name="filePath"/> does not exist, it will be created.
   /// </remarks>
   public static void WriteModuleTo( this CILMetaData module, String filePath, WritingArguments args = null )
   {
      using ( var fs = File.Open( filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None ) )
      {
         module.WriteModule( fs, args );
      }
   }
}
#endif