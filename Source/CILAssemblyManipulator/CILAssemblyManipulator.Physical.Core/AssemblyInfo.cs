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
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CILAssemblyManipulator.Physical
{
   /// <summary>
   /// This class encapsulates common information for both <see cref="AssemblyDefinition"/>s and <see cref="AssemblyReference"/>s.
   /// </summary>
   public sealed class AssemblyInformation : IEquatable<AssemblyInformation>
   {
      private String _name;
      private String _culture;
      private Int32 _majorVersion;
      private Int32 _minorVersion;
      private Int32 _buildNumber;
      private Int32 _revision;
      private Byte[] _publicKeyOrToken;

      /// <summary>
      /// Gets or sets the name of the assembly.
      /// </summary>
      /// <value>The name of the assembly.</value>
      public String Name
      {
         set
         {
            Interlocked.Exchange( ref this._name, value );
         }
         get
         {
            return this._name;
         }
      }

      /// <summary>
      /// Gets or sets the culture of the assembly.
      /// </summary>
      /// <value>The culture of the assembly.</value>
      public String Culture
      {
         set
         {
            Interlocked.Exchange( ref this._culture, value );
         }
         get
         {
            return this._culture;
         }
      }

      /// <summary>
      /// Gets or sets the major version of the assembly.
      /// </summary>
      /// <value>The major version of the assembly.</value>
      /// <remarks>
      /// By default, the value will be casted to <see cref="UInt16"/> when emitting assembly binary file.
      /// </remarks>
      public Int32 VersionMajor
      {
         set
         {
            Interlocked.Exchange( ref this._majorVersion, value );
         }
         get
         {
            return this._majorVersion;
         }
      }

      /// <summary>
      /// Gets or sets the minor version of the assembly.
      /// </summary>
      /// <value>The minor version of the assembly.</value>
      /// <remarks>
      /// By default, the value will be casted to <see cref="UInt16"/> when emitting assembly binary file.
      /// </remarks>
      public Int32 VersionMinor
      {
         set
         {
            Interlocked.Exchange( ref this._minorVersion, value );
         }
         get
         {
            return this._minorVersion;
         }
      }

      /// <summary>
      /// Gets or sets the build number of the assembly.
      /// </summary>
      /// <value>The build number of the assembly.</value>
      /// <remarks>
      /// By default, the value will be casted to <see cref="UInt16"/> when emitting assembly binary file.
      /// </remarks>
      public Int32 VersionBuild
      {
         set
         {
            Interlocked.Exchange( ref this._buildNumber, value );
         }
         get
         {
            return this._buildNumber;
         }
      }

      /// <summary>
      /// Gets or sets the revision of the assembly.
      /// </summary>
      /// <value>The revision of the assembly.</value>
      /// <remarks>
      /// By default, the value will be casted to <see cref="UInt16"/> when emitting assembly binary file.
      /// </remarks>
      public Int32 VersionRevision
      {
         set
         {
            Interlocked.Exchange( ref this._revision, value );
         }
         get
         {
            return this._revision;
         }
      }

      /// <summary>
      /// Gets or sets the public key of the assembly.
      /// </summary>
      /// <value>The public key of the assembly.</value>
      /// <remarks>
      /// Values of <c>null</c> and empty byte array are treated as if public key or token would not exist.
      /// </remarks>
      public Byte[] PublicKeyOrToken
      {
         set
         {
            Interlocked.Exchange( ref this._publicKeyOrToken, value );
         }
         get
         {
            return this._publicKeyOrToken;
         }
      }


      /// <summary>
      /// Checks that this <see cref="AssemblyInformation"/> has the same values as given <paramref name="obj"/>, if <paramref name="obj"/> is of type <see cref="AssemblyInformation"/>.
      /// </summary>
      /// <param name="obj">The object to check.</param>
      /// <returns><c>true</c> if <paramref name="obj"/> is of type <see cref="AssemblyInformation"/> and has same values as this <see cref="AssemblyInformation"/>.</returns>
      public override Boolean Equals( Object obj )
      {
         return this.Equals( obj as AssemblyInformation );
      }

      /// <summary>
      /// Gets the hash code for this <see cref="AssemblyInformation"/>.
      /// </summary>
      /// <returns>The hash code for this <see cref="AssemblyInformation"/>.</returns>
      /// <remarks>
      /// The values that are compared are <see cref="Name"/>, <see cref="Culture"/> (both exact and case-sensitive), <see cref="VersionMajor"/>, <see cref="VersionMinor"/>, <see cref="VersionBuild"/>, <see cref="VersionRevision"/>, and <see cref="PublicKeyOrToken"/> (exact array comparison, except that null matches empty array).
      /// </remarks>
      public override Int32 GetHashCode()
      {
         unchecked
         {
            return
               (
                  ( 17 * 23 + this.Name.GetHashCodeSafe()
                  ) * 23 + this.VersionMajor.GetHashCode()
               ) * 23 + this.VersionMinor.GetHashCode();
         }
      }

      /// <summary>
      /// Checks that this <see cref="AssemblyInformation"/> has the same values as given <see cref="AssemblyInformation"/>.
      /// </summary>
      /// <param name="other">The other <see cref="AssemblyInformation"/>.</param>
      /// <returns><c>true</c> if <paramref name="other"/> is not <c>null</c> and has same values as this <see cref="AssemblyInformation"/>; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// The values that are compared are <see cref="Name"/>, <see cref="Culture"/> (both exact and case-sensitive), <see cref="VersionMajor"/>, <see cref="VersionMinor"/>, <see cref="VersionBuild"/>, <see cref="VersionRevision"/>, and <see cref="PublicKeyOrToken"/> (exact array comparison, except that null matches empty array).
      /// </remarks>
      public Boolean Equals( AssemblyInformation other )
      {
         return this.Equals( other, true );
      }

      /// <summary>
      /// Checks that this <see cref="AssemblyInformation"/> has the same values as given <see cref="AssemblyInformation"/>, with parameterized comparison of <see cref="PublicKeyOrToken"/>.
      /// </summary>
      /// <param name="other">The other <see cref="AssemblyInformation"/>.</param>
      /// <param name="comparePublicKeyOrToken">Whether to compare the <see cref="PublicKeyOrToken"/> properties.</param>
      /// <returns><c>true</c>if <paramref name="other"/> is not <c>null</c> and has same values as this <see cref="AssemblyInformation"/>; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// The values that are compared are <see cref="Name"/>, <see cref="Culture"/> (both exact and case-sensitive), <see cref="VersionMajor"/>, <see cref="VersionMinor"/>, <see cref="VersionBuild"/>, <see cref="VersionRevision"/>, and <see cref="PublicKeyOrToken"/> (exact array comparison, except that nulls and empty arrays are considered equal).
      /// The <see cref="PublicKeyOrToken"/>s are compared only if <paramref name="comparePublicKeyOrToken"/> is <c>true</c>.
      /// </remarks>
      public Boolean Equals( AssemblyInformation other, Boolean comparePublicKeyOrToken )
      {
         Byte[] thisPK, otherPK;
         Boolean thisIsNullOrEmpty;
         return Object.ReferenceEquals( this, other ) ||
            ( other != null
            && String.Equals( this.Name, other.Name )
            && this.VersionMajor == other.VersionMajor
            && this.VersionMinor == other.VersionMinor
            && this.VersionBuild == other.VersionBuild
            && this.VersionRevision == other.VersionRevision
            && ( !comparePublicKeyOrToken ||
                  ( ( thisIsNullOrEmpty = ( thisPK = this.PublicKeyOrToken ).IsNullOrEmpty() ) == ( otherPK = other.PublicKeyOrToken ).IsNullOrEmpty()
                     && ( thisIsNullOrEmpty || ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( thisPK, otherPK ) )
                  )
               )
            && String.Equals( this.Culture, other.Culture )
            );
      }

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
      internal const Char VERSION_SEPARATOR = '.';
      private const String VERSION = "Version";
      private const String CULTURE = "Culture";
      private const String PUBLIC_KEY_TOKEN = "PublicKeyToken";
      private const String PUBLIC_KEY = "PublicKey";
      internal const String NEUTRAL_CULTURE = "neutral";
      private const String NEUTRAL_CULTURE_NAME = "";

      /// <summary>
      /// Creates a new string conforming to CIL standards of how assembly strings should be represented.
      /// </summary>
      /// <param name="appendCultureIfNeutral">If <c>true</c>, then culture information will be always present in string. If <c>false</c>, then culture information is omitted if it is <c>null</c> or empty.</param>
      /// <param name="isFullPublicKey">If <c>true</c>, then the <see cref="PublicKeyOrToken"/> will be interpreted as afull public key. If <c>false</c>, then the <see cref="PublicKeyOrToken"/> will be interpreted as a public key token. In either case, it will be omitted, if it is <c>null</c> or empty.</param>
      /// <returns>The string representing information about this <see cref="AssemblyInformation"/> and conforming to CIL standards.</returns>
      public String ToString(
         Boolean appendCultureIfNeutral,
         Boolean isFullPublicKey
         )
      {
         var sb = new StringBuilder( this.Name.EscapeCILTypeString() );
         sb.Append( ASSEMBLY_NAME_ELEMENTS_SEPARATOR + " " + VERSION + ASSEMBLY_NAME_ELEMENT_VALUE_SEPARATOR )
            .Append( this.VersionMajor )
            .Append( VERSION_SEPARATOR )
            .Append( this.VersionMinor )
            .Append( VERSION_SEPARATOR )
            .Append( this.VersionBuild )
            .Append( VERSION_SEPARATOR )
            .Append( this.VersionRevision );

         var culture = this.Culture;
         var isNullOrEmptyCulture = String.IsNullOrEmpty( culture );
         if ( !isNullOrEmptyCulture || appendCultureIfNeutral )
         {
            sb.Append( ASSEMBLY_NAME_ELEMENTS_SEPARATOR + " " + CULTURE + ASSEMBLY_NAME_ELEMENT_VALUE_SEPARATOR )
               .Append( isNullOrEmptyCulture ? NEUTRAL_CULTURE : culture );
         }

         var pKey = this.PublicKeyOrToken;
         if ( !pKey.IsNullOrEmpty() )
         {
            sb.Append( ASSEMBLY_NAME_ELEMENTS_SEPARATOR + " " + ( isFullPublicKey ? PUBLIC_KEY : PUBLIC_KEY_TOKEN ) + ASSEMBLY_NAME_ELEMENT_VALUE_SEPARATOR )
               .Append( StringConversions.CreateHexString( pKey, false ) );
         }

         return sb.ToString();
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
      public static AssemblyInformation Parse( String textualAssemblyName )
      {
         Boolean isFullPublicKey;
         return Parse( textualAssemblyName, out isFullPublicKey );
      }

      /// <summary>
      /// Tries to parse given textual assembly name and throws <see cref="FormatException"/> if parsing is unsuccessful.
      /// </summary>
      /// <param name="textualAssemblyName">The textual assembly name.</param>
      /// <param name="isFullPublicKey">Whether the textual assembly name contained full public key information.</param>
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
      /// <param name="isFullPublicKey">Whether the textual assembly name contained full public key information.</param>
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
            assemblyName.Name = textualAssemblyName.UnescapeCILTypeString( 0, nameIdx );

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
         else if ( aux > 9 && String.Compare( str, idx, PUBLIC_KEY_TOKEN, 0, aux, StringComparison.OrdinalIgnoreCase ) == 0 )
         {
            result = Elements.PublicKeyToken;
         }
         else if ( aux == 9 && String.Compare( str, idx, PUBLIC_KEY, 0, aux, StringComparison.OrdinalIgnoreCase ) == 0 )
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
            var culture = fullAssemblyName.Substring( nameIdx, aux );
            nameIdx += aux;
            if ( culture.Length > 0 && !String.Equals( NEUTRAL_CULTURE, culture, StringComparison.OrdinalIgnoreCase ) && !String.Equals( "\"\"", culture ) )
            {
               assemblyName.Culture = culture;
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
            assemblyName.PublicKeyOrToken = StringConversions.CreateHexBytes( fullAssemblyName, nameIdx, 0, 0 );
         }
         nameIdx += aux;
         return success;
      }
   }
}

public static partial class E_CILPhysical
{
   /// <summary>
   /// Copies the contents of this <see cref="AssemblyInformation"/> to the given destination <see cref="AssemblyInformation"/>.
   /// </summary>
   /// <param name="source">The <see cref="AssemblyInformation"/> to copy information from.</param>
   /// <param name="destination">The <see cref="AssemblyInformation"/> to copy information to.</param>
   /// <remarks>
   /// The <see cref="AssemblyInformation.PublicKeyOrToken"/> array is copied, if it is not null or empty, so that modification to this array in the original <see cref="AssemblyInformation"/> are not propagated to the <see cref="AssemblyInformation.PublicKeyOrToken"/> of the given target <see cref="AssemblyInformation"/>.
   /// </remarks>
   /// <exception cref="NullReferenceException">If <paramref name="source"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="destination"/> is <c>null</c>.</exception>
   public static void DeepCopyContentsTo( this AssemblyInformation source, AssemblyInformation destination )
   {
      ArgumentValidator.ValidateNotNull( "Destination", destination );

      destination.Name = source.Name;
      destination.VersionMajor = source.VersionMajor;
      destination.VersionMinor = source.VersionMinor;
      destination.VersionBuild = source.VersionBuild;
      destination.VersionRevision = source.VersionRevision;
      destination.Culture = source.Culture;
      destination.PublicKeyOrToken = source.PublicKeyOrToken.IsNullOrEmpty() ? source.PublicKeyOrToken : source.PublicKeyOrToken.CreateBlockCopy();
   }

   /// <summary>
   /// Creates a new <see cref="AssemblyInformation"/> that will hold the same values as source <see cref="AssemblyInformation"/>.
   /// </summary>
   /// <param name="assemblyInfo">The source <see cref="AssemblyInformation"/>.</param>
   /// <returns><c>null</c> if <paramref name="assemblyInfo"/> is <c>null</c>, otherwise a new <see cref="AssemblyInformation"/> with same content as <paramref name="assemblyInfo"/>.</returns>
   /// <remarks>
   /// The <see cref="AssemblyInformation.PublicKeyOrToken"/> array is copied, if it is not null or empty, so that modification to this array in the original <see cref="AssemblyInformation"/> are not propagated to the <see cref="AssemblyInformation.PublicKeyOrToken"/> of the given target <see cref="AssemblyInformation"/>.
   /// </remarks>
   public static AssemblyInformation CreateDeepCopy( this AssemblyInformation assemblyInfo )
   {
      AssemblyInformation retVal;
      if ( assemblyInfo == null )
      {
         retVal = null;
      }
      else
      {
         retVal = new AssemblyInformation();
         assemblyInfo.DeepCopyContentsTo( retVal );
      }
      return retVal;
   }
}