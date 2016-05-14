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

      private static RSAParameters RSAParams = new RSAParameters()
      {
         P = new Byte[] { 0xc9, 0x0a, 0xa8, 0x11, 0xee, 0xc7, 0xdc, 0x90, 0x7d, 0x01, 0xef, 0x2b, 0x3d, 0x50, 0x5d, 0xe2, 0x98, 0x8c, 0x4b, 0xee, 0x89, 0xd1, 0xf9, 0x8a, 0xa6, 0xbc, 0xdd, 0x1a, 0x5d, 0x7b, 0x5c, 0xfe, 0x6d, 0x00, 0xd4, 0x7f, 0x46, 0xcb, 0xb9, 0x69, 0xd1, 0x8b, 0x57, 0x54, 0x75, 0xf6, 0x17, 0xd2, 0x1f, 0x6f, 0x9b, 0x76, 0x77, 0x69, 0x90, 0x24, 0xf9, 0x57, 0xbe, 0x59, 0x02, 0xec, 0x49, 0xc9 },
         Q = new Byte[] { 0xe3, 0x8c, 0x35, 0x65, 0xdd, 0x4c, 0xfe, 0xe0, 0xf1, 0x5a, 0xb1, 0xd9, 0x47, 0x66, 0x7f, 0x6f, 0xbf, 0x1e, 0x16, 0x7c, 0x86, 0x8a, 0xa7, 0x3d, 0x69, 0x57, 0x59, 0xa8, 0x4f, 0x9a, 0x3e, 0x23, 0x86, 0xae, 0x58, 0x01, 0x38, 0x5b, 0xab, 0x12, 0x91, 0x0e, 0xd0, 0xe8, 0x3f, 0x15, 0x10, 0x5a, 0x56, 0x20, 0xa8, 0x6d, 0x54, 0xa7, 0xb8, 0x7e, 0x5e, 0xb7, 0xa6, 0x2a, 0x8c, 0xdc, 0x3c, 0xc5 },
         D = new Byte[] { 0x22, 0x7c, 0x76, 0xb5, 0xc3, 0xe5, 0x69, 0x6c, 0xfd, 0x3b, 0x7e, 0xf4, 0x3d, 0x01, 0x33, 0x26, 0xe2, 0x60, 0x75, 0x6c, 0xaf, 0xd0, 0xb1, 0xc2, 0xd6, 0xb6, 0xc1, 0x9f, 0x12, 0xee, 0xef, 0x21, 0x96, 0x9b, 0x2c, 0x19, 0x1b, 0x25, 0x6e, 0x91, 0x00, 0x44, 0x6f, 0x0c, 0x33, 0xd5, 0x15, 0xe6, 0x84, 0x68, 0xa2, 0x12, 0x48, 0x67, 0x52, 0x7c, 0xd8, 0xfc, 0x0d, 0x91, 0x76, 0xed, 0xd5, 0x5e, 0x81, 0x8e, 0x54, 0x4a, 0xf3, 0x52, 0x60, 0x08, 0xc6, 0x77, 0xde, 0x6e, 0x9d, 0x49, 0xcf, 0x58, 0x1d, 0xaf, 0x67, 0x53, 0x1b, 0x18, 0x39, 0x4a, 0x6b, 0x84, 0x1c, 0x34, 0x1a, 0x53, 0x9f, 0x56, 0xf5, 0x67, 0x84, 0x30, 0x89, 0x03, 0xe1, 0xec, 0x9b, 0x69, 0x09, 0xc3, 0x4a, 0x10, 0xa5, 0x14, 0x59, 0xfc, 0xe2, 0xbc, 0xbd, 0x14, 0xea, 0xf7, 0x13, 0x24, 0x62, 0x2c, 0x75, 0x73, 0x1d, 0xb9 },
         DP = new Byte[] { 0x4c, 0x4b, 0x5e, 0x03, 0x08, 0x32, 0x12, 0xd3, 0x46, 0x8d, 0x80, 0x5d, 0x51, 0x74, 0x79, 0x5c, 0xaf, 0xf5, 0xb6, 0x2f, 0x3d, 0x60, 0x51, 0x2a, 0x3c, 0x22, 0xba, 0x69, 0xf2, 0x06, 0x0a, 0x01, 0x88, 0x0e, 0x63, 0x96, 0x35, 0xa0, 0xc4, 0xa9, 0x92, 0xdb, 0x25, 0x76, 0x29, 0x1a, 0x0e, 0x6a, 0x30, 0x81, 0xe3, 0x66, 0xae, 0xe4, 0x81, 0xce, 0x76, 0x4d, 0xc5, 0x2f, 0xf4, 0x7b, 0x05, 0x79 },
         DQ = new Byte[] { 0x8d, 0xab, 0x86, 0xb7, 0x64, 0x20, 0x02, 0xf4, 0x43, 0xf0, 0x66, 0x98, 0x53, 0xc6, 0xf2, 0x02, 0xbd, 0xe7, 0xda, 0xb2, 0x2f, 0x05, 0xf6, 0x77, 0xda, 0xb5, 0x22, 0xc2, 0x12, 0xc5, 0x82, 0x78, 0x95, 0xea, 0xc8, 0x2a, 0x02, 0x4f, 0xb8, 0x63, 0xf7, 0xe2, 0x54, 0x98, 0xb4, 0x65, 0xc5, 0xe7, 0xa8, 0x85, 0xee, 0xb7, 0x1b, 0x24, 0xcd, 0x4e, 0x08, 0x64, 0xa8, 0xd5, 0x07, 0x1c, 0x3b, 0xcd },
         InverseQ = new Byte[] { 0x99, 0x9b, 0xe6, 0xba, 0xe9, 0xa2, 0xfa, 0x89, 0x0c, 0xff, 0x36, 0xa9, 0xfc, 0xae, 0x18, 0xa5, 0xf0, 0xd5, 0xa5, 0x60, 0x0d, 0x91, 0x51, 0x81, 0x14, 0xd8, 0xba, 0x69, 0x66, 0x94, 0xe8, 0x81, 0x16, 0xee, 0xe5, 0x50, 0x9b, 0x2c, 0xbd, 0x36, 0x29, 0x78, 0x08, 0x11, 0xf7, 0xc4, 0x92, 0x23, 0x5a, 0x23, 0x55, 0x22, 0xfb, 0x29, 0x14, 0x10, 0xc8, 0x87, 0xa9, 0xc5, 0xdd, 0xad, 0xf2, 0xbd },
         Modulus = new Byte[] { 0xb2, 0xb2, 0x8e, 0xcb, 0x04, 0x54, 0x7d, 0xcd, 0xed, 0x75, 0x8c, 0x01, 0x46, 0x3a, 0xa6, 0x97, 0xdd, 0xf1, 0x16, 0xb2, 0xa8, 0xed, 0x4b, 0x45, 0x5e, 0x9f, 0x54, 0xf6, 0x52, 0x2a, 0xe1, 0x9f, 0xc7, 0xbf, 0x00, 0x3e, 0x53, 0x83, 0x9d, 0x60, 0x25, 0x30, 0xe9, 0xc0, 0xcb, 0xba, 0xa1, 0x28, 0xb1, 0xfa, 0xe0, 0xf1, 0xa4, 0xcc, 0x1b, 0x06, 0x5c, 0x2f, 0x3b, 0x9d, 0x4b, 0xc0, 0x82, 0xc4, 0xc8, 0x8d, 0xbb, 0xc4, 0x88, 0x3f, 0x01, 0x72, 0xb9, 0xff, 0xd7, 0xe5, 0x22, 0x70, 0x59, 0x32, 0xd9, 0x32, 0x58, 0xcb, 0x3f, 0x10, 0x5d, 0x16, 0xd5, 0xfe, 0x60, 0xea, 0x87, 0xe0, 0x57, 0x3b, 0x49, 0xc3, 0x3b, 0xfc, 0xb6, 0x9a, 0x0c, 0xd3, 0xf9, 0x43, 0x19, 0xa2, 0x8f, 0x6c, 0xc0, 0x2a, 0x9e, 0x62, 0x41, 0xe8, 0xa4, 0x21, 0x6f, 0xd2, 0xf2, 0x85, 0x83, 0x91, 0xf5, 0xdb, 0xe3, 0xad },
         Exponent = new Byte[] { 0x01, 0x00, 0x01 }
      };

      private static Byte[] ExpectedSignature = new Byte[] { 0x22, 0x28, 0x52, 0xef, 0x17, 0xbe, 0xb8, 0x49, 0x3d, 0xab, 0xf4, 0x6a, 0xdc, 0x73, 0x5d, 0x49, 0x94, 0xba, 0xae, 0x17, 0x54, 0x39, 0xdd, 0x9c, 0xce, 0xdb, 0xb7, 0x5e, 0xa4, 0x2f, 0xb8, 0xf0, 0x77, 0x06, 0xf5, 0xf0, 0x6b, 0x5a, 0xa1, 0x15, 0xe8, 0x2e, 0xd3, 0x30, 0x12, 0x90, 0x8f, 0x5b, 0x28, 0x87, 0x9c, 0xe8, 0xf9, 0xde, 0x8d, 0xc6, 0x97, 0xf1, 0xe9, 0x27, 0xf5, 0xc0, 0x4c, 0x36, 0x9e, 0x15, 0x6c, 0x0b, 0x35, 0xed, 0x70, 0x95, 0x85, 0x84, 0x2e, 0x2d, 0xd4, 0x88, 0x54, 0xcd, 0x54, 0x87, 0xa6, 0xf5, 0x65, 0xb7, 0x35, 0x6f, 0x61, 0x91, 0x62, 0x47, 0xa0, 0x0c, 0xb0, 0x49, 0x05, 0xca, 0xce, 0x75, 0x93, 0x46, 0x7d, 0xc1, 0xf6, 0xfd, 0xcb, 0x9c, 0x59, 0x89, 0x68, 0x57, 0x2d, 0x0a, 0x73, 0xc8, 0xc5, 0x55, 0xdb, 0x44, 0xde, 0x66, 0x77, 0xbf, 0x1e, 0x8c, 0x3c, 0x95 };

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
            () => System.Security.Cryptography.RSA.Create(),
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
         Func<System.Security.Cryptography.RSA> nativeAlgoFactory,
         Func<System.Security.Cryptography.AsymmetricSignatureFormatter> nativeFactory
         )
      {
         //var r = new Random();
         //var count = 1000 + ( r.NextInt32() % 1000 );
         //var bytez = r.NextBytes( count );

         var hash = // new SHA1_128().ComputeHash( bytez, 0, bytez.Length );
            new Byte[] { 0x59, 0xda, 0xd3, 0x8d, 0x43, 0x79, 0xa2, 0x18, 0x9b, 0x2c, 0x98, 0xd1, 0x5f, 0x08, 0x33, 0xb5, 0x5e, 0xdf, 0x58, 0x8c };

         var nativeFormatter = nativeFactory();
         nativeFormatter.SetHashAlgorithm( "SHA1" );


         Byte[] nativeSignature, nativeData;
         System.Security.Cryptography.RSAParameters rParams;
         using ( var nativeAlgo = nativeAlgoFactory() )
         {
            var dotNetParams = RSAParams.CreateDotNETParameters( false );
            nativeAlgo.ImportParameters( dotNetParams );
            //try
            //{
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
            //}
            //finally
            //{
            //   nativeAlgo.PersistKeyInCsp = false;
            //}
         }

         Assert.IsTrue( ArrayEqualityComparer<Byte>.ArrayEquality( nativeSignature, ExpectedSignature ) );

         var manual = ManualRSA_Native( nativeData );
         var manualSig = manual.ToByteArray();
         Array.Reverse( manualSig );
         Assert.IsTrue( ArrayEqualityComparer<Byte>.ArrayEquality( nativeSignature, manualSig ) );

         var manualCAM = ManualRSA_CAM( nativeData );

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

         //var creationString = "var rParams = new RSAParameters()\n{\n"
         //   + "P = new Byte[] { " + String.Join( ", ", rParams.P.Select( b => "0x" + b.ToString( "x2" ) ) ) + " },\n"
         //   + "Q = new Byte[] { " + String.Join( ", ", rParams.Q.Select( b => "0x" + b.ToString( "x2" ) ) ) + " },\n"
         //   + "D = new Byte[] { " + String.Join( ", ", rParams.D.Select( b => "0x" + b.ToString( "x2" ) ) ) + " },\n"
         //   + "DP = new Byte[] { " + String.Join( ", ", rParams.DP.Select( b => "0x" + b.ToString( "x2" ) ) ) + " },\n"
         //   + "DQ = new Byte[] { " + String.Join( ", ", rParams.DQ.Select( b => "0x" + b.ToString( "x2" ) ) ) + " },\n"
         //   + "InverseQ = new Byte[] { " + String.Join( ", ", rParams.InverseQ.Select( b => "0x" + b.ToString( "x2" ) ) ) + " },\n"
         //   + "Modulus = new Byte[] { " + String.Join( ", ", rParams.Modulus.Select( b => "0x" + b.ToString( "x2" ) ) ) + " },\n"
         //   + "Exponent = new Byte[] { " + String.Join( ", ", rParams.Exponent.Select( b => "0x" + b.ToString( "x2" ) ) ) + " }\n"
         //   + "\n};\n";
         //var hashString = "var sourceHash = new Byte[] { " + String.Join( ", ", hash.Select( b => "0x" + b.ToString( "x2" ) ) ) + "};";
         //var sigString = "var expectedSignature = new Byte[] { " + String.Join( ", ", nativeSignature.Select( b => "0x" + b.ToString( "x2" ) ) ) + "};";

         Byte[] camSignature;
         using ( var camCrypto = new DefaultCryptoCallbacks() )
         {
            // Don't use rParams.CreateCAMParameters() since the rsa.ExportParameters() already did the LE -> BE conversion
            camSignature = camCrypto.CreateRSASignature(
               CILAssemblyManipulator.Physical.AssemblyHashAlgorithm.SHA1,
               hash,
               new RSAParameters()
               {
                  D = rParams.D,
                  DP = rParams.DP,
                  DQ = rParams.DQ,
                  P = rParams.P,
                  Q = rParams.Q,
                  InverseQ = rParams.InverseQ,
                  Exponent = rParams.Exponent,
                  Modulus = rParams.Modulus
               },
               null
               );
         }
         Assert.IsTrue( ArrayEqualityComparer<Byte>.ArrayEquality( nativeSignature, camSignature ) );

      }

      private static System.Numerics.BigInteger ManualRSA_Native( Byte[] inputData )
      {
         var parameters = RSAParams;

         var p = new System.Numerics.BigInteger( parameters.P.Reverse().Concat( new Byte[] { 0 } ).ToArray() );
         var q = new System.Numerics.BigInteger( parameters.Q.Reverse().Concat( new Byte[] { 0 } ).ToArray() );
         var dp = new System.Numerics.BigInteger( parameters.DP.Reverse().Concat( new Byte[] { 0 } ).ToArray() );
         var dq = new System.Numerics.BigInteger( parameters.DQ.Reverse().Concat( new Byte[] { 0 } ).ToArray() );
         var iq = new System.Numerics.BigInteger( parameters.InverseQ.Reverse().Concat( new Byte[] { 0 } ).ToArray() );
         var input = new System.Numerics.BigInteger( inputData.Reverse().Concat( new Byte[] { 0 } ).ToArray() );
         System.Numerics.BigInteger retVal;

         //if ( p.HasValue && q.HasValue && dp.HasValue && dq.HasValue )
         //{
         // Decryption

         // mP = ((input Mod p) ^ dP)) Mod p
         var mp = ( input % p ).ModPow( dp, p );

         // mQ = ((input Mod q) ^ dQ)) Mod q
         var mq = ( input % q ).ModPow( dq, q );

         // h = qInv * (mP - mQ) Mod p
         var h = ( iq * ( mp - mq ) ).Remainder_Positive( p );

         // m = h * q + mQ
         retVal = ( h * q ) + mq;
         //}
         //else
         //{
         //   // Encryption

         //   // c = (i ^ e) Mod m
         //   retVal = input.ModPow( parameters.Exponent, parameters.Modulus );
         //}

         return retVal;
      }

      private static BigInteger ManualRSA_CAM( Byte[] inputData )
      {
         var parameters = RSAParams;

         var p = BigInteger.ParseFromBinary( parameters.P, BigInteger.BinaryEndianness.BigEndian, 1 );
         var q = BigInteger.ParseFromBinary( parameters.Q, BigInteger.BinaryEndianness.BigEndian, 1 );
         var dp = BigInteger.ParseFromBinary( parameters.DP, BigInteger.BinaryEndianness.BigEndian, 1 );
         var dq = BigInteger.ParseFromBinary( parameters.DQ, BigInteger.BinaryEndianness.BigEndian, 1 );
         var iq = BigInteger.ParseFromBinary( parameters.InverseQ, BigInteger.BinaryEndianness.BigEndian, 1 );
         var input = BigInteger.ParseFromBinary( inputData, BigInteger.BinaryEndianness.BigEndian, 1 );
         BigInteger retVal;

         //if ( p.HasValue && q.HasValue && dp.HasValue && dq.HasValue )
         //{
         // Decryption

         // mP = ((input Mod p) ^ dP)) Mod p
         var mp = ( input % p ).ModPow( dp, p );

         // mQ = ((input Mod q) ^ dQ)) Mod q
         var mq = ( input % q ).ModPow( dq, q );

         // h = qInv * (mP - mQ) Mod p
         var h = ( iq * ( mp - mq ) ).Remainder_Positive( p );

         // m = h * q + mQ
         retVal = ( h * q ) + mq;

         return retVal;
      }
   }
}

public static partial class E_Util
{
   public static System.Numerics.BigInteger ModPow( this System.Numerics.BigInteger value, System.Numerics.BigInteger exponent, System.Numerics.BigInteger modulus )
   {
      return System.Numerics.BigInteger.ModPow( value, exponent, modulus );
   }

   public static System.Numerics.BigInteger Remainder_Positive( this System.Numerics.BigInteger divident, System.Numerics.BigInteger divisor )
   {
      if ( divisor.Sign < 0 )
      {
         throw new ArgumentException( "Divisor must be positive." );
      }
      var retVal = divident % divisor;
      return retVal.Sign >= 0 ? retVal : ( retVal + divisor );
   }
}