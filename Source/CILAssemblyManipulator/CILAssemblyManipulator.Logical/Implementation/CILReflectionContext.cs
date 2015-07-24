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
   internal class CILReflectionContextImpl : AbstractDisposable, CILReflectionContext
   {
      private sealed class CILAssemblyNameEqualityComparer : AbstractDisposable, IEqualityComparer<CILAssemblyName>
      {
         private readonly Lazy<HashStreamInfo> _publicKeyComputer;

         internal CILAssemblyNameEqualityComparer( CryptoCallbacks cryptoCallbacks )
         {
            this._publicKeyComputer = cryptoCallbacks == null ? null : new Lazy<HashStreamInfo>( () => cryptoCallbacks.CreateHashStream( AssemblyHashAlgorithm.SHA1 ), LazyThreadSafetyMode.None );
         }

         Boolean IEqualityComparer<CILAssemblyName>.Equals( CILAssemblyName x, CILAssemblyName y )
         {
            Boolean retVal;
            var xa = x.AssemblyInformation;
            var ya = y.AssemblyInformation;
            if ( x.Flags.IsFullPublicKey() == y.Flags.IsFullPublicKey() )
            {
               retVal = xa.Equals( ya );
            }
            else
            {
               retVal = xa.Equals( ya, false );
               if ( retVal
                  && !xa.PublicKeyOrToken.IsNullOrEmpty()
                  && !ya.PublicKeyOrToken.IsNullOrEmpty()
                  )
               {
                  if ( this._publicKeyComputer == null )
                  {
                     throw new NotSupportedException( "The crypto callbacks were not supplied to reflection context, so it is not possible to compare assembly name containing full public key and assembly name containing public key token." );
                  }
                  else
                  {
                     Byte[] xBytes, yBytes;
                     if ( x.Flags.IsFullPublicKey() )
                     {
                        // Create public key token for x and compare with y
                        xBytes = this._publicKeyComputer.Value.ComputePublicKeyToken( xa.PublicKeyOrToken );
                        yBytes = ya.PublicKeyOrToken;
                     }
                     else
                     {
                        // Create public key token for y and compare with x
                        xBytes = xa.PublicKeyOrToken;
                        yBytes = this._publicKeyComputer.Value.ComputePublicKeyToken( ya.PublicKeyOrToken );
                     }
                     retVal = ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( xBytes, yBytes );
                  }
               }
            }
            return retVal;
         }

         Int32 IEqualityComparer<CILAssemblyName>.GetHashCode( CILAssemblyName obj )
         {
            return obj == null ? 0 : obj.Name.GetHashCodeSafe( 0 );
         }

         protected override void Dispose( Boolean disposing )
         {
            if ( disposing && this._publicKeyComputer != null && this._publicKeyComputer.IsValueCreated )
            {
               this._publicKeyComputer.Value.Transform.DisposeSafely();
            }
         }
      }

      private static readonly System.Reflection.MethodInfo[] EMPTY_METHODS = new System.Reflection.MethodInfo[0];

      private readonly CollectionsFactory _cf;
      private readonly AbstractCILReflectionContextCache _cache;
      private readonly ListQuery<Type> _arrayInterfaces;
      private readonly ListQuery<Type> _multiDimArrayIFaces;
      private readonly CryptoCallbacks _defaultCryptoCallbacks;
      private readonly CILAssemblyNameEqualityComparer _defaultANComparer;
      private readonly CILReflectionContextConcurrencySupport _concurrencyMode;

      internal CILReflectionContextImpl( CILReflectionContextConcurrencySupport concurrencyMode, Type[] vectorArrayInterfaces, Type[] multiDimArrayIFaces, CryptoCallbacks defaultCryptoCallbacks )
      {
         this._concurrencyMode = concurrencyMode;
         this._cf = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY;
         AbstractCILReflectionContextCache cache;
         switch ( concurrencyMode )
         {
            case CILReflectionContextConcurrencySupport.NotThreadSafe:
               cache = new CILReflectionContextCache_NotThreadSafe( this );
               break;
            case CILReflectionContextConcurrencySupport.ThreadSafe_WithConcurrentCollections:
               cache = new CILReflectionContextCache_ThreadSafe( this );
               break;
            case CILReflectionContextConcurrencySupport.ThreadSafe_Simple:
               cache = new CILReflectionContextCache_ThreadSafe_Simple( this );
               break;
            default:
               throw new ArgumentException( "Unrecognized concurrency mode: " + concurrencyMode + "." );
         }
         this._cache = cache;

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
         this._defaultCryptoCallbacks = defaultCryptoCallbacks;
         this._defaultANComparer = new CILAssemblyNameEqualityComparer( defaultCryptoCallbacks );
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
      public event EventHandler<AssemblyRefResolveFromLoadedAssemblyEventArgs> AssemblyReferenceResolveFromLoadedAssemblyEvent;

      public CILReflectionContextConcurrencySupport ConcurrencySupport
      {
         get
         {
            return this._concurrencyMode;
         }
      }

      public CryptoCallbacks DefaultCryptoCallbacks
      {
         get
         {
            return this._defaultCryptoCallbacks;
         }
      }

      public IEqualityComparer<CILAssemblyName> DefaultAssemblyNameComparer
      {
         get
         {
            return this._defaultANComparer;
         }
      }

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

      //public Byte[] ComputePublicKeyToken( Byte[] publicKey )
      //{
      //   Byte[] retVal;
      //   if ( publicKey == null || publicKey.Length == 0 )
      //   {
      //      retVal = publicKey;
      //   }
      //   else
      //   {
      //      var args = MetaDataWriter.GetArgsForPublicKeyTokenComputing( this );
      //      using ( args.Transform )
      //      {
      //         retVal = args.ComputeHash( publicKey );
      //      }
      //      retVal = retVal.Skip( retVal.Length - 8 ).Reverse().ToArray();
      //   }
      //   return retVal;
      //}

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

      internal Lazy<ListProxy<CILCustomModifier>> LaunchEventAndCreateCustomModifiers( CustomModifierEventLoadArgs args )
      {
         return new Lazy<ListProxy<CILCustomModifier>>( () =>
         {
            this.LaunchCustomModifiersLoadEvent( args );
            return this.CollectionsFactory.NewListProxy<CILCustomModifier>( new List<CILCustomModifier>( args.RequiredModifiers.Select( mod => (CILCustomModifier) new CILCustomModifierImpl( false, (CILType) this.Cache.GetOrAdd( mod ) ) ).Concat( args.OptionalModifiers.Select( mod => (CILCustomModifier) new CILCustomModifierImpl( true, (CILType) this.Cache.GetOrAdd( mod ) ) ) ) ) );
         }, LazyThreadSafetyMode.PublicationOnly );
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


      internal AbstractCILReflectionContextCache Cache
      {
         get
         {
            return this._cache;
         }
      }

      protected override void Dispose( Boolean disposing )
      {
         if ( disposing )
         {
            this._cache.DisposeSafely();
            this._defaultANComparer.DisposeSafely();
         }
      }

   }

   internal abstract class AbstractCILReflectionContextCache : AbstractDisposable
   {


      internal const Int32 NO_ID = -1;

      protected interface IElementTypeCache
      {
         CILType MakeElementType( CILTypeBase type, ElementKind kind, GeneralArrayInfo arrayInfo );
      }

      protected abstract class ElementTypeCache<TCacheDic, TArrayDic> : IElementTypeCache
         where TCacheDic : class, IDictionary<CILTypeBase, InnerElementTypeCache<TArrayDic>>
         where TArrayDic : class, IDictionary<GeneralArrayInfo, CILType>
      {
         private readonly TCacheDic _cache;
         private readonly Func<CILTypeBase, InnerElementTypeCache<TArrayDic>> _outerGetter;

         internal ElementTypeCache( TCacheDic tCacheDic, Func<CILTypeBase, ElementKind, GeneralArrayInfo, CILTypeBase> creatorFunc )
         {
            this._cache = tCacheDic;
            this._outerGetter = type => this.InnerCacheFactory( type, ( kind, info ) => (CILType) creatorFunc( type, kind, info ) );
         }

         public CILType MakeElementType( CILTypeBase type, ElementKind kind, GeneralArrayInfo arrayInfo )
         {
            return this.GetOrAdd( this._cache, type, this._outerGetter ).MakeElementType( type, kind, arrayInfo );
         }

         protected abstract InnerElementTypeCache<TArrayDic> GetOrAdd( TCacheDic dic, CILTypeBase key, Func<CILTypeBase, InnerElementTypeCache<TArrayDic>> factory );

         protected abstract InnerElementTypeCache<TArrayDic> InnerCacheFactory( CILTypeBase originalType, Func<ElementKind, GeneralArrayInfo, CILType> genericElementFunction );
      }

      protected abstract class InnerElementTypeCache<TArrayDic>
         where TArrayDic : class, IDictionary<GeneralArrayInfo, CILType>
      {
         private readonly TArrayDic _arrayTypes;
         private readonly Func<GeneralArrayInfo, CILType> _arrayMakingFunction;
         private readonly Func<ElementKind, GeneralArrayInfo, CILType> _genericElementFunction;
         private CILType _pointerType;
         private CILType _referenceType;
         private CILType _szArrayType;

         internal InnerElementTypeCache( TArrayDic arrayTypes, CILTypeBase originalType, Func<ElementKind, GeneralArrayInfo, CILType> genericElementFunction )
         {
            this._arrayTypes = arrayTypes;
            this._genericElementFunction = genericElementFunction;
            this._arrayMakingFunction = info => (CILType) genericElementFunction( ElementKind.Array, info );
         }

         internal CILType MakeElementType( CILTypeBase type, ElementKind kind, GeneralArrayInfo arrayInfo )
         {
            switch ( kind )
            {
               case ElementKind.Array:
                  // Just like with generic arguments, we may need to create a copy of array info if it is specified
                  CILType result;
                  if ( arrayInfo == null )
                  {
                     result = this._szArrayType;
                     if ( result == null )
                     {
                        Interlocked.CompareExchange( ref this._szArrayType, this._genericElementFunction( ElementKind.Array, null ), null );
                        result = this._szArrayType;
                     }
                  }
                  else if ( !this._arrayTypes.TryGetValue( arrayInfo, out result ) )
                  {
                     arrayInfo = new GeneralArrayInfo( arrayInfo );
                     result = this.GetOrAdd( this._arrayTypes, arrayInfo, this._arrayMakingFunction );
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

         protected abstract CILType GetOrAdd( TArrayDic dic, GeneralArrayInfo key, Func<GeneralArrayInfo, CILType> factory );
      }

      protected interface ISimpleCache<TNative, TEmulated>
         where TNative : class
         where TEmulated : class
      {
         TEmulated GetOrAdd( TNative nativeElement );
      }

      protected abstract class SimpleCILCache<TDic, TNative, TEmulated> : ISimpleCache<TNative, TEmulated>
         where TDic : class, IDictionary<TNative, TEmulated>
         where TNative : class
         where TEmulated : class
      {
         private readonly TDic _cache;
         private readonly Func<TNative, TEmulated> _creatorFunc;

         internal SimpleCILCache( TDic cache, Func<TNative, TEmulated> creatorFunc )
         {
            ArgumentValidator.ValidateNotNull( "Cache", cache );
            ArgumentValidator.ValidateNotNull( "Creator callback", creatorFunc );

            this._cache = cache;
            this._creatorFunc = creatorFunc;
         }

         public TEmulated GetOrAdd( TNative nativeElement )
         {
            if ( nativeElement == null )
            {
               return null;
            }
            else
            {
               return this.GetOrAdd( this._cache, nativeElement, this._creatorFunc );
            }
         }

         protected abstract TEmulated GetOrAdd( TDic cache, TNative key, Func<TNative, TEmulated> factory );
      }

      protected interface IGenericInstanceCache<TInstance>
         where TInstance : class, CILElementWithGenericArguments<Object>
      {
         TInstance MakeGenericInstance( TInstance thisInstance, TInstance gDef, CILTypeBase[] gArgs );
         void ForAllInstancesOf<TCasted>( TInstance gDef, Action<TCasted> action )
            where TCasted : class;
      }

      protected abstract class GenericInstanceCache<TCacheDic, TGenericDic, TInstance> : IGenericInstanceCache<TInstance>
         where TCacheDic : class, IDictionary<TInstance, InnerGenericInstanceCache<TGenericDic, TInstance>>
         where TGenericDic : class, IDictionary<CILTypeBase[], TInstance>
         where TInstance : class, CILElementWithGenericArguments<Object>
      {
         private readonly TCacheDic _cache;
         private readonly Func<TInstance, CILTypeBase[], TInstance> _idCreator;
         private readonly String _elementKindString;

         internal GenericInstanceCache( TCacheDic cache, String elementKindString, Func<TInstance, CILTypeBase[], TInstance> idCreator )
         {
            ArgumentValidator.ValidateNotNull( "Cache dictionary", cache );
            this._cache = cache;
            this._elementKindString = elementKindString;
            this._idCreator = idCreator;
         }

         public TInstance MakeGenericInstance( TInstance thisInstance, TInstance gDef, CILTypeBase[] gArgs )
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

            return this
               .GetOrAdd( this._cache, gDef, gDeff => this.CheckGArgsAndCreate( gDeff, gArgs ) )
               .CreateGenericElement( gArgs );
         }

         public void ForAllInstancesOf<TCasted>( TInstance gDef, Action<TCasted> action )
            where TCasted : class
         {
            InnerGenericInstanceCache<TGenericDic, TInstance> inner;
            if ( this._cache.TryGetValue( gDef, out inner ) )
            {
               inner.DoSomethingForAll( gDef, action );
            }
         }

         protected abstract InnerGenericInstanceCache<TGenericDic, TInstance> GetOrAdd( TCacheDic dic, TInstance key, Func<TInstance, InnerGenericInstanceCache<TGenericDic, TInstance>> factory );
         protected abstract InnerGenericInstanceCache<TGenericDic, TInstance> InnerCacheFactory( TInstance gDef, Func<TInstance, CILTypeBase[], TInstance> creator );

         private InnerGenericInstanceCache<TGenericDic, TInstance> CheckGArgsAndCreate( TInstance gDef, CILTypeBase[] gArgs )
         {
            if ( !gDef.IsGenericDefinition() )
            {
               // TODO make do gDef = gDef.MQ.GenericDefinition maybe? throwing is ok tho, especially in debug mode
               throw new InvalidOperationException( "When making generic " + this._elementKindString + ", the " + this._elementKindString + " must be generic " + this._elementKindString + " definition." );
            }
            else if ( gArgs == null )
            {
               throw new ArgumentNullException( "Generic argument array was null." );
            }

            var gArgCount = gDef.GenericArguments.Count;
            if ( gArgs.Length != gArgCount )
            {
               throw new ArgumentException( "Amount of required generic parameters is " + gArgCount + ", but was given " + gArgs.Length + "." );
            }
            for ( var i = 0; i < gArgs.Length; ++i )
            {
               var gArg = gArgs[i];
               if ( gArg == null )
               {
                  throw new ArgumentNullException( "Generic argument at index " + i + " was null." );
               }
               else if ( gArg.IsPointerType() || gArg.IsByRef() || gArg.GetTypeCode( CILTypeCode.Empty ) == CILTypeCode.Void )
               {
                  throw new ArgumentException( "Generic argument " + gArg + " at index " + i + " was invalid." );
               }
            }

            return this.InnerCacheFactory( gDef, this._idCreator );
         }

         protected TCacheDic Cache
         {
            get
            {
               return this._cache;
            }
         }
      }

      protected abstract class InnerGenericInstanceCache<TGenericDic, TInstance> // : IDisposable
         where TGenericDic : class, IDictionary<CILTypeBase[], TInstance>
         where TInstance : class, CILElementWithGenericArguments<Object>
      {
         private readonly TGenericDic _cache;
         private readonly Func<CILTypeBase[], TInstance> _creator;

         internal InnerGenericInstanceCache( TGenericDic dic, TInstance gDef, Func<TInstance, CILTypeBase[], TInstance> idCreator )
         {
            this._cache = dic;
            this._creator = key => idCreator( gDef, key );
         }

         internal virtual TInstance CreateGenericElement( CILTypeBase[] gArgs )
         {
            TInstance result;
            if ( !this._cache.TryGetValue( gArgs, out result ) )
            {
               // Because array comes from outside of CAM, create a copy of it to prevent modifications
               var tmp = new CILTypeBase[gArgs.Length];
               Array.Copy( gArgs, tmp, gArgs.Length );
               result = this.GetOrAdd( this._cache, tmp, this._creator );
            }
            return result;
         }

         internal virtual void DoSomethingForAll<TCasted>( TInstance source, Action<TCasted> action )
            where TCasted : class
         {
            foreach ( var instance in this._cache.Values )
            {
               if ( !Object.ReferenceEquals( instance, source ) )
               {
                  action( instance as TCasted );
               }
            }
         }

         protected abstract TInstance GetOrAdd( TGenericDic dic, CILTypeBase[] key, Func<CILTypeBase[], TInstance> factory );

         protected TGenericDic Cache
         {
            get
            {
               return this._cache;
            }
         }
      }

      protected interface IGenericDeclaringTypeCache<TInstance>
         where TInstance : class, CILElementOwnedByType
      {
         TInstance GetOrAdd( TInstance instance, CILTypeBase[] gArgs );
         void ForAllInstancesOf<TCasted>( TInstance gDefInstance, Action<TCasted> action )
            where TCasted : class;
      }

      protected abstract class GenericDeclaringTypeCache<TCacheDic, TInnerCacheDic, TGenericDic, TInstance> : IGenericDeclaringTypeCache<TInstance> // : IDisposable
         where TCacheDic : class, IDictionary<CILType, InnerGenericDeclaringTypeCache<TInnerCacheDic, TGenericDic, TInstance>>
         where TInnerCacheDic : class, IDictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<TGenericDic, TInstance>>
         where TGenericDic : class, IDictionary<TInstance, TInstance>
         where TInstance : class, CILElementOwnedByType
      {


         private readonly TCacheDic _cache;
         private readonly Func<CILType, InnerGenericDeclaringTypeCache<TInnerCacheDic, TGenericDic, TInstance>> _cacheCreator;

         internal GenericDeclaringTypeCache( TCacheDic cache, Func<CILType, TInstance, CILTypeBase[], TInstance> creationFunc )
         {
            this._cache = cache;
            this._cacheCreator = gDef => this.InnerCacheFactory( gDef, ( gDefInstance, gArgs ) => creationFunc( gDef, gDefInstance, gArgs ) );
         }

         public TInstance GetOrAdd( TInstance instance, CILTypeBase[] gArgs )
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
            return this.GetOrAdd( this._cache, instance.DeclaringType, this._cacheCreator )
               .GetOrAdd( instance, gArgs );
         }

         public void ForAllInstancesOf<TCasted>( TInstance gDefInstance, Action<TCasted> action )
            where TCasted : class
         {
            InnerGenericDeclaringTypeCache<TInnerCacheDic, TGenericDic, TInstance> cache;
            if ( this._cache.TryGetValue( gDefInstance.DeclaringType, out cache ) )
            {
               cache.DoSomethingForAll( gDefInstance, action );
            }
         }

         protected TCacheDic Cache
         {
            get
            {
               return this._cache;
            }
         }

         protected abstract InnerGenericDeclaringTypeCache<TInnerCacheDic, TGenericDic, TInstance> GetOrAdd( TCacheDic dic, CILType key, Func<CILType, InnerGenericDeclaringTypeCache<TInnerCacheDic, TGenericDic, TInstance>> factory );
         protected abstract InnerGenericDeclaringTypeCache<TInnerCacheDic, TGenericDic, TInstance> InnerCacheFactory( CILType gDef, Func<TInstance, CILTypeBase[], TInstance> creationFunc );
      }

      protected abstract class InnerGenericDeclaringTypeCache<TInnerCacheDic, TGenericDic, TInstance> // : IDisposable
         where TInnerCacheDic : class, IDictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<TGenericDic, TInstance>>
         where TGenericDic : class, IDictionary<TInstance, TInstance>
         where TInstance : class, CILElementOwnedByType
      {
         private readonly TInnerCacheDic _cache;
         private readonly Func<CILTypeBase[], InnermostGenericDeclaringTypeCache<TGenericDic, TInstance>> _cacheCreator;

         internal InnerGenericDeclaringTypeCache( TInnerCacheDic cache, CILType gDef, Func<TInstance, CILTypeBase[], TInstance> creationFunc )
         {
            this._cache = cache;
            this._cacheCreator = gArgs => this.InnermostCacheFactory( gDef, gArgs, creationFunc );
         }

         internal virtual TInstance GetOrAdd( TInstance instance, CILTypeBase[] gArgs )
         {
            return this.GetOrAdd( this._cache, gArgs, this._cacheCreator )
               .GetOrAdd( instance );
         }

         internal virtual void DoSomethingForAll<TCasted>( TInstance gDefInstance, Action<TCasted> action )
            where TCasted : class
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

         protected abstract InnermostGenericDeclaringTypeCache<TGenericDic, TInstance> GetOrAdd( TInnerCacheDic dic, CILTypeBase[] key, Func<CILTypeBase[], InnermostGenericDeclaringTypeCache<TGenericDic, TInstance>> factory );
         protected abstract InnermostGenericDeclaringTypeCache<TGenericDic, TInstance> InnermostCacheFactory( CILType gDef, CILTypeBase[] gArgs, Func<TInstance, CILTypeBase[], TInstance> creationFunc );

         protected TInnerCacheDic Cache
         {
            get
            {
               return this._cache;
            }
         }
      }

      protected abstract class InnermostGenericDeclaringTypeCache<TGenericDic, TInstance>
         where TGenericDic : class, IDictionary<TInstance, TInstance>
         where TInstance : class, CILElementOwnedByType
      {
         private readonly TGenericDic _instances;
         private readonly Func<TInstance, TInstance> _instanceCreator;

         internal InnermostGenericDeclaringTypeCache( TGenericDic instances, CILType gDef, CILTypeBase[] gArgs, Func<TInstance, CILTypeBase[], TInstance> creationFunc )
         {
            this._instances = instances;
            this._instanceCreator = gDefInstance =>
            {
               if ( gArgs.All( gArg => gArg.TypeKind == TypeKind.TypeParameter && Object.Equals( ( (CILTypeParameter) gArg ).DeclaringType, gDef ) && ( (CILTypeParameter) gArg ).DeclaringMethod == null ) )
               {
                  return gDefInstance;
               }
               else
               {
                  if ( gArgs.Any( g => g.TypeKind == TypeKind.MethodSignature && !g.Module.Equals( gDef.Module ) ) )
                  {
                     var gtmp = new CILTypeBase[gArgs.Length];
                     Array.Copy( gArgs, gtmp, gArgs.Length );
                     for ( var i = 0; i < gtmp.Length; ++i )
                     {
                        LogicalUtils.CheckTypeForMethodSig( gDef.Module, ref gtmp[i] );
                     }
                     gArgs = gtmp;
                  }
                  return creationFunc( gDefInstance, gArgs );
               }
            };
         }

         internal TInstance GetOrAdd( TInstance gDefInstance )
         {
            return this.GetOrAdd( this._instances, gDefInstance, this._instanceCreator );
         }

         internal Boolean TryGetValue( TInstance gDefInstance, out TInstance result )
         {
            return this._instances.TryGetValue( gDefInstance, out result );
         }

         protected abstract TInstance GetOrAdd( TGenericDic dic, TInstance key, Func<TInstance, TInstance> factory );
      }

      protected class ListHolder<T>
         where T : class
      {
         private readonly List<T> _list;

         internal ListHolder()
         {
            this._list = new List<T>();
         }

         internal virtual T AcquireNew( Func<Int32, T> func )
         {
            var result = func( this._list.Count );
            this._list.Add( result );
            return result;
         }

         internal virtual T this[Int32 idx]
         {
            get
            {
               return this._list[idx];
            }
         }

         protected List<T> Cache
         {
            get
            {
               return this._list;
            }
         }
      }

      private struct ElementTypeCallbacksArgs
      {
         private readonly AbstractCILReflectionContextCache _cache;
         private readonly CILTypeBase _type;
         private readonly GeneralArrayInfo _arrayInfo;
         private readonly Int32 _elementTypeID;

         internal ElementTypeCallbacksArgs( AbstractCILReflectionContextCache cache, CILTypeBase type, GeneralArrayInfo arrayInfo, Int32 elementTypeID )
         {
            this._cache = cache;
            this._type = type;
            this._arrayInfo = arrayInfo;
            this._elementTypeID = elementTypeID;
         }

         public AbstractCILReflectionContextCache Cache
         {
            get
            {
               return this._cache;
            }
         }

         public CILTypeBase Type
         {
            get
            {
               return this._type;
            }
         }

         public GeneralArrayInfo ArrayInfo
         {
            get
            {
               return this._arrayInfo;
            }
         }

         public Int32 ElementTypeID
         {
            get
            {
               return this._elementTypeID;
            }
         }
      }

      private interface ElementTypeCallbacks
      {
         CILType GetElementTypeBaseType( ElementTypeCallbacksArgs args );
         IEnumerable<CILType> GetElementTypeInterfaces( ElementTypeCallbacksArgs args );
         TypeAttributes GetElementTypeAttributes( ElementTypeCallbacksArgs args );
         IEnumerable<CILMethod> GetElementTypeMethods( ElementTypeCallbacksArgs args );
         IEnumerable<CILConstructor> GetElementTypeConstructors( ElementTypeCallbacksArgs args );
         IEnumerable<CILProperty> GetElementTypeProperties( ElementTypeCallbacksArgs args );
         IEnumerable<CILEvent> GetElementTypeEvents( ElementTypeCallbacksArgs args );
      }

      private sealed class ArrayTypeCallbacks : ElementTypeCallbacks
      {

         public CILType GetElementTypeBaseType( ElementTypeCallbacksArgs args )
         {
            return args.Type.Module.AssociatedMSCorLibModule.GetTypeByName( Consts.ARRAY );
         }

         public IEnumerable<CILType> GetElementTypeInterfaces( ElementTypeCallbacksArgs args )
         {
            var type = args.Type;
            return ( args.ArrayInfo == null ?
               args.Cache._ctx.VectorArrayInterfaces :
               args.Cache._ctx.MultiDimensionalArrayInterfaces )
               .Select( iFace => type.Module.AssociatedMSCorLibModule.GetTypeByName( iFace.FullName, false ) )
               .Where( iFace => iFace != null );
         }

         public TypeAttributes GetElementTypeAttributes( ElementTypeCallbacksArgs args )
         {
            return TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Serializable;
         }

         public IEnumerable<CILMethod> GetElementTypeMethods( ElementTypeCallbacksArgs args )
         {
            var cache = args.Cache;
            var type = args.Type;
            var elementTypeID = args.ElementTypeID;
            Func<Int32, Int32, CILParameter> intParamFunc = ( methodID, pIdx ) => cache._paramContainer.AcquireNew( pID => new CILParameterImpl(
               cache._ctx,
               pID,
               new Lazy<ListProxy<CILCustomAttribute>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>(), LazyThreadSafetyMode.PublicationOnly ),
               new SettableValueForEnums<ParameterAttributes>( ParameterAttributes.None ),
               pIdx,
               new SettableValueForClasses<String>( null ),
               () => cache.ResolveMethodBaseID( methodID ),
               () => type.Module.AssociatedMSCorLibModule.GetTypeByName( Consts.INT32 ),
               new SettableLazy<Object>( () => null ),
               new Lazy<ListProxy<CILCustomModifier>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomModifier>(), LazyThreadSafetyMode.PublicationOnly ),
               new SettableLazy<LogicalMarshalingInfo>( () => null )
               )
            );
            Func<Int32, Int32, Boolean, CILParameter> valueParamFunc = ( methodID, pIdx, makeRef ) => cache._paramContainer.AcquireNew( pID => new CILParameterImpl(
                  cache._ctx,
                  pID,
                  new Lazy<ListProxy<CILCustomAttribute>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>(), LazyThreadSafetyMode.PublicationOnly ),
                  new SettableValueForEnums<ParameterAttributes>( ParameterAttributes.None ),
                  pIdx,
                  new SettableValueForClasses<String>( null ),
                  () => cache.ResolveMethodBaseID( methodID ),
                  () =>
                  {
                     var tR = type;
                     if ( makeRef )
                     {
                        tR = cache.MakeElementType( tR, ElementKind.Reference, null );
                     }
                     return tR;
                  },
                  new SettableLazy<Object>( () => null ),
                  new Lazy<ListProxy<CILCustomModifier>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomModifier>(), LazyThreadSafetyMode.PublicationOnly ),
                  new SettableLazy<LogicalMarshalingInfo>( () => null )
                  )
            );

            var dimSize = args.ArrayInfo == null ? 1 : args.ArrayInfo.Rank;
            yield return (CILMethod) cache._methodContainer.AcquireNew( curMID => new CILMethodImpl(
                  cache._ctx,
                  curMID,
                  new Lazy<ListProxy<CILCustomAttribute>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>(), LazyThreadSafetyMode.PublicationOnly ),
                  new SettableValueForEnums<CallingConventions>( CallingConventions.HasThis | CallingConventions.Standard ),
                  new SettableValueForEnums<MethodAttributes>( MethodAttributes.FamANDAssem | MethodAttributes.Family ),
                  () => (CILType) cache.ResolveTypeID( elementTypeID ),
                  () => cache._ctx.CollectionsFactory.NewListProxy<CILParameter>( Enumerable.Range( 0, dimSize + 1 )
                     .Select( pIdx => pIdx < dimSize ? intParamFunc( curMID, pIdx ) : valueParamFunc( curMID, pIdx, false ) )
                     .ToList() ),
                  new SettableLazy<MethodImplAttributes>( () => MethodImplAttributes.IL ),
                  null,
                  new SettableValueForClasses<String>( "Set" ),
                  () => cache._paramContainer.AcquireNew( pID => new CILParameterImpl(
                        cache._ctx,
                        pID,
                        new Lazy<ListProxy<CILCustomAttribute>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>(), LazyThreadSafetyMode.PublicationOnly ),
                        new SettableValueForEnums<ParameterAttributes>( ParameterAttributes.None ),
                        E_CILLogical.RETURN_PARAMETER_POSITION,
                        new SettableValueForClasses<String>( null ),
                        () => cache.ResolveMethodBaseID( curMID ),
                        () => type.Module.AssociatedMSCorLibModule.GetTypeByName( Consts.VOID ),
                        new SettableLazy<Object>( () => null ),
                        new Lazy<ListProxy<CILCustomModifier>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomModifier>(), LazyThreadSafetyMode.PublicationOnly ),
                        new SettableLazy<LogicalMarshalingInfo>( () => null )
                        )
                  ),
                  () => cache._ctx.CollectionsFactory.NewListProxy<CILTypeBase>(),
                  () => null
                  )
            );
            yield return (CILMethod) cache._methodContainer.AcquireNew( curMID => new CILMethodImpl(
                  cache._ctx,
                  curMID,
                  new Lazy<ListProxy<CILCustomAttribute>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>(), LazyThreadSafetyMode.PublicationOnly ),
                  new SettableValueForEnums<CallingConventions>( CallingConventions.HasThis | CallingConventions.Standard ),
                  new SettableValueForEnums<MethodAttributes>( MethodAttributes.FamANDAssem | MethodAttributes.Family ),
                  () => (CILType) cache.ResolveTypeID( elementTypeID ),
                  () => cache._ctx.CollectionsFactory.NewListProxy<CILParameter>( Enumerable.Range( 0, dimSize ).Select( pIdx => intParamFunc( curMID, pIdx ) ).ToList() ),
                  new SettableLazy<MethodImplAttributes>( () => MethodImplAttributes.IL ),
                  null,
                  new SettableValueForClasses<String>( "Address" ),
                  () => valueParamFunc( curMID, E_CILLogical.RETURN_PARAMETER_POSITION, true ),
                  () => cache._ctx.CollectionsFactory.NewListProxy<CILTypeBase>(),
                  () => null
                  )
            );

            yield return (CILMethod) cache._methodContainer.AcquireNew( curMID => new CILMethodImpl(
                  cache._ctx,
                  curMID,
                  new Lazy<ListProxy<CILCustomAttribute>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>(), LazyThreadSafetyMode.PublicationOnly ),
                  new SettableValueForEnums<CallingConventions>( CallingConventions.HasThis | CallingConventions.Standard ),
                  new SettableValueForEnums<MethodAttributes>( MethodAttributes.FamANDAssem | MethodAttributes.Family ),
                  () => (CILType) cache.ResolveTypeID( elementTypeID ),
                  () => cache._ctx.CollectionsFactory.NewListProxy<CILParameter>( Enumerable.Range( 0, dimSize ).Select( pIdx => intParamFunc( curMID, pIdx ) ).ToList() ),
                  new SettableLazy<MethodImplAttributes>( () => MethodImplAttributes.IL ),
                  null,
                  new SettableValueForClasses<String>( "Get" ),
                  () => valueParamFunc( curMID, E_CILLogical.RETURN_PARAMETER_POSITION, false ),
                  () => cache._ctx.CollectionsFactory.NewListProxy<CILTypeBase>(),
                  () => null
                  )
            );

         }

         public IEnumerable<CILConstructor> GetElementTypeConstructors( ElementTypeCallbacksArgs args )
         {
            var cache = args.Cache;
            var type = args.Type;

            Func<Int32, Int32, CILParameter> intParamFunc = ( methodID, pIdx ) => cache._paramContainer.AcquireNew( pID => new CILParameterImpl(
               cache._ctx,
               pID,
               new Lazy<ListProxy<CILCustomAttribute>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>(), LazyThreadSafetyMode.PublicationOnly ),
               new SettableValueForEnums<ParameterAttributes>( ParameterAttributes.None ),
               pIdx,
               new SettableValueForClasses<String>( null ),
               () => cache.ResolveMethodBaseID( methodID ),
               () => type.Module.AssociatedMSCorLibModule.GetTypeByName( Consts.INT32 ),
               new SettableLazy<Object>( () => null ),
               new Lazy<ListProxy<CILCustomModifier>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomModifier>(), LazyThreadSafetyMode.PublicationOnly ),
               new SettableLazy<LogicalMarshalingInfo>( () => null )
               )
            );

            var curType = type as CILType;
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
            var dimSize = args.ArrayInfo == null ? 1 : args.ArrayInfo.Rank;
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

            var elementTypeID = args.ElementTypeID;

            for ( var i = 0; i < amountOfCtors; ++i )
            {
               var curIdx = i;
               yield return (CILConstructor) cache._methodContainer.AcquireNew( ctorID => new CILConstructorImpl(
                     cache._ctx,
                     ctorID,
                     new Lazy<ListProxy<CILCustomAttribute>>( () => cache._ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>(), LazyThreadSafetyMode.PublicationOnly ),
                     new SettableValueForEnums<CallingConventions>( CallingConventions.HasThis | CallingConventions.Standard ),
                     new SettableValueForEnums<MethodAttributes>( MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.RTSpecialName ),
                     () => (CILType) cache.ResolveTypeID( elementTypeID ),
                     () => cache._ctx.CollectionsFactory.NewListProxy<CILParameter>( Enumerable.Range( 0, amountOfParams * curIdx ).Select( pIdx => intParamFunc( ctorID, pIdx ) ).ToList() ),
                     null,
                     new SettableLazy<MethodImplAttributes>( () => MethodImplAttributes.IL ),
                     null,
                     false
                  )
               );
            }
         }

         public IEnumerable<CILProperty> GetElementTypeProperties( ElementTypeCallbacksArgs args )
         {
            yield break;
         }

         public IEnumerable<CILEvent> GetElementTypeEvents( ElementTypeCallbacksArgs args )
         {
            yield break;
         }
      }

      private sealed class PointerOrByRefTypeCallbacks : ElementTypeCallbacks
      {

         public CILType GetElementTypeBaseType( ElementTypeCallbacksArgs args )
         {
            return null;
         }

         public IEnumerable<CILType> GetElementTypeInterfaces( ElementTypeCallbacksArgs args )
         {
            yield break;
         }

         public TypeAttributes GetElementTypeAttributes( ElementTypeCallbacksArgs args )
         {
            return TypeAttributes.AutoLayout;
         }

         public IEnumerable<CILMethod> GetElementTypeMethods( ElementTypeCallbacksArgs args )
         {
            yield break;
         }

         public IEnumerable<CILConstructor> GetElementTypeConstructors( ElementTypeCallbacksArgs args )
         {
            yield break;
         }

         public IEnumerable<CILProperty> GetElementTypeProperties( ElementTypeCallbacksArgs args )
         {
            yield break;
         }

         public IEnumerable<CILEvent> GetElementTypeEvents( ElementTypeCallbacksArgs args )
         {
            yield break;
         }
      }

      private readonly ISimpleCache<System.Reflection.Assembly, CILAssembly> _assemblies;
      private readonly ISimpleCache<System.Reflection.Module, CILModule> _modules;
      private readonly ISimpleCache<Type, CILTypeBase> _types;
      private readonly ISimpleCache<System.Reflection.FieldInfo, CILField> _fields;
      private readonly ISimpleCache<System.Reflection.ConstructorInfo, CILConstructor> _ctors;
      private readonly ISimpleCache<System.Reflection.MethodInfo, CILMethod> _methods;
      private readonly ISimpleCache<System.Reflection.ParameterInfo, CILParameter> _params;
      private readonly ISimpleCache<System.Reflection.PropertyInfo, CILProperty> _properties;
      private readonly ISimpleCache<System.Reflection.EventInfo, CILEvent> _events;
      private readonly IGenericInstanceCache<CILType> _genericTypes;
      private readonly IGenericInstanceCache<CILMethod> _genericMethods;
      private readonly IElementTypeCache _elementCache;
      private readonly IGenericDeclaringTypeCache<CILField> _fieldsWithGenericDeclaringType;
      private readonly IGenericDeclaringTypeCache<CILMethodBase> _methodsWithGenericDeclaringType;
      private readonly IGenericDeclaringTypeCache<CILProperty> _propertiesWithGenericDeclaringType;
      private readonly IGenericDeclaringTypeCache<CILEvent> _eventsWithGenericDeclaringType;

      private readonly ListHolder<CILAssembly> _assemblyContainer;
      private readonly ListHolder<CILModule> _moduleContainer;
      private readonly ListHolder<CILTypeOrTypeParameter> _typeContainer;
      private readonly ListHolder<CILField> _fieldContainer;
      private readonly ListHolder<CILMethodBase> _methodContainer;
      private readonly ListHolder<CILParameter> _paramContainer;
      private readonly ListHolder<CILProperty> _propertyContainer;
      private readonly ListHolder<CILEvent> _eventContainer;

      private readonly IDictionary<ElementKind, ElementTypeCallbacks> _elementTypeCallbacks;

      private readonly CILReflectionContextImpl _ctx;

      protected AbstractCILReflectionContextCache(
         CILReflectionContextImpl ctx,
         ListHolder<CILAssembly> assemblyContainer,
         ListHolder<CILModule> moduleContainer,
         ListHolder<CILTypeOrTypeParameter> typeContainer,
         ListHolder<CILField> fieldContainer,
         ListHolder<CILMethodBase> methodContainer,
         ListHolder<CILParameter> parameterContainer,
         ListHolder<CILProperty> propertyContainer,
         ListHolder<CILEvent> eventContainer,
         Func<Func<System.Reflection.Assembly, CILAssembly>, ISimpleCache<System.Reflection.Assembly, CILAssembly>> assemblyCacheFactory,
         Func<Func<System.Reflection.Module, CILModule>, ISimpleCache<System.Reflection.Module, CILModule>> moduleCacheFactory,
         Func<Func<Type, CILTypeBase>, ISimpleCache<Type, CILTypeBase>> typeCacheFactory,
         Func<Func<System.Reflection.FieldInfo, CILField>, ISimpleCache<System.Reflection.FieldInfo, CILField>> fieldCacheFactory,
         Func<Func<System.Reflection.ConstructorInfo, CILConstructor>, ISimpleCache<System.Reflection.ConstructorInfo, CILConstructor>> constructorCacheFactory,
         Func<Func<System.Reflection.MethodInfo, CILMethod>, ISimpleCache<System.Reflection.MethodInfo, CILMethod>> methodCacheFactory,
         Func<Func<System.Reflection.ParameterInfo, CILParameter>, ISimpleCache<System.Reflection.ParameterInfo, CILParameter>> parameterCacheFactory,
         Func<Func<System.Reflection.PropertyInfo, CILProperty>, ISimpleCache<System.Reflection.PropertyInfo, CILProperty>> propertyCacheFactory,
         Func<Func<System.Reflection.EventInfo, CILEvent>, ISimpleCache<System.Reflection.EventInfo, CILEvent>> eventCacheFactory,
         Func<Func<CILTypeBase, ElementKind, GeneralArrayInfo, CILTypeBase>, IElementTypeCache> elementTypeCacheFactory,
         Func<String, Func<CILType, CILTypeBase[], CILType>, IGenericInstanceCache<CILType>> genericTypeCacheFactory,
         Func<String, Func<CILMethod, CILTypeBase[], CILMethod>, IGenericInstanceCache<CILMethod>> genericMethodCacheFactory,
         Func<Func<CILType, CILField, CILTypeBase[], CILField>, IGenericDeclaringTypeCache<CILField>> genericTypeFieldsCacheFactory,
         Func<Func<CILType, CILMethodBase, CILTypeBase[], CILMethodBase>, IGenericDeclaringTypeCache<CILMethodBase>> genericTypeMethodsCacheFactory,
         Func<Func<CILType, CILProperty, CILTypeBase[], CILProperty>, IGenericDeclaringTypeCache<CILProperty>> genericTypePropertiesCacheFactory,
         Func<Func<CILType, CILEvent, CILTypeBase[], CILEvent>, IGenericDeclaringTypeCache<CILEvent>> genericTypeEventsCacheFactory
         )
      {
         ArgumentValidator.ValidateNotNull( "Reflection context", ctx );
         ArgumentValidator.ValidateNotNull( "Assembly container", assemblyContainer );
         ArgumentValidator.ValidateNotNull( "Module container", moduleContainer );
         ArgumentValidator.ValidateNotNull( "Type container", typeContainer );
         ArgumentValidator.ValidateNotNull( "Field container", fieldContainer );
         ArgumentValidator.ValidateNotNull( "Method container", methodContainer );
         ArgumentValidator.ValidateNotNull( "Parameter container", parameterContainer );
         ArgumentValidator.ValidateNotNull( "Property container", propertyContainer );
         ArgumentValidator.ValidateNotNull( "Event container", eventContainer );

         this._ctx = ctx;

         this._elementTypeCallbacks = new Dictionary<ElementKind, ElementTypeCallbacks>()
         {
            { ElementKind.Array, new ArrayTypeCallbacks() },
            { ElementKind.Pointer, new PointerOrByRefTypeCallbacks() },
            { ElementKind.Reference, new PointerOrByRefTypeCallbacks() }
         };

         this._assemblyContainer = assemblyContainer;
         this._moduleContainer = moduleContainer;
         this._typeContainer = typeContainer;
         this._fieldContainer = fieldContainer;
         this._methodContainer = methodContainer;
         this._paramContainer = parameterContainer;
         this._propertyContainer = propertyContainer;
         this._eventContainer = eventContainer;

         this._assemblies = assemblyCacheFactory( this.CreateNewEmulatedAssemblyForNativeAssembly );
         this._modules = moduleCacheFactory( this.CreateNewEmulatedModuleForNativeModule );
         this._types = typeCacheFactory( this.CreateNewEmulatedTypeForNativeType );
         this._fields = fieldCacheFactory( this.CreateNewEmulatedFieldForNativeField );
         this._ctors = constructorCacheFactory( this.CreateNewEmulatedCtorForNativeCtor );
         this._methods = methodCacheFactory( this.CreateNewEmulatedMethodForNativeMethod );
         this._params = parameterCacheFactory( this.CreateNewEmulatedParameterForNativeParameter );
         this._properties = propertyCacheFactory( this.CreateNewEmulatedPropertyForNativeProperty );
         this._events = eventCacheFactory( this.CreateNewEmulatedEventForNativeEvent );

         this._elementCache = elementTypeCacheFactory( this.CreateNewElementType );
         this._genericTypes = genericTypeCacheFactory( "type", this.CreateNewGenericInstance );
         this._genericMethods = genericMethodCacheFactory( "method", this.CreateNewGenericInstance );
         this._fieldsWithGenericDeclaringType = genericTypeFieldsCacheFactory( this.CreateNewFieldWithDifferentDeclaringTypeGArgs );
         this._methodsWithGenericDeclaringType = genericTypeMethodsCacheFactory( this.CreateNewMethodBaseWithDifferentDeclaringTypeGArgs );
         this._propertiesWithGenericDeclaringType = genericTypePropertiesCacheFactory( this.CreateNewPropertyWithDifferentDeclaringTypeGArgs );
         this._eventsWithGenericDeclaringType = genericTypeEventsCacheFactory( this.CreateNewEventWithDifferentDeclaringTypeGArgs );

      }

      protected ISimpleCache<System.Reflection.Assembly, CILAssembly> AssemblyCache
      {
         get
         {
            return this._assemblies;
         }
      }

      protected ISimpleCache<System.Reflection.Module, CILModule> ModuleCache
      {
         get
         {
            return this._modules;
         }
      }

      protected ISimpleCache<Type, CILTypeBase> TypeCache
      {
         get
         {
            return this._types;
         }
      }
      protected ISimpleCache<System.Reflection.FieldInfo, CILField> FieldCache
      {
         get
         {
            return this._fields;
         }
      }

      protected ISimpleCache<System.Reflection.ConstructorInfo, CILConstructor> ConstructorCache
      {
         get
         {
            return this._ctors;
         }
      }

      protected ISimpleCache<System.Reflection.MethodInfo, CILMethod> MethodCache
      {
         get
         {
            return this._methods;
         }
      }
      protected ISimpleCache<System.Reflection.ParameterInfo, CILParameter> ParameterCache
      {
         get
         {
            return this._params;
         }
      }

      protected ISimpleCache<System.Reflection.PropertyInfo, CILProperty> PropertyCache
      {
         get
         {
            return this._properties;
         }
      }
      protected ISimpleCache<System.Reflection.EventInfo, CILEvent> EventCache
      {
         get
         {
            return this._events;
         }
      }

      protected IGenericInstanceCache<CILType> GenericTypeCache
      {
         get
         {
            return this._genericTypes;
         }
      }
      protected IGenericInstanceCache<CILMethod> GenericMethodCache
      {
         get
         {
            return this._genericMethods;
         }
      }

      protected IElementTypeCache ElementTypeCacheInstance
      {
         get
         {
            return this._elementCache;
         }
      }

      protected IGenericDeclaringTypeCache<CILField> GenericTypeFieldsCache
      {
         get
         {
            return this._fieldsWithGenericDeclaringType;
         }
      }

      protected IGenericDeclaringTypeCache<CILMethodBase> GenericTypeMethodsCache
      {
         get
         {
            return this._methodsWithGenericDeclaringType;
         }
      }

      protected IGenericDeclaringTypeCache<CILProperty> GenericTypePropertiesCache
      {
         get
         {
            return this._propertiesWithGenericDeclaringType;
         }
      }

      protected IGenericDeclaringTypeCache<CILEvent> GenericTypeEventsCache
      {
         get
         {
            return this._eventsWithGenericDeclaringType;
         }
      }

      protected ListHolder<CILAssembly> AssemblyContainer
      {
         get
         {
            return this._assemblyContainer;
         }
      }

      protected ListHolder<CILModule> ModuleContainer
      {
         get
         {
            return this._moduleContainer;
         }
      }

      protected ListHolder<CILTypeOrTypeParameter> TypeContainer
      {
         get
         {
            return this._typeContainer;
         }
      }

      protected ListHolder<CILField> FieldContainer
      {
         get
         {
            return this._fieldContainer;
         }
      }

      protected ListHolder<CILMethodBase> MethodContainer
      {
         get
         {
            return this._methodContainer;
         }
      }

      protected ListHolder<CILParameter> ParameterContainer
      {
         get
         {
            return this._paramContainer;
         }
      }

      protected ListHolder<CILProperty> PropertyContainer
      {
         get
         {
            return this._propertyContainer;
         }
      }

      protected ListHolder<CILEvent> EventContainer
      {
         get
         {
            return this._eventContainer;
         }
      }

      private CILAssembly CreateNewEmulatedAssemblyForNativeAssembly( System.Reflection.Assembly ass )
      {
         return this._assemblyContainer.AcquireNew( id => new CILAssemblyImpl( this._ctx, id, ass ) );
      }

      private CILModule CreateNewEmulatedModuleForNativeModule( System.Reflection.Module mod )
      {
         return this._moduleContainer.AcquireNew( id => new CILModuleImpl( this._ctx, id, mod ) );
      }

      private CILTypeBase CreateNewEmulatedTypeForNativeType( Type type )
      {
         CILTypeBase result;
         if ( type.IsGenericParameter )
         {
            result = this._typeContainer.AcquireNew( id => new CILTypeParameterImpl( this._ctx, id, type ) );
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
               result = this._typeContainer.AcquireNew( id => new CILTypeImpl( this._ctx, id, type ) );
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
            result = this._fieldContainer.AcquireNew( id => new CILFieldImpl( this._ctx, id, field ) );
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
            result = this._methodContainer.AcquireNew( id => new CILConstructorImpl( this._ctx, id, ctor ) );
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
               result = this._methodContainer.AcquireNew( id => new CILMethodImpl( this._ctx, id, method ) );
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
            result = this._paramContainer.AcquireNew( id => new CILParameterImpl( this._ctx, id, param ) );
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
            result = this._propertyContainer.AcquireNew( id => new CILPropertyImpl( this._ctx, id, prop ) );
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
            result = this._eventContainer.AcquireNew( id => new CILEventImpl( this._ctx, id, evt ) );
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
            result = (CILType) this._typeContainer.AcquireNew( id => new CILTypeImpl(
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
            Func<Int32, CILParameter, CILParameter> newParamFunc = ( mID, param ) => this._paramContainer.AcquireNew( pID => new CILParameterImpl(
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
            result = (CILMethod) this._methodContainer.AcquireNew( id => new CILMethodImpl(
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
         ElementTypeCallbacks callbacks;
         if ( !this._elementTypeCallbacks.TryGetValue( kind, out callbacks ) || callbacks == null )
         {
            throw new InvalidOperationException( "Element kind " + kind + " is not supported." );
         }

         return this._typeContainer.AcquireNew( id =>
         {
            var args = new ElementTypeCallbacksArgs( this, type, arrayInfo, id );

            return new CILTypeImpl(
               this._ctx,
               id,
               new Lazy<ListProxy<CILCustomAttribute>>( () => this._ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>(), LazyThreadSafetyMode.PublicationOnly ),
               () => CILTypeCode.Object,
               ( (CILTypeBaseInternal) type ).NameInternal,
               ( (CILTypeBaseInternal) type ).NamespaceInternal,
               () => type.Module,
               () => null,
               () => callbacks.GetElementTypeBaseType( args ),
               () => this._ctx.CollectionsFactory.NewListProxy<CILType>( callbacks.GetElementTypeInterfaces( args ).ToList() ),
               new SettableValueForEnums<TypeAttributes>( callbacks.GetElementTypeAttributes( args ) ),
               kind,
               arrayInfo,
               () => this._ctx.CollectionsFactory.NewListProxy<CILTypeBase>(),
               () => null,
               new Lazy<ListProxy<CILType>>( () => this._ctx.CollectionsFactory.NewListProxy<CILType>(), LazyThreadSafetyMode.PublicationOnly ),
               () => this._ctx.CollectionsFactory.NewListProxy<CILField>(),
               () => type,
               () => this._ctx.CollectionsFactory.NewListProxy<CILMethod>( callbacks.GetElementTypeMethods( args ).ToList() ),
               () => this._ctx.CollectionsFactory.NewListProxy<CILConstructor>( callbacks.GetElementTypeConstructors( args ).ToList() ),
               () => this._ctx.CollectionsFactory.NewListProxy<CILProperty>( callbacks.GetElementTypeProperties( args ).ToList() ),
               () => this._ctx.CollectionsFactory.NewListProxy<CILEvent>( callbacks.GetElementTypeEvents( args ).ToList() ),
               new SettableLazy<LogicalClassLayout?>( () => null ),
               null
            );
         } );
      }

      private CILField CreateNewFieldWithDifferentDeclaringTypeGArgs( CILType gDef, CILField field, CILTypeBase[] gArgs )
      {
         return this._fieldContainer.AcquireNew( id => new CILFieldImpl(
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
         return this._propertyContainer.AcquireNew( id => new CILPropertyImpl(
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
         return this._eventContainer.AcquireNew( id => new CILEventImpl(
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
         Func<Int32, CILParameter, CILParameter> newParamFunc = ( mID, param ) => this._paramContainer.AcquireNew( pID => new CILParameterImpl(
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
         return this._methodContainer.AcquireNew( id =>
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
         return this._assemblyContainer.AcquireNew( id => new CILAssemblyImpl( this._ctx, id ) );
      }

      internal CILAssembly NewAssembly( Func<Int32, CILAssembly> creator )
      {
         return this._assemblyContainer.AcquireNew( creator );
      }

      internal CILModule NewBlankModule( CILAssembly ass, String name )
      {
         return this._moduleContainer.AcquireNew( id => new CILModuleImpl( this._ctx, id, ass, name ) );
      }

      internal CILModule NewModule( Func<Int32, CILModule> creator )
      {
         return this._moduleContainer.AcquireNew( creator );
      }

      internal CILType NewBlankType( CILModule owner, CILType declType, String name, TypeAttributes attrs, CILTypeCode tc )
      {
         return (CILType) this._typeContainer.AcquireNew( id => new CILTypeImpl( this._ctx, id, tc, owner, name, declType, attrs ) );
      }

      internal CILType NewType( Func<Int32, CILType> creator )
      {
         return (CILType) this._typeContainer.AcquireNew( creator );
      }

      internal CILParameter NewBlankParameter( CILMethodBase ownerMethod, Int32 position, String name, ParameterAttributes attrs, CILTypeBase paramType )
      {
         return this._paramContainer.AcquireNew( id => new CILParameterImpl( this._ctx, id, ownerMethod, position, name, attrs, paramType ) );
      }

      internal CILParameter NewParameter( Func<Int32, CILParameter> creator )
      {
         return this._paramContainer.AcquireNew( creator );
      }

      internal CILConstructor NewBlankConstructor( CILType ownerType, MethodAttributes attrs )
      {
         return (CILConstructor) this._methodContainer.AcquireNew( id => new CILConstructorImpl( this._ctx, id, ownerType, attrs ) );
      }

      internal CILConstructor NewConstructor( Func<Int32, CILConstructor> creator )
      {
         return (CILConstructor) this._methodContainer.AcquireNew( creator );
      }

      internal CILMethod NewBlankMethod( CILType ownerType, String name, MethodAttributes attrs, CallingConventions callingConventions )
      {
         return (CILMethod) this._methodContainer.AcquireNew( id => new CILMethodImpl( this._ctx, id, ownerType, name, attrs, callingConventions ) );
      }

      internal CILMethod NewMethod( Func<Int32, CILMethod> creator )
      {
         return (CILMethod) this._methodContainer.AcquireNew( creator );
      }

      internal CILField NewBlankField( CILType ownerType, String name, FieldAttributes attrs, CILTypeBase fieldType )
      {
         return this._fieldContainer.AcquireNew( id => new CILFieldImpl( this._ctx, id, ownerType, name, fieldType, attrs ) );
      }

      internal CILField NewField( Func<Int32, CILField> creator )
      {
         return this._fieldContainer.AcquireNew( creator );
      }

      internal CILTypeParameter NewBlankTypeParameter( CILType declaringType, CILMethod declaringMethod, String name, Int32 position )
      {
         return (CILTypeParameter) this._typeContainer.AcquireNew( id => new CILTypeParameterImpl( this._ctx, id, new Lazy<ListProxy<CILCustomAttribute>>( () => this._ctx.CollectionsFactory.NewListProxy( new List<CILCustomAttribute>() ), LazyThreadSafetyMode.PublicationOnly ), GenericParameterAttributes.None, declaringType, declaringMethod, name, position, () => this._ctx.CollectionsFactory.NewListProxy( new List<CILTypeBase>() ) ) );
      }

      internal CILTypeParameter NewTypeParameter( Func<Int32, CILTypeParameter> creator )
      {
         return (CILTypeParameter) this._typeContainer.AcquireNew( creator );
      }

      internal CILEvent NewBlankEvent( CILType declaringType, String name, EventAttributes attrs, CILTypeBase evtType )
      {
         return this._eventContainer.AcquireNew( id => new CILEventImpl( this._ctx, id, declaringType, name, attrs, evtType ) );
      }

      internal CILEvent NewEvent( Func<Int32, CILEvent> creator )
      {
         return this._eventContainer.AcquireNew( creator );
      }

      internal CILProperty NewBlankProperty( CILType declaringType, String name, PropertyAttributes attrs )
      {
         return this._propertyContainer.AcquireNew( id => new CILPropertyImpl( this._ctx, id, declaringType, name, attrs ) );
      }

      internal CILProperty NewProperty( Func<Int32, CILProperty> creator )
      {
         return this._propertyContainer.AcquireNew( creator );
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
         return NO_ID == id ? null : this._assemblyContainer[id];
      }


      internal CILModule ResolveModuleID( Int32 id )
      {
         return NO_ID == id ? null : this._moduleContainer[id];
      }


      internal CILTypeOrTypeParameter ResolveTypeID( Int32 id )
      {
         return NO_ID == id ? null : this._typeContainer[id];
      }

      internal CILField ResolveFieldID( Int32 id )
      {
         return NO_ID == id ? null : this._fieldContainer[id];
      }

      internal CILMethodBase ResolveMethodBaseID( Int32 id )
      {
         return NO_ID == id ? null : this._methodContainer[id];
      }

      internal CILParameter ResolveParameterID( Int32 id )
      {
         return NO_ID == id ? null : this._paramContainer[id];
      }

      internal CILProperty ResolvePropertyID( Int32 id )
      {
         return NO_ID == id ? null : this._propertyContainer[id];
      }

      internal CILEvent ResolveEventID( Int32 id )
      {
         return NO_ID == id ? null : this._eventContainer[id];
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
   }

   internal class CILReflectionContextCache_NotThreadSafe : AbstractCILReflectionContextCache
   {
      private sealed class ElementTypeCache : ElementTypeCache<Dictionary<CILTypeBase, InnerElementTypeCache<Dictionary<GeneralArrayInfo, CILType>>>, Dictionary<GeneralArrayInfo, CILType>>
      {
         internal ElementTypeCache( Func<CILTypeBase, ElementKind, GeneralArrayInfo, CILTypeBase> creatorFunc )
            : base( new Dictionary<CILTypeBase, InnerElementTypeCache<Dictionary<GeneralArrayInfo, CILType>>>(), creatorFunc )
         {

         }

         protected override InnerElementTypeCache<Dictionary<GeneralArrayInfo, CILType>> GetOrAdd( Dictionary<CILTypeBase, InnerElementTypeCache<Dictionary<GeneralArrayInfo, CILType>>> dic, CILTypeBase key, Func<CILTypeBase, InnerElementTypeCache<Dictionary<GeneralArrayInfo, CILType>>> factory )
         {
            return dic.GetOrAdd_NotThreadSafe( key, factory );
         }

         protected override InnerElementTypeCache<Dictionary<GeneralArrayInfo, CILType>> InnerCacheFactory( CILTypeBase originalType, Func<ElementKind, GeneralArrayInfo, CILType> genericElementFunction )
         {
            return new InnerElementTypeCache( originalType, genericElementFunction );
         }
      }

      private sealed class InnerElementTypeCache : InnerElementTypeCache<Dictionary<GeneralArrayInfo, CILType>>
      {
         internal InnerElementTypeCache( CILTypeBase originalType, Func<ElementKind, GeneralArrayInfo, CILType> genericElementFunction )
            : base( new Dictionary<GeneralArrayInfo, CILType>(), originalType, genericElementFunction )
         {

         }

         protected override CILType GetOrAdd( Dictionary<GeneralArrayInfo, CILType> dic, GeneralArrayInfo key, Func<GeneralArrayInfo, CILType> factory )
         {
            return dic.GetOrAdd_NotThreadSafe( key, factory );
         }
      }

      private sealed class GenericInstanceCache<TInstance> : GenericInstanceCache<Dictionary<TInstance, InnerGenericInstanceCache<Dictionary<CILTypeBase[], TInstance>, TInstance>>, Dictionary<CILTypeBase[], TInstance>, TInstance>
         where TInstance : class, CILElementWithGenericArguments<Object>
      {
         internal GenericInstanceCache( String elementKindString, Func<TInstance, CILTypeBase[], TInstance> idCreator )
            : base( new Dictionary<TInstance, InnerGenericInstanceCache<Dictionary<CILTypeBase[], TInstance>, TInstance>>(), elementKindString, idCreator )
         {

         }

         protected override InnerGenericInstanceCache<Dictionary<CILTypeBase[], TInstance>, TInstance> GetOrAdd( Dictionary<TInstance, InnerGenericInstanceCache<Dictionary<CILTypeBase[], TInstance>, TInstance>> dic, TInstance key, Func<TInstance, InnerGenericInstanceCache<Dictionary<CILTypeBase[], TInstance>, TInstance>> factory )
         {
            return dic.GetOrAdd_NotThreadSafe( key, factory );
         }

         protected override InnerGenericInstanceCache<Dictionary<CILTypeBase[], TInstance>, TInstance> InnerCacheFactory( TInstance gDef, Func<TInstance, CILTypeBase[], TInstance> creator )
         {
            return new InnerGenericInstanceCache<TInstance>( gDef, creator );
         }
      }

      private sealed class InnerGenericInstanceCache<TInstance> : InnerGenericInstanceCache<Dictionary<CILTypeBase[], TInstance>, TInstance>
         where TInstance : class, CILElementWithGenericArguments<Object>
      {
         internal InnerGenericInstanceCache( TInstance gDef, Func<TInstance, CILTypeBase[], TInstance> idCreator )
            : base( new Dictionary<CILTypeBase[], TInstance>( ArrayEqualityComparer<CILTypeBase>.DefaultArrayEqualityComparer ), gDef, idCreator )
         {

         }

         protected override TInstance GetOrAdd( Dictionary<CILTypeBase[], TInstance> dic, CILTypeBase[] key, Func<CILTypeBase[], TInstance> factory )
         {
            return dic.GetOrAdd_NotThreadSafe( key, factory );
         }
      }

      private sealed class GenericDeclaringTypeCache<TInstance> : GenericDeclaringTypeCache<Dictionary<CILType, InnerGenericDeclaringTypeCache<Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>>, Dictionary<TInstance, TInstance>, TInstance>>, Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>>, Dictionary<TInstance, TInstance>, TInstance>
         where TInstance : class, CILElementOwnedByType
      {

         internal GenericDeclaringTypeCache( Func<CILType, TInstance, CILTypeBase[], TInstance> creationFunc )
            : base( new Dictionary<CILType, InnerGenericDeclaringTypeCache<Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>>, Dictionary<TInstance, TInstance>, TInstance>>(), creationFunc )
         {

         }

         protected override InnerGenericDeclaringTypeCache<Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>>, Dictionary<TInstance, TInstance>, TInstance> GetOrAdd( Dictionary<CILType, InnerGenericDeclaringTypeCache<Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>>, Dictionary<TInstance, TInstance>, TInstance>> dic, CILType key, Func<CILType, InnerGenericDeclaringTypeCache<Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>>, Dictionary<TInstance, TInstance>, TInstance>> factory )
         {
            return dic.GetOrAdd_NotThreadSafe( key, factory );
         }

         protected override InnerGenericDeclaringTypeCache<Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>>, Dictionary<TInstance, TInstance>, TInstance> InnerCacheFactory( CILType gDef, Func<TInstance, CILTypeBase[], TInstance> creationFunc )
         {
            return new InnerGenericDeclaringTypeCache<TInstance>( gDef, creationFunc );
         }
      }

      private sealed class InnerGenericDeclaringTypeCache<TInstance> : InnerGenericDeclaringTypeCache<Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>>, Dictionary<TInstance, TInstance>, TInstance>
         where TInstance : class, CILElementOwnedByType
      {
         internal InnerGenericDeclaringTypeCache( CILType gDef, Func<TInstance, CILTypeBase[], TInstance> creationFunc )
            : base( new Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>>( ArrayEqualityComparer<CILTypeBase>.DefaultArrayEqualityComparer ), gDef, creationFunc )
         {

         }

         protected override InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance> GetOrAdd( Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>> dic, CILTypeBase[] key, Func<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>> factory )
         {
            return dic.GetOrAdd_NotThreadSafe( key, factory );
         }

         protected override InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance> InnermostCacheFactory( CILType gDef, CILTypeBase[] gArgs, Func<TInstance, CILTypeBase[], TInstance> creationFunc )
         {
            return new InnermostGenericDeclaringTypeCache<TInstance>( gDef, gArgs, creationFunc );
         }
      }

      private sealed class InnermostGenericDeclaringTypeCache<TInstance> : InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>
         where TInstance : class, CILElementOwnedByType
      {
         internal InnermostGenericDeclaringTypeCache( CILType gDef, CILTypeBase[] gArgs, Func<TInstance, CILTypeBase[], TInstance> creationFunc )
            : base( new Dictionary<TInstance, TInstance>(), gDef, gArgs, creationFunc )
         {

         }

         protected override TInstance GetOrAdd( Dictionary<TInstance, TInstance> dic, TInstance key, Func<TInstance, TInstance> factory )
         {
            return dic.GetOrAdd_NotThreadSafe( key, factory );
         }
      }

      private sealed class SimpleCILCache<TNative, TEmulated> : SimpleCILCache<Dictionary<TNative, TEmulated>, TNative, TEmulated>
         where TNative : class
         where TEmulated : class
      {
         internal SimpleCILCache( Func<TNative, TEmulated> factory )
            : base( new Dictionary<TNative, TEmulated>(), factory )
         {

         }

         protected override TEmulated GetOrAdd( Dictionary<TNative, TEmulated> cache, TNative key, Func<TNative, TEmulated> factory )
         {
            return cache.GetOrAdd_NotThreadSafe( key, factory );
         }
      }

      internal CILReflectionContextCache_NotThreadSafe( CILReflectionContextImpl ctx )
         : base(
         ctx,
         new ListHolder<CILAssembly>(),
         new ListHolder<CILModule>(),
         new ListHolder<CILTypeOrTypeParameter>(),
         new ListHolder<CILField>(),
         new ListHolder<CILMethodBase>(),
         new ListHolder<CILParameter>(),
         new ListHolder<CILProperty>(),
         new ListHolder<CILEvent>(),
         factory => new SimpleCILCache<System.Reflection.Assembly, CILAssembly>( factory ),
         factory => new SimpleCILCache<System.Reflection.Module, CILModule>( factory ),
         factory => new SimpleCILCache<Type, CILTypeBase>( factory ),
         factory => new SimpleCILCache<System.Reflection.FieldInfo, CILField>( factory ),
         factory => new SimpleCILCache<System.Reflection.ConstructorInfo, CILConstructor>( factory ),
         factory => new SimpleCILCache<System.Reflection.MethodInfo, CILMethod>( factory ),
         factory => new SimpleCILCache<System.Reflection.ParameterInfo, CILParameter>( factory ),
         factory => new SimpleCILCache<System.Reflection.PropertyInfo, CILProperty>( factory ),
         factory => new SimpleCILCache<System.Reflection.EventInfo, CILEvent>( factory ),
         factory => new ElementTypeCache( factory ),
         ( elementKindString, factory ) => new GenericInstanceCache<CILType>( elementKindString, factory ),
         ( elementKindString, factory ) => new GenericInstanceCache<CILMethod>( elementKindString, factory ),
         factory => new GenericDeclaringTypeCache<CILField>( factory ),
         factory => new GenericDeclaringTypeCache<CILMethodBase>( factory ),
         factory => new GenericDeclaringTypeCache<CILProperty>( factory ),
         factory => new GenericDeclaringTypeCache<CILEvent>( factory )
         )
      {

      }

      protected override void Dispose( Boolean disposing )
      {
         // Nothing to do.
      }
   }

   internal class CILReflectionContextCache_ThreadSafe : AbstractCILReflectionContextCache
   {
      private sealed class ElementTypeCache : ElementTypeCache<ConcurrentDictionary<CILTypeBase, InnerElementTypeCache<ConcurrentDictionary<GeneralArrayInfo, CILType>>>, ConcurrentDictionary<GeneralArrayInfo, CILType>>
      {
         internal ElementTypeCache( Func<CILTypeBase, ElementKind, GeneralArrayInfo, CILTypeBase> creatorFunc )
            : base( new ConcurrentDictionary<CILTypeBase, InnerElementTypeCache<ConcurrentDictionary<GeneralArrayInfo, CILType>>>(), creatorFunc )
         {

         }

         protected override InnerElementTypeCache<ConcurrentDictionary<GeneralArrayInfo, CILType>> GetOrAdd( ConcurrentDictionary<CILTypeBase, InnerElementTypeCache<ConcurrentDictionary<GeneralArrayInfo, CILType>>> dic, CILTypeBase key, Func<CILTypeBase, InnerElementTypeCache<ConcurrentDictionary<GeneralArrayInfo, CILType>>> factory )
         {
            return dic.GetOrAdd( key, factory );
         }

         protected override InnerElementTypeCache<ConcurrentDictionary<GeneralArrayInfo, CILType>> InnerCacheFactory( CILTypeBase originalType, Func<ElementKind, GeneralArrayInfo, CILType> genericElementFunction )
         {
            return new InnerElementTypeCache( originalType, genericElementFunction );
         }
      }

      private sealed class InnerElementTypeCache : InnerElementTypeCache<ConcurrentDictionary<GeneralArrayInfo, CILType>>
      {
         internal InnerElementTypeCache( CILTypeBase originalType, Func<ElementKind, GeneralArrayInfo, CILType> genericElementFunction )
            : base( new ConcurrentDictionary<GeneralArrayInfo, CILType>(), originalType, genericElementFunction )
         {

         }

         protected override CILType GetOrAdd( ConcurrentDictionary<GeneralArrayInfo, CILType> dic, GeneralArrayInfo key, Func<GeneralArrayInfo, CILType> factory )
         {
            return dic.GetOrAdd( key, factory );
         }
      }

      private sealed class GenericInstanceCache<TInstance> : GenericInstanceCache<ConcurrentDictionary<TInstance, InnerGenericInstanceCache<ConcurrentDictionary<CILTypeBase[], TInstance>, TInstance>>, ConcurrentDictionary<CILTypeBase[], TInstance>, TInstance>, IDisposable
         where TInstance : class, CILElementWithGenericArguments<Object>
      {
         internal GenericInstanceCache( String elementKindString, Func<TInstance, CILTypeBase[], TInstance> idCreator )
            : base( new ConcurrentDictionary<TInstance, InnerGenericInstanceCache<ConcurrentDictionary<CILTypeBase[], TInstance>, TInstance>>(), elementKindString, idCreator )
         {

         }

         protected override InnerGenericInstanceCache<ConcurrentDictionary<CILTypeBase[], TInstance>, TInstance> GetOrAdd( ConcurrentDictionary<TInstance, InnerGenericInstanceCache<ConcurrentDictionary<CILTypeBase[], TInstance>, TInstance>> dic, TInstance key, Func<TInstance, InnerGenericInstanceCache<ConcurrentDictionary<CILTypeBase[], TInstance>, TInstance>> factory )
         {
            return dic.GetOrAdd( key, factory );
         }

         protected override InnerGenericInstanceCache<ConcurrentDictionary<CILTypeBase[], TInstance>, TInstance> InnerCacheFactory( TInstance gDef, Func<TInstance, CILTypeBase[], TInstance> creator )
         {
            return new InnerGenericInstanceCache<TInstance>( gDef, creator );
         }

         public void Dispose()
         {
            foreach ( var val in this.Cache.Values )
            {
               ( (IDisposable) val ).DisposeSafely();
            }
         }
      }

      private sealed class InnerGenericInstanceCache<TInstance> : InnerGenericInstanceCache<ConcurrentDictionary<CILTypeBase[], TInstance>, TInstance>, IDisposable
         where TInstance : class, CILElementWithGenericArguments<Object>
      {
         private readonly ReaderWriterLockSlim _lock;

         internal InnerGenericInstanceCache( TInstance gDef, Func<TInstance, CILTypeBase[], TInstance> idCreator )
            : base( new ConcurrentDictionary<CILTypeBase[], TInstance>( ArrayEqualityComparer<CILTypeBase>.DefaultArrayEqualityComparer ), gDef, idCreator )
         {
            this._lock = new ReaderWriterLockSlim( LockRecursionPolicy.NoRecursion );
         }

         protected override TInstance GetOrAdd( ConcurrentDictionary<CILTypeBase[], TInstance> dic, CILTypeBase[] key, Func<CILTypeBase[], TInstance> factory )
         {
            return dic.GetOrAdd( key, factory );
         }

         internal override TInstance CreateGenericElement( CILTypeBase[] gArgs )
         {
            this._lock.EnterReadLock();
            try
            {
               return base.CreateGenericElement( gArgs );
            }
            finally
            {
               this._lock.ExitReadLock();
            }
         }

         internal override void DoSomethingForAll<TCasted>( TInstance source, Action<TCasted> action )
         {
            this._lock.EnterWriteLock();
            try
            {
               base.DoSomethingForAll<TCasted>( source, action );
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

      private sealed class GenericDeclaringTypeCache<TInstance> : GenericDeclaringTypeCache<ConcurrentDictionary<CILType, InnerGenericDeclaringTypeCache<ConcurrentDictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<ConcurrentDictionary<TInstance, TInstance>, TInstance>>, ConcurrentDictionary<TInstance, TInstance>, TInstance>>, ConcurrentDictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<ConcurrentDictionary<TInstance, TInstance>, TInstance>>, ConcurrentDictionary<TInstance, TInstance>, TInstance>, IDisposable
         where TInstance : class, CILElementOwnedByType
      {

         internal GenericDeclaringTypeCache( Func<CILType, TInstance, CILTypeBase[], TInstance> creationFunc )
            : base( new ConcurrentDictionary<CILType, InnerGenericDeclaringTypeCache<ConcurrentDictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<ConcurrentDictionary<TInstance, TInstance>, TInstance>>, ConcurrentDictionary<TInstance, TInstance>, TInstance>>(), creationFunc )
         {

         }

         protected override InnerGenericDeclaringTypeCache<ConcurrentDictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<ConcurrentDictionary<TInstance, TInstance>, TInstance>>, ConcurrentDictionary<TInstance, TInstance>, TInstance> GetOrAdd( ConcurrentDictionary<CILType, InnerGenericDeclaringTypeCache<ConcurrentDictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<ConcurrentDictionary<TInstance, TInstance>, TInstance>>, ConcurrentDictionary<TInstance, TInstance>, TInstance>> dic, CILType key, Func<CILType, InnerGenericDeclaringTypeCache<ConcurrentDictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<ConcurrentDictionary<TInstance, TInstance>, TInstance>>, ConcurrentDictionary<TInstance, TInstance>, TInstance>> factory )
         {
            return dic.GetOrAdd( key, factory );
         }

         protected override InnerGenericDeclaringTypeCache<ConcurrentDictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<ConcurrentDictionary<TInstance, TInstance>, TInstance>>, ConcurrentDictionary<TInstance, TInstance>, TInstance> InnerCacheFactory( CILType gDef, Func<TInstance, CILTypeBase[], TInstance> creationFunc )
         {
            return new InnerGenericDeclaringTypeCache<TInstance>( gDef, creationFunc );
         }

         public void Dispose()
         {
            foreach ( var val in this.Cache.Values )
            {
               ( (IDisposable) val ).DisposeSafely();
            }
         }
      }

      private sealed class InnerGenericDeclaringTypeCache<TInstance> : InnerGenericDeclaringTypeCache<ConcurrentDictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<ConcurrentDictionary<TInstance, TInstance>, TInstance>>, ConcurrentDictionary<TInstance, TInstance>, TInstance>, IDisposable
         where TInstance : class, CILElementOwnedByType
      {
         private readonly ReaderWriterLockSlim _lock;

         internal InnerGenericDeclaringTypeCache( CILType gDef, Func<TInstance, CILTypeBase[], TInstance> creationFunc )
            : base( new ConcurrentDictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<ConcurrentDictionary<TInstance, TInstance>, TInstance>>( ArrayEqualityComparer<CILTypeBase>.DefaultArrayEqualityComparer ), gDef, creationFunc )
         {
            this._lock = new ReaderWriterLockSlim( LockRecursionPolicy.NoRecursion );
         }

         protected override InnermostGenericDeclaringTypeCache<ConcurrentDictionary<TInstance, TInstance>, TInstance> GetOrAdd( ConcurrentDictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<ConcurrentDictionary<TInstance, TInstance>, TInstance>> dic, CILTypeBase[] key, Func<CILTypeBase[], InnermostGenericDeclaringTypeCache<ConcurrentDictionary<TInstance, TInstance>, TInstance>> factory )
         {
            return dic.GetOrAdd( key, factory );
         }

         protected override InnermostGenericDeclaringTypeCache<ConcurrentDictionary<TInstance, TInstance>, TInstance> InnermostCacheFactory( CILType gDef, CILTypeBase[] gArgs, Func<TInstance, CILTypeBase[], TInstance> creationFunc )
         {
            return new InnermostGenericDeclaringTypeCache<TInstance>( gDef, gArgs, creationFunc );
         }

         internal override TInstance GetOrAdd( TInstance instance, CILTypeBase[] gArgs )
         {
            this._lock.EnterReadLock();
            try
            {
               return base.GetOrAdd( instance, gArgs );
            }
            finally
            {
               this._lock.ExitReadLock();
            }
         }

         internal override void DoSomethingForAll<TCasted>( TInstance gDefInstance, Action<TCasted> action )
         {
            this._lock.EnterWriteLock();
            try
            {
               base.DoSomethingForAll<TCasted>( gDefInstance, action );
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

      private sealed class InnermostGenericDeclaringTypeCache<TInstance> : InnermostGenericDeclaringTypeCache<ConcurrentDictionary<TInstance, TInstance>, TInstance>
         where TInstance : class, CILElementOwnedByType
      {
         internal InnermostGenericDeclaringTypeCache( CILType gDef, CILTypeBase[] gArgs, Func<TInstance, CILTypeBase[], TInstance> creationFunc )
            : base( new ConcurrentDictionary<TInstance, TInstance>(), gDef, gArgs, creationFunc )
         {

         }

         protected override TInstance GetOrAdd( ConcurrentDictionary<TInstance, TInstance> dic, TInstance key, Func<TInstance, TInstance> factory )
         {
            return dic.GetOrAdd( key, factory );
         }
      }

      private sealed class SimpleCILCache<TNative, TEmulated> : SimpleCILCache<ConcurrentDictionary<TNative, TEmulated>, TNative, TEmulated>
         where TNative : class
         where TEmulated : class
      {
         internal SimpleCILCache( Func<TNative, TEmulated> factory )
            : base( new ConcurrentDictionary<TNative, TEmulated>(), factory )
         {

         }

         protected override TEmulated GetOrAdd( ConcurrentDictionary<TNative, TEmulated> cache, TNative key, Func<TNative, TEmulated> factory )
         {
            return cache.GetOrAdd( key, factory );
         }
      }

      private sealed class ListHolderWithLock<T> : ListHolder<T>, IDisposable
         where T : class
      {
         private readonly ReaderWriterLockSlim _lock;

         internal ListHolderWithLock()
         {
            this._lock = new ReaderWriterLockSlim( LockRecursionPolicy.NoRecursion );
         }

         internal override T AcquireNew( Func<int, T> func )
         {
            this._lock.EnterWriteLock();
            try
            {
               return base.AcquireNew( func );
            }
            finally
            {
               this._lock.ExitWriteLock();
            }
         }

         internal override T this[int idx]
         {
            get
            {
               this._lock.EnterReadLock();
               try
               {
                  return base[idx];
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

      internal CILReflectionContextCache_ThreadSafe( CILReflectionContextImpl ctx )
         : base(
         ctx,
         new ListHolderWithLock<CILAssembly>(),
         new ListHolderWithLock<CILModule>(),
         new ListHolderWithLock<CILTypeOrTypeParameter>(),
         new ListHolderWithLock<CILField>(),
         new ListHolderWithLock<CILMethodBase>(),
         new ListHolderWithLock<CILParameter>(),
         new ListHolderWithLock<CILProperty>(),
         new ListHolderWithLock<CILEvent>(),
         factory => new SimpleCILCache<System.Reflection.Assembly, CILAssembly>( factory ),
         factory => new SimpleCILCache<System.Reflection.Module, CILModule>( factory ),
         factory => new SimpleCILCache<Type, CILTypeBase>( factory ),
         factory => new SimpleCILCache<System.Reflection.FieldInfo, CILField>( factory ),
         factory => new SimpleCILCache<System.Reflection.ConstructorInfo, CILConstructor>( factory ),
         factory => new SimpleCILCache<System.Reflection.MethodInfo, CILMethod>( factory ),
         factory => new SimpleCILCache<System.Reflection.ParameterInfo, CILParameter>( factory ),
         factory => new SimpleCILCache<System.Reflection.PropertyInfo, CILProperty>( factory ),
         factory => new SimpleCILCache<System.Reflection.EventInfo, CILEvent>( factory ),
         factory => new ElementTypeCache( factory ),
         ( elementKindString, factory ) => new GenericInstanceCache<CILType>( elementKindString, factory ),
         ( elementKindString, factory ) => new GenericInstanceCache<CILMethod>( elementKindString, factory ),
         factory => new GenericDeclaringTypeCache<CILField>( factory ),
         factory => new GenericDeclaringTypeCache<CILMethodBase>( factory ),
         factory => new GenericDeclaringTypeCache<CILProperty>( factory ),
         factory => new GenericDeclaringTypeCache<CILEvent>( factory )
         )
      {

      }

      protected override void Dispose( Boolean disposing )
      {
         if ( disposing )
         {
            ( (IDisposable) this.AssemblyContainer ).DisposeSafely();
            ( (IDisposable) this.ModuleContainer ).DisposeSafely();
            ( (IDisposable) this.TypeContainer ).DisposeSafely();
            ( (IDisposable) this.FieldContainer ).DisposeSafely();
            ( (IDisposable) this.MethodContainer ).DisposeSafely();
            ( (IDisposable) this.ParameterContainer ).DisposeSafely();
            ( (IDisposable) this.PropertyContainer ).DisposeSafely();
            ( (IDisposable) this.EventContainer ).DisposeSafely();

            ( (IDisposable) this.GenericTypeCache ).DisposeSafely();
            ( (IDisposable) this.GenericMethodCache ).DisposeSafely();

            ( (IDisposable) this.GenericTypeFieldsCache ).DisposeSafely();
            ( (IDisposable) this.GenericTypeMethodsCache ).DisposeSafely();
            ( (IDisposable) this.GenericTypePropertiesCache ).DisposeSafely();
            ( (IDisposable) this.GenericTypeEventsCache ).DisposeSafely();
         }
      }
   }

   internal class CILReflectionContextCache_ThreadSafe_Simple : AbstractCILReflectionContextCache
   {
      private sealed class ElementTypeCache : ElementTypeCache<Dictionary<CILTypeBase, InnerElementTypeCache<Dictionary<GeneralArrayInfo, CILType>>>, Dictionary<GeneralArrayInfo, CILType>>
      {
         internal ElementTypeCache( Func<CILTypeBase, ElementKind, GeneralArrayInfo, CILTypeBase> creatorFunc )
            : base( new Dictionary<CILTypeBase, InnerElementTypeCache<Dictionary<GeneralArrayInfo, CILType>>>(), creatorFunc )
         {

         }

         protected override InnerElementTypeCache<Dictionary<GeneralArrayInfo, CILType>> GetOrAdd( Dictionary<CILTypeBase, InnerElementTypeCache<Dictionary<GeneralArrayInfo, CILType>>> dic, CILTypeBase key, Func<CILTypeBase, InnerElementTypeCache<Dictionary<GeneralArrayInfo, CILType>>> factory )
         {
            return dic.GetOrAdd_WithLock( key, factory );
         }

         protected override InnerElementTypeCache<Dictionary<GeneralArrayInfo, CILType>> InnerCacheFactory( CILTypeBase originalType, Func<ElementKind, GeneralArrayInfo, CILType> genericElementFunction )
         {
            return new InnerElementTypeCache( originalType, genericElementFunction );
         }
      }

      private sealed class InnerElementTypeCache : InnerElementTypeCache<Dictionary<GeneralArrayInfo, CILType>>
      {
         internal InnerElementTypeCache( CILTypeBase originalType, Func<ElementKind, GeneralArrayInfo, CILType> genericElementFunction )
            : base( new Dictionary<GeneralArrayInfo, CILType>(), originalType, genericElementFunction )
         {

         }

         protected override CILType GetOrAdd( Dictionary<GeneralArrayInfo, CILType> dic, GeneralArrayInfo key, Func<GeneralArrayInfo, CILType> factory )
         {
            return dic.GetOrAdd_WithLock( key, factory );
         }
      }

      private sealed class GenericInstanceCache<TInstance> : GenericInstanceCache<Dictionary<TInstance, InnerGenericInstanceCache<Dictionary<CILTypeBase[], TInstance>, TInstance>>, Dictionary<CILTypeBase[], TInstance>, TInstance>
         where TInstance : class, CILElementWithGenericArguments<Object>
      {
         internal GenericInstanceCache( String elementKindString, Func<TInstance, CILTypeBase[], TInstance> idCreator )
            : base( new Dictionary<TInstance, InnerGenericInstanceCache<Dictionary<CILTypeBase[], TInstance>, TInstance>>(), elementKindString, idCreator )
         {

         }

         protected override InnerGenericInstanceCache<Dictionary<CILTypeBase[], TInstance>, TInstance> GetOrAdd( Dictionary<TInstance, InnerGenericInstanceCache<Dictionary<CILTypeBase[], TInstance>, TInstance>> dic, TInstance key, Func<TInstance, InnerGenericInstanceCache<Dictionary<CILTypeBase[], TInstance>, TInstance>> factory )
         {
            return dic.GetOrAdd_WithLock( key, factory );
         }

         protected override InnerGenericInstanceCache<Dictionary<CILTypeBase[], TInstance>, TInstance> InnerCacheFactory( TInstance gDef, Func<TInstance, CILTypeBase[], TInstance> creator )
         {
            return new InnerGenericInstanceCache<TInstance>( gDef, creator );
         }
      }

      private sealed class InnerGenericInstanceCache<TInstance> : InnerGenericInstanceCache<Dictionary<CILTypeBase[], TInstance>, TInstance>
         where TInstance : class, CILElementWithGenericArguments<Object>
      {
         internal InnerGenericInstanceCache( TInstance gDef, Func<TInstance, CILTypeBase[], TInstance> idCreator )
            : base( new Dictionary<CILTypeBase[], TInstance>( ArrayEqualityComparer<CILTypeBase>.DefaultArrayEqualityComparer ), gDef, idCreator )
         {

         }

         protected override TInstance GetOrAdd( Dictionary<CILTypeBase[], TInstance> dic, CILTypeBase[] key, Func<CILTypeBase[], TInstance> factory )
         {
            return dic.GetOrAdd_WithLock( key, factory );
         }

         internal override TInstance CreateGenericElement( CILTypeBase[] gArgs )
         {
            lock ( this.Cache )
            {
               return base.CreateGenericElement( gArgs );
            }
         }

         internal override void DoSomethingForAll<TCasted>( TInstance source, Action<TCasted> action )
         {
            lock ( this.Cache )
            {
               base.DoSomethingForAll<TCasted>( source, action );
            }
         }
      }

      private sealed class GenericDeclaringTypeCache<TInstance> : GenericDeclaringTypeCache<Dictionary<CILType, InnerGenericDeclaringTypeCache<Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>>, Dictionary<TInstance, TInstance>, TInstance>>, Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>>, Dictionary<TInstance, TInstance>, TInstance>
         where TInstance : class, CILElementOwnedByType
      {

         internal GenericDeclaringTypeCache( Func<CILType, TInstance, CILTypeBase[], TInstance> creationFunc )
            : base( new Dictionary<CILType, InnerGenericDeclaringTypeCache<Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>>, Dictionary<TInstance, TInstance>, TInstance>>(), creationFunc )
         {

         }

         protected override InnerGenericDeclaringTypeCache<Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>>, Dictionary<TInstance, TInstance>, TInstance> GetOrAdd( Dictionary<CILType, InnerGenericDeclaringTypeCache<Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>>, Dictionary<TInstance, TInstance>, TInstance>> dic, CILType key, Func<CILType, InnerGenericDeclaringTypeCache<Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>>, Dictionary<TInstance, TInstance>, TInstance>> factory )
         {
            return dic.GetOrAdd_WithLock( key, factory );
         }

         protected override InnerGenericDeclaringTypeCache<Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>>, Dictionary<TInstance, TInstance>, TInstance> InnerCacheFactory( CILType gDef, Func<TInstance, CILTypeBase[], TInstance> creationFunc )
         {
            return new InnerGenericDeclaringTypeCache<TInstance>( gDef, creationFunc );
         }
      }

      private sealed class InnerGenericDeclaringTypeCache<TInstance> : InnerGenericDeclaringTypeCache<Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>>, Dictionary<TInstance, TInstance>, TInstance>
         where TInstance : class, CILElementOwnedByType
      {
         internal InnerGenericDeclaringTypeCache( CILType gDef, Func<TInstance, CILTypeBase[], TInstance> creationFunc )
            : base( new Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>>( ArrayEqualityComparer<CILTypeBase>.DefaultArrayEqualityComparer ), gDef, creationFunc )
         {

         }

         protected override InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance> GetOrAdd( Dictionary<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>> dic, CILTypeBase[] key, Func<CILTypeBase[], InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>> factory )
         {
            return dic.GetOrAdd_WithLock( key, factory );
         }

         protected override InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance> InnermostCacheFactory( CILType gDef, CILTypeBase[] gArgs, Func<TInstance, CILTypeBase[], TInstance> creationFunc )
         {
            return new InnermostGenericDeclaringTypeCache<TInstance>( gDef, gArgs, creationFunc );
         }

         internal override TInstance GetOrAdd( TInstance instance, CILTypeBase[] gArgs )
         {
            lock ( this.Cache )
            {
               return base.GetOrAdd( instance, gArgs );
            }
         }

         internal override void DoSomethingForAll<TCasted>( TInstance gDefInstance, Action<TCasted> action )
         {
            lock ( this.Cache )
            {
               base.DoSomethingForAll<TCasted>( gDefInstance, action );
            }
         }

      }

      private sealed class InnermostGenericDeclaringTypeCache<TInstance> : InnermostGenericDeclaringTypeCache<Dictionary<TInstance, TInstance>, TInstance>
         where TInstance : class, CILElementOwnedByType
      {
         internal InnermostGenericDeclaringTypeCache( CILType gDef, CILTypeBase[] gArgs, Func<TInstance, CILTypeBase[], TInstance> creationFunc )
            : base( new Dictionary<TInstance, TInstance>(), gDef, gArgs, creationFunc )
         {

         }

         protected override TInstance GetOrAdd( Dictionary<TInstance, TInstance> dic, TInstance key, Func<TInstance, TInstance> factory )
         {
            return dic.GetOrAdd_WithLock( key, factory );
         }
      }

      private sealed class SimpleCILCache<TNative, TEmulated> : SimpleCILCache<Dictionary<TNative, TEmulated>, TNative, TEmulated>
         where TNative : class
         where TEmulated : class
      {
         internal SimpleCILCache( Func<TNative, TEmulated> factory )
            : base( new Dictionary<TNative, TEmulated>(), factory )
         {

         }

         protected override TEmulated GetOrAdd( Dictionary<TNative, TEmulated> cache, TNative key, Func<TNative, TEmulated> factory )
         {
            return cache.GetOrAdd_WithLock( key, factory );
         }
      }

      private sealed class ListHolderWithLock<T> : ListHolder<T>
         where T : class
      {
         internal ListHolderWithLock()
         {

         }

         internal override T AcquireNew( Func<int, T> func )
         {
            lock ( this.Cache )
            {
               return base.AcquireNew( func );
            }
         }

         internal override T this[int idx]
         {
            get
            {
               lock ( this.Cache )
               {
                  return base[idx];
               }
            }
         }
      }

      internal CILReflectionContextCache_ThreadSafe_Simple( CILReflectionContextImpl ctx )
         : base(
         ctx,
         new ListHolderWithLock<CILAssembly>(),
         new ListHolderWithLock<CILModule>(),
         new ListHolderWithLock<CILTypeOrTypeParameter>(),
         new ListHolderWithLock<CILField>(),
         new ListHolderWithLock<CILMethodBase>(),
         new ListHolderWithLock<CILParameter>(),
         new ListHolderWithLock<CILProperty>(),
         new ListHolderWithLock<CILEvent>(),
         factory => new SimpleCILCache<System.Reflection.Assembly, CILAssembly>( factory ),
         factory => new SimpleCILCache<System.Reflection.Module, CILModule>( factory ),
         factory => new SimpleCILCache<Type, CILTypeBase>( factory ),
         factory => new SimpleCILCache<System.Reflection.FieldInfo, CILField>( factory ),
         factory => new SimpleCILCache<System.Reflection.ConstructorInfo, CILConstructor>( factory ),
         factory => new SimpleCILCache<System.Reflection.MethodInfo, CILMethod>( factory ),
         factory => new SimpleCILCache<System.Reflection.ParameterInfo, CILParameter>( factory ),
         factory => new SimpleCILCache<System.Reflection.PropertyInfo, CILProperty>( factory ),
         factory => new SimpleCILCache<System.Reflection.EventInfo, CILEvent>( factory ),
         factory => new ElementTypeCache( factory ),
         ( elementKindString, factory ) => new GenericInstanceCache<CILType>( elementKindString, factory ),
         ( elementKindString, factory ) => new GenericInstanceCache<CILMethod>( elementKindString, factory ),
         factory => new GenericDeclaringTypeCache<CILField>( factory ),
         factory => new GenericDeclaringTypeCache<CILMethodBase>( factory ),
         factory => new GenericDeclaringTypeCache<CILProperty>( factory ),
         factory => new GenericDeclaringTypeCache<CILEvent>( factory )
         )
      {

      }

      protected override void Dispose( Boolean disposing )
      {
         // Nothing to do.
      }
   }
}