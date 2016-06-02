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
   public interface SignatureProvider : SelfDescribingExtensionByCompositionProvider<Object> // TODO document which functionalities are available via SelfDescribingExtensionByCompositionProvider<TFunctionality>
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

   /// <summary>
   /// This delegate contains signature required for callbacks which perform visiting of hierarchically contained items according to visitor-pattern.
   /// </summary>
   /// <typeparam name="TElement">The common type for elements.</typeparam>
   /// <param name="element">The current element.</param>
   /// <param name="callback">The callback to call for all other elements hierarchically contained by given <paramref name="element"/>.</param>
   /// <returns><c>true</c> if should continue visiting; <c>false</c> otherwise.</returns>
   public delegate Boolean VisitElementDelegate<TElement>( TElement element, VisitElementCallbackDelegate<TElement> callback );

   /// <summary>
   /// This is helper delegate for callbacks which want automatic casting to be performed.
   /// </summary>
   /// <typeparam name="TElement">The common type for elements.</typeparam>
   /// <typeparam name="TActualElement">The casted type for elements..</typeparam>
   /// <param name="element">The current element.</param>
   /// <param name="callback">The callback to call for all other elements hierarchically contained by given <paramref name="element"/>.</param>
   /// <returns><c>true</c> if should continue visiting; <c>false</c> otherwise.</returns>
   /// <seealso cref="E_CILPhysical.AsVisitElementDelegate"/>
   public delegate Boolean VisitElementDelegateTyped<TElement, in TActualElement>( TActualElement element, VisitElementCallbackDelegate<TElement> callback )
      where TActualElement : TElement;

   /// <summary>
   /// This is delegate that should be called by methods called by <see cref="VisitElementDelegate{TElement}"/>. It will take care to visit the given element according to the visitor pattern.
   /// </summary>
   /// <typeparam name="TElement">The common type for elements.</typeparam>
   /// <param name="element">The element that should be visited.</param>
   /// <param name="edgeType">The information about type containing edge.</param>
   /// <param name="edgeName">The name of the edge.</param>
   /// <param name="edgeInfo">The edge-specific info object (e.g. list index).</param>
   /// <param name="overrideType">If parameter is supplied, then this type is used to lookup the visitor functionality for given <paramref name="element"/>, instead of <see cref="Object.GetType"/>.</param>
   /// <returns><c>true</c> if should continue visiting; <c>false</c> otherwise.</returns>
   /// <seealso cref="TypeBasedVisitor{TElement}"/>
   public delegate Boolean VisitElementCallbackDelegate<in TElement>( TElement element, Type edgeType, String edgeName, Object edgeInfo, Type overrideType = null );

   internal delegate Boolean AcceptElementDelegate<in TElement>( TElement element );

   internal delegate Boolean AcceptEdgeDelegate<in TElement>( TElement element, Object edgeInfo );

   /// <summary>
   /// This is delegate that captures signature to 'accept', or visit, a single element in visitor pattern.
   /// </summary>
   /// <typeparam name="TElement">The common type for elements.</typeparam>
   /// <typeparam name="TContext">The type of context that is given when performing visiting.</typeparam>
   /// <param name="element">The current element.</param>
   /// <param name="context">The current context.</param>
   /// <returns><c>true</c> if visiting should be continued, <c>false</c> otherwise.</returns>
   /// <remarks>
   /// The methods implementing this should be agnostic to how the hierarchical structure is explored.
   /// This is done by <see cref="VisitElementDelegate{TElement}"/>.
   /// The <see cref="AcceptElementDelegateWithContext{TElement, TContext}"/> should only capture the functionality that is done for element.
   /// In other words, <see cref="VisitElementDelegate{TElement}"/> captures how the hierarchical structure is explored, and <see cref="AcceptElementDelegateWithContext{TElement, TContext}"/> captures what is done for nodes of hierarchical structure.
   /// </remarks>
   public delegate Boolean AcceptElementDelegateWithContext<in TElement, in TContext>( TElement element, /*AcceptElementCallbackDelegate<Object> nonConformingAcceptCallback,*/ TContext context );

   public delegate Boolean AcceptEdgeDelegateWithContext<in TElement, in TContext>( TElement element, Object edgeInfo, TContext context );

   internal class TypeBasedVisitor<TElement>
   {
      private readonly TopMostTypeVisitingStrategy _topMostVisitingStrategy;

      public TypeBasedVisitor( TopMostTypeVisitingStrategy strategy = TopMostTypeVisitingStrategy.Never )
      {
         this._topMostVisitingStrategy = strategy;
         this.VisitorDictionary = new Dictionary<Type, VisitElementDelegate<TElement>>();
      }

      private IDictionary<Type, VisitElementDelegate<TElement>> VisitorDictionary { get; }

      public void RegisterVisitor( Type elementType, VisitElementDelegate<TElement> visitor )
      {
         this.VisitorDictionary[elementType] = ArgumentValidator.ValidateNotNull( "Visitor", visitor );
      }

      public Boolean Visit(
         TElement element,
         AcceptorInformation<TElement> acceptors
         //AcceptElementCallbackDelegate<Object> nonConformingAcceptCallback
         )
      {
         VisitElementCallbackDelegate<TElement> callback = null;
         callback = ( el, edgeType, edgeName, edgeInfo, type ) => this.VisitElementWithNoContext( el, acceptors, callback, edgeType, edgeName, edgeInfo, type );
         return this.VisitElementWithNoContext( element, acceptors, callback, null, null, null, null );

         //return new Visitor( this, acceptorDictionary ) //, nonConformingAcceptCallback );
         //   .Visit( element, null );
      }

      public Boolean VisitWithContext<TContext>(
         TElement element,
         DictionaryQuery<Type, AcceptElementDelegateWithContext<TElement, TContext>> acceptorDictionary,
         //AcceptElementCallbackDelegate<Object> nonConformingAcceptCallback,
         TContext context
         )
      {
         VisitElementCallbackDelegate<TElement> callback = null;
         callback = ( el, edgeType, edgeName, edgeInfo, type ) => this.VisitElementWithContext( el, acceptorDictionary, context, callback, edgeType, edgeName, edgeInfo, type );
         return this.VisitElementWithContext( element, acceptorDictionary, context, callback, null, null, null, null );
         //return new Visitor<TContext>( this, acceptorDictionary, /*nonConformingAcceptCallback,*/ context )
         //   .Visit( element, null );
      }

      private Boolean VisitElementWithNoContext(
         TElement element,
         AcceptorInformation<TElement> acceptors,
         //AcceptElementCallbackDelegate<Object> nonConformingAcceptCallback,
         VisitElementCallbackDelegate<TElement> callback,
         Type edgeType,
         String edgeName,
         Object edgeInfo,
         Type overrideType
         )
      {
         VisitElementDelegate<TElement> visitor;
         AcceptEdgeDelegate<TElement> edgeAcceptor;
         DictionaryQuery<String, AcceptEdgeDelegate<TElement>> edgeDic;
         AcceptElementDelegate<TElement> vertexAcceptor;
         Boolean hadAcceptor;
         return element != null
            && ( edgeType == null || edgeName == null || ( edgeDic = acceptors.EdgeAcceptors.TryGetValue( edgeType, out hadAcceptor ) ) == null || ( edgeAcceptor = edgeDic.TryGetValue( edgeName, out hadAcceptor ) ) == null || edgeAcceptor( element, edgeInfo ) )
            && this.CheckForTopMostTypeStrategy( element, acceptors.VertexAcceptors, overrideType )
            && ( ( vertexAcceptor = acceptors.VertexAcceptors.TryGetValue( overrideType ?? element.GetType(), out hadAcceptor ) ) == null || vertexAcceptor( element ) ) //, nonConformingAcceptCallback ) )
            && ( !this.VisitorDictionary.TryGetValue( overrideType ?? element.GetType(), out visitor ) || visitor( element, callback ) );
      }

      private Boolean VisitElementWithContext<TContext>(
         TElement element,
         DictionaryQuery<Type, AcceptElementDelegateWithContext<TElement, TContext>> acceptorDictionary,
         //AcceptElementCallbackDelegate<Object> nonConformingAcceptCallback,
         TContext context,
         VisitElementCallbackDelegate<TElement> callback,
         Type edgeType,
         String edgeName,
         Object edgeInfo,
         Type overrideType
         )
      {
         VisitElementDelegate<TElement> visitor;
         AcceptElementDelegateWithContext<TElement, TContext> acceptor;
         Boolean hadAcceptor;
         return element != null
            && this.CheckForTopMostTypeStrategy( element, acceptorDictionary, context, overrideType )
            && ( ( acceptor = acceptorDictionary.TryGetValue( overrideType ?? element.GetType(), out hadAcceptor ) ) == null || acceptor( element, /*nonConformingAcceptCallback,*/ context ) )
            && ( !this.VisitorDictionary.TryGetValue( overrideType ?? element.GetType(), out visitor ) || visitor( element, callback ) );
      }

      private Boolean CheckForTopMostTypeStrategy(
         TElement element,
         DictionaryQuery<Type, AcceptElementDelegate<TElement>> acceptorDictionary,
         Type overrideType
         )
      {
         Boolean hadAcceptor;
         AcceptElementDelegate<TElement> acceptor;
         switch ( this._topMostVisitingStrategy )
         {
            case TopMostTypeVisitingStrategy.IfNotOverridingType:
               return overrideType != null
                  || ( acceptor = acceptorDictionary.TryGetValue( typeof( TElement ), out hadAcceptor ) ) == null
                  || acceptor( element );

            case TopMostTypeVisitingStrategy.Always:
               return ( acceptor = acceptorDictionary.TryGetValue( typeof( TElement ), out hadAcceptor ) ) == null
                  || acceptor( element );
            default:
               return true;
         }
      }

      private Boolean CheckForTopMostTypeStrategy<TContext>(
         TElement element,
         DictionaryQuery<Type, AcceptElementDelegateWithContext<TElement, TContext>> acceptorDictionary,
         TContext context,
         Type overrideType
         )
      {
         Boolean hadAcceptor;
         AcceptElementDelegateWithContext<TElement, TContext> acceptor;
         switch ( this._topMostVisitingStrategy )
         {
            case TopMostTypeVisitingStrategy.IfNotOverridingType:
               return overrideType != null
                  || ( acceptor = acceptorDictionary.TryGetValue( typeof( TElement ), out hadAcceptor ) ) == null
                  || acceptor( element, context );

            case TopMostTypeVisitingStrategy.Always:
               return ( acceptor = acceptorDictionary.TryGetValue( typeof( TElement ), out hadAcceptor ) ) == null
                  || acceptor( element, context );
            default:
               return true;
         }
      }
   }

   internal enum TopMostTypeVisitingStrategy
   {
      Never,
      IfNotOverridingType,
      Always
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
      //private readonly AcceptElementCallbackDelegate<Object> _nonConformingAcceptor;

      private readonly AcceptorInformation<TElement> _acceptorInfo;
      internal TypeBasedAcceptor( TypeBasedVisitor<TElement> visitor )
         : base( visitor )
      {
         this.Acceptors = new Dictionary<Type, AcceptElementDelegate<TElement>>().ToDictionaryProxy();
         this.EdgeAcceptors = CollectionsWithRoles.Implementation.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewDictionary<Type, DictionaryProxy<String, AcceptEdgeDelegate<TElement>>, DictionaryProxyQuery<String, AcceptEdgeDelegate<TElement>>, DictionaryQuery<String, AcceptEdgeDelegate<TElement>>>();

         this._acceptorInfo = new AcceptorInformation<TElement>( this.Acceptors.CQ, this.EdgeAcceptors.CQ.IQ );
         //this.NonConformingAcceptors = new Dictionary<Type, AcceptElementDelegate<Object>>().ToDictionaryProxy();
         //this._nonConformingAcceptor = this.AcceptNonConforming;
      }

      private DictionaryProxy<Type, AcceptElementDelegate<TElement>> Acceptors { get; }

      private DictionaryWithRoles<Type, DictionaryProxy<String, AcceptEdgeDelegate<TElement>>, DictionaryProxyQuery<String, AcceptEdgeDelegate<TElement>>, DictionaryQuery<String, AcceptEdgeDelegate<TElement>>> EdgeAcceptors { get; }

      //private DictionaryProxy<Type, AcceptElementDelegate<Object>> NonConformingAcceptors { get; }

      public void RegisterAcceptor( Type type, AcceptElementDelegate<TElement> acceptor )
      {
         this.Acceptors[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
      }

      public void RegisterEdgeAcceptor( Type type, String edgeName, AcceptEdgeDelegate<TElement> enter )
      {
         Boolean success;
         var inner = this.EdgeAcceptors.CQ.TryGetValue( type, out success );
         if ( inner == null )
         {
            inner = new Dictionary<String, AcceptEdgeDelegate<TElement>>().ToDictionaryProxy();
            this.EdgeAcceptors.Add( type, inner );
         }
         inner[edgeName] = enter;
      }

      //public void RegisterNonConformingAcceptor( Type type, AcceptElementDelegate<Object> acceptor )
      //{
      //   this.NonConformingAcceptors[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
      //}

      public Boolean Accept( TElement element )
      {
         return this.Visitor.Visit( element, this._acceptorInfo ); //, this._nonConformingAcceptor );
      }

      //private Boolean AcceptNonConforming( Object element, Type overrideType )
      //{
      //   Boolean hadAcceptor;
      //   AcceptElementDelegate<Object> acceptor;
      //   return element == null
      //      || ( acceptor = this.NonConformingAcceptors.CQ.TryGetValue( overrideType ?? element.GetType(), out hadAcceptor ) ) == null
      //      || acceptor( element, this._nonConformingAcceptor );
      //}
   }

   internal class TypeBasedAcceptor<TElement, TContext> : AbstractTypeBasedAcceptor<TElement>
   {
      //private struct Acceptor
      //{
      //   private readonly TypeBasedAcceptor<TElement, TContext> _acceptor;
      //   private readonly TContext _context;
      //   private readonly AcceptElementCallbackDelegate<Object> _nonConformingAcceptor;

      //   public Acceptor( TypeBasedAcceptor<TElement, TContext> acceptor, TContext context )
      //   {
      //      this._acceptor = acceptor;
      //      this._context = context;
      //      this._nonConformingAcceptor = null;

      //      this._nonConformingAcceptor = this.Accept;
      //   }

      //   public Boolean Accept( Object element, Type overrideType )
      //   {
      //      Boolean hadAcceptor;
      //      AcceptElementDelegateWithContext<Object, TContext> acceptor;
      //      return element == null
      //         || ( acceptor = this._acceptor.NonConformingAcceptors.CQ.TryGetValue( overrideType ?? element.GetType(), out hadAcceptor ) ) == null
      //         || acceptor( element, this._nonConformingAcceptor, this._context );
      //   }
      //}

      internal TypeBasedAcceptor( TypeBasedVisitor<TElement> visitor )
         : base( visitor )
      {
         this.Acceptors = new Dictionary<Type, AcceptElementDelegateWithContext<TElement, TContext>>().ToDictionaryProxy();
         //this.NonConformingAcceptors = new Dictionary<Type, AcceptElementDelegateWithContext<Object, TContext>>().ToDictionaryProxy();
      }

      private DictionaryProxy<Type, AcceptElementDelegateWithContext<TElement, TContext>> Acceptors { get; }

      //private DictionaryProxy<Type, AcceptElementDelegateWithContext<Object, TContext>> NonConformingAcceptors { get; }

      public void RegisterAcceptor( Type type, AcceptElementDelegateWithContext<TElement, TContext> acceptor )
      {
         this.Acceptors[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
      }

      //public void RegisterNonConformingAcceptor( Type type, AcceptElementDelegateWithContext<Object, TContext> acceptor )
      //{
      //   this.NonConformingAcceptors[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
      //}

      public Boolean AcceptWithContext( TElement element, TContext context )
      {
         //var acceptor = new Acceptor( this, context );
         return this.Visitor.VisitWithContext( element, this.Acceptors.CQ, /*acceptor.Accept,*/ context );
      }
   }

   internal class AcceptorInformation<TElement>
   {
      public AcceptorInformation(
         DictionaryQuery<Type, AcceptElementDelegate<TElement>> vertexAcceptors,
         DictionaryQuery<Type, DictionaryQuery<String, AcceptEdgeDelegate<TElement>>> edgeAcceptors
         )
      {
         this.VertexAcceptors = ArgumentValidator.ValidateNotNull( "Vertex acceptors", vertexAcceptors );
         this.EdgeAcceptors = ArgumentValidator.ValidateNotNull( "Edge acceptors", edgeAcceptors );
      }

      public DictionaryQuery<Type, AcceptElementDelegate<TElement>> VertexAcceptors { get; }

      public DictionaryQuery<Type, DictionaryQuery<String, AcceptEdgeDelegate<TElement>>> EdgeAcceptors { get; }

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
   public class DefaultSignatureProvider : DefaultSelfDescribingExtensionByCompositionProvider<Object>, SignatureProvider
   {
      /// <summary>
      /// This class contains information to implement visitor logic and various acceptor logic for signatures.
      /// </summary>
      /// <typeparam name="TFunctionality">The type of functionality</typeparam>
      public class SignatureTypeInfo<TFunctionality>
      {
         /// <summary>
         /// Creates a new instance of <see cref="SignatureTypeInfo{TFunctionality}"/> with given parameters.
         /// </summary>
         /// <param name="signatureElementType">The type of signature element.</param>
         /// <param name="factory">The callback to create <see cref="SignatureTableIndexInfo"/>s from signature element.</param>
         /// <exception cref="ArgumentNullException">If <paramref name="signatureElementType"/> is <c>null</c>.</exception>
         /// <exception cref="ArgumentException">If <paramref name="signatureElementType"/> is generic type or it is not assignable from <see cref="SignatureElement"/>.</exception>
         public SignatureTypeInfo( Type signatureElementType, TFunctionality factory )
         {
            this.SignatureElementType = ArgumentValidator.ValidateNotNull( "Signature element type", signatureElementType );
            if ( this.SignatureElementType.GetGenericArguments().Length > 0 )
            {
               throw new ArgumentException( "Signature element type must not be generic type." );
            }
            else if ( !typeof( SignatureElement ).IsAssignableFrom( signatureElementType ) )
            {
               throw new ArgumentException( "Signature element type must be sub-type of " + typeof( SignatureElement ) + "." );
            }

            this.Functionality = factory;
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
         public TFunctionality Functionality { get; }
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



      /// <summary>
      /// Creates a new instance of <see cref="DefaultSignatureProvider"/> with given supported <see cref="SimpleTypeSignature"/>s and <see cref="CustomAttributeArgumentTypeSimple"/>s.
      /// </summary>
      /// <param name="simpleTypeSignatures">The supported <see cref="SimpleTypeSignature"/>. If <c>null</c>, the return value of <see cref="GetDefaultSimpleTypeSignatures"/> will be used.</param>
      /// <param name="simpleCATypes">The supported <see cref="CustomAttributeArgumentTypeSimple"/>. If <c>null</c>, the return value of <see cref="GetDefaultSimpleCATypes"/> will be used.</param>
      /// <param name="signatureTableIndexInfoProviders">The <see cref="SignatureTypeInfo{TFunctionality}"/> functionality. If <c>null</c>, the return value of <see cref="GetDefaultSignatureTableIndexInfoCollectors"/> will be used.</param>
      /// <param name="signatureVisitors">The enumerable of signature visitors.</param>
      public DefaultSignatureProvider(
         IEnumerable<SimpleTypeSignature> simpleTypeSignatures = null,
         IEnumerable<CustomAttributeArgumentTypeSimple> simpleCATypes = null,
         IEnumerable<SignatureTypeInfo<VisitElementDelegate<SignatureElement>>> signatureVisitors = null,
         IEnumerable<SignatureTypeInfo<AcceptElementDelegateWithContext<SignatureElement, TableIndexCollectorContext>>> signatureTableIndexInfoProviders = null
         )
      {
         this._simpleTypeSignatures = ( simpleTypeSignatures ?? GetDefaultSimpleTypeSignatures() )
            .Where( s => s != null )
            .ToDictionary_Overwrite( s => s.SimpleType, s => s );
         this._simpleCATypes = ( simpleCATypes ?? GetDefaultSimpleCATypes() )
            .Where( s => s != null )
            .ToDictionary_Overwrite( s => s.SimpleType, s => s );

         // Signature visitor
         var visitor = new TypeBasedVisitor<SignatureElement>( TopMostTypeVisitingStrategy.IfNotOverridingType );
         foreach ( var visitorInfo in signatureVisitors ?? GetDefaultSignatureVisitors() )
         {
            visitor.RegisterVisitor( visitorInfo.SignatureElementType, visitorInfo.Functionality );
         }
         this.RegisterFunctionalityDirect( visitor );

         // Signature acceptor: decomposing
         var decomposer = new TypeBasedAcceptor<SignatureElement, DecomposeSignatureContext>( visitor );
         decomposer.RegisterSignatureDecomposer<SignatureElement>( ( el, ctx ) => ctx.AddElement( el ) );
         this.RegisterFunctionalityDirect( decomposer );

         // Signature acceptor: table index collector
         var tableIndexCollector = new TypeBasedAcceptor<SignatureElement, TableIndexCollectorContext>( visitor );
         foreach ( var tableIndexCollectorInfo in signatureTableIndexInfoProviders ?? GetDefaultSignatureTableIndexInfoCollectors() )
         {
            tableIndexCollector.RegisterAcceptor( tableIndexCollectorInfo.SignatureElementType, tableIndexCollectorInfo.Functionality );
         }
         this.RegisterFunctionalityDirect( tableIndexCollector );
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

         var acceptor = new TypeBasedAcceptor<SignatureElement, SignatureMatchingContext>( this.GetFunctionality<TypeBasedVisitor<SignatureElement>>() );


         return retVal;
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
                  && MatchTypeRefs( defModule, defIdx.Index, refModule, refIdx.Index, matcher ) );
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
               && MatchTypeRefs( defModule, defResScopeNullable.Value.Index, refModule, refResScopeNullable.Value.Index, matcher )
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
      /// Returns <see cref="SignatureTypeInfo{TFunctionality}"/> functionality for <see cref="ClassOrValueTypeSignature"/> and <see cref="CustomModifierSignature"/>.
      /// </summary>
      /// <returns>Default <see cref="SignatureTypeInfo{TFunctionality}"/> functionality for <see cref="ClassOrValueTypeSignature"/> and <see cref="CustomModifierSignature"/>.</returns>
      public static IEnumerable<SignatureTypeInfo<AcceptElementDelegateWithContext<SignatureElement, TableIndexCollectorContext>>> GetDefaultSignatureTableIndexInfoCollectors()
      {
         yield return NewAcceptFunctionality<ClassOrValueTypeSignature, TableIndexCollectorContext>( ( sig, ctx ) => ctx.AddElement( new SignatureTableIndexInfo( sig.Type, tIdx => sig.Type = tIdx ) ) );
         yield return NewAcceptFunctionality<CustomModifierSignature, TableIndexCollectorContext>( ( sig, ctx ) => ctx.AddElement( new SignatureTableIndexInfo( sig.CustomModifierType, tIdx => sig.CustomModifierType = tIdx ) ) );
      }

      /// <summary>
      /// Gets enumerable of visitor functionalities for default signatures.
      /// </summary>
      /// <returns>The enumerable of visitor functionalities for default signatures.</returns>
      public static IEnumerable<SignatureTypeInfo<VisitElementDelegate<SignatureElement>>> GetDefaultSignatureVisitors()
      {
         yield return NewVisitFunctionality<ParameterOrLocalSignature>( ( sig, cb ) =>
            VisitSigElementList( typeof( ParameterOrLocalSignature ), nameof( ParameterOrLocalSignature.CustomModifiers ), sig.CustomModifiers, cb )
            && cb.VisitSimpleEdge( sig.Type, typeof( ParameterOrLocalSignature ), nameof( ParameterOrLocalSignature.Type ) )
         );

         yield return NewVisitFunctionality<ParameterSignature>( ( sig, cb ) =>
            cb.VisitBaseType( sig, typeof( ParameterOrLocalSignature ) )
         );

         yield return NewVisitFunctionality<LocalSignature>( ( sig, cb ) =>
            cb.VisitBaseType( sig, typeof( ParameterOrLocalSignature ) )
         );

         yield return NewVisitFunctionality<FieldSignature>( ( sig, cb ) =>
            VisitSigElementList( typeof( FieldSignature ), nameof( FieldSignature.CustomModifiers ), sig.CustomModifiers, cb )
            && cb.VisitSimpleEdge( sig.Type, typeof( FieldSignature ), nameof( FieldSignature.Type ) )
         );

         yield return NewVisitFunctionality<AbstractMethodSignature>( ( sig, cb ) =>
            cb.VisitSimpleEdge( sig.ReturnType, typeof( AbstractMethodSignature ), nameof( AbstractMethodSignature.ReturnType ) )
            && VisitSigElementList( typeof( AbstractMethodSignature ), nameof( AbstractMethodSignature.Parameters ), sig.Parameters, cb )
         );

         yield return NewVisitFunctionality<MethodDefinitionSignature>( ( sig, cb ) =>
            cb.VisitBaseType( sig, typeof( AbstractMethodSignature ) )
         );

         yield return NewVisitFunctionality<MethodReferenceSignature>( ( sig, cb ) =>
            cb.VisitBaseType( sig, typeof( AbstractMethodSignature ) )
            && VisitSigElementList( typeof( MethodReferenceSignature ), nameof( MethodReferenceSignature.VarArgsParameters ), sig.VarArgsParameters, cb )
         );

         yield return NewVisitFunctionality<PropertySignature>( ( sig, cb ) =>
            VisitSigElementList( typeof( PropertySignature ), nameof( PropertySignature.CustomModifiers ), sig.CustomModifiers, cb )
            && cb.VisitSimpleEdge( sig.PropertyType, typeof( PropertySignature ), nameof( PropertySignature.PropertyType ) )
            && VisitSigElementList( typeof( PropertySignature ), nameof( PropertySignature.Parameters ), sig.Parameters, cb )
         );

         yield return NewVisitFunctionality<LocalVariablesSignature>( ( sig, cb ) =>
            VisitSigElementList( typeof( LocalVariablesSignature ), nameof( LocalVariablesSignature.Locals ), sig.Locals, cb )
         );

         yield return NewVisitFunctionality<GenericMethodSignature>( ( sig, cb ) =>
            VisitSigElementList( typeof( GenericMethodSignature ), nameof( GenericMethodSignature.GenericArguments ), sig.GenericArguments, cb )
         );

         yield return NewVisitFunctionality<AbstractArrayTypeSignature>( ( sig, cb ) =>
            cb.VisitSimpleEdge( sig.ArrayType, typeof( AbstractArrayTypeSignature ), nameof( AbstractArrayTypeSignature.ArrayType ) )
         );

         yield return NewVisitFunctionality<ComplexArrayTypeSignature>( ( sig, cb ) =>
            cb.VisitBaseType( sig, typeof( AbstractArrayTypeSignature ) )
         );

         yield return NewVisitFunctionality<SimpleArrayTypeSignature>( ( sig, cb ) =>
            VisitSigElementList( typeof( SimpleArrayTypeSignature ), nameof( SimpleArrayTypeSignature.CustomModifiers ), sig.CustomModifiers, cb )
            && cb.VisitBaseType( sig, typeof( AbstractArrayTypeSignature ) )
         );

         yield return NewVisitFunctionality<ClassOrValueTypeSignature>( ( sig, cb ) =>
            VisitSigElementList( typeof( ClassOrValueTypeSignature ), nameof( ClassOrValueTypeSignature.GenericArguments ), sig.GenericArguments, cb )
         );

         yield return NewVisitFunctionality<FunctionPointerTypeSignature>( ( sig, cb ) =>
            cb.VisitSimpleEdge( sig.MethodSignature, typeof( FunctionPointerTypeSignature ), nameof( FunctionPointerTypeSignature.MethodSignature ) )
         );

         yield return NewVisitFunctionality<PointerTypeSignature>( ( sig, cb ) =>
            VisitSigElementList( typeof( PointerTypeSignature ), nameof( PointerTypeSignature.CustomModifiers ), sig.CustomModifiers, cb )
            && cb.VisitSimpleEdge( sig.PointerType, typeof( PointerTypeSignature ), nameof( PointerTypeSignature.PointerType ) )
         );
      }

      private static Boolean VisitSigElementList<TSigElement>( Type edgeType, String edgeName, List<TSigElement> list, VisitElementCallbackDelegate<SignatureElement> callback )
         where TSigElement : SignatureElement
      {
         var retVal = true;
         var max = list.Count;
         for ( var i = 0; i < max && retVal; ++i )
         {
            retVal = callback.VisitCollectionEdge( list[i], edgeType, edgeName, i );
         }
         return retVal;
      }
      /// <summary>
      /// Creates a new <see cref="SignatureTypeInfo{TFunctionality}"/> with functionality for visiting signatures according to visitor pattern.
      /// </summary>
      /// <typeparam name="TSignature">The type of the signature element.</typeparam>
      /// <param name="functionality">The visitor functionality.</param>
      /// <returns>A new instance of <see cref="SignatureTypeInfo{TFunctionality}"/>.</returns>
      public static SignatureTypeInfo<VisitElementDelegate<SignatureElement>> NewVisitFunctionality<TSignature>( VisitElementDelegateTyped<SignatureElement, TSignature> functionality )
         where TSignature : SignatureElement
      {
         return new SignatureTypeInfo<VisitElementDelegate<SignatureElement>>( typeof( TSignature ), functionality.AsVisitElementDelegate() );
      }

      /// <summary>
      /// Creates a new <see cref="SignatureTypeInfo{TFunctionality}"/> with functionality for accepting signatures according to visitor pattern.
      /// </summary>
      /// <typeparam name="TSignature">The type of the signature element.</typeparam>
      /// <typeparam name="TContext">The type of context.</typeparam>
      /// <param name="acceptor">The acceptor functionality.</param>
      /// <returns>A new instance of <see cref="SignatureTypeInfo{TFunctionality}"/>.</returns>
      public static SignatureTypeInfo<AcceptElementDelegateWithContext<SignatureElement, TContext>> NewAcceptFunctionality<TSignature, TContext>( AcceptElementDelegateWithContext<TSignature, TContext> acceptor )
         where TSignature : SignatureElement
      {
         return new SignatureTypeInfo<AcceptElementDelegateWithContext<SignatureElement, TContext>>( typeof( TSignature ), ( el, ctx ) => acceptor( (TSignature) el, ctx ) );
      }

   }

   /// <summary>
   /// This is abstract base class for functionalities that collect something from hierarchical structures.
   /// </summary>
   /// <typeparam name="TElement">The type of elements to collect.</typeparam>
   public abstract class ElementCollectorState<TElement>
   {
      private readonly ListProxy<TElement> _elements;
      private readonly Boolean _nullsAllowed;

      /// <summary>
      /// Creates new instance of <see cref="ElementCollectorState{TElement}"/>.
      /// </summary>
      /// <param name="nullsAllowed">Whether <c>null</c> values are allowed.</param>
      public ElementCollectorState( Boolean nullsAllowed = false )
      {
         this._nullsAllowed = nullsAllowed;
         this._elements = new List<TElement>().ToListProxy();
      }

      /// <summary>
      /// Adds an element to the current list of elements.
      /// </summary>
      /// <param name="element">The element to add.</param>
      /// <returns>Always returns <c>true</c>.</returns>
      public Boolean AddElement( TElement element )
      {
         if ( this._nullsAllowed || element != null )
         {
            this._elements.Add( element );
         }
         return true;
      }

      /// <summary>
      /// Gets the added elements.
      /// </summary>
      /// <value>The added elements.</value>
      public ListQuery<TElement> Elements
      {
         get
         {
            return this._elements.CQ;
         }
      }
   }

   internal class DecomposeSignatureContext : ElementCollectorState<SignatureElement>
   {
   }

   /// <summary>
   /// This state is used in <see cref="E_CILPhysical.GetSignatureTableIndexInfos"/> method to perform visitor walking in order to extract <see cref="SignatureTableIndexInfo"/>s.
   /// </summary>
   public class TableIndexCollectorContext : ElementCollectorState<SignatureTableIndexInfo>
   {

   }

   internal class SignatureMatchingContext
   {

      public SignatureElement CurrentElement { get; set; }
   }

   internal delegate Boolean EqualityWithContext<TItem, TContext>( TItem x, TItem y, TContext context );
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
   /// Returns enumerable of leaf signature elements of given <see cref="AbstractSignature"/>.
   /// </summary>
   /// <param name="provider"></param>
   /// <param name="signature">The <see cref="AbstractSignature"/> to decompose.</param>
   /// <returns>The recursive enumerable of all <see cref="SignatureElement"/>s of <paramref name="signature"/>. Will be empty if <paramref name="signature"/> is <c>null</c>.</returns>

   public static IEnumerable<SignatureElement> DecomposeSignature( this SignatureProvider provider, AbstractSignature signature )
   {
      var decomposer = provider.GetFunctionality<TypeBasedAcceptor<SignatureElement, DecomposeSignatureContext>>();
      var decomposeContext = new DecomposeSignatureContext();
      decomposer.AcceptWithContext( signature, decomposeContext );
      return decomposeContext.Elements;
   }


   /// <summary>
   /// Extracts all <see cref="SignatureTableIndexInfo"/> related to a single signature.
   /// </summary>
   /// <param name="provider">The <see cref="SignatureProvider"/>.</param>
   /// <param name="signature">The <see cref="AbstractSignature"/>.</param>
   /// <returns>A list of all <see cref="SignatureTableIndexInfo"/> related to a single signature. Will be empty if <paramref name="signature"/> is <c>null</c>.</returns>
   public static IEnumerable<SignatureTableIndexInfo> GetSignatureTableIndexInfos( this SignatureProvider provider, AbstractSignature signature )
   {
      var collector = provider.GetFunctionality<TypeBasedAcceptor<SignatureElement, TableIndexCollectorContext>>();
      var collectorContext = new TableIndexCollectorContext();
      collector.AcceptWithContext( signature, collectorContext );
      return collectorContext.Elements;
   }

   internal static void RegisterVisitor<TElement, TActualElement>( this TypeBasedVisitor<TElement> visitorAggregator, VisitElementDelegateTyped<TElement, TActualElement> visitor )
      where TActualElement : TElement
   {
      visitorAggregator.RegisterVisitor( typeof( TActualElement ), ( el, cb ) => visitor( (TActualElement) el, cb ) );
   }

   internal static void RegisterSignatureVisitor<TSignature>( this TypeBasedVisitor<SignatureElement> visitorAggregator, VisitElementDelegateTyped<SignatureElement, TSignature> visitor )
      where TSignature : SignatureElement
   {
      visitorAggregator.RegisterVisitor( visitor );
   }

   internal static void RegisterAcceptor<TElement, TContext, TActualElement>( this TypeBasedAcceptor<TElement, TContext> acceptorAggregator, AcceptElementDelegateWithContext<TActualElement, TContext> acceptor )
      where TActualElement : TElement
   {
      acceptorAggregator.RegisterAcceptor( typeof( TActualElement ), ( el, ctx ) => acceptor( (TActualElement) el, ctx ) );
   }

   internal static void RegisterSignatureDecomposer<TSignature>( this TypeBasedAcceptor<SignatureElement, DecomposeSignatureContext> acceptorAggregator, AcceptElementDelegateWithContext<TSignature, DecomposeSignatureContext> acceptor )
      where TSignature : SignatureElement
   {
      acceptorAggregator.RegisterAcceptor( acceptor );
   }

   internal static void RegisterSignatureMatcher<TSignature>( this TypeBasedAcceptor<SignatureElement, SignatureMatchingContext> acceptorAggregator, EqualityWithContext<TSignature, SignatureMatchingContext> acceptor )
      where TSignature : class, SignatureElement
   {
      acceptorAggregator.RegisterAcceptor( typeof( TSignature ), ( el, ctx ) =>
      {
         TSignature fromCtx;
         return ReferenceEquals( el, ctx.CurrentElement )
         || ( el != null && ( fromCtx = ctx.CurrentElement as TSignature ) != null
         && acceptor( (TSignature) el, fromCtx, ctx ) );
      } );
   }

   internal static VisitElementDelegate<TElement> AsVisitElementDelegate<TElement, TActualElement>( this VisitElementDelegateTyped<TElement, TActualElement> typed )
      where TActualElement : TElement
   {
      return ( el, cb ) => typed( (TActualElement) el, cb );
   }

   internal static Boolean VisitBaseType<TElement>( this VisitElementCallbackDelegate<TElement> callback, TElement element, Type baseType )
   {
      return callback( element, null, null, null, baseType );
   }

   internal static Boolean VisitSimpleEdge<TElement>( this VisitElementCallbackDelegate<TElement> callback, TElement element, Type edgeType, String edgeName )
   {
      return callback( element, edgeType, edgeName, null );
   }

   internal static Boolean VisitCollectionEdge<TElement>( this VisitElementCallbackDelegate<TElement> callback, TElement element, Type edgeType, String edgeName, Int32 index )
   {
      return callback( element, edgeType, edgeName, index );
   }

   //internal static void RegisterSignatureElementDecomposer<TElement>( this TypeBasedAcceptor<AbstractSignature, DecomposeSignatureContext> acceptorAggregator, AcceptElementDelegateWithContext<TElement, DefaultSignatureProvider.DecomposeSignatureContext> acceptor )
   //   where TElement : SignatureElement
   //{
   //   acceptorAggregator.RegisterNonConformingAcceptor( typeof( TElement ), ( el, nc, ctx ) => acceptor( (TElement) el, nc, ctx ) );
   //}
}
