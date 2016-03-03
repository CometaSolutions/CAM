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

      [Test]
      public void TestInteropWithNativeResourceManagerWriter()
      {
         var dtTicks = DateTime.UtcNow.Ticks;
         var r = new Random();
         var resourceValue = new TestResourceType()
         {
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
            StringValue = r.NextBytes( r.Next( 10, 20 ) ).EncodeBase64( true )
         };

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
         Assert.AreEqual( deserializedResoure.Contents.Count, 1 );
         var serialziedResourceString = new String( serializedResource.Select( b => String.Format( "{0:X2}", b ) ).SelectMany( s => s.ToCharArray() ).ToArray() );
         Assert.IsTrue(
            CAMPhysicalM::CILAssemblyManipulator.Physical.Comparers.AbstractRecordEqualityComparer.Equals( deserializedResoure.Contents[0], CreateRecordFrom( resourceValue ) ),
            "Native value: {0}, serialized value: {1}",
            resourceValue, serialziedResourceString
            );
      }

      private ClassRecord CreateRecordFrom( TestResourceType res )
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
               Name = "<" + nameof(TestResourceType.BooleanValue) + ">k__BackingField",
               Value = res.BooleanValue
            },
            new ClassRecordMember()
            {
               Name = "<" + nameof(TestResourceType.CharValue) + ">k__BackingField",
               Value = res.CharValue
            },
            new ClassRecordMember()
            {
               Name = "<" + nameof(TestResourceType.SByteValue) + ">k__BackingField",
               Value = res.SByteValue
            },
            new ClassRecordMember()
            {
               Name = "<" + nameof(TestResourceType.ByteValue) + ">k__BackingField",
               Value = res.ByteValue
            },
            new ClassRecordMember()
            {
               Name = "<" + nameof(TestResourceType.Int16Value) + ">k__BackingField",
               Value = res.Int16Value
            },
            new ClassRecordMember()
            {
               Name = "<" + nameof(TestResourceType.UInt16Value) + ">k__BackingField",
               Value = res.UInt16Value
            },
            new ClassRecordMember()
            {
               Name = "<" + nameof(TestResourceType.Int32Value) + ">k__BackingField",
               Value = res.Int32Value
            },
            new ClassRecordMember()
            {
               Name = "<" + nameof(TestResourceType.UInt32Value) + ">k__BackingField",
               Value = res.UInt32Value
            },
            new ClassRecordMember()
            {
               Name = "<" + nameof(TestResourceType.Int64Value) + ">k__BackingField",
               Value = res.Int64Value
            },
            new ClassRecordMember()
            {
               Name = "<" + nameof(TestResourceType.UInt64Value) + ">k__BackingField",
               Value = res.UInt64Value
            },
            new ClassRecordMember()
            {
               Name = "<" + nameof(TestResourceType.SingleValue) + ">k__BackingField",
               Value = res.SingleValue
            },
            new ClassRecordMember()
            {
               Name = "<" + nameof(TestResourceType.DoubleValue) + ">k__BackingField",
               Value = res.DoubleValue
            },
            new ClassRecordMember()
            {
               Name = "<" + nameof(TestResourceType.DecimalValue) + ">k__BackingField",
               Value = res.DecimalValue
            },
            new ClassRecordMember()
            {
               Name = "<" + nameof(TestResourceType.DateTimeValue_Local) + ">k__BackingField",
               Value = res.DateTimeValue_Local
            },
            new ClassRecordMember()
            {
               Name = "<" + nameof(TestResourceType.DateTimeValue_LocalAmbiguous) + ">k__BackingField",
               Value = res.DateTimeValue_LocalAmbiguous
            },
            new ClassRecordMember()
            {
               Name = "<" + nameof(TestResourceType.DateTimeValue_UTC) + ">k__BackingField",
               Value = res.DateTimeValue_UTC
            },
            new ClassRecordMember()
            {
               Name = "<" + nameof(TestResourceType.DateTimeValue_Unspecified) + ">k__BackingField",
               Value = res.DateTimeValue_Unspecified
            },
            new ClassRecordMember()
            {
               Name = "<" + nameof(TestResourceType.TimeSpanValue) + ">k__BackingField",
               Value = res.TimeSpanValue
            },
            new ClassRecordMember()
            {
               Name = "<" + nameof(TestResourceType.StringValue) + ">k__BackingField",
               Value = res.StringValue
            },
         } );

         return rec;
      }

   }

   [Serializable]
   public class TestResourceType
   {
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
}

public static class E_Util
{
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