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
using System.Threading;
using CILAssemblyManipulator.API;
using CILAssemblyManipulator.Implementation.Physical;
using CommonUtils;

namespace CILAssemblyManipulator.API
{
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

   /// <summary>
   /// This interface is used by <see cref="E_CIL.EmitModule(CILModule,Stream,EmittingArguments)"/> in order to preprocess all type, field and method references of the module being emitted. The main purpose of this is emitting Portable Class Libraries.
   /// </summary>
   /// <remarks>In most cases, this class will not be needed to be implemented by users of this framework.</remarks>
   public interface EmittingAssemblyMapper
   {
      /// <summary>
      /// Gets the actual type to be used in emitting process for <paramref name="type"/>.
      /// </summary>
      /// <param name="type">The type being referenced to.</param>
      /// <returns>The mapped type for <paramref name="type"/> or <paramref name="type"/> itself. Should never return <c>null</c>.</returns>
      CILTypeBase MapTypeBase( CILTypeBase type );

      /// <summary>
      /// Gets the actual method to be used in emitting process for <paramref name="method"/>.
      /// </summary>
      /// <param name="method">The method being referenced to.</param>
      /// <returns>The mapped method for <paramref name="method"/> or <paramref name="method"/> itself. Should never return <c>null</c>.</returns>
      CILMethodBase MapMethodBase( CILMethodBase method );

      /// <summary>
      /// Gets the actual field to be used in emitting process for <paramref name="field"/>.
      /// </summary>
      /// <param name="field">The field being referenced to.</param>
      /// <returns>The mapped field for <paramref name="field"/> or <paramref name="field"/> itself. Should never return <c>null</c>.</returns>
      CILField MapField( CILField field );

      /// <summary>
      /// Checks whether the assembly with the name <paramref name="name"/> is being redirected by this <see cref="EmittingAssemblyMapper"/>.
      /// </summary>
      /// <param name="name">The name of the assembly being referenced to.</param>
      /// <returns><c>true</c> if this <see cref="EmittingAssemblyMapper"/> redirects at least something related to assembly with the name <paramref name="name"/>; <c>false</c> otherwise.</returns>
      Boolean IsMapped( CILAssemblyName name );

      /// <summary>
      /// Tries to get assembly part of this mapper.
      /// </summary>
      /// <param name="name">The assembly name.</param>
      /// <param name="assembly">This will contain the mapped assembly, or <c>null</c>.</param>
      /// <returns><c>true</c> if mapped assembly found for <paramref name="name"/>; <c>false</c> otherwise.</returns>
      Boolean TryGetMappedAssembly( CILAssemblyName name, out CILAssembly assembly );
   }

   ///// <summary>
   ///// This enum has all currently possible portable class library profiles.
   ///// </summary>
   //public enum PortabilityKind
   //{
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4, Silverlight 4, Windows 8, Windows Phone Silverlight 7, Xbox 360)
   //   ///</summary>
   //   Profile1,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.0.3, Windows 8, Windows Phone 8.1)
   //   ///</summary>
   //   Profile102,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.5, Silverlight 4, Windows 8, Windows Phone Silverlight 7.5)
   //   ///</summary>
   //   Profile104,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.5, Windows 8, Windows Phone 8.1)
   //   ///</summary>
   //   Profile111,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.0.3, Silverlight 4, Windows 8, Windows Phone Silverlight 7, Xbox 360)
   //   ///</summary>
   //   Profile131,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4, Silverlight 5, Windows 8, Windows Phone Silverlight 8)
   //   ///</summary>
   //   Profile136,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4, Silverlight 5)
   //   ///</summary>
   //   Profile14,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.0.3, Silverlight 4, Windows 8, Windows Phone Silverlight 8)
   //   ///</summary>
   //   Profile143,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.0.3, Silverlight 5, Windows 8, Windows Phone Silverlight 8)
   //   ///</summary>
   //   Profile147,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.5.1, Windows 8.1, Windows Phone 8.1)
   //   ///</summary>
   //   Profile151,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.5, Silverlight 4, Windows 8, Windows Phone Silverlight 8)
   //   ///</summary>
   //   Profile154,
   //   ///<summary>
   //   ///.NET Portable Subset (Windows 8.1, Windows Phone 8.1, Windows Phone Silverlight 8.1)
   //   ///</summary>
   //   Profile157,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.5, Silverlight 5, Windows 8, Windows Phone Silverlight 8)
   //   ///</summary>
   //   Profile158,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.0.3, Silverlight 4)
   //   ///</summary>
   //   Profile18,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.0.3, Silverlight 5)
   //   ///</summary>
   //   Profile19,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4, Silverlight 4, Windows 8, Windows Phone Silverlight 7)
   //   ///</summary>
   //   Profile2,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4, Silverlight 5, Windows 8, Windows Phone 8.1)
   //   ///</summary>
   //   Profile225,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.5, Silverlight 4)
   //   ///</summary>
   //   Profile23,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.5, Silverlight 5)
   //   ///</summary>
   //   Profile24,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.0.3, Silverlight 5, Windows 8, Windows Phone 8.1)
   //   ///</summary>
   //   Profile240,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.5, Silverlight 5, Windows 8, Windows Phone 8.1)
   //   ///</summary>
   //   Profile255,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.5, Windows 8, Windows Phone 8.1, Windows Phone Silverlight 8)
   //   ///</summary>
   //   Profile259,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4, Silverlight 4)
   //   ///</summary>
   //   Profile3,
   //   ///<summary>
   //   ///.NET Portable Subset (Windows 8.1, Windows Phone Silverlight 8.1)
   //   ///</summary>
   //   Profile31,
   //   ///<summary>
   //   ///.NET Portable Subset (Windows 8.1, Windows Phone 8.1)
   //   ///</summary>
   //   Profile32,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4, Silverlight 5, Windows 8, Windows Phone 8.1, Windows Phone Silverlight 8)
   //   ///</summary>
   //   Profile328,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.0.3, Silverlight 5, Windows 8, Windows Phone 8.1, Windows Phone Silverlight 8)
   //   ///</summary>
   //   Profile336,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.5, Silverlight 5, Windows 8, Windows Phone 8.1, Windows Phone Silverlight 8)
   //   ///</summary>
   //   Profile344,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4, Silverlight 4, Windows 8, Windows Phone Silverlight 8)
   //   ///</summary>
   //   Profile36,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4, Silverlight 5, Windows 8)
   //   ///</summary>
   //   Profile37,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.5, Silverlight 4, Windows 8, Windows Phone Silverlight 7)
   //   ///</summary>
   //   Profile4,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.0.3, Silverlight 4, Windows 8)
   //   ///</summary>
   //   Profile41,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.0.3, Silverlight 5, Windows 8)
   //   ///</summary>
   //   Profile42,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.5.1, Windows 8.1)
   //   ///</summary>
   //   Profile44,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.5, Silverlight 4, Windows 8)
   //   ///</summary>
   //   Profile46,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.5, Silverlight 5, Windows 8)
   //   ///</summary>
   //   Profile47,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.5, Windows Phone Silverlight 8)
   //   ///</summary>
   //   Profile49,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4, Windows 8)
   //   ///</summary>
   //   Profile5,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.0.3, Windows 8)
   //   ///</summary>
   //   Profile6,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.5, Windows 8)
   //   ///</summary>
   //   Profile7,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.5, Windows 8, Windows Phone Silverlight 8)
   //   ///</summary>
   //   Profile78,
   //   ///<summary>
   //   ///.NET Portable Subset (Windows Phone 8.1, Windows Phone Silverlight 8.1)
   //   ///</summary>
   //   Profile84,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4, Silverlight 4, Windows 8, Windows Phone Silverlight 7.5)
   //   ///</summary>
   //   Profile88,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4, Windows 8, Windows Phone 8.1)
   //   ///</summary>
   //   Profile92,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.0.3, Silverlight 4, Windows 8, Windows Phone Silverlight 7)
   //   ///</summary>
   //   Profile95,
   //   ///<summary>
   //   ///.NET Portable Subset (.NET Framework 4.0.3, Silverlight 4, Windows 8, Windows Phone Silverlight 7.5)
   //   ///</summary>
   //   Profile96,
   //}

   /// <summary>
   /// This class contains information about the debug directory of PE files.
   /// </summary>
   /// <seealso href="http://msdn.microsoft.com/en-us/library/windows/desktop/ms680307%28v=vs.85%29.aspx"/>
   public sealed class EmittingDebugInformation
   {
      private Int32 _characteristics;
      private Int32 _timestamp;
      private Int16 _versionMajor;
      private Int16 _versionMinor;
      private Int32 _type;
      private Byte[] _debugDirData;

      /// <summary>
      /// Creates new instance of <see cref="EmittingDebugInformation"/>.
      /// </summary>
      public EmittingDebugInformation()
      {

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

   ///// <summary>
   ///// This is event arguments for <see cref="EmittingArguments.DebugDataDirectoryEvent"/> event.
   ///// </summary>
   //public sealed class DebugDataDirectoryArgs : EventArgs
   //{
   //   private readonly EmittingArguments _eArgs;

   //   internal DebugDataDirectoryArgs( EmittingArguments args )
   //   {
   //      ArgumentValidator.ValidateNotNull( "Emitting arguments", args );

   //      this._eArgs = args;
   //   }

   //   /// <summary>
   //   /// Gets the <see cref="EmittingArguments"/> associated with this event.
   //   /// </summary>
   //   /// <value>The <see cref="EmittingArguments"/> associated with this event.</value>
   //   public EmittingArguments EmittingArgs
   //   {
   //      get
   //      {
   //         return this._eArgs;
   //      }
   //   }
   //}

   /// <summary>
   /// Provides token information about loaded or emitted module.
   /// Instances of this class may be acquired from <see cref="EmittingArguments.MetadataInfo"/> property.
   /// </summary>
   /// <remarks>Note that all token values in this class are stripped from their table-defining byte and are zero-based.</remarks>
   public sealed class EmittingMetadataInfo
   {
      private readonly IDictionary<CILType, Int32> _type2Token;
      private readonly IList<CILType> _token2Type;
      private readonly IDictionary<CILMethodBase, Int32> _method2Token;
      private readonly IList<CILMethodBase> _token2Method;
      private readonly IDictionary<CILParameter, Int32> _param2Token;
      private readonly IList<CILParameter> _token2Param;
      private readonly IDictionary<CILField, Int32> _field2Token;
      private readonly IList<CILField> _token2Field;
      private readonly IDictionary<CILProperty, Int32> _prop2Token;
      private readonly IList<CILProperty> _token2Prop;
      private readonly IDictionary<CILEvent, Int32> _evt2Token;
      private readonly IList<CILEvent> _token2Evt;
      private readonly IDictionary<CILMethodBase, Int32> _methodRVAs;

      internal EmittingMetadataInfo(
         IList<CILType> token2Type,
         IList<CILMethodBase> token2Method,
         IList<CILParameter> token2Param,
         IList<CILField> token2Field,
         IList<CILProperty> token2Prop,
         IList<CILEvent> token2Evt,
         IList<UInt32> methodRVAs
      )
      {
         this._type2Token = Enumerable.Range( 0, token2Type.Count ).ToDictionary( i => token2Type[i], i => i );
         this._token2Type = token2Type;
         this._method2Token = Enumerable.Range( 0, token2Method.Count ).ToDictionary( i => token2Method[i], i => i );
         this._token2Method = token2Method;
         this._param2Token = Enumerable.Range( 0, token2Param.Count ).ToDictionary( i => token2Param[i], i => i );
         this._token2Param = token2Param;
         this._field2Token = Enumerable.Range( 0, token2Field.Count ).ToDictionary( i => token2Field[i], i => i );
         this._token2Field = token2Field;
         this._prop2Token = Enumerable.Range( 0, token2Prop.Count ).ToDictionary( i => token2Prop[i], i => i );
         this._token2Prop = token2Prop;
         this._evt2Token = Enumerable.Range( 0, token2Evt.Count ).ToDictionary( i => token2Evt[i], i => i );
         this._token2Evt = token2Evt;
         this._methodRVAs = Enumerable.Range( 0, token2Method.Count ).ToDictionary( i => token2Method[i], i => (Int32) methodRVAs[i] );
      }

      /// <summary>
      /// Gets the mapping from type defined in module to its token.
      /// </summary>
      /// <value>the mapping from type defined in module to its token.</value>
      public IDictionary<CILType, Int32> Type2Token
      {
         get
         {
            return this._type2Token;
         }
      }

      /// <summary>
      /// Gets the list of all types defined in the module, in same order as they appear in metadata. 
      /// </summary>
      /// <value>the list of all types defined in the module, in same order as they appear in metadata. </value>
      public IList<CILType> Token2Type
      {
         get
         {
            return this._token2Type;
         }
      }

      /// <summary>
      /// Gets the mapping from method defined in module to its token.
      /// </summary>
      /// <value>the mapping from method defined in module to its token.</value>
      public IDictionary<CILMethodBase, Int32> Method2Token
      {
         get
         {
            return this._method2Token;
         }
      }
      /// <summary>
      /// Gets the list of all methods defined in the module, in same order as they appear in metadata. 
      /// </summary>
      /// <value>the list of all methods defined in the module, in same order as they appear in metadata. </value>
      public IList<CILMethodBase> Token2Method
      {
         get
         {
            return this._token2Method;
         }
      }
      /// <summary>
      /// Gets the mapping from method parameter defined in module to its token.
      /// </summary>
      /// <value>the mapping from method parameter defined in module to its token.</value>
      public IDictionary<CILParameter, Int32> Parameter2Token
      {
         get
         {
            return this._param2Token;
         }
      }
      /// <summary>
      /// Gets the list of all method parameters defined in the module, in same order as they appear in metadata. 
      /// </summary>
      /// <value>the list of all method parameters defined in the module, in same order as they appear in metadata. </value>
      public IList<CILParameter> Token2Parameter
      {
         get
         {
            return this._token2Param;
         }
      }

      /// <summary>
      /// Gets the mapping from field defined in module to its token.
      /// </summary>
      /// <value>the mapping from field defined in module to its token.</value>
      public IDictionary<CILField, Int32> Field2Token
      {
         get
         {
            return this._field2Token;
         }
      }
      /// <summary>
      /// Gets the list of all fields defined in the module, in same order as they appear in metadata. 
      /// </summary>
      /// <value>the list of all fields defined in the module, in same order as they appear in metadata. </value>
      public IList<CILField> Token2Field
      {
         get
         {
            return this._token2Field;
         }
      }
      /// <summary>
      /// Gets the mapping from property defined in module to its token.
      /// </summary>
      /// <value>the mapping from property defined in module to its token.</value>
      public IDictionary<CILProperty, Int32> Property2Token
      {
         get
         {
            return this._prop2Token;
         }
      }
      /// <summary>
      /// Gets the list of all properties defined in the module, in same order as they appear in metadata. 
      /// </summary>
      /// <value>the list of all properties defined in the module, in same order as they appear in metadata. </value>
      public IList<CILProperty> Token2Property
      {
         get
         {
            return this._token2Prop;
         }
      }
      /// <summary>
      /// Gets the mapping from event defined in module to its token.
      /// </summary>
      /// <value>the mapping from event defined in module to its token.</value>
      public IDictionary<CILEvent, Int32> Event2Token
      {
         get
         {
            return this._evt2Token;
         }
      }
      /// <summary>
      /// Gets the list of all events defined in the module, in same order as they appear in metadata. 
      /// </summary>
      /// <value>the list of all events defined in the module, in same order as they appear in metadata. </value>
      public IList<CILEvent> Token2Event
      {
         get
         {
            return this._token2Evt;
         }
      }

      /// <summary>
      /// Gets the mapping from method defined in the module to its code RVA.
      /// </summary>
      public IDictionary<CILMethodBase, Int32> MethodRVAs
      {
         get
         {
            return this._methodRVAs;
         }
      }
   }

   /// <summary>
   /// This class encapsulates all additional information that is required and produced during emitting and loading <see cref="CILModule"/>s.
   /// The instances of this class are created by static methods contained within this class.
   /// </summary>
   /// <seealso cref="E_CIL.EmitModule(CILModule, Stream, EmittingArguments)"/>
   public sealed class EmittingArguments
   {

      private StrongNameKeyPair _strongName;
      private AssemblyHashAlgorithm? _signingAlgorithm;
      private Boolean _delaySign;
      private EmittingAssemblyMapper _assemblyMapper;
      private Guid _moduleID;
      private Lazy<CILMethod> _clrEntryPoint;
      private ImageFileMachine _machine;
      private ModuleKind _moduleKind;
      private ModuleFlags _moduleFlags;
      private String _corLibName;
      private UInt16 _corLibMajor;
      private UInt16 _corLibMinor;
      private UInt16 _corLibBuild;
      private UInt16 _corLibRevision;
      private String _metaDataVersion;
      private Byte _tableHeapMajor;
      private Byte _tableHeapMinor;
      private UInt64 _imageBase;
      private UInt32 _fileAlignment;
      private UInt32 _sectionAlignment;
      private UInt64 _stackReserve;
      private UInt64 _stackCommit;
      private UInt64 _heapReserve;
      private UInt64 _heapCommit;
      private String _importHintName;
      private String _importDirectoryName;
      private Int16 _entryPointInstruction;
      private Byte _linkerMajor;
      private Byte _linkerMinor;
      private UInt16 _osMajor;
      private UInt16 _osMinor;
      private UInt16 _userMajor;
      private UInt16 _userMinor;
      private UInt16 _subSysMajor;
      private UInt16 _subSysMinor;
      private UInt16 _cliMajor;
      private UInt16 _cliMinor;
      private Boolean _highEntropyVA;
      private Func<CILModule, CILAssemblyName, CILAssembly> _assemblyRefLoader;
      private Func<CILModule, CILAssembly> _ownerAssemblyLoader;
      private Func<CILModule, String, Stream> _fileStreamOpener;
      private String _fwName;
      private String _fwVersion;
      private String _fwProfile;
      private readonly IList<CILAssemblyName> _assemblyRefs;
      private Boolean _lazyLoad;
      private EmittingDebugInformation _debugInfo;
      private Func<CILMethodBase, Int32, CILElementTokenizableInILCode> _tokenResolver;
      private Func<CILElementTokenizableInILCode, Int32?> _tokenEncoder;
      private Func<Int32, IEnumerable<CILMethodBase>> _tokenSigResolver;
      private Func<CILMethodBase, Int32?> _tokenSigEncoder;
      private Lazy<EmittingMetadataInfo> _mdInfo;
      private Boolean _useFullPublicKeyInReference;
      private readonly IDictionary<String, Tuple<String, Byte[]>> _otherModules;

      private EmittingArguments()
      {
         this._assemblyRefs = new List<CILAssemblyName>();
         this._lazyLoad = true;
         this._otherModules = new Dictionary<String, Tuple<String, Byte[]>>();
         this._corLibName = Consts.MSCORLIB_NAME;
         this._useFullPublicKeyInReference = true;
      }

      /// <summary>
      /// During emitting, if the module is main module and should be strong-name signed, this <see cref="StrongNameKeyPair"/> will be used.
      /// Set <c>null</c> if the module should not be strong-name signed.
      /// This property is not used during loading.
      /// </summary>
      /// <value>The strong name of the module being emitted.</value>
      public StrongNameKeyPair StrongName { get { return this._strongName; } set { this._strongName = value; } }

      /// <summary>
      /// During emitting, if the module is main module and should be strong-name signed, this property may be used to override the algorithm specified by key BLOB of <see cref="StrongName"/>.
      /// If this property does not have a value, the algorithm specified by key BLOB of <see cref="StrongName"/> will be used.
      /// If the key BLOB of <see cref="StrongName"/> does not specify an algorithm, the assembly will be signed using <see cref="AssemblyHashAlgorithm.SHA1"/>.
      /// This property is not used during loading.
      /// </summary>
      /// <value>The algorithm to compute a hash over emitted assembly data.</value>
      /// <remarks>
      /// If <see name="AssemblyHashAlgorithm.MD5"/> or <see cref="AssemblyHashAlgorithm.None"/> is specified, the <see cref="AssemblyHashAlgorithm.SHA1"/> will be used instead.
      /// </remarks>
      public AssemblyHashAlgorithm? SigningAlgorithm { get { return this._signingAlgorithm; } set { this._signingAlgorithm = value; } }

      /// <summary>
      /// During emitting, if the module is main module and should be strong-name signed, setting this to <c>true</c> will only leave room for the hash, without actually computing it.
      /// This property is not used during loading.
      /// </summary>
      /// <value>Whether to delay signing procedure.</value>
      public Boolean DelaySign { get { return this._delaySign; } set { this._delaySign = value; } }

      /// <summary>
      /// During emitting, this holds the mapper to preprocess all type, field and method references.
      /// Set to <c>null</c> if no such mapper is required.
      /// This property is not used during loading.
      /// </summary>
      /// <value>The <see cref="EmittingAssemblyMapper"/> for the module being emitted.</value>
      public EmittingAssemblyMapper AssemblyMapper { get { return this._assemblyMapper; } set { this._assemblyMapper = value; } }

      /// <summary>
      /// The module <see cref="Guid"/>.
      /// </summary>
      /// <value>The <see cref="Guid"/> of the module being emitted or loaded.</value>
      public Guid ModuleID { get { return this._moduleID; } set { this._moduleID = value; } }

      /// <summary>
      /// Gets or set the entry point <see cref="CILMethod"/>.
      /// </summary>
      /// <value>The entry point <see cref="CILMethod"/> of the module being emitted or loaded.</value>
      public CILMethod CLREntryPoint { get { return this._clrEntryPoint.Value; } set { this.SetCLREntryPoint( () => value ); } }

      /// <summary>
      /// Gets or sets the <see cref="ImageFileMachine"/> of the module (PE file header, 'Machine' field).
      /// </summary>
      /// <value>The <see cref="ImageFileMachine"/> of the module (PE file header, 'Machine' field) of the module being emitted or loaded.</value>
      public ImageFileMachine Machine { get { return this._machine; } set { this._machine = value; } }

      /// <summary>
      /// Gets or sets the <see cref="ModuleKind"/> of the module.
      /// </summary>
      /// <value>The <see cref="ModuleKind"/> of the module being emitted or loaded.</value>
      public ModuleKind ModuleKind { get { return this._moduleKind; } set { this._moduleKind = value; } }

      /// <summary>
      /// Gets or sets the <see cref="ModuleFlags"/> of the module (CLI header, 'Flags' field).
      /// </summary>
      /// <value>The <see cref="ModuleFlags"/> of the module (CLI header, 'Flags' field) being emitted or loaded.</value>
      public ModuleFlags ModuleFlags { get { return this._moduleFlags; } set { this._moduleFlags = value; } }

      /// <summary>
      /// Gets or sets the name of the 'mscorlib' assembly reference.
      /// </summary>
      /// <value>The name of the 'mscorlib' assembly reference (the target runtime version information will use this) to be used during emitting or loading.</value>
      public String CorLibName { get { return this._corLibName; } set { this._corLibName = value; } }

      /// <summary>
      /// Gets or sets the major version to be used in emitting or loading the reference to the 'mscorlib' assembly.
      /// </summary>
      /// <value>The major version to be used in emitting or loading the reference to the 'mscorlib' assembly to be used during emitting.</value>
      /// <remarks>During loading, this will be used only if metadata does not contain reference to <see cref="CorLibName"/>.</remarks>
      [CLSCompliant( false )]
      public UInt16 CorLibMajor { get { return this._corLibMajor; } set { this._corLibMajor = value; } }

      /// <summary>
      /// Gets or sets the minor version to be used in emitting or loading the reference to the 'mscorlib' assembly.
      /// </summary>
      /// <value>The minor version to be used in emitting or loading the reference to the 'mscorlib' assembly to be used during emitting.</value>
      /// <remarks>During loading, this will be used only if metadata does not contain reference to <see cref="CorLibName"/>.</remarks>
      [CLSCompliant( false )]
      public UInt16 CorLibMinor { get { return this._corLibMinor; } set { this._corLibMinor = value; } }

      /// <summary>
      /// Gets or sets the build version to be used in emitting or loading the reference to the 'mscorlib' assembly.
      /// </summary>
      /// <value>The build version to be used in emitting or loading the reference to the 'mscorlib' assembly to be used during emitting.</value>
      /// <remarks>During loading, this will be used only if metadata does not contain reference to <see cref="CorLibName"/>.</remarks>
      [CLSCompliant( false )]
      public UInt16 CorLibBuild { get { return this._corLibBuild; } set { this._corLibBuild = value; } }

      /// <summary>
      /// Gets or sets the revision version to be used in emitting or loading the reference to the 'mscorlib' assembly.
      /// </summary>
      /// <value>The revision version to be used in emitting or loading the reference to the 'mscorlib' assembly to be used during emitting.</value>
      /// <remarks>During loading, this will be used only if metadata does not contain reference to <see cref="CorLibName"/>.</remarks>
      [CLSCompliant( false )]
      public UInt16 CorLibRevision { get { return this._corLibRevision; } set { this._corLibRevision = value; } }

      /// <summary>
      /// Gets or sets the version string of the metadata (metadata root, 'Version' field).
      /// </summary>
      /// <value>The version string of the metadata (metadata root, 'Version' field) of the module being emitted or loaded..</value>
      public String MetaDataVersion { get { return this._metaDataVersion; } set { this._metaDataVersion = value; } }

      /// <summary>
      /// Gets or sets the major version of the #~ stream.
      /// </summary>
      /// <value>The major version of the #~ stream of the module being emitted or loaded.</value>
      public Byte TableHeapMajor { get { return this._tableHeapMajor; } set { this._tableHeapMajor = value; } }

      /// <summary>
      /// Gets or sets the minor version of the #~ stream.
      /// </summary>
      /// <value>The minor version of the #~ stream of the module being emitted or loaded.</value>
      public Byte TableHeapMinor { get { return this._tableHeapMinor; } set { this._tableHeapMinor = value; } }

      /// <summary>
      /// Gets or sets the base of the emitted image file (PE header, Windows NT-specific, 'Image Base' field).
      /// Should be a multiple of <c>0x10000</c>.
      /// </summary>
      /// <value>The base of the emitted image file (PE header, Windows NT-specific, 'Image Base' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt64 ImageBase { get { return this._imageBase; } set { this._imageBase = value; } }

      /// <summary>
      /// Gets or sets the file alignment of the emitted image file (PE header, Windows NT-specific, 'File Alignment' field).
      /// Should be at least <c>0x200</c>.
      /// </summary>
      /// <value>The file alignment of the emitted image file (PE header, Windows NT-specific, 'File Alignment' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt32 FileAlignment { get { return this._fileAlignment; } set { this._fileAlignment = value; } }

      /// <summary>
      /// Gets or sets the section alignment of the emitted image file (PE header, Windows NT-specific, 'Section Alignment' field).
      /// Should be greater than <see cref="FileAlignment"/>.
      /// </summary>
      /// <value>The section alignment of the emitted image file (PE header, Windows NT-specific, 'Section Alignment' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt32 SectionAlignment { get { return this._sectionAlignment; } set { this._sectionAlignment = value; } }

      /// <summary>
      /// Gets or sets the stack reserve size (PE header, Windows NT-specific, 'Stack Reserve Size' field).
      /// Should be <c>0x100000</c>.
      /// </summary>
      /// <value>The stack reserve size (PE header, Windows NT-specific, 'Stack Reserve Size' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt64 StackReserve { get { return this._stackReserve; } set { this._stackReserve = value; } }

      /// <summary>
      /// Gets or sets the stack commit size (PE header, Windows NT-specific, 'Stack Commit Size' field).
      /// Should be <c>0x1000</c>.
      /// </summary>
      /// <value>The stack commit size (PE header, Windows NT-specific, 'Stack Commit Size' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt64 StackCommit { get { return this._stackCommit; } set { this._stackCommit = value; } }

      /// <summary>
      /// Gets or sets the heap reserve size (PE header, Windows NT-specific, 'Heap Reserve Size' field).
      /// Should be <c>0x100000</c>.
      /// </summary>
      /// <value>The heap reserve size (PE header, Windows NT-specific, 'Heap Reserve Size' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt64 HeapReserve { get { return this._heapReserve; } set { this._heapReserve = value; } }

      /// <summary>
      /// Gets or sets the heap commit size (PE header, Windows NT-specific, 'Heap Commit Size' field).
      /// Should be <c>0x1000</c>.
      /// </summary>
      /// <value>The heap commit size (PE header, Windows NT-specific, 'Heap Commit Size' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt64 HeapCommit { get { return this._heapCommit; } set { this._heapCommit = value; } }

      /// <summary>
      /// Gets or sets the name of the entries to import from runtime engine (typically <c>"mscoree.dll"</c>) (Hint/Name table, 'Name' field).
      /// Should be <c>"_CorExeMain"</c> for a .exe file and <c>"_CorDllMain"</c> for a .dll file.
      /// </summary>
      /// <value>The name of the entries to import from runtime engine (typically <c>"mscoree.dll"</c>) (Hint/Name table, 'Name' field) of the module being emitted or loaded.</value>
      public String ImportHintName { get { return this._importHintName; } set { this._importHintName = value; } }

      /// <summary>
      /// Gets or sets the name of the runtime engine to import <see cref="ImportHintName"/> from (Import tables, 'Name' field).
      /// Should be <c>"mscoree.dll"</c>.
      /// </summary>
      /// <value>The name of the runtime engine to import <see cref="ImportHintName"/> from (Import tables, 'Name' field) of the module being emitted or loaded.</value>
      public String ImportDirectoryName { get { return this._importDirectoryName; } set { this._importDirectoryName = value; } }

      /// <summary>
      /// Gets or sets the instruction at PE entrypoint to load the code section.
      /// It should be <c>0x25FF</c>.
      /// </summary>
      /// <value>The instruction at PE entrypoint to load the code section of the module being emitted.</value>
      public Int16 EntryPointInstruction { get { return this._entryPointInstruction; } set { this._entryPointInstruction = value; } }

      /// <summary>
      /// Gets or sets the major version of the linker (PE header standard, 'LMajor' field).
      /// </summary>
      /// <value>The major version of the linker (PE header standard, 'LMajor' field) of the module being emitted or loaded.</value>
      public Byte LinkerMajor { get { return this._linkerMajor; } set { this._linkerMajor = value; } }

      /// <summary>
      /// Gets or sets the minor version of the linker (PE header standard, 'LMinor' field).
      /// </summary>
      /// <value>The minor version of the linker (PE header standard, 'LMinor' field) of the module being emitted.</value>
      public Byte LinkerMinor { get { return this._linkerMinor; } set { this._linkerMinor = value; } }

      /// <summary>
      /// Gets or sets the major version of the OS (PE header, Windows NT-specific, 'OS Major' field).
      /// </summary>
      /// <value>The major version of the OS (PE header, Windows NT-specific, 'OS Major' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt16 OSMajor { get { return this._osMajor; } set { this._osMajor = value; } }

      /// <summary>
      /// Gets or sets the minor version of the OS (PE header, Windows NT-specific, 'OS Minor' field).
      /// </summary>
      /// <value>The minor version of the OS (PE header, Windows NT-specific, 'OS Minor' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt16 OSMinor { get { return this._osMinor; } set { this._osMinor = value; } }

      /// <summary>
      /// Gets or sets the user-specific major version (PE header, Windows NT-specific, 'User Major' field).
      /// </summary>
      /// <value>The user-specific major version (PE header, Windows NT-specific, 'User Major' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt16 UserMajor { get { return this._userMajor; } set { this._userMajor = value; } }

      /// <summary>
      /// Gets or sets the user-specific minor version (PE header, Windows NT-specific, 'User Minor' field).
      /// </summary>
      /// <value>The user-specific minor version (PE header, Windows NT-specific, 'User Minor' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt16 UserMinor { get { return this._userMinor; } set { this._userMinor = value; } }

      /// <summary>
      /// Gets or sets the major version of the subsystem (PE header, Windows NT-specific, 'SubSys Major' field).
      /// </summary>
      /// <value>The major version of the subsystem (PE header, Windows NT-specific, 'SubSys Major' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt16 SubSysMajor { get { return this._subSysMajor; } set { this._subSysMajor = value; } }

      /// <summary>
      /// Gets or sets the minor version of the subsystem (PE header, Windows NT-specific, 'SubSys Minor' field).
      /// </summary>
      /// <value>The minor version of the subsystem (PE header, Windows NT-specific, 'SubSys Minor' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt16 SubSysMinor { get { return this._subSysMinor; } set { this._subSysMinor = value; } }

      /// <summary>
      /// Gets or sets the major version of the targeted CLI (CLI header, 'MajorRuntimeVersion' field).
      /// Should be at least <c>2</c>.
      /// </summary>
      /// <value>The major version of the targeted CLI (CLI header, 'MajorRuntimeVersion' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt16 CLIMajor { get { return this._cliMajor; } set { this._cliMajor = value; } }

      /// <summary>
      /// Gets or sets the minor version of the targeted CLI (CLI header, 'MinorRuntimeVersion' field).
      /// </summary>
      /// <value>The minor version of the targeted CLI (CLI header, 'MinorRuntimeVersion' field) of the module being emitted or loaded.</value>
      [CLSCompliant( false )]
      public UInt16 CLIMinor { get { return this._cliMinor; } set { this._cliMinor = value; } }

      /// <summary>
      /// Gets or sets the flag signalling to use high entropy address space layout randomization ( see <see href="http://msdn.microsoft.com/en-us/library/hh156527.aspx"/> ).
      /// </summary>
      /// <value>Whether to use high entropy address space layout randomization.</value>
      public Boolean HighEntropyVA { get { return this._highEntropyVA; } set { this._highEntropyVA = value; } }

      /// <summary>
      /// This event will get triggered for each assembly reference processed during emitting.
      /// This property is not used during loading.
      /// </summary>
      public event Action<CILAssemblyName> AssemblyRefProcessor;

      /// <summary>
      /// After emitting or loading is completed, this will hold all assembly references of the module being emitted or loaded.
      /// </summary>
      /// <value>All assembly references of the module being emitted or loaded.</value>
      public IList<CILAssemblyName> AssemblyRefs { get { return this._assemblyRefs; } }

      /// <summary>
      /// Gets or sets custom callback to load assembly referenced by module being loaded.
      /// This property is not used during emitting.
      /// </summary>
      /// <value>The custom callback to load assembly referenced by module being loaded.</value>
      public Func<CILModule, CILAssemblyName, CILAssembly> AssemblyRefLoader { get { return this._assemblyRefLoader; } set { this._assemblyRefLoader = value; } }

      /// <summary>
      /// Gets or sets custom callback to load assembly which owns given module, when the loaded module is not the main module of the assembly.
      /// This property is not used during emitting.
      /// </summary>
      /// <value>The custom callback to load assembly which owns given module, when the loaded module is not the main module of the assembly.</value>
      public Func<CILModule, CILAssembly> ModuleAssemblyLoader { get { return this._ownerAssemblyLoader; } set { this._ownerAssemblyLoader = value; } }

      /// <summary>
      /// Gets or sets the custom callback to load stream for entries in File table of the loaded module.
      /// </summary>
      /// <value>The custom callback to load stream for given entry in File table of the module being loaded or emitted.</value>
      /// <remarks>During emitting, this callback is used only if <see cref="FileManifestResource.Hash"/> value is <c>null</c>.</remarks>
      public Func<CILModule, String, Stream> FileStreamOpener { get { return this._fileStreamOpener; } set { this._fileStreamOpener = value; } }

      /// <summary>
      /// Gets or sets the framework name detected during loading.
      /// This property is not used during emitting.
      /// </summary>
      /// <value>The framework name detected during loading.</value>
      public String FrameworkName { get { return this._fwName; } set { this._fwName = value; } }

      /// <summary>
      /// Gets or sets the framework version detected during loading.
      /// This property is not used during emitting.
      /// </summary>
      /// <value>The fraemwork version detected during loading.</value>
      public String FrameworkVersion { get { return this._fwVersion; } set { this._fwVersion = value; } }

      /// <summary>
      /// Gets or sets the framework profile detected during loading.
      /// This property is not used during emitting.
      /// </summary>
      /// <value>The framework profile detected during loading.</value>
      public String FrameworkProfile { get { return this._fwProfile; } set { this._fwProfile = value; } }

      /// <summary>
      /// Gets or sets value indicating whether to perform a lazy load or force to initialize all values right after loading.
      /// By default, this property is <c>true</c>.
      /// This property is not used during emitting.
      /// </summary>
      /// <value><c>true</c> if all lazy values should be left as is; <c>false</c> if all lazy values should be forced to initialize.</value>
      public Boolean LazyLoad { get { return this._lazyLoad; } set { this._lazyLoad = value; } }

      /// <summary>
      /// During emitting, if this property is not <c>null</c>, then the debug directory with the information specified by <see cref="EmittingDebugInformation"/> is written.
      /// During loading, if the PE file contains debug directory, this property is set to reflect the data of the debug directory.
      /// </summary>
      public EmittingDebugInformation DebugInformation { get { return this._debugInfo; } set { this._debugInfo = value; } }

      /// <summary>
      /// Resolves a token that is referring to <see cref="CILElementTokenizableInILCode"/>.
      /// The usage of this method is only valid after loading a module; it is not supported for <see cref="EmittingArguments"/> used for emitting.
      /// </summary>
      /// <param name="resolvingContext">The method which has the body where this resolving happens. May be <c>null</c> if token is not TokenSpec or MethodSpec or does not result in a generic type or method, which is not generic type or method definition, respectively.</param>
      /// <param name="token">The token to resolve.</param>
      /// <returns>A <see cref="CILElementTokenizableInILCode"/> or <c>null</c> if resolving fails.</returns>
      /// <exception cref="InvalidOperationException">If this method is called after emitting a module.</exception>
      public CILElementTokenizableInILCode ResolveToken( CILMethodBase resolvingContext, Int32 token )
      {
         return this._tokenResolver( resolvingContext, token );
      }

      /// <summary>
      /// Resolves all methods that have given token as their locals signature token.
      /// The usage of this method is only valid after loading a module; it is not supported for <see cref="EmittingArguments"/> used for emitting.
      /// </summary>
      /// <param name="token">The signature token to resolve.</param>
      /// <returns>An enumerable for all methods using given token as their locals signature token.</returns>
      /// <exception cref="InvalidOperationException">If this method is called after emitting a module.</exception>
      public IEnumerable<CILMethodBase> ResolveSignatureToken( Int32 token )
      {
         return this._tokenSigResolver( token );
      }

      /// <summary>
      /// Gets a token for given <see cref="CILElementTokenizableInILCode"/>, possibly within a body of some <see cref="CILMethodBase"/>.
      /// The usage of this method is only valid after emitting a module; it is not supported for <see cref="EmittingArguments"/> used for loading.
      /// </summary>
      /// <param name="element">The element to get token for.</param>
      /// <returns>A token or <c>null</c> if encoding fails.</returns>
      /// <remarks>TODO varargs?</remarks>
      /// <exception cref="InvalidOperationException">If this method is called after loading a module.</exception>
      public Int32? GetTokenFor( CILElementTokenizableInILCode element )
      {
         return this._tokenEncoder( element );
      }

      /// <summary>
      /// Gets a token for local variables for a given method.
      /// The usage of this method is only valid after emitting a module; it is not supported for <see cref="EmittingArguments"/> used for loading.
      /// </summary>
      /// <param name="method">The method for which to retrieve signature token for.</param>
      /// <returns>A token or <c>null</c> if encoding fails.</returns>
      /// <exception cref="InvalidOperationException">If this method is called after loading a module.</exception>
      public Int32? GetSignatureTokenFor( CILMethodBase method )
      {
         return this._tokenSigEncoder( method );
      }

      /// <summary>
      /// This property lazily evaluates the <see cref="EmittingMetadataInfo"/> object that provides access to mappings between various elements defined in the module and their metadata tokens.
      /// This property is usable in both cases of emitting and loading.
      /// </summary>
      public EmittingMetadataInfo MetadataInfo { get { return this._mdInfo.Value; } }

      /// <summary>
      /// This property is used during emitting to emit information about other modules composing this assembly.
      /// If emitting happens to a module which is not main module of the assembly, this property will not be used.
      /// The key of the dictionary is module name, and values are 2-tuples having first item as module filename (without any directory information) and second item as hash value of that file.
      /// </summary>
      /// <value>Information about other modules composing this assembly.</value>
      public IDictionary<String, Tuple<String, Byte[]>> OtherModules { get { return this._otherModules; } }

      /// <summary>
      /// Gets or sets the value indicating whether full public key should be used when referencing other assemblies either from AssemblyRef table or within type strings.
      /// This value is used only during emitting, and it is <c>true</c> by default.
      /// </summary>
      /// <value>Value indicating whether full public key should be used when referencing other assemblies either from AssemblyRef table or within type strings.</value>
      public Boolean UseFullPublicKeyInAssemblyReferences { get { return this._useFullPublicKeyInReference; } set { this._useFullPublicKeyInReference = value; } }

      internal void SetMDInfo( Func<EmittingMetadataInfo> mdInfo )
      {
         Interlocked.CompareExchange( ref this._mdInfo, new Lazy<EmittingMetadataInfo>( mdInfo, LazyThreadSafetyMode.ExecutionAndPublication ), null );
      }

      internal void SetTokenFunctions(
         Func<CILMethodBase, Int32, CILElementTokenizableInILCode> tokenResolver,
         Func<CILElementTokenizableInILCode, Int32?> tokenEncoder,
         Func<Int32, IEnumerable<CILMethodBase>> tokenSigResolver,
         Func<CILMethodBase, Int32?> tokenSigEncoder
         )
      {
         System.Threading.Interlocked.CompareExchange( ref this._tokenResolver, tokenResolver, null );
         System.Threading.Interlocked.CompareExchange( ref this._tokenEncoder, tokenEncoder, null );
         System.Threading.Interlocked.CompareExchange( ref this._tokenSigResolver, tokenSigResolver, null );
         System.Threading.Interlocked.CompareExchange( ref this._tokenSigEncoder, tokenSigEncoder, null );
      }

      internal void SetCLREntryPoint( Func<CILMethod> loader )
      {
         this._clrEntryPoint = new Lazy<CILMethod>( loader, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );
      }

      internal Action<CILAssemblyName> AssemblyRefEventValue
      {
         get
         {
            return this.AssemblyRefProcessor;
         }
      }

      /// <summary>
      /// Creates a new instance of <see cref="EmittingArguments"/> which has suitable data for emitting modules which are built by using wrappers around current target framework native types but targeted for different framework.
      /// </summary>
      /// <param name="ctx">The current <see cref="CILReflectionContext"/>.</param>
      /// <param name="targetArchitecture">The <see cref="ImageFileMachine">target architecture</see> of the emitted module.</param>
      /// <param name="targetRuntime">The <see cref="TargetRuntime"/> of the emitted module.</param>
      /// <param name="moduleKind">The <see cref="ModuleKind"/> of the emitted module.</param>
      /// <param name="entryPoint">The <see cref="CILMethod"/> serving as entry point of this module.</param>
      /// <param name="thisRuntimeRoot">The directory containing assemblies of currently loaded runtime.</param>
      /// <param name="monikerAssemblyDir">The directory where target framework assemblies reside.</param>
      /// <param name="streamOpener">The callback to open a file given a specific path.</param>
      /// <param name="strongName">The strong name keypair for signing the module, or <c>null</c> if module should not have strong name.</param>
      /// <param name="monikerInfo">The information about portable profile, <see cref="FrameworkMonikerInfo"/>.</param>
      /// <param name="fwReferencesRetargetable">Whether references to framework assemblies should have <see cref="AssemblyFlags.Retargetable"/> flag.</param>
      /// <param name="moduleFlags">The module flags. The <see cref="API.ModuleFlags.StrongNameSigned"/> will be added automatically if <paramref name="strongName"/> is not <c>null</c>.</param>
      /// <returns>A new instance of <see cref="EmittingArguments"/> which has suitable data for emitting PCLs.</returns>
      public static EmittingArguments CreateForEmittingWithMoniker(
         CILReflectionContext ctx,
         ImageFileMachine targetArchitecture,
         TargetRuntime targetRuntime,
         ModuleKind moduleKind,
         CILMethod entryPoint,
         String thisRuntimeRoot,
         String monikerAssemblyDir,
         Func<String, Stream> streamOpener,
         StrongNameKeyPair strongName,
         FrameworkMonikerInfo monikerInfo,
         Boolean fwReferencesRetargetable,
         ModuleFlags moduleFlags = ModuleFlags.ILOnly
         )
      {
         var result = CreateForEmittingAnyModule(
            strongName,
            targetArchitecture,
            targetRuntime,
            moduleKind,
            entryPoint,
            moduleFlags
            );
         result.AssemblyMapper = ctx.CreateMapperForFrameworkMoniker(
            thisRuntimeRoot,
            monikerAssemblyDir,
            streamOpener,
            monikerInfo
            );
         var mscorLibVersion = monikerInfo.Assemblies[monikerInfo.MsCorLibAssembly].Item1;
         result.CorLibName = monikerInfo.MsCorLibAssembly;
         result.CorLibMajor = (UInt16) mscorLibVersion.Major;
         result.CorLibMinor = (UInt16) mscorLibVersion.Minor;
         result.CorLibBuild = (UInt16) mscorLibVersion.Build;
         result.CorLibRevision = (UInt16) mscorLibVersion.Revision;

         if ( fwReferencesRetargetable )
         {
            result.AssemblyRefProcessor += name =>
            {
               if ( monikerInfo.Assemblies.ContainsKey( name.Name ) )
               {
                  name.Flags |= AssemblyFlags.Retargetable;
               }
            };
         }
         return result;
      }

      /// <summary>
      /// Creates a new instance of <see cref="EmittingArguments"/> which has suitable data for emitting Dynamic Link Libraries (DLL).
      /// </summary>
      /// <param name="strongName">The strong name keypair for signing the module, or <c>null</c> if module should not have strong name.</param>
      /// <param name="targetArchitecture">The <see cref="ImageFileMachine">target architecture</see> of the emitted module.</param>
      /// <param name="targetRuntime">The <see cref="TargetRuntime"/> of the emitted module.</param>
      /// <param name="moduleFlags">The module flags. The <see cref="API.ModuleFlags.StrongNameSigned"/> will be added automatically if <paramref name="strongName"/> is not <c>null</c>.</param>
      /// <returns>A new instance of <see cref="EmittingArguments"/> which has suitable data for emitting DLLs.</returns>
      public static EmittingArguments CreateForEmittingDLL(
         StrongNameKeyPair strongName,
         ImageFileMachine targetArchitecture,
         TargetRuntime targetRuntime,
         ModuleFlags moduleFlags = ModuleFlags.ILOnly
         )
      {
         return CreateForEmittingAnyModule(
            strongName,
            targetArchitecture,
            targetRuntime,
            ModuleKind.Dll,
            null,
            moduleFlags );
      }

      /// <summary>
      /// Creates a new instance of <see cref="EmittingArguments"/> which has suitable data for emitting executable files (EXE).
      /// </summary>
      /// <param name="strongName">The strong name keypair for signing the module, or <c>null</c> if module should not have strong name.</param>
      /// <param name="targetArchitecture">The <see cref="ImageFileMachine">target architecture</see> of the emitted module.</param>
      /// <param name="targetRuntime">The <see cref="TargetRuntime"/> of the emitted module.</param>
      /// <param name="consoleApplication">Whether the resulting .exe file is a console application.</param>
      /// <param name="entryPoint">The <see cref="CILMethod"/> serving as entry point of this module.</param>
      /// <param name="moduleFlags">The module flags. The <see cref="API.ModuleFlags.StrongNameSigned"/> will be added automatically if <paramref name="strongName"/> is not <c>null</c>.</param>
      /// <returns>A new instance of <see cref="EmittingArguments"/> which has suitable data for emitting EXE files.</returns>
      public static EmittingArguments CreateForEmittingEXE(
         StrongNameKeyPair strongName,
         ImageFileMachine targetArchitecture,
         TargetRuntime targetRuntime,
         Boolean consoleApplication,
         CILMethod entryPoint,
         ModuleFlags moduleFlags = ModuleFlags.ILOnly
         )
      {
         ArgumentValidator.ValidateNotNull( "Entry point method", entryPoint );

         return CreateForEmittingAnyModule(
            strongName,
            targetArchitecture,
            targetRuntime,
            consoleApplication ? ModuleKind.Console : ModuleKind.Windows,
            entryPoint,
            moduleFlags );
      }

      /// <summary>
      /// Creates a new instance of <see cref="EmittingArguments"/> with most used default values preset.
      /// </summary>
      /// <param name="strongName">The strong name keypair for signing the module, or <c>null</c> if module should not have strong name.</param>
      /// <param name="targetArchitecture">The <see cref="ImageFileMachine">target architecture</see> of the emitted module.</param>
      /// <param name="targetRuntime">The <see cref="TargetRuntime"/> of the emitted module.</param>
      /// <param name="moduleKind">The <see cref="ModuleKind"/> of the emitted module.</param>
      /// <param name="entryPoint">The <see cref="CILMethod"/> serving as entry point of this module.</param>
      /// <param name="moduleFlags">The module flags. The <see cref="API.ModuleFlags.StrongNameSigned"/> will be added automatically if <paramref name="strongName"/> is not <c>null</c>.</param>
      /// <returns>A new instance of <see cref="EmittingArguments"/> with most used default values preset.</returns>
      public static EmittingArguments CreateForEmittingAnyModule(
         StrongNameKeyPair strongName,
         ImageFileMachine targetArchitecture,
         TargetRuntime targetRuntime,
         ModuleKind moduleKind,
         CILMethod entryPoint,
         ModuleFlags moduleFlags = ModuleFlags.ILOnly
         )
      {
         String mdVersion;
         UInt16 corLibMajor, corLibMinor, corLibBuild, corLibRevision, cliMajor, cliMinor;
         Byte tableHeapMajor, tableHeapMinor;
         targetRuntime.GetTargetRelatedAttributes( out mdVersion, out corLibMajor, out corLibMinor, out corLibBuild, out corLibRevision, out cliMajor, out cliMinor, out tableHeapMajor, out tableHeapMinor );

         return new EmittingArguments
         {
            _strongName = strongName,
            _machine = targetArchitecture,
            _moduleFlags = moduleFlags,
            _moduleID = Guid.NewGuid(),
            CLREntryPoint = entryPoint,
            _moduleKind = moduleKind,
            _corLibMajor = corLibMajor,
            _corLibMinor = corLibMinor,
            _corLibBuild = corLibBuild,
            _corLibRevision = corLibRevision,
            _metaDataVersion = mdVersion,
            _tableHeapMajor = tableHeapMajor,
            _tableHeapMinor = tableHeapMinor,
            _imageBase = ModuleWriter.DEFAULT_IMAGE_BASE,
            _fileAlignment = ModuleWriter.MIN_FILE_ALIGNMENT,
            _sectionAlignment = ModuleWriter.DEFAULT_SECTION_ALIGNMENT,
            _stackReserve = ModuleWriter.DEFAULT_STACK_RESERVE,
            _stackCommit = ModuleWriter.DEFAULT_STACK_COMMIT,
            _heapReserve = ModuleWriter.DEFAULT_HEAP_RESERVE,
            _heapCommit = ModuleWriter.DEFAULT_HEAP_COMMIT,
            _importHintName = moduleKind.IsDLL() ? ModuleWriter.DEFAULT_DLL_HINT_NAME : ModuleWriter.DEFAULT_EXE_HINT_NAME,
            _importDirectoryName = ModuleWriter.DEFAULT_IMPORT_DIRECTORY_NAME,
            _entryPointInstruction = ModuleWriter.DEFAULT_ENTRY_POINT_INSTRUCTION,
            _linkerMajor = ModuleWriter.DEFAULT_LINKER_MAJOR,
            _linkerMinor = ModuleWriter.DEFAULT_LINKER_MINOR,
            _osMajor = ModuleWriter.DEFAULT_OS_MAJOR,
            _osMinor = ModuleWriter.DEFAULT_OS_MINOR,
            _userMajor = ModuleWriter.DEFAULT_USER_MAJOR,
            _userMinor = ModuleWriter.DEFAULT_USER_MINOR,
            _subSysMajor = ModuleWriter.DEFAULT_SUBSYS_MAJOR,
            _subSysMinor = ModuleWriter.DEFAULT_SUBSYS_MINOR,
            _cliMajor = cliMajor,
            _cliMinor = cliMinor
         };
      }

      /// <summary>
      /// Creates a new instance of <see cref="EmittingArguments"/> suitable to be used in loading any module.
      /// </summary>
      /// <param name="customAssemblyLoader">Callback to load any assemblies referenced by the module returned by this method.</param>
      /// <param name="ownerAssemblyLoader">The callback to use to get the assembly of modules, which are not main modules (i.e. their <c>Assembly</c> table does not have exacltly one row).</param>
      /// <param name="fileStreamOpener">The callback to use when the File table contains entries and they are accessed.</param>
      /// <returns>A new instance of <see cref="EmittingArguments"/> suitable to be used in loading any module.</returns>
      public static EmittingArguments CreateForLoadingModule(
         Func<CILModule, CILAssemblyName, CILAssembly> customAssemblyLoader = null,
         Func<CILModule, CILAssembly> ownerAssemblyLoader = null,
         Func<CILModule, String, Stream> fileStreamOpener = null
         )
      {
         return new EmittingArguments
         {
            _assemblyRefLoader = customAssemblyLoader,
            _ownerAssemblyLoader = ownerAssemblyLoader,
            _fileStreamOpener = fileStreamOpener
         };
      }

      /// <summary>
      /// Creates a new instance of <see cref="EmittingArguments"/> suitable to be used in loading any assembly.
      /// </summary>
      /// <param name="customAssemblyLoader">Callback to load any assemblies referenced by the assembly returned by this method.</param>
      /// <param name="fileStreamOpener">The callback to use when the File table contains entries and they are accessed.</param>
      /// <returns>A new instance of <see cref="EmittingArguments"/> suitable to be used in loading any assembly.</returns>
      public static EmittingArguments CreateForLoadingAssembly(
         Func<CILModule, CILAssemblyName, CILAssembly> customAssemblyLoader = null,
         Func<CILModule, String, Stream> fileStreamOpener = null
         )
      {
         return CreateForLoadingModule( customAssemblyLoader, null, fileStreamOpener );
      }
   }
}

public static partial class E_CIL
{

   /// <summary>
   /// Convenience method to map <see cref="CILType"/>s.
   /// </summary>
   /// <param name="mapper">The <see cref="EmittingAssemblyMapper"/>.</param>
   /// <param name="type">The <see cref="CILType"/> to map.</param>
   /// <returns>If <paramref name="mapper"/> is <c>null</c>, returns <paramref name="type"/>. Otherwise, returns casted result of <see cref="EmittingAssemblyMapper.MapTypeBase"/> for <paramref name="type"/>.</returns>
   public static CILType TryMapType( this EmittingAssemblyMapper mapper, CILType type )
   {
      return mapper == null ? type : (CILType) mapper.MapTypeBase( type );
   }

   /// <summary>
   /// Convenience method to map <see cref="CILTypeParameter"/>s.
   /// </summary>
   /// <param name="mapper">The <see cref="EmittingAssemblyMapper"/>.</param>
   /// <param name="typeParam">The <see cref="CILTypeParameter"/> to map.</param>
   /// <returns>If <paramref name="mapper"/> is <c>null</c>, returns <paramref name="typeParam"/>. Otherwise, returns casted result of <see cref="EmittingAssemblyMapper.MapTypeBase"/> for <paramref name="typeParam"/>.</returns>
   public static CILTypeParameter TryMapTypeParameter( this EmittingAssemblyMapper mapper, CILTypeParameter typeParam )
   {
      return mapper == null ? typeParam : (CILTypeParameter) mapper.MapTypeBase( typeParam );
   }

   /// <summary>
   /// Convenience method to map <see cref="CILTypeBase"/>s when <paramref name="mapper"/> may be <c>null</c>.
   /// </summary>
   /// <param name="mapper">The <see cref="EmittingAssemblyMapper"/>.</param>
   /// <param name="typeBase">The <see cref="CILTypeBase"/> to map.</param>
   /// <returns>If <paramref name="mapper"/> is <c>null</c>, returns <paramref name="typeBase"/>. Otherwise, returns result of <see cref="EmittingAssemblyMapper.MapTypeBase"/> for <paramref name="typeBase"/>.</returns>
   public static CILTypeBase TryMapTypeBase( this EmittingAssemblyMapper mapper, CILTypeBase typeBase )
   {
      return mapper == null ? typeBase : mapper.MapTypeBase( typeBase );
   }

   /// <summary>
   /// Convenience method to map <see cref="CILMethod"/>s.
   /// </summary>
   /// <param name="mapper">The <see cref="EmittingAssemblyMapper"/>.</param>
   /// <param name="method">The <see cref="CILMethod"/> to map.</param>
   /// <returns>If <paramref name="mapper"/> is <c>null</c>, returns <paramref name="method"/>. Otherwise, returns casted result of <see cref="EmittingAssemblyMapper.MapMethodBase"/> for <paramref name="method"/>.</returns>
   public static CILMethod TryMapMethod( this EmittingAssemblyMapper mapper, CILMethod method )
   {
      return mapper == null ? method : (CILMethod) mapper.MapMethodBase( method );
   }

   /// <summary>
   /// Convenience method to map <see cref="CILConstructor"/>s.
   /// </summary>
   /// <param name="mapper">The <see cref="EmittingAssemblyMapper"/>.</param>
   /// <param name="ctor">The <see cref="CILConstructor"/> to map.</param>
   /// <returns>If <paramref name="mapper"/> is <c>null</c>, returns <paramref name="ctor"/>. Otherwise, returns casted result of <see cref="EmittingAssemblyMapper.MapMethodBase"/> for <paramref name="ctor"/>.</returns>
   public static CILConstructor TryMapConstructor( this EmittingAssemblyMapper mapper, CILConstructor ctor )
   {
      return mapper == null ? ctor : (CILConstructor) mapper.MapMethodBase( ctor );
   }

   /// <summary>
   /// Convenience method to map <see cref="CILMethodBase"/>s when <paramref name="mapper"/> may be <c>null</c>.
   /// </summary>
   /// <param name="mapper">The <see cref="EmittingAssemblyMapper"/>.</param>
   /// <param name="methodBase">The <see cref="CILMethodBase"/> to map.</param>
   /// <returns>If <paramref name="mapper"/> is <c>null</c>, returns <paramref name="methodBase"/>. Otherwise, returns result of <see cref="EmittingAssemblyMapper.MapMethodBase"/> for <paramref name="methodBase"/>.</returns>
   public static CILMethodBase TryMapMethodBase( this EmittingAssemblyMapper mapper, CILMethodBase methodBase )
   {
      return mapper == null ? methodBase : mapper.MapMethodBase( methodBase );
   }

   /// <summary>
   /// Convenience method to map <see cref="CILField"/>s when <paramref name="mapper"/> may be <c>null</c>.
   /// </summary>
   /// <param name="mapper">The <see cref="EmittingAssemblyMapper"/></param>
   /// <param name="field">The <see cref="CILField"/> to map.</param>
   /// <returns>If <paramref name="mapper"/> is <c>null</c>, returns <paramref name="field"/>. Otherwise, returns result of <see cref="EmittingAssemblyMapper.MapField"/> for <paramref name="field"/>.</returns>
   public static CILField TryMapField( this EmittingAssemblyMapper mapper, CILField field )
   {
      return mapper == null ? field : mapper.MapField( field );
   }

   /// <summary>
   /// Helper method to get <see cref="CILType"/> from given token that represents reference to TypeDef (<c>0x02</c>) table in metadata.
   /// </summary>
   /// <param name="md">The <see cref="EmittingMetadataInfo"/>.</param>
   /// <param name="token">The 1-based, table-encoded token (precisely as it appears in metadata).</param>
   /// <param name="result">The <see cref="CILType"/> corresponding to given token or <c>null</c> if index is too low or too high.</param>
   /// <returns><c>true</c> if lookup succeeds, <c>false</c> otherwise.</returns>
   public static Boolean TryGetTypeDefinitionForToken( this EmittingMetadataInfo md, Int32 token, out CILType result )
   {
      var retVal = ( --token ) >= 0 && token < md.Token2Type.Count;
      result = retVal ? md.Token2Type[token & TokenUtils.INDEX_MASK] : null;
      return retVal;
   }

   /// <summary>
   /// Helper method to get <see cref="CILMethodBase"/> from given token that represents reference to MethodDef (<c>0x06</c>) table in metadata.
   /// </summary>
   /// <param name="md">The <see cref="EmittingMetadataInfo"/>.</param>
   /// <param name="token">The 1-based, table-encoded token (precisely as it appears in metadata).</param>
   /// <param name="result">The <see cref="CILMethodBase"/> corresponding to given token or <c>null</c> if index is too low or too high.</param>
   /// <returns><c>true</c> if lookup succeeds, <c>false</c> otherwise.</returns>
   public static Boolean TryGetMethodDefinitionForToken( this EmittingMetadataInfo md, Int32 token, out CILMethodBase result )
   {
      var retVal = ( --token ) >= 0 && token < md.Token2Method.Count;
      result = retVal ? md.Token2Method[token & TokenUtils.INDEX_MASK] : null;
      return retVal;
   }

   /// <summary>
   /// Helper method to get <see cref="CILParameter"/> from given token that represents reference to Param (<c>0x08</c>) table in metadata.
   /// </summary>
   /// <param name="md">The <see cref="EmittingMetadataInfo"/>.</param>
   /// <param name="token">The 1-based, table-encoded token (precisely as it appears in metadata).</param>
   /// <param name="result">The <see cref="CILParameter"/> corresponding to given token or <c>null</c> if index is too low or too high.</param>
   /// <returns><c>true</c> if lookup succeeds, <c>false</c> otherwise.</returns>
   public static Boolean TryGetParameterDefinitionForToken( this EmittingMetadataInfo md, Int32 token, out CILParameter result )
   {
      var retVal = ( --token ) >= 0 && token < md.Token2Parameter.Count;
      result = retVal ? md.Token2Parameter[token & TokenUtils.INDEX_MASK] : null;
      return retVal;
   }

   /// <summary>
   /// Helper method to get <see cref="CILField"/> from given token that represents reference to Field (<c>0x04</c>) table in metadata.
   /// </summary>
   /// <param name="md">The <see cref="EmittingMetadataInfo"/>.</param>
   /// <param name="token">The 1-based, table-encoded token (precisely as it appears in metadata).</param>
   /// <param name="result">The <see cref="CILType"/> corresponding to given token or <c>null</c> if index is too low or too high.</param>
   /// <returns><c>true</c> if lookup succeeds, <c>false</c> otherwise.</returns>
   public static Boolean TryGetFieldDefinitionForToken( this EmittingMetadataInfo md, Int32 token, out CILField result )
   {
      var retVal = ( --token ) >= 0 && token < md.Token2Field.Count;
      result = retVal ? md.Token2Field[token & TokenUtils.INDEX_MASK] : null;
      return retVal;
   }

   /// <summary>
   /// Helper method to get <see cref="CILProperty"/> from given token that represents reference to Property (<c>0x17</c>) table in metadata.
   /// </summary>
   /// <param name="md">The <see cref="EmittingMetadataInfo"/>.</param>
   /// <param name="token">The 1-based, table-encoded token (precisely as it appears in metadata).</param>
   /// <param name="result">The <see cref="CILProperty"/> corresponding to given token or <c>null</c> if index is too low or too high.</param>
   /// <returns><c>true</c> if lookup succeeds, <c>false</c> otherwise.</returns>
   public static Boolean TryGetPropertyDefinitionForToken( this EmittingMetadataInfo md, Int32 token, out CILProperty result )
   {
      var retVal = ( --token ) >= 0 && token < md.Token2Property.Count;
      result = retVal ? md.Token2Property[token & TokenUtils.INDEX_MASK] : null;
      return retVal;
   }

   /// <summary>
   /// Helper method to get <see cref="CILEvent"/> from given token that represents reference to Event (<c>0x14</c>) table in metadata.
   /// </summary>
   /// <param name="md">The <see cref="EmittingMetadataInfo"/>.</param>
   /// <param name="token">The 1-based, table-encoded token (precisely as it appears in metadata).</param>
   /// <param name="result">The <see cref="CILEvent"/> corresponding to given token or <c>null</c> if index is too low or too high.</param>
   /// <returns><c>true</c> if lookup succeeds, <c>false</c> otherwise.</returns>
   public static Boolean TryGetEventDefinitionForToken( this EmittingMetadataInfo md, Int32 token, out CILEvent result )
   {
      var retVal = ( --token ) >= 0 && token < md.Token2Event.Count;
      result = retVal ? md.Token2Event[token & TokenUtils.INDEX_MASK] : null;
      return retVal;
   }

   /// <summary>
   /// Returns table-encoded, 1-based token (precisely as it should appear in metadata) for given <see cref="CILType"/>.
   /// </summary>
   /// <param name="md">The <see cref="EmittingMetadataInfo"/>.</param>
   /// <param name="type">The <see cref="CILType"/>.</param>
   /// <param name="token">This will contain table-encoded, 1-based token (precisely as it should appear in metadata) for given <see cref="CILType"/> or <c>0</c> if no token information found.</param>
   /// <returns><c>true</c> if lookup succeeds, <c>false</c> otherwise.</returns>
   public static Boolean TryGetTokenForTypeDefinition( this EmittingMetadataInfo md, CILType type, out Int32 token )
   {
      token = 0;
      var retVal = type != null && md.Type2Token.TryGetValue( type, out token );
      if ( retVal )
      {
         token = TokenUtils.EncodeToken( Tables.TypeDef, token + 1 );
      }
      return retVal;
   }

   /// <summary>
   /// Returns table-encoded, 1-based token (precisely as it should appear in metadata) for given <see cref="CILMethodBase"/>.
   /// </summary>
   /// <param name="md">The <see cref="EmittingMetadataInfo"/>.</param>
   /// <param name="method">The <see cref="CILMethodBase"/>.</param>
   /// <param name="token">This will contain table-encoded, 1-based token (precisely as it should appear in metadata) for given <see cref="CILMethodBase"/> or <c>0</c> if no token information found.</param>
   /// <returns><c>true</c> if lookup succeeds, <c>false</c> otherwise.</returns>
   public static Boolean TryGetTokenForMethodDefinition( this EmittingMetadataInfo md, CILMethodBase method, out Int32 token )
   {
      token = 0;
      var retVal = method != null && md.Method2Token.TryGetValue( method, out token );
      if ( retVal )
      {
         token = TokenUtils.EncodeToken( Tables.MethodDef, token + 1 );
      }
      return retVal;
   }

   /// <summary>
   /// Returns table-encoded, 1-based token (precisely as it should appear in metadata) for given <see cref="CILParameter"/>.
   /// </summary>
   /// <param name="md">The <see cref="EmittingMetadataInfo"/>.</param>
   /// <param name="param">The <see cref="CILParameter"/>.</param>
   /// <param name="token">This will contain table-encoded, 1-based token (precisely as it should appear in metadata) for given <see cref="CILParameter"/> or <c>0</c> if no token information found.</param>
   /// <returns><c>true</c> if lookup succeeds, <c>false</c> otherwise.</returns>
   public static Boolean TryGetTokenForParameterDefinition( this EmittingMetadataInfo md, CILParameter param, out Int32 token )
   {
      token = 0;
      var retVal = param != null && md.Parameter2Token.TryGetValue( param, out token );
      if ( retVal )
      {
         token = TokenUtils.EncodeToken( Tables.Parameter, token + 1 );
      }
      return retVal;
   }

   /// <summary>
   /// Returns table-encoded, 1-based token (precisely as it should appear in metadata) for given <see cref="CILField"/>.
   /// </summary>
   /// <param name="md">The <see cref="EmittingMetadataInfo"/>.</param>
   /// <param name="field">The <see cref="CILField"/>.</param>
   /// <param name="token">This will contain table-encoded, 1-based token (precisely as it should appear in metadata) for given <see cref="CILField"/> or <c>0</c> if no token information found.</param>
   /// <returns><c>true</c> if lookup succeeds, <c>false</c> otherwise.</returns>
   public static Boolean TryGetTokenForFieldDefinition( this EmittingMetadataInfo md, CILField field, out Int32 token )
   {
      token = 0;
      var retVal = field != null && md.Field2Token.TryGetValue( field, out token );
      if ( retVal )
      {
         token = TokenUtils.EncodeToken( Tables.Field, token + 1 );
      }
      return retVal;
   }

   /// <summary>
   /// Returns table-encoded, 1-based token (precisely as it should appear in metadata) for given <see cref="CILProperty"/>.
   /// </summary>
   /// <param name="md">The <see cref="EmittingMetadataInfo"/>.</param>
   /// <param name="property">The <see cref="CILProperty"/>.</param>
   /// <param name="token">This will contain table-encoded, 1-based token (precisely as it should appear in metadata) for given <see cref="CILProperty"/> or <c>0</c> if no token information found.</param>
   /// <returns><c>true</c> if lookup succeeds, <c>false</c> otherwise.</returns>
   public static Boolean TryGetTokenForPropertyDefinition( this EmittingMetadataInfo md, CILProperty property, out Int32 token )
   {
      token = 0;
      var retVal = property != null && md.Property2Token.TryGetValue( property, out token );
      if ( retVal )
      {
         token = TokenUtils.EncodeToken( Tables.Property, token + 1 );
      }
      return retVal;
   }

   /// <summary>
   /// Returns table-encoded, 1-based token (precisely as it should appear in metadata) for given <see cref="CILEvent"/>.
   /// </summary>
   /// <param name="md">The <see cref="EmittingMetadataInfo"/>.</param>
   /// <param name="evt">The <see cref="CILEvent"/>.</param>
   /// <param name="token">This will contain table-encoded, 1-based token (precisely as it should appear in metadata) for given <see cref="CILEvent"/> or <c>0</c> if no token information found.</param>
   /// <returns><c>true</c> if lookup succeeds, <c>false</c> otherwise.</returns>
   public static Boolean TryGetTokenForEventDefinition( this EmittingMetadataInfo md, CILEvent evt, out Int32 token )
   {
      token = 0;
      var retVal = evt != null && md.Event2Token.TryGetValue( evt, out token );
      if ( retVal )
      {
         token = TokenUtils.EncodeToken( Tables.Event, token + 1 );
      }
      return retVal;
   }
}