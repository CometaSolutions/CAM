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
         HeadersData headers,
         out CILMetaData newMD
         );
   }

   public interface WriterFunctionality
   {
      WriterILHandler CreateILHandler();

      WriterConstantsHandler CreateConstantsHandler();

      WriterManifestResourceHandler CreateManifestResourceHandler();

      IEnumerable<AbstractWriterStreamHandler> CreateStreamHandlers( WritingData writingData );
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

   public interface AbstractWriterStreamHandler
   {
      String StreamName { get; }

      void WriteStream( Stream sink );

      /// <summary>
      /// This should be max UInt32.Value
      /// </summary>
      Int64 CurrentSize { get; }

      Boolean Accessed { get; }
   }

   public interface WriterTableStreamHandler : AbstractWriterStreamHandler
   {
      void FillHeaps(
         Byte[] thisAssemblyPublicKeyIfPresentNull,
         WriterBLOBStreamHandler blobs,
         WriterStringStreamHandler sysStrings,
         WriterGuidStreamHandler guids,
         IEnumerable<AbstractWriterStreamHandler> otherStreams
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

   public interface WriterGuidStreamHandler : AbstractWriterStreamHandler
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