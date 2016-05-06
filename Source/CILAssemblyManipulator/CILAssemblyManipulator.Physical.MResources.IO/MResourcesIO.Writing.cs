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
using System.IO;
using System.Linq;
using System.Text;
using CommonUtils;
using CollectionsWithRoles.API;
using CILAssemblyManipulator.Physical.MResources;


#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{
   /// <summary>
   /// This method writes this <see cref="ResourceManagerEntry"/> to given <see cref="Stream"/>.
   /// </summary>
   /// <param name="entry">The <see cref="ResourceManagerEntry"/> to write.</param>
   /// <param name="stream">The <see cref="Stream"/> to write <see cref="ResourceManagerEntry"/> to.</param>
   /// <exception cref="ArgumentException">If the <see cref="ResourceManagerEntry.ResourceManagerEntryKind"/> is not one of the values of <see cref="ResourceManagerEntryKind"/> enumeration.</exception>
   public static void WriteEntry( this ResourceManagerEntry entry, Stream stream )
   {
      var kind = entry.ResourceManagerEntryKind;
      switch ( kind )
      {
         case ResourceManagerEntryKind.PreDefined:
            ( (PreDefinedResourceManagerEntry) entry ).WritePreDefinedEntry( stream );
            break;
         case ResourceManagerEntryKind.UserDefined:
            ( (UserDefinedResourceManagerEntry) entry ).WriteUserDefinedEntry( stream );
            break;
         default:
            throw new ArgumentException( "Unrecognized resource manager entry kind: " + kind + "." );
      }
   }

   private static void WritePreDefinedEntry( this PreDefinedResourceManagerEntry entry, Stream stream )
   {
      var array = new ResizableArray<Byte>( 8 );
      var idx = 0;
      var val = entry.Value;
      var tc = ResourceManagerEntryInformation.GetResourceTypeCodeForObject( val );
      if ( tc.HasValue )
      {
         var writeArray = true;
         switch ( tc.Value )
         {
            case ResourceTypeCode.Null:
               break;
            case ResourceTypeCode.String:
               var str = (String) val;
               var byteCount = UTF8.GetByteCount( str );
               array.CurrentMaxCapacity = 5 + byteCount;
               array.Array
                  .WriteInt32LEEncoded7Bit( ref idx, byteCount )
                  .WriteStringToBytes( ref idx, UTF8, str );
               break;
            case ResourceTypeCode.Boolean:
               array.Array.WriteByteToBytes( ref idx, (Byte) ( (Boolean) val ? 1 : 0 ) );
               break;
            case ResourceTypeCode.Char:
               array.Array.WriteUInt16LEToBytes( ref idx, (UInt16) ( (Char) val ) );
               break;
            case ResourceTypeCode.Byte:
               array.Array.WriteByteToBytes( ref idx, (Byte) val );
               break;
            case ResourceTypeCode.SByte:
               array.Array.WriteSByteToBytes( ref idx, (SByte) val );
               break;
            case ResourceTypeCode.Int16:
               array.Array.WriteInt16LEToBytes( ref idx, (Int16) val );
               break;
            case ResourceTypeCode.UInt16:
               array.Array.WriteUInt16LEToBytes( ref idx, (UInt16) val );
               break;
            case ResourceTypeCode.Int32:
               array.Array.WriteInt32LEToBytes( ref idx, (Int32) val );
               break;
            case ResourceTypeCode.UInt32:
               array.Array.WriteUInt32LEToBytes( ref idx, (UInt32) val );
               break;
            case ResourceTypeCode.Int64:
               array.Array.WriteInt64LEToBytes( ref idx, (Int64) val );
               break;
            case ResourceTypeCode.UInt64:
               array.Array.WriteUInt64LEToBytes( ref idx, (UInt64) val );
               break;
            case ResourceTypeCode.Single:
               array.Array.WriteSingleLEToBytes( ref idx, (Single) val );
               break;
            case ResourceTypeCode.Double:
               array.Array.WriteDoubleLEToBytes( ref idx, (Double) val );
               break;
            case ResourceTypeCode.Decimal:
               array.WriteResourceManagerDecimal_AsResourceManagerEntry( ref idx, (Decimal) val );
               break;
            case ResourceTypeCode.DateTime:
               array.WriteResourceManagerDateTime_AsResourceManagerEntry( ref idx, (DateTime) val );
               break;
            case ResourceTypeCode.TimeSpan:
               array.WriteResourceManagerTimeSpan_AsResourceManagerEntry( ref idx, (TimeSpan) val );
               break;
            case ResourceTypeCode.ByteArray:
               var bytes = (Byte[]) val;
               var len = bytes.Length;
               array.CurrentMaxCapacity = len + sizeof( Int32 );
               array.Array
                  .WriteInt32LEToBytes( ref idx, len )
                  .BlockCopyFrom( ref idx, bytes );
               break;
            case ResourceTypeCode.Stream:
               var sourceStream = (Stream) val;
               sourceStream.Position = 0;
               array.Array.WriteInt32LEToBytes( ref idx, (Int32) sourceStream.Length );
               stream.Write( array.Array, idx );
               writeArray = false;
               sourceStream.CopyTo( stream );
               break;
            default:
               throw new ManifestResourceSerializationException( "Unrecognized resource type code: " + tc.Value + "." );
         }

         if ( writeArray )
         {
            stream.Write( array.Array, idx );
         }
      }
      else
      {
         throw new ManifestResourceSerializationException( "The type " + val.GetType() + " is not one of the pre-defined types." );
      }
   }

   private static void WriteUserDefinedEntry( this UserDefinedResourceManagerEntry entry, Stream stream )
   {
      var state = new SerializationState( stream );
      // Write header
      state.EnsureCapacity( 17 ); // Header + header value
      state.array
         .WriteByteToBytes( ref state.idx, (Byte) RecordTypeEnumeration.SerializedStreamHeader )
         .WriteInt32LEToBytes( ref state.idx, 1 ) // ID of "top object"
         .WriteInt32LEToBytes( ref state.idx, -1 ) // ID of header
         .WriteInt32LEToBytes( ref state.idx, 1 ) // Formatter major version
         .WriteInt32LEToBytes( ref state.idx, 0 ); // Formatter minor version
      state.WriteArrayToStream();

      // Collect all assembly names
      var rec = entry.Contents;

      // Write used assembly names first
      WriteAssemblyNames( state, rec );

      // Write record structure
      WriteSingleRecord( state, rec, false );

      // Empty queue of reference objects
      while ( state.recordQueue.Count > 0 )
      {
         var item = state.recordQueue.Dequeue();
         WriteSingleRecord( state, item as AbstractRecord, true, item );
      }

      // Write end
      state.EnsureCapacity( 1 );
      state.array.WriteByteToBytes( ref state.idx, (Byte) RecordTypeEnumeration.MessageEnd );
      state.WriteArrayToStream();
   }

   private static void WriteAssemblyNames( SerializationState state, AbstractRecord curRecord )
   {
      if ( curRecord != null )
      {
         switch ( curRecord.RecordKind )
         {
            case RecordKind.Class:
               var claas = (ClassRecord) curRecord;
               WriteAssemblyName( state, claas );
               foreach ( var member in claas.Members )
               {
                  WriteAssemblyName( state, member );
                  WriteAssemblyNames( state, member.Value as AbstractRecord );
               }
               break;
            case RecordKind.Array:
               var array = (ArrayRecord) curRecord;
               WriteAssemblyName( state, array );
               foreach ( var elem in array.ValuesAsVector )
               {
                  WriteAssemblyNames( state, elem as AbstractRecord );
               }
               break;
         }
      }
   }

   private static void WriteAssemblyName( SerializationState state, ElementWithTypeInfo element )
   {
      if ( element != null )
      {
         var assName = element.AssemblyName;
         Int32 id;
         if ( state.TryAddAssemblyName( assName, out id ) )
         {
            var strByteCount = UTF8.GetByteCount( assName );
            state.EnsureCapacity( 10 + strByteCount );
            state.array
               .WriteByteToBytes( ref state.idx, (Byte) RecordTypeEnumeration.BinaryLibrary )
               .WriteInt32LEToBytes( ref state.idx, id )
               .WriteInt32LEEncoded7Bit( ref state.idx, strByteCount )
               .WriteStringToBytes( ref state.idx, UTF8, assName );
            state.WriteArrayToStream();
         }
      }

   }

   private static void WriteSingleRecord( SerializationState state, AbstractRecord record, Boolean forceWrite, Object primitiveValue = null )
   {
      Int32 id;
      var str = primitiveValue as String;
      if ( state.TryAddRecord( (Object) record ?? str, out id ) || forceWrite )
      {
         // If record hasn't been previously processed, or if we are told to write contents no matter what
         if ( record == null )
         {
            var len = UTF8.GetByteCount( str );
            state.EnsureCapacity( 10 + len );
            state.array
               .WriteByteToBytes( ref state.idx, (Byte) RecordTypeEnumeration.BinaryObjectString )
               .WriteInt32LEToBytes( ref state.idx, id )
               .WriteInt32LEEncoded7Bit( ref state.idx, len )
               .WriteStringToBytes( ref state.idx, UTF8, str );
            state.WriteArrayToStream();
         }
         else
         {
            switch ( record.RecordKind )
            {
               case RecordKind.Class:
                  WriteClassRecord( state, (ClassRecord) record, id );
                  break;
               case RecordKind.Array:
                  WriteArrayRecord( state, (ArrayRecord) record, id );
                  break;
            }
         }

      }
      else
      {
         if ( record == null && str == null )
         {
            if ( primitiveValue == null )
            {
               state.EnsureCapacity( 1 );
               state.array.WriteByteToBytes( ref state.idx, (Byte) RecordTypeEnumeration.ObjectNull );
               state.WriteArrayToStream();
            }
            else
            {
               // Write header
               state.EnsureCapacity( 2 );
               var pType = GetPrimitiveType( primitiveValue );
               state.array
                  .WriteByteToBytes( ref state.idx, (Byte) RecordTypeEnumeration.MemberPrimitiveTyped )
                  .WriteByteToBytes( ref state.idx, (Byte) pType );
               state.WriteArrayToStream();
               // Write primitive
               WritePrimitive( state, primitiveValue, pType );
            }
         }
         else
         {
            // Record was already serialized, write member reference to it
            state.EnsureCapacity( 5 );
            state.array
               .WriteByteToBytes( ref state.idx, (Byte) RecordTypeEnumeration.MemberReference )
               .WriteInt32LEToBytes( ref state.idx, id );
            state.WriteArrayToStream();
         }
      }
   }

   private static void WriteClassRecord( SerializationState state, ClassRecord claas, Int32 id )
   {
      var metaDataKey = Tuple.Create( claas.AssemblyName, claas.TypeName );
      Tuple<BinaryTypeEnumeration, PrimitiveTypeEnumeration>[] mTypeCodes;
      Tuple<Int32, Tuple<BinaryTypeEnumeration, PrimitiveTypeEnumeration>[]> otherID;
      if ( state.serializedObjects.TryGetValue( metaDataKey, out otherID ) )
      {
         // Another record of the same type was serialized earlier, can use previous info
         state.EnsureCapacity( 9 );
         state.array
            .WriteByteToBytes( ref state.idx, (Byte) RecordTypeEnumeration.ClassWithID )
            .WriteInt32LEToBytes( ref state.idx, id )
            .WriteInt32LEToBytes( ref state.idx, otherID.Item1 );
         state.WriteArrayToStream();
         mTypeCodes = otherID.Item2;
      }
      else
      {
         var isSystem = claas.AssemblyName == null;
         var nameByteCount = SafeByteCount( claas.TypeName );
         state.EnsureCapacity( 14 + nameByteCount ); // class type (1), id (4), space for class name length (max 5), member count (4)
         state.array
            .WriteByteToBytes( ref state.idx, (Byte) ( isSystem ? RecordTypeEnumeration.SystemClassWithMembersAndTypes : RecordTypeEnumeration.ClassWithMembersAndTypes ) )
            .WriteInt32LEToBytes( ref state.idx, id )
            .WriteInt32LEEncoded7Bit( ref state.idx, nameByteCount )
            .WriteStringToBytes( ref state.idx, UTF8, claas.TypeName )
            .WriteInt32LEToBytes( ref state.idx, claas.Members.Count );
         state.WriteArrayToStream();

         // Write member names
         foreach ( var member in claas.Members )
         {
            nameByteCount = SafeByteCount( member.Name );
            state.EnsureCapacity( 5 + nameByteCount );
            state.array
               .WriteInt32LEEncoded7Bit( ref state.idx, nameByteCount )
               .WriteStringToBytes( ref state.idx, UTF8, member.Name );
            state.WriteArrayToStream();
         }
         // Write member type infos
         state.EnsureCapacity( claas.Members.Count );
         mTypeCodes = new Tuple<BinaryTypeEnumeration, PrimitiveTypeEnumeration>[claas.Members.Count];
         for ( var i = 0; i < claas.Members.Count; ++i )
         {
            var member = claas.Members[i];
            PrimitiveTypeEnumeration pType;
            var bt = GetTypeInfo( member.Value, out pType );
            mTypeCodes[i] = Tuple.Create( bt, pType );
            state.array.WriteByteToBytes( ref state.idx, (Byte) bt );
         }
         state.WriteArrayToStream();
         state.serializedObjects.Add( metaDataKey, Tuple.Create( id, mTypeCodes ) );

         // Write additional type info where applicable
         for ( var i = 0; i < mTypeCodes.Length; ++i )
         {
            var member = claas.Members[i];
            var tuple = mTypeCodes[i];
            WriteAdditionalTypeInfo( state, tuple.Item1, tuple.Item2, member );
         }

         // Write this class assembly name if needed
         if ( !isSystem )
         {
            state.EnsureCapacity( 4 );
            state.array.WriteInt32LEToBytes( ref state.idx, state.assemblies[claas.AssemblyName] );
            state.WriteArrayToStream();
         }
      }

      // Write member values
      for ( var i = 0; i < mTypeCodes.Length; ++i )
      {
         var member = claas.Members[i];
         var val = member.Value;
         var typeInfo = mTypeCodes[i];
         var binaryType = typeInfo.Item1;
         if ( binaryType == BinaryTypeEnumeration.Primitive )
         {
            WritePrimitive( state, val, typeInfo.Item2 );
         }
         else
         {
            if ( binaryType == BinaryTypeEnumeration.String )
            {
               WriteSingleRecord( state, null, false, val );
            }
            else
            {
               if ( state.TryAddRecord( val, out id ) )
               {
                  // The record hasn't been serialized, add to queue
                  // This will force member references be serialized instead of member values being serialized in-place
                  state.recordQueue.Enqueue( val );
               }
               WriteSingleRecord( state, (AbstractRecord) val, false );
            }
         }


      }
   }

   private static void WriteArrayRecord( SerializationState state, ArrayRecord array, Int32 id )
   {
      var rank = Math.Max( 1, array.Rank );
      var kind = array.ArrayKind;
      var values = array.ValuesAsVector;
      var typeName = array.TypeName;
      var assemblyName = array.AssemblyName;
      RecordTypeEnumeration recType;
      Type pType = null;
      if ( kind == BinaryArrayTypeEnumeration.Single
         && String.IsNullOrEmpty( assemblyName )
         && String.IsNullOrEmpty( typeName )
         )
      {
         recType = GetSingleArrayRecordType( values, out pType );
         WriteArrayRecord_Single( state, id, values, recType, GetPrimitiveTypeFromType( pType ) );
      }
      else
      {
         WriteArrayRecord_Other( state, array, id );
      }
   }

   private static void WriteArrayRecord_Single(
      SerializationState state,
      Int32 id,
      List<Object> values,
      RecordTypeEnumeration recType,
      PrimitiveTypeEnumeration pEnum
      )
   {
      state.EnsureCapacity( 9 );
      state.array
         .WriteByteToBytes( ref state.idx, (Byte) recType )
         .WriteInt32LEToBytes( ref state.idx, id )
         .WriteInt32LEToBytes( ref state.idx, values.Count );
      state.WriteArrayToStream();

      switch ( recType )
      {
         case RecordTypeEnumeration.ArraySinglePrimitive:
            state.EnsureCapacity( 1 );
            state.array.WriteByteToBytes( ref state.idx, (Byte) pEnum );
            state.WriteArrayToStream();
            WriteArrayValues( state, values, obj => WritePrimitive( state, obj, pEnum ) );
            break;
         case RecordTypeEnumeration.ArraySingleObject:
            WriteArrayValues( state, values, obj =>
            {
               WriteSingleRecord( state, obj as AbstractRecord, false, obj );
            } );
            break;
         case RecordTypeEnumeration.ArraySingleString:
            WriteArrayValues( state, values, obj =>
            {
               WriteSingleRecord( state, null, false, obj );
            } );
            break;
         default:
            throw new ManifestResourceSerializationException( "Invalid single array record kind:" + recType + "." );
      }
   }

   private static void WriteArrayRecord_Other(
      SerializationState state,
      ArrayRecord array,
      Int32 id
      )
   {
      state.EnsureCapacity( 5 );
      state.array
         .WriteByteToBytes( ref state.idx, (Byte) RecordTypeEnumeration.BinaryArray )
         .WriteInt32LEToBytes( ref state.idx, id );
      state.WriteArrayToStream();

      var rank = array.Rank;
      var cap = 7 + 4 * rank; // array type (1), rank (4), rank lengths (4 each) + type info (1) + possible primitive info
      var hasOffset = false;
      var arrayKind = array.ArrayKind;
      switch ( arrayKind )
      {
         case BinaryArrayTypeEnumeration.SingleOffset:
         case BinaryArrayTypeEnumeration.JaggedOffset:
         case BinaryArrayTypeEnumeration.RectangularOffset:
            hasOffset = true;
            cap += 4 * rank; // rank offsets (4 each);
            break;
      }
      state.EnsureCapacity( cap );
      state.array
         .WriteByteToBytes( ref state.idx, (Byte) arrayKind )
         .WriteInt32LEToBytes( ref state.idx, rank );
      for ( var i = 0; i < rank; ++i )
      {
         state.array.WriteInt32LEToBytes( ref state.idx, array.Lengths[i] );
      }
      if ( hasOffset )
      {
         for ( var i = 0; i < rank; ++i )
         {
            state.array.WriteInt32LEToBytes( ref state.idx, array.LowerBounds[i] );
         }
      }

      var typeName = array.TypeName;
      var assemblyName = array.AssemblyName;
      var values = array.ValuesAsVector;
      Type pType = null;
      BinaryTypeEnumeration typeEnum;
      if ( !String.IsNullOrEmpty( typeName ) )
      {
         if ( String.IsNullOrEmpty( assemblyName ) )
         {
            typeEnum = BinaryTypeEnumeration.SystemClass;
         }
         else
         {
            typeEnum = BinaryTypeEnumeration.Class;
         }
      }
      else
      {
         var singleKind = GetSingleArrayRecordType( values, out pType );
         switch ( singleKind )
         {
            case RecordTypeEnumeration.ArraySinglePrimitive:
               typeEnum = BinaryTypeEnumeration.Primitive;
               break;
            case RecordTypeEnumeration.ArraySingleObject:
               // This can be: Object, ObjectArray, StringArray, PrimitiveArray
               // It is something else than Object only when all values are non-null ArrayRecords
               var valueArrayInfo = values.Select( v =>
               {
                  var arr = v as ArrayRecord;
                  Type arrPType;
                  return arr == null ? null : Tuple.Create( GetSingleArrayRecordType( arr.ValuesAsVector, out arrPType ), arrPType );
               } );
               if ( valueArrayInfo.All( v => v != null ) )
               {
                  if ( valueArrayInfo.All( v => v.Item1 == RecordTypeEnumeration.ArraySingleString ) )
                  {
                     typeEnum = BinaryTypeEnumeration.StringArray;
                  }
                  else
                  {
                     Tuple<RecordTypeEnumeration, Type> first;
                     typeEnum = valueArrayInfo.EmptyOrAllEqual( out first ) && first.Item1 == RecordTypeEnumeration.ArraySinglePrimitive ?
                        BinaryTypeEnumeration.PrimitiveArray :
                        BinaryTypeEnumeration.ObjectArray;
                     if ( typeEnum == BinaryTypeEnumeration.PrimitiveArray )
                     {
                        pType = first.Item2;
                     }
                  }
               }
               else
               {
                  typeEnum = BinaryTypeEnumeration.Object;
               }
               break;
            case RecordTypeEnumeration.ArraySingleString:
               typeEnum = BinaryTypeEnumeration.String;
               break;
            default:
               throw new InvalidOperationException( "The code to detect array type has changed and this switch clause wasn't adjusted appropriately." );
         }
      }
      state.array.WriteByteToBytes( ref state.idx, (Byte) typeEnum );
      state.WriteArrayToStream();

      var pEnum = GetPrimitiveTypeFromType( pType );
      WriteAdditionalTypeInfo( state, typeEnum, pEnum, array );

      WriteArrayValues( state, values, obj =>
      {
         if ( BinaryTypeEnumeration.Primitive == typeEnum )
         {
            WritePrimitive( state, obj, pEnum );
         }
         else
         {
            WriteSingleRecord( state, obj as AbstractRecord, false, obj );
         }
      } );
   }

   private static void WriteAdditionalTypeInfo(
      SerializationState state,
      BinaryTypeEnumeration typeEnum,
      PrimitiveTypeEnumeration pType,
      ElementWithTypeInfo element
      )
   {
      Int32 nameByteCount;
      String str;
      switch ( typeEnum )
      {
         case BinaryTypeEnumeration.Primitive:
            state.EnsureCapacity( 1 );
            state.array.WriteByteToBytes( ref state.idx, (Byte) pType );
            break;
         case BinaryTypeEnumeration.SystemClass:
            str = element.TypeName;
            nameByteCount = SafeByteCount( str );
            state.EnsureCapacity( 5 + nameByteCount );
            state.array
               .WriteInt32LEEncoded7Bit( ref state.idx, nameByteCount )
               .WriteStringToBytes( ref state.idx, UTF8, str );
            break;
         case BinaryTypeEnumeration.Class:
            str = element.TypeName;
            nameByteCount = SafeByteCount( str );
            state.EnsureCapacity( 9 + nameByteCount );
            state.array
               .WriteInt32LEEncoded7Bit( ref state.idx, nameByteCount )
               .WriteStringToBytes( ref state.idx, UTF8, str )
               .WriteInt32LEToBytes( ref state.idx, state.assemblies[element.AssemblyName] );
            break;
         case BinaryTypeEnumeration.PrimitiveArray:
            state.EnsureCapacity( 1 );
            state.array
               .WriteByteToBytes( ref state.idx, (Byte) pType );
            break;
      }
      state.WriteArrayToStream();
   }

   private static RecordTypeEnumeration GetSingleArrayRecordType( List<Object> values, out Type pType )
   {
      RecordTypeEnumeration recType;
      pType = null;
      if ( values.Count > 0 )
      {
         recType = 0;
         foreach ( var val in values )
         {
            if ( val != null
               && !( val is AbstractRecord )
               && ( pType == null || Equals( pType, val.GetType() ) )
               )
            {
               recType = RecordTypeEnumeration.ArraySinglePrimitive;
               if ( pType == null )
               {
                  pType = val.GetType();
               }
            }
            else
            {
               recType = RecordTypeEnumeration.ArraySingleObject;
            }

            if ( recType == RecordTypeEnumeration.ArraySingleObject )
            {
               break;
            }
         }

         if ( recType == RecordTypeEnumeration.ArraySinglePrimitive && Equals( typeof( String ), pType ) )
         {
            recType = RecordTypeEnumeration.ArraySingleString;
         }
      }
      else
      {
         // Empty arrays are serialized like this
         recType = RecordTypeEnumeration.ArraySinglePrimitive;
      }

      return recType;
   }

   private static void WriteArrayValues( SerializationState state, IEnumerable<Object> elements, Action<Object> nonNullAction )
   {
      var nullCount = 0u;
      foreach ( var elem in elements )
      {
         if ( elem == null )
         {
            ++nullCount;
         }
         else
         {
            WriteNullFillers( state, nullCount );
            nonNullAction( elem );
            nullCount = 0u;
         }
      }
      WriteNullFillers( state, nullCount );
   }

   private static void WriteNullFillers( SerializationState state, UInt32 nullCount )
   {
      if ( nullCount > 0 )
      {
         if ( nullCount > 1 )
         {
            var useByte = nullCount <= Byte.MaxValue;
            state.EnsureCapacity( useByte ? 2 : 5 );
            state.array.WriteByteToBytes( ref state.idx, (Byte) ( useByte ? RecordTypeEnumeration.ObjectNullMultiple256 : RecordTypeEnumeration.ObjectNullMultiple ) );
            if ( useByte )
            {
               state.array.WriteByteToBytes( ref state.idx, (Byte) nullCount );
            }
            else
            {
               state.array.WriteUInt32LEToBytes( ref state.idx, nullCount );
            }
         }
         else
         {
            // Just write one null
            state.EnsureCapacity( 1 );
            state.array.WriteByteToBytes( ref state.idx, (Byte) RecordTypeEnumeration.ObjectNull );
         }
         state.WriteArrayToStream();
      }
   }

   private static BinaryTypeEnumeration GetTypeInfo( Object obj, out PrimitiveTypeEnumeration pType )
   {
      pType = (PrimitiveTypeEnumeration) 255;
      if ( obj == null )
      {
         pType = PrimitiveTypeEnumeration.Null;
         return BinaryTypeEnumeration.Object;
      }
      else
      {
         switch ( Type.GetTypeCode( obj.GetType() ) )
         {
            case TypeCode.Object:
               var rec = obj as AbstractRecord;
               if ( rec != null )
               {
                  if ( !String.IsNullOrEmpty( rec.TypeName ) )
                  {
                     return String.IsNullOrEmpty( rec.AssemblyName ) ? BinaryTypeEnumeration.SystemClass : BinaryTypeEnumeration.Class;
                  }
                  else
                  {
                     switch ( rec.RecordKind )
                     {
                        case RecordKind.Class:
                           return BinaryTypeEnumeration.Object;
                        case RecordKind.Array:
                           var array = (ArrayRecord) obj;
                           if ( array.ArrayKind == BinaryArrayTypeEnumeration.Single )
                           {
                              Type primitiveType;
                              var recType = GetSingleArrayRecordType( array.ValuesAsVector, out primitiveType );
                              pType = GetPrimitiveTypeFromType( primitiveType );
                              switch ( recType )
                              {
                                 case RecordTypeEnumeration.ArraySingleObject:
                                    return BinaryTypeEnumeration.ObjectArray;
                                 case RecordTypeEnumeration.ArraySinglePrimitive:
                                    return BinaryTypeEnumeration.PrimitiveArray;
                                 case RecordTypeEnumeration.ArraySingleString:
                                    return BinaryTypeEnumeration.StringArray;
                                 default:
                                    throw new ManifestResourceSerializationException( "Unrecognized single array kind: " + recType + "." );
                              }
                              //var firstNonNull = array.ValuesAsVector.FirstOrDefault( o => o != null );
                              //switch ( GetTypeInfo( firstNonNull, out pType ) )
                              //{
                              //   case BinaryTypeEnumeration.String:
                              //      return BinaryTypeEnumeration.StringArray;
                              //   case BinaryTypeEnumeration.Primitive:
                              //      return BinaryTypeEnumeration.PrimitiveArray;
                              //   default:
                              //      // If firstNonNull == null then all of them are nulls => PrimitiveArray with PrimitiveTypeEnumeration.Null
                              //      return firstNonNull == null ? BinaryTypeEnumeration.PrimitiveArray : BinaryTypeEnumeration.ObjectArray;
                              //}
                           }
                           else
                           {
                              return BinaryTypeEnumeration.ObjectArray;
                           }
                        default:
                           throw new NotSupportedException( "Unknown record kind " + rec.RecordKind + "." );

                     }
                  }
               }
               else if ( obj is TimeSpan )
               {
                  pType = PrimitiveTypeEnumeration.TimeSpan;
                  return BinaryTypeEnumeration.Primitive;
               }
               else
               {
                  throw new InvalidOperationException( "Only primitives and AbstractRecords allowed as values. Encountered " + obj + " as value." );
               }
            case TypeCode.String:
               pType = PrimitiveTypeEnumeration.String;
               return BinaryTypeEnumeration.String;
            default:
               pType = GetPrimitiveType( obj );
               return BinaryTypeEnumeration.Primitive;
         }
      }
   }

   private static PrimitiveTypeEnumeration GetPrimitiveType( Object obj )
   {
      if ( obj == null )
      {
         return PrimitiveTypeEnumeration.Null;
      }
      else
      {
         return GetPrimitiveTypeFromType( obj.GetType() );
      }
   }

   private static PrimitiveTypeEnumeration GetPrimitiveTypeFromType( Type type )
   {
      switch ( Type.GetTypeCode( type ) )
      {
         case TypeCode.Empty:
            return PrimitiveTypeEnumeration.Null;
         case TypeCode.Object:
            if ( Object.Equals( typeof( TimeSpan ), type ) )
            {
               return PrimitiveTypeEnumeration.TimeSpan;
            }
            else
            {
               throw new InvalidOperationException( "Object type " + type + " is not primitive." );
            }
         case TypeCode.Boolean:
            return PrimitiveTypeEnumeration.Boolean;
         case TypeCode.Char:
            return PrimitiveTypeEnumeration.Char;
         case TypeCode.SByte:
            return PrimitiveTypeEnumeration.SByte;
         case TypeCode.Byte:
            return PrimitiveTypeEnumeration.Byte;
         case TypeCode.Int16:
            return PrimitiveTypeEnumeration.Int16;
         case TypeCode.UInt16:
            return PrimitiveTypeEnumeration.UInt16;
         case TypeCode.Int32:
            return PrimitiveTypeEnumeration.Int32;
         case TypeCode.UInt32:
            return PrimitiveTypeEnumeration.UInt32;
         case TypeCode.Int64:
            return PrimitiveTypeEnumeration.Int64;
         case TypeCode.UInt64:
            return PrimitiveTypeEnumeration.UInt64;
         case TypeCode.Single:
            return PrimitiveTypeEnumeration.Single;
         case TypeCode.Double:
            return PrimitiveTypeEnumeration.Double;
         case TypeCode.Decimal:
            return PrimitiveTypeEnumeration.Decimal;
         case TypeCode.DateTime:
            return PrimitiveTypeEnumeration.DateTime;
         case TypeCode.String:
            return PrimitiveTypeEnumeration.String;
         default:
            throw new InvalidOperationException( "Unknown typecode: " + Type.GetTypeCode( type ) );
      }
   }

   private static void WritePrimitive( SerializationState state, Object primitive, PrimitiveTypeEnumeration pType )
   {
      String s;
      Int32 len;
      switch ( pType )
      {
         case PrimitiveTypeEnumeration.Boolean:
            state.EnsureCapacity( 1 );
            state.array.WriteByteToBytes( ref state.idx, ( (Boolean) primitive ) == true ? (Byte) 1 : (Byte) 0 );
            break;
         case PrimitiveTypeEnumeration.Byte:
            state.EnsureCapacity( 1 );
            state.array.WriteByteToBytes( ref state.idx, (Byte) primitive );
            break;
         case PrimitiveTypeEnumeration.Char:
            state.EnsureCapacity( 4 );
            state.idx = UTF8.GetBytes( new[] { (Char) primitive }, 0, 1, state.array, 0 );
            break;
         case PrimitiveTypeEnumeration.Decimal:
            state.WriteResourceManagerDecimal_AsClassRecordMemberValue( (Decimal) primitive );
            break;
         case PrimitiveTypeEnumeration.Double:
            state.EnsureCapacity( 8 );
            state.array.WriteDoubleLEToBytes( ref state.idx, (Double) primitive );
            break;
         case PrimitiveTypeEnumeration.Int16:
            state.EnsureCapacity( 2 );
            state.array.WriteInt16LEToBytes( ref state.idx, (Int16) primitive );
            break;
         case PrimitiveTypeEnumeration.Int32:
            state.EnsureCapacity( 4 );
            state.array.WriteInt32LEToBytes( ref state.idx, (Int32) primitive );
            break;
         case PrimitiveTypeEnumeration.Int64:
            state.EnsureCapacity( 8 );
            state.array.WriteInt64LEToBytes( ref state.idx, (Int64) primitive );
            break;
         case PrimitiveTypeEnumeration.SByte:
            state.EnsureCapacity( 1 );
            state.array.WriteSByteToBytes( ref state.idx, (SByte) primitive );
            break;
         case PrimitiveTypeEnumeration.Single:
            state.EnsureCapacity( 4 );
            state.array.WriteSingleLEToBytes( ref state.idx, (Single) primitive );
            break;
         case PrimitiveTypeEnumeration.TimeSpan:
            state.WriteResourceManagerTimeSpan_AsClassRecordMemberValue( (TimeSpan) primitive );
            break;
         case PrimitiveTypeEnumeration.DateTime:
            state.WriteResourceManagerDateTime_AsClassRecordMemberValue( (DateTime) primitive );
            break;
         case PrimitiveTypeEnumeration.UInt16:
            state.EnsureCapacity( 2 );
            state.array.WriteUInt16LEToBytes( ref state.idx, (UInt16) primitive );
            break;
         case PrimitiveTypeEnumeration.UInt32:
            state.EnsureCapacity( 4 );
            state.array.WriteUInt32LEToBytes( ref state.idx, (UInt32) primitive );
            break;
         case PrimitiveTypeEnumeration.UInt64:
            state.EnsureCapacity( 8 );
            state.array.WriteUInt64LEToBytes( ref state.idx, (UInt64) primitive );
            break;
         case PrimitiveTypeEnumeration.Null:
            state.EnsureCapacity( 0 );
            break;
         case PrimitiveTypeEnumeration.String:
            s = (String) primitive;
            len = UTF8.GetByteCount( s );
            state.EnsureCapacity( 5 + len );
            state.array
               .WriteInt32LEEncoded7Bit( ref state.idx, len )
               .WriteStringToBytes( ref state.idx, UTF8, s );
            break;
         default:
            throw new ArgumentException( "Unrecognized primitive type enumeration value: " + pType + "." );
      }
      state.WriteArrayToStream();
   }

   private static Int32 SafeByteCount( String str, Encoding encoding = null )
   {
      return String.IsNullOrEmpty( str ) ?
         0 :
         ( encoding ?? UTF8 ).GetByteCount( str );
   }

   private static void WriteResourceManagerDateTime_AsResourceManagerEntry( this ResizableArray<Byte> array, ref Int32 idx, DateTime dt )
   {
      array.WriteInt64LEToBytes( ref idx, dt.ToBinary() );
   }

   private static void WriteResourceManagerTimeSpan_AsResourceManagerEntry( this ResizableArray<Byte> array, ref Int32 idx, TimeSpan ts )
   {
      array.WriteInt64LEToBytes( ref idx, ts.Ticks );
   }

   private static void WriteResourceManagerDateTime_AsClassRecordMemberValue( this SerializationState state, DateTime dt )
   {
      var rawDateTime = (UInt64) dt.ToBinary();
      if ( ( rawDateTime & 0x8000000000000000uL ) != 0uL )
      {
         // This is local date-time, do UTC offset tick adjustment
         rawDateTime += (UInt64) TimeZoneInfo.Local.GetUtcOffset( dt ).Ticks;
      }
      state.EnsureCapacity( state.idx + 8 );
      state.array.WriteUInt64LEToBytes( ref state.idx, rawDateTime );
   }

   private static void WriteResourceManagerTimeSpan_AsClassRecordMemberValue( this SerializationState state, TimeSpan ts )
   {
      state.EnsureCapacity( state.idx + 8 );
      state.array.WriteInt64LEToBytes( ref state.idx, ts.Ticks );
   }

   private static void WriteResourceManagerDecimal_AsResourceManagerEntry( this ResizableArray<Byte> array, ref Int32 idx, Decimal d )
   {
      array.CurrentMaxCapacity = 16;
      foreach ( var val in Decimal.GetBits( d ) )
      {
         array.Array.WriteInt32LEToBytes( ref idx, val );
      }
   }

   private static void WriteResourceManagerDecimal_AsClassRecordMemberValue( this SerializationState state, Decimal d )
   {
      var str = d.ToString( null, System.Globalization.CultureInfo.InvariantCulture );
      var len = UTF8.GetByteCount( str );
      state.EnsureCapacity( 5 + len );
      state.array
         .WriteInt32LEEncoded7Bit( ref state.idx, len )
         .WriteStringToBytes( ref state.idx, UTF8, str );
   }


   private sealed class SerializationState
   {
      internal readonly Stream stream;
      internal Byte[] array;
      internal Int32 idx;
      internal Int32 arrayLen;
      internal readonly IDictionary<Object, Int32> records;
      internal readonly IDictionary<String, Int32> assemblies;
      internal readonly Queue<Object> recordQueue;
      internal readonly IDictionary<Tuple<String, String>, Tuple<Int32, Tuple<BinaryTypeEnumeration, PrimitiveTypeEnumeration>[]>> serializedObjects;

      internal SerializationState( Stream aStream )
      {
         ArgumentValidator.ValidateNotNull( "Stream", aStream );

         this.stream = aStream;
         this.records = new Dictionary<Object, Int32>( ComparerFromFunctions.NewEqualityComparer<Object>(
            ( x, y ) =>
            {
               return ReferenceEquals( x, y ) || String.Equals( x as String, y as String );
            },
            x =>
            {
               return x.GetHashCodeSafe();
            } ) );
         this.assemblies = new Dictionary<String, Int32>();
         this.recordQueue = new Queue<Object>();
         this.serializedObjects = new Dictionary<Tuple<String, String>, Tuple<Int32, Tuple<BinaryTypeEnumeration, PrimitiveTypeEnumeration>[]>>();
      }

      internal Boolean TryAddAssemblyName( String aName, out Int32 id )
      {
         var retVal = !String.IsNullOrEmpty( aName ) && !this.assemblies.ContainsKey( aName );
         if ( retVal )
         {
            // The ID must start with '2', since '1' is reserved for top object
            id = this.assemblies.Count + 2;
            this.assemblies.Add( aName, id );
         }
         else
         {
            id = 0;
         }
         return retVal;
      }

      internal Boolean TryAddRecord( Object record, out Int32 id )
      {
         id = 0;
         var retVal = record != null && !this.records.TryGetValue( record, out id );
         if ( retVal )
         {
            id = this.records.Count == 0 ? 1 : ( this.assemblies.Count + this.records.Count + 1 );
            this.records.Add( record, id );
         }
         return retVal;
      }

      internal void WriteArrayToStream()
      {
         if ( idx > 0 )
         {
            this.stream.Write( this.array, this.idx );
            this.idx = 0;
         }
      }

      internal void EnsureCapacity( Int32 capacity )
      {
         this.idx = 0;
         this.arrayLen = capacity;
         if ( this.array == null || this.array.Length < capacity )
         {
            this.array = new Byte[capacity];
         }
      }
   }


   private static Int64 ToBinary( this DateTime dt )
   {
      // DateTime.ToBinary does not exist in this PCL profile...
      var kind = dt.Kind;
      switch ( dt.Kind )
      {
         case DateTimeKind.Local:
            var localUtcOffset = TimeZoneInfo.Local.GetUtcOffset( dt );
            var tickOffset = dt.Ticks - localUtcOffset.Ticks;
            if ( tickOffset < 0L )
            {
               tickOffset = 0x4000000000000000L + tickOffset;
            }
            // It seems that the representation of date time is one unsigned long, where the two MSB bits behave like this (MSB first, then next-to-MSB):
            // 00 - Unspecified DateTime (unknown, whether it is local or UTC)
            // 01 - UTC DateTime
            // 10 - Local DateTime
            // 11 - Local DateTime, but ambiguous daylight saving time (whatever that means)

            // This OR forces the local-flag
            return tickOffset | unchecked((Int64) 0x8000000000000000uL);
         case DateTimeKind.Unspecified:
            return dt.Ticks;
         case DateTimeKind.Utc:
            return dt.Ticks | 0x4000000000000000L;
         default:
            throw new ArgumentException( "Unrecognized DateTime kind: " + kind );
      }
   }
}
