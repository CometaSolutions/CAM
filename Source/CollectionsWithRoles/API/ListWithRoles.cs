using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;
using CommonUtils;
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
using System.Linq;

namespace CollectionsWithRoles.API
{
   /// <summary>
   /// This is common interface for <see cref="CollectionMutable{TValue, TCollectionQuery}"/> and <see cref="ArrayMutable{TValue, TArrayQuery}"/>.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the collection or array.</typeparam>
   /// <typeparam name="TCollectionQuery">The type of <c>query</c> role of this collection or array.</typeparam>
   public interface CollectionWithIndexer<in TValue, out TCollectionQuery> : CollectionWithQueryRole<TCollectionQuery>
      where TCollectionQuery : CollectionQueryWithIndexer<TValue>
   {
      /// <summary>
      /// Sets the element at the specified index.
      /// </summary>
      /// <param name="index">The zero-based index of the element to set.</param>
      /// <value>The element at the specified index.</value>
      /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is not a valid index in this list.</exception>
      TValue this[Int32 index] { set; }
   }

   /// <summary>
   /// This is <c>command</c> role interface for lists. It defines methods which modify list.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the list.</typeparam>
   /// <typeparam name="TListQuery">The type of the <c>query</c> role of this list.</typeparam>
   public interface ListMutable<TValue, out TListQuery> : CollectionMutable<TValue, TListQuery>, CollectionWithIndexer<TValue, TListQuery>
      where TListQuery : ListQuery<TValue>
   {

      /// <summary>
      /// Inserts an item to this list at the specified index.
      /// </summary>
      /// <param name="index">The zero-based index at which item should be inserted.</param>
      /// <param name="item">The object to insert into this list.</param>
      /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is not a valid index in this list.</exception>
      void Insert( Int32 index, TValue item );

      /// <summary>
      /// Removes the item at the specified index.
      /// </summary>
      /// <param name="index">The zero-based index of the item to remove.</param>
      /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is not a valid index in this list.</exception>
      void RemoveAt( Int32 index );
   }

   /// <summary>
   /// This is a wrapper around <see cref="CollectionQueryWithIndexerAndCount{TValue}"/> that provides functionality of <see cref="CollectionQueryWithIndexerAndCount{TValueOther}"/> with other item type.
   /// </summary>
   /// <typeparam name="TValueThis">The element type of <see cref="CollectionQueryWithIndexerAndCount{T}"/>. The values of original list will be cast to this type.</typeparam>
   /// <typeparam name="TValueOther">The element type of <see cref="CollectionQueryWithIndexerAndCount{T}"/>.</typeparam>
   /// <remarks>
   /// One possible use for this class is when one has a non-generic parent type and generic sub-type, both exposing conceptually same collection, but non-generic parent type needs to use <see cref="Object"/> as the collection element type.
   /// </remarks>
   public sealed class CollectionQueryWithIndexerAndCountWrapper<TValueThis, TValueOther> : CollectionQueryWithIndexerAndCount<TValueThis>
   {
      private readonly CollectionQueryWithIndexerAndCount<TValueOther> _list;

      /// <summary>
      /// Creates new instance of <see cref="CollectionQueryWithIndexerAndCountWrapper{T, U}"/>, providing functionality of <see cref="CollectionQueryWithIndexerAndCount{T}"/> with possibly different type that the elements of given list.
      /// </summary>
      /// <param name="list">The <see cref="ListQuery{T}"/> to wrap.</param>
      public CollectionQueryWithIndexerAndCountWrapper( CollectionQueryWithIndexerAndCount<TValueOther> list )
      {
         ArgumentValidator.ValidateNotNull( "List", list );

         this._list = list;
      }

      /// <inheritdoc />
      public TValueThis this[Int32 index]
      {
         get
         {
            return (TValueThis) (Object) this._list[index];
         }
      }

      /// <inheritdoc />
      public IEnumerator<TValueThis> GetEnumerator()
      {
         return this._list.Select( i => (TValueThis) (Object) i ).GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return this.GetEnumerator();
      }

      /// <inheritdoc />
      public Int32 Count
      {
         get
         {
            return this._list.Count;
         }
      }
   }

   /// <summary>
   /// This is the <c>query</c> role for lists. It defines methods which acquire information about the list without modifying it.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the list.</typeparam>
   public interface ListQuery<TValue> : CollectionQuery<TValue>, CollectionQueryWithIndexerAndCount<TValue>
   {
      /// <summary>
      /// Determines the index of a specific item in this list.
      /// </summary>
      /// <param name="item">The object to locate in this list.</param>
      /// <returns>The index of item if found in the list; otherwise, -1.</returns>
      Int32 IndexOf( TValue item );
   }

   /// <summary>
   /// This is a <c>command</c> role wrapper around <see cref="System.Collections.Generic.IList{T}"/> that allows Command-Query Separation for normal lists.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the list.</typeparam>
   public interface ListProxy<TValue> :
      ListMutable<TValue, ListQuery<TValue>>,
      Mutable<ListProxyQuery<TValue>, ListQuery<TValue>>
   {

   }

   /// <summary>
   /// This is a <c>query</c> role wrapper around <see cref="System.Collections.Generic.IList{T}"/> that allows Command-Query Separation for normal lists.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the list.</typeparam>
   public interface ListProxyQuery<TValue> :
      ListQuery<TValue>,
      MutableQuery<ListQuery<TValue>>
   {

   }

   /// <summary>
   /// This is <c>command</c> role for list, which supports Command-Query Separation of its elements.
   /// </summary>
   /// <typeparam name="TValue">The type of <c>command</c> role of the elements of this list.</typeparam>
   /// <typeparam name="TValueQuery">The type of <c>query</c> role of the elements of this list.</typeparam>
   /// <typeparam name="TValueImmutable">The type of <c>immutable query</c> role of the elements of this list.</typeparam>
   public interface ListWithRoles<TValue, TValueQuery, TValueImmutable> : CollectionWithRoles<ListQueryOfMutables<TValue, TValueQuery, TValueImmutable>, ListQueryOfQueries<TValueQuery, TValueImmutable>, ListQuery<TValueImmutable>, TValue, TValueQuery, TValueImmutable>, ListMutable<TValue, ListQueryOfMutables<TValue, TValueQuery, TValueImmutable>>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
   }

   /// <summary>
   /// This is <c>query of mutables</c> role for list, which supports Command-Query Separation of its elements.
   /// </summary>
   /// <typeparam name="TValue">The type of <c>command</c> role of the elements of this list.</typeparam>
   /// <typeparam name="TValueQuery">The type of <c>query</c> role of the elements of this list.</typeparam>
   /// <typeparam name="TValueImmutable">The type of <c>immutable query</c> role of the elements of this list.</typeparam>
   public interface ListQueryOfMutables<TValue, TValueQuery, TValueImmutable> : CollectionQueryOfMutables<ListQueryOfQueries<TValueQuery, TValueImmutable>, ListQuery<TValueImmutable>, TValue, TValueQuery, TValueImmutable>, ListQuery<TValue>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
   }

   /// <summary>
   /// This is <c>query of queries</c> role for list, which supports Command-Query Separation of its elements.
   /// </summary>
   /// <typeparam name="TValueQuery">The type of <c>query</c> role of the elements of this list.</typeparam>
   /// <typeparam name="TValueImmutable">The type of <c>immutable query</c> role of the elements of this list.</typeparam>
   public interface ListQueryOfQueries<TValueQuery, TValueImmutable> : CollectionQueryOfQueries<ListQuery<TValueImmutable>, TValueQuery, TValueImmutable>, ListQuery<TValueQuery>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
   }
}

public static partial class E_CWR
{
   /// <summary>
   /// Creates a new <see cref="ListProxy{TValue}"/> from given enumerable using default <see cref="CollectionsFactory"/>.
   /// </summary>
   /// <typeparam name="T">The type of enumerable items.</typeparam>
   /// <param name="enumerable">The <see cref="IEnumerable{T}"/>.</param>
   /// <returns>A new <see cref="ListProxy{TValue}"/> with elements from <paramref name="enumerable"/>.</returns>
   /// <seealso cref="ListProxy{TValue}"/>
   /// <seealso cref="CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY"/>
   public static ListProxy<T> ToListProxy<T>( this IEnumerable<T> enumerable )
   {
      return enumerable.ToList().AsListProxy();
   }

   /// <summary>
   /// Creates a new <see cref="ListProxy{TValue}"/> from given enumerable using default <see cref="CollectionsFactory"/>.
   /// </summary>
   /// <typeparam name="T">The type of enumerable items.</typeparam>
   /// <param name="list">The list of <typeparamref name="T"/> typed elements.</param>
   /// <returns>A new <see cref="ListProxy{TValue}"/> with contents directly accessing given <paramref name="list"/>.</returns>
   /// <seealso cref="ListProxy{TValue}"/>
   /// <seealso cref="CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY"/>
   public static ListProxy<T> AsListProxy<T>( this IList<T> list )
   {
      return CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy( list );
   }
}