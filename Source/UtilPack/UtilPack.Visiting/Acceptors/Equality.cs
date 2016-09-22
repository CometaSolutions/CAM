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
using UtilPack.Visiting;
using UtilPack.Visiting.Implementation;

namespace UtilPack.Visiting
{

   /// <summary>
   /// This class contains methods to easily create certain kinds of acceptors.
   /// </summary>
   public static partial class AcceptorFactory
   {
      /// <summary>
      /// Creates a new <see cref="AutomaticTransitionAcceptor_WithContext{TAcceptor, TElement, TEdgeInfo, TContext}"/> for equality comparison functionality, with given visitor and <see cref="TopMostTypeVisitingStrategy"/>.
      /// </summary>
      /// <param name="visitor">The visitor.</param>
      /// <param name="topMostVisitingStrategy">The <see cref="TopMostTypeVisitingStrategy"/>.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="visitor"/> is <c>null</c>.</exception>
      public static AutomaticTransitionAcceptor_WithContext<AcceptorWithContext<TElement, TElement>, TElement, Int32, ObjectGraphEqualityContext<TElement>> NewEqualityComparisonAcceptor<TElement>(
         TypeBasedVisitor<TElement, Int32> visitor,
         TopMostTypeVisitingStrategy topMostVisitingStrategy
         )
      {
         return NewAutomaticAcceptor_WithHiddenContext_Caching(
            visitor,
            topMostVisitingStrategy,
            false,
            () => new ObjectGraphEqualityContext<TElement>(),
            ( TElement obj, ObjectGraphEqualityContext<TElement> ctx, TElement otherObj ) =>
            {
               ctx.CurrentElementStack.Clear();
               ctx.CurrentElementStack.Push( otherObj );
            } );
      }

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TElement"></typeparam>
      /// <param name="visitor"></param>
      /// <returns></returns>
      public static ManualTransitionAcceptor_WithReturnValue<AcceptorWithReturnValue<TElement, Int32>, TElement, Int32> NewHashCodeComputationAcceptor<TElement>(
         ExplicitTypeBasedVisitor<TElement> visitor
         )
      {
         return NewManualAcceptor_WithReturnValue<TElement, Int32>(
            visitor
            );
      }
   }

   /// <summary>
   /// This is base class for code using visitor pattern to test whether object hierarchies are equal.
   /// </summary>
   /// <typeparam name="TElement">The top-level type of the elements.</typeparam>
   public class ObjectGraphEqualityContext<TElement>
   {
      /// <summary>
      /// Creates a new instance of <see cref="ObjectGraphEqualityContext{TElement}"/> with given starting element.
      /// </summary>
      public ObjectGraphEqualityContext()
      {
         this.CurrentElementStack = new Stack<TElement>();
      }

      /// <summary>
      /// Gets the current stack of the objects.
      /// </summary>
      /// <value>The current stack of the objects.</value>
      /// <remarks>
      /// Whenever visiting other object, on-enter-edge acceptor should push to this stack, and on-exit-edge acceptor should pop from this stack.
      /// </remarks>
      public Stack<TElement> CurrentElementStack { get; }
   }
}

#pragma warning disable 1591
public static partial class E_UtilPack
#pragma warning restore 1591
{
   /// <summary>
   /// Gets the current element from <see cref="ObjectGraphEqualityContext{TElement}"/> without modifying stack.
   /// </summary>
   /// <typeparam name="TElement">The type of element.</typeparam>
   /// <param name="ctx">The <see cref="ObjectGraphEqualityContext{TElement}"/>.</param>
   /// <returns>The current element from <see cref="ObjectGraphEqualityContext{TElement}"/>.</returns>
   public static TElement GetCurrentElement<TElement>( this ObjectGraphEqualityContext<TElement> ctx )
   {
      return ctx.CurrentElementStack.Peek();
   }

   /// <summary>
   /// Helper method to register equality functionality for given type.
   /// </summary>
   /// <typeparam name="TElement">The base element type.</typeparam>
   /// <typeparam name="TEdgeInfo">The edge info type.</typeparam>
   /// <typeparam name="TContext">The context type, should be <see cref="ObjectGraphEqualityContext{TElement}"/> or subtype of it.</typeparam>
   /// <typeparam name="TActualElement">The type of the element that equality functionality is registered for.</typeparam>
   /// <param name="acceptor">The <see cref="AutomaticTransitionAcceptor_WithContext{TAcceptor, TElement, TEdgeInfo, TContext}"/> created by <see cref="AcceptorFactory.NewEqualityComparisonAcceptor{TElement}(TypeBasedVisitor{TElement, int}, TopMostTypeVisitingStrategy)"/>.</param>
   /// <param name="equality">The callback comparing two objects of type <typeparamref name="TActualElement"/>.</param>
   public static void RegisterEqualityAcceptor<TElement, TEdgeInfo, TContext, TActualElement>( this AutomaticTransitionAcceptor_WithContext<AcceptorWithContext<TElement, TElement>, TElement, TEdgeInfo, TContext> acceptor, Equality<TActualElement> equality )
      where TElement : class
      where TActualElement : class, TElement
      where TContext : ObjectGraphEqualityContext<TElement>
   {
      acceptor.RegisterVertexAcceptor(
         typeof( TActualElement ),
         ( el, ctx ) =>
         {
            var fromCtx = ctx.GetCurrentElement();
            TActualElement fromCtxTyped;
            return ReferenceEquals( el, fromCtx )
            || ( el != null && ( fromCtxTyped = fromCtx as TActualElement ) != null
            && equality( (TActualElement) el, fromCtxTyped ) );
         }
         );
   }

   /// <summary>
   /// This is helper method to invoke <see cref="AcceptorSetup{TAcceptor, TVertexDelegate, TEdgeDelegate}.RegisterEdgeAcceptor(int, TEdgeDelegate, TEdgeDelegate)"/> method with arguments suitable for equality comparison of hierarchical structures.
   /// </summary>
   /// <typeparam name="TElement">The common type of elements.</typeparam>
   /// <typeparam name="TEdgeInfo">The type of edge information.</typeparam>
   /// <typeparam name="TContext">The context type, must be subclass of <see cref="ObjectGraphEqualityContext{TElement}"/>.</typeparam>
   /// <typeparam name="TActualElement">The type of this element.</typeparam>
   /// <param name="acceptor">The <see cref="AutomaticTransitionAcceptor_WithContext{TAcceptor, TElement, TEdgeInfo, TContext}"/> created by <see cref="AcceptorFactory.NewEqualityComparisonAcceptor{TElement}(TypeBasedVisitor{TElement, int}, TopMostTypeVisitingStrategy)"/>.</param>
   /// <param name="edgeID">The edge ID.</param>
   /// <param name="getter">The callback to get another element held by element given as parameter.</param>
   public static void RegisterEqualityComparisonTransition_Simple<TElement, TEdgeInfo, TContext, TActualElement>( this AutomaticTransitionAcceptor_WithContext<AcceptorWithContext<TElement, TElement>, TElement, TEdgeInfo, TContext> acceptor, Int32 edgeID, Func<TActualElement, TElement> getter )
      where TElement : class
      where TActualElement : TElement
      where TContext : ObjectGraphEqualityContext<TElement>
   {
      // TODO: optimize the lambda if TActualElement == TElement (no need for cast)
      acceptor.RegisterEdgeAcceptor(
         edgeID,
         ( el, info, ctx ) =>
         {
            ctx.CurrentElementStack.Push( getter( (TActualElement) ctx.GetCurrentElement() ) );
            return true;
         },
         EdgeExit
         );
   }

   /// <summary>
   /// This is helper method to invoke <see cref="AcceptorSetup{TAcceptor, TVertexDelegate, TEdgeDelegate}.RegisterEdgeAcceptor(int, TEdgeDelegate, TEdgeDelegate)"/> method with arguments suitable for equality comparison of hierarchical structures.
   /// </summary>
   /// <typeparam name="TElement">The common type of elements.</typeparam>
   /// <typeparam name="TContext">The context type, must be subclass of <see cref="ObjectGraphEqualityContext{TElement}"/>.</typeparam>
   /// <typeparam name="TActualElement">The type of this element.</typeparam>
   /// <param name="acceptor">The <see cref="AutomaticTransitionAcceptor_WithContext{TAcceptor, TElement, TEdgeInfo, TContext}"/> created by <see cref="AcceptorFactory.NewEqualityComparisonAcceptor{TElement}(TypeBasedVisitor{TElement, int}, TopMostTypeVisitingStrategy)"/>.</param>
   /// <param name="edgeID">The edge ID.</param>
   /// <param name="getter">The callback to get a list of another elements held by element given as parameter.</param>
   public static void RegisterEqualityComparisonTransition_List<TElement, TContext, TActualElement>( this AutomaticTransitionAcceptor_WithContext<AcceptorWithContext<TElement, TElement>, TElement, Int32, TContext> acceptor, Int32 edgeID, Func<TActualElement, IList<TElement>> getter )
      where TElement : class
      where TActualElement : TElement
      where TContext : ObjectGraphEqualityContext<TElement>
   {
      // TODO: optimize the lambda if TActualElement == TElement (no need for cast)
      acceptor.RegisterEdgeAcceptor(
         edgeID,
         ( el, info, ctx ) =>
         {
            ctx.CurrentElementStack.Push( getter( (TActualElement) ctx.GetCurrentElement() )[info] );
            return true;
         },
         EdgeExit
         );
   }

   /// <summary>
   /// This is helper method to invoke <see cref="AcceptorSetup{TAcceptor, TVertexDelegate, TEdgeDelegate}.RegisterEdgeAcceptor(int, TEdgeDelegate, TEdgeDelegate)"/> method with arguments suitable for equality comparison of hierarchical structures.
   /// </summary>
   /// <typeparam name="TElement">The common type of elements.</typeparam>
   /// <typeparam name="TContext">The context type, must be subclass of <see cref="ObjectGraphEqualityContext{TElement}"/>.</typeparam>
   /// <typeparam name="TActualElement">The type of this element.</typeparam>
   /// <param name="acceptor">The <see cref="AutomaticTransitionAcceptor_WithContext{TAcceptor, TElement, TEdgeInfo, TContext}"/> created by <see cref="AcceptorFactory.NewEqualityComparisonAcceptor{TElement}(TypeBasedVisitor{TElement, int}, TopMostTypeVisitingStrategy)"/>.</param>
   /// <param name="edgeID">The edge ID.</param>
   /// <param name="getter">The callback to get an array of another elements held by element given as parameter.</param>
   public static void RegisterEqualityComparisonTransition_Array<TElement, TContext, TActualElement>( this AutomaticTransitionAcceptor_WithContext<AcceptorWithContext<TElement, TElement>, TElement, Int32, TContext> acceptor, Int32 edgeID, Func<TActualElement, TElement[]> getter )
      where TElement : class
      where TActualElement : class, TElement
      where TContext : ObjectGraphEqualityContext<TElement>
   {
      // TODO: optimize the lambda if TActualElement == TElement (no need for cast)
      acceptor.RegisterEdgeAcceptor(
         edgeID,
         ( el, info, ctx ) =>
         {
            ctx.CurrentElementStack.Push( getter( (TActualElement) ctx.GetCurrentElement() )[info] );
            return true;
         },
         EdgeExit
         );
   }

   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="TElement"></typeparam>
   /// <typeparam name="TActualElement"></typeparam>
   /// <param name="acceptor"></param>
   /// <param name="hashCode"></param>
   public static void RegisterHashCodeComputer<TElement, TActualElement>( this ManualTransitionAcceptor_WithReturnValue<AcceptorWithReturnValue<TElement, Int32>, TElement, Int32> acceptor, Func<TActualElement, AcceptVertexExplicitCallbackWithResultDelegate<TElement, Int32>, Int32> hashCode )
      where TActualElement : TElement
   {
      acceptor.RegisterVertexAcceptor(
         typeof( TActualElement ),
         ( el, cb ) => hashCode( (TActualElement) el, cb )
         );
   }

   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="TElement"></typeparam>
   /// <typeparam name="TActualElement"></typeparam>
   /// <param name="acceptor"></param>
   /// <param name="hashCode"></param>
   public static void RegisterHashCodeComputer<TElement, TActualElement>( this ManualTransitionAcceptor_WithReturnValue<AcceptorWithReturnValue<TElement, Int32>, TElement, Int32> acceptor, HashCode<TActualElement> hashCode )
      where TActualElement : TElement
   {
      acceptor.RegisterVertexAcceptor(
         typeof( TActualElement ),
         ( el, cb ) => hashCode( (TActualElement) el )
         );
   }

   //public static void RegisterHashCodeComputer<TElement, TEdgeInfo, TContext, TActualElement>( this CachingTypeBasedAcceptor<TElement, TEdgeInfo, TContext> acceptor, Func<TActualElement, AcceptVertexExplicitCallbackDelegate<TElement, TContext>, Int32> hashCode )
   //   where TActualElement : TElement
   //   where TContext : HashCodeComputationContext
   //{
   //   acceptor.RegisterExplicitVertexAcceptor(
   //      typeof( TActualElement ),
   //      ( el, ctx, cb ) =>
   //      {
   //         ctx.CurrentHashCode = hashCode( (TActualElement) el, cb );
   //         return true;
   //      }
   //      );
   //}

   //public static void RegisterHashCodeComputer<TElement, TEdgeInfo, TContext, TActualElement>( this CachingTypeBasedAcceptor<TElement, TEdgeInfo, TContext> acceptor, HashCode<TActualElement> hashCode )
   //   where TActualElement : TElement
   //   where TContext : HashCodeComputationContext
   //{
   //   acceptor.RegisterExplicitVertexAcceptor(
   //      typeof( TActualElement ),
   //      ( el, ctx, cb ) =>
   //      {
   //         ctx.CurrentHashCode = hashCode( (TActualElement) el );
   //         return true;
   //      }
   //      );
   //}

   ///// <summary>
   ///// Creates an equality comparer, which will use the <see cref="CachingTypeBasedAcceptor{TElement, TEdgeInfo, TContext, TAdditionalInfo}"/> for <see cref="IEqualityComparer{T}.Equals(T, T)"/> method, and a given callback for <see cref="IEqualityComparer{T}.GetHashCode(T)"/> method.
   ///// </summary>
   ///// <typeparam name="TElement">The common type of elements.</typeparam>
   ///// <typeparam name="TContext">The type of context, should be <see cref="ObjectGraphEqualityContext{TElement}"/> or subclass.</typeparam>
   ///// <typeparam name="TActualElement">The type of the element that equality comparison functionality should be added to.</typeparam>
   ///// <param name="acceptor">The <see cref="CachingTypeBasedAcceptor{TElement, TEdgeInfo, TContext, TAdditionalInfo}"/>.</param>
   ///// <param name="hashCode">The hash code callback.</param>
   ///// <returns></returns>
   //public static IEqualityComparer<TActualElement> NewEqualityComparer<TElement, TContext, TActualElement>( this CachingTypeBasedAcceptor<TElement, Int32, TContext, TElement> acceptor, HashCode<TActualElement> hashCode )
   //   where TElement : class
   //   where TActualElement : class, TElement
   //   where TContext : ObjectGraphEqualityContext<TElement>
   //{
   //   return new AcceptorBasedEqualityComparer<TElement, TContext, TActualElement>( acceptor, hashCode );
   //}

   //private sealed class AcceptorBasedEqualityComparer<TElement, TContext, TActualElement> : IEqualityComparer<TActualElement>
   //   where TElement : class
   //   where TActualElement : class, TElement
   //   where TContext : ObjectGraphEqualityContext<TElement>
   //{
   //   internal AcceptorBasedEqualityComparer( CachingTypeBasedAcceptor<TElement, Int32, TContext, TElement> acceptor, HashCode<TActualElement> hashCode )
   //   {
   //      this._acceptor = ArgumentValidator.ValidateNotNullReference( acceptor );
   //      this._hashCode = ArgumentValidator.ValidateNotNull( "Hash code callback", hashCode );
   //   }

   //   private readonly CachingTypeBasedAcceptor<TElement, Int32, TContext, TElement> _acceptor;
   //   private readonly HashCode<TActualElement> _hashCode;

   //   Boolean IEqualityComparer<TActualElement>.Equals( TActualElement x, TActualElement y )
   //   {
   //      return this._acceptor.Accept( x, y );
   //   }

   //   Int32 IEqualityComparer<TActualElement>.GetHashCode( TActualElement obj )
   //   {
   //      return this._hashCode( obj );
   //   }
   //}

   private static Boolean EdgeExit<TElement, TEdgeInfo, TContext>( TElement element, TEdgeInfo info, TContext context )
      where TElement : class
      where TContext : ObjectGraphEqualityContext<TElement>
   {
      context.CurrentElementStack.Pop();
      return true;
   }
}