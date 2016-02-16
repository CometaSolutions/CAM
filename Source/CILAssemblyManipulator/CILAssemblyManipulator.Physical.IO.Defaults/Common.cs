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
extern alias CAMPhysical;
extern alias CAMPhysicalR;
extern alias CAMPhysicalIO;

using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;

using CAMPhysicalIO;
using CAMPhysicalIO::CILAssemblyManipulator.Physical.IO;
using CILAssemblyManipulator.Physical.IO.Defaults;
using CollectionsWithRoles.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TRVAList = CollectionsWithRoles.API.ArrayQuery<System.Int64>;

namespace CILAssemblyManipulator.Physical.IO.Defaults
{
   /// <summary>
   /// This is base class to store values (such as integers) now, and use the values later.
   /// The raw value storage has a pre-set capacity, which can not changed, and can only be filled once.
   /// </summary>
   /// <typeparam name="TValue">The type of the values to store.</typeparam>
   public sealed class ColumnValueStorage<TValue>
   {
      private readonly Int32[] _tableColCount;
      private readonly Int32[] _tableStartOffsets;
      private readonly TValue[] _rawValues;
      private Int32 _currentIndex;

      /// <summary>
      /// Creates a new instance of <see cref="ColumnValueStorage{TValue}"/> with given information about table sizes and raw value column count for each table.
      /// </summary>
      /// <param name="tableSizes">The table size array. The index of the array is value of <see cref="Tables"/> enumeration, and the value in that array is the size of that table. So if <see cref="Tables.Module"/> would have 1 element, the element at index <c>0</c> (value of <see cref="Tables.Module"/>) would be <c>1</c>.</param>
      /// <param name="rawColumnInfo">The count of raw value columns for each table. The index of the array is value of <see cref="Tables"/> enumeration, and the value in that array is the raw column count. Since <see cref="Tables.MethodDef"/> has one raw value column (the method IL RVA), the element at index <c>6</c> (value of <see cref="Tables.MethodDef"/>) would be <c>1</c>.</param>
      public ColumnValueStorage(
         ArrayQuery<Int32> tableSizes,
         IEnumerable<Int32> rawColumnInfo
         )
      {
         this.TableSizes = tableSizes;
         this._tableColCount = rawColumnInfo.ToArray();
         this._tableStartOffsets = tableSizes
            .AggregateIntermediate_BeforeAggregation(
               0,
               ( cur, size, idx ) => cur += size * this._tableColCount[idx]
               )
            .ToArray();
         this._rawValues = new TValue[tableSizes.Select( ( size, idx ) => size * this._tableColCount[idx] ).Sum()];
         this._currentIndex = 0;
      }

      /// <summary>
      /// Appends raw value to the end of the list of the raw values.
      /// </summary>
      /// <param name="rawValue">The raw value to append.</param>
      /// <exception cref="IndexOutOfRangeException">If this <see cref="ColumnValueStorage{TValue}"/> has already been filled.</exception>
      public void AddRawValue( TValue rawValue )
      {
         this._rawValues[this._currentIndex++] = rawValue;
      }

      /// <summary>
      /// Gets all raw values for a given column in a given table.
      /// </summary>
      /// <param name="table">The <see cref="Tables"/> value.</param>
      /// <param name="columnIndex">The raw column index among all the raw columns in <paramref name="table"/>.</param>
      /// <returns>Enumerable of all raw values for given column.</returns>
      public IEnumerable<TValue> GetAllRawValuesForColumn( Tables table, Int32 columnIndex )
      {
         var colCount = this._tableColCount[(Int32) table];
         var start = this._tableStartOffsets[(Int32) table] + columnIndex;
         var max = start + this.TableSizes[(Int32) table] * colCount;
         for ( var i = start; i < max; i += colCount )
         {
            yield return this._rawValues[i];
         }
      }

      /// <summary>
      /// Gets all raw values for a given row in a given table.
      /// </summary>
      /// <param name="table">The <see cref="Tables"/> value.</param>
      /// <param name="rowIndex">The zero-based index of the row.</param>
      /// <returns>Enumerable of all raw values for given row.</returns>
      public IEnumerable<TValue> GetAllRawValuesForRow( Tables table, Int32 rowIndex )
      {
         var size = this._tableColCount[(Int32) table];
         var startOffset = this._tableStartOffsets[(Int32) table] + rowIndex * size;
         for ( var i = 0; i < size; ++i )
         {
            yield return this._rawValues[startOffset];
            ++startOffset;
         }
      }

      /// <summary>
      /// Gets raw value for a given row in a given table, at a given column index.
      /// </summary>
      /// <param name="table">The <see cref="Tables"/> value.</param>
      /// <param name="rowIndex">The zero-based row index.</param>
      /// <param name="columnIndex">The zero-based column index amongst all raw value columns in the table.</param>
      /// <returns>The value previously stored at specified table, row, and column.</returns>
      public TValue GetRawValue( Tables table, Int32 rowIndex, Int32 columnIndex )
      {
         return this._rawValues[this.GetArrayIndex( (Int32) table, rowIndex, columnIndex )];
      }

      /// <summary>
      /// Sets raw value for a given row in a given table, at a given column index.
      /// </summary>
      /// <param name="table">The <see cref="Tables"/> value.</param>
      /// <param name="rowIndex">The zero-based row index.</param>
      /// <param name="columnIndex">The zero-based column index amongst all raw value columns in the table.</param>
      /// <param name="value">The value to set.</param>
      public void SetRawValue( Tables table, Int32 rowIndex, Int32 columnIndex, TValue value )
      {
         this._rawValues[this.GetArrayIndex( (Int32) table, rowIndex, columnIndex )] = value;
      }

      /// <summary>
      /// Gets the enumerable representing tables which have at least one storable column value specified.
      /// </summary>
      /// <returns>The enumerable representing tables which have at least one storable column value specified.</returns>
      public IEnumerable<Tables> GetPresentTables()
      {
         var tableColCount = this._tableColCount;
         for ( var i = 0; i < tableColCount.Length; ++i )
         {
            if ( tableColCount[i] > 0 )
            {
               yield return (Tables) i;
            }
         }
      }

      /// <summary>
      /// Gets the amount of stored column values for a specific table.
      /// </summary>
      /// <param name="table">The table.</param>
      /// <returns>The amount of stored column values for a specific table.</returns>
      public Int32 GetStoredColumnsCount( Tables table )
      {
         return this._tableColCount[(Int32) table];
      }

      public ArrayQuery<Int32> TableSizes { get; }

      private Int32 GetArrayIndex( Int32 table, Int32 row, Int32 col )
      {
         return this._tableStartOffsets[table] + row * this._tableColCount[table] + col;
      }

   }
}

public static partial class E_CILPhysical
{
   /// <summary>
   /// This is extension method to create a new instance of <see cref="DataReferencesInfo"/> from this <see cref="MetaDataTableStreamHeader"/> with given enumerable of <see cref="DataReferenceInfo"/>s.
   /// </summary>
   /// <param name="valueStorage">The <see cref="MetaDataTableStreamHeader"/>.</param>
   /// <returns>A new instance of <see cref="DataReferencesInfo"/> with information extracted from given enumerable of <see cref="DataReferencesInfo"/>s.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="MetaDataTableStreamHeader"/> is <c>null</c>.</exception>
   public static DataReferencesInfo CreateDataReferencesInfo<T>( this ColumnValueStorage<T> valueStorage, Func<T, Int64> converter )
   {

      var cf = CollectionsWithRoles.Implementation.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY;
      var dic = new Dictionary<
         Tables,
         ArrayQuery<ArrayQuery<Int64>>
         >();
      foreach ( var tbl in valueStorage.GetPresentTables() )
      {
         var dataRefsColCount = valueStorage.GetStoredColumnsCount( tbl );
         if ( dataRefsColCount > 0 )
         {
            var arr = new ArrayQuery<Int64>[dataRefsColCount];
            var tSize = valueStorage.TableSizes[(Int32) tbl];
            for ( var i = 0; i < dataRefsColCount; ++i )
            {
               // TODO: to CommonUtils: public static T[] FillFromEnumerable<T>(this IEnumerable<T>, Int32 size, SizeMismatchStrategy = Ignore | ThrowIfArraySmaller | ThrowIfArrayGreater )
               var values = new Int64[tSize];
               var idx = 0;
               foreach ( var val in valueStorage.GetAllRawValuesForColumn( tbl, i ) )
               {
                  values[idx++] = converter( val );
               }
               arr[i] = cf.NewArrayProxy( values ).CQ;
            }
            dic.Add( tbl, cf.NewArrayProxy( arr ).CQ );
         }
      }

      return new DataReferencesInfo( cf.NewDictionaryProxy( dic ).CQ );
   }

}