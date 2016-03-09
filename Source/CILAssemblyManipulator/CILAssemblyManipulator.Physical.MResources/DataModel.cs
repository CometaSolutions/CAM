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
using CollectionsWithRoles.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.MResources
{
   /// <summary>
   /// This is abstract class for entries that are managed by <see cref="System.Resources.ResourceManager"/>.
   /// </summary>
   public abstract class ResourceManagerEntry
   {
      internal ResourceManagerEntry()
      {

      }

      /// <summary>
      /// Gets the <see cref="MResources.ResourceManagerEntryKind"/> enumeration descripting the actual type of this <see cref="ResourceManagerEntry"/>.
      /// </summary>
      /// <value>The <see cref="MResources.ResourceManagerEntryKind"/> enumeration descripting the actual type of this <see cref="ResourceManagerEntry"/>.</value>
      /// <seealso cref="MResources.ResourceManagerEntryKind"/>
      public abstract ResourceManagerEntryKind ResourceManagerEntryKind { get; }
   }

   /// <summary>
   /// This class specializes <see cref="ResourceManagerEntry"/> for entry value, the type of which is one of the pre-defined types.
   /// </summary>
   public sealed class PreDefinedResourceManagerEntry : ResourceManagerEntry
   {

      ///// <summary>
      ///// Creates a new instance of <see cref="PreDefinedResourceManagerEntry"/> with given value.
      ///// </summary>
      ///// <param name="value"></param>
      //public PreDefinedResourceManagerEntry( Object value )
      //   : this( GetResourceTypeCode( value ), value )
      //{
      //}

      //internal PreDefinedResourceManagerEntry( ResourceTypeCode typeCode, Object value )
      //{
      //   this.ResourceTypeCode = typeCode;
      //   this.Value = value;
      //}

      ///// <summary>
      ///// Gets the <see cref="MResoures.ResourceTypeCode"/> enumeration for <see cref="Value"/>.
      ///// </summary>
      ///// <value>The <see cref="MResoures.ResourceTypeCode"/> enumeration for <see cref="Value"/>.</value>
      //public ResourceTypeCode ResourceTypeCode { get; }

      /// <summary>
      /// Gets or sets the value of this <see cref="PreDefinedResourceManagerEntry"/>.
      /// </summary>
      /// <value>The value of this <see cref="PreDefinedResourceManagerEntry"/>.</value>
      public Object Value { get; set; }

      /// <summary>
      /// Returns the <see cref="ResourceManagerEntryKind.PreDefined"/>.
      /// </summary>
      /// <value>The <see cref="ResourceManagerEntryKind.PreDefined"/>.</value>
      public override ResourceManagerEntryKind ResourceManagerEntryKind
      {
         get
         {
            return ResourceManagerEntryKind.PreDefined;
         }
      }

      /// <summary>
      /// Creates textual representation of this <see cref="PreDefinedResourceManagerEntry"/>.
      /// </summary>
      /// <returns>Textual representation of this <see cref="PreDefinedResourceManagerEntry"/>.</returns>
      public override String ToString()
      {
         var val = this.Value;
         var str = val as String;
         return str == null ? val.ToStringSafe( "<null>" ) : ( "\"" + str + "\"" );
      }

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
   /// This class specializes <see cref="ResourceManagerEntry"/> for entry value, the type of which is not one of the pre-defined ones.
   /// </summary>
   public sealed class UserDefinedResourceManagerEntry : ResourceManagerEntry
   {
      ///// <summary>
      ///// Gets or sets the textual type information of this <see cref="UserDefinedResourceManagerEntry"/>.
      ///// </summary>
      ///// <value>The textual type information of this <see cref="UserDefinedResourceManagerEntry"/>.</value>
      //public String UserDefinedType { get; set; }

      /// <summary>
      /// Gets or sets the contents of this <see cref="UserDefinedResourceManagerEntry"/> as <see cref="AbstractRecord"/>.
      /// </summary>
      /// <value>The contents of this <see cref="UserDefinedResourceManagerEntry"/> as <see cref="AbstractRecord"/>.</value>
      /// <seealso cref="AbstractRecord"/>
      public AbstractRecord Contents { get; set; }

      /// <summary>
      /// Returns the <see cref="ResourceManagerEntryKind.UserDefined"/>.
      /// </summary>
      /// <value>The <see cref="ResourceManagerEntryKind.UserDefined"/>.</value>
      public override ResourceManagerEntryKind ResourceManagerEntryKind
      {
         get
         {
            return ResourceManagerEntryKind.UserDefined;
         }
      }

      /// <summary>
      /// Creates textual representation of this <see cref="UserDefinedResourceManagerEntry"/>.
      /// </summary>
      /// <returns>Textual representation of this <see cref="UserDefinedResourceManagerEntry"/>.</returns>
      public override String ToString()
      {
         return "User-defined resource";
      }
   }

   /// <summary>
   /// This enumeration tells what type instance of <see cref="ResourceManagerEntry"/> really is.
   /// </summary>
   public enum ResourceManagerEntryKind
   {
      /// <summary>
      /// The <see cref="ResourceManagerEntry"/> is of type <see cref="PreDefinedResourceManagerEntry"/>.
      /// </summary>
      PreDefined,

      /// <summary>
      /// The <see cref="ResourceManagerEntry"/> is of type <see cref="UserDefinedResourceManagerEntry"/>.
      /// </summary>
      UserDefined
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
      /// The <see cref="AbstractRecord"/> is of type <see cref="ClassRecord"/>.
      /// </summary>
      Class,

      /// <summary>
      /// The <see cref="AbstractRecord"/> is of type <see cref="ArrayRecord"/>.
      /// </summary>
      Array,
   }

   /// <summary>
   /// This is common class for all records that make up the contents <see cref="UserDefinedResourceManagerEntry"/>.
   /// </summary>
   /// <remarks>
   /// It is recommended to read the document explaining the .NET Remoting Binary Protocol (NRBF), available from <see href="http://download.microsoft.com/download/9/5/E/95EF66AF-9026-4BB0-A41D-A4F81802D92C/%5BMS-NRBF%5D.pdf"/>.
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

      /// <summary>
      /// Creates the textual representation of this <see cref="ClassRecordMember"/>.
      /// </summary>
      /// <returns>The textual representation of this <see cref="ClassRecordMember"/>.</returns>
      /// <remarks>
      /// The textual representation of this <see cref="ClassRecordMember"/> includes the values of the <see cref="Name"/> and <see cref="Value"/> properties.
      /// </remarks>
      public override String ToString()
      {
         return this.Name + ":" + this.Value;
      }
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
}
