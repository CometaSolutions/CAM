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
using CILAssemblyManipulator.Physical.IO.Defaults;
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
      ColumnValueStorage<Int64> CalculateImageLayout(
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
         ColumnValueStorage<Int64> dataReferences
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


public static partial class E_CILPhysical
{
   private const Int32 DATA_DIR_SIZE = 0x08;

   /// <summary>
   /// This is extension method to write given <see cref="CILMetaData"/> to <see cref="Stream"/> using this <see cref="WriterFunctionalityProvider"/>.
   /// It takes into account the possible new stream, and the possible new meta data created by <see cref="WriterFunctionalityProvider.GetFunctionality"/> method.
   /// </summary>
   /// <param name="writerProvider">This <see cref="WriterFunctionalityProvider"/>.</param>
   /// <param name="stream">The <see cref="Stream"/> to write <see cref="CILMetaData"/> to.</param>
   /// <param name="md">The <see cref="CILMetaData"/> to write.</param>
   /// <param name="options">The <see cref="WritingOptions"/> object to control header values. May be <c>null</c>.</param>
   /// <param name="sn">The <see cref="StrongNameKeyPair"/> to use, if the <see cref="CILMetaData"/> should be strong-name signed.</param>
   /// <param name="delaySign">Setting this to <c>true</c> will leave the room for strong-name signature, but will not compute it.</param>
   /// <param name="cryptoCallbacks">The <see cref="CryptoCallbacks"/> to use, if the <see cref="CILMetaData"/> should be strong-name signed.</param>
   /// <param name="snAlgorithmOverride">Overrides the signing algorithm, which originally is deduceable from the key BLOB of the <see cref="StrongNameKeyPair"/>.</param>
   /// <param name="errorHandler">The callback to handle errors during serialization.</param>
   /// <returns>An instance of <see cref="ImageInformation"/> containing information about the headers and other values created during serialization.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="stream"/> or <paramref name="md"/> is <c>null</c>.</exception>
   /// <exception cref="NullReferenceException">If this <see cref="WriterFunctionalityProvider"/> is <c>null</c>.</exception>
   /// <remarks>
   /// This method is used by <see cref="E_CILPhysical.WriteModule"/> to actually perform serialization, and thus is rarely needed to be used directly.
   /// Instead, use <see cref="E_CILPhysical.WriteModule"/>.
   /// </remarks>
   /// <seealso cref="WriterFunctionalityProvider"/>
   /// <seealso cref="E_CILPhysical.WriteModule"/>
   /// <seealso cref="WriteMetaDataToStream(WriterFunctionality, Stream, CILMetaData, WritingOptions, StrongNameKeyPair, bool, CryptoCallbacks, AssemblyHashAlgorithm?, EventHandler{SerializationErrorEventArgs})"/>
   public static ImageInformation WriteMetaDataToStream(
      this WriterFunctionalityProvider writerProvider,
      Stream stream,
      CILMetaData md,
      WritingOptions options,
      StrongNameKeyPair sn,
      Boolean delaySign,
      CryptoCallbacks cryptoCallbacks,
      AssemblyHashAlgorithm? snAlgorithmOverride,
      EventHandler<SerializationErrorEventArgs> errorHandler
      )
   {
      if ( writerProvider == null )
      {
         throw new NullReferenceException();
      }

      ArgumentValidator.ValidateNotNull( "Stream", stream );
      ArgumentValidator.ValidateNotNull( "Meta data", md );

      if ( options == null )
      {
         options = new WritingOptions();
      }

      CILMetaData newMD; Stream newStream;
      var writer = writerProvider.GetFunctionality( md, options, errorHandler, out newMD, out newStream ) ?? new DefaultWriterFunctionality( md, options, new TableSerializationInfoCreationArgs( errorHandler ) );
      if ( newMD != null )
      {
         md = newMD;
      }

      ImageInformation retVal;
      if ( newStream == null )
      {
         retVal = writer.WriteMetaDataToStream( stream, md, options, sn, delaySign, cryptoCallbacks, snAlgorithmOverride, errorHandler );
      }
      else
      {
         using ( newStream )
         {
            retVal = writer.WriteMetaDataToStream( stream, md, options, sn, delaySign, cryptoCallbacks, snAlgorithmOverride, errorHandler );
            newStream.Position = 0;
            newStream.CopyTo( stream );
         }
      }

      return retVal;
   }

   /// <summary>
   /// This is extension method to write given <see cref="CILMetaData"/> to <see cref="Stream"/> using this <see cref="WriterFunctionality"/>.
   /// </summary>
   /// <param name="writer">This <see cref="WriterFunctionality"/>.</param>
   /// <param name="stream">The <see cref="Stream"/> to write <see cref="CILMetaData"/> to.</param>
   /// <param name="md">The <see cref="CILMetaData"/> to write.</param>
   /// <param name="options">The <see cref="WritingOptions"/> object to control header values. May be <c>null</c>.</param>
   /// <param name="sn">The <see cref="StrongNameKeyPair"/> to use, if the <see cref="CILMetaData"/> should be strong-name signed.</param>
   /// <param name="delaySign">Setting this to <c>true</c> will leave the room for strong-name signature, but will not compute it.</param>
   /// <param name="cryptoCallbacks">The <see cref="CryptoCallbacks"/> to use, if the <see cref="CILMetaData"/> should be strong-name signed.</param>
   /// <param name="snAlgorithmOverride">Overrides the signing algorithm, which originally is deduceable from the key BLOB of the <see cref="StrongNameKeyPair"/>.</param>
   /// <param name="errorHandler">The callback to handle errors during serialization.</param>
   /// <returns>An instance of <see cref="ImageInformation"/> containing information about the headers and other values created during serialization.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="stream"/> or <paramref name="md"/> is <c>null</c>.</exception>
   /// <exception cref="NullReferenceException">If this <see cref="WriterFunctionality"/> is <c>null</c>.</exception>
   /// <exception cref="InvalidOperationException">If this <see cref="WriterFunctionality"/> returns invalid values.</exception>
   /// <remarks>
   /// This method is used by <see cref="E_CILPhysical.WriteModule"/> to actually perform serialization, and thus is rarely needed to be used directly.
   /// Instead, use <see cref="E_CILPhysical.WriteModule"/>.
   /// </remarks>
   /// <seealso cref="WriterFunctionality"/>
   /// <seealso cref="E_CILPhysical.WriteModule"/>
   public static ImageInformation WriteMetaDataToStream(
      this WriterFunctionality writer,
      Stream stream,
      CILMetaData md,
      WritingOptions options,
      StrongNameKeyPair sn,
      Boolean delaySign,
      CryptoCallbacks cryptoCallbacks,
      AssemblyHashAlgorithm? snAlgorithmOverride,
      EventHandler<SerializationErrorEventArgs> errorHandler
      )
   {
      // Check arguments
      if ( writer == null )
      {
         throw new NullReferenceException();
         //writer = new DefaultWriterFunctionality( md, options, new TableSerializationInfoCreationArgs( errorHandler ) );
      }

      ArgumentValidator.ValidateNotNull( "Stream", stream );
      ArgumentValidator.ValidateNotNull( "Meta data", md );
      if ( options == null )
      {
         options = new WritingOptions();
      }

      // 1. Create WritingStatus
      // Prepare strong name
      RSAParameters rParams; String snContainerName;
      var status = writer.CreateWritingStatus( md.PrepareStrongNameVariables( sn, ref delaySign, cryptoCallbacks, snAlgorithmOverride, out rParams, out snContainerName ) );
      if ( status == null )
      {
         throw new InvalidOperationException( "Writer failed to create writing status object." );
      }
      var snVars = status.StrongNameInformation;

      // 2. Create streams
      var mdStreams = writer.CreateMetaDataStreamHandlers().ToArrayProxy().CQ;
      var tblMDStream = mdStreams
         .OfType<WriterTableStreamHandler>()
         .FirstOrDefault() ?? new DefaultWriterTableStreamHandler( md, options.CLIOptions.TablesStreamOptions, DefaultMetaDataSerializationSupportProvider.Instance.CreateTableSerializationInfos( md, new TableSerializationInfoCreationArgs( errorHandler ) ).ToArrayProxy().CQ );

      var blobStream = mdStreams.OfType<WriterBLOBStreamHandler>().FirstOrDefault();
      var guidStream = mdStreams.OfType<WriterGUIDStreamHandler>().FirstOrDefault();
      var sysStringStream = mdStreams.OfType<WriterStringStreamHandler>().FirstOrDefault( s => String.Equals( s.StreamName, MetaDataConstants.SYS_STRING_STREAM_NAME ) );
      var userStringStream = mdStreams.OfType<WriterStringStreamHandler>().FirstOrDefault( s => String.Equals( s.StreamName, MetaDataConstants.USER_STRING_STREAM_NAME ) );
      var mdStreamContainer = new WriterMetaDataStreamContainer(
            blobStream,
            guidStream,
            sysStringStream,
            userStringStream,
            mdStreams.Where( s => !ReferenceEquals( tblMDStream, s ) && !ReferenceEquals( blobStream, s ) && !ReferenceEquals( guidStream, s ) && !ReferenceEquals( sysStringStream, s ) && !ReferenceEquals( userStringStream, s ) )
            );

      // 3. Populate streams
      var array = new ResizableArray<Byte>( initialSize: 0x1000 );
      var thHeader = tblMDStream.FillOtherMDStreams( snVars?.PublicKey?.ToArrayProxy()?.CQ, mdStreamContainer, array );
      if ( thHeader == null )
      {
         throw new InvalidOperationException( "Writer failed to create meta data table header." );
      }

      // 4. Create sections and some headers
      RVAConverter rvaConverter; Int32 mdRootSize;
      var rawValueProvider = writer.CalculateImageLayout(
         status,
         mdStreamContainer,
         mdStreams,
         out rvaConverter,
         out mdRootSize
         );

      if ( rvaConverter == null )
      {
         rvaConverter = new DefaultRVAConverter( status.SectionHeaders );
      }

      // 5. Position stream after headers, and write whatever is needed before meta data
      var headersSize = status.GetAlignedHeadersSize();
      stream.Position = headersSize;
      writer.BeforeMetaData( status, stream, array );

      var cliHeader = status.CLIHeader;
      if ( cliHeader == null )
      {
         throw new InvalidOperationException( "Writer failed to create CLI header." );
      }

      // 6. Write meta data
      stream.SeekFromBegin( rvaConverter.ToOffset( (UInt32) cliHeader.MetaData.RVA ) );
      array.CurrentMaxCapacity = mdRootSize;
      writer.WriteMDRoot( status, array );
      stream.Write( array.Array, mdRootSize );
      foreach ( var mds in mdStreams.Where( mds => mds.Accessed ) )
      {
         mds.WriteStream( stream, array, rawValueProvider );
      }

      // 7. Write whatever is needed after meta data
      writer.AfterMetaData( status, stream, array );

      // 8. Create and write image information
      var cliOptions = options.CLIOptions;
      var snSignature = snVars == null ? null : new Byte[snVars.SignatureSize];
      var cliHeaderOptions = cliOptions.HeaderOptions;
      var thOptions = cliOptions.TablesStreamOptions;
      var machine = status.Machine;
      var peOptions = options.PEOptions;
      var optionalHeaderKind = machine.GetOptionalHeaderKind();
      var optionalHeaderSize = optionalHeaderKind.GetOptionalHeaderSize( status.PEDataDirectories.Length );
      var sections = status.SectionHeaders.ToArrayProxy().CQ;
      var imageInfo = new ImageInformation(
         new PEInformation(
            new DOSHeader( 0x5A4D, 0x00000080u ),
            new NTHeader( 0x00004550,
               new FileHeader(
                  machine, // Machine
                  (UInt16) sections.Count, // Number of sections
                  (UInt32) ( peOptions.Timestamp ?? CreateNewPETimestamp() ), // Timestamp
                  0, // Pointer to symbol table
                  0, // Number of symbols
                  optionalHeaderSize,
                  ( peOptions.Characteristics ?? machine.GetDefaultCharacteristics() ).ProcessCharacteristics( options.IsExecutable )
                  ),
               peOptions.CreateOptionalHeader(
                  status,
                  sections,
                  (UInt32) headersSize,
                  optionalHeaderKind
                  )
               ),
            sections
            ),
         status.DebugInformation,
         new CLIInformation(
            cliHeader,
            status.MDRoot,
            thHeader,
            snSignature?.ToArrayProxy()?.CQ,
            rawValueProvider.GetDataReferenceInfos( i => i )
            )
         );


      writer.WritePEInformation( status, stream, array, imageInfo.PEInformation );

      // 9. Compute strong name signature, if needed
      CreateStrongNameSignature(
         stream,
         snVars,
         delaySign,
         cryptoCallbacks,
         rParams,
         snContainerName,
         cliHeader,
         rvaConverter,
         snSignature,
         imageInfo.PEInformation,
         status.HeadersSizeUnaligned
         );

      return imageInfo;
   }

   private static StrongNameInformation PrepareStrongNameVariables(
      this CILMetaData md,
      StrongNameKeyPair strongName,
      ref Boolean delaySign,
      CryptoCallbacks cryptoCallbacks,
      AssemblyHashAlgorithm? algoOverride,
      out RSAParameters rParams,
      out String containerName
      )
   {
      var useStrongName = strongName != null;
      var snSize = 0;
      var aDefs = md.AssemblyDefinitions.TableContents;
      var thisAssemblyPublicKey = aDefs.Count > 0 ?
         aDefs[0].AssemblyInformation.PublicKeyOrToken.CreateArrayCopy() :
         null;

      if ( !delaySign )
      {
         delaySign = !useStrongName && !thisAssemblyPublicKey.IsNullOrEmpty();
      }
      var signingAlgorithm = AssemblyHashAlgorithm.SHA1;
      var computingHash = useStrongName || delaySign;

      if ( useStrongName && cryptoCallbacks == null )
      {
#if CAM_PHYSICAL_IS_PORTABLE
         throw new ArgumentException( "Assembly strong name was provided, but the crypto callbacks were not." );
#else
         cryptoCallbacks = new CryptoCallbacksDotNET();
#endif
      }

      StrongNameInformation retVal;

      if ( computingHash )
      {
         //// Set appropriate module flags
         //headers.ModuleFlags |= ModuleFlags.StrongNameSigned;

         // Check algorithm override
         var algoOverrideWasInvalid = algoOverride.HasValue && ( algoOverride.Value == AssemblyHashAlgorithm.MD5 || algoOverride.Value == AssemblyHashAlgorithm.None );
         if ( algoOverrideWasInvalid )
         {
            algoOverride = AssemblyHashAlgorithm.SHA1;
         }

         Byte[] pkToProcess;
         containerName = strongName?.ContainerName;
         if ( ( useStrongName && containerName != null ) || ( !useStrongName && delaySign ) )
         {
            if ( thisAssemblyPublicKey.IsNullOrEmpty() )
            {
               thisAssemblyPublicKey = cryptoCallbacks.ExtractPublicKeyFromCSPContainerAndCheck( containerName );
            }
            pkToProcess = thisAssemblyPublicKey;
         }
         else
         {
            // Get public key from BLOB
            pkToProcess = strongName.KeyPair.ToArray();
         }

         // Create RSA parameters and process public key so that it will have proper, full format.
         Byte[] pk; String errorString;
         if ( CryptoUtils.TryCreateSigningInformationFromKeyBLOB( pkToProcess, algoOverride, out pk, out signingAlgorithm, out rParams, out errorString ) )
         {
            thisAssemblyPublicKey = pk;
            snSize = rParams.Modulus.Length;
         }
         else if ( thisAssemblyPublicKey != null && thisAssemblyPublicKey.Length == 16 ) // The "Standard Public Key", ECMA-335 p. 116
         {
            // TODO throw instead (but some tests will fail then...)
            snSize = 0x100;
         }
         else
         {
            throw new CryptographicException( errorString );
         }

         retVal = new StrongNameInformation(
            signingAlgorithm,
            snSize,
            thisAssemblyPublicKey
            );
      }
      else
      {
         retVal = null;
         rParams = default( RSAParameters );
         containerName = null;
      }

      return retVal;
   }

   private static void CreateStrongNameSignature(
      Stream stream,
      StrongNameInformation snVars,
      Boolean delaySign,
      CryptoCallbacks cryptoCallbacks,
      RSAParameters rParams,
      String containerName,
      CLIHeader cliHeader,
      RVAConverter rvaConverter,
      Byte[] snSignatureArray,
      PEInformation imageInfo,
      Int32 headersSizeUnaligned
      )
   {
      if ( snVars != null && !delaySign )
      {
         using ( var rsa = ( containerName == null ? cryptoCallbacks.CreateRSAFromParameters( rParams ) : cryptoCallbacks.CreateRSAFromCSPContainer( containerName ) ) )
         {
            var algo = snVars.HashAlgorithm;
            var snSize = snVars.SignatureSize;
            var buffer = new Byte[0x8000];
            var hashEvtArgs = cryptoCallbacks.CreateHashStreamAndCheck( algo, true, true, false, true );
            var hashStream = hashEvtArgs.CryptoStream;
            var hashGetter = hashEvtArgs.HashGetter;
            var transform = hashEvtArgs.Transform;
            var sigOffset = rvaConverter.ToOffset( cliHeader.StrongNameSignature.RVA );
            Int32 idx;

            Byte[] strongNameArray;
            using ( var tf = transform )
            {
               using ( var cryptoStream = hashStream() )
               {
                  // TODO: WriterFunctionality should have method:
                  // IEnumerable<Tuple<Int64, Int64>> GetRangesSkippedInStrongNameSignatureCalculation(ImageInformation imageInfo);

                  // Calculate hash of required parts of file (ECMA-335, p.117)
                  // Read all headers first DOS header (start of file to the NT headers)
                  stream.SeekFromBegin( 0 );
                  var hdrArray = new Byte[headersSizeUnaligned];
                  stream.ReadSpecificAmount( hdrArray, 0, hdrArray.Length );

                  // Hash the checksum entry + authenticode as zeroes
                  const Int32 peCheckSumOffsetWithinOptionalHeader = 0x40;
                  var ntHeaderStart = (Int32) imageInfo.DOSHeader.NTHeaderOffset;
                  idx = ntHeaderStart
                     + DefaultWriterFunctionality.PE_SIG_AND_FILE_HEADER_SIZE // NT header signature + file header size
                     + peCheckSumOffsetWithinOptionalHeader; // Offset of PE checksum entry.
                  hdrArray.WriteInt32LEToBytes( ref idx, 0 );

                  var optionalHeaderSizeWithoutDataDirs = imageInfo.NTHeader.FileHeader.OptionalHeaderSize - DATA_DIR_SIZE * imageInfo.NTHeader.OptionalHeader.DataDirectories.Count;

                  idx = ntHeaderStart
                     + DefaultWriterFunctionality.PE_SIG_AND_FILE_HEADER_SIZE // NT header signature + file header size
                     + optionalHeaderSizeWithoutDataDirs
                     + 4 * DATA_DIR_SIZE; // Authenticode is 5th data directory, and optionalHeaderSize includes all data directories
                  hdrArray.WriteDataDirectory( ref idx, default( DataDirectory ) );
                  // Hash the correctly zeroed-out header data
                  cryptoStream.Write( hdrArray );

                  // Now, calculate hash for all sections, except we have to skip our own strong name signature hash part
                  foreach ( var section in imageInfo.SectionHeaders )
                  {
                     var min = section.RawDataPointer;
                     var max = min + section.RawDataSize;
                     stream.SeekFromBegin( min );
                     if ( min <= sigOffset && max >= sigOffset )
                     {
                        // Strong name signature is in this section
                        stream.CopyStreamPart( cryptoStream, buffer, sigOffset - min );
                        stream.SeekFromCurrent( snSize );
                        stream.CopyStreamPart( cryptoStream, buffer, max - sigOffset - snSize );
                     }
                     else
                     {
                        stream.CopyStreamPart( cryptoStream, buffer, max - min );
                     }
                  }
               }

               strongNameArray = cryptoCallbacks.CreateRSASignatureAndCheck( rsa, algo.GetAlgorithmName(), hashGetter() );
            }


            if ( snSize != strongNameArray.Length )
            {
               throw new CryptographicException( "Calculated and actual strong name size differ (calculated: " + snSize + ", actual: " + strongNameArray.Length + ")." );
            }
            Array.Reverse( strongNameArray );

            // Write strong name
            stream.Seek( sigOffset, SeekOrigin.Begin );
            stream.Write( strongNameArray );
            idx = 0;
            snSignatureArray.BlockCopyFrom( ref idx, strongNameArray );
         }
      }
   }

   private static Int32 CreateNewPETimestamp()
   {
      return (Int32) ( DateTime.UtcNow - new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc ) ).TotalSeconds;
   }

   internal static UInt16 GetOptionalHeaderSize( this OptionalHeaderKind kind, Int32 peDataDirectoriesCount )
   {
      return (UInt16) ( ( kind == OptionalHeaderKind.Optional64 ? 0x70 : 0x60 ) + DATA_DIR_SIZE * peDataDirectoriesCount );
   }

   private static FileHeaderCharacteristics ProcessCharacteristics(
      this FileHeaderCharacteristics characteristics,
      Boolean isExecutable
      )
   {
      return isExecutable ?
         ( characteristics & ~FileHeaderCharacteristics.Dll ) :
         ( characteristics | FileHeaderCharacteristics.Dll );
   }

   private static OptionalHeader CreateOptionalHeader(
      this WritingOptions_PE options,
      WritingStatus writingStatus,
      ArrayQuery<SectionHeader> sections,
      UInt32 headersSize,
      OptionalHeaderKind kind
      )
   {
      const Byte linkerMajor = 0x0B;
      const Byte linkerMinor = 0x00;
      const Int16 osMajor = 0x04;
      const Int16 osMinor = 0x00;
      const Int16 userMajor = 0x0000;
      const Int16 userMinor = 0x0000;
      const Int16 subsystemMajor = 0x0004;
      const Int16 subsystemMinor = 0x0000;
      const Subsystem subsystem = Subsystem.WindowsConsole;
      const DLLFlags dllFlags = DLLFlags.DynamicBase | DLLFlags.NXCompatible | DLLFlags.NoSEH | DLLFlags.TerminalServerAware;

      // Calculate various sizes in one iteration of sections
      var sAlign = (UInt32) writingStatus.SectionAlignment;
      var fAlign = (UInt32) writingStatus.FileAlignment;
      var imageSize = headersSize.RoundUpU32( sAlign );
      var dataBase = 0u;
      var codeBase = 0u;
      var codeSize = 0u;
      var initDataSize = 0u;
      var uninitDataSize = 0u;
      foreach ( var section in sections )
      {
         var chars = section.Characteristics;
         var curSize = section.RawDataSize;

         if ( chars.HasFlag( SectionHeaderCharacteristics.Contains_Code ) )
         {
            if ( codeBase == 0u )
            {
               codeBase = imageSize;
            }
            codeSize += curSize;
         }
         if ( chars.HasFlag( SectionHeaderCharacteristics.Contains_InitializedData ) )
         {
            if ( dataBase == 0u )
            {
               dataBase = imageSize;
            }
            initDataSize += curSize;
         }
         if ( chars.HasFlag( SectionHeaderCharacteristics.Contains_UninitializedData ) )
         {
            if ( dataBase == 0u )
            {
               dataBase = imageSize;
            }
            uninitDataSize += curSize;
         }

         imageSize += curSize.RoundUpU32( sAlign );
      }

      var ep = (UInt32) writingStatus.EntryPointRVA.GetValueOrDefault();
      var imageBase = writingStatus.ImageBase;

      switch ( kind )
      {
         case OptionalHeaderKind.Optional32:
            return new OptionalHeader32(
               options.MajorLinkerVersion ?? linkerMajor,
               options.MinorLinkerVersion ?? linkerMinor,
               codeSize,
               initDataSize,
               uninitDataSize,
               ep,
               codeBase,
               dataBase,
               (UInt32) imageBase,
               sAlign,
               fAlign,
               (UInt16) ( options.MajorOSVersion ?? osMajor ),
               (UInt16) ( options.MinorOSVersion ?? osMinor ),
               (UInt16) ( options.MajorUserVersion ?? userMajor ),
               (UInt16) ( options.MinorUserVersion ?? userMinor ),
               (UInt16) ( options.MajorSubsystemVersion ?? subsystemMajor ),
               (UInt16) ( options.MinorSubsystemVersion ?? subsystemMinor ),
               (UInt32) ( options.Win32VersionValue ?? 0x00000000 ),
               imageSize,
               headersSize,
               0x00000000,
               options.Subsystem ?? subsystem,
               options.DLLCharacteristics ?? dllFlags,
               (UInt32) ( options.StackReserveSize ?? 0x00100000 ),
               (UInt32) ( options.StackCommitSize ?? 0x00001000 ),
               (UInt32) ( options.HeapReserveSize ?? 0x00100000 ),
               (UInt32) ( options.HeapCommitSize ?? 0x00001000 ),
               options.LoaderFlags ?? 0x00000000,
               (UInt32) ( options.NumberOfDataDirectories ?? (Int32) DataDirectories.MaxValue ),
               writingStatus.PEDataDirectories.ToArrayProxy().CQ
               );
         case OptionalHeaderKind.Optional64:
            return new OptionalHeader64(
               options.MajorLinkerVersion ?? linkerMajor,
               options.MinorLinkerVersion ?? linkerMinor,
               codeSize,
               initDataSize,
               uninitDataSize,
               ep,
               codeBase,
               (UInt64) imageBase,
               sAlign,
               fAlign,
               (UInt16) ( options.MajorOSVersion ?? osMajor ),
               (UInt16) ( options.MinorOSVersion ?? osMinor ),
               (UInt16) ( options.MajorUserVersion ?? userMajor ),
               (UInt16) ( options.MinorUserVersion ?? userMinor ),
               (UInt16) ( options.MajorSubsystemVersion ?? subsystemMajor ),
               (UInt16) ( options.MinorSubsystemVersion ?? subsystemMinor ),
               (UInt32) ( options.Win32VersionValue ?? 0x00000000 ),
               imageSize,
               headersSize,
               0x00000000,
               options.Subsystem ?? subsystem,
               options.DLLCharacteristics ?? dllFlags,
               (UInt64) ( options.StackReserveSize ?? 0x0000000000400000 ),
               (UInt64) ( options.StackCommitSize ?? 0x0000000000004000 ),
               (UInt64) ( options.HeapReserveSize ?? 0x0000000000100000 ),
               (UInt64) ( options.HeapCommitSize ?? 0x0000000000002000 ),
               options.LoaderFlags ?? 0x00000000,
               (UInt32) ( options.NumberOfDataDirectories ?? (Int32) DataDirectories.MaxValue ),
               writingStatus.PEDataDirectories.ToArrayProxy().CQ
               );
         default:
            throw new ArgumentException( "Unsupported optional header kind: " + kind + "." );
      }

   }

   internal static Int32 SetCapacityAndAlign( this ResizableArray<Byte> array, Int64 streamPosition, Int32 dataSize, Int32 dataAlignmnet )
   {
      var paddingBefore = (Int32) ( streamPosition.RoundUpI64( dataAlignmnet ) - streamPosition );
      array.CurrentMaxCapacity = paddingBefore + dataSize;
      return paddingBefore;
   }

   internal static void SkipAlignedData( this Stream stream, Int32 dataSize, Int32 dataAlignment )
   {
      stream.Position = stream.Position.RoundUpI64( dataAlignment ) + dataSize;
   }

   /// <summary>
   /// Creates an <see cref="IEnumerable{T}"/> to enumerate all the streams of this <see cref="MetaDataStreamContainer{TAbstractStream, TBLOBStream, TGUIDStream, TStringStream}"/>.
   /// </summary>
   /// <typeparam name="TAbstractStream">The type of the abstract meta data stream.</typeparam>
   /// <typeparam name="TBLOBStream">The type of the BLOB meta data stream.</typeparam>
   /// <typeparam name="TGUIDStream">The type of the GUID meta data stream.</typeparam>
   /// <typeparam name="TStringStream">The type of the various string meta data streams.</typeparam>
   /// <param name="mdStreams">This <see cref="MetaDataStreamContainer{TAbstractStream, TBLOBStream, TGUIDStream, TStringStream}"/>.</param>
   /// <returns>An enumerable to enumerate all of the streams of this <see cref="MetaDataStreamContainer{TAbstractStream, TBLOBStream, TGUIDStream, TStringStream}"/>.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="MetaDataStreamContainer{TAbstractStream, TBLOBStream, TGUIDStream, TStringStream}"/> is <c>null</c>.</exception>
   public static IEnumerable<TAbstractStream> GetAllStreams<TAbstractStream, TBLOBStream, TGUIDStream, TStringStream>( this MetaDataStreamContainer<TAbstractStream, TBLOBStream, TGUIDStream, TStringStream> mdStreams )
      where TAbstractStream : AbstractMetaDataStreamHandler
      where TBLOBStream : TAbstractStream
      where TGUIDStream : TAbstractStream
      where TStringStream : TAbstractStream
   {
      yield return mdStreams.BLOBs;
      yield return mdStreams.GUIDs;
      yield return mdStreams.SystemStrings;
      yield return mdStreams.UserStrings;
      foreach ( var os in mdStreams.OtherStreams )
      {
         yield return os;
      }
   }

   /// <summary>
   /// This is helper method to get the aligned size of the DOS and NT headers of the PE image.
   /// </summary>
   /// <param name="status">This <see cref="WritingStatus"/>.</param>
   /// <returns>The aligned size of the DOS and NT headers, which will be <see cref="WritingStatus.HeadersSizeUnaligned"/> aligned to <see cref="WritingStatus.FileAlignment"/>.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="WritingStatus"/> is <c>null</c>.</exception>
   public static Int32 GetAlignedHeadersSize( this WritingStatus status )
   {
      return status.HeadersSizeUnaligned.RoundUpI32( status.FileAlignment );
   }
}