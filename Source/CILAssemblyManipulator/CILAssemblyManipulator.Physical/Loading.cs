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

   public interface CILMetaDataLoader
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
   }

   public abstract class AbstractCILMetaDataLoader<TDictionary> : CILMetaDataLoader
      where TDictionary : class, IDictionary<String, CILMetaData>
   {
      private readonly TDictionary _modules;
      private readonly Dictionary<CILMetaData, TModuleInfo> _moduleInfos;

      public AbstractCILMetaDataLoader( TDictionary modules )
      {
         ArgumentValidator.ValidateNotNull( "Modules", modules );

         this._modules = modules;
         this._moduleInfos = new Dictionary<CILMetaData, TModuleInfo>( ReferenceEqualityComparer<CILMetaData>.ReferenceBasedComparer );
      }

      private void _resolver_ModuleReferenceResolveEvent( Object sender, ModuleReferenceResolveEventArgs e )
      {
         TModuleInfo thisModuleInfo;
         if ( this._moduleInfos.TryGetValue( e.ThisMetaData, out thisModuleInfo ) )
         {
            var res = this.GetPossibleResourcesForModuleReference( thisModuleInfo.Item1, e.ThisMetaData, e.ModuleName ).FirstOrDefault( r => this.IsValidResource( r ) );
            if ( !String.IsNullOrEmpty( res ) )
            {
               e.ResolvedMetaData = this.GetOrLoadMetaData( res );
               // TODO verify name at least
            }
         }
      }

      private void _resolver_AssemblyReferenceResolveEvent( Object sender, AssemblyReferenceResolveEventArgs e )
      {
         TModuleInfo thisModuleInfo;
         if ( this._moduleInfos.TryGetValue( e.ThisMetaData, out thisModuleInfo ) )
         {
            var res = this.GetPossibleResourcesForAssemblyReference( thisModuleInfo.Item1, e.ThisMetaData, e.ExistingAssemblyInformation, e.UnparsedAssemblyName ).FirstOrDefault( r => this.IsValidResource( r ) );
            if ( !String.IsNullOrEmpty( res ) )
            {
               e.ResolvedMetaData = this.GetOrLoadMetaData( res );
               // TODO verify name, culture, public key, etc (also that it has assemblyDef row)
            }
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

               return stream.ReadModule( rArgs );
            }
         } );

         Boolean added;
         if ( this.IsThreadSafe )
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

      protected abstract Boolean IsThreadSafe { get; }

      // Something like Path.GetFullPath(..)
      protected abstract String SanitizeResource( String resource );

      protected abstract Boolean IsValidResource( String resource );

      protected abstract Stream GetStreamFor( String resource );

      protected abstract IEnumerable<String> GetPossibleResourcesForModuleReference( String thisModulePath, CILMetaData thisMetaData, String moduleReferenceName );

      protected abstract IEnumerable<String> GetPossibleResourcesForAssemblyReference( String thisModulePath, CILMetaData thisMetaData, AssemblyInformationForResolving? assemblyRefInfo, String unparsedAssemblyName );

      protected abstract CILMetaData GetOrAddFromDictionary( String resource, Func<String, CILMetaData> factory );

      protected abstract void PerformResolving( MetaDataResolver resolver, CILMetaData metaData );


   }

   public interface CILMetaDataLoaderResourceCallbacks
   {
      String SanitizeResource( String resource );
      Boolean IsValidResource( String resource );
      Stream GetStreamFor( String resource );
      IEnumerable<String> GetPossibleResourcesForModuleReference( String thisModulePath, CILMetaData thisMetaData, String moduleReferenceName );
      IEnumerable<String> GetPossibleResourcesForAssemblyReference( String thisModulePath, CILMetaData thisMetaData, AssemblyInformationForResolving? assemblyRefInfo, String unparsedAssemblyName );
   }

   public abstract class CILMetaDataLoaderWithCallbacks<TDictionary> : AbstractCILMetaDataLoader<TDictionary>
      where TDictionary : class, IDictionary<String, CILMetaData>
   {
      private readonly CILMetaDataLoaderResourceCallbacks _resourceCallbacks;

      public CILMetaDataLoaderWithCallbacks(
         TDictionary dictionary,
         CILMetaDataLoaderResourceCallbacks resourceCallbacks
         )
         : base( dictionary )
      {
         ArgumentValidator.ValidateNotNull( "Resource callbacks", resourceCallbacks );

         this._resourceCallbacks = resourceCallbacks;
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
         CILMetaDataLoaderResourceCallbacks resourceCallbacks
         )
         : base( new Dictionary<String, CILMetaData>(), resourceCallbacks )
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

      protected override Boolean IsThreadSafe
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
         CILMetaDataLoaderResourceCallbacks resourceCallbacks
         )
         : base( new Dictionary<String, CILMetaData>(), resourceCallbacks )
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

      protected override Boolean IsThreadSafe
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
}