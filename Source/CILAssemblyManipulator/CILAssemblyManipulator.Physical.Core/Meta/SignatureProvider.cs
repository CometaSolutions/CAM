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
using CollectionsWithRoles.API;
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
      /// Returns all <see cref="SignatureTableIndexInfo"/> objects related to single signature element.
      /// </summary>
      /// <param name="element">The <see cref="SignatureElement"/> to get table indices from.</param>
      /// <returns>All the <see cref="SignatureTableIndexInfo"/> objects related to single signature element.</returns>
      IEnumerable<SignatureTableIndexInfo> GetTableIndexInfoFromSignatureElement( SignatureElement element );

      /// <summary>
      /// Performs structural match on two signatures.
      /// </summary>
      /// <param name="firstMD">The <see cref="CILMetaData"/> containing the <paramref name="firstSignature"/>.</param>
      /// <param name="firstSignature">The first <see cref="AbstractSignature"/>.</param>
      /// <param name="secondMD">The <see cref="CILMetaData"/> containing the <paramref name="secondSignature"/>.</param>
      /// <param name="secondSignature">the second <see cref="AbstractSignature"/>.</param>
      /// <param name="matcher">The object capturing callbacks to perform non-structural compare.</param>
      /// <returns>Whether <paramref name="firstSignature"/> and <paramref name="secondSignature"/> match structurally and using the given <paramref name="matcher"/>.</returns>
      Boolean MatchSignatures( CILMetaData firstMD, AbstractSignature firstSignature, CILMetaData secondMD, AbstractSignature secondSignature, SignatureMatcher matcher );
   }

   /// <summary>
   /// This type encapsulates callbacks needed for comparing signatures using <see cref="SignatureProvider.MatchSignatures"/> method.
   /// </summary>
   public struct SignatureMatcher
   {
      /// <summary>
      /// Creates a new instance of <see cref="SignatureMatcher"/> with given parameters.
      /// </summary>
      /// <param name="typeDefOrRefMatcher">The value for <see cref="TypeDefOrRefMatcher"/>.</param>
      /// <param name="resolutionScopeMatcher">The value for <see cref="ResolutionScopeMatcher"/>.</param>
      /// <exception cref="ArgumentNullException">If either of <paramref name="typeDefOrRefMatcher"/> or <paramref name="resolutionScopeMatcher"/> is <c>null</c>.</exception>
      public SignatureMatcher( SignatureMatcherCallback<TableIndex> typeDefOrRefMatcher, SignatureMatcherCallback<TableIndex?> resolutionScopeMatcher )
      {
         this.TypeDefOrRefMatcher = ArgumentValidator.ValidateNotNull( "Type def or ref matcher", typeDefOrRefMatcher );
         this.ResolutionScopeMatcher = ArgumentValidator.ValidateNotNull( "Resolution scope matcher", resolutionScopeMatcher );
      }

      /// <summary>
      /// This callback will be called when one <see cref="TableIndex"/> is pointing to <see cref="Tables.TypeDef"/> table, and another <see cref="TableIndex"/> is pointing to <see cref="Tables.TypeRef"/>, or both are pointing to <see cref="Tables.TypeDef"/>.
      /// </summary>
      /// <value>The callback to compare <see cref="TableIndex"/>es pointing to <see cref="Tables.TypeDef"/> and <see cref="Tables.TypeRef"/> such that both are never <see cref="Tables.TypeRef"/>.</value>
      public SignatureMatcherCallback<TableIndex> TypeDefOrRefMatcher { get; }

      /// <summary>
      /// This callback will be called to match <see cref="TypeReference.ResolutionScope"/>s of top-level <see cref="TypeReference"/>s.
      /// </summary>
      /// <value>The callback to match <see cref="TypeReference.ResolutionScope"/>s of top-level <see cref="TypeReference"/>s.</value>
      public SignatureMatcherCallback<TableIndex?> ResolutionScopeMatcher { get; }
   }

   /// <summary>
   /// This delegate contains signature for callbacks used by <see cref="SignatureMatcher"/>.
   /// </summary>
   /// <typeparam name="TItem">The type of the first item index.</typeparam>
   /// <param name="firstMD">The <see cref="CILMetaData"/> passed as first to <see cref="SignatureProvider.MatchSignatures"/> method.</param>
   /// <param name="firstIndex">The item within the <see cref="AbstractSignature"/> passed as first to <see cref="SignatureProvider.MatchSignatures"/> method.</param>
   /// <param name="secondMD">The <see cref="CILMetaData"/> passed as second to <see cref="SignatureProvider.MatchSignatures"/> method.</param>
   /// <param name="secondIndex">The item within the <see cref="AbstractSignature"/> passed as second to <see cref="SignatureProvider.MatchSignatures"/> method.</param>
   /// <returns>Whether the <paramref name="firstIndex"/> and <paramref name="secondIndex"/> match.</returns>
   public delegate Boolean SignatureMatcherCallback<TItem>( CILMetaData firstMD, TItem firstIndex, CILMetaData secondMD, TItem secondIndex );

   internal delegate TTransform SignatureTransformCallbackDelegate<TTransform>( AbstractSignature signature );

   internal delegate TTransform SignatureTransformDelegate<TTransform>( AbstractSignature signature, SignatureTransformCallbackDelegate<TTransform> callback );

   internal delegate Boolean VisitElementDelegate<TElement>( TElement element, VisitElementCallbackDelegate<TElement> callback );

   internal delegate Boolean VisitElementDelegateTyped<TElement, in TActualElement>( TActualElement element, VisitElementCallbackDelegate<TElement> callback )
      where TActualElement : TElement;

   internal delegate Boolean VisitElementCallbackDelegate<in TElement>( TElement element, Type overrideType = null );

   internal delegate Boolean AcceptElementDelegate<in TElement>( TElement element );

   internal delegate Boolean AcceptElementDelegateWithContext<in TElement, in TContext>( TElement element, TContext context );

   internal class TypeBasedVisitor<TElement>
   {
      private struct Visitor
      {
         private readonly TypeBasedVisitor<TElement> _visitor;
         private readonly VisitElementCallbackDelegate<TElement> _callback;
         private readonly DictionaryQuery<Type, AcceptElementDelegate<TElement>> _acceptorDictionary;

         public Visitor( TypeBasedVisitor<TElement> visitor, DictionaryQuery<Type, AcceptElementDelegate<TElement>> acceptorDictionary )
         {
            this._visitor = visitor;
            this._acceptorDictionary = acceptorDictionary;
            this._callback = null;

            this._callback = this.Visit;
         }
         public Boolean Visit( TElement element, Type overrideType )
         {
            return this._visitor.VisitElementWithNoContext( element, this._acceptorDictionary, this._callback, overrideType );
         }


      }

      private struct Visitor<TContext>
      {
         private readonly TypeBasedVisitor<TElement> _visitor;
         private readonly VisitElementCallbackDelegate<TElement> _callback;
         private readonly DictionaryQuery<Type, AcceptElementDelegateWithContext<TElement, TContext>> _acceptorDictionary;
         private readonly TContext _context;

         public Visitor( TypeBasedVisitor<TElement> visitor, DictionaryQuery<Type, AcceptElementDelegateWithContext<TElement, TContext>> acceptorDictionary, TContext context )
         {
            this._visitor = visitor;
            this._acceptorDictionary = acceptorDictionary;
            this._context = context;
            this._callback = null;

            this._callback = this.Visit;
         }

         public Boolean Visit( TElement element, Type overrideType )
         {
            return this._visitor.VisitElementWithContext( element, this._acceptorDictionary, this._context, this._callback, overrideType );
         }
      }


      public TypeBasedVisitor()
      {
         this.VisitorDictionary = new Dictionary<Type, VisitElementDelegate<TElement>>();
      }

      private IDictionary<Type, VisitElementDelegate<TElement>> VisitorDictionary { get; }

      public void RegisterVisitor( Type elementType, VisitElementDelegate<TElement> visitor )
      {
         this.VisitorDictionary[elementType] = ArgumentValidator.ValidateNotNull( "Visitor", visitor );
      }

      public Boolean Visit(
         TElement element,
         DictionaryQuery<Type, AcceptElementDelegate<TElement>> acceptorDictionary
         )
      {
         var visitor = new Visitor( this, acceptorDictionary );
         return visitor.Visit( element, null );
      }

      public Boolean VisitWithContext<TContext>(
         TElement element,
         DictionaryQuery<Type, AcceptElementDelegateWithContext<TElement, TContext>> acceptorDictionary,
         TContext context
         )
      {
         var visitor = new Visitor<TContext>( this, acceptorDictionary, context );
         return visitor.Visit( element, null );
      }

      private Boolean VisitElementWithNoContext(
         TElement element,
         DictionaryQuery<Type, AcceptElementDelegate<TElement>> acceptorDictionary,
         VisitElementCallbackDelegate<TElement> callback,
         Type overrideType
         )
      {
         VisitElementDelegate<TElement> visitor;
         AcceptElementDelegate<TElement> acceptor;
         Boolean hadAcceptor;
         return element != null
            && ( ( acceptor = acceptorDictionary.TryGetValue( overrideType ?? element.GetType(), out hadAcceptor ) ) == null || acceptor( element ) )
            && this.VisitorDictionary.TryGetValue( overrideType ?? element.GetType(), out visitor )
            && visitor( element, callback );
      }

      private Boolean VisitElementWithContext<TContext>(
         TElement element,
         DictionaryQuery<Type, AcceptElementDelegateWithContext<TElement, TContext>> acceptorDictionary,
         TContext context,
         VisitElementCallbackDelegate<TElement> callback,
         Type overrideType
         )
      {
         VisitElementDelegate<TElement> visitor;
         AcceptElementDelegateWithContext<TElement, TContext> acceptor;
         Boolean hadAcceptor;
         return element != null
            && ( ( acceptor = acceptorDictionary.TryGetValue( overrideType ?? element.GetType(), out hadAcceptor ) ) == null || acceptor( element, context ) )
            && this.VisitorDictionary.TryGetValue( overrideType ?? element.GetType(), out visitor )
            && visitor( element, callback );
      }
   }

   internal abstract class AbstractTypeBasedAcceptor<TElement>
   {
      internal AbstractTypeBasedAcceptor( TypeBasedVisitor<TElement> visitor )
      {
         this.Visitor = ArgumentValidator.ValidateNotNull( "Visitor", visitor );

      }

      protected TypeBasedVisitor<TElement> Visitor { get; }

   }

   internal sealed class TypeBasedAcceptor<TElement> : AbstractTypeBasedAcceptor<TElement>
   {
      internal TypeBasedAcceptor( TypeBasedVisitor<TElement> visitor )
         : base( visitor )
      {
         this.AcceptorDictionary = new Dictionary<Type, AcceptElementDelegate<TElement>>().ToDictionaryProxy();
      }

      private DictionaryProxy<Type, AcceptElementDelegate<TElement>> AcceptorDictionary { get; }

      public void RegisterAcceptor( Type type, AcceptElementDelegate<TElement> acceptor )
      {
         this.AcceptorDictionary[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
      }

      public Boolean Accept( TElement element )
      {
         return this.Visitor.Visit( element, this.AcceptorDictionary.CQ );
      }
   }

   internal class TypeBasedAcceptor<TElement, TContext> : AbstractTypeBasedAcceptor<TElement>
   {
      internal TypeBasedAcceptor( TypeBasedVisitor<TElement> visitor )
         : base( visitor )
      {
         this.AcceptorDictionary = new Dictionary<Type, AcceptElementDelegateWithContext<TElement, TContext>>().ToDictionaryProxy();
      }

      private DictionaryProxy<Type, AcceptElementDelegateWithContext<TElement, TContext>> AcceptorDictionary { get; }

      public void RegisterAcceptor( Type type, AcceptElementDelegateWithContext<TElement, TContext> acceptor )
      {
         this.AcceptorDictionary[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
      }

      public Boolean AcceptWithContext( TElement element, TContext context )
      {
         return this.Visitor.VisitWithContext( element, this.AcceptorDictionary.CQ, context );
      }
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
      private class DecomposeSignatureContext
      {
         public DecomposeSignatureContext()
         {
            this.CurrentEnumerable = Empty<SignatureElement>.Enumerable;
         }

         public IEnumerable<SignatureElement> CurrentEnumerable { get; private set; }

         public Boolean AddElementsFromSignature( AbstractSignature signature, IEnumerable<SignatureElement> elements )
         {
            var cur = this.CurrentEnumerable.ConcatSingle( signature );
            if ( elements != null )
            {
               cur = cur.Concat( elements );
            }
            this.CurrentEnumerable = cur;

            return true;
         }
      }

      private IEnumerable<SignatureElement> DecomposeSignature_Visitor( AbstractSignature signature )
      {
         var decomposeContext = new DecomposeSignatureContext();
         var signatureVisitor = new TypeBasedVisitor<AbstractSignature>();

         signatureVisitor.RegisterVisitor( typeof( FieldSignature ), VisitSignatureCast<FieldSignature>( ( sig, cb ) => cb( sig.Type ) ) );
         signatureVisitor.RegisterVisitor( typeof( AbstractMethodSignature ), VisitSignatureCast<AbstractMethodSignature>( VisitSignature_AbstractMethod ) );
         signatureVisitor.RegisterVisitor( typeof( MethodDefinitionSignature ), VisitSignatureCast<MethodDefinitionSignature>( VisitSignature_MethodDef ) );
         signatureVisitor.RegisterVisitor( typeof( MethodReferenceSignature ), VisitSignatureCast<MethodReferenceSignature>( VisitSignature_MethodRef ) );
         signatureVisitor.RegisterVisitor( typeof( PropertySignature ), VisitSignatureCast<PropertySignature>( VisitSignature_Property ) );
         signatureVisitor.RegisterVisitor( typeof( LocalVariablesSignature ), VisitSignatureCast<LocalVariablesSignature>( VisitSignature_Locals ) );
         signatureVisitor.RegisterVisitor( typeof( GenericMethodSignature ), VisitSignatureCast<GenericMethodSignature>( VisitSignature_GenericMethod ) );
         signatureVisitor.RegisterVisitor( typeof( AbstractArrayTypeSignature ), VisitSignatureCast<AbstractArrayTypeSignature>( VisitTypeSignature_AbstractArray ) );
         signatureVisitor.RegisterVisitor( typeof( ComplexArrayTypeSignature ), VisitSignatureCast<ComplexArrayTypeSignature>( VisitTypeSignature_ComplexArray ) );
         signatureVisitor.RegisterVisitor( typeof( SimpleArrayTypeSignature ), VisitSignatureCast<SimpleArrayTypeSignature>( VisitTypeSignature_SimpleArray ) );
         signatureVisitor.RegisterVisitor( typeof( ClassOrValueTypeSignature ), VisitSignatureCast<ClassOrValueTypeSignature>( VisitTypeSignature_ClassOrValue ) );
         signatureVisitor.RegisterVisitor( typeof( FunctionPointerTypeSignature ), VisitSignatureCast<FunctionPointerTypeSignature>( VisitTypeSignature_FunctionPointer ) );
         signatureVisitor.RegisterVisitor( typeof( PointerTypeSignature ), VisitSignatureCast<PointerTypeSignature>( VisitTypeSignature_Pointer ) );

         var decomposer = new TypeBasedAcceptor<AbstractSignature, DecomposeSignatureContext>( signatureVisitor );
         decomposer.RegisterAcceptor( typeof( FieldSignature ), AcceptSignatureCast<FieldSignature, DecomposeSignatureContext>( ( field, ctx ) => ctx.AddElementsFromSignature( field, field.CustomModifiers ) ) );
         signatureVisitor.VisitWithContext(
            signature,
            new Dictionary<Type, AcceptElementDelegateWithContext<AbstractSignature, DecomposeSignatureContext>>()
            {
               { typeof(FieldSignature), (sig, ctx) => ctx.AddElementsFromSignature(sig, ( (FieldSignature) sig ).CustomModifiers) },
               { typeof(LocalVariablesSignature), (sig, ctx) => ctx.AddElementsFromSignature(sig, ( ( LocalVariablesSignature) sig).Locals.SelectMany( l => ((IEnumerable<SignatureElement>) l.CustomModifiers).PrependSingle( l ) ) ) }
               //{ typeof(AbstractMethodSignature), (sig, ctx) => ctx.AddElementsFromSignature(sig, ( ( AbstractMethodSignature) sig).ReturnType.CustomModifiers.PrependSingle( }
            }.ToDictionaryProxy().CQ,
            decomposeContext
            );
         return decomposeContext.CurrentEnumerable;
      }

      private static VisitElementDelegate<AbstractSignature> VisitSignatureCast<TSignature>( VisitElementDelegateTyped<AbstractSignature, TSignature> actual )
         where TSignature : AbstractSignature
      {
         return VisitElementCast( actual );
      }

      private static VisitElementDelegate<TElement> VisitElementCast<TElement, TActualElement>( VisitElementDelegateTyped<TElement, TActualElement> actual )
         where TActualElement : TElement
      {
         return ( element, cb ) => actual( (TActualElement) element, cb );
      }

      private static AcceptElementDelegateWithContext<AbstractSignature, TContext> AcceptSignatureCast<TSignature, TContext>( AcceptElementDelegateWithContext<TSignature, TContext> actual )
         where TSignature : AbstractSignature
      {
         return AcceptElementCast<AbstractSignature, TContext, TSignature>( actual );
      }

      private static AcceptElementDelegateWithContext<TElement, TContext> AcceptElementCast<TElement, TContext, TActualElement>( AcceptElementDelegateWithContext<TActualElement, TContext> actual )
         where TActualElement : TElement
      {
         return ( element, context ) => actual( (TActualElement) element, context );
      }


      private static Boolean VisitSignature_AbstractMethod( AbstractMethodSignature signature, VisitElementCallbackDelegate<AbstractSignature> callback )
      {
         return callback( signature.ReturnType.Type )
            && signature.Parameters.All( p => callback( p.Type ) );
      }

      private static Boolean VisitSignature_MethodDef( MethodDefinitionSignature signature, VisitElementCallbackDelegate<AbstractSignature> callback )
      {
         return callback( signature, typeof( AbstractMethodSignature ) );
      }

      private static Boolean VisitSignature_MethodRef( MethodReferenceSignature signature, VisitElementCallbackDelegate<AbstractSignature> callback )
      {
         return callback( signature, typeof( AbstractMethodSignature ) )
            && signature.VarArgsParameters.All( p => callback( p.Type ) );
      }

      private static Boolean VisitSignature_Property( PropertySignature signature, VisitElementCallbackDelegate<AbstractSignature> callback )
      {
         return callback( signature.PropertyType )
            && signature.Parameters.All( p => callback( p.Type ) );
      }

      private static Boolean VisitSignature_Locals( LocalVariablesSignature signature, VisitElementCallbackDelegate<AbstractSignature> callback )
      {
         return signature.Locals.All( l => callback( l.Type ) );
      }

      private static Boolean VisitSignature_GenericMethod( GenericMethodSignature signature, VisitElementCallbackDelegate<AbstractSignature> callback )
      {
         return signature.GenericArguments.All( g => callback( g ) );
      }

      private static Boolean VisitTypeSignature_AbstractArray( AbstractArrayTypeSignature signature, VisitElementCallbackDelegate<AbstractSignature> callback )
      {
         return callback( signature.ArrayType );
      }

      private static Boolean VisitTypeSignature_ComplexArray( ComplexArrayTypeSignature signature, VisitElementCallbackDelegate<AbstractSignature> callback )
      {
         return callback( signature, typeof( AbstractArrayTypeSignature ) );
      }

      private static Boolean VisitTypeSignature_SimpleArray( SimpleArrayTypeSignature signature, VisitElementCallbackDelegate<AbstractSignature> callback )
      {
         return callback( signature, typeof( AbstractArrayTypeSignature ) );
      }

      private static Boolean VisitTypeSignature_ClassOrValue( ClassOrValueTypeSignature signature, VisitElementCallbackDelegate<AbstractSignature> callback )
      {
         return signature.GenericArguments.All( g => callback( g ) );
      }

      private static Boolean VisitTypeSignature_FunctionPointer( FunctionPointerTypeSignature signature, VisitElementCallbackDelegate<AbstractSignature> callback )
      {
         return callback( signature.MethodSignature );
      }

      private static Boolean VisitTypeSignature_Pointer( PointerTypeSignature signature, VisitElementCallbackDelegate<AbstractSignature> callback )
      {
         return callback( signature.PointerType );
      }

      //private static TTransform ExecuteFunctionality<TTransform>( IDictionary<Type, SignatureTransformDelegate<TTransform>> dictionary, AbstractSignature signature )
      //{
      //   SignatureTransformDelegate<TTransform> transformDelegate;
      //   return dictionary.TryGetValue( signature.GetType(), out transformDelegate ) ?
      //      transformDelegate( signature, sig => ExecuteFunctionality( dictionary, sig ) ) :
      //      default( TTransform );
      //}

      //private static IEnumerable<SignatureElement> DecomposeSignature_Field( FieldSignature field, SignatureTransformCallbackDelegate<IEnumerable<SignatureElement>> callback )
      //{
      //   return field.CustomModifiers.Concat( callback( field.Type ) );
      //}

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

      /// <inheritdoc />
      public Boolean MatchSignatures( CILMetaData firstMD, AbstractSignature firstSignature, CILMetaData secondMD, AbstractSignature secondSignature, SignatureMatcher matcher )
      {
         var retVal = ( ( firstSignature == null ) == ( secondSignature == null ) );
         if ( retVal && firstSignature != null )
         {
            retVal = firstSignature.SignatureKind == secondSignature.SignatureKind
               || (
                  ( firstSignature.SignatureKind == SignatureKind.MethodDefinition || firstSignature.SignatureKind == SignatureKind.MethodReference )
                  &&
                  ( secondSignature.SignatureKind == SignatureKind.MethodDefinition || secondSignature.SignatureKind == SignatureKind.MethodReference )
                  );
            if ( retVal )
            {
               switch ( firstSignature.SignatureKind )
               {
                  case SignatureKind.Field:
                     retVal = this.MatchFieldSignatures( firstMD, (FieldSignature) firstSignature, secondMD, (FieldSignature) secondSignature, matcher );
                     break;
                  case SignatureKind.GenericMethodInstantiation:
                     retVal = this.MatchGenericMethodSignatures( firstMD, (GenericMethodSignature) firstSignature, secondMD, (GenericMethodSignature) secondSignature, matcher );
                     break;
                  case SignatureKind.LocalVariables:
                     retVal = this.MatchLocalVarsSignatures( firstMD, (LocalVariablesSignature) firstSignature, secondMD, (LocalVariablesSignature) secondSignature, matcher );
                     break;
                  case SignatureKind.MethodDefinition:
                     retVal = this.MatchAbstractMethodSigntures( firstMD, (AbstractMethodSignature) firstSignature, secondMD, (AbstractMethodSignature) secondSignature, matcher );
                     break;
                  case SignatureKind.MethodReference:
                     retVal = this.MatchAbstractMethodSigntures( firstMD, (AbstractMethodSignature) firstSignature, secondMD, (AbstractMethodSignature) secondSignature, matcher );
                     break;
                  case SignatureKind.Property:
                     retVal = this.MatchPropertySignatures( firstMD, (PropertySignature) firstSignature, secondMD, (PropertySignature) secondSignature, matcher );
                     break;
                  case SignatureKind.Type:
                     retVal = this.MatchTypeSignatures( firstMD, (TypeSignature) firstSignature, secondMD, (TypeSignature) secondSignature, matcher );
                     break;
                  case SignatureKind.Raw:
                     retVal = false;
                     break;
                  default:
                     // TODO
                     retVal = false;
                     break;
               }
            }
         }

         return retVal;
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

      private Boolean MatchFieldSignatures( CILMetaData firstMD, FieldSignature first, CILMetaData secondMD, FieldSignature second, SignatureMatcher matcher )
      {
         return this.MatchCustomModifiers( firstMD, first.CustomModifiers, secondMD, second.CustomModifiers, matcher )
            && this.MatchTypeSignatures( firstMD, first.Type, secondMD, second.Type, matcher );
      }

      private Boolean MatchGenericMethodSignatures( CILMetaData firstMD, GenericMethodSignature first, CILMetaData secondMD, GenericMethodSignature second, SignatureMatcher matcher )
      {
         return ListEqualityComparer<List<TypeSignature>, TypeSignature>.ListEquality( first.GenericArguments, second.GenericArguments, ( firstG, secondG ) => this.MatchTypeSignatures( firstMD, firstG, secondMD, secondG, matcher ) );
      }

      private Boolean MatchLocalVarsSignatures( CILMetaData firstMD, LocalVariablesSignature first, CILMetaData secondMD, LocalVariablesSignature second, SignatureMatcher matcher )
      {
         return ListEqualityComparer<List<LocalSignature>, LocalSignature>.ListEquality( first.Locals, second.Locals, ( firstL, secondL ) => firstL.IsPinned == secondL.IsPinned && this.MatchParameterSignatures( firstMD, firstL, secondMD, secondL, matcher ) );
      }

      private Boolean MatchAbstractMethodSigntures( CILMetaData defModule, AbstractMethodSignature methodDef, CILMetaData refModule, AbstractMethodSignature methodRef, SignatureMatcher matcher )
      {
         return methodDef.MethodSignatureInformation == methodRef.MethodSignatureInformation
            && ListEqualityComparer<List<ParameterSignature>, ParameterSignature>.ListEquality( methodDef.Parameters, methodRef.Parameters, ( pDef, pRef ) => this.MatchParameterSignatures( defModule, pDef, refModule, pRef, matcher ) )
            && this.MatchParameterSignatures( defModule, methodDef.ReturnType, refModule, methodRef.ReturnType, matcher );
      }

      private Boolean MatchParameterSignatures( CILMetaData defModule, ParameterOrLocalSignature paramDef, CILMetaData refModule, ParameterOrLocalSignature paramRef, SignatureMatcher matcher )
      {
         return paramDef.IsByRef == paramRef.IsByRef
            && this.MatchCustomModifiers( defModule, paramDef.CustomModifiers, refModule, paramRef.CustomModifiers, matcher )
            && this.MatchTypeSignatures( defModule, paramDef.Type, refModule, paramRef.Type, matcher );
      }

      private Boolean MatchPropertySignatures( CILMetaData firstMD, PropertySignature first, CILMetaData secondMD, PropertySignature second, SignatureMatcher matcher )
      {
         return first.HasThis == second.HasThis
            && this.MatchTypeSignatures( firstMD, first.PropertyType, secondMD, second.PropertyType, matcher )
            && ListEqualityComparer<List<ParameterSignature>, ParameterSignature>.ListEquality( first.Parameters, second.Parameters, ( firstP, secondP ) => this.MatchParameterSignatures( firstMD, firstP, secondMD, secondP, matcher ) )
            && this.MatchCustomModifiers( firstMD, first.CustomModifiers, secondMD, second.CustomModifiers, matcher );
      }

      private Boolean MatchCustomModifiers( CILMetaData defModule, List<CustomModifierSignature> cmDef, CILMetaData refModule, List<CustomModifierSignature> cmRef, SignatureMatcher matcher )
      {
         return ListEqualityComparer<List<CustomModifierSignature>, CustomModifierSignature>.ListEquality( cmDef, cmRef, ( cDef, cRef ) => this.MatchTypeDefOrRefOrSpec( defModule, cDef.CustomModifierType, refModule, cRef.CustomModifierType, matcher ) );
      }

      private Boolean MatchTypeSignatures( CILMetaData defModule, TypeSignature typeDef, CILMetaData refModule, TypeSignature typeRef, SignatureMatcher matcher )
      {
         var retVal = typeDef.TypeSignatureKind == typeRef.TypeSignatureKind;
         if ( retVal )
         {
            switch ( typeDef.TypeSignatureKind )
            {
               case TypeSignatureKind.ClassOrValue:
                  var classDef = (ClassOrValueTypeSignature) typeDef;
                  var classRef = (ClassOrValueTypeSignature) typeRef;
                  retVal = classDef.TypeReferenceKind == classRef.TypeReferenceKind
                     && this.MatchTypeDefOrRefOrSpec( defModule, classDef.Type, refModule, classRef.Type, matcher )
                     && ListEqualityComparer<List<TypeSignature>, TypeSignature>.ListEquality( classDef.GenericArguments, classRef.GenericArguments, ( gArgDef, gArgRef ) => this.MatchTypeSignatures( defModule, gArgDef, refModule, gArgRef, matcher ) );
                  break;
               case TypeSignatureKind.ComplexArray:
                  var arrayDef = (ComplexArrayTypeSignature) typeDef;
                  var arrayRef = (ComplexArrayTypeSignature) typeRef;
                  retVal = Comparers.ComplexArrayInfoEqualityComparer.Equals( arrayDef.ComplexArrayInfo, arrayRef.ComplexArrayInfo )
                     && this.MatchTypeSignatures( defModule, arrayDef.ArrayType, refModule, arrayRef.ArrayType, matcher );
                  break;
               case TypeSignatureKind.FunctionPointer:
                  retVal = this.MatchAbstractMethodSigntures( defModule, ( (FunctionPointerTypeSignature) typeDef ).MethodSignature, refModule, ( (FunctionPointerTypeSignature) typeRef ).MethodSignature, matcher );
                  break;
               case TypeSignatureKind.GenericParameter:
                  var gDef = (GenericParameterTypeSignature) typeDef;
                  var gRef = (GenericParameterTypeSignature) typeRef;
                  retVal = gDef.GenericParameterKind == gRef.GenericParameterKind
                     && gDef.GenericParameterIndex == gRef.GenericParameterIndex;
                  break;
               case TypeSignatureKind.Pointer:
                  var ptrDef = (PointerTypeSignature) typeDef;
                  var ptrRef = (PointerTypeSignature) typeRef;
                  retVal = this.MatchCustomModifiers( defModule, ptrDef.CustomModifiers, refModule, ptrRef.CustomModifiers, matcher )
                     && this.MatchTypeSignatures( defModule, ptrDef.PointerType, refModule, ptrRef.PointerType, matcher );
                  break;
               case TypeSignatureKind.Simple:
                  retVal = ( (SimpleTypeSignature) typeDef ).SimpleType == ( (SimpleTypeSignature) typeRef ).SimpleType;
                  break;
               case TypeSignatureKind.SimpleArray:
                  var szArrayDef = (SimpleArrayTypeSignature) typeDef;
                  var szArrayRef = (SimpleArrayTypeSignature) typeRef;
                  retVal = this.MatchCustomModifiers( defModule, szArrayDef.CustomModifiers, refModule, szArrayRef.CustomModifiers, matcher )
                     && this.MatchTypeSignatures( defModule, szArrayDef.ArrayType, refModule, szArrayRef.ArrayType, matcher );
                  break;
               default:
                  retVal = false;
                  break;
            }
         }

         return retVal;
      }

      private Boolean MatchTypeDefOrRefOrSpec( CILMetaData defModule, TableIndex defIdx, CILMetaData refModule, TableIndex refIdx, SignatureMatcher matcher )
      {
         switch ( defIdx.Table )
         {
            case Tables.TypeDef:
               return ( ReferenceEquals( defModule, refModule )
                  && refIdx.Table == Tables.TypeDef
                  && matcher.TypeDefOrRefMatcher( defModule, defIdx, refModule, refIdx )
                  ) || ( !ReferenceEquals( defModule, refModule )
                  && refIdx.Table == Tables.TypeRef
                  && matcher.TypeDefOrRefMatcher( defModule, defIdx, refModule, refIdx )
                  );
            case Tables.TypeRef:
               return ( !ReferenceEquals( defModule, refModule )
                  && refIdx.Table == Tables.TypeDef
                  && matcher.TypeDefOrRefMatcher( defModule, defIdx, refModule, refIdx )
                  ) || ( refIdx.Table == Tables.TypeRef
                  && this.MatchTypeRefs( defModule, defIdx.Index, refModule, refIdx.Index, matcher ) );
            case Tables.TypeSpec:
               return refIdx.Table == Tables.TypeSpec && this.MatchTypeSignatures( defModule, defModule.TypeSpecifications.TableContents[defIdx.Index].Signature, refModule, refModule.TypeSpecifications.TableContents[refIdx.Index].Signature, matcher );
            default:
               return false;
         }
      }

      private Boolean MatchTypeRefs( CILMetaData defModule, Int32 defIdx, CILMetaData refModule, Int32 refIdx, SignatureMatcher matcher )
      {
         var defTypeRef = defModule.TypeReferences.TableContents[defIdx];
         var refTypeRef = refModule.TypeReferences.TableContents[refIdx];
         var retVal = String.Equals( defTypeRef.Name, refTypeRef.Name )
            && String.Equals( defTypeRef.Namespace, refTypeRef.Namespace );
         if ( retVal )
         {
            var defResScopeNullable = defTypeRef.ResolutionScope;
            var refResScopeNullable = refTypeRef.ResolutionScope;
            retVal = ( defResScopeNullable.HasValue
               && refResScopeNullable.HasValue
               && defResScopeNullable.Value.Table == Tables.TypeRef
               && refResScopeNullable.Value.Table == Tables.TypeRef
               && this.MatchTypeRefs( defModule, defResScopeNullable.Value.Index, refModule, refResScopeNullable.Value.Index, matcher )
               ) || ( ( !defResScopeNullable.HasValue || defResScopeNullable.Value.Table != Tables.TypeRef )
               && ( !refResScopeNullable.HasValue || refResScopeNullable.Value.Table != Tables.TypeRef )
               && matcher.ResolutionScopeMatcher( defModule, defResScopeNullable, refModule, refResScopeNullable )
               );
         }

         return retVal;
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
