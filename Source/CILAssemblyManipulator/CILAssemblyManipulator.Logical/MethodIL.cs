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
using System.Threading;
using CILAssemblyManipulator.Logical;
using CommonUtils;
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Logical.Implementation;

namespace CILAssemblyManipulator.Logical
{
   /// <summary>
   /// This interface provides read and write access to method body. The functionality roughly corresponds to <see cref="T:System.Reflection.Emit.ILGenerator"/>.
   /// </summary>
   /// <remarks>
   /// <para>Although <see cref="T:System.Reflection.Emit.ILGenerator"/> class operates on actual OpCodes, this interface adds a layer of abstraction on top of that: instead, the information about actual op codes is stored. Thus it is possible to eg. easily emit branching instructions and let the emitting process decide for itself whether short or long form is needed.</para>
   /// <para>Most methods of this interface provide <c>fluid</c> API, meaning it's easy to chain some method calls.</para>
   /// <para>TODO consider making this interface into a class, and changing Add-method to internal-only.</para>
   /// </remarks>
   public interface MethodIL
   {
      /// <summary>
      /// Adds a new <see cref="LogicalOpCodeInfo"/> at the end of this method body.
      /// </summary>
      /// <param name="opCodeInfo">The <see cref="LogicalOpCodeInfo"/>.</param>
      /// <returns>This <see cref="MethodIL"/>.</returns>
      /// <remarks>This method is rarely intended to be used directly, instead, use extension methods in <see cref="E_MethodIL"/>.</remarks>
      /// <exception cref="ArgumentNullException">If <paramref name="opCodeInfo"/> is <c>null</c>.</exception>
      MethodIL Add( LogicalOpCodeInfo opCodeInfo );

      /// <summary>
      /// Gets the op code info at specified index in the list of <see cref="LogicalOpCodeInfo"/>s of this method body.
      /// </summary>
      /// <param name="index">The index of the op code info.</param>
      /// <returns><see cref="LogicalOpCodeInfo"/> at specified index.</returns>
      /// <exception cref="ArgumentOutOfRangeException">If index is too big.</exception>
      LogicalOpCodeInfo GetOpCodeInfo( Int32 index );

      /// <summary>
      /// Gets the amount of <see cref="LogicalOpCodeInfo"/>s in this method body.
      /// </summary>
      /// <value>The amount of <see cref="LogicalOpCodeInfo"/>s in this method body.</value>
      Int32 OpCodeCount { get; }

      /// <summary>
      /// Gets the amount of <see cref="ILLabel"/>s in this method body.
      /// </summary>
      /// <value>The amount of <see cref="ILLabel"/>s in this method body.</value>
      Int32 LabelCount { get; }

      /// <summary>
      /// Defines a new <see cref="ILLabel"/> to use in branching or other instructions.
      /// </summary>
      /// <returns>A new <see cref="ILLabel"/> to use.</returns>
      ILLabel DefineLabel();

      /// <summary>
      /// Marks label at specified position of op code infos.
      /// </summary>
      /// <param name="label">The label to mark.</param>
      /// <param name="offset">The offset (regarding <see cref="OpCodeCount"/>) of the label.</param>
      /// <returns>This <see cref="MethodIL"/>.</returns>
      /// <exception cref="ArgumentException">If <paramref name="label"/> is invalid for this <see cref="MethodIL"/> or if it is already marked or if <paramref name="offset"/> is invalid.</exception>
      MethodIL MarkLabel( ILLabel label, Int32 offset );

      /// <summary>
      /// Gets the offset for <see cref="ILLabel"/> defined for this method.
      /// </summary>
      /// <param name="label">The label.</param>
      /// <returns>The offset of logical op code for given label, or <c>-1</c> if the lable has not been marked yet, or is invalid for this <see cref="MethodIL"/>.</returns>
      Int32 GetLabelOffset( ILLabel label );

      /// <summary>
      /// Declares a new local with specified type and pinned status to this method body.
      /// </summary>
      /// <param name="type">The type of the local variable.</param>
      /// <param name="pinned"><c>true</c> if the local should be <c>pinned</c>; <c>false</c> otherwise.</param>
      /// <returns>A newly created <see cref="LocalBuilder"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>.</exception>
      LocalBuilder DeclareLocal( CILTypeBase type, Boolean pinned = false );

      /// <summary>
      /// Gets or sets the value indicating whether all local variables should be initialized to their default values at method startup. See ECMA specification related to method header flags for more information.
      /// </summary>
      /// <value>The value indicating whether all local variables should be initialized to their default values at method startup.</value>
      Boolean InitLocals { get; set; }

      /// <summary>
      /// Gets all the <see cref="LogicalOpCodeInfo"/>s currently contained within this <see cref="MethodIL"/>.
      /// </summary>
      /// <value>All the <see cref="LogicalOpCodeInfo"/>s currently contained within this <see cref="MethodIL"/>.</value>
      IEnumerable<LogicalOpCodeInfo> OpCodeInfos { get; }

      /// <summary>
      /// Gets all the <see cref="LocalBuilder"/>s currently defined within this <see cref="MethodIL"/>.
      /// </summary>
      /// <value>All the <see cref="LocalBuilder"/>s currently defined within this <see cref="MethodIL"/>.</value>
      IEnumerable<LocalBuilder> Locals { get; }

      /// <summary>
      /// Gets all the currently defined <see cref="ExceptionBlockInfo" /> within this method body.
      /// </summary>
      /// <value>All the currently defined <see cref="ExceptionBlockInfo" /> within this method body.</value>
      IEnumerable<ExceptionBlockInfo> ExceptionBlocks { get; }

      /// <summary>
      /// Adds explicitly information about the exception block in this method.
      /// </summary>
      /// <param name="block">The <see cref="ExceptionBlockInfo"/>.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="block"/> is <c>null</c>.</exception>
      void AddExceptionBlockInfo( ExceptionBlockInfo block );
   }

   /// <summary>
   /// This struct represents a label to be used when emitting IL code using <see cref="MethodIL"/>.
   /// </summary>
   public struct ILLabel
   {
      private readonly Int32 _labelIdx;

      internal ILLabel( Int32 labelIndex )
      {
         this._labelIdx = labelIndex;
      }

      internal Int32 LabelIndex
      {
         get
         {
            return this._labelIdx;
         }
      }
   }

   /// <summary>
   /// This interface represents a single local variable within a method body.
   /// </summary>
   /// <seealso cref="MethodIL"/>
   public interface LocalBuilder
   {
      /// <summary>
      /// Gets the unique index of this local variable within the method body containing it.
      /// </summary>
      /// <value>The unique index of this local variable within the method body containing it.</value>
      Int32 LocalIndex { get; }

      /// <summary>
      /// Gets the type of this local variable.
      /// </summary>
      /// <value>The type of this local variable.</value>
      CILTypeBase LocalType { get; }

      /// <summary>
      /// Returns <c>true</c> if this local variable is <c>pinned</c>; <c>false</c> otherwise.
      /// </summary>
      /// <value><c>true</c> if this local variable is <c>pinned</c>; <c>false</c> otherwise.</value>
      Boolean IsPinned { get; }
   }

   /// <summary>
   /// This interface represents required information about a single op code for emitting process.
   /// </summary>
   /// <remarks>Usually it is sufficient to use extension methods in <see cref="E_MethodIL"/> instead of actually creating instances of this class.</remarks>
   public abstract class LogicalOpCodeInfo
   {
      internal LogicalOpCodeInfo()
      {

      }

      ///// <summary>
      ///// Emits the actual <see cref="OpCode"/> to <see cref="MethodILWriter"/>.
      ///// </summary>
      ///// <param name="emittingContext">The current <see cref="MethodILWriter"/>.</param>
      //internal abstract void EmitOpCode( MethodILWriter emittingContext );

      ///// <summary>
      ///// Gets the minimum amount of bytes that may be emitted for this <see cref="LogicalOpCodeInfo"/>.
      ///// </summary>
      ///// <value>The minimum amount of bytes that may be emitted for this <see cref="LogicalOpCodeInfo"/>.</value>
      //internal abstract Int32 MinSize { get; }

      ///// <summary>
      ///// Gets the maximum amount of bytes that may be emitted for this <see cref="LogicalOpCodeInfo"/>.
      ///// </summary>
      ///// <value>The maximum amount of bytes that may be emitted for this <see cref="LogicalOpCodeInfo"/>.</value>
      //internal abstract Int32 MaxSize { get; }

      ///// <summary>
      ///// Gets the amount of branch instructions for this <see cref="LogicalOpCodeInfo"/>. Should be <c>0</c> for non-branching instructions, <c>1</c> for normal branches, and <c>X</c> for switch instruction, where <c>X</c> is amount of cases.
      ///// </summary>
      ///// <value>The amount of branch instructions for this <see cref="LogicalOpCodeInfo"/>.</value>
      //internal abstract Int32 BranchTargetCount { get; }

      /// <summary>
      /// Gets the <see cref="OpCodeInfoKind"/> identifying what kind of <see cref="LogicalOpCodeInfo"/> this is.
      /// </summary>
      /// <value>The <see cref="OpCodeInfoKind"/> identifying what kind of <see cref="LogicalOpCodeInfo"/> this is.</value>
      public abstract OpCodeInfoKind InfoKind { get; }
   }

   /// <summary>
   /// This enumeration easily identifies what kind is any <see cref="LogicalOpCodeInfo"/>.
   /// </summary>
   /// <seealso cref="LogicalOpCodeInfo.InfoKind"/>
   public enum OpCodeInfoKind
   {
      /// <summary>
      /// This is <see cref="LogicalOpCodeInfoWithNoOperand"/>.
      /// </summary>
      OperandNone,
      /// <summary>
      /// This is <see cref="LogicalOpCodeInfoWithTypeToken"/>.
      /// </summary>
      OperandTypeToken,
      /// <summary>
      /// This is <see cref="LogicalOpCodeInfoWithFieldToken"/>.
      /// </summary>
      OperandFieldToken,
      /// <summary>
      /// This is <see cref="LogicalOpCodeInfoWithMethodToken"/>.
      /// </summary>
      OperandMethodToken,
      /// <summary>
      /// This is <see cref="LogicalOpCodeInfoWithCtorToken"/>.
      /// </summary>
      OperandCtorToken,
      /// <summary>
      /// This is <see cref="LogicalOpCodeInfoWithMethodSig"/>.
      /// </summary>
      OperandMethodSigToken,
      /// <summary>
      /// This is <see cref="LogicalOpCodeInfoForNormalOrVirtual"/>.
      /// </summary>
      NormalOrVirtual,
      /// <summary>
      /// This is <see cref="LogicalOpCodeInfoWithFixedSizeOperandString"/>.
      /// </summary>
      OperandString,
      /// <summary>
      /// This is <see cref="LogicalOpCodeInfoWithFixedSizeOperandUInt16"/>.
      /// </summary>
      OperandUInt16,
      /// <summary>
      /// This is <see cref="LogicalOpCodeInfoWithFixedSizeOperandInt32"/>.
      /// </summary>
      OperandInt32,
      /// <summary>
      /// This is <see cref="LogicalOpCodeInfoWithFixedSizeOperandInt64"/>.
      /// </summary>
      OperandInt64,
      /// <summary>
      /// This is <see cref="LogicalOpCodeInfoWithFixedSizeOperandSingle"/>.
      /// </summary>
      OperandR4,
      /// <summary>
      /// This is <see cref="LogicalOpCodeInfoWithFixedSizeOperandDouble"/>.
      /// </summary>
      OperandR8,
      /// <summary>
      /// This is <see cref="LogicalOpCodeInfoForBranch"/>.
      /// </summary>
      Branch,
      /// <summary>
      /// This is <see cref="LogicalOpCodeInfoForSwitch"/>.
      /// </summary>
      Switch,
      /// <summary>
      /// This is <see cref="LogicalOpCodeInfoForLeave"/>.
      /// </summary>
      Leave,
      /// <summary>
      /// This is <see cref="LogicalOpCodeInfoForFixedBranchOrLeave"/>.
      /// </summary>
      BranchOrLeaveFixed
   }

   /// <summary>
   /// This class represents information about a single exception block within <see cref="MethodIL"/>.
   /// The unit of all offsets and lengths in this class is <see cref="LogicalOpCodeInfo"/>.
   /// That is, the offsets and lengths are *not* in bytes.
   /// </summary>
   public sealed class ExceptionBlockInfo
   {
      private readonly ILLabel _endLabel;
      private readonly Int32 _tryOffset;
      private Int32 _tryLength;
      private Int32 _handlerOffset;
      private Int32 _handlerLength;
      private CILTypeBase _exceptionType;
      private Int32 _filterOffset;
      private ExceptionBlockType _blockType;

      /// <summary>
      /// Creates a new instance of <see cref="ExceptionBlockInfo"/> with specified values.
      /// </summary>
      /// <param name="endLabel">The label marking the end of this exception block.</param>
      /// <param name="tryOffset">The <see cref="LogicalOpCodeInfo"/>-based offset of the try-block.</param>
      /// <param name="tryLength">The <see cref="LogicalOpCodeInfo"/>-based length of the try-block.</param>
      /// <param name="handlerOffset">The <see cref="LogicalOpCodeInfo"/>-based offset of the handler block.</param>
      /// <param name="handlerLength">The <see cref="LogicalOpCodeInfo"/>-based length of the handler block.</param>
      /// <param name="exceptionType">The exception handling type or <c>null</c>.</param>
      /// <param name="filterOffset">The <see cref="LogicalOpCodeInfo"/>-based offset of the filter block.</param>
      /// <param name="blockType">The <see cref="ExceptionBlockType"/>.</param>
      public ExceptionBlockInfo( ILLabel endLabel, Int32 tryOffset, Int32 tryLength, Int32 handlerOffset, Int32 handlerLength, CILTypeBase exceptionType, Int32 filterOffset, ExceptionBlockType blockType )
         : this( tryOffset, endLabel )
      {
         this._tryLength = tryLength;
         this._handlerOffset = handlerOffset;
         this._handlerLength = handlerLength;
         this._exceptionType = exceptionType;
         this._filterOffset = filterOffset;
         this._blockType = blockType;
      }

      internal ExceptionBlockInfo( Int32 aTryOffset, ILLabel label )
      {
         this._endLabel = label;
         this._tryOffset = aTryOffset;
         this._tryLength = -1;
         this._handlerOffset = -1;
         this._handlerLength = -1;
         this._filterOffset = -1;
      }

      internal void HandlerBegun( ExceptionBlockType aBlockType, Int32 offset )
      {
         this._blockType = aBlockType;
         this._tryLength = offset - this._tryOffset;
         this._handlerOffset = offset;
      }

      internal void HandlerEnded( Int32 offset )
      {
         this._handlerLength = offset - this._handlerOffset;
      }

      /// <summary>
      /// Gets or sets the <see cref="CILTypeBase"/> for the type of exception for this <see cref="ExceptionBlockInfo"/>.
      /// </summary>
      /// <value>The <see cref="CILTypeBase"/> for the type of exception for this <see cref="ExceptionBlockInfo"/>.</value>
      public CILTypeBase ExceptionType
      {
         get
         {
            return this._exceptionType;
         }
         set
         {
            this._exceptionType = value;
         }
      }

      /// <summary>
      /// Gets or sets the <see cref="ExceptionBlockType"/> for this <see cref="ExceptionBlockInfo"/>.
      /// </summary>
      /// <value>The <see cref="ExceptionBlockType"/> for this <see cref="ExceptionBlockInfo"/>.</value>
      public ExceptionBlockType BlockType
      {
         get
         {
            return this._blockType;
         }
         set
         {
            this._blockType = value;
         }
      }

      /// <summary>
      /// Gets the label marking the end of this exception block.
      /// </summary>
      /// <value>The label marking the end of this exception block.</value>
      public ILLabel EndLabel
      {
         get
         {
            return this._endLabel;
         }
      }

      /// <summary>
      /// Gets the <see cref="LogicalOpCodeInfo"/>-based offset of the try-block.
      /// </summary>
      /// <value>The <see cref="LogicalOpCodeInfo"/>-based offset of the try-block.</value>
      public Int32 TryOffset
      {
         get
         {
            return this._tryOffset;
         }
      }

      /// <summary>
      /// Gets the <see cref="LogicalOpCodeInfo"/>-based length of the try-block.
      /// </summary>
      /// <value>The <see cref="LogicalOpCodeInfo"/>-based length of the try-block.</value>
      public Int32 TryLength
      {
         get
         {
            return this._tryLength;
         }
         internal set
         {
            this._tryLength = value;
         }
      }

      /// <summary>
      /// Gets the <see cref="LogicalOpCodeInfo"/>-based offset of the handler block.
      /// </summary>
      /// <value>The <see cref="LogicalOpCodeInfo"/>-based offset of the handler block.</value>
      public Int32 HandlerOffset
      {
         get
         {
            return this._handlerOffset;
         }
         internal set
         {
            this._handlerOffset = value;
         }
      }

      /// <summary>
      /// Gets the <see cref="LogicalOpCodeInfo"/>-based length of the handler block.
      /// </summary>
      /// <value>The <see cref="LogicalOpCodeInfo"/>-based length of the handler block.</value>
      public Int32 HandlerLength
      {
         get
         {
            return this._handlerLength;
         }
         internal set
         {
            this._handlerLength = value;
         }
      }

      /// <summary>
      /// Gets the <see cref="LogicalOpCodeInfo"/>-based offset of the filter block.
      /// </summary>
      /// <value>The <see cref="LogicalOpCodeInfo"/>-based offset of the filter block.</value>
      public Int32 FilterOffset
      {
         get
         {
            return this._filterOffset;
         }
         internal set
         {
            this._filterOffset = value;
         }
      }
   }

   /// <summary>
   /// This enumeration contains various branching conditions to be used in extension methods of <see cref="MethodIL"/> in <see cref="E_MethodIL"/>.
   /// </summary>
   public enum BranchType
   {
      /// <summary>
      /// Branches to target instruction if two top-most values on stack are equal.
      /// </summary>
      /// <seealso cref="OpCodes.Beq"/>
      /// <seealso cref="OpCodes.Beq_S"/>
      IF_BOTH_EQUAL,

      /// <summary>
      /// Branches to target instruction if the first value on stack is greater than the second value.
      /// </summary>
      /// <seealso cref="OpCodes.Bgt"/>
      /// <seealso cref="OpCodes.Bgt_S"/>
      IF_FIRST_GREATER_THAN_SECOND,

      /// <summary>
      /// Branches to target instruction if the first value on stack is greater or equal to the second value.
      /// </summary>
      /// <seealso cref="OpCodes.Bge"/>
      /// <seealso cref="OpCodes.Bge_S"/>
      IF_FIRST_GREATER_THAN_OR_EQUAL_TO_SECOND,

      /// <summary>
      /// Branches to target instruction if the first value on stack is greater than the second value, using unordered comparison.
      /// </summary>
      /// <seealso cref="OpCodes.Bgt_Un"/>
      /// <seealso cref="OpCodes.Bgt_Un_S"/>
      IF_FIRST_GREATER_THAN_SECOND_UNORDERED,

      /// <summary>
      /// Branches to target instruction if the first value on stack is greater or equal to the second value, using unordered comparison.
      /// </summary>
      /// <seealso cref="OpCodes.Bge_Un"/>
      /// <seealso cref="OpCodes.Bge_Un_S"/>
      IF_FIRST_GREATER_THAN_OR_EQUAL_TO_SECOND_UNORDERED,

      /// <summary>
      /// Branches to target instruction if the first value on stack is lesser than the second value.
      /// </summary>
      /// <seealso cref="OpCodes.Blt"/>
      /// <seealso cref="OpCodes.Blt_S"/>
      IF_FIRST_LESSER_THAN_SECOND,

      /// <summary>
      /// Branches to target instruction if the first value on stack is lesser than or equal to the second value.
      /// </summary>
      /// <seealso cref="OpCodes.Ble"/>
      /// <seealso cref="OpCodes.Ble_S"/>
      IF_FIRST_LESSER_THAN_OR_EQUAL_TO_SECOND,

      /// <summary>
      /// Branches to target instruction if the first value on stack is lesser than the second value, using unordered comparison.
      /// </summary>
      /// <seealso cref="OpCodes.Blt_Un"/>
      /// <seealso cref="OpCodes.Blt_Un_S"/>
      IF_FIRST_LESSER_THAN_SECOND_UNORDERED,

      /// <summary>
      /// Branches to target instruction if the first value on stack is lesser than or equal to the second value, using unordered comparison.
      /// </summary>
      /// <seealso cref="OpCodes.Ble_Un"/>
      /// <seealso cref="OpCodes.Ble_Un_S"/>
      IF_FIRST_LESSER_THAN_OR_EQUAL_TO_SECOND_UNORDERED,

      /// <summary>
      /// Branches to target instruction if the two values on stack are not equal, using unordered comparison.
      /// </summary>
      /// <seealso cref="OpCodes.Bne_Un"/>
      /// <seealso cref="OpCodes.Bne_Un_S"/>
      IF_NOT_EQUAL_UNORDERED,

      /// <summary>
      /// Always branches to target instruction.
      /// </summary>
      /// <seealso cref="OpCodes.Br"/>
      /// <seealso cref="OpCodes.Br_S"/>
      ALWAYS,

      /// <summary>
      /// Branches to target instruction if the first value on stack is <c>false</c>, <c>0</c> or <c>null</c>.
      /// </summary>
      /// <seealso cref="OpCodes.Brfalse"/>
      /// <seealso cref="OpCodes.Brfalse_S"/>
      IF_FALSE,

      /// <summary>
      /// Branches to target instruction if the first value on stack is <c>true</c>, non-<c>0</c> or non-<c>null</c>.
      /// </summary>
      /// <seealso cref="OpCodes.Brtrue"/>
      /// <seealso cref="OpCodes.Brtrue_S"/>
      IF_TRUE,
   }

   public struct VarArgInstance
   {
      private readonly CILCustomModifier[] _mods;
      private readonly CILTypeBase _type;

      public VarArgInstance( CILTypeBase type )
         : this( type, Empty<CILCustomModifier>.Array )
      {

      }

      public VarArgInstance( CILTypeBase type, params CILCustomModifier[] mods )
      {
         this._type = type;
         this._mods = mods;
      }

      public CILTypeBase Type
      {
         get
         {
            return this._type;
         }
      }

      public CILCustomModifier[] CustomModifiers
      {
         get
         {
            return this._mods;
         }
      }
   }
}

public static partial class E_CILLogical
{
   private static readonly System.Reflection.MethodInfo TYPE_OF_METHOD;
   private static readonly System.Reflection.MethodInfo METHOD_OF_METHOD;
   private static readonly System.Reflection.MethodInfo FIELD_OF_METHOD;
   private static readonly System.Reflection.MethodInfo INTERLOCKED_COMPARE_EXCHANGE_METHOD_GDEF;
   private static readonly System.Reflection.MethodInfo INTERLOCKED_COMPARE_EXCHANGE_METHOD_DOUBLE;
   private static readonly System.Reflection.MethodInfo INTERLOCKED_COMPARE_EXCHANGE_METHOD_SINGLE;
   private static readonly System.Reflection.MethodInfo INTERLOCKED_COMPARE_EXCHANGE_METHOD_INT32;
   private static readonly System.Reflection.MethodInfo INTERLOCKED_COMPARE_EXCHANGE_METHOD_INT64;
   private static readonly System.Reflection.ConstructorInfo DECIMAL_CTOR_INT32;
   private static readonly System.Reflection.ConstructorInfo DECIMAL_CTOR_INT64;
   private static readonly System.Reflection.ConstructorInfo DECIMAL_CTOR_MULTIPLE;


   private static readonly IDictionary<CILTypeCode, LogicalOpCodeInfo> CHECKED_UNSIGNED_CONV_OPCODES;
   private static readonly IDictionary<CILTypeCode, LogicalOpCodeInfo> CHECKED_SIGNED_CONV_OPCODES;
   private static readonly IDictionary<CILTypeCode, LogicalOpCodeInfo> UNCHECKED_UNSIGNED_CONV_OPCODES;
   private static readonly IDictionary<CILTypeCode, LogicalOpCodeInfo> UNCHECKED_SIGNED_CONV_OPCODES;

   static E_CILLogical()
   {
      TYPE_OF_METHOD = typeof( Type ).LoadMethodOrThrow( "GetTypeFromHandle", null );
      METHOD_OF_METHOD = typeof( System.Reflection.MethodBase ).LoadMethodOrThrow( "GetMethodFromHandle", 2 );
      FIELD_OF_METHOD = typeof( System.Reflection.FieldInfo ).LoadMethodOrThrow( "GetFieldFromHandle", 2 );
      INTERLOCKED_COMPARE_EXCHANGE_METHOD_GDEF = typeof( Interlocked ).LoadMethodGDefinitionOrThrow( "CompareExchange" );
      INTERLOCKED_COMPARE_EXCHANGE_METHOD_DOUBLE = typeof( Interlocked ).LoadMethodWithParamTypesOrThrow( "CompareExchange", new Type[] { typeof( Double ).MakeByRefType(), typeof( Double ), typeof( Double ) } );
      INTERLOCKED_COMPARE_EXCHANGE_METHOD_SINGLE = typeof( Interlocked ).LoadMethodWithParamTypesOrThrow( "CompareExchange", new Type[] { typeof( Single ).MakeByRefType(), typeof( Single ), typeof( Single ) } );
      INTERLOCKED_COMPARE_EXCHANGE_METHOD_INT32 = typeof( Interlocked ).LoadMethodWithParamTypesOrThrow( "CompareExchange", new Type[] { typeof( Int32 ).MakeByRefType(), typeof( Int32 ), typeof( Int32 ) } );
      INTERLOCKED_COMPARE_EXCHANGE_METHOD_INT64 = typeof( Interlocked ).LoadMethodWithParamTypesOrThrow( "CompareExchange", new Type[] { typeof( Int64 ).MakeByRefType(), typeof( Int64 ), typeof( Int64 ) } );
      DECIMAL_CTOR_INT32 = typeof( Decimal ).LoadConstructorOrThrow( new Type[] { typeof( Int32 ) } );
      DECIMAL_CTOR_INT64 = typeof( Decimal ).LoadConstructorOrThrow( new Type[] { typeof( Int64 ) } );
      DECIMAL_CTOR_MULTIPLE = typeof( Decimal ).LoadConstructorOrThrow( 5 );

      CHECKED_UNSIGNED_CONV_OPCODES = new Dictionary<CILTypeCode, LogicalOpCodeInfo>();
      CHECKED_UNSIGNED_CONV_OPCODES.Add( CILTypeCode.SByte, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_Ovf_I1_Un ) );
      CHECKED_UNSIGNED_CONV_OPCODES.Add( CILTypeCode.Int16, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_Ovf_I2_Un ) );
      CHECKED_UNSIGNED_CONV_OPCODES.Add( CILTypeCode.Int32, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_Ovf_I4_Un ) );
      CHECKED_UNSIGNED_CONV_OPCODES.Add( CILTypeCode.Int64, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_Ovf_I8_Un ) );
      CHECKED_UNSIGNED_CONV_OPCODES.Add( CILTypeCode.Byte, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_Ovf_U1_Un ) );
      CHECKED_UNSIGNED_CONV_OPCODES.Add( CILTypeCode.UInt16, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_Ovf_U2_Un ) );
      CHECKED_UNSIGNED_CONV_OPCODES.Add( CILTypeCode.Char, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_Ovf_U2_Un ) );
      CHECKED_UNSIGNED_CONV_OPCODES.Add( CILTypeCode.UInt32, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_Ovf_U4_Un ) );
      CHECKED_UNSIGNED_CONV_OPCODES.Add( CILTypeCode.UInt64, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_Ovf_U8_Un ) );

      CHECKED_SIGNED_CONV_OPCODES = new Dictionary<CILTypeCode, LogicalOpCodeInfo>();
      CHECKED_SIGNED_CONV_OPCODES.Add( CILTypeCode.SByte, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_Ovf_I1 ) );
      CHECKED_SIGNED_CONV_OPCODES.Add( CILTypeCode.Int16, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_Ovf_I2 ) );
      CHECKED_SIGNED_CONV_OPCODES.Add( CILTypeCode.Int32, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_Ovf_I4 ) );
      CHECKED_SIGNED_CONV_OPCODES.Add( CILTypeCode.Int64, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_Ovf_I8 ) );
      CHECKED_SIGNED_CONV_OPCODES.Add( CILTypeCode.Byte, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_Ovf_U1 ) );
      CHECKED_SIGNED_CONV_OPCODES.Add( CILTypeCode.UInt16, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_Ovf_U2 ) );
      CHECKED_SIGNED_CONV_OPCODES.Add( CILTypeCode.Char, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_Ovf_U2 ) );
      CHECKED_SIGNED_CONV_OPCODES.Add( CILTypeCode.UInt32, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_Ovf_U4 ) );
      CHECKED_SIGNED_CONV_OPCODES.Add( CILTypeCode.UInt64, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_Ovf_U8 ) );

      UNCHECKED_UNSIGNED_CONV_OPCODES = new Dictionary<CILTypeCode, LogicalOpCodeInfo>();
      UNCHECKED_UNSIGNED_CONV_OPCODES.Add( CILTypeCode.SByte, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_I1 ) );
      UNCHECKED_UNSIGNED_CONV_OPCODES.Add( CILTypeCode.Int16, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_I2 ) );
      UNCHECKED_UNSIGNED_CONV_OPCODES.Add( CILTypeCode.Int32, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_I4 ) );
      UNCHECKED_UNSIGNED_CONV_OPCODES.Add( CILTypeCode.Int64, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_U8 ) );
      UNCHECKED_UNSIGNED_CONV_OPCODES.Add( CILTypeCode.Byte, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_U1 ) );
      UNCHECKED_UNSIGNED_CONV_OPCODES.Add( CILTypeCode.UInt16, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_U2 ) );
      UNCHECKED_UNSIGNED_CONV_OPCODES.Add( CILTypeCode.Char, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_U2 ) );
      UNCHECKED_UNSIGNED_CONV_OPCODES.Add( CILTypeCode.UInt32, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_U4 ) );
      UNCHECKED_UNSIGNED_CONV_OPCODES.Add( CILTypeCode.UInt64, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_U8 ) );

      UNCHECKED_SIGNED_CONV_OPCODES = new Dictionary<CILTypeCode, LogicalOpCodeInfo>();
      UNCHECKED_SIGNED_CONV_OPCODES.Add( CILTypeCode.SByte, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_I1 ) );
      UNCHECKED_SIGNED_CONV_OPCODES.Add( CILTypeCode.Int16, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_I2 ) );
      UNCHECKED_SIGNED_CONV_OPCODES.Add( CILTypeCode.Int32, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_I4 ) );
      UNCHECKED_SIGNED_CONV_OPCODES.Add( CILTypeCode.Int64, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_I8 ) );
      UNCHECKED_SIGNED_CONV_OPCODES.Add( CILTypeCode.Byte, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_U1 ) );
      UNCHECKED_SIGNED_CONV_OPCODES.Add( CILTypeCode.UInt16, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_U2 ) );
      UNCHECKED_SIGNED_CONV_OPCODES.Add( CILTypeCode.Char, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_U2 ) );
      UNCHECKED_SIGNED_CONV_OPCODES.Add( CILTypeCode.UInt32, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_U4 ) );
      UNCHECKED_SIGNED_CONV_OPCODES.Add( CILTypeCode.UInt64, LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Conv_I8 ) );
   }

   #region Extension Methods

   /// <summary>
   /// Convenience method to mark several <see cref="ILLabel"/>s at once.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/></param>
   /// <param name="labels">The labels to mark.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="labels"/> is <c>null</c>.</exception>
   public static MethodIL MarkLabels( this MethodIL il, IEnumerable<ILLabel> labels )
   {
      ArgumentValidator.ValidateNotNull( "Labels", labels );
      foreach ( var label in labels )
      {
         il.MarkLabel( label );
      }
      return il;
   }

   /// <summary>
   /// Adds info which will emit <see cref="OpCodes.Add"/> to <paramref name="il"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/></param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitAdd( this MethodIL il )
   {
      return il.Add( OpCodeEncoding.Add );
   }

   /// <summary>
   /// Adds info which will emit <see cref="OpCodes.And"/> to <paramref name="il"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/></param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitBitwiseAND( this MethodIL il )
   {
      return il.Add( OpCodeEncoding.And );
   }

   /// <summary>
   /// Adds info which will emit <see cref="OpCodes.Or"/> to <paramref name="il"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/></param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitBitwiseOR( this MethodIL il )
   {
      return il.Add( OpCodeEncoding.Or );
   }

   /// <summary>
   /// Adds info which will emit <see cref="OpCodes.Xor"/> to <paramref name="il"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/></param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitBitwiseXOR( this MethodIL il )
   {
      return il.Add( OpCodeEncoding.Xor );
   }

   /// <summary>
   /// Adds info which will emit <see cref="OpCodes.Not"/> to <paramref name="il"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/></param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitBitwiseNOT( this MethodIL il )
   {
      return il.Add( OpCodeEncoding.Not );
   }

   /// <summary>
   /// Adds info which will emit a branch instruction corresponding to <paramref name="branchType"/> to <paramref name="il"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/></param>
   /// <param name="branchType">The type of branch condition to use.</param>
   /// <param name="targetLabel">The branch target.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <remarks>The branch instruction will be in short form, if possible.</remarks>
   public static MethodIL EmitBranch( this MethodIL il, BranchType branchType, ILLabel targetLabel )
   {
      return il.Add( new LogicalOpCodeInfoForBranch( branchType, targetLabel ) );
   }

   /// <summary>
   /// Adds info which will emit a possibly virtual method call to <paramref name="method"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/></param>
   /// <param name="method">The method to emit call to.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="method"/> is <c>null</c>.</exception>
   public static MethodIL EmitCall( this MethodIL il, CILMethod method )
   {
      return il.Add( LogicalOpCodeInfoForNormalOrVirtual.OpCodeInfoForCall( method ) );
   }

   /// <summary>
   /// Adds info which will emit a <see cref="OpCodes.Call"/> to <paramref name="ctor"/>. Typically used when calling for base type constructor.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/></param>
   /// <param name="ctor">The <see cref="CILConstructor"/> to call.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="ctor"/> is <c>null</c>.</exception>
   public static MethodIL EmitCall( this MethodIL il, CILConstructor ctor )
   {
      return il.Add( new LogicalOpCodeInfoWithCtorToken(
         OpCodes.Call,
         ctor
         ) );
   }

   /// <summary>
   /// Emits a <see cref="OpCodes.Calli"/> instruction calling specified method with specified variable arguments.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="methodSig">The <see cref="CILMethodSignature"/> to call.</param>
   /// <param name="varArgs">Optional variable arguments to the call.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="methodSig"/> is <c>null</c>.</exception>
   public static MethodIL EmitCall( this MethodIL il, CILMethodSignature methodSig, params CILTypeBase[] varArgs )
   {
      return EmitCall( il, methodSig, varArgs.Select( v => new VarArgInstance( v ) ).ToArray() );
   }

   /// <summary>
   /// Emits a <see cref="OpCodes.Calli"/> instruction calling specified method with specified variable arguments.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="methodSig">The <see cref="CILMethodSignature"/> to call.</param>
   /// <param name="varArgs">Optional variable arguments to the call.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="methodSig"/> is <c>null</c>.</exception>
   public static MethodIL EmitCall( this MethodIL il, CILMethodSignature methodSig, params VarArgInstance[] varArgs )
   {
      ArgumentValidator.ValidateNotNull( "Method signature", methodSig );
      return il.Add( new LogicalOpCodeInfoWithMethodSig( methodSig, varArgs ) );
   }

   /// <summary>
   /// Adds info which will emit a <see cref="OpCodes.Call"/> to <paramref name="method"/>. Typically used when calling a base method within overriding method.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/></param>
   /// <param name="method">The <see cref="CILMethod"/> to call.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="method"/> is <c>null</c>.</exception>
   public static MethodIL EmitCallBase( this MethodIL il, CILMethod method )
   {
      return il.Add( new LogicalOpCodeInfoWithMethodToken(
         OpCodes.Call,
         method
         ) );
   }

   /// <summary>
   /// Adds required infos to <see cref="MethodIL"/> which will perform a C#-style casting from type <paramref name="typeFrom"/> to type <paramref name="typeTo"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/></param>
   /// <param name="typeFrom">The type to cast from.</param>
   /// <param name="typeTo">The type to cast to.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="typeFrom"/> or <paramref name="typeTo"/> is <c>null</c>.</exception>
   /// <remarks>
   /// TODO: this does not handle numeric conversions automatically.
   /// Implement automatic numeric conversion possibility.
   /// </remarks>
   public static MethodIL EmitCastToType( this MethodIL il, CILTypeBase typeFrom, CILTypeBase typeTo )
   {
      ArgumentValidator.ValidateNotNull( "Type to perform cast from", typeFrom );
      ArgumentValidator.ValidateNotNull( "Type to perform cast to", typeTo );

      if ( !typeFrom.Equals( typeTo ) )
      {
         if ( typeTo.IsByRef() )
         {
            typeTo = ( (CILType) typeTo ).ElementType;
         }
         if ( typeFrom.IsByRef() )
         {
            typeFrom = ( (CILType) typeFrom ).ElementType;
         }

         if ( !typeFrom.IsValueType() && !typeFrom.IsGenericParameter() && ( typeTo.IsValueType() || typeTo.IsGenericParameter() ) )
         {
            il.Add( new LogicalOpCodeInfoWithTypeToken(
               OpCodes.Unbox_Any,
               typeTo
               ) );
         }
         else if ( !typeTo.IsValueType() && !typeTo.IsGenericParameter() && ( typeFrom.IsValueType() || typeFrom.IsGenericParameter() ) )
         {
            il.Add( new LogicalOpCodeInfoWithTypeToken(
               OpCodes.Box,
               typeFrom
               ) );
            if ( typeTo.GetTypeCode() != CILTypeCode.SystemObject )
            {
               il.Add( new LogicalOpCodeInfoWithTypeToken(
                  OpCodes.Castclass,
                  typeTo
                  ) );
            }
         }
         else if ( !typeFrom.IsValueType() && !typeTo.IsValueType() && !typeTo.IsGenericParameter() && !typeFrom.IsGenericParameter() )
         {
            if ( TypeKind.MethodSignature == typeFrom.TypeKind && TypeKind.MethodSignature != typeTo.TypeKind )
            {
               // Do nothing (seems this is what compiler does...)
            }
            else if ( TypeKind.MethodSignature == typeTo.TypeKind )
            {
               il.Add( OpCodeEncoding.Conv_I );
            }
            else if ( typeTo.ContainsGenericParameters() || ( !typeTo.IsAssignableFrom( typeFrom ) && !( typeFrom.IsInterface() && typeTo.GetTypeCode() == CILTypeCode.SystemObject ) ) )
            {
               il.Add( new LogicalOpCodeInfoWithTypeToken(
                  OpCodes.Castclass,
                  typeTo
                  ) );
            }
         }
      }
      return il;
   }

   /// <summary>
   /// Adds info which will emit <see cref="OpCodes.Ceq"/> to <paramref name="il"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c></exception>
   public static MethodIL EmitCeq( this MethodIL il )
   {
      return il.Add( OpCodeEncoding.Ceq );
   }

   /// <summary>
   /// Adds info which will emit <see cref="OpCodes.Cgt"/> or <see cref="OpCodes.Cgt_Un"/> to <paramref name="il"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="unsigned">If <c>true</c>, <see cref="OpCodes.Cgt_Un"/> will be emitted, otherwise, <see cref="OpCodes.Cgt"/> will be emitted.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c></exception>
   public static MethodIL EmitCgt( this MethodIL il, Boolean unsigned = false )
   {
      return il.Add( unsigned ? OpCodeEncoding.Cgt_Un : OpCodeEncoding.Cgt );
   }

   /// <summary>
   /// Adds info which will emit <see cref="OpCodes.Clt"/> or <see cref="OpCodes.Clt_Un"/> to <paramref name="il"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="unsigned">If <c>true</c>, <see cref="OpCodes.Clt_Un"/> will be emitted, otherwise, <see cref="OpCodes.Clt"/> will be emitted.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c></exception>
   public static MethodIL EmitClt( this MethodIL il, Boolean unsigned = false )
   {
      return il.Add( unsigned ? OpCodeEncoding.Clt_Un : OpCodeEncoding.Clt );
   }

   /// <summary>
   /// Adds info which will emit a <see cref="OpCodes.Constrained_"/> prefix the given type as operand to <paramref name="il"/>.
   /// After that, the info to force <see cref="OpCodes.Callvirt"/> call for that method is added.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="type">The type serving as operand for <see cref="OpCodes.Constrained_"/>.</param>
   /// <param name="method">The method to call.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="type"/> or <paramref name="method"/> is <c>null</c>.</exception>
   public static MethodIL EmitConstraintedCall( this MethodIL il, CILTypeOrTypeParameter type, CILMethod method )
   {
      return il.Add( new LogicalOpCodeInfoWithTypeToken( OpCodes.Constrained_, type ) )
         .Add( new LogicalOpCodeInfoWithMethodToken( OpCodes.Callvirt, method ) );
   }

   /// <summary>
   /// Adds info which will emit <see cref="OpCodes.Dup"/> to <paramref name="il"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <returns><paramref name="il"/>.</returns>
   public static MethodIL EmitDup( this MethodIL il )
   {
      return il.Add( OpCodeEncoding.Dup );
   }

   /// <summary>
   /// Adds info which will emit <see cref="OpCodes.Initobj"/> with specified type to <paramref name="il"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="valueType">The type to serve as operand to <see cref="OpCodes.Initobj"/></param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c></exception>
   /// <exception cref="ArgumentNullException">If <paramref name="valueType"/> is <c>null</c>.</exception>
   public static MethodIL EmitInitObj( this MethodIL il, CILTypeBase valueType )
   {
      return il.Add( new LogicalOpCodeInfoWithTypeToken(
         OpCodes.Initobj,
         valueType
         ) );
   }

   /// <summary>
   /// Adds info which will emit <see cref="OpCodes.Isinst"/> with specified type to <paramref name="il"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="targetType">The type to serve as operand to <see cref="OpCodes.Isinst"/>.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c></exception>
   /// <exception cref="ArgumentNullException">If <paramref name="targetType"/> is <c>null</c>.</exception>
   public static MethodIL EmitIsInst( this MethodIL il, CILTypeBase targetType )
   {
      return il.Add( new LogicalOpCodeInfoWithTypeToken(
         OpCodes.Isinst,
         targetType
         ) );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit <see cref="OpCodes.Leave"/> instruction with specified <paramref name="label"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="label">The target of <see cref="OpCodes.Leave"/> instruction.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c></exception>
   public static MethodIL EmitLeave( this MethodIL il, ILLabel label )
   {
      return il.Add( new LogicalOpCodeInfoForLeave( label ) );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit most optimal instruction for loading a method argument at <paramref name="index"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="index">The index of the method argument.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c></exception>
   public static MethodIL EmitLoadArg( this MethodIL il, Int32 index )
   {
      EmitOptimalInstructionForShort(
         il,
         OpCodes.Ldarg,
         OpCodes.Ldarg_S,
         OpCodeEncoding.Ldarg_0,
         OpCodeEncoding.Ldarg_1,
         OpCodeEncoding.Ldarg_2,
         OpCodeEncoding.Ldarg_3,
         index
         );
      return il;
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit most optimal instruction for loading a method parameter.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="param">The method parameter.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <remarks>The index of the argument will be index of <paramref name="param"/> if the method of <paramref name="param"/> is static. Otherwise the index will be index of <paramref name="param"/> plus one.</remarks>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="param"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadArg( this MethodIL il, CILParameter param )
   {
      EmitOptimalInstructionForShort(
         il,
         OpCodes.Ldarg,
         OpCodes.Ldarg_S,
         OpCodeEncoding.Ldarg_0,
         OpCodeEncoding.Ldarg_1,
         OpCodeEncoding.Ldarg_2,
         OpCodeEncoding.Ldarg_3,
         GetParameterIndexForEmitting( param )
         );
      return il;
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit most optimal instruction for loading address of argument at <paramref name="index"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="index">The index of the method argument.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadArgAddress( this MethodIL il, Int32 index )
   {
      return il.Add( new LogicalOpCodeInfoWithFixedSizeOperandUInt16(
            index <= Byte.MaxValue ? OpCodes.Ldarga_S : OpCodes.Ldarga,
            (Int16) index
            )
         );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit most optimal instruction for loading parameter and then will also emit suitable variant of <see cref="OpCodes.Ldobj"/>  if type of <paramref name="paramInfo"/> is <c>by-ref</c>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="paramInfo">The method parameter to load.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="paramInfo"/> is <c>null</c>.</exception>
   /// <seealso cref="EmitLoadArg(MethodIL, CILParameter)"/>
   /// <seealso cref="EmitLoadIndirect(MethodIL, CILTypeBase)"/>
   public static MethodIL EmitLoadArgumentForMethodCall( this MethodIL il, CILParameter paramInfo )
   {
      il.EmitLoadArg( GetParameterIndexForEmitting( paramInfo ) );
      var paramType = paramInfo.ParameterType;
      if ( paramType.IsByRef() )
      {
         paramType = ( (CILType) paramType ).ElementType;
         il.EmitLoadIndirect( paramType );
      }
      return il;
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit instruction to load argument if type of <paramref name="paramInfo"/> is not <c>by-ref</c>. Otherwise, the info will emit instruction to load argument address of <paramref name="paramInfo"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="paramInfo">The method parameter to load.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="paramInfo"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadArgumentToPassAsParameter( this MethodIL il, CILParameter paramInfo )
   {
      ArgumentValidator.ValidateNotNull( "Parameter", paramInfo );
      return EmitLoadArgumentToPassAsParameter( il, paramInfo.ParameterType, GetParameterIndexForEmitting( paramInfo ) );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit instruction to load argument at <paramref name="argIndex"/> if <paramref name="parameterType"/> is not <c>by-ref</c>. Otherwise, the info will emit instruction to load argument address at <paramref name="argIndex"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <param name="parameterType">The type of the parameter.</param>
   /// <param name="argIndex">The index to use for the argument loading instruction.</param>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="parameterType"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadArgumentToPassAsParameter( this MethodIL il, CILTypeBase parameterType, Int32 argIndex )
   {
      ArgumentValidator.ValidateNotNull( "Parameter", parameterType );
      return parameterType.IsByRef() ? il.EmitLoadArgAddress( argIndex ) : il.EmitLoadArg( argIndex );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit <see cref="OpCodes.Ldlen"/> instruction.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadArrayLength( this MethodIL il )
   {
      return il.Add( OpCodeEncoding.Ldlen );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit boolean <paramref name="value"/> onto the stack.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="value">The <see cref="Boolean"/> value to load.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadBoolean( this MethodIL il, Boolean value )
   {
      return il.EmitLoadInt32( Convert.ToInt32( value ) );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit byte <paramref name="value"/> onto the stack.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="value">The <see cref="Byte"/> value to load.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadByte( this MethodIL il, Byte value )
   {
      return il
         .EmitLoadInt32( value )
         .EmitNumericConversion( CILTypeCode.Int32, CILTypeCode.Byte, false );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit signed byte <paramref name="value"/> onto the stack.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="value">The <see cref="SByte"/> value to load.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   [CLSCompliant( false )]
   public static MethodIL EmitLoadSByte( this MethodIL il, SByte value )
   {
      return il
         .EmitLoadInt32( value )
         .EmitNumericConversion( CILTypeCode.Int32, CILTypeCode.SByte, false );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit decimal <paramref name="value"/> onto the stack.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="value">The <see cref="Decimal"/> value to load.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadDecimal( this MethodIL il, Decimal value )
   {
      if ( Decimal.Truncate( value ) == value )
      {
         if ( Int32.MinValue <= value && Int32.MaxValue >= value )
         {
            il.EmitLoadInt32( Decimal.ToInt32( value ) )
               .EmitNewObject( ResolveMSCorLibCtor( il, DECIMAL_CTOR_INT32 ) );
         }
         else if ( Int64.MinValue <= value && Int64.MaxValue >= value )
         {
            il.EmitLoadInt64( Decimal.ToInt64( value ) )
               .EmitNewObject( ResolveMSCorLibCtor( il, DECIMAL_CTOR_INT64 ) );
         }
         else
         {
            EmitLoadDecimalWithMultipleArgsCtor( il, value );
         }
      }
      else
      {
         EmitLoadDecimalWithMultipleArgsCtor( il, value );
      }
      return il;
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit the default value for <paramref name="type"/> onto the stack.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="type">The type of the value to load.</param>
   /// <param name="localGetter">In case type is non-primitive value type or generic type parameter, the function to get <see cref="LocalBuilder"/> to temporary store it.</param>
   /// <param name="emitLoadLocal">If <paramref name="localGetter"/> is used, this parameter tells whether to emit <see cref="OpCodes.Ldloc"/> or its optimized form after <see cref="OpCodes.Initobj"/>.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>, or if <paramref name="localGetter"/> is <c>null</c> when it is needed.</exception>
   public static MethodIL EmitLoadDefault( this MethodIL il, CILTypeBase type, Func<CILTypeBase, LocalBuilder> localGetter, Boolean emitLoadLocal = true )
   {
      ArgumentValidator.ValidateNotNull( "Type", type );
      if ( type.IsEnum() )
      {
         type = ( (CILType) type ).GetEnumValueField().FieldType;
      }
      var tc = type.GetTypeCode( CILTypeCode.Int32 );
      switch ( tc )
      {
         case CILTypeCode.Byte:
            il.EmitLoadByte( default( Byte ) );
            break;
         case CILTypeCode.SByte:
            il.EmitLoadSByte( default( SByte ) );
            break;
         case CILTypeCode.Int16:
            il.EmitLoadInt16( default( Int16 ) );
            break;
         case CILTypeCode.UInt16:
            il.EmitLoadUInt16( default( UInt16 ) );
            break;
         case CILTypeCode.Char:
            il.EmitLoadChar( default( Char ) );
            break;
         case CILTypeCode.Int32:
            il.EmitLoadInt32( default( Int32 ) );
            break;
         case CILTypeCode.UInt32:
            il.EmitLoadUInt32( default( UInt32 ) );
            break;
         case CILTypeCode.Int64:
            il.EmitLoadInt64( default( Int64 ) );
            break;
         case CILTypeCode.UInt64:
            il.EmitLoadUInt64( default( UInt64 ) );
            break;
         case CILTypeCode.Single:
            il.EmitLoadSingle( default( Single ) );
            break;
         case CILTypeCode.Double:
            il.EmitLoadDouble( default( Double ) );
            break;
         case CILTypeCode.Boolean:
            il.EmitLoadBoolean( default( Boolean ) );
            break;
         case CILTypeCode.Empty:
         case CILTypeCode.String:
            //case CILTypeCode.DBNull:
            il.EmitLoadNull();
            break;
         case CILTypeCode.Decimal:
            il.EmitLoadDecimal( default( Decimal ) );
            break;
         case CILTypeCode.Object:
         case CILTypeCode.SystemObject:
         case CILTypeCode.Type:
         case CILTypeCode.DateTime:
         case CILTypeCode.IntPtr:
         case CILTypeCode.UIntPtr:
            if ( tc == CILTypeCode.IntPtr || tc == CILTypeCode.UIntPtr || type.IsValueType() || type.IsGenericParameter() )
            {
               ArgumentValidator.ValidateNotNull( "Local getter", localGetter );
               var dummyLocal = localGetter( type );
               il.EmitLoadLocalAddress( dummyLocal )
                  .EmitInitObj( dummyLocal.LocalType );
               if ( emitLoadLocal )
               {
                  il.EmitLoadLocal( dummyLocal );
               }
            }
            else
            {
               il.EmitLoadNull();
            }
            break;
         default:
            throw new ArgumentException( "Unknown type code " + tc );
      }
      return il;
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit the short <paramref name="value"/> onto the stack.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="value">The <see cref="Int16"/> value to load.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadInt16( this MethodIL il, Int16 value )
   {
      return il
         .EmitLoadInt32( value )
         .EmitNumericConversion( CILTypeCode.Int32, CILTypeCode.Int16, false );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit the ushort <paramref name="value"/> onto the stack.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="value">The <see cref="UInt16"/> value to load.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   [CLSCompliant( false )]
   public static MethodIL EmitLoadUInt16( this MethodIL il, UInt16 value )
   {
      return il
         .EmitLoadInt32( value )
         .EmitNumericConversion( CILTypeCode.Int32, CILTypeCode.UInt16, false );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit the uint <paramref name="value"/> onto the stack.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="value">The <see cref="UInt32"/> value to load.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   [CLSCompliant( false )]
   public static MethodIL EmitLoadUInt32( this MethodIL il, UInt32 value )
   {
      return il
         .EmitLoadInt32( (Int32) value )
         .EmitNumericConversion( CILTypeCode.Int32, CILTypeCode.UInt32, false );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit the long <paramref name="value"/> onto the stack.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="value">The <see cref="Int64"/> value to load.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadInt64( this MethodIL il, Int64 value )
   {
      return il.Add( new LogicalOpCodeInfoWithFixedSizeOperandInt64(
         OpCodes.Ldc_I8,
         value
         ) )
         //
         // Now, emit convert to give the constant type information.
         //
         // Otherwise, it is treated as unsigned and overflow is not
         // detected if it's used in checked ops.
         //
         // Can't use EmitNumericConversion, since it will skip conversion between same types.
         //
         .Add( OpCodeEncoding.Conv_I8 );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit the ulong <paramref name="value"/> onto the stack.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="value">The <see cref="UInt64"/> value to load.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   [CLSCompliant( false )]
   public static MethodIL EmitLoadUInt64( this MethodIL il, UInt64 value )
   {
      return il.Add( new LogicalOpCodeInfoWithFixedSizeOperandInt64(
         OpCodes.Ldc_I8,
         (Int64) value
         ) )
         // Can't use EmitNumericConversion, since the value loaded has different numerical value
         .Add( OpCodeEncoding.Conv_U8 );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit the 32-bit floating point number onto the stack.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="value">The <see cref="Single"/> value to load.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadSingle( this MethodIL il, Single value )
   {
      return il.Add( new LogicalOpCodeInfoWithFixedSizeOperandSingle(
         OpCodes.Ldc_R4,
         value
         ) );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit the 64-bit floating point number onto the stack.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="value">The <see cref="Double"/> value to load.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadDouble( this MethodIL il, Double value )
   {
      return il.Add( new LogicalOpCodeInfoWithFixedSizeOperandDouble(
         OpCodes.Ldc_R8,
         value
         ) );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit char <paramref name="value"/> onto the stack.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="value">The <see cref="Char"/> value to load.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadChar( this MethodIL il, Char value )
   {
      return il
        .EmitLoadInt32( value )
        .EmitNumericConversion( CILTypeCode.Int32, CILTypeCode.Char, false );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit suitable variant of <see cref="OpCodes.Ldelem"/> for <paramref name="type"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="type">The type of the elements in the array.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadElement( this MethodIL il, CILTypeBase type )
   {
      EmitOptimalLoad(
         il,
         type,
         OpCodes.Ldelem,
         OpCodeEncoding.Ldelem_I1,
         OpCodeEncoding.Ldelem_I2,
         OpCodeEncoding.Ldelem_I4,
         OpCodeEncoding.Ldelem_I8,
         OpCodeEncoding.Ldelem_U1,
         OpCodeEncoding.Ldelem_U2,
         OpCodeEncoding.Ldelem_U4,
         OpCodeEncoding.Ldelem_R4,
         OpCodeEncoding.Ldelem_R8,
         OpCodeEncoding.Ldelem_Ref
         );
      return il;
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit <see cref="OpCodes.Ldelema"/> with <paramref name="type"/> as an operand.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="type">The type of the elements in the array.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadElementAddress( this MethodIL il, CILTypeBase type )
   {
      return il.Add(
         new LogicalOpCodeInfoWithTypeToken( OpCodes.Ldelema, type )
         );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit <see cref="OpCodes.Ldarg_0"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <returns><paramref name="il"/>.</returns>
   public static MethodIL EmitLoadThis( this MethodIL il )
   {
      return il.EmitLoadArg( 0 );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit optimal instructions to load static or non-static field belonging to current type or its parent types.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="field">The <see cref="CILField"/> to load.</param>
   /// <param name="isVolatile">Whether the <see cref="OpCodes.Volatile_"/> prefix should be used.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="field"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadThisField( this MethodIL il, CILField field, Boolean isVolatile = false )
   {
      ArgumentValidator.ValidateNotNull( "Field", field );
      OpCode code;
      if ( field.Attributes.IsStatic() )
      {
         if ( isVolatile )
         {
            il.Add( OpCodeEncoding.Volatile_ );
         }
         code = OpCodes.Ldsfld;
      }
      else
      {
         il.EmitLoadArg( 0 );
         if ( isVolatile )
         {
            il.Add( OpCodeEncoding.Volatile_ );
         }
         code = OpCodes.Ldfld;
      }
      return il.Add( new LogicalOpCodeInfoWithFieldToken(
         code,
         field
         ) );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit required instructions to load the address of the static or non-static field belonging to current type or its parent types.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="field">The <see cref="CILField"/> to load.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="field"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadThisFieldAddress( this MethodIL il, CILField field )
   {
      ArgumentValidator.ValidateNotNull( "Field", field );
      OpCode code;
      if ( field.Attributes.IsStatic() )
      {
         code = OpCodes.Ldsflda;
      }
      else
      {
         il.EmitLoadArg( 0 );
         code = OpCodes.Ldflda;
      }
      return il.Add( new LogicalOpCodeInfoWithFieldToken(
         code,
         field
         ) );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit the optimal indirect load instruction for <paramref name="type"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="type">The <see cref="CILTypeBase"/> of the target.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadIndirect( this MethodIL il, CILTypeBase type )
   {
      EmitOptimalLoad(
         il,
         type,
         OpCodes.Ldobj,
         OpCodeEncoding.Ldind_I1,
         OpCodeEncoding.Ldind_I2,
         OpCodeEncoding.Ldind_I4,
         OpCodeEncoding.Ldind_I8,
         OpCodeEncoding.Ldind_U1,
         OpCodeEncoding.Ldind_U2,
         OpCodeEncoding.Ldind_U4,
         OpCodeEncoding.Ldind_R4,
         OpCodeEncoding.Ldind_R8,
         OpCodeEncoding.Ldind_Ref
         );
      return il;
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit optimal instruction to load int <paramref name="value"/> onto the stack.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="value">The <see cref="Int32"/> value to load.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadInt32( this MethodIL il, Int32 value )
   {
      switch ( value )
      {
         case -1:
            il.Add( OpCodeEncoding.Ldc_I4_M1 );
            break;
         case 0:
            il.Add( OpCodeEncoding.Ldc_I4_0 );
            break;
         case 1:
            il.Add( OpCodeEncoding.Ldc_I4_1 );
            break;
         case 2:
            il.Add( OpCodeEncoding.Ldc_I4_2 );
            break;
         case 3:
            il.Add( OpCodeEncoding.Ldc_I4_3 );
            break;
         case 4:
            il.Add( OpCodeEncoding.Ldc_I4_4 );
            break;
         case 5:
            il.Add( OpCodeEncoding.Ldc_I4_5 );
            break;
         case 6:
            il.Add( OpCodeEncoding.Ldc_I4_6 );
            break;
         case 7:
            il.Add( OpCodeEncoding.Ldc_I4_7 );
            break;
         case 8:
            il.Add( OpCodeEncoding.Ldc_I4_8 );
            break;
         default:
            il.Add( new LogicalOpCodeInfoWithFixedSizeOperandInt32(
               value >= SByte.MinValue && value <= SByte.MaxValue ? OpCodes.Ldc_I4_S : OpCodes.Ldc_I4,
               value
               ) );
            break;
      }
      return il;
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit optimal instruction for loading the given <paramref name="local"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="local">The local to load.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentValidator">If <paramref name="local"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadLocal( this MethodIL il, LocalBuilder local )
   {
      ArgumentValidator.ValidateNotNull( "Local", local );
      EmitOptimalInstructionForShort(
         il,
         OpCodes.Ldloc,
         OpCodes.Ldloc_S,
         OpCodeEncoding.Ldloc_0,
         OpCodeEncoding.Ldloc_1,
         OpCodeEncoding.Ldloc_2,
         OpCodeEncoding.Ldloc_3,
         local.LocalIndex
         );
      return il;
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit optimal instruction for loading address of the given <paramref name="local"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="local">The local to load address of.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="local"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadLocalAddress( this MethodIL il, LocalBuilder local )
   {
      var index = local.LocalIndex;
      il.Add( new LogicalOpCodeInfoWithFixedSizeOperandUInt16(
         index <= Byte.MaxValue ? OpCodes.Ldloca_S : OpCodes.Ldloca,
         (Int16) index
      ) );
      return il;
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit instruction to load <c>null</c> onto the stack.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadNull( this MethodIL il )
   {
      return il.Add( OpCodeEncoding.Ldnull );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit given string onto the stack.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="str">The string to emit.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="str"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadString( this MethodIL il, String str )
   {
      return il.Add( new LogicalOpCodeInfoWithFixedSizeOperandString( OpCodes.Ldstr, str ) );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit required instruction (<see cref="OpCodes.Ldftn"/> or <see cref="OpCodes.Ldvirtftn"/>) to load unmanaged method pointer onto stack.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="method">The <see cref="CILMethod"/> to load unmanaged pointer of.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="method"/> is <c>null</c>.</exception>
   public static MethodIL EmitLoadUnmanagedMethodToken( this MethodIL il, CILMethod method )
   {
      return il.Add( LogicalOpCodeInfoForNormalOrVirtual.OpCodeInfoForLdFtn( method ) );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit <see cref="OpCodes.Newarr"/> with the specified <paramref name="arrayElementType"/> as operand.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="arrayElementType">The type of the array.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="arrayElementType"/> is <c>null</c>.</exception>
   public static MethodIL EmitNewArray( this MethodIL il, CILTypeBase arrayElementType )
   {
      return il.Add( new LogicalOpCodeInfoWithTypeToken(
         OpCodes.Newarr,
         arrayElementType
         ) );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit required instructions to perform numeric conversion from <paramref name="typeFrom"/> to <paramref name="typeTo"/>
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="typeFrom">The assumed type of the value on the stack.</param>
   /// <param name="typeTo">The type to which the value should be converted to.</param>
   /// <param name="checkOverflow">Whether emit instructions which will check for overflow.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="typeFrom"/> or <paramref name="typeTo"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If <paramref name="typeTo"/> is not recognized numeric type.</exception>
   public static MethodIL EmitNumericConversion( this MethodIL il, CILTypeBase typeFrom, CILTypeBase typeTo, Boolean checkOverflow )
   {
      ArgumentValidator.ValidateNotNull( "TypeFrom", typeFrom );
      ArgumentValidator.ValidateNotNull( "TypeTo", typeTo );
      if ( TypeKind.MethodSignature == typeFrom.TypeKind )
      {
         throw new ArgumentException( "Type " + typeFrom + " is not a numeric type." );
      }
      if ( TypeKind.MethodSignature == typeTo.TypeKind )
      {
         throw new ArgumentException( "Type " + typeTo + " is not a numeric type." );
      }
      return EmitNumericConversion( il, ( (CILTypeOrTypeParameter) typeFrom ).TypeCode, ( (CILTypeOrTypeParameter) typeTo ).TypeCode, checkOverflow );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit required instructions to perform numeric conversion from <paramref name="typeFrom"/> to <paramref name="typeTo"/>
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="typeFrom">The assumed type of the value on the stack.</param>
   /// <param name="typeTo">The type to which the value should be converted to.</param>
   /// <param name="checkOverflow">Whether emit instructions which will check for overflow.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If <paramref name="typeTo"/> is not recognized type code for numeric type.</exception>
   public static MethodIL EmitNumericConversion( this MethodIL il, CILTypeCode typeFrom, CILTypeCode typeTo, Boolean checkOverflow )
   {
      if ( !typeFrom.Equals( typeTo ) )
      {
         Boolean fromIsUnsigned = IsUnsigned( typeFrom );
         Boolean fromIsFloat = IsFloat( typeFrom );
         if ( CILTypeCode.Single == typeTo )
         {
            if ( fromIsUnsigned )
            {
               il.Add( OpCodeEncoding.Conv_R_Un );
            }
            il.Add( OpCodeEncoding.Conv_R4 );
         }
         else if ( CILTypeCode.Double == typeTo )
         {
            if ( fromIsUnsigned )
            {
               il.Add( OpCodeEncoding.Conv_R_Un );
            }
            il.Add( OpCodeEncoding.Conv_R8 );
         }
         else
         {
            LogicalOpCodeInfo code;
            Boolean allOK;
            if ( checkOverflow )
            {
               allOK = ( fromIsUnsigned ? CHECKED_UNSIGNED_CONV_OPCODES : CHECKED_SIGNED_CONV_OPCODES ).TryGetValue( typeTo, out code );
            }
            else
            {
               // One exception: when converting from float to to UInt64, use unsigned-style conversion
               allOK = ( fromIsUnsigned || ( CILTypeCode.UInt64 == typeTo && fromIsFloat ) ? UNCHECKED_UNSIGNED_CONV_OPCODES : UNCHECKED_SIGNED_CONV_OPCODES ).TryGetValue( typeTo, out code );
            }
            if ( allOK )
            {
               il.Add( code );
            }
            else
            {
               throw new ArgumentException( "Unknown numeric type code: " + typeTo + "." );
            }
         }
      }
      return il;
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit <see cref="OpCodes.Newobj"/> using given <paramref name="constructor"/> as operand.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="constructor">The <see cref="CILConstructor"/> to use.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="constructor"/> is <c>null</c>.</exception>
   public static MethodIL EmitNewObject( this MethodIL il, CILConstructor constructor )
   {
      return il.Add( new LogicalOpCodeInfoWithCtorToken(
         OpCodes.Newobj,
         constructor
         ) );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit <see cref="OpCodes.Nop"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitNop( this MethodIL il )
   {
      return il.Add( OpCodeEncoding.Nop );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit <see cref="OpCodes.Pop"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitPop( this MethodIL il )
   {
      return il.Add( OpCodeEncoding.Pop );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit required instructions for <c>typeof(...)</c> expression (i.e., load token and call <see cref="Type.GetTypeFromHandle"/> method).
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="targetType">The type to load token of.</param>
   /// <param name="useGDef">If <paramref name="targetType"/> is generic type definition, specifies whether to use <c>TypeDef</c> or <c>TypeSpec</c> token. If <c>true</c>, <c>TypeDef</c> token will be used; otherwise <c>TypeSpec</c> token will be used. Is ignored for types which are not generic type definitions.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitReflectionObjectOf( this MethodIL il, CILTypeBase targetType, Boolean useGDef = true )
   {
      return il.Add( new LogicalOpCodeInfoWithTypeToken(
         OpCodes.Ldtoken,
         targetType,
         useGDef
         ) )
         .EmitCall( ResolveMSCorLibMethod( il, TYPE_OF_METHOD ) );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit required instructions for <c>methodof(...)</c> expression (i.e., load method and type tokens and call <see cref="System.Reflection.MethodBase.GetMethodFromHandle(RuntimeMethodHandle, RuntimeTypeHandle)"/> method). The required cast will also be emitted.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="targetMethod">The method to load token of.</param>
   /// <param name="useGDef">If declaring type of <paramref name="targetMethod"/> is generic type definition, specifies whether to use <c>TypeDef</c> or <c>TypeSpec</c> token. If <c>true</c>, <c>TypeDef</c> token will be used; otherwise <c>TypeSpec</c> token will be used. Is ignored for types which are not generic type definitions.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="targetMethod"/> is <c>null</c>.</exception>
   public static MethodIL EmitReflectionObjectOf( this MethodIL il, CILMethod targetMethod, Boolean useGDef = true )
   {
      var methodWrapper = ResolveMSCorLibMethod( il, METHOD_OF_METHOD );
      var mscorlib = ( (MethodILImpl) il ).OwningModule.AssociatedMSCorLibModule;
      return il.Add( new LogicalOpCodeInfoWithMethodToken(
         OpCodes.Ldtoken,
         targetMethod,
         useGDef
         ) )
         .Add( new LogicalOpCodeInfoWithTypeToken(
         OpCodes.Ldtoken,
         targetMethod.DeclaringType,
         useGDef
         ) )
         .EmitCall( methodWrapper )
         .EmitCastToType( methodWrapper.GetReturnType(), mscorlib.GetTypeByName( Consts.METHOD_INFO ) );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit required instructions for <c>methodof(...)</c> expression (i.e., load method and type tokens and call <see cref="System.Reflection.MethodBase.GetMethodFromHandle(RuntimeMethodHandle, RuntimeTypeHandle)"/> method). The required cast will also be emitted.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="targetCtor">The constructor to load token of.</param>
   /// <param name="useGDef">If declaring type of <paramref name="targetCtor"/> is generic type definition, specifies whether to use <c>TypeDef</c> or <c>TypeSpec</c> token. If <c>true</c>, <c>TypeDef</c> token will be used; otherwise <c>TypeSpec</c> token will be used. Is ignored for types which are not generic type definitions.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="targetCtor"/> is <c>null</c>.</exception>
   public static MethodIL EmitReflectionObjectOf( this MethodIL il, CILConstructor targetCtor, Boolean useGDef = true )
   {
      var methodWrapper = ResolveMSCorLibMethod( il, METHOD_OF_METHOD );
      var mscorlib = ( (MethodILImpl) il ).OwningModule.AssociatedMSCorLibModule;
      return il.Add( new LogicalOpCodeInfoWithCtorToken(
         OpCodes.Ldtoken,
         targetCtor,
         useGDef
         ) )
         .Add( new LogicalOpCodeInfoWithTypeToken(
         OpCodes.Ldtoken,
         targetCtor.DeclaringType,
         useGDef
         ) )
         .EmitCall( methodWrapper )
         .EmitCastToType( methodWrapper.GetReturnType(), mscorlib.GetTypeByName( Consts.CTOR_INFO ) );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit required instructions for <c>fieldof(...)</c> expression (i.e., load field and type tokens and call <see cref="System.Reflection.FieldInfo.GetFieldFromHandle(RuntimeFieldHandle, RuntimeTypeHandle)"/> method).
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="field">The constructor to load token of.</param>
   /// <param name="useGDef">If declaring type of <paramref name="field"/> is generic type definition, specifies whether to use <c>TypeDef</c> or <c>TypeSpec</c> token. If <c>true</c>, <c>TypeDef</c> token will be used; otherwise <c>TypeSpec</c> token will be used. Is ignored for types which are not generic type definitions.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="field"/> is <c>null</c>.</exception>
   public static MethodIL EmitReflectionObjectOf( this MethodIL il, CILField field, Boolean useGDef = true )
   {
      return il.Add( new LogicalOpCodeInfoWithFieldToken(
         OpCodes.Ldtoken,
         field,
         useGDef
         ) )
         .Add( new LogicalOpCodeInfoWithTypeToken(
         OpCodes.Ldtoken,
         field.DeclaringType,
         useGDef
         ) )
         .EmitCall( ResolveMSCorLibMethod( il, FIELD_OF_METHOD ) );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit <see cref="OpCodes.Ret"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitReturn( this MethodIL il )
   {
      return il.Add( OpCodeEncoding.Ret );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit <see cref="OpCodes.Rethrow"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitRethrow( this MethodIL il )
   {
      return il.Add( OpCodeEncoding.Rethrow );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit <see cref="OpCodes.Sizeof"/> with given type.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="type">The type argument for <see cref="OpCodes.Sizeof"/>.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>.</exception>
   public static MethodIL EmitSizeOf( this MethodIL il, CILTypeBase type )
   {
      ArgumentValidator.ValidateNotNull( "Type", type );

      // TODO optimize - if Int32/UInt32, then just '4', etc.
      return il.Add( new LogicalOpCodeInfoWithTypeToken( OpCodes.Sizeof, type ) );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit suitable instruction of various indirect store instructions (e.g. <see cref="OpCodes.Stobj"/>).
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="type">The type of the value to store.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>.</exception>
   public static MethodIL EmitStoreIndirect( this MethodIL il, CILTypeBase type )
   {
      EmitOptimalStore(
         il,
         type,
         OpCodes.Stobj,
         OpCodeEncoding.Stind_I1,
         OpCodeEncoding.Stind_I2,
         OpCodeEncoding.Stind_I4,
         OpCodeEncoding.Stind_I8,
         OpCodeEncoding.Stind_R4,
         OpCodeEncoding.Stind_R8,
         OpCodeEncoding.Stind_Ref
         );
      return il;
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit suitable instruction (either <see cref="OpCodes.Starg"/> org <see cref="OpCodes.Starg_S"/>) for storing to argument.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="index">The index of the argument.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitStoreArg( this MethodIL il, Int32 index )
   {
      return il.Add( new LogicalOpCodeInfoWithFixedSizeOperandUInt16(
            index <= Byte.MaxValue ? OpCodes.Starg_S : OpCodes.Starg,
            (Int16) index
            ) );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit suitable instruction of various store instructions (e.g. <see cref="OpCodes.Stelem"/>).
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="elementType">The type of the element to store.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="elementType"/> is <c>null</c>.</exception>
   public static MethodIL EmitStoreElement( this MethodIL il, CILTypeBase elementType )
   {
      EmitOptimalStore(
         il,
         elementType,
         OpCodes.Stelem,
         OpCodeEncoding.Stelem_I1,
         OpCodeEncoding.Stelem_I2,
         OpCodeEncoding.Stelem_I4,
         OpCodeEncoding.Stelem_I8,
         OpCodeEncoding.Stelem_R4,
         OpCodeEncoding.Stelem_R8,
         OpCodeEncoding.Stelem_Ref
         );
      return il;
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit suitable instructions to store something into field belonging to this type or its base types.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="field">The <see cref="CILField"/> to store to.</param>
   /// <param name="whatToStore">Action to emit something to store to field.</param>
   /// <param name="isVolatile">Whether the <see cref="OpCodes.Volatile_"/> prefix should be used.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="field"/> or <paramref name="whatToStore"/> is <c>null</c>.</exception>
   public static MethodIL EmitStoreThisField( this MethodIL il, CILField field, Action<MethodIL> whatToStore, Boolean isVolatile = false )
   {
      return EmitStoreField( il, field, true, whatToStore, isVolatile );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit suitable instructions to store something into field belonging to other type.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="field">The <see cref="CILField"/> to store to.</param>
   /// <param name="whatToStore">Action to emit something to store to field.</param>
   /// <param name="isVolatile">Whether the <see cref="OpCodes.Volatile_"/> prefix should be used.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="field"/> or <paramref name="whatToStore"/> is <c>null</c>.</exception>
   public static MethodIL EmitStoreOtherField( this MethodIL il, CILField field, Action<MethodIL> whatToStore, Boolean isVolatile = false )
   {
      return EmitStoreField( il, field, false, whatToStore, isVolatile );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit most optimal instruction for storing to a local variable.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="localBuilder">The local to load.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="localBuilder"/> is <c>null</c>.</exception>
   public static MethodIL EmitStoreLocal( this MethodIL il, LocalBuilder localBuilder )
   {
      EmitOptimalInstructionForShort(
         il,
         OpCodes.Stloc,
         OpCodes.Stloc_S,
         OpCodeEncoding.Stloc_0,
         OpCodeEncoding.Stloc_1,
         OpCodeEncoding.Stloc_2,
         OpCodeEncoding.Stloc_3,
         localBuilder.LocalIndex
         );
      return il;
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit suitable instructions to store something to an argument.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="paramInfo">The <see cref="CILParameter"/> representing argument.</param>
   /// <param name="emitValueToStore">Action to emit value to store.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="paramInfo"/> or <paramref name="emitValueToStore"/> is <c>null</c>.</exception>
   public static MethodIL EmitStoreToArgument( this MethodIL il, CILParameter paramInfo, Action<MethodIL> emitValueToStore )
   {
      ArgumentValidator.ValidateNotNull( "Parameter", paramInfo );
      EmitStoreToArgument( il, paramInfo.Attributes.IsOut(), GetParameterIndexForEmitting( paramInfo ), paramInfo.ParameterType, emitValueToStore );
      return il;
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit suitable instruction to subtract.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="overflowAndUnsigned">If <c>null</c>, then normal <see cref="OpCodes.Sub"/> will be emitted. If <c>true</c>, the <see cref="OpCodes.Sub_Ovf_Un"/> will be emitted. If <c>false</c>, the <see cref="OpCodes.Sub_Ovf"/> will be emitted.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitSubtract( this MethodIL il, Boolean? overflowAndUnsigned = null )
   {
      return il.Add( overflowAndUnsigned.HasValue ? ( overflowAndUnsigned.Value ? OpCodeEncoding.Sub_Ovf_Un : OpCodeEncoding.Sub_Ovf ) : OpCodeEncoding.Sub );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit suitable instructions for a full <c>switch</c> clause.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="amountOfCases">How many <c>case</c> statements there will be.</param>
   /// <param name="cases">Action to emit the code for cases. First parameter is <paramref name="il"/>, second parameter will be case labels, third parameter will be default case label or <c>null</c> if <paramref name="defaultCase"/> is <c>null</c>, and fourth parameter will be switch end label.</param>
   /// <param name="defaultCase">Action to emit default statement. May be <c>null</c> if no default case is wanted for this switch statement. First parameter will be <paramref name="il"/> and second parameter will be switch end label.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">IF <paramref name="cases"/> is <c>null</c>.</exception>
   /// <remarks>The <paramref name="cases"/> must mark the case labels given to it as second parameter. This method will not do that.</remarks>
   public static MethodIL EmitSwitch( this MethodIL il, Int32 amountOfCases, Action<MethodIL, ILLabel[], ILLabel?, ILLabel> cases, Action<MethodIL, ILLabel> defaultCase )
   {
      ArgumentValidator.ValidateNotNull( "Cases handling action", cases );
      var labels = il.DefineLabels( amountOfCases );
      il.Add( new LogicalOpCodeInfoForSwitch( labels ) );
      var switchEndLabel = il.DefineLabel();
      ILLabel? defaultLabel = defaultCase == null ? (ILLabel?) null : il.DefineLabel();
      if ( defaultLabel.HasValue )
      {
         il.EmitBranch( BranchType.ALWAYS, defaultLabel.Value );
      }
      cases( il, labels, defaultLabel, switchEndLabel );
      if ( defaultLabel.HasValue )
      {
         il.MarkLabel( defaultLabel.Value );
         defaultCase( il, switchEndLabel );
      }
      return il.MarkLabel( switchEndLabel );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit <see cref="OpCodes.Tail_"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitTailcall( this MethodIL il )
   {
      return il.Add( OpCodeEncoding.Tail_ );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit <see cref="OpCodes.Throw"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   public static MethodIL EmitThrow( this MethodIL il )
   {
      return il.Add( OpCodeEncoding.Throw );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will call the <paramref name="exceptionCtor"/> and then emit <see cref="OpCodes.Throw"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="exceptionCtor">The constructor to exception.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="exceptionCtor"/> is <c>null</c>.</exception>
   public static MethodIL EmitThrowNewException( this MethodIL il, CILConstructor exceptionCtor )
   {
      return il
         .EmitNewObject( exceptionCtor )
         .EmitThrow();
   }

   /// <summary>
   /// Helper method to get the index for argument represented by <paramref name="param"/>.
   /// </summary>
   /// <param name="param">The <see cref="CILParameter"/>.</param>
   /// <returns>The argument index for <paramref name="param"/>. Will be <see cref="CILParameterBase{T}.Position"/> if owner method is static, otherwise it will be <c><see cref="CILParameterBase{T}.Position"/> + 1</c>.</returns>
   /// <exception cref="ArgumentNullException">IF <paramref name="param"/> is <c>null</c>.</exception>
   public static Int32 GetParameterIndexForEmitting( CILParameter param )
   {
      ArgumentValidator.ValidateNotNull( "Parameter", param );
      if ( param.Method.Attributes.IsStatic() )
      {
         return param.Position;
      }
      else
      {
         return param.Position + 1;
      }
   }

   /// <summary>
   /// Helper method to define many labels at once.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="amount">How many lables to define.</param>
   /// <returns>The defined labels.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <remarks>If <paramref name="amount"/> is zero or less, an empty array is returned.</remarks>
   public static ILLabel[] DefineLabels( this MethodIL il, Int32 amount )
   {
      ILLabel[] result = new ILLabel[Math.Max( 0, amount )];
      for ( var i = 0; i < amount; ++i )
      {
         result[i] = il.DefineLabel();
      }
      return result;
   }

   /// <summary>
   /// Marks label at current position of op code infos.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="label">The <see cref="ILLabel"/> to mark.</param>
   /// <returns>This <see cref="MethodIL"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If <paramref name="label"/> is invalid for this <see cref="MethodIL"/> or if it is already marked.</exception>
   public static MethodIL MarkLabel( this MethodIL il, ILLabel label )
   {
      return il.MarkLabel( label, il.OpCodeCount );
   }

   #endregion

   #region Complex methods

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit required instructions for an <c>if</c> statement.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="conditionAction">The action to emit condition, including branching. The first parameter will be <paramref name="il"/>, and second parameter will be <c>if</c> end label.</param>
   /// <param name="ifAction">The action to emit instructions inside the <c>if</c> statement. The first parameter will be <paramref name="il"/>, and second parameter will be <c>if</c> end label.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="conditionAction"/> or <paramref name="ifAction"/> is <c>null</c>.</exception>
   public static MethodIL EmitIf(
      this MethodIL il,
      Action<MethodIL, ILLabel> conditionAction,
      Action<MethodIL, ILLabel> ifAction
      )
   {
      ArgumentValidator.ValidateNotNull( "Condition", conditionAction );
      ArgumentValidator.ValidateNotNull( "If body", ifAction );

      // if (<condition>)
      var endIfLabel = il.DefineLabel();
      conditionAction( il, endIfLabel );

      // then
      ifAction( il, endIfLabel );
      // end if
      il.MarkLabel( endIfLabel );
      return il;
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit required instructons for an <c>if-else</c> statement.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="conditionAction">The action to emit condition, including branching. The first parameter will be <paramref name="il"/>, the second parameter will be the label for <c>else</c>, and the third parameter will be label for end of <c>if-else</c> statement.</param>
   /// <param name="ifAction">The action to emit instructions inside the <c>if</c> statement, excluding branching to end-if. The first parameter will be <paramref name="il"/>, the second parameter will be the label for <c>else</c>, and the third parameter will be label for end of <c>if-else</c> statement.</param>
   /// <param name="elseAction">The action to emit instructions inside the <c>else</c> statement. The first parameter will be <paramref name="il"/>, and the second parameter will be label for end of <c>if-else</c> statement.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="ifAction"/> is <c>null</c>.</exception>
   /// <remarks>If <paramref name="elseAction"/> is <c>null</c>, then this emits same code as <see cref="EmitIf"/> method.</remarks>
   public static MethodIL EmitIfElse(
      this MethodIL il,
      Action<MethodIL, ILLabel, ILLabel> conditionAction,
      Action<MethodIL, ILLabel, ILLabel> ifAction,
      Action<MethodIL, ILLabel> elseAction
      )
   {
      ArgumentValidator.ValidateNotNull( "If body", ifAction );

      // if (<condition>)
      var endIfLabel = il.DefineLabel();
      var elseLabel = il.DefineLabel();
      var curCount = il.OpCodeCount;
      if ( conditionAction != null )
      {
         conditionAction( il, elseLabel, endIfLabel );
      }

      // then
      ifAction( il, elseLabel, endIfLabel );
      if ( elseAction != null && il.OpCodeCount > curCount )
      {
         il.EmitBranch( BranchType.ALWAYS, endIfLabel );
      }

      // else
      il.MarkLabel( elseLabel );
      if ( elseAction != null )
      {
         elseAction( il, endIfLabel );
      }
      return il.MarkLabel( endIfLabel );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> to emit required instructions for a <c>for</c> loop.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="initializer">The function to initialize loop variable. First parameter will be <paramref name="il"/>, result should be the loop variable local.</param>
   /// <param name="conditionCheck">The action to perform condition check, including the branching. First parameter will be <paramref name="il"/>, second parameter will be the loop variable, and third parameter will be label to start the loop body.</param>
   /// <param name="incrementor">The action to emit the incremeting part of the loop. First parameter will be <paramref name="il"/> and second parameter will be the loop variable.</param>
   /// <param name="loopBody">The action to emit the loop body. First parameter will be <paramref name="il"/> and second parameter will be the loop variable.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="initializer"/>, <paramref name="conditionCheck"/>, <paramref name="incrementor"/> or <paramref name="loopBody"/> is <c>null</c>.</exception>
   public static MethodIL EmitSimpleForLoop(
      this MethodIL il,
      Func<MethodIL, LocalBuilder> initializer,
      Action<MethodIL, LocalBuilder, ILLabel> conditionCheck,
      Action<MethodIL, LocalBuilder> incrementor,
      Action<MethodIL, LocalBuilder> loopBody
      )
   {
      ArgumentValidator.ValidateNotNull( "Initializer", initializer );
      ArgumentValidator.ValidateNotNull( "Condition check", conditionCheck );
      ArgumentValidator.ValidateNotNull( "Incrementor", incrementor );
      ArgumentValidator.ValidateNotNull( "Loop body", loopBody );

      var loopVariable = initializer( il );

      var loopCheckLabel = il.DefineLabel();
      il.EmitBranch( BranchType.ALWAYS, loopCheckLabel );

      // Loop body
      var loopBodyStartLabel = il.DefineLabel();
      il.MarkLabel( loopBodyStartLabel );
      loopBody( il, loopVariable );

      // Increment
      incrementor( il, loopVariable );

      // Loop condition
      il.MarkLabel( loopCheckLabel );
      conditionCheck( il, loopVariable, loopBodyStartLabel );

      return il;
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit lock-free code to store something which is affected by current value of the field to store to.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="field">The <see cref="CILField"/> to load from and store to.</param>
   /// <param name="emitNewValueOnTopOfStack">The action to emit new value on top of the stack. The first parameter will be <paramref name="il"/> and second parameter will be <c>oldCurrent</c> variable (see code example).</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">IF <paramref name="emitNewValueOnTopOfStack"/> is <c>null</c>.</exception>
   /// <example>
   /// This method will emit the following code:
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CILManipulatorCodeContent.cs" region="MethodILCode1" language="C#" />
   /// </example>
   /// <remarks>The similar code is used in auto-generated add/remove methods for events.</remarks>
   public static MethodIL EmitInterlockedCompareExchangeFieldSettingLoop(
      this MethodIL il,
      CILField field,
      Action<MethodIL, LocalBuilder> emitNewValueOnTopOfStack
      )
   {
      return EmitInterlockedCompareExchangeFieldSettingLoop( il, field, null, emitNewValueOnTopOfStack );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit lock-free code to store something which is affected by current value of the field to store to.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="field">The <see cref="CILField"/> to load from and store to.</param>
   /// <param name="loadCurrent">The action to load the <c>current</c> local (see code example). May be <c>null</c>.</param>
   /// <param name="emitNewValueOnTopOfStack">The action to emit new value on top of the stack. The first parameter will be <paramref name="il"/> and second parameter will be <c>oldCurrent</c> variable (see code example).</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="emitNewValueOnTopOfStack"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If <paramref name="field"/> is of type kind <see cref="TypeKind.MethodSignature"/>.</exception>
   /// <example>
   /// This method will emit the following code:
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CILManipulatorCodeContent.cs" region="MethodILCode2" language="C#" />
   /// </example>
   /// <remarks>The similar code is used in auto-generated add/remove methods for events.</remarks>
   public static MethodIL EmitInterlockedCompareExchangeFieldSettingLoop(
      this MethodIL il,
      CILField field,
      Action<MethodIL, LocalBuilder> loadCurrent,
      Action<MethodIL, LocalBuilder> emitNewValueOnTopOfStack
      )
   {
      ArgumentValidator.ValidateNotNull( "New value emitter", emitNewValueOnTopOfStack );
      var localTypeUT = field.FieldType;
      if ( TypeKind.MethodSignature == localTypeUT.TypeKind )
      {
         throw new ArgumentException( "No suitable method found for field type " + localTypeUT );
      }
      var localType = (CILTypeOrTypeParameter) localTypeUT;

      LocalBuilder currentB = il.DeclareLocal( localType );
      LocalBuilder oldCurrentB = il.DeclareLocal( localType );
      LocalBuilder combinedB = il.DeclareLocal( localType );

      il.EmitLoadThisField( field );
      il.EmitStoreLocal( currentB );

      var loopStart = il.DefineLabel();
      il.MarkLabel( loopStart );

      if ( loadCurrent == null )
      {
         il.EmitLoadLocal( currentB );
      }
      else
      {
         loadCurrent( il, currentB );
      }
      il.EmitStoreLocal( oldCurrentB );

      emitNewValueOnTopOfStack( il, oldCurrentB );
      il.EmitStoreLocal( combinedB )

         .EmitLoadThisFieldAddress( field )
         .EmitLoadLocal( combinedB )
         .EmitLoadLocal( oldCurrentB );
      CILMethod cexMethod;
      switch ( localType.TypeCode )
      {
         case CILTypeCode.Double:
            cexMethod = ResolveMSCorLibMethod( il, INTERLOCKED_COMPARE_EXCHANGE_METHOD_DOUBLE );
            break;
         case CILTypeCode.Single:
            cexMethod = ResolveMSCorLibMethod( il, INTERLOCKED_COMPARE_EXCHANGE_METHOD_SINGLE );
            break;
         case CILTypeCode.Int32:
            cexMethod = ResolveMSCorLibMethod( il, INTERLOCKED_COMPARE_EXCHANGE_METHOD_INT32 );
            break;
         case CILTypeCode.Int64:
            cexMethod = ResolveMSCorLibMethod( il, INTERLOCKED_COMPARE_EXCHANGE_METHOD_INT64 );
            break;
         default:
            cexMethod = ResolveMSCorLibMethod( il, INTERLOCKED_COMPARE_EXCHANGE_METHOD_GDEF ).MakeGenericMethod( localType );
            break;
      }
      return il.EmitCall( cexMethod )
         .EmitStoreLocal( currentB )

         .EmitLoadLocal( currentB )
         .EmitLoadLocal( oldCurrentB )
         .EmitBranch( BranchType.IF_NOT_EQUAL_UNORDERED, loopStart );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit the <c>++i</c> expression, where <c>i</c> is name of <paramref name="var"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="var">The local variable to increment.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="var"/> is <c>null</c>.</exception>
   public static void EmitLeftPlusPlus( this MethodIL il, LocalBuilder var )
   {
      il.EmitLoadLocal( var )
         .EmitLoadInt32( 1 )
         .EmitNumericConversion( CILTypeCode.Int32, var.LocalType.GetTypeCode( CILTypeCode.Empty ), false ) // ( (MethodILImpl) il ).OwningModule.AssociatedMSCorLibModule.GetTypeByName( Consts.INT32 ), var.LocalType, false )
         .EmitAdd()
         .EmitStoreLocal( var );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit the <c>--i</c> expression, where <c>i</c> is name of <paramref name="var"/>.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="var">The local variable to decrement.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="var"/> is <c>null</c>.</exception>
   public static void EmitLeftMinusMinus( this MethodIL il, LocalBuilder var )
   {
      il.EmitLoadLocal( var )
         .EmitLoadInt32( 1 )
         .EmitNumericConversion( CILTypeCode.Int32, var.LocalType.GetTypeCode( CILTypeCode.Empty ), false ) // ( (MethodILImpl) il ).OwningModule.AssociatedMSCorLibModule.GetTypeByName( Consts.INT32 ), var.LocalType, false )
         .EmitSubtract()
         .EmitStoreLocal( var );
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit the <c>try-catch</c> statement.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="exceptionType">The type of the exception to catch. May be <c>null</c> if <paramref name="catchAction"/> is <c>null</c>.</param>
   /// <param name="tryAction">The action to emit <c>try</c>-block. The parameter will be <paramref name="il"/>.</param>
   /// <param name="catchAction">The action to emit <c>catch</c>-block. May be <c>null</c>. The parameter will be <paramref name="il"/>.</param>
   /// <param name="rethrow">Whether to rethrow exception at the end of the <c>catch</c> block.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="tryAction"/> is <c>null</c>, or if <paramref name="catchAction"/> is not <c>null</c> and <paramref name="exceptionType"/> is <c>null</c>.</exception>
   /// <remarks>If <paramref name="catchAction"/> is <c>null</c>, only <paramref name="tryAction"/> is invoked, without creating <c>try-catch</c> statement.</remarks>
   public static MethodIL EmitTryCatch(
      this MethodIL il,
      CILTypeBase exceptionType,
      Action<MethodIL> tryAction,
      Action<MethodIL> catchAction,
      Boolean rethrow
   )
   {
      ArgumentValidator.ValidateNotNull( "Try", tryAction );

      if ( catchAction == null )
      {
         tryAction( il );
      }
      else
      {
         ArgumentValidator.ValidateNotNull( "Exception type", exceptionType );

         var wrapperImpl = (MethodILImpl) il;
         // Begin try
         var tryCatch = wrapperImpl.BeginExceptionBlock();

         // Try body
         tryAction( il );

         // End try
         il.EmitLeave( tryCatch );

         // Start catch
         wrapperImpl.BeginCatchBlock( exceptionType );

         // Catch body
         catchAction( il );

         // End catch
         if ( rethrow )
         {
            il.EmitRethrow();
         }
         else
         {
            il.EmitLeave( tryCatch );
         }

         // End try-catch
         wrapperImpl.EndExceptionBlock();
      }
      return il;
   }


   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit the <c>try-finally</c> statement.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="tryAction">The action to emit <c>try</c>-block. The parameter will be <paramref name="il"/>.</param>
   /// <param name="finallyAction">The action to emit <c>finally</c>-block. May be <c>null</c>. The parameter will be <paramref name="il"/>.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="tryAction"/> is <c>null</c>.</exception>
   /// <remarks>If <paramref name="finallyAction"/> is <c>null</c>, only <paramref name="tryAction"/> is invoked, without creating <c>try-finally</c> statement.</remarks>
   public static MethodIL EmitTryFinally(
       this MethodIL il,
       Action<MethodIL> tryAction,
       Action<MethodIL> finallyAction
       )
   {
      ArgumentValidator.ValidateNotNull( "Try", tryAction );
      if ( finallyAction == null )
      {
         tryAction( il );
      }
      else
      {
         var wrapperImpl = (MethodILImpl) il;
         // Begin try
         var tryFinally = wrapperImpl.BeginExceptionBlock();

         // Try body
         tryAction( il );

         // End try

         il.EmitLeave( tryFinally );

         // Start finally
         wrapperImpl.BeginFinallyBlock();

         // Finally body
         finallyAction( il );

         // End finally
         wrapperImpl.EndExceptionBlock();
      }
      return il;
   }

   /// <summary>
   /// Adds info to <paramref name="il"/> which will emit the <c>try-catch-finally</c> statement.
   /// </summary>
   /// <param name="il">The <see cref="MethodIL"/>.</param>
   /// <param name="exceptionType">The type of the exception to catch. May be <c>null</c> if <paramref name="catchAction"/> is <c>null</c>.</param>
   /// <param name="tryAction">The action to emit <c>try</c>-block. The parameter will be <paramref name="il"/>.</param>
   /// <param name="catchAction">The action to emit <c>catch</c>-block. May be <c>null</c>. The parameter will be <paramref name="il"/>.</param>
   /// <param name="rethrow">Whether to rethrow exception at the end of the <c>catch</c> block.</param>
   /// <param name="finallyAction">The action to emit <c>finally</c>-block. May be <c>null</c>. The parameter will be <paramref name="il"/>.</param>
   /// <returns><paramref name="il"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="tryAction"/> is <c>null</c>, or if <paramref name="catchAction"/> is not <c>null</c> and <paramref name="exceptionType"/> is <c>null</c>.</exception>
   /// <remarks>If <paramref name="catchAction"/> and <paramref name="finallyAction"/> are both<c>null</c>, only <paramref name="tryAction"/> is invoked, without creating <c>try-catch-finally</c> statement. If <paramref name="catchAction"/> is <c>null</c>, then <c>try-finally</c> statement is emitted. If <paramref name="finallyAction"/> is <c>null</c>, then <c>try-catch</c> statement is emitted.</remarks>
   public static MethodIL EmitTryCatchFinally(
       this MethodIL il,
       CILTypeBase exceptionType,
       Action<MethodIL> tryAction,
       Action<MethodIL> catchAction,
       Boolean rethrow,
       Action<MethodIL> finallyAction
       )
   {
      ArgumentValidator.ValidateNotNull( "Try", tryAction );
      // Doesn't call other extension methods for performance reasons (no need to create anonymous actions)
      if ( catchAction == null && finallyAction == null )
      {
         tryAction( il );
      }
      else
      {
         var wrapperImpl = (MethodILImpl) il;

         // Begin try
         var tryFinally = wrapperImpl.BeginExceptionBlock();

         // Try body, which is try-catch
         if ( catchAction == null )
         {
            tryAction( il );
         }
         else
         {
            ArgumentValidator.ValidateNotNull( "Exception type", exceptionType );

            // Begin try
            var tryCatch = wrapperImpl.BeginExceptionBlock();

            // Try body
            tryAction( il );

            // End try
            il.EmitLeave( tryCatch );

            // Start catch
            wrapperImpl.BeginCatchBlock( exceptionType );

            // Catch body
            catchAction( il );

            // End catch
            if ( rethrow )
            {
               il.EmitRethrow();
            }
            else
            {
               il.EmitLeave( tryCatch );
            }

            // End try-catch
            wrapperImpl.EndExceptionBlock();
         }

         // End try
         il.EmitLeave( tryFinally );

         // Start finally
         wrapperImpl.BeginFinallyBlock();

         // Finally body
         finallyAction( il );

         // End finally
         wrapperImpl.EndExceptionBlock();
      }
      return il;
   }

   #endregion

   #region Helper methods

   private static MethodIL EmitStoreField( MethodIL il, CILField field, Boolean isThisField, Action<MethodIL> whatToStore, Boolean isVolatile )
   {
      ArgumentValidator.ValidateNotNull( "Field", field );
      OpCode code;
      if ( field.Attributes.IsStatic() )
      {
         whatToStore( il );
         if ( isVolatile )
         {
            il.Add( OpCodeEncoding.Volatile_ );
         }
         code = OpCodes.Stsfld;
      }
      else
      {
         if ( isThisField )
         {
            il.EmitLoadArg( 0 );
         }
         whatToStore( il );
         if ( isVolatile )
         {
            il.Add( OpCodeEncoding.Volatile_ );
         }
         code = OpCodes.Stfld;
      }
      il.Add( new LogicalOpCodeInfoWithFieldToken(
         code,
         field
         ) );
      return il;
   }

   private static void EmitOptimalInstructionForShort(
      MethodIL il,
      OpCode normalCode,
      OpCode shortForm,
      OpCodeEncoding zeroCode,
      OpCodeEncoding firstCode,
      OpCodeEncoding secondCode,
      OpCodeEncoding thirdCode,
      Int32 arg
   )
   {
      switch ( arg )
      {
         case 0:
            il.Add( zeroCode );
            break;
         case 1:
            il.Add( firstCode );
            break;
         case 2:
            il.Add( secondCode );
            break;
         case 3:
            il.Add( thirdCode );
            break;
         default:
            il.Add( new LogicalOpCodeInfoWithFixedSizeOperandUInt16(
               arg <= Byte.MaxValue ? shortForm : normalCode,
               (Int16) arg
               ) );
            break;
      }
   }

   private static void EmitStoreToArgument( MethodIL il, Boolean isOut, Int32 paramPosition, CILTypeBase paramType, Action<MethodIL> emitValueToStore )
   {
      ArgumentValidator.ValidateNotNull( "Value emitting action", emitValueToStore );

      if ( isOut || paramType.IsByRef() )
      {
         il.EmitLoadArg( paramPosition );
      }
      emitValueToStore( il );
      if ( isOut || paramType.IsByRef() )
      {
         if ( paramType.IsByRef() )
         {
            paramType = ( (CILType) paramType ).ElementType;
         }
         il.EmitStoreIndirect( paramType );
      }
      else
      {
         il.EmitStoreArg( paramPosition );
      }
   }

   private static void EmitOptimalLoad(
      MethodIL il,
      CILTypeBase type,
      OpCode codeWithType,
      OpCodeEncoding i1,
      OpCodeEncoding i2,
      OpCodeEncoding i4,
      OpCodeEncoding i8,
      OpCodeEncoding u1,
      OpCodeEncoding u2,
      OpCodeEncoding u4,
      OpCodeEncoding r4,
      OpCodeEncoding r8,
      OpCodeEncoding @ref
      )
   {
      ArgumentValidator.ValidateNotNull( "Type", type );
      switch ( type.GetTypeCode( CILTypeCode.Int32 ) )
      {
         case CILTypeCode.Boolean:
         case CILTypeCode.SByte:
            il.Add( i1 );
            break;
         case CILTypeCode.Int16:
            il.Add( i2 );
            break;
         case CILTypeCode.Int32:
            il.Add( i4 );
            break;
         case CILTypeCode.Int64:
         case CILTypeCode.UInt64:
            il.Add( i8 );
            break;
         case CILTypeCode.Byte:
            il.Add( u1 );
            break;
         case CILTypeCode.Char:
         case CILTypeCode.UInt16:
            il.Add( u2 );
            break;
         case CILTypeCode.UInt32:
            il.Add( u4 );
            break;
         case CILTypeCode.Single:
            il.Add( r4 );
            break;
         case CILTypeCode.Double:
            il.Add( r8 );
            break;
         default:
            if ( type.IsValueType() || type.IsGenericParameter() )
            {
               il.Add( new LogicalOpCodeInfoWithTypeToken(
                  codeWithType,
                  type
               ) );
            }
            else
            {
               il.Add( @ref );
            }
            break;
      }
   }

   private static void EmitOptimalStore(
      MethodIL il,
      CILTypeBase type,
      OpCode codeWithType,
      OpCodeEncoding i1,
      OpCodeEncoding i2,
      OpCodeEncoding i4,
      OpCodeEncoding i8,
      OpCodeEncoding r4,
      OpCodeEncoding r8,
      OpCodeEncoding @ref
      )
   {
      ArgumentValidator.ValidateNotNull( "Type", type );
      switch ( type.GetTypeCode( CILTypeCode.Int32 ) )
      {
         case CILTypeCode.Boolean:
         case CILTypeCode.Byte:
         case CILTypeCode.SByte:
            il.Add( i1 );
            break;
         case CILTypeCode.Int16:
         case CILTypeCode.UInt16:
         case CILTypeCode.Char:
            il.Add( i2 );
            break;
         case CILTypeCode.Int32:
         case CILTypeCode.UInt32:
            il.Add( i4 );
            break;
         case CILTypeCode.Int64:
         case CILTypeCode.UInt64:
            il.Add( i8 );
            break;
         case CILTypeCode.Single:
            il.Add( r4 );
            break;
         case CILTypeCode.Double:
            il.Add( r8 );
            break;
         default:
            if ( type.IsValueType() || type.IsGenericParameter() )
            {
               il.Add( new LogicalOpCodeInfoWithTypeToken(
                  codeWithType,
                  type
                  ) );
            }
            else
            {
               il.Add( @ref );
            }
            break;
      }
   }

   private static Boolean IsUnsigned( CILTypeCode type )
   {
      switch ( type )
      {
         case CILTypeCode.Byte:
         case CILTypeCode.UInt16:
         case CILTypeCode.UInt32:
         case CILTypeCode.UInt64:
         case CILTypeCode.Char:
            return true;
         default:
            return false;
      }
   }

   private static Boolean IsFloat( CILTypeCode type )
   {
      switch ( type )
      {
         case CILTypeCode.Single:
         case CILTypeCode.Double:
            return true;
         default:
            return false;
      }
   }

   private static void EmitLoadDecimalWithMultipleArgsCtor( MethodIL il, Decimal value )
   {
      var bits = Decimal.GetBits( value );
      il.EmitLoadInt32( bits[0] )
         .EmitLoadInt32( bits[1] )
         .EmitLoadInt32( bits[2] )
         .EmitLoadBoolean( ( bits[3] & 0x80000000 ) != 0 )
         .EmitLoadByte( (Byte) ( bits[3] >> 16 ) )
         .EmitNewObject( ResolveMSCorLibCtor( il, DECIMAL_CTOR_MULTIPLE ) );
   }

   private static CILMethod ResolveMSCorLibMethod( MethodIL il, System.Reflection.MethodInfo nativeMethod )
   {
      var mscorlib = ( (MethodILImpl) il ).OwningModule.AssociatedMSCorLibModule;
      var pCount = nativeMethod.GetParameters().Length;
      return mscorlib.GetTypeByName( nativeMethod.DeclaringType.FullName ).DeclaredMethods.First( m => m.Attributes == (MethodAttributes) nativeMethod.Attributes && m.Name == nativeMethod.Name && m.Parameters.Count == pCount && m.IsGenericMethodDefinition() == nativeMethod.IsGenericMethodDefinition );
   }

   private static CILConstructor ResolveMSCorLibCtor( MethodIL il, System.Reflection.ConstructorInfo nativeCtor )
   {
      var mscorlib = ( (MethodILImpl) il ).OwningModule.AssociatedMSCorLibModule;
      var pCount = nativeCtor.GetParameters().Length;
      return mscorlib.GetTypeByName( nativeCtor.DeclaringType.FullName ).Constructors.First( c => c.Attributes == (MethodAttributes) nativeCtor.Attributes && c.Parameters.Count == pCount );
   }

   private static MethodIL Add( this MethodIL il, OpCodeEncoding noOperandOpCode )
   {
      return il.Add( LogicalOpCodeInfoWithNoOperand.GetInstanceFor( noOperandOpCode ) );
   }

   #endregion
}