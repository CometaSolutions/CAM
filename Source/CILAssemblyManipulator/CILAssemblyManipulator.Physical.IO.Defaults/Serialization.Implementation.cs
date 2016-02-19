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
extern alias CAMPhysicalIO;
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;

using CAMPhysicalIO;
using CAMPhysicalIO::CILAssemblyManipulator.Physical.IO;

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.IO;
using CILAssemblyManipulator.Physical.IO.Defaults;
using CILAssemblyManipulator.Physical.Meta;
using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TabularMetaData;
using TabularMetaData.Meta;

namespace CILAssemblyManipulator.Physical.IO.Defaults
{
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

   public delegate ColumnSerializationFunctionality CreateSerializationSupportDelegate( DefaultColumnSerializationSupportCreationArgs args );

   // Sets a property on a raw row
   public delegate void RawRowColumnSetterDelegate<TRawRow>( TRawRow rawRow, Int32 value );

   // Sets a property on a normal row
   public delegate void RowColumnSerializationSetterDelegate<TRow, TValue>( ColumnFunctionalityArgs<TRow, RowReadingArguments> args, TValue value )
      where TRow : class;

   // Sets a raw value property (Method IL, FieldRVA Data column, Manifest Resource Data) on a normal row
   public delegate void RowRawColumnSetterDelegate<TRow>( ColumnFunctionalityArgs<TRow, RawValueProcessingArgs> args, Int32 rawValue )
      where TRow : class;

   // Gets the heap index that will be written to table stream
   public delegate Int32 RowHeapColumnGetterDelegate<TRow>( ColumnFunctionalityArgs<TRow, RowHeapFillingArguments> args )
      where TRow : class;


   public delegate SectionPartWithDataReferenceTargets RawColumnSectionPartCreationDelegte<TRow>( CILMetaData md, WriterMetaDataStreamContainer mdStreamContainer );


   public class DefaultColumnSerializationInfo<TRawRow, TRow> : DefaultColumnSerializationInfo<TRow>
      where TRawRow : class
      where TRow : class
   {

      public DefaultColumnSerializationInfo(
         CreateSerializationSupportDelegate serializationCreator,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSerializationSetterDelegate<TRow, Int32> setter,
         RowColumnGetterDelegate<TRow, Int32> constExtractor
         )
         : this(
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
         CreateSerializationSupportDelegate serializationCreator,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSerializationSetterDelegate<TRow, Int32> setter,
         RowRawColumnSetterDelegate<TRow> rawValueProcessor,
         RawColumnSectionPartCreationDelegte<TRow> rawColummnSectionPartCreator
         )
         : this(
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
         String heapIndexName,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSerializationSetterDelegate<TRow, Int32> setter,
         RowHeapColumnGetterDelegate<TRow> heapValueExtractor
         )
         : this(
              args => args.IsWide( heapIndexName ) ? ColumnSerializationSupport_Constant32.Instance : ColumnSerializationSupport_Constant16.Instance,
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
         CreateSerializationSupportDelegate creator,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSerializationSetterDelegate<TRow, Int32> setter,
         RowRawColumnSetterDelegate<TRow> rawValueProcessor,
         RowHeapColumnGetterDelegate<TRow> heapValueExtractor,
         RowColumnGetterDelegate<TRow, Int32> constExtractor,
         RawColumnSectionPartCreationDelegte<TRow> rawColummnSectionPartCreator
         )
      {
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

         this.RawSetter = rawSetter;
         this.Setter = setter;
         this.SerializationSupportCreator = creator;
         this.RawColummnSectionPartCreator = rawColummnSectionPartCreator;
         this.RawValueProcessor = rawValueProcessor;
         this.HeapValueExtractor = heapValueExtractor;
         this.ConstantExtractor = constExtractor;
      }

      public CreateSerializationSupportDelegate SerializationSupportCreator { get; }

      // Reading
      public RawRowColumnSetterDelegate<TRawRow> RawSetter { get; }
      public RowColumnSerializationSetterDelegate<TRow, Int32> Setter { get; }
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
      public static Int32 GetCodedTableSize( ArrayQuery<Int32> tableSizes, ArrayQuery<Int32?> referencedTables )
      {
         Int32 max = 0;
         var len = referencedTables.Count;
         for ( var i = 0; i < len; ++i )
         {
            var current = referencedTables[i];
            if ( current.HasValue )
            {
               max = Math.Max( max, tableSizes[current.Value] );
            }
         }
         return max < ( UInt16.MaxValue >> CodedTableIndexDecoder.GetTagBitSize( referencedTables.Count ) ) ?
            2 :
            4;
      }


      public static DefaultColumnSerializationInfo<TRawRow, TRow> Constant8<TRawRow, TRow>(
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSerializationSetterDelegate<TRow, Int32> setter,
         RowColumnGetterDelegate<TRow, Int32> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            args => ColumnSerializationSupport_Constant8.Instance,
            rawSetter,
            setter,
            getter
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> Constant16<TRawRow, TRow>(
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSerializationSetterDelegate<TRow, Int32> setter,
         RowColumnGetterDelegate<TRow, Int32> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            args => ColumnSerializationSupport_Constant16.Instance,
            rawSetter,
            setter,
            getter
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> Constant32<TRawRow, TRow>(
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSerializationSetterDelegate<TRow, Int32> setter,
         RowColumnGetterDelegate<TRow, Int32> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            args => ColumnSerializationSupport_Constant32.Instance,
            rawSetter,
            setter,
            getter
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> SimpleReference<TRawRow, TRow>(
         Tables targetTable,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSerializationSetterDelegate<TRow, TableIndex> setter,
         RowColumnGetterDelegate<TRow, TableIndex> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
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
         ArrayQuery<Int32?> targetTables,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSerializationSetterDelegate<TRow, TableIndex?> setter,
         RowColumnGetterDelegate<TRow, TableIndex?> getter
         )
         where TRawRow : class
         where TRow : class
      {
         var decoder = new CodedTableIndexDecoder( targetTables );

         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            args => GetCodedTableSize( args.TableSizes, targetTables ) < sizeof( Int32 ) ? ColumnSerializationSupport_Constant16.Instance : ColumnSerializationSupport_Constant32.Instance,
            rawSetter,
            ( args, value ) => setter( args, decoder.DecodeTableIndex( value ) ),
            row => decoder.EncodeTableIndex( getter( row ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBCustom<TRawRow, TRow>(
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         Action<ColumnFunctionalityArgs<TRow, RowReadingArguments>, Int32, ReaderBLOBStreamHandler> setter, // TODO delegat-ize these
         Func<ColumnFunctionalityArgs<TRow, RowHeapFillingArguments>, Byte[]> blobCreator
         )
         where TRawRow : class
         where TRow : class
      {
         return HeapIndex<TRawRow, TRow>(
            MetaDataConstants.BLOB_STREAM_NAME,
            rawSetter,
            ( args, value ) => setter( args, value, args.RowArgs.MDStreamContainer.BLOBs ),
            ( args ) => args.RowArgs.MDStreamContainer.BLOBs.RegisterBLOB( blobCreator( args ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> GUID<TRawRow, TRow>(
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSerializationSetterDelegate<TRow, Guid?> setter,
         RowColumnGetterDelegate<TRow, Guid?> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return HeapIndex<TRawRow, TRow>(
            MetaDataConstants.GUID_STREAM_NAME,
            rawSetter,
            ( args, value ) => setter( args, args.RowArgs.MDStreamContainer.GUIDs.GetGUID( value ) ),
            args => args.RowArgs.MDStreamContainer.GUIDs.RegisterGUID( getter( args.Row ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> SystemString<TRawRow, TRow>(
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSerializationSetterDelegate<TRow, String> setter,
         RowColumnGetterDelegate<TRow, String> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return HeapIndex<TRawRow, TRow>(
            MetaDataConstants.SYS_STRING_STREAM_NAME,
            rawSetter,
            ( args, value ) => setter( args, args.RowArgs.MDStreamContainer.SystemStrings.GetString( value ) ),
            args => args.RowArgs.MDStreamContainer.SystemStrings.RegisterString( getter( args.Row ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> HeapIndex<TRawRow, TRow>(
         String heapName,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSerializationSetterDelegate<TRow, Int32> setter,
         RowHeapColumnGetterDelegate<TRow> heapValueExtractor
         )
         where TRawRow : class
         where TRow : class
      {

         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            heapName,
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
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowRawColumnSetterDelegate<TRow> rawValueProcessor,
         RawColumnSectionPartCreationDelegte<TRow> rawColummnSectionPartCreator
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
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

      private readonly ArrayQuery<Int32?> _tablesArray;
      private readonly IDictionary<Int32, Int32> _tablesDictionary;
      private readonly Int32 _tagBitMask;
      private readonly Int32 _tagBitSize;

      public CodedTableIndexDecoder(
         ArrayQuery<Int32?> possibleTables
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
                  new TableIndex( (Tables) tableNullable.Value, (Int32) ( rowIdx - 1 ) ) :
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
            retVal = this._tablesDictionary.TryGetValue( (Int32) tIdxValue.Table, out tableArrayIndex ) ?
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

   public class DefaultTableSerializationInfo<TRawRow, TRow> : TableSerializationInfo
      where TRawRow : class
      where TRow : class
   {

      private readonly DefaultColumnSerializationInfo<TRawRow, TRow>[] _columns;
      private readonly Func<TRow> _rowFactory;
      private readonly Func<TRawRow> _rawRowFactory;
      private readonly TableSerializationInfoCreationArgs _creationArgs;

      public DefaultTableSerializationInfo(
         Tables table,
         Boolean isSorted,
         IEnumerable<DefaultColumnSerializationInfo<TRawRow, TRow>> columns,
         Func<TRow> rowFactory,
         Func<TRawRow> rawRowFactory,
         TableSerializationInfoCreationArgs args
         )
      {
         ArgumentValidator.ValidateNotNull( "Columns", columns );
         ArgumentValidator.ValidateNotNull( "Row factory", rowFactory );
         ArgumentValidator.ValidateNotNull( "Raw row factory", rawRowFactory );

         this.Table = table;
         this.IsSorted = isSorted;
         this._rowFactory = rowFactory;
         this._rawRowFactory = rawRowFactory;
         this._creationArgs = args;
         this._columns = columns.ToArray();
         ArgumentValidator.ValidateAllNotNull( "Columns", this._columns );
      }

      public Tables Table { get; }

      public Boolean IsSorted { get; }

      public Int32 DataReferenceColumnCount
      {
         get
         {
            return this._columns.Count( c => c.RawValueProcessor != null );
         }
      }


      public Int32 MetaDataStreamReferenceColumnCount
      {
         get
         {
            return this._columns.Count( c => c.HeapValueExtractor != null );
         }
      }

      public void PopulateDataReferences(
         RawValueProcessingArgs args
         )
      {
         var md = args.MetaData;
         var tblEnum = this.Table;
         MetaDataTable tbl;
         if ( md.TryGetByTable( (Int32) tblEnum, out tbl )
            && tbl.GetRowCount() > 0
            )
         {
            var table = (MetaDataTable<TRow>) tbl;
            var cols = this._columns
               .Select( ( c, cIdx ) => Tuple.Create( c.RawValueProcessor, cIdx ) )
               .Where( p => p.Item1 != null )
               .ToArray();
            if ( cols.Length > 0 )
            {
               var list = table.TableContents;
               var dataRefs = args.ImageInformation.CLIInformation.DataReferences.DataReferences[tblEnum];
               var dataRefColCount = dataRefs.Count;
               for ( var i = 0; i < list.Count; ++i )
               {
                  var cArgs = new ColumnFunctionalityArgs<TRow, RawValueProcessingArgs>( i, list[i], args );
                  for ( var cur = 0; cur < dataRefColCount; ++cur )
                  {
                     var tuple = cols[cur];
                     try
                     {
                        tuple.Item1( cArgs, (Int32) dataRefs[cur][i] );
                     }
                     catch ( Exception exc )
                     {
                        if ( this._creationArgs.ErrorHandler.ProcessSerializationError( null, exc, this.Table, i, tuple.Item2 ) )
                        {
                           throw;
                        }
                     }
                     ++cur;
                  }
               }
            }
         }
      }

      public IEnumerable<SectionPartWithDataReferenceTargets> CreateDataReferenceSectionParts(
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

      public void ExtractMetaDataStreamReferences(
         CILMetaData md,
         ColumnValueStorage<Int32> storage,
         WriterMetaDataStreamContainer mdStreamContainer,
         ResizableArray<Byte> array,
         ArrayQuery<Byte> publicKey
         )
      {
         MetaDataTable tbl;
         if ( md.TryGetByTable( (Int32) this.Table, out tbl ) )
         {
            var table = (MetaDataTable<TRow>) tbl;
            var cols = this._columns
               .Select( c => c.HeapValueExtractor )
               .Where( e => e != null )
               .ToArray();
            if ( cols.Length > 0 )
            {
               var list = table.TableContents;
               var rArgs = new RowHeapFillingArguments( mdStreamContainer, array, publicKey, md );
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
                        // TODO error reporting in writing phase!
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
         ArrayQuery<ArrayQuery<Int64>> rawValueProvider,
         ColumnValueStorage<Int32> heapIndices
         )
      {
         var list = ( (MetaDataTable<TRow>) table ).TableContents;
         if ( list.Count > 0 )
         {
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
                     yield return (Int32) rawValueProvider[rawIdx][rowIdx];
                     ++rawIdx;
                  }
                  else
                  {
                     // TODO pass error handler here, and process error.
                     yield return 0;
                  }
               }
            }
         }
      }

      public TableSerializationFunctionality CreateSupport( DefaultColumnSerializationSupportCreationArgs supportArgs )
      {
         return new DefaultTableSerializationFunctionality<TRawRow, TRow>(
            this,
            this._columns,
            supportArgs,
            this._rowFactory,
            this._rawRowFactory,
            this._creationArgs.ErrorHandler
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
            DefaultColumnSerializationSupportCreationArgs args
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
            DefaultColumnSerializationSupportCreationArgs args
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
         private readonly RowColumnSerializationSetterDelegate<TRow, Int32> _setter;
         private readonly Tables _table;
         private readonly Int32 _columnIndex;
         private readonly EventHandler<SerializationErrorEventArgs> _errorHandler;

         internal ColumnSerializationInstance_NormalValue(
            DefaultColumnSerializationInfo<TRawRow, TRow> serializationInfo,
            DefaultColumnSerializationSupportCreationArgs args,
            Tables table,
            Int32 columnIndex,
            EventHandler<SerializationErrorEventArgs> errorHandler
            )
            : base( serializationInfo, args )
         {
            var setter = serializationInfo.Setter;
            ArgumentValidator.ValidateNotNull( "Setter", setter );
            this._setter = setter;
            this._table = table;
            this._columnIndex = columnIndex;
            this._errorHandler = errorHandler;
         }

         public override void SetNormalRowValue( RowReadingArguments rowArgs, ref Int32 idx, TRow row, Int32 rowIndex )
         {
            try
            {
               this._setter( new ColumnFunctionalityArgs<TRow, RowReadingArguments>( rowIndex, row, rowArgs ), this.Functionality.ReadRawValue( rowArgs.Array, ref idx ) );
            }
            catch ( Exception exc )
            {
               if ( this._errorHandler.ProcessSerializationError( null, exc, this._table, rowIndex, this._columnIndex ) )
               {
                  throw;
               }
            }
         }
      }

      public DefaultTableSerializationFunctionality(
         TableSerializationInfo tableSerializationInfo,
         IEnumerable<DefaultColumnSerializationInfo<TRawRow, TRow>> columns,
         DefaultColumnSerializationSupportCreationArgs args,
         Func<TRow> rowFactory,
         Func<TRawRow> rawRowFactory,
         EventHandler<SerializationErrorEventArgs> errorHandler
         )
      {
         ArgumentValidator.ValidateNotNull( "Table serialization info", tableSerializationInfo );
         ArgumentValidator.ValidateNotNull( "Columns", columns );
         ArgumentValidator.ValidateNotNull( "Row factory", rowFactory );
         ArgumentValidator.ValidateNotNull( "Raw row factory", rawRowFactory );


         this._rowFactory = rowFactory;
         this._rawRowFactory = rawRowFactory;
         this._columnArray = columns
            .Select( ( c, cIdx ) => c.Setter == null ? (ColumnSerializationInstance) new ColumnSerializationInstance_RawValue( c, args ) : new ColumnSerializationInstance_NormalValue( c, args, tableSerializationInfo.Table, cIdx, errorHandler ) )
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

   internal static Boolean ProcessSerializationError( this EventHandler<SerializationErrorEventArgs> handler, Object sender, Exception error, Tables table, Int32 rowIndex, Int32 columnIndex )
   {
      var retVal = false;
      if ( handler != null )
      {
         var args = new TableStreamSerializationErrorEventArgs( error, table, rowIndex, columnIndex );
         handler.Invoke( sender, args );
         retVal = args.RethrowException;
      }

      return retVal;
   }
}