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

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.IO;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TabularMetaData;

namespace CILAssemblyManipulator.Physical.IO
{
   /// <summary>
   /// The sole purpose of this class is to resolve <see cref="RawCustomAttributeSignature"/>s and <see cref="RawSecurityInformation"/>s into <see cref="CustomAttributeSignature"/>s and <see cref="SecurityInformation"/>, respectively.
   /// </summary>
   /// <remarks>
   /// <para>
   /// This class is rarely used directly, as e.g. <see cref="CILMetaDataLoader"/> will use this by default in <see cref="CILMetaDataLoader.ResolveMetaData()"/> method.
   /// </para>
   /// <para>
   /// The custom attribute signatures are serialized in meta data (see ECMA-335 spec for more info) in such way that enum values have their type names present, but the underlying enum value type (e.g. integer) is not present.
   /// Therefore, the custom attribute signatures, and security signatures (which share some serialization functionality with custom attribute signatures) require dynamic resolving of what is the underlying enum value type.
   /// This class encapsulates this resolving process, which may be complicated and involve loading of several additional assemblies.
   /// </para>
   /// </remarks>
   public sealed class MetaDataResolver
   {
      private sealed class MDSpecificCache
      {
         private static readonly Object NULL = new Object();

         private readonly MetaDataResolver _owner;
         private readonly CILMetaData _md;

         private readonly IDictionary<Object, MDSpecificCache> _assemblyResolveFreeFormCache;
         private readonly IDictionary<Int32, MDSpecificCache> _assembliesByInfoCache;
         private readonly IDictionary<Int32, MDSpecificCache> _modulesCache;

         private readonly IDictionary<KeyValuePair<String, String>, Int32> _topLevelTypeCache; // Key: ns + type pair, Value: TypeDef index
         private readonly IDictionary<Int32, CustomAttributeArgumentTypeSimple> _typeDefCache; // Key: TypeDef index, Value: CA type
         private readonly IDictionary<Int32, Tuple<MDSpecificCache, Int32>> _typeRefCache; // Key: TypeRef index, Value: TypeDef index in another metadata
         private readonly IDictionary<String, Int32> _typeNameCache; // Key - type name (ns + enclosing classes + type name), Value - TypeDef index
         private readonly IDictionary<Int32, String> _typeNameReverseCache; // Key - typeDefIndex, Value - type name
         private readonly IDictionary<Int32, String> _typeRefReverseCache;

         internal MDSpecificCache( MetaDataResolver owner, CILMetaData md )
         {
            ArgumentValidator.ValidateNotNull( "Owner", owner );
            ArgumentValidator.ValidateNotNull( "Metadata", md );

            this._owner = owner;
            this._md = md;

            this._assemblyResolveFreeFormCache = new Dictionary<Object, MDSpecificCache>();
            this._assembliesByInfoCache = new Dictionary<Int32, MDSpecificCache>();
            this._modulesCache = new Dictionary<Int32, MDSpecificCache>();

            this._topLevelTypeCache = new Dictionary<KeyValuePair<String, String>, Int32>();
            this._typeDefCache = new Dictionary<Int32, CustomAttributeArgumentTypeSimple>();
            this._typeRefCache = new Dictionary<Int32, Tuple<MDSpecificCache, Int32>>();
            this._typeNameCache = new Dictionary<String, Int32>();
            this._typeNameReverseCache = new Dictionary<Int32, String>();
            this._typeRefReverseCache = new Dictionary<Int32, String>();
         }

         internal CILMetaData MD
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
                  parseSuccessful ? (AssemblyInformationForResolving) kkey : (AssemblyInformationForResolving?) null
                  )
               );
         }

         internal CustomAttributeArgumentTypeSimple ResolveTypeFromTypeName( String typeName )
         {
            Int32 tDefIdx;
            this.ResolveTypeFromTypeName( typeName, out tDefIdx );

            return this.ResolveTypeFromTypeDef( tDefIdx );
         }

         private void ResolveTypeFromTypeName( String typeName, out Int32 tDefIndexParam )
         {
            tDefIndexParam = this._typeNameCache.GetOrAdd_NotThreadSafe( typeName, tn =>
            {
               Int32 tDefIndex;
               String enclosingType, nestedType;
               var isNestedType = typeName.ParseTypeNameStringForNestedType( out enclosingType, out nestedType );
               if ( isNestedType )
               {
                  this.ResolveTypeFromTypeName( enclosingType, out tDefIndex );
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

         internal CustomAttributeArgumentTypeSimple ResolveTypeFromTypeDef( Int32 index )
         {
            return index < 0 ?
               null :
               this._typeDefCache
                  .GetOrAdd_NotThreadSafe( index, idx =>
                  {
                     var md = this._md;
                     Int32 enumFieldIndex;
                     CustomAttributeArgumentTypeSimple retVal = null;
                     if ( md.TryGetEnumValueFieldIndex( idx, out enumFieldIndex ) )
                     {
                        var sig = md.FieldDefinitions.TableContents[enumFieldIndex].Signature.Type;
                        if ( sig != null && sig.TypeSignatureKind == TypeSignatureKind.Simple )
                        {
                           retVal = ResolveCATypeSimple( ( (SimpleTypeSignature) sig ).SimpleType );
                        }
                     }

                     return retVal;
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

         private void ResolveTypeNameFromTypeRef( Int32 index, out MDSpecificCache otherMDParam, out Int32 tDefIndexParam )
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
            if ( md == null )
            {
               retVal = -1;
            }
            else
            {
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
            }

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
                  return aRef == null ? null : this._owner.ResolveAssemblyReferenceWithEvent( this._md, null, aRef.NewInformationForResolving() );
               } );
         }
      }

      private readonly IDictionary<CILMetaData, MDSpecificCache> _mdCaches;
      private readonly Func<CILMetaData, MDSpecificCache> _mdCacheFactory;

      public MetaDataResolver()
      {
         this._mdCaches = new Dictionary<CILMetaData, MDSpecificCache>();
         this._mdCacheFactory = this.MDSpecificCacheFactory;
      }

      public void ClearCache()
      {
         this._mdCaches.Clear();
      }

      /// <summary>
      /// This event will be fired when an assembly reference will need to be resolved.
      /// </summary>
      /// <seealso cref="AssemblyReferenceResolveEventArgs"/>
      public event EventHandler<AssemblyReferenceResolveEventArgs> AssemblyReferenceResolveEvent;

      /// <summary>
      /// This event will be fired when a module reference will need to be resolved.
      /// </summary>
      /// <seealso cref="ModuleReferenceResolveEventArgs"/>
      public event EventHandler<ModuleReferenceResolveEventArgs> ModuleReferenceResolveEvent;

      /// <summary>
      /// Tries to resolve a custom attribute signature in a given <see cref="CILMetaData"/> at given index.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/>.</param>
      /// <param name="index">The index in <see cref="CILMetaData.CustomAttributeDefinitions"/> to resolve signature.</param>
      /// <returns>non-<c>null</c> resolved signature, or <c>null</c> if resolving was unsuccessful.</returns>
      public CustomAttributeSignature ResolveCustomAttributeSignature(
         CILMetaData md,
         Int32 index
         )
      {
         ArgumentValidator.ValidateNotNull( "Metadata", md );

         var customAttribute = md.CustomAttributeDefinitions.GetOrNull( index );
         CustomAttributeSignature signature = null;

         if ( customAttribute != null )
         {
            var caSig = customAttribute.Signature as RawCustomAttributeSignature;
            if ( caSig != null )
            {
               signature = this.TryResolveCustomAttributeSignature( md, caSig.Bytes, 0, customAttribute.Type );
               if ( signature != null )
               {
                  customAttribute.Signature = signature;
               }
            }
         }

         return signature;
      }

      public void ResolveSecurityDeclaration(
         CILMetaData md,
         Int32 index
         )
      {
         ArgumentValidator.ValidateNotNull( "Metadata", md );

         var sec = md.SecurityDefinitions.GetOrNull( index );
         if ( sec != null )
         {
            var permissions = sec.PermissionSets;
            for ( var i = 0; i < permissions.Count; ++i )
            {
               var permission = permissions[i] as RawSecurityInformation;
               if ( permission != null )
               {
                  var idx = 0;
                  var bytes = permission.Bytes;
                  var argCount = permission.ArgumentCount;
                  var secInfo = new SecurityInformation( argCount ) { SecurityAttributeType = permission.SecurityAttributeType };
                  var success = true;
                  for ( var j = 0; j < argCount && success; ++j )
                  {
                     var arg = this.ReadCANamedArgument( md, bytes, ref idx );
                     if ( arg == null )
                     {
                        success = false;
                     }
                     else
                     {
                        secInfo.NamedArguments.Add( arg );
                     }
                  }

                  if ( success )
                  {
                     permissions[i] = secInfo;
                  }
               }
            }
         }
      }

      internal CustomAttributeSignature TryResolveCustomAttributeSignature(
         CILMetaData md,
         Byte[] blob,
         Int32 idx,
         TableIndex caTypeTableIndex
         )
      {

         AbstractMethodSignature ctorSig;
         switch ( caTypeTableIndex.Table )
         {
            case Tables.MethodDef:
               ctorSig = caTypeTableIndex.Index < md.MethodDefinitions.GetRowCount() ?
                  md.MethodDefinitions.TableContents[caTypeTableIndex.Index].Signature :
                  null;
               break;
            case Tables.MemberRef:
               ctorSig = caTypeTableIndex.Index < md.MemberReferences.GetRowCount() ?
                  md.MemberReferences.TableContents[caTypeTableIndex.Index].Signature as AbstractMethodSignature :
                  null;
               break;
            default:
               ctorSig = null;
               break;
         }

         var success = ctorSig != null;
         CustomAttributeSignature retVal = null;
         if ( success )
         {
            var startIdx = idx;
            retVal = new CustomAttributeSignature( typedArgsCount: ctorSig.Parameters.Count );

            idx += 2; // Skip prolog

            for ( var i = 0; i < ctorSig.Parameters.Count; ++i )
            {
               var caType = md.ResolveCACtorType( ctorSig.Parameters[i].Type, tIdx => this.ResolveCATypeFromTableIndex( md, tIdx ) );
               if ( caType == null )
               {
                  // We don't know the size of the type -> stop
                  retVal.TypedArguments.Clear();
                  break;
               }
               else
               {
                  retVal.TypedArguments.Add( ReadCAFixedArgument( md, blob, ref idx, caType ) );
               }
            }

            // Check if we had failed to resolve ctor type before.
            success = retVal.TypedArguments.Count == ctorSig.Parameters.Count;
            if ( success )
            {
               var namedCount = blob.ReadUInt16LEFromBytes( ref idx );
               for ( var i = 0; i < namedCount && success; ++i )
               {
                  var caNamedArg = this.ReadCANamedArgument( md, blob, ref idx );

                  if ( caNamedArg == null )
                  {
                     // We don't know the size of the type -> stop
                     success = false;
                  }
                  else
                  {
                     retVal.NamedArguments.Add( caNamedArg );
                  }
               }
            }
         }
         return success ? retVal : null;
      }

      private MDSpecificCache ResolveAssemblyReferenceWithEvent( CILMetaData thisMD, String assemblyName, AssemblyInformationForResolving? assemblyInfo ) //, Boolean isRetargetable )
      {
         var args = new AssemblyReferenceResolveEventArgs( thisMD, assemblyName, assemblyInfo ); //, isRetargetable );
         this.AssemblyReferenceResolveEvent?.Invoke( this, args );
         return this.GetCacheFor( args.ResolvedMetaData );
      }

      private MDSpecificCache ResolveModuleReferenceWithEvent( CILMetaData thisMD, String moduleName )
      {
         var args = new ModuleReferenceResolveEventArgs( thisMD, moduleName );
         this.ModuleReferenceResolveEvent?.Invoke( this, args );
         return this.GetCacheFor( args.ResolvedMetaData );
      }

      private MDSpecificCache ResolveAssemblyByString( CILMetaData md, String assemblyString )
      {
         return this.GetCacheFor( md ).ResolveCacheByAssemblyString( assemblyString );
      }

      private CustomAttributeArgumentTypeSimple ResolveTypeFromFullName( CILMetaData md, String typeString )
      {
         // 1. See if there is assembly name present
         // 2. If present, then resolve assembly by name
         // 3. If not present, then try this assembly and then 'mscorlib'
         // 4. Resolve table index by string
         // 5. Resolve return value by CILMetaData + TableIndex pair

         String typeName, assemblyName;
         var assemblyNamePresent = typeString.ParseAssemblyQualifiedTypeString( out typeName, out assemblyName );
         var targetModule = assemblyNamePresent ? this.ResolveAssemblyByString( md, assemblyName ) : this.GetCacheFor( md );

         var retVal = targetModule == null ? null : targetModule.ResolveTypeFromTypeName( typeName );
         if ( retVal == null && !assemblyNamePresent )
         {
            // TODO try 'mscorlib' unless this is mscorlib
         }

         return retVal;
      }


      private String ResolveTypeNameFromTypeDef( CILMetaData md, Int32 index )
      {
         return this.UseMDCache( md, c => c.ResolveTypeNameFromTypeDef( index ) );
      }

      private String ResolveTypeNameFromTypeRef( CILMetaData md, Int32 index )
      {
         return this.UseMDCache( md, c => c.ResolveTypeNameFromTypeRef( index ) );
      }

      private MDSpecificCache MDSpecificCacheFactory( CILMetaData md )
      {
         return new MDSpecificCache( this, md );
      }

      private MDSpecificCache GetCacheFor( CILMetaData otherMD )
      {
         return otherMD == null ?
            null :
            this._mdCaches.GetOrAdd_NotThreadSafe( otherMD, this._mdCacheFactory );
      }


      private T UseMDCache<T>( CILMetaData md, Func<MDSpecificCache, T> func )
         where T : class
      {
         var cache = this.GetCacheFor( md );
         return cache == null ? null : func( cache );
      }


      private static CustomAttributeArgumentTypeSimple ResolveCATypeSimple( SignatureElementTypes elementType )
      {
         switch ( elementType )
         {
            case SignatureElementTypes.Boolean:
               return CustomAttributeArgumentTypeSimple.Boolean;
            case SignatureElementTypes.Char:
               return CustomAttributeArgumentTypeSimple.Char;
            case SignatureElementTypes.I1:
               return CustomAttributeArgumentTypeSimple.SByte;
            case SignatureElementTypes.U1:
               return CustomAttributeArgumentTypeSimple.Byte;
            case SignatureElementTypes.I2:
               return CustomAttributeArgumentTypeSimple.Int16;
            case SignatureElementTypes.U2:
               return CustomAttributeArgumentTypeSimple.UInt16;
            case SignatureElementTypes.I4:
               return CustomAttributeArgumentTypeSimple.Int32;
            case SignatureElementTypes.U4:
               return CustomAttributeArgumentTypeSimple.UInt32;
            case SignatureElementTypes.I8:
               return CustomAttributeArgumentTypeSimple.Int64;
            case SignatureElementTypes.U8:
               return CustomAttributeArgumentTypeSimple.UInt64;
            case SignatureElementTypes.R4:
               return CustomAttributeArgumentTypeSimple.Single;
            case SignatureElementTypes.R8:
               return CustomAttributeArgumentTypeSimple.Double;
            case SignatureElementTypes.String:
               return CustomAttributeArgumentTypeSimple.String;
            case SignatureElementTypes.Object:
               return CustomAttributeArgumentTypeSimple.Object;
            case SignatureElementTypes.Type:
               return CustomAttributeArgumentTypeSimple.Type;
            default:
               return null;
         }
      }

      private CustomAttributeArgumentTypeEnum ResolveCATypeFromTableIndex(
         CILMetaData md,
         TableIndex tIdx
         )
      {
         var idx = tIdx.Index;
         String retVal;
         switch ( tIdx.Table )
         {
            case Tables.TypeDef:
               retVal = this.ResolveTypeNameFromTypeDef( md, tIdx.Index );
               break;
            case Tables.TypeRef:
               retVal = this.ResolveTypeNameFromTypeRef( md, idx );
               break;
            //case Tables.TypeSpec:
            //   // Should never happen but one never knows...
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

         return retVal == null ? null : new CustomAttributeArgumentTypeEnum()
         {
            TypeString = retVal
         };
      }

      private Boolean TryReadCAFixedArgument(
         CILMetaData md,
         Byte[] caBLOB,
         ref Int32 idx,
         CustomAttributeArgumentType type,
         out CustomAttributeTypedArgument typedArg
         )
      {
         typedArg = this.ReadCAFixedArgument( md, caBLOB, ref idx, type );
         return typedArg != null;
      }

      private CustomAttributeTypedArgument ReadCAFixedArgument(
         CILMetaData md,
         Byte[] caBLOB,
         ref Int32 idx,
         CustomAttributeArgumentType type
         )
      {
         var success = type != null;

         Object value;
         if ( !success )
         {
            value = null;
         }
         else
         {
            String str;
            CustomAttributeTypedArgument nestedCAType;
            switch ( type.ArgumentTypeKind )
            {
               case CustomAttributeArgumentTypeKind.Array:
                  var amount = caBLOB.ReadInt32LEFromBytes( ref idx );
                  value = null;
                  if ( ( (UInt32) amount ) != 0xFFFFFFFF )
                  {
                     var elemType = ( (CustomAttributeArgumentTypeArray) type ).ArrayType;
                     var arrayType = elemType.GetNativeTypeForCAArrayType();
                     success = arrayType != null;
                     if ( success )
                     {

                        var array = Array.CreateInstance( arrayType, amount );

                        for ( var i = 0; i < amount && success; ++i )
                        {
                           if ( TryReadCAFixedArgument( md, caBLOB, ref idx, elemType, out nestedCAType ) )
                           {
                              array.SetValue( nestedCAType.Value, i );
                           }
                           else
                           {
                              success = false;
                           }
                        }
                        value = new CustomAttributeValue_Array( array, elemType );
                     }
                  }
                  break;
               case CustomAttributeArgumentTypeKind.Simple:
                  switch ( ( (CustomAttributeArgumentTypeSimple) type ).SimpleType )
                  {
                     case SignatureElementTypes.Boolean:
                        value = caBLOB.ReadByteFromBytes( ref idx ) == 1;
                        break;
                     case SignatureElementTypes.Char:
                        value = Convert.ToChar( caBLOB.ReadUInt16LEFromBytes( ref idx ) );
                        break;
                     case SignatureElementTypes.I1:
                        value = caBLOB.ReadSByteFromBytes( ref idx );
                        break;
                     case SignatureElementTypes.U1:
                        value = caBLOB.ReadByteFromBytes( ref idx );
                        break;
                     case SignatureElementTypes.I2:
                        value = caBLOB.ReadInt16LEFromBytes( ref idx );
                        break;
                     case SignatureElementTypes.U2:
                        value = caBLOB.ReadUInt32LEFromBytes( ref idx );
                        break;
                     case SignatureElementTypes.I4:
                        value = caBLOB.ReadInt32LEFromBytes( ref idx );
                        break;
                     case SignatureElementTypes.U4:
                        value = caBLOB.ReadUInt32LEFromBytes( ref idx );
                        break;
                     case SignatureElementTypes.I8:
                        value = caBLOB.ReadInt64LEFromBytes( ref idx );
                        break;
                     case SignatureElementTypes.U8:
                        value = caBLOB.ReadUInt64LEFromBytes( ref idx );
                        break;
                     case SignatureElementTypes.R4:
                        value = caBLOB.ReadSingleLEFromBytes( ref idx );
                        break;
                     case SignatureElementTypes.R8:
                        value = caBLOB.ReadDoubleLEFromBytes( ref idx );
                        break;
                     case SignatureElementTypes.String:
                        success = caBLOB.ReadLenPrefixedUTF8String( ref idx, out str );
                        value = str;
                        break;
                     case SignatureElementTypes.Object:
                        type = ReadCAFieldOrPropType( caBLOB, ref idx );
                        success = TryReadCAFixedArgument( md, caBLOB, ref idx, type, out nestedCAType );
                        value = success ? nestedCAType.Value : null;
                        break;
                     case SignatureElementTypes.Type:
                        success = caBLOB.ReadLenPrefixedUTF8String( ref idx, out str );
                        value = success ? (Object) new CustomAttributeValue_TypeReference( str ) : null;
                        break;
                     default:
                        value = null;
                        break;
                  }
                  break;
               case CustomAttributeArgumentTypeKind.TypeString:
                  var enumTypeString = ( (CustomAttributeArgumentTypeEnum) type ).TypeString;
                  var actualType = this.ResolveTypeFromFullName( md, enumTypeString );
                  success = TryReadCAFixedArgument( md, caBLOB, ref idx, actualType, out nestedCAType );
                  value = success ? (Object) new CustomAttributeValue_EnumReference( enumTypeString, nestedCAType.Value ) : null;
                  break;
               default:
                  value = null;
                  break;
            }
         }

         return success ?
            new CustomAttributeTypedArgument()
            {
               Value = value
            } :
            null;
      }

      private static CustomAttributeArgumentType ReadCAFieldOrPropType( Byte[] array, ref Int32 idx )
      {
         var sigType = (SignatureElementTypes) array.ReadByteFromBytes( ref idx );
         switch ( sigType )
         {
            case SignatureElementTypes.CA_Enum:
               String str;
               return array.ReadLenPrefixedUTF8String( ref idx, out str ) ?
                  new CustomAttributeArgumentTypeEnum()
                  {
                     TypeString = str.UnescapeCILTypeString()
                  } :
                  null;
            case SignatureElementTypes.SzArray:
               return new CustomAttributeArgumentTypeArray()
               {
                  ArrayType = ReadCAFieldOrPropType( array, ref idx )
               };
            case SignatureElementTypes.CA_Boxed:
               return CustomAttributeArgumentTypeSimple.Object;
            case SignatureElementTypes.Boolean:
               return CustomAttributeArgumentTypeSimple.Boolean;
            case SignatureElementTypes.Char:
               return CustomAttributeArgumentTypeSimple.Char;
            case SignatureElementTypes.I1:
               return CustomAttributeArgumentTypeSimple.SByte;
            case SignatureElementTypes.U1:
               return CustomAttributeArgumentTypeSimple.Byte;
            case SignatureElementTypes.I2:
               return CustomAttributeArgumentTypeSimple.Int16;
            case SignatureElementTypes.U2:
               return CustomAttributeArgumentTypeSimple.UInt16;
            case SignatureElementTypes.I4:
               return CustomAttributeArgumentTypeSimple.Int32;
            case SignatureElementTypes.U4:
               return CustomAttributeArgumentTypeSimple.UInt32;
            case SignatureElementTypes.I8:
               return CustomAttributeArgumentTypeSimple.Int64;
            case SignatureElementTypes.U8:
               return CustomAttributeArgumentTypeSimple.UInt64;
            case SignatureElementTypes.R4:
               return CustomAttributeArgumentTypeSimple.Single;
            case SignatureElementTypes.R8:
               return CustomAttributeArgumentTypeSimple.Double;
            case SignatureElementTypes.String:
               return CustomAttributeArgumentTypeSimple.String;
            case SignatureElementTypes.Type:
               return CustomAttributeArgumentTypeSimple.Type;
            default:
               return null;
         }
      }

      internal CustomAttributeNamedArgument ReadCANamedArgument(
         CILMetaData md,
         Byte[] blob,
         ref Int32 idx
         )
      {
         var isField = (SignatureElementTypes) blob.ReadByteFromBytes( ref idx ) == SignatureElementTypes.CA_Field;

         var type = ReadCAFieldOrPropType( blob, ref idx );
         CustomAttributeNamedArgument retVal = null;
         String name;
         if ( type != null && blob.ReadLenPrefixedUTF8String( ref idx, out name ) )
         {
            var typedArg = this.ReadCAFixedArgument( md, blob, ref idx, type );
            if ( typedArg != null )
            {
               retVal = new CustomAttributeNamedArgument()
               {
                  FieldOrPropertyType = type,
                  IsField = isField,
                  Name = name,
                  Value = typedArg
               };
            }

         }

         return retVal;
      }

   }

   public abstract class AssemblyOrModuleReferenceResolveEventArgs : EventArgs
   {
      private readonly CILMetaData _thisMD;

      internal AssemblyOrModuleReferenceResolveEventArgs( CILMetaData thisMD )
      {
         ArgumentValidator.ValidateNotNull( "This metadata", thisMD );

         this._thisMD = thisMD;
      }

      public CILMetaData ThisMetaData
      {
         get
         {
            return this._thisMD;
         }
      }


      public CILMetaData ResolvedMetaData { get; set; }
   }

   public sealed class ModuleReferenceResolveEventArgs : AssemblyOrModuleReferenceResolveEventArgs
   {
      private readonly String _moduleName;

      internal ModuleReferenceResolveEventArgs( CILMetaData thisMD, String moduleName )
         : base( thisMD )
      {
         this._moduleName = moduleName;
      }

      public String ModuleName
      {
         get
         {
            return this._moduleName;
         }
      }
   }

   public sealed class AssemblyReferenceResolveEventArgs : AssemblyOrModuleReferenceResolveEventArgs
   {
      private readonly String _assemblyName;
      private readonly AssemblyInformationForResolving? _assemblyInfo;
      //private readonly Boolean _isRetargetable;

      internal AssemblyReferenceResolveEventArgs( CILMetaData thisMD, String assemblyName, AssemblyInformationForResolving? assemblyInfo ) //, Boolean isRetargetable )
         : base( thisMD )
      {
         this._assemblyName = assemblyName;
         this._assemblyInfo = assemblyInfo;
         //this._isRetargetable = isRetargetable;
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

      //public Boolean IsRetargetable
      //{
      //   get
      //   {
      //      return this._isRetargetable;
      //   }
      //}

   }

   public struct AssemblyInformationForResolving : IEquatable<AssemblyInformationForResolving>
   {
      private readonly AssemblyInformation _information;
      private readonly Boolean _isFullPublicKey;
      //private readonly Boolean _isRetargetable;

      public AssemblyInformationForResolving( AssemblyReference aRef )
         : this( aRef.AssemblyInformation.CreateDeepCopy(), aRef.Attributes.IsFullPublicKey() ) //, aRef.Attributes.IsRetargetable() )
      {

      }

      public AssemblyInformationForResolving( AssemblyInformation information, Boolean isFullPublicKey ) //, Boolean isRetargetable )
      {
         ArgumentValidator.ValidateNotNull( "Assembly information", information );

         this._information = information;
         this._isFullPublicKey = isFullPublicKey;
         //this._isRetargetable = isRetargetable;
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

      //public Boolean IsRetargetable
      //{
      //   get
      //   {
      //      return this._isRetargetable;
      //   }
      //}

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

      public static Boolean operator ==( AssemblyInformationForResolving x, AssemblyInformationForResolving y )
      {
         return x.Equals( y );
      }

      public static Boolean operator !=( AssemblyInformationForResolving x, AssemblyInformationForResolving y )
      {
         return !( x == y );
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
}

public static partial class E_CILPhysical
{
   public static void ResolveAllCustomAttributes( this MetaDataResolver resolver, CILMetaData md )
   {
      resolver.UseResolver( md, md.CustomAttributeDefinitions, ( r, m, i ) => r.ResolveCustomAttributeSignature( m, i ) );
   }

   public static void ResolveAllSecurityInformation( this MetaDataResolver resolver, CILMetaData md )
   {
      resolver.UseResolver( md, md.SecurityDefinitions, ( r, m, i ) => r.ResolveSecurityDeclaration( m, i ) );
   }

   public static void ResolveEverything( this MetaDataResolver resolver, CILMetaData md )
   {
      resolver.ResolveAllCustomAttributes( md );
      resolver.ResolveAllSecurityInformation( md );
   }

   private static void UseResolver<T>( this MetaDataResolver resolver, CILMetaData md, MetaDataTable<T> list, Action<MetaDataResolver, CILMetaData, Int32> action )
      where T : class
   {
      ArgumentValidator.ValidateNotNull( "Metadata", md );

      var max = list.GetRowCount();
      for ( var i = 0; i < max; ++i )
      {
         action( resolver, md, i );
      }
   }

   public static AssemblyInformationForResolving NewInformationForResolving( this AssemblyReference assemblyRef )
   {
      return new AssemblyInformationForResolving( assemblyRef );
   }

   /// <summary>
   /// This is helper method to search for custom attribute of type <see cref="System.Runtime.Versioning.TargetFrameworkAttribute"/> attribute applied to the assembly, and creates a <see cref="TargetFrameworkInfo"/> based on the information in the custom attribute signature.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="fwInfo">This parameter will contain the <see cref="TargetFrameworkInfo"/> created based on the information in the assembly.</param>
   /// <param name="resolverToUse">The <see cref="MetaDataResolver"/> to use, if the <see cref="AbstractCustomAttributeSignature"/> of the custom attribute is <see cref="RawCustomAttributeSignature"/>.</param>
   /// <returns><c>true</c> if suitable attribute is found, and the information in the signature is enough to create <see cref="TargetFrameworkInfo"/>; <c>false</c> otherwise.</returns>
   /// <remarks>
   /// <para>
   /// In case of multiple matching custom attributes, the first one in <see cref="CILMetaData.CustomAttributeDefinitions"/> table is used.
   /// </para>
   /// <para>
   /// The assemblies in target framework directory usually don't have the <see cref="System.Runtime.Versioning.TargetFrameworkAttribute"/> on them.
   /// </para>
   /// </remarks>
   /// <exception cref="NullReferenceException">If <paramref name="md"/> is <c>null</c>.</exception>
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

   /// <summary>
   /// Wrapper around <see cref="TryGetTargetFrameworkInformation"/>, that will always return <see cref="TargetFrameworkInfo"/>, but it will be <c>null</c> if <see cref="TryGetTargetFrameworkInformation"/> will return <c>false</c>.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="resolverToUse">The <see cref="MetaDataResolver"/> to use, if the <see cref="AbstractCustomAttributeSignature"/> of the custom attribute is <see cref="RawCustomAttributeSignature"/>.</param>
   /// <returns>The parsed <see cref="TargetFrameworkInfo"/> object, or <c>null</c> if such information could not be found from <paramref name="md"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="md"/> is <c>null</c>.</exception>
   public static TargetFrameworkInfo GetTargetFrameworkInformationOrNull( this CILMetaData md, MetaDataResolver resolverToUse = null )
   {
      TargetFrameworkInfo retVal;
      return md.TryGetTargetFrameworkInformation( out retVal, resolverToUse ) ?
         retVal :
         null;
   }
}
