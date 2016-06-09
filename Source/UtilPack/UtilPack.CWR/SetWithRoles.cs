/*
 * Copyright 2012 Stanislav Muhametsin. All rights Reserved.
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

namespace UtilPack.CollectionsWithRoles
{
   /// <summary>
   /// This is <c>command</c> role interface for sets. It defines methods to modify the set.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the set.</typeparam>
   /// <typeparam name="TSetQuery">The type of <c>query</c> role of this set.</typeparam>
   public interface SetMutable<TValue, TSetQuery> : CollectionMutable<TValue, TSetQuery>
      where TSetQuery : SetQuery<TValue>
   {
      /// <summary>
      /// Adds an element to the current set and returns a value to indicate if the element was successfully added.
      /// </summary>
      /// <returns><c>true</c> if the element is added to the set; <c>false</c> if the element is already in the set.</returns>
      /// <param name="item">The element to add to the set.</param>
      new Boolean Add( TValue item );

      /// <summary>
      /// Modifies the current set so that it contains all elements that are present in both the current set and in the specified collection.
      /// </summary>
      /// <param name="other">The collection to compare to the current set.</param>
      /// <exception cref="System.ArgumentNullException"><paramref name="other" /> is <c>null</c>.</exception>
      void UnionWith( IEnumerable<TValue> other );

      /// <summary>
      /// Modifies the current set so that it contains only elements that are also in a specified collection.
      /// </summary>
      /// <param name="other">The collection to compare to the current set.</param>
      /// <exception cref="System.ArgumentNullException"><paramref name="other" /> is <c>null</c>.</exception>
      void IntersectWith( IEnumerable<TValue> other );

      /// <summary>
      /// Removes all elements in the specified collection from the current set.
      /// </summary>
      /// <param name="other">The collection of items to remove from the set.</param>
      /// <exception cref="System.ArgumentNullException"><paramref name="other" /> is <c>null</c>.</exception>
      void ExceptWith( IEnumerable<TValue> other );

      /// <summary>
      /// Modifies the current set so that it contains only elements that are present either in the current set or in the specified collection, but not both.
      /// </summary>
      /// <param name="other">The collection to compare to the current set.</param>
      /// <exception cref="System.ArgumentNullException"><paramref name="other" /> is <c>null</c>.</exception>
      void SymmetricExceptWith( IEnumerable<TValue> other );

   }

   /// <summary>
   /// This is <c>query</c> role interface for sets. It defines methods to access the set without modifying it.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the set.</typeparam>
   public interface SetQuery<TValue> : CollectionQuery<TValue>
   {
      /// <summary>
      /// Determines whether a set is a subset of a specified collection.
      /// </summary>
      /// <returns>true if the current set is a subset of <paramref name="other" />; otherwise, false.</returns>
      /// <param name="other">The collection to compare to the current set.</param>
      /// <exception cref="System.ArgumentNullException"><paramref name="other" /> is <c>null</c>.</exception>
      Boolean IsSubsetOf( IEnumerable<TValue> other );

      /// <summary>
      /// Determines whether the current set is a superset of a specified collection.
      /// </summary>
      /// <returns>true if the current set is a superset of <paramref name="other" />; otherwise, false.</returns>
      /// <param name="other">The collection to compare to the current set.</param>
      /// <exception cref="System.ArgumentNullException"><paramref name="other" /> is <c>null</c>.</exception>
      Boolean IsSupersetOf( IEnumerable<TValue> other );

      /// <summary>
      /// Determines whether the current set is a correct superset of a specified collection.
      /// </summary>
      /// <returns>true if the <see cref="System.Collections.Generic.ISet{T}" /> object is a correct superset of <paramref name="other" />; otherwise, false.</returns>
      /// <param name="other">The collection to compare to the current set. </param>
      /// <exception cref="System.ArgumentNullException"><paramref name="other" /> is <c>null</c>.</exception>
      Boolean IsProperSupersetOf( IEnumerable<TValue> other );

      /// <summary>
      /// Determines whether the current set is a property (strict) subset of a specified collection.
      /// </summary>
      /// <returns>true if the current set is a correct subset of <paramref name="other" />; otherwise, false.</returns>
      /// <param name="other">The collection to compare to the current set.</param>
      /// <exception cref="System.ArgumentNullException"><paramref name="other" /> is <c>null</c>.</exception>
      Boolean IsProperSubsetOf( IEnumerable<TValue> other );

      /// <summary>
      /// Determines whether the current set overlaps with the specified collection.
      /// </summary>
      /// <returns>true if the current set and <paramref name="other" /> share at least one common element; otherwise, false.</returns>
      /// <param name="other">The collection to compare to the current set.</param>
      /// <exception cref="System.ArgumentNullException"><paramref name="other" /> is <c>null</c>.</exception>
      Boolean Overlaps( IEnumerable<TValue> other );

      /// <summary>
      /// Determines whether the current set and the specified collection contain the same elements.
      /// </summary>
      /// <returns>true if the current set is equal to <paramref name="other" />; otherwise, false.</returns>
      /// <param name="other">The collection to compare to the current set.</param>
      /// <exception cref="System.ArgumentNullException"><paramref name="other" /> is <c>null</c>.</exception>
      Boolean SetEquals( IEnumerable<TValue> other );
   }

   /// <summary>
   /// This is <c>command</c> role for sets, which support Command-Query Separation of its elements.
   /// </summary>
   /// <typeparam name="TValue">The type of <c>command</c> role of the elements of this set.</typeparam>
   /// <typeparam name="TValueQuery">The type of <c>query</c> role of the elements of this set.</typeparam>
   /// <typeparam name="TValueImmutable">The type of <c>immutable query</c> role of the elements of this set.</typeparam>
   public interface SetWithRoles<TValue, TValueQuery, TValueImmutable> : CollectionWithRoles<SetQueryOfMutables<TValue, TValueQuery, TValueImmutable>, SetQueryOfQueries<TValueQuery, TValueImmutable>, SetQuery<TValueImmutable>, TValue, TValueQuery, TValueImmutable>, SetMutable<TValue, SetQueryOfMutables<TValue, TValueQuery, TValueImmutable>>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
   }

   /// <summary>
   /// This is <c>query of mutables</c> role for sets, which support Command-Query Separation of its elements.
   /// </summary>
   /// <typeparam name="TValue">The type of <c>command</c> role of the elements of this set.</typeparam>
   /// <typeparam name="TValueQuery">The type of <c>query</c> role of the elements of this set.</typeparam>
   /// <typeparam name="TValueImmutable">The type of <c>immutable query</c> role of the elements of this set.</typeparam>
   public interface SetQueryOfMutables<TValue, TValueQuery, TValueImmutable> : CollectionQueryOfMutables<SetQueryOfQueries<TValueQuery, TValueImmutable>, SetQuery<TValueImmutable>, TValue, TValueQuery, TValueImmutable>, SetQuery<TValue>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {

   }

   /// <summary>
   /// Thi is <c>query of queries</c> role for sets, which support Command-Query Separation of its elements.
   /// </summary>
   /// <typeparam name="TValueQuery">The type of <c>query</c> role of the elements of this set.</typeparam>
   /// <typeparam name="TValueImmutable">The type of <c>immutable query</c> role of the elements of this set.</typeparam>
   public interface SetQueryOfQueries<TValueQuery, TValueImmutable> : CollectionQueryOfQueries<SetQuery<TValueImmutable>, TValueQuery, TValueImmutable>, SetQuery<TValueQuery>
      where TValueQuery : MutableQuery<TValueImmutable>
   {

   }

   /// <summary>
   /// This is a <c>command</c> role wrapper around <see cref="System.Collections.Generic.ISet{T}"/> that allows Command-Query Separation for normal sets.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the set.</typeparam>
   public interface SetProxy<TValue> :
      Mutable<SetProxyQuery<TValue>, SetQuery<TValue>>,
      SetMutable<TValue, SetQuery<TValue>>
   {

   }

   /// <summary>
   /// This is a <c>query</c> role wrapper around <see cref="System.Collections.Generic.ISet{T}"/> that allows Command-Query Separation for normal lists.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the set.</typeparam>
   public interface SetProxyQuery<TValue> :
      MutableQuery<SetQuery<TValue>>,
      SetQuery<TValue>
   {

   }
}
