/*
 * Copyright 2014 Stanislav Muhametsin. All rights Reserved.
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
using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CollectionsWithRoles.API
{
   /// <summary>
   /// This is <c>command</c> role interface for arrays. It defines methods which modify array.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the array.</typeparam>
   /// <typeparam name="TArrayQuery">The type of the <c>query</c> role of this array.</typeparam>
   public interface ArrayMutable<in TValue, out TArrayQuery> : CollectionWithIndexer<TValue, TArrayQuery>
      where TArrayQuery : ArrayQuery<TValue>
   {
   }

   /// <summary>
   /// This is the <c>query</c> role for arrays. It defines methods which acquire information about the array without modifying it.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the list.</typeparam>
   /// <remarks>
   /// Please note that the compiler won't be able to optimize <c>foreach</c> loop over this type like it can with native arrays.
   /// If performance is critical, either use <c>for (var i = 0; i &lt; array.Count; ++i)</c> manually or TODO extension method.
   /// </remarks>
   public interface ArrayQuery<out TValue> : CollectionQueryWithIndexerAndCount<TValue>
   {
   }

   /// <summary>
   /// This is common interface for <see cref="ArrayProxy{T}"/> and <see cref="ArrayWithRoles{T,U,V}"/>, providing a property to get the backing array.
   /// </summary>
   /// <typeparam name="TValue">The type of array elements.</typeparam>
   public interface ArrayHolder<out TValue>
   {
      /// <summary>
      /// Gets the backing array of this <see cref="ArrayProxy{T}"/> or <see cref="ArrayWithRoles{T,U,V}"/>.
      /// </summary>
      /// <value>The backing array of this <see cref="ArrayProxy{T}"/> or <see cref="ArrayWithRoles{T,U,V}"/>.</value>
      TValue[] Array { get; }
   }

   /// <summary>
   /// This is a <c>command</c> role wrapper around array that allows Command-Query Separation for normal arrays.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the list.</typeparam>
   public interface ArrayProxy<TValue> :
      ArrayMutable<TValue, ArrayQuery<TValue>>,
      Mutable<ArrayProxyQuery<TValue>, ArrayQuery<TValue>>,
      ArrayHolder<TValue>
   {

   }

   /// <summary>
   /// This is a <c>query</c> role wrapper around array that allows Command-Query Separation for normal arrays.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements in the list.</typeparam>
   public interface ArrayProxyQuery<out TValue> :
      ArrayQuery<TValue>,
      MutableQuery<ArrayQuery<TValue>>
   {

   }

   /// <summary>
   /// This is <c>command</c> role for array, which supports Command-Query Separation of its elements.
   /// </summary>
   /// <typeparam name="TValue">The type of <c>command</c> role of the elements of this list.</typeparam>
   /// <typeparam name="TValueQuery">The type of <c>query</c> role of the elements of this list.</typeparam>
   /// <typeparam name="TValueImmutable">The type of <c>immutable query</c> role of the elements of this list.</typeparam>
   public interface ArrayWithRoles<TValue, TValueQuery, TValueImmutable> : Mutable<ArrayQueryOfMutables<TValue, TValueQuery, TValueImmutable>, ArrayQuery<TValueImmutable>>, ArrayMutable<TValue, ArrayQueryOfMutables<TValue, TValueQuery, TValueImmutable>>, ArrayHolder<TValue>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
   }

   /// <summary>
   /// This is <c>query of mutables</c> role for array, which supports Command-Query Separation of its elements.
   /// </summary>
   /// <typeparam name="TValue">The type of <c>command</c> role of the elements of this list.</typeparam>
   /// <typeparam name="TValueQuery">The type of <c>query</c> role of the elements of this list.</typeparam>
   /// <typeparam name="TValueImmutable">The type of <c>immutable query</c> role of the elements of this list.</typeparam>
   public interface ArrayQueryOfMutables<TValue, TValueQuery, TValueImmutable> : MutableQuery<ArrayQuery<TValueImmutable>>, ArrayQuery<TValue>, QueriesProvider<ArrayQueryOfQueries<TValueQuery, TValueImmutable>>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
   }

   /// <summary>
   /// This is <c>query of queries</c> role for array, which supports Command-Query Separation of its elements.
   /// </summary>
   /// <typeparam name="TValueQuery">The type of <c>query</c> role of the elements of this list.</typeparam>
   /// <typeparam name="TValueImmutable">The type of <c>immutable query</c> role of the elements of this list.</typeparam>
   public interface ArrayQueryOfQueries<TValueQuery, TValueImmutable> : MutableQuery<ArrayQuery<TValueImmutable>>, ArrayQuery<TValueQuery>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
   }
}

public static partial class E_CWR
{
   /// <summary>
   /// Creates a new <see cref="ArrayProxy{TValue}"/> from given enumerable using default <see cref="CollectionsFactory"/>.
   /// </summary>
   /// <typeparam name="T">The type of enumerable items.</typeparam>
   /// <param name="enumerable">The <see cref="IEnumerable{T}"/>.</param>
   /// <returns>A new <see cref="ArrayProxy{TValue}"/> with elements from <paramref name="enumerable"/>.</returns>
   /// <seealso cref="ArrayProxy{TValue}"/>
   /// <seealso cref="CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY"/>
   public static ArrayProxy<T> ToArrayProxy<T>( this IEnumerable<T> enumerable )
   {
      return CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( enumerable.ToArray() );
   }
}