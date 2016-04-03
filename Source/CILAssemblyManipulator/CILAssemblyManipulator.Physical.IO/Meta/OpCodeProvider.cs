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
extern alias CAMPhysical;

using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.Meta
{
   // This interface will be merged with corresponding interface of CAM.Physical Core project.
#pragma warning disable 1591
   public interface OpCodeProvider
#pragma warning restore 1591
   {
      /// <summary>
      /// Tries to get <see cref="OpCodeSerializationInfo" /> for given <see cref="OpCodeID"/>.
      /// </summary>
      /// <param name="codeID">The <see cref="OpCodeID"/>.</param>
      /// <returns><see cref="OpCodeSerializationInfo"/> for given <paramref name="codeID"/>, or <c>null</c>.</returns>
      OpCodeSerializationInfo GetSerializationInfoOrNull( OpCodeID codeID );

      /// <summary>
      /// Writes the op code (but not the operand) to the given array at given index.
      /// </summary>
      /// <param name="info">The <see cref="OpCodeSerializationInfo"/> about the op code to write.</param>
      /// <param name="array">The byte array to write op code info to.</param>
      /// <param name="index">The index in <paramref name="array"/>.</param>
      void WriteOpCode( OpCodeSerializationInfo info, Byte[] array, Int32 index );

      /// <summary>
      /// Tries to read <see cref="OpCodeID"/> from serialized form in given byte array.
      /// </summary>
      /// <param name="array">The byte array read from.</param>
      /// <param name="index">The index in <paramref name="array"/>.</param>
      /// <param name="info">This parameter will have the <see cref="OpCodeSerializationInfo"/> of the op code serialized at given index in given byte array.</param>
      /// <returns><c>true</c> if this <see cref="OpCodeProvider"/> knew how to deserialize the op code; <c>false</c> otherwise.</returns>
      Boolean TryReadOpCode( Byte[] array, Int32 index, out OpCodeSerializationInfo info );
   }

   /// <summary>
   /// This class encapsulates required information about single <see cref="OpCodeID"/> for the <see cref="DefaultOpCodeProvider"/>.
   /// </summary>
   public class OpCodeSerializationInfo
   {

      /// <summary>
      /// Creates new instance of <see cref="OpCodeSerializationInfo"/> with given parameters.
      /// </summary>
      /// <param name="code">The <see cref="OpCode"/>.</param>
      /// <param name="codeSize">The size of the code, in bytes.</param>
      /// <param name="operandSize">The size of the operand, in bytes.</param>
      /// <param name="serializedValue">The serialized value of the code, as short.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="code"/> is <c>null</c>.</exception>
      public OpCodeSerializationInfo( OpCode code, Byte codeSize, Byte operandSize, Int16 serializedValue )
      {
         ArgumentValidator.ValidateNotNull( "Op code", code );

         this.Code = code;
         this.CodeSize = codeSize;
         this.FixedOperandSize = operandSize;
         this.SerializedValue = serializedValue;
      }

      /// <summary>
      /// Gets the related <see cref="OpCode"/>.
      /// </summary>
      /// <value>The related <see cref="OpCode"/>.</value>
      public OpCode Code { get; }

      /// <summary>
      /// Gets the byte size of the code, without operand.
      /// </summary>
      /// <value>The byte size of the code, without operand.</value>
      public Byte CodeSize { get; }

      /// <summary>
      /// Gets the fixed byte size of the operand, without the code.
      /// </summary>
      /// <value>The fixed byte size of the operand, without the code.</value>
      public Byte FixedOperandSize { get; }

      /// <summary>
      /// Gets the serialized value of the code, as short.
      /// </summary>
      /// <value>The serialized value of the code, as short.</value>
      public Int16 SerializedValue { get; }

      // TODO I am thinking of removing OpCode.OpCodeID property.
      // Would make comparing OpCodes (and caching their textual values) hard tho.
      ///// <summary>
      ///// Gets the <see cref="CAMPhysical::CILAssemblyManipulator.Physical.OpCodeID"/> for this <see cref="OpCodeProviderInfo"/>.
      ///// </summary>
      ///// <value>The <see cref="CAMPhysical::CILAssemblyManipulator.Physical.OpCodeID"/> for this <see cref="OpCodeProviderInfo"/>.</value>
      //public OpCodeID OpCodeID { get; }
   }

   /// <summary>
   /// This class provides default implementation for <see cref="OpCodeProvider"/>.
   /// It caches all the <see cref="OpCode"/>s based on their <see cref="OpCode.OpCodeID"/>, and also caches all <see cref="OpCodeInfoWithNoOperand"/>s based on their <see cref="OpCodeID"/>.
   /// </summary>
   public class DefaultOpCodeProvider : OpCodeProvider, CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider
   {
      /// <summary>
      /// Contains the maximum value for <see cref="OpCodeID"/> which would fit into one byte.
      /// </summary>
      /// <remarks>This value is <c>0xFE</c>.</remarks>
      private const Int32 MAX_ONE_BYTE_INSTRUCTION = 0xFE;


      /// <summary>
      /// Gets the default instance of <see cref="DefaultOpCodeProvider"/>.
      /// It has support for codes returned by <see cref="GetDefaultOpCodes"/>.
      /// </summary>
      public static CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider DefaultInstance { get; }

      static DefaultOpCodeProvider()
      {
         DefaultInstance = new DefaultOpCodeProvider();
      }

      private const Int32 INFOS_ARRAY_SIZE = Byte.MaxValue;

      private readonly OpCodeSerializationInfo[] _infosArray;
      private readonly Dictionary<OpCodeID, OpCodeSerializationInfo> _infosDictionary;

      private readonly OpCodeSerializationInfo[] _infosBySerializedValue_Byte1;
      private readonly OpCodeSerializationInfo[] _infosBySerializedValue_Byte2;
      private readonly IDictionary<OpCodeID, OpCodeInfoWithNoOperand> _operandless;

      /// <summary>
      /// Creates a new instance of <see cref="DefaultOpCodeProvider"/> with support for given op code set.
      /// </summary>
      /// <param name="codes">The <see cref="OpCodeSerializationInfo"/>s to support. If <c>null</c>, the return value of <see cref="GetDefaultOpCodes"/> will be used.</param>
      public DefaultOpCodeProvider( IEnumerable<OpCodeSerializationInfo> codes = null )
      {
         var tooBigCodes = new Dictionary<OpCodeID, OpCodeSerializationInfo>();
         this._infosArray = ( codes ?? GetDefaultOpCodes() ).ToArray_SelfIndexing(
            info => (Int32) info.Code.OpCodeID,
            CollectionOverwriteStrategy.Throw,
            arrayFactory: currentIndex => currentIndex > INFOS_ARRAY_SIZE ? null : new OpCodeSerializationInfo[Math.Max( currentIndex, INFOS_ARRAY_SIZE )], // The tertiary ensures that we never create array bigger than Byte.MaxValue. The Math.Max ensures that we create Byte.MaxValue sized array on first element, instead of resizing array each time.
            settingFailed: info => tooBigCodes.Add( info.Code.OpCodeID, info )
            );
         this._infosDictionary = tooBigCodes;

         var allInfos = this._infosArray.Where( info => info != null ).Concat( tooBigCodes.Values );
         this._infosBySerializedValue_Byte1 = new OpCodeSerializationInfo[Byte.MaxValue];
         allInfos.Where( info => info.CodeSize == 1 ).ToArray_SelfIndexing( info => info.SerializedValue, CollectionOverwriteStrategy.Throw, arrayFactory: len => this._infosBySerializedValue_Byte1 );
         this._infosBySerializedValue_Byte2 = new OpCodeSerializationInfo[Byte.MaxValue];
         allInfos.Where( info => info.CodeSize == 2 ).ToArray_SelfIndexing( info => info.SerializedValue & Byte.MaxValue, CollectionOverwriteStrategy.Throw, arrayFactory: len => this._infosBySerializedValue_Byte2 );
         this._operandless = allInfos
            .Where( info => info.Code.OperandType == OperandType.InlineNone )
            .ToDictionary_Overwrite( info => info.Code.OpCodeID, info => new OpCodeInfoWithNoOperand( info.Code.OpCodeID ) );
      }

      /// <inheritdoc />
      public OpCodeInfoWithNoOperand GetOperandlessInfoOrNull( OpCodeID codeID )
      {
         OpCodeInfoWithNoOperand retVal;
         return this._operandless.TryGetValue( codeID, out retVal ) ? retVal : null;
      }

      /// <inheritdoc />
      public OpCodeSerializationInfo GetSerializationInfoOrNull( OpCodeID codeID )
      {
         var idx = (Int32) codeID;
         if ( idx > INFOS_ARRAY_SIZE )
         {
            OpCodeSerializationInfo info;
            return this._infosDictionary.TryGetValue( codeID, out info ) ? info : null;
         }
         else
         {
            return this._infosArray[idx];
         }
      }

      /// <inheritdoc />
      public void WriteOpCode( OpCodeSerializationInfo info, Byte[] array, Int32 index )
      {
         if ( info.CodeSize == 1 )
         {
            array.WriteByteToBytes( ref index, (Byte) info.SerializedValue );
         }
         else
         {
            // N.B.! Big-endian! Everywhere else everything is little-endian.
            array.WriteUInt16BEToBytes( ref index, (UInt16) info.SerializedValue );
         }
      }

      /// <inheritdoc />
      public Boolean TryReadOpCode( Byte[] array, Int32 index, out OpCodeSerializationInfo info )
      {
         var startIdx = index;
         var b = array[index];
         info = b == MAX_ONE_BYTE_INSTRUCTION ?
            this._infosBySerializedValue_Byte2[array[index + 1]] :
            info = this._infosBySerializedValue_Byte1[b];
         return info != null;
      }


      /// <summary>
      /// Gets all of the op codes in <see cref="OpCodes"/> class.
      /// </summary>
      /// <returns>An enumerable to iterate all codes in <see cref="OpCodes"/> class.</returns>
      public static IEnumerable<OpCodeSerializationInfo> GetDefaultOpCodes()
      {
         yield return NewProviderInfo( OpCodes.Nop, 0x0000 );
         yield return NewProviderInfo( OpCodes.Break, 0x0001 );
         yield return NewProviderInfo( OpCodes.Ldarg_0, 0x0002 );
         yield return NewProviderInfo( OpCodes.Ldarg_1, 0x0003 );
         yield return NewProviderInfo( OpCodes.Ldarg_2, 0x0004 );
         yield return NewProviderInfo( OpCodes.Ldarg_3, 0x0005 );
         yield return NewProviderInfo( OpCodes.Ldloc_0, 0x0006 );
         yield return NewProviderInfo( OpCodes.Ldloc_1, 0x0007 );
         yield return NewProviderInfo( OpCodes.Ldloc_2, 0x0008 );
         yield return NewProviderInfo( OpCodes.Ldloc_3, 0x0009 );
         yield return NewProviderInfo( OpCodes.Stloc_0, 0x000A );
         yield return NewProviderInfo( OpCodes.Stloc_1, 0x000B );
         yield return NewProviderInfo( OpCodes.Stloc_2, 0x000C );
         yield return NewProviderInfo( OpCodes.Stloc_3, 0x000D );
         yield return NewProviderInfo( OpCodes.Ldarg_S, 0x000E );
         yield return NewProviderInfo( OpCodes.Ldarga_S, 0x000F );
         yield return NewProviderInfo( OpCodes.Starg_S, 0x0010 );
         yield return NewProviderInfo( OpCodes.Ldloc_S, 0x0011 );
         yield return NewProviderInfo( OpCodes.Ldloca_S, 0x0012 );
         yield return NewProviderInfo( OpCodes.Stloc_S, 0x0013 );
         yield return NewProviderInfo( OpCodes.Ldnull, 0x0014 );
         yield return NewProviderInfo( OpCodes.Ldc_I4_M1, 0x0015 );
         yield return NewProviderInfo( OpCodes.Ldc_I4_0, 0x0016 );
         yield return NewProviderInfo( OpCodes.Ldc_I4_1, 0x0017 );
         yield return NewProviderInfo( OpCodes.Ldc_I4_2, 0x0018 );
         yield return NewProviderInfo( OpCodes.Ldc_I4_3, 0x0019 );
         yield return NewProviderInfo( OpCodes.Ldc_I4_4, 0x001A );
         yield return NewProviderInfo( OpCodes.Ldc_I4_5, 0x001B );
         yield return NewProviderInfo( OpCodes.Ldc_I4_6, 0x001C );
         yield return NewProviderInfo( OpCodes.Ldc_I4_7, 0x001D );
         yield return NewProviderInfo( OpCodes.Ldc_I4_8, 0x001E );
         yield return NewProviderInfo( OpCodes.Ldc_I4_S, 0x001F );
         yield return NewProviderInfo( OpCodes.Ldc_I4, 0x0020 );
         yield return NewProviderInfo( OpCodes.Ldc_I8, 0x0021 );
         yield return NewProviderInfo( OpCodes.Ldc_R4, 0x0022 );
         yield return NewProviderInfo( OpCodes.Ldc_R8, 0x0023 );
         // 0x0024 is missing
         yield return NewProviderInfo( OpCodes.Dup, 0x0025 );
         yield return NewProviderInfo( OpCodes.Pop, 0x0026 );
         yield return NewProviderInfo( OpCodes.Jmp, 0x0027 );
         yield return NewProviderInfo( OpCodes.Call, 0x0028 );
         yield return NewProviderInfo( OpCodes.Calli, 0x0029 );
         yield return NewProviderInfo( OpCodes.Ret, 0x002A );
         yield return NewProviderInfo( OpCodes.Br_S, 0x002B );
         yield return NewProviderInfo( OpCodes.Brfalse_S, 0x002C );
         yield return NewProviderInfo( OpCodes.Brtrue_S, 0x002D );
         yield return NewProviderInfo( OpCodes.Beq_S, 0x002E );
         yield return NewProviderInfo( OpCodes.Bge_S, 0x002F );
         yield return NewProviderInfo( OpCodes.Bgt_S, 0x0030 );
         yield return NewProviderInfo( OpCodes.Ble_S, 0x0031 );
         yield return NewProviderInfo( OpCodes.Blt_S, 0x0032 );
         yield return NewProviderInfo( OpCodes.Bne_Un_S, 0x0033 );
         yield return NewProviderInfo( OpCodes.Bge_Un_S, 0x0034 );
         yield return NewProviderInfo( OpCodes.Bgt_Un_S, 0x0035 );
         yield return NewProviderInfo( OpCodes.Ble_Un_S, 0x0036 );
         yield return NewProviderInfo( OpCodes.Blt_Un_S, 0x0037 );
         yield return NewProviderInfo( OpCodes.Br, 0x0038 );
         yield return NewProviderInfo( OpCodes.Brfalse, 0x0039 );
         yield return NewProviderInfo( OpCodes.Brtrue, 0x003A );
         yield return NewProviderInfo( OpCodes.Beq, 0x003B );
         yield return NewProviderInfo( OpCodes.Bge, 0x003C );
         yield return NewProviderInfo( OpCodes.Bgt, 0x003D );
         yield return NewProviderInfo( OpCodes.Ble, 0x003E );
         yield return NewProviderInfo( OpCodes.Blt, 0x003F );
         yield return NewProviderInfo( OpCodes.Bne_Un, 0x0040 );
         yield return NewProviderInfo( OpCodes.Bge_Un, 0x0041 );
         yield return NewProviderInfo( OpCodes.Bgt_Un, 0x0042 );
         yield return NewProviderInfo( OpCodes.Ble_Un, 0x0043 );
         yield return NewProviderInfo( OpCodes.Blt_Un, 0x0044 );
         yield return NewProviderInfo( OpCodes.Switch, 0x0045 );
         yield return NewProviderInfo( OpCodes.Ldind_I1, 0x0046 );
         yield return NewProviderInfo( OpCodes.Ldind_U1, 0x0047 );
         yield return NewProviderInfo( OpCodes.Ldind_I2, 0x0048 );
         yield return NewProviderInfo( OpCodes.Ldind_U2, 0x0049 );
         yield return NewProviderInfo( OpCodes.Ldind_I4, 0x004A );
         yield return NewProviderInfo( OpCodes.Ldind_U4, 0x004B );
         yield return NewProviderInfo( OpCodes.Ldind_I8, 0x004C );
         yield return NewProviderInfo( OpCodes.Ldind_I, 0x004D );
         yield return NewProviderInfo( OpCodes.Ldind_R4, 0x004E );
         yield return NewProviderInfo( OpCodes.Ldind_R8, 0x004F );
         yield return NewProviderInfo( OpCodes.Ldind_Ref, 0x0050 );
         yield return NewProviderInfo( OpCodes.Stind_Ref, 0x0051 );
         yield return NewProviderInfo( OpCodes.Stind_I1, 0x0052 );
         yield return NewProviderInfo( OpCodes.Stind_I2, 0x0053 );
         yield return NewProviderInfo( OpCodes.Stind_I4, 0x0054 );
         yield return NewProviderInfo( OpCodes.Stind_I8, 0x0055 );
         yield return NewProviderInfo( OpCodes.Stind_R4, 0x0056 );
         yield return NewProviderInfo( OpCodes.Stind_R8, 0x0057 );
         yield return NewProviderInfo( OpCodes.Add, 0x0058 );
         yield return NewProviderInfo( OpCodes.Sub, 0x0059 );
         yield return NewProviderInfo( OpCodes.Mul, 0x005A );
         yield return NewProviderInfo( OpCodes.Div, 0x005B );
         yield return NewProviderInfo( OpCodes.Div_Un, 0x005C );
         yield return NewProviderInfo( OpCodes.Rem, 0x005D );
         yield return NewProviderInfo( OpCodes.Rem_Un, 0x005E );
         yield return NewProviderInfo( OpCodes.And, 0x005F );
         yield return NewProviderInfo( OpCodes.Or, 0x0060 );
         yield return NewProviderInfo( OpCodes.Xor, 0x0061 );
         yield return NewProviderInfo( OpCodes.Shl, 0x0062 );
         yield return NewProviderInfo( OpCodes.Shr, 0x0063 );
         yield return NewProviderInfo( OpCodes.Shr_Un, 0x0064 );
         yield return NewProviderInfo( OpCodes.Neg, 0x0065 );
         yield return NewProviderInfo( OpCodes.Not, 0x0066 );
         yield return NewProviderInfo( OpCodes.Conv_I1, 0x0067 );
         yield return NewProviderInfo( OpCodes.Conv_I2, 0x0068 );
         yield return NewProviderInfo( OpCodes.Conv_I4, 0x0069 );
         yield return NewProviderInfo( OpCodes.Conv_I8, 0x006A );
         yield return NewProviderInfo( OpCodes.Conv_R4, 0x006B );
         yield return NewProviderInfo( OpCodes.Conv_R8, 0x006C );
         yield return NewProviderInfo( OpCodes.Conv_U4, 0x006D );
         yield return NewProviderInfo( OpCodes.Conv_U8, 0x006E );
         yield return NewProviderInfo( OpCodes.Callvirt, 0x006F );
         yield return NewProviderInfo( OpCodes.Cpobj, 0x0070 );
         yield return NewProviderInfo( OpCodes.Ldobj, 0x0071 );
         yield return NewProviderInfo( OpCodes.Ldstr, 0x0072 );
         yield return NewProviderInfo( OpCodes.Newobj, 0x0073 );
         yield return NewProviderInfo( OpCodes.Castclass, 0x0074 );
         yield return NewProviderInfo( OpCodes.Isinst, 0x0075 );
         yield return NewProviderInfo( OpCodes.Conv_R_Un, 0x0076 );
         // 0x0077, and 0x0078 are missing
         yield return NewProviderInfo( OpCodes.Unbox, 0x0079 );
         yield return NewProviderInfo( OpCodes.Throw, 0x007A );
         yield return NewProviderInfo( OpCodes.Ldfld, 0x007B );
         yield return NewProviderInfo( OpCodes.Ldflda, 0x007C );
         yield return NewProviderInfo( OpCodes.Stfld, 0x007D );
         yield return NewProviderInfo( OpCodes.Ldsfld, 0x007E );
         yield return NewProviderInfo( OpCodes.Ldsflda, 0x007F );
         yield return NewProviderInfo( OpCodes.Stsfld, 0x0080 );
         yield return NewProviderInfo( OpCodes.Stobj, 0x0081 );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_I1_Un, 0x0082 );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_I2_Un, 0x0083 );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_I4_Un, 0x0084 );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_I8_Un, 0x0085 );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_U1_Un, 0x0086 );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_U2_Un, 0x0087 );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_U4_Un, 0x0088 );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_U8_Un, 0x0089 );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_I_Un, 0x008A );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_U_Un, 0x008B );
         yield return NewProviderInfo( OpCodes.Box, 0x008C );
         yield return NewProviderInfo( OpCodes.Newarr, 0x008D );
         yield return NewProviderInfo( OpCodes.Ldlen, 0x008E );
         yield return NewProviderInfo( OpCodes.Ldelema, 0x008F );
         yield return NewProviderInfo( OpCodes.Ldelem_I1, 0x0090 );
         yield return NewProviderInfo( OpCodes.Ldelem_U1, 0x0091 );
         yield return NewProviderInfo( OpCodes.Ldelem_I2, 0x0092 );
         yield return NewProviderInfo( OpCodes.Ldelem_U2, 0x0093 );
         yield return NewProviderInfo( OpCodes.Ldelem_I4, 0x0094 );
         yield return NewProviderInfo( OpCodes.Ldelem_U4, 0x0095 );
         yield return NewProviderInfo( OpCodes.Ldelem_I8, 0x0096 );
         yield return NewProviderInfo( OpCodes.Ldelem_I, 0x0097 );
         yield return NewProviderInfo( OpCodes.Ldelem_R4, 0x0098 );
         yield return NewProviderInfo( OpCodes.Ldelem_R8, 0x0099 );
         yield return NewProviderInfo( OpCodes.Ldelem_Ref, 0x009A );
         yield return NewProviderInfo( OpCodes.Stelem_I, 0x009B );
         yield return NewProviderInfo( OpCodes.Stelem_I1, 0x009C );
         yield return NewProviderInfo( OpCodes.Stelem_I2, 0x009D );
         yield return NewProviderInfo( OpCodes.Stelem_I4, 0x009E );
         yield return NewProviderInfo( OpCodes.Stelem_I8, 0x009F );
         yield return NewProviderInfo( OpCodes.Stelem_R4, 0x00A0 );
         yield return NewProviderInfo( OpCodes.Stelem_R8, 0x00A1 );
         yield return NewProviderInfo( OpCodes.Stelem_Ref, 0x00A2 );
         yield return NewProviderInfo( OpCodes.Ldelem, 0x00A3 );
         yield return NewProviderInfo( OpCodes.Stelem, 0x00A4 );
         yield return NewProviderInfo( OpCodes.Unbox_Any, 0x00A5 );
         // 0x00A6-0x00B2 are missing
         yield return NewProviderInfo( OpCodes.Conv_Ovf_I1, 0x00B3 );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_U1, 0x00B4 );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_I2, 0x00B5 );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_U2, 0x00B6 );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_I4, 0x00B7 );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_U4, 0x00B8 );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_I8, 0x00B9 );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_U8, 0x00BA );
         // 0x00BB-0x00C1 are missing
         yield return NewProviderInfo( OpCodes.Refanyval, 0x00C2 );
         yield return NewProviderInfo( OpCodes.Ckfinite, 0x00C3 );
         // 0x00C4 and 0x00C5 are missing
         yield return NewProviderInfo( OpCodes.Mkrefany, 0x00C6 );
         // 0x00C7-0x00CF are missing
         yield return NewProviderInfo( OpCodes.Ldtoken, 0x00D0 );
         yield return NewProviderInfo( OpCodes.Conv_U2, 0x00D1 );
         yield return NewProviderInfo( OpCodes.Conv_U1, 0x00D2 );
         yield return NewProviderInfo( OpCodes.Conv_I, 0x00D3 );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_I, 0x00D4 );
         yield return NewProviderInfo( OpCodes.Conv_Ovf_U, 0x00D5 );
         yield return NewProviderInfo( OpCodes.Add_Ovf, 0x00D6 );
         yield return NewProviderInfo( OpCodes.Add_Ovf_Un, 0x00D7 );
         yield return NewProviderInfo( OpCodes.Mul_Ovf, 0x00D8 );
         yield return NewProviderInfo( OpCodes.Mul_Ovf_Un, 0x00D9 );
         yield return NewProviderInfo( OpCodes.Sub_Ovf, 0x00DA );
         yield return NewProviderInfo( OpCodes.Sub_Ovf_Un, 0x00DB );
         yield return NewProviderInfo( OpCodes.Endfinally, 0x00DC );
         yield return NewProviderInfo( OpCodes.Leave, 0x00DD );
         yield return NewProviderInfo( OpCodes.Leave_S, 0x00DE );
         yield return NewProviderInfo( OpCodes.Stind_I, 0x00DF );
         yield return NewProviderInfo( OpCodes.Conv_U, 0x00E0 );
         // 0x00E1-0x00FD are missing
         yield return NewProviderInfo( OpCodes.Arglist, 0xFE00 );
         yield return NewProviderInfo( OpCodes.Ceq, 0xFE01 );
         yield return NewProviderInfo( OpCodes.Cgt, 0xFE02 );
         yield return NewProviderInfo( OpCodes.Cgt_Un, 0xFE03 );
         yield return NewProviderInfo( OpCodes.Clt, 0xFE04 );
         yield return NewProviderInfo( OpCodes.Clt_Un, 0xFE05 );
         yield return NewProviderInfo( OpCodes.Ldftn, 0xFE06 );
         yield return NewProviderInfo( OpCodes.Ldvirtftn, 0xFE07 );
         // 0xFE08 is missing
         yield return NewProviderInfo( OpCodes.Ldarg, 0xFE09 );
         yield return NewProviderInfo( OpCodes.Ldarga, 0xFE0A );
         yield return NewProviderInfo( OpCodes.Starg, 0xFE0B );
         yield return NewProviderInfo( OpCodes.Ldloc, 0xFE0C );
         yield return NewProviderInfo( OpCodes.Ldloca, 0xFE0D );
         yield return NewProviderInfo( OpCodes.Stloc, 0xFE0E );
         yield return NewProviderInfo( OpCodes.Localloc, 0xFE0F );
         // 0xFE10 is missing
         yield return NewProviderInfo( OpCodes.Endfilter, 0xFE11 );
         yield return NewProviderInfo( OpCodes.Unaligned_, 0xFE12 );
         yield return NewProviderInfo( OpCodes.Volatile_, 0xFE13 );
         yield return NewProviderInfo( OpCodes.Tail_, 0xFE14 );
         yield return NewProviderInfo( OpCodes.Initobj, 0xFE15 );
         yield return NewProviderInfo( OpCodes.Constrained_, 0xFE16 );
         yield return NewProviderInfo( OpCodes.Cpblk, 0xFE17 );
         yield return NewProviderInfo( OpCodes.Initblk, 0xFE18 );
         yield return NewProviderInfo( OpCodes.No_, 0xFE19 );
         yield return NewProviderInfo( OpCodes.Rethrow, 0xFE1A );
         // 0xFE1B is missing
         yield return NewProviderInfo( OpCodes.Sizeof, 0xFE1C );
         yield return NewProviderInfo( OpCodes.Refanytype, 0xFE1D );
         yield return NewProviderInfo( OpCodes.Readonly_, 0xFE1E );
      }

      /// <summary>
      /// Creates new instance of <see cref="OpCodeSerializationInfo"/> with given parameters, calculating size automatically.
      /// </summary>
      /// <param name="code">The <see cref="OpCode"/>.</param>
      /// <param name="serializedValue">The serialized value of the code, as short.</param>
      /// <exception cref="ArgumentException">If <see cref="OpCode.OperandType"/> of <paramref name="code"/> is not one of the values in <see cref="OperandType"/> enumeration.</exception>
      protected static OpCodeSerializationInfo NewProviderInfo( OpCode code, Int32 serializedValue )
      {
         Byte operandSize;
         switch ( code.OperandType )
         {
            case OperandType.InlineNone:
               operandSize = 0;
               break;
            case OperandType.ShortInlineBrTarget:
            case OperandType.ShortInlineI:
            case OperandType.ShortInlineVar:
               operandSize = 1;
               break;
            case OperandType.InlineVar:
               operandSize = 2;
               break;
            case OperandType.InlineBrTarget:
            case OperandType.InlineField:
            case OperandType.InlineI:
            case OperandType.InlineMethod:
            case OperandType.InlineSignature:
            case OperandType.InlineString:
            case OperandType.InlineSwitch:
            case OperandType.InlineToken:
            case OperandType.InlineType:
            case OperandType.ShortInlineR:
               operandSize = 4;
               break;
            case OperandType.InlineI8:
            case OperandType.InlineR:
               operandSize = 8;
               break;
            default:
               throw new ArgumentException( "Unrecognized operand type: " + code.OperandType + "." );
         }
         return new OpCodeSerializationInfo( code, (Byte) ( ( (UInt32) serializedValue ) > MAX_ONE_BYTE_INSTRUCTION ? 2 : 1 ), operandSize, (Int16) serializedValue );
      }
   }
}

#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{
   /// <summary>
   /// Gets the code for given <see cref="OpCodeID"/>, or throws an exception if no code found.
   /// </summary>
   /// <param name="opCodeProvider">The <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeProvider"/>.</param>
   /// <param name="codeID">The <see cref="OpCodeID"/></param>
   /// <returns>The <see cref="OpCode"/> for given <paramref name="codeID"/></returns>
   /// <exception cref="NullReferenceException">If this <paramref name="opCodeProvider"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If no suitable <see cref="OpCode"/> is found.</exception>
   /// <seealso cref="OpCode"/>
   public static OpCode GetCodeFor( this CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider opCodeProvider, OpCodeID codeID )
   {
      return opCodeProvider.GetInfoFor( codeID ).Code;
   }

   /// <summary>
   /// Gets the <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo"/> for given <see cref="OpCodeID"/>, or throws an exception if no info found.
   /// </summary>
   /// <param name="opCodeProvider">The <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeProvider"/>.</param>
   /// <param name="codeID">The <see cref="OpCodeID"/></param>
   /// <returns>The <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo"/> for given <paramref name="codeID"/></returns>
   /// <exception cref="NullReferenceException">If this <paramref name="opCodeProvider"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If no suitable <see cref="OpCode"/> is found.</exception>
   public static CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo GetInfoFor( this CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider opCodeProvider, OpCodeID codeID )
   {
      var info = ( (CILAssemblyManipulator.Physical.Meta.OpCodeProvider) opCodeProvider ).GetSerializationInfoOrNull( codeID );
      if ( info == null )
      {
         throw new ArgumentException( "Op code " + codeID + " is invalid or not supported by this op code provider." );
      }
      return info;
   }

   /// <summary>
   /// Calculates the total fixed byte count for a specific <see cref="OpCodeID"/>.
   /// This is the sum of <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo.CodeSize"/> and <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo.FixedOperandSize"/>.
   /// </summary>
   /// <param name="opCodeProvider">The <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeProvider"/>.</param>
   /// <param name="codeID">The <see cref="OpCodeID"/>.</param>
   /// <returns>The total fixed byte count for a specific <see cref="OpCode"/>.</returns>
   /// <exception cref="NullReferenceException">If </exception>
   /// <remarks>
   /// One should use <see cref="GetTotalByteCount"/> extension method when calculating byte sizes when writing or reading IL bytecode.
   /// This is because switch instruction (<see cref="OpCodeID.Switch"/>) has additional offset array, the length of which is determined by the fixed operand of switch instruction.
   /// </remarks>
   public static Int32 GetFixedByteCount( this CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider opCodeProvider, OpCodeID codeID )
   {
      return opCodeProvider.GetInfoFor( codeID ).GetFixedByteCount();
   }

   /// <summary>
   /// Helper method to calculate the fixed byte count for specific <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo"/>
   /// </summary>
   /// <param name="info">The <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo"/>.</param>
   /// <returns>The sum of <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo.CodeSize"/> and <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo.FixedOperandSize"/>.</returns>
   public static Int32 GetFixedByteCount( this CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo info )
   {
      return info.CodeSize + info.FixedOperandSize;
   }

   /// <summary>
   /// Gets the total byte count that a single <see cref="OpCodeInfo"/> takes.
   /// </summary>
   /// <param name="opCodeProvider">The <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeProvider"/> to use.</param>
   /// <param name="info">The single <see cref="OpCodeInfo"/>.</param>
   /// <returns>The total byte count of a single <see cref="OpCodeInfo"/>.</returns>
   /// <remarks>
   /// The total byte count is the size of op code of <see cref="OpCodeInfo"/> added with <see cref="OpCodeInfo.DynamicOperandByteSize"/>.
   /// </remarks>
   /// <exception cref="NullReferenceException">If <paramref name="opCodeProvider"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="info"/> is <c>null</c>.</exception>
   public static Int32 GetTotalByteCount( this CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider opCodeProvider, OpCodeInfo info )
   {
      // Get NullReferenceException *before* ArgumentNullException
      return ArgumentValidator.ValidateNotNullReference( opCodeProvider ).GetFixedByteCount( info.OpCodeID ) + info.DynamicOperandByteSize;
   }

   /// <summary>
   /// Calculates the total byte count for IL of given op codes.
   /// </summary>
   /// <param name="ocp">The <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeProvider" />.</param>
   /// <param name="opCodes">The op codes.</param>
   /// <returns>The total byte count for IL of given op codes.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="ocp"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="opCodes"/> is <c>null</c>.</exception>
   public static Int32 GetILByteCount( this CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider ocp, IEnumerable<OpCodeInfo> opCodes )
   {
      ArgumentValidator.ValidateNotNullReference( ocp );

      return ArgumentValidator.ValidateNotNull( "Op codes", opCodes ).Sum( oci => ocp.GetTotalByteCount( oci ) );
   }
}