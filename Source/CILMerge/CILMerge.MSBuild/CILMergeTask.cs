/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using CILAssemblyManipulator.Physical;

namespace CILMerge.MSBuild
{
   public class CILMergeTask : Task, CILMergeOptions, CILMergeLogCallback
   {
      public CILMergeTask()
      {
         // Set versions to negative so CILMerge will copy version information from primary assembly
         this.VerMajor = -1;
         this.VerMinor = -1;
         this.VerBuild = -1;
         this.VerRevision = -1;

         // Enable logging by default
         this.DoLogging = true;

         // Default subsystem version
         this.SubsystemMajor = 4;
         this.SubsystemMinor = 0;
      }

      public override Boolean Execute()
      {
         Boolean retVal;
         var inputs = this.InputAssemblies;
         if ( inputs == null || inputs.Length <= 0 )
         {
            this.Log.LogError( "At least one input assembly is required." );
            retVal = false;
         }
         else
         {
            if ( String.IsNullOrEmpty( this.OutPath ) )
            {
               if ( !String.IsNullOrEmpty( this.OutDir ) )
               {
                  this.OutPath = System.IO.Path.GetFullPath( System.IO.Path.Combine( this.OutDir, System.IO.Path.GetFileName( inputs[0] ) ) );
               }
               else if ( String.IsNullOrEmpty( this.OutPath ) )
               {
                  this.OutPath = inputs[0];
               }
            }

            if ( this.LibPaths == null || this.LibPaths.Length == 0 )
            {
               this.LibPaths = new[] { this.OutDir ?? System.IO.Path.GetDirectoryName( inputs[0] ) };
            }
            else
            {
               ExpandPaths( this.LibPaths );
            }

            ExpandPaths( inputs );

            var verFile = this.VersionFile;
            retVal = String.IsNullOrEmpty( verFile );
            if ( !retVal )
            {
               try
               {
                  var version = Version.Parse( System.IO.File.ReadAllText( verFile ) );
                  this.VerMajor = version.Major;
                  this.VerMinor = version.Minor;
                  this.VerBuild = version.Build;
                  this.VerRevision = version.Revision;

                  retVal = true;
               }
               catch ( Exception e )
               {
                  this.Log.LogError( "Error when getting version information from {0}: {1}.", verFile, e.Message );
                  retVal = false;
               }
            }

            if ( retVal )
            {
               this.Log.LogMessage( MessageImportance.High, "Performing merge for assemblies {0}; with library paths {1}; and outputting to {2} (union: {3}, closed: {4}).", String.Join( ", ", this.InputAssemblies ), String.Join( ", ", this.LibPaths ), this.OutPath, this.Union, this.Closed );

               try
               {
                  new CILMerger( this, this ).PerformMerge();
                  var verify = this.VerifyOutput;
                  var outPath = this.OutPath;
                  if ( verify )
                  {
                     this.Log.LogMessage( MessageImportance.High, "Verifying {0}.", outPath );
                  }

                  String peVerifyError = null, snError = null;
                  retVal = !( this.VerifyOutput && Verification.RunPEVerify( null, outPath, this.OutputHasStrongName(), out peVerifyError, out snError ) );
                  if ( !retVal )
                  {
                     this.Log.LogError( "PEVerify and/or strong name validation failed." );
                     if ( !String.IsNullOrEmpty( peVerifyError ) )
                     {
                        this.Log.LogError( "PEVerify error:\n{0}", peVerifyError );
                     }
                     if ( !String.IsNullOrEmpty( snError ) )
                     {
                        this.Log.LogError( "Strong name error:\n{0}", snError );
                     }
                  }
               }
               catch ( Exception exc )
               {
                  this.Log.LogErrorFromException( exc, true, true, null );
                  retVal = false;
               }
            }
         }
         return retVal;
      }

      #region CILMergeOptions Members

      public Int32 FileAlign { get; set; }

      public Boolean AllowDuplicateResources { get; set; }

      // TODO ISet<String>, is it usable from .csproj file?
      public ISet<String> AllowDuplicateTypes { get; set; }

      public Boolean AllowMultipleAssemblyAttributes { get; set; }

      public Boolean AllowWildCards { get; set; }

      public String TargetAssemblyAttributeSource { get; set; }

      public Boolean Closed { get; set; }

      public Boolean CopyAttributes { get; set; }

      public Boolean DelaySign { get; set; }

      public String ExcludeFile { get; set; }

      [Required]
      public String[] InputAssemblies { get; set; }

      public Boolean Internalize { get; set; }

      public String KeyFile { get; set; }

      public AssemblyHashAlgorithm? SigningAlgorithm { get; set; }

      public String[] LibPaths { get; set; }

      public Boolean DoLogging { get; set; }

      public Boolean NoDebug { get; set; }

      public String OutPath { get; set; }

      public Boolean Parallel { get; set; }

      public ModuleKind? Target { get; set; }

      public String ReferenceAssembliesDirectory { get; set; }

      public Boolean Union { get; set; }

      public String UnionExcludeFile { get; set; }

      public Boolean UseFullPublicKeyForRefs { get; set; }

      public Boolean Verbose { get; set; }

      public Int32 VerBuild { get; set; }

      public Int32 VerMajor { get; set; }

      public Int32 VerMinor { get; set; }

      public Int32 VerRevision { get; set; }

      public Boolean XmlDocs { get; set; }

      public Boolean ZeroPEKind { get; set; }

      public Boolean NoResources { get; set; }

      public Int32 SubsystemMajor { get; set; }
      public Int32 SubsystemMinor { get; set; }
      public Boolean HighEntropyVA { get; set; }

      public String MetadataVersionString { get; set; }
      public Boolean SkipFixingAssemblyReferences { get; set; }
      public String CSPName { get; set; }
      public String TargetAssemblyName { get; set; }
      public String TargetModuleName { get; set; }

      #endregion

      #region CILMergeLogCallback Members

      void CILMergeLogCallback.Log( MessageLevel mLevel, string formatString, params object[] args )
      {
         switch ( mLevel )
         {
            case MessageLevel.Error:
               this.Log.LogError( formatString, args );
               break;
            case MessageLevel.Warning:
               this.Log.LogWarning( formatString, args );
               break;
            case MessageLevel.Info:
               this.Log.LogMessage( MessageImportance.Normal, formatString, args );
               break;
         }
      }

      #endregion

      public String OutDir { get; set; }

      public Boolean VerifyOutput { get; set; }

      public String VersionFile { get; set; }

      private static void ExpandPaths( String[] paths )
      {
         for ( var i = 0; i < paths.Length; ++i )
         {
            if ( paths[i] != null ) //&& System.IO.Path.IsPathRooted(paths[i]) )
            {
               try
               {
                  paths[i] = System.IO.Path.GetFullPath( paths[i] );
               }
               catch
               {
                  // Ignore, happens with wildcards
               }
            }
         }
      }
   }
}
