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
extern alias CAMPhysicalR;
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;
using CAMPhysical::CILAssemblyManipulator.Physical.Meta;

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.Meta;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TabularMetaData;
using TabularMetaData.Meta;

#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{
   /// <summary>
   /// Gets the zero-based metadata token (table + zero-based index value encoded in integer) for this <see cref="TableIndex"/>.
   /// </summary>
   /// <param name="index">The <see cref="TableIndex"/>.</param>
   /// <returns>The zero-based metadata token for this <see cref="TableIndex"/>.</returns>
   public static Int32 GetZeroBasedToken( this TableIndex index )
   {
      return ( index.CombinedValue & CAMCoreInternals.INDEX_MASK ) | ( index.CombinedValue & ~CAMCoreInternals.INDEX_MASK );
   }

   /// <summary>
   /// Gets the one-based metadata token (table + one-based index value encoded in integer) for this <see cref="TableIndex"/>.
   /// </summary>
   /// <param name="index">The <see cref="TableIndex"/>.</param>
   /// <returns>The one-based metadata token for this <see cref="TableIndex"/>.</returns>
   public static Int32 GetOneBasedToken( this TableIndex index )
   {
      return ( ( index.CombinedValue & CAMCoreInternals.INDEX_MASK ) + 1 ) | ( index.CombinedValue & ~CAMCoreInternals.INDEX_MASK );
   }

   /// <summary>
   ///  Gets the one-based metadata token (table + one-based index value encoded in integer) for this nullable <see cref="TableIndex"/>.
   /// </summary>
   /// <param name="tableIdx">The nullable<see cref="TableIndex"/>.</param>
   /// <returns>The one-based metadata token for this <see cref="TableIndex"/>, or <c>0</c> if this nullable <see cref="TableIndex"/> does not have a value.</returns>
   public static Int32 GetOneBasedToken( this TableIndex? tableIdx )
   {
      return tableIdx.HasValue ?
         tableIdx.Value.GetOneBasedToken() :
         0;
   }

   private sealed class StackCalculationState
   {
      private Int32 _maxStack;

      internal StackCalculationState( CILMetaData md, MethodDefinition method, Int32 ilByteCount )
      {
         this.MD = md;
         this.Method = method;
         this.StackSizes = new Int32[ilByteCount];
         this._maxStack = 0;
      }

      public Int32 CurrentStack { get; set; }
      public Int32 CurrentCodeByteOffset { get; set; }
      public Int32 NextCodeByteOffset { get; set; }
      public Int32 MaxStack
      {
         get
         {
            return this._maxStack;
         }
      }

      public CILMetaData MD { get; }

      public MethodDefinition Method { get; }

      public Int32[] StackSizes { get; }

      public void UpdateMaxStack( Int32 newMaxStack )
      {
         if ( this._maxStack < newMaxStack )
         {
            this._maxStack = newMaxStack;
         }
      }
   }

   /// <summary>
   /// This method will calculate the max stack size suitable to use for <see cref="MethodILDefinition.MaxStackSize"/>.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="methodIndex">The zero-based index in <see cref="CILMetaData.MethodDefinitions"/> table for which to calculate max stack size for.</param>
   /// <returns>The max stack size for method in <paramref name="md"/> in table <see cref="CILMetaData.MethodDefinitions"/> at index <paramref name="methodIndex"/>. If <paramref name="methodIndex"/> is invalid or if the <see cref="MethodDefinition"/> does not have IL, returns <c>-1</c>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="md"/> is <c>null</c>.</exception>
   public static Int32 CalculateStackSize( this CILMetaData md, Int32 methodIndex )
   {
      var mDef = md.MethodDefinitions.GetOrNull( methodIndex );
      var retVal = -1;
      if ( mDef != null )
      {
         var il = mDef.IL;
         if ( il != null )
         {

            var ocp = md.OpCodeProvider;
            var state = new StackCalculationState( md, mDef, ocp.GetILByteCount( il.OpCodes ) );

            // Setup exception block stack sizes
            foreach ( var block in il.ExceptionBlocks )
            {
               switch ( block.BlockType )
               {
                  case ExceptionBlockType.Exception:
                     state.StackSizes[block.HandlerOffset] = 1;
                     break;
                  case ExceptionBlockType.Filter:
                     state.StackSizes[block.HandlerOffset] = 1;
                     state.StackSizes[block.FilterOffset] = 1;
                     break;
               }
            }

            // Calculate actual max stack
            foreach ( var codeInfo in il.OpCodes )
            {
               var byteCount = ocp.GetTotalByteCount( codeInfo );
               state.NextCodeByteOffset += byteCount;
               UpdateStackSize( state, codeInfo );
               state.CurrentCodeByteOffset += byteCount;
            }

            retVal = state.MaxStack;
         }
      }

      return retVal;
   }

   private static void UpdateStackSize(
      StackCalculationState state,
      OpCodeInfo codeInfo
      )
   {
      var curStacksize = Math.Max( state.CurrentStack, state.StackSizes[state.CurrentCodeByteOffset] );
      var code = state.MD.OpCodeProvider.GetCodeFor( codeInfo.OpCodeID );
      // Calling GetStackChange will take care of calculating stack change even for Ret/Call/Callvirt/Calli/Newobj codes.
      curStacksize += code.GetStackChange( state.MD, state.Method, codeInfo );

      // Save max stack here
      state.UpdateMaxStack( curStacksize );

      // Copy branch stack size
      if ( curStacksize > 0 )
      {
         switch ( code.OperandType )
         {
            case OperandType.InlineBrTarget:
               UpdateStackSizeAtBranchTarget( state, ( (OpCodeInfoWithInt32) codeInfo ).Operand, curStacksize );
               break;
            case OperandType.ShortInlineBrTarget:
               UpdateStackSizeAtBranchTarget( state, ( (OpCodeInfoWithInt32) codeInfo ).Operand, curStacksize );
               break;
            case OperandType.InlineSwitch:
               var offsets = ( (OpCodeInfoWithIntegers) codeInfo ).Operand;
               for ( var i = 0; i < offsets.Count; ++i )
               {
                  UpdateStackSizeAtBranchTarget( state, offsets[i], curStacksize );
               }
               break;
         }
      }

      // Set stack to zero if required
      if ( code.UnconditionallyEndsBulkOfCode )
      {
         curStacksize = 0;
      }

      // Save current size for next iteration
      state.CurrentStack = curStacksize;
   }

   private static void UpdateStackSizeAtBranchTarget(
      StackCalculationState state,
      Int32 jump,
      Int32 stackSize
      )
   {
      if ( jump >= 0 )
      {
         var idx = state.NextCodeByteOffset + jump;
         state.StackSizes[idx] = Math.Max( state.StackSizes[idx], stackSize );
      }
   }

}