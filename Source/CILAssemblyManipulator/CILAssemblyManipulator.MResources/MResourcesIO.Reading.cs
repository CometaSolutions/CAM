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

namespace CILAssemblyManipulator.MResources
{
   public sealed class ResourceItem
   {
      public String Name { get; set; }
      public String Type { get; set; }
      public Boolean IsUserDefinedType { get; set; }
      public Int32 DataOffset { get; set; }
      public Int32 DataSize { get; set; }
   }

   public static partial class MResourcesIO
   {
      private const UInt32 RES_MANAGER_HEADER_MAGIC = 0xBEEFCACEu;

      // Gets resource information without deserializing the values.
      // I think the same could be accomplished by iterating System.Resources.ResourceReader and never accessing the 'Value' property of it, but I'm not 100% sure.
      // The format is quite simple though so to be sure, just do it ourselves.
      // Item1 - resource name
      // Item2 - resource type
      // Item3 - whether type is user-defined
      // Item4 - offset in array where data starts
      // Item5 - size of data
      public static IEnumerable<ResourceItem> GetResourceInfo( Byte[] array, out Boolean wasResourceManager, Int32 idx = 0 )
      {
         // See http://www.dotnetframework.org/default.aspx/4@0/4@0/untmp/DEVDIV_TFS/Dev10/Releases/RTMRel/ndp/clr/src/BCL/System/Resources/RuntimeResourceSet@cs/1305376/RuntimeResourceSet@cs for some explanation of format
         // Or ResReader.cs in ILRepack
         // http://primates.ximian.com/~lluis/dist/binary_serialization_format.htm Is also useful resource
         var startIdx = idx;
         var resHdrMagic = array.ReadUInt32LEFromBytes( ref idx );
         wasResourceManager = RES_MANAGER_HEADER_MAGIC == resHdrMagic;
         return wasResourceManager ?
            GetResourceInfoFromResourceManagerData( array, startIdx, idx ) :
            Empty<ResourceItem>.Enumerable;
      }

      private static IEnumerable<ResourceItem> GetResourceInfoFromResourceManagerData( Byte[] array, Int32 startIdx, Int32 idx )
      {
         if ( array.ReadInt32LEFromBytes( ref idx ) > 1 ) // Resource manager version
         {
            idx += array.ReadInt32LEFromBytes( ref idx ); // Amount of bytes to skip
         }
         else
         {
            idx += 4; // Skip amount of bytes to skip
            var strLen = array.ReadInt32Encoded7Bit( ref idx ); // Skip resource reader typename
            idx += strLen;
            strLen = array.ReadInt32Encoded7Bit( ref idx ); // Skip resource set typename
            idx += strLen;
         }
         var version = array.ReadInt32LEFromBytes( ref idx ); // Version
         var resCount = array.ReadInt32LEFromBytes( ref idx ); // Amount of resources
         var types = new String[array.ReadInt32LEFromBytes( ref idx )]; // Type strings
         for ( var i = 0; i < types.Length; ++i )
         {
            types[i] = array.Read7BitLengthPrefixedString( ref idx );
         }
         // Skip to next alignment of 8
         idx += 8 - ( idx & 7 );
         // Skip name hashes
         idx += 4 * resCount;
         var namePositions = array.ReadInt32ArrayLEFromBytes( ref idx, resCount ); // Relative positions of names

         var dataOffset = startIdx + array.ReadInt32LEFromBytes( ref idx ); // Data section offset
         var nameOffset = idx; // Names follow right after this

         // Read names and data offsets of resources
         var dataNamesAndOffsets = new Tuple<String, Int32>[resCount];
         for ( var i = 0; i < resCount; ++i )
         {
            var iIdx = nameOffset + namePositions[i];
            // VS2012 should guarantee that parameters are executed in this order.
            dataNamesAndOffsets[i] = Tuple.Create( array.Read7BitLengthPrefixedString( ref iIdx, UTF16LE ), dataOffset + array.ReadInt32LEFromBytes( ref iIdx ) );
         }
         Array.Sort( dataNamesAndOffsets, ( t1, t2 ) => t1.Item2.CompareTo( t2.Item2 ) ); // Sort information according to data offset

         // Return enumerable
         for ( var i = 0; i < resCount; ++i )
         {
            var iIdx = dataNamesAndOffsets[i].Item2;
            var tIdx = array.ReadInt32Encoded7Bit( ref iIdx );
            var isUserType = version != 1 && tIdx >= (Int32) ResourceTypeCode.StartOfUserTypes;
            var resType = version == 1 ?
               types[tIdx] :
               ( isUserType ?
                  types[tIdx - (Int32) ResourceTypeCode.StartOfUserTypes] :
                  ( "ResourceTypeCode." + (ResourceTypeCode) tIdx )
               );
            yield return new ResourceItem()
            {
               Name = dataNamesAndOffsets[i].Item1,
               Type = resType,
               IsUserDefinedType = isUserType,
               DataOffset = iIdx,
               DataSize = ( i < resCount - 1 ? dataNamesAndOffsets[i + 1].Item2 : array.Length ) - iIdx
            };
         }

      }

      public static IList<AbstractRecord> ReadNRBFRecords( Byte[] array, ref Int32 idx, Int32 maxIndex )
      {
         var state = new DeserializationState( array, idx );
         var list = new List<AbstractRecord>();
         while ( idx < maxIndex && !state.recordsEnded )
         {
            var record = ReadSingleRecord( state );
            if ( record != null )
            {
               list.Add( record );
            }
         }
         for ( var i = 0; i < list.Count; ++i )
         {
            var rec = list[i];
            AbstractRecord actual;
            if ( CheckPlaceholder( state, rec, out actual ) )
            {
               list[i] = actual;
            }
         }
         return list;
      }

      private static AbstractRecord ReadSingleRecord( DeserializationState state )
      {
         AbstractRecord retVal;
         if ( state.nullCount > 0 )
         {
            --state.nullCount;
            retVal = null;
         }
         else
         {
            ClassRecord cRecord;
            ArrayRecord aRecord;
            var recType = (RecordTypeEnumeration) state.array.ReadByteFromBytes( ref state.idx );
            switch ( recType )
            {
               case RecordTypeEnumeration.SerializedStreamHeader:
                  state.array.Skip( ref state.idx, 16 ); // Skip the header of 4 ints
                  retVal = null;
                  break;
               case RecordTypeEnumeration.ClassWithID:
                  cRecord = NewRecord<ClassRecord>( state );
                  var refID = state.array.ReadInt32LEFromBytes( ref state.idx );
                  var refRecord = (ClassRecord) state.records[refID];
                  // Copy metadata from referenced object
                  cRecord.AssemblyName = refRecord.AssemblyName;
                  cRecord.TypeName = refRecord.TypeName;
                  cRecord.Members.Capacity = refRecord.Members.Count;
                  for ( var i = 0; i < refRecord.Members.Count; ++i )
                  {
                     var member = new ClassRecordMember();
                     var refMember = refRecord.Members[i];
                     member.AssemblyName = refMember.AssemblyName;
                     member.TypeName = refMember.TypeName;
                     member.Name = refMember.Name;
                     cRecord.Members.Add( member );
                  }
                  // Read values
                  ReadMemberValues( state, cRecord );
                  retVal = cRecord;
                  break;
               case RecordTypeEnumeration.SystemClassWithMembers:
                  cRecord = NewRecord<ClassRecord>( state );
                  ReadMembers( state, cRecord );
                  ReadMemberValues( state, cRecord );
                  retVal = cRecord;
                  break;
               case RecordTypeEnumeration.ClassWithMembers:
                  cRecord = NewRecord<ClassRecord>( state );
                  ReadMembers( state, cRecord );
                  cRecord.AssemblyName = state.assemblies[state.array.ReadInt32LEFromBytes( ref state.idx )];
                  ReadMemberValues( state, cRecord );
                  retVal = cRecord;
                  break;
               case RecordTypeEnumeration.SystemClassWithMembersAndTypes:
                  cRecord = NewRecord<ClassRecord>( state );
                  ReadMembers( state, cRecord );
                  ReadMemberTypes( state, cRecord );
                  ReadMemberValues( state, cRecord );
                  retVal = cRecord;
                  break;
               case RecordTypeEnumeration.ClassWithMembersAndTypes:
                  cRecord = NewRecord<ClassRecord>( state );
                  ReadMembers( state, cRecord );
                  ReadMemberTypes( state, cRecord );
                  cRecord.AssemblyName = state.assemblies[state.array.ReadInt32LEFromBytes( ref state.idx )];
                  ReadMemberValues( state, cRecord );
                  retVal = cRecord;
                  break;
               case RecordTypeEnumeration.BinaryObjectString:
                  var strRecord = NewRecord<StringRecord>( state );
                  strRecord.StringValue = state.array.Read7BitLengthPrefixedString( ref state.idx );
                  retVal = strRecord;
                  break;
               case RecordTypeEnumeration.BinaryArray:
                  aRecord = NewRecord<ArrayRecord>( state );
                  ReadArrayInformation( state, aRecord );
                  ReadArrayValues( state, aRecord );
                  retVal = aRecord;
                  break;
               case RecordTypeEnumeration.MemberPrimitiveTyped:
                  var pRecord = new PrimitiveWrapperRecord();
                  pRecord.Value = ReadPrimitiveValue( state, (PrimitiveTypeEnumeration) state.array.ReadByteFromBytes( ref state.idx ) );
                  retVal = pRecord;
                  break;
               case RecordTypeEnumeration.MemberReference:
                  retVal = new RecordPlaceholder( state.array.ReadInt32LEFromBytes( ref state.idx ) );
                  break;
               case RecordTypeEnumeration.ObjectNull:
                  retVal = null;
                  break;
               case RecordTypeEnumeration.MessageEnd:
                  state.recordsEnded = true;
                  retVal = null;
                  break;
               case RecordTypeEnumeration.BinaryLibrary:
                  // VS2012 should guarantee that first parameter will be executed first
                  state.assemblies.Add( state.array.ReadInt32LEFromBytes( ref state.idx ), state.array.Read7BitLengthPrefixedString( ref state.idx ) );
                  retVal = null;
                  break;
               case RecordTypeEnumeration.ObjectNullMultiple256:
                  state.nullCount = state.array.ReadByteFromBytes( ref state.idx ) - 1; // Returning the first null
                  retVal = null;
                  break;
               case RecordTypeEnumeration.ObjectNullMultiple:
                  state.nullCount = state.array.ReadInt32LEFromBytes( ref state.idx ) - 1; // Returning the first null
                  retVal = null;
                  break;
               case RecordTypeEnumeration.ArraySinglePrimitive:
                  aRecord = NewRecord<ArrayRecord>( state );
                  state.typeInfos.Add( aRecord, BinaryTypeEnumeration.Primitive );
                  ReadArrayLengths( state, aRecord );
                  ReadAdditionalTypeInfo( state, aRecord );
                  ReadArrayValues( state, aRecord );
                  retVal = aRecord;
                  break;
               case RecordTypeEnumeration.ArraySingleObject:
                  aRecord = NewRecord<ArrayRecord>( state );
                  state.typeInfos.Add( aRecord, BinaryTypeEnumeration.Object );
                  ReadArrayLengths( state, aRecord );
                  ReadArrayValues( state, aRecord );
                  retVal = aRecord;
                  break;
               case RecordTypeEnumeration.ArraySingleString:
                  aRecord = NewRecord<ArrayRecord>( state );
                  state.typeInfos.Add( aRecord, BinaryTypeEnumeration.String );
                  ReadArrayLengths( state, aRecord );
                  ReadArrayValues( state, aRecord );
                  retVal = aRecord;
                  break;
               case RecordTypeEnumeration.MethodCall:
                  throw new NotImplementedException(); // TODO
               case RecordTypeEnumeration.MethodReturn:
                  throw new NotImplementedException(); // TODO
               default:
                  throw new InvalidOperationException( "Unsupported record type: " + recType + "." );
            }
         }
         return retVal;
      }

      private static void ReadMembers( DeserializationState state, ClassRecord record )
      {
         record.TypeName = state.array.Read7BitLengthPrefixedString( ref state.idx );
         var cap = state.array.ReadInt32LEFromBytes( ref state.idx );
         record.Members.Capacity = cap;
         for ( var i = 0; i < cap; ++i )
         {
            var member = new ClassRecordMember();
            member.Name = state.array.Read7BitLengthPrefixedString( ref state.idx );
            record.Members.Add( member );
         }
      }

      private static void ReadMemberValues( DeserializationState state, ClassRecord record )
      {
         foreach ( var member in record.Members )
         {
            var val = ReadMemberValue( state, member );
            member.Value = val;
         }
      }

      private static Object ReadMemberValue( DeserializationState state, Object member )
      {
         BinaryTypeEnumeration typeInfo;
         if ( state.typeInfos.TryGetValue( member, out typeInfo ) )
         {
            switch ( typeInfo )
            {
               case BinaryTypeEnumeration.Primitive:
                  return ReadPrimitiveValue( state, state.primitiveTypeInfos[member] );
               default:
                  return ReadSingleRecord( state );
            }
         }
         else
         {
            throw new NotImplementedException( "Neither SystemClassWithMember nor ClassWithMembers is currently implemented." );
         }
      }

      private static Object ReadPrimitiveValue( DeserializationState state, PrimitiveTypeEnumeration primitive )
      {
         Object value;
         switch ( primitive )
         {
            case PrimitiveTypeEnumeration.Boolean:
               value = state.array.ReadByteFromBytes( ref state.idx ) != 0;
               break;
            case PrimitiveTypeEnumeration.Byte:
               value = state.array.ReadByteFromBytes( ref state.idx );
               break;
            case PrimitiveTypeEnumeration.Char:
               var startIdx = state.idx;
               var charArray = new Char[1];
               Int32 charsRead;
               do
               {
                  ++state.idx;
                  charsRead = UTF8.GetChars( state.array, startIdx, state.idx - startIdx, charArray, 0 );
               } while ( charsRead == 0 );
               value = charArray[0];
               break;
            case PrimitiveTypeEnumeration.Decimal:
               var str = state.array.Read7BitLengthPrefixedString( ref state.idx );
               Decimal d;
               Decimal.TryParse( str, out d );
               // I'm not quite sure why but apparently actual binary value of decimal follows?
               var int1 = state.array.ReadInt32LEFromBytes( ref state.idx );
               var int2 = state.array.ReadInt32LEFromBytes( ref state.idx );
               var int3 = state.array.ReadInt32LEFromBytes( ref state.idx );
               var int4 = state.array.ReadInt32LEFromBytes( ref state.idx );
               value = new Decimal( new[] { int1, int2, int3, int4 } );
               break;
            case PrimitiveTypeEnumeration.Double:
               value = state.array.ReadDoubleLEFromBytes( ref state.idx );
               break;
            case PrimitiveTypeEnumeration.Int16:
               value = state.array.ReadInt16LEFromBytes( ref state.idx );
               break;
            case PrimitiveTypeEnumeration.Int32:
               value = state.array.ReadInt32LEFromBytes( ref state.idx );
               break;
            case PrimitiveTypeEnumeration.Int64:
               value = state.array.ReadInt64LEFromBytes( ref state.idx );
               break;
            case PrimitiveTypeEnumeration.SByte:
               value = state.array.ReadSByteFromBytes( ref state.idx );
               break;
            case PrimitiveTypeEnumeration.Single:
               value = state.array.ReadSingleLEFromBytes( ref state.idx );
               break;
            case PrimitiveTypeEnumeration.TimeSpan:
               value = TimeSpan.FromTicks( state.array.ReadInt64LEFromBytes( ref state.idx ) );
               break;
            case PrimitiveTypeEnumeration.DateTime:
               value = DateTime.FromBinary( state.array.ReadInt64LEFromBytes( ref state.idx ) );
               break;
            case PrimitiveTypeEnumeration.UInt16:
               value = state.array.ReadUInt16LEFromBytes( ref state.idx );
               break;
            case PrimitiveTypeEnumeration.UInt32:
               value = state.array.ReadUInt32LEFromBytes( ref state.idx );
               break;
            case PrimitiveTypeEnumeration.UInt64:
               value = state.array.ReadUInt64LEFromBytes( ref state.idx );
               break;
            case PrimitiveTypeEnumeration.Null:
               value = null;
               break;
            case PrimitiveTypeEnumeration.String:
               value = state.array.Read7BitLengthPrefixedString( ref state.idx );
               break;
            default:
               throw new InvalidOperationException( "Unknown primitive type: " + primitive + "." );
         }
         return value;
      }

      private static void ReadMemberTypes( DeserializationState state, ClassRecord record )
      {
         // First normal info
         foreach ( var member in record.Members )
         {
            ReadBasicTypeInfo( state, member );
         }

         // Then additional info, if any
         foreach ( var member in record.Members )
         {
            ReadAdditionalTypeInfo( state, member );
         }
      }

      private static void ReadBasicTypeInfo( DeserializationState state, Object member )
      {
         state.typeInfos.Add( member, (BinaryTypeEnumeration) state.array.ReadByteFromBytes( ref state.idx ) );
      }

      private static void ReadAdditionalTypeInfo( DeserializationState state, ElementWithTypeInfo member )
      {
         switch ( state.typeInfos[member] )
         {
            case BinaryTypeEnumeration.Primitive:
            case BinaryTypeEnumeration.PrimitiveArray:
               state.primitiveTypeInfos.Add( member, (PrimitiveTypeEnumeration) state.array.ReadByteFromBytes( ref state.idx ) );
               break;
            case BinaryTypeEnumeration.SystemClass:
               member.TypeName = state.array.Read7BitLengthPrefixedString( ref state.idx );
               break;
            case BinaryTypeEnumeration.Class:
               member.TypeName = state.array.Read7BitLengthPrefixedString( ref state.idx );
               member.AssemblyName = state.assemblies[state.array.ReadInt32LEFromBytes( ref state.idx )];
               break;
         }
      }

      private static void ReadArrayInformation( DeserializationState state, ArrayRecord record )
      {
         var arType = (BinaryArrayTypeEnumeration) state.array.ReadByteFromBytes( ref state.idx );
         record.ArrayKind = arType;
         record.Rank = state.array.ReadInt32LEFromBytes( ref state.idx );

         ReadArrayLengths( state, record );

         switch ( arType )
         {
            case BinaryArrayTypeEnumeration.SingleOffset:
            case BinaryArrayTypeEnumeration.JaggedOffset:
            case BinaryArrayTypeEnumeration.RectangularOffset:
               record.LowerBounds.Capacity = record.Rank;
               for ( var i = 0; i < record.Rank; ++i )
               {
                  record.LowerBounds.Add( state.array.ReadInt32LEFromBytes( ref state.idx ) );
               }
               break;
         }
         ReadBasicTypeInfo( state, record );
         ReadAdditionalTypeInfo( state, record );
      }

      private static void ReadArrayLengths( DeserializationState state, ArrayRecord record )
      {
         record.Lengths.Capacity = record.Rank;
         for ( var i = 0; i < record.Rank; ++i )
         {
            record.Lengths.Add( state.array.ReadInt32LEFromBytes( ref state.idx ) );
         }
      }

      private static void ReadArrayValues( DeserializationState state, ArrayRecord record )
      {
         var maxLen = record.Lengths.Aggregate( 1, ( cur, length ) => cur * length );
         record.ValuesAsVector.Capacity = maxLen;
         for ( var i = 0; i < maxLen; ++i )
         {
            record.ValuesAsVector.Add( ReadMemberValue( state, record ) );
         }
      }

      private static T NewRecord<T>( DeserializationState state )
         where T : AbstractRecord, new()
      {
         var rec = new T();
         state.records.Add( state.array.ReadInt32LEFromBytes( ref state.idx ), rec );
         return rec;
      }

      private static String Read7BitLengthPrefixedString( this Byte[] array, ref Int32 idx, Encoding encoding = null )
      {
         var len = array.ReadInt32Encoded7Bit( ref idx );
         var str = ( encoding ?? UTF8 ).GetString( array, idx, len );
         idx += len;
         return str;
      }

      private static Boolean CheckPlaceholder( DeserializationState state, Object value, out AbstractRecord rec )
      {
         var record = value as AbstractRecord;
         var retVal = record != null;
         if ( retVal )
         {
            var ph = record as RecordPlaceholder;
            retVal = ph != null;
            rec = retVal ? state.records[( (RecordPlaceholder) record ).id] : null;
            if ( !retVal )
            {
               switch ( record.Kind )
               {
                  case RecordKind.Class:
                     var claas = (ClassRecord) record;
                     foreach ( var member in claas.Members )
                     {
                        if ( CheckPlaceholder( state, member.Value, out rec ) )
                        {
                           member.Value = rec;
                        }
                     }
                     break;
                  case RecordKind.Array:
                     var array = (ArrayRecord) record;
                     for ( var i = 0; i < array.ValuesAsVector.Count; ++i )
                     {
                        if ( CheckPlaceholder( state, array.ValuesAsVector[i], out rec ) )
                        {
                           array.ValuesAsVector[i] = rec;
                        }
                     }
                     break;
               }
            }
            ( (ClassRecord) rec ).IsSerializedInPlace = !retVal;
         }
         else
         {
            rec = null;
         }
         return retVal;
      }

      private sealed class DeserializationState
      {
         internal readonly Byte[] array;
         internal Int32 idx;
         internal readonly IDictionary<Int32, AbstractRecord> records;
         internal readonly IDictionary<Int32, String> assemblies;
         internal readonly IDictionary<Object, BinaryTypeEnumeration> typeInfos;
         internal readonly IDictionary<Object, PrimitiveTypeEnumeration> primitiveTypeInfos;
         internal readonly IDictionary<ArrayRecord, BinaryArrayTypeEnumeration> arrayTypeInfos;
         internal Boolean recordsEnded;
         internal Int32 nullCount;

         internal DeserializationState( Byte[] array, Int32 idx )
         {
            this.array = array;
            this.idx = idx;
            this.records = new Dictionary<Int32, AbstractRecord>();
            this.assemblies = new Dictionary<Int32, String>();
            this.typeInfos = new Dictionary<Object, BinaryTypeEnumeration>();
            this.primitiveTypeInfos = new Dictionary<Object, PrimitiveTypeEnumeration>();
            this.arrayTypeInfos = new Dictionary<ArrayRecord, BinaryArrayTypeEnumeration>();
            this.recordsEnded = false;
            this.nullCount = 0;
         }
      }

      private sealed class RecordPlaceholder : AbstractRecord
      {
         internal readonly Int32 id;

         internal RecordPlaceholder( Int32 anID )
         {
            this.id = anID;
         }

         public override RecordKind Kind
         {
            get
            {
               throw new InvalidOperationException( "This class is not direclty in use." );
            }
         }
      }
   }
}
