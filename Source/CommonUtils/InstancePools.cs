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
using System.Threading;

namespace CommonUtils
{
   /// <summary>
   /// This class implements semantics of containing multiple instances of some type, with methods for taking and returning instances.
   /// This class is fully threadsafe and does not use locks in its implementation.
   /// The type must be class or interface.
   /// </summary>
   /// <typeparam name="TInstance">The type of the instances to hold.</typeparam>
   public class LocklessInstancePoolForClasses<TInstance>
      where TInstance : class
   {
      private InstanceHolder<TInstance> _firstInstance;

      /// <summary>
      /// Creates new instance of <see cref="LocklessInstancePoolForClasses{TInstance}"/>.
      /// </summary>
      public LocklessInstancePoolForClasses()
      {
         this._firstInstance = null;
      }

      /// <summary>
      /// Takes an existing instance from this pool and returns it, or returns <c>null</c> if no existing instance is available.
      /// </summary>
      /// <returns>An existing instance of <typeparamref name="TInstance"/>, or <c>null</c> if no existing instance is available.</returns>
      public TInstance TakeInstance()
      {
         InstanceHolder<TInstance> result;
         do
         {
            result = this._firstInstance;
         } while ( result != null && !Object.ReferenceEquals( result, Interlocked.CompareExchange( ref this._firstInstance, result.Next, result ) ) );

         return result == null ? null : result.Instance;
      }

      /// <summary>
      /// Returns an existing instance to this pool. Nothing is done if <paramref name="instance"/> is <c>null</c>.
      /// </summary>
      /// <param name="instance">The instance to return to this pool.</param>
      public void ReturnInstance( TInstance instance )
      {
         if ( instance != null )
         {
            InstanceHolder<TInstance> first;
            var instanceInfo = new InstanceHolder<TInstance>( instance );
            do
            {
               first = this._firstInstance;
               instanceInfo.Next = first;
            } while ( !Object.ReferenceEquals( first, Interlocked.CompareExchange( ref this._firstInstance, instanceInfo, first ) ) );
         }
      }
   }

   /// <summary>
   /// This class implements semantics of containing multiple instances of some type, with methods for taking and returning instances.
   /// This class is fully threadsafe and does not use locks in its implementation.
   /// The type must be class or interface.
   /// </summary>
   /// <typeparam name="TInstance">The type of the instances to hold.</typeparam>
   /// <remarks>
   /// One should use <see cref="LocklessInstancePoolForClasses{TInstance}"/> if <typeparamref name="TInstance"/> is known at compile time never to be a struct.
   /// This class is a bit slower than <see cref="LocklessInstancePoolForClasses{TInstance}"/> and contains different API.
   /// </remarks>
   public class LocklessInstancePoolGeneric<TInstance>
   {
      private InstanceHolder<TInstance> _firstInstance;

      /// <summary>
      /// Creates new instance of <see cref="LocklessInstancePoolGeneric{TInstance}"/>.
      /// </summary>
      public LocklessInstancePoolGeneric()
      {
      }

      /// <summary>
      /// Attempts to remove an instance from this <see cref="LocklessInstancePoolGeneric{TInstance}"/>.
      /// </summary>
      /// <param name="item">When this method returns, this will contain instance of <typeparamref name="TInstance"/> if this method returned <c>true</c>, or default value of <typeparamref name="TInstance"/> if this method returned <c>false</c>.</param>
      /// <returns><c>true</c> if an instance was acquired successfully; <c>false</c> otherwise.</returns>
      public Boolean TryTake( out TInstance item )
      {
         InstanceHolder<TInstance> result;
         do
         {
            result = this._firstInstance;
         } while ( result != null && !Object.ReferenceEquals( result, System.Threading.Interlocked.CompareExchange( ref this._firstInstance, result.Next, result ) ) );
         var retVal = result != null;
         item = retVal ? result.Instance : default( TInstance );
         return retVal;
      }

      /// <summary>
      /// Attemts to fetch an instance from this <see cref="LocklessInstancePoolGeneric{TInstance}"/> without removing it.
      /// </summary>
      /// <param name="item">When this method returns, this will contain instance of <typeparamref name="TInstance"/> if this method returned <c>true</c>, or default value of <typeparamref name="TInstance"/> if this method returned <c>false</c>.</param>
      /// <returns><c>true</c> if an instance was fetched successfully; <c>false</c> otherwise.</returns>
      public Boolean TryPeek( out TInstance item )
      {
         var retVal = this.TryTake( out item );
         if ( retVal )
         {
            this.ReturnInstance( item );
         }
         return retVal;
      }

      /// <summary>
      /// Returns an existing instance to this <see cref="LocklessInstancePoolGeneric{TInstance}"/>.
      /// </summary>
      /// <param name="item">The instance to return.</param>
      public void ReturnInstance( TInstance item )
      {
         var itemHolder = new InstanceHolder<TInstance>( item );
         InstanceHolder<TInstance> first;
         do
         {
            first = this._firstInstance;
            itemHolder.Next = first;
         } while ( !Object.ReferenceEquals( first, System.Threading.Interlocked.CompareExchange( ref this._firstInstance, itemHolder, first ) ) );
      }
   }

   internal sealed class InstanceHolder<TInstance>
   {
      private readonly TInstance _instance;
      private InstanceHolder<TInstance> _next;

      internal InstanceHolder( TInstance instance )
      {
         this._instance = instance;
      }

      internal TInstance Instance
      {
         get
         {
            return this._instance;
         }
      }

      internal InstanceHolder<TInstance> Next
      {
         get
         {
            return this._next;
         }
         set
         {
            Interlocked.Exchange( ref this._next, value );
         }
      }
   }
}
