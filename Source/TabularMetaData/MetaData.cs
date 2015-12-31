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
using TabularMetaData;
using TabularMetaData.Meta;

// TODO change library name to SchemaBasedTabularMetaData or something like that

namespace TabularMetaData
{
   /// <summary>
   /// This interface is common interface for any metadata that is represented in tabular format.
   /// In most cases, this means CIL metadata.
   /// </summary>
   /// <remarks>
   /// Instead of directly implementing this interface, it is recommended to subclass the <see cref="TabularMetaDataWithSchemaImpl"/> class.
   /// </remarks>
   /// <seealso cref="MetaDataTable"/>
   /// <seealso cref="MetaDataTable{TRow}"/>
   public interface TabularMetaDataWithSchema
   {
      /// <summary>
      /// Gets all the tables, which are considred to be 'always present' in this meta data.
      /// Typically, these tables should be also accessible through separate properties.
      /// </summary>
      /// <returns>All the fixed tables of this meta data.</returns>
      IEnumerable<MetaDataTable> GetFixedTables();

      /// <summary>
      /// Gets all the tables, which are considered to be optional, either by evolution of this meta data format, or otherwise atypical.
      /// </summary>
      /// <returns>All the optional tables of this meta data.</returns>
      IEnumerable<MetaDataTable> GetAdditionalTables();

      /// <summary>
      /// Tries to retrieve a <see cref="MetaDataTable"/> at specified index.
      /// This <see cref="MetaDataTableInformation.TableIndex"/> of the resulting <see cref="MetaDataTable"/> will be the same as <paramref name="index"/>.
      /// </summary>
      /// <param name="index">The table index.</param>
      /// <param name="table">If successful, this variable will hold the <see cref="MetaDataTable"/> at given table index. Otherwise, it will <c>null</c>.</param>
      /// <returns>Returns <c>true</c> if table at given <paramref name="index"/> is present in this meta data; <c>false</c> otherwise.</returns>
      Boolean TryGetByTable( Int32 index, out MetaDataTable table );
   }

   /// <summary>
   /// This abstract class represents a single instance of metadata table, but so that the type of rows is not visible in this interface specification (through generics).
   /// The table may be queried and modified through this class.
   /// </summary>
   /// <remarks>
   /// The instances of this class may be created via <see cref="MetaDataTable{TRow}"/> class.
   /// </remarks>
   /// <seealso cref="MetaDataTable{TRow}"/>
   public abstract class MetaDataTable
   {
      // Don't allow subtypes from other assemblies to instantiate this class.
      internal MetaDataTable(
         Meta.MetaDataTableInformation tableInfo,
         System.Collections.IList rows
         )
      {
         ArgumentValidator.ValidateNotNull( "Table information", tableInfo );
         ArgumentValidator.ValidateNotNull( "Rows", rows );

         this.TableInformationNotGeneric = tableInfo;
         this.TableContentsNotGeneric = rows;
      }

      /// <summary>
      /// Tries to add a row to this table.
      /// </summary>
      /// <param name="row">The row to add.</param>
      /// <returns><c>true</c>, the row was added to this table, i.e. if <paramref name="row"/> is not <c>null</c>, and of correct type; <c>false</c> otherwise.</returns>
      public abstract Boolean TryAddRow( Object row );

      /// <summary>
      /// Gets information about this table and the its columns.
      /// </summary>
      /// <value>Information about this table and its columns.</value>
      /// <remarks>In essence, the <see cref="Meta.MetaDataTableInformation"/> captures the schema for this table, i.e. how many and what kind columns its rows can have.</remarks>
      public Meta.MetaDataTableInformation TableInformationNotGeneric { get; }

      /// <summary>
      /// Gets the non-generic <see cref="System.Collections.IList"/> holding the rows of this table.
      /// </summary>
      /// <value>The non-generic <see cref="System.Collections.IList"/> holding the rows of this table.</value>
      public System.Collections.IList TableContentsNotGeneric { get; }

   }

   /// <summary>
   /// This class represents a single instance of metadata table, so that type of the rows is known at compile-time through the generic argument of this class.
   /// </summary>
   /// <typeparam name="TRow">The type of the rows in this table.</typeparam>
   /// <remarks>
   /// The class-constraint for <typeparamref name="TRow"/> exists because the rows are meant to be mutable in-place.
   /// Furthermore, things are somewhat simpler in re-ordering algorithm when the rows are classes instead of structs.
   /// </remarks>
   public class MetaDataTable<TRow> : MetaDataTable
      where TRow : class
   {

      /// <summary>
      /// Creates a new instance of <see cref="MetaDataTable{TRow}"/> with given table information, and given table row capacity.
      /// </summary>
      /// <param name="tableInfo">The <see cref="Meta.MetaDataTableInformation{TRow}"/> to hold information about this table and its columns.</param>
      /// <param name="tableRowCapacity">The initial capacity of this metadata table. Negative values are ignored.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="tableInfo"/> is <c>null</c>.</exception>
      public MetaDataTable(
         Meta.MetaDataTableInformation<TRow> tableInfo,
         Int32 tableRowCapacity
         )
         : this( tableInfo, new List<TRow>( Math.Max( 0, tableRowCapacity ) ) )
      {

      }

      /// <summary>
      /// Creates a new instance of <see cref="MetaDataTable{TRow}"/> with given table information, and given list of rows.
      /// </summary>
      /// <param name="tableInfo">The <see cref="Meta.MetaDataTableInformation{TRow}"/> to hold information about this table and its columns.</param>
      /// <param name="rows">The existing rows. May be <c>null</c>, in that case, a new empty list is created.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="tableInfo"/> is <c>null</c>.</exception>
      public MetaDataTable(
         Meta.MetaDataTableInformation<TRow> tableInfo,
         List<TRow> rows
         )
         : base( tableInfo, ProcessRowsList( ref rows ) )
      {
         this.TableInformation = tableInfo;
         this.TableContents = rows;
      }

      /// <summary>
      /// Gets the direct access to the list of rows of this <see cref="MetaDataTable{TRow}"/>.
      /// </summary>
      /// <value>The direct access to the list of rows of this <see cref="MetaDataTable{TRow}"/>.</value>
      public List<TRow> TableContents { get; }

      /// <summary>
      /// Gets information about this table and the its columns.
      /// </summary>
      /// <value>Information about this table and its columns.</value>
      /// <remarks>In essence, the <see cref="Meta.MetaDataTableInformation{TRow}"/> captures the schema for this table, i.e. how many and what kind columns its rows can have.</remarks>
      public Meta.MetaDataTableInformation<TRow> TableInformation { get; }


      /// <inheritdoc />
      public sealed override Boolean TryAddRow( object row )
      {
         var rowTyped = row as TRow;
         var retVal = rowTyped != null;
         if ( retVal )
         {
            this.TableContents.Add( rowTyped );
         }
         return retVal;
      }

      private static List<TRow> ProcessRowsList( ref List<TRow> rows )
      {
         if ( rows == null )
         {
            rows = new List<TRow>();
         }

         return rows;
      }
   }

   /// <summary>
   /// This exception will be thrown by <see cref="TabularMetaDataWithSchemaImpl.CreateFixedMDTable"/> method in non-trivial error situations.
   /// </summary>
   public class FixedTableCreationException : Exception
   {
      /// <summary>
      /// Creates a new instance of <see cref="FixedTableCreationException"/> with given message and optional inner exception.
      /// </summary>
      /// <param name="msg">The message.</param>
      /// <param name="inner">The optional inner exception</param>
      public FixedTableCreationException( String msg, Exception inner = null )
         : base( msg, inner )
      {

      }
   }
}


/// <summary>
/// This class contains extensions methods for this library.
/// </summary>
public static partial class E_CILPhysicalBase
{
   /// <summary>
   /// Gets all the tables of given <see cref="TabularMetaDataWithSchema"/>.
   /// The fixed tables are returned first, followed by additional tables.
   /// </summary>
   /// <param name="md">The <see cref="TabularMetaDataWithSchema"/>.</param>
   /// <returns>All of the tables contained in <see cref="TabularMetaDataWithSchema"/>.</returns>
   /// <remarks>
   /// If <paramref name="md"/> is <c>null</c>, this returns empty enumerable, and does not throw.
   /// </remarks>
   public static IEnumerable<MetaDataTable> GetAllTables( this TabularMetaDataWithSchema md )
   {
      return md == null ? Empty<MetaDataTable>.Enumerable : md.GetFixedTables().Concat( md.GetAdditionalTables() );
   }

   /// <summary>
   /// Gets a metadata table with a given table index, or throws an exception, if table is not present.
   /// </summary>
   /// <param name="md">The <see cref="TabularMetaDataWithSchema"/>.</param>
   /// <param name="tableIndex">The table index.</param>
   /// <returns>The <see cref="MetaDataTable"/> at given <paramref name="tableIndex"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="md"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If <paramref name="md"/> does not have a table at given <paramref name="tableIndex"/>.</exception>
   public static MetaDataTable GetByTable( this TabularMetaDataWithSchema md, Int32 tableIndex )
   {
      MetaDataTable retVal;
      if ( !md.TryGetByTable( tableIndex, out retVal ) )
      {
         throw new ArgumentException( "Table " + tableIndex + " is invalid or unsupported." );
      }
      return retVal;
   }

   /// <summary>
   /// Gets the table index of given <see cref="MetaDataTable"/>.
   /// Calling this method is equivalent to calling <see cref="MetaDataTableInformation.TableIndex"/> of the <see cref="MetaDataTable.TableInformationNotGeneric"/> property.
   /// </summary>
   /// <param name="table">The <see cref="MetaDataTable"/>.</param>
   /// <returns>The table index of <paramref name="table"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="table"/> is <c>null</c>.</exception>
   public static Int32 GetTableIndex( this MetaDataTable table )
   {
      return table.TableInformationNotGeneric.TableIndex;
   }

   /// <summary>
   /// Gets the row count of given <see cref="MetaDataTable"/>.
   /// Calling this method is equivalent to calling <see cref="System.Collections.ICollection.Count"/> of the <see cref="MetaDataTable.TableContentsNotGeneric"/> property.
   /// </summary>
   /// <param name="table">The <see cref="MetaDataTable"/>.</param>
   /// <returns>The amount of rows in <paramref name="table"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="table"/> is <c>null</c>.</exception>
   public static Int32 GetRowCount( this MetaDataTable table )
   {
      return table.TableContentsNotGeneric.Count;
   }

   /// <summary>
   /// Gets the row count of given <see cref="MetaDataTable{TRow}"/>.
   /// Calling this method is equivalent to calling <see cref="ICollection{T}.Count"/> of the <see cref="MetaDataTable{TRow}.TableContents"/> property.
   /// </summary>
   /// <typeparam name="TRow">The type of rows in table.</typeparam>
   /// <param name="table">The <see cref="MetaDataTable{TRow}"/>.</param>
   /// <returns>The amount of rows in <paramref name="table"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="table"/> is <c>null</c>.</exception>
   public static Int32 GetRowCount<TRow>( this MetaDataTable<TRow> table )
      where TRow : class
   {
      return table.TableContents.Count;
   }
}