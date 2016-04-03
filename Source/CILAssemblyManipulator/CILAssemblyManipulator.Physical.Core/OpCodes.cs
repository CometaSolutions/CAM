using CILAssemblyManipulator.Physical;
using CollectionsWithRoles.API;
using CommonUtils;
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
   using TDynamicStackChangeParameter = Tuple<DynamicStackChangeDelegate, DynamicStackChangeSizeDelegate>;

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
      /// See <see cref="F:System.Reflection.Emit.OpCodes.No"/>.
      /// </summary>
      public static readonly OpCode No_;
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
         var cf = CollectionsWithRoles.Implementation.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY;

         var Pop0 = cf.NewArrayProxyFromParams<StackValueKind>().CQ;
         var Pop1 = cf.NewArrayProxyFromParams( StackValueKind.Value ).CQ;
         var Pop1_pop1 = cf.NewArrayProxyFromParams( StackValueKind.Value, StackValueKind.Value ).CQ;
         var Popi = cf.NewArrayProxyFromParams( StackValueKind.I4 ).CQ;
         var Popi_pop1 = cf.NewArrayProxyFromParams( StackValueKind.I4, StackValueKind.Value ).CQ;
         var Popi_popi = cf.NewArrayProxyFromParams( StackValueKind.I4, StackValueKind.I4 ).CQ;
         var Popi_popi8 = cf.NewArrayProxyFromParams( StackValueKind.I4, StackValueKind.I8 ).CQ;
         var Popi_popi_popi = cf.NewArrayProxyFromParams( StackValueKind.I4, StackValueKind.I4, StackValueKind.I4 ).CQ;
         var Popi_popr4 = cf.NewArrayProxyFromParams( StackValueKind.I4, StackValueKind.R4 ).CQ;
         var Popi_popr8 = cf.NewArrayProxyFromParams( StackValueKind.I4, StackValueKind.R8 ).CQ;
         var Popref = cf.NewArrayProxyFromParams( StackValueKind.Ref ).CQ;
         var Popref_pop1 = cf.NewArrayProxyFromParams( StackValueKind.Ref, StackValueKind.Value ).CQ;
         var Popref_popi = cf.NewArrayProxyFromParams( StackValueKind.Ref, StackValueKind.I4 ).CQ;
         var Popref_popi_popi = cf.NewArrayProxyFromParams( StackValueKind.Ref, StackValueKind.I4, StackValueKind.I4 ).CQ;
         var Popref_popi_popi8 = cf.NewArrayProxyFromParams( StackValueKind.Ref, StackValueKind.I4, StackValueKind.I8 ).CQ;
         var Popref_popi_popr4 = cf.NewArrayProxyFromParams( StackValueKind.Ref, StackValueKind.I4, StackValueKind.R4 ).CQ;
         var Popref_popi_popr8 = cf.NewArrayProxyFromParams( StackValueKind.Ref, StackValueKind.I4, StackValueKind.R8 ).CQ;
         var Popref_popi_popref = cf.NewArrayProxyFromParams( StackValueKind.Ref, StackValueKind.I4, StackValueKind.Ref ).CQ;
         var Popref_popi_pop1 = cf.NewArrayProxyFromParams( StackValueKind.Ref, StackValueKind.I4, StackValueKind.Value ).CQ;

         var Push0 = Pop0;
         var Push1 = cf.NewArrayProxyFromParams( StackValueKind.Value ).CQ;
         var Push1_push1 = cf.NewArrayProxyFromParams( StackValueKind.Value, StackValueKind.Value ).CQ;
         var Pushi = cf.NewArrayProxyFromParams( StackValueKind.I4 ).CQ;
         var Pushi8 = cf.NewArrayProxyFromParams( StackValueKind.I8 ).CQ;
         var Pushr4 = cf.NewArrayProxyFromParams( StackValueKind.R4 ).CQ;
         var Pushr8 = cf.NewArrayProxyFromParams( StackValueKind.R8 ).CQ;
         var Pushref = cf.NewArrayProxyFromParams( StackValueKind.Ref ).CQ;

         Nop = new OpCode( OpCodeID.Nop, Pop0, Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Break = new OpCode( OpCodeID.Break, Pop0, Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Break );
         Ldarg_0 = new OpCode( OpCodeID.Ldarg_0, Pop0, Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Ldarg_1 = new OpCode( OpCodeID.Ldarg_1, Pop0, Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Ldarg_2 = new OpCode( OpCodeID.Ldarg_2, Pop0, Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Ldarg_3 = new OpCode( OpCodeID.Ldarg_3, Pop0, Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Ldloc_0 = new OpCode( OpCodeID.Ldloc_0, Pop0, Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Ldloc_1 = new OpCode( OpCodeID.Ldloc_1, Pop0, Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Ldloc_2 = new OpCode( OpCodeID.Ldloc_2, Pop0, Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Ldloc_3 = new OpCode( OpCodeID.Ldloc_3, Pop0, Push1, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Stloc_0 = new OpCode( OpCodeID.Stloc_0, Pop1, Push0, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Stloc_1 = new OpCode( OpCodeID.Stloc_1, Pop1, Push0, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Stloc_2 = new OpCode( OpCodeID.Stloc_2, Pop1, Push0, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Stloc_3 = new OpCode( OpCodeID.Stloc_3, Pop1, Push0, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Ldarg_S = new OpCode( OpCodeID.Ldarg_S, Pop0, Push1, OperandType.ShortInlineVar, OpCodeType.Macro, FlowControl.Next );
         Ldarga_S = new OpCode( OpCodeID.Ldarga_S, Pop0, Pushi, OperandType.ShortInlineVar, OpCodeType.Macro, FlowControl.Next );
         Starg_S = new OpCode( OpCodeID.Starg_S, Pop1, Push0, OperandType.ShortInlineVar, OpCodeType.Macro, FlowControl.Next );
         Ldloc_S = new OpCode( OpCodeID.Ldloc_S, Pop0, Push1, OperandType.ShortInlineVar, OpCodeType.Macro, FlowControl.Next );
         Ldloca_S = new OpCode( OpCodeID.Ldloca_S, Pop0, Pushi, OperandType.ShortInlineVar, OpCodeType.Macro, FlowControl.Next );
         Stloc_S = new OpCode( OpCodeID.Stloc_S, Pop1, Push0, OperandType.ShortInlineVar, OpCodeType.Macro, FlowControl.Next );
         Ldnull = new OpCode( OpCodeID.Ldnull, Pop0, Pushref, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Ldc_I4_M1 = new OpCode( OpCodeID.Ldc_I4_M1, Pop0, Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Ldc_I4_0 = new OpCode( OpCodeID.Ldc_I4_0, Pop0, Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Ldc_I4_1 = new OpCode( OpCodeID.Ldc_I4_1, Pop0, Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Ldc_I4_2 = new OpCode( OpCodeID.Ldc_I4_2, Pop0, Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Ldc_I4_3 = new OpCode( OpCodeID.Ldc_I4_3, Pop0, Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Ldc_I4_4 = new OpCode( OpCodeID.Ldc_I4_4, Pop0, Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Ldc_I4_5 = new OpCode( OpCodeID.Ldc_I4_5, Pop0, Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Ldc_I4_6 = new OpCode( OpCodeID.Ldc_I4_6, Pop0, Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Ldc_I4_7 = new OpCode( OpCodeID.Ldc_I4_7, Pop0, Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Ldc_I4_8 = new OpCode( OpCodeID.Ldc_I4_8, Pop0, Pushi, OperandType.InlineNone, OpCodeType.Macro, FlowControl.Next );
         Ldc_I4_S = new OpCode( OpCodeID.Ldc_I4_S, Pop0, Pushi, OperandType.ShortInlineI, OpCodeType.Macro, FlowControl.Next );
         Ldc_I4 = new OpCode( OpCodeID.Ldc_I4, Pop0, Pushi, OperandType.InlineI, OpCodeType.Primitive, FlowControl.Next );
         Ldc_I8 = new OpCode( OpCodeID.Ldc_I8, Pop0, Pushi8, OperandType.InlineI8, OpCodeType.Primitive, FlowControl.Next );
         Ldc_R4 = new OpCode( OpCodeID.Ldc_R4, Pop0, Pushr4, OperandType.ShortInlineR, OpCodeType.Primitive, FlowControl.Next );
         Ldc_R8 = new OpCode( OpCodeID.Ldc_R8, Pop0, Pushr8, OperandType.InlineR, OpCodeType.Primitive, FlowControl.Next );
         Dup = new OpCode( OpCodeID.Dup, Pop1, Push1_push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Pop = new OpCode( OpCodeID.Pop, Pop1, Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Jmp = new OpCode( OpCodeID.Jmp, Pop0, Push0, OperandType.InlineMethod, OpCodeType.Primitive, FlowControl.Call );
         Call = new OpCode( OpCodeID.Call, Tuple.Create<DynamicStackChangeDelegate, DynamicStackChangeSizeDelegate>( DynamicStackChanges.Varpop_CallOrCallvirt, DynamicStackChanges.Varpop_CallOrCallvirt_Count ), Tuple.Create<DynamicStackChangeDelegate, DynamicStackChangeSizeDelegate>( DynamicStackChanges.Varpush_Any, DynamicStackChanges.Varpush_Any_Count ), OperandType.InlineMethod, OpCodeType.Primitive, FlowControl.Call );
         Calli = new OpCode( OpCodeID.Calli, Tuple.Create<DynamicStackChangeDelegate, DynamicStackChangeSizeDelegate>( DynamicStackChanges.Varpop_Calli, DynamicStackChanges.Varpop_Calli_Count ), Tuple.Create<DynamicStackChangeDelegate, DynamicStackChangeSizeDelegate>( DynamicStackChanges.Varpush_Any, DynamicStackChanges.Varpush_Any_Count ), OperandType.InlineSignature, OpCodeType.Primitive, FlowControl.Call );
         Ret = new OpCode( OpCodeID.Ret, Tuple.Create<DynamicStackChangeDelegate, DynamicStackChangeSizeDelegate>( DynamicStackChanges.Varpop_Ret, DynamicStackChanges.Varpop_Ret_Count ), Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Return );
         Br_S = new OpCode( OpCodeID.Br_S, Pop0, Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Branch, OpCodeID.Br );
         Brfalse_S = new OpCode( OpCodeID.Brfalse_S, Popi, Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Brfalse );
         Brtrue_S = new OpCode( OpCodeID.Brtrue_S, Popi, Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Brtrue );
         Beq_S = new OpCode( OpCodeID.Beq_S, Pop1_pop1, Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Beq );
         Bge_S = new OpCode( OpCodeID.Bge_S, Pop1_pop1, Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Bge );
         Bgt_S = new OpCode( OpCodeID.Bgt_S, Pop1_pop1, Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Bgt );
         Ble_S = new OpCode( OpCodeID.Ble_S, Pop1_pop1, Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Ble );
         Blt_S = new OpCode( OpCodeID.Blt_S, Pop1_pop1, Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Blt );
         Bne_Un_S = new OpCode( OpCodeID.Bne_Un_S, Pop1_pop1, Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Bne_Un );
         Bge_Un_S = new OpCode( OpCodeID.Bge_Un_S, Pop1_pop1, Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Bge_Un );
         Bgt_Un_S = new OpCode( OpCodeID.Bgt_Un_S, Pop1_pop1, Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Bgt_Un );
         Ble_Un_S = new OpCode( OpCodeID.Ble_Un_S, Pop1_pop1, Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Ble_Un );
         Blt_Un_S = new OpCode( OpCodeID.Blt_Un_S, Pop1_pop1, Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Blt_Un );
         Br = new OpCode( OpCodeID.Br, Pop0, Push0, OperandType.InlineBrTarget, OpCodeType.Primitive, FlowControl.Branch, OpCodeID.Br_S );
         Brfalse = new OpCode( OpCodeID.Brfalse, Popi, Push0, OperandType.InlineBrTarget, OpCodeType.Primitive, FlowControl.Cond_Branch, OpCodeID.Brfalse_S );
         Brtrue = new OpCode( OpCodeID.Brtrue, Popi, Push0, OperandType.InlineBrTarget, OpCodeType.Primitive, FlowControl.Cond_Branch, OpCodeID.Brtrue_S );
         Beq = new OpCode( OpCodeID.Beq, Pop1_pop1, Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Beq_S );
         Bge = new OpCode( OpCodeID.Bge, Pop1_pop1, Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Bge_S );
         Bgt = new OpCode( OpCodeID.Bgt, Pop1_pop1, Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Bgt_S );
         Ble = new OpCode( OpCodeID.Ble, Pop1_pop1, Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Ble_S );
         Blt = new OpCode( OpCodeID.Blt, Pop1_pop1, Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Blt_S );
         Bne_Un = new OpCode( OpCodeID.Bne_Un, Pop1_pop1, Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Bne_Un_S );
         Bge_Un = new OpCode( OpCodeID.Bge_Un, Pop1_pop1, Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Bge_Un_S );
         Bgt_Un = new OpCode( OpCodeID.Bgt_Un, Pop1_pop1, Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Bgt_Un_S );
         Ble_Un = new OpCode( OpCodeID.Ble_Un, Pop1_pop1, Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Ble_Un_S );
         Blt_Un = new OpCode( OpCodeID.Blt_Un, Pop1_pop1, Push0, OperandType.InlineBrTarget, OpCodeType.Macro, FlowControl.Cond_Branch, OpCodeID.Blt_Un_S );
         Switch = new OpCode( OpCodeID.Switch, Popi, Push0, OperandType.InlineSwitch, OpCodeType.Primitive, FlowControl.Cond_Branch );
         Ldind_I1 = new OpCode( OpCodeID.Ldind_I1, Popi, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Ldind_U1 = new OpCode( OpCodeID.Ldind_U1, Popi, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Ldind_I2 = new OpCode( OpCodeID.Ldind_I2, Popi, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Ldind_U2 = new OpCode( OpCodeID.Ldind_U2, Popi, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Ldind_I4 = new OpCode( OpCodeID.Ldind_I4, Popi, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Ldind_U4 = new OpCode( OpCodeID.Ldind_U4, Popi, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Ldind_I8 = new OpCode( OpCodeID.Ldind_I8, Popi, Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Ldind_I = new OpCode( OpCodeID.Ldind_I, Popi, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Ldind_R4 = new OpCode( OpCodeID.Ldind_R4, Popi, Pushr4, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Ldind_R8 = new OpCode( OpCodeID.Ldind_R8, Popi, Pushr8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Ldind_Ref = new OpCode( OpCodeID.Ldind_Ref, Popi, Pushref, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Stind_Ref = new OpCode( OpCodeID.Stind_Ref, Popi_popi, Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Stind_I1 = new OpCode( OpCodeID.Stind_I1, Popi_popi, Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Stind_I2 = new OpCode( OpCodeID.Stind_I2, Popi_popi, Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Stind_I4 = new OpCode( OpCodeID.Stind_I4, Popi_popi, Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Stind_I8 = new OpCode( OpCodeID.Stind_I8, Popi_popi8, Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Stind_R4 = new OpCode( OpCodeID.Stind_R4, Popi_popr4, Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Stind_R8 = new OpCode( OpCodeID.Stind_R8, Popi_popr8, Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Add = new OpCode( OpCodeID.Add, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Sub = new OpCode( OpCodeID.Sub, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Mul = new OpCode( OpCodeID.Mul, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Div = new OpCode( OpCodeID.Div, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Div_Un = new OpCode( OpCodeID.Div_Un, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Rem = new OpCode( OpCodeID.Rem, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Rem_Un = new OpCode( OpCodeID.Rem_Un, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         And = new OpCode( OpCodeID.And, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Or = new OpCode( OpCodeID.Or, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Xor = new OpCode( OpCodeID.Xor, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Shl = new OpCode( OpCodeID.Shl, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Shr = new OpCode( OpCodeID.Shr, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Shr_Un = new OpCode( OpCodeID.Shr_Un, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Neg = new OpCode( OpCodeID.Neg, Pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Not = new OpCode( OpCodeID.Not, Pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_I1 = new OpCode( OpCodeID.Conv_I1, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_I2 = new OpCode( OpCodeID.Conv_I2, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_I4 = new OpCode( OpCodeID.Conv_I4, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_I8 = new OpCode( OpCodeID.Conv_I8, Pop1, Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_R4 = new OpCode( OpCodeID.Conv_R4, Pop1, Pushr4, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_R8 = new OpCode( OpCodeID.Conv_R8, Pop1, Pushr8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_U4 = new OpCode( OpCodeID.Conv_U4, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_U8 = new OpCode( OpCodeID.Conv_U8, Pop1, Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Callvirt = new OpCode( OpCodeID.Callvirt, Tuple.Create<DynamicStackChangeDelegate, DynamicStackChangeSizeDelegate>( DynamicStackChanges.Varpop_CallOrCallvirt, DynamicStackChanges.Varpop_CallOrCallvirt_Count ), Tuple.Create<DynamicStackChangeDelegate, DynamicStackChangeSizeDelegate>( DynamicStackChanges.Varpush_Any, DynamicStackChanges.Varpush_Any_Count ), OperandType.InlineMethod, OpCodeType.Objmodel, FlowControl.Call );
         Cpobj = new OpCode( OpCodeID.Cpobj, Popi_popi, Push0, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next );
         Ldobj = new OpCode( OpCodeID.Ldobj, Popi, Push1, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next );
         Ldstr = new OpCode( OpCodeID.Ldstr, Pop0, Pushref, OperandType.InlineString, OpCodeType.Objmodel, FlowControl.Next );
         Newobj = new OpCode( OpCodeID.Newobj, Tuple.Create<DynamicStackChangeDelegate, DynamicStackChangeSizeDelegate>( DynamicStackChanges.Varpop_Newobj, DynamicStackChanges.Varpop_Newobj_Count ), Pushref, OperandType.InlineMethod, OpCodeType.Objmodel, FlowControl.Call );
         Castclass = new OpCode( OpCodeID.Castclass, Popref, Pushref, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next );
         Isinst = new OpCode( OpCodeID.Isinst, Popref, Pushi, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next );
         Conv_R_Un = new OpCode( OpCodeID.Conv_R_Un, Pop1, Pushr8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Unbox = new OpCode( OpCodeID.Unbox, Popref, Pushi, OperandType.InlineType, OpCodeType.Primitive, FlowControl.Next );
         Throw = new OpCode( OpCodeID.Throw, Popref, Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Throw );
         Ldfld = new OpCode( OpCodeID.Ldfld, Popref, Push1, OperandType.InlineField, OpCodeType.Objmodel, FlowControl.Next );
         Ldflda = new OpCode( OpCodeID.Ldflda, Popref, Pushi, OperandType.InlineField, OpCodeType.Objmodel, FlowControl.Next );
         Stfld = new OpCode( OpCodeID.Stfld, Popref_pop1, Push0, OperandType.InlineField, OpCodeType.Objmodel, FlowControl.Next );
         Ldsfld = new OpCode( OpCodeID.Ldsfld, Pop0, Push1, OperandType.InlineField, OpCodeType.Objmodel, FlowControl.Next );
         Ldsflda = new OpCode( OpCodeID.Ldsflda, Pop0, Pushi, OperandType.InlineField, OpCodeType.Objmodel, FlowControl.Next );
         Stsfld = new OpCode( OpCodeID.Stsfld, Pop1, Push0, OperandType.InlineField, OpCodeType.Objmodel, FlowControl.Next );
         Stobj = new OpCode( OpCodeID.Stobj, Popi_pop1, Push0, OperandType.InlineType, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_I1_Un = new OpCode( OpCodeID.Conv_Ovf_I1_Un, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_I2_Un = new OpCode( OpCodeID.Conv_Ovf_I2_Un, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_I4_Un = new OpCode( OpCodeID.Conv_Ovf_I4_Un, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_I8_Un = new OpCode( OpCodeID.Conv_Ovf_I8_Un, Pop1, Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_U1_Un = new OpCode( OpCodeID.Conv_Ovf_U1_Un, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_U2_Un = new OpCode( OpCodeID.Conv_Ovf_U2_Un, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_U4_Un = new OpCode( OpCodeID.Conv_Ovf_U4_Un, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_U8_Un = new OpCode( OpCodeID.Conv_Ovf_U8_Un, Pop1, Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_I_Un = new OpCode( OpCodeID.Conv_Ovf_I_Un, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_U_Un = new OpCode( OpCodeID.Conv_Ovf_U_Un, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Box = new OpCode( OpCodeID.Box, Pop1, Pushref, OperandType.InlineType, OpCodeType.Primitive, FlowControl.Next );
         Newarr = new OpCode( OpCodeID.Newarr, Popi, Pushref, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next );
         Ldlen = new OpCode( OpCodeID.Ldlen, Popref, Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Ldelema = new OpCode( OpCodeID.Ldelema, Popref_popi, Pushi, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next );
         Ldelem_I1 = new OpCode( OpCodeID.Ldelem_I1, Popref_popi, Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Ldelem_U1 = new OpCode( OpCodeID.Ldelem_U1, Popref_popi, Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Ldelem_I2 = new OpCode( OpCodeID.Ldelem_I2, Popref_popi, Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Ldelem_U2 = new OpCode( OpCodeID.Ldelem_U2, Popref_popi, Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Ldelem_I4 = new OpCode( OpCodeID.Ldelem_I4, Popref_popi, Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Ldelem_U4 = new OpCode( OpCodeID.Ldelem_U4, Popref_popi, Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Ldelem_I8 = new OpCode( OpCodeID.Ldelem_I8, Popref_popi, Pushi8, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Ldelem_I = new OpCode( OpCodeID.Ldelem_I, Popref_popi, Pushi, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Ldelem_R4 = new OpCode( OpCodeID.Ldelem_R4, Popref_popi, Pushr4, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Ldelem_R8 = new OpCode( OpCodeID.Ldelem_R8, Popref_popi, Pushr8, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Ldelem_Ref = new OpCode( OpCodeID.Ldelem_Ref, Popref_popi, Pushref, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Stelem_I = new OpCode( OpCodeID.Stelem_I, Popref_popi_popi, Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Stelem_I1 = new OpCode( OpCodeID.Stelem_I1, Popref_popi_popi, Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Stelem_I2 = new OpCode( OpCodeID.Stelem_I2, Popref_popi_popi, Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Stelem_I4 = new OpCode( OpCodeID.Stelem_I4, Popref_popi_popi, Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Stelem_I8 = new OpCode( OpCodeID.Stelem_I8, Popref_popi_popi8, Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Stelem_R4 = new OpCode( OpCodeID.Stelem_R4, Popref_popi_popr4, Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Stelem_R8 = new OpCode( OpCodeID.Stelem_R8, Popref_popi_popr8, Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Stelem_Ref = new OpCode( OpCodeID.Stelem_Ref, Popref_popi_popref, Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Next );
         Ldelem = new OpCode( OpCodeID.Ldelem, Popref_popi, Push1, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next );
         Stelem = new OpCode( OpCodeID.Stelem, Popref_popi_pop1, Push0, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next );
         Unbox_Any = new OpCode( OpCodeID.Unbox_Any, Popref, Push1, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next );
         Conv_Ovf_I1 = new OpCode( OpCodeID.Conv_Ovf_I1, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_U1 = new OpCode( OpCodeID.Conv_Ovf_U1, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_I2 = new OpCode( OpCodeID.Conv_Ovf_I2, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_U2 = new OpCode( OpCodeID.Conv_Ovf_U2, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_I4 = new OpCode( OpCodeID.Conv_Ovf_I4, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_U4 = new OpCode( OpCodeID.Conv_Ovf_U4, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_I8 = new OpCode( OpCodeID.Conv_Ovf_I8, Pop1, Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_U8 = new OpCode( OpCodeID.Conv_Ovf_U8, Pop1, Pushi8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Refanyval = new OpCode( OpCodeID.Refanyval, Pop1, Pushi, OperandType.InlineType, OpCodeType.Primitive, FlowControl.Next );
         Ckfinite = new OpCode( OpCodeID.Ckfinite, Pop1, Pushr8, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Mkrefany = new OpCode( OpCodeID.Mkrefany, Popi, Push1, OperandType.InlineType, OpCodeType.Primitive, FlowControl.Next );
         Ldtoken = new OpCode( OpCodeID.Ldtoken, Pop0, Pushi, OperandType.InlineToken, OpCodeType.Primitive, FlowControl.Next );
         Conv_U2 = new OpCode( OpCodeID.Conv_U2, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_U1 = new OpCode( OpCodeID.Conv_U1, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_I = new OpCode( OpCodeID.Conv_I, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_I = new OpCode( OpCodeID.Conv_Ovf_I, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_Ovf_U = new OpCode( OpCodeID.Conv_Ovf_U, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Add_Ovf = new OpCode( OpCodeID.Add_Ovf, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Add_Ovf_Un = new OpCode( OpCodeID.Add_Ovf_Un, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Mul_Ovf = new OpCode( OpCodeID.Mul_Ovf, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Mul_Ovf_Un = new OpCode( OpCodeID.Mul_Ovf_Un, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Sub_Ovf = new OpCode( OpCodeID.Sub_Ovf, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Sub_Ovf_Un = new OpCode( OpCodeID.Sub_Ovf_Un, Pop1_pop1, Push1, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Endfinally = new OpCode( OpCodeID.Endfinally, Pop0, Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Return );
         Leave = new OpCode( OpCodeID.Leave, Tuple.Create<DynamicStackChangeDelegate, DynamicStackChangeSizeDelegate>( DynamicStackChanges.Varpop_Leave, DynamicStackChanges.Varpop_Leave_Count ), Push0, OperandType.InlineBrTarget, OpCodeType.Primitive, FlowControl.Branch, OpCodeID.Leave_S );
         Leave_S = new OpCode( OpCodeID.Leave_S, Tuple.Create<DynamicStackChangeDelegate, DynamicStackChangeSizeDelegate>( DynamicStackChanges.Varpop_Leave, DynamicStackChanges.Varpop_Leave_Count ), Push0, OperandType.ShortInlineBrTarget, OpCodeType.Primitive, FlowControl.Branch, OpCodeID.Leave );
         Stind_I = new OpCode( OpCodeID.Stind_I, Popi_popi, Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Conv_U = new OpCode( OpCodeID.Conv_U, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Arglist = new OpCode( OpCodeID.Arglist, Pop0, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Ceq = new OpCode( OpCodeID.Ceq, Pop1_pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Cgt = new OpCode( OpCodeID.Cgt, Pop1_pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Cgt_Un = new OpCode( OpCodeID.Cgt_Un, Pop1_pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Clt = new OpCode( OpCodeID.Clt, Pop1_pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Clt_Un = new OpCode( OpCodeID.Clt_Un, Pop1_pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Ldftn = new OpCode( OpCodeID.Ldftn, Pop0, Pushi, OperandType.InlineMethod, OpCodeType.Primitive, FlowControl.Next );
         Ldvirtftn = new OpCode( OpCodeID.Ldvirtftn, Popref, Pushi, OperandType.InlineMethod, OpCodeType.Primitive, FlowControl.Next );
         Ldarg = new OpCode( OpCodeID.Ldarg, Pop0, Push1, OperandType.InlineVar, OpCodeType.Primitive, FlowControl.Next );
         Ldarga = new OpCode( OpCodeID.Ldarga, Pop0, Pushi, OperandType.InlineVar, OpCodeType.Primitive, FlowControl.Next );
         Starg = new OpCode( OpCodeID.Starg, Pop1, Push0, OperandType.InlineVar, OpCodeType.Primitive, FlowControl.Next );
         Ldloc = new OpCode( OpCodeID.Ldloc, Pop0, Push1, OperandType.InlineVar, OpCodeType.Primitive, FlowControl.Next );
         Ldloca = new OpCode( OpCodeID.Ldloca, Pop0, Pushi, OperandType.InlineVar, OpCodeType.Primitive, FlowControl.Next );
         Stloc = new OpCode( OpCodeID.Stloc, Pop1, Push0, OperandType.InlineVar, OpCodeType.Primitive, FlowControl.Next );
         Localloc = new OpCode( OpCodeID.Localloc, Popi, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Endfilter = new OpCode( OpCodeID.Endfilter, Popi, Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Return );
         Unaligned_ = new OpCode( OpCodeID.Unaligned_, Pop0, Push0, OperandType.ShortInlineI, OpCodeType.Prefix, FlowControl.Meta );
         Volatile_ = new OpCode( OpCodeID.Volatile_, Pop0, Push0, OperandType.InlineNone, OpCodeType.Prefix, FlowControl.Meta );
         Tail_ = new OpCode( OpCodeID.Tail_, Pop0, Push0, OperandType.InlineNone, OpCodeType.Prefix, FlowControl.Meta );
         Initobj = new OpCode( OpCodeID.Initobj, Popi, Push0, OperandType.InlineType, OpCodeType.Objmodel, FlowControl.Next );
         Constrained_ = new OpCode( OpCodeID.Constrained_, Pop0, Push0, OperandType.InlineType, OpCodeType.Prefix, FlowControl.Meta );
         Cpblk = new OpCode( OpCodeID.Cpblk, Popi_popi_popi, Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Initblk = new OpCode( OpCodeID.Initblk, Popi_popi_popi, Push0, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         No_ = new OpCode( OpCodeID.No_, Pop0, Push0, OperandType.ShortInlineI, OpCodeType.Prefix, FlowControl.Next );
         Rethrow = new OpCode( OpCodeID.Rethrow, Pop0, Push0, OperandType.InlineNone, OpCodeType.Objmodel, FlowControl.Throw );
         Sizeof = new OpCode( OpCodeID.Sizeof, Pop0, Pushi, OperandType.InlineType, OpCodeType.Primitive, FlowControl.Next );
         Refanytype = new OpCode( OpCodeID.Refanytype, Pop1, Pushi, OperandType.InlineNone, OpCodeType.Primitive, FlowControl.Next );
         Readonly_ = new OpCode( OpCodeID.Readonly_, Pop0, Push0, OperandType.InlineNone, OpCodeType.Prefix, FlowControl.Meta );
      }

      private static class DynamicStackChanges
      {
         internal static IEnumerable<StackValueKind> Varpop_Ret( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, IEnumerable<StackValueKind> currentStack )
         {
            return GetStackChangeForReturnOf( method?.Signature );
         }

         internal static IEnumerable<StackValueKind> Varpop_Newobj( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, IEnumerable<StackValueKind> currentStack )
         {
            AbstractMethodSignature signature;
            return Varpop_Newobj( md, method, codeInfo, out signature );
         }

         internal static IEnumerable<StackValueKind> Varpop_CallOrCallvirt( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, IEnumerable<StackValueKind> currentStack )
         {
            AbstractMethodSignature signature;
            var paramz = Varpop_Newobj( md, method, codeInfo, out signature );
            if ( signature != null )
            {
               if ( signature.MethodSignatureInformation.IsHasThis() )
               {
                  // Pop 'this' as first
                  // TODO value vs ref
                  yield return StackValueKind.Value;
               }

               foreach ( var param in paramz )
               {
                  yield return param;
               }
            }
         }

         internal static IEnumerable<StackValueKind> Varpop_Calli( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, IEnumerable<StackValueKind> currentStack )
         {
            var values = Varpop_CallOrCallvirt( md, method, codeInfo, currentStack );

            // Calli takes function pointer in addition to other args
            // TODO Is function pointer value or ref?
            yield return StackValueKind.Value;

            foreach ( var value in values )
            {
               yield return value;
            }
         }

         internal static IEnumerable<StackValueKind> Varpop_Leave( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, IEnumerable<StackValueKind> currentStack )
         {
            // Leave instruction pops all stack values.
            return currentStack;
         }

         internal static IEnumerable<StackValueKind> Varpush_Any( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, IEnumerable<StackValueKind> currentStack )
         {
            return GetStackChangeForReturnOf( GetSignatureFromOpCodeInfo( md, codeInfo ) );
         }

         internal static Int32 Varpop_Ret_Count( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, Int32 currentStack )
         {
            return GetStackChangeForReturnOf_Count( method?.Signature );
         }

         internal static Int32 Varpop_Newobj_Count( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, Int32 currentStack )
         {
            AbstractMethodSignature signature;
            return Varpop_Newobj_Count( md, method, codeInfo, out signature );
         }

         internal static Int32 Varpop_CallOrCallvirt_Count( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, Int32 currentStack )
         {
            AbstractMethodSignature signature;
            var count = Varpop_Newobj_Count( md, method, codeInfo, out signature );
            if ( signature.MethodSignatureInformation.IsHasThis() )
            {
               ++count;
            }
            return count;
         }

         internal static Int32 Varpop_Calli_Count( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, Int32 currentStack )
         {
            return Varpop_CallOrCallvirt_Count( md, method, codeInfo, currentStack ) + 1;
         }

         internal static Int32 Varpop_Leave_Count( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, Int32 currentStack )
         {
            return currentStack;
         }

         internal static Int32 Varpush_Any_Count( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, Int32 currentStack )
         {
            return GetStackChangeForReturnOf_Count( GetSignatureFromOpCodeInfo( md, codeInfo ) );
         }

         private static IEnumerable<StackValueKind> Varpop_Newobj( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, out AbstractMethodSignature signature )
         {
            signature = GetSignatureFromOpCodeInfo( md, codeInfo );
            IEnumerable<ParameterSignature> paramz = signature?.Parameters;
            var refSig = signature as MethodReferenceSignature;
            if ( refSig != null )
            {
               paramz = paramz.Concat( refSig.VarArgsParameters );
            }
            return paramz == null ?
               Empty<StackValueKind>.Enumerable :
               paramz.Select( p => TypeToStackValue( p?.Type ) );
         }

         private static Int32 Varpop_Newobj_Count( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, out AbstractMethodSignature signature )
         {
            signature = GetSignatureFromOpCodeInfo( md, codeInfo );
            return signature?.Parameters?.Count ?? 0;
         }

         private static IEnumerable<StackValueKind> GetStackChangeForReturnOf( AbstractMethodSignature methodSig )
         {
            var retType = methodSig?.ReturnType?.Type;
            var simple = retType as SimpleTypeSignature;
            if ( simple == null || simple.SimpleType != SimpleTypeSignatureKind.Void )
            {
               yield return TypeToStackValue( retType );
            }
         }

         private static Int32 GetStackChangeForReturnOf_Count( AbstractMethodSignature methodSig )
         {
            var retType = methodSig?.ReturnType?.Type;
            var simple = retType as SimpleTypeSignature;
            return simple == null || simple.SimpleType != SimpleTypeSignatureKind.Void ? 1 : 0;
         }

         private static AbstractMethodSignature GetSignatureFromOpCodeInfo( CILMetaData md, OpCodeInfo codeInfo )
         {
            return GetSignatureFromTableIndex( md, ( codeInfo as OpCodeInfoWithTableIndex )?.Operand );
         }

         private static AbstractMethodSignature GetSignatureFromTableIndex( CILMetaData md, TableIndex? tableIndex, Boolean methodSpecAllowed = true )
         {
            // Either method-def or member-ref or method-spec
            AbstractMethodSignature methodSig;
            if ( tableIndex.HasValue )
            {
               var index = tableIndex.Value;
               switch ( index.Table )
               {
                  case Tables.MethodDef:
                     methodSig = md.MethodDefinitions.GetOrNull( index.Index )?.Signature;
                     break;
                  case Tables.MemberRef:
                     methodSig = md.MemberReferences.GetOrNull( index.Index )?.Signature as AbstractMethodSignature;
                     break;
                  case Tables.StandaloneSignature:
                     methodSig = md.StandaloneSignatures.GetOrNull( index.Index )?.Signature as AbstractMethodSignature;
                     break;
                  case Tables.MethodSpec:
                     methodSig = methodSpecAllowed ? // Prevent possible stack overflow in case of malformed data.
                        GetSignatureFromTableIndex( md, md.MethodSpecifications.GetOrNull( index.Index )?.Method, false ) :
                        null;
                     break;
                  default:
                     methodSig = null;
                     break;
               }
            }
            else
            {
               methodSig = null;
            }

            return methodSig;
         }

         private static StackValueKind TypeToStackValue( TypeSignature type )
         {
            if ( type == null )
            {
               return StackValueKind.Ref;
            }
            else
            {
               switch ( type.TypeSignatureKind )
               {
                  case TypeSignatureKind.Simple:
                     var simple = (SimpleTypeSignature) type;
                     switch ( simple.SimpleType )
                     {
                        case SimpleTypeSignatureKind.I4:
                           return StackValueKind.I4;
                        case SimpleTypeSignatureKind.I8:
                           return StackValueKind.I8;
                        case SimpleTypeSignatureKind.R4:
                           return StackValueKind.R4;
                        case SimpleTypeSignatureKind.R8:
                           return StackValueKind.R8;
                        default:
                           return StackValueKind.Value;
                     }
                  case TypeSignatureKind.ClassOrValue:
                     return ( (ClassOrValueTypeSignature) type ).TypeReferenceKind == TypeReferenceKind.Class ? StackValueKind.Ref : StackValueKind.Value;
                  case TypeSignatureKind.GenericParameter:
                  case TypeSignatureKind.FunctionPointer:
                  case TypeSignatureKind.Pointer:
                     return StackValueKind.Value; // ??
                  default:
                     return StackValueKind.Ref;
               }
            }
         }
      }
   }

   /// <summary>
   /// This enumeration describes possible values that can be on the stack when executing IL code.
   /// </summary>
   public enum StackValueKind
   {
      /// <summary>
      /// The value is 32-bit integer.
      /// </summary>
      I4,

      /// <summary>
      /// The value is 64-bit integer.
      /// </summary>
      I8,

      /// <summary>
      /// The value is 32-bit floating point number.
      /// </summary>
      R4,

      /// <summary>
      /// The value is 64-bit floating point number.
      /// </summary>
      R8,

      /// <summary>
      /// The value is value off stack memory.
      /// </summary>
      Value,

      /// <summary>
      /// The value is reference off heap memory.
      /// </summary>
      Ref,
   }



   internal delegate IEnumerable<StackValueKind> DynamicStackChangeDelegate( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, IEnumerable<StackValueKind> currentStack );

   internal delegate Int32 DynamicStackChangeSizeDelegate( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, Int32 currentStack );

   /// <summary>
   /// <para>This class contains all required information about a single CIL op code, except for information related to serialization.
   /// Very similar to System.Reflection.Emit.OpCode struct.
   /// </para>
   /// <para>
   /// In order to get the information related to serialization, see <see cref="T:CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo"/>.
   /// </para>
   /// </summary>
   public sealed class OpCode
   {
      private static class NameCache
      {
         // Currently 219 op codes.
         internal static readonly String[] Cache = new String[219];
      }

      private readonly TDynamicStackChangeParameter _dynamicPop;
      private readonly TDynamicStackChangeParameter _dynamicPush;
      private readonly Lazy<String> _name;

      internal OpCode(
         OpCodeID codeID,
         ArrayQuery<StackValueKind> staticPop,
         ArrayQuery<StackValueKind> staticPush,
         OperandType operand,
         OpCodeType type,
         FlowControl flowControl,
         OpCodeID otherForm = 0
         )
         : this( codeID, staticPop, staticPush, null, null, operand, type, flowControl, otherForm )
      {

      }

      internal OpCode(
         OpCodeID codeID,
         TDynamicStackChangeParameter dynamicPop,
         TDynamicStackChangeParameter dynamicPush,
         OperandType operand,
         OpCodeType type,
         FlowControl flowControl,
         OpCodeID otherForm = 0
         )
         : this( codeID, null, null, dynamicPop, dynamicPush, operand, type, flowControl, otherForm )
      {

      }

      internal OpCode(
         OpCodeID codeID,
         ArrayQuery<StackValueKind> staticPop,
         TDynamicStackChangeParameter dynamicPush,
         OperandType operand,
         OpCodeType type,
         FlowControl flowControl,
         OpCodeID otherForm = 0
         )
         : this( codeID, staticPop, null, null, dynamicPush, operand, type, flowControl, otherForm )
      {

      }

      internal OpCode(
         OpCodeID codeID,
         TDynamicStackChangeParameter dynamicPop,
         ArrayQuery<StackValueKind> staticPush,
         OperandType operand,
         OpCodeType type,
         FlowControl flowControl,
         OpCodeID otherForm = 0
         )
         : this( codeID, null, staticPush, dynamicPop, null, operand, type, flowControl, otherForm )
      {

      }


      private OpCode(
         OpCodeID codeID,
         ArrayQuery<StackValueKind> staticPop,
         ArrayQuery<StackValueKind> staticPush,
         TDynamicStackChangeParameter dynamicPop,
         TDynamicStackChangeParameter dynamicPush,
         OperandType operand,
         OpCodeType type,
         FlowControl flowControl,
         OpCodeID otherForm
         )
      {
         if ( staticPop == null )
         {
            ArgumentValidator.ValidateNotNull( "Dynamic pop", dynamicPop );
            ArgumentValidator.ValidateNotNull( "Dynamic pop 1", dynamicPop.Item1 );
            ArgumentValidator.ValidateNotNull( "Dynamic pop 2", dynamicPop.Item2 );
         }
         else
         {
            dynamicPop = null;
         }

         if ( staticPush == null )
         {
            ArgumentValidator.ValidateNotNull( "Dynamic push", dynamicPush );
            ArgumentValidator.ValidateNotNull( "Dynamic push 1", dynamicPush.Item1 );
            ArgumentValidator.ValidateNotNull( "Dynamic push 2", dynamicPush.Item2 );
         }
         else
         {
            dynamicPush = null;
         }

         this.OpCodeID = codeID;
         this.StaticStackPop = staticPop;
         this.StaticStackPush = staticPush;
         this._dynamicPop = dynamicPop;
         this._dynamicPush = dynamicPush;
         this.OperandType = operand;
         this.OpCodeType = type;
         this.FlowControl = flowControl;
         this.OtherForm = otherForm;
         this._name = new Lazy<String>( () => codeID.ToString( "g" ).ToLowerInvariant().Replace( "_", "." ), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );
      }

      /// <summary>
      /// Gets the textual name of this <see cref="OpCode"/>.
      /// </summary>
      /// <value>The textual name of this <see cref="OpCode"/>.</value>
      public String Name
      {
         get
         {
            return this._name.Value;
         }
      }

      /// <summary>
      /// Gets the <see cref="OperandType"/> of this <see cref="OpCode"/>.
      /// </summary>
      /// <value>The <see cref="OperandType"/> of this <see cref="OpCode"/>.</value>
      public OperandType OperandType { get; }

      /// <summary>
      /// Gets the <see cref="OpCodeType"/> of this <see cref="OpCode"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeType"/> of this <see cref="OpCode"/>.</value>
      public OpCodeType OpCodeType { get; }

      /// <summary>
      /// Gets the <see cref="FlowControl"/> of this <see cref="OpCode"/>.
      /// </summary>
      /// <value>The <see cref="FlowControl"/> of this <see cref="OpCode"/>.</value>
      public FlowControl FlowControl { get; }

      /// <summary>
      /// Gets the static behaviour of popping values off the stack for this <see cref="OpCode"/>.
      /// </summary>
      /// <value>The static behaviour of popping values off the stack for this <see cref="OpCode"/>, or <c>null</c>.</value>
      /// <remarks>
      /// If this returns <c>null</c>, then the pop behaviour is dynamic.
      /// </remarks>
      public ArrayQuery<StackValueKind> StaticStackPop { get; }

      /// <summary>
      /// Gets the static behaviour of pushing values to the stack for this <see cref="OpCode"/>.
      /// </summary>
      /// <value>The static behaviour of pushing values to the stack for this <see cref="OpCode"/>, or <c>null</c>.</value>
      /// <remarks>
      /// If this returns <c>null</c>, then the push behaviour is dynamic.
      /// </remarks>
      public ArrayQuery<StackValueKind> StaticStackPush { get; }

      /// <summary>
      /// Gets enumerable of <see cref="StackValueKind"/>s indicating the pop behaviour for this <see cref="OpCode"/>.
      /// </summary>
      /// <param name="md">The current <see cref="CILMetaData"/>.</param>
      /// <param name="method">The current <see cref="MethodDefinition"/>.</param>
      /// <param name="codeInfo">The current <see cref="OpCodeInfo"/>.</param>
      /// <param name="currentStack">The current stack state.</param>
      /// <returns>The enumerable of <see cref="StackValueKind"/>s indicating the pop behaviour for this <see cref="OpCode"/>, or <c>null</c>.</returns>
      /// <remarks>
      /// If this returns <c>null</c>, then the pop behaviour is static.
      /// </remarks>
      public IEnumerable<StackValueKind> GetDynamicStackPop( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, IEnumerable<StackValueKind> currentStack )
      {
         return this._dynamicPop?.Item1?.Invoke( md, method, codeInfo, currentStack );
      }

      /// <summary>
      /// Gets enumerable of <see cref="StackValueKind"/>s indicating the push behaviour for this <see cref="OpCode"/>.
      /// </summary>
      /// <param name="md">The current <see cref="CILMetaData"/>.</param>
      /// <param name="method">The current <see cref="MethodDefinition"/>.</param>
      /// <param name="codeInfo">The current <see cref="OpCodeInfo"/>.</param>
      /// <param name="currentStack">The current stack state.</param>
      /// <returns>The enumerable of <see cref="StackValueKind"/>s indicating the push behaviour for this <see cref="OpCode"/>, or <c>null</c>.</returns>
      /// <remarks>
      /// If this returns <c>null</c>, then the push behaviour is static.
      /// </remarks>
      public IEnumerable<StackValueKind> GetDynamicStackPush( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, IEnumerable<StackValueKind> currentStack )
      {
         return this._dynamicPush?.Item1?.Invoke( md, method, codeInfo, currentStack );
      }

      /// <summary>
      /// Gets the positive or negative number indicating how this <see cref="OpCode"/> changes stack state.
      /// </summary>
      /// <param name="md">The current <see cref="CILMetaData"/>.</param>
      /// <param name="method">The current <see cref="MethodDefinition"/>.</param>
      /// <param name="codeInfo">The current <see cref="OpCodeInfo"/>.</param>
      /// <param name="currentStack">The current amount of items on stack.</param>
      /// <value>The positive or negative number indicating how this <see cref="OpCode"/> changes stack state.</value>
      public Int32 GetStackChange( CILMetaData md, MethodDefinition method, OpCodeInfo codeInfo, Int32 currentStack )
      {
         var push = this.StaticStackPush?.Count ?? this._dynamicPush.Item2( md, method, codeInfo, currentStack );
         var pop = this.StaticStackPop?.Count ?? this._dynamicPop.Item2( md, method, codeInfo, currentStack );
         return push - pop;
      }

      /// <summary>
      /// Gets the value of this <see cref="OpCode"/> as integer.
      /// </summary>
      /// <value>The value of this <see cref="OpCode"/> as integer.</value>
      public OpCodeID OpCodeID { get; }

      /// <summary>
      /// For short branch instructions, returns their long branch instruction aliases.
      /// For long branch instructions, returns their short instruction aliases.
      /// For others, returns <see cref="OpCodeID.Nop"/>.
      /// </summary>
      public OpCodeID OtherForm { get; }

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
      Dup,
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
      Unbox,
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
      Conv_Ovf_I1,
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
      Refanyval,
      /// <summary>
      /// <see cref="OpCodes.Ckfinite"/>
      /// </summary>
      Ckfinite,
      /// <summary>
      /// <see cref="OpCodes.Mkrefany"/>
      /// </summary>
      Mkrefany,
      /// <summary>
      /// <see cref="OpCodes.Ldtoken"/>
      /// </summary>
      Ldtoken,
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
      Arglist,
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
      Ldarg,
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
      Endfilter,
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
      /// <see cref="OpCodes.No_"/>
      /// </summary>
      No_,
      /// <summary>
      /// <see cref="OpCodes.Rethrow"/>
      /// </summary>
      Rethrow,
      /// <summary>
      /// <see cref="OpCodes.Sizeof"/>
      /// </summary>
      Sizeof,
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
}

public static partial class E_CILPhysical
{
   /// <summary>
   /// Gets the number indicating how many values this <see cref="OpCode"/> pops off the stack.
   /// </summary>
   /// <value>The number indicating how many values this <see cref="OpCode"/> pops off the stack.</value>
   /// <remarks>
   /// If this returns <c>null</c>, then the amount of values that this <see cref="OpCode"/> pops off the stack is dynamic.
   /// </remarks>
   public static Int32? GetStaticStackPopCount( this OpCode code )
   {
      return code.StaticStackPop?.Count;
   }

   /// <summary>
   /// Gets the number indicating how many values this <see cref="OpCode"/> pushes to the stack.
   /// </summary>
   /// <value>The number indicating how many values this <see cref="OpCode"/> pushes to the stack.</value>
   /// <remarks>
   /// If this returns <c>null</c>, then the amount of values that this <see cref="OpCode"/> pushes to the stack is dynamic.
   /// </remarks>
   public static Int32? GetStaticStackPushCount( this OpCode code )
   {
      return code.StaticStackPush?.Count;
   }
}