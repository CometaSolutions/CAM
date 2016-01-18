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
using CILAssemblyManipulator.Physical.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO
{
   /// <summary>
   /// This corresponds to <c>ELEMENT_TYPE_*</c> values of ECMA-335 standard (II.23.1.16), with some extra values.
   /// They are used in various signature BLOBs.
   /// </summary>
   public enum SignatureElementTypes : byte
   {
      /// <summary>
      /// Signifies the end of signature. Not used.
      /// </summary>
      End = 0x00,

      /// <summary>
      /// A <see cref="Void"/> type.
      /// </summary>
      Void = 0x01,

      /// <summary>
      /// A <see cref="Boolean"/> type.
      /// </summary>
      Boolean = 0x02,

      /// <summary>
      /// A <see cref="Char"/> type.
      /// </summary>
      Char = 0x03,

      /// <summary>
      /// A <see cref="SByte"/> type.
      /// </summary>
      I1 = 0x04,

      /// <summary>
      /// A <see cref="Byte"/> type.
      /// </summary>
      U1 = 0x05,

      /// <summary>
      /// A <see cref="Int16"/> type.
      /// </summary>
      I2 = 0x06,

      /// <summary>
      /// A <see cref="UInt16"/> type.
      /// </summary>
      U2 = 0x07,

      /// <summary>
      /// A <see cref="Int32"/> type.
      /// </summary>
      I4 = 0x08,

      /// <summary>
      /// A <see cref="UInt32"/> type.
      /// </summary>
      U4 = 0x09,

      /// <summary>
      /// A <see cref="Int64"/> type.
      /// </summary>
      I8 = 0x0A,

      /// <summary>
      /// A <see cref="UInt64"/> type.
      /// </summary>
      U8 = 0x0B,

      /// <summary>
      /// A <see cref="Single"/> type.
      /// </summary>
      R4 = 0x0C,

      /// <summary>
      /// A <see cref="Double"/> type.
      /// </summary>
      R8 = 0x0D,

      /// <summary>
      /// A <see cref="String"/> type.
      /// </summary>
      String = 0x0E,

      /// <summary>
      /// Prefix for pointer types.
      /// </summary>
      Ptr = 0x0F,

      /// <summary>
      /// Prefix for by-ref types.
      /// </summary>
      ByRef = 0x10,

      /// <summary>
      /// Prefix for value type (C# 'struct').
      /// </summary>
      ValueType = 0x11,

      /// <summary>
      /// Prefix for class type (C# 'class' or 'interface').
      /// </summary>
      Class = 0x12,

      /// <summary>
      /// Type generic parameter reference.
      /// </summary>
      /// <seealso cref="MVar"/>
      Var = 0x13,

      /// <summary>
      /// Prefix for complex array types, <see cref="ComplexArrayTypeSignature"/>.
      /// </summary>
      Array = 0x14,

      /// <summary>
      /// Indicates that the type is generic type instantiation.
      /// </summary>
      GenericInst = 0x15,

      /// <summary>
      /// Indicates that type is <see cref="T:System.TypedReference"/>.
      /// </summary>
      TypedByRef = 0x16,

      /// <summary>
      /// Indicates that type is <see cref="IntPtr"/>.
      /// </summary>
      I = 0x18,

      /// <summary>
      /// Indicates that type is <see cref="UIntPtr"/>.
      /// </summary>
      U = 0x19,

      /// <summary>
      /// Indicates that type is <see cref="FunctionPointerTypeSignature"/>.
      /// </summary>
      FnPtr = 0x1B,

      /// <summary>
      /// Indicates that type is <see cref="Object"/>.
      /// </summary>
      Object = 0x1C,

      /// <summary>
      /// Prefix for simple vector array types, <see cref="SimpleArrayTypeSignature"/>.
      /// </summary>
      SzArray = 0x1D,

      /// <summary>
      /// Method generic parameter reference.
      /// </summary>
      /// <seealso cref="Var"/>
      MVar = 0x1E,

      /// <summary>
      /// Indicates that <see cref="CustomModifierSignature"/> is required.
      /// </summary>
      CModReqd = 0x1F,

      /// <summary>
      /// Indicates that <see cref="CustomModifierSignature"/> is optional.
      /// </summary>
      CModOpt = 0x20,

      /// <summary>
      /// Reserved, for internal use of CLI.
      /// </summary>
      Internal = 0x21,

      /// <summary>
      /// Reserved, for internal use of CLI.
      /// </summary>
      Module = 0x3F,

      /// <summary>
      /// Reserved, for internal use of CLI.
      /// </summary>
      Modifier = 0x40,

      /// <summary>
      /// This is sentinel mark to separate which parameters belong to <see cref="AbstractMethodSignature.Parameters"/> and which ones belong to <see cref="MethodReferenceSignature.VarArgsParameters"/>.
      /// </summary>
      Sentinel = 0x41,

      /// <summary>
      /// Indicates that local variable type is pinned.
      /// </summary>
      Pinned = 0x45,

      /// <summary>
      /// Indicates <see cref="Type"/> type.
      /// </summary>
      Type = 0x50,

      /// <summary>
      /// Indicates that custom attribute value is boxed.
      /// </summary>
      CA_Boxed = 0x51,

      /// <summary>
      /// This value is reserved.
      /// </summary>
      Reserved = 0x52,

      /// <summary>
      /// Indicates that the custom attribute named argument is field (<see cref="CustomAttributeNamedArgument.IsField"/> will be <c>true</c>).
      /// </summary>
      CA_Field = 0x53,

      /// <summary>
      /// Indicates that the custom attribute named argument is property (<see cref="CustomAttributeNamedArgument.IsField"/> will be <c>false</c>).
      /// </summary>
      CA_Property = 0x54,

      /// <summary>
      /// Indicates that <see cref="CustomAttributeNamedArgument.FieldOrPropertyType"/> is <see cref="CustomAttributeArgumentTypeEnum"/>.
      /// </summary>
      CA_Enum = 0x55
   }

   internal enum SignatureStarters : byte
   {
      /// <summary>
      /// This value indicates that method uses standard (managed) calling conventions.
      /// </summary>
      Default = 0x00,

      /// <summary>
      /// This value indicates that method uses unmanaged calling convention in C.
      /// </summary>
      C = 0x01,

      /// <summary>
      /// This value indicates that method uses unmanaged standard calling convention in C++.
      /// </summary>
      StandardCall = 0x02,

      /// <summary>
      /// This value this indicates that method uses unmanaged calling convention passing this-pointer in C++.
      /// </summary>
      ThisCall = 0x03,

      /// <summary>
      /// This value this indicates that method uses unmanaged special optimized calling convention in C++.
      /// </summary>
      FastCall = 0x04,

      /// <summary>
      /// This value this indicates that method uses standard (managed) calling conventions for vararg-method.
      /// </summary>
      VarArgs = 0x05,

      /// <summary>
      /// When present as signature's first byte, this value indicates that the signature is <see cref="FieldSignature"/>.
      /// </summary>
      Field = 0x06,

      /// <summary>
      /// When present as signature's first byte, this value indicates that the signature is <see cref="LocalVariablesSignature"/>.
      /// </summary>
      LocalSignature = 0x07,

      /// <summary>
      /// When present as signature's first byte, this value indicates that the signature is <see cref="PropertySignature"/>.
      /// </summary>
      Property = 0x08,

      /// <summary>
      /// This value indicates that method uses undefined unmanaged calling conventions.
      /// </summary>
      /// <remarks>The meaning of this value is currently not clear.</remarks>
      Unmanaged = 0x09,

      /// <summary>
      /// When present as signature's first byte, this value indicates that the signature is <see cref="GenericMethodSignature"/>.
      /// </summary>
      MethodSpecGenericInst = 0x0A,

      /// <summary>
      /// This value indicates that method uses unmanaged calling conventions for a native vararg-method.
      /// </summary>
      NativeVarArgs = 0x0B,

      /// <summary>
      /// This is mask which holds values relevant for calling conventions of a method (the lower 4 bits).
      /// </summary>
      CallingConventionsMask = 0x0F,

      /// <summary>
      /// This value this indicates that the method is generic.
      /// </summary>
      Generic = 0x10,

      /// <summary>
      /// This value this indicates that the method needs hidden 'this' parameter.
      /// </summary>
      HasThis = 0x20,

      /// <summary>
      /// This value this indicates that the method needs explicit 'this' parameter.
      /// </summary>
      ExplicitThis = 0x40,

      /// <summary>
      /// This value is currently reserved.
      /// </summary>
      Reserved = 0x80,
   }
}

public static partial class E_CILPhysical
{
   /// <summary>
   /// Checks whether the given <see cref="SignatureStarters"/> represents a <see cref="FieldSignature"/>.
   /// </summary>
   /// <param name="starter">The <see cref="SignatureStarters"/> element.</param>
   /// <returns><c>true</c> if <paramref name="starter"/> represents a <see cref="FieldSignature"/>; <c>false</c> otherwise.</returns>
   internal static Boolean IsField( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.CallingConventionsMask ) == SignatureStarters.Field;
   }

   /// <summary>
   /// Checks whether the given <see cref="SignatureStarters"/> represents a <see cref="LocalVariablesSignature"/>.
   /// </summary>
   /// <param name="starter">The <see cref="SignatureStarters"/> element.</param>
   /// <returns><c>true</c> if <paramref name="starter"/> represents a <see cref="LocalVariablesSignature"/>; <c>false</c> otherwise.</returns>
   internal static Boolean IsLocalSignature( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.CallingConventionsMask ) == SignatureStarters.LocalSignature;
   }

   /// <summary>
   /// Checks whether the given <see cref="SignatureStarters"/> represents a <see cref="PropertySignature"/>.
   /// </summary>
   /// <param name="starter">The <see cref="SignatureStarters"/> element.</param>
   /// <returns><c>true</c> if <paramref name="starter"/> represents a <see cref="PropertySignature"/>; <c>false</c> otherwise.</returns>
   internal static Boolean IsProperty( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.CallingConventionsMask ) == SignatureStarters.Property;
   }

   /// <summary>
   /// Checks whether the given <see cref="SignatureStarters"/> represents a <see cref="GenericMethodSignature"/>.
   /// </summary>
   /// <param name="starter">The <see cref="SignatureStarters"/> element.</param>
   /// <returns><c>true</c> if <paramref name="starter"/> represents a <see cref="GenericMethodSignature"/>; <c>false</c> otherwise.</returns>
   internal static Boolean IsMethodSpecGenericInst( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.CallingConventionsMask ) == SignatureStarters.MethodSpecGenericInst;
   }

   internal static Boolean IsGeneric( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.Generic ) != 0;
   }

   internal static Boolean IsHasThis( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.HasThis ) != 0;
   }
}
