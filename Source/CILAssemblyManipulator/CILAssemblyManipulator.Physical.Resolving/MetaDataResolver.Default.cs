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
extern alias CAMPhysical;
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;
using CAMPhysical::CILAssemblyManipulator.Physical.Meta;

using UtilPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.Resolving
{
   /// <summary>
   /// This class provides default implementation for <see cref="MetaDataResolver"/>.
   /// </summary>
   public sealed class DefaultMetaDataResolver : MetaDataResolver
   {
      private sealed class MDSpecificCache
      {
         private static readonly Object NULL = new Object();

         private readonly DefaultMetaDataResolver _owner;
         private readonly CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData _md;

         private readonly IDictionary<Object, MDSpecificCache> _assemblyResolveFreeFormCache;
         private readonly IDictionary<Int32, MDSpecificCache> _assembliesByInfoCache;
         private readonly IDictionary<Int32, MDSpecificCache> _modulesCache;

         private readonly IDictionary<KeyValuePair<String, String>, Int32> _topLevelTypeCache; // Key: ns + type pair, Value: TypeDef index
         private readonly IDictionary<Int32, Tuple<MDSpecificCache, Int32>> _typeRefCache; // Key: TypeRef index, Value: TypeDef index in another metadata
         private readonly IDictionary<String, Int32> _typeNameCache; // Key - type name (ns + enclosing classes + type name), Value - TypeDef index
         private readonly IDictionary<Int32, String> _typeNameReverseCache; // Key - typeDefIndex, Value - type name
         private readonly IDictionary<Int32, String> _typeRefReverseCache;

         internal MDSpecificCache( DefaultMetaDataResolver owner, CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData md )
         {
            ArgumentValidator.ValidateNotNull( "Owner", owner );
            ArgumentValidator.ValidateNotNull( "Metadata", md );

            this._owner = owner;
            this._md = md;

            this._assemblyResolveFreeFormCache = new Dictionary<Object, MDSpecificCache>();
            this._assembliesByInfoCache = new Dictionary<Int32, MDSpecificCache>();
            this._modulesCache = new Dictionary<Int32, MDSpecificCache>();

            this._topLevelTypeCache = new Dictionary<KeyValuePair<String, String>, Int32>();
            this._typeRefCache = new Dictionary<Int32, Tuple<MDSpecificCache, Int32>>();
            this._typeNameCache = new Dictionary<String, Int32>();
            this._typeNameReverseCache = new Dictionary<Int32, String>();
            this._typeRefReverseCache = new Dictionary<Int32, String>();
         }

         internal CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData MD
         {
            get
            {
               return this._md;
            }
         }

         internal MDSpecificCache ResolveCacheByAssemblyString( String assemblyString )
         {
            var parseSuccessful = false;
            Object key;
            if ( String.IsNullOrEmpty( assemblyString ) )
            {
               key = NULL;
            }
            else
            {
               AssemblyInformation aInfo;
               Boolean isFullPublicKey;
               parseSuccessful = AssemblyInformation.TryParse( assemblyString, out aInfo, out isFullPublicKey );
               key = parseSuccessful ?
                  (Object) new AssemblyInformationForResolving( aInfo, isFullPublicKey ) :
                  assemblyString;
            }

            return this._assemblyResolveFreeFormCache.GetOrAdd_NotThreadSafe(
               key,
               kkey => this._owner.ResolveAssemblyReferenceWithEvent(
                  this._md,
                  parseSuccessful ? null : assemblyString,
                  parseSuccessful ? (AssemblyInformationForResolving) kkey : null
                  )
               );
         }


         internal Int32 ResolveTypeFromTypeName( String typeName )
         {
            return this._typeNameCache.GetOrAdd_NotThreadSafe( typeName, tn =>
            {
               Int32 tDefIndex;
               String enclosingType, nestedType;
               var isNestedType = typeName.ParseTypeNameStringForNestedType( out enclosingType, out nestedType );
               if ( isNestedType )
               {
                  tDefIndex = this.ResolveTypeFromTypeName( enclosingType );
                  tDefIndex = this.FindNestedTypeIndex( tDefIndex, nestedType );
               }
               else
               {
                  String ns;
                  var hasNS = typeName.ParseTypeNameStringForNamespace( out ns, out typeName );
                  tDefIndex = this.ResolveTopLevelType( typeName, ns );
               }

               return tDefIndex;
            } );
         }

         internal String ResolveTypeNameFromTypeDef( Int32 index )
         {
            return index < 0 ?
               null :
               this._typeNameReverseCache
                  .GetOrAdd_NotThreadSafe( index, idx =>
                  {
                     var md = this._md;
                     var tDef = md.TypeDefinitions.GetOrNull( idx );
                     String retVal;
                     if ( tDef != null )
                     {
                        var nestedDef = md.NestedClassDefinitions.TableContents.FirstOrDefault( nc => nc != null && nc.NestedClass.Index == idx );
                        if ( nestedDef == null )
                        {
                           // This is top-level class
                           retVal = Miscellaneous.CombineNamespaceAndType( tDef.Namespace, tDef.Name );
                        }
                        else
                        {
                           // Nested type - recursion
                           // TODO get rid of recursion

                           retVal = Miscellaneous.CombineEnclosingAndNestedType( this.ResolveTypeNameFromTypeDef( nestedDef.EnclosingClass.Index ), tDef.Name );
                        }
                     }
                     else
                     {
                        retVal = null;
                     }

                     return retVal;
                  } );
         }

         internal String ResolveTypeNameFromTypeRef( Int32 index )
         {
            return this._typeRefReverseCache.GetOrAdd_NotThreadSafe( index, idx =>
            {
               MDSpecificCache otherMD; Int32 tDefIndex;
               this.ResolveTypeNameFromTypeRef( index, out otherMD, out tDefIndex );
               var typeRefString = otherMD == null ? null : otherMD.ResolveTypeNameFromTypeDef( tDefIndex );
               if ( typeRefString != null && !ReferenceEquals( this, otherMD ) && otherMD._md.AssemblyDefinitions.GetRowCount() > 0 )
               {
                  typeRefString = Miscellaneous.CombineAssemblyAndType( otherMD._md.AssemblyDefinitions.TableContents[0].ToString(), typeRefString );
               }

               return typeRefString;
            } );
         }

         internal void ResolveTypeNameFromTypeRef( Int32 index, out MDSpecificCache otherMDParam, out Int32 tDefIndexParam )
         {
            var tuple = this._typeRefCache.GetOrAdd_NotThreadSafe( index, idx =>
            {
               var md = this._md;
               var tRef = md.TypeReferences.GetOrNull( idx );

               var tDefIndex = -1;
               MDSpecificCache otherMD;
               if ( tRef == null )
               {
                  otherMD = null;
               }
               else
               {
                  otherMD = this;
                  if ( tRef.ResolutionScope.HasValue )
                  {
                     var resScope = tRef.ResolutionScope.Value;
                     var resIdx = resScope.Index;
                     switch ( resScope.Table )
                     {
                        case Tables.TypeRef:
                           // Nested type
                           this.ResolveTypeNameFromTypeRef( resIdx, out otherMD, out tDefIndex );
                           if ( otherMD != null )
                           {
                              tDefIndex = otherMD.FindNestedTypeIndex( tDefIndex, tRef.Name );
                           }
                           break;
                        case Tables.ModuleRef:
                           // Same assembly, different module
                           otherMD = this.ResolveModuleReference( resIdx );
                           if ( otherMD != null )
                           {
                              tDefIndex = otherMD.ResolveTopLevelType( tRef.Name, tRef.Namespace );
                           }
                           break;
                        case Tables.Module:
                           // Same as type-def
                           tDefIndex = resIdx;
                           break;
                        case Tables.AssemblyRef:
                           // Different assembly
                           otherMD = this.ResolveAssemblyReference( resIdx );
                           if ( otherMD != null )
                           {
                              tDefIndex = otherMD.ResolveTopLevelType( tRef.Name, tRef.Namespace );
                           }
                           break;
                     }
                  }
                  else
                  {
                     // Seek exported type table for this type, and check its implementation field
                     throw new NotImplementedException( "Exported type in type reference row." );
                  }
               }

               return Tuple.Create( otherMD, tDefIndex );
            } );

            otherMDParam = tuple.Item1;
            tDefIndexParam = tuple.Item2;
         }

         private Int32 ResolveTopLevelType( String typeName, String typeNamespace )
         {
            return this._topLevelTypeCache
               .GetOrAdd_NotThreadSafe( new KeyValuePair<String, String>( typeNamespace, typeName ), kvp =>
               {
                  var md = this._md;
                  var ns = kvp.Key;
                  var tn = kvp.Value;

                  var hasNS = !String.IsNullOrEmpty( ns );
                  var suitableIndex = md.TypeDefinitions.TableContents.FindIndex( tDef =>
                     String.Equals( tDef.Name, tn )
                     && (
                        ( hasNS && String.Equals( tDef.Namespace, ns ) )
                        || ( !hasNS && String.IsNullOrEmpty( tDef.Namespace ) )
                     ) );

                  // Check that this is not nested type
                  if ( suitableIndex >= 0
                     && md.NestedClassDefinitions.TableContents.Any( nc => nc.NestedClass.Index == suitableIndex ) // TODO cache this? //.GetReferencingRowsFromOrdered( Tables.TypeDef, suitableIndex, nc => nc.NestedClass ).Any() // this will be true if the type definition at index 'suitableIndex' has declaring type, i.e. it is nested type
                     )
                  {
                     suitableIndex = -1;
                  }

                  return suitableIndex;
               } );
         }

         private Int32 FindNestedTypeIndex( Int32 enclosingTypeIndex, String nestedTypeName )
         {
            Int32 retVal;
            var md = this._md;
            var otherTDList = md.TypeDefinitions;
            var otherTD = otherTDList.GetOrNull( enclosingTypeIndex );
            NestedClassDefinition nestedTD = null;
            if ( otherTD != null )
            {
               // Find nested type, which has this type as its declaring type and its name equal to tRef's
               // Skip to the first row where nested class index is greater than type def index (since in typedef table, all nested class definitions must follow encloding class definition)
               nestedTD = md.NestedClassDefinitions.TableContents
                  //.SkipWhile( nc => nc.NestedClass.Index <= enclosingTypeIndex )
                  .Where( nc =>
                  {
                     var match = nc.EnclosingClass.Index == enclosingTypeIndex;
                     if ( match )
                     {
                        var ncTD = otherTDList.GetOrNull( nc.NestedClass.Index );
                        match = ncTD != null
                           && String.Equals( ncTD.Name, nestedTypeName );
                     }
                     return match;
                  } )
                  .FirstOrDefault();
            }

            retVal = nestedTD == null ?
               -1 :
               nestedTD.NestedClass.Index;


            return retVal;
         }

         private MDSpecificCache ResolveModuleReference( Int32 modRefIdx )
         {
            return this._modulesCache.GetOrAdd_NotThreadSafe(
               modRefIdx,
               idx =>
               {
                  var mRef = this._md.ModuleReferences.GetOrNull( idx );
                  return mRef == null ? null : this._owner.ResolveModuleReferenceWithEvent( this._md, mRef.ModuleName );
               } );
         }

         private MDSpecificCache ResolveAssemblyReference( Int32 aRefIdx )
         {
            return this._assembliesByInfoCache.GetOrAdd_NotThreadSafe(
               aRefIdx,
               idx =>
               {
                  var aRef = this._md.AssemblyReferences.GetOrNull( idx );
                  return aRef == null ? null : this._owner.ResolveAssemblyReferenceWithEvent( this._md, null, new AssemblyInformationForResolving( aRef ) );
               } );
         }
      }

      private readonly IDictionary<CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData, MDSpecificCache> _mdCaches;
      private readonly Func<CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData, MDSpecificCache> _mdCacheFactory;

      /// <summary>
      /// Creates a new instance of <see cref="DefaultMetaDataResolver"/> with an empty cache.
      /// </summary>
      public DefaultMetaDataResolver()
      {
         this._mdCaches = new Dictionary<CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData, MDSpecificCache>();
         this._mdCacheFactory = this.MDSpecificCacheFactory;
      }

      /// <inheritdoc />
      public String ResolveTypeNameFromTypeDefOrRefOrSpec( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData md, TableIndex index )
      {
         var idx = index.Index;
         String retVal;
         switch ( index.Table )
         {
            case Tables.TypeDef:
               retVal = this.ResolveTypeNameFromTypeDef( md, index.Index );
               break;
            case Tables.TypeRef:
               retVal = this.ResolveTypeNameFromTypeRef( md, idx );
               break;
            case Tables.TypeSpec:
               throw new NotImplementedException( "Resolving type name from type spec." );
            //   // Recursion within same metadata:
            //   var tSpec = md.TypeSpecifications.GetOrNull( idx );
            //   retVal = tSpec == null ?
            //      null :
            //      ConvertTypeSignatureToCustomAttributeType( md, tSpec.Signature );
            //   break;
            default:
               retVal = null;
               break;
         }

         return retVal;
      }

      /// <inheritdoc />
      public Boolean TryResolveTypeString( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData md, String fullTypeString, out CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData otherMD, out Int32 typeDefIndex )
      {
         // 1. See if there is assembly name present
         // 2. If present, then resolve assembly by name
         // 3. If not present, then try this assembly and then 'mscorlib'
         // 4. Resolve table index by string
         // 5. Resolve return value by CILMetaData + TableIndex pair

         String typeName, assemblyName;
         var assemblyNamePresent = fullTypeString.ParseAssemblyQualifiedTypeString( out typeName, out assemblyName );
         var targetModule = assemblyNamePresent ? this.ResolveAssemblyByString( md, assemblyName ) : this.GetCacheFor( md );
         otherMD = targetModule?.MD;
         typeDefIndex = targetModule?.ResolveTypeFromTypeName( typeName ) ?? -1;

         if ( otherMD == null && !assemblyNamePresent )
         {
            // TODO try 'mscorlib' unless this is mscorlib
         }

         return otherMD != null && typeDefIndex >= 0;
      }

      /// <inheritdoc />
      public Boolean TryResolveTypeDefOrRefOrSpec( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData md, TableIndex index, out CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData otherMD, out Int32 typeDefIndex )
      {
         var idx = index.Index;
         switch ( index.Table )
         {
            case Tables.TypeDef:
               // Easy way out
               otherMD = md;
               typeDefIndex = idx;
               break;
            case Tables.TypeRef:
               var cache = this.GetCacheFor( md );
               MDSpecificCache otherCache;
               cache.ResolveTypeNameFromTypeRef( idx, out otherCache, out typeDefIndex );
               otherMD = otherCache?.MD;
               break;
            case Tables.TypeSpec:
               throw new NotImplementedException( "Resolving type name from type spec." );
            default:
               otherMD = null;
               typeDefIndex = -1;
               break;
         }

         return otherMD != null && typeDefIndex >= 0;
      }

      /// <summary>
      /// Clears all cached information of this <see cref="MetaDataResolver"/>.
      /// </summary>
      public void ClearCache()
      {
         this._mdCaches.Clear();
      }

      /// <inheritdoc />
      public event EventHandler<AssemblyReferenceResolveEventArgs> AssemblyReferenceResolveEvent;

      /// <inheritdoc />
      public event EventHandler<ModuleReferenceResolveEventArgs> ModuleReferenceResolveEvent;

      private MDSpecificCache ResolveAssemblyReferenceWithEvent( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData thisMD, String assemblyName, AssemblyInformationForResolving assemblyInfo ) //, Boolean isRetargetable )
      {
         var args = new AssemblyReferenceResolveEventArgs( thisMD, assemblyName, assemblyInfo ); //, isRetargetable );
         this.AssemblyReferenceResolveEvent?.Invoke( this, args );
         return this.GetCacheFor( args.ResolvedMetaData );
      }

      private MDSpecificCache ResolveModuleReferenceWithEvent( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData thisMD, String moduleName )
      {
         var args = new ModuleReferenceResolveEventArgs( thisMD, moduleName );
         this.ModuleReferenceResolveEvent?.Invoke( this, args );
         return this.GetCacheFor( args.ResolvedMetaData );
      }

      private MDSpecificCache ResolveAssemblyByString( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData md, String assemblyString )
      {
         return this.GetCacheFor( md ).ResolveCacheByAssemblyString( assemblyString );
      }

      private String ResolveTypeNameFromTypeDef( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData md, Int32 index )
      {
         return this.UseMDCache( md, c => c.ResolveTypeNameFromTypeDef( index ) );
      }

      private String ResolveTypeNameFromTypeRef( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData md, Int32 index )
      {
         return this.UseMDCache( md, c => c.ResolveTypeNameFromTypeRef( index ) );
      }

      private MDSpecificCache MDSpecificCacheFactory( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData md )
      {
         return new MDSpecificCache( this, md );
      }

      private MDSpecificCache GetCacheFor( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData otherMD )
      {
         return otherMD == null ?
            null :
            this._mdCaches.GetOrAdd_NotThreadSafe( otherMD, this._mdCacheFactory );
      }


      private T UseMDCache<T>( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData md, Func<MDSpecificCache, T> func )
         where T : class
      {
         var cache = this.GetCacheFor( md );
         return cache == null ? null : func( cache );
      }
   }


}
