/*
 * Copyright 2016 Stanislav Muhametsin. All rights Reserved.
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

namespace UtilPack.Visiting
{
   public static partial class AcceptorFactory
   {
      ///// <summary>
      ///// Creates new acceptor setup which is suitable for functionality that copies elements.
      ///// </summary>
      ///// <typeparam name="TElement">The common type for all elements in system.</typeparam>
      ///// <typeparam name="TContext">The type of copying context, should be <see cref="CopyingContext"/> or subclass.</typeparam>
      ///// <param name="visitor">The visitor.</param>
      ///// <param name="contextFactory">The callback to create a new instance of context.</param>
      ///// <param name="contextInitializer">The callback to initialize context.</param>
      ///// <returns>New instance of acceptor setup suitable for functionality that copies elements.</returns>
      //public static ManualTransitionAcceptor_WithContextAndReturnValue<AcceptorWithReturnValue<TElement, TElement>, TElement, TElement, AcceptVertexExplicitWithResultDelegate<TElement, TContext, TElement>> NewCopyingAcceptor<TElement, TContext>(
      //   ManualTypeBasedVisitor<TElement> visitor,
      //   Func<TContext> contextFactory,
      //   Action<TElement, TContext> contextInitializer = null
      //   )
      //   where TContext : CopyingContext
      //{
      //   return NewManualAcceptor_WithReturnValue_Caching<TElement, TContext, TElement>( visitor, contextFactory, contextInitializer );
      //}

      /// <summary>
      /// Creates new acceptor setup which is suitable for functionality that copies elements.
      /// </summary>
      /// <typeparam name="TElement">The common type for all elements in system.</typeparam>
      /// <typeparam name="TContext">The type of copying context, should be <see cref="CopyingContext"/> or subclass.</typeparam>
      /// <typeparam name="TAdditionalInfo">The type of argument that Accept method receives.</typeparam>
      /// <param name="visitor">The visitor.</param>
      /// <param name="contextFactory">The callback to create a new instance of context.</param>
      /// <param name="contextInitializer">The callback to initialize context.</param>
      /// <returns>New instance of acceptor setup suitable for functionality that copies elements.</returns>
      public static ManualTransitionAcceptor_WithContextAndReturnValue<AcceptorWithContextAndReturnValue<TElement, TAdditionalInfo, TElement>, TElement, TElement, AcceptVertexExplicitWithResultDelegate<TElement, TContext, TElement>> NewCopyingAcceptor<TElement, TContext, TAdditionalInfo>(
         ManualTypeBasedVisitor<TElement> visitor,
         Func<TContext> contextFactory,
         Action<TElement, TContext, TAdditionalInfo> contextInitializer
         )
         where TContext : CopyingContext
      {
         return NewManualAcceptor_WithHiddenContextAndReturnValue_Caching<TElement, TContext, TElement, TAdditionalInfo>( visitor, contextFactory, contextInitializer );
      }

   }

   /// <summary>
   /// This is base class for functionalities utilizing graph-based visitor pattern to create copies of potentially hierarchical objects.
   /// </summary>
   public class CopyingContext
   {
      /// <summary>
      /// Gets or sets the value indicating whether the copying should be deep.
      /// That is, whether each sub-element should be copied.
      /// </summary>
      /// <value>The value indicating whether the copying should be deep.</value>
      public Boolean IsDeepCopy { get; set; }
   }
}
