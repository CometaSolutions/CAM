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
using System.IO;
using System.Linq;
using System.Text;
using CILAssemblyManipulator.Physical;
using CommonUtils;

namespace CILMerge
{
   public interface CILMergeOptions
   {
      int FileAlign { get; set; }
      bool AllowDuplicateResources { get; set; }
      System.Collections.Generic.ISet<string> AllowDuplicateTypes { get; set; }
      bool AllowMultipleAssemblyAttributes { get; set; }
      bool AllowWildCards { get; set; }
      string AttrSource { get; set; }
      bool Closed { get; set; }
      bool CopyAttributes { get; set; }
      bool DelaySign { get; set; }
      string ExcludeFile { get; set; }
      string[] InputAssemblies { get; set; }
      bool Internalize { get; set; }
      string KeyFile { get; set; }
      string CSPName { get; set; }
      CILAssemblyManipulator.Physical.AssemblyHashAlgorithm? SigningAlgorithm { get; set; }
      string[] LibPaths { get; set; }
      bool DoLogging { get; set; }
      //string LogFile { get; set; }
      bool NoDebug { get; set; }
      string OutPath { get; set; }
      bool Parallel { get; set; }
      CILAssemblyManipulator.Physical.ModuleKind? Target { get; set; }
      String MetadataVersionString { get; set; }
      string ReferenceAssembliesDirectory { get; set; }
      bool Union { get; set; }
      // TODO this option might be removed. Setting this to true would require loading all referenced assemblies of input modules (to find out their full public key) which don't have full public key in their refs.
      bool UseFullPublicKeyForRefs { get; set; }
      bool Verbose { get; set; }
      int VerBuild { get; set; }
      int VerMajor { get; set; }
      int VerMinor { get; set; }
      int VerRevision { get; set; }
      bool XmlDocs { get; set; }
      bool ZeroPEKind { get; set; }
      //CILMergeLogCallback CILLogCallback { get; set; }
      bool NoResources { get; set; }

      Int32 SubsystemMajor { get; set; }
      Int32 SubsystemMinor { get; set; }
      Boolean HighEntropyVA { get; set; }

      // When merging non-portable libraries with portable libraries, if non-portable library implements something that has retargetable ref in its signature -> PEVerify will think that the signatures are not matching
      Boolean SkipFixingAssemblyReferences { get; set; }
   }

   public interface CILMergeLogCallback
   {
      void Log( MessageLevel mLevel, String formatString, params Object[] args );
   }

   public abstract class AbstractCILMergeLogCallback : CILMergeLogCallback
   {

      public abstract void Log( MessageLevel mLevel, String formatString, Object[] args );

      protected static String CreateMessageString( MessageLevel mLevel, String formatString, Object[] args )
      {
         return String.Format( "{0}: {1}", mLevel, (Object) String.Format( formatString, args ) );
      }
   }

   public class ConsoleCILMergeLogCallback : StreamWriterCILMergeLogCallback
   {
      public static readonly ConsoleCILMergeLogCallback Instance = new ConsoleCILMergeLogCallback();

      private ConsoleCILMergeLogCallback()
         : base( Console.Out )
      {

      }
   }

   public class StreamWriterCILMergeLogCallback : AbstractCILMergeLogCallback
   {
      private readonly TextWriter _writer;

      public StreamWriterCILMergeLogCallback( TextWriter writer )
      {
         ArgumentValidator.ValidateNotNull( "Stream writer", writer );

         this._writer = writer;
      }

      public override void Log( MessageLevel mLevel, String formatString, Object[] args )
      {
         this._writer.WriteLine( CreateMessageString( mLevel, formatString, args ) );
      }
   }

   public class CILMergeOptionsImpl : CILMergeOptions
   {
      public const String MD_NET_1_0 = "v1.0.3705";
      public const String MD_NET_1_1 = "v1.1.4322";
      public const String MD_NET_2_0 = "v2.0.50727";
      public const String MD_NET_4_0 = "v4.0.30319";

      private String _excludeFile;

      public CILMergeOptionsImpl()
      {

      }

      public String OutPath { get; set; }
      public String KeyFile { get; set; }
      public String CSPName { get; set; }
      public AssemblyHashAlgorithm? SigningAlgorithm { get; set; }
      public Boolean DoLogging { get; set; }
      //public String LogFile { get; set; }
      public Int32 VerMajor { get; set; }
      public Int32 VerMinor { get; set; }
      public Int32 VerBuild { get; set; }
      public Int32 VerRevision { get; set; }
      public Boolean Union { get; set; }
      public Boolean NoDebug { get; set; }
      public Boolean CopyAttributes { get; set; }
      public String AttrSource { get; set; }
      public Boolean AllowMultipleAssemblyAttributes { get; set; }
      public ModuleKind? Target { get; set; }
      public String MetadataVersionString { get; set; }
      public String ReferenceAssembliesDirectory { get; set; }
      public Boolean XmlDocs { get; set; }
      public String[] LibPaths { get; set; }
      public Boolean Internalize { get; set; }
      public String ExcludeFile { get { return this._excludeFile; } set { this._excludeFile = value; if ( !String.IsNullOrEmpty( value ) ) { this.Internalize = true; } } }
      public Boolean DelaySign { get; set; }
      public Boolean UseFullPublicKeyForRefs { get; set; }
      public Int32 FileAlign { get; set; }
      public Boolean Closed { get; set; }
      public ISet<String> AllowDuplicateTypes { get; set; }
      public Boolean AllowDuplicateResources { get; set; }
      public Boolean ZeroPEKind { get; set; }
      public Boolean Parallel { get; set; }
      public Boolean Verbose { get; set; }
      public String[] InputAssemblies { get; set; }
      public Boolean AllowWildCards { get; set; }
      public Boolean NoResources { get; set; }
      //public CILMergeLogCallback CILLogCallback { get; set; }
      public Int32 SubsystemMajor { get; set; }
      public Int32 SubsystemMinor { get; set; }
      public Boolean HighEntropyVA { get; set; }
      public Boolean SkipFixingAssemblyReferences { get; set; }
   }
}
