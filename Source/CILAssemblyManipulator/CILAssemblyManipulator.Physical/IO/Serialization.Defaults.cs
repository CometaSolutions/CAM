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
using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO
{
   public class DefaultColumnSerializationInfo<TRawRow, TRow> : ColumnSerializationInfo
      where TRawRow : class
      where TRow : class
   {

      public DefaultColumnSerializationInfo(
         String columnName,
         Func<ColumnSerializationSupportCreationArgs, ColumnSerializationFunctionality> serializationCreator,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, Int32> setter,
         Func<TRow, Int32> constExtractor
         )
         : this(
        columnName,
        serializationCreator,
        rawSetter,
        setter,
        null,
        null,
        null,
        constExtractor
        )
      {

      }

      public DefaultColumnSerializationInfo(
         String columnName,
         Func<ColumnSerializationSupportCreationArgs, ColumnSerializationFunctionality> serializationCreator,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, Int32> setter,
         Action<ColumnSettingArguments<TRow, RawValueProcessingArgs>, Int32> rawValueProcessor,
         Func<ColumnSettingArguments<TRow, RowRawValueExtractionArguments>, Int32> rawValueExtractor
         )
         : this(
              columnName,
              serializationCreator,
              rawSetter,
              setter,
              rawValueProcessor,
              rawValueExtractor,
              null,
              null
              )
      {

      }

      public DefaultColumnSerializationInfo(
         String columnName,
         HeapIndexKind heapIndexKind,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, Int32> setter,
         Func<ColumnSettingArguments<TRow, RowHeapFillingArguments>, Int32> heapValueExtractor
         )
         : this(
              columnName,
              args => args.IsWide( heapIndexKind ) ? (ColumnSerializationFunctionality) new ColumnSerializationSupport_Constant32() : new ColumnSerializationSupport_Constant16(),
              rawSetter,
              setter,
              null,
              null,
              heapValueExtractor,
              null
              )
      {
         ArgumentValidator.ValidateNotNull( "Heap value extractor", heapValueExtractor );
      }

      protected DefaultColumnSerializationInfo(
         String columnName,
         Func<ColumnSerializationSupportCreationArgs, ColumnSerializationFunctionality> creator,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, Int32> setter,
         Action<ColumnSettingArguments<TRow, RawValueProcessingArgs>, Int32> rawValueProcessor,
         Func<ColumnSettingArguments<TRow, RowRawValueExtractionArguments>, Int32> rawValueExtractor,
         Func<ColumnSettingArguments<TRow, RowHeapFillingArguments>, Int32> heapValueExtractor,
         Func<TRow, Int32> constExtractor
         )
      {
         ArgumentValidator.ValidateNotNull( "Column name", columnName );
         ArgumentValidator.ValidateNotNull( "Raw setter", rawSetter );
         ArgumentValidator.ValidateNotNull( "Serialization support creator", creator );
         if ( setter == null )
         {
            ArgumentValidator.ValidateNotNull( "Raw value processor", rawValueProcessor );
            ArgumentValidator.ValidateNotNull( "Raw value extractor", rawValueExtractor );
         }
         else
         {
            ArgumentValidator.ValidateNotNull( "Setter", setter );
         }


         this.ColumnName = columnName;
         this.RawSetter = rawSetter;
         this.Setter = setter;
         this.SerializationSupportCreator = creator;
         this.RawValueProcessor = rawValueProcessor;
         this.ConstantExtractor = constExtractor;
      }

      public String ColumnName { get; }
      public Func<ColumnSerializationSupportCreationArgs, ColumnSerializationFunctionality> SerializationSupportCreator { get; }

      // Reading
      public Action<TRawRow, Int32> RawSetter { get; }
      public Action<ColumnSettingArguments<TRow, RowReadingArguments>, Int32> Setter { get; }
      public Action<ColumnSettingArguments<TRow, RawValueProcessingArgs>, Int32> RawValueProcessor { get; }

      // Writing
      public Func<ColumnSettingArguments<TRow, RowRawValueExtractionArguments>, Int32> RawValueExtractor { get; }
      public Func<ColumnSettingArguments<TRow, RowHeapFillingArguments>, Int32> HeapValueExtractor { get; }

      public Func<TRow, Int32> ConstantExtractor { get; }
   }

   public static class DefaultColumnSerializationInfoFactory
   {
      public static Int32 GetCodedTableSize( ArrayQuery<Int32> tableSizes, ArrayQuery<Tables?> referencedTables )
      {
         Int32 max = 0;
         var len = referencedTables.Count;
         for ( var i = 0; i < len; ++i )
         {
            var current = referencedTables[i];
            if ( current.HasValue )
            {
               max = Math.Max( max, tableSizes[(Int32) current.Value] );
            }
         }
         return max < ( UInt16.MaxValue >> CodedTableIndexDecoder.GetTagBitSize( referencedTables.Count ) ) ?
            2 :
            4;
      }


      public static DefaultColumnSerializationInfo<TRawRow, TRow> Constant8<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, Int32> setter,
         Func<TRow, Int32> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => new ColumnSerializationSupport_Constant8(),
            rawSetter,
            setter,
            getter
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> Constant16<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, Int32> setter,
         Func<TRow, Int32> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => new ColumnSerializationSupport_Constant16(),
            rawSetter,
            setter,
            getter
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> Constant32<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, Int32> setter,
         Func<TRow, Int32> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => new ColumnSerializationSupport_Constant32(),
            rawSetter,
            setter,
            getter
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> SimpleReference<TRawRow, TRow>(
         String columnName,
         Tables targetTable,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, TableIndex> setter,
         Func<TRow, TableIndex> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => args.TableSizes[(Int32) targetTable] >= UInt16.MaxValue ? (ColumnSerializationFunctionality) new ColumnSerializationSupport_Constant32() : new ColumnSerializationSupport_Constant16(),
            rawSetter,
            ( args, value ) => setter( args, new TableIndex( targetTable, (Int32) Math.Min( 0, (UInt32) value - 1 ) ) ),
            row => getter( row ).Index
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> CodedReference<TRawRow, TRow>(
         String columnName,
         ArrayQuery<Tables?> targetTables,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, TableIndex?> setter,
         Func<TRow, TableIndex?> getter
         )
         where TRawRow : class
         where TRow : class
      {
         var decoder = new CodedTableIndexDecoder( targetTables );

         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => GetCodedTableSize( args.TableSizes, targetTables ) < sizeof( Int32 ) ? (ColumnSerializationFunctionality) new ColumnSerializationSupport_Constant16() : new ColumnSerializationSupport_Constant32(),
            rawSetter,
            ( args, value ) => setter( args, decoder.DecodeTableIndex( value ) ),
            row => decoder.EncodeTableIndex( getter( row ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBArray<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, Byte[]> setter,
         Func<TRow, Byte[]> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return BLOBCustom<TRawRow, TRow>(
            columnName,
            rawSetter,
            ( args, value, blobs ) => setter( args, blobs.GetBLOB( value ) ),
            ( args ) => getter( args.Row )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBTypeSignature<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, TypeSignature> setter,
         Func<TRow, TypeSignature> sigGetter
         )
         where TRawRow : class
         where TRow : class
      {
         return BLOBCustom<TRawRow, TRow>(
            columnName,
            rawSetter,
            ( args, value, blobs ) => setter( args, blobs.ReadTypeSignature( value ) ),
            args => args.RowArgs.Array.CreateTypeSignature( sigGetter( args.Row ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBNonTypeSignature<TRawRow, TRow, TSignature>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, TSignature> setter,
         Func<TRow, TSignature> getter
         )
         where TRawRow : class
         where TRow : class
         where TSignature : AbstractSignature
      {
         var isMethodDef = Equals( typeof( MethodDefinitionSignature ), typeof( TSignature ) );
         return BLOBCustom<TRawRow, TRow>(
            columnName,
            rawSetter,
            ( args, value, blobs ) =>
            {
               Boolean wasFieldSig;
               setter( args, blobs.ReadNonTypeSignature( value, isMethodDef, false, out wasFieldSig ) as TSignature );
            },
            args => args.RowArgs.Array.CreateAnySignature( getter( args.Row ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBCASignature<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, AbstractCustomAttributeSignature> setter
         )
         where TRawRow : class
         where TRow : class
      {
         return BLOBCustom<TRawRow, TRow>(
            columnName,
            rawSetter,
            ( args, value, blobs ) => setter( args, blobs.ReadCASignature( value ) ),
            args => args.RowArgs.Array.CreateCustomAttributeSignature( args.RowArgs.MetaData, args.RowIndex )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBMarshalingInfo<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, MarshalingInfo> setter,
         Func<TRow, MarshalingInfo> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return BLOBCustom<TRawRow, TRow>(
            columnName,
            rawSetter,
            ( args, value, blobs ) => setter( args, blobs.ReadMarshalingInfo( value ) ),
            args => args.RowArgs.Array.CreateMarshalSpec( getter( args.Row ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBSecurityInformation<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, IEnumerable<AbstractSecurityInformation>> setter,
         Func<TRow, List<AbstractSecurityInformation>> getter
         )
         where TRawRow : class
         where TRow : class
      {
         var aux = new ResizableArray<Byte>();
         return BLOBCustom<TRawRow, TRow>(
            columnName,
            rawSetter,
            ( args, value, blobs ) => setter( args, blobs.ReadSecurityInformation( value ) ),
            args => args.RowArgs.Array.CreateSecuritySignature( getter( args.Row ), aux )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBCustom<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, Int32, ReaderBLOBStreamHandler> setter,
         Func<ColumnSettingArguments<TRow, RowHeapFillingArguments>, Byte[]> blobCreator
         )
         where TRawRow : class
         where TRow : class
      {
         return HeapIndex<TRawRow, TRow>(
            columnName,
            HeapIndexKind.BLOB,
            rawSetter,
            ( args, value ) => setter( args, value, args.RowArgs.MDStreamContainer.BLOBs ),
            ( args ) => args.RowArgs.MDStreamContainer.BLOBs.RegisterBLOB( blobCreator( args ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> GUID<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, Guid?> setter,
         Func<TRow, Guid?> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return HeapIndex<TRawRow, TRow>(
            columnName,
            HeapIndexKind.GUID,
            rawSetter,
            ( args, value ) => setter( args, args.RowArgs.MDStreamContainer.GUIDs.GetGUID( value ) ),
            args => args.RowArgs.MDStreamContainer.GUIDs.RegisterGUID( getter( args.Row ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> SystemString<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, String> setter,
         Func<TRow, String> getter
         )
         where TRawRow : class
         where TRow : class
      {
         return HeapIndex<TRawRow, TRow>(
            columnName,
            HeapIndexKind.String,
            rawSetter,
            ( args, value ) => setter( args, args.RowArgs.MDStreamContainer.SystemStrings.GetString( value ) ),
            args => args.RowArgs.MDStreamContainer.SystemStrings.RegisterString( getter( args.Row ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> HeapIndex<TRawRow, TRow>(
         String columnName,
         HeapIndexKind heapKind,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RowReadingArguments>, Int32> setter,
         Func<ColumnSettingArguments<TRow, RowHeapFillingArguments>, Int32> heapValueExtractor
         )
         where TRawRow : class
         where TRow : class
      {

         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            heapKind,
            rawSetter,
            ( args, value ) =>
            {
               if ( value != 0 )
               {
                  setter( args, value );
               }
            },
            heapValueExtractor
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> RawValueStorageColumn<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow, RawValueProcessingArgs>, Int32> rawValueProcessor,
         Func<ColumnSettingArguments<TRow, RowRawValueExtractionArguments>, Int32> rawValueExtractor
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => new ColumnSerializationSupport_Constant32(),
            rawSetter,
            null,
            rawValueProcessor,
            rawValueExtractor
         );
      }

   }

   public sealed class CodedTableIndexDecoder
   {
      // ECMA-335, pp. 274-276
      public static readonly ArrayQuery<Tables?> TypeDefOrRef = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.TypeDef, Tables.TypeRef, Tables.TypeSpec } ).CQ;
      public static readonly ArrayQuery<Tables?> HasConstant = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.Field, Tables.Parameter, Tables.Property } ).CQ;
      public static readonly ArrayQuery<Tables?> HasCustomAttribute = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.MethodDef, Tables.Field, Tables.TypeRef, Tables.TypeDef, Tables.Parameter,
            Tables.InterfaceImpl, Tables.MemberRef, Tables.Module, Tables.DeclSecurity, Tables.Property, Tables.Event,
            Tables.StandaloneSignature, Tables.ModuleRef, Tables.TypeSpec, Tables.Assembly, Tables.AssemblyRef, Tables.File,
            Tables.ExportedType, Tables.ManifestResource, Tables.GenericParameter, Tables.GenericParameterConstraint, Tables.MethodSpec } ).CQ;
      public static readonly ArrayQuery<Tables?> HasFieldMarshal = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.Field, Tables.Parameter } ).CQ;
      public static readonly ArrayQuery<Tables?> HasSecurity = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.TypeDef, Tables.MethodDef, Tables.Assembly } ).CQ;
      public static readonly ArrayQuery<Tables?> MemberRefParent = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.TypeDef, Tables.TypeRef, Tables.ModuleRef, Tables.MethodDef, Tables.TypeSpec } ).CQ;
      public static readonly ArrayQuery<Tables?> HasSemantics = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.Event, Tables.Property } ).CQ;
      public static readonly ArrayQuery<Tables?> MethodDefOrRef = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.MethodDef, Tables.MemberRef } ).CQ;
      public static readonly ArrayQuery<Tables?> MemberForwarded = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.Field, Tables.MethodDef } ).CQ;
      public static readonly ArrayQuery<Tables?> Implementation = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.File, Tables.AssemblyRef, Tables.ExportedType } ).CQ;
      public static readonly ArrayQuery<Tables?> CustomAttributeType = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { null, null, Tables.MethodDef, Tables.MemberRef, null } ).CQ;
      public static readonly ArrayQuery<Tables?> ResolutionScope = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.Module, Tables.ModuleRef, Tables.AssemblyRef, Tables.TypeRef } ).CQ;
      public static readonly ArrayQuery<Tables?> TypeOrMethodDef = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy( new Tables?[] { Tables.TypeDef, Tables.MethodDef } ).CQ;



      public static Int32 GetTagBitSize( Int32 referencedTablesLength )
      {
         return BinaryUtils.Log2( (UInt32) referencedTablesLength );
      }

      private readonly ArrayQuery<Tables?> _tablesArray;
      private readonly IDictionary<Tables, Int32> _tablesDictionary;
      private readonly Int32 _tagBitMask;
      private readonly Int32 _tagBitSize;

      public CodedTableIndexDecoder(
         ArrayQuery<Tables?> possibleTables
         )
      {
         ArgumentValidator.ValidateNotNull( "Possible tables", possibleTables );

         this._tablesArray = possibleTables;
         this._tablesDictionary = possibleTables
            .Select( ( t, idx ) => Tuple.Create( t, idx ) )
            .Where( t => t.Item1.HasValue )
            .ToDictionary_Preserve( t => t.Item1.Value, t => t.Item2 );
         this._tagBitSize = GetTagBitSize( possibleTables.Count );
         this._tagBitMask = ( 1 << this._tagBitSize ) - 1;
      }

      public TableIndex? DecodeTableIndex( Int32 codedIndex )
      {
         TableIndex? retVal;
         var tableIndex = this._tagBitMask & codedIndex;
         if ( tableIndex < this._tablesArray.Count )
         {
            var tableNullable = this._tablesArray[tableIndex];
            if ( tableNullable.HasValue )
            {
               var rowIdx = ( ( (UInt32) codedIndex ) >> this._tagBitSize );
               retVal = rowIdx > 0 ?
                  new TableIndex( tableNullable.Value, (Int32) ( rowIdx - 1 ) ) :
                  (TableIndex?) null;
            }
            else
            {
               retVal = null;
            }
         }
         else
         {
            retVal = null;
         }

         return retVal;
      }

      public Int32 EncodeTableIndex( TableIndex? tableIndex )
      {
         Int32 retVal;
         if ( tableIndex.HasValue )
         {
            var tIdxValue = tableIndex.Value;
            Int32 tableArrayIndex;
            retVal = this._tablesDictionary.TryGetValue( tIdxValue.Table, out tableArrayIndex ) ?
               ( ( ( tIdxValue.Index + 1 ) << this._tagBitSize ) | tableArrayIndex ) :
               0;
         }
         else
         {
            retVal = 0;
         }

         return retVal;
      }

   }

   public struct ColumnSettingArguments<TRow, TRowArgs>
      where TRow : class
      where TRowArgs : class
   {

      public ColumnSettingArguments(
         Tables table,
         Int32 rowIndex,
         TRow row,
         TRowArgs args
         )
      {
         ArgumentValidator.ValidateNotNull( "Row", row );
         ArgumentValidator.ValidateNotNull( "Row arguments", args );

         this.Table = table;
         this.RowIndex = rowIndex;
         this.Row = row;
         this.RowArgs = args;
      }

      public Tables Table { get; }

      public Int32 RowIndex { get; }

      public TRow Row { get; }

      public TRowArgs RowArgs { get; }
   }

   public partial class DefaultMetaDataSerializationSupportProvider : MetaDataSerializationSupportProvider
   {
      private static readonly DefaultMetaDataSerializationSupportProvider _instance = new DefaultMetaDataSerializationSupportProvider();

      public static MetaDataSerializationSupportProvider Instance
      {
         get
         {
            return _instance;
         }
      }

      private readonly IEnumerable<ColumnSerializationInfo>[] _columnInfos;

      public DefaultMetaDataSerializationSupportProvider(
         IEnumerable<IEnumerable<ColumnSerializationInfo>> columnInfos = null
         )
      {
         this._columnInfos = ( columnInfos ?? CreateDefaultColumnInfos() ).ToArray();
      }

      protected IEnumerable<IEnumerable<ColumnSerializationInfo>> CreateDefaultColumnInfos()
      {
         yield return GetModuleDefColumns();
         yield return GetTypeRefColumns();
         yield return GetTypeDefColumns();
         yield return GetFieldPtrColumns();
         yield return GetFieldDefColumns();
         yield return GetMethodPtrColumns();
         yield return GetMethodDefColumns();
         yield return GetParamPtrColumns();
         yield return GetParamColumns();
         yield return GetInterfaceImplColumns();
         yield return GetMemberRefColumns();
         yield return GetConstantColumns();
         yield return GetCustomAttributeColumns();
         yield return GetFieldMarshalColumns();
         yield return GetDeclSecurityColumns();
         yield return GetClassLayoutColumns();
         yield return GetFieldLayoutColumns();
         yield return GetStandaloneSigColumns();
         yield return GetEventMapColumns();
         yield return GetEventPtrColumns();
         yield return GetEventDefColumns();
         yield return GetPropertyMapColumns();
         yield return GetPropertyPtrColumns();
         yield return GetPropertyDefColumns();
         yield return GetMethodSemanticsColumns();
         yield return GetMethodImplColumns();
         yield return GetModuleRefColumns();
         yield return GetTypeSpecColumns();
         yield return GetImplMapColumns();
         yield return GetFieldRVAColumns();
         yield return GetENCLogColumns();
         yield return GetENCMapColumns();
         yield return GetAssemblyDefColumns();
         yield return GetAssemblyDefProcessorColumns();
         yield return GetAssemblyDefOSColumns();
         yield return GetAssemblyRefColumns();
         yield return GetAssemblyRefProcessorColumns();
         yield return GetAssemblyRefOSColumns();
         yield return GetFileColumns();
         yield return GetExportedTypeColumns();
         yield return GetManifestResourceColumns();
         yield return GetNestedClassColumns();
         yield return GetGenericParamColumns();
         yield return GetMethodSpecColumns();
         yield return GetGenericParamConstraintColumns();
      }

      public virtual IEnumerable<TableSerializationInfo> CreateTableSerializationInfos()
      {
         yield return this.CreateTableInfo<RawModuleDefinition, ModuleDefinition>( Tables.Module );
         yield return this.CreateTableInfo<RawTypeReference, TypeReference>( Tables.TypeRef );
         yield return this.CreateTableInfo<RawTypeDefinition, TypeDefinition>( Tables.TypeDef );
         yield return this.CreateTableInfo<RawFieldDefinitionPointer, FieldDefinitionPointer>( Tables.FieldPtr );
         yield return this.CreateTableInfo<RawFieldDefinition, FieldDefinition>( Tables.Field );
         yield return this.CreateTableInfo<RawMethodDefinitionPointer, MethodDefinitionPointer>( Tables.MethodPtr );
         yield return this.CreateTableInfo<RawMethodDefinition, MethodDefinition>( Tables.MethodDef );
         yield return this.CreateTableInfo<RawParameterDefinitionPointer, ParameterDefinitionPointer>( Tables.ParameterPtr );
         yield return this.CreateTableInfo<RawParameterDefinition, ParameterDefinition>( Tables.Parameter );
         yield return this.CreateTableInfo<RawInterfaceImplementation, InterfaceImplementation>( Tables.InterfaceImpl );
         yield return this.CreateTableInfo<RawMemberReference, MemberReference>( Tables.MemberRef );
         yield return this.CreateTableInfo<RawConstantDefinition, ConstantDefinition>( Tables.Constant );
         yield return this.CreateTableInfo<RawCustomAttributeDefinition, CustomAttributeDefinition>( Tables.CustomAttribute );
         yield return this.CreateTableInfo<RawFieldMarshal, FieldMarshal>( Tables.FieldMarshal );
         yield return this.CreateTableInfo<RawSecurityDefinition, SecurityDefinition>( Tables.DeclSecurity );
         yield return this.CreateTableInfo<RawClassLayout, ClassLayout>( Tables.ClassLayout );
         yield return this.CreateTableInfo<RawFieldLayout, FieldLayout>( Tables.FieldLayout );
         yield return this.CreateTableInfo<RawStandaloneSignature, StandaloneSignature>( Tables.StandaloneSignature );
         yield return this.CreateTableInfo<RawEventMap, EventMap>( Tables.EventMap );
         yield return this.CreateTableInfo<RawEventDefinitionPointer, EventDefinitionPointer>( Tables.EventPtr );
         yield return this.CreateTableInfo<RawEventDefinition, EventDefinition>( Tables.Event );
         yield return this.CreateTableInfo<RawPropertyMap, PropertyMap>( Tables.PropertyMap );
         yield return this.CreateTableInfo<RawPropertyDefinitionPointer, PropertyDefinitionPointer>( Tables.PropertyPtr );
         yield return this.CreateTableInfo<RawPropertyDefinition, PropertyDefinition>( Tables.Property );
         yield return this.CreateTableInfo<RawMethodSemantics, MethodSemantics>( Tables.MethodSemantics );
         yield return this.CreateTableInfo<RawMethodImplementation, MethodImplementation>( Tables.MethodImpl );
         yield return this.CreateTableInfo<RawModuleReference, ModuleReference>( Tables.ModuleRef );
         yield return this.CreateTableInfo<RawTypeSpecification, TypeSpecification>( Tables.TypeSpec );
         yield return this.CreateTableInfo<RawMethodImplementationMap, MethodImplementationMap>( Tables.ImplMap );
         yield return this.CreateTableInfo<RawFieldRVA, FieldRVA>( Tables.FieldRVA );
         yield return this.CreateTableInfo<RawEditAndContinueLog, EditAndContinueLog>( Tables.EncLog );
         yield return this.CreateTableInfo<RawEditAndContinueMap, EditAndContinueMap>( Tables.EncMap );
         yield return this.CreateTableInfo<RawAssemblyDefinition, AssemblyDefinition>( Tables.Assembly );
#pragma warning disable 618
         yield return this.CreateTableInfo<RawAssemblyDefinitionProcessor, AssemblyDefinitionProcessor>( Tables.AssemblyProcessor );
         yield return this.CreateTableInfo<RawAssemblyDefinitionOS, AssemblyDefinitionOS>( Tables.AssemblyOS );
#pragma warning restore 618
         yield return this.CreateTableInfo<RawAssemblyReference, AssemblyReference>( Tables.AssemblyRef );
#pragma warning disable 618
         yield return this.CreateTableInfo<RawAssemblyReferenceProcessor, AssemblyReferenceProcessor>( Tables.AssemblyRefProcessor );
         yield return this.CreateTableInfo<RawAssemblyReferenceOS, AssemblyReferenceOS>( Tables.AssemblyRefOS );
#pragma warning restore 618
         yield return this.CreateTableInfo<RawFileReference, FileReference>( Tables.File );
         yield return this.CreateTableInfo<RawExportedType, ExportedType>( Tables.ExportedType );
         yield return this.CreateTableInfo<RawManifestResource, ManifestResource>( Tables.ManifestResource );
         yield return this.CreateTableInfo<RawNestedClassDefinition, NestedClassDefinition>( Tables.NestedClass );
         yield return this.CreateTableInfo<RawGenericParameterDefinition, GenericParameterDefinition>( Tables.GenericParameter );
         yield return this.CreateTableInfo<RawMethodSpecification, MethodSpecification>( Tables.MethodSpec );
         yield return this.CreateTableInfo<RawGenericParameterConstraintDefinition, GenericParameterConstraintDefinition>( Tables.GenericParameterConstraint );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawModuleDefinition, ModuleDefinition>> GetModuleDefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.Generation ), ( r, v ) => r.Generation = v, ( args, v ) => args.Row.Generation = (Int16) v, row => row.Generation );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v, row => row.Name );
         yield return DefaultColumnSerializationInfoFactory.GUID<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.ModuleGUID ), ( r, v ) => r.ModuleGUID = v, ( args, v ) => args.Row.ModuleGUID = v, row => row.ModuleGUID );
         yield return DefaultColumnSerializationInfoFactory.GUID<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.EditAndContinueGUID ), ( r, v ) => r.EditAndContinueGUID = v, ( args, v ) => args.Row.EditAndContinueGUID = v, row => row.EditAndContinueGUID );
         yield return DefaultColumnSerializationInfoFactory.GUID<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.EditAndContinueBaseGUID ), ( r, v ) => r.EditAndContinueBaseGUID = v, ( args, v ) => args.Row.EditAndContinueBaseGUID = v, row => row.EditAndContinueBaseGUID );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawTypeReference, TypeReference>> GetTypeRefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawTypeReference, TypeReference>( nameof( RawTypeReference.ResolutionScope ), CodedTableIndexDecoder.ResolutionScope, ( r, v ) => r.ResolutionScope = v, ( args, v ) => args.Row.ResolutionScope = v, row => row.ResolutionScope );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawTypeReference, TypeReference>( nameof( RawTypeReference.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v, row => row.Name );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawTypeReference, TypeReference>( nameof( RawTypeReference.Namespace ), ( r, v ) => r.Namespace = v, ( args, v ) => args.Row.Namespace = v, row => row.Namespace );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawTypeDefinition, TypeDefinition>> GetTypeDefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.Attributes ), ( r, v ) => r.Attributes = (TypeAttributes) v, ( args, v ) => args.Row.Attributes = (TypeAttributes) v, row => (Int32) row.Attributes );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v, row => row.Name );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.Namespace ), ( r, v ) => r.Namespace = v, ( args, v ) => args.Row.Namespace = v, row => row.Namespace );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.BaseType ), CodedTableIndexDecoder.TypeDefOrRef, ( r, v ) => r.BaseType = v, ( args, v ) => args.Row.BaseType = v, row => row.BaseType );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.FieldList ), Tables.Field, ( r, v ) => r.FieldList = v, ( args, v ) => args.Row.FieldList = v, row => row.FieldList );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.MethodList ), Tables.MethodDef, ( r, v ) => r.MethodList = v, ( args, v ) => args.Row.MethodList = v, row => row.MethodList );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawFieldDefinitionPointer, FieldDefinitionPointer>> GetFieldPtrColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawFieldDefinitionPointer, FieldDefinitionPointer>( nameof( RawFieldDefinitionPointer.FieldIndex ), Tables.Field, ( r, v ) => r.FieldIndex = v, ( args, v ) => args.Row.FieldIndex = v, row => row.FieldIndex );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawFieldDefinition, FieldDefinition>> GetFieldDefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawFieldDefinition, FieldDefinition>( nameof( RawFieldDefinition.Attributes ), ( r, v ) => r.Attributes = (FieldAttributes) v, ( args, v ) => args.Row.Attributes = (FieldAttributes) v, row => (Int32) row.Attributes );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawFieldDefinition, FieldDefinition>( nameof( RawFieldDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v, row => row.Name );
         yield return DefaultColumnSerializationInfoFactory.BLOBNonTypeSignature<RawFieldDefinition, FieldDefinition, FieldSignature>( nameof( RawFieldDefinition.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v, row => row.Signature );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawMethodDefinitionPointer, MethodDefinitionPointer>> GetMethodPtrColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawMethodDefinitionPointer, MethodDefinitionPointer>( nameof( RawMethodDefinitionPointer.MethodIndex ), Tables.MethodDef, ( r, v ) => r.MethodIndex = v, ( args, v ) => args.Row.MethodIndex = v, row => row.MethodIndex );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawMethodDefinition, MethodDefinition>> GetMethodDefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.RawValueStorageColumn<RawMethodDefinition, MethodDefinition>( nameof( RawMethodDefinition.RVA ), ( r, v ) => r.RVA = v, ( args, rva ) => args.Row.IL = this.DeserializeIL( args.RowArgs, rva ), args => this.WriteMethodIL( args.RowArgs, args.Row.IL ) );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawMethodDefinition, MethodDefinition>( nameof( RawMethodDefinition.ImplementationAttributes ), ( r, v ) => r.ImplementationAttributes = (MethodImplAttributes) v, ( args, v ) => args.Row.ImplementationAttributes = (MethodImplAttributes) v, row => (Int32) row.ImplementationAttributes );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawMethodDefinition, MethodDefinition>( nameof( RawMethodDefinition.Attributes ), ( r, v ) => r.Attributes = (MethodAttributes) v, ( args, v ) => args.Row.Attributes = (MethodAttributes) v, row => (Int32) row.Attributes );
         yield return DefaultColumnSerializationInfoFactory.BLOBNonTypeSignature<RawMethodDefinition, MethodDefinition, MethodDefinitionSignature>( nameof( RawMethodDefinition.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v, row => row.Signature );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawMethodDefinition, MethodDefinition>( nameof( RawMethodDefinition.ParameterList ), Tables.Parameter, ( r, v ) => r.ParameterList = v, ( args, v ) => args.Row.ParameterList = v, row => row.ParameterList );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawParameterDefinitionPointer, ParameterDefinitionPointer>> GetParamPtrColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawParameterDefinitionPointer, ParameterDefinitionPointer>( nameof( RawParameterDefinitionPointer.ParameterIndex ), Tables.Parameter, ( r, v ) => r.ParameterIndex = v, ( args, v ) => args.Row.ParameterIndex = v, row => row.ParameterIndex );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawParameterDefinition, ParameterDefinition>> GetParamColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawParameterDefinition, ParameterDefinition>( nameof( RawParameterDefinition.Attributes ), ( r, v ) => r.Attributes = (ParameterAttributes) v, ( args, v ) => args.Row.Attributes = (ParameterAttributes) v, row => (Int32) row.Attributes );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawParameterDefinition, ParameterDefinition>( nameof( RawParameterDefinition.Sequence ), ( r, v ) => r.Sequence = v, ( args, v ) => args.Row.Sequence = v, row => row.Sequence );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawParameterDefinition, ParameterDefinition>( nameof( RawParameterDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v, row => row.Name );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawInterfaceImplementation, InterfaceImplementation>> GetInterfaceImplColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawInterfaceImplementation, InterfaceImplementation>( nameof( RawInterfaceImplementation.Class ), Tables.TypeDef, ( r, v ) => r.Class = v, ( args, v ) => args.Row.Class = v, row => row.Class );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawInterfaceImplementation, InterfaceImplementation>( nameof( RawInterfaceImplementation.Interface ), CodedTableIndexDecoder.TypeDefOrRef, ( r, v ) => r.Interface = v, ( args, v ) => args.Row.Interface = v.GetValueOrDefault(), row => row.Interface );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawMemberReference, MemberReference>> GetMemberRefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawMemberReference, MemberReference>( nameof( RawMemberReference.DeclaringType ), CodedTableIndexDecoder.MemberRefParent, ( r, v ) => r.DeclaringType = v, ( args, v ) => args.Row.DeclaringType = v.GetValueOrDefault(), row => row.DeclaringType );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawMemberReference, MemberReference>( nameof( RawMemberReference.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v, row => row.Name );
         yield return DefaultColumnSerializationInfoFactory.BLOBNonTypeSignature<RawMemberReference, MemberReference, AbstractSignature>( nameof( RawMemberReference.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v, row => row.Signature );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawConstantDefinition, ConstantDefinition>> GetConstantColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant8<RawConstantDefinition, ConstantDefinition>( nameof( RawConstantDefinition.Type ), ( r, v ) => r.Type = (SignatureElementTypes) v, ( args, v ) => args.Row.Type = (SignatureElementTypes) v, row => (Int32) row.Type );
         yield return DefaultColumnSerializationInfoFactory.Constant8<RawConstantDefinition, ConstantDefinition>( nameof( RawConstantDefinition.Padding ), ( r, v ) => r.Padding = (Byte) v, ( args, v ) => { }, row => 0 );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawConstantDefinition, ConstantDefinition>( nameof( RawConstantDefinition.Parent ), CodedTableIndexDecoder.HasConstant, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v.GetValueOrDefault(), row => row.Parent );
         yield return DefaultColumnSerializationInfoFactory.BLOBCustom<RawConstantDefinition, ConstantDefinition>( nameof( RawConstantDefinition.Value ), ( r, v ) => r.Value = v, ( args, v, blobs ) => args.Row.Value = blobs.ReadConstantValue( v, args.Row.Type ), args => args.RowArgs.Array.CreateConstantBytes( args.Row.Value ) );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawCustomAttributeDefinition, CustomAttributeDefinition>> GetCustomAttributeColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawCustomAttributeDefinition, CustomAttributeDefinition>( nameof( RawCustomAttributeDefinition.Parent ), CodedTableIndexDecoder.HasCustomAttribute, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v.GetValueOrDefault(), row => row.Parent );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawCustomAttributeDefinition, CustomAttributeDefinition>( nameof( RawCustomAttributeDefinition.Type ), CodedTableIndexDecoder.CustomAttributeType, ( r, v ) => r.Type = v, ( args, v ) => args.Row.Type = v.GetValueOrDefault(), row => row.Type );
         yield return DefaultColumnSerializationInfoFactory.BLOBCASignature<RawCustomAttributeDefinition, CustomAttributeDefinition>( nameof( RawCustomAttributeDefinition.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawFieldMarshal, FieldMarshal>> GetFieldMarshalColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawFieldMarshal, FieldMarshal>( nameof( RawFieldMarshal.Parent ), CodedTableIndexDecoder.HasFieldMarshal, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v.GetValueOrDefault(), row => row.Parent );
         yield return DefaultColumnSerializationInfoFactory.BLOBMarshalingInfo<RawFieldMarshal, FieldMarshal>( nameof( RawFieldMarshal.NativeType ), ( r, v ) => r.NativeType = v, ( args, v ) => args.Row.NativeType = v, row => row.NativeType );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawSecurityDefinition, SecurityDefinition>> GetDeclSecurityColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawSecurityDefinition, SecurityDefinition>( nameof( RawSecurityDefinition.Action ), ( r, v ) => r.Action = (SecurityAction) v, ( args, v ) => args.Row.Action = (SecurityAction) v, row => (Int32) row.Action );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawSecurityDefinition, SecurityDefinition>( nameof( RawSecurityDefinition.Parent ), CodedTableIndexDecoder.HasSecurity, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v.GetValueOrDefault(), row => row.Parent );
         yield return DefaultColumnSerializationInfoFactory.BLOBSecurityInformation<RawSecurityDefinition, SecurityDefinition>( nameof( RawSecurityDefinition.PermissionSets ), ( r, v ) => r.PermissionSets = v, ( args, v ) => args.Row.PermissionSets.AddRange( v ), row => row.PermissionSets );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawClassLayout, ClassLayout>> GetClassLayoutColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawClassLayout, ClassLayout>( nameof( RawClassLayout.PackingSize ), ( r, v ) => r.PackingSize = v, ( args, v ) => args.Row.PackingSize = v, row => row.PackingSize );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawClassLayout, ClassLayout>( nameof( RawClassLayout.ClassSize ), ( r, v ) => r.ClassSize = v, ( args, v ) => args.Row.ClassSize = v, row => row.ClassSize );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawClassLayout, ClassLayout>( nameof( RawClassLayout.Parent ), Tables.TypeDef, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v, row => row.Parent );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawFieldLayout, FieldLayout>> GetFieldLayoutColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawFieldLayout, FieldLayout>( nameof( RawFieldLayout.Offset ), ( r, v ) => r.Offset = v, ( args, v ) => args.Row.Offset = v, row => row.Offset );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawFieldLayout, FieldLayout>( nameof( RawFieldLayout.Field ), Tables.Field, ( r, v ) => r.Field = v, ( args, v ) => args.Row.Field = v, row => row.Field );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawStandaloneSignature, StandaloneSignature>> GetStandaloneSigColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.BLOBCustom<RawStandaloneSignature, StandaloneSignature>( nameof( RawStandaloneSignature.Signature ), ( r, v ) => r.Signature = v, ( args, v, blobs ) =>
         {
            Boolean wasFieldSig;
            args.Row.Signature = blobs.ReadNonTypeSignature( v, false, true, out wasFieldSig );
            args.Row.StoreSignatureAsFieldSignature = wasFieldSig;
         }, args => args.RowArgs.Array.CreateStandaloneSignature( args.Row ) );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawEventMap, EventMap>> GetEventMapColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawEventMap, EventMap>( nameof( RawEventMap.Parent ), Tables.TypeDef, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v, row => row.Parent );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawEventMap, EventMap>( nameof( RawEventMap.EventList ), Tables.Event, ( r, v ) => r.EventList = v, ( args, v ) => args.Row.EventList = v, row => row.EventList );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawEventDefinitionPointer, EventDefinitionPointer>> GetEventPtrColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawEventDefinitionPointer, EventDefinitionPointer>( nameof( RawEventDefinitionPointer.EventIndex ), Tables.Event, ( r, v ) => r.EventIndex = v, ( args, v ) => args.Row.EventIndex = v, row => row.EventIndex );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawEventDefinition, EventDefinition>> GetEventDefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawEventDefinition, EventDefinition>( nameof( RawEventDefinition.Attributes ), ( r, v ) => r.Attributes = (EventAttributes) v, ( args, v ) => args.Row.Attributes = (EventAttributes) v, row => (Int32) row.Attributes );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawEventDefinition, EventDefinition>( nameof( RawEventDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v, row => row.Name );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawEventDefinition, EventDefinition>( nameof( RawEventDefinition.EventType ), CodedTableIndexDecoder.TypeDefOrRef, ( r, v ) => r.EventType = v, ( args, v ) => args.Row.EventType = v.GetValueOrDefault(), row => row.EventType );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawPropertyMap, PropertyMap>> GetPropertyMapColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawPropertyMap, PropertyMap>( nameof( RawPropertyMap.Parent ), Tables.TypeDef, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v, row => row.Parent );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawPropertyMap, PropertyMap>( nameof( RawPropertyMap.PropertyList ), Tables.Property, ( r, v ) => r.PropertyList = v, ( args, v ) => args.Row.PropertyList = v, row => row.PropertyList );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawPropertyDefinitionPointer, PropertyDefinitionPointer>> GetPropertyPtrColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawPropertyDefinitionPointer, PropertyDefinitionPointer>( nameof( RawPropertyDefinitionPointer.PropertyIndex ), Tables.Property, ( r, v ) => r.PropertyIndex = v, ( args, v ) => args.Row.PropertyIndex = v, row => row.PropertyIndex );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawPropertyDefinition, PropertyDefinition>> GetPropertyDefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawPropertyDefinition, PropertyDefinition>( nameof( RawPropertyDefinition.Attributes ), ( r, v ) => r.Attributes = (PropertyAttributes) v, ( args, v ) => args.Row.Attributes = (PropertyAttributes) v, row => (Int32) row.Attributes );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawPropertyDefinition, PropertyDefinition>( nameof( RawPropertyDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v, row => row.Name );
         yield return DefaultColumnSerializationInfoFactory.BLOBNonTypeSignature<RawPropertyDefinition, PropertyDefinition, PropertySignature>( nameof( RawPropertyDefinition.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v, row => row.Signature );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawMethodSemantics, MethodSemantics>> GetMethodSemanticsColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawMethodSemantics, MethodSemantics>( nameof( RawMethodSemantics.Attributes ), ( r, v ) => r.Attributes = (MethodSemanticsAttributes) v, ( args, v ) => args.Row.Attributes = (MethodSemanticsAttributes) v, row => (Int32) row.Attributes );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawMethodSemantics, MethodSemantics>( nameof( RawMethodSemantics.Method ), Tables.MethodDef, ( r, v ) => r.Method = v, ( args, v ) => args.Row.Method = v, row => row.Method );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawMethodSemantics, MethodSemantics>( nameof( RawMethodSemantics.Associaton ), CodedTableIndexDecoder.HasSemantics, ( r, v ) => r.Associaton = v, ( args, v ) => args.Row.Associaton = v.GetValueOrDefault(), row => row.Associaton );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawMethodImplementation, MethodImplementation>> GetMethodImplColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawMethodImplementation, MethodImplementation>( nameof( RawMethodImplementation.Class ), Tables.TypeDef, ( r, v ) => r.Class = v, ( args, v ) => args.Row.Class = v, row => row.Class );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawMethodImplementation, MethodImplementation>( nameof( RawMethodImplementation.MethodBody ), CodedTableIndexDecoder.MethodDefOrRef, ( r, v ) => r.MethodBody = v, ( args, v ) => args.Row.MethodBody = v.GetValueOrDefault(), row => row.MethodBody );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawMethodImplementation, MethodImplementation>( nameof( RawMethodImplementation.MethodDeclaration ), CodedTableIndexDecoder.MethodDefOrRef, ( r, v ) => r.MethodDeclaration = v, ( args, v ) => args.Row.MethodDeclaration = v.GetValueOrDefault(), row => row.MethodDeclaration );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawModuleReference, ModuleReference>> GetModuleRefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawModuleReference, ModuleReference>( nameof( RawModuleReference.ModuleName ), ( r, v ) => r.ModuleName = v, ( args, v ) => args.Row.ModuleName = v, row => row.ModuleName );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawTypeSpecification, TypeSpecification>> GetTypeSpecColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.BLOBTypeSignature<RawTypeSpecification, TypeSpecification>( nameof( RawTypeSpecification.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v, row => row.Signature );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawMethodImplementationMap, MethodImplementationMap>> GetImplMapColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawMethodImplementationMap, MethodImplementationMap>( nameof( RawMethodImplementationMap.Attributes ), ( r, v ) => r.Attributes = (PInvokeAttributes) v, ( args, v ) => args.Row.Attributes = (PInvokeAttributes) v, row => (Int32) row.Attributes );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawMethodImplementationMap, MethodImplementationMap>( nameof( RawMethodImplementationMap.MemberForwarded ), CodedTableIndexDecoder.MemberForwarded, ( r, v ) => r.MemberForwarded = v, ( args, v ) => args.Row.MemberForwarded = v.GetValueOrDefault(), row => row.MemberForwarded );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawMethodImplementationMap, MethodImplementationMap>( nameof( RawMethodImplementationMap.ImportName ), ( r, v ) => r.ImportName = v, ( args, v ) => args.Row.ImportName = v, row => row.ImportName );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawMethodImplementationMap, MethodImplementationMap>( nameof( RawMethodImplementationMap.ImportScope ), Tables.ModuleRef, ( r, v ) => r.ImportScope = v, ( args, v ) => args.Row.ImportScope = v, row => row.ImportScope );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawFieldRVA, FieldRVA>> GetFieldRVAColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.RawValueStorageColumn<RawFieldRVA, FieldRVA>( nameof( RawFieldRVA.RVA ), ( r, v ) => r.RVA = v, ( args, rva ) => args.Row.Data = this.DeserializeConstantValue( args.RowArgs, args.Row, rva ), args => this.WriteConstant( args.RowArgs, args.Row.Data ) );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawFieldRVA, FieldRVA>( nameof( RawFieldRVA.Field ), Tables.Field, ( r, v ) => r.Field = v, ( args, v ) => args.Row.Field = v, row => row.Field );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawEditAndContinueLog, EditAndContinueLog>> GetENCLogColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawEditAndContinueLog, EditAndContinueLog>( nameof( RawEditAndContinueLog.Token ), ( r, v ) => r.Token = v, ( args, v ) => args.Row.Token = v, row => row.Token );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawEditAndContinueLog, EditAndContinueLog>( nameof( RawEditAndContinueLog.FuncCode ), ( r, v ) => r.FuncCode = v, ( args, v ) => args.Row.FuncCode = v, row => row.FuncCode );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawEditAndContinueMap, EditAndContinueMap>> GetENCMapColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawEditAndContinueMap, EditAndContinueMap>( nameof( RawEditAndContinueMap.Token ), ( r, v ) => r.Token = v, ( args, v ) => args.Row.Token = v, row => row.Token );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawAssemblyDefinition, AssemblyDefinition>> GetAssemblyDefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.HashAlgorithm ), ( r, v ) => r.HashAlgorithm = (AssemblyHashAlgorithm) v, ( args, v ) => args.Row.HashAlgorithm = (AssemblyHashAlgorithm) v, row => (Int32) row.HashAlgorithm );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.MajorVersion ), ( r, v ) => r.MajorVersion = v, ( args, v ) => args.Row.AssemblyInformation.VersionMajor = v, row => row.AssemblyInformation.VersionMajor );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.MinorVersion ), ( r, v ) => r.MinorVersion = v, ( args, v ) => args.Row.AssemblyInformation.VersionMinor = v, row => row.AssemblyInformation.VersionMinor );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.BuildNumber ), ( r, v ) => r.BuildNumber = v, ( args, v ) => args.Row.AssemblyInformation.VersionBuild = v, row => row.AssemblyInformation.VersionBuild );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.RevisionNumber ), ( r, v ) => r.RevisionNumber = v, ( args, v ) => args.Row.AssemblyInformation.VersionRevision = v, row => row.AssemblyInformation.VersionRevision );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.Attributes ), ( r, v ) => r.Attributes = (AssemblyFlags) v, ( args, v ) => args.Row.Attributes = (AssemblyFlags) v, row => (Int32) row.Attributes );
         yield return DefaultColumnSerializationInfoFactory.BLOBCustom<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.PublicKey ), ( r, v ) => r.PublicKey = v, ( args, v, blobs ) => args.Row.AssemblyInformation.PublicKeyOrToken = blobs.GetBLOB( v ), args =>
         {
            var pk = args.Row.AssemblyInformation.PublicKeyOrToken;
            return pk.IsNullOrEmpty() ? args.RowArgs.ThisAssemblyPublicKeyIfPresentNull.ToArray() : pk;
         } );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.AssemblyInformation.Name = v, row => row.AssemblyInformation.Name );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.Culture ), ( r, v ) => r.Culture = v, ( args, v ) => args.Row.AssemblyInformation.Culture = v, row => row.AssemblyInformation.Culture );
      }
#pragma warning disable 618
      protected IEnumerable<DefaultColumnSerializationInfo<RawAssemblyDefinitionProcessor, AssemblyDefinitionProcessor>> GetAssemblyDefProcessorColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyDefinitionProcessor, AssemblyDefinitionProcessor>( nameof( RawAssemblyDefinitionProcessor.Processor ), ( r, v ) => r.Processor = v, ( args, v ) => args.Row.Processor = v, row => row.Processor );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawAssemblyDefinitionOS, AssemblyDefinitionOS>> GetAssemblyDefOSColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyDefinitionOS, AssemblyDefinitionOS>( nameof( RawAssemblyDefinitionOS.OSPlatformID ), ( r, v ) => r.OSPlatformID = v, ( args, v ) => args.Row.OSPlatformID = v, row => row.OSPlatformID );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyDefinitionOS, AssemblyDefinitionOS>( nameof( RawAssemblyDefinitionOS.OSMajorVersion ), ( r, v ) => r.OSMajorVersion = v, ( args, v ) => args.Row.OSMajorVersion = v, row => row.OSMajorVersion );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyDefinitionOS, AssemblyDefinitionOS>( nameof( RawAssemblyDefinitionOS.OSMinorVersion ), ( r, v ) => r.OSMinorVersion = v, ( args, v ) => args.Row.OSMinorVersion = v, row => row.OSMinorVersion );
      }
#pragma warning restore 618

      protected IEnumerable<DefaultColumnSerializationInfo<RawAssemblyReference, AssemblyReference>> GetAssemblyRefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.MajorVersion ), ( r, v ) => r.MajorVersion = v, ( args, v ) => args.Row.AssemblyInformation.VersionMajor = v, row => row.AssemblyInformation.VersionMajor );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.MinorVersion ), ( r, v ) => r.MinorVersion = v, ( args, v ) => args.Row.AssemblyInformation.VersionMinor = v, row => row.AssemblyInformation.VersionMinor );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.BuildNumber ), ( r, v ) => r.BuildNumber = v, ( args, v ) => args.Row.AssemblyInformation.VersionBuild = v, row => row.AssemblyInformation.VersionBuild );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.RevisionNumber ), ( r, v ) => r.RevisionNumber = v, ( args, v ) => args.Row.AssemblyInformation.VersionRevision = v, row => row.AssemblyInformation.VersionRevision );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.Attributes ), ( r, v ) => r.Attributes = (AssemblyFlags) v, ( args, v ) => args.Row.Attributes = (AssemblyFlags) v, row => (Int32) row.Attributes );
         yield return DefaultColumnSerializationInfoFactory.BLOBArray<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.PublicKeyOrToken ), ( r, v ) => r.PublicKeyOrToken = v, ( args, v ) => args.Row.AssemblyInformation.PublicKeyOrToken = v, row => row.AssemblyInformation.PublicKeyOrToken );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.AssemblyInformation.Name = v, row => row.AssemblyInformation.Name );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.Culture ), ( r, v ) => r.Culture = v, ( args, v ) => args.Row.AssemblyInformation.Culture = v, row => row.AssemblyInformation.Culture );
         yield return DefaultColumnSerializationInfoFactory.BLOBArray<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.HashValue ), ( r, v ) => r.HashValue = v, ( args, v ) => args.Row.HashValue = v, row => row.HashValue );
      }

#pragma warning disable 618
      protected IEnumerable<DefaultColumnSerializationInfo<RawAssemblyReferenceProcessor, AssemblyReferenceProcessor>> GetAssemblyRefProcessorColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyReferenceProcessor, AssemblyReferenceProcessor>( nameof( RawAssemblyReferenceProcessor.Processor ), ( r, v ) => r.Processor = v, ( args, v ) => args.Row.Processor = v, row => row.Processor );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawAssemblyReferenceProcessor, AssemblyReferenceProcessor>( nameof( RawAssemblyReferenceProcessor.AssemblyRef ), Tables.AssemblyRef, ( r, v ) => r.AssemblyRef = v, ( args, v ) => args.Row.AssemblyRef = v, row => row.AssemblyRef );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawAssemblyReferenceOS, AssemblyReferenceOS>> GetAssemblyRefOSColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyReferenceOS, AssemblyReferenceOS>( nameof( RawAssemblyReferenceOS.OSPlatformID ), ( r, v ) => r.OSPlatformID = v, ( args, v ) => args.Row.OSPlatformID = v, row => row.OSPlatformID );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyReferenceOS, AssemblyReferenceOS>( nameof( RawAssemblyReferenceOS.OSMajorVersion ), ( r, v ) => r.OSMajorVersion = v, ( args, v ) => args.Row.OSMajorVersion = v, row => row.OSMajorVersion );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyReferenceOS, AssemblyReferenceOS>( nameof( RawAssemblyReferenceOS.OSMinorVersion ), ( r, v ) => r.OSMinorVersion = v, ( args, v ) => args.Row.OSMinorVersion = v, row => row.OSMinorVersion );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawAssemblyReferenceOS, AssemblyReferenceOS>( nameof( RawAssemblyReferenceOS.AssemblyRef ), Tables.AssemblyRef, ( r, v ) => r.AssemblyRef = v, ( args, v ) => args.Row.AssemblyRef = v, row => row.AssemblyRef );
      }
#pragma warning restore 618

      protected IEnumerable<DefaultColumnSerializationInfo<RawFileReference, FileReference>> GetFileColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawFileReference, FileReference>( nameof( RawFileReference.Attributes ), ( r, v ) => r.Attributes = (FileAttributes) v, ( args, v ) => args.Row.Attributes = (FileAttributes) v, row => (Int32) row.Attributes );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawFileReference, FileReference>( nameof( RawFileReference.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v, row => row.Name );
         yield return DefaultColumnSerializationInfoFactory.BLOBArray<RawFileReference, FileReference>( nameof( RawFileReference.HashValue ), ( r, v ) => r.HashValue = v, ( args, v ) => args.Row.HashValue = v, row => row.HashValue );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawExportedType, ExportedType>> GetExportedTypeColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawExportedType, ExportedType>( nameof( RawExportedType.Attributes ), ( r, v ) => r.Attributes = (TypeAttributes) v, ( args, v ) => args.Row.Attributes = (TypeAttributes) v, row => (Int32) row.Attributes );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawExportedType, ExportedType>( nameof( RawExportedType.TypeDefinitionIndex ), ( r, v ) => r.TypeDefinitionIndex = v, ( args, v ) => args.Row.TypeDefinitionIndex = v, row => row.TypeDefinitionIndex );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawExportedType, ExportedType>( nameof( RawExportedType.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v, row => row.Name );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawExportedType, ExportedType>( nameof( RawExportedType.Namespace ), ( r, v ) => r.Namespace = v, ( args, v ) => args.Row.Namespace = v, row => row.Namespace );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawExportedType, ExportedType>( nameof( RawExportedType.Implementation ), CodedTableIndexDecoder.Implementation, ( r, v ) => r.Implementation = v, ( args, v ) => args.Row.Implementation = v.GetValueOrDefault(), row => row.Implementation );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawManifestResource, ManifestResource>> GetManifestResourceColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.RawValueStorageColumn<RawManifestResource, ManifestResource>( nameof( RawManifestResource.Offset ), ( r, v ) => r.Offset = v, ( args, offset ) =>
         {
            var row = args.Row;
            row.Offset = offset;
            if ( !row.Implementation.HasValue )
            {
               row.DataInCurrentFile = this.DeserializeEmbeddedManifest( args.RowArgs, offset );
            }
         }, args =>
         {
            var row = args.Row;
            return row.Implementation.HasValue ?
               row.Offset :
               this.WriteEmbeddedManifestResoruce( args.RowArgs, row.DataInCurrentFile );
         } );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawManifestResource, ManifestResource>( nameof( RawManifestResource.Attributes ), ( r, v ) => r.Attributes = (ManifestResourceAttributes) v, ( args, v ) => args.Row.Attributes = (ManifestResourceAttributes) v, row => (Int32) row.Attributes );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawManifestResource, ManifestResource>( nameof( RawManifestResource.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v, row => row.Name );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawManifestResource, ManifestResource>( nameof( RawManifestResource.Implementation ), CodedTableIndexDecoder.Implementation, ( r, v ) => r.Implementation = v, ( args, v ) => args.Row.Implementation = v, row => row.Implementation );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawNestedClassDefinition, NestedClassDefinition>> GetNestedClassColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawNestedClassDefinition, NestedClassDefinition>( nameof( RawNestedClassDefinition.NestedClass ), Tables.TypeDef, ( r, v ) => r.NestedClass = v, ( args, v ) => args.Row.NestedClass = v, row => row.NestedClass );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawNestedClassDefinition, NestedClassDefinition>( nameof( RawNestedClassDefinition.EnclosingClass ), Tables.TypeDef, ( r, v ) => r.EnclosingClass = v, ( args, v ) => args.Row.EnclosingClass = v, row => row.EnclosingClass );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawGenericParameterDefinition, GenericParameterDefinition>> GetGenericParamColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawGenericParameterDefinition, GenericParameterDefinition>( nameof( RawGenericParameterDefinition.GenericParameterIndex ), ( r, v ) => r.GenericParameterIndex = v, ( args, v ) => args.Row.GenericParameterIndex = v, row => row.GenericParameterIndex );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawGenericParameterDefinition, GenericParameterDefinition>( nameof( RawGenericParameterDefinition.Attributes ), ( r, v ) => r.Attributes = (GenericParameterAttributes) v, ( args, v ) => args.Row.Attributes = (GenericParameterAttributes) v, row => (Int32) row.Attributes );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawGenericParameterDefinition, GenericParameterDefinition>( nameof( RawGenericParameterDefinition.Owner ), CodedTableIndexDecoder.TypeOrMethodDef, ( r, v ) => r.Owner = v, ( args, v ) => args.Row.Owner = v.GetValueOrDefault(), row => row.Owner );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawGenericParameterDefinition, GenericParameterDefinition>( nameof( RawGenericParameterDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v, row => row.Name );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawMethodSpecification, MethodSpecification>> GetMethodSpecColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawMethodSpecification, MethodSpecification>( nameof( RawMethodSpecification.Method ), CodedTableIndexDecoder.MethodDefOrRef, ( r, v ) => r.Method = v, ( args, v ) => args.Row.Method = v.GetValueOrDefault(), row => row.Method );
         yield return DefaultColumnSerializationInfoFactory.BLOBNonTypeSignature<RawMethodSpecification, MethodSpecification, GenericMethodSignature>( nameof( RawMethodSpecification.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v, row => row.Signature );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawGenericParameterConstraintDefinition, GenericParameterConstraintDefinition>> GetGenericParamConstraintColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawGenericParameterConstraintDefinition, GenericParameterConstraintDefinition>( nameof( RawGenericParameterConstraintDefinition.Owner ), Tables.GenericParameter, ( r, v ) => r.Owner = v, ( args, v ) => args.Row.Owner = v, row => row.Owner );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawGenericParameterConstraintDefinition, GenericParameterConstraintDefinition>( nameof( RawGenericParameterConstraintDefinition.Constraint ), CodedTableIndexDecoder.TypeDefOrRef, ( r, v ) => r.Constraint = v, ( args, v ) => args.Row.Constraint = v.GetValueOrDefault(), row => row.Constraint );
      }

      protected virtual TableSerializationInfo CreateTableInfo<TRawRow, TRow>( Tables table )
         where TRawRow : class, new()
         where TRow : class, new()
      {
         return new DefaultTableSerializationInfo<TRawRow, TRow>(
            table,
            (IEnumerable<DefaultColumnSerializationInfo<TRawRow, TRow>>) this._columnInfos[(Int32) table]
            );
      }


   }

   public class DefaultTableSerializationInfo<TRawRow, TRow> : TableSerializationInfo
      where TRawRow : class, new()
      where TRow : class, new()
   {

      private readonly DefaultColumnSerializationInfo<TRawRow, TRow>[] _columns;

      public DefaultTableSerializationInfo(
         Tables table,
         IEnumerable<DefaultColumnSerializationInfo<TRawRow, TRow>> columns
         )
      {
         ArgumentValidator.ValidateNotNull( "Columns", columns );

         this.Table = table;
         this._columns = columns.ToArray();
         this.ColumnSerializationInfos = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy(
            this._columns.Cast<ColumnSerializationInfo>().ToArray()
            ).CQ;
      }

      public Tables Table { get; }

      public ArrayQuery<ColumnSerializationInfo> ColumnSerializationInfos { get; }

      public Int32 RawValueStorageColumnCount
      {
         get
         {
            return this._columns.Count( c => c.RawValueProcessor != null );
         }
      }


      public Int32 HeapValueColumnCount
      {
         get
         {
            return this._columns.Count( c => c.HeapValueExtractor != null );
         }
      }

      public void ProcessRowForRawValues( RawValueProcessingArgs args, Int32 rowIndex, Object row, IEnumerable<Int32> rawValues )
      {
         var cArgs = new ColumnSettingArguments<TRow, RawValueProcessingArgs>( this.Table, rowIndex, (TRow) row, args );
         var colIdx = 0;
         foreach ( var value in rawValues )
         {
            var colInfo = this._columns[colIdx];
            while ( colInfo.RawValueProcessor == null && colIdx < this._columns.Length - 1 )
            {
               colInfo = this._columns[++colIdx];
            }

            if ( colInfo.RawValueProcessor == null )
            {
               break;
            }
            else
            {
               try
               {
                  colInfo.RawValueProcessor( cArgs, value );
               }
               catch
               {
                  // Ignore...
                  // TODO error reporting mechanism
               }
            }
         }
      }

      public void ExtractTableRawValues(
         CILMetaData md,
         RawValueStorage storage,
         StreamHelper stream,
         ResizableArray<Byte> array,
         WriterMetaDataStreamContainer mdStreamContainer,
         RVAConverter rvaConverter
         )
      {
         MetaDataTable tbl;
         if ( md.TryGetByTable( this.Table, out tbl ) )
         {
            var table = (MetaDataTable<TRow>) tbl;
            var cols = this._columns
               .Select( c => c.RawValueExtractor )
               .Where( e => e != null )
               .ToArray();
            if ( cols.Length > 0 )
            {
               var list = table.TableContents;
               var rArgs = new RowRawValueExtractionArguments( stream, array, mdStreamContainer, rvaConverter, md );
               for ( var i = 0; i < list.Count; ++i )
               {
                  var cArgs = new ColumnSettingArguments<TRow, RowRawValueExtractionArguments>( this.Table, i, list[i], rArgs );
                  foreach ( var col in cols )
                  {
                     Int32 rawValue;
                     try
                     {
                        rawValue = col( cArgs );
                     }
                     catch
                     {
                        // TODO error reporting
                        rawValue = 0;
                     }
                     storage.AddRawValue( rawValue );
                  }
               }
            }
         }
      }

      public void ExtractTableHeapValues(
         CILMetaData md,
         RawValueStorage storage,
         WriterMetaDataStreamContainer mdStreamContainer,
         ResizableArray<Byte> array,
         ArrayQuery<Byte> thisAssemblyPublicKeyIfPresentNull
         )
      {
         MetaDataTable tbl;
         if ( md.TryGetByTable( this.Table, out tbl ) )
         {
            var table = (MetaDataTable<TRow>) tbl;
            var cols = this._columns
               .Select( c => c.HeapValueExtractor )
               .Where( e => e != null )
               .ToArray();
            if ( cols.Length > 0 )
            {
               var list = table.TableContents;
               var rArgs = new RowHeapFillingArguments( mdStreamContainer, array, thisAssemblyPublicKeyIfPresentNull, md );
               for ( var i = 0; i < list.Count; ++i )
               {
                  var cArgs = new ColumnSettingArguments<TRow, RowHeapFillingArguments>( this.Table, i, list[i], rArgs );
                  foreach ( var col in cols )
                  {
                     Int32 rawValue;
                     try
                     {
                        rawValue = col( cArgs );
                     }
                     catch
                     {
                        // TODO error reporting
                        rawValue = 0;
                     }
                     storage.AddRawValue( rawValue );
                  }
               }
            }
         }
      }

      public IEnumerable<Int32> GetAllRawValues(
         MetaDataTable table,
         RawValueStorage previousRawValues,
         RawValueStorage heapIndices
         )
      {
         var list = ( (MetaDataTable<TRow>) table ).TableContents;
         if ( list.Count > 0 )
         {
            var cols = this._columns;
            for ( var rowIdx = 0; rowIdx < list.Count; ++rowIdx )
            {
               var row = list[rowIdx];
               for ( var colIdx = 0; colIdx < cols.Length; ++colIdx )
               {
                  var col = cols[colIdx];
                  if ( col.ConstantExtractor != null )
                  {
                     yield return col.ConstantExtractor( row );
                  }
                  else if ( col.HeapValueExtractor != null )
                  {
                     yield return heapIndices.GetRawValue( this.Table, rowIdx, colIdx );
                  }
                  else if ( col.RawValueProcessor != null )
                  {
                     yield return previousRawValues.GetRawValue( this.Table, rowIdx, colIdx );
                  }
                  else
                  {
                     yield return 0;
                  }
               }
            }
         }
      }

      public TableSerializationFunctionality CreateSupport( ColumnSerializationSupportCreationArgs supportArgs )
      {
         return new DefaultTableSerializationFunctionality<TRawRow, TRow>(
            this,
            this._columns,
            supportArgs
            );
      }
   }

   public class DefaultTableSerializationFunctionality<TRawRow, TRow> : TableSerializationFunctionality
      where TRawRow : class, new()
      where TRow : class, new()
   {

      private readonly DefaultColumnSerializationInfo<TRawRow, TRow>[] _columnArray;

      public DefaultTableSerializationFunctionality(
         TableSerializationInfo tableSerializationInfo,
         IEnumerable<DefaultColumnSerializationInfo<TRawRow, TRow>> columns,
         ColumnSerializationSupportCreationArgs args
         )
      {
         ArgumentValidator.ValidateNotNull( "Table serialization info", tableSerializationInfo );
         ArgumentValidator.ValidateNotNull( "Columns", columns );


         this.ColumnSerializationSupports = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy(
            columns
               .Select( c => c.SerializationSupportCreator( args ) )
               .ToArray()
            ).CQ;
         this._columnArray = columns.ToArray();
      }

      public TableSerializationInfo TableSerializationInfo { get; }

      public ArrayQuery<ColumnSerializationFunctionality> ColumnSerializationSupports { get; }

      public void ReadRows( MetaDataTable table, Int32 tableRowCount, RowReadingArguments args )
      {
         if ( tableRowCount > 0 )
         {
            var list = ( (MetaDataTable<TRow>) table ).TableContents;
            for ( var i = 0; i < tableRowCount; ++i )
            {
               var row = new TRow();
               var columnArgs = new ColumnSettingArguments<TRow, RowReadingArguments>( this.TableSerializationInfo.Table, i, row, args );
               var stream = args.Stream;
               for ( var j = 0; j < this._columnArray.Length; ++j )
               {
                  var value = this.ColumnSerializationSupports[i].ReadRawValue( stream );
                  var setter = this._columnArray[i].Setter;
                  if ( setter == null )
                  {
                     args.RawValueStorage.AddRawValue( value );
                  }
                  else
                  {
                     var s = stream.Stream;
                     var position = s.Position;
                     try
                     {
                        setter( columnArgs, value );
                     }
                     catch
                     {
                        // Ignore
                     }
                     s.Position = position;
                  }
               }

               list.Add( row );
            }
         }
      }

      public Object ReadRawRow( StreamHelper stream )
      {
         var row = new TRawRow();
         for ( var i = 0; i < this._columnArray.Length; ++i )
         {
            this._columnArray[i].RawSetter( row, this.ColumnSerializationSupports[i].ReadRawValue( stream ) );
         }
         return row;
      }

   }

   public sealed class ColumnSerializationSupport_Constant8 : ColumnSerializationFunctionality
   {
      public Int32 ColumnByteCount
      {
         get
         {
            return sizeof( Byte );
         }
      }

      public Int32 ReadRawValue( StreamHelper stream )
      {
         return stream.ReadByteFromBytes();
      }

      public void WriteValue( Byte[] bytes, Int32 idx, Int32 value )
      {
         bytes.WriteByteToBytes( ref idx, (Byte) value );
      }
   }
   public sealed class ColumnSerializationSupport_Constant16 : ColumnSerializationFunctionality
   {
      public Int32 ColumnByteCount
      {
         get
         {
            return sizeof( UInt16 );
         }
      }

      public Int32 ReadRawValue( StreamHelper stream )
      {
         return stream.ReadInt16LEFromBytes();
      }

      public void WriteValue( Byte[] bytes, Int32 idx, Int32 value )
      {
         bytes.WriteUInt16LEToBytes( ref idx, (UInt16) value );
      }
   }

   public sealed class ColumnSerializationSupport_Constant32 : ColumnSerializationFunctionality
   {
      public Int32 ColumnByteCount
      {
         get
         {
            return sizeof( Int32 );
         }
      }

      public Int32 ReadRawValue( StreamHelper stream )
      {
         return stream.ReadInt32LEFromBytes();
      }

      public void WriteValue( Byte[] bytes, Int32 idx, Int32 value )
      {
         bytes.WriteInt32LEToBytes( ref idx, value );
      }
   }


}

public static partial class E_CILPhysical
{
   private const Int32 UINT_ONE_BYTE_MAX = 0x7F;
   private const Int32 UINT_TWO_BYTES_MAX = 0x3FFF;
   private const Int32 UINT_FOUR_BYTES_MAX = 0x1FFFFFFF;

   internal static Boolean DecompressUInt32( this StreamHelper stream, out Int32 value, Boolean acceptErraneous = true )
   {
      const Int32 UINT_TWO_BYTES_DECODE_MASK = 0x3F;
      const Int32 UINT_FOUR_BYTES_DECODE_MASK = 0x1F;

      Byte first;
      if ( stream.TryReadByteFromBytes( out first ) )
      {
         if ( ( first & 0x80 ) == 0 )
         {
            // MSB bit not set, so it's just one byte 
            value = first;
         }
         else if ( ( first & 0xC0 ) == 0x80 )
         {
            Byte second;
            // MSB set, but prev bit not set, so it's two bytes
            if ( stream.TryReadByteFromBytes( out second ) )
            {
               value = ( ( first & UINT_TWO_BYTES_DECODE_MASK ) << 8 ) | (Int32) second;
            }
            else
            {
               value = -1;
            }
         }
         else if ( acceptErraneous || ( first & 0xE0 ) == 0xC0 )
         {
            // Whatever it is, it is four bytes long
            if ( stream.Stream.CanReadNextBytes( 3 ).IsTrue() )
            {
               var buf = stream.Buffer;
               stream.Stream.ReadSpecificAmount( buf, 0, 3 );
               value = ( ( first & UINT_FOUR_BYTES_DECODE_MASK ) << 24 ) | ( ( (Int32) buf[0] ) << 16 ) | ( ( (Int32) buf[1] ) << 8 ) | buf[2];
            }
            else
            {
               value = -1;
            }
         }
         else
         {
            value = -1;
         }
      }
      else
      {
         value = -1;
      }

      return value >= 0;
   }

   internal static Boolean DecompressInt32( this StreamHelper stream, out Int32 value )
   {
      const Int32 COMPLEMENT_MASK_ONE_BYTE = unchecked((Int32) 0xFFFFFFC0);
      const Int32 COMPLEMENT_MASK_TWO_BYTES = unchecked((Int32) 0xFFFFE000);
      const Int32 COMPLEMENT_MASK_FOUR_BYTES = unchecked((Int32) 0xF0000000);
      const Int32 ONE = 1;

      var retVal = stream.DecompressUInt32( out value );
      if ( retVal )
      {
         if ( value <= UINT_ONE_BYTE_MAX )
         {
            // Value is one-bit left rotated, 7-bit 2-complement number
            // If LSB is 1 -> then the value is negative
            if ( ( value & ONE ) == ONE )
            {
               value = ( value >> 1 ) | COMPLEMENT_MASK_ONE_BYTE;
            }
            else
            {
               value = value >> 1;
            }
         }
         else if ( value <= UINT_TWO_BYTES_MAX )
         {
            if ( ( value & ONE ) == ONE )
            {
               value = ( value >> 1 ) | COMPLEMENT_MASK_TWO_BYTES;
            }
            else
            {
               value = value >> 1;
            }
         }
         else
         {
            if ( ( value & ONE ) == ONE )
            {
               value = ( value >> 1 ) | COMPLEMENT_MASK_FOUR_BYTES;
            }
            else
            {
               value = value >> 1;
            }
         }
      }

      return retVal;
   }

   internal static Boolean DecompressUInt32( this Byte[] stream, ref Int32 idx, out Int32 value, Boolean acceptErraneous = true )
   {
      const Int32 UINT_TWO_BYTES_DECODE_MASK = 0x3F;
      const Int32 UINT_FOUR_BYTES_DECODE_MASK = 0x1F;
      var len = stream.Length;

      if ( idx < len )
      {
         var first = stream[idx];
         if ( ( first & 0x80 ) == 0 )
         {
            // MSB bit not set, so it's just one byte 
            value = first;
            ++idx;
         }
         else if ( ( first & 0xC0 ) == 0x80 )
         {
            // MSB set, but prev bit not set, so it's two bytes
            if ( idx < len - 1 )
            {
               value = ( ( first & UINT_TWO_BYTES_DECODE_MASK ) << 8 ) | (Int32) stream[idx + 1];
               idx += 2;
            }
            else
            {
               value = -1;
            }
         }
         else if ( acceptErraneous || ( first & 0xE0 ) == 0xC0 )
         {
            // Whatever it is, it is four bytes long
            if ( idx < len - 3 )
            {
               value = ( ( first & UINT_FOUR_BYTES_DECODE_MASK ) << 24 ) | ( ( (Int32) stream[idx + 1] ) << 16 ) | ( ( (Int32) stream[idx + 2] ) << 8 ) | stream[idx + 3];
               idx += 4;
            }
            else
            {
               value = -1;
            }
         }
         else
         {
            value = -1;
         }
      }
      else
      {
         value = -1;
      }

      return value >= 0;
   }
}