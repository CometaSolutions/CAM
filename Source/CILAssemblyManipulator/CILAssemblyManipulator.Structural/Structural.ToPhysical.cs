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
   private sealed class PhysicalCreationState
   {
      private readonly CILMetaData _md;
      private readonly IDictionary<TypeDefinitionStructure, Int32> _typeDefs;
      //private readonly IDictionary<TypeReferenceStructure, Int32> _typeRefs;
      //private readonly IDictionary<TypeSpecificationStructure, Int32> _typeSpecs;

      internal PhysicalCreationState( CILMetaData md, ModuleStructure module )
      {
         this._md = md;
         this._typeDefs = new Dictionary<TypeDefinitionStructure, Int32>( ReferenceEqualityComparer<TypeDefinitionStructure>.ReferenceBasedComparer );
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

      public IDictionary<TypeDefinitionStructure, Int32> TypeDefs
      {
         get
         {
            return this._typeDefs;
         }
      }

      //public IDictionary<TypeReferenceStructure, Int32> TypeRefs
      //{
      //   get
      //   {
      //      return this._typeRefs;
      //   }
      //}

      //public IDictionary<TypeSpecificationStructure, Int32> TypeSpecs
      //{
      //   get
      //   {
      //      return this._typeSpecs;
      //   }
      //}

      public IDictionary<FieldStructure, Int32> FieldDefs
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public IDictionary<MethodStructure, Int32> MethodDefs
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public IDictionary<ParameterStructure, Int32> ParamDefs
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
      var tDefIdx = tDefs.Count;

      // TypeDef
      tDefs.Add( new TypeDefinition()
      {
         Attributes = typeDef.Attributes,
         FieldList = md.GetNextTableIndexFor( Tables.Field ),
         MethodList = md.GetNextTableIndexFor( Tables.MethodDef ),
         Name = typeDef.Name,
         Namespace = typeDef.Namespace
      } );
      state.TypeDefs.Add( typeDef, tDefIdx );

      // NestedClass
      if ( enclosingType != null )
      {
         md.NestedClassDefinitions.TableContents.Add( new NestedClassDefinition()
         {
            NestedClass = new TableIndex( Tables.TypeDef, tDefIdx ),
            EnclosingClass = new TableIndex( Tables.TypeDef, state.TypeDefs[enclosingType] )
         } );
      }

      // Fields
      var fDefs = md.FieldDefinitions.TableContents;
      var fDefIdx = fDefs.Count;
      foreach ( var field in typeDef.Fields )
      {
         fDefs.Add( new FieldDefinition()
         {
            Attributes = field.Attributes,
            Name = field.Name
         } );
         state.FieldDefs.Add( field, fDefIdx );
         ++fDefIdx;
      }

      // Methods and Parameters
      var mDefs = md.MethodDefinitions.TableContents;
      var mDefIdx = mDefs.Count;
      var pDefs = md.ParameterDefinitions.TableContents;
      var pDefIdx = pDefs.Count;
      foreach ( var method in typeDef.Methods )
      {
         mDefs.Add( new MethodDefinition()
         {
            Attributes = method.Attributes,
            ImplementationAttributes = method.ImplementationAttributes,
            Name = method.Name,
            ParameterList = new TableIndex( Tables.Parameter, pDefIdx )
         } );
         state.MethodDefs.Add( method, mDefIdx );
         ++mDefIdx;

         foreach ( var param in method.Parameters )
         {
            pDefs.Add( new ParameterDefinition()
            {
               Attributes = param.Attributes,
               Name = param.Name,
               Sequence = param.Sequence
            } );
            state.ParamDefs.Add( param, pDefIdx );
            ++pDefIdx;
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
         case TypeStructureKind.TypeRef:
         case TypeStructureKind.TypeSpec:
         default:
            throw new InvalidOperationException( "Invalid type structure kind: " + type.TypeStructureKind + "." );
      }
   }
}