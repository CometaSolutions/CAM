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
using CILAssemblyManipulator.Physical.MResources;
using CollectionsWithRoles.API;

namespace CILAssemblyManipulator.Physical.MResources
{
   /// <summary>
   /// This class contains information about single entry in the manifest resource data written by <see cref="System.Resources.ResourceManager"/>.
   /// </summary>
   public sealed class ResourceManagerEntryInformation
   {
      /// <summary>
      /// Gets or sets the name of this resource.
      /// </summary>
      /// <value>The name of this resource.</value>
      public String Name { get; set; }

      /// <summary>
      /// Gets or sets the type of the resource, as user-defined textual type string.
      /// </summary>
      /// <value>The type of the resource, as user-defined textual type string.</value>
      /// <remarks>
      /// If this property is <c>null</c>, then the type of the resource can be deduced from <see cref="PredefinedTypeCode"/> property.
      /// </remarks>
      public String UserDefinedType { get; set; }

      /// <summary>
      /// Gets or sets the type of the resource as one of pre-defined types.
      /// </summary>
      /// <value>The type of the resource as one of pre-defined types.</value>
      /// <remarks>
      /// One should always check for <see cref="UserDefinedType"/> property before this property to find out the actual type of the resource.
      /// </remarks>
      public ResourceTypeCode PredefinedTypeCode { get; set; }

      /// <summary>
      /// Gets or sets the offset in the data where the contents of this resource start.
      /// </summary>
      /// <value>The offset in the data where the contents of this resource start.</value>
      public Int32 DataOffset { get; set; }

      /// <summary>
      /// Gets or sets the size of the data, in bytes.
      /// </summary>
      /// <value>The size of the data, in bytes.</value>
      public Int32 DataSize { get; set; }

      /// <summary>
      /// Gets the <see cref="ResourceTypeCode"/> which corresponds to the type of the given object.
      /// </summary>
      /// <param name="obj">The object, may be <c>null</c>.</param>
      /// <returns>The <see cref="ResourceTypeCode"/> for given <paramref name="obj"/>, or <c>null</c> if the <paramref name="obj"/> needs to be represented using <see cref="UserDefinedResourceManagerEntry"/>.</returns>
      public static ResourceTypeCode? GetResourceTypeCodeForObject( Object obj )
      {
         return GetResourceTypeCodeForType( obj?.GetType() );
      }

      /// <summary>
      /// Gets the <see cref="ResourceTypeCode"/> which corresponds to given type.
      /// </summary>
      /// <param name="type">The type, may be <c>null</c>.</param>
      /// <returns>The <see cref="ResourceTypeCode"/> for given <paramref name="type"/>, or <c>null</c> if the <paramref name="type"/> needs to be represented using <see cref="UserDefinedResourceManagerEntry"/>.</returns>
      public static ResourceTypeCode? GetResourceTypeCodeForType( Type type )
      {
         switch ( Type.GetTypeCode( type ) )
         {
            case TypeCode.Empty:
               return ResourceTypeCode.Null;
            case TypeCode.Boolean:
               return ResourceTypeCode.Boolean;
            case TypeCode.Char:
               return ResourceTypeCode.Char;
            case TypeCode.SByte:
               return ResourceTypeCode.SByte;
            case TypeCode.Byte:
               return ResourceTypeCode.Byte;
            case TypeCode.Int16:
               return ResourceTypeCode.Int16;
            case TypeCode.UInt16:
               return ResourceTypeCode.UInt16;
            case TypeCode.Int32:
               return ResourceTypeCode.Int32;
            case TypeCode.UInt32:
               return ResourceTypeCode.UInt32;
            case TypeCode.Int64:
               return ResourceTypeCode.Int64;
            case TypeCode.UInt64:
               return ResourceTypeCode.UInt64;
            case TypeCode.Single:
               return ResourceTypeCode.Single;
            case TypeCode.Double:
               return ResourceTypeCode.Double;
            case TypeCode.String:
               return ResourceTypeCode.String;
            case TypeCode.DateTime:
               return ResourceTypeCode.DateTime;
            case TypeCode.Decimal:
               return ResourceTypeCode.Decimal;
            default:
               if ( Equals( type, typeof( TimeSpan ) ) )
               {
                  return ResourceTypeCode.TimeSpan;
               }
               else if ( Equals( type, typeof( Byte[] ) ) )
               {
                  return ResourceTypeCode.ByteArray;
               }
               else if ( typeof( System.IO.Stream ).IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( type ) )
               {
                  return ResourceTypeCode.Stream;
               }
               else
               {
                  return null;
               }
         }

      }
   }

   /// <summary>
   /// This enumeration contains type codes for pre-defined resource types.
   /// </summary>
   public enum ResourceTypeCode
   {
      /// <summary>
      /// The resource is <c>null</c> value.
      /// </summary>
      Null = 0,

      /// <summary>
      /// The resource is a <see cref="System.String"/>.
      /// </summary>
      String = 1,

      /// <summary>
      /// The resource is a <see cref="System.Boolean"/>.
      /// </summary>
      Boolean = 2,

      /// <summary>
      /// The resource is a <see cref="System.Char"/>.
      /// </summary>
      Char = 3,

      /// <summary>
      /// The resource is a <see cref="System.Byte"/>.
      /// </summary>
      Byte = 4,

      /// <summary>
      /// The resource is a <see cref="System.SByte"/>.
      /// </summary>
      SByte = 5,

      /// <summary>
      /// The resource is a <see cref="System.Int16"/>.
      /// </summary>
      Int16 = 6,

      /// <summary>
      /// The resource is a <see cref="System.UInt16"/>.
      /// </summary>
      UInt16 = 7,

      /// <summary>
      /// The resource is a <see cref="System.Int32"/>.
      /// </summary>
      Int32 = 8,

      /// <summary>
      /// The resource is a <see cref="System.UInt32"/>.
      /// </summary>
      UInt32 = 9,

      /// <summary>
      /// The resource is a <see cref="System.Int64"/>.
      /// </summary>
      Int64 = 10,

      /// <summary>
      /// The resource is a <see cref="System.UInt64"/>.
      /// </summary>
      UInt64 = 11,

      /// <summary>
      /// The resource is a <see cref="System.Single"/>.
      /// </summary>
      Single = 12,

      /// <summary>
      /// The resource is a <see cref="System.Double"/>.
      /// </summary>
      Double = 13,

      /// <summary>
      /// The resource is a <see cref="System.Decimal"/>.
      /// </summary>
      Decimal = 14,

      /// <summary>
      /// The resource is a <see cref="System.DateTime"/>.
      /// </summary>
      DateTime = 15,

      /// <summary>
      /// This value is the biggest value for primitive types.
      /// </summary>
      LastPrimitive = 16,

      /// <summary>
      /// The resource is a <see cref="System.TimeSpan"/>.
      /// </summary>
      TimeSpan = 16,

      /// <summary>
      /// The resource is a byte array.
      /// </summary>
      ByteArray = 32,

      /// <summary>
      /// The resource is a <see cref="System.IO.Stream"/>.
      /// </summary>
      Stream = 33,

      /// <summary>
      /// This value indicates the first value which is used by user-defined types.
      /// </summary>
      StartOfUserTypes = 64,
   }

   /// <summary>
   /// This exception is thrown whenever something goes wrong when serializing or deserializing manifest resources (<see cref="ResourceManagerEntry"/>).
   /// </summary>
   public class ManifestResourceSerializationException : Exception
   {
      /// <summary>
      /// Creates a new instance of <see cref="ManifestResourceSerializationException"/> with given message and optional inner exception.
      /// </summary>
      /// <param name="msg">The message.</param>
      /// <param name="inner">The inner exception.</param>
      public ManifestResourceSerializationException( String msg, Exception inner = null )
         : base( msg, inner )
      {

      }
   }

   /// <summary>
   /// This class holds extension methods related to manifest resources, but for objects not defined in CAM.Physical framework.
   /// </summary>
   public static partial class MResourcesIO
   {
      private const UInt32 RES_MANAGER_HEADER_MAGIC = 0xBEEFCACEu;
      private static readonly Encoding UTF16LE = new UnicodeEncoding( false, false, true );

      /// <summary>
      /// Tries to read data of the manifest resource as enumerable of <see cref="ResourceManagerEntryInformation"/>s.
      /// </summary>
      /// <param name="array">The manifest resource data.</param>
      /// <param name="wasResourceManager">This parameter will be <c>true</c> if the data was written by <see cref="System.Resources.ResourceManager"/>. Otherwise it will be false.</param>
      /// <param name="idx">The optional index to start reading the <paramref name="array"/>.</param>
      /// <returns>The enumerable of <see cref="ResourceManagerEntryInformation"/>s. Will be empty, if <paramref name="wasResourceManager"/> will be <c>false</c>.</returns>
      public static IEnumerable<ResourceManagerEntryInformation> ReadResourceManagerEntries( this Byte[] array, out Boolean wasResourceManager, Int32 idx = 0 )
      {
         // See http://www.dotnetframework.org/default.aspx/4@0/4@0/untmp/DEVDIV_TFS/Dev10/Releases/RTMRel/ndp/clr/src/BCL/System/Resources/RuntimeResourceSet@cs/1305376/RuntimeResourceSet@cs for some explanation of format
         // Or ResReader.cs in ILRepack
         // http://primates.ximian.com/~lluis/dist/binary_serialization_format.htm Is also useful resource
         var startIdx = idx;
         var resHdrMagic = array.ReadUInt32LEFromBytes( ref idx );
         wasResourceManager = RES_MANAGER_HEADER_MAGIC == resHdrMagic;
         return wasResourceManager ?
            GetResourceInfoFromResourceManagerData( array, startIdx, idx ) :
            Empty<ResourceManagerEntryInformation>.Enumerable;
      }

      private static IEnumerable<ResourceManagerEntryInformation> GetResourceInfoFromResourceManagerData( Byte[] array, Int32 startIdx, Int32 idx )
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
         idx = idx.RoundUpI32( 8 );
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
            yield return new ResourceManagerEntryInformation()
            {
               Name = dataNamesAndOffsets[i].Item1,
               UserDefinedType = isUserType ? types[tIdx - (Int32) ResourceTypeCode.StartOfUserTypes] : null,
               PredefinedTypeCode = isUserType ? ResourceTypeCode.Null : (ResourceTypeCode) tIdx,
               DataOffset = iIdx,
               DataSize = ( i < resCount - 1 ? dataNamesAndOffsets[i + 1].Item2 : array.Length ) - iIdx
            };
         }

      }
   }
}

#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{
   /// <summary>
   /// Gets the value indicating whether this <see cref="ResourceManagerEntryInformation"/> represents information about object of user-defined type.
   /// </summary>
   /// <param name="item">The <see cref="ResourceManagerEntryInformation"/>.</param>
   /// <returns><c>true</c> if this <see cref="ResourceManagerEntryInformation"/> represents information about entry, which is of user-defined type.</returns>
   public static Boolean IsUserDefinedType( this ResourceManagerEntryInformation item )
   {
      return item.UserDefinedType != null;
   }

   /// <summary>
   /// Creates a new <see cref="ResourceManagerEntry"/> from this <see cref="ResourceManagerEntryInformation"/> and given data array.
   /// </summary>
   /// <param name="entry">The <see cref="ResourceManagerEntryInformation"/>.</param>
   /// <param name="array">The data array to use to read data based on <see cref="ResourceManagerEntryInformation.DataOffset"/> and <see cref="ResourceManagerEntryInformation.DataSize"/>.</param>
   /// <returns>A new <see cref="ResourceManagerEntry"/>.</returns>
   /// <remarks>
   /// The exact type of the returned object is determined based on properties of <see cref="ResourceManagerEntryInformation"/>.
   /// If <see cref="IsUserDefinedType"/> returns <c>true</c> for this <see cref="ResourceManagerEntryInformation"/>, then the returned object will be <see cref="UserDefinedResourceManagerEntry"/>.
   /// Otherwise, it will be <see cref="PreDefinedResourceManagerEntry"/>.
   /// </remarks>
   /// <seealso cref="ResourceManagerEntry"/>
   public static ResourceManagerEntry CreateEntry( this ResourceManagerEntryInformation entry, Byte[] array )
   {
      ResourceManagerEntry retVal;
      var idx = entry.DataOffset;
      if ( entry.IsUserDefinedType() )
      {
         // Read the NRBF records directly
         retVal = new UserDefinedResourceManagerEntry()
         {
            Contents = array.ReadUserDefinedEntryContents( idx, idx + entry.DataSize )
         };
      }
      else
      {
         // Pre-defined type
         Object value;
         var tc = entry.PredefinedTypeCode;
         switch ( tc )
         {
            case ResourceTypeCode.Null:
               value = null;
               break;
            case ResourceTypeCode.String:
               value = array.Read7BitLengthPrefixedString( ref idx );
               break;
            case ResourceTypeCode.Boolean:
               value = array.ReadByteFromBytes( ref idx ) > 0;
               break;
            case ResourceTypeCode.Char:
               value = (Char) array.ReadUInt16LEFromBytes( ref idx );
               break;
            case ResourceTypeCode.Byte:
               value = array.ReadByteFromBytes( ref idx );
               break;
            case ResourceTypeCode.SByte:
               value = array.ReadSByteFromBytes( ref idx );
               break;
            case ResourceTypeCode.Int16:
               value = array.ReadInt16LEFromBytes( ref idx );
               break;
            case ResourceTypeCode.UInt16:
               value = array.ReadUInt16LEFromBytes( ref idx );
               break;
            case ResourceTypeCode.Int32:
               value = array.ReadInt32LEFromBytes( ref idx );
               break;
            case ResourceTypeCode.UInt32:
               value = array.ReadUInt32LEFromBytes( ref idx );
               break;
            case ResourceTypeCode.Int64:
               value = array.ReadInt64LEFromBytes( ref idx );
               break;
            case ResourceTypeCode.UInt64:
               value = array.ReadUInt64LEFromBytes( ref idx );
               break;
            case ResourceTypeCode.Single:
               value = array.ReadSingleLEFromBytes( ref idx );
               break;
            case ResourceTypeCode.Double:
               value = array.ReadDoubleLEFromBytes( ref idx );
               break;
            case ResourceTypeCode.Decimal:
               value = array.ReadResourceManagerDecimal_AsResourceManagerEntry( ref idx );
               break;
            case ResourceTypeCode.DateTime:
               value = array.ReadResourceManagerDateTime_AsResourceManagerEntry( ref idx );
               break;
            case ResourceTypeCode.TimeSpan:
               value = array.ReadResourceManagerTimeSpan_AsResourceManagerEntry( ref idx );
               break;
            case ResourceTypeCode.ByteArray:
               var arrayLen = array.ReadInt32LEFromBytes( ref idx );
               value = array.CreateAndBlockCopyTo( ref idx, arrayLen );
               break;
            case ResourceTypeCode.Stream:
               var streamLen = array.ReadInt32LEFromBytes( ref idx );
               var ms = new MemoryStream();
               ms.Write( array, idx, streamLen );
               ms.Position = 0;
               value = ms;
               break;
            default:
               throw new ArgumentException( "The given resource manager entry has invalid resource type code: " + tc + "." );
         }
         retVal = new PreDefinedResourceManagerEntry()
         {
            Value = value
         };
      }
      return retVal;
   }

   private static AbstractRecord ReadUserDefinedEntryContents( this Byte[] array, Int32 idx, Int32 maxIndex )
   {
      var state = new DeserializationState( array, idx );

      // 1. Read records
      while ( state.idx < maxIndex && !state.recordsEnded )
      {
         ReadSingleRecord( state );
      }

      // 2. Check placeholders
      foreach ( var rec in state.records.Values )
      {
         CheckPlaceholder( state, rec as AbstractRecord );
      }

      // 3. Deduce what to return
      var topID = state.TopObjectID;
      Object record;
      AbstractRecord retVal;
      if ( !topID.HasValue )
      {
         throw new ManifestResourceSerializationException( "Missing serialization header." );
      }
      else if ( !state.records.TryGetValue( topID.Value, out record ) )
      {
         throw new ManifestResourceSerializationException( "The record for top ID " + topID.Value + " was not present." );
      }
      else if ( ( retVal = record as AbstractRecord ) == null )
      {
         throw new ManifestResourceSerializationException( "The object for top ID " + topID.Value + " was not a record." );
      }

      return retVal;
   }

   private static Object ReadSingleRecord( DeserializationState state )
   {
      Object retVal;
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
               state.TopObjectID = state.array.ReadInt32LEFromBytes( ref state.idx );
               state.array.Skip( ref state.idx, 12 ); // Skip the rest
               retVal = null;
               break;
            case RecordTypeEnumeration.ClassWithID:
               cRecord = NewRecord<ClassRecord>( state );
               var refRecord = (ClassRecord) state.records[state.array.ReadInt32LEFromBytes( ref state.idx )];
               // Copy metadata from referenced object
               cRecord.AssemblyName = refRecord.AssemblyName;
               cRecord.TypeName = refRecord.TypeName;
               cRecord.Members.Capacity = refRecord.Members.Count;
               cRecord.Members.AddRange( refRecord.Members.Select( refMember => new ClassRecordMember()
               {
                  AssemblyName = refMember.AssemblyName,
                  TypeName = refMember.TypeName,
                  Name = refMember.Name
               } ) );
               // Read values
               ReadMemberValues( state, cRecord, refRecord );
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
               var strID = state.array.ReadInt32LEFromBytes( ref state.idx );
               var str = state.array.Read7BitLengthPrefixedString( ref state.idx );
               if ( !state.records.ContainsKey( strID ) )
               {
                  state.records.Add( strID, str );
               }
               else
               {
                  // TODO what would be best here?
                  // Currently preserving old IDs.
               }
               retVal = str;
               break;
            case RecordTypeEnumeration.BinaryArray:
               aRecord = NewRecord<ArrayRecord>( state );
               ReadArrayInformation( state, aRecord );
               ReadArrayValues( state, aRecord );
               retVal = aRecord;
               break;
            case RecordTypeEnumeration.MemberPrimitiveTyped:
               retVal = ReadPrimitiveValue( state, (PrimitiveTypeEnumeration) state.array.ReadByteFromBytes( ref state.idx ) );
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
               throw new NotImplementedException( "Serialized method calls are not implemented" );
            case RecordTypeEnumeration.MethodReturn:
               throw new NotImplementedException( "Serialized method calls are not implemented" );
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

   private static void ReadMemberValues( DeserializationState state, ClassRecord record, ClassRecord refRecord = null )
   {
      var mems = record.Members;
      var refMems = refRecord == null ? mems : refRecord.Members;
      for ( var i = 0; i < mems.Count; ++i )
      {
         mems[i].Value = ReadMemberValue( state, refMems[i] );
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
            if ( UTF8.GetChars( state.array, state.idx, 4, state.charArray, 0 ) > 0 )
            {
               value = state.charArray[0];
               state.idx += UTF8.GetByteCount( state.charArray, 0, 1 );
            }
            else
            {
               throw new InvalidOperationException( "Char was not really a char?" );
            }
            break;
         case PrimitiveTypeEnumeration.Decimal:
            value = state.array.ReadResourceManagerDecimal_AsClassRecordMemberValue( ref state.idx );
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
            value = state.array.ReadResourceManagerTimeSpan_AsClassRecordMemberValue( ref state.idx );
            break;
         case PrimitiveTypeEnumeration.DateTime:
            value = state.array.ReadResourceManagerDateTime_AsClassRecordMemberValue( ref state.idx );
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

   internal static String Read7BitLengthPrefixedString( this Byte[] array, ref Int32 idx, Encoding encoding = null )
   {
      var len = array.ReadInt32Encoded7Bit( ref idx );
      var str = ( encoding ?? UTF8 ).GetString( array, idx, len );
      idx += len;
      return str;
   }

   private static void CheckPlaceholder( DeserializationState state, AbstractRecord record )
   {
      if ( record != null )
      {
         switch ( record.RecordKind )
         {
            case RecordKind.Class:
               var claas = (ClassRecord) record;
               foreach ( var member in claas.Members )
               {
                  Object cur;
                  if ( CheckPlaceholderValue( state, member.Value, out cur ) )
                  {
                     member.Value = cur;
                  }
               }
               break;
            case RecordKind.Array:
               var array = (ArrayRecord) record;
               for ( var i = 0; i < array.ValuesAsVector.Count; ++i )
               {
                  Object cur;
                  if ( CheckPlaceholderValue( state, array.ValuesAsVector[i], out cur ) )
                  {
                     array.ValuesAsVector[i] = cur;
                  }
               }
               break;
         }
      }
   }

   private static Boolean CheckPlaceholderValue( DeserializationState state, Object value, out Object rec )
   {
      var ph = value as RecordPlaceholder;
      rec = ph == null ? null : state.records[ph.ID];
      return ph != null;
   }

   private static DateTime ReadResourceManagerDateTime_AsResourceManagerEntry( this Byte[] array, ref Int32 idx )
   {
      return DateTime.FromBinary( array.ReadInt64LEFromBytes( ref idx ) );
   }

   private static TimeSpan ReadResourceManagerTimeSpan_AsResourceManagerEntry( this Byte[] array, ref Int32 idx )
   {
      return TimeSpan.FromTicks( array.ReadInt64LEFromBytes( ref idx ) );
   }

   private static DateTime ReadResourceManagerDateTime_AsClassRecordMemberValue( this Byte[] array, ref Int32 idx )
   {
      var rawDateTime = array.ReadUInt64LEFromBytes( ref idx );
      if ( ( rawDateTime & 0x8000000000000000uL ) != 0uL )
      {
         // This is local date-time, do UTC offset tick adjustment
         rawDateTime -= (UInt64) TimeZoneInfo.Local.GetUtcOffset( DateTime.MinValue ).Ticks;
      }
      return DateTime.FromBinary( (Int64) rawDateTime );
   }

   private static TimeSpan ReadResourceManagerTimeSpan_AsClassRecordMemberValue( this Byte[] array, ref Int32 idx )
   {
      return TimeSpan.FromTicks( array.ReadInt64LEFromBytes( ref idx ) );
   }

   private static Decimal ReadResourceManagerDecimal_AsResourceManagerEntry( this Byte[] array, ref Int32 idx )
   {
      return new Decimal( array.ReadInt32ArrayLEFromBytes( ref idx, 4 ) );
   }

   private static Decimal ReadResourceManagerDecimal_AsClassRecordMemberValue( this Byte[] array, ref Int32 idx )
   {
      Decimal d;
      Decimal.TryParse(
         array.Read7BitLengthPrefixedString( ref idx ),
         System.Globalization.NumberStyles.Number,
         System.Globalization.CultureInfo.InvariantCulture,
         out d
         );
      return d;
   }

   private sealed class DeserializationState
   {
      internal readonly Byte[] array;
      internal Int32 idx;
      internal readonly IDictionary<Int32, Object> records;
      internal readonly IDictionary<Int32, String> assemblies;
      internal readonly IDictionary<Object, BinaryTypeEnumeration> typeInfos;
      internal readonly IDictionary<Object, PrimitiveTypeEnumeration> primitiveTypeInfos;
      internal readonly IDictionary<ArrayRecord, BinaryArrayTypeEnumeration> arrayTypeInfos;
      internal Boolean recordsEnded;
      internal Int32 nullCount;
      internal Char[] charArray;

      internal DeserializationState( Byte[] array, Int32 idx )
      {
         this.array = array;
         this.idx = idx;
         this.records = new Dictionary<Int32, Object>();
         this.assemblies = new Dictionary<Int32, String>();
         this.typeInfos = new Dictionary<Object, BinaryTypeEnumeration>();
         this.primitiveTypeInfos = new Dictionary<Object, PrimitiveTypeEnumeration>();
         this.arrayTypeInfos = new Dictionary<ArrayRecord, BinaryArrayTypeEnumeration>();
         this.recordsEnded = false;
         this.nullCount = 0;
         this.charArray = new Char[4];
      }

      public Int32? TopObjectID { get; set; }
   }

   private sealed class RecordPlaceholder
   {
      internal RecordPlaceholder( Int32 anID )
      {
         this.ID = anID;
      }

      public Int32 ID { get; }
   }
}