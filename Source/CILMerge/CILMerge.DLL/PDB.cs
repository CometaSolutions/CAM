/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
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
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using CILAssemblyManipulator.Physical.PDB;
using CommonUtils;
using CILAssemblyManipulator.Physical.IO;

namespace CILMerge
{
   internal class PDBHelper : IDisposable
   {
      internal const String SYM_WRITER_GUID_STR = "0B97726E-9E6D-4f05-9A26-424022093CAA";
      internal const String DOC_WRITER_GUID_STR = "B01FAFEB-C450-3A4D-BEEC-B4CEEC01E006";
      internal const String UNMANAGED_WRITER_GUID_STR = "108296C1-281E-11D3-BD22-0000F80849BD";
      internal const String METADATA_EMIT_GUID_STR = "BA3FEE4C-ECB9-4e41-83B7-183FA41CD859";
      internal const String METADATA_IMPORT_GUID_STR = "7DAC8207-D3AE-4c75-9B67-92801A497D44";

      private static Guid UNMANAGED_WRITER_GUID = new Guid( UNMANAGED_WRITER_GUID_STR );
      private static Guid SYM_WRITER_GUID = new Guid( SYM_WRITER_GUID_STR );

      [DllImport( "ole32.dll" )]
      static extern int CoCreateInstance(
         [In] ref Guid rclsid,
         [In, MarshalAs( UnmanagedType.IUnknown )] Object pUnkOuter,
         [In] uint dwClsContext,
         [In] ref Guid riid,
         [Out, MarshalAs( UnmanagedType.Interface )] out Object ppv );

      private readonly String _tmpFN;
      private readonly String _fn;
      private readonly ISymUnmanagedWriter2 _unmanagedWriter;
      private readonly IDictionary<String, ISymUnmanagedDocumentWriter> _unmanagedDocs;
      private readonly WritingArguments _eArgs;
      internal PDBHelper(
         CILAssemblyManipulator.Physical.CILMetaData module,
         WritingArguments eArgs,
         String outPath
         )
      {
         Object writer;
         CoCreateInstance( ref UNMANAGED_WRITER_GUID, null, 1u, ref SYM_WRITER_GUID, out writer );
         this._unmanagedWriter = (ISymUnmanagedWriter2) writer;
         this._unmanagedDocs = new Dictionary<String, ISymUnmanagedDocumentWriter>();

         this._tmpFN = System.IO.Path.Combine( System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName() ) + ".pdb";
         this._fn = System.IO.Path.ChangeExtension( outPath, "pdb" );
         this._eArgs = eArgs;
         // Initialize writer
         this._unmanagedWriter.Initialize2( new MDHelper( module, eArgs ), this._tmpFN, null, true, this._fn );

         // Get debug header data
         Int32 dbgHdrSize;
         ImageDebugDirectory debugDir;
         // Get size of debug directory
         this._unmanagedWriter.GetDebugInfo( out debugDir, 0, out dbgHdrSize, null );
         // Get the data of debug directory
         var debugDirContents = new Byte[dbgHdrSize];
         this._unmanagedWriter.GetDebugInfo( out debugDir, dbgHdrSize, out dbgHdrSize, debugDirContents );
         // Set information for CILAssemblyManipulator emitter

         var dbgInfo = eArgs.WritingOptions.DebugOptions;
         dbgInfo.Characteristics = debugDir.Characteristics;
         dbgInfo.Timestamp = debugDir.TimeDateStamp;
         dbgInfo.MajorVersion = debugDir.MajorVersion;
         dbgInfo.MinorVersion = debugDir.MinorVersion;
         dbgInfo.DebugType = debugDir.Type;
         dbgInfo.DebugData = debugDirContents;
      }

      internal String PDBFileLocation
      {
         get
         {
            return this._fn;
         }
      }

      internal void ProcessPDB( PDBInstance pdb )
      {
         foreach ( var kvp in pdb.Sources )
         {
            var src = kvp.Value;
            var name = kvp.Key;
            var lang = src.Language;
            var docType = src.DocumentType;
            var vendor = src.Vendor;
            ISymUnmanagedDocumentWriter uDoc;
            this._unmanagedWriter.DefineDocument( name, ref lang, ref vendor, ref docType, out uDoc );
            this._unmanagedDocs.Add( name, uDoc );
         }

         foreach ( var func in pdb.Modules.Values.SelectMany( m => m.Functions ) )
         {
            this._unmanagedWriter.OpenMethod( new SymbolToken( (Int32) func.Token ) );
            Int32 dummy;
            this._unmanagedWriter.OpenScope( 0, out dummy );

            foreach ( var kvp in func.Lines )
            {
               this._unmanagedWriter.DefineSequencePoints(
                  this._unmanagedDocs[kvp.Key],
                  kvp.Value.Count,
                  kvp.Value.Select( l => l.Offset ).ToArray(),
                  kvp.Value.Select( l => l.LineStart ).ToArray(),
                  kvp.Value.Select( l => (Int32) l.ColumnStart.Value ).ToArray(),
                  kvp.Value.Select( l => l.LineEnd ).ToArray(),
                  kvp.Value.Select( l => (Int32) l.ColumnEnd.Value ).ToArray() );
            }

            this.ProcessPDBScopeOrFunc( func, 0, func.Length );

            this._unmanagedWriter.CloseScope( func.Length );
            this._unmanagedWriter.CloseMethod();
         }

      }

      private void ProcessPDBScopeOrFunc( PDBScopeOrFunction scp, Int32 startOffset, Int32 endOffset )
      {
         foreach ( var slot in scp.Slots )
         {
            // For some reason, even if correct .Flags are given to the local variable, the flags are not persisted. I don't know why.
            // This causes compiler-generated locals to show up in 'Locals' when debugging the code.
            this._unmanagedWriter.DefineLocalVariable2( slot.Name, (Int32) slot.Flags, new SymbolToken( (Int32) slot.TypeToken ), (Int32) SymAddressKind.ILOffset, slot.SlotIndex, 0, 0, startOffset, endOffset );
         }
         foreach ( var un in scp.UsedNamespaces )
         {
            this._unmanagedWriter.UsingNamespace( un );
         }

         foreach ( var ss in scp.Scopes )
         {
            Int32 dummy;
            this._unmanagedWriter.OpenScope( ss.Offset, out dummy );
            this.ProcessPDBScopeOrFunc( ss, ss.Offset, ss.Offset + ss.Length );
            this._unmanagedWriter.CloseScope( ss.Offset + ss.Length );
         }
      }


      #region IDisposable Members

      public void Dispose()
      {
         // Remember set entry point before writing out.
         var imageInfo = this._eArgs.ImageInformation;
         if ( imageInfo != null )
         {
            Int32 epToken;
            if ( imageInfo.CLIInformation.CLIHeader.TryGetManagedOrUnmanagedEntryPoint( out epToken ) )
            {
               this._unmanagedWriter.SetUserEntryPoint( new SymbolToken( epToken ) );
            }
            this._unmanagedWriter.Close();
            try
            {
               Marshal.ReleaseComObject( this._unmanagedWriter );
               foreach ( var doc in this._unmanagedDocs )
               {
                  Marshal.ReleaseComObject( doc.Value );
               }
            }
            finally
            {
               // Copy tmp-PDB to new location
               System.IO.File.Copy( this._tmpFN, this._fn, true );
            }
         }
      }

      #endregion

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
         [In, Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 1 )] byte[] data );

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
      private readonly WritingArguments _eArgs;
      private readonly IList<Int32> _methodDeclaringTypes;
      private readonly IDictionary<Int32, Int32> _typeEnclosingTypes;

      internal MDHelper( CILAssemblyManipulator.Physical.CILMetaData module, WritingArguments eArgs )
      {
         ArgumentValidator.ValidateNotNull( "Module", module );
         ArgumentValidator.ValidateNotNull( "Emitting arguments", eArgs );
         this._module = module;
         this._eArgs = eArgs;

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
            PDBHelper.WriteInt32Unmanaged( pulCodeRVA, (Int32) this._eArgs.ImageInformation.CLIInformation.DataReferences.GetMethodRVAs()[mIdx.Index] );
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
}
