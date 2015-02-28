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

namespace CILAssemblyManipulator.Physical
{
   public interface CILModuleData
   {
      HeadersData Headers { get; }
      CILMetaData MetaData { get; }
   }

   public interface HeadersData
   {

   }

   public interface CILMetaData
   {
      List<ModuleDefinition> ModuleDefinitions { get; }

      List<TypeReference> TypeReferences { get; }

      List<TypeDefinition> TypeDefinitions { get; }

      List<FieldDefinition> FieldDefinitions { get; }

      List<MethodDefinition> MethodDefinitions { get; }

      List<ParameterDefinition> ParameterDefinitions { get; }

      List<InterfaceImplementation> InterfaceImplementations { get; }

      List<MemberReference> MemberReferences { get; }

      List<ConstantDefinition> ConstantDefinitions { get; }

      List<CustomAttributeDefinition> CustomAttributeDefinitions { get; }

      List<FieldMarshal> FieldMarshals { get; }

      List<SecurityDefinition> SecurityDefinitions { get; }

      List<ClassLayout> ClassLayouts { get; }

      List<FieldLayout> FieldLayouts { get; }

      List<StandaloneSignature> StandaloneSignatures { get; }

      List<EventMap> EventMaps { get; }

      List<EventDefinition> EventDefinitions { get; }

      List<PropertyMap> PropertyMaps { get; }

      List<PropertyDefinition> PropertyDefinitions { get; }

      List<MethodSemantics> MethodSemantics { get; }

      List<MethodImplementation> MethodImplementations { get; }

      List<ModuleReference> ModuleReferences { get; }

      List<TypeSpecification> TypeSpecifications { get; }

      List<MethodImplementationMap> MethodImplementationMaps { get; }

      List<FieldRVA> FieldRVAs { get; }

      List<AssemblyDefinition> AssemblyDefinitions { get; }

      List<AssemblyReference> AssemblyReferences { get; }

      List<FileReference> FileReferences { get; }

      List<ExportedType> ExportedTypess { get; }

      List<ManifestResource> ManifestResources { get; }

      List<NestedClassDefinition> NestedClassDefinitions { get; }

      List<GenericParameterDefinition> GenericParameterDefinitions { get; }

      List<MethodSpecification> MethodSpecifications { get; }

      List<GenericParameterConstraintDefinition> GenericParameterConstraintDefinitions { get; }
   }

   public sealed class CustomAttributeTypeResolveEventArgs : EventArgs
   {
      private readonly String _assemblyName;
      private readonly AssemblyInformationForResolving? _assemblyInfo;

      internal CustomAttributeTypeResolveEventArgs( String assemblyName, AssemblyInformationForResolving? assemblyInfo )
      {
         this._assemblyName = assemblyName;
         this._assemblyInfo = assemblyInfo;
      }

      /// <summary>
      /// This may be <c>null</c>! This means that it is mscorlib assembly, (or possibly another module?)
      /// </summary>
      public String UnparsedAssemblyName
      {
         get
         {
            return this._assemblyName;
         }
      }

      public AssemblyInformationForResolving? ExistingAssemblyInformation
      {
         get
         {
            return this._assemblyInfo;
         }
      }

      public CILMetaData ResolvedAssembly { get; set; }
   }

   public struct AssemblyInformationForResolving : IEquatable<AssemblyInformationForResolving>
   {
      private readonly AssemblyInformation _information;
      private readonly Boolean _isFullPublicKey;

      public AssemblyInformationForResolving( AssemblyInformation information, Boolean isFullPublicKey )
      {
         ArgumentValidator.ValidateNotNull( "Assembly information", information );

         this._information = information;
         this._isFullPublicKey = isFullPublicKey;
      }

      public AssemblyInformation AssemblyInformation
      {
         get
         {
            return this._information;
         }
      }

      public Boolean IsFullPublicKey
      {
         get
         {
            return this._isFullPublicKey;
         }
      }

      public override Boolean Equals( Object obj )
      {
         return obj is AssemblyInformationForResolving ?
            this.Equals( (AssemblyInformationForResolving) obj ) :
            false;
      }

      public override Int32 GetHashCode()
      {
         return this._information.Name.GetHashCodeSafe();
      }

      public Boolean Equals( AssemblyInformationForResolving other )
      {
         return this._isFullPublicKey == other._isFullPublicKey
               && Equals( this._information, other._information );
      }

      private static Boolean Equals( AssemblyInformation x, AssemblyInformation y )
      {
         return Object.ReferenceEquals( x, y )
            || ( x != null
               && y != null
               && String.Equals( x.Name, y.Name )
               && x.VersionMajor == y.VersionMajor
               && x.VersionMinor == y.VersionMinor
               && x.VersionBuild == y.VersionBuild
               && x.VersionRevision == y.VersionRevision
               && String.Equals( x.Culture, y.Culture )
               && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.PublicKeyOrToken, y.PublicKeyOrToken )
            );

      }
   }

   public sealed class ModuleReadResult
   {
      private readonly CILMetaData _md;
      private readonly HeadersData _headers;

      internal ModuleReadResult( CILMetaData md, HeadersData headers )
      {
         ArgumentValidator.ValidateNotNull( "Metadata", md );
         //ArgumentValidator.ValidateNotNull( "Headers", headers );

         this._md = md;
         this._headers = headers;
      }

      public CILMetaData MetaData
      {
         get
         {
            return this._md;
         }
      }

      public HeadersData Headers
      {
         get
         {
            return this._headers;
         }
      }
   }

   public static class CILModuleIO
   {

      public static ModuleReadResult ReadModule( Stream stream )
      {
         HeadersData headers;
         var md = CILAssemblyManipulator.Physical.Implementation.ModuleReader.ReadFromStream( stream, out headers );
         return new ModuleReadResult( md, headers );
      }
   }
}
