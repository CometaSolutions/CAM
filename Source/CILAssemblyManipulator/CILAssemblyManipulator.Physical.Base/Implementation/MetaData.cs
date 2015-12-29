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
   /// <summary>
   /// This is class providing default skeleton implementation for <see cref="TabularMetaDataWithSchema"/>.
   /// </summary>
   public abstract class TabularMetaDataWithSchemaImpl : TabularMetaDataWithSchema
   {
      // Fixed tables will be stored as separate fields in subclasses
      private readonly MetaDataTable[] _additionalTables;
      private readonly Int32 _amountOfFixedTables;

      /// <summary>
      /// Creates a new instance of <see cref="TabularMetaDataWithSchemaImpl"/>.
      /// </summary>
      /// <param name="tableInfoProvider">The <see cref="MetaDataTableInformationProvider"/>.</param>
      /// <param name="amountOfFixedTables">The amount of fixed tables this metadata has.</param>
      /// <param name="sizes">The initial capacities of tables.</param>
      /// <param name="infos">This parameter will hold the <see cref="MetaDataTableInformation"/>s returned by <paramref name="tableInfoProvider"/>, ordered so that element at index <c>x</c> will be the one with <see cref="MetaDataTableInformation.TableIndex"/> property as <c>x</c>.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="tableInfoProvider"/> is <c>null</c>.</exception>
      /// <exception cref="ArgumentException">If <paramref name="amountOfFixedTables"/> is less than <c>0</c>.</exception>
      public TabularMetaDataWithSchemaImpl(
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
            .ToArray_SelfIndexing_Overwrite( i => i.TableIndex, len => new MetaDataTableInformation[Math.Max( len, amountOfFixedTables )] );

         // Populate additional tables
         if ( infos.Length > amountOfFixedTables )
         {
            this._additionalTables = new MetaDataTable[infos.Length - amountOfFixedTables];
            for ( var i = 0; i < this._additionalTables.Length; ++i )
            {
               var tableValue = i + amountOfFixedTables;
               var info = infos[tableValue];
               if ( info != null )
               {
                  var capacity = sizes != null && tableValue < sizes.Length ? sizes[tableValue] : 0;
                  this._additionalTables[i] = info.CreateMetaDataTableNotGeneric( capacity );
               }

            }
         }

         // Subclass ctor will populate fixed tables
      }

      /// <summary>
      /// This method should be implemented by subclasses to return fixed tables in desired order.
      /// </summary>
      /// <returns>The fixed tables supported by metadata.</returns>
      public abstract IEnumerable<MetaDataTable> GetFixedTables();

      /// <inheritdoc />
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

      /// <inheritdoc />
      public Boolean TryGetByTable( Int32 index, out MetaDataTable table )
      {
         return index >= this._amountOfFixedTables ? this.TryGetAdditionalTable( index, out table ) : this.TryGetFixedTable( index, out table );
      }

      /// <inheritdoc />
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

      /// <summary>
      /// This method should be implemented by sublcasses to try to retrieve fixed table with given index.
      /// </summary>
      /// <param name="index">The table index.</param>
      /// <param name="table">The table, or <c>null</c>, if no fixed table exists with given index.</param>
      /// <returns><c>true</c> if table with given <paramref name="index"/> exists; <c>false</c> otherwise.</returns>
      protected abstract Boolean TryGetFixedTable( Int32 index, out MetaDataTable table );

      /// <summary>
      /// This is helper method to be used by subclasses when the fixed tables are instantiated.
      /// The <see cref="MetaDataTableInformation"/> for resulting <see cref="MetaDataTable{TRow}"/> is first tested to be in <paramref name="infos"/>.
      /// If the element in <paramref name="infos"/> at index <paramref name="table"/> is <c>null</c>, the <paramref name="defaultInfos"/> is used to look up <see cref="MetaDataTableInformation"/>.
      /// If <paramref name="defaultInfos"/> is <c>null</c>, the <paramref name="defaultProviderCreator"/> callback is then used to create <see cref="MetaDataTableInformationProvider"/> and get the array of <see cref="MetaDataTableInformation"/>.
      /// </summary>
      /// <typeparam name="TRow">The type of the rows of the table.</typeparam>
      /// <param name="table">The table index.</param>
      /// <param name="sizes">The initial capacities array.</param>
      /// <param name="infos">The array of <see cref="MetaDataTableInformation"/>, as given by <see cref="TabularMetaDataWithSchemaImpl(MetaDataTableInformationProvider, int, int[], out MetaDataTableInformation[])"/> constructor.</param>
      /// <param name="defaultInfos">The array of default <see cref="MetaDataTableInformation"/>.</param>
      /// <param name="defaultProviderCreator">The callback to create default <see cref="MetaDataTableInformationProvider"/>, which will be used to create <paramref name="defaultInfos"/>.</param>
      /// <returns>A non-<c>null</c> instance of <see cref="MetaDataTable{TRow}"/>.</returns>
      /// <exception cref="InvalidOperationException">If the resolved <see cref="MetaDataTableInformation"/> or the <see cref="MetaDataTable{TRow}"/> that it returns is <c>null</c>.</exception>
      protected static MetaDataTable<TRow> CreateFixedMDTable<TRow>(
         Int32 table,
         Int32[] sizes,
         MetaDataTableInformation[] infos,
         ref MetaDataTableInformation[] defaultInfos,
         Func<MetaDataTableInformationProvider> defaultProviderCreator
         )
         where TRow : class
      {
         var info = infos[table];
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

            info = defaultInfos[table];

            if ( info == null )
            {
               throw new InvalidOperationException( "The metadata table info provided for table " + table + " by default provider was null." );
            }
         }

         var retVal = (MetaDataTable<TRow>) info.CreateMetaDataTableNotGeneric( sizes != null && table < sizes.Length ? sizes[table] : 0 );
         if ( retVal == null )
         {
            throw new InvalidOperationException( "The metadata table created by metadata table info was null." );
         }

         return retVal;
      }
   }
}
