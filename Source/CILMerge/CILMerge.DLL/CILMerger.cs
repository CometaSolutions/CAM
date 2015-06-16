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
using CILAssemblyManipulator.MResources;
using CILAssemblyManipulator.PDB;
using CommonUtils;
using CILAssemblyManipulator.Physical;
using System.Runtime.InteropServices;
using System.Diagnostics;
using CILAssemblyManipulator.Physical.DotNET;

namespace CILMerge
{

   public class CILMerger
   {
      private readonly CILMergeOptions _options;
      private readonly CILMergeLogCallback _log;

      public CILMerger( CILMergeOptions options, CILMergeLogCallback logCallback )
      {
         if ( options == null )
         {
            throw new ArgumentNullException( "Options" );
         }
         this._options = options;

         if ( this._options.DoLogging )
         {
            this._log = logCallback;
         }
      }

      public void PerformMerge()
      {
         // 1. Merge assemblies
         CILModuleMergeResult assemblyMergeResult;
         using ( var assMerger = new CILAssemblyMerger( this, this._options, Environment.CurrentDirectory ) )
         {
            assemblyMergeResult = assMerger.MergeModules();
         }

         // 2. Merge PDBs
         var pdbHelper = assemblyMergeResult.PDBHelper;
         if ( pdbHelper != null )
         {
            try
            {
               this.DoWithStopWatch( "Merging PDB files", () =>
               {
                  using ( pdbHelper )
                  {
                     this.MergePDBs( pdbHelper, assemblyMergeResult );
                  }
               } );
            }
            catch ( Exception exc )
            {
               this.Log( MessageLevel.Warning, "Error when creating PDB file for {0}. Error:\n{1}", this._options.OutPath, exc );
            }
         }

         if ( this._options.XmlDocs )
         {
            // 3. Merge documentation
            this.MergeXMLDocs( assemblyMergeResult );
         }
      }

      private void MergePDBs( PDBHelper pdbHelper, CILModuleMergeResult mergeResult )
      {
         // Merge PDBs
         var bag = new ConcurrentBag<PDBInstance>();
         this.DoPotentiallyInParallel( mergeResult.InputMergeResults, ( isRunningInParallel, iResult ) =>
         {

            var eArg = iResult.ReadingArguments;
            var headers = eArg.Headers;
            var debugInfo = headers.DebugInformation;
            if ( debugInfo != null && debugInfo.DebugType == 2 ) // CodeView
            {
               var pdbFNStartIdx = 24;
               var pdbFN = debugInfo.DebugData.ReadZeroTerminatedStringFromBytes( ref pdbFNStartIdx, Encoding.UTF8 );
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
                        func.Token = this.MapMethodToken( iResult, func.Token );
                        func.ForwardingMethodToken = this.MapMethodToken( iResult, func.ForwardingMethodToken );
                        func.ModuleForwardingMethodToken = this.MapMethodToken( iResult, func.ModuleForwardingMethodToken );
                        this.MapPDBScopeOrFunction( func, iResult );
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

         pdbHelper.ProcessPDB( pdb );
      }

      private void MapPDBScopeOrFunction( PDBScopeOrFunction scp, InputModuleMergeResult mergeResult )
      {
         foreach ( var slot in scp.Slots )
         {
            slot.TypeToken = this.MapTypeToken( mergeResult, slot.TypeToken );
         }
         foreach ( var subScope in scp.Scopes )
         {
            this.MapPDBScopeOrFunction( subScope, mergeResult );
         }
      }

      private UInt32 MapMethodToken( InputModuleMergeResult mergeResult, UInt32 token )
      {
         var inputToken = TableIndex.FromOneBasedTokenNullable( (Int32) token );
         TableIndex targetToken;
         return inputToken.HasValue && mergeResult.InputToTargetMapping.TryGetValue( inputToken.Value, out targetToken ) ?
            (UInt32) targetToken.OneBasedToken :
            0u;
      }

      private UInt32 MapTypeToken( InputModuleMergeResult mergeResult, UInt32 token )
      {
         var inputToken = TableIndex.FromOneBasedTokenNullable( (Int32) token );
         TableIndex targetToken;
         return inputToken.HasValue && mergeResult.InputToTargetMapping.TryGetValue( inputToken.Value, out targetToken ) ?
            (UInt32) targetToken.OneBasedToken :
            0u;
      }

      private void MergeXMLDocs( CILModuleMergeResult assemblyMergeResult )
      {
         var xmlDocs = new ConcurrentDictionary<InputModuleMergeResult, XDocument>( ReferenceEqualityComparer<InputModuleMergeResult>.ReferenceBasedComparer );

         this.DoWithStopWatch( "Merging XML document files", () =>
         {

            // TODO on *nix, comparison should maybe be case-sensitive.
            var outXmlPath = Path.ChangeExtension( this._options.OutPath, ".xml" );
            var libPaths = this._options.LibPaths;
            this.DoPotentiallyInParallel( assemblyMergeResult.InputMergeResults, ( isRunningInParallel, mResult ) =>
            {
               XDocument xDoc = null;
               var path = mResult.ModulePath;
               var xfn = Path.ChangeExtension( path, ".xml" );
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
               if ( xDoc == null && !libPaths.IsNullOrEmpty() )
               {
                  // Try load from lib paths
                  xfn = Path.GetFileName( xfn );
                  foreach ( var p in libPaths )
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
                  xmlDocs.TryAdd( mResult, xDoc );
               }
            } );


            using ( var fs = File.Open( outXmlPath, FileMode.Create, FileAccess.Write, FileShare.Read ) )
            {
               // Create and save document (Need to use XmlWriter if 4-space indent needs to be preserved, the XElement saves using 2-space indent and it is not customizable).
               new XElement( "doc",
                  new XElement( "assembly", new XElement( "name", Path.GetFileNameWithoutExtension( this._options.OutPath ) ) ), // Assembly name
                  new XElement( "members", xmlDocs
                     .Select( kvp => Tuple.Create( kvp.Key, kvp.Value.XPathSelectElements( "/doc/members/*" ) ) )
                     .SelectMany( tuple =>
                     {
                        var renameDic = tuple.Item1.TypeRenames;
                        if ( renameDic.Count > 0 )
                        {
                           foreach ( var el in tuple.Item2 )
                           {
                              var nameAttr = el.Attribute( "name" );
                              String typeName;
                              if ( nameAttr != null && nameAttr.Value.StartsWith( "T:" ) && renameDic.TryGetValue( el.Attribute( "name" ).Value.Substring( 2 ), out typeName ) )
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
         } );
      }

      internal CILMergeException NewCILMergeException( ExitCode code, String message, Exception inner = null )
      {
         this.Log( MessageLevel.Error, message );
         return new CILMergeException( code, message, inner );
      }

      internal void Log( MessageLevel mLevel, String formatString, params Object[] args )
      {
         if ( this._log != null )
         {
            this._log.Log( mLevel, formatString, args );
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

      internal void DoWithStopWatch( String what, Action action )
      {
         var sw = new Stopwatch();
         sw.Start();

         action();

         sw.Stop();
         this.Log( MessageLevel.Info, what + " took {0} ms.", sw.ElapsedMilliseconds );
      }

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
      FailedToMapPDBType,
      VariableTypeGenericParameterCount,
      NoTargetFrameworkSpecified,
      UnresolvableMemberReferenceToAnotherInputModule,
      //ErrorMatchingMemberReferenceSignature
   }

   internal class CILModuleMergeResult
   {
      private readonly InputModuleMergeResult[] _inputMergeResults;
      private readonly CILMetaData _targetModule;
      private readonly EmittingArguments _emittingArguments;
      private readonly PDBHelper _pdbHelper;

      internal CILModuleMergeResult(
         CILMetaData targetModule,
         EmittingArguments emittingArguments,
         PDBHelper pdbHelper,
         IEnumerable<InputModuleMergeResult> inputMergeResults
         )
      {
         ArgumentValidator.ValidateNotNull( "Target module", targetModule );
         ArgumentValidator.ValidateNotNull( "Emitting arguments", emittingArguments );
         ArgumentValidator.ValidateNotNull( "Input merge results", inputMergeResults );

         this._targetModule = targetModule;
         this._emittingArguments = emittingArguments;
         this._inputMergeResults = inputMergeResults.ToArray();
         this._pdbHelper = pdbHelper;
         ArgumentValidator.ValidateNotEmpty( "Input merge results", this._inputMergeResults );
      }

      public CILMetaData TargetModule
      {
         get
         {
            return this._targetModule;
         }
      }

      public EmittingArguments EmittingArguments
      {
         get
         {
            return this._emittingArguments;
         }
      }

      public InputModuleMergeResult[] InputMergeResults
      {
         get
         {
            return this._inputMergeResults;
         }
      }

      public PDBHelper PDBHelper
      {
         get
         {
            return this._pdbHelper;
         }
      }

   }


   internal class InputModuleMergeResult
   {
      private readonly CILMetaData _inputModule;
      private readonly String _modulePath;
      private readonly ReadingArguments _readingArguments;
      private readonly Lazy<IDictionary<String, String>> _typeRenames;
      private readonly Lazy<IDictionary<TableIndex, TableIndex>> _inputToTargetMapping;

      public InputModuleMergeResult(
         CILMetaData inputModule,
         String modulePath,
         ReadingArguments readingArguments,
         Func<IDictionary<String, String>> typeRenames,
         Func<IDictionary<TableIndex, TableIndex>> inputToTarget
         )
      {
         ArgumentValidator.ValidateNotNull( "Input module", inputModule );
         ArgumentValidator.ValidateNotEmpty( "Module path", modulePath );
         ArgumentValidator.ValidateNotNull( "Reading arguments", readingArguments );
         ArgumentValidator.ValidateNotNull( "Type renames", typeRenames );
         ArgumentValidator.ValidateNotNull( "Input to target mapping", inputToTarget );

         this._inputModule = inputModule;
         this._modulePath = modulePath;
         this._readingArguments = readingArguments;
         this._typeRenames = new Lazy<IDictionary<String, String>>( typeRenames, LazyThreadSafetyMode.ExecutionAndPublication );
         this._inputToTargetMapping = new Lazy<IDictionary<TableIndex, TableIndex>>( inputToTarget, LazyThreadSafetyMode.ExecutionAndPublication );
      }

      public String ModulePath
      {
         get
         {
            return this._modulePath;
         }
      }

      public ReadingArguments ReadingArguments
      {
         get
         {
            return this._readingArguments;
         }
      }

      public IDictionary<String, String> TypeRenames
      {
         get
         {
            return this._typeRenames.Value;
         }
      }

      public IDictionary<TableIndex, TableIndex> InputToTargetMapping
      {
         get
         {
            return this._inputToTargetMapping.Value;
         }
      }
   }

   internal class CILAssemblyMerger : AbstractDisposable, IDisposable
   {
      //private class PortabilityHelper
      //{
      //   private readonly String _referenceAssembliesPath;
      //   private readonly IDictionary<Tuple<String, String, String>, FrameworkMonikerInfo> _dic;
      //   private readonly CILAssemblyMerger _merger;
      //   private readonly IDictionary<FrameworkMonikerInfo, String> _explicitDirectories;

      //   internal PortabilityHelper( CILAssemblyMerger merger, String referenceAssembliesPath )
      //   {
      //      this._merger = merger;
      //      this._referenceAssembliesPath = Path.GetFullPath( referenceAssembliesPath ?? DotNETReflectionContext.GetDefaultReferenceAssemblyPath() );
      //      this._dic = new Dictionary<Tuple<String, String, String>, FrameworkMonikerInfo>();
      //      this._explicitDirectories = new Dictionary<FrameworkMonikerInfo, String>();
      //   }

      //   public FrameworkMonikerInfo this[EmittingArguments eArgs]
      //   {
      //      get
      //      {
      //         return this[eArgs.FrameworkName, eArgs.FrameworkVersion, eArgs.FrameworkProfile];
      //      }
      //   }

      //   public FrameworkMonikerInfo this[String fwName, String fwVersion, String fwProfile]
      //   {
      //      get
      //      {
      //         ArgumentValidator.ValidateNotNull( "Framework name", fwName );
      //         ArgumentValidator.ValidateNotNull( "Framework version", fwVersion );
      //         FrameworkMonikerInfo moniker;
      //         var key = Tuple.Create( fwName, fwVersion, fwProfile );
      //         if ( !this._dic.TryGetValue( key, out moniker ) )
      //         {
      //            var dir = this.GetDirectory( fwName, fwVersion, fwProfile );
      //            if ( !Directory.Exists( dir ) )
      //            {
      //               throw this._merger.NewCILMergeException( ExitCode.NoTargetFrameworkMoniker, "Couldn't find framework moniker info for framework \"" + fwName + "\", version \"" + fwVersion + "\"" + ( String.IsNullOrEmpty( fwProfile ) ? "" : ( ", profile \"" + fwProfile + "\"" ) ) + " (reference assembly path: " + this._referenceAssembliesPath + ")." );
      //            }
      //            else
      //            {
      //               var redistListDir = Path.Combine( dir, "RedistList" );
      //               var fn = Path.Combine( redistListDir, "FrameworkList.xml" );
      //               String msCorLibName; String fwDisplayName; String targetFWDir;
      //               try
      //               {
      //                  moniker = new FrameworkMonikerInfo( fwName, fwVersion, fwProfile, DotNETReflectionContext.ReadAssemblyInformationFromRedistXMLFile(
      //                           fn,
      //                           out msCorLibName,
      //                           out fwDisplayName,
      //                           out targetFWDir
      //                           ), msCorLibName, fwDisplayName );
      //                  if ( !String.IsNullOrEmpty( targetFWDir ) )
      //                  {
      //                     this._explicitDirectories.Add( moniker, targetFWDir );
      //                  }
      //               }
      //               catch ( Exception exc )
      //               {
      //                  throw this._merger.NewCILMergeException( ExitCode.FailedToReadTargetFrameworkMonikerInformation, "Failed to read FrameworkList.xml from " + fn + " (" + exc.Message + ").", exc );
      //               }
      //            }
      //         }
      //         return moniker;
      //      }
      //   }

      //   public String GetDirectory( EmittingArguments eArgs )
      //   {
      //      var fwInfo = this[eArgs];
      //      String retVal;
      //      return this._explicitDirectories.TryGetValue( fwInfo, out retVal ) ?
      //         retVal :
      //         this.GetDirectory( eArgs.FrameworkName, eArgs.FrameworkVersion, eArgs.FrameworkProfile );
      //   }

      //   private String GetDirectory( String fwName, String fwVersion, String fwProfile )
      //   {
      //      var retVal = Path.Combine( this._referenceAssembliesPath, fwName, fwVersion );
      //      if ( !String.IsNullOrEmpty( fwProfile ) )
      //      {
      //         retVal = Path.Combine( retVal, "Profile", fwProfile );
      //      }
      //      return retVal;
      //   }

      //   public Boolean TryGetFrameworkInfo( String dir, out String fwName, out String fwVersion, out String fwProfile )
      //   {
      //      dir = Path.GetFullPath( dir );
      //      fwName = null;
      //      fwVersion = null;
      //      fwProfile = null;
      //      var retVal = dir.StartsWith( this._referenceAssembliesPath ) && dir.Length > this._referenceAssembliesPath.Length;
      //      if ( retVal )
      //      {
      //         dir = dir.Substring( this._referenceAssembliesPath.Length );
      //         var dirs = dir.Split( new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries );
      //         retVal = dirs.Length >= 2;
      //         if ( retVal )
      //         {
      //            fwName = dirs[0];
      //            fwVersion = dirs[1];
      //            fwProfile = dirs.Length >= 4 ? dirs[3] : null;
      //         }
      //      }
      //      else
      //      {
      //         // See if this framework is explicitly defined elsewhere
      //         var fwInfo = this._explicitDirectories.Where( kvp => String.Equals( dir, kvp.Value ) ).Select( kvp => kvp.Key ).FirstOrDefault();
      //         retVal = fwInfo != null;
      //         if ( retVal )
      //         {
      //            fwName = fwInfo.FrameworkName;
      //            fwVersion = fwInfo.FrameworkVersion;
      //            fwProfile = fwInfo.ProfileName;
      //         }
      //      }
      //      return retVal;
      //   }

      //   public String ReferenceAssembliesPath
      //   {
      //      get
      //      {
      //         return this._referenceAssembliesPath;
      //      }
      //   }

      //}

      private static readonly Type ATTRIBUTE_USAGE_TYPE = typeof( AttributeUsageAttribute );
      private static readonly System.Reflection.PropertyInfo ALLOW_MULTIPLE_PROP = typeof( AttributeUsageAttribute ).GetProperty( "AllowMultiple" );

      private readonly CILMerger _merger;
      private readonly CILMergeOptions _options;
      private readonly CILMetaDataLoaderResourceCallbacksForFiles _loaderCallbacks;
      private readonly CILMetaDataLoader _moduleLoader;
      private readonly CryptoCallbacks _cryptoCallbacks;

      private readonly List<CILMetaData> _inputModules;
      private readonly IDictionary<AssemblyReference, CILMetaData> _inputModulesAsAssemblyReferences;
      private readonly IDictionary<CILMetaData, IDictionary<String, CILMetaData>> _inputModulesAsModuleReferences;
      private CILMetaData _primaryModule;
      private CILMetaData _targetModule;

      // Key: one of input modules. Value: dictionary; Key: table index in input module. Value: table index in output module
      private readonly IDictionary<CILMetaData, IDictionary<TableIndex, TableIndex>> _tableIndexMappings;
      // Key: table index in target module, Value: input module, and corresponding index in corresponding table in the input module
      private readonly IDictionary<TableIndex, Tuple<CILMetaData, Int32>> _targetTableIndexMappings;
      // Key: one of input modules. Value: dictionary; Key: full type name, value: type def index in TARGET module
      private readonly IDictionary<CILMetaData, IDictionary<String, Int32>> _inputModuleTypeNamesInTargetModule;
      // Key: on of input modules. Value: dictionary; Key: full type name, value: type def index in INPUT module
      private readonly IDictionary<CILMetaData, IDictionary<String, Int32>> _inputModuleTypeNamesInInputModule;


      private readonly IList<String> _targetTypeNames;
      private readonly Lazy<Regex[]> _excludeRegexes;
      private readonly String _inputBasePath;

      private readonly Lazy<HashStreamInfo> _publicKeyComputer;

      //private readonly CILReflectionContext _ctx;
      //private readonly CILAssemblyLoader _assemblyLoader;
      //private readonly ISet<String> _allInputTypeNames;
      //private readonly ISet<String> _typesByName;
      //private readonly IDictionary<CILTypeBase, CILTypeBase> _typeMapping;
      //private readonly IDictionary<CILMethodBase, CILMethodBase> _methodMapping;
      //private readonly IDictionary<CILField, CILField> _fieldMapping;
      //private readonly IDictionary<CILEvent, CILEvent> _eventMapping;
      //private readonly IDictionary<CILProperty, CILProperty> _propertyMapping;
      ////private readonly TextWriter _logStream;
      ////private readonly Boolean _disposeLogStream;
      //private readonly IDictionary<String, CILAssemblyName> _assemblyNameCache;

      //private IDictionary<CILAssembly, CILAssembly> _pcl2TargetMapping;
      //private PDBHelper _pdbHelper;
      //private readonly Lazy<System.Security.Cryptography.SHA1CryptoServiceProvider> _csp;

      internal CILAssemblyMerger( CILMerger merger, CILMergeOptions options, String inputBasePath )
      {
         this._merger = merger;
         this._options = options;

         this._inputModules = new List<CILMetaData>();
         this._loaderCallbacks = new CILMetaDataLoaderResourceCallbacksForFiles(
            referenceAssemblyBasePath: options.ReferenceAssembliesDirectory
            );
         this._moduleLoader = options.Parallel ?
            (CILMetaDataLoader) new CILMetaDataLoaderThreadSafeConcurrentForFiles( this._loaderCallbacks ) :
            new CILMetaDataLoaderNotThreadSafeForFiles( this._loaderCallbacks );
         this._cryptoCallbacks = new CryptoCallbacksDotNET();
         this._publicKeyComputer = new Lazy<HashStreamInfo>( () => this._cryptoCallbacks.CreateHashStream( AssemblyHashAlgorithm.SHA1 ), LazyThreadSafetyMode.None );
         this._inputModulesAsAssemblyReferences = new Dictionary<AssemblyReference, CILMetaData>(
            ComparerFromFunctions.NewEqualityComparer<AssemblyReference>(
               ( x, y ) =>
               {
                  Boolean retVal;
                  var xa = x.AssemblyInformation;
                  var ya = y.AssemblyInformation;
                  if ( x.Attributes.IsFullPublicKey() == y.Attributes.IsFullPublicKey() )
                  {
                     retVal = xa.Equals( ya );
                  }
                  else
                  {
                     retVal = xa.Equals( ya, false );
                     if ( retVal
                        && !xa.PublicKeyOrToken.IsNullOrEmpty()
                        && !ya.PublicKeyOrToken.IsNullOrEmpty()
                        )
                     {
                        Byte[] xBytes, yBytes;
                        if ( x.Attributes.IsFullPublicKey() )
                        {
                           // Create public key token for x and compare with y
                           xBytes = this._publicKeyComputer.Value.ComputePublicKeyToken( xa.PublicKeyOrToken );
                           yBytes = ya.PublicKeyOrToken;
                        }
                        else
                        {
                           // Create public key token for y and compare with x
                           xBytes = xa.PublicKeyOrToken;
                           yBytes = this._publicKeyComputer.Value.ComputePublicKeyToken( ya.PublicKeyOrToken );
                        }
                        retVal = ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( xBytes, yBytes );
                     }
                  }
                  return retVal;
               },
               x => x.AssemblyInformation.GetHashCode()
            ) );
         this._inputModulesAsModuleReferences = new Dictionary<CILMetaData, IDictionary<String, CILMetaData>>( ReferenceEqualityComparer<CILMetaData>.ReferenceBasedComparer );
         this._tableIndexMappings = new Dictionary<CILMetaData, IDictionary<TableIndex, TableIndex>>( ReferenceEqualityComparer<CILMetaData>.ReferenceBasedComparer );
         this._targetTableIndexMappings = new Dictionary<TableIndex, Tuple<CILMetaData, Int32>>();
         this._inputModuleTypeNamesInTargetModule = new Dictionary<CILMetaData, IDictionary<String, Int32>>( ReferenceEqualityComparer<CILMetaData>.ReferenceBasedComparer );
         this._inputModuleTypeNamesInInputModule = new Dictionary<CILMetaData, IDictionary<String, Int32>>( ReferenceEqualityComparer<CILMetaData>.ReferenceBasedComparer );
         this._targetTypeNames = new List<String>();




         //this._ctx = ctx;
         ////this._allModules = new ConcurrentDictionary<String, CILModule>();
         ////this._loadingArgs = new ConcurrentDictionary<CILModule, EmittingArguments>();
         //this._typesByName = new HashSet<String>();
         //this._typeMapping = new Dictionary<CILTypeBase, CILTypeBase>();
         //this._methodMapping = new Dictionary<CILMethodBase, CILMethodBase>();
         //this._fieldMapping = new Dictionary<CILField, CILField>();
         //this._eventMapping = new Dictionary<CILEvent, CILEvent>();
         //this._propertyMapping = new Dictionary<CILProperty, CILProperty>();

         //this._assemblyLoader = ctx.CreateAssemblyLoader( this._options.ReferenceAssembliesDirectory );

         //this._allInputTypeNames = options.Union ?
         //   null :
         //   new HashSet<String>();
         //this._typeRenames = options.Union ?
         //   null :
         //   new Dictionary<Tuple<String, String>, String>();
         //this._assemblyNameCache = new Dictionary<String, CILAssemblyName>();
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
         //if ( this._options.DoLogging )
         //{
         //   // TODO
         //   //var logFile = this._options.LogFile;
         //   //logFile = logFile == null ?
         //   //   null :
         //   //   Path.GetFullPath( logFile );
         //   //try
         //   //{
         //   //   this._disposeLogStream = !String.IsNullOrEmpty( logFile );
         //   //   this._logStream = this._disposeLogStream ?
         //   //      new StreamWriter( logFile, false, Encoding.UTF8 ) :
         //   //      Console.Out;
         //   //}
         //   //catch ( Exception exc )
         //   //{
         //   //   throw this.NewCILMergeException( ExitCode.ErrorAccessingLogFile, "Error accessing log file " + logFile + ".", exc );
         //   //}
         //}
         //this._csp = new Lazy<System.Security.Cryptography.SHA1CryptoServiceProvider>( () => new System.Security.Cryptography.SHA1CryptoServiceProvider(), LazyThreadSafetyMode.ExecutionAndPublication );
      }

      internal CILModuleMergeResult MergeModules()
      {
         EmittingArguments eArgs = null;
         Int32[][] reorderResult = null;
         PDBHelper pdbHelper = null;
         this._merger.DoWithStopWatch( "Merging modules and assemblies as a whole", () =>
         {

            // First of all, load all input modules
            this._merger.DoWithStopWatch( "Part 1: Loading input assemblies and modules", () =>
            {
               this.LoadAllInputModules();
            } );


            // Then, create target module
            eArgs = this.CreateEmittingArgumentsForTargetModule();
            this.CreateTargetAssembly( eArgs );

            this._merger.DoWithStopWatch( "Part 2: Populating target module", () =>
            {
               // 1. Create structural tables
               var typeDefInfo = this.ConstructStructuralTables();
               // 2. Create tables used in signatures and IL tokens
               this.ConstructTablesUsedInSignaturesAndILTokens( typeDefInfo, eArgs );
               // 3. Create signatures and IL
               this.ConstructSignaturesAndMethodIL();
               // 4. Create the rest of the tables
               this.ConstructTheRestOfTheTables();
               // 5. Create assembly & module custom attributes
               this.ApplyAssemblyAndModuleCustomAttributes();
               // 6. Fix regargetable assembly references if needed
               // Do it here, after TargetFrameworkAttribute has been applied specified
               this.FixTargetAssemblyReferences();
            } );
            // Process target module

            this._merger.DoWithStopWatch( "Part 3: Reordering target module tables and removing duplicates", () =>
            {
               // 7. Re-order and remove duplicates from tables
               reorderResult = this._targetModule.OrderTablesAndRemoveDuplicates();
            } );

            // Prepare PDB

            // Create PDB helper here, so it would modify EmittingArguments *before* actual emitting
            pdbHelper = !this._options.NoDebug && this._inputModules.Any( m => this._moduleLoader.GetReadingArgumentsForMetaData( m ).Headers.DebugInformation != null ) ?
               new PDBHelper( this._targetModule, eArgs, this._options.OutPath ) :
               null;

            this._merger.DoWithStopWatch( "Part 4: Writing target module", () =>
            {
               // 8. Emit module
               try
               {
                  using ( var fs = File.Open( this._options.OutPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None ) )
                  {
                     this._targetModule.WriteModule( fs, eArgs );
                  }
               }
               catch ( Exception exc )
               {
                  throw this.NewCILMergeException( ExitCode.ErrorAccessingTargetFile, "Error accessing target file " + this._options.OutPath + "(" + exc + ").", exc );
               }

            } );
         } );

         return new CILModuleMergeResult(
            this._targetModule,
            eArgs,
            pdbHelper,
            this._inputModules.Select( m => new InputModuleMergeResult(
               m,
               this._moduleLoader.GetResourceFor( m ),
               this._moduleLoader.GetReadingArgumentsForMetaData( m ),
               () => this.CreateRenameDictionaryForInputModule( m ),
               () => this.CreateTableIndexMappingForInputModule( m, reorderResult )
               ) )
            );
      }

      private void FixTargetAssemblyReferences()
      {
         // We have to fix target assembly references, because if we are merging non-PCL library with PCL library, and non-PCL lib implements interface from PCL lib, their signatures may differ if assembly refs are not processed.
         // This causes PEVerify errors.
         var retargetableInfos = new HashSet<Int32>();

         if ( !this._options.SkipFixingAssemblyReferences )
         {
            var targetFW = new Lazy<TargetFrameworkInfo>( () =>
            {
               var fw = this._targetModule.GetTargetFrameworkInformationOrNull( this._moduleLoader.CreateNewResolver() );
               if ( fw == null )
               {
                  throw this.NewCILMergeException( ExitCode.NoTargetFrameworkSpecified, "TODO: allow specifying target framework info (id, version, profile) through options." );
               }
               return fw;
            }, LazyThreadSafetyMode.None );

            foreach ( var inputModule in this._inputModules )
            {
               var aRefs = inputModule.AssemblyReferences;
               var inputMappings = this._tableIndexMappings[inputModule];
               for ( var i = 0; i < aRefs.Count; ++i )
               {
                  TableIndex targetAssemblyRefIndex;
                  if ( inputMappings.TryGetValue( new TableIndex( Tables.AssemblyRef, i ), out targetAssemblyRefIndex ) )
                  {
                     var aRef = aRefs[i];
                     if ( aRef.Attributes.IsRetargetable() )
                     {
                        var aInfo = aRef.AssemblyInformation;
                        var correspondingNewAssembly = this._moduleLoader.GetOrLoadMetaData( Path.Combine( this._loaderCallbacks.GetTargetFrameworkPathForFrameworkInfo( targetFW.Value ), aInfo.Name + ".dll" ) );
                        var targetARef = this._targetModule.AssemblyReferences[targetAssemblyRefIndex.Index];
                        if ( !targetFW.Value.AreFrameworkAssemblyReferencesRetargetable )
                        {
                           targetARef.Attributes &= ( ~AssemblyFlags.Retargetable );
                        }
                        var aDefInfo = correspondingNewAssembly.AssemblyDefinitions[0].AssemblyInformation;
                        aDefInfo.DeepCopyContentsTo( targetARef.AssemblyInformation );
                        if ( this._options.UseFullPublicKeyForRefs )
                        {
                           targetARef.Attributes |= AssemblyFlags.PublicKey;
                        }
                        else if ( !targetARef.Attributes.IsFullPublicKey() )
                        {
                           targetARef.AssemblyInformation.PublicKeyOrToken = this._publicKeyComputer.Value.ComputePublicKeyToken( aDefInfo.PublicKeyOrToken );
                        }
                        retargetableInfos.Add( targetAssemblyRefIndex.Index );
                     }
                  }
               }
            }
         }

         if ( this._options.UseFullPublicKeyForRefs )
         {
            foreach ( var inputModule in this._inputModules )
            {
               var aRefs = inputModule.AssemblyReferences;
               var inputModulePath = this._moduleLoader.GetResourceFor( inputModule );
               var inputMappings = this._tableIndexMappings[inputModule];
               for ( var i = 0; i < aRefs.Count; ++i )
               {
                  TableIndex targetAssemblyRefIndex;
                  if ( inputMappings.TryGetValue( new TableIndex( Tables.AssemblyRef, i ), out targetAssemblyRefIndex ) )
                  {
                     var aRef = aRefs[i];
                     if ( !aRef.Attributes.IsFullPublicKey() && !retargetableInfos.Contains( targetAssemblyRefIndex.Index ) )
                     {
                        var aRefModule = this._loaderCallbacks.GetPossibleResourcesForAssemblyReference(
                           inputModulePath,
                           inputModule,
                           new AssemblyInformationForResolving( aRef.AssemblyInformation, aRef.Attributes.IsFullPublicKey() ),
                           null )
                        .Where( p => File.Exists( p ) )
                        .Select( p => this._moduleLoader.GetOrLoadMetaData( p ) )
                        .Where( m => m.AssemblyDefinitions.Count > 0 )
                        .FirstOrDefault();

                        if ( aRefModule != null )
                        {
                           var targetARef = this._targetModule.AssemblyReferences[targetAssemblyRefIndex.Index];
                           targetARef.AssemblyInformation.PublicKeyOrToken = aRefModule.AssemblyDefinitions[0].AssemblyInformation.PublicKeyOrToken.CreateBlockCopy();
                           targetARef.Attributes |= AssemblyFlags.PublicKey;
                        }
                     }
                  }
               }
            }
         }
      }

      private IDictionary<String, String> CreateRenameDictionaryForInputModule( CILMetaData inputModule )
      {
         var retVal = new Dictionary<String, String>();
         foreach ( var kvp in this._inputModuleTypeNamesInTargetModule[inputModule] )
         {
            var oldName = kvp.Key;
            var newName = this._targetTypeNames[kvp.Value];
            if ( !String.Equals( oldName, newName ) )
            {
               retVal.Add( oldName, newName );
            }
         }

         return retVal;
      }

      private IDictionary<TableIndex, TableIndex> CreateTableIndexMappingForInputModule( CILMetaData inputModule, Int32[][] reorderingResult )
      {
         var mapping = this._tableIndexMappings[inputModule];
         return mapping.ToDictionary(
            kvp => kvp.Key,
            kvp =>
            {
               var targetIndex = kvp.Value;
               var reorderingMapping = reorderingResult[(Int32) targetIndex.Table];
               return reorderingMapping == null ?
                  targetIndex :
                  new TableIndex( targetIndex.Table, reorderingMapping[targetIndex.Index] );
            } );
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

         paths = paths.Distinct().ToArray();

         this.DoPotentiallyInParallel( paths, ( isRunningInParallel, path ) =>
         {
            var mod = this._moduleLoader.LoadAndResolve( path );
            var rArgs = this._moduleLoader.GetReadingArgumentsForMetaData( mod );
            if ( !rArgs.Headers.ModuleFlags.IsILOnly() && !this._options.ZeroPEKind )
            {
               throw this.NewCILMergeException( ExitCode.NonILOnlyModule, "The module in " + path + " is not IL-only." );
            }
         } );

         this._inputModules.AddRange( paths.Select( p => this._moduleLoader.GetOrLoadMetaData( p ) ) );

         this._primaryModule = this._inputModules[0];

         if ( this._options.Closed )
         {
            var set = new HashSet<CILMetaData>( this._inputModules, ReferenceEqualityComparer<CILMetaData>.ReferenceBasedComparer );
            foreach ( var mod in this._inputModules.ToArray() )
            {
               this.LoadModuleForClosedSet( mod, set );
            }
            set.UnionWith( this._inputModules );
            this._inputModules.Clear();

            // Add primary module as first one
            this._inputModules.Add( this._primaryModule );

            // Add the rest
            set.Remove( this._primaryModule );
            this._inputModules.AddRange( set );
         }

         // Build helper data structures for input modules
         foreach ( var inputModule in this._inputModules.Where( m => m.AssemblyDefinitions.Count > 0 ) )
         {
            var aRef = new AssemblyReference();
            inputModule.AssemblyDefinitions[0].AssemblyInformation.DeepCopyContentsTo( aRef.AssemblyInformation );
            aRef.Attributes = AssemblyFlags.PublicKey;
            this._inputModulesAsAssemblyReferences.Add( aRef, inputModule );
         }

         foreach ( var inputModule in this._inputModules )
         {
            var dic = new Dictionary<String, CILMetaData>();
            foreach ( var f in inputModule.GetModuleFileReferences() )
            {
               var modRefPath = Path.Combine( Path.GetDirectoryName( this._moduleLoader.GetResourceFor( inputModule ) ), f.Name );
               var targetInputModule = this._inputModules
                  .Where( md => md.AssemblyDefinitions.Count == 0 )
                  .FirstOrDefault( md => String.Equals( this._moduleLoader.GetResourceFor( md ), modRefPath ) ); // TODO maybe case-insensitive match??
               if ( targetInputModule != null )
               {
                  dic[f.Name] = targetInputModule;
               }
            }

            this._inputModulesAsModuleReferences.Add( inputModule, dic );

         }
      }

      private void LoadModuleForClosedSet( CILMetaData md, ISet<CILMetaData> curLoaded )
      {
         var thisPath = this._moduleLoader.GetResourceFor( md );

         // Load all modules
         foreach ( var moduleRef in md.FileReferences.Where( f => f.Attributes.ContainsMetadata() ) )
         {
            var path = this._loaderCallbacks
               .GetPossibleResourcesForModuleReference( thisPath, md, moduleRef.Name )
               .FirstOrDefault( p => this._loaderCallbacks.IsValidResource( p ) );
            if ( String.IsNullOrEmpty( path ) )
            {
               this.Log( MessageLevel.Warning, "Failed to find module referenced in " + thisPath + "." );
            }
            else
            {
               var otherModule = this._moduleLoader.LoadAndResolve( path );
               if ( curLoaded.Add( otherModule ) )
               {
                  this.LoadModuleForClosedSet( otherModule, curLoaded );
               }
            }
         }

         // Load all referenced assemblies, except target framework ones.
         var thisDir = Path.GetDirectoryName( thisPath );
         var fwDir = this._loaderCallbacks.GetTargetFrameworkPathFor( md );
         foreach ( var aRef in md.AssemblyReferences )
         {
            var path = this._loaderCallbacks
               .GetPossibleResourcesForAssemblyReference( thisPath, md, aRef.NewInformationForResolving(), null )
               .Where( p => fwDir == null || !p.StartsWith( fwDir ) )
               .FirstOrDefault( p => this._loaderCallbacks.IsValidResource( p ) );
            if ( !String.IsNullOrEmpty( path ) )
            {
               var otherAssembly = this._moduleLoader.LoadAndResolve( path );
               if ( curLoaded.Add( otherAssembly ) )
               {
                  this.LoadModuleForClosedSet( otherAssembly, curLoaded );
               }
            }
         }
      }

      private CILAssemblyManipulator.Physical.EmittingArguments CreateEmittingArgumentsForTargetModule()
      {

         // Prepare strong _name
         var keyFile = this._options.KeyFile;
         CILAssemblyManipulator.Physical.StrongNameKeyPair sn = null;
         if ( keyFile != null )
         {
            try
            {
               sn = new CILAssemblyManipulator.Physical.StrongNameKeyPair( File.ReadAllBytes( keyFile ) );
            }
            catch ( Exception exc )
            {
               throw this.NewCILMergeException( ExitCode.ErrorAccessingSNFile, "Error accessing strong name file " + keyFile + ".", exc );
            }
         }
         else if ( !String.IsNullOrEmpty( this._options.CSPName ) )
         {
            sn = new CILAssemblyManipulator.Physical.StrongNameKeyPair( this._options.CSPName );
         }

         // Prepare emitting arguments
         var pEArgs = this._moduleLoader.GetReadingArgumentsForMetaData( this._primaryModule );
         var pHeaders = pEArgs.Headers;

         var eArgs = new CILAssemblyManipulator.Physical.EmittingArguments();
         var eHeaders = pEArgs.Headers.CreateCopy();
         eArgs.Headers = eHeaders;
         eHeaders.DebugInformation = null;
         eHeaders.FileAlignment = (UInt32) Math.Max( this._options.FileAlign, HeadersData.DEFAULT_FILE_ALIGNMENT );
         eHeaders.SubSysMajor = (UInt16) this._options.SubsystemMajor;
         eHeaders.SubSysMinor = (UInt16) this._options.SubsystemMinor;
         eHeaders.HighEntropyVA = this._options.HighEntropyVA;
         var md = this._options.MetadataVersionString;
         eHeaders.MetaDataVersion = String.IsNullOrEmpty( md ) ? pHeaders.MetaDataVersion : md;

         eArgs.DelaySign = this._options.DelaySign;
         eArgs.SigningAlgorithm = this._options.SigningAlgorithm;

         return eArgs;
      }

      private void CreateTargetAssembly( CILAssemblyManipulator.Physical.EmittingArguments eArgs )
      {
         var outPath = this._options.OutPath;
         var targetMD = CILMetaDataFactory.CreateMinimalAssembly( Path.GetFileNameWithoutExtension( outPath ), Path.GetExtension( outPath ).Substring( 1 ) ); // Skip dot
         var primaryAInfo = this._primaryModule.AssemblyDefinitions[0].AssemblyInformation;
         var aInfo = targetMD.AssemblyDefinitions[0].AssemblyInformation;

         aInfo.Culture = primaryAInfo.Culture;
         if ( eArgs.StrongName != null )
         {
            aInfo.PublicKeyOrToken = eArgs.CryptoCallbacks.CreatePublicKeyFromStrongName( eArgs.StrongName );
         }

         if ( this._options.VerMajor > -1 )
         {
            // Version was specified explictly
            aInfo.VersionMajor = this._options.VerMajor;
            aInfo.VersionMinor = this._options.VerMinor;
            aInfo.VersionBuild = this._options.VerBuild;
            aInfo.VersionRevision = this._options.VerRevision;
         }
         else
         {
            var an2 = this._primaryModule.AssemblyDefinitions.Count > 0 ?
               this._primaryModule.AssemblyDefinitions[0].AssemblyInformation :
               new AssemblyInformation();

            aInfo.VersionMajor = an2.VersionMajor;
            aInfo.VersionMinor = an2.VersionMinor;
            aInfo.VersionBuild = an2.VersionBuild;
            aInfo.VersionRevision = an2.VersionRevision;
         }

         this._targetModule = targetMD;


      }

      // Constructs TypeDef, NestedClass, MethodDef, ParamDef, GenericParameter tables to target module
      private IList<IList<Tuple<CILMetaData, Int32>>> ConstructStructuralTables()
      {
         //var nestedTypeInfo = new Dictionary<CILMetaData, IDictionary<Int32, IList<Int32>>>( ReferenceEqualityComparer<CILMetaData>.ReferenceBasedComparer );
         var enclosingTypeInfo = new Dictionary<CILMetaData, IDictionary<Int32, Int32>>( ReferenceEqualityComparer<CILMetaData>.ReferenceBasedComparer );
         var genericParamInfo = new Dictionary<CILMetaData, IDictionary<TableIndex, IList<Int32>>>( ReferenceEqualityComparer<CILMetaData>.ReferenceBasedComparer );

         // Populate enclosing type info
         foreach ( var md in this._inputModules )
         {
            //var ntDic = new Dictionary<Int32, IList<Int32>>();
            var eDic = new Dictionary<Int32, Int32>();
            var gDic = new Dictionary<TableIndex, IList<Int32>>();
            foreach ( var nt in md.NestedClassDefinitions )
            {
               //ntDic
               //   .GetOrAdd_NotThreadSafe( nt.EnclosingClass.Index, e => new List<Int32>() )
               //   .Add( nt.NestedClass.Index );
               eDic[nt.NestedClass.Index] = nt.EnclosingClass.Index;
            }

            for ( var i = 0; i < md.GenericParameterDefinitions.Count; ++i )
            {
               gDic
                  .GetOrAdd_NotThreadSafe( md.GenericParameterDefinitions[i].Owner, g => new List<Int32>() )
                  .Add( i );
            }

            //nestedTypeInfo.Add( md, ntDic );
            enclosingTypeInfo.Add( md, eDic );
            genericParamInfo.Add( md, gDic );
         }

         // Create type strings
         var typeStrings = new Dictionary<CILMetaData, IDictionary<Int32, String>>( ReferenceEqualityComparer<CILMetaData>.ReferenceBasedComparer );
         foreach ( var md in this._inputModules )
         {
            var thisTypeStrings = new Dictionary<Int32, String>();
            var thisTypeStringInInput = new Dictionary<String, Int32>();
            var thisEnclosingTypeInfo = enclosingTypeInfo[md];

            for ( var tDefIdx = 1; tDefIdx < md.TypeDefinitions.Count; ++tDefIdx ) // Skip <Module> type
            {
               var typeString = CreateTypeString( md.TypeDefinitions, tDefIdx, thisEnclosingTypeInfo );
               thisTypeStrings.Add( tDefIdx, typeString );
               thisTypeStringInInput.Add( typeString, tDefIdx );

            }
            typeStrings.Add( md, thisTypeStrings );
            this._inputModuleTypeNamesInInputModule.Add( md, thisTypeStringInInput );
         }

         // Construct TypeDef table
         var currentlyAddedTypeNames = new Dictionary<String, Int32>();
         ISet<String> allTypeStringsSet = null;
         var targetModule = this._targetModule;
         var targetTypeDefs = targetModule.TypeDefinitions;
         var targetTypeInfo = new List<IList<Tuple<CILMetaData, Int32>>>();
         var targetTypeFullNames = this._targetTypeNames;
         // Add type info for <Module> type
         targetTypeInfo.Add( this._inputModules
            .Where( m => m.TypeDefinitions.Count > 0 )
            .Select( m => Tuple.Create( m, 0 ) )
            .ToList()
            );
         targetTypeFullNames.Add( targetTypeDefs[0].Name );

         foreach ( var md in this._inputModules )
         {
            var thisTypeStrings = typeStrings[md];
            var thisEnclosingTypeInfo = enclosingTypeInfo[md];
            var thisModuleMapping = new Dictionary<TableIndex, TableIndex>();
            var thisTypeStringInfo = new Dictionary<String, Int32>();

            // Add <Module> type mapping and info
            thisModuleMapping.Add( new TableIndex( Tables.TypeDef, 0 ), new TableIndex( Tables.TypeDef, 0 ) );

            // Process other types
            for ( var tDefIdx = 1; tDefIdx < md.TypeDefinitions.Count; ++tDefIdx ) // Skip <Module> type
            {
               var targetTDefIdx = targetTypeDefs.Count;
               var tDef = md.TypeDefinitions[tDefIdx];
               var typeStr = thisTypeStrings[tDefIdx];
               var added = currentlyAddedTypeNames.TryAdd_NotThreadSafe( typeStr, targetTDefIdx );
               var typeAttrs = this.GetNewTypeAttributesForType( md, tDefIdx, thisEnclosingTypeInfo.ContainsKey( tDefIdx ), typeStr );
               var newName = tDef.Name;
               IList<Tuple<CILMetaData, Int32>> thisMergedTypes;
               var thisTypeFullName = typeStr;
               if ( added || this.IsDuplicateOK( ref newName, typeStr, ref thisTypeFullName, typeAttrs, typeStrings, ref allTypeStringsSet ) )
               {
                  targetTypeDefs.Add( new TypeDefinition()
                  {
                     Name = newName,
                     Namespace = tDef.Namespace,
                     Attributes = typeAttrs
                  } );
                  thisMergedTypes = new List<Tuple<CILMetaData, Int32>>();
                  targetTypeInfo.Add( thisMergedTypes );
                  targetTypeFullNames.Add( thisTypeFullName );
               }
               else if ( this._options.Union )
               {
                  targetTDefIdx = currentlyAddedTypeNames[typeStr];
                  thisMergedTypes = targetTypeInfo[targetTDefIdx];
               }
               else
               {
                  throw this.NewCILMergeException( ExitCode.DuplicateTypeName, "The type " + typeStr + " appears in more than one assembly." );
               }

               thisModuleMapping.Add( new TableIndex( Tables.TypeDef, tDefIdx ), new TableIndex( Tables.TypeDef, targetTDefIdx ) );
               thisMergedTypes.Add( Tuple.Create( md, tDefIdx ) );
               thisTypeStringInfo.Add( typeStr, targetTDefIdx );
            }

            this._tableIndexMappings.Add( md, thisModuleMapping );
            this._inputModuleTypeNamesInTargetModule.Add( md, thisTypeStringInfo );
         }

         // Construct GenericParameter, NestedClass, FieldDef, MethodDef, ParamDef tables
         var targetTableIndexMappings = this._targetTableIndexMappings;
         for ( var tDefIdx = 0; tDefIdx < targetTypeDefs.Count; ++tDefIdx )
         {
            var thisTypeInfo = targetTypeInfo[tDefIdx];
            var gParameters = thisTypeInfo.Select( t =>
               {
                  IList<Int32> gParams;
                  genericParamInfo[t.Item1].TryGetValue( new TableIndex( Tables.TypeDef, t.Item2 ), out gParams );
                  return gParams;
               } )
               .ToArray();

            if ( !gParameters
               .Select( gParams => gParams == null ? 0 : gParams.Count )
               .EmptyOrAllEqual()
               )
            {
               var first = thisTypeInfo[0];
               throw this.NewCILMergeException( ExitCode.VariableTypeGenericParameterCount, "Type " + typeStrings[first.Item1][first.Item2] + " has different amount of generic arguments in different modules." );
            }
            else if ( gParameters[0] != null )
            {
               // GenericParameter, for type
               // Just use generic parameters from first suitable input module
               var gParamModule = thisTypeInfo[0].Item1;
               foreach ( var gParamIdx in gParameters[0] )
               {
                  var targetGParamIdx = new TableIndex( Tables.GenericParameter, targetModule.GenericParameterDefinitions.Count );
                  targetTableIndexMappings.Add( targetGParamIdx, Tuple.Create( gParamModule, gParamIdx ) );
                  this._tableIndexMappings[gParamModule].Add( new TableIndex( Tables.GenericParameter, gParamIdx ), targetGParamIdx );
                  var gParam = gParamModule.GenericParameterDefinitions[gParamIdx];
                  targetModule.GenericParameterDefinitions.Add( new GenericParameterDefinition()
                  {
                     Attributes = gParam.Attributes,
                     GenericParameterIndex = gParam.GenericParameterIndex,
                     Name = gParam.Name,
                     Owner = new TableIndex( Tables.TypeDef, tDefIdx )
                  } );
               }
            }

            var tDef = targetTypeDefs[tDefIdx];

            foreach ( var typeInfo in thisTypeInfo )
            {
               var inputMD = typeInfo.Item1;
               var inputTDefIdx = typeInfo.Item2;
               var thisGenericParamInfo = genericParamInfo[inputMD];
               var thisTableMappings = this._tableIndexMappings[inputMD];

               // NestedClass
               var thisEnclosingInfo = enclosingTypeInfo[inputMD];
               Int32 enclosingTypeIdx;
               if ( thisEnclosingInfo.TryGetValue( inputTDefIdx, out enclosingTypeIdx ) )
               {
                  targetModule.NestedClassDefinitions.Add( new NestedClassDefinition()
                  {
                     NestedClass = new TableIndex( Tables.TypeDef, tDefIdx ),
                     EnclosingClass = thisTableMappings[new TableIndex( Tables.TypeDef, enclosingTypeIdx )]
                  } );
               }

               // FieldDef
               tDef.FieldList = new TableIndex( Tables.Field, targetModule.FieldDefinitions.Count );
               foreach ( var fDefIdx in inputMD.GetTypeFieldIndices( inputTDefIdx ) )
               {
                  var targetFIdx = new TableIndex( Tables.Field, targetModule.FieldDefinitions.Count );
                  targetTableIndexMappings.Add( targetFIdx, Tuple.Create( inputMD, fDefIdx ) );
                  thisTableMappings.Add( new TableIndex( Tables.Field, fDefIdx ), targetFIdx );
                  var fDef = inputMD.FieldDefinitions[fDefIdx];
                  targetModule.FieldDefinitions.Add( new FieldDefinition()
                  {
                     Attributes = fDef.Attributes,
                     Name = fDef.Name,
                  } );
               }

               // MethodDef
               tDef.MethodList = new TableIndex( Tables.MethodDef, targetModule.MethodDefinitions.Count );
               foreach ( var mDefIdx in inputMD.GetTypeMethodIndices( inputTDefIdx ) )
               {
                  var targetMDefIdx = targetModule.MethodDefinitions.Count;
                  targetTableIndexMappings.Add( new TableIndex( Tables.MethodDef, targetMDefIdx ), Tuple.Create( inputMD, mDefIdx ) );
                  thisTableMappings.Add( new TableIndex( Tables.MethodDef, mDefIdx ), new TableIndex( Tables.MethodDef, targetMDefIdx ) );
                  var mDef = inputMD.MethodDefinitions[mDefIdx];
                  targetModule.MethodDefinitions.Add( new MethodDefinition()
                  {
                     Attributes = mDef.Attributes,
                     ImplementationAttributes = mDef.ImplementationAttributes,
                     Name = mDef.Name,
                     ParameterList = new TableIndex( Tables.Parameter, targetModule.ParameterDefinitions.Count ),
                  } );

                  // GenericParameter, for method
                  IList<Int32> methodGParams;
                  if ( thisGenericParamInfo.TryGetValue( new TableIndex( Tables.MethodDef, mDefIdx ), out methodGParams ) )
                  {
                     foreach ( var methodGParamIdx in methodGParams )
                     {
                        var targetGParamIdx = new TableIndex( Tables.GenericParameter, targetModule.GenericParameterDefinitions.Count );
                        targetTableIndexMappings.Add( targetGParamIdx, Tuple.Create( inputMD, methodGParamIdx ) );
                        thisTableMappings.Add( new TableIndex( Tables.GenericParameter, methodGParamIdx ), targetGParamIdx );
                        var methodGParam = inputMD.GenericParameterDefinitions[methodGParamIdx];
                        targetModule.GenericParameterDefinitions.Add( new GenericParameterDefinition()
                        {
                           Attributes = methodGParam.Attributes,
                           GenericParameterIndex = methodGParam.GenericParameterIndex,
                           Name = methodGParam.Name,
                           Owner = new TableIndex( Tables.MethodDef, targetMDefIdx )
                        } );
                     }
                  }

                  // ParamDef
                  foreach ( var pDefIdx in inputMD.GetMethodParameterIndices( mDefIdx ) )
                  {
                     var targetPIdx = new TableIndex( Tables.Parameter, targetModule.ParameterDefinitions.Count );
                     targetTableIndexMappings.Add( targetPIdx, Tuple.Create( inputMD, pDefIdx ) );
                     thisTableMappings.Add( new TableIndex( Tables.Parameter, pDefIdx ), targetPIdx );
                     var pDef = inputMD.ParameterDefinitions[pDefIdx];
                     targetModule.ParameterDefinitions.Add( new ParameterDefinition()
                     {
                        Attributes = pDef.Attributes,
                        Name = pDef.Name,
                        Sequence = pDef.Sequence
                     } );
                  }
               }
            }
         }

         return targetTypeInfo;
      }

      private void ConstructTablesUsedInSignaturesAndILTokens(
         IList<IList<Tuple<CILMetaData, Int32>>> targetTypeInfo,
         EmittingArguments eArgs
         )
      {
         // AssemblyRef (used by MemberRef table)
         this.MergeTables(
            Tables.AssemblyRef,
            md => md.AssemblyReferences,
            ( md, inputIdx, thisIdx ) => !this._inputModulesAsAssemblyReferences.ContainsKey( md.AssemblyReferences[inputIdx.Index] ),
            ( md, aRef, inputIdx, thisIdx ) =>
            {
               var newARef = new AssemblyReference()
               {
                  Attributes = aRef.Attributes,
                  HashValue = aRef.HashValue
               };
               aRef.AssemblyInformation.DeepCopyContentsTo( newARef.AssemblyInformation );
               return newARef;
            } );

         // ModuleRef (used by MemberRef table)
         // Skip the actual module references (but keep the ones from ImplMap table)
         this.MergeTables(
            Tables.ModuleRef,
            md => md.ModuleReferences,
            ( md, inputIdx, thisIdx ) => !this._inputModulesAsModuleReferences[md].ContainsKey( md.ModuleReferences[inputIdx.Index].ModuleName ),
            ( md, mRef, inputIdx, thisIdx ) => new ModuleReference()
            {
               ModuleName = mRef.ModuleName
            } );

         // TypeRef (used by signatures/IL tokens)
         var typeRefInputModules = new Dictionary<CILMetaData, IDictionary<Int32, Tuple<CILMetaData, String>>>();
         this.MergeTables(
            Tables.TypeRef,
            md => md.TypeReferences,
            ( md, inputIdx, thisIdx ) =>
            {
               // Check whether this will be a TypeRef or TypeDef row in target module
               String typeString; CILMetaData refModule;
               var tDefIdx = this.GetTargetModuleTypeDefIndexForInputModuleTypeRef( md, inputIdx, thisIdx, out typeString, out refModule );
               var retVal = tDefIdx < 0;
               if ( !retVal )
               {
                  // This type ref is actually a type def in target module
                  this._tableIndexMappings[md].Add( inputIdx, new TableIndex( Tables.TypeDef, tDefIdx ) );
                  typeRefInputModules
                     .GetOrAdd_NotThreadSafe( md, mdd => new Dictionary<Int32, Tuple<CILMetaData, String>>() )
                     .Add( inputIdx.Index, Tuple.Create( refModule, typeString ) );
               }
               return retVal;
            },
            ( md, tRef, inputIdx, thisIdx ) => new TypeReference()
            {
               Name = tRef.Name,
               Namespace = tRef.Namespace,
            } );

         // Non-null ResolutionScope indexes:
         // Module (already processed)
         // ModuleRef (already processed)
         // AssemblyRef (already processed)
         // TypeRef (just processed)
         // Update TypeRef.ResolutionScope separately
         this.SetTableIndicesNullable(
            Tables.TypeRef,
            md => md.TypeReferences,
            tRef => tRef.ResolutionScope,
            ( tRef, resScope ) => tRef.ResolutionScope = resScope
            );

         // TypeSpec (used by signatures/IL tokens)
         this.MergeTables(
            Tables.TypeSpec,
            md => md.TypeSpecifications,
            null,
            ( md, tSpec, inputIdx, thisIdx ) => new TypeSpecification()
            {

            } );

         // MemberRef (used by IL tokens)
         // DeclaringType indexes:
         // MethodDef (already processed)
         // ModuleRef (already processed, possibly missing rows)
         // TypeDef (already processed)
         // TypeRef (already processed)
         // TypeSpec (already processed)
         // Update MemberReference.DeclaringType in place.
         this.MergeTables(
            Tables.MemberRef,
            md => md.MemberReferences,
            ( md, inputIdx, thisIdx ) =>
            {
               // If member ref declaring type ends up being, replace with corresponding MethodDef/FieldDef reference
               var mRef = md.MemberReferences[inputIdx.Index];
               var thisMappings = this._tableIndexMappings[md];
               var declType = mRef.DeclaringType;
               var targetDeclaringType = thisMappings[declType];
               var isActuallyInTargetModule = declType.Table == Tables.TypeRef && targetDeclaringType.Table == Tables.TypeDef;
               if ( isActuallyInTargetModule )
               {
                  var mRefName = mRef.Name;
                  var targetModule = this._targetModule;
                  var targetFields = targetModule.FieldDefinitions;

                  Tables targetMRefTable;
                  Int32 targetMRefIndex;
                  switch ( mRef.Signature.SignatureKind )
                  {
                     case SignatureKind.Field:
                        // Match simply by name
                        targetMRefTable = Tables.Field;
                        targetMRefIndex = this._targetModule
                           .GetTypeFieldIndices( targetDeclaringType.Index )
                           .Where( fi => String.Equals( mRefName, this._targetModule.FieldDefinitions[fi].Name ) )
                           .FirstOrDefaultCustom( -1 );
                        break;
                     case SignatureKind.MethodReference:
                        // Match by name and signature
                        targetMRefTable = Tables.MethodDef;
                        var moduleContainingMethodDefInfo = typeRefInputModules[md][declType.Index];
                        var moduleContainingMethodDef = moduleContainingMethodDefInfo.Item1;
                        var methodDefContainingTypeIndex = this._inputModuleTypeNamesInInputModule[moduleContainingMethodDef][moduleContainingMethodDefInfo.Item2];
                        targetMRefIndex = moduleContainingMethodDef.GetTypeMethodIndices( methodDefContainingTypeIndex )
                           .Where( mi => String.Equals( mRefName, moduleContainingMethodDef.MethodDefinitions[mi].Name ) && this.MatchTargetMethodSignatureToMemberRefMethodSignature( moduleContainingMethodDef, md, moduleContainingMethodDef.MethodDefinitions[mi].Signature, (MethodReferenceSignature) mRef.Signature ) )
                           .FirstOrDefaultCustom( -1 );
                        if ( targetMRefIndex >= 0 )
                        {
                           targetMRefIndex = this._tableIndexMappings[moduleContainingMethodDef][new TableIndex( Tables.MethodDef, targetMRefIndex )].Index;
                        }
                        break;
                     default:
                        targetMRefTable = (Tables) Byte.MaxValue;
                        targetMRefIndex = -1;
                        break;

                  }

                  if ( targetMRefIndex == -1 )
                  {
                     throw this.NewCILMergeException( ExitCode.UnresolvableMemberReferenceToAnotherInputModule, "Unresolvable member reference in module " + this._moduleLoader.GetResourceFor( md ) + " at index " + inputIdx.Index + " to another input module" );
                  }

                  thisMappings.Add( inputIdx, new TableIndex( targetMRefTable, targetMRefIndex ) );
               }

               return !isActuallyInTargetModule;
            },
            ( md, mRef, inputIdx, thisIdx ) => new MemberReference()
            {
               DeclaringType = mRef.DeclaringType.Table == Tables.ModuleRef && !this._tableIndexMappings[md].ContainsKey( mRef.DeclaringType ) ?
                  new TableIndex( Tables.TypeDef, 0 ) :
                  this.TranslateTableIndex( md, mRef.DeclaringType ),
               Name = mRef.Name
            } );

         // MethodSpec (used by IL tokens)
         // Method indexes:
         // MethodDef (already processed)
         // MemberRef (already processed)
         // Update MethodSpec.Method in place
         this.MergeTables(
            Tables.MethodSpec,
            md => md.MethodSpecifications,
            null,
            ( md, mSpec, inputIdx, thisIdx ) => new MethodSpecification()
            {
               Method = this.TranslateTableIndex( md, mSpec.Method )
            } );

         // StandaloneSignature (used by IL tokens)
         this.MergeTables(
            Tables.StandaloneSignature,
            md => md.StandaloneSignatures,
            null,
            ( md, sig, inputIdx, thisIdx ) => new StandaloneSignature()
            {

            } );

         // Revisit TypeDef table and process its BaseType references
         // TypeDef.BaseType indexes:
         // TypeDef (already processed)
         // TypeRef (already processed)
         // TypeSpec (already processed)
         // Update TypeDef.BaseType separately
         this.SetTableIndicesNullable(
            Tables.TypeDef,
            md => md.TypeDefinitions,
            tDefIdx => targetTypeInfo[tDefIdx][0], // TODO check that base types are 'same' (would involve creating assembly-qualified type string, and even then should take into account the effects of Retargetable assembly ref attribute)
            tDef => tDef.BaseType,
            ( tDef, bType ) => tDef.BaseType = bType
            );
      }

      private Boolean MatchTargetMethodSignatureToMemberRefMethodSignature( CILMetaData defModule, CILMetaData refModule, AbstractMethodSignature methodDef, AbstractMethodSignature methodRef )
      {
         return methodDef.SignatureStarter == methodRef.SignatureStarter
            && methodDef.Parameters.Count == methodRef.Parameters.Count
            && methodDef.Parameters
               .Where( ( p, idx ) => this.MatchTargetParameterSignatureToMemberRefParameterSignature( defModule, refModule, p, methodRef.Parameters[idx] ) )
               .Count() == methodDef.Parameters.Count
            && this.MatchTargetParameterSignatureToMemberRefParameterSignature( defModule, refModule, methodDef.ReturnType, methodRef.ReturnType );
      }

      private Boolean MatchTargetParameterSignatureToMemberRefParameterSignature( CILMetaData defModule, CILMetaData refModule, ParameterSignature paramDef, ParameterSignature paramRef )
      {
         return paramDef.IsByRef == paramRef.IsByRef
            && this.MatchTargetCustomModsToMemberRefCustomMods( defModule, refModule, paramDef.CustomModifiers, paramRef.CustomModifiers )
            && this.MatchTargetTypeSignatureToMemberRefTypeSignature( defModule, refModule, paramDef.Type, paramRef.Type );
      }

      private Boolean MatchTargetCustomModsToMemberRefCustomMods( CILMetaData defModule, CILMetaData refModule, List<CustomModifierSignature> cmDef, List<CustomModifierSignature> cmRef )
      {
         return cmDef.Count == cmRef.Count
            && cmDef
               .Where( ( c, idx ) => c.IsOptional == cmRef[idx].IsOptional && this.MatchTargetSignatureTypeToMemberRefSignatureType( defModule, refModule, c.CustomModifierType, cmRef[idx].CustomModifierType ) )
               .Count() == cmDef.Count;
      }

      private Boolean MatchTargetTypeSignatureToMemberRefTypeSignature( CILMetaData defModule, CILMetaData refModule, TypeSignature typeDef, TypeSignature typeRef )
      {
         var retVal = typeDef.TypeSignatureKind == typeRef.TypeSignatureKind;
         if ( retVal )
         {
            switch ( typeDef.TypeSignatureKind )
            {
               case TypeSignatureKind.ClassOrValue:
                  var classDef = (ClassOrValueTypeSignature) typeDef;
                  var classRef = (ClassOrValueTypeSignature) typeRef;
                  var gArgsDef = classDef.GenericArguments;
                  var gArgsRef = classRef.GenericArguments;
                  retVal = classDef.IsClass == classRef.IsClass
                     && this.MatchTargetSignatureTypeToMemberRefSignatureType( defModule, refModule, classDef.Type, classRef.Type )
                     && gArgsDef.Count == gArgsRef.Count
                     && gArgsDef
                        .Where( ( g, idx ) => this.MatchTargetTypeSignatureToMemberRefTypeSignature( defModule, refModule, g, gArgsRef[idx] ) )
                        .Count() == gArgsDef.Count;
                  break;
               case TypeSignatureKind.ComplexArray:
                  var arrayDef = (ComplexArrayTypeSignature) typeDef;
                  var arrayRef = (ComplexArrayTypeSignature) typeRef;
                  retVal = arrayDef.Rank == arrayRef.Rank
                     && ListEqualityComparer<List<Int32>, Int32>.DefaultListEqualityComparer.Equals( arrayDef.Sizes, arrayRef.Sizes )
                     && ListEqualityComparer<List<Int32>, Int32>.DefaultListEqualityComparer.Equals( arrayDef.LowerBounds, arrayRef.LowerBounds )
                     && this.MatchTargetTypeSignatureToMemberRefTypeSignature( defModule, refModule, arrayDef.ArrayType, arrayRef.ArrayType );
                  break;
               case TypeSignatureKind.FunctionPointer:
                  retVal = this.MatchTargetMethodSignatureToMemberRefMethodSignature( defModule, refModule, ( (FunctionPointerTypeSignature) typeDef ).MethodSignature, ( (FunctionPointerTypeSignature) typeRef ).MethodSignature );
                  break;
               case TypeSignatureKind.GenericParameter:
                  var gDef = (GenericParameterTypeSignature) typeDef;
                  var gRef = (GenericParameterTypeSignature) typeRef;
                  retVal = gDef.IsTypeParameter == gRef.IsTypeParameter
                     && gDef.GenericParameterIndex == gRef.GenericParameterIndex;
                  break;
               case TypeSignatureKind.Pointer:
                  var ptrDef = (PointerTypeSignature) typeDef;
                  var ptrRef = (PointerTypeSignature) typeRef;
                  retVal = this.MatchTargetCustomModsToMemberRefCustomMods( defModule, refModule, ptrDef.CustomModifiers, ptrRef.CustomModifiers )
                     && this.MatchTargetTypeSignatureToMemberRefTypeSignature( defModule, refModule, ptrDef.PointerType, ptrRef.PointerType );
                  break;
               case TypeSignatureKind.Simple:
                  retVal = ( (SimpleTypeSignature) typeDef ).SimpleType == ( (SimpleTypeSignature) typeRef ).SimpleType;
                  break;
               case TypeSignatureKind.SimpleArray:
                  var szArrayDef = (SimpleArrayTypeSignature) typeDef;
                  var szArrayRef = (SimpleArrayTypeSignature) typeRef;
                  retVal = this.MatchTargetCustomModsToMemberRefCustomMods( defModule, refModule, szArrayDef.CustomModifiers, szArrayRef.CustomModifiers )
                     && this.MatchTargetTypeSignatureToMemberRefTypeSignature( defModule, refModule, szArrayDef.ArrayType, szArrayRef.ArrayType );
                  break;
               default:
                  retVal = false;
                  break;
               //throw this.NewCILMergeException( ExitCode.ErrorMatchingMemberReferenceSignature, "Encountered unrecognized type signature kind: " + typeDef.TypeSignatureKind + "." );
            }
         }

         return retVal;
      }

      private Boolean MatchTargetSignatureTypeToMemberRefSignatureType( CILMetaData defModule, CILMetaData refModule, TableIndex defIdx, TableIndex refIdx )
      {
         switch ( defIdx.Table )
         {
            case Tables.TypeDef:
               return refIdx.Table == Tables.TypeRef && this._tableIndexMappings[defModule][defIdx] == this._tableIndexMappings[refModule][refIdx];
            case Tables.TypeRef:
               return refIdx.Table == Tables.TypeRef && this.MatchDefTypeRefToRefTypeRef( defModule, refModule, defIdx.Index, refIdx.Index );
            case Tables.TypeSpec:
               return refIdx.Table == Tables.TypeSpec && this.MatchTargetTypeSignatureToMemberRefTypeSignature( defModule, refModule, defModule.TypeSpecifications[defIdx.Index].Signature, refModule.TypeSpecifications[refIdx.Index].Signature );
            default:
               return false;
         }
      }

      private Boolean MatchDefTypeRefToRefTypeRef( CILMetaData defModule, CILMetaData refModule, Int32 defIdx, Int32 refIdx )
      {
         var defTypeRef = defModule.TypeReferences[defIdx];
         var refTypeRef = refModule.TypeReferences[refIdx];
         var retVal = String.Equals( defTypeRef.Name, refTypeRef.Name )
            && String.Equals( defTypeRef.Namespace, refTypeRef.Namespace );
         if ( retVal )
         {
            var defResScopeNullable = defTypeRef.ResolutionScope;
            var refResScopeNullable = refTypeRef.ResolutionScope;
            if ( defResScopeNullable.HasValue == refResScopeNullable.HasValue )
            {
               if ( defResScopeNullable.HasValue )
               {
                  var defResScope = defResScopeNullable.Value;
                  var refResScope = refResScopeNullable.Value;
                  switch ( defResScope.Table )
                  {
                     case Tables.TypeRef:
                        retVal = refResScope.Table == Tables.TypeRef
                           && this.MatchDefTypeRefToRefTypeRef( defModule, refModule, defResScope.Index, refResScope.Index );
                        break;
                     case Tables.AssemblyRef:
                        retVal = refResScope.Table == Tables.AssemblyRef
                           && this._tableIndexMappings[defModule].ContainsKey( defResScope ) == this._tableIndexMappings[refModule].ContainsKey( refResScope );
                        if ( retVal && this._tableIndexMappings[defModule].ContainsKey( defResScope ) )
                        {
                           retVal = defModule.AssemblyReferences[defResScope.Index].AssemblyInformation.Equals( refModule.AssemblyReferences[refResScope.Index].AssemblyInformation );
                        }
                        break;
                     case Tables.Module:
                     case Tables.ModuleRef:
                        retVal = refResScope.Table == Tables.AssemblyRef
                           && !this._tableIndexMappings[refModule].ContainsKey( refResScope );
                        break;
                     default:
                        retVal = false;
                        break;
                  }

               }
               else
               {
                  // TODO Lazy mapping IDictionary<Tuple<String, String>, ExportedType> for each input module
                  throw new NotImplementedException( "ExportedType in TypeRef while matching method definition and reference signatures." );
               }
            }
         }

         return retVal;
      }

      private Int32 GetTargetModuleTypeDefIndexForInputModuleTypeRef(
         CILMetaData inputModule,
         TableIndex inputIndex,
         TableIndex targetIndex,
         out String thisTypeString,
         out CILMetaData referencedModule
         )
      {
         var tRef = inputModule.TypeReferences[inputIndex.Index];
         var resScopeNullable = tRef.ResolutionScope;
         var retVal = -1;
         thisTypeString = null;
         referencedModule = null;
         if ( resScopeNullable.HasValue )
         {
            var resScope = resScopeNullable.Value;
            switch ( resScope.Table )
            {
               case Tables.TypeRef:
                  retVal = this.GetTargetModuleTypeDefIndexForInputModuleTypeRef( inputModule, resScope, targetIndex, out thisTypeString, out referencedModule );
                  if ( referencedModule != null && retVal >= 0 )
                  {
                     retVal = this._inputModuleTypeNamesInTargetModule[referencedModule][thisTypeString + "+" + tRef.Name];
                  }
                  break;
               case Tables.ModuleRef:
                  if ( !this._tableIndexMappings[inputModule].ContainsKey( resScope ) )
                  {
                     referencedModule = this._inputModulesAsModuleReferences[inputModule][inputModule.ModuleReferences[resScope.Index].ModuleName];
                  }
                  break;
               case Tables.AssemblyRef:
                  if ( !this._tableIndexMappings[inputModule].ContainsKey( resScope ) )
                  {
                     referencedModule = this._inputModulesAsAssemblyReferences[inputModule.AssemblyReferences[resScope.Index]];
                  }
                  break;
               case Tables.Module:
                  referencedModule = inputModule;
                  break;
            }

            if ( referencedModule != null && retVal == -1 )
            {
               thisTypeString = CreateTypeStringFromTopLevelType( tRef.Namespace, tRef.Name );
               retVal = this._inputModuleTypeNamesInTargetModule[referencedModule][thisTypeString];
            }
         }
         else
         {
            throw new NotImplementedException( "ExportedType as ResolutionScope in TypeRef table." );
         }

         return retVal;
      }

      private void ConstructSignaturesAndMethodIL()
      {
         // 1. Create all tables which are still unprocessed, and have signatures.
         this.MergeTables(
            Tables.Property,
            md => md.PropertyDefinitions,
            null,
            ( md, pDef, inputIdx, thisIdx ) => new PropertyDefinition()
            {
               Attributes = pDef.Attributes,
               Name = pDef.Name
            } );

         // CustomAttribute and DeclarativeSecurity signatures do not reference table indices, so they can be skipped

         // 2. Create all signatures
         // Signatures reference only TypeDef, TypeRef, and TypeSpec tables, all of which should've been processed in ConstructNonStructuralTablesUsedInSignaturesAndILTokens method
         var targetModule = this._targetModule;
         // FieldDef
         for ( var i = 0; i < targetModule.FieldDefinitions.Count; ++i )
         {
            var inputInfo = this._targetTableIndexMappings[new TableIndex( Tables.Field, i )];
            var inputModule = inputInfo.Item1;
            var thisMappings = this._tableIndexMappings[inputModule];
            targetModule.FieldDefinitions[i].Signature =
               inputModule.FieldDefinitions[inputInfo.Item2].Signature.CreateDeepCopy( tIdx => thisMappings[tIdx] );
         }

         // MethodDef
         for ( var i = 0; i < targetModule.MethodDefinitions.Count; ++i )
         {
            var inputInfo = this._targetTableIndexMappings[new TableIndex( Tables.MethodDef, i )];
            var inputModule = inputInfo.Item1;
            var thisMappings = this._tableIndexMappings[inputModule];
            var inputMethodDef = inputModule.MethodDefinitions[inputInfo.Item2];
            var targetMethodDef = targetModule.MethodDefinitions[i];
            targetMethodDef.Signature = inputMethodDef.Signature.CreateDeepCopy( tIdx => thisMappings[tIdx] );

            // Create IL
            // IL tokens reference only TypeDef, TypeRef, TypeSpec, MethodDef, FieldDef, MemberRef, MethodSpec or StandaloneSignature tables, all of which should've been processed in ConstructNonStructuralTablesUsedInSignaturesAndILTokens method
            var inputIL = inputMethodDef.IL;
            if ( inputIL != null )
            {
               var targetIL = new MethodILDefinition( inputIL.ExceptionBlocks.Count, inputIL.OpCodes.Count );
               targetMethodDef.IL = targetIL;
               targetIL.ExceptionBlocks.AddRange( inputIL.ExceptionBlocks.Select( eb => new MethodExceptionBlock()
               {
                  BlockType = eb.BlockType,
                  ExceptionType = eb.ExceptionType.HasValue ? thisMappings[eb.ExceptionType.Value] : (TableIndex?) null,
                  FilterOffset = eb.FilterOffset,
                  HandlerLength = eb.HandlerLength,
                  HandlerOffset = eb.HandlerOffset,
                  TryLength = eb.TryLength,
                  TryOffset = eb.TryOffset
               } ) );
               targetIL.InitLocals = inputIL.InitLocals;
               targetIL.LocalsSignatureIndex = inputIL.LocalsSignatureIndex.HasValue ? thisMappings[inputIL.LocalsSignatureIndex.Value] : (TableIndex?) null;
               targetIL.MaxStackSize = inputIL.MaxStackSize;
               targetIL.OpCodes.AddRange( inputIL.OpCodes.Select<OpCodeInfo, OpCodeInfo>( oc =>
               {
                  switch ( oc.InfoKind )
                  {
                     case OpCodeOperandKind.OperandInteger:
                        return new OpCodeInfoWithInt32( oc.OpCode, ( (OpCodeInfoWithInt32) oc ).Operand );
                     case OpCodeOperandKind.OperandInteger64:
                        return new OpCodeInfoWithInt64( oc.OpCode, ( (OpCodeInfoWithInt64) oc ).Operand );
                     case OpCodeOperandKind.OperandNone:
                        return new OpCodeInfoWithNoOperand( oc.OpCode );
                     case OpCodeOperandKind.OperandR4:
                        return new OpCodeInfoWithSingle( oc.OpCode, ( (OpCodeInfoWithSingle) oc ).Operand );
                     case OpCodeOperandKind.OperandR8:
                        return new OpCodeInfoWithDouble( oc.OpCode, ( (OpCodeInfoWithDouble) oc ).Operand );
                     case OpCodeOperandKind.OperandString:
                        return new OpCodeInfoWithString( oc.OpCode, ( (OpCodeInfoWithString) oc ).Operand );
                     case OpCodeOperandKind.OperandSwitch:
                        var ocSwitch = (OpCodeInfoWithSwitch) oc;
                        var ocSwitchTarget = new OpCodeInfoWithSwitch( oc.OpCode, ocSwitch.Offsets.Count );
                        ocSwitchTarget.Offsets.AddRange( ocSwitch.Offsets );
                        return ocSwitchTarget;
                     case OpCodeOperandKind.OperandToken:
                        return new OpCodeInfoWithToken( oc.OpCode, thisMappings[( (OpCodeInfoWithToken) oc ).Operand] );
                     default:
                        throw new NotSupportedException( "Unknown op code kind: " + oc.InfoKind + "." );
                  }
               } ) );
            }

         }

         // MemberRef
         for ( var i = 0; i < targetModule.MemberReferences.Count; ++i )
         {
            var inputInfo = this._targetTableIndexMappings[new TableIndex( Tables.MemberRef, i )];
            var inputModule = inputInfo.Item1;
            var thisMappings = this._tableIndexMappings[inputModule];
            targetModule.MemberReferences[i].Signature =
               inputModule.MemberReferences[inputInfo.Item2].Signature.CreateDeepCopy( tIdx => thisMappings[tIdx] );
         }

         // StandaloneSignature
         for ( var i = 0; i < targetModule.StandaloneSignatures.Count; ++i )
         {
            var inputInfo = this._targetTableIndexMappings[new TableIndex( Tables.StandaloneSignature, i )];
            var inputModule = inputInfo.Item1;
            var thisMappings = this._tableIndexMappings[inputModule];
            targetModule.StandaloneSignatures[i].Signature =
               inputModule.StandaloneSignatures[inputInfo.Item2].Signature.CreateDeepCopy( tIdx => thisMappings[tIdx] );
         }

         // PropertyDef
         for ( var i = 0; i < targetModule.PropertyDefinitions.Count; ++i )
         {
            var inputInfo = this._targetTableIndexMappings[new TableIndex( Tables.Property, i )];
            var inputModule = inputInfo.Item1;
            var thisMappings = this._tableIndexMappings[inputModule];
            targetModule.PropertyDefinitions[i].Signature =
               inputModule.PropertyDefinitions[inputInfo.Item2].Signature.CreateDeepCopy( tIdx => thisMappings[tIdx] );
         }

         // TypeSpec
         for ( var i = 0; i < targetModule.TypeSpecifications.Count; ++i )
         {
            var inputInfo = this._targetTableIndexMappings[new TableIndex( Tables.TypeSpec, i )];
            var inputModule = inputInfo.Item1;
            var thisMappings = this._tableIndexMappings[inputModule];
            targetModule.TypeSpecifications[i].Signature =
               inputModule.TypeSpecifications[inputInfo.Item2].Signature.CreateDeepCopy( tIdx => thisMappings[tIdx] );
         }

         // MethodSpecification
         for ( var i = 0; i < targetModule.MethodSpecifications.Count; ++i )
         {
            var inputInfo = this._targetTableIndexMappings[new TableIndex( Tables.MethodSpec, i )];
            var inputModule = inputInfo.Item1;
            var thisMappings = this._tableIndexMappings[inputModule];
            targetModule.MethodSpecifications[i].Signature =
               inputModule.MethodSpecifications[inputInfo.Item2].Signature.CreateDeepCopy( tIdx => thisMappings[tIdx] );
         }

         // CustomAttribute and DeclarativeSecurity signatures do not reference table indices, so they are processed in ConstructTheRestOfTheTables method

      }

      private void ConstructTheRestOfTheTables()
      {
         // EventDef
         this.MergeTables(
            Tables.Event,
            md => md.EventDefinitions,
            null,
            ( md, evt, inputIdx, thisIdx ) => new EventDefinition()
            {
               Attributes = evt.Attributes,
               EventType = this.TranslateTableIndex( md, evt.EventType ), // TypeRef, TypeDef, TypeSpec -> already processed
               Name = evt.Name
            } );

         // EventMap
         this.MergeTables(
            Tables.EventMap,
            md => md.EventMaps,
            null,
            ( md, evtMap, inputIdx, thisIdx ) => new EventMap()
            {
               EventList = this.TranslateTableIndex( md, evtMap.EventList ), // Event -> already processed
               Parent = this.TranslateTableIndex( md, evtMap.Parent ) // TypeDef -> already processed
            } );

         // PropertyMap
         this.MergeTables(
            Tables.PropertyMap,
            md => md.PropertyMaps,
            null,
            ( md, propMap, inputIdx, thisIdx ) => new PropertyMap()
            {
               Parent = this.TranslateTableIndex( md, propMap.Parent ), // TypeDef -> already processed
               PropertyList = this.TranslateTableIndex( md, propMap.PropertyList ) // Property -> already processed
            } );

         // InterfaceImpl
         this.MergeTables(
            Tables.InterfaceImpl,
            md => md.InterfaceImplementations,
            null,
            ( md, impl, inputIdx, thisIdx ) => new InterfaceImplementation()
            {
               Class = this.TranslateTableIndex( md, impl.Class ), // TypeDef -> already processed
               Interface = this.TranslateTableIndex( md, impl.Interface ) // TypeDef/TypeRef/TypeSpec -> already processed
            } );

         // ConstantDef
         this.MergeTables(
            Tables.Constant,
            md => md.ConstantDefinitions,
            null,
            ( md, constant, inputIdx, thisIdx ) => new ConstantDefinition()
            {
               Parent = this.TranslateTableIndex( md, constant.Parent ), // ParamDef/FieldDef/PropertyDef -> already processed
               Type = constant.Type,
               Value = constant.Value
            } );

         // FieldMarshal
         this.MergeTables(
            Tables.FieldMarshal,
            md => md.FieldMarshals,
            null,
            ( md, marshal, inputIdx, thisIdx ) => new FieldMarshal()
            {
               NativeType = this.ProcessMarshalingInfo( md, marshal.NativeType ),
               Parent = this.TranslateTableIndex( md, marshal.Parent ) // ParamDef/FieldDef -> already processed
            } );

         // DeclSecurity
         this.MergeTables(
            Tables.DeclSecurity,
            md => md.SecurityDefinitions,
            null,
            ( md, sec, inputIdx, thisIdx ) =>
            {
               var retVal = new SecurityDefinition( sec.PermissionSets.Count )
               {
                  Action = sec.Action,
                  Parent = this.TranslateTableIndex( md, sec.Parent ) // TypeDef/MethodDef/AssemblyDef -> already processed
               };
               for ( var i = 0; i < sec.PermissionSets.Count; ++i )
               {
                  retVal.PermissionSets.Add( this.ProcessPermissionSet( md, inputIdx.Index, i, sec.PermissionSets[i] ) );
               }
               return retVal;
            } );

         // ClassLayout
         this.MergeTables(
            Tables.ClassLayout,
            md => md.ClassLayouts,
            null,
            ( md, layout, inputIdx, thisIx ) => new ClassLayout()
            {
               ClassSize = layout.ClassSize,
               PackingSize = layout.PackingSize,
               Parent = this.TranslateTableIndex( md, layout.Parent ) // TypeDef -> already processed
            } );

         // FieldLayout
         this.MergeTables(
            Tables.FieldLayout,
            md => md.FieldLayouts,
            null,
            ( md, layout, inputIdx, thisIdx ) => new FieldLayout()
            {
               Field = this.TranslateTableIndex( md, layout.Field ), // FieldDef -> already processed
               Offset = layout.Offset
            } );

         // MethodSemantics
         this.MergeTables(
            Tables.MethodSemantics,
            md => md.MethodSemantics,
            null,
            ( md, semantics, inputIdx, thisIdx ) => new MethodSemantics()
            {
               Associaton = this.TranslateTableIndex( md, semantics.Associaton ), // Event/Property -> already processed
               Attributes = semantics.Attributes,
               Method = this.TranslateTableIndex( md, semantics.Method ) // MethodDef -> already processed
            } );

         // MethodImpl
         this.MergeTables(
            Tables.MethodImpl,
            md => md.MethodImplementations,
            null,
            ( md, impl, inputIdx, thisIdx ) => new MethodImplementation()
            {
               Class = this.TranslateTableIndex( md, impl.Class ), // TypeDef -> already processed
               MethodBody = this.TranslateTableIndex( md, impl.MethodBody ), // MethodDef/MemberRef -> already processed
               MethodDeclaration = this.TranslateTableIndex( md, impl.MethodDeclaration ) // MetodDef/MemberRef -> already processed
            } );

         // ImplMap
         this.MergeTables(
            Tables.ImplMap,
            md => md.MethodImplementationMaps,
            null,
            ( md, map, inputIdx, thisIdx ) => new MethodImplementationMap()
            {
               Attributes = map.Attributes,
               ImportName = map.ImportName,
               ImportScope = this.TranslateTableIndex( md, map.ImportScope ), // ModuleRef -> already processed
               MemberForwarded = this.TranslateTableIndex( md, map.MemberForwarded ) // FieldDef/MethodDef -> already processed
            } );

         // FieldRVA
         this.MergeTables(
            Tables.FieldRVA,
            md => md.FieldRVAs,
            null,
            ( md, fRVA, inputIdx, thisIdx ) => new FieldRVA()
            {
               Data = fRVA.Data.CreateBlockCopy(),
               Field = this.TranslateTableIndex( md, fRVA.Field ) // FieldDef -> already processed
            } );

         // GenericParameterConstraint
         this.MergeTables(
            Tables.GenericParameterConstraint,
            md => md.GenericParameterConstraintDefinitions,
            null,
            ( md, constraint, inputIdx, thisIdx ) => new GenericParameterConstraintDefinition()
            {
               Constraint = this.TranslateTableIndex( md, constraint.Constraint ), // TypeDef/TypeRef/TypeSpec -> already processed
               Owner = this.TranslateTableIndex( md, constraint.Owner ) // GenericParameterDefinition -> already processed
            } );

         // ExportedType
         this.MergeTables(
            Tables.ExportedType,
            md => md.ExportedTypes,
            ( md, inputIdx, thisIdx ) => this.ExportedTypeRowStaysInTargetModule( md, md.ExportedTypes[inputIdx.Index] ),
            ( md, eType, inputIdx, thisIdx ) => new ExportedType()
            {
               Attributes = eType.Attributes,
               Name = eType.Name,
               Namespace = eType.Namespace,
               TypeDefinitionIndex = eType.TypeDefinitionIndex
            } );
         // ExportedType may reference itself -> update Implementation only now
         this.SetTableIndices1(
            Tables.ExportedType,
            md => md.ExportedTypes,
            eType => eType.Implementation,
            ( eType, impl ) => eType.Implementation = impl
            );

         // FileReference
         this.MergeTables(
            Tables.File,
            md => md.FileReferences,
            ( md, inputIdx, thisIdx ) =>
            {
               var file = md.FileReferences[inputIdx.Index];
               return !file.Attributes.ContainsMetadata() || true; // TODO skip all those netmodules that are part of input modules
            },
            ( md, file, inputIdx, thisIdx ) => new FileReference()
            {
               Attributes = file.Attributes,
               HashValue = file.HashValue,
               Name = file.Name
            } );

         // ManifestResource
         var resDic = new Dictionary<String, IList<Tuple<CILMetaData, ManifestResource>>>();
         foreach ( var inputModule in this._inputModules )
         {
            foreach ( var res in inputModule.ManifestResources )
            {
               resDic
                  .GetOrAdd_NotThreadSafe( res.Name, n => new List<Tuple<CILMetaData, ManifestResource>>() )
                  .Add( Tuple.Create( inputModule, res ) );
            }
         }
         var resNameSet = new HashSet<String>();
         this.MergeTables(
            Tables.ManifestResource,
            md => md.ManifestResources,
            ( md, inputIdx, thisIdx ) => resNameSet.Add( md.ManifestResources[inputIdx.Index].Name ),
            ( md, resource, inputIdx, thisIdx ) => this.ProcessManifestResource( md, resource.Name, resDic )
            );

         // CustomAttributeDef
         this.MergeTables(
            Tables.CustomAttribute,
            md => md.CustomAttributeDefinitions,
            ( md, inputIdx, thisIdx ) =>
            {
               var parent = md.CustomAttributeDefinitions[inputIdx.Index].Parent;
               Boolean retVal;
               switch ( parent.Table )
               {
                  case Tables.Assembly:
                  case Tables.Module:
                     // Skip altogether
                     retVal = false;
                     break;
                  default:
                     // Include only if there is matching row in target module
                     retVal = this._tableIndexMappings[md].ContainsKey( parent );
                     break;
               }
               return retVal;
            },
            ( md, ca, inputIdx, thisIdx ) => new CustomAttributeDefinition()
            {
               Parent = this.TranslateTableIndex( md, ca.Parent ), // TypeDef/MethodDef/FieldDef/ParamDef/TypeRef/InterfaceImpl/MemberRef/ModuleDef/DeclSecurity/PropertyDef/EventDef/StandAloneSig/ModuleRef/TypeSpec/AssemblyDef/AssemblyRef/File/ExportedType/ManifestResource/GenericParameterDefinition/GenericParameterDefinitionConstraint/MethodSpec -> AssemblyDef and ModuleDef are skipped, ModuleRef and AssemblyRef are skipped for those who don't have row in target module
               Signature = this.ProcessCustomAttributeSignature( md, inputIdx.Index, ca.Signature ),
               Type = this.TranslateTableIndex( md, ca.Type ) // Type: MethodDef/MemberRef -> already processed
            } );
      }

      private void ApplyAssemblyAndModuleCustomAttributes()
      {
         var attrSource = this._options.AttrSource;
         this.CopyModuleAndAssemblyAttributesFrom(
            String.IsNullOrEmpty( attrSource ) ?
               ( this._options.CopyAttributes ?
                this._inputModules :
                this._primaryModule.Singleton() ) :
            this._moduleLoader.GetOrLoadMetaData( attrSource ).Singleton()
            );
      }

      private void CopyModuleAndAssemblyAttributesFrom( IEnumerable<CILMetaData> modules )
      {
         var targetCA = this._targetModule.CustomAttributeDefinitions;
         foreach ( var m in modules )
         {
            if ( this._inputModules.IndexOf( m ) >= 0 )
            {
               for ( var i = 0; i < m.CustomAttributeDefinitions.Count; ++i )
               {
                  var ca = m.CustomAttributeDefinitions[i];
                  if ( ca.Parent.Table == Tables.Assembly || ca.Parent.Table == Tables.Module )
                  {
                     targetCA.Add( new CustomAttributeDefinition()
                     {
                        Parent = ca.Parent,
                        Signature = this.ProcessCustomAttributeSignature( m, i, ca.Signature ),
                        Type = this.TranslateTableIndex( m, ca.Type )
                     } );
                  }
               }
            }
            else
            {
               // Table index translate methods want input module as source.
               // Most likely need to make input modules into IList<Tuple<CILMetaData, Boolean>>, where Boolean would indicate whether the module should be merged
               // Then MergeTables method would just simply not add the row to table if merged = false
               throw new NotImplementedException( "Custom attribute source from other than input assembly is not yet implemented." );
            }
         }
      }

      private void MergeTables<T>(
         Tables tableKind,
         Func<CILMetaData, List<T>> tableExtractor,
         Func<CILMetaData, TableIndex, TableIndex, Boolean> filter,
         Func<CILMetaData, T, TableIndex, TableIndex, T> copyFunc
         )
      {
         var targetTable = tableExtractor( this._targetModule );
         System.Diagnostics.Debug.Assert( targetTable.Count == 0, "Merging non-empty table in target module!" );
         foreach ( var md in this._inputModules )
         {
            var inputTable = tableExtractor( md );
            var thisMappings = this._tableIndexMappings[md];
            for ( var i = 0; i < inputTable.Count; ++i )
            {
               var inputIndex = new TableIndex( tableKind, i );
               var targetIndex = new TableIndex( tableKind, targetTable.Count );
               if ( filter == null || filter( md, inputIndex, targetIndex ) )
               {
                  this._targetTableIndexMappings.Add( targetIndex, Tuple.Create( md, i ) );
                  thisMappings.Add( inputIndex, targetIndex );

                  targetTable.Add( copyFunc( md, inputTable[i], inputIndex, targetIndex ) );
               }
            }
         }
      }

      private void SetTableIndicesNullable<T>(
         Tables tableKind,
         Func<CILMetaData, List<T>> tableExtractor,
         Func<T, TableIndex?> tableIndexGetter,
         Action<T, TableIndex> tableIndexSetter
         )
      {
         this.SetTableIndicesNullable(
            tableKind,
            tableExtractor,
            i => this._targetTableIndexMappings[new TableIndex( tableKind, i )],
            tableIndexGetter,
            tableIndexSetter
            );
      }

      private void SetTableIndicesNullable<T>(
         Tables tableKind,
         Func<CILMetaData, List<T>> tableExtractor,
         Func<Int32, Tuple<CILMetaData, Int32>> inputInfoGetter,
         Func<T, TableIndex?> tableIndexGetter,
         Action<T, TableIndex> tableIndexSetter
         )
      {
         var targetTable = tableExtractor( this._targetModule );
         for ( var i = 0; i < targetTable.Count; ++i )
         {
            var inputInfo = inputInfoGetter( i );
            var inputModule = inputInfo.Item1;
            var inputTableIndexNullable = tableIndexGetter( tableExtractor( inputModule )[inputInfo.Item2] );
            if ( inputTableIndexNullable.HasValue )
            {
               var inputTableIndex = inputTableIndexNullable.Value;
               var targetTableIndex = this._tableIndexMappings[inputModule][inputTableIndex];
               tableIndexSetter( targetTable[i], targetTableIndex );
            }
         }
      }

      private void SetTableIndices1<T>(
         Tables tableKind,
         Func<CILMetaData, List<T>> tableExtractor,
         Func<T, TableIndex> tableIndexGetter,
         Action<T, TableIndex> tableIndexSetter
         )
      {
         var targetTable = tableExtractor( this._targetModule );
         for ( var i = 0; i < targetTable.Count; ++i )
         {
            var inputInfo = this._targetTableIndexMappings[new TableIndex( tableKind, i )];
            var inputModule = inputInfo.Item1;
            var inputTableIndex = tableIndexGetter( tableExtractor( inputModule )[inputInfo.Item2] );
            var targetTableIndex = this._tableIndexMappings[inputModule][inputTableIndex];
            tableIndexSetter( targetTable[i], targetTableIndex );
         }
      }

      private TableIndex TranslateTableIndex(
         CILMetaData inputModule,
         TableIndex inputTableIndex
         )
      {
         return this._tableIndexMappings[inputModule][inputTableIndex];
      }

      private Boolean ExportedTypeRowStaysInTargetModule( CILMetaData inputModule, ExportedType exportedType )
      {
         var impl = exportedType.Implementation;
         switch ( impl.Table )
         {
            case Tables.ExportedType:
               // Nested type - return whatever the enclosing type is
               return this.ExportedTypeRowStaysInTargetModule( inputModule, inputModule.ExportedTypes[impl.Index] );
            case Tables.File:
               // The target module will be single-module assembly, so there shouldn't be any of these
               return false;
            case Tables.AssemblyRef:
               // This row stays only if the assembly ref is present in target module
               return this._tableIndexMappings[inputModule].ContainsKey( impl );
            default:
               return false;
         }
      }

      private static String CreateTypeString( IList<TypeDefinition> typeDefs, Int32 tDefIndex, IDictionary<Int32, Int32> enclosingTypeInfo )
      {
         var sb = new StringBuilder( typeDefs[tDefIndex].Name );

         // Iterate: thisType -> enclosingType -> ... -> outMostEnclosingType
         // Use loop detection to avoid nasty stack overflows with faulty modules
         var last = tDefIndex;
         using ( var enumerator = tDefIndex.AsSingleBranchEnumerableWithLoopDetection(
            cur =>
            {
               Int32 enclosingTypeIdx;
               return enclosingTypeInfo.TryGetValue( tDefIndex, out enclosingTypeIdx ) ?
                  enclosingTypeIdx :
                  -1;
            },
            endCondition: cur => cur < 0,
            includeFirst: false
            ).GetEnumerator() )
         {
            while ( enumerator.MoveNext() )
            {
               var enclosingTypeIdx = enumerator.Current;
               sb.Insert( 0, "+" )
                  .Insert( 0, typeDefs[enclosingTypeIdx].Name );
               last = enclosingTypeIdx;
            }
         }

         var ns = typeDefs[last].Namespace;
         if ( !String.IsNullOrEmpty( ns ) )
         {
            sb.Insert( 0, "." )
               .Insert( 0, ns );
         }

         return sb.ToString();
      }

      private static String CreateTypeStringFromTopLevelType( String ns, String name )
      {
         return String.IsNullOrEmpty( ns ) ? name : ( ns + "." + name );
      }

      private TypeAttributes GetNewTypeAttributesForType(
         CILMetaData md,
         Int32 tDefIndex,
         Boolean hasEnclosingType,
         String typeString )
      {
         var attrs = md.TypeDefinitions[tDefIndex].Attributes;
         // TODO cache all modules assembly def strings so we wouldn't call AssemblyDefinition.ToString() too excessively
         if ( this._options.Internalize
            && !this._primaryModule.Equals( md )
            && !this._excludeRegexes.Value.Any(
               reg => reg.IsMatch( typeString )
               || ( md.AssemblyDefinitions.Count > 0 && reg.IsMatch( "[" + md.AssemblyDefinitions[0] + "]" + typeString ) )
               )
            )
         {
            // Have to make this type internal
            if ( !hasEnclosingType )
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

      private Boolean IsDuplicateOK(
         ref String newName,
         String fullTypeString,
         ref String newFullTypeString,
         TypeAttributes newTypeAttrs,
         IDictionary<CILMetaData, IDictionary<Int32, String>> allTypeStrings,
         ref ISet<String> allTypeStringsSet
         )
      {
         var retVal = !this._options.Union
            && ( !newTypeAttrs.IsVisibleToOutsideOfDefinedAssembly()
               || this._options.AllowDuplicateTypes == null
               || this._options.AllowDuplicateTypes.Contains( fullTypeString )
               );
         if ( retVal )
         {
            // Have to rename
            if ( allTypeStringsSet == null )
            {
               allTypeStringsSet = new HashSet<String>( allTypeStrings.Values.SelectMany( dic => dic.Values ) );
            }

            var i = 2;
            var namePrefix = fullTypeString;
            do
            {
               fullTypeString = namePrefix + "_" + i;
               ++i;
            } while ( !allTypeStringsSet.Add( fullTypeString ) );

            newFullTypeString = fullTypeString;
            newName = newName + "_" + i;
         }
         return retVal;
      }

      private MarshalingInfo ProcessMarshalingInfo( CILMetaData inputModule, MarshalingInfo inputMarshalingInfo )
      {
         String processedMarshalType = null, processedArrayUDType = null;
         if ( !String.IsNullOrEmpty( inputMarshalingInfo.MarshalType ) )
         {
            processedMarshalType = this.ProcessTypeString( inputModule, inputMarshalingInfo.MarshalType );
         }

         if ( !String.IsNullOrEmpty( inputMarshalingInfo.SafeArrayUserDefinedType ) )
         {
            processedArrayUDType = this.ProcessTypeString( inputModule, inputMarshalingInfo.SafeArrayUserDefinedType );
         }


         return new MarshalingInfo(
            inputMarshalingInfo.Value,
            inputMarshalingInfo.SafeArrayType,
            processedArrayUDType,
            inputMarshalingInfo.IIDParameterIndex,
            inputMarshalingInfo.ArrayType,
            inputMarshalingInfo.SizeParameterIndex,
            inputMarshalingInfo.ConstSize,
            processedMarshalType,
            inputMarshalingInfo.MarshalCookie
            );

      }

      private AbstractSecurityInformation ProcessPermissionSet( CILMetaData md, Int32 declSecurityIdx, Int32 permissionSetIdx, AbstractSecurityInformation inputSecurityInfo )
      {

         AbstractSecurityInformation retVal;
         switch ( inputSecurityInfo.SecurityInformationKind )
         {
            case SecurityInformationKind.Raw:
               this.Log( MessageLevel.Warning, "Unresolved security information BLOB in {0}, at table index {1}, permission set {2}.", this._moduleLoader.GetResourceFor( md ), declSecurityIdx, permissionSetIdx );
               var raw = (RawSecurityInformation) inputSecurityInfo;
               retVal = new RawSecurityInformation()
               {
                  ArgumentCount = raw.ArgumentCount,
                  Bytes = raw.Bytes.CreateBlockCopy()
               };
               break;
            case SecurityInformationKind.Resolved:
               var resolved = (SecurityInformation) inputSecurityInfo;
               var args = resolved.NamedArguments;
               resolved = new SecurityInformation( resolved.NamedArguments.Count );
               retVal = resolved;
               resolved.NamedArguments.AddRange( args.Select( arg => this.ProcessCANamedArg( md, arg ) ) );
               break;
            default:
               throw new NotSupportedException( "Unsupported security information kind: " + inputSecurityInfo.SecurityInformationKind + "." );
         }
         retVal.SecurityAttributeType = this.ProcessTypeString( md, inputSecurityInfo.SecurityAttributeType );
         return retVal;
      }

      private String ProcessTypeString( CILMetaData inputModule, String typeString )
      {
         String typeName, assemblyName;
         CILMetaData moduleHoldingType;
         if ( typeString.ParseFullTypeString( out typeName, out assemblyName ) )
         {
            AssemblyInformation aInfo; Boolean isFullPublicKey;
            if ( AssemblyInformation.TryParse( assemblyName, out aInfo, out isFullPublicKey ) )
            {
               var aRef = new AssemblyReference();
               if ( isFullPublicKey )
               {
                  aRef.Attributes = AssemblyFlags.PublicKey;
               }
               aInfo.DeepCopyContentsTo( aRef.AssemblyInformation );
               this._inputModulesAsAssemblyReferences.TryGetValue( aRef, out moduleHoldingType );
            }
            else
            {
               this.Log( MessageLevel.Warning, "Type string contained malformed assembly name: \"{0}\", skipping.", assemblyName );
               moduleHoldingType = null;
            }
         }
         else
         {
            moduleHoldingType = this._inputModuleTypeNamesInInputModule[inputModule].ContainsKey( typeName ) ? inputModule : null;
         }

         if ( moduleHoldingType != null )
         {
            // The module holding the type is one of the input modules -> check renames
            typeString = this._targetTypeNames[this._inputModuleTypeNamesInTargetModule[moduleHoldingType][typeName]];
         }
         return typeString;
      }

      private AbstractCustomAttributeSignature ProcessCustomAttributeSignature( CILMetaData md, Int32 caIdx, AbstractCustomAttributeSignature sig )
      {
         AbstractCustomAttributeSignature retVal;
         switch ( sig.CustomAttributeSignatureKind )
         {
            case CustomAttributeSignatureKind.Raw:
               this.Log( MessageLevel.Warning, "Unresolved custom attribute BLOB in {0}, at table index {1}.", this._moduleLoader.GetResourceFor( md ), caIdx );
               retVal = new RawCustomAttributeSignature()
               {
                  Bytes = ( (RawCustomAttributeSignature) sig ).Bytes.CreateBlockCopy()
               };
               break;
            case CustomAttributeSignatureKind.Resolved:
               var resolved = (CustomAttributeSignature) sig;
               var resolvedRetVal = new CustomAttributeSignature( resolved.TypedArguments.Count, resolved.NamedArguments.Count );
               resolvedRetVal.TypedArguments.AddRange( resolved.TypedArguments.Select( arg => this.ProcessCATypedArg( md, arg ) ) );
               resolvedRetVal.NamedArguments.AddRange( resolved.NamedArguments.Select( arg => this.ProcessCANamedArg( md, arg ) ) );
               retVal = resolvedRetVal;
               break;
            default:
               throw new NotSupportedException( "Unsupported custom attribute signature kind: " + sig.CustomAttributeSignatureKind + "." );
         }
         return retVal;
      }

      private CustomAttributeNamedArgument ProcessCANamedArg( CILMetaData inputModule, CustomAttributeNamedArgument arg )
      {
         return new CustomAttributeNamedArgument()
         {
            IsField = arg.IsField,
            Name = arg.Name,
            Value = this.ProcessCATypedArg( inputModule, arg.Value )
         };
      }

      private CustomAttributeTypedArgument ProcessCATypedArg( CILMetaData inputModule, CustomAttributeTypedArgument arg )
      {
         return new CustomAttributeTypedArgument()
         {
            Type = this.ProcessCATypedArgType( inputModule, arg.Type ),
            Value = arg.Type.IsSimpleTypeOfKind( SignatureElementTypes.Type ) ? this.ProcessTypeString( inputModule, (String) arg.Value ) : arg.Value
         };
      }

      private CustomAttributeArgumentType ProcessCATypedArgType( CILMetaData inputModule, CustomAttributeArgumentType type )
      {
         switch ( type.ArgumentTypeKind )
         {
            case CustomAttributeArgumentTypeKind.Array:
               return new CustomAttributeArgumentTypeArray()
               {
                  ArrayType = this.ProcessCATypedArgType( inputModule, ( (CustomAttributeArgumentTypeArray) type ).ArrayType )
               };
            case CustomAttributeArgumentTypeKind.Simple:
               return type;
            case CustomAttributeArgumentTypeKind.TypeString:
               return new CustomAttributeArgumentTypeEnum()
               {
                  TypeString = this.ProcessTypeString( inputModule, ( (CustomAttributeArgumentTypeEnum) type ).TypeString )
               };
            default:
               throw new NotSupportedException( "Unsupported custom attribute typed argument type: " + type.ArgumentTypeKind + "." );
         }
      }

      private ManifestResource ProcessManifestResource(
         CILMetaData md,
         String resourceName,
         IDictionary<String, IList<Tuple<CILMetaData, ManifestResource>>> resourcesByName
         )
      {
         var list = resourcesByName[resourceName];
         if ( list.Count > 1 )
         {
            if ( !this._options.AllowDuplicateResources )
            {
               this.Log( MessageLevel.Warning, "Ignoring duplicate resource {0} in modules {1}.", resourceName, String.Join( ", ", list.Skip( 1 ).Select( t => this._moduleLoader.GetResourceFor( t.Item1 ) ) ) );
               var first = list[0];
               list = new List<Tuple<CILMetaData, ManifestResource>>()
               {
                  first
               };
            }
            else if ( list.All( t => !t.Item2.IsEmbeddedResource() ) )
            {
               // All are external resource -> just use one
               this.Log( MessageLevel.Warning, "Multiple external resource {0} in modules {1}, using resource from first module.", resourceName, String.Join( ", ", list.Select( t => this._moduleLoader.GetResourceFor( t.Item1 ) ) ) );
               var first = list[0];
               list = new List<Tuple<CILMetaData, ManifestResource>>()
               {
                  first
               };
            }
            else if ( list.Any( t => !t.Item2.IsEmbeddedResource() ) )
            {
               var embedded = list.Where( t => t.Item2.IsEmbeddedResource() );
               this.Log( MessageLevel.Warning, "Resource {0} is not embedded in all input modules, merging only embedded ones from modules {1}.", resourceName, String.Join( ", ", embedded.Select( t => this._moduleLoader.GetResourceFor( t.Item1 ) ) ) );
               list = embedded.ToList();
            }
         }

         var firstRes = list[0].Item2;
         var retVal = new ManifestResource()
         {
            Attributes = firstRes.Attributes,
            Implementation = firstRes.Implementation,
            Name = resourceName,
            Offset = firstRes.Offset
         };

         if ( firstRes.IsEmbeddedResource() )
         {
            // Then all resources are embedded
            using ( var strm = new MemoryStream() )
            {
               var rw = new System.Resources.ResourceWriter( strm );
               foreach ( var tuple in list )
               {
                  var inputResource = tuple.Item2;
                  var data = inputResource.DataInCurrentFile;
                  Boolean wasResourceManager;
                  foreach ( var resx in MResourcesIO.GetResourceInfo( data, out wasResourceManager ) )
                  {
                     var resName = resx.Name;
                     var resType = resx.Type;
                     if ( !resx.IsUserDefinedType && String.Equals( "ResourceTypeCode.String", resType ) )
                     {
                        // In case there is textual information about types serialized, have to fix that.
                        var idx = resx.DataOffset;
                        var strlen = data.ReadInt32Encoded7Bit( ref idx );
                        rw.AddResource( resName, this.ProcessTypeString( md, data.ReadStringWithEncoding( ref idx, strlen, Encoding.UTF8 ) ) );
                     }
                     else
                     {
                        var newTypeStr = this.ProcessTypeString( md, resType );
                        if ( String.Equals( newTypeStr, resType ) )
                        {
                           // Predefined ResourceTypeCode or pure reference type, add right away
                           var array = new Byte[resx.DataSize];
                           var dataStart = resx.DataOffset;
                           data.BlockCopyFrom( ref dataStart, array );
                           rw.AddResourceData( resName, resType, array );
                        }
                        else
                        {
                           // Have to fix records one by one
                           var idx = resx.DataOffset;
                           var records = MResourcesIO.ReadNRBFRecords( data, ref idx, idx + resx.DataSize );
                           foreach ( var rec in records )
                           {
                              this.ProcessNRBFRecord( md, rec );
                           }
                           var strm2 = new MemoryStream();
                           MResourcesIO.WriteNRBFRecords( records, strm2 );
                           rw.AddResourceData( resName, newTypeStr, strm2.ToArray() );
                        }
                     }
                  }
               }

               rw.Generate();
               retVal.DataInCurrentFile = strm.ToArray();
            }
         }
         else
         {
            // None are embedded, meaning that there is only one resource, just use its implementation table index
            retVal.Implementation = firstRes.Implementation;
         }

         return retVal;
      }

      private void ProcessNRBFRecord( CILMetaData inputModule, AbstractRecord record )
      {
         if ( record != null )
         {
            switch ( record.Kind )
            {
               case RecordKind.String:
                  ( (StringRecord) record ).StringValue = this.ProcessTypeString( inputModule, ( (StringRecord) record ).StringValue );
                  break;
               case RecordKind.Class:
                  var claas = (ClassRecord) record;
                  this.ProcessNRBFTypeInfo( claas );
                  foreach ( var member in claas.Members )
                  {
                     this.ProcessNRBFTypeInfo( member );
                     this.ProcessNRBFRecord( inputModule, member.Value as AbstractRecord );
                  }
                  break;
               case RecordKind.Array:
                  foreach ( var val in ( (ArrayRecord) record ).ValuesAsVector )
                  {
                     this.ProcessNRBFRecord( inputModule, val as AbstractRecord );
                  }
                  break;
               case RecordKind.PrimitiveWrapper:
                  var wrapper = (PrimitiveWrapperRecord) record;
                  if ( wrapper.Value is String )
                  {
                     wrapper.Value = this.ProcessTypeString( inputModule, wrapper.Value as String );
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
            CILMetaData referencedInputAssembly;
            element.AssemblyName = this.ProcessAssemblyName( tmp, out referencedInputAssembly );
            element.TypeName = this.ProcessTypeName( referencedInputAssembly, element.TypeName );
         }
      }

      private String ProcessTypeName( CILMetaData referencedInputAssembly, String typeName )
      {
         if ( referencedInputAssembly != null )
         {
            Int32 tDefIdx;
            if ( this._inputModuleTypeNamesInTargetModule[referencedInputAssembly].TryGetValue( typeName, out tDefIdx ) )
            {
               typeName = this._targetTypeNames[tDefIdx];
            }
            else
            {
               this.Log( MessageLevel.Warning, "Resource of type {0} from input module {1} did not have corresponding type within module itself.", typeName, this._moduleLoader.GetResourceFor( referencedInputAssembly ) );
            }
         }
         return typeName;
      }

      private String ProcessAssemblyName( String assName, out CILMetaData referencedInputAssembly )
      {
         AssemblyInformation assInfo; Boolean isFullPublicKey;
         referencedInputAssembly = null;
         if ( AssemblyInformation.TryParse( assName, out assInfo, out isFullPublicKey ) )
         {

            var aRef = new AssemblyReference();
            assInfo.DeepCopyContentsTo( aRef.AssemblyInformation );
            if ( isFullPublicKey )
            {
               aRef.Attributes = AssemblyFlags.PublicKey;
            }
            if ( this._inputModulesAsAssemblyReferences.TryGetValue( aRef, out referencedInputAssembly ) )
            {
               // TODO maybe assembly name should be null/empty, since in same assembly??
               assName = this._targetModule.AssemblyDefinitions[0].ToString();
            }
         }

         return assName;
      }

      private void Log( MessageLevel mLevel, String formatString, params Object[] args )
      {
         this._merger.Log( mLevel, formatString, args );
      }

      private CILMergeException NewCILMergeException( ExitCode code, String message, Exception inner = null )
      {
         return this._merger.NewCILMergeException( code, message, inner );
      }

      private void DoPotentiallyInParallel<T>( IEnumerable<T> enumerable, Action<Boolean, T> action )
      {
         this._merger.DoPotentiallyInParallel( enumerable, action );
      }

      protected override void Dispose( Boolean disposing )
      {
         if ( disposing && this._publicKeyComputer.IsValueCreated )
         {
            this._publicKeyComputer.Value.Transform.DisposeSafely();
         }
      }



































      //internal Tuple<String[], IDictionary<Tuple<String, String>, String>> EmitTargetAssembly()
      //{
      //   // TODO log all options here.

      //   this.LoadAllInputModules();

      //   // Save input emitting arguments for possible PDB emission later.
      //   var inputEArgs = this._assemblyLoader.CurrentlyLoadedModules.Select( m => this._assemblyLoader.GetEmittingArgumentsFor( m ) ).ToArray();
      //   var inputFileNames = this._assemblyLoader.CurrentlyLoadedModules.Select( m => this._assemblyLoader.GetResourceFor( m ) ).ToArray();

      //   // Create target module
      //   this._targetModule = this._ctx.NewBlankAssembly( Path.GetFileNameWithoutExtension( this._options.OutPath ) ).AddModule( Path.GetFileName( this._options.OutPath ) );

      //   // Get emitting arguments already at this stage - this will also set the correct AssociatedMSCorLib for the target module.
      //   var targetEArgs = this.CreateEmittingArgumentsForTargetModule();

      //   if ( !this._options.Union )
      //   {
      //      // Have to generate set of all used type names in order for renaming to be stable.
      //      foreach ( var t in this._inputModules.SelectMany( m => m.DefinedTypes.SelectMany( t => t.AsDepthFirstEnumerable( tt => tt.DeclaredNestedTypes ) ) ) )
      //      {
      //         this._allInputTypeNames.Add( t.ToString() );
      //      }
      //   }

      //   var an = this._targetModule.Assembly.Name;
      //   an.Culture = this._primaryModule.Assembly.Name.Culture;
      //   if ( targetEArgs.StrongName != null )
      //   {
      //      an.PublicKey = targetEArgs.StrongName.KeyPair.ToArray();
      //   }
      //   if ( this._options.VerMajor > -1 )
      //   {
      //      // Version was specified explictly
      //      an.MajorVersion = this._options.VerMajor;
      //      an.MinorVersion = this._options.VerMinor;
      //      an.BuildNumber = this._options.VerBuild;
      //      an.Revision = this._options.VerRevision;
      //   }
      //   else
      //   {
      //      var an2 = this._primaryModule.Assembly.Name;
      //      an.MajorVersion = an2.MajorVersion;
      //      an.MinorVersion = an2.MinorVersion;
      //      an.BuildNumber = an2.BuildNumber;
      //      an.Revision = an2.Revision;
      //   }

      //   // Two sweeps - first create structure
      //   this.ProcessTargetModule( true );

      //   // Then add type references and IL
      //   this.ProcessTargetModule( false );

      //   // Then merge resources
      //   if ( !this._options.NoResources )
      //   {
      //      this.MergeResources();
      //   }

      //   // Remember process entry point
      //   targetEArgs.CLREntryPoint = this.ProcessMethodRef( targetEArgs.CLREntryPoint );

      //   // Emit the module
      //   try
      //   {
      //      using ( var fs = File.Open( this._options.OutPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None ) )
      //      {
      //         this._targetModule.EmitModule( fs, targetEArgs );
      //      }
      //   }
      //   catch ( Exception exc )
      //   {
      //      throw this.NewCILMergeException( ExitCode.ErrorAccessingTargetFile, "Error accessing target file " + this._options.OutPath + "(" + exc + ").", exc );
      //   }

      //   var badRefs = targetEArgs.AssemblyRefs.Where( aName => this._inputModules.Any( m => m.Assembly.Name.CorePropertiesEqual( aName ) ) ).ToArray();
      //   if ( badRefs.Length > 0 )
      //   {
      //      throw this.NewCILMergeException( ExitCode.FailedToProduceCorrectResult, "Internal error: the resulting assembly still references " + String.Join( ", ", (Object[]) badRefs ) + "." );
      //   }

      //   // Merge PDBs
      //   if ( this._pdbHelper != null )
      //   {
      //      this.MergePDBs( targetEArgs, inputEArgs );
      //   }

      //   return Tuple.Create( inputFileNames, this._typeRenames == null || this._typeRenames.Count == 0 ? null : this._typeRenames );
      //}





      //private void ProcessTargetModule( Boolean creating )
      //{
      //   // Don't do this in parallel - too much congestion.
      //   // Process primary module first in case of _name conflicts
      //   foreach ( var mod in this._inputModules )
      //   {
      //      this.ProcessModule( mod, creating );
      //   }

      //   if ( !creating )
      //   {
      //      this.ProcessTargetAssemblyAttributes();
      //   }
      //}

      //private void ProcessModule( CILModule inputModule, Boolean creating )
      //{
      //   foreach ( var t in inputModule.DefinedTypes )
      //   {
      //      this.ProcessTypeDefinition( this._targetModule, t, creating );
      //   }
      //}

      //private void ProcessTypeDefinition( CILElementCapableOfDefiningType owner, CILType oldType, Boolean creating )
      //{
      //   CILType newType;
      //   if ( creating )
      //   {
      //      var typeStr = oldType.ToString();
      //      var added = this._typesByName.Add( typeStr );
      //      var typeAttrs = this.GetNewTypeAttributesForType( owner, oldType, typeStr );
      //      var newName = oldType.Name;
      //      if ( added || this.IsDuplicateOK( ref newName, typeStr, typeAttrs ) )
      //      {
      //         newType = this.AddNewTypeToTarget( owner, oldType, typeAttrs, newName );
      //         if ( !added )
      //         {
      //            // Rename occurred, save information
      //            this._typeRenames.Add( Tuple.Create( oldType.Module.Assembly.Name.Name, typeStr ), newType.ToString() );
      //         }
      //      }
      //      else if ( this._options.Union )
      //      {
      //         newType = this._targetModule.GetTypeByName( typeStr );
      //         this._typeMapping.TryAdd( oldType, newType );
      //      }
      //      else
      //      {
      //         throw this.NewCILMergeException( ExitCode.DuplicateTypeName, "The type " + oldType + " appears in more than one assembly." );
      //      }
      //   }
      //   else
      //   {
      //      newType = (CILType) this._typeMapping[oldType];
      //   }

      //   if ( creating )
      //   {
      //      newType.Layout = oldType.Layout;
      //      newType.Namespace = oldType.Namespace;
      //   }
      //   else
      //   {
      //      this.ProcessCustomAttributes( newType, oldType );
      //      newType.AddDeclaredInterfaces( oldType.DeclaredInterfaces.Select( this.ProcessTypeRef ).ToArray() );
      //      newType.BaseType = this.ProcessTypeRef( oldType.BaseType );
      //      this.ProcessGenericParameters( newType, oldType );
      //      this.ProcessDeclSecurity( newType, oldType );
      //   }

      //   // Process type structure
      //   foreach ( var nt in oldType.DeclaredNestedTypes )
      //   {
      //      this.ProcessTypeDefinition( newType, nt, creating );
      //   }
      //   foreach ( var ctor in oldType.Constructors )
      //   {
      //      this.ProcessConstructor( newType, ctor, creating );
      //   }
      //   foreach ( var method in oldType.DeclaredMethods )
      //   {
      //      this.ProcessMethod( newType, method, creating );
      //   }
      //   foreach ( var field in oldType.DeclaredFields )
      //   {
      //      this.ProcessField( newType, field, creating );
      //   }
      //   foreach ( var prop in oldType.DeclaredProperties )
      //   {
      //      this.ProcessProperty( newType, prop, creating );
      //   }
      //   foreach ( var evt in oldType.DeclaredEvents )
      //   {
      //      this.ProcessEvent( newType, evt, creating );
      //   }
      //}

      //private void ProcessDeclSecurity( CILElementWithSecurityInformation newElem, CILElementWithSecurityInformation oldElem )
      //{
      //   foreach ( var ds in oldElem.DeclarativeSecurity.Values.SelectMany( l => l ) )
      //   {
      //      var nds = newElem.AddDeclarativeSecurity( ds.SecurityAction, ds.SecurityAttributeType );
      //      foreach ( var na in ds.NamedArguments )
      //      {
      //         nds.NamedArguments.Add( this.ProcessCustomAttributeNamedArg( na ) );
      //      }
      //   }
      //}

      //private void ProcessGenericParameters<TGDef>( CILElementWithGenericArguments<TGDef> newElem, CILElementWithGenericArguments<TGDef> oldElem )
      //   where TGDef : class
      //{
      //   foreach ( CILTypeParameter gp in oldElem.GenericArguments )
      //   {
      //      var newGP = (CILTypeParameter) newElem.GenericArguments[gp.GenericParameterPosition];
      //      newGP.AddGenericParameterConstraints( gp.GenericParameterConstraints.Select( c => this.ProcessTypeRef( c ) ).ToArray() );
      //      newGP.Attributes = gp.Attributes;
      //      this.ProcessCustomAttributes( newGP, gp );
      //   }
      //}

      //private TypeAttributes GetNewTypeAttributesForType( CILElementCapableOfDefiningType owner, CILType oldType, String typeString )
      //{
      //   var attrs = oldType.Attributes;
      //   if ( this._options.Internalize && !this._primaryModule.Equals( oldType.Module ) && !this._excludeRegexes.Value.Any( reg => reg.IsMatch( typeString ) || reg.IsMatch( "[" + oldType.Module.Assembly.Name.Name + "]" + typeString ) ) )
      //   {
      //      // Have to make this type internal
      //      if ( owner is CILModule )
      //      {
      //         attrs &= ~TypeAttributes.VisibilityMask;
      //      }
      //      else if ( attrs.IsNestedPublic() )
      //      {
      //         attrs |= TypeAttributes.VisibilityMask;
      //         attrs &= ( ( ~TypeAttributes.VisibilityMask ) | TypeAttributes.NestedAssembly );
      //      }
      //      else if ( attrs.IsNestedFamily() || attrs.IsNestedFamilyORAssembly() )
      //      {
      //         attrs |= TypeAttributes.VisibilityMask;
      //         attrs &= ( ~TypeAttributes.VisibilityMask ) | TypeAttributes.NestedFamANDAssem;
      //      }
      //   }
      //   return attrs;
      //}

      //private CILType AddNewTypeToTarget( CILElementCapableOfDefiningType owner, CILType other, TypeAttributes attrs, String newName )
      //{
      //   var t = owner.AddType( newName ?? other.Name, attrs, other.TypeCode );
      //   this._typeMapping.TryAdd( other, t );
      //   if ( other.GenericArguments.Any() )
      //   {
      //      foreach ( var gp in t.DefineGenericParameters( other.GenericArguments.Select( g => ( (CILTypeParameter) g ).Name ).ToArray() ) )
      //      {
      //         this._typeMapping.TryAdd( other.GenericArguments[gp.GenericParameterPosition], gp );
      //      }
      //   }
      //   return t;
      //}

      //private void ProcessConstructor( CILType newType, CILConstructor ctor, Boolean creating )
      //{
      //   CILConstructor newCtor = creating ?
      //      newType.AddConstructor( ctor.Attributes, ctor.CallingConvention ) :
      //      (CILConstructor) this._methodMapping[ctor];
      //   this.ProcessMethodBase( newCtor, ctor, creating );
      //}

      //private void ProcessMethod( CILType newType, CILMethod method, Boolean creating )
      //{
      //   CILMethod newMethod = creating ?
      //      newType.AddMethod( method.Name, method.Attributes, method.CallingConvention ) :
      //      (CILMethod) this._methodMapping[method];
      //   this.ProcessMethodBase( newMethod, method, creating );
      //   this.ProcessParameter( newMethod.ReturnParameter, method.ReturnParameter, creating );
      //   if ( creating )
      //   {
      //      newMethod.PlatformInvokeModuleName = method.PlatformInvokeModuleName;
      //      newMethod.PlatformInvokeName = method.PlatformInvokeName;
      //      newMethod.PlatformInvokeAttributes = method.PlatformInvokeAttributes;
      //      foreach ( var g in newMethod.DefineGenericParameters( method.GenericArguments.Select( g => ( (CILTypeParameter) g ).Name ).ToArray() ) )
      //      {
      //         this._typeMapping.TryAdd( method.GenericArguments[g.GenericParameterPosition], g );
      //      }
      //   }
      //   else
      //   {
      //      newMethod.AddOverriddenMethods( method.OverriddenMethods.Select( this.ProcessMethodRef ).ToArray() );
      //      this.ProcessGenericParameters( newMethod, method );
      //   }
      //}

      //private void ProcessMethodBase( CILMethodBase newMethod, CILMethodBase oldMethod, Boolean creating )
      //{
      //   if ( creating )
      //   {
      //      this._methodMapping.TryAdd( oldMethod, newMethod );
      //      newMethod.ImplementationAttributes = oldMethod.ImplementationAttributes;
      //   }
      //   foreach ( var p in oldMethod.Parameters )
      //   {
      //      var newP = creating ?
      //         newMethod.AddParameter( p.Name, p.Attributes, null ) :
      //         newMethod.Parameters[p.Position];
      //      this.ProcessParameter( newP, p, creating );
      //   }
      //   if ( !creating )
      //   {
      //      this.ProcessCustomAttributes( newMethod, oldMethod );
      //      this.ProcessDeclSecurity( newMethod, oldMethod );

      //      if ( oldMethod.HasILMethodBody() )
      //      {
      //         var oldIL = oldMethod.MethodIL;
      //         var newIL = newMethod.MethodIL;
      //         // First, define labels
      //         var newLabels = newIL.DefineLabels( oldIL.LabelCount );

      //         // Then define locals
      //         foreach ( var local in oldIL.Locals )
      //         {
      //            newIL.DeclareLocal( this.ProcessTypeRef( local.LocalType ), local.IsPinned );
      //         }
      //         // Then define exception blocks
      //         foreach ( var eBlock in oldIL.ExceptionBlocks )
      //         {
      //            newIL.AddExceptionBlockInfo( new ExceptionBlockInfo(
      //               eBlock.EndLabel,
      //               eBlock.TryOffset,
      //               eBlock.TryLength,
      //               eBlock.HandlerOffset,
      //               eBlock.HandlerLength,
      //               this.ProcessTypeRef( eBlock.ExceptionType ),
      //               eBlock.FilterOffset,
      //               eBlock.BlockType ) );
      //         }
      //         // Then copy IL opcode infos
      //         foreach ( var info in oldIL.OpCodeInfos )
      //         {
      //            switch ( info.InfoKind )
      //            {
      //               case OpCodeInfoKind.Branch:
      //               case OpCodeInfoKind.BranchOrLeaveFixed:
      //               case OpCodeInfoKind.Leave:
      //               case OpCodeInfoKind.OperandInt32:
      //               case OpCodeInfoKind.OperandInt64:
      //               case OpCodeInfoKind.OperandNone:
      //               case OpCodeInfoKind.OperandR4:
      //               case OpCodeInfoKind.OperandR8:
      //               case OpCodeInfoKind.OperandString:
      //               case OpCodeInfoKind.OperandUInt16:
      //               case OpCodeInfoKind.Switch:
      //                  // Just add the info - labels etc should remain the same.
      //                  newIL.Add( info );
      //                  break;
      //               case OpCodeInfoKind.NormalOrVirtual:
      //                  var i1 = (OpCodeInfoForNormalOrVirtual) info;
      //                  newIL.Add( new OpCodeInfoForNormalOrVirtual( this.ProcessMethodRef( i1.ReflectionObject ), i1.NormalCode, i1.VirtualCode ) );
      //                  break;
      //               case OpCodeInfoKind.OperandCtorToken:
      //                  var i2 = (OpCodeInfoWithCtorToken) info;
      //                  newIL.Add( new OpCodeInfoWithCtorToken( i2.Code, this.ProcessMethodRef( i2.ReflectionObject ), i2.UseGenericDefinitionIfPossible ) );
      //                  break;
      //               case OpCodeInfoKind.OperandFieldToken:
      //                  var i3 = (OpCodeInfoWithFieldToken) info;
      //                  newIL.Add( new OpCodeInfoWithFieldToken( i3.Code, this.ProcessFieldRef( i3.ReflectionObject ), i3.UseGenericDefinitionIfPossible ) );
      //                  break;
      //               case OpCodeInfoKind.OperandMethodSigToken:
      //                  var i4 = (OpCodeInfoWithMethodSig) info;
      //                  //Tuple<CILCustomModifier[], CILTypeBase>[] varArgs = null;
      //                  //if ( i4.VarArgs != null )
      //                  //{
      //                  //   varArgs = i4.VarArgs.Select( va => Tuple.Create( va.Item1.Select( cm => this.ProcessCustomModifier( cm ) ).ToArray(), this.ProcessTypeRef( va.Item2 ) ) ).ToArray();
      //                  //}
      //                  newIL.Add( new OpCodeInfoWithMethodSig( this.ProcessTypeRef( i4.ReflectionObject ), i4.VarArgs ) );
      //                  break;
      //               case OpCodeInfoKind.OperandMethodToken:
      //                  var i5 = (OpCodeInfoWithMethodToken) info;
      //                  //var mRef = this.ProcessMethodRef( i5.ReflectionObject );
      //                  //OpCodeInfo opToAdd;
      //                  //if ( ( OpCodeEncoding.Call == i5.Code.Value || OpCodeEncoding.Ldftn == i5.Code.Value )
      //                  //     && this._pcl2TargetMapping != null
      //                  //     && this._loadingArgs[oldMethod.DeclaringType.Module].AssemblyRefs.FirstOrDefault( ar => ar.CorePropertiesEqual( i5.ReflectionObject.DeclaringType.Module.Assembly.Name ) && ar.Flags.IsRetargetable() ) != null
      //                  //   )
      //                  //{
      //                  //   // When mapping pcl to .NET, replace .Call with .Callvirt where applicable
      //                  //   opToAdd = OpCodeEncoding.Call == i5.Code.Value ? OpCodeInfoForNormalOrVirtual.OpCodeInfoForCall( mRef ) : OpCodeInfoForNormalOrVirtual.OpCodeInfoForLdFtn( mRef );
      //                  //}
      //                  //else
      //                  //{
      //                  //   opToAdd = new OpCodeInfoWithMethodToken( i5.Code, mRef, i5.UseGenericDefinitionIfPossible );
      //                  //}
      //                  // TODO proper detection of base.Method(); However, maybe someone actually wants to .Call a virtual method from outside base.Method() context?
      //                  // At the moment there is no code to distinguish reliably between base.Method(); and this.AnotherMethod();.
      //                  // Adding a OpCodeInfoForNormalOrVirtual results in possible stackoverflow exceptions caused by merged code.
      //                  // Add instruction as-is, causing some verification errors in resulting merged assembly when retargeting PCL to .NET (better than stackoverflow).
      //                  newIL.Add( new OpCodeInfoWithMethodToken( i5.Code, this.ProcessMethodRef( i5.ReflectionObject ), i5.UseGenericDefinitionIfPossible ) );
      //                  break;
      //               case OpCodeInfoKind.OperandTypeToken:
      //                  var i6 = (OpCodeInfoWithTypeToken) info;
      //                  newIL.Add( new OpCodeInfoWithTypeToken( i6.Code, this.ProcessTypeRef( i6.ReflectionObject ), i6.UseGenericDefinitionIfPossible ) );
      //                  break;
      //            }
      //            if ( !this._options.NoDebug && info.InfoKind == OpCodeInfoKind.Branch || info.InfoKind == OpCodeInfoKind.Leave )
      //            {
      //               throw this.NewCILMergeException( ExitCode.DebugInfoNotEasilyMergeable, "Found dynamic branch opcode info, which would possibly change method IL size and line offsets." );
      //            }
      //         }
      //         // Finally, mark labels
      //         var curLblOffset = 0;
      //         foreach ( var lblOffset in oldIL.LabelOffsets )
      //         {
      //            newIL.MarkLabel( newLabels[curLblOffset], lblOffset );
      //            ++curLblOffset;
      //         }
      //      }
      //   }
      //}

      //private void ProcessParameter( CILParameter newParameter, CILParameter oldParameter, Boolean creating )
      //{
      //   if ( creating )
      //   {
      //      newParameter.Name = oldParameter.Name;
      //      if ( oldParameter.Attributes.HasDefault() )
      //      {
      //         newParameter.ConstantValue = oldParameter.ConstantValue;
      //      }
      //   }
      //   else
      //   {
      //      this.ProcessCustomAttributes( newParameter, oldParameter );
      //      this.ProcessCustomModifiers( newParameter, oldParameter );
      //      newParameter.ParameterType = this.ProcessTypeRef( oldParameter.ParameterType );
      //      this.ProcessMarshalInfo( newParameter, oldParameter );
      //   }
      //}

      //private void ProcessMarshalInfo( CILElementWithMarshalingInfo newElement, CILElementWithMarshalingInfo oldElement )
      //{
      //   var mi = oldElement.MarshalingInformation;
      //   if ( mi != null )
      //   {
      //      newElement.MarshalingInformation = new MarshalingInfo( mi.Value, mi.SafeArrayType, this.ProcessTypeRef( mi.SafeArrayUserDefinedType ), mi.IIDParameterIndex, mi.ArrayType, mi.SizeParameterIndex, mi.ConstSize, this.ProcessTypeString( mi.MarshalType ), null, mi.MarshalCookie );
      //   }
      //}

      //private void ProcessField( CILType newType, CILField oldField, Boolean creating )
      //{
      //   var newField = creating ?
      //      newType.AddField( oldField.Name, null, oldField.Attributes ) :
      //      this._fieldMapping[oldField];
      //   if ( creating )
      //   {
      //      this._fieldMapping.TryAdd( oldField, newField );
      //      if ( oldField.Attributes.HasDefault() )
      //      {
      //         newField.ConstantValue = oldField.ConstantValue;
      //      }
      //      newField.FieldOffset = oldField.FieldOffset;
      //      newField.InitialValue = oldField.InitialValue;
      //   }
      //   else
      //   {
      //      this.ProcessCustomAttributes( newField, oldField );
      //      this.ProcessCustomModifiers( newField, oldField );
      //      newField.FieldType = this.ProcessTypeRef( oldField.FieldType );
      //      this.ProcessMarshalInfo( newField, oldField );
      //   }
      //}

      //private void ProcessProperty( CILType newType, CILProperty oldProperty, Boolean creating )
      //{
      //   var newProperty = creating ?
      //      newType.AddProperty( oldProperty.Name, oldProperty.Attributes ) :
      //      this._propertyMapping[oldProperty];
      //   if ( creating )
      //   {
      //      this._propertyMapping.TryAdd( oldProperty, newProperty );
      //      if ( oldProperty.Attributes.HasDefault() )
      //      {
      //         newProperty.ConstantValue = oldProperty.ConstantValue;
      //      }
      //   }
      //   else
      //   {
      //      this.ProcessCustomAttributes( newProperty, oldProperty );
      //      this.ProcessCustomModifiers( oldProperty, newProperty );
      //      newProperty.GetMethod = this.ProcessMethodRef( oldProperty.GetMethod );
      //      newProperty.SetMethod = this.ProcessMethodRef( oldProperty.SetMethod );
      //   }
      //}

      //private void ProcessEvent( CILType newType, CILEvent oldEvent, Boolean creating )
      //{
      //   var newEvent = creating ?
      //      newType.AddEvent( oldEvent.Name, oldEvent.Attributes, null ) :
      //      this._eventMapping[oldEvent];
      //   if ( creating )
      //   {
      //      this._eventMapping.TryAdd( oldEvent, newEvent );
      //   }
      //   else
      //   {
      //      this.ProcessCustomAttributes( newEvent, oldEvent );
      //      newEvent.AddMethod = this.ProcessMethodRef( oldEvent.AddMethod );
      //      newEvent.RemoveMethod = this.ProcessMethodRef( oldEvent.RemoveMethod );
      //      newEvent.RaiseMethod = this.ProcessMethodRef( oldEvent.RaiseMethod );
      //      newEvent.EventHandlerType = this.ProcessTypeRef( oldEvent.EventHandlerType );
      //   }
      //}

      //private T ProcessTypeRef<T>( T typeRef )
      //   where T : class, CILTypeBase
      //{
      //   if ( typeRef is CILTypeOrTypeParameter && ( (CILTypeOrTypeParameter) typeRef ).Name == "PDBScopeOrFunction" )
      //   {

      //   }
      //   return typeRef == null ?
      //      null :
      //      (T) this._typeMapping.GetOrAdd( typeRef, tr =>
      //      {
      //         CILTypeBase result = typeRef;
      //         switch ( typeRef.TypeKind )
      //         {
      //            case TypeKind.Type:
      //               var t = (CILType) typeRef;
      //               if ( t.ElementKind.HasValue )
      //               {
      //                  // Array/ByRef/Pointer type, save mapped element type.
      //                  result = this.ProcessTypeRef( t.ElementType ).MakeElementType( t.ElementKind.Value, t.ArrayInformation );
      //               }
      //               else if ( t.GenericArguments.Count > 0 && !t.IsGenericTypeDefinition() )
      //               {
      //                  // Generic type which is not generic definition, save mapped generic definition
      //                  result = this.ProcessTypeRef( t.GenericDefinition ).MakeGenericType( t.GenericArguments.Select( g => this.ProcessTypeRef( g ) ).ToArray() );
      //               }
      //               break;
      //            case TypeKind.TypeParameter:
      //               var tp = (CILTypeParameter) typeRef;
      //               if ( tp.DeclaringMethod == null )
      //               {
      //                  result = this.ProcessTypeRef( tp.DeclaringType ).GenericArguments[tp.GenericParameterPosition];
      //               }
      //               else
      //               {
      //                  result = this.ProcessMethodRef( tp.DeclaringMethod ).GenericArguments[tp.GenericParameterPosition];
      //               }
      //               break;
      //            case TypeKind.MethodSignature:
      //               var ms = (CILMethodSignature) typeRef;
      //               result = this._ctx.NewMethodSignature(
      //                        this._targetModule,
      //                        ms.CallingConvention,
      //                        this.ProcessTypeRef( ms.ReturnParameter.ParameterType ),
      //                        ms.ReturnParameter.CustomModifiers.Select( cm => this.ProcessCustomModifier( cm ) ).ToArray(),
      //                        ms.Parameters.Select( p => Tuple.Create( p.CustomModifiers.Select( cm => this.ProcessCustomModifier( cm ) ).ToArray(), this.ProcessTypeRef( p.ParameterType ) ) ).ToArray()
      //                        );
      //               break;
      //         }
      //         CILAssembly ass;
      //         if ( this._pcl2TargetMapping != null && result is CILType && ( (CILType) result ).IsTrueDefinition && this._pcl2TargetMapping.TryGetValue( result.Module.Assembly, out ass ) )
      //         {
      //            // Map PCL type to target framework type
      //            var tResult = (CILType) result;
      //            var typeStr = tResult.GetFullName();
      //            var mappedType = GetMainModuleOrFirst( ass ).GetTypeByName( typeStr, false );
      //            if ( mappedType == null )
      //            {
      //               // Try process type forwarders
      //               TypeForwardingInfo tf;
      //               if ( ass.TryGetTypeForwarder( tResult.Name, tResult.Namespace, out tf ) )
      //               {
      //                  CILAssembly targetAss;
      //                  if ( this._assemblyLoader.TryResolveReference( result.Module, tf.AssemblyName, out targetAss ) )
      //                  {
      //                     mappedType = GetMainModuleOrFirst( targetAss )
      //                        .GetTypeByName( typeStr, false );
      //                  }
      //                  else
      //                  {
      //                     throw this.NewCILMergeException( ExitCode.UnresolvedAssemblyReference, "Failed to resolve " + tf.AssemblyName + " from " + result.Module + "." );
      //                  }
      //               }
      //            }
      //            if ( mappedType == null )
      //            {
      //               throw this.NewCILMergeException( ExitCode.PCL2TargetTypeFail, "Failed to find type " + result + " from target framework." );
      //            }
      //            result = mappedType;
      //         }
      //         return result;
      //      } );
      //}

      //private void ProcessCustomModifiers( CILElementWithCustomModifiers newElement, CILElementWithCustomModifiers oldElement )
      //{
      //   foreach ( var mod in oldElement.CustomModifiers )
      //   {
      //      newElement.AddCustomModifier( this.ProcessTypeRef( mod.Modifier ), mod.Optionality );
      //   }
      //}

      //private CILCustomModifier ProcessCustomModifier( CILCustomModifier mod )
      //{
      //   return CILCustomModifierFactory.CreateModifier( mod.Optionality, this.ProcessTypeRef( mod.Modifier ) );
      //}

      //private TMethod ProcessMethodRef<TMethod>( TMethod method )
      //   where TMethod : class,CILMethodBase
      //{
      //   return method == null ?
      //      null :
      //      (TMethod) this._methodMapping.GetOrAdd( method, mm =>
      //      {
      //         CILMethodBase result = mm;
      //         var m = method as CILMethod;

      //         if ( m != null && m.HasGenericArguments() && !m.IsGenericMethodDefinition() )
      //         {
      //            result = this.ProcessMethodRef( m.GenericDefinition ).MakeGenericMethod( m.GenericArguments.Select( g => this.ProcessTypeRef( g ) ).ToArray() );
      //         }
      //         else if ( method.DeclaringType.IsGenericType() && !method.DeclaringType.IsGenericTypeDefinition() )
      //         {
      //            result = this.ProcessMethodRef( method.ChangeDeclaringTypeUT( method.DeclaringType.GenericDefinition.GenericArguments.ToArray() ) )
      //               .ChangeDeclaringTypeUT( method.DeclaringType.GenericArguments.Select( g => this.ProcessTypeRef( g ) ).ToArray() );
      //         }
      //         else if ( this._pcl2TargetMapping != null )
      //         {
      //            var tt = this.ProcessTypeRef( method.DeclaringType );
      //            if ( !tt.Equals( method.DeclaringType ) )
      //            {
      //               if ( result is CILConstructor )
      //               {
      //                  result = tt.Constructors.FirstOrDefault( ctor => this.MatchMethodParams( (CILConstructor) result, ctor ) );
      //               }
      //               else
      //               {
      //                  result = tt.DeclaredMethods.FirstOrDefault( mtd => String.Equals( mtd.Name, result.GetName() ) && this.MatchMethodParams( (CILMethod) result, mtd ) && this.MatchMethodParam( ( (CILMethod) result ).ReturnParameter, mtd.ReturnParameter ) && ( (CILMethod) result ).GenericArguments.Count == mtd.GenericArguments.Count );
      //               }
      //               if ( result == null )
      //               {
      //                  throw this.NewCILMergeException( ExitCode.PCL2TargetMethodFail, "Failed to find method " + mm + " from target framework." );
      //               }
      //            }
      //         }
      //         return result;
      //      } );
      //}

      //private Boolean MatchMethodParams<TMethod>( TMethod pclMethod, TMethod targetMethod )
      //   where TMethod : CILMethodBase
      //{
      //   return pclMethod.Parameters.Count == targetMethod.Parameters.Count &&
      //      pclMethod.Parameters.All( p => this.MatchMethodParam( p, targetMethod.Parameters[p.Position] ) );
      //}

      //private Boolean MatchMethodParam( CILParameter pclParam, CILParameter targetParam )
      //{
      //   return String.Equals( pclParam.ParameterType.ToString(), targetParam.ParameterType.ToString() );// targetParam.ParameterType.Equals( this.ProcessTypeRef( pclParam.ParameterType ) );
      //}

      //private CILField ProcessFieldRef( CILField field )
      //{
      //   return field == null ?
      //      null :
      //      this._fieldMapping.GetOrAdd( field, ff =>
      //      {
      //         var result = ff;
      //         if ( field.DeclaringType.IsGenericType() && !field.DeclaringType.IsGenericTypeDefinition() )
      //         {
      //            result = this.ProcessFieldRef( field.ChangeDeclaringType( field.DeclaringType.GenericDefinition.GenericArguments.ToArray() ) )
      //               .ChangeDeclaringType( field.DeclaringType.GenericArguments.Select( g => this.ProcessTypeRef( g ) ).ToArray() );
      //         }
      //         else if ( this._pcl2TargetMapping != null )
      //         {
      //            var tt = this.ProcessTypeRef( field.DeclaringType );
      //            if ( !tt.Equals( field.DeclaringType ) )
      //            {
      //               result = tt.DeclaredFields.FirstOrDefault( f => String.Equals( f.Name, result.Name ) && String.Equals( f.FieldType.ToString(), result.FieldType.ToString() ) );//   f.FieldType.Equals( this.ProcessTypeRef( result.FieldType ) ) );
      //               if ( result == null )
      //               {
      //                  throw this.NewCILMergeException( ExitCode.PCL2TargetFieldFail, "Failed to find field " + field + " from target framework." );
      //               }
      //            }
      //         }
      //         return result;
      //      } );
      //}

      //private void ProcessCustomAttributes( CILCustomAttributeContainer newContainer, CILCustomAttributeContainer oldContainer )
      //{
      //   foreach ( var attr in oldContainer.CustomAttributeData )
      //   {
      //      this.ProcessCustomAttribute( newContainer, attr );
      //   }
      //}

      //private CILCustomAttribute ProcessCustomAttribute( CILCustomAttributeContainer container, CILCustomAttribute attr )
      //{
      //   return container.AddCustomAttribute(
      //      this.ProcessMethodRef( attr.Constructor ),
      //      attr.ConstructorArguments.Select( this.ProcessCustomAttributeTypedArg ),
      //      attr.NamedArguments.Select( this.ProcessCustomAttributeNamedArg ) );
      //}

      //private CILCustomAttributeTypedArgument ProcessCustomAttributeTypedArg( CILCustomAttributeTypedArgument arg )
      //{
      //   return CILCustomAttributeFactory.NewTypedArgument( this.ProcessTypeRef( arg.ArgumentType ), arg.Value is CILTypeBase ? this.ProcessTypeRef( (CILTypeBase) arg.Value ) : arg.Value );
      //}

      //private CILCustomAttributeNamedArgument ProcessCustomAttributeNamedArg( CILCustomAttributeNamedArgument arg )
      //{
      //   return CILCustomAttributeFactory.NewNamedArgument( this.ProcessCustomAttributeNamedOwner( arg.NamedMember ), this.ProcessCustomAttributeTypedArg( arg.TypedValue ) );
      //}

      //private CILElementForNamedCustomAttribute ProcessCustomAttributeNamedOwner( CILElementForNamedCustomAttribute element )
      //{
      //   return element is CILField ? (CILElementForNamedCustomAttribute) this.ProcessFieldRef( (CILField) element ) : this.ProcessPropertyRef( (CILProperty) element );
      //}

      //private CILProperty ProcessPropertyRef( CILProperty prop )
      //{
      //   return prop == null ?
      //      null :
      //      this._propertyMapping.GetOrAdd( prop, p =>
      //      {
      //         var result = p;
      //         if ( prop.DeclaringType.IsGenericType() && !prop.DeclaringType.IsGenericTypeDefinition() )
      //         {
      //            result = this.ProcessPropertyRef( prop.ChangeDeclaringType( prop.DeclaringType.GenericDefinition.GenericArguments.ToArray() ) )
      //               .ChangeDeclaringType( prop.DeclaringType.GenericArguments.Select( g => this.ProcessTypeRef( g ) ).ToArray() );
      //         }
      //         return result;
      //      } );
      //}

      //private CILEvent ProcessEventRef( CILEvent evt )
      //{
      //   return evt == null ?
      //      null :
      //      this._eventMapping.GetOrAdd( evt, e =>
      //      {
      //         var result = e;
      //         if ( evt.DeclaringType.IsGenericType() && !evt.DeclaringType.IsGenericTypeDefinition() )
      //         {
      //            result = this.ProcessEventRef( evt.ChangeDeclaringType( evt.DeclaringType.GenericDefinition.GenericArguments.ToArray() ) )
      //               .ChangeDeclaringType( evt.DeclaringType.GenericArguments.Select( g => this.ProcessTypeRef( g ) ).ToArray() );
      //         }
      //         return result;
      //      } );
      //}
      //private void ProcessTargetAssemblyAttributes()
      //{
      //   var attrSource = this._options.AttrSource;
      //   if ( attrSource != null )
      //   {
      //      // Copy all attributes from module assembly
      //      //EmittingArguments eArgs; CILModule mod;
      //      //this.LoadModuleAndArgs( attrSource, out eArgs, out mod, ExitCode.AttrFileSpecifiedButDoesNotExist, "There was an error accessing assembly attribute file " + attrSource + "." );
      //      this.ProcessCustomAttributes( this._targetModule, this._assemblyLoader.LoadAssemblyFrom( attrSource ) );
      //   }
      //   else
      //   {
      //      // Primary module always first.
      //      var mods = this._options.CopyAttributes ?
      //         this._inputModules.ToArray() :
      //         new[] { this._primaryModule };

      //      var set = new HashSet<CILCustomAttribute>( ComparerFromFunctions.NewEqualityComparer<CILCustomAttribute>( ( attr1, attr2 ) =>
      //      {
      //         return String.Equals( attr1.Constructor.DeclaringType.GetAssemblyQualifiedName(), attr2.Constructor.DeclaringType.GetAssemblyQualifiedName() );
      //      }, attr => attr.Constructor.DeclaringType.ToString().GetHashCode() ) );
      //      var allowMultipleOption = this._options.AllowMultipleAssemblyAttributes;
      //      foreach ( var mod in mods )
      //      {
      //         foreach ( var ca in mod.Assembly.CustomAttributeData )
      //         {
      //            if ( !set.Add( ca ) )
      //            {
      //               if ( allowMultipleOption
      //                  && ca.Constructor.DeclaringType.CustomAttributeData.Any( ca2 =>
      //                  String.Equals( ATTRIBUTE_USAGE_TYPE.FullName, ca2.Constructor.DeclaringType.GetFullName() )
      //                  && ca2.NamedArguments.Any( na =>
      //                     String.Equals( na.NamedMember.Name, ALLOW_MULTIPLE_PROP.Name )
      //                     && na.TypedValue.Value is Boolean
      //                     && (Boolean) na.TypedValue.Value ) ) )
      //               {
      //                  this.ProcessCustomAttribute( this._targetModule.Assembly, ca );
      //               }
      //               else
      //               {
      //                  set.Remove( ca );
      //                  set.Add( ca );
      //               }
      //            }
      //         }
      //      }
      //      foreach ( var ca in set )
      //      {
      //         this.ProcessCustomAttribute( this._targetModule.Assembly, ca );
      //      }
      //   }
      //}

      //private Boolean IsDuplicateOK( ref String newName, String fullTypeString, TypeAttributes newTypeAttrs )
      //{
      //   var retVal = !this._options.Union && ( !newTypeAttrs.IsVisibleToOutsideOfDefinedAssembly() || this._options.AllowDuplicateTypes == null || this._options.AllowDuplicateTypes.Contains( fullTypeString ) );
      //   if ( retVal )
      //   {
      //      // Have to rename
      //      var i = 2;
      //      var namePrefix = newName;
      //      do
      //      {
      //         newName = namePrefix + i;
      //         ++i;
      //      } while ( !this._allInputTypeNames.Add( newName ) );
      //   }
      //   return retVal;
      //}



      //private void _assemblyLoader_ModuleLoadedEvent( object sender, ModuleLoadedEventArgs e )
      //{
      //   //if (this._inputModules.Count > 0)
      //   //{
      //   // Only react after we have loaded primary module
      //   String rFWName, rFWVersion, rFWProfile;
      //   var primaryArgs = this._assemblyLoader.GetEmittingArgumentsFor( this._inputModules[0] );
      //   if ( primaryArgs.FrameworkName != null
      //       && primaryArgs.FrameworkVersion != null
      //       && e.EmittingArguments.FrameworkName == null
      //       && e.EmittingArguments.FrameworkVersion == null
      //       && this._assemblyLoader.Callbacks.TryGetFrameworkInfo( e.Resource, out rFWName, out rFWVersion, out rFWProfile )
      //       && !String.Equals( primaryArgs.FrameworkName, rFWName ) // || !String.Equals( primaryArgs.FrameworkVersion, rFWVersion ) )
      //      )
      //   {
      //      // We are changing framework (e.g. merging .NET assembly with PCLs)

      //      // Add mapping from loaded assembly to target framework assembly
      //      if ( this._pcl2TargetMapping == null )
      //      {
      //         this._pcl2TargetMapping = new Dictionary<CILAssembly, CILAssembly>();
      //      }
      //      // Key - this framework library, value - primary input module target framework library
      //      var loadedAssembly = e.Module.Assembly;
      //      String targetFWRes;

      //      if ( !this._pcl2TargetMapping.ContainsKey( loadedAssembly )
      //         && this._assemblyLoader.Callbacks.TryGetFrameworkAssemblyPath(
      //         e.Resource,
      //         new CILAssemblyName( loadedAssembly.Name ),
      //         primaryArgs.FrameworkName,
      //         primaryArgs.FrameworkVersion,
      //         primaryArgs.FrameworkProfile,
      //         out targetFWRes
      //         ) )
      //      {
      //         this._pcl2TargetMapping.Add( loadedAssembly, this._assemblyLoader.LoadModuleFrom( targetFWRes ).Assembly );
      //      }
      //   }
      //   //}
      //}

      ////private Byte[] GetPublicKeyToken( Byte[] pk )
      ////{
      ////   var hash = this._csp.Value.ComputeHash( pk );
      ////   return hash.Skip( hash.Length - 8 ).Reverse().ToArray();
      ////}

      //private static CILModule GetMainModuleOrFirst( CILAssembly ass )
      //{
      //   return ass.MainModule ?? ass.Modules[0];
      //}






      //private void MergeResources()
      //{
      //   foreach ( var mod in this._inputModules )
      //   {
      //      foreach ( var kvp in mod.ManifestResources )
      //      {
      //         if ( this._targetModule.ManifestResources.ContainsKey( kvp.Key ) )
      //         {
      //            if ( !this._options.AllowDuplicateResources )
      //            {
      //               this.Log( MessageLevel.Warning, "Ignoring duplicate resource {0} in module {1}.", kvp.Key, mod );
      //            }
      //         }
      //         else
      //         {
      //            var res = kvp.Value;
      //            ManifestResource resourceToAdd = null;
      //            if ( res is EmbeddedManifestResource )
      //            {
      //               var data = ( (EmbeddedManifestResource) res ).Data;
      //               var strm = new MemoryStream( data.Length );
      //               var rw = new System.Resources.ResourceWriter( strm );
      //               Boolean wasResourceManager;
      //               foreach ( var resx in MResourcesIO.GetResourceInfo( data, out wasResourceManager ) )
      //               {
      //                  var resName = resx.Name;
      //                  var resType = resx.Type;
      //                  if ( !resx.IsUserDefinedType && String.Equals( "ResourceTypeCode.String", resType ) )
      //                  {
      //                     // In case there is textual information about types serialized, have to fix that.
      //                     var idx = resx.DataOffset;
      //                     var strlen = data.ReadInt32Encoded7Bit( ref idx );
      //                     rw.AddResource( resName, this.ProcessTypeString( data.ReadStringWithEncoding( ref idx, strlen, Encoding.UTF8 ) ) );
      //                  }
      //                  else
      //                  {
      //                     var newTypeStr = this.ProcessTypeString( resType );
      //                     if ( String.Equals( newTypeStr, resType ) )
      //                     {
      //                        // Predefined ResourceTypeCode or pure reference type, add right away
      //                        var array = new Byte[resx.Item5];
      //                        var dataStart = resx.Item4;
      //                        data.BlockCopyFrom( ref dataStart, array );
      //                        rw.AddResourceData( resName, resType, array );
      //                     }
      //                     else
      //                     {
      //                        // Have to fix records one by one
      //                        var idx = resx.Item4;
      //                        var records = MResourcesIO.ReadNRBFRecords( data, ref idx, idx + resx.Item5 );
      //                        foreach ( var rec in records )
      //                        {
      //                           this.ProcessNRBFRecord( rec );
      //                        }
      //                        var strm2 = new MemoryStream();
      //                        MResourcesIO.WriteNRBFRecords( records, strm2 );
      //                        rw.AddResourceData( resName, newTypeStr, strm2.ToArray() );
      //                     }
      //                  }
      //               }
      //               rw.Generate();
      //               resourceToAdd = new EmbeddedManifestResource( res.Attributes, strm.ToArray() );
      //            }

      //            else
      //            {
      //               var resMod = ( (ModuleManifestResource) res ).Module;
      //               if ( resMod != null && !this._inputModules.Any( m => Object.Equals( m, resMod ) ) )
      //               {
      //                  // Resource module which is not part of the input modules - add it to make an assembly-ref module manifest
      //                  resourceToAdd = res;
      //               }
      //               // TODO - what if resource-module is part of input modules... ? Should we add link to embedded resource?
      //            }
      //            if ( resourceToAdd != null )
      //            {
      //               this._targetModule.ManifestResources.Add( kvp.Key, resourceToAdd );
      //            }
      //         }
      //      }

      //   }
      //}

      //private String ProcessTypeString( String typeStr )
      //{
      //   if ( !String.IsNullOrEmpty( typeStr ) )
      //   {
      //      var idx = typeStr.IndexOf( ", " );
      //      if ( idx > 0 && idx < typeStr.Length - 2 ) // Skip empty type names and type names without assembly name (will be automatically correct)
      //      {
      //         var aStartIdx = idx;
      //         // Skip whitespaces after comma
      //         while ( ++aStartIdx < typeStr.Length && Char.IsWhiteSpace( typeStr[aStartIdx] ) ) ;
      //         var assName = typeStr.Substring( aStartIdx );
      //         // Check whether we actually need to fix the name
      //         if ( !String.Equals( assName, this.ProcessAssemblyName( assName ) ) )
      //         {
      //            typeStr = this.ProcessTypeName( this._assemblyNameCache[assName].Name, typeStr.Substring( 0, idx ) ); // No need for assembly name since resulting type will be in the target assembly
      //         }
      //      }
      //   }

      //   return typeStr;
      //}







      //#region IDisposable Members

      //public void Dispose()
      //{
      //   Exception pdbException;
      //   var pdbOK = this._pdbHelper.DisposeSafely( out pdbException );
      //   Exception cspException = null;
      //   var cspOK = !this._csp.IsValueCreated || this._csp.Value.DisposeSafely( out cspException );
      //   if ( !pdbOK )
      //   {
      //      this.Log( MessageLevel.Warning, "Error writing PDB file for {0}. Error:\n{1}", this._options.OutPath, pdbException );
      //   }
      //   if ( !cspOK )
      //   {
      //      // TODO log
      //      this.Log( MessageLevel.Info, "Error while disposing CSP provider: {0}.", cspException.Message );
      //   }
      //}

      //#endregion
   }

   public class CILMergeException : Exception
   {
      private readonly ExitCode _exitCode;

      internal CILMergeException( ExitCode exitCode, String msg, Exception inner )
         : base( msg, inner )
      {
         this._exitCode = exitCode;
      }

      public ExitCode ExitCode
      {
         get
         {
            return this._exitCode;
         }
      }
   }

   internal static class E_Internal
   {
      //internal static TValue GetOrAdd<TKey, TValue>( this IDictionary<TKey, TValue> dic, TKey key, Func<TKey, TValue> factory )
      //{
      //   TValue val;
      //   if ( !dic.TryGetValue( key, out val ) )
      //   {
      //      val = factory( key );
      //      dic.Add( key, val );
      //   }
      //   return val;
      //}

      //internal static void TryAdd<TKey, TValue>( this IDictionary<TKey, TValue> dic, TKey key, TValue value )
      //{
      //   dic.Add( key, value );
      //}

      internal static Boolean TryAdd_NotThreadSafe<TKey, TValue>( this IDictionary<TKey, TValue> dic, TKey key, TValue value )
      {
         var retVal = !dic.ContainsKey( key );
         if ( retVal )
         {
            dic.Add( key, value );
         }

         return retVal;
      }


   }
}
