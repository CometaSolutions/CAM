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

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.IO;
using CILAssemblyManipulator.Physical.Crypto;
using CollectionsWithRoles.API;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CILAssemblyManipulator.Physical.Meta;

namespace CILAssemblyManipulator.Physical.IO
{
   /// <summary>
   /// This interface is the 'root' interface which controls what happens when writing <see cref="CILMetaData"/> with <see cref="E_CILPhysical.WriteModule"/> method.
   /// To customize the serialization process, it is possible to set <see cref="WritingArguments.WriterFunctionalityProvider"/> property to your own instances of <see cref="WriterFunctionalityProvider"/>.
   /// </summary>
   /// <seealso cref="WriterFunctionality"/>
   /// <seealso cref="WritingArguments.ReaderFunctionalityProvider"/>
   /// <seealso cref="E_CILPhysical.WriteMetaDataToStream(WriterFunctionalityProvider, Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/>
   /// <seealso cref="DefaultWriterFunctionalityProvider"/>
   public interface WriterFunctionalityProvider
   {
      /// <summary>
      /// Creates a new <see cref="WriterFunctionality"/> to be used to write <see cref="CILMetaData"/> to <see cref="Stream"/>.
      /// Optionally, specifies a new <see cref="Stream"/> and/or a new <see cref="CILMetaData"/> to use.
      /// </summary>
      /// <param name="md">The original <see cref="CILMetaData"/>.</param>
      /// <param name="options">The <see cref="WritingOptions"/> being used.</param>
      /// <param name="errorHandler">The error handler callback.</param>
      /// <param name="newMD">Optional new <see cref="CILMetaData"/> to use instead of <paramref name="md"/> in the further serialization process.</param>
      /// <param name="newStream">Optional new <see cref="Stream"/> to use instead of <paramref name="stream"/> in the further sererialization process.</param>
      /// <returns>The <see cref="WriterFunctionality"/> to use for actual deserialization.</returns>
      /// <seealso cref="WriterFunctionality"/>
      /// <seealso cref="IOArguments.ErrorHandler"/>
      /// <seealso cref="E_CILPhysical.WriteMetaDataToStream(WriterFunctionality, Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/>
      WriterFunctionality GetFunctionality(
         CILMetaData md,
         WritingOptions options,
         EventHandler<SerializationErrorEventArgs> errorHandler,
         out CILMetaData newMD,
         out Stream newStream
         );
   }

   /// <summary>
   /// This interface provides core functionality to be used when serializing <see cref="CILMetaData"/> to <see cref="Stream"/>.
   /// The instances of this interface are created via <see cref="WriterFunctionalityProvider.GetFunctionality"/> method, and the instances of <see cref="WriterFunctionalityProvider"/> may be customized by setting <see cref="WritingArguments.WriterFunctionalityProvider"/> property.
   /// </summary>
   /// <remarks>
   /// The <see cref="E_CILPhysical.WriteMetaDataToStream(WriterFunctionality, Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/> method will call the methods of this interface (and others) in the following order:
   /// <list type="number">
   /// <item><description><see cref="CreateWritingStatus"/>,</description></item>
   /// <item><description><see cref="CreateMetaDataStreamHandlers"/>,</description></item>
   /// <item><description><see cref="WriterTableStreamHandler.FillOtherMDStreams"/>,</description></item>
   /// <item><description><see cref="CalculateImageLayout"/>,</description></item>
   /// <item><description><see cref="BeforeMetaData"/>,</description></item>
   /// <item><description><see cref="WriteMDRoot"/>,</description></item>
   /// <item><description><see cref="AbstractWriterStreamHandler.WriteStream"/> (once for each of the streams returned by <see cref="CreateMetaDataStreamHandlers"/>, in the same order as they were returned by <see cref="CreateMetaDataStreamHandlers"/>),</description></item>
   /// <item><description><see cref="AfterMetaData"/>, and</description></item>
   /// <item><description><see cref="WritePEInformation"/>.</description></item>
   /// </list>
   /// </remarks>
   /// <seealso cref="WriterFunctionalityProvider"/>
   /// <seealso cref="DefaultWriterFunctionality"/>
   /// <seealso cref="WriterFunctionalityProvider.GetFunctionality"/>
   /// <seealso cref="E_CILPhysical.WriteMetaDataToStream(WriterFunctionality, Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/>
   /// <seealso cref="WritingArguments.WriterFunctionalityProvider"/>
   public interface WriterFunctionality
   {
      /// <summary>
      /// This method should return enumerable of all <see cref="AbstractWriterStreamHandler"/>s supported by this <see cref="WriterFunctionality"/>.
      /// </summary>
      /// <returns>An enumerable of all <see cref="AbstractWriterStreamHandler"/>s supported by this <see cref="WriterFunctionality"/>.</returns>
      /// <seealso cref="AbstractWriterStreamHandler"/>
      IEnumerable<AbstractWriterStreamHandler> CreateMetaDataStreamHandlers();

      /// <summary>
      /// This method should create a new instance of <see cref="WritingStatus"/> to be used throughout the serialization process.
      /// </summary>
      /// <param name="snVars">The <see cref="StrongNameInformation"/> for current serialization process. May be <c>null</c> if the resulting image will not be strong-name signed.</param>
      /// <returns>A new instance of <see cref="WritingStatus"/> object.</returns>
      WritingStatus CreateWritingStatus(
         StrongNameInformation snVars
         );

      /// <summary>
      /// This method is called to calculate the layout for the image created by serialization process.
      /// </summary>
      /// <param name="writingStatus">The <see cref="WritingStatus"/> created by<see cref="CreateWritingStatus"/> method.</param>
      /// <param name="mdStreamContainer">The <see cref="WriterMetaDataStreamContainer"/>.</param>
      /// <param name="allStreams">All streams, as returned by <see cref="CreateMetaDataStreamHandlers"/> method.</param>
      /// <param name="rvaConverter">This parameter should contain the <see cref="RVAConverter"/> to be used when serialization process converts from RVAs to offsets.</param>
      /// <param name="mdRootSize">This parameter should contain the size of the <see cref="MetaDataRoot"/> in bytes.</param>
      /// <returns>A <see cref="ColumnValueStorage{TValue}"/> filled with data offsets, e.g. <see cref="MethodDefinition.IL"/> RVAs, and so on.</returns>
      /// <remarks>
      /// The returned <see cref="ColumnValueStorage{TValue}"/> will be used as argument for <see cref="AbstractWriterStreamHandler.WriteStream"/> method.
      /// </remarks>
      /// <seealso cref="ColumnValueStorage{TValue}"/>
      /// <seealso cref="AbstractWriterStreamHandler.WriteStream"/>
      IEnumerable<DataReferenceInfo> CalculateImageLayout(
         WritingStatus writingStatus,
         WriterMetaDataStreamContainer mdStreamContainer,
         IEnumerable<AbstractWriterStreamHandler> allStreams,
         out RVAConverter rvaConverter,
         out Int32 mdRootSize
         );

      /// <summary>
      /// This method should write whatever is needed before the metadata itself is written to the stream.
      /// </summary>
      /// <param name="writingStatus">The <see cref="WritingStatus"/> created by<see cref="CreateWritingStatus"/> method.</param>
      /// <param name="stream">The <see cref="Stream"/> to write data to.</param>
      /// <param name="array">The <see cref="ResizableArray{T}"/> helper.</param>
      void BeforeMetaData(
         WritingStatus writingStatus,
         Stream stream,
         ResizableArray<Byte> array
         );

      /// <summary>
      /// This method should write the metadata root.
      /// </summary>
      /// <param name="writingStatus">The <see cref="WritingStatus"/> created by<see cref="CreateWritingStatus"/> method.</param>
      /// <param name="array">The <see cref="ResizableArray{T}"/> helper.</param>
      void WriteMDRoot(
         WritingStatus writingStatus,
         ResizableArray<Byte> array
         );

      /// <summary>
      /// This method should write whatever is needed after the metadata itself is written to the stream.
      /// </summary>
      /// <param name="writingStatus">The <see cref="WritingStatus"/> created by<see cref="CreateWritingStatus"/> method.</param>
      /// <param name="stream">The <see cref="Stream"/> to write data to.</param>
      /// <param name="array">The <see cref="ResizableArray{T}"/> helper.</param>
      void AfterMetaData(
         WritingStatus writingStatus,
         Stream stream,
         ResizableArray<Byte> array
         );

      /// <summary>
      /// This method should write the given <see cref="PEInformation"/> to the given <see cref="Stream"/>.
      /// </summary>
      /// <param name="writingStatus">The <see cref="WritingStatus"/> created by<see cref="CreateWritingStatus"/> method.</param>
      /// <param name="stream">The <see cref="Stream"/> to write data to.</param>
      /// <param name="array">The <see cref="ResizableArray{T}"/> helper.</param>
      /// <param name="peInfo">The <see cref="PEInformation"/> object.</param>
      void WritePEInformation(
         WritingStatus writingStatus,
         Stream stream,
         ResizableArray<Byte> array,
         PEInformation peInfo
         );
   }

   /// <summary>
   /// This class encapsulates all <see cref="AbstractWriterStreamHandler"/>s created by <see cref="WriterFunctionality.CreateMetaDataStreamHandlers"/> (except <see cref="WriterTableStreamHandler"/>) to be more easily accessable and useable.
   /// </summary>
   public class WriterMetaDataStreamContainer : MetaDataStreamContainer<AbstractWriterStreamHandler, WriterBLOBStreamHandler, WriterGUIDStreamHandler, WriterStringStreamHandler>
   {
      /// <summary>
      /// Creates a new instance of <see cref="WriterMetaDataStreamContainer"/> with given streams.
      /// </summary>
      /// <param name="blobs">The <see cref="WriterBLOBStreamHandler"/> for <c>#Blobs</c> stream.</param>
      /// <param name="guids">The <see cref="WriterGUIDStreamHandler"/> for <c>#GUID</c> stream.</param>
      /// <param name="sysStrings">The <see cref="WriterStringStreamHandler"/> for <c>#String</c> stream.</param>
      /// <param name="userStrings">The <see cref="WriterStringStreamHandler"/> for <c>#US</c> stream.</param>
      /// <param name="otherStreams">Any other streams.</param>
      /// <remarks>
      /// None of the parameters are checked for <c>null</c> values.
      /// </remarks>
      public WriterMetaDataStreamContainer(
         WriterBLOBStreamHandler blobs,
         WriterGUIDStreamHandler guids,
         WriterStringStreamHandler sysStrings,
         WriterStringStreamHandler userStrings,
         IEnumerable<AbstractWriterStreamHandler> otherStreams
         ) : base( blobs, guids, sysStrings, userStrings, otherStreams )
      {
      }
   }

   /// <summary>
   /// This is common interface for all objects, which participate in serialization process as handlers for meta data stream (e.g. table stream, BLOB stream, etc).
   /// </summary>
   /// <seealso cref="WriterTableStreamHandler"/>
   /// <seealso cref="WriterBLOBStreamHandler"/>
   /// <seealso cref="WriterStringStreamHandler"/>
   /// <seealso cref="WriterGUIDStreamHandler"/>
   public interface AbstractWriterStreamHandler : AbstractMetaDataStreamHandler
   {

      /// <summary>
      /// Writes the current contents of this <see cref="AbstractWriterStreamHandler"/> to given <see cref="Stream"/>.
      /// </summary>
      /// <param name="stream">The <see cref="Stream"/> to write the contents to.</param>
      /// <param name="array">The auxiliary <see cref="ResizableArray{T}"/> to use.</param>
      /// <param name="dataReferences">The data references created by <see cref="WriterFunctionality.CalculateImageLayout"/>.</param>
      void WriteStream(
         Stream stream,
         ResizableArray<Byte> array,
         DataReferencesInfo dataReferences
         );


      /// <summary>
      /// Gets the value indicating whether this <see cref="AbstractWriterStreamHandler"/> has been accessed in some way yet.
      /// </summary>
      /// <value>The value indicating whether this <see cref="AbstractWriterStreamHandler"/> has been accessed in some way yet.</value>
      Boolean Accessed { get; }
   }

   /// <summary>
   /// This interface should be implemented by objects handling writing of table stream in meta data.
   /// The table stream is where the structure of <see cref="CILMetaData"/> is defined (present tables, their size, etc).
   /// </summary>
   /// <remarks>
   /// The <see cref="E_CILPhysical.WriteMetaDataToStream(WriterFunctionality, Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/> method will call the methods of this interface in the following order:
   /// <list type="number">
   /// <item><description><see cref="FillOtherMDStreams"/>, and</description></item>
   /// <item><description><see cref="AbstractWriterStreamHandler.WriteStream"/>.</description></item>
   /// </list>
   /// </remarks>
   public interface WriterTableStreamHandler : AbstractWriterStreamHandler
   {
      /// <summary>
      /// Fills the other meta data streams represented by <see cref="AbstractWriterStreamHandler"/>s with the data from the <see cref="CILMetaData"/> originally supplied to <see cref="WriterFunctionalityProvider.GetFunctionality"/> method.
      /// </summary>
      /// <param name="publicKey">The public key to use instead of <see cref="AssemblyInformation.PublicKeyOrToken"/> of the <see cref="AssemblyDefinition"/> row.</param>
      /// <param name="mdStreams">The <see cref="WriterMetaDataStreamContainer"/> object containing other <see cref="AbstractWriterStreamHandler"/>s returned by <see cref="WriterFunctionality.CreateMetaDataStreamHandlers"/> method.</param>
      /// <param name="array">The auxiliary byte <see cref="ResizableArray{T}"/>.</param>
      /// <returns>A new instance of <see cref="MetaDataTableStreamHeader"/> describing the header for this meta-data.</returns>
      MetaDataTableStreamHeader FillOtherMDStreams(
         ArrayQuery<Byte> publicKey,
         WriterMetaDataStreamContainer mdStreams,
         ResizableArray<Byte> array
         );
   }

   /// <summary>
   /// This interface should be implemented by objects handling writing of BLOB stream in meta data.
   /// </summary>
   public interface WriterBLOBStreamHandler : AbstractWriterStreamHandler
   {
      /// <summary>
      /// Gets the index for given BLOB as byte array.
      /// </summary>
      /// <param name="blob">The BLOB byte array. May be <c>null</c>.</param>
      /// <returns>An index for the given BLOB byte array.</returns>
      Int32 RegisterBLOB( Byte[] blob );
   }

   /// <summary>
   /// This interface should be implemented by objects handling writing of GUID stream in meta data.
   /// </summary>
   public interface WriterGUIDStreamHandler : AbstractWriterStreamHandler
   {
      /// <summary>
      /// Gets the index for given <see cref="Guid"/>.
      /// </summary>
      /// <param name="guid">The <see cref="Guid"/>. May be <c>null</c>.</param>
      /// <returns>An index for the given <see cref="Guid"/>.</returns>
      Int32 RegisterGUID( Guid? guid );
   }

   /// <summary>
   /// This interface should be implemented by objects handling writing of various string streams in meta data.
   /// </summary>
   public interface WriterStringStreamHandler : AbstractWriterStreamHandler
   {
      /// <summary>
      /// Gets the index for given string.
      /// </summary>
      /// <param name="str">The string. May be <c>null</c>.</param>
      /// <returns>An index for the given string.</returns>
      Int32 RegisterString( String str );
   }

   /// <summary>
   /// This class contains various information about the image being writen by <see cref="E_CILPhysical.WriteMetaDataToStream(WriterFunctionality, Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/> method.
   /// Some of the information is read-only and set by the method, and another should be set by <see cref="WriterFunctionality"/> itself or the objects it creates.
   /// The purpose of this object is to hide when and where exactly the mutable information is set, as it will be needed only at the very end of the serialization process.
   /// </summary>
   /// <remarks>
   /// Custom <see cref="WriterFunctionality"/> implementors may extend this class containing information specific to their customized writing process.
   /// </remarks>
   public class WritingStatus
   {
      private const Int32 DEFAULT_FILE_ALIGNMENT = 0x200;
      private const Int32 DEFAULT_SECTION_ALIGNMENT = 0x2000;

      /// <summary>
      /// Creates a new instance of <see cref="WritingStatus"/> with given parameters.
      /// </summary>
      /// <param name="headersSizeUnaligned">The exact, unaligned size of the headers. See <see cref="HeadersSizeUnaligned"/> for more information.</param>
      /// <param name="machine">The <see cref="ImageFileMachine"/> enumeration describing the target machine for the image.</param>
      /// <param name="fileAlignment">The optional file alignment. If none supplied, the default will be used.</param>
      /// <param name="sectionAlignment">The optional section alignment. If none supplied, the default will be used.</param>
      /// <param name="imageBase">The optional image base. If none supplied, the default will be used. This default will depend on the <paramref name="machine"/>.</param>
      /// <param name="strongNameVariables">The optional <see cref="IO.StrongNameInformation"/>, describing the public key information for the assembly being emitted. May be <c>null</c> if assembly is not strong-name signed.</param>
      /// <param name="dataDirCount">The amount of data directories in PE header (these will become <see cref="OptionalHeader.DataDirectories"/>). The amount of <see cref="PEDataDirectories"/> will be this amount.</param>
      /// <param name="sectionsCount">The amount of sections in this image. The amount of <see cref="SectionHeaders"/> will be this amount.</param>
      public WritingStatus(
         Int32 headersSizeUnaligned,
         ImageFileMachine machine,
         Int32? fileAlignment,
         Int32? sectionAlignment,
         Int64? imageBase,
         StrongNameInformation strongNameVariables,
         Int32 dataDirCount,
         Int32 sectionsCount
         )
      {
         var fAlign = CheckAlignment( fileAlignment ?? DEFAULT_FILE_ALIGNMENT, DEFAULT_FILE_ALIGNMENT );
         var sAlign = CheckAlignment( sectionAlignment ?? DEFAULT_SECTION_ALIGNMENT, DEFAULT_SECTION_ALIGNMENT );
         this.HeadersSizeUnaligned = headersSizeUnaligned;
         this.Machine = machine;
         this.FileAlignment = fAlign;
         this.SectionAlignment = sAlign;
         this.ImageBase = imageBase ?? ( machine.RequiresPE64() ? 0x0000000140000000 : 0x0000000000400000 );
         this.StrongNameInformation = strongNameVariables;
         this.PEDataDirectories = Enumerable.Repeat( default( DataDirectory ), dataDirCount ).ToArray();
         this.SectionHeaders = Enumerable.Repeat<SectionHeader>( null, sectionsCount ).ToArray();
      }

      /// <summary>
      /// Gets the exact, unaligned size of the headers.
      /// </summary>
      /// <value>The exact, unaligned size of the headers.</value>
      /// <remarks>
      /// The size of the following headers should be included:
      /// <list type="bullet">
      /// <item><description><see cref="DOSHeader"/>,</description></item>
      /// <item><description><see cref="NTHeader"/>, and</description></item>
      /// <item><description>all of the <see cref="SectionHeader"/>s.</description></item>
      /// </list>
      /// </remarks>
      public Int32 HeadersSizeUnaligned { get; }

      /// <summary>
      /// Gets the <see cref="ImageFileMachine"/> of the image.
      /// </summary>
      /// <value>The <see cref="ImageFileMachine"/> of the image.</value>
      /// <seealso cref="ImageFileMachine"/>
      public ImageFileMachine Machine { get; }

      /// <summary>
      /// Gets the file alignment of the image.
      /// </summary>
      /// <value>The file alignment of the image.</value>
      /// <seealso cref="OptionalHeader.FileAlignment"/>
      /// <seealso cref="WritingOptions_PE.FileAlignment"/>
      public Int32 FileAlignment { get; }

      /// <summary>
      /// Gets the section alignment of the image.
      /// </summary>
      /// <value>The section alignment of the image.</value>
      /// <seealso cref="OptionalHeader.SectionAlignment"/>
      /// <seealso cref="WritingOptions_PE.SectionAlignment"/>
      public Int32 SectionAlignment { get; }

      /// <summary>
      /// Gets the image base address.
      /// </summary>
      /// <value>The image base address.</value>
      /// <seealso cref="OptionalHeader.ImageBase"/>
      /// <seealso cref="WritingOptions_PE.ImageBase"/>
      public Int64 ImageBase { get; }

      /// <summary>
      /// Gets the <see cref="IO.StrongNameInformation"/> constructed by <see cref="E_CILPhysical.WriteMetaDataToStream(WriterFunctionality, Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/> method.
      /// </summary>
      /// <value>The <see cref="IO.StrongNameInformation"/> constructed by <see cref="E_CILPhysical.WriteMetaDataToStream(WriterFunctionality, Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/> method.</value>
      public StrongNameInformation StrongNameInformation { get; }

      /// <summary>
      /// Gets the data directories of the <see cref="OptionalHeader"/>.
      /// </summary>
      /// <value>The data directories of the <see cref="OptionalHeader"/>.</value>
      /// <remarks>
      /// The elements of this property should be modified during writing process by <see cref="WriterFunctionality"/> or the objects it creates.
      /// </remarks>
      /// <seealso cref="OptionalHeader.DataDirectories"/>
      /// <seealso cref="WritingOptions_PE.NumberOfDataDirectories"/>
      public DataDirectory[] PEDataDirectories { get; }

      /// <summary>
      /// Gets the <see cref="SectionHeader"/>s.
      /// </summary>
      /// <value>The <see cref="SectionHeader"/>s.</value>
      /// <remarks>
      /// The elements of this property should be modified during writing process by <see cref="WriterFunctionality"/> or the objects it creates.
      /// </remarks>
      /// <seealso cref="FileHeader.NumberOfSections"/>
      public SectionHeader[] SectionHeaders { get; }

      /// <summary>
      /// Gets or sets the optional entry point RVA.
      /// </summary>
      /// <value>The optional entry point RVA.</value>
      /// <remarks>
      /// This property should be modified, if necessary, during writing process by <see cref="WriterFunctionality"/> or the objects it creates.
      /// </remarks>
      /// <seealso cref="OptionalHeader.EntryPointRVA"/>
      public Int32? EntryPointRVA { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="IO.DebugInformation"/> to write.
      /// </summary>
      /// <value>The <see cref="IO.DebugInformation"/> to write.</value>
      /// <remarks>
      /// This property should be modified, if necessary, during writing process by <see cref="WriterFunctionality"/> or the objects it creates.
      /// </remarks>
      /// <seealso cref="ImageInformation.DebugInformation"/>
      public DebugInformation DebugInformation { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="IO.CLIHeader"/> to write.
      /// </summary>
      /// <value>The <see cref="IO.CLIHeader"/> to write.</value>
      /// <remarks>
      /// This property should be modified during writing process by <see cref="WriterFunctionality"/> or the objects it creates.
      /// The <see cref="WriterFunctionality.BeforeMetaData"/> method is the last chance to set this property.
      /// See <see cref="WriterFunctionality"/> description for information in which order the methods are invoked.
      /// </remarks>
      public CLIHeader CLIHeader { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="MetaDataRoot"/> to write.
      /// </summary>
      /// <value>The <see cref="MetaDataRoot"/> to write.</value>
      /// <remarks>
      /// This property should be modified during writing process by <see cref="WriterFunctionality"/> or the objects it creates.
      /// The <see cref="WriterFunctionality.AfterMetaData"/> method is the last chance to set this property.
      /// See <see cref="WriterFunctionality"/> description for information in which order the methods are invoked.
      /// </remarks>
      public MetaDataRoot MDRoot { get; set; }

      private static Int32 CheckAlignment( Int32 alignment, Int32 defaultAlignment )
      {
         // TODO reset all bits following MSB set bit in alignment
         return alignment == 0 ? defaultAlignment : alignment;
      }
   }

   /// <summary>
   /// This class contains the required information for emitting strong-name signed image, without access to private key data.
   /// </summary>
   public class StrongNameInformation
   {
      /// <summary>
      /// Creates a new instance of <see cref="StrongNameInformation"/> with given parameters.
      /// </summary>
      /// <param name="hashAlgorithm">The <see cref="AssemblyHashAlgorithm"/> describing the algorithm to use in strong name signature computation.</param>
      /// <param name="signatureSize">The size of the strong name signature.</param>
      /// <param name="publicKey">The public key of the strong name.</param>
      public StrongNameInformation(
         AssemblyHashAlgorithm hashAlgorithm,
         Int32 signatureSize,
         IEnumerable<Byte> publicKey
         )
      {
         this.HashAlgorithm = hashAlgorithm;
         this.SignatureSize = signatureSize;
         this.PublicKey = publicKey.ToArrayProxy().CQ;
      }
      /// <summary>
      /// Gets the size of the strong-name signature.
      /// </summary>
      /// <value></value>
      /// <remarks>This should be the size of the <see cref="CLIHeader.StrongNameSignature"/> data directory.</remarks>
      public Int32 SignatureSize { get; }

      /// <summary>
      /// Gets the <see cref="AssemblyHashAlgorithm"/> describing the algorithm to use when calcualting strong name signature.
      /// </summary>
      /// <value>The <see cref="AssemblyHashAlgorithm"/> describing the algorithm to use when calcualting strong name signature.</value>
      public AssemblyHashAlgorithm HashAlgorithm { get; }

      /// <summary>
      /// Gets the public key of this strong name.
      /// </summary>
      /// <value>The public key of this strong name.</value>
      public ArrayQuery<Byte> PublicKey { get; }

   }
}