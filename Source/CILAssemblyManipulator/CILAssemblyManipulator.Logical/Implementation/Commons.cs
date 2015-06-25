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
   internal abstract class CommonFunctionality
   {
      internal readonly CILReflectionContextImpl context;
      internal readonly Int32 id;

      internal protected CommonFunctionality( CILReflectionContextImpl aContext, Int32 anID )
      {
         ArgumentValidator.ValidateNotNull( "Context", aContext );

         this.context = aContext;
         this.id = anID;
      }

      internal abstract LazyWithLock<ListProxy<CILCustomAttribute>> CustomAttributeList
      {
         get;
      }

      internal abstract String IsCapableOfChanging();

      protected void ThrowIfNotCapableOfChanging()
      {
         if ( this.IsCapableOfChanging() != null )
         {
            throw new NotSupportedException( "Complex changing (such as adding interfaces, methods, etc) on this instance is not supported: " + this.IsCapableOfChanging() );
         }
      }
   }

   internal abstract class CILCustomAttributeContainerImpl : CommonFunctionality, CILCustomAttributeContainer, CILElementWithContext
   {
      protected internal readonly CILElementKind cilKind;
      private readonly LazyWithLock<ListProxy<CILCustomAttribute>> attributes;

      internal CILCustomAttributeContainerImpl(
         CILReflectionContextImpl ctx,
         Int32 anID,
         CILElementKind kind,
         Func<CustomAttributeDataEventArgs> evtArgsFunc
         )
         : base( ctx, anID )
      {
         ArgumentValidator.ValidateNotNull( "Reflection context", ctx );

         InitFields(
            ref this.cilKind,
            ref this.attributes,
            kind,
            new LazyWithLock<ListProxy<CILCustomAttribute>>( () =>
            {
               var evtArgs = evtArgsFunc();
               ctx.LaunchCustomAttributeDataLoadEvent( evtArgs );
               var thisElement = (CILCustomAttributeContainer) this.context.Cache.ResolveAnyID( this.cilKind, this.id );
               return ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>( new List<CILCustomAttribute>( evtArgs.CustomAttributeData.Select( tuple => CILCustomAttributeFactory.NewAttribute( thisElement, tuple.Item1, tuple.Item2, tuple.Item3 ) ) ) );
            } )
            );
      }

      internal CILCustomAttributeContainerImpl(
         CILReflectionContextImpl ctx,
         CILElementKind kind,
         Int32 anID,
         LazyWithLock<ListProxy<CILCustomAttribute>> cAttrDataFunc
         )
         : base( ctx, anID )
      {
         InitFields(
            ref this.cilKind,
            ref this.attributes,
            kind,
            cAttrDataFunc
            );
      }

      private static void InitFields(
         ref CILElementKind cilKind,
         ref LazyWithLock<ListProxy<CILCustomAttribute>> attributesField,
         CILElementKind kind,
         LazyWithLock<ListProxy<CILCustomAttribute>> attributes
         )
      {
         cilKind = kind;
         attributesField = attributes;
      }

      internal override LazyWithLock<ListProxy<CILCustomAttribute>> CustomAttributeList
      {
         get
         {
            return this.attributes;
         }
      }

      #region CILCustomAttributeContainer<TContainerMQ,TContainerIQ> Members

      public CILCustomAttribute AddCustomAttribute( CILConstructor ctor, IEnumerable<CILCustomAttributeTypedArgument> ctorArgs, IEnumerable<CILCustomAttributeNamedArgument> namedArgs )
      {
         lock ( this.attributes.Lock )
         {
            var result = CILCustomAttributeFactory.NewAttribute( this, ctor, ctorArgs, namedArgs );
            this.attributes.Value.Add( result );
            return result;
         }
      }

      public Boolean RemoveCustomAttribute( CILCustomAttribute attribute )
      {
         lock ( this.attributes.Lock )
         {
            return this.attributes.Value.Remove( attribute );
         }
      }

      public ListQuery<CILCustomAttribute> CustomAttributeData
      {
         get
         {
            return this.attributes.Value.CQ;
         }
      }

      #endregion

      #region CILElementWithContext Members

      public CILReflectionContext ReflectionContext
      {
         get
         {
            return this.context;
         }
      }

      #endregion

      protected DictionaryWithRoles<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>> SecurityInfoFromAttributes()
      {
         return this.context.CollectionsFactory.NewDictionary<SecurityAction, ListProxy<LogicalSecurityInformation>, ListProxyQuery<LogicalSecurityInformation>, ListQuery<LogicalSecurityInformation>>( this.CustomAttributeData
            .Where( ca =>
               ca.Constructor.DeclaringType.GetBaseTypeChain().Any( bt => String.Equals( Consts.SECURITY_ATTR, bt.GetFullName() ) && Object.Equals( LogicalUtils.NATIVE_MSCORLIB.NewWrapper( this.context ), bt.Module.Assembly ) )
               && ca.ConstructorArguments.Count > 0
               && String.Equals( ca.ConstructorArguments[0].ArgumentType.GetFullName(), Consts.SECURITY_ACTION ) )
            .GroupBy( ca => ca.ConstructorArguments[0].Value )
            .ToDictionary( cag => (SecurityAction) cag.Key, cag => this.context.CollectionsFactory.NewListProxy( cag.Select( ca => new LogicalSecurityInformation( (SecurityAction) cag.Key, ca.Constructor.DeclaringType, ca.NamedArguments ) ).ToList() ) ) );
      }
   }

   internal abstract class AbstractSignatureElement : CILElementWithContext
   {

      internal readonly CILReflectionContextImpl _ctx;

      internal AbstractSignatureElement( CILReflectionContext ctx )
      {
         ArgumentValidator.ValidateNotNull( "Context", ctx );

         this._ctx = (CILReflectionContextImpl) ctx;
      }

      #region CILElementWithContext Members

      public CILReflectionContext ReflectionContext
      {
         get
         {
            return this._ctx;
         }
      }

      #endregion
   }

   internal class SettableLazy<T>
   {

      private Lazy<T> _lazy;
      private Object _mutable;

      internal SettableLazy( Func<T> creator )
         : this( new Lazy<T>( creator, LazyThreadSafetyMode.ExecutionAndPublication ) )
      {

      }

      private SettableLazy( Lazy<T> lazy )
      {
         this._lazy = lazy;
      }

      internal T Value
      {
         get
         {
            var lazy = this._lazy; // Read field only once in case it changes after check and 2nd reading
            return lazy == null ? (T) this._mutable : lazy.Value;
         }
         set
         {
            Interlocked.Exchange( ref this._mutable, value );
            Interlocked.Exchange( ref this._lazy, null );
         }
      }
   }

   internal abstract class SettableValue<TField, TValue>
   {
      protected TField _value;

      internal SettableValue( TField value )
      {
         this._value = value;
      }

      internal abstract TValue Value
      {
         get;
         set;
      }

      public override String ToString()
      {
         return String.Format( "{0}", this._value );
      }
   }

   internal class SettableValueForClasses<T> : SettableValue<T, T>
      where T : class
   {
      internal SettableValueForClasses( T value )
         : base( value )
      {

      }

      internal override T Value
      {
         get
         {
            return this._value;
         }
         set
         {
            Interlocked.Exchange( ref this._value, value );
         }
      }
   }

   internal class SettableValueForInt32 : SettableValue<Int32, Int32>
   {
      internal SettableValueForInt32( Int32 value )
         : base( value )
      {

      }

      internal override int Value
      {
         get
         {
            return this._value;
         }
         set
         {
            Interlocked.Exchange( ref this._value, value );
         }
      }
   }

   internal class SettableValueForEnums<T> : SettableValue<Int32, T>
      where T : struct
   {
      internal SettableValueForEnums( T value )
         : base( (Int32) (Object) value )
      {

      }

      internal override T Value
      {
         get
         {
            return (T) (Object) this._value;
         }
         set
         {
            Interlocked.Exchange( ref this._value, (Int32) (Object) value );
         }
      }
   }

   internal class LazyWithLock<T>
   {
      private readonly Object _lock;
      protected Lazy<T> _lazy;

      internal LazyWithLock( Func<T> valueFactory )
      {
         this._lock = new Object();
         this._lazy = new Lazy<T>( valueFactory, LazyThreadSafetyMode.ExecutionAndPublication );
      }

      internal virtual T Value
      {
         get
         {
            return this._lazy.Value;
         }
         set
         {
            throw new NotSupportedException();
         }
      }

      internal Object Lock
      {
         get
         {
            return this._lock;
         }
      }
   }

   internal class ResettableLazy<T> : LazyWithLock<T>
   {
      private readonly Func<T> _valueFactory;

      internal ResettableLazy( Func<T> valueFactory )
         : base( valueFactory )
      {
         this._valueFactory = valueFactory;
      }

      internal virtual void Reset()
      {
         Interlocked.Exchange( ref this._lazy, new Lazy<T>( this._valueFactory, LazyThreadSafetyMode.ExecutionAndPublication ) );
      }
   }

   internal class ResettableAndSettableLazy<T> : ResettableLazy<T>
   {
      private Object _mutable;

      internal ResettableAndSettableLazy( Func<T> valueFactory )
         : base( valueFactory )
      {

      }

      internal override T Value
      {
         get
         {
            var lazy = this._lazy; // Read field only once in case it changes after check and 2nd reading
            return lazy == null ? (T) this._mutable : lazy.Value;
         }
         set
         {
            Interlocked.Exchange( ref this._mutable, value );
            Interlocked.Exchange( ref this._lazy, null );
         }
      }

      internal override void Reset()
      {
         base.Reset();
         Interlocked.Exchange( ref this._mutable, null );
      }
   }

   internal interface CILElementWithSimpleNameInternal
   {
      SettableValueForClasses<String> NameInternal { get; }
   }

   internal interface CILElementWithConstantValueInternal
   {
      SettableLazy<Object> ConstantValueInternal { get; }
   }

   internal interface CILElementWithCustomModifiersInternal
   {
      LazyWithLock<ListProxy<CILCustomModifier>> CustomModifierList { get; }
   }

   internal interface CILFieldInternal : CILElementWithSimpleNameInternal, CILElementWithConstantValueInternal, CILElementWithCustomModifiersInternal, CILElementWithMarshalInfoInternal
   {
      SettableValueForEnums<FieldAttributes> FieldAttributesInternal { get; }
      SettableValueForClasses<Byte[]> FieldRVAValue { get; }
      SettableLazy<Int32> FieldOffsetInternal { get; }

      void ResetFieldType();
   }

   internal interface CILElementWithMarshalInfoInternal
   {
      SettableLazy<LogicalMarshalingInfo> MarshalingInfoInternal { get; }
   }

   internal interface CILParameterInternal : CILElementWithSimpleNameInternal, CILElementWithConstantValueInternal, CILElementWithCustomModifiersInternal, CILElementWithMarshalInfoInternal
   {
      SettableValueForEnums<ParameterAttributes> ParameterAttributesInternal { get; }
      Int32 ParameterPositionInternal { get; }

      void ResetParameterType();
   }

   internal interface CILMethodBaseInternal
   {
      SettableValueForEnums<CallingConventions> CallingConventionInternal { get; }
      SettableValueForEnums<MethodAttributes> MethodAttributesInternal { get; }
      SettableLazy<MethodImplAttributes> MethodImplementationAttributesInternal { get; }

      void ResetParameterList();
   }

   internal interface CILMethodInternal : CILMethodBaseInternal, CILElementWithSimpleNameInternal
   {
   }

   internal interface CILTypeBaseInternal : CILElementWithSimpleNameInternal
   {
      SettableValueForClasses<String> NamespaceInternal { get; }
   }

   internal interface CILTypeOrGenericParamInternal : CILTypeBaseInternal
   {
      //void ResetTypeCode();
   }

   internal interface CILTypeInternal : CILTypeOrGenericParamInternal
   {
      SettableValueForEnums<TypeAttributes> TypeAttributesInternal { get; }
      LazyWithLock<ListProxy<CILType>> NestedTypesInternal { get; }
      SettableLazy<LogicalClassLayout?> ClassLayoutInternal { get; }
      //SettableLazy<CILType> ForwardedTypeInternal { get; }
      void ResetBaseType();
      void ResetDeclaredInterfaces();
      void ResetDeclaredMethods();
      void ResetDeclaredFields();
      void ResetConstructors();
      void ResetDeclaredProperties();
      void ResetDeclaredEvents();
   }

   internal interface CILPropertyInternal : CILElementWithSimpleNameInternal, CILElementWithConstantValueInternal, CILElementWithCustomModifiersInternal
   {
      SettableValueForEnums<PropertyAttributes> PropertyAttributesInternal { get; }

      void ResetGetMethod();
      void ResetSetMethod();
      //void ResetPropertyType();
   }

   internal interface CILEventInternal : CILElementWithSimpleNameInternal
   {
      SettableValueForEnums<EventAttributes> EventAttributesInternal { get; }

      void ResetEventHandlerType();
      void ResetAddMethod();
      void ResetRemoveMethod();
      void ResetRaiseMethod();
      void ResetOtherMethods();
   }

   internal interface CILElementWithGenericArgs
   {
      ListProxy<CILTypeBase> InternalGenericArguments { get; }
   }

   internal enum CILElementKind : byte
   {
      Assembly,
      Module,
      Type,
      Field,
      Constructor,
      Method,
      Parameter,
      Property,
      Event
   }
}