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
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.IO;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
   public static partial class CILMetaDataIO
   {
      public static CILMetaData ReadModule( this Stream stream, ReadingArguments rArgs = null )
      {
         return CILAssemblyManipulator.Physical.Implementation.ModuleReader.ReadFromStream( stream, rArgs );
      }
   }

   public sealed class HeadersData
   {
      public const Int32 DEFAULT_FILE_ALIGNMENT = 0x200;

      internal const String HINTNAME_FOR_DLL = "_CorDllMain";
      internal const String HINTNAME_FOR_EXE = "_CorExeMain";

      public HeadersData()
         : this( true )
      {

      }

      internal HeadersData( Boolean populateDefaults )
      {
         if ( populateDefaults )
         {
            this.Machine = ImageFileMachine.I386;
            this.ImageBase = 0x00400000;
            this.FileAlignment = DEFAULT_FILE_ALIGNMENT;
            this.SectionAlignment = 0x2000;
            this.StackReserve = 0x100000; // ECMA-335, p. 280
            this.StackCommit = 0x1000; // ECMA-335, p. 280
            this.HeapReserve = 0x100000; // ECMA-335, p. 280
            this.HeapCommit = 0x1000; // ECMA-335, p. 280
            this.ImportDirectoryName = "mscoree.dll"; // ECMA-335, p. 282
            this.ImportHintName = "_CorDllMain"; // ECMA-335, p. 282
            //this.EntryPointInstruction = 0x25FF; // ECMA-335, p. 279
            this.LinkerMajor = 0x0B;
            this.LinkerMinor = 0x00;
            this.OSMajor = 0x04;
            this.OSMinor = 0x00;
            this.UserMajor = 0x00;
            this.UserMinor = 0x00;
            this.SubSysMajor = 0x04;
            this.SubSysMinor = 0x00;
            this.CLIMajor = 2;
            this.CLIMinor = 5;
            this.MetaDataVersion = "v4.0.30319";
            this.TableHeapMajor = 2;
            this.TableHeapMinor = 0;
            this.DLLFlags = DLLFlags.TerminalServerAware | DLLFlags.NXCompatible | DLLFlags.NoSEH | DLLFlags.DynamicBase;
            this.ModuleFlags = ModuleFlags.ILOnly;
         }
      }

      /// <summary>
      /// Gets or set the optional index to MethodDef table where CLR entry point method resides.
      /// </summary>
      /// <value>The optional index to MethodDef table where CLR entry point method resides.</value>
      /// <remarks>Remember to set <see cref="ImportHintName"/> as appropriate for EXE/DLL file.</remarks>
      public TableIndex? CLREntryPointIndex { get; set; }

      /// <summary>
      /// Gets or sets the version string of the metadata (metadata root, 'Version' field).
      /// </summary>
      /// <value>The version string of the metadata (metadata root, 'Version' field) of the module being emitted or loaded..</value>
      public String MetaDataVersion { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="ImageFileMachine"/> of the module (PE file header, 'Machine' field).
      /// </summary>
      /// <value>The <see cref="ImageFileMachine"/> of the module (PE file header, 'Machine' field) of the module being emitted or loaded.</value>
      public ImageFileMachine Machine { get; set; }

      /// <summary>
      /// Gets or sets the major version of the #~ stream.
      /// </summary>
      /// <value>The major version of the #~ stream of the module being emitted or loaded.</value>
      public Byte TableHeapMajor { get; set; }

      /// <summary>
      /// Gets or sets the minor version of the #~ stream.
      /// </summary>
      /// <value>The minor version of the #~ stream of the module being emitted or loaded.</value>
      public Byte TableHeapMinor { get; set; }

      /// <summary>
      /// Gets or sets the base of the emitted image file (PE header, Windows NT-specific, 'Image Base' field).
      /// Should be a multiple of <c>0x10000</c>.
      /// </summary>
      /// <value>The base of the emitted image file (PE header, Windows NT-specific, 'Image Base' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt64 ImageBase { get; set; }

      /// <summary>
      /// Gets or sets the file alignment of the emitted image file (PE header, Windows NT-specific, 'File Alignment' field).
      /// Should be at least <c>0x200</c>.
      /// </summary>
      /// <value>The file alignment of the emitted image file (PE header, Windows NT-specific, 'File Alignment' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt32 FileAlignment { get; set; }

      /// <summary>
      /// Gets or sets the section alignment of the emitted image file (PE header, Windows NT-specific, 'Section Alignment' field).
      /// Should be greater than <see cref="FileAlignment"/>.
      /// </summary>
      /// <value>The section alignment of the emitted image file (PE header, Windows NT-specific, 'Section Alignment' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt32 SectionAlignment { get; set; }

      /// <summary>
      /// Gets or sets the stack reserve size (PE header, Windows NT-specific, 'Stack Reserve Size' field).
      /// Should be <c>0x100000</c>.
      /// </summary>
      /// <value>The stack reserve size (PE header, Windows NT-specific, 'Stack Reserve Size' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt64 StackReserve { get; set; }

      /// <summary>
      /// Gets or sets the stack commit size (PE header, Windows NT-specific, 'Stack Commit Size' field).
      /// Should be <c>0x1000</c>.
      /// </summary>
      /// <value>The stack commit size (PE header, Windows NT-specific, 'Stack Commit Size' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt64 StackCommit { get; set; }

      /// <summary>
      /// Gets or sets the heap reserve size (PE header, Windows NT-specific, 'Heap Reserve Size' field).
      /// Should be <c>0x100000</c>.
      /// </summary>
      /// <value>The heap reserve size (PE header, Windows NT-specific, 'Heap Reserve Size' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt64 HeapReserve { get; set; }

      /// <summary>
      /// Gets or sets the heap commit size (PE header, Windows NT-specific, 'Heap Commit Size' field).
      /// Should be <c>0x1000</c>.
      /// </summary>
      /// <value>The heap commit size (PE header, Windows NT-specific, 'Heap Commit Size' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt64 HeapCommit { get; set; }

      /// <summary>
      /// Gets or sets the name of the entries to import from runtime engine (typically <c>"mscoree.dll"</c>) (Hint/Name table, 'Name' field).
      /// Should be <c>"_CorExeMain"</c> for a .exe file and <c>"_CorDllMain"</c> for a .dll file.
      /// </summary>
      /// <value>The name of the entries to import from runtime engine (typically <c>"mscoree.dll"</c>) (Hint/Name table, 'Name' field) of the module being emitted or loaded.</value>
      public String ImportHintName { get; set; }

      /// <summary>
      /// Gets or sets the name of the runtime engine to import <see cref="ImportHintName"/> from (Import tables, 'Name' field).
      /// Should be <c>"mscoree.dll"</c>.
      /// </summary>
      /// <value>The name of the runtime engine to import <see cref="ImportHintName"/> from (Import tables, 'Name' field) of the module being emitted or loaded.</value>
      public String ImportDirectoryName { get; set; }

      ///// <summary>
      ///// Gets or sets the instruction at PE entrypoint to load the code section.
      ///// It should be <c>0x25FF</c>.
      ///// </summary>
      ///// <value>The instruction at PE entrypoint to load the code section of the module being emitted.</value>
      //public Int16 EntryPointInstruction { get; set; }

      /// <summary>
      /// Gets or sets the major version of the linker (PE header standard, 'LMajor' field).
      /// </summary>
      /// <value>The major version of the linker (PE header standard, 'LMajor' field) of the module being emitted or loaded.</value>
      public Byte LinkerMajor { get; set; }

      /// <summary>
      /// Gets or sets the minor version of the linker (PE header standard, 'LMinor' field).
      /// </summary>
      /// <value>The minor version of the linker (PE header standard, 'LMinor' field) of the module being emitted.</value>
      public Byte LinkerMinor { get; set; }

      /// <summary>
      /// Gets or sets the major version of the OS (PE header, Windows NT-specific, 'OS Major' field).
      /// </summary>
      /// <value>The major version of the OS (PE header, Windows NT-specific, 'OS Major' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt16 OSMajor { get; set; }

      /// <summary>
      /// Gets or sets the minor version of the OS (PE header, Windows NT-specific, 'OS Minor' field).
      /// </summary>
      /// <value>The minor version of the OS (PE header, Windows NT-specific, 'OS Minor' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt16 OSMinor { get; set; }

      /// <summary>
      /// Gets or sets the user-specific major version (PE header, Windows NT-specific, 'User Major' field).
      /// </summary>
      /// <value>The user-specific major version (PE header, Windows NT-specific, 'User Major' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt16 UserMajor { get; set; }

      /// <summary>
      /// Gets or sets the user-specific minor version (PE header, Windows NT-specific, 'User Minor' field).
      /// </summary>
      /// <value>The user-specific minor version (PE header, Windows NT-specific, 'User Minor' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt16 UserMinor { get; set; }

      /// <summary>
      /// Gets or sets the major version of the subsystem (PE header, Windows NT-specific, 'SubSys Major' field).
      /// </summary>
      /// <value>The major version of the subsystem (PE header, Windows NT-specific, 'SubSys Major' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt16 SubSysMajor { get; set; }

      /// <summary>
      /// Gets or sets the minor version of the subsystem (PE header, Windows NT-specific, 'SubSys Minor' field).
      /// </summary>
      /// <value>The minor version of the subsystem (PE header, Windows NT-specific, 'SubSys Minor' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt16 SubSysMinor { get; set; }

      /// <summary>
      /// Gets or sets the major version of the targeted CLI (CLI header, 'MajorRuntimeVersion' field).
      /// Should be at least <c>2</c>.
      /// </summary>
      /// <value>The major version of the targeted CLI (CLI header, 'MajorRuntimeVersion' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt16 CLIMajor { get; set; }

      /// <summary>
      /// Gets or sets the minor version of the targeted CLI (CLI header, 'MinorRuntimeVersion' field).
      /// </summary>
      /// <value>The minor version of the targeted CLI (CLI header, 'MinorRuntimeVersion' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt16 CLIMinor { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="DLLFlags"/>. These flags can e.g. signal whether to use high entropy address space layout randomization ( see <see href="http://msdn.microsoft.com/en-us/library/hh156527.aspx"/> ).
      /// </summary>
      /// <value>Whether to use high entropy address space layout randomization.</value>
      public DLLFlags DLLFlags { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="ModuleFlags"/> of the module (CLI header, 'Flags' field).
      /// </summary>
      /// <value>The <see cref="ModuleFlags"/> of the module (CLI header, 'Flags' field) being emitted or loaded.</value>
      public ModuleFlags ModuleFlags { get; set; }

      public Subsystem? Subsystem { get; set; }

      public Int32? TablesHeaderExtraData { get; set; }

      /// <summary>
      /// During emitting, if this property is not <c>null</c>, then the debug directory with the information specified by <see cref="DebugInformation"/> is written.
      /// During loading, if the PE file contains debug directory, this property is set to reflect the data of the debug directory.
      /// </summary>
      public DebugInformation DebugInformation { get; set; }

      public HeadersData CreateCopy()
      {
         return new HeadersData()
         {
            CLREntryPointIndex = this.CLREntryPointIndex,
            MetaDataVersion = this.MetaDataVersion,
            Machine = this.Machine,
            TableHeapMajor = this.TableHeapMajor,
            TableHeapMinor = this.TableHeapMinor,
            ImageBase = this.ImageBase,
            FileAlignment = this.FileAlignment,
            SectionAlignment = this.SectionAlignment,
            StackReserve = this.StackReserve,
            StackCommit = this.StackCommit,
            HeapReserve = this.HeapReserve,
            HeapCommit = this.HeapCommit,
            ImportHintName = this.ImportHintName,
            ImportDirectoryName = this.ImportDirectoryName,
            //EntryPointInstruction = this.EntryPointInstruction,
            LinkerMajor = this.LinkerMajor,
            LinkerMinor = this.LinkerMinor,
            OSMajor = this.OSMajor,
            OSMinor = this.OSMinor,
            UserMajor = this.UserMajor,
            UserMinor = this.UserMinor,
            SubSysMajor = this.SubSysMajor,
            SubSysMinor = this.SubSysMinor,
            CLIMajor = this.CLIMajor,
            CLIMinor = this.CLIMinor,
            DLLFlags = this.DLLFlags,
            ModuleFlags = this.ModuleFlags,
            Subsystem = this.Subsystem,
            DebugInformation = this.DebugInformation,
         };
      }
   }

   /// <summary>
   /// This class contains information about the debug directory of PE files.
   /// </summary>
   /// <seealso href="http://msdn.microsoft.com/en-us/library/windows/desktop/ms680307%28v=vs.85%29.aspx"/>
   public sealed class DebugInformation
   {
      private Int32 _characteristics;
      private Int32 _timestamp;
      private Int16 _versionMajor;
      private Int16 _versionMinor;
      private Int32 _type;
      private Byte[] _debugDirData;

      /// <summary>
      /// Creates new instance of <see cref="DebugInformation"/>.
      /// </summary>
      public DebugInformation()
         : this( true )
      {
      }

      internal DebugInformation( Boolean setDefaults )
      {
         if ( setDefaults )
         {
            this._type = 2; // CodeView
         }
      }

      /// <summary>
      /// Gets or sets the characteristics field of the debug directory.
      /// </summary>
      /// <value>The characteristics field of the debug directory.</value>
      public Int32 Characteristics
      {
         get
         {
            return this._characteristics;
         }
         set
         {
            this._characteristics = value;
         }
      }

      /// <summary>
      /// Gets or sets the timestamp field of the debug directory.
      /// </summary>
      /// <value>The timestamp field of the debug directory.</value>
      public Int32 Timestamp
      {
         get
         {
            return this._timestamp;
         }
         set
         {
            this._timestamp = value;
         }
      }

      /// <summary>
      /// Gets or sets the major version of the debug directory.
      /// </summary>
      /// <value>The major version of the debug directory.</value>
      public Int16 VersionMajor
      {
         get
         {
            return this._versionMajor;
         }
         set
         {
            this._versionMajor = value;
         }
      }

      /// <summary>
      /// Gets or sets the minor version of the debug directory.
      /// </summary>
      /// <value>The minor version of the debug directory.</value>
      public Int16 VersionMinor
      {
         get
         {
            return this._versionMinor;
         }
         set
         {
            this._versionMinor = value;
         }
      }

      /// <summary>
      /// Gets or sets the type field of the debug directory.
      /// </summary>
      /// <value>The field of the debug directory.</value>
      /// <remarks>By default this is <c>CodeView</c> debug type (<c>2</c>).</remarks>
      public Int32 DebugType
      {
         get
         {
            return this._type;
         }
         set
         {
            this._type = value;
         }
      }

      /// <summary>
      /// Gets or sets the binary data of the debug directory.
      /// </summary>
      /// <value>The binary data of the debug directory.</value>
      public Byte[] DebugData
      {
         get
         {
            return this._debugDirData;
         }
         set
         {
            this._debugDirData = value;
         }
      }
   }

   /// <summary>
   /// This class holds the required information when emitting strong-name assemblies. See ECMA specification for more information about strong named assemblies.
   /// </summary>
   public sealed class StrongNameKeyPair
   {
      private readonly Byte[] _keyPairArray;
      private readonly String _containerName;

      /// <summary>
      /// Creates a <see cref="StrongNameKeyPair"/> based on byte contents of the key pair.
      /// </summary>
      /// <param name="keyPairArray">The byte contents of the key pair.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="keyPairArray"/> is <c>null</c>.</exception>
      public StrongNameKeyPair( Byte[] keyPairArray )
         : this( keyPairArray, null, "Key pair array" )
      {
      }

      /// <summary>
      /// Creates a new <see cref="StrongNameKeyPair"/> based on the name of the container holding key pair.
      /// </summary>
      /// <param name="containerName">The name of the container to be used when creating RSA algorithm.</param>
      /// <remarks>Use this constructor only when you want to use the public key not easily accessible via byte array (eg when using machine key store).</remarks>
      /// <exception cref="ArgumentNullException">If <paramref name="containerName"/> is <c>null</c>.</exception>
      public StrongNameKeyPair( String containerName )
         : this( null, containerName, "Container name" )
      {
      }

      private StrongNameKeyPair( Byte[] keyPairArray, String containerName, String argumentName )
      {
         if ( keyPairArray == null && containerName == null )
         {
            throw new ArgumentNullException( argumentName + " was null." );
         }
         this._keyPairArray = keyPairArray;
         this._containerName = containerName;
      }

      /// <summary>
      /// Gets the byte contents of the key pair. Will be <c>null</c> if this <see cref="StrongNameKeyPair"/> was created based on the name of the container holding key pair.
      /// </summary>
      /// <value>The byte contents of the key pair.</value>
      public IEnumerable<Byte> KeyPair
      {
         get
         {
            return this._keyPairArray == null ? null : this._keyPairArray.Skip( 0 );
         }
      }

      /// <summary>
      /// Gets the name of the container holding the key pair. Will be <c>null</c> if this <see cref="StrongNameKeyPair"/> was created based on the byte contents of the key pair.
      /// </summary>
      /// <value>The name of the container holding the key pair.</value>
      public String ContainerName
      {
         get
         {
            return this._containerName;
         }
      }
   }

   public abstract class IOArguments
   {
      private readonly List<Int32> _methodRVAs;
      private readonly List<Int32> _fieldRVAs;
      private readonly List<Int32?> _embeddedManifestResourceOffsets;

      public IOArguments()
      {
         this._methodRVAs = new List<Int32>();
         this._fieldRVAs = new List<Int32>();
         this._embeddedManifestResourceOffsets = new List<Int32?>();
      }

      /// <summary>
      /// The values are interpreted as unsigned 4-byte integers.
      /// </summary>
      public List<Int32> MethodRVAs
      {
         get
         {
            return this._methodRVAs;
         }
      }

      /// <summary>
      /// The values are interpreted as unsigned 4-byte integers.
      /// </summary>
      public List<Int32> FieldRVAs
      {
         get
         {
            return this._fieldRVAs;
         }
      }

      /// <summary>
      /// If has value, then <see cref="ManifestResource"/> at this index is embedded in the module, and is located at given offset. Otherwise, the <see cref="ManifestResource"/> is located at another file.
      /// </summary>
      public List<Int32?> EmbeddedManifestResourceOffsets
      {
         get
         {
            return this._embeddedManifestResourceOffsets;
         }
      }

      public HeadersData Headers { get; set; }

      public Byte[] StrongNameHashValue { get; set; }
   }

   /// <summary>
   /// Class containing information specific to reading the module.
   /// </summary>
   public sealed class ReadingArguments : IOArguments
   {
      public ReadingArguments( Boolean createHeaders = true )
      {
         if ( createHeaders )
         {
            this.Headers = new HeadersData( false );
         }
      }
   }

   /// <summary>
   /// Class containing information specific to writing the module.
   /// </summary>
   public sealed class EmittingArguments : IOArguments
   {

      /// <summary>
      /// During emitting, if the module is main module and should be strong-name signed, this <see cref="StrongNameKeyPair"/> will be used.
      /// Set to <c>null</c> if the module should not be strong-name signed.
      /// </summary>
      /// <value>The strong name of the module being emitted.</value>
      public StrongNameKeyPair StrongName { get; set; }

      /// <summary>
      /// During emitting, if the module is main module and should be strong-name signed, this property may be used to override the algorithm specified by key BLOB of <see cref="StrongName"/>.
      /// If this property does not have a value, the algorithm specified by key BLOB of <see cref="StrongName"/> will be used.
      /// If the key BLOB of <see cref="StrongName"/> does not specify an algorithm, the assembly will be signed using <see cref="AssemblyHashAlgorithm.SHA1"/>.
      /// </summary>
      /// <value>The algorithm to compute a hash over emitted assembly data.</value>
      /// <remarks>
      /// If <see name="AssemblyHashAlgorithm.MD5"/> or <see cref="AssemblyHashAlgorithm.None"/> is specified, the <see cref="AssemblyHashAlgorithm.SHA1"/> will be used instead.
      /// </remarks>
      public AssemblyHashAlgorithm? SigningAlgorithm { get; set; }

      /// <summary>
      /// During emitting, if the module is main module and should be strong-name signed, setting this to <c>true</c> will only leave room for the hash, without actually computing it.
      /// </summary>
      /// <value>Whether to delay signing procedure.</value>
      public Boolean DelaySign { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="ModuleKind"/> of the module.
      /// </summary>
      /// <value>The <see cref="ModuleKind"/> of the module being emitted or loaded.</value>
      public ModuleKind ModuleKind { get; set; }

      public CryptoCallbacks CryptoCallbacks { get; set; }

      public WriterFunctionalityProvider WriterFunctionality { get; set; }

   }

   /// <summary>
   /// This exception is thrown whenever something goes wrong when emitting a strong-signed module.
   /// </summary>
   public class CryptographicException : Exception
   {
      internal CryptographicException( String msg )
         : this( msg, null )
      {

      }

      internal CryptographicException( String msg, Exception inner )
         : base( msg, inner )
      {

      }
   }

   /// <summary>
   /// This will be thrown by <see cref="CILMetaDataIO.ReadModule"/> method if the target file does not contain a managed assembly.
   /// </summary>
   public class NotAManagedModuleException : Exception
   {
      internal NotAManagedModuleException()
      {

      }
   }

   //[AttributeUsage( AttributeTargets.Property | AttributeTargets.ReturnValue | AttributeTargets.Parameter, AllowMultiple = false )]
   //public sealed class InterpretAsUnsignedAttribute : Attribute
   //{

   //}

}

public static partial class E_CILPhysical
{
   public static void WriteModule( this CILMetaData md, Stream stream, EmittingArguments eArgs = null )
   {
      if ( eArgs == null )
      {
         eArgs = new EmittingArguments();
      }

      if ( eArgs.Headers == null )
      {
         eArgs.Headers = new HeadersData();
      }

      var writerProvider = eArgs.WriterFunctionality ?? new DefaultWriterFunctionalityProvider();
      CILMetaData newMD;
      var writer = writerProvider.GetFunctionality( md, eArgs.Headers, out newMD );

      CILAssemblyManipulator.Physical.Implementation.ModuleWriter.WriteToStream( writer, newMD ?? md, eArgs, stream );
   }
}