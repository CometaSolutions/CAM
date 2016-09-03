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
using UtilPack;
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

   public delegate void AcceptVertexExplicitDelegate<TElement, TContext>( TElement element, TContext context, AcceptVertexExplicitCallbackDelegate<TElement, TContext> acceptor );

   public delegate Boolean AcceptVertexExplicitCallbackDelegate<in TElement, in TContext>( TElement element, TContext context );

   public delegate void AcceptVertexExplicitDelegateTyped<TElement, TContext, in TActualElement>( TActualElement element, TContext context, AcceptVertexExplicitCallbackDelegate<TElement, TContext> acceptor )
      where TActualElement : TElement;


   //public delegate TResult AcceptVertexWithResultDelegate<in TElement, out TResult>( TElement element, out Boolean shouldContinue );

   //public delegate TResult AcceptVertexWithResultDelegate<in TElement, in TContext, out TResult>( TElement element, TContext context, out Boolean shouldContinue );

   public delegate TResult AcceptVertexExplicitWithResultDelegate<TElement, TResult>( TElement element, AcceptVertexExplicitCallbackWithResultDelegate<TElement, TResult> acceptor );

   public delegate TResult AcceptVertexExplicitCallbackWithResultDelegate<in TElement, out TResult>( TElement element );

   public enum TopMostTypeVisitingStrategy
   {
      Never,
      IfNotOverridingType,
      Always
   }

   public abstract class AbstractAcceptor<TElement, TVisitor>
      where TVisitor : class
   {
      public AbstractAcceptor( TVisitor visitor )
      {
         this.Visitor = ArgumentValidator.ValidateNotNull( "Visitor", visitor );
      }

      public TVisitor Visitor { get; }
   }

   public abstract class AbstractTypeBasedAcceptor<TElement, TEdgeInfo> : AbstractAcceptor<TElement, TypeBasedVisitor<TElement, TEdgeInfo>>

   {
      internal AbstractTypeBasedAcceptor( TypeBasedVisitor<TElement, TEdgeInfo> visitor )
         : base( visitor )
      {
      }

   }

   public sealed class TypeBasedAcceptor<TElement, TEdgeInfo> : AbstractTypeBasedAcceptor<TElement, TEdgeInfo>
   {
      private readonly VisitorInformation<TElement, TEdgeInfo> _visitorInfo;

      public TypeBasedAcceptor( TypeBasedVisitor<TElement, TEdgeInfo> visitor, TopMostTypeVisitingStrategy topMostVisitingStrategy, Boolean continueOnMissingVertex )
         : base( visitor )
      {
         this.VertexAcceptors = new Dictionary<Type, AcceptVertexDelegate<TElement>>().ToDictionaryProxy();
         this.EdgeAcceptors = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<AcceptEdgeDelegateInformation<TElement, TEdgeInfo>>();
         this._visitorInfo = visitor.CreateVisitorInfo( new AcceptorInformation<TElement, TEdgeInfo>( continueOnMissingVertex, this.VertexAcceptors.CQ, topMostVisitingStrategy, this.EdgeAcceptors.CQ ) );
      }

      private DictionaryProxy<Type, AcceptVertexDelegate<TElement>> VertexAcceptors { get; }

      private ListProxy<AcceptEdgeDelegateInformation<TElement, TEdgeInfo>> EdgeAcceptors { get; }

      public void RegisterVertexAcceptor( Type type, AcceptVertexDelegate<TElement> acceptor )
      {
         this.VertexAcceptors[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
      }

      public void RegisterEdgeAcceptor( Int32 edgeID, AcceptEdgeDelegate<TElement, TEdgeInfo> enter, AcceptEdgeDelegate<TElement, TEdgeInfo> exit )
      {
         if ( edgeID < 0 )
         {
            throw new ArgumentException( "Edge ID must be at least zero." );
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

   public sealed class TypeBasedAcceptorWithResult<TElement, TResult> : AbstractAcceptor<TElement, ExplicitTypeBasedVisitor<TElement>>
   {
      private readonly ExplicitVisitorInformationWithResult<TElement, TResult> _visitorInfo;

      public TypeBasedAcceptorWithResult( ExplicitTypeBasedVisitor<TElement> visitor, Boolean continueOnMissingVertex )
         : base( visitor )
      {
         this.VertexAcceptors = new Dictionary<Type, AcceptVertexExplicitWithResultDelegate<TElement, TResult>>().ToDictionaryProxy();
         this._visitorInfo = visitor.CreateExplicitVisitorInfo( new ExplicitAcceptorInformationWithResult<TElement, TResult>( continueOnMissingVertex, this.VertexAcceptors.CQ ) );
      }

      private DictionaryProxy<Type, AcceptVertexExplicitWithResultDelegate<TElement, TResult>> VertexAcceptors { get; }

      public void RegisterVertexAcceptor( Type type, AcceptVertexExplicitWithResultDelegate<TElement, TResult> acceptor )
      {
         this.VertexAcceptors[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
      }

      public Boolean AcceptExplicit( TElement element, out TResult result )
      {
         return this.Visitor.VisitExplicit( element, this._visitorInfo, out result );
      }
   }

   public abstract class AbstractTypeBasedAcceptor<TElement, TEdgeInfo, TContext> : AbstractTypeBasedAcceptor<TElement, TEdgeInfo>
   {
      internal AbstractTypeBasedAcceptor(
         TypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex
         )
         : base( visitor )
      {
         this.VertexAcceptors = new Dictionary<Type, AcceptVertexDelegate<TElement, TContext>>().ToDictionaryProxy();
         this.ExplicitVertexAcceptors = new Dictionary<Type, AcceptVertexExplicitDelegate<TElement, TContext>>().ToDictionaryProxy();
         this.EdgeAcceptors = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>>();

         this.AcceptorInfo = new AcceptorInformation<TElement, TEdgeInfo, TContext>( continueOnMissingVertex, this.VertexAcceptors.CQ, topMostVisitingStrategy, this.EdgeAcceptors.CQ );
         this.ExplicitAcceptorInfo = new ExplicitAcceptorInformation<TElement, TContext>( continueOnMissingVertex, this.ExplicitVertexAcceptors.CQ );
      }

      private DictionaryProxy<Type, AcceptVertexDelegate<TElement, TContext>> VertexAcceptors { get; }

      private DictionaryProxy<Type, AcceptVertexExplicitDelegate<TElement, TContext>> ExplicitVertexAcceptors { get; }

      private ListProxy<AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>> EdgeAcceptors { get; }

      protected AcceptorInformation<TElement, TEdgeInfo, TContext> AcceptorInfo { get; }

      protected ExplicitAcceptorInformation<TElement, TContext> ExplicitAcceptorInfo { get; }

      public void RegisterVertexAcceptor( Type type, AcceptVertexDelegate<TElement, TContext> acceptor )
      {
         this.VertexAcceptors[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
      }

      public void RegisterExplicitVertexAcceptor( Type type, AcceptVertexExplicitDelegate<TElement, TContext> acceptor )
      {
         this.ExplicitVertexAcceptors[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
      }

      public void RegisterEdgeAcceptor( Int32 edgeID, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> enter, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> exit )
      {
         if ( edgeID < 0 )
         {
            throw new ArgumentException( "Edge ID must be at least zero." );
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
   }

   public sealed class TypeBasedAcceptor<TElement, TEdgeInfo, TContext> : AbstractTypeBasedAcceptor<TElement, TEdgeInfo, TContext>
   {

      public TypeBasedAcceptor(
         TypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex
         )
         : base( visitor, topMostVisitingStrategy, continueOnMissingVertex )
      {
      }

      public Boolean Accept( TElement element, TContext context )
      {
         return this.Visitor.Visit( element, this.Visitor.CreateVisitorInfo( this.AcceptorInfo, context ) );
      }

      public Boolean AcceptExplicit( TElement element, TContext context )
      {
         return this.Visitor.VisitExplicit( element, this.Visitor.CreateExplicitVisitorInfo( this.ExplicitAcceptorInfo, context ) );
      }


   }

   public abstract class AbstractCachingTypeBasedAcceptor<TElement, TEdgeInfo, TContext, TContextInitializer> : AbstractTypeBasedAcceptor<TElement, TEdgeInfo, TContext>
      where TContextInitializer : class
   {
      internal AbstractCachingTypeBasedAcceptor(
         TypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         Func<TContext> contextFactory,
         TContextInitializer contextInitializer
         ) : base( visitor, topMostVisitingStrategy, continueOnMissingVertex )
      {
         this.ContextFactory = ArgumentValidator.ValidateNotNull( "Context factory", contextFactory );
         this.ContextInitializer = ArgumentValidator.ValidateNotNull( "Context initializer", contextInitializer );
      }

      protected Func<TContext> ContextFactory { get; }

      protected TContextInitializer ContextInitializer { get; }


   }

   public sealed class CachingTypeBasedAcceptor<TElement, TEdgeInfo, TContext> : AbstractCachingTypeBasedAcceptor<TElement, TEdgeInfo, TContext, Action<TElement, TContext>>
   {

      private readonly LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<VisitorInformation<TElement, TEdgeInfo, TContext>>> _visitorInfoPool;
      private readonly LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<ExplicitVisitorInformation<TElement, TContext>>> _explicitVisitorInfoPool;

      public CachingTypeBasedAcceptor(
         TypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         Func<TContext> contextFactory,
         Action<TElement, TContext> contextInitializer
         ) : base( visitor, topMostVisitingStrategy, continueOnMissingVertex, contextFactory, contextInitializer )
      {
         this._visitorInfoPool = new LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<VisitorInformation<TElement, TEdgeInfo, TContext>>>();
         this._explicitVisitorInfoPool = new LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<ExplicitVisitorInformation<TElement, TContext>>>();
      }

      public Boolean Accept( TElement element )
      {
         var pool = this._visitorInfoPool;
         var instance = pool.TakeInstance();
         try
         {
            if ( instance == null )
            {
               instance = new InstanceHolder<VisitorInformation<TElement, TEdgeInfo, TContext>>( this.Visitor.CreateVisitorInfo( this.AcceptorInfo, this.ContextFactory() ) );
            }
            var info = instance.Instance;
            this.ContextInitializer( element, info.Context );
            return this.Visitor.Visit( element, info );
         }
         finally
         {
            pool.ReturnInstance( instance );
         }
      }

      public Boolean AcceptExplicit( TElement element )
      {
         var pool = this._explicitVisitorInfoPool;
         var instance = pool.TakeInstance();
         try
         {
            if ( instance == null )
            {
               instance = new InstanceHolder<ExplicitVisitorInformation<TElement, TContext>>( this.Visitor.CreateExplicitVisitorInfo( this.ExplicitAcceptorInfo, this.ContextFactory() ) );
            }
            var info = instance.Instance;
            this.ContextInitializer( element, info.Context );
            return this.Visitor.VisitExplicit( element, info );
         }
         finally
         {
            pool.ReturnInstance( instance );
         }
      }
   }

   public sealed class CachingTypeBasedAcceptor<TElement, TEdgeInfo, TContext, TAdditionalInfo> : AbstractCachingTypeBasedAcceptor<TElement, TEdgeInfo, TContext, Action<TElement, TContext, TAdditionalInfo>>
   {
      private readonly LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<VisitorInformation<TElement, TEdgeInfo, TContext>>> _visitorInfoPool;
      private readonly LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<ExplicitVisitorInformation<TElement, TContext>>> _explicitVisitorInfoPool;

      public CachingTypeBasedAcceptor(
         TypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         Func<TContext> contextFactory,
         Action<TElement, TContext, TAdditionalInfo> contextInitializer
         ) : base( visitor, topMostVisitingStrategy, continueOnMissingVertex, contextFactory, contextInitializer )
      {
         this._visitorInfoPool = new LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<VisitorInformation<TElement, TEdgeInfo, TContext>>>();
         this._explicitVisitorInfoPool = new LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<ExplicitVisitorInformation<TElement, TContext>>>();
      }

      public Boolean Accept( TElement element, TAdditionalInfo additionalInfo )
      {
         var pool = this._visitorInfoPool;
         var instance = pool.TakeInstance();
         try
         {
            if ( instance == null )
            {
               instance = new InstanceHolder<VisitorInformation<TElement, TEdgeInfo, TContext>>( this.Visitor.CreateVisitorInfo( this.AcceptorInfo, this.ContextFactory() ) );
            }
            var info = instance.Instance;
            this.ContextInitializer( element, info.Context, additionalInfo );
            return this.Visitor.Visit( element, info );
         }
         finally
         {
            pool.ReturnInstance( instance );
         }
      }

      public Boolean AcceptExplicit( TElement element, TAdditionalInfo additionalInfo )
      {
         var pool = this._explicitVisitorInfoPool;
         var instance = pool.TakeInstance();
         try
         {
            if ( instance == null )
            {
               instance = new InstanceHolder<ExplicitVisitorInformation<TElement, TContext>>( this.Visitor.CreateExplicitVisitorInfo( this.ExplicitAcceptorInfo, this.ContextFactory() ) );
            }
            var info = instance.Instance;
            this.ContextInitializer( element, info.Context, additionalInfo );
            return this.Visitor.VisitExplicit( element, info );
         }
         finally
         {
            pool.ReturnInstance( instance );
         }
      }
   }



   public abstract class AbstractAcceptorInformation<TVertexDelegate>
   {

      public AbstractAcceptorInformation(
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, TVertexDelegate> vertexAcceptors
         )
      {
         this.ContinueOnMissingVertex = continueOnMissingVertex;
         this.VertexAcceptors = ArgumentValidator.ValidateNotNull( "Vertex acceptors", vertexAcceptors );
      }

      public DictionaryQuery<Type, TVertexDelegate> VertexAcceptors { get; }

      public Boolean ContinueOnMissingVertex { get; }
   }

   public abstract class AbstractImplicitAcceptorInformation<TVertexDelegate> : AbstractAcceptorInformation<TVertexDelegate>
   {
      public AbstractImplicitAcceptorInformation(
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, TVertexDelegate> vertexAcceptors,
         TopMostTypeVisitingStrategy topMostVisitingStrategy
         ) : base( continueOnMissingVertex, vertexAcceptors )
      {
         this.TopMostVisitingStrategy = topMostVisitingStrategy;
      }

      public TopMostTypeVisitingStrategy TopMostVisitingStrategy { get; }
   }


   public abstract class AbstractAcceptorInformation<TVertexDelegate, TEdgeDelegate, TEdgeInfo> : AbstractImplicitAcceptorInformation<TVertexDelegate>
      where TEdgeInfo : AbstractEdgeDelegateInformation<TEdgeDelegate>
   {
      public AbstractAcceptorInformation(
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, TVertexDelegate> vertexAcceptors,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         ListQuery<TEdgeInfo> edgeAcceptors
         )
         : base( continueOnMissingVertex, vertexAcceptors, topMostVisitingStrategy )
      {
         this.EdgeAcceptors = ArgumentValidator.ValidateNotNull( "Edge acceptors", edgeAcceptors );
      }

      public ListQuery<TEdgeInfo> EdgeAcceptors { get; }
   }

   public class AcceptorInformation<TElement, TEdgeInfo> : AbstractAcceptorInformation<AcceptVertexDelegate<TElement>, AcceptEdgeDelegate<TElement, TEdgeInfo>, AcceptEdgeDelegateInformation<TElement, TEdgeInfo>>
   {
      public AcceptorInformation(
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, AcceptVertexDelegate<TElement>> vertexAcceptors,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         ListQuery<AcceptEdgeDelegateInformation<TElement, TEdgeInfo>> edgeAcceptors
         ) : base( continueOnMissingVertex, vertexAcceptors, topMostVisitingStrategy, edgeAcceptors )
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
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, AcceptVertexDelegate<TElement, TContext>> vertexAcceptors,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         ListQuery<AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>> edgeAcceptors
         ) : base( continueOnMissingVertex, vertexAcceptors, topMostVisitingStrategy, edgeAcceptors )
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
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, AcceptVertexExplicitDelegate<TElement, TContext>> vertexAcceptors
         ) : base( continueOnMissingVertex, vertexAcceptors )
      {

      }
   }

   public class ExplicitAcceptorInformationWithResult<TElement, TResult> : AbstractAcceptorInformation<AcceptVertexExplicitWithResultDelegate<TElement, TResult>>
   {
      public ExplicitAcceptorInformationWithResult(
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, AcceptVertexExplicitWithResultDelegate<TElement, TResult>> vertexAcceptors
         ) : base( continueOnMissingVertex, vertexAcceptors )
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

   public static void RegisterEdgeAcceptor<TElement, TEdgeInfo>( this TypeBasedAcceptor<TElement, TEdgeInfo> acceptor, Type type, String edgeName, AcceptEdgeDelegate<TElement, TEdgeInfo> enter, AcceptEdgeDelegate<TElement, TEdgeInfo> exit )
   {
      acceptor.RegisterEdgeAcceptor( acceptor.Visitor.GetEdgeIDOrThrow( type, edgeName ), enter, exit );
   }

   public static void RegisterEdgeAcceptor<TElement, TEdgeInfo, TContext>( this AbstractTypeBasedAcceptor<TElement, TEdgeInfo, TContext> acceptor, Type type, String edgeName, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> enter, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> exit )
   {
      acceptor.RegisterEdgeAcceptor( acceptor.Visitor.GetEdgeIDOrThrow( type, edgeName ), enter, exit );
   }

   public static TResult AcceptExplicit<TElement, TResult>( this TypeBasedAcceptorWithResult<TElement, TResult> acceptor, TElement element )
   {
      TResult retVal;
      acceptor.AcceptExplicit( element, out retVal );
      return retVal;
   }


}