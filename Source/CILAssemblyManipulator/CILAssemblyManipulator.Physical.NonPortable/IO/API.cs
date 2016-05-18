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
extern alias CAMPhysical;
extern alias CAMPhysicalIO;

using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;
using CAMPhysical::CILAssemblyManipulator.Physical.Meta;

using CAMPhysicalIO;
using CAMPhysicalIO::CILAssemblyManipulator.Physical.IO;

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.IO;
using CILAssemblyManipulator.Physical.Crypto;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TabularMetaData.Meta;

namespace CILAssemblyManipulator.Physical.IO
{
   /// <summary>
   /// This class contains extension methods related to IO functionality of CAM.Physical, but adding methods to types not defined in CAM.Physical.
   /// </summary>
   public static partial class CILMetaDataIO
   {
      /// <summary>
      /// Reads the serialized compressed meta data from this <see cref="Stream"/> into a <see cref="CILMetaData"/>, starting from the current position of the stream.
      /// </summary>
      /// <param name="stream">The stream to read compressed meta data from.</param>
      /// <param name="rArgs">The optional <see cref="ReadingArguments"/> to hold additional data and to further customize the reading process.</param>
      /// <returns>A new instance of <see cref="CILMetaData"/> holding the deserialized contents of compressed meta data.</returns>
      /// <exception cref="NullReferenceException">If this <see cref="Stream"/> is <c>null</c>.</exception>
      /// <exception cref="BadImageFormatException">If this <see cref="Stream"/> does not contain a managed meta data module.</exception>
      /// <seealso cref="ReadingArguments"/>
      /// <seealso cref="ReaderFunctionalityProvider"/>
      public static CILMetaData ReadModule( this Stream stream, ReadingArguments rArgs = null )
      {
         if ( rArgs == null )
         {
            rArgs = new ReadingArguments();
         }

         var rawValueReading = rArgs.RawValueReading;
         ImageInformation imageInfo;
         var md = ( rArgs.ReaderFunctionalityProvider ?? new Defaults.DefaultReaderFunctionalityProvider() ).ReadMetaDataFromStream(
            stream,
            rArgs.TableInformationProvider ?? Meta.CILMetaDataTableInformationProviderFactory.CreateDefault(),
            rArgs.ErrorHandler,
            rawValueReading == RawValueReading.ToRow,
            out imageInfo
            );

         rArgs.ImageInformation = imageInfo;
         // TODO when RawValueReading.ToReadingArguments is implemented, add byte arrays based on method RVAs, field RVAs, and manifest resources..

         return md;
      }
   }
}

#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{
   /// <summary>
   /// Writes this <see cref="CILMetaData"/> as a compressed module to given byte stream.
   /// </summary>
   /// <param name="md">This <see cref="CILMetaData"/>.</param>
   /// <param name="stream">The byte <see cref="Stream"/> where to write this <see cref="CILMetaData"/>.</param>
   /// <param name="eArgs">The optional <see cref="WritingArguments"/> to control the serialization process.</param>
   /// <exception cref="ArgumentNullException">If <paramref name="stream"/> is <c>null</c>.</exception>
   /// <seealso cref="WritingArguments"/>
   /// <seealso cref="M:E_CLPhysical.WriteModuleTo(CILAssemblyManipulator.Physical.CILMetaData, System.String, CILAssemblyManipulator.Physical.IO.WritingArguments)"/>
   public static void WriteModule( this CILMetaData md, Stream stream, WritingArguments eArgs = null )
   {
      if ( eArgs == null )
      {
         eArgs = new WritingArguments();
      }

      ImageInformation imageInfo;
      var provider = ( eArgs.WriterFunctionalityProvider ?? new CILAssemblyManipulator.Physical.IO.Defaults.DefaultWriterFunctionalityProvider() );
      var cc = eArgs.CryptoCallbacks;
      if ( cc == null )
      {
         using ( cc =
#if CAM_PHYSICAL_IS_PORTABLE
            DefaultCryptoCallbacks.CreateDefaultInstance()
#else
            new CryptoCallbacksDotNET()
#endif
            )
         {
            imageInfo = provider.WriteMetaDataToStream( stream, md, eArgs.WritingOptions, eArgs.StrongName, eArgs.DelaySign, cc, eArgs.SigningAlgorithm, eArgs.ErrorHandler );
         }
      }
      else
      {
         imageInfo = provider.WriteMetaDataToStream( stream, md, eArgs.WritingOptions, eArgs.StrongName, eArgs.DelaySign, cc, eArgs.SigningAlgorithm, eArgs.ErrorHandler );
      }

      eArgs.ImageInformation = imageInfo;
   }
}