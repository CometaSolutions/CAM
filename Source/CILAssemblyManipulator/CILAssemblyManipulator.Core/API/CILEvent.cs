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
using CILAssemblyManipulator.API;
using CILAssemblyManipulator.Implementation;
using CollectionsWithRoles.API;
using CommonUtils;

namespace CILAssemblyManipulator.API
{
   /// <summary>
   /// This interface represents an event in CIL environment. The interface roughly corresponds to <see cref="System.Reflection.EventInfo"/>. See ECMA specification for more information about CIL events.
   /// </summary>
   public interface CILEvent :
      CILCustomAttributeContainer,
      CILElementWithAttributes<EventAttributes>,
      CILElementOwnedByChangeableType<CILEvent>,
      CILElementWithSimpleName,
      CILElementInstantiable,
      CILElementWithContext
   {
      /// <summary>
      /// Gets or sets the type of the event handler delegate of this event.
      /// </summary>
      /// <value>The type of the event handler delegate of this event.</value>
      /// <exception cref="NotSupportedException">For setter only. The exception is thrown when <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c>, meaning the <see cref="CILElementOwnedByType.DeclaringType"/> is a generic type but not generic type definition.</exception>
      /// <seealso cref="System.Reflection.EventInfo.EventHandlerType"/>
      CILTypeBase EventHandlerType { get; set; }

      /// <summary>
      /// Gets or sets the method to add event handlers to this event.
      /// </summary>
      /// <value>The method to add event handlers to this event.</value>
      /// <exception cref="NotSupportedException">For setter only. The exception is thrown when <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c>, meaning the <see cref="CILElementOwnedByType.DeclaringType"/> is a generic type but not generic type definition.</exception>
      /// <seealso cref="System.Reflection.EventInfo.GetAddMethod()"/>
      CILMethod AddMethod { get; set; }

      /// <summary>
      /// Gets or sets the method to remove event handlers from this event.
      /// </summary>
      /// <value>The method to remove event handlers from this event.</value>
      /// <exception cref="NotSupportedException">For setter only. The exception is thrown when <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c>, meaning the <see cref="CILElementOwnedByType.DeclaringType"/> is a generic type but not generic type definition.</exception>
      /// <seealso cref="System.Reflection.EventInfo.GetRemoveMethod()"/>
      CILMethod RemoveMethod { get; set; }

      /// <summary>
      /// Gets or sets the method to fire this event.
      /// </summary>
      /// <value>The method to fire this event.</value>
      /// <exception cref="NotSupportedException">For setter only. The exception is thrown when <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c>, meaning the <see cref="CILElementOwnedByType.DeclaringType"/> is a generic type but not generic type definition.</exception>
      /// <seealso cref="System.Reflection.EventInfo.GetRaiseMethod()"/>
      CILMethod RaiseMethod { get; set; }

      /// <summary>
      /// Adds other methods related to this event.
      /// </summary>
      /// <param name="methods">Other methods to add.</param>
      /// <exception cref="NotSupportedException">If <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c>, meaning the <see cref="CILElementOwnedByType.DeclaringType"/> is a generic type but not generic type definition.</exception>
      void AddOtherMethods( params CILMethod[] methods );

      /// <summary>
      /// Removes other methods related to this event.
      /// </summary>
      /// <param name="methods">Other methods to remove.</param>
      /// <returns><c>true</c> if at least one method was removed, <c>false</c> otherwise.</returns>
      /// <exception cref="NotSupportedException">If <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c>, meaning the <see cref="CILElementOwnedByType.DeclaringType"/> is a generic type but not generic type definition.</exception>
      Boolean RemoveOtherMethods( params CILMethod[] methods );

      /// <summary>
      /// Gets all other methods currently associated with this event.
      /// </summary>
      /// <value>All other methods currently associated with this event.</value>
      ListQuery<CILMethod> OtherMethods { get; }

      /// <summary>
      /// Gets the synchronization object for concurrent read/write access of other methods via <see cref="AddOtherMethods"/> and <see cref="RemoveOtherMethods"/>.
      /// </summary>
      /// <value>The synchronization object for concurrent read/write access of other methods via <see cref="AddOtherMethods"/> and <see cref="RemoveOtherMethods"/>.</value>
      Object OtherMethodsLock { get; }
   }
}

public static partial class E_CIL
{
   /// <summary>
   /// Returns <c>true</c> if the event is multicast, that is, <see cref="MulticastDelegate"/> is assignable from event's <see cref="CILEvent.EventHandlerType"/>.
   /// </summary>
   /// <param name="evt">The event to check.</param>
   /// <returns><c>true</c> if the event is non-<c>null</c> and multicast, that is, <see cref="MulticastDelegate"/> is assignable from event's <see cref="CILEvent.EventHandlerType"/>; <c>false</c> otherwise.</returns>
   public static Boolean IsMultiCast( this CILEvent evt )
   {
      return evt != null && evt.DeclaringType.Module.AssociatedMSCorLibModule.GetTypeByName( Consts.MULTICAST_DELEGATE ).IsAssignableFrom( evt.EventHandlerType );
   }

   /// <summary>
   /// Gets or creates a new <see cref="CILEvent"/> based on native <see cref="System.Reflection.EventInfo"/>.
   /// </summary>
   /// <param name="eInfo">The native event.</param>
   /// <param name="ctx">The current reflection context.</param>
   /// <returns><see cref="CILEvent"/> wrapping existing native <see cref="System.Reflection.EventInfo"/>.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="eInfo"/> or <paramref name="ctx"/> is <c>null</c>.</exception>
   public static CILEvent NewWrapper( this System.Reflection.EventInfo eInfo, CILReflectionContext ctx )
   {
      ArgumentValidator.ValidateNotNull( "Event info", eInfo );
      ArgumentValidator.ValidateNotNull( "Context", ctx );

      return ( (CILReflectionContextImpl) ctx ).Cache.GetOrAdd( eInfo );
   }

   /// <summary>
   /// Gets all the methods that are semantically related to specified <see cref="CILEvent"/>.
   /// </summary>
   /// <param name="evt">The event which methods must be semantically related to.</param>
   /// <returns>Enumerable of semantic attribute-method pairs.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="evt"/> is <c>null</c>.</exception>
   public static IEnumerable<Tuple<MethodSemanticsAttributes, CILMethod>> GetSemanticMethods( this CILEvent evt )
   {
      ArgumentValidator.ValidateNotNull( "Event", evt );

      var result = evt.AddMethod == null ?
         Enumerable.Empty<Tuple<MethodSemanticsAttributes, CILMethod>>() :
         Enumerable.Repeat( Tuple.Create( MethodSemanticsAttributes.AddOn, evt.AddMethod ), 1 );

      if ( evt.RemoveMethod != null )
      {
         result = result.Concat( Enumerable.Repeat( Tuple.Create( MethodSemanticsAttributes.RemoveOn, evt.RemoveMethod ), 1 ) );
      }
      if ( evt.RaiseMethod != null )
      {
         result = result.Concat( Enumerable.Repeat( Tuple.Create( MethodSemanticsAttributes.Fire, evt.RaiseMethod ), 1 ) );
      }
      result = result.Concat( evt.OtherMethods.Where( method => method != null ).Select( method => Tuple.Create( MethodSemanticsAttributes.Other, method ) ) );
      return result;
   }
}