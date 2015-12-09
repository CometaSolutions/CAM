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
   internal abstract class CILMethodBaseImpl : CILCustomAttributeContainerImpl, CILMethodBase, CILMethodBaseInternal
   {
      private readonly SettableValueForEnums<CallingConventions> callingConvention;
      protected internal readonly SettableValueForEnums<MethodAttributes> methodAttributes;
      private readonly MethodKind methodKind;
      protected internal readonly Lazy<CILType> declaringType;
      private readonly ReadOnlyResettableLazy<ListProxy<CILParameter>> parameters;
      private readonly WriteableLazy<MethodIL> il;
      // Use SettableLazy instead of SettableValue here - we don't want to force this non-portable event to trigger every time.
      private readonly WriteableLazy<MethodImplAttributes> methodImplementationAttributes;
      private readonly Lazy<DictionaryWithRoles<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>> securityInfo;

      protected CILMethodBaseImpl(
         CILReflectionContextImpl ctx,
         Int32 anID,
         System.Reflection.MethodBase method
         )
         : base( ctx, anID, ( method is System.Reflection.ConstructorInfo ) ? CILElementKind.Constructor : CILElementKind.Method, cb => cb.GetCustomAttributesDataForOrThrow( method ) )
      {
         ArgumentValidator.ValidateNotNull( "Method", method );

         if ( method.DeclaringType
#if WINDOWS_PHONE_APP
            .GetTypeInfo()
#endif
.IsGenericType && !method.DeclaringType
#if WINDOWS_PHONE_APP
            .GetTypeInfo()
#endif
.IsGenericTypeDefinition )
         {
            throw new ArgumentException( "This constructor may be used only on methods declared in genericless types or generic type definitions." );
         }
         if ( method is System.Reflection.MethodInfo && method.GetGenericArguments().Length > 0 && !method.IsGenericMethodDefinition )
         {
            throw new ArgumentException( "This constructor may be used only on genericless methods or generic method definitions." );
         }

         InitFields(
            ctx,
            ref this.callingConvention,
            ref this.methodAttributes,
            ref this.methodKind,
            ref this.declaringType,
            ref this.parameters,
            ref this.il,
            ref this.methodImplementationAttributes,
            ref this.securityInfo,
            new SettableValueForEnums<CallingConventions>( (CallingConventions) method.CallingConvention ),
            new SettableValueForEnums<MethodAttributes>( (MethodAttributes) method.Attributes ),
            this.cilKind == CILElementKind.Constructor ? MethodKind.Constructor : MethodKind.Method,
            () => (CILType) ctx.Cache.GetOrAdd( method.DeclaringType ),
            () => ctx.CollectionsFactory.NewListProxy<CILParameter>( method.GetParameters().Select( param => ctx.Cache.GetOrAdd( param ) ).ToList() ),
            () =>
            {
               MethodIL result;
               if ( this.HasILMethodBody() )
               {
                  IEnumerable<Tuple<Boolean, Type>> locals;
                  Boolean initLocals;
                  IEnumerable<Tuple<ExceptionBlockType, Int32, Int32, Int32, Int32, Type, Int32>> exceptionBlocks;
                  Byte[] ilBytes;
                  ctx.WrapperCallbacks.GetMethodBodyDataOrThrow( method, out locals, out initLocals, out exceptionBlocks, out ilBytes );

                  result = new MethodILImpl(
                     this,
                     initLocals,
                     ilBytes,
                     locals.Select( t => Tuple.Create( t.Item1, ctx.NewWrapper( t.Item2 ) ) ).ToArray(),
                     exceptionBlocks.Select( t => Tuple.Create( t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, ctx.NewWrapperAsType( t.Item6 ), t.Item7 ) ).ToArray(),
                     CreateNativeTokenResolverCallback( ctx, method )
                     );
               }
               else
               {
                  result = null;
               }
               return result;
            },
            LazyFactory.NewWriteableLazy( () => ctx.WrapperCallbacks.GetMethodImplementationAttributesOrThrow( method ), ctx.LazyThreadSafetyMode ),
            new Lazy<DictionaryWithRoles<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>>( this.SecurityInfoFromAttributes, ctx.LazyThreadSafetyMode ),
            true
            );
      }

      private static Func<Int32, ILResolveKind, Object> CreateNativeTokenResolverCallback( CILReflectionContextImpl ctx, System.Reflection.MethodBase method )
      {
         var dType = method.DeclaringType;
         var module = new Lazy<System.Reflection.Module>( () => ctx.WrapperCallbacks.GetModuleOfTypeOrThrow( dType ), ctx.LazyThreadSafetyMode );


         var gArgs = dType
#if WINDOWS_PHONE_APP
            .GetTypeInfo()
#endif
.IsGenericType ? dType.GetGenericTypeDefinition().GetGenericArguments() : null;
         var mgArgs = method is System.Reflection.MethodInfo && method.GetGenericArguments().Length > 0 ? ( (System.Reflection.MethodInfo) method ).GetGenericMethodDefinition().GetGenericArguments() : null;

         return ( token, rKind ) =>
         {
            switch ( rKind )
            {
               case ILResolveKind.String:
                  return ctx.WrapperCallbacks.ResolveStringOrThrow( module.Value, token );
               case ILResolveKind.Signature:
                  // TODO basically same thing as in ModuleReadingContext, except use this method to resolve tokens.
                  // Maybe could make static methods to ModuleReadingContext (ReadFieldSignature, ReadMethodSignature, ReadType) that would use callbacks to resolve tokens.
                  throw new NotImplementedException( "Implement creating method signature + var args from byte array (at this point)." );
               default:
                  var retVal = ctx.WrapperCallbacks.ResolveTypeOrMemberOrThrow( module.Value, token, gArgs, mgArgs );
                  return retVal is Type ?
                     ctx.Cache.GetOrAdd( (Type) retVal ) :
                        ( retVal is System.Reflection.FieldInfo ?
                           ctx.Cache.GetOrAdd( (System.Reflection.FieldInfo) retVal ) :
                              ( retVal is System.Reflection.MethodInfo ?
                                 (Object) ctx.Cache.GetOrAdd( (System.Reflection.MethodInfo) retVal ) :
                                 ctx.Cache.GetOrAdd( (System.Reflection.ConstructorInfo) retVal )
                                 )
                            );
            }
         };
      }

      protected CILMethodBaseImpl(
         CILReflectionContextImpl ctx,
         Int32 anID,
         Boolean isCtor,
         Lazy<ListProxy<CILCustomAttribute>> cAttrDataFunc,
         SettableValueForEnums<CallingConventions> aCallingConvention,
         SettableValueForEnums<MethodAttributes> aMethodAttributes,
         Func<CILType> declaringTypeFunc,
         Func<ListProxy<CILParameter>> parametersFunc,
         Func<MethodIL> methodIL,
         WriteableLazy<MethodImplAttributes> aMethodImplementationAttributes,
         Lazy<DictionaryWithRoles<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>> aSecurityInfo,
         Boolean resettablesAreSettable
         )
         : base( ctx, isCtor ? CILElementKind.Constructor : CILElementKind.Method, anID, cAttrDataFunc )
      {
         InitFields(
            ctx,
            ref this.callingConvention,
            ref this.methodAttributes,
            ref this.methodKind,
            ref this.declaringType,
            ref this.parameters,
            ref this.il,
            ref this.methodImplementationAttributes,
            ref this.securityInfo,
            aCallingConvention,
            aMethodAttributes,
            isCtor ? MethodKind.Constructor : MethodKind.Method,
            declaringTypeFunc,
            parametersFunc,
            methodIL,
            aMethodImplementationAttributes,
            aSecurityInfo ?? new Lazy<DictionaryWithRoles<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>>( () => ctx.CollectionsFactory.NewDictionary<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>(), ctx.LazyThreadSafetyMode ),
            resettablesAreSettable
            );
      }

      private static void InitFields(
         CILReflectionContextImpl ctx,
         ref SettableValueForEnums<CallingConventions> callingConvention,
         ref SettableValueForEnums<MethodAttributes> methodAttributes,
         ref MethodKind methodKind,
         ref Lazy<CILType> declaringType,
         ref ReadOnlyResettableLazy<ListProxy<CILParameter>> parameters,
         ref WriteableLazy<MethodIL> il,
         ref WriteableLazy<MethodImplAttributes> methodImplementationAttributes,
         ref Lazy<DictionaryWithRoles<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>> securityInfo,
         SettableValueForEnums<CallingConventions> aCallingConvention,
         SettableValueForEnums<MethodAttributes> aMethodAttributes,
         MethodKind aMethodKind,
         Func<CILType> declaringTypeFunc,
         Func<ListProxy<CILParameter>> parametersFunc,
         Func<MethodIL> methodIL,
         WriteableLazy<MethodImplAttributes> aMethodImplementationAttributes,
         Lazy<DictionaryWithRoles<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>> securityInfoLazy,
         Boolean resettablesAreSettable
         )
      {
         var lazyThreadSafety = ctx.LazyThreadSafetyMode;
         callingConvention = aCallingConvention;
         methodAttributes = aMethodAttributes;
         methodKind = aMethodKind;
         declaringType = new Lazy<CILType>( declaringTypeFunc, lazyThreadSafety );
         // TODO is ResettableAndSettableLazy really needed?
         parameters = LazyFactory.NewReadOnlyResettableLazy( parametersFunc, lazyThreadSafety );
         il = LazyFactory.NewWriteableLazy( resettablesAreSettable ? methodIL : () =>
         {
            throw new NotSupportedException( "Emitting IL is not supported for methods with generic non-definition declaring types or generic non-definition methods." );
         }, lazyThreadSafety );
         methodImplementationAttributes = aMethodImplementationAttributes;
         securityInfo = securityInfoLazy;
      }

      public override String ToString()
      {
         return this.GetThisName() + "(" + String.Join( ", ", this.parameters.Value.CQ ) + ")";
      }

      protected abstract String GetThisName();

      #region CILMethodBase Members

      public MethodKind MethodKind
      {
         get
         {
            return this.methodKind;
         }
      }

      public CallingConventions CallingConvention
      {
         set
         {
            this.callingConvention.Value = value;
         }
         get
         {
            return this.callingConvention.Value;
         }
      }

      public virtual MethodAttributes Attributes
      {
         set
         {
            this.methodAttributes.Value = value;
         }
         get
         {
            return this.methodAttributes.Value;
         }
      }

      public CILType DeclaringType
      {
         get
         {
            return this.declaringType.Value;
         }
      }

      public MethodIL MethodIL
      {
         get
         {
            return this.il.Value;
         }
      }

      public MethodImplAttributes ImplementationAttributes
      {
         set
         {
            this.methodImplementationAttributes.Value = value;
         }
         get
         {
            return this.methodImplementationAttributes.Value;
         }
      }

      public CILParameter AddParameter( String name, ParameterAttributes attrs, CILTypeBase paramType )
      {
         this.ThrowIfNotCapableOfChanging();
         CILParameter result;
         result = this.context.Cache.NewBlankParameter( this, this.parameters.Value.CQ.Count, name, attrs, paramType );
         this.parameters.Value.Add( result );
         this.context.Cache.ForAllGenericInstancesOf<CILMethodBase, CILMethodBaseInternal>( (CILMethodBase) this, method => method.ResetParameterList() );
         return result;
      }

      public Boolean RemoveParameter( CILParameter parameter )
      {
         this.ThrowIfNotCapableOfChanging();
         var result = this.parameters.Value.Remove( parameter );
         if ( result )
         {
            this.context.Cache.ForAllGenericInstancesOf<CILMethodBase, CILMethodBaseInternal>( this, method => method.ResetParameterList() );
         }
         return result;
      }

      public ListQuery<CILParameter> Parameters
      {
         get
         {
            return this.parameters.Value.CQ;
         }
      }

      public MethodIL ResetMethodIL()
      {
         MethodIL retVal = this.il.Value;

         if ( retVal != null )
         {
            this.il.Value = new MethodILImpl( this.DeclaringType.Module );
         }
         return retVal;
      }



      /// <summary>
      /// Gets or sets the name of the module for platform invoke method.
      /// </summary>
      /// <value>The name of the module for platform invoke method.</value>
      String PlatformInvokeModule { get; set; }

      #endregion

      #region CILMethodBaseInternal Members

      SettableValueForEnums<CallingConventions> CILMethodBaseInternal.CallingConventionInternal
      {
         get
         {
            return this.callingConvention;
         }
      }

      SettableValueForEnums<MethodAttributes> CILMethodBaseInternal.MethodAttributesInternal
      {
         get
         {
            return this.methodAttributes;
         }
      }

      WriteableLazy<MethodImplAttributes> CILMethodBaseInternal.MethodImplementationAttributesInternal
      {
         get
         {
            return this.methodImplementationAttributes;
         }
      }

      void CILMethodBaseInternal.ResetParameterList()
      {
         this.parameters.Reset();
      }

      #endregion

      #region CILElementOwnedByChangeableTypeUT<CILMethodBase<TMQ,TIQ>> Members

      public CILMethodBase ChangeDeclaringTypeUT( params CILTypeBase[] args )
      {
         // TODO return this if decl type has no gArgs and args is empty.
         LogicalUtils.ThrowIfDeclaringTypeNotGeneric( this, args );
         var gDef = this.declaringType.Value.GenericDefinition;
         var thisMethod = this.ThisOrGDef();
         return this.MakeGenericIfNeeded( this.context.Cache.MakeMethodWithGenericDeclaringType(
            gDef == null ? this : ( MethodKind.Method == this.methodKind ? (CILMethodBase) gDef.DeclaredMethods[this.declaringType.Value.DeclaredMethods.IndexOf( (CILMethod) thisMethod )] : gDef.Constructors[this.declaringType.Value.Constructors.IndexOf( (CILConstructor) thisMethod )] ),
            args
         ) );
      }

      #endregion

      protected abstract CILMethodBase ThisOrGDef();
      protected abstract CILMethodBase MakeGenericIfNeeded( CILMethodBase method );

      #region CILElementInstantiable Members

      public Boolean IsTrueDefinition
      {
         get
         {
            return this.IsCapableOfChanging() == null;
         }
      }

      #endregion


      public CILElementWithinILCode ElementTypeKind
      {
         get
         {
            return CILElementWithinILCode.Method;
         }
      }

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


   internal class CILConstructorImpl : CILMethodBaseImpl, CILConstructor
   {

      internal CILConstructorImpl(
         CILReflectionContextImpl ctx,
         Int32 anID,
         System.Reflection.ConstructorInfo ctor
         )
         : base( ctx, anID, ctor )
      {

      }

      internal CILConstructorImpl(
         CILReflectionContextImpl ctx,
         Int32 anID,
         CILType declaringType,
         MethodAttributes attrs
         )
         : this(
            ctx,
            anID,
            new Lazy<ListProxy<CILCustomAttribute>>( () => ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>(), ctx.LazyThreadSafetyMode ),
            new SettableValueForEnums<CallingConventions>( CallingConventions.Standard ),
            new SettableValueForEnums<MethodAttributes>( attrs | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName ),
            () => declaringType,
            () => ctx.CollectionsFactory.NewListProxy<CILParameter>(),
            () => new MethodILImpl( declaringType.Module ),
            LazyFactory.NewWriteableLazy( () => MethodImplAttributes.IL, ctx.LazyThreadSafetyMode ),
            null,
            true
         )
      {

      }

      internal CILConstructorImpl(
         CILReflectionContextImpl ctx,
         Int32 anID,
         Lazy<ListProxy<CILCustomAttribute>> cAttrDataFunc,
         SettableValueForEnums<CallingConventions> aCallingConvention,
         SettableValueForEnums<MethodAttributes> aMethodAttributes,
         Func<CILType> declaringTypeFunc,
         Func<ListProxy<CILParameter>> parametersFunc,
         Func<MethodIL> methodIL,
         WriteableLazy<MethodImplAttributes> aMethodImplementationAttributes,
         Lazy<DictionaryWithRoles<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>> aSecurityInfo,
         Boolean resettablesAreSettable
         )
         : base( ctx, anID, true, cAttrDataFunc, aCallingConvention, aMethodAttributes, declaringTypeFunc, parametersFunc, methodIL, aMethodImplementationAttributes, aSecurityInfo, resettablesAreSettable )
      {

      }

      protected override String GetThisName()
      {
         return this.methodAttributes.Value.IsStatic() ? Miscellaneous.CLASS_CTOR_NAME : Miscellaneous.INSTANCE_CTOR_NAME;
      }

      #region CILElementOwnedByChangeableType<CILConstructor> Members

      public CILConstructor ChangeDeclaringType( params CILTypeBase[] args )
      {
         return (CILConstructor) this.ChangeDeclaringTypeUT( args );
      }

      #endregion

      protected override CILMethodBase ThisOrGDef()
      {
         return this;
      }

      protected override CILMethodBase MakeGenericIfNeeded( CILMethodBase method )
      {
         return method;
      }

      internal override string IsCapableOfChanging()
      {
         return ( (CommonFunctionality) this.declaringType.Value ).IsCapableOfChanging();
      }
   }

   internal class CILMethodImpl : CILMethodBaseImpl, CILMethod, CILElementWithGenericArgs, CILMethodInternal
   {
      private readonly SettableValueForClasses<String> name;
      private readonly Lazy<CILParameter> returnParameter;
      private readonly Lazy<ListProxy<CILTypeBase>> gArgs;
      private readonly WriteableLazy<CILMethod> gDef;
      private readonly SettableValueForEnums<PInvokeAttributes> pInvokeAttributes;
      private readonly SettableValueForClasses<String> pInvokeName;
      private readonly SettableValueForClasses<String> pInvokeModule;

      internal CILMethodImpl(
         CILReflectionContextImpl ctx,
         Int32 anID,
         System.Reflection.MethodInfo method
         )
         : base( ctx, anID, method )
      {
         var nGDef = method.IsGenericMethodDefinition ? method : null;

         // TODO SL support (via ctx events?
#if !CAM_LOGICAL_IS_SL
         var dllImportAttr = method.GetCustomAttributes( typeof( System.Runtime.InteropServices.DllImportAttribute ), true ).FirstOrDefault() as System.Runtime.InteropServices.DllImportAttribute;
#endif

         InitFields(
            ctx,
            ref this.name,
            ref this.returnParameter,
            ref this.gArgs,
            ref this.gDef,
            ref this.pInvokeAttributes,
            ref this.pInvokeName,
            ref this.pInvokeModule,
            new SettableValueForClasses<String>( method.Name ),
            () => ctx.Cache.GetOrAdd( method.ReturnParameter ),
            () => ctx.CollectionsFactory.NewListProxy<CILTypeBase>( method.GetGenericArguments().Select( gArg => ctx.Cache.GetOrAdd( gArg ) ).ToList() ),
            () => ctx.Cache.GetOrAdd( nGDef ),
            new SettableValueForEnums<PInvokeAttributes>(
#if CAM_LOGICAL_IS_SL
               (PInvokeAttributes) 0
#else
 dllImportAttr == null ? (PInvokeAttributes) 0 : dllImportAttr.GetCorrespondingPInvokeAttributes()
#endif
 ),
            new SettableValueForClasses<String>(
#if CAM_LOGICAL_IS_SL
               null
#else
 dllImportAttr == null ? null : dllImportAttr.EntryPoint
#endif
 ),
            new SettableValueForClasses<String>(
#if CAM_LOGICAL_IS_SL
               null
#else
 dllImportAttr == null ? null : dllImportAttr.Value
#endif
 ),
            true
            );
      }

      internal CILMethodImpl( CILReflectionContextImpl ctx, Int32 anID, CILType declaringType, String name, MethodAttributes attrs, CallingConventions callingConventions )
         : this(
            ctx,
            anID,
            new Lazy<ListProxy<CILCustomAttribute>>( () => ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>(), ctx.LazyThreadSafetyMode ),
            new SettableValueForEnums<CallingConventions>( callingConventions ),
            new SettableValueForEnums<MethodAttributes>( attrs ),
            () => declaringType,
            () => ctx.CollectionsFactory.NewListProxy<CILParameter>(),
            () => new MethodILImpl( declaringType.Module ),
            LazyFactory.NewWriteableLazy( () => MethodImplAttributes.IL, ctx.LazyThreadSafetyMode ),
            null,
            new SettableValueForClasses<String>( name ),
            () => ctx.Cache.NewBlankParameter( ctx.Cache.ResolveMethodBaseID( anID ), E_CILLogical.RETURN_PARAMETER_POSITION, null, ParameterAttributes.None, null ),
            () => ctx.CollectionsFactory.NewListProxy<CILTypeBase>(),
            () => null,
            null,
            null,
            null,
            true )
      {

      }

      internal CILMethodImpl(
         CILReflectionContextImpl ctx,
         Int32 anID,
         Lazy<ListProxy<CILCustomAttribute>> cAttrDataFunc,
         SettableValueForEnums<CallingConventions> aCallingConvention,
         SettableValueForEnums<MethodAttributes> aMethodAttributes,
         Func<CILType> declaringTypeFunc,
         Func<ListProxy<CILParameter>> parametersFunc,
         WriteableLazy<MethodImplAttributes> aMethodImplementationAttributes,
         Lazy<DictionaryWithRoles<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>> aLogicalSecurityInformation,
         SettableValueForClasses<String> aName,
         Func<CILParameter> returnParameterFunc,
         Func<ListProxy<CILTypeBase>> gArgsFunc,
         Func<CILMethod> gDefFunc,
         Boolean resettablesAreSettable = false
         )
         : this( ctx, anID, cAttrDataFunc, aCallingConvention, aMethodAttributes, declaringTypeFunc, parametersFunc, null, aMethodImplementationAttributes, aLogicalSecurityInformation, aName, returnParameterFunc, gArgsFunc, gDefFunc, null, null, null, resettablesAreSettable )
      {

      }

      internal CILMethodImpl(
         CILReflectionContextImpl ctx,
         Int32 anID,
         Lazy<ListProxy<CILCustomAttribute>> cAttrDataFunc,
         SettableValueForEnums<CallingConventions> aCallingConvention,
         SettableValueForEnums<MethodAttributes> aMethodAttributes,
         Func<CILType> declaringTypeFunc,
         Func<ListProxy<CILParameter>> parametersFunc,
         Func<MethodIL> methodIL,
         WriteableLazy<MethodImplAttributes> aMethodImplementationAttributes,
         Lazy<DictionaryWithRoles<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>> aLogicalSecurityInformation,
         SettableValueForClasses<String> aName,
         Func<CILParameter> returnParameterFunc,
         Func<ListProxy<CILTypeBase>> gArgsFunc,
         Func<CILMethod> gDefFunc,
         SettableValueForEnums<PInvokeAttributes> aPInvokeAttributes,
         SettableValueForClasses<String> aPInvokeName,
         SettableValueForClasses<String> aPInvokeModuleName,
         Boolean resettablesAreSettable
         )
         : base( ctx, anID, false, cAttrDataFunc, aCallingConvention, aMethodAttributes, declaringTypeFunc, parametersFunc, methodIL, aMethodImplementationAttributes, aLogicalSecurityInformation, resettablesAreSettable )
      {
         InitFields(
            ctx,
            ref this.name,
            ref this.returnParameter,
            ref this.gArgs,
            ref this.gDef,
            ref this.pInvokeAttributes,
            ref this.pInvokeName,
            ref this.pInvokeModule,
            aName,
            returnParameterFunc,
            gArgsFunc,
            gDefFunc,
            aPInvokeAttributes,
            aPInvokeName,
            aPInvokeModuleName,
            resettablesAreSettable
            );
      }

      private static void InitFields(
         CILReflectionContextImpl ctx,
         ref SettableValueForClasses<String> name,
         ref Lazy<CILParameter> returnParameter,
         ref Lazy<ListProxy<CILTypeBase>> gArgs,
         ref WriteableLazy<CILMethod> gDef,
         ref SettableValueForEnums<PInvokeAttributes> pInvokeAttributes,
         ref SettableValueForClasses<String> pInvokeName,
         ref SettableValueForClasses<String> pInvokeModule,
         SettableValueForClasses<String> aName,
         Func<CILParameter> returnParameterFunc,
         Func<ListProxy<CILTypeBase>> gArgsFunc,
         Func<CILMethod> gDefFunc,
         SettableValueForEnums<PInvokeAttributes> aPInvokeAttributes,
         SettableValueForClasses<String> aPInvokeName,
         SettableValueForClasses<String> aPInvokeModule,
         Boolean resettablesAreSettable
         )
      {
         var lazyThreadSafety = ctx.LazyThreadSafetyMode;
         name = aName;
         returnParameter = new Lazy<CILParameter>( returnParameterFunc, lazyThreadSafety );
         gArgs = new Lazy<ListProxy<CILTypeBase>>( gArgsFunc, lazyThreadSafety );
         gDef = LazyFactory.NewWriteableLazy<CILMethod>( gDefFunc, lazyThreadSafety );
         pInvokeAttributes = aPInvokeAttributes ?? new SettableValueForEnums<PInvokeAttributes>( (PInvokeAttributes) 0 );
         pInvokeName = aPInvokeName ?? new SettableValueForClasses<String>( null );
         pInvokeModule = aPInvokeModule ?? new SettableValueForClasses<String>( null );
      }

      public override String ToString()
      {
         return this.returnParameter.Value + base.ToString();
      }

      protected override String GetThisName()
      {
         var result = this.name.Value;
         if ( this.gArgs.Value.CQ.Any() )
         {
            result += "[" + String.Join( ", ", this.gArgs.Value.CQ ) + "]";
         }
         return result;
      }


      #region CILElementWithSimpleName Members

      public String Name
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

      #region CILElementWithGenericArguments<CILMethod> Members

      public CILMethod GenericDefinition
      {
         get
         {
            return this.gDef.Value;
         }
      }

      #endregion

      #region CILElementWithGenericArgs Members

      ListProxy<CILTypeBase> CILElementWithGenericArgs.InternalGenericArguments
      {
         get
         {
            return this.gArgs.Value;
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

      #region CILMethod Members

      public PInvokeAttributes PlatformInvokeAttributes
      {
         get
         {
            return this.pInvokeAttributes.Value;
         }
         set
         {
            this.pInvokeAttributes.Value = value;
         }
      }

      public String PlatformInvokeName
      {
         get
         {
            return this.pInvokeName.Value;
         }
         set
         {
            this.pInvokeName.Value = value;
         }
      }

      public String PlatformInvokeModuleName
      {
         get
         {
            return this.pInvokeModule.Value;
         }
         set
         {
            this.pInvokeModule.Value = value;
         }
      }

      public CILMethod MakeGenericMethod( params CILTypeBase[] args )
      {
         return this.context.Cache.MakeGenericMethod( this, this.GenericDefinition, args );
      }

      #endregion

      #region CILElementOwnedByChangeableType<CILMethod> Members

      public CILMethod ChangeDeclaringType( params CILTypeBase[] args )
      {
         return (CILMethod) this.ChangeDeclaringTypeUT( args );
      }

      #endregion

      protected override CILMethodBase ThisOrGDef()
      {
         return this.gArgs.Value.CQ.Any() ? this.gDef.Value : this;
      }

      protected override CILMethodBase MakeGenericIfNeeded( CILMethodBase method )
      {
         return this.gArgs.Value.CQ.Any() ? ( (CILMethod) method ).MakeGenericMethod( this.gArgs.Value.CQ.ToArray() ) : method;
      }

      internal override String IsCapableOfChanging()
      {
         if ( this.HasGenericArguments() && !this.IsGenericDefinition() )
         {
            return "This method is generic method instance";
         }
         else
         {
            return ( (CommonFunctionality) this.declaringType.Value ).IsCapableOfChanging();
         }
      }

      #region CILElementWithGenericArguments<CILMethod> Members


      public CILTypeParameter[] DefineGenericParameters( String[] names )
      {
         CILTypeParameter[] result;

         this.ThrowIfNotCapableOfChanging();
         LogicalUtils.CheckWhenDefiningGArgs( this.gArgs.Value, names );
         if ( names != null && names.Length > 0 )
         {
            result = Enumerable.Range( 0, names.Length ).Select( idx => this.context.Cache.NewBlankTypeParameter( this.declaringType.Value, this, names[idx], idx ) ).ToArray();
            this.gArgs.Value.AddRange( result );
            this.gDef.Value = this;
         }
         else
         {
            result = CILTypeImpl.EMPTY_TYPE_PARAMS;
         }


         return result;
      }

      #endregion

      //public override MethodAttributes Attributes
      //{
      //   set
      //   {
      //      base.Attributes = value;
      //      //LogicalUtils.CheckMethodAttributesForOverriddenMethods( this.methodAttributes, this.overriddenMethods.Value );
      //   }
      //}

      #region CILElementWithGenericArguments<CILMethod> Members


      public ListQuery<CILTypeBase> GenericArguments
      {
         get
         {
            return this.gArgs.Value.CQ;
         }
      }

      #endregion

      #region CILMethod Members


      public CILParameter ReturnParameter
      {
         get
         {
            return this.returnParameter.Value;
         }
      }

      #endregion
   }
}