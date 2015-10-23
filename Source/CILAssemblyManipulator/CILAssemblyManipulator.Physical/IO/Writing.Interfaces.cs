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
using CILAssemblyManipulator.Physical.IO;
using CollectionsWithRoles.API;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO
{
   public interface WriterFunctionalityProvider
   {
      WriterFunctionality GetFunctionality(
         CILMetaData md,
         WritingOptions options,
         out CILMetaData newMD
         );
   }

   public interface WriterFunctionality
   {
      RawValueStorage CreateRawValuesBeforeMDStreams(
         Stream stream,
         ResizableArray<Byte> array
         );

      IEnumerable<AbstractWriterStreamHandler> CreateStreamHandlers(
         WritingOptions options,
         RawValueStorage rawValuesBeforeMDStreams
         );

      // TODO FinishUp( ... )

   }

   public interface WriterILHandler
   {
      Int32 WriteMethodIL(
         ResizableArray<Byte> sink,
         MethodILDefinition il,
         WriterStringStreamHandler userStrings,
         out Boolean isTinyHeader
         );
   }

   public interface WriterManifestResourceHandler
   {
      Int32 WriteEmbeddedManifestResource(
         ResizableArray<Byte> sink,
         Byte[] resource
         );
   }

   public interface WriterConstantsHandler
   {
      Int32 WriteConstant(
         ResizableArray<Byte> sink,
         Byte[] constant
         );
   }

   public class WriterMetaDataStreamContainer
   {
      public WriterMetaDataStreamContainer(
         WriterBLOBStreamHandler blobs,
         WriterGUIDStreamHandler guids,
         WriterStringStreamHandler sysStrings,
         WriterStringStreamHandler userStrings,
         IEnumerable<AbstractWriterStreamHandler> otherStreams
         )
      {
         this.BLOBs = blobs;
         this.GUIDs = guids;
         this.SystemStrings = sysStrings;
         this.UserStrings = userStrings;
         this.OtherStreams = otherStreams.ToArrayProxy().CQ;
      }

      public WriterBLOBStreamHandler BLOBs { get; }

      public WriterGUIDStreamHandler GUIDs { get; }

      public WriterStringStreamHandler SystemStrings { get; }

      public WriterStringStreamHandler UserStrings { get; }

      public ArrayQuery<AbstractWriterStreamHandler> OtherStreams { get; }
   }


   public interface AbstractWriterStreamHandler
   {
      String StreamName { get; }

      void WriteStream(
         Stream sink,
         ResizableArray<Byte> array
         );

      /// <summary>
      /// This should be max UInt32.Value
      /// </summary>
      Int64 CurrentSize { get; }

      Boolean Accessed { get; }
   }

   public interface WriterTableStreamHandler : AbstractWriterStreamHandler
   {
      RawValueStorage FillHeaps(
         Byte[] thisAssemblyPublicKeyIfPresentNull,
         WriterMetaDataStreamContainer mdStreams
         );
   }

   public interface WriterBLOBStreamHandler : AbstractWriterStreamHandler
   {
      Int32 RegisterBLOB( Byte[] blob );
   }

   public interface WriterStringStreamHandler : AbstractWriterStreamHandler
   {
      Int32 RegisterString( String systemString );
   }

   public interface WriterGUIDStreamHandler : AbstractWriterStreamHandler
   {
      Int32 RegisterGUID( Guid? guid );
   }

   public interface WriterCustomStreamHandler : AbstractWriterStreamHandler
   {
   }
}


public static partial class E_CILPhysical
{
   public static Boolean IsWide( this AbstractWriterStreamHandler stream )
   {
      return stream.CurrentSize > UInt16.MaxValue;
   }
}