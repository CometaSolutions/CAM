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
      /// Gets or loads the <see cref="CILMetaData"/> from the given resource (e.g. file path).
      /// </summary>
      /// <param name="resource">The textual resource identifier.</param>
      /// <returns>The cached or loaded instance of <see cref="CILMetaData"/>. If the instance is loaded, it is then cached for this <paramref name="resource"/>.</returns>
      /// <exception cref="MetaDataLoadException">If <see cref="CILMetaData"/> will need to be loaded, and something goes wrong during loading. One should examine the <see cref="Exception.InnerException"/> property of the catched <see cref="MetaDataLoadException"/> to further investigate what went wrong.</exception>
      CILMetaData GetOrLoadMetaData( String resource );

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
   public abstract class AbstractCILMetaDataLoader<TDictionary> : AbstractDisposable, CILMetaDataLoader
      where TDictionary : class, IDictionary<String, CILMetaData>
   {
      private readonly TDictionary _modules;
      private readonly Dictionary<CILMetaData, TModuleInfo> _moduleInfos;
      private readonly Lazy<HashStreamInfo> _hashStream;
      private readonly ReadingArgumentsFactoryDelegate _readingArgumentsFactory;

      public AbstractCILMetaDataLoader(
         TDictionary metadatas,
         CryptoCallbacks cryptoCallbacks,
         ReadingArgumentsFactoryDelegate readingArgsFactory
         )
      {
         ArgumentValidator.ValidateNotNull( "Modules", metadatas );

         this._modules = metadatas;
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
               .GetPossibleResourcesForAssemblyReference( thisResource, e.ThisMetaData, e.ExistingAssemblyInformation, e.UnparsedAssemblyName )
               .Where( r => this.IsValidResource( r ) )
               .Select( r => this.GetOrLoadMetaData( r, thisResource ) )
               .Where( md => md.AssemblyDefinitions.GetOrNull( 0 )?.IsMatch( e.ExistingAssemblyInformation, false, this.ComputePublicKeyTokenOrNull ) ?? false )
               .FirstOrDefault();
         }
      }

      /// <inheritdoc />
      public CILMetaData GetOrLoadMetaData( String resource )
      {
         return this.GetOrLoadMetaData( resource, null );
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
      protected TDictionary Dictionary
      {
         get
         {
            return this._modules;
         }

      }

      protected abstract Boolean IsSupportingConcurrency { get; }

      // Something like Path.GetFullPath(..)
      protected abstract String SanitizeResource( String resource );

      protected abstract Boolean IsValidResource( String resource );

      protected abstract Stream GetStreamFor( String resource );

      protected abstract IEnumerable<String> GetPossibleResourcesForModuleReference( String thisModulePath, CILMetaData thisMetaData, String moduleReferenceName );

      protected abstract IEnumerable<String> GetPossibleResourcesForAssemblyReference( String thisModulePath, CILMetaData thisMetaData, AssemblyInformationForResolving assemblyRefInfo, String unparsedAssemblyName );

      protected abstract CILMetaData GetOrAddFromDictionary( String resource, Func<String, CILMetaData> factory );

      protected abstract void PerformResolving( MetaDataResolver resolver, CILMetaData metaData );


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

         var retVal = this.GetOrAddFromDictionary( resource, res =>
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

   public interface CILMetaDataLoaderWithCallbacks : CILMetaDataLoader
   {
      CILMetaDataLoaderResourceCallbacks LoaderCallbacks { get; }
   }

   public interface CILMetaDataLoaderResourceCallbacks
   {
      String SanitizeResource( String resource );
      Boolean IsValidResource( String resource );
      Stream GetStreamFor( String resource );
      IEnumerable<String> GetPossibleResourcesForModuleReference( String thisModulePath, CILMetaData thisMetaData, String moduleReferenceName );
      IEnumerable<String> GetPossibleResourcesForAssemblyReference( String thisModulePath, CILMetaData thisMetaData, AssemblyInformationForResolving assemblyRefInfo, String unparsedAssemblyName );
      TargetFrameworkInfo GetTargetFrameworkInfoFor( CILMetaData md );
      String GetTargetFrameworkPathForFrameworkInfo( TargetFrameworkInfo targetFW );
      IEnumerable<String> GetAssemblyResourcesForFramework( TargetFrameworkInfo targetFW );
   }

   public abstract class CILMetaDataLoaderWithCallbacks<TDictionary> : AbstractCILMetaDataLoader<TDictionary>, CILMetaDataLoaderWithCallbacks
      where TDictionary : class, IDictionary<String, CILMetaData>
   {
      private readonly CILMetaDataLoaderResourceCallbacks _resourceCallbacks;

      public CILMetaDataLoaderWithCallbacks(
         TDictionary dictionary,
         CryptoCallbacks cryptoCallbacks,
         ReadingArgumentsFactoryDelegate readingArgsFactory,
         CILMetaDataLoaderResourceCallbacks resourceCallbacks
         )
         : base( dictionary, cryptoCallbacks, readingArgsFactory )
      {
         ArgumentValidator.ValidateNotNull( "Resource callbacks", resourceCallbacks );

         this._resourceCallbacks = resourceCallbacks;
      }

      public CILMetaDataLoaderResourceCallbacks LoaderCallbacks
      {
         get
         {
            return this._resourceCallbacks;
         }
      }

      protected override String SanitizeResource( String resource )
      {
         return this._resourceCallbacks.SanitizeResource( resource );
      }

      protected override Boolean IsValidResource( String resource )
      {
         return this._resourceCallbacks.IsValidResource( resource );
      }

      protected override Stream GetStreamFor( String resource )
      {
         return this._resourceCallbacks.GetStreamFor( resource );
      }

      protected override IEnumerable<String> GetPossibleResourcesForModuleReference( String thisModulePath, CILMetaData thisMetaData, String moduleReferenceName )
      {
         return this._resourceCallbacks.GetPossibleResourcesForModuleReference( thisModulePath, thisMetaData, moduleReferenceName );
      }

      protected override IEnumerable<String> GetPossibleResourcesForAssemblyReference( String thisModulePath, CILMetaData thisMetaData, AssemblyInformationForResolving assemblyRefInfo, String unparsedAssemblyName )
      {
         return this._resourceCallbacks.GetPossibleResourcesForAssemblyReference( thisModulePath, thisMetaData, assemblyRefInfo, unparsedAssemblyName );
      }

   }

   public class CILMetaDataLoaderNotThreadSafe : CILMetaDataLoaderWithCallbacks<Dictionary<String, CILMetaData>>
   {
      public CILMetaDataLoaderNotThreadSafe(
         CryptoCallbacks cryptoCallbacks,
         ReadingArgumentsFactoryDelegate readingArgsFactory,
         CILMetaDataLoaderResourceCallbacks resourceCallbacks
         )
         : base( new Dictionary<String, CILMetaData>(), cryptoCallbacks, readingArgsFactory, resourceCallbacks )
      {

      }

      protected override CILMetaData GetOrAddFromDictionary( String resource, Func<String, CILMetaData> factory )
      {
         return this.Dictionary.GetOrAdd_NotThreadSafe( resource, factory );
      }

      protected override void PerformResolving( MetaDataResolver resolver, CILMetaData metaData )
      {
         resolver.ResolveEverything( metaData );
      }

      protected override Boolean IsSupportingConcurrency
      {
         get
         {
            return false;
         }
      }
   }

   public class CILMetaDataLoaderThreadSafeSimple : CILMetaDataLoaderWithCallbacks<Dictionary<String, CILMetaData>>
   {

      public CILMetaDataLoaderThreadSafeSimple(
         CryptoCallbacks cryptoCallbacks,
         ReadingArgumentsFactoryDelegate readingArgsFactory,
         CILMetaDataLoaderResourceCallbacks resourceCallbacks
         )
         : base( new Dictionary<String, CILMetaData>(), cryptoCallbacks, readingArgsFactory, resourceCallbacks )
      {

      }

      protected override CILMetaData GetOrAddFromDictionary( String resource, Func<String, CILMetaData> factory )
      {
         return this.Dictionary.GetOrAdd_WithLock( resource, factory );
      }

      protected override void PerformResolving( MetaDataResolver resolver, CILMetaData metaData )
      {
         // In case we are trying to resolve same module concurrently
         lock ( resolver )
         {
            resolver.ResolveEverything( metaData );
         }
      }

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
   public static CILMetaData LoadAndResolve( this CILMetaDataLoader loader, String resource )
   {
      var retVal = loader.GetOrLoadMetaData( resource );
      loader.ResolveMetaData( retVal );
      return retVal;
   }

   public static String GetTargetFrameworkPathFor( this CILMetaDataLoaderResourceCallbacks cb, CILMetaData md )
   {
      return cb.GetTargetFrameworkPathForFrameworkInfo( cb.GetTargetFrameworkInfoFor( md ) );
   }
}