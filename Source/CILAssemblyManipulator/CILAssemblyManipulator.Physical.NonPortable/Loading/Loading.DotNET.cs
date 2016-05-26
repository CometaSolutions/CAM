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
using CILAssemblyManipulator.Physical.Loading;
using CommonUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO
{
   /// <summary>
   /// This class implements <see cref="CILMetaDataLoaderResourceCallbacks"/> in such way that it works with textual resources acting as file paths.
   /// </summary>
   public sealed class CILMetaDataLoaderResourceCallbacksForFiles : CILMetaDataBinaryLoaderResourceCallbacks
   {
      private readonly IDictionary<CILMetaData, TargetFrameworkInfo> _targetFrameworks;

      /// <summary>
      /// Creates a new instance of <see cref="CILMetaDataLoaderResourceCallbacksForFiles"/> with given optional base path for reference assemblies, and given optional base path to use when resolving non-rooted file paths.
      /// </summary>
      /// <param name="referenceAssemblyBasePath">The optional base path for directory containing well-known directory structure for reference assemblies for various target frameworks. If not supplied, the return value of <see cref="GetDefaultReferenceAssemblyPath"/> will be used.</param>
      /// <param name="basePath">The optional path to use when resolving non-rooted file paths. If not supplied, then <see cref="Environment.CurrentDirectory"/> will be used.</param>
      public CILMetaDataLoaderResourceCallbacksForFiles( String referenceAssemblyBasePath = null, String basePath = null )
      {
         this.TargetFrameworkBasePath = String.IsNullOrEmpty( referenceAssemblyBasePath ) ? GetDefaultReferenceAssemblyPath() : referenceAssemblyBasePath;
         this.BasePath = String.IsNullOrEmpty( basePath ) ? Environment.CurrentDirectory : basePath;
         this._targetFrameworks = new Dictionary<CILMetaData, TargetFrameworkInfo>( ReferenceEqualityComparer<CILMetaData>.ReferenceBasedComparer );
      }

      /// <summary>
      /// Implements the <see cref="CILMetaDataLoaderResourceCallbacks.SanitizeResource"/> method so that the value of <see cref="Path.GetFullPath(string)"/> is returned for given <paramref name="resource"/>.
      /// If <paramref name="resource"/> is not rooted, then the <see cref="BasePath"/> is combined with <paramref name="resource"/> first, using <see cref="Path.Combine(string, string)"/> method.
      /// </summary>
      /// <param name="resource">The file path.</param>
      /// <returns>Expanded and rooted file path.</returns>
      public String SanitizeResource( String resource )
      {
         return Path.GetFullPath( Path.IsPathRooted( resource ) ? resource : Path.Combine( this.BasePath, resource ) );
      }

      /// <summary>
      /// Implements the <see cref="CILMetaDataLoaderResourceCallbacks.IsValidResource"/> method so that it checks whether file exists at given path.
      /// </summary>
      /// <param name="resource">The file path.</param>
      /// <returns><c>true</c>, if file exists at given path; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// The <see cref="SanitizeResource"/> method is *not* invoked for given <paramref name="resource"/>.
      /// </remarks>
      public Boolean IsValidResource( String resource )
      {
         return File.Exists( resource );
      }

      /// <summary>
      /// Implements the <see cref="CILMetaDataBinaryLoaderResourceCallbacks.GetStreamFor"/> method so that it returns read-only stream for given file path.
      /// The <see cref="FileShare.Read"/> flag is used when opening the file.
      /// </summary>
      /// <param name="resource">The file path.</param>
      /// <returns>The read-only <see cref="Stream"/> for given file path.</returns>
      /// <remarks>
      /// The <see cref="SanitizeResource"/> method is *not* invoked for given <paramref name="resource"/>.
      /// All exceptions from <see cref="File.Open(string, FileMode, FileAccess, FileShare)"/> method are propagated directly from this method, and are not catched.
      /// </remarks>
      public Stream GetStreamFor( String resource )
      {
         return File.Open( resource, FileMode.Open, FileAccess.Read, FileShare.Read );
      }

      /// <summary>
      /// Implements the <see cref="CILMetaDataLoaderResourceCallbacks.GetPossibleResourcesForModuleReference"/> so that it returns possible file paths for a module reference within a <see cref="CILMetaData"/>.
      /// </summary>
      /// <param name="thisModulePath">The path for <see cref="CILMetaData"/> containing the module reference.</param>
      /// <param name="thisMetaData">The instance of <see cref="CILMetaData"/> containing the module reference.</param>
      /// <param name="moduleReferenceName">The module reference name.</param>
      /// <returns>The enumerable of possible file paths for given module reference.</returns>
      /// <remarks>
      /// The <see cref="SanitizeResource"/> and <see cref="IsValidResource"/> methods are *not* called on items of the returned enumerable.
      /// </remarks>
      public IEnumerable<String> GetPossibleResourcesForModuleReference( String thisModulePath, CILMetaData thisMetaData, String moduleReferenceName )
      {
         return this.FilterPossibleResources( thisModulePath, this.GetAllPossibleResourcesForModuleReference( thisModulePath, thisMetaData, moduleReferenceName ) );
      }

      /// <summary>
      /// Implements the <see cref="CILMetaDataLoaderResourceCallbacks.GetPossibleResourcesForAssemblyReference"/> so that it returns file paths for an assembly reference withing a <see cref="CILMetaData"/>.
      /// </summary>
      /// <param name="thisModulePath">The path for <see cref="CILMetaData"/> containing the assembly reference.</param>
      /// <param name="thisMetaData">The instance of <see cref="CILMetaData"/> containing the assembly reference.</param>
      /// <param name="assemblyRefInfo">The assembly reference information, either from <see cref="AssemblyReference"/> or from textual type name. Will be <c>null</c> if the assembly information could not be parsed from textual type name.</param>
      /// <param name="unparsedAssemblyName">This will be non-<c>null</c> only when the textual type name contained assembly name, which was unparseable.</param>
      /// <returns>The enumerable of possible file paths for given assembly reference.</returns>
      /// <remarks>
      /// The <see cref="SanitizeResource"/> and <see cref="IsValidResource"/> methods are *not* called on items of the returned enumerable.
      /// </remarks>
      public IEnumerable<String> GetPossibleResourcesForAssemblyReference( String thisModulePath, CILMetaData thisMetaData, AssemblyInformationForResolving assemblyRefInfo, string unparsedAssemblyName )
      {
         return this.FilterPossibleResources( thisModulePath, this.GetAllPossibleResourcesForAssemblyReference( thisModulePath, thisMetaData, assemblyRefInfo, unparsedAssemblyName ) );
      }

      /// <summary>
      /// Gets the base path specified for this <see cref="CILMetaDataLoaderResourceCallbacksForFiles"/>.
      /// This base path will be used when transforming non-rooted filepaths into rooted file paths by <see cref="SanitizeResource"/> method.
      /// </summary>
      /// <value>The base path specified for this <see cref="CILMetaDataLoaderResourceCallbacksForFiles"/>.</value>
      public String BasePath { get; }

      private IEnumerable<String> GetAllPossibleResourcesForModuleReference( String thisModulePath, CILMetaData thisMetaData, String moduleReferenceName )
      {
         if ( !moduleReferenceName.EndsWith( ".dll" ) )
         {
            moduleReferenceName += ".dll";
         }

         if ( !String.IsNullOrEmpty( thisModulePath ) )
         {
            var retVal = Path.Combine(
               Path.GetDirectoryName( thisModulePath ),
               moduleReferenceName
               );

            yield return retVal;
         }
      }

      private IEnumerable<String> GetAllPossibleResourcesForAssemblyReference( String thisModulePath, CILMetaData thisMetaData, AssemblyInformationForResolving assemblyRefInfo, string unparsedAssemblyName )
      {
         // TODO need to emulate behaviour of .dll.config file as well!

         // Process only those string references which are successfully parsed as assembly names
         //if ( assemblyRefInfo != null )
         //{
         var assRefName = assemblyRefInfo?.AssemblyInformation?.Name;
         if ( !String.IsNullOrEmpty( assRefName ) )
         {
            var path = Path.GetDirectoryName( thisModulePath );
            if ( !String.IsNullOrEmpty( path ) )
            {

               // First, try lookup in same folder
               yield return Path.Combine( path, assRefName + ".dll" );
               yield return Path.Combine( path, assRefName + ".exe" );
               yield return Path.Combine( path, assRefName + ".winmd" );
               // TODO more extensions?
            }

            // Then, try lookup in target framework directory, if we can parse target framework attribute
            path = this.GetTargetFrameworkPathFor( thisMetaData );
            if ( !String.IsNullOrEmpty( path ) )
            {
               yield return Path.Combine( path, assRefName + ".dll" );
            }
         }
         //}
      }

      private IEnumerable<String> FilterPossibleResources( String thisModulePath, IEnumerable<String> allPossibleResources )
      {
         // Don't return same resource, which might cause nasty infinite loops elsewhere
         return allPossibleResources.Where( r => !String.Equals( thisModulePath, r ) ); // TODO path comparison (case-(in)sensitive)
      }

      /// <inheritdoc />
      public TargetFrameworkInfo GetOrAddTargetFrameworkInfoFor( CILMetaData md )
      {
         // TODO consider changing this to extension method
         // Since if we change target framework info attribute for 'md', the cache will contain invalid information...
         return md == null ? null : this._targetFrameworks.GetOrAdd_WithLock( md, thisMD =>
         {
            TargetFrameworkInfo fwInfo;
            return thisMD.TryGetTargetFrameworkInformation( out fwInfo ) ? fwInfo : null;
         } );
      }

      /// <summary>
      /// Given a <see cref="TargetFrameworkInfo"/>, will try resolve the directory containing assemblies for target framework represented by <see cref="TargetFrameworkInfo"/>.
      /// </summary>
      /// <param name="targetFW">The <see cref="TargetFrameworkInfo"/> representing target framework.</param>
      /// <returns>The non-<c>null</c> and non-empty path for directory containing target framework assemblies, or <c>null</c> if resolving fails.</returns>
      /// <remarks>
      /// The <see cref="TargetFrameworkBasePath"/> property will be used in resolving process.
      /// If <paramref name="targetFW"/> is not <c>null</c>, and its <see cref="TargetFrameworkInfo.Identifier"/> and <see cref="TargetFrameworkInfo.Version"/> properties are not <c>null</c> nor empty, then the <see cref="TargetFrameworkBasePath"/> is combined with <see cref="TargetFrameworkInfo.Identifier"/> and <see cref="TargetFrameworkInfo.Version"/> with <see cref="Path.Combine(string, string, string)"/> method.
      /// Then, if <see cref="TargetFrameworkInfo.Profile"/> is not <c>null</c> nor empty, it is further combined with the result using <see cref="Path.Combine(string, string)"/> method.
      /// </remarks>
      public String GetTargetFrameworkPathForFrameworkInfo( TargetFrameworkInfo targetFW )
      {
         String retVal;
         String id, v, p;
         if ( targetFW == null || String.IsNullOrEmpty( ( id = targetFW.Identifier ) ) || String.IsNullOrEmpty( ( v = targetFW.Version ) ) )
         {
            retVal = null;
         }
         else
         {
            retVal = Path.Combine( this.TargetFrameworkBasePath, id, v );
            if ( !String.IsNullOrEmpty( ( p = targetFW.Profile ) ) )
            {
               retVal = Path.Combine( retVal, "Profile", p );
            }

         }

         return retVal;
      }

      /// <summary>
      /// Given a <see cref="TargetFrameworkInfo"/>, enumerates all assembly file names in target framework represented by given <see cref="TargetFrameworkInfo"/>.
      /// </summary>
      /// <param name="targetFW">The <see cref="TargetFrameworkInfo"/>.</param>
      /// <returns>Enumerable of all assembly file names in target framework represented by given <see cref="TargetFrameworkInfo"/>. May be empty.</returns>
      /// <remarks>
      /// The <see cref="Directory.EnumerateDirectories(string, string, SearchOption)"/> method is used to enumerate file names, with directory being result of <see cref="GetTargetFrameworkPathForFrameworkInfo"/>, search pattern <c>"*.dll"</c>, and search option <see cref="SearchOption.TopDirectoryOnly"/>.
      /// If <paramref name="targetFW"/> is <c>null</c>, an empty enumerable is returned.
      /// </remarks>
      public IEnumerable<String> GetAssemblyResourcesForFramework( TargetFrameworkInfo targetFW )
      {
         var targetFWPath = this.GetTargetFrameworkPathForFrameworkInfo( targetFW );
         return String.IsNullOrEmpty( targetFWPath ) ?
            Empty<String>.Enumerable :
            Directory.EnumerateFiles( targetFWPath, "*.dll", SearchOption.TopDirectoryOnly );
      }

      /// <summary>
      /// Gets the path for directory containing reference assemblies for various target frameworks.
      /// This path is used by <see cref="GetTargetFrameworkPathForFrameworkInfo"/> method when resolving directory containing assemblies for specific target framework.
      /// </summary>
      /// <value>The path for directory containing reference assemblies for various target frameworks.</value>
      public String TargetFrameworkBasePath { get; }

      /// <summary>
      /// This method is used by the <see cref="CILMetaDataLoaderResourceCallbacksForFiles(string, string)"/> constructor if the reference assembly base path is not specified.
      /// </summary>
      /// <returns>A default path, depending on the operating system this process runs in.</returns>
      /// <remarks>
      /// For <see cref="PlatformID.Unix"/>, this method returns <c>"/usr/lib/mono/xbuild-frameworks"</c>.
      /// For <see cref="PlatformID.MacOSX"/>, this method returns <c>"/Library/Frameworks/Mono.framework/External/xbuild-frameworks"</c>.
      /// For anything else, this method returns <c>"C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework"</c>.
      /// </remarks>
      public static String GetDefaultReferenceAssemblyPath()
      {
         switch ( Environment.OSVersion.Platform )
         {
            case PlatformID.Unix:
               return @"/usr/lib/mono/xbuild-frameworks";
            case PlatformID.MacOSX:
               return @"/Library/Frameworks/Mono.framework/External/xbuild-frameworks";
            default:
               return @"C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework";
         }
      }

   }

   /// <summary>
   /// This class extends the <see cref="CILMetaDataLoaderNotThreadSafe"/> so that <see cref="CryptoCallbacksDotNET"/> and <see cref="CILMetaDataLoaderResourceCallbacksForFiles"/> are used, if not supplied.
   /// </summary>
   public class CILMetaDataLoaderNotThreadSafeForFiles : CILMetaDataLoaderNotThreadSafe
   {
      /// <summary>
      /// Creates a new instance of <see cref="CILMetaDataLoaderNotThreadSafeForFiles"/>.
      /// </summary>
      /// <param name="readingArgsFactory">The optional callback to create <see cref="ReadingArguments"/>.</param>
      /// <param name="callbacks">The optional <see cref="CILMetaDataLoaderResourceCallbacksForFiles"/>. If not supplied, a new instance of <see cref="CILMetaDataLoaderResourceCallbacksForFiles"/> will be used.</param>
      public CILMetaDataLoaderNotThreadSafeForFiles(
         ReadingArgumentsFactoryDelegate readingArgsFactory = null,
         CILMetaDataLoaderResourceCallbacksForFiles callbacks = null
         )
         : base( readingArgsFactory, callbacks ?? new CILMetaDataLoaderResourceCallbacksForFiles() )
      {

      }

      /// <summary>
      /// This method calls <see cref="CILMetaDataIO.ReadModule"/> method to read the module from the stream.
      /// </summary>
      /// <param name="stream">The stream to read module from.</param>
      /// <param name="rArgs">The <see cref="ReadingArguments"/> to use.</param>
      /// <returns>The module, as returned by <see cref="CILMetaDataIO.ReadModule"/> method.</returns>
      protected override CILMetaData ReadModuleFromStream( Stream stream, ReadingArguments rArgs )
      {
         return stream.ReadModule( rArgs );
      }
   }

   /// <summary>
   /// This class extends <see cref="CILMetaDataLoaderThreadSafeSimple"/> so that <see cref="CryptoCallbacksDotNET"/> and <see cref="CILMetaDataLoaderResourceCallbacksForFiles"/> are used, if not supplied.
   /// </summary>
   public class CILMetaDataLoaderThreadSafeSimpleForFiles : CILMetaDataLoaderThreadSafeSimple
   {
      /// <summary>
      /// Creates a new instance of <see cref="CILMetaDataLoaderThreadSafeSimpleForFiles"/>.
      /// </summary>
      /// <param name="readingArgsFactory">The optional callback to create <see cref="ReadingArguments"/>.</param>
      /// <param name="callbacks">The optional <see cref="CILMetaDataLoaderResourceCallbacksForFiles"/>. If not supplied, a new instance of <see cref="CILMetaDataLoaderResourceCallbacksForFiles"/> will be used.</param>
      public CILMetaDataLoaderThreadSafeSimpleForFiles(
         ReadingArgumentsFactoryDelegate readingArgsFactory = null,
         CILMetaDataLoaderResourceCallbacksForFiles callbacks = null
         )
         : base( readingArgsFactory, callbacks ?? new CILMetaDataLoaderResourceCallbacksForFiles() )
      {

      }

      /// <summary>
      /// This method calls <see cref="CILMetaDataIO.ReadModule"/> method to read the module from the stream.
      /// </summary>
      /// <param name="stream">The stream to read module from.</param>
      /// <param name="rArgs">The <see cref="ReadingArguments"/> to use.</param>
      /// <returns>The module, as returned by <see cref="CILMetaDataIO.ReadModule"/> method.</returns>
      protected override CILMetaData ReadModuleFromStream( Stream stream, ReadingArguments rArgs )
      {
         return stream.ReadModule( rArgs );
      }
   }

   /// <summary>
   /// This class extends <see cref="CILMetaDataLoaderWithCallbacks{TDictionary}"/> so that <see cref="ConcurrentDictionary{TKey, TValue}"/> is used to cache <see cref="CILMetaData"/>s.
   /// </summary>
   public class CILMetaDataLoaderThreadSafeConcurrent : CILMetaDataLoaderWithCallbacks<ConcurrentDictionary<String, CILMetaData>>
   {
      /// <summary>
      /// Constructs this <see cref="CILMetaDataLoaderThreadSafeConcurrent"/> with given <see cref="CryptoCallbacks"/> for public key token computation, given <see cref="ReadingArgumentsFactoryDelegate"/> to create <see cref="ReadingArguments"/>, and given <see cref="CILMetaDataLoaderResourceCallbacks"/>.
      /// </summary>
      /// <param name="readingArgsFactory">The optional <see cref="ReadingArgumentsFactoryDelegate"/> to use to create <see cref="ReadingArguments"/>.</param>
      /// <param name="resourceCallbacks">The <see cref="CILMetaDataLoaderResourceCallbacks"/> to use in required methods implementing abstract methods of base class.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="resourceCallbacks"/> is <c>null</c>.</exception>

      public CILMetaDataLoaderThreadSafeConcurrent(
         ReadingArgumentsFactoryDelegate readingArgsFactory,
         CILMetaDataBinaryLoaderResourceCallbacks resourceCallbacks
         )
         : base( new ConcurrentDictionary<String, CILMetaData>(), readingArgsFactory, resourceCallbacks )
      {

      }

      /// <summary>
      /// Gets or adds from the <see cref="AbstractCILMetaDataLoader{TDictionary}.Cache"/> in a thread-safe way.
      /// </summary>
      /// <param name="resource">The resource (key).</param>
      /// <param name="factory">The factory to create value, if needed.</param>
      /// <returns>The existing or created <see cref="CILMetaData"/>.</returns>
      /// <remarks>
      /// This method uses <see cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, Func{TKey, TValue})"/> method to get or create the <see cref="CILMetaData"/>.
      /// </remarks>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.GetOrAddFromCache"/>
      protected override CILMetaData GetOrAddFromCache( String resource, Func<String, CILMetaData> factory )
      {
         return this.Cache.GetOrAdd( resource, factory );
      }

      /// <summary>
      /// Returns <c>true</c>.
      /// </summary>
      /// <value>The <c>true</c>.</value>
      /// <seealso cref="AbstractCILMetaDataLoader{TDictionary}.IsSupportingConcurrency"/>
      protected override sealed Boolean IsSupportingConcurrency
      {
         get
         {
            return true;
         }
      }

      /// <summary>
      /// This method locks the given <see cref="MetaDataResolver"/>, and then calls <see cref="CAMPhysicalR::E_CILPhysical.ResolveEverything"/>.
      /// </summary>
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
      /// This method calls <see cref="CILMetaDataIO.ReadModule"/> method to read the module from the stream.
      /// </summary>
      /// <param name="stream">The stream to read module from.</param>
      /// <param name="rArgs">The <see cref="ReadingArguments"/> to use.</param>
      /// <returns>The module, as returned by <see cref="CILMetaDataIO.ReadModule"/> method.</returns>
      protected override CILMetaData ReadModuleFromStream( Stream stream, ReadingArguments rArgs )
      {
         return stream.ReadModule( rArgs );
      }
   }

   /// <summary>
   /// This class extends <see cref="CILMetaDataLoaderThreadSafeConcurrent"/> so that <see cref="CryptoCallbacksDotNET"/> and <see cref="CILMetaDataLoaderResourceCallbacksForFiles"/> are used, if not supplied.
   /// </summary>
   public class CILMetaDataLoaderThreadSafeConcurrentForFiles : CILMetaDataLoaderThreadSafeConcurrent
   {
      /// <summary>
      /// Creates a new instance of <see cref="CILMetaDataLoaderThreadSafeConcurrentForFiles"/>.
      /// </summary>
      /// <param name="readingArgsFactory">The optional callback to create <see cref="ReadingArguments"/>.</param>
      /// <param name="callbacks">The optional <see cref="CILMetaDataLoaderResourceCallbacksForFiles"/>. If not supplied, a new instance of <see cref="CILMetaDataLoaderResourceCallbacksForFiles"/> will be used.</param>
      public CILMetaDataLoaderThreadSafeConcurrentForFiles(
         ReadingArgumentsFactoryDelegate readingArgsFactory = null,
         CILMetaDataLoaderResourceCallbacksForFiles callbacks = null
         )
         : base( readingArgsFactory, callbacks ?? new CILMetaDataLoaderResourceCallbacksForFiles() )
      {

      }
   }
}
#endif
