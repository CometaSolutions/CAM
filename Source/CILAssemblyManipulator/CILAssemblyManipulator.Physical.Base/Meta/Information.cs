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

namespace CILAssemblyManipulator.Physical.Meta
{
   public interface MetaDataTableInformationProvider
   {

      IEnumerable<MetaDataTableInformation> GetAllSupportedTableInformations();
   }

   public abstract class MetaDataTableInformation
   {
      internal MetaDataTableInformation(
         Int32 tableKind,
         System.Collections.IEqualityComparer equalityComparer,
         System.Collections.IComparer comparer
         )
      {
         ArgumentValidator.ValidateNotNull( "Equality comparer", equalityComparer );

         this.TableKind = tableKind;
         this.EqualityComparerNotGeneric = equalityComparer;
         this.ComparerNotGeneric = comparer;

      }

      public Int32 TableKind { get; }

      public System.Collections.IEqualityComparer EqualityComparerNotGeneric { get; }

      public System.Collections.IComparer ComparerNotGeneric { get; }

      public abstract ArrayQuery<MetaDataColumnInformation> ColumnsInformationNotGeneric { get; }

      public abstract MetaDataTable CreateMetaDataTableNotGeneric( Int32 capacity );

      public abstract Object CreateRowNotGeneric();
   }

   public class MetaDataTableInformation<TRow> : MetaDataTableInformation
      where TRow : class
   {
      public MetaDataTableInformation(
         Int32 tableKind,
         IEqualityComparer<TRow> equalityComparer,
         IComparer<TRow> comparer,
         Func<TRow> rowFactory,
         IEnumerable<MetaDataColumnInformation<TRow>> columns
         )
         : base( tableKind, new EqualityComparerWrapper<TRow>( equalityComparer ), comparer == null ? null : new ComparerWrapper<TRow>( comparer ) )
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

      public IEqualityComparer<TRow> EqualityComparer { get; }

      public IComparer<TRow> Comparer { get; }

      public MetaDataTable<TRow> CreateMetaDataTable( Int32 capacity )
      {
         return new Implementation.MetaDataTableImpl<TRow>( this, capacity );
      }

      public TRow CreateRow()
      {
         return this.RowFactory();
      }

      public sealed override Object CreateRowNotGeneric()
      {
         return this.CreateRow();
      }

      public ArrayQuery<MetaDataColumnInformation<TRow>> ColumnsInformation { get; }

      public sealed override MetaDataTable CreateMetaDataTableNotGeneric( Int32 capacity )
      {
         return this.CreateMetaDataTable( capacity );
      }

      public sealed override ArrayQuery<MetaDataColumnInformation> ColumnsInformationNotGeneric
      {
         get
         {
            return this.ColumnsInformation;
         }
      }

      protected Func<TRow> RowFactory { get; }
   }


   public abstract class MetaDataColumnInformation
   {
      internal MetaDataColumnInformation()
      {
      }

      public abstract Object GetterNotGeneric( Object row, out Boolean success );

      public abstract Boolean SetterNotGeneric( Object row, Object value );

      public abstract Type RowType { get; }

      public abstract Type ValueType { get; }


   }

   public abstract class MetaDataColumnInformation<TRow> : MetaDataColumnInformation
      where TRow : class
   {


      internal MetaDataColumnInformation()
      {
      }

      public abstract Object Getter( TRow row, out Boolean success );

      public abstract Boolean Setter( TRow row, Object value );

      public sealed override Object GetterNotGeneric( Object row, out Boolean success )
      {
         return this.Getter( row as TRow, out success );
      }

      public sealed override Boolean SetterNotGeneric( Object row, Object value )
      {
         return this.Setter( row as TRow, value );
      }

      public sealed override Type RowType
      {
         get
         {
            return typeof( TRow );
         }
      }
   }

   public abstract class MetaDataColumnInformation<TRow, TValue> : MetaDataColumnInformation<TRow>
      where TRow : class
   {

      internal MetaDataColumnInformation()
      {
      }

      public sealed override Type ValueType
      {
         get
         {
            return typeof( TValue );
         }
      }
   }

   public delegate TValue RowColumnGetterDelegate<TRow, TValue>( TRow row )
      where TRow : class;

   public delegate Boolean RowColumnSetterDelegate<TRow, TValue>( TRow row, TValue value )
      where TRow : class;

   public class MetaDataColumnInformationForClassesOrStructs<TRow, TValue> : MetaDataColumnInformation<TRow, TValue>
      where TRow : class
   {
      private readonly RowColumnGetterDelegate<TRow, TValue> _getter;
      private readonly RowColumnSetterDelegate<TRow, TValue> _setter;

      public MetaDataColumnInformationForClassesOrStructs(
         RowColumnGetterDelegate<TRow, TValue> getter,
         RowColumnSetterDelegate<TRow, TValue> setter
         )
      {
         ArgumentValidator.ValidateNotNull( "Column value getter", getter );
         ArgumentValidator.ValidateNotNull( "Column value setter", setter );

         this._getter = getter;
         this._setter = setter;

      }

      public sealed override Object Getter( TRow row, out Boolean success )
      {
         success = row != null;
         return success ? (Object) this._getter( row ) : null;
      }

      public sealed override Boolean Setter( TRow row, Object value )
      {
         // TODO maybe do specific class for structs? Is 'is' operator a lot faster when it is known at compile-time that it is struct?
         return row != null && value is TValue && this._setter( row, (TValue) value );
      }
   }

   public class MetaDataColumnInformationForNullables<TRow, TValue> : MetaDataColumnInformation<TRow, TValue?>
      where TRow : class
      where TValue : struct
   {

      private readonly RowColumnGetterDelegate<TRow, TValue?> _getter;
      private readonly RowColumnSetterDelegate<TRow, TValue?> _setter;

      public MetaDataColumnInformationForNullables(
         RowColumnGetterDelegate<TRow, TValue?> getter,
         RowColumnSetterDelegate<TRow, TValue?> setter
         )
      {
         ArgumentValidator.ValidateNotNull( "Column value getter", getter );
         ArgumentValidator.ValidateNotNull( "Column value setter", setter );

         this._getter = getter;
         this._setter = setter;
      }

      public sealed override Object Getter( TRow row, out Boolean success )
      {
         success = row != null;
         return success ? (Object) this._getter( row ) : null;
      }

      public sealed override Boolean Setter( TRow row, Object value )
      {
         var success = row != null;
         if ( success )
         {
            // Boxed nulls will show up as normal nulls
            if ( value == null )
            {
               this._setter( row, null );
            }
            // Otherwise, this is not null, and "is X" returns true when something is of type X? ( https://msdn.microsoft.com/en-us/library/ms366789.aspx )
            else if ( value is TValue )
            {
               this._setter( row, (TValue) value );
            }
            // Otherwise, this is of wrong type
            else
            {
               success = false;
            }
         }

         return success;
      }
   }
}
