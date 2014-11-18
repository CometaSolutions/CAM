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
using CILAssemblyManipulator.API;
using CILAssemblyManipulator.Implementation;
using CollectionsWithRoles.API;
using CommonUtils;

namespace CILAssemblyManipulator.API
{
   /// <summary>
   /// This interface represents a single custom attribute specification in CIL. See ECMA specification for more information about custom attributes.
   /// </summary>
   public interface CILCustomAttribute
   {
      /// <summary>
      /// Overwrites current custom attribute data with the one given in parameters.
      /// </summary>
      /// <param name="ctor">The constructor of the custom attribute class.</param>
      /// <param name="ctorArgs">The typed arguments to the custom attribute constructor.</param>
      /// <param name="namedArgs">The named arguments to the custom attribute properties or fields.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="ctor"/> is <c>null</c>.</exception>
      void SetCustomAttributeData( CILConstructor ctor, IEnumerable<CILCustomAttributeTypedArgument> ctorArgs, IEnumerable<CILCustomAttributeNamedArgument> namedArgs );

      /// <summary>
      /// Gets typed arguments of this custom attribute data.
      /// </summary>
      /// <value>Typed arguments of this custom attribute data.</value>
      /// <seealso cref="CILCustomAttributeTypedArgument"/>
      ListQuery<CILCustomAttributeTypedArgument> ConstructorArguments { get; }

      /// <summary>
      /// Gets named arguments of this custom attribute data.
      /// </summary>
      /// <value>Named arguments of this custom attribute data.</value>
      /// <seealso cref="CILCustomAttributeNamedArgument"/>
      ListQuery<CILCustomAttributeNamedArgument> NamedArguments { get; }

      /// <summary>
      /// Gets the attribute constructor of this custom attribute data.
      /// </summary>
      /// <value>Attribute constructor of this custom attribute data.</value>
      CILConstructor Constructor { get; }

      /// <summary>
      /// Gets the CIL element containing this custom attribute data.
      /// </summary>
      /// <value>The CIL element containing this custom attribute data.</value>
      /// <seealso cref="CILCustomAttributeContainer"/>
      /// <seealso cref="CILAssembly"/>
      /// <seealso cref="CILEvent"/>
      /// <seealso cref="CILField"/>
      /// <seealso cref="CILMethodBase"/>
      /// <seealso cref="CILModule"/>
      /// <seealso cref="CILParameter"/>
      /// <seealso cref="CILProperty"/>
      /// <seealso cref="CILTypeOrTypeParameter"/>
      CILCustomAttributeContainer Container { get; }
   }

   /// <summary>
   /// This interface represents a single typed argument within custom attribute data. See ECMA specification for more information about custom attribute data and typed arguments within it.
   /// </summary>
   /// <seealso cref="CILCustomAttributeFactory"/>
   public interface CILCustomAttributeTypedArgument
   {
      /// <summary>
      /// Gets or sets the type of this argument.
      /// </summary>
      /// <value>The type of this argument.</value>
      CILType ArgumentType { get; set; }

      /// <summary>
      /// Gets or sets the value of this argument.
      /// </summary>
      /// <value>The value of this argument.</value>
      Object Value { get; set; }
   }

   /// <summary>
   /// This interface represents a single named argument within custom attribute data. See ECMA specification for more information about custom attribute data and named arguments within it.
   /// </summary>
   /// <seealso cref="CILCustomAttributeFactory"/>
   public interface CILCustomAttributeNamedArgument
   {
      /// <summary>
      /// Gets or sets the typed argument of this named argument.
      /// </summary>
      /// <value>The typed argument holding type and value of this named argument.</value>
      CILCustomAttributeTypedArgument TypedValue { get; set; }

      /// <summary>
      /// Gets or sets the affected member of this named argument.
      /// </summary>
      /// <value>The affected member of this named argument.</value>
      /// <seealso cref="CILElementForNamedCustomAttribute"/>
      /// <seealso cref="CILProperty"/>
      /// <seealso cref="CILField"/>
      CILElementForNamedCustomAttribute NamedMember { get; set; }
   }

   ///// <summary>
   ///// This is struct that may be used as a reference to enum when enum type is not part of custom attribute named or typed argument type.
   ///// For example, let's assume a custom attribute constructor takes an object as argument.
   ///// An enum value may be specified as that argument, but if the native type of the enum is not available (eg. the enum is in same module as the one being emitted), then there must be a way to distinguish between normal <c>int</c> argument and argument of boxed enum type.
   ///// By using this struct, the emitter will recognize such boxed enum values and will emit correct custom attribute data for them.
   ///// </summary>
   ///// <remarks>
   ///// The usage of this type is not required if the parameter, field or property already has enum type as its type.
   ///// TODO some code sample.
   ///// </remarks>
   //public struct CILEnumReferenceWithinCustomAttributeData
   //{
   //   private readonly CILType _enumType;
   //   private readonly Object _value;

   //   /// <summary>
   //   /// Creates new <see cref="CILEnumReferenceWithinCustomAttributeData"/> with given values.
   //   /// </summary>
   //   /// <param name="enumType">The <see cref="CILType"/> of the enum.</param>
   //   /// <param name="value">The numeric value of the enum.</param>
   //   /// <exception cref="ArgumentNullException">If <paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
   //   public CILEnumReferenceWithinCustomAttributeData( CILType enumType, Object value )
   //   {
   //      ArgumentValidator.ValidateNotNull( "Enum type", enumType );
   //      ArgumentValidator.ValidateNotNull( "Enum value", value );

   //      this._enumType = enumType;
   //      this._value = value;
   //   }

   //   /// <summary>
   //   /// Gets the enum type of this boxed enum value.
   //   /// </summary>
   //   /// <value>The enum type of this boxed enum value.</value>
   //   public CILType EnumType
   //   {
   //      get
   //      {
   //         return this._enumType;
   //      }
   //   }

   //   /// <summary>
   //   /// Gets the numberic value of this boxed enum value.
   //   /// </summary>
   //   /// <value>The numberic value of this boxed enum value.</value>
   //   public Object Value
   //   {
   //      get
   //      {
   //         return this._value;
   //      }
   //   }
   //}


   /// <summary>
   /// Provides methods to create <see cref="CILCustomAttributeTypedArgument"/> and <see cref="CILCustomAttributeNamedArgument"/>. In order to create <see cref="CILCustomAttribute"/>, use extension methods provided for <see cref="CILCustomAttributeContainer"/> and all of its sub-interfaces.
   /// </summary>
   public static class CILCustomAttributeFactory
   {
      internal static CILCustomAttribute NewAttribute( CILCustomAttributeContainer container, CILConstructor ctor, IEnumerable<CILCustomAttributeTypedArgument> ctorArgs, IEnumerable<CILCustomAttributeNamedArgument> namedArgs )
      {
         var result = new CILCustomAttributeImpl( container );
         result.SetCustomAttributeData( ctor, ctorArgs, namedArgs );
         return result;
      }

      /// <summary>
      /// Creates a new <see cref="CILCustomAttributeTypedArgument"/> with specified value. The type of <paramref name="value"/> will be used as type for typed argument.
      /// </summary>
      /// <param name="value">The value of the typed argument. Can not be <c>null</c>.</param>
      /// <param name="ctx">The current reflection context.</param>
      /// <returns>A new <see cref="CILCustomAttributeTypedArgument"/> with specified value.</returns>
      public static CILCustomAttributeTypedArgument NewTypedArgument( Object value, CILReflectionContext ctx )
      {
         ArgumentValidator.ValidateNotNull( "Value", value );
         ArgumentValidator.ValidateNotNull( "Reflection context", ctx );

         return NewTypedArgument( value.GetType().NewWrapperAsType( ctx ), value );
      }

      /// <summary>
      /// Creates a new <see cref="CILCustomAttributeTypedArgument"/> with specified type and value.
      /// </summary>
      /// <param name="type">The type for the typed argument.</param>
      /// <param name="value">The value for the typed argument.</param>
      /// <returns>A new <see cref="CILCustomAttributeTypedArgument"/> with specified type and value.</returns>
      public static CILCustomAttributeTypedArgument NewTypedArgument( CILType type, Object value )
      {
         return new CILCustomAttributeTypedArgumentImpl( type, value );
      }

      /// <summary>
      /// Creates a new <see cref="CILCustomAttributeNamedArgument"/> with specified member and typed argument.
      /// </summary>
      /// <param name="namedMember">The member of the named argument.</param>
      /// <param name="value">The typed argument of the named argument.</param>
      /// <returns>A new <see cref="CILCustomAttributeNamedArgument"/> with specified member and typed argument.</returns>
      public static CILCustomAttributeNamedArgument NewNamedArgument( CILElementForNamedCustomAttribute namedMember, CILCustomAttributeTypedArgument value )
      {
         return new CILCustomAttributeNamedArgumentImpl( value, namedMember );
      }
   }
}

public static partial class E_CIL
{
   /// <summary>
   /// Returns name of the member of the <see cref="CILCustomAttributeNamedArgument"/>.
   /// </summary>
   /// <param name="arg">The named argument.</param>
   /// <returns>Name of the member of the <see cref="CILCustomAttributeNamedArgument"/>.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="arg"/> is <c>null</c>.</exception>
   /// <seealso cref="CILCustomAttributeNamedArgument"/>
   public static String GetName( this CILCustomAttributeNamedArgument arg )
   {
      ArgumentValidator.ValidateNotNull( "Named argument", arg );
      return arg.NamedMember.Name;
   }
}