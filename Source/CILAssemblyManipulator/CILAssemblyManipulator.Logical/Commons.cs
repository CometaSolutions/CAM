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
using CollectionsWithRoles.API;
using CommonUtils;
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Logical
{
   /// <summary>
   /// This interface represents any CIL element with textual name.
   /// </summary>
   public interface CILElementWithSimpleName
   {
      /// <summary>
      /// Gets or sets the name of this CIL element.
      /// </summary>
      /// <value>The name of this CIL element.</value>
      String Name { get; set; }
   }

   /// <summary>
   /// This interface represents any CIL element with custom attributes. See ECMA specification for more information about custom attributes.
   /// </summary>
   /// <seealso cref="CILEvent"/>
   /// <seealso cref="CILField"/>
   /// <seealso cref="CILMethodBase"/>
   /// <seealso cref="CILModule"/>
   /// <seealso cref="CILParameter"/>
   /// <seealso cref="CILProperty"/>
   /// <seealso cref="CILTypeBase"/>
   public interface CILCustomAttributeContainer : CILElementWithContext
   {
      /// <summary>
      /// Adds new custom attribute declaration to this CIL element. If <paramref name="ctorArgs"/> or <paramref name="namedArgs"/> are <c>null</c>, they are interpreted as empty enumerables.
      /// </summary>
      /// <param name="ctor">The constructor of the custom attribute.</param>
      /// <param name="ctorArgs">The parameters for the custom attribute constructor. May be <c>null</c>.</param>
      /// <param name="namedArgs">The named parameters for custom attribute fields and types. May be <c>null</c>.</param>
      /// <returns>Newly added <see cref="CILCustomAttribute"/></returns>
      /// <exception cref="ArgumentNullException">If <paramref name="ctor"/> is <c>null</c>.</exception>
      CILCustomAttribute AddCustomAttribute( CILConstructor ctor, IEnumerable<CILCustomAttributeTypedArgument> ctorArgs, IEnumerable<CILCustomAttributeNamedArgument> namedArgs );

      /// <summary>
      /// Tries to remove custom attribute from this CIL element.
      /// </summary>
      /// <param name="attribute">The custom attribute to remove.</param>
      /// <returns><c>true</c> if custom attribute was removed; <c>false</c> otherwise.</returns>
      Boolean RemoveCustomAttribute( CILCustomAttribute attribute );

      /// <summary>
      /// Gets all the custom attribute data currently applied on this CIL element.
      /// </summary>
      /// <value>All the custom attribute data currently applied on this CIL element.</value>
      ListQuery<CILCustomAttribute> CustomAttributeData { get; }
   }

   /// <summary>
   /// This interface represents CIL element which can be set by custom attribute named argument. This interface is implemented by <see cref="CILField"/> and <see cref="CILProperty"/>.
   /// </summary>
   /// <seealso cref="CILField"/>
   /// <seealso cref="CILProperty"/>
   public interface CILElementForNamedCustomAttribute : CILElementWithSimpleName, CILElementOwnedByType
   {

   }

   /// <summary>
   /// This interface represents CIL element with generic type arguments. This interface is implemented by <see cref="CILType"/> and <see cref="CILMethod"/>.
   /// </summary>
   /// <typeparam name="TGDef">The type of generic definition.</typeparam>
   /// <seealso cref="CILType"/>
   /// <seealso cref="CILMethod"/>
   public interface CILElementWithGenericArguments<out TGDef>
      where TGDef : class
   {
      /// <summary>
      /// Gets the generic definition of this CIL element. Will be <c>null</c> if this is not generic type or method.
      /// </summary>
      /// <value>The generic definition of this CIL element, or <c>null</c> if this is not generic type or method.</value>
      TGDef GenericDefinition { get; }

      /// <summary>
      /// Gets the generic arguments of this CIL element. Will be empty if this is not generic type or method.
      /// </summary>
      /// <value>The generic arguments of this CIL element. Will be empty if this is not generic type or method.</value>
      ListQuery<CILTypeBase> GenericArguments { get; }

      /// <summary>
      /// If this is not a generic type or method, and <paramref name="names"/> is non-<c>null</c> and contains at least one element, then changes this type or method to become generic type definition or generic method definition, respectively.
      /// </summary>
      /// <param name="names">The names of the generic type parameters to have.</param>
      /// <returns>The newly created generic type parameters. Will return empty array if <paramref name="names"/> is <c>null</c> or empty.</returns>
      /// <remarks>If <paramref name="names"/> is <c>null</c> or empty, this type or method does not become generic type or method.</remarks>
      /// <exception cref="NotSupportedException">For methods only. The exception is thrown when <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c>, meaning <see cref="CILElementOwnedByType.DeclaringType"/> is a generic type but not generic type definition.</exception>
      /// <exception cref="InvalidOperationException">If this type or method already has generic type parameters.</exception>
      CILTypeParameter[] DefineGenericParameters( params String[] names );
   }

   /// <summary>
   /// Represents any CIL element which is contained within a <see cref="CILType"/>. This interface is implemented by <see cref="CILEvent"/>, <see cref="CILField"/>, <see cref="CILMethodBase"/>, <see cref="CILProperty"/> and <see cref="CILTypeBase"/>.
   /// </summary>
   public interface CILElementOwnedByType
   {
      /// <summary>
      /// Gets the declaring type of this CIL element. If this is <see cref="CILType"/>, the return value may be <c>null</c>.
      /// </summary>
      /// <value>The declaring type of this CIL element. If this is <see cref="CILType"/>, the return value may be <c>null</c>.</value>
      CILType DeclaringType { get; }
   }

   /// <summary>
   /// Represents any CIL element which is contained within a <see cref="CILType"/>, and it is possible to create an instance of this element owned by <see cref="CILType"/> with different generic arguments. This interface is implemented by <see cref="CILEvent"/>, <see cref="CILField"/>, <see cref="CILMethodBase"/> and <see cref="CILProperty"/>.
   /// </summary>
   /// <typeparam name="T">The type of this CIL element.</typeparam>
   public interface CILElementOwnedByChangeableType<out T> : CILElementOwnedByType
   {
      /// <summary>
      /// Changes declaring type generic arguments.
      /// </summary>
      /// <param name="args">New generic arguments for the declaring type.</param>
      /// <returns>A CIL element representing this CIL element belonging to different generic declaring type.</returns>
      /// <exception cref="InvalidOperationException">If <see cref="CILElementOwnedByType.DeclaringType"/> is not generic type, and <paramref name="args"/> is not null or empty.</exception>
      /// <exception cref="ArgumentNullException">If <see cref="CILElementOwnedByType.DeclaringType"/> is generic type definition and <paramref name="args"/> is <c>null</c> or any element of <paramref name="args"/> is <c>null</c>.</exception>
      /// <exception cref="ArgumentException">If number of elements in <paramref name="args"/> is not the same as number of generic arguments in <see cref="CILElementOwnedByType.DeclaringType"/></exception>
      T ChangeDeclaringType( params CILTypeBase[] args );
   }

   /// <summary>
   /// Represents any CIL element which has some kind of attributes, which is usually an enum type. This interface is implemented by <see cref="CILEvent"/>, <see cref="CILField"/>, <see cref="CILMethodBase"/>, <see cref="CILParameter"/>, <see cref="CILProperty"/>, <see cref="CILType"/> and <see cref="CILTypeParameter"/>.
   /// </summary>
   /// <typeparam name="TAttributes">The type of the attributes, typically an enum type.</typeparam>
   public interface CILElementWithAttributes<TAttributes>
   {
      /// <summary>
      /// Gets or sets the attributes of this CIL element.
      /// </summary>
      /// <value>The attributes of this CIL element.</value>
      TAttributes Attributes { get; set; }
   }

   /// <summary>
   /// Represents any CIL element with a possible compile-time constant. The presence of the constant is detected usually by reading <see cref="CILElementWithAttributes{T}.Attributes"/> property. This interface is implemented by <see cref="CILField"/>, <see cref="CILParameter"/> and <see cref="CILProperty"/>.
   /// </summary>
   public interface CILElementWithConstant
   {
      /// <summary>
      /// Gets or sets the constant value of this CIL element.
      /// </summary>
      /// <value>The constant value of this CIL element.</value>
      Object ConstantValue { get; set; }
   }

   /// <summary>
   /// Represents any CIL element capable of containing a type. This interface is implemented by <see cref="CILModule"/> and <see cref="CILType"/>.
   /// </summary>
   /// <see cref="CILModule"/>
   /// <see cref="CILType"/>
   public interface CILElementCapableOfDefiningType
   {
      /// <summary>
      /// Adds a new type this <see cref="CILType"/> or <see cref="CILModule"/>.
      /// </summary>
      /// <param name="name">The name of the type.</param>
      /// <param name="attrs">The <see cref="TypeAttributes"/> of the type.</param>
      /// <param name="tc">The <see cref="CILTypeCode"/> of the type. Unless one is emitting <c>mscorlib</c>, this should be <see cref="CILTypeCode.Object"/>.</param>
      /// <returns>A newly added type.</returns>
      CILType AddType( String name, TypeAttributes attrs, CILTypeCode tc = CILTypeCode.Object );

      /// <summary>
      /// Tries to remove <paramref name="type"/> from this <see cref="CILType"/> or <see cref="CILModule"/>.
      /// </summary>
      /// <param name="type">The type to remove.</param>
      /// <returns><c>true</c> if <paramref name="type"/> was removed; <c>false</c> otherwise.</returns>
      Boolean RemoveType( CILType type );
   }

   /// <summary>
   /// Represents any CIL element capable of having 'virtual instance' of itself, as opposed to its actual concrete form. This interface is implemented by <see cref="CILEvent"/>, <see cref="CILField"/>, <see cref="CILMethodBase"/>, <see cref="CILProperty"/> and <see cref="CILType"/>.
   /// </summary>
   /// <remarks>
   /// <para>
   /// For example, a <see cref="CILType"/> which is generic type definition, is a concrete form. However, it may have 'virtual instances' of itself, which have different generic type arguments than the generic type definition. This interface provides a way to query whether some CIL element is in its concrete form.
   /// </para><para>
   /// Many methods which somehow modify the state (however, not all of such methods) require the <see cref="CILElementInstantiable.IsTrueDefinition"/> property to be <c>true</c>.
   /// </para>
   /// </remarks>
   public interface CILElementInstantiable
   {
      /// <summary>
      /// Gets the value indicating whether this CIL element is a 'concrete' instance.
      /// </summary>
      /// <value>The value indicating whether this CIL element is a 'concrete' instance.</value>
      Boolean IsTrueDefinition { get; }
   }

   /// <summary>
   /// Represents any CIL element capable of having custom modifiers. See ECMA specification for more information about custom modifiers. This interface is implemented by <see cref="CILField"/>, <see cref="CILParameter"/> and <see cref="CILProperty"/>.
   /// </summary>
   public interface CILElementWithCustomModifiers : CILElementWithCustomModifiersReadOnly
   {
      /// <summary>
      /// Adds a new custom modifier to the list of existing ones to this CIL element.
      /// </summary>
      /// <param name="type">The type of the custom modifier.</param>
      /// <param name="isOptional">Whether custom modifiers is optional.</param>
      /// <returns>A newly added <see cref="CILCustomModifier"/>.</returns>
      /// <exception cref="NotSupportedException">When <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c> for this element. In case of <see cref="CILParameter"/>, the owning method's <see cref="CILElementInstantiable.IsTrueDefinition"/> property is queried.</exception>
      CILCustomModifier AddCustomModifier( CILType type, Boolean isOptional );

      /// <summary>
      /// Tries to remove an existing custom modifiers from the list of the existing ones in this CIL element.
      /// </summary>
      /// <param name="modifier">The <see cref="CILCustomModifier"/> to remove.</param>
      /// <returns><c>true</c> if <paramref name="modifier"/> was removed; <c>false</c> otherwise.</returns>
      /// <exception cref="NotSupportedException">When <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c> for this element. In case of <see cref="CILParameter"/>, the owning method's <see cref="CILElementInstantiable.IsTrueDefinition"/> property is queried.</exception>
      Boolean RemoveCustomModifier( CILCustomModifier modifier );
   }

   /// <summary>
   /// Represents read-only view to any CIL element capable of having custom modifiers.
   /// </summary>
   public interface CILElementWithCustomModifiersReadOnly
   {
      /// <summary>
      /// Gets the list of all custom modifiers applied to this CIL element.
      /// </summary>
      /// <value>The list of all custom modifiers applied to this CIL element.</value>
      ListQuery<CILCustomModifier> CustomModifiers { get; }
   }

   /// <summary>
   /// This interface provides an easy way to get the <see cref="CILReflectionContext"/> to which this CIL element belongs to. This inteface is implemented by <see cref="CILAssembly"/>, <see cref="CILEvent"/>, <see cref="CILField"/>, <see cref="CILMethodBase"/>, <see cref="CILModule"/>, <see cref="CILParameter"/>, <see cref="CILProperty"/> and <see cref="CILTypeBase"/>.
   /// </summary>
   public interface CILElementWithContext
   {
      /// <summary>
      /// Gets the <see cref="CILReflectionContext"/> of this CIL element.
      /// </summary>
      /// <value>The <see cref="CILReflectionContext"/> of this CIL element.</value>
      CILReflectionContext ReflectionContext { get; }
   }

   /// <summary>
   /// This interfaces is common interface for all CIL elements that can appear as targets of IL bytecode instructions.
   /// </summary>
   public interface CILElementTokenizableInILCode
   {
      /// <summary>
      /// Gets the <see cref="CILElementWithinILCode"/> of this <see cref="CILElementTokenizableInILCode"/>.
      /// </summary>
      /// <value>The <see cref="CILElementWithinILCode"/> of this <see cref="CILElementTokenizableInILCode"/>.</value>
      CILElementWithinILCode ElementTypeKind { get; }
   }

   /// <summary>
   /// This enumeration contains all possible values for CIL elements that can appear as tokens within IL bytecode.
   /// </summary>
   public enum CILElementWithinILCode
   {
      /// <summary>
      /// The element is <see cref="CILTypeBase" />.
      /// </summary>
      Type,
      /// <summary>
      /// The element is <see cref="CILMethodBase" />.
      /// </summary>
      Method,
      /// <summary>
      /// The element is <see cref="CILField" />.
      /// </summary>
      Field,
   }

   /// <summary>
   /// This interface is common interface for CIL elements which have marshalling information about how data should be marshalled when calling to or from unmanaged code via PInvoke.
   /// </summary>
   /// <seealso cref="CILField"/>
   /// <seealso cref="CILParameter"/>
   public interface CILElementWithMarshalingInfo
   {
      /// <summary>
      /// Gets or sets the marshalling information of this field or parameter. If no marshalling information is used, this should be <c>null</c>.
      /// </summary>
      /// <value>The marshalling information of this field or parameter.</value>
      LogicalMarshalingInfo MarshalingInformation { get; set; }
   }

   /// <summary>
   /// This class represents marshalling information used for CIL parameters and fields.
   /// The instances of this class are created via static methods in this class.
   /// </summary>
   /// <seealso cref="CILElementWithMarshalingInfo"/>
   /// <seealso cref="CILField"/>
   /// <seealso cref="CILParameter"/>
   public sealed class LogicalMarshalingInfo
   {

      /// <summary>
      /// Gets or sets the physical marshaling information.
      /// </summary>
      /// <value>The physical marshaling information.</value>
      /// <seealso cref="AbstractMarshalingInfo"/>
      public AbstractMarshalingInfo PhysicalMarshalingInfo { get; set; }

      /// <summary>
      /// Gets or sets the logical <see cref="CILType"/> reference for safe array custom type, or custom marshal type.
      /// </summary>
      /// <value>The logical <see cref="CILType"/> reference for safe array custom type, or custom marshal type.</value>
      public CILType SafeArrayCustomTypeOrCustomMarshalType { get; set; }

#if !CAM_LOGICAL_IS_SL

      /// <summary>
      /// Creates <see cref="AbstractMarshalingInfo"/> with all information specified in <see cref="System.Runtime.InteropServices.MarshalAsAttribute"/>.
      /// </summary>
      /// <param name="attr">The <see cref="System.Runtime.InteropServices.MarshalAsAttribute"/>. If <c>null</c>, then the result will be <c>null</c> as well.</param>
      /// <param name="ctx">The <see cref="CILReflectionContext"/>. May be <c>null</c> if no type information is contained within the given attribute.</param>
      /// <returns>A new <see cref="AbstractMarshalingInfo"/> with given information.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="attr"/> has non-<c>null</c> <see cref="System.Runtime.InteropServices.MarshalAsAttribute.SafeArrayUserDefinedSubType"/>, <see cref="System.Runtime.InteropServices.MarshalAsAttribute.MarshalType"/> or <see cref="System.Runtime.InteropServices.MarshalAsAttribute.MarshalTypeRef"/> fields, and <paramref name="ctx"/> is <c>null</c>.</exception>
      public static LogicalMarshalingInfo FromAttribute( System.Runtime.InteropServices.MarshalAsAttribute attr, CILReflectionContext ctx )
      {
         LogicalMarshalingInfo result;
         if ( attr == null )
         {
            result = null;
         }
         else
         {
            if ( attr.SafeArrayUserDefinedSubType != null || attr.MarshalType != null || attr.MarshalTypeRef != null )
            {
               ArgumentValidator.ValidateNotNull( "Reflection context", ctx );
            }

            CILType customType;
            var physicalInfo = AbstractMarshalingInfo.FromAttribute( attr );
            switch ( physicalInfo.MarshalingInfoKind )
            {
               case MarshalingInfoKind.SafeArray:
                  customType = ctx.NewWrapperAsType( attr.SafeArrayUserDefinedSubType );
                  break;
               case MarshalingInfoKind.Custom:
                  customType = ctx.NewWrapperAsType( attr.MarshalTypeRef ?? Type.GetType( attr.MarshalType, true ) );
                  break;
               default:
                  customType = null;
                  break;
            }

            result = new LogicalMarshalingInfo()
            {
               PhysicalMarshalingInfo = physicalInfo,
               SafeArrayCustomTypeOrCustomMarshalType = customType
            };
         }
         return result;
      }

#endif

   }

   /// <summary>
   /// This class represents a single security attribute declaration.
   /// Instances of this class are created via <see cref="CILElementWithSecurityInformation.AddDeclarativeSecurity(SecurityAction, CILType)"/> method.
   /// </summary>
   /// <seealso cref="CILElementWithSecurityInformation"/>
   /// <seealso cref="CILElementWithSecurityInformation.AddDeclarativeSecurity(SecurityAction, CILType)"/>
   public class LogicalSecurityInformation
   {
      private CILType _securityAttributeType;
      private readonly List<CILCustomAttributeNamedArgument> _namedArguments;
      private readonly SecurityAction _action;

      internal LogicalSecurityInformation( SecurityAction action, CILType securityAttributeType, IEnumerable<CILCustomAttributeNamedArgument> namedArgs = null )
      {
         this._action = action;
         this._securityAttributeType = securityAttributeType;
         this._namedArguments = namedArgs == null ? new List<CILCustomAttributeNamedArgument>() : namedArgs.Where( arg => arg != null ).ToList();
      }

      /// <summary>
      /// Gets or sets the type of the security attribute.
      /// </summary>
      /// <value>The type of the security attribute.</value>
      public CILType SecurityAttributeType
      {
         get
         {
            return this._securityAttributeType;
         }
         set
         {
            this._securityAttributeType = value;
         }
      }

      /// <summary>
      /// Gets the <see cref="SecurityAction"/> associated with this security attribute declaration.
      /// </summary>
      /// <value>The <see cref="SecurityAction"/> associated with this security attribute declaration.</value>
      public SecurityAction SecurityAction
      {
         get
         {
            return this._action;
         }
      }

      /// <summary>
      /// Gets the <see cref="CILCustomAttributeNamedArgument"/>s of this security attribute declaration.
      /// </summary>
      /// <value>The <see cref="CILCustomAttributeNamedArgument"/>s of this security attribute declaration.</value>
      public List<CILCustomAttributeNamedArgument> NamedArguments
      {
         get
         {
            return this._namedArguments;
         }
      }
   }

   /// <summary>
   /// This is common interface for <see cref="CILMethodBase"/> and <see cref="CILType"/> that provides methods for manipulating security information.
   /// </summary>
   public interface CILElementWithSecurityInformation
   {
      /// <summary>
      /// Gets the security information associated with this method or type.
      /// </summary>
      /// <value>the security information associated with this method or type.</value>
      DictionaryQuery<SecurityAction, ListQuery<LogicalSecurityInformation>> DeclarativeSecurity { get; }

      /// <summary>
      /// Adds a new <see cref="SecurityInformation"/> object to this method or type with specified <see cref="SecurityAction"/> and security attribute type.
      /// </summary>
      /// <param name="action">The <see cref="SecurityAction"/>.</param>
      /// <param name="securityAttributeType">The security attribute type.</param>
      /// <returns>A new instance of <see cref="SecurityInformation"/> with given security <paramref name="action"/> and <paramref name="securityAttributeType"/>.</returns>
      LogicalSecurityInformation AddDeclarativeSecurity( SecurityAction action, CILType securityAttributeType );

      /// <summary>
      /// Tries to remove given <see cref="SecurityInformation"/> object from this method or type.
      /// </summary>
      /// <param name="information">The <see cref="SecurityInformation"/> to remove.</param>
      /// <returns><c>true</c> if removal was successul; <c>false</c> otherwise.</returns>
      Boolean RemoveDeclarativeSecurity( LogicalSecurityInformation information );
   }
}

public static partial class E_CILLogical
{
   /// <summary>
   /// Adds a new custom attribute which invokes the non-static parameterless constructor of the <paramref name="type"/>.
   /// </summary>
   /// <param name="container">The <see cref="CILCustomAttributeContainer"/> to own the custom attribute.</param>
   /// <param name="type">The type of the custom attribute.</param>
   /// <returns>Newly added <see cref="CILCustomAttribute"/>.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="container"/> or <paramref name="type"/> is <c>null</c>.</exception>
   /// <exception cref="InvalidOperationException">If non-static parameterless constructor could not be found from <paramref name="type"/>.</exception>
   /// <seealso cref="CILCustomAttributeContainer.AddCustomAttribute"/>
   public static CILCustomAttribute AddNewCustomAttribute( this CILCustomAttributeContainer container, CILType type )
   {
      ArgumentValidator.ValidateNotNull( "Custom attribute container", container );
      ArgumentValidator.ValidateNotNull( "Custom attribute type", type );
      var ctor = type.Constructors.FirstOrDefault( c => !c.Attributes.IsStatic() && !c.Parameters.Any() );
      if ( ctor == null )
      {
         throw new InvalidOperationException( "Could not find parameterless non-static constructor for the " + type + "." );
      }
      return container.AddCustomAttribute( ctor, null, null );
   }

   /// <summary>
   /// Adds a new custom attribute which uses the given constructor, passing given parameters to it.
   /// </summary>
   /// <param name="container">The <see cref="CILCustomAttributeContainer"/> to own the custom attribute.</param>
   /// <param name="ctor">The <see cref="CILConstructor"/> of the custom attribute.</param>
   /// <param name="ctorArgs">The parameters for that <paramref name="ctor"/>.</param>
   /// <returns>Newly added <see cref="CILCustomAttribute"/>.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="container"/> or <paramref name="ctor"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If <paramref name="ctor"/> parameter count differs from amount of elements in <paramref name="ctorArgs"/>.</exception>
   /// <seealso cref="CILCustomAttributeContainer.AddCustomAttribute"/>
   public static CILCustomAttribute AddNewCustomAttributeTypedParams( this CILCustomAttributeContainer container, CILConstructor ctor, params CILCustomAttributeTypedArgument[] ctorArgs )
   {
      ArgumentValidator.ValidateNotNull( "Custom attribute container", container );
      return container.AddCustomAttribute( ctor, ctorArgs, null );
   }

   /// <summary>
   /// Adds a new custom attribute which uses the given parameterless constructor and given named arguments.
   /// </summary>
   /// <param name="container">The <see cref="CILCustomAttributeContainer"/> to own the custom attribute.</param>
   /// <param name="ctor">The parameterless <see cref="CILConstructor"/> of the custom attribute.</param>
   /// <param name="namedArgs">The named parameters for the custom attribute.</param>
   /// <returns>Newly added <see cref="CILCustomAttribute"/>.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="container"/> or <paramref name="ctor"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If <paramref name="ctor"/> is not parameterless.</exception>
   /// <seealso cref="CILCustomAttributeContainer.AddCustomAttribute"/>
   public static CILCustomAttribute AddNewCustomAttributeNamedParams( this CILCustomAttributeContainer container, CILConstructor ctor, params CILCustomAttributeNamedArgument[] namedArgs )
   {
      ArgumentValidator.ValidateNotNull( "Custom attribute container", container );
      return container.AddCustomAttribute( ctor, null, namedArgs );
   }

   /// <summary>
   /// Ads an optional custom modifier to the <paramref name="element"/>.
   /// </summary>
   /// <param name="element">The <see cref="CILElementWithCustomModifiers"/> to add custom modifier to.</param>
   /// <param name="type">The optional custom modifier to add.</param>
   /// <returns>A newly added <see cref="CILCustomModifier" />.</returns>
   /// <exception cref="NotSupportedException">When <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c> for this element. In case of <see cref="CILParameter"/>, the owning method's <see cref="CILElementInstantiable.IsTrueDefinition"/> property is queried.</exception>
   public static CILCustomModifier AddOptionalCustomModifier( this CILElementWithCustomModifiers element, CILType type )
   {
      return element.AddCustomModifier( type, true );
   }

   /// <summary>
   /// Ads an required custom modifier to the <paramref name="element"/>.
   /// </summary>
   /// <param name="element">The <see cref="CILElementWithCustomModifiers"/> to add custom modifier to.</param>
   /// <param name="type">The required custom modifier to add.</param>
   /// <returns>A newly added <see cref="CILCustomModifier" />.</returns>
   /// <exception cref="NotSupportedException">When <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c> for this element. In case of <see cref="CILParameter"/>, the owning method's <see cref="CILElementInstantiable.IsTrueDefinition"/> property is queried.</exception>
   public static CILCustomModifier AddRequiredCustomModifier( this CILElementWithCustomModifiers element, CILType type )
   {
      return element.AddCustomModifier( type, false );
   }

   /// <summary>
   /// Gets the corresponding field which belongs to the type which is generic type definition, if declaring type is generic type.
   /// </summary>
   /// <param name="field">The field.</param>
   /// <returns>The corresponding field which belongs to the type which is generic type definition, if declaring type is generic type.</returns>
   public static CILField GetFieldForDeclaringTypeGenericDefinition( this CILField field )
   {
      return field.DeclaringType.IsGenericType() && !field.DeclaringType.IsGenericTypeDefinition() ?
         field.ChangeDeclaringType( field.DeclaringType.GenericDefinition.GenericArguments.ToArray() ) :
         field;
   }

   /// <summary>
   /// Gets the corresponding method which belongs to the type which is generic type definition, if declaring type is generic type.
   /// </summary>
   /// <typeparam name="T">The actual method type.</typeparam>
   /// <param name="method">The method.</param>
   /// <returns>The corresponding method which belongs to the type which is generic type definition, if declaring type is generic type.</returns>
   public static T GetMethodForDeclaringTypeGenericDefinition<T>( this T method )
      where T : CILMethodBase
   {
      return method.DeclaringType.IsGenericType() && !method.DeclaringType.IsGenericTypeDefinition() ?
         (T) method.ChangeDeclaringTypeUT( method.DeclaringType.GenericDefinition.GenericArguments.ToArray() ) :
         method;
   }


#if WINDOWS_PHONE_APP

   // In some places, adding "using System.Reflection;" would cause massive havoc and need to prepend everything with CILAssemblyManipulator.Logical namespace.
   // Therefore, provide extension methods of that namespace here
   internal static System.Reflection.TypeInfo GetTypeInfo( this Type type )
   {
      return System.Reflection.IntrospectionExtensions.GetTypeInfo( type );
   }

   internal static System.Reflection.EventInfo GetEvent( this Type type, String name )
   {
      return System.Reflection.RuntimeReflectionExtensions.GetRuntimeEvent( type, name );
   }

   internal static IEnumerable<System.Reflection.EventInfo> GetEvents( this Type type )
   {
      return System.Reflection.RuntimeReflectionExtensions.GetRuntimeEvents( type );
   }

   internal static System.Reflection.FieldInfo GetField( this Type type, String name )
   {
      return System.Reflection.RuntimeReflectionExtensions.GetRuntimeField( type, name );
   }

   internal static IEnumerable<System.Reflection.FieldInfo> GetFields( this Type type )
   {
      return System.Reflection.RuntimeReflectionExtensions.GetRuntimeFields( type );
   }

   internal static System.Reflection.MethodInfo GetMethod( this Type type, String name, Type[] parameters )
   {
      return System.Reflection.RuntimeReflectionExtensions.GetRuntimeMethod( type, name, parameters );
   }

   internal static IEnumerable<System.Reflection.MethodInfo> GetMethods( this Type type )
   {
      return System.Reflection.RuntimeReflectionExtensions.GetRuntimeMethods( type );
   }

   internal static System.Reflection.PropertyInfo GetProperty( this Type type, String name )
   {
      return System.Reflection.RuntimeReflectionExtensions.GetRuntimeProperty( type, name );
   }

   internal static IEnumerable<System.Reflection.PropertyInfo> GetProperties( this Type type )
   {
      return System.Reflection.RuntimeReflectionExtensions.GetRuntimeProperties( type );
   }

   internal static IEnumerable<System.Reflection.ConstructorInfo> GetConstructors( this Type type )
   {
      return type.GetTypeInfo().DeclaredConstructors;
   }

   internal static System.Reflection.MethodInfo GetRuntimeBaseDefinition( this System.Reflection.MethodInfo method )
   {
      return System.Reflection.RuntimeReflectionExtensions.GetRuntimeBaseDefinition( method );
   }

   internal static IEnumerable<Object> GetCustomAttributes( this System.Reflection.FieldInfo field, Boolean inherit )
   {
      return System.Reflection.CustomAttributeExtensions.GetCustomAttributes( field, inherit );
   }

   internal static IEnumerable<Object> GetCustomAttributes( this System.Reflection.MethodBase method, Type attributeType, Boolean inherit )
   {
      return System.Reflection.CustomAttributeExtensions.GetCustomAttributes( method, attributeType, inherit );
   }

   internal static IEnumerable<Object> GetCustomAttributes( this Type type, Boolean inherit )
   {
      return System.Reflection.CustomAttributeExtensions.GetCustomAttributes( type.GetTypeInfo(), inherit );
   }

   internal static IEnumerable<Object> GetCustomAttributes( this System.Reflection.ParameterInfo parameter, Boolean inherit )
   {
      return System.Reflection.CustomAttributeExtensions.GetCustomAttributes( parameter, inherit );
   }

   internal static IEnumerable<Type> GetInterfaces( this Type type )
   {
      return type.GetTypeInfo().ImplementedInterfaces;
   }

   internal static System.Reflection.Module[] GetModules( this System.Reflection.Assembly ass )
   {
      return ass.Modules.ToArray();
   }

   internal static System.Reflection.MethodInfo GetAddMethod( this System.Reflection.EventInfo evt, Boolean ignored )
   {
      return evt.AddMethod;
   }

   internal static System.Reflection.MethodInfo GetRemoveMethod( this System.Reflection.EventInfo evt, Boolean ignored )
   {
      return evt.RemoveMethod;
   }

   internal static System.Reflection.MethodInfo GetRaiseMethod( this System.Reflection.EventInfo evt, Boolean ignored )
   {
      return evt.RaiseMethod;
   }

   internal static System.Reflection.MethodInfo GetSetMethod( this System.Reflection.PropertyInfo property, Boolean ignored )
   {
      return property.SetMethod;
   }

   internal static System.Reflection.MethodInfo GetGetMethod( this System.Reflection.PropertyInfo property, Boolean ignored )
   {
      return property.GetMethod;
   }

   internal static IEnumerable<Type> GetNestedTypes( this Type type )
   {
      return type.GetTypeInfo().DeclaredNestedTypes.Select( ti => ti.AsType() );
   }

   internal static CILTypeCode GetTypeCode(this Type type)
   {
      return type == null ? CILTypeCode.Empty : GetTypeCodeNotNull(type);
   }

   private static CILTypeCode GetTypeCodeNotNull(Type type)
   {
      return type.GetTypeInfo().IsEnum ? GetTypeCode(Enum.GetUnderlyingType(type)) : GetTypeCodeNotNullNotEnum(type);
   }

   private static CILTypeCode GetTypeCodeNotNullNotEnum(Type type)
   {
      return Object.Equals(type.GetTypeInfo().Assembly, Utils.NATIVE_MSCORLIB) ? GetTypeCodeNotNullNotEnumSystemAssembly(type) : CILTypeCode.Object;
   }

   private static CILTypeCode GetTypeCodeNotNullNotEnumSystemAssembly(Type type)
   {
      // This method acts as a corresponding Type.GetTypeCode(Type) method of .NET framework.
      // Therefore, don't return unexpected values like CILTypeCode.Void etc from this one.
      switch(type.FullName)
      {
         case Consts.BOOLEAN:
            return CILTypeCode.Boolean;
         case Consts.CHAR:
            return CILTypeCode.Char;
         case Consts.SBYTE:
            return CILTypeCode.SByte;
         case Consts.BYTE:
            return CILTypeCode.Byte;
         case Consts.INT16:
            return CILTypeCode.Int16;
         case Consts.UINT16:
            return CILTypeCode.UInt16;
         case Consts.INT32:
            return CILTypeCode.Int32;
         case Consts.UINT32:
            return CILTypeCode.UInt32;
         case Consts.INT64:
            return CILTypeCode.Int64;
         case Consts.UINT64:
            return CILTypeCode.UInt64;
         case Consts.SINGLE:
            return CILTypeCode.Single;
         case Consts.DOUBLE:
            return CILTypeCode.Double;
         case Consts.DECIMAL:
            return CILTypeCode.Decimal;
         case Consts.DATETIME:
            return CILTypeCode.DateTime;
         case Consts.STRING:
            return CILTypeCode.String;
         default:
            return CILTypeCode.Object;
      }
   }

#endif
}