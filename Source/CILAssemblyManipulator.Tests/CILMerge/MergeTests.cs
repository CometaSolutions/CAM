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
using CILMerge;
using Microsoft.Win32;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Tests.CILMerge
{
   public class MergeTests : AbstractCAMTest
   {
      private static readonly String WinSDKBinPath = GetWinSDKBinFolder();

      [Test]
      public void TestMergingCILMerge()
      {
         this.PerformTest( new CILMergeOptionsImpl()
         {
            OutPath = Path.Combine( Path.GetDirectoryName( CILMergeLocation ), "CILMergeMerged.dll" ),
            Closed = true,
            Union = true,
            InputAssemblies = new[] { CILMergeLocation },
            FileAlign = 0x200
         } );
      }

      private void PerformTest( CILMergeOptions options )
      {
         if ( !Path.IsPathRooted( options.OutPath ) )
         {
            options.OutPath = Path.GetFullPath( options.OutPath );
         }

         var outFile = options.OutPath;

         using ( var merger = new CILMerger( options ) )
         {
            merger.PerformMerge();
         }

         RunPEVerify( outFile, !String.IsNullOrEmpty( options.KeyFile ) );
      }

      private static void RunPEVerify( String fileName, Boolean verifyStrongName )
      {
         const String PEVERIFY_EXE = "PEVerify.exe";

         var peVerifyPath = Path.Combine( WinSDKBinPath, PEVERIFY_EXE );

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
            throw new Exception( "PEVerify detected the following errors in " + fileName + ":\n" + results );
         }

         if ( verifyStrongName )
         {
            startInfo.FileName = Path.Combine( WinSDKBinPath, "sn.exe" );
            if ( File.Exists( startInfo.FileName ) )
            {
               startInfo.Arguments = "-vf \"" + fileName + "\"";
               process = Process.Start( startInfo );

               results = process.StandardOutput.ReadToEnd();
               process.WaitForExit();

               var lines = results.Split( new Char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries );

               if ( !( lines.Length == 3 && String.Equals( lines[2], "Assembly '" + fileName + "' is valid" ) ) )
               {
                  throw new Exception( "Strong name validation detected the following errors in " + fileName + ":\n" + results );
               }

            }
            else
            {
               throw new Exception( "The strong name utility sn.exe is not in same path as " + PEVERIFY_EXE + "." );
            }
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

         return path != null;
      }
   }
}
