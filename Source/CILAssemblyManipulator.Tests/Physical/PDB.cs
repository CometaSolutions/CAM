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

using CAMPhysicalP;
using CAMPhysicalP::CILAssemblyManipulator.Physical.PDB;
using CAMPhysicalP::CILAssemblyManipulator.Physical;

using CAMPhysicalIO;

using CAMPhysicalIOD::CILAssemblyManipulator.Physical.IO;

using CILAssemblyManipulator.Physical.PDB;
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
using System.Runtime.InteropServices.ComTypes;
using CILAssemblyManipulator.Tests.Physical;

namespace CILAssemblyManipulator.Tests.Physical
{
   [Category( "CAM.Physical" )]
   public class PDBTest : AbstractCAMTest
   {
      [Test]
      public void TestPDB()
      {
         PerformPDBTest(
            Path.Combine( Path.GetDirectoryName( CILMergeLocation ), "CILAssemblyManipulator.Physical.PDB.IO.pdb" )
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
         Int32? ep;
         using ( var readerHolder = new SymUnmanagedReaderHolder( mdFile, module, readingArgs.ImageInformation ) )
         {
            pdb2 = readerHolder.Reader.CreateInstanceFromNativeReader( module, out ep );
         }
         // TODO this will throw, since native API does not expose everything needed...
         Assert.IsTrue( Comparers.PDBInstanceEqualityComparer.Equals( pdb, pdb2 ), "PDB instance created from native reader must equal the one read from .pdb file." );

         // Test writing
         Byte[] bytez;
         using ( var mem = new MemoryStream() )
         {
            pdb.WriteToStream( mem, 0x06000001 );
            bytez = mem.ToArray();
         }

         // Test that reading the written file results in same PDB
         using ( var mem = new MemoryStream( bytez ) )
         {
            pdb2 = mem.ReadPDBInstance();
         }
         Assert.IsTrue( Comparers.PDBInstanceEqualityComparer.Equals( pdb, pdb2 ), "PDB after writing and reading must still have same content." );

         // We must write the bytes by original file, since the reference to that file is in the meta-data information.
         //File.Move( pdbFile, Path.ChangeExtension( pdbFile, ".original.pdb" ) );
         File.WriteAllBytes( pdbFile, bytez );


      }
   }



   internal class PDBHelper
   {




      internal const String SYM_WRITER_GUID_STR = "0B97726E-9E6D-4f05-9A26-424022093CAA";
      internal const String SYM_READER_GUID_STR = "B4CE6286-2A6B-3712-A3B7-1EE1DAD467B5"; // "A09E53B2-2A57-4cca-8F63-B84F7C35D4AA";
      internal const String DOC_WRITER_GUID_STR = "B01FAFEB-C450-3A4D-BEEC-B4CEEC01E006";
      internal const String UNMANAGED_WRITER_GUID_STR = "108296C1-281E-11D3-BD22-0000F80849BD"; // SxS version: 0AE2DEB0-F901-478b-BB9F-881EE8066788
      internal const String UNMANAGED_READER_GUID_STR = "0A3976C5-4529-4ef8-B0B0-42EED37082CD"; // SxS version: 0A3976C5-4529-4ef8-B0B0-42EED37082CD, deprectated: 108296C2-281E-11d3-BD22-0000F80849BD
      internal const String METADATA_EMIT_GUID_STR = "BA3FEE4C-ECB9-4e41-83B7-183FA41CD859";
      internal const String METADATA_IMPORT_GUID_STR = "7DAC8207-D3AE-4c75-9B67-92801A497D44";




      internal static Guid UNMANAGED_WRITER_GUID = new Guid( UNMANAGED_WRITER_GUID_STR );
      internal static Guid UNMANAGED_READER_GUID = new Guid( UNMANAGED_READER_GUID_STR );
      internal static Guid SYM_WRITER_GUID = new Guid( SYM_WRITER_GUID_STR );
      internal static Guid SYM_READER_GUID = new Guid( SYM_READER_GUID_STR );

      [DllImport( "ole32.dll" )]
      public static extern int CoCreateInstance(
         [In] ref Guid rclsid,
         [In, MarshalAs( UnmanagedType.IUnknown )] Object pUnkOuter,
         [In] uint dwClsContext,
         [In] ref Guid riid,
         [Out, MarshalAs( UnmanagedType.Interface )] out Object ppv
         );

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
         String fn,
         String tmpFN = null
         )
      {
         this._imageInfo = imageInfo;
         this._fn = fn;
         this._tmpFN = tmpFN;

         Object instance;
         PDBHelper.CoCreateInstance( ref PDBHelper.UNMANAGED_WRITER_GUID, null, 1u, ref PDBHelper.SYM_WRITER_GUID, out instance );
         var writer = (ISymUnmanagedWriter2) instance;
         // Initialize writer
         var emitter = new MDHelper( module, imageInfo );
         if ( !String.IsNullOrEmpty( tmpFN ) )
         {
            writer.Initialize2(
               emitter, // Emitter
               tmpFN, // Temporary file name
               null, // IStream
               true, // isFullBuild
               fn // Final file name
               );
         }
         else
         {
            writer.Initialize(
               emitter, // Emitter
               fn, // Final file name
               null, // IStream
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
            // Copy tmp-PDB to new location
            var tmpFN = this._tmpFN;
            if ( !String.IsNullOrEmpty( tmpFN ) )
            {
               System.IO.File.Copy( this._tmpFN, this._fn, true );
            }
         }
      }
   }


   // See http://msdn.microsoft.com/en-us/library/ms231406%28v=vs.110%29.aspx
   [Guid( PDBHelper.SYM_WRITER_GUID_STR ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown ), ComImport]
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
         [In] IStream pIStream,
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
         [In] IStream pIStream,
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

   [Guid( PDBHelper.DOC_WRITER_GUID_STR ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown ), ComImport]
   internal interface ISymUnmanagedDocumentWriter
   {
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

   [ComImport, InterfaceType( ComInterfaceType.InterfaceIsIUnknown ), Guid( PDBHelper.METADATA_EMIT_GUID_STR )]
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

   [ComImport, InterfaceType( ComInterfaceType.InterfaceIsIUnknown ), Guid( PDBHelper.METADATA_IMPORT_GUID_STR )]
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
            pchTypeDef = PDBHelper.WriteStringUnmanaged( szTypeDef, cchTypeDef, tDef.Name );
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
      internal SymUnmanagedReaderHolder(
         String mdFile,
         CILAssemblyManipulator.Physical.CILMetaData module,
         ImageInformation imageInfo
         )
      {
         //var reader = (ISymUnmanagedReader) ( new CorSymReader_SxS() );
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

   [Guid( "0A3976C5-4529-4ef8-B0B0-42EED37082CD" ), ComImport,]
   internal class CorSymReader_SxS
   {

   }

   //[Guid( PDBHelper.SYM_READER_GUID_STR ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown ), ComImport]
   [
      //Guid( "B4CE6286-2A6B-3712-A3B7-1EE1DAD467B5" ),
      Guid( PDBHelper.SYM_READER_GUID_STR ),
      InterfaceType( ComInterfaceType.InterfaceIsIUnknown ),
      CoClass( typeof( CorSymReader_SxS ) ),
      ComImport
   ]
   public interface ISymUnmanagedReader
   {
      /*
     * Find a document. Language, vendor, and document type are optional.
     */
      void GetDocument( [In, MarshalAs( UnmanagedType.LPWStr )] String url,
                        [In] Guid language,
                        [In] Guid languageVendor,
                        [In] Guid documentType,
                        [Out, MarshalAs( UnmanagedType.Interface )] out ISymUnmanagedDocument pRetVal
                        );

      /*
       * Return an array of all the documents defined in the symbol store.
       */
      void GetDocuments( [In] UInt32 cDocs,
                         [Out] out UInt32 pcDocs,
                         [In, Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0, ArraySubType = UnmanagedType.Interface )] ISymUnmanagedDocument[] pDocs
                        );

      /*
       * Return the method that was specified as the user entry point
       * for the module, if any. This would be, perhaps, the user's main
       * method rather than compiler generated stubs before main.
       */
      void GetUserEntryPoint( [Out] out SymbolToken pToken );

      /*
       * Get a symbol reader method given a method token.
       */
      void GetMethod( [In] SymbolToken token,
                      [Out, MarshalAs( UnmanagedType.Interface )] out ISymUnmanagedMethod pRetVal
                    );

      /*
       * Get a symbol reader method given a method token and an E&C
       * version number. Version numbers start at 1 and are incremented
       * each time the method is changed due to an E&C operation.
       */
      void GetMethodByVersion( [In] SymbolToken token,
                               [In] Int32 version,
                               [Out, MarshalAs( UnmanagedType.Interface )] out ISymUnmanagedMethod pRetVal
                              );

      /*
       * Return a non-local variable given its parent and name.
       */
      void GetVariables( [In] SymbolToken parent,
                         [In] UInt32 cVars,
                         [Out] out UInt32 pcVars,
                         [In, Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 1, ArraySubType = UnmanagedType.Interface )] ISymUnmanagedVariable[] pVars
                       );
      /*
       * Return all global variables.
       */
      void GetGlobalVariables( [In] UInt32 cVars,
                               [Out] out UInt32 pcVars,
                               [In, Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0, ArraySubType = UnmanagedType.Interface )] ISymUnmanagedVariable[] pVars
                             );

      /*
       * Given a position in a document, return the ISymUnmanagedMethod that
       * contains that position.
       */
      void GetMethodFromDocumentPosition( [In, MarshalAs( UnmanagedType.Interface )] ISymUnmanagedDocument document,
                                            [In] UInt32 line,
                                            [In] UInt32 column,
                                            [Out, MarshalAs( UnmanagedType.Interface )] out ISymUnmanagedMethod pRetVal );

      /*
       * Gets a custom attribute based upon its name. Not to be
       * confused with Metadata custom attributes, these attributes are
       * held in the symbol store.
       */
      void GetSymAttribute( [In] SymbolToken parent,
                              [In, MarshalAs( UnmanagedType.LPWStr )] String name,
                              [In] UInt32 cBuffer,
                              [Out] out UInt32 pcBuffer,
                              [In, Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 2 )] Byte[] buffer
                          );

      /*
       * Get the namespaces defined at global scope within this symbol store.
       */
      void GetNamespaces( [In] UInt32 cNameSpaces,
                          [Out] out UInt32 pcNameSpaces,
                          [In, Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0, ArraySubType = UnmanagedType.Interface )] ISymUnmanagedNamespace[] namespaces
                        );

      /*
       * Initialize the symbol reader with the metadata importer interface
       * that this reader will be associated with, along with the filename
       * of the module. This can only be called once, and must be called
       * before any other reader methods are called.
       *
       * Note: you need only specify one of the filename or the pIStream,
       * not both. The searchPath parameter is optional.
       */
      void Initialize( [In, MarshalAs( UnmanagedType.IUnknown )] object importer,
                       [In, MarshalAs( UnmanagedType.LPWStr )] String filename,
                       [In, MarshalAs( UnmanagedType.LPWStr )] String searchPath,
                       [In, MarshalAs( UnmanagedType.Interface )] IStream pIStream
                      );

      /*
       * Update the existing symbol reader with a delta symbol store. This
       * is used in EnC scenarios as a way to update the symbol store to
       * match deltas to the original PE file.
       *
       * Only one of the filename or pIStream parameters need be specified.
       * If a filename is specified, the symbol store will be updated with
       * the symbols in that file. If a IStream is specified, the store will
       * be updated with the data from the IStream.
       */
      void UpdateSymbolStore( [In, MarshalAs( UnmanagedType.LPWStr )] String filename,
                              [In, MarshalAs( UnmanagedType.Interface )] IStream pIStream
                            );

      /*
       * Update the existing symbol reader with a delta symbol
       * store. This is much like UpdateSymbolStore, but the given detla
       * acts as a complete replacement rather than an update.
       *
       * Only one of the filename or pIStream parameters need be specified.
       * If a filename is specified, the symbol store will be updated with
       * the symbols in that file. If a IStream is specified, the store will
       * be updated with the data from the IStream.
       */
      void ReplaceSymbolStore( [In, MarshalAs( UnmanagedType.LPWStr )] String filename,
                               [In, MarshalAs( UnmanagedType.Interface )] IStream pIStream
                             );

      /*
       * Provides the on disk filename of the symbol store.
       */

      void GetSymbolStoreFileName( [In] UInt32 cchName,
                                   [Out] out UInt32 pcchName,
                                   [In, Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0 )] Char[] szName
                                 );

      /*
       * Given a position in a document, return the ISymUnmanagedMethods that
       * contains that position.
       */
      void GetMethodsFromDocumentPosition( [In, MarshalAs( UnmanagedType.Interface )] ISymUnmanagedDocument document,
                                           [In] UInt32 line,
                                           [In] UInt32 column,
                                           [In] UInt32 cMethod,
                                           [Out] out UInt32 pcMethod,
                                           [In, Out, MarshalAs( UnmanagedType.LPWStr, SizeParamIndex = 3, ArraySubType = UnmanagedType.Interface )] ISymUnmanagedMethod[] pRetVal
                                          );

      /*
       * Get the given version of the given document.
      * The document version starts at 1 and is incremented each time
       * the document is updated via UpdateSymbols.
       * bCurrent is true is this is the latest version of the document.
       */
      void GetDocumentVersion( [In, MarshalAs( UnmanagedType.Interface )] ISymUnmanagedDocument pDoc,
                               [Out] out Int32 version,
                               [Out, MarshalAs( UnmanagedType.Bool )] out Boolean pbCurrent
                             );

      /*
       * The method version starts at 1 and is incremented each time
       * the method is recompiled.  (This can happen without changes to the method.)
       */
      void GetMethodVersion( [In, MarshalAs( UnmanagedType.Interface )] ISymUnmanagedMethod pMethod,
                             [Out] out Int32 version
                           );

      ///////////////////////////////
      // ISymUnamangedReader2 methods
      ///////////////////////////////

      ///*
      // * Get a symbol reader method given a method token and an E&C
      // * version number. Version numbers start at 1 and are incremented
      // * each time the method is changed due to an E&C operation.
      // */
      //void GetMethodByVersionPreRemap( [In] SymbolToken token,
      //                                 [In] Int32 version,
      //                                 [Out, MarshalAs( UnmanagedType.Interface )] out ISymUnmanagedMethod pRetVal
      //                                );
      ///*
      // * Gets a custom attribute based upon its name. Not to be
      // * confused with Metadata custom attributes, these attributes are
      // * held in the symbol store.
      // */
      //void GetSymAttributePreRemap( [In] SymbolToken parent,
      //                              [In, MarshalAs( UnmanagedType.LPWStr )] String name,
      //                              [In] UInt32 cBuffer,
      //                              [Out] out UInt32 pcBuffer,
      //                              [In, Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 2 )] Byte[] buffer
      //                            );

      ///*
      // * Gets every method that has line information in the provided Document.  
      // */
      //void GetMethodsInDocument( [In, MarshalAs( UnmanagedType.Interface )] ISymUnmanagedDocument document,
      //                           [In] UInt32 cMethod,
      //                           [Out] out UInt32 pcMethod,
      //                           [In, Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 1, ArraySubType = UnmanagedType.Interface )] ISymUnmanagedMethod[] pRetVal
      //                         );
   }

   [Guid( "40DE4037-7C81-3E1E-B022-AE1ABFF2CA08" ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown ), ComImport]
   public interface ISymUnmanagedDocument
   {
      /*
 * Return the URL for this document.
 */
      void GetURL( [In] UInt32 cchUrl,
                   [Out] out UInt32 pcchUrl,
                   [In, Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0 )] Char[] szUrl
                 );

      /*
       * Get the document type of this document.
       */
      void GetDocumentType( [Out] out Guid pRetVal );

      /*
       * Get the language id of this document.
       */
      void GetLanguage( [Out] out Guid pRetVal );

      /*
       * Get the language vendor of this document.
       */
      void GetLanguageVendor( [Out] out Guid pRetVal );

      /*
       * Get the check sum algorithm id. Returns a guid of all zeros if
       * there is no checksum.
       */
      void GetCheckSumAlgorithmId( [Out] out Guid pRetVal );

      /*
       * Get the check sum.
       */
      void GetCheckSum( [In] UInt32 cData,
                        [Out] out UInt32 pcData,
                        [In, Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0 )] Byte[] data
                      );

      /*
       * Given a line in this document that may or may not be a sequence
       * point, return the closest line that is a sequence point.  */
      void FindClosestLine( [In] UInt32 line,
                            [Out] out UInt32 pRetVal );

      /*
       * Returns true if the document has source embedded in the
       * debugging symbols.
       */
      void HasEmbeddedSource( [Out, MarshalAs( UnmanagedType.Bool )] out Boolean pRetVal );

      /*
       * Returns the length, in bytes, of the embedded source.
       */
      void GetSourceLength( [Out] out UInt32 pRetVal );

      /*
       * Returns the embedded source into the given buffer. The buffer must
       * be large enough to hold the source.
       */
      void GetSourceRange( [In] UInt32 startLine,
                           [In] UInt32 startColumn,
                           [In] UInt32 endLine,
                           [In] UInt32 endColumn,
                           [In] UInt32 cSourceBytes,
                           [Out] out UInt32 pcSourceBytes,
                           [In, Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 4 )] Byte[] source
                          );
   }

   [Guid( "B62B923C-B500-3158-A543-24F307A8B7E1" ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown ), ComImport]
   public interface ISymUnmanagedMethod
   {
      /*
 * Return the metadata token for this method.
 */
      void GetToken( [Out] out SymbolToken pToken );

      /*
       * Get the count of sequence points within this method.
       */
      void GetSequencePointCount( [Out] out UInt32 pRetVal );

      /*
       * Get the root lexical scope within this method.
      * This scope encloses the entire method.
       */
      void GetRootScope( [Out, MarshalAs( UnmanagedType.Interface )] out ISymUnmanagedScope pRetVal );

      /*
       * Get the most enclosing lexical scope within this method that
       * encloses the given offset. This can be used to start
      * local variable searches.
       */
      void GetScopeFromOffset( [In] UInt32 offset,
                               [Out, MarshalAs( UnmanagedType.Interface )] out ISymUnmanagedScope pRetVal
                             );

      /*
       * Given a position within a document, return the offset within
       * this method that cooresponds to the position.
       */
      void GetOffset( [In, MarshalAs( UnmanagedType.Interface )] ISymUnmanagedDocument document,
                        [In] UInt32 line,
                        [In] UInt32 column,
                        [Out] out UInt32 pRetVal
                    );

      /*
       * Given a position in a document, return an array of start/end
       * offset paris that correspond to the ranges of IL that the
       * position covers within this method. The array is an array of
       * integers and is [start,end,start,end]. The number of range
       * pairs is the length of the array / 2.
       */
      void GetRanges( [In, MarshalAs( UnmanagedType.Interface )] ISymUnmanagedDocument document,
                        [In] UInt32 line,
                        [In] UInt32 column,
                        [In] UInt32 cRanges,
                        [Out] out UInt32 pcRanges,
                        [Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 3 )] UInt32[] ranges
                     );

      /*
       * Get the parameters for this method. The parameters are returned
       * in the order they are defined within the method's signature.
       */
      void GetParameters( [In] UInt32 cParams,
                            [Out] out UInt32 pcParams,
                            [Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 1, ArraySubType = UnmanagedType.Interface )] ISymUnmanagedVariable[] paramz
                        );

      /*
       * Get the namespace that this method is defined within.
       */
      void GetNamespace( [Out, MarshalAs( UnmanagedType.Interface )] ISymUnmanagedNamespace pRetVal );

      /*
       * Get the start/end document positions for the source of this
       * method. The first array position is the start while the second
       * is the end. Returns true if positions were defined, false
       * otherwise.
       */
      void GetSourceStartEnd( [In, MarshalAs( UnmanagedType.LPArray, SizeConst = 2, ArraySubType = UnmanagedType.Interface )] ISymUnmanagedDocument[] docs,
                              [In, MarshalAs( UnmanagedType.LPArray, SizeConst = 2 )] UInt32[] lines,
                              [In, MarshalAs( UnmanagedType.LPArray, SizeConst = 2 )] UInt32[] columns,
                              [Out, MarshalAs( UnmanagedType.Bool )] out Boolean pRetVal
                            );

      /*
       * Get all the sequence points within this method.
       */
      void GetSequencePoints( [In] UInt32 cPoints,
                                [Out] out UInt32 pcPoints,
                                [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0 )] UInt32[] offsets,
                                [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0, ArraySubType = UnmanagedType.Interface )] ISymUnmanagedDocument[] documents,
                                [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0 )] UInt32[] lines,
                                [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0 )] UInt32[] columns,
                                [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0 )] UInt32[] endLines,
                                [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0 )] UInt32[] endColumns
                            );
   }

   [Guid( "9F60EEBE-2D9A-3F7C-BF58-80BC991C60BB" ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown ), ComImport]
   public interface ISymUnmanagedVariable
   {
      /*
       * Get the name of this variable.
      */
      void GetName( [In] UInt32 cchName,
                      [Out] out UInt32 pcchName,
                      [Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0 )] Char[] szName
                  );

      /*
       * Get the attribute flags for this variable.
       */
      void GetAttributes( [Out] out UInt32 pRetVal );

      /*
       * Get the signature of this variable.
       */
      void GetSignature( [In] UInt32 cSig,
                           [Out] out UInt32 pcSig,
                           [Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0 )] Byte[] sig
                        );

      /*
       * Get the kind of address of this variable
      * The retval will be one of the CorSymAddrKind constants.
       */
      void GetAddressKind( [Out] out UInt32 pRetVal );

      /*
       * Get the first address field for this variable. Its meaning depends
       * on the address kind.
       */
      void GetAddressField1( [Out] out UInt32 pRetVal );

      /*
       * Get the second address field for this variable. Its meaning depends
       * on the address kind.
       */
      void GetAddressField2( [Out] out UInt32 pRetVal );

      /*
       * Get the third address field for this variable. Its meaning depends
       * on the address kind.
       */
      void GetAddressField3( [Out] out UInt32 pRetVal );

      /*
       * Get the start offset of this variable within its parent. If this is
       * a local variable within a scope, this will fall within the offsets
       * defined for the scope.
       */
      void GetStartOffset( [Out] out UInt32 pRetVal );

      /*
       * Get the end offset of this variable within its parent. If this is
       * a local variable within a scope, this will fall within the offsets
       * defined for the scope.
       */
      void GetEndOffset( [Out] out UInt32 pRetVal );
   }

   [Guid( "0DFF7289-54F8-11d3-BD28-0000F80849BD" ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown ), ComImport]
   public interface ISymUnmanagedNamespace
   {
      /*
       * Get the name of this namespace.
       */
      void GetName( [In] UInt32 cchName,
                      [Out] out UInt32 pcchName,
                      [Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0 )] Char[] szName
                   );

      /*
       * Get the children of this namespace.
       */
      void GetNamespaces( [In] UInt32 cNameSpaces,
                            [Out] out UInt32 pcNameSpaces,
                            [Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0, ArraySubType = UnmanagedType.Interface )] ISymUnmanagedNamespace[] namespaces
                        );

      /*
       * Return all variables defined at global scope within this namespace.
       */
      void GetVariables( [In] UInt32 cVars,
                           [Out] out UInt32 pcVars,
                           [Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0, ArraySubType = UnmanagedType.Interface )] ISymUnmanagedVariable[] pVars
                       );
   }

   [Guid( "68005D0F-B8E0-3B01-84D5-A11A94154942" ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown ), ComImport]
   public interface ISymUnmanagedScope
   {
      /*
       * Get the method that contains this scope.
       */
      void GetMethod( [Out, MarshalAs( UnmanagedType.Interface )] out ISymUnmanagedMethod pRetVal );

      /*
       * Get the parent scope of this scope.
       */
      void GetParent( [Out, MarshalAs( UnmanagedType.Interface )] out ISymUnmanagedScope pRetVal );

      /*
       * Get the children of this scope.
       */
      void GetChildren( [In] UInt32 cChildren,
                          [Out] out UInt32 pcChildren,
                          [In, Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0, ArraySubType = UnmanagedType.Interface )] ISymUnmanagedScope[] children
         );

      /*
       * Get the start offset for this scope,
       */
      void GetStartOffset( [Out] out UInt32 pRetVal );

      /*
       * Get the end offset for this scope.
       */
      void GetEndOffset( [Out] out UInt32 pRetVal );

      /*
       * Get a count of the number of local variables defined within this
       * scope.
       */
      void GetLocalCount( [Out] out UInt32 pRetVal );

      /*
       * Get the local variables defined within this scope.
       */
      void GetLocals( [In] UInt32 cLocals,
                        [Out] out UInt32 pcLocals,
                        [In, Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0, ArraySubType = UnmanagedType.Interface )] ISymUnmanagedVariable[] locals );

      /*
       * Get the namespaces that are being "used" within this scope.
       */
      void GetNamespaces( [In] UInt32 cNameSpaces,
                            [Out] out UInt32 pcNameSpaces,
                            [In, Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0, ArraySubType = UnmanagedType.Interface )] ISymUnmanagedNamespace[] namespaces
         );

   }

   [Guid( "AE932FBA-3FD8-4dba-8232-30A2309B02DB" ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown ), ComImport]
   public interface ISymUnmanagedScope2 : ISymUnmanagedScope
   {
      /*
       * Get a count of the number of constants defined within this
       * scope.
       */
      void GetConstantCount( [Out] out UInt32 pRetVal );
      /*
       * Get the local constants defined within this scope.
       */
      void GetConstants( [In] UInt32 cConstants,
                         [Out] out UInt32 pcConstants,
                         [In, Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0, ArraySubType = UnmanagedType.Interface )] ISymUnmanagedConstant[] constants
                       );
   }

   [Guid( "48B25ED8-5BAD-41bc-9CEE-CD62FABC74E9" ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown ), ComImport]
   public interface ISymUnmanagedConstant
   {
      /*
       * Get the name of this constant.
       */
      void GetName( [In] UInt32 cchName,
                    [Out] out UInt32 pcchName,
                    [In, Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0 )] Char[] szName
                  );

      void GetValue( [In] IntPtr pValue ); // VARIANT*

      void GetSignature( [In] UInt32 cSig,
                         [Out] out UInt32 pcSig,
                         [Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 0 )] Byte[] sig
                       );
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
         Int32 dummy;
         writer.OpenScope( 0, out dummy );

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

         holder.ProcessPDBScopeOrFunc( func, 0, func.Length );

         writer.CloseScope( func.Length );
         writer.CloseMethod();
      }

   }

   internal static void ProcessPDBScopeOrFunc( this SymUnmanagedWriterHolder holder, PDBScopeOrFunction scp, Int32 startOffset, Int32 endOffset )
   {
      var writer = holder.Writer;
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

      foreach ( var ss in scp.Scopes )
      {
         Int32 dummy;
         writer.OpenScope( ss.Offset, out dummy );
         holder.ProcessPDBScopeOrFunc( ss, ss.Offset, ss.Offset + ss.Length );
         writer.CloseScope( ss.Offset + ss.Length );
      }
   }


   // Not set: PDBScope.Name, PDBLine.IsStatement, PDBFunction.Slots (but PDBScope.Slots are ok), PDBSlot.TypeToken, PDBFunction.LocalScopes (?)

   internal static PDBInstance CreateInstanceFromNativeReader(
      this ISymUnmanagedReader reader,
      CILAssemblyManipulator.Physical.CILMetaData md,
      out Int32? ep
      )
   {
      var retVal = new PDBInstance();
      SymbolToken epToken;
      reader.GetUserEntryPoint( out epToken );
      ep = epToken.GetToken() == 0 ? (Int32?) null : epToken.GetToken();
      var methods = md.MethodDefinitions.TableContents;

      var sources = new Dictionary<String, PDBSource>();


      for ( var i = 0; i < methods.Count; ++i )
      {
         var token = new SymbolToken( new CILAssemblyManipulator.Physical.TableIndex( CILAssemblyManipulator.Physical.Tables.MethodDef, i ).GetOneBasedToken() );
         ISymUnmanagedMethod method;
         reader.GetMethod( token, out method );
         if ( method != null )
         {
            method.GetToken( out token );
            var func = new PDBFunction()
            {
               //AsyncMethodInfo =
               //ENCID = 
               //ForwardingMethodToken
               //IteratorClass
               //ModuleForwardingMethodToken =
               Name = methods[i].Name,
               Token = (UInt32) token.GetToken()
            };
            ISymUnmanagedScope scope;
            method.GetRootScope( out scope );
            UInt32 length;
            if ( scope != null )
            {
               func.Scopes.Add( ( (ISymUnmanagedScope2) scope ).CreateScopeFromNativeScope() );
               scope.GetEndOffset( out length );
               func.Length = (Int32) length;
            }
            else
            {
               // Breakpoint
            }

            // Lines
            method.GetSequencePointCount( out length );
            var offsets = new UInt32[length];
            var docs = new ISymUnmanagedDocument[length];
            var lines = new UInt32[length];
            var cols = new UInt32[length];
            var endLines = new UInt32[length];
            var endCols = new UInt32[length];
            method.GetSequencePoints( length, out length, offsets, docs, lines, cols, endLines, endCols );
            func.Lines.AddRange( docs.Select( ( doc, j ) =>
            {
               UInt32 urlCount;
               doc.GetURL( 0, out urlCount, null );
               var charz = new Char[urlCount];
               doc.GetURL( urlCount, out urlCount, charz );
               return new PDBLine()
               {
                  LineStart = (Int32) lines[j],
                  LineEnd = (Int32) endLines[j],
                  ColumnStart = (UInt16) cols[j],
                  ColumnEnd = (UInt16) endCols[j],
                  Offset = (Int32) offsets[j],
                  Source = sources.GetOrAdd_NotThreadSafe( new String( charz.Take( (Int32) urlCount - 1 ).ToArray() ), url => doc.CreateSource( url ) ) // Count has terminating zero included)
               };
            } ) );


            // LocalScopes
            // Slots (are not exposed... ?)

            // Used namespaces are not exposed
         }
         else
         {
            // Breakpoint
         }
      }

      return retVal;
   }


   private static PDBScope CreateScopeFromNativeScope( this ISymUnmanagedScope2 scope )
   {
      // Name
      var retVal = new PDBScope();
      UInt32 tmp;
      scope.GetStartOffset( out tmp );
      retVal.Offset = (Int32) tmp;
      scope.GetEndOffset( out tmp );
      retVal.Length = (Int32) tmp - retVal.Offset;

      // Used namespaces
      scope.GetNamespaces( 0, out tmp, null );
      var nss = new ISymUnmanagedNamespace[tmp];
      scope.GetNamespaces( tmp, out tmp, nss );
      foreach ( var ns in nss )
      {
         ns.GetName( 0, out tmp, null );
         var chars = new Char[tmp];
         ns.GetName( tmp, out tmp, chars );
         retVal.UsedNamespaces.Add( new String( chars.Take( (Int32) tmp - 1 ).ToArray() ) );
         Marshal.ReleaseComObject( ns );
      }

      // Locals
      scope.GetLocals( 0, out tmp, null );
      var locals = new ISymUnmanagedVariable[tmp];
      scope.GetLocals( tmp, out tmp, locals );
      retVal.Slots.AddRange( locals.Select( local =>
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
         local.GetSignature( 0, out tmp, null );
         // TODO type token!!
         Marshal.ReleaseComObject( local );
         return slot;
      } ) );

      // Sub-scopes
      scope.GetChildren( 0, out tmp, null );
      var children = new ISymUnmanagedScope[tmp];
      scope.GetChildren( tmp, out tmp, children );
      retVal.Scopes.AddRange( children.Select( child =>
      {
         var subScope = ( (ISymUnmanagedScope2) child ).CreateScopeFromNativeScope();
         Marshal.ReleaseComObject( child );
         return subScope;
      } ) );

      return retVal;
   }

   private static PDBSource CreateSource( this ISymUnmanagedDocument doc, String url )
   {
      var src = new PDBSource()
      {
         Name = url
      };
      Guid guid;
      doc.GetDocumentType( out guid );
      src.DocumentType = guid;
      doc.GetLanguage( out guid );
      src.Language = guid;
      doc.GetLanguageVendor( out guid );
      src.Vendor = guid;
      doc.GetCheckSumAlgorithmId( out guid );
      src.HashAlgorithm = guid;
      UInt32 count;
      doc.GetCheckSum( 0, out count, null );
      src.Hash = new Byte[count];
      doc.GetCheckSum( count, out count, src.Hash );
      return src;
   }
}