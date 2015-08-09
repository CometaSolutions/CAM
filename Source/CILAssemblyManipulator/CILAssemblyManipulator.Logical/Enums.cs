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
using CILAssemblyManipulator.Logical;
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Logical
{

   /// <summary>
   /// This enumeration provides values for the calling convention of methods. See ECMA specification part with method signatures for more information.
   /// </summary>
   /// <seealso cref="System.Reflection.CallingConventions"/>
   [Flags]
   public enum CallingConventions
   {
      /// <summary>
      /// The calling convention is determinated by the runtime. This should be used for static methods. For instance or virtual methods the <see cref="CallingConventions.HasThis"/> should be used.
      /// </summary>
      Standard = 0x01,
      /// <summary>
      /// The calling convention to use for methods with variable arguments.
      /// </summary>
      VarArgs = 0x02,
      /// <summary>
      /// Either <see cref="CallingConventions.Standard"/> or <see cref="CallingConventions.VarArgs"/> may be used.
      /// </summary>
      Any = 0x03,
      /// <summary>
      /// The calling convention to use for instance or virtual methods. The signature stored in metadata will then omit the first 'this' parameter.
      /// </summary>
      HasThis = 0x20,
      /// <summary>
      /// The calling convention to use for function-pointer signatures. The signature stored in metadata will contain the first 'this' parameter, as the type is thus unknown. When using <see cref="CallingConventions.ExplicitThis"/>, the <see cref="CallingConventions.HasThis"/> must be set too.
      /// </summary>
      ExplicitThis = 0x40
   }

   /// <summary>
   /// This is <see cref="TypeCode"/> enum without <see cref="F:System.TypeCode.DBNull"/> and with some extra other values.
   /// </summary>
   /// <remarks>
   /// Instances of <see cref="TypeCode"/> are directly castable to this enum.
   /// </remarks>
   public enum CILTypeCode
   {
      /// <summary>
      /// Not returned directly by any object but useful in some scenarios.
      /// </summary>
      Empty,
      /// <summary>
      /// A general type representing any reference or value type not explicitly represented by another TypeCode.
      /// </summary>
      Object,
      /// <summary>
      /// A <see cref="System.Boolean"/> type.
      /// </summary>
      Boolean = 3,
      /// <summary>
      /// A <see cref="System.Char"/> type.
      /// </summary>
      Char,
      /// <summary>
      /// A <see cref="System.SByte"/> type.
      /// </summary>
      SByte,
      /// <summary>
      /// A <see cref="System.Byte"/> type.
      /// </summary>
      Byte,
      /// <summary>
      /// A <see cref="System.Int16"/> type.
      /// </summary>
      Int16,
      /// <summary>
      /// A <see cref="System.UInt16"/> type.
      /// </summary>
      UInt16,
      /// <summary>
      /// A <see cref="System.Int32"/> type.
      /// </summary>
      Int32,
      /// <summary>
      /// A <see cref="System.UInt32"/> type.
      /// </summary>
      UInt32,
      /// <summary>
      /// A <see cref="System.Int64"/> type.
      /// </summary>
      Int64,
      /// <summary>
      /// A <see cref="System.UInt64"/> type.
      /// </summary>
      UInt64,
      /// <summary>
      /// A <see cref="System.Single"/> type.
      /// </summary>
      Single,
      /// <summary>
      /// A <see cref="System.Double"/> type.
      /// </summary>
      Double,
      /// <summary>
      /// A <see cref="System.Decimal"/> type.
      /// </summary>
      Decimal,
      /// <summary>
      /// A <see cref="System.DateTime"/> type.
      /// </summary>
      DateTime,
      /// <summary>
      /// A <see cref="System.String"/> type.
      /// </summary>
      String = 18,
      /// <summary>
      /// A <see cref="System.Void"/> type.
      /// </summary>
      Void = System.UInt16.MaxValue,
      /// <summary>
      /// A <see cref="T:System.TypedByReference"/> type.
      /// </summary>
      TypedByRef,
      /// <summary>
      /// A <see cref="System.ValueType"/> type.
      /// </summary>
      Value,
      /// <summary>
      /// A <see cref="System.Enum"/> type.
      /// </summary>
      Enum,
      /// <summary>
      /// A <see cref="System.IntPtr"/> type.
      /// </summary>
      IntPtr,
      /// <summary>
      /// A <see cref="System.UIntPtr"/> type.
      /// </summary>
      UIntPtr,
      /// <summary>
      /// A <see cref="System.Object"/> type.
      /// Note that the <see cref="CILTypeCode"/> of <see cref="System.Object"/> type will be this value, and not <see cref="CILTypeCode.Object"/>.
      /// </summary>
      SystemObject,
      /// <summary>
      /// A <see cref="System.Type"/> type.
      /// Note that the <see cref="CILTypeCode"/> of <see cref="System.Type"/> will be this value, and not <see cref="CILTypeCode.Object"/>.
      /// </summary>
      Type,
   }


}

public static partial class E_CILLogical
{
   /// <summary>
   /// Checks whether calling conventions represents a method with standard calling conventions.
   /// </summary>
   /// <param name="conv">The <see cref="CallingConventions"/>.</param>
   /// <returns><c>true</c> if <paramref name="conv"/> represents a method with standard calling conventions; <c>false</c> otherwise.</returns>
   /// <seealso cref="CallingConventions.Standard"/>
   public static Boolean IsStandard( this CallingConventions conv )
   {
      return ( conv & CallingConventions.Standard ) != 0;
   }

   /// <summary>
   /// Checks whether calling conventions represents a method with variadic parameters.
   /// </summary>
   /// <param name="conv">The <see cref="CallingConventions"/>.</param>
   /// <returns><c>true</c> if <paramref name="conv"/> represents a method with variadic parameters; <c>false</c> otherwise.</returns>
   /// <seealso cref="CallingConventions.VarArgs"/>
   public static Boolean IsVarArgs( this CallingConventions conv )
   {
      return ( conv & CallingConventions.VarArgs ) != 0;
   }

   /// <summary>
   /// Checks whether calling conventions represents a method with implicit <c>this</c> first parameter.
   /// </summary>
   /// <param name="conv">The <see cref="CallingConventions"/>.</param>
   /// <returns><c>true</c> if <paramref name="conv"/> represents a method with implicit <c>this</c> first parameter; <c>false</c> otherwise.</returns>
   /// <seealso cref="CallingConventions.HasThis"/>
   public static Boolean IsThis( this CallingConventions conv )
   {
      return ( conv & CallingConventions.HasThis ) != 0;
   }

   /// <summary>
   /// Checks whether calling conventions represents a method with explicit <c>this</c> first parameter.
   /// </summary>
   /// <param name="conv">The <see cref="CallingConventions"/>.</param>
   /// <returns><c>true</c> if <paramref name="conv"/> represents a method with explicit <c>this</c> first parameter; <c>false</c> otherwise.</returns>
   /// <seealso cref="CallingConventions.ExplicitThis"/>
   public static Boolean IsExplicitThis( this CallingConventions conv )
   {
      return ( conv & CallingConventions.ExplicitThis ) != 0;
   }

#if !CAM_LOGICAL_IS_SL

   /// <summary>
   /// Using values from <see cref="System.Runtime.InteropServices.DllImportAttribute"/>, creates a corresponding <see cref="PInvokeAttributes"/>.
   /// </summary>
   /// <param name="attribute">The <see cref="System.Runtime.InteropServices.DllImportAttribute"/> to create <see cref="PInvokeAttributes"/> from.</param>
   /// <returns>A <see cref="PInvokeAttributes"/> corresponding to <paramref name="attribute"/>.</returns>
   public static PInvokeAttributes GetCorrespondingPInvokeAttributes( this System.Runtime.InteropServices.DllImportAttribute attribute )
   {
      var result = (PInvokeAttributes) 0;
      if ( attribute.BestFitMapping )
      {
         result |= PInvokeAttributes.BestFitMapping;
      }
      switch ( attribute.CallingConvention )
      {
         case System.Runtime.InteropServices.CallingConvention.Cdecl:
            result |= PInvokeAttributes.CallConvCDecl;
            break;
         case System.Runtime.InteropServices.CallingConvention.StdCall:
            result |= PInvokeAttributes.CallConvStdcall;
            break;
         case System.Runtime.InteropServices.CallingConvention.ThisCall:
            result |= PInvokeAttributes.CallConvThiscall;
            break;
         case System.Runtime.InteropServices.CallingConvention.Winapi:
            result |= PInvokeAttributes.CallConvPlatformapi;
            break;
      }
      switch ( attribute.CharSet )
      {
         case System.Runtime.InteropServices.CharSet.Ansi:
            result |= PInvokeAttributes.CharsetAnsi;
            break;
         case System.Runtime.InteropServices.CharSet.Unicode:
            result |= PInvokeAttributes.CharsetUnicode;
            break;
         default:
            result |= PInvokeAttributes.CharsetAuto;
            break;
      }
      if ( attribute.ExactSpelling )
      {
         result |= PInvokeAttributes.NoMangle;
      }
      if ( attribute.SetLastError )
      {
         result |= PInvokeAttributes.SupportsLastError;
      }
      if ( attribute.ThrowOnUnmappableChar )
      {
         result |= PInvokeAttributes.ThrowOnUnmappableChar;
      }
      // TODO preserve sig?
      return result;
   }
#endif

}
