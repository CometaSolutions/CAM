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
   internal abstract class AbstractAcceptor<TAcceptor, TElement, TVisitor, TVertexDelegate> : AcceptorSetup<TAcceptor, TVisitor, TVertexDelegate>
      where TVertexDelegate : class
      where TVisitor : class
   {
      private readonly DictionaryProxy<Type, TVertexDelegate> _vertexAcceptors;

      public AbstractAcceptor( TVisitor visitor )
      {
         this.Visitor = ArgumentValidator.ValidateNotNull( "Visitor", visitor );
         this._vertexAcceptors = new Dictionary<Type, TVertexDelegate>().ToDictionaryProxy();
      }

      public TVisitor Visitor { get; }

      public abstract TAcceptor Acceptor { get; }

      protected DictionaryQuery<Type, TVertexDelegate> VertexAcceptors
      {
         get
         {
            return this._vertexAcceptors.CQ;
         }
      }

      public void RegisterVertexAcceptor( Type type, TVertexDelegate acceptor )
      {
         this._vertexAcceptors[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
      }
   }

   internal abstract class AbstractAutomaticAcceptor<TAcceptor, TElement, TVertexDelegate, TEdgeInfo, TEdgeDelegate, TEdgeDelegateInfo> : AbstractAcceptor<TAcceptor, TElement, TypeBasedVisitor<TElement, TEdgeInfo>, TVertexDelegate>, AcceptorSetup<TAcceptor, TypeBasedVisitor<TElement, TEdgeInfo>, TVertexDelegate, TEdgeDelegate>
      where TVertexDelegate : class
      where TEdgeDelegateInfo : class
   {
      private readonly ListProxy<TEdgeDelegateInfo> _edgeAcceptors;

      internal AbstractAutomaticAcceptor( TypeBasedVisitor<TElement, TEdgeInfo> visitor )
         : base( visitor )
      {
         this._edgeAcceptors = new List<TEdgeDelegateInfo>().ToListProxy();
      }

      protected ListQuery<TEdgeDelegateInfo> EdgeAcceptors
      {
         get
         {
            return this._edgeAcceptors.CQ;
         }
      }

      public void RegisterEdgeAcceptor( Int32 edgeID, TEdgeDelegate enter, TEdgeDelegate exit )
      {
         if ( edgeID < 0 )
         {
            throw new ArgumentException( "Edge ID must be at least zero." );
         }

         var list = this._edgeAcceptors;
         var count = list.CQ.Count;
         var edgeInfo = this.GetInfoFromDelegate( enter, exit );

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
            list.AddRange( Enumerable.Repeat<TEdgeDelegateInfo>( null, edgeID - count ) );
            list.Add( edgeInfo );
         }

      }

      protected abstract TEdgeDelegateInfo GetInfoFromDelegate( TEdgeDelegate enter, TEdgeDelegate exit );

   }

   internal abstract class AbstractManualAcceptor<TAcceptor, TElement, TVertexDelegate> : AbstractAcceptor<TAcceptor, TElement, ExplicitTypeBasedVisitor<TElement>, TVertexDelegate>
      where TVertexDelegate : class
   {
      public AbstractManualAcceptor( ExplicitTypeBasedVisitor<TElement> visitor )
         : base( visitor )
      {

      }
   }

   internal sealed class AutomaticTransitionAcceptor_NoContextImpl<TElement, TEdgeInfo> : AbstractAutomaticAcceptor<Acceptor<TElement>, TElement, AcceptVertexDelegate<TElement>, TEdgeInfo, AcceptEdgeDelegate<TElement, TEdgeInfo>, AcceptEdgeDelegateInformation<TElement, TEdgeInfo>>, AutomaticTransitionAcceptor_NoContext<Acceptor<TElement>, TElement, TEdgeInfo>, Acceptor<TElement>
   {
      private readonly AutomaticVisitorInformation<TElement, TEdgeInfo> _visitorInfo;

      public AutomaticTransitionAcceptor_NoContextImpl(
         TypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex
         )
         : base( visitor )
      {
         this._visitorInfo = visitor.CreateVisitorInfo( new AcceptorInformation<TElement, TEdgeInfo>( continueOnMissingVertex, this.VertexAcceptors, topMostVisitingStrategy, this.EdgeAcceptors ) );
      }

      protected override AcceptEdgeDelegateInformation<TElement, TEdgeInfo> GetInfoFromDelegate( AcceptEdgeDelegate<TElement, TEdgeInfo> enter, AcceptEdgeDelegate<TElement, TEdgeInfo> exit )
      {
         return new AcceptEdgeDelegateInformation<TElement, TEdgeInfo>( enter, exit );
      }

      public override Acceptor<TElement> Acceptor
      {
         get
         {
            return this;
         }
      }


      public Boolean Accept( TElement element )
      {
         return this.Visitor.Visit( element, this._visitorInfo );
      }


   }

   internal sealed class AutomaticTransitionAcceptor_WithContextImpl<TElement, TEdgeInfo, TContext> : AbstractAutomaticAcceptor<AcceptorWithContext<TElement, TContext>, TElement, AcceptVertexDelegate<TElement, TContext>, TEdgeInfo, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext>, AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>>, AutomaticTransitionAcceptor_WithContext<AcceptorWithContext<TElement, TContext>, TElement, TEdgeInfo, TContext>, AcceptorWithContext<TElement, TContext>
   {
      private readonly AcceptorInformation<TElement, TEdgeInfo, TContext> _acceptorInfo;

      public AutomaticTransitionAcceptor_WithContextImpl(
         TypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex
         )
         : base( visitor )
      {
         this._acceptorInfo = new AcceptorInformation<TElement, TEdgeInfo, TContext>( continueOnMissingVertex, this.VertexAcceptors, topMostVisitingStrategy, this.EdgeAcceptors );
      }

      public Boolean Accept( TElement element, TContext context )
      {
         return this.Visitor.Visit( element, this.Visitor.CreateVisitorInfo( this._acceptorInfo, context ) );
      }

      public override AcceptorWithContext<TElement, TContext> Acceptor
      {
         get
         {
            return this;
         }
      }

      protected override AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext> GetInfoFromDelegate( AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> enter, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> exit )
      {
         return new AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>( enter, exit );
      }
   }

   //internal sealed class AutomaticTransitionAcceptor_WithReturnValueImpl<TElement, TEdgeInfo, TResult> : AbstractAutomaticAcceptor<TElement, AcceptVertexWithResultDelegate<TElement, TResult>, TEdgeInfo, AcceptEdgeDelegate<TElement, TEdgeInfo>, AcceptEdgeDelegateInformation<TElement, TEdgeInfo>>, AutomaticTransitionAcceptor_WithReturnValue<TElement, TEdgeInfo, TResult>
   //{
   //   private readonly VisitorInformationWithResult<TElement, TEdgeInfo, TResult> _visitorInfo;

   //   public AutomaticTransitionAcceptor_WithReturnValueImpl(
   //      TypeBasedVisitor<TElement, TEdgeInfo> visitor,
   //      TopMostTypeVisitingStrategy topMostVisitingStrategy,
   //      Boolean continueOnMissingVertex
   //      )
   //      : base( visitor )
   //   {
   //      this._visitorInfo = visitor.CreateVisitorInfo( new AcceptorInformationWithResult<TElement, TEdgeInfo, TResult>( continueOnMissingVertex, this.VertexAcceptors, topMostVisitingStrategy, this.EdgeAcceptors ) );
   //   }
   //}


   internal sealed class ManualTransitionAcceptor_NoContextImpl<TElement> : AbstractManualAcceptor<Acceptor<TElement>, TElement, AcceptVertexExplicitDelegate<TElement>>, ManualTransitionAcceptor_NoContext<Acceptor<TElement>, TElement>, Acceptor<TElement>
   {
      private readonly ManualVisitorInformation<TElement> _visitorInfo;

      public ManualTransitionAcceptor_NoContextImpl( ExplicitTypeBasedVisitor<TElement> visitor )
         : base( visitor )
      {
         this._visitorInfo = visitor.CreateExplicitVisitorInfo( new ExplicitAcceptorInformation<TElement>( this.VertexAcceptors ) );
      }

      public Boolean Accept( TElement element )
      {
         return this.Visitor.VisitExplicit( element, this._visitorInfo );
      }

      public override Acceptor<TElement> Acceptor
      {
         get
         {
            return this;
         }
      }
   }

   internal sealed class ManualTransitionAcceptor_WithContextImpl<TElement, TContext> : AbstractManualAcceptor<AcceptorWithContext<TElement, TContext>, TElement, AcceptVertexExplicitDelegate<TElement, TContext>>, ManualTransitionAcceptor_WithContext<AcceptorWithContext<TElement, TContext>, TElement, TContext>, AcceptorWithContext<TElement, TContext>
   {
      private readonly ExplicitAcceptorInformation<TElement, TContext> _acceptorInfo;

      public ManualTransitionAcceptor_WithContextImpl( ExplicitTypeBasedVisitor<TElement> visitor )
         : base( visitor )
      {
         this._acceptorInfo = new ExplicitAcceptorInformation<TElement, TContext>( this.VertexAcceptors );
      }

      public override AcceptorWithContext<TElement, TContext> Acceptor
      {
         get
         {
            return this;
         }
      }

      public Boolean Accept( TElement element, TContext context )
      {
         return this.Visitor.VisitExplicit( element, this.Visitor.CreateExplicitVisitorInfo( this._acceptorInfo, context ) );
      }
   }

   internal sealed class ManualTransitionAcceptor_WithReturnValueImpl<TElement, TResult> : AbstractManualAcceptor<AcceptorWithReturnValue<TElement, TResult>, TElement, AcceptVertexExplicitWithResultDelegate<TElement, TResult>>, ManualTransitionAcceptor_WithReturnValue<AcceptorWithReturnValue<TElement, TResult>, TElement, TResult>, AcceptorWithReturnValue<TElement, TResult>
   {
      private readonly ManualVisitorInformationWithResult<TElement, TResult> _visitorInfo;

      public ManualTransitionAcceptor_WithReturnValueImpl( ExplicitTypeBasedVisitor<TElement> visitor )
         : base( visitor )
      {
         this._visitorInfo = visitor.CreateExplicitVisitorInfo( new ExplicitAcceptorInformationWithResult<TElement, TResult>( this.VertexAcceptors ) );
      }

      public override AcceptorWithReturnValue<TElement, TResult> Acceptor
      {
         get
         {
            return this;
         }
      }

      public TResult Accept( TElement element, out Boolean success )
      {
         return this.Visitor.VisitExplicit( element, this._visitorInfo, out success );
      }
   }

   internal sealed class ManualTransitionAcceptor_WithContextAndReturnValueImpl<TElement, TContext, TResult> : AbstractManualAcceptor<AcceptorWithContextAndReturnValue<TElement, TContext, TResult>, TElement, AcceptVertexExplicitWithResultDelegate<TElement, TContext, TResult>>, ManualTransitionAcceptor_WithContextAndReturnValue<AcceptorWithContextAndReturnValue<TElement, TContext, TResult>, TElement, TContext, TResult>, AcceptorWithContextAndReturnValue<TElement, TContext, TResult>
   {
      private readonly ExplicitAcceptorInformationWithResult<TElement, TContext, TResult> _acceptorInfo;

      public ManualTransitionAcceptor_WithContextAndReturnValueImpl( ExplicitTypeBasedVisitor<TElement> visitor )
         : base( visitor )
      {
         this._acceptorInfo = new ExplicitAcceptorInformationWithResult<TElement, TContext, TResult>( this.VertexAcceptors );
      }

      public override AcceptorWithContextAndReturnValue<TElement, TContext, TResult> Acceptor
      {
         get
         {
            return this;
         }
      }

      public TResult Accept( TElement element, TContext context, out Boolean success )
      {
         return this.Visitor.VisitExplicit( element, context, this.Visitor.CreateExplicitVisitorInfo( this._acceptorInfo, context ), out success );
      }
   }

   //internal abstract class AbstractTypeBasedAcceptor<TElement, TEdgeInfo, TContext> : AbstractAutomaticAcceptor<TElement, TEdgeInfo>
   //{
   //   internal AbstractTypeBasedAcceptor(
   //      TypeBasedVisitor<TElement, TEdgeInfo> visitor,
   //      TopMostTypeVisitingStrategy topMostVisitingStrategy,
   //      Boolean continueOnMissingVertex
   //      )
   //      : base( visitor )
   //   {
   //      this.VertexAcceptors = new Dictionary<Type, AcceptVertexDelegate<TElement, TContext>>().ToDictionaryProxy();
   //      this.ExplicitVertexAcceptors = new Dictionary<Type, AcceptVertexExplicitDelegate<TElement, TContext>>().ToDictionaryProxy();
   //      this.EdgeAcceptors = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>>();

   //      this.AcceptorInfo = new AcceptorInformation<TElement, TEdgeInfo, TContext>( continueOnMissingVertex, this.VertexAcceptors.CQ, topMostVisitingStrategy, this.EdgeAcceptors.CQ );
   //      this.ExplicitAcceptorInfo = new ExplicitAcceptorInformation<TElement, TContext>( continueOnMissingVertex, this.ExplicitVertexAcceptors.CQ );
   //   }

   //   private DictionaryProxy<Type, AcceptVertexDelegate<TElement, TContext>> VertexAcceptors { get; }

   //   private DictionaryProxy<Type, AcceptVertexExplicitDelegate<TElement, TContext>> ExplicitVertexAcceptors { get; }

   //   private ListProxy<AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>> EdgeAcceptors { get; }

   //   protected AcceptorInformation<TElement, TEdgeInfo, TContext> AcceptorInfo { get; }

   //   protected ExplicitAcceptorInformation<TElement, TContext> ExplicitAcceptorInfo { get; }

   //   public void RegisterVertexAcceptor( Type type, AcceptVertexDelegate<TElement, TContext> acceptor )
   //   {
   //      this.VertexAcceptors[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
   //   }

   //   public void RegisterExplicitVertexAcceptor( Type type, AcceptVertexExplicitDelegate<TElement, TContext> acceptor )
   //   {
   //      this.ExplicitVertexAcceptors[type] = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
   //   }

   //   public void RegisterEdgeAcceptor( Int32 edgeID, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> enter, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> exit )
   //   {
   //      if ( edgeID < 0 )
   //      {
   //         throw new ArgumentException( "Edge ID must be at least zero." );
   //      }
   //      var list = this.EdgeAcceptors;
   //      var count = list.CQ.Count;
   //      var edgeInfo = new AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>( enter, exit );

   //      if ( edgeID == count )
   //      {
   //         list.Add( edgeInfo );
   //      }
   //      else if ( edgeID < count )
   //      {
   //         list[edgeID] = edgeInfo;
   //      }
   //      else // id > list.Count
   //      {
   //         list.AddRange( Enumerable.Repeat<AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>>( null, edgeID - count ) );
   //         list.Add( edgeInfo );
   //      }
   //   }
   //}

   //internal sealed class TypeBasedAcceptor<TElement, TEdgeInfo, TContext> : AbstractTypeBasedAcceptor<TElement, TEdgeInfo, TContext>
   //{

   //   public TypeBasedAcceptor(
   //      TypeBasedVisitor<TElement, TEdgeInfo> visitor,
   //      TopMostTypeVisitingStrategy topMostVisitingStrategy,
   //      Boolean continueOnMissingVertex
   //      )
   //      : base( visitor, topMostVisitingStrategy, continueOnMissingVertex )
   //   {
   //   }

   //   public Boolean Accept( TElement element, TContext context )
   //   {
   //      return this.Visitor.Visit( element, this.Visitor.CreateVisitorInfo( this.AcceptorInfo, context ) );
   //   }

   //   public Boolean AcceptExplicit( TElement element, TContext context )
   //   {
   //      return this.Visitor.VisitExplicit( element, this.Visitor.CreateExplicitVisitorInfo( this.ExplicitAcceptorInfo, context ) );
   //   }


   //}

   //internal abstract class AbstractCachingTypeBasedAcceptor<TElement, TEdgeInfo, TContext, TContextInitializer> : AbstractTypeBasedAcceptor<TElement, TEdgeInfo, TContext>
   //   where TContextInitializer : class
   //{
   //   internal AbstractCachingTypeBasedAcceptor(
   //      TypeBasedVisitor<TElement, TEdgeInfo> visitor,
   //      TopMostTypeVisitingStrategy topMostVisitingStrategy,
   //      Boolean continueOnMissingVertex,
   //      Func<TContext> contextFactory,
   //      TContextInitializer contextInitializer
   //      ) : base( visitor, topMostVisitingStrategy, continueOnMissingVertex )
   //   {
   //      this.ContextFactory = ArgumentValidator.ValidateNotNull( "Context factory", contextFactory );
   //      this.ContextInitializer = ArgumentValidator.ValidateNotNull( "Context initializer", contextInitializer );
   //   }

   //   protected Func<TContext> ContextFactory { get; }

   //   protected TContextInitializer ContextInitializer { get; }


   //}

   //internal sealed class CachingTypeBasedAcceptor<TElement, TEdgeInfo, TContext> : AbstractCachingTypeBasedAcceptor<TElement, TEdgeInfo, TContext, Action<TElement, TContext>>
   //{

   //   private readonly LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<AutomaticVisitorInformation<TElement, TEdgeInfo, TContext>>> _visitorInfoPool;
   //   private readonly LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<ManualVisitorInformation<TElement, TContext>>> _explicitVisitorInfoPool;

   //   public CachingTypeBasedAcceptor(
   //      TypeBasedVisitor<TElement, TEdgeInfo> visitor,
   //      TopMostTypeVisitingStrategy topMostVisitingStrategy,
   //      Boolean continueOnMissingVertex,
   //      Func<TContext> contextFactory,
   //      Action<TElement, TContext> contextInitializer
   //      ) : base( visitor, topMostVisitingStrategy, continueOnMissingVertex, contextFactory, contextInitializer )
   //   {
   //      this._visitorInfoPool = new LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<AutomaticVisitorInformation<TElement, TEdgeInfo, TContext>>>();
   //      this._explicitVisitorInfoPool = new LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<ManualVisitorInformation<TElement, TContext>>>();
   //   }

   //   public Boolean Accept( TElement element )
   //   {
   //      var pool = this._visitorInfoPool;
   //      var instance = pool.TakeInstance();
   //      try
   //      {
   //         if ( instance == null )
   //         {
   //            instance = new InstanceHolder<AutomaticVisitorInformation<TElement, TEdgeInfo, TContext>>( this.Visitor.CreateVisitorInfo( this.AcceptorInfo, this.ContextFactory() ) );
   //         }
   //         var info = instance.Instance;
   //         this.ContextInitializer( element, info.Context );
   //         return this.Visitor.Visit( element, info );
   //      }
   //      finally
   //      {
   //         pool.ReturnInstance( instance );
   //      }
   //   }

   //   public Boolean AcceptExplicit( TElement element )
   //   {
   //      var pool = this._explicitVisitorInfoPool;
   //      var instance = pool.TakeInstance();
   //      try
   //      {
   //         if ( instance == null )
   //         {
   //            instance = new InstanceHolder<ManualVisitorInformation<TElement, TContext>>( this.Visitor.CreateExplicitVisitorInfo( this.ExplicitAcceptorInfo, this.ContextFactory() ) );
   //         }
   //         var info = instance.Instance;
   //         this.ContextInitializer( element, info.Context );
   //         return this.Visitor.VisitExplicit( element, info );
   //      }
   //      finally
   //      {
   //         pool.ReturnInstance( instance );
   //      }
   //   }
   //}

   //internal sealed class CachingTypeBasedAcceptor<TElement, TEdgeInfo, TContext, TAdditionalInfo> : AbstractCachingTypeBasedAcceptor<TElement, TEdgeInfo, TContext, Action<TElement, TContext, TAdditionalInfo>>
   //{
   //   private readonly LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<AutomaticVisitorInformation<TElement, TEdgeInfo, TContext>>> _visitorInfoPool;
   //   private readonly LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<ManualVisitorInformation<TElement, TContext>>> _explicitVisitorInfoPool;

   //   public CachingTypeBasedAcceptor(
   //      TypeBasedVisitor<TElement, TEdgeInfo> visitor,
   //      TopMostTypeVisitingStrategy topMostVisitingStrategy,
   //      Boolean continueOnMissingVertex,
   //      Func<TContext> contextFactory,
   //      Action<TElement, TContext, TAdditionalInfo> contextInitializer
   //      ) : base( visitor, topMostVisitingStrategy, continueOnMissingVertex, contextFactory, contextInitializer )
   //   {
   //      this._visitorInfoPool = new LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<AutomaticVisitorInformation<TElement, TEdgeInfo, TContext>>>();
   //      this._explicitVisitorInfoPool = new LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<ManualVisitorInformation<TElement, TContext>>>();
   //   }

   //   public Boolean Accept( TElement element, TAdditionalInfo additionalInfo )
   //   {
   //      var pool = this._visitorInfoPool;
   //      var instance = pool.TakeInstance();
   //      try
   //      {
   //         if ( instance == null )
   //         {
   //            instance = new InstanceHolder<AutomaticVisitorInformation<TElement, TEdgeInfo, TContext>>( this.Visitor.CreateVisitorInfo( this.AcceptorInfo, this.ContextFactory() ) );
   //         }
   //         var info = instance.Instance;
   //         this.ContextInitializer( element, info.Context, additionalInfo );
   //         return this.Visitor.Visit( element, info );
   //      }
   //      finally
   //      {
   //         pool.ReturnInstance( instance );
   //      }
   //   }

   //   public Boolean AcceptExplicit( TElement element, TAdditionalInfo additionalInfo )
   //   {
   //      var pool = this._explicitVisitorInfoPool;
   //      var instance = pool.TakeInstance();
   //      try
   //      {
   //         if ( instance == null )
   //         {
   //            instance = new InstanceHolder<ManualVisitorInformation<TElement, TContext>>( this.Visitor.CreateExplicitVisitorInfo( this.ExplicitAcceptorInfo, this.ContextFactory() ) );
   //         }
   //         var info = instance.Instance;
   //         this.ContextInitializer( element, info.Context, additionalInfo );
   //         return this.Visitor.VisitExplicit( element, info );
   //      }
   //      finally
   //      {
   //         pool.ReturnInstance( instance );
   //      }
   //   }
   //}



   internal abstract class AbstractAcceptorInformation<TVertexDelegate>
   {

      public AbstractAcceptorInformation(
         DictionaryQuery<Type, TVertexDelegate> vertexAcceptors
         )
      {
         this.VertexAcceptors = ArgumentValidator.ValidateNotNull( "Vertex acceptors", vertexAcceptors );
      }

      public DictionaryQuery<Type, TVertexDelegate> VertexAcceptors { get; }

   }

   //internal abstract class AbstractImplicitAcceptorInformation<TVertexDelegate> : AbstractAcceptorInformation<TVertexDelegate>
   //{
   //   public AbstractImplicitAcceptorInformation(
   //      Boolean continueOnMissingVertex,
   //      DictionaryQuery<Type, TVertexDelegate> vertexAcceptors,
   //      TopMostTypeVisitingStrategy topMostVisitingStrategy
   //      ) : base( continueOnMissingVertex, vertexAcceptors )
   //   {
   //      this.TopMostVisitingStrategy = topMostVisitingStrategy;
   //   }

   //   public TopMostTypeVisitingStrategy TopMostVisitingStrategy { get; }
   //}


   internal abstract class AbstractAcceptorInformation<TVertexDelegate, TEdgeDelegate, TEdgeInfo> : AbstractAcceptorInformation<TVertexDelegate>
      where TEdgeInfo : AbstractEdgeDelegateInformation<TEdgeDelegate>
   {
      public AbstractAcceptorInformation(
         Boolean continueOnMissingVertex,
         DictionaryQuery<Type, TVertexDelegate> vertexAcceptors,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         ListQuery<TEdgeInfo> edgeAcceptors
         )
      : base( vertexAcceptors )
      {
         this.ContinueOnMissingVertex = continueOnMissingVertex;
         this.TopMostVisitingStrategy = topMostVisitingStrategy;
         this.EdgeAcceptors = ArgumentValidator.ValidateNotNull( "Edge acceptors", edgeAcceptors );
      }

      public Boolean ContinueOnMissingVertex { get; }

      public TopMostTypeVisitingStrategy TopMostVisitingStrategy { get; }

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

   //internal class AcceptorInformationWithResult<TElement, TEdgeInfo, TResult> : AbstractAcceptorInformation<AcceptVertexWithResultDelegate<TElement, TResult>, AcceptEdgeDelegate<TElement, TEdgeInfo>, AcceptEdgeDelegateInformation<TElement, TEdgeInfo>>
   //{
   //   public AcceptorInformationWithResult(
   //      Boolean continueOnMissingVertex,
   //      DictionaryQuery<Type, AcceptVertexWithResultDelegate<TElement, TResult>> vertexAcceptors,
   //      TopMostTypeVisitingStrategy topMostVisitingStrategy,
   //      ListQuery<AcceptEdgeDelegateInformation<TElement, TEdgeInfo>> edgeAcceptors
   //      ) : base( continueOnMissingVertex, vertexAcceptors, topMostVisitingStrategy, edgeAcceptors )
   //   {
   //   }
   //}

   internal class ExplicitAcceptorInformation<TElement> : AbstractAcceptorInformation<AcceptVertexExplicitDelegate<TElement>>
   {
      public ExplicitAcceptorInformation(
         DictionaryQuery<Type, AcceptVertexExplicitDelegate<TElement>> vertexAcceptors
         ) : base( vertexAcceptors )
      {

      }
   }

   internal class ExplicitAcceptorInformation<TElement, TContext> : AbstractAcceptorInformation<AcceptVertexExplicitDelegate<TElement, TContext>>
   {
      public ExplicitAcceptorInformation(
         DictionaryQuery<Type, AcceptVertexExplicitDelegate<TElement, TContext>> vertexAcceptors
         ) : base( vertexAcceptors )
      {

      }
   }

   internal class ExplicitAcceptorInformationWithResult<TElement, TResult> : AbstractAcceptorInformation<AcceptVertexExplicitWithResultDelegate<TElement, TResult>>
   {
      public ExplicitAcceptorInformationWithResult(
         DictionaryQuery<Type, AcceptVertexExplicitWithResultDelegate<TElement, TResult>> vertexAcceptors
         ) : base( vertexAcceptors )
      {

      }
   }

   internal class ExplicitAcceptorInformationWithResult<TElement, TContext, TResult> : AbstractAcceptorInformation<AcceptVertexExplicitWithResultDelegate<TElement, TContext, TResult>>
   {
      public ExplicitAcceptorInformationWithResult(
         DictionaryQuery<Type, AcceptVertexExplicitWithResultDelegate<TElement, TContext, TResult>> vertexAcceptors
         ) : base( vertexAcceptors )
      {

      }
   }

}
