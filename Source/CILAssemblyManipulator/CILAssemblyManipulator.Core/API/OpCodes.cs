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
using CILAssemblyManipulator.Implementation.Physical;

namespace CILAssemblyManipulator.API
{
   /// <summary>
   /// Container for instances of <see cref="OpCode"/>s. In public scope, identical to <see cref="T:System.Reflection.Emit.OpCodes"/> class.
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

      internal static IDictionary<OpCodeEncoding, OpCode> Codes;
      internal static IDictionary<OpCodeEncoding, OpCodeInfo> CodeInfosWithNoOperand;

      static OpCodes()
      {
         Codes = new Dictionary<OpCodeEncoding, OpCode>();
         CodeInfosWithNoOperand = new Dictionary<OpCodeEncoding, OpCodeInfo>();

         Nop = new OpCode( OpCodeEncoding.Nop, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Break = new OpCode( OpCodeEncoding.Break, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Break, false );
         Ldarg_0 = new OpCode( OpCodeEncoding.Ldarg_0, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldarg_1 = new OpCode( OpCodeEncoding.Ldarg_1, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldarg_2 = new OpCode( OpCodeEncoding.Ldarg_2, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldarg_3 = new OpCode( OpCodeEncoding.Ldarg_3, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldloc_0 = new OpCode( OpCodeEncoding.Ldloc_0, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldloc_1 = new OpCode( OpCodeEncoding.Ldloc_1, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldloc_2 = new OpCode( OpCodeEncoding.Ldloc_2, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldloc_3 = new OpCode( OpCodeEncoding.Ldloc_3, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Stloc_0 = new OpCode( OpCodeEncoding.Stloc_0, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Stloc_1 = new OpCode( OpCodeEncoding.Stloc_1, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Stloc_2 = new OpCode( OpCodeEncoding.Stloc_2, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Stloc_3 = new OpCode( OpCodeEncoding.Stloc_3, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldarg_S = new OpCode( OpCodeEncoding.Ldarg_S, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.ShortInlineVar, OpCodeType.Macro, FlowControl.Next, false );
         Ldarga_S = new OpCode( OpCodeEncoding.Ldarga_S, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.ShortInlineVar, OpCodeType.Macro, FlowControl.Next, false );
         Starg_S = new OpCode( OpCodeEncoding.Starg_S, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.ShortInlineVar, OpCodeType.Macro, FlowControl.Next, false );
         Ldloc_S = new OpCode( OpCodeEncoding.Ldloc_S, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.ShortInlineVar, OpCodeType.Macro, FlowControl.Next, false );
         Ldloca_S = new OpCode( OpCodeEncoding.Ldloca_S, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.ShortInlineVar, OpCodeType.Macro, FlowControl.Next, false );
         Stloc_S = new OpCode( OpCodeEncoding.Stloc_S, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.ShortInlineVar, OpCodeType.Macro, FlowControl.Next, false );
         Ldnull = new OpCode( OpCodeEncoding.Ldnull, StackBehaviourPop.Pop0, StackBehaviourPush.Pushref, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldc_I4_M1 = new OpCode( OpCodeEncoding.Ldc_I4_M1, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_0 = new OpCode( OpCodeEncoding.Ldc_I4_0, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_1 = new OpCode( OpCodeEncoding.Ldc_I4_1, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_2 = new OpCode( OpCodeEncoding.Ldc_I4_2, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_3 = new OpCode( OpCodeEncoding.Ldc_I4_3, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_4 = new OpCode( OpCodeEncoding.Ldc_I4_4, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_5 = new OpCode( OpCodeEncoding.Ldc_I4_5, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_6 = new OpCode( OpCodeEncoding.Ldc_I4_6, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_7 = new OpCode( OpCodeEncoding.Ldc_I4_7, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_8 = new OpCode( OpCodeEncoding.Ldc_I4_8, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4_S = new OpCode( OpCodeEncoding.Ldc_I4_S, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.ShortInlineI, OpCodeType.Macro, FlowControl.Next, false );
         Ldc_I4 = new OpCode( OpCodeEncoding.Ldc_I4, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineI, OpCodeType.Primitive, FlowControl.Next, false );
         Ldc_I8 = new OpCode( OpCodeEncoding.Ldc_I8, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi8, OperandType.InlineI8, OpCodeType.Primitive, FlowControl.Next, false );
         Ldc_R4 = new OpCode( OpCodeEncoding.Ldc_R4, StackBehaviourPop.Pop0, StackBehaviourPush.Pushr4, OperandType.ShortInlineR, OpCodeType.Primitive, FlowControl.Next, false );
         Ldc_R8 = new OpCode( OpCodeEncoding.Ldc_R8, StackBehaviourPop.Pop0, StackBehaviourPush.Pushr8, OperandType.InlineR, OpCodeType.Primitive, FlowControl.Next, false );
         Dup = new OpCode( OpCodeEncoding.Dup, StackBehaviourPop.Pop1, StackBehaviourPush.Push1_push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Pop = new OpCode( OpCodeEncoding.Pop, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Jmp = new OpCode( OpCodeEncoding.Jmp, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineMethod, OpCodeType.Primitive, FlowControl.Call, true );
         Call = new OpCode( OpCodeEncoding.Call, StackBehaviourPop.Varpop, StackBehaviourPush.Varpush, OperandType.InlineMethod, OpCodeType.Primitive, FlowControl.Call, false );
         Calli = new OpCode( OpCodeEncoding.Calli, StackBehaviourPop.Varpop, StackBehaviourPush.Varpush, OperandType.InlineSig, OpCodeType.Primitive, FlowControl.Call, false );
         Ret = new OpCode( OpCodeEncoding.Ret, StackBehaviourPop.Varpop, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Return, true );
         Br_S = new OpCode( OpCodeEncoding.Br_S, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Branch, true );
         Brfalse_S = new OpCode( OpCodeEncoding.Brfalse_S, StackBehaviourPop.Popi, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Brtrue_S = new OpCode( OpCodeEncoding.Brtrue_S, StackBehaviourPop.Popi, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Beq_S = new OpCode( OpCodeEncoding.Beq_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Bge_S = new OpCode( OpCodeEncoding.Bge_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Bgt_S = new OpCode( OpCodeEncoding.Bgt_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Ble_S = new OpCode( OpCodeEncoding.Ble_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Blt_S = new OpCode( OpCodeEncoding.Blt_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Bne_Un_S = new OpCode( OpCodeEncoding.Bne_Un_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Bge_Un_S = new OpCode( OpCodeEncoding.Bge_Un_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Bgt_Un_S = new OpCode( OpCodeEncoding.Bgt_Un_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Ble_Un_S = new OpCode( OpCodeEncoding.Ble_Un_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Blt_Un_S = new OpCode( OpCodeEncoding.Blt_Un_S, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Br = new OpCode( OpCodeEncoding.Br, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Primitive, FlowControl.Branch, true );
         Brfalse = new OpCode( OpCodeEncoding.Brfalse, StackBehaviourPop.Popi, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Primitive, FlowControl.Cond_Branch, false );
         Brtrue = new OpCode( OpCodeEncoding.Brtrue, StackBehaviourPop.Popi, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Primitive, FlowControl.Cond_Branch, false );
         Beq = new OpCode( OpCodeEncoding.Beq, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Bge = new OpCode( OpCodeEncoding.Bge, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Bgt = new OpCode( OpCodeEncoding.Bgt, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Ble = new OpCode( OpCodeEncoding.Ble, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Blt = new OpCode( OpCodeEncoding.Blt, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Bne_Un = new OpCode( OpCodeEncoding.Bne_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Bge_Un = new OpCode( OpCodeEncoding.Bge_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Bgt_Un = new OpCode( OpCodeEncoding.Bgt_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Ble_Un = new OpCode( OpCodeEncoding.Ble_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Blt_Un = new OpCode( OpCodeEncoding.Blt_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, false );
         Switch = new OpCode( OpCodeEncoding.Switch, StackBehaviourPop.Popi, StackBehaviourPush.Push0, OperandType.InlineSwitch, OpCodeType.Primitive, FlowControl.Cond_Branch, false );
         Ldind_I1 = new OpCode( OpCodeEncoding.Ldind_I1, StackBehaviourPop.Popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_U1 = new OpCode( OpCodeEncoding.Ldind_U1, StackBehaviourPop.Popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_I2 = new OpCode( OpCodeEncoding.Ldind_I2, StackBehaviourPop.Popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_U2 = new OpCode( OpCodeEncoding.Ldind_U2, StackBehaviourPop.Popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_I4 = new OpCode( OpCodeEncoding.Ldind_I4, StackBehaviourPop.Popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_U4 = new OpCode( OpCodeEncoding.Ldind_U4, StackBehaviourPop.Popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_I8 = new OpCode( OpCodeEncoding.Ldind_I8, StackBehaviourPop.Popi, StackBehaviourPush.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_I = new OpCode( OpCodeEncoding.Ldind_I, StackBehaviourPop.Popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_R4 = new OpCode( OpCodeEncoding.Ldind_R4, StackBehaviourPop.Popi, StackBehaviourPush.Pushr4, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_R8 = new OpCode( OpCodeEncoding.Ldind_R8, StackBehaviourPop.Popi, StackBehaviourPush.Pushr8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldind_Ref = new OpCode( OpCodeEncoding.Ldind_Ref, StackBehaviourPop.Popi, StackBehaviourPush.Pushref, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Stind_Ref = new OpCode( OpCodeEncoding.Stind_Ref, StackBehaviourPop.Popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Stind_I1 = new OpCode( OpCodeEncoding.Stind_I1, StackBehaviourPop.Popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Stind_I2 = new OpCode( OpCodeEncoding.Stind_I2, StackBehaviourPop.Popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Stind_I4 = new OpCode( OpCodeEncoding.Stind_I4, StackBehaviourPop.Popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Stind_I8 = new OpCode( OpCodeEncoding.Stind_I8, StackBehaviourPop.Popi_popi8, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Stind_R4 = new OpCode( OpCodeEncoding.Stind_R4, StackBehaviourPop.Popi_popr4, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Stind_R8 = new OpCode( OpCodeEncoding.Stind_R8, StackBehaviourPop.Popi_popr8, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Add = new OpCode( OpCodeEncoding.Add, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Sub = new OpCode( OpCodeEncoding.Sub, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Mul = new OpCode( OpCodeEncoding.Mul, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Div = new OpCode( OpCodeEncoding.Div, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Div_Un = new OpCode( OpCodeEncoding.Div_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Rem = new OpCode( OpCodeEncoding.Rem, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Rem_Un = new OpCode( OpCodeEncoding.Rem_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         And = new OpCode( OpCodeEncoding.And, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Or = new OpCode( OpCodeEncoding.Or, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Xor = new OpCode( OpCodeEncoding.Xor, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Shl = new OpCode( OpCodeEncoding.Shl, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Shr = new OpCode( OpCodeEncoding.Shr, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Shr_Un = new OpCode( OpCodeEncoding.Shr_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Neg = new OpCode( OpCodeEncoding.Neg, StackBehaviourPop.Pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Not = new OpCode( OpCodeEncoding.Not, StackBehaviourPop.Pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_I1 = new OpCode( OpCodeEncoding.Conv_I1, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_I2 = new OpCode( OpCodeEncoding.Conv_I2, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_I4 = new OpCode( OpCodeEncoding.Conv_I4, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_I8 = new OpCode( OpCodeEncoding.Conv_I8, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_R4 = new OpCode( OpCodeEncoding.Conv_R4, StackBehaviourPop.Pop1, StackBehaviourPush.Pushr4, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_R8 = new OpCode( OpCodeEncoding.Conv_R8, StackBehaviourPop.Pop1, StackBehaviourPush.Pushr8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_U4 = new OpCode( OpCodeEncoding.Conv_U4, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_U8 = new OpCode( OpCodeEncoding.Conv_U8, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Callvirt = new OpCode( OpCodeEncoding.Callvirt, StackBehaviourPop.Varpop, StackBehaviourPush.Varpush, OperandType.InlineMethod, OpCodeType.Objmodel, FlowControl.Call, false );
         Cpobj = new OpCode( OpCodeEncoding.Cpobj, StackBehaviourPop.Popi_popi, StackBehaviourPush.Push0, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldobj = new OpCode( OpCodeEncoding.Ldobj, StackBehaviourPop.Popi, StackBehaviourPush.Push1, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldstr = new OpCode( OpCodeEncoding.Ldstr, StackBehaviourPop.Pop0, StackBehaviourPush.Pushref, OperandType.InlineString, OpCodeType.Objmodel, FlowControl.Next, false );
         Newobj = new OpCode( OpCodeEncoding.Newobj, StackBehaviourPop.Varpop, StackBehaviourPush.Pushref, OperandType.InlineMethod, OpCodeType.Objmodel, FlowControl.Call, false );
         Castclass = new OpCode( OpCodeEncoding.Castclass, StackBehaviourPop.Popref, StackBehaviourPush.Pushref, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Isinst = new OpCode( OpCodeEncoding.Isinst, StackBehaviourPop.Popref, StackBehaviourPush.Pushi, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Conv_R_Un = new OpCode( OpCodeEncoding.Conv_R_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushr8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Unbox = new OpCode( OpCodeEncoding.Unbox, StackBehaviourPop.Popref, StackBehaviourPush.Pushi, OperandType.InlineType, OpCodeType.Primitive, FlowControl.Next, false );
         Throw = new OpCode( OpCodeEncoding.Throw, StackBehaviourPop.Popref, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Throw, true );
         Ldfld = new OpCode( OpCodeEncoding.Ldfld, StackBehaviourPop.Popref, StackBehaviourPush.Push1, OperandType.InlineField, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldflda = new OpCode( OpCodeEncoding.Ldflda, StackBehaviourPop.Popref, StackBehaviourPush.Pushi, OperandType.InlineField, OpCodeType.Objmodel, FlowControl.Next, false );
         Stfld = new OpCode( OpCodeEncoding.Stfld, StackBehaviourPop.Popref_pop1, StackBehaviourPush.Push0, OperandType.InlineField, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldsfld = new OpCode( OpCodeEncoding.Ldsfld, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineField, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldsflda = new OpCode( OpCodeEncoding.Ldsflda, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineField, OpCodeType.Objmodel, FlowControl.Next, false );
         Stsfld = new OpCode( OpCodeEncoding.Stsfld, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.InlineField, OpCodeType.Objmodel, FlowControl.Next, false );
         Stobj = new OpCode( OpCodeEncoding.Stobj, StackBehaviourPop.Popi_pop1, StackBehaviourPush.Push0, OperandType.InlineType, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_I1_Un = new OpCode( OpCodeEncoding.Conv_Ovf_I1_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_I2_Un = new OpCode( OpCodeEncoding.Conv_Ovf_I2_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_I4_Un = new OpCode( OpCodeEncoding.Conv_Ovf_I4_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_I8_Un = new OpCode( OpCodeEncoding.Conv_Ovf_I8_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U1_Un = new OpCode( OpCodeEncoding.Conv_Ovf_U1_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U2_Un = new OpCode( OpCodeEncoding.Conv_Ovf_U2_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U4_Un = new OpCode( OpCodeEncoding.Conv_Ovf_U4_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U8_Un = new OpCode( OpCodeEncoding.Conv_Ovf_U8_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_I_Un = new OpCode( OpCodeEncoding.Conv_Ovf_I_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U_Un = new OpCode( OpCodeEncoding.Conv_Ovf_U_Un, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Box = new OpCode( OpCodeEncoding.Box, StackBehaviourPop.Pop1, StackBehaviourPush.Pushref, OperandType.InlineType, OpCodeType.Primitive, FlowControl.Next, false );
         Newarr = new OpCode( OpCodeEncoding.Newarr, StackBehaviourPop.Popi, StackBehaviourPush.Pushref, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldlen = new OpCode( OpCodeEncoding.Ldlen, StackBehaviourPop.Popref, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelema = new OpCode( OpCodeEncoding.Ldelema, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushi, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_I1 = new OpCode( OpCodeEncoding.Ldelem_I1, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_U1 = new OpCode( OpCodeEncoding.Ldelem_U1, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_I2 = new OpCode( OpCodeEncoding.Ldelem_I2, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_U2 = new OpCode( OpCodeEncoding.Ldelem_U2, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_I4 = new OpCode( OpCodeEncoding.Ldelem_I4, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_U4 = new OpCode( OpCodeEncoding.Ldelem_U4, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_I8 = new OpCode( OpCodeEncoding.Ldelem_I8, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushi8, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_I = new OpCode( OpCodeEncoding.Ldelem_I, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_R4 = new OpCode( OpCodeEncoding.Ldelem_R4, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushr4, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_R8 = new OpCode( OpCodeEncoding.Ldelem_R8, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushr8, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem_Ref = new OpCode( OpCodeEncoding.Ldelem_Ref, StackBehaviourPop.Popref_popi, StackBehaviourPush.Pushref, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Stelem_I = new OpCode( OpCodeEncoding.Stelem_I, StackBehaviourPop.Popref_popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Stelem_I1 = new OpCode( OpCodeEncoding.Stelem_I1, StackBehaviourPop.Popref_popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Stelem_I2 = new OpCode( OpCodeEncoding.Stelem_I2, StackBehaviourPop.Popref_popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Stelem_I4 = new OpCode( OpCodeEncoding.Stelem_I4, StackBehaviourPop.Popref_popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Stelem_I8 = new OpCode( OpCodeEncoding.Stelem_I8, StackBehaviourPop.Popref_popi_popi8, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Stelem_R4 = new OpCode( OpCodeEncoding.Stelem_R4, StackBehaviourPop.Popref_popi_popr4, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Stelem_R8 = new OpCode( OpCodeEncoding.Stelem_R8, StackBehaviourPop.Popref_popi_popr8, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Stelem_Ref = new OpCode( OpCodeEncoding.Stelem_Ref, StackBehaviourPop.Popref_popi_popref, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next, false );
         Ldelem = new OpCode( OpCodeEncoding.Ldelem, StackBehaviourPop.Popref_popi, StackBehaviourPush.Push1, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Stelem = new OpCode( OpCodeEncoding.Stelem, StackBehaviourPop.Popref_popi_pop1, StackBehaviourPush.Push0, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Unbox_Any = new OpCode( OpCodeEncoding.Unbox_Any, StackBehaviourPop.Popref, StackBehaviourPush.Push1, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Conv_Ovf_I1 = new OpCode( OpCodeEncoding.Conv_Ovf_I1, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U1 = new OpCode( OpCodeEncoding.Conv_Ovf_U1, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_I2 = new OpCode( OpCodeEncoding.Conv_Ovf_I2, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U2 = new OpCode( OpCodeEncoding.Conv_Ovf_U2, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_I4 = new OpCode( OpCodeEncoding.Conv_Ovf_I4, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U4 = new OpCode( OpCodeEncoding.Conv_Ovf_U4, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_I8 = new OpCode( OpCodeEncoding.Conv_Ovf_I8, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U8 = new OpCode( OpCodeEncoding.Conv_Ovf_U8, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Refanyval = new OpCode( OpCodeEncoding.Refanyval, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineType, OpCodeType.Primitive, FlowControl.Next, false );
         Ckfinite = new OpCode( OpCodeEncoding.Ckfinite, StackBehaviourPop.Pop1, StackBehaviourPush.Pushr8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Mkrefany = new OpCode( OpCodeEncoding.Mkrefany, StackBehaviourPop.Popi, StackBehaviourPush.Push1, OperandType.InlineType, OpCodeType.Primitive, FlowControl.Next, false );
         Ldtoken = new OpCode( OpCodeEncoding.Ldtoken, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineTok, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_U2 = new OpCode( OpCodeEncoding.Conv_U2, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_U1 = new OpCode( OpCodeEncoding.Conv_U1, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_I = new OpCode( OpCodeEncoding.Conv_I, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_I = new OpCode( OpCodeEncoding.Conv_Ovf_I, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_Ovf_U = new OpCode( OpCodeEncoding.Conv_Ovf_U, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Add_Ovf = new OpCode( OpCodeEncoding.Add_Ovf, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Add_Ovf_Un = new OpCode( OpCodeEncoding.Add_Ovf_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Mul_Ovf = new OpCode( OpCodeEncoding.Mul_Ovf, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Mul_Ovf_Un = new OpCode( OpCodeEncoding.Mul_Ovf_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Sub_Ovf = new OpCode( OpCodeEncoding.Sub_Ovf, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Sub_Ovf_Un = new OpCode( OpCodeEncoding.Sub_Ovf_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Endfinally = new OpCode( OpCodeEncoding.Endfinally, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Return, true );
         Leave = new OpCode( OpCodeEncoding.Leave, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineBrTarget, OpCodeType.Primitive, FlowControl.Branch, true );
         Leave_S = new OpCode( OpCodeEncoding.Leave_S, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Primitive, FlowControl.Branch, true );
         Stind_I = new OpCode( OpCodeEncoding.Stind_I, StackBehaviourPop.Popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Conv_U = new OpCode( OpCodeEncoding.Conv_U, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Arglist = new OpCode( OpCodeEncoding.Arglist, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ceq = new OpCode( OpCodeEncoding.Ceq, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Cgt = new OpCode( OpCodeEncoding.Cgt, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Cgt_Un = new OpCode( OpCodeEncoding.Cgt_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Clt = new OpCode( OpCodeEncoding.Clt, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Clt_Un = new OpCode( OpCodeEncoding.Clt_Un, StackBehaviourPop.Pop1_pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Ldftn = new OpCode( OpCodeEncoding.Ldftn, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineMethod, OpCodeType.Primitive, FlowControl.Next, false );
         Ldvirtftn = new OpCode( OpCodeEncoding.Ldvirtftn, StackBehaviourPop.Popref, StackBehaviourPush.Pushi, OperandType.InlineMethod, OpCodeType.Primitive, FlowControl.Next, false );
         Ldarg = new OpCode( OpCodeEncoding.Ldarg, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineVar, OpCodeType.Primitive, FlowControl.Next, false );
         Ldarga = new OpCode( OpCodeEncoding.Ldarga, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineVar, OpCodeType.Primitive, FlowControl.Next, false );
         Starg = new OpCode( OpCodeEncoding.Starg, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.InlineVar, OpCodeType.Primitive, FlowControl.Next, false );
         Ldloc = new OpCode( OpCodeEncoding.Ldloc, StackBehaviourPop.Pop0, StackBehaviourPush.Push1, OperandType.InlineVar, OpCodeType.Primitive, FlowControl.Next, false );
         Ldloca = new OpCode( OpCodeEncoding.Ldloca, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineVar, OpCodeType.Primitive, FlowControl.Next, false );
         Stloc = new OpCode( OpCodeEncoding.Stloc, StackBehaviourPop.Pop1, StackBehaviourPush.Push0, OperandType.InlineVar, OpCodeType.Primitive, FlowControl.Next, false );
         Localloc = new OpCode( OpCodeEncoding.Localloc, StackBehaviourPop.Popi, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Endfilter = new OpCode( OpCodeEncoding.Endfilter, StackBehaviourPop.Popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Return, true );
         Unaligned_ = new OpCode( OpCodeEncoding.Unaligned_, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.ShortInlineI, OpCodeType.Prefix, FlowControl.Meta, false );
         Volatile_ = new OpCode( OpCodeEncoding.Volatile_, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Prefix, FlowControl.Meta, false );
         Tail_ = new OpCode( OpCodeEncoding.Tail_, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Prefix, FlowControl.Meta, false );
         Initobj = new OpCode( OpCodeEncoding.Initobj, StackBehaviourPop.Popi, StackBehaviourPush.Push0, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next, false );
         Constrained_ = new OpCode( OpCodeEncoding.Constrained_, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineType, OpCodeType.Prefix, FlowControl.Meta, false );
         Cpblk = new OpCode( OpCodeEncoding.Cpblk, StackBehaviourPop.Popi_popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Initblk = new OpCode( OpCodeEncoding.Initblk, StackBehaviourPop.Popi_popi_popi, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Rethrow = new OpCode( OpCodeEncoding.Rethrow, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Throw, true );
         Sizeof = new OpCode( OpCodeEncoding.Sizeof, StackBehaviourPop.Pop0, StackBehaviourPush.Pushi, OperandType.InlineType, OpCodeType.Primitive, FlowControl.Next, false );
         Refanytype = new OpCode( OpCodeEncoding.Refanytype, StackBehaviourPop.Pop1, StackBehaviourPush.Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next, false );
         Readonly_ = new OpCode( OpCodeEncoding.Readonly_, StackBehaviourPop.Pop0, StackBehaviourPush.Push0, OperandType.InlineNone, OpCodeType.Prefix, FlowControl.Meta, false );

      }
   }

   /// <summary>
   /// This struct contains all required information about a single CIL op code. Very similar to System.Reflection.Emit.OpCode struct.
   /// </summary>
   public struct OpCode
   {
      internal const Int32 MAX_ONE_BYTE_INSTRUCTION = 0xFE;

      private readonly StackBehaviourPop _stackPop;
      private readonly StackBehaviourPush _stackPush;
      private readonly OperandType _operand;
      private readonly OpCodeType _type;
      private readonly FlowControl _flowControl;
      private readonly Boolean _unconditionallyEndsBulkOfCode;
      private readonly Int32 _stackChange;
      private readonly Int32 _size;
      private readonly Byte _byte1;
      private readonly Byte _byte2;

      internal OpCode(
         OpCodeEncoding encoded,
         StackBehaviourPop stackPop,
         StackBehaviourPush stackPush,
         OperandType operand,
         OpCodeType type,
         FlowControl flowControl,
         Boolean unconditionallyEndsBulkOfCode
         )
      {
         this._size = ( (Int32) encoded ) > MAX_ONE_BYTE_INSTRUCTION ? 2 : 1;
         this._byte1 = (Byte) ( ( (Int32) encoded ) >> 8 );
         this._byte2 = (Byte) encoded;
         this._stackPop = stackPop;
         this._stackPush = stackPush;
         this._operand = operand;
         this._type = type;
         this._flowControl = flowControl;
         this._unconditionallyEndsBulkOfCode = unconditionallyEndsBulkOfCode;
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
         this._stackChange = stackChange;

         OpCodes.Codes.Add( encoded, this );
         if ( API.OperandType.InlineNone == operand )
         {
            OpCodes.CodeInfosWithNoOperand.Add( encoded, new OpCodeInfoWithNoOperand( this ) );
         }
      }

      /// <summary>
      /// Gets the textual name of this <see cref="OpCode"/>.
      /// </summary>
      /// <value>The textual name of this <see cref="OpCode"/>.</value>
      public String Name
      {
         get
         {
            return Enum.Format( typeof( OpCodeEncoding ), this.Value, "g" ).ToLowerInvariant().Replace( "_", "." );
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
            return this._size;
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
            return this._stackPop;
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
            return this._stackPush;
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
            return this._operand;
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
            return this._type;
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
            return this._flowControl;
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
            return this._stackChange;
         }
      }

      /// <summary>
      /// Gets the value of this <see cref="OpCode"/> as integer.
      /// </summary>
      /// <value>The value of this <see cref="OpCode"/> as integer.</value>
      public OpCodeEncoding Value
      {
         get
         {
            return (OpCodeEncoding) this.ValueInternal;
         }
      }

      internal Int32 ValueInternal
      {
         get
         {
            Int32 cur = this._byte2;
            if ( this._size > 1 )
            {
               cur = cur | ( this._byte1 << 8 );
            }
            return cur;
         }
      }

      /// <summary>
      /// Gets the first byte of this <see cref="OpCode"/>.
      /// </summary>
      /// <value>The first byte of this <see cref="OpCode"/>.</value>
      public Byte Byte1
      {
         get
         {
            return this._byte1;
         }
      }

      /// <summary>
      /// Gets the second byte of this <see cref="OpCode"/>.
      /// </summary>
      /// <value>The scond byte of this <see cref="OpCode"/>.</value>
      public Byte Byte2
      {
         get
         {
            return this._byte2;
         }
      }

      internal Boolean UnconditionallyEndsBulkOfCode
      {
         get
         {
            return this._unconditionallyEndsBulkOfCode;
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
         return this.ValueInternal;
      }

      /// <summary>
      /// Checks whether this <see cref="OpCode"/> equals to the given <paramref name="code"/>-
      /// </summary>
      /// <param name="code">Another <see cref="OpCode"/>.</param>
      /// <returns><c>true</c> if this opcode has same <see cref="Value"/> as <paramref name="code"/>; <c>false</c> otherwise.</returns>
      public Boolean Equals( OpCode code )
      {
         return this.Value == code.Value;
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
   public enum OpCodeEncoding
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
   public enum FlowControl
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
   public enum OpCodeType
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
   public enum OperandType
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
      // <summary>The operand is reserved and should not be used.</summary>
      //Reserved,

      /// <summary>The operand is a 64-bit IEEE floating point number.</summary>
      InlineR,
      /// <summary>The operand is a 32-bit metadata signature token.</summary>
      InlineSig,
      /// <summary>The operand is a 32-bit metadata string token.</summary>
      InlineString,
      /// <summary>The operand is the 32-bit integer argument to a switch instruction.</summary>
      InlineSwitch,
      /// <summary>The operand is a FieldRef, MethodRef, or TypeRef token.</summary>
      InlineTok,
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
   public enum StackBehaviourPush
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

   /// <summary>
   /// Contains all possible values for stack popping behaviour of CIL op codes.
   /// </summary>
   public enum StackBehaviourPop
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