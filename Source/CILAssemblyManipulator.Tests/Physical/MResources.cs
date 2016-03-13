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

extern alias CAMPhysicalM;

using CAMPhysicalM;
using CAMPhysicalM::CILAssemblyManipulator.Physical.MResources;

using CILAssemblyManipulator.Physical.MResources;
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
      public void TestManifestResources_UserDefined_Object()
      {
         var resourceValue = CreateNativeResourceObject( null );
         PerformManifestResourceTest( new Tuple<String, Object, ResourceManagerEntry>[]
         {
            Tuple.Create<String, Object, ResourceManagerEntry>( NAME, resourceValue, new UserDefinedResourceManagerEntry() { Contents = CreateRecordFrom(resourceValue) } )
         } );
         //var resourceValue = CreateNativeResourceObject();

         //Byte[] serializedResource;
         //using ( var stream = new MemoryStream() )
         //{
         //   using ( var rw = new ResourceWriter( stream ) )
         //   {
         //      rw.AddResource( NAME, resourceValue );
         //      rw.Generate();
         //   }

         //   serializedResource = stream.ToArray();
         //}

         //Boolean resourceManagerIdentified;
         //var resources = serializedResource.ReadResourceManagerEntries( out resourceManagerIdentified ).ToArray();
         //Assert.IsTrue( resourceManagerIdentified );
         //Assert.AreEqual( 1, resources.Length );
         //var resInfo = resources[0];
         //Assert.AreEqual( NAME, resInfo.Name );
         //Assert.IsTrue( resInfo.IsUserDefinedType() );
         //Assert.AreEqual( typeof( TestResourceType ).AssemblyQualifiedName, resInfo.UserDefinedType );
         //var deserializedResoure = (UserDefinedResourceManagerEntry) resInfo.CreateEntry( serializedResource );
         //var serializedResourceString = new String( serializedResource.Select( b => String.Format( "{0:X2}", b ) ).SelectMany( s => s.ToCharArray() ).ToArray() );
         //Assert.IsTrue(
         //   CAMPhysicalM::CILAssemblyManipulator.Physical.Comparers.AbstractRecordEqualityComparer.Equals( deserializedResoure.Contents, CreateRecordFrom( resourceValue ) ),
         //   "Native value: {0}, serialized value: {1}",
         //   resourceValue, serializedResourceString
         //   );


      }

      [Test]
      public void TestManifestResources_PreDefined()
      {
         var random = new Random();
         var resourceValue = CreateNativeResourceObject( random );
         var bytez = random.NextBytes( random.Next( 50, 100 ) );
         using ( var stream = new MemoryStream( random.NextBytes( random.Next( 50, 100 ) ) ) )
         {
            PerformManifestResourceTest( new Tuple<String, Object, ResourceManagerEntry>[]
            {
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_Null", resourceValue.NullValue, new PreDefinedResourceManagerEntry() { Value = resourceValue.NullValue } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_Boolean", resourceValue.BooleanValue, new PreDefinedResourceManagerEntry() { Value = resourceValue.BooleanValue } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_Char", resourceValue.CharValue, new PreDefinedResourceManagerEntry() { Value = resourceValue.CharValue } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_SByte", resourceValue.SByteValue, new PreDefinedResourceManagerEntry() { Value = resourceValue.SByteValue } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_Byte", resourceValue.ByteValue, new PreDefinedResourceManagerEntry() { Value = resourceValue.ByteValue } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_Int16", resourceValue.Int16Value, new PreDefinedResourceManagerEntry() { Value = resourceValue.Int16Value } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_UInt16", resourceValue.UInt16Value, new PreDefinedResourceManagerEntry() { Value = resourceValue.UInt16Value } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_Int32", resourceValue.Int32Value, new PreDefinedResourceManagerEntry() { Value = resourceValue.Int32Value } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_UInt32", resourceValue.UInt32Value, new PreDefinedResourceManagerEntry() { Value = resourceValue.UInt32Value } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_Int64", resourceValue.Int64Value, new PreDefinedResourceManagerEntry() { Value = resourceValue.Int64Value } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_UInt64", resourceValue.UInt64Value, new PreDefinedResourceManagerEntry() { Value = resourceValue.UInt64Value } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_Single", resourceValue.SingleValue, new PreDefinedResourceManagerEntry() { Value = resourceValue.SingleValue } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_Double", resourceValue.DoubleValue, new PreDefinedResourceManagerEntry() { Value = resourceValue.DoubleValue } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_Decimal", resourceValue.DecimalValue, new PreDefinedResourceManagerEntry() { Value = resourceValue.DecimalValue } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_TimeSpan", resourceValue.TimeSpanValue, new PreDefinedResourceManagerEntry() { Value = resourceValue.TimeSpanValue } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_DateTime_Local", resourceValue.DateTimeValue_Local, new PreDefinedResourceManagerEntry() { Value = resourceValue.DateTimeValue_Local } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_DateTime_LocalAmbiguous", resourceValue.DateTimeValue_LocalAmbiguous, new PreDefinedResourceManagerEntry() { Value = resourceValue.DateTimeValue_LocalAmbiguous } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_DateTime_UTC", resourceValue.DateTimeValue_UTC, new PreDefinedResourceManagerEntry() { Value = resourceValue.DateTimeValue_UTC } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_DateTime_Unspecified", resourceValue.DateTimeValue_Unspecified, new PreDefinedResourceManagerEntry() { Value = resourceValue.DateTimeValue_Unspecified } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_String", resourceValue.StringValue, new PreDefinedResourceManagerEntry() { Value = resourceValue.StringValue } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_Bytes", bytez, new PreDefinedResourceManagerEntry() { Value = bytez } ),
               Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_Stream", stream, new PreDefinedResourceManagerEntry() { Value = stream } ),
            } );
         }
      }

      [Test]
      public void TestManifestResources_UserDefined_Arrays()
      {
         var resourceValue = CreateNativeArrayResourceObject();
         PerformManifestResourceTest( new Tuple<String, Object, ResourceManagerEntry>[]
         {
            Tuple.Create<String, Object, ResourceManagerEntry>( NAME, resourceValue, new UserDefinedResourceManagerEntry() { Contents = CreateRecordFrom(resourceValue ) } ),
            Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_Nulls ), resourceValue.Array_Nulls, new UserDefinedResourceManagerEntry() { Contents = resourceValue.Array_Nulls.CreateArrayRecord() } ),
            Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_String ), resourceValue.Array_String, new UserDefinedResourceManagerEntry() { Contents = resourceValue.Array_String.CreateArrayRecord() } ),
            Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_Int32 ),resourceValue.Array_Int32, new UserDefinedResourceManagerEntry() { Contents = resourceValue.Array_Int32.CreateArrayRecord() } ),
            Tuple.Create<String, Object, ResourceManagerEntry>( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_UserDefined ), resourceValue.Array_UserDefined, new UserDefinedResourceManagerEntry() { Contents = resourceValue.Array_UserDefined.CreateArrayRecord( i => CreateRecordFrom( i ) ) } ),
         } );



         //Byte[] serializedResource;
         //using ( var stream = new MemoryStream() )
         //{
         //   using ( var rw = new ResourceWriter( stream ) )
         //   {
         //      rw.AddResource( NAME, resourceValue );
         //      rw.AddResource( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_Nulls ), resourceValue.Array_Nulls );
         //      rw.AddResource( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_String ), resourceValue.Array_String );
         //      rw.AddResource( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_Int32 ), resourceValue.Array_Int32 );
         //      rw.AddResource( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_UserDefined ), resourceValue.Array_UserDefined );
         //      rw.Generate();
         //   }

         //   serializedResource = stream.ToArray();
         //}

         //var resourceInfo = new Tuple<String, UserDefinedResourceManagerEntry>[]
         //{
         //   Tuple.Create( NAME, new UserDefinedResourceManagerEntry() { Contents = CreateRecordFrom(resourceValue ) } ),
         //   Tuple.Create( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_Nulls ), new UserDefinedResourceManagerEntry() { Contents = resourceValue.Array_Nulls.CreateArrayRecord() } ),
         //   Tuple.Create( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_String ), new UserDefinedResourceManagerEntry() { Contents = resourceValue.Array_String.CreateArrayRecord() } ),
         //   Tuple.Create( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_Int32 ), new UserDefinedResourceManagerEntry() { Contents = resourceValue.Array_Int32.CreateArrayRecord() } ),
         //   Tuple.Create( NAME + "_" + nameof( TestSimpleArrayResourceType.Array_UserDefined ), new UserDefinedResourceManagerEntry() { Contents = resourceValue.Array_UserDefined.CreateArrayRecord( i => CreateRecordFrom( i ) ) } ),
         //};

         //Boolean resourceManagerIdentified;
         //var resources = serializedResource.ReadResourceManagerEntries( out resourceManagerIdentified ).ToArray();
         //Assert.IsTrue( resourceManagerIdentified );
         //Assert.AreEqual( resourceInfo.Length, resources.Length );
         //var deserializedInfo = resources.Select( res => Tuple.Create( res.Name, (UserDefinedResourceManagerEntry) res.CreateEntry( serializedResource ) ) ).ToArray();
         //Assert.IsTrue(
         //   ArrayEqualityComparer<Tuple<String, UserDefinedResourceManagerEntry>>.IsPermutation(
         //      resourceInfo,
         //      deserializedInfo,
         //      ComparerFromFunctions.NewEqualityComparer<Tuple<String, UserDefinedResourceManagerEntry>>(
         //         ( x, y ) => String.Equals( x.Item1, y.Item1 ) && CAMPhysicalM::CILAssemblyManipulator.Physical.Comparers.UserDefinedResourceManagerEntryEqualityComparer.Equals( x.Item2, y.Item2 ),
         //         x => x.Item1.GetHashCodeSafe()
         //         )
         //      ),
         //   "Array values of {0} were not serialized properly",
         //   resourceValue );

      }

      //[Test]
      //public void RoundtripTest_UserDefinedTypes()
      //{
      //   var resource = CreateNativeResourceObject( null );
      //   var typeStr = resource.GetType().AssemblyQualifiedName;
      //   var entry = new UserDefinedResourceManagerEntry()
      //   {
      //      Contents = CreateRecordFrom( resource )
      //   };
      //   Byte[] data;
      //   using ( var stream = new MemoryStream() )
      //   {
      //      entry.WriteEntry( stream );
      //      data = stream.ToArray();
      //   }

      //   var entryInfo = new ResourceManagerEntryInformation()
      //   {
      //      Name = NAME,
      //      DataOffset = 0,
      //      DataSize = data.Length,
      //      UserDefinedType = typeStr
      //   };
      //   var entry2 = entryInfo.CreateEntry( data ) as UserDefinedResourceManagerEntry;


      //   Assert.IsTrue(
      //      CAMPhysicalM::CILAssemblyManipulator.Physical.Comparers.UserDefinedResourceManagerEntryEqualityComparer.Equals( entry, entry2 ),
      //      "Native value: {0}", resource
      //      );


      //   Byte[] fullResourceData;
      //   using ( var stream = new MemoryStream() )
      //   {
      //      using ( var rw = new ResourceWriter( stream ) )
      //      {
      //         rw.AddResourceData( entryInfo.Name, entryInfo.UserDefinedType, data );
      //         rw.Generate();
      //      }
      //      fullResourceData = stream.ToArray();
      //   }

      //   Byte[] data2;
      //   using ( var stream = new MemoryStream() )
      //   {
      //      using ( var rw = new ResourceWriter( stream ) )
      //      {
      //         rw.AddResource( entryInfo.Name, resource );
      //      }
      //      var array = stream.ToArray();
      //      Boolean wasResMan;
      //      var info = array.ReadResourceManagerEntries( out wasResMan ).ToArray()[0];
      //      data2 = array.CreateArrayCopy( info.DataOffset, info.DataSize );
      //   }

      //   Tuple<String, Object>[] deserializedResources;
      //   using ( var stream = new MemoryStream( fullResourceData ) )
      //   using ( var rr = new ResourceReader( stream ) )
      //   {
      //      var enumz = rr.GetEnumerator();
      //      var list = new List<Tuple<String, Object>>();
      //      while ( enumz.MoveNext() )
      //      {
      //         list.Add( Tuple.Create( (String) enumz.Key, enumz.Value ) );
      //      }

      //      deserializedResources = list.ToArray();
      //   }

      //   Assert.IsTrue( resource.Equals( deserializedResources[0].Item2 ), "Serialized value did not equal deserialized: {0}\n\n{1}", resource, deserializedResources[0].Item2 );
      //}

      private static void PerformManifestResourceTest(
         Tuple<String, Object, ResourceManagerEntry>[] resources
         )
      {
         // 1. Test writing
         Byte[][] datas = new Byte[resources.Length][];
         for ( var i = 0; i < resources.Length; ++i )
         {
            var res = resources[i];
            using ( var stream = new MemoryStream() )
            {
               res.Item3.WriteEntry( stream );
               datas[i] = stream.ToArray();
            }
         }

         // 2. Test reading
         var createdInfos = resources.Select( ( tuple, idx ) =>
         {
            var res = resources[idx];
            var obj = res.Item2;
            var code = ResourceManagerEntryInformation.GetResourceTypeCodeForObject( obj );
            return new ResourceManagerEntryInformation()
            {
               Name = res.Item1,
               DataOffset = 0,
               DataSize = datas[idx].Length,
               UserDefinedType = code == null ? obj.GetType().AssemblyQualifiedName : null,
               PredefinedTypeCode = code ?? ResourceTypeCode.Null
            };
         } ).ToArray();
         for ( var i = 0; i < resources.Length; ++i )
         {
            var res = resources[i];
            var entry = createdInfos[i].CreateEntry( datas[i] );

            Assert.IsTrue(
               CAMPhysicalM::CILAssemblyManipulator.Physical.Comparers.ResourceManagerEntryEqualityComparer.Equals( entry, res.Item3 ),
               "Deserialized resource manager entry equality failed, serialized value: {0}", res.Item2
            );
         }

         // 3. Test interop
         Byte[] fullResourceData;
         using ( var stream = new MemoryStream() )
         {
            using ( var rw = new ResourceWriter( stream ) )
            {
               foreach ( var res in resources )
               {
                  rw.AddResource( res.Item1, res.Item2 );
               }
               rw.Generate();
            }
            fullResourceData = stream.ToArray();
         }
         Boolean wasResMan;
         var deserializedInfos = fullResourceData.ReadResourceManagerEntries( out wasResMan ).ToArray();
         Assert.IsTrue( wasResMan );
         Assert.IsTrue( ArrayEqualityComparer<ResourceManagerEntryInformation>.IsPermutation(
            createdInfos,
            deserializedInfos,
            ComparerFromFunctions.NewEqualityComparer<ResourceManagerEntryInformation>(
               ( x, y ) => ReferenceEquals( x, y ) ||
                  ( x != null && y != null
                  && x.PredefinedTypeCode == y.PredefinedTypeCode
                  && String.Equals( x.UserDefinedType, y.UserDefinedType )
                  ),
               x => x == null ? 0 : ( ( 17 * 23 + x.PredefinedTypeCode.GetHashCode() ) * 23 + x.UserDefinedType.GetHashCodeSafe() )
               )
            ) );

         var givenResources = resources.ToDictionary( tuple => tuple.Item1, tuple => tuple.Item3 );
         var deserializedResources = deserializedInfos.ToDictionary( resx => resx.Name, resx => resx.CreateEntry( fullResourceData ) );
         Assert.IsTrue( DictionaryEqualityComparer<String, ResourceManagerEntry>.NewDictionaryEqualityComparer( CAMPhysicalM::CILAssemblyManipulator.Physical.Comparers.ResourceManagerEntryEqualityComparer )
            .Equals( givenResources, deserializedResources ) );
      }

      private static TestResourceType CreateNativeResourceObject( Random r )
      {
         if ( r == null )
         {
            r = new Random();
         }
         var dtTicks = DateTime.UtcNow.Ticks;
         return new TestResourceType()
         {
            NullValue = null,
            BooleanValue = r.Next() % 2 == 1,
            CharValue = r.NextChar(),
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
            Array_UserDefined = Generate( r.Next( 10, 20 ), i => CreateNativeResourceObject( r ) ).ToArray()
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
               Value = res.Array_UserDefined.CreateArrayRecord( el => CreateRecordFrom(el)),
               AssemblyName = typeof(TestResourceType[]).Assembly.GetName().ToString(),
               TypeName = typeof(TestResourceType[]).FullName
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
   public class TestResourceType : IEquatable<TestResourceType>
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

      public override Boolean Equals( Object obj )
      {
         return this.Equals( obj as TestResourceType );
      }

      public override Int32 GetHashCode()
      {
         throw new NotSupportedException();
      }

      public Boolean Equals( TestResourceType other )
      {
         return ReferenceEquals( this, other ) ||
            ( other != null
            && this.NullValue == other.NullValue
            && this.BooleanValue == other.BooleanValue
            && this.CharValue == other.CharValue
            && this.SByteValue == other.SByteValue
            && this.ByteValue == other.ByteValue
            && this.Int16Value == other.Int16Value
            && this.UInt16Value == other.UInt16Value
            && this.Int32Value == other.Int32Value
            && this.UInt32Value == other.UInt32Value
            && this.Int64Value == other.Int64Value
            && this.UInt64Value == other.UInt64Value
            && this.SingleValue == other.SingleValue
            && this.DoubleValue == other.DoubleValue
            && this.DecimalValue == other.DecimalValue
            && this.DateTimeValue_Local == other.DateTimeValue_Local
            && this.DateTimeValue_LocalAmbiguous == other.DateTimeValue_LocalAmbiguous
            && this.DateTimeValue_UTC == other.DateTimeValue_UTC
            && this.DateTimeValue_Unspecified == other.DateTimeValue_Unspecified
            && this.TimeSpanValue == other.TimeSpanValue
            && this.StringValue == other.StringValue
            );
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
         ArrayKind = BinaryArrayTypeEnumeration.Single
      };
      retVal.ValuesAsVector.AddRange( array.Select( t => itemProcessor == null ? (Object) t : itemProcessor( t ) ) );
      if ( ArrayElementTypeNeedsInfo( array.GetType().GetElementType() ) )
      {
         retVal.AssemblyName = elementType.Assembly.GetName().ToString();
         retVal.TypeName = elementType.FullName;
      }
      retVal.Lengths.Add( array.Length );
      return retVal;
   }

   private static Boolean ArrayElementTypeNeedsInfo( Type elementType )
   {
      switch ( Type.GetTypeCode( elementType ) )
      {
         case TypeCode.DateTime:
         case TypeCode.Decimal:
            return true;
         case TypeCode.Object:
            return typeof( Object ).Equals( elementType ) ? false : true;
         default:
            return false;
      }
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

   public static Char NextChar( this Random rng )
   {
      Char ch;
      do
      {
         ch = (Char) rng.NextInt32();
      } while ( Char.IsSurrogate( ch ) );
      return ch;
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