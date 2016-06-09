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
using CILAssemblyManipulator.Logical;
using CILAssemblyManipulator.Logical.Implementation;
using UtilPack;
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Logical
{
   /// <summary>
   /// This interface represents a property in CIL environment. The interface roughly corresponds to <see cref="System.Reflection.PropertyInfo"/>. See ECMA specification for more information about CIL properties.
   /// </summary>
   public interface CILProperty :
      CILCustomAttributeContainer,
      CILElementWithAttributes<PropertyAttributes>,
      CILElementOwnedByChangeableType<CILProperty>,
      CILElementWithSimpleName,
      CILElementForNamedCustomAttribute,
      CILElementWithConstant,
      CILElementInstantiable,
      CILElementWithCustomModifiers,
      CILElementWithContext
   {
      /// <summary>
      /// Gets or sets the <c>get</c> accessor method of this property. The value <c>null</c> means this property has no <c>get</c> accessor method.
      /// </summary>
      /// <value>The <c>get</c> accessor method of this property.</value>
      /// <exception cref="NotSupportedException">For setter only. The exception is thrown when <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c>, meaning the <see cref="CILElementOwnedByType.DeclaringType"/> is a generic type but not generic type definition.</exception>
      /// <seealso cref="System.Reflection.PropertyInfo.GetGetMethod(Boolean)"/>
      CILMethod GetMethod { get; set; }

      /// <summary>
      /// Gets or sets the <c>set</c> accessor method of this property. The value <c>null</c> means this property has no <c>set</c> accessor method.
      /// </summary>
      /// <value>The <c>set</c> accessor method of this property.</value>
      /// <exception cref="NotSupportedException">For setter only. The exception is thrown when <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c>, meaning the <see cref="CILElementOwnedByType.DeclaringType"/> is a generic type but not generic type definition.</exception>
      /// <seealso cref="System.Reflection.PropertyInfo.GetSetMethod(Boolean)"/>
      CILMethod SetMethod { get; set; }
   }
}

public static partial class E_CILLogical
{
   private static readonly CILParameter[] EMPTY_PARAMS = new CILParameter[0];

   /// <summary>
   /// Checks whether the property has a <c>get</c> accessor method.
   /// </summary>
   /// <param name="property">The property to check.</param>
   /// <returns><c>true</c> if <paramref name="property"/> has <c>get</c> accessor method; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.PropertyInfo.CanRead"/>
   public static Boolean CanRead( this CILProperty property )
   {
      return property != null && property.GetMethod != null;
   }

   /// <summary>
   /// Checks whether the property has a <c>set</c> accessor method.
   /// </summary>
   /// <param name="property">The property to check.</param>
   /// <returns><c>true</c> if <paramref name="property"/> has <c>set</c> accessor method; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.PropertyInfo.CanWrite"/>
   public static Boolean CanWrite( this CILProperty property )
   {
      return property != null && property.SetMethod != null;
   }

   /// <summary>
   /// Returns information about all semantically related methods of the property and how they are related.
   /// </summary>
   /// <param name="property">The property.</param>
   /// <returns>The information about all semantically related methods of the <paramref name="property"/> and how they are related.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="property"/> is <c>null</c>.</exception>
   public static IEnumerable<Tuple<MethodSemanticsAttributes, CILMethod>> GetSemanticMethods( this CILProperty property )
   {
      ArgumentValidator.ValidateNotNull( "Property", property );
      var getter = property.GetMethod;
      if ( getter != null )
      {
         yield return Tuple.Create( MethodSemanticsAttributes.Getter, getter );
      }

      var setter = property.SetMethod;
      if ( setter != null )
      {
         yield return Tuple.Create( MethodSemanticsAttributes.Setter, setter );
      }
   }

   /// <summary>
   /// Gets the type of the property.
   /// </summary>
   /// <param name="property">The property.</param>
   /// <returns>The type of the <paramref name="property"/>.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="property"/> is <c>null</c>.</exception>
   /// <exception cref="InvalidOperationException">Both <c>get</c> and <c>set</c> accessor methods of the <paramref name="property"/> are <c>null</c>.</exception>
   /// <seealso cref="System.Reflection.PropertyInfo.PropertyType"/>
   public static CILTypeBase GetPropertyType( this CILProperty property )
   {
      ArgumentValidator.ValidateNotNull( "Property", property );
      CILTypeBase result;
      var getter = property.GetMethod;
      if ( getter == null )
      {
         var setter = property.SetMethod;
         if ( setter == null )
         {
            throw new InvalidOperationException( "Given property has no getter nor setter." );
         }
         result = setter.Parameters.Last().ParameterType;
      }
      else
      {
         result = getter.ReturnParameter.ParameterType;
      }
      return result;
   }

   /// <summary>
   /// Gets any index parameters the property has.
   /// </summary>
   /// <param name="property">The property.</param>
   /// <returns>The index parameters of the <paramref name="property"/>.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="property"/> is <c>null</c>.</exception>
   /// <seealso cref="System.Reflection.PropertyInfo.GetIndexParameters()"/>
   public static IEnumerable<CILParameter> GetIndexParameters( this CILProperty property )
   {
      ArgumentValidator.ValidateNotNull( "Property", property );
      var getter = property.GetMethod;
      IEnumerable<CILParameter> result;
      if ( getter == null )
      {
         var setter = property.SetMethod;
         result = setter == null ? EMPTY_PARAMS : setter.Parameters.Take( property.SetMethod.Parameters.Count - 1 );
      }
      else
      {
         result = getter.Parameters;
      }
      return result;
   }

   /// <summary>
   /// Gets the count of the index parameters of the property.
   /// </summary>
   /// <param name="property">The property.</param>
   /// <exception cref="ArgumentNullException">If <paramref name="property"/> is <c>null</c>.</exception>
   /// <returns>The amount of index parameters of the property.</returns>
   public static Int32 GetIndexTypeCount( this CILProperty property )
   {
      ArgumentValidator.ValidateNotNull( "Property", property );
      var getter = property.GetMethod;
      Int32 result;
      if ( getter == null )
      {
         var setter = property.SetMethod;
         result = setter == null ? 0 : ( setter.Parameters.Count - 1 );
      }
      else
      {
         result = getter.Parameters.Count;
      }
      return result;
   }

   /// <summary>
   /// Helper method to emit a property, getter and/or setter methods for it, and a backing field.
   /// </summary>
   /// <param name="type">The type to add property, methods, and field to.</param>
   /// <param name="name">The name of the property.</param>
   /// <param name="propertyType">The type of the property.</param>
   /// <param name="emitSetter">Whether to emit setter method.</param>
   /// <param name="emitGetter">Whether to emit getter method.</param>
   /// <param name="backingField">This will contain the field that will be acting as a backing field for the property.</param>
   /// <param name="attrs">The method attributes to use for getter and setter methods.</param>
   /// <returns>The newly added property.</returns>
   public static CILProperty AddAutoProperty( this CILType type, String name, CILTypeBase propertyType, Boolean emitSetter, Boolean emitGetter, out CILField backingField, MethodAttributes attrs = MethodAttributes.Public )
   {
      var prop = type.AddProperty( name, PropertyAttributes.None );

      var isStatic = attrs.IsStatic();
      var fAttrs = FieldAttributes.Private;
      if ( isStatic )
      {
         fAttrs |= FieldAttributes.Static;
      }
      if ( !emitSetter )
      {
         fAttrs |= FieldAttributes.InitOnly;
      }

      backingField = type.AddField( "<" + name + ">k__BackingField", propertyType, fAttrs );
      var callConvs = isStatic ? CallingConventions.Standard : CallingConventions.HasThis;

      if ( emitSetter )
      {
         var method = type.AddMethod( "set_" + name, attrs, callConvs );
         method.ReturnParameter.ParameterType = type.Module.AssociatedMSCorLibModule.GetTypeForTypeCode( CILTypeCode.Void );
         var param = method.AddParameter( "value", ParameterAttributes.None, propertyType );
         var il = method.MethodIL;
         il.EmitStoreThisField( backingField, il2 => il2.EmitLoadArg( param ) );
         prop.SetMethod = method;
      }

      if ( emitGetter )
      {
         var method = type.AddMethod( "get_" + name, attrs, callConvs );
         method.ReturnParameter.ParameterType = propertyType;
         var il = method.MethodIL;
         il.EmitLoadThisField( backingField )
            .EmitReturn();
         prop.GetMethod = method;
      }

      return prop;
   }
}