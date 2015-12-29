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
using CILAssemblyManipulator.Physical.Meta;
using CommonUtils;

namespace CILAssemblyManipulator.Physical.Implementation
{
   public abstract class CILMetaDataBaseImpl : CILMetaDataBase
   {
      // Fixed tables will be stored as separate fields in subclasses


      private readonly MetaDataTable[] _additionalTables;
      private readonly Int32 _amountOfFixedTables;

      public CILMetaDataBaseImpl(
         MetaDataTableInformationProvider tableInfoProvider,
         Int32 amountOfFixedTables,
         Int32[] sizes,
         out MetaDataTableInformation[] infos
         )
      {
         ArgumentValidator.ValidateNotNull( "Table information provider", tableInfoProvider );

         if ( amountOfFixedTables < 0 )
         {
            throw new ArgumentException( "Amount of fixed tables has to be at least zero." );
         }

         this._amountOfFixedTables = amountOfFixedTables;

         infos = tableInfoProvider
            .GetAllSupportedTableInformations()
            .Where( i => i != null )
            .ToArray_SelfIndexing_Overwrite( i => i.TableKind, len => new MetaDataTableInformation[Math.Max( len, amountOfFixedTables )] );

         // Populate additional tables
         if ( infos.Length > amountOfFixedTables )
         {
            this._additionalTables = new MetaDataTable[infos.Length - amountOfFixedTables];
            for ( var i = 0; i < this._additionalTables.Length; ++i )
            {
               var tableValue = i + amountOfFixedTables;
               var capacity = sizes != null && tableValue < sizes.Length ? sizes[tableValue] : 0;
               this._additionalTables[i] = infos[tableValue]?.CreateMetaDataTableNotGeneric( capacity );

            }
         }

         // Subclass ctor will populate fixed tables
      }

      public abstract IEnumerable<MetaDataTable> GetFixedTables();

      public IEnumerable<MetaDataTable> GetAdditionalTables()
      {
         var additionalTables = this._additionalTables;
         if ( additionalTables != null )
         {
            foreach ( var table in additionalTables )
            {
               if ( table != null )
               {
                  yield return table;
               }
            }
         }
      }

      public Boolean TryGetByTable( Int32 index, out MetaDataTable table )
      {
         return index >= this._amountOfFixedTables ? this.TryGetAdditionalTable( index, out table ) : this.TryGetFixedTable( index, out table );
      }

      protected Boolean TryGetAdditionalTable( Int32 index, out MetaDataTable table )
      {
         var additionalTables = this._additionalTables;
         if ( additionalTables != null )
         {
            var additionalIndex = index - this._amountOfFixedTables;
            table = additionalIndex < 0 ? null : additionalTables[additionalIndex];
         }
         else
         {
            table = null;
         }
         return table != null;
      }

      protected abstract Boolean TryGetFixedTable( Int32 index, out MetaDataTable table );

      protected static MetaDataTable<TRow> CreateFixedMDTable<TRow>(
         Int32 table,
         Int32[] sizes,
         MetaDataTableInformation[] infos,
         ref MetaDataTableInformation[] defaultInfos,
         Func<MetaDataTableInformationProvider> defaultProviderCreator
         )
         where TRow : class
      {
         var info = infos[(Int32) table];
         if ( info == null )
         {
            if ( defaultInfos == null )
            {
               ArgumentValidator.ValidateNotNull( "Default provider creation callback", defaultProviderCreator );
               var defaultProvider = defaultProviderCreator();
               ArgumentValidator.ValidateNotNull( "Default provider", defaultProvider );
               defaultInfos = defaultProvider
                  .GetAllSupportedTableInformations()
                  // TODO maybe ToArray_SelfIndexing_Overwrite ? Or make this req explicit in docu.
                  .ToArray();


            }

            info = defaultInfos[(Int32) table];

            if ( info == null )
            {
               throw new InvalidOperationException( "The metadata table info provided for table " + table + " by default provider was null." );
            }
         }

         return (MetaDataTable<TRow>) info.CreateMetaDataTableNotGeneric( sizes != null && table < sizes.Length ? sizes[table] : 0 );
      }
   }

   internal sealed class MetaDataTableImpl<TRow> : MetaDataTable<TRow>
      where TRow : class
   {
      private readonly List<TRow> _table;

      internal MetaDataTableImpl(
         MetaDataTableInformation<TRow> tableInfo,
         Int32 tableRowCapacity
         )
      {
         ArgumentValidator.ValidateNotNull( "Table information", tableInfo );

         this.TableInformation = tableInfo;
         this.TableKind = tableInfo.TableKind;
         this._table = new List<TRow>( Math.Max( 0, tableRowCapacity ) );
      }

      public List<TRow> TableContents
      {
         get
         {
            return this._table;
         }
      }

      public Int32 TableKind { get; }

      public Int32 RowCount
      {
         get
         {
            return this._table.Count;
         }
      }

      public IEnumerable<Object> TableContentsAsEnumerable
      {
         get
         {
            return this._table;
         }
      }

      public MetaDataTableInformation<TRow> TableInformation { get; }
      public MetaDataTableInformation TableInformationNotGeneric
      {
         get
         {
            return this.TableInformation;
         }
      }

      public Object GetRowAt( Int32 idx )
      {
         return this._table[idx];
      }

      public Boolean TryAddRow( Object row )
      {
         var rowTyped = row as TRow;
         var retVal = rowTyped != null;
         if ( retVal )
         {
            this._table.Add( rowTyped );
         }
         return retVal;
      }

      public override String ToString()
      {
         return this.TableKind + ", row count: " + this._table.Count + ".";
      }
   }
}
