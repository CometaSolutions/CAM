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
using System.Linq;
using System.Threading;
using CollectionsWithRoles.API;
using CommonUtils;
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Logical.Implementation
{
   internal class CILEventImpl : CILCustomAttributeContainerImpl, CILEvent, CILEventInternal
   {
      private readonly SettableValueForClasses<String> name;
      private readonly SettableValueForEnums<EventAttributes> eventAttributes;
      private readonly IResettableLazy<CILTypeBase> eventType;
      private readonly IResettableLazy<CILMethod> addMethod;
      private readonly IResettableLazy<CILMethod> removeMethod;
      private readonly IResettableLazy<CILMethod> raiseMethod;
      private readonly ReadOnlyResettableLazy<ListProxy<CILMethod>> otherMethods;
      private readonly Lazy<CILType> declaringType;

      internal CILEventImpl(
         CILReflectionContextImpl ctx,
         Int32 anID,
         System.Reflection.EventInfo evt )
         : base( ctx, anID, CILElementKind.Event, cb => cb.GetCustomAttributesDataForOrThrow( evt ) )
      {
         ArgumentValidator.ValidateNotNull( "Event", evt );

         if ( evt.DeclaringType
#if WINDOWS_PHONE_APP
            .GetTypeInfo()
#endif
.IsGenericType && !evt.DeclaringType
#if WINDOWS_PHONE_APP
            .GetTypeInfo()
#endif
.IsGenericTypeDefinition )
         {
            throw new ArgumentException( "This constructor may be used only on events declared in genericless types or generic type definitions." );
         }
         InitFields(
            ctx,
            ref this.name,
            ref this.eventAttributes,
            ref this.eventType,
            ref this.addMethod,
            ref this.removeMethod,
            ref this.raiseMethod,
            ref this.otherMethods,
            ref this.declaringType,
            new SettableValueForClasses<String>( evt.Name ),
            new SettableValueForEnums<EventAttributes>( (EventAttributes) evt.Attributes ),
            () => ctx.Cache.GetOrAdd( evt.EventHandlerType ),
            () => ctx.Cache.GetOrAdd( evt.GetAddMethod( true ) ),
            () => ctx.Cache.GetOrAdd( evt.GetRemoveMethod( true ) ),
            () => ctx.Cache.GetOrAdd( evt.GetRaiseMethod( true ) ),
            () => ctx.CollectionsFactory.NewListProxy<CILMethod>( ctx.WrapperCallbacks.GetEventOtherMethodsOrThrow( evt ).Select( m => ctx.NewWrapper( m ) ).ToList() ),
            () => (CILType) ctx.Cache.GetOrAdd( evt.DeclaringType ),
            true
            );
      }

      internal CILEventImpl( CILReflectionContextImpl ctx, Int32 anID, CILType declaringType, String aName, EventAttributes anEventAttributes, CILTypeBase anEventType )
         : this(
         ctx,
         anID,
         new Lazy<ListProxy<CILCustomAttribute>>( () => ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>(), ctx.LazyThreadSafetyMode ),
         new SettableValueForClasses<String>( aName ),
         new SettableValueForEnums<EventAttributes>( anEventAttributes ),
         () => anEventType,
         () => null,
         () => null,
         () => null,
         () => ctx.CollectionsFactory.NewListProxy<CILMethod>(),
         () => declaringType,
         true )
      {

      }

      internal CILEventImpl(
         CILReflectionContextImpl ctx,
         Int32 anID,
         Lazy<ListProxy<CILCustomAttribute>> cAttrDataFunc,
         SettableValueForClasses<String> aName,
         SettableValueForEnums<EventAttributes> anEventAttributes,
         Func<CILTypeBase> eventTypeFunc,
         Func<CILMethod> addMethodFunc,
         Func<CILMethod> removeMethodFunc,
         Func<CILMethod> raiseMethodFunc,
         Func<ListProxy<CILMethod>> otherMethodsFunc,
         Func<CILType> declaringTypeFunc,
         Boolean resettablesAreSettable = false
         )
         : base( ctx, CILElementKind.Event, anID, cAttrDataFunc )
      {
         InitFields(
            ctx,
            ref this.name,
            ref this.eventAttributes,
            ref this.eventType,
            ref this.addMethod,
            ref this.removeMethod,
            ref this.raiseMethod,
            ref this.otherMethods,
            ref this.declaringType,
            aName,
            anEventAttributes,
            eventTypeFunc,
            addMethodFunc,
            removeMethodFunc,
            raiseMethodFunc,
            otherMethodsFunc,
            declaringTypeFunc,
            resettablesAreSettable
            );
      }

      private static void InitFields(
         CILReflectionContextImpl ctx,
         ref SettableValueForClasses<String> name,
         ref SettableValueForEnums<EventAttributes> eventAttributes,
         ref IResettableLazy<CILTypeBase> eventType,
         ref IResettableLazy<CILMethod> addMethod,
         ref IResettableLazy<CILMethod> removeMethod,
         ref IResettableLazy<CILMethod> raiseMethod,
         ref ReadOnlyResettableLazy<ListProxy<CILMethod>> otherMethods,
         ref Lazy<CILType> declaringType,
         SettableValueForClasses<String> aName,
         SettableValueForEnums<EventAttributes> anEventAttributes,
         Func<CILTypeBase> eventTypeFunc,
         Func<CILMethod> addMethodFunc,
         Func<CILMethod> removeMethodFunc,
         Func<CILMethod> raiseMethodFunc,
         Func<ListProxy<CILMethod>> otherMethodsFunc,
         Func<CILType> declaringTypeFunc,
         Boolean resettablesSettable
      )
      {
         var lazyThreadSafety = ctx.LazyThreadSafetyMode;
         name = aName;
         eventAttributes = anEventAttributes;
         eventType = LazyFactory.NewResettableLazy( resettablesSettable, eventTypeFunc, lazyThreadSafety );
         addMethod = LazyFactory.NewResettableLazy( resettablesSettable, addMethodFunc, lazyThreadSafety );
         removeMethod = LazyFactory.NewResettableLazy( resettablesSettable, removeMethodFunc, lazyThreadSafety );
         raiseMethod = LazyFactory.NewResettableLazy( resettablesSettable, raiseMethodFunc, lazyThreadSafety );
         otherMethods = LazyFactory.NewReadOnlyResettableLazy( otherMethodsFunc, lazyThreadSafety );
         declaringType = new Lazy<CILType>( declaringTypeFunc, lazyThreadSafety );
      }

      #region CILEvent Members

      public CILTypeBase EventHandlerType
      {
         set
         {
            var lazy = this.ThrowIfNotCapableOfChanging( this.eventType );
            LogicalUtils.CheckTypeForMethodSig( this.declaringType.Value.Module, ref value );
            lazy.Value = value;
            this.context.Cache.ForAllGenericInstancesOf( this, evt => evt.ResetEventHandlerType() );
         }
         get
         {
            return this.eventType.Value;
         }
      }

      public CILMethod AddMethod
      {
         set
         {
            var lazy = this.ThrowIfNotCapableOfChanging( this.addMethod );
            value.ThrowIfNotTrueDefinition();
            lazy.Value = value;
            this.context.Cache.ForAllGenericInstancesOf( this, evt => evt.ResetAddMethod() );
         }
         get
         {
            return this.addMethod.Value;
         }
      }

      public CILMethod RemoveMethod
      {
         set
         {
            var lazy = this.ThrowIfNotCapableOfChanging( this.removeMethod );
            value.ThrowIfNotTrueDefinition();
            lazy.Value = value;
            this.context.Cache.ForAllGenericInstancesOf( this, evt => evt.ResetRemoveMethod() );
         }
         get
         {
            return this.removeMethod.Value;
         }
      }

      public CILMethod RaiseMethod
      {
         set
         {
            var lazy = this.ThrowIfNotCapableOfChanging( this.raiseMethod );
            value.ThrowIfNotTrueDefinition();
            lazy.Value = value;
            this.context.Cache.ForAllGenericInstancesOf( this, evt => evt.ResetRaiseMethod() );
         }
         get
         {
            return this.raiseMethod.Value;
         }
      }

      public void AddOtherMethods( params CILMethod[] methods )
      {
         this.ThrowIfNotCapableOfChanging();
         foreach ( var method in methods )
         {
            method.ThrowIfNotTrueDefinition();
         }
         this.otherMethods.Value.AddRange( methods );
         this.context.Cache.ForAllGenericInstancesOf( this, evt => evt.ResetOtherMethods() );
      }

      public Boolean RemoveOtherMethods( params CILMethod[] methods )
      {
         this.ThrowIfNotCapableOfChanging();
         var result = false;
         foreach ( var method in methods )
         {
            result = this.otherMethods.Value.Remove( method ) || result;
         }
         if ( result )
         {
            this.context.Cache.ForAllGenericInstancesOf( this, evt => evt.ResetOtherMethods() );
         }
         return result;
      }

      public ListQuery<CILMethod> OtherMethods
      {
         get
         {
            return this.otherMethods.Value.CQ;
         }
      }

      #endregion

      #region CILElementWithAttributes<EventAttributes> Members

      public EventAttributes Attributes
      {
         set
         {
            this.eventAttributes.Value = value;
         }
         get
         {
            return this.eventAttributes.Value;
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

      #region CILElementOwnedByChangeableType<CILEvent> Members

      public CILEvent ChangeDeclaringType( params CILTypeBase[] args )
      {
         LogicalUtils.ThrowIfDeclaringTypeNotGeneric( this, args );
         CILEvent evtToGive = this;
         CILType dt = this.declaringType.Value;
         if ( dt.GenericDefinition != null )
         {
            evtToGive = dt.GenericDefinition.DeclaredEvents[dt.DeclaredEvents.IndexOf( this )];
         }
         return this.context.Cache.MakeEventWithGenericType( evtToGive, args );
      }

      #endregion

      internal override String IsCapableOfChanging()
      {
         return ( (CommonFunctionality) this.declaringType.Value ).IsCapableOfChanging();
      }

      #region CILElementInstantiable Members

      public Boolean IsTrueDefinition
      {
         get
         {
            return this.IsCapableOfChanging() == null;
         }
      }

      #endregion

      SettableValueForEnums<EventAttributes> CILEventInternal.EventAttributesInternal
      {
         get
         {
            return this.eventAttributes;
         }
      }

      void CILEventInternal.ResetEventHandlerType()
      {
         this.eventType.Reset();
      }

      void CILEventInternal.ResetAddMethod()
      {
         this.addMethod.Reset();
      }

      void CILEventInternal.ResetRemoveMethod()
      {
         this.removeMethod.Reset();
      }

      void CILEventInternal.ResetRaiseMethod()
      {
         this.raiseMethod.Reset();
      }

      void CILEventInternal.ResetOtherMethods()
      {
         this.otherMethods.Reset();
      }

      SettableValueForClasses<String> CILElementWithSimpleNameInternal.NameInternal
      {
         get
         {
            return this.name;
         }
      }

   }
}