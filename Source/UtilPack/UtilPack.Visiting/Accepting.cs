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
   public delegate Boolean AcceptVertexDelegate<in TElement>( TElement element );

   public delegate Boolean AcceptEdgeDelegate<in TElement>( TElement element, Object edgeInfo );

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
   /// This is done by <see cref="VisitElementDelegate{TElement, TEdgeInfo}"/>.
   /// The <see cref="AcceptVertexDelegate{TElement, TContext}"/> should only capture the functionality that is done for element.
   /// In other words, <see cref="VisitElementDelegate{TElement, TEdgeInfo}"/> captures how the hierarchical structure is explored, and <see cref="AcceptVertexDelegate{TElement, TContext}"/> captures what is done for nodes of hierarchical structure.
   /// </remarks>
   public delegate Boolean AcceptVertexDelegate<in TElement, in TContext>( TElement element, TContext context );

   /// <summary>
   /// This is delegate that captures signature to 'accept', or visit, a single edge (transition between elements) in visitor pattern.
   /// </summary>
   /// <typeparam name="TElement">The type of element.</typeparam>
   /// <typeparam name="TContext">The type of context that is given when performing visiting.</typeparam>
   /// <param name="element">The current element.</param>
   /// <param name="edgeInfo">The domain-specific edge information.</param>
   /// <param name="context">The current context.</param>
   /// <returns><c>true</c> if visiting should be continued, <c>false</c> otherwise.</returns>
   public delegate Boolean AcceptEdgeDelegate<in TElement, in TContext>( TElement element, Object edgeInfo, TContext context );

   public delegate Boolean AcceptVertexExplicitDelegate<TElement, TContext>( TElement element, TContext context, AcceptVertexExplicitCallbackDelegate<TElement, TContext> acceptor );

   public delegate Boolean AcceptVertexExplicitCallbackDelegate<in TElement, in TContext>( TElement element, TContext context );//, Type edgeType, String edgeName, Object edgeInfo );

   public delegate Boolean AcceptVertexExplicitDelegateTyped<TElement, TContext, in TActualElement>( TActualElement element, TContext context, AcceptVertexExplicitCallbackDelegate<TElement, TContext> acceptor )
      where TActualElement : TElement;

   public enum TopMostTypeVisitingStrategy
   {
      Never,
      IfNotOverridingType,
      Always
   }

   public abstract class AbstractTypeBasedAcceptor<TElement, TEdgeInfo>
   {
      internal AbstractTypeBasedAcceptor( TypeBasedVisitor<TElement, TEdgeInfo> visitor )
      {
         this.Visitor = ArgumentValidator.ValidateNotNull( "Visitor", visitor );

      }

      protected TypeBasedVisitor<TElement, TEdgeInfo> Visitor { get; }
   }

   public sealed class TypeBasedAcceptor<TElement, TEdgeInfo> : AbstractTypeBasedAcceptor<TElement, TEdgeInfo>
   {
      private readonly VisitorInformation<TElement, TEdgeInfo> _visitorInfo;

      internal TypeBasedAcceptor( TypeBasedVisitor<TElement, TEdgeInfo> visitor, TopMostTypeVisitingStrategy topMostVisitingStrategy, Boolean continueOnMissingVertex )
         : base( visitor )
      {
         this.VertexAcceptors = new Dictionary<Type, AcceptVertexDelegate<TElement>>().ToDictionaryProxy();
         this.EdgeAcceptors = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<AcceptEdgeDelegateInformation<TElement>>();
         this._visitorInfo = visitor.CreateVisitorInfo( new AcceptorInformation<TElement>( topMostVisitingStrategy, continueOnMissingVertex, this.VertexAcceptors.CQ, this.EdgeAcceptors.CQ ) );
      }

      private DictionaryProxy<Type, AcceptVertexDelegate<TElement>> VertexAcceptors { get; }

      private ListProxy<AcceptEdgeDelegateInformation<TElement>> EdgeAcceptors { get; }

      public void RegisterVertexAcceptor( Type type, AcceptVertexDelegate<TElement> acceptor )
      {
         this.VertexAcceptors[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
      }

      public void RegisterEdgeAcceptor( Type type, String edgeName, AcceptEdgeDelegate<TElement> enter, AcceptEdgeDelegate<TElement> exit )
      {
         var edgeID = this.Visitor.GetEdgeIDOrNegative( type, edgeName );
         if ( edgeID < 0 )
         {
            throw new ArgumentException( "No edge information found for " + type + "." + edgeName + "." );
         }
         var list = this.EdgeAcceptors;
         var count = list.CQ.Count;
         var edgeInfo = new AcceptEdgeDelegateInformation<TElement>( enter, exit );

         if ( edgeID == count )
         {
            list.Add( edgeInfo );
         }
         else if ( edgeID < count )
         {
            list[edgeID] = edgeInfo;
         }
         else // id > list.Count
         {
            list.AddRange( Enumerable.Repeat<AcceptEdgeDelegateInformation<TElement>>( null, edgeID - count ) );
            list.Add( edgeInfo );
         }

      }

      public Boolean Accept( TElement element )
      {
         return this.Visitor.Visit( element, this._visitorInfo );
      }
   }

   public class TypeBasedAcceptor<TElement, TEdgeInfo, TContext> : AbstractTypeBasedAcceptor<TElement, TEdgeInfo>
   {
      private readonly AcceptorInformation<TElement, TContext> _acceptorInfo;
      private readonly ExplicitAcceptorInformation<TElement, TContext> _explicitAcceptorInfo;

      public TypeBasedAcceptor( TypeBasedVisitor<TElement, TEdgeInfo> visitor, TopMostTypeVisitingStrategy topMostVisitingStrategy, Boolean continueOnMissingVertex )
         : base( visitor )
      {
         this.VertexAcceptors = new Dictionary<Type, AcceptVertexDelegate<TElement, TContext>>().ToDictionaryProxy();
         this.ExplicitVertexAcceptors = new Dictionary<Type, AcceptVertexExplicitDelegate<TElement, TContext>>().ToDictionaryProxy();
         this.EdgeAcceptors = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<AcceptEdgeDelegateInformation<TElement, TContext>>();

         this._acceptorInfo = new AcceptorInformation<TElement, TContext>( topMostVisitingStrategy, continueOnMissingVertex, this.VertexAcceptors.CQ, this.EdgeAcceptors.CQ );
         this._explicitAcceptorInfo = new ExplicitAcceptorInformation<TElement, TContext>( topMostVisitingStrategy, continueOnMissingVertex, this.ExplicitVertexAcceptors.CQ, this.EdgeAcceptors.CQ );
      }

      private DictionaryProxy<Type, AcceptVertexDelegate<TElement, TContext>> VertexAcceptors { get; }

      private DictionaryProxy<Type, AcceptVertexExplicitDelegate<TElement, TContext>> ExplicitVertexAcceptors { get; }

      private ListProxy<AcceptEdgeDelegateInformation<TElement, TContext>> EdgeAcceptors { get; }

      public void RegisterVertexAcceptor( Type type, AcceptVertexDelegate<TElement, TContext> acceptor )
      {
         this.VertexAcceptors[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
      }

      public void RegisterExplicitVertexAcceptor( Type type, AcceptVertexExplicitDelegate<TElement, TContext> acceptor )
      {
         this.ExplicitVertexAcceptors[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
      }

      public void RegisterEdgeAcceptor( Type type, String edgeName, AcceptEdgeDelegate<TElement, TContext> enter, AcceptEdgeDelegate<TElement, TContext> exit )
      {
         var edgeID = this.Visitor.GetEdgeIDOrNegative( type, edgeName );
         if ( edgeID < 0 )
         {
            throw new ArgumentException( "No edge information found for " + type + "." + edgeName + "." );
         }
         var list = this.EdgeAcceptors;
         var count = list.CQ.Count;
         var edgeInfo = new AcceptEdgeDelegateInformation<TElement, TContext>( enter, exit );

         if ( edgeID == count )
         {
            list.Add( edgeInfo );
         }
         else if ( edgeID < count )
         {
            list[edgeID] = edgeInfo;
         }
         else // id > list.Count
         {
            list.AddRange( Enumerable.Repeat<AcceptEdgeDelegateInformation<TElement, TContext>>( null, edgeID - count ) );
            list.Add( edgeInfo );
         }
      }

      public Boolean Accept( TElement element, TContext context )
      {
         var info = this.Visitor.CreateVisitorInfo( this._acceptorInfo, context );
         return this.Visitor.Visit( element, info );
      }

      public Boolean AcceptExplicit( TElement element, TContext context )
      {
         return this.Visitor.VisitExplicit( element, this._explicitAcceptorInfo, context );
      }
   }

   public class AbstractAcceptorInformation<TVertexDelegate, TEdgeDelegate, TEdgeInfo>
      where TEdgeInfo : AbstractEdgeDelegateInformation<TEdgeDelegate>
   {
      public AbstractAcceptorInformation(
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, TVertexDelegate> vertexAcceptors,
         ListQuery<TEdgeInfo> edgeAcceptors
         )
      {
         this.TopMostVisitingStrategy = topMostVisitingStrategy;
         this.ContinueOnMissingVertex = continueOnMissingVertex;
         this.VertexAcceptors = ArgumentValidator.ValidateNotNull( "Vertex acceptors", vertexAcceptors );
         this.EdgeAcceptors = ArgumentValidator.ValidateNotNull( "Edge acceptors", edgeAcceptors );
      }

      public DictionaryQuery<Type, TVertexDelegate> VertexAcceptors { get; }

      public ListQuery<TEdgeInfo> EdgeAcceptors { get; }

      public TopMostTypeVisitingStrategy TopMostVisitingStrategy { get; }

      public Boolean ContinueOnMissingVertex { get; }
   }

   public class AcceptorInformation<TElement> : AbstractAcceptorInformation<AcceptVertexDelegate<TElement>, AcceptEdgeDelegate<TElement>, AcceptEdgeDelegateInformation<TElement>>
   {
      public AcceptorInformation(
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, AcceptVertexDelegate<TElement>> vertexAcceptors,
         ListQuery<AcceptEdgeDelegateInformation<TElement>> edgeAcceptors
         ) : base( topMostVisitingStrategy, continueOnMissingVertex, vertexAcceptors, edgeAcceptors )
      {
      }

   }

   public abstract class AbstractEdgeDelegateInformation<TDelegate>
   {
      public AbstractEdgeDelegateInformation( TDelegate entry, TDelegate exit )
      {
         this.Entry = entry;
         this.Exit = exit;
      }

      public TDelegate Entry { get; }
      public TDelegate Exit { get; }
   }

   public class AcceptEdgeDelegateInformation<TElement> : AbstractEdgeDelegateInformation<AcceptEdgeDelegate<TElement>>
   {
      public AcceptEdgeDelegateInformation( AcceptEdgeDelegate<TElement> entry, AcceptEdgeDelegate<TElement> exit )
         : base( entry, exit )
      {
      }
   }

   public class AcceptorInformation<TElement, TContext> : AbstractAcceptorInformation<AcceptVertexDelegate<TElement, TContext>, AcceptEdgeDelegate<TElement, TContext>, AcceptEdgeDelegateInformation<TElement, TContext>>
   {
      public AcceptorInformation(
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, AcceptVertexDelegate<TElement, TContext>> vertexAcceptors,
         ListQuery<AcceptEdgeDelegateInformation<TElement, TContext>> edgeAcceptors
         ) : base( topMostVisitingStrategy, continueOnMissingVertex, vertexAcceptors, edgeAcceptors )
      {
      }
   }

   public class AcceptEdgeDelegateInformation<TElement, TContext> : AbstractEdgeDelegateInformation<AcceptEdgeDelegate<TElement, TContext>>
   {
      public AcceptEdgeDelegateInformation( AcceptEdgeDelegate<TElement, TContext> entry, AcceptEdgeDelegate<TElement, TContext> exit )
         : base( entry, exit )
      {

      }
   }

   public class ExplicitAcceptorInformation<TElement, TContext> : AbstractAcceptorInformation<AcceptVertexExplicitDelegate<TElement, TContext>, AcceptEdgeDelegate<TElement, TContext>, AcceptEdgeDelegateInformation<TElement, TContext>>
   {
      public ExplicitAcceptorInformation(
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, AcceptVertexExplicitDelegate<TElement, TContext>> vertexAcceptors,
         ListQuery<AcceptEdgeDelegateInformation<TElement, TContext>> edgeAcceptors
         ) : base( topMostVisitingStrategy, continueOnMissingVertex, vertexAcceptors, edgeAcceptors )
      {

      }
   }
}

public static partial class E_UtilPack
{
   public static AcceptVertexExplicitDelegate<TElement, TContext> AsAcceptVertexExplicitDelegate<TElement, TContext, TActualElement>( this AcceptVertexExplicitDelegateTyped<TElement, TContext, TActualElement> typed )
      where TActualElement : TElement
   {
      return ( el, ctx, cb ) => typed( (TActualElement) el, ctx, cb );
   }

   public static void RegisterAcceptor<TElement, TEdgeInfo, TContext, TActualElement>( this TypeBasedAcceptor<TElement, TEdgeInfo, TContext> acceptorAggregator, AcceptVertexDelegate<TActualElement, TContext> acceptor )
      where TActualElement : TElement
   {
      acceptorAggregator.RegisterVertexAcceptor( typeof( TActualElement ), ( el, ctx ) => acceptor( (TActualElement) el, ctx ) );
   }
}