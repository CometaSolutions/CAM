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
using System.Linq;
using System.Collections.Generic;
using CILAssemblyManipulator.Logical;
using CommonUtils;
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Logical
{
   /// <summary>
   /// This is abstract class for all <see cref="LogicalOpCodeInfo"/>s with one possible <see cref="OpCode"/>.
   /// </summary>
   public abstract class LogicalOpCodeInfoWithOneOpCode : LogicalOpCodeInfo
   {
      internal readonly OpCode _opCode;

      internal LogicalOpCodeInfoWithOneOpCode( OpCode code )
      {
         this._opCode = code;
      }

      //internal override Int32 BranchTargetCount
      //{
      //   get
      //   {
      //      return 0;
      //   }
      //}

      /// <summary>
      /// Returns the <see cref="OpCode"/> to emit.
      /// </summary>
      /// <value>The <see cref="OpCode"/> to emit.</value>
      public OpCode Code
      {
         get
         {
            return this._opCode;
         }
      }

      /// <inheritdoc />
      public override String ToString()
      {
         return this._opCode.ToString();
      }
   }

   /// <summary>
   /// This is <see cref="LogicalOpCodeInfo"/> for all <see cref="OpCode"/>s which take no operand.
   /// </summary>
   public sealed class LogicalOpCodeInfoWithNoOperand : LogicalOpCodeInfoWithOneOpCode
   {
      private static IDictionary<OpCodeEncoding, LogicalOpCodeInfoWithNoOperand> Instances = OpCodeInfoWithNoOperand.OperandlessCodes.ToDictionary(
         code => code,
         code => new LogicalOpCodeInfoWithNoOperand( code )
         );
      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoWithNoOperand"/>.
      /// </summary>
      /// <param name="code">The <see cref="OpCode"/>.</param>
      private LogicalOpCodeInfoWithNoOperand( OpCodeEncoding code )
         : base( OpCodes.GetCodeFor( code ) )
      {
         ///// <exception cref="ArgumentException">If <see cref="OpCode.OperandType"/> property of <paramref name="code"/> is not <see cref="OperandType.InlineNone"/>.</exception>
         //if ( code.OperandType != OperandType.InlineNone )
         //{
         //   throw new ArgumentException( "The op-code " + code + " has " + code.OperandType + " instead of " + OperandType.InlineNone + "." );
         //}
      }

      //internal override void EmitOpCode( MethodILWriter emittingContext )
      //{
      //   emittingContext.Emit( this._opCode );
      //}

      //internal override Int32 MinSize
      //{
      //   get
      //   {
      //      return this._opCode.Size;
      //   }
      //}

      //internal override Int32 MaxSize
      //{
      //   get
      //   {
      //      return this._opCode.Size;
      //   }
      //}

      /// <inheritdoc />
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandNone;
         }
      }

      public static LogicalOpCodeInfoWithNoOperand GetInstanceFor( OpCodeEncoding code )
      {
         LogicalOpCodeInfoWithNoOperand retVal;
         if ( Instances.TryGetValue( code, out retVal ) )
         {
            return retVal;
         }
         else
         {
            throw new ArgumentException( "Opcode " + code + " is not operandless opcode." );
         }
      }

      public static LogicalOpCodeInfoWithNoOperand GetInstanceFor( OpCode code )
      {
         return GetInstanceFor( code.Value );
      }
   }

   /// <summary>
   /// This is abstract class for all <see cref="LogicalOpCodeInfo"/>s with fixed-size operand.
   /// </summary>
   public abstract class LogicalOpCodeInfoWithFixedSizeOperand : LogicalOpCodeInfoWithOneOpCode
   {
      //internal const Byte TOKEN_SIZE = 4;

      //internal readonly Byte _argSize;

      internal LogicalOpCodeInfoWithFixedSizeOperand( OpCode opCode ) //, Byte argSize )
         : base( opCode )
      {
         //this._argSize = argSize;
      }

      //internal override Int32 MinSize
      //{
      //   get
      //   {
      //      return this._opCode.Size + this._argSize;
      //   }
      //}

      //internal override Int32 MaxSize
      //{
      //   get
      //   {
      //      return this._opCode.Size + this._argSize;
      //   }
      //}
   }

   /// <summary>
   /// This is abstract class for all <see cref="LogicalOpCodeInfo"/>s which accept a token as operand.
   /// </summary>
   public abstract class LogicalOpCodeInfoWithTokenOperand : LogicalOpCodeInfoWithFixedSizeOperand
   {
      internal readonly Boolean _useGDefIfPossible;

      internal LogicalOpCodeInfoWithTokenOperand( OpCode opCode, Boolean useGDefIfPossible )
         : base( opCode ) //, TOKEN_SIZE )
      {
         this._useGDefIfPossible = useGDefIfPossible;
      }

      /// <summary>
      /// This setting provides a way to distinguish the emitting TypeDef or TypeSpec token within signatures or tokens.
      /// See remarks for more information.
      /// </summary>
      /// <value>Whether to emit TypeDef or TypeSpec token within signatures or tokens.</value>
      /// <remarks>
      /// Consider the following code:
      /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CILManipulatorCodeContent.cs" region="EmitTypeDefOrTypeSpec" language="C#" />
      /// In order to emit code which would produce the sample above, using this property is required.
      /// If this property is <c>true</c>, then emitted code will load token suitable to <c>type1</c>, that is, a TypeDef token for generic definition rather than generic instantiation.
      /// Consequentially, if this property is <c>false</c>, then emitted code will load token suitable to <c>type2</c>, that is, a TypeSpec token for generic instantiation.
      /// </remarks>
      public Boolean UseGenericDefinitionIfPossible
      {
         get
         {
            return this._useGDefIfPossible;
         }
      }
   }

   /// <summary>
   /// This is class which will emit <see cref="OpCode"/> with type token as operand.
   /// </summary>
   public sealed class LogicalOpCodeInfoWithTypeToken : LogicalOpCodeInfoWithTokenOperand
   {
      private readonly CILTypeBase _type;

      /// <summary>
      /// Creates new instance of <see cref="LogicalOpCodeInfoWithTypeToken"/> with specified values.
      /// </summary>
      /// <param name="opCode">The <see cref="OpCode"/> to emit.</param>
      /// <param name="type">The <see cref="CILTypeBase"/> to have as operand.</param>
      /// <param name="useGDefIfPossible">
      /// If this is <c>true</c>, and <paramref name="type"/> is generic type definition, and the declaring type of method containing this IL is <paramref name="type"/>, then a TypeDef-token will be emitted instead of TypeSpec.
      /// The default behaviour in such scenario is to emit TypeSpec token.
      /// </param>
      /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>.</exception>
      public LogicalOpCodeInfoWithTypeToken( OpCode opCode, CILTypeBase type, Boolean useGDefIfPossible = false )
         : base( opCode, useGDefIfPossible )
      {
         ArgumentValidator.ValidateNotNull( "Type", type );
         this._type = type;
      }

      //internal override void EmitOpCode( MethodILWriter emittingContext )
      //{
      //   emittingContext.Emit( this._opCode, this._type, this._useGDefIfPossible );
      //}

      /// <summary>
      /// Gets the <see cref="CILTypeBase"/> which is the operand of the <see cref="OpCode"/> being emitted.
      /// </summary>
      /// <value>The <see cref="CILTypeBase"/> which is the operand of the <see cref="OpCode"/> being emitted.</value>
      public CILTypeBase ReflectionObject
      {
         get
         {
            return this._type;
         }
      }

      /// <inheritdoc />
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandTypeToken;
         }
      }
   }

   /// <summary>
   /// This is class which will emit <see cref="OpCode"/> with <see cref="CILField"/> token as operand.
   /// </summary>
   public sealed class LogicalOpCodeInfoWithFieldToken : LogicalOpCodeInfoWithTokenOperand
   {
      private readonly CILField _field;

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoWithFieldToken"/> with specified values.
      /// </summary>
      /// <param name="opCode">The <see cref="OpCode"/> to emit.</param>
      /// <param name="field">The <see cref="CILField"/> to use as operand.</param>
      /// <param name="useGDefIfPossible">
      /// If this is <c>true</c>, and the declaring type of <paramref name="field"/> is generic type definition, and the declaring type of method containing this IL is declaring type of <paramref name="field"/>, then a TypeDef-token will be used instead of TypeSpec when emitting declaring type of the field.
      /// The default behaviour in such scenario is to emit TypeSpec token.
      /// </param>
      /// <exception cref="ArgumentNullException">If <paramref name="field"/> is <c>null</c>.</exception>
      public LogicalOpCodeInfoWithFieldToken( OpCode opCode, CILField field, Boolean useGDefIfPossible = false )
         : base( opCode, useGDefIfPossible )
      {
         ArgumentValidator.ValidateNotNull( "Field", field );
         this._field = field;
      }

      //internal override void EmitOpCode( MethodILWriter emittingContext )
      //{
      //   emittingContext.Emit( this._opCode, this._field, this._useGDefIfPossible );
      //}

      /// <summary>
      /// Gets the <see cref="CILField"/> which is the operand of the <see cref="OpCode"/> being emitted.
      /// </summary>
      /// <value>The <see cref="CILField"/> which is the operand of the <see cref="OpCode"/> being emitted.</value>
      public CILField ReflectionObject
      {
         get
         {
            return this._field;
         }
      }

      /// <inheritdoc />
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandFieldToken;
         }
      }
   }

   /// <summary>
   /// This is class which will emit <see cref="OpCode"/> with <see cref="CILMethod"/> token as operand.
   /// If the <see cref="OpCode"/> to emit is <see cref="OpCodes.Call"/>, <see cref="OpCodes.Callvirt"/>, <see cref="OpCodes.Ldftn"/> or <see cref="OpCodes.Ldvirtftn"/>, it is advicable to use the static factory methods in <see cref="LogicalOpCodeInfoForNormalOrVirtual"/> class.
   /// </summary>
   public sealed class LogicalOpCodeInfoWithMethodToken : LogicalOpCodeInfoWithTokenOperand
   {
      private readonly CILMethod _method;

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoWithMethodToken"/> with specified values.
      /// </summary>
      /// <param name="opCode">The <see cref="OpCode"/> to emit.</param>
      /// <param name="method">The <see cref="CILMethod"/> to use as operand.</param>
      /// <param name="useGDefIfPossible">
      /// If this is <c>true</c>, and the declaring type of <paramref name="method"/> is generic type definition, and the declaring type of method containing this IL is declaring type of <paramref name="method"/>, then a TypeDef-token will be used instead of TypeSpec when emitting declaring type of the method.
      /// The default behaviour in such scenario is to emit TypeSpec token.
      /// </param>
      /// <exception cref="ArgumentNullException">If <paramref name="method"/> is <c>null</c>.</exception>
      public LogicalOpCodeInfoWithMethodToken( OpCode opCode, CILMethod method, Boolean useGDefIfPossible = false )
         : base( opCode, useGDefIfPossible )
      {
         ArgumentValidator.ValidateNotNull( "Method", method );
         this._method = method;
      }

      //internal override void EmitOpCode( MethodILWriter emittingContext )
      //{
      //   emittingContext.Emit( this._opCode, this._method, this._useGDefIfPossible );
      //}

      /// <summary>
      /// Gets the <see cref="CILMethod"/> which is the operand of the <see cref="OpCode"/> being emitted.
      /// </summary>
      /// <value>The <see cref="CILMethod"/> which is the operand of the <see cref="OpCode"/> being emitted.</value>
      public CILMethod ReflectionObject
      {
         get
         {
            return this._method;
         }
      }

      /// <inheritdoc />
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandMethodToken;
         }
      }
   }

   /// <summary>
   /// This is class which will automatically emit suitable <see cref="OpCode"/> with token for operand <see cref="CILMethod"/>, based on whether the method is virtual or not.
   /// Instances of this class can be created via static factory methods of this class.
   /// </summary>
   public sealed class LogicalOpCodeInfoForNormalOrVirtual : LogicalOpCodeInfo
   {
      private readonly CILMethod _method;
      private readonly OpCode _normal;
      private readonly OpCode _virtual;
      //private readonly Int32 _minSize;
      //private readonly Int32 _maxSize;

      /// <summary>
      /// Creates a new <see cref="LogicalOpCodeInfoForNormalOrVirtual"/> with specified values.
      /// </summary>
      /// <param name="method">The <see cref="CILMethod"/> to use as operand.</param>
      /// <param name="normal">The <see cref="OpCode"/> to emit when the method will not be virtual.</param>
      /// <param name="aVirtual">The <see cref="OpCode"/> to emit when the method will be virtual.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="method"/> is <c>null</c>.</exception>
      public LogicalOpCodeInfoForNormalOrVirtual( CILMethod method, OpCode normal, OpCode aVirtual )
      {
         ArgumentValidator.ValidateNotNull( "Method", method );

         this._method = method;
         this._normal = normal;
         this._virtual = aVirtual;
         //this._minSize = Math.Min( normal.Size, aVirtual.Size ) + LogicalOpCodeInfoWithFixedSizeOperand.TOKEN_SIZE;
         //this._maxSize = Math.Max( normal.Size, aVirtual.Size ) + LogicalOpCodeInfoWithFixedSizeOperand.TOKEN_SIZE;
      }

      //internal override void EmitOpCode( MethodILWriter emittingContext )
      //{
      //   emittingContext.EmitNormalOrVirtual( this._method, this._normal, this._virtual );
      //}

      //internal override Int32 MinSize
      //{
      //   get
      //   {
      //      return this._minSize;
      //   }
      //}

      //internal override Int32 MaxSize
      //{
      //   get
      //   {
      //      return this._maxSize;
      //   }
      //}

      //internal override Int32 BranchTargetCount
      //{
      //   get
      //   {
      //      return 0;
      //   }
      //}

      /// <summary>
      /// Gets the <see cref="CILMethod"/> which is the operand of the <see cref="OpCode"/> being emitted.
      /// </summary>
      /// <value>The <see cref="CILMethod"/> which is the operand of the <see cref="OpCode"/> being emitted.</value>
      public CILMethod ReflectionObject
      {
         get
         {
            return this._method;
         }
      }

      /// <inheritdoc />
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.NormalOrVirtual;
         }
      }

      /// <summary>
      /// Returns any variable arguments if this is call to vararg method.
      /// TODO not implemented.
      /// </summary>
      /// <value>Variable arguments if this is call to vararg method or <c>null</c>.</value>
      public Tuple<CILCustomModifier[], CILTypeBase>[] VarArgs
      {
         get
         {
            return null;
         }
      }

      /// <summary>
      /// Gets the <see cref="OpCode"/> to emit when the operand <see cref="CILMethod"/> is not virtual.
      /// </summary>
      /// <value>The <see cref="OpCode"/> to emit when the operand <see cref="CILMethod"/> is not virtual.</value>
      public OpCode NormalCode
      {
         get
         {
            return this._normal;
         }
      }

      /// <summary>
      /// Gets the <see cref="OpCode"/> to emit when the operand <see cref="CILMethod"/> is virtual.
      /// </summary>
      /// <value>The <see cref="OpCode"/> to emit when the operand <see cref="CILMethod"/> is virtual.</value>
      public OpCode VirtualCode
      {
         get
         {
            return this._virtual;
         }
      }

      /// <summary>
      /// Creates new instance of <see cref="LogicalOpCodeInfoForNormalOrVirtual"/> which will emit either <see cref="OpCodes.Call"/> or <see cref="OpCodes.Callvirt"/> for the specified method.
      /// </summary>
      /// <param name="method">The method to emit call to.</param>
      /// <returns>New instance of <see cref="LogicalOpCodeInfoForNormalOrVirtual"/> which will emit either <see cref="OpCodes.Call"/> or <see cref="OpCodes.Callvirt"/> for the specified method.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="method"/> is <c>null</c>.</exception>
      public static LogicalOpCodeInfoForNormalOrVirtual OpCodeInfoForCall( CILMethod method )
      {
         return new LogicalOpCodeInfoForNormalOrVirtual( method, OpCodes.Call, OpCodes.Callvirt );
      }

      /// <summary>
      /// Creates new instance of <see cref="LogicalOpCodeInfoForNormalOrVirtual"/> which will emit either <see cref="OpCodes.Ldftn"/> or <see cref="OpCodes.Ldvirtftn"/> for the specified method.
      /// </summary>
      /// <param name="method">The method to emit loading function pointer to.</param>
      /// <returns>New instance of <see cref="LogicalOpCodeInfoForNormalOrVirtual"/> which will emit either <see cref="OpCodes.Ldftn"/> or <see cref="OpCodes.Ldvirtftn"/> for the specified method.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="method"/> is <c>null</c>.</exception>
      public static LogicalOpCodeInfoForNormalOrVirtual OpCodeInfoForLdFtn( CILMethod method )
      {
         return new LogicalOpCodeInfoForNormalOrVirtual( method, OpCodes.Ldftn, OpCodes.Ldvirtftn );
      }
   }

   /// <summary>
   /// This is class which will emit <see cref="OpCode"/> with <see cref="CILConstructor"/> token as operand.
   /// </summary>
   public sealed class LogicalOpCodeInfoWithCtorToken : LogicalOpCodeInfoWithTokenOperand
   {
      private readonly CILConstructor _ctor;

      /// <summary>
      /// Creats a new instance of <see cref="LogicalOpCodeInfoWithCtorToken"/> with specified values.
      /// </summary>
      /// <param name="opCode">The <see cref="OpCode"/> to emit.</param>
      /// <param name="ctor">The <see cref="CILConstructor"/> to use as operand.</param>
      /// <param name="useGDefIfPossible">
      /// If this is <c>true</c>, and the declaring type of <paramref name="ctor"/> is generic type definition, and the declaring type of method containing this IL is declaring type of <paramref name="ctor"/>, then a TypeDef-token will be used instead of TypeSpec when emitting declaring type of the method.
      /// The default behaviour in such scenario is to emit TypeSpec token.
      /// </param>
      /// <exception cref="ArgumentNullException">If <paramref name="ctor"/> is <c>null</c>.</exception>
      public LogicalOpCodeInfoWithCtorToken( OpCode opCode, CILConstructor ctor, Boolean useGDefIfPossible = false )
         : base( opCode, useGDefIfPossible )
      {
         ArgumentValidator.ValidateNotNull( "Constructor", ctor );
         this._ctor = ctor;
      }

      //internal override void EmitOpCode( MethodILWriter emittingContext )
      //{
      //   emittingContext.Emit( this._opCode, this._ctor, this._useGDefIfPossible );
      //}

      /// <summary>
      /// Gets the <see cref="CILConstructor"/> which is the operand of the <see cref="OpCode"/> being emitted.
      /// </summary>
      /// <value>The <see cref="CILConstructor"/> which is the operand of the <see cref="OpCode"/> being emitted.</value>
      public CILConstructor ReflectionObject
      {
         get
         {
            return this._ctor;
         }
      }

      /// <inheritdoc />
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandCtorToken;
         }
      }
   }

   /// <summary>
   /// This is class which will emit <see cref="OpCodes.Calli"/> with <see cref="CILMethodSignature"/> token as operand.
   /// </summary>
   public sealed class LogicalOpCodeInfoWithMethodSig : LogicalOpCodeInfoWithTokenOperand
   {
      private readonly CILMethodSignature _methodSig;
      private readonly Tuple<CILCustomModifier[], CILTypeBase>[] _varArgs;

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoWithMethodSig"/> with specified values.
      /// </summary>
      /// <param name="methodSig">The <see cref="CILMethodSignature"/>.</param>
      /// <param name="varArgs">The variable arguments for this call site. May be <c>null</c> or empty for non-varargs call.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="methodSig"/> is <c>null</c>.</exception>
      public LogicalOpCodeInfoWithMethodSig( CILMethodSignature methodSig, Tuple<CILCustomModifier[], CILTypeBase>[] varArgs )
         : base( OpCodes.Calli, false )
      {
         ArgumentValidator.ValidateNotNull( "Method signature", methodSig );

         this._methodSig = methodSig;
         this._varArgs = varArgs;
      }

      //internal override void EmitOpCode( MethodILWriter emittingContext )
      //{
      //   emittingContext.Emit( this._methodSig, this._varArgs );
      //}

      /// <summary>
      /// Gets the <see cref="CILMethodSignature"/> which is the operand of the <see cref="OpCode"/> being emitted.
      /// </summary>
      /// <value>The <see cref="CILMethodSignature"/> which is the operand of the <see cref="OpCode"/> being emitted.</value>
      public CILMethodSignature ReflectionObject
      {
         get
         {
            return this._methodSig;
         }
      }

      /// <summary>
      /// Gets the variable arguments for this call site.
      /// </summary>
      /// <value>The variable arguments for this call site.</value>
      public Tuple<CILCustomModifier[], CILTypeBase>[] VarArgs
      {
         get
         {
            return this._varArgs;
         }
      }

      /// <inheritdoc />
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandMethodSigToken;
         }
      }
   }

   /// <summary>
   /// This is class which will emit <see cref="OpCodes.Ldstr"/> with a string token as operand.
   /// </summary>
   public sealed class LogicalOpCodeInfoWithFixedSizeOperandString : LogicalOpCodeInfoWithFixedSizeOperand
   {
      private readonly String _string;

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoWithFixedSizeOperandString"/>
      /// </summary>
      /// <param name="str"></param>
      public LogicalOpCodeInfoWithFixedSizeOperandString( OpCode code, String str )
         : base( code ) //, TOKEN_SIZE )
      {
         ArgumentValidator.ValidateNotNull( "String", str );
         this._string = str;
      }

      //internal override void EmitOpCode( MethodILWriter emittingContext )
      //{
      //   emittingContext.Emit( this._opCode, this._string );
      //}

      public String String
      {
         get
         {
            return this._string;
         }
      }

      /// <inheritdoc/>
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandString;
         }
      }
   }

   /// <summary>
   /// This is class which will emit specified <see cref="OpCode"/> with specified <c>short</c> or <c>byte</c> operand.
   /// </summary>
   public sealed class LogicalOpCodeInfoWithFixedSizeOperandUInt16 : LogicalOpCodeInfoWithFixedSizeOperand
   {
      private readonly Int16 _int16;

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoWithFixedSizeOperandUInt16"/> with specified <see cref="OpCode"/> and operand.
      /// </summary>
      /// <param name="opCode">The <see cref="OpCode"/> to emit.</param>
      /// <param name="int16">The operand.</param>
      /// <remarks>
      /// CIL will interpret <paramref name="int16"/> as <see cref="UInt16"/>.
      /// Additionally, if <see cref="OpCode.OperandType"/> property of <paramref name="opCode"/> is <see cref="OperandType.ShortInlineVar"/>, the <paramref name="int16"/> will emitted as <see cref="Byte"/>.
      /// </remarks>
      public LogicalOpCodeInfoWithFixedSizeOperandUInt16( OpCode opCode, Int16 int16 )
         : base( opCode ) //, opCode.OperandType == OperandType.ShortInlineVar ? (Byte) 1 : (Byte) 2 )
      {
         this._int16 = int16;
      }

      //internal override void EmitOpCode( MethodILWriter emittingContext )
      //{
      //   if ( this._argSize == 1 )
      //   {
      //      emittingContext.Emit( this._opCode, (Byte) this._int16 );
      //   }
      //   else
      //   {
      //      emittingContext.Emit( this._opCode, this._int16 );
      //   }
      //}

      /// <inheritdoc/>
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandUInt16;
         }
      }
   }

   /// <summary>
   /// This is class which will emit specified <see cref="OpCode"/> with specified <c>int</c> or <c>sbyte</c> operand.
   /// </summary>
   public sealed class LogicalOpCodeInfoWithFixedSizeOperandInt32 : LogicalOpCodeInfoWithFixedSizeOperand
   {

      private readonly Int32 _int32;

      /// <summary>
      /// Creates a new instane of <see cref="LogicalOpCodeInfoWithFixedSizeOperandInt32"/> with specified <see cref="OpCode"/> and operand.
      /// </summary>
      /// <param name="opCode">The <see cref="OpCode"/> to emit.</param>
      /// <param name="int32">The operand.</param>
      /// <remarks>
      /// If <see cref="OpCode.OperandType"/> property of <paramref name="opCode"/> is <see cref="OperandType.ShortInlineI"/>, the <paramref name="int32"/> will be emitted as <see cref="SByte"/>.
      /// </remarks>
      public LogicalOpCodeInfoWithFixedSizeOperandInt32( OpCode opCode, Int32 int32 )
         : base( opCode ) //, opCode.OperandType == OperandType.ShortInlineI ? (Byte) 1 : (Byte) 4 )
      {
         this._int32 = int32;
      }

      //internal override void EmitOpCode( MethodILWriter emittingContext )
      //{
      //   if ( this._argSize == 1 )
      //   {
      //      emittingContext.EmitAsSByte( this._opCode, this._int32 );
      //   }
      //   else
      //   {
      //      emittingContext.Emit( this._opCode, this._int32 );
      //   }
      //}

      /// <inheritdoc/>
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandInt32;
         }
      }
   }

   /// <summary>
   /// This is class which will emit specified <see cref="OpCode"/> with specified <c>long</c> operand.
   /// </summary>
   public sealed class LogicalOpCodeInfoWithFixedSizeOperandInt64 : LogicalOpCodeInfoWithFixedSizeOperand
   {
      private readonly Int64 _int64;

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoWithFixedSizeOperandInt64"/> with specified <see cref="OpCode"/> and operand.
      /// </summary>
      /// <param name="opCode">The <see cref="OpCode"/> to emit.</param>
      /// <param name="int64">The operand.</param>
      public LogicalOpCodeInfoWithFixedSizeOperandInt64( OpCode opCode, Int64 int64 )
         : base( opCode ) //, (Byte) 8 )
      {
         this._int64 = int64;
      }

      //internal override void EmitOpCode( MethodILWriter emittingContext )
      //{
      //   emittingContext.Emit( this._opCode, this._int64 );
      //}

      /// <inheritdoc/>
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandInt64;
         }
      }
   }

   /// <summary>
   /// This is class which will emit specified <see cref="OpCode"/> with specified <c>float</c> operand.
   /// </summary>
   public sealed class LogicalOpCodeInfoWithFixedSizeOperandSingle : LogicalOpCodeInfoWithFixedSizeOperand
   {
      private readonly Single _single;

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoWithFixedSizeOperandSingle"/> with specified <see cref="OpCode"/> and operand.
      /// </summary>
      /// <param name="opCode">The <see cref="OpCode"/> to emit.</param>
      /// <param name="single">The operand.</param>
      public LogicalOpCodeInfoWithFixedSizeOperandSingle( OpCode opCode, Single single )
         : base( opCode ) //, (Byte) 4 )
      {
         this._single = single;
      }

      //internal override void EmitOpCode( MethodILWriter emittingContext )
      //{
      //   emittingContext.Emit( this._opCode, this._single );
      //}

      /// <inheritdoc/>
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandR4;
         }
      }
   }

   /// <summary>
   /// This is class which will emit specified <see cref="OpCode"/> with specified <c>double</c> operand.
   /// </summary>
   public sealed class LogicalOpCodeInfoWithFixedSizeOperandDouble : LogicalOpCodeInfoWithFixedSizeOperand
   {
      private readonly Double _double;

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoWithFixedSizeOperandDouble"/> with specified <see cref="OpCode"/> and operand.
      /// </summary>
      /// <param name="opCode">The <see cref="OpCode"/> to emit.</param>
      /// <param name="dbl">The operand.</param>
      public LogicalOpCodeInfoWithFixedSizeOperandDouble( OpCode opCode, Double dbl )
         : base( opCode ) // , (Byte) 8 )
      {
         this._double = dbl;
      }

      //internal override void EmitOpCode( MethodILWriter emittingContext )
      //{
      //   emittingContext.Emit( this._opCode, this._double );
      //}

      /// <inheritdoc/>
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandR8;
         }
      }
   }

   /// <summary>
   /// This is base class for <see cref="LogicalOpCodeInfo"/>s having per-instance size information.
   /// </summary>
   public abstract class LogicalDynamicOpCodeInfo : LogicalOpCodeInfo
   {
      //private readonly Int32 _minSize;
      //private readonly Int32 _maxSize;

      //internal const Int32 LONG_BRANCH_OPERAND_SIZE = sizeof( Int32 );
      //internal const Int32 SHORT_BRANCH_OPERAND_SIZE = sizeof( SByte );

      //internal LogicalDynamicOpCodeInfo( Int32 fixedSize )
      //   : this( fixedSize, fixedSize )
      //{

      //}

      //internal LogicalDynamicOpCodeInfo( Int32 minSize, Int32 maxSize )
      //{
      //   if ( minSize < 1 )
      //   {
      //      throw new ArgumentException( "Minimum size must be at least 1, but given: " + minSize + "." );
      //   }
      //   else if ( maxSize < minSize )
      //   {
      //      throw new ArgumentException( "Maximum size must be at least same as min size, but with given min size " + minSize + " max size is " + maxSize + "." );
      //   }

      //   this._minSize = minSize;
      //   this._maxSize = maxSize;
      //}

      //internal override Int32 MinSize
      //{
      //   get
      //   {
      //      return this._minSize;
      //   }
      //}

      //internal override Int32 MaxSize
      //{
      //   get
      //   {
      //      return this._maxSize;
      //   }
      //}
   }

   /// <summary>
   /// This is abstract class for <see cref="LogicalOpCodeInfo"/>s emitting branch or leave instruction.
   /// </summary>
   public abstract class LogicalOpCodeInfoForBranchingControlFlow : LogicalDynamicOpCodeInfo
   {


      internal readonly ILLabel _targetLabel;

      internal LogicalOpCodeInfoForBranchingControlFlow( /* Int32 min, Int32 max,*/ ILLabel targetLabel )
      //: base( min, max )
      {
         this._targetLabel = targetLabel;
      }

      //internal override void EmitOpCode( MethodILWriter emittingContext )
      //{
      //   emittingContext.Emit( emittingContext.GetMaxForLabel( this._targetLabel ) <= SByte.MaxValue ? this.ShortForm : this.LongForm, this._targetLabel );
      //}

      internal override Int32 BranchTargetCount
      {
         get
         {
            return 1;
         }
      }

      /// <summary>
      /// Gets the version of the <see cref="OpCode"/> that uses short operand.
      /// </summary>
      /// <value>The version of the <see cref="OpCode"/> that uses short operand.</value>
      public abstract OpCode ShortForm { get; }

      /// <summary>
      /// Gets the version of the <see cref="OpCode"/> that uses long operand.
      /// </summary>
      /// <value>The version of the <see cref="OpCode"/> that uses long operand.</value>
      public abstract OpCode LongForm { get; }

      /// <summary>
      /// Gets the target <see cref="ILLabel"/> for this <see cref="LogicalOpCodeInfoForBranchingControlFlow"/>.
      /// </summary>
      public ILLabel TargetLabel
      {
         get
         {
            return this._targetLabel;
         }
      }
   }

   /// <summary>
   /// This class will emit a branching instruction.
   /// </summary>
   public sealed class LogicalOpCodeInfoForBranch : LogicalOpCodeInfoForBranchingControlFlow
   {
      private static readonly IDictionary<BranchType, OpCode> LONG_BRANCH_OPCODES;
      private static readonly IDictionary<BranchType, OpCode> SHORT_BRANCH_OPCODES;
      private static readonly IDictionary<OpCode, BranchType> LONG_BRANCH_TYPES;
      private static readonly IDictionary<OpCode, BranchType> SHORT_BRANCH_TYPES;

      static LogicalOpCodeInfoForBranch()
      {
         LONG_BRANCH_OPCODES = new Dictionary<BranchType, OpCode>();
         LONG_BRANCH_OPCODES.Add( BranchType.ALWAYS, OpCodes.Br );
         LONG_BRANCH_OPCODES.Add( BranchType.IF_BOTH_EQUAL, OpCodes.Beq );
         LONG_BRANCH_OPCODES.Add( BranchType.IF_FALSE, OpCodes.Brfalse );
         LONG_BRANCH_OPCODES.Add( BranchType.IF_FIRST_GREATER_THAN_OR_EQUAL_TO_SECOND, OpCodes.Bge );
         LONG_BRANCH_OPCODES.Add( BranchType.IF_FIRST_GREATER_THAN_OR_EQUAL_TO_SECOND_UNORDERED, OpCodes.Bge_Un );
         LONG_BRANCH_OPCODES.Add( BranchType.IF_FIRST_GREATER_THAN_SECOND, OpCodes.Bgt );
         LONG_BRANCH_OPCODES.Add( BranchType.IF_FIRST_GREATER_THAN_SECOND_UNORDERED, OpCodes.Bgt_Un );
         LONG_BRANCH_OPCODES.Add( BranchType.IF_FIRST_LESSER_THAN_OR_EQUAL_TO_SECOND, OpCodes.Ble );
         LONG_BRANCH_OPCODES.Add( BranchType.IF_FIRST_LESSER_THAN_OR_EQUAL_TO_SECOND_UNORDERED, OpCodes.Ble_Un );
         LONG_BRANCH_OPCODES.Add( BranchType.IF_FIRST_LESSER_THAN_SECOND, OpCodes.Blt );
         LONG_BRANCH_OPCODES.Add( BranchType.IF_FIRST_LESSER_THAN_SECOND_UNORDERED, OpCodes.Blt_Un );
         LONG_BRANCH_OPCODES.Add( BranchType.IF_NOT_EQUAL_UNORDERED, OpCodes.Bne_Un );
         LONG_BRANCH_OPCODES.Add( BranchType.IF_TRUE, OpCodes.Brtrue );

         SHORT_BRANCH_OPCODES = new Dictionary<BranchType, OpCode>();
         SHORT_BRANCH_OPCODES.Add( BranchType.ALWAYS, OpCodes.Br_S );
         SHORT_BRANCH_OPCODES.Add( BranchType.IF_BOTH_EQUAL, OpCodes.Beq_S );
         SHORT_BRANCH_OPCODES.Add( BranchType.IF_FALSE, OpCodes.Brfalse_S );
         SHORT_BRANCH_OPCODES.Add( BranchType.IF_FIRST_GREATER_THAN_OR_EQUAL_TO_SECOND, OpCodes.Bge_S );
         SHORT_BRANCH_OPCODES.Add( BranchType.IF_FIRST_GREATER_THAN_OR_EQUAL_TO_SECOND_UNORDERED, OpCodes.Bge_Un_S );
         SHORT_BRANCH_OPCODES.Add( BranchType.IF_FIRST_GREATER_THAN_SECOND, OpCodes.Bgt_S );
         SHORT_BRANCH_OPCODES.Add( BranchType.IF_FIRST_GREATER_THAN_SECOND_UNORDERED, OpCodes.Bgt_Un_S );
         SHORT_BRANCH_OPCODES.Add( BranchType.IF_FIRST_LESSER_THAN_OR_EQUAL_TO_SECOND, OpCodes.Ble_S );
         SHORT_BRANCH_OPCODES.Add( BranchType.IF_FIRST_LESSER_THAN_OR_EQUAL_TO_SECOND_UNORDERED, OpCodes.Ble_Un_S );
         SHORT_BRANCH_OPCODES.Add( BranchType.IF_FIRST_LESSER_THAN_SECOND, OpCodes.Blt_S );
         SHORT_BRANCH_OPCODES.Add( BranchType.IF_FIRST_LESSER_THAN_SECOND_UNORDERED, OpCodes.Blt_Un_S );
         SHORT_BRANCH_OPCODES.Add( BranchType.IF_NOT_EQUAL_UNORDERED, OpCodes.Bne_Un_S );
         SHORT_BRANCH_OPCODES.Add( BranchType.IF_TRUE, OpCodes.Brtrue_S );

         LONG_BRANCH_TYPES = new Dictionary<OpCode, BranchType>();
         LONG_BRANCH_TYPES.Add( LONG_BRANCH_OPCODES[BranchType.ALWAYS], BranchType.ALWAYS );
         LONG_BRANCH_TYPES.Add( LONG_BRANCH_OPCODES[BranchType.IF_BOTH_EQUAL], BranchType.IF_BOTH_EQUAL );
         LONG_BRANCH_TYPES.Add( LONG_BRANCH_OPCODES[BranchType.IF_FALSE], BranchType.IF_FALSE );
         LONG_BRANCH_TYPES.Add( LONG_BRANCH_OPCODES[BranchType.IF_FIRST_GREATER_THAN_OR_EQUAL_TO_SECOND], BranchType.IF_FIRST_GREATER_THAN_OR_EQUAL_TO_SECOND );
         LONG_BRANCH_TYPES.Add( LONG_BRANCH_OPCODES[BranchType.IF_FIRST_GREATER_THAN_OR_EQUAL_TO_SECOND_UNORDERED], BranchType.IF_FIRST_GREATER_THAN_OR_EQUAL_TO_SECOND_UNORDERED );
         LONG_BRANCH_TYPES.Add( LONG_BRANCH_OPCODES[BranchType.IF_FIRST_GREATER_THAN_SECOND], BranchType.IF_FIRST_GREATER_THAN_SECOND );
         LONG_BRANCH_TYPES.Add( LONG_BRANCH_OPCODES[BranchType.IF_FIRST_GREATER_THAN_SECOND_UNORDERED], BranchType.IF_FIRST_GREATER_THAN_SECOND_UNORDERED );
         LONG_BRANCH_TYPES.Add( LONG_BRANCH_OPCODES[BranchType.IF_FIRST_LESSER_THAN_OR_EQUAL_TO_SECOND], BranchType.IF_FIRST_LESSER_THAN_OR_EQUAL_TO_SECOND );
         LONG_BRANCH_TYPES.Add( LONG_BRANCH_OPCODES[BranchType.IF_FIRST_LESSER_THAN_OR_EQUAL_TO_SECOND_UNORDERED], BranchType.IF_FIRST_LESSER_THAN_OR_EQUAL_TO_SECOND_UNORDERED );
         LONG_BRANCH_TYPES.Add( LONG_BRANCH_OPCODES[BranchType.IF_FIRST_LESSER_THAN_SECOND], BranchType.IF_FIRST_LESSER_THAN_SECOND );
         LONG_BRANCH_TYPES.Add( LONG_BRANCH_OPCODES[BranchType.IF_FIRST_LESSER_THAN_SECOND_UNORDERED], BranchType.IF_FIRST_LESSER_THAN_SECOND_UNORDERED );
         LONG_BRANCH_TYPES.Add( LONG_BRANCH_OPCODES[BranchType.IF_NOT_EQUAL_UNORDERED], BranchType.IF_NOT_EQUAL_UNORDERED );
         LONG_BRANCH_TYPES.Add( LONG_BRANCH_OPCODES[BranchType.IF_TRUE], BranchType.IF_TRUE );

         SHORT_BRANCH_TYPES = new Dictionary<OpCode, BranchType>();
         SHORT_BRANCH_TYPES.Add( SHORT_BRANCH_OPCODES[BranchType.ALWAYS], BranchType.ALWAYS );
         SHORT_BRANCH_TYPES.Add( SHORT_BRANCH_OPCODES[BranchType.IF_BOTH_EQUAL], BranchType.IF_BOTH_EQUAL );
         SHORT_BRANCH_TYPES.Add( SHORT_BRANCH_OPCODES[BranchType.IF_FALSE], BranchType.IF_FALSE );
         SHORT_BRANCH_TYPES.Add( SHORT_BRANCH_OPCODES[BranchType.IF_FIRST_GREATER_THAN_OR_EQUAL_TO_SECOND], BranchType.IF_FIRST_GREATER_THAN_OR_EQUAL_TO_SECOND );
         SHORT_BRANCH_TYPES.Add( SHORT_BRANCH_OPCODES[BranchType.IF_FIRST_GREATER_THAN_OR_EQUAL_TO_SECOND_UNORDERED], BranchType.IF_FIRST_GREATER_THAN_OR_EQUAL_TO_SECOND_UNORDERED );
         SHORT_BRANCH_TYPES.Add( SHORT_BRANCH_OPCODES[BranchType.IF_FIRST_GREATER_THAN_SECOND], BranchType.IF_FIRST_GREATER_THAN_SECOND );
         SHORT_BRANCH_TYPES.Add( SHORT_BRANCH_OPCODES[BranchType.IF_FIRST_GREATER_THAN_SECOND_UNORDERED], BranchType.IF_FIRST_GREATER_THAN_SECOND_UNORDERED );
         SHORT_BRANCH_TYPES.Add( SHORT_BRANCH_OPCODES[BranchType.IF_FIRST_LESSER_THAN_OR_EQUAL_TO_SECOND], BranchType.IF_FIRST_LESSER_THAN_OR_EQUAL_TO_SECOND );
         SHORT_BRANCH_TYPES.Add( SHORT_BRANCH_OPCODES[BranchType.IF_FIRST_LESSER_THAN_OR_EQUAL_TO_SECOND_UNORDERED], BranchType.IF_FIRST_LESSER_THAN_OR_EQUAL_TO_SECOND_UNORDERED );
         SHORT_BRANCH_TYPES.Add( SHORT_BRANCH_OPCODES[BranchType.IF_FIRST_LESSER_THAN_SECOND], BranchType.IF_FIRST_LESSER_THAN_SECOND );
         SHORT_BRANCH_TYPES.Add( SHORT_BRANCH_OPCODES[BranchType.IF_FIRST_LESSER_THAN_SECOND_UNORDERED], BranchType.IF_FIRST_LESSER_THAN_SECOND_UNORDERED );
         SHORT_BRANCH_TYPES.Add( SHORT_BRANCH_OPCODES[BranchType.IF_NOT_EQUAL_UNORDERED], BranchType.IF_NOT_EQUAL_UNORDERED );
         SHORT_BRANCH_TYPES.Add( SHORT_BRANCH_OPCODES[BranchType.IF_TRUE], BranchType.IF_TRUE );
      }

      private readonly BranchType _bType;

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoForBranch"/>, which will try its best to emit short-form version of the branch instruction.
      /// </summary>
      /// <param name="bType">The <see cref="BranchType"/>.</param>
      /// <param name="targetLabel">The <see cref="ILLabel"/> where to branch.</param>
      public LogicalOpCodeInfoForBranch( BranchType bType, ILLabel targetLabel )
         : base(/* SHORT_BRANCH_OPCODES[bType].Size + SHORT_BRANCH_OPERAND_SIZE, LONG_BRANCH_OPCODES[bType].Size + LONG_BRANCH_OPERAND_SIZE,*/ targetLabel )
      {
         this._bType = bType;
      }

      internal LogicalOpCodeInfoForBranch( OpCode code, Boolean isShort, ILLabel targetLabel )
         : this( isShort ? SHORT_BRANCH_TYPES[code] : LONG_BRANCH_TYPES[code], targetLabel )
      {
      }

      /// <inheritdoc />
      public override OpCode ShortForm
      {
         get
         {
            return SHORT_BRANCH_OPCODES[this._bType];
         }
      }

      /// <inheritdoc />
      public override OpCode LongForm
      {
         get
         {
            return LONG_BRANCH_OPCODES[this._bType];
         }
      }

      /// <inheritdoc/>
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.Branch;
         }
      }

      /// <summary>
      /// Gets the branch instruction kind.
      /// </summary>
      /// <seealso cref="BranchType"/>.
      public BranchType BranchKind
      {
         get
         {
            return this._bType;
         }
      }
   }

   /// <summary>
   /// This class will emit a <see cref="OpCodes.Switch"/> instruction.
   /// </summary>
   public sealed class LogicalOpCodeInfoForSwitch : LogicalDynamicOpCodeInfo
   {
      private readonly ILLabel[] _labels;

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoForSwitch"/> with specified labels to use as branching table.
      /// </summary>
      /// <param name="labels">The <see cref="ILLabel"/> to use as branching table.</param>
      public LogicalOpCodeInfoForSwitch( ILLabel[] labels )
      //: base( OpCodes.Switch.Size + ( sizeof( Int32 ) * ( labels.Length + 1 ) ) )
      {
         this._labels = labels;
      }

      //internal override void EmitOpCode( MethodILWriter emittingContext )
      //{
      //   emittingContext.Emit( OpCodes.Switch, this._labels );
      //}

      //internal override Int32 BranchTargetCount
      //{
      //   get
      //   {
      //      return this._labels.Length;
      //   }
      //}

      /// <summary>
      /// Gets the labels used for this <see cref="OpCodes.Switch"/> instruction.
      /// </summary>
      /// <value>The labels used for this <see cref="OpCodes.Switch"/> instruction.</value>
      public IEnumerable<ILLabel> Labels
      {
         get
         {
            return this._labels.Skip( 0 );
         }
      }

      /// <inheritdoc/>
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.Switch;
         }
      }
   }

   /// <summary>
   /// This class will emit <see cref="OpCodes.Leave"/> or <see cref="OpCodes.Leave_S"/> instruction.
   /// </summary>
   public sealed class LogicalOpCodeInfoForLeave : LogicalOpCodeInfoForBranchingControlFlow
   {

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoForLeave"/> with specified label.
      /// </summary>
      /// <param name="label">The label marking end of handler block.</param>
      public LogicalOpCodeInfoForLeave( ILLabel label )
         : base(/* OpCodes.Leave_S.Size + SHORT_BRANCH_OPERAND_SIZE, OpCodes.Leave.Size + LONG_BRANCH_OPERAND_SIZE,*/ label )
      {
      }

      /// <inheritdoc />
      public override OpCode ShortForm
      {
         get
         {
            return OpCodes.Leave_S;
         }
      }

      /// <inheritdoc />
      public override OpCode LongForm
      {
         get
         {
            return OpCodes.Leave;
         }
      }

      /// <inheritdoc/>
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.Leave;
         }
      }
   }

   /// <summary>
   /// This class provides a way to emit a branching or leave instruction which has a fixed <see cref="OpCode"/>.
   /// </summary>
   public sealed class LogicalOpCodeInfoForFixedBranchOrLeave : LogicalDynamicOpCodeInfo
   {
      private readonly OpCode _code;
      private readonly ILLabel _label;

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoForFixedBranchOrLeave"/> with specified <see cref="OpCode"/> and <see cref="ILLabel"/>.
      /// </summary>
      /// <param name="code">The <see cref="OpCode"/>.</param>
      /// <param name="label">The <see cref="ILLabel"/> to branch to.</param>
      public LogicalOpCodeInfoForFixedBranchOrLeave( OpCode code, ILLabel label )
      //: base( code.Size + ( code.OperandType == OperandType.ShortInlineBrTarget ? SHORT_BRANCH_OPERAND_SIZE : LONG_BRANCH_OPERAND_SIZE ) )
      {
         this._code = code;
         this._label = label;
      }

      //internal override void EmitOpCode( MethodILWriter emittingContext )
      //{
      //   emittingContext.Emit( this._code, this._label );
      //}

      //internal override Int32 BranchTargetCount
      //{
      //   get
      //   {
      //      return 1;
      //   }
      //}

      /// <summary>
      /// Gets the fixed branch code for this <see cref="LogicalOpCodeInfoForFixedBranchOrLeave"/>.
      /// </summary>
      /// <value>The fixed branch code for this <see cref="LogicalOpCodeInfoForFixedBranchOrLeave"/>.</value>
      public OpCode Code
      {
         get
         {
            return this._code;
         }
      }

      /// <summary>
      /// Gets the target label for this branching instruction.
      /// </summary>
      /// <value>The target label for this branching instruction.</value>
      public ILLabel TargetLabel
      {
         get
         {
            return this._label;
         }
      }

      /// <inheritdoc/>
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.BranchOrLeaveFixed;
         }
      }
   }
}