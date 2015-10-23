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
         Action<ColumnSettingArguments<TRow>, Int32> setter
         )
         : this(
        columnName,
        null,
        serializationCreator,
        rawSetter,
        setter,
        null
        )
      {

      }

      public DefaultColumnSerializationInfo(
         String columnName,
         Func<ColumnSerializationSupportCreationArgs, ColumnSerializationFunctionality> serializationCreator,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, Int32> setter,
         Action<RawValueProcessingArgs, Int32, TRow, Int32> rawValueProcessor
         )
         : this(
              columnName,
              null,
              serializationCreator,
              rawSetter,
              setter,
              rawValueProcessor
              )
      {

      }

      public DefaultColumnSerializationInfo(
         String columnName,
         HeapIndexKind heapIndexKind,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, Int32> setter
         )
         : this(
              columnName,
              heapIndexKind,
              args => args.IsWide( heapIndexKind ) ? (ColumnSerializationFunctionality) new ColumnSerializationSupport_Constant32() : new ColumnSerializationSupport_Constant16(),
              rawSetter,
              setter,
              null
              )
      {
      }

      protected DefaultColumnSerializationInfo(
         String columnName,
         HeapIndexKind? heapIndexKind,
         Func<ColumnSerializationSupportCreationArgs, ColumnSerializationFunctionality> creator,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, Int32> setter,
         Action<RawValueProcessingArgs, Int32, TRow, Int32> rawValueProcessor
         )
      {
         ArgumentValidator.ValidateNotNull( "Column name", columnName );
         ArgumentValidator.ValidateNotNull( "Raw setter", rawSetter );
         ArgumentValidator.ValidateNotNull( "Serialization support creator", creator );
         if ( setter == null )
         {
            ArgumentValidator.ValidateNotNull( "Raw value processor", rawValueProcessor );
         }
         else
         {
            ArgumentValidator.ValidateNotNull( "Setter", setter );
         }


         this.ColumnName = columnName;
         this.HeapIndexKind = heapIndexKind;
         this.RawSetter = rawSetter;
         this.Setter = setter;
         this.SerializationSupportCreator = creator;
         this.RawValueProcessor = rawValueProcessor;
      }

      public String ColumnName { get; }
      public HeapIndexKind? HeapIndexKind { get; }

      public Action<TRawRow, Int32> RawSetter { get; }
      public Action<ColumnSettingArguments<TRow>, Int32> Setter { get; }

      public Func<ColumnSerializationSupportCreationArgs, ColumnSerializationFunctionality> SerializationSupportCreator { get; }

      public Action<RawValueProcessingArgs, Int32, TRow, Int32> RawValueProcessor { get; }

      public void SetRawValue( Object row, Int32 value )
      {
         this.RawSetter( (TRawRow) row, value );
      }
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
         Action<ColumnSettingArguments<TRow>, Int32> setter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => new ColumnSerializationSupport_Constant8(),
            rawSetter,
            setter
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> Constant16<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, Int32> setter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => new ColumnSerializationSupport_Constant16(),
            rawSetter,
            setter
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> Constant32<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, Int32> setter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => new ColumnSerializationSupport_Constant32(),
            rawSetter,
            setter
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> SimpleReference<TRawRow, TRow>(
         String columnName,
         Tables targetTable,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, TableIndex> setter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => args.TableSizes[(Int32) targetTable] >= UInt16.MaxValue ? (ColumnSerializationFunctionality) new ColumnSerializationSupport_Constant32() : new ColumnSerializationSupport_Constant16(),
            rawSetter,
            ( args, value ) => setter( args, new TableIndex( targetTable, (Int32) Math.Min( 0, (UInt32) value - 1 ) ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> CodedReference<TRawRow, TRow>(
         String columnName,
         ArrayQuery<Tables?> targetTables,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, TableIndex?> setter
         )
         where TRawRow : class
         where TRow : class
      {
         var decoder = new CodedTableIndexDecoder( targetTables );

         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => GetCodedTableSize( args.TableSizes, targetTables ) < sizeof( Int32 ) ? (ColumnSerializationFunctionality) new ColumnSerializationSupport_Constant16() : new ColumnSerializationSupport_Constant32(),
            rawSetter,
            ( args, value ) => setter( args, decoder.DecodeTableIndex( value ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBArray<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, Byte[]> setter
         )
         where TRawRow : class
         where TRow : class
      {
         return BLOBCustom<TRawRow, TRow>(
            columnName,
            rawSetter,
            ( args, value, blobs ) => setter( args, blobs.GetBLOB( value ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBTypeSignature<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, TypeSignature> setter
         )
         where TRawRow : class
         where TRow : class
      {
         return BLOBCustom<TRawRow, TRow>(
            columnName,
            rawSetter,
            ( args, value, blobs ) => setter( args, blobs.ReadTypeSignature( value ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBNonTypeSignature<TRawRow, TRow, TSignature>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, TSignature> setter
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
            }
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBCASignature<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, AbstractCustomAttributeSignature> setter
         )
         where TRawRow : class
         where TRow : class
      {
         return BLOBCustom<TRawRow, TRow>(
            columnName,
            rawSetter,
            ( args, value, blobs ) => setter( args, blobs.ReadCASignature( value ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBMarshalingInfo<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, MarshalingInfo> setter
         )
         where TRawRow : class
         where TRow : class
      {
         return BLOBCustom<TRawRow, TRow>(
            columnName,
            rawSetter,
            ( args, value, blobs ) => setter( args, blobs.ReadMarshalingInfo( value ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBSecurityInformation<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, IEnumerable<AbstractSecurityInformation>> setter
         )
         where TRawRow : class
         where TRow : class
      {
         return BLOBCustom<TRawRow, TRow>(
            columnName,
            rawSetter,
            ( args, value, blobs ) => setter( args, blobs.ReadSecurityInformation( value ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBCustom<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, Int32, ReaderBLOBStreamHandler> setter
         )
         where TRawRow : class
         where TRow : class
      {
         return HeapIndex<TRawRow, TRow>(
            columnName,
            HeapIndexKind.BLOB,
            rawSetter,
            ( args, value ) => setter( args, value, args.RowArgs.MDStreamContainer.BLOBs ),
            true
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> GUID<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, Guid?> setter
         )
         where TRawRow : class
         where TRow : class
      {
         return HeapIndex<TRawRow, TRow>(
            columnName,
            HeapIndexKind.GUID,
            rawSetter,
            ( args, value ) => setter( args, args.RowArgs.MDStreamContainer.GUIDs.GetGUID( value ) ),
            false
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> SystemString<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, String> setter
         )
         where TRawRow : class
         where TRow : class
      {
         return HeapIndex<TRawRow, TRow>(
            columnName,
            HeapIndexKind.String,
            rawSetter,
            ( args, value ) => setter( args, args.RowArgs.MDStreamContainer.SystemStrings.GetString( value ) ),
            true
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> HeapIndex<TRawRow, TRow>(
         String columnName,
         HeapIndexKind heapKind,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, Int32> setter,
         Boolean checkForZero
         )
         where TRawRow : class
         where TRow : class
      {
         if ( checkForZero )
         {
            setter = ( args, value ) =>
            {
               if ( value != 0 )
               {
                  setter( args, value );
               }
            };
         }

         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            heapKind,
            rawSetter,
            setter
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> RawValueStorageColumn<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<RawValueProcessingArgs, Int32, TRow, Int32> rawValueProcessor,
         ref Int32 curColIdx
         )
         where TRawRow : class
         where TRow : class
      {
         var colIdx = curColIdx;
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => new ColumnSerializationSupport_Constant32(),
            rawSetter,
            null,
            rawValueProcessor
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

   public class ColumnSerializationSupportCreationArgs
   {
      private readonly Boolean _wideBLOBs;
      private readonly Boolean _wideGUIDs;
      private readonly Boolean _wideStrings;
      public ColumnSerializationSupportCreationArgs(
         ArrayQuery<Int32> tableSizes,
         Boolean wideBLOBs,
         Boolean wideGUIDs,
         Boolean wideStrings
         )
      {
         ArgumentValidator.ValidateNotNull( "Table sizes", tableSizes );

         this.TableSizes = tableSizes;
         this._wideBLOBs = wideBLOBs;
         this._wideGUIDs = wideGUIDs;
         this._wideStrings = wideStrings;
      }

      public ArrayQuery<Int32> TableSizes { get; }

      public Boolean IsWide( HeapIndexKind kind )
      {
         switch ( kind )
         {
            case HeapIndexKind.BLOB:
               return this._wideBLOBs;
            case HeapIndexKind.GUID:
               return this._wideGUIDs;
            case HeapIndexKind.String:
               return this._wideStrings;
            default:
               throw new NotImplementedException( "TODO" );
         }
      }

   }

   public struct ColumnSettingArguments<TRow>
      where TRow : class
   {

      public ColumnSettingArguments( Tables table, TRow row, RowReadingArguments args )
      {
         ArgumentValidator.ValidateNotNull( "Row", row );

         this.Table = table;
         this.Row = row;
         this.RowArgs = args;
      }

      public Tables Table { get; }

      public TRow Row { get; }

      public RowReadingArguments RowArgs { get; }
   }

   public partial class DefaultMetaDataSerializationSupportProvider : MetaDataSerializationSupportProvider
   {
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
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.Generation ), ( r, v ) => r.Generation = v, ( args, v ) => args.Row.Generation = (Int16) v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.GUID<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.ModuleGUID ), ( r, v ) => r.ModuleGUID = v, ( args, v ) => args.Row.ModuleGUID = v );
         yield return DefaultColumnSerializationInfoFactory.GUID<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.EditAndContinueGUID ), ( r, v ) => r.EditAndContinueGUID = v, ( args, v ) => args.Row.EditAndContinueGUID = v );
         yield return DefaultColumnSerializationInfoFactory.GUID<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.EditAndContinueBaseGUID ), ( r, v ) => r.EditAndContinueBaseGUID = v, ( args, v ) => args.Row.EditAndContinueBaseGUID = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawTypeReference, TypeReference>> GetTypeRefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawTypeReference, TypeReference>( nameof( RawTypeReference.ResolutionScope ), CodedTableIndexDecoder.ResolutionScope, ( r, v ) => r.ResolutionScope = v, ( args, v ) => args.Row.ResolutionScope = v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawTypeReference, TypeReference>( nameof( RawTypeReference.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawTypeReference, TypeReference>( nameof( RawTypeReference.Namespace ), ( r, v ) => r.Namespace = v, ( args, v ) => args.Row.Namespace = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawTypeDefinition, TypeDefinition>> GetTypeDefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.Attributes ), ( r, v ) => r.Attributes = (TypeAttributes) v, ( args, v ) => args.Row.Attributes = (TypeAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.Namespace ), ( r, v ) => r.Namespace = v, ( args, v ) => args.Row.Namespace = v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.BaseType ), CodedTableIndexDecoder.TypeDefOrRef, ( r, v ) => r.BaseType = v, ( args, v ) => args.Row.BaseType = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.FieldList ), Tables.Field, ( r, v ) => r.FieldList = v, ( args, v ) => args.Row.FieldList = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.MethodList ), Tables.MethodDef, ( r, v ) => r.MethodList = v, ( args, v ) => args.Row.MethodList = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawFieldDefinitionPointer, FieldDefinitionPointer>> GetFieldPtrColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawFieldDefinitionPointer, FieldDefinitionPointer>( nameof( RawFieldDefinitionPointer.FieldIndex ), Tables.Field, ( r, v ) => r.FieldIndex = v, ( args, v ) => args.Row.FieldIndex = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawFieldDefinition, FieldDefinition>> GetFieldDefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawFieldDefinition, FieldDefinition>( nameof( RawFieldDefinition.Attributes ), ( r, v ) => r.Attributes = (FieldAttributes) v, ( args, v ) => args.Row.Attributes = (FieldAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawFieldDefinition, FieldDefinition>( nameof( RawFieldDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.BLOBNonTypeSignature<RawFieldDefinition, FieldDefinition, FieldSignature>( nameof( RawFieldDefinition.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawMethodDefinitionPointer, MethodDefinitionPointer>> GetMethodPtrColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawMethodDefinitionPointer, MethodDefinitionPointer>( nameof( RawMethodDefinitionPointer.MethodIndex ), Tables.MethodDef, ( r, v ) => r.MethodIndex = v, ( args, v ) => args.Row.MethodIndex = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawMethodDefinition, MethodDefinition>> GetMethodDefColumns()
      {
         var colIdx = 0;
         yield return DefaultColumnSerializationInfoFactory.RawValueStorageColumn<RawMethodDefinition, MethodDefinition>( nameof( RawMethodDefinition.RVA ), ( r, v ) => r.RVA = v, ( args, rowIdx, row, rva ) => row.IL = this.DeserializeIL( args, rva ), ref colIdx );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawMethodDefinition, MethodDefinition>( nameof( RawMethodDefinition.ImplementationAttributes ), ( r, v ) => r.ImplementationAttributes = (MethodImplAttributes) v, ( args, v ) => args.Row.ImplementationAttributes = (MethodImplAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawMethodDefinition, MethodDefinition>( nameof( RawMethodDefinition.Attributes ), ( r, v ) => r.Attributes = (MethodAttributes) v, ( args, v ) => args.Row.Attributes = (MethodAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.BLOBNonTypeSignature<RawMethodDefinition, MethodDefinition, MethodDefinitionSignature>( nameof( RawMethodDefinition.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawMethodDefinition, MethodDefinition>( nameof( RawMethodDefinition.ParameterList ), Tables.Parameter, ( r, v ) => r.ParameterList = v, ( args, v ) => args.Row.ParameterList = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawParameterDefinitionPointer, ParameterDefinitionPointer>> GetParamPtrColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawParameterDefinitionPointer, ParameterDefinitionPointer>( nameof( RawParameterDefinitionPointer.ParameterIndex ), Tables.Parameter, ( r, v ) => r.ParameterIndex = v, ( args, v ) => args.Row.ParameterIndex = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawParameterDefinition, ParameterDefinition>> GetParamColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawParameterDefinition, ParameterDefinition>( nameof( RawParameterDefinition.Attributes ), ( r, v ) => r.Attributes = (ParameterAttributes) v, ( args, v ) => args.Row.Attributes = (ParameterAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawParameterDefinition, ParameterDefinition>( nameof( RawParameterDefinition.Sequence ), ( r, v ) => r.Sequence = v, ( args, v ) => args.Row.Sequence = v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawParameterDefinition, ParameterDefinition>( nameof( RawParameterDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawInterfaceImplementation, InterfaceImplementation>> GetInterfaceImplColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawInterfaceImplementation, InterfaceImplementation>( nameof( RawInterfaceImplementation.Class ), Tables.TypeDef, ( r, v ) => r.Class = v, ( args, v ) => args.Row.Class = v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawInterfaceImplementation, InterfaceImplementation>( nameof( RawInterfaceImplementation.Interface ), CodedTableIndexDecoder.TypeDefOrRef, ( r, v ) => r.Interface = v, ( args, v ) => args.Row.Interface = v.GetValueOrDefault() );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawMemberReference, MemberReference>> GetMemberRefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawMemberReference, MemberReference>( nameof( RawMemberReference.DeclaringType ), CodedTableIndexDecoder.MemberRefParent, ( r, v ) => r.DeclaringType = v, ( args, v ) => args.Row.DeclaringType = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawMemberReference, MemberReference>( nameof( RawMemberReference.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.BLOBNonTypeSignature<RawMemberReference, MemberReference, AbstractSignature>( nameof( RawMemberReference.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawConstantDefinition, ConstantDefinition>> GetConstantColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant8<RawConstantDefinition, ConstantDefinition>( nameof( RawConstantDefinition.Type ), ( r, v ) => r.Type = (SignatureElementTypes) v, ( args, v ) => args.Row.Type = (SignatureElementTypes) v );
         yield return DefaultColumnSerializationInfoFactory.Constant8<RawConstantDefinition, ConstantDefinition>( nameof( RawConstantDefinition.Padding ), ( r, v ) => r.Padding = (Byte) v, ( args, v ) => { } );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawConstantDefinition, ConstantDefinition>( nameof( RawConstantDefinition.Parent ), CodedTableIndexDecoder.HasConstant, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.BLOBCustom<RawConstantDefinition, ConstantDefinition>( nameof( RawConstantDefinition.Value ), ( r, v ) => r.Value = v, ( args, v, blobs ) => args.Row.Value = blobs.ReadConstantValue( v, args.Row.Type ) );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawCustomAttributeDefinition, CustomAttributeDefinition>> GetCustomAttributeColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawCustomAttributeDefinition, CustomAttributeDefinition>( nameof( RawCustomAttributeDefinition.Parent ), CodedTableIndexDecoder.HasCustomAttribute, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawCustomAttributeDefinition, CustomAttributeDefinition>( nameof( RawCustomAttributeDefinition.Type ), CodedTableIndexDecoder.CustomAttributeType, ( r, v ) => r.Type = v, ( args, v ) => args.Row.Type = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.BLOBCASignature<RawCustomAttributeDefinition, CustomAttributeDefinition>( nameof( RawCustomAttributeDefinition.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawFieldMarshal, FieldMarshal>> GetFieldMarshalColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawFieldMarshal, FieldMarshal>( nameof( RawFieldMarshal.Parent ), CodedTableIndexDecoder.HasFieldMarshal, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.BLOBMarshalingInfo<RawFieldMarshal, FieldMarshal>( nameof( RawFieldMarshal.NativeType ), ( r, v ) => r.NativeType = v, ( args, v ) => args.Row.NativeType = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawSecurityDefinition, SecurityDefinition>> GetDeclSecurityColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawSecurityDefinition, SecurityDefinition>( nameof( RawSecurityDefinition.Action ), ( r, v ) => r.Action = (SecurityAction) v, ( args, v ) => args.Row.Action = (SecurityAction) v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawSecurityDefinition, SecurityDefinition>( nameof( RawSecurityDefinition.Parent ), CodedTableIndexDecoder.HasSecurity, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.BLOBSecurityInformation<RawSecurityDefinition, SecurityDefinition>( nameof( RawSecurityDefinition.PermissionSets ), ( r, v ) => r.PermissionSets = v, ( args, v ) => args.Row.PermissionSets.AddRange( v ) );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawClassLayout, ClassLayout>> GetClassLayoutColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawClassLayout, ClassLayout>( nameof( RawClassLayout.PackingSize ), ( r, v ) => r.PackingSize = v, ( args, v ) => args.Row.PackingSize = v );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawClassLayout, ClassLayout>( nameof( RawClassLayout.ClassSize ), ( r, v ) => r.ClassSize = v, ( args, v ) => args.Row.ClassSize = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawClassLayout, ClassLayout>( nameof( RawClassLayout.Parent ), Tables.TypeDef, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawFieldLayout, FieldLayout>> GetFieldLayoutColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawFieldLayout, FieldLayout>( nameof( RawFieldLayout.Offset ), ( r, v ) => r.Offset = v, ( args, v ) => args.Row.Offset = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawFieldLayout, FieldLayout>( nameof( RawFieldLayout.Field ), Tables.Field, ( r, v ) => r.Field = v, ( args, v ) => args.Row.Field = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawStandaloneSignature, StandaloneSignature>> GetStandaloneSigColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.BLOBCustom<RawStandaloneSignature, StandaloneSignature>( nameof( RawStandaloneSignature.Signature ), ( r, v ) => r.Signature = v, ( args, v, blobs ) =>
         {
            Boolean wasFieldSig;
            args.Row.Signature = blobs.ReadNonTypeSignature( v, false, true, out wasFieldSig );
            args.Row.StoreSignatureAsFieldSignature = wasFieldSig;
         } );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawEventMap, EventMap>> GetEventMapColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawEventMap, EventMap>( nameof( RawEventMap.Parent ), Tables.TypeDef, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawEventMap, EventMap>( nameof( RawEventMap.EventList ), Tables.Event, ( r, v ) => r.EventList = v, ( args, v ) => args.Row.EventList = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawEventDefinitionPointer, EventDefinitionPointer>> GetEventPtrColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawEventDefinitionPointer, EventDefinitionPointer>( nameof( RawEventDefinitionPointer.EventIndex ), Tables.Event, ( r, v ) => r.EventIndex = v, ( args, v ) => args.Row.EventIndex = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawEventDefinition, EventDefinition>> GetEventDefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawEventDefinition, EventDefinition>( nameof( RawEventDefinition.Attributes ), ( r, v ) => r.Attributes = (EventAttributes) v, ( args, v ) => args.Row.Attributes = (EventAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawEventDefinition, EventDefinition>( nameof( RawEventDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawEventDefinition, EventDefinition>( nameof( RawEventDefinition.EventType ), CodedTableIndexDecoder.TypeDefOrRef, ( r, v ) => r.EventType = v, ( args, v ) => args.Row.EventType = v.GetValueOrDefault() );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawPropertyMap, PropertyMap>> GetPropertyMapColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawPropertyMap, PropertyMap>( nameof( RawPropertyMap.Parent ), Tables.TypeDef, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawPropertyMap, PropertyMap>( nameof( RawPropertyMap.PropertyList ), Tables.Property, ( r, v ) => r.PropertyList = v, ( args, v ) => args.Row.PropertyList = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawPropertyDefinitionPointer, PropertyDefinitionPointer>> GetPropertyPtrColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawPropertyDefinitionPointer, PropertyDefinitionPointer>( nameof( RawPropertyDefinitionPointer.PropertyIndex ), Tables.Property, ( r, v ) => r.PropertyIndex = v, ( args, v ) => args.Row.PropertyIndex = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawPropertyDefinition, PropertyDefinition>> GetPropertyDefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawPropertyDefinition, PropertyDefinition>( nameof( RawPropertyDefinition.Attributes ), ( r, v ) => r.Attributes = (PropertyAttributes) v, ( args, v ) => args.Row.Attributes = (PropertyAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawPropertyDefinition, PropertyDefinition>( nameof( RawPropertyDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.BLOBNonTypeSignature<RawPropertyDefinition, PropertyDefinition, PropertySignature>( nameof( RawPropertyDefinition.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawMethodSemantics, MethodSemantics>> GetMethodSemanticsColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawMethodSemantics, MethodSemantics>( nameof( RawMethodSemantics.Attributes ), ( r, v ) => r.Attributes = (MethodSemanticsAttributes) v, ( args, v ) => args.Row.Attributes = (MethodSemanticsAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawMethodSemantics, MethodSemantics>( nameof( RawMethodSemantics.Method ), Tables.MethodDef, ( r, v ) => r.Method = v, ( args, v ) => args.Row.Method = v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawMethodSemantics, MethodSemantics>( nameof( RawMethodSemantics.Associaton ), CodedTableIndexDecoder.HasSemantics, ( r, v ) => r.Associaton = v, ( args, v ) => args.Row.Associaton = v.GetValueOrDefault() );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawMethodImplementation, MethodImplementation>> GetMethodImplColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawMethodImplementation, MethodImplementation>( nameof( RawMethodImplementation.Class ), Tables.TypeDef, ( r, v ) => r.Class = v, ( args, v ) => args.Row.Class = v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawMethodImplementation, MethodImplementation>( nameof( RawMethodImplementation.MethodBody ), CodedTableIndexDecoder.MethodDefOrRef, ( r, v ) => r.MethodBody = v, ( args, v ) => args.Row.MethodBody = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawMethodImplementation, MethodImplementation>( nameof( RawMethodImplementation.MethodDeclaration ), CodedTableIndexDecoder.MethodDefOrRef, ( r, v ) => r.MethodDeclaration = v, ( args, v ) => args.Row.MethodDeclaration = v.GetValueOrDefault() );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawModuleReference, ModuleReference>> GetModuleRefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawModuleReference, ModuleReference>( nameof( RawModuleReference.ModuleName ), ( r, v ) => r.ModuleName = v, ( args, v ) => args.Row.ModuleName = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawTypeSpecification, TypeSpecification>> GetTypeSpecColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.BLOBTypeSignature<RawTypeSpecification, TypeSpecification>( nameof( RawTypeSpecification.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawMethodImplementationMap, MethodImplementationMap>> GetImplMapColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawMethodImplementationMap, MethodImplementationMap>( nameof( RawMethodImplementationMap.Attributes ), ( r, v ) => r.Attributes = (PInvokeAttributes) v, ( args, v ) => args.Row.Attributes = (PInvokeAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawMethodImplementationMap, MethodImplementationMap>( nameof( RawMethodImplementationMap.MemberForwarded ), CodedTableIndexDecoder.MemberForwarded, ( r, v ) => r.MemberForwarded = v, ( args, v ) => args.Row.MemberForwarded = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawMethodImplementationMap, MethodImplementationMap>( nameof( RawMethodImplementationMap.ImportName ), ( r, v ) => r.ImportName = v, ( args, v ) => args.Row.ImportName = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawMethodImplementationMap, MethodImplementationMap>( nameof( RawMethodImplementationMap.ImportScope ), Tables.ModuleRef, ( r, v ) => r.ImportScope = v, ( args, v ) => args.Row.ImportScope = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawFieldRVA, FieldRVA>> GetFieldRVAColumns()
      {
         var colIdx = 0;
         yield return DefaultColumnSerializationInfoFactory.RawValueStorageColumn<RawFieldRVA, FieldRVA>( nameof( RawFieldRVA.RVA ), ( r, v ) => r.RVA = v, ( args, rowIdx, row, rva ) => row.Data = this.DeserializeConstantValue( args, row, rva ), ref colIdx );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawFieldRVA, FieldRVA>( nameof( RawFieldRVA.Field ), Tables.Field, ( r, v ) => r.Field = v, ( args, v ) => args.Row.Field = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawEditAndContinueLog, EditAndContinueLog>> GetENCLogColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawEditAndContinueLog, EditAndContinueLog>( nameof( RawEditAndContinueLog.Token ), ( r, v ) => r.Token = v, ( args, v ) => args.Row.Token = v );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawEditAndContinueLog, EditAndContinueLog>( nameof( RawEditAndContinueLog.FuncCode ), ( r, v ) => r.FuncCode = v, ( args, v ) => args.Row.FuncCode = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawEditAndContinueMap, EditAndContinueMap>> GetENCMapColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawEditAndContinueMap, EditAndContinueMap>( nameof( RawEditAndContinueMap.Token ), ( r, v ) => r.Token = v, ( args, v ) => args.Row.Token = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawAssemblyDefinition, AssemblyDefinition>> GetAssemblyDefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.HashAlgorithm ), ( r, v ) => r.HashAlgorithm = (AssemblyHashAlgorithm) v, ( args, v ) => args.Row.HashAlgorithm = (AssemblyHashAlgorithm) v );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.MajorVersion ), ( r, v ) => r.MajorVersion = v, ( args, v ) => args.Row.AssemblyInformation.VersionMajor = v );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.MinorVersion ), ( r, v ) => r.MinorVersion = v, ( args, v ) => args.Row.AssemblyInformation.VersionMinor = v );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.BuildNumber ), ( r, v ) => r.BuildNumber = v, ( args, v ) => args.Row.AssemblyInformation.VersionBuild = v );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.RevisionNumber ), ( r, v ) => r.RevisionNumber = v, ( args, v ) => args.Row.AssemblyInformation.VersionRevision = v );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.Attributes ), ( r, v ) => r.Attributes = (AssemblyFlags) v, ( args, v ) => args.Row.Attributes = (AssemblyFlags) v );
         yield return DefaultColumnSerializationInfoFactory.BLOBArray<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.PublicKey ), ( r, v ) => r.PublicKey = v, ( args, v ) => args.Row.AssemblyInformation.PublicKeyOrToken = v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.AssemblyInformation.Name = v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.Culture ), ( r, v ) => r.Culture = v, ( args, v ) => args.Row.AssemblyInformation.Culture = v );
      }
#pragma warning disable 618
      protected IEnumerable<DefaultColumnSerializationInfo<RawAssemblyDefinitionProcessor, AssemblyDefinitionProcessor>> GetAssemblyDefProcessorColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyDefinitionProcessor, AssemblyDefinitionProcessor>( nameof( RawAssemblyDefinitionProcessor.Processor ), ( r, v ) => r.Processor = v, ( args, v ) => args.Row.Processor = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawAssemblyDefinitionOS, AssemblyDefinitionOS>> GetAssemblyDefOSColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyDefinitionOS, AssemblyDefinitionOS>( nameof( RawAssemblyDefinitionOS.OSPlatformID ), ( r, v ) => r.OSPlatformID = v, ( args, v ) => args.Row.OSPlatformID = v );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyDefinitionOS, AssemblyDefinitionOS>( nameof( RawAssemblyDefinitionOS.OSMajorVersion ), ( r, v ) => r.OSMajorVersion = v, ( args, v ) => args.Row.OSMajorVersion = v );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyDefinitionOS, AssemblyDefinitionOS>( nameof( RawAssemblyDefinitionOS.OSMinorVersion ), ( r, v ) => r.OSMinorVersion = v, ( args, v ) => args.Row.OSMinorVersion = v );
      }
#pragma warning restore 618

      protected IEnumerable<DefaultColumnSerializationInfo<RawAssemblyReference, AssemblyReference>> GetAssemblyRefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.MajorVersion ), ( r, v ) => r.MajorVersion = v, ( args, v ) => args.Row.AssemblyInformation.VersionMajor = v );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.MinorVersion ), ( r, v ) => r.MinorVersion = v, ( args, v ) => args.Row.AssemblyInformation.VersionMinor = v );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.BuildNumber ), ( r, v ) => r.BuildNumber = v, ( args, v ) => args.Row.AssemblyInformation.VersionBuild = v );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.RevisionNumber ), ( r, v ) => r.RevisionNumber = v, ( args, v ) => args.Row.AssemblyInformation.VersionRevision = v );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.Attributes ), ( r, v ) => r.Attributes = (AssemblyFlags) v, ( args, v ) => args.Row.Attributes = (AssemblyFlags) v );
         yield return DefaultColumnSerializationInfoFactory.BLOBArray<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.PublicKeyOrToken ), ( r, v ) => r.PublicKeyOrToken = v, ( args, v ) => args.Row.AssemblyInformation.PublicKeyOrToken = v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.AssemblyInformation.Name = v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.Culture ), ( r, v ) => r.Culture = v, ( args, v ) => args.Row.AssemblyInformation.Culture = v );
         yield return DefaultColumnSerializationInfoFactory.BLOBArray<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.HashValue ), ( r, v ) => r.HashValue = v, ( args, v ) => args.Row.HashValue = v );
      }

#pragma warning disable 618
      protected IEnumerable<DefaultColumnSerializationInfo<RawAssemblyReferenceProcessor, AssemblyReferenceProcessor>> GetAssemblyRefProcessorColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyReferenceProcessor, AssemblyReferenceProcessor>( nameof( RawAssemblyReferenceProcessor.Processor ), ( r, v ) => r.Processor = v, ( args, v ) => args.Row.Processor = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawAssemblyReferenceProcessor, AssemblyReferenceProcessor>( nameof( RawAssemblyReferenceProcessor.AssemblyRef ), Tables.AssemblyRef, ( r, v ) => r.AssemblyRef = v, ( args, v ) => args.Row.AssemblyRef = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawAssemblyReferenceOS, AssemblyReferenceOS>> GetAssemblyRefOSColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyReferenceOS, AssemblyReferenceOS>( nameof( RawAssemblyReferenceOS.OSPlatformID ), ( r, v ) => r.OSPlatformID = v, ( args, v ) => args.Row.OSPlatformID = v );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyReferenceOS, AssemblyReferenceOS>( nameof( RawAssemblyReferenceOS.OSMajorVersion ), ( r, v ) => r.OSMajorVersion = v, ( args, v ) => args.Row.OSMajorVersion = v );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyReferenceOS, AssemblyReferenceOS>( nameof( RawAssemblyReferenceOS.OSMinorVersion ), ( r, v ) => r.OSMinorVersion = v, ( args, v ) => args.Row.OSMinorVersion = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawAssemblyReferenceOS, AssemblyReferenceOS>( nameof( RawAssemblyReferenceOS.AssemblyRef ), Tables.AssemblyRef, ( r, v ) => r.AssemblyRef = v, ( args, v ) => args.Row.AssemblyRef = v );
      }
#pragma warning restore 618

      protected IEnumerable<DefaultColumnSerializationInfo<RawFileReference, FileReference>> GetFileColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawFileReference, FileReference>( nameof( RawFileReference.Attributes ), ( r, v ) => r.Attributes = (FileAttributes) v, ( args, v ) => args.Row.Attributes = (FileAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawFileReference, FileReference>( nameof( RawFileReference.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.BLOBArray<RawFileReference, FileReference>( nameof( RawFileReference.HashValue ), ( r, v ) => r.HashValue = v, ( args, v ) => args.Row.HashValue = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawExportedType, ExportedType>> GetExportedTypeColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawExportedType, ExportedType>( nameof( RawExportedType.Attributes ), ( r, v ) => r.Attributes = (TypeAttributes) v, ( args, v ) => args.Row.Attributes = (TypeAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawExportedType, ExportedType>( nameof( RawExportedType.TypeDefinitionIndex ), ( r, v ) => r.TypeDefinitionIndex = v, ( args, v ) => args.Row.TypeDefinitionIndex = v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawExportedType, ExportedType>( nameof( RawExportedType.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawExportedType, ExportedType>( nameof( RawExportedType.Namespace ), ( r, v ) => r.Namespace = v, ( args, v ) => args.Row.Namespace = v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawExportedType, ExportedType>( nameof( RawExportedType.Implementation ), CodedTableIndexDecoder.Implementation, ( r, v ) => r.Implementation = v, ( args, v ) => args.Row.Implementation = v.GetValueOrDefault() );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawManifestResource, ManifestResource>> GetManifestResourceColumns()
      {
         var colIdx = 0;
         yield return DefaultColumnSerializationInfoFactory.RawValueStorageColumn<RawManifestResource, ManifestResource>( nameof( RawManifestResource.Offset ), ( r, v ) => r.Offset = v, ( args, rowIdx, row, offset ) =>
         {
            row.Offset = offset;
            if ( !row.Implementation.HasValue )
            {
               row.DataInCurrentFile = this.DeserializeEmbeddedManifest( args, offset );
            }
         }, ref colIdx );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawManifestResource, ManifestResource>( nameof( RawManifestResource.Attributes ), ( r, v ) => r.Attributes = (ManifestResourceAttributes) v, ( args, v ) => args.Row.Attributes = (ManifestResourceAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawManifestResource, ManifestResource>( nameof( RawManifestResource.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawManifestResource, ManifestResource>( nameof( RawManifestResource.Implementation ), CodedTableIndexDecoder.Implementation, ( r, v ) => r.Implementation = v, ( args, v ) => args.Row.Implementation = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawNestedClassDefinition, NestedClassDefinition>> GetNestedClassColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawNestedClassDefinition, NestedClassDefinition>( nameof( RawNestedClassDefinition.NestedClass ), Tables.TypeDef, ( r, v ) => r.NestedClass = v, ( args, v ) => args.Row.NestedClass = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawNestedClassDefinition, NestedClassDefinition>( nameof( RawNestedClassDefinition.EnclosingClass ), Tables.TypeDef, ( r, v ) => r.EnclosingClass = v, ( args, v ) => args.Row.EnclosingClass = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawGenericParameterDefinition, GenericParameterDefinition>> GetGenericParamColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawGenericParameterDefinition, GenericParameterDefinition>( nameof( RawGenericParameterDefinition.GenericParameterIndex ), ( r, v ) => r.GenericParameterIndex = v, ( args, v ) => args.Row.GenericParameterIndex = v );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawGenericParameterDefinition, GenericParameterDefinition>( nameof( RawGenericParameterDefinition.Attributes ), ( r, v ) => r.Attributes = (GenericParameterAttributes) v, ( args, v ) => args.Row.Attributes = (GenericParameterAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawGenericParameterDefinition, GenericParameterDefinition>( nameof( RawGenericParameterDefinition.Owner ), CodedTableIndexDecoder.TypeOrMethodDef, ( r, v ) => r.Owner = v, ( args, v ) => args.Row.Owner = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawGenericParameterDefinition, GenericParameterDefinition>( nameof( RawGenericParameterDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawMethodSpecification, MethodSpecification>> GetMethodSpecColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawMethodSpecification, MethodSpecification>( nameof( RawMethodSpecification.Method ), CodedTableIndexDecoder.MethodDefOrRef, ( r, v ) => r.Method = v, ( args, v ) => args.Row.Method = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.BLOBNonTypeSignature<RawMethodSpecification, MethodSpecification, GenericMethodSignature>( nameof( RawMethodSpecification.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v );
      }

      protected IEnumerable<DefaultColumnSerializationInfo<RawGenericParameterConstraintDefinition, GenericParameterConstraintDefinition>> GetGenericParamConstraintColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawGenericParameterConstraintDefinition, GenericParameterConstraintDefinition>( nameof( RawGenericParameterConstraintDefinition.Owner ), Tables.GenericParameter, ( r, v ) => r.Owner = v, ( args, v ) => args.Row.Owner = v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawGenericParameterConstraintDefinition, GenericParameterConstraintDefinition>( nameof( RawGenericParameterConstraintDefinition.Constraint ), CodedTableIndexDecoder.TypeDefOrRef, ( r, v ) => r.Constraint = v, ( args, v ) => args.Row.Constraint = v.GetValueOrDefault() );
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

      public ArrayQuery<ColumnSerializationInfo> ColumnSerializationInfos { get; }

      public Int32 RawValueStorageColumnCount
      {
         get
         {
            return this._columns.Count( c => c.RawValueProcessor != null );
         }
      }

      public void ProcessRowForRawValues( RawValueProcessingArgs args, Int32 rowIndex, Object row, IEnumerable<Int32> rawValues )
      {
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
                  colInfo.RawValueProcessor( args, rowIndex, (TRow) row, value );
               }
               catch
               {
                  // Ignore...
               }
            }
         }
      }

      public Tables Table { get; }

      public TableSerializationFunctionality CreateSupport(
         ArrayQuery<Int32> tableSizes,
         Boolean wideBLOBs,
         Boolean wideGUIDs,
         Boolean wideStrings
         )
      {
         return new DefaultTableSerializationSupport<TRawRow, TRow>(
            this,
            this._columns,
            new ColumnSerializationSupportCreationArgs( tableSizes, wideBLOBs, wideGUIDs, wideStrings )
            );
      }
   }

   public class DefaultTableSerializationSupport<TRawRow, TRow> : TableSerializationFunctionality
      where TRawRow : class, new()
      where TRow : class, new()
   {

      private readonly DefaultColumnSerializationInfo<TRawRow, TRow>[] _columnArray;
      public DefaultTableSerializationSupport(
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

      public Object ReadRow( RowReadingArguments args )
      {
         var row = new TRow();
         var columnArgs = new ColumnSettingArguments<TRow>( this.TableSerializationInfo.Table, row, args );
         var stream = args.Stream;
         for ( var i = 0; i < this._columnArray.Length; ++i )
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
         return row;
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

      public void WriteValue( StreamHelper stream, Int32 value )
      {
         stream.WriteByteToBytes( (Byte) value );
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

      public void WriteValue( StreamHelper stream, Int32 value )
      {
         stream.WriteUInt16LEToBytes( (UInt16) value );
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

      public void WriteValue( StreamHelper stream, Int32 value )
      {
         stream.WriteInt32LEToBytes( value );
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