/*
 * Copyright 2015 Stanislav Muhametsin. All rights Reserved.
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Structural
{
   public sealed class MethodILStructureInfo
   {
      private readonly List<MethodExceptionBlockStructure> _exceptionBlocks;
      private readonly List<OpCodeStructure> _opCodes;

      public MethodILStructureInfo( Int32 exceptionBlockCount = 0, Int32 opCodeCount = 0 )
      {
         this._exceptionBlocks = new List<MethodExceptionBlockStructure>( exceptionBlockCount );
         this._opCodes = new List<OpCodeStructure>( opCodeCount );
      }

      public List<OpCodeStructure> OpCodes
      {
         get
         {
            return this._opCodes;
         }
      }

      public List<MethodExceptionBlockStructure> ExceptionBlocks
      {
         get
         {
            return this._exceptionBlocks;
         }
      }

      public Boolean InitLocals { get; set; }
      public StandaloneSignatureStructure Locals { get; set; }
      public Int32 MaxStackSize { get; set; }
   }

   public sealed class MethodExceptionBlockStructure
   {
      public ExceptionBlockType BlockType { get; set; }
      public Int32 TryOffset { get; set; }
      public Int32 TryLength { get; set; }
      public Int32 HandlerOffset { get; set; }
      public Int32 HandlerLength { get; set; }
      public AbstractTypeStructure ExceptionType { get; set; }
      public Int32 FilterOffset { get; set; }
   }

   public enum OpCodeStructureKind
   {
      Simple,
      Wrapper,
      WithReference
   }

   public abstract class OpCodeStructure
   {
      internal OpCodeStructure()
      {
      }

      public abstract OpCodeStructureKind OpCodeStructureKind { get; }
   }

   public sealed class OpCodeStructureSimple : OpCodeStructure
   {
      private static readonly IDictionary<OpCodeEncoding, OpCodeStructureSimple> CodeInfosWithNoOperand = OpCodes.AllOpCodes
         .Where( c => c.OperandType == OperandType.InlineNone )
         .ToDictionary( c => c.Value, c => new OpCodeStructureSimple( c.Value ) );

      private readonly OpCodeEncoding _opCode;

      private OpCodeStructureSimple( OpCodeEncoding opCode )
      {
         this._opCode = opCode;
      }

      public OpCodeEncoding SimpleOpCode
      {
         get
         {
            return this._opCode;
         }
      }

      public static OpCodeStructureSimple GetInstanceFor( OpCodeEncoding opCode )
      {
         OpCodeStructureSimple retVal;
         if ( CodeInfosWithNoOperand.TryGetValue( opCode, out retVal ) )
         {
            return retVal;
         }
         else
         {
            throw new ArgumentException( "Op code " + opCode + " is not operandless opcode." );
         }
      }

      public override OpCodeStructureKind OpCodeStructureKind
      {
         get
         {
            return OpCodeStructureKind.Simple;
         }
      }
   }

   public sealed class OpCodeStructureWrapper : OpCodeStructure
   {
      public OpCodeInfo PhysicalOpCode { get; set; }

      public override OpCodeStructureKind OpCodeStructureKind
      {
         get
         {
            return OpCodeStructureKind.Wrapper;
         }
      }
   }

   public enum OpCodeStructureTokenKind
   {
      TypeDef,
      TypeRef,
      TypeSpec,
      MethodDef,
      FieldDef,
      MemberRef,
      MethodSpec,
      StandaloneSignature
   }

   public interface StructurePresentInIL
   {
      OpCodeStructureTokenKind StructureTokenKind { get; }
   }

   public sealed class OpCodeStructureWithReference : OpCodeStructure
   {

      public OpCode OpCode { get; set; }
      public StructurePresentInIL Structure { get; set; }

      public override OpCodeStructureKind OpCodeStructureKind
      {
         get
         {
            return OpCodeStructureKind.WithReference;
         }
      }
   }
}
