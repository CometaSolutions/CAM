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

using CommonUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
   public sealed class CILMetaDataLoaderResourceCallbacksForFiles : CILMetaDataLoaderResourceCallbacks
   {
      private readonly String _fwBasePath;
      private readonly String _basePath;
      private readonly IDictionary<CILMetaData, TargetFrameworkInfo> _targetFrameworks;

      public CILMetaDataLoaderResourceCallbacksForFiles( String referenceAssemblyBasePath = null, String basePath = null )
      {
         this._fwBasePath = String.IsNullOrEmpty( referenceAssemblyBasePath ) ? GetDefaultReferenceAssemblyPath() : referenceAssemblyBasePath;
         this._basePath = String.IsNullOrEmpty( basePath ) ? Environment.CurrentDirectory : basePath;
         this._targetFrameworks = new Dictionary<CILMetaData, TargetFrameworkInfo>( ReferenceEqualityComparer<CILMetaData>.ReferenceBasedComparer );
      }

      public String SanitizeResource( String resource )
      {
         return Path.GetFullPath( Path.IsPathRooted( resource ) ? resource : Path.Combine( this._basePath, resource ) );
      }

      public Boolean IsValidResource( String resource )
      {
         return File.Exists( resource );
      }

      public Stream GetStreamFor( String resource )
      {
         return File.Open( resource, FileMode.Open, FileAccess.Read, FileShare.Read );
      }

      public IEnumerable<String> GetPossibleResourcesForModuleReference( String thisModulePath, CILMetaData thisMetaData, String moduleReferenceName )
      {
         return this.FilterPossibleResources( thisModulePath, this.GetAllPossibleResourcesForModuleReference( thisModulePath, thisMetaData, moduleReferenceName ) );
      }

      public IEnumerable<String> GetPossibleResourcesForAssemblyReference( String thisModulePath, CILMetaData thisMetaData, AssemblyInformationForResolving? assemblyRefInfo, string unparsedAssemblyName )
      {
         return this.FilterPossibleResources( thisModulePath, this.GetAllPossibleResourcesForAssemblyReference( thisModulePath, thisMetaData, assemblyRefInfo, unparsedAssemblyName ) );
      }

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

      private IEnumerable<String> GetAllPossibleResourcesForAssemblyReference( String thisModulePath, CILMetaData thisMetaData, AssemblyInformationForResolving? assemblyRefInfo, string unparsedAssemblyName )
      {
         // TODO need to emulate behaviour of .dll.config file as well!

         // Process only those string references which are successfully parsed as assembly names
         if ( assemblyRefInfo.HasValue )
         {
            var assRefName = assemblyRefInfo.Value.AssemblyInformation.Name;
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
      }

      private IEnumerable<String> FilterPossibleResources( String thisModulePath, IEnumerable<String> allPossibleResources )
      {
         // Don't return same resource, which might cause nasty infinite loops elsewhere
         return allPossibleResources.Where( r => !String.Equals( thisModulePath, r ) ); // TODO path comparison (case-(in)sensitive)
      }

      public TargetFrameworkInfo GetTargetFrameworkInfoFor( CILMetaData md )
      {
         // TODO consider changing this to extension method
         // Since if we change target framework info attribute for 'md', the cache will contain invalid information...
         return this._targetFrameworks.GetOrAdd_WithLock( md, thisMD =>
         {
            TargetFrameworkInfo fwInfo;
            return thisMD.TryGetTargetFrameworkInformation( out fwInfo ) ? fwInfo : null;
         } );
      }

      public String GetTargetFrameworkPathForFrameworkInfo( TargetFrameworkInfo targetFW )
      {
         String retVal;
         if ( targetFW != null )
         {
            retVal = Path.Combine( this._fwBasePath, targetFW.Identifier, targetFW.Version );
            if ( !String.IsNullOrEmpty( targetFW.Profile ) )
            {
               retVal = Path.Combine( retVal, "Profile", targetFW.Profile );
            }
         }
         else
         {
            retVal = null;
         }

         return retVal;
      }

      public IEnumerable<String> GetAssemblyResourcesForFramework( TargetFrameworkInfo targetFW )
      {
         var targetFWPath = this.GetTargetFrameworkPathForFrameworkInfo( targetFW );
         return String.IsNullOrEmpty( targetFWPath ) ?
            Empty<String>.Enumerable :
            Directory.EnumerateFiles( targetFWPath, "*.dll", SearchOption.TopDirectoryOnly );
      }

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

   public class CILMetaDataLoaderNotThreadSafeForFiles : CILMetaDataLoaderNotThreadSafe
   {
      public CILMetaDataLoaderNotThreadSafeForFiles( CryptoCallbacks crypto = null, CILMetaDataLoaderResourceCallbacksForFiles callbacks = null )
         : base( crypto ?? new CryptoCallbacksDotNET(), callbacks ?? new CILMetaDataLoaderResourceCallbacksForFiles() )
      {

      }
   }

   public class CILMetaDataLoaderThreadSafeSimpleForFiles : CILMetaDataLoaderThreadSafeSimple
   {
      public CILMetaDataLoaderThreadSafeSimpleForFiles( CryptoCallbacks crypto = null, CILMetaDataLoaderResourceCallbacksForFiles callbacks = null )
         : base( crypto ?? new CryptoCallbacksDotNET(), callbacks ?? new CILMetaDataLoaderResourceCallbacksForFiles() )
      {

      }

      protected override Boolean IsThreadSafe
      {
         get
         {
            return true;
         }
      }
   }

   public class CILMetaDataLoaderThreadSafeConcurrent : CILMetaDataLoaderWithCallbacks<ConcurrentDictionary<String, CILMetaData>>
   {
      public CILMetaDataLoaderThreadSafeConcurrent( CryptoCallbacks crypto, CILMetaDataLoaderResourceCallbacks callbacks )
         : base( new ConcurrentDictionary<String, CILMetaData>(), crypto, callbacks )
      {

      }

      protected override CILMetaData GetOrAddFromDictionary( String resource, Func<String, CILMetaData> factory )
      {
         return this.Dictionary.GetOrAdd( resource, factory );
      }

      protected override Boolean IsThreadSafe
      {
         get
         {
            return true;
         }
      }

      protected override void PerformResolving( MetaDataResolver resolver, CILMetaData metaData )
      {
         // In case we are trying to resolve same module concurrently
         lock ( resolver )
         {
            resolver.ResolveEverything( metaData );
         }
      }
   }

   public class CILMetaDataLoaderThreadSafeConcurrentForFiles : CILMetaDataLoaderThreadSafeConcurrent
   {
      public CILMetaDataLoaderThreadSafeConcurrentForFiles( CryptoCallbacks crypto = null, CILMetaDataLoaderResourceCallbacksForFiles callbacks = null )
         : base( crypto ?? new CryptoCallbacksDotNET(), callbacks ?? new CILMetaDataLoaderResourceCallbacksForFiles() )
      {

      }
   }




}
#endif
