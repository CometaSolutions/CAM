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
using CollectionsWithRoles.API;
using CommonUtils;
using CollectionsWithRoles.Implementation;
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Logical
{
   /// <summary>
   /// This is common interface for all kind of types within CIL assembly.
   /// </summary>
   /// <seealso cref="CILType"/>
   /// <seealso cref="CILTypeParameter"/>
   /// <seealso cref="CILMethodSignature"/>
   public interface CILTypeBase :
      CILElementWithContext,
      CILElementTokenizableInILCode
   {
      /// <summary>
      /// Gets information whether this is type or generic type parameter.
      /// </summary>
      /// <value>Information whether this is type or generic type parameter.</value>
      /// <seealso cref="CILAssemblyManipulator.Logical.TypeKind"/>
      TypeKind TypeKind { get; }

      /// <summary>
      /// Gets the <see cref="CILModule"/> where this type is defined.
      /// </summary>
      /// <value>The <see cref="CILModule"/> where this type is defined.</value>
      CILModule Module { get; }
   }

   /// <summary>
   /// This interface represents function pointer type within CIL.
   /// </summary>
   /// <remarks>
   /// The <see cref="CILMethodSignature"/> acts as a type, and can be used in types of fields, events, parameters and local variables.
   /// Additionally, it may be used to perform native calls, see <see cref="E_MethodIL.EmitCall( MethodIL, CILMethodSignature, CILTypeBase[])"/> and <see cref="E_MethodIL.EmitCall(MethodIL, CILMethodSignature, Tuple{CILCustomModifier[], CILTypeBase}[])"/>.
   /// </remarks>
   public interface CILMethodSignature :
      CILTypeBase,
      CILMethodOrSignature<CILParameterSignature>,
      CILMethodWithReturnParameter<CILParameterSignature>
   {
      /// <summary>
      /// Gets the calling conventions for this method signature.
      /// </summary>
      /// <value>The calling conventions for this method signature.</value>
      /// <remarks>See ECMA specification for more information about calling conventions.</remarks>
      /// <seealso cref="UnmanagedCallingConventions"/>
      UnmanagedCallingConventions CallingConvention { get; }

      /// <summary>
      /// This method is called automatically whenever setting method signature type as field, event, parameter, or local type.
      /// It creates a new copy of this <see cref="CILMethodSignature"/> with different module.
      /// </summary>
      /// <param name="newModule"></param>
      /// <returns></returns>
      CILMethodSignature CopyToOtherModule( CILModule newModule );

      /// <summary>
      /// Gets the <see cref="CILMethodBase"/> this method signature was created from, if any.
      /// </summary>
      /// <value>The <see cref="CILMethodBase"/> this method signature was created from, or <c>null</c>.</value>
      CILMethodBase OriginatingMethod { get; }
   }

   /// <summary>
   /// This interface represents information about types and generic type parameters in CIL environment. The interface is logically equivalent to parts of <see cref="System.Type"/>.
   /// </summary>
   public interface CILTypeOrTypeParameter :
      CILTypeBase,
      CILCustomAttributeContainer,
      CILElementWithSimpleName,
      CILElementOwnedByType
   {
      /// <summary>
      /// Gets or sets the namespace of this type.
      /// </summary>
      /// <value>The namespace of this type.</value>
      /// <seealso cref="System.Type.Namespace"/>
      String Namespace { get; set; }

      /// <summary>
      /// Returns the <see cref="CILTypeCode"/> for this <see cref="CILTypeBase"/>. This behaves in a similar way as <see cref="System.Type.GetTypeCode(System.Type)"/>.
      /// </summary>
      /// <value>The <see cref="CILTypeCode"/> for this <see cref="CILTypeBase"/>.</value>
      /// <seealso cref="System.Type.GetTypeCode(System.Type)"/>
      /// <seealso cref="CILTypeCode"/>
      CILTypeCode TypeCode { get; }
   }

   /// <summary>
   /// This interface represents information about classes, interfaces, and structs in CIL environment. Generic type parameters will never be instances of this interface. The interface is logically equivalent to parts of <see cref="System.Type"/>.
   /// </summary>
   public interface CILType :
      CILTypeOrTypeParameter,
      CILElementWithAttributes<TypeAttributes>,
      CILElementWithGenericArguments<CILType>,
      CILElementCapableOfDefiningType,
      CILElementInstantiable,
      CILElementWithSecurityInformation
   {
      /// <summary>
      /// Gets or sets the base type of this <see cref="CILType"/>.
      /// </summary>
      /// <value>The base type of this <see cref="CILType"/>.</value>
      /// <exception cref="NotSupportedException">For setter only. The exception is thrown when <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c> for this type, meaning this type is a generic type but not generic type definition.</exception>
      /// <seealso cref="System.Type.BaseType"/>
      CILType BaseType { get; set; }

      /// <summary>
      /// Gets the kind of element modifier for this type. Will be <c>null</c> if this type is not element type.
      /// </summary>
      /// <value>The kind of element modifier for this type. Will be <c>null</c> if this type is not element type.</value>
      /// <seealso cref="CILAssemblyManipulator.Logical.ElementKind"/>
      ElementKind? ElementKind { get; }

      /// <summary>
      /// Returns the <see cref="CILTypeBase"/> of the object encompassed or referred to by the current array, pointer or reference type. Returns <c>null</c> if this type is not any of those.
      /// </summary>
      /// <value>The <see cref="CILTypeBase"/> of the object encompassed or referred to by the current array, pointer or reference type. Will be <c>null</c> if this type is not any of those.</value>
      /// <seealso cref="System.Type.GetElementType()"/>
      CILTypeBase ElementType { get; }

      /// <summary>
      /// Returns the array information for this <see cref="CILType"/>. If this <see cref="CILType"/> is not array type, or it is a vector array type, <c>null</c> is returned.
      /// </summary>
      /// <value>The <see cref="GeneralArrayInfo"/> for this array type, if this is non-vector array type. Otherwise, <c>null</c> is returned.</value>
      /// <seealso cref="GeneralArrayInfo"/>
      GeneralArrayInfo ArrayInformation { get; }

      /// <summary>
      /// Gets or sets class layout of this <see cref="CILType"/>. See ECMA specification for more information about class layout.
      /// </summary>
      /// <value>The class layout of this <see cref="CILType"/>.</value>
      /// <seealso cref="ClassLayout"/>
      ClassLayout? Layout { get; set; }

      /// <summary>
      /// Gets all the declared fields of this type.
      /// </summary>
      /// <value>All the declared fields of this type.</value>
      ListQuery<CILField> DeclaredFields { get; }

      /// <summary>
      /// Gets all the constructors of this type.
      /// </summary>
      /// <value>All the constructors of this type.</value>
      ListQuery<CILConstructor> Constructors { get; }

      /// <summary>
      /// Gets all the declared methods of this type.
      /// </summary>
      /// <value>All the declared methods of this type.</value>
      ListQuery<CILMethod> DeclaredMethods { get; }

      /// <summary>
      /// Gets all the types nested within this type.
      /// </summary>
      /// <value>All the types nested within this type.</value>
      ListQuery<CILType> DeclaredNestedTypes { get; }

      /// <summary>
      /// Gets all the declared properties of this type.
      /// </summary>
      /// <value>All the declared properties of this type.</value>
      ListQuery<CILProperty> DeclaredProperties { get; }

      /// <summary>
      /// Gets all the delcared events of this type.
      /// </summary>
      /// <value>All the declared events of this type.</value>
      ListQuery<CILEvent> DeclaredEvents { get; }

      /// <summary>
      /// Gets bottom-most interfaces in the hierarhcy of all interfaces implemented by this type.
      /// </summary>
      /// <value>The bottom-most interfaces in the hierarhcy of all interfaces implemented by this type.</value>
      ListQuery<CILType> DeclaredInterfaces { get; }

      /// <summary>
      /// Adds interfaces to the list of implemented interfaces of this type. If this type already implements any of the <paramref name="iFaces"/> or any of their sub-interfaces, that interface is not added.
      /// </summary>
      /// <param name="iFaces">The interfaces to make this type implement.</param>
      /// <exception cref="ArgumentException">If cycle is detected in inheritance hierarchy between this type and any of the <paramref name="iFaces"/> types.</exception>
      /// <exception cref="NotSupportedException">When <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c> for this type, meaning this type is a generic type but not generic type definition.</exception>
      void AddDeclaredInterfaces( params CILType[] iFaces );

      /// <summary>
      /// Tries to remove given interface from the list of implemented interfaces of this type.
      /// </summary>
      /// <param name="iFace">The interface to remove.</param>
      /// <returns><c>true</c> if the interface was removed from the list; <c>false</c> otherwise.</returns>
      /// <exception cref="NotSupportedException">When <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c> for this type, meaning this type is a generic type but not generic type definition.</exception>
      Boolean RemoveDeclaredInterface( CILType iFace );

      /// <summary>
      /// Adds a new <see cref="CILField"/> to this <see cref="CILType"/>.
      /// </summary>
      /// <param name="name">The name of the field being added.</param>
      /// <param name="fieldType">The <see cref="CILTypeBase">type</see> of the field being added.</param>
      /// <param name="attr">The <see cref="FieldAttributes"/> of the field being added.</param>
      /// <returns>A newly added <see cref="CILConstructor"/>.</returns>
      /// <exception cref="NotSupportedException">When <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c> for this type, meaning this type is a generic type but not generic type definition.</exception>
      CILField AddField( String name, CILTypeBase fieldType, FieldAttributes attr );

      /// <summary>
      /// Tries to remove a <see cref="CILField"/> from this <see cref="CILType"/>.
      /// </summary>
      /// <param name="field">The field to remove.</param>
      /// <returns><c>true</c> if <paramref name="field"/> was removed; <c>false</c> otherwise.</returns>
      /// <exception cref="NotSupportedException">When <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c> for this type, meaning this type is a generic type but not generic type definition.</exception>
      Boolean RemoveField( CILField field );

      /// <summary>
      /// Adds a new <see cref="CILConstructor"/> to this <see cref="CILType"/>.
      /// </summary>
      /// <param name="attrs">The <see cref="MethodAttributes"/> of the constructor being added.</param>
      /// <param name="callingConventions">The <see cref="CallingConventions"/> of the constructor being added.</param>
      /// <returns>A newly added <see cref="CILConstructor"/>.</returns>
      /// <exception cref="NotSupportedException">When <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c> for this type, meaning this type is a generic type but not generic type definition.</exception>
      CILConstructor AddConstructor( MethodAttributes attrs, CallingConventions callingConventions );

      /// <summary>
      /// Tries to remove a <see cref="CILConstructor"/> from this <see cref="CILType"/>.
      /// </summary>
      /// <param name="ctor">The constructor to remove.</param>
      /// <returns><c>true</c> if <paramref name="ctor"/> was removed; <c>false</c> otherwise.</returns>
      /// <exception cref="NotSupportedException">When <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c> for this type, meaning this type is a generic type but not generic type definition.</exception>
      Boolean RemoveConstructor( CILConstructor ctor );

      /// <summary>
      /// Adds a new <see cref="CILMethod"/> to this <see cref="CILType"/>.
      /// </summary>
      /// <param name="name">The name of the method being added.</param>
      /// <param name="attrs">The <see cref="MethodAttributes"/> of the method being added.</param>
      /// <param name="callingConventions">The <see cref="CallingConventions"/> of the method being added.</param>
      /// <returns>A newly added <see cref="CILMethod"/>.</returns>
      /// <exception cref="NotSupportedException">When <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c> for this type, meaning this type is a generic type but not generic type definition.</exception>
      CILMethod AddMethod( String name, MethodAttributes attrs, CallingConventions callingConventions );

      /// <summary>
      /// Tries to remove a <see cref="CILMethod"/> from this <see cref="CILType"/>.
      /// </summary>
      /// <param name="method">The method to remove.</param>
      /// <returns><c>true</c> if <paramref name="method"/> was removed; <c>false</c> otherwise.</returns>
      /// <exception cref="NotSupportedException">When <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c> for this type, meaning this type is a generic type but not generic type definition.</exception>
      Boolean RemoveMethod( CILMethod method );

      /// <summary>
      /// Adds a new <see cref="CILProperty"/> to this <see cref="CILType"/>.
      /// </summary>
      /// <param name="name">The name of the property being added.</param>
      /// <param name="attrs">The <see cref="PropertyAttributes"/> of the property being added.</param>
      /// <returns>A newly added <see cref="CILProperty"/>.</returns>
      /// <exception cref="NotSupportedException">When <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c> for this type, meaning this type is a generic type but not generic type definition.</exception>
      CILProperty AddProperty( String name, PropertyAttributes attrs );

      /// <summary>
      /// Tries to remove a <see cref="CILProperty"/> from this <see cref="CILType"/>.
      /// </summary>
      /// <param name="property">The property to remove.</param>
      /// <returns><c>true</c> if <paramref name="property"/> was removed; <c>false</c> otherwise.</returns>
      /// <exception cref="NotSupportedException">When <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c> for this type, meaning this type is a generic type but not generic type definition.</exception>
      Boolean RemoveProperty( CILProperty property );

      /// <summary>
      /// Adds a new <see cref="CILEvent"/> to this <see cref="CILType"/>.
      /// </summary>
      /// <param name="name">The name of the event being added.</param>
      /// <param name="attrs">The <see cref="EventAttributes"/> of the event being added.</param>
      /// <param name="eventType">The <see cref="CILTypeBase">type</see> of the event being added.</param>
      /// <returns>A newly added <see cref="CILEvent"/>.</returns>
      /// <exception cref="NotSupportedException">When <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c> for this type, meaning this type is a generic type but not generic type definition.</exception>
      CILEvent AddEvent( String name, EventAttributes attrs, CILTypeBase eventType );

      /// <summary>
      /// Tries to remove a <see cref="CILEvent"/> from this <see cref="CILType"/>.
      /// </summary>
      /// <param name="evt">The event to remove.</param>
      /// <returns><c>true</c> if <paramref name="evt"/> was removed; <c>false</c> otherwise.</returns>
      Boolean RemoveEvent( CILEvent evt );

   }

   /// <summary>
   /// This object contains information about non-vector arrays.
   /// </summary>
   public sealed class GeneralArrayInfo
   {
      private readonly Int32 _rank;
      private readonly ArrayProxy<Int32> _sizes;
      private readonly ArrayProxy<Int32> _lowerBounds;

      internal GeneralArrayInfo( Int32 rank, Int32[] sizes, Int32[] lowerBounds )
      {
         if ( rank < 1 )
         {
            throw new ArgumentOutOfRangeException( "Rank must be at least one, but was " + rank + "." );
         }

         this._rank = rank;
         this._sizes = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( sizes );
         this._lowerBounds = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( lowerBounds );
      }

      internal GeneralArrayInfo( GeneralArrayInfo other )
         : this( other._rank, CopyArray( other._sizes ), CopyArray( other._lowerBounds ) )
      {
      }

      private static Int32[] CopyArray( ArrayProxy<Int32> array )
      {
         Int32[] result;
         if ( array.Array.Length > 0 )
         {
            result = new Int32[array.Array.Length];
            Array.Copy( array.Array, result, result.Length );
         }
         else
         {
            result = null;
         }
         return result;
      }

      /// <summary>
      /// Gets the rank of array.
      /// </summary>
      /// <value>The rank of array.</value>
      public Int32 Rank
      {
         get
         {
            return this._rank;
         }
      }

      /// <summary>
      /// Gets the sizes of array dimensions.
      /// </summary>
      /// <value>The sizes of array dimensions.</value>
      public ArrayQuery<Int32> Sizes
      {
         get
         {
            return this._sizes.CQ;
         }
      }

      /// <summary>
      /// Gets the lower bounds of array dimensions.
      /// </summary>
      /// <value>The lower bounds of array dimensions.</value>
      public ArrayQuery<Int32> LowerBounds
      {
         get
         {
            return this._lowerBounds.CQ;
         }
      }

      /// <inheritdoc />
      public override Boolean Equals( Object obj )
      {
         return this.Equals( obj as GeneralArrayInfo );
      }

      /// <inheritdoc />
      public override Int32 GetHashCode()
      {
         // Highest 6 bits is rank, lowest 26 bits are array hash codes
         return ( this._rank << 26 ) | ( ( ArrayEqualityComparer<Int32>.DefaultArrayEqualityComparer.GetHashCode( this._sizes.Array ) ^ ArrayEqualityComparer<Int32>.DefaultArrayEqualityComparer.GetHashCode( this._lowerBounds.Array ) ) >> 6 );
      }

      private Boolean Equals( GeneralArrayInfo other )
      {
         return Object.ReferenceEquals( this, other )
            || ( other != null
               && this._rank == other._rank
               && ArrayEqualityComparer<Int32>.DefaultArrayEqualityComparer.Equals( this._sizes.Array, other._sizes.Array )
               && ArrayEqualityComparer<Int32>.DefaultArrayEqualityComparer.Equals( this._lowerBounds.Array, other._lowerBounds.Array )
               );
      }
   }

   /// <summary>
   /// This struct contains information about a layout of a <see cref="CILType"/>.
   /// </summary>
   /// <seealso cref="CILType.Layout"/>
   public struct ClassLayout
   {
      /// <summary>
      /// Contains the <c>pack</c> size for a type. See ECMA specification for more information about pack size (ClassLayout table).
      /// </summary>
      public Int32 pack;
      /// <summary>
      /// Contains the <c>size</c> of a type. See ECMA specification for more information about size (ClassLayout table).
      /// </summary>
      public Int32 size;
   }

   /// <summary>
   /// This interface represents information about generic type parameters. Generic type parameters will never be instances of <see cref="CILType"/>. The interface is logically equivalent to parts of <see cref="System.Type"/>.
   /// </summary>
   public interface CILTypeParameter :
      CILTypeOrTypeParameter,
      CILElementWithAttributes<GenericParameterAttributes>
   {
      /// <summary>
      /// Gets the declaring method of this <see cref="CILTypeParameter"/>, if this type parameter is defined in a method. Otherwise, returns <c>null</c>.
      /// </summary>
      /// <value>the declaring method of this <see cref="CILTypeParameter"/>, if this type parameter is defined in a method. Otherwise, returns <c>null</c>.</value>
      /// <seealso cref="System.Type.DeclaringMethod"/>
      CILMethod DeclaringMethod { get; }

      /// <summary>
      /// Gets the position of the type parameter in the type parameter list of the generic type or method that declared the parameter.
      /// </summary>
      /// <value>The position of the type parameter in the type parameter list of the generic type or method that declared the parameter.</value>
      /// <seealso cref="System.Type.GenericParameterPosition"/>
      Int32 GenericParameterPosition { get; }

      /// <summary>
      /// Adds generic parameter constraints to this <see cref="CILTypeParameter"/>.
      /// </summary>
      /// <param name="constraints">The constraints to add.</param>
      void AddGenericParameterConstraints( params CILTypeBase[] constraints );

      /// <summary>
      /// Tries to remove given generic parameter constraint from this <see cref="CILTypeParameter"/>.
      /// </summary>
      /// <param name="constraint">The constraint to remove.</param>
      /// <returns><c>true</c> if the <paramref name="constraint"/> was removed; <c>false</c> otherwise.</returns>
      Boolean RemoveGenericParameterConstraint( CILTypeBase constraint );

      /// <summary>
      /// Gets all the generic parameter constraints of this <see cref="CILTypeParameter"/>.
      /// </summary>
      /// <value>All the generic parameter constraints of this <see cref="CILTypeParameter"/>.</value>
      ListQuery<CILTypeBase> GenericParameterConstraints { get; }
   }

   /// <summary>
   /// Helps identify whether any <see cref="CILTypeBase"/> is <see cref="CILType"/> or <see cref="CILTypeParameter"/>.
   /// </summary>
   /// <seealso cref="CILTypeBase.TypeKind"/>
   public enum TypeKind
   {
      /// <summary>
      /// The <see cref="CILTypeBase"/> is <see cref="CILType"/>.
      /// </summary>
      Type,

      /// <summary>
      /// The <see cref="CILTypeBase"/> is <see cref="CILTypeParameter"/>.
      /// </summary>
      TypeParameter,

      /// <summary>
      /// The <see cref="CILTypeBase"/> is <see cref="CILMethodSignature"/>.
      /// </summary>
      MethodSignature
   }

   /// <summary>
   /// Helps identify what kind of element type any <see cref="CILType"/> is.
   /// </summary>
   /// <seealso cref="CILType.ElementKind"/>
   /// <remarks>All values of this enum are non-zero in order to allow easy comparing of <see cref="CILType.ElementKind"/>.</remarks>
   public enum ElementKind
   {
      /// <summary>
      /// The <see cref="CILType"/> is an array type.
      /// </summary>
      Array = 1,

      /// <summary>
      /// The <see cref="CILType"/> is a pointer type.
      /// </summary>
      Pointer,

      /// <summary>
      /// The <see cref="CILType"/> is a reference type.
      /// </summary>
      Reference
   }
}

public static partial class E_CIL
{
   internal const Int32 VECTOR_ARRAY_RANK = 0;

   /// <summary>
   /// Returns a <see cref="CILTypeBase"/> representing array, pointer, or by-ref type.
   /// </summary>
   /// <param name="type">The type to make element type from.</param>
   /// <param name="kind">What kind of element type to make.</param>
   /// <param name="arrayInfo">The array information, if <paramref name="kind"/> is <see cref="ElementKind.Array"/>. Otherwise it is ignored. In array information which is <c>null</c> means a simple vector array, otherwise the resulting type will be general array.</param>
   /// <returns>A <see cref="CILTypeBase"/> representing array, pointer, or by-ref type.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="type"/> is <c>null.</c></exception>
   /// <exception cref="ArgumentOutOfRangeException">If <paramref name="kind"/> is <see cref="ElementKind.Array"/> and <paramref name="arrayInfo"/> is less than <c>0</c>.</exception>
   public static CILType MakeElementType( this CILTypeBase type, ElementKind kind, GeneralArrayInfo arrayInfo )
   {
      return ( (CILReflectionContextImpl) type.ReflectionContext ).Cache.MakeElementType( type, kind, arrayInfo );
   }

   /// <summary>
   /// Gets or creates a new <see cref="CILTypeBase"/> based on native <see cref="System.Type"/>.
   /// </summary>
   /// <param name="type">The native type.</param>
   /// <param name="ctx">The current reflection context.</param>
   /// <returns><see cref="CILTypeBase"/> wrapping existing native <see cref="System.Type"/>. Will be either <see cref="CILType"/> or <see cref="CILTypeParameter"/>, depending on <paramref name="type"/></returns>
   /// <exception cref="NullReferenceException">If <paramref name="type"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="ctx"/> is <c>null</c>.</exception>
   public static CILTypeBase NewWrapper( this Type type, CILReflectionContext ctx )
   {
      ArgumentValidator.ValidateNotNull( "Reflection context", ctx );

      return ( (CILReflectionContextImpl) ctx ).Cache.GetOrAdd( type );
   }

   /// <summary>
   /// Checks whether <paramref name="type"/> is a generic type definition. Behaves in a same way as <see cref="System.Type.IsGenericTypeDefinition"/> property.
   /// </summary>
   /// <param name="type">The <see cref="CILTypeBase"/> to check.</param>
   /// <returns><c>true</c> if <paramref name="type"/> is non-<c>null</c>, is <see cref="CILType"/> and is generic type definition; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Type.IsGenericTypeDefinition"/>
   public static Boolean IsGenericTypeDefinition( this CILTypeBase type )
   {
      return type != null && TypeKind.Type == type.TypeKind && Object.ReferenceEquals( type, ( (CILType) type ).GenericDefinition );
   }

   /// <summary>
   /// Checks whether <paramref name="type"/> is a generic type. Behaves in a same way as <see cref="System.Type.IsGenericType"/> property.
   /// </summary>
   /// <param name="type">The <see cref="CILTypeBase"/> to check.</param>
   /// <returns><c>true</c> if <paramref name="type"/> is non-<c>null</c>, is <see cref="CILType"/> and is generic type; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Type.IsGenericType"/>
   public static Boolean IsGenericType( this CILTypeBase type )
   {
      return type != null && TypeKind.Type == type.TypeKind && ( (CILType) type ).GenericArguments.Any();
   }

   /// <summary>
   /// Makes a vector array type which has <paramref name="type"/> as its element type.
   /// </summary>
   /// <param name="type">The type of the elements of the returned array type.</param>
   /// <returns>A vector array type which <paramref name="type"/> as its element type.</returns>
   /// <remarks>(Copied from MSDN) The common language runtime makes a distinction between vectors (that is, one-dimensional arrays that are always zero-based) and multidimensional arrays. A vector, which always has only one dimension, is not the same as a multidimensional array that happens to have only one dimension. This method overload can only be used to create vector types, and it is the only way to create a vector type. Use the <see cref="MakeArrayType(CILTypeBase, Int32, Int32[], Int32[])"/> method overload to create non-vector array types.</remarks>
   /// <exception cref="NullReferenceException">If <paramref name="type"/> is <c>null</c>.</exception>
   /// <seealso cref="System.Type.MakeArrayType()"/>
   public static CILType MakeArrayType( this CILTypeBase type )
   {
      return type.MakeElementType( ElementKind.Array, null );
   }

   /// <summary>
   /// Makes a multi-dimensional array type which has <paramref name="type"/> as its element type and <paramref name="rank"/> amount of dimensions.
   /// </summary>
   /// <param name="type">The type of the elements of the returned array type.</param>
   /// <param name="rank">The rank of the array.</param>
   /// <param name="sizes">Optional parameter specifying the sizes of array dimensions.</param>
   /// <param name="lowerBounds">Optional parameter specifying the lower indexing bounds of array dimensions.</param>
   /// <returns>A multi-dimensional array type which has <paramref name="type"/> as its element type.</returns>
   /// <remarks>(Copied from MSDN) The common language runtime makes a distinction between vectors (that is, one-dimensional arrays that are always zero-based) and multidimensional arrays. A vector, which always has only one dimension, is not the same as a multidimensional array that happens to have only one dimension. You cannot use this method overload to create a vector type; if rank is 1, this method overload returns a multidimensional array type that happens to have one dimension. Use the <see cref="MakeArrayType(CILTypeBase)" /> method overload to create vector types.</remarks>
   /// <exception cref="NullReferenceException">If <paramref name="type"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentOutOfRangeException">If rank is less than <c>1</c>.</exception>
   /// <seealso cref="System.Type.MakeArrayType(Int32)"/>
   public static CILType MakeArrayType( this CILTypeBase type, Int32 rank, Int32[] sizes = null, Int32[] lowerBounds = null )
   {
      return type.MakeElementType( ElementKind.Array, new GeneralArrayInfo( rank, sizes, lowerBounds ) );
   }

   /// <summary>
   /// Makes a pointer type which has <paramref name="type"/> as its element type.
   /// </summary>
   /// <param name="type">The type of the returned pointer type.</param>
   /// <returns>A pointer type which has <paramref name="type"/> as its element type.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="type"/> is <c>null</c>.</exception>
   /// <exception cref="InvalidOperationException">If <paramref name="type"/> is reference type.</exception>
   /// <seealso cref="System.Type.MakePointerType()"/>
   public static CILType MakePointerType( this CILTypeBase type )
   {
      return type.MakeElementType( ElementKind.Pointer, null );
   }

   /// <summary>
   /// Makes a reference type which has <paramref name="type"/> as its element type.
   /// </summary>
   /// <param name="type">The type of the returned reference type.</param>
   /// <returns>A reference type which has <paramref name="type"/> as its element type.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="type"/> is <c>null</c>.</exception>
   /// <exception cref="InvalidOperationException">If <paramref name="type"/> is reference type.</exception>
   /// <seealso cref="System.Type.MakeByRefType()"/>
   public static CILType MakeByRefType( this CILTypeBase type )
   {
      return type.MakeElementType( ElementKind.Reference, null );
   }

   /// <summary>
   /// Checks whether <paramref name="type"/> is non-<c>null</c> and either is generic type parameter or has any open type parameters.
   /// </summary>
   /// <param name="type">The type to check.</param>
   /// <returns><c>true</c> if <paramref name="type"/> is non-<c>null</c> and either is generic type parameter or has any open type parameters; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Type.ContainsGenericParameters"/>
   public static Boolean ContainsGenericParameters( this CILTypeBase type )
   {
      return type != null
      && ( TypeKind.TypeParameter == type.TypeKind
         || ( TypeKind.Type == type.TypeKind && ( ( (CILType) type ).GenericArguments.Any( gArg => ContainsGenericParameters( gArg ) ) || ( ContainsGenericParameters( ( (CILType) type ).ElementType ) ) ) )
         || ( TypeKind.MethodSignature == type.TypeKind && ( (CILMethodSignature) type ).ContainsGenericParameters() )
         );
   }

   /// <summary>
   /// Checks whether both <paramref name="baseType"/> and <paramref name="subType"/> are non-<c>null</c>, and <paramref name="baseType"/> is assignable from <paramref name="subType"/>.
   /// </summary>
   /// <param name="baseType">The suspected base type.</param>
   /// <param name="subType">The suspected sub type.</param>
   /// <returns><c>true</c> if a <paramref name="baseType"/> and <paramref name="subType"/> are non-<c>null</c>, and <paramref name="baseType"/> is assignable from <paramref name="subType"/>; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Type.IsAssignableFrom(Type)"/>
   public static Boolean IsAssignableFrom( this CILTypeBase baseType, CILTypeBase subType )
   {
      return ( Object.ReferenceEquals( baseType, subType ) && ( baseType == null || TypeKind.MethodSignature != baseType.TypeKind ) )
         || ( baseType != null && subType != null && TypeKind.MethodSignature != baseType.TypeKind && TypeKind.MethodSignature != subType.TypeKind &&
               ( TypeKind.TypeParameter == baseType.TypeKind ?
                  ( (CILTypeParameter) baseType ).GenericParameterConstraints.All( constraint => constraint.IsAssignableFrom( subType ) )
                  : ( ( (CILType) subType ).IsSubclassOf( baseType ) || ( (CILType) subType ).AsBreadthFirstEnumerable( type => type.DeclaredInterfaces ).Any( iFace => Object.Equals( baseType, iFace ) ) )
               )
            );
   }

   /// <summary>
   /// Convenience method to get <see cref="CILType.ElementType"/> from <see cref="CILTypeBase"/> typed variable.
   /// </summary>
   /// <param name="type">The <see cref="CILTypeBase"/> to get <see cref="ElementKind"/> from.</param>
   /// <returns><c>null</c> if <paramref name="type"/> is <see cref="CILTypeParameter"/>; otherwise the value of <see cref="CILType.ElementType"/> (which also may be <c>null</c>).</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>.</exception>
   /// <seealso cref="CILType.ElementType"/>
   /// <seealso cref="System.Type.GetElementType()"/>
   public static CILTypeBase GetElementType( this CILTypeBase type )
   {
      ArgumentValidator.ValidateNotNull( "Type", type );
      return TypeKind.Type == type.TypeKind ? ( (CILType) type ).ElementType : null;
   }

   /// <summary>
   /// Checks whether given <paramref name="type"/> is non-<c>null</c> a value type.
   /// </summary>
   /// <param name="type">The type to check.</param>
   /// <returns><c>true</c> if <paramref name="type"/> is non-<c>null</c> and <see cref="CILType"/> and has a base type which has <see cref="CILTypeCode.Value"/> or <see cref="CILTypeCode.Enum"/> as its <see cref="CILTypeOrTypeParameter.TypeCode"/> property value.</returns>
   /// <seealso cref="System.Type.IsValueType"/>
   public static Boolean IsValueType( this CILTypeBase type )
   {
      return type != null && TypeKind.Type == type.TypeKind && ( (CILType) type ).BaseType != null && ( ( (CILType) type ).BaseType.TypeCode == CILTypeCode.Value || ( (CILType) type ).BaseType.TypeCode == CILTypeCode.Enum ); // ( (CILType) type ).BaseTypeChain().Any( bt => CILTypeCode.Value == bt.TypeCode );
   }

   /// <summary>
   /// Checks whether given <paramref name="type"/> is non-<c>null</c> and an enum type.
   /// </summary>
   /// <param name="type">The type to check,</param>
   /// <returns><c>true</c> if <paramref name="type"/> is non-<c>null</c> and <see cref="CILType"/> and has a base type which has <see cref="CILTypeCode.Enum"/> as its <see cref="CILTypeOrTypeParameter.TypeCode"/> property value.</returns>
   /// <seealso cref="System.Type.IsEnum"/>
   public static Boolean IsEnum( this CILTypeBase type )
   {
      return type != null && TypeKind.Type == type.TypeKind && ( (CILType) type ).BaseType != null && ( (CILType) type ).BaseType.TypeCode == CILTypeCode.Enum;// .BaseTypeChain().Any( bt => CILTypeCode.Enum == bt.TypeCode );
   }

   /// <summary>
   /// Checks whether given <paramref name="type"/> non-<c>null</c> and is generic type parameter.
   /// </summary>
   /// <param name="type">The type to check.</param>
   /// <returns><c>true</c> if <paramref name="type"/> is non-<c>null</c> and <see cref="CILTypeParameter"/>.</returns>
   /// <seealso cref="System.Type.IsGenericParameter"/>
   public static Boolean IsGenericParameter( this CILTypeBase type )
   {
      return type != null && TypeKind.TypeParameter == type.TypeKind;
   }

   /// <summary>
   /// Checks whether given <paramref name="type"/> is non-<c>null</c> and is either array, reference, or pointer type.
   /// </summary>
   /// <param name="type">The type to check.</param>
   /// <returns><c>true</c> if <paramref name="type"/> is non-<c>null</c> and is either array, reference, or pointer type; <c>false</c> otherwise.</returns>
   public static Boolean IsAnyElementType( this CILTypeBase type )
   {
      return type != null && TypeKind.Type == type.TypeKind && ( (CILType) type ).ElementKind.HasValue;
   }

   /// <summary>
   /// Checks whether given <paramref name="type"/> is non-<c>null</c> and is reference type.
   /// </summary>
   /// <param name="type">The type to check.</param>
   /// <returns><c>true</c> if <paramref name="type"/> is non-<c>null</c> and is reference type; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Type.IsByRef"/>
   public static Boolean IsByRef( this CILTypeBase type )
   {
      return type != null && TypeKind.Type == type.TypeKind && ElementKind.Reference == ( (CILType) type ).ElementKind;
   }

   /// <summary>
   /// Checks whether given <paramref name="type"/> is non-<c>null</c> and is pointer type.
   /// </summary>
   /// <param name="type">The type to check.</param>
   /// <returns><c>true</c> if <paramref name="type"/> is non-<c>null</c> and is pointer type; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Type.IsPointer"/>
   public static Boolean IsPointerType( this CILTypeBase type )
   {
      return type != null && TypeKind.Type == type.TypeKind && ElementKind.Pointer == ( (CILType) type ).ElementKind;
   }

   /// <summary>
   /// Checks whether given <paramref name="type"/> is non-<c>null</c> and is array type.
   /// </summary>
   /// <param name="type">The type to check.</param>
   /// <returns><c>true</c> if <paramref name="type"/> is non-<c>null</c> and is array type; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Type.IsArray"/>
   public static Boolean IsArray( this CILTypeBase type )
   {
      return type != null && TypeKind.Type == type.TypeKind && ElementKind.Array == ( (CILType) type ).ElementKind;
   }

   /// <summary>
   /// Checks whether given <paramref name="type"/> is non-<c>null</c> and is class.
   /// </summary>
   /// <param name="type">The type to check.</param>
   /// <returns><c>true</c> if <paramref name="type"/> is non-<c>null</c> and is class; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Type.IsClass"/>
   /// <seealso cref="E_CIL.IsClass(TypeAttributes)"/>
   public static Boolean IsClass( this CILTypeBase type )
   {
      return type != null && TypeKind.Type == type.TypeKind && ( (CILType) type ).Attributes.IsClass();
   }

   /// <summary>
   /// Checks whether given <paramref name="type"/> is non-<c>null</c> and is interface.
   /// </summary>
   /// <param name="type">The type to check.</param>
   /// <returns><c>true</c> if <paramref name="type"/> is non-<c>null</c> and is interface; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Type.IsInterface"/>
   /// <seealso cref="E_CIL.IsInterface(TypeAttributes)"/>
   public static Boolean IsInterface( this CILTypeBase type )
   {
      return type != null && TypeKind.Type == type.TypeKind && ( (CILType) type ).Attributes.IsInterface();
   }

   /// <summary>
   /// Checks whether given <paramref name="type"/> is non-<c>null</c> and is an instance of <see cref="System.Nullable{T}"/>.
   /// </summary>
   /// <param name="type">The type to check.</param>
   /// <param name="nullableType">If this method returns <c>true</c>, then this will hold the type argument of the <see cref="System.Nullable{T}"/>. Otherwise this will be <c>null</c>.</param>
   /// <returns><c>true</c> if <paramref name="type"/> is non-<c>null</c> and is an instance of <see cref="System.Nullable{T}"/>; <c>false</c> otherwise.</returns>
   /// <remarks>TODO: In certain special scenarios, such as emitting a new mscorlib, this might not work as expected.</remarks>
   public static Boolean IsNullable( this CILTypeBase type, out CILTypeBase nullableType )
   {
      return IsGDefGiven( type, out nullableType, type.Module.AssociatedMSCorLibModule.GetTypeByName( Consts.NULLABLE ) );
   }

   /// <summary>
   /// Checks whether given <paramref name="type"/> is non-<c>null</c> and is an instance of <see cref="System.Lazy{T}"/>.
   /// </summary>
   /// <param name="type">The type to check.</param>
   /// <param name="lazyType">If this method returns <c>true</c>, then this will hold the type argument of the <see cref="System.Lazy{T}"/>. Otherwise this will be <c>null</c>.</param>
   /// <returns><c>true</c> if <paramref name="type"/> is non-<c>null</c> and is an instance of <see cref="System.Lazy{T}"/>; <c>false</c> otherwise.</returns>
   /// <remarks>TODO: In certain special scenarios, such as emitting a new mscorlib, this might not work as expected.</remarks>
   public static Boolean IsLazy( this CILTypeBase type, out CILTypeBase lazyType )
   {
      return IsGDefGiven( type, out lazyType, type.Module.AssociatedMSCorLibModule.GetTypeByName( Consts.LAZY ) );
   }

   private static Boolean IsGDefGiven( CILTypeBase thisType, out CILTypeBase gArgType, CILType type )
   {
      var result = thisType != null
         && TypeKind.Type == thisType.TypeKind
         && Object.Equals( type, ( (CILType) thisType ).GenericDefinition );
      gArgType = result ? ( (CILType) thisType ).GenericArguments[0] : null;
      return result;
   }

   /// <summary>
   /// Convenience method to get <see cref="CILType.ElementKind"/> from <see cref="CILTypeBase"/> typed variable.
   /// </summary>
   /// <param name="type">The <see cref="CILTypeBase"/> to get <see cref="ElementKind"/> from.</param>
   /// <returns><c>null</c> if <paramref name="type"/> is <see cref="CILTypeParameter"/>; otherwise the value of <see cref="CILType.ElementKind"/> (which also may be <c>null</c>).</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>.</exception>
   /// <seealso cref="CILType.ElementKind"/>
   public static ElementKind? GetElementKind( this CILTypeBase type )
   {
      return TypeKind.Type == type.TypeKind ? ( (CILType) type ).ElementKind : null;
   }

   /// <summary>
   /// Gets the first non-static field of the <paramref name="type"/>. If <paramref name="type"/> is an enum type, this field will be the value field of the enum. The resulting field may be used eg. to deduce enum underlying type.
   /// </summary>
   /// <param name="type">The type to check.</param>
   /// <returns>The first non-static field of the <paramref name="type"/>, or <c>null</c> if no such field exist.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>.</exception>
   public static CILField GetEnumValueField( this CILType type )
   {
      ArgumentValidator.ValidateNotNull( "Type", type );
      return type.DeclaredFields.FirstOrDefault( field => !field.Attributes.IsStatic() );
   }

   /// <summary>
   /// Gets all fields contained in <paramref name="type"/> that are static and literal. If <paramref name="type"/> is an enum type, these fields will contain the values of the enum.
   /// </summary>
   /// <param name="type">The type to check.</param>
   /// <returns>All fields contained in <paramref name="type"/> that are static and literal.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>.</exception>
   public static IEnumerable<CILField> GetEnumValues( this CILType type )
   {
      ArgumentValidator.ValidateNotNull( "Type", type );
      return type.DeclaredFields.Where( field => field.Attributes.IsStatic() && field.Attributes.IsLiteral() );
   }

   /// <summary>
   /// Helper method to get <see cref="CILTypeCode"/> of <see cref="CILTypeBase"/> and returning given <see cref="CILTypeCode"/> if <paramref name="type"/> is of <see cref="TypeKind.MethodSignature"/>.
   /// </summary>
   /// <param name="type">The <see cref="CILTypeBase"/>.</param>
   /// <param name="methodSigTC">The <see cref="CILTypeCode"/> to return if <paramref name="type"/> is <see cref="TypeKind.MethodSignature"/>.</param>
   /// <returns>The <see cref="TypeCode"/> of <paramref name="type"/> or <paramref name="methodSigTC"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="type"/> is <c>null</c>.</exception>
   public static CILTypeCode GetTypeCode( this CILTypeBase type, CILTypeCode methodSigTC = CILTypeCode.Int32 )
   {
      return TypeKind.MethodSignature == type.TypeKind ? methodSigTC : ( (CILTypeOrTypeParameter) type ).TypeCode;
   }

   private static CILType[] EMPTY_TYPES = new CILType[0];

   /// <summary>
   /// Gets or creates a new <see cref="CILType"/> based on native <see cref="System.Type"/>.
   /// </summary>
   /// <param name="nType">The native type.</param>
   /// <param name="ctx">The current reflection context.</param>
   /// <returns><see cref="CILType"/> wrapping existing native <see cref="System.Type"/>.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="nType"/> or <paramref name="ctx"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If <see cref="Type.IsGenericParameter"/> returns <c>true</c> for <paramref name="nType"/>.</exception>
   public static CILType NewWrapperAsType( this Type nType, CILReflectionContext ctx )
   {
      if ( nType.IsGenericParameter )
      {
         throw new ArgumentException( "Type " + nType + " can not be generic parameter." );
      }
      return (CILType) nType.NewWrapper( ctx );
   }

   /// <summary>
   /// Checks whether <paramref name="type"/> is a generic type. Behaves in a same way as <see cref="System.Type.IsGenericType"/> property.
   /// </summary>
   /// <param name="type">The <see cref="CILType"/> to check.</param>
   /// <returns><c>true</c> if <paramref name="type"/> is non-<c>null</c> and is generic type; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Type.IsGenericType"/>
   public static Boolean IsGenericType( this CILType type )
   {
      return type != null && type.GenericArguments.Any();
   }

   /// <summary>
   /// Checks whether <paramref name="type"/> is a generic type definition. Behaves in a same way as <see cref="System.Type.IsGenericTypeDefinition"/> property.
   /// </summary>
   /// <param name="type">The <see cref="CILType"/> to check.</param>
   /// <returns><c>true</c> if <paramref name="type"/> is non-<c>null</c> and is generic type definition; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Type.IsGenericTypeDefinition"/>
   public static Boolean IsGenericTypeDefinition( this CILType type )
   {
      return type != null && Object.ReferenceEquals( type, type.GenericDefinition );
   }

   /// <summary>
   /// Checks whether both <paramref name="thisType"/> and <paramref name="baseType"/> are non-<c>null</c>, and whether the base type hierarchy of <paramref name="thisType"/> contains <paramref name="baseType"/>.
   /// </summary>
   /// <param name="thisType">The suspected sub-class.</param>
   /// <param name="baseType">The suspected parent class.</param>
   /// <returns><c>true</c> if <paramref name="thisType"/> and <paramref name="baseType"/> are non-<c>null</c> and <paramref name="baseType"/> contains <paramref name="thisType"/> in its base type hierarchy; <c>false</c> otherwise.</returns>
   public static Boolean IsSubclassOf( this CILType thisType, CILTypeBase baseType )
   {
      var result = false;
      if ( thisType != null
         && thisType.Attributes.IsClass()
         && baseType is CILType
         && ( (CILType) baseType ).Attributes.IsClass() )
      {
         var curType = thisType.BaseType;
         while ( !result && curType != null )
         {
            result = Object.Equals( baseType, curType );
            if ( !result )
            {
               curType = curType.BaseType;
            }
         }
      }
      return result;
   }

   /// <summary>
   /// Returns the enumerable for full inheritance chain of <paramref name="type"/>. This means all the base types and all implemented interfaces.
   /// </summary>
   /// <param name="type">The type to extract full inheritance chain from.</param>
   /// <returns>Enumerable for full inheritance chain of <paramref name="type"/>. Will be empty if <paramref name="type"/> is <c>null</c>.</returns>
   public static IEnumerable<CILType> GetFullInheritanceChain( this CILType type )
   {
      return type == null ? EMPTY_TYPES : type.GetBaseTypeChain().Concat( type.GetAllImplementedInterfaces( false ) );
   }

   /// <summary>
   /// Returns the enumerable for all implemented interfaces of <paramref name="type"/>. If <paramref name="includeThisIfInterface"/> is <c>true</c> and <paramref name="type"/> is an interface, then it is included into the result as well.
   /// </summary>
   /// <param name="type">The type to extract all interfaces from.</param>
   /// <param name="includeThisIfInterface">Whether to include <paramref name="type"/> if it is an interface into the result.</param>
   /// <returns>The enumerable for all implemented interfaces of <paramref name="type"/>. Will be empty if <paramref name="type"/> is <c>null</c>.</returns>
   /// <seealso cref="System.Type.GetInterfaces()"/>
   public static IEnumerable<CILType> GetAllImplementedInterfaces( this CILType type, Boolean includeThisIfInterface )
   {
      var retVal = type == null ? EMPTY_TYPES : ( type.Attributes.IsInterface() ?
         type.AsDepthFirstEnumerable( t => t.DeclaredInterfaces ) :
         type.GetBaseTypeChain().SelectMany( t => t.AsDepthFirstEnumerable( t2 => t2.DeclaredInterfaces, false ) ) );
      if ( type != null && type.Attributes.IsInterface() && !includeThisIfInterface )
      {
         retVal = retVal.Skip( 1 );
      }

      return retVal.Distinct();
   }

   /// <summary>
   /// Returns the enumerable for base type inheritance hierarchy of <paramref name="type"/>.
   /// </summary>
   /// <param name="type">The type to extract base type inheritance hierarchy from.</param>
   /// <param name="includeThisType">Whether to include <paramref name="type"/> in the resulting enumerable.</param>
   /// <returns>The enumerable for base type inheritance hierarchy of <paramref name="type"/>. Will be empty if <paramref name="type"/> is <c>null</c>.</returns>
   public static IEnumerable<CILType> GetBaseTypeChain( this CILType type, Boolean includeThisType = true )
   {
      var retVal = type.AsSingleBranchEnumerable( t => t.BaseType );
      if ( !includeThisType )
      {
         retVal = retVal.Skip( 1 );
      }
      return retVal;
   }

   /// <summary>
   /// Adds a parameterless constructor to <paramref name="type"/>, which calls the parameterless constructor of the <see cref="CILType.BaseType"/> property of the <paramref name="type"/>.
   /// </summary>
   /// <param name="type">The type to add constructor to.</param>
   /// <param name="attrs">The <see cref="MethodAttributes"/> of the constructor to add.</param>
   /// <returns>The newly created <see cref="CILConstructor"/>.</returns>
   /// <exception cref="InvalidOperationException">If base type of <paramref name="type"/> does not have a parameterless constructor.</exception>
   /// <remarks>If <see cref="CILType.BaseType"/> property of <paramref name="type"/> is <c>null</c>, then the only instruction emitted to resulting IL of <see cref="CILConstructor"/> is <see cref="OpCodes.Ret"/>.</remarks>
   public static CILConstructor AddDefaultConstructor( this CILType type, MethodAttributes attrs )
   {
      var result = type.AddConstructor( attrs, CallingConventions.Standard );
      var il = result.MethodIL;
      if ( type.BaseType != null )
      {
         var ctor = type.BaseType.Constructors.FirstOrDefault( c => c.Parameters.Count == 0 );
         if ( ctor == null )
         {
            throw new InvalidOperationException( "Could not find parameterless constructor from " + type + "." );
         }

         il.EmitLoadArg( 0 )
            .EmitCall( ctor );
      }
      il.EmitReturn();
      return result;
   }

   /// <summary>
   /// Given an enumerable of <see cref="CILType"/>s, returns enumerable for only the types which do not appear in any other types full inheritance hierarchy.
   /// </summary>
   /// <param name="types">The type to filter.</param>
   /// <returns>Enumerable for only the types which do not appear in any other types full inheritance hierarchy.</returns>
   public static IEnumerable<CILType> OnlyBottomTypes( this IEnumerable<CILType> types )
   {
      return types.Where( type => !types.Any( anotherType => !type.Equals( anotherType ) && type.IsAssignableFrom( anotherType ) ) ).Distinct().ToArray();
   }

   /// <summary>
   /// Sets the parent type for the <paramref name="type"/>. If the <paramref name="type"/> is an interface, it adds <paramref name="parentType"/> to its list of declared interfaces. Otherwise, it sets <paramref name="parentType"/> as <paramref name="type"/>'s base type.
   /// </summary>
   /// <param name="type">The type to add parent type to.</param>
   /// <param name="parentType">The parent type to add.</param>
   /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>.</exception>
   public static void SetParentType( this CILType type, CILType parentType )
   {
      ArgumentValidator.ValidateNotNull( "Type", type );
      if ( parentType.Attributes.IsInterface() )
      {
         if ( parentType != null )
         {
            type.AddDeclaredInterfaces( parentType );
         }
      }
      else
      {
         type.BaseType = parentType;
      }
   }

   /// <summary>
   /// Substitutes the generic arguments of the given <paramref name="type"/> with <paramref name="args"/> and returns the resulting type.
   /// </summary>
   /// <param name="type">The type to substitute generic arguments.</param>
   /// <param name="args">The generic arguments.</param>
   /// <returns>The generic type with <paramref name="args"/> as generic arguments.</returns>
   /// <exception cref="InvalidOperationException">The <paramref name="args"/> are non-<c>null</c> or contain at least one element, and the <paramref name="type"/> is not generic type definition.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>, or if <paramref name="type"/> is generic type definition and <paramref name="args"/> is <c>null</c> or any element of <paramref name="args"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If number of elements in <paramref name="args"/> is not the same as number of generic arguments in <paramref name="type"/></exception>
   /// <remarks>TODO the void-type-check for generic arguments might not properly in some special scenarios, such as emitting mscorlib.</remarks>
   public static CILType MakeGenericType( this CILType type, params CILTypeBase[] args )
   {
      ArgumentValidator.ValidateNotNull( "Type", type );
      return ( (CILReflectionContextImpl) type.ReflectionContext ).Cache.MakeGenericType( type, type.GenericDefinition, args );
   }

   /// <summary>
   /// Convenience method for checking whether <paramref name="type"/> is non-<c>null</c> and is vector array type. The array type is considered to be vector array type when its <see cref="CILType.ElementKind"/> is <see cref="ElementKind.Array"/> and its <see cref="CILType.ArrayInformation"/> is <c>null</c>.
   /// </summary>
   /// <param name="type">The type to check.</param>
   /// <returns><c>true</c> if <paramref name="type"/> is non-<c>null</c> and is vector array type; <c>false</c> otherwise.</returns>
   public static Boolean IsVectorArray( this CILType type )
   {
      return type != null && type.ElementKind.Value == ElementKind.Array && type.ArrayInformation == null;
   }

   /// <summary>
   /// Convenience method for checking whether <paramref name="type"/> is non-<c>null</c> and is general array type. The array type is considered to be general array type when its <see cref="CILType.ElementKind"/> is <see cref="ElementKind.Array"/> and its <see cref="CILType.ArrayInformation"/> is not <c>null</c>.
   /// </summary>
   /// <param name="type">The type to check.</param>
   /// <returns><c>true</c> if <paramref name="type"/> is non-<c>null</c> and is general array type; <c>false</c> otherwise.</returns>
   public static Boolean IsMultiDimensionalArray( this CILType type )
   {
      return type != null && type.ElementKind.Value == ElementKind.Array && type.ArrayInformation != null;
   }

   /// <summary>
   /// Creates the full name of the <see cref="CILTypeBase"/>, similar to <see cref="Type.FullName"/> property.
   /// </summary>
   /// <param name="type">The <see cref="CILTypeBase"/>.</param>
   /// <returns>The full name of the <paramref name="type"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="type"/> is <c>null</c>.</exception>
   public static String GetFullName( this CILTypeOrTypeParameter type )
   {
      return TypeKind.Type == type.TypeKind ? Utils.CreateTypeString( (CILType) type, null, false ) : type.Name;
   }

   /// <summary>
   /// Creates assembly-qualified name of the <see cref="CILTypeBase"/>, similar to <see cref="Type.AssemblyQualifiedName"/> property.
   /// </summary>
   /// <param name="type">The <see cref="CILTypeBase"/>.</param>
   /// <returns>The assembly-qualified name of the <paramref name="type"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="type"/> is <c>null</c>.</exception>
   public static String GetAssemblyQualifiedName( this CILTypeOrTypeParameter type )
   {
      return GetFullName( type ) + ", " + type.Module.Assembly.Name.ToString();
   }
}

public static partial class E_CIL
{
   /// <summary>
   /// Gets or creates a new <see cref="CILTypeParameter"/> based on native <see cref="System.Type"/>.
   /// </summary>
   /// <param name="nType">The native type.</param>
   /// <param name="ctx">The current reflection context.</param>
   /// <returns><see cref="CILTypeParameter"/> wrapping existing native <see cref="System.Type"/>.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="nType"/> or <paramref name="ctx"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If <see cref="Type.IsGenericParameter"/> returns <c>false</c> for <paramref name="nType"/>.</exception>
   public static CILTypeParameter NewWrapperAsTypeParameter( this Type nType, CILReflectionContext ctx )
   {
      if ( !nType.IsGenericParameter )
      {
         throw new ArgumentException( "Type " + nType + " must be generic parameter." );
      }
      return (CILTypeParameter) nType.NewWrapper( ctx );
   }
}