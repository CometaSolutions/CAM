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
using TabularMetaData;

using TSigInfo = System.Tuple<System.Object, CILAssemblyManipulator.Physical.TableIndex>;

namespace CILAssemblyManipulator.Physical
{
   /// <summary>
   /// This interface represents a single metadata instance.
   /// It is composed of metadata tables.
   /// The instances of this interface may be created via static methods of <see cref="T:CILAssemblyManipulator.Physical.CILMetaDataFactory" />.
   /// </summary>
   /// <remarks>
   /// <para>
   /// All indices used with the tables of <see cref="CILMetaData"/> are zero-based, i.e. first element is at index <c>0</c>, second at <c>1</c>, etc.
   /// </para>
   /// <para>
   /// This interface does not enforce any of the integrity and consistency rules of serialized and loadable metadata files.
   /// </para>
   /// </remarks>
   /// <seealso cref="T:CILAssemblyManipulator.Physical.CILMetaDataFactory"/>
   /// <seealso cref="MetaDataTable"/>
   public interface CILMetaData : TabularMetaDataWithSchema
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
      /// <seealso cref="FieldLayout"/>
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
      /// <seealso cref="EventMap"/>
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
      /// <seealso cref="PropertyMap"/>
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
      /// <seealso cref="Physical.EditAndContinueLog"/>
      MetaDataTable<EditAndContinueLog> EditAndContinueLog { get; }

      /// <summary>
      /// This property represents a metadata table for <see cref="Tables.EncMap"/>.
      /// </summary>
      /// <value>The metadata table for <see cref="Tables.EncMap"/>.</value>
      /// <seealso cref="MetaDataTable"/>
      /// <seealso cref="Physical.EditAndContinueMap"/>
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

      /// <summary>
      /// Gets the <see cref="Meta.OpCodeProvider"/> of this <see cref="CILMetaData"/>.
      /// </summary>
      /// <value>The <see cref="Meta.OpCodeProvider"/> of this <see cref="CILMetaData"/>.</value>
      /// <seealso cref="Meta.OpCodeProvider"/>
      Meta.OpCodeProvider OpCodeProvider { get; }

      /// <summary>
      /// Gets the <see cref="Meta.SignatureProvider"/> of this <see cref="CILMetaData"/>.
      /// </summary>
      /// <value>The <see cref="Meta.SignatureProvider"/> of this <see cref="CILMetaData"/>.</value>
      /// <seealso cref="Meta.SignatureProvider"/>
      Meta.SignatureProvider SignatureProvider { get; }
   }



}


public static partial class E_CILPhysical
{
   private sealed class StackCalculationState
   {
      private Int32 _maxStack;

      internal StackCalculationState( CILMetaData md, Int32 ilByteCount )
      {
         this.MD = md;
         this.StackSizes = new Int32[ilByteCount];
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

      public CILMetaData MD { get; }

      public Int32[] StackSizes { get; }

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

      internal MetaDataReOrderState( CILMetaData md )
      {
         this.MetaData = md;
         this.Duplicates = new Dictionary<Tables, IDictionary<Int32, Int32>>();
         this.FinalIndices = new Int32[CAMCoreInternals.AMOUNT_OF_TABLES][];
      }

      public CILMetaData MetaData { get; }

      public IDictionary<Tables, IDictionary<Int32, Int32>> Duplicates { get; }

      public Int32[][] FinalIndices { get; }

      public void MarkDuplicate( Int32 tableIndex, Int32 duplicateIdx, Int32 actualIndex )
      {
         var table = (Tables) tableIndex;
         var thisDuplicates = this.Duplicates
            .GetOrAdd_NotThreadSafe( table, t => new Dictionary<Int32, Int32>() );
         thisDuplicates
            .Add( duplicateIdx, actualIndex );
         // Update all other duplicates as well
         // TODO I'm not sure if this is needed anymore, now that each table is processed only exactly once (and not in e.g. loop)
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
         return this.Duplicates.TryGetValue( index.Table, out tableDuplicates )
            && tableDuplicates.ContainsKey( index.Index );
      }

      public Boolean IsDuplicate( TableIndex index, out Int32 newIndex )
      {
         IDictionary<Int32, Int32> tableDuplicates;
         newIndex = -1;
         var retVal = this.Duplicates.TryGetValue( index.Table, out tableDuplicates )
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
         var tIdx = table.GetTableIndex();
         var retVal = this.FinalIndices[tIdx];
         if ( retVal == null )
         {
            var list = table.TableContents;
            retVal = new Int32[list.Count];
            for ( var i = 0; i < retVal.Length; ++i )
            {
               retVal[i] = i;
            }
            this.FinalIndices[tIdx] = retVal;
         }
         return retVal;
      }

      public Int32 GetFinalIndex( TableIndex index )
      {
         return this.FinalIndices[(Int32) index.Table][index.Index];
      }

      public Int32 GetFinalIndex( Tables table, Int32 index )
      {
         return this.FinalIndices[(Int32) table][index];
      }
   }

   /// <summary>
   /// Gets the local signature corresponding to method at given index in given <see cref="CILMetaData"/>.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="methodDefIndex">The index of method in <see cref="CILMetaData.MethodDefinitions"/> table.</param>
   /// <returns>The local signature corresponding to method at given index in given <see cref="CILMetaData"/>. Will be <c>null</c> if <paramref name="md"/> is <c>null</c>, or any table index out of range.</returns>
   public static LocalVariablesSignature GetLocalsSignatureForMethodOrNull( this CILMetaData md, Int32 methodDefIndex )
   {
      var tIdx = md?.MethodDefinitions?.GetOrNull( methodDefIndex )?.IL?.LocalsSignatureIndex;
      LocalVariablesSignature retVal;
      if ( tIdx.HasValue )
      {
         retVal = md?.StandaloneSignatures?.GetOrNull( tIdx.Value.Index )?.Signature as LocalVariablesSignature;
      }
      else
      {
         retVal = null;
      }

      return retVal;
   }

   /// <summary>
   /// Gets all the <see cref="MethodDefinition"/>s that are considered to be part of a type at given index.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="typeDefIndex">The index of type in <see cref="CILMetaData.TypeDefinitions"/> table.</param>
   /// <returns>The enumerable that will iterate over methods of the type. Will be empty if <paramref name="md"/> is <c>null</c>, the type has no methods, the <paramref name="typeDefIndex"/> is out of range, or the possible next item in <see cref="CILMetaData.TypeDefinitions"/> will have bad <see cref="TypeDefinition.MethodList"/> index.</returns>
   /// <seealso cref="GetTypeMethodIndices"/>
   /// <remarks>
   /// The returned enumerable is reactive in a sense that modifications done to tables e.g. after first iteration but before second iteration will affect the results of second iteration.
   /// </remarks>
   public static IEnumerable<MethodDefinition> GetTypeMethods( this CILMetaData md, Int32 typeDefIndex )
   {
      return md.GetTypeMethodIndices( typeDefIndex ).Select( idx => md.MethodDefinitions.TableContents[idx] );
   }

   /// <summary>
   /// Gets all the <see cref="FieldDefinition"/>s that are considered to be part of a type at given index.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="typeDefIndex">The index of type in <see cref="CILMetaData.TypeDefinitions"/> table.</param>
   /// <returns>The enumerable that will iterate over fields of the type. Will be empty if <paramref name="md"/> is <c>null</c>, the type has no fields, the <paramref name="typeDefIndex"/> is out of range, or the possible next item in <see cref="CILMetaData.TypeDefinitions"/> will have bad <see cref="TypeDefinition.FieldList"/> index.</returns>
   /// <seealso cref="GetTypeFieldIndices"/>
   /// <remarks>
   /// The returned enumerable is reactive in a sense that modifications done to tables e.g. after first iteration but before second iteration will affect the results of second iteration.
   /// </remarks>
   public static IEnumerable<FieldDefinition> GetTypeFields( this CILMetaData md, Int32 typeDefIndex )
   {
      return md.GetTypeFieldIndices( typeDefIndex ).Select( idx => md.FieldDefinitions.TableContents[idx] );
   }

   /// <summary>
   /// Gets all the <see cref="ParameterDefinition"/>s that are considered to be part of a method at given index.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="methodDefIndex">The index of method in <see cref="CILMetaData.MethodDefinitions"/> table.</param>
   /// <returns>The enumerable that will iterate over parameters of the methods. Will be empty if <paramref name="md"/> is <c>null</c>, the method has no parameters, the <paramref name="methodDefIndex"/> is out of range, or the possible next item in <see cref="CILMetaData.MethodDefinitions"/> will have bad <see cref="MethodDefinition.ParameterList"/> index.</returns>
   /// <seealso cref="GetMethodParameterIndices"/>
   /// <remarks>
   /// The returned enumerable is reactive in a sense that modifications done to tables e.g. after first iteration but before second iteration will affect the results of second iteration.
   /// </remarks>
   public static IEnumerable<ParameterDefinition> GetMethodParameters( this CILMetaData md, Int32 methodDefIndex )
   {
      return md.GetMethodParameterIndices( methodDefIndex ).Select( idx => md.ParameterDefinitions.TableContents[idx] );
   }

   /// <summary>
   /// Gets all the <see cref="PropertyDefinition"/>s that are considered to be part of a property mapping at given index.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="propertyMapIndex">The index of method in <see cref="CILMetaData.PropertyMaps"/> table.</param>
   /// <returns>The enumerable that will iterate over properties of the property mapping. Will be empty if <paramref name="md"/> is <c>null</c>, the property mapping has no properties, the <paramref name="propertyMapIndex"/> is out of range, or the possible next item in <see cref="CILMetaData.PropertyMaps"/> will have bad <see cref="PropertyMap.PropertyList"/> index.</returns>
   /// <seealso cref="GetPropertyMapPropertyIndices"/>
   /// <remarks>
   /// The returned enumerable is reactive in a sense that modifications done to tables e.g. after first iteration but before second iteration will affect the results of second iteration.
   /// </remarks>
   public static IEnumerable<PropertyDefinition> GetPropertyMapProperties( this CILMetaData md, Int32 propertyMapIndex )
   {
      return md.GetPropertyMapPropertyIndices( propertyMapIndex ).Select( idx => md.PropertyDefinitions.TableContents[idx] );
   }

   /// <summary>
   /// Gets all the <see cref="EventDefinition"/>s that are considered to be part of a event mapping at given index.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="eventMapIndex">The index of method in <see cref="CILMetaData.EventMaps"/> table.</param>
   /// <returns>The enumerable that will iterate over events of the event mapping. Will be empty if <paramref name="md"/> is <c>null</c>, the event mapping has no events, the <paramref name="eventMapIndex"/> is out of range, or the possible next item in <see cref="CILMetaData.EventMaps"/> will have bad <see cref="EventMap.EventList"/> index.</returns>
   /// <seealso cref="GetEventMapEventIndices"/>
   /// <remarks>
   /// The returned enumerable is reactive in a sense that modifications done to tables e.g. after first iteration but before second iteration will affect the results of second iteration.
   /// </remarks>
   public static IEnumerable<EventDefinition> GetEventMapEvents( this CILMetaData md, Int32 eventMapIndex )
   {
      return md.GetEventMapEventIndices( eventMapIndex ).Select( idx => md.EventDefinitions.TableContents[idx] );
   }

   /// <summary>
   /// Gets the indices of all the <see cref="MethodDefinition"/>s that are considered to be part of a type at given index.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="typeDefIndex">The index of type in <see cref="CILMetaData.TypeDefinitions"/> table.</param>
   /// <returns>The enumerable that will iterate over method indices of the type. Will be empty if <paramref name="md"/> is <c>null</c>, the type has no methods, the <paramref name="typeDefIndex"/> is out of range, or the possible next item in <see cref="CILMetaData.TypeDefinitions"/> will have bad <see cref="TypeDefinition.MethodList"/> index.</returns>
   /// <seealso cref="GetTypeMethods"/>
   /// <remarks>
   /// The returned enumerable is reactive in a sense that modifications done to tables e.g. after first iteration but before second iteration will affect the results of second iteration.
   /// </remarks>
   public static IEnumerable<Int32> GetTypeMethodIndices( this CILMetaData md, Int32 typeDefIndex )
   {
      return md == null ? Empty<Int32>.Enumerable : md.TypeDefinitions.GetTargetIndicesForAscendingReferenceListTable( typeDefIndex, md.MethodDefinitions.GetRowCount(), td => td.MethodList.Index );
   }

   /// <summary>
   /// Gets the indices of all the <see cref="FieldDefinition"/>s that are considered to be part of a type at given index.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="typeDefIndex">The index of type in <see cref="CILMetaData.TypeDefinitions"/> table.</param>
   /// <returns>The enumerable that will iterate over indices of fields of the type. Will be empty if <paramref name="md"/> is <c>null</c>, the type has no fields, the <paramref name="typeDefIndex"/> is out of range, or the possible next item in <see cref="CILMetaData.TypeDefinitions"/> will have bad <see cref="TypeDefinition.FieldList"/> index.</returns>
   /// <seealso cref="GetTypeFields"/>
   /// <remarks>
   /// The returned enumerable is reactive in a sense that modifications done to tables e.g. after first iteration but before second iteration will affect the results of second iteration.
   /// </remarks>
   public static IEnumerable<Int32> GetTypeFieldIndices( this CILMetaData md, Int32 typeDefIndex )
   {
      return md == null ? Empty<Int32>.Enumerable : md.TypeDefinitions.GetTargetIndicesForAscendingReferenceListTable( typeDefIndex, md.FieldDefinitions.GetRowCount(), td => td.FieldList.Index );
   }

   /// <summary>
   /// Gets the indices of all the <see cref="ParameterDefinition"/>s that are considered to be part of a method at given index.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="methodDefIndex">The index of method in <see cref="CILMetaData.MethodDefinitions"/> table.</param>
   /// <returns>The enumerable that will iterate over indices of parameters of the methods. Will be empty if <paramref name="md"/> is <c>null</c>, the method has no parameters, the <paramref name="methodDefIndex"/> is out of range, or the possible next item in <see cref="CILMetaData.MethodDefinitions"/> will have bad <see cref="MethodDefinition.ParameterList"/> index.</returns>
   /// <seealso cref="GetMethodParameters"/>
   /// <remarks>
   /// The returned enumerable is reactive in a sense that modifications done to tables e.g. after first iteration but before second iteration will affect the results of second iteration.
   /// </remarks>
   public static IEnumerable<Int32> GetMethodParameterIndices( this CILMetaData md, Int32 methodDefIndex )
   {
      return md == null ? Empty<Int32>.Enumerable : md.MethodDefinitions.GetTargetIndicesForAscendingReferenceListTable( methodDefIndex, md.ParameterDefinitions.GetRowCount(), mdef => mdef.ParameterList.Index );
   }

   /// <summary>
   /// Gets the indices of all the <see cref="PropertyDefinition"/>s that are considered to be part of a property mapping at given index.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="propertyMapIndex">The index of method in <see cref="CILMetaData.PropertyMaps"/> table.</param>
   /// <returns>The enumerable that will iterate over indices of properties of the property mapping. Will be empty if <paramref name="md"/> is <c>null</c>, the property mapping has no properties, the <paramref name="propertyMapIndex"/> is out of range, or the possible next item in <see cref="CILMetaData.PropertyMaps"/> will have bad <see cref="PropertyMap.PropertyList"/> index.</returns>
   /// <seealso cref="GetPropertyMapProperties"/>
   /// <remarks>
   /// The returned enumerable is reactive in a sense that modifications done to tables e.g. after first iteration but before second iteration will affect the results of second iteration.
   /// </remarks>
   public static IEnumerable<Int32> GetPropertyMapPropertyIndices( this CILMetaData md, Int32 propertyMapIndex )
   {
      return md == null ? Empty<Int32>.Enumerable : md.PropertyMaps.GetTargetIndicesForAscendingReferenceListTable( propertyMapIndex, md.PropertyDefinitions.GetRowCount(), pMap => pMap.PropertyList.Index );
   }

   /// <summary>
   /// Gets the indices of all the <see cref="EventDefinition"/>s that are considered to be part of a event mapping at given index.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="eventMapIndex">The index of method in <see cref="CILMetaData.EventMaps"/> table.</param>
   /// <returns>The enumerable that will iterate over indices of events of the event mapping. Will be empty if <paramref name="md"/> is <c>null</c>, the event mapping has no events, the <paramref name="eventMapIndex"/> is out of range, or the possible next item in <see cref="CILMetaData.EventMaps"/> will have bad <see cref="EventMap.EventList"/> index.</returns>
   /// <seealso cref="GetEventMapEvents"/>
   /// <remarks>
   /// The returned enumerable is reactive in a sense that modifications done to tables e.g. after first iteration but before second iteration will affect the results of second iteration.
   /// </remarks>
   public static IEnumerable<Int32> GetEventMapEventIndices( this CILMetaData md, Int32 eventMapIndex )
   {
      return md == null ? Empty<Int32>.Enumerable : md.EventMaps.GetTargetIndicesForAscendingReferenceListTable( eventMapIndex, md.EventDefinitions.GetRowCount(), eMap => eMap.EventList.Index );
   }

   /// <summary>
   /// Generic method to fetch indices of rows from target table, when source table references a continuous list of target rows with single item.
   /// These kind of references are <see cref="TypeDefinition.FieldList"/>, <see cref="TypeDefinition.MethodList"/>, <see cref="MethodDefinition.ParameterList"/>, <see cref="EventMap.EventList"/> and <see cref="PropertyMap.PropertyList"/>.
   /// </summary>
   /// <typeparam name="T">The type of source table rows.</typeparam>
   /// <param name="mdTableWithReferences">The source table.</param>
   /// <param name="tableWithReferencesIndex">The index of row in source table.</param>
   /// <param name="targetTableCount">The target table count.</param>
   /// <param name="referenceExtractor">The callback to extract target table index from row in source table.</param>
   /// <returns>The enumerable that will return indices in target table. Will be empty if <paramref name="mdTableWithReferences"/> is <c>null</c>.</returns>
   /// <seealso cref="GetTypeFields"/>
   /// <seealso cref="GetTypeFieldIndices"/>
   /// <seealso cref="GetTypeMethods"/>
   /// <seealso cref="GetTypeMethodIndices"/>
   /// <seealso cref="GetMethodParameters"/>
   /// <seealso cref="GetMethodParameterIndices"/>
   /// <seealso cref="GetEventMapEvents"/>
   /// <seealso cref="GetEventMapEventIndices"/>
   /// <seealso cref="GetPropertyMapProperties"/>
   /// <seealso cref="GetPropertyMapPropertyIndices"/>
   /// <remarks>
   /// <para>
   /// The returned enumerable is reactive in a sense that modifications done to tables e.g. after first iteration but before second iteration will affect the results of second iteration.
   /// </para>
   /// <para>
   /// This method is intended to be used by libraries providing additional metadata tables.
   /// </para>
   /// </remarks>
   /// <exception cref="ArgumentNullException">If <paramref name="referenceExtractor"/> is <c>null</c> when it is needed.</exception>
   public static IEnumerable<Int32> GetTargetIndicesForAscendingReferenceListTable<T>(
      this MetaDataTable<T> mdTableWithReferences,
      Int32 tableWithReferencesIndex,
      Int32 targetTableCount,
      Func<T, Int32> referenceExtractor
      )
      where T : class
   {
      Int32 min, max;
      if ( mdTableWithReferences.TryGetTargetIndicesBoundsForAscendingReferenceListTable( tableWithReferencesIndex, targetTableCount, referenceExtractor, out min, out max ) )
      {
         while ( min < max )
         {
            yield return min;
            ++min;
         }
      }
   }

   /// <summary>
   /// This is helper method to extract inclusive minimum and exclusive maximum boundaries for continuous list references, such as e.g. <see cref="TypeDefinition.FieldList"/>.
   /// </summary>
   /// <typeparam name="T">The type of rows in <see cref="MetaDataTable{TRow}"/>.</typeparam>
   /// <param name="mdTableWithReferences">The source table.</param>
   /// <param name="tableWithReferencesIndex">The index of row in source table.</param>
   /// <param name="targetTableCount">The target table count.</param>
   /// <param name="referenceExtractor">The callback to extract target table index from row in source table.</param>
   /// <param name="min">This parameter will hold the inclusive minimum index for target table.</param>
   /// <param name="max">This parameter will hold the exclusive maximum index for target table.</param>
   /// <returns><c>true</c> if <paramref name="mdTableWithReferences"/> is not null and has row at <paramref name="tableWithReferencesIndex"/>; <c>false</c> otherwise.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="referenceExtractor"/> is <c>null</c> when it is needed.</exception>
   public static Boolean TryGetTargetIndicesBoundsForAscendingReferenceListTable<T>(
      this MetaDataTable<T> mdTableWithReferences,
      Int32 tableWithReferencesIndex,
      Int32 targetTableCount,
      Func<T, Int32> referenceExtractor,
      out Int32 min,
      out Int32 max
      )
      where T : class
   {
      Boolean retVal;
      if ( mdTableWithReferences == null )
      {
         min = max = -1;
         retVal = false;
      }
      else
      {
         retVal = mdTableWithReferences.TableContents.TryGetTargetIndicesBoundsForAscendingReferenceListTable(
            tableWithReferencesIndex,
            targetTableCount,
            referenceExtractor,
            out min,
            out max
            );
      }
      return retVal;
   }

   internal static Boolean TryGetTargetIndicesBoundsForAscendingReferenceListTable<T>(
      this List<T> tableWithReferences,
      Int32 tableWithReferencesIndex,
      Int32 targetTableCount,
      Func<T, Int32> referenceExtractor,
      out Int32 min,
      out Int32 max
      )
      where T : class
   {
      var retVal = tableWithReferences != null
         && tableWithReferencesIndex >= 0
         && tableWithReferencesIndex < tableWithReferences.Count;
      if ( retVal )
      {
         ArgumentValidator.ValidateNotNull( "Reference extractor", referenceExtractor );

         min = referenceExtractor( tableWithReferences[tableWithReferencesIndex] );
         max = tableWithReferencesIndex < tableWithReferences.Count - 1 ?
            referenceExtractor( tableWithReferences[tableWithReferencesIndex + 1] ) :
            targetTableCount;
      }
      else
      {
         min = -1;
         max = -1;
      }
      return retVal;
   }

   /// <summary>
   /// Gets all the <see cref="FileReference"/>s that are marked to be containing meta data.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <returns>All the <see cref="FileReference"/>s that are marked to be containing meta data. Will be empty if <paramref name="md"/> is <c>null</c>.</returns>
   public static IEnumerable<FileReference> GetModuleFileReferences( this CILMetaData md )
   {
      return md?.FileReferences?.TableContents?.Where( f => f.Attributes.ContainsMetadata() ) ?? Empty<FileReference>.Enumerable;
   }

   /// <summary>
   /// Given the index of possibly enum type, tries to get the index of the field that is the enum value field.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="tDefIndex">The index of type in <see cref="CILMetaData.TypeDefinitions"/> table.</param>
   /// <param name="enumValueFieldIndex">If succeeded, this parameter will contain the index of the field in <see cref="CILMetaData.FieldDefinitions"/> table. Otherwise it will be <c>-1</c>.</param>
   /// <returns><c>true</c> if succeeded; <c>false</c> otherwise.</returns>
   /// <remarks>
   /// Current logic is to check that the base type of <see cref="TypeDefinition"/> at given index is <c>System.Enum</c>, and then to try fetch the first non-static field of the given type.
   /// </remarks>
   public static Boolean TryGetEnumValueFieldIndex(
      this CILMetaData md,
      Int32 tDefIndex,
      out Int32 enumValueFieldIndex
      )
   {
      var typeRow = md?.TypeDefinitions?.GetOrNull( tDefIndex );
      enumValueFieldIndex = -1;
      if ( typeRow != null && md.IsEnum( typeRow.BaseType ) )
      {
         // First non-static field of enum type is the field containing enum value
         var fDefs = md.FieldDefinitions.TableContents;
         enumValueFieldIndex = md.GetTypeFieldIndices( tDefIndex )
            .Where( i => i < fDefs.Count && !fDefs[i].Attributes.IsStatic() )
            .FirstOrDefaultCustom( -1 );
      }

      return enumValueFieldIndex >= 0;
   }

   /// <summary>
   /// Checks whether type at given index is considered to be a <c>System.Enum</c> type.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="tIdx">The <see cref="TableIndex"/> nullable.</param>
   /// <returns><c>true</c> if <paramref name="md"/> and <paramref name="tIdx"/> are both non-<c>null</c>, and the <see cref="TableIndex.Table"/> of <paramref name="tIdx"/> is <see cref="Tables.TypeDef"/> or <see cref="Tables.TypeRef"/>, and that the type is considered to be <c>System.Enum</c> by <see cref="IsSystemType"/> method; <c>false</c> otherwise.</returns>
   public static Boolean IsEnum( this CILMetaData md, TableIndex? tIdx )
   {
      return md.IsSystemType( tIdx, Consts.ENUM_NAMESPACE, Consts.ENUM_TYPENAME );
   }

   /// <summary>
   /// Checks whether type at given index is considered to be a <c>System.Type</c> type.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="tIdx">The <see cref="TableIndex"/> nullable.</param>
   /// <returns><c>true</c> if <paramref name="md"/> and <paramref name="tIdx"/> are both non-<c>null</c>, and the <see cref="TableIndex.Table"/> of <paramref name="tIdx"/> is <see cref="Tables.TypeDef"/> or <see cref="Tables.TypeRef"/>, and that the type is considered to be <c>System.Type</c> by <see cref="IsSystemType"/> method; <c>false</c> otherwise.</returns>
   public static Boolean IsTypeType( this CILMetaData md, TableIndex? tIdx )
   {
      return md.IsSystemType( tIdx, Consts.TYPE_NAMESPACE, Consts.TYPE_TYPENAME );
   }

   /// <summary>
   /// Checks whether type at given index is considered to be a <c>System.Object</c> type.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="tIdx">The <see cref="TableIndex"/> nullable.</param>
   /// <returns><c>true</c> if <paramref name="md"/> and <paramref name="tIdx"/> are both non-<c>null</c>, and the <see cref="TableIndex.Table"/> of <paramref name="tIdx"/> is <see cref="Tables.TypeDef"/> or <see cref="Tables.TypeRef"/>, and that the type is considered to be <c>System.Object</c> by <see cref="IsSystemType"/> method; <c>false</c> otherwise.</returns>
   public static Boolean IsSystemObjectType( this CILMetaData md, TableIndex tIdx )
   {
      return md.IsSystemType( tIdx, Consts.SYSTEM_OBJECT_NAMESPACE, Consts.SYSTEM_OBJECT_TYPENAME );
   }

   /// <summary>
   /// Checks whether type at given table index is a system type with given namespace and name.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="tIdx">The <see cref="TableIndex"/> nullable.</param>
   /// <param name="systemNS">The namespace that the type should have.</param>
   /// <param name="systemTN">The name that the type should have.</param>
   /// <returns>
   /// <c>true</c> if <paramref name="md"/> and <paramref name="tIdx"/> are both non-<c>null</c>, and the <see cref="TableIndex.Table"/> of <paramref name="tIdx"/> is <see cref="Tables.TypeDef"/> or <see cref="Tables.TypeRef"/>, and that the <see cref="TypeReference.ResolutionScope"/> in case of <see cref="Tables.TypeRef"/> is <see cref="Tables.ModuleRef"/> or <see cref="Tables.AssemblyRef"/>, and if the namespace and name match; <c>false</c> otherwise.
   /// </returns>
   /// <remarks>
   /// No checks are done to parent type.
   /// </remarks>
   public static Boolean IsSystemType(
      this CILMetaData md,
      TableIndex? tIdx,
      String systemNS,
      String systemTN
      )
   {
      var result = md != null;
      if ( result )
      {
         result = tIdx.HasValue && tIdx.Value.Table != Tables.TypeSpec;

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
      }
      return result;
   }

   /// <summary>
   /// This method will return enumerable returning the full type names of types in <see cref="CILMetaData.TypeDefinitions"/> table, taking the nested types into account.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <returns>Enumerable with full type names of types in <see cref="CILMetaData.TypeDefinitions"/> table. Will be empty if the table is empty or if <paramref name="md"/> is <c>null</c>.</returns>
   /// <remarks>
   /// The returned enumerable is reactive in a sense that modifications done to tables e.g. after first iteration but before second iteration will affect the results of second iteration.
   /// </remarks>
   public static IEnumerable<String> GetTypeDefinitionsFullNames( this CILMetaData md )
   {
      if ( md != null )
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
   }

   /// <summary>
   /// Gets a row located at given <see cref="TableIndex"/>.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="index">The <see cref="TableIndex"/>.</param>
   /// <returns>The row at given table index.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="md"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentOutOfRangeException">If <paramref name="md"/> had the table present, but the index was out of range.</exception>
   /// <exception cref="InvalidOperationException">If <paramref name="md"/> did not have the table present.</exception>
   public static Object GetByTableIndex( this CILMetaData md, TableIndex index )
   {
      Object retVal;
      if ( !md.TryGetByTableIndex( index, out retVal ) )
      {
         MetaDataTable tbl;
         if ( md.TryGetByTable( (Int32) index.Table, out tbl ) )
         {
            throw new ArgumentOutOfRangeException( "Table index " + index + " was out of range." );
         }
         else
         {
            throw new InvalidOperationException( "The table " + index.Table + " does not have representation in this meta data." );
         }
      }

      return retVal;
   }


   /// <summary>
   /// Tries to retrieve a row at given <see cref="TableIndex"/>.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="index">The <see cref="TableIndex"/>.</param>
   /// <param name="row">This parameter will have the row, if the operation is succesful; otherwise it will be <c>null</c>. Use the return value to differentiate row which was fetched but still was <c>null</c>.</param>
   /// <returns><c>true</c> if <paramref name="md"/> was not <c>null</c>, had a representation of <see cref="TableIndex.Table"/> of <paramref name="index"/>, and <see cref="TableIndex.Index"/> pointed into existing item in the <see cref="MetaDataTable.TableContentsNotGeneric"/>; <c>false</c> otherwise.</returns>
   public static Boolean TryGetByTableIndex( this CILMetaData md, TableIndex index, out Object row )
   {
      var retVal = md != null;
      if ( retVal )
      {
         MetaDataTable table;
         retVal = md.TryGetByTable( (Int32) index.Table, out table );

         if ( retVal )
         {
            var list = table.TableContentsNotGeneric;
            if ( index.Index <= list.Count )
            {
               row = list[index.Index];
            }
            else
            {
               retVal = false;
               row = null;
            }
         }
         else
         {
            row = null;
         }
      }
      else
      {
         row = null;
      }

      return retVal;
   }

   /// <summary>
   /// Gets the index of the row which the row would have it would be appended to a given table.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="table">The <see cref="Tables"/>.</param>
   /// <returns>A new <see cref="TableIndex"/> with <see cref="TableIndex.Table"/> being given <paramref name="table"/>, and <see cref="TableIndex.Index"/> being the row count of the given table in <paramref name="md"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="md"/> is <c>null</c>.</exception>
   public static TableIndex GetNextTableIndexFor( this CILMetaData md, Tables table )
   {
      return new TableIndex( table, md.GetByTable( (Int32) table ).GetRowCount() );
   }

   /// <summary>
   /// This method will re-order some tables and modify the rows and signatures of the rows appropriately.
   /// The rules for re-ordering are the ones found in ECMA-335 standard, and after this method completes, the given <see cref="CILMetaData"/> will be adhering to these rules.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <returns>An array containinting re-ordering information for each table. The first index of array is table index, and then the integer at index <c>x</c> will tell the new index of the row that previously was at index <c>x</c>.</returns>
   /// <remarks>
   /// <para>
   /// This method assumes that each row in each metadata table is not <c>null</c>, and will produce incorrect results, if that is the case.
   /// </para>
   /// <para>
   /// The following tables are not checked for duplicates:
   /// <list type="bullet">
   /// <item><description><see cref="Tables.TypeDef"/>,</description></item>
   /// <item><description><see cref="Tables.Field"/>,</description></item>
   /// <item><description><see cref="Tables.MethodDef"/>,</description></item>
   /// <item><description><see cref="Tables.Property"/>,</description></item>
   /// <item><description><see cref="Tables.Event"/>, and</description></item>
   /// <item><description><see cref="Tables.ExportedType"/>.</description></item>
   /// </list>
   /// </para>
   /// </remarks>
   /// <exception cref="NullReferenceException">If <paramref name="md"/> is <c>null</c>.</exception>
   public static Int32[][] OrderTablesAndRemoveDuplicates( this CILMetaData md )
   {
      // TODO maybe just create a new CILMetaData which would be a sorted version of this??
      // Would simplify a lot of things, and possibly could be even faster (unless given md is already in order)


      //var allTableIndices = new Int32[Consts.AMOUNT_OF_TABLES][];
      var reorderState = new MetaDataReOrderState( md );

      // Start by re-ordering structural (TypeDef, MethodDef, ParamDef, Field, NestedClass) tables
      // Phase 1: ReOrderTablesThatCantHaveDuplicates
      reorderState.ReOrderStructuralTables();

      // Keep updating and removing duplicates from TypeRef, TypeSpec, MemberRef, MethodSpec, StandaloneSignature and Property tables, while updating all signatures and IL code
      // Phase 2. RemoveDuplicatesAndUpdateAttachedDataStructures
      reorderState.UpdateSignaturesAndILWhileRemovingDuplicates();

      // Update and sort the remaining tables which don't have signatures
      // Phase 3. SortTablesAndUpdateReferences
      reorderState.UpdateAndSortTablesWithNoSignatures();

      // Remove duplicates
      // This is not callbackable phase - do it right here
      reorderState.RemoveDuplicatesAfterSorting();

      // Sort exception blocks of all ILs
      // This should be in phase 3
      md.SortMethodILExceptionBlocks();

      // TODO Extra tables!

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
      var propMap = md.PropertyMaps;
      var evtMap = md.EventMaps;
      var tDefCount = typeDef.GetRowCount();
      var mDefCount = methodDef.GetRowCount();
      var fDefCount = fieldDef.GetRowCount();
      var pDefCount = paramDef.GetRowCount();
      var ncCount = nestedClass.GetRowCount();

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

         // Update PropertyMap and EventMap table indices
         reorderState.UpdateMDTableWithTableIndices1(
            propMap,
            p => p.Parent,
            ( p, t ) => p.Parent = t
            );

         reorderState.UpdateMDTableWithTableIndices1(
            evtMap,
            e => e.Parent,
            ( e, t ) => e.Parent = t
            );
      }

      reorderState.ReOrderMDTableWithAscendingReferencesAndUniqueColumn(
         propMap,
         md.PropertyDefinitions,
         ComparerFromFunctions.NewEqualityComparer<PropertyMap>( ( x, y ) => x.Parent.Equals( y.Parent ), x => x.Parent.GetHashCode() ),
         p => p.PropertyList.Index,
         ( p, i ) => p.PropertyList = new TableIndex( Tables.Property, i )
         );

      reorderState.ReOrderMDTableWithAscendingReferencesAndUniqueColumn(
         evtMap,
         md.EventDefinitions,
         ComparerFromFunctions.NewEqualityComparer<EventMap>( ( x, y ) => x.Parent.Equals( y.Parent ), x => x.Parent.GetHashCode() ),
         e => e.EventList.Index,
         ( e, i ) => e.EventList = new TableIndex( Tables.Event, i )
         );

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

      // Sort InterfaceImpl table ( Class, Interface)      
      reorderState.UpdateMDTableIndices(
         md.InterfaceImplementations,
         ( iFaceImpl, indices ) =>
         {
            reorderState.UpdateMDTableWithTableIndices2( iFaceImpl, i => i.Class, ( i, c ) => i.Class = c, i => i.Interface, ( i, iface ) => i.Interface = iface );
            reorderState.CheckMDDuplicatesUnsorted( md.InterfaceImplementations );
         } );
      reorderState.FixDuplicatesAfterSorting( Tables.InterfaceImpl );

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
      var count = mdTable.GetRowCount();
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
         md.GetByTable( (Int32) table ).RemoveDuplicatesFromTable( indices );
      }
   }

   private static void RemoveDuplicatesFromTable( this MetaDataTable mdTable, IDictionary<Int32, Int32> indices )
   {
      var table = mdTable.TableContentsNotGeneric;
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

   private static void FixDuplicatesAfterSorting( this MetaDataReOrderState reorderState, Tables table )
   {
      IDictionary<Int32, Int32> dupInfo;
      if ( reorderState.Duplicates.TryGetValue( table, out dupInfo ) )
      {
         var newDupInfo = new Dictionary<Int32, Int32>();
         var indices = reorderState.FinalIndices[(Int32) table];
         foreach ( var kvp in dupInfo )
         {
            var dupIndex = kvp.Key;
            var actualIndex = kvp.Value;
            newDupInfo.Add( indices[dupIndex], indices[actualIndex] );
         }
         reorderState.Duplicates[table] = newDupInfo;
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
         var count = mdTable.GetRowCount();
         if ( count > 1 )
         {
            // 1. Make a copy of array
            var table = mdTable.TableContents;
            var copy = table.ToArray();

            // 2. Sort original array
            table.Sort( comparer );

            // 3. For each element, do a binary search to find where it is now after sorting
            var nullCount = 0;
            for ( var i = 0; i < count; ++i )
            {
               var idx = table.BinarySearchDeferredEqualityDetection( copy[i], comparer );
               if ( copy[i] == null )
               {
                  // Duplicate
                  idx += nullCount;
                  ++nullCount;
               }
               else
               {
                  while ( !ReferenceEquals( copy[i], table[idx] ) )
                  {
                     ++idx;
                  }
               }
               indices[i] = idx;
            }
         }

      }
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
      var refTableCount = referencingMDTable.GetRowCount();
      var thisTableCount = mdTable.GetRowCount();

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

   private static void ReOrderMDTableWithAscendingReferencesAndUniqueColumn<T, U>(
      this MetaDataReOrderState reorderState,
      MetaDataTable<T> sourceTable,
      MetaDataTable<U> targetTable,
      IEqualityComparer<T> sourceTableEqualityComparer,
      Func<T, Int32> sourceGetter,
      Action<T, Int32> sourceSetter
      )
      where T : class
      where U : class
   {
      // This is for situations when e.g. PropertyMap has several rows for same type

      // First, remove duplicates from source table
      // Create a copy of list, since duplicates are removed right away and old values are not preserved
      var sourceContents = sourceTable.TableContents;
      var sourceCopy = new List<T>( sourceContents );
      if ( reorderState.CheckMDDuplicatesUnsorted(
         sourceTable,
         sourceTableEqualityComparer
         ) > 0 )
      {
         // There were duplicates - have to reorder the target table
         var duplicates = reorderState.Duplicates[(Tables) sourceTable.GetTableIndex()];
         var originals = new Dictionary<Int32, List<Int32>>();
         foreach ( var kvp in duplicates )
         {
            originals.GetOrAdd_NotThreadSafe( kvp.Value, i => new List<Int32>() ).Add( kvp.Key );
         }

         var targetIndex = 0;
         var targetContents = targetTable.TableContents;
         var targetCopy = targetContents.ToArray();
         var targetIndices = reorderState.GetOrCreateIndexArray( targetTable );

         // We can't modify the ref indices as we go, since that would mess up things
         var sourceRefs = new Int32[sourceCopy.Count];
         for ( var i = 0; i < sourceCopy.Count; ++i )
         {
            List<Int32> duplicateIndices;

            var row = sourceContents[i];
            if ( row != null )
            {
               sourceRefs[i] = targetIndex;

               Int32 min, max;
               if ( sourceCopy.TryGetTargetIndicesBoundsForAscendingReferenceListTable( i, targetCopy.Length, sourceGetter, out min, out max ) )
               {
                  while ( min < max )
                  {
                     targetContents[targetIndex] = targetCopy[min];
                     targetIndices[min] = targetIndex;
                     ++min;
                     ++targetIndex;
                  }
               }


               if ( originals.TryGetValue( i, out duplicateIndices ) )
               {
                  // This row has duplicates later, so need to 
                  foreach ( var dupIdx in duplicateIndices )
                  {
                     if ( sourceCopy.TryGetTargetIndicesBoundsForAscendingReferenceListTable( dupIdx, targetCopy.Length, sourceGetter, out min, out max ) )
                     {
                        while ( min < max )
                        {
                           targetContents[targetIndex] = targetCopy[min];
                           targetIndices[min] = targetIndex;
                           ++min;
                           ++targetIndex;
                        }
                     }
                  }
               }
            }
         }

         for ( var i = 0; i < sourceCopy.Count; ++i )
         {
            var row = sourceContents[i];
            if ( row != null )
            {
               sourceSetter( row, sourceRefs[i] );
            }
         }
      }
   }

   private static Int32 CheckMDDuplicatesUnsorted<T>(
      this MetaDataReOrderState reorderState,
      MetaDataTable<T> mdTable,
      IEqualityComparer<T> comparer = null,
      Int32? start = null,
      Int32? end = null
      )
      where T : class
   {
      var list = mdTable.TableContents;
      var table = mdTable.GetTableIndex();
      var count = list.Count;
      var indices = reorderState.GetOrCreateIndexArray( mdTable );
      var removedDuplicates = 0;
      if ( count > 1 )
      {
         var dic = new Dictionary<T, Int32>( comparer ?? mdTable.TableInformation.EqualityComparer );
         var actualStart = start ?? 0;
         var actualEnd = end ?? list.Count;
         for ( var i = actualStart; i < actualEnd; ++i )
         {
            var cur = list[i];
            if ( cur != null )
            {
               Int32 actualIndex;
               if ( dic.TryGetValue( cur, out actualIndex ) )
               {
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

                  ++removedDuplicates;
               }
               else
               {
                  dic.Add( cur, i );
               }
            }

         }
      }

      return removedDuplicates;
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


      //// Now, remove duplicates from FieldDef table
      //// This has to be done *after* modifying signatures - otherwise duplicate matching based on signatures won't work properly
      //// But this has to be done *before* modifying IL - as IL may have references to the fields
      //// Remove duplicate fields (name + signature + owner)
      //var tDefsTable = md.TypeDefinitions;
      //var tDefs = tDefsTable.TableContents;
      //var fDefsTable = md.FieldDefinitions;
      //var fDefsCount = fDefsTable.GetRowCount();
      //var fListOffset = 0;
      //for ( var i = 0; i < tDefs.Count; ++i )
      //{
      //   Int32 min, max;
      //   var hasFields = tDefsTable.TryGetTargetIndicesBoundsForAscendingReferenceListTable( i, fDefsCount, tDef => tDef.FieldList.Index, out min, out max );

      //   if ( fListOffset > 0 )
      //   {
      //      // Update this tDef
      //      var tDef = tDefs[i];
      //      tDef.FieldList = tDef.FieldList.IncrementIndex( -fListOffset );
      //   }

      //   if ( hasFields && max > min )
      //   {
      //      var duplicateCount = reorderState.CheckMDDuplicatesUnsorted(
      //         fDefsTable,
      //         ComparerFromFunctions.NewEqualityComparer<FieldDefinition>(
      //            ( x, y ) =>
      //            ReferenceEquals( x, y ) ||
      //            ( x != null && y != null // In order to be duplicate, both fields must not be compiler-controlled, and
      //               && !x.Attributes.IsCompilerControlled()
      //               && !y.Attributes.IsCompilerControlled()
      //               && String.Equals( x.Name, y.Name ) // Both names must match, and
      //               && Comparers.FieldSignatureEqualityComparer.Equals( x.Signature, y.Signature ) // Both signatures must match
      //            ),
      //            x => x.Name.GetHashCodeSafe()
      //            ),
      //         min,
      //         max
      //      );

      //      fListOffset += duplicateCount;

      //   }
      //}

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
         case SignatureKind.Raw:
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

   private static IEnumerable<TSigInfo> GetAllSignaturesToUpdateForReOrder_LocalOrSig( this ParameterOrLocalSignature sig )
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
            foreach ( var code in il.OpCodes.Where( code => code.InfoKind == OpCodeInfoKind.OperandTableIndex ) )
            {
               var codeInfo = (OpCodeInfoWithTableIndex) code;
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

   /// <summary>
   /// This method will calculate the max stack size suitable to use for <see cref="MethodILDefinition.MaxStackSize"/>.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="methodIndex">The zero-based index in <see cref="CILMetaData.MethodDefinitions"/> table for which to calculate max stack size for.</param>
   /// <returns>The max stack size for method in <paramref name="md"/> in table <see cref="CILMetaData.MethodDefinitions"/> at index <paramref name="methodIndex"/>. If <paramref name="methodIndex"/> is invalid or if the <see cref="MethodDefinition"/> does not have IL, returns <c>-1</c>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="md"/> is <c>null</c>.</exception>
   public static Int32 CalculateStackSize( this CILMetaData md, Int32 methodIndex )
   {
      var mDef = md.MethodDefinitions.GetOrNull( methodIndex );
      var retVal = -1;
      if ( mDef != null )
      {
         var il = mDef.IL;
         if ( il != null )
         {

            var ocp = md.OpCodeProvider;
            var state = new StackCalculationState( md, il.OpCodes.Sum( oc => oc.GetTotalByteCount( ocp ) ) );

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
               var byteCount = codeInfo.GetTotalByteCount( ocp );
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
      var code = state.MD.OpCodeProvider.GetCodeFor( codeInfo.OpCodeID );
      var curStacksize = Math.Max( state.CurrentStack, state.StackSizes[state.CurrentCodeByteOffset] );
      if ( FlowControl.Call == code.FlowControl )
      {
         curStacksize = UpdateStackSizeForMethod( state, code, ( (OpCodeInfoWithTableIndex) codeInfo ).Operand, curStacksize );
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
               var offsets = ( (OpCodeInfoWithIntegers) codeInfo ).Operand;
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
         var isNewObj = code.OpCodeID == OpCodeID.Newobj;
         if ( sig.MethodSignatureInformation.IsHasThis() && !isNewObj )
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

         if ( code.OpCodeID == OpCodeID.Calli )
         {
            // Pop function pointer
            --curStacksize;
         }

         var rType = sig.ReturnType.Type;

         // TODO we could check here for stack underflow!

         if ( isNewObj
            || rType.TypeSignatureKind != TypeSignatureKind.Simple
            || ( (SimpleTypeSignature) rType ).SimpleType != SimpleTypeSignatureKind.Void
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



   /// <summary>
   /// Gets textual representation of the version information contained in <paramref name="info"/>. The format is: "<c>&lt;major&gt;.&lt;minor&gt;.&lt;build&gt;.&lt;revision&gt;</c>".
   /// </summary>
   /// <param name="info">The assembly information containing version information.</param>
   /// <returns>Textual representation of the version information contained in <paramref name="info"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="info"/> is <c>null</c>.</exception>
   public static String GetVersionString( this AssemblyInformation info )
   {
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
   /// <exception cref="NullReferenceException">If <paramref name="info"/> is <c>null</c>.</exception>
   public static String GetCultureString( this AssemblyInformation info )
   {
      var culture = info.Culture;
      return String.IsNullOrEmpty( culture ) ? AssemblyInformation.NEUTRAL_CULTURE : culture;
   }

}