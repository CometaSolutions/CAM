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
using CILAssemblyManipulator.Physical.IO.Defaults;
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
   /// The <see cref="E_CILPhysical.ReadMetaDataFromStream(ReaderFunctionality, Stream, CILMetaDataTableInformationProvider, EventHandler{SerializationErrorEventArgs}, bool, out ImageInformation, out RawValueStorage{int}, out RVAConverter)"/> method will call the methods of this interface in the following order:
   /// <list type="number">
   /// <item><description><see cref="ReadImageInformation"/>,</description></item>
   /// <item><description><see cref="CreateStreamHandler"/> (once for each header in <see cref="MetaDataRoot.StreamHeaders"/>), and</description></item>
   /// <item><description><see cref="HandleStoredRawValues"/>.</description></item>
   /// </list>
   /// </remarks>
   /// <seealso cref="ReaderFunctionalityProvider"/>
   /// <seealso cref="ReaderFunctionalityProvider.GetFunctionality"/>
   /// <seealso cref="E_CILPhysical.ReadMetaDataFromStream(ReaderFunctionality, Stream, CILMetaDataTableInformationProvider, EventHandler{SerializationErrorEventArgs}, bool, out ImageInformation, out RawValueStorage{int}, out RVAConverter)"/>
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
      /// The <see cref="AbstractReaderStreamHandler"/>s returned by this method are further handled to <see cref="ReaderMetaDataStreamContainer"/>, used in <see cref="ReaderTableStreamHandler.PopulateMetaDataStructure"/> and <see cref="HandleStoredRawValues"/> methods.
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
      /// <param name="imageInfo">The full <see cref="ImageInformation"/>.</param>
      /// <param name="rvaConverter">The <see cref="RVAConverter"/> created by <see cref="ReadImageInformation"/> method.</param>
      /// <param name="mdStreamContainer">The <see cref="ReaderMetaDataStreamContainer"/> containing all streams created by <see cref="CreateStreamHandler"/> method.</param>
      /// <param name="md">The instance of <see cref="CILMetaData"/> that will be result of this deserialization process.</param>
      /// <param name="rawValues"></param>
      /// <remarks>
      /// The values that need data from elsewhere than <see cref="AbstractReaderStreamHandler"/>s, are at least:
      /// <list type="bullet">
      /// <item><description><see cref="MethodDefinition.IL"/>,</description></item>
      /// <item><description><see cref="FieldRVA.Data"/>, and</description></item>
      /// <item><description><see cref="ManifestResource.EmbeddedData"/>.</description></item>
      /// </list>
      /// </remarks>
      void HandleStoredRawValues(
         StreamHelper stream,
         ImageInformation imageInfo,
         RVAConverter rvaConverter,
         ReaderMetaDataStreamContainer mdStreamContainer,
         CILMetaData md,
         RawValueStorage<Int32> rawValues
         );
   }

   /// <summary>
   /// This is base class to store values (such as integers) now, and use the values later.
   /// The raw value storage has a pre-set capacity, which can not changed, and can only be filled once.
   /// </summary>
   /// <typeparam name="TValue">The type of the values to store.</typeparam>
   public sealed class RawValueStorage<TValue>
   {
      private readonly ArrayQuery<Int32> _tableSizes;
      private readonly Int32[] _tableColCount;
      private readonly Int32[] _tableStartOffsets;
      private readonly TValue[] _rawValues;
      private Int32 _currentIndex;

      /// <summary>
      /// Creates a new instance of <see cref="RawValueStorage{TValue}"/> with given information about table sizes and raw value column count for each table.
      /// </summary>
      /// <param name="tableSizes">The table size array. The index of the array is value of <see cref="Tables"/> enumeration, and the value in that array is the size of that table. So if <see cref="Tables.Module"/> would have 1 element, the element at index <c>0</c> (value of <see cref="Tables.Module"/>) would be <c>1</c>.</param>
      /// <param name="rawColumnInfo">The count of raw value columns for each table. The index of the array is value of <see cref="Tables"/> enumeration, and the value in that array is the raw column count. Since <see cref="Tables.MethodDef"/> has one raw value column (the method IL RVA), the element at index <c>6</c> (value of <see cref="Tables.MethodDef"/>) would be <c>1</c>.</param>
      public RawValueStorage(
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
      /// <exception cref="IndexOutOfRangeException">If this <see cref="RawValueStorage{TValue}"/> has already been filled.</exception>
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
         var size = this._tableSizes[(Int32) table];
         for ( var i = this._tableStartOffsets[(Int32) table]; i < size; ++i )
         {
            yield return this._rawValues[i + columnIndex];
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
      /// <param name="rowIndex"></param>
      /// <param name="columnIndex"></param>
      /// <returns></returns>
      public TValue GetRawValue( Tables table, Int32 rowIndex, Int32 columnIndex )
      {
         return this._rawValues[this._tableStartOffsets[(Int32) table] + rowIndex * this._tableColCount[(Int32) table] + columnIndex];
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

   /// <summary>
   /// This class encapsulates all <see cref="AbstractReaderStreamHandler"/>s created by <see cref="ReaderFunctionality.CreateStreamHandler"/> to be more easily accessable and useable.
   /// </summary>
   public class ReaderMetaDataStreamContainer
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
         )
      {
         this.BLOBs = blobs;
         this.GUIDs = guids;
         this.SystemStrings = sysStrings;
         this.UserStrings = userStrings;
         this.OtherStreams = ( otherStreams ?? Empty<AbstractReaderStreamHandler>.Enumerable ).ToArrayProxy().CQ;
      }

      /// <summary>
      /// Gets the <see cref="ReaderBLOBStreamHandler"/>.
      /// </summary>
      /// <value>The <see cref="ReaderBLOBStreamHandler"/>.</value>
      /// <remarks>
      /// This value may be <c>null</c>, if null was specific to the constructor of this <see cref="ReaderMetaDataStreamContainer"/>.
      /// </remarks>
      /// <seealso cref="ReaderBLOBStreamHandler"/>
      public ReaderBLOBStreamHandler BLOBs { get; }

      /// <summary>
      /// Gets the <see cref="ReaderGUIDStreamHandler"/>.
      /// </summary>
      /// <value>The <see cref="ReaderGUIDStreamHandler"/>.</value>
      /// <remarks>
      /// This value may be <c>null</c>, if null was specific to the constructor of this <see cref="ReaderMetaDataStreamContainer"/>.
      /// </remarks>
      /// <seealso cref="ReaderGUIDStreamHandler"/>
      public ReaderGUIDStreamHandler GUIDs { get; }

      /// <summary>
      /// Gets the <see cref="ReaderStringStreamHandler"/> for system strings.
      /// </summary>
      /// <value>The <see cref="ReaderStringStreamHandler"/> for system strings.</value>
      /// <remarks>
      /// This value may be <c>null</c>, if null was specific to the constructor of this <see cref="ReaderMetaDataStreamContainer"/>.
      /// </remarks>
      /// <seealso cref="ReaderStringStreamHandler"/>
      public ReaderStringStreamHandler SystemStrings { get; }

      /// <summary>
      /// Gets the <see cref="ReaderStringStreamHandler"/> for user strings.
      /// </summary>
      /// <value>The <see cref="ReaderStringStreamHandler"/> for user strings.</value>
      /// <remarks>
      /// This value may be <c>null</c>, if null was specific to the constructor of this <see cref="ReaderMetaDataStreamContainer"/>.
      /// </remarks>
      /// <seealso cref="ReaderStringStreamHandler"/>
      public ReaderStringStreamHandler UserStrings { get; }

      /// <summary>
      /// Gets the other <see cref="AbstractReaderStreamHandler"/>s given to this <see cref="ReaderMetaDataStreamContainer"/>.
      /// </summary>
      /// <value>The other <see cref="AbstractReaderStreamHandler"/>s given to this <see cref="ReaderMetaDataStreamContainer"/>.</value>
      /// <remarks>
      /// This value may be empty, but it is never <c>null</c>.
      /// </remarks>
      /// <seealso cref="AbstractReaderStreamHandler"/>
      public ArrayQuery<AbstractReaderStreamHandler> OtherStreams { get; }
   }

   /// <summary>
   /// This is common interface for all objects, which participate in deserialization process as handlers for meta data stream (e.g. table stream, BLOB stream, etc).
   /// </summary>
   /// <seealso cref="ReaderTableStreamHandler"/>
   /// <seealso cref="ReaderBLOBStreamHandler"/>
   /// <seealso cref="ReaderStringStreamHandler"/>
   /// <seealso cref="ReaderGUIDStreamHandler"/>
   public interface AbstractReaderStreamHandler
   {
      /// <summary>
      /// Gets the textual name of this <see cref="AbstractReaderStreamHandler"/>.
      /// </summary>
      /// <value>The textual name of this <see cref="AbstractReaderStreamHandler"/>.</value>
      String StreamName { get; }
   }

   /// <summary>
   /// This interface should be implemented by objects handling reading of table stream in meta data section.
   /// The table stream is where the structure of <see cref="CILMetaData"/> is defined (present tables, their size, etc).
   /// </summary>
   /// <remarks>
   /// The <see cref="E_CILPhysical.ReadMetaDataFromStream(ReaderFunctionality, Stream, CILMetaDataTableInformationProvider, EventHandler{SerializationErrorEventArgs}, bool, out ImageInformation, out RawValueStorage{int}, out RVAConverter)"/> method will call the methods of this interface in the following order:
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
      /// After that is done, all the remaining values (RVAs) should be stored to <see cref="RawValueStorage{TValue}"/> and returned by this method.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/> to populate.</param>
      /// <param name="mdStreamContainer">The <see cref="ReaderMetaDataStreamContainer"/> containing meta data streams.</param>
      /// <returns>The raw values (RVAs) which were read from this table stream. The stored raw values will be used by <see cref="ReaderFunctionality.HandleStoredRawValues"/> method.</returns>
      RawValueStorage<Int32> PopulateMetaDataStructure(
         CILMetaData md,
         ReaderMetaDataStreamContainer mdStreamContainer
         );

   }

   /// <summary>
   /// This interface should be implemented by objects handling reading of BLOB stream in meta data section.
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
      /// <param name="securityInfo">The list of seucurity information attributes to populate.</param>
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

   public interface ReaderGUIDStreamHandler : AbstractReaderStreamHandler
   {
      Guid? GetGUID( Int32 streamIndex );
   }

   public interface ReaderStringStreamHandler : AbstractReaderStreamHandler
   {
      String GetString( Int32 streamIndex );
   }
}

public static partial class E_CILPhysical
{
   public static CILMetaData ReadMetaDataFromStream(
      this ReaderFunctionalityProvider readerProvider,
      Stream stream,
      CILMetaDataTableInformationProvider tableInfoProvider,
      EventHandler<SerializationErrorEventArgs> errorHandler,
      Boolean readRawValues,
      out ImageInformation imageInfo
      )
   {
      if ( readerProvider == null )
      {
         throw new NullReferenceException();
      }

      if ( tableInfoProvider == null )
      {
         tableInfoProvider = DefaultMetaDataTableInformationProvider.CreateDefault();
      }

      Stream newStream;
      var reader = readerProvider.GetFunctionality( ArgumentValidator.ValidateNotNullAndReturn( "Stream", stream ), tableInfoProvider, errorHandler, out newStream );

      CILMetaData md;
      RawValueStorage<Int32> rawValueStorage;
      RVAConverter rvaConverter;
      if ( newStream != null && !ReferenceEquals( stream, newStream ) )
      {
         using ( newStream )
         {
            md = reader.ReadMetaDataFromStream( newStream, tableInfoProvider, errorHandler, readRawValues, out imageInfo, out rawValueStorage, out rvaConverter );
         }
      }
      else
      {
         md = reader.ReadMetaDataFromStream( stream, tableInfoProvider, errorHandler, readRawValues, out imageInfo, out rawValueStorage, out rvaConverter );
      }

      return md;
   }

   public static CILMetaData ReadMetaDataFromStream(
      this ReaderFunctionality reader,
      Stream stream,
      CILMetaDataTableInformationProvider tableInfoProvider,
      EventHandler<SerializationErrorEventArgs> errorHandler,
      Boolean readRawValues,
      out ImageInformation imageInfo,
      out RawValueStorage<Int32> rawValueStorage,
      out RVAConverter rvaConverter
      )
   {
      if ( reader == null )
      {
         throw new NullReferenceException();
         //reader = new DefaultReaderFunctionality( new TableSerializationInfoCreationArgs( errorHandler ) );
      }

      ArgumentValidator.ValidateNotNull( "Stream", stream );

      var helper = new StreamHelper( stream );

      // 1. Read image basic information (PE, sections, CLI header, md root)
      PEInformation peInfo;
      CLIHeader cliHeader;
      MetaDataRoot mdRoot;
      reader.ReadImageInformation( helper, out peInfo, out rvaConverter, out cliHeader, out mdRoot );

      if ( peInfo == null )
      {
         throw new BadImageFormatException( "Not a PE image." );
      }
      else if ( cliHeader == null )
      {
         throw new BadImageFormatException( "Missing CLI header." );
      }
      else if ( mdRoot == null )
      {
         throw new BadImageFormatException( "Missing meta-data root." );
      }

      if ( rvaConverter == null )
      {
         rvaConverter = new DefaultRVAConverter( peInfo.SectionHeaders );
      }

      // 2. Create MD streams
      var mdStreamHeaders = mdRoot.StreamHeaders;
      var mdStreams = new AbstractReaderStreamHandler[mdStreamHeaders.Count];
      for ( var i = 0; i < mdStreams.Length; ++i )
      {
         var hdr = mdStreamHeaders[i];
         var startPos = rvaConverter.ToOffset( cliHeader.MetaData.RVA ) + hdr.Offset;
         var mdHelper = helper.NewStreamPortion( startPos, (UInt32) hdr.Size );
         mdStreams[i] = reader.CreateStreamHandler( mdHelper, mdRoot, 0, hdr ) ?? CreateDefaultHandlerFor( hdr, helper );
      }

      // 3. Create and populate meta-data structure
      var tblMDStream = mdStreams
         .OfType<ReaderTableStreamHandler>()
         .FirstOrDefault();

      if ( tblMDStream == null )
      {
         throw new BadImageFormatException( "No table stream exists." );
      }

      var tblHeader = tblMDStream.ReadHeader();
      var md = CILMetaDataFactory.NewBlankMetaData( sizes: tblMDStream.TableSizes.ToArray(), tableInfoProvider: tableInfoProvider );
      var blobStream = mdStreams.OfType<ReaderBLOBStreamHandler>().FirstOrDefault();
      var guidStream = mdStreams.OfType<ReaderGUIDStreamHandler>().FirstOrDefault();
      var sysStringStream = mdStreams.OfType<ReaderStringStreamHandler>().FirstOrDefault( s => String.Equals( s.StreamName, MetaDataConstants.SYS_STRING_STREAM_NAME ) );
      var userStringStream = mdStreams.OfType<ReaderStringStreamHandler>().FirstOrDefault( s => String.Equals( s.StreamName, MetaDataConstants.USER_STRING_STREAM_NAME ) );
      var mdStreamContainer = new ReaderMetaDataStreamContainer(
            blobStream,
            guidStream,
            sysStringStream,
            userStringStream,
            mdStreams.Where( s => !ReferenceEquals( tblMDStream, s ) && !ReferenceEquals( blobStream, s ) && !ReferenceEquals( guidStream, s ) && !ReferenceEquals( sysStringStream, s ) && !ReferenceEquals( userStringStream, s ) )
            );

      rawValueStorage = tblMDStream.PopulateMetaDataStructure(
         md,
         mdStreamContainer
         );

      // 4. Create image information
      var snDD = cliHeader.StrongNameSignature;
      var snOffset = rvaConverter.ToOffset( snDD.RVA );
      imageInfo = new ImageInformation(
         peInfo,
         helper.NewDebugInformationFromStream( peInfo, rvaConverter ),
         new CLIInformation(
            cliHeader,
            mdRoot,
            tblHeader,
            snOffset > 0 && snDD.Size > 0 ?
               helper.At( snOffset ).ReadAndCreateArray( checked((Int32) snDD.Size) ).ToArrayProxy().CQ :
               null,
            rawValueStorage.GetAllRawValuesForColumn( Tables.MethodDef, 0 ).Select( rva => (UInt32) rva ).ToArrayProxy().CQ,
            rawValueStorage.GetAllRawValuesForColumn( Tables.FieldRVA, 0 ).Select( rva => (UInt32) rva ).ToArrayProxy().CQ
            )
         );

      // 5. Populate IL, FieldRVA, and ManifestResource data
      if ( readRawValues )
      {
         reader.HandleStoredRawValues( helper, imageInfo, rvaConverter, mdStreamContainer, md, rawValueStorage );
      }

      // We're done
      return md;
   }

   private static AbstractReaderStreamHandler CreateDefaultHandlerFor( MetaDataStreamHeader header, StreamHelper helper )
   {
      throw new NotImplementedException( "Creating default handler for stream." );
   }
}