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

#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
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
}