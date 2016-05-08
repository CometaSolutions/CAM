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
   public class CryptoTests : AbstractCAMTest
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

      [Test]
      public void VerifySignerData()
      {
         VerifyNativeVSCAM(
            () => new System.Security.Cryptography.RSACryptoServiceProvider( 1024 ),
            () => new System.Security.Cryptography.RSAPKCS1SignatureFormatter()
            );
      }

      private void VerifyNativeVSCAM(
         Func<System.Security.Cryptography.HashAlgorithm> nativeFactory,
         Func<BlockDigestAlgorithm> camFactory
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

      private void VerifyNativeVSCAM(
         Func<System.Security.Cryptography.RSACryptoServiceProvider> nativeAlgoFactory,
         Func<System.Security.Cryptography.AsymmetricSignatureFormatter> nativeFactory
         )
      {
         var r = new Random();
         var count = 1000 + ( r.NextInt32() % 1000 );
         var bytez = r.NextBytes( count );

         var hash = new SHA1_128().ComputeHash( bytez, 0, bytez.Length );

         var nativeFormatter = nativeFactory();
         nativeFormatter.SetHashAlgorithm( "SHA1" );


         Byte[] nativeSignature, nativeData;
         System.Security.Cryptography.RSAParameters rParams;
         using ( var nativeAlgo = nativeAlgoFactory() )
         {
            try
            {
               nativeFormatter.SetKey( nativeAlgo );
               nativeSignature = nativeFormatter.CreateSignature( hash );
               rParams = nativeAlgo.ExportParameters( true );
               var utilsType = "System.Security.Cryptography.Utils, " + MSCorLib.GetName().ToString();
               var rsapkcs1PaddingMethod = Type.GetType( utilsType )
                  .GetMethod( "RsaPkcs1Padding", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic );
               nativeData = (Byte[]) rsapkcs1PaddingMethod.Invoke( null, new Object[]
               {
                  nativeAlgo,
                  System.Security.Cryptography.CryptoConfig.EncodeOID( System.Security.Cryptography.CryptoConfig.MapNameToOID( "SHA1" ) ),
                  hash
               } );
            }
            finally
            {
               nativeAlgo.PersistKeyInCsp = false;
            }
         }

         //var pkcs = PKCS1Encoder.Create(
         //   rParams.Modulus.Length * 8,
         //   ASNFormatter.Create(
         //      hash,
         //      CILAssemblyManipulator.Physical.AssemblyHashAlgorithm.SHA1
         //   )
         //);
         //var camData = new Byte[pkcs.DataSize];
         //pkcs.PopulateData(
         //   camData,
         //   0
         //   );
         //Assert.IsTrue( ArrayEqualityComparer<Byte>.ArrayEquality( camData, nativeData ) );

         Byte[] camSignature;
         using ( var camCrypto = new DefaultCryptoCallbacks() )
         {
            camSignature = camCrypto.CreateRSASignature(
               CILAssemblyManipulator.Physical.AssemblyHashAlgorithm.SHA1,
               hash,
               rParams.CreateCAMParameters(),
               null
               );
         }
         Assert.IsTrue( ArrayEqualityComparer<Byte>.ArrayEquality( nativeSignature, camSignature ) );

      }
   }
}
