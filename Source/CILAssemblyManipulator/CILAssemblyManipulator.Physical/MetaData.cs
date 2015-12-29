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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommonUtils;
using CILAssemblyManipulator.Physical;

using TSigInfo = System.Tuple<System.Object, CILAssemblyManipulator.Physical.TableIndex>;


namespace CILAssemblyManipulator.Physical
{
   /// <summary>
   /// This interface represents a single metadata instance.
   /// It is composed of metadata tables.
   /// The instances of this interface may be created via static methods of <see cref="CILMetaDataFactory" />.
   /// </summary>
   /// <remarks>
   /// This interface does not enforce any of the integrity and consistency rules of serialized and loadable metadata files.
   /// </remarks>
   /// <seealso cref="CILMetaDataFactory"/>
   /// <seealso cref="MetaDataTable"/>
   public interface CILMetaData : CILMetaDataBase
   {
      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.Module"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.Module"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="ModuleDefinition"/>
      MetaDataTable<ModuleDefinition> ModuleDefinitions { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.TypeRef"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.TypeRef"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="TypeReference"/>
      MetaDataTable<TypeReference> TypeReferences { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.TypeDef"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.TypeDef"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="TypeDefinition"/>
      MetaDataTable<TypeDefinition> TypeDefinitions { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.Field"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.Field"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="FieldDefinition"/>
      MetaDataTable<FieldDefinition> FieldDefinitions { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.MethodDef"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.MethodDef"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="MethodDefinition"/>
      MetaDataTable<MethodDefinition> MethodDefinitions { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.Parameter"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.Parameter"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="ParameterDefinition"/>
      MetaDataTable<ParameterDefinition> ParameterDefinitions { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.InterfaceImpl"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.InterfaceImpl"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="InterfaceImplementation"/>
      MetaDataTable<InterfaceImplementation> InterfaceImplementations { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.MemberRef"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.MemberRef"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="MemberReference"/>
      MetaDataTable<MemberReference> MemberReferences { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.Constant"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.Constant"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="ConstantDefinition"/>
      MetaDataTable<ConstantDefinition> ConstantDefinitions { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.CustomAttribute"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.CustomAttribute"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="CustomAttributeDefinition"/>
      MetaDataTable<CustomAttributeDefinition> CustomAttributeDefinitions { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.FieldMarshal"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.FieldMarshal"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="FieldMarshal"/>
      MetaDataTable<FieldMarshal> FieldMarshals { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.DeclSecurity"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.DeclSecurity"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="SecurityDefinition"/>
      MetaDataTable<SecurityDefinition> SecurityDefinitions { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.ClassLayout"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.ClassLayout"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="ClassLayout"/>
      MetaDataTable<ClassLayout> ClassLayouts { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.FieldLayout"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.FieldLayout"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="ModuleDefinition"/>
      MetaDataTable<FieldLayout> FieldLayouts { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.StandaloneSignature"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.StandaloneSignature"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="StandaloneSignature"/>
      MetaDataTable<StandaloneSignature> StandaloneSignatures { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.EventMap"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.EventMap"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="ModuleDefinition"/>
      MetaDataTable<EventMap> EventMaps { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.Event"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.Event"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="EventDefinition"/>
      MetaDataTable<EventDefinition> EventDefinitions { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.PropertyMap"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.PropertyMap"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="ModuleDefinition"/>
      MetaDataTable<PropertyMap> PropertyMaps { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.Property"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.Property"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="PropertyDefinition"/>
      MetaDataTable<PropertyDefinition> PropertyDefinitions { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.MethodSemantics"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.MethodSemantics"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="MethodSemantics"/>
      MetaDataTable<MethodSemantics> MethodSemantics { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.MethodImpl"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.MethodImpl"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="MethodImplementation"/>
      MetaDataTable<MethodImplementation> MethodImplementations { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.ModuleRef"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.ModuleRef"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="ModuleReference"/>
      MetaDataTable<ModuleReference> ModuleReferences { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.TypeSpec"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.TypeSpec"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="TypeSpecification"/>
      MetaDataTable<TypeSpecification> TypeSpecifications { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.ImplMap"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.ImplMap"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="MethodImplementationMap"/>
      MetaDataTable<MethodImplementationMap> MethodImplementationMaps { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.FieldRVA"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.FieldRVA"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="FieldRVA"/>
      MetaDataTable<FieldRVA> FieldRVAs { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.Assembly"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.Assembly"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="AssemblyDefinition"/>
      MetaDataTable<AssemblyDefinition> AssemblyDefinitions { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.AssemblyRef"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.AssemblyRef"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="AssemblyReference"/>
      MetaDataTable<AssemblyReference> AssemblyReferences { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.File"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.File"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="FileReference"/>
      MetaDataTable<FileReference> FileReferences { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.ExportedType"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.ExportedType"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="ExportedType"/>
      MetaDataTable<ExportedType> ExportedTypes { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.ManifestResource"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.ManifestResource"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="ManifestResource"/>
      MetaDataTable<ManifestResource> ManifestResources { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.NestedClass"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.NestedClass"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="NestedClassDefinition"/>
      MetaDataTable<NestedClassDefinition> NestedClassDefinitions { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.GenericParameter"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.GenericParameter"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="GenericParameterDefinition"/>
      MetaDataTable<GenericParameterDefinition> GenericParameterDefinitions { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.MethodSpec"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.MethodSpec"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="MethodSpecification"/>
      MetaDataTable<MethodSpecification> MethodSpecifications { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.GenericParameterConstraint"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.GenericParameterConstraint"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="GenericParameterConstraintDefinition"/>
      MetaDataTable<GenericParameterConstraintDefinition> GenericParameterConstraintDefinitions { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.EncLog"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.EncLog"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="EditAndContinueLog"/>
      MetaDataTable<EditAndContinueLog> EditAndContinueLog { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.EncMap"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.EncMap"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="EditAndContinueMap"/>
      MetaDataTable<EditAndContinueMap> EditAndContinueMap { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.FieldPtr"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.FieldPtr"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="FieldDefinitionPointer"/>
      MetaDataTable<FieldDefinitionPointer> FieldDefinitionPointers { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.MethodPtr"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.MethodPtr"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="MethodDefinitionPointer"/>
      MetaDataTable<MethodDefinitionPointer> MethodDefinitionPointers { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.ParameterPtr"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.ParameterPtr"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="ParameterDefinitionPointer"/>
      MetaDataTable<ParameterDefinitionPointer> ParameterDefinitionPointers { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.EventPtr"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.EventPtr"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="EventDefinitionPointer"/>
      MetaDataTable<EventDefinitionPointer> EventDefinitionPointers { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.PropertyPtr"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.PropertyPtr"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="PropertyDefinitionPointer"/>
      MetaDataTable<PropertyDefinitionPointer> PropertyDefinitionPointers { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.AssemblyProcessor"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.AssemblyProcessor"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="AssemblyDefinitionProcessor"/>
      [Obsolete( "This table should not be used anymore.", false )]
      MetaDataTable<AssemblyDefinitionProcessor> AssemblyDefinitionProcessors { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.AssemblyOS"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.AssemblyOS"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="AssemblyDefinitionOS"/>
      [Obsolete( "This table should not be used anymore.", false )]
      MetaDataTable<AssemblyDefinitionOS> AssemblyDefinitionOSs { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.AssemblyRefProcessor"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.AssemblyRefProcessor"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="AssemblyReferenceProcessor"/>
      [Obsolete( "This table should not be used anymore.", false )]
      MetaDataTable<AssemblyReferenceProcessor> AssemblyReferenceProcessors { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.AssemblyRefOS"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.AssemblyRefOS"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="AssemblyReferenceOS"/>
      [Obsolete( "This table should not be used anymore.", false )]
      MetaDataTable<AssemblyReferenceOS> AssemblyReferenceOSs { get; }
   }


   public static class CILMetaDataFactory
   {

      public static CILMetaData NewBlankMetaData( Int32[] sizes = null, Meta.MetaDataTableInformationProvider tableInfoProvider = null )
      {
         Meta.MetaDataTableInformation[] infos;
         return new CILAssemblyManipulator.Physical.Implementation.CILMetadataImpl( tableInfoProvider, sizes, out infos );
      }

      public static CILMetaData CreateMinimalAssembly( String assemblyName, String moduleName, Boolean createModuleType = true, Meta.MetaDataTableInformationProvider tableInfoProvider = null )
      {
         if ( !String.IsNullOrEmpty( assemblyName ) && String.IsNullOrEmpty( moduleName ) )
         {
            moduleName = assemblyName + ".dll";
         }

         var md = CreateMinimalModule( moduleName, createModuleType, tableInfoProvider );

         var aDef = new AssemblyDefinition();
         aDef.AssemblyInformation.Name = assemblyName;
         md.AssemblyDefinitions.TableContents.Add( aDef );

         return md;
      }

      public static CILMetaData CreateMinimalModule( String moduleName, Boolean createModuleType = true, Meta.MetaDataTableInformationProvider tableInfoProvider = null )
      {
         var md = CILMetaDataFactory.NewBlankMetaData( tableInfoProvider: tableInfoProvider );

         // Module definition
         md.ModuleDefinitions.TableContents.Add( new ModuleDefinition()
         {
            Name = moduleName,
            ModuleGUID = Guid.NewGuid()
         } );

         if ( createModuleType )
         {
            // Module type
            md.TypeDefinitions.TableContents.Add( new TypeDefinition()
            {
               Name = Miscellaneous.MODULE_TYPE_NAME
            } );
         }

         return md;
      }
   }
}


public static partial class E_CILPhysical
{
   private sealed class StackCalculationState
   {
      private Int32 _maxStack;
      private readonly Int32[] _stackSizes;
      private readonly CILMetaData _md;

      internal StackCalculationState( CILMetaData md, Int32 ilByteCount )
      {
         this._md = md;
         this._stackSizes = new Int32[ilByteCount];
         this._maxStack = 0;
      }

      public Int32 CurrentStack { get; set; }
      public Int32 CurrentCodeByteOffset { get; set; }
      public Int32 NextCodeByteOffset { get; set; }
      public Int32 MaxStack
      {
         get
         {
            return this._maxStack;
         }
      }
      public CILMetaData MD
      {
         get
         {
            return this._md;
         }
      }
      public Int32[] StackSizes
      {
         get
         {
            return this._stackSizes;
         }
      }

      public void UpdateMaxStack( Int32 newMaxStack )
      {
         if ( this._maxStack < newMaxStack )
         {
            this._maxStack = newMaxStack;
         }
      }
   }

   private sealed class MetaDataReOrderState
   {
      private readonly CILMetaData _md;
      private readonly IDictionary<Tables, IDictionary<Int32, Int32>> _duplicates;
      private readonly Int32[][] _finalIndices;

      internal MetaDataReOrderState( CILMetaData md )
      {
         this._md = md;
         this._duplicates = new Dictionary<Tables, IDictionary<Int32, Int32>>();
         this._finalIndices = new Int32[Consts.AMOUNT_OF_TABLES][];
      }

      public CILMetaData MetaData
      {
         get
         {
            return this._md;
         }
      }

      public IDictionary<Tables, IDictionary<Int32, Int32>> Duplicates
      {
         get
         {
            return this._duplicates;
         }
      }

      public Int32[][] FinalIndices
      {
         get
         {
            return this._finalIndices;
         }
      }

      public void MarkDuplicate( Int32 tableIndex, Int32 duplicateIdx, Int32 actualIndex )
      {
         var table = (Tables) tableIndex;
         var thisDuplicates = this._duplicates
            .GetOrAdd_NotThreadSafe( table, t => new Dictionary<Int32, Int32>() );
         thisDuplicates
            .Add( duplicateIdx, actualIndex );
         // Update all other duplicates as well
         foreach ( var kvp in thisDuplicates.ToArray() )
         {
            if ( kvp.Value == duplicateIdx )
            {
               thisDuplicates.Remove( kvp.Key );
               thisDuplicates.Add( kvp.Key, actualIndex );
            }
         }
      }

      public Boolean IsDuplicate( TableIndex index )
      {
         IDictionary<Int32, Int32> tableDuplicates;
         return this._duplicates.TryGetValue( index.Table, out tableDuplicates )
            && tableDuplicates.ContainsKey( index.Index );
      }

      public Boolean IsDuplicate( TableIndex index, out Int32 newIndex )
      {
         IDictionary<Int32, Int32> tableDuplicates;
         newIndex = -1;
         var retVal = this._duplicates.TryGetValue( index.Table, out tableDuplicates )
            && tableDuplicates.TryGetValue( index.Index, out newIndex );
         if ( !retVal )
         {
            newIndex = -1;
         }
         return retVal;
      }

      public Int32[] GetOrCreateIndexArray<T>( MetaDataTable<T> table )
         where T : class
      {
         var tIdx = (Int32) table.TableKind;
         var retVal = this._finalIndices[tIdx];
         if ( retVal == null )
         {
            var list = table.TableContents;
            retVal = new Int32[list.Count];
            for ( var i = 0; i < retVal.Length; ++i )
            {
               retVal[i] = i;
            }
            this._finalIndices[tIdx] = retVal;
         }
         return retVal;
      }

      public Int32 GetFinalIndex( TableIndex index )
      {
         return this._finalIndices[(Int32) index.Table][index.Index];
      }

      public Int32 GetFinalIndex( Tables table, Int32 index )
      {
         return this._finalIndices[(Int32) table][index];
      }
   }

   public static LocalVariablesSignature GetLocalsSignatureForMethodOrNull( this CILMetaData md, Int32 methodDefIndex )
   {
      var method = md.MethodDefinitions.GetOrNull( methodDefIndex );
      LocalVariablesSignature retVal;
      if ( method == null || method.IL == null )
      {
         retVal = null;
      }
      else
      {
         var il = method.IL;
         var tIdx = il.LocalsSignatureIndex;
         if ( tIdx.HasValue )
         {
            var idx = tIdx.Value.Index;
            var list = md.StandaloneSignatures.TableContents;
            retVal = idx >= 0 && idx < list.Count ?
               list[idx].Signature as LocalVariablesSignature :
               null;
         }
         else
         {
            retVal = null;
         }
      }

      return retVal;
   }

   public static IEnumerable<MethodDefinition> GetTypeMethods( this CILMetaData md, Int32 typeDefIndex )
   {
      return md.GetTypeMethodIndices( typeDefIndex ).Select( idx => md.MethodDefinitions.TableContents[idx] );
   }

   public static IEnumerable<FieldDefinition> GetTypeFields( this CILMetaData md, Int32 typeDefIndex )
   {
      return md.GetTypeFieldIndices( typeDefIndex ).Select( idx => md.FieldDefinitions.TableContents[idx] );
   }

   public static IEnumerable<ParameterDefinition> GetMethodParameters( this CILMetaData md, Int32 methodDefIndex )
   {
      return md.GetMethodParameterIndices( methodDefIndex ).Select( idx => md.ParameterDefinitions.TableContents[idx] );
   }

   public static IEnumerable<PropertyDefinition> GetTypeProperties( this CILMetaData md, Int32 propertyMapIndex )
   {
      return md.GetTypePropertyIndices( propertyMapIndex ).Select( idx => md.PropertyDefinitions.TableContents[idx] );
   }

   public static IEnumerable<EventDefinition> GetTypeEvents( this CILMetaData md, Int32 eventMapIndex )
   {
      return md.GetTypeEventIndices( eventMapIndex ).Select( idx => md.EventDefinitions.TableContents[idx] );
   }

   public static IEnumerable<Int32> GetTypeMethodIndices( this CILMetaData md, Int32 typeDefIndex )
   {
      return md.TypeDefinitions.GetTargetIndicesForAscendingReferenceListTable( md.MethodDefinitions.RowCount, typeDefIndex, td => td.MethodList.Index );
   }

   public static IEnumerable<Int32> GetTypeFieldIndices( this CILMetaData md, Int32 typeDefIndex )
   {
      return md.TypeDefinitions.GetTargetIndicesForAscendingReferenceListTable( md.FieldDefinitions.RowCount, typeDefIndex, td => td.FieldList.Index );
   }

   public static IEnumerable<Int32> GetMethodParameterIndices( this CILMetaData md, Int32 methodDefIndex )
   {
      return md.MethodDefinitions.GetTargetIndicesForAscendingReferenceListTable( md.ParameterDefinitions.RowCount, methodDefIndex, mdef => mdef.ParameterList.Index );
   }

   public static IEnumerable<Int32> GetTypePropertyIndices( this CILMetaData md, Int32 propertyMapIndex )
   {
      return md.PropertyMaps.GetTargetIndicesForAscendingReferenceListTable( md.PropertyDefinitions.RowCount, propertyMapIndex, pMap => pMap.PropertyList.Index );
   }

   public static IEnumerable<Int32> GetTypeEventIndices( this CILMetaData md, Int32 eventMapIndex )
   {
      return md.EventMaps.GetTargetIndicesForAscendingReferenceListTable( md.EventDefinitions.RowCount, eventMapIndex, eMap => eMap.EventList.Index );
   }

   internal static IEnumerable<Int32> GetTargetIndicesForAscendingReferenceListTable<T>( this MetaDataTable<T> mdTableWithReferences, Int32 targetTableCount, Int32 tableWithReferencesIndex, Func<T, Int32> referenceExtractor )
      where T : class
   {
      var tableWithReferences = mdTableWithReferences.TableContents;
      if ( tableWithReferencesIndex < 0 || tableWithReferencesIndex >= tableWithReferences.Count )
      {
         throw new ArgumentOutOfRangeException( "Table index." );
      }

      var min = referenceExtractor( tableWithReferences[tableWithReferencesIndex] );
      var max = tableWithReferencesIndex < tableWithReferences.Count - 1 ?
         referenceExtractor( tableWithReferences[tableWithReferencesIndex + 1] ) :
         targetTableCount;
      while ( min < max )
      {
         yield return min;
         ++min;
      }
   }

   public static IEnumerable<FileReference> GetModuleFileReferences( this CILMetaData md )
   {
      return md.FileReferences.TableContents.Where( f => f.Attributes.ContainsMetadata() );
   }

   public static Boolean TryGetEnumValueFieldIndex(
   this CILMetaData md,
   Int32 tDefIndex,
   out Int32 enumValueFieldIndex
   )
   {
      var typeRow = md.TypeDefinitions.GetOrNull( tDefIndex );
      enumValueFieldIndex = -1;
      if ( typeRow != null )
      {
         var extendInfo = typeRow.BaseType;
         if ( extendInfo.HasValue )
         {
            var isEnum = md.IsEnum( extendInfo );
            if ( isEnum )
            {
               // First non-static field of enum type is the field containing enum value
               var fDefs = md.FieldDefinitions.TableContents;
               enumValueFieldIndex = md.GetTypeFieldIndices( tDefIndex )
                  .Where( i => i < fDefs.Count && !fDefs[i].Attributes.IsStatic() )
                  .FirstOrDefaultCustom( -1 );
            }
         }
      }


      return enumValueFieldIndex >= 0;
   }

   public static Boolean IsEnum(
      this CILMetaData md,
      TableIndex? tIdx
      )
   {
      return md.IsSystemType( tIdx, Consts.ENUM_NAMESPACE, Consts.ENUM_TYPENAME );
   }

   internal static Boolean IsTypeType( this CILMetaData md, TableIndex? tIdx )
   {
      return md.IsSystemType( tIdx, Consts.TYPE_NAMESPACE, Consts.TYPE_TYPENAME );
   }

   internal static Boolean IsSystemObjectType( this CILMetaData md, TableIndex tIdx )
   {
      return md.IsSystemType( tIdx, Consts.SYSTEM_OBJECT_NAMESPACE, Consts.SYSTEM_OBJECT_TYPENAME );
   }

   internal static Boolean IsSystemType(
      this CILMetaData md,
      TableIndex? tIdx,
      String systemNS,
      String systemTN
      )
   {
      var result = tIdx.HasValue && tIdx.Value.Table != Tables.TypeSpec;

      if ( result )
      {
         var tIdxValue = tIdx.Value;
         var table = tIdxValue.Table;
         var idx = tIdxValue.Index;

         String tn = null, ns = null;
         if ( table == Tables.TypeDef )
         {
            var tDefs = md.TypeDefinitions.TableContents;
            result = idx < tDefs.Count;
            if ( result )
            {
               tn = tDefs[idx].Name;
               ns = tDefs[idx].Namespace;
            }
         }
         else if ( table == Tables.TypeRef )
         {
            var tRef = md.TypeReferences.GetOrNull( idx );
            result = tRef != null
               && tRef.ResolutionScope.HasValue
               && ( tRef.ResolutionScope.Value.Table == Tables.Module || tRef.ResolutionScope.Value.Table == Tables.AssemblyRef ); // TODO check for 'mscorlib', except that sometimes it may be System.Runtime ...
            if ( result )
            {
               tn = tRef.Name;
               ns = tRef.Namespace;
            }
         }

         if ( result )
         {
            result = String.Equals( tn, systemTN ) && String.Equals( ns, systemNS );
         }
      }
      return result;
   }

   public static IEnumerable<String> GetTypeDefinitionsFullNames( this CILMetaData md )
   {
      var ncInfo = new Dictionary<Int32, Int32>();
      foreach ( var nc in md.NestedClassDefinitions.TableContents )
      {
         ncInfo[nc.NestedClass.Index] = nc.EnclosingClass.Index;
      }

      var tDefs = md.TypeDefinitions.TableContents;
      var enclosingTypeCache = new Dictionary<Int32, String>();
      for ( var i = 0; i < tDefs.Count; ++i )
      {
         var thisIdx = i;
         Int32 enclosingType;
         if ( tDefs[thisIdx].Name == "LogicalCreationState" )
         {

         }
         if ( ncInfo.TryGetValue( thisIdx, out enclosingType ) )
         {
            String typeStr;
            if ( !enclosingTypeCache.TryGetValue( thisIdx, out typeStr ) )
            {
               var thisTDef = tDefs[thisIdx];
               // This should by all logic always return at least 2-element array
               var enclosingTypeChain = thisIdx.AsSingleBranchEnumerableWithLoopDetection(
                  cur =>
                  {
                     Int32 idx;
                     return ncInfo.TryGetValue( cur, out idx ) ? idx : -1;
                  },
                  cur => cur == -1,
                  true )
                  .ToArray();

               // Check if we have cached immediately enclosing type
               if ( enclosingTypeCache.TryGetValue( enclosingTypeChain[1], out typeStr ) )
               {
                  typeStr = Miscellaneous.CombineEnclosingAndNestedType( typeStr, thisTDef.Name );
                  enclosingTypeCache.Add( thisIdx, typeStr );
               }
               else
               {
                  // Build type string
                  var topLevelIdx = enclosingTypeChain[enclosingTypeChain.Length - 1];
                  typeStr = Miscellaneous.CombineNamespaceAndType( tDefs[topLevelIdx].Namespace, tDefs[topLevelIdx].Name );
                  enclosingTypeCache.Add( topLevelIdx, typeStr );

                  for ( var j = enclosingTypeChain.Length - 2; j >= 0; --j )
                  {
                     var curIdx = enclosingTypeChain[j];
                     typeStr += Miscellaneous.NESTED_TYPE_SEPARATOR + tDefs[curIdx].Name;
                     enclosingTypeCache.Add( curIdx, typeStr );
                  }
               }
            }

            yield return typeStr;
         }
         else
         {
            yield return Miscellaneous.CombineNamespaceAndType( tDefs[i].Namespace, tDefs[i].Name );
         }
      }
   }

   public static Object GetByTableIndex( this CILMetaData md, TableIndex index )
   {
      Object retVal;
      if ( !md.TryGetByTableIndex( index, out retVal ) )
      {
         var tbl = (Int32) index.Table;
         if ( tbl >= 0 && tbl < Consts.AMOUNT_OF_TABLES )
         {
            throw new ArgumentOutOfRangeException( "Table index " + index + " was out of range." );
         }
         else
         {
            throw new InvalidOperationException( "The table " + index.Table + " does not have representation in this framework." );
         }
      }

      return retVal;
   }


   public static Boolean TryGetByTableIndex( this CILMetaData md, TableIndex index, out Object row )
   {
      MetaDataTable table;
      var retVal = md.TryGetByTable( (Int32) index.Table, out table ) && index.Index <= table.RowCount;
      row = retVal ? table.GetRowAt( index.Index ) : null;
      return retVal;
   }

   public static TableIndex GetNextTableIndexFor( this CILMetaData md, Tables table )
   {
      return new TableIndex( table, md.GetByTable( (Int32) table ).RowCount );
   }

   // Assumes that all lists of CILMetaData have only non-null elements.
   // TypeDef and MethodDef can not have duplicate instances of same object!!
   // Assumes that MethodList, FieldList indices in TypeDef and ParameterList in MethodDef are all ordered correctly.
   // TODO check that everything works even though <Module> class is not a first row in TypeDef table
   // Duplicates *not* checked from the following tables:
   // TypeDef
   // MethodDef
   // FieldDef
   // PropertyDef
   // EventDef
   // ExportedType


   public static Int32[][] OrderTablesAndRemoveDuplicates( this CILMetaData md )
   {
      // TODO maybe just create a new CILMetaData which would be a sorted version of this??
      // Would simplify a lot of things, and possibly could be even faster (unless given md is already in order)


      //var allTableIndices = new Int32[Consts.AMOUNT_OF_TABLES][];
      var reorderState = new MetaDataReOrderState( md );

      // Start by re-ordering structural (TypeDef, MethodDef, ParamDef, Field, NestedClass) tables
      reorderState.ReOrderStructuralTables();

      // Keep updating and removing duplicates from TypeRef, TypeSpec, MemberRef, MethodSpec, StandaloneSignature and Property tables, while updating all signatures and IL code
      reorderState.UpdateSignaturesAndILWhileRemovingDuplicates();

      // Update and sort the remaining tables which don't have signatures
      reorderState.UpdateAndSortTablesWithNoSignatures();

      // Remove duplicates
      reorderState.RemoveDuplicatesAfterSorting();

      // Sort exception blocks of all ILs
      md.SortMethodILExceptionBlocks();

      return reorderState.FinalIndices;
   }

   // Re-orders TypeDef, MethodDef, ParamDef, Field, and NestedClass tables, if necessary
   private static void ReOrderStructuralTables( this MetaDataReOrderState reorderState )
   {
      var md = reorderState.MetaData;
      // No matter what, we have to remove nested class duplicates
      // Don't need to keep track of changes - nested class table is not referenced by anything

      var nestedClass = md.NestedClassDefinitions;
      nestedClass.RemoveDuplicatesUnsortedInPlace();

      var typeDef = md.TypeDefinitions;
      var methodDef = md.MethodDefinitions;
      var fieldDef = md.FieldDefinitions;
      var paramDef = md.ParameterDefinitions;
      var tDefCount = typeDef.RowCount;
      var mDefCount = methodDef.RowCount;
      var fDefCount = fieldDef.RowCount;
      var pDefCount = paramDef.RowCount;
      var ncCount = nestedClass.RowCount;

      var typeDefIndices = reorderState.GetOrCreateIndexArray( typeDef );
      var methodDefIndices = reorderState.GetOrCreateIndexArray( methodDef );
      var paramDefIndices = reorderState.GetOrCreateIndexArray( paramDef );
      var fDefIndices = reorderState.GetOrCreateIndexArray( fieldDef );
      var ncIndices = reorderState.GetOrCreateIndexArray( nestedClass );


      // So, start by reading nested class data into more easily accessible data structure

      // TypeDef table has special constraint - enclosing class must precede nested class.
      // In other words, for all rows in NestedClass table, the EnclosingClass index must be less than NestedClass index
      // All the tables that are handled in this method will only be needed to re-shuffle if TypeDef table changes, that is, if there are violating rows in NestedClass table.
      var typeDefOrderingChanged = nestedClass.TableContents.Any( nc => nc.NestedClass.Index < nc.EnclosingClass.Index );

      if ( typeDefOrderingChanged )
      {
         // We have to pre-calculate method and field counts for types
         // We have to do this BEFORE typedef table is re-ordered
         var methodAndFieldCounts = new Dictionary<TypeDefinition, KeyValuePair<Int32, Int32>>( tDefCount, ReferenceEqualityComparer<TypeDefinition>.ReferenceBasedComparer );
         var typeDefL = typeDef.TableContents;
         for ( var i = 0; i < tDefCount; ++i )
         {
            var curTD = typeDefL[i];
            Int32 mMax, fMax;
            if ( i + 1 < tDefCount )
            {
               var nextTD = typeDefL[i + 1];
               mMax = nextTD.MethodList.Index;
               fMax = nextTD.FieldList.Index;
            }
            else
            {
               mMax = mDefCount;
               fMax = fDefCount;
            }
            methodAndFieldCounts.Add( curTD, new KeyValuePair<Int32, Int32>( mMax - curTD.MethodList.Index, fMax - curTD.FieldList.Index ) );
         }

         // We have to pre-calculate param count for methods
         // We have to do this BEFORE methoddef table is re-ordered
         var paramCounts = new Dictionary<MethodDefinition, Int32>( mDefCount, ReferenceEqualityComparer<MethodDefinition>.ReferenceBasedComparer );
         var mDefL = methodDef.TableContents;
         for ( var i = 0; i < mDefCount; ++i )
         {
            var curMD = mDefL[i];
            Int32 max;
            if ( i + 1 < mDefCount )
            {
               max = mDefL[i + 1].ParameterList.Index;
            }
            else
            {
               max = pDefCount;
            }
            paramCounts.Add( curMD, max - curMD.ParameterList.Index );
         }

         // Create data structure
         var nestedClassInfo = new Dictionary<Int32, List<Int32>>(); // Key - enclosing type which is lower in TypeDef table than its nested type, Value: list of nested types higher in TypeDef table
         var nestedTypeIndices = new HashSet<Int32>();
         // Populate data structure
         foreach ( var nc in nestedClass.TableContents )
         {
            var enclosing = nc.EnclosingClass.Index;
            var nested = nc.NestedClass.Index;
            nestedClassInfo
                  .GetOrAdd_NotThreadSafe( enclosing, i => new List<Int32>( 1 ) )
                  .Add( nested );
            nestedTypeIndices.Add( nested );
         }
         // Now we can sort TypeDef table

         // Probably most simple and efficient way is to just add nested types right after enclosing types, in BFS style and update typeDefIndices as we go.
         var tDefCopy = typeDefL.ToArray();
         for ( Int32 i = 0, tDefCopyIdx = 0; i < tDefCount; ++i, ++tDefCopyIdx )
         {
            // If we encounter nested type HERE, it means that this nested type is above of enclosing type in the table, skip that
            while ( nestedTypeIndices.Contains( tDefCopyIdx ) )
            {
               ++tDefCopyIdx;
            }

            // Type at index 'tDefCopyIdx' is guaranteed now to be top-level type
            if ( i != tDefCopyIdx )
            {
               typeDefL[i] = tDefCopy[tDefCopyIdx];
               typeDefIndices[tDefCopyIdx] = i;
            }

            // Does this type has nested types
            if ( nestedClassInfo.ContainsKey( tDefCopyIdx ) )
            {
               // Iterate all nested types with BFS
               foreach ( var nested in tDefCopyIdx.AsBreadthFirstEnumerable( cur =>
               {
                  List<Int32> nestedTypes;
                  return nestedClassInfo.TryGetValue( cur, out nestedTypes ) ?
                     nestedTypes :
                     Empty<Int32>.Enumerable;
               }, false ) // Skip this type
               .EndOnFirstLoop() ) // Detect loops to avoid infite enumerable
               {
                  typeDefL[++i] = tDefCopy[nested];
                  typeDefIndices[nested] = i;
               }
            }
         }

         // Update NestedClass indices and sort NestedClass
         reorderState.UpdateMDTableWithTableIndices2(
            md.NestedClassDefinitions,
            nc => nc.NestedClass,
            ( nc, ncIdx ) => nc.NestedClass = ncIdx,
            nc => nc.EnclosingClass,
            ( nc, ecIdx ) => nc.EnclosingClass = ecIdx
            );
         nestedClass.SortMDTable( ncIndices );

         // Sort MethodDef table and update references in TypeDef table
         reorderState.ReOrderMDTableWithAscendingReferences(
            methodDef,
            methodDefIndices,
            typeDef,
            typeDefIndices,
            td => td.MethodList.Index,
            ( td, mIdx ) => td.MethodList = new TableIndex( Tables.MethodDef, mIdx ),
            tdIdx => methodAndFieldCounts[tdIdx].Key
            );

         // Sort ParameterDef table and update references in MethodDef table
         reorderState.ReOrderMDTableWithAscendingReferences(
            paramDef,
            paramDefIndices,
            methodDef,
            methodDefIndices,
            mDef => mDef.ParameterList.Index,
            ( mDef, pIdx ) => mDef.ParameterList = new TableIndex( Tables.Parameter, pIdx ),
            mdIdx => paramCounts[mdIdx]
            );

         // Sort FieldDef table and update references in TypeDef table
         reorderState.ReOrderMDTableWithAscendingReferences(
            fieldDef,
            fDefIndices,
            typeDef,
            typeDefIndices,
            td => td.FieldList.Index,
            ( td, fIdx ) => td.FieldList = new TableIndex( Tables.Field, fIdx ),
            tdIdx => methodAndFieldCounts[tdIdx].Value
            );
      }
   }

   private static void UpdateAndSortTablesWithNoSignatures( this MetaDataReOrderState reorderState )
   {
      var md = reorderState.MetaData;
      // Create table index arrays for tables which are untouched (but can be used by various table indices in table rows)
      reorderState.GetOrCreateIndexArray( md.AssemblyDefinitions );
      reorderState.GetOrCreateIndexArray( md.FileReferences );
      reorderState.GetOrCreateIndexArray( md.PropertyDefinitions );

      // Update TypeDef
      reorderState.UpdateMDTableIndices(
         md.TypeDefinitions,
         ( td, indices ) => reorderState.UpdateMDTableWithTableIndices1Nullable( td, t => t.BaseType, ( t, b ) => t.BaseType = b )
         );

      // Update EventDefinition
      reorderState.UpdateMDTableIndices(
         md.EventDefinitions,
         ( ed, indices ) => reorderState.UpdateMDTableWithTableIndices1( ed, e => e.EventType, ( e, t ) => e.EventType = t )
         );

      // Update EventMap
      reorderState.UpdateMDTableIndices(
         md.EventMaps,
         ( em, indices ) => reorderState.UpdateMDTableWithTableIndices2( em, e => e.Parent, ( e, p ) => e.Parent = p, e => e.EventList, ( e, l ) => e.EventList = l )
         );

      // No table indices in PropertyDefinition

      // Update PropertyMap
      reorderState.UpdateMDTableIndices(
         md.PropertyMaps,
         ( pm, indices ) => reorderState.UpdateMDTableWithTableIndices2( pm, p => p.Parent, ( p, pp ) => p.Parent = pp, p => p.PropertyList, ( p, pl ) => p.PropertyList = pl )
         );

      // Sort InterfaceImpl table ( Class, Interface)
      reorderState.UpdateMDTableIndices(
         md.InterfaceImplementations,
         ( iFaceImpl, indices ) => reorderState.UpdateMDTableWithTableIndices2( iFaceImpl, i => i.Class, ( i, c ) => i.Class = c, i => i.Interface, ( i, iface ) => i.Interface = iface )
         );

      // Sort ConstantDef table (Parent)
      reorderState.UpdateMDTableIndices(
         md.ConstantDefinitions,
         ( constant, indices ) => reorderState.UpdateMDTableWithTableIndices1( constant, c => c.Parent, ( c, p ) => c.Parent = p )
         );

      // Sort FieldMarshal table (Parent)
      reorderState.UpdateMDTableIndices(
         md.FieldMarshals,
         ( marshal, indices ) => reorderState.UpdateMDTableWithTableIndices1( marshal, f => f.Parent, ( f, p ) => f.Parent = p )
         );

      // Sort DeclSecurity table (Parent)
      reorderState.UpdateMDTableIndices(
         md.SecurityDefinitions,
         ( sec, indices ) => reorderState.UpdateMDTableWithTableIndices1( sec, s => s.Parent, ( s, p ) => s.Parent = p )
         );

      // Sort ClassLayout table (Parent)
      reorderState.UpdateMDTableIndices(
         md.ClassLayouts,
         ( clazz, indices ) => reorderState.UpdateMDTableWithTableIndices1( clazz, c => c.Parent, ( c, p ) => c.Parent = p )
         );

      // Sort FieldLayout table (Field)
      reorderState.UpdateMDTableIndices(
         md.FieldLayouts,
         ( fieldLayout, indices ) => reorderState.UpdateMDTableWithTableIndices1( fieldLayout, f => f.Field, ( f, p ) => f.Field = p )
         );

      // Sort MethodSemantics table (Association)
      reorderState.UpdateMDTableIndices(
         md.MethodSemantics,
         ( semantics, indices ) => reorderState.UpdateMDTableWithTableIndices2( semantics, s => s.Method, ( s, m ) => s.Method = m, s => s.Associaton, ( s, a ) => s.Associaton = a )
         );

      // Sort MethodImpl table (Class)
      reorderState.UpdateMDTableIndices(
         md.MethodImplementations,
         ( impl, indices ) => reorderState.UpdateMDTableWithTableIndices3( impl, i => i.Class, ( i, c ) => i.Class = c, i => i.MethodBody, ( i, b ) => i.MethodBody = b, i => i.MethodDeclaration, ( i, d ) => i.MethodDeclaration = d )
         );

      // Sort ImplMap table (MemberForwarded)
      reorderState.UpdateMDTableIndices(
         md.MethodImplementationMaps,
         ( map, indices ) => reorderState.UpdateMDTableWithTableIndices2( map, m => m.MemberForwarded, ( m, mem ) => m.MemberForwarded = mem, m => m.ImportScope, ( m, i ) => m.ImportScope = i )
         );

      // Sort FieldRVA table (Field)
      reorderState.UpdateMDTableIndices(
         md.FieldRVAs,
         ( fieldRVAs, indices ) => reorderState.UpdateMDTableWithTableIndices1( fieldRVAs, f => f.Field, ( f, field ) => f.Field = field )
         );

      // Sort GenericParamDef table (Owner, Sequence)
      reorderState.UpdateMDTableIndices(
         md.GenericParameterDefinitions,
         ( gDef, indices ) => reorderState.UpdateMDTableWithTableIndices1( gDef, g => g.Owner, ( g, o ) => g.Owner = o )
         );

      // Sort GenericParameterConstraint table (Owner)
      reorderState.UpdateMDTableIndices(
         md.GenericParameterConstraintDefinitions,
         ( gDef, indices ) => reorderState.UpdateMDTableWithTableIndices2( gDef, g => g.Owner, ( g, o ) => g.Owner = o, g => g.Constraint, ( g, c ) => g.Constraint = c )
         );

      // Update ExportedType
      reorderState.UpdateMDTableIndices(
         md.ExportedTypes,
         ( et, indices ) => reorderState.UpdateMDTableWithTableIndices1( et, e => e.Implementation, ( e, i ) => e.Implementation = i )
         );

      // Update ManifestResource
      reorderState.UpdateMDTableIndices(
         md.ManifestResources,
         ( mr, indices ) => reorderState.UpdateMDTableWithTableIndices1Nullable( mr, m => m.Implementation, ( m, i ) => m.Implementation = i )
         );

      // Sort CustomAttributeDef table (Parent) 
      reorderState.UpdateMDTableIndices(
         md.CustomAttributeDefinitions,
         ( ca, indices ) => reorderState.UpdateMDTableWithTableIndices2( ca, c => c.Parent, ( c, p ) => c.Parent = p, c => c.Type, ( c, t ) => c.Type = t )
         );
   }

   private static void RemoveDuplicatesUnsortedInPlace<T>( this MetaDataTable<T> mdTable )
      where T : class
   {
      var count = mdTable.RowCount;
      if ( count > 1 )
      {
         var table = mdTable.TableContents;
         var set = new HashSet<T>( mdTable.TableInformation.EqualityComparer );
         for ( var i = 0; i < table.Count; )
         {
            var item = table[i];
            if ( set.Add( item ) )
            {
               ++i;
            }
            else
            {
               table.RemoveAt( i );
            }
         }
      }
   }

   private static void RemoveDuplicatesAfterSorting( this MetaDataReOrderState reorderState )
   {
      var md = reorderState.MetaData;
      foreach ( var kvp in reorderState.Duplicates )
      {
         var table = kvp.Key;
         var indices = kvp.Value;
         switch ( table )
         {
            case Tables.AssemblyRef:
               md.AssemblyReferences.RemoveDuplicatesFromTable( indices );
               break;
            case Tables.ModuleRef:
               md.ModuleReferences.RemoveDuplicatesFromTable( indices );
               break;
            case Tables.TypeSpec:
               md.TypeSpecifications.RemoveDuplicatesFromTable( indices );
               break;
            case Tables.TypeRef:
               md.TypeReferences.RemoveDuplicatesFromTable( indices );
               break;
            case Tables.MemberRef:
               md.MemberReferences.RemoveDuplicatesFromTable( indices );
               break;
            case Tables.MethodSpec:
               md.MethodSpecifications.RemoveDuplicatesFromTable( indices );
               break;
            case Tables.StandaloneSignature:
               md.StandaloneSignatures.RemoveDuplicatesFromTable( indices );
               break;
            case Tables.NestedClass:
               md.NestedClassDefinitions.RemoveDuplicatesFromTable( indices );
               break;
         }
      }
   }

   private static void RemoveDuplicatesFromTable<T>( this MetaDataTable<T> mdTable, IDictionary<Int32, Int32> indices )
      where T : class
   {
      var table = mdTable.TableContents;
      var max = table.Count;
      for ( Int32 curIdx = 0, originalIdx = 0; originalIdx < max; ++originalIdx )
      {
         if ( indices.ContainsKey( originalIdx ) )
         {
            table.RemoveAt( curIdx );
         }
         else
         {
            ++curIdx;
         }
      }
   }

   private static void UpdateMDTableIndices<T>(
      this MetaDataReOrderState reorderState,
      MetaDataTable<T> mdTable,
      Action<MetaDataTable<T>, Int32[]> tableUpdateCallback
      )
      where T : class
   {
      var thisTableIndices = reorderState.GetOrCreateIndexArray( mdTable );
      tableUpdateCallback( mdTable, thisTableIndices );
      mdTable.SortMDTable( thisTableIndices );
   }

   private static void UpdateMDTableWithTableIndices1<T>(
      this MetaDataReOrderState reorderState,
      MetaDataTable<T> mdTable,
      Func<T, TableIndex> tableIndexGetter1,
      Action<T, TableIndex> tableIndexSetter1,
      Func<T, Int32, TableIndex, Boolean> rowAdditionalCheck = null
      )
      where T : class
   {
      var table = mdTable.TableContents;
      for ( var i = 0; i < table.Count; ++i )
      {
         reorderState.ProcessSingleTableIndexToUpdate( table[i], i, tableIndexGetter1, tableIndexSetter1, rowAdditionalCheck );
      }
   }

   private static void UpdateMDTableWithTableIndices1Nullable<T>(
      this MetaDataReOrderState reorderState,
      MetaDataTable<T> mdTable,
      Func<T, TableIndex?> tableIndexGetter1,
      Action<T, TableIndex> tableIndexSetter1,
      Func<T, Int32, TableIndex, Boolean> rowAdditionalCheck = null
      )
      where T : class
   {
      var table = mdTable.TableContents;
      for ( var i = 0; i < table.Count; ++i )
      {
         reorderState.ProcessSingleTableIndexToUpdateNullable( table[i], i, tableIndexGetter1, tableIndexSetter1, rowAdditionalCheck );
      }
   }

   private static void UpdateMDTableWithTableIndices2<T>(
      this MetaDataReOrderState reorderState,
      MetaDataTable<T> mdTable,
      Func<T, TableIndex> tableIndexGetter1,
      Action<T, TableIndex> tableIndexSetter1,
      Func<T, TableIndex> tableIndexGetter2,
      Action<T, TableIndex> tableIndexSetter2
      )
      where T : class
   {
      var table = mdTable.TableContents;
      for ( var i = 0; i < table.Count; ++i )
      {
         var row = table[i];
         reorderState.ProcessSingleTableIndexToUpdate( row, i, tableIndexGetter1, tableIndexSetter1, null );
         reorderState.ProcessSingleTableIndexToUpdate( row, i, tableIndexGetter2, tableIndexSetter2, null );
      }
   }

   private static void UpdateMDTableWithTableIndices3<T>(
      this MetaDataReOrderState reorderState,
      MetaDataTable<T> mdTable,
      Func<T, TableIndex> tableIndexGetter1,
      Action<T, TableIndex> tableIndexSetter1,
      Func<T, TableIndex> tableIndexGetter2,
      Action<T, TableIndex> tableIndexSetter2,
      Func<T, TableIndex> tableIndexGetter3,
      Action<T, TableIndex> tableIndexSetter3
      )
      where T : class
   {
      var table = mdTable.TableContents;
      for ( var i = 0; i < table.Count; ++i )
      {
         var row = table[i];
         reorderState.ProcessSingleTableIndexToUpdate( row, i, tableIndexGetter1, tableIndexSetter1, null );
         reorderState.ProcessSingleTableIndexToUpdate( row, i, tableIndexGetter2, tableIndexSetter2, null );
         reorderState.ProcessSingleTableIndexToUpdate( row, i, tableIndexGetter3, tableIndexSetter3, null );
      }
   }

   private static void ProcessSingleTableIndexToUpdate<T>( this MetaDataReOrderState reorderState, T row, Int32 rowIndex, Func<T, TableIndex> tableIndexGetter, Action<T, TableIndex> tableIndexSetter, Func<T, Int32, TableIndex, Boolean> rowAdditionalCheck )
      where T : class
   {
      if ( row != null )
      {
         reorderState.ProcessSingleTableIndexToUpdateWithTableIndex( row, rowIndex, tableIndexGetter( row ), tableIndexSetter, rowAdditionalCheck );
      }
   }

   private static void ProcessSingleTableIndexToUpdateWithTableIndex<T>( this MetaDataReOrderState reorderState, T row, Int32 rowIndex, TableIndex tableIndex, Action<T, TableIndex> tableIndexSetter, Func<T, Int32, TableIndex, Boolean> rowAdditionalCheck )
      where T : class
   {
      if ( rowIndex == 128 )
      {

      }
      var newIndex = reorderState.GetFinalIndex( tableIndex );
      if ( newIndex != tableIndex.Index && ( rowAdditionalCheck == null || rowAdditionalCheck( row, rowIndex, tableIndex ) ) )
      {
         tableIndexSetter( row, new TableIndex( tableIndex.Table, newIndex ) );
      }
   }

   private static void ProcessSingleTableIndexToUpdateNullable<T>( this MetaDataReOrderState reorderState, T row, Int32 rowIndex, Func<T, TableIndex?> tableIndexGetter, Action<T, TableIndex> tableIndexSetter, Func<T, Int32, TableIndex, Boolean> rowAdditionalCheck )
      where T : class
   {
      if ( row != null )
      {
         var tIdx = tableIndexGetter( row );
         if ( tIdx.HasValue )
         {
            reorderState.ProcessSingleTableIndexToUpdateWithTableIndex( row, rowIndex, tIdx.Value, tableIndexSetter, rowAdditionalCheck );
         }
      }
   }

   private static void SortMDTable<T>( this MetaDataTable<T> mdTable, Int32[] indices )
      where T : class
   {
      var comparer = mdTable.TableInformation.Comparer;
      if ( comparer != null )
      {
         // If within 'indices' array, we have value '2' at index '0', it means that within the 'table', there should be value at index '0' which is currently at index '2'
         var count = mdTable.RowCount;
         if ( count > 1 )
         {
            // 1. Make a copy of array
            var table = mdTable.TableContents;
            var copy = table.ToArray();

            // 2. Sort original array
            table.Sort( comparer );

            // 3. For each element, do a binary search to find where it is now after sorting
            for ( var i = 0; i < count; ++i )
            {
               var idx = table.BinarySearchDeferredEqualityDetection( copy[i], comparer );
               while ( !ReferenceEquals( copy[i], table[idx] ) )
               {
                  ++idx;
               }
               indices[i] = idx;
            }
         }

      }

      //table.SortMDTableWithInt32Comparison( indices, ( x, y ) => comparer.Compare( table[x], table[y] ) );
   }

   private static void SortMethodILExceptionBlocks( this CILMetaData md )
   {
      // Remember that inner exception blocks must precede outer ones
      foreach ( var il in md.MethodDefinitions.TableContents.Where( methodDef => methodDef.IL != null ).Select( methodDef => methodDef.IL ) )
      {
         il.ExceptionBlocks.Sort(
            ( item1, item2 ) =>
            {
               // Return -1 if item1 is inner block of item2, 0 if they are same, 1 if item1 is not inner block of item2
               return Object.ReferenceEquals( item1, item2 ) || ( item1.TryOffset == item2.TryOffset && item1.HandlerOffset == item2.HandlerOffset ) ? 0 :
                  ( item1.TryOffset >= item2.HandlerOffset + item2.HandlerLength
                     || ( item1.TryOffset <= item2.TryOffset && item1.HandlerOffset + item1.HandlerLength > item2.HandlerOffset + item2.HandlerLength ) ?
                  1 :
                  -1
                  );
            } );
      }
   }

   private static void ReOrderMDTableWithAscendingReferences<T, U>(
      this MetaDataReOrderState reorderState,
      MetaDataTable<T> mdTable,
      Int32[] thisTableIndices,
      MetaDataTable<U> referencingMDTable,
      Int32[] referencingTableIndices,
      Func<U, Int32> referenceIndexGetter,
      Action<U, Int32> referenceIndexSetter,
      Func<U, Int32> referenceCountGetter
      )
      where T : class
      where U : class
   {
      var refTableCount = referencingMDTable.RowCount;
      var thisTableCount = mdTable.RowCount;

      if ( thisTableCount > 0 )
      {
         var table = mdTable.TableContents;
         var referencingTable = referencingMDTable.TableContents;

         var originalTable = table.ToArray();

         // Comments talk about typedefs and methoddefs but this method is generalized to handle any two tables with ascending reference pattern
         // This loop walks one typedef at a time, updating methoddef index and re-ordering methoddef array as needed
         for ( Int32 tIdx = 0, mIdx = 0; tIdx < refTableCount; ++tIdx )
         {
            var curTD = referencingTable[tIdx];

            // Inclusive min (the method where current typedef points to)
            var originalMin = referenceIndexGetter( curTD );

            // The count must be pre-calculated - we can't use typedef table to calculate that, as this for loop modifies the reference (e.g. MethodList property of TypeDefinition)
            var blockCount = referenceCountGetter( curTD );

            if ( blockCount > 0 )
            {
               var min = thisTableIndices[originalMin];

               for ( var i = 0; i < blockCount; ++i )
               {
                  var thisMethodIndex = mIdx + i;
                  var originalIndex = min + i;
                  table[thisMethodIndex] = originalTable[originalIndex];
                  thisTableIndices[originalIndex] = thisMethodIndex;
               }

               mIdx += blockCount;
            }

            // Set methoddef index for this typedef
            referenceIndexSetter( curTD, mIdx - blockCount );
         }
      }
   }

   private static Boolean CheckMDDuplicatesUnsorted<T>(
      this MetaDataReOrderState reorderState,
      MetaDataTable<T> mdTable,
      IEqualityComparer<T> comparer = null
      )
      where T : class
   {
      var list = mdTable.TableContents;
      var table = mdTable.TableKind;
      var foundDuplicates = false;
      var count = list.Count;
      var indices = reorderState.GetOrCreateIndexArray( mdTable );
      if ( count > 1 )
      {
         var dic = new Dictionary<T, Int32>( comparer ?? mdTable.TableInformation.EqualityComparer );
         for ( var i = 0; i < list.Count; ++i )
         {
            var cur = list[i];
            if ( cur != null )
            {
               Int32 actualIndex;
               if ( dic.TryGetValue( cur, out actualIndex ) )
               {
                  if ( !foundDuplicates )
                  {
                     foundDuplicates = true;
                  }

                  // Mark as duplicate - replace value with null
                  reorderState.MarkDuplicate( table, i, actualIndex );
                  list[i] = null;

                  // Update index which point to this to point to previous instead
                  var current = indices[i];
                  var prevNotNullIndex = indices[actualIndex];
                  for ( var j = 0; j < indices.Length; ++j )
                  {
                     if ( indices[j] == current )
                     {
                        indices[j] = prevNotNullIndex;
                     }
                     else if ( indices[j] > current )
                     {
                        --indices[j];
                     }
                  }
               }
               else
               {
                  dic.Add( cur, i );
               }
            }

         }
      }

      return foundDuplicates;
   }

   private static void UpdateSignaturesAndILWhileRemovingDuplicates( this MetaDataReOrderState reorderState )
   {
      var md = reorderState.MetaData;

      // Remove duplicates from AssemblyRef table (since reordering of the TypeRef table will require the indices in this table to be present)
      // ECMA-335: The AssemblyRef table shall contain no duplicates (where duplicate rows are deemd  to be those having the same MajorVersion, MinorVersion, BuildNumber, RevisionNumber, PublicKeyOrToken, Name, and Culture) [WARNING] 
      reorderState.CheckMDDuplicatesUnsorted(
         md.AssemblyReferences,
         ComparerFromFunctions.NewEqualityComparer<AssemblyReference>(
            ( x, y ) => x.AssemblyInformation.Equals( y.AssemblyInformation ),
            x => x.AssemblyInformation.GetHashCode() )
         );

      // Remove duplicates from ModuleRef table (since reordering of the TypeRef table will require the indices in this table to be present)
      // ECMA-335: There should be no duplicate rows  [WARNING] 
      reorderState.CheckMDDuplicatesUnsorted(
         md.ModuleReferences
         );


      // TypeRef
      // ECMA-335:  There shall be no duplicate rows, where a duplicate has the same ResolutionScope, TypeName and TypeNamespace  [ERROR] 
      // Do in a loop, since TypeRef may reference itself

      // First, sort them so that all indices into same table would come last, and that they would always index previous row.
      var tRefs = md.TypeReferences;
      var tRefList = tRefs.TableContents;
      var tRefIndices = reorderState.GetOrCreateIndexArray( tRefs );
      // Create index array for Module table, as TypeRef.ResolutionScope may reference that.
      reorderState.GetOrCreateIndexArray( md.ModuleDefinitions );
      var tRefsCorrectOrder = tRefList
         .Select( ( tRef, idx ) => Tuple.Create( tRef, idx ) )
         .OrderBy( tpl => tpl.Item1.ResolutionScope.HasValue && tpl.Item1.ResolutionScope.Value.Table == Tables.TypeRef ? tpl.Item1.ResolutionScope.Value.Index : -1 )
         .ToList();
      tRefList.Clear();
      var tRefDic = new Dictionary<TypeReference, Int32>( tRefs.TableInformation.EqualityComparer );
      for ( var i = 0; i < tRefsCorrectOrder.Count; ++i )
      {
         var tuple = tRefsCorrectOrder[i];
         var tRef = tuple.Item1;
         var rsn = tRef.ResolutionScope;
         if ( rsn.HasValue )
         {
            var rs = rsn.Value;
            var rsIdx = reorderState.GetFinalIndex( rs );
            if ( rs.Index != rsIdx )
            {
               tRef.ResolutionScope = rs.ChangeIndex( rsIdx );
            }
         }

         Int32 newTRefIdx;
         if ( !tRefDic.TryGetValue( tRef, out newTRefIdx ) )
         {
            newTRefIdx = tRefList.Count;
            tRefDic.Add( tRef, newTRefIdx );
            tRefList.Add( tRef );
         }

         tRefIndices[tuple.Item2] = newTRefIdx;
      }

      // TypeSpec
      // ECMA-335: There shall be no duplicate rows, based upon Signature  [ERROR] 
      // Just like TypeRef, there may be self-references
      var tSpecs = md.TypeSpecifications;
      var tSpecList = tSpecs.TableContents;
      var tSpecIndices = reorderState.GetOrCreateIndexArray( tSpecs );
      var tSpecsCorrectOrder = tSpecList
         .Select( ( tSpec, idx ) => Tuple.Create( tSpec, idx ) )
         .OrderBy( tpl =>
         {
            var maxTSpecIdx = -1;
            foreach ( var info in tpl.Item1.Signature.GetAllSignaturesToUpdateForReOrder_Type() )
            {
               var tDefOrRefOrSpec = info.Item2;
               if ( tDefOrRefOrSpec.Table == Tables.TypeSpec )
               {
                  maxTSpecIdx = Math.Max( maxTSpecIdx, tDefOrRefOrSpec.Index );
               }
            }
            return maxTSpecIdx;
         } )
         .ToList();

      tSpecList.Clear();
      var tSpecDic = new Dictionary<TypeSpecification, Int32>( tSpecs.TableInformation.EqualityComparer );
      for ( var i = 0; i < tSpecsCorrectOrder.Count; ++i )
      {
         var tuple = tSpecsCorrectOrder[i];
         var tSpec = tuple.Item1;
         foreach ( var info in tSpec.Signature.GetAllSignaturesToUpdateForReOrder_Type() )
         {
            reorderState.UpdateSingleSignatureElement( info );
         }

         Int32 newTSpecIdx;
         if ( !tSpecDic.TryGetValue( tSpec, out newTSpecIdx ) )
         {
            newTSpecIdx = tSpecList.Count;
            tSpecDic.Add( tSpec, newTSpecIdx );
            tSpecList.Add( tSpec );
         }

         tSpecIndices[tuple.Item2] = newTSpecIdx;
      }

      // TypeSpec was the last of three tables (TypeDef, TypeRef, TypeSpec) which may appear in signatures, so update the rest of the signatures now
      foreach ( var info in reorderState.GetAllSignaturesToUpdateForReOrder() )
      {
         reorderState.UpdateSingleSignatureElement( info );
      }

      // ECMA-335: IL tokens shall be from TypeDef, TypeRef, TypeSpec, MethodDef, FieldDef, MemberRef, MethodSpec or StandaloneSignature tables.
      // The only unprocessed tables from those are MemberRef, MethodSpec and StandaloneSignature
      // ECMA-335:  The MemberRef table shall contain no duplicates, where duplicate rows have the same Class, Name, and Signature  [WARNING] 
      var memberRefs = md.MemberReferences;
      reorderState.UpdateMDTableWithTableIndices1(
         memberRefs,
         mRef => mRef.DeclaringType,
         ( mRef, dType ) => mRef.DeclaringType = dType
         );
      reorderState.CheckMDDuplicatesUnsorted(
         memberRefs
         );

      // MethodSpec
      // ECMA-335: There shall be no duplicate rows based upon Method+Instantiation  [ERROR] 
      var mSpecs = md.MethodSpecifications;
      reorderState.UpdateMDTableWithTableIndices1(
         mSpecs,
         mSpec => mSpec.Method,
         ( mSpec, method ) => mSpec.Method = method
         );
      reorderState.CheckMDDuplicatesUnsorted(
         mSpecs
         );

      // StandaloneSignature
      // ECMA-335: Duplicates allowed (but we will make them all unique anyway)
      var standaloneSigs = md.StandaloneSignatures;
      reorderState.CheckMDDuplicatesUnsorted(
         md.StandaloneSignatures
         );

      // Now update IL
      reorderState.UpdateIL();
   }

   private static void UpdateSingleSignatureElement( this MetaDataReOrderState state, TSigInfo info )
   {
      var tDefOrRefOrSpec = info.Item2;
      var newIdx = state.GetFinalIndex( tDefOrRefOrSpec );
      if ( tDefOrRefOrSpec.Index != newIdx )
      {
         var newTIdx = tDefOrRefOrSpec.ChangeIndex( newIdx );
         var owner = info.Item1;
         if ( owner is CustomModifierSignature )
         {
            ( (CustomModifierSignature) owner ).CustomModifierType = newTIdx;
         }
         else
         {
            ( (ClassOrValueTypeSignature) owner ).Type = newTIdx;
         }
      }
   }

   private static IEnumerable<TSigInfo> GetAllSignaturesToUpdateForReOrder( this MetaDataReOrderState state )
   {
      // TypeSpec table should've been processed at this point already
      // CustomAttribute and DeclarativeSecurity signatures do not reference table indices, so they can be skipped

      var md = state.MetaData;
      return md.FieldDefinitions.TableContents.Where( f => f != null ).SelectMany( f => f.Signature.GetAllSignaturesToUpdateForReOrder_Field() )
         .Concat( md.MethodDefinitions.TableContents.Where( m => m != null ).SelectMany( m => m.Signature.GetAllSignaturesToUpdateForReOrder_MethodDef() ) )
         .Concat( md.MemberReferences.TableContents.SelectMany( m => m.Signature.GetAllSignaturesToUpdateForReOrder() ) )
         .Concat( md.StandaloneSignatures.TableContents.SelectMany( s => s.Signature.GetAllSignaturesToUpdateForReOrder() ) )
         .Concat( md.PropertyDefinitions.TableContents.SelectMany( p => p.Signature.GetAllSignaturesToUpdateForReOrder_Property() ) )
         .Concat( md.MethodSpecifications.TableContents.SelectMany( m => m.Signature.GetAllSignaturesToUpdateForReOrder_GenericMethod() ) );
   }

   private static IEnumerable<TSigInfo> GetAllSignaturesToUpdateForReOrder( this AbstractSignature sig )
   {
      switch ( sig.SignatureKind )
      {
         case SignatureKind.Field:
            return ( (FieldSignature) sig ).GetAllSignaturesToUpdateForReOrder_Field();
         case SignatureKind.GenericMethodInstantiation:
            return ( (GenericMethodSignature) sig ).GetAllSignaturesToUpdateForReOrder_GenericMethod();
         case SignatureKind.LocalVariables:
            return ( (LocalVariablesSignature) sig ).GetAllSignaturesToUpdateForReOrder_Locals();
         case SignatureKind.MethodDefinition:
            return ( (MethodDefinitionSignature) sig ).GetAllSignaturesToUpdateForReOrder_MethodDef();
         case SignatureKind.MethodReference:
            return ( (MethodReferenceSignature) sig ).GetAllSignaturesToUpdateForReOrder_MethodRef();
         case SignatureKind.Property:
            return ( (PropertySignature) sig ).GetAllSignaturesToUpdateForReOrder_Property();
         case SignatureKind.Type:
            return ( (TypeSignature) sig ).GetAllSignaturesToUpdateForReOrder_Type();
         case SignatureKind.RawSignature:
            return Empty<TSigInfo>.Enumerable;
         default:
            throw new InvalidOperationException( "Unrecognized signature kind: " + sig.SignatureKind + "." );
      }
   }

   private static IEnumerable<TSigInfo> GetAllSignaturesToUpdateForReOrder_Field( this FieldSignature sig )
   {
      return sig.CustomModifiers.Select( cm => Tuple.Create( (Object) cm, cm.CustomModifierType ) )
         .Concat( sig.Type.GetAllSignaturesToUpdateForReOrder_Type() );
   }

   private static IEnumerable<TSigInfo> GetAllSignaturesToUpdateForReOrder_GenericMethod( this GenericMethodSignature sig )
   {
      return sig.GenericArguments.SelectMany( arg => arg.GetAllSignaturesToUpdateForReOrder() );
   }

   private static IEnumerable<TSigInfo> GetAllSignaturesToUpdateForReOrder_Locals( this LocalVariablesSignature sig )
   {
      return sig.Locals.SelectMany( l => l.GetAllSignaturesToUpdateForReOrder_LocalOrSig() );
   }

   private static IEnumerable<TSigInfo> GetAllSignaturesToUpdateForReOrder_AbstractMethod( this AbstractMethodSignature sig )
   {
      return sig.ReturnType.GetAllSignaturesToUpdateForReOrder_LocalOrSig()
         .Concat( sig.Parameters.SelectMany( p => p.GetAllSignaturesToUpdateForReOrder_LocalOrSig() ) );
   }

   private static IEnumerable<TSigInfo> GetAllSignaturesToUpdateForReOrder_LocalOrSig( this ParameterOrLocalVariableSignature sig )
   {
      return sig.CustomModifiers.Select( cm => Tuple.Create( (Object) cm, cm.CustomModifierType ) )
         .Concat( sig.Type.GetAllSignaturesToUpdateForReOrder_Type() );
   }

   private static IEnumerable<TSigInfo> GetAllSignaturesToUpdateForReOrder_MethodDef( this MethodDefinitionSignature sig )
   {
      return sig.GetAllSignaturesToUpdateForReOrder_AbstractMethod();
   }

   private static IEnumerable<TSigInfo> GetAllSignaturesToUpdateForReOrder_MethodRef( this MethodReferenceSignature sig )
   {
      return sig.GetAllSignaturesToUpdateForReOrder_AbstractMethod()
         .Concat( sig.VarArgsParameters.SelectMany( p => p.GetAllSignaturesToUpdateForReOrder_LocalOrSig() ) );
   }

   private static IEnumerable<TSigInfo> GetAllSignaturesToUpdateForReOrder_Property( this PropertySignature sig )
   {
      return sig.CustomModifiers.Select( cm => Tuple.Create( (Object) cm, cm.CustomModifierType ) )
         .Concat( sig.Parameters.SelectMany( p => p.GetAllSignaturesToUpdateForReOrder_LocalOrSig() ) )
         .Concat( sig.PropertyType.GetAllSignaturesToUpdateForReOrder_Type() );
   }

   private static IEnumerable<TSigInfo> GetAllSignaturesToUpdateForReOrder_Type( this TypeSignature sig )
   {
      switch ( sig.TypeSignatureKind )
      {
         case TypeSignatureKind.ClassOrValue:
            var clazz = (ClassOrValueTypeSignature) sig;
            return Tuple.Create( (Object) clazz, clazz.Type ).Singleton()
               .Concat( clazz.GenericArguments.SelectMany( g => g.GetAllSignaturesToUpdateForReOrder_Type() ) );
         case TypeSignatureKind.ComplexArray:
            return ( (ComplexArrayTypeSignature) sig ).ArrayType.GetAllSignaturesToUpdateForReOrder_Type();
         case TypeSignatureKind.FunctionPointer:
            return ( (FunctionPointerTypeSignature) sig ).MethodSignature.GetAllSignaturesToUpdateForReOrder_MethodRef();
         case TypeSignatureKind.Pointer:
            var ptr = (PointerTypeSignature) sig;
            return ptr.CustomModifiers.Select( cm => Tuple.Create( (Object) cm, cm.CustomModifierType ) )
               .Concat( ptr.PointerType.GetAllSignaturesToUpdateForReOrder_Type() );
         case TypeSignatureKind.SimpleArray:
            var arr = (SimpleArrayTypeSignature) sig;
            return arr.CustomModifiers.Select( cm => Tuple.Create( (Object) cm, cm.CustomModifierType ) )
               .Concat( arr.ArrayType.GetAllSignaturesToUpdateForReOrder_Type() );
         case TypeSignatureKind.GenericParameter:
         case TypeSignatureKind.Simple:
            return Empty<TSigInfo>.Enumerable;
         default:
            throw new InvalidOperationException( "Unrecognized type signature kind: " + sig.TypeSignatureKind + "." );
      }
   }

   private static void UpdateIL( this MetaDataReOrderState state )
   {
      foreach ( var mDef in state.MetaData.MethodDefinitions.TableContents )
      {
         var il = mDef.IL;
         if ( il != null )
         {
            // Local signature
            var localIdx = il.LocalsSignatureIndex;
            if ( localIdx.HasValue )
            {
               var newIdx = state.GetFinalIndex( localIdx.Value );
               if ( newIdx != localIdx.Value.Index )
               {
                  il.LocalsSignatureIndex = localIdx.Value.ChangeIndex( newIdx );
               }
            }

            // Exception blocks
            foreach ( var block in il.ExceptionBlocks )
            {
               var excIdx = block.ExceptionType;
               if ( excIdx.HasValue )
               {
                  var newIdx = state.GetFinalIndex( excIdx.Value );
                  if ( newIdx != excIdx.Value.Index )
                  {
                     block.ExceptionType = excIdx.Value.ChangeIndex( newIdx );
                  }
               }
            }

            // Op codes
            foreach ( var code in il.OpCodes.Where( code => code.InfoKind == OpCodeOperandKind.OperandToken ) )
            {
               var codeInfo = (OpCodeInfoWithToken) code;
               var token = codeInfo.Operand;
               var newIdx = state.GetFinalIndex( token );
               if ( newIdx != token.Index )
               {
                  codeInfo.Operand = token.ChangeIndex( newIdx );
               }
            }
         }
      }
   }

   // Returns token with 1-based indexing, or zero if tableIdx has no value
   internal static Int32 GetOneBasedToken( this TableIndex? tableIdx )
   {
      return tableIdx.HasValue ?
         tableIdx.Value.OneBasedToken :
         0;
   }

   public static Int32 CalculateStackSize( this CILMetaData md, Int32 methodIndex )
   {
      var mDef = md.MethodDefinitions.GetOrNull( methodIndex );
      var retVal = -1;
      if ( mDef != null )
      {
         var il = mDef.IL;
         if ( il != null )
         {
            var state = new StackCalculationState( md, il.OpCodes.Sum( oc => oc.GetTotalByteCount() ) );

            // Setup exception block stack sizes
            foreach ( var block in il.ExceptionBlocks )
            {
               switch ( block.BlockType )
               {
                  case ExceptionBlockType.Exception:
                     state.StackSizes[block.HandlerOffset] = 1;
                     break;
                  case ExceptionBlockType.Filter:
                     state.StackSizes[block.HandlerOffset] = 1;
                     state.StackSizes[block.FilterOffset] = 1;
                     break;
               }
            }

            // Calculate actual max stack
            foreach ( var codeInfo in il.OpCodes )
            {
               var byteCount = codeInfo.GetTotalByteCount();
               state.NextCodeByteOffset += byteCount;
               UpdateStackSize( state, codeInfo );
               state.CurrentCodeByteOffset += byteCount;
            }

            retVal = state.MaxStack;
         }
      }

      return retVal;
   }

   private static void UpdateStackSize(
      StackCalculationState state,
      OpCodeInfo codeInfo
      )
   {
      var code = codeInfo.OpCode;
      var curStacksize = Math.Max( state.CurrentStack, state.StackSizes[state.CurrentCodeByteOffset] );
      if ( FlowControl.Call == code.FlowControl )
      {
         curStacksize = UpdateStackSizeForMethod( state, code, ( (OpCodeInfoWithToken) codeInfo ).Operand, curStacksize );
      }
      else
      {
         curStacksize += code.StackChange;
      }

      // Save max stack here
      state.UpdateMaxStack( curStacksize );

      // Copy branch stack size
      if ( curStacksize > 0 )
      {
         switch ( code.OperandType )
         {
            case OperandType.InlineBrTarget:
               UpdateStackSizeAtBranchTarget( state, ( (OpCodeInfoWithInt32) codeInfo ).Operand, curStacksize );
               break;
            case OperandType.ShortInlineBrTarget:
               UpdateStackSizeAtBranchTarget( state, ( (OpCodeInfoWithInt32) codeInfo ).Operand, curStacksize );
               break;
            case OperandType.InlineSwitch:
               var offsets = ( (OpCodeInfoWithSwitch) codeInfo ).Offsets;
               for ( var i = 0; i < offsets.Count; ++i )
               {
                  UpdateStackSizeAtBranchTarget( state, offsets[i], curStacksize );
               }
               break;
         }
      }

      // Set stack to zero if required
      if ( code.UnconditionallyEndsBulkOfCode )
      {
         curStacksize = 0;
      }

      // Save current size for next iteration
      state.CurrentStack = curStacksize;
   }

   private static Int32 UpdateStackSizeForMethod(
      StackCalculationState state,
      OpCode code,
      TableIndex method,
      Int32 curStacksize
      )
   {
      var sig = ResolveSignatureFromTableIndex( state, method );

      if ( sig != null )
      {
         var isNewObj = code.Value == OpCodeEncoding.Newobj;
         if ( sig.SignatureStarter.IsHasThis() && !isNewObj )
         {
            // Pop 'this'
            --curStacksize;
         }

         // Pop parameters
         curStacksize -= sig.Parameters.Count;
         var refSig = sig as MethodReferenceSignature;
         if ( refSig != null )
         {
            curStacksize -= refSig.VarArgsParameters.Count;
         }

         if ( code.Value == OpCodeEncoding.Calli )
         {
            // Pop function pointer
            --curStacksize;
         }

         var rType = sig.ReturnType.Type;

         // TODO we could check here for stack underflow!

         if ( isNewObj
            || rType.TypeSignatureKind != TypeSignatureKind.Simple
            || ( (SimpleTypeSignature) rType ).SimpleType != SignatureElementTypes.Void
            )
         {
            // Push return value
            ++curStacksize;
         }
      }

      return curStacksize;
   }

   private static AbstractMethodSignature ResolveSignatureFromTableIndex(
      StackCalculationState state,
      TableIndex method
      )
   {
      var mIdx = method.Index;
      switch ( method.Table )
      {
         case Tables.MethodDef:
            var mDef = state.MD.MethodDefinitions.GetOrNull( mIdx );
            return mDef == null ? null : mDef.Signature;
         case Tables.MemberRef:
            var mRef = state.MD.MemberReferences.GetOrNull( mIdx );
            return mRef == null ? null : mRef.Signature as AbstractMethodSignature;
         case Tables.StandaloneSignature:
            var sig = state.MD.StandaloneSignatures.GetOrNull( mIdx );
            return sig == null ? null : sig.Signature as AbstractMethodSignature;
         case Tables.MethodSpec:
            var mSpec = state.MD.MethodSpecifications.GetOrNull( mIdx );
            return mSpec == null ? null : ResolveSignatureFromTableIndex( state, mSpec.Method );
         default:
            return null;
      }
   }

   private static void UpdateStackSizeAtBranchTarget(
      StackCalculationState state,
      Int32 jump,
      Int32 stackSize
      )
   {
      if ( jump >= 0 )
      {
         var idx = state.NextCodeByteOffset + jump;
         state.StackSizes[idx] = Math.Max( state.StackSizes[idx], stackSize );
      }
   }

   public static Boolean TryGetTargetFrameworkInformation( this CILMetaData md, out TargetFrameworkInfo fwInfo, MetaDataResolver resolverToUse = null )
   {
      fwInfo = md.CustomAttributeDefinitions.TableContents
         .Where( ( ca, caIdx ) =>
         {
            var isTargetFWAttribute = false;
            if ( ca.Parent.Table == Tables.Assembly
            && md.AssemblyDefinitions.GetOrNull( ca.Parent.Index ) != null
            && ca.Type.Table == Tables.MemberRef ) // Remember that framework assemblies don't have TargetFrameworkAttribute defined
            {
               var memberRef = md.MemberReferences.GetOrNull( ca.Type.Index );
               if ( memberRef != null
                  && memberRef?.Signature?.SignatureKind == SignatureKind.MethodReference
                  && memberRef.DeclaringType.Table == Tables.TypeRef
                  && String.Equals( memberRef.Name, Miscellaneous.INSTANCE_CTOR_NAME )
                  )
               {
                  var typeRef = md.TypeReferences.GetOrNull( memberRef.DeclaringType.Index );
                  if ( typeRef != null
                     && typeRef.ResolutionScope.HasValue
                     && typeRef.ResolutionScope.Value.Table == Tables.AssemblyRef
                     && String.Equals( typeRef.Namespace, "System.Runtime.Versioning" )
                     && String.Equals( typeRef.Name, "TargetFrameworkAttribute" )
                     )
                  {
                     if ( ca.Signature is RawCustomAttributeSignature )
                     {
                        // Use resolver with no events, so nothing additional will be loaded (and is not required, as both arguments are strings
                        ( resolverToUse ?? new MetaDataResolver() ).ResolveCustomAttributeSignature( md, caIdx );
                     }

                     var caSig = ca.Signature as CustomAttributeSignature;
                     if ( caSig != null
                        && caSig.TypedArguments.Count > 0
                        )
                     {
                        // Resolving succeeded
                        isTargetFWAttribute = true;
                     }
#if DEBUG
                     else
                     {
                        // Breakpoint (resolving failed, even though it should have succeeded
                     }
#endif
                  }
               }
            }
            return isTargetFWAttribute;
         } )
         .Select( ca =>
         {

            var fwInfoString = ( (CustomAttributeSignature) ca.Signature ).TypedArguments[0].Value.ToStringSafe( null );
            //var displayName = caSig.NamedArguments.Count > 0
            //   && String.Equals( caSig.NamedArguments[0].Name, "FrameworkDisplayName" )
            //   && caSig.NamedArguments[0].Value.Type.IsSimpleTypeOfKind( SignatureElementTypes.String ) ?
            //   caSig.NamedArguments[0].Value.Value.ToStringSafe( null ) :
            //   null;
            TargetFrameworkInfo thisFWInfo;
            return TargetFrameworkInfo.TryParse( fwInfoString, out thisFWInfo ) ? thisFWInfo : null;

         } )
         .FirstOrDefault();

      return fwInfo != null;
   }

   public static TargetFrameworkInfo GetTargetFrameworkInformationOrNull( this CILMetaData md, MetaDataResolver resolverToUse = null )
   {
      TargetFrameworkInfo retVal;
      return md.TryGetTargetFrameworkInformation( out retVal, resolverToUse ) ?
         retVal :
         null;
   }

   /// <summary>
   /// Gets textual representation of the version information contained in <paramref name="info"/>. The format is: "<c>&lt;major&gt;.&lt;minor&gt;.&lt;build&gt;.&lt;revision&gt;</c>".
   /// </summary>
   /// <param name="info">The assembly information containing version information.</param>
   /// <returns>Textual representation of the version information contained in <paramref name="info"/>.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="info"/> is <c>null</c>.</exception>
   public static String GetVersionString( this AssemblyInformation info )
   {
      ArgumentValidator.ValidateNotNull( "Assembly name", info );
      return new StringBuilder( info.VersionMajor )
         .Append( AssemblyInformation.VERSION_SEPARATOR )
         .Append( info.VersionMinor )
         .Append( AssemblyInformation.VERSION_SEPARATOR )
         .Append( info.VersionBuild )
         .Append( AssemblyInformation.VERSION_SEPARATOR )
         .Append( info.VersionRevision )
         .ToString();
   }

   /// <summary>
   /// Returns textual representation of the culture information contained in <paramref name="info"/>. If the <see cref="AssemblyInformation.Culture"/> returns <c>null</c> or empty string, this method returns string "neutral". Otherwise it returns the result of <see cref="AssemblyInformation.Culture"/>.
   /// </summary>
   /// <param name="info">The assembly information containing culture information.</param>
   /// <returns>Textual representation of the culture information contained in <paramref name="info"/>.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="info"/> is <c>null</c>.</exception>
   public static String GetCultureString( this AssemblyInformation info )
   {
      ArgumentValidator.ValidateNotNull( "Assembly name", info );
      var culture = info.Culture;
      return String.IsNullOrEmpty( culture ) ? AssemblyInformation.NEUTRAL_CULTURE : culture;
   }

}