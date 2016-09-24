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
using UtilPack.Visiting;

#pragma warning disable 1591
namespace UtilPack.Visiting
{
   // These four interfaces could be merged into two, if 'void' would be acceptable type parameter...
   public interface Acceptor<in TElement>
   {
      Boolean Accept( TElement element );
   }

   public interface AcceptorWithContext<in TElement, in TContext>
   {
      Boolean Accept( TElement element, TContext context );
   }

   public interface AcceptorWithReturnValue<in TElement, out TResult>
   {
      TResult Accept( TElement element, out Boolean success );
   }

   public interface AcceptorWithContextAndReturnValue<in TElement, in TContext, out TResult>
   {
      TResult Accept( TElement element, TContext context, out Boolean success );
   }

   // These interfaces define how the acceptors are set up
   public interface AcceptorSetup<out TAcceptor, out TVisitor, in TVertexDelegate>
   {
      void RegisterVertexAcceptor( Type type, TVertexDelegate vertexAcceptor );

      TAcceptor Acceptor { get; }

      TVisitor Visitor { get; }
   }

   public interface AcceptorSetup<out TAcceptor, out TVisitor, in TVertexDelegate, in TEdgeDelegate> : AcceptorSetup<TAcceptor, TVisitor, TVertexDelegate>
   {
      void RegisterEdgeAcceptor( Int32 edgeID, TEdgeDelegate enter, TEdgeDelegate exit );
   }

   // Manual transition acceptors

   public interface ManualTransitionAcceptor_NoContext<out TAcceptor, TElement> : AcceptorSetup<TAcceptor, ExplicitTypeBasedVisitor<TElement>, AcceptVertexExplicitDelegate<TElement>>
   {

   }

   public interface ManualTransitionAcceptor_WithContext<out TAcceptor, TElement, TContext> : AcceptorSetup<TAcceptor, ExplicitTypeBasedVisitor<TElement>, AcceptVertexExplicitDelegate<TElement, TContext>>
   {

   }

   public interface ManualTransitionAcceptor_WithReturnValue<out TAcceptor, TElement, TResult> : AcceptorSetup<TAcceptor, ExplicitTypeBasedVisitor<TElement>, AcceptVertexExplicitWithResultDelegate<TElement, TResult>>
   {

   }

   public interface ManualTransitionAcceptor_WithContextAndReturnValue<out TAcceptor, TElement, TContext, TResult> : AcceptorSetup<TAcceptor, ExplicitTypeBasedVisitor<TElement>, AcceptVertexExplicitWithResultDelegate<TElement, TContext, TResult>>
   {

   }

   // Automatic transition acceptors

   public interface AutomaticTransitionAcceptor_NoContext<out TAcceptor, TElement, TEdgeInfo> : AcceptorSetup<TAcceptor, TypeBasedVisitor<TElement, TEdgeInfo>, AcceptVertexDelegate<TElement>, AcceptEdgeDelegate<TElement, TEdgeInfo>>
   {

   }

   public interface AutomaticTransitionAcceptor_WithContext<out TAcceptor, TElement, TEdgeInfo, TContext> : AcceptorSetup<TAcceptor, TypeBasedVisitor<TElement, TEdgeInfo>, AcceptVertexDelegate<TElement, TContext>, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext>>
   {

   }

   //public interface AutomaticTransitionAcceptor_WithReturnValue<TElement, TEdgeInfo, TResult> : AcceptorWithReturnValue<TElement, TResult>, AcceptorSetup<AcceptVertexWithResultDelegate<TElement, TResult>, AcceptEdgeDelegate<TElement, TEdgeInfo>>, VisitorHolder<TypeBasedVisitor<TElement, TEdgeInfo>>
   //{

   //}

   //public interface AutomaticTransitionAcceptor_WithContextAndReturnValue<TElement, TEdgeInfo, TContext, TResult> : AcceptorWithContextAndReturnValue<TElement, TContext, TResult>, AcceptorSetup<AcceptVertexWithResultDelegate<TElement, TContext, TResult>, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext>>, VisitorHolder<TypeBasedVisitor<TElement, TEdgeInfo>>
   //{

   //}

   //public interface VisitorHolder<out TVisitor>
   //{
   //   TVisitor Visitor { get; }
   //}

   // Delegate types

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

   //public delegate TResult AcceptVertexWithResultDelegate<in TElement, out TResult>( TElement element, out Boolean shouldContinue );

   //public delegate TResult AcceptVertexWithResultDelegate<in TElement, in TContext, out TResult>( TElement element, TContext context, out Boolean shouldContinue );

   public delegate void AcceptVertexExplicitDelegate<TElement>( TElement element, AcceptVertexExplicitCallbackDelegate<TElement> acceptor );

   public delegate Boolean AcceptVertexExplicitCallbackDelegate<in TElement>( TElement element );

   public delegate void AcceptVertexExplicitDelegate<TElement, in TContext>( TElement element, TContext context, AcceptVertexExplicitCallbackDelegate<TElement> acceptor );


   public delegate void AcceptVertexExplicitDelegateTyped<TElement, TContext, in TActualElement>( TActualElement element, TContext context, AcceptVertexExplicitCallbackDelegate<TElement> acceptor )
      where TActualElement : TElement;


   public delegate TResult AcceptVertexExplicitWithResultDelegate<TElement, TResult>( TElement element, AcceptVertexExplicitCallbackWithResultDelegate<TElement, TResult> acceptor );

   public delegate TResult AcceptVertexExplicitCallbackWithResultDelegate<in TElement, out TResult>( TElement element );

   public delegate TResult AcceptVertexExplicitWithResultDelegate<TElement, TContext, TResult>( TElement element, TContext context, AcceptVertexExplicitCallbackWithResultDelegate<TElement, TResult> acceptor );

   public enum TopMostTypeVisitingStrategy
   {
      Never,
      IfNotOverridingType,
      Always
   }

   public static partial class AcceptorFactory
   {
      public static AutomaticTransitionAcceptor_NoContext<Acceptor<TElement>, TElement, TEdgeInfo> NewAutomaticAcceptor_NoContext<TElement, TEdgeInfo>(
         TypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex
         )
      {
         return new Implementation.AutomaticTransitionAcceptor_NoContextImpl<TElement, TEdgeInfo>( visitor, topMostVisitingStrategy, continueOnMissingVertex );
      }

      public static AutomaticTransitionAcceptor_WithContext<AcceptorWithContext<TElement, TContext>, TElement, TEdgeInfo, TContext> NewAutomaticAcceptor_WithContext<TElement, TEdgeInfo, TContext>(
         TypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex
         )
      {
         return new Implementation.AutomaticTransitionAcceptor_WithContextImpl<TElement, TEdgeInfo, TContext>( visitor, topMostVisitingStrategy, continueOnMissingVertex );
      }

      public static AutomaticTransitionAcceptor_WithContext<Acceptor<TElement>, TElement, TEdgeInfo, TContext> NewAutomaticAcceptor_WithContext_Caching<TElement, TEdgeInfo, TContext>(
         TypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         Func<TContext> contextFactory,
         Action<TElement, TContext> contextInitializer
         )
      {
         return new Implementation.AutomaticTransitionAcceptor_WithContextImpl_Caching<TElement, TEdgeInfo, TContext>( visitor, topMostVisitingStrategy, continueOnMissingVertex, contextFactory, contextInitializer );
      }

      public static AutomaticTransitionAcceptor_WithContext<AcceptorWithContext<TElement, TAdditionalInfo>, TElement, TEdgeInfo, TContext> NewAutomaticAcceptor_WithHiddenContext_Caching<TElement, TEdgeInfo, TContext, TAdditionalInfo>(
         TypeBasedVisitor<TElement, TEdgeInfo> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy,
         Boolean continueOnMissingVertex,
         Func<TContext> contextFactory,
         Action<TElement, TContext, TAdditionalInfo> contextInitializer
         )
      {
         return new Implementation.AutomaticTransitionAcceptor_WithContextImpl_Caching<TElement, TEdgeInfo, TContext, TAdditionalInfo>( visitor, topMostVisitingStrategy, continueOnMissingVertex, contextFactory, contextInitializer );
      }

      public static ManualTransitionAcceptor_NoContext<Acceptor<TElement>, TElement> NewManualAcceptor_NoContext<TElement>(
         ExplicitTypeBasedVisitor<TElement> visitor
         )
      {
         return new Implementation.ManualTransitionAcceptor_NoContextImpl<TElement>( visitor );
      }

      public static ManualTransitionAcceptor_WithContext<AcceptorWithContext<TElement, TContext>, TElement, TContext> NewManualAcceptor_WithContext<TElement, TContext>(
         ExplicitTypeBasedVisitor<TElement> visitor
         )
      {
         return new Implementation.ManualTransitionAcceptor_WithContextImpl<TElement, TContext>( visitor );
      }

      public static ManualTransitionAcceptor_WithReturnValue<AcceptorWithReturnValue<TElement, TResult>, TElement, TResult> NewManualAcceptor_WithReturnValue<TElement, TResult>(
         ExplicitTypeBasedVisitor<TElement> visitor
         )
      {
         return new Implementation.ManualTransitionAcceptor_WithReturnValueImpl<TElement, TResult>( visitor );
      }

      public static ManualTransitionAcceptor_WithContextAndReturnValue<AcceptorWithContextAndReturnValue<TElement, TContext, TResult>, TElement, TContext, TResult> NewManualAcceptor_WithContextAndReturnValue<TElement, TContext, TResult>(
         ExplicitTypeBasedVisitor<TElement> visitor
         )
      {
         return new Implementation.ManualTransitionAcceptor_WithContextAndReturnValueImpl<TElement, TContext, TResult>( visitor );
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

   public static void RegisterVertexAcceptor<TAcceptor, TVisitor, TElement, TContext, TActualElement>( this AcceptorSetup<TAcceptor, TVisitor, AcceptVertexDelegate<TElement, TContext>> acceptor, AcceptVertexDelegate<TActualElement, TContext> callback )
      where TActualElement : TElement
   {
      acceptor.RegisterVertexAcceptor( typeof( TActualElement ), ( el, ctx ) => callback( (TActualElement) el, ctx ) );
   }

   public static void RegisterEdgeAcceptor<TAcceptor, TVertexDelegate, TEdgeDelegate, TElement, TEdgeInfo>( this AcceptorSetup<TAcceptor, TypeBasedVisitor<TElement, TEdgeInfo>, TVertexDelegate, TEdgeDelegate> acceptor, Type type, String edgeName, TEdgeDelegate enter, TEdgeDelegate exit )
   {
      acceptor.RegisterEdgeAcceptor( acceptor.Visitor.GetEdgeIDOrThrow( type, edgeName ), enter, exit );
   }

   public static TResult Accept<TElement, TResult>( this AcceptorWithReturnValue<TElement, TResult> acceptor, TElement element )
   {
      Boolean success;
      return acceptor.Accept( element, out success );
   }


}