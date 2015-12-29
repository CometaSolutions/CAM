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
using System.IO;
using System.Linq;
using System.Text;
using CommonUtils;
using CILAssemblyManipulator.Physical;



namespace CILAssemblyManipulator.Physical
{
   /// <summary>
   /// This interface is common interface for any metadata that is represented in tabular format.
   /// In most cases, this means CIL metadata.
   /// </summary>
   /// <seealso cref="MetaDataTable"/>
   public interface CILMetaDataBase
   {
      IEnumerable<MetaDataTable> GetFixedTables();

      IEnumerable<MetaDataTable> GetAdditionalTables();

      Boolean TryGetByTable( Int32 index, out MetaDataTable table );
   }

   public interface MetaDataTable
   {
      Int32 TableKind { get; }

      Int32 RowCount { get; }

      Object GetRowAt( Int32 idx );

      IEnumerable<Object> TableContentsAsEnumerable { get; }

      Boolean TryAddRow( Object row );

      Meta.MetaDataTableInformation TableInformationNotGeneric { get; }
   }

   public interface MetaDataTable<TRow> : MetaDataTable
      where TRow : class
   {
      List<TRow> TableContents { get; }

      Meta.MetaDataTableInformation<TRow> TableInformation { get; }
   }
}


public static partial class E_CILPhysicalBase
{
   public static IEnumerable<MetaDataTable> GetAllTables( this CILMetaDataBase md )
   {
      return md.GetFixedTables().Concat( md.GetAdditionalTables() );
   }

   public static MetaDataTable GetByTable( this CILMetaDataBase md, Int32 tableKind )
   {
      MetaDataTable retVal;
      if ( !md.TryGetByTable( tableKind, out retVal ) )
      {
         throw new ArgumentException( "Table " + tableKind + " is invalid or unsupported." );
      }
      return retVal;
   }
}