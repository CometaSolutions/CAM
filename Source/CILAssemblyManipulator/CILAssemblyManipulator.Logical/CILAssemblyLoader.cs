/*
 * Copyright 2014 Stanislav Muhametsin. All rights Reserved.
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
#if !NO_ALIASES
extern alias CAMPhysicalIO;
extern alias CAMPhysicalR;
using CAMPhysicalIO;
using CAMPhysicalR;
using CAMPhysicalIO::CILAssemblyManipulator.Physical.IO;
using CAMPhysicalR::CILAssemblyManipulator.Physical.Resolving;
#else
using CILAssemblyManipulator.Physical.IO;
using CILAssemblyManipulator.Physical.Resolving;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilPack;
using System.IO;
using CILAssemblyManipulator.Logical;
using System.Threading;
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.Loading;

namespace CILAssemblyManipulator.Logical
{
   /// <summary>
   /// This is helper class to cache instances of <see cref="CILAssembly"/> loaded from files.
   /// </summary>
   public interface CILAssemblyLoader
   {
      /// <summary>
      /// If the given resource (e.g. file path) is already cached in this <see cref="CILAssemblyLoader"/>, then it is returned.
      /// Otherwise, the resource is read by <see cref="CILMetaDataLoaderWithCallbacks"/> and <see cref="CILAssembly"/> is constructed from the physical layer <see cref="CILMetaData"/>, and returned.
      /// </summary>
      /// <param name="resource">The resource, e.g. file path.</param>
      /// <returns>The <see cref="CILAssembly"/> located in the given <paramref name="resource"/>.</returns>
      CILAssembly GetOrLoadAssemblyFrom( String resource );
   }

   /// <summary>
   /// This is abstract class for both thread-safe and non-thread-safe <see cref="CILAssemblyLoader"/>s.
   /// This is infrastructure class and typically one needs to use <see cref="CILAssemblyLoaderNotThreadSafe"/> or <see cref="CILAssemblyLoaderThreadSafeSimple"/>.
   /// </summary>
   /// <typeparam name="TDictionary">The type of the dictionary used to cache </typeparam>
   /// <remarks>
   /// TODO: CILAssemblyLoader using System.Collections.Concurrent.ConcurrentDictionary class.
   /// </remarks>
   public abstract class AbstractCILAssemblyLoader<TDictionary>
      where TDictionary : class, IDictionary<String, LogicalAssemblyCreationResult>
   {
      private readonly TDictionary _assemblies;
      private readonly CILReflectionContext _ctx;
      private readonly CILMetaDataLoaderWithCallbacks _mdLoader;

      /// <summary>
      /// Creates new instance of <see cref="AbstractCILAssemblyLoader{T}"/>.
      /// </summary>
      /// <param name="assemblies">The dictionary to cache <see cref="CILAssembly"/> instances.</param>
      /// <param name="ctx">The <see cref="CILReflectionContext"/>.</param>
      /// <param name="mdLoader">The <see cref="CILMetaDataLoaderWithCallbacks"/> to use to load Physical layer <see cref="CILMetaData"/> instances.</param>
      public AbstractCILAssemblyLoader(
         TDictionary assemblies,
         CILReflectionContext ctx,
         CILMetaDataLoaderWithCallbacks mdLoader
         )
      {
         ArgumentValidator.ValidateNotNull( "Context", ctx );
         ArgumentValidator.ValidateNotNull( "MetaData loader", mdLoader );
         ArgumentValidator.ValidateNotNull( "Assemblies", assemblies );

         this._ctx = ctx;
         this._mdLoader = mdLoader;
         this._assemblies = assemblies;
      }

      /// <inheritdoc />
      public CILAssembly LoadAssemblyFrom( String resource )
      {
         return this.DoGetOrLoadAssemblyFrom( resource ).Assembly;
      }

      /// <summary>
      /// Gets the dictionary used to cache assemblies.
      /// </summary>
      protected TDictionary Dictionary
      {
         get
         {
            return this._assemblies;
         }
      }

      /// <summary>
      /// Subclasses should implement this to use correct get-or-add functionality for <see cref="Dictionary"/>.
      /// </summary>
      /// <param name="resource">The key for dictionary.</param>
      /// <param name="factory">The factory callback for dictionary.</param>
      /// <returns>The <see cref="LogicalAssemblyCreationResult"/> for given key.</returns>
      protected abstract LogicalAssemblyCreationResult GetOrAddFromDictionary( String resource, Func<String, LogicalAssemblyCreationResult> factory );

      private LogicalAssemblyCreationResult DoGetOrLoadAssemblyFrom( String resource )
      {
         var loader = this._mdLoader;
         var callbacks = loader.LoaderCallbacks;
         var md = loader.LoadAndResolve( resource );
         resource = loader.GetResourceFor( md );
         // TODO instead, create blank assembly in factory
         // If created -> populate
         // If not -> return existing
         return this.GetOrAddFromDictionary( resource, aResource =>
            this._ctx.CreateLogicalRepresentation(
            md,
            modName =>
            {
               var modRefResource = callbacks.GetPossibleResourcesForModuleReference( loader.GetResourceFor( md ), md, modName )
                  .Select( r => callbacks.SanitizeResource( r ) )
                  .FirstOrDefault( r => callbacks.IsValidResource( r ) );
               return String.IsNullOrEmpty( modRefResource ) ?
                  null :
                  loader.GetOrLoadMetaData( modRefResource );
            },
            this.ResolveAssemblyReference
            ) );
      }

      private LogicalAssemblyCreationResult ResolveAssemblyReference( CILMetaData thisMD, CILAssemblyName aName )
      {
         var loader = this._mdLoader;
         var callbacks = loader.LoaderCallbacks;
         var aRefResource = loader.LoaderCallbacks.GetPossibleResourcesForAssemblyReference( loader.GetResourceFor( thisMD ), thisMD, new AssemblyInformationForResolving( aName.AssemblyInformation, aName.Flags.IsFullPublicKey() ), null )
            .Select( r => callbacks.SanitizeResource( r ) )
            .FirstOrDefault( r => callbacks.IsValidResource( r ) );

         return String.IsNullOrEmpty( aRefResource ) ?
            null :
            this.DoGetOrLoadAssemblyFrom( aRefResource );
      }
   }

   /// <summary>
   /// This class implements <see cref="CILAssemblyLoader"/> in a non-thread-safe way.
   /// </summary>
   public class CILAssemblyLoaderNotThreadSafe : AbstractCILAssemblyLoader<IDictionary<String, LogicalAssemblyCreationResult>>
   {
      /// <summary>
      /// Creates new instance of <see cref="CILAssemblyLoaderNotThreadSafe"/> for given <see cref="CILReflectionContext"/> and using given <see cref="CILMetaDataLoaderWithCallbacks"/>.
      /// </summary>
      /// <param name="ctx">The <see cref="CILReflectionContext"/> to use.</param>
      /// <param name="mdLoader">The <see cref="CILMetaDataLoaderWithCallbacks"/> to use to load Physical layer <see cref="CILMetaData"/> objects.</param>
      public CILAssemblyLoaderNotThreadSafe(
         CILReflectionContext ctx,
         CILMetaDataLoaderWithCallbacks mdLoader
         )
         : base( new Dictionary<String, LogicalAssemblyCreationResult>(), ctx, mdLoader )
      {

      }

      /// <summary>
      /// This method uses <see cref="E_UtilPack.GetOrAdd_NotThreadSafe{T,U}(IDictionary{T,U}, T, Func{T,U})"/> extension method to get or load <see cref="LogicalAssemblyCreationResult"/> from dictionary.
      /// </summary>
      /// <param name="resource">The key to dictionary.</param>
      /// <param name="factory">The factory callback for dictionary.</param>
      /// <returns>The <see cref="LogicalAssemblyCreationResult"/>.</returns>
      protected override LogicalAssemblyCreationResult GetOrAddFromDictionary( String resource, Func<String, LogicalAssemblyCreationResult> factory )
      {
         return this.Dictionary.GetOrAdd_NotThreadSafe( resource, factory );
      }
   }

   /// <summary>
   /// This class implements <see cref="CILAssemblyLoader"/> in a simple thread-safe way.
   /// This means locking whole dictionary when present key is not found, and needs to be added.
   /// </summary>
   public class CILAssemblyLoaderThreadSafeSimple : AbstractCILAssemblyLoader<IDictionary<String, LogicalAssemblyCreationResult>>
   {
      /// <summary>
      /// Creates new instance of <see cref="CILAssemblyLoaderThreadSafeSimple"/> for given <see cref="CILReflectionContext"/> and using given <see cref="CILMetaDataLoaderWithCallbacks"/>.
      /// </summary>
      /// <param name="ctx">The <see cref="CILReflectionContext"/> to use.</param>
      /// <param name="mdLoader">The <see cref="CILMetaDataLoaderWithCallbacks"/> to use to load Physical layer <see cref="CILMetaData"/> objects.</param>
      public CILAssemblyLoaderThreadSafeSimple(
         CILReflectionContext ctx,
         CILMetaDataLoaderWithCallbacks mdLoader
         )
         : base( new Dictionary<String, LogicalAssemblyCreationResult>(), ctx, mdLoader )
      {

      }

      /// <summary>
      /// This method uses <see cref="E_UtilPack.GetOrAdd_WithLock{T,U}(IDictionary{T, U}, T, Func{T,U}, Object)"/> extension method to get or load <see cref="LogicalAssemblyCreationResult"/> from dictionary.
      /// </summary>
      /// <param name="resource">The key to dictionary.</param>
      /// <param name="factory">The factory callback for dictionary.</param>
      /// <returns>The <see cref="LogicalAssemblyCreationResult"/>.</returns>
      protected override LogicalAssemblyCreationResult GetOrAddFromDictionary( String resource, Func<String, LogicalAssemblyCreationResult> factory )
      {
         return this.Dictionary.GetOrAdd_WithLock( resource, factory );
      }
   }
}