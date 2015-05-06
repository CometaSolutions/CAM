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
using CommonUtils;

public static partial class E_CommonUtils
{
   /// <summary>
   /// Gets or adds value from <paramref name="dictionary"/> given a <paramref name="key"/>, using <paramref name="valueFactory"/> as value factory. Not threadsafe.
   /// </summary>
   /// <typeparam name="TKey">The type of the keys in <paramref name="dictionary"/>.</typeparam>
   /// <typeparam name="TValue">The type of the values in <paramref name="dictionary"/>.</typeparam>
   /// <param name="dictionary">The dictionary to get value from. If value does not exist for <paramref name="key"/>, it will be added to dictionary.</param>
   /// <param name="key">The key to use to search value from <paramref name="dictionary"/>.</param>
   /// <param name="valueFactory">The callback to generate value.</param>
   /// <returns>The value which was either found in <paramref name="dictionary"/> or created by <paramref name="valueFactory"/>.</returns>
   /// <exception cref="ArgumentNullException">If value is not found from <paramref name="dictionary"/> using <paramref name="key"/>, and <paramref name="valueFactory"/> is <c>null</c>.</exception>
   /// <exception cref="NullReferenceException">If <paramref name="dictionary"/> is <c>null</c>.</exception>
   public static TValue GetOrAdd_NotThreadSafe<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory )
   {
      TValue result;
      if ( !dictionary.TryGetValue( key, out result ) )
      {
         ArgumentValidator.ValidateNotNull( "Value factory", valueFactory );
         result = valueFactory();
         dictionary.Add( key, result );
      }
      return result;
   }

   /// <summary>
   /// Gets or adds value from <paramref name="dictionary"/> given a <paramref name="key"/>, using <paramref name="valueFactory"/> as value factory. Not threadsafe.
   /// </summary>
   /// <typeparam name="TKey">The type of the keys in <paramref name="dictionary"/>.</typeparam>
   /// <typeparam name="TValue">The type of the values in <paramref name="dictionary"/>.</typeparam>
   /// <param name="dictionary">The dictionary to get value from. If value does not exist for <paramref name="key"/>, it will be added to dictionary.</param>
   /// <param name="key">The key to use to search value from <paramref name="dictionary"/>.</param>
   /// <param name="valueFactory">The callback to generate value. The parameter will be <paramref name="key"/>.</param>
   /// <returns>The value which was either found in <paramref name="dictionary"/> or created by <paramref name="valueFactory"/>.</returns>
   /// <exception cref="ArgumentNullException">If value is not found from <paramref name="dictionary"/> using <paramref name="key"/>, and <paramref name="valueFactory"/> is <c>null</c>.</exception>
   /// <exception cref="NullReferenceException">If <paramref name="dictionary"/> is <c>null</c>.</exception>
   public static TValue GetOrAdd_NotThreadSafe<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory )
   {
      TValue result;
      if ( !dictionary.TryGetValue( key, out result ) )
      {
         ArgumentValidator.ValidateNotNull( "Value factory", valueFactory );
         result = valueFactory( key );
         dictionary.Add( key, result );
      }
      return result;
   }

   ///// <summary>
   ///// Gets or adds value from <paramref name="dictionary"/> given a <paramref name="key"/>, using <paramref name="valueFactory"/> as value factory. Threadsafe - will lock given lock or whole dictionary when adding.
   ///// </summary>
   ///// <typeparam name="TKey">The type of the keys in <paramref name="dictionary"/>.</typeparam>
   ///// <typeparam name="TValue">The type of the values in <paramref name="dictionary"/>.</typeparam>
   ///// <param name="dictionary">The dictionary to get value from. If value does not exist for <paramref name="key"/>, it will be added to dictionary.</param>
   ///// <param name="key">The key to use to search value from <paramref name="dictionary"/>.</param>
   ///// <param name="valueFactory">The callback to generate value. The parameter will be <paramref name="key"/>.</param>
   ///// <param name="added">This parameter will be <c>true</c> if this method added a new value to dictionary; <c>false</c> otherwise.</param>
   ///// <param name="lockToUse">The lock to use when the value does not exist. If not given (is <c>null</c>), the dictionary itself will be used as lock.</param>
   ///// <returns>The value which was either found in <paramref name="dictionary"/> or created by <paramref name="valueFactory"/>.</returns>
   ///// <exception cref="ArgumentNullException">If value is not found from <paramref name="dictionary"/> using <paramref name="key"/>, and <paramref name="valueFactory"/> is <c>null</c>.</exception>
   ///// <exception cref="NullReferenceException">If <paramref name="dictionary"/> is <c>null</c>.</exception>
   //public static TValue GetOrAdd_WithLock<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory, out Boolean added, Object lockToUse = null )
   //{
   //   TValue value;
   //   added = false;
   //   if ( !dictionary.TryGetValue( key, out value ) )
   //   {
   //      ArgumentValidator.ValidateNotNull( "Value factory", valueFactory );

   //      lock ( lockToUse ?? dictionary )
   //      {
   //         if ( !dictionary.TryGetValue( key, out value ) )
   //         {
   //            value = valueFactory( key );
   //            dictionary.Add( key, value );
   //            added = true;
   //         }
   //      }
   //   }

   //   return value;
   //}

   /// <summary>
   /// Gets or adds value from <paramref name="dictionary"/> given a <paramref name="key"/>, using <paramref name="valueFactory"/> as value factory. Threadsafe - will lock given lock or whole dictionary when adding.
   /// </summary>
   /// <typeparam name="TKey">The type of the keys in <paramref name="dictionary"/>.</typeparam>
   /// <typeparam name="TValue">The type of the values in <paramref name="dictionary"/>.</typeparam>
   /// <param name="dictionary">The dictionary to get value from. If value does not exist for <paramref name="key"/>, it will be added to dictionary.</param>
   /// <param name="key">The key to use to search value from <paramref name="dictionary"/>.</param>
   /// <param name="valueFactory">The callback to generate value. The parameter will be <paramref name="key"/>.</param>
   /// <param name="lockToUse">The lock to use when the value does not exist. If not given (is <c>null</c>), the dictionary itself will be used as lock.</param>
   /// <returns>The value which was either found in <paramref name="dictionary"/> or created by <paramref name="valueFactory"/>.</returns>
   /// <exception cref="ArgumentNullException">If value is not found from <paramref name="dictionary"/> using <paramref name="key"/>, and <paramref name="valueFactory"/> is <c>null</c>.</exception>
   /// <exception cref="NullReferenceException">If <paramref name="dictionary"/> is <c>null</c>.</exception>
   public static TValue GetOrAdd_WithLock<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory, Object lockToUse = null )
   {
      TValue value;
      if ( !dictionary.TryGetValue( key, out value ) )
      {
         ArgumentValidator.ValidateNotNull( "Value factory", valueFactory );

         lock ( lockToUse ?? dictionary )
         {
            if ( !dictionary.TryGetValue( key, out value ) )
            {
               value = valueFactory( key );
               dictionary.Add( key, value );
            }
         }
      }

      return value;
   }

   /// <summary>
   /// Tries to get value from <paramref name="dic"/> using <paramref name="key"/>, or return <paramref name="defaultValue"/> if no value is associated for <paramref name="key"/> in <paramref name="dic"/>.
   /// </summary>
   /// <typeparam name="TKey">The type of the keys in <paramref name="dic"/>.</typeparam>
   /// <typeparam name="TValue">The type of the values in <paramref name="dic"/>.</typeparam>
   /// <param name="dic">The dictionary to search value from. If value does not exist for <paramref name="key"/>, it will not be added to dictionary.</param>
   /// <param name="key">The key to use to search value from <paramref name="dic"/>.</param>
   /// <param name="defaultValue">The value to return if no value exists for <paramref name="key"/> in <paramref name="dic"/>.</param>
   /// <returns>The value for <paramref name="key"/> in <paramref name="dic"/>, or <paramref name="defaultValue"/> if <paramref name="dic"/> does not have value associated for <paramref name="key"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="dic"/> is <c>null</c>.</exception>
   public static TValue GetOrDefault<TKey, TValue>( this IDictionary<TKey, TValue> dic, TKey key, TValue defaultValue = default(TValue) )
   {
      TValue value;
      return dic.TryGetValue( key, out value ) ? value : defaultValue;
   }

   /// <summary>
   /// Checks whether two struct arrays are both <c>null</c> or both non-<c>null</c> and they contain the same sequence of elements.
   /// </summary>
   /// <typeparam name="T">The type of elements in the array.</typeparam>
   /// <param name="array1">The first array.</param>
   /// <param name="array2">The second array.</param>
   /// <returns><c>true</c> if both arrays are <c>null</c> or if both arrays are non-<c>null</c> and contain the same sequence of elements; <c>false</c> otherwise.</returns>
   public static Boolean StructArrayEquals<T>( this T[] array1, T[] array2 )
      where T : struct
   {
      var result = Object.ReferenceEquals( array1, array2 );
      if ( !result && array1 != null && array2 != null && array1.Length == array1.Length )
      {
         for ( var i = 0; i < array1.Length; ++i )
         {
            result = array1[i].Equals( array2[i] );
            if ( !result )
            {
               break;
            }
         }
      }
      return result;
   }
   // TODO: swap method for byte, sbyte, int16, uint16, int32, uint32, int64, uin64
   // From http://graphics.stanford.edu/~seander/bithacks.html#SwappingValuesXOR
   // (((a) ^ (b)) && ((b) ^= (a) ^= (b), (a) ^= (b)))

   /// <summary>
   /// Helper method to swap two elements in the array.
   /// </summary>
   /// <typeparam name="T">The type of the elements in the array.</typeparam>
   /// <param name="array">The array.</param>
   /// <param name="idx1">The index of one element to swap.</param>
   /// <param name="idx2">The index of another element to swap.</param>
   /// <exception cref="NullReferenceException">If <paramref name="array"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentOutOfRangeException">If <paramref name="idx1"/> or <paramref name="idx2"/> are out of index range for the array.</exception>
   public static void Swap<T>( this T[] array, Int32 idx1, Int32 idx2 )
   {
      var tmp = array[idx1];
      array[idx1] = array[idx2];
      array[idx2] = tmp;
   }

   /// <summary>
   /// Uses deferred equality detection version of binary search to find suitable item from given array.
   /// This means that if the elements are not unique, it returns the smallest index of the region where element is considered to match the given item.
   /// </summary>
   /// <typeparam name="T">The type of the elements in the array.</typeparam>
   /// <param name="array">The array.</param>
   /// <param name="item">The item to search.</param>
   /// <param name="comparer">The comparer to use. If <c>null</c>, a default comparer will be used.</param>
   /// <returns>The index of the first element matching the given <paramref name="item"/>, or <c>-1</c> if no such element found or if <paramref name="array"/> is <c>null</c>.</returns>
   /// <remarks>
   /// As normal binary search algorithm, this assumes that the array is sorted based on the given comparer.
   /// Wrong result will be produced if the array is not sorted.
   /// </remarks>
   public static Int32 BinarySearchDeferredEqualityDetection<T>( this T[] array, T item, IComparer<T> comparer = null )
   {
      comparer = comparer ?? Comparer<T>.Default;

      var max = array == null ? 0 : array.Length - 1;
      var min = 0;
      while ( min < max )
      {
         var mid = min + ( ( max - min ) >> 1 ); // Overflow protection
         if ( comparer.Compare( array[mid], item ) < 0 )
         {
            min = mid + 1;
         }
         else
         {
            max = mid;
         }
      }
      return array != null && min == max && comparer.Compare( array[min], item ) == 0 ?
         min :
         -1;
   }

   /// <summary>
   /// Uses deferred equality detection version of binary search to find suitable item from given list.
   /// This means that if the elements are not unique, it returns the smallest index of the region where element is considered to match the given item.
   /// </summary>
   /// <typeparam name="T">The type of the elements in the list.</typeparam>
   /// <param name="list">The array.</param>
   /// <param name="item">The item to search.</param>
   /// <param name="comparer">The comparer to use. If <c>null</c>, a default comparer will be used.</param>
   /// <returns>The index of the first element matching the given <paramref name="item"/>, or <c>-1</c> if no such element found or if <paramref name="list"/> is <c>null</c>.</returns>
   /// <remarks>
   /// As normal binary search algorithm, this assumes that the list is sorted based on the given comparer.
   /// Wrong result will be produced if the list is not sorted.
   /// </remarks>
   public static Int32 BinarySearchDeferredEqualityDetection<T>( this IList<T> list, T item, IComparer<T> comparer = null )
   {
      comparer = comparer ?? Comparer<T>.Default;

      var max = list == null ? 0 : list.Count - 1;
      var min = 0;
      while ( min < max )
      {
         var mid = min + ( ( max - min ) >> 1 ); // Overflow protection
         if ( comparer.Compare( list[mid], item ) < 0 )
         {
            min = mid + 1;
         }
         else
         {
            max = mid;
         }
      }
      return list != null && min == max && comparer.Compare( list[min], item ) == 0 ?
         min :
         -1;
   }

   /// <summary>
   /// Checks whether the array is <c>null</c> or an empty array.
   /// </summary>
   /// <typeparam name="T">The array element type.</typeparam>
   /// <param name="array">The array.</param>
   /// <returns><c>true</c> if <paramref name="array"/> is not <c>null</c> and contains at least one element; <c>false</c> otherwise.</returns>
   public static Boolean IsNullOrEmpty<T>( this T[] array )
   {
      return array == null || array.Length <= 0;
   }

   /// <summary>
   /// Checks whether the enumerable is <c>null</c> or an empty enumerable.
   /// </summary>
   /// <typeparam name="T">The enumerable element type.</typeparam>
   /// <param name="enumerable">The enumerable.</param>
   /// <returns><c>true</c> if <paramref name="enumerable"/> is not <c>null</c> and contains at least one element; <c>false</c> otherwise.</returns>
   public static Boolean IsNullOrEmpty<T>( this IEnumerable<T> enumerable )
   {
      return enumerable == null || !enumerable.Any();
   }

   /// <summary>
   /// Checks whether given array is not <c>null</c> and has at least <paramref name="count"/> elements starting at <paramref name="offset"/>.
   /// </summary>
   /// <typeparam name="T">The array element type.</typeparam>
   /// <param name="array">The array.</param>
   /// <param name="offset">The offset in array.</param>
   /// <param name="count">The amount of elements array must have starting at <paramref name="offset"/>.</param>
   /// <exception cref="ArgumentNullException">If <paramref name="array"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentOutOfRangeException">If <paramref name="offset"/> or <paramref name="count"/> is less than <c>0</c>, or if array length is smaller than <paramref name="offset"/> <c>+</c> <paramref name="count"/>.</exception>
   /// <exception cref="ArgumentException">If <paramref name="offset"/> + <paramref name="count"/> is greater than array length.</exception>
   public static void CheckArrayArguments<T>( this T[] array, Int32 offset, Int32 count )
   {
      ArgumentValidator.ValidateNotNull( "Array", array );
      if ( offset < 0 )
      {
         throw new ArgumentOutOfRangeException( "Offset" );
      }
      if ( count < 0 )
      {
         throw new ArgumentOutOfRangeException( "Count" );
      }
      if ( array.Length - offset < count )
      {
         throw new ArgumentException( "Invalid offset and length" );
      }
   }

   /// <summary>
   /// Changes a single element into a enumerable containing only that element.
   /// </summary>
   /// <typeparam name="T">The type of the element.</typeparam>
   /// <param name="element">The element.</param>
   /// <returns><see cref="IEnumerable{T}"/> containing only <paramref name="element"/>.</returns>
   public static IEnumerable<T> Singleton<T>( this T element )
   {
      return Enumerable.Repeat( element, 1 );
   }

   /// <summary>
   /// Helper method to filter out <c>null</c> values from arrays.
   /// If the array itself is <c>null</c>, an empty array is returned.
   /// </summary>
   /// <typeparam name="T">The type of array elements.</typeparam>
   /// <param name="array">The array.</param>
   /// <returns>An array where no element is <c>null</c>.</returns>
   /// <remarks>This will always return different array than given one.</remarks>
   public static T[] FilterNulls<T>( this T[] array )
      where T : class
   {
      return array == null ? Empty<T>.Array : array.Where( t => t != null ).ToArray();
   }

   /// <summary>
   /// Helper method to return empty array in case given array is <c>null</c>.
   /// </summary>
   /// <typeparam name="T">The type of array elements.</typeparam>
   /// <param name="array">The array.</param>
   /// <returns>Empty array if <paramref name="array"/> is <c>null</c>; the <paramref name="array"/> if it is not <c>null</c>.</returns>
   /// <remarks>This will return different array only if it is <c>null</c>.</remarks>
   public static T[] EmptyIfNull<T>( this T[] array )
   {
      return array ?? Empty<T>.Array;
   }

   /// <summary>
   /// This is method to quickly fill array with values, utilizing the fact that <see cref="Array.Copy(Array, Array, Int32)"/> methods are very, very fast.
   /// </summary>
   /// <typeparam name="T">The type of array elements.</typeparam>
   /// <param name="destinationArray">The array to be filled with values.</param>
   /// <param name="value">The values to fill array with.</param>
   /// <returns>The <paramref name="destinationArray"/></returns>
   /// <remarks>
   /// Source code is found at <see href="http://stackoverflow.com/questions/5943850/fastest-way-to-fill-an-array-with-a-single-value"/> and <see href="http://coding.grax.com/2014/04/better-array-fill-function.html"/>.
   /// According to first link, "<c>In my test with 20,000,000 array items, this function is twice as fast as a for loop.</c>".
   /// </remarks>
   /// <exception cref="ArgumentNullException">If <paramref name="destinationArray"/> or <paramref name="value"/> are null.</exception>
   /// <exception cref="ArgumentException">If <paramref name="destinationArray"/> is not empty, and length of <paramref name="value"/> is greater than length of <paramref name="destinationArray"/>.</exception>
   public static T[] Fill<T>( this T[] destinationArray, params T[] value )
   {
      return destinationArray.FillWithOffsetAndCount( 0, destinationArray.Length, value );
   }

   /// <summary>
   /// This is method to quickly fill array with values, utilizing the fact that <see cref="Array.Copy(Array, Array, Int32)"/> methods are very, very fast.
   /// </summary>
   /// <typeparam name="T">The type of array elements.</typeparam>
   /// <param name="destinationArray">The array to be filled with values.</param>
   /// <param name="value">The values to fill array with.</param>
   /// <param name="offset">The offset at which to start filling array.</param>
   /// <returns>The <paramref name="destinationArray"/></returns>
   /// <remarks>
   /// Source code is found at <see href="http://stackoverflow.com/questions/5943850/fastest-way-to-fill-an-array-with-a-single-value"/> and <see href="http://coding.grax.com/2014/04/better-array-fill-function.html"/>.
   /// According to first link, "<c>In my test with 20,000,000 array items, this function is twice as fast as a for loop.</c>".
   /// </remarks>
   /// <exception cref="ArgumentNullException">If <paramref name="destinationArray"/> or <paramref name="value"/> are null.</exception>
   /// <exception cref="ArgumentException">If <paramref name="destinationArray"/> is not empty, and length of <paramref name="value"/> is greater than length of <paramref name="destinationArray"/>.</exception>
   public static T[] FillWithOffset<T>( this T[] destinationArray, Int32 offset, params T[] value )
   {
      return destinationArray.FillWithOffsetAndCount( offset, destinationArray.Length - offset, value );
   }

   /// <summary>
   /// This is method to quickly fill array with values, utilizing the fact that <see cref="Array.Copy(Array, Array, Int32)"/> methods are very, very fast.
   /// </summary>
   /// <typeparam name="T">The type of array elements.</typeparam>
   /// <param name="destinationArray">The array to be filled with values.</param>
   /// <param name="value">The values to fill array with.</param>
   /// <param name="offset">The offset at which to start filling array.</param>
   /// <param name="count">How many items to fill.</param>
   /// <returns>The <paramref name="destinationArray"/></returns>
   /// <remarks>
   /// Source code is found at <see href="http://stackoverflow.com/questions/5943850/fastest-way-to-fill-an-array-with-a-single-value"/> and <see href="http://coding.grax.com/2014/04/better-array-fill-function.html"/>.
   /// According to first link, "<c>In my test with 20,000,000 array items, this function is twice as fast as a for loop.</c>".
   /// </remarks>
   /// <exception cref="ArgumentNullException">If <paramref name="destinationArray"/> or <paramref name="value"/> are null.</exception>
   /// <exception cref="ArgumentException">If <paramref name="destinationArray"/> is not empty, and length of <paramref name="value"/> is greater than length of <paramref name="destinationArray"/>.</exception>
   public static T[] FillWithOffsetAndCount<T>( this T[] destinationArray, Int32 offset, Int32 count, params T[] value )
   {
      ArgumentValidator.ValidateNotNull( "Destination array", destinationArray );
      ArgumentValidator.ValidateNotEmpty( "Value array", value );
      destinationArray.CheckArrayArguments( offset, count );

      if ( destinationArray.Length > 0 )
      {
         var max = offset + count;
         if ( value.Length > count )
         {
            throw new ArgumentException( "Length of value array must not be more than count in destination" );
         }

         // set the initial array value
         Array.Copy( value, 0, destinationArray, offset, value.Length );

         Int32 copyLength;

         for ( copyLength = value.Length; copyLength + copyLength < count; copyLength <<= 1 )
         {
            Array.Copy( destinationArray, offset, destinationArray, offset + copyLength, copyLength );
         }

         Array.Copy( destinationArray, offset, destinationArray, offset + copyLength, count - copyLength );
      }

      return destinationArray;
   }

   /// <summary>
   /// This method will return a fast reversed enumerable of a given <see cref="IList{T}"/>, without the buffer overhead of <see cref="Enumerable.Reverse{T}(IEnumerable{T})"/> extension method.
   /// </summary>
   /// <typeparam name="T">The type of list elements.</typeparam>
   /// <param name="list">The <see cref="IList{T}"/>.</param>
   /// <returns>Enumerable that will traverse the <paramref name="list"/> in reversed order, without using any buffers.</returns>
   /// <remarks>The resulting enumerable may break if one removes items between iterations.</remarks>
   /// <exception cref="NullReferenceException">If <paramref name="list"/> is <c>null</c>.</exception>
   public static IEnumerable<T> ReverseFast<T>( this IList<T> list )
   {
      for ( var i = list.Count - 1; i >= 0; --i )
      {
         yield return list[i];
      }
   }

   ///// <summary>
   ///// This method will return a fast reversed enumerable of a given array, without the buffer overhead of <see cref="Enumerable.Reverse{T}(IEnumerable{T})"/> extension method.
   ///// </summary>
   ///// <typeparam name="T">The type of array elements.</typeparam>
   ///// <param name="array">The array.</param>
   ///// <returns>Enumerable that will traverse the <paramref name="array"/> in reversed order, without using any buffers.</returns>
   ///// <exception cref="NullReferenceException">If <paramref name="array"/> is <c>null</c>.</exception>
   //public static IEnumerable<T> ReverseFast<T>( this T[] array )
   //{
   //   for ( var i = array.Length - 1; i >= 0; --i )
   //   {
   //      yield return array[i];
   //   }
   //}

   /// <summary>
   /// Acts like <see cref="Enumerable.FirstOrDefault{T}(IEnumerable{T})"/>, except the value which is returned when there are no elements can be customized.
   /// </summary>
   /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
   /// <param name="enumerable">The enumerable.</param>
   /// <param name="defaultValue">The value to return when there are no elements in the enumerable.</param>
   /// <returns>The first element of the enumerable, or <paramref name="defaultValue"/> if there are no elements in the enumerable.</returns>
   public static T FirstOrDefaultCustom<T>( this IEnumerable<T> enumerable, T defaultValue = default(T) )
   {
      using ( var enumerator = enumerable.GetEnumerator() )
      {
         return enumerator.MoveNext() ?
            enumerator.Current :
            defaultValue;
      }
   }

   /// <summary>
   /// This extension method will make enumerable stop returning more items after it detects a loop in the sequence when enumerating.
   /// </summary>
   /// <typeparam name="T">The type of elements of <see cref="IEnumerable{T}"/>.</typeparam>
   /// <param name="enumerable">The <see cref="IEnumerable{T}"/>.</param>
   /// <param name="equalityComparer">The equality comparer to use when detecting loops. If <c>null</c>, the default will be used.</param>
   /// <returns>Enumerable which will end when it detects a loop.</returns>
   public static IEnumerable<T> EndOnFirstLoop<T>( this IEnumerable<T> enumerable, IEqualityComparer<T> equalityComparer = null )
   {
      var set = new HashSet<T>( equalityComparer );
      foreach ( var item in enumerable )
      {
         if ( set.Add( item ) )
         {
            yield return item;
         }
         else
         {
            yield break;
         }
      }
   }

   /// <summary>
   /// Checks whether two arrays are of same size and they have the same elements.
   /// </summary>
   /// <typeparam name="T">The type of array elements.</typeparam>
   /// <param name="x">The first array.</param>
   /// <param name="y">The second array.</param>
   /// <param name="comparer">The optional equality comparer for array elements.</param>
   /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> are of same size and have same elements; <c>false</c> otherwise.</returns>
   public static Boolean ArraysDeepEquals<T>( this T[] x, T[] y, IEqualityComparer<T> comparer = null )
   {
      var retVal = ReferenceEquals( x, y );
      if ( !retVal && x != null && y != null && x.Length == y.Length && x.Length > 0 )
      {
         if ( comparer == null )
         {
            comparer = EqualityComparer<T>.Default;
         }
         var max = x.Length;
         var i = 0;
         for ( ; i < max && comparer.Equals( x[i], y[i] ); ++i ) ;
         retVal = i == max;
      }

      return retVal;
   }

   /// <summary>
   /// Method for checking whether two arrays are of same size and they have the same elements, when the type of array elements is unknown.
   /// </summary>
   /// <param name="x">The first array.</param>
   /// <param name="y">The second array.</param>
   /// <param name="comparer">The optional equality comparer for array elements.</param>
   /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> are of same size and have same elements; <c>false</c> otherwise.</returns>
   public static Boolean ArraysDeepEqualUntyped( this Array x, Array y, IEqualityComparer<Object> comparer = null )
   {
      var retVal = ReferenceEquals( x, y );
      if ( !retVal && x != null && y != null && x.Length == y.Length && x.Length > 0 && x.Rank == y.Rank )
      {
         if ( comparer == null )
         {
            comparer = EqualityComparer<Object>.Default;
         }
         var max = x.Length;
         var i = 0;
         for ( ; i < max && comparer.Equals( x.GetValue( i ), y.GetValue( i ) ); ++i ) ;
         retVal = i == max;
      }
      return retVal;
   }
}
