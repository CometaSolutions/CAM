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
using CILAssemblyManipulator.API;
using CILAssemblyManipulator.Implementation;
using CommonUtils;

namespace CILAssemblyManipulator.API
{
   /// <summary>
   /// This is base interface for both <see cref="CILParameter"/> and <see cref="CILParameterSignature"/>.
   /// </summary>
   /// <typeparam name="TMethod">The type of the method element owning this parameter.</typeparam>
   public interface CILParameterBase<out TMethod> :
      CILElementWithCustomModifiersReadOnly,
      CILElementWithContext
      where TMethod : class
   {
      /// <summary>
      /// Gets the <see cref="CILMethod"/> owning this parameter.
      /// </summary>
      /// <value>The <see cref="CILMethod"/> owning this parameter.</value>
      /// <seealso cref="System.Reflection.ParameterInfo.Member"/>
      TMethod Method { get; }

      /// <summary>
      /// Gets the zero-based position of this parameter. If this parameter represents method's return parameter, the position is -1.
      /// </summary>
      /// <value>The zero-based position of this parameter. For method's return parameter, the value is -1.</value>
      /// <seealso cref="System.Reflection.ParameterInfo.Position"/>
      Int32 Position { get; }

   }

   /// <summary>
   /// This interface represents a method parameter in CIL environment. The interface roughly corresponds to <see cref="System.Reflection.ParameterInfo"/>. See ECMA specification for more information about CIL parameters.
   /// </summary>
   public interface CILParameter :
      CILCustomAttributeContainer,
      CILElementWithAttributes<ParameterAttributes>,
      CILElementWithSimpleName,
      CILElementWithConstant,
      CILParameterBase<CILMethodBase>,
      CILElementWithCustomModifiers,
      CILElementWithMarshalingInfo
   {

      /// <summary>
      /// Gets or sets the type of this parameter.
      /// </summary>
      /// <value>The type of this parameter.</value>
      /// <exception cref="NotSupportedException">For setter only. The exception is thrown when <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c>, meaning for the method owning this parameter, the method is generic method but not generic method definition, the <see cref="CILElementOwnedByType.DeclaringType"/> is a generic type but not generic type definition.</exception>
      /// <seealso cref="System.Reflection.ParameterInfo.ParameterType"/>
      CILTypeBase ParameterType { get; set; }
   }

   /// <summary>
   /// This interface represents parameters within <see cref="CILMethodSignature"/>.
   /// </summary>
   public interface CILParameterSignature : CILParameterBase<CILMethodSignature>
   {
      /// <summary>
      /// Gets the type of this parameter.
      /// </summary>
      /// <value>The type of this parameter.</value>
      /// <exception cref="NotSupportedException">For setter only. The exception is thrown when <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c>, meaning for the method owning this parameter, the method is generic method but not generic method definition, the <see cref="CILElementOwnedByType.DeclaringType"/> is a generic type but not generic type definition.</exception>
      /// <seealso cref="System.Reflection.ParameterInfo.ParameterType"/>
      CILTypeBase ParameterType { get; }
   }
}

public static partial class E_CIL
{
   internal const Int32 RETURN_PARAMETER_POSITION = -1;

   /// <summary>
   /// Convenience method for checking whether given <see cref="CILParameterBase{T}"/> is method's return parameter.
   /// </summary>
   /// <param name="parameter">The parameter.</param>
   /// <typeparam name="TMethod">The type parameter of <paramref name="parameter"/>.</typeparam>
   /// <returns><c>true</c> if parameter is method's return parameter; <c>false</c> otherwise.</returns>
   public static Boolean IsReturnParameter<TMethod>( this CILParameterBase<TMethod> parameter )
      where TMethod : class
   {
      return parameter != null && parameter.Position == RETURN_PARAMETER_POSITION;
   }

   /// <summary>
   /// Gets or creates a new <see cref="CILParameter"/> based on native <see cref="System.Reflection.ParameterInfo"/>.
   /// </summary>
   /// <param name="param">The native parameter.</param>
   /// <param name="ctx">The current reflection context.</param>
   /// <returns><see cref="CILParameter"/> wrapping existing native <see cref="System.Reflection.ParameterInfo"/>.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="param"/> or <paramref name="ctx"/> is <c>null</c>.</exception>
   public static CILParameter NewWrapper( this System.Reflection.ParameterInfo param, CILReflectionContext ctx )
   {
      ArgumentValidator.ValidateNotNull( "Module", param );
      ArgumentValidator.ValidateNotNull( "Reflection context", ctx );

      return ( (CILReflectionContextImpl) ctx ).Cache.GetOrAdd( param );
   }
}