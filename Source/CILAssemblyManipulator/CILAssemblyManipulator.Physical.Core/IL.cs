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
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.Meta;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
   /// <summary>
   /// This class captures all information related to the IL code of <see cref="MethodDefinition"/>.
   /// </summary>
   public sealed class MethodILDefinition
   {
      /// <summary>
      /// Creates a new instance of <see cref="MethodILDefinition"/>, with optional given capacities for <see cref="ExceptionBlocks"/> and <see cref="OpCodes"/>.
      /// </summary>
      /// <param name="exceptionBlockCount">The optional initial capacity for <see cref="ExceptionBlocks"/>.</param>
      /// <param name="opCodeCount">The optional initial capacity for <see cref="OpCodes"/>.</param>
      public MethodILDefinition( Int32 exceptionBlockCount = 0, Int32 opCodeCount = 0 )
      {
         this.ExceptionBlocks = new List<MethodExceptionBlock>( exceptionBlockCount );
         this.OpCodes = new List<OpCodeInfo>( opCodeCount );
      }

      /// <summary>
      /// Gets or sets whether all locals should be initialzied to their default values on method entry.
      /// </summary>
      /// <value>Whether all locals should be initialzied to their default values on method entry.</value>
      public Boolean InitLocals { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="TableIndex"/> indicating where the <see cref="LocalVariablesSignature"/> for this <see cref="MethodILDefinition"/> is located.
      /// </summary>
      /// <value>The <see cref="TableIndex"/> indicating where the <see cref="LocalVariablesSignature"/> for this <see cref="MethodILDefinition"/> is located.</value>
      public TableIndex? LocalsSignatureIndex { get; set; }

      /// <summary>
      /// Gets or sets the max stack size for this <see cref="MethodILDefinition"/>.
      /// </summary>
      /// <value>The max stack size for this <see cref="MethodILDefinition"/>.</value>
      public Int32 MaxStackSize { get; set; }

      /// <summary>
      /// Gets the list of <see cref="MethodExceptionBlock"/>s of this <see cref="MethodILDefinition"/>.
      /// </summary>
      /// <value>The list of <see cref="MethodExceptionBlock"/>s of this <see cref="MethodILDefinition"/>.</value>
      /// <seealso cref="MethodExceptionBlock"/>
      public List<MethodExceptionBlock> ExceptionBlocks { get; }

      /// <summary>
      /// Gets the list of <see cref="OpCodeInfo"/>s of this <see cref="MethodILDefinition"/>.
      /// </summary>
      /// <value>The list of <see cref="OpCodeInfo"/>s of this <see cref="MethodILDefinition"/>.</value>
      /// <seealso cref="OpCodeInfo"/>
      public List<OpCodeInfo> OpCodes { get; }
   }

   /// <summary>
   /// This class contains all information related to a single exception block of <see cref="MethodILDefinition"/>.
   /// </summary>
   public sealed class MethodExceptionBlock
   {
      /// <summary>
      /// Gets or sets the <see cref="ExceptionBlockType"/> of this <see cref="MethodExceptionBlock"/>.
      /// </summary>
      /// <value>The <see cref="ExceptionBlockType"/> of this <see cref="MethodExceptionBlock"/>.</value>
      /// <remarks>
      /// Depending on value of this property, "handler" (information of which is in <see cref="HandlerOffset"/> and <see cref="HandlerLength"/> properties) has different semantic meaning.
      /// Furthermore, if this property is <see cref="ExceptionBlockType.Exception"/>, then <see cref="ExceptionType"/> should have non-<c>null</c> value.
      /// </remarks>
      public ExceptionBlockType BlockType { get; set; }

      /// <summary>
      /// Gets or sets the offset of try block, in bytes.
      /// </summary>
      /// <value>The offset of try block, in bytes.</value>
      public Int32 TryOffset { get; set; }

      /// <summary>
      /// Gets or sets the length of try block, in bytes.
      /// </summary>
      /// <value>The length of try block, in bytes.</value>
      public Int32 TryLength { get; set; }

      /// <summary>
      /// Gets or sets the offset of handler block, in bytes.
      /// </summary>
      /// <value>The offset of handler block, in bytes.</value>
      public Int32 HandlerOffset { get; set; }

      /// <summary>
      /// Gets or sets the length of handler block, in bytes.
      /// </summary>
      /// <value>The length of handler block, in bytes.</value>
      public Int32 HandlerLength { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="TableIndex"/> indicating the type of exception being catched.
      /// </summary>
      /// <value>
      /// The <see cref="TableIndex"/> indicating the type of exception being catched.
      /// </value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> should be either <see cref="Tables.TypeDef"/>, <see cref="Tables.TypeRef"/> or <see cref="Tables.TypeSpec"/>.
      /// This value has meaning only when <see cref="BlockType"/> is set to <see cref="ExceptionBlockType.Exception"/> or <see cref="ExceptionBlockType.Fault"/>.
      /// </remarks>
      public TableIndex? ExceptionType { get; set; }

      /// <summary>
      /// Gets or sets the filter offset, in bytes.
      /// </summary>
      /// <value></value>
      /// <remarks>
      /// This value has meaning only when <see cref="BlockType"/> is set to <see cref="ExceptionBlockType.Filter"/>.
      /// </remarks>
      public Int32 FilterOffset { get; set; }
   }

   /// <summary>
   /// This interface implemented by <see cref="OpCodeInfo"/> and extended by <see cref="IOpCodeInfoWithOperand{TOperand}"/>.
   /// </summary>
   /// <remarks>
   /// TODO make all classes inheriting this internal and only expose interfaces.
   /// Then make extension methods to properly create op codes via OpCodeProvider!!
   /// </remarks>
   public interface IOpCodeInfo
   {
      /// <summary>
      /// Gets the <see cref="OpCodeInfoKind"/> of this <see cref="IOpCodeInfo"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeInfoKind"/> of this <see cref="IOpCodeInfo"/>.</value>
      OpCodeInfoKind InfoKind { get; }

      /// <summary>
      /// Gets the <see cref="Physical.OpCodeID"/> for this <see cref="IOpCodeInfo"/>.
      /// </summary>
      /// <seealso cref="Physical.OpCodeID"/>
      /// <seealso cref="Physical.OpCode"/>
      /// <seealso cref="OpCodes"/>
      OpCodeID OpCodeID { get; }


      /// <summary>
      /// Gets the additional size of operand of this <see cref="IOpCodeInfo"/>, in bytes.
      /// </summary>
      /// <value>The additional size of operand of this <see cref="IOpCodeInfo"/>, in bytes.</value>
      Int32 DynamicOperandByteSize { get; }

      /// <summary>
      /// Gets the value indicating whether this op code has operand.
      /// </summary>
      /// <value>The value indicating whether this op code has operand.</value>
      Boolean HasOperand { get; }
   }

   /// <summary>
   /// This is abstract base class for any op code stored in <see cref="MethodILDefinition"/>.
   /// The purpose of this class is to capture <see cref="Physical.OpCode"/> and its operand, if any.
   /// </summary>
   /// <remarks>
   /// The instances of this class are not instantiable directly, instead use <see cref="OpCodeInfoWithTableIndex"/>, <see cref="OpCodeInfoWithInt32"/>, <see cref="OpCodeInfoWithInt64"/>, <see cref="OpCodeInfoWithSingle"/>, <see cref="OpCodeInfoWithDouble"/>, <see cref="OpCodeInfoWithString"/>, <see cref="OpCodeInfoWithIntegers"/>, or <see cref="OpCodeInfoWithNoOperand"/>.
   /// </remarks>
   public abstract class OpCodeInfo : IOpCodeInfo
   {
      private readonly Byte _code; // Save some memory - use byte instead of integer.

      // Disable inheritance to other assemblies
      internal OpCodeInfo( OpCodeID code )
      {
         this._code = checked((Byte) code);
      }

      /// <summary>
      /// Gets the <see cref="Physical.OpCodeID"/> for this <see cref="OpCodeInfo"/>.
      /// </summary>
      /// <seealso cref="Physical.OpCodeID"/>
      /// <seealso cref="Physical.OpCode"/>
      /// <seealso cref="OpCodes"/>
      public OpCodeID OpCodeID
      {
         get
         {
            return (OpCodeID) this._code;
         }
      }

      /// <summary>
      /// Returns the <see cref="OpCodeInfoKind"/> of this <see cref="OpCodeInfo"/>, which can be used to deduce the actual type of this <see cref="OpCodeInfo"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeInfoKind"/> of this <see cref="OpCodeInfo"/>, which can be used to deduce the actual type of this <see cref="OpCodeInfo"/>.</value>
      /// <seealso cref="OpCodeInfoKind"/>
      public abstract OpCodeInfoKind InfoKind { get; }

      /// <inheritdoc />
      public abstract Boolean HasOperand { get; }

      /// <summary>
      /// Gets the additional size of operand of this <see cref="OpCodeInfo"/>, in bytes.
      /// </summary>
      /// <value>The additional size of operand of this <see cref="OpCodeInfo"/>, in bytes.</value>
      public virtual Int32 DynamicOperandByteSize
      {
         get
         {
            return 0;
         }
      }

      /// <summary>
      /// Returns the textual representation of this <see cref="OpCodeInfo"/>.
      /// This includes at least textual representation of the <see cref="OpCodeID"/>.
      /// </summary>
      /// <returns>The textual representation of this <see cref="OpCodeInfo"/>.</returns>
      public override String ToString()
      {
         return this.OpCodeID.ToString();
      }

   }

   /// <summary>
   /// This is interface providing access to the operand of the <see cref="IOpCodeInfo" /> without generic argument of the interface.
   /// </summary>
   public interface IOpCodeInfoWithOperand : IOpCodeInfo
   {
      /// <summary>
      /// Gets the operand for this <see cref="IOpCodeInfoWithOperand"/>.
      /// </summary>
      /// <value>The operand for this <see cref="IOpCodeInfoWithOperand"/>.</value>
      Object Operand { get; }
   }

   /// <summary>
   /// This interface is for any subclass of <see cref="OpCodeInfo"/>, which has a read-only operand.
   /// </summary>
   /// <typeparam name="TOperand">The type of the operand.</typeparam>
   /// <remarks>
   /// This interface exists because not all <see cref="OpCodeInfo"/>s which have operand, inherit from <see cref="OpCodeInfoWithOperand{TOperand}"/>.
   /// One reason for this is that <see cref="OpCodeInfoWithOperand{TOperand}"/> provides both getter and setter for operand, which is not always sensible (e.g. for <see cref="OpCodeInfoWithIntegers"/>).
   /// </remarks>
   public interface IOpCodeInfoWithOperand<out TOperand> : IOpCodeInfoWithOperand
   {
      /// <summary>
      /// Gets the operand for this <see cref="IOpCodeInfoWithOperand{TOperand}"/>.
      /// </summary>
      /// <value>The operand for this <see cref="IOpCodeInfoWithOperand{TOperand}"/>.</value>
      new TOperand Operand { get; }

   }

   /// <summary>
   /// This interface is for any subclass of <see cref="OpCodeInfo"/>, which has a settable operand.
   /// </summary>
   /// <typeparam name="TOperand">The type of the operand.</typeparam>
   public interface IOpCodeInfoWithOperandAndSetter<TOperand> : IOpCodeInfoWithOperand<TOperand>
   {
      /// <summary>
      /// Gets or sets the operand for this <see cref="IOpCodeInfoWithOperandAndSetter{TOperand}"/>.
      /// </summary>
      /// <value>The operand for this <see cref="IOpCodeInfoWithOperandAndSetter{TOperand}"/>.</value>
      new TOperand Operand { get; set; }
   }

   /// <summary>
   /// This is abstract base class for all <see cref="OpCodeInfo"/>s which have a gettable and settable operand of some sort.
   /// </summary>
   /// <typeparam name="TOperand">The type of the operand.</typeparam>
   /// <remarks>
   /// <para>
   /// The instances of this class are not instantiable directly, instead use <see cref="OpCodeInfoWithTableIndex"/>, <see cref="OpCodeInfoWithInt32"/>, <see cref="OpCodeInfoWithInt64"/>, <see cref="OpCodeInfoWithSingle"/>, <see cref="OpCodeInfoWithDouble"/>, or <see cref="OpCodeInfoWithString"/>.
   /// </para>
   /// <para>
   /// It is also possible to create custom instances inheriting from this <see cref="OpCodeInfoWithOperand{TOperand}"/>, which should have <see cref="OpCodeInfo.InfoKind"/> of value <see cref="OpCodeInfoKind.CustomStart"/> or larger.
   /// </para>
   /// </remarks>
   public abstract class OpCodeInfoWithOperand<TOperand> : OpCodeInfo, IOpCodeInfoWithOperandAndSetter<TOperand>
   {

      /// <summary>
      /// Initializes a new instance of <see cref="OpCodeInfoWithOperand{TOperand}"/>.
      /// </summary>
      /// <param name="code">The <see cref="OpCodeID"/>.</param>
      /// <param name="operand">The operand.</param>
      public OpCodeInfoWithOperand( OpCodeID code, TOperand operand )
         : base( code )
      {
         this.Operand = operand;
      }

      /// <inheritdoc />
      public sealed override Boolean HasOperand
      {
         get
         {
            return true;
         }
      }

      Object IOpCodeInfoWithOperand.Operand
      {
         get
         {
            return this.Operand;
         }
      }

      /// <summary>
      /// Gets or sets the operand for this <see cref="OpCodeInfoWithOperand{TOperand}"/>.
      /// </summary>
      /// <value>The operand for this <see cref="OpCodeInfoWithOperand{TOperand}"/>.</value>
      public TOperand Operand { get; set; }
   }

   /// <summary>
   /// This is abstract base class for all <see cref="OpCodeInfo"/>s which have a gettable, but not settable, operand of some sort.
   /// </summary>
   /// <typeparam name="TOperand">The type of the operand.</typeparam>
   /// <remarks>
   /// <para>
   /// The instances of this class are not instantiable directly, instead use <see cref="OpCodeInfoWithIntegers"/>.
   /// </para>
   /// <para>
   /// It is also possible to create custom instances inheriting from this <see cref="OpCodeInfoWithOperandGetter{TOperand}"/>, which should have <see cref="OpCodeInfo.InfoKind"/> of value <see cref="OpCodeInfoKind.CustomStart"/> or larger.
   /// </para>
   /// </remarks>
   public abstract class OpCodeInfoWithOperandGetter<TOperand> : OpCodeInfo, IOpCodeInfoWithOperand<TOperand>
   {
      /// <summary>
      /// Initializes a new instance of <see cref="OpCodeInfoWithOperandGetter{TOperand}"/>.
      /// </summary>
      /// <param name="code">The <see cref="OpCodeID"/>.</param>
      /// <param name="operand">The operand.</param>
      public OpCodeInfoWithOperandGetter( OpCodeID code, TOperand operand )
         : base( code )
      {
         this.Operand = operand;
      }

      Object IOpCodeInfoWithOperand.Operand
      {
         get
         {
            return this.Operand;
         }
      }

      /// <summary>
      /// Gets the operand for this <see cref="OpCodeInfoWithOperandGetter{TOperand}"/>.
      /// </summary>
      /// <value>The operand for this <see cref="OpCodeInfoWithOperandGetter{TOperand}"/>.</value>
      public TOperand Operand { get; }

      /// <inheritdoc />
      public sealed override Boolean HasOperand
      {
         get
         {
            return true;
         }
      }
   }

   /// <summary>
   /// This class represents any op code which takes a <see cref="TableIndex"/> as an operand.
   /// </summary>
   public sealed class OpCodeInfoWithTableIndex : OpCodeInfoWithOperand<TableIndex>
   {
      /// <summary>
      /// Creates a new instance of <see cref="OpCodeInfoWithTableIndex"/> with given <see cref="OpCode"/> and <see cref="TableIndex"/> as an operand.
      /// </summary>
      /// <param name="code">The <see cref="OpCodeID"/>.</param>
      /// <param name="token">The <see cref="TableIndex"/> acting as operand of the <paramref name="code"/>.</param>
      /// <seealso cref="OpCodes"/>
      public OpCodeInfoWithTableIndex( OpCodeID code, TableIndex token )
         : base( code, token )
      {
      }

      /// <summary>
      /// Returns the <see cref="OpCodeInfoKind.OperandTableIndex"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeInfoKind.OperandTableIndex"/>.</value>
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandTableIndex;
         }
      }
   }

   /// <summary>
   /// This class represents any op code which takes byte, signed byte, short, unsigned short, or integer as an operand.
   /// </summary>
   public sealed class OpCodeInfoWithInt32 : OpCodeInfoWithOperand<Int32>
   {

      /// <summary>
      /// Creates a new instance of <see cref="OpCodeInfoWithInt32"/> with given <see cref="OpCode"/> and integer as operand.
      /// </summary>
      /// <param name="code">The <see cref="OpCodeID"/>.</param>
      /// <param name="operand">The integer acting as an operand for <paramref name="code"/>.</param>
      /// <seealso cref="OpCodes"/>
      public OpCodeInfoWithInt32( OpCodeID code, Int32 operand )
         : base( code, operand )
      {
      }

      /// <summary>
      /// Returns the <see cref="OpCodeInfoKind.OperandInteger"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeInfoKind.OperandInteger"/>.</value>
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandInteger;
         }
      }
   }

   /// <summary>
   /// This class represents any op code which takes 64-bit integer as an operand.
   /// </summary>
   public sealed class OpCodeInfoWithInt64 : OpCodeInfoWithOperand<Int64>
   {
      /// <summary>
      /// Creates a new instance of <see cref="OpCodeInfoWithInt64"/> with given <see cref="OpCode"/> and 64-bit integer as operand.
      /// </summary>
      /// <param name="code">The <see cref="OpCodeID"/>.</param>
      /// <param name="operand">The 64-bit integer acting as an operand for <paramref name="code"/>.</param>
      /// <seealso cref="OpCodes"/>
      public OpCodeInfoWithInt64( OpCodeID code, Int64 operand )
         : base( code, operand )
      {
      }

      /// <summary>
      /// Returns the <see cref="OpCodeInfoKind.OperandInteger64"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeInfoKind.OperandInteger64"/>.</value>
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandInteger64;
         }
      }
   }

   /// <summary>
   /// This class represents any op code which takes no operand.
   /// </summary>
   /// <remarks>
   /// The instances of this class should be obtained through <see cref="OpCodeProvider.GetOperandlessInfoOrNull"/> or <see cref="E_CILPhysical.GetOperandlessInfoFor"/> methods.
   /// This is to save memory - no need to allocate duplicate <see cref="OpCodeInfoWithNoOperand"/> objects with identical state (assuming <see cref="OpCodeProvider"/> caches the <see cref="OpCodeInfoWithNoOperand"/>s).
   /// </remarks>
   public sealed class OpCodeInfoWithNoOperand : OpCodeInfo
   {
      /// <summary>
      /// Creates a new instance of <see cref="OpCodeInfoWithNoOperand"/> with specified <see cref="OpCodeID"/>.
      /// </summary>
      /// <param name="code">The <see cref="OpCodeID"/>.</param>
      /// <remarks>
      /// The only place where this constructor should be used is by types implementing <see cref="OpCodeProvider"/>.
      /// </remarks>
      public OpCodeInfoWithNoOperand( OpCodeID code )
         : base( code )
      {

      }

      /// <inheritdoc />
      public override Boolean HasOperand
      {
         get
         {
            return false;
         }
      }

      /// <summary>
      /// Returns the <see cref="OpCodeInfoKind.OperandNone"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeInfoKind.OperandNone"/>.</value>
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandNone;
         }
      }

   }

   /// <summary>
   /// This class represents any op code which takes 64-bit floating point number as an operand.
   /// </summary>
   public sealed class OpCodeInfoWithDouble : OpCodeInfoWithOperand<Double>
   {
      /// <summary>
      /// Creates a new instance of <see cref="OpCodeInfoWithDouble"/> with given <see cref="OpCode"/> and 64-bit floating point number as operand.
      /// </summary>
      /// <param name="code">The <see cref="OpCodeID"/>.</param>
      /// <param name="operand">The 64-bit floating point number acting as an operand for <paramref name="code"/>.</param>
      /// <seealso cref="OpCodes"/>
      public OpCodeInfoWithDouble( OpCodeID code, Double operand )
         : base( code, operand )
      {
      }

      /// <summary>
      /// Returns the <see cref="OpCodeInfoKind.OperandR8"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeInfoKind.OperandR8"/>.</value>
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandR8;
         }
      }
   }

   /// <summary>
   /// This class represents any op code which takes string as an operand.
   /// </summary>
   public sealed class OpCodeInfoWithString : OpCodeInfoWithOperand<String>
   {
      /// <summary>
      /// Creates a new instance of <see cref="OpCodeInfoWithString"/> with given <see cref="OpCode"/> and string as operand.
      /// </summary>
      /// <param name="code">The <see cref="OpCodeID"/>.</param>
      /// <param name="operand">The string acting as an operand for <paramref name="code"/>.</param>
      /// <seealso cref="OpCodes"/>
      public OpCodeInfoWithString( OpCodeID code, String operand )
         : base( code, operand )
      {
      }

      /// <summary>
      /// Returns the <see cref="OpCodeInfoKind.OperandString"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeInfoKind.OperandString"/>.</value>
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandString;
         }
      }
   }

   /// <summary>
   /// This class represents any op code which takes variable amount of integers as an operand.
   /// </summary>
   public sealed class OpCodeInfoWithIntegers : OpCodeInfoWithOperandGetter<List<Int32>>
   {
      /// <summary>
      /// Creates a new instance of <see cref="OpCodeInfoWithIntegers"/> with given <see cref="OpCode"/> and initial capacity for integer list.
      /// </summary>
      /// <param name="code">The <see cref="OpCodeID"/>.</param>
      /// <param name="offsetsCount">The initial capacity for integer list.</param>
      /// <seealso cref="OpCodes"/>
      public OpCodeInfoWithIntegers( OpCodeID code, Int32 offsetsCount = 0 )
         : base( code, new List<Int32>( offsetsCount ) )
      {
      }

      /// <summary>
      /// Returns the <see cref="OpCodeInfoKind.OperandIntegerList"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeInfoKind.OperandIntegerList"/>.</value>
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandIntegerList;
         }
      }


      /// <inheritdoc />
      public override Int32 DynamicOperandByteSize
      {
         get
         {
            return this.Operand.Count * sizeof( Int32 );
         }
      }
   }

   /// <summary>
   /// This class represents any op code which takes 32-bit floating point number as an operand.
   /// </summary>
   public sealed class OpCodeInfoWithSingle : OpCodeInfoWithOperand<Single>
   {
      /// <summary>
      /// Creates a new instance of <see cref="OpCodeInfoWithSingle"/> with given <see cref="OpCode"/> and 32-bit floating point number as operand.
      /// </summary>
      /// <param name="code">The <see cref="OpCode"/>.</param>
      /// <param name="operand">The 32-bit floating point number acting as an operand for <paramref name="code"/>.</param>
      /// <seealso cref="OpCodes"/>
      public OpCodeInfoWithSingle( OpCodeID code, Single operand )
         : base( code, operand )
      {
      }

      /// <summary>
      /// Returns the <see cref="OpCodeInfoKind.OperandR4"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeInfoKind.OperandR4"/>.</value>
      public override OpCodeInfoKind InfoKind
      {
         get
         {
            return OpCodeInfoKind.OperandR4;
         }
      }
   }

   /// <summary>
   /// This enumeration contains information on what kind of of operand the <see cref="OpCodeInfo"/> has, and thus also what type it really is.
   /// </summary>
   public enum OpCodeInfoKind
   {
      /// <summary>
      /// The <see cref="OpCodeInfo"/> does not have operand, and is of type <see cref="OpCodeInfoWithNoOperand"/>.
      /// </summary>
      OperandNone,
      /// <summary>
      /// The <see cref="OpCodeInfo"/> has <see cref="TableIndex"/> as operand, and is of type <see cref="OpCodeInfoWithTableIndex"/>.
      /// </summary>
      OperandTableIndex,
      /// <summary>
      /// The <see cref="OpCodeInfo"/> has integer as operand, and is of type <see cref="OpCodeInfoWithInt32"/>.
      /// </summary>
      OperandInteger,
      /// <summary>
      /// The <see cref="OpCodeInfo"/> has 64-bit integer as operand, and is of type <see cref="OpCodeInfoWithInt64"/>.
      /// </summary>
      OperandInteger64,
      /// <summary>
      /// The <see cref="OpCodeInfo"/> has 64-bit floating point number as operand, and is of type <see cref="OpCodeInfoWithDouble"/>.
      /// </summary>
      OperandR8,
      /// <summary>
      /// The <see cref="OpCodeInfo"/> has 32-bit floating point number as operand, and is of type <see cref="OpCodeInfoWithSingle"/>.
      /// </summary>
      OperandR4,
      /// <summary>
      /// The <see cref="OpCodeInfo"/> has string as operand, and is of type <see cref="OpCodeInfoWithString"/>.
      /// </summary>
      OperandString,
      /// <summary>
      /// The <see cref="OpCodeInfo"/> has integer list as operand, and is of type <see cref="OpCodeInfoWithIntegers"/>.
      /// </summary>
      OperandIntegerList,
      /// <summary>
      /// This is smallest value for <see cref="IOpCodeInfo"/>s of context-specific custom type.
      /// </summary>
      CustomStart,
   }
}

public static partial class E_CILPhysical
{


   /// <summary>
   /// This method will sort all <see cref="MethodILDefinition.ExceptionBlocks"/> so that they are in order they appear when traversing byte code.
   /// </summary>
   /// <param name="il">The <see cref="MethodILDefinition"/>.</param>
   /// <exception cref="NullReferenceException">If <paramref name="il"/> is <c>null</c>.</exception>
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