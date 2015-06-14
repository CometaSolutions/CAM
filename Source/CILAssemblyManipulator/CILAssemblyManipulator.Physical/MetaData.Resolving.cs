using CILAssemblyManipulator.Physical;
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
using CILAssemblyManipulator.Physical.Implementation;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
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
         private readonly IDictionary<Int32, CustomAttributeArgumentType> _typeDefCache; // Key: TypeDef index, Value: CA type
         private readonly IDictionary<Int32, Tuple<MDSpecificCache, Int32>> _typeRefCache; // Key: TypeRef index, Value: TypeDef index in another metadata
         private readonly IDictionary<String, Int32> _typeNameCache; // Key - type name (ns + enclosing classes + type name), Value - TypeDef index

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
            this._typeDefCache = new Dictionary<Int32, CustomAttributeArgumentType>();
            this._typeRefCache = new Dictionary<Int32, Tuple<MDSpecificCache, Int32>>();
            this._typeNameCache = new Dictionary<String, Int32>();
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

         internal CustomAttributeArgumentType ResolveTypeFromTypeName( String typeName )
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
                  this.ResolveTypeFromTypeName( typeName, out tDefIndex );
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

         internal CustomAttributeArgumentType ResolveTypeFromTypeDef( Int32 index )
         {
            return index < 0 ?
               null :
               this._typeDefCache
                  .GetOrAdd_NotThreadSafe( index, idx =>
                  {
                     var md = this._md;
                     TableIndex? dummy;
                     var enumFieldIndex = ModuleReader.GetEnumValueFieldIndex( md, idx, out dummy );

                     CustomAttributeArgumentType retVal = null;
                     if ( enumFieldIndex >= 0 )
                     {
                        var sig = md.FieldDefinitions[enumFieldIndex].Signature.Type;
                        if ( sig != null && sig.TypeSignatureKind == TypeSignatureKind.Simple )
                        {
                           retVal = ResolveCATypeSimple( ( (SimpleTypeSignature) sig ).SimpleType );
                        }
                     }

                     return retVal;
                  } );
         }

         internal CustomAttributeArgumentType ResolveTypeFromTypeRef( Int32 index )
         {
            MDSpecificCache otherMD; Int32 tDefIndex;
            this.ResolveTypeFromTypeRef( index, out otherMD, out tDefIndex );
            return otherMD == null ? null : otherMD.ResolveTypeFromTypeDef( tDefIndex );
         }

         private void ResolveTypeFromTypeRef( Int32 index, out MDSpecificCache otherMDParam, out Int32 tDefIndexParam )
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
                           this.ResolveTypeFromTypeRef( resIdx, out otherMD, out tDefIndex );
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

         internal Int32 ResolveTopLevelType( String typeName, String typeNamespace )
         {
            return this._topLevelTypeCache
               .GetOrAdd_NotThreadSafe( new KeyValuePair<String, String>( typeNamespace, typeName ), kvp =>
               {
                  var md = this._md;
                  var ns = kvp.Key;
                  var tn = kvp.Value;

                  var hasNS = !String.IsNullOrEmpty( ns );
                  var suitableIndex = md.TypeDefinitions.FindIndex( tDef =>
                     String.Equals( tDef.Name, tn )
                     && (
                        ( hasNS && String.Equals( tDef.Namespace, ns ) )
                        || ( !hasNS && String.IsNullOrEmpty( tDef.Namespace ) )
                     ) );

                  // Check that this is not nested type
                  if ( suitableIndex >= 0
                     && md.NestedClassDefinitions.GetReferencingRowsFromOrdered( Tables.TypeDef, suitableIndex, nc => nc.NestedClass ).Any() // this will be true if the type definition at index 'suitableIndex' has declaring type, i.e. it is nested type
                     )
                  {
                     suitableIndex = -1;
                  }

                  return suitableIndex;
               } );
         }

         internal Int32 FindNestedTypeIndex( Int32 enclosingTypeIndex, String nestedTypeName )
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
                  nestedTD = md.NestedClassDefinitions
                     .SkipWhile( nc => nc.NestedClass.Index <= enclosingTypeIndex )
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

      public event EventHandler<AssemblyReferenceResolveEventArgs> AssemblyReferenceResolveEvent;

      public event EventHandler<ModuleReferenceResolveEventArgs> ModuleReferenceResolveEvent;

      public void ResolveCustomAttributeSignature(
         CILMetaData md,
         Int32 index
         )
      {
         ArgumentValidator.ValidateNotNull( "Metadata", md );

         var customAttribute = md.CustomAttributeDefinitions.GetOrNull( index );
         if ( customAttribute != null )
         {
            var caSig = customAttribute.Signature as RawCustomAttributeSignature;
            if ( caSig != null )
            {
               var ca = this.TryResolveCustomAttributeSignature( md, caSig.Bytes, 0, customAttribute.Type );
               if ( ca != null )
               {
                  customAttribute.Signature = ca;
               }
            }
         }
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
               ctorSig = caTypeTableIndex.Index < md.MethodDefinitions.Count ?
                  md.MethodDefinitions[caTypeTableIndex.Index].Signature :
                  null;
               break;
            case Tables.MemberRef:
               ctorSig = caTypeTableIndex.Index < md.MemberReferences.Count ?
                  md.MemberReferences[caTypeTableIndex.Index].Signature as AbstractMethodSignature :
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
               var caType = this.ConvertTypeSignatureToCustomAttributeType( md, ctorSig.Parameters[i].Type );
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

      private CustomAttributeArgumentType ConvertTypeSignatureToCustomAttributeType(
         CILMetaData md,
         TypeSignature type
         )
      {
         switch ( type.TypeSignatureKind )
         {
            case TypeSignatureKind.SimpleArray:
               var arrayType = new CustomAttributeArgumentTypeArray();
               var caType = this.ConvertTypeSignatureToCustomAttributeType( md, ( (SimpleArrayTypeSignature) type ).ArrayType );
               if ( caType == null )
               {
                  return null;
               }
               else
               {
                  arrayType.ArrayType = caType;
                  return arrayType;
               }
            case TypeSignatureKind.Simple:
               return ResolveCATypeSimple( ( (SimpleTypeSignature) type ).SimpleType );
            case TypeSignatureKind.ClassOrValue:
               // Either enum or System.Type
               var tIdx = ( (ClassOrValueTypeSignature) type ).Type;
               return IsTypeType( md, tIdx ) ? // Avoid loading mscorlib metadata if this is System.Type
                  CustomAttributeArgumentTypeSimple.Type :
                  this.ResolveCATypeFromTableIndex( md, tIdx );
            default:
               return null;
         }
      }


      private MDSpecificCache ResolveAssemblyReferenceWithEvent( CILMetaData thisMD, String assemblyName, AssemblyInformationForResolving? assemblyInfo )
      {
         var args = new AssemblyReferenceResolveEventArgs( thisMD, assemblyName, assemblyInfo );
         this.AssemblyReferenceResolveEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
         return this.GetCacheFor( args.ResolvedMetaData );
      }

      private MDSpecificCache ResolveModuleReferenceWithEvent( CILMetaData thisMD, String moduleName )
      {
         var args = new ModuleReferenceResolveEventArgs( thisMD, moduleName );
         this.ModuleReferenceResolveEvent.InvokeEventIfNotNull( evt => evt( this, args ) );
         return this.GetCacheFor( args.ResolvedMetaData );
      }

      private MDSpecificCache ResolveAssemblyByString( CILMetaData md, String assemblyString )
      {
         return this.GetCacheFor( md ).ResolveCacheByAssemblyString( assemblyString );
      }

      private CustomAttributeArgumentType ResolveTypeFromFullName( CILMetaData md, String typeString )
      {
         // 1. See if there is assembly name present
         // 2. If present, then resolve assembly by name
         // 3. If not present, then try this assembly and then 'mscorlib'
         // 4. Resolve table index by string
         // 5. Resolve return value by CILMetaData + TableIndex pair

         String typeName, assemblyName;
         var assemblyNamePresent = typeString.ParseFullTypeString( out typeName, out assemblyName );
         var targetModule = assemblyNamePresent ? this.ResolveAssemblyByString( md, assemblyName ) : this.GetCacheFor( md );

         var retVal = targetModule == null ? null : targetModule.ResolveTypeFromTypeName( typeName );
         if ( retVal == null && !assemblyNamePresent )
         {
            // TODO try 'mscorlib' unless this is mscorlib
         }

         return retVal;
      }


      private CustomAttributeArgumentType ResolveTypeFromTypeDef( CILMetaData md, Int32 index )
      {
         return this.UseMDCache( md, c => c.ResolveTypeFromTypeDef( index ) );
      }

      private CustomAttributeArgumentType ResolveTypeFromTypeRef( CILMetaData md, Int32 index )
      {
         return this.UseMDCache( md, c => c.ResolveTypeFromTypeRef( index ) );
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

      private CustomAttributeArgumentType ResolveCATypeFromTableIndex(
         CILMetaData md,
         TableIndex tIdx
         )
      {
         var idx = tIdx.Index;
         CustomAttributeArgumentType retVal;
         switch ( tIdx.Table )
         {
            case Tables.TypeDef:
               retVal = this.ResolveTypeFromTypeDef( md, tIdx.Index );
               break;
            case Tables.TypeRef:
               retVal = this.ResolveTypeFromTypeRef( md, idx );
               break;
            case Tables.TypeSpec:
               // Should never happen but one never knows...
               // Recursion within same metadata:
               var tSpec = md.TypeSpecifications.GetOrNull( idx );
               retVal = tSpec == null ?
                  null :
                  ConvertTypeSignatureToCustomAttributeType( md, tSpec.Signature );
               break;
            default:
               retVal = null;
               break;
         }

         return retVal;
      }

      private static Boolean IsTypeType( CILMetaData md, TableIndex? tIdx )
      {
         return ModuleReader.IsSystemType( md, tIdx, Consts.TYPE_NAMESPACE, Consts.TYPE_TYPENAME );
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
            CustomAttributeTypedArgument nestedCAType;
            switch ( type.ArgumentTypeKind )
            {
               case CustomAttributeArgumentTypeKind.Array:
                  var amount = caBLOB.ReadInt32LEFromBytes( ref idx );
                  if ( ( (UInt32) amount ) == 0xFFFFFFFF )
                  {
                     value = null;
                  }
                  else
                  {
                     var array = new Object[amount];
                     var elemType = ( (CustomAttributeArgumentTypeArray) type ).ArrayType;
                     for ( var i = 0; i < amount && success; ++i )
                     {
                        if ( TryReadCAFixedArgument( md, caBLOB, ref idx, elemType, out nestedCAType ) )
                        {
                           array[i] = nestedCAType.Value;
                        }
                        else
                        {
                           success = false;
                        }
                     }
                     value = array;
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
                        value = caBLOB.ReadLenPrefixedUTF8String( ref idx );
                        break;
                     case SignatureElementTypes.Object:
                        type = ReadCAFieldOrPropType( caBLOB, ref idx );
                        success = TryReadCAFixedArgument( md, caBLOB, ref idx, type, out nestedCAType );
                        value = success ? nestedCAType.Value : null;
                        break;
                     case SignatureElementTypes.Type:
                        value = caBLOB.ReadLenPrefixedUTF8String( ref idx );
                        break;
                     default:
                        value = null;
                        break;
                  }
                  break;
               case CustomAttributeArgumentTypeKind.TypeString:
                  type = this.ResolveTypeFromFullName( md, ( (CustomAttributeArgumentTypeEnum) type ).TypeString );
                  success = TryReadCAFixedArgument( md, caBLOB, ref idx, type, out nestedCAType );
                  value = success ? nestedCAType.Value : null;
                  break;
               default:
                  value = null;
                  break;
            }
         }

         return success ?
            new CustomAttributeTypedArgument()
            {
               Type = type,
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
               return new CustomAttributeArgumentTypeEnum()
               {
                  TypeString = array.ReadLenPrefixedUTF8String( ref idx )
               };
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
         if ( type != null )
         {
            var name = blob.ReadLenPrefixedUTF8String( ref idx );
            var typedArg = this.ReadCAFixedArgument( md, blob, ref idx, type );
            if ( typedArg != null )
            {
               retVal = new CustomAttributeNamedArgument()
               {
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

      internal AssemblyReferenceResolveEventArgs( CILMetaData thisMD, String assemblyName, AssemblyInformationForResolving? assemblyInfo )
         : base( thisMD )
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

   private static void UseResolver<T>( this MetaDataResolver resolver, CILMetaData md, IList<T> list, Action<MetaDataResolver, CILMetaData, Int32> action )
   {
      ArgumentValidator.ValidateNotNull( "Metadata", md );

      var max = list.Count;
      for ( var i = 0; i < max; ++i )
      {
         action( resolver, md, i );
      }
   }

   public static AssemblyInformationForResolving NewInformationForResolving( this AssemblyReference assemblyRef )
   {
      return new AssemblyInformationForResolving( assemblyRef.AssemblyInformation, assemblyRef.Attributes.IsFullPublicKey() );
   }
}
