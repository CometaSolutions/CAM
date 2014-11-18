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
using CollectionsWithRoles.API;
using CommonUtils;
using System.Diagnostics;

namespace CollectionsWithRoles.Implementation
{

   [DebuggerDisplay( "Count = {_state.array.Length}" ), DebuggerTypeProxy( typeof( ArrayMutableDebugView<,> ) )]
   internal abstract class ArrayMutableImpl<TValue, TArrayQuery> : ArrayMutable<TValue, TArrayQuery>
      where TArrayQuery : class, ArrayQuery<TValue>
   {
      private readonly TArrayQuery _cq;
      protected readonly ArrayState<TValue> _state;

      protected ArrayMutableImpl( TArrayQuery cq, ArrayState<TValue> state )
      {
         ArgumentValidator.ValidateNotNull( "Array query", cq );
         ArgumentValidator.ValidateNotNull( "State", state );

         this._cq = cq;
         this._state = state;
      }

      public TValue this[Int32 index]
      {
         set
         {
            this._state.array[index] = value;
         }
      }

      public TArrayQuery CQ
      {
         get
         {
            return this._cq;
         }
      }
   }

   [DebuggerDisplay( "Count = {_state.array.Length}" ), DebuggerTypeProxy( typeof( ArrayQueryDebugView<> ) )]
   internal abstract class ArrayQueryImpl<TValue> : ArrayQuery<TValue>
   {
      private readonly ArrayState<TValue> _state;

      protected ArrayQueryImpl( ArrayState<TValue> state )
      {
         ArgumentValidator.ValidateNotNull( "State", state );

         this._state = state;
      }

      public TValue this[Int32 index]
      {
         get
         {
            return this._state.array[index];
         }
      }

      public Int32 Count
      {
         get
         {
            return this._state.array.Length;
         }
      }

      public IEnumerator<TValue> GetEnumerator()
      {
         // If just cast this.GetEnumerator() to IEnumerator<TValue>, will get exception:
         // Unable to cast object of type 'SZArrayEnumerator' to type 'System.Collections.Generic.IEnumerator`1[System.Int32]'
         return ( (IEnumerable<TValue>) this._state.array ).GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return this._state.array.GetEnumerator();
      }
   }

   internal class ArrayState<TValue>
   {
      internal readonly TValue[] array;

      internal ArrayState( TValue[] anArray )
      {
         this.array = anArray ?? Empty<TValue>.Array;
      }
   }

   [DebuggerDisplay( "Count = {_state.array.Length}" ), DebuggerTypeProxy( typeof( ArrayWithRolesDebugView<,,> ) )]
   internal class ArrayWithRolesImpl<TValue, TValueQuery, TValueImmutable> : ArrayMutableImpl<TValue, ArrayQueryOfMutables<TValue, TValueQuery, TValueImmutable>>, ArrayWithRoles<TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      internal ArrayWithRolesImpl( ArrayQueryOfMutables<TValue, TValueQuery, TValueImmutable> mq, ArrayState<TValue> state )
         : base( mq, state )
      {

      }

      public ArrayQueryOfMutables<TValue, TValueQuery, TValueImmutable> MQ
      {
         get
         {
            return this.CQ;
         }
      }

      public TValue[] Array
      {
         get
         {
            return this._state.array;
         }
      }
   }

   [DebuggerDisplay( "Count = {_state.array.Length}" ), DebuggerTypeProxy( typeof( ArrayWithRolesDebugView<,,> ) )]
   internal class ArrayQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable> : ArrayQueryImpl<TValue>, ArrayQueryOfMutables<TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly ArrayQuery<TValueImmutable> _iq;
      private readonly ArrayQueryOfQueries<TValueQuery, TValueImmutable> _cmq;

      internal ArrayQueryOfMutablesImpl( ArrayQuery<TValueImmutable> immutableQuery, ArrayQueryOfQueries<TValueQuery, TValueImmutable> cmq, ArrayState<TValue> state )
         : base( state )
      {
         ArgumentValidator.ValidateNotNull( "Collection query of queries", cmq );
         ArgumentValidator.ValidateNotNull( "Immutable query", immutableQuery );

         this._iq = immutableQuery;
         this._cmq = cmq;
      }

      public ArrayQuery<TValueImmutable> IQ
      {
         get
         {
            return this._iq;
         }
      }


      public ArrayQueryOfQueries<TValueQuery, TValueImmutable> Queries
      {
         get
         {
            return this._cmq;
         }
      }
   }

   [DebuggerDisplay( "Count = {_state.array.Length}" ), DebuggerTypeProxy( typeof( ArrayWithRolesDebugView<,,> ) )]
   internal class ArrayQueryOfQueriesImpl<TValue, TValueQuery, TValueImmutable> : MutableQuerySkeleton<ArrayQuery<TValueImmutable>>, ArrayQueryOfQueries<TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly ArrayState<TValue> _state;

      internal ArrayQueryOfQueriesImpl( ArrayQuery<TValueImmutable> iq, ArrayState<TValue> state )
         : base( iq )
      {
         ArgumentValidator.ValidateNotNull( "State", state );
         this._state = state;
      }

      public TValueQuery this[Int32 index]
      {
         get
         {
            var item = this._state.array[index];
            return item == null ? default( TValueQuery ) : item.MQ;
         }
      }

      public Int32 Count
      {
         get
         {
            return this._state.array.Length;
         }
      }

      public IEnumerator<TValueQuery> GetEnumerator()
      {
         return this._state.array.Select( item => item == null ? default( TValueQuery ) : item.MQ ).GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return this._state.array.Select( item => item == null ? default( TValueQuery ) : item.MQ ).GetEnumerator();
      }
   }

   [DebuggerDisplay( "Count = {_state.array.Length}" ), DebuggerTypeProxy( typeof( ArrayWithRolesDebugView<,,> ) )]
   internal class ArrayImmutableQueryImpl<TValue, TValueQuery, TValueImmutable> : ArrayQuery<TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly ArrayState<TValue> _state;

      internal ArrayImmutableQueryImpl( ArrayState<TValue> state )
      {
         ArgumentValidator.ValidateNotNull( "State", state );
         this._state = state;
      }

      public TValueImmutable this[Int32 index]
      {
         get
         {
            var item = this._state.array[index];
            return item == null || item.MQ == null ? default( TValueImmutable ) : item.MQ.IQ;
         }
      }

      public Int32 Count
      {
         get
         {
            return this._state.array.Length;
         }
      }

      public IEnumerator<TValueImmutable> GetEnumerator()
      {
         return this._state.array.Select( item => item == null || item.MQ == null ? default( TValueImmutable ) : item.MQ.IQ ).GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return this._state.array.Select( item => item == null || item.MQ == null ? default( TValueImmutable ) : item.MQ.IQ ).GetEnumerator();
      }
   }

   internal class ArrayProxyImpl<TValue> : ArrayMutableImpl<TValue, ArrayQuery<TValue>>, ArrayProxy<TValue>
   {
      internal ArrayProxyImpl( ArrayQuery<TValue> cq, ArrayState<TValue> state )
         : base( cq, state )
      {

      }

      public ArrayProxyQuery<TValue> MQ
      {
         get
         {
            return (ArrayProxyQuery<TValue>) this.CQ;
         }
      }

      public TValue[] Array
      {
         get
         {
            return this._state.array;
         }
      }
   }

   internal class ArrayProxyQueryImpl<TValue> : ArrayQueryImpl<TValue>, ArrayProxyQuery<TValue>
   {
      internal ArrayProxyQueryImpl( ArrayState<TValue> state )
         : base( state )
      {
      }

      public ArrayQuery<TValue> IQ
      {
         get
         {
            return this;
         }
      }
   }

   internal sealed class ArrayMutableDebugView<TValue, TArrayQuery>
      where TArrayQuery : class, ArrayQuery<TValue>
   {
      private readonly ArrayQuery<TValue> _array;

      internal ArrayMutableDebugView( ArrayMutableImpl<TValue, TArrayQuery> array )
      {
         this._array = array.CQ;
      }


      [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
      public TValue[] Items
      {
         get
         {
            return this._array.ToArrayCWR();
         }
      }
   }

   internal sealed class ArrayQueryDebugView<TValue>
   {
      private readonly ArrayQuery<TValue> _array;

      internal ArrayQueryDebugView( ArrayQuery<TValue> array )
      {
         this._array = array;
      }

      [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
      public TValue[] Items
      {
         get
         {
            return this._array.ToArrayCWR();
         }
      }
   }

   internal sealed class ArrayWithRolesDebugView<TValue, TValueQuery, TValueImmutable>
      where TValue : Mutable<TValueQuery, TValueImmutable>
      where TValueQuery : MutableQuery<TValueImmutable>
   {
      private readonly ArrayQuery<TValueImmutable> _array;

      internal ArrayWithRolesDebugView( ArrayWithRolesImpl<TValue, TValueQuery, TValueImmutable> array )
      {
         this._array = array.MQ.IQ;
      }

      internal ArrayWithRolesDebugView( ArrayQueryOfMutablesImpl<TValue, TValueQuery, TValueImmutable> array )
      {
         this._array = array.IQ;
      }

      internal ArrayWithRolesDebugView( ArrayQueryOfQueriesImpl<TValue, TValueQuery, TValueImmutable> array )
      {
         this._array = array.IQ;
      }

      internal ArrayWithRolesDebugView( ArrayImmutableQueryImpl<TValue, TValueQuery, TValueImmutable> array )
      {
         this._array = array;
      }

      [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
      public TValueImmutable[] Items
      {
         get
         {
            return this._array.ToArrayCWR();
         }
      }
   }

}
