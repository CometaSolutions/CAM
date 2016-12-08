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
using UtilPack;
using UtilPack.Visiting;

namespace UtilPack.Visiting
{
   /// <summary>
   /// This delegate captures signature for methods which perform serialization of objects of type <typeparamref name="TActualElement"/> to <see cref="ResizableArray{T}"/>.
   /// </summary>
   /// <typeparam name="TElement">The common type for all elements in this serialization system.</typeparam>
   /// <typeparam name="TActualElement">The type of element that is being serialized.</typeparam>
   /// <typeparam name="TArrayElement">The type of elements in <see cref="ResizableArray{T}"/>.</typeparam>
   /// <param name="element">The element to serialize.</param>
   /// <param name="array">The <see cref="ResizableArrayIndex{TElement}"/> containing array to write data to.</param>
   /// <param name="arrayIndex">The array index of first free element in <paramref name="array"/>, i.e. index where to start serializing. After the method completes, this parameter should point to next free index in <paramref name="array"/>.</param>
   /// <param name="callback">The callback to serialize hierarchical elements that <paramref name="element"/> may contain.</param>
   public delegate void SerializeWithResizableArrayDelegate<TElement, TActualElement, TArrayElement>( TActualElement element, ResizableArrayIndex<TArrayElement> array, ref Int32 arrayIndex, AcceptVertexExplicitCallbackWithResultDelegate<TElement, ResizableArrayIndex<TArrayElement>, Int32> callback )
      where TActualElement : TElement;

   /// <summary>
   /// This class implements common interface <see cref="SerializationFunctionality{TSerializationSink, TValue}"/> by using <see cref="AcceptorWithContextAndReturnValue{TElement, TContext, TResult}"/>.
   /// </summary>
   /// <typeparam name="TSerializationSink">The type of the object being used to write data to.</typeparam>
   /// <typeparam name="TElement">The common type of all elements in this serialization system.</typeparam>
   /// <typeparam name="TArrayElement">The type of elements in <see cref="ResizableArray{T}"/>.</typeparam>
   public class SerializerWithAcceptor<TSerializationSink, TElement, TArrayElement> : SerializationFunctionality<TSerializationSink, TElement>
   {
      private readonly AcceptorWithContextAndReturnValue<TElement, TSerializationSink, Int32> _acceptor;

      /// <summary>
      /// Creates a new instance of <see cref="SerializerWithAcceptor{TSerializationSink, TElement, TArrayElement}"/> with given acceptor.
      /// </summary>
      /// <param name="acceptor">The acceptor to use.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="acceptor"/> is <c>null</c>.</exception>
      public SerializerWithAcceptor( AcceptorWithContextAndReturnValue<TElement, TSerializationSink, Int32> acceptor )
      {
         this._acceptor = ArgumentValidator.ValidateNotNull( "Acceptor", acceptor );
      }

      /// <summary>
      /// Performs serialization by calling <see cref="AcceptorWithContextAndReturnValue{TElement, TContext, TResult}.Accept(TElement, TContext, out bool)"/> method.
      /// </summary>
      /// <param name="sink">The serialization sink.</param>
      /// <param name="value">The value to serialize.</param>
      /// <returns>The amount of units written.</returns>
      public Int32 Serialize( TSerializationSink sink, TElement value )
      {
         return this._acceptor.Accept( value, sink );
      }
   }

}

public static partial class E_UtilPack
{
   /// <summary>
   /// This is helper method to register functionality which serializes elements of type <typeparamref name="TActualElement"/> using <see cref="ManualTransitionAcceptor_WithContextAndReturnValue{TAcceptor, TElement, TResult, TDelegate}"/>.
   /// </summary>
   /// <typeparam name="TAcceptor">The type of the acceptor.</typeparam>
   /// <typeparam name="TElement">The common type for all elements in this serialization system.</typeparam>
   /// <typeparam name="TActualElement">The type of elements that given callback will serialize.</typeparam>
   /// <typeparam name="TArrayElement">The type of elements in <see cref="ResizableArray{T}"/>.</typeparam>
   /// <param name="acceptor">The <see cref="ManualTransitionAcceptor_WithContextAndReturnValue{TAcceptor, TElement, TResult, TDelegate}"/>.</param>
   /// <param name="callback">The callback to call to perform serialization.</param>
   /// <exception cref="ArgumentNullException">If <paramref name="callback"/> is <c>null</c>.</exception>
   /// <exception cref="NullReferenceException">If this <see cref="ManualTransitionAcceptor_WithContextAndReturnValue{TAcceptor, TElement, TResult, TDelegate}"/> is <c>null</c>.</exception>
   public static void RegisterSerializer<TAcceptor, TElement, TActualElement, TArrayElement>( this ManualTransitionAcceptor_WithContextAndReturnValue<TAcceptor, TElement, Int32, AcceptVertexExplicitWithContextAndResultDelegate<TElement, ResizableArrayIndex<TArrayElement>, Int32>> acceptor, SerializeWithResizableArrayDelegate<TElement, TActualElement, TArrayElement> callback )
      where TActualElement : TElement
   {
      ArgumentValidator.ValidateNotNull( "Serialization callback", callback );
      acceptor.RegisterVertexAcceptor(
         typeof( TActualElement ),
         ( el, ctx, cb ) =>
         {
            var idx = ctx.Index;
            var idxToGive = idx;
            callback( (TActualElement) el, ctx, ref idxToGive, cb );
            // Return amount of units written.
            return idxToGive - idx;
         } );
   }

   /// <summary>
   /// Helper method to call <see cref="AcceptVertexExplicitCallbackWithResultDelegate{TElement, TResult}"/> with <see cref="ResizableArray{T}"/> when serializing hierarchical elements.
   /// </summary>
   /// <typeparam name="TElement">The common type for all elements in this serialization system.</typeparam>
   /// <typeparam name="TArrayElement">The type of elements in <see cref="ResizableArray{T}"/>.</typeparam>
   /// <param name="array">This <see cref="ResizableArray{T}"/>.</param>
   /// <param name="arrayIndex">The array index where to start writing.</param>
   /// <param name="callback">The acceptor callback to serialize the given element.</param>
   /// <param name="element">The element to serialize.</param>
   /// <returns>This <paramref name="array"/>.</returns>
   public static ResizableArray<TArrayElement> SerializeWithAcceptorCallback<TElement, TArrayElement>( this ResizableArray<TArrayElement> array, ref Int32 arrayIndex, TElement element, AcceptVertexExplicitCallbackWithResultDelegate<TElement, ResizableArrayIndex<TArrayElement>, Int32> callback )
   {
      arrayIndex += callback( element, new ResizableArrayIndex<TArrayElement>( array, arrayIndex ) );
      return array;
   }

   /// <summary>
   /// Helper method to call <see cref="AcceptVertexExplicitCallbackWithResultDelegate{TElement, TResult}"/> with array when serializing hierarchical elements.
   /// </summary>
   /// <typeparam name="TElement">The common type for all elements in this serialization system.</typeparam>
   /// <typeparam name="TArrayElement">The type of elements in array.</typeparam>
   /// <param name="array">This array.</param>
   /// <param name="arrayIndex">The array index where to start writing.</param>
   /// <param name="callback">The acceptor callback to serialize the given element.</param>
   /// <param name="element">The element to serialize.</param>
   /// <returns>This <paramref name="array"/>.</returns>
   public static TArrayElement[] SerializeWithAcceptorCallback<TElement, TArrayElement>( this TArrayElement[] array, ref Int32 arrayIndex, TElement element, AcceptVertexExplicitCallbackWithResultDelegate<TElement, ArrayIndex<TArrayElement>, Int32> callback )
   {
      arrayIndex += callback( element, new ArrayIndex<TArrayElement>( array, arrayIndex ) );
      return array;
   }
}
