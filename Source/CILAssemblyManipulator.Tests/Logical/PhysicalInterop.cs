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
using CILAssemblyManipulator.Logical;
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Tests.Logical;
using CommonUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CILAssemblyManipulator.Tests.Logical
{
   public class LogicalPhysicalInteropTest : AbstractCAMTest
   {

      [Test]
      public void TestPhysicalInteropWithWrapperAssemblies()
      {
         PerformRoundtripTest( CAMPhysicalLocation );
      }

      private static void PerformRoundtripTest( String mdLocation )
      {
         CILMetaData md;
         using ( var fs = File.OpenRead( mdLocation ) )
         {
            md = fs.ReadModule();
         }
         PerformTest( ctx =>
         {
            var mdLoader = new CILMetaDataLoaderNotThreadSafeForFiles();
            var loader = new CILAssemblyLoaderNotThreadSafe( ctx, mdLoader );
            var logical = loader.LoadAssemblyFrom( mdLocation );
            var physicalLoaded = mdLoader.GetOrLoadMetaData( mdLocation );
            var physicalCreated = logical.MainModule.CreatePhysicalRepresentation();
            Console.WriteLine( "Hmz" );
         } );
      }

      private static void PerformTest( Action<CILReflectionContext> test )
      {
         using ( var ctx = DotNETReflectionContext.CreateDotNETContext() )
         {
            test( ctx );
         }
      }
   }

   internal class ModuleStructureCreationInfo
   {
      private readonly CILMetaData _md;
      private readonly String[] _typeRefNames;
      private readonly TableIndex?[] _outmostTypeRefResolutionScopes;

      private readonly AbstractTypeDescription[] _typeDefDescriptions;
      private readonly AbstractTypeDescription[] _typeRefDescriptions;
      private readonly Lazy<AbstractTypeDescription>[] _typeSpecDescriptions;

      public ModuleStructureCreationInfo( CILMetaData md )
      {
         // TypeDef names
         var nestedTypes = new Dictionary<Int32, ISet<Int32>>();
         var enclosingTypes = new Dictionary<Int32, Int32>();
         foreach ( var nc in md.NestedClassDefinitions.TableContents )
         {
            nestedTypes
               .GetOrAdd_NotThreadSafe( nc.EnclosingClass.Index, i => new HashSet<Int32>() )
               .Add( nc.NestedClass.Index );
            enclosingTypes[nc.NestedClass.Index] = nc.EnclosingClass.Index;
         }

         var tDefs = md.TypeDefinitions.TableContents;
         var tlTypes = new HashSet<Int32>( Enumerable.Range( 0, tDefs.Count ) );
         tlTypes.ExceptWith( nestedTypes.Values.SelectMany( v => v ) );

         var typeDefNames = new String[tDefs.Count];
         foreach ( var type in tlTypes.SelectMany( t => t.AsDepthFirstEnumerable( tt =>
         {
            ISet<Int32> set;
            return nestedTypes.TryGetValue( tt, out set ) ?
               Empty<Int32>.Enumerable :
               set;
         } ) ) )
         {
            var tDef = tDefs[type];
            String typeString;
            Int32 enclosingIdx;
            if ( enclosingTypes.TryGetValue( type, out enclosingIdx ) )
            {
               var enclosingName = typeDefNames[enclosingIdx];
               if ( String.IsNullOrEmpty( enclosingName ) )
               {
                  throw new Exception( "This should not happen (is a bug?)." );
               }
               typeString = LogicalUtils.CombineEnclsosingAndNestedType( enclosingName, tDef.Name );
            }
            else
            {
               typeString = LogicalUtils.CombineTypeAndNamespace( tDef.Name, tDef.Namespace );
            }

            typeDefNames[type] = typeString;
         }

         // TypeRef names
         var tRefs = md.TypeReferences.TableContents;
         var typeRefNames = new String[tRefs.Count];
         var outmostTypeRefResolutionScopes = new TableIndex?[tRefs.Count];


         this._typeDefDescriptions = typeDefNames.Select( name => new TypeDescriptionTextual( name ) ).ToArray();
         this._typeRefDescriptions = this._typeRefNames.Select( ( name, idx ) =>
         {
            var resScopeNullable = this._outmostTypeRefResolutionScopes[idx];
            if ( resScopeNullable.HasValue )
            {
               var resScope = resScopeNullable.Value;
               switch ( resScope.Table )
               {
                  case Tables.AssemblyRef:
                     return new TypeDescriptionTextual( md.AssemblyReferences.TableContents[resScope.Index], name );
                  case Tables.ModuleRef:
                     return new TypeDescriptionTextual( md.ModuleReferences.TableContents[resScope.Index].ModuleName, name );
                  case Tables.Module:
                     return new TypeDescriptionTextual( name );
                  default:
                     throw new InvalidOperationException( "Unsupported resolution scope: " + resScope + "." );
               }
            }
            else
            {
               return new TypeDescriptionTextual( name, (ExportedType) null );
            }
         } ).ToArray();
         this._typeSpecDescriptions = md.TypeSpecifications.TableContents.Select( spec => new Lazy<AbstractTypeDescription>( () => new TypeDescriptionSignature( new SignatureInfo( this, spec.Signature ) ), LazyThreadSafetyMode.None ) ).ToArray();
      }

      public CILMetaData MetaData
      {
         get
         {
            return this._md;
         }
      }

      public AbstractTypeDescription GetTypeDefDescription( Int32 idx )
      {
         return this._typeDefDescriptions[idx];
      }

      public AbstractTypeDescription GetTypeRefDescription( Int32 idx )
      {
         return this._typeRefDescriptions[idx];
      }

      public AbstractTypeDescription GetTypeSpecDescription( Int32 idx )
      {
         return this._typeSpecDescriptions[idx].Value;
      }

      private AbstractTypeDescription FromTypeDefOrRefOrSpec( TableIndex index )
      {
         var md = this._md;
         switch ( index.Table )
         {
            case Tables.TypeDef:
               return new TypeDescriptionTextual( this._typeDefNames[index.Index] );
            case Tables.TypeRef:
               var resScopeNullable = this._outmostTypeRefResolutionScopes[index.Index];
               var typeName = this._typeRefNames[index.Index];

               if ( resScopeNullable.HasValue )
               {
                  var resScope = resScopeNullable.Value;
                  switch ( resScope.Table )
                  {
                     case Tables.AssemblyRef:
                        return new TypeDescriptionTextual( md.AssemblyReferences.TableContents[resScope.Index], typeName );
                     case Tables.ModuleRef:
                        return new TypeDescriptionTextual( md.ModuleReferences.TableContents[resScope.Index].ModuleName, typeName );
                     case Tables.Module:
                        return new TypeDescriptionTextual( typeName );
                     default:
                        throw new InvalidOperationException( "Unsupported resolution scope: " + resScope + "." );
                  }
               }
               else
               {
                  return new TypeDescriptionTextual( typeName, (ExportedType) null );
               }
            case Tables.TypeSpec:
               return new TypeDescriptionSignature( new SignatureInfo( this, md.TypeSpecifications.TableContents[index.Index].Signature ) );
            default:
               throw new InvalidOperationException( "Unsupported TypeDef/Ref/Spec: " + index + "." );
         }
      }
   }

   public class ModuleStructureInfo
   {
      private readonly ISet<TypeStructureInfo> _topLevelTypes;

   }

   public class TypeStructureInfo : IEquatable<TypeStructureInfo>
   {
      private readonly String _namespace;
      private readonly String _name;
      private readonly ISet<TypeStructureInfo> _nestedTypes;
      private readonly IList<FieldStructureInfo> _fields;
      private readonly IList<MethodStructureInfo> _methods;

      public TypeStructureInfo( CILMetaData md, Int32 idx )
      {
         var tDef = md.TypeDefinitions.TableContents[idx];
         this._namespace = tDef.Namespace;
         this._name = tDef.Name;
         this._fields = md.GetTypeMethodIndices( idx )
            .Select( fIdx => new FieldStructureInfo( md, fIdx ) )
            .ToList();
      }

      public override Boolean Equals( Object obj )
      {
         return this.Equals( obj as TypeStructureInfo );
      }

      public override Int32 GetHashCode()
      {
         return ( 17 * 23 + this._name.GetHashCodeSafe() ) * 23 + this._namespace.GetHashCodeSafe();
      }

      public Boolean Equals( TypeStructureInfo other )
      {
         return ReferenceEquals( this, other )
            || ( other != null
            && String.Equals( this._name, other._name )
            && String.Equals( this._namespace, other._namespace )
            && SetEqualityComparer<TypeStructureInfo>.DefaultEqualityComparer.Equals( this._nestedTypes, other._nestedTypes )
            && ListEqualityComparer<IList<FieldStructureInfo>, FieldStructureInfo>.DefaultListEqualityComparer.Equals( this._fields, other._fields )
            && ListEqualityComparer<IList<MethodStructureInfo>, MethodStructureInfo>.DefaultListEqualityComparer.Equals( this._methods, other._methods )
            );
      }
   }

   public class FieldStructureInfo : IEquatable<FieldStructureInfo>
   {
      private readonly Int32 _index;
      private readonly FieldAttributes _attrs;
      private readonly String _name;
      private readonly ConstantDefinition _const;
      private readonly FieldMarshal _marshal;
      private readonly FieldRVA _initialValue;
      private readonly FieldLayout _offset;
      private SignatureInfo _fieldType;

      public FieldStructureInfo( ModuleStructureCreationInfo info, Int32 idx )
      {
         var md = info.MetaData;
         var fDef = md.FieldDefinitions.TableContents[idx];
         this._index = idx;
         this._attrs = fDef.Attributes;
         this._name = fDef.Name;

         var thisIndex = new TableIndex( Tables.Field, idx );
         this._const = md.ConstantDefinitions.TableContents.FirstOrDefault( c => c.Parent == thisIndex );
         this._marshal = md.FieldMarshals.TableContents.FirstOrDefault( c => c.Parent == thisIndex );
         this._initialValue = md.FieldRVAs.TableContents.FirstOrDefault( c => c.Field == thisIndex );
         this._offset = md.FieldLayouts.TableContents.FirstOrDefault( c => c.Field == thisIndex );
      }

      public void PopulateTypes( ModuleStructureCreationInfo info )
      {
         this._fieldType = new SignatureInfo( info, info.MetaData.FieldDefinitions.TableContents[this._index].Signature );
      }

      public override Boolean Equals( Object obj )
      {
         return this.Equals( obj as FieldStructureInfo );
      }

      public override Int32 GetHashCode()
      {
         return 17 * 23 + this._index;
      }

      public Boolean Equals( FieldStructureInfo other )
      {
         return ReferenceEquals(this, other)
            || (other != null
            && this._attrs == other._attrs
            && String.Equals(this._name, other._name)
            && this._fieldType.Equals(other._fieldType)
            && ConstsEquality(this._const, other._const)
            )
      }

      private static Boolean ConstsEquality( ConstantDefinition x, ConstantDefinition y )
      {
         return ReferenceEquals( x, y ) || ( x != null && y != null && x.Type == y.Type && Equals( x.Value, y.Value ) );
      }

      private static Boolean FieldMarshalEquality( FieldMarshal x, FieldMarshal y )
      {
         return ReferenceEquals( x, y ) || ( x != null && y != null && Comparers.MarshalingInfoEqualityComparer.Equals( x.NativeType, y.NativeType ) );
      }

      private static Boolean FieldRVAEquality( FieldRVA x, FieldRVA y )
      {
         return ReferenceEquals( x, y ) || ( x != null && y != null && ArrayEqualityComparer<Byte>.DefaultArrayEqualityComparer.Equals( x.Data, y.Data ) );
      }

      private static Boolean FieldLayoutEquality( FieldLayout x, FieldLayout y )
      {
         return ReferenceEquals( x, y ) || ( x != null && y != null && x.Offset == y.Offset );
      }

   }

   public class MethodStructureInfo
   {

   }

   public class SignatureInfo : IEquatable<SignatureInfo>
   {
      public SignatureInfo( ModuleStructureCreationInfo info, AbstractSignature signature )
      {

      }
   }

   public enum TypeDescriptionKind
   {
      Textual,
      Signature
   }

   public abstract class AbstractTypeDescription
   {
      public abstract TypeDescriptionKind TypeDescriptionKind { get; }


   }

   // For type refs & type defs
   public class TypeDescriptionTextual : AbstractTypeDescription
   {
      private readonly String _moduleName;
      private readonly AssemblyReference _assemblyRef;
      private readonly String _fullTypeName;
      private readonly Boolean _isExported;

      public TypeDescriptionTextual( String fullTypeName )
         : this( fullTypeName, null, null, false )
      {

      }

      public TypeDescriptionTextual( AssemblyReference assemblyRef, String fullTypeName )
         : this( fullTypeName, assemblyRef, null, false )
      {

      }

      public TypeDescriptionTextual( String moduleName, String fullTypeName )
         : this( fullTypeName, null, moduleName, false )
      {

      }

      public TypeDescriptionTextual( String fullTypeName, ExportedType exported )
         : this( fullTypeName, null, null, true )
      {

      }

      private TypeDescriptionTextual( String fullTypeName, AssemblyReference assemblyRef, String moduleName, Boolean isExported )
      {
         ArgumentValidator.ValidateNotEmpty( "Full type name", fullTypeName );

         this._moduleName = moduleName;
         this._assemblyRef = assemblyRef;
         this._fullTypeName = fullTypeName;
      }

      public override TypeDescriptionKind TypeDescriptionKind
      {
         get
         {
            return TypeDescriptionKind.Textual;
         }
      }


   }

   // For type specs
   public class TypeDescriptionSignature : AbstractTypeDescription
   {
      private readonly SignatureInfo _signature;

      public TypeDescriptionSignature( SignatureInfo signature )
      {
         this._signature = signature;
      }

      public override TypeDescriptionKind TypeDescriptionKind
      {
         get
         {
            return TypeDescriptionKind.Signature;
         }
      }
   }
}

public static partial class E_CILTests
{
   public static AbstractTypeDescription FromTypeDefOrRefOrSpec( this ModuleStructureCreationInfo moduleInfo, TableIndex index )
   {
      switch ( index.Table )
      {
         case Tables.TypeDef:
            return moduleInfo.GetTypeDefDescription( index.Index );
         case Tables.TypeRef:
            return moduleInfo.GetTypeRefDescription( index.Index );
         case Tables.TypeSpec:
            return moduleInfo.GetTypeSpecDescription( index.Index );
         default:
            throw new InvalidOperationException( "Unsupported TypeDef/Ref/Spec: " + index + "." );
      }
   }
}