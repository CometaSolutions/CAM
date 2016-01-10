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
using CILAssemblyManipulator.Physical.Crypto;
using CILAssemblyManipulator.Physical.IO;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static partial class E_CILPhysical
{
   public static Byte[] CreatePublicKeyFromStrongName( this CryptoCallbacks eArgs, StrongNameKeyPair strongName, AssemblyHashAlgorithm? algorithmOverride = null )
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
               algorithmOverride,
               out retVal,
               out signingAlgorithm,
               out rParams,
               out errorString
               );
         }
         else
         {
            retVal = eArgs.ExtractPublicKeyFromCSPContainer( container );
         }
      }

      return retVal;
   }

   public static Boolean IsMatch( this AssemblyDefinition aDef, AssemblyReference aRef, HashStreamInfo? hashStreamInfo )
   {
      return aDef.IsMatch( new AssemblyInformationForResolving( aRef ), aRef.Attributes.IsRetargetable(), hashStreamInfo );
   }

   public static Boolean IsMatch( this AssemblyDefinition aDef, AssemblyInformationForResolving? aRef, Boolean isRetargetable, HashStreamInfo? hashStreamInfo )
   {
      var retVal = aDef != null
         && aRef != null;
      if ( retVal )
      {
         var aReff = aRef.Value;
         var defInfo = aDef.AssemblyInformation;
         var refInfo = aReff.AssemblyInformation;
         if ( isRetargetable )
         {
            retVal = String.Equals( defInfo.Name, refInfo.Name );
         }
         else
         {
            var defPK = defInfo.PublicKeyOrToken;
            var refPK = refInfo.PublicKeyOrToken;
            retVal = defPK.IsNullOrEmpty() == refPK.IsNullOrEmpty()
               && defInfo.Equals( refInfo, aReff.IsFullPublicKey )
               && ( aReff.IsFullPublicKey || ( hashStreamInfo.HasValue && ArrayEqualityComparer<Byte>.ArrayEquality( hashStreamInfo.Value.ComputePublicKeyToken( defPK ), refPK ) ) );
         }
      }

      return retVal;
   }
}