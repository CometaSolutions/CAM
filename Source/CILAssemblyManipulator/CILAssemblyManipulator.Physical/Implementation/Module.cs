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
using System.Linq;
using System.Text;
using CommonUtils;

namespace CILAssemblyManipulator.Physical.Implementation
{
   internal sealed class CILModuleDataImpl : CILModuleData
   {
      private readonly HeadersData _headers;
      private readonly CILMetaData _md;

      internal CILModuleDataImpl(
         HeadersData headers,
         CILMetaData md
         )
      {
         ArgumentValidator.ValidateNotNull( "Headers", headers );
         ArgumentValidator.ValidateNotNull( "Metadata", md );

         this._headers = headers;
         this._md = md;
      }

      public HeadersData Headers
      {
         get
         {
            return this._headers;
         }
      }

      public CILMetaData MetaData
      {
         get
         {
            return this._md;
         }
      }
   }

   internal sealed class CILMetadataImpl : CILMetaData
   {
      private static readonly Int32[] EMPTY_SIZES = Enumerable.Repeat( 0, Consts.AMOUNT_OF_TABLES ).ToArray();

      private readonly List<ModuleDefinition> _moduleDefinitions;
      private readonly List<TypeReference> _typeReferences;
      private readonly List<TypeDefinition> _typeDefinitions;
      private readonly List<FieldDefinition> _fieldDefinitions;
      private readonly List<MethodDefinition> _methodDefinitions;
      private readonly List<ParameterDefinition> _parameterDefinitions;
      private readonly List<InterfaceImplementation> _interfaceImplementations;
      private readonly List<MemberReference> _memberReferences;
      private readonly List<ConstantDefinition> _constantDefinitions;
      private readonly List<CustomAttributeDefinition> _customAttributeDefinitions;
      private readonly List<FieldMarshal> _fieldMarshals;
      private readonly List<SecurityDefinition> _securityDefinitions;
      private readonly List<ClassLayout> _classLayouts;
      private readonly List<FieldLayout> _fieldLayouts;
      private readonly List<StandaloneSignature> _standaloneSignatures;
      private readonly List<EventMap> _eventMaps;
      private readonly List<EventDefinition> _eventDefinitions;
      private readonly List<PropertyMap> _propertyMaps;
      private readonly List<PropertyDefinition> _propertyDefinitions;
      private readonly List<MethodSemantics> _methodSemantics;
      private readonly List<MethodImplementation> _methodImplementations;
      private readonly List<ModuleReference> _moduleReferences;
      private readonly List<TypeSpecification> _typeSpecifications;
      private readonly List<MethodImplementationMap> _methodImplementationMaps;
      private readonly List<FieldRVA> _fieldRVAs;
      private readonly List<AssemblyDefinition> _assemblyDefinitions;
      private readonly List<AssemblyReference> _assemblyReferences;
      private readonly List<FileReference> _fileReferences;
      private readonly List<ExportedType> _exportedTypess;
      private readonly List<ManifestResource> _manifestResources;
      private readonly List<NestedClassDefinition> _nestedClassDefinitions;
      private readonly List<GenericParameterDefinition> _genericParameterDefinitions;
      private readonly List<MethodSpecification> _methodSpecifications;
      private readonly List<GenericParameterConstraintDefinition> _genericParameterConstraintDefinitions;

      internal CILMetadataImpl()
         : this( EMPTY_SIZES )
      {

      }

      internal CILMetadataImpl( Int32[] sizes )
      {
         this._moduleDefinitions = new List<ModuleDefinition>( sizes[(Int32) Tables.Module] );
         this._typeReferences = new List<TypeReference>( sizes[(Int32) Tables.TypeRef] );
         this._typeDefinitions = new List<TypeDefinition>( sizes[(Int32) Tables.TypeDef] );
         this._fieldDefinitions = new List<FieldDefinition>( sizes[(Int32) Tables.Field] );
         this._methodDefinitions = new List<MethodDefinition>( sizes[(Int32) Tables.MethodDef] );
         this._parameterDefinitions = new List<ParameterDefinition>( sizes[(Int32) Tables.Parameter] );
         this._interfaceImplementations = new List<InterfaceImplementation>( sizes[(Int32) Tables.InterfaceImpl] );
         this._memberReferences = new List<MemberReference>( sizes[(Int32) Tables.MemberRef] );
         this._constantDefinitions = new List<ConstantDefinition>( sizes[(Int32) Tables.Constant] );
         this._customAttributeDefinitions = new List<CustomAttributeDefinition>( sizes[(Int32) Tables.CustomAttribute] );
         this._fieldMarshals = new List<FieldMarshal>( sizes[(Int32) Tables.FieldMarshal] );
         this._securityDefinitions = new List<SecurityDefinition>( sizes[(Int32) Tables.DeclSecurity] );
         this._classLayouts = new List<ClassLayout>( sizes[(Int32) Tables.ClassLayout] );
         this._fieldLayouts = new List<FieldLayout>( sizes[(Int32) Tables.FieldLayout] );
         this._standaloneSignatures = new List<StandaloneSignature>( sizes[(Int32) Tables.StandaloneSignature] );
         this._eventMaps = new List<EventMap>( sizes[(Int32) Tables.EventMap] );
         this._eventDefinitions = new List<EventDefinition>( sizes[(Int32) Tables.Event] );
         this._propertyMaps = new List<PropertyMap>( sizes[(Int32) Tables.PropertyMap] );
         this._propertyDefinitions = new List<PropertyDefinition>( sizes[(Int32) Tables.Property] );
         this._methodSemantics = new List<MethodSemantics>( sizes[(Int32) Tables.MethodSemantics] );
         this._methodImplementations = new List<MethodImplementation>( sizes[(Int32) Tables.MethodImpl] );
         this._moduleReferences = new List<ModuleReference>( sizes[(Int32) Tables.ModuleRef] );
         this._typeSpecifications = new List<TypeSpecification>( sizes[(Int32) Tables.TypeSpec] );
         this._methodImplementationMaps = new List<MethodImplementationMap>( sizes[(Int32) Tables.ImplMap] );
         this._fieldRVAs = new List<FieldRVA>( sizes[(Int32) Tables.FieldRVA] );
         this._assemblyDefinitions = new List<AssemblyDefinition>( sizes[(Int32) Tables.Assembly] );
         this._assemblyReferences = new List<AssemblyReference>( sizes[(Int32) Tables.AssemblyRef] );
         this._fileReferences = new List<FileReference>( sizes[(Int32) Tables.File] );
         this._exportedTypess = new List<ExportedType>( sizes[(Int32) Tables.ExportedType] );
         this._manifestResources = new List<ManifestResource>( sizes[(Int32) Tables.ManifestResource] );
         this._nestedClassDefinitions = new List<NestedClassDefinition>( sizes[(Int32) Tables.NestedClass] );
         this._genericParameterDefinitions = new List<GenericParameterDefinition>( sizes[(Int32) Tables.GenericParameter] );
         this._methodSpecifications = new List<MethodSpecification>( sizes[(Int32) Tables.MethodSpec] );
         this._genericParameterConstraintDefinitions = new List<GenericParameterConstraintDefinition>( sizes[(Int32) Tables.GenericParameterConstraint] );
      }

      public List<ModuleDefinition> ModuleDefinitions
      {
         get
         {
            return this._moduleDefinitions;
         }
      }

      public List<TypeReference> TypeReferences
      {
         get
         {
            return this._typeReferences;
         }
      }

      public List<TypeDefinition> TypeDefinitions
      {
         get
         {
            return this._typeDefinitions;
         }
      }

      public List<FieldDefinition> FieldDefinitions
      {
         get
         {
            return this._fieldDefinitions;
         }
      }

      public List<MethodDefinition> MethodDefinitions
      {
         get
         {
            return this._methodDefinitions;
         }
      }

      public List<ParameterDefinition> ParameterDefinitions
      {
         get
         {
            return this._parameterDefinitions;
         }
      }

      public List<InterfaceImplementation> InterfaceImplementations
      {
         get
         {
            return this._interfaceImplementations;
         }
      }

      public List<MemberReference> MemberReferences
      {
         get
         {
            return this._memberReferences;
         }
      }

      public List<ConstantDefinition> ConstantDefinitions
      {
         get
         {
            return this._constantDefinitions;
         }
      }

      public List<CustomAttributeDefinition> CustomAttributeDefinitions
      {
         get
         {
            return this._customAttributeDefinitions;
         }
      }

      public List<FieldMarshal> FieldMarshals
      {
         get
         {
            return this._fieldMarshals;
         }
      }

      public List<SecurityDefinition> SecurityDefinitions
      {
         get
         {
            return this._securityDefinitions;
         }
      }

      public List<ClassLayout> ClassLayouts
      {
         get
         {
            return this._classLayouts;
         }
      }

      public List<FieldLayout> FieldLayouts
      {
         get
         {
            return this._fieldLayouts;
         }
      }

      public List<StandaloneSignature> StandaloneSignatures
      {
         get
         {
            return this._standaloneSignatures;
         }
      }

      public List<EventMap> EventMaps
      {
         get
         {
            return this._eventMaps;
         }
      }

      public List<EventDefinition> EventDefinitions
      {
         get
         {
            return this._eventDefinitions;
         }
      }

      public List<PropertyMap> PropertyMaps
      {
         get
         {
            return this._propertyMaps;
         }
      }

      public List<PropertyDefinition> PropertyDefinitions
      {
         get
         {
            return this._propertyDefinitions;
         }
      }

      public List<MethodSemantics> MethodSemantics
      {
         get
         {
            return this._methodSemantics;
         }
      }

      public List<MethodImplementation> MethodImplementations
      {
         get
         {
            return this._methodImplementations;
         }
      }

      public List<ModuleReference> ModuleReferences
      {
         get
         {
            return this._moduleReferences;
         }
      }

      public List<TypeSpecification> TypeSpecifications
      {
         get
         {
            return this._typeSpecifications;
         }
      }

      public List<MethodImplementationMap> MethodImplementationMaps
      {
         get
         {
            return this._methodImplementationMaps;
         }
      }

      public List<FieldRVA> FieldRVAs
      {
         get
         {
            return this._fieldRVAs;
         }
      }

      public List<AssemblyDefinition> AssemblyDefinitions
      {
         get
         {
            return this._assemblyDefinitions;
         }
      }

      public List<AssemblyReference> AssemblyReferences
      {
         get
         {
            return this._assemblyReferences;
         }
      }

      public List<FileReference> FileReferences
      {
         get
         {
            return this._fileReferences;
         }
      }

      public List<ExportedType> ExportedTypess
      {
         get
         {
            return this._exportedTypess;
         }
      }

      public List<ManifestResource> ManifestResources
      {
         get
         {
            return this._manifestResources;
         }
      }

      public List<NestedClassDefinition> NestedClassDefinitions
      {
         get
         {
            return this._nestedClassDefinitions;
         }
      }

      public List<GenericParameterDefinition> GenericParameterDefinitions
      {
         get
         {
            return this._genericParameterDefinitions;
         }
      }

      public List<MethodSpecification> MethodSpecifications
      {
         get
         {
            return this._methodSpecifications;
         }
      }

      public List<GenericParameterConstraintDefinition> GenericParameterConstraintDefinitions
      {
         get
         {
            return this._genericParameterConstraintDefinitions;
         }
      }
   }
}
