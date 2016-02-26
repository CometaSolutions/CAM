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
using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TabularMetaData.Meta;

namespace TabularMetaData.Meta
{
   /// <summary>
   /// This interface is used to create <see cref="MetaDataTableInformation"/>s when creating new <see cref="TabularMetaDataWithSchema"/>s.
   /// </summary>
   public interface MetaDataTableInformationProvider
   {
      /// <summary>
      /// This method is used to obtain all <see cref="MetaDataTableInformation"/> instances that will indicate the present tables in <see cref="TabularMetaDataWithSchema"/>.
      /// Any <c>null</c> values will be filtered out.
      /// </summary>
      /// <returns>The <see cref="MetaDataTableInformation"/> instances. The order does not matter.</returns>
      IEnumerable<MetaDataTableInformation> GetAllSupportedTableInformations();
   }

   /// <summary>
   /// This class encapsulates some general information about metadata table, and about the columns it has.
   /// </summary>
   /// <remarks>
   /// Instances of this class may not be created directly, instead use <see cref="MetaDataTableInformation{TRow}"/> class.
   /// </remarks>
   /// <seealso cref="MetaDataTableInformation{TRow}"/>
   /// <seealso cref="MetaDataColumnInformation"/>
   public abstract class MetaDataTableInformation
   {
      // Disable instatiation of this class from other assemblies.
      internal MetaDataTableInformation(
         Int32 tableKind,
         System.Collections.IEqualityComparer equalityComparer,
         System.Collections.IComparer comparer
         )
      {
         ArgumentValidator.ValidateNotNull( "Equality comparer", equalityComparer );

         this.TableIndex = tableKind;
         this.EqualityComparerNotGeneric = equalityComparer;
         this.ComparerNotGeneric = comparer;

      }

      /// <summary>
      /// Get the unique table index, that the instances of metadata tables will be found at, with <see cref="TabularMetaDataWithSchema.TryGetByTable(int, out MetaDataTable)"/> method.
      /// </summary>
      /// <value>The unique table index for instances of metadata tables.</value>
      public Int32 TableIndex { get; }

      /// <summary>
      /// Gets the non-generic version of equality comparer for rows of instances of metadata tables.
      /// </summary>
      /// <value>The non-generic version of equality comparer for rows of instances of metadata tables.</value>
      /// <remarks>The returned value is always non-<c>null</c>.</remarks>
      public System.Collections.IEqualityComparer EqualityComparerNotGeneric { get; }

      /// <summary>
      /// Gets the non-generic of comparer for rows of instances of metadata tables.
      /// </summary>
      /// <value>The non-generic of comparer for rows of instances of metadta tables.</value>
      /// <remarks>The returned value may be <c>null</c>.</remarks>
      public System.Collections.IComparer ComparerNotGeneric { get; }

      /// <summary>
      /// Gets the immutable array of <see cref="MetaDataColumnInformation"/>, containing information about the columns of instances of metadata tables.
      /// </summary>
      /// <value>the immutable array of <see cref="MetaDataColumnInformation"/>, containing information about the columns of instances of metadata tables.</value>
      /// <remarks>The returned value is always non-<c>null</c>, and each element is always non-<c>null</c>.</remarks>
      public abstract ArrayQuery<MetaDataColumnInformation> ColumnsInformationNotGeneric { get; }

      /// <summary>
      /// Creates a new instance of <see cref="MetaDataTable"/> that will support the schema that this <see cref="MetaDataTableInformation"/> exposes.
      /// </summary>
      /// <param name="capacity">The initial capacity of returned <see cref="MetaDataTable"/>.</param>
      /// <returns>A new instance of <see cref="MetaDataTable"/> that will support the schema that this <see cref="MetaDataTableInformation"/> exposes.</returns>
      public abstract MetaDataTable CreateMetaDataTableNotGeneric( Int32 capacity );

      /// <summary>
      /// Creates a new instance of row, that can be added to the <see cref="MetaDataTable"/> created by this <see cref="MetaDataTableInformation"/>.
      /// </summary>
      /// <returns>A new instance of row, that can be added to the <see cref="MetaDataTable"/> created by this <see cref="MetaDataTableInformation"/>.</returns>
      public abstract Object CreateRowNotGeneric();
   }

   /// <summary>
   /// This class further specializes the <see cref="MetaDataTableInformation"/> by constraining the type of the rows that may be present in the <see cref="MetaDataTable"/> created with this <see cref="MetaDataTableInformation{TRow}"/>.
   /// </summary>
   /// <typeparam name="TRow">The type of the rows that may be present in the <see cref="MetaDataTable"/> created with this <see cref="MetaDataTableInformation{TRow}"/>.</typeparam>
   /// <seealso cref="MetaDataColumnInformation{TRow}"/>
   public class MetaDataTableInformation<TRow> : MetaDataTableInformation
      where TRow : class
   {
      /// <summary>
      /// Creates a new instance of <see cref="MetaDataTableInformation{TRow}"/>,
      /// </summary>
      /// <param name="tableIndex">The unique (within the context of similar <see cref="TabularMetaDataWithSchema"/>s) table index for the to store the tables.</param>
      /// <param name="equalityComparer">The equality comparer for rows.</param>
      /// <param name="comparer">The comparer for rows.</param>
      /// <param name="rowFactory">The callback to create a new row.</param>
      /// <param name="columns">The information about columns that this table information exposes.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="equalityComparer"/>, <paramref name="rowFactory"/>, or <paramref name="columns"/> is <c>null</c>. Also thrown if any item in <paramref name="columns"/> is <c>null</c>.</exception>
      /// <exception cref="ArgumentException">If <paramref name="columns"/> is empty.</exception>
      public MetaDataTableInformation(
         Int32 tableIndex,
         IEqualityComparer<TRow> equalityComparer,
         IComparer<TRow> comparer,
         Func<TRow> rowFactory,
         IEnumerable<MetaDataColumnInformation<TRow>> columns
         )
         : base( tableIndex, new EqualityComparerWrapper<TRow>( equalityComparer ), comparer == null ? null : ( ( comparer as System.Collections.IComparer ) ?? new ComparerWrapper<TRow>( comparer ) ) )
      {
         ArgumentValidator.ValidateNotNull( "Row factory", rowFactory );
         ArgumentValidator.ValidateNotNull( "Columns", columns );

         this.EqualityComparer = equalityComparer;
         this.Comparer = comparer;
         this.RowFactory = rowFactory;

         this.ColumnsInformation = columns.ToArrayProxy().CQ;
         ArgumentValidator.ValidateAllNotNull( "Columns", this.ColumnsInformation );
         if ( this.ColumnsInformation.Count <= 0 )
         {
            throw new ArgumentException( "Table must have at least one column." );
         }
      }

      /// <summary>
      /// Gets the equality comparer for rows of instances of metadata tables.
      /// </summary>
      /// <value>The equality comparer for rows of instances of metadata tables.</value>
      public IEqualityComparer<TRow> EqualityComparer { get; }

      /// <summary>
      /// Gets the comparer for rows of instances of metadata tables.
      /// </summary>
      /// <value>The comparer for rows of instances of metadata tables.</value>
      public IComparer<TRow> Comparer { get; }

      /// <summary>
      /// Creates a new <see cref="MetaDataTable{TRow}"/> with given initial capacity.
      /// </summary>
      /// <param name="capacity">The inital capacity for <see cref="MetaDataTable{TRow}"/>.</param>
      /// <returns>A new <see cref="MetaDataTable{TRow}"/> with given inital capacity.</returns>
      /// <remarks>
      /// Subclasses may override this method to return customized instances of <see cref="MetaDataTable{TRow}"/>.
      /// </remarks>
      public virtual MetaDataTable<TRow> CreateMetaDataTable( Int32 capacity )
      {
         return new MetaDataTable<TRow>( this, capacity );
      }

      /// <summary>
      /// Creates a new instance of row, that can be added to the <see cref="MetaDataTable"/> created by this <see cref="MetaDataTableInformation"/>.
      /// </summary>
      /// <returns>A new instance of row, that can be added to the <see cref="MetaDataTable"/> created by this <see cref="MetaDataTableInformation"/>.</returns>
      public TRow CreateRow()
      {
         return this.RowFactory();
      }

      /// <inheritdoc />
      public sealed override Object CreateRowNotGeneric()
      {
         return this.CreateRow();
      }

      /// <summary>
      /// Gets the immutable array of <see cref="MetaDataColumnInformation{TRow}"/>, containing information about the columns of instances of metadata tables.
      /// </summary>
      /// <value>the immutable array of <see cref="MetaDataColumnInformation{TRow}"/>, containing information about the columns of instances of metadata tables.</value>
      /// <remarks>The returned value is always non-<c>null</c>, and each element is always non-<c>null</c>.</remarks>
      public ArrayQuery<MetaDataColumnInformation<TRow>> ColumnsInformation { get; }

      /// <inheritdoc />
      public sealed override MetaDataTable CreateMetaDataTableNotGeneric( Int32 capacity )
      {
         return this.CreateMetaDataTable( capacity );
      }

      /// <inheritdoc />
      public sealed override ArrayQuery<MetaDataColumnInformation> ColumnsInformationNotGeneric
      {
         get
         {
            return this.ColumnsInformation;
         }
      }

      /// <summary>
      /// Gets the callback used in <see cref="CreateRow"/> method.
      /// </summary>
      /// <value>The callback used in <see cref="CreateRow"/> method.</value>
      protected Func<TRow> RowFactory { get; }
   }

   /// <summary>
   /// This class encapsulates information about a single column of a single metadata table.
   /// </summary>
   /// <remarks>
   /// The instances of this class may not be instantiated directly, instead use <see cref="MetaDataColumnInformation{TRow, TValue}"/>.
   /// </remarks>
   /// <seealso cref="MetaDataTableInformation"/>
   /// <seealso cref="MetaDataColumnInformation{TRow}"/>
   /// <seealso cref="MetaDataColumnInformation{TRow, TValue}"/>
   public abstract class MetaDataColumnInformation
   {
      private readonly DictionaryProxy<Type, Lazy<Object>> _functionalities;

      // Disable instatiation of this class from other assemblies.
      internal MetaDataColumnInformation()
      {
         this._functionalities = new Dictionary<Type, Lazy<Object>>().ToDictionaryProxy();
      }

      /// <summary>
      /// This method will get the value corresponding to this column from given row.
      /// </summary>
      /// <param name="row">The row to get value from.</param>
      /// <param name="success">This will be <c>true</c>, if <paramref name="row"/> was not <c>null</c> and was of correct row type.</param>
      /// <returns>The value corresponding to this column from given <paramref name="row"/>. It may be <c>null</c>, use <paramref name="success"/> to differentiate between <c>null</c> column value and error situations.</returns>
      public abstract Object GetterNotGeneric( Object row, out Boolean success );

      /// <summary>
      /// This method will set the value corresponding to this column from given row.
      /// </summary>
      /// <param name="row">The row to set value.</param>
      /// <param name="value">The new value.</param>
      /// <returns><c>true</c>, if setting value is successful, i.e. <paramref name="row"/> and <paramref name="value"/> are both of correct types, <paramref name="row"/> is not <c>null</c>, and domain-specific additional checks pass; <c>false</c> otherwise.</returns>
      public abstract Boolean SetterNotGeneric( Object row, Object value );

      /// <summary>
      /// Gets the type of the row that this column is associated with.
      /// </summary>
      /// <value>the type of the row that this column is associated with.</value>
      public abstract Type RowType { get; }

      /// <summary>
      /// Gets the type of the accepted column values.
      /// </summary>
      /// <value>The type of the accepted column values.</value>
      public abstract Type ValueType { get; }

      /// <summary>
      /// Registers a certain type of functionality for this <see cref="MetaDataColumnInformation"/>, with lazy initialization of functionality.
      /// </summary>
      /// <typeparam name="TFunctionality">The type of the functionality.</typeparam>
      /// <param name="functionality">The callback to create an instance of functionality.</param>
      /// <returns><c>true</c> if <paramref name="functionality"/> was not <c>null</c> and registered; <c>false</c> otherwise.</returns>
      public Boolean RegisterFunctionality<TFunctionality>( Func<TFunctionality> functionality )
         where TFunctionality : class
      {
         var retVal = functionality != null;
         if ( retVal )
         {
            this._functionalities[typeof( TFunctionality )] = new Lazy<Object>( functionality, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );
         }
         return retVal;
      }

      /// <summary>
      /// Registers a certain type of functionality for this <see cref="MetaDataColumnInformation"/>, when the functionality is already created.
      /// </summary>
      /// <typeparam name="TFunctionality">The type of the functionality.</typeparam>
      /// <param name="functionality">The instance of functionality.</param>
      /// <returns><c>true</c> if <paramref name="functionality"/> was not <c>null</c> and registered; <c>false</c> otherwise.</returns>
      public Boolean RegisterFunctionalityDirect<TFunctionality>( TFunctionality functionality )
         where TFunctionality : class
      {
         return functionality != null && this.RegisterFunctionality<TFunctionality>( () => functionality );
      }

      /// <summary>
      /// Gets all the functionalities for this <see cref="MetaDataColumnInformation"/>.
      /// </summary>
      /// <value>All the functionalities for this <see cref="MetaDataColumnInformation"/>.</value>
      public DictionaryQuery<Type, Lazy<Object>> Functionalities
      {
         get
         {
            return this._functionalities.CQ;
         }
      }
   }

   /// <summary>
   /// This class further specializes the <see cref="MetaDataColumnInformation"/> by constraining the type of the row that the column may be attached to.
   /// </summary>
   /// <typeparam name="TRow">The type of the row this column can be attached to.</typeparam>
   /// <remarks>
   /// The instances of this class may not be instantiated directly, instead use <see cref="MetaDataColumnInformation{TRow, TValue}"/>.
   /// </remarks>
   /// <seealso cref="MetaDataTableInformation"/>
   /// <seealso cref="MetaDataColumnInformation{TRow}"/>
   /// <seealso cref="MetaDataColumnInformation{TRow, TValue}"/>
   public abstract class MetaDataColumnInformation<TRow> : MetaDataColumnInformation
      where TRow : class
   {

      // Disable instatiation of this class from other assemblies.
      internal MetaDataColumnInformation()
      {
      }

      /// <summary>
      /// This method will get the value corresponding to this column from given row.
      /// </summary>
      /// <param name="row">The row to get value from.</param>
      /// <param name="success">This will be <c>true</c>, if <paramref name="row"/> was not <c>null</c>.</param>
      /// <returns>The value corresponding to this column from given <paramref name="row"/>. It may be <c>null</c>, use <paramref name="success"/> to differentiate between <c>null</c> column value and error situations.</returns>

      public abstract Object Getter( TRow row, out Boolean success );

      /// <summary>
      /// This method will set the value corresponding to this column from given row.
      /// </summary>
      /// <param name="row">The row to set value.</param>
      /// <param name="value">The new value.</param>
      /// <returns><c>true</c>, if setting value is successful, i.e. <paramref name="value"/> is of correct type, <paramref name="row"/> is not <c>null</c>, and domain-specific additional checks pass; <c>false</c> otherwise.</returns>
      public abstract Boolean Setter( TRow row, Object value );

      /// <inheritdoc />
      public sealed override Object GetterNotGeneric( Object row, out Boolean success )
      {
         return this.Getter( row as TRow, out success );
      }

      /// <inheritdoc />
      public sealed override Boolean SetterNotGeneric( Object row, Object value )
      {
         return this.Setter( row as TRow, value );
      }

      /// <inheritdoc />
      public sealed override Type RowType
      {
         get
         {
            return typeof( TRow );
         }
      }
   }

   /// <summary>
   /// This class specializes the <see cref="MetaDataColumnInformation{TRow}"/> by constraining the type of the values that the column can hold.
   /// </summary>
   /// <typeparam name="TRow">The type of the row this column can be attached to.</typeparam>
   /// <typeparam name="TValue">The type of the values that the column can hold.</typeparam>

   /// <seealso cref="MetaDataTableInformation"/>
   /// <seealso cref="MetaDataColumnInformation{TRow}"/>
   /// <seealso cref="MetaDataColumnInformation{TRow, TValue}"/>
   public sealed class MetaDataColumnInformation<TRow, TValue> : MetaDataColumnInformation<TRow>
      where TRow : class
   {
      private readonly RowColumnGetterDelegate<TRow, TValue> _getter;
      private readonly RowColumnSetterDelegate<TRow, TValue> _setter;
      private readonly Boolean _acceptsNulls;

      /// <summary>
      /// Creates a new instance of <see cref="MetaDataColumnInformation{TRow, TValue}"/> with given callbacks to get and set column value.
      /// </summary>
      /// <param name="getter">The callback to get row value.</param>
      /// <param name="setter">The callback to set row value.</param>
      /// <seealso cref="RowColumnGetterDelegate{TRow, TValue}"/>
      /// <seealso cref="RowColumnSetterDelegate{TRow, TValue}"/>
      public MetaDataColumnInformation(
         RowColumnGetterDelegate<TRow, TValue> getter,
         RowColumnSetterDelegate<TRow, TValue> setter
         )
      {
         // <remarks>
         // The instances of this class may not be instantiated directly, instead use <see cref="MetaDataColumnInformationForClassesOrStructs{TRow, TValue}"/> or <see cref="MetaDataColumnInformationForNullables{TRow, TValue}"/> classes.
         // </remarks>

         ArgumentValidator.ValidateNotNull( "Column value getter", getter );
         ArgumentValidator.ValidateNotNull( "Column value setter", setter );

         this._getter = getter;
         this._setter = setter;
         var type = typeof( TValue );
         this._acceptsNulls = !type.IsValueType || type.IsNullable();
      }

      /// <inheritdoc />
      public sealed override Type ValueType
      {
         get
         {
            return typeof( TValue );
         }
      }

      /// <inheritdoc />
      public sealed override Object Getter( TRow row, out Boolean success )
      {
         success = row != null;
         return success ? (Object) this._getter( row ) : null;
      }

      /// <inheritdoc />
      public sealed override Boolean Setter( TRow row, Object value )
      {
         // TODO maybe do specific class for structs? Is 'is' operator a lot faster when it is known at compile-time that it is struct?
         return row != null
            && ( ( this._acceptsNulls && value == null ) || value is TValue )
            && this._setter( row, (TValue) value );
      }
   }

   /// <summary>
   /// This delegate is used by <see cref="MetaDataColumnInformation{TRow, TValue}"/> to retrieve the column value from a row.
   /// </summary>
   /// <typeparam name="TRow">The type of the row.</typeparam>
   /// <typeparam name="TValue">The type of the column value.</typeparam>
   /// <param name="row">The row. Guaranteed to be non-<c>null</c>.</param>
   /// <returns>The row's value corresponding to the column in question.</returns>
   public delegate TValue RowColumnGetterDelegate<in TRow, out TValue>( TRow row )
      where TRow : class;

   /// <summary>
   /// This delegate is used by <see cref="MetaDataColumnInformation{TRow, TValue}"/> to set the column value of a row.
   /// </summary>
   /// <typeparam name="TRow">The type of the row.</typeparam>
   /// <typeparam name="TValue">The type of the column value.</typeparam>
   /// <param name="row">The row. Guaranteed to be non-<c>null</c>.</param>
   /// <param name="value">The value to set.</param>
   /// <returns><c>true</c> if value passed domain-specific checks, and was set to <paramref name="row"/>; <c>false</c> otherwise.</returns>
   public delegate Boolean RowColumnSetterDelegate<in TRow, in TValue>( TRow row, TValue value )
      where TRow : class;

   ///// <summary>
   ///// This class implements the <see cref="MetaDataColumnInformation{TRow, TValue}"/> for columns accepting structs or classes.
   ///// If the column value is a nullable type, the <see cref="MetaDataColumnInformationForNullables{TRow, TValue}"/> should be used.
   ///// </summary>
   ///// <typeparam name="TRow">The type of the row this column can be attached to.</typeparam>
   ///// <typeparam name="TValue">The type of the values that the column can hold.</typeparam>
   ///// <seealso cref="MetaDataTableInformation"/>
   ///// <seealso cref="MetaDataColumnInformation{TRow}"/>
   ///// <seealso cref="MetaDataColumnInformation{TRow, TValue}"/>
   ///// <seealso cref="MetaDataColumnInformationForNullables{TRow, TValue}"/>
   //public class MetaDataColumnInformationForClassesOrStructs<TRow, TValue> : MetaDataColumnInformation<TRow, TValue>
   //   where TRow : class
   //{
   //   private readonly RowColumnGetterDelegate<TRow, TValue> _getter;
   //   private readonly RowColumnSetterDelegate<TRow, TValue> _setter;
   //   private readonly Boolean _acceptsNulls;

   //   /// <summary>
   //   /// Creates a new instance of <see cref="MetaDataColumnInformationForClassesOrStructs{TRow, TValue}"/> with given callbacks to get and set column value.
   //   /// </summary>
   //   /// <param name="getter">The callback to get row value.</param>
   //   /// <param name="setter">The callback to set row value.</param>
   //   /// <seealso cref="RowColumnGetterDelegate{TRow, TValue}"/>
   //   /// <seealso cref="RowColumnSetterDelegate{TRow, TValue}"/>
   //   public MetaDataColumnInformationForClassesOrStructs(
   //      RowColumnGetterDelegate<TRow, TValue> getter,
   //      RowColumnSetterDelegate<TRow, TValue> setter
   //      )
   //   {
   //      ArgumentValidator.ValidateNotNull( "Column value getter", getter );
   //      ArgumentValidator.ValidateNotNull( "Column value setter", setter );

   //      this._getter = getter;
   //      this._setter = setter;
   //      this._acceptsNulls = typeof( TValue ).IsValueType;
   //   }

   //   /// <inheritdoc />
   //   public sealed override Object Getter( TRow row, out Boolean success )
   //   {
   //      success = row != null;
   //      return success ? (Object) this._getter( row ) : null;
   //   }

   //   /// <inheritdoc />
   //   public sealed override Boolean Setter( TRow row, Object value )
   //   {
   //      // TODO maybe do specific class for structs? Is 'is' operator a lot faster when it is known at compile-time that it is struct?
   //      return row != null
   //         && ((this._acceptsNulls && value == null) || value is TValue )
   //         && this._setter( row, (TValue) value );
   //   }
   //}

   ///// <summary>
   ///// This class implements the <see cref="MetaDataColumnInformation{TRow, TValue}"/> for columns accepting nullable types.
   ///// If the column value is not a nullable type, the <see cref="MetaDataColumnInformationForClassesOrStructs{TRow, TValue}"/> should be used.
   ///// </summary>
   ///// <typeparam name="TRow">The type of the row this column can be attached to.</typeparam>
   ///// <typeparam name="TValue">The nullable type of the values that the column can hold. E.g. if column holds value <c>Int32?</c>, this should be <c>Int32</c>.</typeparam>
   ///// <seealso cref="MetaDataTableInformation"/>
   ///// <seealso cref="MetaDataColumnInformation{TRow}"/>
   ///// <seealso cref="MetaDataColumnInformation{TRow, TValue}"/>
   ///// <seealso cref="MetaDataColumnInformationForClassesOrStructs{TRow, TValue}"/>
   //public class MetaDataColumnInformationForNullables<TRow, TValue> : MetaDataColumnInformation<TRow, TValue?>
   //   where TRow : class
   //   where TValue : struct
   //{

   //   private readonly RowColumnGetterDelegate<TRow, TValue?> _getter;
   //   private readonly RowColumnSetterDelegate<TRow, TValue?> _setter;

   //   /// <summary>
   //   /// Creates a new instance of <see cref="MetaDataColumnInformationForNullables{TRow, TValue}"/> with given callbacks to get and set column value.
   //   /// </summary>
   //   /// <param name="getter">The callback to get row value.</param>
   //   /// <param name="setter">The callback to set row value.</param>
   //   /// <seealso cref="RowColumnGetterDelegate{TRow, TValue}"/>
   //   /// <seealso cref="RowColumnSetterDelegate{TRow, TValue}"/>
   //   public MetaDataColumnInformationForNullables(
   //      RowColumnGetterDelegate<TRow, TValue?> getter,
   //      RowColumnSetterDelegate<TRow, TValue?> setter
   //      )
   //   {
   //      ArgumentValidator.ValidateNotNull( "Column value getter", getter );
   //      ArgumentValidator.ValidateNotNull( "Column value setter", setter );

   //      this._getter = getter;
   //      this._setter = setter;
   //   }

   //   /// <inheritdoc />
   //   public sealed override Object Getter( TRow row, out Boolean success )
   //   {
   //      success = row != null;
   //      return success ? (Object) this._getter( row ) : null;
   //   }

   //   /// <inheritdoc />
   //   public sealed override Boolean Setter( TRow row, Object value )
   //   {
   //      var success = row != null;
   //      if ( success )
   //      {
   //         // Boxed nulls will show up as normal nulls
   //         if ( value == null )
   //         {
   //            this._setter( row, null );
   //         }
   //         // Otherwise, this is not null, and "is X" returns true when something is of type X? ( https://msdn.microsoft.com/en-us/library/ms366789.aspx )
   //         else if ( value is TValue )
   //         {
   //            this._setter( row, (TValue) value );
   //         }
   //         // Otherwise, this is of wrong type
   //         else
   //         {
   //            success = false;
   //         }
   //      }

   //      return success;
   //   }
   //}
}

public static partial class E_TabularMetaData
{
   /// <summary>
   /// Helper method to get functionality when the type of functionality is known at compile time.
   /// </summary>
   /// <typeparam name="TFunctionality">The type of the functionality.</typeparam>
   /// <param name="info">The <see cref="MetaDataColumnInformation"/>.</param>
   /// <returns>The functionality, or <c>null</c> if functionality is not found.</returns>
   /// <exception cref="NullReferenceException">If the <see cref="MetaDataColumnInformation"/> is <c>null</c>.</exception>
   public static TFunctionality GetFunctionality<TFunctionality>( this MetaDataColumnInformation info )
      where TFunctionality : class
   {
      return info.GetFunctionality( typeof( TFunctionality ) ) as TFunctionality;
   }

   /// <summary>
   /// Helpe rmethod to get functionality when the type of functionality is not known at compile time.
   /// </summary>
   /// <param name="info">The <see cref="MetaDataColumnInformation"/>.</param>
   /// <param name="functionalityType">The type of the functionality.</param>
   /// <returns>The functionality, or <c>null</c> if functionality is not found.</returns>
   /// <exception cref="NullReferenceException">If the <see cref="MetaDataColumnInformation"/> is <c>null</c>.</exception>
   public static Object GetFunctionality( this MetaDataColumnInformation info, Type functionalityType )
   {
      Lazy<Object> retVal;
      return functionalityType != null && info.Functionalities.TryGetValue( functionalityType, out retVal ) ?
         retVal.Value :
         null;
   }
}