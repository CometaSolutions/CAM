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
         listToUse = listToUse ?? new List<TValue>();
         var state = new ListState<TValue>( listToUse );
         var listIQ = new ListImmutableQueryImpl<TValue, TValueQuery, TValueImmutable>( state );
         return new ListWithRolesImpl<TValue, TValueQuery, TValueImmutable>( new ListQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable>( listIQ, new ListQueryOfQueriesImpl<TValue, TValueQuery, TValueImmutable>( listIQ, state ), state ), state );
      }

      public virtual DictionaryWithRoles<TKey, TValue, TValueQuery, TValueImmutable> NewDictionary<TKey, TValue, TValueQuery, TValueImmutable>( IDictionary<TKey, TValue> dictionary = null )
         where TValue : Mutable<TValueQuery, TValueImmutable>
         where TValueQuery : API.MutableQuery<TValueImmutable>
      {
         dictionary = dictionary ?? new Dictionary<TKey, TValue>();
         var state = new DictionaryWithRolesState<TKey, TValue, TValueQuery, TValueImmutable>( dictionary );
         var dicIQ = new DictionaryImmutableQueryImpl<TKey, TValue, TValueQuery, TValueImmutable>( state );
         return new DictionaryWithRolesImpl<TKey, TValue, TValueQuery, TValueImmutable>( new DictionaryQueryOfMutablesImpl<TKey, TValue, TValueQuery, TValueImmutable>( dicIQ, new DictionaryQueryOfQueriesImpl<TKey, TValue, TValueQuery, TValueImmutable>( dicIQ, state ), state ), state );
      }

      public virtual SetWithRoles<TValue, TValueQuery, TValueImmutable> NewMemoryEfficientSet<TValue, TValueQuery, TValueImmutable>( ISet<TValue> setToUse = null )
         where TValue : Mutable<TValueQuery, TValueImmutable>
         where TValueQuery : MutableQuery<TValueImmutable>
      {
         setToUse = setToUse ?? new HashSet<TValue>();
         var state = new SetState<TValue>( setToUse );
         var setIQ = new SetImmutableQueryImpl<TValue, TValueQuery, TValueImmutable>( state );
         return new SetWithRolesImpl<TValue, TValueQuery, TValueImmutable>( new SetQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable>( setIQ, new SetQueryOfQueriesImpl<TValue, TValueQuery, TValueImmutable>( setIQ, state ), state ), state );
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
         setToUse = setToUse ?? new HashSet<TValue>();
         queriesSet = queriesSet ?? new HashSet<TValueQuery>();
         immutablesSet = immutablesSet ?? new HashSet<TValueImmutable>();
         var state = new FastSetState<TValue, TValueQuery, TValueImmutable>( setToUse, queriesSet, immutablesSet );
         var setIQ = new FastSetImmutableQueryImpl<TValue, TValueQuery, TValueImmutable>( state );
         return new SetWithRolesImpl<TValue, TValueQuery, TValueImmutable>( new SetQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable>( setIQ, new FastSetQueryOfQueriesImpl<TValue, TValueQuery, TValueImmutable>( setIQ, state ), state ), state );
      }

      public virtual ArrayWithRoles<TValue, TValueQuery, TValueImmutable> NewArrayWithRoles<TValue, TValueQuery, TValueImmutable>( TValue[] array = null )
         where TValue : Mutable<TValueQuery, TValueImmutable>
         where TValueQuery : MutableQuery<TValueImmutable>
      {
         ArrayWithRoles<TValue, TValueQuery, TValueImmutable> retVal;
         if ( array != null && array.Length > 0 )
         {
            var state = new ArrayState<TValue>( array );
            var arrayIQ = new ArrayImmutableQueryImpl<TValue, TValueQuery, TValueImmutable>( state );
            retVal = new ArrayWithRolesImpl<TValue, TValueQuery, TValueImmutable>( new ArrayQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable>( arrayIQ, new ArrayQueryOfQueriesImpl<TValue, TValueQuery, TValueImmutable>( arrayIQ, state ), state ), state );
         }
         else
         {
            retVal = EmptyArrayWithRoles<TValue, TValueQuery, TValueImmutable>.Array;
         }
         return retVal;
      }

      public virtual ListProxy<TValue> NewListProxy<TValue>( IList<TValue> list = null )
      {
         list = list ?? new List<TValue>();
         var state = new ListState<TValue>( list );
         return new ListProxyImpl<TValue>( new ListProxyQueryImpl<TValue>( state ), state );
      }

      public virtual DictionaryProxy<TKey, TValue> NewDictionaryProxy<TKey, TValue>( IDictionary<TKey, TValue> dictionary = null )
      {
         dictionary = dictionary ?? new Dictionary<TKey, TValue>();
         var state = new DictionaryProxyState<TKey, TValue>( dictionary );
         return new DictionaryProxyImpl<TKey, TValue>( new DictionaryProxyQueryImpl<TKey, TValue>( state ), state );
      }

      public virtual SetProxy<TValue> NewSetProxy<TValue>( ISet<TValue> set = null )
      {
         set = set ?? new HashSet<TValue>();
         var state = new SetState<TValue>( set );
         return new SetProxyImpl<TValue>( new SetProxyQueryImpl<TValue>( state ), state );
      }

      public virtual ArrayProxy<TValue> NewArrayProxy<TValue>( TValue[] array = null )
      {
         ArrayProxy<TValue> retVal;
         if ( array != null && array.Length > 0 )
         {
            var state = new ArrayState<TValue>( array );
            retVal = new ArrayProxyImpl<TValue>( new ArrayProxyQueryImpl<TValue>( state ), state );
         }
         else
         {
            retVal = EmptyArrayProxy<TValue>.Proxy;
         }
         return retVal;
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
      public static CollectionsFactory DEFAULT_COLLECTIONS_FACTORY = new CollectionsFactoryImpl();
   }
}
