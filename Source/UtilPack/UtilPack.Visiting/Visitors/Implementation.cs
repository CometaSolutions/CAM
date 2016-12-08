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
   internal abstract class AbstractVisitorInformation<TDelegate, TAcceptorInfo>
      where TDelegate : class
      where TAcceptorInfo : class
   {
      public AbstractVisitorInformation( TDelegate callback, TAcceptorInfo acceptorInfo )
      {
         this.Callback = ArgumentValidator.ValidateNotNull( "Callback", callback );
         this.AcceptorInformation = ArgumentValidator.ValidateNotNull( "Acceptor info", acceptorInfo );
      }

      public TDelegate Callback { get; }

      public TAcceptorInfo AcceptorInformation { get; }
   }

   internal abstract class AbstractVisitorInformation<TDelegate, TAcceptorInfo, TContext> : AbstractVisitorInformation<TDelegate, TAcceptorInfo>
      where TDelegate : class
      where TAcceptorInfo : class
   {

      public AbstractVisitorInformation( TDelegate callback, TAcceptorInfo acceptorInfo, TContext context )
         : base( callback, acceptorInfo )
      {
         this.Context = context;
      }

      public TContext Context { get; }

   }

   internal class AutomaticVisitorInformation<TElement, TEdgeInfo> : AbstractVisitorInformation<VisitElementCallbackDelegate<TElement, TEdgeInfo>, AcceptorInformation<TElement, TEdgeInfo>>
   {
      public AutomaticVisitorInformation( VisitElementCallbackDelegate<TElement, TEdgeInfo> callback, AcceptorInformation<TElement, TEdgeInfo> acceptorInfo )
         : base( callback, acceptorInfo )
      {
      }
   }

   internal class AutomaticVisitorInformation<TElement, TEdgeInfo, TContext> : AbstractVisitorInformation<VisitElementCallbackDelegate<TElement, TEdgeInfo>, AcceptorInformation<TElement, TEdgeInfo, TContext>, TContext>
   {
      public AutomaticVisitorInformation( VisitElementCallbackDelegate<TElement, TEdgeInfo> callback, AcceptorInformation<TElement, TEdgeInfo, TContext> acceptorInfo, TContext context )
         : base( callback, acceptorInfo, context )
      {
      }
   }

   //internal class VisitorInformationWithResult<TElement, TEdgeInfo, TResult> : AbstractVisitorInformation<VisitElementCallbackDelegate<TElement, TEdgeInfo>, AcceptorInformationWithResult<TElement, TEdgeInfo, TResult>>
   //{
   //   public VisitorInformationWithResult( VisitElementCallbackDelegate<TElement, TEdgeInfo> callback, AcceptorInformationWithResult<TElement, TEdgeInfo, TResult> acceptorInfo )
   //      : base( callback, acceptorInfo )
   //   {
   //   }
   //}

   internal class ManualVisitorInformationWithResult<TElement, TResult> : AbstractVisitorInformation<AcceptVertexExplicitCallbackWithResultDelegate<TElement, TResult>, ExplicitAcceptorInformationWithResult<TElement, TResult>>
   {
      public ManualVisitorInformationWithResult( AcceptVertexExplicitCallbackWithResultDelegate<TElement, TResult> callback, ExplicitAcceptorInformationWithResult<TElement, TResult> acceptorInfo )
         : base( callback, acceptorInfo )
      {

      }
   }

   internal class ManualVisitorInformationWithResult<TElement, TContext, TResult> : AbstractVisitorInformation<AcceptVertexExplicitCallbackWithResultDelegate<TElement, TResult>, ExplicitAcceptorInformationWithResult<TElement, TContext, TResult>, TContext>
   {
      public ManualVisitorInformationWithResult( AcceptVertexExplicitCallbackWithResultDelegate<TElement, TResult> callback, ExplicitAcceptorInformationWithResult<TElement, TContext, TResult> acceptorInfo, TContext context )
         : base( callback, acceptorInfo, context )
      {

      }
   }

   internal class ManualVisitorInformationWithResultAndContext<TElement, TContext, TResult> : AbstractVisitorInformation<AcceptVertexExplicitCallbackWithResultDelegate<TElement, TContext, TResult>, ExplicitAcceptorInformationWithResultAndContext<TElement, TContext, TResult>>
   {
      public ManualVisitorInformationWithResultAndContext( AcceptVertexExplicitCallbackWithResultDelegate<TElement, TContext, TResult> callback, ExplicitAcceptorInformationWithResultAndContext<TElement, TContext, TResult> acceptorInfo )
         : base( callback, acceptorInfo )
      {
      }
   }


   internal class ManualVisitorInformation<TElement> : AbstractVisitorInformation<AcceptVertexExplicitCallbackDelegate<TElement>, ExplicitAcceptorInformation<TElement>>
   {
      public ManualVisitorInformation( AcceptVertexExplicitCallbackDelegate<TElement> callback, ExplicitAcceptorInformation<TElement> acceptorInfo )
         : base( callback, acceptorInfo )
      {
      }
   }

   internal class ManualVisitorInformation<TElement, TContext> : AbstractVisitorInformation<AcceptVertexExplicitCallbackDelegate<TElement>, ExplicitAcceptorInformation<TElement, TContext>, TContext>
   {
      public ManualVisitorInformation( AcceptVertexExplicitCallbackDelegate<TElement> callback, ExplicitAcceptorInformation<TElement, TContext> acceptorInfo, TContext context )
         : base( callback, acceptorInfo, context )
      {
      }

   }
}
