﻿/*
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

namespace CommonUtils
{
   /// <summary>
   /// This is delegate used in equality comparisons when getting hash code is not required.
   /// </summary>
   /// <typeparam name="T">The type of items to compare.</typeparam>
   /// <param name="x">The first item.</param>
   /// <param name="y">The second item.</param>
   /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> are considered to be equal; <c>false</c> otherwise.</returns>
   public delegate Boolean Equality<T>( T x, T y );

   /// <summary>
   /// This is delegate used in getting hash code, when equality is not required.
   /// </summary>
   /// <typeparam name="T">The type of items to get hash code of.</typeparam>
   /// <param name="obj">The item.</param>
   /// <returns>The hashcode for <paramref name="obj"/>.</returns>
   public delegate Int32 HashCode<T>( T obj );

   /// <summary>
   /// This class provides content-based equality comparing for sequences.
   /// </summary>
   /// <typeparam name="T">The type of the sequence.</typeparam>
   /// <typeparam name="U">The type of the elements of the sequence.</typeparam>
   /// <seealso cref="ArrayEqualityComparer{T}"/>
   public sealed class SequenceEqualityComparer<T, U> : IEqualityComparer<T>, System.Collections.IEqualityComparer
      where T : IEnumerable<U>
   {
      private static readonly IEqualityComparer<T> INSTANCE = new SequenceEqualityComparer<T, U>();

      /// <summary>
      /// Returns the equality comparer for sequence <typeparamref name="T"/> which will use default equality comparer for the elements of the sequence.
      /// </summary>
      /// <value>The equality comparer for sequence <typeparamref name="T"/> which will use default equality comparer for the elements of the sequence.</value>
      /// <remarks>The return value can be casted to <see cref="System.Collections.IEqualityComparer"/>.</remarks>
      public static IEqualityComparer<T> DefaultSequenceEqualityComparer
      {
         get
         {
            return INSTANCE;
         }
      }

      /// <summary>
      /// Creates a new equality comparer for sequence <typeparamref name="T"/> which will use the given equality comparer for the elements of the sequence.
      /// </summary>
      /// <param name="itemComparer">The equality comparer to use when comparing elements of the sequence.</param>
      /// <returns>A new equality comparer for sequence <typeparamref name="T"/> which will use the given equality comparer for the elements of the sequence.</returns>
      /// <remarks>The return value can be casted to <see cref="System.Collections.IEqualityComparer"/>.</remarks>
      public static IEqualityComparer<T> NewSequenceEqualsComparer( IEqualityComparer<U> itemComparer )
      {
         return new SequenceEqualityComparer<T, U>( itemComparer );
      }

      private readonly IEqualityComparer<U> _itemComparer;

      private SequenceEqualityComparer()
         : this( null )
      {

      }

      private SequenceEqualityComparer( IEqualityComparer<U> itemComparer )
      {
         this._itemComparer = itemComparer ?? EqualityComparer<U>.Default;
      }

      #region IEqualityComparer<T> Members

      Boolean IEqualityComparer<T>.Equals( T x, T y )
      {
         return Object.ReferenceEquals( x, y ) || ( x != null && y != null && x.SequenceEqual( y, this._itemComparer ) );
      }

      Int32 IEqualityComparer<T>.GetHashCode( T obj )
      {
         // LINQ version of Jon Skeet's answer on http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
         return obj == null ? 0 : obj.Aggregate( 17, ( cur, item ) =>
         {
            unchecked
            {
               return cur * 23 + this._itemComparer.GetHashCode( item );
            }
         } );
      }

      #endregion

      Boolean System.Collections.IEqualityComparer.Equals( Object x, Object y )
      {
         return ( (IEqualityComparer<T>) this ).Equals( (T) x, (T) y );
      }

      Int32 System.Collections.IEqualityComparer.GetHashCode( Object obj )
      {
         return ( (IEqualityComparer<T>) this ).GetHashCode( (T) obj );
      }
   }

   /// <summary>
   /// This is abstract class for comparing sequences as sets, i.e. order of elements does not matter.
   /// </summary>
   /// <typeparam name="T">The type of set.</typeparam>
   /// <typeparam name="U">The type of elements in set.</typeparam>
   public abstract class AbstractSetEqualityComparer<T, U> : IEqualityComparer<T>, System.Collections.IEqualityComparer
      where T : IEnumerable<U>
   {

      Boolean IEqualityComparer<T>.Equals( T x, T y )
      {
         return Object.ReferenceEquals( x, y )
            || ( x != null
               && y != null
               && this.CheckSize( x, y )
               && x.All( item => this.Contains( y, item ) )
               );
      }

      Int32 IEqualityComparer<T>.GetHashCode( T obj )
      {
         return SequenceEqualityComparer<T, U>.DefaultSequenceEqualityComparer.GetHashCode( obj );
      }

      Boolean System.Collections.IEqualityComparer.Equals( Object x, Object y )
      {
         return ( (IEqualityComparer<T>) this ).Equals( (T) x, (T) y );
      }

      Int32 System.Collections.IEqualityComparer.GetHashCode( Object obj )
      {
         return ( (IEqualityComparer<T>) this ).GetHashCode( (T) obj );
      }

      /// <summary>
      /// The subclasses should override this method for checking whether the given set contains given item.
      /// </summary>
      /// <param name="set">The set. Will never be <c>null</c>.</param>
      /// <param name="item">The item to check.</param>
      /// <returns><c>true</c> if <paramref name="set"/> contains <paramref name="item"/>; <c>false</c> otherwise.</returns>
      protected abstract Boolean Contains( T set, U item );

      /// <summary>
      /// The subclasses should override this method for checking whether two sets are of equal size.
      /// </summary>
      /// <param name="set1">The first set. Will never be <c>null</c>.</param>
      /// <param name="set2">The second set. Will never be <c>null</c>.</param>
      /// <returns><c>true</c> if <paramref name="set1"/> is of same size as <paramref name="set2"/>; <c>false</c> otherwise.</returns>
      protected abstract Boolean CheckSize( T set1, T set2 );

   }

   /// <summary>
   /// This class provides a way to compare dictionaries by-value, i.e. checking that two dictionaries are of same size and that all items in one dictionary are same as in other dictionary.
   /// </summary>
   /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
   /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
   public sealed class DictionaryEqualityComparer<TKey, TValue> : AbstractSetEqualityComparer<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>
   {
      private static readonly IEqualityComparer<IDictionary<TKey, TValue>> INSTANCE = new DictionaryEqualityComparer<TKey, TValue>();

      /// <summary>
      /// Returns the equality comparer which will compare dictionaries by-value using default equality compare for type <typeparamref name="TValue"/>.
      /// </summary>
      /// <value>The equality comparer which will compare dictionaries by-value using default equality compare for type <typeparamref name="TValue"/>.</value>
      public static IEqualityComparer<IDictionary<TKey, TValue>> DefaultEqualityComparer
      {
         get
         {
            return INSTANCE;
         }
      }

      /// <summary>
      /// Creates a new equality comparer for comparing dictionaries by-value with specified equality comparer for value type.
      /// </summary>
      /// <param name="valueComparer">The equality comparer for value type. If <c>null</c>, a default comparer will be used.</param>
      /// <returns>A new equality comparer for comparing dictionaries by-value with specified equality comparer for value type.</returns>
      public static IEqualityComparer<IDictionary<TKey, TValue>> NewDictionaryEqualityComparer( IEqualityComparer<TValue> valueComparer )
      {
         return new DictionaryEqualityComparer<TKey, TValue>( valueComparer );
      }

      private readonly IEqualityComparer<TValue> _valueComparer;

      private DictionaryEqualityComparer()
         : this( null )
      {

      }

      private DictionaryEqualityComparer( IEqualityComparer<TValue> itemComparer )
      {
         this._valueComparer = itemComparer ?? EqualityComparer<TValue>.Default;
      }

      /// <inheritdoc />
      protected override Boolean Contains( IDictionary<TKey, TValue> collection, KeyValuePair<TKey, TValue> key )
      {
         TValue val;
         return collection.TryGetValue( key.Key, out val ) && this._valueComparer.Equals( key.Value, val );
      }

      /// <inheritdoc />
      protected override Boolean CheckSize( IDictionary<TKey, TValue> set1, IDictionary<TKey, TValue> set2 )
      {
         return set1.Count == set2.Count;
      }
   }

   /// <summary>
   /// This class provides a way to compare sets by-value, i.e. checking that two sets are of same size and that one set contains all elements as other set.
   /// </summary>
   /// <typeparam name="TValue">The type of elements in the set.</typeparam>
   public sealed class SetEqualityComparer<TValue> : AbstractSetEqualityComparer<ISet<TValue>, TValue>
   {
      private static readonly IEqualityComparer<ISet<TValue>> INSTANCE = new SetEqualityComparer<TValue>();

      /// <summary>
      /// Returns the equality comparer for comparing sets by-value using the equality comparer specified for the set.
      /// </summary>
      /// <value>The equality comparer for comparing sets by-value using the equality comparer specified for the set.</value>
      public static IEqualityComparer<ISet<TValue>> DefaultEqualityComparer
      {
         get
         {
            return INSTANCE;
         }
      }

      /// <inheritdoc />
      protected override bool CheckSize( ISet<TValue> set1, ISet<TValue> set2 )
      {
         return set1.Count == set2.Count;
      }

      /// <inheritdoc />
      protected override Boolean Contains( ISet<TValue> collection, TValue key )
      {
         return collection.Contains( key );
      }
   }

   /// <summary>
   /// This class provides content-based equality comparing for arrays. It is still possible to use <see cref="SequenceEqualityComparer{T,U}"/> for arrays and have same result, but this class will be faster.
   /// </summary>
   /// <typeparam name="T">The type of the elements of the array.</typeparam>
   /// <seealso cref="SequenceEqualityComparer{T,U}"/>
   public sealed class ArrayEqualityComparer<T> : IEqualityComparer<T[]>
   {
      private static readonly IEqualityComparer<T[]> INSTANCE = new ArrayEqualityComparer<T>( null );

      /// <summary>
      /// Returns the equality comparer for arrays with element type <typeparamref name="T"/> which will use default equality comparer for the elements of the array.
      /// </summary>
      /// <value>The equality comparer for arrays with element type <typeparamref name="T"/> which will use default equality comparer for the elements of the array.</value>
      public static IEqualityComparer<T[]> DefaultArrayEqualityComparer
      {
         get
         {
            return INSTANCE;
         }
      }

      /// <summary>
      /// Creates a new equality comparer for arrays with element type <typeparamref name="T"/> which will use the given equality comparer for the elements of the array.
      /// </summary>
      /// <param name="itemComparer">The equality comparer to use when comparing elements of the array.</param>
      /// <returns>A new equality comparer for arrays with element type <typeparamref name="T"/> which will use the given equality comparer for the elements of the array.</returns>
      public static IEqualityComparer<T[]> NewArrayEqualityComparer( IEqualityComparer<T> itemComparer )
      {
         return new ArrayEqualityComparer<T>( itemComparer );
      }

      /// <summary>
      /// Checks equality of given collections without creating a new instance of this class.
      /// </summary>
      /// <param name="x">The first array. May be <c>null</c>.</param>
      /// <param name="y">The second array. May be <c>null</c>.</param>
      /// <param name="itemComparer">The optional equality comparer for items of the array. If <c>null</c>, the default will be used.</param>
      /// <returns>Whether two collections equal.</returns>
      public static Boolean Equals( T[] x, T[] y, IEqualityComparer<T> itemComparer = null )
      {
         return ArrayEquality_NoCheck( x, y, ( itemComparer ?? EqualityComparer<T>.Default ).Equals );
      }

      /// <summary>
      /// Calculates the hash code of given array without creating a new instance of this class.
      /// </summary>
      /// <param name="obj">The colleciton. May be <c>null</c>.</param>
      /// <param name="itemComparer">The optional equality comparer for items of the array. If <c>null</c>, the default will be used.</param>
      /// <returns>The hash code for given array.</returns>
      public static Int32 GetHashCode( T[] obj, IEqualityComparer<T> itemComparer = null )
      {
         return ArrayHashCode_NoCheck( obj, ( itemComparer ?? EqualityComparer<T>.Default ).GetHashCode );
      }

      /// <summary>
      /// Helper method to check whether two arrays are considered to be equal given optional equality comparer for items.
      /// </summary>
      /// <param name="x">The first array. May be <c>null</c>.</param>
      /// <param name="y">The second array. May be <c>null</c>.</param>
      /// <param name="equality">The optional equality callback for items. If not supplied, a default callback (the <see cref="IEqualityComparer{T}.Equals(T, T)" /> method from <see cref="EqualityComparer{T}.Default"/> ) will be used.</param>
      /// <returns><c>true</c> if both arrays are <c>null</c>, or both arrays are non null and have same amount and same items; <c>false</c> otherwise.</returns>
      public static Boolean ArrayEquality( T[] x, T[] y, Equality<T> equality = null )
      {
         return ArrayEquality_NoCheck( x, y, equality ?? EqualityComparer<T>.Default.Equals );
      }

      /// <summary>
      /// Helper method to calculate hash code for array.
      /// </summary>
      /// <param name="obj">The array. May be <c>null</c>.</param>
      /// <param name="hashCode">The optional hashcode calculation callback. If not supplied, a default callback (the <see cref="IEqualityComparer{T}.GetHashCode(T)" /> method from <see cref="EqualityComparer{T}.Default"/> ) will be used.</param>
      /// <returns>The hash code for <paramref name="obj" />.</returns>
      public static Int32 ArrayHashCode( T[] obj, HashCode<T> hashCode = null )
      {
         ArgumentValidator.ValidateNotNull( "Hash code delegate", hashCode );
         return ArrayHashCode_NoCheck( obj, hashCode ?? EqualityComparer<T>.Default.GetHashCode );
      }

      private static Boolean ArrayEquality_NoCheck( T[] x, T[] y, Equality<T> equality )
      {
         Boolean result;
         if ( x == null )
         {
            result = y == null;
         }
         else
         {
            result = y != null && x.Length == y.Length;
            if ( result && x.Length > 0 )
            {
               var max = x.Length;
               var i = 0;
               while ( i < max && equality( x[i], y[i] ) )
               {
                  ++i;
               }
               result = i == max;
            }
         }
         return result;
      }

      private static Int32 ArrayHashCode_NoCheck( T[] obj, HashCode<T> hashCode )
      {
         var result = 0;
         if ( obj != null )
         {
            // Jon Skeet's answer on http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
            result = 17;
            unchecked // Overflow is fine, just wrap
            {
               var max = obj.Length;
               for ( var i = 0; i < max; ++i )
               {
                  result = result * 23 + hashCode( obj[i] );
               }
            }
         }
         return result;
      }

      private readonly Equality<T> _equality;
      private readonly HashCode<T> _hashCode;

      private ArrayEqualityComparer( IEqualityComparer<T> itemComparer )
      {
         if ( itemComparer == null )
         {
            itemComparer = EqualityComparer<T>.Default;
         }
         this._equality = itemComparer.Equals;
         this._hashCode = itemComparer.GetHashCode;
      }

      #region IEqualityComparer<T[]> Members

      Boolean IEqualityComparer<T[]>.Equals( T[] x, T[] y )
      {
         return ArrayEquality_NoCheck( x, y, this._equality );
      }

      Int32 IEqualityComparer<T[]>.GetHashCode( T[] obj )
      {
         return ArrayHashCode_NoCheck( obj, this._hashCode );
      }

      #endregion
   }

   /// <summary>
   /// This class provides content-based equality comparing for collections. It is still possible to use <see cref="SequenceEqualityComparer{T,U}"/> for collections and have same result, but this class will be faster.
   /// </summary>
   /// <typeparam name="T">The type of collection, e.g. <see cref="IList{X}"/>.</typeparam>
   /// <typeparam name="U">The type of the elements of the collection.</typeparam>
   /// <seealso cref="SequenceEqualityComparer{T,U}"/>
   public sealed class CollectionEqualityComparer<T, U> : IEqualityComparer<T>
      where T : ICollection<U>
   {
      private static readonly IEqualityComparer<T> INSTANCE = new CollectionEqualityComparer<T, U>( null );

      /// <summary>
      /// Returns the equality comparer for collections with element type <typeparamref name="T"/> which will use default equality comparer for the elements of the array.
      /// </summary>
      /// <value>The equality comparer for collections with element type <typeparamref name="T"/> which will use default equality comparer for the elements of the array.</value>
      public static IEqualityComparer<T> DefaultCollectionEqualityComparer
      {
         get
         {
            return INSTANCE;
         }
      }

      /// <summary>
      /// Creates a new equality comparer for collections with element type <typeparamref name="T"/> which will use the given equality comparer for the elements of the collection.
      /// </summary>
      /// <param name="itemComparer">The equality comparer to use when comparing elements of the collection.</param>
      /// <returns>A new equality comparer for collections with element type <typeparamref name="T"/> which will use the given equality comparer for the elements of the collection.</returns>
      public static IEqualityComparer<T> NewCollectionEqualityComparer( IEqualityComparer<U> itemComparer )
      {
         return new CollectionEqualityComparer<T, U>( itemComparer );
      }

      /// <summary>
      /// Checks equality of given collections without creating a new instance of this class.
      /// </summary>
      /// <param name="x">The first collection. May be <c>null</c>.</param>
      /// <param name="y">The second collection. May be <c>null</c>.</param>
      /// <param name="itemComparer">The optional equality comparer for items of the collection. If <c>null</c>, the default will be used.</param>
      /// <returns>Whether two collections equal.</returns>
      public static Boolean Equals( T x, T y, IEqualityComparer<U> itemComparer = null )
      {
         return CollectionEquality_NoCheck( x, y, ( itemComparer ?? EqualityComparer<U>.Default ).Equals );
      }

      /// <summary>
      /// Calculates the hash code of given collection without creating a new instance of this class.
      /// </summary>
      /// <param name="obj">The colleciton. May be <c>null</c>.</param>
      /// <param name="itemComparer">The optional equality comparer for items of the collection. If <c>null</c>, the default will be used.</param>
      /// <returns>The hash code for given collection.</returns>
      public static Int32 GetHashCode( T obj, IEqualityComparer<U> itemComparer = null )
      {
         return CollectionHashCode_NoCheck( obj, ( itemComparer ?? EqualityComparer<U>.Default ).GetHashCode );
      }

      /// <summary>
      /// Helper method to check whether two collections are considered to be equal given optional equality comparer for items.
      /// </summary>
      /// <param name="x">The first collection. May be <c>null</c>.</param>
      /// <param name="y">The second collection. May be <c>null</c>.</param>
      /// <param name="equality">The optional equality callback for items. If not supplied, a default callback (the <see cref="IEqualityComparer{U}.Equals(U, U)" /> method from <see cref="EqualityComparer{U}.Default"/> ) will be used.</param>
      /// <returns><c>true</c> if both collections are <c>null</c>, or both collections are non null and have same amount and same items; <c>false</c> otherwise.</returns>
      public static Boolean CollectionEquality( T x, T y, Equality<U> equality = null )
      {
         return CollectionEquality_NoCheck( x, y, equality ?? EqualityComparer<U>.Default.Equals );
      }

      /// <summary>
      /// Calculates the hash code of given collection without creating a new instance of this class.
      /// </summary>
      /// <param name="obj">The collection. May be <c>null</c>.</param>
      /// <param name="hashCode">The optional equality comparer for items of the collection. If <c>null</c>, the default will be used.</param>
      /// <returns>The hash code for given collection.</returns>
      public static Int32 CollectionHashCode( T obj, HashCode<U> hashCode = null )
      {
         return CollectionHashCode_NoCheck( obj, hashCode ?? EqualityComparer<U>.Default.GetHashCode );
      }

      private static Boolean CollectionEquality_NoCheck( T x, T y, Equality<U> equality )
      {
         Boolean result;
         if ( x == null )
         {
            result = y == null;
         }
         else
         {
            result = y != null && x.Count == y.Count;
            if ( result && x.Count > 0 )
            {
               using ( var xEnum = x.GetEnumerator() )
               using ( var yEnum = y.GetEnumerator() )
               {
                  while ( xEnum.MoveNext() && yEnum.MoveNext() ) // this should always work since .Count was the same...
                  {
                     if ( !equality( xEnum.Current, yEnum.Current ) )
                     {
                        result = false;
                        break;
                     }
                  }
               }
            }
         }
         return result;
      }

      private static Int32 CollectionHashCode_NoCheck( T obj, HashCode<U> hashCode )
      {
         var result = 0;
         if ( obj != null )
         {
            // Jon Skeet's answer on http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
            result = 17;
            unchecked // Overflow is fine, just wrap
            {
               foreach ( var item in obj )
               {
                  result = result * 23 + hashCode( item );
               }
            }
         }
         return result;
      }

      private readonly Equality<U> _equality;
      private readonly HashCode<U> _hashCode;

      private CollectionEqualityComparer( IEqualityComparer<U> itemComparer )
      {
         if ( itemComparer == null )
         {
            itemComparer = EqualityComparer<U>.Default;
         }
         this._equality = itemComparer.Equals;
         this._hashCode = itemComparer.GetHashCode;
      }

      Boolean IEqualityComparer<T>.Equals( T x, T y )
      {
         return CollectionEquality_NoCheck( x, y, this._equality );
      }

      Int32 IEqualityComparer<T>.GetHashCode( T obj )
      {
         return CollectionHashCode_NoCheck( obj, this._hashCode );
      }
   }

   /// <summary>
   /// This class provides content-based equality comparing for lists. It is still possible to use <see cref="SequenceEqualityComparer{T,U}"/> or <see cref="CollectionEqualityComparer{T, U}"/> for lists and have same result, but this class will be faster and will not use heap memory.
   /// </summary>
   /// <typeparam name="T">The type of list, e.g. <see cref="List{X}"/>.</typeparam>
   /// <typeparam name="U">The type of the elements of the list.</typeparam>
   /// <seealso cref="SequenceEqualityComparer{T,U}"/>
   /// <seealso cref="CollectionEqualityComparer{T, U}"/>
   public sealed class ListEqualityComparer<T, U> : IEqualityComparer<T>
      where T : IList<U>
   {
      private static readonly IEqualityComparer<T> INSTANCE = new ListEqualityComparer<T, U>( null );

      /// <summary>
      /// Returns the equality comparer for lists with element type <typeparamref name="T"/> which will use default equality comparer for the elements of the array.
      /// </summary>
      /// <value>The equality comparer for lists with element type <typeparamref name="T"/> which will use default equality comparer for the elements of the array.</value>
      public static IEqualityComparer<T> DefaultListEqualityComparer
      {
         get
         {
            return INSTANCE;
         }
      }

      /// <summary>
      /// Creates a new equality comparer for lists with element type <typeparamref name="T"/> which will use the given equality comparer for the elements of the list.
      /// </summary>
      /// <param name="itemComparer">The equality comparer to use when comparing elements of the list.</param>
      /// <returns>A new equality comparer for lists with element type <typeparamref name="T"/> which will use the given equality comparer for the elements of the list.</returns>
      public static IEqualityComparer<T> NewListEqualityComparer( IEqualityComparer<U> itemComparer )
      {
         return new ListEqualityComparer<T, U>( itemComparer );
      }

      /// <summary>
      /// Checks equality of given lists without creating a new instance of this class.
      /// </summary>
      /// <param name="x">The first list. May be <c>null</c>.</param>
      /// <param name="y">The second list. May be <c>null</c>.</param>
      /// <param name="itemComparer">The optional equality comparer for items of the list. If <c>null</c>, the default will be used.</param>
      /// <returns>Whether two lists equal.</returns>
      public static Boolean Equals( T x, T y, IEqualityComparer<U> itemComparer = null )
      {
         return ListEquality_NoCheck( x, y, ( itemComparer ?? EqualityComparer<U>.Default ).Equals );
      }

      /// <summary>
      /// Calculates the hash code of given list without creating a new instance of this class.
      /// </summary>
      /// <param name="obj">The list. May be <c>null</c>.</param>
      /// <param name="itemComparer">The optional equality comparer for items of the list. If <c>null</c>, the default will be used.</param>
      /// <returns>The hash code for given list.</returns>
      public static Int32 GetHashCode( T obj, IEqualityComparer<U> itemComparer = null )
      {
         return ListHashCode_NoCheck( obj, ( itemComparer ?? EqualityComparer<U>.Default ).GetHashCode );
      }

      /// <summary>
      /// Helper method to check whether two lists are considered to be equal given optional equality comparer for items.
      /// </summary>
      /// <param name="x">The first list. May be <c>null</c>.</param>
      /// <param name="y">The second list. May be <c>null</c>.</param>
      /// <param name="equality">The optional equality callback for items. If not supplied, a default callback (the <see cref="IEqualityComparer{U}.Equals(U, U)" /> method from <see cref="EqualityComparer{U}.Default"/> ) will be used.</param>
      /// <returns><c>true</c> if both lists are <c>null</c>, or both lists are non null and have same amount and same items; <c>false</c> otherwise.</returns>
      public static Boolean ListEquality( T x, T y, Equality<U> equality = null )
      {
         return ListEquality_NoCheck( x, y, equality ?? EqualityComparer<U>.Default.Equals );
      }

      /// <summary>
      /// Helper method to calculate hash code for list.
      /// </summary>
      /// <param name="obj">The list. May be <c>null</c>.</param>
      /// <param name="hashCode">The optional hashcode calculation callback. If not supplied, a default callback (the <see cref="IEqualityComparer{T}.GetHashCode(T)" /> method from <see cref="EqualityComparer{T}.Default"/> ) will be used.</param>
      /// <returns>The hash code for <paramref name="obj" />.</returns>
      public static Int32 ListHashCode( T obj, HashCode<U> hashCode = null )
      {
         return ListHashCode_NoCheck( obj, hashCode ?? EqualityComparer<U>.Default.GetHashCode );
      }

      private static Boolean ListEquality_NoCheck( T x, T y, Equality<U> equality )
      {
         Boolean result;
         if ( x == null )
         {
            result = y == null;
         }
         else
         {
            result = y != null && x.Count == y.Count;
            if ( result && x.Count > 0 )
            {
               var max = x.Count;
               var i = 0;
               while ( i < max && equality( x[i], y[i] ) )
               {
                  ++i;
               }
               result = i == max;
            }
         }
         return result;
      }

      private static Int32 ListHashCode_NoCheck( T obj, HashCode<U> hashCode )
      {
         var result = 0;
         if ( obj != null )
         {
            // Jon Skeet's answer on http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
            result = 17;
            unchecked // Overflow is fine, just wrap
            {
               var max = obj.Count;
               for ( var i = 0; i < max; ++i )
               {
                  result = result * 23 + hashCode( obj[i] );
               }
            }
         }
         return result;
      }

      private readonly Equality<U> _equality;
      private readonly HashCode<U> _hashCode;

      private ListEqualityComparer( IEqualityComparer<U> itemComparer )
      {
         if ( itemComparer == null )
         {
            itemComparer = EqualityComparer<U>.Default;
         }

         this._equality = itemComparer.Equals;
         this._hashCode = itemComparer.GetHashCode;
      }

      Boolean IEqualityComparer<T>.Equals( T x, T y )
      {
         return ListEquality_NoCheck( x, y, this._equality );
      }

      Int32 IEqualityComparer<T>.GetHashCode( T obj )
      {
         return ListHashCode_NoCheck( obj, this._hashCode );
      }
   }
}
