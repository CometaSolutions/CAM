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
using UtilPack.CollectionsWithRoles;
using UtilPack;

namespace UtilPack.CollectionsWithRoles.Implementation
{
   [DebuggerDisplay( "Count = {CQImpl.Count}" ), DebuggerTypeProxy( typeof( CollectionMutableDebugView<,,,> ) )]
   internal class CollectionMutableImpl<TCollection, TValue, TCollectionQuery, TCollectionQueryImpl> : CollectionMutable<TValue, TCollectionQuery>
      where TCollection : class, ICollection<TValue>
      where TCollectionQuery : class, CollectionQuery<TValue>
      where TCollectionQueryImpl : CollectionQueryImpl<TCollection, TValue>, TCollectionQuery
   {

      internal CollectionMutableImpl( TCollectionQueryImpl cq )
      {
         this.CQImpl = ArgumentValidator.ValidateNotNull( "Collection query", cq );
      }

      #region CollectionMutable<TValue> Members

      public virtual void Add( TValue item )
      {
         this.CQImpl.Collection.Add( item );
      }

      public void AddRange( IEnumerable<TValue> items )
      {
         ArgumentValidator.ValidateNotNull( "Items", items );
         var list = this.CQImpl.Collection as List<TValue>;
         if ( list != null )
         {
            list.AddRange( items );
         }
         else
         {
            foreach ( TValue item in items )
            {
               this.CQImpl.Collection.Add( item );
            }
         }
      }

      public Boolean Remove( TValue item )
      {
         return this.CQImpl.Collection.Remove( item );
      }

      public void Clear()
      {
         this.CQImpl.Collection.Clear();
      }

      public TCollectionQuery CQ
      {
         get
         {
            return this.CQImpl;
         }
      }

      public CollectionAdditionOnly<TValue> AO
      {
         get
         {
            return this;
         }
      }

      #endregion

      protected TCollectionQueryImpl CQImpl { get; }
   }

   //[DebuggerDisplay( "Count = {_mutable.CQ.Count}" ), DebuggerTypeProxy( typeof( CollectionAdditionOnlyDebugView<,> ) )]
   //internal class CollectionAdditionOnlyImpl<TValue, TCollectionQuery> : CollectionAdditionOnly<TValue>
   //   where TCollectionQuery : CollectionQuery<TValue>
   //{
   //   private readonly CollectionMutable<TValue, TCollectionQuery> _mutable;

   //   internal CollectionAdditionOnlyImpl( CollectionMutable<TValue, TCollectionQuery> mutable )
   //   {
   //      ArgumentValidator.ValidateNotNull( "Mutable collection", mutable );

   //      this._mutable = mutable;
   //   }

   //   internal CollectionMutable<TValue, TCollectionQuery> Mutable
   //   {
   //      get
   //      {
   //         return this._mutable;
   //      }
   //   }

   //   #region CollectionAdditionOnly<TValue> Members

   //   public void Add( TValue item )
   //   {
   //      this._mutable.Add( item );
   //   }

   //   public void AddRange( IEnumerable<TValue> items )
   //   {
   //      this._mutable.AddRange( items );
   //   }

   //   #endregion
   //}

   [DebuggerDisplay( "Count = {_state.collection.Count}" ), DebuggerTypeProxy( typeof( CollectionQueryDebugView<,> ) )]
   internal class CollectionQueryImpl<TCollection, TValue> : CollectionQuery<TValue>
      where TCollection : class, ICollection<TValue>
   {

      internal CollectionQueryImpl( TCollection collection )
      {
         this.Collection = ArgumentValidator.ValidateNotNull( "Collection", collection );
      }

      #region CollectionQuery<TValue> Members

      public Int32 Count
      {
         get
         {
            return this.Collection.Count;
         }
      }

      public virtual Boolean Contains( TValue item, IEqualityComparer<TValue> equalityComparer = null )
      {
         return equalityComparer == null ?
            this.Collection.Contains( item ) :
            this.Collection.Any( m => equalityComparer.Equals( m, item ) );
      }

      public void CopyTo( TValue[] array, Int32 arrayOffset )
      {
         this.Collection.CopyTo( array, arrayOffset );
      }

      #endregion

      #region IEnumerable<TValue> Members

      public IEnumerator<TValue> GetEnumerator()
      {
         return this.Collection.GetEnumerator();
      }

      #endregion

      #region IEnumerable Members

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return this.Collection.GetEnumerator();
      }

      #endregion

      internal TCollection Collection { get; }

   }

   [DebuggerDisplay( "Count = {_state.collection.Count}" ), DebuggerTypeProxy( typeof( CollectionWithRolesDebugView<,,,,,,,> ) )]
   internal class CollectionWithRolesImpl<TCollection, TMutableQueryRole, TMutableQueryRoleImpl, TQueriesQueryRole, TImmutableQueryRole, TValue, TValueQuery, TValueImmutable> : CollectionMutableImpl<TCollection, TValue, TMutableQueryRole, TMutableQueryRoleImpl>, CollectionWithRoles<TMutableQueryRole, TQueriesQueryRole, TImmutableQueryRole, TValue, TValueQuery, TValueImmutable>
      where TCollection : class, ICollection<TValue>
      where TMutableQueryRole : class, CollectionQueryOfMutables<TQueriesQueryRole, TImmutableQueryRole, TValue, TValueQuery, TValueImmutable>
      where TMutableQueryRoleImpl : CollectionQueryImpl<TCollection, TValue>, TMutableQueryRole
      where TQueriesQueryRole : class, CollectionQueryOfQueries<TImmutableQueryRole, TValueQuery, TValueImmutable>
      where TImmutableQueryRole : CollectionQuery<TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      internal CollectionWithRolesImpl( TMutableQueryRoleImpl mutableQuery )
         : base( mutableQuery )
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

   [DebuggerDisplay( "Count = {_state.collection.Count}" ), DebuggerTypeProxy( typeof( CollectionQueryOfMutablesDebugView<,,,,,,> ) )]
   internal class CollectionQueryOfMutablesImpl<TCollection, TQueryOfQueriesRole, TImmutableQueryRole, TImmutableQueryRoleImpl, TValue, TValueQuery, TValueImmutable> : CollectionQueryImpl<TCollection, TValue>, CollectionQueryOfMutables<TQueryOfQueriesRole, TImmutableQueryRole, TValue, TValueQuery, TValueImmutable>
      where TCollection : class, ICollection<TValue>
      where TQueryOfQueriesRole : class, CollectionQueryOfQueries<TImmutableQueryRole, TValueQuery, TValueImmutable>
      where TImmutableQueryRole : class, CollectionQuery<TValueImmutable>
      where TImmutableQueryRoleImpl : CollectionImmutableQueryImpl<TCollection, TValue, TValueQuery, TValueImmutable>, TImmutableQueryRole
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly TImmutableQueryRoleImpl _iq;
      private readonly TQueryOfQueriesRole _cmq;

      internal CollectionQueryOfMutablesImpl( TImmutableQueryRoleImpl immutableQuery, TQueryOfQueriesRole cmq )
         : base( immutableQuery.Collection )
      {
         this._cmq = ArgumentValidator.ValidateNotNull( "Collection query of queries", cmq );
         this._iq = ArgumentValidator.ValidateNotNull( "Immutable query", immutableQuery );
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

   [DebuggerDisplay( "Count = {_state.collection.Count}" ), DebuggerTypeProxy( typeof( CollectionQueryOfQueriesDebugView<,,,,,> ) )]
   internal class CollectionQueryOfQueriesImpl<TCollection, TImmutableQueryRole, TImmutableQueryRoleImpl, TValue, TValueQuery, TValueImmutable> : MutableQuerySkeleton<TImmutableQueryRole>, CollectionQueryOfQueries<TImmutableQueryRole, TValueQuery, TValueImmutable>
      where TCollection : class, ICollection<TValue>
      where TImmutableQueryRole : class, CollectionQuery<TValueImmutable>
      where TImmutableQueryRoleImpl : CollectionImmutableQueryImpl<TCollection, TValue, TValueQuery, TValueImmutable>, TImmutableQueryRole
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      protected readonly TCollection _collection;

      internal CollectionQueryOfQueriesImpl( TImmutableQueryRoleImpl immutableQuery )
         : base( immutableQuery )
      {
         this._collection = immutableQuery.Collection;
      }

      #region CollectionMutableQueryOfQueries<TImmutableQueryRole,TValueQuery,TValueImmutable> Members

      public Int32 Count
      {
         get
         {
            return this._collection.Count;
         }
      }

      public virtual Boolean Contains( TValueQuery item, IEqualityComparer<TValueQuery> equalityComparer = null )
      {
         equalityComparer = equalityComparer ?? EqualityComparer<TValueQuery>.Default;
         return this._collection.Any( m => ( m == null && item == null ) || ( m != null && equalityComparer.Equals( m.MQ, item ) ) );
      }

      public void CopyTo( TValueQuery[] array, int arrayOffset )
      {
         TValue[] arr = new TValue[Math.Max( array.Length - arrayOffset, 0 )];
         this._collection.CopyTo( arr, 0 );
         for ( Int32 idx = 0; idx < arr.Length; ++idx )
         {
            array[idx + arrayOffset] = arr[idx] == null ? default( TValueQuery ) : arr[idx].MQ;
         }
      }

      #endregion

      #region IEnumerable<TValueQuery> Members

      public IEnumerator<TValueQuery> GetEnumerator()
      {
         return this._collection.Select( item => item == null ? default( TValueQuery ) : item.MQ ).GetEnumerator();
      }

      #endregion

      #region IEnumerable Members

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return this._collection.Select( item => item == null ? default( TValueQuery ) : item.MQ ).GetEnumerator();
      }

      #endregion
   }

   [DebuggerDisplay( "Count = {_state.collection.Count}" ), DebuggerTypeProxy( typeof( CollectionImmutableQueryDebugView<,,,> ) )]
   internal class CollectionImmutableQueryImpl<TCollection, TValue, TValueQuery, TValueImmutable> : CollectionQuery<TValueImmutable>
      where TCollection : class, ICollection<TValue>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {

      internal CollectionImmutableQueryImpl( TCollection collection )
      {
         this.Collection = ArgumentValidator.ValidateNotNull( "Collection", collection );
      }

      #region CollectionImmutableQuery<TImmutable> Members

      public Int32 Count
      {
         get
         {
            return this.Collection.Count;
         }
      }

      public virtual Boolean Contains( TValueImmutable item, IEqualityComparer<TValueImmutable> equalityComparer = null )
      {
         equalityComparer = equalityComparer ?? EqualityComparer<TValueImmutable>.Default;
         return this.Collection.Any( m => ( m == null && item == null ) || ( m != null && equalityComparer.Equals( m.MQ.IQ, item ) ) );
      }

      public void CopyTo( TValueImmutable[] array, int arrayOffset )
      {
         TValue[] arr = new TValue[Math.Max( array.Length - arrayOffset, 0 )];
         this.Collection.CopyTo( arr, 0 );
         for ( Int32 idx = 0; idx < arr.Length; ++idx )
         {
            array[idx + arrayOffset] = arr[idx] == null || arr[idx].MQ == null ? default( TValueImmutable ) : arr[idx].MQ.IQ;
         }
      }

      #endregion

      #region IEnumerable<TImmutable> Members

      public virtual IEnumerator<TValueImmutable> GetEnumerator()
      {
         return this.Collection.Select( item => item == null || item.MQ == null ? default( TValueImmutable ) : item.MQ.IQ ).GetEnumerator();
      }

      #endregion

      #region IEnumerable Members

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return this.Collection.Select( item => item == null || item.MQ == null ? default( TValueImmutable ) : item.MQ.IQ ).GetEnumerator();
      }

      #endregion

      internal TCollection Collection { get; }

   }

   internal sealed class CollectionImmutableQueryDebugView<TCollection, TValue, TValueQuery, TValueImmutable>
      where TCollection : class, ICollection<TValue>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly CollectionQuery<TValueImmutable> _collection;

      internal CollectionImmutableQueryDebugView( CollectionImmutableQueryImpl<TCollection, TValue, TValueQuery, TValueImmutable> collection )
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

   internal sealed class CollectionMutableDebugView<TCollection, TValue, TCollectionQuery, TCollectionQueryImpl>
      where TCollection : class, ICollection<TValue>
      where TCollectionQuery : class, CollectionQuery<TValue>
      where TCollectionQueryImpl : CollectionQueryImpl<TCollection, TValue>, TCollectionQuery
   {
      private readonly CollectionQuery<TValue> _collection;

      internal CollectionMutableDebugView( CollectionMutableImpl<TCollection, TValue, TCollectionQuery, TCollectionQueryImpl> collection )
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

   //internal sealed class CollectionAdditionOnlyDebugView<TValue, TCollectionQuery>
   //   where TCollectionQuery : CollectionQuery<TValue>
   //{
   //   private readonly CollectionQuery<TValue> _collection;

   //   internal CollectionAdditionOnlyDebugView( CollectionAdditionOnlyImpl<TValue, TCollectionQuery> collection )
   //   {
   //      this._collection = collection.Mutable.CQ;
   //   }

   //   [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
   //   public TValue[] Items
   //   {
   //      get
   //      {
   //         TValue[] array = new TValue[this._collection.Count];
   //         this._collection.CopyTo( array, 0 );
   //         return array;
   //      }
   //   }
   //}

   internal sealed class CollectionQueryDebugView<TCollection, TValue>
      where TCollection : class, ICollection<TValue>
   {
      private readonly CollectionQuery<TValue> _collection;

      internal CollectionQueryDebugView( CollectionQueryImpl<TCollection, TValue> collection )
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

   internal sealed class CollectionWithRolesDebugView<TCollection, TMutableQueryRole, TMutableQueryRoleImpl, TQueriesQueryRole, TImmutableQueryRole, TValue, TValueQuery, TValueImmutable>
      where TCollection : class, ICollection<TValue>
      where TMutableQueryRole : class, CollectionQueryOfMutables<TQueriesQueryRole, TImmutableQueryRole, TValue, TValueQuery, TValueImmutable>
      where TMutableQueryRoleImpl : CollectionQueryImpl<TCollection, TValue>, TMutableQueryRole
      where TQueriesQueryRole : class, CollectionQueryOfQueries<TImmutableQueryRole, TValueQuery, TValueImmutable>
      where TImmutableQueryRole : CollectionQuery<TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly CollectionQuery<TValueImmutable> _collection;

      internal CollectionWithRolesDebugView( CollectionWithRolesImpl<TCollection, TMutableQueryRole, TMutableQueryRoleImpl, TQueriesQueryRole, TImmutableQueryRole, TValue, TValueQuery, TValueImmutable> collection )
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

   internal sealed class CollectionQueryOfMutablesDebugView<TCollection, TQueryOfQueriesRole, TImmutableQueryRole, TImmutableQueryRoleImpl, TValue, TValueQuery, TValueImmutable>
      where TCollection : class, ICollection<TValue>
      where TQueryOfQueriesRole : class, CollectionQueryOfQueries<TImmutableQueryRole, TValueQuery, TValueImmutable>
      where TImmutableQueryRole : class, CollectionQuery<TValueImmutable>
      where TImmutableQueryRoleImpl : CollectionImmutableQueryImpl<TCollection, TValue, TValueQuery, TValueImmutable>, TImmutableQueryRole
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly CollectionQuery<TValueImmutable> _collection;

      internal CollectionQueryOfMutablesDebugView( CollectionQueryOfMutablesImpl<TCollection, TQueryOfQueriesRole, TImmutableQueryRole, TImmutableQueryRoleImpl, TValue, TValueQuery, TValueImmutable> collection )
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

   internal sealed class CollectionQueryOfQueriesDebugView<TCollection, TImmutableQueryRole, TImmutableQueryRoleImpl, TValue, TValueQuery, TValueImmutable>
      where TCollection : class, ICollection<TValue>
      where TImmutableQueryRole : class, CollectionQuery<TValueImmutable>
      where TImmutableQueryRoleImpl : CollectionImmutableQueryImpl<TCollection, TValue, TValueQuery, TValueImmutable>, TImmutableQueryRole
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly CollectionQuery<TValueImmutable> _collection;

      internal CollectionQueryOfQueriesDebugView( CollectionQueryOfQueriesImpl<TCollection, TImmutableQueryRole, TImmutableQueryRoleImpl, TValue, TValueQuery, TValueImmutable> collection )
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
