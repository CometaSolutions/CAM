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

namespace CILMerge
{
   public interface CILMergeOptions
   {
      int Align { get; set; }
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
      string LogFile { get; set; }
      bool NoDebug { get; set; }
      string OutPath { get; set; }
      bool Parallel { get; set; }
      CILAssemblyManipulator.Physical.ModuleKind? Target { get; set; }
      // TODO replace this with 'metadata version string' or similar property.
      //CILAssemblyManipulator.Physical.TargetRuntime? TargetPlatform { get; set; }
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
      CILMergeLogCallback CILLogCallback { get; set; }
      bool NoResources { get; set; }

      Int32 SubsystemMajor { get; set; }
      Int32 SubsystemMinor { get; set; }
      Boolean HighEntropyVA { get; set; }
   }

   public interface CILMergeLogCallback
   {
      void Log( MessageLevel mLevel, String formatString, params Object[] args );
   }

   public class CILMergeOptionsImpl : CILMergeOptions
   {
      private String _out;
      private String _keyFile;
      private AssemblyHashAlgorithm? _signingAlgorithm;
      private Boolean _log;
      private String _logFile;
      private Int32 _verMajor;
      private Int32 _verMinor;
      private Int32 _verBuild;
      private Int32 _verRevision;
      private Boolean _union;
      private Boolean _noDebug;
      private Boolean _copyAttributes;
      private String _attrSource;
      private Boolean _allowMultipleAssemblyAttributes;
      private ModuleKind? _target;
      //private TargetRuntime? _targetPlatform;
      private String _refAssDir;
      private Boolean _xmlDocs;
      private String[] _libPaths;
      private Boolean _internalize;
      private String _excludeFile;
      private Boolean _delaySign;
      private Boolean _useFullPublicKeyForRefs;
      private Int32 _align;
      private Boolean _closed;
      private ISet<String> _allowDuplicateTypes;
      private Boolean _allowDuplicateResources;
      private Boolean _zeroPEKind;
      private Boolean _parallel;
      private Boolean _verbose;
      private String[] _inputAssemblies;
      private Boolean _allowWildCards;
      private CILMergeLogCallback _logCallback;
      private Boolean _noResources;

      private Int32 _subsystemMajor;
      private Int32 _subsystemMinor;
      private Boolean _highEntropyVA;

      public CILMergeOptionsImpl()
      {

      }

      public String OutPath { get { return this._out; } set { this._out = value; } }
      public String KeyFile { get { return this._keyFile; } set { this._keyFile = value; } }
      public AssemblyHashAlgorithm? SigningAlgorithm { get { return this._signingAlgorithm; } set { this._signingAlgorithm = value; } }
      public Boolean DoLogging { get { return this._log; } set { this._log = value; } }
      public String LogFile { get { return this._logFile; } set { this._logFile = value; } }
      public Int32 VerMajor { get { return this._verMajor; } set { this._verMajor = value; } }
      public Int32 VerMinor { get { return this._verMinor; } set { this._verMinor = value; } }
      public Int32 VerBuild { get { return this._verBuild; } set { this._verBuild = value; } }
      public Int32 VerRevision { get { return this._verRevision; } set { this._verRevision = value; } }
      public Boolean Union { get { return this._union; } set { this._union = value; } }
      public Boolean NoDebug { get { return this._noDebug; } set { this._noDebug = value; } }
      public Boolean CopyAttributes { get { return this._copyAttributes; } set { this._copyAttributes = value; } }
      public String AttrSource { get { return this._attrSource; } set { this._attrSource = value; } }
      public Boolean AllowMultipleAssemblyAttributes { get { return this._allowMultipleAssemblyAttributes; } set { this._allowMultipleAssemblyAttributes = value; } }
      public ModuleKind? Target { get { return this._target; } set { this._target = value; } }
      //public TargetRuntime? TargetPlatform { get { return this._targetPlatform; } set { this._targetPlatform = value; } }
      public String ReferenceAssembliesDirectory { get { return this._refAssDir; } set { this._refAssDir = value; } }
      public Boolean XmlDocs { get { return this._xmlDocs; } set { this._xmlDocs = value; } }
      public String[] LibPaths { get { return this._libPaths; } set { this._libPaths = value; } }
      public Boolean Internalize { get { return this._internalize; } set { this._internalize = value; } }
      public String ExcludeFile { get { return this._excludeFile; } set { this._excludeFile = value; if ( !String.IsNullOrEmpty( value ) ) { this._internalize = true; } } }
      public Boolean DelaySign { get { return this._delaySign; } set { this._delaySign = value; } }
      public Boolean UseFullPublicKeyForRefs { get { return this._useFullPublicKeyForRefs; } set { this._useFullPublicKeyForRefs = value; } }
      public Int32 Align { get { return this._align; } set { this._align = value; } }
      public Boolean Closed { get { return this._closed; } set { this._closed = value; } }
      public ISet<String> AllowDuplicateTypes { get { return this._allowDuplicateTypes; } set { this._allowDuplicateTypes = value; } }
      public Boolean AllowDuplicateResources { get { return this._allowDuplicateResources; } set { this._allowDuplicateResources = value; } }
      public Boolean ZeroPEKind { get { return this._zeroPEKind; } set { this._zeroPEKind = value; } }
      public Boolean Parallel { get { return this._parallel; } set { this._parallel = value; } }
      public Boolean Verbose { get { return this._verbose; } set { this._verbose = value; } }
      public String[] InputAssemblies { get { return this._inputAssemblies; } set { this._inputAssemblies = value; } }
      public Boolean AllowWildCards { get { return this._allowWildCards; } set { this._allowWildCards = value; } }
      public Boolean NoResources { get { return this._noResources; } set { this._noResources = value; } }
      public CILMergeLogCallback CILLogCallback { get { return this._logCallback; } set { this._logCallback = value; } }
      public Int32 SubsystemMajor { get { return this._subsystemMajor; } set { this._subsystemMajor = value; } }
      public Int32 SubsystemMinor { get { return this._subsystemMinor; } set { this._subsystemMinor = value; } }
      public Boolean HighEntropyVA { get { return this._highEntropyVA; } set { this._highEntropyVA = value; } }
   }
}
