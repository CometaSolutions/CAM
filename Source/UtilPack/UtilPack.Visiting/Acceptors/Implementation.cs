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

#pragma warning disable 1591

namespace UtilPack.Visiting.Implementation
{
   internal abstract class AbstractAcceptor<TElement, TVisitor>
   where TVisitor : class
   {
      public AbstractAcceptor( TVisitor visitor )
      {
         this.Visitor = ArgumentValidator.ValidateNotNull( "Visitor", visitor );
      }

      public TVisitor Visitor { get; }
   }

   internal abstract class AbstractAutomaticAcceptor<TElement, TEdgeInfo> : AbstractAcceptor<TElement, TypeBasedVisitor<TElement, TEdgeInfo>>

   {
      internal AbstractAutomaticAcceptor( TypeBasedVisitor<TElement, TEdgeInfo> visitor )
         : base( visitor )
      {
      }

   }

   internal sealed class TypeBasedAcceptor<TElement, TEdgeInfo> : AbstractAutomaticAcceptor<TElement, TEdgeInfo>
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

   internal sealed class TypeBasedAcceptorWithResult<TElement, TResult> : AbstractAcceptor<TElement, ExplicitTypeBasedVisitor<TElement>>
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

      public TResult AcceptExplicit( TElement element, out Boolean success )
      {
         return this.Visitor.VisitExplicit( element, this._visitorInfo, out success );
      }
   }

   internal abstract class AbstractTypeBasedAcceptor<TElement, TEdgeInfo, TContext> : AbstractAutomaticAcceptor<TElement, TEdgeInfo>
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

   internal sealed class TypeBasedAcceptor<TElement, TEdgeInfo, TContext> : AbstractTypeBasedAcceptor<TElement, TEdgeInfo, TContext>
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

   internal abstract class AbstractCachingTypeBasedAcceptor<TElement, TEdgeInfo, TContext, TContextInitializer> : AbstractTypeBasedAcceptor<TElement, TEdgeInfo, TContext>
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

   internal sealed class CachingTypeBasedAcceptor<TElement, TEdgeInfo, TContext> : AbstractCachingTypeBasedAcceptor<TElement, TEdgeInfo, TContext, Action<TElement, TContext>>
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

   internal sealed class CachingTypeBasedAcceptor<TElement, TEdgeInfo, TContext, TAdditionalInfo> : AbstractCachingTypeBasedAcceptor<TElement, TEdgeInfo, TContext, Action<TElement, TContext, TAdditionalInfo>>
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



   internal abstract class AbstractAcceptorInformation<TVertexDelegate>
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

   internal abstract class AbstractImplicitAcceptorInformation<TVertexDelegate> : AbstractAcceptorInformation<TVertexDelegate>
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


   internal abstract class AbstractAcceptorInformation<TVertexDelegate, TEdgeDelegate, TEdgeInfo> : AbstractImplicitAcceptorInformation<TVertexDelegate>
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

   internal class AcceptorInformation<TElement, TEdgeInfo> : AbstractAcceptorInformation<AcceptVertexDelegate<TElement>, AcceptEdgeDelegate<TElement, TEdgeInfo>, AcceptEdgeDelegateInformation<TElement, TEdgeInfo>>
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

   internal abstract class AbstractEdgeDelegateInformation<TDelegate>
   {
      public AbstractEdgeDelegateInformation( TDelegate entry, TDelegate exit )
      {
         this.Entry = entry;
         this.Exit = exit;
      }

      public TDelegate Entry { get; }
      public TDelegate Exit { get; }
   }

   internal class AcceptEdgeDelegateInformation<TElement, TEdgeInfo> : AbstractEdgeDelegateInformation<AcceptEdgeDelegate<TElement, TEdgeInfo>>
   {
      public AcceptEdgeDelegateInformation( AcceptEdgeDelegate<TElement, TEdgeInfo> entry, AcceptEdgeDelegate<TElement, TEdgeInfo> exit )
         : base( entry, exit )
      {
      }
   }

   internal class AcceptorInformation<TElement, TEdgeInfo, TContext> : AbstractAcceptorInformation<AcceptVertexDelegate<TElement, TContext>, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext>, AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>>
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

   internal class AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext> : AbstractEdgeDelegateInformation<AcceptEdgeDelegate<TElement, TEdgeInfo, TContext>>
   {
      public AcceptEdgeDelegateInformation( AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> entry, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> exit )
         : base( entry, exit )
      {

      }
   }

   internal class ExplicitAcceptorInformation<TElement, TContext> : AbstractAcceptorInformation<AcceptVertexExplicitDelegate<TElement, TContext>>
   {
      public ExplicitAcceptorInformation(
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, AcceptVertexExplicitDelegate<TElement, TContext>> vertexAcceptors
         ) : base( continueOnMissingVertex, vertexAcceptors )
      {

      }
   }

   internal class ExplicitAcceptorInformationWithResult<TElement, TResult> : AbstractAcceptorInformation<AcceptVertexExplicitWithResultDelegate<TElement, TResult>>
   {
      public ExplicitAcceptorInformationWithResult(
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, AcceptVertexExplicitWithResultDelegate<TElement, TResult>> vertexAcceptors
         ) : base( continueOnMissingVertex, vertexAcceptors )
      {

      }
   }

}
