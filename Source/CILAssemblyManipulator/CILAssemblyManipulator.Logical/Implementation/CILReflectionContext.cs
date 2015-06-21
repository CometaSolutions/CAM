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
using System.Text;
using System.Threading;
using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;
using CommonUtils;
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Logical.Implementation
{
   internal class CILReflectionContextImpl : CILReflectionContext
   {
      private static readonly System.Reflection.MethodInfo[] EMPTY_METHODS = new System.Reflection.MethodInfo[0];

      private readonly CollectionsFactory _cf;
      private readonly CILReflectionContextCache _cache;
      private readonly ListQuery<Type> _arrayInterfaces;
      private readonly ListQuery<Type> _multiDimArrayIFaces;

      internal CILReflectionContextImpl( Type[] vectorArrayInterfaces, Type[] multiDimArrayIFaces )
      {
         this._cf = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY;
         this._cache = new CILReflectionContextCache( this );
         if ( vectorArrayInterfaces == null )
         {
            vectorArrayInterfaces = Empty<Type>.Array;
         }
         if ( multiDimArrayIFaces == null )
         {
            multiDimArrayIFaces = Empty<Type>.Array;
         }
         this._arrayInterfaces = this._cf.NewListProxy( vectorArrayInterfaces.Where( iFace => iFace != null ).GetBottomTypes().ToList() ).CQ;
         this._multiDimArrayIFaces = this._cf.NewListProxy( multiDimArrayIFaces.Where( iFace => iFace != null ).GetBottomTypes().ToList() ).CQ;
      }

      #region CILReflectionContext Members

      public event EventHandler<CustomAttributeDataEventArgs> CustomAttributeDataLoadEvent;
      //public event EventHandler<ModuleCustomAttributeEventArgs> ModuleCustomAttributeLoadEvent;
      public event EventHandler<ModuleTypesEventArgs> ModuleTypesLoadEvent;
      public event EventHandler<TypeModuleEventArgs> TypeModuleLoadEvent;
      public event EventHandler<EventOtherMethodsEventArgs> EventOtherMethodsLoadEvent;
      public event EventHandler<ConstantValueLoadArgs> ConstantValueLoadEvent;
      public event EventHandler<ExplicitMethodImplementationLoadArgs> ExplicitMethodImplementationLoadEvent;
      public event EventHandler<MethodBodyLoadArgs> MethodBodyLoadEvent;
      public event EventHandler<TokenResolveArgs> TokenResolveEvent;
      public event EventHandler<MethodImplAttributesEventArgs> MethodImplementationAttributesLoadEvent;
      public event EventHandler<TypeLayoutEventArgs> TypeLayoutLoadEvent;
      public event EventHandler<AssemblyNameEventArgs> AssemblyNameLoadEvent;
      public event EventHandler<CustomModifierEventLoadArgs> CustomModifierLoadEvent;

      public event EventHandler<HashStreamLoadEventArgs> HashStreamLoadEvent;
      public event EventHandler<RSACreationEventArgs> RSACreationEvent;
      public event EventHandler<RSASignatureCreationEventArgs> RSASignatureCreationEvent;
      public event EventHandler<CSPPublicKeyEventArgs> CSPPublicKeyEvent;
      public event EventHandler<AssemblyRefResolveFromLoadedAssemblyEventArgs> AssemblyReferenceResolveFromLoadedAssemblyEvent;

      public CollectionsFactory CollectionsFactory
      {
         get
         {
            return this._cf;
         }
      }

      public ListQuery<Type> VectorArrayInterfaces
      {
         get
         {
            return this._arrayInterfaces;
         }
      }

      public ListQuery<Type> MultiDimensionalArrayInterfaces
      {
         get
         {
            return this._multiDimArrayIFaces;
         }
      }

      public Byte[] ComputePublicKeyToken( Byte[] publicKey )
      {
         Byte[] retVal;
         if ( publicKey == null || publicKey.Length == 0 )
         {
            retVal = publicKey;
         }
         else
         {
            var args = MetaDataWriter.GetArgsForPublicKeyTokenComputing( this );
            using ( args.Transform )
            {
               retVal = args.ComputeHash( publicKey );
            }
            retVal = retVal.Skip( retVal.Length - 8 ).Reverse().ToArray();
         }
         return retVal;
      }

      public Byte[] ExtractPublicKeyFromCSP( String cspName )
      {
         var args = new CSPPublicKeyEventArgs( cspName );
         this.CSPPublicKeyEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
         var pk = args.PublicKey;
         if ( pk.IsNullOrEmpty() )
         {
            throw new InvalidOperationException( "The public key of CSP \"" + cspName + "\" could not be resolved." );
         }
         return pk;
      }

      #endregion

      internal void LaunchCustomAttributeDataLoadEvent( CustomAttributeDataEventArgs args )
      {
         this.CustomAttributeDataLoadEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
         if ( args.CustomAttributeData == null )
         {
            throw new CustomAttributeDataLoadException( args );
         }
      }

      internal IEnumerable<Type> LaunchModuleTypesLoadEvent( ModuleTypesEventArgs args )
      {
         this.ModuleTypesLoadEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
         if ( args.DefinedTypes == null )
         {
            throw new TypesLoadException( args );
         }
         return args.DefinedTypes;
      }

      internal System.Reflection.Module LaunchTypeModuleLoadEvent( TypeModuleEventArgs args )
      {
         this.TypeModuleLoadEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
         if ( args.Module == null )
         {
            throw new ModuleLoadException( args );
         }
         return args.Module;
      }

      internal System.Reflection.MethodInfo[] LaunchEventOtherMethodsLoadEvent( EventOtherMethodsEventArgs args )
      {
         this.EventOtherMethodsLoadEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
         if ( args.OtherMethods == null )
         {
            args.OtherMethods = EMPTY_METHODS;
         }
         return args.OtherMethods;
      }

      internal Object LaunchConstantValueLoadEvent( ConstantValueLoadArgs args )
      {
         this.ConstantValueLoadEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
         return args.ConstantValue;
      }

      internal void LaunchInterfaceMappingLoadEvent( ExplicitMethodImplementationLoadArgs args )
      {
         this.ExplicitMethodImplementationLoadEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
      }

      internal void LaunchMethodBodyLoadEvent( MethodBodyLoadArgs args )
      {
         this.MethodBodyLoadEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
      }

      internal void LaunchTokenResolveEvent( TokenResolveArgs args )
      {
         this.TokenResolveEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
      }

      internal void LaunchMethodImplAttributesEvent( MethodImplAttributesEventArgs args )
      {
         this.MethodImplementationAttributesLoadEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
      }

      internal void LaunchTypeLayoutLoadEvent( TypeLayoutEventArgs args )
      {
         this.TypeLayoutLoadEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
      }

      internal void LaunchAssemblyNameLoadEvent( AssemblyNameEventArgs args )
      {
         this.AssemblyNameLoadEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
      }

      internal void LaunchCustomModifiersLoadEvent( CustomModifierEventLoadArgs args )
      {
         this.CustomModifierLoadEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
      }

      internal LazyWithLock<ListProxy<CILCustomModifier>> LaunchEventAndCreateCustomModifiers( CustomModifierEventLoadArgs args )
      {
         return new LazyWithLock<ListProxy<CILCustomModifier>>( () =>
         {
            this.LaunchCustomModifiersLoadEvent( args );
            return this.CollectionsFactory.NewListProxy<CILCustomModifier>( new List<CILCustomModifier>( args.RequiredModifiers.Select( mod => (CILCustomModifier) new CILCustomModifierImpl( CILCustomModifierOptionality.Required, (CILType) this.Cache.GetOrAdd( mod ) ) ).Concat( args.OptionalModifiers.Select( mod => (CILCustomModifier) new CILCustomModifierImpl( CILCustomModifierOptionality.Optional, (CILType) this.Cache.GetOrAdd( mod ) ) ) ) ) );
         } );
      }

      internal void LaunchHashStreamEvent( AssemblyHashAlgorithm algo, out Func<System.IO.Stream> cryptoStream, out Func<Byte[]> hashGetter, out IDisposable transform, Boolean checkCryptoStreamAndTransform = true )
      {
         var args = new HashStreamLoadEventArgs( algo );
         this.HashStreamLoadEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
         cryptoStream = args.CryptoStream;
         hashGetter = args.HashGetter;
         transform = args.Transform;
         if ( hashGetter == null || ( checkCryptoStreamAndTransform && ( cryptoStream == null || transform == null ) ) )
         {
            throw new InvalidOperationException( "Reflection context's HashStreamLoadEvent handler returned invalid crypto stream result." );
         }
      }

      internal void LaunchHashStreamEvent( HashStreamLoadEventArgs args )
      {
         this.HashStreamLoadEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
      }

      internal void LaunchRSACreationEvent( RSACreationEventArgs args )
      {
         this.RSACreationEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
      }

      internal void LaunchRSASignatureCreationEvent( RSASignatureCreationEventArgs args )
      {
         this.RSASignatureCreationEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
      }

      internal CILAssembly LaunchAssemblyRefResolveEvent( AssemblyRefResolveFromLoadedAssemblyEventArgs args )
      {
         this.AssemblyReferenceResolveFromLoadedAssemblyEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
         if ( args.ResolvedAssembly == null )
         {
            throw new InvalidOperationException( "Could not resolve assembly " + args.AssemblyName + "." );
         }
         else
         {
            return args.ResolvedAssembly;
         }
      }


      internal CILReflectionContextCache Cache
      {
         get
         {
            return this._cache;
         }
      }

      public void Dispose()
      {
         this._cache.Dispose();
      }
   }

   internal class CILReflectionContextCache : IDisposable
   {
      internal const Int32 NO_ID = -1;

      private class ElementCache
      {
         private class InnerCache
         {
            private static readonly Object NO_ARRAY_INFO = new Object();

            private readonly ConcurrentDictionary<Object, CILType> _arrayTypes;
            private readonly Func<Object, CILType> _arrayMakingFunction;
            private readonly Func<ElementKind, GeneralArrayInfo, CILType> _genericElementFunction;
            private CILType _pointerType;
            private CILType _referenceType;

            internal InnerCache( CILTypeBase originalType, Func<ElementKind, GeneralArrayInfo, CILType> genericElementFunction )
            {
               this._genericElementFunction = genericElementFunction;
               this._arrayMakingFunction = info => (CILType) genericElementFunction( ElementKind.Array, info as GeneralArrayInfo );
               this._arrayTypes = new ConcurrentDictionary<Object, CILType>();
            }

            internal CILType MakeElementType( CILTypeBase type, ElementKind kind, GeneralArrayInfo arrayInfo )
            {
               switch ( kind )
               {
                  case ElementKind.Array:
                     // Just like with generic arguments, we may need to create a copy of array info if it is specified
                     CILType result;
                     if ( arrayInfo != null )
                     {
                        if ( !this._arrayTypes.TryGetValue( arrayInfo, out result ) )
                        {
                           arrayInfo = new GeneralArrayInfo( arrayInfo );
                           result = this._arrayTypes.GetOrAdd( arrayInfo, this._arrayMakingFunction );
                        }
                     }
                     else
                     {
                        result = this._arrayTypes.GetOrAdd( NO_ARRAY_INFO, this._arrayMakingFunction );
                     }
                     return result;
                  case ElementKind.Pointer:
                     if ( ElementKind.Reference == type.GetElementKind() )
                     {
                        throw new InvalidOperationException( "Can not create pointer type from reference type." );
                     }
                     if ( this._pointerType == null )
                     {
                        Interlocked.Exchange( ref this._pointerType, this._genericElementFunction( ElementKind.Pointer, null ) );
                     }
                     return this._pointerType;
                  case ElementKind.Reference:
                     if ( ElementKind.Reference == type.GetElementKind() )
                     {
                        throw new InvalidOperationException( "Can not create reference type from another reference type." );
                     }
                     if ( this._referenceType == null )
                     {
                        Interlocked.Exchange( ref this._referenceType, this._genericElementFunction( ElementKind.Reference, null ) );
                     }
                     return this._referenceType;
                  default:
                     throw new ArgumentException( "Unknown element kind: " + kind );
               }
            }
         }

         private readonly ConcurrentDictionary<CILTypeBase, InnerCache> _cache;
         private readonly Func<CILTypeBase, InnerCache> _outerGetter;

         internal ElementCache( Func<CILTypeBase, ElementKind, GeneralArrayInfo, CILTypeBase> creatorFunc )
         {
            this._cache = new ConcurrentDictionary<CILTypeBase, InnerCache>();
            this._outerGetter = type => new InnerCache( type, ( kind, info ) => (CILType) creatorFunc( type, kind, info ) );
         }

         internal CILType MakeElementType( CILTypeBase type, ElementKind kind, GeneralArrayInfo arrayInfo )
         {
            return this._cache.GetOrAdd( type, this._outerGetter ).MakeElementType( type, kind, arrayInfo );
         }
      }

      private class SimpleConcurrentCache<TNative, TEmulated>
         where TNative : class
         where TEmulated : class
      {
         private readonly ConcurrentDictionary<TNative, TEmulated> _cache;
         private readonly Func<TNative, TEmulated> _creatorFunc;

         internal SimpleConcurrentCache( Func<TNative, TEmulated> creatorFunc )
         {
            this._creatorFunc = creatorFunc;
            this._cache = new ConcurrentDictionary<TNative, TEmulated>();
         }

         internal TEmulated GetOrAdd( TNative nativeElement )
         {
            if ( nativeElement == null )
            {
               return null;
            }
            else
            {
               return this._cache.GetOrAdd( nativeElement, this._creatorFunc );
            }
         }
      }

      private class GenericInstanceCache<TInstance> : IDisposable
         where TInstance : class, CILElementWithGenericArguments<Object>
      {
         private class InnerCache : IDisposable
         {
            private readonly ReaderWriterLockSlim _lock;
            private readonly ConcurrentDictionary<CILTypeBase[], TInstance> _cache;
            private readonly Func<CILTypeBase[], TInstance> _creator;

            internal InnerCache( TInstance gDef, Func<TInstance, CILTypeBase[], TInstance> idCreator )
            {
               this._lock = new ReaderWriterLockSlim( LockRecursionPolicy.NoRecursion );
               this._cache = new ConcurrentDictionary<CILTypeBase[], TInstance>( ArrayEqualityComparer<CILTypeBase>.DefaultArrayEqualityComparer );
               this._creator = key => idCreator( gDef, key );
            }

            internal TInstance GetOrAdd( CILTypeBase[] gArgs )
            {
               this._lock.EnterReadLock();
               try
               {
                  TInstance result;
                  if ( !this._cache.TryGetValue( gArgs, out result ) )
                  {
                     // Because array comes from outside of CAM, create a copy of it to prevent modifications
                     var tmp = new CILTypeBase[gArgs.Length];
                     Array.Copy( gArgs, tmp, gArgs.Length );
                     result = this._cache.GetOrAdd( tmp, this._creator );
                  }
                  return result;
               }
               finally
               {
                  this._lock.ExitReadLock();
               }
            }

            internal void DoSomethingForAll<TCasted>( TInstance source, Action<TCasted> action )
               where TCasted : class
            {
               this._lock.EnterWriteLock();
               try
               {
                  foreach ( var instance in this._cache.Values )
                  {
                     if ( !Object.ReferenceEquals( instance, source ) )
                     {
                        action( instance as TCasted );
                     }
                  }
               }
               finally
               {
                  this._lock.ExitWriteLock();
               }
            }

            public void Dispose()
            {
               this._lock.Dispose();
            }
         }

         private readonly ConcurrentDictionary<TInstance, InnerCache> _cache;
         private readonly Func<TInstance, InnerCache> _outerCreator;
         private readonly Func<TInstance, Int32> _gArgsCountFunc;
         private readonly String _elementKindString;

         internal GenericInstanceCache( String elementKindString, Func<TInstance, CILTypeBase[], TInstance> idCreator, Func<TInstance, Int32> gArgsCountFunc )
         {
            this._elementKindString = elementKindString;
            this._cache = new ConcurrentDictionary<TInstance, InnerCache>();
            this._outerCreator = instance => new InnerCache( instance, idCreator );
            this._gArgsCountFunc = gArgsCountFunc;
         }

         internal TInstance MakeGenericInstance( TInstance thisInstance, TInstance gDef, CILTypeBase[] gArgs )
         {
            if ( gDef == null )
            {
               if ( gArgs == null || gArgs.Length == 0 )
               {
                  return thisInstance;
               }
               else
               {
                  throw new InvalidOperationException( "Tried to give generic arguments ( " + String.Join( ", ", (Object[]) gArgs ) + ") to non-generic " + this._elementKindString + "." );
               }
            }
            else if ( !gDef.IsGenericDefinition() )
            {
               // TODO make do gDef = gDef.MQ.GenericDefinition maybe? throwing is ok tho, especially in debug mode
               throw new InvalidOperationException( "When making generic " + this._elementKindString + ", the " + this._elementKindString + " must be generic " + this._elementKindString + " definition." );
            }
            else if ( gArgs == null )
            {
               throw new ArgumentNullException( "Generic argument array was null." );
            }

            if ( gArgs.Length != this._gArgsCountFunc( gDef ) )
            {
               throw new ArgumentException( "Amount of required generic parameters is " + this._gArgsCountFunc( gDef ) + ", but was given " + gArgs.Length + "." );
            }
            for ( var i = 0; i < gArgs.Length; ++i )
            {
               if ( gArgs[i] == null )
               {
                  throw new ArgumentNullException( "Generic argument at index " + i + " was null." );
               }
               else if ( gArgs[i].IsPointerType() || gArgs[i].IsByRef() || gArgs[i].Equals( gArgs[i].Module.AssociatedMSCorLibModule.GetTypeByName( Consts.VOID ) ) )
               {
                  throw new ArgumentException( "Generic argument " + gArgs[i] + " at index " + i + " was invalid." );
               }
            }
            return this._cache.GetOrAdd( gDef, this._outerCreator ).GetOrAdd( gArgs );
         }

         internal void ForAllInstancesOf<TCasted>( TInstance gDef, Action<TCasted> action )
            where TCasted : class
         {
            InnerCache inner;
            if ( this._cache.TryGetValue( gDef, out inner ) )
            {
               inner.DoSomethingForAll( gDef, action );
            }
         }

         public void Dispose()
         {
            foreach ( var val in this._cache.Values )
            {
               val.Dispose();
            }
         }
      }

      private class GenericDeclaringTypeCache<TInstance> : IDisposable
         where TInstance : CILElementOwnedByType
      {
         private class InnerCache : IDisposable
         {
            private class InnermostCache
            {
               private readonly ConcurrentDictionary<TInstance, TInstance> _instances;
               private readonly Func<TInstance, TInstance> _instanceCreator;

               internal InnermostCache( CILType gDef, CILTypeBase[] gArgs, Func<TInstance, CILTypeBase[], TInstance> creationFunc )
               {
                  this._instances = new ConcurrentDictionary<TInstance, TInstance>();
                  this._instanceCreator = gDefInstance =>
                  {
                     if ( gArgs.All( gArg => gArg.TypeKind == TypeKind.TypeParameter && Object.Equals( ( (CILTypeParameter) gArg ).DeclaringType, gDef ) && ( (CILTypeParameter) gArg ).DeclaringMethod == null ) )
                     {
                        return gDefInstance;
                     }
                     else
                     {
                        foreach ( var g in gArgs )
                        {
                           if ( g.TypeKind == TypeKind.MethodSignature && !g.Module.Equals( gDef.Module ) )
                           {
                              var gtmp = new CILTypeBase[gArgs.Length];
                              Array.Copy( gArgs, gtmp, gArgs.Length );
                              for ( var i = 0; i < gtmp.Length; ++i )
                              {
                                 LogicalUtils.CheckTypeForMethodSig( gDef.Module, ref gtmp[i] );
                              }
                              gArgs = gtmp;
                              break;
                           }
                        }
                        return creationFunc( gDefInstance, gArgs );
                     }
                  };
               }

               internal TInstance GetOrAdd( TInstance gDefInstance )
               {
                  return this._instances.GetOrAdd( gDefInstance, this._instanceCreator );
               }

               internal Boolean TryGetValue( TInstance gDefInstance, out TInstance result )
               {
                  return this._instances.TryGetValue( gDefInstance, out result );
               }
            }

            private readonly ReaderWriterLockSlim _lock;
            private readonly ConcurrentDictionary<CILTypeBase[], InnermostCache> _cache;
            private readonly Func<CILTypeBase[], InnermostCache> _cacheCreator;

            internal InnerCache( CILType gDef, Func<TInstance, CILTypeBase[], TInstance> creationFunc )
            {
               this._lock = new ReaderWriterLockSlim( LockRecursionPolicy.NoRecursion );
               this._cache = new ConcurrentDictionary<CILTypeBase[], InnermostCache>( ArrayEqualityComparer<CILTypeBase>.DefaultArrayEqualityComparer );
               this._cacheCreator = gArgs => new InnermostCache( gDef, gArgs, creationFunc );
            }

            internal TInstance GetOrAdd( TInstance instance, CILTypeBase[] gArgs )
            {
               this._lock.EnterReadLock();
               try
               {
                  return this._cache.GetOrAdd( gArgs, this._cacheCreator )
                     .GetOrAdd( instance );
               }
               finally
               {
                  this._lock.ExitReadLock();
               }
            }

            internal void DoSomethingForAll<TCasted>( TInstance gDefInstance, Action<TCasted> action )
               where TCasted : class
            {
               this._lock.EnterWriteLock();
               try
               {
                  TInstance instance;
                  foreach ( var cache in this._cache.Values )
                  {
                     if ( cache.TryGetValue( gDefInstance, out instance ) && !Object.ReferenceEquals( gDefInstance, instance ) )
                     {
                        action( instance as TCasted );
                     }
                  }
               }
               finally
               {
                  this._lock.ExitWriteLock();
               }
            }

            public void Dispose()
            {
               this._lock.Dispose();
            }
         }

         private readonly ConcurrentDictionary<CILType, InnerCache> _cache;
         private readonly Func<CILType, InnerCache> _cacheCreator;

         internal GenericDeclaringTypeCache( Func<CILType, TInstance, CILTypeBase[], TInstance> creationFunc )
         {
            this._cache = new ConcurrentDictionary<CILType, InnerCache>();
            this._cacheCreator = gDef => new InnerCache( gDef, ( gDefInstance, gArgs ) => creationFunc( gDef, gDefInstance, gArgs ) );
         }

         internal TInstance GetOrAdd( TInstance instance, CILTypeBase[] gArgs )
         {
            if ( !instance.DeclaringType.IsGenericTypeDefinition() )
            {
               if ( !instance.DeclaringType.IsGenericType() )
               {
                  return instance;
               }
               else
               {
                  throw new ArgumentException( "The declaring type of " + instance + " is not from generic type defintion" );
               }
            }
            return this._cache.GetOrAdd( instance.DeclaringType, this._cacheCreator )
               .GetOrAdd( instance, gArgs );
         }

         internal void ForAllInstancesOf<TCasted>( TInstance gDefInstance, Action<TCasted> action )
            where TCasted : class
         {
            InnerCache cache;
            if ( this._cache.TryGetValue( gDefInstance.DeclaringType, out cache ) )
            {
               cache.DoSomethingForAll( gDefInstance, action );
            }
         }

         public void Dispose()
         {
            foreach ( var val in this._cache.Values )
            {
               val.Dispose();
            }
         }
      }

      private class ListHolder<T> : IDisposable
         where T : class
      {
         private readonly ReaderWriterLockSlim _lock;
         private readonly IList<T> _list;

         internal ListHolder()
         {
            this._lock = new ReaderWriterLockSlim( LockRecursionPolicy.NoRecursion );
            this._list = new List<T>();
         }

         internal T AcquireNew( Func<Int32, T> func )
         {
            this._lock.EnterWriteLock();
            T result;
            try
            {
               result = func( this._list.Count );
               this._list.Add( result );
            }
            finally
            {
               this._lock.ExitWriteLock();
            }
            return result;
         }

         internal T this[Int32 idx]
         {
            get
            {
               this._lock.EnterReadLock();
               try
               {
                  return this._list[idx];
               }
               finally
               {
                  this._lock.ExitReadLock();
               }
            }
         }

         public void Dispose()
         {
            this._lock.Dispose();
         }
      }

      // TODO need to add additional .IsCapableOfChanging checks for this to work properly.
      //// Since array, by-ref and pointer types can't be changed (new methods etc can't be added), these should always remain empty.
      //private static readonly ListProxy<CILType> EMPTY_TYPES = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<CILType>();
      //private static readonly ListProxy<CILField> EMPTY_FIELDS = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<CILField>();
      //private static readonly ListProxy<CILMethod> EMPTY_METHODS = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<CILMethod>();
      //private static readonly ListProxy<CILCustomAttribute> EMPTY_ATTRIBUTES = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<CILCustomAttribute>();
      //private static readonly ListProxy<CILParameter> EMPTY_PARAMETERS = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<CILParameter>();
      //private static readonly ListProxy<CILConstructor> EMPTY_CTORS = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<CILConstructor>();
      //private static readonly ListProxy<CILProperty> EMPTY_PROPERTIES = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<CILProperty>();
      //private static readonly ListProxy<CILEvent> EMPTY_EVENTS = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<CILEvent>();
      //private static readonly ListProxy<CILCustomModifier> EMPTY_MODIFIERS = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<CILCustomModifier>();

      private static readonly CILType[] EMPTY_TYPES = Empty<CILType>.Array;
      //private static readonly CILField[] EMPTY_FIELDS = Empty<CILField>.Array;
      private static readonly CILMethod[] EMPTY_METHODS = Empty<CILMethod>.Array;
      //private static readonly CILCustomAttribute[] EMPTY_ATTRIBUTES = Empty<CILCustomAttribute>.Array;
      //private static readonly CILParameter[] EMPTY_PARAMETERS = Empty<CILParameter>.Array;
      private static readonly CILConstructor[] EMPTY_CTORS = Empty<CILConstructor>.Array;
      private static readonly CILProperty[] EMPTY_PROPERTIES = Empty<CILProperty>.Array;
      private static readonly CILEvent[] EMPTY_EVENTS = Empty<CILEvent>.Array;
      //private static readonly CILCustomModifier[] EMPTY_MODIFIERS = Empty<CILCustomModifier>.Array;

      private static readonly IDictionary<ElementKind, Type> ELEMENT_KIND_BASE_TYPES;
      private static readonly IDictionary<ElementKind, Func<CILReflectionContextCache, CILTypeBase, GeneralArrayInfo, CILType[]>> ELEMENT_KIND_INTERFACES;
      private static readonly IDictionary<ElementKind, TypeAttributes> ELEMENT_KIND_ATTRIBUTES;
      private static readonly IDictionary<ElementKind, Func<CILReflectionContextCache, CILTypeBase, Int32, GeneralArrayInfo, CILMethod[]>> ELEMENT_KIND_METHODS;
      private static readonly IDictionary<ElementKind, Func<CILReflectionContextCache, CILTypeBase, Int32, GeneralArrayInfo, CILConstructor[]>> ELEMENT_KIND_CTORS;
      private static readonly IDictionary<ElementKind, Func<CILReflectionContextCache, CILTypeBase, Int32, GeneralArrayInfo, CILProperty[]>> ELEMENT_KIND_PROPERTIES;
      private static readonly IDictionary<ElementKind, Func<CILReflectionContextCache, CILTypeBase, Int32, GeneralArrayInfo, CILEvent[]>> ELEMENT_KIND_EVENTS;

      static CILReflectionContextCache()
      {
         var dic2 = new Dictionary<ElementKind, Type>();
         dic2.Add( ElementKind.Array, typeof( System.Array ) );
         dic2.Add( ElementKind.Pointer, null );
         dic2.Add( ElementKind.Reference, null );
         ELEMENT_KIND_BASE_TYPES = dic2;

         var dic3 = new Dictionary<ElementKind, Func<CILReflectionContextCache, CILTypeBase, GeneralArrayInfo, CILType[]>>();
         dic3.Add( ElementKind.Array, ( cache, type, aInfo ) => ( aInfo != null ? cache._ctx.MultiDimensionalArrayInterfaces : cache._ctx.VectorArrayInterfaces ).Select( iFace =>
         {
            // TODO use AssociatedMSCorLib of current module to get these types.
            var result = cache.GetOrAdd( iFace );
            if ( iFace
#if WINDOWS_PHONE_APP
               .GetTypeInfo()
#endif
.IsGenericTypeDefinition )
            {
               result = cache.MakeGenericType( (CILType) result, (CILType) result, type );
            }
            return (CILType) result;
         } ).ToArray() );
         dic3.Add( ElementKind.Pointer, ( cache, type, aInfo ) => EMPTY_TYPES );
         dic3.Add( ElementKind.Reference, ( cache, type, aInfo ) => EMPTY_TYPES );
         ELEMENT_KIND_INTERFACES = dic3;

         var dic4 = new Dictionary<ElementKind, TypeAttributes>();
         dic4.Add( ElementKind.Array, TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Serializable );
         dic4.Add( ElementKind.Pointer, TypeAttributes.AutoLayout );
         dic4.Add( ElementKind.Reference, TypeAttributes.AutoLayout );
         ELEMENT_KIND_ATTRIBUTES = dic4;

         var dic5 = new Dictionary<ElementKind, Func<CILReflectionContextCache, CILTypeBase, Int32, GeneralArrayInfo, CILMethod[]>>();
         dic5.Add( ElementKind.Array, ( cache, originalType, elementType, arrayInfo ) =>
         {
            Func<Int32, Int32, CILParameter> intParamFunc = ( methodID, pIdx ) => cache._paramContainers.AcquireNew( pID => new CILParameterImpl(
                  cache._ctx,
                  pID,
                  new LazyWithLock<ListProxy<CILCustomAttribute>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>() ),
                  new SettableValueForEnums<ParameterAttributes>( ParameterAttributes.None ),
                  pIdx,
                  new SettableValueForClasses<String>( null ),
                  () => cache.ResolveMethodBaseID( methodID ),
                  () => cache.GetOrAdd( typeof( Int32 ) ),
                  new SettableLazy<Object>( () => null ),
                  new LazyWithLock<ListProxy<CILCustomModifier>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomModifier>() ),
                  new SettableLazy<MarshalingInfo>( () => null )
                  )
             );
            Func<Int32, Int32, Boolean, CILParameter> valueParamFunc = ( methodID, pIdx, makeRef ) => cache._paramContainers.AcquireNew( pID => new CILParameterImpl(
                  cache._ctx,
                  pID,
                  new LazyWithLock<ListProxy<CILCustomAttribute>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>() ),
                  new SettableValueForEnums<ParameterAttributes>( ParameterAttributes.None ),
                  pIdx,
                  new SettableValueForClasses<String>( null ),
                  () => cache.ResolveMethodBaseID( methodID ),
                  () =>
                  {
                     var tR = originalType;
                     if ( makeRef )
                     {
                        tR = cache.MakeElementType( tR, ElementKind.Reference, null );
                     }
                     return tR;
                  },
                  new SettableLazy<Object>( () => null ),
                  new LazyWithLock<ListProxy<CILCustomModifier>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomModifier>() ),
                  new SettableLazy<MarshalingInfo>( () => null )
                  )
            );

            var result = new CILMethod[3];
            var dimSize = arrayInfo == null ? 1 : arrayInfo.Rank;
            result[0] = (CILMethod) cache._methodContainers.AcquireNew( curMID => new CILMethodImpl(
                  cache._ctx,
                  curMID,
                  new LazyWithLock<ListProxy<CILCustomAttribute>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>() ),
                  new SettableValueForEnums<CallingConventions>( CallingConventions.HasThis | CallingConventions.Standard ),
                  new SettableValueForEnums<MethodAttributes>( MethodAttributes.FamANDAssem | MethodAttributes.Family ),
                  () => (CILType) cache.ResolveTypeID( elementType ),
                  () => cache._ctx.CollectionsFactory.NewListProxy<CILParameter>( Enumerable.Range( 0, dimSize + 1 )
                     .Select( pIdx => pIdx < dimSize ? intParamFunc( curMID, pIdx ) : valueParamFunc( curMID, pIdx, false ) )
                     .ToList() ),
                  new SettableLazy<MethodImplAttributes>( () => MethodImplAttributes.IL ),
                  null,
                  new SettableValueForClasses<String>( "Set" ),
                  () => cache._paramContainers.AcquireNew( pID => new CILParameterImpl(
                        cache._ctx,
                        pID,
                        new LazyWithLock<ListProxy<CILCustomAttribute>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>() ),
                        new SettableValueForEnums<ParameterAttributes>( ParameterAttributes.None ),
                        E_CIL.RETURN_PARAMETER_POSITION,
                        new SettableValueForClasses<String>( null ),
                        () => cache.ResolveMethodBaseID( curMID ),
                        () => cache.GetOrAdd( typeof( void ) ),
                        new SettableLazy<Object>( () => null ),
                        new LazyWithLock<ListProxy<CILCustomModifier>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomModifier>() ),
                        new SettableLazy<MarshalingInfo>( () => null )
                        )
                  ),
                  () => cache._ctx.CollectionsFactory.NewListProxy<CILTypeBase>(),
                  () => null
                  )
            );
            result[1] = (CILMethod) cache._methodContainers.AcquireNew( curMID => new CILMethodImpl(
                  cache._ctx,
                  curMID,
                  new LazyWithLock<ListProxy<CILCustomAttribute>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>() ),
                  new SettableValueForEnums<CallingConventions>( CallingConventions.HasThis | CallingConventions.Standard ),
                  new SettableValueForEnums<MethodAttributes>( MethodAttributes.FamANDAssem | MethodAttributes.Family ),
                  () => (CILType) cache.ResolveTypeID( elementType ),
                  () => cache._ctx.CollectionsFactory.NewListProxy<CILParameter>( Enumerable.Range( 0, dimSize ).Select( pIdx => intParamFunc( curMID, pIdx ) ).ToList() ),
                  new SettableLazy<MethodImplAttributes>( () => MethodImplAttributes.IL ),
                  null,
                  new SettableValueForClasses<String>( "Address" ),
                  () => valueParamFunc( curMID, E_CIL.RETURN_PARAMETER_POSITION, true ),
                  () => cache._ctx.CollectionsFactory.NewListProxy<CILTypeBase>(),
                  () => null
                  )
            );
            result[2] = (CILMethod) cache._methodContainers.AcquireNew( curMID => new CILMethodImpl(
                  cache._ctx,
                  curMID,
                  new LazyWithLock<ListProxy<CILCustomAttribute>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>() ),
                  new SettableValueForEnums<CallingConventions>( CallingConventions.HasThis | CallingConventions.Standard ),
                  new SettableValueForEnums<MethodAttributes>( MethodAttributes.FamANDAssem | MethodAttributes.Family ),
                  () => (CILType) cache.ResolveTypeID( elementType ),
                  () => cache._ctx.CollectionsFactory.NewListProxy<CILParameter>( Enumerable.Range( 0, dimSize ).Select( pIdx => intParamFunc( curMID, pIdx ) ).ToList() ),
                  new SettableLazy<MethodImplAttributes>( () => MethodImplAttributes.IL ),
                  null,
                  new SettableValueForClasses<String>( "Get" ),
                  () => valueParamFunc( curMID, E_CIL.RETURN_PARAMETER_POSITION, false ),
                  () => cache._ctx.CollectionsFactory.NewListProxy<CILTypeBase>(),
                  () => null
                  )
            );
            return result;
         } );
         dic5.Add( ElementKind.Pointer, ( cache, originalType, elementType, arrayRank ) => EMPTY_METHODS );
         dic5.Add( ElementKind.Reference, ( cache, originalType, elementType, arrayRank ) => EMPTY_METHODS );
         ELEMENT_KIND_METHODS = dic5;

         var dic6 = new Dictionary<ElementKind, Func<CILReflectionContextCache, CILTypeBase, Int32, GeneralArrayInfo, CILConstructor[]>>();
         dic6.Add( ElementKind.Array, ( cache, originalType, elementType, arrayInfo ) =>
         {
            Func<Int32, Int32, CILParameter> intParamFunc = ( methodID, pIdx ) => cache._paramContainers.AcquireNew( pID => new CILParameterImpl(
                  cache._ctx,
                  pID,
                  new LazyWithLock<ListProxy<CILCustomAttribute>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>() ),
                  new SettableValueForEnums<ParameterAttributes>( ParameterAttributes.None ),
                  pIdx,
                  new SettableValueForClasses<String>( null ),
                  () => cache.ResolveMethodBaseID( methodID ),
                  () => cache.GetOrAdd( typeof( Int32 ) ),
                  new SettableLazy<Object>( () => null ),
                  new LazyWithLock<ListProxy<CILCustomModifier>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomModifier>() ),
                  new SettableLazy<MarshalingInfo>( () => null )
                  )
            );

            var curType = originalType as CILType;
            Stack<Int32> stk = null;
            if ( curType != null && ElementKind.Array == curType.ElementKind && curType.ArrayInformation != null )
            {
               stk = new Stack<Int32>();

               while ( curType != null && ElementKind.Array == curType.ElementKind )
               {
                  var aInfo = curType.ArrayInformation;
                  stk.Push( aInfo == null ? 0 : aInfo.Rank );
                  curType = curType.ElementType as CILType;
               }
            }

            Int32 amountOfParams;
            Int32 amountOfCtors;
            var dimSize = arrayInfo == null ? 1 : arrayInfo.Rank;
            if ( stk != null && dimSize == 1 )
            {
               amountOfCtors = dimSize == 1 ? 1 : 0;
               amountOfParams = 1;
               while ( stk.Count > 0 && stk.Pop() == 1 )
               {
                  ++amountOfCtors;
               }
            }
            else if ( stk == null && dimSize == 1 )
            {
               // Vector array
               amountOfCtors = 1;
               amountOfParams = 1;
            }
            else
            {
               amountOfCtors = 2;
               amountOfParams = dimSize == 1 && stk != null ? stk.Last() : dimSize;
            }

            return Enumerable.Range( 1, amountOfCtors ).Select( cIdx => (CILConstructor) cache._methodContainers.AcquireNew( ctorID => new CILConstructorImpl(
                  cache._ctx,
                  ctorID,
                  new LazyWithLock<ListProxy<CILCustomAttribute>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>() ),
                  new SettableValueForEnums<CallingConventions>( CallingConventions.HasThis | CallingConventions.Standard ),
                  new SettableValueForEnums<MethodAttributes>( MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.RTSpecialName ),
                  () => (CILType) cache.ResolveTypeID( elementType ),
                  () => cache._ctx.CollectionsFactory.NewListProxy<CILParameter>( Enumerable.Range( 0, amountOfParams * cIdx ).Select( pIdx => intParamFunc( ctorID, pIdx ) ).ToList() ),
                  null,
                  new SettableLazy<MethodImplAttributes>( () => MethodImplAttributes.IL ),
                  null,
                  false
               )
            ) ).ToArray();
         } );
         dic6.Add( ElementKind.Pointer, ( cache, originalType, elementType, arrayInfo ) => EMPTY_CTORS );
         dic6.Add( ElementKind.Reference, ( cache, originalType, elementType, arrayInfo ) => EMPTY_CTORS );
         ELEMENT_KIND_CTORS = dic6;

         var dic7 = new Dictionary<ElementKind, Func<CILReflectionContextCache, CILTypeBase, Int32, GeneralArrayInfo, CILProperty[]>>();
         dic7.Add( ElementKind.Array, ( cache, originalType, elementType, arrayInfo ) =>
         {
            return EMPTY_PROPERTIES;
         } );
         dic7.Add( ElementKind.Pointer, ( cache, originalType, elementType, arrayInfo ) => EMPTY_PROPERTIES );
         dic7.Add( ElementKind.Reference, ( cache, originalType, elementType, arrayInfo ) => EMPTY_PROPERTIES );
         ELEMENT_KIND_PROPERTIES = dic7;

         var dic8 = new Dictionary<ElementKind, Func<CILReflectionContextCache, CILTypeBase, Int32, GeneralArrayInfo, CILEvent[]>>();
         dic8.Add( ElementKind.Array, ( cache, originalType, elementType, arrayInfo ) => EMPTY_EVENTS );
         dic8.Add( ElementKind.Pointer, ( cache, originalType, elementType, arrayInfo ) => EMPTY_EVENTS );
         dic8.Add( ElementKind.Reference, ( cache, originalType, elementType, arrayInfo ) => EMPTY_EVENTS );
         ELEMENT_KIND_EVENTS = dic8;
      }

      private readonly SimpleConcurrentCache<System.Reflection.Assembly, CILAssembly> _assemblies;
      private readonly SimpleConcurrentCache<System.Reflection.Module, CILModule> _modules;
      private readonly SimpleConcurrentCache<Type, CILTypeBase> _types;
      private readonly SimpleConcurrentCache<System.Reflection.FieldInfo, CILField> _fields;
      private readonly SimpleConcurrentCache<System.Reflection.ConstructorInfo, CILConstructor> _ctors;
      private readonly SimpleConcurrentCache<System.Reflection.MethodInfo, CILMethod> _methods;
      private readonly SimpleConcurrentCache<System.Reflection.ParameterInfo, CILParameter> _params;
      private readonly SimpleConcurrentCache<System.Reflection.PropertyInfo, CILProperty> _properties;
      private readonly SimpleConcurrentCache<System.Reflection.EventInfo, CILEvent> _events;
      private readonly GenericInstanceCache<CILType> _genericTypes;
      private readonly GenericInstanceCache<CILMethod> _genericMethods;
      private readonly ElementCache _elementCache;
      private readonly GenericDeclaringTypeCache<CILField> _fieldsWithGenericDeclaringType;
      private readonly GenericDeclaringTypeCache<CILMethodBase> _methodsWithGenericDeclaringType;
      private readonly GenericDeclaringTypeCache<CILProperty> _propertiesWithGenericDeclaringType;
      private readonly GenericDeclaringTypeCache<CILEvent> _eventsWithGenericDeclaringType;

      private readonly ListHolder<CILAssembly> _assemblyContainers;
      private readonly ListHolder<CILModule> _moduleContainers;
      private readonly ListHolder<CILTypeOrTypeParameter> _typeContainers;
      private readonly ListHolder<CILField> _fieldContainers;
      private readonly ListHolder<CILMethodBase> _methodContainers;
      private readonly ListHolder<CILParameter> _paramContainers;
      private readonly ListHolder<CILProperty> _propertyContainers;
      private readonly ListHolder<CILEvent> _eventContainers;

      private readonly CILReflectionContextImpl _ctx;

      internal CILReflectionContextCache( CILReflectionContextImpl ctx )
      {
         ArgumentValidator.ValidateNotNull( "Reflection context", ctx );

         this._ctx = ctx;

         this._assemblyContainers = new ListHolder<CILAssembly>();
         this._moduleContainers = new ListHolder<CILModule>();
         this._typeContainers = new ListHolder<CILTypeOrTypeParameter>();
         this._fieldContainers = new ListHolder<CILField>();
         this._methodContainers = new ListHolder<CILMethodBase>();
         this._paramContainers = new ListHolder<CILParameter>();
         this._propertyContainers = new ListHolder<CILProperty>();
         this._eventContainers = new ListHolder<CILEvent>();

         this._assemblies = new SimpleConcurrentCache<System.Reflection.Assembly, CILAssembly>( this.CreateNewEmulatedAssemblyForNativeAssembly );
         this._modules = new SimpleConcurrentCache<System.Reflection.Module, CILModule>( this.CreateNewEmulatedModuleForNativeModule );
         this._types = new SimpleConcurrentCache<Type, CILTypeBase>( this.CreateNewEmulatedTypeForNativeType );
         this._fields = new SimpleConcurrentCache<System.Reflection.FieldInfo, CILField>( this.CreateNewEmulatedFieldForNativeField );
         this._ctors = new SimpleConcurrentCache<System.Reflection.ConstructorInfo, CILConstructor>( this.CreateNewEmulatedCtorForNativeCtor );
         this._methods = new SimpleConcurrentCache<System.Reflection.MethodInfo, CILMethod>( this.CreateNewEmulatedMethodForNativeMethod );
         this._params = new SimpleConcurrentCache<System.Reflection.ParameterInfo, CILParameter>( this.CreateNewEmulatedParameterForNativeParameter );
         this._properties = new SimpleConcurrentCache<System.Reflection.PropertyInfo, CILProperty>( this.CreateNewEmulatedPropertyForNativeProperty );
         this._events = new SimpleConcurrentCache<System.Reflection.EventInfo, CILEvent>( this.CreateNewEmulatedEventForNativeEvent );
         this._genericTypes = new GenericInstanceCache<CILType>( "type", this.CreateNewGenericInstance, type => type.GenericArguments.Count );
         this._genericMethods = new GenericInstanceCache<CILMethod>( "method", this.CreateNewGenericInstance, method => method.GenericArguments.Count );
         this._fieldsWithGenericDeclaringType = new GenericDeclaringTypeCache<CILField>( this.CreateNewFieldWithDifferentDeclaringTypeGArgs );
         this._methodsWithGenericDeclaringType = new GenericDeclaringTypeCache<CILMethodBase>( this.CreateNewMethodBaseWithDifferentDeclaringTypeGArgs );
         this._propertiesWithGenericDeclaringType = new GenericDeclaringTypeCache<CILProperty>( this.CreateNewPropertyWithDifferentDeclaringTypeGArgs );
         this._eventsWithGenericDeclaringType = new GenericDeclaringTypeCache<CILEvent>( this.CreateNewEventWithDifferentDeclaringTypeGArgs );
         this._elementCache = new ElementCache( this.CreateNewElementType );
      }

      private CILAssembly CreateNewEmulatedAssemblyForNativeAssembly( System.Reflection.Assembly ass )
      {
         return this._assemblyContainers.AcquireNew( id => new CILAssemblyImpl( this._ctx, id, ass ) );
      }

      private CILModule CreateNewEmulatedModuleForNativeModule( System.Reflection.Module mod )
      {
         return this._moduleContainers.AcquireNew( id => new CILModuleImpl( this._ctx, id, mod ) );
      }

      private CILTypeBase CreateNewEmulatedTypeForNativeType( Type type )
      {
         CILTypeBase result;
         if ( type.IsGenericParameter )
         {
            result = this._typeContainers.AcquireNew( id => new CILTypeParameterImpl( this._ctx, id, type ) );
         }
         else
         {
            if ( type
#if WINDOWS_PHONE_APP
               .GetTypeInfo()
#endif
.IsGenericTypeDefinition || !type
#if WINDOWS_PHONE_APP
               .GetTypeInfo()
#endif
.IsGenericType )
            {
               result = this._typeContainers.AcquireNew( id => new CILTypeImpl( this._ctx, id, type ) );
            }
            else
            {
               result = this.MakeGenericType( null, (CILType) this.GetOrAdd( type.GetGenericTypeDefinition() ), type.GetGenericArguments().Select( gArg => this.GetOrAdd( gArg ) ).ToArray() );
            }
         }
         return result;
      }

      private CILField CreateNewEmulatedFieldForNativeField( System.Reflection.FieldInfo field )
      {
         CILField result;
         var declType = field.DeclaringType;
         if ( declType
#if WINDOWS_PHONE_APP
               .GetTypeInfo()
#endif
.IsGenericTypeDefinition || !declType
#if WINDOWS_PHONE_APP
               .GetTypeInfo()
#endif
.IsGenericType )
         {
            result = this._fieldContainers.AcquireNew( id => new CILFieldImpl( this._ctx, id, field ) );
         }
         else
         {
            result = this.MakeFieldWithGenericDeclaringType(
               this.GetOrAdd( declType.GetGenericTypeDefinition().GetField( field.Name
#if !WINDOWS_PHONE_APP
, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly
#endif
 ) ),
               declType.GetGenericArguments().Select( gArg => this.GetOrAdd( gArg ) ).ToArray()
               );
         }
         return result;
      }

      private CILConstructor CreateNewEmulatedCtorForNativeCtor( System.Reflection.ConstructorInfo ctor )
      {
         CILMethodBase result;
         var declType = ctor.DeclaringType;
         if ( declType
#if WINDOWS_PHONE_APP
               .GetTypeInfo()
#endif
.IsGenericTypeDefinition || !declType
#if WINDOWS_PHONE_APP
               .GetTypeInfo()
#endif
.IsGenericType )
         {
            result = this._methodContainers.AcquireNew( id => new CILConstructorImpl( this._ctx, id, ctor ) );
         }
         else
         {
            var gDef = declType.GetGenericTypeDefinition();
            result = this.MakeMethodWithGenericDeclaringType(
               this.GetOrAdd( ChangeNativeGArgs( ctor, gDef ) ),
               declType.GetGenericArguments().Select( gArg => this.GetOrAdd( gArg ) ).ToArray()
               );
         }
         return (CILConstructor) result;
      }

      private CILMethod CreateNewEmulatedMethodForNativeMethod( System.Reflection.MethodInfo method )
      {
         CILMethodBase result;
         if ( method.IsGenericMethodDefinition || !method.GetGenericArguments().Any() )
         {
            var declType = method.DeclaringType;
            if ( declType == null || declType
#if WINDOWS_PHONE_APP
               .GetTypeInfo()
#endif
.IsGenericTypeDefinition || !declType
#if WINDOWS_PHONE_APP
               .GetTypeInfo()
#endif
.IsGenericType )
            {
               result = this._methodContainers.AcquireNew( id => new CILMethodImpl( this._ctx, id, method ) );
            }
            else
            {
               var gDef = declType.GetGenericTypeDefinition();
               result = this.MakeMethodWithGenericDeclaringType(
                  this.GetOrAdd( ChangeNativeGArgs( method, gDef ) ),
                  declType.GetGenericArguments().Select( gArg => this.GetOrAdd( gArg ) ).ToArray()
                  );
            }
         }
         else
         {
            var gDef = method.GetGenericMethodDefinition();
            result = this.MakeGenericMethod( null, (CILMethod) this.GetOrAdd( gDef ), method.GetGenericArguments().Select( gArg => this.GetOrAdd( gArg ) ).ToArray() );
         }
         return (CILMethod) result;
      }

      private static TMethod ChangeNativeGArgs<TMethod>( TMethod method, Type type )
         where TMethod : System.Reflection.MethodBase
      {
         return
            // TODO hmmm... there is no .MethodHandle property in windows phone apps... how to get matching methods?
            // Consider this:
            // public class A<T>
            // {
            //    void Method(T t);
            //    void Method(String t);
            // }
            // If we get instance of Method2 with declaring type A<String>, how to pick the correct method without usage of GetMethodFromHandle ?
#if WINDOWS_PHONE_APP
         null
#else
 (TMethod) System.Reflection.MethodBase.GetMethodFromHandle( method.MethodHandle, type.TypeHandle )
#endif
;
      }

      private CILParameter CreateNewEmulatedParameterForNativeParameter( System.Reflection.ParameterInfo param )
      {
         var method = (System.Reflection.MethodBase) param.Member;
         var declType = method.DeclaringType;
         var isMethodPlain = method.IsGenericMethodDefinition || method is System.Reflection.ConstructorInfo || !method.GetGenericArguments().Any();
         var isDeclTypePlain = declType == null || declType
#if WINDOWS_PHONE_APP
               .GetTypeInfo()
#endif
.IsGenericTypeDefinition || !declType
#if WINDOWS_PHONE_APP
               .GetTypeInfo()
#endif
.IsGenericType;
         CILParameter result;
         if ( isMethodPlain && isDeclTypePlain )
         {
            result = this._paramContainers.AcquireNew( id => new CILParameterImpl( this._ctx, id, param ) );
         }
         else
         {
            var ctor = method as System.Reflection.ConstructorInfo;
            var cilMethod = ctor == null ? this.GetOrAdd( (System.Reflection.MethodInfo) method ) : (CILMethodBase) this.GetOrAdd( (System.Reflection.ConstructorInfo) method );
            result = param.Position >= 0 ? cilMethod.Parameters[param.Position] : ( (CILMethod) cilMethod ).ReturnParameter;
         }
         return result;
      }

      private CILProperty CreateNewEmulatedPropertyForNativeProperty( System.Reflection.PropertyInfo prop )
      {
         CILProperty result;
         var declType = prop.DeclaringType;
         if ( declType
#if WINDOWS_PHONE_APP
               .GetTypeInfo()
#endif
.IsGenericTypeDefinition || !declType
#if WINDOWS_PHONE_APP
               .GetTypeInfo()
#endif
.IsGenericType )
         {
            result = this._propertyContainers.AcquireNew( id => new CILPropertyImpl( this._ctx, id, prop ) );
         }
         else
         {
            result = this.MakePropertyWithGenericType(
               this.GetOrAdd( declType.GetGenericTypeDefinition().GetProperty( prop.Name
#if !WINDOWS_PHONE_APP
, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly
#endif
 ) ),
               declType.GetGenericArguments().Select( gArg => this.GetOrAdd( gArg ) ).ToArray()
               );
         }
         return result;
      }

      private CILEvent CreateNewEmulatedEventForNativeEvent( System.Reflection.EventInfo evt )
      {
         CILEvent result;
         var declType = evt.DeclaringType;
         if ( declType
#if WINDOWS_PHONE_APP
               .GetTypeInfo()
#endif
.IsGenericTypeDefinition || !declType
#if WINDOWS_PHONE_APP
               .GetTypeInfo()
#endif
.IsGenericType )
         {
            result = this._eventContainers.AcquireNew( id => new CILEventImpl( this._ctx, id, evt ) );
         }
         else
         {
            result = this.MakeEventWithGenericType(
               this.GetOrAdd( declType.GetGenericTypeDefinition().GetEvent( evt.Name
#if !WINDOWS_PHONE_APP
, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly
#endif
 ) ),
               declType.GetGenericArguments().Select( gArg => this.GetOrAdd( gArg ) ).ToArray()
               );
         }
         return result;
      }

      private static void CheckGenericArgumentsWhenCreating( CILTypeBase[] gArgs )
      {
         for ( var i = 0; i < gArgs.Length; ++i )
         {
            if ( gArgs[i] == null )
            {
               throw new ArgumentNullException( "The generic argument at index " + i + " was null." );
            }
         }
      }

      private CILType CreateNewGenericInstance( CILType gDef, CILTypeBase[] gArgs )
      {
         CheckGenericArgumentsWhenCreating( gArgs );
         CILType result;
         if ( gArgs.All( gArg => gArg.TypeKind == TypeKind.TypeParameter && Object.ReferenceEquals( ( (CILTypeParameter) gArg ).DeclaringType, gDef ) && ( (CILTypeParameter) gArg ).DeclaringMethod == null ) )
         {
            result = gDef;
         }
         else
         {
            result = (CILType) this._typeContainers.AcquireNew( id => new CILTypeImpl(
                  this._ctx,
                  id,
                  ( (CommonFunctionality) gDef ).CustomAttributeList,
                  () => CILTypeCode.Object,
                  ( (CILTypeInternal) gDef ).NameInternal,
                  ( (CILTypeInternal) gDef ).NamespaceInternal,
                  () => gDef.Module,
                  () => gDef.DeclaringType,
                  () => this.ReplaceGArgParameterCILType( gDef, gDef.BaseType, gArgs ),
                  () => this._ctx.CollectionsFactory.NewListProxy<CILType>(
                     gDef.DeclaredInterfaces.Select( iFace => this.ReplaceGArgParameterCILType( gDef, (CILType) iFace, gArgs ) ).ToList() ),
                  ( (CILTypeInternal) gDef ).TypeAttributesInternal,
                  null,
                  null,
                  () => this._ctx.CollectionsFactory.NewListProxy<CILTypeBase>( gArgs.ToList() ),
                  () => gDef,
                  ( (CILTypeInternal) gDef ).NestedTypesInternal,
                  () => this._ctx.CollectionsFactory.NewListProxy<CILField>( gDef.DeclaredFields.Select( field => this.MakeFieldWithGenericDeclaringType( field, gArgs ) ).ToList() ),
                  () => null,
                  () => this._ctx.CollectionsFactory.NewListProxy<CILMethod>( gDef.DeclaredMethods.Select( method => (CILMethod) this.MakeMethodWithGenericDeclaringType( method, gArgs ) ).ToList() ),
                  () => this._ctx.CollectionsFactory.NewListProxy<CILConstructor>( gDef.Constructors.Select( ctor => (CILConstructor) this.MakeMethodWithGenericDeclaringType( ctor, gArgs ) ).ToList() ),
                  () => this._ctx.CollectionsFactory.NewListProxy<CILProperty>( gDef.DeclaredProperties.Select( property => this.MakePropertyWithGenericType( property, gArgs ) ).ToList() ),
                  () => this._ctx.CollectionsFactory.NewListProxy<CILEvent>( gDef.DeclaredEvents.Select( evt => this.MakeEventWithGenericType( evt, gArgs ) ).ToList() ),
                  ( (CILTypeInternal) gDef ).ClassLayoutInternal,
                  ( (CILTypeImpl) gDef ).DeclarativeSecurityInternal
                  )
            );
         }
         return result;
      }

      private CILMethod CreateNewGenericInstance( CILMethod gDef, CILTypeBase[] gArgs )
      {
         CheckGenericArgumentsWhenCreating( gArgs );

         var methodGArgDeclaringMethod = gDef;
         var declGDef = gDef.DeclaringType.GenericDefinition;
         if ( declGDef != null )
         {
            methodGArgDeclaringMethod = ( (CILTypeParameter) gDef.GenericArguments[0] ).DeclaringMethod;
         }

         CILMethod result = null;
         if ( gArgs.All( gArg => gArg.TypeKind == TypeKind.TypeParameter && Object.ReferenceEquals( ( (CILTypeParameter) gArg ).DeclaringMethod, methodGArgDeclaringMethod ) ) )
         {
            result = gDef;
         }
         else
         {
            Func<Int32, CILParameter, CILParameter> newParamFunc = ( mID, param ) => this._paramContainers.AcquireNew( pID => new CILParameterImpl(
                  this._ctx,
                  pID,
                 ( (CommonFunctionality) param ).CustomAttributeList,
                  ( (CILParameterInternal) param ).ParameterAttributesInternal,
                  ( (CILParameterInternal) param ).ParameterPositionInternal,
                  ( (CILParameterInternal) param ).NameInternal,
                  () => this.ResolveMethodBaseID( mID ),
                  () => this.ReplaceGArgParameter( methodGArgDeclaringMethod, param.ParameterType, gArgs ),
                  ( (CILParameterInternal) param ).ConstantValueInternal,
                  ( (CILParameterInternal) param ).CustomModifierList,
                  ( (CILElementWithMarshalInfoInternal) param ).MarshalingInfoInternal
                  ) );
            result = (CILMethod) this._methodContainers.AcquireNew( id => new CILMethodImpl(
                  this._ctx,
                  id,
                  ( (CommonFunctionality) gDef ).CustomAttributeList,
                  ( (CILMethodInternal) gDef ).CallingConventionInternal,
                  ( (CILMethodInternal) gDef ).MethodAttributesInternal,
                  () => gDef.DeclaringType,
                  () => this._ctx.CollectionsFactory.NewListProxy<CILParameter>( gDef.Parameters.Select( param => newParamFunc( id, param ) ).ToList() ),
                  ( (CILMethodBaseInternal) gDef ).MethodImplementationAttributesInternal,
                  ( (CILMethodBaseImpl) gDef ).DeclarativeSecurityInternal,
                  ( (CILMethodInternal) gDef ).NameInternal,
                  () => newParamFunc( id, gDef.ReturnParameter ),
                  () => this._ctx.CollectionsFactory.NewListProxy<CILTypeBase>( gArgs.ToList() ),
                  () => gDef
                  ) );
         }
         return result;
      }

      private CILTypeBase CreateNewElementType( CILTypeBase type, ElementKind kind, GeneralArrayInfo arrayInfo )
      {
         return this._typeContainers.AcquireNew( id => new CILTypeImpl(
               this._ctx,
               id,
               new LazyWithLock<ListProxy<CILCustomAttribute>>( () => this._ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>() ),
               () => CILTypeCode.Object,
               ( (CILTypeBaseInternal) type ).NameInternal,
               ( (CILTypeBaseInternal) type ).NamespaceInternal,
               () => type.Module,
               () => null,
               () => ELEMENT_KIND_BASE_TYPES[kind] == null ? null : type.Module.AssociatedMSCorLibModule.GetTypeByName( ELEMENT_KIND_BASE_TYPES[kind].FullName ),
               () => this._ctx.CollectionsFactory.NewListProxy<CILType>( ELEMENT_KIND_INTERFACES[kind]( this, type, arrayInfo ).ToList() ),
               new SettableValueForEnums<TypeAttributes>( ELEMENT_KIND_ATTRIBUTES[kind] ),
               kind,
               arrayInfo,
               () => this._ctx.CollectionsFactory.NewListProxy<CILTypeBase>(),
               () => null,
               new LazyWithLock<ListProxy<CILType>>( () => this._ctx.CollectionsFactory.NewListProxy<CILType>() ),
               () => this._ctx.CollectionsFactory.NewListProxy<CILField>(),
               () => type,
               () => this._ctx.CollectionsFactory.NewListProxy<CILMethod>( ELEMENT_KIND_METHODS[kind]( this, type, id, arrayInfo ).ToList() ),
               () => this._ctx.CollectionsFactory.NewListProxy<CILConstructor>( ELEMENT_KIND_CTORS[kind]( this, type, id, arrayInfo ).ToList() ),
               () => this._ctx.CollectionsFactory.NewListProxy<CILProperty>( ELEMENT_KIND_PROPERTIES[kind]( this, type, id, arrayInfo ).ToList() ),
               () => this._ctx.CollectionsFactory.NewListProxy<CILEvent>( ELEMENT_KIND_EVENTS[kind]( this, type, id, arrayInfo ).ToList() ),
               new SettableLazy<ClassLayout?>( () => null ),
               null
            ) );
      }

      private CILField CreateNewFieldWithDifferentDeclaringTypeGArgs( CILType gDef, CILField field, CILTypeBase[] gArgs )
      {
         return this._fieldContainers.AcquireNew( id => new CILFieldImpl(
                     this._ctx,
                     id,
                     ( (CommonFunctionality) field ).CustomAttributeList,
                     ( (CILFieldInternal) field ).FieldAttributesInternal,
                     ( (CILFieldInternal) field ).NameInternal,
                     () => this.MakeGenericType( gDef, gDef, gArgs ),
                     () => this.ReplaceGArgParameter( gDef, field.FieldType, gArgs ),
                     ( (CILFieldInternal) field ).ConstantValueInternal,
                     ( (CILFieldInternal) field ).FieldRVAValue,
                     ( (CILFieldInternal) field ).CustomModifierList,
                     ( (CILFieldInternal) field ).FieldOffsetInternal,
                     ( (CILElementWithMarshalInfoInternal) field ).MarshalingInfoInternal
                     ) );
      }

      private CILProperty CreateNewPropertyWithDifferentDeclaringTypeGArgs( CILType gDef, CILProperty prop, CILTypeBase[] gArgs )
      {
         return this._propertyContainers.AcquireNew( id => new CILPropertyImpl(
               this._ctx,
               id,
               ( (CommonFunctionality) prop ).CustomAttributeList,
               ( (CILPropertyInternal) prop ).NameInternal,
               ( (CILPropertyInternal) prop ).PropertyAttributesInternal,
               () => (CILMethod) this.MakeMethodWithGenericDeclaringTypeOrNull( prop.SetMethod, gArgs ),
               () => (CILMethod) this.MakeMethodWithGenericDeclaringTypeOrNull( prop.GetMethod, gArgs ),
               () => this.MakeGenericType( gDef, gDef, gArgs ),
               ( (CILPropertyInternal) prop ).ConstantValueInternal,
               ( (CILPropertyInternal) prop ).CustomModifierList
            ) );
      }

      private CILEvent CreateNewEventWithDifferentDeclaringTypeGArgs( CILType gDef, CILEvent evt, CILTypeBase[] gArgs )
      {
         return this._eventContainers.AcquireNew( id => new CILEventImpl(
               this._ctx,
               id,
               ( (CommonFunctionality) evt ).CustomAttributeList,
               ( (CILEventInternal) evt ).NameInternal,
               ( (CILEventInternal) evt ).EventAttributesInternal,
               () => this.ReplaceGArgParameter( gDef, evt.EventHandlerType, gArgs ),
               () => (CILMethod) this.MakeMethodWithGenericDeclaringTypeOrNull( evt.AddMethod, gArgs ),
               () => (CILMethod) this.MakeMethodWithGenericDeclaringTypeOrNull( evt.RemoveMethod, gArgs ),
               () => (CILMethod) this.MakeMethodWithGenericDeclaringTypeOrNull( evt.RaiseMethod, gArgs ),
               () => this._ctx.CollectionsFactory.NewListProxy<CILMethod>( evt.OtherMethods.Select( method => (CILMethod) this.MakeMethodWithGenericDeclaringType( method, gArgs ) ).ToList() ),
               () => this.MakeGenericType( gDef, gDef, gArgs )
               ) );
      }

      private CILMethodBase CreateNewMethodBaseWithDifferentDeclaringTypeGArgs( CILType gDef, CILMethodBase method, CILTypeBase[] gArgs )
      {
         Func<Int32, CILParameter, CILParameter> newParamFunc = ( mID, param ) => this._paramContainers.AcquireNew( pID => new CILParameterImpl(
               this._ctx,
               pID,
               ( (CommonFunctionality) param ).CustomAttributeList,
               ( (CILParameterInternal) param ).ParameterAttributesInternal,
               ( (CILParameterInternal) param ).ParameterPositionInternal,
               ( (CILParameterInternal) param ).NameInternal,
               () => this.ResolveMethodBaseID( mID ),
               () => this.ReplaceGArgParameter( gDef, param.ParameterType, gArgs ),
               ( (CILParameterInternal) param ).ConstantValueInternal,
               ( (CILParameterInternal) param ).CustomModifierList,
               ( (CILElementWithMarshalInfoInternal) param ).MarshalingInfoInternal
               ) );

         var gDefMethodAsMethod = method as CILMethod;
         return this._methodContainers.AcquireNew( id =>
         {
            if ( gDefMethodAsMethod == null )
            {
               return new CILConstructorImpl(
                  this._ctx,
                  id,
                  ( (CommonFunctionality) method ).CustomAttributeList,
                  ( (CILMethodBaseInternal) method ).CallingConventionInternal,
                  ( (CILMethodBaseInternal) method ).MethodAttributesInternal,
                  () => this.MakeGenericType( gDef, gDef, gArgs ),
                  () => this._ctx.CollectionsFactory.NewListProxy<CILParameter>( method.Parameters.Select( param => newParamFunc( id, param ) ).ToList() ),
                  null,
                  ( (CILMethodBaseInternal) method ).MethodImplementationAttributesInternal,
                  ( (CILMethodBaseImpl) method ).DeclarativeSecurityInternal,
                  false
                  );
            }
            else
            {
               return new CILMethodImpl(
                  this._ctx,
                  id,
                  ( (CommonFunctionality) method ).CustomAttributeList,
                  ( (CILMethodBaseInternal) method ).CallingConventionInternal,
                  ( (CILMethodBaseInternal) method ).MethodAttributesInternal,
                  () => this.MakeGenericType( gDef, gDef, gArgs ),
                  () => this._ctx.CollectionsFactory.NewListProxy<CILParameter>( method.Parameters.Select( param => newParamFunc( id, param ) ).ToList() ),
                  ( (CILMethodBaseInternal) method ).MethodImplementationAttributesInternal,
                  ( (CILMethodBaseImpl) method ).DeclarativeSecurityInternal,
                  ( (CILMethodInternal) method ).NameInternal,
                  () => newParamFunc( id, gDefMethodAsMethod.ReturnParameter ),
                  () => ( (CILElementWithGenericArgs) gDefMethodAsMethod ).InternalGenericArguments,
                  () => gDefMethodAsMethod.IsGenericMethodDefinition() ? (CILMethod) this.ResolveMethodBaseID( id ) : null
                  );
            }
         } );
      }

      internal CILAssembly GetOrAdd( System.Reflection.Assembly ass )
      {
         return this._assemblies.GetOrAdd( ass );
      }

      internal CILModule GetOrAdd( System.Reflection.Module mod )
      {
         return this._modules.GetOrAdd( mod );
      }

      internal CILTypeBase GetOrAdd( Type type )
      {
         CILTypeBase result = null;
         if ( type != null )
         {
            var kind = type.GetElementKind();
            if ( kind.HasValue )
            {
               var stack = new Stack<Tuple<ElementKind, GeneralArrayInfo>>();
               do
               {
                  stack.Push( Tuple.Create( kind.Value, ElementKind.Array == kind.Value ? ( type.IsVectorArray() ? null : new GeneralArrayInfo( type.GetArrayRank(), null, null ) ) : null ) );
                  type = type.GetElementType();
                  kind = type.GetElementKind();
               } while ( kind.HasValue );
               result = this.GetOrAdd( type );
               while ( stack.Any() )
               {
                  var tuple = stack.Pop();
                  // TODO does C# reflection API support getting array lower bounds and sizes? e.g. through .ToString or .FullName ?
                  result = this.MakeElementType( result, tuple.Item1, tuple.Item2 );
               }
            }
            else
            {
               result = this._types.GetOrAdd( type );
            }
         }
         return result;
      }

      internal CILField GetOrAdd( System.Reflection.FieldInfo field )
      {
         return this._fields.GetOrAdd( field );
      }

      internal CILMethod GetOrAdd( System.Reflection.MethodInfo method )
      {
         return this._methods.GetOrAdd( method );
      }

      internal CILConstructor GetOrAdd( System.Reflection.ConstructorInfo ctor )
      {
         return this._ctors.GetOrAdd( ctor );
      }

      internal CILParameter GetOrAdd( System.Reflection.ParameterInfo param )
      {
         return this._params.GetOrAdd( param );
      }

      internal CILProperty GetOrAdd( System.Reflection.PropertyInfo property )
      {
         return this._properties.GetOrAdd( property );
      }

      internal CILEvent GetOrAdd( System.Reflection.EventInfo evt )
      {
         return this._events.GetOrAdd( evt );
      }

      internal CILType MakeGenericType( CILType thisType, CILType gDef, params CILTypeBase[] gArgs )
      {
         return this._genericTypes.MakeGenericInstance( thisType, gDef, gArgs );
      }

      internal CILMethod MakeGenericMethod( CILMethod thisMethod, CILMethod gDef, params CILTypeBase[] gArgs )
      {
         return this._genericMethods.MakeGenericInstance( thisMethod, gDef, gArgs );
      }

      internal CILField MakeFieldWithGenericDeclaringType( CILField gDefField, params CILTypeBase[] gArgs )
      {
         return this._fieldsWithGenericDeclaringType.GetOrAdd( gDefField, gArgs );
      }

      private CILMethodBase MakeMethodWithGenericDeclaringTypeOrNull( CILMethodBase method, params CILTypeBase[] gArgs )
      {
         return method == null ? null : this.MakeMethodWithGenericDeclaringType( method, gArgs );
      }

      internal CILMethodBase MakeMethodWithGenericDeclaringType( CILMethodBase method, params CILTypeBase[] gArgs )
      {
         var mKind = method.MethodKind;
         CILMethod gDefMethodAsMethod = null;
         CILTypeBase[] oldGArgs = null;
         if ( MethodKind.Method == mKind )
         {
            gDefMethodAsMethod = (CILMethod) method;

            var gDef = gDefMethodAsMethod.GenericDefinition;

            if ( gDef != null )
            {
               oldGArgs = gDefMethodAsMethod.GenericArguments.ToArray();
               gDefMethodAsMethod = ( (CILTypeParameter) gDef.GenericArguments[0] ).DeclaringMethod;
               method = gDefMethodAsMethod;
            }
         }
         var result = this._methodsWithGenericDeclaringType.GetOrAdd( method, gArgs );

         if ( gDefMethodAsMethod != null && oldGArgs != null )
         {
            var gDefType = method.DeclaringType.GenericDefinition;
            if ( gDefType != null )
            {
               result = this.MakeGenericMethod( (CILMethod) result, (CILMethod) result, oldGArgs.Select( arg => this.ReplaceGArgParameter( gDefType, arg, gArgs ) ).ToArray() );
            }
         }

         return result;
      }

      internal CILProperty MakePropertyWithGenericType( CILProperty property, params CILTypeBase[] gArgs )
      {
         return this._propertiesWithGenericDeclaringType.GetOrAdd( property, gArgs );
      }

      internal CILEvent MakeEventWithGenericType( CILEvent evt, params CILTypeBase[] gArgs )
      {
         return this._eventsWithGenericDeclaringType.GetOrAdd( evt, gArgs );
      }

      internal CILType MakeElementType( CILTypeBase type, ElementKind kind, GeneralArrayInfo arrayInfo )
      {
         return this._elementCache.MakeElementType( type, kind, arrayInfo );
      }

      private CILType ReplaceGArgParameterCILType( CILElementWithGenericArguments<Object> gDef, CILType gType, CILTypeBase[] newGArgs )
      {
         return (CILType) this.ReplaceGArgParameter( gDef, gType, newGArgs );
      }

      private CILTypeBase ReplaceGArgParameter( CILElementWithGenericArguments<Object> gDef, CILTypeBase gType, CILTypeBase[] newGArgs )
      {
         CILTypeBase result = null;
         if ( gType != null )
         {
            if ( gType.TypeKind == TypeKind.TypeParameter )
            {
               if ( ( gDef.Equals( ( (CILTypeParameter) gType ).DeclaringType.GenericDefinition ) && ( (CILTypeParameter) gType ).DeclaringMethod == null ) || gDef.Equals( ( (CILTypeParameter) gType ).DeclaringMethod ) )
               {
                  result = newGArgs[( (CILTypeParameter) gType ).GenericParameterPosition];
               }
               else
               {
                  result = gType;
               }
            }
            else
            {
               if ( gType.ContainsGenericParameters() )
               {
                  var kind = gType.GetElementKind();
                  if ( kind.HasValue )
                  {
                     result = this.DoSomethingForRootElementType( gType, kind, gTypeArg => this.ReplaceGArgParameter( gDef, gTypeArg, newGArgs ) );
                  }
                  else if ( TypeKind.MethodSignature != gType.TypeKind )
                  {
                     var gTypee = (CILType) gType;
                     result = this.MakeGenericType( gTypee.GenericDefinition, gTypee.GenericDefinition, gTypee.GenericArguments.Select( gArg => this.ReplaceGArgParameter( gDef, gArg, newGArgs ) ).ToArray() );
                  }
                  else
                  {
                     // TODO
                  }
               }
               else
               {
                  result = gType;
               }
            }
         }
         return result;
      }

      private CILTypeBase DoSomethingForRootElementType( CILTypeBase gType, ElementKind? kind, Func<CILTypeBase, CILTypeBase> func )
      {
         var elements = new Stack<Tuple<ElementKind, GeneralArrayInfo>>();
         while ( kind.HasValue )
         {
            var gtc = (CILType) gType;
            elements.Push( Tuple.Create( kind.Value, gtc.ArrayInformation ) );
            gType = gtc.ElementType;
            if ( gType.TypeKind == TypeKind.Type )
            {
               kind = ( (CILType) gType ).ElementKind;
            }
            else
            {
               kind = null;
            }
         }
         var result = func( gType );
         while ( elements.Count > 0 )
         {
            var tuple = elements.Pop();
            result = this.MakeElementType( result, tuple.Item1, tuple.Item2 );
         }
         return result;
      }

      internal CILAssembly NewBlankAssembly()
      {
         return this._assemblyContainers.AcquireNew( id => new CILAssemblyImpl( this._ctx, id ) );
      }

      internal CILAssembly NewAssembly( Func<Int32, CILAssembly> creator )
      {
         return this._assemblyContainers.AcquireNew( creator );
      }

      internal CILModule NewBlankModule( CILAssembly ass, String name )
      {
         return this._moduleContainers.AcquireNew( id => new CILModuleImpl( this._ctx, id, ass, name ) );
      }

      internal CILModule NewModule( Func<Int32, CILModule> creator )
      {
         return this._moduleContainers.AcquireNew( creator );
      }

      internal CILType NewBlankType( CILModule owner, CILType declType, String name, TypeAttributes attrs, CILTypeCode tc )
      {
         return (CILType) this._typeContainers.AcquireNew( id => new CILTypeImpl( this._ctx, id, tc, owner, name, declType, attrs ) );
      }

      internal CILType NewType( Func<Int32, CILType> creator )
      {
         return (CILType) this._typeContainers.AcquireNew( creator );
      }

      internal CILParameter NewBlankParameter( CILMethodBase ownerMethod, Int32 position, String name, ParameterAttributes attrs, CILTypeBase paramType )
      {
         return this._paramContainers.AcquireNew( id => new CILParameterImpl( this._ctx, id, ownerMethod, position, name, attrs, paramType ) );
      }

      internal CILParameter NewParameter( Func<Int32, CILParameter> creator )
      {
         return this._paramContainers.AcquireNew( creator );
      }

      internal CILConstructor NewBlankConstructor( CILType ownerType, MethodAttributes attrs )
      {
         return (CILConstructor) this._methodContainers.AcquireNew( id => new CILConstructorImpl( this._ctx, id, ownerType, attrs ) );
      }

      internal CILConstructor NewConstructor( Func<Int32, CILConstructor> creator )
      {
         return (CILConstructor) this._methodContainers.AcquireNew( creator );
      }

      internal CILMethod NewBlankMethod( CILType ownerType, String name, MethodAttributes attrs, CallingConventions callingConventions )
      {
         return (CILMethod) this._methodContainers.AcquireNew( id => new CILMethodImpl( this._ctx, id, ownerType, name, attrs, callingConventions ) );
      }

      internal CILMethod NewMethod( Func<Int32, CILMethod> creator )
      {
         return (CILMethod) this._methodContainers.AcquireNew( creator );
      }

      internal CILField NewBlankField( CILType ownerType, String name, FieldAttributes attrs, CILTypeBase fieldType )
      {
         return this._fieldContainers.AcquireNew( id => new CILFieldImpl( this._ctx, id, ownerType, name, fieldType, attrs ) );
      }

      internal CILField NewField( Func<Int32, CILField> creator )
      {
         return this._fieldContainers.AcquireNew( creator );
      }

      internal CILTypeParameter NewBlankTypeParameter( CILType declaringType, CILMethod declaringMethod, String name, Int32 position )
      {
         return (CILTypeParameter) this._typeContainers.AcquireNew( id => new CILTypeParameterImpl( this._ctx, id, new LazyWithLock<ListProxy<CILCustomAttribute>>( () => this._ctx.CollectionsFactory.NewListProxy( new List<CILCustomAttribute>() ) ), GenericParameterAttributes.None, declaringType, declaringMethod, name, position, () => this._ctx.CollectionsFactory.NewListProxy( new List<CILTypeBase>() ) ) );
      }

      internal CILTypeParameter NewTypeParameter( Func<Int32, CILTypeParameter> creator )
      {
         return (CILTypeParameter) this._typeContainers.AcquireNew( creator );
      }

      internal CILEvent NewBlankEvent( CILType declaringType, String name, EventAttributes attrs, CILTypeBase evtType )
      {
         return this._eventContainers.AcquireNew( id => new CILEventImpl( this._ctx, id, declaringType, name, attrs, evtType ) );
      }

      internal CILEvent NewEvent( Func<Int32, CILEvent> creator )
      {
         return this._eventContainers.AcquireNew( creator );
      }

      internal CILProperty NewBlankProperty( CILType declaringType, String name, PropertyAttributes attrs )
      {
         return this._propertyContainers.AcquireNew( id => new CILPropertyImpl( this._ctx, id, declaringType, name, attrs ) );
      }

      internal CILProperty NewProperty( Func<Int32, CILProperty> creator )
      {
         return this._propertyContainers.AcquireNew( creator );
      }

      internal CILCustomAttributeContainer ResolveAnyID( CILElementKind kind, Int32 id )
      {
         switch ( kind )
         {
            case CILElementKind.Assembly:
               return this.ResolveAssemblyID( id );
            case CILElementKind.Module:
               return this.ResolveModuleID( id );
            case CILElementKind.Type:
               return this.ResolveTypeID( id );
            case CILElementKind.Field:
               return this.ResolveFieldID( id );
            case CILElementKind.Method:
            case CILElementKind.Constructor:
               return this.ResolveMethodBaseID( id );
            case CILElementKind.Parameter:
               return this.ResolveParameterID( id );
            case CILElementKind.Property:
               return this.ResolvePropertyID( id );
            case CILElementKind.Event:
               return this.ResolveEventID( id );
            default:
               throw new ArgumentException( "Unknown element kind: " + kind );
         }
      }

      internal CILAssembly ResolveAssemblyID( Int32 id )
      {
         return NO_ID == id ? null : this._assemblyContainers[id];
      }


      internal CILModule ResolveModuleID( Int32 id )
      {
         return NO_ID == id ? null : this._moduleContainers[id];
      }


      internal CILTypeOrTypeParameter ResolveTypeID( Int32 id )
      {
         return NO_ID == id ? null : this._typeContainers[id];
      }

      internal CILField ResolveFieldID( Int32 id )
      {
         return NO_ID == id ? null : this._fieldContainers[id];
      }

      internal CILMethodBase ResolveMethodBaseID( Int32 id )
      {
         return NO_ID == id ? null : this._methodContainers[id];
      }

      internal CILParameter ResolveParameterID( Int32 id )
      {
         return NO_ID == id ? null : this._paramContainers[id];
      }

      internal CILProperty ResolvePropertyID( Int32 id )
      {
         return NO_ID == id ? null : this._propertyContainers[id];
      }

      internal CILEvent ResolveEventID( Int32 id )
      {
         return NO_ID == id ? null : this._eventContainers[id];
      }

      internal void ForAllGenericInstancesOf( CILType gDef, Action<CILTypeInternal> action )
      {
         this._genericTypes.ForAllInstancesOf( gDef, action );
      }

      internal void ForAllGenericInstancesOf<TMethod, TMethodInternal>( TMethod gDefOrCtor, Action<TMethodInternal> action )
         where TMethod : CILMethodBase
         where TMethodInternal : class
      {
         var method = gDefOrCtor as CILMethod;
         if ( method == null )
         {
            this._methodsWithGenericDeclaringType.ForAllInstancesOf( gDefOrCtor, action );
         }
         else
         {
            // TODO - acquire all locks or make a common lock for genericMethods and methodWithGenericDeclaringType for this particular method
            // Actually - since these methods should only reset lazies, this should work too, I think
            this._genericMethods.ForAllInstancesOf( method, action );
            this._methodsWithGenericDeclaringType.ForAllInstancesOf( gDefOrCtor, new Action<TMethodInternal>( casted =>
            {
               action( casted );
               this._genericMethods.ForAllInstancesOf( (CILMethod) casted, action );
            } ) );
         }
      }

      internal void ForAllGenericInstancesOf( CILProperty prop, Action<CILPropertyInternal> action )
      {
         this._propertiesWithGenericDeclaringType.ForAllInstancesOf( prop, action );
      }

      internal void ForAllGenericInstancesOf( CILEvent evt, Action<CILEventInternal> action )
      {
         this._eventsWithGenericDeclaringType.ForAllInstancesOf( evt, action );
      }

      internal void ForAllGenericInstancesOf( CILField field, Action<CILFieldInternal> action )
      {
         this._fieldsWithGenericDeclaringType.ForAllInstancesOf( field, action );
      }

      public void Dispose()
      {
         this._assemblyContainers.Dispose();
         this._moduleContainers.Dispose();
         this._typeContainers.Dispose();
         this._fieldContainers.Dispose();
         this._methodContainers.Dispose();
         this._paramContainers.Dispose();
         this._propertyContainers.Dispose();
         this._eventContainers.Dispose();

         this._genericTypes.Dispose();
         this._genericMethods.Dispose();
         this._fieldsWithGenericDeclaringType.Dispose();
         this._methodsWithGenericDeclaringType.Dispose();
         this._propertiesWithGenericDeclaringType.Dispose();
         this._eventsWithGenericDeclaringType.Dispose();
      }
   }
}