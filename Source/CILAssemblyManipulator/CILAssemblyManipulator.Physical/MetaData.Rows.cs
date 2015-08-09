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
using System.Linq;
using System.Text;
using CILAssemblyManipulator.Physical;
using CommonUtils;

namespace CILAssemblyManipulator.Physical
{
   public sealed class ModuleDefinition
   {
      public Int16 Generation { get; set; }
      public String Name { get; set; }
      public Guid? ModuleGUID { get; set; }
      public Guid? EditAndContinueGUID { get; set; }
      public Guid? EditAndContinueBaseGUID { get; set; }
   }

   public sealed class TypeReference
   {
      public TableIndex? ResolutionScope { get; set; }
      public String Name { get; set; }
      public String Namespace { get; set; }
   }

   public sealed class TypeDefinition
   {
      public TypeDefinition()
      {
         this.FieldList = new TableIndex( Tables.Field, 0 );
         this.MethodList = new TableIndex( Tables.MethodDef, 0 );
      }

      public TypeAttributes Attributes { get; set; }
      public String Name { get; set; }
      public String Namespace { get; set; }
      public TableIndex? BaseType { get; set; }
      public TableIndex FieldList { get; set; }
      public TableIndex MethodList { get; set; }
   }

   public sealed class FieldDefinition
   {
      public FieldAttributes Attributes { get; set; }
      public String Name { get; set; }
      public FieldSignature Signature { get; set; }
   }

   public sealed class MethodDefinition
   {
      public MethodDefinition()
      {
         this.ParameterList = new TableIndex( Tables.Parameter, 0 );
      }

      public MethodILDefinition IL { get; set; }
      public MethodImplAttributes ImplementationAttributes { get; set; }
      public MethodAttributes Attributes { get; set; }
      public String Name { get; set; }
      public MethodDefinitionSignature Signature { get; set; }
      public TableIndex ParameterList { get; set; }
   }

   public sealed class ParameterDefinition
   {
      public ParameterAttributes Attributes { get; set; }
      public Int32 Sequence { get; set; }
      public String Name { get; set; }
   }

   public sealed class InterfaceImplementation
   {
      public InterfaceImplementation()
      {
         this.Class = new TableIndex( Tables.TypeDef, 0 );
      }

      public TableIndex Class { get; set; }
      public TableIndex Interface { get; set; }
   }

   public sealed class MemberReference
   {
      public TableIndex DeclaringType { get; set; }
      public String Name { get; set; }
      public AbstractSignature Signature { get; set; }
   }

   public sealed class ConstantDefinition
   {
      public SignatureElementTypes Type { get; set; }
      public TableIndex Parent { get; set; }
      public Object Value { get; set; }
   }

   public sealed class CustomAttributeDefinition
   {
      public TableIndex Parent { get; set; }
      public TableIndex Type { get; set; }
      public AbstractCustomAttributeSignature Signature { get; set; }
   }

   public sealed class FieldMarshal
   {
      public TableIndex Parent { get; set; }
      public MarshalingInfo NativeType { get; set; }
   }

   public sealed class SecurityDefinition
   {
      private readonly List<AbstractSecurityInformation> _permissionSets;

      public SecurityDefinition( Int32 permissionSetsCount = 0 )
      {
         this._permissionSets = new List<AbstractSecurityInformation>( permissionSetsCount );
      }

      /// <summary>
      /// Gets or sets the <see cref="SecurityAction"/> associated with this security attribute declaration.
      /// </summary>
      /// <value>The <see cref="SecurityAction"/> associated with this security attribute declaration.</value>
      public SecurityAction Action { get; set; }

      public TableIndex Parent { get; set; }

      public List<AbstractSecurityInformation> PermissionSets
      {
         get
         {
            return this._permissionSets;
         }
      }
   }

   public sealed class ClassLayout
   {
      public ClassLayout()
      {
         this.Parent = new TableIndex( Tables.TypeDef, 0 );
      }

      public Int32 PackingSize { get; set; }
      public Int32 ClassSize { get; set; }
      public TableIndex Parent { get; set; }
   }

   public sealed class FieldLayout
   {
      public FieldLayout()
      {
         this.Field = new TableIndex( Tables.Field, 0 );
      }

      public Int32 Offset { get; set; }
      public TableIndex Field { get; set; }
   }

   public sealed class StandaloneSignature
   {
      public AbstractSignature Signature { get; set; }

      // From https://social.msdn.microsoft.com/Forums/en-US/b4252eab-7aae-4456-9829-2707c8459e13/pinned-fields-in-the-common-language-runtime?forum=netfxtoolsdev
      // After messing around further, and noticing that even the C# compiler emits Field signatures in the StandAloneSig table, the signatures seem to relate to PDB debugging symbols.
      // When you emit symbols with the Debug or Release versions of your code, I'm guessing a StandAloneSig entry is injected and referred to by the PDB file.
      // If you are in release mode and you generate no PDB info, the StandAloneSig table contains no Field signatures.
      // One such condition for the emission of such information is constants within the scope of a method body.
      // Original thread:  http://www.netframeworkdev.com/building-development-diagnostic-tools-for-net/field-signatures-in-standalonesig-table-30658.shtml
      public Boolean StoreSignatureAsFieldSignature { get; set; }

   }

   public sealed class EventMap
   {
      public EventMap()
      {
         this.Parent = new TableIndex( Tables.TypeDef, 0 );
         this.EventList = new TableIndex( Tables.Event, 0 );
      }

      public TableIndex Parent { get; set; }
      public TableIndex EventList { get; set; }
   }

   public sealed class EventDefinition
   {
      public EventAttributes Attributes { get; set; }
      public String Name { get; set; }
      public TableIndex EventType { get; set; }
   }

   public sealed class PropertyMap
   {
      public PropertyMap()
      {
         this.Parent = new TableIndex( Tables.TypeDef, 0 );
         this.PropertyList = new TableIndex( Tables.Property, 0 );
      }

      public TableIndex Parent { get; set; }
      public TableIndex PropertyList { get; set; }
   }

   public sealed class PropertyDefinition
   {
      public PropertyAttributes Attributes { get; set; }
      public String Name { get; set; }
      public PropertySignature Signature { get; set; }
   }

   public sealed class MethodSemantics
   {
      public MethodSemantics()
      {
         this.Method = new TableIndex( Tables.MethodDef, 0 );
      }

      public MethodSemanticsAttributes Attributes { get; set; }
      public TableIndex Method { get; set; }
      public TableIndex Associaton { get; set; }
   }

   public sealed class MethodImplementation
   {
      public MethodImplementation()
      {
         this.Class = new TableIndex( Tables.TypeDef, 0 );
      }

      public TableIndex Class { get; set; }
      public TableIndex MethodBody { get; set; }
      public TableIndex MethodDeclaration { get; set; }
   }

   public sealed class ModuleReference
   {
      public String ModuleName { get; set; }
   }

   public sealed class TypeSpecification
   {
      public TypeSignature Signature { get; set; }
   }

   public sealed class MethodImplementationMap
   {
      public MethodImplementationMap()
      {
         this.ImportScope = new TableIndex( Tables.ModuleRef, 0 );
      }

      public PInvokeAttributes Attributes { get; set; }
      public TableIndex MemberForwarded { get; set; }
      public String ImportName { get; set; }
      public TableIndex ImportScope { get; set; }
   }

   public sealed class FieldRVA
   {
      public FieldRVA()
      {
         this.Field = new TableIndex( Tables.Field, 0 );
      }

      public Byte[] Data { get; set; }
      public TableIndex Field { get; set; }
   }

   public sealed class AssemblyDefinition
   {
      private readonly AssemblyInformation _assemblyInfo;

      public AssemblyDefinition()
      {
         this._assemblyInfo = new AssemblyInformation();
      }

      public AssemblyFlags Attributes { get; set; }

      public AssemblyInformation AssemblyInformation
      {
         get
         {
            return this._assemblyInfo;
         }
      }

      public AssemblyHashAlgorithm HashAlgorithm { get; set; }

      public override String ToString()
      {
         return this.AssemblyInformation.ToString( true, true );
      }
   }

   public sealed class AssemblyReference
   {
      private readonly AssemblyInformation _assemblyInfo;

      public AssemblyReference()
      {
         this._assemblyInfo = new AssemblyInformation();
      }

      public AssemblyFlags Attributes { get; set; }

      public AssemblyInformation AssemblyInformation
      {
         get
         {
            return this._assemblyInfo;
         }
      }

      public Byte[] HashValue { get; set; }

      public override String ToString()
      {
         return this.AssemblyInformation.ToString( true, this.Attributes.IsFullPublicKey() );
      }
   }

   public sealed class FileReference
   {
      public FileAttributes Attributes { get; set; }
      public String Name { get; set; }
      public Byte[] HashValue { get; set; }
   }

   public sealed class ExportedType
   {
      public TypeAttributes Attributes { get; set; }
      public Int32 TypeDefinitionIndex { get; set; }
      public String Name { get; set; }
      public String Namespace { get; set; }
      public TableIndex Implementation { get; set; }
   }

   public sealed class ManifestResource
   {
      /// <summary>
      /// This value is interpreted as unsigned 4-byte integer.
      /// </summary>
      public Int32 Offset { get; set; }
      public ManifestResourceAttributes Attributes { get; set; }
      public String Name { get; set; }
      public TableIndex? Implementation { get; set; }

      // This will be used only if Implementation is null
      public Byte[] DataInCurrentFile { get; set; }
   }

   public sealed class NestedClassDefinition
   {
      public NestedClassDefinition()
      {
         this.NestedClass = new TableIndex( Tables.TypeDef, 0 );
         this.EnclosingClass = new TableIndex( Tables.TypeDef, 0 );
      }

      public TableIndex NestedClass { get; set; }
      public TableIndex EnclosingClass { get; set; }
   }

   public sealed class GenericParameterDefinition
   {
      public Int32 GenericParameterIndex { get; set; }
      public GenericParameterAttributes Attributes { get; set; }
      public TableIndex Owner { get; set; }
      public String Name { get; set; }
   }

   public sealed class MethodSpecification
   {
      public TableIndex Method { get; set; }
      public GenericMethodSignature Signature { get; set; }
   }

   public sealed class GenericParameterConstraintDefinition
   {
      public GenericParameterConstraintDefinition()
      {
         this.Owner = new TableIndex( Tables.GenericParameter, 0 );
      }

      public TableIndex Owner { get; set; }
      public TableIndex Constraint { get; set; }
   }

   public struct TableIndex : IEquatable<TableIndex>, IComparable<TableIndex>, IComparable
   {
      private const Int32 INDEX_MASK = 0x00FFFFF;

      private const Int32 TYPE_DEF = 0;
      private const Int32 TYPE_REF = 1;
      private const Int32 TYPE_SPEC = 2;
      private const Int32 TDRS_TABLE_EXTRACT_MASK = 0x3;
      private const Int32 TYPE_DEF_MASK = ( (Byte) Tables.TypeDef ) << 24; // 0x2000000;
      private const Int32 TYPE_REF_MASK = ( (Byte) Tables.TypeRef ) << 24; // 0x1000000;
      private const Int32 TYPE_SPEC_MASK = ( (Byte) Tables.TypeSpec ) << 24; // 0x1B000000;

      private readonly Int32 _token;

      // index is zero-based
      public TableIndex( Tables aTable, Int32 anIdx )
         : this( ( (Int32) aTable << 24 ) | anIdx )
      {
      }

      internal TableIndex( Int32 token )
      {
         this._token = token;
      }

      public Tables Table
      {
         get
         {
            return (Tables) ( this._token >> 24 );
         }
      }

      /// <summary>
      /// This index is zero-based.
      /// </summary>
      public Int32 Index
      {
         get
         {
            return this._token & INDEX_MASK;
         }
      }

      public Int32 ZeroBasedToken
      {
         get
         {
            return this._token;
         }
      }

      public Int32 OneBasedToken
      {
         get
         {
            return ( ( this._token & INDEX_MASK ) + 1 ) | ( this._token & ~INDEX_MASK );
         }
      }

      public override Boolean Equals( Object obj )
      {
         return obj is TableIndex && this.Equals( (TableIndex) obj );
      }

      public override Int32 GetHashCode()
      {
         return this._token;
      }

      public Boolean Equals( TableIndex other )
      {
         return this._token == other._token;
      }

      public override String ToString()
      {
         return this.Table + "[" + this.Index + "]";
      }

      public Int32 CompareTo( TableIndex other )
      {
         var retVal = this.Table.CompareTo( other.Table );
         if ( retVal == 0 )
         {
            retVal = this.Index.CompareTo( other.Index );
         }

         return retVal;
      }

      internal Int32 CompareTo( TableIndex other, Int32[] tableOrderArray )
      {
         var retVal = this.Index.CompareTo( other.Index );
         if ( retVal == 0 )
         {
            retVal = tableOrderArray[(Int32) this.Table].CompareTo( tableOrderArray[(Int32) other.Table] );
         }
         return retVal;
      }

      Int32 IComparable.CompareTo( Object obj )
      {
         if ( obj == null )
         {
            // This is always 'greater' than null
            return 1;
         }
         else if ( obj is TableIndex )
         {
            return this.CompareTo( (TableIndex) obj );
         }
         else
         {
            throw new ArgumentException( "Given object must be of type " + this.GetType() + " or null." );
         }
      }

      public static Boolean operator ==( TableIndex x, TableIndex y )
      {
         return x.Equals( y );
      }

      public static Boolean operator !=( TableIndex x, TableIndex y )
      {
         return !( x == y );
      }

      public static Boolean operator <( TableIndex x, TableIndex y )
      {
         return x.CompareTo( y ) < 0;
      }

      public static Boolean operator >( TableIndex x, TableIndex y )
      {
         return x.CompareTo( y ) > 0;
      }

      public static Boolean operator <=( TableIndex x, TableIndex y )
      {
         return !( x > y );
      }

      public static Boolean operator >=( TableIndex x, TableIndex y )
      {
         return !( x < y );
      }

      public static TableIndex? FromOneBasedTokenNullable( Int32 token )
      {
         return token == 0 ?
            (TableIndex?) null :
            FromOneBasedToken( token );
      }

      public static TableIndex FromOneBasedToken( Int32 token )
      {
         return new TableIndex( ( ( token & INDEX_MASK ) - 1 ) | ( token & ~INDEX_MASK ) );
      }

      public static TableIndex FromZeroBasedToken( Int32 token )
      {
         return new TableIndex( token );
      }



      internal static Int32 DecodeTypeDefOrRefOrSpec( Byte[] array, ref Int32 offset )
      {
         var decodedValue = array.DecompressUInt32( ref offset );
         switch ( decodedValue & TDRS_TABLE_EXTRACT_MASK )
         {
            case TYPE_DEF:
               decodedValue = TYPE_DEF_MASK | ( decodedValue >> 2 );
               break;
            case TYPE_REF:
               decodedValue = TYPE_REF_MASK | ( decodedValue >> 2 );
               break;
            case TYPE_SPEC:
               decodedValue = TYPE_SPEC_MASK | ( decodedValue >> 2 );
               break;
            default:
               throw new ArgumentException( "Token table resolved to not supported: " + (Tables) ( decodedValue & TDRS_TABLE_EXTRACT_MASK ) + "." );
         }
         return decodedValue;
      }

      internal static Int32 EncodeTypeDefOrRefOrSpec( Int32 token )
      {
         Int32 encodedValue;
         switch ( unchecked( (UInt32) token ) >> 24 )
         {
            case (UInt32) Tables.TypeDef:
               encodedValue = ( ( INDEX_MASK & token ) << 2 ) | TYPE_DEF;
               break;
            case (UInt32) Tables.TypeRef:
               encodedValue = ( ( INDEX_MASK & token ) << 2 ) | TYPE_REF;
               break;
            case (UInt32) Tables.TypeSpec:
               encodedValue = ( ( INDEX_MASK & token ) << 2 ) | TYPE_SPEC;
               break;
            default:
               throw new ArgumentException( "Token must reference one of the following tables: " + String.Join( ", ", Tables.TypeDef, Tables.TypeRef, Tables.TypeSpec ) + "." );
         }
         return encodedValue;
      }
   }

   public enum Tables : byte
   {
      Assembly = 0x20,
      AssemblyOS = 0x22,
      AssemblyProcessor = 0x21,
      AssemblyRef = 0x23,
      AssemblyRefOS = 0x25,
      AssemblyRefProcessor = 0x24,
      ClassLayout = 0x0F,
      Constant = 0x0B,
      CustomAttribute = 0x0C,
      DeclSecurity = 0x0E,
      EncLog = 0x1E,
      EncMap = 0x1F,
      EventMap = 0x12,
      Event = 0x14,
      EventPtr = 0x13,
      ExportedType = 0x27,
      Field = 0x04,
      FieldLayout = 0x10,
      FieldMarshal = 0x0D,
      FieldPtr = 0x03,
      FieldRVA = 0x1D,
      File = 0x26,
      GenericParameter = 0x2A,
      GenericParameterConstraint = 0x2C,
      ImplMap = 0x1C,
      InterfaceImpl = 0x09,
      ManifestResource = 0x28,
      MemberRef = 0x0A,
      MethodDef = 0x06,
      MethodImpl = 0x19,
      MethodPtr = 0x05,
      MethodSemantics = 0x18,
      MethodSpec = 0x2B,
      Module = 0x00,
      ModuleRef = 0x1A,
      NestedClass = 0x29,
      Parameter = 0x08,
      ParameterPtr = 0x07,
      Property = 0x17,
      PropertyPtr = 0x16,
      PropertyMap = 0x15,
      StandaloneSignature = 0x11,
      TypeDef = 0x02,
      TypeRef = 0x01,
      TypeSpec = 0x1B
   }

   // System.Runtime.Versioning.FrameworkName is amazingly missing from all PCL framework assemblies.
   public sealed class TargetFrameworkInfo : IEquatable<TargetFrameworkInfo>
   {
      private readonly String _fwName;
      private readonly String _fwVersion;
      private readonly String _fwProfile;
      private readonly Boolean _assemblyRefsRetargetable;

      public TargetFrameworkInfo( String name, String version, String profile )
      {
         this._fwName = name;
         this._fwVersion = version;
         this._fwProfile = profile;
         // TODO better
         this._assemblyRefsRetargetable = String.Equals( this._fwName, ".NETPortable" );
      }

      public String Identifier
      {
         get
         {
            return this._fwName;
         }
      }

      public String Version
      {
         get
         {
            return this._fwVersion;
         }
      }

      public String Profile
      {
         get
         {
            return this._fwProfile;
         }
      }

      public Boolean AreFrameworkAssemblyReferencesRetargetable
      {
         get
         {
            return this._assemblyRefsRetargetable;
         }
      }

      public override Boolean Equals( Object obj )
      {
         return this.Equals( obj as TargetFrameworkInfo );
      }

      public override Int32 GetHashCode()
      {
         return ( ( 17 * 23 + this._fwName.GetHashCodeSafe() ) * 23 + this._fwVersion.GetHashCodeSafe() ) * 23 + this._fwProfile.GetHashCodeSafe();
      }

      public override String ToString()
      {
         var retVal = this._fwName + SEPARATOR + VERSION_PREFIX + this._fwVersion;
         if ( !String.IsNullOrEmpty( this._fwProfile ) )
         {
            retVal += SEPARATOR + PROFILE_PREFIX + this._fwProfile;
         }
         return retVal;
      }

      public Boolean Equals( TargetFrameworkInfo other )
      {
         return ReferenceEquals( this, other )
            || ( other != null
            && String.Equals( this._fwName, other._fwName )
            && String.Equals( this._fwVersion, other._fwVersion )
            && String.Equals( this._fwProfile, other._fwProfile )
            && this._assemblyRefsRetargetable == other._assemblyRefsRetargetable
            );
      }


      private const String PROFILE_PREFIX = "Profile=";
      private const String VERSION_PREFIX = "Version=";
      private const Char SEPARATOR = ',';

      public static Boolean TryParse( String str, out TargetFrameworkInfo fwInfo )
      {
         var retVal = !String.IsNullOrEmpty( str );
         if ( retVal )
         {
            // First, framework name
            var idx = str.IndexOf( SEPARATOR );
            var fwName = idx == -1 ? str : str.Substring( 0, idx );

            String fwVersion = null, fwProfile = null;
            if ( idx > 0 )
            {

               // Then, framework version
               idx = str.IndexOf( VERSION_PREFIX, idx, StringComparison.Ordinal );
               var nextIdx = idx + VERSION_PREFIX.Length;
               var endIdx = str.IndexOf( SEPARATOR, nextIdx );
               if ( endIdx == -1 )
               {
                  endIdx = str.Length;
               }
               fwVersion = idx != -1 && nextIdx < str.Length ? str.Substring( nextIdx, endIdx - nextIdx ) : null;

               // Then, profile
               if ( idx > 0 )
               {
                  idx = str.IndexOf( PROFILE_PREFIX, idx, StringComparison.Ordinal );
                  nextIdx = idx + PROFILE_PREFIX.Length;
                  endIdx = str.IndexOf( SEPARATOR, nextIdx );
                  if ( endIdx == -1 )
                  {
                     endIdx = str.Length;
                  }
                  fwProfile = idx != -1 && nextIdx < str.Length ? str.Substring( nextIdx, endIdx - nextIdx ) : null;
               }
            }

            fwInfo = new TargetFrameworkInfo( fwName, fwVersion, fwProfile );
         }
         else
         {
            fwInfo = null;
         }

         return retVal;
      }
   }
}

public static partial class E_CILPhysical
{
   public static Boolean IsHasThis( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.HasThis ) != 0;
   }

   public static Boolean IsExplicitThis( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.ExplicitThis ) != 0;
   }

   public static Boolean IsDefault( this SignatureStarters starter )
   {
      return starter == 0;
   }

   public static Boolean IsVarArg( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.VarArgs ) != 0;
   }

   public static Boolean IsGeneric( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.Generic ) != 0;
   }

   public static Boolean IsProperty( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.Property ) != 0;
   }

   /// <summary>
   /// Checks whether the method is eligible to have method body. See ECMA specification (condition 33 for MethodDef table) for exact condition of methods having method bodies. In addition to that, the <see cref="E_CIL.IsIL"/> must return <c>true</c>.
   /// </summary>
   /// <param name="method">The method to check.</param>
   /// <returns><c>true</c> if the <paramref name="method"/> is non-<c>null</c> and can have IL method body; <c>false</c> otherwise.</returns>
   /// <seealso cref="E_CIL.IsIL"/>
   /// <seealso cref="E_CIL.CanEmitIL"/>
   public static Boolean ShouldHaveMethodBody( this MethodDefinition method )
   {
      return method != null && method.Attributes.CanEmitIL() && method.ImplementationAttributes.IsIL();
   }

   public static TableIndex ChangeIndex( this TableIndex index, Int32 newIndex )
   {
      return new TableIndex( index.Table, newIndex );
   }

   public static TableIndex IncrementIndex( this TableIndex index )
   {
      return index.ChangeIndex( index.Index + 1 );
   }

   public static Boolean IsSimpleTypeOfKind( this CustomAttributeArgumentType caType, SignatureElementTypes typeKind )
   {
      return caType.ArgumentTypeKind == CustomAttributeArgumentTypeKind.Simple
         && ( (CustomAttributeArgumentTypeSimple) caType ).SimpleType == typeKind;
   }

   public static Boolean CanBeReferencedFromIL( this Tables table )
   {
      switch ( table )
      {
         case Tables.TypeDef:
         case Tables.TypeRef:
         case Tables.TypeSpec:
         case Tables.MethodDef:
         case Tables.Field:
         case Tables.MemberRef:
         case Tables.MethodSpec:
         case Tables.StandaloneSignature:
            return true;
         default:
            return false;
      }
   }

   public static Boolean IsEmbeddedResource( this ManifestResource resource )
   {
      return !resource.Implementation.HasValue;
   }

   public static AssemblyReference AsAssemblyReference( this AssemblyDefinition definition )
   {
      var retVal = new AssemblyReference()
      {
         Attributes = definition.AssemblyInformation.PublicKeyOrToken.IsNullOrEmpty() ? AssemblyFlags.None : AssemblyFlags.PublicKey
      };

      definition.AssemblyInformation.DeepCopyContentsTo( retVal.AssemblyInformation );

      return retVal;

   }
}
