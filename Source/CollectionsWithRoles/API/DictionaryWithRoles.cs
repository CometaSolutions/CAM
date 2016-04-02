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
using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;
using System;
using System.Collections.Generic;

namespace CollectionsWithRoles.API
{
   /// <summary>
   /// This is <c>command</c> role interface for dictionaries. It defines methods which modify the dictionary.
   /// </summary>
   /// <typeparam name="TKey">The type of the keys of the dictionary.</typeparam>
   /// <typeparam name="TValue">The type of the values of the dictionary.</typeparam>
   /// <typeparam name="TDictionaryQuery">The type of <c>query</c> role of this dictionary.</typeparam>
   public interface DictionaryMutable<TKey, TValue, out TDictionaryQuery> : CollectionMutable<KeyValuePair<TKey, TValue>, TDictionaryQuery>
      where TDictionaryQuery : DictionaryQuery<TKey, TValue>
   {
      /// <summary>
      /// Sets the element with the specified key.
      /// </summary>
      /// <param name="key">The key of the element to set.</param>
      /// <value>The element with the specified key.</value>
      /// <exception cref="ArgumentNullException"><paramref name="key" /> is <c>null</c>.</exception>
      TValue this[TKey key] { set; }

      /// <summary>
      /// Adds an element with the provided key and value to this dictionary.
      /// </summary>
      /// <param name="key">The object to use as the key of the element to add.</param>
      /// <param name="value">The object to use as the value of the element to add.</param>
      /// <exception cref="ArgumentNullException"><paramref name="key" /> is <c>null</c>.</exception>
      /// <exception cref="ArgumentException">An element with the same key already exists in this dictionary.</exception>
      void Add( TKey key, TValue value );

      /// <summary>
      /// Removes the element with the specified key from this dictionary.
      /// </summary>
      /// <returns><c>true</c> if the element is successfully removed; otherwise, <c>false</c>. This method also returns <c>false</c> if <paramref name="key" /> was not found in this dictionary.</returns>
      /// <param name="key">The key of the element to remove.</param>
      /// <exception cref="ArgumentNullException"><paramref name="key" /> is <c>null</c>.</exception>
      Boolean Remove( TKey key );
   }

   /// <summary>
   /// This is <c>query</c> role interface for dictionaries. It defines methods which access the dictionary without modifying it.
   /// </summary>
   /// <typeparam name="TKey">The type of the keys of the dictionary.</typeparam>
   /// <typeparam name="TValue">The type of the values of the dictionary.</typeparam>
   public interface DictionaryQuery<TKey, TValue> : CollectionQuery<KeyValuePair<TKey, TValue>>
   {
      /// <summary>
      /// Gets the element with the specified key.
      /// </summary>
      /// <param name="key">The key of the element to get.</param>
      /// <value>The element with the specified key.</value>
      /// <exception cref="ArgumentNullException"><paramref name="key" /> is <c>null</c>.</exception>
      /// <exception cref="System.Collections.Generic.KeyNotFoundException">No value found for <paramref name="key" />.</exception>
      TValue this[TKey key] { get; }

      /// <summary>
      /// Gets an <see cref="IEnumerable{T}"/> containing the keys of this dictionary.
      /// </summary>
      /// <value>An <see cref="IEnumerable{T}"/> containing the keys of this dictionary.</value>
      IEnumerable<TKey> Keys { get; }

      /// <summary>
      /// Gets an <see cref="IEnumerable{T}"/> containing the values of this dictionary.
      /// </summary>
      /// <value>An <see cref="IEnumerable{T}"/> containing the values of this dictionary.</value>
      IEnumerable<TValue> Values { get; }

      /// <summary>
      /// Determines whether this dictionary contains an element with the specified key.
      /// </summary>
      /// <returns><c>true</c> if this dictionary contains an element with the key; otherwise, <c>false</c>.</returns>
      /// <param name="key">The key to locate in this dictionary.</param>
      /// <exception cref="System.ArgumentNullException"><paramref name="key" /> is <c>null</c>.</exception>
      Boolean ContainsKey( TKey key );

      /// <summary>
      /// Gets the value associated with the specified key.
      /// </summary>
      /// <returns><c>true</c> if this dictionary contains an element with the specified key; otherwise, <c>false</c>.</returns>
      /// <param name="key">The key whose value to get.</param>
      /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the <typeparamref name="TValue"/>. This parameter may be passed uninitialized.</param>
      /// <exception cref="System.ArgumentNullException"><paramref name="key" /> is <c>null</c>.</exception>
      Boolean TryGetValue( TKey key, out TValue value );
   }

   /// <summary>
   /// This is a <c>command</c> role wrapper around <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/> that allows Command-Query Separation for normal dictionaries.
   /// </summary>
   /// <typeparam name="TKey">The type of the keys of the dictionary.</typeparam>
   /// <typeparam name="TValue">The type of the values of the dictionary.</typeparam>
   public interface DictionaryProxy<TKey, TValue> :
      DictionaryMutable<TKey, TValue, DictionaryQuery<TKey, TValue>>,
      Mutable<DictionaryProxyQuery<TKey, TValue>, DictionaryQuery<TKey, TValue>>
   {

   }

   /// <summary>
   /// This is a <c>query</c> role wrapper around <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/> that allows Command-Query Separation for normal dictionaries.
   /// </summary>
   /// <typeparam name="TKey">The type of the keys of the dictionary.</typeparam>
   /// <typeparam name="TValue">The type of the values of the dictionary.</typeparam>
   public interface DictionaryProxyQuery<TKey, TValue> :
      DictionaryQuery<TKey, TValue>,
      MutableQuery<DictionaryQuery<TKey, TValue>>
   {

   }

   /// <summary>
   /// This is <c>command</c> role for dictionary, which supports Command-Query Separation of its elements.
   /// </summary>
   /// <typeparam name="TKey">The type of the keys of the dictionary.</typeparam>
   /// <typeparam name="TValue">The type of <c>command</c> role of the values of this dictionary.</typeparam>
   /// <typeparam name="TValueQuery">The type of <c>query</c> role of the values of this dictionary.</typeparam>
   /// <typeparam name="TValueImmutable">The type of <c>immutable query</c> role of the values of this dictionary.</typeparam>
   public interface DictionaryWithRoles<TKey, TValue, TValueQuery, TValueImmutable> : Mutable<DictionaryQueryOfMutables<TKey, TValue, TValueQuery, TValueImmutable>, DictionaryQuery<TKey, TValueImmutable>>, DictionaryMutable<TKey, TValue, DictionaryQueryOfMutables<TKey, TValue, TValueQuery, TValueImmutable>>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
   }

   /// <summary>
   /// This is <c>query of mutables</c> role for dictionary, which supports Command-Query Separation of its elements.
   /// </summary>
   /// <typeparam name="TKey">The type of the keys of the dictionary.</typeparam>
   /// <typeparam name="TValue">The type of <c>command</c> role of the values of this dictionary.</typeparam>
   /// <typeparam name="TValueQuery">The type of <c>query</c> role of the values of this dictionary.</typeparam>
   /// <typeparam name="TValueImmutable">The type of <c>immutable query</c> role of the values of this dictionary.</typeparam>
   public interface DictionaryQueryOfMutables<TKey, TValue, TValueQuery, TValueImmutable> : MutableQuery<DictionaryQuery<TKey, TValueImmutable>>, DictionaryQuery<TKey, TValue>, QueriesProvider<DictionaryQueryOfQueries<TKey, TValueQuery, TValueImmutable>>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
   }

   /// <summary>
   /// This is <c>query of queries</c> role for dictionary, which supports Command-Query Separation of its elements.
   /// </summary>
   /// <typeparam name="TKey">The type of the keys of the dictionary.</typeparam>
   /// <typeparam name="TValueQuery">The type of <c>query</c> role of the values of this dictionary.</typeparam>
   /// <typeparam name="TValueImmutable">The type of <c>immutable query</c> role of the values of this dictionary.</typeparam>
   public interface DictionaryQueryOfQueries<TKey, TValueQuery, TValueImmutable> : MutableQuery<DictionaryQuery<TKey, TValueImmutable>>, DictionaryQuery<TKey, TValueQuery>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
   }
}

public static partial class E_CWR
{
   /// <summary>
   /// Creates a new <see cref="DictionaryProxy{TKey, TValue}"/> from given dictionary using default <see cref="CollectionsFactory"/>.
   /// </summary>
   /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
   /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
   /// <param name="dictionary">The <see cref="IDictionary{TKey, TValue}"/>.</param>
   /// <returns>A new <see cref="DictionaryProxy{TKey, TValue}"/> with elements from <paramref name="dictionary"/>.</returns>
   /// <seealso cref="ArrayProxy{TValue}"/>
   /// <seealso cref="CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY"/>
   public static DictionaryProxy<TKey, TValue> ToDictionaryProxy<TKey, TValue>( this IDictionary<TKey, TValue> dictionary )
   {
      return CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewDictionaryProxy( dictionary );
   }
}