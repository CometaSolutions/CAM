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
   /// This interface is the 'root' interface which controls what happens when reading <see cref="CILMetaData"/> with <see cref="M:CILMetaDataIO.ReadModule"/> method.
   /// To customize the deserialization process, it is possible to set <see cref="ReadingArguments.ReaderFunctionalityProvider"/> property to your own instances of <see cref="ReaderFunctionalityProvider"/>.
   /// </summary>
   /// <seealso cref="ReaderFunctionality"/>
   /// <seealso cref="ReadingArguments.ReaderFunctionalityProvider"/>
   /// <seealso cref="E_CILPhysical.ReadMetaDataFromStream(ReaderFunctionalityProvider, Stream, CILMetaDataTableInformationProvider, EventHandler{SerializationErrorEventArgs}, bool, out ImageInformation)"/>
   /// <seealso cref="T:CILAssemblyManipulator.Physical.IO.Defaults.DefaultReaderFunctionalityProvider"/>
   public interface ReaderFunctionalityProvider
   {
      /// <summary>
      /// Creates a new <see cref="ReaderFunctionality"/> to be used to read <see cref="CILMetaData"/> from <see cref="Stream"/>.
      /// Optionally, specifies a new <see cref="Stream"/> to be used.
      /// </summary>
      /// <param name="stream">The original <see cref="Stream"/>.</param>
      /// <param name="mdTableInfoProvider">The <see cref="CILMetaDataTableInformationProvider"/> which describes what tables are supported by this deserialization process.</param>
      /// <param name="errorHandler">The error handler callback.</param>
      /// <param name="deserialingDataReferences">Whether the data references (e.g. method RVAs, etc) will be deserialized.</param>
      /// <param name="newStream">Optional new <see cref="Stream"/> to use instead of <paramref name="stream"/> in the further desererialization process.</param>
      /// <returns>The <see cref="ReaderFunctionality"/> to use for actual deserialization.</returns>
      /// <seealso cref="ReaderFunctionality"/>
      /// <seealso cref="IOArguments.ErrorHandler"/>
      /// <seealso cref="CILMetaDataTableInformationProvider"/>
      /// <seealso cref="E_CILPhysical.ReadMetaDataFromStream(ReaderFunctionality, Stream,  EventHandler{SerializationErrorEventArgs}, bool, out ImageInformation)"/>
      ReaderFunctionality GetFunctionality(
         Stream stream,
         CILMetaDataTableInformationProvider mdTableInfoProvider,
         EventHandler<SerializationErrorEventArgs> errorHandler,
         Boolean deserialingDataReferences,
         out Stream newStream
         );
   }

   /// <summary>
   /// This interface provides core functionality to be used when deserializing <see cref="CILMetaData"/> from <see cref="Stream"/>.
   /// The instances of this interface are created via <see cref="ReaderFunctionalityProvider.GetFunctionality"/> method, and the instances of <see cref="ReaderFunctionalityProvider"/> may be customized by setting <see cref="ReadingArguments.ReaderFunctionalityProvider"/> property.
   /// </summary>
   /// <remarks>
   /// The <see cref="E_CILPhysical.ReadMetaDataFromStream(ReaderFunctionality, Stream, EventHandler{SerializationErrorEventArgs}, bool, out ImageInformation)"/> method will call the methods of this interface (and others) in the following order:
   /// <list type="number">
   /// <item><description><see cref="ReadImageInformation"/>,</description></item>
   /// <item><description><see cref="CreateStreamHandler"/> (once for each header in <see cref="MetaDataRoot.StreamHeaders"/>),</description></item>
   /// <item><description><see cref="ReaderTableStreamHandler.ReadHeader"/>,</description></item>
   /// <item><description><see cref="CreateBlankMetaData"/>,</description></item>
   /// <item><description><see cref="ReaderTableStreamHandler.PopulateMetaDataStructure"/>, and</description></item>
   /// <item><description><see cref="ReaderTableStreamHandler.HandleDataReferences"/>.</description></item>
   /// </list>
   /// </remarks>
   /// <seealso cref="ReaderFunctionalityProvider"/>
   /// <seealso cref="T:CILAssemblyManipulator.Physical.IO.Defaults.DefaultReaderFunctionality"/>
   /// <seealso cref="ReaderFunctionalityProvider.GetFunctionality"/>
   /// <seealso cref="E_CILPhysical.ReadMetaDataFromStream(ReaderFunctionality, Stream, EventHandler{SerializationErrorEventArgs}, bool, out ImageInformation)"/>
   /// <seealso cref="ReadingArguments.ReaderFunctionalityProvider"/>
   public interface ReaderFunctionality
   {

      /// <summary>
      /// This method should read required image information from the <see cref="StreamHelper"/>.
      /// </summary>
      /// <returns><c>true</c> if the PE image is managed assembly; <c>false</c> otherwise.</returns>
      /// <param name="stream">The <see cref="StreamHelper"/> object, encapsulating the actual <see cref="Stream"/>.</param>
      /// <param name="peInfo">This parameter should have the <see cref="PEInformation"/> object read from the stream.</param>
      /// <param name="rvaConverter">This parameter should have <see cref="RVAConverter"/> object to convert RVA values of this stream into actual offsets.</param>
      /// <param name="cliHeader">This parameter should have the <see cref="CLIHeader"/> object read from the stream.</param>
      /// <param name="mdRoot">This parameter should have the <see cref="MetaDataRoot"/> object read from the stream.</param>
      Boolean ReadImageInformation(
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
      /// The <see cref="AbstractReaderStreamHandler"/>s returned by this method are further handled to <see cref="ReaderMetaDataStreamContainer"/>, used in <see cref="ReaderTableStreamHandler.PopulateMetaDataStructure"/> and <see cref="ReaderTableStreamHandler.HandleDataReferences"/> methods.
      /// </remarks>
      AbstractReaderStreamHandler CreateStreamHandler(
         StreamHelper stream,
         MetaDataRoot mdRoot,
         Int64 startPosition,
         MetaDataStreamHeader header
         );

      /// <summary>
      /// This method should create blank <see cref="CILMetaData"/>, preferably with all meta data tables having the capacity of their lists set to the given table sizes.
      /// </summary>
      /// <param name="tableSizes">The table size array.</param>
      /// <returns>A new instance of <see cref="CILMetaData"/>.</returns>
      CILMetaData CreateBlankMetaData( ArrayQuery<Int32> tableSizes );


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
   /// The <see cref="E_CILPhysical.ReadMetaDataFromStream(ReaderFunctionality, Stream, EventHandler{SerializationErrorEventArgs}, bool, out ImageInformation)"/> method will call the methods of this interface in the following order:
   /// <list type="number">
   /// <item><description><see cref="ReadHeader"/>,</description></item>
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
      /// This method should populate those properties of the rows in metadata tables, which are either stored directly in table stream, or can be created using meta data streams (e.g. strings, guids, signatures, etc).
      /// After that is done, all the remaining data references (e.g. RVAs) should be returned by this method.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/> to populate.</param>
      /// <param name="mdStreamContainer">The <see cref="ReaderMetaDataStreamContainer"/> containing meta data streams.</param>
      /// <returns>The data references (e.g. RVAs) which were read from this table stream. The data references will be used by <see cref="HandleDataReferences"/> method.</returns>
      DataReferencesInfo PopulateMetaDataStructure(
         CILMetaData md,
         ReaderMetaDataStreamContainer mdStreamContainer
         );

      /// <summary>
      /// This method is called after <see cref="ReaderTableStreamHandler.PopulateMetaDataStructure"/>, to populate the values that need data from elsewhere than <see cref="AbstractReaderStreamHandler"/>s.
      /// </summary>
      /// <param name="stream">The <see cref="StreamHelper"/> object, encapsulating the actual <see cref="Stream"/>.</param>
      /// <param name="imageInfo">The full <see cref="ImageInformation"/>. The data references will be in <see cref="CLIInformation.DataReferences"/> property.</param>
      /// <param name="rvaConverter">The <see cref="RVAConverter"/> created by <see cref="ReaderFunctionality.ReadImageInformation"/> method.</param>
      /// <param name="mdStreamContainer">The <see cref="ReaderMetaDataStreamContainer"/> containing all other streams created by <see cref="ReaderFunctionality.CreateStreamHandler"/> method.</param>
      /// <param name="md">The instance of <see cref="CILMetaData"/> that will be result of this deserialization process.</param>
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
      AbstractCustomAttributeSignature ReadCASignature( Int32 streamIndex, SignatureProvider sigProvider );

      /// <summary>
      /// Given the stream index, reads the security information into given list of <see cref="AbstractSecurityInformation"/>s.
      /// </summary>
      /// <param name="streamIndex">The zero-based stream index.</param>
      /// <param name="sigProvider">The <see cref="SignatureProvider"/> to use when deserializing the signature.</param>
      /// <param name="securityInfo">The list of security information attributes to populate.</param>
      /// <seealso cref="SecurityDefinition.PermissionSets"/>
      void ReadSecurityInformation( Int32 streamIndex, SignatureProvider sigProvider, List<AbstractSecurityInformation> securityInfo );

      /// <summary>
      /// Given the stream index, reads the marshaling information as <see cref="AbstractMarshalingInfo"/>.
      /// </summary>
      /// <param name="streamIndex">The zero-based stream index.</param>
      /// <param name="sigProvider">The <see cref="SignatureProvider"/> to use when deserializing the signature.</param>
      /// <returns>One of the concrete subclasses of <see cref="AbstractMarshalingInfo"/>.</returns>
      /// <seealso cref="M:E_CILPhysical.ReadMarshalingInfo"/>
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
   public interface ReaderStringStreamHandler : AbstractReaderStreamHandler, AbstractStringStreamHandler
   {
      /// <summary>
      /// Given the stream index, reads a string thatis located there.
      /// </summary>
      /// <param name="streamIndex">The zero-based stream index.</param>
      /// <returns>The deserialized string, or <c>null</c> if <paramref name="streamIndex"/> is <c>0</c> or reading a string would go outside the bounds of this stream.</returns>
      String GetString( Int32 streamIndex );
   }

}

public static partial class E_CILPhysical
{
   /// <summary>
   /// This is extension method to read <see cref="CILMetaData"/> from <see cref="Stream"/> using this <see cref="ReaderFunctionalityProvider"/>.
   /// It takes into account the possible new stream created by <see cref="ReaderFunctionalityProvider.GetFunctionality"/> method.
   /// </summary>
   /// <param name="readerProvider">This <see cref="ReaderFunctionalityProvider"/>.</param>
   /// <param name="stream">The <see cref="Stream"/> to read <see cref="CILMetaData"/> from.</param>
   /// <param name="tableInfoProvider">The <see cref="CILMetaDataTableInformationProvider"/> to use when creating a new instance of <see cref="CILMetaData"/> with <see cref="M:CILMetaDataFactory.NewBlankMetaData"/>.</param>
   /// <param name="errorHandler">The callback to handle errors during deserialization.</param>
   /// <param name="deserializeDataReferences">Whether to deserialize data references (e.g. <see cref="MethodDefinition.IL"/>).</param>
   /// <param name="imageInfo">This parameter will hold the <see cref="ImageInformation"/> read from the <see cref="Stream"/>.</param>
   /// <returns>An instance of <see cref="CILMetaData"/> with its data read from the <paramref name="stream"/>.</returns>
   /// <exception cref="BadImageFormatException">If the structure of the image represented by <see cref="Stream"/> is invalid (e.g. missing PE header or CLI header, etc).</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="stream"/> or <paramref name="tableInfoProvider"/> is <c>null</c>.</exception>
   /// <exception cref="NullReferenceException">If this <see cref="ReaderFunctionalityProvider"/> is <c>null</c>.</exception>
   /// <exception cref="SerializationFunctionalityException">If this <see cref="ReaderFunctionalityProvider"/> or the objects it creates return <c>null</c>s when they shouldn't.</exception>
   /// <remarks>
   /// This method is used by <see cref="M:CILMetaDataIO.ReadModule"/> to actually perform deserialization, and thus is rarely needed to be used directly.
   /// Instead, use <see cref="M:CILMetaDataIO.ReadModule"/> or any of the classes implementing <see cref="T:CILAssemblyManipulator.Physical.Loading.CILMetaDataLoader"/>.
   /// </remarks>
   /// <seealso cref="ReaderFunctionalityProvider"/>
   /// <seealso cref="M:CILMetaDataIO.ReadModule"/>
   /// <seealso cref="T:CILAssemblyManipulator.Physical.Loading.CILMetaDataLoader"/>
   /// <seealso cref="ReadMetaDataFromStream(ReaderFunctionality, Stream, EventHandler{SerializationErrorEventArgs}, bool, out ImageInformation)"/>
   public static CILMetaData ReadMetaDataFromStream(
      this ReaderFunctionalityProvider readerProvider,
      Stream stream,
      CILMetaDataTableInformationProvider tableInfoProvider,
      EventHandler<SerializationErrorEventArgs> errorHandler,
      Boolean deserializeDataReferences,
      out ImageInformation imageInfo
      )
   {
      ArgumentValidator.ValidateNotNullReference( readerProvider );
      Stream newStream;
      var reader = readerProvider
         .GetFunctionality( ArgumentValidator.ValidateNotNull( "Stream", stream ), ArgumentValidator.ValidateNotNull( "Table info provider", tableInfoProvider ), errorHandler, deserializeDataReferences, out newStream )
         .CheckForDeserializationException( "Reader", true );

      CILMetaData md;
      if ( newStream != null && !ReferenceEquals( stream, newStream ) )
      {
         using ( newStream )
         {
            md = reader.ReadMetaDataFromStream( newStream, errorHandler, deserializeDataReferences, out imageInfo );
         }
      }
      else
      {
         md = reader.ReadMetaDataFromStream( stream, errorHandler, deserializeDataReferences, out imageInfo );
      }

      return md;
   }

   /// <summary>
   /// This is extension method to read <see cref="CILMetaData"/> from <see cref="Stream"/> using this <see cref="ReaderFunctionality"/>.
   /// </summary>
   /// <param name="reader">This <see cref="ReaderFunctionality"/>.</param>
   /// <param name="stream">The <see cref="Stream"/> to read <see cref="CILMetaData"/> from.</param>
   /// <param name="errorHandler">The callback to handle errors during deserialization.</param>
   /// <param name="deserializeDataReferences">Whether to deserialize data references (e.g. <see cref="MethodDefinition.IL"/>).</param>
   /// <param name="imageInfo">This parameter will hold the <see cref="ImageInformation"/> read from the <see cref="Stream"/>.</param>
   /// <returns>An instance of <see cref="CILMetaData"/> with its data read from the <paramref name="stream"/>.</returns>
   /// <exception cref="BadImageFormatException">If the structure of the image represented by <see cref="Stream"/> is invalid (e.g. missing PE header or CLI header, etc).</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="stream"/> is <c>null</c>.</exception>
   /// <exception cref="NullReferenceException">If this <see cref="ReaderFunctionality"/> is <c>null</c>.</exception>
   /// <exception cref="SerializationFunctionalityException">If this <see cref="ReaderFunctionality"/> or the objects it creates return <c>null</c>s when they shouldn't.</exception>
   /// <remarks>
   /// This method is used by <see cref="ReadMetaDataFromStream(ReaderFunctionalityProvider, Stream, CILMetaDataTableInformationProvider, EventHandler{SerializationErrorEventArgs}, bool, out ImageInformation)"/>, which in turn is used by <see cref="M:CILMetaDataIO.ReadModule"/> to actually perform deserialization.
   /// Thus, this method is rarely needed to be used directly.
   /// Instead, use <see cref="M:CILMetaDataIO.ReadModule"/> or any of the classes implementing <see cref="T:CILAssemblyManipulator.Physical.Loading.CILMetaDataLoader"/>.
   /// </remarks>
   public static CILMetaData ReadMetaDataFromStream(
      this ReaderFunctionality reader,
      Stream stream,
      EventHandler<SerializationErrorEventArgs> errorHandler,
      Boolean deserializeDataReferences,
      out ImageInformation imageInfo
      )
   {
      ArgumentValidator.ValidateNotNullReference( reader );
      ArgumentValidator.ValidateNotNull( "Stream", stream );

      var helper = new StreamHelper( stream );

      // 1. Read image basic information (PE, sections, CLI header, md root)
      PEInformation peInfo;
      CLIHeader cliHeader;
      MetaDataRoot mdRoot;
      RVAConverter rvaConverter;
      var wasManaged = reader
         .ReadImageInformation( helper, out peInfo, out rvaConverter, out cliHeader, out mdRoot );

      if ( !wasManaged )
      {
         throw new BadImageFormatException( "Not a managed assembly." );
      }
      peInfo.CheckForDeserializationException( "PE information" );
      cliHeader.CheckForDeserializationException( "CLI header" );
      mdRoot.CheckForDeserializationException( "Meta data root" );

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
         mdStreams[i] = reader.CreateStreamHandler( mdHelper, mdRoot, 0, hdr );
      }

      // 3. Create and populate meta-data structure
      var tblMDStream = mdStreams
         .OfType<ReaderTableStreamHandler>()
         .FirstOrDefault()
         .CheckForDeserializationException( "Table stream" );

      var tblHeader = tblMDStream.ReadHeader().CheckForDeserializationException( "Table stream header" );

      var md = reader.CreateBlankMetaData( tblHeader.CreateTableSizesArray().ToArrayProxy().CQ ).CheckForDeserializationException( "Blank meta data" );

      var blobStream = mdStreams.OfType<ReaderBLOBStreamHandler>().FirstOrDefault();
      var guidStream = mdStreams.OfType<ReaderGUIDStreamHandler>().FirstOrDefault();
      var sysStringStream = mdStreams.OfType<ReaderStringStreamHandler>().FirstOrDefault( s => s.StringStreamKind == StringStreamKind.SystemStrings );
      var userStringStream = mdStreams.OfType<ReaderStringStreamHandler>().FirstOrDefault( s => s.StringStreamKind == StringStreamKind.UserStrings );
      var mdStreamContainer = new ReaderMetaDataStreamContainer(
            blobStream,
            guidStream,
            sysStringStream,
            userStringStream,
            mdStreams.Where( s => !ReferenceEquals( tblMDStream, s ) && !ReferenceEquals( blobStream, s ) && !ReferenceEquals( guidStream, s ) && !ReferenceEquals( sysStringStream, s ) && !ReferenceEquals( userStringStream, s ) )
            );

      var dataReferences = tblMDStream.PopulateMetaDataStructure(
         md,
         mdStreamContainer
         ).CheckForDeserializationException( "Data references" );

      // 4. Create image information
      var snDD = cliHeader.StrongNameSignature;
      var snOffset = rvaConverter.ToOffset( snDD.RVA );
      imageInfo = new ImageInformation(
         peInfo,
         helper.ReadDebugInformation( peInfo, rvaConverter ),
         new CLIInformation(
            cliHeader,
            mdRoot,
            tblHeader,
            snOffset > 0 && snDD.Size > 0 ?
               helper.At( snOffset ).ReadAndCreateArray( checked((Int32) snDD.Size) ).ToArrayProxy().CQ :
               null,
             dataReferences
            )
         );

      // 5. Populate IL, FieldRVA, and ManifestResource data
      if ( deserializeDataReferences )
      {
         tblMDStream.HandleDataReferences( helper, imageInfo, rvaConverter, mdStreamContainer, md );
      }

      // We're done
      return md;
   }

   /// <summary>
   /// This is helper method to position given stream at offset transformed from given RVA.
   /// </summary>
   /// <param name="stream">The stream, as <see cref="StreamHelper"/>.</param>
   /// <param name="rvaConverter">The <see cref="RVAConverter"/> to use.</param>
   /// <param name="rva">The RVA.</param>
   /// <returns>The <paramref name="stream"/> positioned at offset transformed from given RVA.</returns>
   public static StreamHelper GoToRVA( this StreamHelper stream, RVAConverter rvaConverter, Int64 rva )
   {
      stream.Stream.SeekFromBegin( rvaConverter.ToOffset( rva ) );
      return stream;
   }


   private static DebugInformation ReadDebugInformation( this StreamHelper stream, PEInformation peInfo, RVAConverter rvaConverter )
   {
      var dataDirs = peInfo.NTHeader.OptionalHeader.DataDirectories;
      DataDirectory debugDD;
      var debugDDIdx = (Int32) DataDirectories.Debug;
      return dataDirs.Count > debugDDIdx
         && ( debugDD = dataDirs[debugDDIdx] ).RVA > 0 ?
         stream
            .At( rvaConverter.ToOffset( debugDD.RVA ) )
            .ReadDebugInformation() :
         null;
   }

   private static T CheckForDeserializationException<T>( this T value, String what, Boolean isProvider = false )
      where T : class
   {
      if ( value == null )
      {
         throw SerializationFunctionalityException.ExceptionDuringDeserialization( what, isProvider );
      }
      return value;
   }
}