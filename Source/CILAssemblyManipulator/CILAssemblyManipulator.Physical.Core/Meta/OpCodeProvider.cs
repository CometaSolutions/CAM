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
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.Meta
{
   public interface OpCodeProvider
   {
      Boolean TryGetCodeFor( OpCodeID codeEnum, out OpCode opCode );

      OpCodeInfoWithNoOperand GetOperandlessInfoOrNull( OpCodeID codeEnum );

   }

   public class DefaultOpCodeProvider : OpCodeProvider
   {
      private static readonly OpCodeProvider _Instance = new DefaultOpCodeProvider();

      public static OpCodeProvider Instance
      {
         get
         {
            return _Instance;
         }
      }

      private readonly IDictionary<OpCodeID, OpCode> _codes;
      private readonly IDictionary<OpCodeID, OpCodeInfoWithNoOperand> _operandless;

      public DefaultOpCodeProvider( IEnumerable<OpCode> codes = null )
      {
         this._codes = ( codes ?? GetDefaultOpCodes() ).ToDictionary_Overwrite( c => c.OpCodeID, c => c );
         this._operandless = this._codes
            .Values
            .Where( c => c.OperandType == OperandType.InlineNone )
            .ToDictionary_Overwrite( c => c.OpCodeID, c => new OpCodeInfoWithNoOperand( c.OpCodeID ) );
      }

      public Boolean TryGetCodeFor( OpCodeID codeEnum, out OpCode opCode )
      {
         return this._codes.TryGetValue( codeEnum, out opCode );
      }

      public OpCodeInfoWithNoOperand GetOperandlessInfoOrNull( OpCodeID codeEnum )
      {
         OpCodeInfoWithNoOperand retVal;
         return this._operandless.TryGetValue( codeEnum, out retVal ) ? retVal : null;
      }

      public static IEnumerable<OpCode> GetDefaultOpCodes()
      {
         yield return OpCodes.Nop;
         yield return OpCodes.Break;
         yield return OpCodes.Ldarg_0;
         yield return OpCodes.Ldarg_1;
         yield return OpCodes.Ldarg_2;
         yield return OpCodes.Ldarg_3;
         yield return OpCodes.Ldloc_0;
         yield return OpCodes.Ldloc_1;
         yield return OpCodes.Ldloc_2;
         yield return OpCodes.Ldloc_3;
         yield return OpCodes.Stloc_0;
         yield return OpCodes.Stloc_1;
         yield return OpCodes.Stloc_2;
         yield return OpCodes.Stloc_3;
         yield return OpCodes.Ldarg_S;
         yield return OpCodes.Ldarga_S;
         yield return OpCodes.Starg_S;
         yield return OpCodes.Ldloc_S;
         yield return OpCodes.Ldloca_S;
         yield return OpCodes.Stloc_S;
         yield return OpCodes.Ldnull;
         yield return OpCodes.Ldc_I4_M1;
         yield return OpCodes.Ldc_I4_0;
         yield return OpCodes.Ldc_I4_1;
         yield return OpCodes.Ldc_I4_2;
         yield return OpCodes.Ldc_I4_3;
         yield return OpCodes.Ldc_I4_4;
         yield return OpCodes.Ldc_I4_5;
         yield return OpCodes.Ldc_I4_6;
         yield return OpCodes.Ldc_I4_7;
         yield return OpCodes.Ldc_I4_8;
         yield return OpCodes.Ldc_I4_S;
         yield return OpCodes.Ldc_I4;
         yield return OpCodes.Ldc_I8;
         yield return OpCodes.Ldc_R4;
         yield return OpCodes.Ldc_R8;
         yield return OpCodes.Dup;
         yield return OpCodes.Pop;
         yield return OpCodes.Jmp;
         yield return OpCodes.Call;
         yield return OpCodes.Calli;
         yield return OpCodes.Ret;
         yield return OpCodes.Br_S;
         yield return OpCodes.Brfalse_S;
         yield return OpCodes.Brtrue_S;
         yield return OpCodes.Beq_S;
         yield return OpCodes.Bge_S;
         yield return OpCodes.Bgt_S;
         yield return OpCodes.Ble_S;
         yield return OpCodes.Blt_S;
         yield return OpCodes.Bne_Un_S;
         yield return OpCodes.Bge_Un_S;
         yield return OpCodes.Bgt_Un_S;
         yield return OpCodes.Ble_Un_S;
         yield return OpCodes.Blt_Un_S;
         yield return OpCodes.Br;
         yield return OpCodes.Brfalse;
         yield return OpCodes.Brtrue;
         yield return OpCodes.Beq;
         yield return OpCodes.Bge;
         yield return OpCodes.Bgt;
         yield return OpCodes.Ble;
         yield return OpCodes.Blt;
         yield return OpCodes.Bne_Un;
         yield return OpCodes.Bge_Un;
         yield return OpCodes.Bgt_Un;
         yield return OpCodes.Ble_Un;
         yield return OpCodes.Blt_Un;
         yield return OpCodes.Switch;
         yield return OpCodes.Ldind_I1;
         yield return OpCodes.Ldind_U1;
         yield return OpCodes.Ldind_I2;
         yield return OpCodes.Ldind_U2;
         yield return OpCodes.Ldind_I4;
         yield return OpCodes.Ldind_U4;
         yield return OpCodes.Ldind_I8;
         yield return OpCodes.Ldind_I;
         yield return OpCodes.Ldind_R4;
         yield return OpCodes.Ldind_R8;
         yield return OpCodes.Ldind_Ref;
         yield return OpCodes.Stind_Ref;
         yield return OpCodes.Stind_I1;
         yield return OpCodes.Stind_I2;
         yield return OpCodes.Stind_I4;
         yield return OpCodes.Stind_I8;
         yield return OpCodes.Stind_R4;
         yield return OpCodes.Stind_R8;
         yield return OpCodes.Add;
         yield return OpCodes.Sub;
         yield return OpCodes.Mul;
         yield return OpCodes.Div;
         yield return OpCodes.Div_Un;
         yield return OpCodes.Rem;
         yield return OpCodes.Rem_Un;
         yield return OpCodes.And;
         yield return OpCodes.Or;
         yield return OpCodes.Xor;
         yield return OpCodes.Shl;
         yield return OpCodes.Shr;
         yield return OpCodes.Shr_Un;
         yield return OpCodes.Neg;
         yield return OpCodes.Not;
         yield return OpCodes.Conv_I1;
         yield return OpCodes.Conv_I2;
         yield return OpCodes.Conv_I4;
         yield return OpCodes.Conv_I8;
         yield return OpCodes.Conv_R4;
         yield return OpCodes.Conv_R8;
         yield return OpCodes.Conv_U4;
         yield return OpCodes.Conv_U8;
         yield return OpCodes.Callvirt;
         yield return OpCodes.Cpobj;
         yield return OpCodes.Ldobj;
         yield return OpCodes.Ldstr;
         yield return OpCodes.Newobj;
         yield return OpCodes.Castclass;
         yield return OpCodes.Isinst;
         yield return OpCodes.Conv_R_Un;
         yield return OpCodes.Unbox;
         yield return OpCodes.Throw;
         yield return OpCodes.Ldfld;
         yield return OpCodes.Ldflda;
         yield return OpCodes.Stfld;
         yield return OpCodes.Ldsfld;
         yield return OpCodes.Ldsflda;
         yield return OpCodes.Stsfld;
         yield return OpCodes.Stobj;
         yield return OpCodes.Conv_Ovf_I1_Un;
         yield return OpCodes.Conv_Ovf_I2_Un;
         yield return OpCodes.Conv_Ovf_I4_Un;
         yield return OpCodes.Conv_Ovf_I8_Un;
         yield return OpCodes.Conv_Ovf_U1_Un;
         yield return OpCodes.Conv_Ovf_U2_Un;
         yield return OpCodes.Conv_Ovf_U4_Un;
         yield return OpCodes.Conv_Ovf_U8_Un;
         yield return OpCodes.Conv_Ovf_I_Un;
         yield return OpCodes.Conv_Ovf_U_Un;
         yield return OpCodes.Box;
         yield return OpCodes.Newarr;
         yield return OpCodes.Ldlen;
         yield return OpCodes.Ldelema;
         yield return OpCodes.Ldelem_I1;
         yield return OpCodes.Ldelem_U1;
         yield return OpCodes.Ldelem_I2;
         yield return OpCodes.Ldelem_U2;
         yield return OpCodes.Ldelem_I4;
         yield return OpCodes.Ldelem_U4;
         yield return OpCodes.Ldelem_I8;
         yield return OpCodes.Ldelem_I;
         yield return OpCodes.Ldelem_R4;
         yield return OpCodes.Ldelem_R8;
         yield return OpCodes.Ldelem_Ref;
         yield return OpCodes.Stelem_I;
         yield return OpCodes.Stelem_I1;
         yield return OpCodes.Stelem_I2;
         yield return OpCodes.Stelem_I4;
         yield return OpCodes.Stelem_I8;
         yield return OpCodes.Stelem_R4;
         yield return OpCodes.Stelem_R8;
         yield return OpCodes.Stelem_Ref;
         yield return OpCodes.Ldelem;
         yield return OpCodes.Stelem;
         yield return OpCodes.Unbox_Any;
         yield return OpCodes.Conv_Ovf_I1;
         yield return OpCodes.Conv_Ovf_U1;
         yield return OpCodes.Conv_Ovf_I2;
         yield return OpCodes.Conv_Ovf_U2;
         yield return OpCodes.Conv_Ovf_I4;
         yield return OpCodes.Conv_Ovf_U4;
         yield return OpCodes.Conv_Ovf_I8;
         yield return OpCodes.Conv_Ovf_U8;
         yield return OpCodes.Refanyval;
         yield return OpCodes.Ckfinite;
         yield return OpCodes.Mkrefany;
         yield return OpCodes.Ldtoken;
         yield return OpCodes.Conv_U2;
         yield return OpCodes.Conv_U1;
         yield return OpCodes.Conv_I;
         yield return OpCodes.Conv_Ovf_I;
         yield return OpCodes.Conv_Ovf_U;
         yield return OpCodes.Add_Ovf;
         yield return OpCodes.Add_Ovf_Un;
         yield return OpCodes.Mul_Ovf;
         yield return OpCodes.Mul_Ovf_Un;
         yield return OpCodes.Sub_Ovf;
         yield return OpCodes.Sub_Ovf_Un;
         yield return OpCodes.Endfinally;
         yield return OpCodes.Leave;
         yield return OpCodes.Leave_S;
         yield return OpCodes.Stind_I;
         yield return OpCodes.Conv_U;
         yield return OpCodes.Arglist;
         yield return OpCodes.Ceq;
         yield return OpCodes.Cgt;
         yield return OpCodes.Cgt_Un;
         yield return OpCodes.Clt;
         yield return OpCodes.Clt_Un;
         yield return OpCodes.Ldftn;
         yield return OpCodes.Ldvirtftn;
         yield return OpCodes.Ldarg;
         yield return OpCodes.Ldarga;
         yield return OpCodes.Starg;
         yield return OpCodes.Ldloc;
         yield return OpCodes.Ldloca;
         yield return OpCodes.Stloc;
         yield return OpCodes.Localloc;
         yield return OpCodes.Endfilter;
         yield return OpCodes.Unaligned_;
         yield return OpCodes.Volatile_;
         yield return OpCodes.Tail_;
         yield return OpCodes.Initobj;
         yield return OpCodes.Constrained_;
         yield return OpCodes.Cpblk;
         yield return OpCodes.Initblk;
         yield return OpCodes.Rethrow;
         yield return OpCodes.Sizeof;
         yield return OpCodes.Refanytype;
         yield return OpCodes.Readonly_;
      }
   }
}

public static partial class E_CILPhysical
{
   public static OpCode GetCodeFor( this OpCodeProvider opCodeProvider, OpCodeID codeID )
   {
      OpCode retVal;
      if ( !opCodeProvider.TryGetCodeFor( codeID, out retVal ) )
      {
         throw new ArgumentException( "Op code " + codeID + " is invalid or not supported by this op code provider." );
      }
      return retVal;
   }

   public static OpCodeInfoWithNoOperand GetOperandlessInfoFor( this OpCodeProvider opCodeProvider, OpCodeID codeID )
   {
      var retVal = opCodeProvider.GetOperandlessInfoOrNull( codeID );
      if ( retVal == null )
      {
         throw new ArgumentException( "Op code " + codeID + " is not operandless opcode." );
      }
      return retVal;
   }
}