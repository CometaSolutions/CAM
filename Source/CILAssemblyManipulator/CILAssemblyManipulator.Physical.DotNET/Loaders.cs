﻿using CommonUtils;
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.DotNET
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
         if ( !moduleReferenceName.EndsWith( ".dll" ) )
         {
            moduleReferenceName += ".dll";
         }
         yield return Path.Combine(
            Path.GetDirectoryName( thisModulePath ),
            moduleReferenceName
            );
      }

      public IEnumerable<String> GetPossibleResourcesForAssemblyReference( String thisModulePath, CILMetaData thisMetaData, AssemblyInformationForResolving? assemblyRefInfo, string unparsedAssemblyName )
      {
         // TODO need to emulate behaviour of .dll.config file as well!

         // Process only those string references which are successfully parsed as assembly names
         if ( assemblyRefInfo.HasValue )
         {
            var path = Path.GetDirectoryName( thisModulePath );
            var assRefName = assemblyRefInfo.Value.AssemblyInformation.Name;

            // First, try lookup in same folder
            yield return Path.Combine( path, assRefName + ".dll" );
            yield return Path.Combine( path, assRefName + ".exe" );
            yield return Path.Combine( path, assRefName + ".winmd" );
            // TODO more extensions?

            // Then, try lookup in target framework directory, if we can parse target framework attribute
            path = this.GetTargetFrameworkPathFor( thisMetaData );
            if ( path != null )
            {
               yield return Path.Combine( path, assRefName + ".dll" );
            }

         }
      }

      public String GetTargetFrameworkPathFor( CILMetaData md )
      {
         var targetFW = this._targetFrameworks.GetOrAdd_WithLock( md, thisMD =>
         {
            TargetFrameworkInfo fwInfo;
            return thisMD.TryGetTargetFrameworkInformation( out fwInfo ) ? fwInfo : null;
         } );

         return this.GetTargetFrameworkPathForFrameworkInfo( targetFW );
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
      public CILMetaDataLoaderNotThreadSafeForFiles( CILMetaDataLoaderResourceCallbacksForFiles callbacks = null )
         : base( callbacks ?? new CILMetaDataLoaderResourceCallbacksForFiles() )
      {

      }
   }

   public class CILMetaDataLoaderThreadSafeSimpleForFiles : CILMetaDataLoaderThreadSafeSimple
   {
      public CILMetaDataLoaderThreadSafeSimpleForFiles( CILMetaDataLoaderResourceCallbacksForFiles callbacks = null )
         : base( callbacks ?? new CILMetaDataLoaderResourceCallbacksForFiles() )
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
      public CILMetaDataLoaderThreadSafeConcurrent( CILMetaDataLoaderResourceCallbacks callbacks )
         : base( new ConcurrentDictionary<String, CILMetaData>(), callbacks )
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
      public CILMetaDataLoaderThreadSafeConcurrentForFiles( CILMetaDataLoaderResourceCallbacksForFiles callbacks = null )
         : base( callbacks ?? new CILMetaDataLoaderResourceCallbacksForFiles() )
      {

      }
   }




}