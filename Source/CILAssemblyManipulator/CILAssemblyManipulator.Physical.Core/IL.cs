using CILAssemblyManipulator.Physical;
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
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
   public sealed class MethodILDefinition
   {
      private readonly List<MethodExceptionBlock> _exceptionBlocks;
      private readonly List<OpCodeInfo> _opCodes;

      public MethodILDefinition( Int32 exceptionBlockCount = 0, Int32 opCodeCount = 0 )
      {
         this._exceptionBlocks = new List<MethodExceptionBlock>( exceptionBlockCount );
         this._opCodes = new List<OpCodeInfo>( opCodeCount );
      }

      public Boolean InitLocals { get; set; }
      public TableIndex? LocalsSignatureIndex { get; set; }
      public Int32 MaxStackSize { get; set; }

      public List<MethodExceptionBlock> ExceptionBlocks
      {
         get
         {
            return this._exceptionBlocks;
         }
      }

      public List<OpCodeInfo> OpCodes
      {
         get
         {
            return this._opCodes;
         }
      }
   }

   public sealed class MethodExceptionBlock
   {
      public ExceptionBlockType BlockType { get; set; }
      public Int32 TryOffset { get; set; }
      public Int32 TryLength { get; set; }
      public Int32 HandlerOffset { get; set; }
      public Int32 HandlerLength { get; set; }
      public TableIndex? ExceptionType { get; set; }
      public Int32 FilterOffset { get; set; }
   }


   public abstract class OpCodeInfo
   {
      private readonly UInt16 _code; // Save some memory - use integer instead of actual code (Int64 -> Int16)

      // Disable inheritance to other assemblies
      internal OpCodeInfo( OpCode code )
      {
         this._code = (UInt16) code.Value;
      }

      public OpCode OpCode
      {
         get
         {
            return OpCodes.GetCodeFor( (OpCodeEncoding) this._code );
         }
      }

      public abstract OpCodeOperandKind InfoKind { get; }

      public virtual Int32 OperandByteSize
      {
         get
         {
            return this.OpCode.OperandSize;
         }
      }

      public override String ToString()
      {
         return this.OpCode.ToString();
      }

   }

   public abstract class OpCodeInfoWithOperand<TOperand> : OpCodeInfo
   {
      // Disable inheritance to other assemblies
      internal OpCodeInfoWithOperand( OpCode code, TOperand operand )
         : base( code )
      {
         this.Operand = operand;
      }

      public TOperand Operand { get; set; }
   }

   public sealed class OpCodeInfoWithToken : OpCodeInfoWithOperand<TableIndex>
   {
      public OpCodeInfoWithToken( OpCode code, TableIndex token )
         : base( code, token )
      {
      }

      public override OpCodeOperandKind InfoKind
      {
         get
         {
            return OpCodeOperandKind.OperandToken;
         }
      }
   }

   public sealed class OpCodeInfoWithInt32 : OpCodeInfoWithOperand<Int32>
   {
      public OpCodeInfoWithInt32( OpCode code, Int32 operand )
         : base( code, operand )
      {
      }

      public override OpCodeOperandKind InfoKind
      {
         get
         {
            return OpCodeOperandKind.OperandInteger;
         }
      }
   }

   public sealed class OpCodeInfoWithInt64 : OpCodeInfoWithOperand<Int64>
   {
      public OpCodeInfoWithInt64( OpCode code, Int64 operand )
         : base( code, operand )
      {

      }

      public override OpCodeOperandKind InfoKind
      {
         get
         {
            return OpCodeOperandKind.OperandInteger64;
         }
      }
   }

   public sealed class OpCodeInfoWithNoOperand : OpCodeInfo
   {
      private static readonly IDictionary<OpCodeEncoding, OpCodeInfoWithNoOperand> CodeInfosWithNoOperand = OpCodes.AllOpCodes
         .Where( c => c.OperandType == OperandType.InlineNone )
         .ToDictionary( c => c.Value, c => new OpCodeInfoWithNoOperand( c ) );


      private OpCodeInfoWithNoOperand( OpCode code )
         : base( code )
      {

      }

      public override OpCodeOperandKind InfoKind
      {
         get
         {
            return OpCodeOperandKind.OperandNone;
         }
      }

      public override Int32 OperandByteSize
      {
         get
         {
            return 0;
         }
      }

      public static OpCodeInfoWithNoOperand GetInstanceFor( OpCodeEncoding encoded )
      {
         OpCodeInfoWithNoOperand retVal;
         if ( CodeInfosWithNoOperand.TryGetValue( encoded, out retVal ) )
         {
            return retVal;
         }
         else
         {
            throw new ArgumentException( "Op code " + encoded + " is not operandless opcode." );
         }
      }

      public static OpCodeInfoWithNoOperand GetInstanceFor( OpCode code )
      {
         return GetInstanceFor( code.Value );
      }

      public static IEnumerable<OpCodeEncoding> OperandlessCodes
      {
         get
         {
            return CodeInfosWithNoOperand.Keys;
         }
      }
   }

   public sealed class OpCodeInfoWithDouble : OpCodeInfoWithOperand<Double>
   {
      public OpCodeInfoWithDouble( OpCode code, Double operand )
         : base( code, operand )
      {

      }

      public override OpCodeOperandKind InfoKind
      {
         get
         {
            return OpCodeOperandKind.OperandR8;
         }
      }
   }

   public sealed class OpCodeInfoWithString : OpCodeInfoWithOperand<String>
   {
      public OpCodeInfoWithString( OpCode code, String operand )
         : base( code, operand )
      {

      }

      public override OpCodeOperandKind InfoKind
      {
         get
         {
            return OpCodeOperandKind.OperandString;
         }
      }
   }

   public sealed class OpCodeInfoWithSwitch : OpCodeInfo
   {
      private readonly List<Int32> _offsets;

      public OpCodeInfoWithSwitch( OpCode code, Int32 offsetsCount = 0 )
         : base( code )
      {
         this._offsets = new List<Int32>( offsetsCount );
      }

      public override OpCodeOperandKind InfoKind
      {
         get
         {
            return OpCodeOperandKind.OperandSwitch;
         }
      }

      public List<Int32> Offsets
      {
         get
         {
            return this._offsets;
         }
      }

      public override Int32 OperandByteSize
      {
         get
         {
            return base.OperandByteSize + this._offsets.Count * sizeof( Int32 );
         }
      }
   }

   public sealed class OpCodeInfoWithSingle : OpCodeInfoWithOperand<Single>
   {
      public OpCodeInfoWithSingle( OpCode code, Single operand )
         : base( code, operand )
      {

      }

      public override OpCodeOperandKind InfoKind
      {
         get
         {
            return OpCodeOperandKind.OperandR4;
         }
      }
   }

   public enum OpCodeOperandKind
   {
      /// <summary>No operand.</summary>
      OperandNone,
      /// <summary>The operand is a 32-bit metadata token.</summary>
      OperandToken,
      /// <summary>The operand is a 32-bit or 16-bit or 8-bit integer.</summary>
      OperandInteger,
      /// <summary>The operand is a 64-bit integer.</summary>
      OperandInteger64,
      /// <summary>The operand is a 64-bit IEEE floating point number.</summary>
      OperandR8,
      /// <summary>The operand is a 32-bit IEEE floating point number.</summary>
      OperandR4,
      /// <summary>The operand is a string.</summary>
      OperandString,
      /// <summary>The operand is the 32-bit integer argument to a switch instruction.</summary>
      OperandSwitch,
   }
}

public static partial class E_CILPhysical
{
   public static Int32 GetTotalByteCount( this OpCodeInfo info )
   {
      return info.OpCode.Size + info.OperandByteSize;
   }

   public static void SortExceptionBlocks( this MethodILDefinition il )
   {
      il.ExceptionBlocks.Sort( ( x, y ) =>
      {
         // Return -1 if x is inner block of y, 0 if they are same, 1 if x is not inner block of y
         return Object.ReferenceEquals( x, y ) ? 0 :
            ( x.TryOffset >= y.HandlerOffset + y.HandlerLength || ( x.TryOffset <= y.TryOffset && x.HandlerOffset + x.HandlerLength > y.HandlerOffset + y.HandlerLength ) ? 1 : -1 );
      } );
   }
}