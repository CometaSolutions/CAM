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
   public interface AcceptorSetup<in TVertexDelegate>
   {
      void RegisterVertexAcceptor( Type type, TVertexDelegate vertexAcceptor );
   }

   public interface AcceptorSetup<in TVertexDelegate, in TEdgeDelegate> : AcceptorSetup<TVertexDelegate>
   {
      void RegisterEdgeAcceptor( Int32 edgeID, TEdgeDelegate enter, TEdgeDelegate exit );
   }

   // Manual transition acceptors

   public interface ManualTransitionAcceptor_NoContext<TElement> : Acceptor<TElement>, AcceptorSetup<AcceptVertexExplicitDelegate<TElement>>, VisitorHolder<ExplicitTypeBasedVisitor<TElement>>
   {

   }

   public interface ManualTransitionAcceptor_WithContext<TElement, TContext> : AcceptorWithContext<TElement, TContext>, AcceptorSetup<AcceptVertexExplicitDelegate<TElement, TContext>>, VisitorHolder<ExplicitTypeBasedVisitor<TElement>>
   {

   }

   public interface ManualTransitionAcceptor_WithReturnValue<TElement, TResult> : AcceptorWithReturnValue<TElement, TResult>, AcceptorSetup<AcceptVertexExplicitWithResultDelegate<TElement, TResult>>, VisitorHolder<ExplicitTypeBasedVisitor<TElement>>
   {

   }

   public interface ManualTransitionAcceptor_WithContextAndReturnValue<TElement, TContext, TResult> : AcceptorWithContextAndReturnValue<TElement, TContext, TResult>, AcceptorSetup<AcceptVertexExplicitWithResultDelegate<TElement, TContext, TResult>>, VisitorHolder<ExplicitTypeBasedVisitor<TElement>>
   {

   }

   // Automatic transition acceptors

   public interface AutomaticTransitionAcceptor_NoContext<TElement, TEdgeInfo> : Acceptor<TElement>, AcceptorSetup<AcceptVertexDelegate<TElement>, AcceptEdgeDelegate<TElement, TEdgeInfo>>, VisitorHolder<TypeBasedVisitor<TElement, TEdgeInfo>>
   {

   }

   public interface AutomaticTransitionAcceptor_WithContext<TElement, TEdgeInfo, TContext> : AcceptorWithContext<TElement, TContext>, AcceptorSetup<AcceptVertexDelegate<TElement, TContext>, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext>>, VisitorHolder<TypeBasedVisitor<TElement, TEdgeInfo>>
   {

   }

   public interface AutomaticTransitionAcceptor_WithReturnValue<TElement, TEdgeInfo, TResult> : AcceptorWithReturnValue<TElement, TResult>, AcceptorSetup<AcceptVertexWithResultDelegate<TElement, TResult>, AcceptEdgeDelegate<TElement, TEdgeInfo>>, VisitorHolder<TypeBasedVisitor<TElement, TEdgeInfo>>
   {

   }

   public interface AutomaticTransitionAcceptor_WithContextAndReturnValue<TElement, TEdgeInfo, TContext, TResult> : AcceptorWithContextAndReturnValue<TElement, TContext, TResult>, AcceptorSetup<AcceptVertexWithResultDelegate<TElement, TContext, TResult>, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext>>, VisitorHolder<TypeBasedVisitor<TElement, TEdgeInfo>>
   {

   }

   public interface VisitorHolder<TVisitor>
   {
      TVisitor Visitor { get; }
   }

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

   public delegate TResult AcceptVertexWithResultDelegate<in TElement, out TResult>( TElement element, out Boolean shouldContinue );

   public delegate TResult AcceptVertexWithResultDelegate<in TElement, in TContext, out TResult>( TElement element, TContext context, out Boolean shouldContinue );

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

   }
}

public static partial class E_UtilPack
{
   public static AcceptVertexExplicitDelegate<TElement, TContext> AsAcceptVertexExplicitDelegate<TElement, TContext, TActualElement>( this AcceptVertexExplicitDelegateTyped<TElement, TContext, TActualElement> typed )
      where TActualElement : TElement
   {
      return ( el, ctx, cb ) => typed( (TActualElement) el, ctx, cb );
   }

   public static void RegisterAcceptor<TElement, TContext, TActualElement>( this AcceptorSetup<AcceptVertexDelegate<TElement, TContext>> acceptor, AcceptVertexDelegate<TActualElement, TContext> callback )
      where TActualElement : TElement
   {
      acceptor.RegisterVertexAcceptor( typeof( TActualElement ), ( el, ctx ) => callback( (TActualElement) el, ctx ) );
   }

   public static void RegisterEdgeAcceptor<TAcceptorSetup, TElement, TEdgeInfo>( this TAcceptorSetup acceptor, Type type, String edgeName, AcceptEdgeDelegate<TElement, TEdgeInfo> enter, AcceptEdgeDelegate<TElement, TEdgeInfo> exit )
      where TAcceptorSetup : AcceptorSetup<AcceptVertexDelegate<TElement>, AcceptEdgeDelegate<TElement, TEdgeInfo>>, VisitorHolder<TypeBasedVisitor<TElement, TEdgeInfo>>
   {
      acceptor.RegisterEdgeAcceptor( acceptor.Visitor.GetEdgeIDOrThrow( type, edgeName ), enter, exit );
   }

   public static void RegisterEdgeAcceptor<TAcceptorSetup, TElement, TEdgeInfo, TContext>( this TAcceptorSetup acceptor, Type type, String edgeName, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> enter, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext> exit )
      where TAcceptorSetup : AcceptorSetup<AcceptVertexDelegate<TElement, TContext>, AcceptEdgeDelegate<TElement, TEdgeInfo, TContext>>, VisitorHolder<TypeBasedVisitor<TElement, TEdgeInfo>>
   {
      acceptor.RegisterEdgeAcceptor( acceptor.Visitor.GetEdgeIDOrThrow( type, edgeName ), enter, exit );
   }

   public static TResult Accept<TElement, TResult>( this ManualTransitionAcceptor_WithReturnValue<TElement, TResult> acceptor, TElement element )
   {
      Boolean success;
      return acceptor.Accept( element, out success );
   }


}