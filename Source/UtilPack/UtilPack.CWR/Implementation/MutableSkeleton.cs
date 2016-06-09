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
using System.Diagnostics;
using UtilPack.CollectionsWithRoles;
using UtilPack;

namespace UtilPack.CollectionsWithRoles.Implementation
{
   /// <summary>
   /// This is skeleton implementation for any <c>command</c> role to be used with collections that support Command-Query Separation.
   /// </summary>
   /// <typeparam name="TMutableQuery">The type of <c>mutable query</c> role.</typeparam>
   /// <typeparam name="TImmutableQuery">The type of <c>immutable query</c> role.</typeparam>
   [DebuggerDisplay( "{MQ.IQ}" )]
   public class MutableSkeleton<TMutableQuery, TImmutableQuery> : Mutable<TMutableQuery, TImmutableQuery>
      where TMutableQuery : class, MutableQuery<TImmutableQuery>
   {
      private readonly TMutableQuery _mutableQuery;

      /// <summary>
      /// Creates a new <see cref="MutableSkeleton{T,U}"/> object.
      /// </summary>
      /// <param name="mutableQuery">The <c>mutable query</c> role of this object.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="mutableQuery"/> is <c>null</c>.</exception>
      protected MutableSkeleton( TMutableQuery mutableQuery )
      {
         ArgumentValidator.ValidateNotNull( "Mutable query", mutableQuery );

         this._mutableQuery = mutableQuery;
      }

      #region Mutable<MutableQueryRole,ImmutableQueryRole> Members

      /// <inheritdoc/>
      public TMutableQuery MQ
      {
         get
         {
            return this._mutableQuery;
         }
      }

      #endregion

      /// <summary>
      /// Delegates equality to <c>immutable query</c> role if applicable.
      /// </summary>
      /// <param name="obj">Another object.</param>
      /// <returns><c>True</c> if <c>immutable query</c> role for both this and <paramref name="obj"/> returns <c>true</c>; <c>false</c> otherwise.</returns>
      public override Boolean Equals( Object obj )
      {
         return Object.ReferenceEquals( this, obj ) ||
            ( obj is Mutable<TMutableQuery, TImmutableQuery> && this._mutableQuery.IQ.Equals( ( (Mutable<TMutableQuery, TImmutableQuery>) obj ).MQ.IQ ) );
      }

      /// <summary>
      /// Delegates hashcode creation to <c>immutable query</c> role.
      /// </summary>
      /// <returns>The hashcode of <c>immutable query</c> role.</returns>
      public override Int32 GetHashCode()
      {
         return this._mutableQuery.IQ.GetHashCode();
      }

      /// <summary>
      /// Delegates string creation to <c>immutable query</c> role.
      /// </summary>
      /// <returns>The string representation of the <c>immutable query</c> role.</returns>
      public override String ToString()
      {
         return this._mutableQuery.IQ.ToString();
      }
   }

   /// <summary>
   /// This is skeleton implementation for any <c>mutable query</c> role to be used with collections that support Command-Query Separation.
   /// </summary>
   /// <typeparam name="TImmutableQuery">The type of <c>immutable query</c> role.</typeparam>
   [DebuggerDisplay( "{IQ}" )]
   public class MutableQuerySkeleton<TImmutableQuery> : MutableQuery<TImmutableQuery>
      where TImmutableQuery : class
   {
      private readonly TImmutableQuery _immutableQuery;

      /// <summary>
      /// Creates a new <see cref="MutableQuerySkeleton{T}"/> object.
      /// </summary>
      /// <param name="immutableQuery">The <c>immutable query</c> role of this object.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="immutableQuery"/> is <c>null</c>.</exception>
      protected MutableQuerySkeleton( TImmutableQuery immutableQuery )
      {
         ArgumentValidator.ValidateNotNull( "Immutable query", immutableQuery );

         this._immutableQuery = immutableQuery;
      }

      #region MutableQuery<ImmutableQueryRole> Members

      /// <inheritdoc/>
      public TImmutableQuery IQ
      {
         get
         {
            return this._immutableQuery;
         }
      }

      #endregion

      /// <summary>
      /// Delegates equality to <c>immutable query</c> role if applicable.
      /// </summary>
      /// <param name="obj">Another object.</param>
      /// <returns><c>True</c> if <c>immutable query</c> role for both this and <paramref name="obj"/> returns <c>true</c>; <c>false</c> otherwise.</returns>
      public override Boolean Equals( Object obj )
      {
         return Object.ReferenceEquals( this, obj ) ||
            ( obj is MutableQuery<TImmutableQuery> && this._immutableQuery.Equals( ( (MutableQuery<TImmutableQuery>) obj ).IQ ) );
      }

      /// <summary>
      /// Delegates hashcode creation to <c>immutable query</c> role.
      /// </summary>
      /// <returns>The hashcode of <c>immutable query</c> role.</returns>
      public override Int32 GetHashCode()
      {
         return this._immutableQuery.GetHashCode();
      }

      /// <summary>
      /// Delegates string creation to <c>immutable query</c> role.
      /// </summary>
      /// <returns>The string representation of the <c>immutable query</c> role.</returns>
      public override String ToString()
      {
         return this._immutableQuery.ToString();
      }
   }
}
