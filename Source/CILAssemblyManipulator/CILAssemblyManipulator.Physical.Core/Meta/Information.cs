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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TabularMetaData.Meta;

namespace CILAssemblyManipulator.Physical.Meta
{
   /// <summary>
   /// This interface further extends <see cref="MetaDataTableInformationProvider"/> to include some aspects which are specific to CIL environment.
   /// </summary>
   /// <remarks>
   /// Unless specifically desired, instead of directly implementing this interface, a <see cref="T:CILAssemblyManipulator.Physical.Meta.DefaultMetaDataTableInformationProvider"/> should be used direclty, or by subclassing.
   /// </remarks>
   public interface CILMetaDataTableInformationProvider : MetaDataTableInformationProvider
   {
      /// <summary>
      /// Gets or creates a new <see cref="OpCodeProvider"/>.
      /// </summary>
      /// <returns>A <see cref="OpCodeProvider"/> supported by this <see cref="CILMetaDataTableInformationProvider"/>.</returns>
      /// <seealso cref="OpCodeProvider"/>
      OpCodeProvider CreateOpCodeProvider();

      /// <summary>
      /// Gets or creates a new <see cref="SignatureProvider"/>.
      /// </summary>
      /// <returns>A <see cref="SignatureProvider"/> supported by this <see cref="CILMetaDataTableInformationProvider"/>.</returns>
      /// <seealso cref="SignatureProvider"/>
      SignatureProvider CreateSignatureProvider();

      // TODO ReOrderingProvider
   }
}
