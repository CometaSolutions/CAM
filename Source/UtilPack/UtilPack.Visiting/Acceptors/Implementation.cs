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

   internal abstract class AbstractAutomaticAcceptor<TAcceptor, TElement, TVertexDelegate, TEdgeInfo, TEdgeDelegate, TEdgeDelegateInfo> : AbstractAcceptor<TAcceptor, TElement, AutomaticTypeBasedVisitor<TElement, TEdgeInfo>, TVertexDelegate>, AcceptorSetup<TAcceptor, AutomaticTypeBasedVisitor<TElement, TEdgeInfo>, TVertexDelegate, TEdgeDelegate>
      where TVertexDelegate : class
      where TEdgeDelegateInfo : class
   {
      private readonly ListProxy<TEdgeDelegateInfo> _edgeAcceptors;

      internal AbstractAutomaticAcceptor( AutomaticTypeBasedVisitor<TElement, TEdgeInfo> visitor )
         : base( visitor )
      {
         this._edgeAcceptors = new List<TEdgeDelegateInfo>().AsListProxy();
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

   internal abstract class AbstractManualAcceptor<TAcceptor, TElement, TVertexDelegate> : AbstractAcceptor<TAcceptor, TElement, ManualTypeBasedVisitor<TElement>, TVertexDelegate>
      where TVertexDelegate : class
   {
      public AbstractManualAcceptor( ManualTypeBasedVisitor<TElement> visitor )
         : base( visitor )
      {

      }
   }

   internal sealed class AutomaticTransitionAcceptor_NoContextImpl<TElement, TEdgeInfo> : AbstractAutomaticAcceptor<Acceptor<TElement>, TElement, AcceptVertexDelegate<TElement>, TEdgeInfo, AcceptEdgeDelegate<TElement, TEdgeInfo>, AcceptEdgeDelegateInformation<TElement, TEdgeInfo>>, AutomaticTransitionAcceptor_NoContext<Acceptor<TElement>, TElement, TEdgeInfo>
   {
      private sealed class AcceptorImpl : Acceptor<TElement>
      {
         private readonly AutomaticTypeBasedVisitor<TElement, TEdgeInfo> _visitor;
         private readonly AutomaticVisitorInformation<TElement, TEdgeInfo> _visitorInfo;

         internal AcceptorImpl(
            AutomaticTypeBasedVisitor<TElement, TEdgeInfo> visitor,
            AutomaticVisitorInformation<TElement, TEdgeInfo> visitorInfo
            )
         {
            this._visitor = ArgumentValidator.ValidateNotNull( "Visitor", visitor );
            this._visitorInfo = ArgumentValidator.ValidateNotNull( "Visitor info", visitorInfo );
         }

         public Boolean Accept( TElement element )
         {
            return this._visitor.Visit( element, this._visitorInfo );
         }
      }

      private readonly AcceptorImpl _acceptor;

      public AutomaticTransitionAcceptor_NoContextImpl(
         AutomaticTypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex
         )
         : base( visitor )
      {
         this._acceptor = new AcceptorImpl( visitor, visitor.CreateVisitorInfo( new AcceptorInformation<TElement, TEdgeInfo>( continueOnMissingVertex, this.VertexAcceptors, topMostVisitingStrategy, this.EdgeAcceptors ) ) );
      }

      protected override AcceptEdgeDelegateInformation<TElement, TEdgeInfo> GetInfoFromDelegate( AcceptEdgeDelegate<TElement, TEdgeInfo> enter, AcceptEdgeDelegate<TElement, TEdgeInfo> exit )
      {
         return new AcceptEdgeDelegateInformation<TElement, TEdgeInfo>( enter, exit );
      }

      public override Acceptor<TElement> Acceptor
      {
         get
         {
            return this._acceptor;
         }
      }
   }

   internal sealed class AutomaticTransitionAcceptor_WithContextImpl<TElement, TEdgeInfo, TContext> : AbstractAutomaticAcceptor<AcceptorWithContext<TElement, TContext>, TElement, AcceptVertexDelegate<TElement, TContext>, TEdgeInfo, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext>, AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext>>, AutomaticTransitionAcceptor_WithContext<AcceptorWithContext<TElement, TContext>, TElement, TEdgeInfo, TContext>
   {
      private sealed class AcceptorImpl : AcceptorWithContext<TElement, TContext>
      {
         private readonly AutomaticTypeBasedVisitor<TElement, TEdgeInfo> _visitor;
         private readonly AcceptorInformation<TElement, TEdgeInfo, TContext> _acceptorInfo;

         internal AcceptorImpl(
            AutomaticTypeBasedVisitor<TElement, TEdgeInfo> visitor,
            AcceptorInformation<TElement, TEdgeInfo, TContext> acceptorInfo
            )
         {
            this._visitor = ArgumentValidator.ValidateNotNull( "Visitor", visitor );
            this._acceptorInfo = ArgumentValidator.ValidateNotNull( "Acceptor info", acceptorInfo );
         }

         public Boolean Accept( TElement element, TContext context )
         {
            return this._visitor.Visit( element, this._visitor.CreateVisitorInfo( this._acceptorInfo, context ) );
         }
      }

      private readonly AcceptorImpl _acceptor;

      public AutomaticTransitionAcceptor_WithContextImpl(
         AutomaticTypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex
         )
         : base( visitor )
      {
         this._acceptor = new AcceptorImpl( visitor, new AcceptorInformation<TElement, TEdgeInfo, TContext>( continueOnMissingVertex, this.VertexAcceptors, topMostVisitingStrategy, this.EdgeAcceptors ) );
      }

      public override AcceptorWithContext<TElement, TContext> Acceptor
      {
         get
         {
            return this._acceptor;
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


   internal sealed class ManualTransitionAcceptor_NoContextImpl<TElement> : AbstractManualAcceptor<Acceptor<TElement>, TElement, AcceptVertexExplicitDelegate<TElement>>, ManualTransitionAcceptor_NoContext<Acceptor<TElement>, TElement>
   {
      private sealed class AcceptorImpl : Acceptor<TElement>
      {
         private readonly ManualTypeBasedVisitor<TElement> _visitor;
         private readonly ManualVisitorInformation<TElement> _visitorInfo;

         internal AcceptorImpl(
            ManualTypeBasedVisitor<TElement> visitor,
            ManualVisitorInformation<TElement> visitorInfo
            )
         {
            this._visitor = ArgumentValidator.ValidateNotNull( "Visitor", visitor );
            this._visitorInfo = ArgumentValidator.ValidateNotNull( "Visitor info", visitorInfo );
         }

         public Boolean Accept( TElement element )
         {
            return this._visitor.VisitExplicit( element, this._visitorInfo );
         }
      }

      private readonly AcceptorImpl _acceptor;

      public ManualTransitionAcceptor_NoContextImpl( ManualTypeBasedVisitor<TElement> visitor )
         : base( visitor )
      {
         this._acceptor = new AcceptorImpl( visitor, visitor.CreateExplicitVisitorInfo( new ExplicitAcceptorInformation<TElement>( this.VertexAcceptors ) ) );
      }

      public override Acceptor<TElement> Acceptor
      {
         get
         {
            return this._acceptor;
         }
      }
   }

   internal sealed class ManualTransitionAcceptor_WithContextImpl<TElement, TContext> : AbstractManualAcceptor<AcceptorWithContext<TElement, TContext>, TElement, AcceptVertexExplicitDelegate<TElement, TContext>>, ManualTransitionAcceptor_WithContext<AcceptorWithContext<TElement, TContext>, TElement, TContext>
   {
      private sealed class AcceptorImpl : AcceptorWithContext<TElement, TContext>
      {
         private readonly ManualTypeBasedVisitor<TElement> _visitor;
         private readonly ExplicitAcceptorInformation<TElement, TContext> _acceptorInfo;

         internal AcceptorImpl(
            ManualTypeBasedVisitor<TElement> visitor,
            ExplicitAcceptorInformation<TElement, TContext> acceptorInfo
            )
         {
            this._visitor = ArgumentValidator.ValidateNotNull( "Visitor", visitor );
            this._acceptorInfo = ArgumentValidator.ValidateNotNull( "Acceptor info", acceptorInfo );
         }

         public Boolean Accept( TElement element, TContext context )
         {
            return this._visitor.VisitExplicit( element, this._visitor.CreateExplicitVisitorInfo( this._acceptorInfo, context ) );
         }
      }

      private readonly AcceptorImpl _acceptor;

      public ManualTransitionAcceptor_WithContextImpl( ManualTypeBasedVisitor<TElement> visitor )
         : base( visitor )
      {
         this._acceptor = new AcceptorImpl( visitor, new ExplicitAcceptorInformation<TElement, TContext>( this.VertexAcceptors ) );
      }

      public override AcceptorWithContext<TElement, TContext> Acceptor
      {
         get
         {
            return this._acceptor;
         }
      }

   }

   internal sealed class ManualTransitionAcceptor_WithReturnValueImpl<TElement, TResult> : AbstractManualAcceptor<AcceptorWithReturnValue<TElement, TResult>, TElement, AcceptVertexExplicitWithResultDelegate<TElement, TResult>>, ManualTransitionAcceptor_WithReturnValue<AcceptorWithReturnValue<TElement, TResult>, TElement, TResult>
   {
      private sealed class AcceptorImpl : AcceptorWithReturnValue<TElement, TResult>
      {
         private readonly ManualTypeBasedVisitor<TElement> _visitor;
         private readonly ManualVisitorInformationWithResult<TElement, TResult> _visitorInfo;

         internal AcceptorImpl(
            ManualTypeBasedVisitor<TElement> visitor,
            ManualVisitorInformationWithResult<TElement, TResult> visitorInfo
            )
         {
            this._visitor = ArgumentValidator.ValidateNotNull( "Visitor", visitor );
            this._visitorInfo = ArgumentValidator.ValidateNotNull( "Visitor info", visitorInfo );
         }

         public TResult Accept( TElement element, out Boolean success )
         {
            return this._visitor.VisitExplicit( element, this._visitorInfo, out success );
         }
      }

      private readonly AcceptorImpl _acceptor;

      public ManualTransitionAcceptor_WithReturnValueImpl( ManualTypeBasedVisitor<TElement> visitor )
         : base( visitor )
      {
         this._acceptor = new AcceptorImpl( visitor, visitor.CreateExplicitVisitorInfo( new ExplicitAcceptorInformationWithResult<TElement, TResult>( this.VertexAcceptors ) ) );
      }

      public override AcceptorWithReturnValue<TElement, TResult> Acceptor
      {
         get
         {
            return this._acceptor;
         }
      }

   }

   internal sealed class ManualTransitionAcceptor_WithContextAndReturnValueImpl<TElement, TContext, TResult> : AbstractManualAcceptor<AcceptorWithContextAndReturnValue<TElement, TContext, TResult>, TElement, AcceptVertexExplicitWithResultDelegate<TElement, TContext, TResult>>, ManualTransitionAcceptor_WithContextAndReturnValue<AcceptorWithContextAndReturnValue<TElement, TContext, TResult>, TElement, TResult, AcceptVertexExplicitWithResultDelegate<TElement, TContext, TResult>>
   {
      private sealed class AcceptorImpl : AcceptorWithContextAndReturnValue<TElement, TContext, TResult>
      {
         private readonly ManualTypeBasedVisitor<TElement> _visitor;
         private readonly ExplicitAcceptorInformationWithResult<TElement, TContext, TResult> _acceptorInfo;

         internal AcceptorImpl(
            ManualTypeBasedVisitor<TElement> visitor,
            ExplicitAcceptorInformationWithResult<TElement, TContext, TResult> acceptorInfo
            )
         {
            this._visitor = ArgumentValidator.ValidateNotNull( "Visitor", visitor );
            this._acceptorInfo = ArgumentValidator.ValidateNotNull( "Acceptor info", acceptorInfo );
         }

         public TResult Accept( TElement element, TContext context, out Boolean success )
         {
            return this._visitor.VisitExplicit( element, context, this._visitor.CreateExplicitVisitorInfo( this._acceptorInfo, context ), out success );
         }
      }

      private readonly AcceptorImpl _acceptor;

      public ManualTransitionAcceptor_WithContextAndReturnValueImpl( ManualTypeBasedVisitor<TElement> visitor )
         : base( visitor )
      {
         this._acceptor = new AcceptorImpl( visitor, new ExplicitAcceptorInformationWithResult<TElement, TContext, TResult>( this.VertexAcceptors ) );
      }

      public override AcceptorWithContextAndReturnValue<TElement, TContext, TResult> Acceptor
      {
         get
         {
            return this._acceptor;
         }
      }
   }

   internal sealed class ManualTransitionAcceptor_WithContextAndReturnValueImpl_ContextForCallback<TElement, TContext, TResult> : AbstractManualAcceptor<AcceptorWithContextAndReturnValue<TElement, TContext, TResult>, TElement, AcceptVertexExplicitWithContextAndResultDelegate<TElement, TContext, TResult>>, ManualTransitionAcceptor_WithContextAndReturnValue<AcceptorWithContextAndReturnValue<TElement, TContext, TResult>, TElement, TResult, AcceptVertexExplicitWithContextAndResultDelegate<TElement, TContext, TResult>>
   {
      private sealed class AcceptorImpl : AcceptorWithContextAndReturnValue<TElement, TContext, TResult>
      {
         private readonly ManualTypeBasedVisitor<TElement> _visitor;
         private readonly ManualVisitorInformationWithResultAndContext<TElement, TContext, TResult> _visitorInfo;

         public AcceptorImpl( ManualTypeBasedVisitor<TElement> visitor, ManualVisitorInformationWithResultAndContext<TElement, TContext, TResult> visitorInfo )
         {
            this._visitor = ArgumentValidator.ValidateNotNull( "Visitor", visitor );
            this._visitorInfo = ArgumentValidator.ValidateNotNull( "Visitor info", visitorInfo );
         }

         public TResult Accept( TElement element, TContext context, out Boolean success )
         {
            return this._visitor.VisitExplicit( element, context, this._visitorInfo, out success );
         }
      }

      private readonly AcceptorImpl _acceptor;

      public ManualTransitionAcceptor_WithContextAndReturnValueImpl_ContextForCallback( ManualTypeBasedVisitor<TElement> visitor )
         : base( visitor )
      {
         this._acceptor = new AcceptorImpl( visitor, visitor.CreateExplicitVisitorInfo( new ExplicitAcceptorInformationWithResultAndContext<TElement, TContext, TResult>( this.VertexAcceptors ) ) );
      }

      public override AcceptorWithContextAndReturnValue<TElement, TContext, TResult> Acceptor
      {
         get
         {
            return this._acceptor;
         }
      }
   }

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

   internal class ExplicitAcceptorInformationWithResultAndContext<TElement, TContext, TResult> : AbstractAcceptorInformation<AcceptVertexExplicitWithContextAndResultDelegate<TElement, TContext, TResult>>
   {
      public ExplicitAcceptorInformationWithResultAndContext(
         DictionaryQuery<Type, AcceptVertexExplicitWithContextAndResultDelegate<TElement, TContext, TResult>> vertexAcceptors
         ) : base( vertexAcceptors )
      {

      }
   }

}
