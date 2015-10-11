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
