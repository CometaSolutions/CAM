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
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO
{
   public class DefaultReaderFunctionality : ReaderFunctionality
   {
      protected const Int32 CLI_DATADIR_INDEX = 14;

      public void ReadImageInformation(
         StreamHelper stream,
         out PEInformation peInfo,
         out RVAConverter rvaConverter,
         out CLIHeader cliHeader,
         out MetaDataRoot mdRoot
         )
      {
         // Read PE info
         peInfo = stream.NewPEImageInformationFromStream();

         var dataDirs = peInfo.NTHeader.OptionalHeader.DataDirectories;

         if ( dataDirs.Count <= CLI_DATADIR_INDEX )
         {
            throw new BadImageFormatException( "No data directory for CLI header." );
         }

         // Create RVA converter
         rvaConverter = this.CreateRVAConverter( peInfo ) ?? new DefaultRVAConverter( peInfo );

         // Read CLI header
         cliHeader = stream
            .GoToRVA( rvaConverter, dataDirs[CLI_DATADIR_INDEX].RVA )
            .NewCLIHeaderFromStream();

         // Read MD root
         mdRoot = stream
            .GoToRVA( rvaConverter, cliHeader.MetaData.RVA )
            .NewMetaDataRootFromStream();
      }

      public virtual AbstractReaderStreamHandler CreateStreamHandler(
         StreamHelper stream,
         MetaDataStreamHeader header
         )
      {
         throw new NotImplementedException();
      }

      protected virtual RVAConverter CreateRVAConverter(
         PEInformation peInformation
         )
      {
         return new DefaultRVAConverter( peInformation );
      }
   }

   public class DefaultRVAConverter : RVAConverter
   {
      private readonly SectionHeader[] _sections;

      public DefaultRVAConverter( PEInformation peInfo )
      {
         ArgumentValidator.ValidateNotNull( "PE information", peInfo );

         this._sections = peInfo.SectionHeaders.ToArray();
      }

      public Int64 ToOffset( Int64 rva )
      {
         // TODO some kind of interval-map for sections...
         var sections = this._sections;
         var retVal = -1L;
         for ( var i = 0; i < sections.Length; ++i )
         {
            var sec = sections[i];
            if ( sec.VirtualAddress <= rva && rva < (Int64) sec.VirtualAddress + (Int64) Math.Max( sec.VirtualSize, sec.RawDataSize ) )
            {
               retVal = sec.RawDataPointer + ( rva - sec.VirtualAddress );
               break;
            }
         }

         return retVal;
      }

      public Int64 ToRVA( Int64 offset )
      {
         // TODO some kind of interval-map for sections...
         var sections = this._sections;
         var retVal = -1L;
         for ( var i = 0; i < sections.Length; ++i )
         {
            var sec = sections[i];
            if ( sec.RawDataPointer <= offset && offset < (Int64) sec.RawDataPointer + (Int64) sec.RawDataSize )
            {
               retVal = sec.VirtualAddress + ( offset - sec.RawDataPointer );
               break;
            }
         }

         return retVal;
      }
   }

   public abstract class AbstractReaderStreamHandlerImpl : AbstractReaderStreamHandler
   {
      protected AbstractReaderStreamHandlerImpl( StreamHelper stream )
      {
         ArgumentValidator.ValidateNotNull( "Stream", stream );

         this.Stream = stream;
         this.StartingPosition = stream.Stream.Position;
      }

      public abstract String StreamName { get; }

      protected StreamHelper Stream { get; }

      protected Int64 StartingPosition { get; }
   }

   public abstract class AbstractReaderStreamHandlerWithCustomName : AbstractReaderStreamHandlerImpl
   {
      protected AbstractReaderStreamHandlerWithCustomName( StreamHelper stream, String streamName )
         : base( stream )
      {
         this.StreamName = streamName;
      }

      public override String StreamName { get; }
   }

   public class DefaultReaderTableStreamHandler : AbstractReaderStreamHandlerWithCustomName, ReaderTableStreamHandler
   {
      private const Int32 TABLE_ARRAY_SIZE = 64;

      private readonly MetaDataTableStreamHeader _tableHeader;
      private readonly Int64 _tableStartPosition;
      private readonly Int32[] _tableSizes;
      private readonly Int32[] _tableOffsets;

      public DefaultReaderTableStreamHandler( StreamHelper stream, String tableStreamName )
         : base( stream, tableStreamName )
      {
         this._tableHeader = stream.NewTableStreamHeaderFromStream();
         this._tableStartPosition = stream.Stream.Position;

         this._tableSizes = new Int32[TABLE_ARRAY_SIZE];
         var present = this._tableHeader.PresentTablesBitVector;
         var sizeIdx = 0;
         for ( var i = 0; i < TABLE_ARRAY_SIZE; ++i )
         {
            if ( ( ( present >> i ) & 0x1 ) != 0 )
            {
               this._tableSizes[i] = (Int32) this._tableHeader.TableSizes[sizeIdx++];
            }
         }

         this._tableOffsets = new Int32[TABLE_ARRAY_SIZE];
      }

      public virtual Object GetRawRowOrNull( Tables table, Int32 idx )
      {
         var tableSizes = this._tableSizes;
         var tableInt = (Int32) table;
         Int32 tableSize;
         if ( tableInt >= 0
            && tableInt < tableSizes.Length
            && idx >= 0
            && ( tableSize = tableSizes[tableInt] ) > idx
            )
         {

         }
      }

      public virtual MetaDataTableStreamHeader ReadHeader()
      {
         return this._tableHeader;
      }
   }
}
