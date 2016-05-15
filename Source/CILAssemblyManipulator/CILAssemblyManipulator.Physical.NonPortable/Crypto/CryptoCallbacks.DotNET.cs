﻿/*
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
#if !CAM_PHYSICAL_IS_PORTABLE
extern alias CAMPhysical;

using CAMPhysical::CILAssemblyManipulator.Physical;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Cryptography;

namespace CILAssemblyManipulator.Physical.Crypto
{
   /// <summary>
   /// This class implements the <see cref="CryptoCallbacks"/> using standard .NET framework cryptographic services.
   /// </summary>
   /// <remarks>
   /// When creating <see cref="BlockDigestAlgorithm"/>, this class always tries the managed version first, then the <c>cng</c> version, and the <c>csp</c> version when creating the <see cref="System.Security.Cryptography.HashAlgorithm"/> transform.
   /// </remarks>
   public class CryptoCallbacksDotNET : AbstractCryptoCallbacks<System.Security.Cryptography.RSA>
   {
      private sealed class BlockHashAlgorithmWrapper : AbstractDisposable, BlockDigestAlgorithm
      {
         private System.Security.Cryptography.HashAlgorithm _algorithm;

         public BlockHashAlgorithmWrapper( System.Security.Cryptography.HashAlgorithm algorithm )
         {
            this._algorithm = algorithm;
         }

         protected override void Dispose( Boolean disposing )
         {
            if ( disposing )
            {
               this._algorithm.DisposeSafely();
            }
         }

         public void ProcessBlock( Byte[] data, Int32 offset, Int32 count )
         {
            this._algorithm.TransformBlock( data, offset, count, null, -1 );
         }

         public void WriteDigest( Byte[] array, Int32 offset )
         {
            this._algorithm.TransformFinalBlock( Empty<Byte>.Array, 0, 0 );
            var retVal = this._algorithm.Hash;
            Array.Copy( retVal, 0, array, offset, retVal.Length );
            // We have to explicitly initialize - TransformFinalBlock won't do this in .NET algorithms
            this._algorithm.Initialize();
         }

         public Int32 DigestByteCount
         {
            get
            {
               return this._algorithm.HashSize / 8;
            }
         }

      }

      private Boolean _canUseManagedCryptoAlgorithms;
      private Boolean _canUseCNGCryptoAlgorithms;

      /// <summary>
      /// Creates a new instance of <see cref="CryptoCallbacksDotNET"/>.
      /// </summary>
      public CryptoCallbacksDotNET()
      {
         this._canUseManagedCryptoAlgorithms = true;
         this._canUseCNGCryptoAlgorithms = true;
      }

      /// <inheritdoc />
      public override BlockDigestAlgorithm CreateHashAlgorithm( AssemblyHashAlgorithm algorithm )
      {
         System.Security.Cryptography.HashAlgorithm transform;
         switch ( algorithm )
         {
            case AssemblyHashAlgorithm.MD5:
               transform = GetTransform( null, () => new System.Security.Cryptography.MD5Cng(), () => new System.Security.Cryptography.MD5CryptoServiceProvider() );
               break;
            case AssemblyHashAlgorithm.SHA1:
               transform = GetTransform( () => new System.Security.Cryptography.SHA1Managed(), () => new System.Security.Cryptography.SHA1Cng(), () => new System.Security.Cryptography.SHA1CryptoServiceProvider() );
               break;
            case AssemblyHashAlgorithm.SHA256:
               transform = GetTransform( () => new System.Security.Cryptography.SHA256Managed(), () => new System.Security.Cryptography.SHA256Cng(), () => new System.Security.Cryptography.SHA256CryptoServiceProvider() );
               break;
            case AssemblyHashAlgorithm.SHA384:
               transform = GetTransform( () => new System.Security.Cryptography.SHA384Managed(), () => new System.Security.Cryptography.SHA384Cng(), () => new System.Security.Cryptography.SHA384CryptoServiceProvider() );
               break;
            case AssemblyHashAlgorithm.SHA512:
               transform = GetTransform( () => new System.Security.Cryptography.SHA512Managed(), () => new System.Security.Cryptography.SHA512Cng(), () => new System.Security.Cryptography.SHA512CryptoServiceProvider() );
               break;
            case AssemblyHashAlgorithm.None:
               throw new InvalidOperationException( "Tried to create hash stream with no hash algorithm" );
            default:
               throw new ArgumentException( "Unknown hash algorithm: " + algorithm + "." );
         }

         return new BlockHashAlgorithmWrapper( transform );
      }

      /// <inheritdoc />
      protected override System.Security.Cryptography.RSA CreateRSAFromCSPContainer( String containerName )
      {
         var csp = new System.Security.Cryptography.CspParameters { Flags = System.Security.Cryptography.CspProviderFlags.UseMachineKeyStore };
         if ( containerName != null )
         {
            csp.KeyContainerName = containerName;
            csp.KeyNumber = 2;
         }
         return new System.Security.Cryptography.RSACryptoServiceProvider( csp );
      }

      /// <inheritdoc />
      protected override System.Security.Cryptography.RSA CreateRSAFromParameters( RSAParameters parameters )
      {
         System.Security.Cryptography.RSA result = null;
         var rParams = parameters.CreateDotNETParameters( reverse: true );

         try
         {
            result = System.Security.Cryptography.RSA.Create();
            result.ImportParameters( rParams );
         }
         catch ( System.Security.Cryptography.CryptographicException )
         {
            var success = false;
            try
            {
               // Try SP without key container name instead
               result = this.CreateRSAFromCSPContainer( null );
               result.ImportParameters( rParams );
               success = true;
            }
            catch
            {
               // Ignore
            }

            if ( !success )
            {
               throw;
            }
         }
         return result;
      }

      /// <inheritdoc />
      protected override Byte[] DoCreateRSASignature( AssemblyHashAlgorithm hashAlgorithm, Byte[] contentsHash, RSAParameters parameters, RSA rsa )
      {
         var formatter = new System.Security.Cryptography.RSAPKCS1SignatureFormatter( rsa );
         formatter.SetHashAlgorithm( GetAlgorithmName( hashAlgorithm ) );
         return formatter.CreateSignature( contentsHash );
      }

      private System.Security.Cryptography.HashAlgorithm GetTransform(
         Func<System.Security.Cryptography.HashAlgorithm> managedVersion,
         Func<System.Security.Cryptography.HashAlgorithm> cngVersion,
         Func<System.Security.Cryptography.HashAlgorithm> spVersion
         )
      {
         if ( this._canUseManagedCryptoAlgorithms && managedVersion != null )
         {
            try
            {
               return managedVersion();
            }
            catch
            {
               this._canUseManagedCryptoAlgorithms = false;
            }
         }
         if ( this._canUseCNGCryptoAlgorithms )
         {
            try
            {
               return cngVersion();
            }
            catch
            {
               this._canUseCNGCryptoAlgorithms = false;
            }
         }
         return spVersion();
      }

      /// <inheritdoc />
      public override Byte[] ExtractPublicKeyFromCSPContainer( String containerName )
      {
#if MONO
         throw new NotSupportedException("This is not supported on Mono framework.");
#else
         Byte[] pk;
         Int32 winError1, winError2;
         if ( !TryExportCSPPublicKey( containerName, 0, out pk, out winError1 ) // Try user-specific key first
            && !TryExportCSPPublicKey( containerName, 32u, out pk, out winError2 ) ) // Then try machine-specific key (32u = CRYPT_MACHINE_KEYSET )
         {
            throw new InvalidOperationException( "Error when using user keystore: " + GetWin32ErrorString( winError1 ) + "\nError when using machine keystore: " + GetWin32ErrorString( winError2 ) );
         }
         return pk;
#endif
      }

      /// <summary>
      /// Returns the textual representation of the <paramref name="algorithm"/>. In non-portable environment, this value can be used to set algorithm for strong name signature creation.
      /// </summary>
      /// <param name="algorithm">The algorithm.</param>
      /// <returns>The textual representation of the <paramref name="algorithm"/>.</returns>
      /// <exception cref="ArgumentException">If <paramref name="algorithm"/> is not one of the ones specified in <see cref="AssemblyHashAlgorithm"/> enumeration.</exception>
      private static String GetAlgorithmName( AssemblyHashAlgorithm algorithm )
      {
         switch ( algorithm )
         {
            case AssemblyHashAlgorithm.None:
               return null;
            case AssemblyHashAlgorithm.MD5:
               return "MD5";
            case AssemblyHashAlgorithm.SHA1:
               return "SHA1";
            case AssemblyHashAlgorithm.SHA256:
               return "SHA256";
            case AssemblyHashAlgorithm.SHA384:
               return "SHA384";
            case AssemblyHashAlgorithm.SHA512:
               return "SHA512";
            default:
               throw new ArgumentException( "Unknown algorithm: " + algorithm );
         }
      }

#if !MONO
      private static String GetWin32ErrorString( Int32 errorCode )
      {
         return new System.ComponentModel.Win32Exception( errorCode ).Message + " ( Win32 error code: 0x" + Convert.ToString( errorCode, 16 ) + ").";
      }

      // Much of this code is adapted by disassembling KeyPal.exe, available at http://www.jensign.com/KeyPal/index.html
      private static Boolean TryExportCSPPublicKey( String cspName, UInt32 keyType, out Byte[] pk, out Int32 winError )
      {
         var dwProvType = 1u;
         var ctxPtr = IntPtr.Zero;
         winError = 0;
         pk = null;
         var retVal = false;
         try
         {
            if ( Win32.CryptAcquireContext( out ctxPtr, cspName, "Microsoft Base Cryptographic Provider v1.0", dwProvType, keyType )
               || Win32.CryptAcquireContext( out ctxPtr, cspName, "Microsoft Strong Cryptographic Provider", dwProvType, keyType )
               || Win32.CryptAcquireContext( out ctxPtr, cspName, "Microsoft Enhanced Cryptographic Provider v1.0", dwProvType, keyType ) )
            {
               IntPtr keyPtr = IntPtr.Zero;
               try
               {
                  if ( Win32.CryptGetUserKey( ctxPtr, 2u, out keyPtr ) ) // 2 = AT_SIGNATURE
                  {
                     IntPtr expKeyPtr = IntPtr.Zero; // When exporting public key, this is zero
                     var arraySize = 0u;
                     if ( Win32.CryptExportKey( keyPtr, expKeyPtr, 6u, 0u, null, ref arraySize ) ) // 6 = PublicKey
                     {
                        pk = new Byte[arraySize];
                        if ( Win32.CryptExportKey( keyPtr, expKeyPtr, 6u, 0u, pk, ref arraySize ) )
                        {
                           retVal = true;
                        }
                        else
                        {
                           winError = Marshal.GetLastWin32Error();
                        }
                     }
                     else
                     {
                        winError = Marshal.GetLastWin32Error();
                     }
                  }
                  else
                  {
                     winError = Marshal.GetLastWin32Error();
                  }
               }
               finally
               {
                  if ( keyPtr != IntPtr.Zero )
                  {
                     Win32.CryptDestroyKey( keyPtr );
                  }
               }
            }
            else
            {
               winError = Marshal.GetLastWin32Error();
            }
         }
         finally
         {
            if ( ctxPtr != IntPtr.Zero )
            {
               Win32.CryptReleaseContext( ctxPtr, 0u );
            }
         }
         return retVal;
      }

      private static void ThrowFromLastWin32Error()
      {
         throw new System.ComponentModel.Win32Exception( Marshal.GetLastWin32Error() );
      }

      private static class Win32
      {
         // http://msdn.microsoft.com/en-us/library/windows/desktop/aa379886%28v=vs.85%29.aspx
         [DllImport( "advapi32.dll", CharSet = CharSet.Auto, SetLastError = true )]
         internal static extern Boolean CryptAcquireContext(
            [Out] out IntPtr hProv,
            [In, System.Runtime.InteropServices.MarshalAs( System.Runtime.InteropServices.UnmanagedType.LPWStr )] String pszContainer,
            [In, System.Runtime.InteropServices.MarshalAs( System.Runtime.InteropServices.UnmanagedType.LPWStr )] String pszProvider,
            [In] UInt32 dwProvType,
            [In] UInt32 dwFlags );

         // http://msdn.microsoft.com/en-us/library/windows/desktop/aa380268%28v=vs.85%29.aspx
         [DllImport( "advapi32.dll" )]
         internal static extern Boolean CryptReleaseContext(
            [In] IntPtr hProv,
            [In] UInt32 dwFlags
            );

         // http://msdn.microsoft.com/en-us/library/windows/desktop/aa380199%28v=vs.85%29.aspx
         [DllImport( "advapi32.dll" )]
         internal static extern Boolean CryptGetUserKey(
            [In] IntPtr hProv,
            [In] UInt32 dwKeySpec,
            [Out] out IntPtr hKey
            );

         // http://msdn.microsoft.com/en-us/library/windows/desktop/aa379918%28v=vs.85%29.aspx
         [DllImport( "advapi32.dll" )]
         internal static extern Boolean CryptDestroyKey( [In] IntPtr hKey );

         // http://msdn.microsoft.com/en-us/library/windows/desktop/aa379931%28v=vs.85%29.aspx
         [DllImport( "advapi32.dll", SetLastError = true )]
         internal static extern Boolean CryptExportKey(
            [In] IntPtr hKey,
            [In] IntPtr hExpKey,
            [In] UInt32 dwBlobType,
            [In] UInt32 dwFlags,
            [In] Byte[] pbData,
            [In, Out] ref UInt32 dwDataLen );
      }

#endif

   }
}

#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{
   /// <summary>
   /// Helper method to create a new instance of CAM <see cref="CILAssemblyManipulator.Physical.Crypto.RSAParameters"/> from .NET <see cref="RSAParameters"/>.
   /// </summary>
   /// <param name="dotNetParams">The .NET <see cref="RSAParameters"/>.</param>
   /// <param name="reverse">Whether to reverse (change endianness) the arrays of the resulting <see cref="CILAssemblyManipulator.Physical.Crypto.RSAParameters"/>.</param>
   /// <returns>The new instance of CAM <see cref="CILAssemblyManipulator.Physical.Crypto.RSAParameters"/>.</returns>
   /// <remarks>
   /// Since some of the .NET cryptorgraphic API methods expect <see cref="RSAParameters"/> to be LE format (e.g. <see cref="RSA.ImportParameters"/>) and CAM <see cref="CILAssemblyManipulator.Physical.Crypto.RSAParameters"/> expects them to be in BE format (as they are stored in key BLOB), the byte arrays can be reversed.
   /// </remarks>
   public static CILAssemblyManipulator.Physical.Crypto.RSAParameters CreateCAMParameters( this RSAParameters dotNetParams, Boolean reverse )
   {
      var retVal = new CILAssemblyManipulator.Physical.Crypto.RSAParameters()
      {
         D = dotNetParams.D.CreateBlockCopy(),
         DP = dotNetParams.DP.CreateBlockCopy(),
         DQ = dotNetParams.DQ.CreateBlockCopy(),
         Exponent = dotNetParams.Exponent.CreateBlockCopy(),
         InverseQ = dotNetParams.InverseQ.CreateBlockCopy(),
         Modulus = dotNetParams.Modulus.CreateBlockCopy(),
         P = dotNetParams.P.CreateBlockCopy(),
         Q = dotNetParams.Q.CreateBlockCopy()
      };

      // The .NET RSAParameters are in LE format, but the CAM.Physical RSAParameters just reads them from key BLOB, where they are in BE format.
      // So reverse them here
      if ( reverse )
      {
         Array.Reverse( retVal.D );
         Array.Reverse( retVal.DP );
         Array.Reverse( retVal.DQ );
         Array.Reverse( retVal.Exponent );
         Array.Reverse( retVal.InverseQ );
         Array.Reverse( retVal.Modulus );
         Array.Reverse( retVal.P );
         Array.Reverse( retVal.Q );
      }

      return retVal;
   }

   /// <summary>
   /// Helper method to create a new instance of .NET <see cref="RSAParameters"/> from CAM <see cref="CILAssemblyManipulator.Physical.Crypto.RSAParameters"/>.
   /// </summary>
   /// <param name="camParams">The CAM <see cref="CILAssemblyManipulator.Physical.Crypto.RSAParameters"/>.</param>
   /// <param name="reverse">Whether to reverse (change endianness) the byte arrays of the resulting <see cref="RSAParameters"/>.</param>
   /// <returns>The new instance of CAM <see cref="RSAParameters"/>.</returns>
   /// <remarks>
   /// Since some of the .NET cryptorgraphic API methods expect <see cref="RSAParameters"/> to be LE format (e.g. <see cref="RSA.ImportParameters"/>) and CAM <see cref="CILAssemblyManipulator.Physical.Crypto.RSAParameters"/> expects them to be in BE format (as they are stored in key BLOB), the byte arrays can be reversed.
   /// </remarks>
   public static RSAParameters CreateDotNETParameters( this CILAssemblyManipulator.Physical.Crypto.RSAParameters camParams, Boolean reverse )
   {
      var retVal = new System.Security.Cryptography.RSAParameters()
      {
         D = camParams.D.CreateBlockCopy(),
         DP = camParams.DP.CreateBlockCopy(),
         DQ = camParams.DQ.CreateBlockCopy(),
         Exponent = camParams.Exponent.CreateBlockCopy(),
         InverseQ = camParams.InverseQ.CreateBlockCopy(),
         Modulus = camParams.Modulus.CreateBlockCopy(),
         P = camParams.P.CreateBlockCopy(),
         Q = camParams.Q.CreateBlockCopy()
      };

      // The .NET RSAParameters are in LE format, but the CAM.Physical RSAParameters just reads them from key BLOB, where they are in BE format.
      // So reverse them here
      if ( reverse )
      {
         Array.Reverse( retVal.D );
         Array.Reverse( retVal.DP );
         Array.Reverse( retVal.DQ );
         Array.Reverse( retVal.Exponent );
         Array.Reverse( retVal.InverseQ );
         Array.Reverse( retVal.Modulus );
         Array.Reverse( retVal.P );
         Array.Reverse( retVal.Q );
      }

      return retVal;
   }
}

#endif