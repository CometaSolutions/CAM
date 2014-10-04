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
using CILAssemblyManipulator.API;
using CILAssemblyManipulator.Implementation;
using CommonUtils;

namespace CILAssemblyManipulator.Implementation.Physical
{
   internal class MethodILWriter
   {
      private struct LabelEmittingInfo
      {
         internal Int32 byteOffset;
         internal Int32 labelIdx;
         internal Int32 startCountOffset;
      }

      private const Int32 MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION = 20;
      private const Int32 METHOD_DATA_SECTION_SIZE = 4;

      private readonly CILMethodBase _method;
      private readonly MethodILImpl _methodIL;
      private readonly MetaDataWriter _metaData;
      private readonly Byte[] _ilCode;
      private EmittingAssemblyMapper _assemblyMapper;
      private Int32 _ilCodeCount;
      private Int32 _methodILOffset;
      private readonly Int32[] _opCodeInfoOffsets;
      private readonly LabelEmittingInfo[] _labelInfos;
      private Int32 _labelInfoIndex;
      private readonly IDictionary<Int32, Int32> _stackSizes; // TODO make this into a dictionary
      private Int32 _currentStack;
      private Int32 _maxStack;

      internal MethodILWriter( CILReflectionContextImpl ctx, MetaDataWriter md, CILMethodBase method, EmittingAssemblyMapper mapper )
      {
         var methodIL = (MethodILImpl) method.MethodIL;

         this._method = method;
         this._methodIL = methodIL;
         this._metaData = md;
         this._assemblyMapper = mapper;
         this._ilCode = new Byte[methodIL._opCodes.Sum( info => info.MaxSize )];
         this._ilCodeCount = 0;
         this._methodILOffset = 0;
         this._opCodeInfoOffsets = new Int32[methodIL._opCodes.Count];
         this._labelInfos = new LabelEmittingInfo[methodIL._branchTargetsCount];
         this._labelInfoIndex = 0;

         this._stackSizes = new Dictionary<Int32, Int32>();
         this._currentStack = 0;
         this._maxStack = 0;
      }

      internal void Emit( OpCode code )
      {
         this.BeforeEmittingOpCode();
         this.EmitOpCode( code );
         this.UpdateStackSize( code, null, null );
      }

      internal void Emit( OpCode code, CILTypeBase type, Boolean useGDefIfPossible )
      {
         ArgumentValidator.ValidateNotNull( "Type", type );
         this.BeforeEmittingOpCode();
         this.EmitOpCode( code );
         this.UpdateStackSize( code, null, null );
         this.AddInt32( this._metaData.GetTokenFor( this._assemblyMapper.TryMapTypeBase( type ), useGDefIfPossible && code.OperandType == OperandType.InlineTok ) );
      }

      internal void Emit( OpCode code, CILField field, Boolean useGDefIfPossible )
      {
         ArgumentValidator.ValidateNotNull( "Field", field );
         this.BeforeEmittingOpCode();
         this.EmitOpCode( code );
         this.UpdateStackSize( code, null, null );
         this.AddInt32( this._metaData.GetTokenFor( this._assemblyMapper.TryMapField( field ), useGDefIfPossible && code.OperandType == OperandType.InlineTok ) );
      }

      internal void Emit( OpCode code, CILMethod method, Boolean useGDefIfPossible )
      {
         this.EmitWithMethod( code, this._assemblyMapper.TryMapMethod( method ), useGDefIfPossible );
      }

      internal void Emit( OpCode code, CILConstructor ctor, Boolean useGDefIfPossible )
      {
         ArgumentValidator.ValidateNotNull( "Constructor", ctor );
         ctor = this._assemblyMapper.TryMapConstructor( ctor );
         this.BeforeEmittingOpCode();
         this.EmitOpCode( code );
         this.UpdateStackSize( code, ctor, null );
         this.AddInt32( this._metaData.GetTokenFor( ctor, useGDefIfPossible && code.OperandType == OperandType.InlineTok ) );
      }

      internal void EmitNormalOrVirtual( CILMethod method, OpCode normalCode, OpCode virtualCode )
      {
         method = this._assemblyMapper.TryMapMethod( method );
         this.EmitWithMethod( method.Attributes.IsVirtual() && !method.DeclaringType.IsValueType() ? virtualCode : normalCode, method, false );
      }

      private void EmitWithMethod( OpCode code, CILMethod method, Boolean useGDefIfPossible )
      {
         ArgumentValidator.ValidateNotNull( "Method", method );
         this.BeforeEmittingOpCode();
         this.EmitOpCode( code );
         this.UpdateStackSize( code, method, null );
         this.AddInt32( this._metaData.GetTokenFor( method, useGDefIfPossible && code.OperandType == OperandType.InlineTok ) );
      }

      internal void Emit( OpCode code, String str )
      {
         ArgumentValidator.ValidateNotNull( "String", str );
         this.BeforeEmittingOpCode();
         this.EmitOpCode( code );
         this.UpdateStackSize( code, null, null );
         this.AddInt32( this._metaData.GetTokenFor( str ) );
      }

      internal void Emit( OpCode code, Byte aByte )
      {
         this.BeforeEmittingOpCode();
         this.EmitOpCode( code );
         this.UpdateStackSize( code, null, null );
         this._ilCode[this._ilCodeCount++] = aByte;
      }

      internal void EmitAsSByte( OpCode code, Int32 sByte )
      {
         this.BeforeEmittingOpCode();
         this.EmitOpCode( code );
         this.UpdateStackSize( code, null, null );
         this._ilCode.WriteSByteToBytes( ref this._ilCodeCount, sByte );
      }

      internal void Emit( OpCode code, Int32 int32 )
      {
         this.BeforeEmittingOpCode();
         this.EmitOpCode( code );
         this.UpdateStackSize( code, null, null );
         this._ilCode.WriteInt32LEToBytes( ref this._ilCodeCount, int32 );
      }

      internal void Emit( OpCode code, Int16 int16 )
      {
         this.BeforeEmittingOpCode();
         this.EmitOpCode( code );
         this.UpdateStackSize( code, null, null );
         this._ilCode.WriteInt16LEToBytes( ref this._ilCodeCount, int16 );
      }

      internal void Emit( OpCode code, Int64 int64 )
      {
         this.BeforeEmittingOpCode();
         this.EmitOpCode( code );
         this.UpdateStackSize( code, null, null );
         this._ilCode.WriteInt64LEToBytes( ref this._ilCodeCount, int64 );
      }

      internal void Emit( OpCode code, Single single )
      {
         this.BeforeEmittingOpCode();
         this.EmitOpCode( code );
         this.UpdateStackSize( code, null, null );
         this._ilCode.WriteSingleLEToBytes( ref this._ilCodeCount, single );
      }

      internal void Emit( OpCode code, Double aDouble )
      {
         this.BeforeEmittingOpCode();
         this.EmitOpCode( code );
         this.UpdateStackSize( code, null, null );
         this._ilCode.WriteDoubleLEToBytes( ref this._ilCodeCount, aDouble );
      }

      internal void Emit( OpCode code, ILLabel label )
      {
         this.BeforeEmittingOpCode();
         this.EmitOpCode( code );
         this.UpdateStackSize( code, null, label );
         this.UpdateLabelInfos( label, OperandType.ShortInlineBrTarget == code.OperandType ? 1 : 4, true );
      }

      internal void Emit( OpCode code, ILLabel[] labels )
      {
         this.BeforeEmittingOpCode();
         this.EmitOpCode( code );
         this.UpdateStackSize( code, null, labels );
         this.AddInt32( labels.Length );
         var max = labels.Length * 4;

         for ( var i = 0; i < labels.Length; ++i )
         {
            this.UpdateLabelInfos( labels[i], max, false );
            this._ilCodeCount += 4;
            max -= 4;
         }
      }

      internal void Emit( CILMethodSignature methodSig, Tuple<CILCustomModifier[], CILTypeBase>[] varArgs )
      {
         this.BeforeEmittingOpCode();
         this.EmitOpCode( OpCodes.Calli );
         this.UpdateStackSize( OpCodes.Calli, methodSig, varArgs );
         this.AddInt32( this._metaData.GetSignatureTokenFor( methodSig, varArgs ) );
      }

      internal Int32 GetMaxForLabel( ILLabel label )
      {
         return this.GetMaxForLabel( label, this._methodILOffset, this._methodILOffset );
      }

      private Int32 GetMaxForLabel( ILLabel label, Int32 startingOpCodeOffset, Int32 opCodeOffset, Boolean checkForBranchingCtrlFlows = true )
      {
         Int32 max;
         var labelOffset = this._methodIL._labelOffsets[label.labelIdx];
         if ( checkForBranchingCtrlFlows && labelOffset < startingOpCodeOffset )
         {
            // Branching backwards into already emitted part - just calculate the jump from
            // this offset + 1-byte IL instruction + 1 byte data
            // to
            // target
            max = this._ilCodeCount + 2 - this._opCodeInfoOffsets[labelOffset];
         }
         else
         {
            max = 0;
            var delta = labelOffset < opCodeOffset ? -1 : 1;
            var i = opCodeOffset + ( delta > 0 ? 1 : 0 );
            var minI = delta > 0 ? i : labelOffset;
            var maxI = ( delta > 0 ? labelOffset : i );
            while ( i >= minI && i <= maxI )
            {
               var info = this._methodIL._opCodes[i];
               var cf = checkForBranchingCtrlFlows ? info as OpCodeInfoForBranchingControlFlow : null;
               if ( checkForBranchingCtrlFlows && cf != null )
               {
                  var max2 = this.GetMaxForLabel( cf._targetLabel, startingOpCodeOffset, i, false );
                  max += max2 <= SByte.MaxValue ? info.MinSize : info.MaxSize;
               }
               else
               {
                  max += info.MaxSize;
               }
               i += delta;
            }
         }
         return max;
      }

      internal Byte[] PerformEmitting( UInt32 currentOffset, out Boolean isTiny )
      {
         if ( this._methodIL._labelOffsets.Any( offset => offset == MethodILImpl.NO_OFFSET ) )
         {
            throw new InvalidOperationException( "Not all labels have been marked." );
         }
         if ( this._methodIL._currentExceptionBlocks.Any() )
         {
            throw new InvalidOperationException( "Not all exception blocks have been completed." );
         }

         // Remember that inner exception blocks must precede outer ones
         var allExceptionBlocksCorrectlyOrdered = this._methodIL._allExceptionBlocks.ToArray();
         Array.Sort(
            allExceptionBlocksCorrectlyOrdered,
            ( item1, item2 ) =>
            {
               // Return -1 if item1 is inner block of item2, 0 if they are same, 1 if item1 is not inner block of item2
               return Object.ReferenceEquals( item1, item2 ) ? 0 :
                  ( item1._tryOffset >= item2._handlerOffset + item2._handlerLength || ( item1._tryOffset <= item2._tryOffset && item1._handlerOffset + item1._handlerLength > item2._handlerOffset + item2._handlerLength ) ? 1 : -1 );
            } );

         // Setup stack sizes based on exception blocks
         foreach ( var block in allExceptionBlocksCorrectlyOrdered )
         {
            switch ( block._blockType )
            {
               case ExceptionBlockType.Exception:
                  this._stackSizes[block._handlerOffset] = 1;
                  break;
               case ExceptionBlockType.Filter:
                  this._stackSizes[block._handlerOffset] = 1;
                  this._stackSizes[block._filterOffset] = 1;
                  break;
            }
         }

         // Emit opcodes and arguments
         foreach ( var info in this._methodIL._opCodes )
         {
            info.EmitOpCode( this );
            ++this._methodILOffset;
         }

         // Mark label targets
         for ( var i = 0; i < this._labelInfoIndex; ++i )
         {
            var thisOffset = this._labelInfos[i].byteOffset;
            var startCountOffset = this._labelInfos[i].startCountOffset;
            var amountToJump = this._opCodeInfoOffsets[this._methodIL._labelOffsets[this._labelInfos[i].labelIdx]] - ( thisOffset + startCountOffset );
            if ( startCountOffset == 1 )
            {
               if ( amountToJump >= SByte.MinValue && amountToJump <= SByte.MaxValue )
               {
                  this._ilCode.WriteSByteToBytes( ref thisOffset, amountToJump );
               }
               else
               {
                  throw new InvalidOperationException( "Tried to use one-byte branch instruction for offset of amount " + amountToJump );
               }
            }
            else
            {
               this._ilCode.WriteInt32LEToBytes( ref thisOffset, amountToJump );
            }
         }

         // Create exception blocks with byte offsets
         byte[][] exceptionBlocks = new byte[allExceptionBlocksCorrectlyOrdered.Length][];
         Boolean[] exceptionFormats = new Boolean[exceptionBlocks.Length];

         // TODO PEVerify doesn't like mixed small and fat blocks at all (however, at least Cecil understands that kind of situation)
         // TODO Apparently, PEVerify doesn't like multiple small blocks either (Cecil still loads code fine)
         // Also, because of exception block ordering, it is easier to do this way.
         var allAreSmall = allExceptionBlocksCorrectlyOrdered.Length <= MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION
            && allExceptionBlocksCorrectlyOrdered.All( excBlock =>
         {
            var tryOffset = this._opCodeInfoOffsets[excBlock._tryOffset];
            var tryLength = this._opCodeInfoOffsets[excBlock._tryOffset + excBlock._tryLength] - tryOffset;
            var handlerOffset = this._opCodeInfoOffsets[excBlock._handlerOffset];
            var handlerLength = this._opCodeInfoOffsets[excBlock._handlerOffset + excBlock._handlerLength] - handlerOffset;
            return tryLength <= Byte.MaxValue && handlerLength <= Byte.MaxValue && tryOffset <= UInt16.MaxValue && handlerOffset <= UInt16.MaxValue;
         } );

         for ( var i = 0; i < exceptionBlocks.Length; ++i )
         {
            // ECMA-335, pp. 286-287
            var block = allExceptionBlocksCorrectlyOrdered[i];
            Int32 idx = 0;
            Byte[] array;
            var tryOffset = this._opCodeInfoOffsets[block._tryOffset];
            var tryLength = this._opCodeInfoOffsets[block._tryOffset + block._tryLength] - tryOffset;
            var handlerOffset = this._opCodeInfoOffsets[block._handlerOffset];
            var handlerLength = this._opCodeInfoOffsets[block._handlerOffset + block._handlerLength] - handlerOffset;
            var useSmallFormat = allAreSmall &&
               tryLength <= Byte.MaxValue && handlerLength <= Byte.MaxValue && tryOffset <= UInt16.MaxValue && handlerOffset <= UInt16.MaxValue;
            exceptionFormats[i] = useSmallFormat;
            if ( useSmallFormat )
            {
               array = new Byte[12];
               array.WriteInt16LEToBytes( ref idx, (Int16) block._blockType )
                  .WriteUInt16LEToBytes( ref idx, (UInt16) tryOffset )
                  .WriteByteToBytes( ref idx, (Byte) tryLength )
                  .WriteUInt16LEToBytes( ref idx, (UInt16) handlerOffset )
                  .WriteByteToBytes( ref idx, (Byte) handlerLength );
            }
            else
            {
               array = new Byte[24];
               array.WriteInt32LEToBytes( ref idx, (Int32) block._blockType )
                  .WriteInt32LEToBytes( ref idx, tryOffset )
                  .WriteInt32LEToBytes( ref idx, tryLength )
                  .WriteInt32LEToBytes( ref idx, handlerOffset )
                  .WriteInt32LEToBytes( ref idx, handlerLength );
            }

            if ( ExceptionBlockType.Exception == block._blockType )
            {
               array.WriteInt32LEToBytes( ref idx, this._metaData.GetTokenFor( this._assemblyMapper == null ? block._exceptionType : this._assemblyMapper.MapTypeBase( block._exceptionType ), false ) );
            }
            else if ( ExceptionBlockType.Filter == block._blockType )
            {
               array.WriteInt32LEToBytes( ref idx, block._filterOffset );
            }
            exceptionBlocks[i] = array;
         }

         // Write method header, extra data sections, and IL
         Byte[] result;
         isTiny = this._ilCodeCount < 64
            && exceptionBlocks.Length == 0
            && this._maxStack <= 8
            && this._methodIL._locals.Count == 0;
         var resultIndex = 0;
         var hasAnyExc = false;
         var hasSmallExc = false;
         var hasLargExc = false;
         var smallExcCount = 0;
         var largeExcCount = 0;
         var amountToNext4ByteBoundary = 0;
         if ( isTiny )
         {
            // Can use tiny header
            result = new Byte[this._ilCodeCount + 1];
            result[resultIndex++] = (Byte) ( (Int32) MethodHeaderFlags.TinyFormat | ( this._ilCodeCount << 2 ) );
         }
         else
         {
            // Use fat header
            hasAnyExc = exceptionBlocks.Length > 0;
            hasSmallExc = hasAnyExc && exceptionFormats.Any( excFormat => excFormat );
            hasLargExc = hasAnyExc && exceptionFormats.Any( excFormat => !excFormat );
            smallExcCount = hasSmallExc ? exceptionFormats.Count( excFormat => excFormat ) : 0;
            largeExcCount = hasLargExc ? exceptionFormats.Count( excFormat => !excFormat ) : 0;
            var offsetAfterIL = (Int32) ( BitUtils.MultipleOf4( currentOffset ) + 12 + (UInt32) this._ilCodeCount );
            amountToNext4ByteBoundary = BitUtils.MultipleOf4( offsetAfterIL ) - offsetAfterIL;

            result = new Byte[12
               + this._ilCodeCount +
               ( hasAnyExc ? amountToNext4ByteBoundary : 0 ) +
               ( hasSmallExc ? METHOD_DATA_SECTION_SIZE : 0 ) +
               ( hasLargExc ? METHOD_DATA_SECTION_SIZE : 0 ) +
               smallExcCount * 12 +
               ( smallExcCount / MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION ) * METHOD_DATA_SECTION_SIZE + // (Amount of extra section headers ) * section size
               largeExcCount * 24
               ];
            var flags = MethodHeaderFlags.FatFormat;
            if ( hasAnyExc )
            {
               flags |= MethodHeaderFlags.MoreSections;
            }
            if ( this._methodIL.InitLocals )
            {
               flags |= MethodHeaderFlags.InitLocals;
            }

            result.WriteInt16LEToBytes( ref resultIndex, (Int16) ( ( (Int32) flags ) | ( 3 << 12 ) ) )
               .WriteInt16LEToBytes( ref resultIndex, (Int16) this._maxStack )
               .WriteInt32LEToBytes( ref resultIndex, this._ilCodeCount )
               .WriteInt32LEToBytes( ref resultIndex, this._metaData.GetSignatureTokenFor( this._method, this._methodIL._locals.ToArray() ) );
         }

         Array.Copy( this._ilCode, 0, result, resultIndex, this._ilCodeCount );
         resultIndex += this._ilCodeCount;

         if ( hasAnyExc )
         {
            var processedIndices = new HashSet<Int32>();
            resultIndex += amountToNext4ByteBoundary;
            var flags = MethodDataFlags.ExceptionHandling;
            // First, write fat sections
            if ( hasLargExc )
            {
               // TODO like with small sections, what if too many exception clauses to be fit into DataSize?
               flags |= MethodDataFlags.FatFormat;
               if ( hasSmallExc )
               {
                  flags |= MethodDataFlags.MoreSections;
               }
               result.WriteByteToBytes( ref resultIndex, (Byte) flags )
                  .WriteInt32LEToBytes( ref resultIndex, largeExcCount * 24 + METHOD_DATA_SECTION_SIZE );
               --resultIndex;
               for ( var i = 0; i < exceptionBlocks.Length; ++i )
               {
                  if ( !exceptionFormats[i] && processedIndices.Add( i ) )
                  {
                     var length = exceptionBlocks[i].Length;
                     Array.Copy( exceptionBlocks[i], 0, result, resultIndex, length );
                     resultIndex += length;
                  }
               }
            }
            // Then, write small sections
            // If exception counts * 12 + 4 are > Byte.MaxValue, have to write several sections
            // (Max 20 handlers per section)
            flags = MethodDataFlags.ExceptionHandling;
            if ( hasSmallExc )
            {
               var curSmallIdx = 0;
               while ( smallExcCount > 0 )
               {
                  var amountToBeWritten = Math.Min( smallExcCount, MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION );
                  if ( amountToBeWritten < smallExcCount )
                  {
                     flags |= MethodDataFlags.MoreSections;
                  }
                  else
                  {
                     flags = flags & ~( MethodDataFlags.MoreSections );
                  }

                  result.WriteByteToBytes( ref resultIndex, (Byte) flags )
                     .WriteByteToBytes( ref resultIndex, (Byte) ( amountToBeWritten * 12 + METHOD_DATA_SECTION_SIZE ) )
                     .WriteInt16LEToBytes( ref resultIndex, 0 );
                  var amountActuallyWritten = 0;
                  while ( curSmallIdx < exceptionBlocks.Length && amountActuallyWritten < amountToBeWritten )
                  {
                     if ( exceptionFormats[curSmallIdx] )
                     {
                        var length = exceptionBlocks[curSmallIdx].Length;
                        Array.Copy( exceptionBlocks[curSmallIdx], 0, result, resultIndex, length );
                        resultIndex += length;
                        ++amountActuallyWritten;
                     }
                     ++curSmallIdx;
                  }
                  smallExcCount -= amountToBeWritten;
               }
            }

         }
#if DEBUG
         if ( resultIndex != result.Length )
         {
            throw new Exception( "Something went wrong when emitting method headers and body. Emitted " + resultIndex + " bytes, but was supposed to emit " + result.Length + " bytes." );
         }
#endif
         return result;
      }

      private void BeforeEmittingOpCode()
      {
         this._opCodeInfoOffsets[this._methodILOffset] = this._ilCodeCount;
      }

      private void EmitOpCode( OpCode code )
      {
         if ( code.Size > 1 )
         {
            this._ilCode[this._ilCodeCount++] = code.Byte1;
         }
         this._ilCode[this._ilCodeCount++] = code.Byte2;
      }

      private void AddInt32( Int32 int32 )
      {
         this._ilCode.WriteInt32LEToBytes( ref this._ilCodeCount, int32 );
      }

      private void UpdateLabelInfos( ILLabel label, Int32 offsetTillCountStarts, Boolean updateSize )
      {
         this._labelInfos[this._labelInfoIndex].byteOffset = this._ilCodeCount;
         this._labelInfos[this._labelInfoIndex].labelIdx = label.labelIdx;
         this._labelInfos[this._labelInfoIndex].startCountOffset = offsetTillCountStarts;
         ++this._labelInfoIndex;
         if ( updateSize )
         {
            this._ilCodeCount += offsetTillCountStarts;
         }
      }

      private void UpdateStackSize( OpCode code, Object possibleMethod, Object labelOrManyLabelsOrVarArgs )
      {
         var curStacksize = Math.Max( this._currentStack, this._stackSizes.GetOrDefault( this._methodILOffset, 0 ) );
         if ( FlowControl.Call == code.FlowControl )
         {
            var isMB = possibleMethod as CILMethodBase;
            if ( isMB != null && !isMB.Attributes.IsStatic() && OpCodes.Newobj != code )
            {
               // Pop 'this'
               --curStacksize;
            }
            // Pop parameters
            curStacksize -= isMB == null ? ( (CILMethodSignature) possibleMethod ).Parameters.Count : isMB.Parameters.Count;
            if ( labelOrManyLabelsOrVarArgs != null )
            {
               curStacksize -= ( (Object[]) labelOrManyLabelsOrVarArgs ).Length;
            }
            if ( OpCodes.Calli == code )
            {
               // Pop function pointer
               --curStacksize;
            }
            // TODO we could check here for stack underflow!
            // We assume that all incoming methods and ctors and types and fields are from same context, so we wouldn't need to make a new void-wrapper every time here.
            var rType = possibleMethod is CILMethod ?
               ( (CILMethod) possibleMethod ).GetReturnType() :
               ( possibleMethod is CILMethodSignature ?
               ( (CILMethodSignature) possibleMethod ).ReturnParameter.ParameterType :
               null );
            if ( OpCodes.Newobj == code || ( rType != null && CILTypeCode.Void != rType.GetTypeCode( CILTypeCode.Empty ) ) )
            {
               // Push return value
               ++curStacksize;
            }
         }
         else
         {
            curStacksize += code.StackChange;
         }

         // Save max stack here
         this._maxStack = Math.Max( curStacksize, this._maxStack );

         // Copy branch stack size
         if ( curStacksize > 0 )
         {
            switch ( code.OperandType )
            {
               case OperandType.InlineBrTarget:
               case OperandType.ShortInlineBrTarget:
                  this.UpdateStackSizeAtBranchTarget( (ILLabel) labelOrManyLabelsOrVarArgs, curStacksize );
                  break;
               case OperandType.InlineSwitch:
                  var labels = (ILLabel[]) labelOrManyLabelsOrVarArgs;
                  for ( var i = 0; i < labels.Length; ++i )
                  {
                     this.UpdateStackSizeAtBranchTarget( labels[i], curStacksize );
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
         this._currentStack = curStacksize;
      }

      private void UpdateStackSizeAtBranchTarget( ILLabel label, Int32 stackSize )
      {
         var idx = this._methodIL._labelOffsets[label.labelIdx];
         this._stackSizes[idx] = Math.Max( this._stackSizes.GetOrDefault( idx, 0 ), stackSize );
      }

   }

   [Flags]
   internal enum MethodHeaderFlags
   {
      TinyFormat = 0x2,
      FatFormat = 0x3,
      MoreSections = 0x8,
      InitLocals = 0x10
   }

   [Flags]
   internal enum MethodDataFlags
   {
      ExceptionHandling = 0x1,
      OptimizeILTable = 0x2,
      FatFormat = 0x40,
      MoreSections = 0x80
   }
}