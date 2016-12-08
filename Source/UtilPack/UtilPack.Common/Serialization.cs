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

namespace UtilPack
{
   /// <summary>
   /// This interface represents functionality to deserialize objects from serialized data.
   /// </summary>
   /// <typeparam name="TDeserializationSource">The type of object from which read data.</typeparam>
   /// <typeparam name="TValue">The common type from which all objects to be deserialized must inherit.</typeparam>
   public interface DeserializationFunctionality<in TDeserializationSource, out TValue>
   {
      /// <summary>
      /// Deserializes an instance of <typeparamref name="TValue"/> from given deserialization source.
      /// </summary>
      /// <param name="source">The source from where to read serialized data.</param>
      /// <param name="unitsProcessed">This should hold the amount of units that this <see cref="DeserializationFunctionality{TDeserializationSource, TValue}"/> processed from <paramref name="source"/>.</param>
      /// <returns>The deserialized value.</returns>
      TValue Deserialize( TDeserializationSource source, out Int32 unitsProcessed );
   }

   /// <summary>
   ///  This interface represents functionality to serialize objects into serialized data.
   /// </summary>
   /// <typeparam name="TSerializationSink">The type of object to which write data.</typeparam>
   /// <typeparam name="TValue">The common type from which all objects to be serialized must inherit.</typeparam>
   public interface SerializationFunctionality<in TSerializationSink, in TValue>
   {

      /// <summary>
      /// Serializes an instance of <typeparamref name="TValue"/> into given serialization sink.
      /// </summary>
      /// <param name="sink">The sink where to write serialized data.</param>
      /// <param name="value">The value to serialize.</param>
      /// <returns>The amount of units that his <see cref="SerializationFunctionality{TSerializationSink, TValue}"/> used to serialize <paramref name="value"/>.</returns>
      Int32 Serialize( TSerializationSink sink, TValue value );
   }

   ///// <summary>
   ///// This is interface for functionality, which provides (de)serialization functionality for objects inheriting from one common type.
   ///// </summary>
   ///// <typeparam name="TDeserializationSource">The type of object from which read data.</typeparam>
   ///// <typeparam name="TSerializationSink">The type of object to which write data.</typeparam>
   ///// <typeparam name="TValue">The common type from which all objects to be (de)serialized must inherit.</typeparam>
   //public interface FullSerializationFunctionality<in TDeserializationSource, in TSerializationSink, TValue> : DeserializationFunctionality<TDeserializationSource, TValue>, SerializationFunctionality<TSerializationSink, TValue>
   //{

   //}

   /// <summary>
   /// This structure represents a single index within a <see cref="ResizableArray{T}"/>.
   /// </summary>
   /// <typeparam name="TElement">The type of the elements in the array.</typeparam>
   public struct ResizableArrayIndex<TElement>
   {
      /// <summary>
      /// Creates a new instance of <see cref="ResizableArrayIndex{TElement}"/> with given <see cref="ResizableArray{T}"/> and index.
      /// </summary>
      /// <param name="array">The <see cref="ResizableArray{T}"/>.</param>
      /// <param name="index">The index within <see cref="ResizableArray{T}"/>.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="array"/> is <c>null</c>.</exception>
      public ResizableArrayIndex( ResizableArray<TElement> array, Int32 index )
      {
         this.Array = ArgumentValidator.ValidateNotNull( "Array", array );
         this.Index = index;
      }

      /// <summary>
      /// Gets the <see cref="ResizableArray{T}"/>.
      /// </summary>
      /// <value>The <see cref="ResizableArray{T}"/>.</value>
      public ResizableArray<TElement> Array { get; }

      /// <summary>
      /// Gets the index within <see cref="Array"/>.
      /// </summary>
      /// <value>The index within <see cref="Array"/>.</value>
      public Int32 Index { get; }
   }

   /// <summary>
   /// This structure represents a single index within a single-dimensional array.
   /// </summary>
   /// <typeparam name="TElement">The type of the elements in the array.</typeparam>
   public struct ArrayIndex<TElement>
   {
      /// <summary>
      /// Creates a new instance of <see cref="ResizableArrayIndex{TElement}"/> with given array and index.
      /// </summary>
      /// <param name="array">The array.</param>
      /// <param name="index">The index within the array.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="array"/> is <c>null</c>.</exception>
      public ArrayIndex( TElement[] array, Int32 index )
      {
         this.Array = ArgumentValidator.ValidateNotNull( "Array", array );
         this.Index = index;
      }

      /// <summary>
      /// Gets the array.
      /// </summary>
      /// <value>The array.</value>
      public TElement[] Array { get; }

      /// <summary>
      /// Gets the index within <see cref="Array"/>.
      /// </summary>
      /// <value>The index within <see cref="Array"/>.</value>
      public Int32 Index { get; }
   }

   ///// <summary>
   ///// This interface binds generic parameters of <see cref="FullSerializationFunctionality{TDeserializationSource, TSerializationSink, TValue}"/> to match when serialization process operates on arrays.
   ///// </summary>
   ///// <typeparam name="TValue">The common type from which all objects to be (de)serialized must inherit.</typeparam>
   ///// <typeparam name="TArrayElement">The type of the array element.</typeparam>
   //public interface ArrayFullSerializationFunctionality<TValue, TArrayElement> : FullSerializationFunctionality<ArrayIndex<TArrayElement>, ResizableArrayIndex<TArrayElement>, TValue>
   //{

   //}

   ///// <summary>
   ///// This interface further binds generic parameters of <see cref="ArrayFullSerializationFunctionality{TValue, TArrayElement}"/> to match when serialization process operates on byte arrays.
   ///// </summary>
   ///// <typeparam name="TValue">The common type from which all objects to be (de)serialized must inherit.</typeparam>
   //public interface BinaryArrayFullSerializationFunctionality<TValue> : ArrayFullSerializationFunctionality<TValue, Byte>
   //{

   //}

   /// <summary>
   /// This class implements <see cref="SerializationFunctionality{TSerializationSink, TValue}"/> based on assumption, that each object to be serialized contains a certain key, and serialization process is further redirected to the sub-serializer based on the key.
   /// </summary>
   /// <typeparam name="TSerializationSink">The type of object to which write data.</typeparam>
   /// <typeparam name="TValue">The common type from which all objects to be serialized must inherit.</typeparam>
   /// <typeparam name="TKey">The type of the key to extract from objects to be serialized.</typeparam>
   public class KeyBasedSerializer<TSerializationSink, TValue, TKey> : SerializationFunctionality<TSerializationSink, TValue>
   {

      private readonly IDictionary<TKey, SerializationFunctionality<TSerializationSink, TValue>> _serializers;
      private readonly Func<TValue, TKey> _extractKey;

      /// <summary>
      /// Creates new instance of <see cref="KeyBasedSerializer{TSerializationSink, TValue, TKey}"/>.
      /// </summary>
      /// <param name="serializers">The dictionary holding sub-serializers based on key.</param>
      /// <param name="extractKey">The callback to extract key from value.</param>
      public KeyBasedSerializer(
         IDictionary<TKey, SerializationFunctionality<TSerializationSink, TValue>> serializers,
         Func<TValue, TKey> extractKey
         )
      {
         this._serializers = ArgumentValidator.ValidateNotNull( "Serializers", serializers );
         this._extractKey = ArgumentValidator.ValidateNotNull( "Extract key", extractKey );
      }

      /// <summary>
      /// This method will extract the key from given <paramref name="value"/> using key extraction callback given to constructor.
      /// If the key is present in given serializer dictionary, the <see cref="SerializationFunctionality{TSerializationSink, TValue}.Serialize"/> will be called for the value.
      /// Otherwise, the <see cref="SerializerNotPresent"/> method will be called.
      /// </summary>
      /// <param name="sink">The sink where to serialize.</param>
      /// <param name="value">The value to serialize.</param>
      /// <returns>The amount of serialization units used by serializing given <paramref name="value"/>.</returns>
      public Int32 Serialize( TSerializationSink sink, TValue value )
      {
         var key = this._extractKey( value );
         SerializationFunctionality<TSerializationSink, TValue> serializer;
         Int32 retVal;
         if ( ( serializer = this.GetSerializer( key ) ) != null )
         {
            retVal = serializer.Serialize( sink, value );
         }
         else
         {
            retVal = this.SerializerNotPresent( key, value );
         }

         return retVal;
      }

      /// <summary>
      /// This method gets the serializer from dictionary provided to constructor of this class.
      /// It may be overridden by subclasses.
      /// </summary>
      /// <param name="key">The key extracted from value.</param>
      /// <returns>The <see cref="SerializationFunctionality{TSerializationSink, TValue}"/> to perform serialization.</returns>
      protected virtual SerializationFunctionality<TSerializationSink, TValue> GetSerializer( TKey key )
      {
         SerializationFunctionality<TSerializationSink, TValue> serializer;
         this._serializers.TryGetValue( key, out serializer );
         return serializer;
      }

      /// <summary>
      /// This method is called by <see cref="Serialize"/> when the actual serializer is not found by key extracted by key extraction callback.
      /// This implementation always throws exception.
      /// </summary>
      /// <param name="key">The extracted key.</param>
      /// <param name="value">The value to serialize.</param>
      /// <returns>The amount of serialization units used by serializing given <paramref name="value"/>.</returns>
      /// <exception cref="Exception">Always.</exception>
      protected virtual Int32 SerializerNotPresent( TKey key, TValue value )
      {
         throw new Exception( "Serializer not present for key: " + key );
      }
   }


   /// <summary>
   /// This class implements <see cref="DeserializationFunctionality{TDeserializationSource, TValue}"/> for situations when the deserialized value may be determined by deserializing a unique key first, and then deducing how to serialize.
   /// </summary>
   /// <typeparam name="TDeserializationSource">The type of object from which read data.</typeparam>
   /// <typeparam name="TValue">The common type from which all objects to be deserialized must inherit.</typeparam>
   /// <typeparam name="TKey">The type of the key being read from deserialization source.</typeparam>
   public class KeyBasedDeserializer<TDeserializationSource, TValue, TKey> : DeserializationFunctionality<TDeserializationSource, TValue>
   {
      /// <summary>
      /// This delegate captures signature of callback used to extract deserialization key.
      /// </summary>
      /// <param name="source">The deserialization source.</param>
      /// <param name="unitsProcessed">How many units has been read.</param>
      /// <param name="newSource"></param>
      /// <returns></returns>
      public delegate TKey ExtractKeyCallback( TDeserializationSource source, out Int32 unitsProcessed, out TDeserializationSource newSource );

      private readonly IDictionary<TKey, DeserializationFunctionality<TDeserializationSource, TValue>> _deserializers;
      private readonly ExtractKeyCallback _extractKey;

      /// <summary>
      /// Creates a new instance of <see cref="KeyBasedDeserializer{TDeserializationSource, TValue, TKey}"/> with given deserializer dictionary and key extraction callback.
      /// </summary>
      /// <param name="deserializers">The deserializer dictionary.</param>
      /// <param name="extractKey">The key extraction callback.</param>
      /// <exception cref="ArgumentNullException">If any of <paramref name="deserializers"/>, <paramref name="extractKey"/> is <c>null</c>.</exception>
      public KeyBasedDeserializer(
         IDictionary<TKey, DeserializationFunctionality<TDeserializationSource, TValue>> deserializers,
         ExtractKeyCallback extractKey
         )
      {
         this._deserializers = ArgumentValidator.ValidateNotNull( "Serializers", deserializers );
         this._extractKey = ArgumentValidator.ValidateNotNull( "Extract key", extractKey );
      }

      /// <summary>
      /// This method will extract the key by using the provided key extraction callback, and then delegate further deserialization to <see cref="DeserializationFunctionality{TDeserializationSource, TValue}"/> provided by <see cref="GetDeserializer(TKey)"/>.
      /// </summary>
      /// <param name="source">The deserialization source.</param>
      /// <param name="unitsProcessed">How many units deserialization process takes.</param>
      /// <returns>The deserialized value.</returns>
      public TValue Deserialize( TDeserializationSource source, out Int32 unitsProcessed )
      {
         var key = this._extractKey( source, out unitsProcessed, out source );
         TValue retVal;
         DeserializationFunctionality<TDeserializationSource, TValue> deserializer;
         Int32 newUnits;
         if ( ( deserializer = this.GetDeserializer( key ) ) != null )
         {
            retVal = deserializer.Deserialize( source, out newUnits );
         }
         else
         {
            retVal = this.DeserializerNotPresent( key, source, out newUnits );
         }

         unitsProcessed += newUnits;
         return retVal;
      }

      /// <summary>
      /// This method gets the deserializer from dictionary provided to constructor of this class.
      /// It may be overridden by subclasses.
      /// </summary>
      /// <param name="key">The key extracted from deserialization source.</param>
      /// <returns>The <see cref="DeserializationFunctionality{TDeserializationSource, TValue}"/> to perform deserialization.</returns>
      protected virtual DeserializationFunctionality<TDeserializationSource, TValue> GetDeserializer( TKey key )
      {
         DeserializationFunctionality<TDeserializationSource, TValue> deserializer;
         this._deserializers.TryGetValue( key, out deserializer );
         return deserializer;
      }

      /// <summary>
      /// This method is called by <see cref="Deserialize"/> when the actual serializer is not found by key extracted by key extraction callback.
      /// This implementation always throws exception.
      /// </summary>
      /// <param name="key">The extracted key.</param>
      /// <param name="source">The deserialization source.</param>
      /// <param name="unitsProcessed">How many units will be used by this method.</param>
      /// <exception cref="Exception">Always.</exception>
      /// <returns>The deserialized value.</returns>
      protected virtual TValue DeserializerNotPresent( TKey key, TDeserializationSource source, out Int32 unitsProcessed )
      {
         throw new Exception( "Serializer not present for key: " + key );
      }
   }

   /// <summary>
   /// This class further specializes the <see cref="KeyBasedDeserializer{TDeserializationSource, TValue, TKey}"/> by assuming that all keys are numerical values.
   /// </summary>
   /// <typeparam name="TDeserializationSource">The type of object from which read data.</typeparam>
   /// <typeparam name="TValue">The common type from which all objects to be deserialized must inherit.</typeparam>
   public class NumericIDBasedDeserializer<TDeserializationSource, TValue> : KeyBasedDeserializer<TDeserializationSource, TValue, Int32>
   {
      private readonly DeserializationFunctionality<TDeserializationSource, TValue>[] _deserializers;
      private readonly Func<Int32, Int32> _processKeyForDictionary;

      /// <summary>
      /// Creates a new <see cref="NumericIDBasedDeserializer{TDeserializationSource, TValue}"/> with given parameters.
      /// </summary>
      /// <param name="deserializers">The deserializer dictionary.</param>
      /// <param name="extractKey">The key extraction callback.</param>
      /// <param name="deserializersArray">The optional array to use for keys which are small enough.</param>
      /// <param name="dictionaryKeyProcessor">The optional callback to use when giving the key to dictionary lookup.</param>
      public NumericIDBasedDeserializer(
         IDictionary<Int32, DeserializationFunctionality<TDeserializationSource, TValue>> deserializers,
         ExtractKeyCallback extractKey,
         DeserializationFunctionality<TDeserializationSource, TValue>[] deserializersArray,
         Func<Int32, Int32> dictionaryKeyProcessor
         ) : base( deserializers, extractKey )
      {
         this._deserializers = deserializersArray ?? Empty<DeserializationFunctionality<TDeserializationSource, TValue>>.Array;
         this._processKeyForDictionary = dictionaryKeyProcessor;
      }

      /// <summary>
      /// This method overrides <see cref="KeyBasedDeserializer{TDeserializationSource, TValue, TKey}.GetDeserializer(TKey)"/> to enable the lookup from array given to constructor.
      /// </summary>
      /// <param name="key">The key extracted by key callback.</param>
      /// <returns>The <see cref="DeserializationFunctionality{TDeserializationSource, TValue}"/> to use to deserialize the value.</returns>
      protected override DeserializationFunctionality<TDeserializationSource, TValue> GetDeserializer( Int32 key )
      {
         return key < this._deserializers.Length ?
            this._deserializers[key] :
            base.GetDeserializer( this._processKeyForDictionary?.Invoke( key ) ?? key );
      }
   }

   //public class ArrayReader<TArrayElement>
   //{
   //   private readonly TArrayElement[] _array;
   //   private Int32 _index;

   //   public ArrayReader( TArrayElement[] array, Int32 startIndex, Int32 bufferSize = 8 )
   //   {
   //      this._array = ArgumentValidator.ValidateNotNull( "Array", array );
   //      this._index = startIndex;
   //      this.Buffer = new TArrayElement[bufferSize];
   //   }

   //   public void ReadIntoCustomBuffer( TArrayElement[] buffer, Int32 start, Int32 count )
   //   {
   //      Array.Copy( this._array, this._index, buffer, start, count );
   //      this._index += count;
   //   }

   //   public TArrayElement[] ReadIntoThisBuffer( Int32 count )
   //   {
   //      var buffer = this.Buffer;
   //      this.ReadIntoCustomBuffer( buffer, 0, count );
   //      return buffer;
   //   }

   //   public TArrayElement[] Buffer { get; }

   //   public Int32 Index
   //   {
   //      get
   //      {
   //         return this._index;
   //      }
   //   }
   //}
}
