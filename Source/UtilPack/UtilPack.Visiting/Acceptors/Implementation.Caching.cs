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

namespace UtilPack.Visiting.Implementation
{
   internal abstract class AbstractTypeBasedCachingAcceptor<TAcceptor, TElement, TEdgeInfo, TContext, TContextInitializer> : AbstractAutomaticAcceptor<TAcceptor, TElement, AcceptVertexDelegate<TElement, TContext>, TEdgeInfo, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext>, AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>>
      where TContextInitializer : class
   {
      internal AbstractTypeBasedCachingAcceptor(
         TypeBasedVisitor<TElement, TEdgeInfo> visitor,
         Func<TContext> contextFactory,
         TContextInitializer contextInitializer
         ) : base( visitor )
      {
         this.ContextFactory = ArgumentValidator.ValidateNotNull( "Context factory", contextFactory );
         this.ContextInitializer = ArgumentValidator.ValidateNotNull( "Context initializer", contextInitializer );
      }

      protected Func<TContext> ContextFactory { get; }

      protected TContextInitializer ContextInitializer { get; }

      protected override AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext> GetInfoFromDelegate( AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> enter, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> exit )
      {
         return new AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>( enter, exit );
      }
   }

   internal abstract class AbstractTypeBasedCachingAcceptor_WithContext<TAcceptor, TElement, TEdgeInfo, TContext, TContextInitializer> : AbstractTypeBasedCachingAcceptor<TAcceptor, TElement, TEdgeInfo, TContext, TContextInitializer>
      where TContextInitializer : class
   {
      public AbstractTypeBasedCachingAcceptor_WithContext(
         TypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         Func<TContext> contextFactory,
         TContextInitializer contextInitializer
         ) : base( visitor, contextFactory, contextInitializer )
      {
         this.AcceptorInfo = new AcceptorInformation<TElement, TEdgeInfo, TContext>( continueOnMissingVertex, this.VertexAcceptors, topMostVisitingStrategy, this.EdgeAcceptors );
         this.VisitorInfoPool = new LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<AutomaticVisitorInformation<TElement, TEdgeInfo, TContext>>>();
      }

      protected AcceptorInformation<TElement, TEdgeInfo, TContext> AcceptorInfo { get; }

      protected LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<AutomaticVisitorInformation<TElement, TEdgeInfo, TContext>>> VisitorInfoPool { get; }
   }


   internal sealed class AutomaticTransitionAcceptor_WithContextImpl_Caching<TElement, TEdgeInfo, TContext> : AbstractTypeBasedCachingAcceptor_WithContext<Acceptor<TElement>, TElement, TEdgeInfo, TContext, Action<TElement, TContext>>, AutomaticTransitionAcceptor_WithContext<Acceptor<TElement>, TElement, TEdgeInfo, TContext>
   {
      private sealed class AcceptorImpl : Acceptor<TElement>
      {
         private readonly AutomaticTransitionAcceptor_WithContextImpl_Caching<TElement, TEdgeInfo, TContext> _setup;

         internal AcceptorImpl(
             AutomaticTransitionAcceptor_WithContextImpl_Caching<TElement, TEdgeInfo, TContext> setup
            )
         {
            this._setup = ArgumentValidator.ValidateNotNull( "Setup", setup );
         }

         public Boolean Accept( TElement element )
         {
            var pool = this._setup.VisitorInfoPool;
            var instance = pool.TakeInstance();
            try
            {
               if ( instance == null )
               {
                  instance = new InstanceHolder<AutomaticVisitorInformation<TElement, TEdgeInfo, TContext>>( this._setup.Visitor.CreateVisitorInfo( this._setup.AcceptorInfo, this._setup.ContextFactory() ) );
               }
               var info = instance.Instance;
               this._setup.ContextInitializer( element, info.Context );
               return this._setup.Visitor.Visit( element, info );
            }
            finally
            {
               pool.ReturnInstance( instance );
            }
         }
      }

      private readonly AcceptorImpl _acceptor;
      public AutomaticTransitionAcceptor_WithContextImpl_Caching(
         TypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         Func<TContext> contextFactory,
         Action<TElement, TContext> contextInitializer
         ) : base( visitor, topMostVisitingStrategy, continueOnMissingVertex, contextFactory, contextInitializer )
      {
         this._acceptor = new AcceptorImpl( this );
      }

      public override Acceptor<TElement> Acceptor
      {
         get
         {
            return this._acceptor;
         }
      }

      //public Boolean AcceptExplicit( TElement element )
      //{
      //   var pool = this._explicitVisitorInfoPool;
      //   var instance = pool.TakeInstance();
      //   try
      //   {
      //      if ( instance == null )
      //      {
      //         instance = new InstanceHolder<ManualVisitorInformation<TElement, TContext>>( this.Visitor.CreateExplicitVisitorInfo( this.ExplicitAcceptorInfo, this.ContextFactory() ) );
      //      }
      //      var info = instance.Instance;
      //      this.ContextInitializer( element, info.Context );
      //      return this.Visitor.VisitExplicit( element, info );
      //   }
      //   finally
      //   {
      //      pool.ReturnInstance( instance );
      //   }
      //}
   }

   internal sealed class AutomaticTransitionAcceptor_WithContextImpl_Caching<TElement, TEdgeInfo, TContext, TAdditionalInfo> : AbstractTypeBasedCachingAcceptor_WithContext<AcceptorWithContext<TElement, TAdditionalInfo>, TElement, TEdgeInfo, TContext, Action<TElement, TContext, TAdditionalInfo>>, AutomaticTransitionAcceptor_WithContext<AcceptorWithContext<TElement, TAdditionalInfo>, TElement, TEdgeInfo, TContext>
   {
      private sealed class AcceptorImpl : AcceptorWithContext<TElement, TAdditionalInfo>
      {
         private readonly AutomaticTransitionAcceptor_WithContextImpl_Caching<TElement, TEdgeInfo, TContext, TAdditionalInfo> _setup;

         internal AcceptorImpl(
             AutomaticTransitionAcceptor_WithContextImpl_Caching<TElement, TEdgeInfo, TContext, TAdditionalInfo> setup
            )
         {
            this._setup = ArgumentValidator.ValidateNotNull( "Setup", setup );
         }

         public Boolean Accept( TElement element, TAdditionalInfo additionalInfo )
         {
            var pool = this._setup.VisitorInfoPool;
            var instance = pool.TakeInstance();
            try
            {
               if ( instance == null )
               {
                  instance = new InstanceHolder<AutomaticVisitorInformation<TElement, TEdgeInfo, TContext>>( this._setup.Visitor.CreateVisitorInfo( this._setup.AcceptorInfo, this._setup.ContextFactory() ) );
               }
               var info = instance.Instance;
               this._setup.ContextInitializer( element, info.Context, additionalInfo );
               return this._setup.Visitor.Visit( element, info );
            }
            finally
            {
               pool.ReturnInstance( instance );
            }
         }
      }

      private readonly AcceptorImpl _acceptor;

      public AutomaticTransitionAcceptor_WithContextImpl_Caching(
         TypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         Func<TContext> contextFactory,
         Action<TElement, TContext, TAdditionalInfo> contextInitializer
         ) : base( visitor, topMostVisitingStrategy, continueOnMissingVertex, contextFactory, contextInitializer )
      {
         this._acceptor = new AcceptorImpl( this );
      }

      public override AcceptorWithContext<TElement, TAdditionalInfo> Acceptor
      {
         get
         {
            return this._acceptor;
         }
      }

   }
}
