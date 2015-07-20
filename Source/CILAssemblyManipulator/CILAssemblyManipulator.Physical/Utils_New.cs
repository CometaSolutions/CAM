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

namespace CILAssemblyManipulator.Physical
{
   public static class Miscellaneous
   {
      private const Char ESCAPE_CHAR = '\\';
      private static readonly Char[] ESCAPABLE_CHARS_WITHIN_TYPESTRING = { ESCAPE_CHAR, ',', '+', '&', '*', '[', ']' };
      private static readonly Char[] CHARS_ENDING_SIMPLE_TYPENAME = { '&', '*', '[' };
      public const Char NESTED_TYPE_SEPARATOR = '+';
      private const String NESTED_TYPE_SEPARATOR_STRING = "+";
      public const Char NAMESPACE_SEPARATOR = '.';

      private const String TYPE_ASSEMBLY_SEPARATOR = ", ";

      public const String INSTANCE_CTOR_NAME = ".ctor";
      public const String CLASS_CTOR_NAME = ".cctor";

      /// <summary>
      /// Detects whether type string is assembly name -qualified or not.
      /// </summary>
      /// <param name="str">The type string.</param>
      /// <param name="typeString">This parameter will be the actual type string without assembly name.</param>
      /// <param name="assemblyString">This parameter will be the assembly name string, if the type string was assembly name -qualified. Otherwise, this parameter will be <c>null</c>.</param>
      /// <returns><c>true</c> if assembly name was present; <c>false</c> otherwise.</returns>
      public static Boolean ParseAssemblyQualifiedTypeString( this String str, out String typeString, out String assemblyString )
      {
         Int32 typeLength;
         // If there is an assembly name, there will be ", " but not "\, " in type string. 
         var retVal = str.GetFirstSeparatorsFromFullTypeString( TYPE_ASSEMBLY_SEPARATOR, out typeLength );
         if ( retVal )
         {
            // Assembly name present
            typeString = str.Substring( 0, typeLength );
            assemblyString = str.Substring( typeLength + TYPE_ASSEMBLY_SEPARATOR.Length );
         }
         else
         {
            // Assembly name not present
            typeString = str;
            assemblyString = null;
         }

         return retVal;
      }

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
      /// However, the <paramref name="enclosdingTypeName"/> will always be the name of the top-level type.
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
                  --curIdx;
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

      // As specified in combination of
      // http://msdn.microsoft.com/en-us/library/yfsftwz6%28v=vs.110%29.aspx 
      // and
      // http://msdn.microsoft.com/en-us/library/system.type.assemblyqualifiedname.aspx
      public static String UnescapeCILTypeString( this String str )
      {
         return String.IsNullOrEmpty( str ) ? str : str.UnescapeCILTypeString( 0, str.Length );
      }

      public static String EscapeCILTypeString( this String str )
      {
         return String.IsNullOrEmpty( str ) ? str : str.EscapeCILTypeString( 0, str.Length );
      }

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

      public static String CombineNamespaceAndType( String typeNamespace, String typeName )
      {
         return ( typeNamespace != null && typeNamespace.Length > 0 ? ( typeNamespace + NAMESPACE_SEPARATOR ) : "" ) + typeName;
      }

      public static String CombineEnclsosingAndNestedType( String enclosing, String nested )
      {
         return enclosing + NESTED_TYPE_SEPARATOR + nested;
      }
   }
}
