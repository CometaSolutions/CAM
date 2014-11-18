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
using System.Collections.Generic;
using System.Linq;
using CollectionsWithRoles.API;
using CommonUtils;

namespace CollectionsWithRoles.Implementation
{
   internal class SetWithRolesImpl<TValue, TValueQuery, TValueImmutable> : CollectionWithRolesImpl<SetQueryOfMutables<TValue, TValueQuery, TValueImmutable>, SetQueryOfQueries<TValueQuery, TValueImmutable>, SetQuery<TValueImmutable>, TValue, TValueQuery, TValueImmutable>, SetWithRoles<TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {

      private readonly SetState<TValue> _state;

      internal SetWithRolesImpl( SetQueryOfMutables<TValue, TValueQuery, TValueImmutable> queryOfMutables, SetState<TValue> state )
         : base( queryOfMutables, state )
      {
         this._state = state;
      }

      #region SetMutable<TValue,SetQueryOfMutables<TValue,TValueQuery,TValueImmutable>> Members

      public new bool Add( TValue item )
      {
         return this._state.set.Add( item );
      }

      public void UnionWith( IEnumerable<TValue> other )
      {
         this._state.set.UnionWith( other );
      }

      public void IntersectWith( IEnumerable<TValue> other )
      {
         this._state.set.IntersectWith( other );
      }

      public void ExceptWith( IEnumerable<TValue> other )
      {
         this._state.set.ExceptWith( other );
      }

      public void SymmetricExceptWith( IEnumerable<TValue> other )
      {
         this._state.set.SymmetricExceptWith( other );
      }

      #endregion
   }

   internal class SetQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable> : CollectionQueryOfMutablesImpl<SetQueryOfQueries<TValueQuery, TValueImmutable>, SetQuery<TValueImmutable>, TValue, TValueQuery, TValueImmutable>, SetQueryOfMutables<TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly SetState<TValue> _state;

      internal SetQueryOfMutablesImpl( SetQuery<TValueImmutable> immutableQuery, SetQueryOfQueries<TValueQuery, TValueImmutable> cmq, SetState<TValue> state )
         : base( immutableQuery, cmq, state )
      {
         this._state = state;
      }

      #region SetQuery<TValue> Members

      public bool IsSubsetOf( IEnumerable<TValue> other )
      {
         return this._state.set.IsSubsetOf( other );
      }

      public bool IsSupersetOf( IEnumerable<TValue> other )
      {
         return this._state.set.IsSupersetOf( other );
      }

      public bool IsProperSupersetOf( IEnumerable<TValue> other )
      {
         return this._state.set.IsProperSupersetOf( other );
      }

      public bool IsProperSubsetOf( IEnumerable<TValue> other )
      {
         return this._state.set.IsProperSubsetOf( other );
      }

      public bool Overlaps( IEnumerable<TValue> other )
      {
         return this._state.set.Overlaps( other );
      }

      public bool SetEquals( IEnumerable<TValue> other )
      {
         return this._state.set.SetEquals( other );
      }

      #endregion
   }

   // TODO the following two classes (SetQueryOfQueriesImpl and SetImmutableQueryImpl) consume as little memory as normal collections, but are quite slow. Provide alternative classes, which consume more memory but provide fast operations.
   internal class SetQueryOfQueriesImpl<TValue, TValueQuery, TValueImmutable> : CollectionQueryOfQueriesImpl<SetQuery<TValueImmutable>, TValue, TValueQuery, TValueImmutable>, SetQueryOfQueries<TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly SetState<TValue> _state;

      internal SetQueryOfQueriesImpl( SetQuery<TValueImmutable> iq, SetState<TValue> state )
         : base( iq, state )
      {
         this._state = state;
      }

      #region SetQuery<TValueQuery> Members

      public bool IsSubsetOf( IEnumerable<TValueQuery> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this._state.set.Select( item => item == null ? default( TValueQuery ) : item.MQ ).Intersect( other ).Count() == this._state.set.Count;
      }

      public bool IsSupersetOf( IEnumerable<TValueQuery> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this._state.set.Select( item => item == null ? default( TValueQuery ) : item.MQ ).Intersect( other ).Count() == other.Count();
      }

      public bool IsProperSupersetOf( IEnumerable<TValueQuery> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this._state.set.Count > other.Count() && this.IsSupersetOf( other );
      }

      public bool IsProperSubsetOf( IEnumerable<TValueQuery> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this._state.set.Count < other.Count() && this.IsSubsetOf( other );
      }

      public bool Overlaps( IEnumerable<TValueQuery> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this._state.set.Select( item => item == null ? default( TValueQuery ) : item.MQ ).Intersect( other ).Any();
      }

      public bool SetEquals( IEnumerable<TValueQuery> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this._state.set.Count == other.Count() && this.IsSubsetOf( other );
      }

      #endregion
   }

   internal class SetImmutableQueryImpl<TValue, TValueQuery, TValueImmutable> : CollectionImmutableQueryImpl<TValue, TValueQuery, TValueImmutable>, SetQuery<TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly SetState<TValue> _state;

      internal SetImmutableQueryImpl( SetState<TValue> state )
         : base( state )
      {
         this._state = state;
      }

      #region SetQuery<TValueImmutable> Members

      public bool IsSubsetOf( IEnumerable<TValueImmutable> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this._state.set.Select( item => item == null || item.MQ == null ? default( TValueImmutable ) : item.MQ.IQ ).Intersect( other ).Count() == this._state.set.Count;
      }

      public bool IsSupersetOf( IEnumerable<TValueImmutable> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this._state.set.Select( item => item == null || item.MQ == null ? default( TValueImmutable ) : item.MQ.IQ ).Intersect( other ).Count() == other.Count();
      }

      public bool IsProperSupersetOf( IEnumerable<TValueImmutable> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this._state.set.Count > other.Count() && this.IsSupersetOf( other );
      }

      public bool IsProperSubsetOf( IEnumerable<TValueImmutable> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this._state.set.Count < other.Count() && this.IsSubsetOf( other );
      }

      public bool Overlaps( IEnumerable<TValueImmutable> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this._state.set.Select( item => item == null || item.MQ == null ? default( TValueImmutable ) : item.MQ.IQ ).Intersect( other ).Any();
      }

      public bool SetEquals( IEnumerable<TValueImmutable> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this._state.set.Count == other.Count() && this.IsSubsetOf( other );
      }

      #endregion
   }

   internal class SetState<TValue> : CollectionState<TValue>
   {
      public readonly ISet<TValue> set;

      internal SetState( ISet<TValue> setToUse )
         : base( setToUse )
      {
         this.set = setToUse;
      }
   }

   internal class SetQueryImpl<TValue> : CollectionQueryImpl<TValue>, SetQuery<TValue>
   {
      private readonly SetState<TValue> _state;

      internal SetQueryImpl( SetState<TValue> state )
         : base( state )
      {
         this._state = state;
      }

      #region SetQuery<TValue> Members

      public bool IsSubsetOf( IEnumerable<TValue> other )
      {
         return this._state.set.IsSubsetOf( other );
      }

      public bool IsSupersetOf( IEnumerable<TValue> other )
      {
         return this._state.set.IsSupersetOf( other );
      }

      public bool IsProperSupersetOf( IEnumerable<TValue> other )
      {
         return this._state.set.IsProperSupersetOf( other );
      }

      public bool IsProperSubsetOf( IEnumerable<TValue> other )
      {
         return this._state.set.IsProperSubsetOf( other );
      }

      public bool Overlaps( IEnumerable<TValue> other )
      {
         return this._state.set.Overlaps( other );
      }

      public bool SetEquals( IEnumerable<TValue> other )
      {
         return this._state.set.SetEquals( other );
      }

      #endregion
   }

   internal class SetProxyImpl<TValue> : CollectionMutableImpl<TValue, SetQuery<TValue>>, SetProxy<TValue>
   {
      private readonly SetState<TValue> _state;

      internal SetProxyImpl( SetProxyQuery<TValue> cq, SetState<TValue> state )
         : base( cq, state )
      {
         this._state = state;
      }

      #region SetMutable<TValue,SetQuery<TValue>> Members

      public new bool Add( TValue item )
      {
         return this._state.set.Add( item );
      }

      public void UnionWith( IEnumerable<TValue> other )
      {
         this._state.set.UnionWith( other );
      }

      public void IntersectWith( IEnumerable<TValue> other )
      {
         this._state.set.IntersectWith( other );
      }

      public void ExceptWith( IEnumerable<TValue> other )
      {
         this._state.set.ExceptWith( other );
      }

      public void SymmetricExceptWith( IEnumerable<TValue> other )
      {
         this._state.set.SymmetricExceptWith( other );
      }

      #endregion

      #region Mutable<SetProxyQuery<TValue>,SetQuery<TValue>> Members

      public SetProxyQuery<TValue> MQ
      {
         get
         {
            return (SetProxyQuery<TValue>) this.CQ;
         }
      }

      #endregion
   }

   internal class SetProxyQueryImpl<TValue> : SetQueryImpl<TValue>, SetProxyQuery<TValue>
   {
      internal SetProxyQueryImpl( SetState<TValue> state )
         : base( state )
      {
      }

      #region MutableQuery<SetQuery<TValue>> Members

      public SetQuery<TValue> IQ
      {
         get
         {
            return this;
         }
      }

      #endregion
   }

   internal class FastSetState<TValue, TValueQuery, TValueImmutable> : SetState<TValue>
   {
      public readonly ISet<TValueQuery> queries;
      public readonly ISet<TValueImmutable> immutables;

      internal FastSetState( ISet<TValue> mutables, ISet<TValueQuery> queries, ISet<TValueImmutable> immutables )
         : base( mutables )
      {
         ArgumentValidator.ValidateNotNull( "Queries", queries );
         ArgumentValidator.ValidateNotNull( "Immutables", immutables );

         this.queries = queries;
         this.immutables = immutables;
      }
   }

   internal class FastSetQueryOfQueriesImpl<TValue, TValueQuery, TValueImmutable> : CollectionQueryOfQueriesImpl<SetQuery<TValueImmutable>, TValue, TValueQuery, TValueImmutable>, SetQueryOfQueries<TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly FastSetState<TValue, TValueQuery, TValueImmutable> _state;

      internal FastSetQueryOfQueriesImpl( SetQuery<TValueImmutable> immutableQuery, FastSetState<TValue, TValueQuery, TValueImmutable> state )
         : base( immutableQuery, state )
      {
         this._state = state;
      }

      #region SetQuery<TValueQuery> Members

      public bool IsSubsetOf( IEnumerable<TValueQuery> other )
      {
         return this._state.queries.IsSubsetOf( other );
      }

      public bool IsSupersetOf( IEnumerable<TValueQuery> other )
      {
         return this._state.queries.IsSupersetOf( other );
      }

      public bool IsProperSupersetOf( IEnumerable<TValueQuery> other )
      {
         return this._state.queries.IsProperSupersetOf( other );
      }

      public bool IsProperSubsetOf( IEnumerable<TValueQuery> other )
      {
         return this._state.queries.IsProperSubsetOf( other );
      }

      public bool Overlaps( IEnumerable<TValueQuery> other )
      {
         return this._state.queries.Overlaps( other );
      }

      public bool SetEquals( IEnumerable<TValueQuery> other )
      {
         return this._state.queries.SetEquals( other );
      }

      #endregion
   }

   internal class FastSetImmutableQueryImpl<TValue, TValueQuery, TValueImmutable> : CollectionImmutableQueryImpl<TValue, TValueQuery, TValueImmutable>, SetQuery<TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly FastSetState<TValue, TValueQuery, TValueImmutable> _state;

      internal FastSetImmutableQueryImpl( FastSetState<TValue, TValueQuery, TValueImmutable> state )
         : base( state )
      {
         this._state = state;
      }

      #region SetQuery<TValueImmutable> Members

      public bool IsSubsetOf( IEnumerable<TValueImmutable> other )
      {
         return this._state.immutables.IsSubsetOf( other );
      }

      public bool IsSupersetOf( IEnumerable<TValueImmutable> other )
      {
         return this._state.immutables.IsSupersetOf( other );
      }

      public bool IsProperSupersetOf( IEnumerable<TValueImmutable> other )
      {
         return this._state.immutables.IsProperSupersetOf( other );
      }

      public bool IsProperSubsetOf( IEnumerable<TValueImmutable> other )
      {
         return this._state.immutables.IsProperSubsetOf( other );
      }

      public bool Overlaps( IEnumerable<TValueImmutable> other )
      {
         return this._state.immutables.Overlaps( other );
      }

      public bool SetEquals( IEnumerable<TValueImmutable> other )
      {
         return this._state.immutables.SetEquals( other );
      }

      #endregion
   }
}
