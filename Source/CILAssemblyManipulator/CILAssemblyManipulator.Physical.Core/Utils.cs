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
using UtilPack.CollectionsWithRoles;
using UtilPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TabularMetaData;

namespace CILAssemblyManipulator.Physical
{
   /// <summary>
   /// This class encapsulates information about character range when parsing type string.
   /// </summary>
   /// <seealso cref="TypeParseInfo"/>
   public sealed class ParseRange
   {
      /// <summary>
      /// Gets or sets the index of the first character that is included in the range.
      /// </summary>
      /// <value>The index of the first character that is included in the range.</value>
      public Int32 StartIndex { get; set; }

      /// <summary>
      /// Gets or sets the amount of characters for this range.
      /// </summary>
      /// <value>The amount of characters for this range.</value>
      public Int32 CharacterCount { get; set; }

      /// <summary>
      /// Returns a textual representation of this <see cref="ParseRange"/>.
      /// </summary>
      /// <returns>A textual representation of this <see cref="ParseRange"/>.</returns>
      public override String ToString()
      {
         return "[" + this.StartIndex + ":" + this.CharacterCount + "]";
      }
   }

   /// <summary>
   /// This class represents parsing information about parsed assembly-qualified type strings.
   /// </summary>
   /// <seealso cref="Miscellaneous.ParseAssemblyQualifiedTypeString(string)"/>
   public sealed class TypeParseInfo
   {
      /// <summary>
      /// Creates a new instance of <see cref="TypeParseInfo"/>.
      /// </summary>
      public TypeParseInfo()
      {
         this.GenericParameters = new List<TypeParseInfo>();
      }

      /// <summary>
      /// Gets or sets the <see cref="ParseRange"/> for the type string.
      /// </summary>
      /// <value>The <see cref="ParseRange"/> for the type string.</value>
      public ParseRange TypeStringRange { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="ParseRange"/> for the assembly string.
      /// </summary>
      /// <value>The <see cref="ParseRange"/> for the assembly string.</value>
      public ParseRange AssemblyStringRange { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="ParseRange"/> for array, pointer, or reference specification string.
      /// </summary>
      /// <value>The <see cref="ParseRange"/> for array, pointer, or reference specification string.</value>
      public ParseRange ElementStringRange { get; set; }

      /// <summary>
      /// Gets the list of nested generic parameters.
      /// </summary>
      /// <value>The list of nested generic parameters.</value>
      public List<TypeParseInfo> GenericParameters { get; }
   }
   /// <summary>
   /// This class contains miscellaneous methods, which are either not extension methods, or extension methods for types defined in other assemblies than this.
   /// </summary>
   public static class Miscellaneous
   {
      private const Char ESCAPE_CHAR = '\\';
      private static readonly Char[] ESCAPABLE_CHARS_WITHIN_TYPESTRING = { ESCAPE_CHAR, ',', '+', '&', '*', '[', ']' };
      private static readonly Char[] CHARS_ENDING_SIMPLE_TYPENAME = { '&', '*', '[' };

      /// <summary>
      /// This is character that separates nested type names (<c>+</c>).
      /// </summary>
      public const Char NESTED_TYPE_SEPARATOR = '+';
      private const String NESTED_TYPE_SEPARATOR_STRING = "+";

      /// <summary>
      /// This is character that separates namespace and type name (<c>.</c>).
      /// </summary>
      public const Char NAMESPACE_SEPARATOR = '.';

      private const String TYPE_ASSEMBLY_SEPARATOR = ", ";

      /// <summary>
      /// This is the textual name of instance constructors (<c>.ctor</c>).
      /// </summary>
      public const String INSTANCE_CTOR_NAME = ".ctor";

      /// <summary>
      /// This is the textual name of static constructors (<c>.cctor</c>).
      /// </summary>
      public const String CLASS_CTOR_NAME = ".cctor";

      /// <summary>
      /// This is the name for type holding global functions and variables within the single CIL module (<c>&lt;Module&gt;</c>).
      /// </summary>
      public const String MODULE_TYPE_NAME = "<Module>";

      /// <summary>
      /// Detects whether type string is assembly name -qualified or not.
      /// </summary>
      /// <param name="str">The type string.</param>
      /// <param name="typeString">This parameter will be the actual type string without assembly name.</param>
      /// <param name="assemblyString">This parameter will be the assembly name string, if the type string was assembly name -qualified. Otherwise, this parameter will be <c>null</c>.</param>
      /// <returns><c>true</c> if assembly name was present; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// This method does not check whether the actual assembly string, if recognized, is valid assembly string.
      /// Use <see cref="AssemblyInformation.TryParse"/> method for that.
      /// </remarks>
      public static Boolean ParseAssemblyQualifiedTypeString( this String str, out String typeString, out String assemblyString )
      {
         var assemblySeparatorIndex = -1;
         {
            var depth = 0;
            // Do everything in one scan, and stop when we find top-level assembly separator.
            for ( Int32 scanIdx = 0, max = str.Length - 1; assemblySeparatorIndex == -1 && scanIdx < max; ++scanIdx )
            {
               switch ( str[scanIdx] )
               {
                  case '\\':
                     // Skip escapes
                     ++scanIdx;
                     break;
                  case '[':
                     if ( str[scanIdx + 1] == '[' )
                     {
                        // Start of the list of generic parameters
                        ++depth;
                        ++scanIdx;
                     }
                     break;
                  case ']':
                     if ( str[scanIdx + 1] == ']' )
                     {
                        --depth;
                        ++scanIdx;
                     }
                     break;
                  case ',':
                     if ( str[scanIdx + 1] == ' ' )
                     {
                        if ( depth == 0 && scanIdx < max - 1 )
                        {
                           assemblySeparatorIndex = scanIdx;
                           //++scanIdx;
                        }
                     }
                     break;
               }
            }
         }

         var retVal = assemblySeparatorIndex != -1;
         if ( retVal )
         {
            typeString = str.Substring( 0, assemblySeparatorIndex );
            assemblyString = str.Substring( assemblySeparatorIndex + 2 );
         }
         else
         {
            typeString = str;
            assemblyString = null;
         }
         return retVal;
      }

      /// <summary>
      /// Detects whether type string is assembly name -qualified or not.
      /// </summary>
      /// <param name="str">The type string.</param>
      /// <returns>A <see cref="TypeParseInfo"/> object containing information about the type string.</returns>
      /// <remarks>
      /// This method does not check whether the actual assembly string, if recognized, is valid assembly string.
      /// Use <see cref="AssemblyInformation.TryParse"/> method for that.
      /// </remarks>
      public static TypeParseInfo ParseAssemblyQualifiedTypeString( this String str )
      {
         var typeInfo = new TypeParseInfo();
         // If there is an assembly name, there will be ", " but not "\, " in type string. 

         // First, scan for '[['. This will mean the start of the generic parameter list. We must skip through all the generic parameters, as the assembly string will start only then.
         // E.g. for type strings like this: X[[Y, assembly1][Z[[A, assembly2]], assembly3]], assembly4, we want to capture 'assembly4' as assembly string

         var arrayAux = -1;
         var assemblySeparatorIndex = -1;
         {
            var depth = 0;
            var typeInfoStack = new Stack<TypeParseInfo>();
            typeInfoStack.Push( typeInfo );

            var aux = 0;

            // Do everything in one scan, and stop when we find top-level assembly separator.
            for ( Int32 scanIdx = 0, max = str.Length - 1; assemblySeparatorIndex == -1 && scanIdx < max; ++scanIdx )
            {
               switch ( str[scanIdx] )
               {
                  case '\\':
                     // Skip escapes
                     ++scanIdx;
                     break;
                  case '[':
                     if ( str[scanIdx + 1] == '[' )
                     {
                        // Start of the list of generic parameters
                        ++depth;
                        ++scanIdx;

                        // Update type string of current type info
                        var pInfo = typeInfoStack.Peek();
                        pInfo.SetTypeAndElementStringRanges( aux, scanIdx - 1, ref arrayAux );
                        aux = scanIdx + 1;

                        // Add generic parameter to be used later
                        var newInfo = new TypeParseInfo();
                        pInfo.GenericParameters.Add( newInfo );
                        typeInfoStack.Push( newInfo );

                     }
                     else if ( depth > 0 && scanIdx > 2 && str[scanIdx - 1] == ',' && str[scanIdx - 2] == ']' )
                     {
                        // In middle of generic parameter list, when next starts
                        // Pop current generic parameter
                        typeInfoStack.Pop();

                        // Create new generic parameter
                        var newInfo = new TypeParseInfo();
                        // Add to the parent
                        typeInfoStack.Peek().GenericParameters.Add( newInfo );
                        // Set as currently top-most type info
                        typeInfoStack.Push( newInfo );
                        // Remember to update auxiliary index 
                        aux = scanIdx + 1;
                     }
                     else
                     {
                        arrayAux = scanIdx;
                     }
                     break;
                  case ']':
                     if ( str[scanIdx + 1] == ']' )
                     {
                        --depth;
                        ++scanIdx;
                        if ( depth >= 0 )
                        {
                           var pInfo = typeInfoStack.Pop();
                           if ( pInfo.TypeStringRange == null )
                           {
                              pInfo.SetTypeAndElementStringRanges( aux, scanIdx - 1, ref arrayAux );
                           }
                           else
                           {
                              pInfo.AssemblyStringRange = new ParseRange() { StartIndex = aux, CharacterCount = scanIdx - aux - 1 };
                           }
                           aux = scanIdx + 1;
                        }
                     }
                     else if ( depth > 0 && scanIdx < max - 2 && str[scanIdx + 1] == ',' && str[scanIdx + 2] == '[' )
                     {
                        // In middle of generic parameter list, when previous ends
                        var pInfo = typeInfoStack.Peek();
                        if ( pInfo.TypeStringRange == null )
                        {
                           pInfo.SetTypeAndElementStringRanges( aux, scanIdx, ref arrayAux );
                        }
                        else
                        {
                           pInfo.AssemblyStringRange = new ParseRange() { StartIndex = aux, CharacterCount = scanIdx - aux };
                        }
                        aux = scanIdx + 2;
                     }
                     break;
                  case ',':
                     if ( str[scanIdx + 1] == ' ' )
                     {
                        if ( depth == 0 && scanIdx < max - 1 )
                        {
                           assemblySeparatorIndex = scanIdx;
                           //++scanIdx;
                        }
                        else if ( depth > 0 )
                        {
                           var pInfo = typeInfoStack.Peek();
                           if ( pInfo.TypeStringRange == null )
                           {
                              pInfo.SetTypeAndElementStringRanges( aux, scanIdx, ref arrayAux );
                              aux = scanIdx + 2;
                           }
                           else if ( aux == scanIdx )
                           {
                              // Right after ]]
                              aux = scanIdx + 2;
                           }
                        }
                     }
                     break;
               }
            }
         }

         if ( assemblySeparatorIndex == -1 )
         {
            if ( typeInfo.TypeStringRange == null )
            {
               typeInfo.SetTypeAndElementStringRanges( 0, str.Length, ref arrayAux );
            }
         }
         else
         {
            if ( typeInfo.TypeStringRange == null )
            {
               typeInfo.SetTypeAndElementStringRanges( 0, assemblySeparatorIndex, ref arrayAux );
            }
            typeInfo.AssemblyStringRange = new ParseRange() { StartIndex = assemblySeparatorIndex + 2, CharacterCount = str.Length - assemblySeparatorIndex - 2 };
         }

         return typeInfo;

      }

      private static void SetTypeAndElementStringRanges( this TypeParseInfo info, Int32 typeStart, Int32 typeEnd, ref Int32 arrayAux )
      {
         // TODO array range
         info.TypeStringRange = new ParseRange() { StartIndex = typeStart, CharacterCount = typeEnd - typeStart };
      }

      /// <summary>
      /// This is helper method to append assembly name to the current <see cref="StringBuilder"/>.
      /// It appends a special separator (comma followed by space), and then it appends the given assembly name string.
      /// </summary>
      /// <param name="sb">This <see cref="StringBuilder"/>.</param>
      /// <param name="assembly">The assembly name string.</param>
      /// <returns>This <see cref="StringBuilder"/>.</returns>
      /// <exception cref="NullReferenceException">If <paramref name="sb"/> is <c>null</c>.</exception>
      public static StringBuilder AppendAssemblyNameToTypeString( this StringBuilder sb, String assembly )
      {
         return sb
            .Append( TYPE_ASSEMBLY_SEPARATOR )
            .Append( assembly );
      }

      /// <summary>
      /// Detects whether type string represents a nested or top-level type.
      /// </summary>
      /// <param name="str">The type string.</param>
      /// <param name="enclosingTypeName">If type string represents nested type string, then this parameter will be the type string of immediately enclosing type. Otherwise, this parameter will be <c>null</c>.</param>
      /// <param name="nestedTypeName">If type string represents nested type string, then this parameter will be the nested type name. Otherwise, this parameter will be <paramref name="str"/>.</param>
      /// <returns><c>true</c> if type string represents nested type; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// The <paramref name="enclosingTypeName"/> may still represent nested type even if type string represents a nested type.
      /// However, the <paramref name="nestedTypeName"/> will always be the name of the nested-most type, if type string represents a nested type.
      /// </remarks>
      public static Boolean ParseTypeNameStringForNestedType( this String str, out String enclosingTypeName, out String nestedTypeName )
      {
         Int32 enclosingTypeLength;
         var retVal = str.GetLastSeparatorsFromTypeString( NESTED_TYPE_SEPARATOR, out enclosingTypeLength );
         SetNestedTypeStringResults( str, retVal, enclosingTypeLength, out enclosingTypeName, out nestedTypeName );
         return retVal;
      }

      /// <summary>
      /// Detects whether type string represents a nested or top-level type.
      /// </summary>
      /// <param name="str">The type string.</param>
      /// <param name="topLevelTypeName">If type string represents nested type string, then this parameter will the the type name of top-level enclosing type. Otherwise, this parameter will be <c>null</c>.</param>
      /// <param name="nestedTypeName">If type string represents nested type string, then this parameter will be the nested type string. Otherwise, this parameter will be <paramref name="str"/>.</param>
      /// <returns><c>true</c> if type string represents nested type; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// The <paramref name="nestedTypeName"/> may still represent nested type even if type string represents a nested type.
      /// However, the <paramref name="topLevelTypeName"/> will always be the name of the top-level type.
      /// </remarks>
      public static Boolean ParseTypeNameStringForTopLevelType( this String str, out String topLevelTypeName, out String nestedTypeName )
      {
         Int32 enclosingTypeLength;
         var retVal = str.GetFirstSeparatorsFromFullTypeString( NESTED_TYPE_SEPARATOR_STRING, out enclosingTypeLength );
         if ( retVal )
         {
            SetNestedTypeStringResults( str, retVal, enclosingTypeLength, out topLevelTypeName, out nestedTypeName );
         }
         else
         {
            topLevelTypeName = str;
            nestedTypeName = null;
         }
         return retVal;
      }

      private static void SetNestedTypeStringResults( String str, Boolean retVal, Int32 enclosingTypeLength, out String enclosingTypeName, out String nestedTypeName )
      {
         if ( retVal )
         {
            // This is nested type
            enclosingTypeName = str.Substring( 0, enclosingTypeLength );
            nestedTypeName = str.Substring( enclosingTypeLength + 1 );
         }
         else
         {
            // This is top-level type
            enclosingTypeName = null;
            nestedTypeName = str;
         }
      }

      /// <summary>
      /// Detects the namespace in type string.
      /// </summary>
      /// <param name="str">The type string.</param>
      /// <param name="ns">This parameter will be the namespace of the type string, or <c>null</c> if the type did not have namespace.</param>
      /// <param name="name">This parameter will be the name of the type without namespace.</param>
      /// <returns><c>true</c> if type string had namespace; <c>false</c> otherwise.</returns>
      public static Boolean ParseTypeNameStringForNamespace( this String str, out String ns, out String name )
      {
         Int32 nsLength;
         var retVal = str.GetLastSeparatorsFromTypeString( NAMESPACE_SEPARATOR, out nsLength );
         if ( retVal )
         {
            // Type name has namespace
            ns = str.Substring( 0, nsLength );
            name = str.Substring( nsLength + 1 );
         }
         else
         {
            // Type name does not have namespace
            ns = null;
            name = str;
         }

         return retVal;
      }

      private static Boolean GetLastSeparatorsFromTypeString( this String str, Char separatorChar, out Int32 firstLength )
      {
         // Length needs to be at least 3 for even have namespace (namespace + dot + type name)
         var retVal = str.Length >= 3;
         firstLength = -1;
         if ( retVal )
         {
            var curIdx = str.Length - 1;
            Int32 sepIdx;
            do
            {
               sepIdx = str.LastIndexOf( separatorChar, curIdx );
               if ( sepIdx > 0 && str[sepIdx - 1] == ESCAPE_CHAR )
               {
                  curIdx -= 2;
                  sepIdx = -1;
               }
               else
               {
                  curIdx = sepIdx;
               }
            } while ( curIdx > 0 && sepIdx == -1 );

            retVal = sepIdx > 0;
            if ( retVal )
            {
               firstLength = sepIdx;
            }
         }

         return retVal;
      }

      private static Boolean GetFirstSeparatorsFromFullTypeString( this String str, String separatorString, out Int32 firstLength )
      {
         var strMax = 0;
         var curIdx = -1;
         while ( strMax < str.Length
            && ( curIdx = str.IndexOf( separatorString, strMax ) ) > 0
            && str[curIdx - 1] == ESCAPE_CHAR )
         {
            strMax = curIdx + separatorString.Length;
         }

         var retVal = !( curIdx < 0 || strMax >= str.Length );
         if ( retVal )
         {
            // Separator present, mark its length
            firstLength = curIdx;
         }
         else
         {
            // Separator not present, so full string is length of first part
            firstLength = str.Length;
         }

         return retVal;
      }

      /// <summary>
      /// Given a type string, unescapes any escaped characters that were added by <see cref="EscapeCILTypeString(string)"/> or <see cref="EscapeCILTypeString(string, int, int)"/> methods.
      /// </summary>
      /// <param name="str">The string to unescape. May be <c>null</c>.</param>
      /// <returns>The unescaped string.</returns>
      /// <seealso cref="EscapeCILTypeString(string)"/>
      /// <seealso cref="EscapeCILTypeString(string, int, int)"/>
      public static String UnescapeCILTypeString( this String str )
      {
         return String.IsNullOrEmpty( str ) ? str : str.UnescapeCILTypeString( 0, str.Length );
      }


      /// <summary>
      /// Given a type string, escapes any character that would mess the textual parsing of the type string back to type.
      /// </summary>
      /// <param name="str">The string to escape. May be <c>null</c>.</param>
      /// <returns>The escaped type string.</returns>
      /// <remarks>
      /// The escapable characters are:
      /// <list type="bullet">
      /// <item><description><c>\</c> (the escape character itself),</description></item>
      /// <item><description><c>,</c> (comma),</description></item>
      /// <item><description><c>+</c> (plus),</description></item>
      /// <item><description><c>&amp;</c> (ampersand),</description></item>
      /// <item><description><c>*</c> (asterisk),</description></item>
      /// <item><description><c>[</c> (left bracket), and</description></item>
      /// <item><description><c>]</c> (right bracket).</description></item>
      /// </list>
      /// For more information about type strings, see <see href="http://msdn.microsoft.com/en-us/library/yfsftwz6%28v=vs.110%29.aspx"/> and <see href="http://msdn.microsoft.com/en-us/library/system.type.assemblyqualifiedname.aspx"/>.
      /// </remarks>
      public static String EscapeCILTypeString( this String str )
      {
         return String.IsNullOrEmpty( str ) ? str : str.EscapeCILTypeString( 0, str.Length );
      }

      /// <summary>
      /// Given a type string, escapes any character that would mess the textual parsing of the type string back to type.
      /// </summary>
      /// <param name="str">The string to escape. May be <c>null</c>.</param>
      /// <param name="startIdx">The index within given string to start looking for escapable characters.</param>
      /// <param name="count">The amount of characters to check.</param>
      /// <returns>The escaped type string.</returns>
      /// <remarks>
      /// The escapable characters are:
      /// <list type="bullet">
      /// <item><description><c>\</c> (the escape character itself),</description></item>
      /// <item><description><c>,</c> (comma),</description></item>
      /// <item><description><c>+</c> (plus),</description></item>
      /// <item><description><c>&amp;</c> (ampersand),</description></item>
      /// <item><description><c>*</c> (asterisk),</description></item>
      /// <item><description><c>[</c> (left bracket), and</description></item>
      /// <item><description><c>]</c> (right bracket).</description></item>
      /// </list>
      /// For more information about type strings, see <see href="http://msdn.microsoft.com/en-us/library/yfsftwz6%28v=vs.110%29.aspx"/> and <see href="http://msdn.microsoft.com/en-us/library/system.type.assemblyqualifiedname.aspx"/>.
      /// </remarks>
      public static String EscapeCILTypeString( this String str, Int32 startIdx, Int32 count )
      {
         if ( !String.IsNullOrEmpty( str ) )
         {
            if ( str.IndexOfAny( ESCAPABLE_CHARS_WITHIN_TYPESTRING, startIdx, count ) >= 0 )
            {
               // String contains characters that should be escaped
               var chars = new Char[count * 2];
               var cIdx = 0;
               for ( var i = startIdx; i < count; ++i )
               {
                  var ch = str[i];
                  if ( Array.IndexOf( ESCAPABLE_CHARS_WITHIN_TYPESTRING, ch ) >= 0 )
                  {
                     chars[cIdx++] = ESCAPE_CHAR;
                  }
                  chars[cIdx++] = ch;
               }
               str = new String( chars, 0, cIdx );
            }
            else if ( startIdx > 0 || count < str.Length )
            {
               str = str.Substring( startIdx, count );
            }
         }
         return str;
      }

      /// <summary>
      /// Given a type string, unescapes any escaped characters that were added by <see cref="EscapeCILTypeString(string)"/> or <see cref="EscapeCILTypeString(string, int, int)"/> methods.
      /// </summary>
      /// <param name="str">The string to unescape. May be <c>null</c>.</param>
      /// <param name="startIdx">The index within given string to start looking for escapable characters.</param>
      /// <param name="count">The amount of characters to check.</param>
      /// <returns>The unescaped string.</returns>
      /// <seealso cref="EscapeCILTypeString(string)"/>
      /// <seealso cref="EscapeCILTypeString(string, int, int)"/>
      public static String UnescapeCILTypeString( this String str, Int32 startIdx, Int32 count )
      {
         if ( !String.IsNullOrEmpty( str ) )
         {
            if ( str.IndexOf( ESCAPE_CHAR, startIdx, count ) >= 0 )
            {
               // String contains escaped charcters
               var chars = new Char[count];
               var prevWasEscaped = false;
               var curLen = 0;
               for ( var i = startIdx; i < count; ++i )
               {
                  var ch = str[i];
                  if ( ch != ESCAPE_CHAR || prevWasEscaped )
                  {
                     chars[curLen] = ch;
                     ++curLen;
                     prevWasEscaped = false;
                  }
                  else
                  {
                     prevWasEscaped = true;
                  }
               }
               str = new String( chars, 0, curLen );
            }
            else if ( startIdx > 0 || count < str.Length )
            {
               str = str.Substring( startIdx, count );
            }
         }
         return str;
      }

      /// <summary>
      /// Helper method to combine the namespace and name of the type.
      /// Will check for null or empty namespaces, but will not check for empty or null type names.
      /// </summary>
      /// <param name="typeNamespace">The namespace of the type.</param>
      /// <param name="typeName">The name of the type.</param>
      /// <returns>If <paramref name="typeNamespace"/> is not <c>null</c> nor empty, then returns <paramref name="typeNamespace"/> concatenated with <see cref="NAMESPACE_SEPARATOR"/> concatenated with <paramref name="typeName"/>. Otherwise, returns <paramref name="typeName"/>.</returns>
      public static String CombineNamespaceAndType( String typeNamespace, String typeName )
      {
         return String.IsNullOrEmpty( typeNamespace ) ? typeName : ( typeNamespace + NAMESPACE_SEPARATOR + typeName );
      }

      /// <summary>
      /// Helper method to combine enclosing and nested type names.
      /// Will check for null or empty enclosing type, but will not check for empty or null nested type names.
      /// </summary>
      /// <param name="enclosing">The enclosing type name, if any.</param>
      /// <param name="nested">The nested type string.</param>
      /// <returns>If <paramref name="enclosing"/> is not <c>null</c> nor empty, then returns <paramref name="enclosing"/> concatenated with <see cref="NESTED_TYPE_SEPARATOR"/> concatenated with <paramref name="nested"/>. Otherwise, returns <paramref name="nested"/>.</returns>
      public static String CombineEnclosingAndNestedType( String enclosing, String nested )
      {
         return String.IsNullOrEmpty( enclosing ) ? nested : ( enclosing + NESTED_TYPE_SEPARATOR + nested );
      }

      /// <summary>
      /// Helper method to combine assembly name string and type string.
      /// Will check for null or empty assembly name, but will not check for empty or null type names.
      /// </summary>
      /// <param name="assembly">The assembly name string.</param>
      /// <param name="type">The type string.</param>
      /// <returns>If <paramref name="assembly"/> is not <c>null</c> nor empty, then returns <paramref name="assembly"/> concatenated with comma and space, concatenated with <paramref name="type"/>. Otherwise, returns <paramref name="type"/>.</returns>
      public static String CombineAssemblyAndType( String assembly, String type )
      {
         return String.IsNullOrEmpty( assembly ) ? type : ( type + TYPE_ASSEMBLY_SEPARATOR + assembly );
      }
   }

#pragma warning disable 1591
   // Note: This class will become internal when merging CAM.Physical DLL.
   public static class CAMCoreInternals
   {
      public const Int32 INDEX_MASK = 0x00FFFFF;

      public const Int32 AMOUNT_OF_TABLES = Byte.MaxValue + 1;

      public static T GetOrNull<T>( this MetaDataTable<T> list, Int32 idx )
         where T : class
      {
         return idx < list.GetRowCount() ? list.TableContents[idx] : null;
      }

      public static Boolean ArrayQueryEquality<T>( this ArrayQuery<T> x, ArrayQuery<T> y, Equality<T> equality = null )
      {
         // TODO make equality classes to CWR
         return SequenceEqualityComparer<ArrayQuery<T>, T>.SequenceEquality( x, y, equality );
      }

      public static Int32 ArrayQueryHashCode<T>( this ArrayQuery<T> x, HashCode<T> hashCode = null )
      {
         // TODO make equality classes to CWR
         return SequenceEqualityComparer<ArrayQuery<T>, T>.SequenceHashCode( x, hashCode );
      }

      public static Boolean DictionaryQueryEquality<TKey, TValue>( this DictionaryQuery<TKey, TValue> x, DictionaryQuery<TKey, TValue> y, Equality<TValue> equality = null )
      {
         // TODO make equality classes to CWR
         if ( equality == null )
         {
            equality = ( xVal, yVal ) => EqualityComparer<TValue>.Default.Equals( xVal, yVal );
         }

         return x.Count == y.Count && x.All( kvp =>
         {
            Boolean success;
            var val = y.TryGetValue( kvp.Key, out success );
            return success && equality( kvp.Value, val );
         } );
      }

      // For some reason, this method is missing from PCL
      public static Int32 FindIndex<T>( this IList<T> list, Predicate<T> match )
      {
         var max = list.Count;
         for ( var i = 0; i < max; ++i )
         {
            if ( match( list[i] ) )
            {
               return i;
            }
         }
         return -1;
      }
   }

#pragma warning restore 1591

   internal static class Consts
   {

      internal const String ENUM_NAMESPACE = "System";
      internal const String ENUM_TYPENAME = "Enum";
      internal const String TYPE_NAMESPACE = "System";
      internal const String TYPE_TYPENAME = "Type";

      internal const String SYSTEM_OBJECT_NAMESPACE = "System";
      internal const String SYSTEM_OBJECT_TYPENAME = "Object";
   }
}
