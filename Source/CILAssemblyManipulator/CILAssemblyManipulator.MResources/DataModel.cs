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
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.MResources
{
   public interface ElementWithTypeInfo
   {
      String AssemblyName { get; set; }
      String TypeName { get; set; }
   }

   public interface ElementWithValue
   {
      // Can be primitive or another AbstractRecord
      Object Value { get; set; }
   }

   public enum RecordKind
   {
      String,
      Class,
      Array,
      PrimitiveWrapper
   }

   public abstract class AbstractRecord
   {
      internal AbstractRecord()
      {
      }

      public abstract RecordKind Kind { get; }
   }

   public sealed class StringRecord : AbstractRecord
   {
      public StringRecord()
      {
      }

      public String StringValue { get; set; }

      public override RecordKind Kind
      {
         get
         {
            return RecordKind.String;
         }
      }

      public override Boolean Equals( Object obj )
      {
         return Object.ReferenceEquals( this, obj ) || this.DoesEqual( obj as StringRecord );
      }

      public override Int32 GetHashCode()
      {
         var str = this.StringValue;
         return str == null ? 0 : str.GetHashCode();
      }

      private Boolean DoesEqual( StringRecord rec )
      {
         return rec != null && String.Equals( rec.StringValue, this.StringValue );
      }


   }

   public sealed class ClassRecord : AbstractRecord, ElementWithTypeInfo
   {
      private readonly List<ClassRecordMember> _members;

      public ClassRecord()
      {
         this._members = new List<ClassRecordMember>();
      }

      public List<ClassRecordMember> Members
      {
         get
         {
            return this._members;
         }
      }

      public String TypeName { get; set; }
      public String AssemblyName { get; set; }

      public Boolean IsSerializedInPlace { get; set; }

      public override RecordKind Kind
      {
         get
         {
            return RecordKind.Class;
         }
      }
   }

   public sealed class ClassRecordMember : ElementWithTypeInfo, ElementWithValue
   {
      public String Name { get; set; }

      public String AssemblyName { get; set; }
      public String TypeName { get; set; }

      public Object Value { get; set; }
   }

   public sealed class ArrayRecord : AbstractRecord, ElementWithTypeInfo
   {
      private readonly List<Int32> _lengths;
      private readonly List<Int32> _lowerBounds;
      private readonly List<Object> _valuesAsVector;

      public ArrayRecord()
      {
         this._lengths = new List<Int32>();
         this._lowerBounds = new List<Int32>();
         this._valuesAsVector = new List<Object>();
         this.Rank = 1;
      }

      public Int32 Rank { get; set; }

      public BinaryArrayTypeEnumeration ArrayKind { get; set; }

      public List<Int32> Lengths
      {
         get
         {
            return this._lengths;
         }
      }

      public List<Int32> LowerBounds
      {
         get
         {
            return this._lowerBounds;
         }
      }

      public List<Object> ValuesAsVector
      {
         get
         {
            return this._valuesAsVector;
         }
      }

      public String AssemblyName { get; set; }
      public String TypeName { get; set; }


      public override RecordKind Kind
      {
         get
         {
            return RecordKind.Array;
         }
      }

   }

   public enum BinaryArrayTypeEnumeration
   {
      Single = 0,
      Jagged = 1,
      Rectangular = 2,
      SingleOffset = 3,
      JaggedOffset = 4,
      RectangularOffset = 5
   }

   public sealed class PrimitiveWrapperRecord : AbstractRecord, ElementWithValue
   {

      public PrimitiveWrapperRecord()
      {

      }

      public Object Value { get; set; }

      public override RecordKind Kind
      {
         get
         {
            return RecordKind.PrimitiveWrapper;
         }
      }

      //public override Boolean Equals( Object obj )
      //{
      //   return Object.ReferenceEquals( this, obj ) || this.DoesEqual( obj as PrimitiveWrapperRecord );
      //}

      //public override Int32 GetHashCode()
      //{
      //   var obj = this.Value;
      //   return obj == null ? 0 : obj.GetHashCode();
      //}

      //private Boolean DoesEqual( PrimitiveWrapperRecord other )
      //{
      //   return other != null && Object.Equals( this.Value, other.Value );
      //}
   }
}
