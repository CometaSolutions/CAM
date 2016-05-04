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

namespace CILAssemblyManipulator.Physical.Crypto
{
   /// <summary>
   /// This class implements the <see cref="CryptoCallbacks"/> using standard .NET framework cryptographic services.
   /// </summary>
   /// <remarks>
   /// When creating <see cref="HashStreamInfo"/>, this class always tries the managed version first, then the <c>cng</c> version, and the <c>csp</c> version when creating the <see cref="System.Security.Cryptography.HashAlgorithm"/> transform.
   /// </remarks>
   public class CryptoCallbacksDotNET : AbstractCryptoCallbacks
   {
      private sealed class BlockHashAlgorithmWrapper : AbstractDisposable, BlockHashAlgorithm
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

         public Byte[] ProcessFinalBlock( Byte[] data, Int32 offset, Int32 count )
         {
            this._algorithm.TransformFinalBlock( data ?? Empty<Byte>.Array, offset, count );
            var retVal = this._algorithm.Hash;
            // We have to explicitly initialize - TransformFinalBlock won't do this in .NET algorithms
            this._algorithm.Initialize();
            return retVal;
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
      public override BlockHashAlgorithm CreateHashAlgorithm( AssemblyHashAlgorithm algorithm )
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
      public override IDisposable CreateRSAFromCSPContainer( String containerName )
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
      public override IDisposable CreateRSAFromParameters( RSAParameters parameters )
      {
         System.Security.Cryptography.RSA result = null;
         var rParams = new System.Security.Cryptography.RSAParameters()
         {
            D = parameters.D,
            DP = parameters.DP,
            DQ = parameters.DQ,
            Exponent = parameters.Exponent,
            InverseQ = parameters.InverseQ,
            Modulus = parameters.Modulus,
            P = parameters.P,
            Q = parameters.Q
         };
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
               result = (System.Security.Cryptography.RSA) this.CreateRSAFromCSPContainer( null );
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
      public override Byte[] CreateRSASignature( IDisposable rsa, String hashAlgorithmName, Byte[] contentsHash )
      {
         var formatter = new System.Security.Cryptography.RSAPKCS1SignatureFormatter( (System.Security.Cryptography.AsymmetricAlgorithm) rsa );
         formatter.SetHashAlgorithm( hashAlgorithmName );
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
#endif