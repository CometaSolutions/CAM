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

   public delegate Boolean AcceptEdgeDelegate<in TElement, in TEdgeInfo>( TElement element, TEdgeInfo edgeInfo );

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
   /// <typeparam name="TEdgeInfo">The type of edge info (typically <see cref="Int32"/> indicating list/array index in case of list/array property).</typeparam>
   /// <param name="element">The current element.</param>
   /// <param name="edgeInfo">The domain-specific edge information.</param>
   /// <param name="context">The current context.</param>
   /// <returns><c>true</c> if visiting should be continued, <c>false</c> otherwise.</returns>
   public delegate Boolean AcceptEdgeDelegate<in TElement, in TEdgeInfo, in TContext>( TElement element, TEdgeInfo edgeInfo, TContext context );

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

      public TypeBasedAcceptor( TypeBasedVisitor<TElement, TEdgeInfo> visitor, TopMostTypeVisitingStrategy topMostVisitingStrategy, Boolean continueOnMissingVertex )
         : base( visitor )
      {
         this.VertexAcceptors = new Dictionary<Type, AcceptVertexDelegate<TElement>>().ToDictionaryProxy();
         this.EdgeAcceptors = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<AcceptEdgeDelegateInformation<TElement, TEdgeInfo>>();
         this._visitorInfo = visitor.CreateVisitorInfo( new AcceptorInformation<TElement, TEdgeInfo>( topMostVisitingStrategy, continueOnMissingVertex, this.VertexAcceptors.CQ, this.EdgeAcceptors.CQ ) );
      }

      private DictionaryProxy<Type, AcceptVertexDelegate<TElement>> VertexAcceptors { get; }

      private ListProxy<AcceptEdgeDelegateInformation<TElement, TEdgeInfo>> EdgeAcceptors { get; }

      public void RegisterVertexAcceptor( Type type, AcceptVertexDelegate<TElement> acceptor )
      {
         this.VertexAcceptors[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
      }

      public void RegisterEdgeAcceptor( Type type, String edgeName, AcceptEdgeDelegate<TElement, TEdgeInfo> enter, AcceptEdgeDelegate<TElement, TEdgeInfo> exit )
      {
         var edgeID = this.Visitor.GetEdgeIDOrNegative( type, edgeName );
         if ( edgeID < 0 )
         {
            throw new ArgumentException( "No edge information found for " + type + "." + edgeName + "." );
         }
         var list = this.EdgeAcceptors;
         var count = list.CQ.Count;
         var edgeInfo = new AcceptEdgeDelegateInformation<TElement, TEdgeInfo>( enter, exit );

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
            list.AddRange( Enumerable.Repeat<AcceptEdgeDelegateInformation<TElement, TEdgeInfo>>( null, edgeID - count ) );
            list.Add( edgeInfo );
         }

      }

      public Boolean Accept( TElement element )
      {
         return this.Visitor.Visit( element, this._visitorInfo );
      }
   }

   public sealed class TypeBasedAcceptor<TElement, TEdgeInfo, TContext> : AbstractTypeBasedAcceptor<TElement, TEdgeInfo>
   {

      public TypeBasedAcceptor(
         TypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex
         )
         : this( visitor, topMostVisitingStrategy, continueOnMissingVertex, null )
      {

      }

      public TypeBasedAcceptor(
         TypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         Action<TElement, TContext> contextInitializer
         )
         : base( visitor )
      {
         this.VertexAcceptors = new Dictionary<Type, AcceptVertexDelegate<TElement, TContext>>().ToDictionaryProxy();
         this.ExplicitVertexAcceptors = new Dictionary<Type, AcceptVertexExplicitDelegate<TElement, TContext>>().ToDictionaryProxy();
         this.EdgeAcceptors = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>>();

         this.AcceptorInfo = new AcceptorInformation<TElement, TEdgeInfo, TContext>( topMostVisitingStrategy, continueOnMissingVertex, this.VertexAcceptors.CQ, this.EdgeAcceptors.CQ );
         this.ExplicitAcceptorInfo = new ExplicitAcceptorInformation<TElement, TContext>( topMostVisitingStrategy, continueOnMissingVertex, this.ExplicitVertexAcceptors.CQ );
         if ( contextInitializer != null )
         {
            this.VisitorInfoPool = new LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<VisitorInformation<TElement, TEdgeInfo, TContext>>>();
            this.ContextInitializer = contextInitializer;
         }
      }

      private DictionaryProxy<Type, AcceptVertexDelegate<TElement, TContext>> VertexAcceptors { get; }

      private DictionaryProxy<Type, AcceptVertexExplicitDelegate<TElement, TContext>> ExplicitVertexAcceptors { get; }

      private ListProxy<AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>> EdgeAcceptors { get; }

      private AcceptorInformation<TElement, TEdgeInfo, TContext> AcceptorInfo { get; }

      private ExplicitAcceptorInformation<TElement, TContext> ExplicitAcceptorInfo { get; }

      private Action<TElement, TContext> ContextInitializer { get; }

      private Boolean CacheVisitorInfos { get; }

      private LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<VisitorInformation<TElement, TEdgeInfo, TContext>>> VisitorInfoPool { get; }

      public void RegisterVertexAcceptor( Type type, AcceptVertexDelegate<TElement, TContext> acceptor )
      {
         this.VertexAcceptors[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
      }

      public void RegisterExplicitVertexAcceptor( Type type, AcceptVertexExplicitDelegate<TElement, TContext> acceptor )
      {
         this.ExplicitVertexAcceptors[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
      }

      public void RegisterEdgeAcceptor( Type type, String edgeName, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> enter, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> exit )
      {
         var edgeID = this.Visitor.GetEdgeIDOrNegative( type, edgeName );
         if ( edgeID < 0 )
         {
            throw new ArgumentException( "No edge information found for " + type + "." + edgeName + "." );
         }
         var list = this.EdgeAcceptors;
         var count = list.CQ.Count;
         var edgeInfo = new AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>( enter, exit );

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
            list.AddRange( Enumerable.Repeat<AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>>( null, edgeID - count ) );
            list.Add( edgeInfo );
         }
      }

      public Boolean Accept( TElement element, TContext context )
      {
         Boolean retVal;
         if ( this.CacheVisitorInfos )
         {
            var instance = this.VisitorInfoPool.TakeInstance();
            try
            {
               if ( instance == null )
               {
                  instance = new InstanceHolder<VisitorInformation<TElement, TEdgeInfo, TContext>>( this.Visitor.CreateVisitorInfo( this.AcceptorInfo, context ) );
               }
               var info = instance.Instance;
               this.ContextInitializer( element, info.Context );
               retVal = this.Visitor.Visit( element, info );
            }
            finally
            {
               this.VisitorInfoPool.ReturnInstance( instance );
            }
         }
         else
         {
            retVal = this.Visitor.Visit( element, this.Visitor.CreateVisitorInfo( this.AcceptorInfo, context ) );
         }
         return retVal;
      }

      public Boolean AcceptExplicit( TElement element, TContext context )
      {
         var info = this.Visitor.CreateExplicitVisitorInfo( this.ExplicitAcceptorInfo, context );
         return this.Visitor.VisitExplicit( element, info );
      }

      private Boolean Accept( TElement element, TContext context, VisitorInformation<TElement, TEdgeInfo, TContext> visitorInfo )
      {
         return this.Visitor.Visit( element, visitorInfo );
      }
   }


   public abstract class AbstractAcceptorInformation<TVertexDelegate>
   {

      public AbstractAcceptorInformation(
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, TVertexDelegate> vertexAcceptors
         )
      {
         this.TopMostVisitingStrategy = topMostVisitingStrategy;
         this.ContinueOnMissingVertex = continueOnMissingVertex;
         this.VertexAcceptors = ArgumentValidator.ValidateNotNull( "Vertex acceptors", vertexAcceptors );
      }

      public DictionaryQuery<Type, TVertexDelegate> VertexAcceptors { get; }

      public TopMostTypeVisitingStrategy TopMostVisitingStrategy { get; }

      public Boolean ContinueOnMissingVertex { get; }
   }

   public abstract class AbstractAcceptorInformation<TVertexDelegate, TEdgeDelegate, TEdgeInfo> : AbstractAcceptorInformation<TVertexDelegate>
      where TEdgeInfo : AbstractEdgeDelegateInformation<TEdgeDelegate>
   {
      public AbstractAcceptorInformation(
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, TVertexDelegate> vertexAcceptors,
         ListQuery<TEdgeInfo> edgeAcceptors
         )
         : base( topMostVisitingStrategy, continueOnMissingVertex, vertexAcceptors )
      {
         this.EdgeAcceptors = ArgumentValidator.ValidateNotNull( "Edge acceptors", edgeAcceptors );
      }

      public ListQuery<TEdgeInfo> EdgeAcceptors { get; }
   }

   public class AcceptorInformation<TElement, TEdgeInfo> : AbstractAcceptorInformation<AcceptVertexDelegate<TElement>, AcceptEdgeDelegate<TElement, TEdgeInfo>, AcceptEdgeDelegateInformation<TElement, TEdgeInfo>>
   {
      public AcceptorInformation(
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, AcceptVertexDelegate<TElement>> vertexAcceptors,
         ListQuery<AcceptEdgeDelegateInformation<TElement, TEdgeInfo>> edgeAcceptors
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

   public class AcceptEdgeDelegateInformation<TElement, TEdgeInfo> : AbstractEdgeDelegateInformation<AcceptEdgeDelegate<TElement, TEdgeInfo>>
   {
      public AcceptEdgeDelegateInformation( AcceptEdgeDelegate<TElement, TEdgeInfo> entry, AcceptEdgeDelegate<TElement, TEdgeInfo> exit )
         : base( entry, exit )
      {
      }
   }

   public class AcceptorInformation<TElement, TEdgeInfo, TContext> : AbstractAcceptorInformation<AcceptVertexDelegate<TElement, TContext>, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext>, AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>>
   {
      public AcceptorInformation(
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, AcceptVertexDelegate<TElement, TContext>> vertexAcceptors,
         ListQuery<AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>> edgeAcceptors
         ) : base( topMostVisitingStrategy, continueOnMissingVertex, vertexAcceptors, edgeAcceptors )
      {
      }
   }

   public class AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext> : AbstractEdgeDelegateInformation<AcceptEdgeDelegate<TElement, TEdgeInfo, TContext>>
   {
      public AcceptEdgeDelegateInformation( AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> entry, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> exit )
         : base( entry, exit )
      {

      }
   }

   public class ExplicitAcceptorInformation<TElement, TContext> : AbstractAcceptorInformation<AcceptVertexExplicitDelegate<TElement, TContext>>
   {
      public ExplicitAcceptorInformation(
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, AcceptVertexExplicitDelegate<TElement, TContext>> vertexAcceptors
         ) : base( topMostVisitingStrategy, continueOnMissingVertex, vertexAcceptors )
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