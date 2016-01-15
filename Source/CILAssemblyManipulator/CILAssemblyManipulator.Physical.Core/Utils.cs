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
using CollectionsWithRoles.API;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TabularMetaData;

namespace CILAssemblyManipulator.Physical
{

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
   }

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
