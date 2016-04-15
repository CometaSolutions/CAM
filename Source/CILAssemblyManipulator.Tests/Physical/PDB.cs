/*
 * Copyright 2016 Stanislav Muhametsin. All rights Reserved.
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
extern alias CAMPhysicalP;
extern alias CAMPhysicalIO;
extern alias CAMPhysicalIOD;
extern alias CAMPhysicalPIO;

using CAMPhysicalP;
using CAMPhysicalP::CILAssemblyManipulator.Physical.PDB;
using CAMPhysicalP::CILAssemblyManipulator.Physical;

using CAMPhysicalIO;

using CAMPhysicalIOD::CILAssemblyManipulator.Physical.IO;

using CAMPhysicalPIO;
using CAMPhysicalPIO::CILAssemblyManipulator.Physical.PDB;

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CommonUtils;
using CAMPhysicalIO::CILAssemblyManipulator.Physical.IO;
using System.Diagnostics.SymbolStore;
using ct = System.Runtime.InteropServices.ComTypes;
using CILAssemblyManipulator.Tests.Physical;
using Microsoft.DiaSymReader;

namespace CILAssemblyManipulator.Tests.Physical
{
   [Category( "CAM.Physical" )]
   public class PDBTest : AbstractCAMTest
   {
      [Test]
      public void TestPDB()
      {
         PerformPDBTest(
            Path.Combine( Path.GetDirectoryName( CILMergeLocation ), "CILAssemblyManipulator.Physical.PDB.IO.dll" )
            );
      }

      private static void PerformPDBTest(
         String mdFile
         )
      {
         var pdbFile = Path.ChangeExtension( mdFile, "pdb" );

         // Test reading and equality to itself
         PDBInstance pdb;
         using ( var fs = File.OpenRead( pdbFile ) )
         {
            pdb = fs.ReadPDBInstance();
         }
         Assert.IsTrue( Comparers.PDBInstanceEqualityComparer.Equals( pdb, pdb ), "PDB instance must equal itself." );

         // Test equality to identical PDB instance
         PDBInstance pdb2;
         using ( var fs = File.OpenRead( pdbFile ) )
         {
            pdb2 = fs.ReadPDBInstance();
         }
         Assert.IsTrue( Comparers.PDBInstanceEqualityComparer.Equals( pdb, pdb2 ), "Different PDB instances with same content must be equal." );

         // Test that reading using native API results in same PDB instance
         var readingArgs = new ReadingArguments();
         CILAssemblyManipulator.Physical.CILMetaData module;
         using ( var fs = File.OpenRead( mdFile ) )
         {
            module = fs.ReadModule( readingArgs );
         }
         var imageInfo = readingArgs.ImageInformation;
         Int32? ep;
         using ( var readerHolder = new SymUnmanagedReaderHolder( mdFile, module, imageInfo ) )
         {
            pdb2 = readerHolder.Reader.CreateInstanceFromNativeReader( module, out ep );
         }
         // Make sure that what we read using native reader matches what we read using our reader.
         Assert.IsTrue( Equality_PDBInstance( pdb, pdb2 ), "PDB instance created from native reader must equal the one read from .pdb file." );

         // Test writing
         Byte[] bytez;
         using ( var mem = new MemoryStream() )
         {
            pdb.WriteToStream( mem, ep );
            bytez = mem.ToArray();
         }

         // Test that reading the written file results in same PDB
         using ( var mem = new MemoryStream( bytez ) )
         {
            pdb2 = mem.ReadPDBInstance();
         }

         Assert.IsTrue( Comparers.PDBInstanceEqualityComparer.Equals( pdb, pdb2 ), "PDB after writing and reading must still have same content." );

         // Test that reading using native API results in same PDB instance
         // TODO

         // Make sure that what we read using native reader matches what we read using our reader.
         Assert.IsTrue( Equality_PDBInstance( pdb, pdb2 ), "PDB instance created from native reader must equal the one read from .pdb file." );

         // Use native writer to write to stream
         using ( var stream = new MemoryStream() )
         {
            var comStream = new COMStreamWrapper( stream );
            using ( var writerHolder = new SymUnmanagedWriterHolder( module, imageInfo, comStream ) )
            {
               writerHolder.ProcessPDB( pdb );
            }
            bytez = stream.ToArray();
         }

         // Read the PDB written by native writer
         using ( var stream = new MemoryStream( bytez ) )
         {
            pdb2 = stream.ReadPDBInstance();
         }

         Assert.IsTrue( Equality_PDBInstance( pdb, pdb2 ), "PDB after writing and reading must still have same content." );

      }

      private static Boolean Equality_PDBInstance( PDBInstance x, PDBInstance y )
      {
         // Native reader does not expose DebugGUID, TimeStamp, nor Age
         return ListEqualityComparer<List<PDBModule>, PDBModule>.ListEquality( x.Modules, y.Modules, Equality_PDBModule );
      }

      private static Boolean Equality_PDBModule( PDBModule x, PDBModule y )
      {
         // Native reader does not expose anything about the PDBModule, actually, but we get the name from meta-data.
         var retVal = String.Equals( x.Name, y.Name )
            && ListEqualityComparer<List<PDBFunction>, PDBFunction>.ListEquality( x.Functions, y.Functions, Equality_PDBFunction );
         if ( !retVal )
         {

         }
         return retVal;
      }

      private static Boolean Equality_PDBFunction( PDBFunction x, PDBFunction y )
      {
         var retVal = Equality_PDBScopeOrFunction( x, y )
            && String.Equals( x.Name, y.Name )
            && x.Token == y.Token
            && Equality_PDBAsyncInfo( x.AsyncMethodInfo, y.AsyncMethodInfo )
            && x.ENCID == y.ENCID
            && x.ForwardingMethodToken == y.ForwardingMethodToken
            && x.ModuleForwardingMethodToken == y.ModuleForwardingMethodToken
            && String.Equals( x.IteratorClass, y.IteratorClass )
            && ListEqualityComparer<List<PDBLocalScope>, PDBLocalScope>.ListEquality( x.LocalScopes, y.LocalScopes, Equality_PDBLocalScope )
            && ListEqualityComparer<List<PDBLine>, PDBLine>.ListEquality( x.Lines, y.Lines, Equality_PDBLine );

         if ( !retVal )
         {

         }
         return retVal;
      }

      private static Boolean Equality_PDBAsyncInfo( PDBAsyncMethodInfo x, PDBAsyncMethodInfo y )
      {
         // Native API exposes all data of async method info
         var retVal = Comparers.PDBAsyncMethodInfoEqualityComparer.Equals( x, y );
         if ( !retVal )
         {

         }
         return retVal;
      }

      private static Boolean Equality_PDBLocalScope( PDBLocalScope x, PDBLocalScope y )
      {
         var retVal = x.Offset == y.Offset
            && x.Length == y.Length;
         if ( !retVal )
         {

         }
         return retVal;
      }

      private static Boolean Equality_PDBScopeOrFunction( PDBScopeOrFunction x, PDBScopeOrFunction y )
      {
         var retVal = x.Length == y.Length
            && ListEqualityComparer<List<String>, String>.ListEquality( x.UsedNamespaces, y.UsedNamespaces )
            && ListEqualityComparer<List<PDBSlot>, PDBSlot>.ListEquality( x.Slots, y.Slots, Equality_PDBSlot )
            && ListEqualityComparer<List<PDBConstant>, PDBConstant>.ListEquality( x.Constants, y.Constants, Equality_PDBConstant )
            && ListEqualityComparer<List<PDBScope>, PDBScope>.ListEquality( x.Scopes, y.Scopes, Equality_PDBScope );
         if ( !retVal )
         {

         }
         return retVal;
      }

      private static Boolean Equality_PDBScope( PDBScope x, PDBScope y )
      {
         var retVal = Equality_PDBScopeOrFunction( x, y )
            && x.Offset == y.Offset;
         if ( !retVal )
         {

         }
         return retVal;
      }

      private static Boolean Equality_PDBSlot( PDBSlot x, PDBSlot y )
      {
         // Type token is not obtaineable through native API
         var retVal = x.Flags == y.Flags
            && String.Equals( x.Name, y.Name )
            && x.SlotIndex == y.SlotIndex;
         if ( !retVal )
         {

         }
         return retVal;
      }

      private static Boolean Equality_PDBConstant( PDBConstant x, PDBConstant y )
      {
         // Token not obtaineable through native API
         var retVal = String.Equals( x.Name, y.Name )
            && Comparers.PDBConstantValueEqualityComparer.Equals( x.Value, y.Value );
         if ( !retVal )
         {

         }
         return retVal;
      }

      private static Boolean Equality_PDBLine( PDBLine x, PDBLine y )
      {
         var retVal = x.LineStart == y.LineStart
            && x.LineEnd == y.LineEnd
            && x.ColumnStart.Equals( y.ColumnStart )
            && x.ColumnEnd.Equals( y.ColumnEnd )
            && x.Offset == y.Offset
            //&& x.IsStatement == y.IsStatement
            && Equality_PDBSource( x.Source, y.Source );
         if ( !retVal )
         {

         }
         return retVal;
      }

      private static Boolean Equality_PDBSource( PDBSource x, PDBSource y )
      {
         var retVal = x.DocumentType == y.DocumentType
            && x.Vendor == y.Vendor
            && x.Language == y.Language
            && x.HashAlgorithm == y.HashAlgorithm
            && String.Equals( x.Name, y.Name )
            && ArrayEqualityComparer<Byte>.ArrayEquality( x.Hash, y.Hash );
         if ( !retVal )
         {

         }
         return retVal;
      }
   }



   internal class PDBHelper
   {
      internal static UInt32 WriteStringUnmanaged( IntPtr bufferPtr, UInt32 buffSize, String str )
      {
         var maxLen = str.Length + 1 >= buffSize ? buffSize - 1 : (UInt32) str.Length;
         for ( var i = 0; i < maxLen; ++i )
         {
            Marshal.WriteInt16( bufferPtr, i * 2, str[i] );
         }
         Marshal.WriteInt16( bufferPtr, (Int32) maxLen * 2, 0 ); // Remember terminating zero
         return maxLen + 1;
      }

      internal static void WriteInt32Unmanaged( IntPtr ptr, Int32 i32 )
      {
         if ( ptr != IntPtr.Zero )
         {
            Marshal.WriteInt32( ptr, i32 );
         }
      }
   }

   #region PDB Writing

   internal class SymUnmanagedWriterHolder : AbstractDisposable
   {
      private readonly String _fn;
      private readonly String _tmpFN;
      private readonly ImageInformation _imageInfo;
      private readonly IDictionary<PDBSource, ISymUnmanagedDocumentWriter> _unmanagedDocs;

      public SymUnmanagedWriterHolder(
         CILAssemblyManipulator.Physical.CILMetaData module,
         ImageInformation imageInfo,
         ct.IStream stream
         ) : this( module, imageInfo, null, null, stream )
      {

      }

      public SymUnmanagedWriterHolder(
         CILAssemblyManipulator.Physical.CILMetaData module,
         ImageInformation imageInfo,
         String fn,
         String tmpFN = null
         ) : this( module, imageInfo, fn, tmpFN, null )
      {
      }

      private SymUnmanagedWriterHolder(
         CILAssemblyManipulator.Physical.CILMetaData module,
         ImageInformation imageInfo,
         String fn,
         String tmpFN,
         ct.IStream stream
         )
      {
         this._imageInfo = imageInfo;
         this._fn = fn;
         this._tmpFN = tmpFN;

         var writer = (ISymUnmanagedWriter2) Activator.CreateInstance( Type.GetTypeFromCLSID( new Guid( "0AE2DEB0-F901-478b-BB9F-881EE8066788" ) ) );
         // Initialize writer
         var emitter = new MDHelper( module, imageInfo );
         if ( !String.IsNullOrEmpty( tmpFN ) )
         {
            writer.Initialize2(
               emitter, // Emitter
               tmpFN, // Temporary file name
               stream, // IStream
               true, // isFullBuild
               fn // Final file name
               );
         }
         else
         {
            writer.Initialize(
               emitter, // Emitter
               fn, // Final file name
               stream, // IStream
               true // isFullBuild
               );
         }
         this.Writer = writer;
         this._unmanagedDocs = new Dictionary<PDBSource, ISymUnmanagedDocumentWriter>( ComparerFromFunctions.NewEqualityComparer<PDBSource>(
            ( x, y ) => String.Equals( x.Name, y.Name ),
            x => x?.Name.GetHashCode() ?? 0
            ) );
      }

      ~SymUnmanagedWriterHolder()
      {
         this.Dispose( false );
      }

      public ISymUnmanagedWriter2 Writer { get; }

      public ISymUnmanagedDocumentWriter GetDocumentWriterFor( PDBSource source )
      {
         return this._unmanagedDocs.GetOrAdd_NotThreadSafe( source, s =>
         {
            var name = s.Name;
            var lang = s.Language;
            var docType = s.DocumentType;
            var vendor = s.Vendor;
            ISymUnmanagedDocumentWriter uDoc;
            this.Writer.DefineDocument( name, ref lang, ref vendor, ref docType, out uDoc );
            uDoc.SetCheckSum( s.HashAlgorithm, s.Hash.Length, s.Hash );
            return uDoc;
         } );
      }

      protected override void Dispose( Boolean disposing )
      {
         // Remember set entry point before writing out.
         var imageInfo = this._imageInfo;
         var writer = this.Writer;
         Int32 epToken;
         if ( imageInfo.CLIInformation.CLIHeader.TryGetManagedOrUnmanagedEntryPoint( out epToken ) )
         {
            writer.SetUserEntryPoint( new SymbolToken( epToken ) );
         }
         writer.Close();
         try
         {
            Marshal.ReleaseComObject( writer );
            foreach ( var doc in this._unmanagedDocs )
            {
               Marshal.ReleaseComObject( doc.Value );
            }
         }
         finally
         {
            var fn = this._fn;
            if ( !String.IsNullOrEmpty( fn ) )
            {
               // Copy tmp-PDB to new location
               var tmpFN = this._tmpFN;
               if ( !String.IsNullOrEmpty( tmpFN ) )
               {
                  System.IO.File.Copy( this._tmpFN, fn, true );
               }
            }
         }
      }
   }


   // See http://msdn.microsoft.com/en-us/library/ms231406%28v=vs.110%29.aspx
   [Guid( "0B97726E-9E6D-4f05-9A26-424022093CAA" ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown ), ComImport]
   internal interface ISymUnmanagedWriter2
   {
      void DefineDocument(
         [In, MarshalAs( UnmanagedType.LPWStr )] String url,
         [In] ref Guid language,
         [In] ref Guid languageVendor,
         [In] ref Guid documentType,
         [Out, MarshalAs( UnmanagedType.Interface )] out ISymUnmanagedDocumentWriter pRetVal );
      void SetUserEntryPoint( [In] SymbolToken method );
      void OpenMethod( [In] SymbolToken method );
      void CloseMethod();
      void OpenScope( [In] Int32 startOffset, [Out] out Int32 pRetVal );
      void CloseScope( [In] Int32 endOffset );

      // Unused
      void SetScopeRange( [In] Int32 scopeID, [In] Int32 startOffset, [In] Int32 endOffset );

      // Unused
      void DefineLocalVariable(
         [In, MarshalAs( UnmanagedType.LPWStr )] String name,
         [In] Int32 attributes,
         [In] Int32 cSig,
         [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 2 )] Byte[] signature,
         [In] Int32 addrKind,
         [In] Int32 addr1,
         [In] Int32 addr2,
         [In] Int32 addr3,
         [In] Int32 startOffset,
         [In] Int32 endOffset );

      // Unused
      void DefineParameter(
         [In, MarshalAs( UnmanagedType.LPWStr )] String name,
         [In] Int32 attributes,
         [In] Int32 sequence,
         [In] Int32 addrKind,
         [In] Int32 addr1,
         [In] Int32 addr2,
         [In] Int32 addr3
         );

      // Unused
      void DefineField(
         [In] SymbolToken parent,
         [In, MarshalAs( UnmanagedType.LPWStr )] String name,
         [In] Int32 attributes,
         [In] Int32 cSig,
         [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 3 )] Byte[] signature,
         [In] Int32 addrKind,
         [In] Int32 addr1,
         [In] Int32 addr2,
         [In] Int32 addr3
         );

      // Unused
      void DefineGlobalVariable(
         [In, MarshalAs( UnmanagedType.LPWStr )] String name,
         [In] Int32 attributes,
         [In] Int32 cSig,
         [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 2 )] Byte[] signature,
         [In] Int32 addrKind,
         [In] Int32 addr1,
         [In] Int32 addr2,
         [In] Int32 addr3
         );

      void Close();

      // Unused
      void SetSymAttribute(
         [In] SymbolToken parent,
         [In, MarshalAs( UnmanagedType.LPWStr )] String name,
         [In] Int32 cData,
         [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 2 )] Byte[] data
         );

      // Unused
      void OpenNamespace( [In, MarshalAs( UnmanagedType.LPWStr )] String name );

      // Unused
      void CloseNamespace();

      void UsingNamespace( [In, MarshalAs( UnmanagedType.LPWStr )] String fullName );

      // Unused
      void SetMethodSourceRange(
         [In] ISymUnmanagedDocumentWriter startDoc,
         [In] Int32 startLine,
         [In] Int32 startColumn,
         [In] ISymUnmanagedDocumentWriter endDoc,
         [In] Int32 endLine,
         [In] Int32 endColumn
         );

      // Unused
      void Initialize(
         [In, MarshalAs( UnmanagedType.IUnknown )] Object emitter,
         [In, MarshalAs( UnmanagedType.LPWStr )] String filename,
         [In] ct.IStream pIStream,
         [In] Boolean fFullBuild );

      void GetDebugInfo(
         [Out] out ImageDebugDirectory pIDD,
         [In] Int32 cData,
         [Out] out Int32 pcData,
         [In, Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 1 )] Byte[] data );

      void DefineSequencePoints(
         [In, MarshalAs( UnmanagedType.Interface )] ISymUnmanagedDocumentWriter document,
         [In] Int32 spCount,
         [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 1 )] Int32[] offsets,
         [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 1 )] Int32[] lines,
         [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 1 )] Int32[] columns,
         [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 1 )] Int32[] endLines,
         [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 1 )] Int32[] endColumns );

      // Unused
      void RemapToken(
         [In] SymbolToken oldToken,
         [In] SymbolToken newToken
         );

      void Initialize2(
         [In, MarshalAs( UnmanagedType.IUnknown )] Object emitter,
         [In, MarshalAs( UnmanagedType.LPWStr )] String tempFileName,
         [In] ct.IStream pIStream,
         [In] Boolean fFullBuild,
         [In, MarshalAs( UnmanagedType.LPWStr )] String finalFileName
         );

      // Unused
      void DefineConstant(
         [MarshalAs( UnmanagedType.LPWStr )] String name,
         [In] Object value,
         [In] Int32 cSig,
         [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 2 )] Byte[] signature
         );

      // Unused
      void Abort();

      // Begin ISymUnmanagedWriter2 methods

      void DefineLocalVariable2(
         [In, MarshalAs( UnmanagedType.LPWStr )] String name,
         [In] Int32 attributes,
         [In] SymbolToken sigToken,
         [In] Int32 addrKind,
         [In] Int32 addr1,
         [In] Int32 addr2,
         [In] Int32 addr3,
         [In] Int32 startOffset,
         [In] Int32 endOffset );

      // Unused
      void DefineGlobalVariable2(
         [In, MarshalAs( UnmanagedType.LPWStr )] String name,
         [In] Int32 attributes,
         [In] SymbolToken sigToken,
         [In] Int32 addressKind,
         [In] Int32 addr1,
         [In] Int32 addr2,
         [In] Int32 addr3
         );

      // Unused
      void DefineConstant2(
         [In, MarshalAs( UnmanagedType.LPWStr )] String name,
         [In] Object value,
         [In] SymbolToken sigToken
         );
   }

   [Guid( "B01FAFEB-C450-3A4D-BEEC-B4CEEC01E006" ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown ), ComImport]
   internal interface ISymUnmanagedDocumentWriter
   {
      /*
       * Sets embedded source for a document being written.
       */
      void SetSource( [In] Int32 sourceSize,
                      [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0 )] Byte[] source
                  );

      /*
       * Sets check sum info.
       */
      void SetCheckSum( [In] Guid algorithmId,
                        [In] Int32 checkSumSize,
                        [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 1 )] Byte[] checkSum
         );
   }

   internal interface ISymUnmanagedAsyncMethodPropertiesWriter
   {
      /*
       * Sets the starting method that initiates the async operation.
       *
       * When performing a step-out of an async method, if the caller matches
       * the kickoff method, we will step out synchronously.  Otherwise, an
       * async step-out will occur.
       *
       * This works in C#/VB because there is an initial method stub that
       * creates the state machine object and starts if off, hence "kickoff"
       * Still have to determine if this will work with F#.
       */
      void DefineKickoffMethod( [In] SymbolToken kickoffMethod );

      /*
      * Sets the IL offset for the compiler generated catch handler that wraps
      * an async method.
      *
      * The IL offset of the generated catch is used by the debugger to handle
      * the catch as though it were non-user code even though it may occur in
      * a user code method.  In particular it is used in response to a
      * CatchHandlerFound exception event.
      */
      void DefineCatchHandlerILOffset( [In] Int32 catchHandlerOffset );

      /*
       * Define a group of async scopes within the current method.
       *
       * Each yield offset matches an await's return instruction,
       * identifying a potential yield.  Each breakpointMethod/breakpointOffset
       * pair tells us where the asynchronous operation will resume
       * (which may be in a different method).
       */
      void DefineAsyncStepInfo( [In] Int32 count,
                                  [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0 )] Int32[] yieldOffsets,
                                  [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0 )] Int32[] breakpointOffset,
                                  [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0 )] SymbolToken[] breakpointMethod
         );
   }

   // Needed for ISymUnmanagedWriter2
   [StructLayout( LayoutKind.Sequential )]
   internal struct ImageDebugDirectory
   {
      public Int32 Characteristics;
      public Int32 TimeDateStamp;
      public Int16 MajorVersion;
      public Int16 MinorVersion;
      public Int32 Type;
      public Int32 SizeOfData;
      public Int32 AddressOfRawData;
      public Int32 PointerToRawData;
   }

   [ComImport, InterfaceType( ComInterfaceType.InterfaceIsIUnknown ), Guid( "BA3FEE4C-ECB9-4e41-83B7-183FA41CD859" )]
   internal interface IMetaDataEmit
   {
      void SetModuleProps( string szName );
      void Save( string szFile, uint dwSaveFlags );
      void SaveToStream( IntPtr pIStream, uint dwSaveFlags );
      uint GetSaveSize( uint fSave );
      uint DefineTypeDef( IntPtr szTypeDef, uint dwTypeDefFlags, uint tkExtends, IntPtr rtkImplements );
      uint DefineNestedType( IntPtr szTypeDef, uint dwTypeDefFlags, uint tkExtends, IntPtr rtkImplements, uint tdEncloser );
      void SetHandler( [MarshalAs( UnmanagedType.IUnknown ), In] object pUnk );
      uint DefineMethod( uint td, IntPtr zName, uint dwMethodFlags, IntPtr pvSigBlob, uint cbSigBlob, uint ulCodeRVA, uint dwImplFlags );
      void DefineMethodImpl( uint td, uint tkBody, uint tkDecl );
      uint DefineTypeRefByName( uint tkResolutionScope, IntPtr szName );
      uint DefineImportType( IntPtr pAssemImport, IntPtr pbHashValue, uint cbHashValue, IMetaDataImport pImport,
        uint tdImport, IntPtr pAssemEmit );
      uint DefineMemberRef( uint tkImport, string szName, IntPtr pvSigBlob, uint cbSigBlob );
      uint DefineImportMember( IntPtr pAssemImport, IntPtr /* void* */ pbHashValue, uint cbHashValue,
        IMetaDataImport pImport, uint mbMember, IntPtr pAssemEmit, uint tkParent );
      uint DefineEvent( uint td, string szEvent, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, IntPtr /* uint* */ rmdOtherMethods );
      void SetClassLayout( uint td, uint dwPackSize, IntPtr /*COR_FIELD_OFFSET**/ rFieldOffsets, uint ulClassSize );
      void DeleteClassLayout( uint td );
      void SetFieldMarshal( uint tk, IntPtr /* byte* */ pvNativeType, uint cbNativeType );
      void DeleteFieldMarshal( uint tk );
      uint DefinePermissionSet( uint tk, uint dwAction, IntPtr /* void* */ pvPermission, uint cbPermission );
      void SetRVA( uint md, uint ulRVA );
      uint GetTokenFromSig( IntPtr /* byte* */ pvSig, uint cbSig );
      uint DefineModuleRef( string szName );
      void SetParent( uint mr, uint tk );
      uint GetTokenFromTypeSpec( IntPtr /* byte* */ pvSig, uint cbSig );
      void SaveToMemory( IntPtr /* void* */ pbData, uint cbData );
      uint DefineUserString( string szString, uint cchString );
      void DeleteToken( uint tkObj );
      void SetMethodProps( uint md, uint dwMethodFlags, uint ulCodeRVA, uint dwImplFlags );
      void SetTypeDefProps( uint td, uint dwTypeDefFlags, uint tkExtends, IntPtr /* uint* */ rtkImplements );
      void SetEventProps( uint ev, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, IntPtr /* uint* */ rmdOtherMethods );
      uint SetPermissionSetProps( uint tk, uint dwAction, IntPtr /* void* */ pvPermission, uint cbPermission );
      void DefinePinvokeMap( uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL );
      void SetPinvokeMap( uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL );
      void DeletePinvokeMap( uint tk );
      uint DefineCustomAttribute( uint tkObj, uint tkType, IntPtr /* void* */ pCustomAttribute, uint cbCustomAttribute );
      void SetCustomAttributeValue( uint pcv, IntPtr /* void* */ pCustomAttribute, uint cbCustomAttribute );
      uint DefineField( uint td, string szName, uint dwFieldFlags, IntPtr /* byte* */ pvSigBlob, uint cbSigBlob, uint dwCPlusTypeFlag, IntPtr /* void* */ pValue, uint cchValue );
      uint DefineProperty( uint td, string szProperty, uint dwPropFlags, IntPtr /* byte* */ pvSig, uint cbSig, uint dwCPlusTypeFlag,
        IntPtr /* void* */ pValue, uint cchValue, uint mdSetter, uint mdGetter, IntPtr /* uint*  */ rmdOtherMethods );
      uint DefineParam( uint md, uint ulParamSeq, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, IntPtr /* void* */ pValue, uint cchValue );
      void SetFieldProps( uint fd, uint dwFieldFlags, uint dwCPlusTypeFlag, IntPtr /* void* */ pValue, uint cchValue );
      void SetPropertyProps( uint pr, uint dwPropFlags, uint dwCPlusTypeFlag, IntPtr /* void* */ pValue, uint cchValue, uint mdSetter, uint mdGetter, IntPtr /* uint* */ rmdOtherMethods );
      void SetParamProps( uint pd, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, IntPtr /* void* */ pValue, uint cchValue );
      uint DefineSecurityAttributeSet( uint tkObj, IntPtr rSecAttrs, uint cSecAttrs );
      void ApplyEditAndContinue( [MarshalAs( UnmanagedType.IUnknown )]object pImport );
      uint TranslateSigWithScope( IntPtr pAssemImport, IntPtr /* void* */ pbHashValue, uint cbHashValue,
        IMetaDataImport import, IntPtr /* byte* */ pbSigBlob, uint cbSigBlob, IntPtr pAssemEmit, IMetaDataEmit emit, IntPtr /* byte* */ pvTranslatedSig, uint cbTranslatedSigMax );
      void SetMethodImplFlags( uint md, uint dwImplFlags );
      void SetFieldRVA( uint fd, uint ulRVA );
      void Merge( IMetaDataImport pImport, IntPtr pHostMapToken, [MarshalAs( UnmanagedType.IUnknown )]object pHandler );
      void MergeEnd();
   }

   [ComImport, InterfaceType( ComInterfaceType.InterfaceIsIUnknown ), Guid( "7DAC8207-D3AE-4c75-9B67-92801A497D44" )]
   internal interface IMetaDataImport
   {
      [PreserveSig]
      void CloseEnum( uint hEnum );
      uint CountEnum( uint hEnum );
      void ResetEnum( uint hEnum, uint ulPos );
      uint EnumTypeDefs( ref uint phEnum, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 2 )] uint[] rTypeDefs, uint cMax );
      uint EnumInterfaceImpls( ref uint phEnum, uint td, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 3 )] uint[] rImpls, uint cMax );
      uint EnumTypeRefs( ref uint phEnum, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 2 )] uint[] rTypeRefs, uint cMax );
      uint FindTypeDefByName( string szTypeDef, uint tkEnclosingClass );
      Guid GetScopeProps( StringBuilder szName, uint cchName, out uint pchName );
      uint GetModuleFromScope();
      uint GetTypeDefProps( uint td, IntPtr szTypeDef, uint cchTypeDef, out uint pchTypeDef, IntPtr pdwTypeDefFlags );
      uint GetInterfaceImplProps( uint iiImpl, out uint pClass );
      uint GetTypeRefProps( uint tr, out uint ptkResolutionScope, StringBuilder szName, uint cchName );
      uint ResolveTypeRef( uint tr, [In] ref Guid riid, [MarshalAs( UnmanagedType.Interface )] out object ppIScope );
      uint EnumMembers( ref uint phEnum, uint cl, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 3 )] uint[] rMembers, uint cMax );
      uint EnumMembersWithName( ref uint phEnum, uint cl, string szName, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 4 )] uint[] rMembers, uint cMax );
      uint EnumMethods( ref uint phEnum, uint cl, IntPtr /* uint* */ rMethods, uint cMax );
      uint EnumMethodsWithName( ref uint phEnum, uint cl, string szName, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 4 )] uint[] rMethods, uint cMax );
      uint EnumFields( ref uint phEnum, uint cl, IntPtr /* uint* */ rFields, uint cMax );
      uint EnumFieldsWithName( ref uint phEnum, uint cl, string szName, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 4 )] uint[] rFields, uint cMax );
      uint EnumParams( ref uint phEnum, uint mb, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 3 )] uint[] rParams, uint cMax );
      uint EnumMemberRefs( ref uint phEnum, uint tkParent, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 3 )] uint[] rMemberRefs, uint cMax );
      uint EnumMethodImpls( ref uint phEnum, uint td, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 4 )] uint[] rMethodBody,
         [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 4 )] uint[] rMethodDecl, uint cMax );
      uint EnumPermissionSets( ref uint phEnum, uint tk, uint dwActions, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 4 )] uint[] rPermission,
         uint cMax );
      uint FindMember( uint td, string szName, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 3 )] byte[] pvSigBlob, uint cbSigBlob );
      uint FindMethod( uint td, string szName, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 3 )] byte[] pvSigBlob, uint cbSigBlob );
      uint FindField( uint td, string szName, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 3 )] byte[] pvSigBlob, uint cbSigBlob );
      uint FindMemberRef( uint td, string szName, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 3 )] byte[] pvSigBlob, uint cbSigBlob );
      uint GetMethodProps( uint mb, out uint pClass, IntPtr szMethod, uint cchMethod, out uint pchMethod, IntPtr pdwAttr, IntPtr ppvSigBlob, IntPtr pcbSigBlob, IntPtr pulCodeRVA );
      uint GetMemberRefProps( uint mr, ref uint ptk, StringBuilder szMember, uint cchMember, out uint pchMember, out IntPtr /* byte* */ ppvSigBlob );
      uint EnumProperties( ref uint phEnum, uint td, IntPtr /* uint* */ rProperties, uint cMax );
      uint EnumEvents( ref uint phEnum, uint td, IntPtr /* uint* */ rEvents, uint cMax );
      uint GetEventProps( uint ev, out uint pClass, StringBuilder szEvent, uint cchEvent, out uint pchEvent, out uint pdwEventFlags,
        out uint ptkEventType, out uint pmdAddOn, out uint pmdRemoveOn, out uint pmdFire,
        [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 11 )] uint[] rmdOtherMethod, uint cMax );
      uint EnumMethodSemantics( ref uint phEnum, uint mb, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 3 )] uint[] rEventProp, uint cMax );
      uint GetMethodSemantics( uint mb, uint tkEventProp );
      uint GetClassLayout( uint td, out uint pdwPackSize, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 3 )] IntPtr /*COR_FIELD_OFFSET **/ rFieldOffset, uint cMax, out uint pcFieldOffset );
      uint GetFieldMarshal( uint tk, out IntPtr /* byte* */ ppvNativeType );
      uint GetRVA( uint tk, out uint pulCodeRVA );
      uint GetPermissionSetProps( uint pm, out uint pdwAction, out IntPtr /* void* */ ppvPermission );
      uint GetSigFromToken( uint mdSig, out IntPtr /* byte* */ ppvSig );
      uint GetModuleRefProps( uint mur, StringBuilder szName, uint cchName );
      uint EnumModuleRefs( ref uint phEnum, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 2 )] uint[] rModuleRefs, uint cmax );
      uint GetTypeSpecFromToken( uint typespec, out IntPtr /* byte* */ ppvSig );
      uint GetNameFromToken( uint tk );
      uint EnumUnresolvedMethods( ref uint phEnum, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 2 )] uint[] rMethods, uint cMax );
      uint GetUserString( uint stk, StringBuilder szString, uint cchString );
      uint GetPinvokeMap( uint tk, out uint pdwMappingFlags, StringBuilder szImportName, uint cchImportName, out uint pchImportName );
      uint EnumSignatures( ref uint phEnum, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 2 )] uint[] rSignatures, uint cmax );
      uint EnumTypeSpecs( ref uint phEnum, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 2 )] uint[] rTypeSpecs, uint cmax );
      uint EnumUserStrings( ref uint phEnum, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 2 )] uint[] rStrings, uint cmax );
      [PreserveSig]
      int GetParamForMethodIndex( uint md, uint ulParamSeq, out uint pParam );
      uint EnumCustomAttributes( ref uint phEnum, uint tk, uint tkType, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 4 )] uint[] rCustomAttributes, uint cMax );
      uint GetCustomAttributeProps( uint cv, out uint ptkObj, out uint ptkType, out IntPtr /* void* */ ppBlob );
      uint FindTypeRef( uint tkResolutionScope, string szName );
      uint GetMemberProps( uint mb, out uint pClass, StringBuilder szMember, uint cchMember, out uint pchMember, out uint pdwAttr,
        out IntPtr /* byte* */ ppvSigBlob, out uint pcbSigBlob, out uint pulCodeRVA, out uint pdwImplFlags, out uint pdwCPlusTypeFlag, out IntPtr /* void* */ ppValue );
      uint GetFieldProps( uint mb, out uint pClass, StringBuilder szField, uint cchField, out uint pchField, out uint pdwAttr,
        out IntPtr /* byte* */ ppvSigBlob, out uint pcbSigBlob, out uint pdwCPlusTypeFlag, out IntPtr /* void* */ ppValue );
      uint GetPropertyProps( uint prop, out uint pClass, StringBuilder szProperty, uint cchProperty, out uint pchProperty, out uint pdwPropFlags,
        out IntPtr /* byte* */ ppvSig, out uint pbSig, out uint pdwCPlusTypeFlag, out IntPtr /* void* */ ppDefaultValue, out uint pcchDefaultValue, out uint pmdSetter,
        out uint pmdGetter, [MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 14 )] uint[] rmdOtherMethod, uint cMax );
      uint GetParamProps( uint tk, out uint pmd, out uint pulSequence, StringBuilder szName, uint cchName, out uint pchName,
        out uint pdwAttr, out uint pdwCPlusTypeFlag, out IntPtr /* void* */ ppValue );
      uint GetCustomAttributeByName( uint tkObj, string szName, out IntPtr /* void* */ ppData );
      [PreserveSig]
      [return: MarshalAs( UnmanagedType.Bool )]
      bool IsValidToken( uint tk );
      uint GetNestedClassProps( uint tdNestedClass );
      uint GetNativeCallConvFromSig( IntPtr /* void* */ pvSig, uint cbSig );
      int IsGlobal( uint pd );
   }

   internal class MDHelper : IMetaDataEmit, IMetaDataImport
   {
      private readonly CILAssemblyManipulator.Physical.CILMetaData _module;
      private readonly ImageInformation _imageInfo;
      private readonly IList<Int32> _methodDeclaringTypes;
      private readonly IDictionary<Int32, Int32> _typeEnclosingTypes;

      internal MDHelper( CILAssemblyManipulator.Physical.CILMetaData module, ImageInformation imageInfo )
      {
         ArgumentValidator.ValidateNotNull( "Module", module );
         ArgumentValidator.ValidateNotNull( "Image information", imageInfo );
         this._module = module;
         this._imageInfo = imageInfo;

         var methodDeclaringTypes = new List<Int32>( module.MethodDefinitions.GetRowCount() );
         for ( var i = 0; i < module.TypeDefinitions.GetRowCount(); ++i )
         {
            // Don't use loop variable in lambda
            var cur = i;
            methodDeclaringTypes.AddRange( module.GetTypeMethodIndices( cur ).Select( m => cur ) );
         }
         this._methodDeclaringTypes = methodDeclaringTypes;

         var typeEnclosingTypes = new Dictionary<Int32, Int32>( module.NestedClassDefinitions.GetRowCount() );
         foreach ( var nc in module.NestedClassDefinitions.TableContents )
         {
            typeEnclosingTypes[nc.NestedClass.Index] = nc.EnclosingClass.Index;
         }
         this._typeEnclosingTypes = typeEnclosingTypes;
      }

      #region IMetaDataEmit Members

      public void SetModuleProps( string szName )
      {
         throw new NotImplementedException();
      }

      public void Save( string szFile, uint dwSaveFlags )
      {
         throw new NotImplementedException();
      }

      public void SaveToStream( IntPtr pIStream, uint dwSaveFlags )
      {
         throw new NotImplementedException();
      }

      public uint GetSaveSize( uint fSave )
      {
         throw new NotImplementedException();
      }

      public uint DefineTypeDef( IntPtr szTypeDef, uint dwTypeDefFlags, uint tkExtends, IntPtr rtkImplements )
      {
         throw new NotImplementedException();
      }

      public uint DefineNestedType( IntPtr szTypeDef, uint dwTypeDefFlags, uint tkExtends, IntPtr rtkImplements, uint tdEncloser )
      {
         throw new NotImplementedException();
      }

      public void SetHandler( object pUnk )
      {
         throw new NotImplementedException();
      }

      public uint DefineMethod( uint td, IntPtr zName, uint dwMethodFlags, IntPtr pvSigBlob, uint cbSigBlob, uint ulCodeRVA, uint dwImplFlags )
      {
         throw new NotImplementedException();
      }

      public void DefineMethodImpl( uint td, uint tkBody, uint tkDecl )
      {
         throw new NotImplementedException();
      }

      public uint DefineTypeRefByName( uint tkResolutionScope, IntPtr szName )
      {
         throw new NotImplementedException();
      }

      public uint DefineImportType( IntPtr pAssemImport, IntPtr pbHashValue, uint cbHashValue, IMetaDataImport pImport, uint tdImport, IntPtr pAssemEmit )
      {
         throw new NotImplementedException();
      }

      public uint DefineMemberRef( uint tkImport, string szName, IntPtr pvSigBlob, uint cbSigBlob )
      {
         throw new NotImplementedException();
      }

      public uint DefineImportMember( IntPtr pAssemImport, IntPtr pbHashValue, uint cbHashValue, IMetaDataImport pImport, uint mbMember, IntPtr pAssemEmit, uint tkParent )
      {
         throw new NotImplementedException();
      }

      public uint DefineEvent( uint td, string szEvent, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, IntPtr rmdOtherMethods )
      {
         throw new NotImplementedException();
      }

      public void SetClassLayout( uint td, uint dwPackSize, IntPtr rFieldOffsets, uint ulClassSize )
      {
         throw new NotImplementedException();
      }

      public void DeleteClassLayout( uint td )
      {
         throw new NotImplementedException();
      }

      public void SetFieldMarshal( uint tk, IntPtr pvNativeType, uint cbNativeType )
      {
         throw new NotImplementedException();
      }

      public void DeleteFieldMarshal( uint tk )
      {
         throw new NotImplementedException();
      }

      public uint DefinePermissionSet( uint tk, uint dwAction, IntPtr pvPermission, uint cbPermission )
      {
         throw new NotImplementedException();
      }

      public void SetRVA( uint md, uint ulRVA )
      {
         throw new NotImplementedException();
      }

      public uint GetTokenFromSig( IntPtr pvSig, uint cbSig )
      {
         throw new NotImplementedException();
      }

      public uint DefineModuleRef( string szName )
      {
         throw new NotImplementedException();
      }

      public void SetParent( uint mr, uint tk )
      {
         throw new NotImplementedException();
      }

      public uint GetTokenFromTypeSpec( IntPtr pvSig, uint cbSig )
      {
         throw new NotImplementedException();
      }

      public void SaveToMemory( IntPtr pbData, uint cbData )
      {
         throw new NotImplementedException();
      }

      public uint DefineUserString( string szString, uint cchString )
      {
         throw new NotImplementedException();
      }

      public void DeleteToken( uint tkObj )
      {
         throw new NotImplementedException();
      }

      public void SetMethodProps( uint md, uint dwMethodFlags, uint ulCodeRVA, uint dwImplFlags )
      {
         throw new NotImplementedException();
      }

      public void SetTypeDefProps( uint td, uint dwTypeDefFlags, uint tkExtends, IntPtr rtkImplements )
      {
         throw new NotImplementedException();
      }

      public void SetEventProps( uint ev, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, IntPtr rmdOtherMethods )
      {
         throw new NotImplementedException();
      }

      public uint SetPermissionSetProps( uint tk, uint dwAction, IntPtr pvPermission, uint cbPermission )
      {
         throw new NotImplementedException();
      }

      public void DefinePinvokeMap( uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL )
      {
         throw new NotImplementedException();
      }

      public void SetPinvokeMap( uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL )
      {
         throw new NotImplementedException();
      }

      public void DeletePinvokeMap( uint tk )
      {
         throw new NotImplementedException();
      }

      public uint DefineCustomAttribute( uint tkObj, uint tkType, IntPtr pCustomAttribute, uint cbCustomAttribute )
      {
         throw new NotImplementedException();
      }

      public void SetCustomAttributeValue( uint pcv, IntPtr pCustomAttribute, uint cbCustomAttribute )
      {
         throw new NotImplementedException();
      }

      public uint DefineField( uint td, string szName, uint dwFieldFlags, IntPtr pvSigBlob, uint cbSigBlob, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue )
      {
         throw new NotImplementedException();
      }

      public uint DefineProperty( uint td, string szProperty, uint dwPropFlags, IntPtr pvSig, uint cbSig, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue, uint mdSetter, uint mdGetter, IntPtr rmdOtherMethods )
      {
         throw new NotImplementedException();
      }

      public uint DefineParam( uint md, uint ulParamSeq, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue )
      {
         throw new NotImplementedException();
      }

      public void SetFieldProps( uint fd, uint dwFieldFlags, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue )
      {
         throw new NotImplementedException();
      }

      public void SetPropertyProps( uint pr, uint dwPropFlags, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue, uint mdSetter, uint mdGetter, IntPtr rmdOtherMethods )
      {
         throw new NotImplementedException();
      }

      public void SetParamProps( uint pd, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue )
      {
         throw new NotImplementedException();
      }

      public uint DefineSecurityAttributeSet( uint tkObj, IntPtr rSecAttrs, uint cSecAttrs )
      {
         throw new NotImplementedException();
      }

      public void ApplyEditAndContinue( object pImport )
      {
         throw new NotImplementedException();
      }

      public uint TranslateSigWithScope( IntPtr pAssemImport, IntPtr pbHashValue, uint cbHashValue, IMetaDataImport import, IntPtr pbSigBlob, uint cbSigBlob, IntPtr pAssemEmit, IMetaDataEmit emit, IntPtr pvTranslatedSig, uint cbTranslatedSigMax )
      {
         throw new NotImplementedException();
      }

      public void SetMethodImplFlags( uint md, uint dwImplFlags )
      {
         throw new NotImplementedException();
      }

      public void SetFieldRVA( uint fd, uint ulRVA )
      {
         throw new NotImplementedException();
      }

      public void Merge( IMetaDataImport pImport, IntPtr pHostMapToken, object pHandler )
      {
         throw new NotImplementedException();
      }

      public void MergeEnd()
      {
         throw new NotImplementedException();
      }

      #endregion

      #region IMetaDataImport Members

      public void CloseEnum( uint hEnum )
      {
         throw new NotImplementedException();
      }

      public uint CountEnum( uint hEnum )
      {
         throw new NotImplementedException();
      }

      public void ResetEnum( uint hEnum, uint ulPos )
      {
         throw new NotImplementedException();
      }

      public uint EnumTypeDefs( ref uint phEnum, uint[] rTypeDefs, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint EnumInterfaceImpls( ref uint phEnum, uint td, uint[] rImpls, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint EnumTypeRefs( ref uint phEnum, uint[] rTypeRefs, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint FindTypeDefByName( string szTypeDef, uint tkEnclosingClass )
      {
         throw new NotImplementedException();
      }

      public Guid GetScopeProps( StringBuilder szName, uint cchName, out uint pchName )
      {
         throw new NotImplementedException();
      }

      public uint GetModuleFromScope()
      {
         throw new NotImplementedException();
      }

      public uint GetTypeDefProps( uint td, IntPtr szTypeDef, uint cchTypeDef, out uint pchTypeDef, IntPtr pdwTypeDefFlags )
      {
         var tDefs = this._module.TypeDefinitions.TableContents;
         var tIdx = CILAssemblyManipulator.Physical.TableIndex.FromOneBasedToken( (Int32) td );
         CILAssemblyManipulator.Physical.TableIndex? baseType;
         if ( tIdx.Index < tDefs.Count )
         {
            var tDef = tDefs[tIdx.Index];
            pchTypeDef = PDBHelper.WriteStringUnmanaged( szTypeDef, cchTypeDef, CILAssemblyManipulator.Physical.Miscellaneous.CombineNamespaceAndType( tDef.Namespace, tDef.Name ) );
            PDBHelper.WriteInt32Unmanaged( pdwTypeDefFlags, (Int32) tDef.Attributes );
            baseType = tDef.BaseType;
         }
         else
         {
            Marshal.WriteInt16( szTypeDef, 0 );
            pchTypeDef = 1;
            baseType = null;
         }

         return (UInt32) ( baseType.HasValue ?
            baseType.Value.GetOneBasedToken() :
            0 );
      }

      public uint GetInterfaceImplProps( uint iiImpl, out uint pClass )
      {
         throw new NotImplementedException();
      }

      public uint GetTypeRefProps( uint tr, out uint ptkResolutionScope, StringBuilder szName, uint cchName )
      {
         throw new NotImplementedException();
      }

      public uint ResolveTypeRef( uint tr, ref Guid riid, out object ppIScope )
      {
         throw new NotImplementedException();
      }

      public uint EnumMembers( ref uint phEnum, uint cl, uint[] rMembers, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint EnumMembersWithName( ref uint phEnum, uint cl, string szName, uint[] rMembers, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint EnumMethods( ref uint phEnum, uint cl, IntPtr rMethods, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint EnumMethodsWithName( ref uint phEnum, uint cl, string szName, uint[] rMethods, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint EnumFields( ref uint phEnum, uint cl, IntPtr rFields, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint EnumFieldsWithName( ref uint phEnum, uint cl, string szName, uint[] rFields, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint EnumParams( ref uint phEnum, uint mb, uint[] rParams, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint EnumMemberRefs( ref uint phEnum, uint tkParent, uint[] rMemberRefs, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint EnumMethodImpls( ref uint phEnum, uint td, uint[] rMethodBody, uint[] rMethodDecl, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint EnumPermissionSets( ref uint phEnum, uint tk, uint dwActions, uint[] rPermission, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint FindMember( uint td, string szName, byte[] pvSigBlob, uint cbSigBlob )
      {
         throw new NotImplementedException();
      }

      public uint FindMethod( uint td, string szName, byte[] pvSigBlob, uint cbSigBlob )
      {
         throw new NotImplementedException();
      }

      public uint FindField( uint td, string szName, byte[] pvSigBlob, uint cbSigBlob )
      {
         throw new NotImplementedException();
      }

      public uint FindMemberRef( uint td, string szName, byte[] pvSigBlob, uint cbSigBlob )
      {
         throw new NotImplementedException();
      }

      public uint GetMethodProps( uint mb, out uint pClass, IntPtr szMethod, uint cchMethod, out uint pchMethod, IntPtr pdwAttr, IntPtr ppvSigBlob, IntPtr pcbSigBlob, IntPtr pulCodeRVA )
      {
         var mDefs = this._module.MethodDefinitions.TableContents;
         var mIdx = CILAssemblyManipulator.Physical.TableIndex.FromOneBasedToken( (Int32) mb );

         Int32 implAttrs;
         if ( mIdx.Index < mDefs.Count )
         {
            var mDef = mDefs[mIdx.Index];
            pchMethod = PDBHelper.WriteStringUnmanaged( szMethod, cchMethod, mDef.Name );
            PDBHelper.WriteInt32Unmanaged( pdwAttr, (Int32) mDef.Attributes );
            PDBHelper.WriteInt32Unmanaged( pulCodeRVA, (Int32) this._imageInfo.CLIInformation.DataReferences.GetMethodRVAs()[mIdx.Index] );
            implAttrs = (Int32) mDef.ImplementationAttributes;
            pClass = (UInt32) new CILAssemblyManipulator.Physical.TableIndex(
               CILAssemblyManipulator.Physical.Tables.TypeDef,
               this._methodDeclaringTypes[mIdx.Index] ).GetOneBasedToken();
         }
         else
         {
            Marshal.WriteInt16( szMethod, 0 );
            pchMethod = 1;
            pClass = 0;
            implAttrs = 0;
         }

         return (UInt32) implAttrs;
      }

      public uint GetMemberRefProps( uint mr, ref uint ptk, StringBuilder szMember, uint cchMember, out uint pchMember, out IntPtr ppvSigBlob )
      {
         throw new NotImplementedException();
      }

      public uint EnumProperties( ref uint phEnum, uint td, IntPtr rProperties, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint EnumEvents( ref uint phEnum, uint td, IntPtr rEvents, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint GetEventProps( uint ev, out uint pClass, StringBuilder szEvent, uint cchEvent, out uint pchEvent, out uint pdwEventFlags, out uint ptkEventType, out uint pmdAddOn, out uint pmdRemoveOn, out uint pmdFire, uint[] rmdOtherMethod, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint EnumMethodSemantics( ref uint phEnum, uint mb, uint[] rEventProp, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint GetMethodSemantics( uint mb, uint tkEventProp )
      {
         throw new NotImplementedException();
      }

      public uint GetClassLayout( uint td, out uint pdwPackSize, IntPtr rFieldOffset, uint cMax, out uint pcFieldOffset )
      {
         throw new NotImplementedException();
      }

      public uint GetFieldMarshal( uint tk, out IntPtr ppvNativeType )
      {
         throw new NotImplementedException();
      }

      public uint GetRVA( uint tk, out uint pulCodeRVA )
      {
         throw new NotImplementedException();
      }

      public uint GetPermissionSetProps( uint pm, out uint pdwAction, out IntPtr ppvPermission )
      {
         throw new NotImplementedException();
      }

      public uint GetSigFromToken( uint mdSig, out IntPtr ppvSig )
      {
         throw new NotImplementedException();
      }

      public uint GetModuleRefProps( uint mur, StringBuilder szName, uint cchName )
      {
         throw new NotImplementedException();
      }

      public uint EnumModuleRefs( ref uint phEnum, uint[] rModuleRefs, uint cmax )
      {
         throw new NotImplementedException();
      }

      public uint GetTypeSpecFromToken( uint typespec, out IntPtr ppvSig )
      {
         throw new NotImplementedException();
      }

      public uint GetNameFromToken( uint tk )
      {
         throw new NotImplementedException();
      }

      public uint EnumUnresolvedMethods( ref uint phEnum, uint[] rMethods, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint GetUserString( uint stk, StringBuilder szString, uint cchString )
      {
         throw new NotImplementedException();
      }

      public uint GetPinvokeMap( uint tk, out uint pdwMappingFlags, StringBuilder szImportName, uint cchImportName, out uint pchImportName )
      {
         throw new NotImplementedException();
      }

      public uint EnumSignatures( ref uint phEnum, uint[] rSignatures, uint cmax )
      {
         throw new NotImplementedException();
      }

      public uint EnumTypeSpecs( ref uint phEnum, uint[] rTypeSpecs, uint cmax )
      {
         throw new NotImplementedException();
      }

      public uint EnumUserStrings( ref uint phEnum, uint[] rStrings, uint cmax )
      {
         throw new NotImplementedException();
      }

      public int GetParamForMethodIndex( uint md, uint ulParamSeq, out uint pParam )
      {
         throw new NotImplementedException();
      }

      public uint EnumCustomAttributes( ref uint phEnum, uint tk, uint tkType, uint[] rCustomAttributes, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint GetCustomAttributeProps( uint cv, out uint ptkObj, out uint ptkType, out IntPtr ppBlob )
      {
         throw new NotImplementedException();
      }

      public uint FindTypeRef( uint tkResolutionScope, string szName )
      {
         throw new NotImplementedException();
      }

      public uint GetMemberProps( uint mb, out uint pClass, StringBuilder szMember, uint cchMember, out uint pchMember, out uint pdwAttr, out IntPtr ppvSigBlob, out uint pcbSigBlob, out uint pulCodeRVA, out uint pdwImplFlags, out uint pdwCPlusTypeFlag, out IntPtr ppValue )
      {
         throw new NotImplementedException();
      }

      public uint GetFieldProps( uint mb, out uint pClass, StringBuilder szField, uint cchField, out uint pchField, out uint pdwAttr, out IntPtr ppvSigBlob, out uint pcbSigBlob, out uint pdwCPlusTypeFlag, out IntPtr ppValue )
      {
         throw new NotImplementedException();
      }

      public uint GetPropertyProps( uint prop, out uint pClass, StringBuilder szProperty, uint cchProperty, out uint pchProperty, out uint pdwPropFlags, out IntPtr ppvSig, out uint pbSig, out uint pdwCPlusTypeFlag, out IntPtr ppDefaultValue, out uint pcchDefaultValue, out uint pmdSetter, out uint pmdGetter, uint[] rmdOtherMethod, uint cMax )
      {
         throw new NotImplementedException();
      }

      public uint GetParamProps( uint tk, out uint pmd, out uint pulSequence, StringBuilder szName, uint cchName, out uint pchName, out uint pdwAttr, out uint pdwCPlusTypeFlag, out IntPtr ppValue )
      {
         throw new NotImplementedException();
      }

      public uint GetCustomAttributeByName( uint tkObj, string szName, out IntPtr ppData )
      {
         throw new NotImplementedException();
      }

      public bool IsValidToken( uint tk )
      {
         throw new NotImplementedException();
      }

      public uint GetNestedClassProps( uint tdNestedClass )
      {
         var nestedIdx = CILAssemblyManipulator.Physical.TableIndex.FromOneBasedToken( (Int32) tdNestedClass );
         Int32 enclosingTypeIdx;
         return (UInt32) ( this._typeEnclosingTypes.TryGetValue( nestedIdx.Index, out enclosingTypeIdx ) ?
            new CILAssemblyManipulator.Physical.TableIndex( CILAssemblyManipulator.Physical.Tables.TypeDef, enclosingTypeIdx ).GetOneBasedToken() :
            0 );
      }

      public uint GetNativeCallConvFromSig( IntPtr pvSig, uint cbSig )
      {
         throw new NotImplementedException();
      }

      public int IsGlobal( uint pd )
      {
         throw new NotImplementedException();
      }

      #endregion
   }

   #endregion

   #region PDB Reading

   internal class SymUnmanagedReaderHolder : AbstractDisposable
   {
      public SymUnmanagedReaderHolder(
         String mdFile,
         CILAssemblyManipulator.Physical.CILMetaData module,
         ImageInformation imageInfo
         )
      {
         this.Reader = (ISymUnmanagedReader) Activator.CreateInstance( Type.GetTypeFromCLSID( new Guid( "0A3976C5-4529-4ef8-B0B0-42EED37082CD" ) ) );
         this.Reader.Initialize(
            new MDHelper( module, imageInfo ),
            mdFile,
            null,
            null
            );
      }

      ~SymUnmanagedReaderHolder()
      {
         this.Dispose( false );
      }

      public ISymUnmanagedReader Reader { get; }

      protected override void Dispose( Boolean disposing )
      {
         Marshal.ReleaseComObject( this.Reader );
      }
   }

   #endregion

   #region Generic COM stuff
   public class COMStreamWrapper : ct.IStream
   {

      private readonly Stream _stream;

      public COMStreamWrapper( Stream stream )
      {
         this._stream = ArgumentValidator.ValidateNotNull( "Stream", stream );
      }


      public void Read( Byte[] pv, int cb, System.IntPtr pcbRead )
      {
         Marshal.WriteInt32( pcbRead, (Int32) _stream.Read( pv, 0, cb ) );
      }

      public void Seek( Int64 dlibMove, Int32 dwOrigin, System.IntPtr plibNewPosition )
      {
         var newPos = this._stream.Seek( dlibMove, (SeekOrigin) dwOrigin );
         Marshal.WriteInt64( plibNewPosition, newPos ); // .WriteInt32( plibNewPosition, newPos );
      }

      public void Write( byte[] pv, int cb, IntPtr pcbWritten )
      {
         this._stream.Write( pv, cb );
         Marshal.WriteInt32( pcbWritten, cb );
      }

      public void SetSize( long libNewSize )
      {
         this._stream.SetLength( libNewSize );
      }

      public void CopyTo( ct.IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten )
      {
         throw new NotImplementedException();
      }

      public void Commit( int grfCommitFlags )
      {
         if ( grfCommitFlags != 0 )
         {
            throw new NotImplementedException();
         }
      }

      public void Revert()
      {
         throw new NotImplementedException();
      }

      public void LockRegion( long libOffset, long cb, int dwLockType )
      {
         throw new NotImplementedException();
      }

      public void UnlockRegion( long libOffset, long cb, int dwLockType )
      {
         throw new NotImplementedException();
      }

      public void Stat( out ct.STATSTG pstatstg, int grfStatFlag )
      {
         if ( grfStatFlag != 1 )
         {
            throw new NotImplementedException();
         }
         pstatstg = new ct.STATSTG()
         {
            cbSize = this._stream.Length,
            type = 2, // STGTY_STREAM
            grfMode = 0x00000002, // STGM_READWRITE
         };

      }

      public void Clone( out ct.IStream ppstm )
      {
         throw new NotImplementedException();
      }
   }

   #endregion
}

public static partial class E_CILPhysicalTests
{
   internal static void ProcessPDB( this SymUnmanagedWriterHolder holder, PDBInstance pdb )
   {
      var writer = holder.Writer;

      foreach ( var func in pdb.Modules.SelectMany( m => m.Functions ) )
      {
         writer.OpenMethod( new SymbolToken( (Int32) func.Token ) );


         // 1. Handle OEM stuff
         func.ProcessAsyncInfo( writer );
         func.HandleAllOEM( writer );

         // 2. Handle lines
         var lineDic = new Dictionary<ISymUnmanagedDocumentWriter, List<PDBLine>>( ReferenceEqualityComparer<ISymUnmanagedDocumentWriter>.ReferenceBasedComparer );
         foreach ( var line in func.Lines )
         {
            lineDic
               .GetOrAdd_NotThreadSafe( holder.GetDocumentWriterFor( line.Source ), s => new List<PDBLine>() )
               .Add( line );
         }

         foreach ( var kvp in lineDic )
         {
            var src = kvp.Key;
            var lines = kvp.Value;
            writer.DefineSequencePoints(
               src,
               lines.Count,
               lines.Select( l => l.Offset ).ToArray(),
               lines.Select( l => l.LineStart ).ToArray(),
               lines.Select( l => (Int32) l.ColumnStart.Value ).ToArray(),
               lines.Select( l => l.LineEnd ).ToArray(),
               lines.Select( l => (Int32) l.ColumnEnd.Value ).ToArray() );
         }

         // 3. Handle the scopes
         // Internally, each function has a root scope, which is visible as one PDBScope in PDBFunction.Scopes
         var scopes = func.Scopes;
         Int32 dummy;
         writer.OpenScope( 0, out dummy );
         if ( scopes.Count > 0 )
         {
            writer.ProcessPDBScope( scopes[0], 0, func.Length );
         }
         writer.CloseScope( func.Length );

         // 4. We're done
         writer.CloseMethod();
      }

   }

   internal static void ProcessAsyncInfo( this PDBFunction func, ISymUnmanagedWriter2 writer )
   {
      var asyncInfo = func.AsyncMethodInfo;
      if ( asyncInfo != null )
      {
         var asyncWriter = (ISymUnmanagedAsyncMethodPropertiesWriter) writer;
         throw new NotImplementedException();
      }
   }

   internal static void ProcessPDBScope( this ISymUnmanagedWriter2 writer, PDBScope scp, Int32 startOffset, Int32 endOffset )
   {
      foreach ( var slot in scp.Slots )
      {
         // For some reason, even if correct .Flags are given to the local variable, the flags are not persisted. I don't know why.
         // This causes compiler-generated locals to show up in 'Locals' when debugging the code.
         writer.DefineLocalVariable2(
            slot.Name,
            (Int32) slot.Flags,
            new SymbolToken( (Int32) slot.TypeToken ),
            (Int32) SymAddressKind.ILOffset,
            slot.SlotIndex,
            0,
            0,
            startOffset,
            endOffset
            );
      }

      foreach ( var un in scp.UsedNamespaces )
      {
         writer.UsingNamespace( un );
      }

      foreach ( var constant in scp.Constants )
      {
         writer.DefineConstant2( constant.Name, constant.Value, new SymbolToken( (Int32) constant.Token ) );
      }

      foreach ( var ss in scp.Scopes )
      {
         Int32 dummy;
         writer.OpenScope( ss.Offset, out dummy );
         writer.ProcessPDBScope( ss, ss.Offset, ss.Offset + ss.Length );
         writer.CloseScope( ss.Offset + ss.Length );
      }


   }

   private delegate Int32 OEMHandlerWriterSizeDelegate( PDBFunction func );
   private delegate void OEMHandlerWriterDelegate( PDBFunction func, Byte[] array, ref Int32 idx );

   private static void HandleAllOEM( this PDBFunction func, ISymUnmanagedWriter2 writer )
   {
      func.HandleCustomOEM( writer, PDBIO.ENC_OEM_NAME, CAMPhysicalPIO::E_CILPhysical.CalculateByteCountENCInfo, CAMPhysicalPIO::E_CILPhysical.WriteOEMENCID );
      var nsList = new List<Int32>();
      func.HandleCustomOEM( writer, PDBIO.MD_OEM_NAME, f =>
      {
         CAMPhysicalPIO::E_CILPhysical.CalculateByteCountFromLists( f, nsList );
         return CAMPhysicalPIO::E_CILPhysical.CalculateByteCountMD2Info( f, ref nsList );
      },
      ( PDBFunction f, Byte[] array, ref Int32 idx ) => CAMPhysicalPIO::E_CILPhysical.WriteOEMMD2( f, array, ref idx, nsList )
      );
   }

   private static void HandleCustomOEM( this PDBFunction func, ISymUnmanagedWriter2 writer, String oemName, OEMHandlerWriterSizeDelegate size, OEMHandlerWriterDelegate writerAction )
   {
      var bytez = new Byte[size( func )];
      if ( bytez.Length > 0 )
      {
         var idx = 0;
         writerAction( func, bytez, ref idx );
         var actualBytez = bytez.Skip( CAMPhysicalPIO::E_CILPhysical.GetFixedOEMSize( oemName ) ).ToArray();
         writer.SetSymAttribute( new SymbolToken( (Int32) func.Token ), oemName, actualBytez.Length, actualBytez );
      }
   }


   // Not set: PDBScope.Name, PDBFunction.Slots (but PDBScope.Slots are ok), PDBSlot.TypeToken, PDBConstant.Token

   internal static PDBInstance CreateInstanceFromNativeReader(
      this ISymUnmanagedReader reader,
      CILAssemblyManipulator.Physical.CILMetaData md,
      out Int32? ep
      )
   {
      var retVal = new PDBInstance();

      Int32 epToken;
      ep = reader.GetUserEntryPoint( out epToken ) == 0 ? epToken : (Int32?) null;
      var typeDefs = md.TypeDefinitions.TableContents;
      var methods = md.MethodDefinitions.TableContents;

      var sources = new Dictionary<String, PDBSource>();

      var curTDef = 0;
      foreach ( var fullName in md.GetTypeDefinitionsFullNames() )
      {
         var module = new PDBModule()
         {
            Name = fullName.Replace( '+', '.' ) // PDB stores class names with only dot separators.
         };
         foreach ( var i in md.GetTypeMethodIndices( curTDef ) )
         {
            var token = new CILAssemblyManipulator.Physical.TableIndex( CILAssemblyManipulator.Physical.Tables.MethodDef, i ).GetOneBasedToken();
            ISymUnmanagedMethod theMethod = null;
            reader.GetMethod( token, out theMethod );

            UseCOMObject( theMethod, method =>
            {
               method.GetToken( out token );
               var func = new PDBFunction()
               {
                  AsyncMethodInfo = method.CreateAsyncInfo(),
                  Name = methods[i].Name,
                  Token = (UInt32) token
               };

               func.HandleAllOEM( reader );

               ISymUnmanagedScope scope;
               method.GetRootScope( out scope );
               Int32 length;
               UseCOMObject( scope, s =>
               {
                  ( (ISymUnmanagedScope2) s ).CreateScopeFromNativeScope( func );
               } );

               // Lines
               method.GetSequencePointCount( out length );
               var offsets = new Int32[length];
               var docs = new ISymUnmanagedDocument[length];
               var lines = new Int32[length];
               var cols = new Int32[length];
               var endLines = new Int32[length];
               var endCols = new Int32[length];
               method.GetSequencePoints( length, out length, offsets, docs, lines, cols, endLines, endCols );
               docs.UseCOMObjectArray( ( doc, j ) =>
               {
                  Int32 urlCount;
                  doc.GetUrl( 0, out urlCount, null );
                  var charz = new Char[urlCount];
                  doc.GetUrl( urlCount, out urlCount, charz );
                  func.Lines.Add( new PDBLine()
                  {
                     LineStart = (Int32) lines[j],
                     LineEnd = (Int32) endLines[j],
                     ColumnStart = (UInt16) cols[j],
                     ColumnEnd = (UInt16) endCols[j],
                     Offset = (Int32) offsets[j],
                     Source = sources.GetOrAdd_NotThreadSafe( charz.CreateStringFromCOMChars(), url => doc.CreateSource( url ) ),
                     //IsStatement = ( lines[j] == 0x00FEEFEE ) // IsHidden... ?
                  } );
               } );


               // Slots (are not exposed... ?)

               // Used namespaces are not exposed
               module.Functions.Add( func );
            } );
         }

         if ( module.Functions.Count > 0 )
         {
            retVal.Modules.Add( module );
         }

         ++curTDef;
      }

      return retVal;
   }


   private static void CreateScopeFromNativeScope( this ISymUnmanagedScope2 scope, PDBScopeOrFunction retVal )
   {
      Int32 tmp;
      var curScope = retVal as PDBScope;
      Int32 start;
      if ( curScope != null )
      {
         scope.GetStartOffset( out tmp );
         curScope.Offset = (Int32) tmp;
         start = curScope.Offset;
      }
      else
      {
         start = 0;
      }
      scope.GetEndOffset( out tmp );
      retVal.Length = (Int32) tmp - start;

      // Used namespaces
      scope.GetNamespaces( 0, out tmp, null );
      var nss = new ISymUnmanagedNamespace[tmp];
      scope.GetNamespaces( tmp, out tmp, nss );
      nss.UseCOMObjectArray( ns =>
      {
         ns.GetName( 0, out tmp, null );
         var charz = new Char[tmp];
         ns.GetName( tmp, out tmp, charz );
         retVal.UsedNamespaces.Add( charz.CreateStringFromCOMChars() );
      } );

      // Locals
      scope.GetLocals( 0, out tmp, null );
      var locals = new ISymUnmanagedVariable[tmp];
      scope.GetLocals( tmp, out tmp, locals );
      locals.UseCOMObjectArray( local =>
      {
         var slot = new PDBSlot();
         local.GetAttributes( out tmp );
         slot.Flags = (PDBSlotFlags) tmp;
         local.GetName( 0, out tmp, null );
         var chars = new Char[tmp];
         local.GetName( tmp, out tmp, chars );
         slot.Name = new String( chars.Take( (Int32) tmp - 1 ).ToArray() );
         local.GetAddressField1( out tmp );
         slot.SlotIndex = (Int32) tmp;
         retVal.Slots.Add( slot );
      } );

      // Constants
      scope.GetConstants( 0, out tmp, null );
      var consts = new ISymUnmanagedConstant[tmp];
      scope.GetConstants( tmp, out tmp, consts );
      consts.UseCOMObjectArray( aConst =>
      {
         var pdbConst = new PDBConstant();
         aConst.GetName( 0, out tmp, null );
         var chars = new Char[tmp];
         aConst.GetName( tmp, out tmp, chars );
         pdbConst.Name = chars.CreateStringFromCOMChars();
         Object val;
         aConst.GetValue( out val );
         pdbConst.Value = val;
         retVal.Constants.Add( pdbConst );
      } );

      // Sub-scopes
      scope.GetChildren( 0, out tmp, null );
      var children = new ISymUnmanagedScope[tmp];
      scope.GetChildren( tmp, out tmp, children );
      children.UseCOMObjectArray( child =>
      {
         var childScope = new PDBScope();
         ( (ISymUnmanagedScope2) child ).CreateScopeFromNativeScope( childScope );
         retVal.Scopes.Add( childScope );
      } );

   }

   private static PDBSource CreateSource( this ISymUnmanagedDocument doc, String url )
   {
      var src = new PDBSource()
      {
         Name = url
      };
      Guid guid = default( Guid );
      doc.GetDocumentType( ref guid );
      src.DocumentType = guid;
      doc.GetLanguage( ref guid );
      src.Language = guid;
      doc.GetLanguageVendor( ref guid );
      src.Vendor = guid;
      doc.GetChecksumAlgorithmId( ref guid );
      src.HashAlgorithm = guid;
      Int32 count;
      doc.GetChecksum( 0, out count, null );
      src.Hash = new Byte[count];
      doc.GetChecksum( count, out count, src.Hash );
      return src;
   }

   private static PDBAsyncMethodInfo CreateAsyncInfo( this ISymUnmanagedMethod method )
   {
      var asyncInfo = (ISymUnmanagedAsyncMethod) method;

      Int32 tmp;
      asyncInfo.GetAsyncStepInfoCount( out tmp );
      PDBAsyncMethodInfo retVal;
      if ( tmp == 0 )
      {
         retVal = null;
      }
      else
      {
         retVal = new PDBAsyncMethodInfo();
         var yieldOffsets = new Int32[tmp];
         var breakpointOffsets = new Int32[tmp];
         var breakpointMethods = new Int32[tmp];
         asyncInfo.GetAsyncStepInfo( tmp, out tmp, yieldOffsets, breakpointOffsets, breakpointMethods );
         for ( var i = 0; i < yieldOffsets.Length; ++i )
         {
            retVal.SynchronizationPoints.Add( new PDBSynchronizationPoint()
            {
               SyncOffset = yieldOffsets[i],
               ContinuationOffset = breakpointOffsets[i],
               ContinuationMethodToken = (UInt32) breakpointMethods[i]
            } );
         }

         asyncInfo.GetCatchHandlerILOffset( out tmp );
         retVal.CatchHandlerOffset = tmp;

         asyncInfo.GetKickoffMethod( out tmp );
         retVal.KickoffMethodToken = (UInt32) tmp;
      }

      return retVal;
   }

   private delegate void OEMHandlerReaderDelegate( PDBFunction func, Byte[] array, ref Int32 idx );

   private static void HandleAllOEM( this PDBFunction func, ISymUnmanagedReader reader )
   {
      HandleCustomOEM( func, reader, PDBIO.ENC_OEM_NAME, PDBIO.HandleENCOEM );
      HandleCustomOEM( func, reader, PDBIO.MD_OEM_NAME, PDBIO.HandleMD2OEM );
   }

   private static void HandleCustomOEM( this PDBFunction func, ISymUnmanagedReader reader, String oemName, OEMHandlerReaderDelegate handler )
   {
      Int32 tmp;
      reader.GetSymAttribute( (Int32) func.Token, oemName, 0, out tmp, null );
      if ( tmp > 0 )
      {
         var bytez = new Byte[tmp];
         reader.GetSymAttribute( (Int32) func.Token, oemName, tmp, out tmp, bytez );
         var idx = 0;
         handler( func, bytez, ref idx );
      }
   }

   private static void UseCOMObject<T>( T comObj, Action<T> action )
      where T : class
   {
      if ( comObj != null )
      {
         try
         {
            action( comObj );
         }
         finally
         {
            Marshal.ReleaseComObject( comObj );
         }
      }
   }
   private static void UseCOMObjectArray<T>( this T[] array, Action<T> action )
      where T : class
   {
      array.UseCOMObjectArray( ( elem, idx ) => action( elem ) );
   }

   private static void UseCOMObjectArray<T>( this T[] array, Action<T, Int32> action )
      where T : class
   {
      try
      {
         for ( var i = 0; i < array.Length; ++i )
         {
            action( array[i], i );
         }
      }
      finally
      {
         foreach ( var elem in array )
         {
            try
            {
               Marshal.ReleaseComObject( elem );
            }
            catch
            {
               // Ignore
            }
         }
      }
   }

   private static String CreateStringFromCOMChars( this Char[] chars )
   {
      // Remember terminating zero
      return new String( chars.Take( chars.Length - 1 ).ToArray() );
   }
}