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
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.PDB
{
   /// <summary>
   /// This exception is thrown when something goes wrong in <see cref="PDBIO.ReadPDBInstance"/> method.
   /// </summary>
   public class PDBException : Exception
   {
      /// <summary>
      /// Creates a new instance of <see cref="PDBException"/> with given message and optional inner exception.
      /// </summary>
      /// <param name="msg">The exception message.</param>
      /// <param name="inner">The optional inner exception.</param>
      public PDBException( String msg, Exception inner = null )
         : base( msg, inner )
      {

      }
   }
}

