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
namespace CILAssemblyManipulator.Implementation.Physical
{

   internal enum SignatureStarters : byte
   {
      Default = 0x00,
      C = 0x01,
      StandardCall = 0x02,
      ThisCall = 0x03,
      FastCall = 0x04,
      VarArgs = 0x05,
      Generic = 0x10,
      HasThis = 0x20,
      ExplicitThis = 0x40,
      Field = 0x06,
      Property = 0x08,
      LocalSignature = 0x07,
      MethodSpecGenericInst = 0x0A
   }

   internal enum SignatureElementTypes : byte
   {
      End = 0x00,
      Void = 0x01,
      Boolean = 0x02,
      Char = 0x03,
      I1 = 0x04,
      U1 = 0x05,
      I2 = 0x06,
      U2 = 0x07,
      I4 = 0x08,
      U4 = 0x09,
      I8 = 0x0A,
      U8 = 0x0B,
      R4 = 0x0C,
      R8 = 0x0D,
      String = 0x0E,
      Ptr = 0x0F,
      ByRef = 0x10,
      ValueType = 0x11,
      Class = 0x12,
      Var = 0x13,
      Array = 0x14,
      GenericInst = 0x15,
      TypedByRef = 0x16,
      I = 0x18,
      U = 0x19,
      FnPtr = 0x1B,
      Object = 0x1C,
      SzArray = 0x1D,
      MVar = 0x1E,
      CModReqd = 0x1F,
      CModOpt = 0x20,
      Internal = 0x21,
      Modifier = 0x40,
      Sentinel = 0x41,
      Pinned = 0x45,
      Type = 0x50,
      CA_Boxed = 0x51,
      Reserved = 0x52,
      CA_Field = 0x53,
      CA_Property = 0x54,
      CA_Enum = 0x55
   }

}