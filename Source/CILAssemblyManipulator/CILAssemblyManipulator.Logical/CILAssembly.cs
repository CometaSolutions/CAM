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
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using CILAssemblyManipulator.Logical;
using CILAssemblyManipulator.Logical.Implementation;
using CollectionsWithRoles.API;
using CommonUtils;
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Logical
{
   /// <summary>
   /// This interface represents an assembly in CIL environment. The interface roughly corresponds to <see cref="System.Reflection.Assembly"/>. See ECMA specification for more information about CIL assemblies.
   /// </summary>
   public interface CILAssembly :
      CILCustomAttributeContainer,
      CILElementWithContext
   {
      /// <summary>
      /// Adds a new module with the specified name to this assembly.
      /// </summary>
      /// <param name="name">The name of the module.</param>
      /// <returns>A blank module with specified name.</returns>
      /// <remarks>
      /// This method does not throw exception even if there already exists a module with similar name. Since module names can be changed, there is no reason to throw exception at this point.
      /// Additionally, if this is the first module to be added to assembly, then it becomes the <see cref="MainModule"/> of this assembly.
      /// </remarks>
      CILModule AddModule( String name );

      /// <summary>
      /// Returns qualified name of this assembly.
      /// </summary>
      /// <value>Qualified name of this assembly.</value>
      /// <seealso cref="CILAssemblyName"/>
      CILAssemblyName Name { get; }

      /// <summary>
      /// Returns all the modules this assembly currently contains.
      /// </summary>
      /// <value>All the modules this assembly currently contains.</value>
      ListQuery<CILModule> Modules { get; }

      /// <summary>
      /// Gets or sets the module which will have assembly manifest when emitted.
      /// </summary>
      /// <value>The module which will have assembly manifest when emitted.</value>
      /// <exception cref="ArgumentException">For setter only. Is thrown when trying to set value to some module which is not part of this assembly (not found in <see cref="Modules"/> list).</exception>
      CILModule MainModule { get; set; }

      /// <summary>
      /// Returns all the forwarded types from this assembly into other assemblies, including nested types. To find out more about forwarded types, see ECMA specification about ExportedType table in module.
      /// The key is a <c>(name, namespace)</c> tuple, the value is <see cref="TypeForwardingInfo"/>.
      /// </summary>
      /// <value>All the forwarded types of this assembly into other assemblies, including nested types.</value>
      /// <seealso cref="TypeForwardingInfo"/>
      DictionaryQuery<Tuple<String, String>, TypeForwardingInfo> ForwardedTypeInfos { get; }

      /// <summary>
      /// Tries to add a new <see cref="TypeForwardingInfo"/> to <see cref="ForwardedTypeInfos"/> of this <see cref="CILAssembly"/>.
      /// </summary>
      /// <param name="info">The <see cref="TypeForwardingInfo"/>.</param>
      /// <returns><c>true</c> if adding succeeded, that is, there previously was no other <see cref="TypeForwardingInfo"/> associated with name and namespace of given <paramref name="info"/>; <c>false</c> otherwise.</returns>
      Boolean TryAddForwardedType( TypeForwardingInfo info );

      /// <summary>
      /// Tries to remove a <see cref="TypeForwardingInfo"/> from <see cref="ForwardedTypeInfos"/> of this <see cref="CILAssembly"/>.
      /// </summary>
      /// <param name="name">The <see cref="TypeForwardingInfo.Name"/> of the <see cref="TypeForwardingInfo"/> to be removed.</param>
      /// <param name="ns">The <see cref="TypeForwardingInfo.Namespace"/> of the <see cref="TypeForwardingInfo"/> to be removed.</param>
      /// <returns><c>true</c> if removing succeeded; <c>false</c> otherwise.</returns>
      Boolean RemoveForwardedType( String name, String ns );
   }

   /// <summary>
   /// This structure describes information about a single type redirected from current assembly to another assembly.
   /// </summary>
   public struct TypeForwardingInfo
   {
      private readonly TypeAttributes _typeAttrs;
      private readonly String _name;
      private readonly String _namespace;
      private readonly String _declTypeName;
      private readonly String _declTypeNamespace;
      private readonly CILAssemblyName _assemblyName;

      /// <summary>
      /// Creates <see cref="TypeForwardingInfo"/> with information based on a <see cref="CILType"/>.
      /// </summary>
      /// <param name="type"></param>
      public TypeForwardingInfo( CILType type )
         : this( type.Attributes, type.Name, type.Namespace, type.Module.Assembly.Name )
      {

      }

      /// <summary>
      /// Creates <see cref="TypeForwardingInfo"/> with specified information.
      /// </summary>
      /// <param name="typeAttributes">The <see cref="TypeAttributes"/> of the target type.</param>
      /// <param name="name">The type name.</param>
      /// <param name="namespace">The type namespace.</param>
      /// <param name="assemblyName">The name of the assembly containing the type.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="name"/> or <paramref name="assemblyName"/> is <c>null</c>.</exception>
      /// <remarks>
      /// A copy of <paramref name="assemblyName"/> is created so that modifications done to the assembly name would not reflect to the assembly name given as parameter.
      /// </remarks>
      public TypeForwardingInfo( TypeAttributes typeAttributes, String name, String @namespace, CILAssemblyName assemblyName )
         : this( typeAttributes, name, @namespace, null, null, assemblyName )
      {

      }

      /// <summary>
      /// Creates <see cref="TypeForwardingInfo"/> with specified information.
      /// </summary>
      /// <param name="typeAttributes">The <see cref="TypeAttributes"/> of the target type.</param>
      /// <param name="name">The type name.</param>
      /// <param name="namespace">The type namespace.</param>
      /// <param name="declTypeName">The declaring type name. May be <c>null</c>.</param>
      /// <param name="declTypeNamespace">The declaring type namespace. May be <c>null</c>.</param>
      /// <param name="assemblyName">The name of the assembly containing the type.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="name"/> or <paramref name="assemblyName"/> is <c>null</c>.</exception>
      /// <exception cref="ArgumentException">If <paramref name="declTypeNamespace"/> is not <c>null</c> or empty, but <paramref name="declTypeName"/> is.</exception>
      /// <remarks>
      /// A copy of <paramref name="assemblyName"/> is created so that modifications done to the assembly name would not reflect to the assembly name given as parameter.
      /// </remarks>
      public TypeForwardingInfo( TypeAttributes typeAttributes, String name, String @namespace, String declTypeName, String declTypeNamespace, CILAssemblyName assemblyName )
      {
         ArgumentValidator.ValidateNotNull( "Type name", name );
         ArgumentValidator.ValidateNotNull( "Assembly name", assemblyName );
         if ( !String.IsNullOrEmpty( declTypeNamespace ) && String.IsNullOrEmpty( declTypeName ) )
         {
            throw new ArgumentException( "The declaring type namespace was specified but the declaring type name was not." );
         }

         this._typeAttrs = typeAttributes;
         this._name = name;
         this._namespace = @namespace;
         this._declTypeName = declTypeName;
         this._declTypeNamespace = declTypeNamespace;
         this._assemblyName = new CILAssemblyName( assemblyName );
      }

      /// <summary>
      /// Gets the <see cref="TypeAttributes"/> of the target type.
      /// </summary>
      /// <value>The <see cref="TypeAttributes"/> of the target type.</value>
      public TypeAttributes Attributes
      {
         get
         {
            return this._typeAttrs;
         }
      }

      /// <summary>
      /// Gets the name of the forwarded type.
      /// </summary>
      /// <value>The name of the forwarded type.</value>
      public String Name
      {
         get
         {
            return this._name;
         }
      }

      /// <summary>
      /// Gets the namespace of the forwarded type.
      /// </summary>
      /// <value>The namespace of the forwarded type.</value>
      public String Namespace
      {
         get
         {
            return this._namespace;
         }
      }

      /// <summary>
      /// Gets the name of the declaring type of the forwarded type.
      /// </summary>
      /// <value>The name of the forwarded type.</value>
      public String DeclaringTypeName
      {
         get
         {
            return this._declTypeName;
         }
      }

      /// <summary>
      /// Gets the namespace of the declaring type of the forwarded type.
      /// </summary>
      /// <value>The namespace of the forwarded type.</value>
      public String DeclaringTypeNamespace
      {
         get
         {
            return this._declTypeNamespace;
         }
      }

      /// <summary>
      /// Gets the <see cref="CILAssemblyName"/> describing the assembly where the type belongs to.
      /// It is safe to modify this.
      /// </summary>
      /// <value>The <see cref="CILAssemblyName"/> describing the assembly where the type belongs to.</value>
      public CILAssemblyName AssemblyName
      {
         get
         {
            return this._assemblyName;
         }
      }

      /// <inheritdoc />
      public override Boolean Equals( Object obj )
      {
         return obj is TypeForwardingInfo && this.Equals( (TypeForwardingInfo) obj );
      }

      /// <summary>
      /// Checks whether this <see cref="TypeForwardingInfo"/> has same values as given <see cref="TypeForwardingInfo"/>.
      /// </summary>
      /// <param name="tf">The <see cref="TypeForwardingInfo"/> to compare</param>
      /// <returns><c>true</c> if this <see cref="TypeForwardingInfo"/> has same values as <paramref name="tf"/>; <c>false</c> otherwise.</returns>
      public Boolean Equals( TypeForwardingInfo tf )
      {
         return String.Equals( this._name, tf._name )
            && String.Equals( this._namespace, tf._namespace )
            && String.Equals( this._declTypeName, tf._declTypeName )
            && String.Equals( this._declTypeNamespace, tf._declTypeName )
            && this._typeAttrs.Equals( tf._typeAttrs )
            && this._assemblyName.CorePropertiesEqual( tf._assemblyName );
      }

      /// <inheritdoc />
      public override Int32 GetHashCode()
      {
         return this._name.GetHashCode() ^ ( this._namespace == null ? 0 : this._namespace.GetHashCode() );
      }
   }

   /// <summary>
   /// This interface represents a qualified assembly name. See ECMA specification for more information about assembly names.
   /// </summary>
   public sealed class CILAssemblyName : CILElementWithSimpleName
   {
      private Int32 _hashAlgorithm;
      private Int32 _flags;
      private readonly AssemblyInformation _assemblyInfo;

      /// <summary>
      /// Creates a new instance of <see cref="CILAssemblyName"/> with given information in <see cref="AssemblyInformation"/>, and whether the public key is full public key or a public key token.
      /// </summary>
      /// <param name="assemblyInfo">The <see cref="AssemblyInformation"/> from which to read name, version, culture and public key information.</param>
      /// <param name="isFullPublicKey">Whether the public key in <paramref name="assemblyInfo"/> is full public key or a public key token.</param>
      public CILAssemblyName( AssemblyInformation assemblyInfo, Boolean isFullPublicKey )
         : this(
         assemblyInfo.Name,
         assemblyInfo.VersionMajor,
         assemblyInfo.VersionMinor,
         assemblyInfo.VersionBuild,
         assemblyInfo.VersionRevision,
         culture: assemblyInfo.Culture,
         pKey: assemblyInfo.PublicKeyOrToken,
         flags: isFullPublicKey ? AssemblyFlags.PublicKey : AssemblyFlags.None
         )
      {

      }


      /// <summary>
      /// Creates a new instance of <see cref="CILAssemblyName"/> with all fields set to default values.
      /// </summary>
      public CILAssemblyName()
         : this( (CILAssemblyName) null )
      {

      }

      /// <summary>
      /// Creates a new instance of <see cref="CILAssemblyName"/> with all fields set to have same values as <paramref name="otherName"/>.
      /// </summary>
      /// <param name="otherName">The <see cref="CILAssemblyName"/> to copy values from.</param>
      public CILAssemblyName( CILAssemblyName otherName )
         : this( otherName == null ? null : otherName.Name, otherName == null ? 0 : otherName.MajorVersion, otherName == null ? 0 : otherName.MinorVersion, otherName == null ? 0 : otherName.BuildNumber, otherName == null ? 0 : otherName.Revision, otherName == null ? AssemblyHashAlgorithm.SHA1 : otherName.HashAlgorithm, otherName == null ? AssemblyFlags.None : otherName.Flags, null, otherName == null ? null : otherName.Culture )
      {
         if ( otherName != null )
         {
            var pk = otherName.PublicKey;
            if ( !pk.IsNullOrEmpty() )
            {
               this._assemblyInfo.PublicKeyOrToken = pk.CreateBlockCopy();
            }
         }
      }

      /// <summary>
      /// Creates a new instance of <see cref="CILAssemblyName"/> with specified values.
      /// </summary>
      /// <param name="name">Assembly name.</param>
      /// <param name="major">Major version of the assembly name.</param>
      /// <param name="minor">Minor version of the assembly name.</param>
      /// <param name="build">Build number of the assembly name.</param>
      /// <param name="revision">Revision of the assembly name.</param>
      /// <param name="hashAlgorithm">The hash algorithm of the assembly name. For more information <see cref="AssemblyHashAlgorithm"/>.</param>
      /// <param name="flags">Assembly flags of the assembly name. For more information <see cref="AssemblyFlags"/>.</param>
      /// <param name="pKey">Public key of the assembly name. Specify <c>null</c> or empty array to create assembly name without public key.</param>
      /// <param name="culture">Culture string of the assembly name. Specify <c>null</c> or empty string to create culture-neutral assembly name.</param>
      /// <returns>A new <see cref="CILAssemblyName"/> with specified parameters.</returns>
      public CILAssemblyName(
         String name,
         Int32 major,
         Int32 minor,
         Int32 build,
         Int32 revision,
         AssemblyHashAlgorithm hashAlgorithm = AssemblyHashAlgorithm.None,
         AssemblyFlags flags = AssemblyFlags.None,
         Byte[] pKey = null,
         String culture = null
         )
      {
         this._hashAlgorithm = (Int32) hashAlgorithm;
         this._flags = (Int32) flags;
         this._assemblyInfo = new AssemblyInformation()
         {
            Name = name,
            Culture = culture,
            VersionMajor = major,
            VersionMinor = minor,
            VersionBuild = build,
            VersionRevision = revision,
            PublicKeyOrToken = pKey.IsNullOrEmpty() ? null : pKey
         };
      }

      #region CILAssemblyName Members

      /// <summary>
      /// Gets or sets the hash algorithm used for the related assembly.
      /// </summary>
      /// <value>The hash algorithm used for the related assembly.</value>
      /// <seealso cref="AssemblyHashAlgorithm"/>
      public AssemblyHashAlgorithm HashAlgorithm
      {
         set
         {
            Interlocked.Exchange( ref this._hashAlgorithm, (Int32) value );
         }
         get
         {
            return (AssemblyHashAlgorithm) this._hashAlgorithm;
         }
      }

      /// <summary>
      /// Gets or sets the flags associated with the related assembly.
      /// </summary>
      /// <value>The flags associated with the related assembly.</value>
      /// <seealso cref="AssemblyFlags"/>
      public AssemblyFlags Flags
      {
         set
         {
            Interlocked.Exchange( ref this._flags, (Int32) value );
         }
         get
         {
            return (AssemblyFlags) this._flags;
         }
      }

      /// <summary>
      /// Gets or sets the culture of the related assembly. Please note that culture-neutral assemblies have this property set to <c>null</c>.
      /// </summary>
      /// <value>The culture of the related assembly.</value>
      public String Culture
      {
         set
         {
            this._assemblyInfo.Culture = value;
         }
         get
         {
            return this._assemblyInfo.Culture;
         }
      }

      /// <summary>
      /// Gets or sets the major version of the related assembly. The value will be casted to <see cref="UInt16"/> when emitting.
      /// </summary>
      /// <value>The major version of the related assembly.</value>
      public Int32 MajorVersion
      {
         set
         {
            this._assemblyInfo.VersionMajor = value;
         }
         get
         {
            return this._assemblyInfo.VersionMajor;
         }
      }

      /// <summary>
      /// Gets or sets the minor version of the related assembly. The value will be casted to <see cref="UInt16"/> when emitting.
      /// </summary>
      /// <value>The minor version of the related assembly.</value>
      public Int32 MinorVersion
      {
         set
         {
            this._assemblyInfo.VersionMinor = value;
         }
         get
         {
            return this._assemblyInfo.VersionMinor;
         }
      }

      /// <summary>
      /// Gets or sets the build number of the related assembly. The value will be casted to <see cref="UInt16"/> when emitting.
      /// </summary>
      /// <value>The build number of the related assembly.</value>
      public Int32 BuildNumber
      {
         set
         {
            this._assemblyInfo.VersionBuild = value;
         }
         get
         {
            return this._assemblyInfo.VersionBuild;
         }
      }

      /// <summary>
      /// Gets or sets the revision of the related assembly. The value will be casted to <see cref="UInt16"/> when emitting.
      /// </summary>
      /// <value>The revision of the related assembly.</value>
      public Int32 Revision
      {
         set
         {
            this._assemblyInfo.VersionRevision = value;
         }
         get
         {
            return this._assemblyInfo.VersionRevision;
         }
      }

      /// <summary>
      /// Gets or sets the public key of the related assembly. Set to <c>null</c> or empty array to remove the usage of public key in the related assembly.
      /// </summary>
      /// <value>The public key of the related assembly.</value>
      public Byte[] PublicKey
      {
         set
         {
            this._assemblyInfo.PublicKeyOrToken = value.IsNullOrEmpty() ? null : value;
         }
         get
         {
            return this._assemblyInfo.PublicKeyOrToken;
         }
      }

      #endregion

      #region CILElementWithSimpleName Members

      /// <inheritdoc />
      public String Name
      {
         set
         {
            this._assemblyInfo.Name = value;
         }
         get
         {
            return this._assemblyInfo.Name;
         }
      }

      #endregion

      /// <summary>
      /// Returns the physical assembly information about this assembly.
      /// </summary>
      /// <value>The physical assembly information about this assembly.</value>
      public AssemblyInformation AssemblyInformation
      {
         get
         {
            return this._assemblyInfo;
         }
      }

      /// <inheritdoc />
      public override String ToString()
      {
         return this._assemblyInfo.ToString( true, this.Flags.IsFullPublicKey() );
      }

      /// <summary>
      /// Tries to parse given textual assembly name and throws <see cref="FormatException"/> if parsing is unsuccessful.
      /// </summary>
      /// <param name="textualAssemblyName">The textual assembly name.</param>
      /// <returns>An instance <see cref="CILAssemblyName"/> with parsed components.</returns>
      /// <exception cref="FormatException">If <paramref name="textualAssemblyName"/> is not a valid assembly name as whole.</exception>
      /// <remarks>
      /// The <see cref="System.Reflection.AssemblyName(String)"/> constructor apparently requires that the assembly of the referenced name actually exists and will try to load it.
      /// Because of this, this method implements pure parsing of assembly name, without caring whether it actually exists or not.
      /// The <see href="http://msdn.microsoft.com/en-us/library/yfsftwz6%28v=vs.110%29.aspx">Specifying Fully Qualified Type Names</see> resource at MSDN provides information about textual assembly names.
      /// </remarks>
      public static CILAssemblyName Parse( String textualAssemblyName )
      {
         CILAssemblyName an;
         if ( TryParse( textualAssemblyName, out an ) )
         {
            return an;
         }
         else
         {
            throw new FormatException( "The string " + textualAssemblyName + " does not represent a CIL assembly name." );
         }
      }

      /// <summary>
      /// Tries to parse textual name of the assembly into a <see cref="CILAssemblyName"/>.
      /// </summary>
      /// <param name="textualAssemblyName">The textual assembly name.</param>
      /// <param name="assemblyName">If <paramref name="textualAssemblyName"/> is <c>null</c>, this will be <c>null</c>. Otherwise, this will hold a new instance of <see cref="CILAssemblyName"/> with any successfully parsed components.</param>
      /// <returns><c>true</c> if <paramref name="textualAssemblyName"/> was successfully parsed till the end; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// The <see cref="System.Reflection.AssemblyName(String)"/> constructor apparently requires that the assembly of the referenced name actually exists and will try to load it.
      /// Because of this, this method implements pure parsing of assembly name, without caring whether it actually exists or not.
      /// The <see href="http://msdn.microsoft.com/en-us/library/yfsftwz6%28v=vs.110%29.aspx">Specifying Fully Qualified Type Names</see> resource at MSDN provides information about textual assembly names.
      /// </remarks>
      public static Boolean TryParse( String textualAssemblyName, out CILAssemblyName assemblyName )
      {
         AssemblyInformation info; Boolean wasFullPK;
         var retVal = AssemblyInformation.TryParse( textualAssemblyName, out info, out wasFullPK );
         assemblyName = retVal ?
            new CILAssemblyName( info, wasFullPK ) :
            null;
         return retVal;
      }

   }
}

/// <summary>
/// This class contains extension methods for elements in <see cref="CILAssemblyManipulator.Logical"/> namespace.
/// </summary>
public static partial class E_CILLogical
{
   /// <summary>
   /// Gets all defined types in <paramref name="assembly"/>, including nested types.
   /// </summary>
   /// <param name="assembly">The <see cref="CILAssembly"/> to get types from.</param>
   /// <returns>Enumerable for all defined types in <paramref name="assembly"/>.</returns>
   public static IEnumerable<CILType> GetAllDefinedTypes( this CILAssembly assembly )
   {
      return assembly.Modules.SelectMany( m => m.DefinedTypes ).SelectMany( t => t.AsBreadthFirstEnumerable( tt => tt.DeclaredNestedTypes ) );
   }

   internal const String VERSION_NUMBER_SEPARATOR = ".";

   /// <summary>
   /// Returns <c>true</c> if <see cref="CILElementWithSimpleName.Name"/>, <see cref="CILAssemblyName.Culture"/>, <see cref="CILAssemblyName.MajorVersion"/>, <see cref="CILAssemblyName.MinorVersion"/>, <see cref="CILAssemblyName.BuildNumber"/> and <see cref="CILAssemblyName.Revision"/> all match for both <paramref name="thisName"/> and <paramref name="other"/>.
   /// </summary>
   /// <param name="thisName">First assembly name.</param>
   /// <param name="other">Second assembly name.</param>
   /// <returns><c>true</c> if <see cref="CILElementWithSimpleName.Name"/>, <see cref="CILAssemblyName.Culture"/>, <see cref="CILAssemblyName.MajorVersion"/>, <see cref="CILAssemblyName.MinorVersion"/>, <see cref="CILAssemblyName.BuildNumber"/> and <see cref="CILAssemblyName.Revision"/> all match for both <paramref name="thisName"/> and <paramref name="other"/>; <c>false</c> otherwise.</returns>
   public static Boolean CorePropertiesEqual( this CILAssemblyName thisName, CILAssemblyName other )
   {
      return Object.ReferenceEquals( thisName, other ) || (
         thisName != null
         && other != null
         && Object.Equals( thisName.Name, other.Name )
         && thisName.MajorVersion == other.MajorVersion
         && thisName.MinorVersion == other.MinorVersion
         && thisName.BuildNumber == other.BuildNumber
         && thisName.Revision == other.Revision
         && Object.Equals( thisName.Culture, other.Culture ) );
   }

   private static readonly Byte[] DOT_NET_PUBLIC_KEY_BYTES = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
   private static readonly Byte[] DOT_NET_PUBLIC_KEY_TOKEN_BYTES = { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 };
   /// <summary>
   /// Checks whether given <see cref="CILAssemblyName"/> represents assembly from .NET framework based on its public key.
   /// </summary>
   /// <param name="aName">The <see cref="CILAssemblyName"/>.</param>
   /// <returns><c>true</c> if <paramref name="aName"/> is not <c>null</c> and its public key matches the standard public key.</returns>
   /// <remarks>
   /// ECMA-335 (p. 116) defines the standard public key to be the following 16 bytes <c>00 00 00 00 00 00 00 00 04 00 00 00 00 00 00 00</c>.
   /// The resulting public key token is the following 8 bytes: <c>b7 7a 5c 56 19 34 e0 89</c>.
   /// </remarks>
   public static Boolean HasStandardPublicKey( this CILAssemblyName aName )
   {
      return aName != null
         && aName.PublicKey != null
         && ( ( aName.Flags.IsFullPublicKey() && aName.PublicKey.Length == DOT_NET_PUBLIC_KEY_BYTES.Length && DOT_NET_PUBLIC_KEY_BYTES.SequenceEqual( aName.PublicKey ) )
             || ( !aName.Flags.IsFullPublicKey() && aName.PublicKey.Length == DOT_NET_PUBLIC_KEY_TOKEN_BYTES.Length && DOT_NET_PUBLIC_KEY_TOKEN_BYTES.SequenceEqual( aName.PublicKey ) )
            );
   }

   ///// <summary>
   ///// This extension method gets the public key token of the given <see cref="CILAssembly"/>.
   ///// If the <see cref="CILAssemblyName.Flags"/> specify that the assembly name is not using full public key, then the <see cref="CILAssemblyName.PublicKey"/> is returned directly.
   ///// Otherwise, the public key token is computed by using <see cref="CILReflectionContext.ComputePublicKeyToken"/> method.
   ///// </summary>
   ///// <param name="assembly">The <see cref="CILAssembly"/>.</param>
   ///// <returns>The public key token of the given <see cref="CILAssembly"/>.</returns>
   //public static Byte[] GetPublicKeyToken( this CILAssembly assembly )
   //{
   //   var an = assembly.Name;
   //   var result = an.PublicKey;
   //   return an.Flags.IsFullPublicKey() && result != null && result.Length > 0 ?
   //      assembly.ReflectionContext.ComputePublicKeyToken( result ) :
   //      result;
   //}

   ///// <summary>
   ///// Tries to get PCL profile based on <see cref="TargetFrameworkAttribute"/> applied on <see cref="CILAssembly"/>.
   ///// </summary>
   ///// <param name="assembly">The <see cref="CILAssembly"/>.</param>
   ///// <param name="fwName">This will contain the framework name, if attribute is successfully found, otherwise <c>null</c>.</param>
   ///// <param name="fwVersion">This will contain the framework version, if attribute is successfully found, otherwise <c>null</c>.</param>
   ///// <param name="fwProfile">This will contain the framework profile, if attribute is successfully found, otherwise <c>null</c>.</param>
   ///// <remarks>
   ///// This method works by detecting an custom attribute of type <c>System.Runtime.Versioning.TargetFrameworkAttribute</c> which contains one constructor argument of type <see cref="String"/>.
   ///// Then the method tries to detect to parse the required information by assuming the string is in format <c>&lt;fwName&gt;,Version=&lt;fwVersion&gt;,Profile=&lt;fwProfile&gt;</c>, where version and profile information are optional.
   ///// </remarks>
   //public static void TryGetTargetFrameworkInfoBasedOnAttribute( this CILAssembly assembly, out String fwName, out String fwVersion, out String fwProfile )
   //{
   //   fwName = null;
   //   fwVersion = null;
   //   fwProfile = null;
   //   if ( assembly != null )
   //   {
   //      foreach ( var cd in assembly.CustomAttributeData )
   //      {
   //         if ( String.Equals( cd.Constructor.DeclaringType.Name, "TargetFrameworkAttribute" )
   //         && String.Equals( cd.Constructor.DeclaringType.Namespace, "System.Runtime.Versioning" )
   //         && cd.ConstructorArguments.Count == 1
   //         && cd.ConstructorArguments[0].Value is String )
   //         {
   //            break;
   //         }
   //      }
   //   }
   //}

   ///// <summary>
   ///// Adds a <see cref="TargetFrameworkAttribute"/> to the <see cref="CILAssembly"/> which represents information for given target framework.
   ///// </summary>
   ///// <param name="assembly">The <see cref="CILAssembly"/>.</param>
   ///// <param name="monikerInfo">The information about target framework, see <see cref="FrameworkMonikerInfo"/>.</param>
   ///// <param name="monikerMapper">The assembly mapper helper, created by <see cref="E_CIL.CreateMapperForFrameworkMoniker"/> method, or by creating <see cref="EmittingArguments"/> by <see cref="EmittingArguments.CreateForEmittingWithMoniker"/> method and accessing its <see cref="EmittingArguments.AssemblyMapper"/> property.</param>
   ///// <exception cref="NullReferenceException">If <paramref name="assembly"/> is <c>null</c>.</exception>
   ///// <exception cref="ArgumentNullException">If <paramref name="monikerInfo"/> or <paramref name="monikerMapper"/> is <c>null</c>.</exception>
   //public static void AddTargetFrameworkAttributeWithMonikerInfo( this CILAssembly assembly, FrameworkMonikerInfo monikerInfo, EmittingAssemblyMapper monikerMapper )
   //{
   //   ArgumentValidator.ValidateNotNull( "Moniker info", monikerInfo );
   //   ArgumentValidator.ValidateNotNull( "Moniker mapper", monikerMapper );
   //   var targetFWType = (CILType) monikerMapper.MapTypeBase( ModuleReader.TARGET_FRAMEWORK_ATTRIBUTE_CTOR.DeclaringType.NewWrapper( assembly.ReflectionContext ) );
   //   var targetFWCtor = targetFWType.Constructors.First( ctor => ctor.Parameters.Count == 1 && ctor.Parameters[0].ParameterType is CILType && ( (CILType) ctor.Parameters[0].ParameterType ).TypeCode == CILTypeCode.String );
   //   var targetFWProp = targetFWType.DeclaredProperties.First( prop => prop.Name == ModuleReader.TARGET_FRAMEWORK_ATTRIBUTE_NAMED_PROPERTY.Name );
   //   var fwString = monikerInfo.FrameworkName + ",Version=" + monikerInfo.FrameworkVersion;
   //   if ( !String.IsNullOrEmpty( monikerInfo.ProfileName ) )
   //   {
   //      fwString += ",Profile=" + monikerInfo.ProfileName;
   //   }
   //   assembly.AddCustomAttribute(
   //      targetFWCtor,
   //      new CILCustomAttributeTypedArgument[] { CILCustomAttributeFactory.NewTypedArgument( (CILType) targetFWCtor.Parameters[0].ParameterType, fwString ) },
   //      new CILCustomAttributeNamedArgument[] { CILCustomAttributeFactory.NewNamedArgument( targetFWProp, CILCustomAttributeFactory.NewTypedArgument( (CILType) targetFWProp.GetPropertyType(), monikerInfo.FrameworkDisplayName ) ) }
   //   );
   //}

   ///// <summary>
   ///// Adds a <see cref="TargetFrameworkAttribute"/> to the <see cref="CILAssembly"/> which has given framework information.
   ///// This method should not be used when adding <see cref="TargetFrameworkAttribute"/> to Portable Class Libraries.
   ///// </summary>
   ///// <param name="assembly">The <see cref="CILAssembly"/>.</param>
   ///// <param name="fwName">The name of the target framework. This will be passed directly as string argument to the attribute constructor.</param>
   ///// <param name="fwDisplayName">The display name of the target framework.</param>
   ///// <exception cref="NullReferenceException">If <paramref name="assembly"/> is <c>null</c>.</exception>
   //public static void AddTargetFrameworkAttributeNative( this CILAssembly assembly, String fwName, String fwDisplayName )
   //{
   //   assembly.AddCustomAttribute(
   //      ModuleReader.TARGET_FRAMEWORK_ATTRIBUTE_CTOR.NewWrapper( assembly.ReflectionContext ),
   //      new CILCustomAttributeTypedArgument[] { CILCustomAttributeFactory.NewTypedArgument( ModuleReader.TARGET_FRAMEWORK_ATTRIBUTE_CTOR.GetParameters().First().ParameterType.NewWrapperAsType( assembly.ReflectionContext ), fwName ) },
   //      fwDisplayName == null ? null : new CILCustomAttributeNamedArgument[] { CILCustomAttributeFactory.NewNamedArgument( ModuleReader.TARGET_FRAMEWORK_ATTRIBUTE_NAMED_PROPERTY.NewWrapper( assembly.ReflectionContext ), CILCustomAttributeFactory.NewTypedArgument( (CILType) ModuleReader.TARGET_FRAMEWORK_ATTRIBUTE_NAMED_PROPERTY.NewWrapper( assembly.ReflectionContext ).GetPropertyType(), fwDisplayName ) ) }
   //    );
   //}

   /// <summary>
   /// Helper method to try get <see cref="TypeForwardingInfo"/> based on type name and type namespace from <see cref="CILAssembly"/>.
   /// </summary>
   /// <param name="assembly">The <see cref="CILAssembly"/>.</param>
   /// <param name="typeName">The type name.</param>
   /// <param name="typeNamespace">The type namespace.</param>
   /// <param name="tfInfo">The resulting <see cref="TypeForwardingInfo"/>.</param>
   /// <returns><c>true</c> if <paramref name="assembly"/> contained a <see cref="TypeForwardingInfo"/> with given <paramref name="typeName"/> and <paramref name="typeNamespace"/> in its <see cref="CILAssembly.ForwardedTypeInfos"/> property; <c>false</c> otherwise.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="assembly"/> is <c>null</c>.</exception>
   public static Boolean TryGetTypeForwarder( this CILAssembly assembly, String typeName, String typeNamespace, out TypeForwardingInfo tfInfo )
   {
      return assembly.ForwardedTypeInfos.TryGetValue( Tuple.Create( typeName, typeNamespace ), out tfInfo );
   }
}