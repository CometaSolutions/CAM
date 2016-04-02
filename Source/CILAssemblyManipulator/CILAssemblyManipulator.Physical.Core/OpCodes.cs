using CILAssemblyManipulator.Physical;
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

namespace CILAssemblyManipulator.Physical
{
   /// <summary>
   /// Container for instances of <see cref="OpCode"/>s.
   /// Mimics the <see cref="T:System.Reflection.Emit.OpCodes"/> class.
   /// </summary>
   public static class OpCodes
   {
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Nop"/>
      /// </summary>
      public static readonly OpCode Nop;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Break"/>
      /// </summary>
      public static readonly OpCode Break;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldarg_0"/>
      /// </summary>
      public static readonly OpCode Ldarg_0;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldarg_1"/>
      /// </summary>
      public static readonly OpCode Ldarg_1;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldarg_2"/>
      /// </summary>
      public static readonly OpCode Ldarg_2;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldarg_3"/>
      /// </summary>
      public static readonly OpCode Ldarg_3;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldloc_0"/>
      /// </summary>
      public static readonly OpCode Ldloc_0;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldloc_1"/>
      /// </summary>
      public static readonly OpCode Ldloc_1;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldloc_2"/>
      /// </summary>
      public static readonly OpCode Ldloc_2;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldloc_3"/>
      /// </summary>
      public static readonly OpCode Ldloc_3;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stloc_0"/>
      /// </summary>
      public static readonly OpCode Stloc_0;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stloc_1"/>
      /// </summary>
      public static readonly OpCode Stloc_1;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stloc_2"/>
      /// </summary>
      public static readonly OpCode Stloc_2;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stloc_3"/>
      /// </summary>
      public static readonly OpCode Stloc_3;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldarg_S"/>
      /// </summary>
      public static readonly OpCode Ldarg_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldarga_S"/>
      /// </summary>
      public static readonly OpCode Ldarga_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Starg_S"/>
      /// </summary>
      public static readonly OpCode Starg_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldloc_S"/>
      /// </summary>
      public static readonly OpCode Ldloc_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldloca_S"/>
      /// </summary>
      public static readonly OpCode Ldloca_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stloc_S"/>
      /// </summary>
      public static readonly OpCode Stloc_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldnull"/>
      /// </summary>
      public static readonly OpCode Ldnull;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldc_I4_M1"/>
      /// </summary>
      public static readonly OpCode Ldc_I4_M1;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldc_I4_0"/>
      /// </summary>
      public static readonly OpCode Ldc_I4_0;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldc_I4_1"/>
      /// </summary>
      public static readonly OpCode Ldc_I4_1;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldc_I4_2"/>
      /// </summary>
      public static readonly OpCode Ldc_I4_2;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldc_I4_3"/>
      /// </summary>
      public static readonly OpCode Ldc_I4_3;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldc_I4_4"/>
      /// </summary>
      public static readonly OpCode Ldc_I4_4;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldc_I4_5"/>
      /// </summary>
      public static readonly OpCode Ldc_I4_5;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldc_I4_6"/>
      /// </summary>
      public static readonly OpCode Ldc_I4_6;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldc_I4_7"/>
      /// </summary>
      public static readonly OpCode Ldc_I4_7;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldc_I4_8"/>
      /// </summary>
      public static readonly OpCode Ldc_I4_8;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldc_I4_S"/>
      /// </summary>
      public static readonly OpCode Ldc_I4_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldc_I4"/>
      /// </summary>
      public static readonly OpCode Ldc_I4;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldc_I8"/>
      /// </summary>
      public static readonly OpCode Ldc_I8;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldc_R4"/>
      /// </summary>
      public static readonly OpCode Ldc_R4;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldc_R8"/>
      /// </summary>
      public static readonly OpCode Ldc_R8;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Dup"/>
      /// </summary>
      public static readonly OpCode Dup;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Pop"/>
      /// </summary>
      public static readonly OpCode Pop;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Jmp"/>
      /// </summary>
      public static readonly OpCode Jmp;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Call"/>
      /// </summary>
      public static readonly OpCode Call;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Calli"/>
      /// </summary>
      public static readonly OpCode Calli;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ret"/>
      /// </summary>
      public static readonly OpCode Ret;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Br_S"/>
      /// </summary>
      public static readonly OpCode Br_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Brfalse_S"/>
      /// </summary>
      public static readonly OpCode Brfalse_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Brtrue_S"/>
      /// </summary>
      public static readonly OpCode Brtrue_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Beq_S"/>
      /// </summary>
      public static readonly OpCode Beq_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Bge_S"/>
      /// </summary>
      public static readonly OpCode Bge_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Bgt_S"/>
      /// </summary>
      public static readonly OpCode Bgt_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ble_S"/>
      /// </summary>
      public static readonly OpCode Ble_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Blt_S"/>
      /// </summary>
      public static readonly OpCode Blt_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Bne_Un_S"/>
      /// </summary>
      public static readonly OpCode Bne_Un_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Bge_Un_S"/>
      /// </summary>
      public static readonly OpCode Bge_Un_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Bgt_Un_S"/>
      /// </summary>
      public static readonly OpCode Bgt_Un_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ble_Un_S"/>
      /// </summary>
      public static readonly OpCode Ble_Un_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Blt_Un_S"/>
      /// </summary>
      public static readonly OpCode Blt_Un_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Br"/>
      /// </summary>
      public static readonly OpCode Br;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Brfalse"/>
      /// </summary>
      public static readonly OpCode Brfalse;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Brtrue"/>
      /// </summary>
      public static readonly OpCode Brtrue;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Beq"/>
      /// </summary>
      public static readonly OpCode Beq;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Bge"/>
      /// </summary>
      public static readonly OpCode Bge;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Bgt"/>
      /// </summary>
      public static readonly OpCode Bgt;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ble"/>
      /// </summary>
      public static readonly OpCode Ble;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Blt"/>
      /// </summary>
      public static readonly OpCode Blt;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Bne_Un"/>
      /// </summary>
      public static readonly OpCode Bne_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Bge_Un"/>
      /// </summary>
      public static readonly OpCode Bge_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Bgt_Un"/>
      /// </summary>
      public static readonly OpCode Bgt_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ble_Un"/>
      /// </summary>
      public static readonly OpCode Ble_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Blt_Un"/>
      /// </summary>
      public static readonly OpCode Blt_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Switch"/>
      /// </summary>
      public static readonly OpCode Switch;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldind_I1"/>
      /// </summary>
      public static readonly OpCode Ldind_I1;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldind_U1"/>
      /// </summary>
      public static readonly OpCode Ldind_U1;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldind_I2"/>
      /// </summary>
      public static readonly OpCode Ldind_I2;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldind_U2"/>
      /// </summary>
      public static readonly OpCode Ldind_U2;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldind_I4"/>
      /// </summary>
      public static readonly OpCode Ldind_I4;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldind_U4"/>
      /// </summary>
      public static readonly OpCode Ldind_U4;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldind_I8"/>
      /// </summary>
      public static readonly OpCode Ldind_I8;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldind_I"/>
      /// </summary>
      public static readonly OpCode Ldind_I;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldind_R4"/>
      /// </summary>
      public static readonly OpCode Ldind_R4;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldind_R8"/>
      /// </summary>
      public static readonly OpCode Ldind_R8;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldind_Ref"/>
      /// </summary>
      public static readonly OpCode Ldind_Ref;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stind_Ref"/>
      /// </summary>
      public static readonly OpCode Stind_Ref;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stind_I1"/>
      /// </summary>
      public static readonly OpCode Stind_I1;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stind_I2"/>
      /// </summary>
      public static readonly OpCode Stind_I2;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stind_I4"/>
      /// </summary>
      public static readonly OpCode Stind_I4;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stind_I8"/>
      /// </summary>
      public static readonly OpCode Stind_I8;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stind_R4"/>
      /// </summary>
      public static readonly OpCode Stind_R4;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stind_R8"/>
      /// </summary>
      public static readonly OpCode Stind_R8;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Add"/>
      /// </summary>
      public static readonly OpCode Add;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Sub"/>
      /// </summary>
      public static readonly OpCode Sub;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Mul"/>
      /// </summary>
      public static readonly OpCode Mul;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Div"/>
      /// </summary>
      public static readonly OpCode Div;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Div_Un"/>
      /// </summary>
      public static readonly OpCode Div_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Rem"/>
      /// </summary>
      public static readonly OpCode Rem;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Rem_Un"/>
      /// </summary>
      public static readonly OpCode Rem_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.And"/>
      /// </summary>
      public static readonly OpCode And;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Or"/>
      /// </summary>
      public static readonly OpCode Or;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Xor"/>
      /// </summary>
      public static readonly OpCode Xor;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Shl"/>
      /// </summary>
      public static readonly OpCode Shl;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Shr"/>
      /// </summary>
      public static readonly OpCode Shr;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Shr_Un"/>
      /// </summary>
      public static readonly OpCode Shr_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Neg"/>
      /// </summary>
      public static readonly OpCode Neg;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Not"/>
      /// </summary>
      public static readonly OpCode Not;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_I1"/>
      /// </summary>
      public static readonly OpCode Conv_I1;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_I2"/>
      /// </summary>
      public static readonly OpCode Conv_I2;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_I4"/>
      /// </summary>
      public static readonly OpCode Conv_I4;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_I8"/>
      /// </summary>
      public static readonly OpCode Conv_I8;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_R4"/>
      /// </summary>
      public static readonly OpCode Conv_R4;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_R8"/>
      /// </summary>
      public static readonly OpCode Conv_R8;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_U4"/>
      /// </summary>
      public static readonly OpCode Conv_U4;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_U8"/>
      /// </summary>
      public static readonly OpCode Conv_U8;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Callvirt"/>
      /// </summary>
      public static readonly OpCode Callvirt;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Cpobj"/>
      /// </summary>
      public static readonly OpCode Cpobj;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldobj"/>
      /// </summary>
      public static readonly OpCode Ldobj;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldstr"/>
      /// </summary>
      public static readonly OpCode Ldstr;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Newobj"/>
      /// </summary>
      public static readonly OpCode Newobj;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Castclass"/>
      /// </summary>
      public static readonly OpCode Castclass;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Isinst"/>
      /// </summary>
      public static readonly OpCode Isinst;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_R_Un"/>
      /// </summary>
      public static readonly OpCode Conv_R_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Unbox"/>
      /// </summary>
      public static readonly OpCode Unbox;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Throw"/>
      /// </summary>
      public static readonly OpCode Throw;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldfld"/>
      /// </summary>
      public static readonly OpCode Ldfld;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldflda"/>
      /// </summary>
      public static readonly OpCode Ldflda;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stfld"/>
      /// </summary>
      public static readonly OpCode Stfld;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldsfld"/>
      /// </summary>
      public static readonly OpCode Ldsfld;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldsflda"/>
      /// </summary>
      public static readonly OpCode Ldsflda;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stsfld"/>
      /// </summary>
      public static readonly OpCode Stsfld;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stobj"/>
      /// </summary>
      public static readonly OpCode Stobj;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_I1_Un"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_I1_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_I2_Un"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_I2_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_I4_Un"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_I4_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_I8_Un"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_I8_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_U1_Un"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_U1_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_U2_Un"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_U2_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_U4_Un"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_U4_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_U8_Un"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_U8_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_I_Un"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_I_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_U_Un"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_U_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Box"/>
      /// </summary>
      public static readonly OpCode Box;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Newarr"/>
      /// </summary>
      public static readonly OpCode Newarr;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldlen"/>
      /// </summary>
      public static readonly OpCode Ldlen;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldelema"/>
      /// </summary>
      public static readonly OpCode Ldelema;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldelem_I1"/>
      /// </summary>
      public static readonly OpCode Ldelem_I1;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldelem_U1"/>
      /// </summary>
      public static readonly OpCode Ldelem_U1;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldelem_I2"/>
      /// </summary>
      public static readonly OpCode Ldelem_I2;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldelem_U2"/>
      /// </summary>
      public static readonly OpCode Ldelem_U2;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldelem_I4"/>
      /// </summary>
      public static readonly OpCode Ldelem_I4;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldelem_U4"/>
      /// </summary>
      public static readonly OpCode Ldelem_U4;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldelem_I8"/>
      /// </summary>
      public static readonly OpCode Ldelem_I8;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldelem_I"/>
      /// </summary>
      public static readonly OpCode Ldelem_I;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldelem_R4"/>
      /// </summary>
      public static readonly OpCode Ldelem_R4;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldelem_R8"/>
      /// </summary>
      public static readonly OpCode Ldelem_R8;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldelem_Ref"/>
      /// </summary>
      public static readonly OpCode Ldelem_Ref;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stelem_I"/>
      /// </summary>
      public static readonly OpCode Stelem_I;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stelem_I1"/>
      /// </summary>
      public static readonly OpCode Stelem_I1;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stelem_I2"/>
      /// </summary>
      public static readonly OpCode Stelem_I2;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stelem_I4"/>
      /// </summary>
      public static readonly OpCode Stelem_I4;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stelem_I8"/>
      /// </summary>
      public static readonly OpCode Stelem_I8;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stelem_R4"/>
      /// </summary>
      public static readonly OpCode Stelem_R4;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stelem_R8"/>
      /// </summary>
      public static readonly OpCode Stelem_R8;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stelem_Ref"/>
      /// </summary>
      public static readonly OpCode Stelem_Ref;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldelem"/>
      /// </summary>
      public static readonly OpCode Ldelem;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stelem"/>
      /// </summary>
      public static readonly OpCode Stelem;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Unbox_Any"/>
      /// </summary>
      public static readonly OpCode Unbox_Any;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_I1"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_I1;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_U1"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_U1;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_I2"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_I2;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_U2"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_U2;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_I4"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_I4;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_U4"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_U4;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_I8"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_I8;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_U8"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_U8;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Refanyval"/>
      /// </summary>
      public static readonly OpCode Refanyval;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ckfinite"/>
      /// </summary>
      public static readonly OpCode Ckfinite;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Mkrefany"/>
      /// </summary>
      public static readonly OpCode Mkrefany;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldtoken"/>
      /// </summary>
      public static readonly OpCode Ldtoken;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_U2"/>
      /// </summary>
      public static readonly OpCode Conv_U2;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_U1"/>
      /// </summary>
      public static readonly OpCode Conv_U1;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_I"/>
      /// </summary>
      public static readonly OpCode Conv_I;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_I"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_I;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_Ovf_U"/>
      /// </summary>
      public static readonly OpCode Conv_Ovf_U;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Add_Ovf"/>
      /// </summary>
      public static readonly OpCode Add_Ovf;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Add_Ovf_Un"/>
      /// </summary>
      public static readonly OpCode Add_Ovf_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Mul_Ovf"/>
      /// </summary>
      public static readonly OpCode Mul_Ovf;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Mul_Ovf_Un"/>
      /// </summary>
      public static readonly OpCode Mul_Ovf_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Sub_Ovf"/>
      /// </summary>
      public static readonly OpCode Sub_Ovf;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Sub_Ovf_Un"/>
      /// </summary>
      public static readonly OpCode Sub_Ovf_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Endfinally"/>
      /// </summary>
      public static readonly OpCode Endfinally;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Leave"/>
      /// </summary>
      public static readonly OpCode Leave;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Leave_S"/>
      /// </summary>
      public static readonly OpCode Leave_S;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stind_I"/>
      /// </summary>
      public static readonly OpCode Stind_I;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Conv_U"/>
      /// </summary>
      public static readonly OpCode Conv_U;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Arglist"/>
      /// </summary>
      public static readonly OpCode Arglist;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ceq"/>
      /// </summary>
      public static readonly OpCode Ceq;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Cgt"/>
      /// </summary>
      public static readonly OpCode Cgt;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Cgt_Un"/>
      /// </summary>
      public static readonly OpCode Cgt_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Clt"/>
      /// </summary>
      public static readonly OpCode Clt;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Clt_Un"/>
      /// </summary>
      public static readonly OpCode Clt_Un;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldftn"/>
      /// </summary>
      public static readonly OpCode Ldftn;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldvirtftn"/>
      /// </summary>
      public static readonly OpCode Ldvirtftn;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldarg"/>
      /// </summary>
      public static readonly OpCode Ldarg;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldarga"/>
      /// </summary>
      public static readonly OpCode Ldarga;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Starg"/>
      /// </summary>
      public static readonly OpCode Starg;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldloc"/>
      /// </summary>
      public static readonly OpCode Ldloc;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Ldloca"/>
      /// </summary>
      public static readonly OpCode Ldloca;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Stloc"/>
      /// </summary>
      public static readonly OpCode Stloc;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Localloc"/>
      /// </summary>
      public static readonly OpCode Localloc;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Endfilter"/>
      /// </summary>
      public static readonly OpCode Endfilter;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Unaligned"/>
      /// </summary>
      public static readonly OpCode Unaligned_;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Volatile"/>
      /// </summary>
      public static readonly OpCode Volatile_;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Tailcall"/>
      /// </summary>
      public static readonly OpCode Tail_;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Initobj"/>
      /// </summary>
      public static readonly OpCode Initobj;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Constrained"/>
      /// </summary>
      public static readonly OpCode Constrained_;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Cpblk"/>
      /// </summary>
      public static readonly OpCode Cpblk;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Initblk"/>
      /// </summary>
      public static readonly OpCode Initblk;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Rethrow"/>
      /// </summary>
      public static readonly OpCode Rethrow;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Sizeof"/>
      /// </summary>
      public static readonly OpCode Sizeof;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Refanytype"/>
      /// </summary>
      public static readonly OpCode Refanytype;
      /// <summary>
      /// See <see cref="F:System.Reflection.Emit.OpCodes.Readonly"/>
      /// </summary>
      public static readonly OpCode Readonly_;

      static OpCodes()
      {
         Nop = new OpCode( OpCodeID.Nop, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Break = new OpCode( OpCodeID.Break, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Break, false );
         Ldarg_0 = new OpCode( OpCodeID.Ldarg_0, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldarg_1 = new OpCode( OpCodeID.Ldarg_1, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldarg_2 = new OpCode( OpCodeID.Ldarg_2, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldarg_3 = new OpCode( OpCodeID.Ldarg_3, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldloc_0 = new OpCode( OpCodeID.Ldloc_0, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldloc_1 = new OpCode( OpCodeID.Ldloc_1, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldloc_2 = new OpCode( OpCodeID.Ldloc_2, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldloc_3 = new OpCode( OpCodeID.Ldloc_3, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Stloc_0 = new OpCode( OpCodeID.Stloc_0, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Stloc_1 = new OpCode( OpCodeID.Stloc_1, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Stloc_2 = new OpCode( OpCodeID.Stloc_2, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Stloc_3 = new OpCode( OpCodeID.Stloc_3, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldarg_S = new OpCode( OpCodeID.Ldarg_S, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.ShortInlineVar, OpCodeType.Macro, FlowControl.Next, false );
         Ldarga_S = new OpCode( OpCodeID.Ldarga_S, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.ShortInlineVar, OpCodeType.Macro, FlowControl.Next, false );
         Starg_S = new OpCode( OpCodeID.Starg_S, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.ShortInlineVar, OpCodeType.Macro, FlowControl.Next, false );
         Ldloc_S = new OpCode( OpCodeID.Ldloc_S, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.ShortInlineVar, OpCodeType.Macro, FlowControl.Next, false );
         Ldloca_S = new OpCode( OpCodeID.Ldloca_S, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.ShortInlineVar, OpCodeType.Macro, FlowControl.Next, false );
         Stloc_S = new OpCode( OpCodeID.Stloc_S, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.ShortInlineVar, OpCodeType.Macro, FlowControl.Next, false );
         Ldnull = new OpCode( OpCodeID.Ldnull, StackBehaviourPop.Pop0, StackBehaviourPush.Pushref, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldc_I4_M1 = new OpCode( OpCodeID.Ldc_I4_M1, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_0 = new OpCode( OpCodeID.Ldc_I4_0, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_1 = new OpCode( OpCodeID.Ldc_I4_1, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_2 = new OpCode( OpCodeID.Ldc_I4_2, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_3 = new OpCode( OpCodeID.Ldc_I4_3, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_4 = new OpCode( OpCodeID.Ldc_I4_4, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_5 = new OpCode( OpCodeID.Ldc_I4_5, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_6 = new OpCode( OpCodeID.Ldc_I4_6, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_7 = new OpCode( OpCodeID.Ldc_I4_7, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_8 = new OpCode( OpCodeID.Ldc_I4_8, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_S = new OpCode( OpCodeID.Ldc_I4_S, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.ShortInlineI, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4 = new OpCode( OpCodeID.Ldc_I4, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineI, OpCodeType.Primitive, FlowControl.Next, false );
         Ldc_I8 = new OpCode( OpCodeID.Ldc_I8, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi8, OperandType.InlineI8, OpCodeType.Primitive, FlowControl.Next, false );
         Ldc_R4 = new OpCode( OpCodeID.Ldc_R4, StackBehaviourPop.Pop0, StackBehaviourPush.Pushr4, OperandType.ShortInlineR, OpCodeType.Primitive, FlowControl.Next, false );
         Ldc_R8 = new OpCode( OpCodeID.Ldc_R8, StackBehaviourPop.Pop0, StackBehaviourPush.Pushr8, OperandType.InlineR, OpCodeType.Primitive, FlowControl.Next, false );
         Dup = new OpCode( OpCodeID.Dup, StackBehaviourPop.Pop1, StackBehaviourPush.Push1_push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Pop = new OpCode( OpCodeID.Pop, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Jmp = new OpCode( OpCodeID.Jmp, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineMethod, OpCodeType.Primitive, FlowControl.Call, true );
         Call = new OpCode( OpCodeID.Call, StackBehaviourPop.Varpop, StackBehaviourPush.Varpush, OperandType.InlineMethod, OpCodeType.Primitive, FlowControl.Call, false );
         Calli = new OpCode( OpCodeID.Calli, StackBehaviourPop.Varpop, StackBehaviourPush.Varpush, OperandType.InlineSignature, OpCodeType.Primitive, FlowControl.Call, false );
         Ret = new OpCode( OpCodeID.Ret, StackBehaviourPop.Varpop, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Return, true );
         Br_S = new OpCode( OpCodeID.Br_S, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Branch, true, OpCodeID.Br );
         Brfalse_S = new OpCode( OpCodeID.Brfalse_S, StackBehaviourPop.Popi, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Brfalse );
         Brtrue_S = new OpCode( OpCodeID.Brtrue_S, StackBehaviourPop.Popi, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Brtrue );
         Beq_S = new OpCode( OpCodeID.Beq_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Beq );
         Bge_S = new OpCode( OpCodeID.Bge_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Bge );
         Bgt_S = new OpCode( OpCodeID.Bgt_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Bgt );
         Ble_S = new OpCode( OpCodeID.Ble_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Ble );
         Blt_S = new OpCode( OpCodeID.Blt_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Blt );
         Bne_Un_S = new OpCode( OpCodeID.Bne_Un_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Bne_Un );
         Bge_Un_S = new OpCode( OpCodeID.Bge_Un_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Bge_Un );
         Bgt_Un_S = new OpCode( OpCodeID.Bgt_Un_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Bgt_Un );
         Ble_Un_S = new OpCode( OpCodeID.Ble_Un_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Ble_Un );
         Blt_Un_S = new OpCode( OpCodeID.Blt_Un_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Blt_Un );
         Br = new OpCode( OpCodeID.Br, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Primitive, FlowControl.Branch, true, OpCodeID.Br_S );
         Brfalse = new OpCode( OpCodeID.Brfalse, StackBehaviourPop.Popi, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Primitive, FlowControl.Cond_Branch, false, OpCodeID.Brfalse_S );
         Brtrue = new OpCode( OpCodeID.Brtrue, StackBehaviourPop.Popi, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Primitive, FlowControl.Cond_Branch, false, OpCodeID.Brtrue_S );
         Beq = new OpCode( OpCodeID.Beq, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Beq_S );
         Bge = new OpCode( OpCodeID.Bge, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Bge_S );
         Bgt = new OpCode( OpCodeID.Bgt, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Bgt_S );
         Ble = new OpCode( OpCodeID.Ble, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Ble_S );
         Blt = new OpCode( OpCodeID.Blt, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Blt_S );
         Bne_Un = new OpCode( OpCodeID.Bne_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Bne_Un_S );
         Bge_Un = new OpCode( OpCodeID.Bge_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Bge_Un_S );
         Bgt_Un = new OpCode( OpCodeID.Bgt_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Bgt_Un_S );
         Ble_Un = new OpCode( OpCodeID.Ble_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Ble_Un_S );
         Blt_Un = new OpCode( OpCodeID.Blt_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false, OpCodeID.Blt_Un_S );
         Switch = new OpCode( OpCodeID.Switch, StackBehaviourPop.Popi, StackBehaviourPush.Push0, OperandType.InlineSwitch, OpCodeType.Primitive, FlowControl.Cond_Branch, false );
         Ldind_I1 = new OpCode( OpCodeID.Ldind_I1, StackBehaviourPop.Popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_U1 = new OpCode( OpCodeID.Ldind_U1, StackBehaviourPop.Popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_I2 = new OpCode( OpCodeID.Ldind_I2, StackBehaviourPop.Popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_U2 = new OpCode( OpCodeID.Ldind_U2, StackBehaviourPop.Popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_I4 = new OpCode( OpCodeID.Ldind_I4, StackBehaviourPop.Popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_U4 = new OpCode( OpCodeID.Ldind_U4, StackBehaviourPop.Popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_I8 = new OpCode( OpCodeID.Ldind_I8, StackBehaviourPop.Popi, StackBehaviourPush.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_I = new OpCode( OpCodeID.Ldind_I, StackBehaviourPop.Popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_R4 = new OpCode( OpCodeID.Ldind_R4, StackBehaviourPop.Popi, StackBehaviourPush.Pushr4, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_R8 = new OpCode( OpCodeID.Ldind_R8, StackBehaviourPop.Popi, StackBehaviourPush.Pushr8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_Ref = new OpCode( OpCodeID.Ldind_Ref, StackBehaviourPop.Popi, StackBehaviourPush.Pushref, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Stind_Ref = new OpCode( OpCodeID.Stind_Ref, StackBehaviourPop.Popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Stind_I1 = new OpCode( OpCodeID.Stind_I1, StackBehaviourPop.Popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Stind_I2 = new OpCode( OpCodeID.Stind_I2, StackBehaviourPop.Popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Stind_I4 = new OpCode( OpCodeID.Stind_I4, StackBehaviourPop.Popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Stind_I8 = new OpCode( OpCodeID.Stind_I8, StackBehaviourPop.Popi_popi8, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Stind_R4 = new OpCode( OpCodeID.Stind_R4, StackBehaviourPop.Popi_popr4, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Stind_R8 = new OpCode( OpCodeID.Stind_R8, StackBehaviourPop.Popi_popr8, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Add = new OpCode( OpCodeID.Add, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Sub = new OpCode( OpCodeID.Sub, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Mul = new OpCode( OpCodeID.Mul, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Div = new OpCode( OpCodeID.Div, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Div_Un = new OpCode( OpCodeID.Div_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Rem = new OpCode( OpCodeID.Rem, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Rem_Un = new OpCode( OpCodeID.Rem_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         And = new OpCode( OpCodeID.And, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Or = new OpCode( OpCodeID.Or, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Xor = new OpCode( OpCodeID.Xor, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Shl = new OpCode( OpCodeID.Shl, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Shr = new OpCode( OpCodeID.Shr, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Shr_Un = new OpCode( OpCodeID.Shr_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Neg = new OpCode( OpCodeID.Neg, StackBehaviourPop.Pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Not = new OpCode( OpCodeID.Not, StackBehaviourPop.Pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_I1 = new OpCode( OpCodeID.Conv_I1, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_I2 = new OpCode( OpCodeID.Conv_I2, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_I4 = new OpCode( OpCodeID.Conv_I4, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_I8 = new OpCode( OpCodeID.Conv_I8, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_R4 = new OpCode( OpCodeID.Conv_R4, StackBehaviourPop.Pop1, StackBehaviourPush.Pushr4, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_R8 = new OpCode( OpCodeID.Conv_R8, StackBehaviourPop.Pop1, StackBehaviourPush.Pushr8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_U4 = new OpCode( OpCodeID.Conv_U4, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_U8 = new OpCode( OpCodeID.Conv_U8, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Callvirt = new OpCode( OpCodeID.Callvirt, StackBehaviourPop.Varpop, StackBehaviourPush.Varpush, OperandType.InlineMethod, OpCodeType.Objmodel, FlowControl.Call, false );
         Cpobj = new OpCode( OpCodeID.Cpobj, StackBehaviourPop.Popi_popi, StackBehaviourPush.Push0, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldobj = new OpCode( OpCodeID.Ldobj, StackBehaviourPop.Popi, StackBehaviourPush.Push1, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldstr = new OpCode( OpCodeID.Ldstr, StackBehaviourPop.Pop0, StackBehaviourPush.Pushref, OperandType.InlineString, OpCodeType.Objmodel, FlowControl.Next, false );
         Newobj = new OpCode( OpCodeID.Newobj, StackBehaviourPop.Varpop, StackBehaviourPush.Pushref, OperandType.InlineMethod, OpCodeType.Objmodel, FlowControl.Call, false );
         Castclass = new OpCode( OpCodeID.Castclass, StackBehaviourPop.Popref, StackBehaviourPush.Pushref, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Isinst = new OpCode( OpCodeID.Isinst, StackBehaviourPop.Popref, StackBehaviourPush.Pushi, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Conv_R_Un = new OpCode( OpCodeID.Conv_R_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushr8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Unbox = new OpCode( OpCodeID.Unbox, StackBehaviourPop.Popref, StackBehaviourPush.Pushi, OperandType.InlineType, OpCodeType.Primitive, FlowControl.Next, false );
         Throw = new OpCode( OpCodeID.Throw, StackBehaviourPop.Popref, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Throw, true );
         Ldfld = new OpCode( OpCodeID.Ldfld, StackBehaviourPop.Popref, StackBehaviourPush.Push1, OperandType.InlineField, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldflda = new OpCode( OpCodeID.Ldflda, StackBehaviourPop.Popref, StackBehaviourPush.Pushi, OperandType.InlineField, OpCodeType.Objmodel, FlowControl.Next, false );
         Stfld = new OpCode( OpCodeID.Stfld, StackBehaviourPop.Popref_pop1, StackBehaviourPush.Push0, OperandType.InlineField, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldsfld = new OpCode( OpCodeID.Ldsfld, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineField, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldsflda = new OpCode( OpCodeID.Ldsflda, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineField, OpCodeType.Objmodel, FlowControl.Next, false );
         Stsfld = new OpCode( OpCodeID.Stsfld, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.InlineField, OpCodeType.Objmodel, FlowControl.Next, false );
         Stobj = new OpCode( OpCodeID.Stobj, StackBehaviourPop.Popi_pop1, StackBehaviourPush.Push0, OperandType.InlineType, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_I1_Un = new OpCode( OpCodeID.Conv_Ovf_I1_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_I2_Un = new OpCode( OpCodeID.Conv_Ovf_I2_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_I4_Un = new OpCode( OpCodeID.Conv_Ovf_I4_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_I8_Un = new OpCode( OpCodeID.Conv_Ovf_I8_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U1_Un = new OpCode( OpCodeID.Conv_Ovf_U1_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U2_Un = new OpCode( OpCodeID.Conv_Ovf_U2_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U4_Un = new OpCode( OpCodeID.Conv_Ovf_U4_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U8_Un = new OpCode( OpCodeID.Conv_Ovf_U8_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_I_Un = new OpCode( OpCodeID.Conv_Ovf_I_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U_Un = new OpCode( OpCodeID.Conv_Ovf_U_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Box = new OpCode( OpCodeID.Box, StackBehaviourPop.Pop1, StackBehaviourPush.Pushref, OperandType.InlineType, OpCodeType.Primitive, FlowControl.Next, false );
         Newarr = new OpCode( OpCodeID.Newarr, StackBehaviourPop.Popi, StackBehaviourPush.Pushref, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldlen = new OpCode( OpCodeID.Ldlen, StackBehaviourPop.Popref, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelema = new OpCode( OpCodeID.Ldelema, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushi, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_I1 = new OpCode( OpCodeID.Ldelem_I1, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_U1 = new OpCode( OpCodeID.Ldelem_U1, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_I2 = new OpCode( OpCodeID.Ldelem_I2, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_U2 = new OpCode( OpCodeID.Ldelem_U2, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_I4 = new OpCode( OpCodeID.Ldelem_I4, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_U4 = new OpCode( OpCodeID.Ldelem_U4, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_I8 = new OpCode( OpCodeID.Ldelem_I8, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushi8, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_I = new OpCode( OpCodeID.Ldelem_I, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_R4 = new OpCode( OpCodeID.Ldelem_R4, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushr4, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_R8 = new OpCode( OpCodeID.Ldelem_R8, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushr8, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_Ref = new OpCode( OpCodeID.Ldelem_Ref, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushref, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Stelem_I = new OpCode( OpCodeID.Stelem_I, StackBehaviourPop.Popref_popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Stelem_I1 = new OpCode( OpCodeID.Stelem_I1, StackBehaviourPop.Popref_popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Stelem_I2 = new OpCode( OpCodeID.Stelem_I2, StackBehaviourPop.Popref_popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Stelem_I4 = new OpCode( OpCodeID.Stelem_I4, StackBehaviourPop.Popref_popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Stelem_I8 = new OpCode( OpCodeID.Stelem_I8, StackBehaviourPop.Popref_popi_popi8, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Stelem_R4 = new OpCode( OpCodeID.Stelem_R4, StackBehaviourPop.Popref_popi_popr4, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Stelem_R8 = new OpCode( OpCodeID.Stelem_R8, StackBehaviourPop.Popref_popi_popr8, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Stelem_Ref = new OpCode( OpCodeID.Stelem_Ref, StackBehaviourPop.Popref_popi_popref, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem = new OpCode( OpCodeID.Ldelem, StackBehaviourPop.Popref_popi, StackBehaviourPush.Push1, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Stelem = new OpCode( OpCodeID.Stelem, StackBehaviourPop.Popref_popi_pop1, StackBehaviourPush.Push0, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Unbox_Any = new OpCode( OpCodeID.Unbox_Any, StackBehaviourPop.Popref, StackBehaviourPush.Push1, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Conv_Ovf_I1 = new OpCode( OpCodeID.Conv_Ovf_I1, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U1 = new OpCode( OpCodeID.Conv_Ovf_U1, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_I2 = new OpCode( OpCodeID.Conv_Ovf_I2, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U2 = new OpCode( OpCodeID.Conv_Ovf_U2, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_I4 = new OpCode( OpCodeID.Conv_Ovf_I4, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U4 = new OpCode( OpCodeID.Conv_Ovf_U4, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_I8 = new OpCode( OpCodeID.Conv_Ovf_I8, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U8 = new OpCode( OpCodeID.Conv_Ovf_U8, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Refanyval = new OpCode( OpCodeID.Refanyval, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineType, OpCodeType.Primitive, FlowControl.Next, false );
         Ckfinite = new OpCode( OpCodeID.Ckfinite, StackBehaviourPop.Pop1, StackBehaviourPush.Pushr8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Mkrefany = new OpCode( OpCodeID.Mkrefany, StackBehaviourPop.Popi, StackBehaviourPush.Push1, OperandType.InlineType, OpCodeType.Primitive, FlowControl.Next, false );
         Ldtoken = new OpCode( OpCodeID.Ldtoken, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineToken, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_U2 = new OpCode( OpCodeID.Conv_U2, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_U1 = new OpCode( OpCodeID.Conv_U1, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_I = new OpCode( OpCodeID.Conv_I, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_I = new OpCode( OpCodeID.Conv_Ovf_I, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U = new OpCode( OpCodeID.Conv_Ovf_U, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Add_Ovf = new OpCode( OpCodeID.Add_Ovf, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Add_Ovf_Un = new OpCode( OpCodeID.Add_Ovf_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Mul_Ovf = new OpCode( OpCodeID.Mul_Ovf, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Mul_Ovf_Un = new OpCode( OpCodeID.Mul_Ovf_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Sub_Ovf = new OpCode( OpCodeID.Sub_Ovf, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Sub_Ovf_Un = new OpCode( OpCodeID.Sub_Ovf_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Endfinally = new OpCode( OpCodeID.Endfinally, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Return, true );
         Leave = new OpCode( OpCodeID.Leave, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Primitive, FlowControl.Branch, true, OpCodeID.Leave_S );
         Leave_S = new OpCode( OpCodeID.Leave_S, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Primitive, FlowControl.Branch, true, OpCodeID.Leave );
         Stind_I = new OpCode( OpCodeID.Stind_I, StackBehaviourPop.Popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_U = new OpCode( OpCodeID.Conv_U, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Arglist = new OpCode( OpCodeID.Arglist, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ceq = new OpCode( OpCodeID.Ceq, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Cgt = new OpCode( OpCodeID.Cgt, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Cgt_Un = new OpCode( OpCodeID.Cgt_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Clt = new OpCode( OpCodeID.Clt, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Clt_Un = new OpCode( OpCodeID.Clt_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldftn = new OpCode( OpCodeID.Ldftn, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineMethod, OpCodeType.Primitive, FlowControl.Next, false );
         Ldvirtftn = new OpCode( OpCodeID.Ldvirtftn, StackBehaviourPop.Popref, StackBehaviourPush.Pushi, OperandType.InlineMethod, OpCodeType.Primitive, FlowControl.Next, false );
         Ldarg = new OpCode( OpCodeID.Ldarg, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineVar, OpCodeType.Primitive, FlowControl.Next, false );
         Ldarga = new OpCode( OpCodeID.Ldarga, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineVar, OpCodeType.Primitive, FlowControl.Next, false );
         Starg = new OpCode( OpCodeID.Starg, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.InlineVar, OpCodeType.Primitive, FlowControl.Next, false );
         Ldloc = new OpCode( OpCodeID.Ldloc, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineVar, OpCodeType.Primitive, FlowControl.Next, false );
         Ldloca = new OpCode( OpCodeID.Ldloca, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineVar, OpCodeType.Primitive, FlowControl.Next, false );
         Stloc = new OpCode( OpCodeID.Stloc, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.InlineVar, OpCodeType.Primitive, FlowControl.Next, false );
         Localloc = new OpCode( OpCodeID.Localloc, StackBehaviourPop.Popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Endfilter = new OpCode( OpCodeID.Endfilter, StackBehaviourPop.Popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Return, true );
         Unaligned_ = new OpCode( OpCodeID.Unaligned_, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.ShortInlineI, OpCodeType.Prefix, FlowControl.Meta, false );
         Volatile_ = new OpCode( OpCodeID.Volatile_, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Prefix, FlowControl.Meta, false );
         Tail_ = new OpCode( OpCodeID.Tail_, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Prefix, FlowControl.Meta, false );
         Initobj = new OpCode( OpCodeID.Initobj, StackBehaviourPop.Popi, StackBehaviourPush.Push0, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Constrained_ = new OpCode( OpCodeID.Constrained_, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineType, OpCodeType.Prefix, FlowControl.Meta, false );
         Cpblk = new OpCode( OpCodeID.Cpblk, StackBehaviourPop.Popi_popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Initblk = new OpCode( OpCodeID.Initblk, StackBehaviourPop.Popi_popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Rethrow = new OpCode( OpCodeID.Rethrow, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Throw, true );
         Sizeof = new OpCode( OpCodeID.Sizeof, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineType, OpCodeType.Primitive, FlowControl.Next, false );
         Refanytype = new OpCode( OpCodeID.Refanytype, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Readonly_ = new OpCode( OpCodeID.Readonly_, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Prefix, FlowControl.Meta, false );

      }
   }

   /// <summary>
   /// This struct contains all required information about a single CIL op code. Very similar to System.Reflection.Emit.OpCode struct.
   /// </summary>
   public struct OpCode
   {
      /// <summary>
      /// Contains the maximum value for <see cref="Physical.OpCodeID"/> which would fit into one byte.
      /// </summary>
      /// <remarks>This value is <c>0xFE</c>.</remarks>
      public const Int32 MAX_ONE_BYTE_INSTRUCTION = 0xFE;

      private static class NameCache
      {
         // Reserve enough space for all possible op codes useable with 2 bytes by current encoding
         internal static readonly String[] Cache = new String[Byte.MaxValue * 2 + 2];
      }

      private const UInt64 STACK_POP_MASK = 0x1FU;

      private const UInt64 STACK_PUSH_MASK = 0x1E0U;
      private const Int32 STACK_PUSH_SHIFT = 5;

      private const UInt64 OPERAND_TYPE_MASK = 0x3E00U;
      private const Int32 OPERAND_TYPE_SHIFT = STACK_PUSH_SHIFT + 4;

      private const UInt64 OPCODE_TYPE_MASK = 0x1C000U;
      private const Int32 OPCODE_TYPE_SHIFT = OPERAND_TYPE_SHIFT + 5;

      private const UInt64 FLOW_CONTROL_MASK = 0x1E0000U;
      private const Int32 FLOW_CONTROL_SHIFT = OPCODE_TYPE_SHIFT + 3;

      private const UInt64 SIZE_MASK = 0x200000U;
      private const Int32 SIZE_SHIFT = FLOW_CONTROL_SHIFT + 4;

      private const UInt64 VALUE_MASK = 0x3FFFC00000U;
      private const Int32 VALUE_SHIFT = SIZE_SHIFT + 1;

      private const UInt64 STACK_CHANGE_MASK = 0x1C000000000U;
      private const Int32 STACK_CHANGE_SHIFT = VALUE_SHIFT + 16;

      private const UInt64 ENDS_BLK_CODE_MASK = 0x20000000000U;
      private const Int32 ENDS_BLK_CODE_SHIFT = STACK_CHANGE_SHIFT + 3;

      private const UInt64 OPERAND_SIZE_MASK = 0x3C0000000000U;
      private const Int32 OPERAND_SIZE_SHIFT = ENDS_BLK_CODE_SHIFT + 1;

      private const UInt64 OTHER_FORM_MASK = 0x3FC00000000000U;
      private const Int32 OTHER_FORM_SHIFT = OPERAND_SIZE_SHIFT + 4;

      // Bits 0-4 (5 bits): StackBehaviourPop
      // Bits 5-8 (4 bits): StackBehaviourPush
      // Bits 9-13 (5 bits): OperandType
      // Bits 14-16 (3 bits): OpCodeType
      // Bits 17-20 (4 bits): FlowControl
      // Bit 21: Size (0 = 1, 1 = 2)
      // Bits 22-37 (16 bits): Bytes 1 & 2
      // Bits 38-40 (3 bits): Stack change (0 = -3, 1 = -2, etc)
      // Bit 41: Unconditionally ends bulk of code (1 = true, 0 = false)
      // Bits 42-45 (4bits): Operand size in bytes
      // Bits 46-53 (8bits): Other form (short or long) for branch-instruction
      private readonly UInt64 _state;

      internal OpCode(
         OpCodeID encoded,
         StackBehaviourPop stackPop,
         StackBehaviourPush stackPush,
         OperandType operand,
         OpCodeType type,
         FlowControl flowControl,
         Boolean unconditionallyEndsBulkOfCode
         ) : this( encoded, stackPop, stackPush, operand, type, flowControl, unconditionallyEndsBulkOfCode, 0 )
      {

      }

      internal OpCode(
         OpCodeID encoded,
         StackBehaviourPop stackPop,
         StackBehaviourPush stackPush,
         OperandType operand,
         OpCodeType type,
         FlowControl flowControl,
         Boolean unconditionallyEndsBulkOfCode,
         OpCodeID otherForm
         )
      {
         var stackChange = 0;
         switch ( stackPop )
         {
            case StackBehaviourPop.Pop0:
            case StackBehaviourPop.Varpop:
               break;
            case StackBehaviourPop.Pop1:
            case StackBehaviourPop.Popi:
            case StackBehaviourPop.Popref:
               --stackChange;
               break;
            case StackBehaviourPop.Pop1_pop1:
            case StackBehaviourPop.Popi_pop1:
            case StackBehaviourPop.Popi_popi:
            case StackBehaviourPop.Popi_popi8:
            case StackBehaviourPop.Popi_popr4:
            case StackBehaviourPop.Popi_popr8:
            case StackBehaviourPop.Popref_pop1:
            case StackBehaviourPop.Popref_popi:
               stackChange -= 2;
               break;
            case StackBehaviourPop.Popi_popi_popi:
            case StackBehaviourPop.Popref_popi_pop1:
            case StackBehaviourPop.Popref_popi_popi:
            case StackBehaviourPop.Popref_popi_popi8:
            case StackBehaviourPop.Popref_popi_popr4:
            case StackBehaviourPop.Popref_popi_popr8:
            case StackBehaviourPop.Popref_popi_popref:
               stackChange -= 3;
               break;
         }
         switch ( stackPush )
         {
            case StackBehaviourPush.Push0:
            case StackBehaviourPush.Varpush:
               break;
            case StackBehaviourPush.Push1:
            case StackBehaviourPush.Pushi:
            case StackBehaviourPush.Pushi8:
            case StackBehaviourPush.Pushr4:
            case StackBehaviourPush.Pushr8:
            case StackBehaviourPush.Pushref:
               ++stackChange;
               break;
            case StackBehaviourPush.Push1_push1:
               stackChange += 2;
               break;
         }

         var size = ( (Int32) encoded ) > MAX_ONE_BYTE_INSTRUCTION ? 1 : 0;
         //var byte1 = (Byte) ( ( (Int32) encoded ) >> 8 );
         //var byte2 = (Byte) encoded;
         UInt64 operandSize;
         switch ( operand )
         {
            case OperandType.InlineNone:
               operandSize = 0U;
               break;
            case OperandType.ShortInlineBrTarget:
            case OperandType.ShortInlineI:
            case OperandType.ShortInlineVar:
               operandSize = 1U;
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
               operandSize = 4U;
               break;
            case OperandType.InlineI8:
            case OperandType.InlineR:
               operandSize = 8U;
               break;
            default:
               operandSize = 0U;
               break;
         }

         this._state =
             ( (UInt64) (UInt32) stackPop )
             | ( ( (UInt64) stackPush ) << STACK_PUSH_SHIFT )
             | ( ( (UInt64) operand ) << OPERAND_TYPE_SHIFT )
             | ( ( (UInt64) type ) << OPCODE_TYPE_SHIFT )
             | ( ( (UInt64) flowControl ) << FLOW_CONTROL_SHIFT )
             | ( ( (UInt64) size ) << SIZE_SHIFT )
             | ( ( ( (UInt64) encoded ) << VALUE_SHIFT ) )
             //| ( ( (UInt64) byte1 ) << BYTE_1_SHIFT )
             //| ( ( (UInt64) byte2 ) << BYTE_2_SHIFT )
             | ( ( (UInt64) stackChange + 3 ) << STACK_CHANGE_SHIFT )
             | ( ( unconditionallyEndsBulkOfCode ? 1UL : 0UL ) << ENDS_BLK_CODE_SHIFT )
             | ( ( operandSize ) << OPERAND_SIZE_SHIFT )
             | ( ( (UInt64) otherForm ) << OTHER_FORM_SHIFT )
             ;
#if DEBUG
         System.Diagnostics.Debug.Assert( this.OpCodeID == encoded );
         System.Diagnostics.Debug.Assert( this.StackPop == stackPop );
         System.Diagnostics.Debug.Assert( this.StackPush == stackPush );
         System.Diagnostics.Debug.Assert( this.OpCodeType == type );
         System.Diagnostics.Debug.Assert( this.Size == size + 1 );
         System.Diagnostics.Debug.Assert( this.FlowControl == flowControl );
         System.Diagnostics.Debug.Assert( this.OperandType == operand );
         System.Diagnostics.Debug.Assert( this.UnconditionallyEndsBulkOfCode == unconditionallyEndsBulkOfCode );
         System.Diagnostics.Debug.Assert( this.StackChange == stackChange );
         System.Diagnostics.Debug.Assert( (UInt64) this.OperandSize == operandSize );
         System.Diagnostics.Debug.Assert( this.OtherForm == otherForm );
#endif
      }

      /// <summary>
      /// Gets the textual name of this <see cref="OpCode"/>.
      /// </summary>
      /// <value>The textual name of this <see cref="OpCode"/>.</value>
      public String Name
      {
         get
         {
            var id = this.OpCodeID;
            var cacheIdx = (Int32) id;
            var cache = NameCache.Cache;
            if ( cacheIdx > MAX_ONE_BYTE_INSTRUCTION )
            {
               cacheIdx = Byte.MaxValue + 1 + ( cacheIdx - ( MAX_ONE_BYTE_INSTRUCTION << 8 ) );
            }
            var retVal = cache[cacheIdx];
            if ( retVal == null )
            {
               retVal = this.OpCodeID.ToString( "g" ).ToLowerInvariant().Replace( "_", "." );
               cache[cacheIdx] = retVal;
            }
            return retVal;
         }
      }

      /// <summary>
      /// Gets the size of this <see cref="OpCode"/> in bytes.
      /// </summary>
      /// <value>The size of this <see cref="OpCode"/> in bytes.</value>
      public Int32 Size
      {
         get
         {
            return (Int32) ( ( this._state & SIZE_MASK ) >> SIZE_SHIFT ) + 1;
         }
      }

      /// <summary>
      /// Gets the <see cref="StackBehaviourPop"/> of this <see cref="OpCode"/>.
      /// </summary>
      /// <value>The <see cref="StackBehaviourPop"/> of this <see cref="OpCode"/>.</value>
      public StackBehaviourPop StackPop
      {
         get
         {
            return (StackBehaviourPop) ( this._state & STACK_POP_MASK );
         }
      }

      /// <summary>
      /// Gets the <see cref="StackBehaviourPush"/> of this <see cref="OpCode"/>.
      /// </summary>
      /// <value>The <see cref="StackBehaviourPush"/> of this <see cref="OpCode"/>.</value>
      public StackBehaviourPush StackPush
      {
         get
         {
            return (StackBehaviourPush) ( ( this._state & STACK_PUSH_MASK ) >> STACK_PUSH_SHIFT );
         }
      }

      /// <summary>
      /// Gets the <see cref="OperandType"/> of this <see cref="OpCode"/>.
      /// </summary>
      /// <value>The <see cref="OperandType"/> of this <see cref="OpCode"/>.</value>
      public OperandType OperandType
      {
         get
         {
            return (OperandType) ( ( this._state & OPERAND_TYPE_MASK ) >> OPERAND_TYPE_SHIFT );
         }
      }

      /// <summary>
      /// Gets the <see cref="OpCodeType"/> of this <see cref="OpCode"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeType"/> of this <see cref="OpCode"/>.</value>
      public OpCodeType OpCodeType
      {
         get
         {
            return (OpCodeType) ( ( this._state & OPCODE_TYPE_MASK ) >> OPCODE_TYPE_SHIFT );
         }
      }

      /// <summary>
      /// Gets the <see cref="FlowControl"/> of this <see cref="OpCode"/>.
      /// </summary>
      /// <value>The <see cref="FlowControl"/> of this <see cref="OpCode"/>.</value>
      public FlowControl FlowControl
      {
         get
         {
            return (FlowControl) ( ( this._state & FLOW_CONTROL_MASK ) >> FLOW_CONTROL_SHIFT );
         }
      }

      /// <summary>
      /// Gets the positive or negative number indicating how this <see cref="OpCode"/> changes stack state.
      /// </summary>
      /// <value>The positive or negative number indicating how this <see cref="OpCode"/> changes stack state.</value>
      public Int32 StackChange
      {
         get
         {
            return (Int32) ( ( this._state & STACK_CHANGE_MASK ) >> STACK_CHANGE_SHIFT ) - 3;
         }
      }

      /// <summary>
      /// Gets the value of this <see cref="OpCode"/> as integer.
      /// </summary>
      /// <value>The value of this <see cref="OpCode"/> as integer.</value>
      public OpCodeID OpCodeID
      {
         get
         {
            return (OpCodeID) ( ( this._state & VALUE_MASK ) >> VALUE_SHIFT );
         }
      }

      /// <summary>
      /// Gets the fixed operand size, in bytes.
      /// </summary>
      public Int32 OperandSize
      {
         get
         {
            return (Int32) ( ( this._state & OPERAND_SIZE_MASK ) >> OPERAND_SIZE_SHIFT );
         }
      }

      /// <summary>
      /// For short branch instructions, returns their long branch instruction aliases.
      /// For long branch instructions, returns their short instruction aliases.
      /// For others, returns <see cref="OpCodeID.Nop"/>.
      /// </summary>
      public OpCodeID OtherForm
      {
         get
         {
            return (OpCodeID) ( ( this._state & OTHER_FORM_MASK ) >> OTHER_FORM_SHIFT );
         }
      }

      internal Boolean UnconditionallyEndsBulkOfCode
      {
         get
         {
            return ( ( this._state & ENDS_BLK_CODE_MASK ) >> ENDS_BLK_CODE_SHIFT ) != 0;
         }
      }

      /// <inheritdoc/>
      public override String ToString()
      {
         return this.Name;
      }

      /// <inheritdoc/>
      public override Boolean Equals( Object obj )
      {
         return obj is OpCode && this.Equals( (OpCode) obj );
      }

      /// <inheritdoc/>
      public override Int32 GetHashCode()
      {
         return (Int32) this.OpCodeID * 31;
      }

      /// <summary>
      /// Checks whether this <see cref="OpCode"/> equals to the given <paramref name="code"/>-
      /// </summary>
      /// <param name="code">Another <see cref="OpCode"/>.</param>
      /// <returns><c>true</c> if this opcode has same <see cref="OpCodeID"/> as <paramref name="code"/>; <c>false</c> otherwise.</returns>
      public Boolean Equals( OpCode code )
      {
         return this.OpCodeID == code.OpCodeID;
      }

      /// <summary>
      /// Checks whether two <see cref="OpCode"/>s are equal.
      /// </summary>
      /// <param name="a">An <see cref="OpCode"/>.</param>
      /// <param name="b">Another <see cref="OpCode"/>.</param>
      /// <returns><c>true</c> if <paramref name="a"/> is equal to <paramref name="b"/>; <c>false</c> otherwise.</returns>
      public static Boolean operator ==( OpCode a, OpCode b )
      {
         return a.Equals( b );
      }

      /// <summary>
      /// Checks whether two <see cref="OpCode"/>s are not equal.
      /// </summary>
      /// <param name="a">An <see cref="OpCode"/>.</param>
      /// <param name="b">Another <see cref="OpCode"/>.</param>
      /// <returns><c>true</c> if <paramref name="a"/> is not equal to <paramref name="b"/>; <c>false</c> otherwise.</returns>
      public static Boolean operator !=( OpCode a, OpCode b )
      {
         return !( a == b );
      }
   }

   /// <summary>
   /// Enumeration containing all CIL op code values.
   /// </summary>
   public enum OpCodeID
   {
      /// <summary>
      /// <see cref="OpCodes.Nop"/>
      /// </summary>
      Nop,
      /// <summary>
      /// <see cref="OpCodes.Break"/>
      /// </summary>
      Break,
      /// <summary>
      /// <see cref="OpCodes.Ldarg_0"/>
      /// </summary>
      Ldarg_0,
      /// <summary>
      /// <see cref="OpCodes.Ldarg_1"/>
      /// </summary>
      Ldarg_1,
      /// <summary>
      /// <see cref="OpCodes.Ldarg_2"/>
      /// </summary>
      Ldarg_2,
      /// <summary>
      /// <see cref="OpCodes.Ldarg_3"/>
      /// </summary>
      Ldarg_3,
      /// <summary>
      /// <see cref="OpCodes.Ldloc_0"/>
      /// </summary>
      Ldloc_0,
      /// <summary>
      /// <see cref="OpCodes.Ldloc_1"/>
      /// </summary>
      Ldloc_1,
      /// <summary>
      /// <see cref="OpCodes.Ldloc_2"/>
      /// </summary>
      Ldloc_2,
      /// <summary>
      /// <see cref="OpCodes.Ldloc_3"/>
      /// </summary>
      Ldloc_3,
      /// <summary>
      /// <see cref="OpCodes.Stloc_0"/>
      /// </summary>
      Stloc_0,
      /// <summary>
      /// <see cref="OpCodes.Stloc_1"/>
      /// </summary>
      Stloc_1,
      /// <summary>
      /// <see cref="OpCodes.Stloc_2"/>
      /// </summary>
      Stloc_2,
      /// <summary>
      /// <see cref="OpCodes.Stloc_3"/>
      /// </summary>
      Stloc_3,
      /// <summary>
      /// <see cref="OpCodes.Ldarg_S"/>
      /// </summary>
      Ldarg_S,
      /// <summary>
      /// <see cref="OpCodes.Ldarga_S"/>
      /// </summary>
      Ldarga_S,
      /// <summary>
      /// <see cref="OpCodes.Starg_S"/>
      /// </summary>
      Starg_S,
      /// <summary>
      /// <see cref="OpCodes.Ldloc_S"/>
      /// </summary>
      Ldloc_S,
      /// <summary>
      /// <see cref="OpCodes.Ldloca_S"/>
      /// </summary>
      Ldloca_S,
      /// <summary>
      /// <see cref="OpCodes.Stloc_S"/>
      /// </summary>
      Stloc_S,
      /// <summary>
      /// <see cref="OpCodes.Ldnull"/>
      /// </summary>
      Ldnull,
      /// <summary>
      /// <see cref="OpCodes.Ldc_I4_M1"/>
      /// </summary>
      Ldc_I4_M1,
      /// <summary>
      /// <see cref="OpCodes.Ldc_I4_0"/>
      /// </summary>
      Ldc_I4_0,
      /// <summary>
      /// <see cref="OpCodes.Ldc_I4_1"/>
      /// </summary>
      Ldc_I4_1,
      /// <summary>
      /// <see cref="OpCodes.Ldc_I4_2"/>
      /// </summary>
      Ldc_I4_2,
      /// <summary>
      /// <see cref="OpCodes.Ldc_I4_3"/>
      /// </summary>
      Ldc_I4_3,
      /// <summary>
      /// <see cref="OpCodes.Ldc_I4_4"/>
      /// </summary>
      Ldc_I4_4,
      /// <summary>
      /// <see cref="OpCodes.Ldc_I4_5"/>
      /// </summary>
      Ldc_I4_5,
      /// <summary>
      /// <see cref="OpCodes.Ldc_I4_6"/>
      /// </summary>
      Ldc_I4_6,
      /// <summary>
      /// <see cref="OpCodes.Ldc_I4_7"/>
      /// </summary>
      Ldc_I4_7,
      /// <summary>
      /// <see cref="OpCodes.Ldc_I4_8"/>
      /// </summary>
      Ldc_I4_8,
      /// <summary>
      /// <see cref="OpCodes.Ldc_I4_S"/>
      /// </summary>
      Ldc_I4_S,
      /// <summary>
      /// <see cref="OpCodes.Ldc_I4"/>
      /// </summary>
      Ldc_I4,
      /// <summary>
      /// <see cref="OpCodes.Ldc_I8"/>
      /// </summary>
      Ldc_I8,
      /// <summary>
      /// <see cref="OpCodes.Ldc_R4"/>
      /// </summary>
      Ldc_R4,
      /// <summary>
      /// <see cref="OpCodes.Ldc_R8"/>
      /// </summary>
      Ldc_R8,
      /// <summary>
      /// <see cref="OpCodes.Dup"/>
      /// </summary>
      Dup = 0x25,
      /// <summary>
      /// <see cref="OpCodes.Pop"/>
      /// </summary>
      Pop,
      /// <summary>
      /// <see cref="OpCodes.Jmp"/>
      /// </summary>
      Jmp,
      /// <summary>
      /// <see cref="OpCodes.Call"/>
      /// </summary>
      Call,
      /// <summary>
      /// <see cref="OpCodes.Calli"/>
      /// </summary>
      Calli,
      /// <summary>
      /// <see cref="OpCodes.Ret"/>
      /// </summary>
      Ret,
      /// <summary>
      /// <see cref="OpCodes.Br_S"/>
      /// </summary>
      Br_S,
      /// <summary>
      /// <see cref="OpCodes.Brfalse_S"/>
      /// </summary>
      Brfalse_S,
      /// <summary>
      /// <see cref="OpCodes.Brtrue_S"/>
      /// </summary>
      Brtrue_S,
      /// <summary>
      /// <see cref="OpCodes.Beq_S"/>
      /// </summary>
      Beq_S,
      /// <summary>
      /// <see cref="OpCodes.Bge_S"/>
      /// </summary>
      Bge_S,
      /// <summary>
      /// <see cref="OpCodes.Bgt_S"/>
      /// </summary>
      Bgt_S,
      /// <summary>
      /// <see cref="OpCodes.Ble_S"/>
      /// </summary>
      Ble_S,
      /// <summary>
      /// <see cref="OpCodes.Blt_S"/>
      /// </summary>
      Blt_S,
      /// <summary>
      /// <see cref="OpCodes.Bne_Un_S"/>
      /// </summary>
      Bne_Un_S,
      /// <summary>
      /// <see cref="OpCodes.Bge_Un_S"/>
      /// </summary>
      Bge_Un_S,
      /// <summary>
      /// <see cref="OpCodes.Bgt_Un_S"/>
      /// </summary>
      Bgt_Un_S,
      /// <summary>
      /// <see cref="OpCodes.Ble_Un_S"/>
      /// </summary>
      Ble_Un_S,
      /// <summary>
      /// <see cref="OpCodes.Blt_Un_S"/>
      /// </summary>
      Blt_Un_S,
      /// <summary>
      /// <see cref="OpCodes.Br"/>
      /// </summary>
      Br,
      /// <summary>
      /// <see cref="OpCodes.Brfalse"/>
      /// </summary>
      Brfalse,
      /// <summary>
      /// <see cref="OpCodes.Brtrue"/>
      /// </summary>
      Brtrue,
      /// <summary>
      /// <see cref="OpCodes.Beq"/>
      /// </summary>
      Beq,
      /// <summary>
      /// <see cref="OpCodes.Bge"/>
      /// </summary>
      Bge,
      /// <summary>
      /// <see cref="OpCodes.Bgt"/>
      /// </summary>
      Bgt,
      /// <summary>
      /// <see cref="OpCodes.Ble"/>
      /// </summary>
      Ble,
      /// <summary>
      /// <see cref="OpCodes.Blt"/>
      /// </summary>
      Blt,
      /// <summary>
      /// <see cref="OpCodes.Bne_Un"/>
      /// </summary>
      Bne_Un,
      /// <summary>
      /// <see cref="OpCodes.Bge_Un"/>
      /// </summary>
      Bge_Un,
      /// <summary>
      /// <see cref="OpCodes.Bgt_Un"/>
      /// </summary>
      Bgt_Un,
      /// <summary>
      /// <see cref="OpCodes.Ble_Un"/>
      /// </summary>
      Ble_Un,
      /// <summary>
      /// <see cref="OpCodes.Blt_Un"/>
      /// </summary>
      Blt_Un,
      /// <summary>
      /// <see cref="OpCodes.Switch"/>
      /// </summary>
      Switch,
      /// <summary>
      /// <see cref="OpCodes.Ldind_I1"/>
      /// </summary>
      Ldind_I1,
      /// <summary>
      /// <see cref="OpCodes.Ldind_U1"/>
      /// </summary>
      Ldind_U1,
      /// <summary>
      /// <see cref="OpCodes.Ldind_I2"/>
      /// </summary>
      Ldind_I2,
      /// <summary>
      /// <see cref="OpCodes.Ldind_U2"/>
      /// </summary>
      Ldind_U2,
      /// <summary>
      /// <see cref="OpCodes.Ldind_I4"/>
      /// </summary>
      Ldind_I4,
      /// <summary>
      /// <see cref="OpCodes.Ldind_U4"/>
      /// </summary>
      Ldind_U4,
      /// <summary>
      /// <see cref="OpCodes.Ldind_I8"/>
      /// </summary>
      Ldind_I8,
      /// <summary>
      /// <see cref="OpCodes.Ldind_I"/>
      /// </summary>
      Ldind_I,
      /// <summary>
      /// <see cref="OpCodes.Ldind_R4"/>
      /// </summary>
      Ldind_R4,
      /// <summary>
      /// <see cref="OpCodes.Ldind_R8"/>
      /// </summary>
      Ldind_R8,
      /// <summary>
      /// <see cref="OpCodes.Ldind_Ref"/>
      /// </summary>
      Ldind_Ref,
      /// <summary>
      /// <see cref="OpCodes.Stind_Ref"/>
      /// </summary>
      Stind_Ref,
      /// <summary>
      /// <see cref="OpCodes.Stind_I1"/>
      /// </summary>
      Stind_I1,
      /// <summary>
      /// <see cref="OpCodes.Stind_I2"/>
      /// </summary>
      Stind_I2,
      /// <summary>
      /// <see cref="OpCodes.Stind_I4"/>
      /// </summary>
      Stind_I4,
      /// <summary>
      /// <see cref="OpCodes.Stind_I8"/>
      /// </summary>
      Stind_I8,
      /// <summary>
      /// <see cref="OpCodes.Stind_R4"/>
      /// </summary>
      Stind_R4,
      /// <summary>
      /// <see cref="OpCodes.Stind_R8"/>
      /// </summary>
      Stind_R8,
      /// <summary>
      /// <see cref="OpCodes.Add"/>
      /// </summary>
      Add,
      /// <summary>
      /// <see cref="OpCodes.Sub"/>
      /// </summary>
      Sub,
      /// <summary>
      /// <see cref="OpCodes.Mul"/>
      /// </summary>
      Mul,
      /// <summary>
      /// <see cref="OpCodes.Div"/>
      /// </summary>
      Div,
      /// <summary>
      /// <see cref="OpCodes.Div_Un"/>
      /// </summary>
      Div_Un,
      /// <summary>
      /// <see cref="OpCodes.Rem"/>
      /// </summary>
      Rem,
      /// <summary>
      /// <see cref="OpCodes.Rem_Un"/>
      /// </summary>
      Rem_Un,
      /// <summary>
      /// <see cref="OpCodes.And"/>
      /// </summary>
      And,
      /// <summary>
      /// <see cref="OpCodes.Or"/>
      /// </summary>
      Or,
      /// <summary>
      /// <see cref="OpCodes.Xor"/>
      /// </summary>
      Xor,
      /// <summary>
      /// <see cref="OpCodes.Shl"/>
      /// </summary>
      Shl,
      /// <summary>
      /// <see cref="OpCodes.Shr"/>
      /// </summary>
      Shr,
      /// <summary>
      /// <see cref="OpCodes.Shr_Un"/>
      /// </summary>
      Shr_Un,
      /// <summary>
      /// <see cref="OpCodes.Neg"/>
      /// </summary>
      Neg,
      /// <summary>
      /// <see cref="OpCodes.Not"/>
      /// </summary>
      Not,
      /// <summary>
      /// <see cref="OpCodes.Conv_I1"/>
      /// </summary>
      Conv_I1,
      /// <summary>
      /// <see cref="OpCodes.Conv_I2"/>
      /// </summary>
      Conv_I2,
      /// <summary>
      /// <see cref="OpCodes.Conv_I4"/>
      /// </summary>
      Conv_I4,
      /// <summary>
      /// <see cref="OpCodes.Conv_I8"/>
      /// </summary>
      Conv_I8,
      /// <summary>
      /// <see cref="OpCodes.Conv_R4"/>
      /// </summary>
      Conv_R4,
      /// <summary>
      /// <see cref="OpCodes.Conv_R8"/>
      /// </summary>
      Conv_R8,
      /// <summary>
      /// <see cref="OpCodes.Conv_U4"/>
      /// </summary>
      Conv_U4,
      /// <summary>
      /// <see cref="OpCodes.Conv_U8"/>
      /// </summary>
      Conv_U8,
      /// <summary>
      /// <see cref="OpCodes.Callvirt"/>
      /// </summary>
      Callvirt,
      /// <summary>
      /// <see cref="OpCodes.Cpobj"/>
      /// </summary>
      Cpobj,
      /// <summary>
      /// <see cref="OpCodes.Ldobj"/>
      /// </summary>
      Ldobj,
      /// <summary>
      /// <see cref="OpCodes.Ldstr"/>
      /// </summary>
      Ldstr,
      /// <summary>
      /// <see cref="OpCodes.Newobj"/>
      /// </summary>
      Newobj,
      /// <summary>
      /// <see cref="OpCodes.Castclass"/>
      /// </summary>
      Castclass,
      /// <summary>
      /// <see cref="OpCodes.Isinst"/>
      /// </summary>
      Isinst,
      /// <summary>
      /// <see cref="OpCodes.Conv_R_Un"/>
      /// </summary>
      Conv_R_Un,
      /// <summary>
      /// <see cref="OpCodes.Unbox"/>
      /// </summary>
      Unbox = 0x79,
      /// <summary>
      /// <see cref="OpCodes.Throw"/>
      /// </summary>
      Throw,
      /// <summary>
      /// <see cref="OpCodes.Ldfld"/>
      /// </summary>
      Ldfld,
      /// <summary>
      /// <see cref="OpCodes.Ldflda"/>
      /// </summary>
      Ldflda,
      /// <summary>
      /// <see cref="OpCodes.Stfld"/>
      /// </summary>
      Stfld,
      /// <summary>
      /// <see cref="OpCodes.Ldsfld"/>
      /// </summary>
      Ldsfld,
      /// <summary>
      /// <see cref="OpCodes.Ldsflda"/>
      /// </summary>
      Ldsflda,
      /// <summary>
      /// <see cref="OpCodes.Stsfld"/>
      /// </summary>
      Stsfld,
      /// <summary>
      /// <see cref="OpCodes.Stobj"/>
      /// </summary>
      Stobj,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_I1_Un"/>
      /// </summary>
      Conv_Ovf_I1_Un,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_I2_Un"/>
      /// </summary>
      Conv_Ovf_I2_Un,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_I4_Un"/>
      /// </summary>
      Conv_Ovf_I4_Un,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_I8_Un"/>
      /// </summary>
      Conv_Ovf_I8_Un,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_U1_Un"/>
      /// </summary>
      Conv_Ovf_U1_Un,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_U2_Un"/>
      /// </summary>
      Conv_Ovf_U2_Un,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_U4_Un"/>
      /// </summary>
      Conv_Ovf_U4_Un,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_U8_Un"/>
      /// </summary>
      Conv_Ovf_U8_Un,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_I_Un"/>
      /// </summary>
      Conv_Ovf_I_Un,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_U_Un"/>
      /// </summary>
      Conv_Ovf_U_Un,
      /// <summary>
      /// <see cref="OpCodes.Box"/>
      /// </summary>
      Box,
      /// <summary>
      /// <see cref="OpCodes.Newarr"/>
      /// </summary>
      Newarr,
      /// <summary>
      /// <see cref="OpCodes.Ldlen"/>
      /// </summary>
      Ldlen,
      /// <summary>
      /// <see cref="OpCodes.Ldelema"/>
      /// </summary>
      Ldelema,
      /// <summary>
      /// <see cref="OpCodes.Ldelem_I1"/>
      /// </summary>
      Ldelem_I1,
      /// <summary>
      /// <see cref="OpCodes.Ldelem_U1"/>
      /// </summary>
      Ldelem_U1,
      /// <summary>
      /// <see cref="OpCodes.Ldelem_I2"/>
      /// </summary>
      Ldelem_I2,
      /// <summary>
      /// <see cref="OpCodes.Ldelem_U2"/>
      /// </summary>
      Ldelem_U2,
      /// <summary>
      /// <see cref="OpCodes.Ldelem_I4"/>
      /// </summary>
      Ldelem_I4,
      /// <summary>
      /// <see cref="OpCodes.Ldelem_U4"/>
      /// </summary>
      Ldelem_U4,
      /// <summary>
      /// <see cref="OpCodes.Ldelem_I8"/>
      /// </summary>
      Ldelem_I8,
      /// <summary>
      /// <see cref="OpCodes.Ldelem_I"/>
      /// </summary>
      Ldelem_I,
      /// <summary>
      /// <see cref="OpCodes.Ldelem_R4"/>
      /// </summary>
      Ldelem_R4,
      /// <summary>
      /// <see cref="OpCodes.Ldelem_R8"/>
      /// </summary>
      Ldelem_R8,
      /// <summary>
      /// <see cref="OpCodes.Ldelem_Ref"/>
      /// </summary>
      Ldelem_Ref,
      /// <summary>
      /// <see cref="OpCodes.Stelem_I"/>
      /// </summary>
      Stelem_I,
      /// <summary>
      /// <see cref="OpCodes.Stelem_I1"/>
      /// </summary>
      Stelem_I1,
      /// <summary>
      /// <see cref="OpCodes.Stelem_I2"/>
      /// </summary>
      Stelem_I2,
      /// <summary>
      /// <see cref="OpCodes.Stelem_I4"/>
      /// </summary>
      Stelem_I4,
      /// <summary>
      /// <see cref="OpCodes.Stelem_I8"/>
      /// </summary>
      Stelem_I8,
      /// <summary>
      /// <see cref="OpCodes.Stelem_R4"/>
      /// </summary>
      Stelem_R4,
      /// <summary>
      /// <see cref="OpCodes.Stelem_R8"/>
      /// </summary>
      Stelem_R8,
      /// <summary>
      /// <see cref="OpCodes.Stelem_Ref"/>
      /// </summary>
      Stelem_Ref,
      /// <summary>
      /// <see cref="OpCodes.Ldelem"/>
      /// </summary>
      Ldelem,
      /// <summary>
      /// <see cref="OpCodes.Stelem"/>
      /// </summary>
      Stelem,
      /// <summary>
      /// <see cref="OpCodes.Unbox_Any"/>
      /// </summary>
      Unbox_Any,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_I1"/>
      /// </summary>
      Conv_Ovf_I1 = 0xB3,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_U1"/>
      /// </summary>
      Conv_Ovf_U1,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_I2"/>
      /// </summary>
      Conv_Ovf_I2,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_U2"/>
      /// </summary>
      Conv_Ovf_U2,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_I4"/>
      /// </summary>
      Conv_Ovf_I4,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_U4"/>
      /// </summary>
      Conv_Ovf_U4,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_I8"/>
      /// </summary>
      Conv_Ovf_I8,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_U8"/>
      /// </summary>
      Conv_Ovf_U8,
      /// <summary>
      /// <see cref="OpCodes.Refanyval"/>
      /// </summary>
      Refanyval = 0xC2,
      /// <summary>
      /// <see cref="OpCodes.Ckfinite"/>
      /// </summary>
      Ckfinite,
      /// <summary>
      /// <see cref="OpCodes.Mkrefany"/>
      /// </summary>
      Mkrefany = 0xC6,
      /// <summary>
      /// <see cref="OpCodes.Ldtoken"/>
      /// </summary>
      Ldtoken = 0xD0,
      /// <summary>
      /// <see cref="OpCodes.Conv_U2"/>
      /// </summary>
      Conv_U2,
      /// <summary>
      /// <see cref="OpCodes.Conv_U1"/>
      /// </summary>
      Conv_U1,
      /// <summary>
      /// <see cref="OpCodes.Conv_I"/>
      /// </summary>
      Conv_I,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_I"/>
      /// </summary>
      Conv_Ovf_I,
      /// <summary>
      /// <see cref="OpCodes.Conv_Ovf_U"/>
      /// </summary>
      Conv_Ovf_U,
      /// <summary>
      /// <see cref="OpCodes.Add_Ovf"/>
      /// </summary>
      Add_Ovf,
      /// <summary>
      /// <see cref="OpCodes.Add_Ovf_Un"/>
      /// </summary>
      Add_Ovf_Un,
      /// <summary>
      /// <see cref="OpCodes.Mul_Ovf"/>
      /// </summary>
      Mul_Ovf,
      /// <summary>
      /// <see cref="OpCodes.Mul_Ovf_Un"/>
      /// </summary>
      Mul_Ovf_Un,
      /// <summary>
      /// <see cref="OpCodes.Sub_Ovf"/>
      /// </summary>
      Sub_Ovf,
      /// <summary>
      /// <see cref="OpCodes.Sub_Ovf_Un"/>
      /// </summary>
      Sub_Ovf_Un,
      /// <summary>
      /// <see cref="OpCodes.Endfinally"/>
      /// </summary>
      Endfinally,
      /// <summary>
      /// <see cref="OpCodes.Leave"/>
      /// </summary>
      Leave,
      /// <summary>
      /// <see cref="OpCodes.Leave_S"/>
      /// </summary>
      Leave_S,
      /// <summary>
      /// <see cref="OpCodes.Stind_I"/>
      /// </summary>
      Stind_I,
      /// <summary>
      /// <see cref="OpCodes.Conv_U"/>
      /// </summary>
      Conv_U,
      /// <summary>
      /// <see cref="OpCodes.Arglist"/>
      /// </summary>
      Arglist = 0xFE00,
      /// <summary>
      /// <see cref="OpCodes.Ceq"/>
      /// </summary>
      Ceq,
      /// <summary>
      /// <see cref="OpCodes.Cgt"/>
      /// </summary>
      Cgt,
      /// <summary>
      /// <see cref="OpCodes.Cgt_Un"/>
      /// </summary>
      Cgt_Un,
      /// <summary>
      /// <see cref="OpCodes.Clt"/>
      /// </summary>
      Clt,
      /// <summary>
      /// <see cref="OpCodes.Clt_Un"/>
      /// </summary>
      Clt_Un,
      /// <summary>
      /// <see cref="OpCodes.Ldftn"/>
      /// </summary>
      Ldftn,
      /// <summary>
      /// <see cref="OpCodes.Ldvirtftn"/>
      /// </summary>
      Ldvirtftn,
      /// <summary>
      /// <see cref="OpCodes.Ldarg"/>
      /// </summary>
      Ldarg = 0xFE09,
      /// <summary>
      /// <see cref="OpCodes.Ldarga"/>
      /// </summary>
      Ldarga,
      /// <summary>
      /// <see cref="OpCodes.Starg"/>
      /// </summary>
      Starg,
      /// <summary>
      /// <see cref="OpCodes.Ldloc"/>
      /// </summary>
      Ldloc,
      /// <summary>
      /// <see cref="OpCodes.Ldloca"/>
      /// </summary>
      Ldloca,
      /// <summary>
      /// <see cref="OpCodes.Stloc"/>
      /// </summary>
      Stloc,
      /// <summary>
      /// <see cref="OpCodes.Localloc"/>
      /// </summary>
      Localloc,
      /// <summary>
      /// <see cref="OpCodes.Endfilter"/>
      /// </summary>
      Endfilter = 0xFE11,
      /// <summary>
      /// <see cref="OpCodes.Unaligned_"/>
      /// </summary>
      Unaligned_,
      /// <summary>
      /// <see cref="OpCodes.Volatile_"/>
      /// </summary>
      Volatile_,
      /// <summary>
      /// <see cref="OpCodes.Tail_"/>
      /// </summary>
      Tail_,
      /// <summary>
      /// <see cref="OpCodes.Initobj"/>
      /// </summary>
      Initobj,
      /// <summary>
      /// <see cref="OpCodes.Constrained_"/>
      /// </summary>
      Constrained_,
      /// <summary>
      /// <see cref="OpCodes.Cpblk"/>
      /// </summary>
      Cpblk,
      /// <summary>
      /// <see cref="OpCodes.Initblk"/>
      /// </summary>
      Initblk,
      /// <summary>
      /// <see cref="OpCodes.Rethrow"/>
      /// </summary>
      Rethrow = 0xFE1A,
      /// <summary>
      /// <see cref="OpCodes.Sizeof"/>
      /// </summary>
      Sizeof = 0xFE1C,
      /// <summary>
      /// <see cref="OpCodes.Refanytype"/>
      /// </summary>
      Refanytype,
      /// <summary>
      /// <see cref="OpCodes.Readonly_"/>
      /// </summary>
      Readonly_,
   }

   /// <summary>
   /// Contains all possible values for flow control of CIL op codes.
   /// </summary>
   public enum FlowControl : byte
   {
      /// <summary>Branch instruction.</summary>
      Branch,
      /// <summary>Break instruction.</summary>
      Break,
      /// <summary>Call instruction.</summary>
      Call,
      /// <summary>Conditional branch instruction.</summary>
      Cond_Branch,
      /// <summary>Provides information about a subsequent instruction. For example, the Unaligned instruction of Reflection.Emit.Opcodes has FlowControl.Meta and specifies that the subsequent pointer instruction might be unaligned.</summary>
      Meta,
      /// <summary>Normal flow of control.</summary>
      Next,
      /// <summary>This enumerator value is reserved and should not be used.</summary>
      Phi,
      /// <summary>Return instruction.</summary>
      Return,
      /// <summary>Exception throw instruction.</summary>
      Throw
   }

   /// <summary>
   /// Contains all values for meta-information about CIL op codes.
   /// </summary>
   public enum OpCodeType : byte
   {
      /// <summary>This enumerator value is reserved and should not be used.</summary>
      Annotation,
      /// <summary>These are Microsoft intermediate language (MSIL) instructions that are used as a synonym for other MSIL instructions. For example, ldarg.0 represents the ldarg instruction with an argument of 0.</summary>
      Macro,
      /// <summary>Describes a reserved Microsoft intermediate language (MSIL) instruction.</summary>
      Internal,
      /// <summary>Describes a Microsoft intermediate language (MSIL) instruction that applies to objects.</summary>
      Objmodel,
      /// <summary>Describes a prefix instruction that modifies the behavior of the following instruction.</summary>
      Prefix,
      /// <summary>Describes a built-in instruction.</summary>
      Primitive
   }

   /// <summary>
   /// Contains all values for operand types of CIL op codes.
   /// </summary>
   public enum OperandType : byte
   {
      /// <summary>The operand is a 32-bit integer branch target.</summary>
      InlineBrTarget,
      /// <summary>The operand is a 32-bit metadata token.</summary>
      InlineField,
      /// <summary>The operand is a 32-bit integer.</summary>
      InlineI,
      /// <summary>The operand is a 64-bit integer.</summary>
      InlineI8,
      /// <summary>The operand is a 32-bit metadata token.</summary>
      InlineMethod,
      /// <summary>No operand.</summary>
      InlineNone,
      /// <summary>The operand is a 64-bit IEEE floating point number.</summary>
      InlineR,
      /// <summary>The operand is a 32-bit metadata signature token.</summary>
      InlineSignature,
      /// <summary>The operand is a 32-bit metadata string token.</summary>
      InlineString,
      /// <summary>The operand is the 32-bit integer argument to a switch instruction.</summary>
      InlineSwitch,
      /// <summary>The operand is a FieldRef, MethodRef, or TypeRef token.</summary>
      InlineToken,
      /// <summary>The operand is a 32-bit metadata token.</summary>
      InlineType,
      /// <summary>The operand is 16-bit integer containing the ordinal of a local variable or an argument.</summary>
      InlineVar,
      /// <summary>The operand is an 8-bit integer branch target.</summary>
      ShortInlineBrTarget,
      /// <summary>The operand is an 8-bit integer.</summary>
      ShortInlineI,
      /// <summary>The operand is a 32-bit IEEE floating point number.</summary>
      ShortInlineR,
      /// <summary>The operand is an 8-bit integer containing the ordinal of a local variable or an argumenta.</summary>
      ShortInlineVar
   }

   /// <summary>
   /// Contains all possible values for stack pushing behaviour of CIL op codes.
   /// </summary>
   public enum StackBehaviourPush : byte
   {
      /// <summary>No values are pushed onto the stack.</summary>
      Push0,
      /// <summary>Pushes one value onto the stack.</summary>
      Push1,
      /// <summary>Pushes 1 value onto the stack for the first operand, and 1 value onto the stack for the second operand.</summary>
      Push1_push1,
      /// <summary>Pushes a 32-bit integer onto the stack.</summary>
      Pushi,
      /// <summary>Pushes a 64-bit integer onto the stack.</summary>
      Pushi8,
      /// <summary>Pushes a 32-bit floating point number onto the stack.</summary>
      Pushr4,
      /// <summary>Pushes a 64-bit floating point number onto the stack.</summary>
      Pushr8,
      /// <summary>Pushes a reference onto the stack.</summary>
      Pushref,
      /// <summary>Pushes a variable onto the stack.</summary>
      Varpush,
   }

   // TODO this enum should not be present anymore in this library
   // OpCodeInfoProvider should have something like ArrayQuery<StackValueKind> GetPopBehaviour(OpCodeID id)
   // and
   // ArrayQuery<StackValueKind> GetPushBehaviour(OpCodeID id)
   // where StackValueKind would be something like
   // I4
   // I8
   // R4
   // R8
   // Value
   // Ref
   // Var

   /// <summary>
   /// Contains all possible values for stack popping behaviour of CIL op codes.
   /// </summary>
   public enum StackBehaviourPop : byte
   {
      /// <summary>No values are popped off the stack.</summary>
      Pop0,
      /// <summary>Pops one value off the stack.</summary>
      Pop1,
      /// <summary>Pops 1 value off the stack for the first operand, and 1 value of the stack for the second operand.</summary>
      Pop1_pop1,
      /// <summary>Pops a 32-bit integer off the stack.</summary>
      Popi,
      /// <summary>Pops a 32-bit integer off the stack for the first operand, and a value off the stack for the second operand.</summary>
      Popi_pop1,
      /// <summary>Pops a 32-bit integer off the stack for the first operand, and a 32-bit integer off the stack for the second operand.</summary>
      Popi_popi,
      /// <summary>Pops a 32-bit integer off the stack for the first operand, and a 64-bit integer off the stack for the second operand.</summary>
      Popi_popi8,
      /// <summary>Pops a 32-bit integer off the stack for the first operand, a 32-bit integer off the stack for the second operand, and a 32-bit integer off the stack for the third operand.</summary>
      Popi_popi_popi,
      /// <summary>Pops a 32-bit integer off the stack for the first operand, and a 32-bit floating point number off the stack for the second operand.</summary>
      Popi_popr4,
      /// <summary>Pops a 32-bit integer off the stack for the first operand, and a 64-bit floating point number off the stack for the second operand.</summary>
      Popi_popr8,
      /// <summary>Pops a reference off the stack.</summary>
      Popref,
      /// <summary>Pops a reference off the stack for the first operand, and a value off the stack for the second operand.</summary>
      Popref_pop1,
      /// <summary>Pops a reference off the stack for the first operand, and a 32-bit integer off the stack for the second operand.</summary>
      Popref_popi,
      /// <summary>Pops a reference off the stack for the first operand, a value off the stack for the second operand, and a value off the stack for the third operand.</summary>
      Popref_popi_popi,
      /// <summary>Pops a reference off the stack for the first operand, a value off the stack for the second operand, and a 64-bit integer off the stack for the third operand.</summary>
      Popref_popi_popi8,
      /// <summary>Pops a reference off the stack for the first operand, a value off the stack for the second operand, and a 32-bit integer off the stack for the third operand.</summary>
      Popref_popi_popr4,
      /// <summary>Pops a reference off the stack for the first operand, a value off the stack for the second operand, and a 64-bit floating point number off the stack for the third operand.</summary>
      Popref_popi_popr8,
      /// <summary>Pops a reference off the stack for the first operand, a value off the stack for the second operand, and a reference off the stack for the third operand.</summary>
      Popref_popi_popref,
      /// <summary>Pops a variable off the stack.</summary>
      Varpop,
      /// <summary>Pops a reference off the stack for the first operand, a value off the stack for the second operand, and a 32-bit integer off the stack for the third operand.</summary>
      Popref_popi_pop1
   }
}

public static partial class E_CILPhysical
{
   /// <summary>
   /// Calculates the total fixed byte count for a specific <see cref="OpCode"/>.
   /// This is the sum of <see cref="OpCode.Size"/> and <see cref="OpCode.OperandSize"/>.
   /// </summary>
   /// <param name="code">The <see cref="OpCode"/>.</param>
   /// <returns>The total fixed byte count for a specific <see cref="OpCode"/>.</returns>
   /// <remarks>
   /// One should use <see cref="GetTotalByteCount"/> extension method when calculating byte sizes when writing or reading IL bytecode.
   /// This is because switch instruction (<see cref="OpCodeID.Switch"/>) has additional offset array, the length of which is determined by the fixed operand of switch instruction.
   /// </remarks>
   public static Int32 GetFixedByteCount( this OpCode code )
   {
      return code.Size + code.OperandSize;
   }


   private const Int32 MAX_ONE_BYTE_INSTRUCTION = 0xFE;

}