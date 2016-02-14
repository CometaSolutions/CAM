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
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;
using CAMPhysical::CILAssemblyManipulator.Physical.Meta;

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.IO;
using CILAssemblyManipulator.Physical.Meta;
using CollectionsWithRoles.API;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TabularMetaData.Meta;

namespace CILAssemblyManipulator.Physical.IO
{
   /// <summary>
   /// This interface is the 'root' interface which controls what happens when reading <see cref="CILMetaData"/> with <see cref="CILMetaDataIO.ReadModule"/> method.
   /// To customize the deserialization process, it is possible to set <see cref="ReadingArguments.ReaderFunctionalityProvider"/> property to your own instances of <see cref="ReaderFunctionalityProvider"/>.
   /// </summary>
   /// <seealso cref="ReaderFunctionality"/>
   /// <seealso cref="ReadingArguments.ReaderFunctionalityProvider"/>
   /// <seealso cref="E_CILPhysical.ReadMetaDataFromStream(ReaderFunctionalityProvider, Stream, CILMetaDataTableInformationProvider, EventHandler{SerializationErrorEventArgs}, bool, out ImageInformation)"/>
   /// <seealso cref="DefaultReaderFunctionalityProvider"/>
   public interface ReaderFunctionalityProvider
   {
      /// <summary>
      /// Creates a new <see cref="ReaderFunctionality"/> to be used to read <see cref="CILMetaData"/> from <see cref="Stream"/>.
      /// Optionally, specifies a new <see cref="Stream"/> to be used.
      /// </summary>
      /// <param name="stream">The original <see cref="Stream"/>.</param>
      /// <param name="mdTableInfoProvider">The <see cref="CILMetaDataTableInformationProvider"/> which describes what tables are supported by this deserialization process.</param>
      /// <param name="errorHandler">The error handler callback.</param>
      /// <param name="newStream">Optional new <see cref="Stream"/> to use instead of <paramref name="stream"/> in the further desererialization process.</param>
      /// <returns>The <see cref="ReaderFunctionality"/> to use for actual deserialization.</returns>
      /// <seealso cref="ReaderFunctionality"/>
      /// <seealso cref="IOArguments.ErrorHandler"/>
      /// <seealso cref="CILMetaDataTableInformationProvider"/>
      /// <seealso cref="E_CILPhysical.ReadMetaDataFromStream(ReaderFunctionality, Stream, CILMetaDataTableInformationProvider, EventHandler{SerializationErrorEventArgs}, bool, out ImageInformation, out ColumnValueStorage{int}, out RVAConverter)"/>
      ReaderFunctionality GetFunctionality(
         Stream stream,
         CILMetaDataTableInformationProvider mdTableInfoProvider,
         EventHandler<SerializationErrorEventArgs> errorHandler,
         out Stream newStream
         );
   }

   /// <summary>
   /// This interface provides core functionality to be used when deserializing <see cref="CILMetaData"/> from <see cref="Stream"/>.
   /// The instances of this interface are created via <see cref="ReaderFunctionalityProvider.GetFunctionality"/> method, and the instances of <see cref="ReaderFunctionalityProvider"/> may be customized by setting <see cref="ReadingArguments.ReaderFunctionalityProvider"/> property.
   /// </summary>
   /// <remarks>
   /// The <see cref="E_CILPhysical.ReadMetaDataFromStream(ReaderFunctionality, Stream, CILMetaDataTableInformationProvider, EventHandler{SerializationErrorEventArgs}, bool, out ImageInformation, out ColumnValueStorage{int}, out RVAConverter)"/> method will call the methods of this interface (and others) in the following order:
   /// <list type="number">
   /// <item><description><see cref="ReadImageInformation"/>,</description></item>
   /// <item><description><see cref="CreateStreamHandler"/> (once for each header in <see cref="MetaDataRoot.StreamHeaders"/>),</description></item>
   /// <item><description><see cref="ReaderTableStreamHandler.PopulateMetaDataStructure"/>, and</description></item>
   /// <item><description><see cref="HandleDataReferences"/>.</description></item>
   /// </list>
   /// </remarks>
   /// <seealso cref="ReaderFunctionalityProvider"/>
   /// <seealso cref="DefaultReaderFunctionality"/>
   /// <seealso cref="ReaderFunctionalityProvider.GetFunctionality"/>
   /// <seealso cref="E_CILPhysical.ReadMetaDataFromStream(ReaderFunctionality, Stream, CILMetaDataTableInformationProvider, EventHandler{SerializationErrorEventArgs}, bool, out ImageInformation, out ColumnValueStorage{int}, out RVAConverter)"/>
   /// <seealso cref="ReadingArguments.ReaderFunctionalityProvider"/>
   public interface ReaderFunctionality
   {

      /// <summary>
      /// This method should read required image information from the <see cref="StreamHelper"/>.
      /// </summary>
      /// <param name="stream">The <see cref="StreamHelper"/> object, encapsulating the actual <see cref="Stream"/>.</param>
      /// <param name="peInfo">This parameter should have the <see cref="PEInformation"/> object read from the stream.</param>
      /// <param name="rvaConverter">This parameter should have <see cref="RVAConverter"/> object to convert RVA values of this stream into actual offsets.</param>
      /// <param name="cliHeader">This parameter should have the <see cref="CLIHeader"/> object read from the stream.</param>
      /// <param name="mdRoot">This parameter should have the <see cref="MetaDataRoot"/> object read from the stream.</param>
      void ReadImageInformation(
         StreamHelper stream,
         out PEInformation peInfo,
         out RVAConverter rvaConverter,
         out CLIHeader cliHeader,
         out MetaDataRoot mdRoot
         );

      /// <summary>
      /// This method should create appropriate <see cref="AbstractReaderStreamHandler"/> for a given <see cref="MetaDataStreamHeader"/>.
      /// </summary>
      /// <param name="stream">The <see cref="StreamHelper"/> object, encapsulating the actual <see cref="Stream"/>.</param>
      /// <param name="mdRoot">The <see cref="MetaDataRoot"/> acquired from <see cref="ReadImageInformation"/>.</param>
      /// <param name="startPosition">The position for <paramref name="stream"/> where the this stream contents start.</param>
      /// <param name="header">The <see cref="MetaDataStreamHeader"/>, containing the size and name information for this meta data stream.</param>
      /// <returns>An instance of <see cref="AbstractReaderStreamHandler"/> representing the contents</returns>
      /// <remarks>
      /// The <see cref="AbstractReaderStreamHandler"/>s returned by this method are further handled to <see cref="ReaderMetaDataStreamContainer"/>, used in <see cref="ReaderTableStreamHandler.PopulateMetaDataStructure"/> and <see cref="HandleDataReferences"/> methods.
      /// </remarks>
      AbstractReaderStreamHandler CreateStreamHandler(
         StreamHelper stream,
         MetaDataRoot mdRoot,
         Int64 startPosition,
         MetaDataStreamHeader header
         );

      /// <summary>
      /// This method is called after <see cref="ReaderTableStreamHandler.PopulateMetaDataStructure"/>, to populate the values that need data from elsewhere than <see cref="AbstractReaderStreamHandler"/>s.
      /// </summary>
      /// <param name="stream">The <see cref="StreamHelper"/> object, encapsulating the actual <see cref="Stream"/>.</param>
      /// <param name="imageInfo">The full <see cref="ImageInformation"/>. The data references will be in <see cref="CLIInformation.DataReferences"/> property.</param>
      /// <param name="rvaConverter">The <see cref="RVAConverter"/> created by <see cref="ReadImageInformation"/> method.</param>
      /// <param name="mdStreamContainer">The <see cref="ReaderMetaDataStreamContainer"/> containing all streams created by <see cref="CreateStreamHandler"/> method.</param>
      /// <param name="md">The instance of <see cref="CILMetaData"/> that will be result of this deserialization process.</param>
      ///// <param name="dataReferences">The <see cref="ColumnValueStorage{TValue}"/> created by <see cref="ReaderTableStreamHandler.PopulateMetaDataStructure"/> method.</param>
      /// <remarks>
      /// The values that need data from elsewhere than <see cref="AbstractReaderStreamHandler"/>s, are at least:
      /// <list type="bullet">
      /// <item><description><see cref="MethodDefinition.IL"/>,</description></item>
      /// <item><description><see cref="FieldRVA.Data"/>, and</description></item>
      /// <item><description><see cref="ManifestResource.EmbeddedData"/>.</description></item>
      /// </list>
      /// </remarks>
      void HandleDataReferences(
         StreamHelper stream,
         ImageInformation imageInfo,
         RVAConverter rvaConverter,
         ReaderMetaDataStreamContainer mdStreamContainer,
         CILMetaData md
         );
   }

   /// <summary>
   /// This is base class to store values (such as integers) now, and use the values later.
   /// The raw value storage has a pre-set capacity, which can not changed, and can only be filled once.
   /// </summary>
   /// <typeparam name="TValue">The type of the values to store.</typeparam>
   public sealed class ColumnValueStorage<TValue>
   {
      private readonly ArrayQuery<Int32> _tableSizes;
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
         this._tableSizes = tableSizes;
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
         var max = start + this._tableSizes[(Int32) table] * colCount;
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

      private Int32 GetArrayIndex( Int32 table, Int32 row, Int32 col )
      {
         return this._tableStartOffsets[table] + row * this._tableColCount[table] + col;
      }

   }

   /// <summary>
   /// This interface provides methods to convert between Relative Virtual Address (RVA) values and absolute offsets for specific stream.
   /// </summary>
   public interface RVAConverter
   {
      /// <summary>
      /// This method should convert the given absolute offset to an RVA.
      /// </summary>
      /// <param name="offset">The absolute offset.</param>
      /// <returns>The RVA value for given <paramref name="offset"/>.</returns>
      /// <remarks>
      /// The types are <see cref="Int64"/> because of portability and CLS compatibility.
      /// </remarks>
      Int64 ToRVA( Int64 offset );

      /// <summary>
      /// This method should convert the given RVA to an absolute offset.
      /// </summary>
      /// <param name="rva">The RVA.</param>
      /// <returns>The absolute offset for given <paramref name="rva"/>.</returns>
      /// <remarks>
      /// The types are <see cref="Int64"/> because of portability and CLS compatibility.
      /// </remarks>
      Int64 ToOffset( Int64 rva );
   }

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

   /// <summary>
   /// This class encapsulates all <see cref="AbstractReaderStreamHandler"/>s or <see cref="AbstractWriterStreamHandler"/>s used in (de)serialization process.
   /// </summary>
   /// <typeparam name="TAbstractStream">The type of the abstract meta data stream. Should be <see cref="AbstractReaderStreamHandler"/> for deserialization process, and <see cref="AbstractWriterStreamHandler"/> for serialization process.</typeparam>
   /// <typeparam name="TBLOBStream">The type of the BLOB meta data stream. Should be <see cref="ReaderBLOBStreamHandler"/> for deserialization process, and <see cref="WriterBLOBStreamHandler"/> for serialization process.</typeparam>
   /// <typeparam name="TGUIDStream">The type of the GUID meta data stream. Should be <see cref="ReaderGUIDStreamHandler"/> for deserialization process, and <see cref="WriterGUIDStreamHandler"/> for serialization process.</typeparam>
   /// <typeparam name="TStringStream">The type of the various string meta data streams. Should be <see cref="ReaderStringStreamHandler"/> for deserialization process, and <see cref="WriterStringStreamHandler"/> for serialization process.</typeparam>
   /// <seealso cref="ReaderMetaDataStreamContainer"/>
   /// <seealso cref="WriterMetaDataStreamContainer"/>
   public class MetaDataStreamContainer<TAbstractStream, TBLOBStream, TGUIDStream, TStringStream>
      where TAbstractStream : AbstractMetaDataStreamHandler
      where TBLOBStream : TAbstractStream
      where TGUIDStream : TAbstractStream
      where TStringStream : TAbstractStream
   {
      /// <summary>
      /// Creates a new instance of <see cref="MetaDataStreamContainer{TAbstractStream, TBLOBStream, TGUIDStream, TStringStream}"/> with given streams.
      /// </summary>
      /// <param name="blobs">The handler for <c>#Blobs</c> stream.</param>
      /// <param name="guids">The handler for <c>#GUID</c> stream.</param>
      /// <param name="sysStrings">The handler for <c>#String</c> stream.</param>
      /// <param name="userStrings">The handler for <c>#US</c> stream.</param>
      /// <param name="otherStreams">Any other streams.</param>
      /// <remarks>
      /// None of the parameters are checked for <c>null</c> values.
      /// </remarks>
      public MetaDataStreamContainer(
         TBLOBStream blobs,
         TGUIDStream guids,
         TStringStream sysStrings,
         TStringStream userStrings,
         IEnumerable<TAbstractStream> otherStreams
         )
      {
         this.BLOBs = blobs;
         this.GUIDs = guids;
         this.SystemStrings = sysStrings;
         this.UserStrings = userStrings;
         this.OtherStreams = otherStreams.ToArrayProxy().CQ;
      }

      /// <summary>
      /// Gets the handler for <c>#Blobs</c> stream..
      /// </summary>
      /// <value>The handler for <c>#Blobs</c> stream..</value>
      /// <remarks>
      /// This value may be <c>null</c>, if null was specified to the constructor of this <see cref="MetaDataStreamContainer{TAbstractStream, TBLOBStream, TGUIDStream, TStringStream}"/>.
      /// </remarks>
      /// <seealso cref="ReaderBLOBStreamHandler"/>
      /// <seealso cref="WriterBLOBStreamHandler"/>
      public TBLOBStream BLOBs { get; }

      /// <summary>
      /// Gets the handler for <c>#GUID</c> stream..
      /// </summary>
      /// <value>The handler for <c>#GUID</c> stream..</value>
      /// <remarks>
      /// This value may be <c>null</c>, if null was specified to the constructor of this <see cref="MetaDataStreamContainer{TAbstractStream, TBLOBStream, TGUIDStream, TStringStream}"/>.
      /// </remarks>
      /// <seealso cref="ReaderGUIDStreamHandler"/>
      /// <seealso cref="WriterGUIDStreamHandler"/>
      public TGUIDStream GUIDs { get; }

      /// <summary>
      /// Gets the handler for <c>#String</c> stream.
      /// </summary>
      /// <value>The the handler for <c>#String</c> stream.</value>
      /// <remarks>
      /// This value may be <c>null</c>, if null was specified to the constructor of this <see cref="MetaDataStreamContainer{TAbstractStream, TBLOBStream, TGUIDStream, TStringStream}"/>.
      /// </remarks>
      /// <seealso cref="ReaderStringStreamHandler"/>
      /// <seealso cref="WriterStringStreamHandler"/>
      public TStringStream SystemStrings { get; }

      /// <summary>
      /// Gets the handler for <c>#US</c> stream..
      /// </summary>
      /// <value>The handler for <c>#US</c> stream..</value>
      /// <remarks>
      /// This value may be <c>null</c>, if null was specified to the constructor of this <see cref="MetaDataStreamContainer{TAbstractStream, TBLOBStream, TGUIDStream, TStringStream}"/>.
      /// </remarks>
      /// <seealso cref="ReaderStringStreamHandler"/>
      /// <seealso cref="WriterStringStreamHandler"/>
      public TStringStream UserStrings { get; }

      /// <summary>
      /// Gets the other streams given to this <see cref="MetaDataStreamContainer{TAbstractStream, TBLOBStream, TGUIDStream, TStringStream}"/>.
      /// </summary>
      /// <value>The other streams given to this <see cref="MetaDataStreamContainer{TAbstractStream, TBLOBStream, TGUIDStream, TStringStream}"/>.</value>
      /// <remarks>
      /// This value may be empty, but it is never <c>null</c>.
      /// </remarks>
      /// <seealso cref="AbstractReaderStreamHandler"/>
      /// <seealso cref="AbstractWriterStreamHandler"/>
      public ArrayQuery<TAbstractStream> OtherStreams { get; }
   }



   /// <summary>
   /// This class encapsulates all <see cref="AbstractReaderStreamHandler"/>s created by <see cref="ReaderFunctionality.CreateStreamHandler"/> (except <see cref="ReaderTableStreamHandler"/>) to be more easily accessable and useable.
   /// </summary>
   public class ReaderMetaDataStreamContainer : MetaDataStreamContainer<AbstractReaderStreamHandler, ReaderBLOBStreamHandler, ReaderGUIDStreamHandler, ReaderStringStreamHandler>
   {
      /// <summary>
      /// Creates a new instance of <see cref="ReaderMetaDataStreamContainer"/> with given streams.
      /// </summary>
      /// <param name="blobs">The <see cref="ReaderBLOBStreamHandler"/> for <c>#Blobs</c> stream.</param>
      /// <param name="guids">The <see cref="ReaderGUIDStreamHandler"/> for <c>#GUID</c> stream.</param>
      /// <param name="sysStrings">The <see cref="ReaderStringStreamHandler"/> for <c>#String</c> stream.</param>
      /// <param name="userStrings">The <see cref="ReaderStringStreamHandler"/> for <c>#US</c> stream.</param>
      /// <param name="otherStreams">Any other streams.</param>
      /// <remarks>
      /// None of the parameters are checked for <c>null</c> values.
      /// </remarks>
      public ReaderMetaDataStreamContainer(
         ReaderBLOBStreamHandler blobs,
         ReaderGUIDStreamHandler guids,
         ReaderStringStreamHandler sysStrings,
         ReaderStringStreamHandler userStrings,
         IEnumerable<AbstractReaderStreamHandler> otherStreams
         ) : base( blobs, guids, sysStrings, userStrings, otherStreams )
      {
      }

   }

   /// <summary>
   /// This is common interface for <see cref="AbstractReaderStreamHandler"/> and <see cref="AbstractWriterStreamHandler"/>.
   /// It contains elements common for meta data streams in both serialization and deserialization processes.
   /// </summary>
   /// <seealso cref="AbstractReaderStreamHandler"/>
   /// <seealso cref="AbstractWriterStreamHandler"/>
   public interface AbstractMetaDataStreamHandler
   {
      /// <summary>
      /// Gets the textual name of this <see cref="ReaderOrWriterStreamHandler"/>.
      /// </summary>
      /// <value>The textual name of this <see cref="ReaderOrWriterStreamHandler"/>.</value>
      String StreamName { get; }

      /// <summary>
      /// Gets the size of this <see cref="ReaderOrWriterStreamHandler"/> in bytes.
      /// </summary>
      /// <value>The size of this <see cref="ReaderOrWriterStreamHandler"/> in bytes.</value>
      Int32 StreamSize { get; }
   }

   /// <summary>
   /// This is common interface for all objects, which participate in deserialization process as handlers for meta data stream (e.g. table stream, BLOB stream, etc).
   /// </summary>
   /// <seealso cref="ReaderTableStreamHandler"/>
   /// <seealso cref="ReaderBLOBStreamHandler"/>
   /// <seealso cref="ReaderStringStreamHandler"/>
   /// <seealso cref="ReaderGUIDStreamHandler"/>
   public interface AbstractReaderStreamHandler : AbstractMetaDataStreamHandler
   {
   }

   /// <summary>
   /// This interface should be implemented by objects handling reading of table stream in meta data.
   /// The table stream is where the structure of <see cref="CILMetaData"/> is defined (present tables, their size, etc).
   /// </summary>
   /// <remarks>
   /// The <see cref="E_CILPhysical.ReadMetaDataFromStream(ReaderFunctionality, Stream, CILMetaDataTableInformationProvider, EventHandler{SerializationErrorEventArgs}, bool, out ImageInformation, out ColumnValueStorage{int}, out RVAConverter)"/> method will call the methods of this interface in the following order:
   /// <list type="number">
   /// <item><description><see cref="ReadHeader"/>,</description></item>
   /// <item><description><see cref="TableSizes"/>, and</description></item>
   /// <item><description><see cref="PopulateMetaDataStructure"/>.</description></item>
   /// </list>
   /// </remarks>
   public interface ReaderTableStreamHandler : AbstractReaderStreamHandler
   {
      /// <summary>
      /// Reads the <see cref="MetaDataTableStreamHeader"/> from this <see cref="ReaderTableStreamHandler"/>.
      /// </summary>
      /// <returns>The <see cref="MetaDataTableStreamHeader"/> with information about </returns>
      MetaDataTableStreamHeader ReadHeader();


      /// <summary>
      /// Gets the table sizes for the tables in this <see cref="ReaderTableStreamHandler"/>.
      /// </summary>
      /// <value>The table sizes for the tables in this <see cref="ReaderTableStreamHandler"/>.</value>
      /// <remarks>
      /// This property is used instead of directly creating table size array from <see cref="MetaDataTableStreamHeader"/> returned by <see cref="ReadHeader"/> in order to better customize the deserialization process in case the header for the table stream changes greatly in future releases.
      /// </remarks>
      ArrayQuery<Int32> TableSizes { get; }

      /// <summary>
      /// This method should populate those properties of the rows in metadata tables, which are either stored directly in table stream, or can be created using meta data streams (e.g. strings, guids, signatures, etc).
      /// After that is done, all the remaining values (RVAs) should be stored to <see cref="ColumnValueStorage{TValue}"/> and returned by this method.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/> to populate.</param>
      /// <param name="mdStreamContainer">The <see cref="ReaderMetaDataStreamContainer"/> containing meta data streams.</param>
      /// <returns>The raw values (RVAs) which were read from this table stream. The stored raw values will be transformed into <see cref="CLIInformation.DataReferences"/> dictionary, and used by <see cref="ReaderFunctionality.HandleDataReferences"/> method.</returns>
      IEnumerable<DataReferenceInfo> PopulateMetaDataStructure(
         CILMetaData md,
         ReaderMetaDataStreamContainer mdStreamContainer
         );

   }

   /// <summary>
   /// This is simple struct containing information about a single data reference.
   /// </summary>
   /// <remarks>
   /// One example of data reference is the <see cref="RawMethodDefinition.RVA"/> property, which gets transformed into <see cref="MethodDefinition.IL"/>.
   /// </remarks>
   public struct DataReferenceInfo
   {
      public DataReferenceInfo( Tables table, Int32 columnIndex, Int64 dataReference )
      {
         this.Table = table;
         this.ColumnIndex = columnIndex;
         this.DataReference = dataReference;
      }

      /// <summary>
      /// Gets the table id of this data reference.
      /// </summary>
      /// <value>The table id of this data reference.</value>
      /// <seealso cref="Tables"/>
      public Tables Table { get; }

      /// <summary>
      /// Gets the zero-based column index of this data reference.
      /// </summary>
      /// <value>The zero-based column index of this data reference.</value>
      public Int32 ColumnIndex { get; }

      /// <summary>
      /// Gets the data reference value.
      /// </summary>
      /// <value>The data reference value.</value>
      public Int64 DataReference { get; }
   }

   /// <summary>
   /// This interface should be implemented by objects handling reading of BLOB stream in meta data.
   /// The BLOB stream stores various signatures and also raw byte arrays.
   /// </summary>
   public interface ReaderBLOBStreamHandler : AbstractReaderStreamHandler
   {
      /// <summary>
      /// Given the stream index, reads the BLOB array that is located there.
      /// </summary>
      /// <param name="streamIndex">The zero-based stream index.</param>
      /// <returns>The BLOB as byte array.</returns>
      Byte[] GetBLOBByteArray( Int32 streamIndex );

      /// <summary>
      /// Given the stream index, reads a <see cref="AbstractSignature"/> which will not be a <see cref="TypeSignature"/>.
      /// </summary>
      /// <param name="streamIndex">The zero-based stream index.</param>
      /// <param name="sigProvider">The <see cref="SignatureProvider"/> to use when deserializing the signature.</param>
      /// <param name="methodSigIsDefinition">Whether the method signature, if any, should be a <see cref="MethodDefinitionSignature"/>.</param>
      /// <param name="handleFieldSigAsLocalsSig">Whether to handle the field signature as <see cref="LocalVariablesSignature"/>.</param>
      /// <param name="fieldSigTransformedToLocalsSig">If <paramref name="handleFieldSigAsLocalsSig"/> is <c>true</c> and the signature started with field prefix, this will be <c>true</c>. Otherwise, this will be <c>false</c>.</param>
      /// <returns>The deserialized <see cref="AbstractSignature"/>.</returns>
      /// <seealso cref="AbstractSignature"/>
      AbstractSignature ReadNonTypeSignature( Int32 streamIndex, SignatureProvider sigProvider, Boolean methodSigIsDefinition, Boolean handleFieldSigAsLocalsSig, out Boolean fieldSigTransformedToLocalsSig );

      /// <summary>
      /// Given the stream index, reads a <see cref="TypeSignature"/>.
      /// </summary>
      /// <param name="streamIndex">The zero-based stream index.</param>
      /// <param name="sigProvider">The <see cref="SignatureProvider"/> to use when deserializing the signature.</param>
      /// <returns>The deserialized <see cref="AbstractSignature"/>.</returns>
      TypeSignature ReadTypeSignature( Int32 streamIndex, SignatureProvider sigProvider );

      /// <summary>
      /// Given the stream index, reads the custom attribute signature as <see cref="AbstractCustomAttributeSignature"/>.
      /// </summary>
      /// <param name="streamIndex">The zero-based stream index.</param>
      /// <param name="sigProvider">The <see cref="SignatureProvider"/> to use when deserializing the signature.</param>
      /// <returns>The deserialized <see cref="AbstractCustomAttributeSignature"/>.</returns>
      /// <seealso cref="DefaultReaderBLOBStreamHandler.ReadCASignature"/>
      AbstractCustomAttributeSignature ReadCASignature( Int32 streamIndex, SignatureProvider sigProvider );

      /// <summary>
      /// Given the stream index, reads the security information into given list of <see cref="AbstractSecurityInformation"/>s.
      /// </summary>
      /// <param name="streamIndex">The zero-based stream index.</param>
      /// <param name="sigProvider">The <see cref="SignatureProvider"/> to use when deserializing the signature.</param>
      /// <param name="securityInfo">The list of security information attributes to populate.</param>
      /// <seealso cref="DefaultReaderBLOBStreamHandler.ReadSecurityInformation"/>
      /// <seealso cref="SecurityDefinition.PermissionSets"/>
      void ReadSecurityInformation( Int32 streamIndex, SignatureProvider sigProvider, List<AbstractSecurityInformation> securityInfo );

      /// <summary>
      /// Given the stream index, reads the marshaling information as <see cref="AbstractMarshalingInfo"/>.
      /// </summary>
      /// <param name="streamIndex">The zero-based stream index.</param>
      /// <param name="sigProvider">The <see cref="SignatureProvider"/> to use when deserializing the signature.</param>
      /// <returns>One of the concrete subclasses of <see cref="AbstractMarshalingInfo"/>.</returns>
      /// <seealso cref="E_CILPhysical.ReadMarshalingInfo"/>
      AbstractMarshalingInfo ReadMarshalingInfo( Int32 streamIndex, SignatureProvider sigProvider );

      /// <summary>
      /// Given the stream index, reads the constant value stored in the stream.
      /// </summary>
      /// <param name="streamIndex">The zero-based stream index.</param>
      /// <param name="sigProvider">The <see cref="SignatureProvider"/> to use when deserializing the signature.</param>
      /// <param name="constType">The <see cref="ConstantValueType"/> describing what kind of constant is stored in the stream.</param>
      /// <returns>The deserialized constant value.</returns>
      /// <seealso cref="ConstantDefinition"/>
      /// <seealso cref="ConstantDefinition.Value"/>
      Object ReadConstantValue( Int32 streamIndex, SignatureProvider sigProvider, ConstantValueType constType );

   }

   /// <summary>
   /// This interface should be implemented by objects handling reading of GUID stream in meta data.
   /// The GUID stream stores <see cref="Guid"/> objects used by <see cref="CILMetaData"/>.
   /// </summary>
   public interface ReaderGUIDStreamHandler : AbstractReaderStreamHandler
   {
      /// <summary>
      /// Given the stream index, reads the GUID that is located there.
      /// </summary>
      /// <param name="streamIndex">The zero-based stream index.</param>
      /// <returns>The deserialized <see cref="Guid"/>, or <c>null</c> if <paramref name="streamIndex"/> is <c>0</c> or reading a <see cref="Guid"/> would go outside the bounds of this stream.</returns>
      Guid? GetGUID( Int32 streamIndex );
   }

   /// <summary>
   /// This interface should be implemented by objects handling reading of various string streams in meta data.
   /// </summary>
   public interface ReaderStringStreamHandler : AbstractReaderStreamHandler
   {
      /// <summary>
      /// Given the stream index, reads a string thatis located there.
      /// </summary>
      /// <param name="streamIndex">The zero-based stream index.</param>
      /// <returns>The deserialized string, or <c>null</c> if <paramref name="streamIndex"/> is <c>0</c> or reading a string would go outside the bounds of this stream.</returns>
      String GetString( Int32 streamIndex );
   }
}
