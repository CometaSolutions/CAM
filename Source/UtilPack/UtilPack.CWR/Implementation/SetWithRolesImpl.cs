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
using CollectionsWithRoles.API;
using CommonUtils;

namespace CollectionsWithRoles.Implementation
{
   internal class SetWithRolesImpl<TValue, TValueQuery, TValueImmutable> : CollectionWithRolesImpl<ISet<TValue>, SetQueryOfMutables<TValue, TValueQuery, TValueImmutable>, SetQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable>, SetQueryOfQueries<TValueQuery, TValueImmutable>, SetQuery<TValueImmutable>, TValue, TValueQuery, TValueImmutable>, SetWithRoles<TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {

      internal SetWithRolesImpl( SetQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable> queryOfMutables )
         : base( queryOfMutables )
      {
      }

      #region SetMutable<TValue,SetQueryOfMutables<TValue,TValueQuery,TValueImmutable>> Members

      public override void Add( TValue item )
      {
         this.CQImpl.Collection.Add( item );
      }

      Boolean SetMutable<TValue, SetQueryOfMutables<TValue, TValueQuery, TValueImmutable>>.Add( TValue item )
      {
         return this.CQImpl.Collection.Add( item );
      }

      public void UnionWith( IEnumerable<TValue> other )
      {
         this.CQImpl.Collection.UnionWith( other );
      }

      public void IntersectWith( IEnumerable<TValue> other )
      {
         this.CQImpl.Collection.IntersectWith( other );
      }

      public void ExceptWith( IEnumerable<TValue> other )
      {
         this.CQImpl.Collection.ExceptWith( other );
      }

      public void SymmetricExceptWith( IEnumerable<TValue> other )
      {
         this.CQImpl.Collection.SymmetricExceptWith( other );
      }

      #endregion
   }

   internal class SetQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable> : CollectionQueryOfMutablesImpl<ISet<TValue>, SetQueryOfQueries<TValueQuery, TValueImmutable>, SetQuery<TValueImmutable>, SetImmutableQueryImpl<TValue, TValueQuery, TValueImmutable>, TValue, TValueQuery, TValueImmutable>, SetQueryOfMutables<TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {

      internal SetQueryOfMutablesImpl( SetImmutableQueryImpl<TValue, TValueQuery, TValueImmutable> immutableQuery, SetQueryOfQueries<TValueQuery, TValueImmutable> cmq )
         : base( immutableQuery, cmq )
      {
      }

      #region SetQuery<TValue> Members

      public bool IsSubsetOf( IEnumerable<TValue> other )
      {
         return this.Collection.IsSubsetOf( other );
      }

      public bool IsSupersetOf( IEnumerable<TValue> other )
      {
         return this.Collection.IsSupersetOf( other );
      }

      public bool IsProperSupersetOf( IEnumerable<TValue> other )
      {
         return this.Collection.IsProperSupersetOf( other );
      }

      public bool IsProperSubsetOf( IEnumerable<TValue> other )
      {
         return this.Collection.IsProperSubsetOf( other );
      }

      public bool Overlaps( IEnumerable<TValue> other )
      {
         return this.Collection.Overlaps( other );
      }

      public bool SetEquals( IEnumerable<TValue> other )
      {
         return this.Collection.SetEquals( other );
      }

      #endregion
   }

   // TODO the following two classes (SetQueryOfQueriesImpl and SetImmutableQueryImpl) consume as little memory as normal collections, but are quite slow. Provide alternative classes, which consume more memory but provide fast operations.
   internal class SetQueryOfQueriesImpl<TValue, TValueQuery, TValueImmutable> : CollectionQueryOfQueriesImpl<ISet<TValue>, SetQuery<TValueImmutable>, SetImmutableQueryImpl<TValue, TValueQuery, TValueImmutable>, TValue, TValueQuery, TValueImmutable>, SetQueryOfQueries<TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {

      internal SetQueryOfQueriesImpl( SetImmutableQueryImpl<TValue, TValueQuery, TValueImmutable> iq )
         : base( iq )
      {
      }

      #region SetQuery<TValueQuery> Members

      public bool IsSubsetOf( IEnumerable<TValueQuery> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this._collection.Select( item => item == null ? default( TValueQuery ) : item.MQ ).Intersect( other ).Count() == this._collection.Count;
      }

      public bool IsSupersetOf( IEnumerable<TValueQuery> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this._collection.Select( item => item == null ? default( TValueQuery ) : item.MQ ).Intersect( other ).Count() == other.Count();
      }

      public bool IsProperSupersetOf( IEnumerable<TValueQuery> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this._collection.Count > other.Count() && this.IsSupersetOf( other );
      }

      public bool IsProperSubsetOf( IEnumerable<TValueQuery> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this._collection.Count < other.Count() && this.IsSubsetOf( other );
      }

      public bool Overlaps( IEnumerable<TValueQuery> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this._collection.Select( item => item == null ? default( TValueQuery ) : item.MQ ).Intersect( other ).Any();
      }

      public bool SetEquals( IEnumerable<TValueQuery> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this._collection.Count == other.Count() && this.IsSubsetOf( other );
      }

      #endregion
   }

   internal class SetImmutableQueryImpl<TValue, TValueQuery, TValueImmutable> : CollectionImmutableQueryImpl<ISet<TValue>, TValue, TValueQuery, TValueImmutable>, SetQuery<TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {

      internal SetImmutableQueryImpl( ISet<TValue> collection )
         : base( collection )
      {
      }

      #region SetQuery<TValueImmutable> Members

      public virtual bool IsSubsetOf( IEnumerable<TValueImmutable> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this.Collection.Select( item => item == null || item.MQ == null ? default( TValueImmutable ) : item.MQ.IQ ).Intersect( other ).Count() == this.Collection.Count;
      }

      public virtual bool IsSupersetOf( IEnumerable<TValueImmutable> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this.Collection.Select( item => item == null || item.MQ == null ? default( TValueImmutable ) : item.MQ.IQ ).Intersect( other ).Count() == other.Count();
      }

      public virtual bool IsProperSupersetOf( IEnumerable<TValueImmutable> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this.Collection.Count > other.Count() && this.IsSupersetOf( other );
      }

      public virtual bool IsProperSubsetOf( IEnumerable<TValueImmutable> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this.Collection.Count < other.Count() && this.IsSubsetOf( other );
      }

      public virtual bool Overlaps( IEnumerable<TValueImmutable> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this.Collection.Select( item => item == null || item.MQ == null ? default( TValueImmutable ) : item.MQ.IQ ).Intersect( other ).Any();
      }

      public virtual bool SetEquals( IEnumerable<TValueImmutable> other )
      {
         ArgumentValidator.ValidateNotNull( "Other", other );
         return this.Collection.Count == other.Count() && this.IsSubsetOf( other );
      }

      #endregion
   }

   internal class SetQueryImpl<TValue> : CollectionQueryImpl<ISet<TValue>, TValue>, SetQuery<TValue>
   {

      internal SetQueryImpl( ISet<TValue> collection )
         : base( collection )
      {
      }

      #region SetQuery<TValue> Members

      public bool IsSubsetOf( IEnumerable<TValue> other )
      {
         return this.Collection.IsSubsetOf( other );
      }

      public bool IsSupersetOf( IEnumerable<TValue> other )
      {
         return this.Collection.IsSupersetOf( other );
      }

      public bool IsProperSupersetOf( IEnumerable<TValue> other )
      {
         return this.Collection.IsProperSupersetOf( other );
      }

      public bool IsProperSubsetOf( IEnumerable<TValue> other )
      {
         return this.Collection.IsProperSubsetOf( other );
      }

      public bool Overlaps( IEnumerable<TValue> other )
      {
         return this.Collection.Overlaps( other );
      }

      public bool SetEquals( IEnumerable<TValue> other )
      {
         return this.Collection.SetEquals( other );
      }

      #endregion
   }

   internal class SetProxyImpl<TValue> : CollectionMutableImpl<ISet<TValue>, TValue, SetQuery<TValue>, SetProxyQueryImpl<TValue>>, SetProxy<TValue>
   {
      internal SetProxyImpl( SetProxyQueryImpl<TValue> cq )
         : base( cq )
      {
      }

      #region SetMutable<TValue,SetQuery<TValue>> Members

      public override void Add( TValue item )
      {
         this.CQImpl.Collection.Add( item );
      }

      Boolean SetMutable<TValue, SetQuery<TValue>>.Add( TValue item )
      {
         return this.CQImpl.Collection.Add( item );
      }


      public void UnionWith( IEnumerable<TValue> other )
      {
         this.CQImpl.Collection.UnionWith( other );
      }

      public void IntersectWith( IEnumerable<TValue> other )
      {
         this.CQImpl.Collection.IntersectWith( other );
      }

      public void ExceptWith( IEnumerable<TValue> other )
      {
         this.CQImpl.Collection.ExceptWith( other );
      }

      public void SymmetricExceptWith( IEnumerable<TValue> other )
      {
         this.CQImpl.Collection.SymmetricExceptWith( other );
      }

      #endregion

      #region Mutable<SetProxyQuery<TValue>,SetQuery<TValue>> Members

      public SetProxyQuery<TValue> MQ
      {
         get
         {
            return this.CQImpl;
         }
      }

      #endregion
   }

   internal class SetProxyQueryImpl<TValue> : SetQueryImpl<TValue>, SetProxyQuery<TValue>
   {
      internal SetProxyQueryImpl( ISet<TValue> collection )
         : base( collection )
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

   internal sealed class FastSetState<TValue, TValueQuery, TValueImmutable>
   {

      internal FastSetState( ISet<TValueQuery> queries, ISet<TValueImmutable> immutables )
      {
         this.Queries = ArgumentValidator.ValidateNotNull( "Queries", queries );
         this.Immutables = ArgumentValidator.ValidateNotNull( "Immutables", immutables );
      }

      public ISet<TValueQuery> Queries { get; }
      public ISet<TValueImmutable> Immutables { get; }
   }

   internal class FastSetQueryOfQueriesImpl<TValue, TValueQuery, TValueImmutable> : CollectionQueryOfQueriesImpl<ISet<TValue>, SetQuery<TValueImmutable>, FastSetImmutableQueryImpl<TValue, TValueQuery, TValueImmutable>, TValue, TValueQuery, TValueImmutable>, SetQueryOfQueries<TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly FastSetState<TValue, TValueQuery, TValueImmutable> _state;

      internal FastSetQueryOfQueriesImpl( FastSetImmutableQueryImpl<TValue, TValueQuery, TValueImmutable> immutableQuery, FastSetState<TValue, TValueQuery, TValueImmutable> state )
         : base( immutableQuery )
      {
         this._state = state;
      }

      #region SetQuery<TValueQuery> Members

      public bool IsSubsetOf( IEnumerable<TValueQuery> other )
      {
         return this._state.Queries.IsSubsetOf( other );
      }

      public bool IsSupersetOf( IEnumerable<TValueQuery> other )
      {
         return this._state.Queries.IsSupersetOf( other );
      }

      public bool IsProperSupersetOf( IEnumerable<TValueQuery> other )
      {
         return this._state.Queries.IsProperSupersetOf( other );
      }

      public bool IsProperSubsetOf( IEnumerable<TValueQuery> other )
      {
         return this._state.Queries.IsProperSubsetOf( other );
      }

      public bool Overlaps( IEnumerable<TValueQuery> other )
      {
         return this._state.Queries.Overlaps( other );
      }

      public bool SetEquals( IEnumerable<TValueQuery> other )
      {
         return this._state.Queries.SetEquals( other );
      }

      #endregion
   }

   internal class FastSetImmutableQueryImpl<TValue, TValueQuery, TValueImmutable> : SetImmutableQueryImpl<TValue, TValueQuery, TValueImmutable>
      // CollectionImmutableQueryImpl<ISet<TValue>, TValue, TValueQuery, TValueImmutable>, SetQuery<TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly FastSetState<TValue, TValueQuery, TValueImmutable> _state;

      internal FastSetImmutableQueryImpl( ISet<TValue> mutables, FastSetState<TValue, TValueQuery, TValueImmutable> state )
         : base( mutables )
      {
         this._state = state;
      }

      #region SetQuery<TValueImmutable> Members

      public override bool IsSubsetOf( IEnumerable<TValueImmutable> other )
      {
         return this._state.Immutables.IsSubsetOf( other );
      }

      public override bool IsSupersetOf( IEnumerable<TValueImmutable> other )
      {
         return this._state.Immutables.IsSupersetOf( other );
      }

      public override bool IsProperSupersetOf( IEnumerable<TValueImmutable> other )
      {
         return this._state.Immutables.IsProperSupersetOf( other );
      }

      public override bool IsProperSubsetOf( IEnumerable<TValueImmutable> other )
      {
         return this._state.Immutables.IsProperSubsetOf( other );
      }

      public override bool Overlaps( IEnumerable<TValueImmutable> other )
      {
         return this._state.Immutables.Overlaps( other );
      }

      public override bool SetEquals( IEnumerable<TValueImmutable> other )
      {
         return this._state.Immutables.SetEquals( other );
      }

      #endregion
   }
}
