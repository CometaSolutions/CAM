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

      internal abstract Lazy<ListProxy<CILCustomAttribute>> CustomAttributeList
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

      protected WriteableResettableLazy<T> ThrowIfNotCapableOfChanging<T>( IResettableLazy<T> lazy )
      {
         var retVal = lazy as WriteableResettableLazy<T>;
         if ( retVal == null )
         {
            throw new NotSupportedException( "Complex changing (such as adding interfaces, methods, etc) on this instance is not supported: " + this.IsCapableOfChanging() );
         }
         return retVal;
      }
   }

   internal abstract class CILCustomAttributeContainerImpl : CommonFunctionality, CILCustomAttributeContainer, CILElementWithContext
   {
      protected internal readonly CILElementKind cilKind;
      private readonly Lazy<ListProxy<CILCustomAttribute>> attributes;

      internal CILCustomAttributeContainerImpl(
         CILReflectionContextImpl ctx,
         Int32 anID,
         CILElementKind kind,
         Func<CILReflectionContextWrapperCallbacks, IEnumerable<Object>> evtArgsFunc
         )
         : base( ctx, anID )
      {
         ArgumentValidator.ValidateNotNull( "Reflection context", ctx );

         InitFields(
            ref this.cilKind,
            ref this.attributes,
            kind,
            new Lazy<ListProxy<CILCustomAttribute>>( () => ctx.CollectionsFactory.NewListProxy<CILCustomAttribute>( evtArgsFunc( ctx.WrapperCallbacks ).Select( caData => ctx.WrapperCallbacks.GetCILCustomAttributeFromNativeOrThrow( this, caData ) ).ToList() ), ctx.LazyThreadSafetyMode )
            );
      }

      internal CILCustomAttributeContainerImpl(
         CILReflectionContextImpl ctx,
         CILElementKind kind,
         Int32 anID,
         Lazy<ListProxy<CILCustomAttribute>> cAttrDataFunc
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
         ref Lazy<ListProxy<CILCustomAttribute>> attributesField,
         CILElementKind kind,
         Lazy<ListProxy<CILCustomAttribute>> attributes
         )
      {
         cilKind = kind;
         attributesField = attributes;
      }

      internal override Lazy<ListProxy<CILCustomAttribute>> CustomAttributeList
      {
         get
         {
            return this.attributes;
         }
      }

      #region CILCustomAttributeContainer<TContainerMQ,TContainerIQ> Members

      public CILCustomAttribute AddCustomAttribute( CILConstructor ctor, IEnumerable<CILCustomAttributeTypedArgument> ctorArgs, IEnumerable<CILCustomAttributeNamedArgument> namedArgs )
      {
         var result = CILCustomAttributeFactory.NewAttribute( this, ctor, ctorArgs, namedArgs );
         this.attributes.Value.Add( result );
         return result;
      }

      public Boolean RemoveCustomAttribute( CILCustomAttribute attribute )
      {
         return this.attributes.Value.Remove( attribute );
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
               ca.Constructor.DeclaringType.GetBaseTypeChain().Any( bt => String.Equals( Consts.SECURITY_ATTR, bt.GetFullName() ) && Object.Equals( this.context.NewWrapper( LogicalUtils.NATIVE_MSCORLIB ), bt.Module.Assembly ) )
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


   internal interface CILElementWithSimpleNameInternal
   {
      SettableValueForClasses<String> NameInternal { get; }
   }

   internal interface CILElementWithConstantValueInternal
   {
      WriteableLazy<Object> ConstantValueInternal { get; }
   }

   internal interface CILElementWithCustomModifiersInternal
   {
      Lazy<ListProxy<CILCustomModifier>> CustomModifierList { get; }
   }

   internal interface CILFieldInternal : CILElementWithSimpleNameInternal, CILElementWithConstantValueInternal, CILElementWithCustomModifiersInternal, CILElementWithMarshalInfoInternal
   {
      SettableValueForEnums<FieldAttributes> FieldAttributesInternal { get; }
      SettableValueForClasses<Byte[]> FieldRVAValue { get; }
      WriteableLazy<Int32> FieldOffsetInternal { get; }

      void ResetFieldType();
   }

   internal interface CILElementWithMarshalInfoInternal
   {
      WriteableLazy<LogicalMarshalingInfo> MarshalingInfoInternal { get; }
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
      WriteableLazy<MethodImplAttributes> MethodImplementationAttributesInternal { get; }

      void ResetParameterList();
   }

   internal interface CILMethodInternal : CILMethodBaseInternal, CILElementWithSimpleNameInternal
   {
      WriteableLazy<PlatformInvokeInfo> PlatformInvokeInfoInternal { get; }
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
      Lazy<ListProxy<CILType>> NestedTypesInternal { get; }
      WriteableLazy<LogicalClassLayout?> ClassLayoutInternal { get; }
      //SettableLazy<CILType> ForwardedTypeInternal { get; }
      void ResetBaseType();
      void ResetDeclaredInterfaces();
      void ResetDeclaredMethods();
      void ResetDeclaredFields();
      void ResetConstructors();
      void ResetDeclaredProperties();
      void ResetDeclaredEvents();
      void ResetExplicitMethods();
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