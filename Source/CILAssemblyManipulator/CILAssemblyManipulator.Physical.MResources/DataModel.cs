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

namespace CILAssemblyManipulator.Physical.MResources
{
   /// <summary>
   /// This is interface for manifest resource elements which can have assembly and type name.
   /// </summary>
   /// <seealso cref="ClassRecord"/>
   /// <seealso cref="ClassRecordMember"/>
   /// <seealso cref="ArrayRecord"/>
   /// <seealso cref="AbstractRecord"/>
   public interface ElementWithTypeInfo
   {
      /// <summary>
      /// Gets or sets the textual assembly name of this element.
      /// </summary>
      /// <value>The textual assembly name of this element.</value>
      String AssemblyName { get; set; }

      /// <summary>
      /// Gets or sets the textual type name of this element.
      /// </summary>
      /// <value>The textual type name of this element.</value>
      String TypeName { get; set; }
   }

   /// <summary>
   /// This is interface for manifest resource elements which have a serializable value.
   /// </summary>
   public interface ElementWithValue
   {
      /// <summary>
      /// Gets or sets the value of this element.
      /// </summary>
      /// <value>The value of this element.</value>
      /// <remarks>
      /// If the value is not <c>null</c>, then the type of the value should be one of the following:
      /// <list type="bullet">
      /// <item><description>any sub-type of <see cref="AbstractRecord"/>,</description></item>
      /// <item><description><see cref="Boolean"/>,</description></item>
      /// <item><description><see cref="Byte"/>,</description></item>
      /// <item><description><see cref="Char"/>,</description></item>
      /// <item><description><see cref="Decimal"/>,</description></item>
      /// <item><description><see cref="Double"/>,</description></item>
      /// <item><description><see cref="Int16"/>,</description></item>
      /// <item><description><see cref="Int32"/>,</description></item>
      /// <item><description><see cref="Int64"/>,</description></item>
      /// <item><description><see cref="SByte"/>,</description></item>
      /// <item><description><see cref="Single"/>,</description></item>
      /// <item><description><see cref="TimeSpan"/>,</description></item>
      /// <item><description><see cref="DateTime"/>,</description></item>
      /// <item><description><see cref="UInt16"/>,</description></item>
      /// <item><description><see cref="UInt32"/>,</description></item>
      /// <item><description><see cref="UInt64"/>, or</description></item>
      /// <item><description><see cref="String"/>.</description></item>
      /// </list>
      /// </remarks>
      Object Value { get; set; }
   }

   /// <summary>
   /// This enumeration tells what type instance of <see cref="AbstractRecord"/> really is.
   /// </summary>
   public enum RecordKind
   {
      /// <summary>
      /// The <see cref="AbstractRecord"/> is of type <see cref="StringRecord"/>.
      /// </summary>
      String,

      /// <summary>
      /// The <see cref="AbstractRecord"/> is of type <see cref="ClassRecord"/>.
      /// </summary>
      Class,

      /// <summary>
      /// The <see cref="AbstractRecord"/> is of type <see cref="ArrayRecord"/>.
      /// </summary>
      Array,

      /// <summary>
      /// The <see cref="AbstractRecord"/> is of type <see cref="PrimitiveWrapperRecord"/>.
      /// </summary>
      PrimitiveWrapper
   }

   /// <summary>
   /// This is common class for all records that make up the contents of manifest resource.
   /// </summary>
   /// <remarks>
   /// It is recommended to read the document explaining the .NET remoting API, available from <see href="http://download.microsoft.com/download/9/5/E/95EF66AF-9026-4BB0-A41D-A4F81802D92C/%5BMS-NRBF%5D.pdf"/>.
   /// The various records and overall structure of contents of manifest resource are a subset of the protocol defined in that document.
   /// </remarks>
   public abstract class AbstractRecord
   {
      // Disable inheritance to other assemblies
      internal AbstractRecord()
      {
      }

      /// <summary>
      /// Gets the <see cref="MResources.RecordKind"/> enumeration descripting the actual type of this <see cref="AbstractRecord"/>.
      /// </summary>
      /// <value>The <see cref="MResources.RecordKind"/> enumeration descripting the actual type of this <see cref="AbstractRecord"/>.</value>
      /// <seealso cref="MResources.RecordKind"/>
      public abstract RecordKind RecordKind { get; }
   }

   /// <summary>
   /// This class represents a single string within the contents manifest resource.
   /// </summary>
   public sealed class StringRecord : AbstractRecord //, IEquatable<StringRecord>
   {
      /// <summary>
      /// Creates a blank instance of <see cref="StringRecord"/>.
      /// </summary>
      public StringRecord()
      {
      }

      /// <summary>
      /// Gets or sets the string value of this <see cref="StringRecord"/>.
      /// </summary>
      /// <value>The string value of this <see cref="StringRecord"/>.</value>
      public String StringValue { get; set; }

      /// <summary>
      /// Returns the <see cref="RecordKind.String"/>.
      /// </summary>
      /// <value>The <see cref="RecordKind.String"/>.</value>
      public override RecordKind RecordKind
      {
         get
         {
            return RecordKind.String;
         }
      }
   }

   /// <summary>
   /// This class represents a single instance of object within the contents of manifest resource.
   /// </summary>
   public sealed class ClassRecord : AbstractRecord, ElementWithTypeInfo
   {
      /// <summary>
      /// Creates a blank instance of <see cref="ClassRecord"/>.
      /// </summary>
      public ClassRecord()
      {
         this.Members = new List<ClassRecordMember>();
      }

      /// <summary>
      /// Gets the list of all members of the serialized object instance.
      /// </summary>
      /// <value>The list of all members of the serialized object instance.</value>
      /// <seealso cref="ClassRecordMember"/>
      public List<ClassRecordMember> Members { get; }

      /// <summary>
      /// Gets or sets the type name of the serialized object instance.
      /// </summary>
      /// <value>The type name of the serialized object instance.</value>
      public String TypeName { get; set; }

      /// <summary>
      /// Gets or sets the assembly name of the type of the serialized object instance.
      /// </summary>
      /// <value>The assembly name of the type of the serialized object instance.</value>
      public String AssemblyName { get; set; }

      /// <summary>
      /// Gets or sets the value indicating whether this record should be serialized in place, instead of being serialized as reference.
      /// </summary>
      /// <value>The value indicating whether this record should be serialized in place, instead of being serialized as reference.</value>
      /// <remarks>
      /// This property might be removed in the future.
      /// </remarks>
      public Boolean IsSerializedInPlace { get; set; }

      /// <summary>
      /// Returns the <see cref="RecordKind.Class"/>.
      /// </summary>
      /// <value>The <see cref="RecordKind.Class"/>.</value>
      public override RecordKind RecordKind
      {
         get
         {
            return RecordKind.Class;
         }
      }
   }

   /// <summary>
   /// This class represents a single field of the serialized object instance.
   /// </summary>
   public sealed class ClassRecordMember : ElementWithTypeInfo, ElementWithValue
   {
      /// <summary>
      /// Gets or sets the name of this field.
      /// </summary>
      /// <value>The name of this field.</value>
      public String Name { get; set; }

      /// <summary>
      /// Gets or sets the assembly name of the type name of the field type.
      /// </summary>
      /// <value>The assembly name of the type name of the field type.</value>
      public String AssemblyName { get; set; }

      /// <summary>
      /// Gets or sets the type name of the field type.
      /// </summary>
      /// <value>The type name of the field type.</value>
      public String TypeName { get; set; }

      /// <summary>
      /// Gets or sets the actual value of the field.
      /// </summary>
      /// <value>The actual value of the field.</value>
      /// <remarks>
      /// See <see cref="ElementWithValue.Value"/> property for more information about possible values for this property.
      /// </remarks>
      public Object Value { get; set; }
   }

   /// <summary>
   /// This class represents a serialized array of objects.
   /// </summary>
   public sealed class ArrayRecord : AbstractRecord, ElementWithTypeInfo
   {

      /// <summary>
      /// Creates a blank instance of <see cref="ArrayRecord"/>.
      /// </summary>
      public ArrayRecord()
      {
         this.Lengths = new List<Int32>();
         this.LowerBounds = new List<Int32>();
         this.ValuesAsVector = new List<Object>();
         this.Rank = 1;
      }

      /// <summary>
      /// Gets or sets the rank of the array.
      /// </summary>
      /// <value>The rank of the array.</value>
      public Int32 Rank { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="BinaryArrayTypeEnumeration"/> describing what kind of array this is.
      /// </summary>
      /// <value>The <see cref="BinaryArrayTypeEnumeration"/> describing what kind of array this is.</value>
      /// <seealso cref="BinaryArrayTypeEnumeration"/>
      public BinaryArrayTypeEnumeration ArrayKind { get; set; }

      /// <summary>
      /// Gets the list containing length of each dimension of this array.
      /// </summary>
      /// <value>The list containing length of each dimension of this array.</value>
      public List<Int32> Lengths { get; }

      /// <summary>
      /// Gets the list containing lower boundary for indices of each dimension of this array.
      /// </summary>
      /// <value>The list containing lower boundary for indices of each dimension of this array.</value>
      public List<Int32> LowerBounds { get; }

      /// <summary>
      /// Gets all values of this <see cref="ArrayRecord"/> as a single list.
      /// </summary>
      /// <value>All values of this <see cref="ArrayRecord"/> as a single list.</value>
      public List<Object> ValuesAsVector { get; }

      /// <summary>
      /// Gets or sets the assembly name of the type name of the array type.
      /// </summary>
      /// <value>The assembly name of the type name of the array type.</value>
      public String AssemblyName { get; set; }

      /// <summary>
      /// Gets or sets the type name of the array type.
      /// </summary>
      /// <value>The type name of the array type.</value>
      public String TypeName { get; set; }

      /// <summary>
      /// Returns the <see cref="RecordKind.Array"/>.
      /// </summary>
      /// <value>The <see cref="RecordKind.Array"/>.</value>
      public override RecordKind RecordKind
      {
         get
         {
            return RecordKind.Array;
         }
      }

   }

   /// <summary>
   /// This enumeration describes the kind of array represented by <see cref="ArrayRecord"/>.
   /// </summary>
   public enum BinaryArrayTypeEnumeration
   {
      /// <summary>
      /// The array is single-dimensional array.
      /// </summary>
      Single = 0,

      /// <summary>
      /// The array is jagged array, meaning that each element of the array is another array, potentially each one of different rank and size.
      /// </summary>
      Jagged = 1,

      /// <summary>
      /// The array is multi-dimensional rectangular array.
      /// </summary>
      Rectangular = 2,

      /// <summary>
      /// The array is single-dimensional array with lower bound index greater than <c>0</c>.
      /// </summary>
      SingleOffset = 3,

      /// <summary>
      /// The array array is jagged array with lower bound index greater than <c>0</c>.
      /// </summary>
      JaggedOffset = 4,

      /// <summary>
      /// The array is multi-dimensional array with at least one of the dimensions having lower bound index greater than <c>0</c>.
      /// </summary>
      RectangularOffset = 5
   }

   /// <summary>
   /// This class represents a wrapped primitive value as <see cref="AbstractRecord"/>.
   /// </summary>
   public sealed class PrimitiveWrapperRecord : AbstractRecord, ElementWithValue
   {
      /// <summary>
      /// Creates a new blank instance of <see cref="PrimitiveWrapperRecord"/>.
      /// </summary>
      public PrimitiveWrapperRecord()
      {

      }

      /// <summary>
      /// Gets or sets the primitive value of this <see cref="PrimitiveWrapperRecord"/>.
      /// </summary>
      /// <value>The primitive value of this <see cref="PrimitiveWrapperRecord"/>.</value>
      /// <remarks>
      /// See the <see cref="ElementWithValue.Value"/> property for possible values, except that <see cref="AbstractRecord"/> values should not be stored here.
      /// </remarks>
      public Object Value { get; set; }

      /// <summary>
      /// Returns the <see cref="RecordKind.PrimitiveWrapper"/>.
      /// </summary>
      /// <value>The <see cref="RecordKind.PrimitiveWrapper"/>.</value>
      public override RecordKind RecordKind
      {
         get
         {
            return RecordKind.PrimitiveWrapper;
         }
      }
   }
}
