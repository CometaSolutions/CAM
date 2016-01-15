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
using ApplicationParameters;
using CILAssemblyManipulator.Physical;

namespace CILMerge
{
   internal class CILMerge
   {
      private const String OUT = "out";
      private const String KEY_FILE = "keyfile";
      private const String SIGN_ALGORITHM = "signingalgorithm";
      private const String LOG = "log";
      private const String VER = "ver";
      private const String UNION = "union";
      private const String UNION_EXCLUDE = "unionexclude";
      private const String NODEBUG = "ndebug";
      private const String COPY_ATTRS = "copyattrs";
      private const String ATTR = "attr";
      private const String ALLOW_MULTIPLE = "allowmultiple";
      private const String TARGET = "target";
      private const String TARGET_PLATFORM = "targetplatform";
      private const String XML_DOCS = "xmldocs";
      private const String LIB = "lib";
      private const String INTERNALIZE = "internalize";
      private const String INTERNALIZE_EXCLUDE = "internalizeexclude";
      private const String DELAY_SIGN = "delaysign";
      private const String USE_FULL_PUBLIC_KEY_FOR_REFERENCES = "usefullpublickeyforreferences";
      private const String ALIGN = "align";
      private const String CLOSED = "closed";
      private const String ALLOW_DUP = "allowdup";
      private const String ALLOW_DUP_RES = "allowduplicateresources";
      private const String ZERO_PE_KIND = "zeropekind";
      private const String PARALLEL = "parallel";
      private const String PAUSE = "pause";
      private const String VERBOSE = "verbose";
      private const String WILDCARDS = "wildcards";
      private const String RENAME_FILE = "renamefile";

      private const String SUBSYSTEMVERSION = "subsystemversion";
      private const String HIGH_ENTROPY_VA = "highentropyva";

      private const Int32 MIN_FILE_ALIGN = 0x200;


      internal static Int32 Main( String[] args )
      {
         ExitCode retVal;
         SimpleApplicationParameters paramInstance = null;
         try
         {
            var paramModel = new SimpleApplicationParametersModel(
               "Files",
               new StringOptionModel( "help", "?" ),
               new StringOptionModel( OUT ),
               new OptionModel[]
               {
                  new StringOptionModel(KEY_FILE),
                  new EnumOptionModel<AssemblyHashAlgorithm>(SIGN_ALGORITHM, true),
                  new StringOptionModel(LOG),
                  new ListOptionModel(VER, new OptionModel(typeof(UInt16), null), ".", 4, 4),
                  new StringOptionModel(UNION),
                  new StringOptionModel(UNION_EXCLUDE),
                  new SwitchOptionModel(NODEBUG),
                  new SwitchOptionModel(COPY_ATTRS),
                  new StringOptionModel(ATTR),
                  new SwitchOptionModel(ALLOW_MULTIPLE),
                  new EnumOptionModel<ModuleKind>(TARGET, true),
                  new StringOptionModel(TARGET_PLATFORM),
                  new SwitchOptionModel(XML_DOCS),
                  new StringOptionModel(LIB),
                  new StringOptionModel(INTERNALIZE),
                  new StringOptionModel(INTERNALIZE_EXCLUDE),
                  new SwitchOptionModel(DELAY_SIGN),
                  new SwitchOptionModel(USE_FULL_PUBLIC_KEY_FOR_REFERENCES),
                  new OptionModel(typeof(Int32), ALIGN),
                  new SwitchOptionModel(CLOSED),
                  new StringOptionModel(ALLOW_DUP),
                  new SwitchOptionModel(ALLOW_DUP_RES),
                  new SwitchOptionModel(ZERO_PE_KIND),
                  new SwitchOptionModel(PARALLEL),
                  new SwitchOptionModel(PAUSE),
                  new SwitchOptionModel(VERBOSE),
                  new SwitchOptionModel(WILDCARDS),
                  new StringOptionModel(SUBSYSTEMVERSION, new System.Text.RegularExpressions.Regex(@"\d{1,5}\.\d{1,5}")),
                  new SwitchOptionModel(HIGH_ENTROPY_VA),
                  new StringOptionModel(RENAME_FILE)
               },
               1,
               LIB,
               ALLOW_DUP
               );
            paramInstance = new SimpleApplicationParameters( paramModel, args );
            if ( paramInstance.Errors.Count > 0 )
            {
               Console.Error.WriteLine( "Errors in command-line arguments:\n" + String.Join( "\n", paramInstance.Errors ) );
               retVal = ExitCode.ExceptionDuringStartup;
            }
            else if ( paramInstance.HelpOptionPresent )
            {
               Console.WriteLine( "TODO Help." );
               retVal = ExitCode.Success;
            }
            else
            {
               String logFileName;
               var options = FromApplicationOptions( paramInstance, out logFileName );
               CILMergeLogCallback logCallback;
               Stream logStream = null;
#if DEBUG
               options.DoLogging = true;
               logCallback = new CILMergeLogCallbackImpl();
#else
               if (options.DoLogging)
               {
                  if (String.IsNullOrEmpty(logFileName))
                  {
                     logCallback = new ConsoleCILMergeLogCallback();
                  } else
                  {
                     logStream = new StreamWriter( logFileName, false, Encoding.UTF8 );
                     logCallback = new StreamWriterCILMergeLogCallback( logStream );
                  }
               }
#endif
               try
               {
                  var merger = new CILMerger( options, logCallback );
                  merger.PerformMerge();
               }
               finally
               {
                  logStream.DisposeSafely();
               }
               retVal = ExitCode.Success;
            }
         }
         catch ( CILMergeException cExc )
         {
            retVal = cExc.ExitCode;
            Console.Error.WriteLine( "Error: " + cExc.Message );
         }
         catch ( Exception exc )
         {
            Console.Error.WriteLine( "An exception occurred:\n" + exc );
            retVal = ExitCode.ExceptionDuringMerge;
         }
         if ( paramInstance != null && paramInstance.GetSingleOptionOrNull( PAUSE ).GetOrDefault( false ) )
         {
            Console.ReadKey();
         }
         return (Int32) retVal;
      }

      private static CILMergeOptionsImpl FromApplicationOptions( SimpleApplicationParameters args, out String logFileName )
      {
         var options = new CILMergeOptionsImpl();

         options.OutPath = args.SeparatorOption.OptionValueAsString;
         options.KeyFile = args.GetSingleOptionOrNull( KEY_FILE ).GetOrDefault<String>();
         options.SigningAlgorithm = args.GetSingleOptionOrNull( SIGN_ALGORITHM ).GetOrDefault<AssemblyHashAlgorithm?>();

         var logOption = args.GetSingleOptionOrNull( LOG );
         options.DoLogging = logOption != null;
         logFileName = logOption == null ? null : logOption.OptionValueAsString;

         var ver = args.GetSingleOptionOrNull( VER ).GetOrDefault<List<UInt16>>();
         if ( ver != null )
         {
            options.VerMajor = ver[0];
            options.VerMinor = ver[1];
            options.VerBuild = ver[2];
            options.VerRevision = ver[3];
         }
         else
         {
            options.VerMajor = options.VerMinor = options.VerBuild = options.VerRevision = -1;
         }
         options.Union = args.GetSingleOptionOrNull( UNION )?.OptionValueAsString;
         options.UnionExcludeFile = args.GetSingleOptionOrNull( UNION_EXCLUDE )?.OptionValueAsString;
         options.NoDebug = args.GetSingleOptionOrNull( NODEBUG ).GetOrDefault( false );
         options.CopyAttributes = args.GetSingleOptionOrNull( COPY_ATTRS ).GetOrDefault( false );
         options.TargetAssemblyAttributeSource = args.GetSingleOptionOrNull( ATTR ).GetOrDefault<String>();
         options.AllowMultipleAssemblyAttributes = args.GetSingleOptionOrNull( ALLOW_MULTIPLE ).GetOrDefault( false );
         options.Target = args.GetSingleOptionOrNull( TARGET ).GetOrDefault<ModuleKind?>();
         var tp = args.GetSingleOptionOrNull( TARGET_PLATFORM ).GetOrDefault<String>();
         if ( tp != null )
         {
            var idx = tp.IndexOf( "," );
            var trString = idx == -1 ? tp : tp.Substring( 0, idx );
            switch ( trString )
            {
               case "v1":
                  options.MetadataVersionString = CILMergeOptionsImpl.MD_NET_1_0;
                  break;
               case "v1.1":
                  options.MetadataVersionString = CILMergeOptionsImpl.MD_NET_1_1;
                  break;
               case "v2":
                  options.MetadataVersionString = CILMergeOptionsImpl.MD_NET_2_0;
                  break;
               case "v4":
                  options.MetadataVersionString = CILMergeOptionsImpl.MD_NET_4_0;
                  break;
               default:
                  options.MetadataVersionString = trString;
                  break;
            }
         }

         options.XmlDocs = args.GetSingleOptionOrNull( XML_DOCS ).GetOrDefault( false );
         options.LibPaths = args.GetMultipleOptionsOrEmpty( LIB ).Select( o => o.OptionValueAsString ).ToArray();
         options.Internalize = args.GetSingleOptionOrNull( INTERNALIZE )?.OptionValueAsString;
         options.InternalizeExcludeFile = args.GetSingleOptionOrNull( INTERNALIZE_EXCLUDE )?.OptionValueAsString;
         options.DelaySign = args.GetSingleOptionOrNull( DELAY_SIGN ).GetOrDefault( false );
         options.UseFullPublicKeyForRefs = args.GetSingleOptionOrNull( USE_FULL_PUBLIC_KEY_FOR_REFERENCES ).GetOrDefault( false );
         options.FileAlign = args.GetSingleOptionOrNull( ALIGN ).GetOrDefault( MIN_FILE_ALIGN );
         options.Closed = args.GetSingleOptionOrNull( CLOSED ).GetOrDefault( false );
         var dups = args.GetMultipleOptionsOrEmpty( ALLOW_DUP );
         options.AllowDuplicateTypes = dups.Count > 0 ?
            ( dups.Any( d => String.IsNullOrEmpty( d.OptionValueAsString ) ) ?
               null :
               new HashSet<String>( dups.Where( d => !String.IsNullOrEmpty( d.OptionValueAsString ) ).Select( d => d.OptionValueAsString ).ToArray() ) ) :
               new HashSet<String>();
         options.AllowDuplicateResources = args.GetSingleOptionOrNull( ALLOW_DUP_RES ).GetOrDefault( false );
         options.ZeroPEKind = args.GetSingleOptionOrNull( ZERO_PE_KIND ).GetOrDefault( false );
         options.Parallel = args.GetSingleOptionOrNull( PARALLEL ).GetOrDefault( false );
         options.Verbose = args.GetSingleOptionOrNull( VERBOSE ).GetOrDefault( false );
         options.InputAssemblies = args.Values.Distinct().ToArray();
         options.AllowWildCards = args.GetSingleOptionOrNull( WILDCARDS ).GetOrDefault<Boolean>();

         options.OutPath = RootPath( options.OutPath );
         if ( !options.AllowWildCards )
         {
            for ( var i = 0; i < options.InputAssemblies.Length; ++i )
            {
               options.InputAssemblies[i] = RootPath( options.InputAssemblies[i] );
            }
         }
         options.KeyFile = RootPath( options.KeyFile );
         options.TargetAssemblyAttributeSource = RootPath( options.TargetAssemblyAttributeSource );
         options.InternalizeExcludeFile = RootPath( options.InternalizeExcludeFile );
         options.RenameFile = args.GetSingleOptionOrNull( RENAME_FILE )?.OptionValueAsString;

         var subSysStr = args.GetSingleOptionOrNull( SUBSYSTEMVERSION ).GetOrDefault( "4.0" );
         var sep = subSysStr.IndexOf( '.' );
         options.SubsystemMajor = Int32.Parse( subSysStr.Substring( 0, sep ) );
         options.SubsystemMinor = Int32.Parse( subSysStr.Substring( sep + 1 ) );
         options.HighEntropyVA = args.GetSingleOptionOrNull( HIGH_ENTROPY_VA ).GetOrDefault( false );

         return options;
      }

      private static String RootPath( String path )
      {
         if ( path != null && !Path.IsPathRooted( path ) )
         {
            path = Path.GetFullPath( path );
         }
         return path;
      }
   }

#if DEBUG
   internal class CILMergeLogCallbackImpl : AbstractCILMergeLogCallback
   {

      #region CILMergeLogCallback Members

      public override void Log( MessageLevel mLevel, String formatString, Object[] args )
      {
         System.Diagnostics.Debug.WriteLine( CreateMessageString( mLevel, formatString, args ) );
      }

      #endregion
   }
#endif
}
