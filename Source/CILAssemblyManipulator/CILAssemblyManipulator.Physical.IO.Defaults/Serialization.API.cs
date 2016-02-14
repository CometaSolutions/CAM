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
using CILAssemblyManipulator.Physical.IO.Defaults;
using CILAssemblyManipulator.Physical.Meta;
using CollectionsWithRoles.API;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TabularMetaData;
using TabularMetaData.Meta;

namespace CILAssemblyManipulator.Physical.IO.Defaults
{
   public sealed class RawModuleDefinition
   {
      public Int32 Generation { get; set; }

      public Int32 Name { get; set; }

      public Int32 ModuleGUID { get; set; }

      public Int32 EditAndContinueGUID { get; set; }

      public Int32 EditAndContinueBaseGUID { get; set; }
   }

   public sealed class RawTypeReference
   {
      public Int32 ResolutionScope { get; set; }

      public Int32 Name { get; set; }

      public Int32 Namespace { get; set; }
   }

   public sealed class RawTypeDefinition
   {
      public TypeAttributes Attributes { get; set; }

      public Int32 Name { get; set; }

      public Int32 Namespace { get; set; }

      public Int32 BaseType { get; set; }

      public Int32 FieldList { get; set; }

      public Int32 MethodList { get; set; }
   }

   public sealed class RawFieldDefinition
   {
      public FieldAttributes Attributes { get; set; }

      public Int32 Name { get; set; }

      public Int32 Signature { get; set; }
   }

   public sealed class RawMethodDefinition
   {
      public Int32 RVA { get; set; }

      public MethodImplAttributes ImplementationAttributes { get; set; }

      public MethodAttributes Attributes { get; set; }

      public Int32 Name { get; set; }

      public Int32 Signature { get; set; }

      public Int32 ParameterList { get; set; }
   }

   public sealed class RawParameterDefinition
   {
      public ParameterAttributes Attributes { get; set; }

      public Int32 Sequence { get; set; }

      public Int32 Name { get; set; }
   }

   public sealed class RawInterfaceImplementation
   {
      public Int32 Class { get; set; }

      public Int32 Interface { get; set; }
   }

   public sealed class RawMemberReference
   {
      public Int32 DeclaringType { get; set; }

      public Int32 Name { get; set; }

      public Int32 Signature { get; set; }
   }

   public sealed class RawConstantDefinition
   {
      public ConstantValueType Type { get; set; }

      public Byte Padding { get; set; }

      public Int32 Parent { get; set; }

      public Int32 Value { get; set; }
   }

   public sealed class RawCustomAttributeDefinition
   {
      public Int32 Parent { get; set; }

      public Int32 Type { get; set; }

      public Int32 Signature { get; set; }
   }

   public sealed class RawFieldMarshal
   {
      public Int32 Parent { get; set; }

      public Int32 NativeType { get; set; }
   }

   public sealed class RawSecurityDefinition
   {
      public SecurityAction Action { get; set; }

      public Int32 Parent { get; set; }

      public Int32 PermissionSets { get; set; }
   }

   public sealed class RawClassLayout
   {
      public Int32 PackingSize { get; set; }

      public Int32 ClassSize { get; set; }

      public Int32 Parent { get; set; }
   }

   public sealed class RawFieldLayout
   {
      public Int32 Offset { get; set; }

      public Int32 Field { get; set; }
   }

   public sealed class RawStandaloneSignature
   {
      public Int32 Signature { get; set; }

   }

   public sealed class RawEventMap
   {
      public Int32 Parent { get; set; }

      public Int32 EventList { get; set; }
   }

   public sealed class RawEventDefinition
   {
      public EventAttributes Attributes { get; set; }

      public Int32 Name { get; set; }

      public Int32 EventType { get; set; }
   }

   public sealed class RawPropertyMap
   {
      public Int32 Parent { get; set; }

      public Int32 PropertyList { get; set; }
   }

   public sealed class RawPropertyDefinition
   {
      public PropertyAttributes Attributes { get; set; }

      public Int32 Name { get; set; }

      public Int32 Signature { get; set; }
   }

   public sealed class RawMethodSemantics
   {

      public MethodSemanticsAttributes Attributes { get; set; }

      public Int32 Method { get; set; }

      public Int32 Associaton { get; set; }
   }

   public sealed class RawMethodImplementation
   {
      public Int32 Class { get; set; }

      public Int32 MethodBody { get; set; }

      public Int32 MethodDeclaration { get; set; }
   }

   public sealed class RawModuleReference
   {
      public Int32 ModuleName { get; set; }
   }

   public sealed class RawTypeSpecification
   {
      public Int32 Signature { get; set; }
   }

   public sealed class RawMethodImplementationMap
   {

      public PInvokeAttributes Attributes { get; set; }

      public Int32 MemberForwarded { get; set; }

      public Int32 ImportName { get; set; }

      public Int32 ImportScope { get; set; }
   }

   public sealed class RawFieldRVA
   {
      public Int32 RVA { get; set; }

      public Int32 Field { get; set; }
   }

   public sealed class RawAssemblyDefinition
   {
      public AssemblyHashAlgorithm HashAlgorithm { get; set; }

      public Int32 MajorVersion { get; set; }

      public Int32 MinorVersion { get; set; }

      public Int32 BuildNumber { get; set; }

      public Int32 RevisionNumber { get; set; }

      public AssemblyFlags Attributes { get; set; }

      public Int32 PublicKey { get; set; }

      public Int32 Name { get; set; }

      public Int32 Culture { get; set; }

   }

   public sealed class RawAssemblyReference
   {
      public Int32 MajorVersion { get; set; }

      public Int32 MinorVersion { get; set; }

      public Int32 BuildNumber { get; set; }

      public Int32 RevisionNumber { get; set; }

      public AssemblyFlags Attributes { get; set; }

      public Int32 PublicKeyOrToken { get; set; }

      public Int32 Name { get; set; }

      public Int32 Culture { get; set; }

      public Int32 HashValue { get; set; }

   }

   public sealed class RawFileReference
   {
      public CAMPhysical::CILAssemblyManipulator.Physical.FileAttributes Attributes { get; set; }

      public Int32 Name { get; set; }

      public Int32 HashValue { get; set; }
   }

   public sealed class RawExportedType
   {
      public TypeAttributes Attributes { get; set; }

      public Int32 TypeDefinitionIndex { get; set; }

      public Int32 Name { get; set; }

      public Int32 Namespace { get; set; }

      public Int32 Implementation { get; set; }
   }

   public sealed class RawManifestResource
   {
      public Int32 Offset { get; set; }

      public ManifestResourceAttributes Attributes { get; set; }

      public Int32 Name { get; set; }

      public Int32 Implementation { get; set; }

   }

   public sealed class RawNestedClassDefinition
   {
      public Int32 NestedClass { get; set; }

      public Int32 EnclosingClass { get; set; }
   }

   public sealed class RawGenericParameterDefinition
   {
      public Int32 GenericParameterIndex { get; set; }

      public GenericParameterAttributes Attributes { get; set; }

      public Int32 Owner { get; set; }

      public Int32 Name { get; set; }
   }

   public sealed class RawMethodSpecification
   {
      public Int32 Method { get; set; }

      public Int32 Signature { get; set; }
   }

   public sealed class RawGenericParameterConstraintDefinition
   {
      public Int32 Owner { get; set; }

      public Int32 Constraint { get; set; }
   }

   public sealed class RawEditAndContinueLog
   {
      public Int32 Token { get; set; }

      public Int32 FuncCode { get; set; }
   }

   public sealed class RawEditAndContinueMap
   {
      public Int32 Token { get; set; }
   }

   public sealed class RawFieldDefinitionPointer
   {
      public Int32 FieldIndex { get; set; }
   }

   public sealed class RawMethodDefinitionPointer
   {
      public Int32 MethodIndex { get; set; }
   }

   public sealed class RawParameterDefinitionPointer
   {
      public Int32 ParameterIndex { get; set; }
   }

   public sealed class RawEventDefinitionPointer
   {
      public Int32 EventIndex { get; set; }
   }

   public sealed class RawPropertyDefinitionPointer
   {
      public Int32 PropertyIndex { get; set; }
   }

   public sealed class RawAssemblyDefinitionProcessor
   {
      public Int32 Processor { get; set; }
   }

   public sealed class RawAssemblyDefinitionOS
   {
      public Int32 OSPlatformID { get; set; }

      public Int32 OSMajorVersion { get; set; }

      public Int32 OSMinorVersion { get; set; }
   }

   public sealed class RawAssemblyReferenceProcessor
   {
      public Int32 Processor { get; set; }

      public Int32 AssemblyRef { get; set; }
   }

   public sealed class RawAssemblyReferenceOS
   {
      public Int32 OSPlatformID { get; set; }

      public Int32 OSMajorVersion { get; set; }

      public Int32 OSMinorVersion { get; set; }

      public Int32 AssemblyRef { get; set; }

   }

   public interface MetaDataSerializationSupportProvider
   {
      IEnumerable<TableSerializationInfo> CreateTableSerializationInfos(
         IEnumerable<MetaDataTableInformation> tableInfos,
         TableSerializationInfoCreationArgs serializationCreationArgs
         );
   }

   public interface TableSerializationInfo
   {
      Tables Table { get; }

      Boolean IsSorted { get; }

      TableSerializationFunctionality CreateSupport(
         DefaultColumnSerializationSupportCreationArgs args
         );

      Int32 RawValueStorageColumnCount { get; }

      Int32 HeapValueColumnCount { get; }

      void ProcessRowForRawValues(
         RawValueProcessingArgs args
         );

      IEnumerable<SectionPartWithRVAs> CreateRawValueSectionParts(
         CILMetaData md,
         WriterMetaDataStreamContainer mdStreamContainer
         );


      void PopulateTableHeapValues(
         CILMetaData md,
         ColumnValueStorage<Int32> storage,
         WriterMetaDataStreamContainer mdStreamContainer,
         ResizableArray<Byte> array,
         ArrayQuery<Byte> publicKey
         );

      IEnumerable<Int32> GetAllRawValues(
         MetaDataTable table,
         DictionaryQuery<Int32, ArrayQuery<Int64>> rawValueProvder,
         ColumnValueStorage<Int32> heapIndices
         );
   }

   public class DefaultColumnSerializationSupportCreationArgs
   {

      public DefaultColumnSerializationSupportCreationArgs(
         ArrayQuery<Int32> tableSizes,
         DictionaryQuery<String, Int32> streamSizes
         )
      {
         ArgumentValidator.ValidateNotNull( "Table sizes", tableSizes );

         this.TableSizes = tableSizes;
         this.StreamSizes = streamSizes;
      }

      public ArrayQuery<Int32> TableSizes { get; }

      public DictionaryQuery<String, Int32> StreamSizes { get; }

      public Boolean IsWide( String heapName )
      {
         Int32 streamSize;
         return heapName != null
            && this.StreamSizes.TryGetValue( heapName, out streamSize )
            && ( (UInt32) streamSize ) > UInt16.MaxValue;
      }

   }

   public interface TableSerializationFunctionality
   {
      TableSerializationInfo TableSerializationInfo { get; }

      Object ReadRawRow( Byte[] array, Int32 idx );

      void ReadRows( MetaDataTable table, Int32 tableRowCount, RowReadingArguments args );

      ArrayQuery<ColumnSerializationFunctionality> ColumnSerializationSupports { get; }
   }

   public class RowReadingArguments
   {
      public RowReadingArguments(
         Byte[] array,
         Int32 index,
         ReaderMetaDataStreamContainer mdStreamContainer,
         ColumnValueStorage<Int32> rawValueStorage,
         SignatureProvider sigProvider
         )
      {
         ArgumentValidator.ValidateNotNull( "Array", array );
         ArgumentValidator.ValidateNotNull( "Meta data stream container", mdStreamContainer );
         ArgumentValidator.ValidateNotNull( "Raw value storage", rawValueStorage );
         ArgumentValidator.ValidateNotNull( "Signature provider", sigProvider );

         this.Array = array;
         this.Index = index;
         this.MDStreamContainer = mdStreamContainer;
         this.RawValueStorage = rawValueStorage;
         this.SignatureProvider = sigProvider;
      }

      public Byte[] Array { get; }

      public Int32 Index { get; }

      public ReaderMetaDataStreamContainer MDStreamContainer { get; }

      public ColumnValueStorage<Int32> RawValueStorage { get; }

      public SignatureProvider SignatureProvider { get; }
   }

   public class RawValueProcessingArgs
   {

      private readonly Lazy<DictionaryQuery<Int32, ClassLayout>> _layoutInfo;
      public RawValueProcessingArgs(
         StreamHelper stream,
         ImageInformation imageInformation,
         RVAConverter rvaConverter,
         ReaderMetaDataStreamContainer mdStreamContainer,
         CILMetaData md,
         ResizableArray<Byte> array
         )
      {
         ArgumentValidator.ValidateNotNull( "Stream", stream );
         ArgumentValidator.ValidateNotNull( "Image information", imageInformation );
         ArgumentValidator.ValidateNotNull( "RVA converter", rvaConverter );
         ArgumentValidator.ValidateNotNull( "Meta data stream container", mdStreamContainer );
         ArgumentValidator.ValidateNotNull( "Meta data", md );
         ArgumentValidator.ValidateNotNull( "Array", array );

         this.Stream = stream;
         this.ImageInformation = imageInformation;
         this.RVAConverter = rvaConverter;
         this.MDStreamContainer = mdStreamContainer;
         this.MetaData = md;
         this.Array = array;
         this._layoutInfo = new Lazy<DictionaryQuery<Int32, ClassLayout>>(
            () => md.ClassLayouts.TableContents
            .ToDictionary_Overwrite( l => l.Parent.Index, l => l )
            .ToDictionaryProxy().CQ,
            System.Threading.LazyThreadSafetyMode.None );
      }

      public StreamHelper Stream { get; }
      public ImageInformation ImageInformation { get; }
      public RVAConverter RVAConverter { get; }

      public ReaderMetaDataStreamContainer MDStreamContainer { get; }
      public CILMetaData MetaData { get; }

      public ResizableArray<Byte> Array { get; }

      public DictionaryQuery<Int32, ClassLayout> LayoutInfo
      {
         get
         {
            return this._layoutInfo.Value;
         }
      }

   }

   public class RowHeapFillingArguments
   {
      public RowHeapFillingArguments(
         WriterMetaDataStreamContainer mdStreamContainer,
         ResizableArray<Byte> array,
         ArrayQuery<Byte> publicKey,
         CILMetaData metaData
         )
      {
         ArgumentValidator.ValidateNotNull( "Meta data stream container", mdStreamContainer );
         ArgumentValidator.ValidateNotNull( "Byte array", array );
         ArgumentValidator.ValidateNotNull( "Meta data", metaData );

         this.MDStreamContainer = mdStreamContainer;
         this.Array = array;
         this.PublicKey = publicKey;
         this.MetaData = metaData;
         this.AuxArray = new ResizableArray<Byte>();
      }

      public ResizableArray<Byte> Array { get; }

      public WriterMetaDataStreamContainer MDStreamContainer { get; }

      public ArrayQuery<Byte> PublicKey { get; }

      public CILMetaData MetaData { get; }

      public ResizableArray<Byte> AuxArray { get; }
   }

   public interface ColumnSerializationFunctionality
   {

      Int32 ColumnByteCount { get; }

      Int32 ReadRawValue( Byte[] array, ref Int32 idx );

      void WriteValue( Byte[] bytes, Int32 idx, Int32 value );
   }

   internal static class MetaDataConstants
   {

      internal static readonly Encoding SYS_STRING_ENCODING = new UTF8Encoding( false, false );
      internal static readonly Encoding USER_STRING_ENCODING = new UnicodeEncoding( false, false, false );

      internal const String TABLE_STREAM_NAME = "#~";
      internal const String SYS_STRING_STREAM_NAME = "#Strings";
      internal const String USER_STRING_STREAM_NAME = "#US";
      internal const String GUID_STREAM_NAME = "#GUID";
      internal const String BLOB_STREAM_NAME = "#Blob";

      internal const Int32 GUID_SIZE = 16;
   }
}

public static partial class E_CILPhysical
{
   public static ArrayQuery<Int32> CreateTableSizeArray( this IEnumerable<TableSerializationInfo> infos, CILMetaData md )
   {
      return infos.Select( info =>
      {
         MetaDataTable tbl;
         return info != null && md.TryGetByTable( (Int32) info.Table, out tbl ) ?
            tbl.GetRowCount() :
            0;
      } ).ToArrayProxy().CQ;
   }

   public static IEnumerable<TableSerializationInfo> CreateTableSerializationInfos( this MetaDataSerializationSupportProvider serializationSupportProvider, CILMetaData md, TableSerializationInfoCreationArgs serializationCreationArgs )
   {
      return serializationSupportProvider.CreateTableSerializationInfos( md.GetAllTables().Select( t => t.TableInformationNotGeneric ), serializationCreationArgs );
   }

   public static IEnumerable<TableSerializationInfo> CreateTableSerializationInfos( this MetaDataSerializationSupportProvider serializationSupportProvider, MetaDataTableInformationProvider tableInfoProvider, TableSerializationInfoCreationArgs serializationCreationArgs )
   {
      return serializationSupportProvider.CreateTableSerializationInfos( tableInfoProvider.GetAllSupportedTableInformations(), serializationCreationArgs );
   }
}