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
using CILAssemblyManipulator.Physical.IO;
using CollectionsWithRoles.API;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO
{
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
   /// This class provides default implementaiton for <see cref="RVAConverter"/>.
   /// </summary>
   /// <remarks>
   /// Currently, this class does not use interval tree, so it is not the most efficient implementation.
   /// </remarks>
   public class DefaultRVAConverter : RVAConverter
   {
      private readonly SectionHeader[] _sections;

      /// <summary>
      /// Creates a new instance of <see cref="DefaultRVAConverter"/> with given section headers.
      /// </summary>
      /// <param name="headers">All present <see cref="SectionHeader"/>s. If <c>null</c>, empty enumerable will be used.</param>
      public DefaultRVAConverter( IEnumerable<SectionHeader> headers )
      {
         this._sections = ( headers ?? Empty<SectionHeader>.Enumerable ).ToArray();
      }

      /// <inheritdoc />
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

      /// <inheritdoc />
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
   /// This is common interface for <see cref="AbstractReaderStreamHandler"/> and <see cref="AbstractWriterStreamHandler"/>.
   /// It contains elements common for meta data streams in both serialization and deserialization processes.
   /// </summary>
   /// <seealso cref="AbstractReaderStreamHandler"/>
   /// <seealso cref="AbstractWriterStreamHandler"/>
   public interface AbstractMetaDataStreamHandler
   {
      /// <summary>
      /// Gets the textual name of this <see cref="AbstractMetaDataStreamHandler"/>.
      /// </summary>
      /// <value>The textual name of this <see cref="AbstractMetaDataStreamHandler"/>.</value>
      String StreamName { get; }

      /// <summary>
      /// Gets the size of this <see cref="AbstractMetaDataStreamHandler"/> in bytes.
      /// </summary>
      /// <value>The size of this <see cref="AbstractMetaDataStreamHandler"/> in bytes.</value>
      Int32 StreamSize { get; }
   }

   /// <summary>
   /// This exception is thrown when the functionality (<see cref="WriterFunctionalityProvider"/>, <see cref="WriterFunctionality"/>, <see cref="ReaderFunctionalityProvider"/>, or <see cref="ReaderFunctionality"/>) in (de)serialization process violates some contract.
   /// </summary>
   public class SerializationFunctionalityException : Exception
   {
      /// <summary>
      /// Creates a new instance of <see cref="SerializationFunctionalityException"/> with given message and inner exception.
      /// </summary>
      /// <param name="msg">The textual message.</param>
      /// <param name="inner">The optional inner exception.</param>
      public SerializationFunctionalityException( String msg, Exception inner = null )
         : base( msg, inner )
      {

      }

      /// <summary>
      /// Creates a new instance of <see cref="SerializationFunctionalityException"/> with pre-built message about failure in serialization (writing) process.
      /// </summary>
      /// <param name="whatFailedToProvide">What the writer functionality (provider) failed to provide.</param>
      /// <param name="isProvider"><c>true</c> if the object in question was <see cref="WriterFunctionalityProvider"/>; <c>false</c> if it was <see cref="WriterFunctionality"/>.</param>
      /// <returns>A new instance of <see cref="SerializationFunctionalityException"/> with pre-built message.</returns>
      public static SerializationFunctionalityException ExceptionDuringSerialization( String whatFailedToProvide, Boolean isProvider )
      {
         return ExceptionDuringProcess( "Writer", whatFailedToProvide, isProvider );
      }


      /// <summary>
      /// Creates a new instance of <see cref="SerializationFunctionalityException"/> with pre-built message about failure in deserialization (reading) process.
      /// </summary>
      /// <param name="whatFailedToProvide">What the reader functionality (provider) failed to provide.</param>
      /// <param name="isProvider"><c>true</c> if the object in question was <see cref="ReaderFunctionalityProvider"/>; <c>false</c> if it was <see cref="ReaderFunctionality"/>.</param>
      /// <returns>A new instance of <see cref="SerializationFunctionalityException"/> with pre-built message.</returns>
      public static SerializationFunctionalityException ExceptionDuringDeserialization( String whatFailedToProvide, Boolean isProvider )
      {
         return ExceptionDuringProcess( "Reader", whatFailedToProvide, isProvider );
      }

      /// <summary>
      /// Creates a new instance of <see cref="SerializationFunctionalityException"/> with pre-built message. 
      /// </summary>
      /// <param name="processName">The customized process name.</param>
      /// <param name="whatFailedToProvide">What the writer functionality (provider) failed to provide.</param>
      /// <param name="isProvider"><c>true</c> if the object in question was <see cref="WriterFunctionalityProvider"/> or <see cref="ReaderFunctionalityProvider"/>; <c>false</c> if it was <see cref="WriterFunctionality"/> or <see cref="ReaderFunctionality"/>.</param>
      /// <returns>A new instance of <see cref="SerializationFunctionalityException"/> with pre-built message.</returns>
      public static SerializationFunctionalityException ExceptionDuringProcess( String processName, String whatFailedToProvide, Boolean isProvider )
      {
         return new SerializationFunctionalityException( processName + " functionality" + ( isProvider ? " provider" : "" ) + " failed to provide " + whatFailedToProvide + "." );
      }
   }

   /// <summary>
   /// This is common interface for <see cref="ReaderStringStreamHandler"/> and <see cref="WriterStringStreamHandler"/>.
   /// </summary>
   public interface AbstractStringStreamHandler : AbstractReaderStreamHandler
   {
      /// <summary>
      /// Gets the <see cref="IO.StringStreamKind"/> describing what kind of strings are stored in this stream.
      /// </summary>
      /// <value>The <see cref="IO.StringStreamKind"/> describing what kind of strings are stored in this stream.</value>
      /// <seealso cref="IO.StringStreamKind"/>
      StringStreamKind StringStreamKind { get; }
   }


   /// <summary>
   /// This enumeration tells the deserialization process what kind of strings are stored to <see cref="AbstractStringStreamHandler"/>.
   /// </summary>
   public enum StringStreamKind
   {
      /// <summary>
      /// The <see cref="AbstractStringStreamHandler"/> stores system strings (type and method names, etc).
      /// </summary>
      SystemStrings,

      /// <summary>
      /// The <see cref="AbstractStringStreamHandler"/> stores user strings (string literals within IL code).
      /// </summary>
      UserStrings
   }
}

public static partial class E_CILPhysical
{
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
}