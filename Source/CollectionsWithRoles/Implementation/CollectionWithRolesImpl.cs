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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CollectionsWithRoles.API;
using CommonUtils;

namespace CollectionsWithRoles.Implementation
{
   [DebuggerDisplay( "Count = {_state.collection.Count}" ), DebuggerTypeProxy( typeof( CollectionMutableDebugView<,> ) )]
   internal class CollectionMutableImpl<TValue, TCollectionQuery> : CollectionMutable<TValue, TCollectionQuery>
      where TCollectionQuery : class, CollectionQuery<TValue>
   {
      private readonly TCollectionQuery _cq;
      private readonly CollectionAdditionOnly<TValue> _addition;
      private readonly CollectionState<TValue> _state;

      internal CollectionMutableImpl( TCollectionQuery cq, CollectionState<TValue> state )
      {
         ArgumentValidator.ValidateNotNull( "Collection query", cq );
         ArgumentValidator.ValidateNotNull( "State", state );
         this._cq = cq;
         this._state = state;
         this._addition = new CollectionAdditionOnlyImpl<TValue, TCollectionQuery>( this );
      }

      #region CollectionMutable<TValue> Members

      public void Add( TValue item )
      {
         this._state.collection.Add( item );
      }

      public void AddRange( IEnumerable<TValue> items )
      {
         ArgumentValidator.ValidateNotNull( "Items", items );
         if ( this._state.collection is List<TValue> )
         {
            ( (List<TValue>) this._state.collection ).AddRange( items );
         }
         else
         {
            foreach ( TValue item in items )
            {
               this._state.collection.Add( item );
            }
         }
      }

      public bool Remove( TValue item )
      {
         return this._state.collection.Remove( item );
      }

      public void Clear()
      {
         this._state.collection.Clear();
      }

      public TCollectionQuery CQ
      {
         get
         {
            return this._cq;
         }
      }

      public CollectionAdditionOnly<TValue> AO
      {
         get
         {
            return this._addition;
         }
      }

      #endregion
   }

   [DebuggerDisplay( "Count = {_mutable.CQ.Count}" ), DebuggerTypeProxy( typeof( CollectionAdditionOnlyDebugView<,> ) )]
   internal class CollectionAdditionOnlyImpl<TValue, TCollectionQuery> : CollectionAdditionOnly<TValue>
      where TCollectionQuery : CollectionQuery<TValue>
   {
      private readonly CollectionMutable<TValue, TCollectionQuery> _mutable;

      internal CollectionAdditionOnlyImpl( CollectionMutable<TValue, TCollectionQuery> mutable )
      {
         ArgumentValidator.ValidateNotNull( "Mutable collection", mutable );

         this._mutable = mutable;
      }

      #region CollectionAdditionOnly<TValue> Members

      public void Add( TValue item )
      {
         this._mutable.Add( item );
      }

      public void AddRange( IEnumerable<TValue> items )
      {
         this._mutable.AddRange( items );
      }

      #endregion
   }

   [DebuggerDisplay( "Count = {_state.collection.Count}" ), DebuggerTypeProxy( typeof( CollectionQueryDebugView<> ) )]
   internal class CollectionQueryImpl<TValue> : CollectionQuery<TValue>
   {
      private readonly CollectionState<TValue> _state;

      internal CollectionQueryImpl( CollectionState<TValue> state )
      {
         ArgumentValidator.ValidateNotNull( "State", state );
         this._state = state;
      }

      #region CollectionQuery<TValue> Members

      public Int32 Count
      {
         get
         {
            return this._state.collection.Count;
         }
      }

      public virtual Boolean Contains( TValue item, IEqualityComparer<TValue> equalityComparer = null )
      {
         return equalityComparer == null ?
            this._state.collection.Contains( item ) :
            this._state.collection.Any( m => equalityComparer.Equals( m, item ) );
      }

      public void CopyTo( TValue[] array, Int32 arrayOffset )
      {
         this._state.collection.CopyTo( array, arrayOffset );
      }

      #endregion

      #region IEnumerable<TValue> Members

      public IEnumerator<TValue> GetEnumerator()
      {
         return this._state.collection.GetEnumerator();
      }

      #endregion

      #region IEnumerable Members

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return this._state.collection.GetEnumerator();
      }

      #endregion

   }

   [DebuggerDisplay( "Count = {_state.collection.Count}" ), DebuggerTypeProxy( typeof( CollectionWithRolesDebugView<,,,,,> ) )]
   internal class CollectionWithRolesImpl<TMutableQueryRole, TQueriesQueryRole, TImmutableQueryRole, TValue, TValueQuery, TValueImmutable> : CollectionMutableImpl<TValue, TMutableQueryRole>, CollectionWithRoles<TMutableQueryRole, TQueriesQueryRole, TImmutableQueryRole, TValue, TValueQuery, TValueImmutable>
      where TMutableQueryRole : class, CollectionQueryOfMutables<TQueriesQueryRole, TImmutableQueryRole, TValue, TValueQuery, TValueImmutable>
      where TQueriesQueryRole : class, CollectionQueryOfQueries<TImmutableQueryRole, TValueQuery, TValueImmutable>
      where TImmutableQueryRole : CollectionQuery<TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      internal CollectionWithRolesImpl( TMutableQueryRole mutableQuery, CollectionState<TValue> state )
         : base( mutableQuery, state )
      {
      }

      #region Mutable<TMutableQueryRole,TImmutableQueryRole> Members

      public TMutableQueryRole MQ
      {
         get
         {
            return this.CQ;
         }
      }

      #endregion
   }

   [DebuggerDisplay( "Count = {_state.collection.Count}" ), DebuggerTypeProxy( typeof( CollectionQueryOfMutablesDebugView<,,,,> ) )]
   internal class CollectionQueryOfMutablesImpl<TQueryOfQueriesRole, TImmutableQueryRole, TValue, TValueQuery, TValueImmutable> : CollectionQueryImpl<TValue>, CollectionQueryOfMutables<TQueryOfQueriesRole, TImmutableQueryRole, TValue, TValueQuery, TValueImmutable>
      where TQueryOfQueriesRole : class, CollectionQueryOfQueries<TImmutableQueryRole, TValueQuery, TValueImmutable>
      where TImmutableQueryRole : class, CollectionQuery<TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly TImmutableQueryRole _iq;
      private readonly TQueryOfQueriesRole _cmq;

      internal CollectionQueryOfMutablesImpl( TImmutableQueryRole immutableQuery, TQueryOfQueriesRole cmq, CollectionState<TValue> state )
         : base( state )
      {
         ArgumentValidator.ValidateNotNull( "Collection query of queries", cmq );
         ArgumentValidator.ValidateNotNull( "Immutable query", immutableQuery );

         this._iq = immutableQuery;
         this._cmq = cmq;
      }

      #region CollectionMutableQuery<TMutableQuery,TImmutableQuery> Members

      public TQueryOfQueriesRole Queries
      {
         get
         {
            return this._cmq;
         }
      }

      #endregion

      #region MutableQuery<TImmutableQueryRole> Members

      public TImmutableQueryRole IQ
      {
         get
         {
            return this._iq;
         }
      }

      #endregion
   }

   [DebuggerDisplay( "Count = {_state.collection.Count}" ), DebuggerTypeProxy( typeof( CollectionQueryOfQueriesDebugView<,,,> ) )]
   internal class CollectionQueryOfQueriesImpl<TImmutableQueryRole, TValue, TValueQuery, TValueImmutable> : MutableQuerySkeleton<TImmutableQueryRole>, CollectionQueryOfQueries<TImmutableQueryRole, TValueQuery, TValueImmutable>
      where TImmutableQueryRole : class, CollectionQuery<TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly CollectionState<TValue> _state;

      internal CollectionQueryOfQueriesImpl( TImmutableQueryRole immutableQuery, CollectionState<TValue> state )
         : base( immutableQuery )
      {
         ArgumentValidator.ValidateNotNull( "State", state );

         this._state = state;
      }

      #region CollectionMutableQueryOfQueries<TImmutableQueryRole,TValueQuery,TValueImmutable> Members

      public Int32 Count
      {
         get
         {
            return this._state.collection.Count;
         }
      }

      public virtual Boolean Contains( TValueQuery item, IEqualityComparer<TValueQuery> equalityComparer = null )
      {
         equalityComparer = equalityComparer ?? EqualityComparer<TValueQuery>.Default;
         return this._state.collection.Any( m => ( m == null && item == null ) || ( m != null && equalityComparer.Equals( m.MQ, item ) ) );
      }

      public void CopyTo( TValueQuery[] array, int arrayOffset )
      {
         TValue[] arr = new TValue[Math.Max( array.Length - arrayOffset, 0 )];
         this._state.collection.CopyTo( arr, 0 );
         for ( Int32 idx = 0; idx < arr.Length; ++idx )
         {
            array[idx + arrayOffset] = arr[idx] == null ? default( TValueQuery ) : arr[idx].MQ;
         }
      }

      #endregion

      #region IEnumerable<TValueQuery> Members

      public IEnumerator<TValueQuery> GetEnumerator()
      {
         return this._state.collection.Select( item => item == null ? default( TValueQuery ) : item.MQ ).GetEnumerator();
      }

      #endregion

      #region IEnumerable Members

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return this._state.collection.Select( item => item == null ? default( TValueQuery ) : item.MQ ).GetEnumerator();
      }

      #endregion
   }

   [DebuggerDisplay( "Count = {_state.collection.Count}" ), DebuggerTypeProxy( typeof( CollectionImmutableQueryDebugView<,,> ) )]
   internal class CollectionImmutableQueryImpl<TValue, TValueQuery, TValueImmutable> : CollectionQuery<TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly CollectionState<TValue> _state;

      internal CollectionImmutableQueryImpl( CollectionState<TValue> state )
      {
         ArgumentValidator.ValidateNotNull( "State", state );

         this._state = state;
      }

      #region CollectionImmutableQuery<TImmutable> Members

      public Int32 Count
      {
         get
         {
            return this._state.collection.Count;
         }
      }

      public virtual Boolean Contains( TValueImmutable item, IEqualityComparer<TValueImmutable> equalityComparer = null )
      {
         equalityComparer = equalityComparer ?? EqualityComparer<TValueImmutable>.Default;
         return this._state.collection.Any( m => ( m == null && item == null ) || ( m != null && equalityComparer.Equals( m.MQ.IQ, item ) ) );
      }

      public void CopyTo( TValueImmutable[] array, int arrayOffset )
      {
         TValue[] arr = new TValue[Math.Max( array.Length - arrayOffset, 0 )];
         this._state.collection.CopyTo( arr, 0 );
         for ( Int32 idx = 0; idx < arr.Length; ++idx )
         {
            array[idx + arrayOffset] = arr[idx] == null || arr[idx].MQ == null ? default( TValueImmutable ) : arr[idx].MQ.IQ;
         }
      }

      #endregion

      #region IEnumerable<TImmutable> Members

      public virtual IEnumerator<TValueImmutable> GetEnumerator()
      {
         return this._state.collection.Select( item => item == null || item.MQ == null ? default( TValueImmutable ) : item.MQ.IQ ).GetEnumerator();
      }

      #endregion

      #region IEnumerable Members

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return this._state.collection.Select( item => item == null || item.MQ == null ? default( TValueImmutable ) : item.MQ.IQ ).GetEnumerator();
      }

      #endregion

   }

   internal class CollectionState<TMutable>
   {
      public readonly ICollection<TMutable> collection;

      internal CollectionState( ICollection<TMutable> collection )
      {
         ArgumentValidator.ValidateNotNull( "Collection", collection );
         this.collection = collection;
      }

   }

   internal sealed class CollectionImmutableQueryDebugView<TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly CollectionQuery<TValueImmutable> _collection;

      internal CollectionImmutableQueryDebugView( CollectionImmutableQueryImpl<TValue, TValueQuery, TValueImmutable> collection )
         : this( (CollectionQuery<TValueImmutable>) collection )
      {
      }

      internal CollectionImmutableQueryDebugView( CollectionQuery<TValueImmutable> collectionIQ )
      {
         this._collection = collectionIQ;
      }

      [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
      public TValueImmutable[] Items
      {
         get
         {
            TValueImmutable[] array = new TValueImmutable[this._collection.Count];
            this._collection.CopyTo( array, 0 );
            return array;
         }
      }
   }

   internal sealed class CollectionMutableDebugView<TValue, TCollectionQuery>
      where TCollectionQuery : class, CollectionQuery<TValue>
   {
      private readonly CollectionQuery<TValue> _collection;

      internal CollectionMutableDebugView( CollectionMutableImpl<TValue, TCollectionQuery> collection )
      {
         this._collection = collection.CQ;
      }


      [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
      public TValue[] Items
      {
         get
         {
            TValue[] array = new TValue[this._collection.Count];
            this._collection.CopyTo( array, 0 );
            return array;
         }
      }
   }

   internal sealed class CollectionAdditionOnlyDebugView<TValue, TCollectionQuery>
      where TCollectionQuery : CollectionQuery<TValue>
   {
      private readonly CollectionQuery<TValue> _collection;

      internal CollectionAdditionOnlyDebugView( CollectionAdditionOnlyImpl<TValue, TCollectionQuery> collection )
      {
         this._collection = ( (CollectionMutable<TValue, TCollectionQuery>) collection.GetType()
#if WINDOWS_PHONE_APP
            .GetRuntimeField("_mutable")
#else
.GetField( "_mutable" )
#endif
.GetValue( collection ) ).CQ;
      }

      [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
      public TValue[] Items
      {
         get
         {
            TValue[] array = new TValue[this._collection.Count];
            this._collection.CopyTo( array, 0 );
            return array;
         }
      }
   }

   internal sealed class CollectionQueryDebugView<TValue>
   {
      private readonly CollectionQuery<TValue> _collection;

      internal CollectionQueryDebugView( CollectionQueryImpl<TValue> collection )
      {
         this._collection = collection;
      }


      [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
      public TValue[] Items
      {
         get
         {
            TValue[] array = new TValue[this._collection.Count];
            this._collection.CopyTo( array, 0 );
            return array;
         }
      }
   }

   internal sealed class CollectionWithRolesDebugView<TMutableQueryRole, TQueriesQueryRole, TImmutableQueryRole, TValue, TValueQuery, TValueImmutable>
      where TMutableQueryRole : class, CollectionQueryOfMutables<TQueriesQueryRole, TImmutableQueryRole, TValue, TValueQuery, TValueImmutable>
      where TQueriesQueryRole : class, CollectionQueryOfQueries<TImmutableQueryRole, TValueQuery, TValueImmutable>
      where TImmutableQueryRole : CollectionQuery<TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly CollectionQuery<TValueImmutable> _collection;

      internal CollectionWithRolesDebugView( CollectionWithRolesImpl<TMutableQueryRole, TQueriesQueryRole, TImmutableQueryRole, TValue, TValueQuery, TValueImmutable> collection )
      {
         this._collection = collection.MQ.IQ;
      }

      [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
      public TValueImmutable[] Items
      {
         get
         {
            TValueImmutable[] array = new TValueImmutable[this._collection.Count];
            this._collection.CopyTo( array, 0 );
            return array;
         }
      }
   }

   internal sealed class CollectionQueryOfMutablesDebugView<TQueryOfQueriesRole, TImmutableQueryRole, TValue, TValueQuery, TValueImmutable>
      where TQueryOfQueriesRole : class, CollectionQueryOfQueries<TImmutableQueryRole, TValueQuery, TValueImmutable>
      where TImmutableQueryRole : class, CollectionQuery<TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly CollectionQuery<TValueImmutable> _collection;

      internal CollectionQueryOfMutablesDebugView( CollectionQueryOfMutablesImpl<TQueryOfQueriesRole, TImmutableQueryRole, TValue, TValueQuery, TValueImmutable> collection )
      {
         this._collection = collection.IQ;
      }

      [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
      public TValueImmutable[] Items
      {
         get
         {
            TValueImmutable[] array = new TValueImmutable[this._collection.Count];
            this._collection.CopyTo( array, 0 );
            return array;
         }
      }
   }

   internal sealed class CollectionQueryOfQueriesDebugView<TImmutableQueryRole, TValue, TValueQuery, TValueImmutable>
      where TImmutableQueryRole : class, CollectionQuery<TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly CollectionQuery<TValueImmutable> _collection;

      internal CollectionQueryOfQueriesDebugView( CollectionQueryOfQueriesImpl<TImmutableQueryRole, TValue, TValueQuery, TValueImmutable> collection )
      {
         this._collection = collection.IQ;
      }

      [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
      public TValueImmutable[] Items
      {
         get
         {
            TValueImmutable[] array = new TValueImmutable[this._collection.Count];
            this._collection.CopyTo( array, 0 );
            return array;
         }
      }
   }
}
