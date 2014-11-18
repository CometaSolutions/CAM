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

namespace CollectionsWithRoles.API
{
   /// <summary>
   /// This is common interface for collections and arrays.
   /// </summary>
   /// <typeparam name="TCollectionQuery">The type of <c>query</c> role of this collection or array.</typeparam>
   public interface CollectionWithQueryRole<out TCollectionQuery>
   {
      /// <summary>
      /// Returns the <c>query</c> role of this collection or array. The returned object can not be casted back to this interface.
      /// </summary>
      /// <value>The <c>query</c> role of this collection or array.</value>
      TCollectionQuery CQ { get; }
   }

   /// <summary>
   /// This is <c>command</c> role interface for collections. It defines methods which modify collection, in addition to providing getters for other roles of this collection.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the collection.</typeparam>
   /// <typeparam name="TCollectionQuery">The type of the <c>query</c> role of this collection.</typeparam>
   public interface CollectionMutable<TValue, out TCollectionQuery> : CollectionAdditionOnly<TValue>, CollectionWithQueryRole<TCollectionQuery>
      where TCollectionQuery : CollectionQuery<TValue>
   {
      /// <summary>
      /// Removes the first occurrence of a specific object from this collection.
      /// </summary>
      /// <param name="item">The object to remove from this collection.</param>
      /// <returns><c>true</c> if item was successfully removed from this collection; otherwise, <c>false</c>. This method also returns false if item is not found in this collection.</returns>
      Boolean Remove( TValue item );

      /// <summary>
      /// Removes all items from this collection.
      /// </summary>
      void Clear();

      /// <summary>
      /// Returns the <c>addition-only</c> role of this collection. The returned object can not be casted back to this interface.
      /// </summary>
      /// <value>The <c>addition-only</c> role of this collection.</value>
      CollectionAdditionOnly<TValue> AO { get; }
   }

   /// <summary>
   /// This is <c>addition-only</c> role interface for collections. It defines methods which modify collection by adding new elements to it.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the collection.</typeparam>
   public interface CollectionAdditionOnly<in TValue>
   {
      /// <summary>
      /// Adds an item to the this collection.
      /// </summary>
      /// <param name="item">The object to add to this collection.</param>
      void Add( TValue item );

      /// <summary>
      /// Adds the elements of the specified enumerable to this collection.
      /// </summary>
      /// <param name="items">The elements that should be added to this collection. The enumerable itself cannot be <c>null</c>, but it can contain elements that are <c>null</c>, if type <typeparamref name="TValue"/> is a reference type.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="items"/> is <c>null</c>.</exception>
      void AddRange( IEnumerable<TValue> items );
   }

   /// <summary>
   /// This is common interface for <see cref="CollectionQuery{TValue}"/> and <see cref="ArrayQuery{TValue}"/>.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the collection or array.</typeparam>
   public interface CollectionWithCountQuery<out TValue> : IEnumerable<TValue>
   {
      /// <summary>
      /// Gets the number of elements contained in this collection or array.
      /// </summary>
      /// <value>The number of elements contained in this collection or array.</value>
      Int32 Count { get; }
   }

   /// <summary>
   /// This is superinterface for <c>query</c> role of the list and array.
   /// It defines a method for indexing list elements, using <c>out</c> variance for the type parameter.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the collection.</typeparam>
   public interface CollectionQueryWithIndexer<out TValue> : IEnumerable<TValue>
   {
      /// <summary>
      /// Gets the element at the specified index.
      /// </summary>
      /// <param name="index">The zero-based index of the element to get.</param>
      /// <returns>The element at the specified index.</returns>
      /// <value>The element at the specified index.</value>
      /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is not a valid index in this list or array.</exception>
      TValue this[Int32 index] { get; }
   }

   /// <summary>
   /// This interface combines the <see cref="CollectionQueryWithIndexer{T}"/> and <see cref="CollectionWithCountQuery{T}"/> interfaces.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the collection.</typeparam>
   public interface CollectionQueryWithIndexerAndCount<out TValue> : CollectionQueryWithIndexer<TValue>, CollectionWithCountQuery<TValue>
   {

   }

   /// <summary>
   /// This is <c>query</c> role interface for collections. It defines methods which do not modify the collection.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the collection.</typeparam>
   public interface CollectionQuery<TValue> : CollectionWithCountQuery<TValue>
   {

      /// <summary>
      /// Determines whether this collection contains a specific value.
      /// </summary>
      /// <param name="item">The object to locate in this collection.</param>
      /// <param name="equalityComparer">The equality comparer to use when searching for object. If <c>null</c>, then default equality comparer is used.</param>
      /// <returns><c>true</c> if item is found in this collection; otherwise, <c>false</c>.</returns>
      /// <remarks>The <see cref="DictionaryWithRoles{T,U,V,W}"/> currently ignores the <paramref name="equalityComparer"/>.</remarks>
      Boolean Contains( TValue item, IEqualityComparer<TValue> equalityComparer = null );

      /// <summary>
      /// Copies the elements of this collection to an <see cref="System.Array"/>, starting at a particular index.
      /// </summary>
      /// <param name="array">The one-dimensional array that is the destination of the elements copied from this collection. The array must have zero-based indexing.</param>
      /// <param name="arrayOffset">The zero-based index in array at which copying begins.</param>
      /// <exception cref="ArgumentNullException"><paramref name="array"/> is <c>null</c>.</exception>
      /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayOffset"/> is less than 0.</exception>
      /// <exception cref="ArgumentException">The number of elements in this collection is greater than the available space from <paramref name="arrayOffset"/> to the end of the destination array.</exception>
      void CopyTo( TValue[] array, Int32 arrayOffset );
   }

   /// <summary>
   /// Common interface to provide <c>query</c> role of the elements in collections.
   /// </summary>
   /// <typeparam name="TQueries">The type of <c>query</c> role of the elements in collections.</typeparam>
   public interface QueriesProvider<out TQueries>
   {
      /// <summary>
      /// Returns <c>query</c> role of the collection of elements with type of <c>query</c> role of the collection elements.
      /// </summary>
      /// <value>The <c>query</c> role of the collection of elements with type of <c>query</c> role of the collection elements.</value>
      TQueries Queries { get; }
   }

   /// <summary>
   /// This is <c>command</c> role of the interface for all collections which support the Command-Query Separation of its elements.
   /// </summary>
   /// <typeparam name="TMutableQueryRole">The type of collection which provides <c>query</c> role over <c>command</c> -typed (<typeparamref name="TValue"/>) elements; it is also called <c>query of mutables</c> role.</typeparam>
   /// <typeparam name="TQueriesQueryRole">The type of collection which provides <c>query</c> role over <c>query</c> -typed (<typeparamref name="TValueQuery"/>) elements; it is also called <c>query of queries</c> role.</typeparam>
   /// <typeparam name="TImmutableQueryRole">The type of collection which provides <c>immutable query</c> role over <c>immutable query</c> -typed (<typeparamref name="TValueImmutable"/>) elements; it is also called <c>immutable query</c> role.</typeparam>
   /// <typeparam name="TValue">The type of <c>command</c> role of the elements of this collection.</typeparam>
   /// <typeparam name="TValueQuery">The type of <c>query</c> role of the elements of this collection.</typeparam>
   /// <typeparam name="TValueImmutable">The type of <c>immutable query</c> role of the elements of this collection.</typeparam>
   public interface CollectionWithRoles<out TMutableQueryRole, out TQueriesQueryRole, out TImmutableQueryRole, TValue, TValueQuery, TValueImmutable> : Mutable<TMutableQueryRole, TImmutableQueryRole>, CollectionMutable<TValue, TMutableQueryRole>
      where TMutableQueryRole : CollectionQueryOfMutables<TQueriesQueryRole, TImmutableQueryRole, TValue, TValueQuery, TValueImmutable>
      where TQueriesQueryRole : CollectionQueryOfQueries<TImmutableQueryRole, TValueQuery, TValueImmutable>
      where TImmutableQueryRole : CollectionQuery<TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
   }

   /// <summary>
   /// This is <c>query of mutables</c> role of the interface for all collections which support the Command-Query Separation of its elements.
   /// </summary>
   /// <typeparam name="TQueriesQueryRole">The type of collection which provides <c>query</c> role over <c>query</c> -typed (<typeparamref name="TValueQuery"/>) elements.</typeparam>
   /// <typeparam name="TImmutableQueryRole">The type of collection which provides <c>immutable query</c> role over <c>immutable query</c> -typed (<typeparamref name="TValueImmutable"/>) elements.</typeparam>
   /// <typeparam name="TValue">The type of <c>command</c> role of the elements of this collection.</typeparam>
   /// <typeparam name="TValueQuery">The type of <c>query</c> role of the elements of this collection.</typeparam>
   /// <typeparam name="TValueImmutable">The type of <c>immutable query</c> role of the elements of this collection.</typeparam>
   public interface CollectionQueryOfMutables<out TQueriesQueryRole, out TImmutableQueryRole, TValue, TValueQuery, TValueImmutable> : MutableQuery<TImmutableQueryRole>, CollectionQuery<TValue>, QueriesProvider<TQueriesQueryRole>
      where TQueriesQueryRole : CollectionQueryOfQueries<TImmutableQueryRole, TValueQuery, TValueImmutable>
      where TImmutableQueryRole : CollectionQuery<TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
   }

   /// <summary>
   /// This is <c>query of queries</c> role of the interface for all collections which support the Command-Query Separation of its elements.
   /// </summary>
   /// <typeparam name="TImmutableQueryRole">The type of collection which provides <c>immutable query</c> role over <c>immutable query</c> -typed (<typeparamref name="TValueImmutable"/>) elements.</typeparam>
   /// <typeparam name="TValueQuery">The type of <c>query</c> role of the elements of this collection.</typeparam>
   /// <typeparam name="TValueImmutable">The type of <c>immutable query</c> role of the elements of this collection.</typeparam>
   public interface CollectionQueryOfQueries<out TImmutableQueryRole, TValueQuery, TValueImmutable> : MutableQuery<TImmutableQueryRole>, CollectionQuery<TValueQuery>
      where TValueQuery : MutableQuery<TValueImmutable>
      where TImmutableQueryRole : CollectionQuery<TValueImmutable>
   {
   }
}
