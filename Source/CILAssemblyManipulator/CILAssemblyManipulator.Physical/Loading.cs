using CILAssemblyManipulator.Physical;
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
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
   using TModuleInfo = Tuple<String, ReadingArguments, MetaDataResolver>;

   public interface CILMetaDataLoader : IDisposable
   {
      // Resource can be e.g. file name
      CILMetaData GetOrLoadMetaData( String resource );

      // The metaData must be one of the metaDatas loaded by this loader
      Boolean ResolveMetaData( CILMetaData metaData );

      // null if given module is not loaded by this loader
      ReadingArguments GetReadingArgumentsForMetaData( CILMetaData metaData );

      // null if given module is not loaded by this loader
      String GetResourceFor( CILMetaData metaData );

      MetaDataResolver CreateNewResolver();

      HashStreamInfo? PublicKeyComputer { get; }
   }

   public class MetaDataLoadException : Exception
   {
      public MetaDataLoadException( String msg, Exception inner = null )
         : base( msg, inner )
      {

      }
   }

   public abstract class AbstractCILMetaDataLoader<TDictionary> : AbstractDisposable, CILMetaDataLoader
      where TDictionary : class, IDictionary<String, CILMetaData>
   {
      private readonly TDictionary _modules;
      private readonly Dictionary<CILMetaData, TModuleInfo> _moduleInfos;
      private readonly Lazy<HashStreamInfo> _hashStream;
      private readonly Func<ReadingArguments> _readingArgumentsFactory;

      public AbstractCILMetaDataLoader(
         TDictionary metadatas,
         CryptoCallbacks cryptoCallbacks,
         Func<ReadingArguments> readingArgsFactory
         )
      {
         ArgumentValidator.ValidateNotNull( "Modules", metadatas );

         this._modules = metadatas;
         this._moduleInfos = new Dictionary<CILMetaData, TModuleInfo>( ReferenceEqualityComparer<CILMetaData>.ReferenceBasedComparer );
         this._hashStream = cryptoCallbacks == null ? null : new Lazy<HashStreamInfo>( () => cryptoCallbacks.CreateHashStream( AssemblyHashAlgorithm.SHA1 ), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );
         this._readingArgumentsFactory = readingArgsFactory ?? new Func<ReadingArguments>( () => new ReadingArguments() );
      }

      private void _resolver_ModuleReferenceResolveEvent( Object sender, ModuleReferenceResolveEventArgs e )
      {
         TModuleInfo thisModuleInfo;
         if ( this._moduleInfos.TryGetValue( e.ThisMetaData, out thisModuleInfo ) )
         {
            e.ResolvedMetaData = this
               .GetPossibleResourcesForModuleReference( thisModuleInfo.Item1, e.ThisMetaData, e.ModuleName )
               .Where( r => this.IsValidResource( r ) )
               .Select( r => this.GetOrLoadMetaData( r ) )
               .Where( md => md.AssemblyDefinitions.RowCount == 0 && md.ModuleDefinitions.RowCount > 0 && String.Equals( md.ModuleDefinitions.TableContents[0].Name, e.ModuleName ) )
               .FirstOrDefault();
         }
      }

      private void _resolver_AssemblyReferenceResolveEvent( Object sender, AssemblyReferenceResolveEventArgs e )
      {
         TModuleInfo thisModuleInfo;
         if ( this._moduleInfos.TryGetValue( e.ThisMetaData, out thisModuleInfo ) )
         {
            e.ResolvedMetaData = this
               .GetPossibleResourcesForAssemblyReference( thisModuleInfo.Item1, e.ThisMetaData, e.ExistingAssemblyInformation, e.UnparsedAssemblyName )
               .Where( r => this.IsValidResource( r ) )
               .Select( r => this.GetOrLoadMetaData( r ) )
               .Where( md => md.AssemblyDefinitions.GetOrNull( 0 ).IsMatch( e.ExistingAssemblyInformation, false, this._hashStream == null ? (HashStreamInfo?) null : this._hashStream.Value ) )
               .FirstOrDefault();
         }
      }

      public CILMetaData GetOrLoadMetaData( String resource )
      {
         resource = this.SanitizeResource( resource );
         ArgumentValidator.ValidateNotNull( "Resource", resource );
         ReadingArguments rArgs = null;

         var retVal = this.GetOrAddFromDictionary( resource, res =>
         {
            using ( var stream = this.GetStreamFor( res ) )
            {
               rArgs = new ReadingArguments();
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

      public ReadingArguments GetReadingArgumentsForMetaData( CILMetaData metaData )
      {
         TModuleInfo moduleInfo;
         return this._moduleInfos.TryGetValue( metaData, out moduleInfo ) ?
            moduleInfo.Item2 :
            null;
      }

      public String GetResourceFor( CILMetaData metaData )
      {
         TModuleInfo moduleInfo;
         return this._moduleInfos.TryGetValue( metaData, out moduleInfo ) ?
            moduleInfo.Item1 :
            null;
      }

      public MetaDataResolver CreateNewResolver()
      {
         var resolver = new MetaDataResolver();
         resolver.AssemblyReferenceResolveEvent += _resolver_AssemblyReferenceResolveEvent;
         resolver.ModuleReferenceResolveEvent += _resolver_ModuleReferenceResolveEvent;
         return resolver;
      }

      public HashStreamInfo? PublicKeyComputer
      {
         get
         {
            return this._hashStream == null ? (HashStreamInfo?) null : this._hashStream.Value;
         }
      }

      private TModuleInfo ModuleInfoFactory( String resource, CILMetaData md, ReadingArguments rArgs )
      {
         var resolver = this.CreateNewResolver();
         return Tuple.Create( resource, rArgs, resolver );
      }


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

      protected abstract IEnumerable<String> GetPossibleResourcesForAssemblyReference( String thisModulePath, CILMetaData thisMetaData, AssemblyInformationForResolving? assemblyRefInfo, String unparsedAssemblyName );

      protected abstract CILMetaData GetOrAddFromDictionary( String resource, Func<String, CILMetaData> factory );

      protected abstract void PerformResolving( MetaDataResolver resolver, CILMetaData metaData );


      protected override void Dispose( Boolean disposing )
      {
         if ( disposing && this._hashStream != null && this._hashStream.IsValueCreated )
         {
            this._hashStream.Value.Transform.DisposeSafely();
         }
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
      IEnumerable<String> GetPossibleResourcesForAssemblyReference( String thisModulePath, CILMetaData thisMetaData, AssemblyInformationForResolving? assemblyRefInfo, String unparsedAssemblyName );
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
         Func<ReadingArguments> readingArgsFactory,
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

      protected override IEnumerable<String> GetPossibleResourcesForAssemblyReference( String thisModulePath, CILMetaData thisMetaData, AssemblyInformationForResolving? assemblyRefInfo, String unparsedAssemblyName )
      {
         return this._resourceCallbacks.GetPossibleResourcesForAssemblyReference( thisModulePath, thisMetaData, assemblyRefInfo, unparsedAssemblyName );
      }

   }

   public class CILMetaDataLoaderNotThreadSafe : CILMetaDataLoaderWithCallbacks<Dictionary<String, CILMetaData>>
   {
      public CILMetaDataLoaderNotThreadSafe(
         CryptoCallbacks cryptoCallbacks,
         Func<ReadingArguments> readingArgsFactory,
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
         Func<ReadingArguments> readingArgsFactory,
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