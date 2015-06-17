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
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.DotNET
{

   public static class Verification
   {
      private static readonly Lazy<String> WinSDKBinPath = new Lazy<String>( () => GetWinSDKBinFolder(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );


      public static Boolean RunPEVerify(
         String winSDKBinDir,
         String fileName,
         Boolean verifyStrongName,
         out String peVerifyError,
         out String strongNameError
         )
      {
         peVerifyError = null;
         strongNameError = null;

         const String PEVERIFY_EXE = "PEVerify.exe";
         if ( String.IsNullOrEmpty( winSDKBinDir ) )
         {
            winSDKBinDir = WinSDKBinPath.Value;
         }

         var peVerifyPath = Path.Combine( winSDKBinDir, PEVERIFY_EXE );

         if ( File.Exists( peVerifyPath ) )
         {

            // Call PEVerify
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = peVerifyPath;
            // Ignore loading direct pointer to delegate ctors.
            startInfo.Arguments = "/IL /MD /VERBOSE /NOLOGO /HRESULT" + " \"" + fileName + "\"";
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = Path.GetDirectoryName( fileName );// validationPath;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            var process = Process.Start( startInfo );

            // First 'read to end', only then wait for exit.
            // Otherwise, might get stuck (forgot the link to StackOverflow which explained this).
            var results = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if ( !results.StartsWith( "All Classes and Methods in " + fileName + " Verified." ) )
            {
               peVerifyError = results;
            }

            if ( verifyStrongName )
            {
               startInfo.FileName = Path.Combine( winSDKBinDir, "sn.exe" );
               if ( File.Exists( startInfo.FileName ) )
               {
                  startInfo.Arguments = "-q -vf \"" + fileName + "\"";
                  process = Process.Start( startInfo );

                  results = process.StandardOutput.ReadToEnd();
                  process.WaitForExit();

                  if ( results != null && !String.IsNullOrEmpty( results.Trim() ) )
                  {
                     strongNameError = results;
                  }
               }
               else
               {
                  throw new Exception( "The strong name utility sn.exe is not in same path as " + PEVERIFY_EXE + "." );
               }
            }
         }
         else
         {
            throw new Exception( "PEVerify file \"" + peVerifyPath + "\" does not exist." );
         }

         return peVerifyError != null && strongNameError != null;
      }

      public static void RunPEVerify(
         String winSDKBinDir,
         String fileName,
         Boolean verifyStrongName
         )
      {
         String peVerifyError, snError;
         if ( RunPEVerify( winSDKBinDir, fileName, verifyStrongName, out peVerifyError, out snError ) )
         {
            var msg = peVerifyError;
            if ( String.IsNullOrEmpty( msg ) )
            {
               msg = snError;
            }
            else if ( !String.IsNullOrEmpty( snError ) )
            {
               msg = "\nIn addition, strong name validation failed: " + snError;
            }

            throw new VerificationException( msg );
         }
      }

      private static String GetWinSDKBinFolder()
      {
         String str;
         if ( !TryGetWinSDKBinFolder( out str ) )
         {
            throw new InvalidOperationException( "Failed to localte WinSDK bin path." );
         }

         return str;
      }

      private static Boolean TryGetWinSDKBinFolder( out String path )
      {
         using ( var baseKey = RegistryKey.OpenBaseKey( RegistryHive.LocalMachine, RegistryView.Registry32 ) )
         using ( var key = baseKey.OpenSubKey( @"SOFTWARE\Microsoft\Microsoft SDKs\Windows" ) )
         {
            path = null;
            foreach ( var subKeyName in key.GetSubKeyNames().OrderByDescending( s => s ) )
            {
               using ( var subKey = key.OpenSubKey( subKeyName ) )
               {
                  var names = subKey.GetSubKeyNames();
                  var suitablePath = names.FirstOrDefault( x => x.EndsWith( "-x64" ) );
                  if ( String.IsNullOrEmpty( suitablePath ) )
                  {
                     suitablePath = names.FirstOrDefault( x => x.EndsWith( "-x86" ) );
                     if ( String.IsNullOrEmpty( suitablePath ) )
                     {
                        suitablePath = names.FirstOrDefault();
                     }
                  }

                  if ( !String.IsNullOrEmpty( suitablePath ) )
                  {
                     using ( var dirInfoKey = subKey.OpenSubKey( suitablePath ) )
                     {
                        path = dirInfoKey.GetValue( "InstallationFolder", null ).ToStringSafe( null );
                     }
                  }

                  if ( path != null )
                  {
                     break;
                  }
               }
            }
         }

         return !String.IsNullOrEmpty( path );
      }

   }

   public class VerificationException : Exception
   {
      public VerificationException( String msg, Exception inner = null )
         : base( msg, inner )
      {

      }
   }
}
