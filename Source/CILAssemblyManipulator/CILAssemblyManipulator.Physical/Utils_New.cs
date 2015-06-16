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

      private const String TYPE_ASSEMBLY_SEPARATOR = ", ";

      public static Boolean ParseFullTypeString( this String str, out String typeString, out String assemblyString )
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

      private static Boolean GetFirstSeparatorsFromFullTypeString( this String str, String separatorString, out Int32 firstLength )
      {
         var strMax = 0;
         var curIdx = -1;
         while ( strMax < str.Length
            && ( curIdx = str.IndexOf( separatorString, strMax ) ) > 0
            && str[curIdx - 1] == InternalExtensions.ESCAPE_CHAR )
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
   }
}
