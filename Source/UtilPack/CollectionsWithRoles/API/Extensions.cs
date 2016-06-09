/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
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
using CommonUtils;
using CollectionsWithRoles.API;

public static partial class E_CWR
{
   /// <summary>
   /// The default implementation of extension method <see cref="Enumerable.ToArray{T}(IEnumerable{T})"/> doesn't recognize other arrays or <see cref="CollectionQuery{T}"/> objects.
   /// This method provides implementation of converting <see cref="IEnumerable{T}"/>s to arrays that recognizes <see cref="CollectionQuery{T}"/> and other arrays.
   /// </summary>
   /// <typeparam name="T">The type of the elements of <paramref name="enumerable"/>.</typeparam>
   /// <param name="enumerable">The <see cref="IEnumerable{T}"/>.</param>
   /// <param name="returnThisIfArray">If <c>true</c>, this method will check first whether <paramref name="enumerable"/> is an array, and return it. Otherwise, a new array is always created.</param>
   /// <returns>An array with contents of <paramref name="enumerable"/>.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="enumerable"/> is <c>null</c>.</exception>
   public static T[] ToArrayCWR<T>( this IEnumerable<T> enumerable, Boolean returnThisIfArray = false )
   {
      ArgumentValidator.ValidateNotNull( "Enumerable", enumerable );

      T[] result;
      if ( returnThisIfArray && enumerable is T[] )
      {
         result = (T[]) enumerable;
      }
      else
      {
         Int32 size;

         // Try convert to .NET collection first
         var coll = enumerable as ICollection<T>;
         if ( coll != null )
         {
            // Success, we can easily create & populate array now
            size = coll.Count;
            if ( size > 0 )
            {
               result = new T[size];
               coll.CopyTo( result, 0 );
            }
            else
            {
               result = Empty<T>.Array;
            }
         }
         else
         {
            // Try convert to CollectionWithRoles CollectionQuery
            var collWR = enumerable as CollectionQuery<T>;
            if ( collWR != null )
            {
               // Success, just like in .NET collection, we can easily create & populate array now
               size = collWR.Count;
               if ( size > 0 )
               {
                  result = new T[size];
                  collWR.CopyTo( result, 0 );
               }
               else
               {
                  result = Empty<T>.Array;
               }
            }
            else
            {
               // Try convert to CollectionWithRoles ArrayQuery
               var arrWR = enumerable as ArrayQuery<T>;
               if ( arrWR != null )
               {
                  // Can once again easily create & populate array now
                  size = arrWR.Count;
                  if ( size > 0 )
                  {
                     result = new T[size];
                     for ( var i = 0; i < arrWR.Count; ++i )
                     {
                        result[i] = arrWR[i];
                     }
                  }
                  else
                  {
                     result = Empty<T>.Array;
                  }
               }
               else
               {
                  // Build array as we iterate the enumerable
                  result = null;
                  size = 0;
                  T[] tmp;
                  foreach ( var elem in enumerable )
                  {
                     if ( result == null )
                     {
                        // Initial step - create array with a guess of 4 elements
                        result = new T[4];
                     }
                     else if ( result.Length == size )
                     {
                        // Have to grow array x2
                        tmp = new T[checked(size * 2)];
                        Array.Copy( result, 0, tmp, 0, size );
                        result = tmp;
                     }
                     result[size] = elem;
                     ++size;
                  }

                  // Trim array
                  if ( size == 0 )
                  {
                     result = Empty<T>.Array;
                  }
                  else if ( size != result.Length )
                  {
                     tmp = new T[size];
                     Array.Copy( result, 0, tmp, 0, size );
                  }
               }
            }
         }
      }
      return result;
   }

   /// <summary>
   /// Will override default <see cref="Enumerable.ToArray{T}(IEnumerable{T})"/> when the target object reference type is of <see cref="CollectionQuery{T}"/> or sub-type.
   /// This method just calls <see cref="ToArrayCWR"/>.
   /// </summary>
   /// <typeparam name="T">The type of elements in collection.</typeparam>
   /// <param name="collectionQ">The <see cref="CollectionQuery{T}"/>.</param>
   /// <returns>The array containing same elements as <paramref name="collectionQ"/>.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="collectionQ"/> is <c>null</c>.</exception>
   public static T[] ToArray<T>( this CollectionQuery<T> collectionQ )
   {
      return ToArrayCWR( collectionQ );
   }

   /// <summary>
   /// Tries to add an element with the provided key and value to this dictionary.
   /// </summary>
   /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
   /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
   /// <typeparam name="TDictionaryQuery">The type of the Query-role of the dictionary.</typeparam>
   /// <param name="dic">The <see cref="DictionaryMutable{TKey, TValue, TDictionaryQuery}"/>.</param>
   /// <param name="key">The key.</param>
   /// <param name="value">The value.</param>
   /// <returns><c>true</c> if an element was successfully added; <c>false</c> otherwise.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="dic"/> is <c>null</c>.</exception>
   public static Boolean TryAdd<TKey, TValue, TDictionaryQuery>( this DictionaryMutable<TKey, TValue, TDictionaryQuery> dic, TKey key, TValue value )
      where TDictionaryQuery : DictionaryQuery<TKey, TValue>
   {
      var retVal = !dic.CQ.ContainsKey( key );
      if ( retVal )
      {
         dic.Add( key, value );
      }
      return retVal;
   }

   /// <summary>
   /// Helper function to perform simple for-loop (not the foreach loop with try-finally and .GetEnumerator() calls) over a <see cref="ArrayQuery{T}"/>.
   /// </summary>
   /// <typeparam name="TValue">The type of array elements.</typeparam>
   /// <param name="array">The array. If <c>null</c>, this method does nothing.</param>
   /// <param name="action">The action to execute for each array element. If <c>null</c>, this method does nothing.</param>
   public static void ForEach<TValue>( this ArrayQuery<TValue> array, Action<TValue> action )
   {
      if ( array != null && action != null && array.Count > 0 )
      {
         for ( var i = 0; i < array.Count; ++i )
         {
            action( array[i] );
         }
      }
   }

   /// <summary>
   /// Helper method similar to <see cref="E_CommonUtils.GetOrDefault"/>, but for <see cref="ArrayQuery{TValue}"/>.
   /// </summary>
   /// <typeparam name="T">The type of the values in the array.</typeparam>
   /// <param name="array">This <see cref="ArrayQuery{TValue}"/>.</param>
   /// <param name="index">The index to retrieve value at.</param>
   /// <returns>Value at <paramref name="index"/>, or default value for <typeparamref name="T"/>, if index was outside array index bounds.</returns>
   public static T GetOrDefault<T>( this ArrayQuery<T> array, Int32 index )
   {
      return index >= 0 && index < array.Count ? array[index] : default( T );
   }
}