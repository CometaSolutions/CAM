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

      private readonly IList<ModuleDefinition> _moduleDefinitions;
      private readonly IList<TypeReference> _typeReferences;
      private readonly IList<TypeDefinition> _typeDefinitions;
      private readonly IList<FieldDefinition> _fieldDefinitions;
      private readonly IList<MethodDefinition> _methodDefinitions;
      private readonly IList<ParameterDefinition> _parameterDefinitions;
      private readonly IList<InterfaceImplementation> _interfaceImplementations;
      private readonly IList<MemberReference> _memberReferences;
      private readonly IList<ConstantDefinition> _constantDefinitions;
      private readonly IList<CustomAttributeDefinition> _customAttributeDefinitions;
      private readonly IList<FieldMarshal> _fieldMarshals;
      private readonly IList<SecurityDefinition> _securityDefinitions;
      private readonly IList<ClassLayout> _classLayouts;
      private readonly IList<FieldLayout> _fieldLayouts;
      private readonly IList<StandaloneSignature> _standaloneSignatures;
      private readonly IList<EventMap> _eventMaps;
      private readonly IList<EventDefinition> _eventDefinitions;
      private readonly IList<PropertyMap> _propertyMaps;
      private readonly IList<PropertyDefinition> _propertyDefinitions;
      private readonly IList<MethodSemantics> _methodSemantics;
      private readonly IList<MethodImplementation> _methodImplementations;
      private readonly IList<ModuleReference> _moduleReferences;
      private readonly IList<TypeSpecification> _typeSpecifications;
      private readonly IList<MethodImplementationMap> _methodImplementationMaps;
      private readonly IList<FieldRVA> _fieldRVAs;
      private readonly IList<AssemblyDefinition> _assemblyDefinitions;
      private readonly IList<AssemblyReference> _assemblyReferences;
      private readonly IList<FileReference> _fileReferences;
      private readonly IList<ExportedTypes> _exportedTypess;
      private readonly IList<ManifestResource> _manifestResources;
      private readonly IList<NestedClassDefinition> _nestedClassDefinitions;
      private readonly IList<GenericParameterDefinition> _genericParameterDefinitions;
      private readonly IList<MethodSpecification> _methodSpecifications;
      private readonly IList<GenericParameterConstraintDefinition> _genericParameterConstraintDefinitions;

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
         this._exportedTypess = new List<ExportedTypes>( sizes[(Int32) Tables.ExportedType] );
         this._manifestResources = new List<ManifestResource>( sizes[(Int32) Tables.ManifestResource] );
         this._nestedClassDefinitions = new List<NestedClassDefinition>( sizes[(Int32) Tables.NestedClass] );
         this._genericParameterDefinitions = new List<GenericParameterDefinition>( sizes[(Int32) Tables.GenericParameter] );
         this._methodSpecifications = new List<MethodSpecification>( sizes[(Int32) Tables.MethodSpec] );
         this._genericParameterConstraintDefinitions = new List<GenericParameterConstraintDefinition>( sizes[(Int32) Tables.GenericParameterConstraint] );
      }

      public IList<ModuleDefinition> ModuleDefinitions
      {
         get
         {
            return this._moduleDefinitions;
         }
      }

      public IList<TypeReference> TypeReferences
      {
         get
         {
            return this._typeReferences;
         }
      }

      public IList<TypeDefinition> TypeDefinitions
      {
         get
         {
            return this._typeDefinitions;
         }
      }

      public IList<FieldDefinition> FieldDefinitions
      {
         get
         {
            return this._fieldDefinitions;
         }
      }

      public IList<MethodDefinition> MethodDefinitions
      {
         get
         {
            return this._methodDefinitions;
         }
      }

      public IList<ParameterDefinition> ParameterDefinitions
      {
         get
         {
            return this._parameterDefinitions;
         }
      }

      public IList<InterfaceImplementation> InterfaceImplementations
      {
         get
         {
            return this._interfaceImplementations;
         }
      }

      public IList<MemberReference> MemberReferences
      {
         get
         {
            return this._memberReferences;
         }
      }

      public IList<ConstantDefinition> ConstantDefinitions
      {
         get
         {
            return this._constantDefinitions;
         }
      }

      public IList<CustomAttributeDefinition> CustomAttributeDefinitions
      {
         get
         {
            return this._customAttributeDefinitions;
         }
      }

      public IList<FieldMarshal> FieldMarshals
      {
         get
         {
            return this._fieldMarshals;
         }
      }

      public IList<SecurityDefinition> SecurityDefinitions
      {
         get
         {
            return this._securityDefinitions;
         }
      }

      public IList<ClassLayout> ClassLayouts
      {
         get
         {
            return this._classLayouts;
         }
      }

      public IList<FieldLayout> FieldLayouts
      {
         get
         {
            return this._fieldLayouts;
         }
      }

      public IList<StandaloneSignature> StandaloneSignatures
      {
         get
         {
            return this._standaloneSignatures;
         }
      }

      public IList<EventMap> EventMaps
      {
         get
         {
            return this._eventMaps;
         }
      }

      public IList<EventDefinition> EventDefinitions
      {
         get
         {
            return this._eventDefinitions;
         }
      }

      public IList<PropertyMap> PropertyMaps
      {
         get
         {
            return this._propertyMaps;
         }
      }

      public IList<PropertyDefinition> PropertyDefinitions
      {
         get
         {
            return this._propertyDefinitions;
         }
      }

      public IList<MethodSemantics> MethodSemantics
      {
         get
         {
            return this._methodSemantics;
         }
      }

      public IList<MethodImplementation> MethodImplementations
      {
         get
         {
            return this._methodImplementations;
         }
      }

      public IList<ModuleReference> ModuleReferences
      {
         get
         {
            return this._moduleReferences;
         }
      }

      public IList<TypeSpecification> TypeSpecifications
      {
         get
         {
            return this._typeSpecifications;
         }
      }

      public IList<MethodImplementationMap> MethodImplementationMaps
      {
         get
         {
            return this._methodImplementationMaps;
         }
      }

      public IList<FieldRVA> FieldRVAs
      {
         get
         {
            return this._fieldRVAs;
         }
      }

      public IList<AssemblyDefinition> AssemblyDefinitions
      {
         get
         {
            return this._assemblyDefinitions;
         }
      }

      public IList<AssemblyReference> AssemblyReferences
      {
         get
         {
            return this._assemblyReferences;
         }
      }

      public IList<FileReference> FileReferences
      {
         get
         {
            return this._fileReferences;
         }
      }

      public IList<ExportedTypes> ExportedTypess
      {
         get
         {
            return this._exportedTypess;
         }
      }

      public IList<ManifestResource> ManifestResources
      {
         get
         {
            return this._manifestResources;
         }
      }

      public IList<NestedClassDefinition> NestedClassDefinitions
      {
         get
         {
            return this._nestedClassDefinitions;
         }
      }

      public IList<GenericParameterDefinition> GenericParameterDefinitions
      {
         get
         {
            return this._genericParameterDefinitions;
         }
      }

      public IList<MethodSpecification> MethodSpecifications
      {
         get
         {
            return this._methodSpecifications;
         }
      }

      public IList<GenericParameterConstraintDefinition> GenericParameterConstraintDefinitions
      {
         get
         {
            return this._genericParameterConstraintDefinitions;
         }
      }
   }
}
