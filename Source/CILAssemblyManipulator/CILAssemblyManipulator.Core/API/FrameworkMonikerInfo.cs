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
using System.Threading;
using CILAssemblyManipulator.Implementation;
using CommonUtils;

namespace CILAssemblyManipulator.API
{
   /// <summary>
   /// This class represents information about a single framework moniker.
   /// </summary>
   public class FrameworkMonikerInfo
   {
      private readonly String _fwName;
      private readonly String _fwVersion;
      private readonly String _profileName;
      private readonly IDictionary<String, Tuple<Version, Byte[]>> _assemblies;
      private readonly String _mscorlib;
      private readonly String _fwDisplayName;

      /// <summary>
      /// Creates new instance of <see cref="FrameworkMonikerInfo"/>.
      /// </summary>
      /// <param name="fwName">The framework name. For example, <c>.NETFramework</c> or <c>.NETPortable</c>.</param>
      /// <param name="fwVersion">The framework version. For example, <c>v4.0</c> or <c>v4.5</c>.</param>
      /// <param name="profileName">The framework profile name. For example, <c>Client</c> or <c>Profile5</c>.</param>
      /// <param name="assemblies">Information about assemblies exposed by the framework. The key is assembly name and value is assembly version.</param>
      /// <param name="mscorLibAssemblyName">The name of the assembly which acts as <c>mscorlib</c> assembly, that is, contains types such as <see cref="System.Int32"/>, <see cref="System.Object"/> and all other types essential for framework operation.</param>
      /// <param name="fwDisplayName">The framework display name.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="fwName"/>, <paramref name="fwVersion"/> or <paramref name="assemblies"/> are <c>null</c>.</exception>
      public FrameworkMonikerInfo( String fwName, String fwVersion, String profileName, IDictionary<String, Tuple<Version, Byte[]>> assemblies, String mscorLibAssemblyName, String fwDisplayName )
      {
         ArgumentValidator.ValidateNotNull( "Framework name", fwName );
         ArgumentValidator.ValidateNotEmpty( "Runtime version", fwVersion );
         ArgumentValidator.ValidateNotNull( "Assemblies", assemblies );

         if ( mscorLibAssemblyName == null )
         {
            mscorLibAssemblyName = Consts.MSCORLIB_NAME;
         }

         if ( !assemblies.ContainsKey( mscorLibAssemblyName ) )
         {
            throw new ArgumentException( "Assemblies information must contain the given mscorlib-assembly name (\"" + mscorLibAssemblyName + "\"." );
         }

         this._fwName = fwName;
         this._fwVersion = fwVersion;
         this._profileName = profileName;
         this._assemblies = assemblies;
         this._mscorlib = mscorLibAssemblyName;
         this._fwDisplayName = fwDisplayName;
      }

      /// <summary>
      /// Gets the framework name.
      /// </summary>
      /// <value>The framework name.</value>
      public String FrameworkName
      {
         get
         {
            return this._fwName;
         }
      }

      /// <summary>
      /// Gets the framework version.
      /// </summary>
      /// <value>The framework version.</value>
      public String FrameworkVersion
      {
         get
         {
            return this._fwVersion;
         }
      }

      /// <summary>
      /// Gets the framework profile.
      /// </summary>
      /// <value>The framework profile.</value>
      public String ProfileName
      {
         get
         {
            return this._profileName;
         }
      }

      /// <summary>
      /// Gets the information about assemblies exposed by this framework.
      /// </summary>
      /// <value>The information about assemblies exposed by this framework.</value>
      /// <remarks>
      /// <para>The key is assembly name, and value is assembly version information.</para>
      /// <para>Note: the dictionary is modifiable.</para>
      /// </remarks>
      public IDictionary<String, Tuple<Version, Byte[]>> Assemblies
      {
         get
         {
            return this._assemblies;
         }
      }

      /// <summary>
      /// Gets the name of the assembly containing all types essential for emitting assemblies referencing this framework.
      /// </summary>
      /// <value>The name of the assembly containing all types essential for emitting assemblies referencing this framework.</value>
      /// <remarks>
      /// The assembly with this name must contain types such as <see cref="System.Int32"/>, <see cref="System.Object"/> and other essential types.
      /// </remarks>
      public String MsCorLibAssembly
      {
         get
         {
            return this._mscorlib;
         }
      }

      /// <summary>
      /// Gets the display name of this framework.
      /// </summary>
      /// <value>The display name of this framework.</value>
      public String FrameworkDisplayName
      {
         get
         {
            return this._fwDisplayName;
         }
      }

      /// <summary>
      /// Provides default value for reference assemblies base path on Windows operating system.
      /// This value is <c>C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework</c>.
      /// </summary>
      public const String DEFAULT_REFERENCE_ASSEMBLY_DIR_WINDOWS = @"C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework";

      /// <summary>
      /// Provides default value for reference assemblies base path on Unix operating system.
      /// This value is <c>/usr/lib/mono/xbuild-frameworks</c>.
      /// </summary>
      public const String DEFAULT_REFERENCE_ASSEMBLY_DIR_UNIX = @"/usr/lib/mono/xbuild-frameworks";

      /// <summary>
      /// Provides default value for reference assemblies base path on OSX operating system.
      /// This value is <c>/Library/Frameworks/Mono.framework/External/xbuild-frameworks</c>.
      /// </summary>
      public const String DEFAULT_REFERENCE_ASSEMBLY_DIR_OSX = @"/Library/Frameworks/Mono.framework/External/xbuild-frameworks";

      /// <summary>
      /// The default value for portable class library framework name.
      /// This value is <c>.NETPortable</c>.
      /// </summary>
      public const String DEFAULT_PCL_FW_NAME = ".NETPortable";

      /// <summary>
      /// This method reads assembly information from the <c>FrameworkList.xml</c> located in reference assemblies sud-directory <c>RedistList</c>.
      /// </summary>
      /// <param name="stream">The opened file to <c>FrameworkList.xml</c>.</param>
      /// <param name="assemblyFilenameEnumerator">The callback to enumerate all assembly files in the directory. This will be used if <c>TargetFrameworkDirectory</c> attribute of <c>FileList</c> element is present, and will receive the attribute as parameter. The callback is supposed to return full paths to all assemblies in the specified, potentially relative, directory.</param>
      /// <param name="ctxFactory">The callback to create a new <see cref="CILReflectionContext"/>. This will only be used if <c>TargetFrameworkDirectory</c> attribute is present in <c>FileList</c> element.</param>
      /// <param name="streamOpener">The callback to open assemblies in the target framework directory. This will only be used if <c>TargetFrameworkDirectory</c> attribute is present in <c>FileList</c> element.</param>
      /// <param name="msCorLibName">The detected name of the assembly which acts as <c>mscorlib</c> assembly of this framework.</param>
      /// <param name="frameworkDisplayName">The detected display name of the framework.</param>
      /// <param name="targetFWDir">The detected value of target framework directory, potentially relative.</param>
      /// <returns>Assembly information persisted in the file.</returns>
      /// <exception cref="InvalidDataException">If the <c>FrameworkList.xml</c> is in malformed format.</exception>
      public static IDictionary<String, Tuple<Version, Byte[]>> ReadAssemblyInformationFromRedistXMLFile( Stream stream, Func<String, IEnumerable<String>> assemblyFilenameEnumerator, Func<CILReflectionContext> ctxFactory, Func<String, Stream> streamOpener, out String msCorLibName, out String frameworkDisplayName, out String targetFWDir )
      {
         var xmlSettings = new System.Xml.XmlReaderSettings();
         xmlSettings.CloseInput = false;

         using ( var xml = System.Xml.XmlReader.Create( stream, xmlSettings ) )
         {
            xml.Read();
            // Move to Root and then to File-listing
            if ( !xml.ReadToNextSibling( "FileList" ) )
            {
               throw new InvalidDataException( "FrameworkList.xml seems to be in invalid format (FileList)." );
            }

            frameworkDisplayName = xml.GetAttribute( "Name" );

            var asses = new List<Tuple<String, Version, Byte[]>>();

            // On Mono, .NETFramework assemblies are not enumerated in the FrameworkList.xml (which sucks).
            targetFWDir = xml.GetAttribute( "TargetFrameworkDirectory" );

            if ( String.IsNullOrEmpty( targetFWDir ) )
            {
               if ( !xml.ReadToDescendant( "File" ) )
               {
                  throw new InvalidDataException( "FrameworkList.xml seems to be in invalid format (File)." );
               }

               do
               {
                  asses.Add( Tuple.Create( xml.GetAttribute( "AssemblyName" ), Version.Parse( xml.GetAttribute( "Version" ) ), StringConversions.HexStr2ByteArray( xml.GetAttribute( "PublicKeyToken" ) ) ) );
               } while ( xml.ReadToNextSibling( "File" ) );
            }
            else
            {
               ArgumentValidator.ValidateNotNull( "Assembly file name enumerator", assemblyFilenameEnumerator );
               ArgumentValidator.ValidateNotNull( "CIL Reflection Context factory", ctxFactory );

               using ( var ctx = ctxFactory() )
               {
                  foreach ( var fn in assemblyFilenameEnumerator( targetFWDir ) )
                  {
                     var eArgs = EmittingArguments.CreateForLoadingAssembly();
                     using ( var curStream = streamOpener( fn ) )
                     {
                        var ass = ctx.LoadAssembly( curStream, eArgs );
                        var an = ass.Name;
                        asses.Add( Tuple.Create( an.Name, new Version( an.MajorVersion, an.MinorVersion, an.BuildNumber, an.Revision ), ass.GetPublicKeyToken() ) );
                     }
                  }
               }
            }

            var result = asses.ToDictionary( t => t.Item1, t => Tuple.Create( t.Item2, t.Item3 ) );
            msCorLibName = result.ContainsKey( Consts.NEW_MSCORLIB_NAME ) ? Consts.NEW_MSCORLIB_NAME : Consts.MSCORLIB_NAME;
            return result;
         }
      }

      /// <inheritdoc />
      public override String ToString()
      {
         var retVal = this.FrameworkName + ",Version=" + this.FrameworkVersion;
         if ( !String.IsNullOrEmpty( this.ProfileName ) )
         {
            retVal += ",Profile=" + this.ProfileName;
         }
         return retVal;
      }


   }
}