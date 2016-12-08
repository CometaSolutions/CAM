/*
 * Copyright 2016 Stanislav Muhametsin. All rights Reserved.
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
using UtilPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TabularMetaData.Meta;
using UtilPack.Extension;
using TOpCodeEqualityAcceptor = UtilPack.Visiting.AcceptorWithContext<CILAssemblyManipulator.Physical.IOpCodeInfo, CILAssemblyManipulator.Physical.IOpCodeInfo>;
using TOpCodeHashCodeAcceptor = UtilPack.Visiting.AcceptorWithReturnValue<CILAssemblyManipulator.Physical.IOpCodeInfo, System.Int32>;
using UtilPack.Visiting;

namespace CILAssemblyManipulator.Physical.Meta
{
   /// <summary>
   /// This interface captures some of the operations crucial for interacting with reading and manipulating IL byte code.
   /// </summary>
   /// <remarks>
   /// Unless specifically desired, instead of directly implementing this interface, a <see cref="T:CILAssemblyManipulator.Physical.Meta.DefaultOpCodeProvider"/> should be used direclty, or by subclassing.
   /// </remarks>
   public interface OpCodeProvider : SelfDescribingExtensionByCompositionProvider<Object>
   {
      /// <summary>
      /// Tries to get an instance of <see cref="OpCodeInfoWithNoOperand"/> for a given <see cref="OpCodeID"/>, or return <c>null</c> on failure.
      /// </summary>
      /// <param name="codeID">The <see cref="OpCodeID"/>.</param>
      /// <returns>An instance of <see cref="OpCodeInfoWithNoOperand"/> for a given <paramref name="codeID"/>, or <c>null</c> if the <paramref name="codeID"/> was unrecognized or did not represent an operandless op code.</returns>
      OpCodeInfoWithNoOperand GetOperandlessInfoOrNull( OpCode codeID );
   }
}

public static partial class E_CILPhysical
{
   /// <summary>
   /// Gets the <see cref="OpCodeInfoWithNoOperand"/> for a given <see cref="OpCodeID"/>, or throws an exception if no <see cref="OpCodeInfoWithNoOperand"/> is found.
   /// </summary>
   /// <param name="opCodeProvider">The <see cref="OpCodeProvider"/>.</param>
   /// <param name="codeID">The <see cref="OpCodeID"/>.</param>
   /// <returns>An instance of <see cref="OpCodeInfoWithNoOperand"/> for <paramref name="codeID"/>.</returns>
   /// <exception cref="NullReferenceException">If this <paramref name="opCodeProvider"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If no suitable <see cref="OpCodeInfoWithNoOperand"/> is found.</exception>
   /// <seealso cref="OpCodeProvider.GetOperandlessInfoOrNull"/>
   public static OpCodeInfoWithNoOperand GetOperandlessInfoFor( this OpCodeProvider opCodeProvider, OpCode codeID )
   {
      var retVal = opCodeProvider.GetOperandlessInfoOrNull( codeID );
      if ( retVal == null )
      {
         throw new ArgumentException( "Op code " + codeID + " is not operandless opcode." );
      }
      return retVal;
   }

   /// <summary>
   /// Gets or creates a new <see cref="OpCodeProvider"/>.
   /// </summary>
   /// <param name="provider">The <see cref="MetaDataTableInformationProvider"/>.</param>
   /// <returns>A <see cref="OpCodeProvider"/> supported by this <see cref="MetaDataTableInformationProvider"/>.</returns>
   /// <seealso cref="OpCodeProvider"/>
   public static OpCodeProvider CreateOpCodeProvider( this MetaDataTableInformationProvider provider )
   {
      return provider.GetFunctionality<OpCodeProvider>();
   }


   /// <summary>
   /// Checks whether two <see cref="IOpCodeInfo"/>s are exactly equal.
   /// </summary>
   /// <param name="provider">The <see cref="OpCodeProvider"/>.</param>
   /// <param name="x">The first <see cref="IOpCodeInfo"/>.</param>
   /// <param name="y">The second <see cref="IOpCodeInfo"/>.</param>
   /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> are same object or otherwise are exactly equal; <c>false</c> otherwise.</returns>
   public static Boolean OpCodeEquality( this OpCodeProvider provider, IOpCodeInfo x, IOpCodeInfo y )
   {
      return provider.GetFunctionality<TOpCodeEqualityAcceptor>().Accept( x, y );
   }

   /// <summary>
   /// Computes the hash code for given <see cref="SignatureElement"/>.
   /// </summary>
   /// <param name="provider">The <see cref="OpCodeProvider"/>.</param>
   /// <param name="x">The <see cref="IOpCodeInfo"/>.</param>
   /// <returns>The hash code for given <see cref="IOpCodeInfo"/>.</returns>
   public static Int32 OpCodeHashCode( this OpCodeProvider provider, IOpCodeInfo x )
   {
      return x == null ? 0 : provider.GetFunctionality<TOpCodeHashCodeAcceptor>().Accept( x );
   }

   /// <summary>
   /// Creates a new instance of signature of given type, which will contain a shallow or deep copy of this signature.
   /// </summary>
   /// <typeparam name="TOpCodeInfo">The type of signature reference.</typeparam>
   /// <param name="provider">The <see cref="SignatureProvider"/>.</param>
   /// <param name="sig">The <see cref="AbstractSignature"/>.</param>
   /// <param name="tableIndexTranslator">Optional callback to translate table indices of <see cref="CustomModifierSignature.CustomModifierType"/> and <see cref="ClassOrValueTypeSignature.Type"/> properties while copying.</param>
   /// <param name="isDeep">Whether the copy is deep.</param>
   /// <returns>A new instance of <typeparamref name="TOpCodeInfo"/>, which has all of its contents deeply copied from given signature.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="sig"/> is <c>null</c>.</exception>
   /// <exception cref="NotSupportedException">If <see cref="AbstractSignature.SignatureKind"/> returns any other value than what the <see cref="SignatureKind"/> enumeration has.</exception>
   public static TOpCodeInfo CopyOpCode<TOpCodeInfo>( this OpCodeProvider provider, TOpCodeInfo sig, Boolean isDeep, Func<TableIndex, TableIndex> tableIndexTranslator = null )
      where TOpCodeInfo : IOpCodeInfo
   {
      TOpCodeInfo retVal;
      if ( sig == null )
      {
         retVal = sig;
      }
      else
      {
         var acceptor = provider.GetFunctionality<AcceptorWithContextAndReturnValue<IOpCodeInfo, CopyingArgs, IOpCodeInfo>>();
         Boolean success;
         retVal = (TOpCodeInfo) acceptor.Accept( sig, new CopyingArgs( isDeep, tableIndexTranslator ), out success );
         if ( !success )
         {
            throw new NotSupportedException( "Could not find functionality to copy signature or part of it." );
         }
      }
      return retVal;
   }
}