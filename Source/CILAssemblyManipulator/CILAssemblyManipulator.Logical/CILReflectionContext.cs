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
using CILAssemblyManipulator.Logical;
using CILAssemblyManipulator.Physical.Crypto;

#if !CAM_LOGICAL_IS_DOT_NET
#pragma warning disable 1574
#endif

namespace CILAssemblyManipulator.Logical
{
   /// <summary>
   /// This interfaces encapsulates all functionality required for native reflection element wrappers, but which is missing from PCL.
   /// </summary>
   public interface CILReflectionContextWrapperCallbacks
   {
      /// <summary>
      /// Gets all top-level types defined in a given module.
      /// </summary>
      /// <param name="module">The <see cref="System.Reflection.Module"/>.</param>
      /// <returns>All top-level types defined in <paramref name="module"/>.</returns>
      IEnumerable<Type> GetTopLevelDefinedTypes( System.Reflection.Module module );

      /// <summary>
      /// Gets the module where a given type is defined.
      /// </summary>
      /// <param name="type">The <see cref="Type"/>.</param>
      /// <returns>The <see cref="System.Reflection.Module"/> where the <paramref name="type"/> is defined.</returns>
      System.Reflection.Module GetModuleOfType( Type type );

      /// <summary>
      /// Gets all custom attribute data for a given member.
      /// </summary>
      /// <param name="member">The <see cref="System.Reflection.MemberInfo"/>.</param>
      /// <returns>All <see cref="System.Reflection.CustomAttributeData"/> objects for the <paramref name="member"/>.</returns>
      /// <remarks>The return type is enumerable of <see cref="Object"/>s instead of <see cref="System.Reflection.CustomAttributeData"/> so that this would compile for Silverlight. The actual values of must be of type <see cref="System.Reflection.CustomAttributeData"/>.</remarks>
      IEnumerable<Object> GetCustomAttributesDataFor( System.Reflection.MemberInfo member );

      /// <summary>
      /// Gets all custom attribute data for a given parameter.
      /// </summary>
      /// <param name="parameter">The <see cref="System.Reflection.ParameterInfo"/>.</param>
      /// <returns>All <see cref="System.Reflection.CustomAttributeData"/> objects for the <paramref name="parameter"/>.</returns>
      /// <remarks>The return type is enumerable of <see cref="Object"/>s instead of <see cref="System.Reflection.CustomAttributeData"/> so that this would compile for PCL. The actual values of must be of type <see cref="System.Reflection.CustomAttributeData"/>.</remarks>
      IEnumerable<Object> GetCustomAttributesDataFor( System.Reflection.ParameterInfo parameter );

      /// <summary>
      /// Gets all custom attribute data for a given assembly.
      /// </summary>
      /// <param name="assembly">The <see cref="System.Reflection.Assembly"/>.</param>
      /// <returns>All <see cref="System.Reflection.CustomAttributeData"/> objects for the <paramref name="assembly"/>.</returns>
      /// <remarks>The return type is enumerable of <see cref="Object"/>s instead of <see cref="System.Reflection.CustomAttributeData"/> so that this would compile for PCL. The actual values of must be of type <see cref="System.Reflection.CustomAttributeData"/>.</remarks>
      IEnumerable<Object> GetCustomAttributesDataFor( System.Reflection.Assembly assembly );

      /// <summary>
      /// Gets all custom attribute data for a given module.
      /// </summary>
      /// <param name="module">The <see cref="System.Reflection.Module"/>.</param>
      /// <returns>All <see cref="System.Reflection.CustomAttributeData"/> objects for the <paramref name="module"/>.</returns>
      /// <remarks>The return type is enumerable of <see cref="Object"/>s instead of <see cref="System.Reflection.CustomAttributeData"/> so that this would compile for PCL. The actual values of must be of type <see cref="System.Reflection.CustomAttributeData"/>.</remarks>
      IEnumerable<Object> GetCustomAttributesDataFor( System.Reflection.Module module );

      /// <summary>
      /// Converts native <see cref="System.Reflection.CustomAttributeData"/> into <see cref="CILCustomAttribute"/>.
      /// </summary>
      /// <param name="container">The container to hold <see cref="CILCustomAttribute"/>.</param>
      /// <param name="caData">The <see cref="System.Reflection.CustomAttributeData"/> to transform. It is typed as <see cref="Object"/> so that this would compile for PCL.</param>
      /// <returns>The properly converted <see cref="CILCustomAttribute"/>.</returns>
      CILCustomAttribute GetCILCustomAttributeFromNative( CILCustomAttributeContainer container, Object caData );

      /// <summary>
      /// Gets all the methods for a given event, which have <see cref="MethodSemanticsAttributes.Other"/> semantics.
      /// </summary>
      /// <param name="evt">The <see cref="System.Reflection.EventInfo"/>.</param>
      /// <returns>all the methods for <paramref name="evt"/>, which have <see cref="MethodSemanticsAttributes.Other"/> semantics.</returns>
      IEnumerable<System.Reflection.MethodInfo> GetEventOtherMethods( System.Reflection.EventInfo evt );

      /// <summary>
      /// Gets the raw constant value for a given property.
      /// </summary>
      /// <param name="property">The <see cref="System.Reflection.PropertyInfo"/>.</param>
      /// <returns>The raw constant value for <paramref name="property"/>.</returns>
      Object GetConstantValueFor( System.Reflection.PropertyInfo property );

      /// <summary>
      /// Gets the raw constant value for a given field.
      /// </summary>
      /// <param name="field">The <see cref="System.Reflection.FieldInfo"/>.</param>
      /// <returns>The raw constant value for <paramref name="field"/>.</returns>
      Object GetConstantValueFor( System.Reflection.FieldInfo field );

      /// <summary>
      /// Gets the raw constant value for a given parameter.
      /// </summary>
      /// <param name="parameter">The <see cref="System.Reflection.ParameterInfo"/>.</param>
      /// <returns>The raw constant value for <paramref name="parameter"/>.</returns>
      Object GetConstantValueFor( System.Reflection.ParameterInfo parameter );

      /// <summary>
      /// Gets the mapping for a given type, describing method overrides for each method body.
      /// </summary>
      /// <param name="type">The <see cref="Type"/>.</param>
      /// <returns>Dictionary describing which method declarations each method body has.</returns>
      IDictionary<System.Reflection.MethodInfo, System.Reflection.MethodInfo[]> GetExplicitlyImplementedMethods( Type type );

      /// <summary>
      /// Gets information about the body of a given method.
      /// </summary>
      /// <param name="method">The <see cref="System.Reflection.MethodBase"/>.</param>
      /// <param name="locals">Information about the locals.</param>
      /// <param name="initLocals">Whether to initialize locals with their default values.</param>
      /// <param name="exceptionBlocks">Information about exception blocks.</param>
      /// <param name="ilBytes">The IL code in binary format.</param>
      void GetMethodBodyData(
         System.Reflection.MethodBase method,
         out IEnumerable<Tuple<Boolean, Type>> locals,
         out Boolean initLocals,
         out IEnumerable<Tuple<ExceptionBlockType, Int32, Int32, Int32, Int32, Type, Int32>> exceptionBlocks,
         out Byte[] ilBytes
         );

      /// <summary>
      /// Resolves type or member from a given module, identified by a given token.
      /// </summary>
      /// <param name="module">The <see cref="System.Reflection.Module"/>.</param>
      /// <param name="token">The token for member.</param>
      /// <param name="typeGenericArguments">The type arguments of the current type, if any.</param>
      /// <param name="methodGenericArguments">The type arguments of the current method, if any.</param>
      /// <returns>Resolved member.</returns>
      System.Reflection.MemberInfo ResolveTypeOrMember( System.Reflection.Module module, Int32 token, Type[] typeGenericArguments, Type[] methodGenericArguments );

      /// <summary>
      /// Resolves string from a given module, identified by a given token.
      /// </summary>
      /// <param name="module">The <see cref="System.Reflection.Module"/>.</param>
      /// <param name="token">The token for string.</param>
      /// <returns>Resolved string.</returns>
      String ResolveString( System.Reflection.Module module, Int32 token );

      /// <summary>
      /// Resolves a signature from a given module, identified by a given token.
      /// </summary>
      /// <param name="module">The <see cref="System.Reflection.Module"/>.</param>
      /// <param name="token">The token for a row containing a signature.</param>
      /// <returns>The signature in binary form.</returns>
      Byte[] ResolveSignature( System.Reflection.Module module, Int32 token );

      /// <summary>
      /// Gets the <see cref="MethodImplAttributes"/> for a given method.
      /// </summary>
      /// <param name="method">The <see cref="System.Reflection.MethodBase"/>.</param>
      /// <returns>The <see cref="MethodImplAttributes"/> for the <paramref name="method"/>.</returns>
      MethodImplAttributes GetMethodImplementationAttributes( System.Reflection.MethodBase method );

      /// <summary>
      /// Gets the <see cref="System.Runtime.InteropServices.StructLayoutAttribute"/> for a given type.
      /// </summary>
      /// <param name="type">The <see cref="Type"/>.</param>
      /// <returns>The <see cref="System.Runtime.InteropServices.StructLayoutAttribute"/> for the <paramref name="type"/>.</returns>
      System.Runtime.InteropServices.StructLayoutAttribute GetStructLayoutAttribute( Type type );

      /// <summary>
      /// Gets the information about assembly name for a given assembly.
      /// </summary>
      /// <param name="assembly">The <see cref="System.Reflection.Assembly"/>.</param>
      /// <param name="hashAlgorithm">This should be the <see cref="AssemblyHashAlgorithm"/> of the <paramref name="assembly"/>.</param>
      /// <param name="flags">This should be the <see cref="AssemblyFlags"/> of the <paramref name="assembly"/>.</param>
      /// <param name="publicKey">This should be the public key of the <paramref name="assembly" />.</param>
      void GetAssemblyNameInformation( System.Reflection.Assembly assembly, out AssemblyHashAlgorithm hashAlgorithm, out AssemblyFlags flags, out Byte[] publicKey );

      /// <summary>
      /// Gets optional and required custom modifiers for a given field.
      /// </summary>
      /// <param name="field">The <see cref="System.Reflection.FieldInfo"/>.</param>
      /// <param name="optionalModifiers">This should contain all optional custom modifiers of <paramref name="field"/>.</param>
      /// <param name="requiredModifiers">This should contain all required custom modifiers of <paramref name="field"/>.</param>
      void GetCustomModifiersFor( System.Reflection.FieldInfo field, out Type[] optionalModifiers, out Type[] requiredModifiers );

      /// <summary>
      /// Gets optional and required custom modifiers for a given property.
      /// </summary>
      /// <param name="property">The <see cref="System.Reflection.PropertyInfo"/>.</param>
      /// <param name="optionalModifiers">This should contain all optional custom modifiers of <paramref name="property"/>.</param>
      /// <param name="requiredModifiers">This should contain all required custom modifiers of <paramref name="property"/>.</param>
      void GetCustomModifiersFor( System.Reflection.PropertyInfo property, out Type[] optionalModifiers, out Type[] requiredModifiers );

      /// <summary>
      /// Gets optional and required custom modifiers for a given parameter.
      /// </summary>
      /// <param name="parameter">The <see cref="System.Reflection.ParameterInfo"/>.</param>
      /// <param name="optionalModifiers">This should contain all optional custom modifiers of <paramref name="parameter"/>.</param>
      /// <param name="requiredModifiers">This should contain all required custom modifiers of <paramref name="parameter"/>.</param>
      void GetCustomModifiersFor( System.Reflection.ParameterInfo parameter, out Type[] optionalModifiers, out Type[] requiredModifiers );

   }

   /// <summary>
   /// This class represents the isolated 'universe' in which any reading, loading, or emitting operations are done.
   /// </summary>
   /// <remarks>Typical usecase scenario would be this:
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CILManipulatorCodeContent.cs" region="CILReflectionContextCode1" language="C#" />
   /// </remarks>
   /// <seealso cref="CILReflectionContextFactory.NewContext(CILReflectionContextConcurrencySupport, CILReflectionContextWrapperCallbacks, CryptoCallbacks)"/>
   public interface CILReflectionContext : IDisposable
   {

      /// <summary>
      /// Gets the concurrency mode for this <see cref="CILReflectionContext"/>.
      /// </summary>
      /// <value>The concurrency mode for this <see cref="CILReflectionContext"/>.</value>
      /// <seealso cref="CILReflectionContextConcurrencySupport"/>
      CILReflectionContextConcurrencySupport ConcurrencySupport { get; }

      /// <summary>
      /// Gets the <see cref="CILReflectionContextWrapperCallbacks"/> used to get data for native reflection element wrappers.
      /// </summary>
      /// <value>The <see cref="CILReflectionContextWrapperCallbacks"/> used to get data for native reflection element wrappers.</value>
      CILReflectionContextWrapperCallbacks WrapperCallbacks { get; }

      ///// <summary>
      ///// This event occurs when defined types are loaded in <see cref="CILModule"/> wrapping a native <see cref="System.Reflection.Module"/> by accessing the <see cref="CILModule.DefinedTypes"/> property. The event handler should set <see cref="ModuleTypesEventArgs.DefinedTypes"/> property. This assembly can not directly access the GetTypes() method in <see cref="System.Reflection.Module"/> as the method is not present in this portable profile.
      ///// </summary>
      //event EventHandler<ModuleTypesEventArgs> ModuleTypesLoadEvent;

      ///// <summary>
      ///// This event occurs when owner module is loaded in <see cref="CILTypeBase"/> wrapping a native <see cref="System.Type"/> by accessing the <see cref="CILTypeBase.Module"/> property. The event handler should set the <see cref="TypeModuleEventArgs.Module"/> property. This assembly can not directly access the Module property in <see cref="System.Type"/> as the method is not present in this portable profile.
      ///// </summary>
      //event EventHandler<TypeModuleEventArgs> TypeModuleLoadEvent;

      ///// <summary>
      ///// This event occurs when custom attribute data is <see cref="CILCustomAttributeContainer"/> wrapping a native reflection element by accessing the <see cref="CILCustomAttributeContainer.CustomAttributeData"/> property. The event handler should set the <see cref="CustomAttributeDataEventArgs.CustomAttributeData"/> property to have same data as in System.Reflection.CustomAttributeData of the native reflection elements present in <see cref="CustomAttributeDataEventArgs"/>. This assembly can not directly access the GetCustomAttributesData() methods of native reflection elements as the methods are not present in this portable profile.
      ///// </summary>
      //event EventHandler<CustomAttributeDataEventArgs> CustomAttributeDataLoadEvent;

      ///// <summary>
      ///// This event occurs when other methods are loaded in <see cref="CILEvent"/> wrapping a native <see cref="System.Reflection.EventInfo"/> by accessing the <see cref="CILEvent.OtherMethods"/> property. The event handler should set the <see cref="EventOtherMethodsEventArgs.OtherMethods"/> property. This assembly can not directly access the GetOtherMethods() method in <see cref="System.Reflection.EventInfo"/> as the method is not present in this portable profile.
      ///// </summary>
      //event EventHandler<EventOtherMethodsEventArgs> EventOtherMethodsLoadEvent;

      ///// <summary>
      ///// This event occurs when a constant value is loaded in <see cref="CILElementWithConstant"/> wrapping a native <see cref="System.Reflection.PropertyInfo"/>, <see cref="System.Reflection.FieldInfo"/> or <see cref="System.Reflection.ParameterInfo"/>. The event handler should set the <see cref="ConstantValueLoadArgs.ConstantValue"/> property. This assembly can not directly access the GetRawConstantValue() methods in <see cref="System.Reflection.PropertyInfo"/> and <see cref="System.Reflection.FieldInfo"/> and RawDefaultValue property in <see cref="System.Reflection.ParameterInfo"/> as the property and the methods are not present in this portable profile.
      ///// </summary>
      //event EventHandler<ConstantValueLoadArgs> ConstantValueLoadEvent;

      ///// <summary>
      ///// This event occurs when an explicitly implemented methods are loaded in <see cref="CILMethod"/> wrapping a native <see cref="System.Reflection.MethodInfo"/> by accessing the <see cref="CILMethod.OverriddenMethods"/> property. The event handler should set the <see cref="ExplicitMethodImplementationLoadArgs.ExplicitlyImplementedMethods"/> property. This assembly can not directly access the GetInterfaceMap() in <see cref="System.Type"/> as the method is not present in this portable profile.
      ///// </summary>
      //event EventHandler<ExplicitMethodImplementationLoadArgs> ExplicitMethodImplementationLoadEvent;

      ///// <summary>
      ///// This event occurs when a method body is loaded in <see cref="CILMethodBase"/> wrapping a native <see cref="System.Reflection.MethodBase"/> by accessing the <see cref="CILMethodBase.MethodIL"/> property. The event handler should set the <see cref="MethodBodyLoadArgs.Locals"/>, <see cref="MethodBodyLoadArgs.InitLocals"/>, <see cref="MethodBodyLoadArgs.ExceptionInfos"/> and <see cref="MethodBodyLoadArgs.IL"/> properties. This assembly can not directly access the GetMethodBody() method in <see cref="System.Reflection.MethodBase"/> as the method is not present in this portable profile.
      ///// </summary>
      //event EventHandler<MethodBodyLoadArgs> MethodBodyLoadEvent;

      ///// <summary>
      ///// This event occurs when a method body is loaded in <see cref="CILMethodBase"/> wrapping a native <see cref="System.Reflection.MethodBase"/> by accessing the <see cref="CILMethodBase.MethodIL"/> property. The event handler should set the <see cref="TokenResolveArgs.ResolvedMember"/> or <see cref="TokenResolveArgs.ResolvedString"/> property, depending on value of the <see cref="TokenResolveArgs.ResolveKind"/> property. This assembly can not directly access the ResolveMember(Int32, Type[], Type[]) and ResolveString() methods in <see cref="System.Reflection.Module"/> as the methods are not present in this portable profile.
      ///// </summary>
      //event EventHandler<TokenResolveArgs> TokenResolveEvent;

      ///// <summary>
      ///// This event occurs when a method implementation attributes are loaded in <see cref="CILMethodBase"/> wrapping a native <see cref="System.Reflection.MethodBase"/> by accessing the <see cref="CILMethodBase.ImplementationAttributes"/> property. The event handler should set the <see cref="MethodImplAttributesEventArgs.MethodImplementationAttributes"/> property. This assembly can not directly access the MethodImplementationFlags property in <see cref="System.Reflection.MethodBase"/> as the property is not present in this portable profile.
      ///// </summary>
      //event EventHandler<MethodImplAttributesEventArgs> MethodImplementationAttributesLoadEvent;

      ///// <summary>
      ///// This event occurs when a type layout is loaded in <see cref="CILType"/> wrapping a native <see cref="System.Type"/> by accessing the <see cref="CILType.Layout"/> property. The event handler should set the <see cref="TypeLayoutEventArgs.Layout"/> property. This assembly can not directly access the StructLayoutAttribute property in <see cref="System.Type"/> as the property is not present in this portable profile.
      ///// </summary>
      //event EventHandler<TypeLayoutEventArgs> TypeLayoutLoadEvent;

      ///// <summary>
      ///// This event occurs when a assembly full name is loaded in <see cref="CILAssembly"/> wrapping a native <see cref="System.Reflection.Assembly"/> by accessing the <see cref="CILAssembly.Name"/> property. The event handler should set the <see cref="AssemblyNameEventArgs.AssemblyNameInfo"/> property. This assembly can not directly access the GetName() method in <see cref="System.Reflection.Assembly"/> as the method is not present in this portable profile.
      ///// </summary>
      //event EventHandler<AssemblyNameEventArgs> AssemblyNameLoadEvent;

      ///// <summary>
      ///// This event occurs when custom modifiers are loaded in <see cref="CILElementWithCustomModifiers"/> wrapping a native <see cref="System.Reflection.MemberInfo"/> or <see cref="System.Reflection.ParameterInfo"/> by accessing the <see cref="CILElementWithCustomModifiersReadOnly.CustomModifiers"/> property. The event handler should set the <see cref="CustomModifierEventLoadArgs.OptionalModifiers"/> and <see cref="CustomModifierEventLoadArgs.RequiredModifiers"/> properties. This assembly can not directly access the GetOptionalCustomModifiers() and GetRequiredCustomModifiers() methods in <see cref="System.Reflection.FieldInfo"/>, <see cref="System.Reflection.PropertyInfo"/> and <see cref="System.Reflection.ParameterInfo"/> as the methods are not present in this portable profile.
      ///// </summary>
      //event EventHandler<CustomModifierEventLoadArgs> CustomModifierLoadEvent;

      /// <summary>
      /// Gets the default cryptological function callbacks defined for this <see cref="CILReflectionContext"/>.
      /// </summary>
      /// <value>The default cryptological function callbacks defined for this <see cref="CILReflectionContext"/>.</value>
      CryptoCallbacks DefaultCryptoCallbacks { get; }

      /// <summary>
      /// Gets the default assembly name comparer, which should take into account the token vs full public key when comparing assembly names.
      /// </summary>
      /// <value>The default assembly name comparer, which should take into account the token vs full public key when comparing assembly names.</value>
      IEqualityComparer<CILAssemblyName> DefaultAssemblyNameComparer { get; }

      ///// <summary>
      ///// When a <see cref="CILAssembly"/> or <see cref="CILModule"/> is loaded via <see cref="E_CILLogical.LoadAssembly(CILReflectionContext, System.IO.Stream, EmittingArguments)"/> or <see cref="E_CILLogical.LoadModule(CILReflectionContext,System.IO.Stream, EmittingArguments)"/> methods, respectively, any access causing additional load of assemblies is triggered via this event, if the assembly loader function of the aforementioned methods is <c>null</c> or fails to load the assembly. The event handler should set the <see cref="AssemblyRefResolveFromLoadedAssemblyEventArgs.ResolvedAssembly"/> property.
      ///// </summary>
      //event EventHandler<AssemblyRefResolveFromLoadedAssemblyEventArgs> AssemblyReferenceResolveFromLoadedAssemblyEvent;


      /// <summary>
      /// Gets or creates a new instance of <see cref="CILAssembly"/> which will wrap the existing native <paramref name="assembly"/>.
      /// </summary>
      /// <param name="assembly">The native assembly which will serve as basis for the resulting <see cref="CILAssembly"/>.</param>
      /// <returns><see cref="CILAssembly"/> wrapping existing native <paramref name="assembly"/>, or <c>null</c> if <paramref name="assembly"/> is <c>null</c>.</returns>
      CILAssembly NewWrapper( System.Reflection.Assembly assembly );

      /// <summary>
      /// Gets or creates a new <see cref="CILEvent"/> based on native <see cref="System.Reflection.EventInfo"/>.
      /// </summary>
      /// <param name="evt">The native event.</param>
      /// <returns><see cref="CILEvent"/> wrapping existing native <see cref="System.Reflection.EventInfo"/>, or <c>null</c> if <paramref name="evt"/> is <c>null</c>.</returns>
      CILEvent NewWrapper( System.Reflection.EventInfo evt );

      /// <summary>
      /// Gets or creates a new <see cref="CILField"/> based on native <see cref="System.Reflection.FieldInfo"/>.
      /// </summary>
      /// <param name="field">The native field.</param>
      /// <returns><see cref="CILField"/> wrapping existing native <see cref="System.Reflection.FieldInfo"/>, or <c>null</c> if <paramref name="field"/> is <c>null</c>.</returns>
      CILField NewWrapper( System.Reflection.FieldInfo field );

      /// <summary>
      /// Gets or creates a new <see cref="CILMethod"/> based on native <see cref="System.Reflection.MethodInfo"/>.
      /// </summary>
      /// <param name="method">The native method.</param>
      /// <returns><see cref="CILMethod"/> wrapping existing native <see cref="System.Reflection.MethodInfo"/>, or <c>null</c> if <paramref name="method"/> is <c>null</c>.</returns>
      CILMethod NewWrapper( System.Reflection.MethodInfo method );

      /// <summary>
      /// Gets or creates a new <see cref="CILConstructor"/> based on native <see cref="System.Reflection.ConstructorInfo"/>.
      /// </summary>
      /// <param name="ctor">The native constructor.</param>
      /// <returns><see cref="CILConstructor"/> wrapping existing native <see cref="System.Reflection.ConstructorInfo"/>, or <c>null</c> if <paramref name="ctor"/> is <c>null</c>.</returns>
      CILConstructor NewWrapper( System.Reflection.ConstructorInfo ctor );

      /// <summary>
      /// Gets or creates a new <see cref="CILModule"/> based on native <see cref="System.Reflection.Module"/>.
      /// </summary>
      /// <param name="module">The native module.</param>
      /// <returns><see cref="CILModule"/> wrapping existing native <see cref="System.Reflection.Module"/>, or <c>null</c> if <paramref name="module"/> is <c>null</c>.</returns>
      CILModule NewWrapper( System.Reflection.Module module );

      /// <summary>
      /// Gets or creates a new <see cref="CILParameter"/> based on native <see cref="System.Reflection.ParameterInfo"/>.
      /// </summary>
      /// <param name="param">The native parameter.</param>
      /// <returns><see cref="CILParameter"/> wrapping existing native <see cref="System.Reflection.ParameterInfo"/>, or <c>null</c> if <paramref name="param"/> is <c>null</c>.</returns>
      CILParameter NewWrapper( System.Reflection.ParameterInfo param );

      /// <summary>
      /// Gets or creates a new <see cref="CILProperty"/> based on native <see cref="System.Reflection.PropertyInfo"/>.
      /// </summary>
      /// <param name="property">The native property.</param>
      /// <returns><see cref="CILProperty"/> wrapping existing native <see cref="System.Reflection.PropertyInfo"/>, or <c>null</c> if <paramref name="property"/> is <c>null</c>.</returns>
      CILProperty NewWrapper( System.Reflection.PropertyInfo property );

      /// <summary>
      /// Gets or creates a new <see cref="CILTypeBase"/> based on native <see cref="System.Type"/>.
      /// </summary>
      /// <param name="type">The native type.</param>
      /// <returns><see cref="CILTypeBase"/> wrapping existing native <see cref="System.Type"/>. Will be either <see cref="CILType"/> or <see cref="CILTypeParameter"/>, depending on <paramref name="type"/>. Will be <c>null</c> if <paramref name="type"/> is <c>null</c></returns>
      CILTypeBase NewWrapper( Type type );

      /// <summary>
      /// Creates a new <see cref="CILMethodSignature"/> which has all its information specified from the parameters of this method.
      /// </summary>
      /// <param name="currentModule">The current <see cref="CILModule"/>.</param>
      /// <param name="callingConventions">The <see cref="MethodSignatureInformation"/> for the method signature.</param>
      /// <param name="returnType">The return type for the method signature.</param>
      /// <param name="returnParamMods">The <see cref="CILCustomModifier"/>s for the method signature. May be <c>null</c> if no modifiers should be used.</param>
      /// <param name="parameters">The parameter information for the method signature. Each element is a tuple containing <see cref="CILCustomModifier"/>s and type for the parameter. Custom modifiers array may be <c>null</c> if no modifiers should be used.</param>
      /// <returns>A new <see cref="CILMethodSignature"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="currentModule"/>, <paramref name="returnType"/> or any of the types within <paramref name="parameters"/> is <c>null</c>.</exception>
      /// <seealso cref="CILMethodSignature"/>
      CILMethodSignature NewMethodSignature( CILModule currentModule, MethodSignatureInformation callingConventions, CILTypeBase returnType, CILCustomModifier[] returnParamMods, params Tuple<CILCustomModifier[], CILTypeBase>[] parameters );

      /// <summary>
      /// Creates a new, blank instance of <see cref="CILAssembly"/>.
      /// </summary>
      /// <param name="name">The name of the assembly.</param>
      /// <returns>New instance of <see cref="CILAssembly"/> with specified <paramref name="name"/> which will have no modules.</returns>
      CILAssembly NewBlankAssembly( String name );
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
      /// <param name="nativeWrapperCallbacks">The callbacks to get required information from native reflection element wrappers.</param>
      /// <param name="defaultCryptoCallbacks">The default callbacks for cryptographic functions. May be <c>null</c> if no default callbacks for cryptographic functions are provided.</param>
      /// <returns>A new instance of <see cref="CILReflectionContext"/>.</returns>
      public static CILReflectionContext NewContext( CILReflectionContextConcurrencySupport concurrencyMode, CILReflectionContextWrapperCallbacks nativeWrapperCallbacks, CryptoCallbacks defaultCryptoCallbacks )
      {
         return new CILReflectionContextImpl( concurrencyMode, nativeWrapperCallbacks, defaultCryptoCallbacks );
      }

#if CAM_LOGICAL_IS_DOT_NET

      /// <summary>
      /// Creates a new <see cref="CILReflectionContext"/> utilizing <see cref="CILReflectionContextWrapperCallbacksDotNET"/> and <see cref="CryptoCallbacksDotNET"/>.
      /// </summary>
      /// <param name="concurrencyMode">The concurrency mode for the new <see cref="CILReflectionContext"/>.</param>
      /// <returns>A new instance of <see cref="CILReflectionContext"/>.</returns>
      public static CILReflectionContext NewContext( CILReflectionContextConcurrencySupport concurrencyMode = CILReflectionContextConcurrencySupport.NotThreadSafe )
      {
         return NewContext( concurrencyMode, new CILReflectionContextWrapperCallbacksDotNET(), new CryptoCallbacksDotNET() );
      }
#endif
   }
}

public static partial class E_CILLogical
{
   internal static IEnumerable<Type> GetTopLevelDefinedTypesOrThrow( this CILReflectionContextWrapperCallbacks callbacks, System.Reflection.Module module )
   {
      return callbacks.PerformForCallbacks( () => callbacks.GetTopLevelDefinedTypes( module ), "defined types in module" );
   }

   internal static System.Reflection.Module GetModuleOfTypeOrThrow( this CILReflectionContextWrapperCallbacks callbacks, Type type )
   {
      return callbacks.PerformForCallbacks( () => callbacks.GetModuleOfType( type ), "module of type" );
   }

   internal static IEnumerable<Object> GetCustomAttributesDataForOrThrow( this CILReflectionContextWrapperCallbacks callbacks, System.Reflection.MemberInfo member )
   {
      return callbacks.PerformForCallbacks( () => callbacks.GetCustomAttributesDataFor( member ), "custom attributes for member" );
   }

   internal static IEnumerable<Object> GetCustomAttributesDataForOrThrow( this CILReflectionContextWrapperCallbacks callbacks, System.Reflection.ParameterInfo parameter )
   {
      return callbacks.PerformForCallbacks( () => callbacks.GetCustomAttributesDataFor( parameter ), "custom attributes for parameter" );
   }

   internal static IEnumerable<Object> GetCustomAttributesDataForOrThrow( this CILReflectionContextWrapperCallbacks callbacks, System.Reflection.Assembly assembly )
   {
      return callbacks.PerformForCallbacks( () => callbacks.GetCustomAttributesDataFor( assembly ), "custom attributes for assembly" );
   }

   internal static IEnumerable<Object> GetCustomAttributesDataForOrThrow( this CILReflectionContextWrapperCallbacks callbacks, System.Reflection.Module module )
   {
      return callbacks.PerformForCallbacks( () => callbacks.GetCustomAttributesDataFor( module ), "custom attributes for module" );
   }

   internal static CILCustomAttribute GetCILCustomAttributeFromNativeOrThrow( this CILReflectionContextWrapperCallbacks callbacks, CILCustomAttributeContainer container, Object caData )
   {
      return callbacks.PerformForCallbacks( () => callbacks.GetCILCustomAttributeFromNative( container, caData ), "custom attribute data" );
   }

   internal static IEnumerable<System.Reflection.MethodInfo> GetEventOtherMethodsOrThrow( this CILReflectionContextWrapperCallbacks callbacks, System.Reflection.EventInfo evt )
   {
      return callbacks.PerformForCallbacks( () => callbacks.GetEventOtherMethods( evt ), "other methods for event" );
   }

   internal static Object GetConstantValueForOrThrow( this CILReflectionContextWrapperCallbacks callbacks, System.Reflection.PropertyInfo property )
   {
      return callbacks.PerformForCallbacksNoCheck( () => callbacks.GetConstantValueFor( property ), "constant value for property" );
   }

   internal static Object GetConstantValueForOrThrow( this CILReflectionContextWrapperCallbacks callbacks, System.Reflection.FieldInfo field )
   {
      return callbacks.PerformForCallbacksNoCheck( () => callbacks.GetConstantValueFor( field ), "constant value for field" );
   }

   internal static Object GetConstantValueForOrThrow( this CILReflectionContextWrapperCallbacks callbacks, System.Reflection.ParameterInfo parameter )
   {
      return callbacks.PerformForCallbacksNoCheck( () => callbacks.GetConstantValueFor( parameter ), "constant value for parameter" );
   }

   internal static IDictionary<System.Reflection.MethodInfo, System.Reflection.MethodInfo[]> GetExplicitlyImplementedMethodsOrThrow( this CILReflectionContextWrapperCallbacks callbacks, Type type )
   {
      return callbacks.PerformForCallbacks( () => callbacks.GetExplicitlyImplementedMethods( type ), "explicitly implemented methods for type" );
   }

   internal static void GetMethodBodyDataOrThrow(
      this CILReflectionContextWrapperCallbacks callbacks,
      System.Reflection.MethodBase method,
      out IEnumerable<Tuple<Boolean, Type>> locals,
      out Boolean initLocals,
      out IEnumerable<Tuple<ExceptionBlockType, Int32, Int32, Int32, Int32, Type, Int32>> exceptionBlocks,
      out Byte[] ilBytes
      )
   {
      ThrowWrapperCallbacksException( callbacks, "method body data" );
      callbacks.GetMethodBodyData( method, out locals, out initLocals, out exceptionBlocks, out ilBytes );
      ThrowWrapperCallbacksExceptionNoCallbacksCheck( locals, "local variables of method" );
      ThrowWrapperCallbacksExceptionNoCallbacksCheck( exceptionBlocks, "exception blocks of method" );
      ThrowWrapperCallbacksExceptionNoCallbacksCheck( ilBytes, "IL bytes of method" );
   }

   internal static System.Reflection.MemberInfo ResolveTypeOrMemberOrThrow( this CILReflectionContextWrapperCallbacks callbacks, System.Reflection.Module module, Int32 token, Type[] typeGenericArguments, Type[] methodGenericArguments )
   {
      return callbacks.PerformForCallbacks( () => callbacks.ResolveTypeOrMember( module, token, typeGenericArguments, methodGenericArguments ), "member for token" );
   }

   internal static String ResolveStringOrThrow( this CILReflectionContextWrapperCallbacks callbacks, System.Reflection.Module module, Int32 token )
   {
      return callbacks.PerformForCallbacks( () => callbacks.ResolveString( module, token ), "string for token" );
   }

   internal static Byte[] ResolveSignatureOrThrow( this CILReflectionContextWrapperCallbacks callbacks, System.Reflection.Module module, Int32 token )
   {
      return callbacks.PerformForCallbacks( () => callbacks.ResolveSignature( module, token ), "signature for token" );
   }

   internal static MethodImplAttributes GetMethodImplementationAttributesOrThrow( this CILReflectionContextWrapperCallbacks callbacks, System.Reflection.MethodBase method )
   {
      return callbacks.PerformForCallbacks( () => callbacks.GetMethodImplementationAttributes( method ), "method implementation attributes" );
   }

   internal static System.Runtime.InteropServices.StructLayoutAttribute GetStructLayoutAttributeOrThrow( this CILReflectionContextWrapperCallbacks callbacks, Type type )
   {
      return callbacks.PerformForCallbacks( () => callbacks.GetStructLayoutAttribute( type ), "type layout information" );
   }

   internal static void GetAssemblyNameInformationOrThrow( this CILReflectionContextWrapperCallbacks callbacks, System.Reflection.Assembly assembly, out AssemblyHashAlgorithm hashAlgorithm, out AssemblyFlags flags, out Byte[] publicKey )
   {
      ThrowWrapperCallbacksException( callbacks, "assembly name information" );
      callbacks.GetAssemblyNameInformation( assembly, out hashAlgorithm, out flags, out publicKey );
   }

   internal static void GetCustomModifiersForOrThrow( this CILReflectionContextWrapperCallbacks callbacks, System.Reflection.PropertyInfo property, out Type[] optionalModifiers, out Type[] requiredModifiers )
   {
      ThrowWrapperCallbacksException( callbacks, "custom modifiers for property" );
      callbacks.GetCustomModifiersFor( property, out optionalModifiers, out requiredModifiers );
      ThrowWrapperCallbacksExceptionNoCallbacksCheck( optionalModifiers, "optional modifiers for property" );
      ThrowWrapperCallbacksExceptionNoCallbacksCheck( requiredModifiers, "required modifiers for property" );
   }

   internal static void GetCustomModifiersForOrThrow( this CILReflectionContextWrapperCallbacks callbacks, System.Reflection.FieldInfo field, out Type[] optionalModifiers, out Type[] requiredModifiers )
   {
      ThrowWrapperCallbacksException( callbacks, "custom modifiers for field" );
      callbacks.GetCustomModifiersFor( field, out optionalModifiers, out requiredModifiers );
      ThrowWrapperCallbacksExceptionNoCallbacksCheck( optionalModifiers, "optional modifiers for field" );
      ThrowWrapperCallbacksExceptionNoCallbacksCheck( requiredModifiers, "required modifiers for field" );
   }

   internal static void GetCustomModifiersForOrThrow( this CILReflectionContextWrapperCallbacks callbacks, System.Reflection.ParameterInfo parameter, out Type[] optionalModifiers, out Type[] requiredModifiers )
   {
      ThrowWrapperCallbacksException( callbacks, "custom modifiers for parameter" );
      callbacks.GetCustomModifiersFor( parameter, out optionalModifiers, out requiredModifiers );
      ThrowWrapperCallbacksExceptionNoCallbacksCheck( optionalModifiers, "optional modifiers for parameter" );
      ThrowWrapperCallbacksExceptionNoCallbacksCheck( requiredModifiers, "required modifiers for parameter" );
   }

   private static T PerformForCallbacksNoCheck<T>( this CILReflectionContextWrapperCallbacks callbacks, Func<T> func, String whatIsResult )
      where T : class
   {
      ThrowWrapperCallbacksException( callbacks, whatIsResult );
      return func();
   }

   private static T PerformForCallbacks<T>( this CILReflectionContextWrapperCallbacks callbacks, Func<T> func, String whatIsResult )
   {
      Object retVal = callbacks == null ? null : (Object) func();
      ThrowWrapperCallbacksException( callbacks, retVal, whatIsResult );
      return (T) retVal;
   }

   private static void ThrowWrapperCallbacksException( CILReflectionContextWrapperCallbacks callbacks, String whatIsResult )
   {
      if ( callbacks == null )
      {
         throw new NativeWrapperException( "Providing " + whatIsResult + " requires callback object for native wrappers, but it was not provided." );
      }
   }

   private static void ThrowWrapperCallbacksException( CILReflectionContextWrapperCallbacks callbacks, Object result, String whatIsResult )
   {
      ThrowWrapperCallbacksException( callbacks, whatIsResult );
      ThrowWrapperCallbacksExceptionNoCallbacksCheck( result, whatIsResult );
   }

   private static void ThrowWrapperCallbacksExceptionNoCallbacksCheck( Object result, String whatIsResult )
   {
      if ( result == null )
      {
         throw new NativeWrapperException( "The callback object for native wrappers failed to provide " + whatIsResult + "." );
      }
   }
}

#if !CAM_LOGICAL_IS_DOT_NET
#pragma warning restore 1574
#endif