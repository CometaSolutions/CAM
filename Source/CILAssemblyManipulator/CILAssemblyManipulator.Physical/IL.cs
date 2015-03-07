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
   public abstract class OpCodeInfo
   {
      private readonly OpCode _code;

      // Disable inheritance to other assemblies
      internal OpCodeInfo( OpCode code )
      {
         this._code = code;
      }

      public OpCode OpCode
      {
         get
         {
            return this._code;
         }
      }

      public abstract OpCodeOperandKind InfoKind { get; }

      // Returns code size + operand size
      public virtual Int32 ByteSize
      {
         get
         {
            return this._code.Size + this._code.OperandSize;
         }
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
      public OpCodeInfoWithNoOperand( OpCode code )
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
      private readonly IList<Int32> _offsets;

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

      public IList<Int32> Offsets
      {
         get
         {
            return this._offsets;
         }
      }

      public override int ByteSize
      {
         get
         {
            return base.ByteSize + this._offsets.Count * sizeof( Int32 );
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
