/*
 * Copyright 2016 Stanislav Muhametsin. All rights Reserved.
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
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.Meta;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TabularMetaData.Meta;

namespace CILAssemblyManipulator.Physical.Meta
{
   /// <summary>
   /// This interface captures some of the operations crucial for interacting with signatures in CAM.Physical framework.
   /// </summary>
   /// <remarks>
   /// Unless specifically desired, instead of directly implementing this interface, a <see cref="DefaultSignatureProvider"/> should be used direclty, or by subclassing.
   /// </remarks>
   public interface SignatureProvider
   {
      /// <summary>
      /// Gets an instance of <see cref="SimpleTypeSignature"/> for a given <see cref="SimpleTypeSignatureKind"/>, or returns <c>null</c> on failure.
      /// </summary>
      /// <param name="kind">The <see cref="SimpleTypeSignatureKind"/></param>
      /// <returns>An instance of <see cref="SimpleTypeSignature"/> for a given <paramref name="kind"/>, or <c>null</c> if the <paramref name="kind"/> was unrecognized.</returns>
      /// <seealso cref="SimpleTypeSignature"/>
      SimpleTypeSignature GetSimpleTypeSignatureOrNull( SimpleTypeSignatureKind kind );

      /// <summary>
      /// Gets an instance of <see cref="CustomAttributeArgumentTypeSimple"/> for a given <see cref="CustomAttributeArgumentTypeSimpleKind"/>, or returns <c>null</c> on failure.
      /// </summary>
      /// <param name="kind">The <see cref="CustomAttributeArgumentTypeSimpleKind"/></param>
      /// <returns>An instance of <see cref="CustomAttributeArgumentTypeSimple"/> for a given <paramref name="kind"/>, or <c>null</c> if the <paramref name="kind"/> was unrecognized.</returns>
      /// <seealso cref="CustomAttributeArgumentTypeSimple"/>
      CustomAttributeArgumentTypeSimple GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind kind );

      /// <summary>
      /// Returns enumerable of leaf signature elements of given <see cref="AbstractSignature"/>.
      /// </summary>
      /// <param name="signature">The <see cref="AbstractSignature"/> to decompose.</param>
      /// <returns>The recursive enumerable of all <see cref="SignatureElement"/>s of <paramref name="signature"/>. Will be empty if <paramref name="signature"/> is <c>null</c>.</returns>
      IEnumerable<SignatureElement> DecomposeSignature( AbstractSignature signature );

      /// <summary>
      /// Returns all <see cref="SignatureTableIndexInfo"/> objects related to single signature.
      /// </summary>
      /// <param name="element">The <see cref="SignatureElement"/> to get table indices from.</param>
      /// <returns></returns>
      IEnumerable<SignatureTableIndexInfo> GetTableIndexInfoFromSignatureElement( SignatureElement element );
   }

   /// <summary>
   /// This type represents information about a single <see cref="Physical.TableIndex"/> reference located in <see cref="SignatureElement"/>.
   /// </summary>
   public struct SignatureTableIndexInfo
   {
      /// <summary>
      /// Creates a new instance of <see cref="SignatureTableIndexInfo"/> with given parameters.
      /// </summary>
      /// <param name="tableIndex">The <see cref="Physical.TableIndex"/>.</param>
      /// <param name="setter">The callback to set a new table index for the owner of <paramref name="tableIndex"/>.</param>
      public SignatureTableIndexInfo( TableIndex tableIndex, Action<TableIndex> setter )
      {
         this.TableIndex = tableIndex;
         this.TableIndexSetter = ArgumentValidator.ValidateNotNull( "Setter", setter );
      }

      /// <summary>
      /// Gets the <see cref="Physical.TableIndex"/> that is referenced by the signature element.
      /// </summary>
      /// <value>The <see cref="Physical.TableIndex"/> that is referenced by the signature element.</value>
      public TableIndex TableIndex { get; }

      ///// <summary>
      ///// Gets the signature element that has the <see cref="TableIndex"/>.
      ///// </summary>
      ///// <value>The signature element that has the <see cref="TableIndex"/>.</value>
      ///// <remarks>
      ///// By default, this is either <see cref="ClassOrValueTypeSignature"/>, or <see cref="CustomModifierSignature"/>.
      ///// </remarks>
      //public SignatureElement Owner { get; }

      /// <summary>
      /// Gets the callback to set a new <see cref="Physical.TableIndex"/> value to the <see cref="SignatureElement"/>.
      /// </summary>
      /// <value>The callback to set a new <see cref="Physical.TableIndex"/> value to the <see cref="SignatureElement"/>.</value>
      public Action<TableIndex> TableIndexSetter { get; }
   }

   /// <summary>
   /// This class provides default implementation for <see cref="SignatureProvider"/>.
   /// It caches all the <see cref="SimpleTypeSignature"/>s based on their <see cref="SimpleTypeSignature.SimpleType"/>, and also caches all <see cref="CustomAttributeArgumentTypeSimple"/>s based on their <see cref="CustomAttributeArgumentTypeSimple.SimpleType"/>.
   /// </summary>
   public class DefaultSignatureProvider : SignatureProvider
   {
      /// <summary>
      /// This class contains information to implement <see cref="SignatureProvider.GetTableIndexInfoFromSignatureElement(SignatureElement)"/> for single signature element type.
      /// </summary>
      public class SignatureTableIndexInfoProvider
      {
         /// <summary>
         /// Creates a new instance of <see cref="SignatureTableIndexInfoProvider"/> with given parameters.
         /// </summary>
         /// <param name="signatureElementType">The type of signature element.</param>
         /// <param name="factory">The callback to create <see cref="SignatureTableIndexInfo"/>s from signature element.</param>
         /// <exception cref="ArgumentNullException">If <paramref name="signatureElementType"/> or <paramref name="factory"/> is <c>null</c>.</exception>
         /// <exception cref="ArgumentException">If <paramref name="signatureElementType"/> is generic type or it is not assignable from <see cref="SignatureElement"/>.</exception>
         public SignatureTableIndexInfoProvider( Type signatureElementType, Func<SignatureElement, IEnumerable<SignatureTableIndexInfo>> factory )
         {
            this.SignatureElementType = ArgumentValidator.ValidateNotNull( "Signature element type", signatureElementType );
            if ( this.SignatureElementType.GetGenericArguments().Length > 0 )
            {
               throw new ArgumentException( "Signature element type must not be generic type." );
            }
            if ( !typeof( SignatureElement ).IsAssignableFrom( signatureElementType ) )
            {
               throw new ArgumentException( "Signature element type must be sub-type of " + typeof( SignatureElement ) + "." );
            }

            this.Factory = ArgumentValidator.ValidateNotNull( "Factory", factory );
         }

         /// <summary>
         /// Gets the type of the signature element.
         /// </summary>
         /// <value>The type of the signature element.</value>
         public Type SignatureElementType { get; }

         /// <summary>
         /// Gets the factory callback to create enumerable of <see cref="SignatureTableIndexInfo"/>s from single <see cref="SignatureElement"/>.
         /// </summary>
         /// <value>The factory callback to create enumerable of <see cref="SignatureTableIndexInfo"/>s from single <see cref="SignatureElement"/>.</value>
         public Func<SignatureElement, IEnumerable<SignatureTableIndexInfo>> Factory { get; }
      }
      /// <summary>
      /// Gets the default instance of <see cref="DefaultSignatureProvider"/>.
      /// It has support for simple type signatures returned by <see cref="GetDefaultSimpleTypeSignatures"/> method, and custom attribute simple types returned by <see cref="GetDefaultSimpleCATypes"/> method.
      /// </summary>
      public static SignatureProvider DefaultInstance { get; }

      static DefaultSignatureProvider()
      {
         DefaultInstance = new DefaultSignatureProvider();
      }

      private readonly IDictionary<SimpleTypeSignatureKind, SimpleTypeSignature> _simpleTypeSignatures;
      private readonly IDictionary<CustomAttributeArgumentTypeSimpleKind, CustomAttributeArgumentTypeSimple> _simpleCATypes;
      private readonly IDictionary<Type, SignatureTableIndexInfoProvider> _signatureElementTableIndexProviders;

      /// <summary>
      /// Creates a new instance of <see cref="DefaultSignatureProvider"/> with given supported <see cref="SimpleTypeSignature"/>s and <see cref="CustomAttributeArgumentTypeSimple"/>s.
      /// </summary>
      /// <param name="simpleTypeSignatures">The supported <see cref="SimpleTypeSignature"/>. If <c>null</c>, the return value of <see cref="GetDefaultSimpleTypeSignatures"/> will be used.</param>
      /// <param name="simpleCATypes">The supported <see cref="CustomAttributeArgumentTypeSimple"/>. If <c>null</c>, the return value of <see cref="GetDefaultSimpleCATypes"/> will be used.</param>
      /// <param name="signatureInfoProviders">The <see cref="SignatureTableIndexInfoProvider"/> functionality. If <c>null</c>, the return value of <see cref="GetDefaultSignatureTableIndexInfoProviders"/> will be used.</param>
      public DefaultSignatureProvider(
         IEnumerable<SimpleTypeSignature> simpleTypeSignatures = null,
         IEnumerable<CustomAttributeArgumentTypeSimple> simpleCATypes = null,
         IEnumerable<SignatureTableIndexInfoProvider> signatureInfoProviders = null
         )
      {
         this._simpleTypeSignatures = ( simpleTypeSignatures ?? GetDefaultSimpleTypeSignatures() )
            .Where( s => s != null )
            .ToDictionary_Overwrite( s => s.SimpleType, s => s );
         this._simpleCATypes = ( simpleCATypes ?? GetDefaultSimpleCATypes() )
            .Where( s => s != null )
            .ToDictionary_Overwrite( s => s.SimpleType, s => s );

         this._signatureElementTableIndexProviders = ( signatureInfoProviders ?? GetDefaultSignatureTableIndexInfoProviders() )
            .Where( s => s != null )
            .ToDictionary_Overwrite( s => s.SignatureElementType, s => s );
      }

      /// <inheritdoc />
      public SimpleTypeSignature GetSimpleTypeSignatureOrNull( SimpleTypeSignatureKind kind )
      {
         SimpleTypeSignature sig;
         return this._simpleTypeSignatures.TryGetValue( kind, out sig ) ? sig : null;
      }

      /// <inheritdoc />
      public CustomAttributeArgumentTypeSimple GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind kind )
      {
         CustomAttributeArgumentTypeSimple retVal;
         return this._simpleCATypes.TryGetValue( kind, out retVal ) ? retVal : null;
      }

      /// <summary>
      /// Returns enumerable of leaf signature elements of given <see cref="AbstractSignature"/>.
      /// </summary>
      /// <param name="signature">The <see cref="AbstractSignature"/> to decompose.</param>
      /// <returns>The recursive enumerable of all <see cref="SignatureElement"/>s of <paramref name="signature"/>. Will be empty if <paramref name="signature"/> is <c>null</c>.</returns>
      public IEnumerable<SignatureElement> DecomposeSignature( AbstractSignature signature )
      {
         if ( signature != null )
         {
            yield return signature;
            IEnumerable<SignatureElement> subElements = null;
            switch ( signature.SignatureKind )
            {
               case SignatureKind.Field:
                  subElements = this.GetSignatureTableIndexInfos_Field( (FieldSignature) signature );
                  break;
               case SignatureKind.GenericMethodInstantiation:
                  subElements = this.GetSignatureTableIndexInfos_GenericMethod( (GenericMethodSignature) signature );
                  break;
               case SignatureKind.LocalVariables:
                  subElements = this.GetSignatureTableIndexInfos_Locals( (LocalVariablesSignature) signature );
                  break;
               case SignatureKind.MethodDefinition:
                  subElements = this.GetSignatureTableIndexInfos_MethodDef( (MethodDefinitionSignature) signature );
                  break;
               case SignatureKind.MethodReference:
                  subElements = this.GetSignatureTableIndexInfos_MethodRef( (MethodReferenceSignature) signature );
                  break;
               case SignatureKind.Property:
                  subElements = this.GetSignatureTableIndexInfos_Property( (PropertySignature) signature );
                  break;
               case SignatureKind.Type:
                  subElements = this.GetSignatureTableIndexInfos_Type( (TypeSignature) signature, false );
                  break;
               case SignatureKind.Raw:
                  break;
               default:
                  subElements = this.GetSignatureTableIndexInfos_Custom( signature );
                  break;
            }

            if ( subElements != null )
            {
               foreach ( var elem in subElements )
               {
                  yield return elem;
               }
            }
         }
      }

      /// <inheritdoc />
      public IEnumerable<SignatureTableIndexInfo> GetTableIndexInfoFromSignatureElement( SignatureElement element )
      {
         SignatureTableIndexInfoProvider provider;
         if ( element != null && this._signatureElementTableIndexProviders.TryGetValue( element.GetType(), out provider ) )
         {
            foreach ( var info in provider.Factory( element ) )
            {
               yield return info;
            }
         }
      }

      /// <summary>
      /// This is called by <see cref="DecomposeSignature"/> when the type of given <see cref="AbstractSignature"/> is not one of the defaults.
      /// </summary>
      /// <param name="signature">The given <see cref="AbstractSignature"/>.</param>
      /// <returns>This implementation always throws and never returns.</returns>
      /// <exception cref="ArgumentException">Always.</exception>
      protected virtual IEnumerable<SignatureElement> GetSignatureTableIndexInfos_Custom( AbstractSignature signature )
      {
         throw new ArgumentException( "Unrecognized signature kind: " + signature.SignatureKind + "." );
      }

      /// <summary>
      /// This is called by <see cref="GetSignatureTableIndexInfos_Type"/> when the type of given <see cref="TypeSignature"/> is not one of the defaults.
      /// </summary>
      /// <param name="signature">The given <see cref="TypeSignature"/>.</param>
      /// <returns>This implementation always throws and never returns.</returns>
      /// <exception cref="ArgumentException">Always.</exception>
      protected virtual IEnumerable<SignatureElement> GetSignatureTableIndexInfos_Type_Custom( TypeSignature signature )
      {
         throw new ArgumentException( "Unrecognized type signature kind: " + signature.TypeSignatureKind + "." );
      }

      private IEnumerable<SignatureElement> GetSignatureTableIndexInfos_Field( FieldSignature sig )
      {
         return sig.CustomModifiers.Concat( this.GetSignatureTableIndexInfos_Type( sig.Type ) );
      }

      private IEnumerable<SignatureElement> GetSignatureTableIndexInfos_GenericMethod( GenericMethodSignature sig )
      {
         return sig.GenericArguments.SelectMany( arg => this.GetSignatureTableIndexInfos_Type( arg ) );
      }

      private IEnumerable<SignatureElement> GetSignatureTableIndexInfos_Locals( LocalVariablesSignature sig )
      {
         return sig.Locals.SelectMany( l => this.GetSignatureTableIndexInfos_LocalOrSig( l ) );
      }

      private IEnumerable<SignatureElement> GetSignatureTableIndexInfos_AbstractMethod( AbstractMethodSignature sig )
      {
         return this.GetSignatureTableIndexInfos_LocalOrSig( sig.ReturnType )
            .Concat( sig.Parameters.SelectMany( p => this.GetSignatureTableIndexInfos_LocalOrSig( p ) ) );
      }

      private IEnumerable<SignatureElement> GetSignatureTableIndexInfos_LocalOrSig( ParameterOrLocalSignature sig )
      {
         return sig.CustomModifiers
            .Concat( this.GetSignatureTableIndexInfos_Type( sig.Type ) )
            .PrependSingle( sig );
      }

      private IEnumerable<SignatureElement> GetSignatureTableIndexInfos_MethodDef( MethodDefinitionSignature sig )
      {
         return this.GetSignatureTableIndexInfos_AbstractMethod( sig );
      }

      private IEnumerable<SignatureElement> GetSignatureTableIndexInfos_MethodRef( MethodReferenceSignature sig )
      {
         return this.GetSignatureTableIndexInfos_AbstractMethod( sig )
            .Concat( sig.VarArgsParameters.SelectMany( p => this.GetSignatureTableIndexInfos_LocalOrSig( p ) ) );
      }

      private IEnumerable<SignatureElement> GetSignatureTableIndexInfos_Property( PropertySignature sig )
      {
         return sig.CustomModifiers.Concat( sig.Parameters.SelectMany( p => this.GetSignatureTableIndexInfos_LocalOrSig( p ) ) )
            .Concat( this.GetSignatureTableIndexInfos_Type( sig.PropertyType ) );
      }

      private IEnumerable<SignatureElement> GetSignatureTableIndexInfos_Type( TypeSignature sig, Boolean returnSig = true )
      {
         if ( sig != null )
         {
            if ( returnSig )
            {
               yield return sig;
            }
            IEnumerable<SignatureElement> subElements = null;
            switch ( sig.TypeSignatureKind )
            {
               case TypeSignatureKind.ClassOrValue:
                  subElements = ( (ClassOrValueTypeSignature) sig ).GenericArguments.SelectMany( g => this.GetSignatureTableIndexInfos_Type( g ) );
                  break;
               case TypeSignatureKind.ComplexArray:
                  subElements = this.GetSignatureTableIndexInfos_Type( ( (ComplexArrayTypeSignature) sig ).ArrayType );
                  break;
               case TypeSignatureKind.FunctionPointer:
                  subElements = this.GetSignatureTableIndexInfos_MethodRef( ( (FunctionPointerTypeSignature) sig ).MethodSignature );
                  break;
               case TypeSignatureKind.Pointer:
                  var ptr = (PointerTypeSignature) sig;
                  subElements = ptr.CustomModifiers.Concat( this.GetSignatureTableIndexInfos_Type( ptr.PointerType ) );
                  break;
               case TypeSignatureKind.SimpleArray:
                  var arr = (SimpleArrayTypeSignature) sig;
                  subElements = arr.CustomModifiers.Concat( this.GetSignatureTableIndexInfos_Type( arr.ArrayType ) );
                  break;
               case TypeSignatureKind.GenericParameter:
               case TypeSignatureKind.Simple:
                  break;
               default:
                  subElements = this.GetSignatureTableIndexInfos_Type_Custom( sig );
                  break;
            }

            if ( subElements != null )
            {
               foreach ( var elem in subElements )
               {
                  yield return elem;
               }
            }
         }
      }

      /// <summary>
      /// Gets an instance of <see cref="SimpleTypeSignature"/> for every value of <see cref="SimpleTypeSignatureKind"/> enumeration.
      /// </summary>
      /// <returns>An enumerable to iterate instances of <see cref="SimpleTypeSignature"/>s for every value of <see cref="SimpleTypeSignatureKind"/> enumeration.</returns>
      public static IEnumerable<SimpleTypeSignature> GetDefaultSimpleTypeSignatures()
      {
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.Boolean );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.Char );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.I1 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.U1 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.I2 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.U2 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.I4 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.U4 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.I8 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.U8 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.R4 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.R8 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.I );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.U );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.Object );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.String );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.Void );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.TypedByRef );
      }

      /// <summary>
      /// Gets an instance of <see cref="CustomAttributeArgumentTypeSimple"/> for every value of <see cref="CustomAttributeArgumentTypeSimpleKind"/> enumeration.
      /// </summary>
      /// <returns>An enumerable to iterate instances of <see cref="CustomAttributeArgumentTypeSimple"/>s for every value of <see cref="CustomAttributeArgumentTypeSimpleKind"/> enumeration.</returns>

      public static IEnumerable<CustomAttributeArgumentTypeSimple> GetDefaultSimpleCATypes()
      {
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.Boolean );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.Char );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.I1 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.U1 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.I2 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.U2 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.I4 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.U4 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.I8 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.U8 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.R4 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.R8 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.String );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.Type );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.Object );
      }

      /// <summary>
      /// Returns <see cref="SignatureTableIndexInfoProvider"/> functionality for <see cref="ClassOrValueTypeSignature"/> and <see cref="CustomModifierSignature"/>.
      /// </summary>
      /// <returns>Default <see cref="SignatureTableIndexInfoProvider"/> functionality for <see cref="ClassOrValueTypeSignature"/> and <see cref="CustomModifierSignature"/>.</returns>
      public static IEnumerable<SignatureTableIndexInfoProvider> GetDefaultSignatureTableIndexInfoProviders()
      {
         yield return new SignatureTableIndexInfoProvider( typeof( ClassOrValueTypeSignature ), elem => GetTableIndexInfos( (ClassOrValueTypeSignature) elem ) );
         yield return new SignatureTableIndexInfoProvider( typeof( CustomModifierSignature ), elem => GetTableIndexInfos( (CustomModifierSignature) elem ) );
      }

      private static IEnumerable<SignatureTableIndexInfo> GetTableIndexInfos( ClassOrValueTypeSignature clazz )
      {
         yield return new SignatureTableIndexInfo( clazz.Type, tIdx => clazz.Type = tIdx );
      }

      private static IEnumerable<SignatureTableIndexInfo> GetTableIndexInfos( CustomModifierSignature custom )
      {
         yield return new SignatureTableIndexInfo( custom.CustomModifierType, tIdx => custom.CustomModifierType = tIdx );
      }

   }
}

public static partial class E_CILPhysical
{
   /// <summary>
   /// Gets simple type signature for given <see cref="SimpleTypeSignatureKind"/>, or throws an exception if no suitable signature found.
   /// </summary>
   /// <param name="sigProvider">The <see cref="SignatureProvider"/>.</param>
   /// <param name="kind">The <see cref="SimpleTypeSignatureKind"/></param>
   /// <returns>The <see cref="SimpleTypeSignature"/> for given <paramref name="kind"/></returns>
   /// <exception cref="NullReferenceException">If this <paramref name="sigProvider"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If no suitable <see cref="SimpleTypeSignature"/> is found.</exception>
   /// <seealso cref="SimpleTypeSignature"/>
   /// <seealso cref="SignatureProvider.GetSimpleTypeSignatureOrNull"/>
   public static SimpleTypeSignature GetSimpleTypeSignature( this SignatureProvider sigProvider, SimpleTypeSignatureKind kind )
   {
      var retVal = sigProvider.GetSimpleTypeSignatureOrNull( kind );
      if ( retVal == null )
      {
         throw new ArgumentException( "The type signature kind " + kind + " is not simple." );
      }
      return retVal;
   }

   /// <summary>
   /// Gets simple custom attribute type for given <see cref="CustomAttributeArgumentTypeSimpleKind"/>, or throws an exception if no suitable signature found.
   /// </summary>
   /// <param name="sigProvider">The <see cref="SignatureProvider"/>.</param>
   /// <param name="kind">The <see cref="CustomAttributeArgumentTypeSimpleKind"/></param>
   /// <returns>The <see cref="CustomAttributeArgumentTypeSimple"/> for given <paramref name="kind"/></returns>
   /// <exception cref="NullReferenceException">If this <paramref name="sigProvider"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If no suitable <see cref="CustomAttributeArgumentTypeSimple"/> is found.</exception>
   /// <seealso cref="CustomAttributeArgumentTypeSimple"/>
   /// <seealso cref="SignatureProvider.GetSimpleCATypeOrNull"/>
   public static CustomAttributeArgumentTypeSimple GetSimpleCAType( this SignatureProvider sigProvider, CustomAttributeArgumentTypeSimpleKind kind )
   {
      var retVal = sigProvider.GetSimpleCATypeOrNull( kind );
      if ( retVal == null )
      {
         throw new ArgumentException( "Unrecognized CA argument simple type kind: " + kind + "." );
      }
      return retVal;
   }

   /// <summary>
   /// Creates a new instance of <see cref="CustomAttributeArgumentType"/> based on a native type.
   /// </summary>
   /// <param name="sigProvider">The <see cref="SignatureProvider"/>.</param>
   /// <param name="type">The native type.</param>
   /// <param name="enumTypeStringFactory">The callback to get textual type string when type is <see cref="CustomAttributeValue_EnumReference"/>.</param>
   /// <returns>A new instance of <see cref="CustomAttributeArgumentType"/>, or <c>null</c> if <paramref name="type"/> is <c>null</c> or if the type could not be resolved.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="sigProvider"/> is <c>null</c></exception>
   public static CustomAttributeArgumentType ResolveCAArgumentTypeFromType( this SignatureProvider sigProvider, Type type, Func<Type, Boolean, String> enumTypeStringFactory )
   {
      return type == null ? null : sigProvider.ResolveCAArgumentTypeOrNull( type, enumTypeStringFactory );
   }

   /// <summary>
   /// Creates a new instance of <see cref="CustomAttributeArgumentType"/> based on a existing value.
   /// </summary>
   /// <param name="sigProvider">The <see cref="SignatureProvider"/>.</param>
   /// <param name="obj">The existing value.</param>
   /// <returns>A new instance of <see cref="CustomAttributeArgumentType"/>, or <c>null</c> if <paramref name="obj"/> is <c>null</c> or if the type could not be resolved.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="sigProvider"/> is <c>null</c></exception>
   public static CustomAttributeArgumentType ResolveCAArgumentTypeFromObject( this SignatureProvider sigProvider, Object obj )
   {
      return obj == null ? null : sigProvider.ResolveCAArgumentTypeOrNull( obj.GetType(), ( t, isWithinArray ) =>
      {
         String enumType = null;
         if ( isWithinArray )
         {
            var array = (Array) obj;
            if ( array.Length > 0 )
            {
               var elem = array.GetValue( 0 );
               if ( elem is CustomAttributeValue_EnumReference )
               {
                  enumType = ( (CustomAttributeValue_EnumReference) elem ).EnumType;
               }
            }
         }
         else
         {
            enumType = ( (CustomAttributeValue_EnumReference) obj ).EnumType;
         }
         return enumType;
      } );
   }

   private static CustomAttributeArgumentType ResolveCAArgumentTypeOrNull(
      this SignatureProvider sigProvider,
      Type argType,
      Func<Type, Boolean, String> enumTypeStringFactory,
      Boolean isWithinArray = false
      )
   {
      if ( argType.IsEnum )
      {
         return new CustomAttributeArgumentTypeEnum()
         {
            TypeString = argType.AssemblyQualifiedName
         };
      }
      else
      {
         switch ( Type.GetTypeCode( argType ) )
         {
            case TypeCode.Boolean:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.Boolean );
            case TypeCode.Char:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.Char );
            case TypeCode.SByte:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.I1 );
            case TypeCode.Byte:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.U1 );
            case TypeCode.Int16:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.I2 );
            case TypeCode.UInt16:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.U2 );
            case TypeCode.Int32:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.I4 );
            case TypeCode.UInt32:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.U4 );
            case TypeCode.Int64:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.I8 );
            case TypeCode.UInt64:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.U8 );
            case TypeCode.Single:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.R4 );
            case TypeCode.Double:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.R8 );
            case TypeCode.String:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.String );
            case TypeCode.Object:
               if ( argType.IsArray )
               {
                  if ( isWithinArray )
                  {
                     return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.Object );
                  }
                  else
                  {
                     var arrayType = sigProvider.ResolveCAArgumentTypeOrNull( argType.GetElementType(), enumTypeStringFactory, true );
                     return arrayType == null ?
                        null :
                        new CustomAttributeArgumentTypeArray()
                        {
                           ArrayType = arrayType
                        };
                  }
               }
               else
               {
                  // Check for enum reference
                  if ( Equals( typeof( CustomAttributeValue_EnumReference ), argType ) )
                  {
                     var enumTypeStr = enumTypeStringFactory( argType, isWithinArray );
                     return enumTypeStr == null ? null : new CustomAttributeArgumentTypeEnum()
                     {
                        TypeString = enumTypeStr
                     };
                  }
                  // System.Type or System.Object or CustomAttributeTypeReference
                  else if ( Equals( typeof( CustomAttributeValue_TypeReference ), argType ) || Equals( typeof( Type ), argType ) )
                  {
                     return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.Type );
                  }
                  else if ( isWithinArray && Equals( typeof( Object ), argType ) )
                  {
                     return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.Object );
                  }
                  else
                  {
                     return null;
                     //throw new InvalidOperationException( "Failed to deduce custom attribute type for " + argType + "." );
                  }
               }
            default:
               return null;
               //throw new InvalidOperationException( "Failed to deduce custom attribute type for " + argType + "." );
         }
      }
   }

   /// <summary>
   /// Gets or creates a new <see cref="SignatureProvider"/>.
   /// </summary>
   /// <param name="provider">The <see cref="MetaDataTableInformationProvider"/>.</param>
   /// <returns>A <see cref="SignatureProvider"/> supported by this <see cref="MetaDataTableInformationProvider"/>.</returns>
   /// <seealso cref="SignatureProvider"/>
   public static SignatureProvider CreateSignatureProvider( this MetaDataTableInformationProvider provider )
   {
      return provider.GetFunctionality<SignatureProvider>();
   }

   /// <summary>
   /// Extracts all <see cref="SignatureTableIndexInfo"/> related to a single signature.
   /// </summary>
   /// <param name="provider">The <see cref="SignatureProvider"/>.</param>
   /// <param name="signature">The <see cref="AbstractSignature"/>.</param>
   /// <returns>A list of all <see cref="SignatureTableIndexInfo"/> related to a single signature. Will be empty if <paramref name="signature"/> is <c>null</c>.</returns>
   public static IEnumerable<SignatureTableIndexInfo> GetSignatureTableIndexInfos( this SignatureProvider provider, AbstractSignature signature )
   {
      return provider.DecomposeSignature( signature ).SelectMany( elem => provider.GetTableIndexInfoFromSignatureElement( elem ) );
   }
}
