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

namespace CommonUtils
{
   /// <summary>
   /// Provides an easy way to create (equality) comparer based on lambdas.
   /// </summary>
   public static class ComparerFromFunctions
   {
      private sealed class EqualityComparerWithFunction<T> : IEqualityComparer<T>, System.Collections.IEqualityComparer
      {
         private readonly Equality<T> _equalsFunc;
         private readonly HashCode<T> _hashCodeFunc;

         internal EqualityComparerWithFunction( Equality<T> equalsFunc, HashCode<T> hashCodeFunc )
         {
            ArgumentValidator.ValidateNotNull( "Equality function", equalsFunc );
            ArgumentValidator.ValidateNotNull( "Hash code function", hashCodeFunc );

            this._equalsFunc = equalsFunc;
            this._hashCodeFunc = hashCodeFunc;
         }

         #region IEqualityComparer<T> Members

         Boolean IEqualityComparer<T>.Equals( T x, T y )
         {
            return this._equalsFunc( x, y );
         }

         Int32 IEqualityComparer<T>.GetHashCode( T obj )
         {
            return this._hashCodeFunc( obj );
         }

         #endregion

         Boolean System.Collections.IEqualityComparer.Equals( Object x, Object y )
         {
            return this._equalsFunc( (T) x, (T) y );
         }

         Int32 System.Collections.IEqualityComparer.GetHashCode( Object obj )
         {
            return this._hashCodeFunc( (T) obj );
         }
      }

      private sealed class ComparerWithFunction<T> : IComparer<T>, System.Collections.IComparer
      {
         private readonly Func<T, T, Int32> _compareFunc;

         internal ComparerWithFunction( Func<T, T, Int32> compareFunc )
         {
            ArgumentValidator.ValidateNotNull( "Comparer function", compareFunc );
            this._compareFunc = compareFunc;
         }

         #region IComparer<T> Members

         Int32 IComparer<T>.Compare( T x, T y )
         {
            return this._compareFunc( x, y );
         }

         #endregion

         Int32 System.Collections.IComparer.Compare( Object x, Object y )
         {
            return this._compareFunc( (T) x, (T) y );
         }
      }

      /// <summary>
      /// Creates a new <see cref="IEqualityComparer{T}"/> which behaves as <paramref name="equals"/> and <paramref name="hashCode"/> callbakcs specify.
      /// </summary>
      /// <typeparam name="T">The type of objects being compared for equality.</typeparam>
      /// <param name="equals">The function for comparing equality for <typeparamref name="T"/>.</param>
      /// <param name="hashCode">The function for calculating hash code for <typeparamref name="T"/>.</param>
      /// <returns>A new <see cref="IEqualityComparer{T}"/> which behaves as parameters specify.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="equals"/> or <paramref name="hashCode"/> is <c>null</c>.</exception>
      /// <remarks>The return value can be casted to <see cref="System.Collections.IEqualityComparer"/>.</remarks>
      public static IEqualityComparer<T> NewEqualityComparer<T>( Equality<T> equals, HashCode<T> hashCode )
      {
         return new EqualityComparerWithFunction<T>( equals, hashCode );
      }

      /// <summary>
      /// Creates a new <see cref="IComparer{T}"/> which behaves as <paramref name="comparerFunc"/> callbacks specify.
      /// </summary>
      /// <typeparam name="T">The type of object being compared.</typeparam>
      /// <param name="comparerFunc">The function comparing the object, should return same as <see cref="IComparer{T}.Compare(T,T)"/> method.</param>
      /// <returns>A new <see cref="IComparer{T}"/> which behaves as parameters specify.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="comparerFunc"/> is <c>null</c>.</exception>
      /// <remarks>The return value can be casted to <see cref="System.Collections.IComparer"/>.</remarks>
      public static IComparer<T> NewComparer<T>( Func<T, T, Int32> comparerFunc )
      {
         return new ComparerWithFunction<T>( comparerFunc );
      }

      /// <summary>
      /// If <typeparamref name="T"/> is an array, then returns a <see cref="ArrayEqualityComparer{T}.DefaultArrayEqualityComparer"/> for array element type.
      /// If <typeparamref name="T"/> is or implements <see cref="IList{T}"/> exactly once, then returns a <see cref="ListEqualityComparer{T,U}.DefaultListEqualityComparer"/> for list element type.
      /// If <typeparamref name="T"/> is or implements <see cref="ICollection{T}"/> exactly once, then returns a <see cref="CollectionEqualityComparer{T,U}.DefaultCollectionEqualityComparer"/> for collection element type.
      /// Otherwise returns <see cref="EqualityComparer{T}.Default"/>.
      /// </summary>
      /// <typeparam name="T">The type of items in sequence.</typeparam>
      /// <returns>The default equality comparer for array element type, keeping an eye on the fact that <typeparamref name="T"/> may be an array, a list, or a collection.</returns>
      /// <remarks>
      /// Since <see cref="ArrayEqualityComparer{T}.DefaultArrayEqualityComparer"/>, <see cref="ListEqualityComparer{T,U}.DefaultListEqualityComparer"/>, and <see cref="CollectionEqualityComparer{T,U}.DefaultCollectionEqualityComparer"/> all use this method, this will recursively travel all types.
      /// </remarks>
      public static IEqualityComparer<T> GetDefaultItemComparerForSomeSequence<T>()
      {
         var t = typeof( T );
         IEqualityComparer<T> retVal;
         if ( t.IsArray )
         {
            retVal = (IEqualityComparer<T>) GetEqualityComparerForArrayElementType( t.GetElementType() );
         }
         else
         {
            Type elementType;
            var iFaces = t.GetInterfaces();
            if ( iFaces
               .Where( iface => Equals( iface.GetGenericDefinitionIfGenericType(), typeof( IList<> ) ) )
               .TryGetSingle( out elementType )
               )
            {
               retVal = (IEqualityComparer<T>) GetEqualityComparerForListElementType( elementType );
            }
            else if ( iFaces
               .Where( iface => Equals( iface.GetGenericDefinitionIfGenericType(), typeof( ICollection<> ) ) )
               .TryGetSingle( out elementType )
            )
            {
               retVal = (IEqualityComparer<T>) GetEqualityComparerForCollectionElementType( elementType );
            }
            else
            {
               retVal = EqualityComparer<T>.Default;
            }
         }
         return retVal;
      }

      private static Object GetEqualityComparerForArrayElementType( Type arrayElementType )
      {
         return typeof( ArrayEqualityComparer<> )
               .MakeGenericType( arrayElementType )
               .GetProperty( nameof( ArrayEqualityComparer<Int32>.DefaultArrayEqualityComparer ), System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static )
               .GetGetMethod()
               .Invoke( null, null );
      }

      private static Object GetEqualityComparerForListElementType( Type listType )
      {
         return typeof( ListEqualityComparer<,> )
            .MakeGenericType( listType, listType.GetGenericArguments()[0] )
            .GetProperty( nameof( ListEqualityComparer<List<Int32>, Int32>.DefaultListEqualityComparer ), System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static )
            .GetGetMethod()
            .Invoke( null, null );
      }

      private static Object GetEqualityComparerForCollectionElementType( Type collectionType )
      {
         return typeof( CollectionEqualityComparer<,> )
            .MakeGenericType( collectionType, collectionType.GetGenericArguments()[0] )
            .GetProperty( nameof( CollectionEqualityComparer<List<Int32>, Int32>.DefaultCollectionEqualityComparer ), System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static )
            .GetGetMethod()
            .Invoke( null, null );
      }
   }
}
