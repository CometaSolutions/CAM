///*
// * Copyright 2014 Stanislav Muhametsin. All rights Reserved.
// *
// * Licensed  under the  Apache License,  Version 2.0  (the "License");
// * you may not use  this file  except in  compliance with the License.
// * You may obtain a copy of the License at
// *
// *   http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed  under the  License is distributed on an "AS IS" BASIS,
// * WITHOUT  WARRANTIES OR CONDITIONS  OF ANY KIND, either  express  or
// * implied.
// *
// * See the License for the specific language governing permissions and
// * limitations under the License. 
// */
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Collections.Concurrent;
//using System.IO;
//using CommonUtils;

//namespace CILAssemblyManipulator.Logical
//{
//   /// <summary>
//   /// This class implements <see cref="CILAssemblyLoaderCallbacks"/> with default functionality of loading assemblies from files.
//   /// </summary>
//   public class DotNETCILAssemblyLoaderCallbacks : CILAssemblyLoaderCallbacks
//   {
//      private readonly String _referenceAssembliesRes;
//      private readonly ConcurrentDictionary<FrameworkMonikerID, FrameworkMonikerInfo> _monikers;
//      private readonly ConcurrentDictionary<FrameworkMonikerInfo, String> _explicitDirectories;

//      /// <summary>
//      /// Creates a new instance of <see cref="DotNETCILAssemblyLoaderCallbacks"/> with given reference assemblies directory.
//      /// </summary>
//      /// <param name="referenceAssembliesDir">The directory containing reference assemblies information. See <see cref="DotNETReflectionContext.GetDefaultReferenceAssemblyPath"/> for more info.</param>
//      public DotNETCILAssemblyLoaderCallbacks( String referenceAssembliesDir )
//      {
//         this._referenceAssembliesRes = Path.GetFullPath( String.IsNullOrEmpty( referenceAssembliesDir ) ? DotNETReflectionContext.GetDefaultReferenceAssemblyPath() : referenceAssembliesDir );
//         this._monikers = new ConcurrentDictionary<FrameworkMonikerID, FrameworkMonikerInfo>();
//         this._explicitDirectories = new ConcurrentDictionary<FrameworkMonikerInfo, String>();
//      }

//      /// <inheritdoc />
//      public System.IO.Stream OpenStream( String resource )
//      {
//         return File.Open( resource, FileMode.Open, FileAccess.Read, FileShare.Read );
//      }

//      /// <inheritdoc />
//      public String CleanResource( String path )
//      {
//         return Path.GetFullPath( path );
//      }

//      /// <inheritdoc />
//      public Boolean TryResolveAssemblyFilePath( String thisModulePath, CILAssemblyManipulator.API.CILAssemblyName referencedAssembly, out String referencedAssemblyPath )
//      {
//         var dirName = Path.GetDirectoryName( thisModulePath );
//         // Check if .dll or .exe exists in same directory
//         var dllName = Path.Combine( dirName, referencedAssembly.Name + ".dll" );
//         var exeName = Path.Combine( dirName, referencedAssembly.Name + ".exe" );
//         var winmdName = Path.Combine( dirName, referencedAssembly.Name + ".winmd" );
//         // TODO other extensions?
//         referencedAssemblyPath = new[] { dllName, exeName, winmdName }.FirstOrDefault( fn => File.Exists( fn ) );
//         return referencedAssemblyPath != null;
//      }

//      /// <inheritdoc />
//      public Boolean TryGetFrameworkInfo( String thisModulePath, out String fwName, out String fwVersion, out String fwProfile )
//      {
//         var dir = Path.GetDirectoryName( thisModulePath );
//         fwName = null;
//         fwVersion = null;
//         fwProfile = null;
//         var retVal = dir.StartsWith( this._referenceAssembliesRes, Environment.OSVersion.Platform.FileNamesCaseSensitive() ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase ) && dir.Length > this._referenceAssembliesRes.Length;
//         if ( retVal )
//         {
//            dir = dir.Substring( this._referenceAssembliesRes.Length );
//            var dirs = dir.Split( new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries );
//            retVal = dirs.Length >= 2;
//            if ( retVal )
//            {
//               fwName = dirs[0];
//               fwVersion = dirs[1];
//               fwProfile = dirs.Length >= 4 ? dirs[3] : null;
//            }
//         }
//         else
//         {
//            // See if this framework is explicitly defined elsewhere
//            var fwInfo = this._explicitDirectories.Where( kvp => String.Equals( dir, kvp.Value ) ).Select( kvp => kvp.Key ).FirstOrDefault();
//            retVal = fwInfo != null;
//            if ( retVal )
//            {
//               fwName = fwInfo.FrameworkName;
//               fwVersion = fwInfo.FrameworkVersion;
//               fwProfile = fwInfo.ProfileName;
//            }
//         }
//         return retVal;
//      }

//      /// <inheritdoc />
//      public Boolean TryGetFrameworkMoniker( String fwName, String fwVersion, String fwProfile, out FrameworkMonikerInfo moniker )
//      {
//         var retVal = !String.IsNullOrEmpty( fwName ) && !String.IsNullOrEmpty( fwVersion );
//         if ( retVal )
//         {
//            var key = new FrameworkMonikerID( fwName, fwVersion, fwProfile );
//            retVal = this._monikers.TryGetValue( key, out moniker );
//            if ( !retVal )
//            {
//               var dir = this.GetDirectory( fwName, fwVersion, fwProfile );
//               retVal = Directory.Exists( dir );
//               if ( retVal )
//               {
//                  var redistListDir = Path.Combine( dir, "RedistList" );
//                  var fn = Path.Combine( redistListDir, "FrameworkList.xml" );
//                  String msCorLibName; String fwDisplayName; String targetFWDir;
//                  moniker = new FrameworkMonikerInfo( fwName, fwVersion, fwProfile, DotNETReflectionContext.ReadAssemblyInformationFromRedistXMLFile(
//                           fn,
//                           out msCorLibName,
//                           out fwDisplayName,
//                           out targetFWDir
//                           ), msCorLibName, fwDisplayName );
//                  if ( !String.IsNullOrEmpty( targetFWDir ) )
//                  {
//                     this._explicitDirectories.TryAdd( moniker, targetFWDir );
//                  }
//               }
//            }
//         }
//         else
//         {
//            moniker = null;
//         }
//         return retVal;
//      }

//      /// <inheritdoc />
//      public Boolean TryGetFrameworkAssemblyPath( String thisModulePath, CILAssemblyManipulator.API.CILAssemblyName referencedAssembly, String fwName, String fwVersion, String fwProfile, out String fwAssemblyPath )
//      {
//         // TODO Additional checks
//         fwAssemblyPath = Path.Combine( this.GetDirectory( fwName, fwVersion, fwProfile ), referencedAssembly.Name + ".dll" );
//         return true;
//      }

//      private String GetDirectory( String fwName, String fwVersion, String fwProfile )
//      {
//         var retVal = Path.Combine( this._referenceAssembliesRes, fwName, fwVersion );
//         if ( !String.IsNullOrEmpty( fwProfile ) )
//         {
//            retVal = Path.Combine( retVal, "Profile", fwProfile );
//         }
//         return retVal;
//      }
//   }

//   /// <summary>
//   /// This struct provides uniform way of identifying target frameworks.
//   /// </summary>
//   public struct FrameworkMonikerID : IEquatable<FrameworkMonikerID>
//   {
//      private readonly String _fwTextualID;
//      private readonly String _fwVersionString;
//      private readonly String _fwProfile;

//      /// <summary>
//      /// Creates a new instance of <see cref="FrameworkMonikerID"/>.
//      /// </summary>
//      /// <param name="fwTextualID">The framework identifier string.</param>
//      /// <param name="fwVersionString">The framework version string.</param>
//      /// <param name="fwProfile">The optional framework profile string.</param>
//      /// <exception cref="ArgumentNullException">If <paramref name="fwTextualID"/> or <paramref name="fwVersionString"/> are <c>null</c>.</exception>
//      /// <exception cref="ArgumentException">If <paramref name="fwTextualID"/> or <paramref name="fwVersionString"/> are empty.</exception>
//      public FrameworkMonikerID( String fwTextualID, String fwVersionString, String fwProfile )
//      {
//         ArgumentValidator.ValidateNotEmpty( "Framework textual ID", fwTextualID );
//         ArgumentValidator.ValidateNotEmpty( "Framework version string", fwVersionString );

//         this._fwTextualID = fwTextualID;
//         this._fwVersionString = fwVersionString;
//         this._fwProfile = fwProfile;
//      }

//      /// <summary>
//      /// Gets the identifier string of this framework.
//      /// </summary>
//      /// <value>The identifier string of this framework.</value>
//      public String FrameworkTextualID
//      {
//         get
//         {
//            return this._fwTextualID;
//         }
//      }

//      /// <summary>
//      /// Gets the version string of this framework.
//      /// </summary>
//      /// <value>The version string of this framework.</value>
//      public String FrameworkVersionString
//      {
//         get
//         {
//            return this._fwVersionString;
//         }
//      }

//      /// <summary>
//      /// Gets the optional profile string of this framework.
//      /// </summary>
//      /// <value>The optional profile string of this framework.</value>
//      public String FrameworkProfile
//      {
//         get
//         {
//            return this._fwProfile;
//         }
//      }

//      /// <inheritdoc />
//      public Boolean Equals( FrameworkMonikerID other )
//      {
//         return String.Equals( this._fwTextualID, other._fwTextualID )
//            && String.Equals( this._fwVersionString, other._fwVersionString )
//            && String.Equals( this._fwProfile, other._fwProfile );
//      }

//      /// <inheritdoc />
//      public override Boolean Equals( object obj )
//      {
//         return obj is FrameworkMonikerID && this.Equals( (FrameworkMonikerID) obj );
//      }

//      /// <inheritdoc />
//      public override Int32 GetHashCode()
//      {
//         unchecked
//         {
//            return ( ( 17 * 23 + this._fwTextualID.GetHashCodeSafe() ) * 23 + this._fwVersionString.GetHashCodeSafe() ) * 23 + this._fwProfile.GetHashCodeSafe();
//         }
//      }

//      /// <inheritdoc />
//      public override string ToString()
//      {
//         var retVal = this._fwTextualID + ", " + this._fwVersionString;
//         if ( !String.IsNullOrEmpty( this._fwProfile ) )
//         {
//            retVal += ", " + this._fwProfile;
//         }
//         return retVal;
//      }
//   }
//}
