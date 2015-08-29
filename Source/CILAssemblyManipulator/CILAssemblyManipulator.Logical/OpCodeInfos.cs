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
      private readonly UInt16 _opCode; // Save some memory and store enum instead of actual struct

      internal LogicalOpCodeInfoWithOneOpCode( OpCode code )
      {
         this._opCode = (UInt16) code.Value;
      }

      /// <summary>
      /// Returns the <see cref="OpCode"/> to emit.
      /// </summary>
      /// <value>The <see cref="OpCode"/> to emit.</value>
      public OpCode Code
      {
         get
         {
            return OpCodes.GetCodeFor( (OpCodeEncoding) this._opCode );
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

      /// <inheritdoc />
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandNone;
         }
      }

      /// <summary>
      /// Gets the <see cref="LogicalOpCodeInfoWithNoOperand"/> instance for given <see cref="OpCodeEncoding"/>.
      /// </summary>
      /// <param name="code">The <see cref="OpCodeEncoding"/> representing opcode.</param>
      /// <returns><see cref="LogicalOpCodeInfoWithNoOperand"/> instance for given <paramref name="code"/>.</returns>
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

      /// <summary>
      /// Gets the <see cref="LogicalOpCodeInfoWithNoOperand"/> instance for given <see cref="OpCode"/>.
      /// </summary>
      /// <param name="code">The <see cref="OpCode"/>.</param>
      /// <returns><see cref="LogicalOpCodeInfoWithNoOperand"/> instance for given <paramref name="code"/>.</returns>
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
      internal LogicalOpCodeInfoWithFixedSizeOperand( OpCode opCode )
         : base( opCode )
      {
      }
   }

   /// <summary>
   /// This is abstract class with operand type as generic type parameter for all <see cref="LogicalOpCodeInfo"/>s with fixed-size operand.
   /// </summary>
   /// <typeparam name="TOperand">The type of the operand.</typeparam>
   public abstract class LogicalOpCodeInfoWithFixedSizeOperand<TOperand> : LogicalOpCodeInfoWithFixedSizeOperand
   {
      private readonly TOperand _operand;

      internal LogicalOpCodeInfoWithFixedSizeOperand( OpCode opCode, TOperand operand )
         : base( opCode )
      {
         this._operand = operand;
      }

      /// <summary>
      /// Gets the operand of this <see cref="LogicalOpCodeInfoWithFixedSizeOperand"/>.
      /// </summary>
      public TOperand Operand
      {
         get
         {
            return this._operand;
         }
      }
   }

   /// <summary>
   /// This is abstract class for all <see cref="LogicalOpCodeInfo"/>s which accept a token as operand.
   /// </summary>
   public abstract class LogicalOpCodeInfoWithTokenOperand : LogicalOpCodeInfoWithFixedSizeOperand
   {
      internal LogicalOpCodeInfoWithTokenOperand( OpCode opCode )
         : base( opCode )
      {
      }
   }

   /// <summary>
   /// This enum controls what table the tokens of <see cref="LogicalOpCodeInfoWithTokenOperandAndTypeTokenKind"/> will use.
   /// </summary>
   /// <remarks>
   /// <para>
   /// This only has effect on fields with generic declaring type, or generic types.
   /// Additionally, in case of fields, only fields declared in the same type as this method are affected.
   /// Similarly, in case of types, only types that is the same as the type containing current method is affected.
   /// </para>
   /// <para>
   /// Consider the following code:
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CILManipulatorCodeContent.cs" region="EmitTypeDefOrTypeSpec" language="C#" />
   /// In order to emit code which would produce the sample above, using this enum is required.
   /// If this property is <see cref="TypeTokenKind.GenericInstantiation"/>, then emitted code will load token suitable to <c>type2</c>, that is, a TypeSpec token for generic instantiation.
   /// Consequentially, if enum value is <see cref="TypeTokenKind.GenericDefinition"/>, then emitted code will load token suitable to <c>type1</c>, that is, a TypeDef token for generic definition rather than generic instantiation.
   /// </para>
   /// </remarks>
   public enum TypeTokenKind
   {
      /// <summary>
      /// This is the default behaviour and is always effective.
      /// <list type="table">
      /// <listheader>
      /// <term>Reflection element</term>
      /// <term>Behaviour</term>
      /// </listheader>
      /// <item>
      /// <term>
      /// <see cref="CILField"/>
      /// </term>
      /// <term>
      /// The <see cref="Tables.Field"/> will be used for fields with non-generic declaring type defined in current module.
      /// The <see cref="Tables.MemberRef"/> will be used for fields with generic declaring type (regardless where the type is defined) and for fields declared in other modules.
      /// </term>
      /// </item>
      /// <item>
      /// <term>
      /// <see cref="CILTypeBase"/>
      /// </term>
      /// <term>
      /// The <see cref="Tables.TypeDef"/> will be used for non-generic types defined in current module.
      /// The <see cref="Tables.TypeRef"/> will be used for non-generic types defined in other modules.
      /// The <see cref="Tables.TypeSpec"/> will be used for generic types regardless where they are defined.
      /// </term>
      /// </item>
      /// </list>
      /// </summary>
      GenericInstantiation,

      /// <summary>
      /// This is non-default behaviour, and is only effective when 
      /// <list type="table">
      /// <listheader>
      /// <term>Reflection element</term>
      /// <term>Behaviour</term>
      /// </listheader>
      /// <item>
      /// <term>
      /// <see cref="CILField"/>
      /// </term>
      /// <term>
      /// The <see cref="Tables.Field"/> will be used for any field defined in current module.
      /// The <see cref="Tables.MemberRef"/> will be used for any field defined in other modules.
      /// </term>
      /// </item>
      /// <item>
      /// <term>
      /// <see cref="CILTypeBase"/>
      /// </term>
      /// <term>
      /// The <see cref="Tables.TypeDef"/> will be used for any type defined in current module.
      /// The <see cref="Tables.TypeRef"/> will be used for any type defined in other modules.
      /// </term>
      /// </item>
      /// </list>
      /// </summary>
      GenericDefinition,
   }

   /// <summary>
   /// This is abstract class for all <see cref="LogicalOpCodeInfoWithTokenOperand"/>s which can have two metadata table options for a single CAM.Logical reflection element.
   /// </summary>
   public abstract class LogicalOpCodeInfoWithTokenOperandAndTypeTokenKind : LogicalOpCodeInfoWithTokenOperand
   {
      private readonly TypeTokenKind _typeTokenKind;

      internal LogicalOpCodeInfoWithTokenOperandAndTypeTokenKind( OpCode opCode, TypeTokenKind typeTokenKind )
         : base( opCode ) //, TOKEN_SIZE )
      {
         this._typeTokenKind = typeTokenKind;
      }

      /// <summary>
      /// This setting provides a way to distinguish the emitting TypeDef or TypeSpec token within signatures or tokens.
      /// See <see cref="TypeTokenKind"/> for more information.
      /// </summary>
      public TypeTokenKind TypeTokenKind
      {
         get
         {
            return this._typeTokenKind;
         }
      }
   }

   /// <summary>
   /// This is class which will emit <see cref="OpCode"/> with type token as operand.
   /// </summary>
   public sealed class LogicalOpCodeInfoWithTypeToken : LogicalOpCodeInfoWithTokenOperandAndTypeTokenKind
   {
      private readonly CILTypeBase _type;

      /// <summary>
      /// Creates new instance of <see cref="LogicalOpCodeInfoWithTypeToken"/> with specified values.
      /// </summary>
      /// <param name="opCode">The <see cref="OpCode"/> to emit.</param>
      /// <param name="type">The <see cref="CILTypeBase"/> to have as operand.</param>
      /// <param name="typeTokenKind"> The <see cref="TypeTokenKind"/></param>
      /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>.</exception>
      public LogicalOpCodeInfoWithTypeToken( OpCode opCode, CILTypeBase type, TypeTokenKind typeTokenKind = TypeTokenKind.GenericInstantiation )
         : base( opCode, typeTokenKind )
      {
         ArgumentValidator.ValidateNotNull( "Type", type );
         this._type = type;
      }

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
   public sealed class LogicalOpCodeInfoWithFieldToken : LogicalOpCodeInfoWithTokenOperandAndTypeTokenKind
   {
      private readonly CILField _field;

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoWithFieldToken"/> with specified values.
      /// </summary>
      /// <param name="opCode">The <see cref="OpCode"/> to emit.</param>
      /// <param name="field">The <see cref="CILField"/> to use as operand.</param>
      /// <param name="typeTokenKind">The <see cref="TypeTokenKind"/>.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="field"/> is <c>null</c>.</exception>
      public LogicalOpCodeInfoWithFieldToken( OpCode opCode, CILField field, TypeTokenKind typeTokenKind = TypeTokenKind.GenericInstantiation )
         : base( opCode, typeTokenKind )
      {
         ArgumentValidator.ValidateNotNull( "Field", field );
         this._field = field;
      }

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
   /// This enum controls what table the tokens of <see cref="LogicalOpCodeInfoWithMethodBaseToken"/> will use.
   /// </summary>
   /// <remarks>
   /// Together with <see cref="TypeTokenKind"/>, this enum controls how one will use the reflection object of the method that is the owner of the IL being created.
   /// Considering one has type <c>MyType&lt;T&gt;</c> with method <c>MyMethod&lt;U&gt;</c> and one is emitting IL for <c>MyMethod&lt;U&gt;</c>, there are four possibilities.
   /// <list type="bullet">
   /// 
   /// <item>
   /// <description>
   /// In order to emit token referring to generic instantiation of <c>MyMethod&lt;U&gt;</c> having U replaced by the current generic argument, with declaring type being generic instantiation of <c>MyType&lt;T&gt;</c> (<c>typeof(MyType&lt;T&gt;)</c>) in e.g. recursion scenario, a combination of <see cref="TypeTokenKind.GenericInstantiation"/> and <see cref="MethodTokenKind.GenericInstantiation"/> should be used.
   /// </description>
   /// </item>
   /// 
   /// <item>
   /// <description>
   /// In order to emit token referring to generic method definition of <c>MyMethod&lt;U&gt;</c>, but with declaring type still being generic instantiation of <c>MyType&lt;T&gt;</c> (<c>typeof(MyType&lt;T&gt;)</c>), a combination of <see cref="TypeTokenKind.GenericInstantiation"/> and <see cref="MethodTokenKind.GenericDefinition"/> should be used.
   /// </description>
   /// </item>
   /// 
   /// <item>
   /// <description>
   /// In order to emit token referring to generic method instantiation of <c>MyMethod&lt;U&gt;</c>, but with declaring type being generic type definition of <c>MyType&lt;T&gt;</c> (<c>typeof(MyType&lt;&gt;)</c>), a combination of <see cref="TypeTokenKind.GenericDefinition"/> and <see cref="MethodTokenKind.GenericInstantiation"/> should be used.
   /// </description>
   /// </item>
   /// 
   /// <item>
   /// <description>
   /// In order to emit token referring to generic method definition of <c>MyMethod&lt;U&gt;</c> and generic type definition of <c>MyType&lt;T&gt;</c> (<c>typeof(MyType&lt;&gt;)</c>), a combination of <see cref="TypeTokenKind.GenericDefinition"/> and <see cref="MethodTokenKind.GenericDefinition"/> should be used.
   /// </description>
   /// </item>
   /// 
   /// </list>
   /// </remarks>
   public enum MethodTokenKind
   {
      /// <summary>
      /// The <see cref="Tables.MethodSpec"/> table will be used for generic methods, regardless where they are defined.
      /// The <see cref="Tables.MemberRef"/> table will be used for non-generic methods with generic declaring type (regardless where the type is defined) or with declaring type in another module.
      /// The <see cref="Tables.MethodDef"/> table will be used for non-generic methods with non-generic declaring type defined in current module.
      /// </summary>
      GenericInstantiation,

      /// <summary>
      /// <list type="table">
      /// 
      /// <listheader>
      /// <term>
      /// <see cref="TypeTokenKind"/>
      /// </term>
      /// <term>
      /// Behaviour
      /// </term>
      /// </listheader>
      /// 
      /// <item>
      /// <term>
      /// <see cref="TypeTokenKind.GenericInstantiation"/>
      /// </term>
      /// <term>
      /// The <see cref="Tables.MemberRef"/> table will be used for all methods with generic declaring type (regardless where the type is defined) or with declaring type in another module.
      /// The <see cref="Tables.MethodDef"/> table will be used for non-generic methods with non-generic declaring type defined in current module.
      /// </term>
      /// </item>
      /// <item>
      /// 
      /// <term>
      /// <see cref="TypeTokenKind.GenericDefinition"/>
      /// </term>
      /// <term>
      /// The <see cref="Tables.MemberRef"/> table will be used for all methods with declaring type defined in other modules.
      /// The <see cref="Tables.MethodDef"/> table will be used for all methods with declaring type defined in current module.
      /// </term>
      /// </item>
      /// 
      /// </list>
      /// </summary>
      /// <remarks>
      /// Using this option will never cause <see cref="Tables.MethodSpec"/> token to be emitted.
      /// </remarks>
      GenericDefinition,
   }

   /// <summary>
   /// This is base class for all <see cref="LogicalOpCodeInfoWithTokenOperand"/>s which accept <see cref="CILMethodBase"/> as reflection elements.
   /// </summary>
   public abstract class LogicalOpCodeInfoWithMethodBaseToken : LogicalOpCodeInfoWithTokenOperandAndTypeTokenKind
   {
      private readonly MethodTokenKind _tokenKind;

      internal LogicalOpCodeInfoWithMethodBaseToken( OpCode opCode, TypeTokenKind typeTokenKind, MethodTokenKind methodTokenKind )
         : base( opCode, typeTokenKind )
      {
         this._tokenKind = methodTokenKind;
      }

      /// <summary>
      /// Gets the <see cref="Logical.MethodTokenKind"/> which controls what kind of <see cref="Tables">table</see> the token will reference.
      /// </summary>
      /// <value>The <see cref="Logical.MethodTokenKind"/> which controls what kind of <see cref="Tables">table</see> the token will reference.</value>
      public MethodTokenKind MethodTokenKind
      {
         get
         {
            return this._tokenKind;
         }
      }
   }

   /// <summary>
   /// This is class which will emit <see cref="OpCode"/> with <see cref="CILMethod"/> token as operand.
   /// </summary>
   public sealed class LogicalOpCodeInfoWithMethodToken : LogicalOpCodeInfoWithMethodBaseToken
   {
      private readonly CILMethod _method;

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoWithMethodToken"/> with specified values.
      /// </summary>
      /// <param name="opCode">The <see cref="OpCode"/> to emit.</param>
      /// <param name="method">The <see cref="CILMethod"/> to use as operand.</param>
      /// <param name="typeTokenKind">The <see cref="Logical.TypeTokenKind"/>.</param>
      /// <param name="methodTokenKind"> The <see cref="Logical.MethodTokenKind"/>.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="method"/> is <c>null</c>.</exception>
      /// <remarks>TODO varargs parameters</remarks>
      public LogicalOpCodeInfoWithMethodToken( OpCode opCode, CILMethod method, TypeTokenKind typeTokenKind = TypeTokenKind.GenericInstantiation, MethodTokenKind methodTokenKind = MethodTokenKind.GenericInstantiation )
         : base( opCode, typeTokenKind, methodTokenKind )
      {
         ArgumentValidator.ValidateNotNull( "Method", method );
         this._method = method;
      }

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
   /// This is class which will emit <see cref="OpCode"/> with <see cref="CILConstructor"/> token as operand.
   /// </summary>
   public sealed class LogicalOpCodeInfoWithCtorToken : LogicalOpCodeInfoWithTokenOperandAndTypeTokenKind
   {
      private readonly CILConstructor _ctor;

      /// <summary>
      /// Creats a new instance of <see cref="LogicalOpCodeInfoWithCtorToken"/> with specified values.
      /// </summary>
      /// <param name="opCode">The <see cref="OpCode"/> to emit.</param>
      /// <param name="ctor">The <see cref="CILConstructor"/> to use as operand.</param>
      /// <param name="typeTokenKind">The <see cref="Logical.TypeTokenKind"/>.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="ctor"/> is <c>null</c>.</exception>
      /// <remarks>TODO varargs parameters.</remarks>
      public LogicalOpCodeInfoWithCtorToken( OpCode opCode, CILConstructor ctor, TypeTokenKind typeTokenKind = TypeTokenKind.GenericInstantiation )
         : base( opCode, typeTokenKind )
      {
         ArgumentValidator.ValidateNotNull( "Constructor", ctor );
         this._ctor = ctor;
      }

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
      private readonly VarArgInstance[] _varArgs;

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoWithMethodSig"/> with specified values.
      /// </summary>
      /// <param name="methodSig">The <see cref="CILMethodSignature"/>.</param>
      /// <param name="varArgs">The variable arguments for this call site. May be <c>null</c> or empty for non-varargs call.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="methodSig"/> is <c>null</c>.</exception>
      public LogicalOpCodeInfoWithMethodSig( CILMethodSignature methodSig, VarArgInstance[] varArgs )
         : base( OpCodes.Calli )
      {
         ArgumentValidator.ValidateNotNull( "Method signature", methodSig );

         this._methodSig = methodSig;
         this._varArgs = varArgs;
      }

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
      public VarArgInstance[] VarArgs
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
   public sealed class LogicalOpCodeInfoWithFixedSizeOperandString : LogicalOpCodeInfoWithFixedSizeOperand<String>
   {
      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoWithFixedSizeOperandString"/>
      /// </summary>
      /// <param name="code">The code to use.</param>
      /// <param name="str">The string to use.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="str"/> is <c>null</c>.</exception>
      /// <exception cref="ArgumentException">If <see cref="OpCode.OperandType"/> for <paramref name="code"/> is not <see cref="OperandType.InlineString"/>.</exception>
      public LogicalOpCodeInfoWithFixedSizeOperandString( OpCode code, String str )
         : base( code, str ) //, TOKEN_SIZE )
      {
         if ( code.OperandType != OperandType.InlineString )
         {
            throw new ArgumentException( "The operand type of opcode is " + code.OperandType + " instead of string." );
         }
         ArgumentValidator.ValidateNotNull( "String", str );
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
   public sealed class LogicalOpCodeInfoWithFixedSizeOperandUInt16 : LogicalOpCodeInfoWithFixedSizeOperand<Int16>
   {
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
         : base( opCode, int16 ) //, opCode.OperandType == OperandType.ShortInlineVar ? (Byte) 1 : (Byte) 2 )
      {
      }

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
   public sealed class LogicalOpCodeInfoWithFixedSizeOperandInt32 : LogicalOpCodeInfoWithFixedSizeOperand<Int32>
   {

      /// <summary>
      /// Creates a new instane of <see cref="LogicalOpCodeInfoWithFixedSizeOperandInt32"/> with specified <see cref="OpCode"/> and operand.
      /// </summary>
      /// <param name="opCode">The <see cref="OpCode"/> to emit.</param>
      /// <param name="int32">The operand.</param>
      /// <remarks>
      /// If <see cref="OpCode.OperandType"/> property of <paramref name="opCode"/> is <see cref="OperandType.ShortInlineI"/>, the <paramref name="int32"/> will be emitted as <see cref="SByte"/>.
      /// </remarks>
      public LogicalOpCodeInfoWithFixedSizeOperandInt32( OpCode opCode, Int32 int32 )
         : base( opCode, int32 ) //, opCode.OperandType == OperandType.ShortInlineI ? (Byte) 1 : (Byte) 4 )
      {
      }

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
   public sealed class LogicalOpCodeInfoWithFixedSizeOperandInt64 : LogicalOpCodeInfoWithFixedSizeOperand<Int64>
   {

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoWithFixedSizeOperandInt64"/> with specified <see cref="OpCode"/> and operand.
      /// </summary>
      /// <param name="opCode">The <see cref="OpCode"/> to emit.</param>
      /// <param name="int64">The operand.</param>
      public LogicalOpCodeInfoWithFixedSizeOperandInt64( OpCode opCode, Int64 int64 )
         : base( opCode, int64 ) //, (Byte) 8 )
      {
      }

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
   public sealed class LogicalOpCodeInfoWithFixedSizeOperandSingle : LogicalOpCodeInfoWithFixedSizeOperand<Single>
   {

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoWithFixedSizeOperandSingle"/> with specified <see cref="OpCode"/> and operand.
      /// </summary>
      /// <param name="opCode">The <see cref="OpCode"/> to emit.</param>
      /// <param name="single">The operand.</param>
      public LogicalOpCodeInfoWithFixedSizeOperandSingle( OpCode opCode, Single single )
         : base( opCode, single ) //, (Byte) 4 )
      {
      }

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
   public sealed class LogicalOpCodeInfoWithFixedSizeOperandDouble : LogicalOpCodeInfoWithFixedSizeOperand<Double>
   {

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoWithFixedSizeOperandDouble"/> with specified <see cref="OpCode"/> and operand.
      /// </summary>
      /// <param name="opCode">The <see cref="OpCode"/> to emit.</param>
      /// <param name="dbl">The operand.</param>
      public LogicalOpCodeInfoWithFixedSizeOperandDouble( OpCode opCode, Double dbl )
         : base( opCode, dbl ) // , (Byte) 8 )
      {
      }

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
   /// This is abstract class for <see cref="LogicalOpCodeInfo"/>s emitting branch or leave instruction.
   /// </summary>
   public abstract class LogicalOpCodeInfoForBranchingControlFlow : LogicalOpCodeInfo
   {
      private readonly ILLabel _targetLabel;

      internal LogicalOpCodeInfoForBranchingControlFlow( ILLabel targetLabel )
      {
         this._targetLabel = targetLabel;
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
   public sealed class LogicalOpCodeInfoForSwitch : LogicalOpCodeInfo
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
   public sealed class LogicalOpCodeInfoForFixedBranchOrLeave : LogicalOpCodeInfo
   {
      private readonly UInt16 _code;
      private readonly ILLabel _label;

      /// <summary>
      /// Creates a new instance of <see cref="LogicalOpCodeInfoForFixedBranchOrLeave"/> with specified <see cref="OpCode"/> and <see cref="ILLabel"/>.
      /// </summary>
      /// <param name="code">The <see cref="OpCode"/>.</param>
      /// <param name="label">The <see cref="ILLabel"/> to branch to.</param>
      public LogicalOpCodeInfoForFixedBranchOrLeave( OpCode code, ILLabel label )
      //: base( code.Size + ( code.OperandType == OperandType.ShortInlineBrTarget ? SHORT_BRANCH_OPERAND_SIZE : LONG_BRANCH_OPERAND_SIZE ) )
      {
         this._code = (UInt16) code.Value;
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
            return OpCodes.GetCodeFor( (OpCodeEncoding) this._code );
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