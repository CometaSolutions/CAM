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
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.IO;
using CILAssemblyManipulator.Physical.Crypto;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO
{
   using TModuleInfo = Tuple<String, ReadingArguments, MetaDataResolver>;

   /// <summary>
   /// This interface represents a object which caches instances of <see cref="CILMetaData"/> based on textual resource, e.g. file path.
   /// </summary>
   /// <remarks>
   /// This interface does not specify whether it is thread-safe or not, that depends on the class implementing this interface.
#if !CAM_PHYSICAL_IS_PORTABLE
   /// The <see cref="CILMetaDataLoaderNotThreadSafeForFiles"/>, <see cref="CILMetaDataLoaderThreadSafeSimpleForFiles"/>, and <see cref="CILMetaDataLoaderThreadSafeConcurrentForFiles"/> provide ready-to-use implementation of this interface.
#endif
   /// The <see cref="CILMetaDataLoaderNotThreadSafe"/> and <see cref="CILMetaDataLoaderThreadSafeSimple"/> implement this interface in a way suitable for most of the portable usage scenarios.
   /// </remarks>
   public interface CILMetaDataLoader : IDisposable
   {
      /// <summary>
      /// Gets or loads the <see cref="CILMetaData"/> from the given textual resource (e.g. file path).
      /// </summary>
      /// <param name="resource">The textual resource identifier.</param>
      /// <returns>The cached or loaded instance of <see cref="CILMetaData"/>. If the instance is loaded, it is then cached for this <paramref name="resource"/>.</returns>
      /// <exception cref="MetaDataLoadException">If <see cref="CILMetaData"/> will need to be loaded, and something goes wrong during loading. One should examine the <see cref="Exception.InnerException"/> property of the catched <see cref="MetaDataLoadException"/> to further investigate what went wrong.</exception>
      CILMetaData GetOrLoadMetaData( String resource );

      /// <summary>
      /// Checks whether this <see cref="CILMetaDataLoader"/> has a <see cref="CILMetaData"/> cached for given textual resource.
      /// </summary>
      /// <param name="resource">The textual resource.</param>
      /// <returns><c>true</c> if this <see cref="CILMetaDataLoader"/> has a <see cref="CILMetaData"/> cached for given textual resource; <c>false</c> otherwise.</returns>
      Boolean IsResourceCached( String resource );

      /// <summary>
      /// When the <see cref="CILMetaData"/> is loaded, some of its signatures are left to raw forms (e.g. <see cref="CustomAttributeDefinition.Signature"/>, for more info see <see cref="ReaderBLOBStreamHandler.ReadCASignature(int, Meta.SignatureProvider)"/> method).
      /// </summary>
      /// <param name="metaData">The <see cref="CILMetaData"/>, which should have been obtained through <see cref="GetOrLoadMetaData"/> method of this <see cref="CILMetaDataLoader"/>.</param>
      /// <returns><c>true</c> if <paramref name="metaData"/> was obtained through <see cref="GetOrLoadMetaData"/> method of this <see cref="CILMetaDataLoader"/>; <c>false</c> otherwise.</returns>
      Boolean ResolveMetaData( CILMetaData metaData );

      // TODO this method might not be portable enough - what if implementation of this interface uses something else than serialization provided by this framework?
      /// <summary>
      /// Gets the <see cref="ReadingArguments"/> used to load the <see cref="CILMetaData"/>, or <c>null</c>.
      /// </summary>
      /// <param name="metaData">The <see cref="CILMetaData"/>, which should have been obtained through <see cref="GetOrLoadMetaData"/> method of this <see cref="CILMetaDataLoader"/>.</param>
      /// <returns>An instance of <see cref="ReadingArguments"/> used to load given <paramref name="metaData"/> if <paramref name="metaData"/> was obtained through <see cref="GetOrLoadMetaData"/> method of this <see cref="CILMetaDataLoader"/>; <c>null</c> otherwise.</returns>
      /// <seealso cref="ReadingArguments"/>
      ReadingArguments GetReadingArgumentsForMetaData( CILMetaData metaData );

      /// <summary>
      /// Gets the resource used to load the <see cref="CILMetaData"/>, or <c>null</c>.
      /// </summary>
      /// <param name="metaData">The <see cref="CILMetaData"/>, which should have been obtained through <see cref="GetOrLoadMetaData"/> method of this <see cref="CILMetaDataLoader"/>.</param>
      /// <returns>A textual resource used to load given <paramref name="metaData"/> if <paramref name="metaData"/> was obtained through <see cref="GetOrLoadMetaData"/> method of this <see cref="CILMetaDataLoader"/>; <c>null</c> otherwise.</returns>
      String GetResourceFor( CILMetaData metaData );

      /// <summary>
      /// Creates a new instance of <see cref="MetaDataResolver"/> with its <see cref="MetaDataResolver.ModuleReferenceResolveEvent"/> and <see cref="MetaDataResolver.AssemblyReferenceResolveEvent"/> events having proper handlers using this <see cref="CILMetaDataLoader"/>.
      /// </summary>
      /// <returns>A new instance of <see cref="MetaDataResolver"/> to be used for resolving <see cref="CILMetaData"/>s with this <see cref="CILMetaDataLoader"/>.</returns>
      /// <seealso cref="MetaDataResolver"/>
      MetaDataResolver CreateNewResolver();

      /// <summary>
      /// Computes the public key token based on given public key.
      /// </summary>
      /// <param name="publicKey">The public key. May be <c>null</c>.</param>
      /// <returns>The public key token, or <c>null</c> if <paramref name="publicKey"/> is <c>null</c> or if this <see cref="CILMetaDataLoader"/> is unable to compute public key tokens.</returns>
      Byte[] ComputePublicKeyTokenOrNull( Byte[] publicKey );
   }

   /// <summary>
   /// This exception is thrown by <see cref="CILMetaDataLoader.GetOrLoadMetaData"/> method when somethign goes wrong when loading <see cref="CILMetaData"/> from stream.
   /// </summary>
   public class MetaDataLoadException : Exception
   {
      /// <summary>
      /// Creates a new instance of <see cref="MetaDataLoadException"/> with given message and optional inner exception.
      /// </summary>
      /// <param name="msg">The textual message.</param>
      /// <param name="inner">The optional inner exception.</param>
      public MetaDataLoadException( String msg, Exception inner = null )
         : base( msg, inner )
      {

      }
   }

   /// <summary>
   /// This is the delegate for callback used by <see cref="AbstractCILMetaDataLoader{TDictionary}"/> to create instance of <see cref="ReadingArguments"/> to be used by <see cref="CILMetaDataIO.ReadModule(Stream, ReadingArguments)"/> method.
   /// </summary>
   /// <param name="resourceToLoad">The textual resource (e.g. file path) of module being loaded.</param>
   /// <param name="pathForModuleBeingResolved">The optional path of the module currently being resolved by <see cref="CILMetaDataLoader.ResolveMetaData"/> method. This separates the situations when module is loaded directly by <see cref="CILMetaDataLoader.GetOrLoadMetaData"/> method and when modules are being loaded indirectly by <see cref="CILMetaDataLoader.ResolveMetaData"/> method.</param>
   /// <returns>Should return the <see cref="ReadingArguments"/> to be used when loading module. If <c>null</c> is returned, then the code will create a new instance of <see cref="ReadingArguments"/>.</returns>
   public delegate ReadingArguments ReadingArgumentsFactoryDelegate( String resourceToLoad, String pathForModuleBeingResolved );

   /// <summary>
   /// This class provides skeleton implementation of <see cref="CILMetaDataLoader"/> with parametrizable dictionary type (for possible use of <see cref="T:System.Collections.Concurrent.ConcurrentDictionary"/>).
   /// </summary>
   /// <typeparam name="TDictionary">The type of the dictionary to hold cached <see cref="CILMetaData"/> instances.</typeparam>
   /// <remarks>
   /// This class assumes that each textual resource can be transformed into a <see cref="Stream"/>, from which the <see cref="CILMetaData"/> is then read.
   /// </remarks>
   public abstract class AbstractCILMetaDataLoader<TDictionary> : AbstractDisposable, CILMetaDataLoader
      where TDictionary : class, IDictionary<String, CILMetaData>
   {
      private readonly TDictionary _modules;
      private readonly Dictionary<CILMetaData, TModuleInfo> _moduleInfos;
      private readonly Lazy<HashStreamInfo> _hashStream;
      private readonly ReadingArgumentsFactoryDelegate _readingArgumentsFactory;

      /// <summary>
      /// Constructs this <see cref="AbstractCILMetaDataLoader{TDictionary}"/> with given dictionary, <see cref="CryptoCallbacks"/> (for public key token computation), and <see cref="ReadingArgumentsFactoryDelegate"/> to use when creating <see cref="ReadingArguments"/>.
      /// </summary>
      /// <param name="dictionary">The dictionary to cache <see cref="CILMetaData"/> instances.</param>
      /// <param name="cryptoCallbacks">The optional <see cref="CryptoCallbacks"/> to use for public key token computation.</param>
      /// <param name="readingArgsFactory">The optional <see cref="ReadingArgumentsFactoryDelegate"/> to use to create <see cref="ReadingArguments"/>.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="dictionary"/> is <c>null</c>.</exception>
      public AbstractCILMetaDataLoader(
         TDictionary dictionary,
         CryptoCallbacks cryptoCallbacks,
         ReadingArgumentsFactoryDelegate readingArgsFactory
         )
      {
         ArgumentValidator.ValidateNotNull( "Modules", dictionary );

         this._modules = dictionary;
         this._moduleInfos = new Dictionary<CILMetaData, TModuleInfo>( ReferenceEqualityComparer<CILMetaData>.ReferenceBasedComparer );
         this._hashStream = cryptoCallbacks == null ? null : new Lazy<HashStreamInfo>( () => cryptoCallbacks.CreateHashStream( AssemblyHashAlgorithm.SHA1 ), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );
         this._readingArgumentsFactory = readingArgsFactory ?? new ReadingArgumentsFactoryDelegate( ( resource, resolving ) => new ReadingArguments() );
      }

      private void _resolver_ModuleReferenceResolveEvent( Object sender, ModuleReferenceResolveEventArgs e )
      {
         TModuleInfo thisModuleInfo;
         if ( this._moduleInfos.TryGetValue( e.ThisMetaData, out thisModuleInfo ) )
         {
            var thisResource = thisModuleInfo.Item1;
            e.ResolvedMetaData = this
               .GetPossibleResourcesForModuleReference( thisResource, e.ThisMetaData, e.ModuleName )
               .Where( r => this.IsValidResource( r ) )
               .Select( r => this.GetOrLoadMetaData( r, thisResource ) )
               .Where( md => md.AssemblyDefinitions.GetRowCount() == 0 && md.ModuleDefinitions.GetRowCount() > 0 && String.Equals( md.ModuleDefinitions.TableContents[0].Name, e.ModuleName ) )
               .FirstOrDefault();
         }
      }

      private void _resolver_AssemblyReferenceResolveEvent( Object sender, AssemblyReferenceResolveEventArgs e )
      {
         TModuleInfo thisModuleInfo;
         if ( this._moduleInfos.TryGetValue( e.ThisMetaData, out thisModuleInfo ) )
         {
            var thisResource = thisModuleInfo.Item1;
            e.ResolvedMetaData = this
               .GetPossibleResourcesForAssemblyReference( thisResource, e.ThisMetaData, e.AssemblyInformation, e.UnparsedAssemblyName )
               .Where( r => this.IsValidResource( r ) )
               .Select( r => this.GetOrLoadMetaData( r, thisResource ) )
               .Where( md => md.AssemblyDefinitions.GetOrNull( 0 )?.IsMatch( e.AssemblyInformation, false, this.ComputePublicKeyTokenOrNull ) ?? false )
               .FirstOrDefault();
         }
      }

      /// <inheritdoc />
      public CILMetaData GetOrLoadMetaData( String resource )
      {
         return this.GetOrLoadMetaData( resource, null );
      }

      /// <inheritdoc />
      public Boolean IsResourceCached( String resource )
      {
         resource = this.SanitizeResource( resource );
         return resource != null && this.Cache.ContainsKey( resource );
      }

      /// <inheritdoc />
      public Boolean ResolveMetaData( CILMetaData metaData )
      {
         TModuleInfo moduleInfo;
         var retVal = this._moduleInfos.TryGetValue( metaData, out moduleInfo );

         if ( retVal )
         {
            this.PerformResolving( moduleInfo.Item3, metaData );
         }

         return retVal;
      }

      /// <inheritdoc />
      public ReadingArguments GetReadingArgumentsForMetaData( CILMetaData metaData )
      {
         TModuleInfo moduleInfo;
         return this._moduleInfos.TryGetValue( metaData, out moduleInfo ) ?
            moduleInfo.Item2 :
            null;
      }

      /// <inheritdoc />
      public String GetResourceFor( CILMetaData metaData )
      {
         TModuleInfo moduleInfo;
         return this._moduleInfos.TryGetValue( metaData, out moduleInfo ) ?
            moduleInfo.Item1 :
            null;
      }

      /// <inheritdoc />
      public MetaDataResolver CreateNewResolver()
      {
         var resolver = new MetaDataResolver();
         resolver.AssemblyReferenceResolveEvent += _resolver_AssemblyReferenceResolveEvent;
         resolver.ModuleReferenceResolveEvent += _resolver_ModuleReferenceResolveEvent;
         return resolver;
      }

      /// <inheritdoc />
      public Byte[] ComputePublicKeyTokenOrNull( Byte[] publicKey )
      {
         return this._hashStream?.Value.ComputePublicKeyToken( publicKey );
      }

      private TModuleInfo ModuleInfoFactory( String resource, CILMetaData md, ReadingArguments rArgs )
      {
         var resolver = this.CreateNewResolver();
         return Tuple.Create( resource, rArgs, resolver );
      }

      /// <summary>
      /// Gets the dictionary used to cache instances of <see cref="CILMetaData"/>.
      /// </summary>
      /// <value>The dictionary used to cache instances of <see cref="CILMetaData"/>.</value>
      protected TDictionary Cache
      {
         get
         {
            return this._modules;
         }

      }

      /// <summary>
      /// Gets the value indicating whether this <see cref="AbstractCILMetaDataLoader{TDictionary}"/> supports concurrency.
      /// </summary>
      /// <value>The value indicating whether this <see cref="AbstractCILMetaDataLoader{TDictionary}"/> supports concurrency.</value>
      /// <remarks>
      /// This method is used by this class after updating internal information about specific <see cref="CILMetaData"/> after it has been loaded.
      /// </remarks>
      protected abstract Boolean IsSupportingConcurrency { get; }

      /// <summary>
      /// This method will be used before checking whether there is a cached <see cref="CILMetaData"/> for given resource, by <see cref="GetOrLoadMetaData(string)" /> method.
      /// </summary>
      /// <param name="resource">The given textual resource.</param>
      /// <returns>The sanitized resource.</returns>
      /// <remarks>
      /// In file-oriented loader, this usually means returning the value of <see cref="M:System.IO.Path.GetFullPath(System.String)"/>.
      /// </remarks>
      protected abstract String SanitizeResource( String resource );

      /// <summary>
      /// This method will be used when searching for referenced assembly or module when performing resolving (<see cref="ResolveMetaData(CILMetaData)"/>).
      /// </summary>
      /// <param name="resource">The resource to check.</param>
      /// <returns><c>true</c> if the resource is valid; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// In file-oriented loader, this usually means returning the value of <see cref="M:System.IO.File.Exists(System.String)"/>
      /// </remarks>
      protected abstract Boolean IsValidResource( String resource );

      /// <summary>
      /// This method should transform the given textual resource into a byte stream.
      /// </summary>
      /// <param name="resource">The textual resource.</param>
      /// <returns>The <see cref="Stream"/> for the <paramref name="resource"/>.</returns>
      /// <remarks>
      /// In file-oriented loader, this usually means returning the value of <see cref="M:System.IO.File.Open(System.String, System.IO.FileMode, System.IO.FileAccess, System.IO.FileShare)"/>
      /// </remarks>
      protected abstract Stream GetStreamFor( String resource );

      /// <summary>
      /// This method should return all possible (valid or not valid) textual resources for a module reference occurring in a <see cref="CILMetaData"/>.
      /// </summary>
      /// <param name="thisMetaDataResource">The textual resource for this <see cref="CILMetaData"/>.</param>
      /// <param name="thisMetaData">This <see cref="CILMetaData"/> instance.</param>
      /// <param name="moduleReferenceName">The name of the module reference, that <paramref name="thisMetaData"/> references.</param>
      /// <returns>The enumerable for all possible (valid or not valid) resource for a module reference.</returns>
      /// <remarks>
      /// This method is used during resolving of <see cref="CILMetaData"/> in <see cref="ResolveMetaData(CILMetaData)"/>.
      /// </remarks>
      protected abstract IEnumerable<String> GetPossibleResourcesForModuleReference( String thisMetaDataResource, CILMetaData thisMetaData, String moduleReferenceName );

      /// <summary>
      /// This method should return all possible (valid or not valid) textual resources for an assembly reference occurring in a <see cref="CILMetaData"/>.
      /// </summary>
      /// <param name="thisMetaDataResource">The textual resource for this <see cref="CILMetaData"/>.</param>
      /// <param name="thisMetaData">This <see cref="CILMetaData"/> instance.</param>
      /// <param name="assemblyRefInfo">The assembly reference information, either from <see cref="AssemblyReference"/> or from textual type name. Will be <c>null</c> if the assembly information could not be parsed from textual type name.</param>
      /// <param name="unparsedAssemblyName">This will be non-<c>null</c> only when the textual type name contained assembly name, which was unparseable.</param>
      /// <returns>The enumerable for all possible (valid or not valid) textual resources for an assembly reference.</returns>
      /// <remarks>
      /// This method is used during resolving of <see cref="CILMetaData"/> in <see cref="ResolveMetaData(CILMetaData)"/>.
      /// </remarks>
      protected abstract IEnumerable<String> GetPossibleResourcesForAssemblyReference( String thisMetaDataResource, CILMetaData thisMetaData, AssemblyInformationForResolving assemblyRefInfo, String unparsedAssemblyName );

      /// <summary>
      /// Gets or adds the value from <see cref="Cache"/>.
      /// </summary>
      /// <param name="resource">The resource (key) to the dictionary.</param>
      /// <param name="factory">The factory callback if the key is not present.</param>
      /// <returns>The value, either the one existing in cache, or returned by <paramref name="factory"/>.</returns>
      /// <remarks>
      /// The purpose of this method is to customize whether <see cref="E_CommonUtils.GetOrAdd_NotThreadSafe{TKey, TValue}(IDictionary{TKey, TValue}, TKey, Func{TKey, TValue})"/>, <see cref="E_CommonUtils.GetOrAdd_WithLock{TKey, TValue}(IDictionary{TKey, TValue}, TKey, Func{TKey, TValue}, object)"/>, or <see cref="M:System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, Func{TKey, TValue})"/> is used to obtain the value from the cache.
      /// </remarks>
      protected abstract CILMetaData GetOrAddFromCache( String resource, Func<String, CILMetaData> factory );

      /// <summary>
      /// This method is called by <see cref="ResolveMetaData(CILMetaData)"/> to actually perform resolving.
      /// </summary>
      /// <param name="resolver">The <see cref="MetaDataResolver"/> to use.</param>
      /// <param name="metaData">The <see cref="CILMetaData"/> to resolve.</param>
      /// <remarks>
      /// The purpose of this method is to enable e.g. locking or some other function around/before/after resolving.
      /// It is assumed that this method will call <see cref="E_CILPhysical.ResolveEverything(MetaDataResolver, CILMetaData)"/>.
      /// </remarks>
      /// <seealso cref="MetaDataResolver"/>
      /// <seealso cref="E_CILPhysical.ResolveEverything(MetaDataResolver, CILMetaData)"/>
      protected abstract void PerformResolving( MetaDataResolver resolver, CILMetaData metaData );

      /// <summary>
      /// Disposes resources created by <see cref="CryptoCallbacks"/> supplied to this instance, if any.
      /// </summary>
      /// <param name="disposing">Whether this is called from <see cref="AbstractDisposable.Dispose"/> method.</param>
      protected override void Dispose( Boolean disposing )
      {
         if ( disposing && this._hashStream != null && this._hashStream.IsValueCreated )
         {
            this._hashStream.Value.Transform.DisposeSafely();
         }
      }

      private CILMetaData GetOrLoadMetaData( String resource, String pathForModuleBeingResolved )
      {
         resource = this.SanitizeResource( resource );
         ArgumentValidator.ValidateNotNull( "Resource", resource );
         ReadingArguments rArgs = null;

         var retVal = this.GetOrAddFromCache( resource, res =>
         {
            using ( var stream = this.GetStreamFor( res ) )
            {
               rArgs = this._readingArgumentsFactory( resource, pathForModuleBeingResolved ) ?? new ReadingArguments();
               try
               {
                  return stream.ReadModule( rArgs );
               }
               catch ( Exception exc )
               {
                  throw new MetaDataLoadException( "Error when loading CIL module from " + resource + ".", exc );
               }
            }
         } );

         Boolean added;
         if ( this.IsSupportingConcurrency )
         {
            this._moduleInfos.GetOrAdd_WithLock( retVal, md => this.ModuleInfoFactory( resource, md, rArgs ), out added );
         }
         else
         {
            this._moduleInfos.GetOrAdd_NotThreadSafe( retVal, md => this.ModuleInfoFactory( resource, md, rArgs ), out added );
         }

         if ( added )
         {
            // TODO possibly event? ModuleLoadedEvent
         }

         return retVal;
      }

   }

   /// <summary>
   /// This interface specializes <see cref="CILMetaDataLoader"/> by exposing the <see cref="CILMetaDataLoaderResourceCallbacks"/>, which encapsulate the methods left abstract by <see cref="AbstractCILMetaDataLoader{TDictionary}"/>.
   /// </summary>
   /// <seealso cref="CILMetaDataLoaderResourceCallbacks"/>
   /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}"/>
   public interface CILMetaDataLoaderWithCallbacks : CILMetaDataLoader
   {
      /// <summary>
      /// Gets the <see cref="CILMetaDataLoaderResourceCallbacks"/> for this <see cref="CILMetaDataLoaderWithCallbacks"/>.
      /// </summary>
      /// <value>The <see cref="CILMetaDataLoaderResourceCallbacks"/> for this <see cref="CILMetaDataLoaderWithCallbacks"/>.</value>
      /// <seealso cref="CILMetaDataLoaderResourceCallbacks"/>
      CILMetaDataLoaderResourceCallbacks LoaderCallbacks { get; }
   }

   /// <summary>
   /// This interface combines most abstract methods of <see cref="AbstractCILMetaDataLoader{TDictionary}"/> into separate interface, which then may be used directly from outside of <see cref="CILMetaDataLoader"/> or <see cref="AbstractCILMetaDataLoader{TDictionary}"/>.
   /// Some additional utility methods are added to this interface as well.
   /// </summary>
   public interface CILMetaDataLoaderResourceCallbacks
   {
      /// <summary>
      /// This method will be used before checking whether there is a cached <see cref="CILMetaData"/> for given resource, by <see cref="AbstractCILMetaDataLoader{TDictionary}.GetOrLoadMetaData(string)" /> method.
      /// </summary>
      /// <param name="resource">The given textual resource.</param>
      /// <returns>The sanitized resource.</returns>
      /// <remarks>
      /// In file-oriented loader, this usually means returning the value of <see cref="M:System.IO.Path.GetFullPath(System.String)"/>.
      /// </remarks>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.SanitizeResource(string)"/>
      String SanitizeResource( String resource );

      /// <summary>
      /// This method will be used when searching for referenced assembly or module when performing resolving (<see cref="ResolveMetaData(CILMetaData)"/>).
      /// </summary>
      /// <param name="resource">The resource to check.</param>
      /// <returns><c>true</c> if the resource is valid; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// In file-oriented loader, this usually means returning the value of <see cref="M:System.IO.File.Exists(System.String)"/>
      /// </remarks>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.IsValidResource(string)"/>
      Boolean IsValidResource( String resource );

      /// <summary>
      /// This method should transform the given textual resource into a byte stream.
      /// </summary>
      /// <param name="resource">The textual resource.</param>
      /// <returns>The <see cref="Stream"/> for the <paramref name="resource"/>.</returns>
      /// <remarks>
      /// In file-oriented loader, this usually means returning the value of <see cref="M:System.IO.File.Open(System.String, System.IO.FileMode, System.IO.FileAccess, System.IO.FileShare)"/>
      /// </remarks>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.GetStreamFor(string)"/>
      Stream GetStreamFor( String resource );

      /// <summary>
      /// This method should return all possible (valid or not valid) textual resources for a module reference occurring in a <see cref="CILMetaData"/>.
      /// </summary>
      /// <param name="thisMetaDataResource">The textual resource for this <see cref="CILMetaData"/>.</param>
      /// <param name="thisMetaData">This <see cref="CILMetaData"/> instance.</param>
      /// <param name="moduleReferenceName">The name of the module reference, that <paramref name="thisMetaData"/> references.</param>
      /// <returns>The enumerable for all possible (valid or not valid) resource for a module reference.</returns>
      /// <remarks>
      /// This method is used during resolving of <see cref="CILMetaData"/> in <see cref="ResolveMetaData(CILMetaData)"/>.
      /// </remarks>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.GetPossibleResourcesForModuleReference(string, CILMetaData, string)"/>
      IEnumerable<String> GetPossibleResourcesForModuleReference( String thisMetaDataResource, CILMetaData thisMetaData, String moduleReferenceName );

      /// <summary>
      /// This method should return all possible (valid or not valid) textual resources for an assembly reference occurring in a <see cref="CILMetaData"/>.
      /// </summary>
      /// <param name="thisMetaDataResource">The textual resource for this <see cref="CILMetaData"/>.</param>
      /// <param name="thisMetaData">This <see cref="CILMetaData"/> instance.</param>
      /// <param name="assemblyRefInfo">The assembly reference information, either from <see cref="AssemblyReference"/> or from textual type name. Will be <c>null</c> if the assembly information could not be parsed from textual type name.</param>
      /// <param name="unparsedAssemblyName">This will be non-<c>null</c> only when the textual type name contained assembly name, which was unparseable.</param>
      /// <returns>The enumerable for all possible (valid or not valid) textual resources for an assembly reference.</returns>
      /// <remarks>
      /// This method is used during resolving of <see cref="CILMetaData"/> in <see cref="ResolveMetaData(CILMetaData)"/>.
      /// </remarks>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}GetPossibleResourcesForAssemblyReference(string, CILMetaData, AssemblyInformationForResolving, string)"/>
      IEnumerable<String> GetPossibleResourcesForAssemblyReference( String thisMetaDataResource, CILMetaData thisMetaData, AssemblyInformationForResolving assemblyRefInfo, String unparsedAssemblyName );


      /// <summary>
      /// Gets possibly cached value of <see cref="TargetFrameworkInfo"/> for a given <see cref="CILMetaData"/>.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/>.</param>
      /// <returns>The <see cref="TargetFrameworkInfo"/> for given <see cref="CILMetaData"/>, or <c>null</c> if <see cref="TargetFrameworkInfo"/> could not be deduced. Should also return <c>null</c> if <paramref name="md"/> is <c>null</c>.</returns>
      /// <remarks>
      /// Note that if the <see cref="CILMetaData"/> is modified after calling this method in such way that it affects is <see cref="TargetFrameworkInfo"/> (constructed by e.g. <see cref="E_CILPhysical.TryGetTargetFrameworkInformation(CILMetaData, out TargetFrameworkInfo, MetaDataResolver)"/> method), this cache will not be updated.
      /// </remarks>
      /// <seealso cref="TargetFrameworkInfo"/>
      /// <seealso cref="E_CILPhysical.TryGetTargetFrameworkInformation(CILMetaData, out TargetFrameworkInfo, MetaDataResolver)"/>
      TargetFrameworkInfo GetOrAddTargetFrameworkInfoFor( CILMetaData md );

      /// <summary>
      /// Given a <see cref="TargetFrameworkInfo"/>, tries to resolve textual resource where the target framework assemblies should be located.
      /// </summary>
      /// <param name="targetFW">The <see cref="TargetFrameworkInfo"/>.</param>
      /// <returns>The textual resource where the target framework assemblies should be located. Will return <c>null</c> if <paramref name="targetFW"/> is <c>null</c> or if its profile information is invalid.</returns>
      /// <remarks>
      /// In file-oriented <see cref="CILMetaDataLoaderResourceCallbacks"/>, this should return directory where target framework assemblies may be found.
      /// </remarks>
      /// <seealso cref="TargetFrameworkInfo"/>
      String GetTargetFrameworkPathForFrameworkInfo( TargetFrameworkInfo targetFW );

      /// <summary>
      /// Enumerates all textual resources for all assemblies in a given target framework.
      /// </summary>
      /// <param name="targetFW">The <see cref="TargetFrameworkInfo"/>.</param>
      /// <returns>Enumerable of all textual resources for all assemblies in a given target framework.</returns>
      IEnumerable<String> GetAssemblyResourcesForFramework( TargetFrameworkInfo targetFW );
   }

   /// <summary>
   /// This class extends the <see cref="AbstractCILMetaDataLoader{TDictionary}"/> and implements <see cref="CILMetaDataLoaderWithCallbacks"/>, so that the required abstract methods of <see cref="AbstractCILMetaDataLoader{TDictionary}"/> will be call-throughs to the methods of <see cref="CILMetaDataLoaderResourceCallbacks"/>.
   /// </summary>
   /// <typeparam name="TDictionary">The type of dictionary to cache instances of <see cref="CILMetaData"/>.</typeparam>
   public abstract class CILMetaDataLoaderWithCallbacks<TDictionary> : AbstractCILMetaDataLoader<TDictionary>, CILMetaDataLoaderWithCallbacks
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
         CILMetaDataLoaderResourceCallbacks resourceCallbacks
         )
         : base( dictionary, cryptoCallbacks, readingArgsFactory )
      {
         ArgumentValidator.ValidateNotNull( "Resource callbacks", resourceCallbacks );

         this.LoaderCallbacks = resourceCallbacks;
      }

      /// <inheritdoc />
      public CILMetaDataLoaderResourceCallbacks LoaderCallbacks { get; }

      /// <summary>
      /// The implementation of this method is delegated to <see cref="CILMetaDataLoaderResourceCallbacks.SanitizeResource(string)"/>.
      /// </summary>
      /// <param name="resource">The resource to sanitize.</param>
      /// <returns>Result of <see cref="CILMetaDataLoaderResourceCallbacks.SanitizeResource(string)"/>.</returns>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.SanitizeResource(string)"/>
      protected override String SanitizeResource( String resource )
      {
         return this.LoaderCallbacks.SanitizeResource( resource );
      }

      /// <summary>
      /// The implementation of this method is delegated to <see cref="CILMetaDataLoaderResourceCallbacks.IsValidResource(string)"/>.
      /// </summary>
      /// <param name="resource">The resource to check.</param>
      /// <returns>Result of <see cref="CILMetaDataLoaderResourceCallbacks.IsValidResource(string)"/>.</returns>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.IsValidResource(string)"/>
      protected override Boolean IsValidResource( String resource )
      {
         return this.LoaderCallbacks.IsValidResource( resource );
      }

      /// <summary>
      /// The implementation of this method is delegated to <see cref="CILMetaDataLoaderResourceCallbacks.GetStreamFor(string)"/>.
      /// </summary>
      /// <param name="resource">The textual resource.</param>
      /// <returns>Result of <see cref="CILMetaDataLoaderResourceCallbacks.GetStreamFor(string)"/>.</returns>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.GetStreamFor(string)"/>
      protected override Stream GetStreamFor( String resource )
      {
         return this.LoaderCallbacks.GetStreamFor( resource );
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
         return this.LoaderCallbacks.GetPossibleResourcesForModuleReference( thisMetaDataResource, thisMetaData, moduleReferenceName );
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
         return this.LoaderCallbacks.GetPossibleResourcesForAssemblyReference( thisMetaDataResource, thisMetaData, assemblyRefInfo, unparsedAssemblyName );
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
         CILMetaDataLoaderResourceCallbacks resourceCallbacks
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
      /// This method simply calls <see cref="E_CILPhysical.ResolveEverything(MetaDataResolver, CILMetaData)"/>.
      /// </summary>
      /// <param name="resolver">The <see cref="MetaDataResolver"/>.</param>
      /// <param name="metaData">The <see cref="CILMetaData"/>.</param>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.PerformResolving"/>
      protected override void PerformResolving( MetaDataResolver resolver, CILMetaData metaData )
      {
         resolver.ResolveEverything( metaData );
      }

      /// <summary>
      /// Returns <c>false</c>.
      /// </summary>
      /// <value>The <c>false</c>.</value>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.IsSupportingConcurrency"/>
      protected override Boolean IsSupportingConcurrency
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
         CILMetaDataLoaderResourceCallbacks resourceCallbacks
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
      /// This method locks the given <see cref="MetaDataResolver"/>, and then calls <see cref="E_CILPhysical.ResolveEverything(MetaDataResolver, CILMetaData)"/>.
      /// </summary>
      /// <param name="resolver">The <see cref="MetaDataResolver"/>.</param>
      /// <param name="metaData">The <see cref="CILMetaData"/>.</param>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.PerformResolving"/>
      /// <remarks>
      /// The locking is done in case an attempt is made to resolve same <see cref="CILMetaData"/> concurrently, since <see cref="MetaDataResolver"/> itself does not support concurrency.
      /// </remarks>
      protected override void PerformResolving( MetaDataResolver resolver, CILMetaData metaData )
      {
         // In case we are trying to resolve same module concurrently
         lock ( resolver )
         {
            resolver.ResolveEverything( metaData );
         }
      }

      /// <summary>
      /// Returns <c>true</c>.
      /// </summary>
      /// <value>The <c>true</c>.</value>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.IsSupportingConcurrency"/>
      protected override Boolean IsSupportingConcurrency
      {
         get
         {
            return true;
         }
      }
   }
}

public static partial class E_CILPhysical
{
   /// <summary>
   /// Helper method to load <see cref="CILMetaData"/> and resolve 
   /// </summary>
   /// <param name="loader">This <see cref="CILMetaDataLoader"/>.</param>
   /// <param name="resource">The textual resource.</param>
   /// <returns>The loaded and resolved <see cref="CILMetaData"/>.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="CILMetaDataLoader"/> is <c>null</c>.</exception>
   /// <remarks>
   /// The <see cref="CILMetaDataLoader.ResolveMetaData(CILMetaData)"/> method will be called only if <see cref="CILMetaDataLoader.IsResourceCached(string)"/> returns <c>false</c> before obtaining the <see cref="CILMetaData"/> with <see cref="CILMetaDataLoader.GetOrLoadMetaData(string)"/>.
   /// </remarks>
   public static CILMetaData LoadAndResolve( this CILMetaDataLoader loader, String resource )
   {
      var existed = loader.IsResourceCached( resource );
      var retVal = loader.GetOrLoadMetaData( resource );
      if ( !existed )
      {
         loader.ResolveMetaData( retVal );
      }
      return retVal;
   }

   /// <summary>
   /// Helper method to get the textual resource containing target framework assemblies, where the target framework information is extracted from given <see cref="CILMetaData"/>.
   /// </summary>
   /// <param name="cb">The <see cref="CILMetaDataLoaderResourceCallbacks"/>.</param>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <returns>The textual resource containing target framework assemblies for given <see cref="CILMetaData"/>.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="CILMetaDataLoaderResourceCallbacks"/> is <c>null</c>.</exception>
   public static String GetTargetFrameworkPathFor( this CILMetaDataLoaderResourceCallbacks cb, CILMetaData md )
   {
      return cb.GetTargetFrameworkPathForFrameworkInfo( cb.GetOrAddTargetFrameworkInfoFor( md ) );
   }
}