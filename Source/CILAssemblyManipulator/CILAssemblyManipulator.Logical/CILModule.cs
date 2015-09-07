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
using System.Text.RegularExpressions;
using CILAssemblyManipulator.Logical;
using CILAssemblyManipulator.Logical.Implementation;
using CollectionsWithRoles.API;
using CommonUtils;
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Logical
{
   /// <summary>
   /// This interface represents a module in CIL environment. The interface roughly corresponds to <see cref="System.Reflection.Module"/>. See ECMA specification for more information about CIL modules.
   /// </summary>
   public interface CILModule :
      CILCustomAttributeContainer,
      CILElementWithSimpleName,
      CILElementCapableOfDefiningType,
      CILElementWithContext
   {
      /// <summary>
      /// Gets the <see cref="CILAssembly">assembly</see> containing this module.
      /// </summary>
      /// <value>The <see cref="CILAssembly">assembly</see> containing this module.</value>
      /// <seealso cref="System.Reflection.Module.Assembly"/>
      CILAssembly Assembly { get; }

      /// <summary>
      /// Gets all top-level (not nested) types defined in this module.
      /// </summary>
      /// <value>All top-level (not nested) types defined in this module.</value>
      ListQuery<CILType> DefinedTypes { get; }

      /// <summary>
      /// Gets the module initializer type. This type, among other things, contains globally declared methods. See ECMA specification for more information about module initializer type.
      /// </summary>
      /// <value>The module initializer type.</value>
      CILType ModuleInitializer { get; }

      /// <summary>
      /// Gets or sets the <c>mscorlib</c> module associated with this <see cref="CILModule"/>.
      /// </summary>
      /// <value>The <c>mscorlib</c> module associated with this <see cref="CILModule"/>.</value>
      CILModule AssociatedMSCorLibModule { get; set; }

      /// <summary>
      /// Resolves a type string of any type defined in this module.
      /// </summary>
      /// <param name="typeString">The textual name of the type.</param>
      /// <param name="throwOnError">Whether to throw an exception if matching type is not found.</param>
      /// <returns>The resolved <see cref="CILType"/>, or <c>null</c> if <paramref name="throwOnError"/> is <c>false</c> and type could not be resolved.</returns>
      /// <exception cref="ArgumentException">If <paramref name="throwOnError"/> is <c>true</c> and <see cref="CILType"/> could not be resolved for <paramref name="typeString"/>.</exception>
      CILType GetTypeByName( String typeString, Boolean throwOnError = true );

      /// <summary>
      /// Gets the manifest resource information of this module.
      /// </summary>
      /// <value>The manifest resource information of this module.</value>
      IDictionary<String, AbstractLogicalManifestResource> ManifestResources { get; }
   }

   /// <summary>
   /// This enumeration describes what kind of manifest resource the <see cref="AbstractLogicalManifestResource"/> actually is.
   /// </summary>
   public enum ManifestResourceKind
   {
      /// <summary>
      /// This manifest resource is <see cref="EmbeddedManifestResource"/>.
      /// </summary>
      Embedded,
      /// <summary>
      /// This manifest resource is <see cref="FileManifestResource"/>.
      /// </summary>
      AnotherFile,
      /// <summary>
      /// This manifest resource is <see cref="AssemblyManifestResource"/>.
      /// </summary>
      AnotherAssembly
   }

   /// <summary>
   /// This class is common base type for <see cref="AssemblyManifestResource"/> and <see cref="EmbeddedManifestResource"/>.
   /// </summary>
   public abstract class AbstractLogicalManifestResource
   {
      private ManifestResourceAttributes _attributes;

      internal AbstractLogicalManifestResource( ManifestResourceAttributes attributes )
      {
         this._attributes = attributes;
      }

      /// <summary>
      /// Gets or sets the <see cref="ManifestResourceAttributes"/> associated with this manifest resource.
      /// </summary>
      /// <value>The <see cref="ManifestResourceAttributes"/> associated with this manifest resource.</value>
      public ManifestResourceAttributes Attributes
      {
         get
         {
            return this._attributes;
         }
         set
         {
            this._attributes = value;
         }
      }

      /// <summary>
      /// Returns enumeration telling what kind of manifest resource this is.
      /// </summary>
      /// <value>The enumeration telling what kind of manifest resource this is.</value>
      /// <seealso cref="ManifestResourceKind"/>
      public abstract ManifestResourceKind ManifestResourceKind { get; }
   }

   /// <summary>
   /// This class represents a manifest resource which is other module.
   /// </summary>
   public sealed class AssemblyManifestResource : AbstractLogicalManifestResource
   {
      private readonly Lazy<CILAssembly> _assembly;

      /// <summary>
      /// Creates new instance of <see cref="AssemblyManifestResource"/>.
      /// </summary>
      /// <param name="attributes">The <see cref="ManifestResourceAttributes"/> associated with this manifest resource.</param>
      /// <param name="assembly">The module which acts as manifest resource.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="assembly"/> is <c>null</c>.</exception>
      public AssemblyManifestResource( ManifestResourceAttributes attributes, CILAssembly assembly )
         : base( attributes )
      {
         ArgumentValidator.ValidateNotNull( "Module", assembly );
         this._assembly = new Lazy<CILAssembly>( () => assembly, assembly.ReflectionContext.GetLazyThreadSafetyMode() );
      }

      /// <summary>
      /// Gets the module that acts as manifest resource.
      /// </summary>
      /// <value>The module that acts as manifest resource.</value>
      public CILAssembly Assembly
      {
         get
         {
            return this._assembly.Value;
         }
      }

      /// <summary>
      /// Returns <see cref="Logical.ManifestResourceKind.AnotherAssembly"/>.
      /// </summary>
      /// <value>The <see cref="Logical.ManifestResourceKind.AnotherAssembly"/>.</value>
      public override ManifestResourceKind ManifestResourceKind
      {
         get
         {
            return ManifestResourceKind.AnotherAssembly;
         }
      }
   }

   /// <summary>
   /// This class represents a manfiest resource which is in a file, and the file is not a CIL module.
   /// </summary>
   public sealed class FileManifestResource : AbstractLogicalManifestResource
   {
      private readonly String _fileName;
      private readonly Byte[] _hash;

      /// <summary>
      /// Creates a new instance of <see cref="FileManifestResource"/>.
      /// </summary>
      /// <param name="attributes">The <see cref="ManifestResourceAttributes"/> associated with this manifest resource.</param>
      /// <param name="fileName">The name of the file containing the resource data. If <c>null</c>, this resource will be ignored when emitting the module.</param>
      /// <param name="hash">
      /// SHA1 hash of file contents.
      /// </param>
      /// <remarks>
      /// If <paramref name="hash"/> is non-<c>null</c> (includes scenario when it is empty), then the hash will be written as is to metadata.
      /// </remarks>
      public FileManifestResource( ManifestResourceAttributes attributes, String fileName, Byte[] hash = null )
         : base( attributes )
      {
         this._fileName = fileName;
         this._hash = hash;
      }

      /// <summary>
      /// Gets the file name of this manifest resource.
      /// </summary>
      /// <value>The file name of this manifest resource.</value>
      public String FileName
      {
         get
         {
            return this._fileName;
         }
      }

      /// <summary>
      /// Gets the hash of the file contents.
      /// </summary>
      /// <value>The hash of the file contents.</value>
      public Byte[] Hash
      {
         get
         {
            return this._hash;
         }
      }

      /// <summary>
      /// Returns <see cref="Logical.ManifestResourceKind.AnotherFile"/>.
      /// </summary>
      /// <value>The <see cref="Logical.ManifestResourceKind.AnotherFile"/>.</value>
      public override ManifestResourceKind ManifestResourceKind
      {
         get
         {
            return ManifestResourceKind.AnotherFile;
         }
      }
   }
   /// <summary>
   /// This class represents a manifest resource which will be embeddeed into the module when it is emitted.
   /// </summary>
   public sealed class EmbeddedManifestResource : AbstractLogicalManifestResource
   {
      private readonly Byte[] _data;

      /// <summary>
      /// Creates new instance of <see cref="EmbeddedManifestResource"/>.
      /// </summary>
      /// <param name="attributes">The <see cref="ManifestResourceAttributes"/> associated with this embedded manifest resource.</param>
      /// <param name="data">The raw data as byte array.</param>
      public EmbeddedManifestResource( ManifestResourceAttributes attributes, Byte[] data )
         : base( attributes )
      {
         this._data = data ?? Empty<Byte>.Array;
      }

      /// <summary>
      /// Gets the raw data of this manifest resource.
      /// </summary>
      /// <value>The raw data of this manifest resource.</value>
      public Byte[] Data
      {
         get
         {
            return this._data;
         }
      }

      /// <summary>
      /// Returns <see cref="Logical.ManifestResourceKind.Embedded"/>.
      /// </summary>
      /// <value>The <see cref="Logical.ManifestResourceKind.Embedded"/>.</value>
      public override ManifestResourceKind ManifestResourceKind
      {
         get
         {
            return ManifestResourceKind.Embedded;
         }
      }
   }
}

public static partial class E_CILLogical
{
   private static readonly Regex MODULE_NAME_WITHOUT_EXTENSION_REGEX = new Regex( @"\.(dll|exe|netmodule)$", RegexOptions.IgnoreCase );

   /// <summary>
   /// Gets the plain module name without '.dll', '.exe' or '.netmodule' extension.
   /// </summary>
   /// <param name="module">The module.</param>
   /// <returns>The value of <see cref="CILElementWithSimpleName.Name"/> of the <paramref name="module"/> without '.dll', '.exe' or '.netmodule' extension.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="module"/> is <c>null</c>.</exception>
   public static String GetPlainModuleName( this CILModule module )
   {
      ArgumentValidator.ValidateNotNull( "Module", module );
      return MODULE_NAME_WITHOUT_EXTENSION_REGEX.Replace( module.Name, "" );
   }

   /// <summary>
   /// Adds a global method with the specified name, attributes, and calling conventions to <see cref="CILModule"/>.
   /// </summary>
   /// <param name="module">The module to add global method to.</param>
   /// <param name="name">The name of the global method.</param>
   /// <param name="attrs">The <see cref="MethodAttributes"/> of the global method.</param>
   /// <param name="callingConventions">The <see cref="CallingConventions"/> of the global method.</param>
   /// <returns>A newly created global method.</returns>
   /// <seealso cref="CILMethod"/>
   /// <see cref="CILModule.ModuleInitializer"/>
   public static CILMethod AddGlobalMethod( this CILModule module, String name, MethodAttributes attrs, CallingConventions callingConventions )
   {
      return module.ModuleInitializer.AddMethod( name, attrs, callingConventions );
   }

   ///// <summary>
   ///// Performs the emitting of the associated <see cref="CILModule"/> to <paramref name="stream"/>. See ECMA specification for more information about various PE and CLR header fields.
   ///// </summary>
   ///// <param name="module">The module to emit.</param>
   ///// <param name="stream">The stream to emit the associated <see cref="CILModule"/> to.</param>
   ///// <param name="emittingArgs">The <see cref="EmittingArguments"/>.</param>
   ///// <exception cref="ArgumentNullException">
   ///// If <paramref name="module"/>, <paramref name="stream"/> or <see cref="EmittingArguments.MetaDataVersion"/> of <paramref name="emittingArgs"/> is <c>null</c>.
   ///// Also if any of <see cref="EmittingArguments.CorLibName"/>, <see cref="EmittingArguments.ImportHintName"/> or <see cref="EmittingArguments.ImportDirectoryName"/> of <paramref name="emittingArgs"/> is used and is <c>null</c>.
   ///// </exception>
   //public static void EmitModule( this CILModule module, Stream stream, EmittingArguments emittingArgs )
   //{
   //   new ModuleWriter( module )
   //      .PerformEmitting( stream, emittingArgs );
   //}

   /// <summary>
   /// Returns enumerable of all types defined in this module, including module initializer type, and recursively all nested types.
   /// </summary>
   /// <param name="module">The module.</param>
   /// <returns>An enumerable of all types defined in this module. Will be empty if <paramref name="module"/> is <c>null</c>.</returns>
   public static IEnumerable<CILType> GetAllTypes( this CILModule module )
   {
      if ( module != null )
      {
         yield return module.ModuleInitializer;

         foreach ( var type in module.DefinedTypes.SelectMany( t => t.AsDepthFirstEnumerable( tt => tt.DeclaredNestedTypes ) ) )
         {
            yield return type;
         }
      }
   }

   /// <summary>
   /// Gets the corresponding <see cref="CILType"/> for given <see cref="CILTypeCode"/>.
   /// </summary>
   /// <param name="module">The current <see cref="CILModule"/>.</param>
   /// <param name="code">The <see cref="CILTypeCode"/>.</param>
   /// <returns>The <see cref="CILTypeCode"/> which corresponds to given <see cref="CILTypeCode"/>.</returns>
   /// <exception cref="ArgumentException">If <paramref name="module"/> is <c>null</c>, or corresponding type was not found.</exception>
   public static CILType GetTypeForTypeCode( this CILModule module, CILTypeCode code )
   {
      CILType retVal;
      if ( !module.TryGetTypeForTypeCode( code, out retVal ) )
      {
         throw new ArgumentException( "Failed to get corresponding type for " + code + " from associated mscorlib of " + module + "." );
      }

      return retVal;
   }

   /// <summary>
   /// Tries to find a type from <see cref="CILModule.AssociatedMSCorLibModule"/> that corresponds given <see cref="CILTypeCode"/>.
   /// </summary>
   /// <param name="module">The current <see cref="CILModule"/>.</param>
   /// <param name="code">The <see cref="CILTypeCode"/>.</param>
   /// <param name="type">This will hold the result of look-up.</param>
   /// <returns><c>true</c> if look-up was successful; <c>false</c> otherwise.</returns>
   public static Boolean TryGetTypeForTypeCode( this CILModule module, CILTypeCode code, out CILType type )
   {
      String typeName;
      switch ( code )
      {
         case CILTypeCode.Boolean:
            typeName = Consts.BOOLEAN;
            break;
         case CILTypeCode.Char:
            typeName = Consts.CHAR;
            break;
         case CILTypeCode.SByte:
            typeName = Consts.SBYTE;
            break;
         case CILTypeCode.Byte:
            typeName = Consts.BYTE;
            break;
         case CILTypeCode.Int16:
            typeName = Consts.INT16;
            break;
         case CILTypeCode.UInt16:
            typeName = Consts.UINT16;
            break;
         case CILTypeCode.Int32:
            typeName = Consts.INT32;
            break;
         case CILTypeCode.UInt32:
            typeName = Consts.UINT32;
            break;
         case CILTypeCode.Int64:
            typeName = Consts.INT64;
            break;
         case CILTypeCode.UInt64:
            typeName = Consts.UINT64;
            break;
         case CILTypeCode.Single:
            typeName = Consts.SINGLE;
            break;
         case CILTypeCode.Double:
            typeName = Consts.DOUBLE;
            break;
         case CILTypeCode.String:
            typeName = Consts.STRING;
            break;
         case CILTypeCode.Void:
            typeName = Consts.VOID;
            break;
         case CILTypeCode.IntPtr:
            typeName = Consts.INT_PTR;
            break;
         case CILTypeCode.UIntPtr:
            typeName = Consts.UINT_PTR;
            break;
         case CILTypeCode.DateTime:
            typeName = Consts.DATETIME;
            break;
         case CILTypeCode.Decimal:
            typeName = Consts.DECIMAL;
            break;
         case CILTypeCode.Enum:
            typeName = Consts.ENUM;
            break;
         case CILTypeCode.SystemObject:
            typeName = Consts.OBJECT;
            break;
         case CILTypeCode.Type:
            typeName = Consts.TYPE;
            break;
         case CILTypeCode.TypedByRef:
            typeName = Consts.TYPED_BY_REF;
            break;
         case CILTypeCode.Value:
            typeName = Consts.VALUE_TYPE;
            break;
         default:
            typeName = null;
            break;
      }

      var msCorLib = module == null ? null : module.AssociatedMSCorLibModule;
      type = typeName == null || msCorLib == null ? null : msCorLib.GetTypeByName( typeName, false );

      return type != null;
   }

}