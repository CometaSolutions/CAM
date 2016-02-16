/*
* Copyright 2016 Stanislav Muhametsin. All rights Reserved.
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
extern alias CAMPhysical;
extern alias CAMPhysicalR;
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;
using CAMPhysical::CILAssemblyManipulator.Physical.Meta;

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.Meta;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TabularMetaData;
using TabularMetaData.Meta;

public static partial class E_CILPhysical
{
   /// <summary>
   /// Gets the zero-based metadata token (table + zero-based index value encoded in integer) for this <see cref="TableIndex"/>.
   /// </summary>
   /// <param name="index">The <see cref="TableIndex"/>.</param>
   /// <returns>The zero-based metadata token for this <see cref="TableIndex"/>.</returns>
   public static Int32 GetZeroBasedToken( this TableIndex index )
   {
      return ( index.CombinedValue & CAMCoreInternals.INDEX_MASK ) | ( index.CombinedValue & ~CAMCoreInternals.INDEX_MASK );
   }

   /// <summary>
   /// Gets the one-based metadata token (table + one-based index value encoded in integer) for this <see cref="TableIndex"/>.
   /// </summary>
   /// <param name="index">The <see cref="TableIndex"/>.</param>
   /// <returns>The one-based metadata token for this <see cref="TableIndex"/>.</returns>
   public static Int32 GetOneBasedToken( this TableIndex index )
   {
      return ( ( index.CombinedValue & CAMCoreInternals.INDEX_MASK ) + 1 ) | ( index.CombinedValue & ~CAMCoreInternals.INDEX_MASK );
   }

   /// <summary>
   ///  Gets the one-based metadata token (table + one-based index value encoded in integer) for this nullable <see cref="TableIndex"/>.
   /// </summary>
   /// <param name="tableIdx">The nullable<see cref="TableIndex"/>.</param>
   /// <returns>The one-based metadata token for this <see cref="TableIndex"/>, or <c>0</c> if this nullable <see cref="TableIndex"/> does not have a value.</returns>
   public static Int32 GetOneBasedToken( this TableIndex? tableIdx )
   {
      return tableIdx.HasValue ?
         tableIdx.Value.GetOneBasedToken() :
         0;
   }
}