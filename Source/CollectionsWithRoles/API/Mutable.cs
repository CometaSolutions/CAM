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

using CollectionsWithRoles.API;

namespace CollectionsWithRoles.API
{
   /// <summary>
   /// This is base interface for all <c>command</c> roles related to interfaces in this namespace.
   /// </summary>
   /// <typeparam name="TMutableQuery">The type of the <c>mutable query</c> role. This role allows access to mutable elements, if this object is a collection.</typeparam>
   /// <typeparam name="TImmutableQuery">The type of the <c>immutable query</c> role. This role allows access to immutable elements, if this objects is a collection.</typeparam>
   public interface Mutable<out TMutableQuery, out TImmutableQuery>
      where TMutableQuery : MutableQuery<TImmutableQuery>
   {
      /// <summary>
      /// Returns the <c>mutable query</c> role. This role allows access to mutable elements, if this object is a collection.
      /// </summary>
      /// <value>The <c>mutable query</c> role. This role allows access to mutable elements, if this object is a collection.</value>
      TMutableQuery MQ { get; }
   }

   /// <summary>
   /// This is base interface for all <c>mutable query</c> roles related to interfaces in this namespace.
   /// </summary>
   /// <typeparam name="ImmutableQueryType">The type of the <c>immutable query</c> role. This role allows access to immutable elements, if this objects is a collection.</typeparam>
   public interface MutableQuery<out ImmutableQueryType>
   {
      /// <summary>
      /// Returns the <c>immutable query</c> role. This role allows access to immutable elements, if this object is a collection.
      /// </summary>
      /// <value>The <c>immutable query</c> role. This role allows access to immutable elements, if this object is a collection.</value>
      ImmutableQueryType IQ { get; }
   }
}

public static partial class E_CWR
{
   internal static TImmutableQuery GetIQ<TMutableQuery, TImmutableQuery>( this Mutable<TMutableQuery, TImmutableQuery> mutable )
      where TMutableQuery : MutableQuery<TImmutableQuery>
   {
      TImmutableQuery retVal;
      if ( mutable == null )
      {
         retVal = default( TImmutableQuery );
      }
      else
      {
         var mq = mutable.MQ;
         retVal = mq == null ? default( TImmutableQuery ) : mq.IQ;
      }
      return retVal;
   }
}