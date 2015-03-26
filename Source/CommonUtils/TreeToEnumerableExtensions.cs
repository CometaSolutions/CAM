/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
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
using System.Linq;
using System.Collections.Generic;

namespace CommonUtils
{
   /// <summary>
   /// Extension method holder to enumerate graphs as depth first IEnumerables and breadth first IEnumerables. Additionally contains method to enumerate single chain as enumerable.
   /// </summary>
   /// <remarks>
   /// The <see cref="E_TreeToEnumerable.AsDepthFirstEnumerable"/> and <see cref="E_TreeToEnumerable.AsBreadthFirstEnumerable"/> methods are copy-pasted from <see href="http://www.claassen.net/geek/blog/2009/06/searching-tree-of-objects-with-linq.html"/>.
   /// </remarks>
   public static class E_TreeToEnumerable
   {
      /// <summary>
      /// Using a starting node and function to get children, returns enumerable which walks transitively through all nodes accessible from the starting node, in depth-first order. Does not check for loops, so StackOverflowException is guaranteed if there are loops.
      /// </summary>
      /// <typeparam name="T">The type of the node</typeparam>
      /// <param name="head">Starting node</param>
      /// <param name="childrenFunc">Function to return children given a single node</param>
      /// <param name="returnHead">Whether to return <paramref name="head"/> as first element of resulting enumerable.</param>
      /// <returns>Enumerable to walk through all nodes accessible from start node, in depth-first order</returns>
      public static IEnumerable<T> AsDepthFirstEnumerable<T>( this T head, Func<T, IEnumerable<T>> childrenFunc, Boolean returnHead = true )
      {
         if ( returnHead )
         {
            yield return head;
         }

         foreach ( var node in childrenFunc( head ) )
         {
            foreach ( var child in AsDepthFirstEnumerable( node, childrenFunc ) )
            {
               yield return child;
            }
         }
      }

      /// <summary>
      /// Using a starting node and function to get children, returns enumerable which walks transitively through all nodes accessible from the starting node, in breadth-first order. Does not check for loops, so StackOverflowException is guaranteed if there are loops. 
      /// </summary>
      /// <typeparam name="T">The type of the node</typeparam>
      /// <param name="head">Starting node</param>
      /// <param name="childrenFunc">Function to return children given a single node</param>
      /// <param name="returnHead">Whether to return <paramref name="head"/> as first element of resulting enumerable.</param>
      /// <returns>Enumerable to walk through all nodes accessible from start node, in breadth-first order</returns>
      public static IEnumerable<T> AsBreadthFirstEnumerable<T>( this T head, Func<T, IEnumerable<T>> childrenFunc, Boolean returnHead = true )
      {
         if ( returnHead )
         {
            yield return head;
         }
         var last = head;
         foreach ( var node in AsBreadthFirstEnumerable( head, childrenFunc ) )
         {
            foreach ( var child in childrenFunc( node ) )
            {
               yield return child;
               last = child;
            }
            if ( Object.Equals( last, node ) )
            {
               yield break;
            }
         }
      }

      /// <summary>
      /// Using a starting node and function get child, returns enumerable which walks transitively through all nodes accessible from the starting node. Does not check for loops, so StackOverflowException is guaranteed if there are loops.
      /// </summary>
      /// <typeparam name="T">The type of the node.</typeparam>
      /// <param name="head">Starting node.</param>
      /// <param name="childFunc">Function to return child given a single node.</param>
      /// <param name="endCondition">Customizable condition to end enumeration. By default it will end when the child returned by <paramref name="childFunc"/> will be <c>default(T)</c></param>
      /// <param name="includeFirst">Whether to include <paramref name="head"/> in the result. Note that if this is <c>false</c>, the <paramref name="childFunc"/> will be invoked on the <paramref name="head"/> without checking the end-condition.</param>
      /// <returns>Enumerable to walk through all nodes accessible from the start node.</returns>
      public static IEnumerable<T> AsSingleBranchEnumerable<T>( this T head, Func<T, T> childFunc, Func<T, Boolean> endCondition = null, Boolean includeFirst = true )
      {
         // Check end condition variable
         if ( endCondition == null )
         {
            var def = default( T );
            endCondition = x => Object.Equals( x, def );
         }
         if ( !includeFirst )
         {
            head = childFunc( head );
         }

         while ( !endCondition( head ) )
         {
            yield return head;
            head = childFunc( head );
         }
      }
   }
}
