/*
 * Copyright 2015 Stanislav Muhametsin. All rights Reserved.
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
using CILAssemblyManipulator.Physical.Crypto;
using CommonUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Tests.Physical
{
   public class CryptoTests
   {
      [Test]
      public void TestHashComputation()
      {
         var sha1 = new SHA1_128();
         var hash = sha1.ComputeHash(
            new Byte[]
            {
               00, 00, 00, 00, 00, 00, 00, 00, 04, 00, 00, 00, 00, 00, 00, 00
            },
            0,
            16
            );
         var pkToken = hash.Skip( hash.Length - 8 ).Reverse().ToArray();
         Assert.IsTrue( ArrayEqualityComparer<Byte>.ArrayEquality(
            new Byte[] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 },
            pkToken
            ) );
      }

      [Test]
      public void VerifyHashComputation()
      {
         VerifyNativeVSCAM( () => new System.Security.Cryptography.SHA1Managed(), () => new SHA1_128() );
         VerifyNativeVSCAM( () => new System.Security.Cryptography.SHA256Managed(), () => new SHA2_256() );
         VerifyNativeVSCAM( () => new System.Security.Cryptography.SHA384Managed(), () => new SHA2_384() );
         VerifyNativeVSCAM( () => new System.Security.Cryptography.SHA512Managed(), () => new SHA2_512() );
         VerifyNativeVSCAM( () => new System.Security.Cryptography.MD5Cng(), () => new MD5() );
      }

      private void VerifyNativeVSCAM(
         Func<System.Security.Cryptography.HashAlgorithm> nativeFactory,
         Func<BlockHashAlgorithm> camFactory
         )
      {
         var r = new Random();
         var count = 1000 + ( r.NextInt32() % 1000 );
         var bytez = r.NextBytes( count );

         Byte[] nativeHash;
         using ( var native = nativeFactory() )
         {
            nativeHash = native.ComputeHash( bytez );
         }

         Byte[] camHash;
         using ( var cam = camFactory() )
         {
            camHash = cam.ComputeHash( bytez, 0, bytez.Length );
         }

         Assert.IsTrue(
            ArrayEqualityComparer<Byte>.ArrayEquality( nativeHash, camHash ),
            "The hash differed:\nnative hash: {0}\nCAM hash: {1}\ninput: {2}",
            StringConversions.CreateHexString( nativeHash ),
            StringConversions.CreateHexString( camHash ),
            StringConversions.CreateHexString( bytez )
            );
      }
   }
}
