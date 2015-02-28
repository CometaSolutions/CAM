using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.Implementation
{
   internal static partial class ModuleReader
   {
      private static AbstractSignature ReadMemberRefSignature( Byte[] bytes, Int32 idx )
      {
         return (SignatureStarters) bytes[idx] == SignatureStarters.Field ?
            (AbstractSignature) FieldSignature.ReadFromBytesWithRef( bytes, ref idx ) :
            MethodReferenceSignature.ReadFromBytes( bytes, ref idx );
      }

      private static AbstractSignature ReadStandaloneSignature( Byte[] bytes, Int32 idx )
      {
         // From https://social.msdn.microsoft.com/Forums/en-US/b4252eab-7aae-4456-9829-2707c8459e13/pinned-fields-in-the-common-language-runtime?forum=netfxtoolsdev
         // After messing around further, and noticing that even the C# compiler emits Field signatures in the StandAloneSig table, the signatures seem to relate to PDB debugging symbols.
         // When you emit symbols with the Debug or Release versions of your code, I'm guessing a StandAloneSig entry is injected and referred to by the PDB file.
         // If you are in release mode and you generate no PDB info, the StandAloneSig table contains no Field signatures.
         // One such condition for the emission of such information is constants within the scope of a method body.
         // Original thread:  http://www.netframeworkdev.com/building-development-diagnostic-tools-for-net/field-signatures-in-standalonesig-table-30658.shtml
         return (SignatureStarters) bytes[idx] == SignatureStarters.LocalSignature ?
            (AbstractSignature) LocalVariablesSignature.ReadFromBytes( bytes, ref idx ) :
            ( (SignatureStarters) bytes[idx] == SignatureStarters.Field ?
               null : // We could parse field signature but it sometimes may contain stuff like Pinned etc, which would just mess it up
               MethodReferenceSignature.ReadFromBytes( bytes, ref idx ) );
      }

      private static AbstractCustomAttributeSignature ReadCustomAttributeSignature(
         ModuleLoadingArguments loadingArgs,
         BLOBContainer blobContainer,
         Stream stream,
         CILMetaData md,
         TableIndex methodRef,
         CATypeResolveCache typeResolveCache
         )
      {
         Int32 blobSize, blobIndex;
         var idx = blobContainer.GetBLOBIndex( stream, out blobIndex, out blobSize );
         AbstractCustomAttributeSignature retVal = null;
         if ( blobSize > 2 )
         {
            AbstractMethodSignature ctorSig;
            switch ( methodRef.Table )
            {
               case Tables.MethodDef:
                  ctorSig = methodRef.Index < md.MethodDefinitions.Count ?
                     md.MethodDefinitions[methodRef.Index].Signature :
                     null;
                  break;
               case Tables.MemberRef:
                  ctorSig = methodRef.Index < md.MemberReferences.Count ?
                     md.MemberReferences[methodRef.Index].Signature as AbstractMethodSignature :
                     null;
                  break;
               default:
                  ctorSig = null;
                  break;
            }
            if ( ctorSig != null )
            {
               retVal = ReadCustomAttributeSignature( loadingArgs, md, blobContainer.WholeBLOBArray, idx, ctorSig, typeResolveCache );
            }
         }

         if ( retVal == null )
         {
            retVal = new RawCustomAttributeSignature() { Bytes = blobContainer.GetBLOB( blobIndex ) };
         }

         return retVal;
      }

      private static AbstractCustomAttributeSignature ReadCustomAttributeSignature(
         ModuleLoadingArguments loadingArgs,
         CILMetaData md,
         Byte[] blob,
         Int32 idx,
         AbstractMethodSignature ctorSig,
         CATypeResolveCache typeResolveCache
         )
      {
         var startIdx = idx;
         var retVal = new CustomAttributeSignature( typedArgsCount: ctorSig.Parameters.Count );

         idx += 2; // Skip prolog

         for ( var i = 0; i < ctorSig.Parameters.Count; ++i )
         {
            var caType = ConvertTypeSignatureToCustomAttributeType( loadingArgs, md, ctorSig.Parameters[i].Type, typeResolveCache );
            if ( caType == null )
            {
               // We don't know the size of the type -> stop
               retVal.TypedArguments.Clear();
               break;
            }
            else
            {
               retVal.TypedArguments.Add( ReadCAFixedArgument( md, blob, ref idx, caType, typeResolveCache ) );
            }
         }

         // Check if we had failed to resolve ctor type before.
         var success = retVal.TypedArguments.Count == ctorSig.Parameters.Count;
         if ( success )
         {
            var namedCount = blob.ReadUInt16LEFromBytes( ref idx );
            for ( var i = 0; i < namedCount && success; ++i )
            {
               var caNamedArg = ReadCANamedArgument( md, blob, ref idx, typeResolveCache );

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

         return success ? retVal : null;
      }

      private static CustomAttributeArgumentType ConvertTypeSignatureToCustomAttributeType(
         ModuleLoadingArguments loadingArgs,
         CILMetaData md,
         TypeSignature type,
         CATypeResolveCache typeResolveCache
         )
      {
         switch ( type.TypeSignatureKind )
         {
            case TypeSignatureKind.SimpleArray:
               var arrayType = new CustomAttributeArgumentTypeArray();
               var caType = ConvertTypeSignatureToCustomAttributeType( loadingArgs, md, ( (SimpleArrayTypeSignature) type ).ArrayType, typeResolveCache );
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
               return ResolveCATypeFromTableIndex( loadingArgs, md, ( (ClassOrValueTypeSignature) type ).Type, typeResolveCache );
            default:
               return null;
         }
      }

      private static CustomAttributeArgumentSimple ResolveCATypeSimple( SignatureElementTypes elementType )
      {
         switch ( elementType )
         {
            case SignatureElementTypes.Boolean:
               return CustomAttributeArgumentSimple.Boolean;
            case SignatureElementTypes.Char:
               return CustomAttributeArgumentSimple.Char;
            case SignatureElementTypes.I1:
               return CustomAttributeArgumentSimple.SByte;
            case SignatureElementTypes.U1:
               return CustomAttributeArgumentSimple.Byte;
            case SignatureElementTypes.I2:
               return CustomAttributeArgumentSimple.Int16;
            case SignatureElementTypes.U2:
               return CustomAttributeArgumentSimple.UInt16;
            case SignatureElementTypes.I4:
               return CustomAttributeArgumentSimple.Int32;
            case SignatureElementTypes.U4:
               return CustomAttributeArgumentSimple.UInt32;
            case SignatureElementTypes.I8:
               return CustomAttributeArgumentSimple.Int64;
            case SignatureElementTypes.U8:
               return CustomAttributeArgumentSimple.UInt64;
            case SignatureElementTypes.R4:
               return CustomAttributeArgumentSimple.Single;
            case SignatureElementTypes.R8:
               return CustomAttributeArgumentSimple.Double;
            case SignatureElementTypes.String:
               return CustomAttributeArgumentSimple.String;
            case SignatureElementTypes.Object:
               return CustomAttributeArgumentSimple.Object;
            case SignatureElementTypes.Type:
               return CustomAttributeArgumentSimple.Type;
            default:
               return null;
         }
      }

      private static CustomAttributeArgumentType ResolveCATypeFromTableIndex(
         ModuleLoadingArguments loadingArgs,
         CILMetaData md,
         TableIndex tIdx,
         CATypeResolveCache typeResolveCache
         )
      {
         var idx = tIdx.Index;
         CustomAttributeArgumentType retVal;
         switch ( tIdx.Table )
         {
            case Tables.TypeDef:
               retVal = IsTypeType( md, tIdx ) ?
                  CustomAttributeArgumentSimple.Type :
                  typeResolveCache.ResolveTypeFromTypeDef( md, tIdx.Index );
               break;
            case Tables.TypeRef:
               retVal = IsTypeType( md, tIdx ) ?   // Avoid loading mscorlib metadata if this is type
                  CustomAttributeArgumentSimple.Type :
                  typeResolveCache.ResolveTypeFromTypeRef( md, idx );
               break;
            case Tables.TypeSpec:
               // Should never happen but one never knows...
               // Recursion within same metadata:
               var tSpec = md.TypeSpecifications.GetOrNull( idx );
               retVal = tSpec == null ?
                  null :
                  ConvertTypeSignatureToCustomAttributeType( loadingArgs, md, tSpec.Signature, typeResolveCache );
               break;
            default:
               retVal = null;
               break;
         }

         return retVal;
      }

      private static Boolean IsTypeType( CILMetaData md, TableIndex? tIdx )
      {
         return IsSystemType( md, tIdx, Consts.TYPE_NAMESPACE, Consts.TYPE_TYPENAME );
      }

      private static Boolean TryReadCAFixedArgument(
         CILMetaData md,
         Byte[] caBLOB,
         ref Int32 idx,
         CustomAttributeArgumentType type,
         CATypeResolveCache typeResolveCache,
         out CustomAttributeTypedArgument typedArg
         )
      {
         typedArg = ReadCAFixedArgument( md, caBLOB, ref idx, type, typeResolveCache );
         return typedArg != null;
      }

      private static CustomAttributeTypedArgument ReadCAFixedArgument(
         CILMetaData md,
         Byte[] caBLOB,
         ref Int32 idx,
         CustomAttributeArgumentType type,
         CATypeResolveCache typeResolveCache
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
                        if ( TryReadCAFixedArgument( md, caBLOB, ref idx, elemType, typeResolveCache, out nestedCAType ) )
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
                  switch ( ( (CustomAttributeArgumentSimple) type ).SimpleType )
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
                        success = TryReadCAFixedArgument( md, caBLOB, ref idx, type, typeResolveCache, out nestedCAType );
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
                  type = typeResolveCache.ResolveTypeFromFullName( md, ( (CustomAttributeArgumentTypeString) type ).TypeString );
                  success = TryReadCAFixedArgument( md, caBLOB, ref idx, type, typeResolveCache, out nestedCAType );
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
               return new CustomAttributeArgumentTypeString()
               {
                  TypeString = array.ReadLenPrefixedUTF8String( ref idx )
               };
            case SignatureElementTypes.SzArray:
               return new CustomAttributeArgumentTypeArray()
               {
                  ArrayType = ReadCAFieldOrPropType( array, ref idx )
               };
            case SignatureElementTypes.CA_Boxed:
               return CustomAttributeArgumentSimple.Object;
            case SignatureElementTypes.Boolean:
               return CustomAttributeArgumentSimple.Boolean;
            case SignatureElementTypes.Char:
               return CustomAttributeArgumentSimple.Char;
            case SignatureElementTypes.I1:
               return CustomAttributeArgumentSimple.SByte;
            case SignatureElementTypes.U1:
               return CustomAttributeArgumentSimple.Byte;
            case SignatureElementTypes.I2:
               return CustomAttributeArgumentSimple.Int16;
            case SignatureElementTypes.U2:
               return CustomAttributeArgumentSimple.UInt16;
            case SignatureElementTypes.I4:
               return CustomAttributeArgumentSimple.Int32;
            case SignatureElementTypes.U4:
               return CustomAttributeArgumentSimple.UInt32;
            case SignatureElementTypes.I8:
               return CustomAttributeArgumentSimple.Int64;
            case SignatureElementTypes.U8:
               return CustomAttributeArgumentSimple.UInt64;
            case SignatureElementTypes.R4:
               return CustomAttributeArgumentSimple.Single;
            case SignatureElementTypes.R8:
               return CustomAttributeArgumentSimple.Double;
            case SignatureElementTypes.String:
               return CustomAttributeArgumentSimple.String;
            case SignatureElementTypes.Type:
               return CustomAttributeArgumentSimple.Type;
            default:
               return null;
         }
      }

      private static CustomAttributeNamedArgument ReadCANamedArgument(
         CILMetaData md,
         Byte[] blob,
         ref Int32 idx,
         CATypeResolveCache typeResolveCache
         )
      {
         var isField = (SignatureElementTypes) blob.ReadByteFromBytes( ref idx ) == SignatureElementTypes.CA_Field;

         var type = ReadCAFieldOrPropType( blob, ref idx );
         CustomAttributeNamedArgument retVal = null;
         if ( type != null )
         {
            var name = blob.ReadLenPrefixedUTF8String( ref idx );
            var typedArg = ReadCAFixedArgument( md, blob, ref idx, type, typeResolveCache );
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

      private static void ReadSecurityBLOB(
         CILMetaData md,
         BLOBContainer blobs,
         Stream stream,
         SecurityDefinition declSecurity,
         CATypeResolveCache typeResolveCache
         )
      {
         Int32 blobSize;
         var bIdx = blobs.GetBLOBIndex( stream, out blobSize );
         if ( blobSize > 0 )
         {
            var blob = blobs.WholeBLOBArray;

            if ( blob[bIdx] == MetaDataConstants.DECL_SECURITY_HEADER )
            {
               // New (.NET 2.0+) security spec
               ++bIdx;
               // Amount of security attributes
               var attrCount = blob.DecompressUInt32( ref bIdx );
               for ( var j = 0; j < attrCount; ++j )
               {
                  var secType = blob.ReadLenPrefixedUTF8String( ref bIdx );
                  // For some reason, there is an amount of remaining bytes here
                  blob.DecompressUInt32( ref bIdx );
                  // Now, amount of named args
                  var argCount = blob.DecompressUInt32( ref bIdx );
                  var secInfo = new SecurityInformation( argCount )
                  {
                     SecurityAttributeType = secType
                  };
                  // Read named args
                  for ( var k = 0; k < argCount; ++k )
                  {
                     secInfo.NamedArguments.Add( ReadCANamedArgument( md, blob, ref bIdx, typeResolveCache ) );
                  }
                  declSecurity.PermissionSets.Add( secInfo );
               }
            }
            else
            {
               // Old (.NET 1.x) security spec
               // Create a single SecurityInformation with PermissionSetAttribute type and XML property argument containing the XML of the blob
               var secInfo = new SecurityInformation( 1 )
               {
                  SecurityAttributeType = Consts.PERMISSION_SET
               };
               secInfo.NamedArguments.Add( new CustomAttributeNamedArgument()
               {
                  IsField = false,
                  Name = Consts.PERMISSION_SET_XML_PROP,
                  Value = new CustomAttributeTypedArgument()
                  {
                     Type = CustomAttributeArgumentSimple.String,
                     Value = MetaDataConstants.USER_STRING_ENCODING.GetString( blob, bIdx, blobSize )
                  }
               } );
               declSecurity.PermissionSets.Add( secInfo );
            }
         }
      }

      private sealed class CATypeResolveCache
      {
         private sealed class MDSpecificCache
         {
            private readonly CATypeResolveCache _owner;
            private readonly CILMetaData _md;

            private readonly IDictionary<KeyValuePair<String, String>, Int32> _topLevelTypeCache; // Key: ns + type pair, Value: TypeDef index
            private readonly IDictionary<Int32, CustomAttributeArgumentType> _typeDefCache; // Key: TypeDef index, Value: CA type
            private readonly IDictionary<Int32, Tuple<CILMetaData, Int32>> _typeRefCache; // Key: TypeRef index, Value: TypeDef index in another metadata
            private readonly IDictionary<String, Int32> _typeNameCache; // Key - type name (ns + enclosing classes + type name), Value - TypeDef index

            internal MDSpecificCache( CATypeResolveCache owner, CILMetaData md )
            {
               ArgumentValidator.ValidateNotNull( "Owner", owner );
               ArgumentValidator.ValidateNotNull( "Metadata", md );

               this._owner = owner;
               this._md = md;

               this._topLevelTypeCache = new Dictionary<KeyValuePair<String, String>, Int32>();
               this._typeDefCache = new Dictionary<Int32, CustomAttributeArgumentType>();
               this._typeRefCache = new Dictionary<Int32, Tuple<CILMetaData, Int32>>();
               this._typeNameCache = new Dictionary<String, Int32>();
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
               CILMetaData otherMD; Int32 tDefIndex;
               this.ResolveTypeFromTypeRef( index, out otherMD, out tDefIndex );
               var otherCache = this.GetOtherCache( otherMD );
               return otherCache == null ? null : otherCache.ResolveTypeFromTypeDef( index );
            }

            private void ResolveTypeFromTypeRef( Int32 index, out CILMetaData otherMDParam, out Int32 tDefIndexParam )
            {
               var tuple = this._typeRefCache.GetOrAdd_NotThreadSafe( index, idx =>
               {
                  var md = this._md;
                  var tRef = md.TypeReferences.GetOrNull( idx );

                  var tDefIndex = -1;
                  CILMetaData otherMD;
                  if ( tRef == null )
                  {
                     otherMD = null;
                  }
                  else
                  {
                     otherMD = md;
                     if ( tRef.ResolutionScope.HasValue )
                     {
                        var resScope = tRef.ResolutionScope.Value;
                        var resIdx = resScope.Index;
                        MDSpecificCache otherCache;
                        switch ( resScope.Table )
                        {
                           case Tables.TypeRef:
                              // Nested type
                              this.ResolveTypeFromTypeRef( resIdx, out otherMD, out tDefIndex );
                              otherCache = this.GetOtherCache( otherMD );
                              if ( otherCache != null )
                              {
                                 tDefIndex = otherCache.FindNestedTypeIndex( tDefIndex, tRef.Name );
                              }

                              break;
                           case Tables.ModuleRef:
                              // Same assembly, different module
                              throw new NotImplementedException( "Module reference in type reference row." );
                           case Tables.Module:
                              // Same as type-def
                              tDefIndex = resIdx;
                              break;
                           case Tables.AssemblyRef:
                              // Resolve assembly ref -> CILMetaData
                              // Then recursion with different metadata
                              var aRef = md.AssemblyReferences.GetOrNull( resIdx );
                              otherMD = aRef == null ?
                                 null :
                                 this._owner.ResolveAssemblyByAssemblyInformation( md, aRef.AssemblyInformation, aRef.Attributes.IsFullPublicKey() );
                              otherCache = this.GetOtherCache( otherMD );
                              if ( otherCache != null )
                              {
                                 tDefIndex = otherCache.ResolveTopLevelType( tRef.Name, tRef.Namespace );
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

            private MDSpecificCache GetOtherCache( CILMetaData otherMD )
            {
               return otherMD == null ?
                  null :
                  (
                     Object.ReferenceEquals( this, otherMD ) ?
                     this :
                     null
                  );
            }
         }

         private readonly ModuleLoadingArguments _loadingArgs;

         private readonly IDictionary<AssemblyInformationForResolving, CILMetaData> _mdResolveCacheByAssemblyInfo;
         private readonly IDictionary<String, CILMetaData> _mdResolveCacheByName;


         private readonly IDictionary<CILMetaData, MDSpecificCache> _mdCaches;
         private readonly Func<CILMetaData, MDSpecificCache> _mdCacheFactory;

         internal CATypeResolveCache( ModuleLoadingArguments loadingArgs )
         {
            ArgumentValidator.ValidateNotNull( "Loading arguments", loadingArgs );


            this._loadingArgs = loadingArgs;
            this._mdResolveCacheByAssemblyInfo = new Dictionary<AssemblyInformationForResolving, CILMetaData>();
            this._mdResolveCacheByName = new Dictionary<String, CILMetaData>();
            this._mdCaches = new Dictionary<CILMetaData, MDSpecificCache>();
            this._mdCacheFactory = this.MDSpecificCacheFactory;
         }

         internal CILMetaData ResolveAssemblyByAssemblyInformation( CILMetaData md, AssemblyInformation information, Boolean isFullPublicKey )
         {
            return this._mdResolveCacheByAssemblyInfo.GetOrAdd_NotThreadSafe(
               new AssemblyInformationForResolving( information, isFullPublicKey ),
               ai =>
               {
                  // TODO put ca-sigs list behind lazy. then we can use md.AssemblyDefinition table here to check if assembly name is in fact this assembly!
                  return this._loadingArgs.ResolveAssemblyReference( null, ai );
               } );
         }

         internal CILMetaData ResolveAssemblyByString( CILMetaData md, String assemblyString )
         {
            if ( assemblyString == null )
            {
               assemblyString = String.Empty;
            }
            AssemblyInformation aInfo;
            Boolean isFullPublicKey;
            return AssemblyInformation.TryParse( assemblyString, out aInfo, out isFullPublicKey ) ?
               ResolveAssemblyByAssemblyInformation( md, aInfo, isFullPublicKey ) :
               this._mdResolveCacheByName.GetOrAdd_NotThreadSafe( assemblyString, assString => this._loadingArgs.ResolveAssemblyReference( assString, null ) );
         }

         internal CustomAttributeArgumentType ResolveTypeFromFullName( CILMetaData md, String typeString )
         {
            // 1. See if there is assembly name present
            // 2. If present, then resolve assembly by name
            // 3. If not present, then try this assembly and then 'mscorlib'
            // 4. Resolve table index by string
            // 5. Resolve return value by CILMetaData + TableIndex pair

            String typeName, assemblyName;
            var assemblyNamePresent = typeString.ParseFullTypeString( out typeName, out assemblyName );
            CILMetaData targetModule;
            if ( assemblyNamePresent )
            {
               // Other assembly
               targetModule = this.ResolveAssemblyByString( md, assemblyName );
            }
            else
            {
               // This assembly or mscorlib
               targetModule = md;
            }

            var retVal = this.UseMDCache( targetModule, c => c.ResolveTypeFromTypeName( typeName ) );
            if ( retVal == null && !assemblyNamePresent )
            {
               // TODO try 'mscorlib' unless this is mscorlib
            }

            return retVal;
         }


         internal CustomAttributeArgumentType ResolveTypeFromTypeDef( CILMetaData md, Int32 index )
         {
            return this.UseMDCache( md, c => c.ResolveTypeFromTypeDef( index ) );
         }

         internal CustomAttributeArgumentType ResolveTypeFromTypeRef( CILMetaData md, Int32 index )
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
      }
   }
}
