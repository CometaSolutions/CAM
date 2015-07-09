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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtils;
using System.IO;
using CILAssemblyManipulator.Logical;
using System.Threading;
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Logical
{
   public interface CILAssemblyLoader
   {
      CILAssembly LoadAssemblyFrom( String resource );
   }

   public abstract class AbstractCILAssemblyLoader<TDictionary>
      where TDictionary : class, IDictionary<String, CILAssembly>
   {
      private readonly TDictionary _assemblies;
      private readonly CILReflectionContext _ctx;
      private readonly CILMetaDataLoaderWithCallbacks _mdLoader;


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

      public CILAssembly LoadAssemblyFrom( String resource )
      {
         var loader = this._mdLoader;
         var callbacks = loader.LoaderCallbacks;
         var md = loader.LoadAndResolve( resource );
         resource = loader.GetResourceFor( md );
         // TODO instead, create blank assembly in factory
         // If created -> populate
         // If not -> return existing
         TODO
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

      protected TDictionary Dictionary
      {
         get
         {
            return this._assemblies;
         }
      }

      protected abstract CILAssembly GetOrAddFromDictionary( String resource, Func<String, CILAssembly> factory );

      private CILAssembly ResolveAssemblyReference( CILMetaData thisMD, CILAssemblyName aName )
      {
         var loader = this._mdLoader;
         var callbacks = loader.LoaderCallbacks;
         var aRefResource = loader.LoaderCallbacks.GetPossibleResourcesForAssemblyReference( loader.GetResourceFor( thisMD ), thisMD, new AssemblyInformationForResolving( aName.AssemblyInformation, aName.Flags.IsFullPublicKey() ), null )
            .Select( r => callbacks.SanitizeResource( r ) )
            .FirstOrDefault( r => callbacks.IsValidResource( r ) );

         return String.IsNullOrEmpty( aRefResource ) ?
            null :
            this.LoadAssemblyFrom( aRefResource );
      }
   }

   public class CILAssemblyLoaderNotThreadSafe : AbstractCILAssemblyLoader<IDictionary<String, CILAssembly>>
   {
      public CILAssemblyLoaderNotThreadSafe(
         CILReflectionContext ctx,
         CILMetaDataLoaderWithCallbacks mdLoader
         )
         : base( new Dictionary<String, CILAssembly>(), ctx, mdLoader )
      {

      }

      protected override CILAssembly GetOrAddFromDictionary( String resource, Func<String, CILAssembly> factory )
      {
         return this.Dictionary.GetOrAdd_NotThreadSafe( resource, factory );
      }
   }

   public class CILAssemblyLoaderThreadSafeSimpl : AbstractCILAssemblyLoader<IDictionary<String, CILAssembly>>
   {
      public CILAssemblyLoaderThreadSafeSimpl(
         CILReflectionContext ctx,
         CILMetaDataLoaderWithCallbacks mdLoader
         )
         : base( new Dictionary<String, CILAssembly>(), ctx, mdLoader )
      {

      }

      protected override CILAssembly GetOrAddFromDictionary( String resource, Func<String, CILAssembly> factory )
      {
         return this.Dictionary.GetOrAdd_WithLock( resource, factory );
      }
   }

   //   /// <summary>
   //   /// Helper class to keep track of which assemblies have been loaded from which files (or other resources identifiable by a string).
   //   /// </summary>
   //   public class CILAssemblyLoader
   //   {
   //      // We are using multiple dictionaries and therefore require a lock
   //      private readonly IDictionary<String, CILModule> _allModules;
   //      private readonly IDictionary<CILModule, EmittingArguments> _loadingArgs;
   //      private readonly IDictionary<CILModule, String> _moduleResources;
   //      private readonly Object _modulesLock;


   //      private readonly CILReflectionContext _ctx;
   //      private readonly CILAssemblyLoaderCallbacks _callbacks;

   //      /// <summary>
   //      /// Creates a new instance of <see cref="CILAssemblyLoader"/> which will be bound to a given <see cref="CILReflectionContext"/>.
   //      /// </summary>
   //      /// <param name="ctx">The <see cref="CILReflectionContext"/>.</param>
   //      /// <param name="callbacks">The object implementing required callback methods.</param>
   //      /// <param name="resourceEqualityComparer">The equality comparer to use when comparing resources (file names). Defaults to system's default string equality comparer (case-sensitive).</param>
   //      /// <exception cref="ArgumentNullException">If <paramref name="ctx"/> or <paramref name="callbacks"/> are <c>null</c>.</exception>
   //      public CILAssemblyLoader( CILReflectionContext ctx, CILAssemblyLoaderCallbacks callbacks, IEqualityComparer<String> resourceEqualityComparer )
   //      {
   //         ArgumentValidator.ValidateNotNull( "Reflection context", ctx );
   //         ArgumentValidator.ValidateNotNull( "Callbacks", callbacks );

   //         this._ctx = ctx;
   //         this._callbacks = callbacks;
   //         this._allModules = new Dictionary<String, CILModule>( resourceEqualityComparer );
   //         this._loadingArgs = new Dictionary<CILModule, EmittingArguments>();
   //         this._moduleResources = new Dictionary<CILModule, String>();
   //         this._modulesLock = new Object();
   //      }

   //      /// <summary>
   //      /// Loads a <see cref="CILModule"/> from given resource.
   //      /// If module from that resource is already loaded, returns that module.
   //      /// </summary>
   //      /// <param name="resource">The resource to load module from, e.g. file path.</param>
   //      /// <returns>A <see cref="CILModule"/> loaded from given resource.</returns>
   //      /// <remarks>
   //      /// This throws whatever exceptions the <see cref="CILAssemblyLoaderCallbacks.OpenStream"/> and <see cref="E_CIL.LoadModule(CILReflectionContext, Stream, EmittingArguments)"/> methods throw.
   //      /// </remarks>
   //      /// <exception cref="ArgumentNullException">If <paramref name="resource"/> is <c>null</c>.</exception>
   //      public CILModule LoadModuleFrom( String resource )
   //      {
   //         ArgumentValidator.ValidateNotNull( "Resource", resource );

   //         resource = this._callbacks.CleanResource( resource );
   //         CILModule retVal;
   //         if ( !this._allModules.TryGetValue( resource, out retVal ) )
   //         {
   //            Boolean created = false;
   //            EmittingArguments eArgs = null;
   //            lock ( this._modulesLock )
   //            {
   //               if ( !this._allModules.TryGetValue( resource, out retVal ) )
   //               {
   //                  retVal = this.LoadModuleAndArgs( resource, out eArgs );
   //                  this._allModules.Add( resource, retVal );
   //                  this._loadingArgs.Add( retVal, eArgs );
   //                  this._moduleResources.Add( retVal, resource );
   //                  created = true;
   //               }
   //            }

   //            if ( created )
   //            {
   //               this.ModuleLoadedEvent.InvokeEventIfNotNull( evt => evt( this, new ModuleLoadedEventArgs( this, retVal, resource, eArgs ) ) );
   //            }
   //         }
   //         return retVal;
   //      }

   //      /// <summary>
   //      /// Gets a framework moniker information for a given module.
   //      /// </summary>
   //      /// <param name="module">The <see cref="CILModule"/> module.</param>
   //      /// <returns>The <see cref="FrameworkMonikerInfo"/> for given <paramref name="module"/>.</returns>
   //      /// <exception cref="ArgumentNullException">If <paramref name="module"/> is <c>null</c>.</exception>
   //      /// <exception cref="ArgumentException">If <paramref name="module"/> was not loaded with this <see cref="CILAssemblyLoader"/>.</exception>
   //      public FrameworkMonikerInfo GetFrameworkInfoFor( CILModule module )
   //      {
   //         ArgumentValidator.ValidateNotNull( "Module", module );
   //         EmittingArguments eArgs;
   //         if ( this._loadingArgs.TryGetValue( module, out eArgs ) )
   //         {
   //            return this.GetMonikerInfoFor( this.GetThisModulePath( module ), eArgs );
   //         }
   //         else
   //         {
   //            throw new ArgumentException( "The module " + module + " was not loaded with this loader." );
   //         }
   //      }

   //      /// <summary>
   //      /// Gets all currently loaded modules in this <see cref="CILAssemblyLoader"/>.
   //      /// </summary>
   //      /// <value>All currently loaded modules in this <see cref="CILAssemblyLoader"/>.</value>
   //      /// <remarks>Enumerating this is not threadsafe if you have another threads using this same loader.</remarks>
   //      public IEnumerable<CILModule> CurrentlyLoadedModules
   //      {
   //         get
   //         {
   //            return this._allModules.Values;
   //         }
   //      }

   //      /// <summary>
   //      /// Tries to get resource of a loaded module.
   //      /// </summary>
   //      /// <param name="module">The <see cref="CILModule"/> to get resource for.</param>
   //      /// <param name="resource">The resource that the <paramref name="module"/> was loaded from, if it was loaded by this <see cref="CILAssemblyLoader"/>.</param>
   //      /// <returns><c>true</c> if <paramref name="module"/> was loaded by this <see cref="CILAssemblyLoader"/>; <c>false</c> otherwise.</returns>
   //      public Boolean TryGetResourceFor( CILModule module, out String resource )
   //      {
   //         resource = null;
   //         return module != null && this._moduleResources.TryGetValue( module, out resource );
   //      }

   //      /// <summary>
   //      /// Tries to get <see cref="EmittingArguments"/> of a loaded module.
   //      /// </summary>
   //      /// <param name="module">The <see cref="CILModule"/> to get emitting arguments for.</param>
   //      /// <param name="eArgs">The emitting arguments associated with <paramref name="module"/>, if it was loaded by this <see cref="CILAssemblyLoader"/>.</param>
   //      /// <returns><c>true</c> if <paramref name="module"/> was loaded by this <see cref="CILAssemblyLoader"/>; <c>false</c> otherwise.</returns>
   //      public Boolean TryGetEmittingArgumentsFor( CILModule module, out EmittingArguments eArgs )
   //      {
   //         eArgs = null;
   //         return module != null && this._loadingArgs.TryGetValue( module, out eArgs );
   //      }

   //      /// <summary>
   //      /// Gets the callbacks object of this <see cref="CILAssemblyLoader"/>.
   //      /// </summary>
   //      /// <value>The callbacks object of this <see cref="CILAssemblyLoader"/>.</value>
   //      /// <seealso cref="CILAssemblyLoaderCallbacks"/>
   //      public CILAssemblyLoaderCallbacks Callbacks
   //      {
   //         get
   //         {
   //            return this._callbacks;
   //         }
   //      }

   //      /// <summary>
   //      /// This event gets triggered whenever this <see cref="CILAssemblyLoader"/> loads a module from resource.
   //      /// </summary>
   //      public event EventHandler<ModuleLoadedEventArgs> ModuleLoadedEvent;

   //      /// <summary>
   //      /// Tries to resolve given assembly name as if given module would reference it.
   //      /// </summary>
   //      /// <param name="thisModule">The <see cref="CILModule"/> loaded by this <see cref="CILAssemblyLoader"/>.</param>
   //      /// <param name="name">The <see cref="CILAssemblyName"/> of reference.</param>
   //      /// <param name="resolvedAssembly">This will hold the resolved assembly, if successful.</param>
   //      /// <returns><c>true</c> if successfully resolved reference; <c>false</c> otherwise.</returns>
   //      public Boolean TryResolveReference( CILModule thisModule, CILAssemblyName name, out CILAssembly resolvedAssembly )
   //      {
   //         String modPath = null;
   //         String libAssemblyPath = null;
   //         var retVal = thisModule != null
   //            && this._moduleResources.TryGetValue( thisModule, out modPath );
   //         if ( retVal )
   //         {
   //            if ( !this._callbacks.TryResolveAssemblyFilePath( modPath, name, out libAssemblyPath ) )
   //            {
   //               // Have to deduce ourselves - most likely client assembly referencing system assembly
   //               var fwInfo = this.GetMonikerInfoFor( modPath, this._loadingArgs[thisModule] );
   //               if ( !this._callbacks.TryGetFrameworkAssemblyPath( modPath, name, fwInfo.FrameworkName, fwInfo.FrameworkVersion, fwInfo.ProfileName, out libAssemblyPath ) )
   //               {
   //                  // TODO event
   //                  libAssemblyPath = null;
   //               }
   //            }
   //         }

   //         retVal = libAssemblyPath != null;
   //         resolvedAssembly = retVal ? this.LoadModuleFrom( libAssemblyPath ).Assembly : null;
   //         return retVal;
   //      }

   //      /// <summary>
   //      /// Tries to get the module at specified resource without loading it, if it isn't loaded.
   //      /// </summary>
   //      /// <param name="resource">The resource.</param>
   //      /// <param name="module">This will contain the module, if it was loaded by this <see cref="CILAssemblyLoader"/> at given resource.</param>
   //      /// <returns><c>true</c> if this <see cref="CILAssemblyLoader"/> has module loaded at <paramref name="resource"/>; <c>false</c> otherwise.</returns>
   //      public Boolean TryGetLoadedModule( String resource, out CILModule module )
   //      {
   //         resource = this._callbacks.CleanResource( resource );
   //         return this._allModules.TryGetValue( resource, out module );
   //      }

   //      private CILModule LoadModuleAndArgs( String path, out EmittingArguments eArgs )
   //      {
   //         eArgs = EmittingArguments.CreateForLoadingModule( this.LoadLibAssembly );
   //         eArgs.LazyLoad = true; // Just to make sure - it is important to be set as true, otherwise we load things too soon.

   //         CILModule module;
   //         using ( var fs = this._callbacks.OpenStream( path ) )
   //         {
   //            module = this._ctx.LoadModule( fs, eArgs );
   //         }

   //         var fwInfo = this.GetMonikerInfoFor( path, eArgs );

   //         eArgs.CorLibName = fwInfo.MsCorLibAssembly;
   //         var corLibVersion = fwInfo.Assemblies[eArgs.CorLibName].Item1;
   //         eArgs.CorLibMajor = (UInt16) corLibVersion.Major;
   //         eArgs.CorLibMinor = (UInt16) corLibVersion.Minor;
   //         eArgs.CorLibBuild = (UInt16) corLibVersion.Build;
   //         eArgs.CorLibRevision = (UInt16) corLibVersion.Revision;

   //         return module;
   //      }

   //      private CILAssembly LoadLibAssembly( CILModule thisModule, CILAssemblyName name )
   //      {
   //         CILAssembly retVal;
   //         if ( !this.TryResolveReference( thisModule, name, out retVal ) )
   //         {
   //            throw new Exception( "Failed to deduce path for " + name + " referenced from " + thisModule.Name + "." );
   //         }
   //         return retVal;
   //      }

   //      private FrameworkMonikerInfo GetMonikerInfoFor( String path, EmittingArguments eArgs )
   //      {
   //         FrameworkMonikerInfo fwInfo = null;
   //         Boolean callbackFailed = false;
   //         if ( eArgs.FrameworkName == null || eArgs.FrameworkVersion == null )
   //         {
   //            // Part of the target framework most likely
   //            String fwName, fwVersion, fwProfile;
   //            if ( this._callbacks.TryGetFrameworkInfo( path, out fwName, out fwVersion, out fwProfile ) )
   //            {
   //               callbackFailed = !this._callbacks.TryGetFrameworkMoniker( fwName, fwVersion, fwProfile, out fwInfo );
   //            }
   //         }
   //         else
   //         {
   //            callbackFailed = !this._callbacks.TryGetFrameworkMoniker( eArgs.FrameworkName, eArgs.FrameworkVersion, eArgs.FrameworkProfile, out fwInfo );
   //         }

   //         if ( fwInfo == null )
   //         {
   //            // TODO event or something
   //            throw new Exception( "Failed to deduce target framework for " + path + "." );
   //         }

   //         return fwInfo;
   //      }

   //      private String GetThisModulePath( CILModule thisModule )
   //      {
   //         return this._moduleResources[thisModule];
   //      }
   //   }

   //   /// <summary>
   //   /// This is interface defining the callbacks for <see cref="CILAssemblyLoader"/>.
   //   /// </summary>
   //   public interface CILAssemblyLoaderCallbacks
   //   {
   //      /// <summary>
   //      /// Called to open a read-only stream to a given resource.
   //      /// </summary>
   //      /// <param name="resource">The resource to open stream to (e.g. file path).</param>
   //      /// <returns>An opened <see cref="Stream"/> to given resource.</returns>
   //      Stream OpenStream( String resource );

   //      /// <summary>
   //      /// Preprocesses a resource received by <see cref="CILAssemblyLoader.LoadModuleFrom"/> method.
   //      /// </summary>
   //      /// <param name="resource">The resource given to <see cref="CILAssemblyLoader.LoadModuleFrom"/> method.</param>
   //      /// <returns>The cleaned resource string.</returns>
   //      String CleanResource( String resource );

   //      /// <summary>
   //      /// This method gets called to resolve assembly reference.
   //      /// Should return <c>false</c> if assembly reference can not be directly resolved (e.g. client library referencing system assembly).
   //      /// </summary>
   //      /// <param name="thisModuleResource">The resource where the module or assembly that has this assembly reference, is located.</param>
   //      /// <param name="referencedAssembly">The <see cref="CILAssemblyName"/> of the referenced assembly.</param>
   //      /// <param name="referencedAssemblyResource">This should contain the resource to referenced assembly.</param>
   //      /// <returns><c>true</c> if resource to referenced assembly was resolved and <paramref name="referencedAssemblyResource"/> contains a resource to that assembly; <c>false</c> otherwise.</returns>
   //      /// <remarks>
   //      /// If this method returns <c>false</c>, then <see cref="CILAssemblyLoader"/> will try to resolve the reference as if it would be a system assembly, and will use the <see cref="TryGetFrameworkInfo"/>, <see cref="TryGetFrameworkMoniker"/> and <see cref="TryGetFrameworkAssemblyPath"/> methods accordingly.
   //      /// </remarks>
   //      Boolean TryResolveAssemblyFilePath( String thisModuleResource, CILAssemblyName referencedAssembly, out String referencedAssemblyResource );

   //      /// <summary>
   //      /// This method gets called when module is loaded but the <see cref="System.Runtime.Versioning.TargetFrameworkAttribute"/> could not be found.
   //      /// This happens e.g. for system assemblies.
   //      /// </summary>
   //      /// <param name="thisModuleResource">The resource where the module or assembly that is being processed, is located.</param>
   //      /// <param name="fwName">This should contain the framework identifier string, if this returns <c>true</c>.</param>
   //      /// <param name="fwVersion">This should contain the framework version string, if this returns <c>true</c>.</param>
   //      /// <param name="fwProfile">This should contain the optional framework profile string, if this returns <c>true</c>.</param>
   //      /// <returns><c>true</c> if framework information for the module was resolved; <c>false</c> otherwise.</returns>
   //      Boolean TryGetFrameworkInfo( String thisModuleResource, out String fwName, out String fwVersion, out String fwProfile );

   //      /// <summary>
   //      /// This method gets called in order to obtain <see cref="FrameworkMonikerInfo"/> for a given framework.
   //      /// </summary>
   //      /// <param name="fwName">The framework identifier string.</param>
   //      /// <param name="fwVersion">The framework version string.</param>
   //      /// <param name="fwProfile">The optional framework profile string.</param>
   //      /// <param name="moniker">This should contain the <see cref="FrameworkMonikerInfo"/> for a given framework, if this returns <c>true</c>.</param>
   //      /// <returns><c>true</c> if obtaining the <see cref="FrameworkMonikerInfo"/> for a given framework was successful; <c>false</c> otherwise.</returns>
   //      Boolean TryGetFrameworkMoniker( String fwName, String fwVersion, String fwProfile, out FrameworkMonikerInfo moniker );

   //      /// <summary>
   //      /// This method gets called in order to get the path where the system assembly is located for a given framework.
   //      /// In certain scenarios (e.g. Mono on *nix), the actual framework reference assemblies are located in different directory than the default directory structure in Windows and .NET.
   //      /// </summary>
   //      /// <param name="thisModuleResource">The resource where the module of assembly that is being processed, is located.</param>
   //      /// <param name="referencedAssembly">The <see cref="CILAssemblyName"/> of a system assembly being referenced.</param>
   //      /// <param name="fwName">The framework identifier string.</param>
   //      /// <param name="fwVersion">The framework version string.</param>
   //      /// <param name="fwProfile">The optional framework profile string.</param>
   //      /// <param name="fwAssemblyResource">This should contain the resource to referenced system assembly, if this returns <c>true</c>.</param>
   //      /// <returns><c>true</c> if system assembly reference for a target framework was resolved; <c>false</c> otherwise.</returns>
   //      Boolean TryGetFrameworkAssemblyPath( String thisModuleResource, CILAssemblyName referencedAssembly, String fwName, String fwVersion, String fwProfile, out String fwAssemblyResource );
   //   }

   //   ///// <summary>
   //   ///// This structure represents information about a loaded <see cref="CILModule"/> in <see cref="CILAssemblyLoader"/>.
   //   ///// </summary>
   //   //public struct LoadedModuleInformation
   //   //{
   //   //   private readonly CILModule _module;
   //   //   private readonly String _resource;

   //   //   /// <summary>
   //   //   /// Creates a new instance of <see cref="LoadedModuleInformation"/> with given module and resource.
   //   //   /// </summary>
   //   //   /// <param name="module">The <see cref="CILModule"/>.</param>
   //   //   /// <param name="resource">The resource string (e.g. filepath) where the <paramref name="module"/> was loaded from.</param>
   //   //   /// <exception cref="ArgumentNullException">If <paramref name="module"/> or <paramref name="resource"/> are <c>null</c>.</exception>
   //   //   /// <exception cref="ArgumentException">If <paramref name="resource"/> is an empty string.</exception>
   //   //   public LoadedModuleInformation( CILModule module, String resource )
   //   //   {
   //   //      ArgumentValidator.ValidateNotNull( "Module", module );
   //   //      ArgumentValidator.ValidateNotEmpty( "Resource", resource );

   //   //      this._module = module;
   //   //      this._resource = resource;
   //   //   }

   //   //   /// <summary>
   //   //   /// Gets the <see cref="CILModule"/>.
   //   //   /// </summary>
   //   //   /// <value>The <see cref="CILModule"/>.</value>
   //   //   public CILModule Module
   //   //   {
   //   //      get
   //   //      {
   //   //         return this._module;
   //   //      }
   //   //   }

   //   //   /// <summary>
   //   //   /// Gets the resource this <see cref="Module"/> was loaded from.
   //   //   /// </summary>
   //   //   /// <value>The resource this <see cref="Module"/> was loaded from.</value>
   //   //   public String Resource
   //   //   {
   //   //      get
   //   //      {
   //   //         return this._resource;
   //   //      }
   //   //   }
   //   //}

   //   /// <summary>
   //   /// This is event arguments class for <see cref="CILAssemblyLoader.ModuleLoadedEvent"/> event.
   //   /// </summary>
   //   public class ModuleLoadedEventArgs : EventArgs
   //   {
   //      private readonly CILModule _module;
   //      private readonly String _resource;
   //      private readonly EmittingArguments _eArgs;
   //      private readonly CILAssemblyLoader _loader;

   //      internal ModuleLoadedEventArgs( CILAssemblyLoader loader, CILModule module, String resource, EmittingArguments eArgs )
   //      {
   //         ArgumentValidator.ValidateNotNull( "Assembly loader", loader );
   //         ArgumentValidator.ValidateNotNull( "Module", module );
   //         ArgumentValidator.ValidateNotEmpty( "Resource", resource );
   //         ArgumentValidator.ValidateNotNull( "Emitting arguments", eArgs );

   //         this._loader = loader;
   //         this._module = module;
   //         this._resource = resource;
   //         this._eArgs = eArgs;
   //      }

   //      /// <summary>
   //      /// Gets the <see cref="CILAssemblyLoader"/> that loaded the module.
   //      /// </summary>
   //      /// <value>The <see cref="CILAssemblyLoader"/> that loaded the module.</value>
   //      public CILAssemblyLoader Loader
   //      {
   //         get
   //         {
   //            return this._loader;
   //         }
   //      }

   //      /// <summary>
   //      /// Gets the loaded <see cref="CILModule"/>.
   //      /// </summary>
   //      /// <value>The loaded <see cref="CILModule"/>.</value>
   //      public CILModule Module
   //      {
   //         get
   //         {
   //            return this._module;
   //         }
   //      }

   //      /// <summary>
   //      /// Gets the resource where <see cref="Module"/> was loaded from.
   //      /// </summary>
   //      /// <value>The resource where <see cref="Module"/> was loaded from.</value>
   //      public String Resource
   //      {
   //         get
   //         {
   //            return this._resource;
   //         }
   //      }

   //      /// <summary>
   //      /// Gets the <see cref="EmittingArguments"/> of the module.
   //      /// </summary>
   //      /// <value>The <see cref="EmittingArguments"/> of the module.</value>
   //      public EmittingArguments EmittingArguments
   //      {
   //         get
   //         {
   //            return this._eArgs;
   //         }
   //      }
   //   }
   //}

   //public static partial class E_CIL
   //{
   //   /// <summary>
   //   /// Helper method to load a <see cref="CILAssembly"/> from a given resource.
   //   /// </summary>
   //   /// <param name="loader">The <see cref="CILAssemblyLoader"/>.</param>
   //   /// <param name="resource">The resource to load <see cref="CILAssembly"/> from.</param>
   //   /// <returns>The assembly at given resource.</returns>
   //   /// <seealso cref="CILAssemblyLoader.LoadModuleFrom"/>
   //   /// <remarks>
   //   /// For exception information, see <see cref="CILAssemblyLoader.LoadModuleFrom"/>.
   //   /// </remarks>
   //   public static CILAssembly LoadAssemblyFrom( this CILAssemblyLoader loader, String resource )
   //   {
   //      return loader.LoadModuleFrom( resource ).Assembly;
   //   }

   //   /// <summary>
   //   /// Helper method to get framework moniker info directly from <see cref="CILAssembly"/>.
   //   /// </summary>
   //   /// <param name="loader">The <see cref="CILAssemblyLoader"/>.</param>
   //   /// <param name="assembly">The <see cref="CILAssembly"/>.</param>
   //   /// <returns>The <see cref="FrameworkMonikerInfo"/> for given <paramref name="assembly"/>.</returns>
   //   /// <remarks>
   //   /// <see cref="CILAssemblyLoader.GetFrameworkInfoFor"/> for exception information.
   //   /// </remarks>
   //   public static FrameworkMonikerInfo GetFrameworkInfoFor( this CILAssemblyLoader loader, CILAssembly assembly )
   //   {
   //      ArgumentValidator.ValidateNotNull( "Assembly", assembly );
   //      return loader.GetFrameworkInfoFor( assembly.MainModule );
   //   }

   //   /// <summary>
   //   /// Helper method to call <see cref="CILAssemblyLoader.TryGetResourceFor"/> method
   //   /// </summary>
   //   /// <param name="loader">The <see cref="CILAssemblyLoader"/>.</param>
   //   /// <param name="module">The <see cref="CILModule"/>.</param>
   //   /// <returns>The out parameter of <see cref="CILAssemblyLoader.TryGetResourceFor"/>.</returns>
   //   /// <exception cref="ArgumentException">If <see cref="CILAssemblyLoader.TryGetResourceFor"/> returns <c>false</c>.</exception>
   //   public static String GetResourceFor( this CILAssemblyLoader loader, CILModule module )
   //   {
   //      String retVal;
   //      if ( !loader.TryGetResourceFor( module, out retVal ) )
   //      {
   //         throw new ArgumentException( "Module was not loaded with this loader." );
   //      }
   //      return retVal;
   //   }

   //   /// <summary>
   //   /// Helper method to call <see cref="CILAssemblyLoader.TryGetEmittingArgumentsFor"/> method
   //   /// </summary>
   //   /// <param name="loader">The <see cref="CILAssemblyLoader"/>.</param>
   //   /// <param name="module">The <see cref="CILModule"/>.</param>
   //   /// <returns>The out parameter of <see cref="CILAssemblyLoader.TryGetEmittingArgumentsFor"/>.</returns>
   //   /// <exception cref="ArgumentException">If <see cref="CILAssemblyLoader.TryGetEmittingArgumentsFor"/> returns <c>false</c>.</exception>
   //   public static EmittingArguments GetEmittingArgumentsFor( this CILAssemblyLoader loader, CILModule module )
   //   {
   //      EmittingArguments retVal;
   //      if ( !loader.TryGetEmittingArgumentsFor( module, out retVal ) )
   //      {
   //         throw new ArgumentException( "Module was not loaded with this loader." );
   //      }
   //      return retVal;
   //   }
}