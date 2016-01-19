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
   /// This is abstract base class for any op code stored in <see cref="MethodILDefinition"/>.
   /// The purpose of this class is to capture <see cref="Physical.OpCode"/> and its operand, if any.
   /// </summary>
   /// <remarks>
   /// The instances of this class are not instantiable directly, instead use <see cref="OpCodeInfoWithTableIndex"/>, <see cref="OpCodeInfoWithInt32"/>, <see cref="OpCodeInfoWithInt64"/>, <see cref="OpCodeInfoWithSingle"/>, <see cref="OpCodeInfoWithDouble"/>, <see cref="OpCodeInfoWithString"/>, <see cref="OpCodeInfoWithIntegers"/>, or <see cref="OpCodeInfoWithNoOperand"/>.
   /// </remarks>
   public abstract class OpCodeInfo
   {
      private readonly UInt16 _code; // Save some memory - use integer instead of actual code (Int64 (amount of space taken by OpCode structure) -> Int16)

      // Disable inheritance to other assemblies
      internal OpCodeInfo( OpCodeEncoding code )
      {
         this._code = (UInt16) code;
      }

      /// <summary>
      /// Gets the <see cref="OpCodeEncoding"/> for this <see cref="OpCodeInfo"/>.
      /// </summary>
      /// <seealso cref="OpCodeEncoding"/>
      /// <seealso cref="Physical.OpCode"/>
      /// <seealso cref="OpCodes"/>
      public OpCodeEncoding OpCode
      {
         get
         {
            return (OpCodeEncoding) this._code;
         }
      }

      /// <summary>
      /// Returns the <see cref="OpCodeOperandKind"/> of this <see cref="OpCodeInfo"/>, which can be used to deduce the actual type of this <see cref="OpCodeInfo"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeOperandKind"/> of this <see cref="OpCodeInfo"/>, which can be used to deduce the actual type of this <see cref="OpCodeInfo"/>.</value>
      /// <seealso cref="OpCodeOperandKind"/>
      public abstract OpCodeOperandKind InfoKind { get; }

      /// <summary>
      /// Gets the additional size of operand of this <see cref="OpCode"/>, in bytes.
      /// </summary>
      /// <value>The additional size of operand of this <see cref="OpCode"/>, in bytes.</value>
      public virtual Int32 AdditionalOperandByteSize
      {
         get
         {
            return 0;
         }
      }

      /// <summary>
      /// Returns the textual representation of this <see cref="OpCodeInfo"/>.
      /// This includes at least textual representation of the <see cref="OpCode"/>.
      /// </summary>
      /// <returns>The textual representation of this <see cref="OpCodeInfo"/>.</returns>
      public override String ToString()
      {
         return this.OpCode.ToString();
      }

   }

   /// <summary>
   /// This interface is for any subclass of <see cref="OpCodeInfo"/>, which has an operand.
   /// </summary>
   /// <typeparam name="TOperand">The type of the operand.</typeparam>
   /// <remarks>
   /// This interface exists because not all <see cref="OpCodeInfo"/>s which have operand, inherit from <see cref="OpCodeInfoWithOperand{TOperand}"/>.
   /// One reason for this is that <see cref="OpCodeInfoWithOperand{TOperand}"/> provides both getter and setter for operand, which is not always sensible (e.g. for <see cref="OpCodeInfoWithIntegers"/>).
   /// </remarks>
   public interface IOpCodeInfoWithOperand<out TOperand>
   {
      /// <summary>
      /// Gets the operand for this <see cref="IOpCodeInfoWithOperand{TOperand}"/>.
      /// </summary>
      /// <value>The operand for this <see cref="IOpCodeInfoWithOperand{TOperand}"/>.</value>
      TOperand Operand { get; }
   }

   /// <summary>
   /// This is abstract base class for all <see cref="OpCodeInfo"/>s which have an operand of some sort.
   /// </summary>
   /// <typeparam name="TOperand">The type of the operand.</typeparam>
   /// <remarks>
   /// The instances of this class are not instantiable directly, instead use <see cref="OpCodeInfoWithTableIndex"/>, <see cref="OpCodeInfoWithInt32"/>, <see cref="OpCodeInfoWithInt64"/>, <see cref="OpCodeInfoWithSingle"/>, <see cref="OpCodeInfoWithDouble"/>, <see cref="OpCodeInfoWithString"/>, or <see cref="OpCodeInfoWithIntegers"/>.
   /// </remarks>
   public abstract class OpCodeInfoWithOperand<TOperand> : OpCodeInfo, IOpCodeInfoWithOperand<TOperand>
   {
      // Disable inheritance to other assemblies
      internal OpCodeInfoWithOperand( OpCodeEncoding code, TOperand operand )
         : base( code )
      {
         this.Operand = operand;
      }

      /// <summary>
      /// Gets or sets the operand for this <see cref="OpCodeInfoWithOperand{TOperand}"/>.
      /// </summary>
      /// <value>The operand for this <see cref="OpCodeInfoWithOperand{TOperand}"/>.</value>
      public TOperand Operand { get; set; }
   }

   /// <summary>
   /// This class represents any op code which takes a <see cref="TableIndex"/> as an operand.
   /// </summary>
   public sealed class OpCodeInfoWithTableIndex : OpCodeInfoWithOperand<TableIndex>
   {
      /// <summary>
      /// Creates a new instance of <see cref="OpCodeInfoWithTableIndex"/> with given <see cref="OpCode"/> and <see cref="TableIndex"/> as an operand.
      /// </summary>
      /// <param name="code">The <see cref="OpCodeEncoding"/>.</param>
      /// <param name="token">The <see cref="TableIndex"/> acting as operand of the <paramref name="code"/>.</param>
      /// <seealso cref="OpCodes"/>
      public OpCodeInfoWithTableIndex( OpCodeEncoding code, TableIndex token )
         : base( code, token )
      {
      }

      /// <summary>
      /// Returns the <see cref="OpCodeOperandKind.OperandTableIndex"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeOperandKind.OperandTableIndex"/>.</value>
      public override OpCodeOperandKind InfoKind
      {
         get
         {
            return OpCodeOperandKind.OperandTableIndex;
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
      /// <param name="code">The <see cref="OpCodeEncoding"/>.</param>
      /// <param name="operand">The integer acting as an operand for <paramref name="code"/>.</param>
      /// <seealso cref="OpCodes"/>
      public OpCodeInfoWithInt32( OpCodeEncoding code, Int32 operand )
         : base( code, operand )
      {
      }

      /// <summary>
      /// Returns the <see cref="OpCodeOperandKind.OperandInteger"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeOperandKind.OperandInteger"/>.</value>
      public override OpCodeOperandKind InfoKind
      {
         get
         {
            return OpCodeOperandKind.OperandInteger;
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
      /// <param name="code">The <see cref="OpCodeEncoding"/>.</param>
      /// <param name="operand">The 64-bit integer acting as an operand for <paramref name="code"/>.</param>
      /// <seealso cref="OpCodes"/>
      public OpCodeInfoWithInt64( OpCodeEncoding code, Int64 operand )
         : base( code, operand )
      {
      }

      /// <summary>
      /// Returns the <see cref="OpCodeOperandKind.OperandInteger64"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeOperandKind.OperandInteger64"/>.</value>
      public override OpCodeOperandKind InfoKind
      {
         get
         {
            return OpCodeOperandKind.OperandInteger64;
         }
      }
   }

   /// <summary>
   /// This class represents any op code which takes no operand.
   /// </summary>
   /// <remarks>
   /// The instances of this class should be obtained through <see cref="Meta.OpCodeProvider.TryGetOperandlessInfoFor"/> or <see cref="E_CILPhysical.GetOperandlessInfoFor"/> methods.
   /// This is to save memory - no need to allocate duplicate <see cref="OpCodeInfoWithNoOperand"/> objects with identical state.
   /// </remarks>
   public sealed class OpCodeInfoWithNoOperand : OpCodeInfo
   {
      public OpCodeInfoWithNoOperand( OpCodeEncoding code )
         : base( code )
      {

      }

      /// <summary>
      /// Returns the <see cref="OpCodeOperandKind.OperandNone"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeOperandKind.OperandNone"/>.</value>
      public override OpCodeOperandKind InfoKind
      {
         get
         {
            return OpCodeOperandKind.OperandNone;
         }
      }

      ///// <summary>
      ///// This method can be used to obtain instance of <see cref="OpCodeInfoWithNoOperand"/> when the <see cref="OpCodeEncoding"/> is known.
      ///// </summary>
      ///// <param name="encoded">The <see cref="OpCodeEncoding"/> of an op code.</param>
      ///// <returns>An instance of <see cref="OpCodeInfoWithNoOperand"/> for given <see cref="OpCodeEncoding"/>.</returns>
      ///// <exception cref="ArgumentException">If <paramref name="encoded"/> does not represent an op code, which takes no operand.</exception>
      ///// <remarks>
      ///// The <see cref="OpCode"/> is deemed to accept no operand when its <see cref="OpCode.OperandType"/> is one of the following:
      ///// <list type="bullet">
      ///// <item><description><see cref="OperandType.InlineNone"/>.</description></item>
      ///// </list>
      ///// </remarks>
      ///// <seealso cref="OpCodeEncoding"/>
      ///// <seealso cref="OpCodes"/>
      ///// 
      ////public static OpCodeInfoWithNoOperand GetInstanceFor( OpCodeEncoding encoded )
      ////{
      ////   OpCodeInfoWithNoOperand retVal;
      ////   if ( !TryGetInstanceFor( encoded, out retVal ) )
      ////   {
      ////      throw new ArgumentException( "Op code " + encoded + " is not operandless opcode." );
      ////   }
      ////   return retVal;
      ////}

      ///// <summary>
      ///// Tries to get an instance of <see cref="OpCodeInfoWithNoOperand"/> for a given <see cref="OpCodeEncoding"/>.
      ///// </summary>
      ///// <param name="encoded">The <see cref="OpCodeEncoding"/> of an op code.</param>
      ///// <param name="opCodeInfo">This parameter will hold the retrieved instance of <see cref="OpCodeInfoWithNoOperand"/>, if any.</param>
      ///// <returns><c>true</c> if <paramref name="encoded"/> represents an op code, which takes no operand; <c>false</c> otherwise.</returns>
      ///// <remarks>
      ///// The <see cref="OpCode"/> is deemed to accept no operand when its <see cref="OpCode.OperandType"/> is one of the following:
      ///// <list type="bullet">
      ///// <item><description><see cref="OperandType.InlineNone"/>.</description></item>
      ///// </list>
      ///// </remarks>
      ///// <seealso cref="OpCodeEncoding"/>
      ///// <seealso cref="OpCodes"/>
      //public static Boolean TryGetInstanceFor( OpCodeEncoding encoded, out OpCodeInfoWithNoOperand opCodeInfo )
      //{
      //   return CodeInfosWithNoOperand.TryGetValue( encoded, out opCodeInfo );
      //}

      //public static OpCodeInfoWithNoOperand GetInstanceFor( OpCode code )
      //{
      //   return GetInstanceFor( code.Value );
      //}

      ///// <summary>
      ///// Gets all the values of the <see cref="OpCodeEncoding"/> enumeration, which accept no operands.
      ///// </summary>
      ///// <value>All the values of the <see cref="OpCodeEncoding"/> enumeration, which accept no operands.</value>
      //public static IEnumerable<OpCodeEncoding> OperandlessCodes
      //{
      //   get
      //   {
      //      return CodeInfosWithNoOperand.Keys;
      //   }
      //}
   }

   /// <summary>
   /// This class represents any op code which takes 64-bit floating point number as an operand.
   /// </summary>
   public sealed class OpCodeInfoWithDouble : OpCodeInfoWithOperand<Double>
   {
      /// <summary>
      /// Creates a new instance of <see cref="OpCodeInfoWithDouble"/> with given <see cref="OpCode"/> and 64-bit floating point number as operand.
      /// </summary>
      /// <param name="code">The <see cref="OpCodeEncoding"/>.</param>
      /// <param name="operand">The 64-bit floating point number acting as an operand for <paramref name="code"/>.</param>
      /// <seealso cref="OpCodes"/>
      public OpCodeInfoWithDouble( OpCodeEncoding code, Double operand )
         : base( code, operand )
      {
      }

      /// <summary>
      /// Returns the <see cref="OpCodeOperandKind.OperandR8"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeOperandKind.OperandR8"/>.</value>
      public override OpCodeOperandKind InfoKind
      {
         get
         {
            return OpCodeOperandKind.OperandR8;
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
      /// <param name="code">The <see cref="OpCodeEncoding"/>.</param>
      /// <param name="operand">The string acting as an operand for <paramref name="code"/>.</param>
      /// <seealso cref="OpCodes"/>
      public OpCodeInfoWithString( OpCodeEncoding code, String operand )
         : base( code, operand )
      {
      }

      /// <summary>
      /// Returns the <see cref="OpCodeOperandKind.OperandString"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeOperandKind.OperandString"/>.</value>
      public override OpCodeOperandKind InfoKind
      {
         get
         {
            return OpCodeOperandKind.OperandString;
         }
      }
   }

   /// <summary>
   /// This class represents any op code which takes variable amount of integers as an operand.
   /// </summary>
   public sealed class OpCodeInfoWithIntegers : OpCodeInfo, IOpCodeInfoWithOperand<List<Int32>>
   {
      /// <summary>
      /// Creates a new instance of <see cref="OpCodeInfoWithIntegers"/> with given <see cref="OpCode"/> and initial capacity for integer list.
      /// </summary>
      /// <param name="code">The <see cref="OpCodeEncoding"/>.</param>
      /// <param name="offsetsCount">The initial capacity for <see cref="Operand"/>.</param>
      /// <seealso cref="OpCodes"/>
      public OpCodeInfoWithIntegers( OpCodeEncoding code, Int32 offsetsCount = 0 )
         : base( code )
      {
         this.Operand = new List<Int32>( offsetsCount );
      }

      /// <summary>
      /// Returns the <see cref="OpCodeOperandKind.OperandIntegerList"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeOperandKind.OperandIntegerList"/>.</value>
      public override OpCodeOperandKind InfoKind
      {
         get
         {
            return OpCodeOperandKind.OperandIntegerList;
         }
      }

      /// <summary>
      /// Gets the list of integers acting as an operand for this <see cref="OpCodeInfoWithIntegers"/>.
      /// </summary>
      /// <value>The list of integers acting as an operand for this <see cref="OpCodeInfoWithIntegers"/>.</value>
      public List<Int32> Operand { get; }

      /// <inheritdoc />
      public override Int32 AdditionalOperandByteSize
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
      public OpCodeInfoWithSingle( OpCodeEncoding code, Single operand )
         : base( code, operand )
      {
      }

      /// <summary>
      /// Returns the <see cref="OpCodeOperandKind.OperandR4"/>.
      /// </summary>
      /// <value>The <see cref="OpCodeOperandKind.OperandR4"/>.</value>
      public override OpCodeOperandKind InfoKind
      {
         get
         {
            return OpCodeOperandKind.OperandR4;
         }
      }
   }

   /// <summary>
   /// This enumeration contains information on what kind of of operand the <see cref="OpCodeInfo"/> has, and thus also what type it really is.
   /// </summary>
   public enum OpCodeOperandKind
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
   }
}

public static partial class E_CILPhysical
{
   /// <summary>
   /// Gets the total byte count that a single <see cref="OpCodeInfo"/> takes.
   /// </summary>
   /// <param name="info">The single <see cref="OpCodeInfo"/>.</param>
   /// <returns>The total byte count of a single <see cref="OpCodeInfo"/>.</returns>
   /// <remarks>
   /// The total byte count is the size of op code of <see cref="OpCodeInfo"/> added with <see cref="OpCodeInfo.OperandByteSize"/>.
   /// </remarks>
   public static Int32 GetTotalByteCount( this OpCodeInfo info, OpCodeProvider opCodeProvider )
   {
      return info == null ? 0 : ( opCodeProvider.GetCodeFor( info.OpCode ).GetTotalByteCount() + info.AdditionalOperandByteSize );
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