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
      public MethodILDefinition IL { get; set; }
      public MethodImplAttributes ImplementationAttributes { get; set; }
      public MethodAttributes Attributes { get; set; }
      public String Name { get; set; }
      public MethodDefinitionSignature Signature { get; set; }
      public TableIndex ParameterList { get; set; }
   }

   public sealed class MethodILDefinition
   {
      private readonly IList<MethodExceptionBlock> _exceptionBlocks;
      private readonly IList<OpCodeInfo> _opCodes;

      public MethodILDefinition( Int32 exceptionBlockCount = 0, Int32 opCodeCount = 0 )
      {
         this._exceptionBlocks = new List<MethodExceptionBlock>( exceptionBlockCount );
         this._opCodes = new List<OpCodeInfo>( opCodeCount );
      }

      public Boolean InitLocals { get; set; }
      public TableIndex? LocalsSignatureIndex { get; set; }

      public IList<MethodExceptionBlock> ExceptionBlocks
      {
         get
         {
            return this._exceptionBlocks;
         }
      }

      public IList<OpCodeInfo> OpCodes
      {
         get
         {
            return this._opCodes;
         }
      }
   }

   public sealed class MethodExceptionBlock
   {
      public ExceptionBlockType BlockType { get; set; }
      public Int32 TryOffset { get; set; }
      public Int32 TryLength { get; set; }
      public Int32 HandlerOffset { get; set; }
      public Int32 HandlerLength { get; set; }
      public TableIndex? ExceptionType { get; set; }
      public Int32 FilterOffset { get; set; }
   }

   public sealed class ParameterDefinition
   {
      public ParameterAttributes Attributes { get; set; }
      public Int32 Sequence { get; set; }
      public String Name { get; set; }
   }

   public sealed class InterfaceImplementation
   {
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
      private readonly IList<SecurityInformation> _permissionSets;

      public SecurityDefinition( Int32 permissionSetsCount = 0 )
      {
         this._permissionSets = new List<SecurityInformation>( permissionSetsCount );
      }

      /// <summary>
      /// Gets or sets the <see cref="SecurityAction"/> associated with this security attribute declaration.
      /// </summary>
      /// <value>The <see cref="SecurityAction"/> associated with this security attribute declaration.</value>
      public SecurityAction Action { get; set; }
      public TableIndex Parent { get; set; }
      public IList<SecurityInformation> PermissionSets
      {
         get
         {
            return this._permissionSets;
         }
      }
   }

   public sealed class ClassLayout
   {
      public Int16 PackingSize { get; set; }
      public Int32 ClassSize { get; set; }
      public TableIndex Parent { get; set; }
   }

   public sealed class FieldLayout
   {
      public Int32 Offset { get; set; }
      public TableIndex Field { get; set; }
   }

   public sealed class StandaloneSignature
   {
      public AbstractSignature Signature { get; set; }
   }

   public sealed class EventMap
   {
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
      public MethodSemanticsAttributes Attributes { get; set; }
      public TableIndex Method { get; set; }
      public TableIndex Associaton { get; set; }
   }

   public sealed class MethodImplementation
   {
      public TableIndex Class { get; set; }
      public TableIndex MethodBody { get; set; }
      public TableIndex MethodDeclaration { get; set; }
   }

   public sealed class ModuleReference
   {
      public String ModuleRefeference { get; set; }
   }

   public sealed class TypeSpecification
   {
      public TypeSignature Signature { get; set; }
   }

   public sealed class MethodImplementationMap
   {
      public PInvokeAttributes Attributes { get; set; }
      public TableIndex MemberForwarded { get; set; }
      public String ImportName { get; set; }
      public TableIndex ImportScope { get; set; }
   }

   public sealed class FieldRVA
   {
      public Byte[] Data { get; set; }
      public TableIndex Field { get; set; }
   }

   public sealed class AssemblyDefinition
   {
      public AssemblyFlags Attributes { get; set; }
      public AssemblyInformation AssemblyInformation { get; set; }
      public AssemblyHashAlgorithm HashAlgorithm { get; set; }
   }

   public sealed class AssemblyReference
   {
      public AssemblyFlags Attributes { get; set; }
      public AssemblyInformation AssemblyInformation { get; set; }
      public Byte[] HashValue { get; set; }
   }

   public sealed class AssemblyInformation
   {
      public Int32 VersionMajor { get; set; }
      public Int32 VersionMinor { get; set; }
      public Int32 VersionBuild { get; set; }
      public Int32 VersionRevision { get; set; }
      public Byte[] PublicKeyOrToken { get; set; }
      public String Name { get; set; }
      public String Culture { get; set; }

      private const Int32 NOT_FOUND = -1;

      private enum Elements
      {
         Version,
         Culture,
         PublicKey,
         PublicKeyToken,
         Other
      }

      private const Char ASSEMBLY_NAME_ELEMENTS_SEPARATOR = ',';
      private const Char ASSEMBLY_NAME_ELEMENT_VALUE_SEPARATOR = '=';
      private const Char VERSION_SEPARATOR = '.';
      private const String VERSION = "Version";
      private const String CULTURE = "Culture";
      private const String PUBLIC_KEY_TOKEN = "PublicKeyToken";
      private const String PUBLIC_KEY = "PublicKey";
      private const String NEUTRAL_CULTURE = "neutral";
      private const String NEUTRAL_CULTURE_NAME = "";

      public static AssemblyInformation Parse( String textualAssemblyName )
      {
         Boolean isFullPublicKey;
         return Parse( textualAssemblyName, out isFullPublicKey );
      }

      /// <summary>
      /// Tries to parse given textual assembly name and throws <see cref="FormatException"/> if parsing is unsuccessful.
      /// </summary>
      /// <param name="textualAssemblyName">The textual assembly name.</param>
      /// <returns>An instance <see cref="AssemblyInformation"/> with parsed components.</returns>
      /// <exception cref="FormatException">If <paramref name="textualAssemblyName"/> is not a valid assembly name as whole.</exception>
      /// <remarks>
      /// The <see cref="System.Reflection.AssemblyName(String)"/> constructor apparently requires that the assembly of the referenced name actually exists and will try to load it.
      /// Because of this, this method implements pure parsing of assembly name, without caring whether it actually exists or not.
      /// The <see href="http://msdn.microsoft.com/en-us/library/yfsftwz6%28v=vs.110%29.aspx">Specifying Fully Qualified Type Names</see> resource at MSDN provides information about textual assembly names.
      /// </remarks>
      public static AssemblyInformation Parse( String textualAssemblyName, out Boolean isFullPublicKey )
      {
         AssemblyInformation an;
         if ( TryParse( textualAssemblyName, out an, out isFullPublicKey ) )
         {
            return an;
         }
         else
         {
            throw new FormatException( "The string " + textualAssemblyName + " does not represent a CIL assembly name." );
         }
      }

      /// <summary>
      /// Tries to parse textual name of the assembly into a <see cref="AssemblyInformation"/>.
      /// </summary>
      /// <param name="textualAssemblyName">The textual assembly name.</param>
      /// <param name="assemblyName">If <paramref name="textualAssemblyName"/> is <c>null</c>, this will be <c>null</c>. Otherwise, this will hold a new instance of <see cref="CILAssemblyName"/> with any successfully parsed components.</param>
      /// <returns><c>true</c> if <paramref name="textualAssemblyName"/> was successfully parsed till the end; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// The <see cref="System.Reflection.AssemblyName(String)"/> constructor apparently requires that the assembly of the referenced name actually exists and will try to load it.
      /// Because of this, this method implements pure parsing of assembly name, without caring whether it actually exists or not.
      /// The <see href="http://msdn.microsoft.com/en-us/library/yfsftwz6%28v=vs.110%29.aspx">Specifying Fully Qualified Type Names</see> resource at MSDN provides information about textual assembly names.
      /// </remarks>
      public static Boolean TryParse( String textualAssemblyName, out AssemblyInformation assemblyName, out Boolean isFullPublicKey )
      {
         var success = !String.IsNullOrEmpty( textualAssemblyName );
         isFullPublicKey = false;
         if ( success )
         {
            assemblyName = new AssemblyInformation();

            // First, name
            var nameIdx = TryParseName( textualAssemblyName );
            // Name may contain escape characters
            assemblyName.Name = InternalExtensions.UnescapeSomeString( textualAssemblyName, 0, nameIdx );

            success = !String.IsNullOrEmpty( assemblyName.Name );
            if ( success )
            {

               // Then, other components. Other components shouldn't contain escaped characters.
               var publicKeyOrTokenEncountered = false;
               while ( success && nameIdx < textualAssemblyName.Length )
               {
                  success = textualAssemblyName[nameIdx] == ASSEMBLY_NAME_ELEMENTS_SEPARATOR;
                  if ( success )
                  {
                     // Skip following whitespaces
                     while ( ++nameIdx < textualAssemblyName.Length && Char.IsWhiteSpace( textualAssemblyName[nameIdx] ) ) ;

                     success = nameIdx < textualAssemblyName.Length;
                     if ( success )
                     {
                        // Find next separator
                        var aux = NextSeparatorIdx( textualAssemblyName, ASSEMBLY_NAME_ELEMENT_VALUE_SEPARATOR, nameIdx );
                        success = aux > 0 && aux < textualAssemblyName.Length - 1 - nameIdx;
                        if ( success )
                        {
                           var el = GetElement( textualAssemblyName, nameIdx, aux );
                           nameIdx += aux + 1;
                           switch ( el )
                           {
                              case Elements.Version:
                                 success = TryParseVersion( assemblyName, textualAssemblyName, ref nameIdx );
                                 break;
                              case Elements.Culture:
                                 success = TryParseCulture( assemblyName, textualAssemblyName, ref nameIdx );
                                 break;
                              case Elements.PublicKeyToken:
                                 if ( !publicKeyOrTokenEncountered )
                                 {
                                    publicKeyOrTokenEncountered = true;
                                    success = TryParsePublicKeyFullOrToken( assemblyName, textualAssemblyName, ref nameIdx );
                                 }
                                 break;
                              case Elements.PublicKey:
                                 if ( !publicKeyOrTokenEncountered )
                                 {
                                    publicKeyOrTokenEncountered = true;
                                    isFullPublicKey = true;

                                    success = TryParsePublicKeyFullOrToken( assemblyName, textualAssemblyName, ref nameIdx );
                                 }
                                 break;
                              default:
                                 success = false;
                                 break;
                           }
                        }
                     }
                  }
               }
               // Return true only if successfully parsed whole string till the end.
               success = success && nameIdx == textualAssemblyName.Length;
            }
         }
         else
         {
            assemblyName = null;
         }
         return success;
      }

      private static Int32 NextSeparatorIdx( String str, Char separator, Int32 startIdx = 0 )
      {
         var result = str.IndexOf( separator, startIdx );
         return ( result == NOT_FOUND ? str.Length : result ) - startIdx;
      }

      private static Elements GetElement( String str, Int32 idx, Int32 aux )
      {
         Elements result;
         if ( String.Compare( str, idx, VERSION, 0, aux, StringComparison.OrdinalIgnoreCase ) == 0 )
         {
            result = Elements.Version;
         }
         else if ( String.Compare( str, idx, CULTURE, 0, aux, StringComparison.OrdinalIgnoreCase ) == 0 )
         {
            result = Elements.Culture;
         }
         else if ( String.Compare( str, idx, PUBLIC_KEY_TOKEN, 0, aux, StringComparison.OrdinalIgnoreCase ) == 0 )
         {
            result = Elements.PublicKeyToken;
         }
         else if ( String.Compare( str, idx, PUBLIC_KEY, 0, aux, StringComparison.OrdinalIgnoreCase ) == 0 )
         {
            result = Elements.PublicKey;
         }
         else
         {
            result = Elements.Other;
         }
         return result;
      }

      private static Int32 TryParseName( String fullAssemblyName )
      {
         var nameIdx = 0;
         var dontMatch = false;
         while ( nameIdx < fullAssemblyName.Length && ( dontMatch || fullAssemblyName[nameIdx] != ASSEMBLY_NAME_ELEMENTS_SEPARATOR ) )
         {
            if ( !dontMatch && fullAssemblyName[nameIdx] == '\\' )
            {
               // The escaped character follows.
               dontMatch = true;
            }
            else if ( dontMatch )
            {
               // Previous character was escape character
               dontMatch = false;
            }
            ++nameIdx;
         }

         // dontMatch will be true if string ended with escape character but no actual character to escape followed.
         return dontMatch ? 0 : nameIdx;
      }

      private static Boolean TryParseVersion( AssemblyInformation assemblyName, String fullAssemblyName, ref Int32 nameIdx )
      {
         var aux = NextSeparatorIdx( fullAssemblyName, VERSION_SEPARATOR, nameIdx );
         UInt16 tmp = 0;
         var success = aux > 0 && UInt16.TryParse( fullAssemblyName.Substring( nameIdx, aux ), out tmp );
         if ( success )
         {
            assemblyName.VersionMajor = tmp;
            nameIdx += aux + 1;
            aux = NextSeparatorIdx( fullAssemblyName, VERSION_SEPARATOR, nameIdx );
            success = aux > 0 && UInt16.TryParse( fullAssemblyName.Substring( nameIdx, aux ), out tmp );
            if ( success )
            {
               assemblyName.VersionMinor = tmp;
               nameIdx += aux + 1;
               aux = NextSeparatorIdx( fullAssemblyName, VERSION_SEPARATOR, nameIdx );
               success = aux > 0 && UInt16.TryParse( fullAssemblyName.Substring( nameIdx, aux ), out tmp );
               if ( success )
               {
                  assemblyName.VersionBuild = tmp;
                  nameIdx += aux + 1;
                  aux = NextSeparatorIdx( fullAssemblyName, ASSEMBLY_NAME_ELEMENTS_SEPARATOR, nameIdx );
                  success = aux > 0 && UInt16.TryParse( fullAssemblyName.Substring( nameIdx, aux ), out tmp );
                  if ( success )
                  {
                     nameIdx += aux;
                     assemblyName.VersionRevision = tmp;
                  }
               }
            }
         }
         return success;
      }

      private static Boolean TryParseCulture( AssemblyInformation assemblyName, String fullAssemblyName, ref Int32 nameIdx )
      {
         var aux = NextSeparatorIdx( fullAssemblyName, ASSEMBLY_NAME_ELEMENTS_SEPARATOR, nameIdx );
         var success = aux > 0;
         if ( success )
         {
            assemblyName.Culture = fullAssemblyName.Substring( nameIdx, aux );
            nameIdx += aux;
            if ( String.Equals( "\"\"", assemblyName.Culture ) || String.Compare( assemblyName.Culture, NEUTRAL_CULTURE, StringComparison.OrdinalIgnoreCase ) == 0 )
            {
               assemblyName.Culture = NEUTRAL_CULTURE_NAME;
            }
         }
         return success;
      }

      private static Boolean TryParsePublicKeyFullOrToken( AssemblyInformation assemblyName, String fullAssemblyName, ref Int32 nameIdx )
      {
         var aux = NextSeparatorIdx( fullAssemblyName, ASSEMBLY_NAME_ELEMENTS_SEPARATOR, nameIdx );
         var success = aux > 0;
         if ( success && !String.Equals( "null", fullAssemblyName.Substring( nameIdx, aux ), StringComparison.OrdinalIgnoreCase ) )
         {
            assemblyName.PublicKeyOrToken = StringConversions.HexStr2ByteArray( fullAssemblyName, nameIdx, 0, 0 );
         }
         nameIdx += aux;
         return success;
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
      public Int64 Offset { get; set; }
      public ManifestResourceAttributes Attributes { get; set; }
      public String Name { get; set; }
      public TableIndex? Implementation { get; set; }
   }

   public sealed class NestedClassDefinition
   {
      public TableIndex NestedClass { get; set; }
      public TableIndex EnclosingClass { get; set; }
   }

   public sealed class GenericParameterDefinition
   {
      public Int16 GenericParameterIndex { get; set; }
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
      public TableIndex Owner { get; set; }
      public TableIndex Constraint { get; set; }
   }

   public struct TableIndex
   {
      private readonly Int32 _token;

      // index is zero-based
      internal TableIndex( Tables aTable, Int32 anIdx )
      {
         this._token = ( (Int32) aTable << 24 ) | anIdx;
      }

      internal TableIndex( Int32 token )
      {
         // Index is zero-based in CAM
         this._token = ( ( token & TokenUtils.INDEX_MASK ) - 1 ) | ( token & ~TokenUtils.INDEX_MASK );

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
            return this._token & TokenUtils.INDEX_MASK;
         }
      }

      public override string ToString()
      {
         return this.Table + "[" + this.Index + "]";
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
}
