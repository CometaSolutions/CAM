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
using System.IO;
using CILAssemblyManipulator.Logical.Implementation;
using CollectionsWithRoles.API;
using CommonUtils;
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Logical
{
   /// <summary>
   /// This class represents the isolated 'universe' in which any reading, loading, or emitting operations are done. It fully supports concurrent access.
   /// </summary>
   /// <remarks>Typical usecase scenario would be this:
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CILManipulatorCodeContent.cs" region="CILReflectionContextCode1" language="C#" />
   /// </remarks>
   /// <seealso cref="CILReflectionContextFactory.NewContext"/>
   public interface CILReflectionContext : IDisposable
   {

      /// <summary>
      /// Gets the concurrency mode for this <see cref="CILReflectionContext"/>.
      /// </summary>
      /// <seealso cref="CILReflectionContextConcurrencySupport"/>
      CILReflectionContextConcurrencySupport ConcurrencySupport { get; }

      /// <summary>
      /// This event occurs when defined types are loaded in <see cref="CILModule"/> wrapping a native <see cref="System.Reflection.Module"/> by accessing the <see cref="CILModule.DefinedTypes"/> property. The event handler should set <see cref="ModuleTypesEventArgs.DefinedTypes"/> property. This assembly can not directly access the GetTypes() method in <see cref="System.Reflection.Module"/> as the method is not present in this portable profile.
      /// </summary>
      event EventHandler<ModuleTypesEventArgs> ModuleTypesLoadEvent;

      /// <summary>
      /// This event occurs when owner module is loaded in <see cref="CILTypeBase"/> wrapping a native <see cref="System.Type"/> by accessing the <see cref="CILTypeBase.Module"/> property. The event handler should set the <see cref="TypeModuleEventArgs.Module"/> property. This assembly can not directly access the Module property in <see cref="System.Type"/> as the method is not present in this portable profile.
      /// </summary>
      event EventHandler<TypeModuleEventArgs> TypeModuleLoadEvent;

      /// <summary>
      /// This event occurs when custom attribute data is <see cref="CILCustomAttributeContainer"/> wrapping a native reflection element by accessing the <see cref="CILCustomAttributeContainer.CustomAttributeData"/> property. The event handler should set the <see cref="CustomAttributeDataEventArgs.CustomAttributeData"/> property to have same data as in System.Reflection.CustomAttributeData of the native reflection elements present in <see cref="CustomAttributeDataEventArgs"/>. This assembly can not directly access the GetCustomAttributesData() methods of native reflection elements as the methods are not present in this portable profile.
      /// </summary>
      event EventHandler<CustomAttributeDataEventArgs> CustomAttributeDataLoadEvent;

      /// <summary>
      /// This event occurs when other methods are loaded in <see cref="CILEvent"/> wrapping a native <see cref="System.Reflection.EventInfo"/> by accessing the <see cref="CILEvent.OtherMethods"/> property. The event handler should set the <see cref="EventOtherMethodsEventArgs.OtherMethods"/> property. This assembly can not directly access the GetOtherMethods() method in <see cref="System.Reflection.EventInfo"/> as the method is not present in this portable profile.
      /// </summary>
      event EventHandler<EventOtherMethodsEventArgs> EventOtherMethodsLoadEvent;

      /// <summary>
      /// This event occurs when a constant value is loaded in <see cref="CILElementWithConstant"/> wrapping a native <see cref="System.Reflection.PropertyInfo"/>, <see cref="System.Reflection.FieldInfo"/> or <see cref="System.Reflection.ParameterInfo"/>. The event handler should set the <see cref="ConstantValueLoadArgs.ConstantValue"/> property. This assembly can not directly access the GetRawConstantValue() methods in <see cref="System.Reflection.PropertyInfo"/> and <see cref="System.Reflection.FieldInfo"/> and RawDefaultValue property in <see cref="System.Reflection.ParameterInfo"/> as the property and the methods are not present in this portable profile.
      /// </summary>
      event EventHandler<ConstantValueLoadArgs> ConstantValueLoadEvent;

      /// <summary>
      /// This event occurs when an explicitly implemented methods are loaded in <see cref="CILMethod"/> wrapping a native <see cref="System.Reflection.MethodInfo"/> by accessing the <see cref="CILMethod.OverriddenMethods"/> property. The event handler should set the <see cref="ExplicitMethodImplementationLoadArgs.ExplicitlyImplementedMethods"/> property. This assembly can not directly access the GetInterfaceMap() in <see cref="System.Type"/> as the method is not present in this portable profile.
      /// </summary>
      event EventHandler<ExplicitMethodImplementationLoadArgs> ExplicitMethodImplementationLoadEvent;

      /// <summary>
      /// This event occurs when a method body is loaded in <see cref="CILMethodBase"/> wrapping a native <see cref="System.Reflection.MethodBase"/> by accessing the <see cref="CILMethodBase.MethodIL"/> property. The event handler should set the <see cref="MethodBodyLoadArgs.Locals"/>, <see cref="MethodBodyLoadArgs.InitLocals"/>, <see cref="MethodBodyLoadArgs.ExceptionInfos"/> and <see cref="MethodBodyLoadArgs.IL"/> properties. This assembly can not directly access the GetMethodBody() method in <see cref="System.Reflection.MethodBase"/> as the method is not present in this portable profile.
      /// </summary>
      event EventHandler<MethodBodyLoadArgs> MethodBodyLoadEvent;

      /// <summary>
      /// This event occurs when a method body is loaded in <see cref="CILMethodBase"/> wrapping a native <see cref="System.Reflection.MethodBase"/> by accessing the <see cref="CILMethodBase.MethodIL"/> property. The event handler should set the <see cref="TokenResolveArgs.ResolvedMember"/> or <see cref="TokenResolveArgs.ResolvedString"/> property, depending on value of the <see cref="TokenResolveArgs.ResolveKind"/> property. This assembly can not directly access the ResolveMember(Int32, Type[], Type[]) and ResolveString() methods in <see cref="System.Reflection.Module"/> as the methods are not present in this portable profile.
      /// </summary>
      event EventHandler<TokenResolveArgs> TokenResolveEvent;

      /// <summary>
      /// This event occurs when a method implementation attributes are loaded in <see cref="CILMethodBase"/> wrapping a native <see cref="System.Reflection.MethodBase"/> by accessing the <see cref="CILMethodBase.ImplementationAttributes"/> property. The event handler should set the <see cref="MethodImplAttributesEventArgs.MethodImplementationAttributes"/> property. This assembly can not directly access the MethodImplementationFlags property in <see cref="System.Reflection.MethodBase"/> as the property is not present in this portable profile.
      /// </summary>
      event EventHandler<MethodImplAttributesEventArgs> MethodImplementationAttributesLoadEvent;

      /// <summary>
      /// This event occurs when a type layout is loaded in <see cref="CILType"/> wrapping a native <see cref="System.Type"/> by accessing the <see cref="CILType.Layout"/> property. The event handler should set the <see cref="TypeLayoutEventArgs.Layout"/> property. This assembly can not directly access the StructLayoutAttribute property in <see cref="System.Type"/> as the property is not present in this portable profile.
      /// </summary>
      event EventHandler<TypeLayoutEventArgs> TypeLayoutLoadEvent;

      /// <summary>
      /// This event occurs when a assembly full name is loaded in <see cref="CILAssembly"/> wrapping a native <see cref="System.Reflection.Assembly"/> by accessing the <see cref="CILAssembly.Name"/> property. The event handler should set the <see cref="AssemblyNameEventArgs.AssemblyNameInfo"/> property. This assembly can not directly access the GetName() method in <see cref="System.Reflection.Assembly"/> as the method is not present in this portable profile.
      /// </summary>
      event EventHandler<AssemblyNameEventArgs> AssemblyNameLoadEvent;

      /// <summary>
      /// This event occurs when custom modifiers are loaded in <see cref="CILElementWithCustomModifiers"/> wrapping a native <see cref="System.Reflection.MemberInfo"/> or <see cref="System.Reflection.ParameterInfo"/> by accessing the <see cref="CILElementWithCustomModifiersReadOnly.CustomModifiers"/> property. The event handler should set the <see cref="CustomModifierEventLoadArgs.OptionalModifiers"/> and <see cref="CustomModifierEventLoadArgs.RequiredModifiers"/> properties. This assembly can not directly access the GetOptionalCustomModifiers() and GetRequiredCustomModifiers() methods in <see cref="System.Reflection.FieldInfo"/>, <see cref="System.Reflection.PropertyInfo"/> and <see cref="System.Reflection.ParameterInfo"/> as the methods are not present in this portable profile.
      /// </summary>
      event EventHandler<CustomModifierEventLoadArgs> CustomModifierLoadEvent;

      CryptoCallbacks DefaultCryptoCallbacks { get; }

      IEqualityComparer<CILAssemblyName> DefaultAssemblyNameComparer { get; }

      /// <summary>
      /// When a <see cref="CILAssembly"/> or <see cref="CILModule"/> is loaded via <see cref="E_CILLogical.LoadAssembly(CILReflectionContext, System.IO.Stream, EmittingArguments)"/> or <see cref="E_CILLogical.LoadModule(CILReflectionContext,System.IO.Stream, EmittingArguments)"/> methods, respectively, any access causing additional load of assemblies is triggered via this event, if the assembly loader function of the aforementioned methods is <c>null</c> or fails to load the assembly. The event handler should set the <see cref="AssemblyRefResolveFromLoadedAssemblyEventArgs.ResolvedAssembly"/> property.
      /// </summary>
      event EventHandler<AssemblyRefResolveFromLoadedAssemblyEventArgs> AssemblyReferenceResolveFromLoadedAssemblyEvent;


      /// <summary>
      /// Gets the interfaces implemented by a single-dimensional arrays.
      /// </summary>
      /// <value>The interfaces implemented by a single-dimensional arrays.</value>
      /// <seealso cref="CILReflectionContextFactory.NewContext"/>
      ListQuery<Type> VectorArrayInterfaces { get; }

      /// <summary>
      /// Gets the interfaces implemented by a multi-dimensional arrays.
      /// </summary>
      /// <value>The interfaces implemented by a multi-dimensional arrays.</value>
      /// <seealso cref="CILReflectionContextFactory.NewContext"/>
      ListQuery<Type> MultiDimensionalArrayInterfaces { get; }
   }

   /// <summary>
   /// This enumeration describes the concurrency support level of <see cref="CILReflectionContext"/> and all reflection elements that belong to the context.
   /// </summary>
   /// <remarks>
   /// The <see cref="CILReflectionContext"/> is deemed to be <c>thread-safe</c>, when it is possible to create element types or make generic types, or create wrappers of native <see cref="N:System.Reflection"/> types concurrently.
   /// But even then, concurrently adding/remove types to the module, or adding/removing methods/fields/etc to same type is not supported.
   /// However, one can safely e.g. add/remove nested types to different enclosing types, or add/remove methods/fields/etc to different type concurrently.
   /// When the <see cref="CILReflectionContext.ConcurrencySupport"/> is <see cref="NotThreadSafe"/>, the <see cref="CILReflectionContext"/> should not be used concurrently at all (including creating element types, making generic types, or reading/modifying any reflection elements).
   /// </remarks>
   public enum CILReflectionContextConcurrencySupport
   {
      /// <summary>
      /// The <see cref="CILReflectionContext"/> is not safe to use concurrently at all.
      /// </summary>
      NotThreadSafe,

      /// <summary>
      /// The <see cref="CILReflectionContext"/> is safe to use concurrently as described in remarks of <see cref="CILReflectionContextConcurrencySupport"/>, and the implementation uses types in <see cref="N:System.Collections.Concurrent"/> and <see cref="N:System.Threading"/> namespaces.
      /// This provides the most performant yet concurrent solution, but is not supported on all platforms.
      /// </summary>
      ThreadSafe_WithConcurrentCollections,

      /// <summary>
      /// The <see cref="CILReflectionContext"/> is safe to use concurrently as described in remarks of <see cref="CILReflectionContextConcurrencySupport"/>, and the implementation uses types in <see cref="N:System.Collections.Generic"/> namespace, and simple locks.
      /// As a result, this is significantly slower than <see cref="ThreadSafe_WithConcurrentCollections"/>, but is supported on more platforms.
      /// </summary>
      ThreadSafe_Simple
   }
   /// <summary>
   /// This is factory class for creating new instances of <see cref="CILReflectionContext"/>.
   /// </summary>
   public static class CILReflectionContextFactory
   {
      /// <summary>
      /// Creates a new <see cref="CILReflectionContext"/>. This context is needed for most operations of interfaces in this namespace.
      /// </summary>
      /// <param name="concurrencyMode">The concurrency mode for the new <see cref="CILReflectionContext"/>.</param>
      /// <param name="vectorArrayInterfaces">All interfaces implemented by single-dimensional array types, besides the ones implemented by <see cref="System.Array"/>. Value <c>null</c> is interpreted as an empty array.</param>
      /// <param name="multiDimArrayIFaces">All interfaces implemented by multi-dimensional array types, besdies the ones implemented by <see cref="System.Array"/>. Value <c>null</c> is interpreted as an empty array.</param>
      /// <param name="defaultCryptoCallbacks">The default callbacks for cryptographic functions. May be <c>null</c> if no default callbacks for cryptographic functions are provided.</param>
      /// <returns>A new instance of <see cref="CILReflectionContext"/>.</returns>
      public static CILReflectionContext NewContext( CILReflectionContextConcurrencySupport concurrencyMode, Type[] vectorArrayInterfaces, Type[] multiDimArrayIFaces, CryptoCallbacks defaultCryptoCallbacks )
      {
         return new CILReflectionContextImpl( concurrencyMode, vectorArrayInterfaces, multiDimArrayIFaces, defaultCryptoCallbacks );
      }
   }

   /// <summary>
   /// The event argument class used by <see cref="CILReflectionContext.ModuleTypesLoadEvent"/> event.
   /// </summary>
   public sealed class ModuleTypesEventArgs : EventArgs
   {
      private readonly System.Reflection.Module _mod;
      private IEnumerable<Type> _result;

      internal ModuleTypesEventArgs( System.Reflection.Module mod )
      {
         this._mod = mod;
      }

      /// <summary>
      /// Gets the native <see cref="System.Reflection.Module"/> from which to fetch all defined types.
      /// </summary>
      /// <value>The native <see cref="System.Reflection.Module"/> from which to fetch all defined types.</value>
      public System.Reflection.Module Module
      {
         get
         {
            return this._mod;
         }
      }

      /// <summary>
      /// Gets or sets all the defined types in <see cref="Module"/>.
      /// </summary>
      /// <value>All the defined types in <see cref="Module"/>.</value>
      /// <remarks>The event handlers should set this property.</remarks>
      public IEnumerable<Type> DefinedTypes
      {
         get
         {
            return this._result;
         }
         set
         {
            this._result = value;
         }
      }
   }

   /// <summary>
   /// The event argument class used by <see cref="CILReflectionContext.TypeModuleLoadEvent"/> event.
   /// </summary>
   public sealed class TypeModuleEventArgs : EventArgs
   {
      private readonly Type _type;
      private System.Reflection.Module _module;

      internal TypeModuleEventArgs( Type type )
      {
         this._type = type;
      }

      /// <summary>
      /// Gets the native <see cref="System.Type"/> from which to fetch the module it is defined in.
      /// </summary>
      /// <value>The native <see cref="System.Type"/> from which to fetch the module it is defined in.</value>
      public Type Type
      {
         get
         {
            return this._type;
         }
      }

      /// <summary>
      /// Gets or sets the module which defines <see cref="Type"/>.
      /// </summary>
      /// <value>The module which defines <see cref="Type"/>.</value>
      /// <remarks>The event handlers should set this property.</remarks>
      public System.Reflection.Module Module
      {
         get
         {
            return this._module;
         }
         set
         {
            this._module = value;
         }
      }
   }

   /// <summary>
   /// The event argument class used by <see cref="CILReflectionContext.EventOtherMethodsLoadEvent"/> event.
   /// </summary>
   public sealed class EventOtherMethodsEventArgs : EventArgs
   {
      private readonly System.Reflection.EventInfo _evt;
      private System.Reflection.MethodInfo[] _otherMethods;

      internal EventOtherMethodsEventArgs( System.Reflection.EventInfo evt )
      {
         this._evt = evt;
      }

      /// <summary>
      /// Gets the native <see cref="System.Reflection.EventInfo"/> from which to fetch other methods.
      /// </summary>
      /// <value>The native <see cref="System.Reflection.EventInfo"/> from which to fetch other methods.</value>
      public System.Reflection.EventInfo Event
      {
         get
         {
            return this._evt;
         }
      }

      /// <summary>
      /// Gets or sets the other methods of the <see cref="Event"/>.
      /// </summary>
      /// <value>The other methods of the <see cref="Event"/>.</value>
      /// <remarks>The event handlers should set this property.</remarks>
      public System.Reflection.MethodInfo[] OtherMethods
      {
         get
         {
            return this._otherMethods;
         }
         set
         {
            this._otherMethods = value;
         }
      }
   }

   /// <summary>
   /// The event argument class used by <see cref="CILReflectionContext.CustomAttributeDataLoadEvent"/> event.
   /// </summary>
   public sealed class CustomAttributeDataEventArgs : EventArgs
   {
      private readonly Type _type;
      private readonly System.Reflection.MemberInfo _mInfo;
      private readonly System.Reflection.ParameterInfo _pInfo;
      private readonly System.Reflection.Assembly _ass;
      private readonly System.Reflection.Module _mod;

      private readonly CILReflectionContext _context;

      internal CustomAttributeDataEventArgs( CILReflectionContext ctx, Type type )
         : this( ctx, type, null, null, null, null )
      {

      }

      internal CustomAttributeDataEventArgs( CILReflectionContext ctx, System.Reflection.MemberInfo mInfo )
         : this( ctx, null, mInfo, null, null, null )
      {
      }

      internal CustomAttributeDataEventArgs( CILReflectionContext ctx, System.Reflection.ParameterInfo pInfo )
         : this( ctx, null, null, pInfo, null, null )
      {
      }

      internal CustomAttributeDataEventArgs( CILReflectionContext ctx, System.Reflection.Assembly ass )
         : this( ctx, null, null, null, ass, null )
      {
      }

      internal CustomAttributeDataEventArgs( CILReflectionContext ctx, System.Reflection.Module mod )
         : this( ctx, null, null, null, null, mod )
      {
      }

      private CustomAttributeDataEventArgs( CILReflectionContext ctx, Type type, System.Reflection.MemberInfo mInfo, System.Reflection.ParameterInfo pInfo, System.Reflection.Assembly ass, System.Reflection.Module mod )
      {
         this._context = ctx;
         this._type = type;
         this._mInfo = mInfo;
         this._pInfo = pInfo;
         this._ass = ass;
         this._mod = mod;
      }

      /// <summary>
      /// Gets the native <see cref="System.Reflection.MemberInfo"/>, from which to get custom attribute data. May be <c>null</c>.
      /// </summary>
      /// <value>The native <see cref="System.Reflection.MemberInfo"/>, from which to get custom attribute data. May be <c>null</c>.</value>
      /// <remarks>One of the <see cref="Member"/>, <see cref="Type"/>, <see cref="Parameter"/>, <see cref="Assembly"/> and <see cref="Module"/> properties is always non-<c>null</c>.</remarks>
      public System.Reflection.MemberInfo Member
      {
         get
         {
            return this._mInfo;
         }
      }

      /// <summary>
      /// Gets the native <see cref="Type"/>, from which to get custom attribute data. May be <c>null</c>.
      /// </summary>
      /// <value>The native <see cref="Type"/>, from which to get custom attribute data. May be <c>null</c>.</value>
      /// <remarks>One of the <see cref="Member"/>, <see cref="Type"/>, <see cref="Parameter"/>, <see cref="Assembly"/> and <see cref="Module"/> properties is always non-<c>null</c>.</remarks>
      public Type Type
      {
         get
         {
            return this._type;
         }
      }

      /// <summary>
      /// Gets the native <see cref="System.Reflection.ParameterInfo"/>, from which to get custom attribute data. May be <c>null</c>.
      /// </summary>
      /// <value>The native <see cref="System.Reflection.ParameterInfo"/>, from which to get custom attribute data. May be <c>null</c>.</value>
      /// <remarks>One of the <see cref="Member"/>, <see cref="Type"/>, <see cref="Parameter"/>, <see cref="Assembly"/> and <see cref="Module"/> properties is always non-<c>null</c>.</remarks>
      public System.Reflection.ParameterInfo Parameter
      {
         get
         {
            return this._pInfo;
         }
      }

      /// <summary>
      /// Gets the native <see cref="System.Reflection.Assembly"/>, from which to get custom attribute data. May be <c>null</c>.
      /// </summary>
      /// <value>The native <see cref="System.Reflection.Assembly"/>, from which to get custom attribute data. May be <c>null</c>.</value>
      /// <remarks>One of the <see cref="Member"/>, <see cref="Type"/>, <see cref="Parameter"/>, <see cref="Assembly"/> and <see cref="Module"/> properties is always non-<c>null</c>.</remarks>
      public System.Reflection.Assembly Assembly
      {
         get
         {
            return this._ass;
         }
      }

      /// <summary>
      /// Gets the native <see cref="System.Reflection.Module"/>, from which to get custom attribute data. May be <c>null</c>.
      /// </summary>
      /// <value>The native <see cref="System.Reflection.Module"/>, from which to get custom attribute data. May be <c>null</c>.</value>
      /// <remarks>One of the <see cref="Member"/>, <see cref="Type"/>, <see cref="Parameter"/>, <see cref="Assembly"/> and <see cref="Module"/> properties is always non-<c>null</c>.</remarks>
      public System.Reflection.Module Module
      {
         get
         {
            return this._mod;
         }
      }

      /// <summary>
      /// Gets the current reflection context.
      /// </summary>
      /// <value>The current reflection context.</value>
      public CILReflectionContext Context
      {
         get
         {
            return this._context;
         }
      }

      /// <summary>
      /// Gets or sets the custom attribute data. Each element of the enumerable should have the custom attribute constructor as the first item of the tuple, all constructor arguments as the second item of the tuple, and all named custom attribute arguments as the third item of the tuple.
      /// </summary>
      /// <value>The custom attribute data of the <see cref="Member"/>, <see cref="Parameter"/>, <see cref="Assembly"/> or <see cref="Module"/> properties.</value>
      /// <remarks>The event handlers should set this property.</remarks>
      public IEnumerable<Tuple<CILConstructor, IEnumerable<CILCustomAttributeTypedArgument>, IEnumerable<CILCustomAttributeNamedArgument>>> CustomAttributeData { get; set; }
   }

   /// <summary>
   /// The event argument class used by <see cref="CILReflectionContext.ConstantValueLoadEvent"/> event.
   /// </summary>
   public sealed class ConstantValueLoadArgs : EventArgs
   {
      private readonly System.Reflection.PropertyInfo _property;
      private readonly System.Reflection.FieldInfo _field;
      private readonly System.Reflection.ParameterInfo _parameter;
      private Object _constantValue;

      internal ConstantValueLoadArgs( System.Reflection.PropertyInfo property )
         : this( property, null, null )
      {

      }

      internal ConstantValueLoadArgs( System.Reflection.FieldInfo field )
         : this( null, field, null )
      {

      }

      internal ConstantValueLoadArgs( System.Reflection.ParameterInfo param )
         : this( null, null, param )
      {

      }

      private ConstantValueLoadArgs( System.Reflection.PropertyInfo property, System.Reflection.FieldInfo field, System.Reflection.ParameterInfo param )
      {
         this._property = property;
         this._field = field;
         this._parameter = param;
         this._constantValue = null;
      }

      /// <summary>
      /// Gets the native <see cref="System.Reflection.PropertyInfo"/>, from which to get constant value. May be <c>null</c>.
      /// </summary>
      /// <value>The native <see cref="System.Reflection.PropertyInfo"/>, from which to get constant value. May be <c>null</c>.</value>
      /// <remarks>One of the <see cref="Property"/>, <see cref="Field"/> and <see cref="Parameter"/> properties is always non-<c>null</c>.</remarks>
      public System.Reflection.PropertyInfo Property
      {
         get
         {
            return this._property;
         }
      }

      /// <summary>
      /// Gets the native <see cref="System.Reflection.FieldInfo"/>, from which to get constant value. May be <c>null</c>.
      /// </summary>
      /// <value>The native <see cref="System.Reflection.FieldInfo"/>, from which to get constant value. May be <c>null</c>.</value>
      /// <remarks>One of the <see cref="Property"/>, <see cref="Field"/> and <see cref="Parameter"/> properties is always non-<c>null</c>.</remarks>
      public System.Reflection.FieldInfo Field
      {
         get
         {
            return this._field;
         }
      }

      /// <summary>
      /// Gets the native <see cref="System.Reflection.ParameterInfo"/>, from which to get constant value. May be <c>null</c>.
      /// </summary>
      /// <value>The native <see cref="System.Reflection.ParameterInfo"/>, from which to get constant value. May be <c>null</c>.</value>
      /// <remarks>One of the <see cref="Property"/>, <see cref="Field"/> and <see cref="Parameter"/> properties is always non-<c>null</c>.</remarks>
      public System.Reflection.ParameterInfo Parameter
      {
         get
         {
            return this._parameter;
         }
      }

      /// <summary>
      /// Gets or sets the constant value of the <see cref="Property"/>, <see cref="Field"/> or <see cref="Parameter"/> property.
      /// </summary>
      /// <value>The constant value of the <see cref="Property"/>, <see cref="Field"/> or <see cref="Parameter"/> property.</value>
      /// <remarks>The event handlers should set this property.</remarks>
      public Object ConstantValue
      {
         get
         {
            return this._constantValue;
         }
         set
         {
            this._constantValue = value;
         }
      }
   }

   /// <summary>
   /// The event argument class used by <see cref="CILReflectionContext.ExplicitMethodImplementationLoadEvent"/> event.
   /// </summary>
   public sealed class ExplicitMethodImplementationLoadArgs : EventArgs
   {
      private readonly Type _type;
      private IDictionary<System.Reflection.MethodInfo, System.Reflection.MethodInfo[]> _implementedMethods;

      internal ExplicitMethodImplementationLoadArgs( Type type )
      {
         this._type = type;
      }

      /// <summary>
      /// Gets the native <see cref="System.Type"/> from which to get interface mapping.
      /// </summary>
      /// <value>The native <see cref="System.Type"/> from which to get interface mapping.</value>
      public Type Type
      {
         get
         {
            return this._type;
         }
      }

      /// <summary>
      /// Gets or sets the explicitly implemented methods for each of the method belonging to <see cref="Type"/>.
      /// </summary>
      /// <value>The explicitly implemented methods for each of the method belonging to <see cref="Type"/>.</value>
      /// <remarks>The event handlers should set this property. The key should be the method belonging to <see cref="Type"/>, and the mapped value should be all the methods explicitly implemented by that method.</remarks>
      public IDictionary<System.Reflection.MethodInfo, System.Reflection.MethodInfo[]> ExplicitlyImplementedMethods
      {
         get
         {
            return this._implementedMethods;
         }
         set
         {
            this._implementedMethods = value;
         }
      }
   }

   /// <summary>
   /// The event argument class used by <see cref="CILReflectionContext.MethodBodyLoadEvent"/> event.
   /// </summary>
   public sealed class MethodBodyLoadArgs : EventArgs
   {
      private readonly System.Reflection.MethodBase _method;
      private IList<Tuple<Boolean, Type>> _locals;
      private Boolean _initLocals;
      private IList<Tuple<ExceptionBlockType, Int32, Int32, Int32, Int32, Type, Int32>> _exceptionInfos;
      private Byte[] _il;

      internal MethodBodyLoadArgs( System.Reflection.MethodBase method )
      {
         this._method = method;
      }

      /// <summary>
      /// Gets the native <see cref="System.Reflection.MethodBase"/> from which to load method body.
      /// </summary>
      /// <value>The native <see cref="System.Reflection.MethodBase"/> from which to load method body.</value>
      public System.Reflection.MethodBase Method
      {
         get
         {
            return this._method;
         }
      }

      /// <summary>
      /// Gets or sets the local variables of the <see cref="Method"/>.
      /// </summary>
      /// <value>The local variables of the <see cref="Method"/>.</value>
      /// <remarks>The event handlers should set this property. Each element should tell whether local is pinned as first item of the tuple, and the type of the local as second item of the tuple.</remarks>
      public IList<Tuple<Boolean, Type>> Locals
      {
         get
         {
            return this._locals;
         }
         set
         {
            this._locals = value;
         }
      }

      /// <summary>
      /// Gets or sets whether locals are initialized to their default value at the beginning of the <see cref="Method"/>. See ECMA specification for more information about this.
      /// </summary>
      /// <value>Whether locals are initialized to their default value at the beginning of the <see cref="Method"/>.</value>
      /// <remarks>The event handlers should set this property.</remarks>
      public Boolean InitLocals
      {
         get
         {
            return this._initLocals;
         }
         set
         {
            this._initLocals = value;
         }
      }

      /// <summary>
      /// Gets or sets the information about all exception blocks in the <see cref="Method"/>
      /// </summary>
      /// <value>The information about all exception blocks in the <see cref="Method"/></value>
      /// <remarks>The event handlers should set this property. Each element of the list should have <see cref="ExceptionBlockType"/> as first item of the tuple, try start offset in bytes as second item of the tuple, try length in bytes as third item of the tuple, handler offset in bytes as fourth item of the tuple, handler length in bytes as fifth item of the tuple, catch type for exception catcher or <c>null</c> if not applicable as sixth item of the tuple, and filter offset in bytes or <c>-1</c> if not applicable as seventh item of the tuple.</remarks>
      public IList<Tuple<ExceptionBlockType, Int32, Int32, Int32, Int32, Type, Int32>> ExceptionInfos
      {
         get
         {
            return this._exceptionInfos;
         }
         set
         {
            this._exceptionInfos = value;
         }
      }

      /// <summary>
      /// Gets or sets the IL bytecode of the <see cref="Method"/>.
      /// </summary>
      /// <value>The IL bytecode of the <see cref="Method"/>.</value>
      /// <remarks>The event handlers should set this property.</remarks>
      public Byte[] IL
      {
         get
         {
            return this._il;
         }
         set
         {
            this._il = value;
         }
      }
   }

   /// <summary>
   /// The event argument class used by <see cref="CILReflectionContext.TokenResolveEvent"/> event.
   /// </summary>
   public sealed class TokenResolveArgs : EventArgs
   {
      /// <summary>
      /// This enum specifies what kind of token is needed to resolve.
      /// </summary>
      public enum ResolveKinds
      {
         /// <summary>
         /// The token will resolve to a reflection element.
         /// </summary>
         Member,
         /// <summary>
         /// The token will resolve to a string.
         /// </summary>
         String,

         /// <summary>
         /// The token will resolve to a signature in StandAloneSignature table.
         /// </summary>
         Signature
      }

      private readonly System.Reflection.Module _module;
      private readonly Int32 _token;
      private readonly ResolveKinds _rKind;
      private readonly Type[] _gArgs;
      private readonly Type[] _mGArgs;

      internal TokenResolveArgs( System.Reflection.Module module, Int32 token, ResolveKinds rKind )
         : this( module, token, rKind, null, null )
      {
      }

      internal TokenResolveArgs( System.Reflection.Module module, Int32 token, ResolveKinds rKind, Type[] gArgs, Type[] mgArgs )
      {
         this._module = module;
         this._token = token;
         this._rKind = rKind;
         this._gArgs = gArgs;
         this._mGArgs = mgArgs;
      }

      /// <summary>
      /// Gets the native <see cref="System.Reflection.Module"/> to use to resolve token.
      /// </summary>
      /// <value>The native <see cref="System.Reflection.Module"/> to use to resolve token.</value>
      public System.Reflection.Module Module
      {
         get
         {
            return this._module;
         }
      }

      /// <summary>
      /// Gets the token to resolve.
      /// </summary>
      /// <value>The token to resolve.</value>
      public Int32 Token
      {
         get
         {
            return this._token;
         }
      }

      /// <summary>
      /// Gets the information about whether token represents a string or a reflection element.
      /// </summary>
      /// <value>The information about whether token represents a string or a reflection element.</value>
      /// <seealso cref="ResolveKinds"/>
      public ResolveKinds ResolveKind
      {
         get
         {
            return this._rKind;
         }
      }

      /// <summary>
      /// Gets the generic type arguments of the resolving context.
      /// </summary>
      /// <value>The generic type arguments of the resolving context.</value>
      public Type[] TypeGenericArguments
      {
         get
         {
            return this._gArgs;
         }
      }

      /// <summary>
      /// Gets the generic method type arguments of the resolving context.
      /// </summary>
      /// <value>The generic method type arguments of the resolving context.</value>
      public Type[] MethodGenericArguments
      {
         get
         {
            return this._mGArgs;
         }
      }

      /// <summary>
      /// Gets or sets the resolved <see cref="System.Reflection.MemberInfo"/> (or <see cref="Type"/> ) from <see cref="Token"/> if <see cref="ResolveKind"/> returns <see cref="ResolveKinds.Member"/>.
      /// </summary>
      /// <value>The resolved <see cref="System.Reflection.MemberInfo"/> (or <see cref="Type"/> ) from <see cref="Token"/> if <see cref="ResolveKind"/> returns <see cref="ResolveKinds.Member"/>.</value>
      /// <remarks>Event handlers should set this property.</remarks>
      public Object ResolvedMember { get; set; }

      /// <summary>
      /// Gets or sets the resolved <see cref="String"/> from <see cref="Token"/> if <see cref="ResolveKind"/> returns <see cref="ResolveKinds.String"/>.
      /// </summary>
      /// <value>The resolved <see cref="String"/> from <see cref="Token"/> if <see cref="ResolveKind"/> returns <see cref="ResolveKinds.String"/>.</value>
      /// <remarks>Event handlers should set this property.</remarks>
      public String ResolvedString { get; set; }

      /// <summary>
      /// Gets or sets the resolved signature from <see cref="Token"/> if <see cref="ResolveKind"/> returns <see cref="ResolveKinds.Signature"/>.
      /// </summary>
      /// <value>The resolved signature from <see cref="Token"/> if <see cref="ResolveKind"/> returns <see cref="ResolveKinds.Signature"/>.</value>
      /// <remarks>Event handlers should set this property.</remarks>
      public Byte[] ResolvedSignature { get; set; }
   }

   /// <summary>
   /// The event argument class used by <see cref="CILReflectionContext.MethodImplementationAttributesLoadEvent"/> event.
   /// </summary>
   public sealed class MethodImplAttributesEventArgs : EventArgs
   {
      private readonly System.Reflection.MethodBase _method;
      internal MethodImplAttributesEventArgs( System.Reflection.MethodBase method )
      {
         this._method = method;
      }

      /// <summary>
      /// Gets the native <see cref="System.Reflection.MethodBase"/> from which the method implementation attributes should be fetched.
      /// </summary>
      /// <value>The native <see cref="System.Reflection.MethodBase"/> from which the method implementation attributes should be fetched.</value>
      public System.Reflection.MethodBase Method
      {
         get
         {
            return this._method;
         }
      }

      /// <summary>
      /// Gets or sets the method implementation attributes of the <see cref="Method"/>.
      /// </summary>
      /// <value>The method implementation attributes of the <see cref="Method"/>.</value>
      /// <remarks>Event handlers should set this property.</remarks>
      public MethodImplAttributes MethodImplementationAttributes { get; set; }
   }

   /// <summary>
   /// The event argument class used by <see cref="CILReflectionContext.TypeLayoutLoadEvent"/> event.
   /// </summary>
   public sealed class TypeLayoutEventArgs : EventArgs
   {
      private readonly Type _type;

      internal TypeLayoutEventArgs( Type type )
      {
         this._type = type;
      }

      /// <summary>
      /// Gets the native <see cref="System.Type"/> from which the layout information should be extracted.
      /// </summary>
      /// <value>The native <see cref="System.Type"/> from which the layout information should be extracted.</value>
      public Type Type
      {
         get
         {
            return this._type;
         }
      }

      /// <summary>
      /// Gets or sets the layout information of the <see cref="Type"/>.
      /// </summary>
      /// <value>The layout information of the <see cref="Type"/>.</value>
      /// <remarks>The event handlers should set this property.</remarks>
      public System.Runtime.InteropServices.StructLayoutAttribute Layout { get; set; }
   }

   /// <summary>
   /// The event argument class used by <see cref="CILReflectionContext.AssemblyNameLoadEvent"/> event.
   /// </summary>
   public sealed class AssemblyNameEventArgs : EventArgs
   {
      private readonly System.Reflection.Assembly _ass;

      internal AssemblyNameEventArgs( System.Reflection.Assembly ass )
      {
         this._ass = ass;
      }

      /// <summary>
      /// Gets the native <see cref="System.Reflection.Assembly"/> from which to extract assembly name information.
      /// </summary>
      /// <value>The native <see cref="System.Reflection.Assembly"/> from which to extract assembly name information.</value>
      public System.Reflection.Assembly Assembly
      {
         get
         {
            return this._ass;
         }
      }

      /// <summary>
      /// Gets or sets the assembly name information for the <see cref="Assembly"/>.
      /// </summary>
      /// <value>The assembly name information for the <see cref="Assembly"/>.</value>
      /// <remarks>Event handlers should set this property. The first item of the tuple should be <see cref="AssemblyHashAlgorithm"/>, the second item of the tuple should be <see cref="AssemblyFlags"/>, and the third item of the tuple should be the full public key.</remarks>
      public Tuple<AssemblyHashAlgorithm, AssemblyFlags, Byte[]> AssemblyNameInfo { get; set; }
   }

   ///// <summary>
   ///// The event argument class used by <see cref="CILReflectionContext.HashStreamLoadEvent"/> event.
   ///// </summary>
   //public sealed class HashStreamLoadEventArgs : EventArgs
   //{
   //   private readonly AssemblyHashAlgorithm _algorithm;

   //   internal HashStreamLoadEventArgs( AssemblyHashAlgorithm algo )
   //   {
   //      this._algorithm = algo;
   //   }

   //   /// <summary>
   //   /// Gets the hash algorithm for the hash stream.
   //   /// </summary>
   //   /// <value>The hash algorithm for the hash stream.</value>
   //   public AssemblyHashAlgorithm Algorithm
   //   {
   //      get
   //      {
   //         return this._algorithm;
   //      }
   //   }

   //   /// <summary>
   //   /// Gets or sets the resulting hash stream creator callback.
   //   /// </summary>
   //   /// <value>The resulting hash stream creator callback.</value>
   //   /// <remarks>
   //   /// Event handlers should set this property.
   //   /// This should be set to the callback creating cryptographic stream.
   //   /// Typically the callback will just invoke the <see cref="M:System.Security.Cryptography.CryptoStream#ctor(System.IO.Stream, System.Security.Cryptography.ICryptoTransform, System.Security.Cryptography.CryptoStreamMode)"/> constructor, and pass <see cref="Stream.Null"/> as first parameter, the resulting <see cref="Transform"/> as second parameter, and <see cref="F:System.Security.Cryptography.CryptoStreamMode.Write"/> as third parameter.
   //   /// </remarks>
   //   public Func<Stream> CryptoStream { set; get; }

   //   /// <summary>
   //   /// Gets or sets the callback to get hash from the transform.
   //   /// </summary>
   //   /// <value>The callback to get hash from the transform.</value>
   //   /// <remarks>
   //   /// Event handlers should set this property.
   //   /// This callback will be used to get the hash after the copying file contents to crypto stream.
   //   /// Typically the callback will just cast the parameter to <see cref="T:System.Security.Cryptography.HashAlgorithm"/> and return its <see cref="P:System.Security.Cryptography.HashAlgorithm.Hash"/> property.
   //   /// </remarks>
   //   public Func<Byte[]> HashGetter { get; set; }

   //   /// <summary>
   //   /// Gets or sets the callback to compute hash from byte array using the transform.
   //   /// </summary>
   //   /// <value>The callback to compute hash from byte array.</value>
   //   /// <remarks>
   //   /// Event handlers should set this property.
   //   /// This callback will be used to compute public key tokens.
   //   /// </remarks>
   //   public Func<Byte[], Byte[]> ComputeHash { get; set; }

   //   /// <summary>
   //   /// Gets or sets the cryptographic transform object.
   //   /// </summary>
   //   /// <value>The cryptographic transform object.</value>
   //   /// <remarks>
   //   /// Event handlers should set this propery.
   //   /// Once the transform is not needed, the <see cref="IDisposable.Dispose"/> method will be called for it.
   //   /// </remarks>
   //   public IDisposable Transform { get; set; }
   //}

   ///// <summary>
   ///// This is identical to System.Security.Cryptography.RSAParameters struct.
   ///// </summary>
   //public struct RSAParameters
   //{

   //   /// <summary>
   //   /// Represents the <c>D</c> parameter for the RSA algorithm.
   //   /// </summary>
   //   public Byte[] D;

   //   /// <summary>
   //   /// Represents the <c>DP</c> parameter for the RSA algorithm.
   //   /// </summary>
   //   public Byte[] DP;

   //   /// <summary>
   //   /// Represents the <c>DQ</c> parameter for the RSA algorithm.
   //   /// </summary>
   //   public Byte[] DQ;

   //   /// <summary>
   //   /// Represents the <c>Exponent</c> parameter for the RSA algorithm.
   //   /// </summary>
   //   public Byte[] Exponent;

   //   /// <summary>
   //   /// Represents the <c>InverseQ</c> parameter for the RSA algorithm.
   //   /// </summary>
   //   public Byte[] InverseQ;

   //   /// <summary>
   //   /// Represents the <c>Modulus</c> parameter for the RSA algorithm.
   //   /// </summary>
   //   public Byte[] Modulus;

   //   /// <summary>
   //   /// Represents the <c>P</c> parameter for the RSA algorithm.
   //   /// </summary>
   //   public Byte[] P;

   //   /// <summary>
   //   /// Represents the <c>Q</c> parameter for the RSA algorithm.
   //   /// </summary>
   //   public Byte[] Q;
   //}

   ///// <summary>
   ///// The event argument class used by <see cref="CILReflectionContext.RSACreationEvent"/> event.
   ///// </summary>
   //public sealed class RSACreationEventArgs : EventArgs
   //{
   //   private readonly String _keyPairContainer;
   //   private readonly RSAParameters? _rsaParams;

   //   internal RSACreationEventArgs( String containerName )
   //      : this( containerName, null )
   //   {
   //   }

   //   internal RSACreationEventArgs( RSAParameters rsaParams )
   //      : this( null, rsaParams )
   //   {
   //   }

   //   private RSACreationEventArgs( String containerName, RSAParameters? rsaParams )
   //   {
   //      this._keyPairContainer = containerName;
   //      this._rsaParams = rsaParams;
   //   }

   //   /// <summary>
   //   /// Gets the key-pair container name to use when creating <see cref="RSA"/>. May be <c>null</c> if no named key-pair container should be used.
   //   /// </summary>
   //   /// <value>Key-pair container name to use when creating <see cref="RSA"/>. May be <c>null</c> if no named key-pair container should be used.</value>
   //   /// <remarks>One of the properties <see cref="KeyPairContainer"/> and <see cref="RSAParameters"/> is always non-<c>null</c>.</remarks>
   //   public String KeyPairContainer
   //   {
   //      get
   //      {
   //         return this._keyPairContainer;
   //      }
   //   }

   //   /// <summary>
   //   /// Gets the RSA parameters for the resulting <see cref="RSA"/>. May be <c>null</c> if named container should be used for creating <see cref="RSA"/>.
   //   /// </summary>
   //   /// <value>The RSA parameters for the resulting <see cref="RSA"/>. May be <c>null</c> if named container should be used for creating <see cref="RSA"/>.</value>
   //   public RSAParameters? RSAParameters
   //   {
   //      get
   //      {
   //         return this._rsaParams;
   //      }
   //   }

   //   /// <summary>
   //   /// Gets or sets the RSA algorithm to use based on <see cref="KeyPairContainer"/> or <see cref="RSAParameters"/> property.
   //   /// </summary>
   //   /// <value>The RSA algorithm to use based on <see cref="KeyPairContainer"/> or <see cref="RSAParameters"/> property.</value>
   //   /// <remarks>Event handlers should set this property.</remarks>
   //   public IDisposable RSA { get; set; }
   //}

   ///// <summary>
   ///// The event argument class used by <see cref="CILReflectionContext.RSASignatureCreationEvent"/> event.
   ///// </summary>
   //public sealed class RSASignatureCreationEventArgs : EventArgs
   //{
   //   private readonly IDisposable _rsa;
   //   private readonly String _hashAlgorithm;
   //   private readonly Byte[] _contentsHash;

   //   internal RSASignatureCreationEventArgs( IDisposable rsa, AssemblyHashAlgorithm algorithm, Byte[] contentsHash )
   //   {
   //      this._rsa = rsa;
   //      this._hashAlgorithm = algorithm.GetAlgorithmName();
   //      this._contentsHash = contentsHash;
   //   }

   //   /// <summary>
   //   /// Gets the RSA algorithm to use.
   //   /// </summary>
   //   /// <value>The RSA algorithm to use.</value>
   //   public IDisposable RSA
   //   {
   //      get
   //      {
   //         return this._rsa;
   //      }
   //   }

   //   /// <summary>
   //   /// Gets the hash algorithm to use.
   //   /// </summary>
   //   /// <value>The hash algorithm to use.</value>
   //   public String HashAlgorithm
   //   {
   //      get
   //      {
   //         return this._hashAlgorithm;
   //      }
   //   }

   //   /// <summary>
   //   /// Gets the hash of the file contents.
   //   /// </summary>
   //   /// <value>The hash of the file contents.</value>
   //   public Byte[] ContentsHash
   //   {
   //      get
   //      {
   //         return this._contentsHash;
   //      }
   //   }

   //   /// <summary>
   //   /// Gets or sets the signature calculated using <see cref="RSA"/>, with the hash algorithm <see cref="HashAlgorithm"/> and given <see cref="ContentsHash"/>.
   //   /// </summary>
   //   /// <value>The signature calculated using <see cref="RSA"/>, with the hash algorithm <see cref="HashAlgorithm"/> and given <see cref="ContentsHash"/>.</value>
   //   /// <remarks>Event handlers should set this property.</remarks>
   //   public Byte[] Signature { get; set; }
   //}

   /// <summary>
   /// The event argument class used by <see cref="CILReflectionContext.CustomModifierLoadEvent"/> event.
   /// </summary>
   public sealed class CustomModifierEventLoadArgs : EventArgs
   {
      private readonly System.Reflection.MemberInfo _memberInfo;
      private readonly System.Reflection.ParameterInfo _paramInfo;

      internal CustomModifierEventLoadArgs( System.Reflection.FieldInfo field )
         : this( field, null )
      { }

      internal CustomModifierEventLoadArgs( System.Reflection.ParameterInfo param )
         : this( null, param )
      { }

      internal CustomModifierEventLoadArgs( System.Reflection.PropertyInfo prop )
         : this( prop, null )
      { }

      private CustomModifierEventLoadArgs( System.Reflection.MemberInfo memberInfo, System.Reflection.ParameterInfo param )
      {
         this._memberInfo = memberInfo;
         this._paramInfo = param;
      }

      /// <summary>
      /// Gets the <see cref="System.Reflection.MemberInfo"/> from which to get custom modifiers. May be <c>null</c>.
      /// </summary>
      /// <value>Gets the <see cref="System.Reflection.MemberInfo"/> from which to get custom modifiers. May be <c>null</c>.</value>
      /// <remarks>One of the properties <see cref="MemberInfo"/> and <see cref="ParameterInfo"/> will always be non-<c>null</c>.</remarks>
      public System.Reflection.MemberInfo MemberInfo
      {
         get
         {
            return this._memberInfo;
         }
      }

      /// <summary>
      /// Gets the <see cref="System.Reflection.ParameterInfo"/> from which to get custom modifiers. May be <c>null</c>.
      /// </summary>
      /// <value>Gets the <see cref="System.Reflection.ParameterInfo"/> from which to get custom modifiers. May be <c>null</c>.</value>
      /// <remarks>One of the properties <see cref="MemberInfo"/> and <see cref="ParameterInfo"/> will always be non-<c>null</c>.</remarks>
      public System.Reflection.ParameterInfo ParameterInfo
      {
         get
         {
            return this._paramInfo;
         }
      }

      /// <summary>
      /// Gets or sets the optional custom modifiers of the <see cref="MemberInfo"/> or <see cref="ParameterInfo"/> properties.
      /// </summary>
      /// <value>The optional custom modifiers of the <see cref="MemberInfo"/> or <see cref="ParameterInfo"/> properties.</value>
      /// <remarks>Event handlers should set this property.</remarks>
      public Type[] OptionalModifiers { get; set; }

      /// <summary>
      /// Gets or sets the required custom modifiers of the <see cref="MemberInfo"/> or <see cref="ParameterInfo"/> properties.
      /// </summary>
      /// <value>The required custom modifiers of the <see cref="MemberInfo"/> or <see cref="ParameterInfo"/> properties.</value>
      /// <remarks>Event handlers should set this property.</remarks>
      public Type[] RequiredModifiers { get; set; }
   }

   /// <summary>
   /// The event argument class used by <see cref="CILReflectionContext.AssemblyReferenceResolveFromLoadedAssemblyEvent"/> event.
   /// </summary>
   /// <seealso cref="E_CILLogical.LoadAssembly(CILReflectionContext, System.IO.Stream, EmittingArguments)"/>
   /// <seealso cref="E_CILLogical.LoadModule(CILReflectionContext,System.IO.Stream, EmittingArguments)"/>
   public sealed class AssemblyRefResolveFromLoadedAssemblyEventArgs : EventArgs
   {
      private readonly CILAssemblyName _assName;
      private readonly CILReflectionContext _context;

      internal AssemblyRefResolveFromLoadedAssemblyEventArgs( CILAssemblyName assName, CILReflectionContext ctx )
      {
         this._context = ctx;
         this._assName = assName;
      }

      /// <summary>
      /// Gets the name of the referenced assembly.
      /// </summary>
      /// <value>The name of the referenced assembly.</value>
      public CILAssemblyName AssemblyName
      {
         get
         {
            return this._assName;
         }
      }

      /// <summary>
      /// Gets or sets the loaded assembly for <see cref="AssemblyName"/>.
      /// </summary>
      /// <value>The loaded assembly for <see cref="AssemblyName"/>.</value>
      /// <remarks>Event handlers should set this property.</remarks>
      public CILAssembly ResolvedAssembly
      {
         get;
         set;
      }

      /// <summary>
      /// Gets the current <see cref="CILReflectionContext"/>.
      /// </summary>
      /// <value>The current <see cref="CILReflectionContext"/>.</value>
      public CILReflectionContext ReflectionContext
      {
         get
         {
            return this._context;
         }
      }
   }

   //#if !MONO

   //   /// <summary>
   //   /// This is event argument class for <see cref="CILReflectionContext.CSPPublicKeyEvent"/> event.
   //   /// </summary>
   //   public sealed class CSPPublicKeyEventArgs : EventArgs
   //   {
   //      private readonly String _cspName;

   //      internal CSPPublicKeyEventArgs( String cspName )
   //      {
   //         ArgumentValidator.ValidateNotNull( "Cryptographic service provider name", cspName );

   //         this._cspName = cspName;
   //      }

   //      /// <summary>
   //      /// Gets the name of the cryptographic service provider.
   //      /// </summary>
   //      /// <value>The name of the cryptographic service provider.</value>
   //      public String CSPName
   //      {
   //         get
   //         {
   //            return this._cspName;
   //         }
   //      }

   //      /// <summary>
   //      /// Gets or sets the public key of the named cryptographic service provider.
   //      /// </summary>
   //      /// <value>The public key of the named cryptographic service provider.</value>
   //      public Byte[] PublicKey { get; set; }
   //   }

   //#endif
}