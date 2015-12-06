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
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.IO.Defaults;
using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO.Defaults
{
   public class DefaultRVAConverter : RVAConverter
   {
      private readonly SectionHeader[] _sections;

      public DefaultRVAConverter( IEnumerable<SectionHeader> headers )
      {
         this._sections = ( headers ?? Empty<SectionHeader>.Enumerable ).ToArray();
      }

      public Int64 ToOffset( Int64 rva )
      {
         // TODO some kind of interval-map for sections...
         var sections = this._sections;
         var retVal = -1L;
         if ( rva > 0 )
         {
            for ( var i = 0; i < sections.Length; ++i )
            {
               var sec = sections[i];
               if ( sec.VirtualAddress <= rva && rva < (Int64) sec.VirtualAddress + (Int64) Math.Max( sec.VirtualSize, sec.RawDataSize ) )
               {
                  retVal = sec.RawDataPointer + ( rva - sec.VirtualAddress );
                  break;
               }
            }
         }
         return retVal;
      }

      public Int64 ToRVA( Int64 offset )
      {
         // TODO some kind of interval-map for sections...
         var sections = this._sections;
         var retVal = -1L;
         if ( offset > 0 )
         {
            for ( var i = 0; i < sections.Length; ++i )
            {
               var sec = sections[i];
               if ( sec.RawDataPointer <= offset && offset < (Int64) sec.RawDataPointer + (Int64) sec.RawDataSize )
               {
                  retVal = sec.VirtualAddress + ( offset - sec.RawDataPointer );
                  break;
               }
            }
         }

         return retVal;
      }
   }

   public abstract class DefaultColumnSerializationInfo
   {
      internal DefaultColumnSerializationInfo()
      {

      }
   }


   public abstract class DefaultColumnSerializationInfo<TRow> : DefaultColumnSerializationInfo
      where TRow : class
   {
      internal DefaultColumnSerializationInfo()
         : base()
      {

      }
   }

   public delegate ColumnSerializationFunctionality CreateSerializationSupportDelegate( ColumnSerializationSupportCreationArgs args );

   // Sets a property on a raw row
   public delegate void RawRowColumnSetterDelegate<TRawRow>( TRawRow rawRow, Int32 value );

   // Sets a property on a normal row
   public delegate void RowColumnSetterDelegate<TRow, TValue>( ColumnFunctionalityArgs<TRow, RowReadingArguments> args, TValue value )
      where TRow : class;

   // Sets a raw value property (Method IL, FieldRVA Data column, Manifest Resource Data) on a normal row
   public delegate void RowRawColumnSetterDelegate<TRow>( ColumnFunctionalityArgs<TRow, RawValueProcessingArgs> args, Int32 rawValue )
      where TRow : class;

   // Gets the heap index that will be written to table stream
   public delegate Int32 RowHeapColumnGetterDelegate<TRow>( ColumnFunctionalityArgs<TRow, RowHeapFillingArguments> args )
      where TRow : class;

   // Gets the const value that will be written to table stream
   public delegate TValue RowColumnGetterDelegate<TRow, TValue>( TRow row );

   public delegate SectionPartWithRVAs RawColumnSectionPartCreationDelegte<TRow>( CILMetaData md, WriterMetaDataStreamContainer mdStreamContainer );


   public class DefaultColumnSerializationInfo<TRawRow, TRow> : DefaultColumnSerializationInfo<TRow>
      where TRawRow : class
      where TRow : class
   {

      public DefaultColumnSerializationInfo(
         String columnName,
         CreateSerializationSupportDelegate serializationCreator,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, Int32> setter,
         RowColumnGetterDelegate<TRow, Int32> constExtractor
         )
         : this(
        columnName,
        serializationCreator,
        rawSetter,
        setter,
        null,
        null,
        constExtractor,
        null
        )
      {

      }

      public DefaultColumnSerializationInfo(
         String columnName,
         CreateSerializationSupportDelegate serializationCreator,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, Int32> setter,
         RowRawColumnSetterDelegate<TRow> rawValueProcessor,
         RawColumnSectionPartCreationDelegte<TRow> rawColummnSectionPartCreator
         )
         : this(
              columnName,
              serializationCreator,
              rawSetter,
              setter,
              rawValueProcessor,
              null,
              null,
              rawColummnSectionPartCreator
              )
      {

      }

      public DefaultColumnSerializationInfo(
         String columnName,
         HeapIndexKind heapIndexKind,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, Int32> setter,
         RowHeapColumnGetterDelegate<TRow> heapValueExtractor
         )
         : this(
              columnName,
              args => args.IsWide( heapIndexKind ) ? ColumnSerializationSupport_Constant32.Instance : ColumnSerializationSupport_Constant16.Instance,
              rawSetter,
              setter,
              null,
              heapValueExtractor,
              null,
              null
              )
      {
         ArgumentValidator.ValidateNotNull( "Heap value extractor", heapValueExtractor );
      }

      protected DefaultColumnSerializationInfo(
         String columnName,
         CreateSerializationSupportDelegate creator,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, Int32> setter,
         RowRawColumnSetterDelegate<TRow> rawValueProcessor,
         RowHeapColumnGetterDelegate<TRow> heapValueExtractor,
         RowColumnGetterDelegate<TRow, Int32> constExtractor,
         RawColumnSectionPartCreationDelegte<TRow> rawColummnSectionPartCreator
         )
      {
         ArgumentValidator.ValidateNotNull( "Column name", columnName );
         ArgumentValidator.ValidateNotNull( "Raw setter", rawSetter );
         ArgumentValidator.ValidateNotNull( "Serialization support creator", creator );
         if ( setter == null )
         {
            ArgumentValidator.ValidateNotNull( "Raw value processor", rawValueProcessor );
            ArgumentValidator.ValidateNotNull( "Raw value section part creator", rawColummnSectionPartCreator );
         }
         else
         {
            ArgumentValidator.ValidateNotNull( "Setter", setter );
         }


         this.ColumnName = columnName;
         this.RawSetter = rawSetter;
         this.Setter = setter;
         this.SerializationSupportCreator = creator;
         this.RawColummnSectionPartCreator = rawColummnSectionPartCreator;
         this.RawValueProcessor = rawValueProcessor;
         this.HeapValueExtractor = heapValueExtractor;
         this.ConstantExtractor = constExtractor;
      }

      public String ColumnName { get; }
      public CreateSerializationSupportDelegate SerializationSupportCreator { get; }

      // Reading
      public RawRowColumnSetterDelegate<TRawRow> RawSetter { get; }
      public RowColumnSetterDelegate<TRow, Int32> Setter { get; }
      public RowRawColumnSetterDelegate<TRow> RawValueProcessor { get; }

      // Writing
      public RawColumnSectionPartCreationDelegte<TRow> RawColummnSectionPartCreator { get; }
      public RowHeapColumnGetterDelegate<TRow> HeapValueExtractor { get; }
      public RowColumnGetterDelegate<TRow, Int32> ConstantExtractor { get; }
   }

   public interface ColumnFunctionalityInfo<TDelegate, TArgs>
   {
      TDelegate CreateDelegate( TArgs args, Tables table, Int32 columnIndex );
   }

   public sealed class StaticColumnFunctionalityInfo<TDelegate, TArgs> : ColumnFunctionalityInfo<TDelegate, TArgs>
   {
      private readonly TDelegate _delegate;

      public StaticColumnFunctionalityInfo( TDelegate del )
      {
         this._delegate = del;
      }

      public TDelegate CreateDelegate( TArgs args, Tables table, Int32 columnIndex )
      {
         return this._delegate;
      }
   }

   public sealed class DynamicColumnFunctionalityInfo<TDelegate, TArgs> : ColumnFunctionalityInfo<TDelegate, TArgs>
   {
      private readonly Func<TArgs, Tables, Int32, TDelegate> _creator;

      public DynamicColumnFunctionalityInfo( Func<TArgs, Tables, Int32, TDelegate> creator )
      {
         this._creator = creator;
      }

      public TDelegate CreateDelegate( TArgs args, Tables table, Int32 columnIndex )
      {
         return this._creator( args, table, columnIndex );
      }
   }


   public static class DefaultColumnSerializationInfoFactory
   {
      public static Int32 GetCodedTableSize( ArrayQuery<Int32> tableSizes, ArrayQuery<Tables?> referencedTables )
      {
         Int32 max = 0;
         var len = referencedTables.Count;
         for ( var i = 0; i < len; ++i )
         {
            var current = referencedTables[i];
            if ( current.HasValue )
            {
               max = Math.Max( max, tableSizes[(Int32) current.Value] );
            }
         }
         return max < ( UInt16.MaxValue >> CodedTableIndexDecoder.GetTagBitSize( referencedTables.Count ) ) ?
            2 :
            4;
      }


      public static DefaultColumnSerializationInfo<TRawRow, TRow> Constant8<TRawRow, TRow>(
         String columnName,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, Int32> setter,
         RowColumnGetterDelegate<TRow, Int32> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => ColumnSerializationSupport_Constant8.Instance,
            rawSetter,
            setter,
            getter
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> Constant16<TRawRow, TRow>(
         String columnName,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, Int32> setter,
         RowColumnGetterDelegate<TRow, Int32> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => ColumnSerializationSupport_Constant16.Instance,
            rawSetter,
            setter,
            getter
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> Constant32<TRawRow, TRow>(
         String columnName,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, Int32> setter,
         RowColumnGetterDelegate<TRow, Int32> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => ColumnSerializationSupport_Constant32.Instance,
            rawSetter,
            setter,
            getter
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> SimpleReference<TRawRow, TRow>(
         String columnName,
         Tables targetTable,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, TableIndex> setter,
         RowColumnGetterDelegate<TRow, TableIndex> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => args.TableSizes[(Int32) targetTable] >= UInt16.MaxValue ? ColumnSerializationSupport_Constant32.Instance : ColumnSerializationSupport_Constant16.Instance,
            rawSetter,
            ( args, value ) =>
            {
               if ( value != 0 )
               {
                  setter( args, new TableIndex( targetTable, value - 1 ) );
               }
            },
            row => getter( row ).Index + 1
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> CodedReference<TRawRow, TRow>(
         String columnName,
         ArrayQuery<Tables?> targetTables,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, TableIndex?> setter,
         RowColumnGetterDelegate<TRow, TableIndex?> getter
         )
         where TRawRow : class
         where TRow : class
      {
         var decoder = new CodedTableIndexDecoder( targetTables );

         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => GetCodedTableSize( args.TableSizes, targetTables ) < sizeof( Int32 ) ? ColumnSerializationSupport_Constant16.Instance : ColumnSerializationSupport_Constant32.Instance,
            rawSetter,
            ( args, value ) => setter( args, decoder.DecodeTableIndex( value ) ),
            row => decoder.EncodeTableIndex( getter( row ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBCustom<TRawRow, TRow>(
         String columnName,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         Action<ColumnFunctionalityArgs<TRow, RowReadingArguments>, Int32, ReaderBLOBStreamHandler> setter, // TODO delegat-ize these
         Func<ColumnFunctionalityArgs<TRow, RowHeapFillingArguments>, Byte[]> blobCreator
         )
         where TRawRow : class
         where TRow : class
      {
         return HeapIndex<TRawRow, TRow>(
            columnName,
            HeapIndexKind.BLOB,
            rawSetter,
            ( args, value ) => setter( args, value, args.RowArgs.MDStreamContainer.BLOBs ),
            ( args ) => args.RowArgs.MDStreamContainer.BLOBs.RegisterBLOB( blobCreator( args ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> GUID<TRawRow, TRow>(
         String columnName,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, Guid?> setter,
         RowColumnGetterDelegate<TRow, Guid?> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return HeapIndex<TRawRow, TRow>(
            columnName,
            HeapIndexKind.GUID,
            rawSetter,
            ( args, value ) => setter( args, args.RowArgs.MDStreamContainer.GUIDs.GetGUID( value ) ),
            args => args.RowArgs.MDStreamContainer.GUIDs.RegisterGUID( getter( args.Row ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> SystemString<TRawRow, TRow>(
         String columnName,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, String> setter,
         RowColumnGetterDelegate<TRow, String> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return HeapIndex<TRawRow, TRow>(
            columnName,
            HeapIndexKind.String,
            rawSetter,
            ( args, value ) => setter( args, args.RowArgs.MDStreamContainer.SystemStrings.GetString( value ) ),
            args => args.RowArgs.MDStreamContainer.SystemStrings.RegisterString( getter( args.Row ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> HeapIndex<TRawRow, TRow>(
         String columnName,
         HeapIndexKind heapKind,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, Int32> setter,
         RowHeapColumnGetterDelegate<TRow> heapValueExtractor
         )
         where TRawRow : class
         where TRow : class
      {

         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            heapKind,
            rawSetter,
            ( args, value ) =>
            {
               if ( value != 0 )
               {
                  setter( args, value );
               }
            },
            heapValueExtractor
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> RawValueStorageColumn<TRawRow, TRow>(
         String columnName,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowRawColumnSetterDelegate<TRow> rawValueProcessor,
         RawColumnSectionPartCreationDelegte<TRow> rawColummnSectionPartCreator
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => ColumnSerializationSupport_Constant32.Instance,
            rawSetter,
            null,
            rawValueProcessor,
            rawColummnSectionPartCreator
         );
      }


   }

   public sealed class CodedTableIndexDecoder
   {

      public static Int32 GetTagBitSize( Int32 referencedTablesLength )
      {
         return BinaryUtils.Log2( (UInt32) referencedTablesLength - 1 ) + 1;
      }

      private readonly ArrayQuery<Tables?> _tablesArray;
      private readonly IDictionary<Tables, Int32> _tablesDictionary;
      private readonly Int32 _tagBitMask;
      private readonly Int32 _tagBitSize;

      public CodedTableIndexDecoder(
         ArrayQuery<Tables?> possibleTables
         )
      {
         ArgumentValidator.ValidateNotNull( "Possible tables", possibleTables );

         this._tablesArray = possibleTables;
         this._tablesDictionary = possibleTables
            .Select( ( t, idx ) => Tuple.Create( t, idx ) )
            .Where( t => t.Item1.HasValue )
            .ToDictionary_Preserve( t => t.Item1.Value, t => t.Item2 );
         this._tagBitSize = GetTagBitSize( possibleTables.Count );
         this._tagBitMask = ( 1 << this._tagBitSize ) - 1;
      }

      public TableIndex? DecodeTableIndex( Int32 codedIndex )
      {
         TableIndex? retVal;
         var tableIndex = this._tagBitMask & codedIndex;
         if ( tableIndex < this._tablesArray.Count )
         {
            var tableNullable = this._tablesArray[tableIndex];
            if ( tableNullable.HasValue )
            {
               var rowIdx = ( ( (UInt32) codedIndex ) >> this._tagBitSize );
               retVal = rowIdx > 0 ?
                  new TableIndex( tableNullable.Value, (Int32) ( rowIdx - 1 ) ) :
                  (TableIndex?) null;
            }
            else
            {
               retVal = null;
            }
         }
         else
         {
            retVal = null;
         }

         return retVal;
      }

      public Int32 EncodeTableIndex( TableIndex? tableIndex )
      {
         Int32 retVal;
         if ( tableIndex.HasValue )
         {
            var tIdxValue = tableIndex.Value;
            Int32 tableArrayIndex;
            retVal = this._tablesDictionary.TryGetValue( tIdxValue.Table, out tableArrayIndex ) ?
               ( ( ( tIdxValue.Index + 1 ) << this._tagBitSize ) | tableArrayIndex ) :
               0;
         }
         else
         {
            retVal = 0;
         }

         return retVal;
      }

   }

   public struct ColumnFunctionalityArgs<TRow, TRowArgs>
      where TRow : class
      where TRowArgs : class
   {

      public ColumnFunctionalityArgs(
         Int32 rowIndex,
         TRow row,
         TRowArgs args
         )
      {
         ArgumentValidator.ValidateNotNull( "Row", row );
         ArgumentValidator.ValidateNotNull( "Row arguments", args );

         this.RowIndex = rowIndex;
         this.Row = row;
         this.RowArgs = args;
      }

      public Int32 RowIndex { get; }

      public TRow Row { get; }

      public TRowArgs RowArgs { get; }
   }

   public partial class DefaultMetaDataSerializationSupportProvider : MetaDataSerializationSupportProvider
   {
      private static readonly DefaultMetaDataSerializationSupportProvider _instance = new DefaultMetaDataSerializationSupportProvider();

      public static MetaDataSerializationSupportProvider Instance
      {
         get
         {
            return _instance;
         }
      }

      public virtual IEnumerable<TableSerializationInfo> CreateTableSerializationInfos(
         IEnumerable<Meta.MetaDataTableInformation> tableInfos
         )
      {
         return tableInfos.Select( info => info.TableSerializationInfoNotGeneric );
      }
   }

   public class DefaultTableSerializationInfo<TRawRow, TRow> : TableSerializationInfo
      where TRawRow : class
      where TRow : class
   {

      private readonly DefaultColumnSerializationInfo<TRawRow, TRow>[] _columns;
      private readonly Func<TRow> _rowFactory;
      private readonly Func<TRawRow> _rawRowFactory;

      public DefaultTableSerializationInfo(
         Tables table,
         Boolean isSorted,
         IEnumerable<DefaultColumnSerializationInfo<TRawRow, TRow>> columns,
         Func<TRow> rowFactory,
         Func<TRawRow> rawRowFactory
         )
      {
         ArgumentValidator.ValidateNotNull( "Columns", columns );
         ArgumentValidator.ValidateNotNull( "Row factory", rowFactory );
         ArgumentValidator.ValidateNotNull( "Raw row factory", rawRowFactory );

         this.Table = table;
         this.IsSorted = isSorted;
         this._rowFactory = rowFactory;
         this._rawRowFactory = rawRowFactory;
         this._columns = columns.ToArray();
      }

      public Tables Table { get; }

      public Boolean IsSorted { get; }

      public Int32 RawValueStorageColumnCount
      {
         get
         {
            return this._columns.Count( c => c.RawValueProcessor != null );
         }
      }


      public Int32 HeapValueColumnCount
      {
         get
         {
            return this._columns.Count( c => c.HeapValueExtractor != null );
         }
      }

      public void ProcessRowForRawValues(
         RawValueProcessingArgs args,
         RawValueStorage<Int32> storage
         )
      {
         var md = args.MetaData;
         var tblEnum = this.Table;
         MetaDataTable tbl;
         if ( md.TryGetByTable( tblEnum, out tbl ) )
         {
            var table = (MetaDataTable<TRow>) tbl;
            var cols = this._columns
               .Select( c => c.RawValueProcessor )
               .Where( p => p != null )
               .ToArray();
            if ( cols.Length > 0 )
            {
               var list = table.TableContents;
               for ( var i = 0; i < list.Count; ++i )
               {
                  var cArgs = new ColumnFunctionalityArgs<TRow, RawValueProcessingArgs>( i, list[i], args );
                  var cur = 0;
                  foreach ( var rawValue in storage.GetAllRawValuesForRow( tblEnum, i ) )
                  {
                     try
                     {
                        cols[cur]( cArgs, rawValue );
                     }
                     catch
                     {
                        // Ignore...
                        // TODO error reporting mechanism
                     }
                     ++cur;
                  }
               }
            }
         }
      }

      public IEnumerable<SectionPartWithRVAs> CreateRawValueSectionParts(
         CILMetaData md,
         WriterMetaDataStreamContainer mdStreamContainer
      )
      {
         foreach ( var col in this._columns )
         {
            var creator = col.RawColummnSectionPartCreator;
            if ( creator != null )
            {
               yield return creator( md, mdStreamContainer );
            }
         }
      }

      public void ExtractTableHeapValues(
         CILMetaData md,
         RawValueStorage<Int32> storage,
         WriterMetaDataStreamContainer mdStreamContainer,
         ResizableArray<Byte> array,
         ArrayQuery<Byte> thisAssemblyPublicKeyIfPresentNull
         )
      {
         MetaDataTable tbl;
         if ( md.TryGetByTable( this.Table, out tbl ) )
         {
            var table = (MetaDataTable<TRow>) tbl;
            var cols = this._columns
               .Select( c => c.HeapValueExtractor )
               .Where( e => e != null )
               .ToArray();
            if ( cols.Length > 0 )
            {
               var list = table.TableContents;
               var rArgs = new RowHeapFillingArguments( mdStreamContainer, array, thisAssemblyPublicKeyIfPresentNull, md );
               for ( var i = 0; i < list.Count; ++i )
               {
                  var cArgs = new ColumnFunctionalityArgs<TRow, RowHeapFillingArguments>( i, list[i], rArgs );
                  foreach ( var col in cols )
                  {
                     Int32 rawValue;
                     try
                     {
                        rawValue = col( cArgs );
                     }
                     catch
                     {
                        // TODO error reporting
                        rawValue = 0;
                     }
                     storage.AddRawValue( rawValue );
                  }
               }
            }
         }
      }

      public IEnumerable<Int32> GetAllRawValues(
         MetaDataTable table,
         RawValueProvider rawValueProvider,
         RawValueStorage<Int32> heapIndices,
         RVAConverter rvaConverter
         )
      {
         var list = ( (MetaDataTable<TRow>) table ).TableContents;
         if ( list.Count > 0 )
         {
            var rawTransofrmArgs = new RawValueTransformationArguments( rvaConverter );
            var cols = this._columns;
            for ( var rowIdx = 0; rowIdx < list.Count; ++rowIdx )
            {
               var row = list[rowIdx];
               Int32 heapIdx = 0, rawIdx = 0;
               for ( var colIdx = 0; colIdx < cols.Length; ++colIdx )
               {
                  var col = cols[colIdx];
                  if ( col.ConstantExtractor != null )
                  {
                     yield return col.ConstantExtractor( row );
                  }
                  else if ( col.HeapValueExtractor != null )
                  {
                     yield return heapIndices.GetRawValue( this.Table, rowIdx, heapIdx );
                     ++heapIdx;
                  }
                  else if ( col.RawColummnSectionPartCreator != null )
                  {
                     yield return rawValueProvider.GetRawValueFor( this.Table, rowIdx, rawIdx );
                     ++rawIdx;
                  }
                  else
                  {
                     yield return 0;
                  }
               }
            }
         }
      }

      public TableSerializationFunctionality CreateSupport( ColumnSerializationSupportCreationArgs supportArgs )
      {
         return new DefaultTableSerializationFunctionality<TRawRow, TRow>(
            this,
            this._columns,
            supportArgs,
            this._rowFactory,
            this._rawRowFactory
            );
      }
   }

   public class DefaultTableSerializationFunctionality<TRawRow, TRow> : TableSerializationFunctionality
      where TRawRow : class
      where TRow : class
   {

      private readonly ColumnSerializationInstance[] _columnArray;
      private readonly Func<TRow> _rowFactory;
      private readonly Func<TRawRow> _rawRowFactory;

      private abstract class ColumnSerializationInstance
      {
         private readonly RawRowColumnSetterDelegate<TRawRow> _rawSetter;

         internal ColumnSerializationInstance(
            DefaultColumnSerializationInfo<TRawRow, TRow> serializationInfo,
            ColumnSerializationSupportCreationArgs args
            )
         {

            ArgumentValidator.ValidateNotNull( "Serialization info", serializationInfo );
            ArgumentValidator.ValidateNotNull( "Functionality creation args", args );

            this.Functionality = serializationInfo.SerializationSupportCreator( args );
            this._rawSetter = serializationInfo.RawSetter;
         }

         public ColumnSerializationFunctionality Functionality { get; }

         public abstract void SetNormalRowValue( RowReadingArguments rowArgs, ref Int32 idx, TRow row, Int32 rowIndex );

         public void SetRawRowValue( TRawRow row, Byte[] array, ref Int32 idx )
         {
            this._rawSetter( row, this.Functionality.ReadRawValue( array, ref idx ) );
         }
      }

      private sealed class ColumnSerializationInstance_RawValue : ColumnSerializationInstance
      {
         internal ColumnSerializationInstance_RawValue(
            DefaultColumnSerializationInfo<TRawRow, TRow> serializationInfo,
            ColumnSerializationSupportCreationArgs args
            )
            : base( serializationInfo, args )
         {

         }

         public override void SetNormalRowValue( RowReadingArguments rowArgs, ref Int32 idx, TRow row, Int32 rowIndex )
         {
            rowArgs.RawValueStorage.AddRawValue( this.Functionality.ReadRawValue( rowArgs.Array, ref idx ) );
         }
      }

      private sealed class ColumnSerializationInstance_NormalValue : ColumnSerializationInstance
      {
         private readonly RowColumnSetterDelegate<TRow, Int32> _setter;

         internal ColumnSerializationInstance_NormalValue(
            DefaultColumnSerializationInfo<TRawRow, TRow> serializationInfo,
            ColumnSerializationSupportCreationArgs args
            )
            : base( serializationInfo, args )
         {
            var setter = serializationInfo.Setter;
            ArgumentValidator.ValidateNotNull( "Setter", setter );
            this._setter = setter;
         }

         public override void SetNormalRowValue( RowReadingArguments rowArgs, ref Int32 idx, TRow row, Int32 rowIndex )
         {
            try
            {
               this._setter( new ColumnFunctionalityArgs<TRow, RowReadingArguments>( rowIndex, row, rowArgs ), this.Functionality.ReadRawValue( rowArgs.Array, ref idx ) );
            }
            catch
            {
               // TODO error reporting
            }
         }
      }

      public DefaultTableSerializationFunctionality(
         TableSerializationInfo tableSerializationInfo,
         IEnumerable<DefaultColumnSerializationInfo<TRawRow, TRow>> columns,
         ColumnSerializationSupportCreationArgs args,
         Func<TRow> rowFactory,
         Func<TRawRow> rawRowFactory
         )
      {
         ArgumentValidator.ValidateNotNull( "Table serialization info", tableSerializationInfo );
         ArgumentValidator.ValidateNotNull( "Columns", columns );
         ArgumentValidator.ValidateNotNull( "Row factory", rowFactory );
         ArgumentValidator.ValidateNotNull( "Raw row factory", rawRowFactory );


         this._rowFactory = rowFactory;
         this._rawRowFactory = rawRowFactory;
         this._columnArray = columns
            .Select( c => c.Setter == null ? (ColumnSerializationInstance) new ColumnSerializationInstance_RawValue( c, args ) : new ColumnSerializationInstance_NormalValue( c, args ) )
            .ToArray();
         this.ColumnSerializationSupports = this._columnArray
            .Select( c => c.Functionality )
            .ToArrayProxy()
            .CQ;
         this.TableSerializationInfo = tableSerializationInfo;
      }

      public TableSerializationInfo TableSerializationInfo { get; }

      public ArrayQuery<ColumnSerializationFunctionality> ColumnSerializationSupports { get; }

      public void ReadRows(
         MetaDataTable table,
         Int32 tableRowCount,
         RowReadingArguments args
         )
      {
         if ( tableRowCount > 0 )
         {
            var list = ( (MetaDataTable<TRow>) table ).TableContents;
            var idx = args.Index;
            var cArray = this._columnArray;
            var cArrayMax = this._columnArray.Length;

            for ( var i = 0; i < tableRowCount; ++i )
            {
               var row = this._rowFactory();
               for ( var j = 0; j < cArrayMax; ++j )
               {
                  cArray[j].SetNormalRowValue( args, ref idx, row, i );
               }

               list.Add( row );
            }
         }
      }

      public Object ReadRawRow( Byte[] array, Int32 idx )
      {
         var row = this._rawRowFactory();
         for ( var i = 0; i < this._columnArray.Length; ++i )
         {
            this._columnArray[i].SetRawRowValue( row, array, ref idx );
         }
         return row;
      }

   }

   public sealed class ColumnSerializationSupport_Constant8 : ColumnSerializationFunctionality
   {

      private static ColumnSerializationFunctionality _instance;

      public static ColumnSerializationFunctionality Instance
      {
         get
         {
            var retVal = _instance;
            if ( retVal == null )
            {
               retVal = new ColumnSerializationSupport_Constant8();
               _instance = retVal;
            }
            return retVal;
         }
      }

      private ColumnSerializationSupport_Constant8()
      {

      }


      public Int32 ColumnByteCount
      {
         get
         {
            return sizeof( Byte );
         }
      }

      public Int32 ReadRawValue( Byte[] array, ref Int32 idx )
      {
         return array.ReadByteFromBytes( ref idx );
      }

      public void WriteValue( Byte[] bytes, Int32 idx, Int32 value )
      {
         bytes.WriteByteToBytes( ref idx, (Byte) value );
      }
   }
   public sealed class ColumnSerializationSupport_Constant16 : ColumnSerializationFunctionality
   {
      private static ColumnSerializationFunctionality _instance;

      public static ColumnSerializationFunctionality Instance
      {
         get
         {
            var retVal = _instance;
            if ( retVal == null )
            {
               retVal = new ColumnSerializationSupport_Constant16();
               _instance = retVal;
            }
            return retVal;
         }
      }

      private ColumnSerializationSupport_Constant16()
      {

      }

      public Int32 ColumnByteCount
      {
         get
         {
            return sizeof( UInt16 );
         }
      }

      public Int32 ReadRawValue( Byte[] array, ref Int32 idx )
      {
         return array.ReadUInt16LEFromBytes( ref idx );
      }

      public void WriteValue( Byte[] bytes, Int32 idx, Int32 value )
      {
         bytes.WriteUInt16LEToBytes( ref idx, (UInt16) value );
      }
   }

   public sealed class ColumnSerializationSupport_Constant32 : ColumnSerializationFunctionality
   {
      private static ColumnSerializationFunctionality _instance;

      public static ColumnSerializationFunctionality Instance
      {
         get
         {
            var retVal = _instance;
            if ( retVal == null )
            {
               retVal = new ColumnSerializationSupport_Constant32();
               _instance = retVal;
            }
            return retVal;
         }
      }

      private ColumnSerializationSupport_Constant32()
      {

      }

      public Int32 ColumnByteCount
      {
         get
         {
            return sizeof( Int32 );
         }
      }

      public Int32 ReadRawValue( Byte[] array, ref Int32 idx )
      {
         return array.ReadInt32LEFromBytes( ref idx );
      }

      public void WriteValue( Byte[] bytes, Int32 idx, Int32 value )
      {
         bytes.WriteInt32LEToBytes( ref idx, value );
      }
   }


}

public static partial class E_CILPhysical
{
   private const Int32 UINT_ONE_BYTE_MAX = 0x7F;
   private const Int32 UINT_TWO_BYTES_MAX = 0x3FFF;
   private const Int32 UINT_FOUR_BYTES_MAX = 0x1FFFFFFF;

   internal static Int32 DecompressUInt32OrDefault( this Byte[] array, ref Int32 idx, Int32 max, Int32 defaultValue )
   {
      return idx < max ? array.DecompressUInt32( ref idx ) : defaultValue;
   }

   internal static Int32 DecompressInt32( this Byte[] array, ref Int32 idx )
   {
      const Int32 COMPLEMENT_MASK_ONE_BYTE = unchecked((Int32) 0xFFFFFFC0);
      const Int32 COMPLEMENT_MASK_TWO_BYTES = unchecked((Int32) 0xFFFFE000);
      const Int32 COMPLEMENT_MASK_FOUR_BYTES = unchecked((Int32) 0xF0000000);
      const Int32 ONE = 1;

      var value = array.DecompressUInt32( ref idx );
      if ( value <= UINT_ONE_BYTE_MAX )
      {
         // Value is one-bit left rotated, 7-bit 2-complement number
         // If LSB is 1 -> then the value is negative
         if ( ( value & ONE ) == ONE )
         {
            value = ( value >> 1 ) | COMPLEMENT_MASK_ONE_BYTE;
         }
         else
         {
            value = value >> 1;
         }
      }
      else if ( value <= UINT_TWO_BYTES_MAX )
      {
         if ( ( value & ONE ) == ONE )
         {
            value = ( value >> 1 ) | COMPLEMENT_MASK_TWO_BYTES;
         }
         else
         {
            value = value >> 1;
         }
      }
      else
      {
         if ( ( value & ONE ) == ONE )
         {
            value = ( value >> 1 ) | COMPLEMENT_MASK_FOUR_BYTES;
         }
         else
         {
            value = value >> 1;
         }
      }


      return value;
   }

   internal static Int32 DecompressUInt32( this Byte[] stream, ref Int32 idx )
   {
      const Int32 UINT_TWO_BYTES_DECODE_MASK = 0x3F;
      const Int32 UINT_FOUR_BYTES_DECODE_MASK = 0x1F;

      Int32 value = stream[idx];
      if ( ( value & 0x80 ) == 0 )
      {
         // MSB bit not set, so it's just one byte 
         ++idx;
      }
      else if ( ( value & 0xC0 ) == 0x80 )
      {
         // MSB set, but prev bit not set, so it's two bytes
         value = ( ( value & UINT_TWO_BYTES_DECODE_MASK ) << 8 ) | (Int32) stream[idx + 1];
         idx += 2;
      }
      else
      {
         // Whatever it is, it is four bytes long
         value = ( ( value & UINT_FOUR_BYTES_DECODE_MASK ) << 24 ) | ( ( (Int32) stream[idx + 1] ) << 16 ) | ( ( (Int32) stream[idx + 2] ) << 8 ) | stream[idx + 3];
         idx += 4;
      }

      return value;
   }

   internal static Boolean TryDecompressUInt32( this Byte[] stream, ref Int32 idx, Int32 max, out Int32 value, Boolean acceptErraneous = true )
   {
      const Int32 UINT_TWO_BYTES_DECODE_MASK = 0x3F;
      const Int32 UINT_FOUR_BYTES_DECODE_MASK = 0x1F;

      if ( idx < max )
      {
         Int32 first = stream[idx];
         if ( ( first & 0x80 ) == 0 )
         {
            // MSB bit not set, so it's just one byte 
            value = first;
            ++idx;
         }
         else if ( ( first & 0xC0 ) == 0x80 )
         {
            // MSB set, but prev bit not set, so it's two bytes
            if ( idx < max - 1 )
            {
               value = ( ( first & UINT_TWO_BYTES_DECODE_MASK ) << 8 ) | (Int32) stream[idx + 1];
               idx += 2;
            }
            else
            {
               value = -1;
            }
         }
         else if ( acceptErraneous || ( first & 0xE0 ) == 0xC0 )
         {
            if ( idx < max - 3 )
            {
               value = ( ( first & UINT_FOUR_BYTES_DECODE_MASK ) << 24 ) | ( ( (Int32) stream[idx + 1] ) << 16 ) | ( ( (Int32) stream[idx + 2] ) << 8 ) | stream[idx + 3];
               idx += 4;
            }
            else
            {
               value = -1;
            }
         }
         else
         {
            value = -1;
         }
      }
      else
      {
         value = -1;
      }
      return value >= 0;
   }

   internal static Byte[] CreateAnySignature( this ResizableArray<Byte> info, AbstractSignature sig )
   {
      if ( sig == null )
      {
         return null;
      }
      else
      {
         switch ( sig.SignatureKind )
         {
            case SignatureKind.Field:
               return info.CreateFieldSignature( sig as FieldSignature );
            case SignatureKind.GenericMethodInstantiation:
               return info.CreateMethodSpecSignature( sig as GenericMethodSignature );
            case SignatureKind.LocalVariables:
               return info.CreateLocalsSignature( sig as LocalVariablesSignature );
            case SignatureKind.MethodDefinition:
            case SignatureKind.MethodReference:
               return info.CreateMethodSignature( sig as AbstractMethodSignature );
            case SignatureKind.Property:
               return info.CreatePropertySignature( sig as PropertySignature );
            case SignatureKind.RawSignature:
               return ( (RawSignature) sig ).Bytes.CreateArrayCopy();
            case SignatureKind.Type:
               return info.CreateTypeSignature( sig as TypeSignature );
            default:
               return null;
         }
      }
   }

   internal static Byte[] CreateFieldSignature( this ResizableArray<Byte> info, FieldSignature sig )
   {
      var idx = 0;
      info.WriteFieldSignature( ref idx, sig );
      return info.Array.CreateArrayCopy( idx );
   }

   internal static Byte[] CreateMethodSignature( this ResizableArray<Byte> info, AbstractMethodSignature sig )
   {
      var idx = 0;
      info.WriteMethodSignature( ref idx, sig );
      return info.Array.CreateArrayCopy( idx );
   }

   internal static Byte[] CreateConstantBytes( this ResizableArray<Byte> info, Object constant, SignatureElementTypes elementType )
   {
      var idx = 0;
      return info.WriteConstantValue( ref idx, constant, elementType ) ?
         info.Array.CreateArrayCopy( idx ) :
         null;
   }

   internal static Byte[] CreateLocalsSignature( this ResizableArray<Byte> info, LocalVariablesSignature sig )
   {
      var idx = 0;
      info.WriteLocalsSignature( ref idx, sig );
      return info.Array.CreateArrayCopy( idx );
   }

   internal static Byte[] CreateCustomAttributeSignature( this ResizableArray<Byte> info, CILMetaData md, Int32 caIdx )
   {
      var sig = md.CustomAttributeDefinitions.TableContents[caIdx].Signature;
      Byte[] retVal;
      if ( sig != null ) // sig.TypedArguments.Count > 0 || sig.NamedArguments.Count > 0 )
      {
         var sigg = sig as CustomAttributeSignature;
         if ( sigg != null )
         {
            var idx = 0;
            info.WriteCustomAttributeSignature( ref idx, md, caIdx );
            retVal = info.Array.CreateArrayCopy( idx );
         }
         else
         {
            retVal = ( (RawCustomAttributeSignature) sig ).Bytes;
         }
      }
      else
      {
         // Signature missing
         retVal = null;
      }
      return retVal;
   }

   internal static Byte[] CreateMarshalSpec( this ResizableArray<Byte> info, AbstractMarshalingInfo marshal )
   {
      var idx = 0;
      return info.WriteMarshalInfo( ref idx, marshal ) ?
         info.Array.CreateArrayCopy( idx ) :
         null;
   }

   internal static Byte[] CreateSecuritySignature( this ResizableArray<Byte> info, List<AbstractSecurityInformation> permissions, ResizableArray<Byte> aux )
   {
      var idx = 0;
      info.WriteSecuritySignature( ref idx, permissions, aux );
      return info.Array.CreateArrayCopy( idx );
   }

   internal static Byte[] CreateStandaloneSignature( this ResizableArray<Byte> info, StandaloneSignature standaloneSig )
   {
      var sig = standaloneSig.Signature;
      var locals = sig as LocalVariablesSignature;
      var idx = 0;
      if ( locals != null )
      {
         if ( standaloneSig.StoreSignatureAsFieldSignature && locals.Locals.Count > 0 )
         {
            info
               .AddSigStarterByte( ref idx, SignatureStarters.Field )
               .WriteLocalSignature( ref idx, locals.Locals[0] );
         }
         else
         {
            info.WriteLocalsSignature( ref idx, locals );
         }
      }
      else
      {
         var raw = sig as RawSignature;
         if ( raw != null )
         {
            info.WriteArray( ref idx, raw.Bytes );
         }
         else
         {
            info.WriteMethodSignature( ref idx, sig as AbstractMethodSignature );
         }
      }

      return idx == 0 ? null : info.Array.CreateArrayCopy( idx );
   }

   internal static Byte[] CreatePropertySignature( this ResizableArray<Byte> info, PropertySignature sig )
   {
      var idx = 0;
      info.WritePropertySignature( ref idx, sig );
      return info.Array.CreateArrayCopy( idx );
   }

   internal static Byte[] CreateTypeSignature( this ResizableArray<Byte> info, TypeSignature sig )
   {
      var idx = 0;
      info.WriteTypeSignature( ref idx, sig );
      return info.Array.CreateArrayCopy( idx );
   }

   internal static Byte[] CreateMethodSpecSignature( this ResizableArray<Byte> info, GenericMethodSignature sig )
   {
      var idx = 0;
      info.WriteMethodSpecSignature( ref idx, sig );
      return info.Array.CreateArrayCopy( idx );
   }

   private static void WriteFieldSignature( this ResizableArray<Byte> info, ref Int32 idx, FieldSignature sig )
   {
      if ( sig != null )
      {
         info
            .AddSigStarterByte( ref idx, SignatureStarters.Field )
            .WriteCustomModifiers( ref idx, sig.CustomModifiers )
            .WriteTypeSignature( ref idx, sig.Type );
      }
   }

   private static ResizableArray<Byte> WriteCustomModifiers( this ResizableArray<Byte> info, ref Int32 idx, IList<CustomModifierSignature> mods )
   {
      if ( mods.Count > 0 )
      {
         foreach ( var mod in mods )
         {
            info
               .AddSigByte( ref idx, mod.IsOptional ? SignatureElementTypes.CModOpt : SignatureElementTypes.CModReqd )
               .AddTDRSToken( ref idx, mod.CustomModifierType );
         }
      }
      return info;
   }

   private static ResizableArray<Byte> WriteTypeSignature( this ResizableArray<Byte> info, ref Int32 idx, TypeSignature type )
   {
      switch ( type.TypeSignatureKind )
      {
         case TypeSignatureKind.Simple:
            info.AddSigByte( ref idx, ( (SimpleTypeSignature) type ).SimpleType );
            break;
         case TypeSignatureKind.SimpleArray:
            var szArray = (SimpleArrayTypeSignature) type;
            info
               .AddSigByte( ref idx, SignatureElementTypes.SzArray )
               .WriteCustomModifiers( ref idx, szArray.CustomModifiers )
               .WriteTypeSignature( ref idx, szArray.ArrayType );
            break;
         case TypeSignatureKind.ComplexArray:
            var array = (ComplexArrayTypeSignature) type;
            info
               .AddSigByte( ref idx, SignatureElementTypes.Array )
               .WriteTypeSignature( ref idx, array.ArrayType )
               .AddCompressedUInt32( ref idx, array.Rank )
               .AddCompressedUInt32( ref idx, array.Sizes.Count );
            foreach ( var size in array.Sizes )
            {
               info.AddCompressedUInt32( ref idx, size );
            }
            info.AddCompressedUInt32( ref idx, array.LowerBounds.Count );
            foreach ( var lobo in array.LowerBounds )
            {
               info.AddCompressedInt32( ref idx, lobo );
            }
            break;
         case TypeSignatureKind.ClassOrValue:
            var clazz = (ClassOrValueTypeSignature) type;
            var gArgs = clazz.GenericArguments;
            var isGenericType = gArgs.Count > 0;
            if ( isGenericType )
            {
               info.AddSigByte( ref idx, SignatureElementTypes.GenericInst );
            }
            info
               .AddSigByte( ref idx, clazz.IsClass ? SignatureElementTypes.Class : SignatureElementTypes.ValueType )
               .AddTDRSToken( ref idx, clazz.Type );
            if ( isGenericType )
            {
               info.AddCompressedUInt32( ref idx, gArgs.Count );
               foreach ( var gArg in gArgs )
               {
                  info.WriteTypeSignature( ref idx, gArg );
               }
            }
            break;
         case TypeSignatureKind.GenericParameter:
            var gParam = (GenericParameterTypeSignature) type;
            info
               .AddSigByte( ref idx, gParam.IsTypeParameter ? SignatureElementTypes.Var : SignatureElementTypes.MVar )
               .AddCompressedUInt32( ref idx, gParam.GenericParameterIndex );
            break;
         case TypeSignatureKind.FunctionPointer:
            info
               .AddSigByte( ref idx, SignatureElementTypes.FnPtr )
               .WriteMethodSignature( ref idx, ( (FunctionPointerTypeSignature) type ).MethodSignature );
            break;
         case TypeSignatureKind.Pointer:
            var ptr = (PointerTypeSignature) type;
            info
               .AddSigByte( ref idx, SignatureElementTypes.Ptr )
               .WriteCustomModifiers( ref idx, ptr.CustomModifiers )
               .WriteTypeSignature( ref idx, ptr.PointerType );
            break;

      }
      return info;
   }

   private static ResizableArray<Byte> WriteMethodSignature( this ResizableArray<Byte> info, ref Int32 idx, AbstractMethodSignature method )
   {
      if ( method != null )
      {
         var starter = method.SignatureStarter;
         info.AddSigStarterByte( ref idx, method.SignatureStarter );

         if ( starter.IsGeneric() )
         {
            info.AddCompressedUInt32( ref idx, method.GenericArgumentCount );
         }

         info
            .AddCompressedUInt32( ref idx, method.Parameters.Count )
            .WriteParameterSignature( ref idx, method.ReturnType );

         foreach ( var param in method.Parameters )
         {
            info.WriteParameterSignature( ref idx, param );
         }

         if ( method.SignatureKind == SignatureKind.MethodReference )
         {
            var mRef = (MethodReferenceSignature) method;
            if ( mRef.VarArgsParameters.Count > 0 )
            {
               info.AddSigByte( ref idx, SignatureElementTypes.Sentinel );
               foreach ( var v in mRef.VarArgsParameters )
               {
                  info.WriteParameterSignature( ref idx, v );
               }
            }
         }
      }
      return info;
   }

   private static ResizableArray<Byte> WriteParameterSignature( this ResizableArray<Byte> info, ref Int32 idx, ParameterSignature parameter )
   {
      info
         .WriteCustomModifiers( ref idx, parameter.CustomModifiers );
      if ( SimpleTypeSignature.TypedByRef.Equals( parameter.Type ) )
      {
         info.AddSigByte( ref idx, SignatureElementTypes.TypedByRef );
      }
      else
      {
         if ( parameter.IsByRef )
         {
            info.AddSigByte( ref idx, SignatureElementTypes.ByRef );
         }

         info.WriteTypeSignature( ref idx, parameter.Type );
      }
      return info;
   }

   private static Boolean WriteConstantValue( this ResizableArray<Byte> info, ref Int32 idx, Object constant, SignatureElementTypes elementType )
   {
      var retVal = true;
      if ( constant == null )
      {
         retVal = elementType != SignatureElementTypes.String;
         if ( retVal )
         {
            info.WriteInt32LEToBytes( ref idx, 0 );
         }
      }
      else
      {
         info.WriteConstantValueNotNull( ref idx, constant );
      }

      return retVal;
   }

   private static void WriteConstantValueNotNull( this ResizableArray<Byte> info, ref Int32 idx, Object constant )
   {

      switch ( Type.GetTypeCode( constant.GetType() ) )
      {
         case TypeCode.Boolean:
            info.WriteByteToBytes( ref idx, Convert.ToBoolean( constant ) ? (Byte) 1 : (Byte) 0 );
            break;
         case TypeCode.SByte:
            info.WriteSByteToBytes( ref idx, Convert.ToSByte( constant ) );
            break;
         case TypeCode.Byte:
            info.WriteByteToBytes( ref idx, Convert.ToByte( constant ) );
            break;
         case TypeCode.Char:
            info.WriteUInt16LEToBytes( ref idx, Convert.ToUInt16( Convert.ToChar( constant ) ) );
            break;
         case TypeCode.Int16:
            info.WriteInt16LEToBytes( ref idx, Convert.ToInt16( constant ) );
            break;
         case TypeCode.UInt16:
            info.WriteUInt16LEToBytes( ref idx, Convert.ToUInt16( constant ) );
            break;
         case TypeCode.Int32:
            info.WriteInt32LEToBytes( ref idx, Convert.ToInt32( constant ) );
            break;
         case TypeCode.UInt32:
            info.WriteUInt32LEToBytes( ref idx, Convert.ToUInt32( constant ) );
            break;
         case TypeCode.Int64:
            info.WriteInt64LEToBytes( ref idx, Convert.ToInt64( constant ) );
            break;
         case TypeCode.UInt64:
            info.WriteUInt64LEToBytes( ref idx, Convert.ToUInt64( constant ) );
            break;
         case TypeCode.Single:
            info.WriteSingleLEToBytes( ref idx, Convert.ToSingle( constant ) );
            break;
         case TypeCode.Double:
            info.WriteDoubleLEToBytes( ref idx, Convert.ToDouble( constant ) );
            break;
         case TypeCode.String:
            var str = Convert.ToString( constant );
            var encoding = MetaDataConstants.USER_STRING_ENCODING;
            var size = encoding.GetByteCount( str );
            info.EnsureThatCanAdd( idx, size );
            idx += encoding.GetBytes( str, 0, str.Length, info.Array, idx );
            break;
         default:
            info.WriteInt32LEToBytes( ref idx, 0 );
            break;
      }
   }

   private static void WriteCustomAttributeSignature( this ResizableArray<Byte> info, ref Int32 idx, CILMetaData md, Int32 caIdx )
   {
      var ca = md.CustomAttributeDefinitions.TableContents[caIdx];
      var attrData = ca.Signature as CustomAttributeSignature;

      var ctor = ca.Type;
      var sig = ctor.Table == Tables.MethodDef ?
         md.MethodDefinitions.TableContents[ctor.Index].Signature :
         md.MemberReferences.TableContents[ctor.Index].Signature as AbstractMethodSignature;

      if ( sig == null )
      {
         throw new InvalidOperationException( "Custom attribute constructor signature was null (custom attribute at index " + caIdx + ", ctor: " + ctor + ")." );
      }
      else if ( sig.Parameters.Count != attrData.TypedArguments.Count )
      {
         throw new InvalidOperationException( "Custom attribute constructor has different amount of parameters than supplied custom attribute data (custom attribute at index " + caIdx + ", ctor: " + ctor + ")." );
      }


      // Prolog
      info
         .WriteByteToBytes( ref idx, 1 )
         .WriteByteToBytes( ref idx, 0 );

      // Fixed args
      for ( var i = 0; i < attrData.TypedArguments.Count; ++i )
      {
         var arg = attrData.TypedArguments[i];
         var caType = md.ResolveCACtorType( sig.Parameters[i].Type, tIdx => new CustomAttributeArgumentTypeEnum()
         {
            TypeString = "" // Type string doesn't matter, as values will be serialized directly...
         } );

         if ( caType == null )
         {
            // TODO some kind of warning system instead of throwing
            throw new InvalidOperationException( "Failed to resolve custom attribute type for constructor parameter (custom attribute at index " + caIdx + ", ctor: " + ctor + ", param: " + i + ")." );
         }
         info.WriteCustomAttributeFixedArg( ref idx, caType, arg.Value );
      }

      // Named args
      info.WriteUInt16LEToBytes( ref idx, (UInt16) attrData.NamedArguments.Count );
      foreach ( var arg in attrData.NamedArguments )
      {
         info.WriteCustomAttributeNamedArg( ref idx, arg );
      }
   }

   private static ResizableArray<Byte> WriteCustomAttributeFixedArg( this ResizableArray<Byte> info, ref Int32 idx, CustomAttributeArgumentType argType, Object arg )
   {
      switch ( argType.ArgumentTypeKind )
      {
         case CustomAttributeArgumentTypeKind.Array:
            if ( arg == null )
            {
               info.WriteInt32LEToBytes( ref idx, unchecked((Int32) 0xFFFFFFFF) );
            }
            else
            {
               var isDirectArray = arg is Array;
               Array array;
               if ( isDirectArray )
               {
                  array = (Array) arg;
                  argType = ( (CustomAttributeArgumentTypeArray) argType ).ArrayType;
               }
               else
               {
                  var indirectArray = (CustomAttributeValue_Array) arg;
                  array = indirectArray.Array;
                  argType = indirectArray.ArrayElementType;
               }

               info.WriteInt32LEToBytes( ref idx, array.Length );
               foreach ( var elem in array )
               {
                  info.WriteCustomAttributeFixedArg( ref idx, argType, elem );
               }
            }
            break;
         case CustomAttributeArgumentTypeKind.Simple:
            switch ( ( (CustomAttributeArgumentTypeSimple) argType ).SimpleType )
            {
               case SignatureElementTypes.Boolean:
                  info.WriteByteToBytes( ref idx, Convert.ToBoolean( arg ) ? (Byte) 1 : (Byte) 0 );
                  break;
               case SignatureElementTypes.I1:
                  info.WriteSByteToBytes( ref idx, Convert.ToSByte( arg ) );
                  break;
               case SignatureElementTypes.U1:
                  info.WriteByteToBytes( ref idx, Convert.ToByte( arg ) );
                  break;
               case SignatureElementTypes.Char:
                  info.WriteUInt16LEToBytes( ref idx, Convert.ToUInt16( Convert.ToChar( arg ) ) );
                  break;
               case SignatureElementTypes.I2:
                  info.WriteInt16LEToBytes( ref idx, Convert.ToInt16( arg ) );
                  break;
               case SignatureElementTypes.U2:
                  info.WriteUInt16LEToBytes( ref idx, Convert.ToUInt16( arg ) );
                  break;
               case SignatureElementTypes.I4:
                  info.WriteInt32LEToBytes( ref idx, Convert.ToInt32( arg ) );
                  break;
               case SignatureElementTypes.U4:
                  info.WriteUInt32LEToBytes( ref idx, Convert.ToUInt32( arg ) );
                  break;
               case SignatureElementTypes.I8:
                  info.WriteInt64LEToBytes( ref idx, Convert.ToInt64( arg ) );
                  break;
               case SignatureElementTypes.U8:
                  info.WriteUInt64LEToBytes( ref idx, Convert.ToUInt64( arg ) );
                  break;
               case SignatureElementTypes.R4:
                  info.WriteSingleLEToBytes( ref idx, Convert.ToSingle( arg ) );
                  break;
               case SignatureElementTypes.R8:
                  info.WriteDoubleLEToBytes( ref idx, Convert.ToDouble( arg ) );
                  break;
               case SignatureElementTypes.String:
                  info.AddCAString( ref idx, arg == null ? null : Convert.ToString( arg ) );
                  break;
               case SignatureElementTypes.Type:
                  String typeStr;
                  if ( arg != null )
                  {
                     if ( arg is CustomAttributeValue_TypeReference )
                     {
                        typeStr = ( (CustomAttributeValue_TypeReference) arg ).TypeString;
                     }
                     else if ( arg is Type )
                     {
                        typeStr = ( (Type) arg ).AssemblyQualifiedName;
                     }
                     else
                     {
                        typeStr = Convert.ToString( arg );
                     }
                  }
                  else
                  {
                     typeStr = null;
                  }
                  info.AddCAString( ref idx, typeStr );
                  break;
               case SignatureElementTypes.Object:
                  if ( arg == null )
                  {
                     // Nulls are serialized as null strings
                     if ( !CustomAttributeArgumentTypeSimple.String.Equals( argType ) )
                     {
                        argType = CustomAttributeArgumentTypeSimple.String;
                     }
                  }
                  else
                  {
                     argType = ResolveBoxedCAType( arg );
                  }
                  info
                     .WriteCustomAttributeFieldOrPropType( ref idx, ref argType, ref arg )
                     .WriteCustomAttributeFixedArg( ref idx, argType, arg );
                  break;
            }
            break;
         case CustomAttributeArgumentTypeKind.TypeString:
            // TODO check for invalid types (bool, char, single, double, string, any other non-primitive)
            var valueToWrite = arg is CustomAttributeValue_EnumReference ? ( (CustomAttributeValue_EnumReference) arg ).EnumValue : arg;
            if ( valueToWrite == null )
            {
               throw new InvalidOperationException( "Tried to serialize null as enum." );
            }
            info.WriteConstantValueNotNull( ref idx, valueToWrite );
            break;
      }

      return info;
   }

   private static CustomAttributeArgumentType ResolveBoxedCAType( Object arg, Boolean isWithinArray = false )
   {
      var argType = arg.GetType();
      if ( argType.IsEnum )
      {
         return new CustomAttributeArgumentTypeEnum()
         {
            TypeString = argType.AssemblyQualifiedName
         };
      }
      else
      {
         switch ( Type.GetTypeCode( argType ) )
         {
            case TypeCode.Boolean:
               return CustomAttributeArgumentTypeSimple.Boolean;
            case TypeCode.Char:
               return CustomAttributeArgumentTypeSimple.Char;
            case TypeCode.SByte:
               return CustomAttributeArgumentTypeSimple.SByte;
            case TypeCode.Byte:
               return CustomAttributeArgumentTypeSimple.Byte;
            case TypeCode.Int16:
               return CustomAttributeArgumentTypeSimple.Int16;
            case TypeCode.UInt16:
               return CustomAttributeArgumentTypeSimple.UInt16;
            case TypeCode.Int32:
               return CustomAttributeArgumentTypeSimple.Int32;
            case TypeCode.UInt32:
               return CustomAttributeArgumentTypeSimple.UInt32;
            case TypeCode.Int64:
               return CustomAttributeArgumentTypeSimple.Int64;
            case TypeCode.UInt64:
               return CustomAttributeArgumentTypeSimple.UInt64;
            case TypeCode.Single:
               return CustomAttributeArgumentTypeSimple.Single;
            case TypeCode.Double:
               return CustomAttributeArgumentTypeSimple.Double;
            case TypeCode.String:
               return CustomAttributeArgumentTypeSimple.String;
            case TypeCode.Object:
               if ( argType.IsArray )
               {
                  return isWithinArray ?
                     (CustomAttributeArgumentType) CustomAttributeArgumentTypeSimple.Object :
                     new CustomAttributeArgumentTypeArray()
                     {
                        ArrayType = ResolveBoxedCAType( argType.GetElementType(), true )
                     };
               }
               else
               {
                  // Check for enum reference
                  if ( Equals( typeof( CustomAttributeValue_EnumReference ), argType ) )
                  {
                     return new CustomAttributeArgumentTypeEnum()
                     {
                        TypeString = ( (CustomAttributeValue_EnumReference) arg ).EnumType
                     };
                  }
                  // System.Type or System.Object or CustomAttributeTypeReference
                  else if ( Equals( typeof( CustomAttributeValue_TypeReference ), argType ) || Equals( typeof( Type ), argType ) )
                  {
                     return CustomAttributeArgumentTypeSimple.Type;
                  }
                  else if ( isWithinArray && Equals( typeof( Object ), argType ) )
                  {
                     return CustomAttributeArgumentTypeSimple.Object;
                  }
                  else
                  {
                     throw new InvalidOperationException( "Failed to deduce custom attribute type for " + argType + "." );
                  }
               }
            default:
               throw new InvalidOperationException( "Failed to deduce custom attribute type for " + argType + "." );
         }
      }
   }

   private static ResizableArray<Byte> WriteCustomAttributeNamedArg( this ResizableArray<Byte> info, ref Int32 idx, CustomAttributeNamedArgument arg )
   {
      var elem = arg.IsField ? SignatureElementTypes.CA_Field : SignatureElementTypes.CA_Property;
      var typedValueValue = arg.Value.Value;
      var caType = arg.FieldOrPropertyType;
      return info
         .AddSigByte( ref idx, elem )
         .WriteCustomAttributeFieldOrPropType( ref idx, ref caType, ref typedValueValue )
         .AddCAString( ref idx, arg.Name )
         .WriteCustomAttributeFixedArg( ref idx, caType, typedValueValue );
   }

   private static ResizableArray<Byte> WriteCustomAttributeFieldOrPropType( this ResizableArray<Byte> info, ref Int32 idx, ref CustomAttributeArgumentType type, ref Object value, Boolean processEnumTypeAndValue = true )
   {
      if ( type == null )
      {
         throw new InvalidOperationException( "Custom attribute signature typed argument type was null." );
      }

      switch ( type.ArgumentTypeKind )
      {
         case CustomAttributeArgumentTypeKind.Array:
            var arrayType = ( (CustomAttributeArgumentTypeArray) type ).ArrayType;
            Object dummy = null;
            info
               .AddSigByte( ref idx, SignatureElementTypes.SzArray )
               .WriteCustomAttributeFieldOrPropType( ref idx, ref arrayType, ref dummy, false );
            break;
         case CustomAttributeArgumentTypeKind.Simple:
            var sigStarter = ( (CustomAttributeArgumentTypeSimple) type ).SimpleType;
            if ( sigStarter == SignatureElementTypes.Object )
            {
               sigStarter = SignatureElementTypes.CA_Boxed;
            }
            info.AddSigByte( ref idx, sigStarter );
            break;
         case CustomAttributeArgumentTypeKind.TypeString:
            info
               .AddSigByte( ref idx, SignatureElementTypes.CA_Enum )
               .AddCAString( ref idx, ( (CustomAttributeArgumentTypeEnum) type ).TypeString );
            if ( processEnumTypeAndValue )
            {
               if ( value == null )
               {
                  throw new InvalidOperationException( "Tried to serialize null as enum." );
               }
               else
               {
                  if ( value is CustomAttributeValue_EnumReference )
                  {
                     value = ( (CustomAttributeValue_EnumReference) value ).EnumValue;
                  }

                  switch ( Type.GetTypeCode( value.GetType() ) )
                  {
                     //case TypeCode.Boolean:
                     //   type = CustomAttributeArgumentTypeSimple.Boolean;
                     //   break;
                     //case TypeCode.Char:
                     //   type = CustomAttributeArgumentTypeSimple.Char;
                     //   break;
                     case TypeCode.SByte:
                        type = CustomAttributeArgumentTypeSimple.SByte;
                        break;
                     case TypeCode.Byte:
                        type = CustomAttributeArgumentTypeSimple.Byte;
                        break;
                     case TypeCode.Int16:
                        type = CustomAttributeArgumentTypeSimple.Int16;
                        break;
                     case TypeCode.UInt16:
                        type = CustomAttributeArgumentTypeSimple.UInt16;
                        break;
                     case TypeCode.Int32:
                        type = CustomAttributeArgumentTypeSimple.Int32;
                        break;
                     case TypeCode.UInt32:
                        type = CustomAttributeArgumentTypeSimple.UInt32;
                        break;
                     case TypeCode.Int64:
                        type = CustomAttributeArgumentTypeSimple.Int64;
                        break;
                     case TypeCode.UInt64:
                        type = CustomAttributeArgumentTypeSimple.UInt64;
                        break;
                     //case TypeCode.Single:
                     //   type = CustomAttributeArgumentTypeSimple.Single;
                     //   break;
                     //case TypeCode.Double:
                     //   type = CustomAttributeArgumentTypeSimple.Double;
                     //   break;
                     //case TypeCode.String:
                     //   type = CustomAttributeArgumentTypeSimple.String;
                     //break;
                     default:
                        throw new NotSupportedException( "The custom attribute type was marked to be enum, but the actual value's type was: " + value.GetType() + "." );
                  }
               }
            }
            break;
      }

      return info;
   }

   private static Boolean WriteMarshalInfo( this ResizableArray<Byte> info, ref Int32 idx, AbstractMarshalingInfo marshal )
   {
      var retVal = marshal != null;
      if ( retVal )
      {
         info.WriteByteToBytes( ref idx, (Byte) marshal.Value );
         var canWrite = true;
         Int32 tmp; String tmpString;
         switch ( marshal.MarshalingInfoKind )
         {
            case MarshalingInfoKind.Simple:
               // Nothing else to write
               break;
            case MarshalingInfoKind.FixedLengthString:
               if ( IsMarshalSizeValid( ( tmp = ( (FixedLengthStringMarshalingInfo) marshal ).Size ) ) )
               {
                  info.AddCompressedUInt32( ref idx, tmp );
               }
               break;
            case MarshalingInfoKind.FixedLengthArray:
               var flArray = (FixedLengthArrayMarshalingInfo) marshal;
               if ( CanWriteNextMarshalElement( IsMarshalSizeValid( ( tmp = flArray.Size ) ), ref canWrite ) )
               {
                  info.AddCompressedUInt32( ref idx, tmp );
               }
               if ( CanWriteNextMarshalElement( IsUnmanagedTypeValid( ( tmp = (Int32) flArray.ElementType ) ), ref canWrite ) )
               {
                  info.AddCompressedUInt32( ref idx, tmp );
               }
               break;
            case MarshalingInfoKind.SafeArray:
               var sArray = (SafeArrayMarshalingInfo) marshal;
               if ( CanWriteNextMarshalElement( ( tmp = (Int32) sArray.ElementType ) != 0, ref canWrite ) )
               {
                  info.AddCompressedUInt32( ref idx, tmp );
               }
               if ( CanWriteNextMarshalElement( ( tmpString = sArray.UserDefinedType ) != null, ref canWrite ) )
               {
                  info.AddCAString( ref idx, tmpString );
               }
               break;
            case MarshalingInfoKind.Array:
               var array = (ArrayMarshalingInfo) marshal;
               if ( CanWriteNextMarshalElement( IsUnmanagedTypeValid( ( tmp = (Int32) array.ElementType ) ), ref canWrite ) )
               {
                  info.AddCompressedUInt32( ref idx, tmp );
               }
               if ( CanWriteNextMarshalElement( IsMarshalIndexValid( ( tmp = array.SizeParameterIndex ) ), ref canWrite ) )
               {
                  info.AddCompressedUInt32( ref idx, tmp );
               }
               if ( CanWriteNextMarshalElement( IsMarshalSizeValid( ( tmp = array.Size ) ), ref canWrite ) )
               {
                  info.AddCompressedUInt32( ref idx, tmp );
               }
               if ( CanWriteNextMarshalElement( ( tmp = array.Flags ) >= 0, ref canWrite ) )
               {
                  info.AddCompressedUInt32( ref idx, tmp );
               }
               break;
            case MarshalingInfoKind.Interface:
               if ( IsMarshalIndexValid( ( tmp = ( (InterfaceMarshalingInfo) marshal ).IIDParameterIndex ) ) )
               {
                  info.AddCompressedUInt32( ref idx, tmp );
               }
               break;
            case MarshalingInfoKind.Custom:
               var custom = (CustomMarshalingInfo) marshal;
               info
                     .AddCAString( ref idx, custom.GUIDString ?? "" )
                     .AddCAString( ref idx, custom.NativeTypeName ?? "" )
                     .AddCAString( ref idx, custom.CustomMarshalerTypeName ?? "" )
                     .AddCAString( ref idx, custom.MarshalCookie ?? "" );
               break;
            case MarshalingInfoKind.Raw:
               info.WriteArray( ref idx, ( (RawMarshalingInfo) marshal ).Bytes );
               break;
         }
      }

      return retVal;
   }

   private static Boolean CanWriteNextMarshalElement( Boolean condition, ref Boolean previousResult )
   {
      if ( previousResult )
      {
         if ( !condition )
         {
            previousResult = false;
         }
      }
      else
      {
         // TODO some sort of error reporting
      }

      return previousResult;
   }

   private static Boolean IsMarshalSizeValid( Int32 size )
   {
      return size >= 0;
   }

   private static Boolean IsMarshalIndexValid( Int32 idx )
   {
      return idx >= 0;
   }

   private static Boolean IsUnmanagedTypeValid( Int32 ut )
   {
      return ut != (Int32) UnmanagedType.NotPresent;
   }

   private static ResizableArray<Byte> WriteSecuritySignature(
      this ResizableArray<Byte> info,
      ref Int32 idx,
      List<AbstractSecurityInformation> permissions,
      ResizableArray<Byte> aux
      )
   {
      // TODO currently only newer format, .NET 1 format not supported for writing
      info
         .WriteByteToBytes( ref idx, AbstractSecurityInformation.DECL_SECURITY_HEADER )
         .AddCompressedUInt32( ref idx, permissions.Count );
      foreach ( var sec in permissions )
      {
         info.AddCAString( ref idx, sec.SecurityAttributeType );
         var secInfo = sec as SecurityInformation;
         Byte[] secInfoBLOB;
         if ( secInfo != null )
         {
            // Store arguments in separate bytes
            var auxIdx = 0;
            foreach ( var arg in secInfo.NamedArguments )
            {
               aux.WriteCustomAttributeNamedArg( ref auxIdx, arg );
            }
            // Now write to sec blob
            secInfoBLOB = aux.Array.CreateArrayCopy( auxIdx );
            // The length of named arguments blob
            info
               .AddCompressedUInt32( ref idx, secInfoBLOB.Length + BitUtils.GetEncodedUIntSize( secInfo.NamedArguments.Count ) )
            // The amount of named arguments
               .AddCompressedUInt32( ref idx, secInfo.NamedArguments.Count );
         }
         else
         {
            secInfoBLOB = ( (RawSecurityInformation) sec ).Bytes;
            info.AddCompressedUInt32( ref idx, secInfoBLOB.Length );
         }

         info.WriteArray( ref idx, secInfoBLOB );
      }

      return info;
   }

   private static ResizableArray<Byte> WriteLocalsSignature(
      this ResizableArray<Byte> info,
      ref Int32 idx,
      LocalVariablesSignature sig
      )
   {
      if ( sig != null )
      {
         var locals = sig.Locals;
         info
            .AddSigStarterByte( ref idx, SignatureStarters.LocalSignature )
            .AddCompressedUInt32( ref idx, locals.Count );
         foreach ( var local in locals )
         {
            info.WriteLocalSignature( ref idx, local );
         }
      }
      return info;
   }

   private static ResizableArray<Byte> WriteLocalSignature(
      this ResizableArray<Byte> info,
      ref Int32 idx,
      LocalVariableSignature sig
      )
   {
      if ( SimpleTypeSignature.TypedByRef.Equals( sig.Type ) )
      {
         info.AddSigByte( ref idx, SignatureElementTypes.TypedByRef );
      }
      else
      {
         info.WriteCustomModifiers( ref idx, sig.CustomModifiers );
         if ( sig.IsPinned )
         {
            info.AddSigByte( ref idx, SignatureElementTypes.Pinned );
         }

         if ( sig.IsByRef )
         {
            info.AddSigByte( ref idx, SignatureElementTypes.ByRef );
         }
         info.WriteTypeSignature( ref idx, sig.Type );
      }

      return info;
   }

   private static ResizableArray<Byte> WritePropertySignature(
      this ResizableArray<Byte> info,
      ref Int32 idx,
      PropertySignature sig
      )
   {
      if ( sig != null )
      {
         var starter = SignatureStarters.Property;
         if ( sig.HasThis )
         {
            starter |= SignatureStarters.HasThis;
         }
         info
            .AddSigStarterByte( ref idx, starter )
            .AddCompressedUInt32( ref idx, sig.Parameters.Count )
            .WriteCustomModifiers( ref idx, sig.CustomModifiers )
            .WriteTypeSignature( ref idx, sig.PropertyType );

         foreach ( var param in sig.Parameters )
         {
            info.WriteParameterSignature( ref idx, param );
         }
      }
      return info;
   }

   private static ResizableArray<Byte> WriteMethodSpecSignature(
      this ResizableArray<Byte> info,
      ref Int32 idx,
      GenericMethodSignature sig
      )
   {
      info
         .AddSigStarterByte( ref idx, SignatureStarters.MethodSpecGenericInst )
         .AddCompressedUInt32( ref idx, sig.GenericArguments.Count );
      foreach ( var gArg in sig.GenericArguments )
      {
         info.WriteTypeSignature( ref idx, gArg );
      }
      return info;
   }

   private static ResizableArray<Byte> AddSigStarterByte( this ResizableArray<Byte> info, ref Int32 idx, SignatureStarters starter )
   {
      return info.WriteByteToBytes( ref idx, (Byte) starter );
   }

   private static ResizableArray<Byte> AddSigByte( this ResizableArray<Byte> info, ref Int32 idx, SignatureElementTypes sig )
   {
      return info.WriteByteToBytes( ref idx, (Byte) sig );
   }

   private static ResizableArray<Byte> AddTDRSToken( this ResizableArray<Byte> info, ref Int32 idx, TableIndex token )
   {
      return info.AddCompressedUInt32( ref idx, TableIndex.EncodeTypeDefOrRefOrSpec( token.OneBasedToken ) );
   }

   internal static ResizableArray<Byte> AddCompressedUInt32( this ResizableArray<Byte> info, ref Int32 idx, Int32 value )
   {
      info.EnsureThatCanAdd( idx, (Int32) BitUtils.GetEncodedUIntSize( value ) )
         .Array.CompressUInt32( ref idx, value );
      return info;
   }

   internal static ResizableArray<Byte> AddCompressedInt32( this ResizableArray<Byte> info, ref Int32 idx, Int32 value )
   {
      info.EnsureThatCanAdd( idx, (Int32) BitUtils.GetEncodedIntSize( value ) )
         .Array.CompressInt32( ref idx, value );
      return info;
   }

   private static ResizableArray<Byte> AddCAString( this ResizableArray<Byte> info, ref Int32 idx, String str )
   {
      if ( str == null )
      {
         info.WriteByteToBytes( ref idx, 0xFF );
      }
      else
      {
         var encoding = MetaDataConstants.SYS_STRING_ENCODING;
         var size = encoding.GetByteCount( str );
         info
            .AddCompressedUInt32( ref idx, size )
            .EnsureThatCanAdd( idx, size );
         idx += encoding.GetBytes( str, 0, str.Length, info.Array, idx );
      }
      return info;
   }

   internal static U FirstOfTypeOrAddDefault<T, U>( this IList<T> list, Int32 insertIdx, Func<U, Boolean> additionalFilter, Func<U> defaultFactory )
      where U : T
   {
      var correctTyped = list.OfType<U>();
      if ( additionalFilter != null )
      {
         correctTyped = correctTyped.Where( additionalFilter );
      }
      var retVal = correctTyped.FirstOrDefault();
      if ( retVal == null )
      {
         retVal = defaultFactory();
         if ( insertIdx >= 0 )
         {
            list.Insert( insertIdx, retVal );
         }
         else
         {
            list.Add( retVal );
         }
      }

      return retVal;
   }
}