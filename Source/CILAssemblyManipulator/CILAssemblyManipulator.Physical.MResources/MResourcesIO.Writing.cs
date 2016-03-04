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
            throw new NotImplementedException( "Not implemented yet." );
         case ResourceManagerEntryKind.UserDefined:
            ( (UserDefinedResourceManagerEntry) entry ).WriteNRBFRecords( stream );
            break;
         default:
            throw new ArgumentException( "Unrecognized resource manager entry kind: " + kind + "." );
      }
   }

   private static void WriteNRBFRecords( this UserDefinedResourceManagerEntry entry, Stream stream )
   {
      var state = new SerializationState( stream );
      // Write header
      state.EnsureCapacity( 17 ); // Header + header value
      state.array
         .WriteByteToBytes( ref state.idx, (Byte) RecordTypeEnumeration.SerializedStreamHeader )
         .WriteInt32LEToBytes( ref state.idx, 1 )
         .WriteInt32LEToBytes( ref state.idx, -1 )
         .WriteInt32LEToBytes( ref state.idx, 1 )
         .WriteInt32LEToBytes( ref state.idx, 0 );
      stream.Write( state.array );

      // Collect all assembly names
      var rec = entry.Contents;

      // Write used assembly names first
      WriteAssemblyNames( state, rec );

      // Write record structure
      WriteSingleRecord( state, rec, false );

      // Empty queue of reference objects
      while ( state.recordQueue.Count > 0 )
      {
         WriteSingleRecord( state, state.recordQueue.Dequeue(), true );
      }


      // Write end
      state.EnsureCapacity( 1 );
      state.array.WriteByteToBytes( ref state.idx, (Byte) RecordTypeEnumeration.MessageEnd );
      stream.Write( state.array );
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
               .WriteInt32Encoded7Bit( ref state.idx, strByteCount )
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
               .WriteInt32Encoded7Bit( ref state.idx, len )
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
      else if ( record == null )
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

   private static void WriteClassRecord( SerializationState state, ClassRecord claas, Int32 id )
   {
      var metaDataKey = Tuple.Create( claas.AssemblyName, claas.TypeName );
      Int32 otherID;
      if ( state.serializedObjects.TryGetValue( metaDataKey, out otherID ) )
      {
         // Another record of the same type was serialized earlier, can use previous info
         state.EnsureCapacity( 9 );
         state.array
            .WriteByteToBytes( ref state.idx, (Byte) RecordTypeEnumeration.ClassWithID )
            .WriteInt32LEToBytes( ref state.idx, id )
            .WriteInt32LEToBytes( ref state.idx, otherID );
         state.WriteArrayToStream();
      }
      else
      {
         var isSystem = claas.AssemblyName == null;
         var nameByteCount = SafeByteCount( claas.TypeName );
         state.EnsureCapacity( 14 + nameByteCount ); // class type (1), id (4), space for class name length (max 5), member count (4)
         state.array
            .WriteByteToBytes( ref state.idx, (Byte) ( isSystem ? RecordTypeEnumeration.SystemClassWithMembersAndTypes : RecordTypeEnumeration.ClassWithMembersAndTypes ) )
            .WriteInt32LEToBytes( ref state.idx, id )
            .WriteInt32Encoded7Bit( ref state.idx, nameByteCount )
            .WriteStringToBytes( ref state.idx, UTF8, claas.TypeName )
            .WriteInt32LEToBytes( ref state.idx, claas.Members.Count );
         state.WriteArrayToStream();

         // Write member names
         foreach ( var member in claas.Members )
         {
            nameByteCount = SafeByteCount( member.Name );
            state.EnsureCapacity( 5 + nameByteCount );
            state.array
               .WriteInt32Encoded7Bit( ref state.idx, nameByteCount )
               .WriteStringToBytes( ref state.idx, UTF8, member.Name );
            state.WriteArrayToStream();
         }
         // Write member type infos
         state.EnsureCapacity( claas.Members.Count );
         var mTypeCodes = new List<Tuple<BinaryTypeEnumeration, PrimitiveTypeEnumeration>>( claas.Members.Count );
         foreach ( var member in claas.Members )
         {
            PrimitiveTypeEnumeration pType;
            var bt = GetTypeInfo( member.Value, out pType );
            mTypeCodes.Add( Tuple.Create( bt, pType ) );
            state.array.WriteByteToBytes( ref state.idx, (Byte) bt );
         }
         state.WriteArrayToStream();
         // Write additional type info where applicable
         for ( var i = 0; i < mTypeCodes.Count; ++i )
         {
            var member = claas.Members[i];
            var tuple = mTypeCodes[i];
            switch ( tuple.Item1 )
            {
               case BinaryTypeEnumeration.Primitive:
                  state.EnsureCapacity( 1 );
                  state.array.WriteByteToBytes( ref state.idx, (Byte) tuple.Item2 );
                  break;
               case BinaryTypeEnumeration.SystemClass:
                  nameByteCount = SafeByteCount( member.TypeName );
                  state.EnsureCapacity( 5 + nameByteCount );
                  state.array
                     .WriteInt32Encoded7Bit( ref state.idx, nameByteCount )
                     .WriteStringToBytes( ref state.idx, UTF8, member.TypeName );
                  break;
               case BinaryTypeEnumeration.Class:
                  nameByteCount = SafeByteCount( member.TypeName );
                  state.EnsureCapacity( 9 + nameByteCount );
                  state.array
                     .WriteInt32Encoded7Bit( ref state.idx, nameByteCount )
                     .WriteStringToBytes( ref state.idx, UTF8, member.TypeName )
                     .WriteInt32LEToBytes( ref state.idx, state.assemblies[member.AssemblyName] );
                  break;
               case BinaryTypeEnumeration.PrimitiveArray:
                  state.EnsureCapacity( 1 );
                  state.array
                     .WriteByteToBytes( ref state.idx, (Byte) tuple.Item2 );
                  break;
            }
            state.WriteArrayToStream();
         }


         // Write this class assembly name if needed
         if ( !isSystem )
         {
            state.EnsureCapacity( 4 );
            state.array.WriteInt32LEToBytes( ref state.idx, state.assemblies[claas.AssemblyName] );
            state.WriteArrayToStream();
         }

         // Write member values
         for ( var i = 0; i < mTypeCodes.Count; ++i )
         {
            var member = claas.Members[i];
            var val = member.Value;

            var rec = val as AbstractRecord;
            if ( val is String || rec != null )
            {
               // All arrays and non-structs are serialized afterwards.
               if ( rec is ArrayRecord || ( rec is ClassRecord && !( (ClassRecord) rec ).IsSerializedInPlace ) )
               {
                  // Add to mapping before calling recursively in order to create MemberReference
                  if ( state.TryAddRecord( rec, out id ) )
                  {
                     // The record hasn't been serialized, add to queue
                     state.recordQueue.Enqueue( rec );
                  }
               }

               // Write the record
               WriteSingleRecord( state, rec, false );

            }
            else
            {
               // Write value as primitive
               WritePrimitive( state, val, mTypeCodes[i].Item2 );
            }
         }

      }
      if ( !state.serializedObjects.ContainsKey( metaDataKey ) )
      {
         state.serializedObjects.Add( metaDataKey, id );
      }
   }

   private static void WriteArrayRecord( SerializationState state, ArrayRecord array, Int32 id )
   {
      var rank = Math.Max( 1, array.Rank );
      Type pType = null;
      // Default for empty arrays and arrays with just nulls is ArraySinglePrimitive
      var recType = RecordTypeEnumeration.ArraySinglePrimitive;
      var nonNullEncountered = false;
      foreach ( var val in array.ValuesAsVector )
      {
         if ( val != null )
         {
            var valType = val.GetType();
            if ( nonNullEncountered )
            {
               if (
                  ( recType == RecordTypeEnumeration.ArraySingleString && !( val is String ) )
                  || ( recType == RecordTypeEnumeration.ArraySinglePrimitive && !Object.Equals( pType, valType ) )
                  )
               {
                  recType = RecordTypeEnumeration.ArraySingleObject;
               }
            }
            else
            {
               recType = val is String ?
                     RecordTypeEnumeration.ArraySingleString :
                     ( val is AbstractRecord ?
                        RecordTypeEnumeration.ArraySingleObject :
                        RecordTypeEnumeration.ArraySinglePrimitive );
               if ( recType == RecordTypeEnumeration.ArraySinglePrimitive )
               {
                  pType = valType;
               }
               nonNullEncountered = true;
            }
         }
         if ( recType == RecordTypeEnumeration.ArraySingleObject )
         {
            break;
         }
      }
      var recTypeToUse = BinaryArrayTypeEnumeration.Single == array.ArrayKind ? recType : RecordTypeEnumeration.BinaryArray;

      // Write information common for all arrays
      state.EnsureCapacity( 9 );
      state.array
         .WriteByteToBytes( ref state.idx, (Byte) recTypeToUse )
         .WriteInt32LEToBytes( ref state.idx, id );
      if ( RecordTypeEnumeration.BinaryArray != recTypeToUse )
      {
         state.array.WriteInt32LEToBytes( ref state.idx, array.ValuesAsVector.Count );
      }
      state.WriteArrayToStream();
      PrimitiveTypeEnumeration pEnum;
      switch ( recTypeToUse )
      {
         case RecordTypeEnumeration.BinaryArray:
            var ak = array.ArrayKind;
            var cap = 7 + 4 * rank; // array type (1), rank (4), rank lengths (4 each) + type info (1) + possible primitive info
            var hasOffset = false;
            switch ( array.ArrayKind )
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
               .WriteByteToBytes( ref state.idx, (Byte) RecordTypeEnumeration.BinaryArray )
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
            BinaryTypeEnumeration typeEnum;
            switch ( recType )
            {
               case RecordTypeEnumeration.ArraySinglePrimitive:
                  typeEnum = BinaryTypeEnumeration.Primitive;
                  break;
               case RecordTypeEnumeration.ArraySingleObject:
                  typeEnum = BinaryTypeEnumeration.Object;
                  break;
               case RecordTypeEnumeration.ArraySingleString:
                  typeEnum = BinaryTypeEnumeration.String;
                  break;
               default:
                  throw new InvalidOperationException( "The code to detect array type has changed and this switch clause wasn't adjusted appropriately." );
            }
            state.array.WriteByteToBytes( ref state.idx, (Byte) typeEnum );
            pEnum = GetPrimitiveTypeFromType( pType );
            if ( BinaryTypeEnumeration.Primitive == typeEnum )
            {
               state.array.WriteByteToBytes( ref state.idx, (Byte) pEnum );
            }
            state.WriteArrayToStream();
            WriteArrayValues( state, array.ValuesAsVector, obj =>
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
            break;
         // Serialize all information about array
         case RecordTypeEnumeration.ArraySinglePrimitive:
            state.EnsureCapacity( 1 );
            pEnum = GetPrimitiveTypeFromType( pType );
            state.array.WriteByteToBytes( ref state.idx, (Byte) pEnum );
            state.WriteArrayToStream();
            WriteArrayValues( state, array.ValuesAsVector, obj => WritePrimitive( state, obj, pEnum ) );
            break;
         case RecordTypeEnumeration.ArraySingleObject:
            WriteArrayValues( state, array.ValuesAsVector, obj =>
            {
               WriteSingleRecord( state, obj as AbstractRecord, false, obj );
            } );
            break;
         case RecordTypeEnumeration.ArraySingleString:
            WriteArrayValues( state, array.ValuesAsVector, obj =>
            {
               WriteSingleRecord( state, null, false, obj );
            } );
            break;
      }
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
         return BinaryTypeEnumeration.Primitive;
      }
      else
      {
         switch ( Type.GetTypeCode( obj.GetType() ) )
         {
            case TypeCode.Object:
               if ( obj is AbstractRecord )
               {
                  switch ( ( (AbstractRecord) obj ).RecordKind )
                  {
                     case RecordKind.Class:
                        return ( (ClassRecord) obj ).AssemblyName == null ? BinaryTypeEnumeration.SystemClass : BinaryTypeEnumeration.Class;
                     case RecordKind.Array:
                        var array = (ArrayRecord) obj;
                        if ( array.Rank == 1 )
                        {
                           var firstNonNull = array.ValuesAsVector.FirstOrDefault( o => o != null );
                           switch ( GetTypeInfo( firstNonNull, out pType ) )
                           {
                              case BinaryTypeEnumeration.String:
                                 return BinaryTypeEnumeration.StringArray;
                              case BinaryTypeEnumeration.Primitive:
                                 return BinaryTypeEnumeration.PrimitiveArray;
                              default:
                                 return BinaryTypeEnumeration.ObjectArray;
                           }
                        }
                        else
                        {
                           return BinaryTypeEnumeration.ObjectArray;
                        }
                     default:
                        throw new NotSupportedException( "Unknown record " + obj + "." );

                  }
               }
               else
               {
                  throw new InvalidOperationException( "Only primitives and AbstractRecords allowed as values. Encountered " + obj + " as value." );
               }
            case TypeCode.String:
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
            var d = (Decimal) primitive;
            s = d.ToString();
            len = UTF8.GetByteCount( s );
            var ints = Decimal.GetBits( d );
            state.EnsureCapacity( 5 + len + 16 );
            state.array
               .WriteInt32Encoded7Bit( ref state.idx, len )
               .WriteStringToBytes( ref state.idx, UTF8, s )
               .WriteInt32LEToBytes( ref state.idx, ints[0] )
               .WriteInt32LEToBytes( ref state.idx, ints[1] )
               .WriteInt32LEToBytes( ref state.idx, ints[2] )
               .WriteInt32LEToBytes( ref state.idx, ints[3] );
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
            state.EnsureCapacity( 8 );
            state.array.WriteInt64LEToBytes( ref state.idx, ( (TimeSpan) primitive ).Ticks );
            break;
         case PrimitiveTypeEnumeration.DateTime:
            state.EnsureCapacity( 8 );
            state.array.WriteInt64LEToBytes( ref state.idx, ( (DateTime) primitive ).ToBinary() );
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
               .WriteInt32Encoded7Bit( ref state.idx, len )
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


   private sealed class SerializationState
   {
      internal readonly Stream stream;
      internal Byte[] array;
      internal Int32 idx;
      internal Int32 arrayLen;
      internal readonly IDictionary<Object, Int32> records;
      internal readonly IDictionary<String, Int32> assemblies;
      internal readonly Queue<AbstractRecord> recordQueue;
      internal readonly IDictionary<Tuple<String, String>, Int32> serializedObjects;

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
         this.recordQueue = new Queue<AbstractRecord>();
         this.serializedObjects = new Dictionary<Tuple<String, String>, Int32>();
      }

      internal Boolean TryAddAssemblyName( String aName, out Int32 id )
      {
         var retVal = !String.IsNullOrEmpty( aName ) && !this.assemblies.ContainsKey( aName );
         if ( retVal )
         {
            id = this.assemblies.Count + 1;
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
            id = this.assemblies.Count + this.records.Count + 1;
            this.records.Add( record, id );
         }
         return retVal;
      }

      internal void WriteArrayToStream()
      {
         if ( idx > 0 )
         {
            this.stream.Write( this.array, this.idx );
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
