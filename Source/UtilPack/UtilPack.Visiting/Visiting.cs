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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilPack.CollectionsWithRoles;
using UtilPack.Visiting;

namespace UtilPack.Visiting
{
#pragma warning disable 1591


   /// <summary>
   /// This delegate contains signature required for callbacks which perform visiting of hierarchically contained items according to visitor-pattern.
   /// </summary>
   /// <typeparam name="TElement">The common type for elements.</typeparam>
   /// <typeparam name="TEdgeInfo">The type for edge information (e.g. list index).</typeparam>
   /// <param name="element">The current element.</param>
   /// <param name="callback">The callback to call for all other elements hierarchically contained by given <paramref name="element"/>.</param>
   /// <returns><c>true</c> if should continue visiting; <c>false</c> otherwise.</returns>
   public delegate Boolean VisitElementDelegate<TElement, TEdgeInfo>( TElement element, VisitElementCallbackDelegate<TElement, TEdgeInfo> callback );

   /// <summary>
   /// This is helper delegate for callbacks which want automatic casting to be performed.
   /// </summary>
   /// <typeparam name="TElement">The common type for elements.</typeparam>
   /// <typeparam name="TEdgeInfo">The type for edge information (e.g. list index).</typeparam>
   /// <typeparam name="TActualElement">The casted type for elements..</typeparam>
   /// <param name="element">The current element.</param>
   /// <param name="callback">The callback to call for all other elements hierarchically contained by given <paramref name="element"/>.</param>
   /// <returns><c>true</c> if should continue visiting; <c>false</c> otherwise.</returns>
   /// <seealso cref="M:E_UtilPack.AsVisitElementDelegate"/>
   public delegate Boolean VisitElementDelegateTyped<TElement, TEdgeInfo, in TActualElement>( TActualElement element, VisitElementCallbackDelegate<TElement, TEdgeInfo> callback )
      where TActualElement : TElement;

   /// <summary>
   /// This is delegate that should be called by methods called by <see cref="VisitElementDelegate{TElement, TEdgeInfo}"/>. It will take care to visit the given element according to the visitor pattern.
   /// </summary>
   /// <typeparam name="TElement">The common type for elements.</typeparam>
   /// <typeparam name="TEdgeInfo">The type for edge information (e.g. list index).</typeparam>
   /// <param name="element">The element that should be visited.</param>
   /// <param name="edgeType">The information about type containing edge.</param>
   /// <param name="edgeName">The name of the edge.</param>
   /// <param name="edgeInfo">The edge-specific info object (e.g. list index).</param>
   /// <param name="overrideType">If parameter is supplied, then this type is used to lookup the visitor functionality for given <paramref name="element"/>, instead of <see cref="Object.GetType"/>.</param>
   /// <returns><c>true</c> if should continue visiting; <c>false</c> otherwise.</returns>
   /// <seealso cref="TypeBasedVisitor{TElement, TEdgeInfo}"/>
   public delegate Boolean VisitElementCallbackDelegate<in TElement, in TEdgeInfo>( TElement element, Type edgeType, String edgeName, TEdgeInfo edgeInfo, Type overrideType = null );


   public class VisitorVertexInfo<TElement, TEdgeInfo>
   {
      public VisitorVertexInfo( Type type, params VisitorEdgeInfo<TElement, TEdgeInfo>[] edges )
      {
         this.Type = ArgumentValidator.ValidateNotNull( "Type", type );
         this.Edges = ( edges ?? Empty<VisitorEdgeInfo<TElement, TEdgeInfo>>.Array ).Where( e => e != null ).ToArrayProxy().CQ;
      }
      public Type Type { get; }

      public ArrayQuery<VisitorEdgeInfo<TElement, TEdgeInfo>> Edges { get; }
   }

   public class VisitorEdgeInfo<TElement, TEdgeInfo>
   {
      public VisitorEdgeInfo( Type type, String name, VisitElementDelegate<TElement, TEdgeInfo> del )
      {
         this.Type = ArgumentValidator.ValidateNotNull( "Type", type );
         this.Name = name;
         this.Delegate = ArgumentValidator.ValidateNotNull( "Delegate", del );
      }

      public Type Type { get; }

      public String Name { get; }

      public VisitElementDelegate<TElement, TEdgeInfo> Delegate { get; }

      public static VisitorEdgeInfo<TElement, TEdgeInfo> CreatePropertyEdge<TActualElement>( String name, VisitElementDelegateTyped<TElement, TEdgeInfo, TActualElement> typedDelegate )
         where TActualElement : TElement
      {
         return new VisitorEdgeInfo<TElement, TEdgeInfo>( typeof( TActualElement ), name, typedDelegate.AsVisitElementDelegate() );
      }

      public static VisitorEdgeInfo<TElement, TEdgeInfo> CreateBaseTypeEdge<TActualElement, TBaseType>()
         where TActualElement : TBaseType
         where TBaseType : TElement
      {
         return new VisitorEdgeInfo<TElement, TEdgeInfo>( typeof( TBaseType ), null, ( sig, cb ) => cb.VisitBaseType( sig, typeof( TBaseType ) ) );
      }
   }

   public class TypeBasedVisitor<TElement, TEdgeInfo>
   {

      public TypeBasedVisitor()
      {
         this.VerticeInfos = new Dictionary<Type, VisitorVertexInfo<TElement, TEdgeInfo>>();
      }

      private IDictionary<Type, VisitorVertexInfo<TElement, TEdgeInfo>> VerticeInfos { get; }


      public void RegisterVisitor( Type elementType, VisitorVertexInfo<TElement, TEdgeInfo> visitor )
      {
         this.VerticeInfos[elementType] = ArgumentValidator.ValidateNotNull( "Visitor", visitor );
      }

      public VisitorInformation<TElement, TEdgeInfo> CreateVisitorInfo( AcceptorInformation<TElement> acceptorInfo )
      {
         return new VisitorInformation<TElement, TEdgeInfo>( this, acceptorInfo );
      }

      public VisitorInformation<TElement, TEdgeInfo, TContext> CreateVisitorInfo<TContext>( AcceptorInformation<TElement, TContext> acceptorInfo, TContext context )
      {
         return new VisitorInformation<TElement, TEdgeInfo, TContext>( this, acceptorInfo, context );
      }

      public Boolean Visit(
         TElement element,
         VisitorInformation<TElement, TEdgeInfo> visitorInfo
         )
      {
         return this.VisitElementWithNoContext( element, visitorInfo.AcceptorInformation, visitorInfo.Callback, null, null, default( TEdgeInfo ), null );
      }

      public Boolean Visit<TContext>(
         TElement element,
         VisitorInformation<TElement, TEdgeInfo, TContext> visitorInfo
         )
      {
         return this.VisitElementWithContext( element, visitorInfo.AcceptorInformation, visitorInfo.Context, visitorInfo.Callback, null, null, default( TEdgeInfo ), null );
      }

      // TODO parametrize what to return when vertex type not found.
      public Boolean VisitExplicit<TContext>(
         TElement element,
         ExplicitAcceptorInformation<TElement, TContext> acceptors,
         TContext context
         )
      {
         VisitElementCallbackDelegate<TElement, TEdgeInfo> callback = null;
         AcceptVertexExplicitCallbackDelegate<TElement, TContext> acceptorCallback = null;
         acceptorCallback = ( el, ctx ) => el == null || this.VisitElementWithContext( el, acceptors, context, callback, acceptorCallback, null, null, default( TEdgeInfo ), null ); // this.VisitEdges( el, el.GetType(), callback );
         callback = ( el, edgeType, edgeName, edgeInfo, type ) => this.VisitElementWithContext( el, acceptors, context, callback, acceptorCallback, edgeType, edgeName, edgeInfo, type );

         return this.VisitElementWithContext( element, acceptors, context, callback, acceptorCallback, null, null, default( TEdgeInfo ), null );
      }

      internal Boolean VisitElementWithNoContext(
         TElement element,
         AcceptorInformation<TElement> acceptorInfo,
         VisitElementCallbackDelegate<TElement, TEdgeInfo> callback,
         Type edgeType,
         String edgeName,
         TEdgeInfo edgeInfo,
         Type overrideType
         )
      {
         DictionaryQuery<String, AcceptEdgeDelegateInformation<TElement>> edgeDic;
         AcceptEdgeDelegateInformation<TElement> edgeDelegateInfo = null;
         AcceptVertexDelegate<TElement> vertexAcceptor;
         Boolean hadAcceptor;
         return element != null
            && ( edgeType == null || edgeName == null || ( edgeDic = acceptorInfo.EdgeAcceptors.TryGetValue( edgeType, out hadAcceptor ) ) == null || ( edgeDelegateInfo = edgeDic.TryGetValue( edgeName, out hadAcceptor ) ) == null || ( edgeDelegateInfo?.Entry( element, edgeInfo ) ?? true ) )
            && this.CheckForTopMostTypeStrategy( element, acceptorInfo, overrideType )
            && ( ( ( vertexAcceptor = acceptorInfo.VertexAcceptors.TryGetValue( overrideType ?? element.GetType(), out hadAcceptor ) ) != null && vertexAcceptor( element ) ) || ( vertexAcceptor == null && acceptorInfo.ContinueOnMissingVertex ) )
            && this.VisitEdges( element, overrideType ?? element.GetType(), callback )
            && ( edgeDelegateInfo?.Exit( element, edgeInfo ) ?? true );
      }

      internal Boolean VisitElementWithContext<TContext>(
         TElement element,
         AcceptorInformation<TElement, TContext> acceptors,
         TContext context,
         VisitElementCallbackDelegate<TElement, TEdgeInfo> callback,
         Type edgeType,
         String edgeName,
         TEdgeInfo edgeInfo,
         Type overrideType
         )
      {
         DictionaryQuery<String, AcceptEdgeDelegateInformation<TElement, TContext>> edgeDic;
         AcceptEdgeDelegateInformation<TElement, TContext> edgeDelegateInfo = null;
         AcceptVertexDelegate<TElement, TContext> vertexAcceptor;
         Boolean hadAcceptor;
         return element != null
            && ( edgeType == null || edgeName == null || ( edgeDic = acceptors.EdgeAcceptors.TryGetValue( edgeType, out hadAcceptor ) ) == null || ( edgeDelegateInfo = edgeDic.TryGetValue( edgeName, out hadAcceptor ) ) == null || ( edgeDelegateInfo?.Entry( element, edgeInfo, context ) ?? true ) )
            && this.CheckForTopMostTypeStrategy( element, acceptors, context, overrideType )
            && ( ( vertexAcceptor = acceptors.VertexAcceptors.TryGetValue( overrideType ?? element.GetType(), out hadAcceptor ) ) == null || vertexAcceptor( element, context ) ) //( ( ( vertexAcceptor = acceptors.VertexAcceptors.TryGetValue( overrideType ?? element.GetType(), out hadAcceptor ) ) != null && vertexAcceptor( element, context ) ) || ( vertexAcceptor == null && acceptors.ContinueOnMissingVertex ) )
            && this.VisitEdges( element, overrideType ?? element.GetType(), callback )
            && ( edgeDelegateInfo?.Exit( element, edgeInfo, context ) ?? true );
      }

      private Boolean VisitElementWithContext<TContext>(
         TElement element,
         ExplicitAcceptorInformation<TElement, TContext> acceptors,
         TContext context,
         VisitElementCallbackDelegate<TElement, TEdgeInfo> callback,
         AcceptVertexExplicitCallbackDelegate<TElement, TContext> acceptorCallback,
         Type edgeType,
         String edgeName,
         TEdgeInfo edgeInfo,
         Type overrideType
         )
      {
         DictionaryQuery<String, AcceptEdgeDelegateInformation<TElement, TContext>> edgeDic;
         AcceptEdgeDelegateInformation<TElement, TContext> edgeDelegateInfo = null;
         AcceptVertexExplicitDelegate<TElement, TContext> vertexAcceptor;
         Boolean hadAcceptor;
         return element != null
            && ( edgeType == null || edgeName == null || ( edgeDic = acceptors.EdgeAcceptors.TryGetValue( edgeType, out hadAcceptor ) ) == null || ( edgeDelegateInfo = edgeDic.TryGetValue( edgeName, out hadAcceptor ) ) == null || ( edgeDelegateInfo?.Entry( element, edgeInfo, context ) ?? true ) )
            && this.CheckForTopMostTypeStrategy( element, acceptors, context, overrideType, acceptorCallback )
            && ( ( vertexAcceptor = acceptors.VertexAcceptors.TryGetValue( overrideType ?? element.GetType(), out hadAcceptor ) ) == null || vertexAcceptor( element, context, acceptorCallback ) ) //( ( ( vertexAcceptor = acceptors.VertexAcceptors.TryGetValue( overrideType ?? element.GetType(), out hadAcceptor ) ) != null && vertexAcceptor( element, context, acceptorCallback ) ) || ( vertexAcceptor == null && acceptors.ContinueOnMissingVertex ) )
                                                                                                                                                                                                    // && this.VisitEdges( element, overrideType ?? element.GetType(), callback )
            && ( edgeDelegateInfo?.Exit( element, edgeInfo, context ) ?? true );
      }

      private Boolean VisitEdges( TElement element, Type type, VisitElementCallbackDelegate<TElement, TEdgeInfo> callback )
      {
         VisitorVertexInfo<TElement, TEdgeInfo> info;
         var retVal = true;
         if ( this.VerticeInfos.TryGetValue( type, out info ) )
         {
            var edges = info.Edges;
            for ( var i = 0; i < edges.Count && retVal; ++i )
            {
               retVal = edges[i].Delegate( element, callback );
            }
         }
         return retVal;
      }

      private Boolean CheckForTopMostTypeStrategy(
         TElement element,
         AcceptorInformation<TElement> info,
         Type overrideType
         )
      {
         Boolean hadAcceptor;
         AcceptVertexDelegate<TElement> acceptor;
         var acceptorDictionary = info.VertexAcceptors;
         switch ( info.TopMostVisitingStrategy )
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
         AcceptorInformation<TElement, TContext> info,
         TContext context,
         Type overrideType
         )
      {
         Boolean hadAcceptor;
         AcceptVertexDelegate<TElement, TContext> acceptor;
         var acceptorDictionary = info.VertexAcceptors;
         switch ( info.TopMostVisitingStrategy )
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

      private Boolean CheckForTopMostTypeStrategy<TContext>(
         TElement element,
         ExplicitAcceptorInformation<TElement, TContext> info,
         TContext context,
         Type overrideType,
         AcceptVertexExplicitCallbackDelegate<TElement, TContext> callback
         )
      {
         Boolean hadAcceptor;
         AcceptVertexExplicitDelegate<TElement, TContext> acceptor;
         var acceptorDictionary = info.VertexAcceptors;
         switch ( info.TopMostVisitingStrategy )
         {
            case TopMostTypeVisitingStrategy.IfNotOverridingType:
               return overrideType != null
                  || ( acceptor = acceptorDictionary.TryGetValue( typeof( TElement ), out hadAcceptor ) ) == null
                  || acceptor( element, context, callback );

            case TopMostTypeVisitingStrategy.Always:
               return ( acceptor = acceptorDictionary.TryGetValue( typeof( TElement ), out hadAcceptor ) ) == null
                  || acceptor( element, context, callback );
            default:
               return true;
         }
      }
   }

   public abstract class AbstractVisitorInformation<TElement, TEdgeInfo>
   {
      public AbstractVisitorInformation( VisitElementCallbackDelegate<TElement, TEdgeInfo> callback )
      {
         this.Callback = ArgumentValidator.ValidateNotNull( "Callback", callback );
      }

      public VisitElementCallbackDelegate<TElement, TEdgeInfo> Callback { get; }
   }

   public class VisitorInformation<TElement, TEdgeInfo> : AbstractVisitorInformation<TElement, TEdgeInfo>
   {
      public VisitorInformation( TypeBasedVisitor<TElement, TEdgeInfo> visitor, AcceptorInformation<TElement> acceptorInfo )
         : base( CreateCallback( visitor, acceptorInfo ) )
      {
         this.AcceptorInformation = ArgumentValidator.ValidateNotNull( "Acceptor information", acceptorInfo );
      }

      public AcceptorInformation<TElement> AcceptorInformation { get; }

      private static VisitElementCallbackDelegate<TElement, TEdgeInfo> CreateCallback( TypeBasedVisitor<TElement, TEdgeInfo> visitor, AcceptorInformation<TElement> acceptorInfo )
      {
         ArgumentValidator.ValidateNotNull( "Visitor", visitor );

         VisitElementCallbackDelegate<TElement, TEdgeInfo> callback = null;
         callback = ( el, edgeType, edgeName, edgeInfo, type ) => visitor.VisitElementWithNoContext( el, acceptorInfo, callback, edgeType, edgeName, edgeInfo, type );
         return callback;
      }
   }

   public class VisitorInformation<TElement, TEdgeInfo, TContext> : AbstractVisitorInformation<TElement, TEdgeInfo>
   {
      public VisitorInformation( TypeBasedVisitor<TElement, TEdgeInfo> visitor, AcceptorInformation<TElement, TContext> acceptorInfo, TContext context )
         : base( CreateCallback( visitor, acceptorInfo, context ) )
      {
         this.AcceptorInformation = ArgumentValidator.ValidateNotNull( "Acceptor information", acceptorInfo );
      }

      public AcceptorInformation<TElement, TContext> AcceptorInformation { get; }

      public TContext Context { get; }

      private static VisitElementCallbackDelegate<TElement, TEdgeInfo> CreateCallback( TypeBasedVisitor<TElement, TEdgeInfo> visitor, AcceptorInformation<TElement, TContext> acceptorInfo, TContext context )
      {
         ArgumentValidator.ValidateNotNull( "Visitor", visitor );

         VisitElementCallbackDelegate<TElement, TEdgeInfo> callback = null;
         callback = ( el, edgeType, edgeName, edgeInfo, type ) => visitor.VisitElementWithContext( el, acceptorInfo, context, callback, edgeType, edgeName, edgeInfo, type );
         return callback;
      }
   }

}

public static partial class E_UtilPack
{
   public static VisitElementDelegate<TElement, TEdgeInfo> AsVisitElementDelegate<TElement, TEdgeInfo, TActualElement>( this VisitElementDelegateTyped<TElement, TEdgeInfo, TActualElement> typed )
      where TActualElement : TElement
   {
      return ( el, cb ) => typed( (TActualElement) el, cb );
   }

   public static Boolean VisitBaseType<TElement, TEdgeInfo>( this VisitElementCallbackDelegate<TElement, TEdgeInfo> callback, TElement element, Type baseType )
   {
      return callback( element, null, null, default( TEdgeInfo ), baseType );
   }

   public static Boolean VisitSimpleEdge<TElement, TEdgeInfo>( this VisitElementCallbackDelegate<TElement, TEdgeInfo> callback, TElement element, Type edgeType, String edgeName )
   {
      return callback( element, edgeType, edgeName, default( TEdgeInfo ) );
   }

   public static Boolean VisitCollectionEdge<TElement>( this VisitElementCallbackDelegate<TElement, Int32> callback, TElement element, Type edgeType, String edgeName, Int32 index )
   {
      return callback( element, edgeType, edgeName, index );
   }

   public static Boolean VisitListEdge<TElement, TEdgeElement>( this VisitElementCallbackDelegate<TElement, Int32> callback, Type edgeType, String edgeName, List<TEdgeElement> list )
      where TEdgeElement : TElement
   {
      var retVal = true;
      var max = list.Count;
      for ( var i = 0; i < max && retVal; ++i )
      {
         retVal = callback.VisitCollectionEdge( list[i], edgeType, edgeName, i );
      }
      return retVal;
   }


}