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


   internal abstract class AbstractAutomaticCachingAcceptor<TAcceptor, TElement, TEdgeInfo, TContext, TContextInitializer> : AbstractAutomaticAcceptor<TAcceptor, TElement, AcceptVertexDelegate<TElement, TContext>, TEdgeInfo, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext>, AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>>
      where TContextInitializer : class
   {
      public AbstractAutomaticCachingAcceptor(
         AutomaticTypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         Func<TContext> contextFactory,
         TContextInitializer contextInitializer
         ) : base( visitor )
      {
         this.ContextFactory = ArgumentValidator.ValidateNotNull( "Context factory", contextFactory );
         this.ContextInitializer = contextInitializer;
         this.AcceptorInfo = new AcceptorInformation<TElement, TEdgeInfo, TContext>( continueOnMissingVertex, this.VertexAcceptors, topMostVisitingStrategy, this.EdgeAcceptors );
         this.VisitorInfoPool = new LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<AutomaticVisitorInformation<TElement, TEdgeInfo, TContext>>>();
      }

      protected Func<TContext> ContextFactory { get; }

      protected TContextInitializer ContextInitializer { get; }

      protected override AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext> GetInfoFromDelegate( AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> enter, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> exit )
      {
         return new AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>( enter, exit );
      }

      protected AcceptorInformation<TElement, TEdgeInfo, TContext> AcceptorInfo { get; }

      protected LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<AutomaticVisitorInformation<TElement, TEdgeInfo, TContext>>> VisitorInfoPool { get; }
   }


   internal sealed class AutomaticTransitionAcceptor_WithContextImpl_Caching<TElement, TEdgeInfo, TContext> : AbstractAutomaticCachingAcceptor<Acceptor<TElement>, TElement, TEdgeInfo, TContext, Action<TElement, TContext>>, AutomaticTransitionAcceptor_WithContext<Acceptor<TElement>, TElement, TEdgeInfo, TContext>
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
               this._setup?.ContextInitializer( element, info.Context );
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
         AutomaticTypeBasedVisitor<TElement, TEdgeInfo> visitor,
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

   internal sealed class AutomaticTransitionAcceptor_WithContextImpl_Caching<TElement, TEdgeInfo, TContext, TAdditionalInfo> : AbstractAutomaticCachingAcceptor<AcceptorWithContext<TElement, TAdditionalInfo>, TElement, TEdgeInfo, TContext, Action<TElement, TContext, TAdditionalInfo>>, AutomaticTransitionAcceptor_WithContext<AcceptorWithContext<TElement, TAdditionalInfo>, TElement, TEdgeInfo, TContext>
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
            var preCheckerResult = this._setup._preChecker?.Invoke( element, additionalInfo );
            Boolean retVal;
            if ( preCheckerResult.HasValue )
            {
               retVal = preCheckerResult.Value;
            }
            else
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
                  this._setup?.ContextInitializer( element, info.Context, additionalInfo );
                  retVal = this._setup.Visitor.Visit( element, info );
               }
               finally
               {
                  pool.ReturnInstance( instance );
               }
            }

            return retVal;
         }
      }

      private readonly AcceptorImpl _acceptor;
      private readonly PreCheckerDelegate<TElement, TAdditionalInfo> _preChecker;

      public AutomaticTransitionAcceptor_WithContextImpl_Caching(
         AutomaticTypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         Func<TContext> contextFactory,
         Action<TElement, TContext, TAdditionalInfo> contextInitializer,
         PreCheckerDelegate<TElement, TAdditionalInfo> preChecker
         ) : base( visitor, topMostVisitingStrategy, continueOnMissingVertex, contextFactory, contextInitializer )
      {
         this._preChecker = preChecker;
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

   internal abstract class AbstractManualCachingAcceptor<TAcceptor, TElement, TContext, TVertexDelegate, TContextInitializer> : AbstractAcceptor<TAcceptor, TElement, ManualTypeBasedVisitor<TElement>, TVertexDelegate>
      where TVertexDelegate : class
      where TContextInitializer : class
   {
      public AbstractManualCachingAcceptor(
         ManualTypeBasedVisitor<TElement> visitor,
         Func<TContext> contextFactory,
         TContextInitializer contextInitializer
         )
         : base( visitor )
      {
         this.ContextFactory = ArgumentValidator.ValidateNotNull( "Context factory", contextFactory );
         this.ContextInitializer = contextInitializer;
      }

      protected Func<TContext> ContextFactory { get; }

      protected TContextInitializer ContextInitializer { get; }
   }

   internal abstract class AbstractManualCachingAcceptor_WithResult<TAcceptor, TElement, TContext, TContextInitializer, TResult> : AbstractManualCachingAcceptor<TAcceptor, TElement, TContext, AcceptVertexExplicitWithResultDelegate<TElement, TContext, TResult>, TContextInitializer>
      where TContextInitializer : class
   {
      public AbstractManualCachingAcceptor_WithResult(
         ManualTypeBasedVisitor<TElement> visitor,
         Func<TContext> contextFactory,
         TContextInitializer contextInitializer )
         : base( visitor, contextFactory, contextInitializer )
      {
         this.AcceptorInfo = new ExplicitAcceptorInformationWithResult<TElement, TContext, TResult>( this.VertexAcceptors );
         this.VisitorInfoPool = new LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<ManualVisitorInformationWithResult<TElement, TContext, TResult>>>();
      }

      protected ExplicitAcceptorInformationWithResult<TElement, TContext, TResult> AcceptorInfo { get; }

      protected LocklessInstancePoolForClassesNoHeapAllocations<InstanceHolder<ManualVisitorInformationWithResult<TElement, TContext, TResult>>> VisitorInfoPool { get; }
   }

   internal sealed class ManualTransitionAcceptor_WithContextAndReturnValueImpl_Caching<TElement, TContext, TResult> : AbstractManualCachingAcceptor_WithResult<AcceptorWithReturnValue<TElement, TResult>, TElement, TContext, Action<TElement, TContext>, TResult>, ManualTransitionAcceptor_WithContextAndReturnValue<AcceptorWithReturnValue<TElement, TResult>, TElement, TResult, AcceptVertexExplicitWithResultDelegate<TElement, TContext, TResult>>
   {

      private sealed class AcceptorImpl : AcceptorWithReturnValue<TElement, TResult>
      {
         private readonly ManualTransitionAcceptor_WithContextAndReturnValueImpl_Caching<TElement, TContext, TResult> _setup;

         public AcceptorImpl( ManualTransitionAcceptor_WithContextAndReturnValueImpl_Caching<TElement, TContext, TResult> setup )
         {
            this._setup = ArgumentValidator.ValidateNotNull( "Setup", setup );
         }

         public TResult Accept( TElement element, out Boolean success )
         {
            var pool = this._setup.VisitorInfoPool;
            var instance = pool.TakeInstance();
            try
            {
               if ( instance == null )
               {
                  instance = new InstanceHolder<ManualVisitorInformationWithResult<TElement, TContext, TResult>>( this._setup.Visitor.CreateExplicitVisitorInfo( this._setup.AcceptorInfo, this._setup.ContextFactory() ) );
               }
               var info = instance.Instance;
               var context = info.Context;
               this._setup?.ContextInitializer( element, context );
               return this._setup.Visitor.VisitExplicit( element, context, info, out success );
            }
            finally
            {
               pool.ReturnInstance( instance );
            }
         }
      }

      private readonly AcceptorImpl _acceptor;

      public ManualTransitionAcceptor_WithContextAndReturnValueImpl_Caching(
         ManualTypeBasedVisitor<TElement> visitor,
         Func<TContext> contextFactory,
         Action<TElement, TContext> contextInitializer )
         : base( visitor, contextFactory, contextInitializer )
      {
         this._acceptor = new AcceptorImpl( this );
      }

      public override AcceptorWithReturnValue<TElement, TResult> Acceptor
      {
         get
         {
            return this._acceptor;
         }
      }
   }

   internal sealed class ManualTransitionAcceptor_WithContextAndReturnValueImpl_Caching<TElement, TContext, TResult, TAdditionalInfo> : AbstractManualCachingAcceptor_WithResult<AcceptorWithContextAndReturnValue<TElement, TAdditionalInfo, TResult>, TElement, TContext, Action<TElement, TContext, TAdditionalInfo>, TResult>, ManualTransitionAcceptor_WithContextAndReturnValue<AcceptorWithContextAndReturnValue<TElement, TAdditionalInfo, TResult>, TElement, TResult, AcceptVertexExplicitWithResultDelegate<TElement, TContext, TResult>>
   {
      private sealed class AcceptorImpl : AcceptorWithContextAndReturnValue<TElement, TAdditionalInfo, TResult>
      {
         private readonly ManualTransitionAcceptor_WithContextAndReturnValueImpl_Caching<TElement, TContext, TResult, TAdditionalInfo> _setup;

         public AcceptorImpl( ManualTransitionAcceptor_WithContextAndReturnValueImpl_Caching<TElement, TContext, TResult, TAdditionalInfo> setup )
         {
            this._setup = ArgumentValidator.ValidateNotNull( "Setup", setup );
         }

         public TResult Accept( TElement element, TAdditionalInfo additionalInfo, out Boolean success )
         {
            var pool = this._setup.VisitorInfoPool;
            var instance = pool.TakeInstance();
            try
            {
               if ( instance == null )
               {
                  instance = new InstanceHolder<ManualVisitorInformationWithResult<TElement, TContext, TResult>>( this._setup.Visitor.CreateExplicitVisitorInfo( this._setup.AcceptorInfo, this._setup.ContextFactory() ) );
               }
               var info = instance.Instance;
               var context = info.Context;
               this._setup?.ContextInitializer( element, context, additionalInfo );
               return this._setup.Visitor.VisitExplicit( element, context, info, out success );
            }
            finally
            {
               pool.ReturnInstance( instance );
            }
         }
      }

      private readonly AcceptorImpl _acceptor;

      public ManualTransitionAcceptor_WithContextAndReturnValueImpl_Caching(
         ManualTypeBasedVisitor<TElement> visitor,
         Func<TContext> contextFactory,
         Action<TElement, TContext, TAdditionalInfo> contextInitializer
         )
         : base( visitor, contextFactory, contextInitializer )
      {
         this._acceptor = new AcceptorImpl( this );
      }

      public override AcceptorWithContextAndReturnValue<TElement, TAdditionalInfo, TResult> Acceptor
      {
         get
         {
            return this._acceptor;
         }
      }
   }
}
