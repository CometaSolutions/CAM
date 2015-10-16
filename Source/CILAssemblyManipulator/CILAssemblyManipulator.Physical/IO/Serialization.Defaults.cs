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
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO
{
   public struct ColumnSerializationInfo<TRawRow, TRow> : ColumnSerializationInfo
      where TRawRow : class
      where TRow : class
   {

      public ColumnSerializationInfo(
         String columnName,
         ColumnSerializationSupport serialization,
         Action<TRawRow, Int32> rawSetter,
         Action<TRow, Int32, ReaderBLOBStreamHandler, ReaderGUIDStreamHandler, ReaderStringStreamHandler> setter
         )
      {
         ArgumentValidator.ValidateNotNull( "Column name", columnName );
         ArgumentValidator.ValidateNotNull( "Serialization", serialization );
         ArgumentValidator.ValidateNotNull( "Raw setter", rawSetter );
         ArgumentValidator.ValidateNotNull( "Setter", setter );

         this.ColumnName = columnName;
         this.Serialization = serialization;
         this.RawSetter = rawSetter;
         this.Setter = setter;
      }

      public String ColumnName { get; }
      public ColumnSerializationSupport Serialization { get; }
      public Action<TRawRow, Int32> RawSetter { get; }
      public Action<TRow, Int32, ReaderBLOBStreamHandler, ReaderGUIDStreamHandler, ReaderStringStreamHandler> Setter { get; }

      public void SetRawValue( Object row, Int32 value )
      {
         this.RawSetter( (TRawRow) row, value );
      }
   }

   public class DefaultMetaDataSerializationSupportProvider : MetaDataSerializationSupportProvider
   {
      public virtual TableSerializationSupportProvider CreateTableSerializationSupportProvider(
         ArrayQuery<Int32> tableSizes,
         Boolean wideBLOBs,
         Boolean wideGUIDs,
         Boolean wideStrings
         )
      {
         return new DefaultTableSerializationSupportProvider( tableSizes, wideBLOBs, wideGUIDs, wideStrings );
      }
   }


   public class DefaultTableSerializationSupportProvider : TableSerializationSupportProvider
   {

      private readonly ColumnSerializationSupport[] _simpleIndices;
      public DefaultTableSerializationSupportProvider(
         ArrayQuery<Int32> tableSizes,
         Boolean wideBLOBs,
         Boolean wideGUIDs,
         Boolean wideStrings
         )
      {
         ArgumentValidator.ValidateNotNull( "Table sizes", tableSizes );

         this.TableSizes = tableSizes;
         this.WideBLOBs = wideBLOBs;
         this.WideGUIDs = wideGUIDs;
         this.WideStrings = wideStrings;

         this.Constant8 = new ColumnSerializationSupport_Constant8();
         this.Constant16 = new ColumnSerializationSupport_Constant16();
         this.Constant32 = new ColumnSerializationSupport_Constant32();

         this.BLOBIndex = wideBLOBs ?
            (ColumnSerializationSupport) new ColumnSerializationSupport_Constant32() :
            new ColumnSerializationSupport_Constant16();
         this.GUIDIndex = wideGUIDs ?
            (ColumnSerializationSupport) new ColumnSerializationSupport_Constant32() :
            new ColumnSerializationSupport_Constant16();
         this.StringIndex = wideStrings ?
            (ColumnSerializationSupport) new ColumnSerializationSupport_Constant32() :
            new ColumnSerializationSupport_Constant16();

         this.TypeDefOrRef = this.CodedIndex( ColumnSerializationSupport_CodedTableIndex.TypeDefOrRef );
         this.TypeDefOrRefNullable = this.CodedIndexNullable( ColumnSerializationSupport_CodedTableIndex.TypeDefOrRef );
         this.HasConstant = this.CodedIndex( ColumnSerializationSupport_CodedTableIndex.TypeDefOrRef );
         this.HasCustomAttribute = this.CodedIndex( ColumnSerializationSupport_CodedTableIndex.TypeDefOrRef );
         this.HasFieldMarshal = this.CodedIndex( ColumnSerializationSupport_CodedTableIndex.TypeDefOrRef );
         this.HasSecurity = this.CodedIndex( ColumnSerializationSupport_CodedTableIndex.TypeDefOrRef );
         this.MemberRefParent = this.CodedIndex( ColumnSerializationSupport_CodedTableIndex.TypeDefOrRef );
         this.HasSemantics = this.CodedIndex( ColumnSerializationSupport_CodedTableIndex.TypeDefOrRef );
         this.MethodDefOrRef = this.CodedIndex( ColumnSerializationSupport_CodedTableIndex.TypeDefOrRef );
         this.MemberForwarded = this.CodedIndex( ColumnSerializationSupport_CodedTableIndex.TypeDefOrRef );
         this.Implementation = this.CodedIndex( ColumnSerializationSupport_CodedTableIndex.Implementation );
         this.ImplementationNullable = this.CodedIndexNullable( ColumnSerializationSupport_CodedTableIndex.Implementation );
         this.CustomAttributeType = this.CodedIndex( ColumnSerializationSupport_CodedTableIndex.TypeDefOrRef );
         this.ResolutionScopeNullable = this.CodedIndexNullable( ColumnSerializationSupport_CodedTableIndex.ResolutionScope );
         this.TypeOrMethodDef = this.CodedIndex( ColumnSerializationSupport_CodedTableIndex.TypeDefOrRef );

         this._simpleIndices = tableSizes
            .Select( ( size, idx ) => this.NewSimpleIndex( (Tables) idx ) )
            .ToArray();
      }

      public virtual TableSerializationSupport CreateSerializationSupport(
         Tables table
         )
      {
         switch ( table )
         {
            case Tables.Module:
               return this.CreateTableSupport( table, this.GetModuleDefColumns() );
            case Tables.TypeRef:
               return this.CreateTableSupport( table, this.GetTypeRefColumns() );
            case Tables.TypeDef:
               return this.CreateTableSupport( table, this.GetTypeDefColumns() );
            case Tables.FieldPtr:
               return this.CreateTableSupport( table, this.GetFieldPtrColumns() );
            case Tables.Field:
               return this.CreateTableSupport( table, this.GetFieldDefColumns() );
            case Tables.MethodPtr:
               return this.CreateTableSupport( table, this.GetMethodPtrColumns() );
            case Tables.MethodDef:
               return this.CreateTableSupport( table, this.GetMethodDefColumns() );
            case Tables.ParameterPtr:
               return this.CreateTableSupport( table, this.GetParamPtrColumns() );
            case Tables.Parameter:
               return this.CreateTableSupport( table, this.GetParamColumns() );
            case Tables.InterfaceImpl:
               return this.CreateTableSupport( table, this.GetInterfaceImplColumns() );
            case Tables.MemberRef:
               return this.CreateTableSupport( table, this.GetMemberRefColumns() );
            case Tables.Constant:
               return this.CreateTableSupport( table, this.GetConstantColumns() );
            case Tables.CustomAttribute:
               return this.CreateTableSupport( table, this.GetCustomAttributeColumns() );
            case Tables.FieldMarshal:
               return this.CreateTableSupport( table, this.GetFieldMarshalColumns() );
            case Tables.DeclSecurity:
               return this.CreateTableSupport( table, this.GetDeclSecurityColumns() );
            case Tables.ClassLayout:
               return this.CreateTableSupport( table, this.GetClassLayoutColumns() );
            case Tables.FieldLayout:
               return this.CreateTableSupport( table, this.GetFieldLayoutColumns() );
            case Tables.StandaloneSignature:
               return this.CreateTableSupport( table, this.GetStandaloneSigColumns() );
            case Tables.EventMap:
               return this.CreateTableSupport( table, this.GetEventMapColumns() );
            case Tables.EventPtr:
               return this.CreateTableSupport( table, this.GetEventPtrColumns() );
            case Tables.Event:
               return this.CreateTableSupport( table, this.GetEventDefColumns() );
            case Tables.PropertyMap:
               return this.CreateTableSupport( table, this.GetPropertyMapColumns() );
            case Tables.PropertyPtr:
               return this.CreateTableSupport( table, this.GetPropertyPtrColumns() );
            case Tables.Property:
               return this.CreateTableSupport( table, this.GetPropertyDefColumns() );
            case Tables.MethodSemantics:
               return this.CreateTableSupport( table, this.GetMethodSemanticsColumns() );
            case Tables.MethodImpl:
               return this.CreateTableSupport( table, this.GetMethodImplColumns() );
            case Tables.ModuleRef:
               return this.CreateTableSupport( table, this.GetModuleRefColumns() );
            case Tables.TypeSpec:
               return this.CreateTableSupport( table, this.GetTypeSpecColumns() );
            case Tables.ImplMap:
               return this.CreateTableSupport( table, this.GetImplMapColumns() );
            case Tables.FieldRVA:
               return this.CreateTableSupport( table, this.GetFieldRVAColumns() );
            case Tables.EncLog:
               return this.CreateTableSupport( table, this.GetENCLogColumns() );
            case Tables.EncMap:
               return this.CreateTableSupport( table, this.GetENCMapColumns() );
            case Tables.Assembly:
               return this.CreateTableSupport( table, this.GetAssemblyDefColumns() );
            case Tables.AssemblyProcessor:
               return this.CreateTableSupport( table, this.GetAssemblyDefProcessorColumns() );
            case Tables.AssemblyOS:
               return this.CreateTableSupport( table, this.GetAssemblyDefOSColumns() );
            case Tables.AssemblyRef:
               return this.CreateTableSupport( table, this.GetAssemblyRefColumns() );
            case Tables.AssemblyRefProcessor:
               return this.CreateTableSupport( table, this.GetAssemblyRefProcessorColumns() );
            case Tables.AssemblyRefOS:
               return this.CreateTableSupport( table, this.GetAssemblyRefOSColumns() );
            case Tables.File:
               return this.CreateTableSupport( table, this.GetFileColumns() );
            case Tables.ExportedType:
               return this.CreateTableSupport( table, this.GetExportedTypeColumns() );
            case Tables.ManifestResource:
               return this.CreateTableSupport( table, this.GetManifestResourceColumns() );
            case Tables.NestedClass:
               return this.CreateTableSupport( table, this.GetNestedClassColumns() );
            case Tables.GenericParameter:
               return this.CreateTableSupport( table, this.GetGenericParamColumns() );
            case Tables.MethodSpec:
               return this.CreateTableSupport( table, this.GetMethodSpecColumns() );
            case Tables.GenericParameterConstraint:
               return this.CreateTableSupport( table, this.GetGenericParamConstraintColumns() );
            default:
               return null;
         }
      }

      protected ArrayQuery<Int32> TableSizes { get; }
      protected Boolean WideBLOBs { get; }
      protected Boolean WideGUIDs { get; }
      protected Boolean WideStrings { get; }

      protected virtual IEnumerable<ColumnSerializationInfo<RawModuleDefinition, ModuleDefinition>> GetModuleDefColumns()
      {
         yield return new ColumnSerializationInfo<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.Generation ), this.Constant16, ( r, v ) => r.Generation = v, ( r, v, b, g, s ) => r.Generation = (Int16) v );
         yield return new ColumnSerializationInfo<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.Name ), this.StringIndex, ( r, v ) => r.Name = v, ( r, v, b, g, s ) => r.Name = this.ReadString( v, s ) );
         yield return new ColumnSerializationInfo<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.ModuleGUID ), this.GUIDIndex, ( r, v ) => r.ModuleGUID = v, ( r, v, b, g, s ) => r.ModuleGUID = this.ReadGUID( v, g ) );
         yield return new ColumnSerializationInfo<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.EditAndContinueGUID ), this.GUIDIndex, ( r, v ) => r.EditAndContinueGUID = v, ( r, v, b, g, s ) => r.EditAndContinueGUID = this.ReadGUID( v, g ) );
         yield return new ColumnSerializationInfo<RawModuleDefinition, ModuleDefinition>( nameof( RawModuleDefinition.EditAndContinueBaseGUID ), this.GUIDIndex, ( r, v ) => r.EditAndContinueBaseGUID = v, ( r, v, b, g, s ) => r.EditAndContinueBaseGUID = this.ReadGUID( v, g ) );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawTypeReference, TypeReference>> GetTypeRefColumns()
      {
         yield return new ColumnSerializationInfo<RawTypeReference, TypeReference>( nameof( RawTypeReference.ResolutionScope ), this.ResolutionScopeNullable, ( r, v ) => r.ResolutionScope = v, ( r, v, b, g, s ) => r.ResolutionScope = this.ResolutionScopeNullable.DecodeTableIndex( v ) );
         yield return new ColumnSerializationInfo<RawTypeReference, TypeReference>( nameof( RawTypeReference.Name ), this.StringIndex, ( r, v ) => r.Name = v, ( r, v, b, g, s ) => r.Name = this.ReadString( v, s ) );
         yield return new ColumnSerializationInfo<RawTypeReference, TypeReference>( nameof( RawTypeReference.Namespace ), this.StringIndex, ( r, v ) => r.Namespace = v, ( r, v, b, g, s ) => r.Namespace = this.ReadString( v, s ) );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawTypeDefinition, TypeDefinition>> GetTypeDefColumns()
      {
         yield return new ColumnSerializationInfo<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.Attributes ), this.Constant32, ( r, v ) => r.Attributes = (TypeAttributes) v, ( r, v, b, g, s ) => r.Attributes = (TypeAttributes) v );
         yield return new ColumnSerializationInfo<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.Name ), this.StringIndex, ( r, v ) => r.Name = v, ( r, v, b, g, s ) => r.Name = this.ReadString( v, s ) );
         yield return new ColumnSerializationInfo<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.Namespace ), this.StringIndex, ( r, v ) => r.Namespace = v, ( r, v, b, g, s ) => r.Namespace = this.ReadString( v, s ) );
         yield return new ColumnSerializationInfo<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.BaseType ), this.TypeDefOrRefNullable, ( r, v ) => r.BaseType = v, ( r, v, b, g, s ) => r.BaseType = this.TypeDefOrRefNullable.DecodeTableIndex( v ) );
         yield return new ColumnSerializationInfo<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.FieldList ), this.GetSimpleIndex( Tables.Field ), ( r, v ) => r.FieldList = v, ( r, v, b, g, s ) => r.FieldList = this.ReadSimpleIndex( v, Tables.Field ) );
         yield return new ColumnSerializationInfo<RawTypeDefinition, TypeDefinition>( nameof( RawTypeDefinition.MethodList ), this.GetSimpleIndex( Tables.MethodDef ), ( r, v ) => r.MethodList = v, ( r, v, b, g, s ) => r.MethodList = this.ReadSimpleIndex( v, Tables.MethodDef ) );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawFieldDefinitionPointer, FieldDefinitionPointer>> GetFieldPtrColumns()
      {
         yield return new ColumnSerializationInfo<RawFieldDefinitionPointer, FieldDefinitionPointer>( nameof( RawFieldDefinitionPointer.FieldIndex ), this.GetSimpleIndex( Tables.Field ), ( r, v ) => r.FieldIndex = v, ( r, v, b, g, s ) => r.FieldIndex = this.ReadSimpleIndex( v, Tables.Field ) );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawFieldDefinition, FieldDefinition>> GetFieldDefColumns()
      {
         yield return new ColumnSerializationInfo<RawFieldDefinition, FieldDefinition>( nameof( RawFieldDefinition.Attributes ), this.Constant16, ( r, v ) => r.Attributes = (FieldAttributes) v, ( r, v, b, g, s ) => r.Attributes = (FieldAttributes) v );
         yield return new ColumnSerializationInfo<RawFieldDefinition, FieldDefinition>( nameof( RawFieldDefinition.Name ), this.StringIndex, ( r, v ) => r.Name = v, ( r, v, b, g, s ) => r.Name = this.ReadString( v, s ) );
         yield return new ColumnSerializationInfo<RawFieldDefinition, FieldDefinition>( nameof( RawFieldDefinition.Signature ), this.BLOBIndex, ( r, v ) => r.Signature = v, ( r, v, b, g, s ) => r.Signature = this.ReadBLOBSignature<FieldSignature>( v, b ) );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawMethodDefinitionPointer, MethodDefinitionPointer>> GetMethodPtrColumns()
      {
         yield return new ColumnSerializationInfo<RawMethodDefinitionPointer, MethodDefinitionPointer>( nameof( RawMethodDefinitionPointer.MethodIndex ), this.GetSimpleIndex( Tables.MethodDef ), ( r, v ) => r.MethodIndex = v, ( r, v, b, g, s ) => r.MethodIndex = this.ReadSimpleIndex( v, Tables.MethodDef ) );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawMethodDefinition, MethodDefinition>> GetMethodDefColumns()
      {
         yield return new ColumnSerializationInfo<RawMethodDefinition, MethodDefinition>( nameof( RawMethodDefinition.RVA ), this.Constant32, ( r, v ) => r.RVA = v, ( r, v, b, g, s ) => { } );
         yield return new ColumnSerializationInfo<RawMethodDefinition, MethodDefinition>( nameof( RawMethodDefinition.ImplementationAttributes ), this.Constant16, ( r, v ) => r.ImplementationAttributes = (MethodImplAttributes) v, ( r, v, b, g, s ) => r.ImplementationAttributes = (MethodImplAttributes) v );
         yield return new ColumnSerializationInfo<RawMethodDefinition, MethodDefinition>( nameof( RawMethodDefinition.Attributes ), this.Constant16, ( r, v ) => r.Attributes = (MethodAttributes) v, ( r, v, b, g, s ) => r.Attributes = (MethodAttributes) v );
         yield return new ColumnSerializationInfo<RawMethodDefinition, MethodDefinition>( nameof( RawMethodDefinition.Signature ), this.BLOBIndex, ( r, v ) => r.Signature = v, ( r, v, b, g, s ) => r.Signature = this.ReadBLOBSignature<MethodDefinitionSignature>( v, b ) );
         yield return new ColumnSerializationInfo<RawMethodDefinition, MethodDefinition>( nameof( RawMethodDefinition.ParameterList ), this.GetSimpleIndex( Tables.Parameter ), ( r, v ) => r.ParameterList = v, ( r, v, b, g, s ) => r.ParameterList = this.ReadSimpleIndex( v, Tables.Parameter ) );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawParameterDefinitionPointer, ParameterDefinitionPointer>> GetParamPtrColumns()
      {
         yield return new ColumnSerializationInfo<RawParameterDefinitionPointer, ParameterDefinitionPointer>( nameof( RawParameterDefinitionPointer.ParameterIndex ), this.GetSimpleIndex( Tables.Parameter ), ( r, v ) => r.ParameterIndex = v, ( r, v, b, g, s ) => r.ParameterIndex = this.ReadSimpleIndex( v, Tables.Parameter ) );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawParameterDefinition, ParameterDefinition>> GetParamColumns()
      {
         yield return new ColumnSerializationInfo<RawParameterDefinition, ParameterDefinition>( nameof( RawParameterDefinition.Attributes ), this.Constant16, ( r, v ) => r.Attributes = (ParameterAttributes) v, ( r, v, b, g, s ) => r.Attributes = (ParameterAttributes) v );
         yield return new ColumnSerializationInfo<RawParameterDefinition, ParameterDefinition>( nameof( RawParameterDefinition.Sequence ), this.Constant16, ( r, v ) => r.Sequence = v, ( r, v, b, g, s ) => r.Sequence = v );
         yield return new ColumnSerializationInfo<RawParameterDefinition, ParameterDefinition>( nameof( RawParameterDefinition.Name ), this.StringIndex, ( r, v ) => r.Name = v, ( r, v, b, g, s ) => r.Name = this.ReadString( v, s ) );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawInterfaceImplementation, InterfaceImplementation>> GetInterfaceImplColumns()
      {
         yield return new ColumnSerializationInfo<RawInterfaceImplementation, InterfaceImplementation>( nameof( RawInterfaceImplementation.Class ), this.GetSimpleIndex( Tables.TypeDef ), ( r, v ) => r.Class = v, ( r, v, b, g, s ) => r.Class = this.ReadSimpleIndex( v, Tables.TypeDef ) );
         yield return new ColumnSerializationInfo<RawInterfaceImplementation, InterfaceImplementation>( nameof( RawInterfaceImplementation.Interface ), this.TypeDefOrRef, ( r, v ) => r.Interface = v, ( r, v, b, g, s ) => r.Interface = this.TypeDefOrRef.DecodeTableIndex( v ).GetValueOrDefault() );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawMemberReference, MemberReference>> GetMemberRefColumns()
      {
         yield return new ColumnSerializationInfo<RawMemberReference, MemberReference>( nameof( RawMemberReference.DeclaringType ), this.MemberRefParent, ( r, v ) => r.DeclaringType = v, ( r, v, b, g, s ) => r.DeclaringType = this.MemberRefParent.DecodeTableIndex( v ).GetValueOrDefault() );
         yield return new ColumnSerializationInfo<RawMemberReference, MemberReference>( nameof( RawMemberReference.Name ), this.StringIndex, ( r, v ) => r.Name = v, ( r, v, b, g, s ) => r.Name = this.ReadString( v, s ) );
         yield return new ColumnSerializationInfo<RawMemberReference, MemberReference>( nameof( RawMemberReference.Signature ), this.BLOBIndex, ( r, v ) => r.Signature = v, ( r, v, b, g, s ) => r.Signature = this.ReadBLOBSignature<AbstractSignature>( v, b ) );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawConstantDefinition, ConstantDefinition>> GetConstantColumns()
      {
         yield return new ColumnSerializationInfo<RawConstantDefinition, ConstantDefinition>( nameof( RawConstantDefinition.Type ), this.Constant8, ( r, v ) => r.Type = (SignatureElementTypes) v );
         yield return new ColumnSerializationInfo<RawConstantDefinition, ConstantDefinition>( nameof( RawConstantDefinition.Padding ), this.Constant8, ( r, v ) => r.Padding = (Byte) v );
         yield return new ColumnSerializationInfo<RawConstantDefinition, ConstantDefinition>( nameof( RawConstantDefinition.Parent ), this.HasConstant, ( r, v ) => r.Parent = v );
         yield return new ColumnSerializationInfo<RawConstantDefinition, ConstantDefinition>( nameof( RawConstantDefinition.Value ), this.BLOBIndex, ( r, v ) => r.Value = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawCustomAttributeDefinition, CustomAttributeDefinition>> GetCustomAttributeColumns()
      {
         yield return new ColumnSerializationInfo<RawCustomAttributeDefinition, CustomAttributeDefinition>( nameof( RawCustomAttributeDefinition.Parent ), this.HasCustomAttribute, ( r, v ) => r.Parent = v );
         yield return new ColumnSerializationInfo<RawCustomAttributeDefinition, CustomAttributeDefinition>( nameof( RawCustomAttributeDefinition.Type ), this.CustomAttributeType, ( r, v ) => r.Type = v );
         yield return new ColumnSerializationInfo<RawCustomAttributeDefinition, CustomAttributeDefinition>( nameof( RawCustomAttributeDefinition.Signature ), this.BLOBIndex, ( r, v ) => r.Signature = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawFieldMarshal, FieldMarshal>> GetFieldMarshalColumns()
      {
         yield return new ColumnSerializationInfo<RawFieldMarshal, FieldMarshal>( nameof( RawFieldMarshal.Parent ), this.HasFieldMarshal, ( r, v ) => r.Parent = v );
         yield return new ColumnSerializationInfo<RawFieldMarshal, FieldMarshal>( nameof( RawFieldMarshal.NativeType ), this.BLOBIndex, ( r, v ) => r.NativeType = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawSecurityDefinition, SecurityDefinition>> GetDeclSecurityColumns()
      {
         yield return new ColumnSerializationInfo<RawSecurityDefinition, SecurityDefinition>( nameof( RawSecurityDefinition.Action ), this.Constant16, ( r, v ) => r.Action = (SecurityAction) v );
         yield return new ColumnSerializationInfo<RawSecurityDefinition, SecurityDefinition>( nameof( RawSecurityDefinition.Parent ), this.HasSecurity, ( r, v ) => r.Parent = v );
         yield return new ColumnSerializationInfo<RawSecurityDefinition, SecurityDefinition>( nameof( RawSecurityDefinition.PermissionSets ), this.BLOBIndex, ( r, v ) => r.PermissionSets = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawClassLayout, ClassLayout>> GetClassLayoutColumns()
      {
         yield return new ColumnSerializationInfo<RawClassLayout, ClassLayout>( nameof( RawClassLayout.PackingSize ), this.Constant16, ( r, v ) => r.PackingSize = v );
         yield return new ColumnSerializationInfo<RawClassLayout, ClassLayout>( nameof( RawClassLayout.ClassSize ), this.Constant32, ( r, v ) => r.ClassSize = v );
         yield return new ColumnSerializationInfo<RawClassLayout, ClassLayout>( nameof( RawClassLayout.Parent ), this.GetSimpleIndex( Tables.TypeDef ), ( r, v ) => r.Parent = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawFieldLayout, FieldLayout>> GetFieldLayoutColumns()
      {
         yield return new ColumnSerializationInfo<RawFieldLayout, FieldLayout>( nameof( RawFieldLayout.Offset ), this.Constant32, ( r, v ) => r.Offset = v );
         yield return new ColumnSerializationInfo<RawFieldLayout, FieldLayout>( nameof( RawFieldLayout.Field ), this.GetSimpleIndex( Tables.TypeDef ), ( r, v ) => r.Field = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawStandaloneSignature, StandaloneSignature>> GetStandaloneSigColumns()
      {
         yield return new ColumnSerializationInfo<RawStandaloneSignature, StandaloneSignature>( nameof( RawStandaloneSignature.Signature ), this.BLOBIndex, ( r, v ) => r.Signature = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawEventMap, EventMap>> GetEventMapColumns()
      {
         yield return new ColumnSerializationInfo<RawEventMap, EventMap>( nameof( RawEventMap.Parent ), this.GetSimpleIndex( Tables.TypeDef ), ( r, v ) => r.Parent = v );
         yield return new ColumnSerializationInfo<RawEventMap, EventMap>( nameof( RawEventMap.EventList ), this.GetSimpleIndex( Tables.Event ), ( r, v ) => r.EventList = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawEventDefinitionPointer, EventDefinitionPointer>> GetEventPtrColumns()
      {
         yield return new ColumnSerializationInfo<RawEventDefinitionPointer, EventDefinitionPointer>( nameof( RawEventDefinitionPointer.EventIndex ), this.GetSimpleIndex( Tables.Event ), ( r, v ) => r.EventIndex = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawEventDefinition, EventDefinition>> GetEventDefColumns()
      {
         yield return new ColumnSerializationInfo<RawEventDefinition, EventDefinition>( nameof( RawEventDefinition.Attributes ), this.Constant16, ( r, v ) => r.Attributes = (EventAttributes) v );
         yield return new ColumnSerializationInfo<RawEventDefinition, EventDefinition>( nameof( RawEventDefinition.Name ), this.StringIndex, ( r, v ) => r.Name = v );
         yield return new ColumnSerializationInfo<RawEventDefinition, EventDefinition>( nameof( RawEventDefinition.EventType ), this.TypeDefOrRef, ( r, v ) => r.EventType = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawPropertyMap, PropertyMap>> GetPropertyMapColumns()
      {
         yield return new ColumnSerializationInfo<RawPropertyMap, PropertyMap>( nameof( RawPropertyMap.Parent ), this.GetSimpleIndex( Tables.TypeDef ), ( r, v ) => r.Parent = v );
         yield return new ColumnSerializationInfo<RawPropertyMap, PropertyMap>( nameof( RawPropertyMap.PropertyList ), this.GetSimpleIndex( Tables.Property ), ( r, v ) => r.PropertyList = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawPropertyDefinitionPointer, PropertyDefinitionPointer>> GetPropertyPtrColumns()
      {
         yield return new ColumnSerializationInfo<RawPropertyDefinitionPointer, PropertyDefinitionPointer>( nameof( RawPropertyDefinitionPointer.PropertyIndex ), this.GetSimpleIndex( Tables.Property ), ( r, v ) => r.PropertyIndex = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawPropertyDefinition, PropertyDefinition>> GetPropertyDefColumns()
      {
         yield return new ColumnSerializationInfo<RawPropertyDefinition, PropertyDefinition>( nameof( RawPropertyDefinition.Attributes ), this.Constant16, ( r, v ) => r.Attributes = (PropertyAttributes) v );
         yield return new ColumnSerializationInfo<RawPropertyDefinition, PropertyDefinition>( nameof( RawPropertyDefinition.Name ), this.StringIndex, ( r, v ) => r.Name = v );
         yield return new ColumnSerializationInfo<RawPropertyDefinition, PropertyDefinition>( nameof( RawPropertyDefinition.Signature ), this.BLOBIndex, ( r, v ) => r.Signature = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawMethodSemantics, MethodSemantics>> GetMethodSemanticsColumns()
      {
         yield return new ColumnSerializationInfo<RawMethodSemantics, MethodSemantics>( nameof( RawMethodSemantics.Attributes ), this.Constant16, ( r, v ) => r.Attributes = (MethodSemanticsAttributes) v );
         yield return new ColumnSerializationInfo<RawMethodSemantics, MethodSemantics>( nameof( RawMethodSemantics.Method ), this.GetSimpleIndex( Tables.MethodDef ), ( r, v ) => r.Method = v );
         yield return new ColumnSerializationInfo<RawMethodSemantics, MethodSemantics>( nameof( RawMethodSemantics.Associaton ), this.HasSemantics, ( r, v ) => r.Associaton = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawMethodImplementation, MethodImplementation>> GetMethodImplColumns()
      {
         yield return new ColumnSerializationInfo<RawMethodImplementation, MethodImplementation>( nameof( RawMethodImplementation.Class ), this.GetSimpleIndex( Tables.TypeDef ), ( r, v ) => r.Class = v );
         yield return new ColumnSerializationInfo<RawMethodImplementation, MethodImplementation>( nameof( RawMethodImplementation.MethodBody ), this.MethodDefOrRef, ( r, v ) => r.MethodBody = v );
         yield return new ColumnSerializationInfo<RawMethodImplementation, MethodImplementation>( nameof( RawMethodImplementation.MethodDeclaration ), this.MethodDefOrRef, ( r, v ) => r.MethodDeclaration = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawModuleReference, ModuleReference>> GetModuleRefColumns()
      {
         yield return new ColumnSerializationInfo<RawModuleReference, ModuleReference>( nameof( RawModuleReference.ModuleName ), this.StringIndex, ( r, v ) => r.ModuleName = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawTypeSpecification, TypeSpecification>> GetTypeSpecColumns()
      {
         yield return new ColumnSerializationInfo<RawTypeSpecification, TypeSpecification>( nameof( RawTypeSpecification.Signature ), this.BLOBIndex, ( r, v ) => r.Signature = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawMethodImplementationMap, MethodImplementationMap>> GetImplMapColumns()
      {
         yield return new ColumnSerializationInfo<RawMethodImplementationMap, MethodImplementationMap>( nameof( RawMethodImplementationMap.Attributes ), this.Constant16, ( r, v ) => r.Attributes = (PInvokeAttributes) v );
         yield return new ColumnSerializationInfo<RawMethodImplementationMap, MethodImplementationMap>( nameof( RawMethodImplementationMap.MemberForwarded ), this.MemberForwarded, ( r, v ) => r.MemberForwarded = v );
         yield return new ColumnSerializationInfo<RawMethodImplementationMap, MethodImplementationMap>( nameof( RawMethodImplementationMap.ImportName ), this.StringIndex, ( r, v ) => r.ImportName = v );
         yield return new ColumnSerializationInfo<RawMethodImplementationMap, MethodImplementationMap>( nameof( RawMethodImplementationMap.ImportScope ), this.GetSimpleIndex( Tables.ModuleRef ), ( r, v ) => r.ImportScope = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawFieldRVA, FieldRVA>> GetFieldRVAColumns()
      {
         yield return new ColumnSerializationInfo<RawFieldRVA, FieldRVA>( nameof( RawFieldRVA.RVA ), this.Constant32, ( r, v ) => r.RVA = v );
         yield return new ColumnSerializationInfo<RawFieldRVA, FieldRVA>( nameof( RawFieldRVA.Field ), this.GetSimpleIndex( Tables.Field ), ( r, v ) => r.Field = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawEditAndContinueLog, EditAndContinueLog>> GetENCLogColumns()
      {
         yield return new ColumnSerializationInfo<RawEditAndContinueLog, EditAndContinueLog>( nameof( RawEditAndContinueLog.Token ), this.Constant32, ( r, v ) => r.Token = v );
         yield return new ColumnSerializationInfo<RawEditAndContinueLog, EditAndContinueLog>( nameof( RawEditAndContinueLog.FuncCode ), this.Constant32, ( r, v ) => r.FuncCode = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawEditAndContinueMap, EditAndContinueMap>> GetENCMapColumns()
      {
         yield return new ColumnSerializationInfo<RawEditAndContinueMap, EditAndContinueMap>( nameof( RawEditAndContinueMap.Token ), this.Constant32, ( r, v ) => r.Token = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawAssemblyDefinition, AssemblyDefinition>> GetAssemblyDefColumns()
      {
         yield return new ColumnSerializationInfo<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.HashAlgorithm ), this.Constant32, ( r, v ) => r.HashAlgorithm = (AssemblyHashAlgorithm) v );
         yield return new ColumnSerializationInfo<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.MajorVersion ), this.Constant16, ( r, v ) => r.MajorVersion = v );
         yield return new ColumnSerializationInfo<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.MinorVersion ), this.Constant16, ( r, v ) => r.MinorVersion = v );
         yield return new ColumnSerializationInfo<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.BuildNumber ), this.Constant16, ( r, v ) => r.BuildNumber = v );
         yield return new ColumnSerializationInfo<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.RevisionNumber ), this.Constant16, ( r, v ) => r.RevisionNumber = v );
         yield return new ColumnSerializationInfo<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.Attributes ), this.Constant32, ( r, v ) => r.Attributes = (AssemblyFlags) v );
         yield return new ColumnSerializationInfo<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.PublicKey ), this.BLOBIndex, ( r, v ) => r.PublicKey = v );
         yield return new ColumnSerializationInfo<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.Name ), this.StringIndex, ( r, v ) => r.Name = v );
         yield return new ColumnSerializationInfo<RawAssemblyDefinition, AssemblyDefinition>( nameof( RawAssemblyDefinition.Culture ), this.StringIndex, ( r, v ) => r.Culture = v );
      }
#pragma warning disable 618
      protected virtual IEnumerable<ColumnSerializationInfo<RawAssemblyDefinitionProcessor, AssemblyDefinitionProcessor>> GetAssemblyDefProcessorColumns()
      {
         yield return new ColumnSerializationInfo<RawAssemblyDefinitionProcessor, AssemblyDefinitionProcessor>( nameof( RawAssemblyDefinitionProcessor.Processor ), this.Constant32, ( r, v ) => r.Processor = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawAssemblyDefinitionOS, AssemblyDefinitionOS>> GetAssemblyDefOSColumns()
      {
         yield return new ColumnSerializationInfo<RawAssemblyDefinitionOS, AssemblyDefinitionOS>( nameof( RawAssemblyDefinitionOS.OSPlatformID ), this.Constant32, ( r, v ) => r.OSPlatformID = v );
         yield return new ColumnSerializationInfo<RawAssemblyDefinitionOS, AssemblyDefinitionOS>( nameof( RawAssemblyDefinitionOS.OSMajorVersion ), this.Constant32, ( r, v ) => r.OSMajorVersion = v );
         yield return new ColumnSerializationInfo<RawAssemblyDefinitionOS, AssemblyDefinitionOS>( nameof( RawAssemblyDefinitionOS.OSMinorVersion ), this.Constant32, ( r, v ) => r.OSMinorVersion = v );
      }
#pragma warning restore 618

      protected virtual IEnumerable<ColumnSerializationInfo<RawAssemblyReference, AssemblyReference>> GetAssemblyRefColumns()
      {
         yield return new ColumnSerializationInfo<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.MajorVersion ), this.Constant16, ( r, v ) => r.MajorVersion = v );
         yield return new ColumnSerializationInfo<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.MinorVersion ), this.Constant16, ( r, v ) => r.MinorVersion = v );
         yield return new ColumnSerializationInfo<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.BuildNumber ), this.Constant16, ( r, v ) => r.BuildNumber = v );
         yield return new ColumnSerializationInfo<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.RevisionNumber ), this.Constant16, ( r, v ) => r.RevisionNumber = v );
         yield return new ColumnSerializationInfo<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.Attributes ), this.Constant32, ( r, v ) => r.Attributes = (AssemblyFlags) v );
         yield return new ColumnSerializationInfo<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.PublicKeyOrToken ), this.BLOBIndex, ( r, v ) => r.PublicKeyOrToken = v );
         yield return new ColumnSerializationInfo<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.Name ), this.StringIndex, ( r, v ) => r.Name = v );
         yield return new ColumnSerializationInfo<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.Culture ), this.StringIndex, ( r, v ) => r.Culture = v );
         yield return new ColumnSerializationInfo<RawAssemblyReference, AssemblyReference>( nameof( RawAssemblyReference.HashValue ), this.BLOBIndex, ( r, v ) => r.HashValue = v );
      }

#pragma warning disable 618
      protected virtual IEnumerable<ColumnSerializationInfo<RawAssemblyReferenceProcessor, AssemblyReferenceProcessor>> GetAssemblyRefProcessorColumns()
      {
         yield return new ColumnSerializationInfo<RawAssemblyReferenceProcessor, AssemblyReferenceProcessor>( nameof( RawAssemblyReferenceProcessor.Processor ), this.Constant32, ( r, v ) => r.Processor = v );
         yield return new ColumnSerializationInfo<RawAssemblyReferenceProcessor, AssemblyReferenceProcessor>( nameof( RawAssemblyReferenceProcessor.AssemblyRef ), this.GetSimpleIndex( Tables.AssemblyRef ), ( r, v ) => r.AssemblyRef = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawAssemblyReferenceOS, AssemblyReferenceOS>> GetAssemblyRefOSColumns()
      {
         yield return new ColumnSerializationInfo<RawAssemblyReferenceOS, AssemblyReferenceOS>( nameof( RawAssemblyReferenceOS.OSPlatformID ), this.Constant32, ( r, v ) => r.OSPlatformID = v );
         yield return new ColumnSerializationInfo<RawAssemblyReferenceOS, AssemblyReferenceOS>( nameof( RawAssemblyReferenceOS.OSMajorVersion ), this.Constant32, ( r, v ) => r.OSMajorVersion = v );
         yield return new ColumnSerializationInfo<RawAssemblyReferenceOS, AssemblyReferenceOS>( nameof( RawAssemblyReferenceOS.OSMinorVersion ), this.Constant32, ( r, v ) => r.OSMinorVersion = v );
         yield return new ColumnSerializationInfo<RawAssemblyReferenceOS, AssemblyReferenceOS>( nameof( RawAssemblyReferenceOS.AssemblyRef ), this.GetSimpleIndex( Tables.AssemblyRef ), ( r, v ) => r.AssemblyRef = v );
      }
#pragma warning restore 618

      protected virtual IEnumerable<ColumnSerializationInfo<RawFileReference, FileReference>> GetFileColumns()
      {
         yield return new ColumnSerializationInfo<RawFileReference, FileReference>( nameof( RawFileReference.Attributes ), this.Constant32, ( r, v ) => r.Attributes = (FileAttributes) v );
         yield return new ColumnSerializationInfo<RawFileReference, FileReference>( nameof( RawFileReference.Name ), this.StringIndex, ( r, v ) => r.Name = v );
         yield return new ColumnSerializationInfo<RawFileReference, FileReference>( nameof( RawFileReference.HashValue ), this.BLOBIndex, ( r, v ) => r.HashValue = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawExportedType, ExportedType>> GetExportedTypeColumns()
      {
         yield return new ColumnSerializationInfo<RawExportedType, ExportedType>( nameof( RawExportedType.Attributes ), this.Constant32, ( r, v ) => r.Attributes = (TypeAttributes) v );
         yield return new ColumnSerializationInfo<RawExportedType, ExportedType>( nameof( RawExportedType.TypeDefinitionIndex ), this.Constant32, ( r, v ) => r.TypeDefinitionIndex = v );
         yield return new ColumnSerializationInfo<RawExportedType, ExportedType>( nameof( RawExportedType.Name ), this.StringIndex, ( r, v ) => r.Name = v );
         yield return new ColumnSerializationInfo<RawExportedType, ExportedType>( nameof( RawExportedType.Namespace ), this.StringIndex, ( r, v ) => r.Namespace = v );
         yield return new ColumnSerializationInfo<RawExportedType, ExportedType>( nameof( RawExportedType.Implementation ), this.Implementation, ( r, v ) => r.Implementation = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawManifestResource, ManifestResource>> GetManifestResourceColumns()
      {
         yield return new ColumnSerializationInfo<RawManifestResource, ManifestResource>( nameof( RawManifestResource.Offset ), this.Constant32, ( r, v ) => r.Offset = v );
         yield return new ColumnSerializationInfo<RawManifestResource, ManifestResource>( nameof( RawManifestResource.Attributes ), this.Constant32, ( r, v ) => r.Attributes = (ManifestResourceAttributes) v );
         yield return new ColumnSerializationInfo<RawManifestResource, ManifestResource>( nameof( RawManifestResource.Name ), this.StringIndex, ( r, v ) => r.Name = v );
         yield return new ColumnSerializationInfo<RawManifestResource, ManifestResource>( nameof( RawManifestResource.Implementation ), this.ImplementationNullable, ( r, v ) => r.Implementation = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawNestedClassDefinition, NestedClassDefinition>> GetNestedClassColumns()
      {
         yield return new ColumnSerializationInfo<RawNestedClassDefinition, NestedClassDefinition>( nameof( RawNestedClassDefinition.NestedClass ), this.GetSimpleIndex( Tables.TypeDef ), ( r, v ) => r.NestedClass = v );
         yield return new ColumnSerializationInfo<RawNestedClassDefinition, NestedClassDefinition>( nameof( RawNestedClassDefinition.EnclosingClass ), this.GetSimpleIndex( Tables.TypeDef ), ( r, v ) => r.EnclosingClass = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawGenericParameterDefinition, GenericParameterDefinition>> GetGenericParamColumns()
      {
         yield return new ColumnSerializationInfo<RawGenericParameterDefinition, GenericParameterDefinition>( nameof( RawGenericParameterDefinition.GenericParameterIndex ), this.Constant16, ( r, v ) => r.GenericParameterIndex = v );
         yield return new ColumnSerializationInfo<RawGenericParameterDefinition, GenericParameterDefinition>( nameof( RawGenericParameterDefinition.Attributes ), this.Constant16, ( r, v ) => r.Attributes = (GenericParameterAttributes) v );
         yield return new ColumnSerializationInfo<RawGenericParameterDefinition, GenericParameterDefinition>( nameof( RawGenericParameterDefinition.Owner ), this.TypeOrMethodDef, ( r, v ) => r.Owner = v );
         yield return new ColumnSerializationInfo<RawGenericParameterDefinition, GenericParameterDefinition>( nameof( RawGenericParameterDefinition.Name ), this.StringIndex, ( r, v ) => r.Name = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawMethodSpecification, MethodSpecification>> GetMethodSpecColumns()
      {
         yield return new ColumnSerializationInfo<RawMethodSpecification, MethodSpecification>( nameof( RawMethodSpecification.Method ), this.MethodDefOrRef, ( r, v ) => r.Method = v );
         yield return new ColumnSerializationInfo<RawMethodSpecification, MethodSpecification>( nameof( RawMethodSpecification.Signature ), this.BLOBIndex, ( r, v ) => r.Signature = v );
      }

      protected virtual IEnumerable<ColumnSerializationInfo<RawGenericParameterConstraintDefinition, GenericParameterConstraintDefinition>> GetGenericParamConstraintColumns()
      {
         yield return new ColumnSerializationInfo<RawGenericParameterConstraintDefinition, GenericParameterConstraintDefinition>( nameof( RawGenericParameterConstraintDefinition.Owner ), this.GetSimpleIndex( Tables.GenericParameter ), ( r, v ) => r.Owner = v );
         yield return new ColumnSerializationInfo<RawGenericParameterConstraintDefinition, GenericParameterConstraintDefinition>( nameof( RawGenericParameterConstraintDefinition.Constraint ), this.TypeDefOrRef, ( r, v ) => r.Constraint = v );
      }

      protected ColumnSerializationSupport Constant8 { get; }

      protected ColumnSerializationSupport Constant16 { get; }

      protected ColumnSerializationSupport Constant32 { get; }


      protected ColumnSerializationSupport BLOBIndex { get; }

      protected ColumnSerializationSupport GUIDIndex { get; }

      protected ColumnSerializationSupport StringIndex { get; }


      protected ColumnSerializationSupport_CodedTableIndex TypeDefOrRef { get; }

      protected ColumnSerializationSupport_CodedTableIndex TypeDefOrRefNullable { get; }

      protected ColumnSerializationSupport_CodedTableIndex HasConstant { get; }

      protected ColumnSerializationSupport_CodedTableIndex HasCustomAttribute { get; }

      protected ColumnSerializationSupport_CodedTableIndex HasFieldMarshal { get; }

      protected ColumnSerializationSupport_CodedTableIndex HasSecurity { get; }

      protected ColumnSerializationSupport_CodedTableIndex MemberRefParent { get; }

      protected ColumnSerializationSupport_CodedTableIndex HasSemantics { get; }

      protected ColumnSerializationSupport_CodedTableIndex MethodDefOrRef { get; }

      protected ColumnSerializationSupport_CodedTableIndex MemberForwarded { get; }

      protected ColumnSerializationSupport_CodedTableIndex Implementation { get; }

      protected ColumnSerializationSupport_CodedTableIndex ImplementationNullable { get; }

      protected ColumnSerializationSupport_CodedTableIndex CustomAttributeType { get; }

      protected ColumnSerializationSupport_CodedTableIndex ResolutionScopeNullable { get; }

      protected ColumnSerializationSupport_CodedTableIndex TypeOrMethodDef { get; }

      protected Byte[] ReadBLOBArray( Int32 value, ReaderBLOBStreamHandler blobs )
      {
         return blobs.GetBLOB( value );
      }

      protected TSignature ReadBLOBSignature<TSignature>( Int32 value, ReaderBLOBStreamHandler blobs )
         where TSignature : AbstractSignature
      {
         return blobs.ReadSignature( value ) as TSignature;
      }

      protected Guid? ReadGUID( Int32 value, ReaderGUIDStreamHandler guids )
      {
         return guids.GetGUID( value );
      }

      protected String ReadString( Int32 value, ReaderStringStreamHandler strings )
      {
         return strings.GetString( value );
      }

      protected TableIndex ReadSimpleIndex( Int32 value, Tables table )
      {
         return new TableIndex( table, Math.Min( 0, value - 1 ) );
      }


      protected ColumnSerializationSupport GetSimpleIndex( Tables targetTable )
      {
         return this._simpleIndices[(Int32) targetTable];
      }

      private ColumnSerializationSupport NewSimpleIndex( Tables targetTable )
      {
         return this.TableSizes[(Int32) targetTable] > UInt16.MaxValue ?
            (ColumnSerializationSupport) new ColumnSerializationSupport_SimpleTableIndex32() :
            new ColumnSerializationSupport_SimpleTableIndex16();
      }

      protected ColumnSerializationSupport_CodedTableIndex CodedIndexNullable( ArrayQuery<Tables?> tables )
      {
         return ColumnSerializationSupport_CodedTableIndex.GetCodedTableSize( this.TableSizes, tables ) < 4 ?
            (ColumnSerializationSupport_CodedTableIndex) new ColumnSerializationSupport_CodedTableIndexNullable16( tables ) :
            new ColumnSerializationSupport_CodedTableIndexNullable32( tables );
      }

      protected ColumnSerializationSupport_CodedTableIndex CodedIndex( ArrayQuery<Tables?> tables )
      {
         return ColumnSerializationSupport_CodedTableIndex.GetCodedTableSize( this.TableSizes, tables ) < 4 ?
            (ColumnSerializationSupport_CodedTableIndex) new ColumnSerializationSupport_CodedTableIndex16( tables ) :
            new ColumnSerializationSupport_CodedTableIndex32( tables );
      }

      protected virtual TableSerializationSupport CreateTableSupport<TRawRow, TRow>( Tables table, IEnumerable<ColumnSerializationInfo<TRawRow, TRow>> columns )
         where TRawRow : class, new()
         where TRow : class, new()
      {
         return new DefaultTableSerializationSupport<TRawRow, TRow>(
            table,
            columns
            );
      }
   }

   public class DefaultTableSerializationSupport<TRawRow, TRow> : TableSerializationSupport
      where TRawRow : class, new()
      where TRow : class, new()
   {

      private readonly ColumnSerializationInfo<TRawRow, TRow>[] _columnArray;
      public DefaultTableSerializationSupport(
         Tables table,
         IEnumerable<ColumnSerializationInfo<TRawRow, TRow>> columns
         )
      {
         ArgumentValidator.ValidateNotNull( "Columns", columns );

         this.Table = table;
         this.ColumnSerializationSupports = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewArrayProxy(
            columns.Cast<ColumnSerializationInfo>().ToArray()
            ).CQ;
         this._columnArray = columns.ToArray();
      }

      public Tables Table { get; }

      public ArrayQuery<ColumnSerializationInfo> ColumnSerializationSupports { get; }

      public Object ReadRow(
         StreamHelper stream,
         ReaderBLOBStreamHandler blobs,
         ReaderGUIDStreamHandler guids,
         ReaderStringStreamHandler sysStrings
         )
      {
         var row = new TRow();
         foreach ( var serialization in this._columnArray )
         {
            serialization.Setter( row, serialization.Serialization.ReadRawValue( stream ), blobs, guids, sysStrings );
         }
         return row;
      }

      public Object ReadRawRow( StreamHelper stream )
      {
         var row = new TRawRow();
         foreach ( var serialization in this._columnArray )
         {
            serialization.RawSetter( row, serialization.Serialization.ReadRawValue( stream ) );
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

      public Object ReadValue(
         StreamHelper stream,
         ReaderBLOBStreamHandler blobs,
         ReaderGUIDStreamHandler guids,
         ReaderStringStreamHandler sysStrings
         )
      {
         return this.ReadRawValue( stream );
      }

      public void WriteValue( StreamHelper stream, Object value )
      {
         stream.WriteByteToBytes( Convert.ToByte( value ) );
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

      public Object ReadValue(
         StreamHelper stream,
         ReaderBLOBStreamHandler blobs,
         ReaderGUIDStreamHandler guids,
         ReaderStringStreamHandler sysStrings
         )
      {
         return this.ReadRawValue( stream );
      }

      public void WriteValue( StreamHelper stream, Object value )
      {
         stream.WriteInt16LEToBytes( Convert.ToInt16( value ) );
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

      public Object ReadValue(
         StreamHelper stream,
         ReaderBLOBStreamHandler blobs,
         ReaderGUIDStreamHandler guids,
         ReaderStringStreamHandler sysStrings
         )
      {
         return this.ReadRawValue( stream );
      }

      public void WriteValue( StreamHelper stream, Object value )
      {
         stream.WriteInt32LEToBytes( Convert.ToInt32( value ) );
      }
   }

   public sealed class ColumnSerializationSupport_SimpleTableIndex16 : ColumnSerializationSupport
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

      public Object ReadValue(
         StreamHelper stream,
         ReaderBLOBStreamHandler blobs,
         ReaderGUIDStreamHandler guids,
         ReaderStringStreamHandler sysStrings
         )
      {
         return this.ReadRawValue( stream );
      }

      public void WriteValue( StreamHelper stream, Object value )
      {
         stream.WriteUInt16LEToBytes( (UInt16) ( (TableIndex) value ).Index );
      }
   }

   public sealed class ColumnSerializationSupport_SimpleTableIndex32 : ColumnSerializationSupport
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

      public Object ReadValue(
         StreamHelper stream,
         ReaderBLOBStreamHandler blobs,
         ReaderGUIDStreamHandler guids,
         ReaderStringStreamHandler sysStrings
         )
      {
         return this.ReadRawValue( stream );
      }

      public void WriteValue( StreamHelper stream, Object value )
      {
         stream.WriteInt32LEToBytes( ( (TableIndex) value ).Index );
      }
   }

   public abstract class ColumnSerializationSupport_CodedTableIndex : ColumnSerializationSupport
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
         return max < ( UInt16.MaxValue >> GetTagBitSize( referencedTables.Count ) ) ?
            2 :
            4;
      }

      private static Int32 GetTagBitSize( Int32 referencedTablesLength )
      {
         return BinaryUtils.Log2( (UInt32) referencedTablesLength );
      }


      private readonly ArrayQuery<Tables?> _tablesArray;
      private readonly IDictionary<Tables, Int32> _tablesDictionary;
      private readonly Int32 _tagBitMask;
      private readonly Int32 _tagBitSize;

      protected ColumnSerializationSupport_CodedTableIndex(
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

      // TODO CWR extension method
      //private static Int32 IndexOf<T>( ArrayQuery<T> array, T value )
      //{
      //   var max = array.Count;
      //   var retVal = -1;
      //   for ( var i = 0; i < max; ++i )
      //   {
      //      if ( Equals( array[i], value ) )
      //      {
      //         retVal = i;
      //         break;
      //      }
      //   }
      //   return retVal;
      //}

      public abstract Int32 ColumnByteCount { get; }

      public abstract Int32 ReadRawValue( StreamHelper stream );

      public abstract Object ReadValue(
         StreamHelper stream,
         ReaderBLOBStreamHandler blobs,
         ReaderGUIDStreamHandler guids,
         ReaderStringStreamHandler sysStrings
         );

      public abstract void WriteValue( StreamHelper stream, Object value );
   }

   public sealed class ColumnSerializationSupport_CodedTableIndexNullable16 : ColumnSerializationSupport_CodedTableIndex
   {

      public override Int32 ColumnByteCount
      {
         get
         {
            return sizeof( UInt16 );
         }
      }

      public ColumnSerializationSupport_CodedTableIndexNullable16( ArrayQuery<Tables?> tables )
         : base( tables )
      {

      }

      public override Int32 ReadRawValue( StreamHelper stream )
      {
         return stream.ReadUInt16LEFromBytes();
      }

      public override Object ReadValue(
         StreamHelper stream,
         ReaderBLOBStreamHandler blobs,
         ReaderGUIDStreamHandler guids,
         ReaderStringStreamHandler sysStrings
         )
      {
         return this.DecodeTableIndex( this.ReadRawValue( stream ) );
      }

      public override void WriteValue( StreamHelper stream, Object value )
      {
         stream.WriteUInt16LEToBytes( (UInt16) this.EncodeTableIndex( (TableIndex?) value ) );
      }
   }

   public sealed class ColumnSerializationSupport_CodedTableIndexNullable32 : ColumnSerializationSupport_CodedTableIndex
   {
      public override Int32 ColumnByteCount
      {
         get
         {
            return sizeof( Int32 );
         }
      }

      public ColumnSerializationSupport_CodedTableIndexNullable32( ArrayQuery<Tables?> tables )
         : base( tables )
      {

      }

      public override Int32 ReadRawValue( StreamHelper stream )
      {
         return stream.ReadInt32LEFromBytes();
      }

      public override Object ReadValue(
         StreamHelper stream,
         ReaderBLOBStreamHandler blobs,
         ReaderGUIDStreamHandler guids,
         ReaderStringStreamHandler sysStrings
         )
      {
         return this.DecodeTableIndex( this.ReadRawValue( stream ) );
      }

      public override void WriteValue( StreamHelper stream, Object value )
      {
         stream.WriteInt32LEToBytes( this.EncodeTableIndex( (TableIndex?) value ) );
      }
   }

   public sealed class ColumnSerializationSupport_CodedTableIndex16 : ColumnSerializationSupport_CodedTableIndex
   {

      public override Int32 ColumnByteCount
      {
         get
         {
            return sizeof( UInt16 );
         }
      }

      public ColumnSerializationSupport_CodedTableIndex16( ArrayQuery<Tables?> tables )
         : base( tables )
      {

      }

      public override Int32 ReadRawValue( StreamHelper stream )
      {
         return stream.ReadUInt16LEFromBytes();
      }

      public override Object ReadValue(
         StreamHelper stream,
         ReaderBLOBStreamHandler blobs,
         ReaderGUIDStreamHandler guids,
         ReaderStringStreamHandler sysStrings
         )
      {
         return this.DecodeTableIndex( this.ReadRawValue( stream ) ).GetValueOrDefault();
      }

      public override void WriteValue( StreamHelper stream, Object value )
      {
         stream.WriteUInt16LEToBytes( (UInt16) this.EncodeTableIndex( (TableIndex) value ) );
      }
   }

   public sealed class ColumnSerializationSupport_CodedTableIndex32 : ColumnSerializationSupport_CodedTableIndex
   {
      public override Int32 ColumnByteCount
      {
         get
         {
            return sizeof( Int32 );
         }
      }

      public ColumnSerializationSupport_CodedTableIndex32( ArrayQuery<Tables?> tables )
         : base( tables )
      {

      }

      public override Int32 ReadRawValue( StreamHelper stream )
      {
         return stream.ReadInt32LEFromBytes();
      }

      public override Object ReadValue(
         StreamHelper stream,
         ReaderBLOBStreamHandler blobs,
         ReaderGUIDStreamHandler guids,
         ReaderStringStreamHandler sysStrings
         )
      {
         return this.DecodeTableIndex( this.ReadRawValue( stream ) ).GetValueOrDefault();
      }

      public override void WriteValue( StreamHelper stream, Object value )
      {
         stream.WriteInt32LEToBytes( this.EncodeTableIndex( (TableIndex) value ) );
      }
   }
}
