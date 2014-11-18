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
   internal class ListWithRolesImpl<TValue, TValueQuery, TValueImmutable> : CollectionWithRolesImpl<ListQueryOfMutables<TValue, TValueQuery, TValueImmutable>, ListQueryOfQueries<TValueQuery, TValueImmutable>, ListQuery<TValueImmutable>, TValue, TValueQuery, TValueImmutable>, ListWithRoles<TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly ListState<TValue> _state;

      internal ListWithRolesImpl( ListQueryOfMutables<TValue, TValueQuery, TValueImmutable> mutableQuery, ListState<TValue> state )
         : base( mutableQuery, state )
      {
         this._state = state;
      }

      #region ListWithRoles<TMutableQuery,TImmutableQuery> Members

      public TValue this[int index]
      {
         set
         {
            this._state.list[index] = value;
         }
      }

      public void Insert( int index, TValue item )
      {
         this._state.list.Insert( index, item );
      }

      public void RemoveAt( int index )
      {
         this._state.list.RemoveAt( index );
      }

      #endregion
   }

   internal class ListQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable> : CollectionQueryOfMutablesImpl<ListQueryOfQueries<TValueQuery, TValueImmutable>, ListQuery<TValueImmutable>, TValue, TValueQuery, TValueImmutable>, ListQueryOfMutables<TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly ListState<TValue> _state;

      internal ListQueryOfMutablesImpl( ListQuery<TValueImmutable> immutableQuery, ListQueryOfQueries<TValueQuery, TValueImmutable> cmq, ListState<TValue> state )
         : base( immutableQuery, cmq, state )
      {
         this._state = state;
      }

      #region ListMutableQuery<TMutableQuery,TImmutableQuery> Members

      public TValue this[int index]
      {
         get
         {
            return this._state.list[index];
         }
      }

      public int IndexOf( TValue item )
      {
         return this._state.list.IndexOf( item );
      }

      #endregion

      public override Boolean Contains( TValue item, IEqualityComparer<TValue> equalityComparer = null )
      {
         equalityComparer = equalityComparer ?? EqualityComparer<TValue>.Default;
         for ( var i = 0; i < this._state.list.Count; ++i )
         {
            if ( equalityComparer.Equals( this._state.list[i], item ) )
            {
               return true;
            }
         }
         return false;
      }
   }

   internal class ListQueryOfQueriesImpl<TValue, TValueQuery, TValueImmutable> : CollectionQueryOfQueriesImpl<ListQuery<TValueImmutable>, TValue, TValueQuery, TValueImmutable>, ListQueryOfQueries<TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly ListState<TValue> _state;

      internal ListQueryOfQueriesImpl( ListQuery<TValueImmutable> immutableQuery, ListState<TValue> state )
         : base( immutableQuery, state )
      {
         this._state = state;
      }

      #region ListMutableQueryOfQueries<TValueQuery,TValueImmutable> Members

      public TValueQuery this[int index]
      {
         get
         {
            TValue value = this._state.list[index];
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
         while ( result == -1 && idx < this._state.list.Count )
         {
            TValue value = this._state.list[idx];
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

   internal class ListImmutableQueryImpl<TValue, TMutableQuery, TImmutableQuery> : CollectionImmutableQueryImpl<TValue, TMutableQuery, TImmutableQuery>, ListQuery<TImmutableQuery>
      where TValue : Mutable<TMutableQuery, TImmutableQuery>
      where TMutableQuery : MutableQuery<TImmutableQuery>
   {
      private readonly ListState<TValue> _state;

      internal ListImmutableQueryImpl( ListState<TValue> state )
         : base( state )
      {
         this._state = state;
      }

      #region ListImmutableQuery<TImmutableQuery> Members

      public TImmutableQuery this[int index]
      {
         get
         {
            TValue value = this._state.list[index];
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
         while ( result == -1 && idx < this._state.list.Count )
         {
            TValue value = this._state.list[idx];
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
         for ( var i = 0; i < this._state.list.Count; ++i )
         {
            var cur = this._state.list[i];
            if ( ( cur == null || item == null ) || ( cur != null && equalityComparer.Equals( cur.MQ.IQ, item ) ) )
            {
               return true;
            }
         }
         return false;
      }
   }

   internal class ListState<TValue> : CollectionState<TValue>
   {
      public readonly IList<TValue> list;

      internal ListState( IList<TValue> list )
         : base( list )
      {
         this.list = list;
      }
   }

   internal class ListQueryImpl<TValueImmutable> : CollectionQueryImpl<TValueImmutable>, ListQuery<TValueImmutable>
   {
      private readonly ListState<TValueImmutable> _state;

      internal ListQueryImpl( ListState<TValueImmutable> state )
         : base( state )
      {
         this._state = state;
      }

      #region ListImmutableQuery<TImmutableQuery> Members

      public TValueImmutable this[int index]
      {
         get
         {
            return this._state.list[index];
         }
      }

      public int IndexOf( TValueImmutable item )
      {
         return this._state.list.IndexOf( item );
      }

      #endregion


      public override Boolean Contains( TValueImmutable item, IEqualityComparer<TValueImmutable> equalityComparer = null )
      {
         equalityComparer = equalityComparer ?? EqualityComparer<TValueImmutable>.Default;
         for ( var i = 0; i < this._state.list.Count; ++i )
         {
            if ( equalityComparer.Equals( this._state.list[i], item ) )
            {
               return true;
            }
         }
         return false;
      }
   }

   internal class ListProxyImpl<TValue> : CollectionMutableImpl<TValue, ListQuery<TValue>>, ListProxy<TValue>
   {
      private readonly ListState<TValue> _state;

      internal ListProxyImpl( ListProxyQuery<TValue> cq, ListState<TValue> state )
         : base( cq, state )
      {
         this._state = state;
      }

      #region ListMutable<TValue> Members

      public TValue this[int index]
      {
         set
         {
            this._state.list[index] = value;
         }
      }

      public void Insert( int index, TValue item )
      {
         this._state.list.Insert( index, item );
      }

      public void RemoveAt( int index )
      {
         this._state.list.RemoveAt( index );
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
      internal ListProxyQueryImpl( ListState<TValue> state )
         : base( state )
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
