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
         Func<ColumnSerializationSupportCreationArgs, ColumnSerializationSupport> serializationCreator,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, Int32> setter
         )
         : this( columnName, null, serializationCreator, rawSetter, setter )
      {

      }

      public DefaultColumnSerializationInfo(
         String columnName,
         HeapIndexKind heapIndexKind,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, Int32> setter
         )
         : this( columnName, heapIndexKind, args => args.IsWide( heapIndexKind ) ? (ColumnSerializationSupport) new ColumnSerializationSupport_Constant32() : new ColumnSerializationSupport_Constant16(), rawSetter, setter )
      {
      }

      protected DefaultColumnSerializationInfo(
         String columnName,
         HeapIndexKind? heapIndexKind,
         Func<ColumnSerializationSupportCreationArgs, ColumnSerializationSupport> creator,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, Int32> setter
         )
      {
         ArgumentValidator.ValidateNotNull( "Column name", columnName );
         ArgumentValidator.ValidateNotNull( "Raw setter", rawSetter );
         ArgumentValidator.ValidateNotNull( "Setter", setter );
         ArgumentValidator.ValidateNotNull( "Serialization support creator", creator );

         this.ColumnName = columnName;
         this.HeapIndexKind = heapIndexKind;
         this.RawSetter = rawSetter;
         this.Setter = setter;
         this.SerializationSupportCreator = creator;
      }

      public String ColumnName { get; }
      public HeapIndexKind? HeapIndexKind { get; }
      public Action<TRawRow, Int32> RawSetter { get; }
      public Action<ColumnSettingArguments<TRow>, Int32> Setter { get; }

      public Func<ColumnSerializationSupportCreationArgs, ColumnSerializationSupport> SerializationSupportCreator { get; }

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
            args => args.TableSizes[(Int32) targetTable] >= UInt16.MaxValue ? (ColumnSerializationSupport) new ColumnSerializationSupport_Constant32() : new ColumnSerializationSupport_Constant16(),
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
            args => GetCodedTableSize( args.TableSizes, targetTables ) < sizeof( Int32 ) ? (ColumnSerializationSupport) new ColumnSerializationSupport_Constant16() : new ColumnSerializationSupport_Constant32(),
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

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBSignature<TRawRow, TRow, TSignature>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, TSignature> setter
         )
         where TRawRow : class
         where TRow : class
         where TSignature : AbstractSignature
      {
         return BLOBSignatureFull<TRawRow, TRow, TSignature>(
            columnName,
            rawSetter,
            ( args, sig, wasFieldSig ) => setter( args, sig )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBSignatureFull<TRawRow, TRow, TSignature>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, TSignature, Boolean> setter
         )
         where TRawRow : class
         where TRow : class
         where TSignature : AbstractSignature
      {
         return BLOBCustom<TRawRow, TRow>(
            columnName,
            rawSetter,
            ( args, value, blobs ) =>
            {
               Boolean wasFieldSig;
               var sig = blobs.ReadSignature( value, out wasFieldSig );
               setter( args, sig as TSignature, wasFieldSig );
            }
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> BLOBCASignature<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, CustomAttributeSignature> setter
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
            ( args, value ) => setter( args, value, args.RowArgs.BLOBs )
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
            ( args, value ) => setter( args, args.RowArgs.GUIDs.GetGUID( value ) )
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
            ( args, value ) => setter( args, args.RowArgs.SystemStrings.GetString( value ) )
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> HeapIndex<TRawRow, TRow>(
         String columnName,
         HeapIndexKind heapKind,
         Action<TRawRow, Int32> rawSetter,
         Action<ColumnSettingArguments<TRow>, Int32> setter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            heapKind,
            rawSetter,
            setter
            );
      }

      public static DefaultColumnSerializationInfo<TRawRow, TRow> RawValueStorageColumn<TRawRow, TRow>(
         String columnName,
         Action<TRawRow, Int32> rawSetter
         )
         where TRawRow : class
         where TRow : class
      {
         return new DefaultColumnSerializationInfo<TRawRow, TRow>(
            columnName,
            args => new ColumnSerializationSupport_Constant32(),
            rawSetter,
            ( args, value ) => { }
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

      public ColumnSettingArguments( TRow row, RowReadingArguments args )
      {
         ArgumentValidator.ValidateNotNull( "Row", row );

         this.Row = row;
         this.RowArgs = args;
      }

      public TRow Row { get; }

      public RowReadingArguments RowArgs { get; }
   }

   public class DefaultMetaDataSerializationSupportProvider : MetaDataSerializationSupportProvider
   {
      private readonly IEnumerable<ColumnSerializationInfo>[] _columnInfos;

      public DefaultMetaDataSerializationSupportProvider(
         IEnumerable<IEnumerable<ColumnSerializationInfo>> columnInfos = null
         )
      {
         this._columnInfos = ( columnInfos ?? CreateDefaultColumnInfos() ).ToArray();
      }

      protected static IEnumerable<IEnumerable<ColumnSerializationInfo>> CreateDefaultColumnInfos()
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

      public virtual TableSerializationInfo CreateTableSerializationInfo(
         Tables table
         )
      {
         switch ( table )
         {
            case Tables.Module:
               return this.CreateTableInfo( table, (IEnumerable<DefaultColumnSerializationInfo<RawModuleDefinition, ModuleDefinition>>) this._columnInfos[(Int32) table] );
            case Tables.TypeRef:
               return this.CreateTableInfo( table, GetTypeRefColumns() );
            case Tables.TypeDef:
               return this.CreateTableInfo( table, GetTypeDefColumns() );
            case Tables.FieldPtr:
               return this.CreateTableInfo( table, GetFieldPtrColumns() );
            case Tables.Field:
               return this.CreateTableInfo( table, GetFieldDefColumns() );
            case Tables.MethodPtr:
               return this.CreateTableInfo( table, GetMethodPtrColumns() );
            case Tables.MethodDef:
               return this.CreateTableInfo( table, GetMethodDefColumns() );
            case Tables.ParameterPtr:
               return this.CreateTableInfo( table, GetParamPtrColumns() );
            case Tables.Parameter:
               return this.CreateTableInfo( table, GetParamColumns() );
            case Tables.InterfaceImpl:
               return this.CreateTableInfo( table, GetInterfaceImplColumns() );
            case Tables.MemberRef:
               return this.CreateTableInfo( table, GetMemberRefColumns() );
            case Tables.Constant:
               return this.CreateTableInfo( table, GetConstantColumns() );
            case Tables.CustomAttribute:
               return this.CreateTableInfo( table, GetCustomAttributeColumns() );
            case Tables.FieldMarshal:
               return this.CreateTableInfo( table, GetFieldMarshalColumns() );
            case Tables.DeclSecurity:
               return this.CreateTableInfo( table, GetDeclSecurityColumns() );
            case Tables.ClassLayout:
               return this.CreateTableInfo( table, GetClassLayoutColumns() );
            case Tables.FieldLayout:
               return this.CreateTableInfo( table, GetFieldLayoutColumns() );
            case Tables.StandaloneSignature:
               return this.CreateTableInfo( table, GetStandaloneSigColumns() );
            case Tables.EventMap:
               return this.CreateTableInfo( table, GetEventMapColumns() );
            case Tables.EventPtr:
               return this.CreateTableInfo( table, GetEventPtrColumns() );
            case Tables.Event:
               return this.CreateTableInfo( table, GetEventDefColumns() );
            case Tables.PropertyMap:
               return this.CreateTableInfo( table, GetPropertyMapColumns() );
            case Tables.PropertyPtr:
               return this.CreateTableInfo( table, GetPropertyPtrColumns() );
            case Tables.Property:
               return this.CreateTableInfo( table, GetPropertyDefColumns() );
            case Tables.MethodSemantics:
               return this.CreateTableInfo( table, GetMethodSemanticsColumns() );
            case Tables.MethodImpl:
               return this.CreateTableInfo( table, GetMethodImplColumns() );
            case Tables.ModuleRef:
               return this.CreateTableInfo( table, GetModuleRefColumns() );
            case Tables.TypeSpec:
               return this.CreateTableInfo( table, GetTypeSpecColumns() );
            case Tables.ImplMap:
               return this.CreateTableInfo( table, GetImplMapColumns() );
            case Tables.FieldRVA:
               return this.CreateTableInfo( table, GetFieldRVAColumns() );
            case Tables.EncLog:
               return this.CreateTableInfo( table, GetENCLogColumns() );
            case Tables.EncMap:
               return this.CreateTableInfo( table, GetENCMapColumns() );
            case Tables.Assembly:
               return this.CreateTableInfo( table, GetAssemblyDefColumns() );
            case Tables.AssemblyProcessor:
               return this.CreateTableInfo( table, GetAssemblyDefProcessorColumns() );
            case Tables.AssemblyOS:
               return this.CreateTableInfo( table, GetAssemblyDefOSColumns() );
            case Tables.AssemblyRef:
               return this.CreateTableInfo( table, GetAssemblyRefColumns() );
            case Tables.AssemblyRefProcessor:
               return this.CreateTableInfo( table, GetAssemblyRefProcessorColumns() );
            case Tables.AssemblyRefOS:
               return this.CreateTableInfo( table, GetAssemblyRefOSColumns() );
            case Tables.File:
               return this.CreateTableInfo( table, GetFileColumns() );
            case Tables.ExportedType:
               return this.CreateTableInfo( table, GetExportedTypeColumns() );
            case Tables.ManifestResource:
               return this.CreateTableInfo( table, GetManifestResourceColumns() );
            case Tables.NestedClass:
               return this.CreateTableInfo( table, GetNestedClassColumns() );
            case Tables.GenericParameter:
               return this.CreateTableInfo( table, GetGenericParamColumns() );
            case Tables.MethodSpec:
               return this.CreateTableInfo( table, GetMethodSpecColumns() );
            case Tables.GenericParameterConstraint:
               return this.CreateTableInfo( table, GetGenericParamConstraintColumns() );
            default:
               return null;
         }
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawModuleDefinition, ModuleDefinition>> GetModuleDefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.Generation ), ( r, v ) => r.Generation = v, ( args, v ) => args.Row.Generation = (Int16) v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.GUID<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.ModuleGUID ), ( r, v ) => r.ModuleGUID = v, ( args, v ) => args.Row.ModuleGUID = v );
         yield return DefaultColumnSerializationInfoFactory.GUID<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.EditAndContinueGUID ), ( r, v ) => r.EditAndContinueGUID = v, ( args, v ) => args.Row.EditAndContinueGUID = v );
         yield return DefaultColumnSerializationInfoFactory.GUID<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.EditAndContinueBaseGUID ), ( r, v ) => r.EditAndContinueBaseGUID = v, ( args, v ) => args.Row.EditAndContinueBaseGUID = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawTypeReference, TypeReference>> GetTypeRefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawTypeReference, TypeReference>( nameof( RawTypeReference.ResolutionScope ), CodedTableIndexDecoder.ResolutionScope, ( r, v ) => r.ResolutionScope = v, ( args, v ) => args.Row.ResolutionScope = v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawTypeReference, TypeReference>( nameof( RawTypeReference.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawTypeReference, TypeReference>( nameof( RawTypeReference.Namespace ), ( r, v ) => r.Namespace = v, ( args, v ) => args.Row.Namespace = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawTypeDefinition, TypeDefinition>> GetTypeDefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.Attributes ), ( r, v ) => r.Attributes = (TypeAttributes) v, ( args, v ) => args.Row.Attributes = (TypeAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.Namespace ), ( r, v ) => r.Namespace = v, ( args, v ) => args.Row.Namespace = v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.BaseType ), CodedTableIndexDecoder.TypeDefOrRef, ( r, v ) => r.BaseType = v, ( args, v ) => args.Row.BaseType = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.FieldList ), Tables.Field, ( r, v ) => r.FieldList = v, ( args, v ) => args.Row.FieldList = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.MethodList ), Tables.MethodDef, ( r, v ) => r.MethodList = v, ( args, v ) => args.Row.MethodList = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawFieldDefinitionPointer, FieldDefinitionPointer>> GetFieldPtrColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawFieldDefinitionPointer, FieldDefinitionPointer>( nameof( RawFieldDefinitionPointer.FieldIndex ), Tables.Field, ( r, v ) => r.FieldIndex = v, ( args, v ) => args.Row.FieldIndex = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawFieldDefinition, FieldDefinition>> GetFieldDefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawFieldDefinition, FieldDefinition>( nameof( RawFieldDefinition.Attributes ), ( r, v ) => r.Attributes = (FieldAttributes) v, ( args, v ) => args.Row.Attributes = (FieldAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawFieldDefinition, FieldDefinition>( nameof( RawFieldDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.BLOBSignature<RawFieldDefinition, FieldDefinition, FieldSignature>( nameof( RawFieldDefinition.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawMethodDefinitionPointer, MethodDefinitionPointer>> GetMethodPtrColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawMethodDefinitionPointer, MethodDefinitionPointer>( nameof( RawMethodDefinitionPointer.MethodIndex ), Tables.MethodDef, ( r, v ) => r.MethodIndex = v, ( args, v ) => args.Row.MethodIndex = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawMethodDefinition, MethodDefinition>> GetMethodDefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawMethodDefinition, MethodDefinition>( nameof( RawMethodDefinition.RVA ), ( r, v ) => r.RVA = v, ( args, v ) => args.RowArgs.MethodRVAs.Add( v ) );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawMethodDefinition, MethodDefinition>( nameof( RawMethodDefinition.ImplementationAttributes ), ( r, v ) => r.ImplementationAttributes = (MethodImplAttributes) v, ( args, v ) => args.Row.ImplementationAttributes = (MethodImplAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawMethodDefinition, MethodDefinition>( nameof( RawMethodDefinition.Attributes ), ( r, v ) => r.Attributes = (MethodAttributes) v, ( args, v ) => args.Row.Attributes = (MethodAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.BLOBSignature<RawMethodDefinition, MethodDefinition, MethodDefinitionSignature>( nameof( RawMethodDefinition.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawMethodDefinition, MethodDefinition>( nameof( RawMethodDefinition.ParameterList ), Tables.Parameter, ( r, v ) => r.ParameterList = v, ( args, v ) => args.Row.ParameterList = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawParameterDefinitionPointer, ParameterDefinitionPointer>> GetParamPtrColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawParameterDefinitionPointer, ParameterDefinitionPointer>( nameof( RawParameterDefinitionPointer.ParameterIndex ), Tables.Parameter, ( r, v ) => r.ParameterIndex = v, ( args, v ) => args.Row.ParameterIndex = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawParameterDefinition, ParameterDefinition>> GetParamColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawParameterDefinition, ParameterDefinition>( nameof( RawParameterDefinition.Attributes ), ( r, v ) => r.Attributes = (ParameterAttributes) v, ( args, v ) => args.Row.Attributes = (ParameterAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawParameterDefinition, ParameterDefinition>( nameof( RawParameterDefinition.Sequence ), ( r, v ) => r.Sequence = v, ( args, v ) => args.Row.Sequence = v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawParameterDefinition, ParameterDefinition>( nameof( RawParameterDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawInterfaceImplementation, InterfaceImplementation>> GetInterfaceImplColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawInterfaceImplementation, InterfaceImplementation>( nameof( RawInterfaceImplementation.Class ), Tables.TypeDef, ( r, v ) => r.Class = v, ( args, v ) => args.Row.Class = v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawInterfaceImplementation, InterfaceImplementation>( nameof( RawInterfaceImplementation.Interface ), CodedTableIndexDecoder.TypeDefOrRef, ( r, v ) => r.Interface = v, ( args, v ) => args.Row.Interface = v.GetValueOrDefault() );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawMemberReference, MemberReference>> GetMemberRefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawMemberReference, MemberReference>( nameof( RawMemberReference.DeclaringType ), CodedTableIndexDecoder.MemberRefParent, ( r, v ) => r.DeclaringType = v, ( args, v ) => args.Row.DeclaringType = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawMemberReference, MemberReference>( nameof( RawMemberReference.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.BLOBSignature<RawMemberReference, MemberReference, AbstractSignature>( nameof( RawMemberReference.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawConstantDefinition, ConstantDefinition>> GetConstantColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant8<RawConstantDefinition, ConstantDefinition>( nameof( RawConstantDefinition.Type ), ( r, v ) => r.Type = (SignatureElementTypes) v, ( args, v ) => args.Row.Type = (SignatureElementTypes) v );
         yield return DefaultColumnSerializationInfoFactory.Constant8<RawConstantDefinition, ConstantDefinition>( nameof( RawConstantDefinition.Padding ), ( r, v ) => r.Padding = (Byte) v, ( args, v ) => { } );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawConstantDefinition, ConstantDefinition>( nameof( RawConstantDefinition.Parent ), CodedTableIndexDecoder.HasConstant, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.BLOBCustom<RawConstantDefinition, ConstantDefinition>( nameof( RawConstantDefinition.Value ), ( r, v ) => r.Value = v, ( args, v, blobs ) => args.Row.Value = blobs.ReadConstantValue( v, args.Row.Type ) );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawCustomAttributeDefinition, CustomAttributeDefinition>> GetCustomAttributeColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawCustomAttributeDefinition, CustomAttributeDefinition>( nameof( RawCustomAttributeDefinition.Parent ), CodedTableIndexDecoder.HasCustomAttribute, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawCustomAttributeDefinition, CustomAttributeDefinition>( nameof( RawCustomAttributeDefinition.Type ), CodedTableIndexDecoder.CustomAttributeType, ( r, v ) => r.Type = v, ( args, v ) => args.Row.Type = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.BLOBCASignature<RawCustomAttributeDefinition, CustomAttributeDefinition>( nameof( RawCustomAttributeDefinition.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawFieldMarshal, FieldMarshal>> GetFieldMarshalColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawFieldMarshal, FieldMarshal>( nameof( RawFieldMarshal.Parent ), CodedTableIndexDecoder.HasFieldMarshal, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.BLOBMarshalingInfo<RawFieldMarshal, FieldMarshal>( nameof( RawFieldMarshal.NativeType ), ( r, v ) => r.NativeType = v, ( args, v ) => args.Row.NativeType = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawSecurityDefinition, SecurityDefinition>> GetDeclSecurityColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawSecurityDefinition, SecurityDefinition>( nameof( RawSecurityDefinition.Action ), ( r, v ) => r.Action = (SecurityAction) v, ( args, v ) => args.Row.Action = (SecurityAction) v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawSecurityDefinition, SecurityDefinition>( nameof( RawSecurityDefinition.Parent ), CodedTableIndexDecoder.HasSecurity, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.BLOBSecurityInformation<RawSecurityDefinition, SecurityDefinition>( nameof( RawSecurityDefinition.PermissionSets ), ( r, v ) => r.PermissionSets = v, ( args, v ) => args.Row.PermissionSets.AddRange( v ) );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawClassLayout, ClassLayout>> GetClassLayoutColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawClassLayout, ClassLayout>( nameof( RawClassLayout.PackingSize ), ( r, v ) => r.PackingSize = v, ( args, v ) => args.Row.PackingSize = v );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawClassLayout, ClassLayout>( nameof( RawClassLayout.ClassSize ), ( r, v ) => r.ClassSize = v, ( args, v ) => args.Row.ClassSize = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawClassLayout, ClassLayout>( nameof( RawClassLayout.Parent ), Tables.TypeDef, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawFieldLayout, FieldLayout>> GetFieldLayoutColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawFieldLayout, FieldLayout>( nameof( RawFieldLayout.Offset ), ( r, v ) => r.Offset = v, ( args, v ) => args.Row.Offset = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawFieldLayout, FieldLayout>( nameof( RawFieldLayout.Field ), Tables.Field, ( r, v ) => r.Field = v, ( args, v ) => args.Row.Field = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawStandaloneSignature, StandaloneSignature>> GetStandaloneSigColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.BLOBSignatureFull<RawStandaloneSignature, StandaloneSignature, AbstractSignature>( nameof( RawStandaloneSignature.Signature ), ( r, v ) => r.Signature = v, ( args, v, wasFieldSig ) =>
         {
            args.Row.Signature = v;
            args.Row.StoreSignatureAsFieldSignature = wasFieldSig;
         } );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawEventMap, EventMap>> GetEventMapColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawEventMap, EventMap>( nameof( RawEventMap.Parent ), Tables.TypeDef, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawEventMap, EventMap>( nameof( RawEventMap.EventList ), Tables.Event, ( r, v ) => r.EventList = v, ( args, v ) => args.Row.EventList = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawEventDefinitionPointer, EventDefinitionPointer>> GetEventPtrColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawEventDefinitionPointer, EventDefinitionPointer>( nameof( RawEventDefinitionPointer.EventIndex ), Tables.Event, ( r, v ) => r.EventIndex = v, ( args, v ) => args.Row.EventIndex = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawEventDefinition, EventDefinition>> GetEventDefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawEventDefinition, EventDefinition>( nameof( RawEventDefinition.Attributes ), ( r, v ) => r.Attributes = (EventAttributes) v, ( args, v ) => args.Row.Attributes = (EventAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawEventDefinition, EventDefinition>( nameof( RawEventDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawEventDefinition, EventDefinition>( nameof( RawEventDefinition.EventType ), CodedTableIndexDecoder.TypeDefOrRef, ( r, v ) => r.EventType = v, ( args, v ) => args.Row.EventType = v.GetValueOrDefault() );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawPropertyMap, PropertyMap>> GetPropertyMapColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawPropertyMap, PropertyMap>( nameof( RawPropertyMap.Parent ), Tables.TypeDef, ( r, v ) => r.Parent = v, ( args, v ) => args.Row.Parent = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawPropertyMap, PropertyMap>( nameof( RawPropertyMap.PropertyList ), Tables.Property, ( r, v ) => r.PropertyList = v, ( args, v ) => args.Row.PropertyList = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawPropertyDefinitionPointer, PropertyDefinitionPointer>> GetPropertyPtrColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawPropertyDefinitionPointer, PropertyDefinitionPointer>( nameof( RawPropertyDefinitionPointer.PropertyIndex ), Tables.Property, ( r, v ) => r.PropertyIndex = v, ( args, v ) => args.Row.PropertyIndex = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawPropertyDefinition, PropertyDefinition>> GetPropertyDefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawPropertyDefinition, PropertyDefinition>( nameof( RawPropertyDefinition.Attributes ), ( r, v ) => r.Attributes = (PropertyAttributes) v, ( args, v ) => args.Row.Attributes = (PropertyAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawPropertyDefinition, PropertyDefinition>( nameof( RawPropertyDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.BLOBSignature<RawPropertyDefinition, PropertyDefinition, PropertySignature>( nameof( RawPropertyDefinition.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawMethodSemantics, MethodSemantics>> GetMethodSemanticsColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawMethodSemantics, MethodSemantics>( nameof( RawMethodSemantics.Attributes ), ( r, v ) => r.Attributes = (MethodSemanticsAttributes) v, ( args, v ) => args.Row.Attributes = (MethodSemanticsAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawMethodSemantics, MethodSemantics>( nameof( RawMethodSemantics.Method ), Tables.MethodDef, ( r, v ) => r.Method = v, ( args, v ) => args.Row.Method = v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawMethodSemantics, MethodSemantics>( nameof( RawMethodSemantics.Associaton ), CodedTableIndexDecoder.HasSemantics, ( r, v ) => r.Associaton = v, ( args, v ) => args.Row.Associaton = v.GetValueOrDefault() );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawMethodImplementation, MethodImplementation>> GetMethodImplColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawMethodImplementation, MethodImplementation>( nameof( RawMethodImplementation.Class ), Tables.TypeDef, ( r, v ) => r.Class = v, ( args, v ) => args.Row.Class = v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawMethodImplementation, MethodImplementation>( nameof( RawMethodImplementation.MethodBody ), CodedTableIndexDecoder.MethodDefOrRef, ( r, v ) => r.MethodBody = v, ( args, v ) => args.Row.MethodBody = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawMethodImplementation, MethodImplementation>( nameof( RawMethodImplementation.MethodDeclaration ), CodedTableIndexDecoder.MethodDefOrRef, ( r, v ) => r.MethodDeclaration = v, ( args, v ) => args.Row.MethodDeclaration = v.GetValueOrDefault() );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawModuleReference, ModuleReference>> GetModuleRefColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawModuleReference, ModuleReference>( nameof( RawModuleReference.ModuleName ), ( r, v ) => r.ModuleName = v, ( args, v ) => args.Row.ModuleName = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawTypeSpecification, TypeSpecification>> GetTypeSpecColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.BLOBSignature<RawTypeSpecification, TypeSpecification, TypeSignature>( nameof( RawTypeSpecification.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawMethodImplementationMap, MethodImplementationMap>> GetImplMapColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawMethodImplementationMap, MethodImplementationMap>( nameof( RawMethodImplementationMap.Attributes ), ( r, v ) => r.Attributes = (PInvokeAttributes) v, ( args, v ) => args.Row.Attributes = (PInvokeAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawMethodImplementationMap, MethodImplementationMap>( nameof( RawMethodImplementationMap.MemberForwarded ), CodedTableIndexDecoder.MemberForwarded, ( r, v ) => r.MemberForwarded = v, ( args, v ) => args.Row.MemberForwarded = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawMethodImplementationMap, MethodImplementationMap>( nameof( RawMethodImplementationMap.ImportName ), ( r, v ) => r.ImportName = v, ( args, v ) => args.Row.ImportName = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawMethodImplementationMap, MethodImplementationMap>( nameof( RawMethodImplementationMap.ImportScope ), Tables.ModuleRef, ( r, v ) => r.ImportScope = v, ( args, v ) => args.Row.ImportScope = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawFieldRVA, FieldRVA>> GetFieldRVAColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawFieldRVA, FieldRVA>( nameof( RawFieldRVA.RVA ), ( r, v ) => r.RVA = v, ( args, v ) => args.RowArgs.FieldRVAs.Add( v ) );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawFieldRVA, FieldRVA>( nameof( RawFieldRVA.Field ), Tables.Field, ( r, v ) => r.Field = v, ( args, v ) => args.Row.Field = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawEditAndContinueLog, EditAndContinueLog>> GetENCLogColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawEditAndContinueLog, EditAndContinueLog>( nameof( RawEditAndContinueLog.Token ), ( r, v ) => r.Token = v, ( args, v ) => args.Row.Token = v );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawEditAndContinueLog, EditAndContinueLog>( nameof( RawEditAndContinueLog.FuncCode ), ( r, v ) => r.FuncCode = v, ( args, v ) => args.Row.FuncCode = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawEditAndContinueMap, EditAndContinueMap>> GetENCMapColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawEditAndContinueMap, EditAndContinueMap>( nameof( RawEditAndContinueMap.Token ), ( r, v ) => r.Token = v, ( args, v ) => args.Row.Token = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawAssemblyDefinition, AssemblyDefinition>> GetAssemblyDefColumns()
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
      protected static IEnumerable<DefaultColumnSerializationInfo<RawAssemblyDefinitionProcessor, AssemblyDefinitionProcessor>> GetAssemblyDefProcessorColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyDefinitionProcessor, AssemblyDefinitionProcessor>( nameof( RawAssemblyDefinitionProcessor.Processor ), ( r, v ) => r.Processor = v, ( args, v ) => args.Row.Processor = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawAssemblyDefinitionOS, AssemblyDefinitionOS>> GetAssemblyDefOSColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyDefinitionOS, AssemblyDefinitionOS>( nameof( RawAssemblyDefinitionOS.OSPlatformID ), ( r, v ) => r.OSPlatformID = v, ( args, v ) => args.Row.OSPlatformID = v );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyDefinitionOS, AssemblyDefinitionOS>( nameof( RawAssemblyDefinitionOS.OSMajorVersion ), ( r, v ) => r.OSMajorVersion = v, ( args, v ) => args.Row.OSMajorVersion = v );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyDefinitionOS, AssemblyDefinitionOS>( nameof( RawAssemblyDefinitionOS.OSMinorVersion ), ( r, v ) => r.OSMinorVersion = v, ( args, v ) => args.Row.OSMinorVersion = v );
      }
#pragma warning restore 618

      protected static IEnumerable<DefaultColumnSerializationInfo<RawAssemblyReference, AssemblyReference>> GetAssemblyRefColumns()
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
      protected static IEnumerable<DefaultColumnSerializationInfo<RawAssemblyReferenceProcessor, AssemblyReferenceProcessor>> GetAssemblyRefProcessorColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyReferenceProcessor, AssemblyReferenceProcessor>( nameof( RawAssemblyReferenceProcessor.Processor ), ( r, v ) => r.Processor = v, ( args, v ) => args.Row.Processor = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawAssemblyReferenceProcessor, AssemblyReferenceProcessor>( nameof( RawAssemblyReferenceProcessor.AssemblyRef ), Tables.AssemblyRef, ( r, v ) => r.AssemblyRef = v, ( args, v ) => args.Row.AssemblyRef = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawAssemblyReferenceOS, AssemblyReferenceOS>> GetAssemblyRefOSColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyReferenceOS, AssemblyReferenceOS>( nameof( RawAssemblyReferenceOS.OSPlatformID ), ( r, v ) => r.OSPlatformID = v, ( args, v ) => args.Row.OSPlatformID = v );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyReferenceOS, AssemblyReferenceOS>( nameof( RawAssemblyReferenceOS.OSMajorVersion ), ( r, v ) => r.OSMajorVersion = v, ( args, v ) => args.Row.OSMajorVersion = v );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawAssemblyReferenceOS, AssemblyReferenceOS>( nameof( RawAssemblyReferenceOS.OSMinorVersion ), ( r, v ) => r.OSMinorVersion = v, ( args, v ) => args.Row.OSMinorVersion = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawAssemblyReferenceOS, AssemblyReferenceOS>( nameof( RawAssemblyReferenceOS.AssemblyRef ), Tables.AssemblyRef, ( r, v ) => r.AssemblyRef = v, ( args, v ) => args.Row.AssemblyRef = v );
      }
#pragma warning restore 618

      protected static IEnumerable<DefaultColumnSerializationInfo<RawFileReference, FileReference>> GetFileColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawFileReference, FileReference>( nameof( RawFileReference.Attributes ), ( r, v ) => r.Attributes = (FileAttributes) v, ( args, v ) => args.Row.Attributes = (FileAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawFileReference, FileReference>( nameof( RawFileReference.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.BLOBArray<RawFileReference, FileReference>( nameof( RawFileReference.HashValue ), ( r, v ) => r.HashValue = v, ( args, v ) => args.Row.HashValue = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawExportedType, ExportedType>> GetExportedTypeColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawExportedType, ExportedType>( nameof( RawExportedType.Attributes ), ( r, v ) => r.Attributes = (TypeAttributes) v, ( args, v ) => args.Row.Attributes = (TypeAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawExportedType, ExportedType>( nameof( RawExportedType.TypeDefinitionIndex ), ( r, v ) => r.TypeDefinitionIndex = v, ( args, v ) => args.Row.TypeDefinitionIndex = v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawExportedType, ExportedType>( nameof( RawExportedType.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawExportedType, ExportedType>( nameof( RawExportedType.Namespace ), ( r, v ) => r.Namespace = v, ( args, v ) => args.Row.Namespace = v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawExportedType, ExportedType>( nameof( RawExportedType.Implementation ), CodedTableIndexDecoder.Implementation, ( r, v ) => r.Implementation = v, ( args, v ) => args.Row.Implementation = v.GetValueOrDefault() );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawManifestResource, ManifestResource>> GetManifestResourceColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawManifestResource, ManifestResource>( nameof( RawManifestResource.Offset ), ( r, v ) => r.Offset = v, ( args, v ) => args.Row.Offset = v );
         yield return DefaultColumnSerializationInfoFactory.Constant32<RawManifestResource, ManifestResource>( nameof( RawManifestResource.Attributes ), ( r, v ) => r.Attributes = (ManifestResourceAttributes) v, ( args, v ) => args.Row.Attributes = (ManifestResourceAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawManifestResource, ManifestResource>( nameof( RawManifestResource.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawManifestResource, ManifestResource>( nameof( RawManifestResource.Implementation ), CodedTableIndexDecoder.Implementation, ( r, v ) => r.Implementation = v, ( args, v ) => args.Row.Implementation = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawNestedClassDefinition, NestedClassDefinition>> GetNestedClassColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawNestedClassDefinition, NestedClassDefinition>( nameof( RawNestedClassDefinition.NestedClass ), Tables.TypeDef, ( r, v ) => r.NestedClass = v, ( args, v ) => args.Row.NestedClass = v );
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawNestedClassDefinition, NestedClassDefinition>( nameof( RawNestedClassDefinition.EnclosingClass ), Tables.TypeDef, ( r, v ) => r.EnclosingClass = v, ( args, v ) => args.Row.EnclosingClass = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawGenericParameterDefinition, GenericParameterDefinition>> GetGenericParamColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawGenericParameterDefinition, GenericParameterDefinition>( nameof( RawGenericParameterDefinition.GenericParameterIndex ), ( r, v ) => r.GenericParameterIndex = v, ( args, v ) => args.Row.GenericParameterIndex = v );
         yield return DefaultColumnSerializationInfoFactory.Constant16<RawGenericParameterDefinition, GenericParameterDefinition>( nameof( RawGenericParameterDefinition.Attributes ), ( r, v ) => r.Attributes = (GenericParameterAttributes) v, ( args, v ) => args.Row.Attributes = (GenericParameterAttributes) v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawGenericParameterDefinition, GenericParameterDefinition>( nameof( RawGenericParameterDefinition.Owner ), CodedTableIndexDecoder.TypeOrMethodDef, ( r, v ) => r.Owner = v, ( args, v ) => args.Row.Owner = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.SystemString<RawGenericParameterDefinition, GenericParameterDefinition>( nameof( RawGenericParameterDefinition.Name ), ( r, v ) => r.Name = v, ( args, v ) => args.Row.Name = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawMethodSpecification, MethodSpecification>> GetMethodSpecColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawMethodSpecification, MethodSpecification>( nameof( RawMethodSpecification.Method ), CodedTableIndexDecoder.MethodDefOrRef, ( r, v ) => r.Method = v, ( args, v ) => args.Row.Method = v.GetValueOrDefault() );
         yield return DefaultColumnSerializationInfoFactory.BLOBSignature<RawMethodSpecification, MethodSpecification, GenericMethodSignature>( nameof( RawMethodSpecification.Signature ), ( r, v ) => r.Signature = v, ( args, v ) => args.Row.Signature = v );
      }

      protected static IEnumerable<DefaultColumnSerializationInfo<RawGenericParameterConstraintDefinition, GenericParameterConstraintDefinition>> GetGenericParamConstraintColumns()
      {
         yield return DefaultColumnSerializationInfoFactory.SimpleReference<RawGenericParameterConstraintDefinition, GenericParameterConstraintDefinition>( nameof( RawGenericParameterConstraintDefinition.Owner ), Tables.GenericParameter, ( r, v ) => r.Owner = v, ( args, v ) => args.Row.Owner = v );
         yield return DefaultColumnSerializationInfoFactory.CodedReference<RawGenericParameterConstraintDefinition, GenericParameterConstraintDefinition>( nameof( RawGenericParameterConstraintDefinition.Constraint ), CodedTableIndexDecoder.TypeDefOrRef, ( r, v ) => r.Constraint = v, ( args, v ) => args.Row.Constraint = v.GetValueOrDefault() );
      }

      protected virtual TableSerializationInfo CreateTableInfo<TRawRow, TRow>( Tables table, IEnumerable<DefaultColumnSerializationInfo<TRawRow, TRow>> columns )
         where TRawRow : class, new()
         where TRow : class, new()
      {
         return new DefaultTableSerializationInfo<TRawRow, TRow>(
            table,
            columns
            );
      }
   }

   public class DefaultTableSerializationInfo<TRawRow, TRow> : TableSerializationInfo
      where TRawRow : class, new()
      where TRow : class, new()
   {

      private readonly IEnumerable<DefaultColumnSerializationInfo<TRawRow, TRow>> _columns;

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

      public Tables Table { get; }

      public TableSerializationSupport CreateSupport(
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

   public class DefaultTableSerializationSupport<TRawRow, TRow> : TableSerializationSupport
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

      public ArrayQuery<ColumnSerializationSupport> ColumnSerializationSupports { get; }

      public Object ReadRow( RowReadingArguments args )
      {
         var row = new TRow();
         var columnArgs = new ColumnSettingArguments<TRow>( row, args );
         var stream = args.Stream;
         for ( var i = 0; i < this._columnArray.Length; ++i )
         {
            this._columnArray[i].Setter( columnArgs, this.ColumnSerializationSupports[i].ReadRawValue( stream ) );
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

   public sealed class ColumnSerializationSupport_Constant8 : ColumnSerializationSupport
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
   public sealed class ColumnSerializationSupport_Constant16 : ColumnSerializationSupport
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

   public sealed class ColumnSerializationSupport_Constant32 : ColumnSerializationSupport
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

   internal static Boolean DecompressUInt32( this StreamHelper stream, out Int32 value )
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
         else
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
}