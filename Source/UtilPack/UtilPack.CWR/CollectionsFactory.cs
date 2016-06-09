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
using UtilPack.CollectionsWithRoles;
using UtilPack;

namespace UtilPack.CollectionsWithRoles
{
   /// <summary>
   /// This is factory interface to create all collections within this assembly.
   /// </summary>
   /// <remarks>TODO: make it possible to pass equality comparers for TValueQuery/TValueImmutable for sets.</remarks>
   public interface CollectionsFactory
   {
      /// <summary>
      /// Creates a new <see cref="ListWithRoles{TValue, TValueQuery, TValueImmutable}"/> with given list. If <paramref name="listToUse"/> is <c>null</c>, then a new instance of <see cref="List{TValue}"/> is used. Otherwise, the returned list has the same elements as the <paramref name="listToUse"/>. If the changes are made to <paramref name="listToUse"/>, the contents of returned list change accordingly, and vice versa.
      /// </summary>
      /// <typeparam name="TValue">The mutable type of the list elements.</typeparam>
      /// <typeparam name="TValueQuery">The query type of the list elements.</typeparam>
      /// <typeparam name="TValueImmutable">The immutable query type of the list elements.</typeparam>
      /// <param name="listToUse">The list to use as basis for the returned list. May be <c>null</c>.</param>
      /// <returns>A new <see cref="ListWithRoles{TValue, TValueQuery, TValueImmutable}"/> object.</returns>
      ListWithRoles<TValue, TValueQuery, TValueImmutable> NewList<TValue, TValueQuery, TValueImmutable>( IList<TValue> listToUse = null )
         where TValue : Mutable<TValueQuery, TValueImmutable>
         where TValueQuery : MutableQuery<TValueImmutable>;

      /// <summary>
      /// Creates a new <see cref="DictionaryWithRoles{TKey, TValue, TValueQuery, TValueImmutable}"/> with given dictionary. If <paramref name="dictionaryToUse"/> is <c>null</c>, then a new instance of <see cref="Dictionary{TKey, TValue}"/> is used. Otherwise, the returned dictionary has the same elements as the <paramref name="dictionaryToUse"/>. If the changes are made to <paramref name="dictionaryToUse"/>, the contents of returned dictionary change accordingly, and vice versa.
      /// </summary>
      /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
      /// <typeparam name="TValue">The mutable type of the dictionary values.</typeparam>
      /// <typeparam name="TValueQuery">The query type of the dictionary values.</typeparam>
      /// <typeparam name="TValueImmutable">The immutable query type of the dictionary values.</typeparam>
      /// <param name="dictionaryToUse">The dictionary to use as basis for the returned dictionary. May be <c>null</c>.</param>
      /// <returns>A new <see cref="DictionaryWithRoles{TKey, TValue, TValueQuery, TValueImmutable}"/> object.</returns>
      DictionaryWithRoles<TKey, TValue, TValueQuery, TValueImmutable> NewDictionary<TKey, TValue, TValueQuery, TValueImmutable>( IDictionary<TKey, TValue> dictionaryToUse = null )
         where TValue : Mutable<TValueQuery, TValueImmutable>
         where TValueQuery : MutableQuery<TValueImmutable>;

      /// <summary>
      /// Creates a new <see cref="SetWithRoles{TValue, TValueQuery, TValueImmutable}"/> with given set. The returned set is memory-efficient, however it may be somewhat slower in certain operations. If <paramref name="setToUse"/> is <c>null</c>, then a new instance of <see cref="HashSet{TValue}"/> is used. Otherwise, the returned set has the same elements as the <paramref name="setToUse"/>. If the changes are made to <paramref name="setToUse"/>, the contents of returned set change accordingly, and vice versa.
      /// </summary>
      /// <typeparam name="TValue">The mutable type of the set elements.</typeparam>
      /// <typeparam name="TValueQuery">The query type of the set elements.</typeparam>
      /// <typeparam name="TValueImmutable">The immutable query type of the set elements.</typeparam>
      /// <param name="setToUse">The set to use as basis for the returned set. May be <c>null</c>.</param>
      /// <returns>A new <see cref="SetWithRoles{TValue, TValueQuery, TValueImmutable}"/> object.</returns>
      SetWithRoles<TValue, TValueQuery, TValueImmutable> NewMemoryEfficientSet<TValue, TValueQuery, TValueImmutable>( ISet<TValue> setToUse = null )
         where TValue : Mutable<TValueQuery, TValueImmutable>
         where TValueQuery : MutableQuery<TValueImmutable>;

      /// <summary>
      /// Creates a new <see cref="SetWithRoles{TValue, TValueQuery, TValueImmutable}"/> with given set. The returned set is as quick as <see cref="HashSet{TValue}"/>, however it maintains three different sets and is thus memory-inefficient. If <paramref name="setToUse"/> is <c>null</c>, then a new instance of <see cref="HashSet{TValue}"/> is used. Otherwise, the returned set has the same elements as the <paramref name="setToUse"/>.  If the changes are made to <paramref name="setToUse"/>, the contents of returned set change accordingly, and vice versa. The same applies to <paramref name="queriesSet"/> and <paramref name="immutablesSet"/>.
      /// </summary>
      /// <typeparam name="TValue">The mutable type of the set elements.</typeparam>
      /// <typeparam name="TValueQuery">The query type of the set elements.</typeparam>
      /// <typeparam name="TValueImmutable">The immutable query type of the set elements.</typeparam>
      /// <param name="setToUse">The set to use as basis for the returned set. May be <c>null</c>.</param>
      /// <param name="queriesSet">The set of query-typed objects to use as basis for the returned set. May be <c>null</c>.</param>
      /// <param name="immutablesSet">The set of immutable query-typed objects to use as basis for the returned set. May be <c>null</c>.</param>
      /// <returns>A new <see cref="SetWithRoles{TValue, TValueQuery, TValueImmutable}"/> object.</returns>
      /// <exception cref="ArgumentException">If there is count mismatch between given sets.</exception>
      SetWithRoles<TValue, TValueQuery, TValueImmutable> NewFastSet<TValue, TValueQuery, TValueImmutable>( ISet<TValue> setToUse = null, ISet<TValueQuery> queriesSet = null, ISet<TValueImmutable> immutablesSet = null )
         where TValue : Mutable<TValueQuery, TValueImmutable>
         where TValueQuery : MutableQuery<TValueImmutable>;

      /// <summary>
      /// Creates a new <see cref="ArrayWithRoles{TValue, TValueQuery, TValueImmutable}"/> with given array. If <paramref name="array"/> is <c>null</c>, then empty array is used. Otherwise returned array has the same elements as given <paramref name="array"/>. If any changes are made to the <paramref name="array"/>, those contents of returned array change accordingly, and vice versa.
      /// </summary>
      /// <typeparam name="TValue">The mutable type of the array elements.</typeparam>
      /// <typeparam name="TValueQuery">The query type of the array elements.</typeparam>
      /// <typeparam name="TValueImmutable">The immutable query type of the array elements.</typeparam>
      /// <param name="array">The array to use as basis for the returned array. May be <c>null</c>.</param>
      /// <returns>A new <see cref="ArrayWithRoles{TValue, TValueQuery, TValueImmutable}"/> object, or <see cref="EmptyArrayWithRoles{TValue, TValueQuery, TValueImmutable}.Array"/> if <paramref name="array"/> is <c>null</c> or empty.</returns>
      ArrayWithRoles<TValue, TValueQuery, TValueImmutable> NewArrayWithRoles<TValue, TValueQuery, TValueImmutable>( TValue[] array = null )
         where TValue : Mutable<TValueQuery, TValueImmutable>
         where TValueQuery : MutableQuery<TValueImmutable>;

      /// <summary>
      /// Creates a new <see cref="ListProxy{TValue}"/> with given list. If <paramref name="list"/> is <c>null</c>, a new empty <see cref="List{TValue}"/> is used. Otherwise, the returned list proxy has same elements as <paramref name="list"/>.
      /// </summary>
      /// <typeparam name="TValue">The type of list elements.</typeparam>
      /// <param name="list">The list to use as basis for the returned list. May be <c>null</c>.</param>
      /// <returns>A new <see cref="ListProxy{TValue}"/> object.</returns>
      ListProxy<TValue> NewListProxy<TValue>( IList<TValue> list = null );

      /// <summary>
      /// Creates a new <see cref="DictionaryProxy{TKey, TValue}"/> with given dictionary. If <paramref name="dictionary"/> is <c>null</c>, a new empty <see cref="Dictionary{TKey, TValue}"/> is used. Otherwise, the returned dictionary proxy has same content as <paramref name="dictionary"/>. If the changes are made to <paramref name="dictionary"/>, the contents of returned dictionary change accordingly, and vice versa.
      /// </summary>
      /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
      /// <typeparam name="TValue">The type of dictionary values.</typeparam>
      /// <param name="dictionary">The dictionary to use as basis for the returned dictionary. May be <c>null</c>.</param>
      /// <returns>A new <see cref="DictionaryProxy{TKey, TValue}"/> object.</returns>
      DictionaryProxy<TKey, TValue> NewDictionaryProxy<TKey, TValue>( IDictionary<TKey, TValue> dictionary = null );

      /// <summary>
      /// Creates a new <see cref="SetProxy{TValue}"/> with given set. If <paramref name="set"/> is <c>null</c>, a new empty <see cref="HashSet{TValue}"/> is used. Otherwise, the returned set proxy has same content as <paramref name="set"/>.  If the changes are made to <paramref name="set"/>, the contents of returned set change accordingly, and vice versa.
      /// </summary>
      /// <typeparam name="TValue">The type of set elements.</typeparam>
      /// <param name="set">The set to use as basis for the returned set. May be <c>null</c>.</param>
      /// <returns>A new <see cref="SetProxy{TValue}"/> object.</returns>
      SetProxy<TValue> NewSetProxy<TValue>( ISet<TValue> set = null );

      /// <summary>
      /// Creates a new <see cref="ArrayProxy{TValue}"/> with given array. If <paramref name="array"/> is <c>null</c>, an empty array is used. Otherwise, the returned array proxy has same elements as <paramref name="array"/>.
      /// </summary>
      /// <typeparam name="TValue">The type of array elements.</typeparam>
      /// <param name="array">The array to use as basis for the returned set. May be <c>null</c>.</param>
      /// <returns>A new <see cref="ArrayProxy{TValue}"/> object, or <see cref="EmptyArrayProxy{TValue}.Proxy"/> if <paramref name="array"/> is <c>null</c> or empty.</returns>
      ArrayProxy<TValue> NewArrayProxy<TValue>( TValue[] array = null );
   }

   /// <summary>
   /// Class that exposes the <see cref="CollectionsFactory"/> singleton.
   /// </summary>
   public static class CollectionsFactorySingleton
   {
      /// <summary>
      /// Provides access to singleton <see cref="CollectionsFactory"/> implementation.
      /// </summary>
      public static readonly CollectionsFactory DEFAULT_COLLECTIONS_FACTORY = new UtilPack.CollectionsWithRoles.Implementation.CollectionsFactoryImpl();
   }
}

/// <summary>
/// This is class to hold extension methods related to CollectionsWithRoles.API namespace.
/// </summary>
public static partial class E_UtilPack
{
   /// <summary>
   /// Creates a new <see cref="ListProxy{TValue}"/> directly from method parameters. The underlying list will be <see cref="System.Collections.Generic.List{TValue}"/>.
   /// </summary>
   /// <typeparam name="TValue">The type of list elements.</typeparam>
   /// <param name="factory">The <see cref="CollectionsFactory"/>.</param>
   /// <param name="values">The values for the returned list proxy to contain.</param>
   /// <returns>A new <see cref="ListProxy{TValue}"/> with given values.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="factory"/> is <c>null</c>.</exception>
   public static ListProxy<TValue> NewListProxyFromParams<TValue>( this CollectionsFactory factory, params TValue[] values )
   {
      return factory.NewListProxy<TValue>( new List<TValue>( values ?? Empty<TValue>.Array ) );
   }

   /// <summary>
   /// Creates a new <see cref="SetProxy{TValue}"/> directly from method parameters. The underlying set will be <see cref="System.Collections.Generic.HashSet{TValue}"/>.
   /// </summary>
   /// <typeparam name="TValue">The type of set elements.</typeparam>
   /// <param name="factory">The <see cref="CollectionsFactory"/>.</param>
   /// <param name="values">The values for the returned set proxy to contain.</param>
   /// <returns>A new <see cref="SetProxy{TValue}"/> with given values.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="factory"/> is <c>null</c>.</exception>
   public static SetProxy<TValue> NewSetProxyFromParams<TValue>( this CollectionsFactory factory, params TValue[] values )
   {
      return factory.NewSetProxy<TValue>( new HashSet<TValue>( values ?? Empty<TValue>.Array ) );
   }

   /// <summary>
   /// Creates a new <see cref="ArrayProxy{TValue}"/> directly from method parameters.
   /// </summary>
   /// <typeparam name="TValue">The type of array elements.</typeparam>
   /// <param name="factory">The <see cref="CollectionsFactory"/>.</param>
   /// <param name="values">The values for the returned array proxy to contain.</param>
   /// <returns>A new <see cref="ArrayProxy{TValue}"/> with given values.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="factory"/> is <c>null</c>.</exception>
   public static ArrayProxy<TValue> NewArrayProxyFromParams<TValue>( this CollectionsFactory factory, params TValue[] values )
   {
      return factory.NewArrayProxy( values );
   }
}