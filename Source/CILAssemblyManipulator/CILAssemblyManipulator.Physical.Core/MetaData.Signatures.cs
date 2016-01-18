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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtils;
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Physical
{
   public enum SignatureKind
   {
      MethodDefinition,
      MethodReference,
      Field,
      Property,
      LocalVariables,
      Type,
      GenericMethodInstantiation,
      RawSignature
   }

   public abstract class AbstractSignature
   {
      // Disable inheritance to other assemblies
      internal AbstractSignature()
      {

      }

      public abstract SignatureKind SignatureKind { get; }
   }

   public sealed class RawSignature : AbstractSignature
   {
      public RawSignature()
      {

      }

      public Byte[] Bytes { get; set; }
      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.RawSignature;
         }
      }
   }

   public abstract class AbstractNotRawSignature : AbstractSignature
   {
      public Byte[] ExtraData { get; set; }

   }

   public abstract class AbstractMethodSignature : AbstractNotRawSignature
   {
      private readonly List<ParameterSignature> _parameters;

      // Disable inheritance to other assemblies
      internal AbstractMethodSignature( Int32 parameterCount )
      {
         this._parameters = new List<ParameterSignature>( parameterCount );
      }

      public MethodSignatureInformation MethodSignatureInformation { get; set; }
      public Int32 GenericArgumentCount { get; set; }
      public ParameterSignature ReturnType { get; set; }
      public List<ParameterSignature> Parameters
      {
         get
         {
            return this._parameters;
         }
      }
   }

   public sealed class MethodDefinitionSignature : AbstractMethodSignature
   {
      public MethodDefinitionSignature( Int32 parameterCount = 0 )
         : base( parameterCount )
      {
      }

      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.MethodDefinition;
         }
      }
   }

   public sealed class MethodReferenceSignature : AbstractMethodSignature
   {
      private readonly List<ParameterSignature> _varArgsParameters;

      public MethodReferenceSignature( Int32 parameterCount = 0, Int32 varArgsParameterCount = 0 )
         : base( parameterCount )
      {
         this._varArgsParameters = new List<ParameterSignature>( varArgsParameterCount );
      }

      public List<ParameterSignature> VarArgsParameters
      {
         get
         {
            return this._varArgsParameters;
         }
      }

      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.MethodReference;
         }
      }

   }

   public abstract class AbstractSignatureWithCustomMods : AbstractNotRawSignature
   {
      private readonly List<CustomModifierSignature> _customMods;

      // Disable inheritance to other assemblies
      internal AbstractSignatureWithCustomMods( Int32 customModCount )
      {
         this._customMods = new List<CustomModifierSignature>( customModCount );
      }

      public List<CustomModifierSignature> CustomModifiers
      {
         get
         {
            return this._customMods;
         }
      }
   }

   public sealed class FieldSignature : AbstractSignatureWithCustomMods
   {
      public FieldSignature( Int32 customModCount = 0 )
         : base( customModCount )
      {
      }

      public TypeSignature Type { get; set; }

      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.Field;
         }
      }
   }

   public sealed class PropertySignature : AbstractSignatureWithCustomMods
   {
      private readonly List<ParameterSignature> _parameters;

      public PropertySignature( Int32 customModCount = 0, Int32 parameterCount = 0 )
         : base( customModCount )
      {
         this._parameters = new List<ParameterSignature>( parameterCount );
      }

      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.Property;
         }
      }

      public Boolean HasThis { get; set; }
      public TypeSignature PropertyType { get; set; }
      public List<ParameterSignature> Parameters
      {
         get
         {
            return this._parameters;
         }
      }
   }

   public sealed class LocalVariablesSignature : AbstractNotRawSignature
   {
      private readonly List<LocalVariableSignature> _locals;

      public LocalVariablesSignature( Int32 localsCount = 0 )
      {
         this._locals = new List<LocalVariableSignature>();
      }

      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.LocalVariables;
         }
      }

      public List<LocalVariableSignature> Locals
      {
         get
         {
            return this._locals;
         }
      }
   }

   public abstract class ParameterOrLocalVariableSignature
   {
      private readonly List<CustomModifierSignature> _customMods;

      // Disable inheritance to other assemblies
      internal ParameterOrLocalVariableSignature( Int32 customModCount )
      {
         this._customMods = new List<CustomModifierSignature>( customModCount );
      }

      public List<CustomModifierSignature> CustomModifiers
      {
         get
         {
            return this._customMods;
         }
      }

      public Boolean IsByRef { get; set; }
      public TypeSignature Type { get; set; }
   }

   public sealed class LocalVariableSignature : ParameterOrLocalVariableSignature
   {
      public LocalVariableSignature( Int32 customModCount = 0 )
         : base( customModCount )
      {

      }

      public Boolean IsPinned { get; set; }

   }

   public sealed class ParameterSignature : ParameterOrLocalVariableSignature
   {
      public ParameterSignature( Int32 customModCount = 0 )
         : base( customModCount )
      {

      }
   }

   public sealed class CustomModifierSignature
   {
      public Boolean IsOptional { get; set; }
      public TableIndex CustomModifierType { get; set; }
   }

   public abstract class TypeSignature : AbstractNotRawSignature
   {
      // Disable inheritance to other assemblies
      internal TypeSignature()
      {

      }

      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.Type;
         }
      }

      public abstract TypeSignatureKind TypeSignatureKind { get; }

   }

   public enum TypeSignatureKind : byte
   {
      Simple,
      ComplexArray,
      ClassOrValue,
      GenericParameter,
      FunctionPointer,
      Pointer,
      SimpleArray,
   }

   public sealed class SimpleTypeSignature : TypeSignature
   {
      public static readonly SimpleTypeSignature Boolean = new SimpleTypeSignature( SimpleTypeSignatureKind.Boolean );
      public static readonly SimpleTypeSignature Char = new SimpleTypeSignature( SimpleTypeSignatureKind.Char );
      public static readonly SimpleTypeSignature SByte = new SimpleTypeSignature( SimpleTypeSignatureKind.I1 );
      public static readonly SimpleTypeSignature Byte = new SimpleTypeSignature( SimpleTypeSignatureKind.U1 );
      public static readonly SimpleTypeSignature Int16 = new SimpleTypeSignature( SimpleTypeSignatureKind.I2 );
      public static readonly SimpleTypeSignature UInt16 = new SimpleTypeSignature( SimpleTypeSignatureKind.U2 );
      public static readonly SimpleTypeSignature Int32 = new SimpleTypeSignature( SimpleTypeSignatureKind.I4 );
      public static readonly SimpleTypeSignature UInt32 = new SimpleTypeSignature( SimpleTypeSignatureKind.U4 );
      public static readonly SimpleTypeSignature Int64 = new SimpleTypeSignature( SimpleTypeSignatureKind.I8 );
      public static readonly SimpleTypeSignature UInt64 = new SimpleTypeSignature( SimpleTypeSignatureKind.U8 );
      public static readonly SimpleTypeSignature Single = new SimpleTypeSignature( SimpleTypeSignatureKind.R4 );
      public static readonly SimpleTypeSignature Double = new SimpleTypeSignature( SimpleTypeSignatureKind.R8 );
      public static readonly SimpleTypeSignature IntPtr = new SimpleTypeSignature( SimpleTypeSignatureKind.I );
      public static readonly SimpleTypeSignature UIntPtr = new SimpleTypeSignature( SimpleTypeSignatureKind.U );
      public static readonly SimpleTypeSignature Object = new SimpleTypeSignature( SimpleTypeSignatureKind.Object );
      public static readonly SimpleTypeSignature String = new SimpleTypeSignature( SimpleTypeSignatureKind.String );
      public static readonly SimpleTypeSignature Void = new SimpleTypeSignature( SimpleTypeSignatureKind.Void );
      public static readonly SimpleTypeSignature TypedByRef = new SimpleTypeSignature( SimpleTypeSignatureKind.TypedByRef );

      private SimpleTypeSignature( SimpleTypeSignatureKind type )
      {
         this.SimpleType = type;
      }

      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.Simple;
         }
      }

      public SimpleTypeSignatureKind SimpleType { get; }



      public static SimpleTypeSignature GetByKind( SimpleTypeSignatureKind kind )
      {
         SimpleTypeSignature retVal;
         if ( !TryGetByKind( kind, out retVal ) )
         {
            throw new ArgumentException( "Unrecognized simple type signature kind: " + kind + "." );
         }
         return retVal;
      }

      public static Boolean TryGetByKind( SimpleTypeSignatureKind kind, out SimpleTypeSignature simpleType )
      {
         switch ( kind )
         {
            case SimpleTypeSignatureKind.Boolean:
               simpleType = Boolean;
               break;
            case SimpleTypeSignatureKind.Char:
               simpleType = Char;
               break;
            case SimpleTypeSignatureKind.I1:
               simpleType = SByte;
               break;
            case SimpleTypeSignatureKind.U1:
               simpleType = Byte;
               break;
            case SimpleTypeSignatureKind.I2:
               simpleType = Int16;
               break;
            case SimpleTypeSignatureKind.U2:
               simpleType = UInt16;
               break;
            case SimpleTypeSignatureKind.I4:
               simpleType = Int32;
               break;
            case SimpleTypeSignatureKind.U4:
               simpleType = UInt32;
               break;
            case SimpleTypeSignatureKind.I8:
               simpleType = Int64;
               break;
            case SimpleTypeSignatureKind.U8:
               simpleType = UInt64;
               break;
            case SimpleTypeSignatureKind.R4:
               simpleType = Single;
               break;
            case SimpleTypeSignatureKind.R8:
               simpleType = Double;
               break;
            case SimpleTypeSignatureKind.I:
               simpleType = IntPtr;
               break;
            case SimpleTypeSignatureKind.U:
               simpleType = UIntPtr;
               break;
            case SimpleTypeSignatureKind.Object:
               simpleType = Object;
               break;
            case SimpleTypeSignatureKind.String:
               simpleType = String;
               break;
            case SimpleTypeSignatureKind.Void:
               simpleType = Void;
               break;
            case SimpleTypeSignatureKind.TypedByRef:
               simpleType = TypedByRef;
               break;
            default:
               simpleType = null;
               break;
         }

         return simpleType != null;
      }
   }

   /// <summary>
   /// This enumeration represents the kind of <see cref="SimpleTypeSignature"/>.
   /// </summary>
   /// <remarks>
   /// The values of this enumeration are safe to be casted to <see cref="T:CILAssemblyManipulator.Physical.IO.SignatureElementTypes"/>.
   /// </remarks>
   public enum SimpleTypeSignatureKind : byte
   {
      Void = 0x01, // Same as SignatureElementTypes.Void
      Boolean,
      Char,
      I1,
      U1,
      I2,
      U2,
      I4,
      U4,
      I8,
      U8,
      R4,
      R8,
      String,
      TypedByRef = 0x16, // Same as SignatureElementTypes.TypedByRef
      I = 0x18, // Same as SignatureElementTypes.I
      U,
      Object = 0x1C, // Same as SignatureElementTypes.Object
   }

   public sealed class ClassOrValueTypeSignature : TypeSignature
   {
      private readonly List<TypeSignature> _genericArguments;

      public ClassOrValueTypeSignature( Int32 genericArgumentsCount = 0 )
      {
         this._genericArguments = new List<TypeSignature>( genericArgumentsCount );
      }

      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.ClassOrValue;
         }
      }

      public Boolean IsClass { get; set; }
      public TableIndex Type { get; set; }
      public List<TypeSignature> GenericArguments
      {
         get
         {
            return this._genericArguments;
         }
      }
   }

   public sealed class GenericParameterTypeSignature : TypeSignature
   {

      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.GenericParameter;
         }
      }

      public Boolean IsTypeParameter { get; set; }
      public Int32 GenericParameterIndex { get; set; }
   }

   public sealed class FunctionPointerTypeSignature : TypeSignature
   {

      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.FunctionPointer;
         }
      }

      public MethodReferenceSignature MethodSignature { get; set; }
   }

   public sealed class PointerTypeSignature : TypeSignature
   {
      private readonly List<CustomModifierSignature> _customMods;

      public PointerTypeSignature( Int32 customModCount = 0 )
      {
         this._customMods = new List<CustomModifierSignature>( customModCount );
      }

      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.Pointer;
         }
      }

      public List<CustomModifierSignature> CustomModifiers
      {
         get
         {
            return this._customMods;
         }
      }

      public TypeSignature PointerType { get; set; }
   }

   public abstract class AbstractArrayTypeSignature : TypeSignature
   {
      // Disable inheritance to other assemblies
      internal AbstractArrayTypeSignature()
      {

      }

      public TypeSignature ArrayType { get; set; }
   }

   public sealed class ComplexArrayTypeSignature : AbstractArrayTypeSignature
   {
      private readonly List<Int32> _sizes;
      private readonly List<Int32> _lowerBounds;

      public ComplexArrayTypeSignature( Int32 sizesCount = 0, Int32 lowerBoundsCount = 0 )
      {
         this._sizes = new List<Int32>( sizesCount );
         this._lowerBounds = new List<Int32>( lowerBoundsCount );
      }

      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.ComplexArray;
         }
      }

      public Int32 Rank { get; set; }
      public List<Int32> Sizes
      {
         get
         {
            return this._sizes;
         }
      }
      public List<Int32> LowerBounds
      {
         get
         {
            return this._lowerBounds;
         }
      }
   }

   public sealed class SimpleArrayTypeSignature : AbstractArrayTypeSignature
   {
      private readonly List<CustomModifierSignature> _customMods;

      public SimpleArrayTypeSignature( Int32 customModCount = 0 )
      {
         this._customMods = new List<CustomModifierSignature>( customModCount );
      }

      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.SimpleArray;
         }
      }

      public List<CustomModifierSignature> CustomModifiers
      {
         get
         {
            return this._customMods;
         }
      }
   }

   public sealed class GenericMethodSignature : AbstractNotRawSignature
   {
      private readonly List<TypeSignature> _genericArguments;

      public GenericMethodSignature( Int32 genericArgumentsCount = 0 )
      {
         this._genericArguments = new List<TypeSignature>( genericArgumentsCount );
      }

      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.GenericMethodInstantiation;
         }
      }

      public List<TypeSignature> GenericArguments
      {
         get
         {
            return this._genericArguments;
         }
      }
   }

   /// <summary>
   /// This class represents marshalling information used for CIL parameters and fields.
   /// Subclasses of this class further define what kind of marshaling info is in question.
   /// </summary>
   /// <seealso cref="CILElementWithMarshalingInfo"/>
   /// <seealso cref="CILField"/>
   /// <seealso cref="CILParameter"/>
   public abstract class AbstractMarshalingInfo
   {
      // Disable inheritance to other assemblies
      internal AbstractMarshalingInfo()
      {
      }

      /// <summary>
      /// Gets the <see cref="UnmanagedType" /> value the data is to be marshaled as.
      /// </summary>
      /// <value>The <see cref="UnmanagedType" /> value the data is to be marshaled as.</value>
      public UnmanagedType Value { get; set; }

      public abstract MarshalingInfoKind MarshalingInfoKind { get; }

      /// <summary>
      /// Creates a new <see cref="AbstractMarshalingInfo"/> from the information from <see cref="T:System.Runtime.InteropServices.MarshalAsAttribute"/> attribute type.
      /// Since the attribute type is not included in all portable profiles, this method does not take the attribute as parameter directly.
      /// </summary>
      /// <param name="ut">The value of <see cref="P:System.Runtime.InteropServices.MarshalAsAttribute.Value"/>.</param>
      /// <param name="sizeConst">The value of <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.SizeConst"/>.</param>
      /// <param name="iidParameterIndex">The value of <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.IidParameterIndex"/>.</param>
      /// <param name="sizeParameterIndex">The value of <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.SizeParamIndex"/>.</param>
      /// <param name="arraySubType">The value of <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.ArraySubType"/>.</param>
      /// <param name="safeArraySubType">The value of <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.SafeArraySubType"/>.</param>
      /// <param name="safeArrayUserDefinedSubType">The value of <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.SafeArrayUserDefinedSubType"/>.</param>
      /// <param name="customMarshalType">The value of <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.MarshalType"/>.</param>
      /// <param name="customMarshalCookie">The value of <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.MarshalCookie"/>.</param>
      /// <returns>A new <see cref="AbstractMarshalingInfo"/> with given information.</returns>
      public static AbstractMarshalingInfo FromAttributeInfo(
         UnmanagedType ut,
         Int32 sizeConst,
         Int32 iidParameterIndex,
         Int16 sizeParameterIndex,
         UnmanagedType arraySubType,
         VarEnum safeArraySubType,
         Type safeArrayUserDefinedSubType,
         String customMarshalType,
         String customMarshalCookie
         )
      {
         AbstractMarshalingInfo result;
         switch ( ut )
         {
            case UnmanagedType.ByValTStr:
               result = new FixedLengthStringMarshalingInfo()
               {
                  Value = ut,
                  Size = sizeConst
               };
               break;
            case UnmanagedType.IUnknown:
            case UnmanagedType.IDispatch:
            case UnmanagedType.Interface:
               result = new InterfaceMarshalingInfo()
               {
                  Value = ut,
                  IIDParameterIndex = iidParameterIndex
               };
               break;
            case UnmanagedType.SafeArray:
               result = new SafeArrayMarshalingInfo()
               {
                  Value = ut,
                  ElementType = safeArraySubType,
                  UserDefinedType = safeArrayUserDefinedSubType?.AssemblyQualifiedName
               };
               break;
            case UnmanagedType.ByValArray:
               result = new FixedLengthArrayMarshalingInfo()
               {
                  Value = ut,
                  Size = sizeConst,
                  ElementType = arraySubType
               };
               break;
            case UnmanagedType.LPArray:
               result = new ArrayMarshalingInfo()
               {
                  Value = ut,
                  ElementType = arraySubType,
                  SizeParameterIndex = sizeParameterIndex,
                  Size = sizeConst,
                  Flags = -1
               };
               break;
            case UnmanagedType.CustomMarshaler:
               result = new CustomMarshalingInfo()
               {
                  Value = ut,
                  GUIDString = null,
                  NativeTypeName = null,
                  CustomMarshalerTypeName = customMarshalType,
                  MarshalCookie = customMarshalCookie
               };
               break;
            default:
               result = new SimpleMarshalingInfo()
               {
                  Value = ut
               };
               break;
         }

         return result;
      }


#if !CAM_PHYSICAL_IS_PORTABLE

      /// <summary>
      /// Creates <see cref="AbstractMarshalingInfo"/> with all information specified in <see cref="System.Runtime.InteropServices.MarshalAsAttribute"/>.
      /// </summary>
      /// <param name="attr">The <see cref="System.Runtime.InteropServices.MarshalAsAttribute"/>. If <c>null</c>, then the result will be <c>null</c> as well.</param>
      /// <returns>A new <see cref="AbstractMarshalingInfo"/> with given information, or <c>null</c> if <paramref name="attr"/> is <c>null</c>.</returns>
      /// <remarks>
      /// This is a wrapper around <see cref="FromAttributeInfo"/> method.
      /// </remarks>
      public static AbstractMarshalingInfo FromAttribute( System.Runtime.InteropServices.MarshalAsAttribute attr )
      {
         return attr == null ?
            null :
            FromAttributeInfo( (UnmanagedType) attr.Value, attr.SizeConst, attr.IidParameterIndex, attr.SizeParamIndex, (UnmanagedType) attr.ArraySubType, (VarEnum) attr.SafeArraySubType, attr.SafeArrayUserDefinedSubType, attr.MarshalType, attr.MarshalCookie );
      }

#endif



   }

   public enum MarshalingInfoKind
   {
      Simple,
      FixedLengthString,
      FixedLengthArray,
      SafeArray,
      Array,
      Interface,
      Custom,
      Raw
   }

   public sealed class SimpleMarshalingInfo : AbstractMarshalingInfo
   {
      public override MarshalingInfoKind MarshalingInfoKind
      {
         get
         {
            return MarshalingInfoKind.Simple;
         }
      }
   }

   public sealed class FixedLengthStringMarshalingInfo : AbstractMarshalingInfo
   {
      public override MarshalingInfoKind MarshalingInfoKind
      {
         get
         {
            return MarshalingInfoKind.FixedLengthString;
         }
      }

      /// <summary>
      /// Gets or sets the number of characters (not bytes) in a string.
      /// </summary>
      /// <value>The number of characters (not bytes) in a string.</value>
      public Int32 Size { get; set; }
   }

   public sealed class InterfaceMarshalingInfo : AbstractMarshalingInfo
   {
      public override MarshalingInfoKind MarshalingInfoKind
      {
         get
         {
            return MarshalingInfoKind.Interface;
         }
      }

      /// <summary>
      /// Gets or sets the zero-based parameter index of the unmanaged iid_is attribute used by COM.
      /// </summary>
      /// <value>The parameter index of the unmanaged iid_is attribute used by COM.</value>
      public Int32 IIDParameterIndex { get; set; }
   }

   public sealed class SafeArrayMarshalingInfo : AbstractMarshalingInfo
   {
      public override MarshalingInfoKind MarshalingInfoKind
      {
         get
         {
            return MarshalingInfoKind.SafeArray;
         }
      }

      /// <summary>
      /// Gets or sets the type for array elements.
      /// </summary>
      /// <value>The type for array elements.</value>
      public VarEnum ElementType { get; set; }

      /// <summary>
      /// Gets or sets the element type string.
      /// </summary>
      /// <value>The element type string.</value>
      public String UserDefinedType { get; set; }
   }

   public sealed class FixedLengthArrayMarshalingInfo : AbstractMarshalingInfo
   {
      public override MarshalingInfoKind MarshalingInfoKind
      {
         get
         {
            return MarshalingInfoKind.FixedLengthArray;
         }
      }

      /// <summary>
      /// Gets or sets the number of elements in an array.
      /// </summary>
      /// <value>The number of elements in an array.</value>
      public Int32 Size { get; set; }

      /// <summary>
      /// Gets or sets the type for array elements.
      /// </summary>
      /// <value>The type for array elements.</value>
      public UnmanagedType ElementType { get; set; }
   }

   public sealed class ArrayMarshalingInfo : AbstractMarshalingInfo
   {
      public override MarshalingInfoKind MarshalingInfoKind
      {
         get
         {
            return MarshalingInfoKind.Array;
         }
      }

      /// <summary>
      /// Gets or sets the type for array elements.
      /// </summary>
      /// <value>The type for array elements.</value>
      public UnmanagedType ElementType { get; set; }

      /// <summary>
      /// Gets the zero-based index of the parameter containing the count of the array elements.
      /// </summary>
      /// <value>The zero-based index of the parameter containing the count of the array elements.</value>
      public Int32 SizeParameterIndex { get; set; }

      /// <summary>
      /// Gets or sets the number of elements in an array.
      /// </summary>
      /// <value>The number of elements in an array.</value>
      public Int32 Size { get; set; }

      /// <summary>
      /// Gets or sets flags for this marshaling info.
      /// </summary>
      /// <value>The flags for this marshaling info.</value>
      public Int32 Flags { get; set; }
   }

   public sealed class CustomMarshalingInfo : AbstractMarshalingInfo
   {
      public override MarshalingInfoKind MarshalingInfoKind
      {
         get
         {
            return MarshalingInfoKind.Custom;
         }
      }

      /// <summary>
      /// Gets or sets the COM GUID string for this marshaling info.
      /// </summary>
      /// <value>The COM GUID string for this marshaling info.</value>
      public String GUIDString { get; set; }

      /// <summary>
      /// Gets or sets the native type name for this marshaling info.
      /// </summary>
      /// <value>The native type name for this marshaling info.</value>
      public String NativeTypeName { get; set; }

      /// <summary>
      /// Gets or sets the custom marshaler type name for this marshaling info.
      /// </summary>
      /// <value>The custom marshaler type name for this marshaling info.</value>
      public String CustomMarshalerTypeName { get; set; }

      /// <summary>
      /// Gets or sets the additional information for custom marshaler.
      /// </summary>
      /// <value>The additional information for custom marshaler.</value>
      public String MarshalCookie { get; set; }
   }

   public sealed class RawMarshalingInfo : AbstractMarshalingInfo
   {
      public override MarshalingInfoKind MarshalingInfoKind
      {
         get
         {
            return MarshalingInfoKind.Raw;
         }
      }

      /// <summary>
      /// Gets or sets the raw binary marshaling info, except for the starting byte (the value of <see cref="AbstractMarshalingInfo.Value"/> property).
      /// </summary>
      /// <value>The raw binary marshaling info.</value>
      public Byte[] Bytes { get; set; }
   }

   public abstract class AbstractCustomAttributeSignature
   {
      // Disable inheritance to other assemblies
      internal AbstractCustomAttributeSignature()
      {

      }

      public abstract CustomAttributeSignatureKind CustomAttributeSignatureKind { get; }
   }

   public enum CustomAttributeSignatureKind
   {
      Raw,
      Resolved
   }

   public sealed class RawCustomAttributeSignature : AbstractCustomAttributeSignature
   {
      public Byte[] Bytes { get; set; }
      public override CustomAttributeSignatureKind CustomAttributeSignatureKind
      {
         get
         {
            return CustomAttributeSignatureKind.Raw;
         }
      }
   }

   public sealed class CustomAttributeSignature : AbstractCustomAttributeSignature
   {
      private readonly List<CustomAttributeTypedArgument> _typedArgs;
      private readonly List<CustomAttributeNamedArgument> _namedArgs;

      public CustomAttributeSignature( Int32 typedArgsCount = 0, Int32 namedArgsCount = 0 )
      {
         this._typedArgs = new List<CustomAttributeTypedArgument>( typedArgsCount );
         this._namedArgs = new List<CustomAttributeNamedArgument>( namedArgsCount );
      }

      public override CustomAttributeSignatureKind CustomAttributeSignatureKind
      {
         get
         {
            return CustomAttributeSignatureKind.Resolved;
         }
      }

      public List<CustomAttributeTypedArgument> TypedArguments
      {
         get
         {
            return this._typedArgs;
         }
      }

      public List<CustomAttributeNamedArgument> NamedArguments
      {
         get
         {
            return this._namedArgs;
         }
      }
   }

   /// <summary>
   /// TODO: modification is easier if there is only one class for typed arguments, i.e. just use Value setter instead of creating new TypedArgument object and set value.
   /// </summary>
   public sealed class CustomAttributeTypedArgument
   {
      // Note: Enum values should be CustomAttributeValue_EnumReferences
      // Note: Type values should be CustomAttributeValue_TypeReferences
      // Note: Arrays should be CustomAttributeValue_Arrays
      public Object Value { get; set; }
   }

   public enum CustomAttributeTypedArgumentValueKind
   {
      Type,
      Enum,
      Array
   }

   public interface CustomAttributeTypedArgumentValueComplex
   {
      CustomAttributeTypedArgumentValueKind CustomAttributeTypedArgumentValueKind { get; }
   }

   public struct CustomAttributeValue_TypeReference : IEquatable<CustomAttributeValue_TypeReference>, CustomAttributeTypedArgumentValueComplex
   {
      private readonly String _typeString;

      public CustomAttributeValue_TypeReference( String typeString )
      {
         this._typeString = typeString;
      }

      public CustomAttributeTypedArgumentValueKind CustomAttributeTypedArgumentValueKind
      {
         get
         {
            return CustomAttributeTypedArgumentValueKind.Type;
         }
      }

      public String TypeString
      {
         get
         {
            return this._typeString;
         }
      }

      public override Boolean Equals( Object obj )
      {
         return obj is CustomAttributeValue_TypeReference && this.Equals( (CustomAttributeValue_TypeReference) obj );
      }

      public override Int32 GetHashCode()
      {
         return this._typeString.GetHashCodeSafe();
      }

      public Boolean Equals( CustomAttributeValue_TypeReference other )
      {
         return String.Equals( this._typeString, other._typeString );
      }


   }

   public struct CustomAttributeValue_EnumReference : IEquatable<CustomAttributeValue_EnumReference>, CustomAttributeTypedArgumentValueComplex
   {
      private readonly String _enumType;
      private readonly Object _enumValue;

      public CustomAttributeValue_EnumReference( String enumType, Object enumValue )
      {
         this._enumType = enumType;
         this._enumValue = enumValue;
      }

      public CustomAttributeTypedArgumentValueKind CustomAttributeTypedArgumentValueKind
      {
         get
         {
            return CustomAttributeTypedArgumentValueKind.Enum;
         }
      }

      public String EnumType
      {
         get
         {
            return this._enumType;
         }
      }

      public Object EnumValue
      {
         get
         {
            return this._enumValue;
         }
      }

      public override Boolean Equals( Object obj )
      {
         return obj is CustomAttributeValue_EnumReference && this.Equals( (CustomAttributeValue_EnumReference) obj );
      }

      public override Int32 GetHashCode()
      {
         return ( 17 * 23 + this._enumType.GetHashCodeSafe() ) * 23 + this._enumValue.GetHashCodeSafe();
      }

      public Boolean Equals( CustomAttributeValue_EnumReference other )
      {
         return String.Equals( this._enumType, other._enumType )
            && Equals( this._enumValue, other._enumValue );
      }
   }

   public struct CustomAttributeValue_Array : IEquatable<CustomAttributeValue_Array>, CustomAttributeTypedArgumentValueComplex
   {
      private readonly Array _array;
      private readonly CustomAttributeArgumentType _arrayElementType;

      public CustomAttributeValue_Array( Array array, CustomAttributeArgumentType arrayElementTypeString )
      {
         this._array = array;
         this._arrayElementType = arrayElementTypeString;
      }

      public CustomAttributeTypedArgumentValueKind CustomAttributeTypedArgumentValueKind
      {
         get
         {
            return CustomAttributeTypedArgumentValueKind.Array;
         }
      }

      public Array Array
      {
         get
         {
            return this._array;
         }
      }

      public CustomAttributeArgumentType ArrayElementType
      {
         get
         {
            return this._arrayElementType;
         }
      }

      public override Boolean Equals( Object obj )
      {
         return obj is CustomAttributeValue_Array && this.Equals( (CustomAttributeValue_Array) obj );
      }

      public override Int32 GetHashCode()
      {
         return ( 17 * 23 + this._arrayElementType.GetHashCodeSafe() ) * 23 + SequenceEqualityComparer<IEnumerable<Object>, Object>.SequenceHashCode( this.Array.Cast<Object>() );
      }

      public Boolean Equals( CustomAttributeValue_Array other )
      {
         return this._arrayElementType.EqualsTyped( other._arrayElementType )
            && SequenceEqualityComparer<IEnumerable<Object>, Object>.SequenceEquality( this._array.Cast<Object>(), other._array.Cast<Object>() );
      }
   }

   public sealed class CustomAttributeNamedArgument
   {
      public CustomAttributeTypedArgument Value { get; set; }
      public CustomAttributeArgumentType FieldOrPropertyType { get; set; }
      public String Name { get; set; }
      public Boolean IsField { get; set; }
   }

   public enum CustomAttributeArgumentTypeKind
   {
      Simple,
      TypeString,
      Array
   }

   public abstract class CustomAttributeArgumentType
   {
      // Disable inheritance to other assemblies
      internal CustomAttributeArgumentType()
      {

      }

      public abstract CustomAttributeArgumentTypeKind ArgumentTypeKind { get; }
   }

   public sealed class CustomAttributeArgumentTypeSimple : CustomAttributeArgumentType
   {
      public static readonly CustomAttributeArgumentTypeSimple Boolean = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.Boolean );
      public static readonly CustomAttributeArgumentTypeSimple Char = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.Char );
      public static readonly CustomAttributeArgumentTypeSimple SByte = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.I1 );
      public static readonly CustomAttributeArgumentTypeSimple Byte = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.U1 );
      public static readonly CustomAttributeArgumentTypeSimple Int16 = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.I2 );
      public static readonly CustomAttributeArgumentTypeSimple UInt16 = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.U2 );
      public static readonly CustomAttributeArgumentTypeSimple Int32 = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.I4 );
      public static readonly CustomAttributeArgumentTypeSimple UInt32 = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.U4 );
      public static readonly CustomAttributeArgumentTypeSimple Int64 = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.I8 );
      public static readonly CustomAttributeArgumentTypeSimple UInt64 = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.U8 );
      public static readonly CustomAttributeArgumentTypeSimple Single = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.R4 );
      public static readonly CustomAttributeArgumentTypeSimple Double = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.R8 );
      public static readonly CustomAttributeArgumentTypeSimple String = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.String );
      public static readonly CustomAttributeArgumentTypeSimple Type = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.Type );
      public static readonly CustomAttributeArgumentTypeSimple Object = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.Object );

      private CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind kind )
      {
         this.SimpleType = kind;
      }

      public override CustomAttributeArgumentTypeKind ArgumentTypeKind
      {
         get
         {
            return CustomAttributeArgumentTypeKind.Simple;
         }
      }

      public CustomAttributeArgumentTypeSimpleKind SimpleType { get; }

      public static CustomAttributeArgumentTypeSimple GetByKind( CustomAttributeArgumentTypeSimpleKind kind )
      {
         CustomAttributeArgumentTypeSimple retVal;
         if ( !TryGetByKind( kind, out retVal ) )
         {
            throw new ArgumentException( "Unrecognized CA argument simple type kind: " + kind + "." );
         }
         return retVal;
      }

      public static Boolean TryGetByKind( CustomAttributeArgumentTypeSimpleKind kind, out CustomAttributeArgumentTypeSimple caArgType )
      {
         switch ( kind )
         {
            case CustomAttributeArgumentTypeSimpleKind.Boolean:
               caArgType = Boolean;
               break;
            case CustomAttributeArgumentTypeSimpleKind.Char:
               caArgType = Char;
               break;
            case CustomAttributeArgumentTypeSimpleKind.I1:
               caArgType = SByte;
               break;
            case CustomAttributeArgumentTypeSimpleKind.U1:
               caArgType = Byte;
               break;
            case CustomAttributeArgumentTypeSimpleKind.I2:
               caArgType = Int16;
               break;
            case CustomAttributeArgumentTypeSimpleKind.U2:
               caArgType = UInt16;
               break;
            case CustomAttributeArgumentTypeSimpleKind.I4:
               caArgType = Int32;
               break;
            case CustomAttributeArgumentTypeSimpleKind.U4:
               caArgType = UInt32;
               break;
            case CustomAttributeArgumentTypeSimpleKind.I8:
               caArgType = Int64;
               break;
            case CustomAttributeArgumentTypeSimpleKind.U8:
               caArgType = UInt64;
               break;
            case CustomAttributeArgumentTypeSimpleKind.R4:
               caArgType = Single;
               break;
            case CustomAttributeArgumentTypeSimpleKind.R8:
               caArgType = Double;
               break;
            case CustomAttributeArgumentTypeSimpleKind.String:
               caArgType = String;
               break;
            case CustomAttributeArgumentTypeSimpleKind.Object:
               caArgType = Object;
               break;
            case CustomAttributeArgumentTypeSimpleKind.Type:
               caArgType = Type;
               break;
            default:
               caArgType = null;
               break;
         }

         return caArgType != null;
      }
   }

   /// <summary>
   /// This enumeration represents the kind of <see cref="CustomAttributeArgumentTypeSimple"/>.
   /// </summary>
   /// <remarks>
   /// The values of this enumeration are safe to be casted to <see cref="T:CILAssemblyManipulator.Physical.IO.SignatureElementTypes"/>.
   /// </remarks>
   public enum CustomAttributeArgumentTypeSimpleKind : byte
   {
      Boolean = 0x02, // Same as SignatureElementTypes.Boolean
      Char,
      I1,
      U1,
      I2,
      U2,
      I4,
      U4,
      I8,
      U8,
      R4,
      R8,
      String,
      Type = 0x50, // Same as SignatureElementTypes.Type
      Object // Same as SignatureElementTypes.CA_Boxed
   }

   public sealed class CustomAttributeArgumentTypeEnum : CustomAttributeArgumentType
   {
      public override CustomAttributeArgumentTypeKind ArgumentTypeKind
      {
         get
         {
            return CustomAttributeArgumentTypeKind.TypeString;
         }
      }

      public String TypeString { get; set; }
   }

   public sealed class CustomAttributeArgumentTypeArray : CustomAttributeArgumentType
   {

      public override CustomAttributeArgumentTypeKind ArgumentTypeKind
      {
         get
         {
            return CustomAttributeArgumentTypeKind.Array;
         }
      }

      public CustomAttributeArgumentType ArrayType { get; set; }
   }

   public abstract class AbstractSecurityInformation
   {


      // Disable inheritance to other assemblies
      internal AbstractSecurityInformation()
      {

      }

      /// <summary>
      /// Gets or sets the type of the security attribute.
      /// </summary>
      /// <value>The type of the security attribute.</value>
      public String SecurityAttributeType { get; set; }

      public abstract SecurityInformationKind SecurityInformationKind { get; }

   }

   public enum SecurityInformationKind
   {
      Resolved,
      Raw
   }

   public sealed class RawSecurityInformation : AbstractSecurityInformation
   {
      public Int32 ArgumentCount { get; set; }
      public Byte[] Bytes { get; set; }
      public override SecurityInformationKind SecurityInformationKind
      {
         get
         {
            return SecurityInformationKind.Raw;
         }
      }
   }

   /// <summary>
   /// This class represents a single security attribute declaration.
   /// Instances of this class are created via <see cref="CILElementWithSecurityInformation.AddDeclarativeSecurity(API.SecurityAction, CILType)"/> method.
   /// </summary>
   /// <seealso cref="CILElementWithSecurityInformation"/>
   /// <seealso cref="CILElementWithSecurityInformation.AddDeclarativeSecurity(API.SecurityAction, CILType)"/>
   public sealed class SecurityInformation : AbstractSecurityInformation
   {
      private readonly List<CustomAttributeNamedArgument> _namedArguments;

      public SecurityInformation( Int32 namedArgumentsCount = 0 )
      {
         this._namedArguments = new List<CustomAttributeNamedArgument>( namedArgumentsCount );
      }

      /// <summary>
      /// Gets the <see cref="CILCustomAttributeNamedArgument"/>s of this security attribute declaration.
      /// </summary>
      /// <value>The <see cref="CILCustomAttributeNamedArgument"/>s of this security attribute declaration.</value>
      public List<CustomAttributeNamedArgument> NamedArguments
      {
         get
         {
            return this._namedArguments;
         }
      }

      public override SecurityInformationKind SecurityInformationKind
      {
         get
         {
            return SecurityInformationKind.Resolved;
         }
      }
   }
}

public static partial class E_CILPhysical
{
   public static TSignature CreateDeepCopy<TSignature>( this TSignature sig, Func<TableIndex, TableIndex> tableIndexTranslator = null )
      where TSignature : AbstractSignature
   {
      switch ( sig.SignatureKind )
      {
         case SignatureKind.Field:
            return CloneFieldSignature( sig as FieldSignature, tableIndexTranslator ) as TSignature;
         case SignatureKind.GenericMethodInstantiation:
            return CloneGenericMethodSignature( sig as GenericMethodSignature, tableIndexTranslator ) as TSignature;
         case SignatureKind.LocalVariables:
            return CloneLocalsSignature( sig as LocalVariablesSignature, tableIndexTranslator ) as TSignature;
         case SignatureKind.MethodDefinition:
            return CloneMethodDefSignature( sig as MethodDefinitionSignature, tableIndexTranslator ) as TSignature;
         case SignatureKind.MethodReference:
            return CloneMethodRefSignature( sig as MethodReferenceSignature, tableIndexTranslator ) as TSignature;
         case SignatureKind.Type:
            return CloneTypeSignature( sig as TypeSignature, tableIndexTranslator ) as TSignature;
         case SignatureKind.RawSignature:
            return CloneRawSignature( sig as RawSignature, tableIndexTranslator ) as TSignature;
         case SignatureKind.Property:
            return ClonePropertySignature( sig as PropertySignature, tableIndexTranslator ) as TSignature;
         default:
            throw new NotSupportedException( "Invalid signature kind: " + sig.SignatureKind + "." );
      }
   }

   private static RawSignature CloneRawSignature( RawSignature sig, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var idx = 0;
      var bytes = sig.Bytes;
      return new RawSignature() { Bytes = bytes.CreateAndBlockCopyTo( ref idx, bytes.Length ) };
   }

   private static TypeSignature CloneTypeSignature( TypeSignature sig, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      TypeSignature retVal;
      switch ( sig.TypeSignatureKind )
      {
         case TypeSignatureKind.ClassOrValue:
            var clazz = (ClassOrValueTypeSignature) sig;
            var clazzClone = new ClassOrValueTypeSignature( clazz.GenericArguments.Count )
            {
               IsClass = clazz.IsClass,
               Type = tableIndexTranslator == null ? clazz.Type : tableIndexTranslator( clazz.Type )
            };
            clazzClone.GenericArguments.AddRange( clazz.GenericArguments.Select( gArg => CloneTypeSignature( gArg, tableIndexTranslator ) ) );
            retVal = clazzClone;
            break;
         case TypeSignatureKind.ComplexArray:
            var cArray = (ComplexArrayTypeSignature) sig;
            var cClone = new ComplexArrayTypeSignature( cArray.Sizes.Count, cArray.LowerBounds.Count )
            {
               Rank = cArray.Rank,
               ArrayType = CloneTypeSignature( cArray.ArrayType, tableIndexTranslator )
            };
            cClone.LowerBounds.AddRange( cArray.LowerBounds );
            cClone.Sizes.AddRange( cArray.Sizes );
            retVal = cClone;
            break;
         case TypeSignatureKind.FunctionPointer:
            retVal = new FunctionPointerTypeSignature()
            {
               MethodSignature = CloneMethodRefSignature( ( (FunctionPointerTypeSignature) sig ).MethodSignature, tableIndexTranslator )
            };
            break;
         case TypeSignatureKind.Pointer:
            var ptr = (PointerTypeSignature) sig;
            var ptrClone = new PointerTypeSignature( ptr.CustomModifiers.Count )
            {
               PointerType = CloneTypeSignature( ptr.PointerType, tableIndexTranslator )
            };
            ptrClone.CustomModifiers.AddRange( ptr.CustomModifiers.CloneCustomMods( tableIndexTranslator ) );
            retVal = ptrClone;
            break;
         case TypeSignatureKind.GenericParameter:
         case TypeSignatureKind.Simple:
            retVal = sig;
            break;
         case TypeSignatureKind.SimpleArray:
            var array = (SimpleArrayTypeSignature) sig;
            var clone = new SimpleArrayTypeSignature( array.CustomModifiers.Count )
            {
               ArrayType = CloneTypeSignature( array.ArrayType, tableIndexTranslator )
            };
            clone.CustomModifiers.AddRange( array.CustomModifiers.CloneCustomMods( tableIndexTranslator ) );
            retVal = clone;
            break;
         default:
            throw new NotSupportedException( "Invalid type signature kind: " + sig.TypeSignatureKind );
      }

      return retVal;
   }

   private static void PopulateAbstractMethodSignature( AbstractMethodSignature original, AbstractMethodSignature clone, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      clone.GenericArgumentCount = original.GenericArgumentCount;
      clone.MethodSignatureInformation = original.MethodSignatureInformation;
      clone.ReturnType = CloneParameterSignature( original.ReturnType, tableIndexTranslator );
      clone.Parameters.AddRange( original.Parameters.Select( p => CloneParameterSignature( p, tableIndexTranslator ) ) );
   }

   private static MethodReferenceSignature CloneMethodRefSignature( MethodReferenceSignature methodRef, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var retVal = new MethodReferenceSignature( methodRef.Parameters.Count, methodRef.VarArgsParameters.Count );
      PopulateAbstractMethodSignature( methodRef, retVal, tableIndexTranslator );
      retVal.VarArgsParameters.AddRange( methodRef.VarArgsParameters.Select( p => CloneParameterSignature( p, tableIndexTranslator ) ) );
      return retVal;
   }

   private static MethodDefinitionSignature CloneMethodDefSignature( MethodDefinitionSignature methodDef, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var retVal = new MethodDefinitionSignature( methodDef.Parameters.Count );
      PopulateAbstractMethodSignature( methodDef, retVal, tableIndexTranslator );
      return retVal;
   }

   private static ParameterSignature CloneParameterSignature( ParameterSignature paramSig, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var retVal = new ParameterSignature( paramSig.CustomModifiers.Count )
      {
         IsByRef = paramSig.IsByRef,
         Type = CloneTypeSignature( paramSig.Type, tableIndexTranslator )
      };
      retVal.CustomModifiers.AddRange( paramSig.CustomModifiers.CloneCustomMods( tableIndexTranslator ) );
      return retVal;
   }

   private static GenericMethodSignature CloneGenericMethodSignature( GenericMethodSignature gSig, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var retVal = new GenericMethodSignature( gSig.GenericArguments.Count );
      retVal.GenericArguments.AddRange( gSig.GenericArguments.Select( gArg => CloneTypeSignature( gArg, tableIndexTranslator ) ) );
      return retVal;
   }

   private static FieldSignature CloneFieldSignature( FieldSignature sig, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var retVal = new FieldSignature( sig.CustomModifiers.Count );
      retVal.Type = CloneTypeSignature( sig.Type, tableIndexTranslator );
      retVal.CustomModifiers.AddRange( sig.CustomModifiers.CloneCustomMods( tableIndexTranslator ) );
      return retVal;
   }

   private static LocalVariablesSignature CloneLocalsSignature( LocalVariablesSignature locals, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var retVal = new LocalVariablesSignature( locals.Locals.Count );
      retVal.Locals.AddRange( locals.Locals.Select( l => CloneLocalSignature( l, tableIndexTranslator ) ) );
      return retVal;
   }

   private static LocalVariableSignature CloneLocalSignature( LocalVariableSignature local, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var retVal = new LocalVariableSignature( local.CustomModifiers.Count )
      {
         IsByRef = local.IsByRef,
         IsPinned = local.IsPinned,
         Type = CloneTypeSignature( local.Type, tableIndexTranslator )
      };
      retVal.CustomModifiers.AddRange( local.CustomModifiers.CloneCustomMods( tableIndexTranslator ) );
      return retVal;
   }

   private static PropertySignature ClonePropertySignature( PropertySignature sig, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var retVal = new PropertySignature( sig.CustomModifiers.Count, sig.Parameters.Count )
      {
         HasThis = sig.HasThis,
         PropertyType = CloneTypeSignature( sig.PropertyType, tableIndexTranslator )
      };
      retVal.CustomModifiers.AddRange( sig.CustomModifiers.CloneCustomMods( tableIndexTranslator ) );
      retVal.Parameters.AddRange( sig.Parameters.Select( p => CloneParameterSignature( p, tableIndexTranslator ) ) );
      return retVal;
   }

   private static IEnumerable<CustomModifierSignature> CloneCustomMods( this List<CustomModifierSignature> original, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      foreach ( var cm in original )
      {
         yield return new CustomModifierSignature()
         {
            IsOptional = cm.IsOptional,
            CustomModifierType = tableIndexTranslator == null ? cm.CustomModifierType : tableIndexTranslator( cm.CustomModifierType )
         };
      }
   }


   public static Type GetNativeTypeForCAArrayType( this CustomAttributeArgumentType elemType )
   {
      switch ( elemType.ArgumentTypeKind )
      {
         case CustomAttributeArgumentTypeKind.Array:
            // Shouldn't be possible...
            return null;
         case CustomAttributeArgumentTypeKind.Simple:
            switch ( ( (CustomAttributeArgumentTypeSimple) elemType ).SimpleType )
            {
               case CustomAttributeArgumentTypeSimpleKind.Boolean:
                  return typeof( Boolean );
               case CustomAttributeArgumentTypeSimpleKind.Char:
                  return typeof( Char );
               case CustomAttributeArgumentTypeSimpleKind.I1:
                  return typeof( SByte );
               case CustomAttributeArgumentTypeSimpleKind.U1:
                  return typeof( Byte );
               case CustomAttributeArgumentTypeSimpleKind.I2:
                  return typeof( Int16 );
               case CustomAttributeArgumentTypeSimpleKind.U2:
                  return typeof( UInt16 );
               case CustomAttributeArgumentTypeSimpleKind.I4:
                  return typeof( Int32 );
               case CustomAttributeArgumentTypeSimpleKind.U4:
                  return typeof( UInt32 );
               case CustomAttributeArgumentTypeSimpleKind.I8:
                  return typeof( Int64 );
               case CustomAttributeArgumentTypeSimpleKind.U8:
                  return typeof( UInt64 );
               case CustomAttributeArgumentTypeSimpleKind.R4:
                  return typeof( Single );
               case CustomAttributeArgumentTypeSimpleKind.R8:
                  return typeof( Double );
               case CustomAttributeArgumentTypeSimpleKind.String:
                  return typeof( String );
               case CustomAttributeArgumentTypeSimpleKind.Type:
                  return typeof( CustomAttributeValue_TypeReference );
               case CustomAttributeArgumentTypeSimpleKind.Object:
                  return typeof( Object );
               default:
                  return null;
            }
         case CustomAttributeArgumentTypeKind.TypeString:
            return typeof( CustomAttributeValue_EnumReference );
         default:
            return null;
      }
   }

   //internal static Boolean TryReadSigElementType( this Byte[] array, ref Int32 idx, out SignatureElementTypes sig )
   //{
   //   var retVal = idx < array.Length;
   //   sig = retVal ? (SignatureElementTypes) array[idx++] : SignatureElementTypes.End;
   //   return retVal;
   //}

   //internal static Boolean TryReadSigStarter( this Byte[] array, ref Int32 idx, out SignatureStarters sig )
   //{
   //   var retVal = idx < array.Length;
   //   sig = retVal ? (SignatureStarters) array[idx++] : (SignatureStarters) 0;
   //   return retVal;
   //}

   //internal static Boolean TryReadUnmanagedType( this Byte[] array, ref Int32 idx, out UnmanagedType ut )
   //{
   //   var retVal = idx < array.Length;
   //   ut = retVal ? (UnmanagedType) array[idx++] : (UnmanagedType) 0;
   //   return retVal;
   //}

   //internal static Boolean TryReadByte( this Byte[] array, ref Int32 idx, out Byte b )
   //{
   //   var retVal = idx < array.Length;
   //   b = retVal ? array[idx++] : (Byte) 0;
   //   return retVal;
   //}

   public static AbstractMarshalingInfo CreateDeepCopy( this AbstractMarshalingInfo marshal )
   {
      AbstractMarshalingInfo retVal;
      if ( marshal == null )
      {
         retVal = null;
      }
      else
      {
         var mKind = marshal.MarshalingInfoKind;
         switch ( mKind )
         {
            case MarshalingInfoKind.Simple:
               retVal = new SimpleMarshalingInfo()
               {
                  Value = marshal.Value
               };
               break;
            case MarshalingInfoKind.FixedLengthString:
               retVal = new FixedLengthStringMarshalingInfo()
               {
                  Value = marshal.Value,
                  Size = ( (FixedLengthStringMarshalingInfo) marshal ).Size
               };
               break;
            case MarshalingInfoKind.FixedLengthArray:
               var flArray = (FixedLengthArrayMarshalingInfo) marshal;
               retVal = new FixedLengthArrayMarshalingInfo()
               {
                  Value = marshal.Value,
                  Size = flArray.Size,
                  ElementType = flArray.ElementType
               };
               break;
            case MarshalingInfoKind.SafeArray:
               var safeArray = (SafeArrayMarshalingInfo) marshal;
               retVal = new SafeArrayMarshalingInfo()
               {
                  Value = marshal.Value,
                  ElementType = safeArray.ElementType,
                  UserDefinedType = safeArray.UserDefinedType
               };
               break;
            case MarshalingInfoKind.Array:
               var array = (ArrayMarshalingInfo) marshal;
               retVal = new ArrayMarshalingInfo()
               {
                  Value = marshal.Value,
                  ElementType = array.ElementType,
                  SizeParameterIndex = array.SizeParameterIndex,
                  Size = array.Size,
                  Flags = array.Flags
               };
               break;
            case MarshalingInfoKind.Interface:
               retVal = new InterfaceMarshalingInfo()
               {
                  Value = marshal.Value,
                  IIDParameterIndex = ( (InterfaceMarshalingInfo) marshal ).IIDParameterIndex
               };
               break;
            case MarshalingInfoKind.Custom:
               var custom = (CustomMarshalingInfo) marshal;
               retVal = new CustomMarshalingInfo()
               {
                  Value = marshal.Value,
                  GUIDString = custom.GUIDString,
                  NativeTypeName = custom.NativeTypeName,
                  CustomMarshalerTypeName = custom.CustomMarshalerTypeName,
                  MarshalCookie = custom.MarshalCookie
               };
               break;
            case MarshalingInfoKind.Raw:
               retVal = new RawMarshalingInfo()
               {
                  Value = marshal.Value,
                  Bytes = ( (RawMarshalingInfo) marshal ).Bytes.CreateArrayCopy()
               };
               break;
            default:
               throw new InvalidOperationException( "Unrecognized marshal kind: " + mKind + "." );
         }
      }

      return retVal;
   }
}