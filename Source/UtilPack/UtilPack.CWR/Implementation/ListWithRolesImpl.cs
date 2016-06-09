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
   internal class ListWithRolesImpl<TValue, TValueQuery, TValueImmutable> : CollectionWithRolesImpl<IList<TValue>, ListQueryOfMutables<TValue, TValueQuery, TValueImmutable>, ListQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable>, ListQueryOfQueries<TValueQuery, TValueImmutable>, ListQuery<TValueImmutable>, TValue, TValueQuery, TValueImmutable>, ListWithRoles<TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      internal ListWithRolesImpl( ListQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable> mutableQuery )
         : base( mutableQuery )
      {
      }

      #region ListWithRoles<TMutableQuery,TImmutableQuery> Members

      public TValue this[int index]
      {
         set
         {
            this.CQImpl.Collection[index] = value;
         }
      }

      public void Insert( int index, TValue item )
      {
         this.CQImpl.Collection.Insert( index, item );
      }

      public void RemoveAt( int index )
      {
         this.CQImpl.Collection.RemoveAt( index );
      }

      #endregion
   }

   internal class ListQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable> : CollectionQueryOfMutablesImpl<IList<TValue>, ListQueryOfQueries<TValueQuery, TValueImmutable>, ListQuery<TValueImmutable>, ListImmutableQueryImpl<TValue, TValueQuery, TValueImmutable>, TValue, TValueQuery, TValueImmutable>, ListQueryOfMutables<TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {

      internal ListQueryOfMutablesImpl( ListImmutableQueryImpl<TValue, TValueQuery, TValueImmutable> immutableQuery, ListQueryOfQueries<TValueQuery, TValueImmutable> cmq )
         : base( immutableQuery, cmq )
      {
      }

      #region ListMutableQuery<TMutableQuery,TImmutableQuery> Members

      public TValue this[int index]
      {
         get
         {
            return this.Collection[index];
         }
      }

      public int IndexOf( TValue item )
      {
         return this.Collection.IndexOf( item );
      }

      #endregion

      public override Boolean Contains( TValue item, IEqualityComparer<TValue> equalityComparer = null )
      {
         equalityComparer = equalityComparer ?? EqualityComparer<TValue>.Default;
         for ( var i = 0; i < this.Collection.Count; ++i )
         {
            if ( equalityComparer.Equals( this.Collection[i], item ) )
            {
               return true;
            }
         }
         return false;
      }
   }

   internal class ListQueryOfQueriesImpl<TValue, TValueQuery, TValueImmutable> : CollectionQueryOfQueriesImpl<IList<TValue>, ListQuery<TValueImmutable>, ListImmutableQueryImpl<TValue, TValueQuery, TValueImmutable>, TValue, TValueQuery, TValueImmutable>, ListQueryOfQueries<TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {

      internal ListQueryOfQueriesImpl( ListImmutableQueryImpl<TValue, TValueQuery, TValueImmutable> immutableQuery )
         : base( immutableQuery )
      {
      }

      #region ListMutableQueryOfQueries<TValueQuery,TValueImmutable> Members

      public TValueQuery this[int index]
      {
         get
         {
            TValue value = this._collection[index];
            TValueQuery result;
            if ( value == null )
            {
               result = default( TValueQuery );
            }
            else
            {
               result = value.MQ;
            }
            return result;
         }
      }

      public int IndexOf( TValueQuery item )
      {
         Int32 result = -1;
         Int32 idx = 0;
         while ( result == -1 && idx < this._collection.Count )
         {
            TValue value = this._collection[idx];
            if ( ( value == null && item == null ) || ( value != null && Object.Equals( value.MQ, item ) ) )
            {
               result = idx;
            }
            ++idx;
         }

         return result;
      }

      #endregion
   }

   internal class ListImmutableQueryImpl<TValue, TMutableQuery, TImmutableQuery> : CollectionImmutableQueryImpl<IList<TValue>, TValue, TMutableQuery, TImmutableQuery>, ListQuery<TImmutableQuery>
      where TValue : Mutable<TMutableQuery, TImmutableQuery>
      where TMutableQuery : MutableQuery<TImmutableQuery>
   {

      internal ListImmutableQueryImpl( IList<TValue> list )
         : base( list )
      {
      }

      #region ListImmutableQuery<TImmutableQuery> Members

      public TImmutableQuery this[int index]
      {
         get
         {
            TValue value = this.Collection[index];
            TImmutableQuery result;
            if ( value == null )
            {
               result = default( TImmutableQuery );
            }
            else
            {
               result = value.MQ.IQ;
            }
            return result;
         }
      }

      public int IndexOf( TImmutableQuery item )
      {
         Int32 result = -1;
         Int32 idx = 0;
         while ( result == -1 && idx < this.Collection.Count )
         {
            TValue value = this.Collection[idx];
            if ( ( value == null && item == null ) || ( value != null && value.MQ != null && Object.Equals( value.MQ.IQ, item ) ) )
            {
               result = idx;
            }
            ++idx;
         }

         return result;
      }

      #endregion

      public override Boolean Contains( TImmutableQuery item, IEqualityComparer<TImmutableQuery> equalityComparer = null )
      {
         equalityComparer = equalityComparer ?? EqualityComparer<TImmutableQuery>.Default;
         for ( var i = 0; i < this.Collection.Count; ++i )
         {
            var cur = this.Collection[i];
            if ( ( cur == null || item == null ) || ( cur != null && equalityComparer.Equals( cur.MQ.IQ, item ) ) )
            {
               return true;
            }
         }
         return false;
      }
   }

   internal class ListQueryImpl<TValueImmutable> : CollectionQueryImpl<IList<TValueImmutable>, TValueImmutable>, ListQuery<TValueImmutable>
   {

      internal ListQueryImpl( IList<TValueImmutable> list )
         : base( list )
      {
      }

      #region ListImmutableQuery<TImmutableQuery> Members

      public TValueImmutable this[int index]
      {
         get
         {
            return this.Collection[index];
         }
      }

      public int IndexOf( TValueImmutable item )
      {
         return this.Collection.IndexOf( item );
      }

      #endregion


      public override Boolean Contains( TValueImmutable item, IEqualityComparer<TValueImmutable> equalityComparer = null )
      {
         equalityComparer = equalityComparer ?? EqualityComparer<TValueImmutable>.Default;
         for ( var i = 0; i < this.Collection.Count; ++i )
         {
            if ( equalityComparer.Equals( this.Collection[i], item ) )
            {
               return true;
            }
         }
         return false;
      }
   }

   internal class ListProxyImpl<TValue> : CollectionMutableImpl<IList<TValue>, TValue, ListQuery<TValue>, ListProxyQueryImpl<TValue>>, ListProxy<TValue>
   {
      internal ListProxyImpl( ListProxyQueryImpl<TValue> cq )
         : base( cq )
      {
      }

      #region ListMutable<TValue> Members

      public TValue this[int index]
      {
         set
         {
            this.CQImpl.Collection[index] = value;
         }
      }

      public void Insert( int index, TValue item )
      {
         this.CQImpl.Collection.Insert( index, item );
      }

      public void RemoveAt( int index )
      {
         this.CQImpl.Collection.RemoveAt( index );
      }

      #endregion

      #region Mutable<ListProxyQuery<TValue>,ListQuery<TValue>> Members

      public ListProxyQuery<TValue> MQ
      {
         get
         {
            return (ListProxyQuery<TValue>) this.CQ;
         }
      }

      #endregion
   }

   internal class ListProxyQueryImpl<TValue> : ListQueryImpl<TValue>, ListProxyQuery<TValue>
   {
      internal ListProxyQueryImpl( IList<TValue> list )
         : base( list )
      {
      }

      #region MutableQuery<ListQuery<TValue>> Members

      public ListQuery<TValue> IQ
      {
         get
         {
            return this;
         }
      }

      #endregion
   }
}
