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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CommonUtils;
using CILAssemblyManipulator.Physical;
using CollectionsWithRoles.API;

#if !CAM_LOGICAL_IS_SL
using System.Collections.Concurrent;
#endif

namespace CILAssemblyManipulator.Logical.Implementation
{
   internal class CILModuleImpl : CILCustomAttributeContainerImpl, CILModule
   {
      private String name;
      private readonly Lazy<CILAssembly> assembly;
      private readonly Lazy<ListProxy<CILType>> types;
      private readonly Lazy<CILType> moduleInitializer;
      private readonly SettableLazy<CILModule> associatedMSCorLib;
#if CAM_LOGICAL_IS_SL
      private readonly Dictionary<String, CILType> typeNameCache;
#else
      private readonly ConcurrentDictionary<String, CILType> typeNameCache;
#endif
      private readonly IDictionary<String, AbstractLogicalManifestResource> manifestResources;

      internal CILModuleImpl( CILReflectionContextImpl ctx, Int32 anID, System.Reflection.Module mod )
         : base( ctx, anID, CILElementKind.Module, cb => cb.GetCustomAttributesDataForOrThrow( mod ) )
      {
         ArgumentValidator.ValidateNotNull( "Module", mod );
         var mName = mod.Name;
         InitFields(
            ctx,
            ref this.name,
            ref this.assembly,
            ref this.types,
            ref this.moduleInitializer,
            ref this.associatedMSCorLib,
            ref this.typeNameCache,
            ref this.manifestResources,
            mod.Name,
            () => ctx.Cache.GetOrAdd( mod.Assembly ),
            () => ctx.CollectionsFactory.NewListProxy<CILType>( ctx.WrapperCallbacks.GetTopLevelDefinedTypesOrThrow( mod )
               .Select( type => ctx.NewWrapperAsType( type ) )
               .ToList()
            ),
            this.BuildModuleInitializerType,
            this.LoadNativeMSCorLibModule,
            null,
            this
            );
      }

      internal CILModuleImpl( CILReflectionContextImpl ctx, Int32 anID, CILAssembly ass, String name )
         : base( ctx, CILElementKind.Module, anID, new Lazy<ListProxy<CILCustomAttribute>>( () => ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>(), ctx.LazyThreadSafetyMode ) )
      {
         InitFields(
            ctx,
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

      private static void InitFields(
         CILReflectionContextImpl ctx,
         ref String name,
         ref Lazy<CILAssembly> assembly,
         ref Lazy<ListProxy<CILType>> types,
         ref Lazy<CILType> moduleInitializer,
         ref SettableLazy<CILModule> associatedMSCorLib,
#if CAM_LOGICAL_IS_SL
         ref Dictionary<String, CILType> typeNameCache,
#else
 ref ConcurrentDictionary<String, CILType> typeNameCache,
#endif
 ref IDictionary<String, AbstractLogicalManifestResource> manifestResources,
         String aName,
         Func<CILAssembly> assemblyFunc,
         Func<ListProxy<CILType>> typesFunc,
         Func<CILType> moduleInitializerFunc,
         Func<CILModule> associatedMSCorLibFunc,
         IDictionary<String, AbstractLogicalManifestResource> mResources,
         CILModuleImpl me
         )
      {
         var lazyThreadSafety = ctx.LazyThreadSafetyMode;
         name = aName;
         assembly = new Lazy<CILAssembly>( assemblyFunc, lazyThreadSafety );
         types = new Lazy<ListProxy<CILType>>( typesFunc, lazyThreadSafety );
         moduleInitializer = new Lazy<CILType>( moduleInitializerFunc, lazyThreadSafety );
         associatedMSCorLib = new SettableLazy<CILModule>( associatedMSCorLibFunc, lazyThreadSafety );
         typeNameCache =
#if CAM_LOGICAL_IS_SL
            new Dictionary<String, CILType>()
#else
 new ConcurrentDictionary<String, CILType>()
#endif
;
         manifestResources = mResources ?? new Dictionary<String, AbstractLogicalManifestResource>();
      }

      private CILType BuildModuleInitializerType()
      {
         var retVal = this.context.Cache.NewBlankType(
            this.context.Cache.ResolveModuleID( this.id ),
            null,
            Miscellaneous.MODULE_TYPE_NAME,
            TypeAttributes.NotPublic | TypeAttributes.AnsiClass | TypeAttributes.AutoLayout,
            CILTypeCode.Object
            );
         retVal.BaseType = null;
         return retVal;
      }

      private CILModule LoadNativeMSCorLibModule()
      {
         return this.context.Cache.GetOrAdd( LogicalUtils.NATIVE_MSCORLIB.GetModules()[0] );
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
         // TODO don't lock if not context is not concurrent.
#if CAM_LOGICAL_IS_SL
         lock( this.typeNameCache )
         {
            this.typeNameCache.Remove( oldName );
         }
#else
         CILType dummy;
         this.typeNameCache.TryRemove( oldName, out dummy );
#endif
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
         this.types.Value.Add( result );
         return result;
      }

      public Boolean RemoveType( CILType type )
      {
         // ((CILTypeInternal)type).RemoveModule(); // TODO
         return this.types.Value.Remove( type );
      }

      public ListQuery<CILType> DefinedTypes
      {
         get
         {
            return this.types.Value.CQ;
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
         // TODO better:
         // Parse top-level type here, use cache to find it.
         // Then walk nested types within another cache access factory func.
         // Benefit: finding another nested type of enclosing type is faster.

         CILType result;
         if ( !this.typeNameCache.TryGetValue( typeString, out result ) )
         {
#if CAM_LOGICAL_IS_SL
            lock( this.typeNameCache )
            {
               if ( !this.typeNameCache.TryGetValue( typeString, out result ) )
               {
#endif
            result = this.FindTypeByName( typeString );
            if ( throwOnError && result == null )
            {
               throw new ArgumentException( "Could not find type " + typeString + "." );
            }
            if ( result != null )
            {
#if CAM_LOGICAL_IS_SL
               this.typeNameCache.Add( typeString, result );
#else
               this.typeNameCache.TryAdd( typeString, result );
#endif
            }
#if CAM_LOGICAL_IS_SL
               }
            }
#endif
         }
         return result;
      }

      public IDictionary<String, AbstractLogicalManifestResource> ManifestResources
      {
         get
         {
            return this.manifestResources;
         }
      }

      #endregion

      public override String ToString()
      {
         return this.name;
      }

      private CILType FindTypeByName( String ts )
      {
         String tlType, nType;
         ts.ParseTypeNameStringForTopLevelType( out tlType, out nType );
         String ns, tn;
         tlType.ParseTypeNameStringForNamespace( out ns, out tn );
         CILType result;
         result = this.types.Value.CQ.FirstOrDefault( t => String.Equals( t.Namespace, ns ) && String.Equals( t.Name, tn ) );

         while ( result != null && nType != null )
         {
            nType.ParseTypeNameStringForTopLevelType( out tlType, out tn );
            result = result.DeclaredNestedTypes.FirstOrDefault( nested => String.Equals( nested.Name, tlType ) );
            nType = tn;
         }
         return result;
      }
   }
}