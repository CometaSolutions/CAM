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
using CollectionsWithRoles.API;

namespace CollectionsWithRoles.Implementation
{
   internal class CollectionsFactoryImpl : CollectionsFactory
   {

      internal CollectionsFactoryImpl()
      {
      }

      #region CollectionsFactory Members

      public virtual ListWithRoles<TValue, TValueQuery, TValueImmutable> NewList<TValue, TValueQuery, TValueImmutable>( IList<TValue> listToUse = null )
         where TValue : Mutable<TValueQuery, TValueImmutable>
         where TValueQuery : MutableQuery<TValueImmutable>
      {
         var listIQ = new ListImmutableQueryImpl<TValue, TValueQuery, TValueImmutable>( listToUse ?? new List<TValue>() );
         return new ListWithRolesImpl<TValue, TValueQuery, TValueImmutable>( new ListQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable>( listIQ, new ListQueryOfQueriesImpl<TValue, TValueQuery, TValueImmutable>( listIQ ) ) );
      }

      public virtual DictionaryWithRoles<TKey, TValue, TValueQuery, TValueImmutable> NewDictionary<TKey, TValue, TValueQuery, TValueImmutable>( IDictionary<TKey, TValue> dictionary = null )
         where TValue : Mutable<TValueQuery, TValueImmutable>
         where TValueQuery : API.MutableQuery<TValueImmutable>
      {
         var dicIQ = new DictionaryImmutableQueryImpl<TKey, TValue, TValueQuery, TValueImmutable>( dictionary ?? new Dictionary<TKey, TValue>() );
         return new DictionaryWithRolesImpl<TKey, TValue, TValueQuery, TValueImmutable>( new DictionaryQueryOfMutablesImpl<TKey, TValue, TValueQuery, TValueImmutable>( dicIQ, new DictionaryQueryOfQueriesImpl<TKey, TValue, TValueQuery, TValueImmutable>( dicIQ ) ) );
      }

      public virtual SetWithRoles<TValue, TValueQuery, TValueImmutable> NewMemoryEfficientSet<TValue, TValueQuery, TValueImmutable>( ISet<TValue> setToUse = null )
         where TValue : Mutable<TValueQuery, TValueImmutable>
         where TValueQuery : MutableQuery<TValueImmutable>
      {
         var setIQ = new SetImmutableQueryImpl<TValue, TValueQuery, TValueImmutable>( setToUse ?? new HashSet<TValue>() );
         return new SetWithRolesImpl<TValue, TValueQuery, TValueImmutable>( new SetQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable>( setIQ, new SetQueryOfQueriesImpl<TValue, TValueQuery, TValueImmutable>( setIQ ) ) );
      }

      public virtual SetWithRoles<TValue, TValueQuery, TValueImmutable> NewFastSet<TValue, TValueQuery, TValueImmutable>( ISet<TValue> setToUse = null, ISet<TValueQuery> queriesSet = null, ISet<TValueImmutable> immutablesSet = null )
         where TValue : Mutable<TValueQuery, TValueImmutable>
         where TValueQuery : MutableQuery<TValueImmutable>
      {
         if ( setToUse == null )
         {
            if ( ( queriesSet != null && queriesSet.Count > 0 )
                || ( immutablesSet != null && immutablesSet.Count > 0 ) )
            {
               throw new ArgumentException( "The set of mutable typed objects was null, however the set of either query or immutable query contained at least one element." );
            }
         }
         else
         {
            var count = setToUse.Count;
            if ( ( count > 0 && queriesSet == null )
               || ( queriesSet != null && queriesSet.Count != count )
               || ( count > 0 && immutablesSet == null )
               || ( immutablesSet != null && immutablesSet.Count != count ) )
            {
               throw new ArgumentException( "The set of mutable type objects was not null, however there was a count mismatch in either query or immutable query sets." );
            }
         }
         var state = new FastSetState<TValue, TValueQuery, TValueImmutable>( queriesSet ?? new HashSet<TValueQuery>(), immutablesSet ?? new HashSet<TValueImmutable>() );
         var setIQ = new FastSetImmutableQueryImpl<TValue, TValueQuery, TValueImmutable>( setToUse ?? new HashSet<TValue>(), state );
         return new SetWithRolesImpl<TValue, TValueQuery, TValueImmutable>( new SetQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable>( setIQ, new FastSetQueryOfQueriesImpl<TValue, TValueQuery, TValueImmutable>( setIQ, state ) ) );
      }

      public virtual ArrayWithRoles<TValue, TValueQuery, TValueImmutable> NewArrayWithRoles<TValue, TValueQuery, TValueImmutable>( TValue[] array = null )
         where TValue : Mutable<TValueQuery, TValueImmutable>
         where TValueQuery : MutableQuery<TValueImmutable>
      {
         ArrayWithRoles<TValue, TValueQuery, TValueImmutable> retVal;
         if ( !array.IsNullOrEmpty() )
         {
            var arrayIQ = new ArrayImmutableQueryImpl<TValue, TValueQuery, TValueImmutable>( array );
            retVal = new ArrayWithRolesImpl<TValue, TValueQuery, TValueImmutable>( new ArrayQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable>( arrayIQ, new ArrayQueryOfQueriesImpl<TValue, TValueQuery, TValueImmutable>( arrayIQ ) ) );
         }
         else
         {
            retVal = EmptyArrayWithRoles<TValue, TValueQuery, TValueImmutable>.Array;
         }
         return retVal;
      }

      public virtual ListProxy<TValue> NewListProxy<TValue>( IList<TValue> list = null )
      {
         return new ListProxyImpl<TValue>( new ListProxyQueryImpl<TValue>( list ?? new List<TValue>() ) );
      }

      public virtual DictionaryProxy<TKey, TValue> NewDictionaryProxy<TKey, TValue>( IDictionary<TKey, TValue> dictionary = null )
      {
         return new DictionaryProxyImpl<TKey, TValue>( new DictionaryProxyQueryImpl<TKey, TValue>( dictionary ?? new Dictionary<TKey, TValue>() ) );
      }

      public virtual SetProxy<TValue> NewSetProxy<TValue>( ISet<TValue> set = null )
      {
         return new SetProxyImpl<TValue>( new SetProxyQueryImpl<TValue>( set ?? new HashSet<TValue>() ) );
      }

      public virtual ArrayProxy<TValue> NewArrayProxy<TValue>( TValue[] array = null )
      {
         return array.IsNullOrEmpty() ?
            EmptyArrayProxy<TValue>.Proxy :
            new ArrayProxyImpl<TValue>( new ArrayProxyQueryImpl<TValue>( array ) );
      }
      #endregion
   }

   /// <summary>
   /// Class that exposes the <see cref="CollectionsFactory"/> singleton.
   /// </summary>
   public static class CollectionsFactorySingleton
   {
      /// <summary>
      /// Provides access to singleton <see cref="CollectionsFactory"/> implementation.
      /// </summary>
      public static readonly CollectionsFactory DEFAULT_COLLECTIONS_FACTORY = new CollectionsFactoryImpl();
   }
}
