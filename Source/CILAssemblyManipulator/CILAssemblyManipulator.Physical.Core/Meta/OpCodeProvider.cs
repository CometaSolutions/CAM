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
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TabularMetaData.Meta;

namespace CILAssemblyManipulator.Physical.Meta
{
   /// <summary>
   /// This interface captures some of the operations crucial for interacting with reading and manipulating IL byte code.
   /// </summary>
   /// <remarks>
   /// Unless specifically desired, instead of directly implementing this interface, a <see cref="T:CILAssemblyManipulator.Physical.Meta.DefaultOpCodeProvider"/> should be used direclty, or by subclassing.
   /// </remarks>
   public interface OpCodeProvider
   {
      /// <summary>
      /// Tries to get an instance of <see cref="OpCodeInfoWithNoOperand"/> for a given <see cref="OpCodeID"/>, or return <c>null</c> on failure.
      /// </summary>
      /// <param name="codeID">The <see cref="OpCodeID"/>.</param>
      /// <returns>An instance of <see cref="OpCodeInfoWithNoOperand"/> for a given <paramref name="codeID"/>, or <c>null</c> if the <paramref name="codeID"/> was unrecognized or did not represent an operandless op code.</returns>
      OpCodeInfoWithNoOperand GetOperandlessInfoOrNull( OpCodeID codeID );
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
   public static OpCodeInfoWithNoOperand GetOperandlessInfoFor( this OpCodeProvider opCodeProvider, OpCodeID codeID )
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
}