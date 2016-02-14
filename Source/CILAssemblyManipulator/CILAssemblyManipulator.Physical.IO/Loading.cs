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
extern alias CAMPhysical;
extern alias CAMPhysicalR;
using CAMPhysical;
using CAMPhysicalR;
using CAMPhysical::CILAssemblyManipulator.Physical;
using CAMPhysicalR::CILAssemblyManipulator.Physical.Resolving;

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.Crypto;
using CILAssemblyManipulator.Physical.IO;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.Loading
{
   /// <summary>
   /// This interface further refines the <see cref="CILMetaDataLoader"/> with the assumption that each textual resource represents a binary stream readable by <see cref="CILMetaDataIO.ReadModule"/> method.
   /// </summary>
   public interface CILMetaDataBinaryLoader : CILMetaDataLoader
   {
      /// <summary>
      /// Gets the <see cref="ImageInformation"/> for given <see cref="CILMetaData"/>, or <c>null</c>.
      /// </summary>
      /// <param name="metaData">The <see cref="CILMetaData"/>, which should have been obtained through <see cref="CILMetaDataLoader.GetOrLoadMetaData"/> method of this <see cref="CILMetaDataLoader"/>.</param>
      /// <returns>An instance of <see cref="ImageInformation"/> for given <paramref name="metaData"/> if <paramref name="metaData"/> was obtained through <see cref="CILMetaDataLoader.GetOrLoadMetaData"/> method of this <see cref="CILMetaDataLoader"/>; <c>null</c> otherwise.</returns>
      /// <seealso cref="ImageInformation"/>
      ImageInformation GetImageInformation( CILMetaData metaData );
   }

   /// <summary>
   /// This interface further refines the <see cref="CILMetaDataLoaderResourceCallbacks"/> with ability to get binary stream from a given resource, e.g. file path.
   /// </summary>
   public interface CILMetaDataBinaryLoaderResourceCallbacks : CILMetaDataLoaderResourceCallbacks
   {
      /// <summary>
      /// This method should transform the given textual resource into a byte stream.
      /// </summary>
      /// <param name="resource">The textual resource.</param>
      /// <returns>The <see cref="Stream"/> for the <paramref name="resource"/>.</returns>
      /// <remarks>
      /// In file-oriented loader, this usually means returning the value of <see cref="M:System.IO.File.Open(System.String, System.IO.FileMode, System.IO.FileAccess, System.IO.FileShare)"/>
      /// </remarks>
      /// <seealso cref="AbstractCILMetaDataBinaryLoader{TDictionary}.GetStreamFor(string)"/>
      Stream GetStreamFor( String resource );
   }


}
