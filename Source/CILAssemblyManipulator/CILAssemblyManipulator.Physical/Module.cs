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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommonUtils;

namespace CILAssemblyManipulator.Physical
{
   public interface CILModuleData
   {
      HeadersData Headers { get; }
      CILMetaData MetaData { get; }
   }

   public sealed class HeadersData
   {
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
            this.FileAlignment = 0x200;
            this.SectionAlignment = 0x2000;
            this.StackReserve = 0x100000; // ECMA-335, p. 280
            this.StackCommit = 0x1000; // ECMA-335, p. 280
            this.HeapReserve = 0x100000; // ECMA-335, p. 280
            this.HeapCommit = 0x1000; // ECMA-335, p. 280
            this.ImportDirectoryName = "mscoree.dll"; // ECMA-335, p. 282
            this.EntryPointInstruction = 0x25FF; // ECMA-335, p. 279
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
            this.ModuleFlags = ModuleFlags.ILOnly;
         }
      }

      /// <summary>
      /// Gets or set the optional index to MethodDef table where CLR entry point method resides.
      /// </summary>
      /// <value>The optional index to MethodDef table where CLR entry point method resides.</value>
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

      /// <summary>
      /// Gets or sets the instruction at PE entrypoint to load the code section.
      /// It should be <c>0x25FF</c>.
      /// </summary>
      /// <value>The instruction at PE entrypoint to load the code section of the module being emitted.</value>
      public Int16 EntryPointInstruction { get; set; }

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
      /// Gets or sets the flag signalling to use high entropy address space layout randomization ( see <see href="http://msdn.microsoft.com/en-us/library/hh156527.aspx"/> ).
      /// </summary>
      /// <value>Whether to use high entropy address space layout randomization.</value>
      public Boolean HighEntropyVA { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="ModuleFlags"/> of the module (CLI header, 'Flags' field).
      /// </summary>
      /// <value>The <see cref="ModuleFlags"/> of the module (CLI header, 'Flags' field) being emitted or loaded.</value>
      public ModuleFlags ModuleFlags { get; set; }

      /// <summary>
      /// During emitting, if this property is not <c>null</c>, then the debug directory with the information specified by <see cref="DebugInformation"/> is written.
      /// During loading, if the PE file contains debug directory, this property is set to reflect the data of the debug directory.
      /// </summary>
      public DebugInformation DebugInformation { get; set; }
   }

   public interface CILMetaData
   {
      List<ModuleDefinition> ModuleDefinitions { get; }

      List<TypeReference> TypeReferences { get; }

      List<TypeDefinition> TypeDefinitions { get; }

      List<FieldDefinition> FieldDefinitions { get; }

      List<MethodDefinition> MethodDefinitions { get; }

      List<ParameterDefinition> ParameterDefinitions { get; }

      List<InterfaceImplementation> InterfaceImplementations { get; }

      List<MemberReference> MemberReferences { get; }

      List<ConstantDefinition> ConstantDefinitions { get; }

      List<CustomAttributeDefinition> CustomAttributeDefinitions { get; }

      List<FieldMarshal> FieldMarshals { get; }

      List<SecurityDefinition> SecurityDefinitions { get; }

      List<ClassLayout> ClassLayouts { get; }

      List<FieldLayout> FieldLayouts { get; }

      List<StandaloneSignature> StandaloneSignatures { get; }

      List<EventMap> EventMaps { get; }

      List<EventDefinition> EventDefinitions { get; }

      List<PropertyMap> PropertyMaps { get; }

      List<PropertyDefinition> PropertyDefinitions { get; }

      List<MethodSemantics> MethodSemantics { get; }

      List<MethodImplementation> MethodImplementations { get; }

      List<ModuleReference> ModuleReferences { get; }

      List<TypeSpecification> TypeSpecifications { get; }

      List<MethodImplementationMap> MethodImplementationMaps { get; }

      List<FieldRVA> FieldRVAs { get; }

      List<AssemblyDefinition> AssemblyDefinitions { get; }

      List<AssemblyReference> AssemblyReferences { get; }

      List<FileReference> FileReferences { get; }

      List<ExportedType> ExportedTypes { get; }

      List<ManifestResource> ManifestResources { get; }

      List<NestedClassDefinition> NestedClassDefinitions { get; }

      List<GenericParameterDefinition> GenericParameterDefinitions { get; }

      List<MethodSpecification> MethodSpecifications { get; }

      List<GenericParameterConstraintDefinition> GenericParameterConstraintDefinitions { get; }
   }

   public static class CILMetaDataFactory
   {
      public static CILMetaData NewMetaData()
      {
         return new CILAssemblyManipulator.Physical.Implementation.CILMetadataImpl();
      }
   }

   public sealed class ModuleReadResult
   {
      private readonly CILMetaData _md;
      private readonly HeadersData _headers;

      internal ModuleReadResult( CILMetaData md, HeadersData headers )
      {
         ArgumentValidator.ValidateNotNull( "Metadata", md );
         //ArgumentValidator.ValidateNotNull( "Headers", headers );

         this._md = md;
         this._headers = headers;
      }

      public CILMetaData MetaData
      {
         get
         {
            return this._md;
         }
      }

      public HeadersData Headers
      {
         get
         {
            return this._headers;
         }
      }
   }

   public sealed class ModuleWriteResult
   {
      // TODO: hash value for .netmodules part of strong-named assemblies
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
            this._type = MetaDataConstants.CODE_VIEW_DEBUG_TYPE;
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
}
