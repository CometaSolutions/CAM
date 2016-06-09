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
using CollectionsWithRoles.API;
using CommonUtils;

namespace CollectionsWithRoles.Implementation
{
   [DebuggerDisplay( "Count = {_state.dictionary.Count}" ), DebuggerTypeProxy( typeof( DictionaryWithRolesDebugView<,,,> ) )]
   internal class DictionaryWithRolesImpl<TKey, TValue, TValueQuery, TValueImmutable> : CollectionMutableImpl<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, DictionaryQueryOfMutables<TKey, TValue, TValueQuery, TValueImmutable>, DictionaryQueryOfMutablesImpl<TKey, TValue, TValueQuery, TValueImmutable>>, DictionaryWithRoles<TKey, TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      internal DictionaryWithRolesImpl( DictionaryQueryOfMutablesImpl<TKey, TValue, TValueQuery, TValueImmutable> mutableQuery )
         : base( mutableQuery )
      {
      }

      #region DictionaryWithRoles<TKey,TMutableQuery,TImmutableQuery> Members

      public TValue this[TKey key]
      {
         set
         {
            this.CQImpl.Collection[key] = value;
         }
      }

      public void Add( TKey key, TValue value )
      {
         this.CQImpl.Collection.Add( key, value );
      }

      public bool Remove( TKey key )
      {
         return this.CQImpl.Collection.Remove( key );
      }

      #endregion

      #region Mutable<DictionaryMutableQuery<TKey,TValue,TValueQuery,TValueImmutable>,DictionaryQuery<TKey,TValueImmutable>> Members

      public DictionaryQueryOfMutables<TKey, TValue, TValueQuery, TValueImmutable> MQ
      {
         get
         {
            return this.CQ;
         }
      }

      #endregion
   }

   [DebuggerDisplay( "Count = {_state.dictionary.Count}" ), DebuggerTypeProxy( typeof( DictionaryQueryOfMutablesDebugView<,,,> ) )]
   internal class DictionaryQueryOfMutablesImpl<TKey, TValue, TValueQuery, TValueImmutable> : CollectionQueryImpl<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>, DictionaryQueryOfMutables<TKey, TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly DictionaryImmutableQueryImpl<TKey, TValue, TValueQuery, TValueImmutable> _iq;
      private readonly DictionaryQueryOfQueries<TKey, TValueQuery, TValueImmutable> _cmq;

      internal DictionaryQueryOfMutablesImpl( DictionaryImmutableQueryImpl<TKey, TValue, TValueQuery, TValueImmutable> immutableQuery, DictionaryQueryOfQueries<TKey, TValueQuery, TValueImmutable> cmq )
         : base( immutableQuery.Collection )
      {
         this._cmq = ArgumentValidator.ValidateNotNull( "Query of queries", cmq );
         this._iq = ArgumentValidator.ValidateNotNull( "Immutable query", immutableQuery );
      }

      #region DictionaryMutableQuery<TKey,TMutableQuery,TImmutableQuery> Members


      public TValue this[TKey key]
      {
         get
         {
            return this._iq.Collection[key];
         }
      }

      public IEnumerable<TKey> Keys
      {
         get
         {
            return this._iq.Collection.Keys;
         }
      }

      public IEnumerable<TValue> Values
      {
         get
         {
            return this._iq.Collection.Values;
         }
      }

      public bool ContainsKey( TKey key )
      {
         return this._iq.Collection.ContainsKey( key );
      }

      public TValue TryGetValue( TKey key, out Boolean success ) // Boolean TryGetValue( TKey key, out TValue value )
      {
         TValue value;
         success = this._iq.Collection.TryGetValue( key, out value );
         return value;
      }

      #endregion


      #region MutableQuery<DictionaryQuery<TKey,TValueImmutable>> Members

      public DictionaryQuery<TKey, TValueImmutable> IQ
      {
         get
         {
            return this._iq;
         }
      }

      #endregion

      #region QueriesProvider<DictionaryMutableQueryOfQueries<TKey,TValueQuery,TValueImmutable>> Members

      public DictionaryQueryOfQueries<TKey, TValueQuery, TValueImmutable> Queries
      {
         get
         {
            return this._cmq;
         }
      }

      #endregion

      public override Boolean Contains( KeyValuePair<TKey, TValue> item, IEqualityComparer<KeyValuePair<TKey, TValue>> equalityComparer = null )
      {
         return this._iq.Collection.Contains( item );
      }
   }

   [DebuggerDisplay( "Count = {_state.dictionary.Count}" ), DebuggerTypeProxy( typeof( DictionaryQueryOfQueriesDebugView<,,,> ) )]
   internal class DictionaryQueryOfQueriesImpl<TKey, TValue, TValueQuery, TValueImmutable> : MutableQuerySkeleton<DictionaryQuery<TKey, TValueImmutable>>, DictionaryQueryOfQueries<TKey, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {

      private readonly IDictionary<TKey, TValue> _collection;
      internal DictionaryQueryOfQueriesImpl( DictionaryImmutableQueryImpl<TKey, TValue, TValueQuery, TValueImmutable> immutableQuery )
         : base( immutableQuery )
      {
         this._collection = immutableQuery.Collection;
      }

      #region DictionaryQuery<TKey,TValueQuery> Members

      public TValueQuery this[TKey key]
      {
         get
         {
            TValue mutable = this._collection[key];
            TValueQuery result;
            if ( mutable != null )
            {
               result = mutable.MQ;
            }
            else
            {
               result = default( TValueQuery );
            }
            return result;
         }
      }

      public IEnumerable<TKey> Keys
      {
         get
         {
            return this._collection.Keys;
         }
      }

      public IEnumerable<TValueQuery> Values
      {
         get
         {
            return this._collection.Values.Select( v => v == null ? default( TValueQuery ) : v.MQ );
         }
      }

      public bool ContainsKey( TKey key )
      {
         return this._collection.ContainsKey( key );
      }

      public TValueQuery TryGetValue( TKey key, out Boolean success ) // bool TryGetValue( TKey key, out TValueQuery value )
      {
         TValue mutable;
         success = this._collection.TryGetValue( key, out mutable );
         return mutable == null ? default( TValueQuery ) : mutable.MQ;
         //return result;
      }

      #endregion

      #region CollectionQuery<KeyValuePair<TKey,TValueQuery>> Members

      public Int32 Count
      {
         get
         {
            return this._collection.Count;
         }
      }

      public Boolean Contains( KeyValuePair<TKey, TValueQuery> item, IEqualityComparer<KeyValuePair<TKey, TValueQuery>> equalityComparer = null )
      {
         Boolean result = this._collection.ContainsKey( item.Key );
         if ( result )
         {
            TValue value = this._collection[item.Key];
            result = ( value == null && item.Value == null ) || ( value != null && Object.Equals( value.MQ, item.Value ) );
         }
         return result;
      }

      public void CopyTo( KeyValuePair<TKey, TValueQuery>[] array, Int32 arrayOffset )
      {
         KeyValuePair<TKey, TValue>[] arr = new KeyValuePair<TKey, TValue>[Math.Max( array.Length - arrayOffset, 0 )];
         this._collection.CopyTo( arr, 0 );
         for ( Int32 idx = 0; idx < arr.Length; ++idx )
         {
            TValue value = arr[idx].Value;
            array[idx + arrayOffset] = new KeyValuePair<TKey, TValueQuery>( arr[idx].Key, value == null ? default( TValueQuery ) : value.MQ );
         }
      }

      #endregion

      #region IEnumerable<KeyValuePair<TKey,TValueQuery>> Members

      public IEnumerator<KeyValuePair<TKey, TValueQuery>> GetEnumerator()
      {
         return this._collection.Select( kvp => new KeyValuePair<TKey, TValueQuery>( kvp.Key, kvp.Value == null ? default( TValueQuery ) : kvp.Value.MQ ) ).GetEnumerator();
      }

      #endregion

      #region IEnumerable Members

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return this._collection.Select( kvp => new KeyValuePair<TKey, TValueQuery>( kvp.Key, kvp.Value == null ? default( TValueQuery ) : kvp.Value.MQ ) ).GetEnumerator();
      }

      #endregion
   }

   [DebuggerDisplay( "Count = {_state.dictionary.Count}" ), DebuggerTypeProxy( typeof( DictionaryImmutableQueryDebugView<,,,> ) )]
   internal class DictionaryImmutableQueryImpl<TKey, TValue, TValueQuery, TValueImmutable> : DictionaryQuery<TKey, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly IDictionary<TKey, TValue> _collection;

      internal DictionaryImmutableQueryImpl( IDictionary<TKey, TValue> collection )
      {
         this._collection = ArgumentValidator.ValidateNotNull( "Collection", collection );
      }

      #region DictionaryImmutableQuery<TKey,TImmutableQuery> Members

      public TValueImmutable this[TKey key]
      {
         get
         {
            return this._collection[key].GetIQ();
         }
      }

      public IEnumerable<TKey> Keys
      {
         get
         {
            return this._collection.Keys;
         }
      }

      public IEnumerable<TValueImmutable> Values
      {
         get
         {
            return this._collection.Values.Select( v => v.GetIQ() );
         }
      }

      public bool ContainsKey( TKey key )
      {
         return this._collection.ContainsKey( key );
      }

      public TValueImmutable TryGetValue( TKey key, out Boolean success ) // bool TryGetValue( TKey key, out TValueImmutable value )
      {
         TValue mutable;
         success = this._collection.TryGetValue( key, out mutable );
         return mutable.GetIQ();
      }

      #endregion

      #region IEnumerable<KeyValuePair<TKey,TImmutableQuery>> Members

      public IEnumerator<KeyValuePair<TKey, TValueImmutable>> GetEnumerator()
      {
         return this._collection.Select( kvp => new KeyValuePair<TKey, TValueImmutable>( kvp.Key, kvp.Value.GetIQ() ) ).GetEnumerator();
      }

      #endregion

      #region IEnumerable Members

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return this._collection.Select( kvp => new KeyValuePair<TKey, TValueImmutable>( kvp.Key, kvp.Value.GetIQ() ) ).GetEnumerator();
      }

      #endregion

      #region CollectionQuery<KeyValuePair<TKey,TValueImmutable>> Members

      public int Count
      {
         get
         {
            return this._collection.Count;
         }
      }

      public bool Contains( KeyValuePair<TKey, TValueImmutable> item, IEqualityComparer<KeyValuePair<TKey, TValueImmutable>> equalityComparer = null )
      {
         TValue value;
         Boolean result = this._collection.TryGetValue( item.Key, out value );
         if ( result )
         {
            result = ( value == null && item.Value == null ) || ( value != null && value.MQ != null && Object.Equals( value.GetIQ(), item.Value ) );
         }
         return result;
      }

      public void CopyTo( KeyValuePair<TKey, TValueImmutable>[] array, int arrayOffset )
      {
         KeyValuePair<TKey, TValue>[] arr = new KeyValuePair<TKey, TValue>[Math.Max( array.Length - arrayOffset, 0 )];
         this._collection.CopyTo( arr, 0 );
         for ( Int32 idx = 0; idx < arr.Length; ++idx )
         {
            TValue value = arr[idx].Value;
            array[idx + arrayOffset] = new KeyValuePair<TKey, TValueImmutable>( arr[idx].Key, value == null || value.MQ == null ? default( TValueImmutable ) : value.MQ.IQ );
         }
      }

      #endregion

      internal IDictionary<TKey, TValue> Collection
      {
         get
         {
            return this._collection;
         }
      }

   }

   //internal class DictionaryWithRolesState<TKey, TValue, TValueQuery, TValueImmutable> : CollectionState<KeyValuePair<TKey, TValue>>
   //   where TValue : Mutable<TValueQuery, TValueImmutable>
   //   where TValueQuery : MutableQuery<TValueImmutable>
   //{
   //   public readonly IDictionary<TKey, TValue> dictionary;
   //   public readonly CollectionQuery<TKey> keys;
   //   public readonly CollectionQueryOfMutables<CollectionQueryOfQueries<CollectionQuery<TValueImmutable>, TValueQuery, TValueImmutable>, CollectionQuery<TValueImmutable>, TValue, TValueQuery, TValueImmutable> values;

   //   internal DictionaryWithRolesState( IDictionary<TKey, TValue> dictionary )
   //      : base( dictionary )
   //   {
   //      this.dictionary = dictionary;
   //      // Keys
   //      this.keys = new CollectionQueryImpl<TKey>( new CollectionState<TKey>( this.dictionary.Keys ) );

   //      // Values
   //      CollectionState<TValue> valuesState = new CollectionState<TValue>( this.dictionary.Values );
   //      CollectionQuery<TValueImmutable> valuesIQ = new CollectionImmutableQueryImpl<TValue, TValueQuery, TValueImmutable>( valuesState );
   //      this.values = new CollectionQueryOfMutablesImpl<CollectionQueryOfQueries<CollectionQuery<TValueImmutable>, TValueQuery, TValueImmutable>, CollectionQuery<TValueImmutable>, TValue, TValueQuery, TValueImmutable>( valuesIQ, new CollectionQueryOfQueriesImpl<CollectionQuery<TValueImmutable>, TValue, TValueQuery, TValueImmutable>( valuesIQ, valuesState ), valuesState );

   //   }
   //}

   internal class DictionaryWithRolesDebugView<TKey, TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly DictionaryQuery<TKey, TValueImmutable> _dic;

      internal DictionaryWithRolesDebugView( DictionaryWithRolesImpl<TKey, TValue, TValueQuery, TValueImmutable> dic )
      {
         this._dic = dic.MQ.IQ;
      }

      [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
      public KeyValuePair<TKey, TValueImmutable>[] Items
      {
         get
         {
            KeyValuePair<TKey, TValueImmutable>[] array = new KeyValuePair<TKey, TValueImmutable>[this._dic.Count];
            this._dic.CopyTo( array, 0 );
            return array;
         }
      }
   }

   internal class DictionaryQueryOfMutablesDebugView<TKey, TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly DictionaryQuery<TKey, TValueImmutable> _dic;

      internal DictionaryQueryOfMutablesDebugView( DictionaryQueryOfMutablesImpl<TKey, TValue, TValueQuery, TValueImmutable> dic )
      {
         this._dic = dic.IQ;
      }

      [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
      public KeyValuePair<TKey, TValueImmutable>[] Items
      {
         get
         {
            KeyValuePair<TKey, TValueImmutable>[] array = new KeyValuePair<TKey, TValueImmutable>[this._dic.Count];
            this._dic.CopyTo( array, 0 );
            return array;
         }
      }
   }

   internal class DictionaryQueryOfQueriesDebugView<TKey, TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly DictionaryQuery<TKey, TValueImmutable> _dic;

      internal DictionaryQueryOfQueriesDebugView( DictionaryQueryOfQueriesImpl<TKey, TValue, TValueQuery, TValueImmutable> dic )
      {
         this._dic = dic.IQ;
      }

      [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
      public KeyValuePair<TKey, TValueImmutable>[] Items
      {
         get
         {
            KeyValuePair<TKey, TValueImmutable>[] array = new KeyValuePair<TKey, TValueImmutable>[this._dic.Count];
            this._dic.CopyTo( array, 0 );
            return array;
         }
      }
   }

   internal class DictionaryImmutableQueryDebugView<TKey, TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly DictionaryQuery<TKey, TValueImmutable> _dic;

      internal DictionaryImmutableQueryDebugView( DictionaryImmutableQueryImpl<TKey, TValue, TValueQuery, TValueImmutable> dic )
      {
         this._dic = dic;
      }

      [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
      public KeyValuePair<TKey, TValueImmutable>[] Items
      {
         get
         {
            KeyValuePair<TKey, TValueImmutable>[] array = new KeyValuePair<TKey, TValueImmutable>[this._dic.Count];
            this._dic.CopyTo( array, 0 );
            return array;
         }
      }
   }

   internal class DictionaryProxyImpl<TKey, TValue> : CollectionMutableImpl<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, DictionaryQuery<TKey, TValue>, DictionaryProxyQueryImpl<TKey, TValue>>, DictionaryProxy<TKey, TValue>
   {
      internal DictionaryProxyImpl( DictionaryProxyQueryImpl<TKey, TValue> cq )
         : base( cq )
      {
      }

      #region DictionaryMutable<TKey,TValue,DictionaryQuery<TKey,TValue>> Members

      public TValue this[TKey key]
      {
         set
         {
            this.CQImpl.Collection[key] = value;
         }
      }

      public void Add( TKey key, TValue value )
      {
         this.CQImpl.Collection.Add( key, value );
      }

      public bool Remove( TKey key )
      {
         return this.CQImpl.Collection.Remove( key );
      }

      #endregion

      #region Mutable<DictionaryProxyQuery<TKey,TValue>,DictionaryQuery<TKey,TValue>> Members

      public DictionaryProxyQuery<TKey, TValue> MQ
      {
         get
         {
            return this.CQImpl;
         }
      }

      #endregion
   }

   internal class DictionaryQueryImpl<TKey, TValue> : CollectionQueryImpl<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>, DictionaryQuery<TKey, TValue>
   {

      internal DictionaryQueryImpl( IDictionary<TKey, TValue> collection )
         : base( collection )
      {
      }

      #region DictionaryQuery<TKey,TValue> Members

      public TValue this[TKey key]
      {
         get
         {
            return this.Collection[key];
         }
      }

      public IEnumerable<TKey> Keys
      {
         get
         {
            return this.Collection.Keys;
         }
      }

      public IEnumerable<TValue> Values
      {
         get
         {
            return this.Collection.Values;
         }
      }

      public bool ContainsKey( TKey key )
      {
         return this.Collection.ContainsKey( key );
      }

      public TValue TryGetValue( TKey key, out Boolean success ) // bool TryGetValue( TKey key, out TValue value )
      {
         TValue value;
         success = this.Collection.TryGetValue( key, out value );
         return value;
      }

      #endregion

      public override Boolean Contains( KeyValuePair<TKey, TValue> item, IEqualityComparer<KeyValuePair<TKey, TValue>> equalityComparer = null )
      {
         return this.Collection.Contains( item );
      }
   }

   internal class DictionaryProxyQueryImpl<TKey, TValue> : DictionaryQueryImpl<TKey, TValue>, DictionaryProxyQuery<TKey, TValue>
   {

      internal DictionaryProxyQueryImpl( IDictionary<TKey, TValue> collection )
         : base( collection )
      {
      }

      #region MutableQuery<DictionaryQuery<TKey,TValue>> Members

      public DictionaryQuery<TKey, TValue> IQ
      {
         get
         {
            return this;
         }
      }

      #endregion
   }
}