using CollectionsWithRoles.Implementation;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CollectionsWithRoles.API
{
   /// <summary>
   /// This class contains static properties to access query roles of empty lists and sets.
   /// </summary>
   /// <typeparam name="T">The type of the elements for the collection.</typeparam>
   public static class EmptyCollectionQuery<T>
   {
      private static readonly ListQuery<T> LIST = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy( new List<T>() ).CQ;
      private static readonly SetQuery<T> SET = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewSetProxy( new HashSet<T>() ).CQ;

      /// <summary>
      /// Returns the empty <see cref="ListQuery{T}"/> instance.
      /// </summary>
      /// <value>The empty <see cref="ListQuery{T}"/> instance.</value>
      public static ListQuery<T> List
      {
         get
         {
            return LIST;
         }
      }

      /// <summary>
      /// Returns the empty <see cref="SetQuery{T}"/> instance.
      /// </summary>
      /// <value>The empty <see cref="SetQuery{T}"/> instance.</value>
      public static SetQuery<T> Set
      {
         get
         {
            return SET;
         }
      }

   }

   /// <summary>
   /// This class contains static properties to access empty <see cref="ArrayProxy{T}"/> and <see cref="ArrayQuery{T}"/>.
   /// </summary>
   /// <typeparam name="TValue">The type of the elements for the array.</typeparam>
   public static class EmptyArrayProxy<TValue>
   {
      private static readonly ArrayProxy<TValue> ARRAY;

      static EmptyArrayProxy()
      {
         // Can not cache this to CollectionsFactoryImpl since it is not generic class.
         ARRAY = new ArrayProxyImpl<TValue>( new ArrayProxyQueryImpl<TValue>( new TValue[0] ) );
      }

      /// <summary>
      /// Returns the empty <see cref="ArrayProxy{T}"/> instance.
      /// </summary>
      /// <value>The empty <see cref="ArrayProxy{T}"/> instance.</value>
      public static ArrayProxy<TValue> Proxy
      {
         get
         {
            return ARRAY;
         }
      }

      /// <summary>
      /// Returns the empty <see cref="ArrayQuery{T}"/> instance.
      /// </summary>
      /// <value>The empty <see cref="ArrayQuery{T}"/> instance.</value>
      public static ArrayQuery<TValue> Query
      {
         get
         {
            return ARRAY.CQ;
         }
      }
   }

   /// <summary>
   /// This class contains static properties to access empty <see cref="ArrayWithRoles{T,U,V}"/>, <see cref="ArrayQueryOfMutables{T,U,V}"/> and <see cref="ArrayQueryOfQueries{T,U}"/>.
   /// </summary>
   /// <typeparam name="TValue">The mutable type of the array elements.</typeparam>
   /// <typeparam name="TValueQuery">The query type of the array elements.</typeparam>
   /// <typeparam name="TValueImmutable">The immutable query type of the array elements.</typeparam>
   public static class EmptyArrayWithRoles<TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private static readonly ArrayWithRoles<TValue, TValueQuery, TValueImmutable> ARRAY;

      static EmptyArrayWithRoles()
      {
         var arrayIQ = new ArrayImmutableQueryImpl<TValue, TValueQuery, TValueImmutable>( new TValue[0] );
         ARRAY = new ArrayWithRolesImpl<TValue, TValueQuery, TValueImmutable>( new ArrayQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable>( arrayIQ, new ArrayQueryOfQueriesImpl<TValue, TValueQuery, TValueImmutable>( arrayIQ ) ) );
      }

      /// <summary>
      /// Returns the empty <see cref="ArrayWithRoles{T,U,V}"/> instance.
      /// </summary>
      /// <value>The empty <see cref="ArrayWithRoles{T,U,V}"/> instance.</value>
      public static ArrayWithRoles<TValue, TValueQuery, TValueImmutable> Array
      {
         get
         {
            return ARRAY;
         }
      }

      /// <summary>
      /// Returns the empty <see cref="ArrayQueryOfMutables{T,U,V}"/> instance.
      /// </summary>
      /// <value>The empty <see cref="ArrayQueryOfMutables{T,U,V}"/> instance.</value>
      public static ArrayQueryOfMutables<TValue, TValueQuery, TValueImmutable> QueryOfMutable
      {
         get
         {
            return ARRAY.CQ;
         }
      }

      /// <summary>
      /// Returns the empty <see cref="ArrayQueryOfQueries{T,U}"/> instance.
      /// </summary>
      /// <value>The empty <see cref="ArrayQueryOfQueries{T,U}"/> instance.</value>
      public static ArrayQueryOfQueries<TValueQuery, TValueImmutable> QueryOfQueries
      {
         get
         {
            return ARRAY.MQ.Queries;
         }
      }
   }

   /// <summary>
   /// This class contains static property to access query role of empty dictionary.
   /// </summary>
   /// <typeparam name="TKey">The type of the key for the dictionary.</typeparam>
   /// <typeparam name="TValue">The type of the value for the dictionary.</typeparam>
   public static class EmptyDictionaryQuery<TKey, TValue>
   {
      private static readonly DictionaryQuery<TKey, TValue> INSTANCE = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewDictionaryProxy( new Dictionary<TKey, TValue>() ).CQ;

      /// <summary>
      /// Returns the empty <see cref="DictionaryQuery{TKey, TValue}"/>.
      /// </summary>
      /// <value>The empty <see cref="DictionaryQuery{TKey, TValue}"/>.</value>
      public static DictionaryQuery<TKey, TValue> Dictionary
      {
         get
         {
            return INSTANCE;
         }
      }
   }
}
