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
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
   public sealed class AssemblyInformation : IEquatable<AssemblyInformation>
   {
      public Int32 VersionMajor { get; set; }
      public Int32 VersionMinor { get; set; }
      public Int32 VersionBuild { get; set; }
      public Int32 VersionRevision { get; set; }
      public Byte[] PublicKeyOrToken { get; set; }
      public String Name { get; set; }
      public String Culture { get; set; }

      public override Boolean Equals( Object obj )
      {
         return this.Equals( obj as AssemblyInformation );
      }

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

      public Boolean Equals( AssemblyInformation other )
      {
         return Object.ReferenceEquals( this, other ) ||
            ( other != null
            && String.Equals( this.Name, other.Name )
            && this.VersionMajor == other.VersionMajor
            && this.VersionMinor == other.VersionMinor
            && this.VersionBuild == other.VersionBuild
            && this.VersionRevision == other.VersionRevision
            && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( this.PublicKeyOrToken, other.PublicKeyOrToken )
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
}
