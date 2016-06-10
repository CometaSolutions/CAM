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
extern alias CAMPhysicalR;
using CAMPhysicalR;
using CAMPhysicalR::CILAssemblyManipulator.Physical.Resolving;

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.Crypto;
using UtilPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CILAssemblyManipulator.Physical.Loading;

namespace CILAssemblyManipulator.Physical.Loading
{

   /// <summary>
   /// This interface represents a object which caches instances of <see cref="CILMetaData"/> based on textual resource, e.g. file path.
   /// </summary>
   /// <remarks>
   /// This interface does not specify whether it is thread-safe or not, that depends on the class implementing this interface.
   /// The <see cref="T:CILAssemblyManipulator.Physical.Loading.CILMetaDataLoaderNotThreadSafeForFiles"/>, <see cref="T:CILAssemblyManipulator.Physical.Loading.CILMetaDataLoaderThreadSafeSimpleForFiles"/>, and <see cref="T:CILAssemblyManipulator.Physical.IO.CILMetaDataLoaderThreadSafeConcurrentForFiles"/> provide ready-to-use implementation of this interface in non-portable usage scenarios.
   /// The <see cref="T:CILAssemblyManipulator.Physical.Loading.CILMetaDataLoaderNotThreadSafe"/> and <see cref="T:CILAssemblyManipulator.Physical.Loading.CILMetaDataLoaderThreadSafeSimple"/> implement this interface in a way suitable for most of the portable usage scenarios.
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
      /// When the <see cref="CILMetaData"/> is loaded, some of its signatures are left to raw forms (e.g. <see cref="CustomAttributeDefinition.Signature"/>, for more info see <see cref="CAMPhysicalR::CILAssemblyManipulator.Physical.Meta.ResolvingProvider"/>).
      /// </summary>
      /// <param name="metaData">The <see cref="CILMetaData"/>, which should have been obtained through <see cref="GetOrLoadMetaData"/> method of this <see cref="CILMetaDataLoader"/>.</param>
      /// <returns><c>true</c> if <paramref name="metaData"/> was obtained through <see cref="GetOrLoadMetaData"/> method of this <see cref="CILMetaDataLoader"/>; <c>false</c> otherwise.</returns>
      Boolean ResolveMetaData( CILMetaData metaData );

      /// <summary>
      /// Gets the resource used to load the <see cref="CILMetaData"/>, or <c>null</c>.
      /// </summary>
      /// <param name="metaData">The <see cref="CILMetaData"/>, which should have been obtained through <see cref="GetOrLoadMetaData"/> method of this <see cref="CILMetaDataLoader"/>.</param>
      /// <returns>A textual resource used to load given <paramref name="metaData"/> if <paramref name="metaData"/> was obtained through <see cref="GetOrLoadMetaData"/> method of this <see cref="CILMetaDataLoader"/>; <c>null</c> otherwise.</returns>
      String GetResourceFor( CILMetaData metaData );
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
      private readonly Dictionary<CILMetaData, String> _moduleInfos;

      /// <summary>
      /// Constructs this <see cref="AbstractCILMetaDataLoader{TDictionary}"/> with given dictionary and <see cref="CryptoCallbacks"/> (for public key token computation).
      /// </summary>
      /// <param name="dictionary">The dictionary to cache <see cref="CILMetaData"/> instances.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="dictionary"/> is <c>null</c>.</exception>
      public AbstractCILMetaDataLoader(
         TDictionary dictionary
         )
      {
         ArgumentValidator.ValidateNotNull( "Modules", dictionary );

         this._modules = dictionary;
         this._moduleInfos = new Dictionary<CILMetaData, String>( ReferenceEqualityComparer<CILMetaData>.ReferenceBasedComparer );
      }

      private void _resolver_ModuleReferenceResolveEvent( Object sender, ModuleReferenceResolveEventArgs e )
      {
         String thisResource;
         if ( this._moduleInfos.TryGetValue( e.ThisMetaData, out thisResource ) )
         {
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
         String thisResource;
         if ( this._moduleInfos.TryGetValue( e.ThisMetaData, out thisResource ) )
         {
            e.ResolvedMetaData = this
               .GetPossibleResourcesForAssemblyReference( thisResource, e.ThisMetaData, e.AssemblyInformation, e.UnparsedAssemblyName )
               .Where( r => this.IsValidResource( r ) )
               .Select( r => this.GetOrLoadMetaData( r, thisResource ) )
               .Where( md => AssemblyReferenceMatcherExact.Match( md.AssemblyDefinitions.GetOrNull( 0 ), e.AssemblyInformation.AssemblyInformation, e.AssemblyInformation.IsFullPublicKey ? AssemblyFlags.PublicKey : AssemblyFlags.None ) ) // ?.IsMatch( e.AssemblyInformation, false ) ?? false )
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
         var retVal = this._moduleInfos.ContainsKey( metaData );

         if ( retVal )
         {
            this.PerformResolving( metaData );
         }

         return retVal;
      }

      /// <inheritdoc />
      public String GetResourceFor( CILMetaData metaData )
      {
         String moduleInfo;
         return this._moduleInfos.TryGetValue( metaData, out moduleInfo ) ?
            moduleInfo :
            null;
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
      /// The purpose of this method is to customize whether <see cref="M:E_UtilPack.GetOrAdd_NotThreadSafe{TKey, TValue}(IDictionary{TKey, TValue}, TKey, Func{TKey, TValue})"/>, <see cref="M:E_UtilPack.GetOrAdd_WithLock{TKey, TValue}(IDictionary{TKey, TValue}, TKey, Func{TKey, TValue}, object)"/>, or <see cref="M:System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, Func{TKey, TValue})"/> is used to obtain the value from the cache.
      /// </remarks>
      protected abstract CILMetaData GetOrAddFromCache( String resource, Func<String, CILMetaData> factory );

      /// <summary>
      /// This method is called by <see cref="ResolveMetaData(CILMetaData)"/> to actually perform resolving.
      /// </summary>
      /// <param name="metaData">The <see cref="CILMetaData"/> to resolve.</param>
      /// <remarks>
      /// The purpose of this method is to enable e.g. locking or some other function around/before/after resolving.
      /// It is assumed that this method will call <see cref="CAMPhysicalR::E_CILPhysical.ResolveEverything"/>.
      /// </remarks>
      /// <seealso cref="MetaDataResolver"/>
      /// <seealso cref="CAMPhysicalR::E_CILPhysical.ResolveEverything"/>
      protected abstract void PerformResolving( CILMetaData metaData );

      /// <summary>
      /// By default, does nothing.
      /// </summary>
      /// <param name="disposing">Whether this is called from <see cref="AbstractDisposable.Dispose()"/> method.</param>
      protected override void Dispose( Boolean disposing )
      {
         // Nothing to do.
      }

      private CILMetaData GetOrLoadMetaData( String resource, String pathForModuleBeingResolved )
      {
         resource = this.SanitizeResource( resource );
         ArgumentValidator.ValidateNotNull( "Resource", resource );

         var retVal = this.GetOrAddFromCache( resource, res =>
         {
            try
            {
               return this.GetMetaDataFromResource( res, pathForModuleBeingResolved );
            }
            catch ( Exception exc )
            {
               throw new MetaDataLoadException( "Error when loading CIL module from " + res + ".", exc );
            }

         } );

         Boolean added;
         if ( this.IsSupportingConcurrency )
         {
            this._moduleInfos.GetOrAdd_WithLock( retVal, md => this.ModuleInfoFactory( resource, md ), out added );
         }
         else
         {
            this._moduleInfos.GetOrAdd_NotThreadSafe( retVal, md => this.ModuleInfoFactory( resource, md ), out added );
         }

         if ( added )
         {
            // TODO possibly event? ModuleLoadedEvent
         }

         return retVal;
      }

      /// <summary>
      /// This method should return an instance of <see cref="CILMetaData"/> for given sanitized textual resource (e.g. file path).
      /// </summary>
      /// <param name="sanitizedResource">The sanitized textual resource.</param>
      /// <param name="pathForModuleBeingResolved">If the load</param>
      /// <returns>An instance of <see cref="CILMetaData"/> which represents the given <paramref name="sanitizedResource"/>.</returns>
      protected abstract CILMetaData GetMetaDataFromResource( String sanitizedResource, String pathForModuleBeingResolved );


      private String ModuleInfoFactory( String resource, CILMetaData md )
      {
         this.AfterModuleLoad( resource, md );
         var resolver = md.GetResolvingProvider().Resolver;
         resolver.AssemblyReferenceResolveEvent += this._resolver_AssemblyReferenceResolveEvent;
         resolver.ModuleReferenceResolveEvent += this._resolver_ModuleReferenceResolveEvent;
         return resource;
      }

      /// <summary>
      /// This method is called right after a new instance of <see cref="CILMetaData"/> has been added to <see cref="Cache"/>.
      /// </summary>
      /// <param name="resource">The sanitized textual resource.</param>
      /// <param name="md">The new instance of <see cref="CILMetaData"/>.</param>
      /// <remarks>
      /// This method implementation, by default, does nothing.
      /// If <see cref="IsSupportingConcurrency"/> returns <c>true</c> for this <see cref="AbstractCILMetaDataLoader{TDictionary}"/>, this method will be called inside a lock, meaning that any other concurrent requests to load the <see cref="CILMetaData"/> from the *same resource* will be waiting for this method to complete.
      /// Furthermore, during this method, even though the given <see cref="CILMetaData"/> is present in the <see cref="Cache"/> dictionary, methods <see cref="ResolveMetaData"/> and <see cref="GetResourceFor"/> will think that the <paramref name="md"/> is not present in this <see cref="AbstractCILMetaDataLoader{TDictionary}"/>.
      /// </remarks>
      protected virtual void AfterModuleLoad( String resource, CILMetaData md )
      {
         // Nothing to do.
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
      /// This method will be used when searching for referenced assembly or module when performing resolving (<see cref="CILMetaDataLoader.ResolveMetaData"/>).
      /// </summary>
      /// <param name="resource">The resource to check.</param>
      /// <returns><c>true</c> if the resource is valid; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// In file-oriented loader, this usually means returning the value of <see cref="M:System.IO.File.Exists(System.String)"/>
      /// </remarks>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.IsValidResource(string)"/>
      Boolean IsValidResource( String resource );

      /// <summary>
      /// This method should return all possible (valid or not valid) textual resources for a module reference occurring in a <see cref="CILMetaData"/>.
      /// </summary>
      /// <param name="thisMetaDataResource">The textual resource for this <see cref="CILMetaData"/>.</param>
      /// <param name="thisMetaData">This <see cref="CILMetaData"/> instance.</param>
      /// <param name="moduleReferenceName">The name of the module reference, that <paramref name="thisMetaData"/> references.</param>
      /// <returns>The enumerable for all possible (valid or not valid) resource for a module reference.</returns>
      /// <remarks>
      /// This method is used during resolving of <see cref="CILMetaData"/> in <see cref="CILMetaDataLoader.ResolveMetaData"/>.
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
      /// This method is used during resolving of <see cref="CILMetaData"/> in <see cref="CILMetaDataLoader.ResolveMetaData"/>.
      /// </remarks>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.GetPossibleResourcesForAssemblyReference(string, CILMetaData, AssemblyInformationForResolving, string)"/>
      IEnumerable<String> GetPossibleResourcesForAssemblyReference( String thisMetaDataResource, CILMetaData thisMetaData, AssemblyInformationForResolving assemblyRefInfo, String unparsedAssemblyName );

      /// <summary>
      /// Gets possibly cached value of <see cref="TargetFrameworkInfo"/> for a given <see cref="CILMetaData"/>.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/>.</param>
      /// <returns>The <see cref="TargetFrameworkInfo"/> for given <see cref="CILMetaData"/>, or <c>null</c> if <see cref="TargetFrameworkInfo"/> could not be deduced. Should also return <c>null</c> if <paramref name="md"/> is <c>null</c>.</returns>
      /// <remarks>
      /// Note that if the <see cref="CILMetaData"/> is modified after calling this method in such way that it affects is <see cref="TargetFrameworkInfo"/> (constructed by e.g. <see cref="CAMPhysicalR::E_CILPhysical.TryGetTargetFrameworkInformation"/> method), this cache will not be updated.
      /// </remarks>
      /// <seealso cref="TargetFrameworkInfo"/>
      /// <seealso cref="CAMPhysicalR::E_CILPhysical.TryGetTargetFrameworkInformation"/>
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

}

#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
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