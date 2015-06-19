/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
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
using System.Text.RegularExpressions;
using System.Threading;
using CollectionsWithRoles.API;
using CommonUtils;

namespace CILAssemblyManipulator.Logical.Implementation
{
   internal class CILModuleImpl : CILCustomAttributeContainerImpl, CILModule
   {
      internal static readonly Regex EXTENSION_REGEX = new Regex( "(" + Regex.Escape( "." ) + "dll|" + Regex.Escape( "." ) + "exe)$", RegexOptions.IgnoreCase );

      private String name;
      private readonly Lazy<CILAssembly> assembly;
      private readonly LazyWithLock<ListProxy<CILType>> types;
      private readonly Lazy<CILType> moduleInitializer;
      private readonly SettableLazy<CILModule> associatedMSCorLib;
      private readonly ConcurrentDictionary<String, CILType> typeNameCache;
      private readonly IDictionary<String, ManifestResource> manifestResources;

      internal CILModuleImpl( CILReflectionContextImpl ctx, Int32 anID, System.Reflection.Module mod )
         : base( ctx, anID, CILElementKind.Module, () => new CustomAttributeDataEventArgs( ctx, mod ) )
      {
         ArgumentValidator.ValidateNotNull( "Module", mod );
         var mName = mod.Name;
         //var match = EXTENSION_REGEX.Match( mName );
         InitFields(
            ref this.name,
            ref this.assembly,
            ref this.types,
            ref this.moduleInitializer,
            ref this.associatedMSCorLib,
            ref this.typeNameCache,
            ref this.manifestResources,
            mod.Name, // match.Success ? mName.Substring( 0, match.Index ) : mName,
            () => ctx.Cache.GetOrAdd( mod.Assembly ),
            () => ctx.CollectionsFactory.NewListProxy<CILType>(
               ctx.LaunchModuleTypesLoadEvent( new ModuleTypesEventArgs( mod ) )
               .Select( type => (CILType) ctx.Cache.GetOrAdd( type ) )
               .ToList()
            ),
            this.BuildModuleInitializerType,
            this.LoadNativeMSCorLibModule,
            null,
            this
            );
      }

      internal CILModuleImpl( CILReflectionContextImpl ctx, Int32 anID, CILAssembly ass, String name )
         : base( ctx, CILElementKind.Module, anID, new LazyWithLock<ListProxy<CILCustomAttribute>>( () => ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>() ) )
      {
         InitFields(
            ref this.name,
            ref this.assembly,
            ref this.types,
            ref this.moduleInitializer,
            ref this.associatedMSCorLib,
            ref this.typeNameCache,
            ref this.manifestResources,
            name,
            () => ass,
            () => ctx.CollectionsFactory.NewListProxy<CILType>(),
            this.BuildModuleInitializerType,
            this.LoadNativeMSCorLibModule,
            null,
            this
            );
      }

      internal CILModuleImpl( CILReflectionContextImpl ctx, Int32 anID, LazyWithLock<ListProxy<CILCustomAttribute>> cAttrs, Func<CILAssembly> ass, String name, Func<CILType> moduleInitializerFunc, Func<ListProxy<CILType>> definedTypes, Func<CILModule> associatedMSCorLibFunc, IDictionary<String, ManifestResource> mResources )
         : base( ctx, CILElementKind.Module, anID, cAttrs )
      {
         InitFields(
            ref this.name,
            ref this.assembly,
            ref this.types,
            ref this.moduleInitializer,
            ref this.associatedMSCorLib,
            ref this.typeNameCache,
            ref this.manifestResources,
            name,
            ass,
            definedTypes,
            moduleInitializerFunc,
            associatedMSCorLibFunc,
            mResources,
            this
            );
      }

      private static void InitFields(
         ref String name,
         ref Lazy<CILAssembly> assembly,
         ref LazyWithLock<ListProxy<CILType>> types,
         ref Lazy<CILType> moduleInitializer,
         ref SettableLazy<CILModule> associatedMSCorLib,
         ref ConcurrentDictionary<String, CILType> typeNameCache,
         ref IDictionary<String, ManifestResource> manifestResources,
         String aName,
         Func<CILAssembly> assemblyFunc,
         Func<ListProxy<CILType>> typesFunc,
         Func<CILType> moduleInitializerFunc,
         Func<CILModule> associatedMSCorLibFunc,
         IDictionary<String, ManifestResource> mResources,
         CILModuleImpl me
         )
      {
         name = aName;
         assembly = new Lazy<CILAssembly>( assemblyFunc, LazyThreadSafetyMode.ExecutionAndPublication );
         types = new LazyWithLock<ListProxy<CILType>>( typesFunc );
         moduleInitializer = new Lazy<CILType>( moduleInitializerFunc, LazyThreadSafetyMode.ExecutionAndPublication );
         associatedMSCorLib = new SettableLazy<CILModule>( associatedMSCorLibFunc );
         typeNameCache = new ConcurrentDictionary<String, CILType>();
         manifestResources = mResources ?? new Dictionary<String, ManifestResource>();
      }

      private CILType BuildModuleInitializerType()
      {
         return this.context.Cache.NewBlankType( this.context.Cache.ResolveModuleID( this.id ), null, "<Module>", TypeAttributes.NotPublic | TypeAttributes.AnsiClass | TypeAttributes.AutoLayout, CILTypeCode.Object );
      }

      private CILModule LoadNativeMSCorLibModule()
      {
         return this.context.Cache.GetOrAdd( Utils.NATIVE_MSCORLIB.GetModules()[0] );
      }

      #region CILElementWithSimpleName Members

      public String Name
      {
         set
         {
            Interlocked.Exchange( ref this.name, value );
         }
         get
         {
            return this.name;
         }
      }

      #endregion

      internal override String IsCapableOfChanging()
      {
         // Always capable of changing
         return null;
      }

      internal void TypeNameChanged( String oldName )
      {
         CILType dummy;
         this.typeNameCache.TryRemove( oldName, out dummy );
      }

      #region CILModule Members

      public CILAssembly Assembly
      {
         get
         {
            return this.assembly.Value;
         }
      }

      public CILType AddType( String name, TypeAttributes attrs, CILTypeCode tc = CILTypeCode.Object )
      {
         var result = this.context.Cache.NewBlankType( this, null, name, attrs, tc );
         lock ( this.types.Lock )
         {
            this.types.Value.Add( result );
         }
         return result;
      }

      public Boolean RemoveType( CILType type )
      {
         lock ( this.types.Lock )
         {
            // ((CILTypeInternal)type).RemoveModule(); // TODO
            return this.types.Value.Remove( type );
         }
      }

      public ListQuery<CILType> DefinedTypes
      {
         get
         {
            return this.types.Value.CQ;
         }
      }

      public Object DefinedTypesLock
      {
         get
         {
            return this.types.Lock;
         }
      }

      public CILType ModuleInitializer
      {
         get
         {
            return this.moduleInitializer.Value;
         }
      }

      public CILModule AssociatedMSCorLibModule
      {
         get
         {
            return this.associatedMSCorLib.Value;
         }
         set
         {
            this.associatedMSCorLib.Value = value;
         }
      }

      public CILType GetTypeByName( String typeString, Boolean throwOnError = true )
      {
         CILType result;
         if ( !this.typeNameCache.TryGetValue( typeString, out result ) )
         {
            result = this.FindTypeByName( typeString );
            if ( throwOnError && result == null )
            {
               throw new ArgumentException( "Could not find type " + typeString + "." );
            }
            if ( result != null )
            {
               this.typeNameCache.TryAdd( typeString, result );
            }
         }
         return result;
      }

      public IDictionary<String, ManifestResource> ManifestResources
      {
         get
         {
            return this.manifestResources;
         }
      }

      #endregion

      public override string ToString()
      {
         return this.name;
      }

      private CILType FindTypeByName( String ts )
      {
         var pResult = Utils.ParseTypeString( ts );
         CILType result;
         if ( pResult.elementInfo == null && pResult.genericArguments == null )
         {
            lock ( this.types.Lock )
            {
               result = this.types.Value.CQ.FirstOrDefault( t => String.Equals( t.Namespace, pResult.nameSpace ) && String.Equals( t.Name, pResult.typeName ) );
            }

            if ( result != null && pResult.nestedTypes != null )
            {
               foreach ( var nt in pResult.nestedTypes )
               {
                  result = result == null ? null : result.DeclaredNestedTypes.FirstOrDefault( nested => String.Equals( nested.Name, nt ) );
               }
            }
         }
         else
         {
            result = null;
         }
         return result;
      }

      internal ConcurrentDictionary<String, CILType> TypeNameCache
      {
         get
         {
            return this.typeNameCache;
         }
      }
   }
}