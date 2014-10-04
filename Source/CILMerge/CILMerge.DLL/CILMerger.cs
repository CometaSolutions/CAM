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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using CILAssemblyManipulator.API;
using CILAssemblyManipulator.DotNET;
using CILAssemblyManipulator.MResources;
using CILAssemblyManipulator.PDB;
using CommonUtils;

namespace CILMerge
{
   public class CILMerger : IDisposable
   {
      private readonly CILMergeOptions _options;
      private readonly Boolean _disposeLogStream;
      private readonly TextWriter _logStream;

      public CILMerger( CILMergeOptions options )
      {
         if ( options == null )
         {
            throw new ArgumentNullException( "Options" );
         }
         this._options = options;

         if ( this._options.DoLogging )
         {
            var logFile = this._options.LogFile;
            logFile = logFile == null ?
               null :
               Path.GetFullPath( logFile );
            try
            {
               this._disposeLogStream = !String.IsNullOrEmpty( logFile );
               this._logStream = this._disposeLogStream ?
                  new StreamWriter( logFile, false, Encoding.UTF8 ) :
                  Console.Out;
            }
            catch ( Exception exc )
            {
               throw this.NewCILMergeException( ExitCode.ErrorAccessingLogFile, "Error accessing log file " + logFile + ".", exc );
            }
         }
      }

      public void PerformMerge()
      {
         // Merge assemblies
         var assemblyMergeResult = DotNETReflectionContext.UseDotNETContext( ctx =>
         {
            using ( var assMerger = new CILAssemblyMerger( this, this._options, Environment.CurrentDirectory, ctx ) )
            {
               return assMerger.EmitTargetAssembly();
            }
         } );

         if ( this._options.XmlDocs )
         {
            // Merge documentation
            this.MergeXMLDocs( assemblyMergeResult );
         }
      }

      private void MergeXMLDocs( Tuple<String[], IDictionary<Tuple<String, String>, String>> assemblyMergeResult )
      {
         var xmlDocs = new ConcurrentDictionary<String, XDocument>();

         // TODO on *nix, comparison should maybe be case-sensitive.
         var outXmlPath = Path.ChangeExtension( this._options.OutPath, ".xml" );
         this.DoPotentiallyInParallel( assemblyMergeResult.Item1, ( isRunningInParallel, ass ) =>
         {
            XDocument xDoc = null;
            var xfn = Path.ChangeExtension( ass, ".xml" );
            if ( !String.Equals( Path.GetFullPath( xfn ), outXmlPath, StringComparison.OrdinalIgnoreCase ) )
            {
               try
               {
                  xDoc = XDocument.Load( xfn );
               }
               catch
               {
                  // Ignore
               }
            }
            if ( xDoc == null )
            {
               // Try load from lib paths
               xfn = Path.GetFileName( xfn );
               foreach ( var p in this._options.LibPaths )
               {
                  var curXfn = Path.Combine( p, xfn );
                  if ( !String.Equals( Path.GetFullPath( curXfn ), outXmlPath, StringComparison.OrdinalIgnoreCase ) )
                  {
                     try
                     {
                        xDoc = XDocument.Load( curXfn );
                        xfn = curXfn;
                     }
                     catch
                     {
                        // Ignore
                     }
                     if ( xDoc != null )
                     {
                        break;
                     }
                  }
               }
            }
            if ( xDoc != null )
            {
               xmlDocs.TryAdd( ass, xDoc );
            }
         } );


         var renameDic = assemblyMergeResult.Item2;
         using ( var fs = File.Open( outXmlPath, FileMode.Create, FileAccess.Write, FileShare.Read ) )
         {
            // Create and save document (Need to use XmlWriter if 4-space indent needs to be preserved, the XElement saves using 2-space indent and it is not customizable).
            new XElement( "doc",
               new XElement( "assembly", new XElement( "name", Path.GetFileNameWithoutExtension( this._options.OutPath ) ) ), // Assembly name
               new XElement( "members", xmlDocs.Select( kvp => Tuple.Create( kvp.Key, kvp.Value.XPathSelectElements( "/doc/members/*" ) ) )
                  .SelectMany( tuple =>
                  {
                     if ( renameDic != null )
                     {
                        foreach ( var el in tuple.Item2 )
                        {
                           var nameAttr = el.Attribute( "name" );
                           String typeName;
                           if ( nameAttr != null && nameAttr.Value.StartsWith( "T:" ) && renameDic.TryGetValue( Tuple.Create( tuple.Item1, el.Attribute( "name" ).Value.Substring( 2 ) ), out typeName ) )
                           {
                              // The name was changed during merge.
                              // TODO need to do same for members, etc.
                              nameAttr.SetValue( typeName );
                           }
                        }
                     }
                     return tuple.Item2;
                  } )
               )
            ).Save( fs );
         }
      }

      internal CILMergeException NewCILMergeException( ExitCode code, String message, Exception inner = null )
      {
         this.Log( MessageLevel.Error, message );
         return new CILMergeException( code, message, inner );
      }

      internal void Log( MessageLevel mLevel, String formatString, params Object[] args )
      {
         if ( this._logStream != null )
         {
            this._logStream.Write( mLevel.ToString().ToUpper() + ": " );
            this._logStream.WriteLine( formatString, (Object[]) args );
         }
         else if ( this._options.CILLogCallback != null )
         {
            this._options.CILLogCallback.Log( mLevel, formatString, args );
         }
      }

      internal void DoPotentiallyInParallel<T>( IEnumerable<T> enumerable, Action<Boolean, T> action )
      {
         if ( this._options.Parallel && enumerable.Skip( 1 ).Any() )
         {
            System.Threading.Tasks.Parallel.ForEach( enumerable, item => action( true, item ) );
         }
         else
         {
            foreach ( var item in enumerable )
            {
               action( false, item );
            }
         }
      }

      #region IDisposable Members

      public void Dispose()
      {
         if ( this._disposeLogStream && this._logStream != null )
         {
            this._logStream.Dispose();
         }
      }

      #endregion
   }

   public enum MessageLevel { Warning, Error, Info }

   public enum ExitCode
   {
      Success = 0,
      ExceptionDuringStartup,
      ExceptionDuringMerge,
      NoInputAssembly,
      DuplicateTypeName,
      FailedToProduceCorrectResult,
      DebugInfoNotEasilyMergeable,
      NonILOnlyModule,
      AttrFileSpecifiedButDoesNotExist,
      ErrorAccessingInputFile,
      ErrorAccessingReferencedAssembly,
      ErrorAccessingExcludeFile,
      ErrorAccessingTargetFile,
      ErrorAccessingSNFile,
      ErrorAccessingLogFile,
      PCL2TargetTypeFail,
      PCL2TargetMethodFail,
      PCL2TargetFieldFail,
      ErrorAccessingInputPDB,
      ErrorAccessingTargetPDB,
      ErrorReadingPDB,
      ErrorWritingPDB,
      UnresolvedAssemblyReference,
      NoTargetFrameworkMoniker,
      FailedToDeduceTargetFramework,
      FailedToReadTargetFrameworkMonikerInformation,
      FailedToMapPDBType
   }

   internal class CILAssemblyMerger : IDisposable
   {
      private class PortabilityHelper
      {
         private readonly String _referenceAssembliesPath;
         private readonly IDictionary<Tuple<String, String, String>, FrameworkMonikerInfo> _dic;
         private readonly CILAssemblyMerger _merger;
         private readonly IDictionary<FrameworkMonikerInfo, String> _explicitDirectories;

         internal PortabilityHelper( CILAssemblyMerger merger, String referenceAssembliesPath )
         {
            this._merger = merger;
            this._referenceAssembliesPath = Path.GetFullPath( referenceAssembliesPath ?? DotNETReflectionContext.GetDefaultReferenceAssemblyPath() );
            this._dic = new Dictionary<Tuple<String, String, String>, FrameworkMonikerInfo>();
            this._explicitDirectories = new Dictionary<FrameworkMonikerInfo, String>();
         }

         public FrameworkMonikerInfo this[EmittingArguments eArgs]
         {
            get
            {
               return this[eArgs.FrameworkName, eArgs.FrameworkVersion, eArgs.FrameworkProfile];
            }
         }

         public FrameworkMonikerInfo this[String fwName, String fwVersion, String fwProfile]
         {
            get
            {
               ArgumentValidator.ValidateNotNull( "Framework name", fwName );
               ArgumentValidator.ValidateNotNull( "Framework version", fwVersion );
               FrameworkMonikerInfo moniker;
               var key = Tuple.Create( fwName, fwVersion, fwProfile );
               if ( !this._dic.TryGetValue( key, out moniker ) )
               {
                  var dir = this.GetDirectory( fwName, fwVersion, fwProfile );
                  if ( !Directory.Exists( dir ) )
                  {
                     throw this._merger.NewCILMergeException( ExitCode.NoTargetFrameworkMoniker, "Couldn't find framework moniker info for framework \"" + fwName + "\", version \"" + fwVersion + "\"" + ( String.IsNullOrEmpty( fwProfile ) ? "" : ( ", profile \"" + fwProfile + "\"" ) ) + " (reference assembly path: " + this._referenceAssembliesPath + ")." );
                  }
                  else
                  {
                     var redistListDir = Path.Combine( dir, "RedistList" );
                     var fn = Path.Combine( redistListDir, "FrameworkList.xml" );
                     String msCorLibName; String fwDisplayName; String targetFWDir;
                     try
                     {
                        moniker = new FrameworkMonikerInfo( fwName, fwVersion, fwProfile, DotNETReflectionContext.ReadAssemblyInformationFromRedistXMLFile(
                                 fn,
                                 out msCorLibName,
                                 out fwDisplayName,
                                 out targetFWDir
                                 ), msCorLibName, fwDisplayName );
                        if ( !String.IsNullOrEmpty( targetFWDir ) )
                        {
                           this._explicitDirectories.Add( moniker, targetFWDir );
                        }
                     }
                     catch ( Exception exc )
                     {
                        throw this._merger.NewCILMergeException( ExitCode.FailedToReadTargetFrameworkMonikerInformation, "Failed to read FrameworkList.xml from " + fn + " (" + exc.Message + ").", exc );
                     }
                  }
               }
               return moniker;
            }
         }

         public String GetDirectory( EmittingArguments eArgs )
         {
            var fwInfo = this[eArgs];
            String retVal;
            return this._explicitDirectories.TryGetValue( fwInfo, out retVal ) ?
               retVal :
               this.GetDirectory( eArgs.FrameworkName, eArgs.FrameworkVersion, eArgs.FrameworkProfile );
         }

         private String GetDirectory( String fwName, String fwVersion, String fwProfile )
         {
            var retVal = Path.Combine( this._referenceAssembliesPath, fwName, fwVersion );
            if ( !String.IsNullOrEmpty( fwProfile ) )
            {
               retVal = Path.Combine( retVal, "Profile", fwProfile );
            }
            return retVal;
         }

         public Boolean TryGetFrameworkInfo( String dir, out String fwName, out String fwVersion, out String fwProfile )
         {
            dir = Path.GetFullPath( dir );
            fwName = null;
            fwVersion = null;
            fwProfile = null;
            var retVal = dir.StartsWith( this._referenceAssembliesPath ) && dir.Length > this._referenceAssembliesPath.Length;
            if ( retVal )
            {
               dir = dir.Substring( this._referenceAssembliesPath.Length );
               var dirs = dir.Split( new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries );
               retVal = dirs.Length >= 2;
               if ( retVal )
               {
                  fwName = dirs[0];
                  fwVersion = dirs[1];
                  fwProfile = dirs.Length >= 4 ? dirs[3] : null;
               }
            }
            else
            {
               // See if this framework is explicitly defined elsewhere
               var fwInfo = this._explicitDirectories.Where( kvp => String.Equals( dir, kvp.Value ) ).Select( kvp => kvp.Key ).FirstOrDefault();
               retVal = fwInfo != null;
               if ( retVal )
               {
                  fwName = fwInfo.FrameworkName;
                  fwVersion = fwInfo.FrameworkVersion;
                  fwProfile = fwInfo.ProfileName;
               }
            }
            return retVal;
         }

         public String ReferenceAssembliesPath
         {
            get
            {
               return this._referenceAssembliesPath;
            }
         }

      }

      private static readonly Type ATTRIBUTE_USAGE_TYPE = typeof( AttributeUsageAttribute );
      private static readonly System.Reflection.PropertyInfo ALLOW_MULTIPLE_PROP = typeof( AttributeUsageAttribute ).GetProperty( "AllowMultiple" );

      private readonly CILMerger _merger;
      private readonly ConcurrentDictionary<String, CILModule> _allModules;
      private readonly ConcurrentDictionary<CILModule, EmittingArguments> _loadingArgs;
      private readonly CILMergeOptions _options;
      private readonly CILReflectionContext _ctx;
      private readonly PortabilityHelper _portableHelper;
      private readonly ISet<String> _allInputTypeNames;
      private readonly ISet<String> _typesByName;
      private readonly IDictionary<CILTypeBase, CILTypeBase> _typeMapping;
      private readonly IDictionary<CILMethodBase, CILMethodBase> _methodMapping;
      private readonly IDictionary<CILField, CILField> _fieldMapping;
      private readonly IDictionary<CILEvent, CILEvent> _eventMapping;
      private readonly IDictionary<CILProperty, CILProperty> _propertyMapping;
      private readonly IList<CILModule> _inputModules;
      private readonly Lazy<Regex[]> _excludeRegexes;
      private readonly String _inputBasePath;
      //private readonly TextWriter _logStream;
      //private readonly Boolean _disposeLogStream;
      private readonly IDictionary<Tuple<String, String>, String> _typeRenames;
      private readonly IDictionary<String, CILAssemblyName> _assemblyNameCache;

      private CILModule _targetModule;
      private CILModule _primaryModule;
      private IDictionary<CILAssembly, CILAssembly> _pcl2TargetMapping;
      private PDBHelper _pdbHelper;
      private readonly Lazy<System.Security.Cryptography.SHA1CryptoServiceProvider> _csp;

      internal CILAssemblyMerger( CILMerger merger, CILMergeOptions options, String inputBasePath, CILReflectionContext ctx )
      {
         this._merger = merger;
         this._options = options;
         this._ctx = ctx;
         this._allModules = new ConcurrentDictionary<String, CILModule>();
         this._loadingArgs = new ConcurrentDictionary<CILModule, EmittingArguments>();
         this._typesByName = new HashSet<String>();
         this._typeMapping = new Dictionary<CILTypeBase, CILTypeBase>();
         this._methodMapping = new Dictionary<CILMethodBase, CILMethodBase>();
         this._fieldMapping = new Dictionary<CILField, CILField>();
         this._eventMapping = new Dictionary<CILEvent, CILEvent>();
         this._propertyMapping = new Dictionary<CILProperty, CILProperty>();

         this._portableHelper = new PortabilityHelper( this, this._options.ReferenceAssembliesDirectory );

         this._inputModules = new List<CILModule>();
         this._allInputTypeNames = options.Union ?
            null :
            new HashSet<String>();
         this._typeRenames = options.Union ?
            null :
            new Dictionary<Tuple<String, String>, String>();
         this._assemblyNameCache = new Dictionary<String, CILAssemblyName>();
         this._excludeRegexes = new Lazy<Regex[]>( () =>
         {
            var excl = options.ExcludeFile;
            if ( options.Internalize && !String.IsNullOrEmpty( excl ) )
            {
               try
               {
                  return File.ReadAllLines( excl ).Select( line => new Regex( line ) ).ToArray();
               }
               catch ( Exception exc )
               {
                  throw this.NewCILMergeException( ExitCode.ErrorAccessingExcludeFile, "Error accessing exclude file " + excl + ".", exc );
               }
            }
            else
            {
               return Empty<Regex>.Array;
            }
         }, LazyThreadSafetyMode.None );
         this._inputBasePath = inputBasePath ?? Environment.CurrentDirectory;
         if ( this._options.DoLogging )
         {
            // TODO
            //var logFile = this._options.LogFile;
            //logFile = logFile == null ?
            //   null :
            //   Path.GetFullPath( logFile );
            //try
            //{
            //   this._disposeLogStream = !String.IsNullOrEmpty( logFile );
            //   this._logStream = this._disposeLogStream ?
            //      new StreamWriter( logFile, false, Encoding.UTF8 ) :
            //      Console.Out;
            //}
            //catch ( Exception exc )
            //{
            //   throw this.NewCILMergeException( ExitCode.ErrorAccessingLogFile, "Error accessing log file " + logFile + ".", exc );
            //}
         }
         this._csp = new Lazy<System.Security.Cryptography.SHA1CryptoServiceProvider>( () => new System.Security.Cryptography.SHA1CryptoServiceProvider(), LazyThreadSafetyMode.ExecutionAndPublication );
      }

      internal Tuple<String[], IDictionary<Tuple<String, String>, String>> EmitTargetAssembly()
      {
         // TODO log all options here.

         this.LoadAllInputModules();

         // Save input emitting arguments for possible PDB emission later.
         var inputEArgs = this._loadingArgs.Values.ToArray();
         var inputFileNames = this._allModules.Keys.ToArray();

         // Create target module
         this._targetModule = this._ctx.NewBlankAssembly( Path.GetFileNameWithoutExtension( this._options.OutPath ) ).AddModule( Path.GetFileName( this._options.OutPath ) );

         // Get emitting arguments already at this stage - this will also set the correct AssociatedMSCorLib for the target module.
         var targetEArgs = this.GetEmittingArgumentsForTargetModule();

         if ( !this._options.Union )
         {
            // Have to generate set of all used type names in order for renaming to be stable.
            foreach ( var t in this._inputModules.SelectMany( m => m.DefinedTypes.SelectMany( t => t.AsDepthFirstEnumerable( tt => tt.DeclaredNestedTypes ) ) ) )
            {
               this._allInputTypeNames.Add( t.ToString() );
            }
         }

         var an = this._targetModule.Assembly.Name;
         an.Culture = this._primaryModule.Assembly.Name.Culture;
         if ( targetEArgs.StrongName != null )
         {
            an.PublicKey = targetEArgs.StrongName.KeyPair.ToArray();
         }
         if ( this._options.VerMajor > -1 )
         {
            // Version was specified explictly
            an.MajorVersion = this._options.VerMajor;
            an.MinorVersion = this._options.VerMinor;
            an.BuildNumber = this._options.VerBuild;
            an.Revision = this._options.VerRevision;
         }
         else
         {
            var an2 = this._primaryModule.Assembly.Name;
            an.MajorVersion = an2.MajorVersion;
            an.MinorVersion = an2.MinorVersion;
            an.BuildNumber = an2.BuildNumber;
            an.Revision = an2.Revision;
         }

         // Two sweeps - first create structure
         this.ProcessTargetModule( true );

         // Then add type references and IL
         this.ProcessTargetModule( false );

         // Then merge resources
         if ( !this._options.NoResources )
         {
            this.MergeResources();
         }

         // Remember process entry point
         targetEArgs.CLREntryPoint = this.ProcessMethodRef( targetEArgs.CLREntryPoint );

         // Emit the module
         try
         {
            using ( var fs = File.Open( this._options.OutPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None ) )
            {
               this._targetModule.EmitModule( fs, targetEArgs );
            }
         }
         catch ( Exception exc )
         {
            throw this.NewCILMergeException( ExitCode.ErrorAccessingTargetFile, "Error accessing target file " + this._options.OutPath + "(" + exc + ").", exc );
         }

         var badRefs = targetEArgs.AssemblyRefs.Where( aName => this._inputModules.Any( m => m.Assembly.Name.CorePropertiesEqual( aName ) ) ).ToArray();
         if ( badRefs.Length > 0 )
         {
            throw this.NewCILMergeException( ExitCode.FailedToProduceCorrectResult, "Internal error: the resulting assembly still references " + String.Join( ", ", (Object[]) badRefs ) + "." );
         }

         // Merge PDBs
         if ( this._pdbHelper != null )
         {
            this.MergePDBs( targetEArgs, inputEArgs );
         }

         return Tuple.Create( inputFileNames, this._typeRenames == null || this._typeRenames.Count == 0 ? null : this._typeRenames );
      }

      private void MapPDBScopeOrFunction( PDBScopeOrFunction scp, EmittingArguments targetEArgs, EmittingArguments eArg, CILMethodBase thisMethod )
      {
         foreach ( var slot in scp.Slots )
         {
            slot.TypeToken = this.MapTypeToken( targetEArgs, eArg, thisMethod, slot.TypeToken );
         }
         foreach ( var subScope in scp.Scopes )
         {
            this.MapPDBScopeOrFunction( subScope, targetEArgs, eArg, thisMethod );
         }
      }

      private UInt32 MapMethodToken( EmittingArguments targetEArgs, EmittingArguments inputEArgs, CILMethodBase thisMethod, UInt32 token )
      {
         return token == 0u ? 0u : (UInt32) targetEArgs.GetTokenFor( this._methodMapping[(CILMethodBase) inputEArgs.ResolveToken( thisMethod, (Int32) token )] ).Value;
      }

      private UInt32 MapTypeToken( EmittingArguments targetEArgs, EmittingArguments inputEArgs, CILMethodBase thisMethod, UInt32 token )
      {
         // Sometimes, there is a field signature in Standalone table. Reason for this is in ( http://msdn.developer-works.com/article/13221925/Pinned+Fields+in+the+Common+Language+Runtime , unavailable at 2014, viewable through goole cache http://webcache.googleusercontent.com/search?q=cache:UeFk80O2rGMJ:http://msdn.developer-works.com/article/13221925/Pinned%2BFields%2Bin%2Bthe%2BCommon%2BLanguage%2BRuntime%2Bstandalonesig+contains+field+signatures&oe=utf-8&rls=org.mozilla:en-US:official&client=firefox-a&channel=sb&gfe_rd=cr&hl=fi&ct=clnk&gws_rd=cr )
         // Shortly, the field signature seems to be generated for constant values within the method (sometimes) or when a field is used as by-ref parameter.
         // The ECMA standard, however, explicitly prohibits to have anything else except LOCALSIG and METHOD signatures in Standalone table.
         // The PDB, however, requires a token, which would represent the type of the slot (for some reason), and thus the faulty rows are being emitted to Standalone table.

         // This doesn't affect much at runtime, since these faulty Standalone rows are only used from within the PDB file, and never from within the CIL file.
         // However, we cannot emit such Standalone rows during emitting stage.
         // Therefore, detect this formally erroneus situation here, and mark that this slot should just use the normal standalone sig token of this method.
         // A bit hack-ish but oh well...
         return token == 0u ? 0u : (UInt32) targetEArgs.GetSignatureTokenFor( this._methodMapping[inputEArgs.ResolveSignatureToken( (Int32) token ).FirstOrDefault() ?? thisMethod] );
      }

      private EmittingArguments GetEmittingArgumentsForTargetModule()
      {
         // Prepare strong _name
         var keyFile = this._options.KeyFile;
         StrongNameKeyPair sn = null;
         if ( keyFile != null )
         {
            try
            {
               sn = new StrongNameKeyPair( File.ReadAllBytes( keyFile ) );
            }
            catch ( Exception exc )
            {
               throw this.NewCILMergeException( ExitCode.ErrorAccessingSNFile, "Error accessing strong name file " + keyFile + ".", exc );
            }
         }
         var pEArgs = this._loadingArgs[this._primaryModule];

         var fwInfo = this._portableHelper[pEArgs];

         this._targetModule.AssociatedMSCorLibModule = GetMainModuleOrFirst( this.LoadLibAssemblyFromPath( this._portableHelper.GetDirectory( pEArgs ), fwInfo.MsCorLibAssembly, false ) );
         var mKind = this._options.Target.HasValue ?
            this._options.Target.Value :
            pEArgs.ModuleKind;
         var eArgs = EmittingArguments.CreateForEmittingAnyModule(
            sn,
            pEArgs.Machine,
            this._options.TargetPlatform.HasValue ? this._options.TargetPlatform.Value : TargetRuntime.Net_4_0,
            mKind,
            mKind.IsDLL() ? null : pEArgs.CLREntryPoint
            );
         var msCorLibVersion = fwInfo.Assemblies[fwInfo.MsCorLibAssembly].Item1;
         eArgs.CorLibMajor = (UInt16) msCorLibVersion.Major;
         eArgs.CorLibMinor = (UInt16) msCorLibVersion.Minor;
         eArgs.CorLibBuild = (UInt16) msCorLibVersion.Build;
         eArgs.CorLibRevision = (UInt16) msCorLibVersion.Revision;

         if ( String.Equals( pEArgs.FrameworkName, FrameworkMonikerInfo.DEFAULT_PCL_FW_NAME ) )
         {
            eArgs.AssemblyRefProcessor += aRef =>
            {
               if ( fwInfo.Assemblies.ContainsKey( aRef.Name ) )
               {
                  aRef.Flags |= AssemblyFlags.Retargetable;
               }
            };
         }

         eArgs.FileAlignment = (UInt32) this._options.Align;
         eArgs.AssemblyRefProcessor += aName =>
         {
            if ( aName.Flags.IsFullPublicKey() && aName.PublicKey != null && !this._options.UseFullPublicKeyForRefs )
            {
               aName.PublicKey = this._ctx.ComputePublicKeyToken( aName.PublicKey );// this.GetPublicKeyToken( aName.PublicKey );
               aName.Flags &= ~AssemblyFlags.PublicKey;
            }
         };
         if ( !this._options.NoDebug && this._inputModules.Any( im => this._loadingArgs[im].DebugInformation != null ) )
         {
            this._pdbHelper = new PDBHelper( eArgs, this._options.OutPath );
         }
         eArgs.UseFullPublicKeyInAssemblyReferences = this._options.UseFullPublicKeyForRefs;
         eArgs.DelaySign = this._options.DelaySign;
         eArgs.SigningAlgorithm = this._options.SigningAlgorithm;
         eArgs.SubSysMajor = (UInt16) this._options.SubsystemMajor;
         eArgs.SubSysMinor = (UInt16) this._options.SubsystemMinor;
         eArgs.HighEntropyVA = this._options.HighEntropyVA;
         return eArgs;
      }

      private void ProcessTargetModule( Boolean creating )
      {
         // Don't do this in parallel - too much congestion.
         // Process primary module first in case of _name conflicts
         foreach ( var mod in this._inputModules )
         {
            this.ProcessModule( mod, creating );
         }

         if ( !creating )
         {
            this.ProcessTargetAssemblyAttributes();
         }
      }

      private void ProcessModule( CILModule inputModule, Boolean creating )
      {
         foreach ( var t in inputModule.DefinedTypes )
         {
            this.ProcessTypeDefinition( this._targetModule, t, creating );
         }
      }

      private void ProcessTypeDefinition( CILElementCapableOfDefiningType owner, CILType oldType, Boolean creating )
      {
         CILType newType;
         if ( creating )
         {
            var typeStr = oldType.ToString();
            var added = this._typesByName.Add( typeStr );
            var typeAttrs = this.GetNewTypeAttributesForType( owner, oldType, typeStr );
            var newName = oldType.Name;
            if ( added || this.IsDuplicateOK( ref newName, typeStr, typeAttrs ) )
            {
               newType = this.AddNewTypeToTarget( owner, oldType, typeAttrs, newName );
               if ( !added )
               {
                  // Rename occurred, save information
                  this._typeRenames.Add( Tuple.Create( oldType.Module.Assembly.Name.Name, typeStr ), newType.ToString() );
               }
            }
            else if ( this._options.Union )
            {
               newType = this._targetModule.GetTypeByName( typeStr );
               this._typeMapping.TryAdd( oldType, newType );
            }
            else
            {
               throw this.NewCILMergeException( ExitCode.DuplicateTypeName, "The type " + oldType + " appears in more than one assembly." );
            }
         }
         else
         {
            newType = (CILType) this._typeMapping[oldType];
         }

         if ( creating )
         {
            newType.Layout = oldType.Layout;
            newType.Namespace = oldType.Namespace;
         }
         else
         {
            this.ProcessCustomAttributes( newType, oldType );
            newType.AddDeclaredInterfaces( oldType.DeclaredInterfaces.Select( this.ProcessTypeRef ).ToArray() );
            newType.BaseType = this.ProcessTypeRef( oldType.BaseType );
            this.ProcessGenericParameters( newType, oldType );
            this.ProcessDeclSecurity( newType, oldType );
         }

         // Process type structure
         foreach ( var nt in oldType.DeclaredNestedTypes )
         {
            this.ProcessTypeDefinition( newType, nt, creating );
         }
         foreach ( var ctor in oldType.Constructors )
         {
            this.ProcessConstructor( newType, ctor, creating );
         }
         foreach ( var method in oldType.DeclaredMethods )
         {
            this.ProcessMethod( newType, method, creating );
         }
         foreach ( var field in oldType.DeclaredFields )
         {
            this.ProcessField( newType, field, creating );
         }
         foreach ( var prop in oldType.DeclaredProperties )
         {
            this.ProcessProperty( newType, prop, creating );
         }
         foreach ( var evt in oldType.DeclaredEvents )
         {
            this.ProcessEvent( newType, evt, creating );
         }
      }

      private void ProcessDeclSecurity( CILElementWithSecurityInformation newElem, CILElementWithSecurityInformation oldElem )
      {
         foreach ( var ds in oldElem.DeclarativeSecurity.Values.SelectMany( l => l ) )
         {
            var nds = newElem.AddDeclarativeSecurity( ds.SecurityAction, ds.SecurityAttributeType );
            foreach ( var na in ds.NamedArguments )
            {
               nds.NamedArguments.Add( this.ProcessCustomAttributeNamedArg( na ) );
            }
         }
      }

      private void ProcessGenericParameters<TGDef>( CILElementWithGenericArguments<TGDef> newElem, CILElementWithGenericArguments<TGDef> oldElem )
         where TGDef : class
      {
         foreach ( CILTypeParameter gp in oldElem.GenericArguments )
         {
            var newGP = (CILTypeParameter) newElem.GenericArguments[gp.GenericParameterPosition];
            newGP.AddGenericParameterConstraints( gp.GenericParameterConstraints.Select( c => this.ProcessTypeRef( c ) ).ToArray() );
            newGP.Attributes = gp.Attributes;
            this.ProcessCustomAttributes( newGP, gp );
         }
      }

      private TypeAttributes GetNewTypeAttributesForType( CILElementCapableOfDefiningType owner, CILType oldType, String typeString )
      {
         var attrs = oldType.Attributes;
         if ( this._options.Internalize && !this._primaryModule.Equals( oldType.Module ) && !this._excludeRegexes.Value.Any( reg => reg.IsMatch( typeString ) || reg.IsMatch( "[" + oldType.Module.Assembly.Name.Name + "]" + typeString ) ) )
         {
            // Have to make this type internal
            if ( owner is CILModule )
            {
               attrs &= ~TypeAttributes.VisibilityMask;
            }
            else if ( attrs.IsNestedPublic() )
            {
               attrs |= TypeAttributes.VisibilityMask;
               attrs &= ( ( ~TypeAttributes.VisibilityMask ) | TypeAttributes.NestedAssembly );
            }
            else if ( attrs.IsNestedFamily() || attrs.IsNestedFamilyORAssembly() )
            {
               attrs |= TypeAttributes.VisibilityMask;
               attrs &= ( ~TypeAttributes.VisibilityMask ) | TypeAttributes.NestedFamANDAssem;
            }
         }
         return attrs;
      }

      private CILType AddNewTypeToTarget( CILElementCapableOfDefiningType owner, CILType other, TypeAttributes attrs, String newName )
      {
         var t = owner.AddType( newName ?? other.Name, attrs, other.TypeCode );
         this._typeMapping.TryAdd( other, t );
         if ( other.GenericArguments.Any() )
         {
            foreach ( var gp in t.DefineGenericParameters( other.GenericArguments.Select( g => ( (CILTypeParameter) g ).Name ).ToArray() ) )
            {
               this._typeMapping.TryAdd( other.GenericArguments[gp.GenericParameterPosition], gp );
            }
         }
         return t;
      }

      private void ProcessConstructor( CILType newType, CILConstructor ctor, Boolean creating )
      {
         CILConstructor newCtor = creating ?
            newType.AddConstructor( ctor.Attributes, ctor.CallingConvention ) :
            (CILConstructor) this._methodMapping[ctor];
         this.ProcessMethodBase( newCtor, ctor, creating );
      }

      private void ProcessMethod( CILType newType, CILMethod method, Boolean creating )
      {
         CILMethod newMethod = creating ?
            newType.AddMethod( method.Name, method.Attributes, method.CallingConvention ) :
            (CILMethod) this._methodMapping[method];
         this.ProcessMethodBase( newMethod, method, creating );
         this.ProcessParameter( newMethod.ReturnParameter, method.ReturnParameter, creating );
         if ( creating )
         {
            newMethod.PlatformInvokeModuleName = method.PlatformInvokeModuleName;
            newMethod.PlatformInvokeName = method.PlatformInvokeName;
            newMethod.PlatformInvokeAttributes = method.PlatformInvokeAttributes;
            foreach ( var g in newMethod.DefineGenericParameters( method.GenericArguments.Select( g => ( (CILTypeParameter) g ).Name ).ToArray() ) )
            {
               this._typeMapping.TryAdd( method.GenericArguments[g.GenericParameterPosition], g );
            }
         }
         else
         {
            newMethod.AddOverriddenMethods( method.OverriddenMethods.Select( this.ProcessMethodRef ).ToArray() );
            this.ProcessGenericParameters( newMethod, method );
         }
      }

      private void ProcessMethodBase( CILMethodBase newMethod, CILMethodBase oldMethod, Boolean creating )
      {
         if ( creating )
         {
            this._methodMapping.TryAdd( oldMethod, newMethod );
            newMethod.ImplementationAttributes = oldMethod.ImplementationAttributes;
         }
         foreach ( var p in oldMethod.Parameters )
         {
            var newP = creating ?
               newMethod.AddParameter( p.Name, p.Attributes, null ) :
               newMethod.Parameters[p.Position];
            this.ProcessParameter( newP, p, creating );
         }
         if ( !creating )
         {
            this.ProcessCustomAttributes( newMethod, oldMethod );
            this.ProcessDeclSecurity( newMethod, oldMethod );

            if ( oldMethod.HasILMethodBody() )
            {
               var oldIL = oldMethod.MethodIL;
               var newIL = newMethod.MethodIL;
               // First, define labels
               var newLabels = newIL.DefineLabels( oldIL.LabelCount );

               // Then define locals
               foreach ( var local in oldIL.Locals )
               {
                  newIL.DeclareLocal( this.ProcessTypeRef( local.LocalType ), local.IsPinned );
               }
               // Then define exception blocks
               foreach ( var eBlock in oldIL.ExceptionBlocks )
               {
                  newIL.AddExceptionBlockInfo( new ExceptionBlockInfo(
                     eBlock.EndLabel,
                     eBlock.TryOffset,
                     eBlock.TryLength,
                     eBlock.HandlerOffset,
                     eBlock.HandlerLength,
                     this.ProcessTypeRef( eBlock.ExceptionType ),
                     eBlock.FilterOffset,
                     eBlock.BlockType ) );
               }
               // Then copy IL opcode infos
               foreach ( var info in oldIL.OpCodeInfos )
               {
                  switch ( info.InfoKind )
                  {
                     case OpCodeInfoKind.Branch:
                     case OpCodeInfoKind.BranchOrLeaveFixed:
                     case OpCodeInfoKind.Leave:
                     case OpCodeInfoKind.OperandInt32:
                     case OpCodeInfoKind.OperandInt64:
                     case OpCodeInfoKind.OperandNone:
                     case OpCodeInfoKind.OperandR4:
                     case OpCodeInfoKind.OperandR8:
                     case OpCodeInfoKind.OperandString:
                     case OpCodeInfoKind.OperandUInt16:
                     case OpCodeInfoKind.Switch:
                        // Just add the info - labels etc should remain the same.
                        newIL.Add( info );
                        break;
                     case OpCodeInfoKind.NormalOrVirtual:
                        var i1 = (OpCodeInfoForNormalOrVirtual) info;
                        newIL.Add( new OpCodeInfoForNormalOrVirtual( this.ProcessMethodRef( i1.ReflectionObject ), i1.NormalCode, i1.VirtualCode ) );
                        break;
                     case OpCodeInfoKind.OperandCtorToken:
                        var i2 = (OpCodeInfoWithCtorToken) info;
                        newIL.Add( new OpCodeInfoWithCtorToken( i2.Code, this.ProcessMethodRef( i2.ReflectionObject ), i2.UseGenericDefinitionIfPossible ) );
                        break;
                     case OpCodeInfoKind.OperandFieldToken:
                        var i3 = (OpCodeInfoWithFieldToken) info;
                        newIL.Add( new OpCodeInfoWithFieldToken( i3.Code, this.ProcessFieldRef( i3.ReflectionObject ), i3.UseGenericDefinitionIfPossible ) );
                        break;
                     case OpCodeInfoKind.OperandMethodSigToken:
                        var i4 = (OpCodeInfoWithMethodSig) info;
                        //Tuple<CILCustomModifier[], CILTypeBase>[] varArgs = null;
                        //if ( i4.VarArgs != null )
                        //{
                        //   varArgs = i4.VarArgs.Select( va => Tuple.Create( va.Item1.Select( cm => this.ProcessCustomModifier( cm ) ).ToArray(), this.ProcessTypeRef( va.Item2 ) ) ).ToArray();
                        //}
                        newIL.Add( new OpCodeInfoWithMethodSig( this.ProcessTypeRef( i4.ReflectionObject ), i4.VarArgs ) );
                        break;
                     case OpCodeInfoKind.OperandMethodToken:
                        var i5 = (OpCodeInfoWithMethodToken) info;
                        //var mRef = this.ProcessMethodRef( i5.ReflectionObject );
                        //OpCodeInfo opToAdd;
                        //if ( ( OpCodeEncoding.Call == i5.Code.Value || OpCodeEncoding.Ldftn == i5.Code.Value )
                        //     && this._pcl2TargetMapping != null
                        //     && this._loadingArgs[oldMethod.DeclaringType.Module].AssemblyRefs.FirstOrDefault( ar => ar.CorePropertiesEqual( i5.ReflectionObject.DeclaringType.Module.Assembly.Name ) && ar.Flags.IsRetargetable() ) != null
                        //   )
                        //{
                        //   // When mapping pcl to .NET, replace .Call with .Callvirt where applicable
                        //   opToAdd = OpCodeEncoding.Call == i5.Code.Value ? OpCodeInfoForNormalOrVirtual.OpCodeInfoForCall( mRef ) : OpCodeInfoForNormalOrVirtual.OpCodeInfoForLdFtn( mRef );
                        //}
                        //else
                        //{
                        //   opToAdd = new OpCodeInfoWithMethodToken( i5.Code, mRef, i5.UseGenericDefinitionIfPossible );
                        //}
                        // TODO proper detection of base.Method(); However, maybe someone actually wants to .Call a virtual method from outside base.Method() context?
                        // At the moment there is no code to distinguish reliably between base.Method(); and this.AnotherMethod();.
                        // Adding a OpCodeInfoForNormalOrVirtual results in possible stackoverflow exceptions caused by merged code.
                        // Add instruction as-is, causing some verification errors in resulting merged assembly when retargeting PCL to .NET (better than stackoverflow).
                        newIL.Add( new OpCodeInfoWithMethodToken( i5.Code, this.ProcessMethodRef( i5.ReflectionObject ), i5.UseGenericDefinitionIfPossible ) );
                        break;
                     case OpCodeInfoKind.OperandTypeToken:
                        var i6 = (OpCodeInfoWithTypeToken) info;
                        newIL.Add( new OpCodeInfoWithTypeToken( i6.Code, this.ProcessTypeRef( i6.ReflectionObject ), i6.UseGenericDefinitionIfPossible ) );
                        break;
                  }
                  if ( !this._options.NoDebug && info.InfoKind == OpCodeInfoKind.Branch || info.InfoKind == OpCodeInfoKind.Leave )
                  {
                     throw this.NewCILMergeException( ExitCode.DebugInfoNotEasilyMergeable, "Found dynamic branch opcode info, which would possibly change method IL size and line offsets." );
                  }
               }
               // Finally, mark labels
               var curLblOffset = 0;
               foreach ( var lblOffset in oldIL.LabelOffsets )
               {
                  newIL.MarkLabel( newLabels[curLblOffset], lblOffset );
                  ++curLblOffset;
               }
            }
         }
      }

      private void ProcessParameter( CILParameter newParameter, CILParameter oldParameter, Boolean creating )
      {
         if ( creating )
         {
            newParameter.Name = oldParameter.Name;
            if ( oldParameter.Attributes.HasDefault() )
            {
               newParameter.ConstantValue = oldParameter.ConstantValue;
            }
         }
         else
         {
            this.ProcessCustomAttributes( newParameter, oldParameter );
            this.ProcessCustomModifiers( newParameter, oldParameter );
            newParameter.ParameterType = this.ProcessTypeRef( oldParameter.ParameterType );
            this.ProcessMarshalInfo( newParameter, oldParameter );
         }
      }

      private void ProcessMarshalInfo( CILElementWithMarshalingInfo newElement, CILElementWithMarshalingInfo oldElement )
      {
         var mi = oldElement.MarshalingInformation;
         if ( mi != null )
         {
            newElement.MarshalingInformation = new MarshalingInfo( mi.Value, mi.SafeArrayType, this.ProcessTypeRef( mi.SafeArrayUserDefinedType ), mi.IIDParameterIndex, mi.ArrayType, mi.SizeParameterIndex, mi.ConstSize, this.ProcessTypeString( mi.MarshalType ), null, mi.MarshalCookie );
         }
      }

      private void ProcessField( CILType newType, CILField oldField, Boolean creating )
      {
         var newField = creating ?
            newType.AddField( oldField.Name, null, oldField.Attributes ) :
            this._fieldMapping[oldField];
         if ( creating )
         {
            this._fieldMapping.TryAdd( oldField, newField );
            if ( oldField.Attributes.HasDefault() )
            {
               newField.ConstantValue = oldField.ConstantValue;
            }
            newField.FieldOffset = oldField.FieldOffset;
            newField.InitialValue = oldField.InitialValue;
         }
         else
         {
            this.ProcessCustomAttributes( newField, oldField );
            this.ProcessCustomModifiers( newField, oldField );
            newField.FieldType = this.ProcessTypeRef( oldField.FieldType );
            this.ProcessMarshalInfo( newField, oldField );
         }
      }

      private void ProcessProperty( CILType newType, CILProperty oldProperty, Boolean creating )
      {
         var newProperty = creating ?
            newType.AddProperty( oldProperty.Name, oldProperty.Attributes ) :
            this._propertyMapping[oldProperty];
         if ( creating )
         {
            this._propertyMapping.TryAdd( oldProperty, newProperty );
            if ( oldProperty.Attributes.HasDefault() )
            {
               newProperty.ConstantValue = oldProperty.ConstantValue;
            }
         }
         else
         {
            this.ProcessCustomAttributes( newProperty, oldProperty );
            this.ProcessCustomModifiers( oldProperty, newProperty );
            newProperty.GetMethod = this.ProcessMethodRef( oldProperty.GetMethod );
            newProperty.SetMethod = this.ProcessMethodRef( oldProperty.SetMethod );
         }
      }

      private void ProcessEvent( CILType newType, CILEvent oldEvent, Boolean creating )
      {
         var newEvent = creating ?
            newType.AddEvent( oldEvent.Name, oldEvent.Attributes, null ) :
            this._eventMapping[oldEvent];
         if ( creating )
         {
            this._eventMapping.TryAdd( oldEvent, newEvent );
         }
         else
         {
            this.ProcessCustomAttributes( newEvent, oldEvent );
            newEvent.AddMethod = this.ProcessMethodRef( oldEvent.AddMethod );
            newEvent.RemoveMethod = this.ProcessMethodRef( oldEvent.RemoveMethod );
            newEvent.RaiseMethod = this.ProcessMethodRef( oldEvent.RaiseMethod );
            newEvent.EventHandlerType = this.ProcessTypeRef( oldEvent.EventHandlerType );
         }
      }

      private T ProcessTypeRef<T>( T typeRef )
         where T : class, CILTypeBase
      {
         return typeRef == null ?
            null :
            (T) this._typeMapping.GetOrAdd( typeRef, tr =>
            {
               CILTypeBase result = typeRef;
               switch ( typeRef.TypeKind )
               {
                  case TypeKind.Type:
                     var t = (CILType) typeRef;
                     if ( t.ElementKind.HasValue )
                     {
                        // Array/ByRef/Pointer type, save mapped element type.
                        result = this.ProcessTypeRef( t.ElementType ).MakeElementType( t.ElementKind.Value, t.ArrayInformation );
                     }
                     else if ( t.GenericArguments.Count > 0 && !t.IsGenericTypeDefinition() )
                     {
                        // Generic type which is not generic definition, save mapped generic definition
                        result = this.ProcessTypeRef( t.GenericDefinition ).MakeGenericType( t.GenericArguments.Select( g => this.ProcessTypeRef( g ) ).ToArray() );
                     }
                     break;
                  case TypeKind.TypeParameter:
                     var tp = (CILTypeParameter) typeRef;
                     if ( tp.DeclaringMethod == null )
                     {
                        result = this.ProcessTypeRef( tp.DeclaringType ).GenericArguments[tp.GenericParameterPosition];
                     }
                     else
                     {
                        result = this.ProcessMethodRef( tp.DeclaringMethod ).GenericArguments[tp.GenericParameterPosition];
                     }
                     break;
                  case TypeKind.MethodSignature:
                     var ms = (CILMethodSignature) typeRef;
                     result = this._ctx.NewMethodSignature(
                              this._targetModule,
                              ms.CallingConvention,
                              this.ProcessTypeRef( ms.ReturnParameter.ParameterType ),
                              ms.ReturnParameter.CustomModifiers.Select( cm => this.ProcessCustomModifier( cm ) ).ToArray(),
                              ms.Parameters.Select( p => Tuple.Create( p.CustomModifiers.Select( cm => this.ProcessCustomModifier( cm ) ).ToArray(), this.ProcessTypeRef( p.ParameterType ) ) ).ToArray()
                              );
                     break;
               }
               CILAssembly ass;
               if ( this._pcl2TargetMapping != null && result is CILType && ( (CILType) result ).IsTrueDefinition && this._pcl2TargetMapping.TryGetValue( result.Module.Assembly, out ass ) )
               {
                  // Map PCL type to target framework type
                  var tResult = (CILType) result;
                  var typeStr = tResult.GetFullName();
                  var mappedType = GetMainModuleOrFirst( ass ).GetTypeByName( typeStr, false );
                  if ( mappedType == null )
                  {
                     // Try process type forwarders
                     TypeForwardingInfo tf;
                     if ( ass.TryGetTypeForwarder( tResult.Name, tResult.Namespace, out tf ) )
                     {
                        mappedType = GetMainModuleOrFirst( this.LoadLibAssembly( result.Module, tf.AssemblyName ) ).GetTypeByName( typeStr, false );
                     }
                  }
                  if ( mappedType == null )
                  {
                     throw this.NewCILMergeException( ExitCode.PCL2TargetTypeFail, "Failed to find type " + result + " from target framework." );
                  }
                  result = mappedType;
               }
               return result;
            } );
      }

      private void ProcessCustomModifiers( CILElementWithCustomModifiers newElement, CILElementWithCustomModifiers oldElement )
      {
         foreach ( var mod in oldElement.CustomModifiers )
         {
            newElement.AddCustomModifier( this.ProcessTypeRef( mod.Modifier ), mod.Optionality );
         }
      }

      private CILCustomModifier ProcessCustomModifier( CILCustomModifier mod )
      {
         return CILCustomModifierFactory.CreateModifier( mod.Optionality, this.ProcessTypeRef( mod.Modifier ) );
      }

      private TMethod ProcessMethodRef<TMethod>( TMethod method )
         where TMethod : class,CILMethodBase
      {
         return method == null ?
            null :
            (TMethod) this._methodMapping.GetOrAdd( method, mm =>
            {
               CILMethodBase result = mm;
               var m = method as CILMethod;

               if ( m != null && m.HasGenericArguments() && !m.IsGenericMethodDefinition() )
               {
                  result = this.ProcessMethodRef( m.GenericDefinition ).MakeGenericMethod( m.GenericArguments.Select( g => this.ProcessTypeRef( g ) ).ToArray() );
               }
               else if ( method.DeclaringType.IsGenericType() && !method.DeclaringType.IsGenericTypeDefinition() )
               {
                  result = this.ProcessMethodRef( method.ChangeDeclaringTypeUT( method.DeclaringType.GenericDefinition.GenericArguments.ToArray() ) )
                     .ChangeDeclaringTypeUT( method.DeclaringType.GenericArguments.Select( g => this.ProcessTypeRef( g ) ).ToArray() );
               }
               else if ( this._pcl2TargetMapping != null )
               {
                  var tt = this.ProcessTypeRef( method.DeclaringType );
                  if ( !tt.Equals( method.DeclaringType ) )
                  {
                     if ( result is CILConstructor )
                     {
                        result = tt.Constructors.FirstOrDefault( ctor => this.MatchMethodParams( (CILConstructor) result, ctor ) );
                     }
                     else
                     {
                        result = tt.DeclaredMethods.FirstOrDefault( mtd => String.Equals( mtd.Name, result.GetName() ) && this.MatchMethodParams( (CILMethod) result, mtd ) && this.MatchMethodParam( ( (CILMethod) result ).ReturnParameter, mtd.ReturnParameter ) && ( (CILMethod) result ).GenericArguments.Count == mtd.GenericArguments.Count );
                     }
                     if ( result == null )
                     {
                        throw this.NewCILMergeException( ExitCode.PCL2TargetMethodFail, "Failed to find method " + mm + " from target framework." );
                     }
                  }
               }
               return result;
            } );
      }

      private Boolean MatchMethodParams<TMethod>( TMethod pclMethod, TMethod targetMethod )
         where TMethod : CILMethodBase
      {
         return pclMethod.Parameters.Count == targetMethod.Parameters.Count &&
            pclMethod.Parameters.All( p => this.MatchMethodParam( p, targetMethod.Parameters[p.Position] ) );
      }

      private Boolean MatchMethodParam( CILParameter pclParam, CILParameter targetParam )
      {
         return String.Equals( pclParam.ParameterType.ToString(), targetParam.ParameterType.ToString() );// targetParam.ParameterType.Equals( this.ProcessTypeRef( pclParam.ParameterType ) );
      }

      private CILField ProcessFieldRef( CILField field )
      {
         return field == null ?
            null :
            this._fieldMapping.GetOrAdd( field, ff =>
            {
               var result = ff;
               if ( field.DeclaringType.IsGenericType() && !field.DeclaringType.IsGenericTypeDefinition() )
               {
                  result = this.ProcessFieldRef( field.ChangeDeclaringType( field.DeclaringType.GenericDefinition.GenericArguments.ToArray() ) )
                     .ChangeDeclaringType( field.DeclaringType.GenericArguments.Select( g => this.ProcessTypeRef( g ) ).ToArray() );
               }
               else if ( this._pcl2TargetMapping != null )
               {
                  var tt = this.ProcessTypeRef( field.DeclaringType );
                  if ( !tt.Equals( field.DeclaringType ) )
                  {
                     result = tt.DeclaredFields.FirstOrDefault( f => String.Equals( f.Name, result.Name ) && String.Equals( f.FieldType.ToString(), result.FieldType.ToString() ) );//   f.FieldType.Equals( this.ProcessTypeRef( result.FieldType ) ) );
                     if ( result == null )
                     {
                        throw this.NewCILMergeException( ExitCode.PCL2TargetFieldFail, "Failed to find field " + field + " from target framework." );
                     }
                  }
               }
               return result;
            } );
      }

      private void ProcessCustomAttributes( CILCustomAttributeContainer newContainer, CILCustomAttributeContainer oldContainer )
      {
         foreach ( var attr in oldContainer.CustomAttributeData )
         {
            this.ProcessCustomAttribute( newContainer, attr );
         }
      }

      private CILCustomAttribute ProcessCustomAttribute( CILCustomAttributeContainer container, CILCustomAttribute attr )
      {
         return container.AddCustomAttribute(
            this.ProcessMethodRef( attr.Constructor ),
            attr.ConstructorArguments.Select( this.ProcessCustomAttributeTypedArg ),
            attr.NamedArguments.Select( this.ProcessCustomAttributeNamedArg ) );
      }

      private CILCustomAttributeTypedArgument ProcessCustomAttributeTypedArg( CILCustomAttributeTypedArgument arg )
      {
         return CILCustomAttributeFactory.NewTypedArgument( this.ProcessTypeRef( arg.ArgumentType ), arg.Value is CILTypeBase ? this.ProcessTypeRef( (CILTypeBase) arg.Value ) : arg.Value );
      }

      private CILCustomAttributeNamedArgument ProcessCustomAttributeNamedArg( CILCustomAttributeNamedArgument arg )
      {
         return CILCustomAttributeFactory.NewNamedArgument( this.ProcessCustomAttributeNamedOwner( arg.NamedMember ), this.ProcessCustomAttributeTypedArg( arg.TypedValue ) );
      }

      private CILElementForNamedCustomAttribute ProcessCustomAttributeNamedOwner( CILElementForNamedCustomAttribute element )
      {
         return element is CILField ? (CILElementForNamedCustomAttribute) this.ProcessFieldRef( (CILField) element ) : this.ProcessPropertyRef( (CILProperty) element );
      }

      private CILProperty ProcessPropertyRef( CILProperty prop )
      {
         return prop == null ?
            null :
            this._propertyMapping.GetOrAdd( prop, p =>
            {
               var result = p;
               if ( prop.DeclaringType.IsGenericType() && !prop.DeclaringType.IsGenericTypeDefinition() )
               {
                  result = this.ProcessPropertyRef( prop.ChangeDeclaringType( prop.DeclaringType.GenericDefinition.GenericArguments.ToArray() ) )
                     .ChangeDeclaringType( prop.DeclaringType.GenericArguments.Select( g => this.ProcessTypeRef( g ) ).ToArray() );
               }
               return result;
            } );
      }

      private CILEvent ProcessEventRef( CILEvent evt )
      {
         return evt == null ?
            null :
            this._eventMapping.GetOrAdd( evt, e =>
            {
               var result = e;
               if ( evt.DeclaringType.IsGenericType() && !evt.DeclaringType.IsGenericTypeDefinition() )
               {
                  result = this.ProcessEventRef( evt.ChangeDeclaringType( evt.DeclaringType.GenericDefinition.GenericArguments.ToArray() ) )
                     .ChangeDeclaringType( evt.DeclaringType.GenericArguments.Select( g => this.ProcessTypeRef( g ) ).ToArray() );
               }
               return result;
            } );
      }
      private void ProcessTargetAssemblyAttributes()
      {
         var attrSource = this._options.AttrSource;
         if ( attrSource != null )
         {
            // Copy all attributes from module assembly
            EmittingArguments eArgs; CILModule mod;
            this.LoadModuleAndArgs( attrSource, out eArgs, out mod, ExitCode.AttrFileSpecifiedButDoesNotExist, "There was an error accessing assembly attribute file " + attrSource + "." );
            this.ProcessCustomAttributes( this._targetModule, mod.Assembly );
         }
         else
         {
            // Primary module always first.
            var mods = this._options.CopyAttributes ?
               this._inputModules.ToArray() :
               new[] { this._primaryModule };

            var set = new HashSet<CILCustomAttribute>( ComparerFromFunctions.NewEqualityComparer<CILCustomAttribute>( ( attr1, attr2 ) =>
            {
               return String.Equals( attr1.Constructor.DeclaringType.GetAssemblyQualifiedName(), attr2.Constructor.DeclaringType.GetAssemblyQualifiedName() );
            }, attr => attr.Constructor.DeclaringType.ToString().GetHashCode() ) );
            var allowMultipleOption = this._options.AllowMultipleAssemblyAttributes;
            foreach ( var mod in mods )
            {
               foreach ( var ca in mod.Assembly.CustomAttributeData )
               {
                  if ( !set.Add( ca ) )
                  {
                     if ( allowMultipleOption
                        && ca.Constructor.DeclaringType.CustomAttributeData.Any( ca2 =>
                        String.Equals( ATTRIBUTE_USAGE_TYPE.FullName, ca2.Constructor.DeclaringType.GetFullName() )
                        && ca2.NamedArguments.Any( na =>
                           String.Equals( na.NamedMember.Name, ALLOW_MULTIPLE_PROP.Name )
                           && na.TypedValue.Value is Boolean
                           && (Boolean) na.TypedValue.Value ) ) )
                     {
                        this.ProcessCustomAttribute( this._targetModule.Assembly, ca );
                     }
                     else
                     {
                        set.Remove( ca );
                        set.Add( ca );
                     }
                  }
               }
            }
            foreach ( var ca in set )
            {
               this.ProcessCustomAttribute( this._targetModule.Assembly, ca );
            }
         }
      }

      private Boolean IsDuplicateOK( ref String newName, String fullTypeString, TypeAttributes newTypeAttrs )
      {
         var retVal = !this._options.Union && ( !newTypeAttrs.IsVisibleToOutsideOfDefinedAssembly() || this._options.AllowDuplicateTypes == null || this._options.AllowDuplicateTypes.Contains( fullTypeString ) );
         if ( retVal )
         {
            // Have to rename
            var i = 2;
            var namePrefix = newName;
            do
            {
               newName = namePrefix + i;
               ++i;
            } while ( !this._allInputTypeNames.Add( newName ) );
         }
         return retVal;
      }

      private void LoadAllInputModules()
      {
         var paths = this._options.InputAssemblies;
         if ( paths == null || paths.Length == 0 )
         {
            throw this.NewCILMergeException( ExitCode.NoInputAssembly, "Input assembly list must contain at least one assembly." );
         }

         if ( this._options.AllowWildCards )
         {
            paths = paths.SelectMany( path =>
            {
               return path.IndexOf( '*' ) == -1 && path.IndexOf( '?' ) == -1 ?
                  (IEnumerable<String>) new[] { path } :
                  Directory.EnumerateFiles( Path.IsPathRooted( path ) ? Path.GetDirectoryName( path ) : this._inputBasePath, Path.GetFileName( path ) );
            } ).Select( path => Path.GetFullPath( path ) ).ToArray();
         }
         else
         {
            paths = paths.Select( path => Path.IsPathRooted( path ) ? path : Path.Combine( this._inputBasePath, path ) ).ToArray();
         }

         this.DoPotentiallyInParallel( paths, ( isRunningInParallel, path ) =>
         {
            var mod = this.LoadInputModule( path );
            lock ( this._inputModules )
            {
               this._inputModules.Add( mod );
            }
         } );

         if ( this._options.Closed )
         {
            foreach ( var mod in paths.Select( p => this._allModules[p] ).ToArray() )
            {
               foreach ( var aRef in this._loadingArgs[mod].AssemblyRefs )
               {
                  this.LoadLibAssembly( mod, aRef, true );
               }
            }
         }

         this._primaryModule = this._allModules[paths[0]];
      }

      private CILModule LoadInputModule( String path )
      {
         path = Path.GetFullPath( path ); // Removes all "..\.." etc.
         return this._allModules.GetOrAdd( path, pathInner =>
         {
            //Console.WriteLine( "LOADING INPUT MODULE FROM " + path + "." );
            EmittingArguments eArgs; CILModule mod;
            this.LoadModuleAndArgs( pathInner, out eArgs, out mod, ExitCode.ErrorAccessingInputFile, "Error accessing input file " + pathInner + "." );

            if ( !eArgs.ModuleFlags.IsILOnly() && !this._options.ZeroPEKind )
            {
               throw this.NewCILMergeException( ExitCode.NonILOnlyModule, "The module " + mod.Name + " is not IL-only." );
            }
            this._loadingArgs.TryAdd( mod, eArgs );
            return mod;
         } );
      }

      private void LoadModuleAndArgs( String path, out EmittingArguments eArgs, out CILModule module, ExitCode errorCode, String errorMessage )
      {
         eArgs = EmittingArguments.CreateForLoadingModule( this.LoadLibAssembly );
         eArgs.LazyLoad = true; // Just to make sure - it is important to be set as true, otherwise we load things too soon.

         try
         {
            using ( var fs = File.Open( path, FileMode.Open, FileAccess.Read, FileShare.Read ) )
            {
               module = this._ctx.LoadModule( fs, eArgs );
            }
         }
         catch ( Exception exc )
         {
            throw this.NewCILMergeException( errorCode, errorMessage, exc );
         }

         // Deduce mscorlib name
         FrameworkMonikerInfo fwInfo;
         if ( eArgs.FrameworkName == null || eArgs.FrameworkVersion == null )
         {
            // Part of the target framework most likely
            String fwName, fwVersion, fwProfile;
            fwInfo = this._portableHelper.TryGetFrameworkInfo( Path.GetDirectoryName( path ), out fwName, out fwVersion, out fwProfile ) ?
               this._portableHelper[fwName, fwVersion, fwProfile] :
               null;
         }
         else
         {
            fwInfo = this._portableHelper[eArgs];
         }

         if ( fwInfo == null )
         {
            throw this.NewCILMergeException( ExitCode.FailedToDeduceTargetFramework, "Failed to deduce target framework for assembly " + path + ". Make sure TargetFrameworkAttribute is present." );
         }

         eArgs.CorLibName = fwInfo.MsCorLibAssembly;
         var corLibVersion = fwInfo.Assemblies[eArgs.CorLibName].Item1;
         eArgs.CorLibMajor = (UInt16) corLibVersion.Major;
         eArgs.CorLibMinor = (UInt16) corLibVersion.Minor;
         eArgs.CorLibBuild = (UInt16) corLibVersion.Build;
         eArgs.CorLibRevision = (UInt16) corLibVersion.Revision;
      }

      private CILAssembly LoadLibAssembly( CILModule thisModule, CILAssemblyName name )
      {
         return this.LoadLibAssembly( thisModule, name, false );
      }

      private CILAssembly LoadLibAssembly( CILModule thisModule, CILAssemblyName name, Boolean loadForClosedSet )
      {
         var assName = name.Name;
         var suitableFolder = ( this._options.LibPaths ?? Empty<String>.Array ).FirstOrDefault( p => p != null && AssemblyExistsInDirectory( p, assName ) );
         CILAssembly result = null;
         if ( suitableFolder == null && !loadForClosedSet )
         {
            var eArgs = this._loadingArgs[thisModule];
            if ( eArgs.FrameworkName == null || eArgs.FrameworkVersion == null )
            {
               // System library most likely
               // TODO bidi-map for _allModules (now just one directional module -> filename)
               suitableFolder = Path.GetDirectoryName( this._allModules.First( kvp => kvp.Value.Equals( thisModule ) ).Key );
            }
            else
            {
               // Client library referencing system library
               suitableFolder = this._portableHelper.GetDirectory( eArgs );
            }
         }

         if ( result == null && suitableFolder != null )
         {
            result = this.LoadLibAssemblyFromPath( suitableFolder, assName, loadForClosedSet );
         }

         if ( result != null && suitableFolder != null )
         {
            String rFWName, rFWVersion, rFWProfile;
            var primaryArgs = this._loadingArgs[this._inputModules[0]];
            if ( primaryArgs.FrameworkName != null
                && primaryArgs.FrameworkVersion != null
                && this._portableHelper.TryGetFrameworkInfo( suitableFolder, out rFWName, out rFWVersion, out rFWProfile )
                && !String.Equals( primaryArgs.FrameworkName, rFWName ) // || !String.Equals( primaryArgs.FrameworkVersion, rFWVersion ) )
               )
            {
               // We are changing framework (e.g. merging .NET assembly with PCLs)

               // Add mapping from loaded assembly to target framework assembly
               if ( this._pcl2TargetMapping == null )
               {
                  this._pcl2TargetMapping = new Dictionary<CILAssembly, CILAssembly>();
               }
               // Key - this framework library, value - primary input module target framework library
               if ( !this._pcl2TargetMapping.ContainsKey( result ) )
               {
                  this._pcl2TargetMapping.Add( result, this.LoadLibAssemblyFromPath( this._portableHelper.GetDirectory( primaryArgs ), result.Name.Name, false ) );
               }
            }
         }

         if ( result == null && !loadForClosedSet )
         {
            throw this.NewCILMergeException( ExitCode.UnresolvedAssemblyReference, "Unresolved assembly reference from " + thisModule + " to " + name + "." );
         }
         return result;
      }

      private static Boolean AssemblyExistsInDirectory( String dir, String assName )
      {
         return File.Exists( Path.Combine( dir, assName ) + ".dll" ) || File.Exists( Path.Combine( dir, assName ) + ".exe" );
      }

      private CILAssembly LoadLibAssemblyFromPath( String folder, String assName, Boolean loadForClosedSet )
      {
         folder = Path.GetFullPath( folder ); // Remove ..\..\ etc.
         var dllPath = Path.Combine( folder, assName ) + ".dll";
         var fn = File.Exists( dllPath ) ?
            dllPath :
            Path.Combine( folder, assName ) + ".exe";
         var factoryCalled = false;
         var mod = this._allModules.GetOrAdd( fn, fnInner =>
         {
            //Console.WriteLine( "LOADING LIBRARY MODULE FROM " + fnInner + "." );
            factoryCalled = true;
            try
            {
               EmittingArguments eArgs; CILModule mmod;
               this.LoadModuleAndArgs( fn, out eArgs, out mmod, ExitCode.ErrorAccessingReferencedAssembly, "Error accessing referenced assembly " + fn + "." );
               this._loadingArgs.TryAdd( mmod, eArgs );
               return mmod;
            }
            catch ( Exception exc )
            {
               throw this.NewCILMergeException( ExitCode.ErrorAccessingReferencedAssembly, "Error accessing referenced assembly " + fn + ".", exc );
            }
         } );

         if ( loadForClosedSet && factoryCalled )
         {
            this._inputModules.Add( mod );
            // Process all references
            foreach ( var aRef in this._loadingArgs[mod].AssemblyRefs )
            {
               this.LoadLibAssembly( mod, aRef, true );
            }
         }
         return mod == null ? null : mod.Assembly;
      }

      //private Byte[] GetPublicKeyToken( Byte[] pk )
      //{
      //   var hash = this._csp.Value.ComputeHash( pk );
      //   return hash.Skip( hash.Length - 8 ).Reverse().ToArray();
      //}

      private static CILModule GetMainModuleOrFirst( CILAssembly ass )
      {
         return ass.MainModule ?? ass.Modules[0];
      }

      private CILMergeException NewCILMergeException( ExitCode code, String message, Exception inner = null )
      {
         return this._merger.NewCILMergeException( code, message, inner );
      }

      private void Log( MessageLevel mLevel, String formatString, params Object[] args )
      {
         this._merger.Log( mLevel, formatString, args );
      }

      private void DoPotentiallyInParallel<T>( IEnumerable<T> enumerable, Action<Boolean, T> action )
      {
         this._merger.DoPotentiallyInParallel( enumerable, action );
      }

      private void MergeResources()
      {
         foreach ( var mod in this._inputModules )
         {
            foreach ( var kvp in mod.ManifestResources )
            {
               if ( this._targetModule.ManifestResources.ContainsKey( kvp.Key ) )
               {
                  if ( !this._options.AllowDuplicateResources )
                  {
                     this.Log( MessageLevel.Warning, "Ignoring duplicate resource {0} in module {1}.", kvp.Key, mod );
                  }
               }
               else
               {
                  var res = kvp.Value;
                  ManifestResource resourceToAdd = null;
                  if ( res is EmbeddedManifestResource )
                  {
                     var data = ( (EmbeddedManifestResource) res ).Data;
                     var strm = new MemoryStream( data.Length );
                     var rw = new System.Resources.ResourceWriter( strm );
                     Boolean wasResourceManager;
                     foreach ( var resx in MResourcesIO.GetResourceInfo( data, out wasResourceManager ) )
                     {
                        var resName = resx.Item1;
                        var resType = resx.Item2;
                        if ( !resx.Item3 && String.Equals( "ResourceTypeCode.String", resType ) )
                        {
                           // In case there is textual information about types serialized, have to fix that.
                           var idx = resx.Item4;
                           var strlen = data.ReadInt32Encoded7Bit( ref idx );
                           rw.AddResource( resName, this.ProcessTypeString( data.ReadStringWithEncoding( ref idx, strlen, Encoding.UTF8 ) ) );
                        }
                        else
                        {
                           var newTypeStr = this.ProcessTypeString( resType );
                           if ( String.Equals( newTypeStr, resType ) )
                           {
                              // Predefined ResourceTypeCode or pure reference type, add right away
                              var array = new Byte[resx.Item5];
                              var dataStart = resx.Item4;
                              data.BlockCopyFrom( ref dataStart, array );
                              rw.AddResourceData( resName, resType, array );
                           }
                           else
                           {
                              // Have to fix records one by one
                              var idx = resx.Item4;
                              var records = MResourcesIO.ReadNRBFRecords( data, ref idx, idx + resx.Item5 );
                              foreach ( var rec in records )
                              {
                                 this.ProcessNRBFRecord( rec );
                              }
                              var strm2 = new MemoryStream();
                              MResourcesIO.WriteNRBFRecords( records, strm2 );
                              rw.AddResourceData( resName, newTypeStr, strm2.ToArray() );
                           }
                        }
                     }
                     rw.Generate();
                     resourceToAdd = new EmbeddedManifestResource( res.Attributes, strm.ToArray() );
                  }

                  else
                  {
                     var resMod = ( (ModuleManifestResource) res ).Module;
                     if ( resMod != null && !this._inputModules.Any( m => Object.Equals( m, resMod ) ) )
                     {
                        // Resource module which is not part of the input modules - add it to make an assembly-ref module manifest
                        resourceToAdd = res;
                     }
                     // TODO - what if resource-module is part of input modules... ? Should we add link to embedded resource?
                  }
                  if ( resourceToAdd != null )
                  {
                     this._targetModule.ManifestResources.Add( kvp.Key, resourceToAdd );
                  }
               }
            }

         }
      }

      private String ProcessTypeString( String typeStr )
      {
         if ( !String.IsNullOrEmpty( typeStr ) )
         {
            var idx = typeStr.IndexOf( ", " );
            if ( idx > 0 && idx < typeStr.Length - 2 ) // Skip empty type names and type names without assembly name (will be automatically correct)
            {
               var aStartIdx = idx;
               // Skip whitespaces after comma
               while ( ++aStartIdx < typeStr.Length && Char.IsWhiteSpace( typeStr[aStartIdx] ) ) ;
               var assName = typeStr.Substring( aStartIdx );
               // Check whether we actually need to fix the name
               if ( !String.Equals( assName, this.ProcessAssemblyName( assName ) ) )
               {
                  typeStr = this.ProcessTypeName( this._assemblyNameCache[assName].Name, typeStr.Substring( 0, idx ) ); // No need for assembly name since resulting type will be in the target assembly
               }
            }
         }

         return typeStr;
      }

      private String ProcessTypeName( String simpleAssName, String typeName )
      {
         String str;
         if ( this._typeRenames.TryGetValue( Tuple.Create( simpleAssName, typeName ), out str ) )
         {
            typeName = str;
         }
         return typeName;
      }

      private String ProcessAssemblyName( String assName )
      {
         if ( !String.IsNullOrEmpty( assName ) )
         {
            var an = this._assemblyNameCache.GetOrAdd_NotThreadSafe( assName, ani =>
            {
               CILAssemblyName anii;
               if ( CILAssemblyName.TryParse( ani, out anii ) )
               {
                  return anii;
               }
               else
               {
                  return null;
               }
            } );
            if ( this._inputModules.Any( m => m.Assembly.Name.CorePropertiesEqual( an ) ) )
            {
               // TODO this can be optimized so .ToString() wouldn't need to be called every time
               assName = this._targetModule.Assembly.Name.ToString();
            }
         }
         return assName;
      }

      private void ProcessNRBFRecord( AbstractRecord record )
      {
         if ( record != null )
         {
            switch ( record.Kind )
            {
               case RecordKind.String:
                  ( (StringRecord) record ).StringValue = this.ProcessTypeString( ( (StringRecord) record ).StringValue );
                  break;
               case RecordKind.Class:
                  var claas = (ClassRecord) record;
                  this.ProcessNRBFTypeInfo( claas );
                  foreach ( var member in claas.Members )
                  {
                     this.ProcessNRBFTypeInfo( member );
                     this.ProcessNRBFRecord( member.Value as AbstractRecord );
                  }
                  break;
               case RecordKind.Array:
                  foreach ( var val in ( (ArrayRecord) record ).ValuesAsVector )
                  {
                     this.ProcessNRBFRecord( val as AbstractRecord );
                  }
                  break;
               case RecordKind.PrimitiveWrapper:
                  var wrapper = (PrimitiveWrapperRecord) record;
                  if ( wrapper.Value is String )
                  {
                     wrapper.Value = this.ProcessTypeString( wrapper.Value as String );
                  }
                  break;
            }
         }
      }

      private void ProcessNRBFTypeInfo( ElementWithTypeInfo element )
      {
         var tmp = element.AssemblyName;
         if ( tmp != null )
         {
            var tmp2 = this.ProcessAssemblyName( tmp );
            if ( !String.Equals( tmp, tmp2 ) )
            {
               element.AssemblyName = tmp2;
            }
            tmp2 = this.ProcessTypeName( this._assemblyNameCache[tmp].Name, element.TypeName );
            if ( !String.Equals( element.TypeName, tmp2 ) )
            {
               element.TypeName = tmp2;
            }
         }
      }

      private void MergePDBs( EmittingArguments targetEArgs, EmittingArguments[] inputEArgs )
      {
         // Merge PDBs
         var bag = new ConcurrentBag<PDBInstance>();
         this.DoPotentiallyInParallel( inputEArgs, ( isRunningInParallel, eArg ) =>
         {
            if ( eArg.DebugInformation != null && eArg.DebugInformation.DebugType == 2 ) // CodeView
            {
               var pdbFNStartIdx = 24;
               var pdbFN = eArg.DebugInformation.DebugData.ReadZeroTerminatedStringFromBytes( ref pdbFNStartIdx, Encoding.UTF8 );
               if ( pdbFN != null )
               {
                  PDBInstance iPDB = null;
                  try
                  {
                     using ( var fs = File.Open( pdbFN, FileMode.Open, FileAccess.Read, FileShare.Read ) )
                     {
                        iPDB = PDBIO.FromStream( fs );
                     }
                  }
                  catch ( Exception exc )
                  {
                     this.Log( MessageLevel.Warning, "Could not open file {0} because of {1}.", pdbFN, exc );
                  }

                  if ( iPDB != null )
                  {
                     bag.Add( iPDB );
                     // Translate all tokens
                     foreach ( var func in iPDB.Modules.SelectMany( m => m.Functions ) )
                     {
                        var thisMethod = (CILMethodBase) eArg.ResolveToken( null, (Int32) func.Token );
                        func.Token = this.MapMethodToken( targetEArgs, eArg, thisMethod, func.Token );
                        func.ForwardingMethodToken = this.MapMethodToken( targetEArgs, eArg, thisMethod, func.ForwardingMethodToken );
                        func.ModuleForwardingMethodToken = this.MapMethodToken( targetEArgs, eArg, thisMethod, func.ModuleForwardingMethodToken );
                        this.MapPDBScopeOrFunction( func, targetEArgs, eArg, thisMethod );
                        //if ( func.AsyncMethodInfo != null )
                        //{
                        //   func.AsyncMethodInfo.KickoffMethodToken = this.MapMethodToken( targetEArgs, eArg, thisMethod, func.AsyncMethodInfo.KickoffMethodToken );
                        //   foreach ( var syncP in func.AsyncMethodInfo.SynchronizationPoints )
                        //   {
                        //      syncP.ContinuationMethodToken = this.MapMethodToken( targetEArgs, eArg, thisMethod, syncP.ContinuationMethodToken );
                        //   }
                        //}
                     }
                  }
               }
            }
         } );

         var pdb = new PDBInstance();
         //pdb.Age = (UInt32) targetEArgs.DebugInformation.DebugFileAge;
         //pdb.DebugGUID = targetEArgs.DebugInformation.DebugFileGUID;
         // Add all information. Should be ok, since all IL offsets remain unchanged, and all tokens are translated.
         foreach ( var currentPDB in bag )
         {
            // Merge sources
            foreach ( var src in currentPDB.Sources )
            {
               pdb.TryAddSource( src.Name, src );
            }
            // Merge modules
            foreach ( var mod in currentPDB.Modules )
            {
               var curMod = pdb.GetOrAddModule( mod.Name );
               foreach ( var func in mod.Functions )
               {
                  curMod.Functions.Add( func );
               }
            }
         }

         // Write PDB
         //try
         //{
         //using ( var fs = File.Open( targetEArgs.DebugInformation.DebugFileLocation, FileMode.Create, FileAccess.Write, FileShare.Read ) )
         //{
         //pdb.WriteToStream( fs, targetEArgs.DebugInformation.Timestamp );

         this._pdbHelper.ProcessPDB( pdb );
         //}
         //}
         //catch ( Exception exc )
         //{
         //   if ( exc is PDBException )
         //   {
         //      throw this.NewCILMergeException( ExitCode.ErrorWritingPDB, "Error writing PDB file for " + this._options.OutPath + ".", exc );
         //   }
         //   else
         //   {
         //      throw this.NewCILMergeException( ExitCode.ErrorAccessingTargetPDB, "Error accessing target PDB file for " + this._options.OutPath + ".", exc );
         //   }

         //}
      }

      #region IDisposable Members

      public void Dispose()
      {
         try
         {
            var pdb = this._pdbHelper;
            if ( pdb != null )
            {
               pdb.Dispose();
            }
            if ( this._csp.IsValueCreated )
            {
               this._csp.Value.Dispose();
            }
         }
         catch ( Exception exc )
         {
            //if ( exc is PDBException )
            //{
            throw this.NewCILMergeException( ExitCode.ErrorWritingPDB, "Error writing PDB file for " + this._options.OutPath + ".", exc );
            //}
            //else
            //{
            //throw this.NewCILMergeException( ExitCode.ErrorAccessingTargetPDB, "Error accessing target PDB file for " + this._options.OutPath + ".", exc );
            //}

         }
      }

      #endregion
   }

   internal class CILMergeException : Exception
   {
      internal readonly ExitCode _exitCode;

      internal CILMergeException( ExitCode exitCode, String msg, Exception inner )
         : base( msg, inner )
      {
         this._exitCode = exitCode;
      }
   }

   internal static class E_Internal
   {
      internal static TValue GetOrAdd<TKey, TValue>( this IDictionary<TKey, TValue> dic, TKey key, Func<TKey, TValue> factory )
      {
         TValue val;
         if ( !dic.TryGetValue( key, out val ) )
         {
            val = factory( key );
            dic.Add( key, val );
         }
         return val;
      }

      internal static void TryAdd<TKey, TValue>( this IDictionary<TKey, TValue> dic, TKey key, TValue value )
      {
         dic.Add( key, value );
      }


   }
}
