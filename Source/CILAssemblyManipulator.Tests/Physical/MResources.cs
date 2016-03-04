/*
 * Copyright 2015 Stanislav Muhametsin. All rights Reserved.
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

extern alias CAMPhysicalM;

using CAMPhysicalM;
using CAMPhysicalM::CILAssemblyManipulator.Physical.MResources;

using CommonUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;

namespace CILAssemblyManipulator.Tests.Physical
{
   [Category( "CAM.Physical" )]
   public class MResourceTest : AbstractCAMTest
   {
      private const String NAME = "MyResource";
      const String FIELD_PREFIX = "<";
      const String FIELD_SUFFIX = ">k__BackingField";

      [Test]
      public void TestInteropWithNativeResourceManagerWriter_UserDefinedTypes()
      {
         var resourceValue = CreateNativeResourceObject();

         Byte[] serializedResource;
         using ( var stream = new MemoryStream() )
         {
            using ( var rw = new ResourceWriter( stream ) )
            {
               rw.AddResource( NAME, resourceValue );
               rw.Generate();
            }

            serializedResource = stream.ToArray();
         }

         Boolean resourceManagerIdentified;
         var resources = serializedResource.ReadResourceManagerEntries( out resourceManagerIdentified ).ToArray();
         Assert.IsTrue( resourceManagerIdentified );
         Assert.AreEqual( 1, resources.Length );
         var resInfo = resources[0];
         Assert.AreEqual( NAME, resInfo.Name );
         Assert.IsTrue( resInfo.IsUserDefinedType() );
         Assert.AreEqual( typeof( TestResourceType ).AssemblyQualifiedName, resInfo.UserDefinedType );
         var deserializedResoure = (UserDefinedResourceManagerEntry) resInfo.CreateEntry( serializedResource );
         var serializedResourceString = new String( serializedResource.Select( b => String.Format( "{0:X2}", b ) ).SelectMany( s => s.ToCharArray() ).ToArray() );
         Assert.IsTrue(
            CAMPhysicalM::CILAssemblyManipulator.Physical.Comparers.AbstractRecordEqualityComparer.Equals( deserializedResoure.Contents, CreateRecordFrom( resourceValue ) ),
            "Native value: {0}, serialized value: {1}",
            resourceValue, serializedResourceString
            );
      }

      [Test]
      public void TestInteropWithNativeResourceManagerWriter_PreDefinedTypes()
      {
         var resourceValue = CreateNativeResourceObject();
         var random = new Random();
         var primitiveInfo = new Tuple<String, ResourceTypeCode, PreDefinedResourceManagerEntry>[]
         {
            Tuple.Create( NAME + "_Null", ResourceTypeCode.Null, new PreDefinedResourceManagerEntry() { Value = resourceValue.NullValue } ),
            Tuple.Create( NAME + "_Boolean", ResourceTypeCode.Boolean, new PreDefinedResourceManagerEntry() { Value = resourceValue.BooleanValue } ),
            Tuple.Create( NAME + "_Char", ResourceTypeCode.Char, new PreDefinedResourceManagerEntry() { Value = resourceValue.CharValue } ),
            Tuple.Create( NAME + "_SByte", ResourceTypeCode.SByte, new PreDefinedResourceManagerEntry() { Value = resourceValue.SByteValue } ),
            Tuple.Create( NAME + "_Byte", ResourceTypeCode.Byte, new PreDefinedResourceManagerEntry() { Value = resourceValue.ByteValue } ),
            Tuple.Create( NAME + "_Int16", ResourceTypeCode.Int16, new PreDefinedResourceManagerEntry() { Value = resourceValue.Int16Value } ),
            Tuple.Create( NAME + "_UInt16", ResourceTypeCode.UInt16, new PreDefinedResourceManagerEntry() { Value = resourceValue.UInt16Value } ),
            Tuple.Create( NAME + "_Int32", ResourceTypeCode.Int32, new PreDefinedResourceManagerEntry() { Value = resourceValue.Int32Value } ),
            Tuple.Create( NAME + "_UInt32", ResourceTypeCode.UInt32, new PreDefinedResourceManagerEntry() { Value = resourceValue.UInt32Value } ),
            Tuple.Create( NAME + "_Int64", ResourceTypeCode.Int64, new PreDefinedResourceManagerEntry() { Value = resourceValue.Int64Value } ),
            Tuple.Create( NAME + "_UInt64", ResourceTypeCode.UInt64, new PreDefinedResourceManagerEntry() { Value = resourceValue.UInt64Value } ),
            Tuple.Create( NAME + "_Single", ResourceTypeCode.Single, new PreDefinedResourceManagerEntry() { Value = resourceValue.SingleValue } ),
            Tuple.Create( NAME + "_Double", ResourceTypeCode.Double, new PreDefinedResourceManagerEntry() { Value = resourceValue.DoubleValue } ),
            Tuple.Create( NAME + "_Decimal", ResourceTypeCode.Decimal, new PreDefinedResourceManagerEntry() { Value = resourceValue.DecimalValue } ),
            Tuple.Create( NAME + "_TimeSpan", ResourceTypeCode.TimeSpan, new PreDefinedResourceManagerEntry() { Value = resourceValue.TimeSpanValue } ),
            Tuple.Create( NAME + "_DateTime_Local", ResourceTypeCode.DateTime, new PreDefinedResourceManagerEntry() { Value = resourceValue.DateTimeValue_Local } ),
            Tuple.Create( NAME + "_DateTime_LocalAmbiguous", ResourceTypeCode.DateTime, new PreDefinedResourceManagerEntry() { Value = resourceValue.DateTimeValue_LocalAmbiguous } ),
            Tuple.Create( NAME + "_DateTime_UTC", ResourceTypeCode.DateTime, new PreDefinedResourceManagerEntry() { Value = resourceValue.DateTimeValue_UTC } ),
            Tuple.Create( NAME + "_DateTime_Unspecified", ResourceTypeCode.DateTime, new PreDefinedResourceManagerEntry() { Value = resourceValue.DateTimeValue_Unspecified } ),
            Tuple.Create( NAME + "_String", ResourceTypeCode.String, new PreDefinedResourceManagerEntry() { Value = resourceValue.StringValue } ),
            Tuple.Create( NAME + "_Bytes", ResourceTypeCode.ByteArray, new PreDefinedResourceManagerEntry() { Value = random.NextBytes( random.Next( 50, 100 ) ) } ),
            Tuple.Create( NAME + "_Stream", ResourceTypeCode.Stream, new PreDefinedResourceManagerEntry() { Value = new MemoryStream( random.NextBytes( random.Next( 50, 100 ) ) ) } ),
         };

         Byte[] serializedResource;
         using ( var stream = new MemoryStream() )
         {
            using ( var rw = new ResourceWriter( stream ) )
            {
               foreach ( var tuple in primitiveInfo )
               {
                  rw.AddResource( tuple.Item1, tuple.Item3.Value );
               }
               rw.Generate();
            }

            serializedResource = stream.ToArray();
         }

         Boolean resourceManagerIdentified;
         var resources = serializedResource.ReadResourceManagerEntries( out resourceManagerIdentified ).ToArray();
         Assert.IsTrue( resourceManagerIdentified );
         Assert.AreEqual( primitiveInfo.Length, resources.Length );
         var deserializedPrimitiveInfo = resources.Select( res => Tuple.Create( res.Name, res.PredefinedTypeCode, (PreDefinedResourceManagerEntry) res.CreateEntry( serializedResource ) ) ).ToArray();
         Assert.IsTrue(
            ArrayEqualityComparer<Tuple<String, ResourceTypeCode, PreDefinedResourceManagerEntry>>.IsPermutation(
               primitiveInfo,
               deserializedPrimitiveInfo,
               ComparerFromFunctions.NewEqualityComparer<Tuple<String, ResourceTypeCode, PreDefinedResourceManagerEntry>>(
                  ( x, y ) => String.Equals( x.Item1, y.Item1 ) && x.Item2 == y.Item2 && CAMPhysicalM::CILAssemblyManipulator.Physical.Comparers.PreDefinedResourceManagerEntryEqualityComparer.Equals( x.Item3, y.Item3 ),
                  x => x.Item1.GetHashCodeSafe()
                  )
               ),
            "Primitive values of {0} were not serialized properly",
            resourceValue );
      }

      [Test]
      public void TestInteropWithNativeResourceManagerWriter_Arrays()
      {
         var resourceValue = CreateNativeArrayResourceObject();

         Byte[] serializedResource;
         using ( var stream = new MemoryStream() )
         {
            using ( var rw = new ResourceWriter( stream ) )
            {
               rw.AddResource( NAME, resourceValue );
               rw.AddResource( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_Nulls ), resourceValue.Array_Nulls );
               rw.AddResource( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_String ), resourceValue.Array_String );
               rw.AddResource( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_Int32 ), resourceValue.Array_Int32 );
               rw.AddResource( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_UserDefined ), resourceValue.Array_UserDefined );
               rw.Generate();
            }

            serializedResource = stream.ToArray();
         }

         var resourceInfo = new Tuple<String, UserDefinedResourceManagerEntry>[]
         {
            Tuple.Create( NAME, new UserDefinedResourceManagerEntry() { UserDefinedType = resourceValue.GetType().AssemblyQualifiedName, Contents = CreateRecordFrom(resourceValue ) } ),
            Tuple.Create( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_Nulls ), new UserDefinedResourceManagerEntry() { UserDefinedType = typeof( Object[] ).AssemblyQualifiedName, Contents = resourceValue.Array_Nulls.CreateArrayRecord() } ),
            Tuple.Create( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_String ), new UserDefinedResourceManagerEntry() { UserDefinedType = typeof( String[] ).AssemblyQualifiedName, Contents = resourceValue.Array_String.CreateArrayRecord() } ),
            Tuple.Create( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_Int32 ), new UserDefinedResourceManagerEntry() { UserDefinedType = typeof( Int32[] ).AssemblyQualifiedName, Contents = resourceValue.Array_Int32.CreateArrayRecord() } ),
            Tuple.Create( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_UserDefined ), new UserDefinedResourceManagerEntry() { UserDefinedType = typeof( UserDefinedResourceManagerEntry[] ).AssemblyQualifiedName, Contents = resourceValue.Array_UserDefined.CreateArrayRecord( i => CreateRecordFrom( i ) ) } ),
         };

         Boolean resourceManagerIdentified;
         var resources = serializedResource.ReadResourceManagerEntries( out resourceManagerIdentified ).ToArray();
         Assert.IsTrue( resourceManagerIdentified );
         Assert.AreEqual( resourceInfo.Length, resources.Length );
         var deserializedPrimitiveInfo = resources.Select( res => Tuple.Create( res.Name, (UserDefinedResourceManagerEntry) res.CreateEntry( serializedResource ) ) ).ToArray();
         Assert.IsTrue(
            ArrayEqualityComparer<Tuple<String, UserDefinedResourceManagerEntry>>.IsPermutation(
               resourceInfo,
               deserializedPrimitiveInfo,
               ComparerFromFunctions.NewEqualityComparer<Tuple<String, UserDefinedResourceManagerEntry>>(
                  ( x, y ) => String.Equals( x.Item1, y.Item1 ) && CAMPhysicalM::CILAssemblyManipulator.Physical.Comparers.UserDefinedResourceManagerEntryEqualityComparer.Equals( x.Item2, y.Item2 ),
                  x => x.Item1.GetHashCodeSafe()
                  )
               ),
            "Primitive values of {0} were not serialized properly",
            resourceValue );

      }

      private static TestResourceType CreateNativeResourceObject()
      {
         var dtTicks = DateTime.UtcNow.Ticks;
         var r = new Random();
         return new TestResourceType()
         {
            NullValue = null,
            BooleanValue = r.Next() % 2 == 1,
            CharValue = (Char) r.Next(),
            SByteValue = (SByte) r.Next(),
            ByteValue = (Byte) r.Next(),
            Int16Value = (Int16) r.Next(),
            UInt16Value = (UInt16) r.Next(),
            Int32Value = r.NextInt32(),
            UInt32Value = (UInt32) r.NextInt32(),
            Int64Value = r.NextInt64(),
            UInt64Value = (UInt64) r.NextInt64(),
            SingleValue = (Single) r.NextDouble(),
            DoubleValue = r.NextDouble(),
            DecimalValue = r.NextDecimal(),
            TimeSpanValue = new TimeSpan( r.NextInt64() ),
            DateTimeValue_Local = DateTime.FromBinary( dtTicks | unchecked((Int64) 0x8000000000000000) ),
            DateTimeValue_LocalAmbiguous = DateTime.FromBinary( dtTicks | unchecked((Int64) 0xC000000000000000) ),
            DateTimeValue_UTC = DateTime.FromBinary( dtTicks | 0x4000000000000000 ),
            DateTimeValue_Unspecified = DateTime.FromBinary( dtTicks ),
            StringValue = r.NextString()
         };
      }

      private static TestSimpleArrayResourceType CreateNativeArrayResourceObject()
      {
         var r = new Random();
         return new TestSimpleArrayResourceType()
         {
            Array_Nulls = Enumerable.Repeat<Object>( null, r.Next( 10, 20 ) ).ToArray(),
            Array_String = Generate( r.Next( 10, 20 ), i => r.NextString() ).ToArray(),
            Array_Int32 = Generate( r.Next( 10, 20 ), i => r.NextInt32() ).ToArray(),
            Array_UserDefined = Generate( r.Next( 10, 20 ), i => CreateNativeResourceObject() ).ToArray()
         };
      }

      private static ClassRecord CreateRecordFrom( TestResourceType res )
      {
         var rec = new ClassRecord()
         {
            AssemblyName = res.GetType().Assembly.GetName().ToString(),
            TypeName = res.GetType().FullName
         };


         rec.Members.AddRange( new ClassRecordMember[]
         {
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.NullValue) + FIELD_SUFFIX,
               Value = res.NullValue
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.BooleanValue) + FIELD_SUFFIX,
               Value = res.BooleanValue
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.CharValue) + FIELD_SUFFIX,
               Value = res.CharValue
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.SByteValue) + FIELD_SUFFIX,
               Value = res.SByteValue
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.ByteValue) + FIELD_SUFFIX,
               Value = res.ByteValue
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.Int16Value) + FIELD_SUFFIX,
               Value = res.Int16Value
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.UInt16Value) + FIELD_SUFFIX,
               Value = res.UInt16Value
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.Int32Value) + FIELD_SUFFIX,
               Value = res.Int32Value
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.UInt32Value) + FIELD_SUFFIX,
               Value = res.UInt32Value
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.Int64Value) + FIELD_SUFFIX,
               Value = res.Int64Value
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.UInt64Value) + FIELD_SUFFIX,
               Value = res.UInt64Value
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.SingleValue) + FIELD_SUFFIX,
               Value = res.SingleValue
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.DoubleValue) + FIELD_SUFFIX,
               Value = res.DoubleValue
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.DecimalValue) + FIELD_SUFFIX,
               Value = res.DecimalValue
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.DateTimeValue_Local) + FIELD_SUFFIX,
               Value = res.DateTimeValue_Local
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.DateTimeValue_LocalAmbiguous) + FIELD_SUFFIX,
               Value = res.DateTimeValue_LocalAmbiguous
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.DateTimeValue_UTC) + FIELD_SUFFIX,
               Value = res.DateTimeValue_UTC
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.DateTimeValue_Unspecified) + FIELD_SUFFIX,
               Value = res.DateTimeValue_Unspecified
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.TimeSpanValue) + FIELD_SUFFIX,
               Value = res.TimeSpanValue
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestResourceType.StringValue) + FIELD_SUFFIX,
               Value = res.StringValue
            },
         } );

         return rec;
      }

      private static ClassRecord CreateRecordFrom( TestSimpleArrayResourceType res )
      {
         var rec = new ClassRecord()
         {
            AssemblyName = res.GetType().Assembly.GetName().ToString(),
            TypeName = res.GetType().FullName,
         };
         rec.Members.AddRange( new ClassRecordMember[]
         {
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestSimpleArrayResourceType.Array_Nulls) + FIELD_SUFFIX,
               Value = res.Array_Nulls.CreateArrayRecord()
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestSimpleArrayResourceType.Array_String) + FIELD_SUFFIX,
               Value = res.Array_String.CreateArrayRecord()
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestSimpleArrayResourceType.Array_Int32) + FIELD_SUFFIX,
               Value = res.Array_Int32.CreateArrayRecord()
            },
            new ClassRecordMember()
            {
               Name = FIELD_PREFIX + nameof(TestSimpleArrayResourceType.Array_UserDefined) + FIELD_SUFFIX,
               Value = res.Array_UserDefined.CreateArrayRecord( el => CreateRecordFrom(el))
            },
         } );
         return rec;
      }

      private static IEnumerable<T> Generate<T>( Int32 count, Func<Int32, T> generator )
      {
         for ( var i = 0; i < count; ++i )
         {
            yield return generator( i );
         }
      }
   }

   [Serializable]
   public class TestResourceType
   {
      public Object NullValue { get; set; }
      public Boolean BooleanValue { get; set; }

      public Char CharValue { get; set; }

      public SByte SByteValue { get; set; }

      public Byte ByteValue { get; set; }

      public Int16 Int16Value { get; set; }

      public UInt16 UInt16Value { get; set; }

      public Int32 Int32Value { get; set; }

      public UInt32 UInt32Value { get; set; }

      public Int64 Int64Value { get; set; }

      public UInt64 UInt64Value { get; set; }

      public Single SingleValue { get; set; }

      public Double DoubleValue { get; set; }

      public Decimal DecimalValue { get; set; }

      public DateTime DateTimeValue_Local { get; set; }

      public DateTime DateTimeValue_LocalAmbiguous { get; set; }

      public DateTime DateTimeValue_UTC { get; set; }

      public DateTime DateTimeValue_Unspecified { get; set; }

      public TimeSpan TimeSpanValue { get; set; }

      public String StringValue { get; set; }

      public override String ToString()
      {
         return "TESTRESOURCE: {\n"
            + nameof( BooleanValue ) + ":" + this.BooleanValue + "\n"
            + nameof( CharValue ) + ":" + this.CharValue + "\n"
            + nameof( SByteValue ) + ":" + this.SByteValue + "\n"
            + nameof( ByteValue ) + ":" + this.ByteValue + "\n"
            + nameof( Int16Value ) + ":" + this.Int16Value + "\n"
            + nameof( UInt16Value ) + ":" + this.UInt16Value + "\n"
            + nameof( Int32Value ) + ":" + this.Int32Value + "\n"
            + nameof( UInt32Value ) + ":" + this.UInt32Value + "\n"
            + nameof( Int64Value ) + ":" + this.Int64Value + "\n"
            + nameof( UInt64Value ) + ":" + this.UInt64Value + "\n"
            + nameof( SingleValue ) + ":" + this.SingleValue + "\n"
            + nameof( DoubleValue ) + ":" + this.DoubleValue + "\n"
            + nameof( DecimalValue ) + ":" + this.DecimalValue + "\n"
            + nameof( DateTimeValue_Local ) + ":" + this.DateTimeValue_Local + "\n"
            + nameof( DateTimeValue_LocalAmbiguous ) + ":" + this.DateTimeValue_LocalAmbiguous + "\n"
            + nameof( DateTimeValue_UTC ) + ":" + this.DateTimeValue_UTC + "\n"
            + nameof( DateTimeValue_Unspecified ) + ":" + this.DateTimeValue_Unspecified + "\n"
            + nameof( TimeSpanValue ) + ":" + this.TimeSpanValue + "\n"
            + nameof( StringValue ) + ":" + this.StringValue + "\n"
            + "}";
      }
   }

   [Serializable]
   public class TestSimpleArrayResourceType
   {
      public Object[] Array_Nulls { get; set; }

      public String[] Array_String { get; set; }

      public Int32[] Array_Int32 { get; set; }

      public TestResourceType[] Array_UserDefined { get; set; }

   }
}

public static class E_Util
{
   internal static ArrayRecord CreateArrayRecord<T>( this T[] array, Func<T, Object> itemProcessor = null )
   {
      var elementType = typeof( T );
      var retVal = new ArrayRecord()
      {
         ArrayKind = BinaryArrayTypeEnumeration.Single,
         AssemblyName = elementType.ToString(),
         TypeName = elementType.FullName,
         Rank = 1
      }.AddValuesToArrayRecord( array.Select( t => itemProcessor == null ? (Object) t : itemProcessor( t ) ) );
      retVal.Lengths.Add( array.Length );
      return retVal;
   }

   internal static ArrayRecord AddValuesToArrayRecord( this ArrayRecord record, IEnumerable<Object> values )
   {
      record.ValuesAsVector.AddRange( values );
      return record;
   }

   public static String NextString( this Random rng )
   {
      return rng.NextBytes( rng.Next( 10, 20 ) ).EncodeBase64( true );
   }

   public static Byte[] NextBytes( this Random rng, Int32 byteCount )
   {
      var bytez = new Byte[byteCount];
      rng.NextBytes( bytez );
      return bytez;
   }

   public static Int64 NextInt64( this Random rng )
   {
      return ( (Int64) rng.NextInt32() ) | ( (Int64) rng.NextInt32() << 32 );
   }

   // From Jon Skeet's answer on http://stackoverflow.com/questions/609501/generating-a-random-decimal-in-c-sharp
   /// <summary>
   /// Returns an Int32 with a random value across the entire range of
   /// possible values.
   /// </summary>
   public static int NextInt32( this Random rng )
   {
      unchecked
      {
         int firstBits = rng.Next( 0, 1 << 4 ) << 28;
         int lastBits = rng.Next( 0, 1 << 28 );
         return firstBits | lastBits;
      }
   }



   public static decimal NextDecimal( this Random rng )
   {
      byte scale = (byte) rng.Next( 29 );
      bool sign = rng.Next( 2 ) == 1;
      return new decimal( rng.NextInt32(),
                         rng.NextInt32(),
                         rng.NextInt32(),
                         sign,
                         scale );
   }
}