/*
* Copyright 2015 Stanislav Muhametsin. All rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
* implied.
*
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CommonUtils
{
   /// <summary>
   /// This is helper class to hold an array, which can be resized.
   /// Unlike <see cref="List{T}"/>, this class provides direct access to the array.
   /// </summary>
   /// <typeparam name="T">The type of elements in the array.</typeparam>
   public class ResizableArray<T>
   {
      private readonly Int32 _maxLimit;
      private Int32 _currentCapacity;
      private T[] _array;

      /// <summary>
      /// Creates a new <see cref="ResizableArray{T}"/> with given initial size and maximum size.
      /// </summary>
      /// <param name="initialSize">The initial size of the array. If this is less than <c>0</c>, then the initial size of the array will be <c>0</c>.</param>
      /// <param name="maxLimit">The maximum limit. If this is less than <c>0</c>, then the array may grow indefinetly.</param>
      public ResizableArray( Int32 initialSize, Int32 maxLimit = -1 )
      {
         this._maxLimit = maxLimit;
         if ( initialSize < 0 )
         {
            initialSize = 0;
         }
         this.EnsureSize( initialSize );
      }

      /// <summary>
      /// Ensures that after calling this method, the array will be at least given size.
      /// </summary>
      /// <param name="size">The desired size for the array. If this is less than <c>0</c>, then this method does nothing.</param>
      /// <remarks>
      /// This method may grow the array, so that the array reference that the array acquired prior to calling this method may no longer reference the same array that is returned by <see cref="Array"/> property after calling this method.
      /// </remarks>
      public void EnsureSize( Int32 size )
      {
         var curCap = this._currentCapacity;
         if ( size > 0 && curCap < size )
         {
            this.EnsureArraySize( size, curCap );
            Interlocked.Exchange( ref this._currentCapacity, size );
         }
      }

      /// <summary>
      /// Gets the maximum size for this <see cref="ResizableArray{T}"/>, as specified in constructor.
      /// </summary>
      public Int32 MaximumSize
      {
         get
         {
            return this._maxLimit;
         }
      }

      /// <summary>
      /// Gets the reference to the current array of this <see cref="ResizableArray{T}"/>.
      /// </summary>
      /// <value>The reference to the current array of this <see cref="ResizableArray{T}"/>.</value>
      /// <remarks>
      /// Calling <see cref="EnsureSize"/> may cause this to return reference to different instance of the array.
      /// </remarks>
      public T[] Array
      {
         get
         {
            return this._array;
         }
      }

      private void EnsureArraySize( Int32 size, Int32 currentCapacity )
      {
         var max = this._maxLimit;
         if ( max < 0 || size < max )
         {
            var array = this._array;
            if ( array == null || array.Length < currentCapacity )
            {
               var newArray = new T[size];
               Interlocked.Exchange( ref this._array, newArray );
               System.Array.Copy( array, 0, newArray, 0, array.Length );
            }
         }
         else
         {
            throw new InvalidOperationException( "The wanted size " + size + " exceeds maximum limit of " + max + " for this resizable array." );
         }
      }

   }
}
