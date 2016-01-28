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
extern alias CAMPhysical;
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.Crypto;
using CILAssemblyManipulator.Physical.IO;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static partial class E_CILPhysical
{
   /// <summary>
   /// Helper method to create a public key from <see cref="StrongNameKeyPair"/>, be it the one storing whole public-private key pair in its <see cref="StrongNameKeyPair.KeyPair"/> property, or container name in its <see cref="StrongNameKeyPair.ContainerName"/>.
   /// </summary>
   /// <param name="cryptoCallbacks">The <see cref="CryptoCallbacks"/>.</param>
   /// <param name="strongName">The <see cref="StrongNameKeyPair"/>. May be <c>null</c>.</param>
   /// <returns>The extracted public key, if successful, <c>null</c> otherwise.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="strongName"/> is not <c>null</c> and has <see cref="StrongNameKeyPair.ContainerName"/> set, and this <see cref="CryptoCallbacks"/> is <c>null</c>.</exception>
   public static Byte[] CreatePublicKeyFromStrongName(
      this CryptoCallbacks cryptoCallbacks,
      StrongNameKeyPair strongName
      )
   {
      Byte[] retVal;
      if ( strongName == null )
      {
         retVal = null;
      }
      else
      {
         var container = strongName.ContainerName;
         if ( container == null )
         {
            String errorString;
            AssemblyHashAlgorithm signingAlgorithm;
            RSAParameters rParams;
            CryptoUtils.TryCreateSigningInformationFromKeyBLOB(
               strongName.KeyPair.ToArray(),
               null,
               out retVal,
               out signingAlgorithm,
               out rParams,
               out errorString
               );
         }
         else
         {
            retVal = cryptoCallbacks.ExtractPublicKeyFromCSPContainer( container );
         }
      }

      return retVal;
   }

   /// <summary>
   /// Checks whether this <see cref="AssemblyDefinition"/> matches the given <see cref="AssemblyReference"/>, 
   /// </summary>
   /// <param name="aDef">The <see cref="AssemblyDefinition"/>.</param>
   /// <param name="aRef">The optional <see cref="AssemblyReference"/>.</param>
   /// <param name="publicKeyTokenComputer">The callback to use, if public key token computation is required.</param>
   /// <returns><c>true</c> if <paramref name="aRef"/> is not <c>null</c> and matches this <see cref="AssemblyDefinition"/>, taking into account that <paramref name="aRef"/> might have public key token instead of full public key; <c>false</c> otherwise.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="AssemblyDefinition"/> is <c>null</c>.</exception>
   /// <seealso cref="HashStreamInfo.HashComputer"/>
   public static Boolean IsMatch( this AssemblyDefinition aDef, AssemblyReference aRef, Func<Byte[], Byte[]> publicKeyTokenComputer )
   {
      return aDef.IsMatch( aRef == null ? null : new AssemblyInformationForResolving( aRef ), aRef?.Attributes.IsRetargetable() ?? false, publicKeyTokenComputer );
   }

   /// <summary>
   /// Checks whether this <see cref="AssemblyDefinition"/> matches the given <see cref="AssemblyInformationForResolving"/>.
   /// </summary>
   /// <param name="aDef">The <see cref="AssemblyDefinition"/>.</param>
   /// <param name="aRef">The optional <see cref="AssemblyInformationForResolving"/>.</param>
   /// <param name="isRetargetable">Whether the <paramref name="aRef"/> is retargetable.</param>
   /// <param name="publicKeyTokenComputer">The callback to use, if public key token computation is required.</param>
   /// <returns><c>true</c> if <paramref name="aRef"/> is not <c>null</c> and matches this <see cref="AssemblyDefinition"/>, taking into account that <paramref name="aRef"/> might have public key token instead of full public key; <c>false</c> otherwise.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="AssemblyDefinition"/> is <c>null</c>.</exception>
   /// <seealso cref="HashStreamInfo.HashComputer"/>
   public static Boolean IsMatch( this AssemblyDefinition aDef, AssemblyInformationForResolving aRef, Boolean isRetargetable, Func<Byte[], Byte[]> publicKeyTokenComputer )
   {
      var defInfo = aDef.AssemblyInformation;
      var retVal = aRef != null;
      if ( retVal )
      {
         var refInfo = aRef.AssemblyInformation;
         retVal = String.Equals( defInfo.Name, refInfo.Name );
         if ( retVal && !isRetargetable )
         {
            var defPK = defInfo.PublicKeyOrToken;
            var refPK = refInfo.PublicKeyOrToken;
            retVal = defPK.IsNullOrEmpty() == refPK.IsNullOrEmpty()
               && defInfo.Equals( refInfo, aRef.IsFullPublicKey )
               && ( aRef.IsFullPublicKey || ArrayEqualityComparer<Byte>.ArrayEquality( publicKeyTokenComputer?.Invoke( defPK ), refPK ) );
         }
      }

      return retVal;
   }
}