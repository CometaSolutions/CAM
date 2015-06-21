﻿/*
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

      public static OpCodeInfo ReadFromBytes(
         Byte[] bytes,
         Func<Int32, String> stringGetter
         )
      {
         var idx = 0;
         return ReadFromBytes( bytes, ref idx, stringGetter );
      }

      public static OpCodeInfo ReadFromBytesNoRef(
         Byte[] bytes,
         Int32 idx,
         Func<Int32, String> stringGetter
         )
      {
         return ReadFromBytes( bytes, ref idx, stringGetter );
      }

      public static OpCodeInfo ReadFromBytes(
         Byte[] bytes,
         ref Int32 idx,
         Func<Int32, String> stringGetter
         )
      {
         using ( var stream = new MemoryStream( bytes ) )
         {
            stream.Position = idx;
            Int32 byteCount;
            var retVal = ReadFromStream( stream, stringGetter, out byteCount );
            idx += byteCount;
            return retVal;
         }
      }

      public static OpCodeInfo ReadFromStream(
         Stream stream,
         Func<Int32, String> stringGetter,
         out Int32 byteCount
         )
      {
         return ReadFromStream( stream, new Byte[8], stringGetter, out byteCount );
      }

      internal static OpCodeInfo ReadFromStream(
         Stream stream,
         Byte[] tmpArray,
         Func<Int32, String> stringGetter,
         out Int32 byteCount
         )
      {
         var curInstruction = (Int32) stream.ReadByteFromStream();
         byteCount = 1;
         if ( curInstruction == OpCode.MAX_ONE_BYTE_INSTRUCTION )
         {
            curInstruction = ( curInstruction << 8 ) | (Int32) stream.ReadByteFromStream();
            ++byteCount;
         }

         var code = OpCodes.Codes[(OpCodeEncoding) curInstruction];
         OpCodeInfo info;

         switch ( code.OperandType )
         {
            case OperandType.InlineNone:
               info = OpCodeInfoWithNoOperand.GetInstanceFor( (OpCodeEncoding) curInstruction );
               break;
            case OperandType.ShortInlineBrTarget:
            case OperandType.ShortInlineI:
               byteCount += sizeof( Byte );
               info = new OpCodeInfoWithInt32( code, (Int32) ( (SByte) stream.ReadByteFromStream() ) );
               break;
            case OperandType.ShortInlineVar:
               byteCount += sizeof( Byte );
               info = new OpCodeInfoWithInt32( code, stream.ReadByteFromStream() );
               break;
            case OperandType.ShortInlineR:
               stream.ReadSpecificAmount( tmpArray, 0, sizeof( Single ) );
               byteCount += sizeof( Single );
               info = new OpCodeInfoWithSingle( code, tmpArray.ReadSingleLEFromBytesNoRef( 0 ) );
               break;
            case OperandType.InlineBrTarget:
            case OperandType.InlineI:
               byteCount += sizeof( Int32 );
               info = new OpCodeInfoWithInt32( code, stream.ReadI32( tmpArray ) );
               break;
            case OperandType.InlineVar:
               byteCount += sizeof( Int16 );
               info = new OpCodeInfoWithInt32( code, stream.ReadU16( tmpArray ) );
               break;
            case OperandType.InlineR:
               byteCount += sizeof( Double );
               stream.ReadSpecificAmount( tmpArray, 0, sizeof( Double ) );
               info = new OpCodeInfoWithDouble( code, tmpArray.ReadDoubleLEFromBytesNoRef( 0 ) );
               break;
            case OperandType.InlineI8:
               byteCount += sizeof( Int64 );
               stream.ReadSpecificAmount( tmpArray, 0, sizeof( Int64 ) );
               info = new OpCodeInfoWithInt64( code, tmpArray.ReadInt64LEFromBytesNoRef( 0 ) );
               break;
            case OperandType.InlineString:
               byteCount += sizeof( Int32 );
               info = new OpCodeInfoWithString( code, stringGetter( stream.ReadI32( tmpArray ) ) );
               break;
            case OperandType.InlineField:
            case OperandType.InlineMethod:
            case OperandType.InlineType:
            case OperandType.InlineTok:
            case OperandType.InlineSig:
               byteCount += sizeof( Int32 );
               info = new OpCodeInfoWithToken( code, TableIndex.FromOneBasedToken( stream.ReadI32( tmpArray ) ) );
               break;
            case OperandType.InlineSwitch:
               var count = stream.ReadI32( tmpArray );
               byteCount += sizeof( Int32 ) + count * sizeof( Int32 );
               var sInfo = new OpCodeInfoWithSwitch( code, count );
               for ( var i = 0; i < count; ++i )
               {
                  sInfo.Offsets.Add( stream.ReadI32( tmpArray ) );
               }
               info = sInfo;
               break;
            default:
               throw new ArgumentException( "Unknown operand type: " + code.OperandType + " for " + code + "." );
         }

         return info;
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

      public override Int32 ByteSize
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
