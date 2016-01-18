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
using System.Threading;
using CollectionsWithRoles.API;
using CommonUtils;
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Logical.Implementation
{
   internal abstract class CILTypeOrParameterImpl : CILCustomAttributeContainerImpl, CILTypeOrTypeParameter, CILTypeOrGenericParamInternal
   {
      private static readonly IDictionary<String, CILTypeCode> TC_MAPPING = new Dictionary<String, CILTypeCode>()
      {
         {typeof(void).FullName, CILTypeCode.Void},
         {typeof(ValueType).FullName, CILTypeCode.Value},
         {typeof(Enum).FullName, CILTypeCode.Enum},
         {typeof(IntPtr).FullName, CILTypeCode.IntPtr},
         {typeof(UIntPtr).FullName, CILTypeCode.UIntPtr},
         {typeof(Object).FullName, CILTypeCode.SystemObject},
         {typeof(Type).FullName, CILTypeCode.Type},
         {"System.TypedReference", CILTypeCode.TypedByRef}
      };

      protected internal readonly TypeKind typeKind;
      protected internal readonly WriteableLazy<CILTypeCode> typeCode;
      protected internal readonly SettableValueForClasses<String> name;
      protected internal readonly SettableValueForClasses<String> @namespace;
      protected internal readonly Lazy<CILModule> module;
      protected internal readonly Lazy<CILType> declaringType;

      protected CILTypeOrParameterImpl( CILReflectionContextImpl ctx, Int32 anID, Type type )
         : base( ctx, anID, CILElementKind.Type, cb => cb.GetCustomAttributesDataForOrThrow( type ) )
      {
         ArgumentValidator.ValidateNotNull( "Reflection context", ctx );

         InitFields(
            ctx,
            ref this.typeKind,
            ref this.typeCode,
            ref this.name,
            ref this.@namespace,
            ref this.module,
            ref this.declaringType,
            type.IsGenericParameter ? TypeKind.TypeParameter : TypeKind.Type,
            () =>
            {
               var tc =
#if WINDOWS_PHONE_APP
                  type.GetTypeCode()
#else
 (CILTypeCode) Type.GetTypeCode( type )
#endif
;
               if ( tc == (CILTypeCode) 2 )
               {
                  // DBNull
                  tc = CILTypeCode.Object;
               }
               else if ( tc == CILTypeCode.Object && LogicalUtils.NATIVE_MSCORLIB.Equals( type
#if WINDOWS_PHONE_APP
            .GetTypeInfo()
#endif
.Assembly ) && type.FullName != null )
               {
                  // Check for void, typedbyref, valuetype, enum, etc
                  if ( !TC_MAPPING.TryGetValue( type.FullName, out tc ) )
                  {
                     tc = CILTypeCode.Object;
                  }
               }
               return tc;
            },
            new SettableValueForClasses<String>( type.Name ),
            new SettableValueForClasses<String>( type.DeclaringType == null ? type.Namespace : null ),
            () => ctx.NewWrapper( ctx.WrapperCallbacks.GetModuleOfType( type ) ),
            () => ctx.NewWrapperAsType( type.DeclaringType ),
            true
         );
      }

      protected CILTypeOrParameterImpl(
         CILReflectionContextImpl ctx,
         Int32 anID,
         Lazy<ListProxy<CILCustomAttribute>> cAttrDataFunc,
         TypeKind aTypeKind,
         Func<CILTypeCode> typeCode,
         SettableValueForClasses<String> aName,
         SettableValueForClasses<String> aNamespace,
         Func<CILModule> moduleFunc,
         Func<CILType> declaringTypeFunc,
         Boolean baseTypeSettable
         )
         : base( ctx, CILElementKind.Type, anID, cAttrDataFunc )
      {
         InitFields(
            ctx,
            ref this.typeKind,
            ref this.typeCode,
            ref this.name,
            ref this.@namespace,
            ref this.module,
            ref this.declaringType,
            aTypeKind,
            typeCode,
            aName,
            aNamespace,
            moduleFunc,
            declaringTypeFunc,
            baseTypeSettable
            );
      }

      private static void InitFields(
         CILReflectionContextImpl ctx,
         ref TypeKind typeKind,
         ref WriteableLazy<CILTypeCode> typeCode,
         ref SettableValueForClasses<String> name,
         ref SettableValueForClasses<String> @namespace,
         ref Lazy<CILModule> module,
         ref Lazy<CILType> declaringType,
         TypeKind aTypeKind,
         Func<CILTypeCode> aTypeCode,
         SettableValueForClasses<String> aName,
         SettableValueForClasses<String> aNamespace,
         Func<CILModule> moduleFunc,
         Func<CILType> declaringTypeFunc,
         Boolean baseTypeSettable
         )
      {
         typeKind = aTypeKind;
         typeCode = LazyFactory.NewWriteableLazy( aTypeCode, ctx.LazyThreadSafetyMode );
         name = aName;
         @namespace = aNamespace;
         module = new Lazy<CILModule>( moduleFunc, ctx.LazyThreadSafetyMode );
         declaringType = new Lazy<CILType>( declaringTypeFunc, ctx.LazyThreadSafetyMode );
      }

      #region CILTypeBase Members

      public CILModule Module
      {
         get
         {
            return this.module.Value;
         }
      }

      public String Namespace
      {
         set
         {
            this.@namespace.Value = value;
         }
         get
         {
            return this.@namespace.Value;
         }
      }

      public TypeKind TypeKind
      {
         get
         {
            return this.typeKind;
         }
      }

      public CILTypeCode TypeCode
      {
         get
         {
            return this.typeCode.Value;
         }
         //set
         //{
         //   this.typeCode.Value = value;
         //}
      }

      public CILType MakeElementType( ElementKind kind, GeneralArrayInfo arrayInfo )
      {
         return this.context.Cache.MakeElementType( this, kind, arrayInfo );
      }

      #endregion

      #region CILElementWithSimpleName Members

      public virtual String Name
      {
         set
         {
            this.name.Value = value;
         }
         get
         {
            return this.name.Value;
         }
      }

      #endregion

      #region CILElementOwnedByType Members

      public CILType DeclaringType
      {
         get
         {
            return this.declaringType.Value;
         }
      }

      #endregion

      #region CILTypeBaseInternal Members

      SettableValueForClasses<String> CILTypeBaseInternal.NamespaceInternal
      {
         get
         {
            return this.@namespace;
         }
      }

      #endregion

      #region CILElementWithSimpleNameInternal Members

      SettableValueForClasses<String> CILElementWithSimpleNameInternal.NameInternal
      {
         get
         {
            return this.name;
         }
      }

      #endregion

      #region CILElementInstantiable Members

      public Boolean IsTrueDefinition
      {
         get
         {
            return this.IsCapableOfChanging() == null;
         }
      }

      #endregion

      internal virtual String GetNameString()
      {
         return this.name.Value;
      }

      public CILElementWithinILCode ElementTypeKind
      {
         get
         {
            return CILElementWithinILCode.Type;
         }
      }
   }

   internal class CILTypeImpl : CILTypeOrParameterImpl, CILType, CILTypeInternal
   {
      // TODO move this to API
      internal const Int32 NO_ARRAY_RANK = -1;
      internal const String NESTED_TYPENAME_SEPARATOR = "+";
      //internal const String NAMESPACE_SEPARATOR = ".";
      internal const String G_ARGS_START = "[";
      internal const String G_ARGS_END = "]";

      internal static readonly CILTypeParameter[] EMPTY_TYPE_PARAMS = new CILTypeParameter[0];

      private readonly SettableValueForEnums<TypeAttributes> typeAttributes;
      private readonly ElementKind? elementKind;
      private readonly GeneralArrayInfo arrayInfo;
      private readonly Lazy<ListProxy<CILTypeBase>> gArgs;
      private readonly Lazy<ListProxy<CILType>> nestedTypes;
      private readonly ReadOnlyResettableLazy<ListProxy<CILField>> fields;
      private readonly WriteableLazy<CILType> genericDefinition;
      private readonly Lazy<CILTypeBase> elementType;
      private readonly ReadOnlyResettableLazy<ListProxy<CILMethod>> methods;
      private readonly ReadOnlyResettableLazy<ListProxy<CILConstructor>> ctors;
      private readonly ReadOnlyResettableLazy<ListProxy<CILProperty>> properties;
      private readonly ReadOnlyResettableLazy<ListProxy<CILEvent>> events;
      private readonly WriteableLazy<LogicalClassLayout?> layout;
      private readonly IResettableLazy<CILType> baseType;
      private readonly ReadOnlyResettableLazy<ListProxy<CILType>> declaredInterfaces;
      private readonly Lazy<DictionaryWithRoles<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>> securityInfo;
      private readonly ReadOnlyResettableLazy<DictionaryWithRoles<CILMethod, ListProxy<CILMethod>, ListProxyQuery<CILMethod>, ListQuery<CILMethod>>> explicitMethodImplementationMap;

      internal CILTypeImpl( CILReflectionContextImpl ctx, Int32 anID, Type type )
         : base( ctx, anID, type )
      {
         if ( TypeKind.Type != this.typeKind )
         {
            throw new ArgumentException( "Trying to create type for type parameter " + type );
         }
         var isGDef = type
#if WINDOWS_PHONE_APP
         .GetTypeInfo()
#endif
.IsGenericTypeDefinition;

         if ( type
#if WINDOWS_PHONE_APP
         .GetTypeInfo()
#endif
.IsGenericType && !isGDef )
         {
            throw new ArgumentException( "This constructor may only be used for non-generic types or generic type defintions." );
         }

         if ( type.GetElementKind().HasValue )
         {
            throw new ArgumentException( "This constructor may only be used for non-array, non-pointer, and non-byref types." );
         }
         var nGDef = isGDef ? type : null;
         var tAttrs = (TypeAttributes) type
#if WINDOWS_PHONE_APP
         .GetTypeInfo()
#endif
.Attributes;
         var bType = type
#if WINDOWS_PHONE_APP
         .GetTypeInfo()
#endif
.BaseType;

         InitFields(
            ctx,
            ref this.typeAttributes,
            ref this.elementKind,
            ref this.arrayInfo,
            ref this.gArgs,
            ref this.genericDefinition,
            ref this.nestedTypes,
            ref this.fields,
            ref this.elementType,
            ref this.methods,
            ref this.ctors,
            ref this.properties,
            ref this.events,
            ref this.layout,
            ref this.baseType,
            ref this.declaredInterfaces,
            ref this.securityInfo,
            ref this.explicitMethodImplementationMap,
            new SettableValueForEnums<TypeAttributes>( tAttrs ),
            null,
            null,
            () => ctx.CollectionsFactory.NewListProxy<CILTypeBase>(
               type.GetGenericArguments()
                  .Select( gArg => ctx.Cache.GetOrAdd( gArg ) )
                  .ToList() ),
            () => (CILType) ctx.Cache.GetOrAdd( nGDef ),
            new Lazy<ListProxy<CILType>>( () => ctx.CollectionsFactory.NewListProxy<CILType>(
               type.GetNestedTypes(
#if !WINDOWS_PHONE_APP
 System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic
#endif
 )
                  .Select( nested => (CILType) ctx.Cache.GetOrAdd( nested ) )
                  .ToList() ), ctx.LazyThreadSafetyMode ),
            () => ctx.CollectionsFactory.NewListProxy<CILField>(
               type.GetFields(
#if !WINDOWS_PHONE_APP
 System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly
#endif
 )
                  .Select( field => ctx.Cache.GetOrAdd( field ) )
                  .ToList() ),
            () => ctx.Cache.GetOrAdd( type.GetElementType() ),
            () => ctx.CollectionsFactory.NewListProxy<CILMethod>(
               type.GetMethods(
#if !WINDOWS_PHONE_APP
 System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly
#endif
 )
                  .Select( method => ctx.Cache.GetOrAdd( method ) )
                  .ToList()
               ),
            () => ctx.CollectionsFactory.NewListProxy<CILConstructor>(
               type.GetConstructors(
#if !WINDOWS_PHONE_APP
 System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly
#endif
 )
                  .Select( ctor => ctx.Cache.GetOrAdd( ctor ) )
                  .ToList()
               ),
            () => ctx.CollectionsFactory.NewListProxy<CILProperty>(
               type.GetProperties(
#if !WINDOWS_PHONE_APP
 System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly
#endif
 )
                  .Select( property => ctx.Cache.GetOrAdd( property ) )
                  .ToList()
               ),
            () => ctx.CollectionsFactory.NewListProxy<CILEvent>(
               type.GetEvents(
#if !WINDOWS_PHONE_APP
 System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly
#endif
 )
                  .Select( evt => ctx.Cache.GetOrAdd( evt ) )
                  .ToList()
               ),
            LazyFactory.NewWriteableLazy<LogicalClassLayout?>( () =>
            {
               if ( tAttrs.IsExplicitLayout() || tAttrs.IsSequentialLayout() )
               {
                  var layout = ctx.WrapperCallbacks.GetStructLayoutAttributeOrThrow( type );
                  return new LogicalClassLayout( layout.Size
#if !CAM_LOGICAL_IS_SL
// For some reason, SL version of this attribute does not have Pack property...
, layout.Pack
#endif
 );
               }
               else
               {
                  return null;
               }
            }, ctx.LazyThreadSafetyMode ),
            () => (CILType) ctx.Cache.GetOrAdd( bType ),
            () =>
            {
               var iFaces = type.GetInterfaces().GetBottomTypes();
               if ( bType != null )
               {
                  var iFacesSet = new HashSet<Type>( iFaces );
                  var bIFaces = bType.GetInterfaces();
                  foreach ( var iFace in iFaces )
                  {
                     if ( bIFaces.Any( bIFace => Object.Equals( bIFace.GetGenericDefinitionIfContainsGenericParameters(), iFace.GetGenericDefinitionIfContainsGenericParameters() ) ) )
                     {
                        iFacesSet.Remove( iFace );
                     }
                  }
                  iFaces = iFacesSet.ToArray();
               }
               return ctx.CollectionsFactory.NewListProxy<CILType>(
                  iFaces
                  .Select( iFace => (CILType) ctx.Cache.GetOrAdd( iFace ) )
                  .ToList() );
            },
            new Lazy<DictionaryWithRoles<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>>( this.SecurityInfoFromAttributes, ctx.LazyThreadSafetyMode ),
            () => ctx.CollectionsFactory.NewDictionary<CILMethod, ListProxy<CILMethod>, ListProxyQuery<CILMethod>, ListQuery<CILMethod>>(
               type.IsInterface ? null : ( ctx.WrapperCallbacks.GetExplicitlyImplementedMethodsOrThrow( type )
               .ToDictionary(
                  kvp => ctx.NewWrapper( kvp.Key ),
                  kvp => ctx.CollectionsFactory.NewListProxy( kvp.Value.Select( m => ctx.NewWrapper( m ) ).ToList() ) ) ) ),
            true
            );
      }

      internal CILTypeImpl( CILReflectionContextImpl ctx, Int32 anID, CILTypeCode typeCode, CILModule owningModule, String name, CILType declaringType, TypeAttributes attrs )
         : this(
         ctx,
         anID,
         new Lazy<ListProxy<CILCustomAttribute>>( () => ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>(), ctx.LazyThreadSafetyMode ),
         () => typeCode,
         new SettableValueForClasses<String>( name ),
         new SettableValueForClasses<String>( null ),
         () => owningModule,
         () => declaringType,
         () =>
         {
            return attrs.IsInterface() ? null : owningModule.AssociatedMSCorLibModule.GetTypeByName( Consts.OBJECT );
         },
         () => ctx.CollectionsFactory.NewListProxy<CILType>(),
         new SettableValueForEnums<TypeAttributes>( attrs ),
         null,
         null,
         () => ctx.CollectionsFactory.NewListProxy<CILTypeBase>(),
         () => null,
         new Lazy<ListProxy<CILType>>( () => ctx.CollectionsFactory.NewListProxy<CILType>(), ctx.LazyThreadSafetyMode ),
         () => ctx.CollectionsFactory.NewListProxy<CILField>(),
         () => null,
         () => ctx.CollectionsFactory.NewListProxy<CILMethod>(),
         () => ctx.CollectionsFactory.NewListProxy<CILConstructor>(),
         () => ctx.CollectionsFactory.NewListProxy<CILProperty>(),
         () => ctx.CollectionsFactory.NewListProxy<CILEvent>(),
         LazyFactory.NewWriteableLazy<LogicalClassLayout?>( () => null, ctx.LazyThreadSafetyMode ),
         null,
         null,
         true
         )
      {
      }

      internal CILTypeImpl(
         CILReflectionContextImpl ctx,
         Int32 anID,
         Lazy<ListProxy<CILCustomAttribute>> cAttrDataFunc,
         Func<CILTypeCode> typeCode,
         SettableValueForClasses<String> aName,
         SettableValueForClasses<String> aNamespace,
         Func<CILModule> moduleFunc,
         Func<CILType> declaringTypeFunc,
         Func<CILType> baseTypeFunc,
         Func<ListProxy<CILType>> declaredInterfacesFunc,
         SettableValueForEnums<TypeAttributes> typeAttrs,
         ElementKind? anElementKind,
         GeneralArrayInfo arrayInfo,
         Func<ListProxy<CILTypeBase>> gArgsFunc,
         Func<CILType> gDefFunc,
         Lazy<ListProxy<CILType>> nestedTypesFunc,
         Func<ListProxy<CILField>> fieldsFunc,
         Func<CILTypeBase> elementTypeFunc,
         Func<ListProxy<CILMethod>> methodsFunc,
         Func<ListProxy<CILConstructor>> ctorsFunc,
         Func<ListProxy<CILProperty>> propertiesFunc,
         Func<ListProxy<CILEvent>> eventsFunc,
         WriteableLazy<LogicalClassLayout?> aLayout,
         Lazy<DictionaryWithRoles<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>> aSecurityInfo,
         Func<DictionaryWithRoles<CILMethod, ListProxy<CILMethod>, ListProxyQuery<CILMethod>, ListQuery<CILMethod>>> anExplicitMethodImplementationMap,
         Boolean resettablesAreSettable = false
         )
         : base( ctx, anID, cAttrDataFunc, TypeKind.Type, typeCode, aName, aNamespace, moduleFunc, declaringTypeFunc, resettablesAreSettable )
      {
         InitFields(
            ctx,
            ref this.typeAttributes,
            ref this.elementKind,
            ref this.arrayInfo,
            ref this.gArgs,
            ref this.genericDefinition,
            ref this.nestedTypes,
            ref this.fields,
            ref this.elementType,
            ref this.methods,
            ref this.ctors,
            ref this.properties,
            ref this.events,
            ref this.layout,
            ref this.baseType,
            ref this.declaredInterfaces,
            ref this.securityInfo,
            ref this.explicitMethodImplementationMap,
            typeAttrs,
            anElementKind,
            arrayInfo,
            gArgsFunc,
            gDefFunc,
            nestedTypesFunc,
            fieldsFunc,
            elementTypeFunc,
            methodsFunc,
            ctorsFunc,
            propertiesFunc,
            eventsFunc,
            aLayout,
            baseTypeFunc,
            declaredInterfacesFunc,
            aSecurityInfo ?? new Lazy<DictionaryWithRoles<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>>( () => ctx.CollectionsFactory.NewDictionary<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>(), ctx.LazyThreadSafetyMode ),
            anExplicitMethodImplementationMap ?? ( () => ctx.CollectionsFactory.NewDictionary<CILMethod, ListProxy<CILMethod>, ListProxyQuery<CILMethod>, ListQuery<CILMethod>>() ),
            resettablesAreSettable
            );
      }

      private static void InitFields(
         CILReflectionContextImpl ctx,
         ref SettableValueForEnums<TypeAttributes> typeAttrs,
         ref ElementKind? elementKind,
         ref GeneralArrayInfo arrayInfo,
         ref Lazy<ListProxy<CILTypeBase>> gArgs,
         ref WriteableLazy<CILType> genericDefinition,
         ref Lazy<ListProxy<CILType>> nested,
         ref ReadOnlyResettableLazy<ListProxy<CILField>> fields,
         ref Lazy<CILTypeBase> elementType,
         ref ReadOnlyResettableLazy<ListProxy<CILMethod>> methods,
         ref ReadOnlyResettableLazy<ListProxy<CILConstructor>> ctors,
         ref ReadOnlyResettableLazy<ListProxy<CILProperty>> properties,
         ref ReadOnlyResettableLazy<ListProxy<CILEvent>> events,
         ref WriteableLazy<LogicalClassLayout?> layout,
         ref IResettableLazy<CILType> baseType,
         ref ReadOnlyResettableLazy<ListProxy<CILType>> declaredInterfaces,
         ref Lazy<DictionaryWithRoles<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>> securityInfo,
         ref ReadOnlyResettableLazy<DictionaryWithRoles<CILMethod, ListProxy<CILMethod>, ListProxyQuery<CILMethod>, ListQuery<CILMethod>>> explicitMethodImplementationMap,
         SettableValueForEnums<TypeAttributes> typeAttrsVal,
         ElementKind? anElementKind,
         GeneralArrayInfo anArrayInfo,
         Func<ListProxy<CILTypeBase>> gArgsFunc,
         Func<CILType> genericDefinitionFunc,
         Lazy<ListProxy<CILType>> nestedTypesFunc,
         Func<ListProxy<CILField>> fieldsFunc,
         Func<CILTypeBase> elementTypeFunc,
         Func<ListProxy<CILMethod>> methodsFunc,
         Func<ListProxy<CILConstructor>> ctorsFunc,
         Func<ListProxy<CILProperty>> propertiesFunc,
         Func<ListProxy<CILEvent>> eventsFunc,
         WriteableLazy<LogicalClassLayout?> aLayout,
         Func<CILType> baseTypeFunc,
         Func<ListProxy<CILType>> declaredInterfacesFunc,
         Lazy<DictionaryWithRoles<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>> aSecurityInfo,
         Func<DictionaryWithRoles<CILMethod, ListProxy<CILMethod>, ListProxyQuery<CILMethod>, ListQuery<CILMethod>>> anExplicitMethodImplementationMap,
         Boolean resettablesAreSettable
         )
      {
         var lazyThreadSafety = ctx.LazyThreadSafetyMode;
         typeAttrs = typeAttrsVal;
         elementKind = anElementKind;
         arrayInfo = anArrayInfo;
         gArgs = new Lazy<ListProxy<CILTypeBase>>( gArgsFunc, lazyThreadSafety );
         genericDefinition = LazyFactory.NewWriteableLazy( genericDefinitionFunc, lazyThreadSafety );
         nested = nestedTypesFunc;
         fields = LazyFactory.NewReadOnlyResettableLazy( fieldsFunc, lazyThreadSafety );
         elementType = new Lazy<CILTypeBase>( elementTypeFunc, lazyThreadSafety );
         methods = LazyFactory.NewReadOnlyResettableLazy( methodsFunc, lazyThreadSafety );
         ctors = LazyFactory.NewReadOnlyResettableLazy( ctorsFunc, lazyThreadSafety );
         properties = LazyFactory.NewReadOnlyResettableLazy( propertiesFunc, lazyThreadSafety );
         events = LazyFactory.NewReadOnlyResettableLazy( eventsFunc, lazyThreadSafety );
         baseType = LazyFactory.NewResettableLazy( resettablesAreSettable, baseTypeFunc, lazyThreadSafety );
         declaredInterfaces = LazyFactory.NewReadOnlyResettableLazy( declaredInterfacesFunc, lazyThreadSafety );
         layout = aLayout;
         securityInfo = aSecurityInfo;
         explicitMethodImplementationMap = LazyFactory.NewReadOnlyResettableLazy( anExplicitMethodImplementationMap, lazyThreadSafety );
      }

      public override String ToString()
      {
         return LogicalUtils.CreateTypeString( this, true );
      }

      internal override String GetNameString()
      {
         // TODO move this to Utils.
         var eKind = this.elementKind;
         return eKind.HasValue ? ( ( (CILTypeOrParameterImpl) this.elementType.Value ).GetNameString() + LogicalUtils.CreateElementKindString( eKind.Value, this.arrayInfo ) ) : base.GetNameString();
      }


      #region CILType Members

      public TypeAttributes Attributes
      {
         set
         {
            if ( value.IsInterface() )
            {
               this.GetTypeCapableOfChanging().BaseType = null;
            }
            this.typeAttributes.Value = value;
         }
         get
         {
            return this.typeAttributes.Value;
         }
      }

      public CILType BaseType
      {
         set
         {
            var lazy = this.ThrowIfNotCapableOfChanging( this.baseType );
            lazy.Value = value;
            this.context.Cache.ForAllGenericInstancesOf( this, type => type.ResetBaseType() );
         }
         get
         {
            return this.baseType.Value;
         }
      }

      public CILTypeBase ElementType
      {
         get
         {
            return this.elementType.Value;
         }
      }

      public LogicalClassLayout? Layout
      {
         set
         {
            //if ( value.HasValue && this.typeAttributes.Value.IsInterface() )
            //{
            //   throw new InvalidOperationException( "Setting class layout is not possible for interfaces." );
            //}
            this.layout.Value = value;
         }
         get
         {
            return this.layout.Value;
         }
      }

      public CILField AddField( String name, CILTypeBase fieldType, FieldAttributes attr )
      {
         this.ThrowIfNotCapableOfChanging();
         var result = this.fields.AddToResettableLazyList( this.context.Cache.NewBlankField( this, name, attr, fieldType ) );
         this.context.Cache.ForAllGenericInstancesOf( this, type => type.ResetDeclaredFields() );
         return result;
      }

      public Boolean RemoveField( CILField field )
      {
         this.ThrowIfNotCapableOfChanging();
         var result = this.fields.Value.Remove( field );
         if ( result )
         {
            this.context.Cache.ForAllGenericInstancesOf( this, type => type.ResetDeclaredFields() );
         }
         return result;
      }

      public CILConstructor AddConstructor( MethodAttributes attrs, CallingConventions callingConventions )
      {
         this.ThrowIfNotCapableOfChanging();
         var result = this.ctors.AddToResettableLazyList( this.context.Cache.NewBlankConstructor( this, attrs ) );
         this.context.Cache.ForAllGenericInstancesOf( this, type => type.ResetConstructors() );
         return result;
      }

      public Boolean RemoveConstructor( CILConstructor ctor )
      {
         this.ThrowIfNotCapableOfChanging();
         var result = this.ctors.Value.Remove( ctor );
         if ( result )
         {
            this.context.Cache.ForAllGenericInstancesOf( this, type => type.ResetConstructors() );
         }
         return result;
      }

      public CILMethod AddMethod( String name, MethodAttributes attrs, CallingConventions callingConventions )
      {
         this.ThrowIfNotCapableOfChanging();
         var result = this.methods.AddToResettableLazyList( this.context.Cache.NewBlankMethod( this, name, attrs, callingConventions ) );
         this.context.Cache.ForAllGenericInstancesOf( this, type => type.ResetDeclaredMethods() );
         return result;
      }

      public Boolean RemoveMethod( CILMethod method )
      {
         this.ThrowIfNotCapableOfChanging();
         var result = this.methods.Value.Remove( method );
         if ( result )
         {
            this.context.Cache.ForAllGenericInstancesOf( this, type => type.ResetDeclaredMethods() );
         }
         return result;
      }

      public CILProperty AddProperty( String name, PropertyAttributes attrs )
      {
         this.ThrowIfNotCapableOfChanging();
         var result = this.properties.AddToResettableLazyList( this.context.Cache.NewBlankProperty( this, name, attrs ) );
         this.context.Cache.ForAllGenericInstancesOf( this, type => type.ResetDeclaredProperties() );
         return result;
      }

      public Boolean RemoveProperty( CILProperty property )
      {
         this.ThrowIfNotCapableOfChanging();
         var result = this.properties.Value.Remove( property );
         if ( result )
         {
            this.context.Cache.ForAllGenericInstancesOf( this, type => type.ResetDeclaredProperties() );
         }
         return result;
      }

      public CILEvent AddEvent( String name, EventAttributes attrs, CILTypeBase eventType )
      {
         this.ThrowIfNotCapableOfChanging();
         var result = this.events.AddToResettableLazyList( this.context.Cache.NewBlankEvent( this, name, attrs, eventType ) );
         this.context.Cache.ForAllGenericInstancesOf( this, type => type.ResetDeclaredEvents() );
         return result;
      }

      public Boolean RemoveEvent( CILEvent evt )
      {
         this.ThrowIfNotCapableOfChanging();
         var result = this.events.Value.Remove( evt );
         if ( result )
         {
            this.context.Cache.ForAllGenericInstancesOf( this, type => type.ResetDeclaredEvents() );
         }
         return result;
      }

      public void ChangeTypeCode( CILTypeCode newTypeCode )
      {
         this.ThrowIfNotCapableOfChanging();
         // TODO maybe throw if generic parameters are present?
         this.typeCode.Value = newTypeCode;
      }

      //public CILType ForwardedType
      //{
      //   get
      //   {
      //      return this.forwardedType.Value;
      //   }
      //   set
      //   {
      //      this.ThrowIfNotCapableOfChanging();
      //      this.forwardedType.Value = value;
      //   }
      //}

      #endregion

      #region CILElementWithGenericArguments<CILType> Members

      public CILType GenericDefinition
      {
         get
         {
            return this.genericDefinition.Value;
         }
      }

      #endregion

      #region CILTypeInternal Members

      SettableValueForEnums<TypeAttributes> CILTypeInternal.TypeAttributesInternal
      {
         get
         {
            return this.typeAttributes;
         }
      }

      Lazy<ListProxy<CILType>> CILTypeInternal.NestedTypesInternal
      {
         get
         {
            return this.nestedTypes;
         }
      }

      WriteableLazy<LogicalClassLayout?> CILTypeInternal.ClassLayoutInternal
      {
         get
         {
            return this.layout;
         }
      }

      //SettableLazy<CILType> CILTypeInternal.ForwardedTypeInternal
      //{
      //   get
      //   {
      //      return this.forwardedType;
      //   }
      //}

      void CILTypeInternal.ResetDeclaredInterfaces()
      {
         this.declaredInterfaces.Reset();
      }

      void CILTypeInternal.ResetDeclaredMethods()
      {
         this.methods.Reset();
      }

      void CILTypeInternal.ResetDeclaredFields()
      {
         this.fields.Reset();
      }

      void CILTypeInternal.ResetConstructors()
      {
         this.ctors.Reset();
      }

      void CILTypeInternal.ResetDeclaredProperties()
      {
         this.properties.Reset();
      }

      void CILTypeInternal.ResetDeclaredEvents()
      {
         this.events.Reset();
      }

      void CILTypeInternal.ResetBaseType()
      {
         this.baseType.Reset();
      }

      void CILTypeInternal.ResetExplicitMethods()
      {
         this.explicitMethodImplementationMap.Reset();
      }

      #endregion

      public override String Name
      {
         set
         {
            if ( this.elementKind.HasValue )
            {
               throw new InvalidOperationException( "Can not set name on " + this.elementKind + " type." );
            }
            else
            {
               ( (CILModuleImpl) this.module.Value ).TypeNameChanged( base.name.Value );
               base.Name = value;
            }
         }
         get
         {
            return this.GetNameString();
         }
      }

      public void AddDeclaredInterfaces( params CILType[] iFaces )
      {
         this.ThrowIfNotCapableOfChanging();
         if ( iFaces.Any( iFace => TypeKind.Type != iFace.TypeKind ) )
         {
            throw new ArgumentException( "Given types must be actual types (not type parameters or method signatures)." );
         }
         // Since declared interfaces are mutable, no point checking this...
         //iFaces.SelectMany( iFace => iFace.AsDepthFirstEnumerable( i => i.DeclaredInterfaces ) ).CheckCyclity( this );
         this.declaredInterfaces.Value.AddRange( iFaces.OnlyBottomTypes().Except( this.declaredInterfaces.Value.MQ ) );
         this.context.Cache.ForAllGenericInstancesOf( this, type => type.ResetDeclaredInterfaces() );
      }

      public Boolean RemoveDeclaredInterface( CILType iFace )
      {
         this.ThrowIfNotCapableOfChanging();
         Boolean result;
         result = this.declaredInterfaces.Value.Remove( iFace );
         if ( result )
         {
            this.context.Cache.ForAllGenericInstancesOf( this, type => type.ResetDeclaredInterfaces() );
         }
         return result;
      }

      internal override String IsCapableOfChanging()
      {
         if ( this.IsGenericType() && !this.IsGenericTypeDefinition() )
         {
            return "Type is generic type instance.";
         }
         //else if ( this.elementKind.HasValue ) // If this limitation will be removed, the CILReflectionContextCache.EMPTY_* fields must be changed from ListProxy<...> to ...[] arrays.
         //{
         //   return "Type is array, pointer or by-ref type.";
         //}
         else
         {
            return null;
         }
      }

      private CILType GetTypeCapableOfChanging()
      {
         return this.IsCapableOfChanging() == null ? this : this.genericDefinition.Value;
      }

      #region CILElementCapableOfDefiningType Members

      public CILType AddType( String name, TypeAttributes attrs, CILTypeCode tc = CILTypeCode.Object )
      {
         this.ThrowIfNotCapableOfChanging();
         var result = this.context.Cache.NewBlankType( this.module.Value, this, name, attrs, tc );
         this.nestedTypes.Value.Add( result );
         return result;
      }

      public Boolean RemoveType( CILType type )
      {
         this.ThrowIfNotCapableOfChanging();
         //( (CILTypeInternal) type ).RemoveDeclaringType(); // TODO
         return this.nestedTypes.Value.Remove( type );
      }

      #endregion

      #region CILElementWithGenericArguments<CILType> Members


      public CILTypeParameter[] DefineGenericParameters( String[] names )
      {
         CILTypeParameter[] result;

         LogicalUtils.CheckWhenDefiningGArgs( this.gArgs.Value, names );
         if ( names != null && names.Length > 0 )
         {
            result = Enumerable.Range( 0, names.Length ).Select( idx => this.context.Cache.NewBlankTypeParameter( this, null, names[idx], idx ) ).ToArray();
            this.gArgs.Value.AddRange( result );
            this.genericDefinition.Value = this;
         }
         else
         {
            result = EMPTY_TYPE_PARAMS;
         }

         return result;
      }

      #endregion


      #region CILType Members


      public ElementKind? ElementKind
      {
         get
         {
            return this.elementKind;
         }
      }

      public GeneralArrayInfo ArrayInformation
      {
         get
         {
            return this.arrayInfo;
         }
      }

      public ListQuery<CILField> DeclaredFields
      {
         get
         {
            return this.fields.Value.CQ;
         }
      }

      public ListQuery<CILConstructor> Constructors
      {
         get
         {
            return this.ctors.Value.CQ;
         }
      }

      public ListQuery<CILType> DeclaredNestedTypes
      {
         get
         {
            return this.nestedTypes.Value.CQ;
         }
      }

      public ListQuery<CILProperty> DeclaredProperties
      {
         get
         {
            return this.properties.Value.CQ;
         }
      }

      public ListQuery<CILEvent> DeclaredEvents
      {
         get
         {
            return this.events.Value.CQ;
         }
      }

      public ListQuery<CILType> DeclaredInterfaces
      {
         get
         {
            return this.declaredInterfaces.Value.CQ;
         }
      }

      public CILType MakeGenericType( params CILTypeBase[] args )
      {
         return this.context.Cache.MakeGenericType( this, this.GenericDefinition, args );
      }

      public void AddExplicitMethodImplementation( CILMethod methodBody, params CILMethod[] methodDeclarations )
      {
         this.ThrowIfNotTrueDefinition();
         var map = this.explicitMethodImplementationMap.Value;
         ListProxy<CILMethod> list;
         if ( !map.MQ.TryGetValue( methodBody, out list ) )
         {
            list = this.context.CollectionsFactory.NewListProxy<CILMethod>();
            map.Add( methodBody, list );
         }
         list.AddRange( methodDeclarations );

         this.context.Cache.ForAllGenericInstancesOf( this, type => type.ResetExplicitMethods() );
      }

      public Boolean RemoveExplicitMethodImplementation( CILMethod methodBody, params CILMethod[] methodDeclarations )
      {
         this.ThrowIfNotTrueDefinition();
         var map = this.explicitMethodImplementationMap.Value;
         ListProxy<CILMethod> list;
         var retVal = false;
         if ( map.MQ.TryGetValue( methodBody, out list ) )
         {
            foreach ( var method in methodDeclarations )
            {
               retVal = list.Remove( method ) || retVal;
            }
            if ( list.MQ.Count <= 0 )
            {
               map.Remove( methodBody );
            }

            if ( retVal )
            {
               this.context.Cache.ForAllGenericInstancesOf( this, type => type.ResetExplicitMethods() );
            }
         }

         return retVal;
      }

      public DictionaryQuery<CILMethod, ListQuery<CILMethod>> ExplicitMethodImplementations
      {
         get
         {
            return this.explicitMethodImplementationMap.Value.CQ.IQ;
         }
      }

      #endregion

      #region CILElementWithGenericArguments<CILType> Members


      public ListQuery<CILTypeBase> GenericArguments
      {
         get
         {
            return this.gArgs.Value.CQ;
         }
      }

      #endregion

      #region CapableOfDefiningMethod Members


      public ListQuery<CILMethod> DeclaredMethods
      {
         get
         {
            return this.methods.Value.CQ;
         }
      }

      #endregion

      public DictionaryQuery<SecurityAction, ListQuery<LogicalSecurityInformation>> DeclarativeSecurity
      {
         get
         {
            return this.securityInfo.Value.CQ.IQ;
         }
      }

      public LogicalSecurityInformation AddDeclarativeSecurity( SecurityAction action, CILType securityAttributeType )
      {
         lock ( this.securityInfo.Value )
         {
            ListProxy<LogicalSecurityInformation> list;
            if ( !this.securityInfo.Value.CQ.TryGetValue( action, out list ) )
            {
               list = this.context.CollectionsFactory.NewListProxy<LogicalSecurityInformation>();
               this.securityInfo.Value.Add( action, list );
            }
            var result = new LogicalSecurityInformation( action, securityAttributeType );
            list.Add( result );
            return result;
         }
      }

      public Boolean RemoveDeclarativeSecurity( LogicalSecurityInformation information )
      {
         lock ( this.securityInfo.Value )
         {
            var result = information != null;
            if ( !result )
            {
               ListProxy<LogicalSecurityInformation> list;
               if ( this.securityInfo.Value.CQ.TryGetValue( information.SecurityAction, out list ) )
               {
                  result = list.Remove( information );
               }
            }
            return result;
         }
      }

      internal Lazy<DictionaryWithRoles<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>> DeclarativeSecurityInternal
      {
         get
         {
            return this.securityInfo;
         }
      }
   }

   internal class CILTypeParameterImpl : CILTypeOrParameterImpl, CILTypeParameter
   {
      private GenericParameterAttributes paramAttributes;
      private readonly Int32 position;
      private readonly Lazy<CILMethod> declaringMethod;
      private readonly Lazy<ListProxy<CILTypeBase>> genericParameterConstraints;

      internal CILTypeParameterImpl( CILReflectionContextImpl ctx, Int32 anID, Type type )
         : base( ctx, anID, type )
      {
         if ( TypeKind.TypeParameter != this.typeKind )
         {
            throw new ArgumentException( "Trying to create type parameter for type " + type );
         }
         InitFields(
            ctx,
            ref this.paramAttributes,
            ref this.position,
            ref this.declaringMethod,
            ref this.genericParameterConstraints,
            (GenericParameterAttributes) type
#if WINDOWS_PHONE_APP
         .GetTypeInfo()
#endif
.GenericParameterAttributes,
            type.GenericParameterPosition,
            () => ctx.Cache.GetOrAdd( (System.Reflection.MethodInfo) type
#if WINDOWS_PHONE_APP
         .GetTypeInfo()
#endif
.DeclaringMethod ),
            () => ctx.CollectionsFactory.NewListProxy<CILTypeBase>( type
#if WINDOWS_PHONE_APP
         .GetTypeInfo()
#endif
.GetGenericParameterConstraints().Select( constraint => ctx.Cache.GetOrAdd( constraint ) ).ToList() )
            );
      }

      internal CILTypeParameterImpl(
         CILReflectionContextImpl ctx,
         Int32 anID,
         Lazy<ListProxy<CILCustomAttribute>> cAttrs,
         GenericParameterAttributes gpAttrs,
         CILType declaringType,
         CILMethod declaringMethod,
         String aName,
         Int32 aPosition,
         Func<ListProxy<CILTypeBase>> constraintsFunc
         )
         : base(
         ctx,
         anID,
         cAttrs,
         TypeKind.TypeParameter,
         () => CILTypeCode.Object,
         new SettableValueForClasses<String>( aName ),
         new SettableValueForClasses<String>( null ),
         () => declaringType.Module,
         () => declaringType,
         true
         )
      {
         InitFields(
            ctx,
            ref this.paramAttributes,
            ref this.position,
            ref this.declaringMethod,
            ref this.genericParameterConstraints,
            gpAttrs,
            aPosition,
            () => declaringMethod,
            constraintsFunc
            );
      }

      private static void InitFields(
         CILReflectionContextImpl ctx,
         ref GenericParameterAttributes paramAttributes,
         ref Int32 position,
         ref Lazy<CILMethod> declaringMethod,
         ref Lazy<ListProxy<CILTypeBase>> genericParameterConstraints,
         GenericParameterAttributes aParamAttributes,
         Int32 aPosition,
         Func<CILMethod> declaringMethodFunc,
         Func<ListProxy<CILTypeBase>> genericParameterConstraintsFunc
         )
      {
         var lazyThreadSafety = ctx.LazyThreadSafetyMode;
         paramAttributes = aParamAttributes;
         position = aPosition;
         declaringMethod = new Lazy<CILMethod>( declaringMethodFunc, lazyThreadSafety );
         genericParameterConstraints = new Lazy<ListProxy<CILTypeBase>>( genericParameterConstraintsFunc, lazyThreadSafety );
      }

      public override String ToString()
      {
         return this.name.Value;
      }

      #region CILTypeParameter Members

      public GenericParameterAttributes Attributes
      {
         set
         {
            this.paramAttributes = value;
         }
         get
         {
            return this.paramAttributes;
         }
      }

      public CILMethod DeclaringMethod
      {
         get
         {
            return this.declaringMethod.Value;
         }
      }

      public void AddGenericParameterConstraints( params CILTypeBase[] constraints )
      {
         this.genericParameterConstraints.Value.AddRange( constraints.Except( this.genericParameterConstraints.Value.CQ ) );
      }

      public Boolean RemoveGenericParameterConstraint( CILTypeBase constraint )
      {
         return this.genericParameterConstraints.Value.Remove( constraint );
      }

      public Int32 GenericParameterPosition
      {
         get
         {
            return this.position;
         }
      }

      public ListQuery<CILTypeBase> GenericParameterConstraints
      {
         get
         {
            return this.genericParameterConstraints.Value.CQ;
         }
      }

      #endregion

      internal override String IsCapableOfChanging()
      {
         // Always capable of changing
         return null;
      }
   }

   // Reason for method sig immutability:
   // use it in other module -> copy is created
   // modify original -> copy is not modified! (modify copy -> original is not modified)
   // -> behavious is very tricky from user point of view
   internal class CILMethodSignatureImpl : AbstractSignatureElement, CILMethodSignature, CILTypeBaseInternal
   {
      private readonly MethodSignatureInformation _callConv;
      private readonly CILModule _module;
      private readonly ListQuery<CILParameterSignature> _params;
      private readonly CILParameterSignature _returnParam;
      private readonly SettableValueForClasses<String> _nsDummy;
      private readonly Lazy<SettableValueForClasses<String>> _nameDummy;
      private readonly CILMethodBase _originatingMethod;

      internal CILMethodSignatureImpl( CILReflectionContextImpl ctx, CILModule module, MethodSignatureInformation callConv, ListProxy<CILCustomModifier> rpCMods, CILTypeBase rpType, IList<Tuple<ListProxy<CILCustomModifier>, CILTypeBase>> paramsInfo, CILMethodBase originatingMethod )
         : base( ctx )
      {
         ArgumentValidator.ValidateNotNull( "Module", module );
         ArgumentValidator.ValidateNotNull( "Return parameter type", rpType );
         if ( paramsInfo != null )
         {
            foreach ( var p in paramsInfo )
            {
               ArgumentValidator.ValidateNotNull( "Parameter type", p.Item2 );
            }
         }

         this._module = module;
         this._callConv = callConv;
         this._returnParam = new CILParameterSignatureImpl( ctx, this, E_CILLogical.RETURN_PARAMETER_POSITION, rpType, rpCMods );
         var parameters = this._ctx.CollectionsFactory.NewListProxy<CILParameterSignature>();
         if ( paramsInfo != null )
         {
            parameters.AddRange( paramsInfo.Select( ( tuple, idx ) => new CILParameterSignatureImpl( ctx, this, idx, paramsInfo[idx].Item2, paramsInfo[idx].Item1 ) ) );
         }
         this._params = parameters.CQ;

         this._nsDummy = new SettableValueForClasses<string>( null );
         this._nameDummy = new Lazy<SettableValueForClasses<string>>( () => new SettableValueForClasses<string>( this.ToString() ), ctx.LazyThreadSafetyMode );
         this._originatingMethod = originatingMethod;
      }

      private CILMethodSignatureImpl( CILMethodSignatureImpl other, CILModule newModule )
         : base( other._ctx )
      {
         this._module = newModule;
         this._callConv = other._callConv;
         this._returnParam = other._returnParam;
         this._params = other._params;
         this._nsDummy = other._nsDummy;
         this._nameDummy = other._nameDummy;
         this._originatingMethod = other._originatingMethod;
      }

      #region CILMethodSignature Members

      public CILMethodSignature CopyToOtherModule( CILModule newModule )
      {
         return new CILMethodSignatureImpl( this, newModule );
      }

      public CILMethodBase OriginatingMethod
      {
         get
         {
            return this._originatingMethod;
         }
      }

      #endregion

      #region CILTypeBase Members

      public TypeKind TypeKind
      {
         get
         {
            return TypeKind.MethodSignature;
         }
      }

      public CILModule Module
      {
         get
         {
            return this._module;
         }
      }

      public CILType MakeElementType( ElementKind kind, GeneralArrayInfo arrayInfo )
      {
         return this._ctx.Cache.MakeElementType( this, kind, arrayInfo );
      }

      #endregion

      #region CILMethodOrSignature<CILParameterSignature> Members

      public MethodSignatureInformation CallingConvention
      {
         get
         {
            return this._callConv;
         }
      }


      public ListQuery<CILParameterSignature> Parameters
      {
         get
         {
            return this._params;
         }
      }

      #endregion

      #region CILMethodWithReturnParameter<CILParameterSignature> Members

      public CILParameterSignature ReturnParameter
      {
         get
         {
            return this._returnParam;
         }
      }

      #endregion

      public override Boolean Equals( Object obj )
      {
         return this.Equals( obj as CILMethodSignature );
      }

      public override Int32 GetHashCode()
      {
         return ( ( (Int32) this._callConv ) << 24 ) | this._params.Count;
      }

      public override String ToString()
      {
         // TODO: method <type> <mods> *(<type> <mods>)
         return "method " + this._returnParam + " *(" + String.Join( ", ", this._params.Select( p => p.ToString() ).Concat( Enumerable.Repeat( "...", this._callConv.IsVarArg() ? 1 : 0 ) ) ) + ")";
      }

      internal Boolean Equals( CILMethodSignature other )
      {
         // Don't check module when checking equality.
         return Object.ReferenceEquals( this, other )
            || ( other != null
               && this.CallingConvention == other.CallingConvention
               && ( (CILParameterSignatureImpl) this._returnParam ).Equals( other.ReturnParameter, false )
               && this._params.Count == other.Parameters.Count
               && this._params.Where( ( p, idx ) => ( (CILParameterSignatureImpl) p ).Equals( other.Parameters[idx], false ) ).Count() == this._params.Count
               );
      }

      #region CILTypeBaseInternal Members

      SettableValueForClasses<string> CILTypeBaseInternal.NamespaceInternal
      {
         get
         {
            return this._nsDummy;
         }
      }

      #endregion

      #region CILElementWithSimpleNameInternal Members

      SettableValueForClasses<string> CILElementWithSimpleNameInternal.NameInternal
      {
         get
         {
            return this._nameDummy.Value;
         }
      }

      #endregion

      public CILElementWithinILCode ElementTypeKind
      {
         get
         {
            return CILElementWithinILCode.Type;
         }
      }
   }
}