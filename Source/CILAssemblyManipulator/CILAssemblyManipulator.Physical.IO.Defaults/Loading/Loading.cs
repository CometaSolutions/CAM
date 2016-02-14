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
extern alias CAMPhysicalIO;

using CAMPhysical;
using CAMPhysicalR;
using CAMPhysical::CILAssemblyManipulator.Physical;
using CAMPhysicalR::CILAssemblyManipulator.Physical.Resolving;

using CAMPhysicalIO;
using CAMPhysicalIO::CILAssemblyManipulator.Physical.IO;
using CAMPhysicalIO::CILAssemblyManipulator.Physical.Loading;

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
   /// This is the delegate for callback used by <see cref="AbstractCILMetaDataBinaryLoader{TDictionary}"/> to create instance of <see cref="ReadingArguments"/> to be used by <see cref="CILMetaDataIO.ReadModule(Stream, ReadingArguments)"/> method.
   /// </summary>
   /// <param name="resourceToLoad">The textual resource (e.g. file path) of module being loaded.</param>
   /// <param name="pathForModuleBeingResolved">The optional path of the module currently being resolved by <see cref="CILMetaDataLoader.ResolveMetaData"/> method. This separates the situations when module is loaded directly by <see cref="CILMetaDataLoader.GetOrLoadMetaData"/> method and when modules are being loaded indirectly by <see cref="CILMetaDataLoader.ResolveMetaData"/> method.</param>
   /// <returns>Should return the <see cref="ReadingArguments"/> to be used when loading module. If <c>null</c> is returned, then the code will create a new instance of <see cref="ReadingArguments"/>.</returns>
   public delegate ReadingArguments ReadingArgumentsFactoryDelegate( String resourceToLoad, String pathForModuleBeingResolved );

   /// <summary>
   /// This class extends <see cref="AbstractCILMetaDataLoader{TDictionary}"/> and implements <see cref="CILMetaDataBinaryLoader"/> in order to provide implementation for common things needed when loading <see cref="CILMetaData"/> from binary stream with <see cref="CILMetaDataIO.ReadModule"/> method.
   /// </summary>
   /// <typeparam name="TDictionary"></typeparam>
   public abstract class AbstractCILMetaDataBinaryLoader<TDictionary> : AbstractCILMetaDataLoader<TDictionary>, CILMetaDataBinaryLoader
      where TDictionary : class, IDictionary<String, CILMetaData>
   {

      private readonly ReadingArgumentsFactoryDelegate _readingArgumentsFactory;
      private readonly IDictionary<CILMetaData, ImageInformation> _imageInfos;

      /// <summary>
      /// Creates a new instance of <see cref="AbstractCILMetaDataBinaryLoader{TDictionary}"/> with given dictionary, <see cref="CryptoCallbacks"/> (for public key token computation), and <see cref="ReadingArgumentsFactoryDelegate"/> to use when creating <see cref="ReadingArguments"/>.
      /// </summary>
      /// <param name="dictionary">The dictionary to cache <see cref="CILMetaData"/> instances.</param>
      /// <param name="cryptoCallbacks">The optional <see cref="CryptoCallbacks"/> to use for public key token computation.</param>
      /// <param name="readingArgsFactory">The optional <see cref="ReadingArgumentsFactoryDelegate"/> to use to create <see cref="ReadingArguments"/>.</param>
      public AbstractCILMetaDataBinaryLoader(
         TDictionary dictionary,
         CryptoCallbacks cryptoCallbacks,
         ReadingArgumentsFactoryDelegate readingArgsFactory
         ) : base( dictionary, cryptoCallbacks )
      {
         this._readingArgumentsFactory = readingArgsFactory;
         this._imageInfos = new Dictionary<CILMetaData, ImageInformation>( ReferenceEqualityComparer<CILMetaData>.ReferenceBasedComparer );
      }

      /// <summary>
      /// This method will use <see cref="CILMetaDataIO.ReadModule"/> method to read the <see cref="CILMetaData"/> from file path represented by given textual resource.
      /// </summary>
      /// <param name="sanitizedResource">The file path.</param>
      /// <param name="pathForModuleBeingResolved">The optional path of the module currently being resolved by <see cref="CILMetaDataLoader.ResolveMetaData"/> method. This separates the situations when module is loaded directly by <see cref="CILMetaDataLoader.GetOrLoadMetaData"/> method and when modules are being loaded indirectly by <see cref="CILMetaDataLoader.ResolveMetaData"/> method.</param>
      /// <returns>A new instance of <see cref="CILMetaData"/> with contents read from the given file path.</returns>
      protected override CILMetaData GetMetaDataFromResource( String sanitizedResource, String pathForModuleBeingResolved )
      {
         using ( var stream = this.GetStreamFor( sanitizedResource ) )
         {
            var rArgs = this._readingArgumentsFactory?.Invoke( sanitizedResource, pathForModuleBeingResolved ) ?? new ReadingArguments();
            var retVal = stream.ReadModule( rArgs );
            if ( this.IsSupportingConcurrency )
            {
               this._imageInfos.GetOrAdd_WithLock( retVal, md => rArgs.ImageInformation );
            }
            else
            {
               this._imageInfos.GetOrAdd_NotThreadSafe( retVal, md => rArgs.ImageInformation );
            }
            return retVal;
         }
      }

      /// <inheritdoc />
      public ImageInformation GetImageInformation( CILMetaData metaData )
      {
         ImageInformation retVal;
         return this._imageInfos.TryGetValue( metaData, out retVal ) ? retVal : null;
      }

      /// <summary>
      /// This method should transform the given textual resource into a byte stream.
      /// </summary>
      /// <param name="resource">The textual resource.</param>
      /// <returns>The <see cref="Stream"/> for the <paramref name="resource"/>.</returns>
      /// <remarks>
      /// In file-oriented loader, this usually means returning the value of <see cref="M:System.IO.File.Open(System.String, System.IO.FileMode, System.IO.FileAccess, System.IO.FileShare)"/>
      /// </remarks>
      protected abstract Stream GetStreamFor( String resource );
   }

   /// <summary>
   /// This class extends the <see cref="AbstractCILMetaDataLoader{TDictionary}"/> and implements <see cref="CILMetaDataLoaderWithCallbacks"/>, so that the required abstract methods of <see cref="AbstractCILMetaDataLoader{TDictionary}"/> will be call-throughs to the methods of <see cref="CILMetaDataLoaderResourceCallbacks"/>.
   /// </summary>
   /// <typeparam name="TDictionary">The type of dictionary to cache instances of <see cref="CILMetaData"/>.</typeparam>
   public abstract class CILMetaDataLoaderWithCallbacks<TDictionary> : AbstractCILMetaDataBinaryLoader<TDictionary>, CILMetaDataLoaderWithCallbacks
      where TDictionary : class, IDictionary<String, CILMetaData>
   {

      /// <summary>
      /// Constructs this <see cref="CILMetaDataLoaderWithCallbacks{TDictionary}"/> with given dictionary for <see cref="CILMetaData"/> cache, given <see cref="CryptoCallbacks"/> for public key token computation, given <see cref="ReadingArgumentsFactoryDelegate"/> to create <see cref="ReadingArguments"/>, and given <see cref="CILMetaDataLoaderResourceCallbacks"/>.
      /// </summary>
      /// <param name="dictionary">The dictionary to cache <see cref="CILMetaData"/> instances.</param>
      /// <param name="cryptoCallbacks">The optional <see cref="CryptoCallbacks"/> to use for public key token computation.</param>
      /// <param name="readingArgsFactory">The optional <see cref="ReadingArgumentsFactoryDelegate"/> to use to create <see cref="ReadingArguments"/>.</param>
      /// <param name="resourceCallbacks">The <see cref="CILMetaDataLoaderResourceCallbacks"/> to use in required methods implementing abstract methods of base class.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="dictionary"/> or <paramref name="resourceCallbacks"/> is <c>null</c>.</exception>
      public CILMetaDataLoaderWithCallbacks(
         TDictionary dictionary,
         CryptoCallbacks cryptoCallbacks,
         ReadingArgumentsFactoryDelegate readingArgsFactory,
         CILMetaDataBinaryLoaderResourceCallbacks resourceCallbacks
         )
         : base( dictionary, cryptoCallbacks, readingArgsFactory )
      {
         ArgumentValidator.ValidateNotNull( "Resource callbacks", resourceCallbacks );

         this.LoaderBinaryCallbacks = resourceCallbacks;
      }

      /// <inheritdoc />
      public CILMetaDataLoaderResourceCallbacks LoaderCallbacks
      {
         get
         {
            return this.LoaderBinaryCallbacks;
         }
      }

      /// <summary>
      /// Gets the <see cref="CILMetaDataBinaryLoaderResourceCallbacks"/> callbacks object of this <see cref="CILMetaDataLoaderWithCallbacks{TDictionary}"/>.
      /// </summary>
      /// <value>The <see cref="CILMetaDataBinaryLoaderResourceCallbacks"/> callbacks object of this <see cref="CILMetaDataLoaderWithCallbacks{TDictionary}"/>.</value>
      public CILMetaDataBinaryLoaderResourceCallbacks LoaderBinaryCallbacks { get; }

      /// <summary>
      /// The implementation of this method is delegated to <see cref="CILMetaDataLoaderResourceCallbacks.SanitizeResource(string)"/>.
      /// </summary>
      /// <param name="resource">The resource to sanitize.</param>
      /// <returns>Result of <see cref="CILMetaDataLoaderResourceCallbacks.SanitizeResource(string)"/>.</returns>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.SanitizeResource(string)"/>
      protected override String SanitizeResource( String resource )
      {
         return this.LoaderBinaryCallbacks.SanitizeResource( resource );
      }

      /// <summary>
      /// The implementation of this method is delegated to <see cref="CILMetaDataLoaderResourceCallbacks.IsValidResource(string)"/>.
      /// </summary>
      /// <param name="resource">The resource to check.</param>
      /// <returns>Result of <see cref="CILMetaDataLoaderResourceCallbacks.IsValidResource(string)"/>.</returns>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.IsValidResource(string)"/>
      protected override Boolean IsValidResource( String resource )
      {
         return this.LoaderBinaryCallbacks.IsValidResource( resource );
      }

      /// <summary>
      /// The implementation of this method is delegated to <see cref="CILMetaDataBinaryLoaderResourceCallbacks.GetStreamFor(string)"/>.
      /// </summary>
      /// <param name="resource">The textual resource.</param>
      /// <returns>Result of <see cref="CILMetaDataBinaryLoaderResourceCallbacks.GetStreamFor(string)"/>.</returns>
      /// <seealso cref="AbstractCILMetaDataBinaryLoader{TDictionary}.GetStreamFor(string)"/>
      protected override Stream GetStreamFor( String resource )
      {
         return this.LoaderBinaryCallbacks.GetStreamFor( resource );
      }

      /// <summary>
      /// The implementation of this method is delegated to <see cref="CILMetaDataLoaderResourceCallbacks.GetPossibleResourcesForModuleReference(string, CILMetaData, string)"/>.
      /// </summary>
      /// <param name="thisMetaDataResource">The textual resource of this <see cref="CILMetaData"/> instance.</param>
      /// <param name="thisMetaData">This <see cref="CILMetaData"/> instance.</param>
      /// <param name="moduleReferenceName">The name of the referenced module.</param>
      /// <returns>Result of <see cref="CILMetaDataLoaderResourceCallbacks.GetPossibleResourcesForModuleReference(string, CILMetaData, string)"/>.</returns>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.GetPossibleResourcesForModuleReference(string, CILMetaData, string)"/>
      protected override IEnumerable<String> GetPossibleResourcesForModuleReference( String thisMetaDataResource, CILMetaData thisMetaData, String moduleReferenceName )
      {
         return this.LoaderBinaryCallbacks.GetPossibleResourcesForModuleReference( thisMetaDataResource, thisMetaData, moduleReferenceName );
      }

      /// <summary>
      /// The implementation of this method is delegated to <see cref="CILMetaDataLoaderResourceCallbacks.GetPossibleResourcesForAssemblyReference(string, CILMetaData, AssemblyInformationForResolving, string)"/>.
      /// </summary>
      /// <param name="thisMetaDataResource">The textual resource of this <see cref="CILMetaData"/> instance.</param>
      /// <param name="thisMetaData">This <see cref="CILMetaData"/> instance.</param>
      /// <param name="assemblyRefInfo">The constructed <see cref="AssemblyInformationForResolving"/>, or <c>null</c>.</param>
      /// <param name="unparsedAssemblyName">The unparsed textual assembly name, or <c>null</c>.</param>
      /// <returns>Result of <see cref="CILMetaDataLoaderResourceCallbacks.GetPossibleResourcesForAssemblyReference(string, CILMetaData, AssemblyInformationForResolving, string)"/>.</returns>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.GetPossibleResourcesForAssemblyReference(string, CILMetaData, AssemblyInformationForResolving, string)"/>
      protected override IEnumerable<String> GetPossibleResourcesForAssemblyReference( String thisMetaDataResource, CILMetaData thisMetaData, AssemblyInformationForResolving assemblyRefInfo, String unparsedAssemblyName )
      {
         return this.LoaderBinaryCallbacks.GetPossibleResourcesForAssemblyReference( thisMetaDataResource, thisMetaData, assemblyRefInfo, unparsedAssemblyName );
      }

   }

   /// <summary>
   /// This class implements <see cref="CILMetaDataLoaderWithCallbacks{TDictionary}"/> in a not thread-safe way.
   /// </summary>
   public class CILMetaDataLoaderNotThreadSafe : CILMetaDataLoaderWithCallbacks<Dictionary<String, CILMetaData>>
   {
      /// <summary>
      /// Creates a new instance of <see cref="CILMetaDataLoaderNotThreadSafe"/> with given <see cref="CryptoCallbacks"/> for public key computation, <see cref="ReadingArgumentsFactoryDelegate"/> to create <see cref="ReadingArguments"/>, and <see cref="CILMetaDataLoaderResourceCallbacks"/> to handle textual resources.
      /// </summary>
      /// <param name="cryptoCallbacks">The optional <see cref="CryptoCallbacks"/> to use for public key token computation.</param>
      /// <param name="readingArgsFactory">The optional <see cref="ReadingArgumentsFactoryDelegate"/> to use to create <see cref="ReadingArguments"/>.</param>
      /// <param name="resourceCallbacks">The <see cref="CILMetaDataLoaderResourceCallbacks"/> to handle textual resources.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="resourceCallbacks"/> is <c>null</c>.</exception>
      public CILMetaDataLoaderNotThreadSafe(
         CryptoCallbacks cryptoCallbacks,
         ReadingArgumentsFactoryDelegate readingArgsFactory,
         CILMetaDataBinaryLoaderResourceCallbacks resourceCallbacks
         )
         : base( new Dictionary<String, CILMetaData>(), cryptoCallbacks, readingArgsFactory, resourceCallbacks )
      {

      }

      /// <summary>
      /// Gets or adds from the <see cref="AbstractCILMetaDataLoader{TDictionary}.Cache"/> in not thread-safe way.
      /// </summary>
      /// <param name="resource">The resource (key).</param>
      /// <param name="factory">The factory to create value, if needed.</param>
      /// <returns>The existing or created <see cref="CILMetaData"/>.</returns>
      /// <remarks>
      /// This method uses <see cref="E_CommonUtils.GetOrAdd_NotThreadSafe{TKey, TValue}(IDictionary{TKey, TValue}, TKey, Func{TKey, TValue})"/> method to get or create the <see cref="CILMetaData"/>.
      /// </remarks>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.GetOrAddFromCache"/>
      protected override CILMetaData GetOrAddFromCache( String resource, Func<String, CILMetaData> factory )
      {
         return this.Cache.GetOrAdd_NotThreadSafe( resource, factory );
      }

      /// <summary>
      /// This method simply calls <see cref="CAMPhysicalResolving::E_CILPhysical.ResolveEverything(MetaDataResolver, CILMetaData)"/>.
      /// </summary>
      /// <param name="resolver">The <see cref="MetaDataResolver"/>.</param>
      /// <param name="metaData">The <see cref="CILMetaData"/>.</param>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.PerformResolving"/>
      protected override void PerformResolving( CILMetaData metaData )
      {
         metaData.ResolveEverything();
      }

      /// <summary>
      /// Returns <c>false</c>.
      /// </summary>
      /// <value>The <c>false</c>.</value>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.IsSupportingConcurrency"/>
      protected sealed override Boolean IsSupportingConcurrency
      {
         get
         {
            return false;
         }
      }
   }

   /// <summary>
   /// This class implements <see cref="CILMetaDataLoaderWithCallbacks{TDictionary}"/> in a simple thread-safe way.
   /// This involves locking whole cache when the <see cref="CILMetaData"/> is still not in cache, which is simple, but not most performant way to solve concurrency.
   /// </summary>
   public class CILMetaDataLoaderThreadSafeSimple : CILMetaDataLoaderWithCallbacks<Dictionary<String, CILMetaData>>
   {

      /// <summary>
      /// Creates a new instance of <see cref="CILMetaDataLoaderThreadSafeSimple"/> with given <see cref="CryptoCallbacks"/> for public key computation, <see cref="ReadingArgumentsFactoryDelegate"/> to create <see cref="ReadingArguments"/>, and <see cref="CILMetaDataLoaderResourceCallbacks"/> to handle textual resources.
      /// </summary>
      /// <param name="cryptoCallbacks">The optional <see cref="CryptoCallbacks"/> to use for public key token computation.</param>
      /// <param name="readingArgsFactory">The optional <see cref="ReadingArgumentsFactoryDelegate"/> to use to create <see cref="ReadingArguments"/>.</param>
      /// <param name="resourceCallbacks">The <see cref="CILMetaDataLoaderResourceCallbacks"/> to handle textual resources.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="resourceCallbacks"/> is <c>null</c>.</exception>
      public CILMetaDataLoaderThreadSafeSimple(
         CryptoCallbacks cryptoCallbacks,
         ReadingArgumentsFactoryDelegate readingArgsFactory,
         CILMetaDataBinaryLoaderResourceCallbacks resourceCallbacks
         )
         : base( new Dictionary<String, CILMetaData>(), cryptoCallbacks, readingArgsFactory, resourceCallbacks )
      {

      }

      /// <summary>
      /// Gets or adds from the <see cref="AbstractCILMetaDataLoader{TDictionary}.Cache"/> in thread-safe way.
      /// </summary>
      /// <param name="resource">The resource (key).</param>
      /// <param name="factory">The factory to create value, if needed.</param>
      /// <returns>The existing or created <see cref="CILMetaData"/>.</returns>
      /// <remarks>
      /// This method uses <see cref="E_CommonUtils.GetOrAdd_WithLock{TKey, TValue}(IDictionary{TKey, TValue}, TKey, Func{TKey, TValue}, object)"/> method to get or create the <see cref="CILMetaData"/>.
      /// </remarks>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.GetOrAddFromCache"/>
      protected override CILMetaData GetOrAddFromCache( String resource, Func<String, CILMetaData> factory )
      {
         return this.Cache.GetOrAdd_WithLock( resource, factory );
      }

      /// <summary>
      /// This method locks the given <see cref="MetaDataResolver"/>, and then calls <see cref="CAMPhysicalResolving::E_CILPhysical.ResolveEverything(MetaDataResolver, CILMetaData)"/>.
      /// </summary>
      /// <param name="resolver">The <see cref="MetaDataResolver"/>.</param>
      /// <param name="metaData">The <see cref="CILMetaData"/>.</param>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.PerformResolving"/>
      /// <remarks>
      /// The locking is done in case an attempt is made to resolve same <see cref="CILMetaData"/> concurrently, since <see cref="MetaDataResolver"/> itself does not support concurrency.
      /// </remarks>
      protected override void PerformResolving( CILMetaData metaData )
      {
         // In case we are trying to resolve same module concurrently
         lock ( metaData )
         {
            metaData.ResolveEverything();
         }
      }

      /// <summary>
      /// Returns <c>true</c>.
      /// </summary>
      /// <value>The <c>true</c>.</value>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.IsSupportingConcurrency"/>
      protected sealed override Boolean IsSupportingConcurrency
      {
         get
         {
            return true;
         }
      }
   }
}
