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
   //public abstract class DefaultColumnSerializationInfo
   //{
   //   internal DefaultColumnSerializationInfo()
   //   {

   //   }
   //}


   //public abstract class DefaultColumnSerializationInfo<TRow> : DefaultColumnSerializationInfo
   //   where TRow : class
   //{
   //   internal DefaultColumnSerializationInfo()
   //      : base()
   //   {

   //   }
   //}

   /// <summary>
   /// This delegate defines signature for methods creating <see cref="ColumnSerializationBinaryFunctionality"/>.
   /// </summary>
   /// <param name="args">The <see cref="TableSerializationBinaryFunctionalityCreationArgs"/> containing information to create <see cref="ColumnSerializationBinaryFunctionality"/>.</param>
   /// <returns>A new instance of <see cref="ColumnSerializationBinaryFunctionality"/>.</returns>
   /// <remarks>
   /// This delegate is used by <see cref="TableSerializationBinaryFunctionalityImpl{TRow, TRawRow}"/> in order to create <see cref="ColumnSerializationBinaryFunctionality"/> instances.
   /// </remarks>
   public delegate ColumnSerializationBinaryFunctionality CreateSerializationSupportDelegate( TableSerializationBinaryFunctionalityCreationArgs args );

   /// <summary>
   /// This delegate defines signature for methods setting column values on raw rows (returned by <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/>).
   /// </summary>
   /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
   /// <param name="rawRow">The instance of the raw row.</param>
   /// <param name="value">The integer value to set.</param>
   /// <remarks>
   /// This delegate is used by <see cref="TableSerializationBinaryFunctionalityImpl{TRow, TRawRow}.ReadRawRow"/> to set columns of the raw row.
   /// </remarks>
   public delegate void RawRowColumnSetterDelegate<in TRawRow>( TRawRow rawRow, Int32 value );

   /// <summary>
   /// This delegate defines signature for methods setting column values on normal (not raw) rows (part of the tables in <see cref="CILMetaData"/>).
   /// The column value is assumed to be normal value, and not data reference.
   /// For data reference column values (e.g. <see cref="MethodDefinition.IL"/>), see <see cref="RowColumnDataReferenceSetterDelegate{TRow}"/>.
   /// </summary>
   /// <typeparam name="TRow">The type of the row.</typeparam>
   /// <param name="args">The <see cref="ColumnValueArgs{TRow, TRowArgs}"/> containing row and <see cref="RowReadingArguments"/>.</param>
   /// <param name="value">The value, as integer.</param>
   /// <remarks>
   /// This delegate is used by <see cref="TableSerializationBinaryFunctionalityImpl{TRow, TRawRow}.ReadRows"/> method for setting column values for rows.
   /// </remarks>
   public delegate void RowColumnNormalSetterDelegate<TRow>( ColumnValueArgs<TRow, RowReadingArguments> args, Int32 value )
      where TRow : class;

   /// <summary>
   /// This delegate defines signature for methods setting column values on normal (not raw) rows (part of the tables in <see cref="CILMetaData"/>).
   /// The column value is assumed to be data reference, such as <see cref="MethodDefinition.IL"/>, as opposed to normal value columns.
   /// For normal column values (e.g. <see cref="TypeDefinition.Attributes"/>), see <see cref="RowColumnNormalSetterDelegate{TRow}"/>.
   /// </summary>
   /// <typeparam name="TRow">The type of the row.</typeparam>
   /// <param name="args">The <see cref="ColumnValueArgs{TRow, TRowArgs}"/> containing row and <see cref="DataReferencesProcessingArgs"/>.</param>
   /// <param name="dataReference">The data reference, as integer.</param>
   /// <remarks>
   /// This delegate is used by <see cref="TableSerializationLogicalFunctionalityImpl{TRow, TRawRow}.PopulateDataReferences"/> when setting data reference columns.
   /// </remarks>
   public delegate void RowColumnDataReferenceSetterDelegate<TRow>( ColumnValueArgs<TRow, DataReferencesProcessingArgs> args, Int32 dataReference )
      where TRow : class;

   /// <summary>
   /// This delegate defines signature for methods returning an index to other meta data stream than table stream (e.g. system strings, blobs, etc).
   /// </summary>
   /// <typeparam name="TRow">The type of the row.</typeparam>
   /// <param name="args">The <see cref="ColumnValueArgs{TRow, TRowArgs}"/> containing row and <see cref="RowHeapFillingArguments"/>.</param>
   /// <returns>An index to other meta data stream for the related column of this row.</returns>
   /// <remarks>
   /// This delegate is used by <see cref="TableSerializationLogicalFunctionalityImpl{TRow, TRawRow}.ExtractMetaDataStreamReferences"/> when populating other meta data streams.
   /// </remarks>
   public delegate Int32 RowHeapColumnGetterDelegate<TRow>( ColumnValueArgs<TRow, RowHeapFillingArguments> args )
      where TRow : class;

   /// <summary>
   /// This delegate defines signature for methods returning instances of <see cref="SectionPartWithDataReferenceTargets"/>, representing a continuous range of targets of data reference columns.
   /// </summary>
   /// <typeparam name="TRow">The type of the row.</typeparam>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="mdStreamContainer">The <see cref="WriterMetaDataStreamContainer"/> containing other streams.</param>
   /// <returns>An instance of <see cref="SectionPartWithDataReferenceTargets"/>.</returns>
   /// <remarks>
   /// For example, <see cref="MethodILDefinition"/> is a target for data reference column <see cref="MethodDefinition.IL"/>, so the returned <see cref="SectionPartWithDataReferenceTargets"/> could represent the serialized <see cref="MethodILDefinition"/>s.
   /// </remarks>
   /// <seealso cref="SectionPartWithDataReferenceTargets"/>
   public delegate SectionPartWithDataReferenceTargets DataReferenceColumnSectionPartCreationDelegate<TRow>( CILMetaData md, WriterMetaDataStreamContainer mdStreamContainer );

   /// <summary>
   /// This class combines all delegates participating in serializing and deserializing single column value of a single row.
   /// </summary>
   /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
   /// <typeparam name="TRow">The type of normal row (part of the tables in <see cref="CILMetaData"/>).</typeparam>
   /// <remarks>
   /// The instances of this class are not meant to be directly created.
   /// Instead, use methods of <see cref="MetaDataColumnInformationFactory"/> or <see cref="DefaultColumnSerializationInfoFactory"/>.
   /// </remarks>
   public class DefaultColumnSerializationInfo<TRow, TRawRow> // : DefaultColumnSerializationInfo<TRow>
      where TRawRow : class
      where TRow : class
   {

      /// <summary>
      /// This constructor is used for columns, which have value embedded directly in table stream.
      /// </summary>
      /// <param name="serializationCreator">The value for <see cref="SerializationSupportCreator"/>.</param>
      /// <param name="rawSetter">The value for <see cref="RawSetter"/>.</param>
      /// <param name="setter">The value for <see cref="Setter"/>.</param>
      /// <param name="constExtractor">The value for <see cref="ConstantExtractor"/>.</param>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="serializationCreator"/>, <paramref name="rawSetter"/>, <paramref name="setter"/>, or <paramref name="constExtractor"/> is <c>null</c>.</exception>
      public DefaultColumnSerializationInfo(
         CreateSerializationSupportDelegate serializationCreator,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnNormalSetterDelegate<TRow> setter,
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
         ArgumentValidator.ValidateNotNull( "Setter", setter );
         ArgumentValidator.ValidateNotNull( "Const extractor", constExtractor );
      }

      /// <summary>
      /// This constructor is used for columns, the value of which is reference (e.g. RVA) to actual data contained elsewhere in image (one such column is <see cref="MethodDefinition.IL"/>).
      /// </summary>
      /// <param name="serializationCreator">The value for <see cref="SerializationSupportCreator"/>.</param>
      /// <param name="rawSetter">The value for <see cref="RawSetter"/>.</param>
      /// <param name="dataReferenceSetter">The value for <see cref="DataReferenceSetter"/>.</param>
      /// <param name="dataReferenceColumnSectionPartCreator">The value <see cref="DataReferenceColumnSectionPartCreator"/>.</param>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="serializationCreator"/>, <paramref name="rawSetter"/>, <paramref name="dataReferenceSetter"/>, or <paramref name="dataReferenceColumnSectionPartCreator"/> is <c>null</c>.</exception>
      public DefaultColumnSerializationInfo(
         CreateSerializationSupportDelegate serializationCreator,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnDataReferenceSetterDelegate<TRow> dataReferenceSetter,
         DataReferenceColumnSectionPartCreationDelegate<TRow> dataReferenceColumnSectionPartCreator
         )
         : this(
              serializationCreator,
              rawSetter,
              null,
              dataReferenceSetter,
              null,
              null,
              dataReferenceColumnSectionPartCreator
              )
      {
         ArgumentValidator.ValidateNotNull( "Data reference setter", dataReferenceSetter );
         ArgumentValidator.ValidateNotNull( "Data reference column section creator", dataReferenceColumnSectionPartCreator );
      }

      /// <summary>
      /// This constructor is used for columns, the value of which is index into other meta data stream (e.g. system strings, blobs, etc).
      /// </summary>
      /// <param name="heapIndexName">The textual name of the other meta data stream.</param>
      /// <param name="rawSetter">The value for <see cref="RawSetter"/>.</param>
      /// <param name="setter">The value for <see cref="Setter"/>.</param>
      /// <param name="heapValueExtractor">The value for <see cref="HeapValueExtractor"/>.</param>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="rawSetter"/>, <paramref name="setter"/>, or <paramref name="heapValueExtractor"/>.</exception>
      public DefaultColumnSerializationInfo(
         String heapIndexName,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnNormalSetterDelegate<TRow> setter,
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
         ArgumentValidator.ValidateNotNull( "Setter", setter );
         ArgumentValidator.ValidateNotNull( "Heap value extractor", heapValueExtractor );
      }

      /// <summary>
      /// This constructor is used by other constructors, and can be used by subclasses.
      /// It accepts all the possible delegates.
      /// </summary>
      /// <param name="serializationCreator">The value for <see cref="SerializationSupportCreator"/>.</param>
      /// <param name="rawSetter">The value for <see cref="RawSetter"/>.</param>
      /// <param name="setter">The value for <see cref="Setter"/>.</param>
      /// <param name="dataReferenceSetter">The value for <see cref="DataReferenceSetter"/>.</param>
      /// <param name="heapValueExtractor">The value for <see cref="HeapValueExtractor"/>.</param>
      /// <param name="constExtractor">The value for <see cref="ConstantExtractor"/>.</param>
      /// <param name="dataReferenceColumnSectionPartCreator">The value <see cref="DataReferenceColumnSectionPartCreator"/>.</param>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="serializationCreator"/> or <paramref name="rawSetter"/> is <c>null</c>.</exception>
      protected DefaultColumnSerializationInfo(
         CreateSerializationSupportDelegate serializationCreator,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnNormalSetterDelegate<TRow> setter,
         RowColumnDataReferenceSetterDelegate<TRow> dataReferenceSetter,
         RowHeapColumnGetterDelegate<TRow> heapValueExtractor,
         RowColumnGetterDelegate<TRow, Int32> constExtractor,
         DataReferenceColumnSectionPartCreationDelegate<TRow> dataReferenceColumnSectionPartCreator
         )
      {
         ArgumentValidator.ValidateNotNull( "Serialization support creator", serializationCreator );
         ArgumentValidator.ValidateNotNull( "Raw setter", rawSetter );


         this.RawSetter = rawSetter;
         this.Setter = setter;
         this.SerializationSupportCreator = serializationCreator;
         this.DataReferenceColumnSectionPartCreator = dataReferenceColumnSectionPartCreator;
         this.DataReferenceSetter = dataReferenceSetter;
         this.HeapValueExtractor = heapValueExtractor;
         this.ConstantExtractor = constExtractor;
      }

      /// <summary>
      /// Gets the <see cref="CreateSerializationSupportDelegate"/> used to create <see cref="ColumnSerializationBinaryFunctionality"/> for this column.
      /// </summary>
      /// <value>The <see cref="CreateSerializationSupportDelegate"/> used to create <see cref="ColumnSerializationBinaryFunctionality"/> for this column.</value>
      public CreateSerializationSupportDelegate SerializationSupportCreator { get; }


      /// <summary>
      /// Gets the <see cref="RawRowColumnSetterDelegate{TRawRow}"/> used to set column value of the raw row.
      /// </summary>
      /// <value>The <see cref="RawRowColumnSetterDelegate{TRawRow}"/> used to set column value of the raw row.</value>
      /// <remarks>
      /// This property is used during deserialization (reading) process.
      /// </remarks>
      public RawRowColumnSetterDelegate<TRawRow> RawSetter { get; }

      /// <summary>
      /// Gets the <see cref="RowColumnNormalSetterDelegate{TRow}"/> used to set column value of the normal row.
      /// </summary>
      /// <value>The <see cref="RowColumnNormalSetterDelegate{TRow}"/> used to set column value of the normal row.</value>
      /// <remarks>
      /// This property is used during deserialization (reading) process.
      /// </remarks>
      public RowColumnNormalSetterDelegate<TRow> Setter { get; }

      /// <summary>
      /// Gets the <see cref="RowColumnDataReferenceSetterDelegate{TRow}"/> used to set the column value of the normal row, from data reference.
      /// </summary>
      /// <value>The <see cref="RowColumnDataReferenceSetterDelegate{TRow}"/> used to set the column value of the normal row, from data reference.</value>
      /// <remarks>
      /// This property is used during deserialization (reading) process.
      /// </remarks>
      public RowColumnDataReferenceSetterDelegate<TRow> DataReferenceSetter { get; }

      /// <summary>
      /// Gets the <see cref="DataReferenceColumnSectionPartCreationDelegate{TRow}"/> used to create <see cref="SectionPartWithDataReferenceTargets"/> for data reference column targets.
      /// </summary>
      /// <value>The <see cref="DataReferenceColumnSectionPartCreationDelegate{TRow}"/> used to create <see cref="SectionPartWithDataReferenceTargets"/> for data reference column targets.</value>
      /// <remarks>
      /// This property is used during serialization (writing) process.
      /// </remarks>
      public DataReferenceColumnSectionPartCreationDelegate<TRow> DataReferenceColumnSectionPartCreator { get; }

      /// <summary>
      /// Gets the <see cref="RowHeapColumnGetterDelegate{TRow}"/> used to extract column value as reference to other meta data streams.
      /// </summary>
      /// <value>The <see cref="RowHeapColumnGetterDelegate{TRow}"/> used to extract column value as reference to other meta data streams.</value>
      /// <remarks>
      /// This property is used during serialization (writing) process.
      /// </remarks>
      public RowHeapColumnGetterDelegate<TRow> HeapValueExtractor { get; }

      /// <summary>
      /// Gets the <see cref="RowColumnGetterDelegate{TRow, TValue}"/> used to extract column value as embedded value in table stream.
      /// </summary>
      /// <value>The <see cref="RowColumnGetterDelegate{TRow, TValue}"/> used to extract column value as embedded value in table stream.</value>
      /// <remarks>
      /// This property is used during serialization (writing) process.
      /// </remarks>
      public RowColumnGetterDelegate<TRow, Int32> ConstantExtractor { get; }
   }

   /// <summary>
   /// This is static class used to create instances of <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/>.
   /// </summary>
   /// <remarks>
   /// This class and methods of it are typically not meant to be used directly, instead one should use <see cref="MetaDataColumnInformationFactory"/>.
   /// </remarks>
   public static class DefaultColumnSerializationInfoFactory
   {
      /// <summary>
      /// Returns a <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/> for <c>1</c>-byte value embedded in table stream.
      /// </summary>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <typeparam name="TRow">The type of the normal row (part of the tables in <see cref="CILMetaData"/>).</typeparam>
      /// <param name="rawSetter">The <see cref="RawRowColumnSetterDelegate{TRawRow}"/>.</param>
      /// <param name="setter">The <see cref="RowColumnSetterDelegate{TRow, TValue}"/>.</param>
      /// <param name="getter">The <see cref="RowColumnGetterDelegate{TRow, TValue}"/>.</param>
      /// <returns>A new instance of <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/>.</returns>
      public static DefaultColumnSerializationInfo<TRow, TRawRow> Constant8<TRow, TRawRow>(
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, Byte> setter,
         RowColumnGetterDelegate<TRow, Byte> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRow, TRawRow>(
            args => ColumnSerializationSupport_Constant8.Instance,
            rawSetter,
            ( args, v ) => setter( args.Row, (Byte) v ),
            row => getter( row )
            );
      }

      /// <summary>
      /// Returns a <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/> for <c>2</c>-byte value embedded in table stream.
      /// </summary>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <typeparam name="TRow">The type of the normal row (part of the tables in <see cref="CILMetaData"/>).</typeparam>
      /// <param name="rawSetter">The <see cref="RawRowColumnSetterDelegate{TRawRow}"/>.</param>
      /// <param name="setter">The <see cref="RowColumnSetterDelegate{TRow, TValue}"/>.</param>
      /// <param name="getter">The <see cref="RowColumnGetterDelegate{TRow, TValue}"/>.</param>
      /// <returns>A new instance of <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/>.</returns>
      public static DefaultColumnSerializationInfo<TRow, TRawRow> Constant16<TRow, TRawRow>(
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, Int16> setter,
         RowColumnGetterDelegate<TRow, Int16> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRow, TRawRow>(
            args => ColumnSerializationSupport_Constant16.Instance,
            rawSetter,
            ( args, v ) => setter( args.Row, (Int16) v ),
            row => getter( row )
            );
      }

      /// <summary>
      /// Returns a <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/> for <c>4</c>-byte value embedded in table stream.
      /// </summary>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <typeparam name="TRow">The type of the normal row (part of the tables in <see cref="CILMetaData"/>).</typeparam>
      /// <param name="rawSetter">The <see cref="RawRowColumnSetterDelegate{TRawRow}"/>.</param>
      /// <param name="setter">The <see cref="RowColumnSetterDelegate{TRow, TValue}"/>.</param>
      /// <param name="getter">The <see cref="RowColumnGetterDelegate{TRow, TValue}"/>.</param>
      /// <returns>A new instance of <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/>.</returns>
      public static DefaultColumnSerializationInfo<TRow, TRawRow> Constant32<TRow, TRawRow>(
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, Int32> setter,
         RowColumnGetterDelegate<TRow, Int32> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRow, TRawRow>(
            args => ColumnSerializationSupport_Constant32.Instance,
            rawSetter,
            ( args, v ) => setter( args.Row, v ),
            getter
            );
      }

      /// <summary>
      /// Returns a <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/> for column value, which is table index into one pre-defined table.
      /// </summary>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <typeparam name="TRow">The type of the normal row (part of the tables in <see cref="CILMetaData"/>).</typeparam>
      /// <param name="targetTable">The pre-defined table as <see cref="Tables"/>.</param>
      /// <param name="rawSetter">The <see cref="RawRowColumnSetterDelegate{TRawRow}"/>.</param>
      /// <param name="setter">The <see cref="RowColumnSetterDelegate{TRow, TValue}"/>.</param>
      /// <param name="getter">The <see cref="RowColumnGetterDelegate{TRow, TValue}"/>.</param>
      /// <returns>A new instance of <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/>.</returns>
      public static DefaultColumnSerializationInfo<TRow, TRawRow> SimpleReference<TRow, TRawRow>(
         Tables targetTable,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, TableIndex> setter,
         RowColumnGetterDelegate<TRow, TableIndex> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRow, TRawRow>(
            args => args.TableSizes[(Int32) targetTable] >= UInt16.MaxValue ? ColumnSerializationSupport_Constant32.Instance : ColumnSerializationSupport_Constant16.Instance,
            rawSetter,
            ( args, value ) =>
            {
               if ( value != 0 )
               {
                  setter( args.Row, new TableIndex( targetTable, value - 1 ) );
               }
            },
            row => getter( row ).Index + 1
            );
      }

      /// <summary>
      /// Returns a <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/> for column value, which is table index into a set of pre-defined tables.
      /// </summary>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <typeparam name="TRow">The type of the normal row (part of the tables in <see cref="CILMetaData"/>).</typeparam>
      /// <param name="targetTables">The pre-defined tables, as nullable integer values of <see cref="Tables"/> enumeration.</param>
      /// <param name="rawSetter">The <see cref="RawRowColumnSetterDelegate{TRawRow}"/>.</param>
      /// <param name="setter">The <see cref="RowColumnSetterDelegate{TRow, TValue}"/>.</param>
      /// <param name="getter">The <see cref="RowColumnGetterDelegate{TRow, TValue}"/>.</param>
      /// <returns>A new instance of <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/>.</returns>
      public static DefaultColumnSerializationInfo<TRow, TRawRow> CodedReference<TRow, TRawRow>(
         ArrayQuery<Int32?> targetTables,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, TableIndex?> setter,
         RowColumnGetterDelegate<TRow, TableIndex?> getter
         )
         where TRawRow : class
         where TRow : class
      {
         var decoder = new CodedTableIndexDecoder( targetTables );

         return new DefaultColumnSerializationInfo<TRow, TRawRow>(
            args => CodedTableIndexDecoder.GetCodedTableSize( args.TableSizes, targetTables ) < sizeof( Int32 ) ? ColumnSerializationSupport_Constant16.Instance : ColumnSerializationSupport_Constant32.Instance,
            rawSetter,
            ( args, value ) => setter( args.Row, decoder.DecodeTableIndex( value ) ),
            row => decoder.EncodeTableIndex( getter( row ) )
            );
      }

      /// <summary>
      /// Returns a <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/> for column value, which is serialized as reference to <c>"#Blob"</c> meta data stream.
      /// </summary>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <typeparam name="TRow">The type of the normal row (part of the tables in <see cref="CILMetaData"/>).</typeparam>
      /// <param name="rawSetter">The <see cref="RawRowColumnSetterDelegate{TRawRow}"/>.</param>
      /// <param name="setter">The callback to set the value.</param>
      /// <param name="blobCreator">The callback to create byte array from the value.</param>
      /// <returns>A new instance of <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/>.</returns>
      public static DefaultColumnSerializationInfo<TRow, TRawRow> BLOB<TRow, TRawRow>(
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         Action<ColumnValueArgs<TRow, RowReadingArguments>, Int32, ReaderBLOBStreamHandler> setter, // TODO delegat-ize these
         Func<ColumnValueArgs<TRow, RowHeapFillingArguments>, Byte[]> blobCreator
         )
         where TRawRow : class
         where TRow : class
      {
         return HeapIndex<TRow, TRawRow>(
            MetaDataConstants.BLOB_STREAM_NAME,
            rawSetter,
            ( args, value ) => setter( args, value, args.RowArgs.MDStreamContainer.BLOBs ),
            ( args ) => args.RowArgs.MDStreamContainer.BLOBs.RegisterBLOB( blobCreator( args ) )
            );
      }

      /// <summary>
      /// Returns a <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/> for column value, which is serialized as reference to <c>"#GUID"</c> meta data stream.
      /// </summary>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <typeparam name="TRow">The type of the normal row (part of the tables in <see cref="CILMetaData"/>).</typeparam>
      /// <param name="rawSetter">The <see cref="RawRowColumnSetterDelegate{TRawRow}"/>.</param>
      /// <param name="setter">The <see cref="RowColumnSetterDelegate{TRow, TValue}"/>.</param>
      /// <param name="getter">The <see cref="RowColumnGetterDelegate{TRow, TValue}"/>.</param>
      /// <returns>A new instance of <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/>.</returns>
      public static DefaultColumnSerializationInfo<TRow, TRawRow> GUID<TRow, TRawRow>(
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, Guid?> setter,
         RowColumnGetterDelegate<TRow, Guid?> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return HeapIndex<TRow, TRawRow>(
            MetaDataConstants.GUID_STREAM_NAME,
            rawSetter,
            ( args, value ) => setter( args.Row, args.RowArgs.MDStreamContainer.GUIDs.GetGUID( value ) ),
            args => args.RowArgs.MDStreamContainer.GUIDs.RegisterGUID( getter( args.Row ) )
            );
      }

      /// <summary>
      /// Returns a <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/> for column value, which is serialized as reference to <c>"#Strings"</c> meta data stream.
      /// </summary>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <typeparam name="TRow">The type of the normal row (part of the tables in <see cref="CILMetaData"/>).</typeparam>
      /// <param name="rawSetter">The <see cref="RawRowColumnSetterDelegate{TRawRow}"/>.</param>
      /// <param name="setter">The <see cref="RowColumnSetterDelegate{TRow, TValue}"/>.</param>
      /// <param name="getter">The <see cref="RowColumnGetterDelegate{TRow, TValue}"/>.</param>
      /// <returns>A new instance of <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/>.</returns>
      public static DefaultColumnSerializationInfo<TRow, TRawRow> SystemString<TRow, TRawRow>(
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnSetterDelegate<TRow, String> setter,
         RowColumnGetterDelegate<TRow, String> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return HeapIndex<TRow, TRawRow>(
            MetaDataConstants.SYS_STRING_STREAM_NAME,
            rawSetter,
            ( args, value ) => setter( args.Row, args.RowArgs.MDStreamContainer.SystemStrings.GetString( value ) ),
            args => args.RowArgs.MDStreamContainer.SystemStrings.RegisterString( getter( args.Row ) )
            );
      }

      /// <summary>
      /// Returns a <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/> for column value, which is serialized as reference to meta data stream with given name.
      /// </summary>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <typeparam name="TRow">The type of the normal row (part of the tables in <see cref="CILMetaData"/>).</typeparam>
      /// <param name="heapName">The name of the meta data stream.</param>
      /// <param name="rawSetter">The <see cref="RawRowColumnSetterDelegate{TRawRow}"/>.</param>
      /// <param name="setter">The <see cref="RowColumnSetterDelegate{TRow, TValue}"/>.</param>
      /// <param name="heapValueExtractor">The <see cref="RowHeapColumnGetterDelegate{TRow}"/>.</param>
      /// <returns>A new instance of <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/>.</returns>
      public static DefaultColumnSerializationInfo<TRow, TRawRow> HeapIndex<TRow, TRawRow>(
         String heapName,
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnNormalSetterDelegate<TRow> setter,
         RowHeapColumnGetterDelegate<TRow> heapValueExtractor
         )
         where TRawRow : class
         where TRow : class
      {

         return new DefaultColumnSerializationInfo<TRow, TRawRow>(
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

      /// <summary>
      /// Returns a <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/> for column value, which is serialized as reference to data somewhere in image.
      /// </summary>
      /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
      /// <typeparam name="TRow">The type of the normal row (part of the tables in <see cref="CILMetaData"/>).</typeparam>
      /// <param name="rawSetter">The <see cref="RawRowColumnSetterDelegate{TRawRow}"/>.</param>
      /// <param name="dataReferenceSetter">The <see cref="RowColumnDataReferenceSetterDelegate{TRow}"/>.</param>
      /// <param name="dataReferenceColumnSectionPartCreator">The <see cref="DataReferenceColumnSectionPartCreationDelegate{TRow}"/>.</param>
      /// <returns>A new instance of <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/>.</returns>
      public static DefaultColumnSerializationInfo<TRow, TRawRow> DataReferenceColumn<TRow, TRawRow>(
         RawRowColumnSetterDelegate<TRawRow> rawSetter,
         RowColumnDataReferenceSetterDelegate<TRow> dataReferenceSetter,
         DataReferenceColumnSectionPartCreationDelegate<TRow> dataReferenceColumnSectionPartCreator
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRow, TRawRow>(
            args => ColumnSerializationSupport_Constant32.Instance,
            rawSetter,
            dataReferenceSetter,
            dataReferenceColumnSectionPartCreator
         );
      }
   }

   /// <summary>
   /// This class is used by <see cref="DefaultColumnSerializationInfoFactory.CodedReference"/> method to aid with (de)serializing coded table indices.
   /// </summary>
   public sealed class CodedTableIndexDecoder
   {

      /// <summary>
      /// This method returns the count of bits reserved to encode target table, given the amount of target tables.
      /// </summary>
      /// <param name="referencedTablesLength">The amount of target tables.</param>
      /// <returns>The count of bits reserved to encode target table of coded table index.</returns>
      /// <remarks>
      /// <para>
      /// The returned value will be the optimal amount of bits to store given target tables (calculated by <see cref="BinaryUtils.Log2(uint)"/>).
      /// </para>
      /// <para>
      /// This method will return <c>-1</c> if <paramref name="referencedTablesLength"/> is <c>0</c>.
      /// </para>
      /// </remarks>
      public static Int32 GetTagBitSize( Int32 referencedTablesLength )
      {
         return BinaryUtils.Log2( ( (UInt32) referencedTablesLength ) - 1 ) + 1;
      }

      /// <summary>
      /// This method returns the size of the coded table index, given the current table sizes, and the possible tables for the coded table index.
      /// </summary>
      /// <param name="tableSizes">The table size array.</param>
      /// <param name="referencedTables">The possible tables for the coded table index.</param>
      /// <returns>The byte count for serialized coded table index.</returns>
      /// <remarks>
      /// The returned byte count will be <c>2</c>, if the coded table index will fit into that.
      /// Otherwise, the returned byte count will be <c>4</c>.
      /// </remarks>
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
         return max < ( UInt16.MaxValue >> GetTagBitSize( referencedTables.Count ) ) ?
            2 :
            4;
      }

      private readonly IDictionary<Int32, Int32> _tablesDictionary;
      private readonly Int32 _targetTablesBitMask;

      /// <summary>
      /// Creates a new instance of <see cref="CodedTableIndexDecoder"/> for given target tables.
      /// </summary>
      /// <param name="possibleTables">The target tables, as an array of nullable integer values of <see cref="Tables"/> enumeration.</param>
      /// <param name="targetTablesBitCount">The optional bit count for target tables. If not supplied, then the result of <see cref="GetTagBitSize"/> will be used.</param>
      public CodedTableIndexDecoder(
         ArrayQuery<Int32?> possibleTables,
         Int32? targetTablesBitCount = null
         )
      {
         ArgumentValidator.ValidateNotNull( "Possible tables", possibleTables );

         this.TargetTables = possibleTables;
         this._tablesDictionary = possibleTables
            .Select( ( t, idx ) => Tuple.Create( t, idx ) )
            .Where( t => t.Item1.HasValue )
            .ToDictionary_Preserve( t => t.Item1.Value, t => t.Item2 );
         this.TargetTablesBitCount = targetTablesBitCount ?? GetTagBitSize( possibleTables.Count );
         this._targetTablesBitMask = ( 1 << this.TargetTablesBitCount ) - 1;
      }

      /// <summary>
      /// Given serialized coded table index, as integer, decodes it into an instance of <see cref="TableIndex"/> or <c>null</c>.
      /// </summary>
      /// <param name="codedIndex">The serialized coded table index.</param>
      /// <returns>An instance of <see cref="TableIndex"/>, or <c>null</c> if <paramref name="codedIndex"/> was invalid or does not represent an index to anything.</returns>
      public TableIndex? DecodeTableIndex( Int32 codedIndex )
      {
         TableIndex? retVal;
         if ( codedIndex != 0 )
         {
            var tableIndex = this._targetTablesBitMask & codedIndex;
            if ( tableIndex < this.TargetTables.Count )
            {
               var tableNullable = this.TargetTables[tableIndex];
               if ( tableNullable.HasValue )
               {
                  var rowIdx = ( ( (UInt32) codedIndex ) >> this.TargetTablesBitCount );
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
         }
         else
         {
            retVal = null;
         }

         return retVal;
      }

      /// <summary>
      /// Given deserialized nullable instance of <see cref="TableIndex"/>, encodes it into serialized integer.
      /// </summary>
      /// <param name="tableIndex">The nullable <see cref="TableIndex"/>.</param>
      /// <returns>Serialized coded table index. Will be <c>0</c> if <paramref name="tableIndex"/> does not have value, or the target table is invalid.</returns>
      public Int32 EncodeTableIndex( TableIndex? tableIndex )
      {
         Int32 retVal;
         if ( tableIndex.HasValue )
         {
            var tIdxValue = tableIndex.Value;
            Int32 tableArrayIndex;
            retVal = this._tablesDictionary.TryGetValue( (Int32) tIdxValue.Table, out tableArrayIndex ) ?
               ( ( ( tIdxValue.Index + 1 ) << this.TargetTablesBitCount ) | tableArrayIndex ) :
               0;
         }
         else
         {
            retVal = 0;
         }

         return retVal;
      }

      /// <summary>
      /// Gets the target tables of this <see cref="CodedTableIndexDecoder"/>, as an array of nullable integer values of <see cref="Tables"/> enumeration.
      /// </summary>
      /// <value>The target tables of this <see cref="CodedTableIndexDecoder"/>, as an array of nullable integer values of <see cref="Tables"/> enumeration.</value>
      public ArrayQuery<Int32?> TargetTables { get; }

      /// <summary>
      /// Gets the amount of bits used to encode target table.
      /// </summary>
      /// <value>The amount of bits used to encode target table.</value>
      public Int32 TargetTablesBitCount { get; }
   }

   /// <summary>
   /// This struct contains all required information for setting or getting a single column value of row.
   /// </summary>
   /// <typeparam name="TRow">The type of the row.</typeparam>
   /// <typeparam name="TRowArgs">The type of arguments related to functionality this <see cref="ColumnValueArgs{TRow, TRowArgs}"/> is used for.</typeparam>
   public struct ColumnValueArgs<TRow, TRowArgs>
      where TRow : class
      where TRowArgs : class
   {

      /// <summary>
      /// Creates a new instance of <see cref="ColumnValueArgs{TRow, TRowArgs}"/> with given arguments.
      /// </summary>
      /// <param name="rowIndex">The index of the row being processed.</param>
      /// <param name="row">The row instance.</param>
      /// <param name="args">The additional arguments.</param>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="row"/> or <paramref name="args"/> is <c>null</c>.</exception>
      public ColumnValueArgs(
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

      /// <summary>
      /// Gets the index of the row in this <see cref="MetaDataTable"/>.
      /// </summary>
      /// <value>The index of the row in this <see cref="MetaDataTable"/>.</value>
      public Int32 RowIndex { get; }

      /// <summary>
      /// Gets the row instance.
      /// </summary>
      /// <value>The row instance.</value>
      public TRow Row { get; }

      /// <summary>
      /// Gets the additional data.
      /// </summary>
      /// <value>The additional data.</value>
      public TRowArgs RowArgs { get; }
   }

   /// <summary>
   /// This class implements <see cref="TableSerializationLogicalFunctionality"/> by using the callbacks of given <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/>s.
   /// </summary>
   /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
   /// <typeparam name="TRow">The type of the normal row (part of the tables in <see cref="CILMetaData"/>).</typeparam>
   /// <remarks>
   /// This class is not meant to be instanced directly, instead the <see cref="MetaDataTableInformation{TRow, TRawRow}.CreateTableSerializationInfo"/> will create object of this type.
   /// </remarks>
   /// <seealso cref="MetaDataTableInformation{TRow, TRawRow}"/>
   /// <seealso cref="MetaDataTableInformation{TRow, TRawRow}.CreateTableSerializationInfo"/>
   public class TableSerializationLogicalFunctionalityImpl<TRow, TRawRow> : TableSerializationLogicalFunctionality
      where TRawRow : class
      where TRow : class
   {

      private readonly DefaultColumnSerializationInfo<TRow, TRawRow>[] _columns;
      private readonly Func<TRow> _rowFactory;
      private readonly Func<TRawRow> _rawRowFactory;
      private readonly TableSerializationLogicalFunctionalityCreationArgs _creationArgs;

      /// <summary>
      /// Creates a new instance of <see cref="TableSerializationLogicalFunctionalityImpl{TRow, TRawRow}"/> with given parameters.
      /// </summary>
      /// <param name="table">The table ID, as <see cref="Tables"/> enumeration.</param>
      /// <param name="isSorted">Whether this table is marked as sorted in <see cref="MetaDataTableStreamHeader.SortedTablesBitVector"/>.</param>
      /// <param name="columns">The enumerable of <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/>s containing callbacks to use by this <see cref="TableSerializationLogicalFunctionalityImpl{TRow, TRawRow}"/>.</param>
      /// <param name="rowFactory">The callback to create a blank normal row.</param>
      /// <param name="rawRowFactory">The callback to create a blank raw row.</param>
      /// <param name="args">The <see cref="TableSerializationLogicalFunctionalityCreationArgs"/> containing e.g. error handler.</param>
      /// <exception cref="ArgumentNullException">If any of the <paramref name="columns"/>, <paramref name="rowFactory"/>, or <paramref name="rawRowFactory"/> is <c>null</c>. Additionally, this is thrown if any of the elements of <paramref name="columns"/> is <c>null</c>.</exception>
      public TableSerializationLogicalFunctionalityImpl(
         Tables table,
         Boolean isSorted,
         IEnumerable<DefaultColumnSerializationInfo<TRow, TRawRow>> columns,
         Func<TRow> rowFactory,
         Func<TRawRow> rawRowFactory,
         TableSerializationLogicalFunctionalityCreationArgs args
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

      /// <summary>
      /// Gets the table ID of the represented <see cref="MetaDataTable"/>, as <see cref="Tables"/> enumeraiton.
      /// </summary>
      /// <value>The table ID of the represented <see cref="MetaDataTable"/>, as <see cref="Tables"/> enumeraiton.</value>
      public Tables Table { get; }

      /// <summary>
      /// Gets the value indicating whether this table is considered to be sorted by meta data.
      /// </summary>
      /// <value>The value indicating whether this table is considered to be sorted by meta data.</value>
      /// <remarks>
      /// This value will affect the <see cref="MetaDataTableStreamHeader.SortedTablesBitVector"/> value.
      /// </remarks>
      public Boolean IsSorted { get; }

      /// <summary>
      /// Gets the amount of columns in the represented meta data table, which are data reference columns.
      /// </summary>
      /// <value>The amount of columns in the represented meta data table, which are data reference columns.</value>
      /// <remarks>
      /// For example, the <see cref="MethodDefinition.IL"/> is data reference column.
      /// </remarks>
      public Int32 DataReferenceColumnCount
      {
         get
         {
            return this._columns.Count( c => c.DataReferenceSetter != null );
         }
      }

      /// <summary>
      /// Gets the amount of columns in the represented meta data table, which are references to other meta data streams.
      /// </summary>
      /// <value>The amount of columns in the represented meta data table, which are references to other meta data streams.</value>
      /// <remarks>
      /// For example, the <see cref="TypeDefinition.Name"/> is a reference to other meta data stream (namely, <c>#Strings</c> stream).
      /// </remarks>
      public Int32 MetaDataStreamReferenceColumnCount
      {
         get
         {
            return this._columns.Count( c => c.HeapValueExtractor != null );
         }
      }

      /// <summary>
      /// This method implements <see cref="TableSerializationLogicalFunctionality.PopulateDataReferences"/> by utilizing the <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}.DataReferenceSetter"/> callback.
      /// </summary>
      /// <param name="args">The <see cref="DataReferencesProcessingArgs"/>.</param>
      public void PopulateDataReferences(
         DataReferencesProcessingArgs args
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
               .Select( ( c, cIdx ) => Tuple.Create( c.DataReferenceSetter, cIdx ) )
               .Where( p => p.Item1 != null )
               .ToArray();
            if ( cols.Length > 0 )
            {
               var list = table.TableContents;
               var dataRefs = args.ImageInformation.CLIInformation.DataReferences.DataReferences[tblEnum];
               var dataRefColCount = dataRefs.Count;
               for ( var i = 0; i < list.Count; ++i )
               {
                  var cArgs = new ColumnValueArgs<TRow, DataReferencesProcessingArgs>( i, list[i], args );
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

      /// <summary>
      /// This method implements the <see cref="TableSerializationLogicalFunctionality.CreateDataReferenceSectionParts"/> by utilizing the <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}.DataReferenceColumnSectionPartCreator"/> callback.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/>.</param>
      /// <param name="mdStreamContainer">The <see cref="WriterMetaDataStreamContainer"/>.</param>
      /// <returns>An enumerable of <see cref="SectionPartWithDataReferenceTargets"/>.</returns>
      public IEnumerable<SectionPartWithDataReferenceTargets> CreateDataReferenceSectionParts(
         CILMetaData md,
         WriterMetaDataStreamContainer mdStreamContainer
      )
      {
         foreach ( var col in this._columns )
         {
            var creator = col.DataReferenceColumnSectionPartCreator;
            if ( creator != null )
            {
               yield return creator( md, mdStreamContainer );
            }
         }
      }

      /// <summary>
      /// This method implements the <see cref="TableSerializationLogicalFunctionality.ExtractMetaDataStreamReferences"/> by utilizing <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}.HeapValueExtractor"/> callback.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/>.</param>
      /// <param name="storage">The <see cref="ColumnValueStorage{TValue}"/> to store meta data references.</param>
      /// <param name="mdStreamContainer">The <see cref="WriterMetaDataStreamContainer"/>.</param>
      /// <param name="array">The auxiliary array.</param>
      /// <param name="publicKey">The public key of the assembly being emitted, or <c>null</c>.</param>
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
                  var cArgs = new ColumnValueArgs<TRow, RowHeapFillingArguments>( i, list[i], rArgs );
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

      /// <summary>
      /// This method implements the <see cref="TableSerializationLogicalFunctionality.GetAllRawValues"/> by utilizing the <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}.ConstantExtractor"/>, or extracting value from given <see cref="ColumnValueStorage{TValue}"/> of meta data stream references or from given data references array.
      /// </summary>
      /// <param name="table">The <see cref="MetaDataTable"/>.</param>
      /// <param name="dataReferences">The data references array.</param>
      /// <param name="metaDataStreamRefs">The meta data stream references storage.</param>
      /// <returns>An enumerable of all columns of all rows in given table, as integers.</returns>
      public IEnumerable<Int32> GetAllRawValues(
         MetaDataTable table,
         ArrayQuery<ArrayQuery<Int64>> dataReferences,
         ColumnValueStorage<Int32> metaDataStreamRefs
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
                     yield return metaDataStreamRefs.GetRawValue( this.Table, rowIdx, heapIdx );
                     ++heapIdx;
                  }
                  else if ( col.DataReferenceColumnSectionPartCreator != null )
                  {
                     yield return (Int32) dataReferences[rawIdx][rowIdx];
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

      /// <summary>
      /// This method implements <see cref="TableSerializationLogicalFunctionality.CreateBinaryFunctionality"/> by returning instance of <see cref="TableSerializationBinaryFunctionalityImpl{TRow, TRawRow}"/>.
      /// </summary>
      /// <param name="binaryCreationArg">The <see cref="TableSerializationBinaryFunctionalityCreationArgs"/>.</param>
      /// <returns>A new instance of <see cref="TableSerializationBinaryFunctionalityImpl{TRow, TRawRow}"/>.</returns>
      public TableSerializationBinaryFunctionality CreateBinaryFunctionality( TableSerializationBinaryFunctionalityCreationArgs binaryCreationArg )
      {
         return new TableSerializationBinaryFunctionalityImpl<TRow, TRawRow>(
            this,
            this._columns,
            binaryCreationArg,
            this._rowFactory,
            this._rawRowFactory,
            this._creationArgs.ErrorHandler
            );
      }
   }

   /// <summary>
   /// This class implements the <see cref="TableSerializationBinaryFunctionality"/> by using the callbacks of given <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/>s.
   /// </summary>
   /// <typeparam name="TRow">The type of the normal row (part of the tables in <see cref="CILMetaData"/>).</typeparam>
   /// <typeparam name="TRawRow">The type of the raw row.</typeparam>
   /// <remarks>
   /// This class is not meant to be instanced directly, instead use the <see cref="TableSerializationLogicalFunctionalityImpl{TRow, TRawRow}.CreateBinaryFunctionality"/> method.
   /// The <see cref="TableSerializationLogicalFunctionalityImpl{TRow, TRawRow}"/> themselves are created by <see cref="MetaDataTableInformation{TRow, TRawRow}.CreateTableSerializationInfo"/>.
   /// </remarks>
   /// <seealso cref="TableSerializationLogicalFunctionalityImpl{TRow, TRawRow}"/>
   /// <seealso cref="TableSerializationLogicalFunctionalityImpl{TRow, TRawRow}.CreateBinaryFunctionality"/>
   /// <seealso cref="MetaDataTableInformation{TRow, TRawRow}"/>
   /// <seealso cref="MetaDataTableInformation{TRow, TRawRow}.CreateTableSerializationInfo"/>
   public class TableSerializationBinaryFunctionalityImpl<TRow, TRawRow> : TableSerializationBinaryFunctionality
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
            DefaultColumnSerializationInfo<TRow, TRawRow> serializationInfo,
            TableSerializationBinaryFunctionalityCreationArgs args
            )
         {

            ArgumentValidator.ValidateNotNull( "Serialization info", serializationInfo );
            ArgumentValidator.ValidateNotNull( "Functionality creation args", args );

            this.Functionality = serializationInfo.SerializationSupportCreator( args );
            this._rawSetter = serializationInfo.RawSetter;
         }

         public ColumnSerializationBinaryFunctionality Functionality { get; }

         public abstract void SetNormalRowValue( RowReadingArguments rowArgs, Byte[] array, ref Int32 idx, TRow row, Int32 rowIndex );

         public void SetRawRowValue( TRawRow row, Byte[] array, ref Int32 idx )
         {
            this._rawSetter( row, this.Functionality.ReadRawValue( array, idx ) );
            idx += this.Functionality.ColumnByteCount;
         }
      }

      private sealed class ColumnSerializationInstance_RawValue : ColumnSerializationInstance
      {
         internal ColumnSerializationInstance_RawValue(
            DefaultColumnSerializationInfo<TRow, TRawRow> serializationInfo,
            TableSerializationBinaryFunctionalityCreationArgs args
            )
            : base( serializationInfo, args )
         {

         }

         public override void SetNormalRowValue( RowReadingArguments rowArgs, Byte[] array, ref Int32 idx, TRow row, Int32 rowIndex )
         {
            rowArgs.DataReferencesStorage.AddRawValue( this.Functionality.ReadRawValue( array, idx ) );
            idx += this.Functionality.ColumnByteCount;
         }
      }

      private sealed class ColumnSerializationInstance_NormalValue : ColumnSerializationInstance
      {
         private readonly RowColumnNormalSetterDelegate<TRow> _setter;
         private readonly Tables _table;
         private readonly Int32 _columnIndex;
         private readonly EventHandler<SerializationErrorEventArgs> _errorHandler;

         internal ColumnSerializationInstance_NormalValue(
            DefaultColumnSerializationInfo<TRow, TRawRow> serializationInfo,
            TableSerializationBinaryFunctionalityCreationArgs args,
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

         public override void SetNormalRowValue( RowReadingArguments rowArgs, Byte[] array, ref Int32 idx, TRow row, Int32 rowIndex )
         {
            try
            {
               this._setter( new ColumnValueArgs<TRow, RowReadingArguments>( rowIndex, row, rowArgs ), this.Functionality.ReadRawValue( array, idx ) );
            }
            catch ( Exception exc )
            {
               if ( this._errorHandler.ProcessSerializationError( null, exc, this._table, rowIndex, this._columnIndex ) )
               {
                  throw;
               }
            }
            idx += this.Functionality.ColumnByteCount;
         }
      }

      /// <summary>
      /// Creates a new instance of <see cref="TableSerializationBinaryFunctionalityImpl{TRow, TRawRow}"/> with given parameters.
      /// </summary>
      /// <param name="tableSerializationInfo">The <see cref="TableSerializationLogicalFunctionality"/> which created this <see cref="TableSerializationBinaryFunctionalityImpl{TRow, TRawRow}"/>.</param>
      /// <param name="columns">The enumerable of <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}"/>.</param>
      /// <param name="args">The <see cref="TableSerializationBinaryFunctionalityCreationArgs"/>.</param>
      /// <param name="rowFactory">The callback to create blank normal rows.</param>
      /// <param name="rawRowFactory">The callback to create blank raw rows.</param>
      /// <param name="errorHandler">The error handler callback.</param>
      public TableSerializationBinaryFunctionalityImpl(
         TableSerializationLogicalFunctionality tableSerializationInfo,
         IEnumerable<DefaultColumnSerializationInfo<TRow, TRawRow>> columns,
         TableSerializationBinaryFunctionalityCreationArgs args,
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
         this.LogicalFunctionality = tableSerializationInfo;
      }

      /// <summary>
      /// Gets the <see cref="TableSerializationLogicalFunctionality"/> which created this <see cref="TableSerializationBinaryFunctionalityImpl{TRow, TRawRow}"/>.
      /// </summary>
      /// <value>The <see cref="TableSerializationLogicalFunctionality"/> which created this <see cref="TableSerializationBinaryFunctionalityImpl{TRow, TRawRow}"/>.</value>
      /// <seealso cref="TableSerializationLogicalFunctionality"/>
      /// <seealso cref="TableSerializationLogicalFunctionalityImpl{TRow, TRawRow}"/>
      public TableSerializationLogicalFunctionality LogicalFunctionality { get; }

      /// <summary>
      /// Gets the array of <see cref="ColumnSerializationBinaryFunctionality"/> responsible for serializing and deserializing column values.
      /// </summary>
      /// <value>The array of <see cref="ColumnSerializationBinaryFunctionality"/> responsible for serializing and deserializing column values.</value>
      /// <seealso cref="ColumnSerializationBinaryFunctionality"/>
      public ArrayQuery<ColumnSerializationBinaryFunctionality> ColumnSerializationSupports { get; }

      /// <summary>
      /// This method implements the <see cref="TableSerializationBinaryFunctionality.ReadRows"/> method by storing data reference value into <see cref="RowReadingArguments.DataReferencesStorage"/> for data reference columns, and utilizing the <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}.Setter"/> callback for other columns.
      /// </summary>
      /// <param name="table">The <see cref="MetaDataTable"/> to fill.</param>
      /// <param name="tableRowCount">The amount of rows to read.</param>
      /// <param name="array">The byte array where to read raw integer values.</param>
      /// <param name="index">The index in <paramref name="array"/> where to start reading.</param>
      /// <param name="args">The <see cref="RowReadingArguments"/>.</param>
      public void ReadRows(
         MetaDataTable table,
         Int32 tableRowCount,
         Byte[] array,
         Int32 index,
         RowReadingArguments args
         )
      {
         if ( tableRowCount > 0 )
         {
            var list = ( (MetaDataTable<TRow>) table ).TableContents;
            var cArray = this._columnArray;
            var cArrayMax = this._columnArray.Length;

            for ( var i = 0; i < tableRowCount; ++i )
            {
               var row = this._rowFactory();
               for ( var j = 0; j < cArrayMax; ++j )
               {
                  cArray[j].SetNormalRowValue( args, array, ref index, row, i );
               }

               list.Add( row );
            }
         }
      }

      /// <summary>
      /// This method implements the <see cref="TableSerializationBinaryFunctionality.ReadRawRow"/> method by utilizing the <see cref="DefaultColumnSerializationInfo{TRow, TRawRow}.RawSetter"/> callback.
      /// </summary>
      /// <param name="array">The array where to read row from.</param>
      /// <param name="idx">The index in <paramref name="array"/> where to start reading.</param>
      /// <returns>An instance of raw row.</returns>
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

   /// <summary>
   /// This class implements <see cref="ColumnSerializationBinaryFunctionality"/> for integers serialized as <c>1</c>-byte values.
   /// </summary>
   /// <remarks>
   /// This is stateless singleton class, instance of which are accessible through <see cref="Instance"/> static property.
   /// </remarks>
   public sealed class ColumnSerializationSupport_Constant8 : ColumnSerializationBinaryFunctionality
   {

      /// <summary>
      /// Gets the instance of this <see cref="ColumnSerializationSupport_Constant8"/>.
      /// </summary>
      /// <value>The instance of this <see cref="ColumnSerializationSupport_Constant8"/>.</value>
      public static ColumnSerializationBinaryFunctionality Instance { get; }

      static ColumnSerializationSupport_Constant8()
      {
         Instance = new ColumnSerializationSupport_Constant8();
      }

      private ColumnSerializationSupport_Constant8()
      {

      }

      /// <summary>
      /// Implements the <see cref="ColumnSerializationBinaryFunctionality.ColumnByteCount"/>, returning <c>1</c>.
      /// </summary>
      /// <value>The value <c>1</c>.</value>
      public Int32 ColumnByteCount
      {
         get
         {
            return sizeof( Byte );
         }
      }

      /// <summary>
      /// Implements the <see cref="ColumnSerializationBinaryFunctionality.ReadRawValue"/>, reading one byte from the given array.
      /// </summary>
      /// <param name="array">The byte array to read value from.</param>
      /// <param name="idx">The index in <paramref name="array"/> where to start reading.</param>
      /// <returns>The integer value of deserialized <see cref="Byte"/>.</returns>
      public Int32 ReadRawValue( Byte[] array, Int32 idx )
      {
         return array[idx];
      }

      /// <summary>
      /// Implements the <see cref="ColumnSerializationBinaryFunctionality.WriteValue"/>, writing one byte into given array.
      /// </summary>
      /// <param name="bytes">The array to write value to.</param>
      /// <param name="idx">The index in <paramref name="array"/> where to start writing.</param>
      /// <param name="value">The value to write.</param>
      public void WriteValue( Byte[] bytes, Int32 idx, Int32 value )
      {
         bytes.WriteByteToBytes( ref idx, (Byte) value );
      }
   }

   /// <summary>
   /// This class implements <see cref="ColumnSerializationBinaryFunctionality"/> for integers serialized as <c>2</c>-byte values.
   /// </summary>
   /// <remarks>
   /// This is stateless singleton class, instance of which are accessible through <see cref="Instance"/> static property.
   /// </remarks>
   public sealed class ColumnSerializationSupport_Constant16 : ColumnSerializationBinaryFunctionality
   {
      /// <summary>
      /// Gets the instance of this <see cref="ColumnSerializationSupport_Constant16"/>.
      /// </summary>
      /// <value>The instance of this <see cref="ColumnSerializationSupport_Constant16"/>.</value>
      public static ColumnSerializationBinaryFunctionality Instance { get; }

      static ColumnSerializationSupport_Constant16()
      {
         Instance = new ColumnSerializationSupport_Constant16();
      }

      private ColumnSerializationSupport_Constant16()
      {

      }

      /// <summary>
      /// Implements the <see cref="ColumnSerializationBinaryFunctionality.ColumnByteCount"/>, returning <c>2</c>.
      /// </summary>
      /// <value>The value <c>2</c>.</value>
      public Int32 ColumnByteCount
      {
         get
         {
            return sizeof( UInt16 );
         }
      }

      /// <summary>
      /// Implements the <see cref="ColumnSerializationBinaryFunctionality.ReadRawValue"/>, reading two bytes from the given array and interpreting them as little-endian <see cref="UInt16"/>.
      /// </summary>
      /// <param name="array">The byte array to read value from.</param>
      /// <param name="idx">The index in <paramref name="array"/> where to start reading.</param>
      /// <returns>The integer value of deserialized <see cref="UInt16"/>.</returns>
      public Int32 ReadRawValue( Byte[] array, Int32 idx )
      {
         return array.ReadUInt16LEFromBytesNoRef( idx );
      }

      /// <summary>
      /// Implements the <see cref="ColumnSerializationBinaryFunctionality.WriteValue"/>, writing two bytes into given array, as little-endian <see cref="UInt16"/>.
      /// </summary>
      /// <param name="bytes">The array to write value to.</param>
      /// <param name="idx">The index in <paramref name="array"/> where to start writing.</param>
      /// <param name="value">The value to write.</param>
      public void WriteValue( Byte[] bytes, Int32 idx, Int32 value )
      {
         bytes.WriteUInt16LEToBytes( ref idx, (UInt16) value );
      }
   }

   /// <summary>
   /// This class implements <see cref="ColumnSerializationBinaryFunctionality"/> for integers serialized as <c>4</c>-byte values.
   /// </summary>
   /// <remarks>
   /// This is stateless singleton class, instance of which are accessible through <see cref="Instance"/> static property.
   /// </remarks>
   public sealed class ColumnSerializationSupport_Constant32 : ColumnSerializationBinaryFunctionality
   {
      /// <summary>
      /// Gets the instance of this <see cref="ColumnSerializationSupport_Constant32"/>.
      /// </summary>
      /// <value>The instance of this <see cref="ColumnSerializationSupport_Constant32"/>.</value>
      public static ColumnSerializationBinaryFunctionality Instance { get; }

      static ColumnSerializationSupport_Constant32()
      {
         Instance = new ColumnSerializationSupport_Constant32();
      }

      private ColumnSerializationSupport_Constant32()
      {

      }

      /// <summary>
      /// Implements the <see cref="ColumnSerializationBinaryFunctionality.ColumnByteCount"/>, returning <c>4</c>.
      /// </summary>
      /// <value>The value <c>4</c>.</value>
      public Int32 ColumnByteCount
      {
         get
         {
            return sizeof( Int32 );
         }
      }

      /// <summary>
      /// Implements the <see cref="ColumnSerializationBinaryFunctionality.ReadRawValue"/>, reading four bytes from the given array and interpreting them as little-endian <see cref="Int32"/>.
      /// </summary>
      /// <param name="array">The byte array to read value from.</param>
      /// <param name="idx">The index in <paramref name="array"/> where to start reading.</param>
      /// <returns>The integer value of deserialized <see cref="Int32"/>.</returns>
      public Int32 ReadRawValue( Byte[] array, Int32 idx )
      {
         return array.ReadInt32LEFromBytesNoRef( idx );
      }

      /// <summary>
      /// Implements the <see cref="ColumnSerializationBinaryFunctionality.WriteValue"/>, writing four bytes into given array, as little-endian <see cref="Int32"/>.
      /// </summary>
      /// <param name="bytes">The array to write value to.</param>
      /// <param name="idx">The index in <paramref name="array"/> where to start writing.</param>
      /// <param name="value">The value to write.</param>
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