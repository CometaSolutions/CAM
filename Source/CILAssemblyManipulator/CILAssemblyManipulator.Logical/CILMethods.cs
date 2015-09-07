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
using CILAssemblyManipulator.Logical;
using CILAssemblyManipulator.Logical.Implementation;
using CollectionsWithRoles.API;
using CommonUtils;
using CILAssemblyManipulator.Physical;
using System.Collections.Generic;

namespace CILAssemblyManipulator.Logical
{
   /// <summary>
   /// This is base type for <see cref="CILMethodBase"/> and <see cref="CILMethodSignature"/>.
   /// It contains properties and methods common for both CIL methods and method signatures.
   /// </summary>
   /// <typeparam name="TParameter">The type of the parameter, which will be either <see cref="CILParameter"/> for <see cref="CILMethodBase"/>, or <see cref="CILParameterSignature"/> for <see cref="CILMethodSignature"/>.</typeparam>
   public interface CILMethodOrSignature<TParameter> : CILElementWithContext
      where TParameter : class
   {
      /// <summary>
      /// Gets the parameters of this method.
      /// </summary>
      /// <value>The parameters of this method.</value>
      /// <see cref="System.Reflection.MethodBase.GetParameters()"/>
      ListQuery<TParameter> Parameters { get; }
   }

   /// <summary>
   /// This is common interface for <see cref="CILMethod"/> and <see cref="CILMethodSignature"/>.
   /// </summary>
   /// <typeparam name="TParameter">The type of the parameter.</typeparam>
   public interface CILMethodWithReturnParameter<out TParameter>
   {
      /// <summary>
      /// Returns a <see cref="CILParameter"/> representing the return parameter of this method.
      /// </summary>
      /// <value>The return parameter of this method.</value>
      TParameter ReturnParameter { get; }
   }

   /// <summary>
   /// This interface represents common information about methods and constructors in CIL environment. The interface roughly corresponds to <see cref="System.Reflection.MethodBase"/>.
   /// </summary>
   public interface CILMethodBase :
      CILCustomAttributeContainer,
      CILElementWithAttributes<MethodAttributes>,
      CILElementOwnedByType,
      CILElementInstantiable,
      CILMethodOrSignature<CILParameter>,
      CILElementTokenizableInILCode,
      CILElementWithSecurityInformation
   {

      /// <summary>
      /// Gets or sets the calling conventions for this method.
      /// </summary>
      /// <value>The calling conventions for this method.</value>
      /// <remarks>See ECMA specification for more information about calling conventions.</remarks>
      /// <seealso cref="System.Reflection.MethodBase.CallingConvention"/>
      CallingConventions CallingConvention { get; set; }

      /// <summary>
      /// Adds a new parameter to this method.
      /// </summary>
      /// <param name="name">The name of the new parameter.</param>
      /// <param name="attrs">The attributes of the new parameter.</param>
      /// <param name="paramType">The type of the new parameter.</param>
      /// <returns>A new instance of <see cref="CILParameter"/> which represents the newly added method parameter.</returns>
      /// <exception cref="NotSupportedException">If <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c>, meaning the <see cref="CILElementOwnedByType.DeclaringType"/> is a generic type but not generic type definition.</exception>
      /// <remarks>See ECMA specification for more information about method parameters.</remarks>
      /// <seealso cref="CILParameter"/>
      /// <seealso cref="CILParameterSignature"/>
      CILParameter AddParameter( String name, ParameterAttributes attrs, CILTypeBase paramType );

      /// <summary>
      /// Tries to remove an existing parameter from this method.
      /// </summary>
      /// <param name="parameter">The <see cref="CILParameter"/> to remove.</param>
      /// <returns><c>true</c> if parameter was removed successfully, <c>false</c> otherwise.</returns>
      /// <exception cref="NotSupportedException">If <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c>, meaning the <see cref="CILElementOwnedByType.DeclaringType"/> is a generic type but not generic type definition.</exception>
      Boolean RemoveParameter( CILParameter parameter );

      /// <summary>
      /// Gets or sets the method implementation attributes for this method.
      /// </summary>
      /// <value>The method implementation attributes for this method.</value>
      /// <remarks>See ECMA specification for more information about method implementation attributes.</remarks>
      MethodImplAttributes ImplementationAttributes { get; set; }

      /// <summary>
      /// Gets the IL bytecode manager for this method. See ECMA specification for more information about IL bytecode.
      /// </summary>
      /// <value>The IL bytecode manager for this method.</value>
      /// <remarks>This property will return <c>null</c> if this method can not emit IL, that is, if the <see cref="E_CILLogical.HasILMethodBody"/> returns <c>false</c>.</remarks>
      /// <exception cref="NotSupportedException">If <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c>, meaning the <see cref="CILElementOwnedByType.DeclaringType"/> is a generic type but not generic type definition.</exception>
      MethodIL MethodIL { get; }

      /// <summary>
      /// Gets information whether this is method or constructor.
      /// </summary>
      /// <value>Information whether this is method or constructor.</value>
      /// <seealso cref="MethodKind"/>
      MethodKind MethodKind { get; }

      /// <summary>
      /// Changes declaring type generic arguments. The <c>UT</c> suffix of method name means <c>UnTyped</c>, meaning that the return value is <see cref="CILMethodBase"/>. This is callthrough to the actual <see cref="CILElementOwnedByChangeableType{T}.ChangeDeclaringType(CILTypeBase[])"/>.
      /// </summary>
      /// <param name="args">New generic arguments for the declaring type.</param>
      /// <returns>A method or constructor representing this method or constructor belonging to different generic declaring type.</returns>
      CILMethodBase ChangeDeclaringTypeUT( params CILTypeBase[] args );

      /// <summary>
      /// Discards old method body and creates a new one. Returns the old method body.
      /// </summary>
      /// <returns>Old value of <see cref="MethodIL"/>.</returns>
      /// <remarks>This method will do nothing if the <see cref="E_CILLogical.HasILMethodBody"/> returns <c>false</c> for this <see cref="CILMethod"/>.</remarks>
      /// <exception cref="NotSupportedException">If <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c>, meaning the <see cref="CILElementOwnedByType.DeclaringType"/> is a generic type but not generic type definition.</exception>
      MethodIL ResetMethodIL();

   }

   /// <summary>
   /// An enumeration to help find out whether <see cref="CILMethodBase"/> is actually <see cref="CILMethod"/> or <see cref="CILConstructor"/> without casting.
   /// </summary>
   public enum MethodKind
   {
      /// <summary>
      /// Signifies that this <see cref="CILMethodBase"/> is actually an <see cref="CILMethod"/>.
      /// </summary>
      Method,
      /// <summary>
      /// Signifies that this <see cref="CILMethodBase"/> is actually an <see cref="CILConstructor"/>.
      /// </summary>
      Constructor
   }

   /// <summary>
   /// This interface represents a method in CIL environment. The interface roughly corresponds to <see cref="System.Reflection.MethodInfo"/>. See ECMA specification for more information about CIL methods.
   /// </summary>
   public interface CILMethod :
      CILMethodBase,
      CILElementWithSimpleName,
      CILElementWithGenericArguments<CILMethod>,
      CILElementOwnedByChangeableType<CILMethod>,
      CILMethodWithReturnParameter<CILParameter>
   {
      /// <summary>
      /// Gets or sets the <see cref="PInvokeAttributes"/> associated with this method.
      /// </summary>
      /// <value>The <see cref="PInvokeAttributes"/> associated with this method.</value>
      PInvokeAttributes PlatformInvokeAttributes { get; set; }

      /// <summary>
      /// Gets or sets the name of the external method.
      /// </summary>
      /// <value>The name of the external method.</value>
      String PlatformInvokeName { get; set; }

      /// <summary>
      /// Gets or sets the name of the module for platform invoke method.
      /// </summary>
      /// <value>The name of the module for platform invoke method.</value>
      String PlatformInvokeModuleName { get; set; }

      /// <summary>
      /// Returns a method representing the constructed method based on this generic method definition.
      /// </summary>
      /// <param name="args">The types to substitute the type parameters of this generic method definition.</param>
      /// <returns>A method representing the constructed method based on this generic method definition.</returns>
      /// <exception cref="InvalidOperationException">The <paramref name="args"/> are non-<c>null</c> or contain at least one element, and the this method is not generic method definition, that is, <see cref="E_CILLogical.IsGenericMethodDefinition" /> returns <c>false</c> for this method.</exception>
      /// <exception cref="ArgumentNullException">If <paramref name="args" /> is <c>null</c>.-or- Any element of <paramref name="args" /> is <c>null</c>. </exception>
      /// <exception cref="ArgumentException">The number of elements in <paramref name="args" /> is not the same as the number of type parameters of the current generic method definition.</exception>
      CILMethod MakeGenericMethod( params CILTypeBase[] args );
   }

   /// <summary>
   /// This interface represents a constructor in CIL environment. The interface roughly corresponds to <see cref="System.Reflection.ConstructorInfo"/>. See ECMA specification for more information about CIL constructors.
   /// </summary>
   public interface CILConstructor :
      CILMethodBase,
      CILElementOwnedByChangeableType<CILConstructor>
   {

   }
}

public static partial class E_CILLogical
{
   /// <summary>
   /// Checks whether the method is eligible to have method body. See ECMA specification (condition 33 for MethodDef table) for exact condition of methods having method bodies. In addition to that, the <see cref="E_CILPhysical.IsIL"/> must return <c>true</c>.
   /// </summary>
   /// <param name="method">The method to check.</param>
   /// <returns><c>true</c> if the <paramref name="method"/> is non-<c>null</c> and can have IL method body; <c>false</c> otherwise.</returns>
   /// <seealso cref="E_CILPhysical.IsIL"/>
   /// <seealso cref="E_CILPhysical.CanEmitIL"/>
   public static Boolean HasILMethodBody( this CILMethodBase method )
   {
      return method != null && method.Attributes.CanEmitIL() && method.ImplementationAttributes.IsIL();
   }

   /// <summary>
   /// Checks whether method is a generic method. The method is considered to be generic method if either its declaring type is generic or the method has any generic parameters. This method returns same result as <see cref="System.Reflection.MethodBase.IsGenericMethod"/> property.
   /// </summary>
   /// <param name="method">The method to check.</param>
   /// <returns><c>true</c> if the <paramref name="method"/> is non-<c>null</c> and is generic method; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.MethodBase.IsGenericMethod"/>
   public static Boolean IsGenericMethod( this CILMethodBase method )
   {
      return method != null && method.DeclaringType.IsGenericType() || ( MethodKind.Method == method.MethodKind && ( (CILMethod) method ).GenericArguments.Any() );
   }

   /// <summary>
   /// Returns the name of the method. For <see cref="CILConstructor"/>s it will be either <c>.ctor</c> for instance constructors or <c>.cctor</c> for static constructors.
   /// </summary>
   /// <param name="method">The method.</param>
   /// <returns>The name of the method.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="method"/> is <c>null</c>.</exception>
   public static String GetName( this CILMethodBase method )
   {
      ArgumentValidator.ValidateNotNull( "Method", method );
      return MethodKind.Method == method.MethodKind ? ( (CILMethod) method ).Name : ( method.Attributes.IsStatic() ? Miscellaneous.CLASS_CTOR_NAME : Miscellaneous.INSTANCE_CTOR_NAME );
   }

   /// <summary>
   /// Gets or creates a new <see cref="CILMethodBase"/> based on native <see cref="System.Reflection.MethodBase"/>. The actual return value will be either <see cref="CILMethod"/> or <see cref="CILConstructor"/>.
   /// </summary>
   /// <param name="ctx">The <see cref="CILReflectionContext"/>.</param>
   /// <param name="methodBase">The native method base.</param>
   /// <returns><see cref="CILMethodBase"/> wrapping existing native <see cref="System.Reflection.MethodBase"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="ctx"/> is <c>null</c>.</exception>
   public static CILMethodBase NewWrapperFromBase( this CILReflectionContext ctx, System.Reflection.MethodBase methodBase )
   {
      return methodBase is System.Reflection.ConstructorInfo ? (CILMethodBase) ctx.NewWrapper( (System.Reflection.ConstructorInfo) methodBase ) : ctx.NewWrapper( (System.Reflection.MethodInfo) methodBase );
   }

   /// <summary>
   /// Checks whether method contains generic parameters. See <see cref="System.Reflection.MethodBase.ContainsGenericParameters"/> property for more information about when method contains generic parameters.
   /// </summary>
   /// <param name="method">The method to check.</param>
   /// <returns><c>true</c> if method is non-<c>null</c> and contains any unassigned generic type parameters; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.MethodBase.ContainsGenericParameters"/>
   public static Boolean ContainsGenericParameters( this CILMethodBase method )
   {
      var result = false;
      if ( method != null )
      {
         if ( MethodKind.Method == method.MethodKind )
         {
            var mmethod = (CILMethod) method;
            result = mmethod.GenericArguments.Any( gArg => gArg.ContainsGenericParameters() )
            || mmethod.ReturnParameter.ParameterType.ContainsGenericParameters();
         }
         if ( !result )
         {
            result = method.DeclaringType.ContainsGenericParameters() || method.Parameters.Any( param => param.ParameterType.ContainsGenericParameters() );
         }
      }
      return result;
   }

   /// <summary>
   /// Checks whether method signature contains generic parameters.
   /// </summary>
   /// <param name="method">The method to check.</param>
   /// <returns><c>true</c> if method is non-<c>null</c> and contains any unassigned generic type parameters; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.MethodBase.ContainsGenericParameters"/>
   /// <remarks>
   /// Method signature contains generic parameters when any of the following is true:
   /// <list type="bullet">
   /// <item><description>the parameter type of the method signature return parameter contains generic parameters, or</description></item>
   /// <item><description>any of the parameter types of the parameters of the method signature contain generic parameters.</description></item>
   /// </list>
   /// </remarks>
   public static Boolean ContainsGenericParameters( this CILMethodSignature method )
   {
      return method != null
         && ( method.ReturnParameter.ParameterType.ContainsGenericParameters()
              || method.Parameters.Any( p => p.ParameterType.ContainsGenericParameters() ) );
   }

   /// <summary>
   /// Checks whether method is generic method definition.
   /// </summary>
   /// <param name="method">Method to check.</param>
   /// <returns><c>true</c> if <paramref name="method"/> is non-<c>null</c> and generic method definition; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.MethodBase.IsGenericMethodDefinition"/>
   public static Boolean IsGenericMethodDefinition( this CILMethod method )
   {
      return method != null && Object.ReferenceEquals( method, method.GenericDefinition );
   }

   /// <summary>
   /// Retrieves the <see cref="CILMethod"/> which is generic method definition, if applicable, and is contained in a type which is generic type definition, if applicable.
   /// </summary>
   /// <param name="method">The method.</param>
   /// <returns>The 'true' generic definition of the method; that is, its <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>true</c>.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="method"/> is <c>null</c>.</exception>
   public static CILMethod GetTrueGenericDefinition( this CILMethod method )
   {
      ArgumentValidator.ValidateNotNull( "Method", method );
      return method.HasGenericArguments() ? ( (CILTypeParameter) method.GenericDefinition.GenericArguments[0] ).DeclaringMethod : ( method.DeclaringType.IsGenericType() ? method.ChangeDeclaringType( method.DeclaringType.GenericDefinition.GenericArguments.ToArray() ) : method );
   }
   /// <summary>
   /// Convenience method to get the return type of the method.
   /// </summary>
   /// <param name="method">The method.</param>
   /// <returns>The return type of the <paramref name="method"/>.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="method"/> is <c>null</c>.</exception>
   /// <seealso cref="System.Reflection.MethodInfo.ReturnType"/>
   public static CILTypeBase GetReturnType( this CILMethod method )
   {
      ArgumentValidator.ValidateNotNull( "Method", method );
      return method.ReturnParameter.ParameterType;
   }

   /// <summary>
   /// Checks whether the method has any generic arguments, that is, its <see cref="CILElementWithGenericArguments{T}.GenericArguments"/> property has at least one item.
   /// </summary>
   /// <param name="method">The method to check.</param>
   /// <returns><c>true</c> if the <paramref name="method"/> is non-<c>null</c> and has any generic arguments; <c>false</c> otherwise.</returns>
   public static Boolean HasGenericArguments( this CILMethod method )
   {
      return method != null && method.GenericArguments.Any();
   }

   /// <summary>
   /// Creates a new <see cref="CILMethodSignature"/> that captures the signature of the current method.
   /// </summary>
   /// <param name="method">The <see cref="CILMethodBase"/>.</param>
   /// <param name="callConventions">The <see cref="UnmanagedCallingConventions"/> for the resulting <see cref="CILMethodSignature"/>.</param>
   /// <returns>The new <see cref="CILMethodSignature"/> that captures the signature of the <paramref name="method"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="method"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If any type of parameters is <c>null</c>.</exception>
   public static CILMethodSignature CreateMethodSignature( this CILMethodBase method, UnmanagedCallingConventions callConventions = UnmanagedCallingConventions.C )
   {
      return method.ReflectionContext.NewMethodSignature(
         method.DeclaringType.Module,
         callConventions | (UnmanagedCallingConventions) ( method.CallingConvention & CallingConventions.HasThis & CallingConventions.ExplicitThis ),
         MethodKind.Method == method.MethodKind ?
            ( (CILMethod) method ).ReturnParameter.ParameterType :
            method.DeclaringType.Module.AssociatedMSCorLibModule.GetTypeByName( Consts.VOID ),
         MethodKind.Method == method.MethodKind ?
            ( (CILMethod) method ).ReturnParameter.CustomModifiers.ToArray() :
            null,
         method.Parameters.Select( param => Tuple.Create( param.CustomModifiers.ToArray(), param.ParameterType ) ).ToArray()
         );
      //var cf = CollectionsWithRoles.Implementation.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY;
      //return new CILMethodSignatureImpl(
      //   method.ReflectionContext,
      //   method.DeclaringType.Module,
      //   callConventions | (UnmanagedCallingConventions) ( method.CallingConvention & CallingConventions.HasThis & CallingConventions.ExplicitThis ),
      //   MethodKind.Method == method.MethodKind ?
      //      cf.NewListProxyFromParams( ( (CILMethod) method ).ReturnParameter.CustomModifiers.ToArray() ) :
      //      null,
      //   MethodKind.Method == method.MethodKind ?
      //      ( (CILMethod) method ).ReturnParameter.ParameterType :
      //      method.DeclaringType.Module.AssociatedMSCorLibModule.GetTypeByName( Consts.VOID ),
      //      method.Parameters.Select( param => Tuple.Create( cf.NewListProxyFromParams( param.CustomModifiers.ToArray() ), param.ParameterType ) ).ToList(),
      //   method
      //      );
   }

   /// <summary>
   /// Returns all parameters of this method, including return parameter if the given method is of type <see cref="CILMethod"/>.
   /// </summary>
   /// <param name="method">The <see cref="CILMethodBase"/>.</param>
   /// <returns>All the parameters of the given method. If <paramref name="method"/> is <c>null</c>, then this returns empty enumerable.</returns>
   /// <remarks>
   /// If <paramref name="method"/> is of type <see cref="CILMethod"/>, return parameter is always returned first.
   /// </remarks>
   public static IEnumerable<CILParameter> GetAllParameters( this CILMethodBase method )
   {
      if ( method != null )
      {
         if ( method.MethodKind == MethodKind.Method )
         {
            yield return ( (CILMethod) method ).ReturnParameter;
         }

         foreach ( var param in method.Parameters )
         {
            yield return param;
         }
      }
   }

   private static Boolean IsSystemType( this CILModule thisModule, CILType other, String typeName, String typeNS = Consts.SYSTEM_NS )
   {
      return Object.Equals( thisModule.AssociatedMSCorLibModule, other.Module )
         && String.Equals( other.Name, typeName )
         && String.Equals( other.Namespace, typeNS );
   }
}