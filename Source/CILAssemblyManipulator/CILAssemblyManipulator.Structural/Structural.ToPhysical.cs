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
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Structural;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static partial class E_CILStructural
{
   private sealed class TableIndexTracker<T>
      where T : class
   {
      private readonly Tables _table;
      private readonly IDictionary<T, Int32> _dic;
      private readonly Action<T, Int32> _afterAddition;

      internal TableIndexTracker( Tables table, Action<T, Int32> afterAddition )
      {
         this._table = table;
         this._dic = new Dictionary<T, Int32>( ReferenceEqualityComparer<T>.ReferenceBasedComparer );
         this._afterAddition = afterAddition;
      }

      public TableIndex Add( T obj )
      {
         if ( obj == null )
         {
            throw new InvalidOperationException( "Trying to add null to table " + this._table + "." );
         }

         var idx = this._dic.Count;
         this._dic.Add( obj, idx );
         this._afterAddition( obj, idx );
         return new TableIndex( this._table, idx );
      }

      public TableIndex Get( T obj )
      {
         TableIndex retVal;
         if ( !this.TryGet( obj, out retVal ) )
         {
            throw new InvalidOperationException( "The " + this._table + " object " + obj + " was not part of this module." );
         }
         return retVal;
      }

      public TableIndex GetOrAdd( T obj )
      {
         Int32 idx;
         return this._dic.TryGetValue( obj, out idx ) ?
            new TableIndex( this._table, idx ) :
            this.Add( obj );
      }

      public Boolean TryGet( T obj, out TableIndex index )
      {
         if ( obj == null )
         {
            throw new InvalidOperationException( "Null reference to table " + this._table + "." );
         }
         Int32 idx;
         var retVal = this._dic.TryGetValue( obj, out idx );
         index = retVal ? new TableIndex( this._table, idx ) : default( TableIndex );
         return retVal;
      }

      public Int32 Count
      {
         get
         {
            return this._dic.Count;
         }
      }
   }
   private sealed class PhysicalCreationState
   {
      private readonly CILMetaData _md;
      private readonly TableIndexTracker<TypeDefinitionStructure> _typeDefs;
      private readonly TableIndexTracker<FieldStructure> _fieldDefs;
      private readonly TableIndexTracker<MethodStructure> _methodDefs;
      private readonly TableIndexTracker<ParameterStructure> _paramDefs;
      //private readonly IDictionary<TypeReferenceStructure, Int32> _typeRefs;
      //private readonly IDictionary<TypeSpecificationStructure, Int32> _typeSpecs;

      internal PhysicalCreationState( CILMetaData md, ModuleStructure module )
      {
         this._md = md;
         this._typeDefs = new TableIndexTracker<TypeDefinitionStructure>( Tables.TypeDef, ( tDef, idx ) =>
            md.TypeDefinitions.TableContents.Add( new TypeDefinition()
            {
               Attributes = tDef.Attributes,
               FieldList = new TableIndex( Tables.Field, this._fieldDefs.Count ),
               MethodList = new TableIndex( Tables.MethodDef, this._methodDefs.Count ),
               Name = tDef.Name,
               Namespace = tDef.Namespace
            } ) );
         this._fieldDefs = new TableIndexTracker<FieldStructure>( Tables.Field, ( fDef, idx ) =>
            md.FieldDefinitions.TableContents.Add( new FieldDefinition()
            {
               Attributes = fDef.Attributes,
               Name = fDef.Name
            } ) );
         this._methodDefs = new TableIndexTracker<MethodStructure>( Tables.MethodDef, ( mDef, idx ) =>
            md.MethodDefinitions.TableContents.Add( new MethodDefinition()
            {
               Attributes = mDef.Attributes,
               ImplementationAttributes = mDef.ImplementationAttributes,
               Name = mDef.Name,
               ParameterList = new TableIndex( Tables.Parameter, this._paramDefs.Count )
            } ) );
         this._paramDefs = new TableIndexTracker<ParameterStructure>( Tables.Parameter, ( pDef, idx ) =>
            md.ParameterDefinitions.TableContents.Add( new ParameterDefinition()
            {
               Attributes = pDef.Attributes,
               Name = pDef.Name,
               Sequence = pDef.Sequence
            } ) );

         //this._typeRefs = new Dictionary<TypeReferenceStructure, Int32>( ReferenceEqualityComparer<TypeReferenceStructure>.ReferenceBasedComparer );
         //this._typeSpecs = new Dictionary<TypeSpecificationStructure, Int32>( ReferenceEqualityComparer<TypeSpecificationStructure>.ReferenceBasedComparer );

         this.PopulateStructualTables( module );
      }

      public CILMetaData MetaData
      {
         get
         {
            return this._md;
         }
      }

      public TableIndexTracker<TypeDefinitionStructure> TypeDefs
      {
         get
         {
            return this._typeDefs;
         }
      }

      public TableIndexTracker<TypeReferenceStructure> TypeRefs
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public TableIndexTracker<TypeSpecificationStructure> TypeSpecs
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public TableIndexTracker<FieldStructure> FieldDefs
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public TableIndexTracker<MethodStructure> MethodDefs
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public TableIndexTracker<ParameterStructure> ParamDefs
      {
         get
         {
            throw new NotImplementedException();
         }
      }
   }
   public static CILMetaData[] CreatePhysicalRepresentation( this AssemblyStructure assembly )
   {
      return assembly.Modules
         .Select( m => m.CreatePhysicalRepresentation( assembly ) )
         .ToArray();
   }

   public static CILMetaData CreatePhysicalRepresentation( this ModuleStructure module )
   {
      return module.CreatePhysicalRepresentation( null );
   }

   private static CILMetaData CreatePhysicalRepresentation( this ModuleStructure module, AssemblyStructure assembly )
   {
      var md = CILMetaDataFactory.CreateMinimalModule( module.Name );
      if ( module.IsMainModule && assembly != null )
      {
         var aDef = new AssemblyDefinition()
         {
            Attributes = assembly.Attributes,
            HashAlgorithm = assembly.HashAlgorithm
         };
         assembly.AssemblyInfo.DeepCopyContentsTo( aDef.AssemblyInformation );
      }

      var state = new PhysicalCreationState( md, module );


      return md;
   }

   private static void PopulateStructualTables( this PhysicalCreationState state, ModuleStructure module )
   {
      foreach ( var tDef in module.TopLevelTypeDefinitions )
      {
         state.AddTypeDefRelatedRows( tDef, null );
      }
   }

   private static void AddTypeDefRelatedRows( this PhysicalCreationState state, TypeDefinitionStructure typeDef, TypeDefinitionStructure enclosingType )
   {
      var md = state.MetaData;
      var tDefs = md.TypeDefinitions.TableContents;
      // TypeDef
      var tDefIdx = state.TypeDefs.Add( typeDef );

      // NestedClass
      if ( enclosingType != null )
      {
         md.NestedClassDefinitions.TableContents.Add( new NestedClassDefinition()
         {
            NestedClass = tDefIdx,
            EnclosingClass = state.TypeDefs.Get( enclosingType )
         } );
      }

      // Fields
      foreach ( var field in typeDef.Fields )
      {
         state.FieldDefs.Add( field );
      }

      // Methods and Parameters
      foreach ( var method in typeDef.Methods )
      {
         state.MethodDefs.Add( method );

         foreach ( var param in method.Parameters )
         {
            state.ParamDefs.Add( param );
         }
      }

      // Nested types
      foreach ( var nestedType in typeDef.NestedTypes )
      {
         state.AddTypeDefRelatedRows( nestedType, typeDef );
      }
   }

   private static void FillRestOfTheTables( this PhysicalCreationState state, ModuleStructure module, AssemblyStructure assembly )
   {

   }

   private static TableIndex FromTypeDefOrRefOrSpec( this PhysicalCreationState state, AbstractTypeStructure type )
   {
      if ( type == null )
      {
         throw new InvalidOperationException( "TypeDef/Ref/Spec was null when it wasn't supposed to be." );
      }

      switch ( type.TypeStructureKind )
      {
         case TypeStructureKind.TypeDef:
            return state.TypeDefs.Get( (TypeDefinitionStructure) type );
         case TypeStructureKind.TypeRef:
            return state.TypeRefs.Get( (TypeReferenceStructure) type );
         case TypeStructureKind.TypeSpec:
            return state.TypeSpecs.Get( (TypeSpecificationStructure) type );
         default:
            throw new InvalidOperationException( "Invalid type structure kind: " + type.TypeStructureKind + "." );
      }
   }
}