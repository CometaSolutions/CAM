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

using CommonUtils;
using CILAssemblyManipulator.Physical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CILAssemblyManipulator.Physical.IO;

namespace CILAssemblyManipulator.Physical.IO.Defaults
{
   public static class SignatureSerialization
   {

      private const Byte DECL_SECURITY_HEADER = 0x2E; // '.'

      #region Deserialization

      public static AbstractNotRawSignature ReadNonTypeSignature(
         this SignatureProvider sigProvider,
         Byte[] array,
         ref Int32 idx,
         Int32 max,
         Boolean methodSigIsDefinition,
         Boolean fieldSigIsLocalsSig,
         out Boolean fieldSigTransformedToLocalsSig
         )
      {
         AbstractNotRawSignature retVal;
         fieldSigTransformedToLocalsSig = false;
         var starter = array.ReadSigStarter( ref idx );

         switch ( starter )
         {
            case SignatureStarters.Field:
               if ( fieldSigIsLocalsSig )
               {
                  var locals = new LocalVariablesSignature( 1 );
                  locals.Locals.Add( sigProvider.ReadLocalVariableSignature( array, ref idx ) );
                  retVal = locals;
                  fieldSigTransformedToLocalsSig = true;
               }
               else
               {
                  retVal = sigProvider.ReadFieldSignatureAfterStarter( array, ref idx );
               }
               break;
            case SignatureStarters.LocalSignature:
               retVal = sigProvider.ReadLocalVariablesSignatureAfterStarter( array, ref idx );
               break;
            case SignatureStarters.Property:
            case SignatureStarters.Property | SignatureStarters.HasThis:
               retVal = sigProvider.ReadPropertySignatureAfterStarter( array, ref idx, starter );
               break;
            case SignatureStarters.MethodSpecGenericInst:
               retVal = sigProvider.ReadGenericMethodSignatureAfterStarter( array, ref idx );
               break;
            default:
               --idx;
               retVal = methodSigIsDefinition ?
                  (AbstractNotRawSignature) sigProvider.ReadMethodDefSignature( array, ref idx ) :
                  sigProvider.ReadMethodRefSignature( array, ref idx );
               break;
         }

         Byte[] extraData;
         if ( retVal != null && array.TryReadExtraData( idx, max, out extraData ) )
         {
            retVal.ExtraData = extraData;
         }

         return retVal;
      }
      public static MethodDefinitionSignature ReadMethodDefSignature(
         this SignatureProvider sigProvider,
         Byte[] array,
         ref Int32 idx
         )
      {
         SignatureStarters elementType;
         Int32 genericCount;
         ParameterSignature returnParameter;
         ParameterSignature[] parameters;
         Int32 sentinelMark;
         MethodDefinitionSignature retVal;
         if ( sigProvider.ReadAbstractMethodSig( array, ref idx, false, out elementType, out genericCount, out returnParameter, out parameters, out sentinelMark ) )
         {
            retVal = new MethodDefinitionSignature( parameters.Length )
            {
               GenericArgumentCount = genericCount,
               ReturnType = returnParameter,
               MethodSignatureInformation = (MethodSignatureInformation) elementType,
            };
            retVal.Parameters.AddRange( parameters );
         }
         else
         {
            retVal = null;
         }
         return retVal;
      }

      public static MethodReferenceSignature ReadMethodRefSignature(
         this SignatureProvider sigProvider,
         Byte[] array,
         ref Int32 idx
         )
      {

         SignatureStarters elementType;
         Int32 genericCount;
         ParameterSignature returnParameter;
         ParameterSignature[] parameters;
         Int32 sentinelMark;
         MethodReferenceSignature retVal;
         if ( sigProvider.ReadAbstractMethodSig( array, ref idx, true, out elementType, out genericCount, out returnParameter, out parameters, out sentinelMark ) )
         {
            var pLength = sentinelMark == -1 ? parameters.Length : sentinelMark;
            var vLength = sentinelMark == -1 ? 0 : ( parameters.Length - sentinelMark );
            retVal = new MethodReferenceSignature( pLength, vLength )
            {
               GenericArgumentCount = genericCount,
               ReturnType = returnParameter,
               MethodSignatureInformation = (MethodSignatureInformation) elementType,
            };
            retVal.Parameters.AddRange( vLength > 0 ? parameters.Take( pLength ) : parameters );
            if ( vLength > 0 )
            {
               retVal.VarArgsParameters.AddRange( parameters.Skip( pLength ) );
            }
         }
         else
         {
            retVal = null;
         }
         return retVal;
      }

      private static Boolean ReadAbstractMethodSig(
         this SignatureProvider sigProvider,
         Byte[] array,
         ref Int32 idx,
         Boolean canHaveSentinel,
         out SignatureStarters elementType,
         out Int32 genericCount,
         out ParameterSignature returnParameter,
         out ParameterSignature[] parameters,
         out Int32 sentinelMark
         )
      {
         elementType = array.ReadSigStarter( ref idx );

         genericCount = elementType.IsGeneric() ? array.DecompressUInt32( ref idx ) : 0;

         var amountOfParams = array.DecompressUInt32( ref idx );

         var retVal = amountOfParams <= UInt16.MaxValue;
         if ( retVal )
         {
            returnParameter = sigProvider.ReadParameter( array, ref idx );
            sentinelMark = -1;
            if ( amountOfParams > 0 )
            {
               parameters = new ParameterSignature[amountOfParams];
               Int32 i;

               if ( canHaveSentinel )
               {
                  for ( i = 0; i < amountOfParams; ++i )
                  {
                     if ( array[idx] == (Byte) SignatureElementTypes.Sentinel )
                     {
                        sentinelMark = i;
                     }
                     parameters[i] = sigProvider.ReadParameter( array, ref idx );
                  }
               }
               else
               {
                  for ( i = 0; i < amountOfParams; ++i )
                  {
                     parameters[i] = sigProvider.ReadParameter( array, ref idx );
                  }
               }
            }
            else
            {
               parameters = Empty<ParameterSignature>.Array;
            }
         }
         else
         {
            elementType = default( SignatureStarters );
            genericCount = -1;
            returnParameter = null;
            parameters = null;
            sentinelMark = -1;
         }

         return retVal;
      }

      internal static ParameterSignature ReadParameter(
         this SignatureProvider sigProvider,
         Byte[] array,
         ref Int32 idx
         )
      {
         var retVal = new ParameterSignature();
         AddFromBytes( array, ref idx, retVal.CustomModifiers );
         var elementType = array.ReadSigElementType( ref idx );
         if ( elementType == SignatureElementTypes.TypedByRef )
         {
            retVal.Type = sigProvider.GetSimpleTypeSignatureOrNull( (SimpleTypeSignatureKind) elementType );
         }
         else
         {
            if ( elementType == SignatureElementTypes.ByRef )
            {
               retVal.IsByRef = true;
            }
            else
            {
               // Go backwards
               --idx;
            }
            retVal.Type = sigProvider.ReadTypeSignature( array, ref idx );
         }
         return retVal;
      }

      public static FieldSignature ReadFieldSignature(
         this SignatureProvider sigProvider,
         Byte[] array,
         ref Int32 idx
         )
      {
         SignatureStarters starter;
         return ( starter = array.ReadSigStarter( ref idx ) ).IsField() ?
            sigProvider.ReadFieldSignatureAfterStarter( array, ref idx ) :
            null;
      }

      private static FieldSignature ReadFieldSignatureAfterStarter(
         this SignatureProvider sigProvider,
         Byte[] array,
         ref Int32 idx
         )
      {
         var retVal = new FieldSignature();
         AddFromBytes( array, ref idx, retVal.CustomModifiers );
         retVal.Type = sigProvider.ReadTypeSignature( array, ref idx );
         return retVal;
      }

      public static PropertySignature ReadPropertySignature(
         this SignatureProvider sigProvider,
         Byte[] array,
         ref Int32 idx
         )
      {
         SignatureStarters starter;
         return ( starter = array.ReadSigStarter( ref idx ) ).IsProperty() ?
            sigProvider.ReadPropertySignatureAfterStarter( array, ref idx, starter ) :
            null;
      }

      private static PropertySignature ReadPropertySignatureAfterStarter(
         this SignatureProvider sigProvider,
         Byte[] array,
         ref Int32 idx,
         SignatureStarters starter
         )
      {
         PropertySignature retVal;
         var paramCount = array.DecompressUInt32( ref idx );
         if ( paramCount <= UInt16.MaxValue )
         {
            retVal = new PropertySignature( parameterCount: paramCount )
            {
               HasThis = starter.IsHasThis()
            };
            AddFromBytes( array, ref idx, retVal.CustomModifiers );
            retVal.PropertyType = sigProvider.ReadTypeSignature( array, ref idx );

            for ( var i = 0; i < paramCount; ++i )
            {
               retVal.Parameters.Add( sigProvider.ReadParameter( array, ref idx ) );
            }
         }
         else
         {
            retVal = null;
         }
         return retVal;
      }

      public static LocalVariablesSignature ReadLocalVariablesSignature(
         this SignatureProvider sigProvider,
         Byte[] array,
         ref Int32 idx
         )
      {
         SignatureStarters starter;
         return ( starter = array.ReadSigStarter( ref idx ) ).IsLocalSignature() ?
            sigProvider.ReadLocalVariablesSignatureAfterStarter( array, ref idx ) :
            null;
      }

      internal static LocalVariablesSignature ReadLocalVariablesSignatureAfterStarter(
         this SignatureProvider sigProvider,
         Byte[] array,
         ref Int32 idx
         )
      {
         var localsCount = array.DecompressUInt32( ref idx );
         var retVal = new LocalVariablesSignature( localsCount );
         for ( var i = 0; i < localsCount; ++i )
         {
            retVal.Locals.Add( sigProvider.ReadLocalVariableSignature( array, ref idx ) );
         }
         return retVal;
      }

      public static LocalSignature ReadLocalVariableSignature(
         this SignatureProvider sigProvider,
         Byte[] array,
         ref Int32 idx
         )
      {
         var retVal = new LocalSignature();
         if ( array[idx] == (Byte) SignatureElementTypes.TypedByRef )
         {
            retVal.Type = sigProvider.GetSimpleTypeSignatureOrNull( SimpleTypeSignatureKind.TypedByRef );
         }
         else
         {
            AddFromBytes( array, ref idx, retVal.CustomModifiers );
            if ( array[idx] == (Byte) SignatureElementTypes.Pinned )
            {
               retVal.IsPinned = true;
               ++idx;
            }

            if ( array[idx] == (Byte) SignatureElementTypes.ByRef )
            {
               retVal.IsByRef = true;
               ++idx;
            }

            retVal.Type = sigProvider.ReadTypeSignature( array, ref idx );
         }

         return retVal;
      }

      public static CustomModifierSignature ReadFromBytes(
         Byte[] array,
         ref Int32 idx
         )
      {
         var sigType = (SignatureElementTypes) array[idx];
         CustomModifierSignature retVal;
         if ( sigType == SignatureElementTypes.CModOpt || sigType == SignatureElementTypes.CModReqd )
         {
            ++idx;
            retVal =
               new CustomModifierSignature()
               {
                  CustomModifierType = TableIndex.FromOneBasedToken( array.DecodeTypeDefOrRefOrSpec( ref idx ) ),
                  Optionality = (CustomModifierSignatureOptionality) sigType
               };
         }
         else
         {
            retVal = null;
         }

         return retVal;
      }

      public static void AddFromBytes(
         Byte[] array,
         ref Int32 idx,
         IList<CustomModifierSignature> customMods
         )
      {
         CustomModifierSignature curMod;
         while ( ( curMod = ReadFromBytes( array, ref idx ) ) != null )
         {
            customMods.Add( curMod );
         }
      }

      public static TypeSignature ReadTypeSignature(
         this SignatureProvider sigProvider,
         Byte[] array,
         ref Int32 idx,
         Int32 max = -1
         )
      {
         Int32 auxiliary;
         var elementType = array.ReadSigElementType( ref idx );
         TypeSignature retVal;
         switch ( elementType )
         {
            case SignatureElementTypes.Boolean:
            case SignatureElementTypes.Char:
            case SignatureElementTypes.I1:
            case SignatureElementTypes.U1:
            case SignatureElementTypes.I2:
            case SignatureElementTypes.U2:
            case SignatureElementTypes.I4:
            case SignatureElementTypes.U4:
            case SignatureElementTypes.I8:
            case SignatureElementTypes.U8:
            case SignatureElementTypes.R4:
            case SignatureElementTypes.R8:
            case SignatureElementTypes.I:
            case SignatureElementTypes.U:
            case SignatureElementTypes.String:
            case SignatureElementTypes.Object:
            case SignatureElementTypes.Void:
               retVal = sigProvider.GetSimpleTypeSignatureOrNull( (SimpleTypeSignatureKind) elementType );
               break;
            case SignatureElementTypes.Array:
               var arrayType = sigProvider.ReadTypeSignature( array, ref idx );
               var arraySig = ReadArrayInfo( array, ref idx );
               arraySig.ArrayType = arrayType;
               retVal = arraySig;
               break;
            case SignatureElementTypes.Class:
            case SignatureElementTypes.ValueType:
            case SignatureElementTypes.GenericInst:
               var isGeneric = elementType == SignatureElementTypes.GenericInst;
               if ( isGeneric )
               {
                  elementType = array.ReadSigElementType( ref idx );
               }

               var actualType = TableIndex.FromOneBasedToken( array.DecodeTypeDefOrRefOrSpec( ref idx ) );
               auxiliary = isGeneric ? array.DecompressUInt32( ref idx ) : 0;
               if ( auxiliary <= UInt16.MaxValue )
               {
                  var classOrValue = new ClassOrValueTypeSignature()
                  {
                     IsClass = elementType == SignatureElementTypes.Class,
                     Type = actualType
                  };
                  if ( isGeneric )
                  {
                     for ( var i = 0; i < auxiliary; ++i )
                     {
                        var curGArg = sigProvider.ReadTypeSignature( array, ref idx );
                        classOrValue.GenericArguments.Add( curGArg );
                     }
                  }
                  retVal = classOrValue;
               }
               else
               {
                  retVal = null;
               }

               break;
            case SignatureElementTypes.FnPtr:
               retVal = new FunctionPointerTypeSignature()
               {
                  MethodSignature = sigProvider.ReadMethodRefSignature( array, ref idx )
               };
               break;
            case SignatureElementTypes.MVar:
            case SignatureElementTypes.Var:
               retVal = new GenericParameterTypeSignature()
               {
                  GenericParameterIndex = array.DecompressUInt32( ref idx ),
                  IsTypeParameter = elementType == SignatureElementTypes.Var
               };
               break;
            case SignatureElementTypes.Ptr:
               var ptr = new PointerTypeSignature();
               AddFromBytes( array, ref idx, ptr.CustomModifiers );
               ptr.PointerType = sigProvider.ReadTypeSignature( array, ref idx );
               retVal = ptr;
               break;
            case SignatureElementTypes.SzArray:
               var szArr = new SimpleArrayTypeSignature();
               AddFromBytes( array, ref idx, szArr.CustomModifiers );
               szArr.ArrayType = sigProvider.ReadTypeSignature( array, ref idx );
               retVal = szArr;
               break;
            default:
               retVal = null;
               break;
         }

         Byte[] extraData;
         if ( max >= 0 && retVal != null && array.TryReadExtraData( idx, max, out extraData ) )
         {
            retVal.ExtraData = extraData;
         }

         return retVal;
      }

      private static ComplexArrayTypeSignature ReadArrayInfo(
         Byte[] array,
         ref Int32 idx
         )
      {
         var rank = array.DecompressUInt32( ref idx );
         var curSize = array.DecompressUInt32( ref idx );
         var retVal = new ComplexArrayTypeSignature( sizesCount: curSize ) // TODO skip thru sizes and detect lower bound count
         {
            Rank = rank
         };
         while ( curSize > 0 )
         {
            retVal.Sizes.Add( array.DecompressUInt32( ref idx ) );
            --curSize;
         }
         curSize = array.DecompressUInt32( ref idx );

         while ( curSize > 0 )
         {
            retVal.LowerBounds.Add( array.DecompressInt32( ref idx ) );
            --curSize;
         }
         return retVal;
      }

      public static GenericMethodSignature ReadGenericMethodSignature(
         this SignatureProvider sigProvider,
         Byte[] array,
         ref Int32 idx
         )
      {
         SignatureStarters starter;
         return ( starter = array.ReadSigStarter( ref idx ) ) == SignatureStarters.MethodSpecGenericInst ?
            sigProvider.ReadGenericMethodSignatureAfterStarter( array, ref idx ) :
            null;
      }

      public static GenericMethodSignature ReadGenericMethodSignatureAfterStarter(
         this SignatureProvider sigProvider,
         Byte[] array,
         ref Int32 idx
         )
      {
         var gArgsCount = array.DecompressUInt32( ref idx );
         var retVal = new GenericMethodSignature( gArgsCount );
         for ( var i = 0; i < gArgsCount; ++i )
         {
            retVal.GenericArguments.Add( sigProvider.ReadTypeSignature( array, ref idx ) );
         }
         return retVal;
      }

      public static AbstractMarshalingInfo ReadMarshalingInfo(
         Byte[] array,
         ref Int32 idx,
         Int32 max
         )
      {
         const Int32 NO_INDEX = -1;
         AbstractMarshalingInfo result = null;
         var ut = array.ReadUnmanagedType( ref idx );

         switch ( ut )
         {
            case UnmanagedType.ByValTStr:
               result = new FixedLengthStringMarshalingInfo()
               {
                  Value = ut,
                  Size = array.DecompressUInt32OrDefault( ref idx, max, NO_INDEX )
               };
               break;
            case UnmanagedType.IUnknown:
            case UnmanagedType.IDispatch:
            case UnmanagedType.Interface:
               result = new InterfaceMarshalingInfo()
               {
                  Value = ut,
                  IIDParameterIndex = array.DecompressUInt32OrDefault( ref idx, max, NO_INDEX )
               };
               break;
            case UnmanagedType.SafeArray:
               result = new SafeArrayMarshalingInfo()
               {
                  Value = ut,
                  ElementType = (VarEnum) array.DecompressUInt32OrDefault( ref idx, max, NO_INDEX ),
                  UserDefinedType = array.ReadLenPrefixedUTF8StringOrDefault( ref idx, max ).UnescapeCILTypeString()
               };
               break;
            case UnmanagedType.ByValArray:
               result = new FixedLengthArrayMarshalingInfo()
               {
                  Value = ut,
                  Size = array.DecompressUInt32OrDefault( ref idx, max, NO_INDEX ),
                  ElementType = (UnmanagedType) array.DecompressUInt32OrDefault( ref idx, max, NO_INDEX )
               };
               break;
            case UnmanagedType.LPArray:
               result = new ArrayMarshalingInfo()
               {
                  Value = ut,
                  ElementType = (UnmanagedType) array.DecompressUInt32OrDefault( ref idx, max, NO_INDEX ),
                  SizeParameterIndex = array.DecompressUInt32OrDefault( ref idx, max, NO_INDEX ),
                  Size = array.DecompressUInt32OrDefault( ref idx, max, NO_INDEX ),
                  Flags = array.DecompressUInt32OrDefault( ref idx, max, NO_INDEX )
               };
               break;
            case UnmanagedType.CustomMarshaler:
               result = new CustomMarshalingInfo()
               {
                  Value = ut,
                  GUIDString = array.ReadLenPrefixedUTF8StringOrDefault( ref idx, max ),
                  NativeTypeName = array.ReadLenPrefixedUTF8StringOrDefault( ref idx, max ),
                  CustomMarshalerTypeName = array.ReadLenPrefixedUTF8StringOrDefault( ref idx, max ).UnescapeCILTypeString(),
                  MarshalCookie = array.ReadLenPrefixedUTF8StringOrDefault( ref idx, max )
               };
               break;
            default:
               result = idx < max ?
                  (AbstractMarshalingInfo) new RawMarshalingInfo()
                  {
                     Value = ut,
                     Bytes = array.CreateArrayCopy( idx, max - idx )
                  } :
                  new SimpleMarshalingInfo()
                  {
                     Value = ut
                  };
               break;
         }

         return result;
      }

      public static void ReadSecurityInformation(
         Byte[] array,
         ref Int32 idx,
         Int32 max,
         List<AbstractSecurityInformation> secInfos
         )
      {
         const String PERMISSION_SET = "System.Security.Permissions.PermissionSetAttribute";
         const String PERMISSION_SET_XML_PROP = "XML";

         var b = array[idx++];
         if ( b == DECL_SECURITY_HEADER )
         {
            // New (.NET 2.0+) security spec
            // Amount of security attributes
            var attrCount = array.DecompressUInt32( ref idx );

            for ( var j = 0; j < attrCount; ++j )
            {
               var secType = array.ReadLenPrefixedUTF8StringOrDefault( ref idx, max );
               // There is an amount of remaining bytes here
               var attributeByteCount = array.DecompressUInt32( ref idx );
               var copyStart = idx;
               // Now, amount of named args
               var argCount = array.DecompressUInt32( ref idx );
               AbstractSecurityInformation secInfo;
               var bytesToCopy = attributeByteCount - ( idx - copyStart );
               secInfo = new RawSecurityInformation()
               {
                  SecurityAttributeType = secType,
                  ArgumentCount = argCount,
                  Bytes = array.CreateArrayCopy( ref idx, bytesToCopy )
               };
               //}
               //else
               //{
               //   secInfo = new SecurityInformation()
               //   {
               //      SecurityAttributeType = secType
               //   };
               //   idx += attributeByteCount - 1;
               //}

               secInfos.Add( secInfo );

            }

         }
         else
         {
            // Old (.NET 1.x) security spec
            // Create a single SecurityInformation with PermissionSetAttribute type and XML property argument containing the XML of the blob
            var secInfo = new SecurityInformation( 1 )
            {
               SecurityAttributeType = PERMISSION_SET
            };
            secInfo.NamedArguments.Add( new CustomAttributeNamedArgument()
            {
               IsField = false,
               Name = PERMISSION_SET_XML_PROP,
               Value = new CustomAttributeTypedArgument()
               {
                  Value = IO.Defaults.MetaDataConstants.USER_STRING_ENCODING.GetString( array, idx, max - idx )
               }
            } );
            secInfos.Add( secInfo );
         }


      }

      #endregion

      #region Serialization
      internal static Byte[] CreateAnySignature( this ResizableArray<Byte> info, AbstractSignature sig )
      {
         if ( sig == null )
         {
            return null;
         }
         else
         {
            switch ( sig.SignatureKind )
            {
               case SignatureKind.Field:
                  return info.CreateFieldSignature( sig as FieldSignature );
               case SignatureKind.GenericMethodInstantiation:
                  return info.CreateMethodSpecSignature( sig as GenericMethodSignature );
               case SignatureKind.LocalVariables:
                  return info.CreateLocalsSignature( sig as LocalVariablesSignature );
               case SignatureKind.MethodDefinition:
               case SignatureKind.MethodReference:
                  return info.CreateMethodSignature( sig as AbstractMethodSignature );
               case SignatureKind.Property:
                  return info.CreatePropertySignature( sig as PropertySignature );
               case SignatureKind.Raw:
                  return ( (RawSignature) sig ).Bytes.CreateArrayCopy();
               case SignatureKind.Type:
                  return info.CreateTypeSignature( sig as TypeSignature );
               default:
                  return null;
            }
         }
      }

      internal static Byte[] CreateFieldSignature( this ResizableArray<Byte> info, FieldSignature sig )
      {
         var idx = 0;
         info.WriteFieldSignature( ref idx, sig );
         return info.Array.CreateArrayCopy( idx );
      }

      internal static Byte[] CreateMethodSignature( this ResizableArray<Byte> info, AbstractMethodSignature sig )
      {
         var idx = 0;
         info.WriteMethodSignature( ref idx, sig );
         return info.Array.CreateArrayCopy( idx );
      }

      internal static Byte[] CreateConstantBytes( this ResizableArray<Byte> info, Object constant, ConstantValueType elementType )
      {
         var idx = 0;
         return info.WriteConstantValue( ref idx, constant, elementType ) ?
            info.Array.CreateArrayCopy( idx ) :
            null;
      }

      internal static Byte[] CreateLocalsSignature( this ResizableArray<Byte> info, LocalVariablesSignature sig )
      {
         var idx = 0;
         info.WriteLocalsSignature( ref idx, sig );
         return info.Array.CreateArrayCopy( idx );
      }

      internal static Byte[] CreateCustomAttributeSignature( this ResizableArray<Byte> info, CILMetaData md, Int32 caIdx )
      {
         var sig = md.CustomAttributeDefinitions.TableContents[caIdx].Signature;
         Byte[] retVal;
         if ( sig != null ) // sig.TypedArguments.Count > 0 || sig.NamedArguments.Count > 0 )
         {
            var sigg = sig as CustomAttributeSignature;
            if ( sigg != null )
            {
               var idx = 0;
               info.WriteCustomAttributeSignature( ref idx, md, caIdx );
               retVal = info.Array.CreateArrayCopy( idx );
            }
            else
            {
               retVal = ( (RawCustomAttributeSignature) sig ).Bytes;
            }
         }
         else
         {
            // Signature missing
            retVal = null;
         }
         return retVal;
      }

      internal static Byte[] CreateMarshalSpec( this ResizableArray<Byte> info, AbstractMarshalingInfo marshal )
      {
         var idx = 0;
         return info.WriteMarshalInfo( ref idx, marshal ) ?
            info.Array.CreateArrayCopy( idx ) :
            null;
      }

      internal static Byte[] CreateSecuritySignature( this ResizableArray<Byte> info, List<AbstractSecurityInformation> permissions, ResizableArray<Byte> aux )
      {
         var idx = 0;
         info.WriteSecuritySignature( ref idx, permissions, aux );
         return info.Array.CreateArrayCopy( idx );
      }

      internal static Byte[] CreateStandaloneSignature( this ResizableArray<Byte> info, StandaloneSignature standaloneSig )
      {
         var sig = standaloneSig.Signature;
         var locals = sig as LocalVariablesSignature;
         var idx = 0;
         if ( locals != null )
         {
            if ( standaloneSig.StoreSignatureAsFieldSignature && locals.Locals.Count > 0 )
            {
               info
                  .AddSigStarterByte( ref idx, SignatureStarters.Field )
                  .WriteLocalSignature( ref idx, locals.Locals[0] );
            }
            else
            {
               info.WriteLocalsSignature( ref idx, locals );
            }
         }
         else
         {
            var raw = sig as RawSignature;
            if ( raw != null )
            {
               info.WriteArray( ref idx, raw.Bytes );
            }
            else
            {
               info.WriteMethodSignature( ref idx, sig as AbstractMethodSignature );
            }
         }

         return idx == 0 ? null : info.Array.CreateArrayCopy( idx );
      }

      internal static Byte[] CreatePropertySignature( this ResizableArray<Byte> info, PropertySignature sig )
      {
         var idx = 0;
         info.WritePropertySignature( ref idx, sig );
         return info.Array.CreateArrayCopy( idx );
      }

      internal static Byte[] CreateTypeSignature( this ResizableArray<Byte> info, TypeSignature sig )
      {
         var idx = 0;
         info.WriteTypeSignature( ref idx, sig );
         return info.Array.CreateArrayCopy( idx );
      }

      internal static Byte[] CreateMethodSpecSignature( this ResizableArray<Byte> info, GenericMethodSignature sig )
      {
         var idx = 0;
         info.WriteMethodSpecSignature( ref idx, sig );
         return info.Array.CreateArrayCopy( idx );
      }

      private static void WriteFieldSignature( this ResizableArray<Byte> info, ref Int32 idx, FieldSignature sig )
      {
         if ( sig != null )
         {
            info
               .AddSigStarterByte( ref idx, SignatureStarters.Field )
               .WriteCustomModifiers( ref idx, sig.CustomModifiers )
               .WriteTypeSignature( ref idx, sig.Type );
         }
      }

      private static ResizableArray<Byte> WriteCustomModifiers( this ResizableArray<Byte> info, ref Int32 idx, IList<CustomModifierSignature> mods )
      {
         if ( mods.Count > 0 )
         {
            foreach ( var mod in mods )
            {
               info
                  .AddSigByte( ref idx, (SignatureElementTypes) mod.Optionality )
                  .AddTDRSToken( ref idx, mod.CustomModifierType );
            }
         }
         return info;
      }

      private static ResizableArray<Byte> WriteTypeSignature( this ResizableArray<Byte> info, ref Int32 idx, TypeSignature type )
      {
         switch ( type.TypeSignatureKind )
         {
            case TypeSignatureKind.Simple:
               info.AddSigByte( ref idx, (SignatureElementTypes) ( (SimpleTypeSignature) type ).SimpleType );
               break;
            case TypeSignatureKind.SimpleArray:
               var szArray = (SimpleArrayTypeSignature) type;
               info
                  .AddSigByte( ref idx, SignatureElementTypes.SzArray )
                  .WriteCustomModifiers( ref idx, szArray.CustomModifiers )
                  .WriteTypeSignature( ref idx, szArray.ArrayType );
               break;
            case TypeSignatureKind.ComplexArray:
               var array = (ComplexArrayTypeSignature) type;
               info
                  .AddSigByte( ref idx, SignatureElementTypes.Array )
                  .WriteTypeSignature( ref idx, array.ArrayType )
                  .AddCompressedUInt32( ref idx, array.Rank )
                  .AddCompressedUInt32( ref idx, array.Sizes.Count );
               foreach ( var size in array.Sizes )
               {
                  info.AddCompressedUInt32( ref idx, size );
               }
               info.AddCompressedUInt32( ref idx, array.LowerBounds.Count );
               foreach ( var lobo in array.LowerBounds )
               {
                  info.AddCompressedInt32( ref idx, lobo );
               }
               break;
            case TypeSignatureKind.ClassOrValue:
               var clazz = (ClassOrValueTypeSignature) type;
               var gArgs = clazz.GenericArguments;
               var isGenericType = gArgs.Count > 0;
               if ( isGenericType )
               {
                  info.AddSigByte( ref idx, SignatureElementTypes.GenericInst );
               }
               info
                  .AddSigByte( ref idx, clazz.IsClass ? SignatureElementTypes.Class : SignatureElementTypes.ValueType )
                  .AddTDRSToken( ref idx, clazz.Type );
               if ( isGenericType )
               {
                  info.AddCompressedUInt32( ref idx, gArgs.Count );
                  foreach ( var gArg in gArgs )
                  {
                     info.WriteTypeSignature( ref idx, gArg );
                  }
               }
               break;
            case TypeSignatureKind.GenericParameter:
               var gParam = (GenericParameterTypeSignature) type;
               info
                  .AddSigByte( ref idx, gParam.IsTypeParameter ? SignatureElementTypes.Var : SignatureElementTypes.MVar )
                  .AddCompressedUInt32( ref idx, gParam.GenericParameterIndex );
               break;
            case TypeSignatureKind.FunctionPointer:
               info
                  .AddSigByte( ref idx, SignatureElementTypes.FnPtr )
                  .WriteMethodSignature( ref idx, ( (FunctionPointerTypeSignature) type ).MethodSignature );
               break;
            case TypeSignatureKind.Pointer:
               var ptr = (PointerTypeSignature) type;
               info
                  .AddSigByte( ref idx, SignatureElementTypes.Ptr )
                  .WriteCustomModifiers( ref idx, ptr.CustomModifiers )
                  .WriteTypeSignature( ref idx, ptr.PointerType );
               break;

         }
         return info;
      }

      private static ResizableArray<Byte> WriteMethodSignature( this ResizableArray<Byte> info, ref Int32 idx, AbstractMethodSignature method )
      {
         if ( method != null )
         {
            var starter = method.MethodSignatureInformation;
            info.AddSigStarterByte( ref idx, (SignatureStarters) starter );

            if ( starter.IsGeneric() )
            {
               info.AddCompressedUInt32( ref idx, method.GenericArgumentCount );
            }

            info
               .AddCompressedUInt32( ref idx, method.Parameters.Count )
               .WriteParameterSignature( ref idx, method.ReturnType );

            foreach ( var param in method.Parameters )
            {
               info.WriteParameterSignature( ref idx, param );
            }

            if ( method.SignatureKind == SignatureKind.MethodReference )
            {
               var mRef = (MethodReferenceSignature) method;
               if ( mRef.VarArgsParameters.Count > 0 )
               {
                  info.AddSigByte( ref idx, SignatureElementTypes.Sentinel );
                  foreach ( var v in mRef.VarArgsParameters )
                  {
                     info.WriteParameterSignature( ref idx, v );
                  }
               }
            }
         }
         return info;
      }

      private static Boolean ContinueWritingParameterOrLocalType( this ResizableArray<Byte> info, ref Int32 idx, TypeSignature type )
      {
         var wasTypedByRef = type != null
            && type.TypeSignatureKind == TypeSignatureKind.Simple
            && ( (SimpleTypeSignature) type ).SimpleType == SimpleTypeSignatureKind.TypedByRef;
         if ( wasTypedByRef )
         {
            info.AddSigByte( ref idx, SignatureElementTypes.TypedByRef );
         }
         return !wasTypedByRef;
      }

      private static ResizableArray<Byte> WriteParameterSignature( this ResizableArray<Byte> info, ref Int32 idx, ParameterSignature parameter )
      {
         info
            .WriteCustomModifiers( ref idx, parameter.CustomModifiers );
         var type = parameter.Type;
         if ( info.ContinueWritingParameterOrLocalType( ref idx, type ) )
         {
            if ( parameter.IsByRef )
            {
               info.AddSigByte( ref idx, SignatureElementTypes.ByRef );
            }

            info.WriteTypeSignature( ref idx, type );
         }
         return info;
      }

      private static Boolean WriteConstantValue( this ResizableArray<Byte> info, ref Int32 idx, Object constant, ConstantValueType elementType )
      {
         var retVal = true;
         if ( constant == null )
         {
            retVal = elementType != ConstantValueType.String;
            if ( retVal )
            {
               info.WriteInt32LEToBytes( ref idx, 0 );
            }
         }
         else
         {
            info.WriteConstantValueNotNull( ref idx, constant );
         }

         return retVal;
      }

      private static void WriteConstantValueNotNull( this ResizableArray<Byte> info, ref Int32 idx, Object constant )
      {

         switch ( Type.GetTypeCode( constant.GetType() ) )
         {
            case TypeCode.Boolean:
               info.WriteByteToBytes( ref idx, Convert.ToBoolean( constant ) ? (Byte) 1 : (Byte) 0 );
               break;
            case TypeCode.SByte:
               info.WriteSByteToBytes( ref idx, Convert.ToSByte( constant ) );
               break;
            case TypeCode.Byte:
               info.WriteByteToBytes( ref idx, Convert.ToByte( constant ) );
               break;
            case TypeCode.Char:
               info.WriteUInt16LEToBytes( ref idx, Convert.ToUInt16( Convert.ToChar( constant ) ) );
               break;
            case TypeCode.Int16:
               info.WriteInt16LEToBytes( ref idx, Convert.ToInt16( constant ) );
               break;
            case TypeCode.UInt16:
               info.WriteUInt16LEToBytes( ref idx, Convert.ToUInt16( constant ) );
               break;
            case TypeCode.Int32:
               info.WriteInt32LEToBytes( ref idx, Convert.ToInt32( constant ) );
               break;
            case TypeCode.UInt32:
               info.WriteUInt32LEToBytes( ref idx, Convert.ToUInt32( constant ) );
               break;
            case TypeCode.Int64:
               info.WriteInt64LEToBytes( ref idx, Convert.ToInt64( constant ) );
               break;
            case TypeCode.UInt64:
               info.WriteUInt64LEToBytes( ref idx, Convert.ToUInt64( constant ) );
               break;
            case TypeCode.Single:
               info.WriteSingleLEToBytes( ref idx, Convert.ToSingle( constant ) );
               break;
            case TypeCode.Double:
               info.WriteDoubleLEToBytes( ref idx, Convert.ToDouble( constant ) );
               break;
            case TypeCode.String:
               var str = Convert.ToString( constant );
               var encoding = MetaDataConstants.USER_STRING_ENCODING;
               var size = encoding.GetByteCount( str );
               info.EnsureThatCanAdd( idx, size );
               idx += encoding.GetBytes( str, 0, str.Length, info.Array, idx );
               break;
            default:
               info.WriteInt32LEToBytes( ref idx, 0 );
               break;
         }
      }

      private static void WriteCustomAttributeSignature( this ResizableArray<Byte> info, ref Int32 idx, CILMetaData md, Int32 caIdx )
      {
         var ca = md.CustomAttributeDefinitions.TableContents[caIdx];
         var attrData = ca.Signature as CustomAttributeSignature;

         var ctor = ca.Type;
         var sig = ctor.Table == Tables.MethodDef ?
            md.MethodDefinitions.TableContents[ctor.Index].Signature :
            md.MemberReferences.TableContents[ctor.Index].Signature as AbstractMethodSignature;

         if ( sig == null )
         {
            throw new InvalidOperationException( "Custom attribute constructor signature was null (custom attribute at index " + caIdx + ", ctor: " + ctor + ")." );
         }
         else if ( sig.Parameters.Count != attrData.TypedArguments.Count )
         {
            throw new InvalidOperationException( "Custom attribute constructor has different amount of parameters than supplied custom attribute data (custom attribute at index " + caIdx + ", ctor: " + ctor + ")." );
         }


         // Prolog
         info
            .WriteByteToBytes( ref idx, 1 )
            .WriteByteToBytes( ref idx, 0 );

         // Fixed args
         for ( var i = 0; i < attrData.TypedArguments.Count; ++i )
         {
            var arg = attrData.TypedArguments[i];
            var caType = md.ResolveCACtorType( sig.Parameters[i].Type, tIdx => new CustomAttributeArgumentTypeEnum()
            {
               TypeString = "" // Type string doesn't matter, as values will be serialized directly...
            } );

            if ( caType == null )
            {
               // TODO some kind of warning system instead of throwing
               throw new InvalidOperationException( "Failed to resolve custom attribute type for constructor parameter (custom attribute at index " + caIdx + ", ctor: " + ctor + ", param: " + i + ")." );
            }
            info.WriteCustomAttributeFixedArg( ref idx, caType, arg.Value );
         }

         // Named args
         info.WriteUInt16LEToBytes( ref idx, (UInt16) attrData.NamedArguments.Count );
         foreach ( var arg in attrData.NamedArguments )
         {
            info.WriteCustomAttributeNamedArg( ref idx, arg );
         }
      }

      private static ResizableArray<Byte> WriteCustomAttributeFixedArg( this ResizableArray<Byte> info, ref Int32 idx, CustomAttributeArgumentType argType, Object arg )
      {
         switch ( argType.ArgumentTypeKind )
         {
            case CustomAttributeArgumentTypeKind.Array:
               if ( arg == null )
               {
                  info.WriteInt32LEToBytes( ref idx, unchecked((Int32) 0xFFFFFFFF) );
               }
               else
               {
                  var isDirectArray = arg is Array;
                  Array array;
                  if ( isDirectArray )
                  {
                     array = (Array) arg;
                     argType = ( (CustomAttributeArgumentTypeArray) argType ).ArrayType;
                  }
                  else
                  {
                     var indirectArray = (CustomAttributeValue_Array) arg;
                     array = indirectArray.Array;
                     argType = indirectArray.ArrayElementType;
                  }

                  info.WriteInt32LEToBytes( ref idx, array.Length );
                  foreach ( var elem in array )
                  {
                     info.WriteCustomAttributeFixedArg( ref idx, argType, elem );
                  }
               }
               break;
            case CustomAttributeArgumentTypeKind.Simple:
               switch ( ( (CustomAttributeArgumentTypeSimple) argType ).SimpleType )
               {
                  case CustomAttributeArgumentTypeSimpleKind.Boolean:
                     info.WriteByteToBytes( ref idx, Convert.ToBoolean( arg ) ? (Byte) 1 : (Byte) 0 );
                     break;
                  case CustomAttributeArgumentTypeSimpleKind.I1:
                     info.WriteSByteToBytes( ref idx, Convert.ToSByte( arg ) );
                     break;
                  case CustomAttributeArgumentTypeSimpleKind.U1:
                     info.WriteByteToBytes( ref idx, Convert.ToByte( arg ) );
                     break;
                  case CustomAttributeArgumentTypeSimpleKind.Char:
                     info.WriteUInt16LEToBytes( ref idx, Convert.ToUInt16( Convert.ToChar( arg ) ) );
                     break;
                  case CustomAttributeArgumentTypeSimpleKind.I2:
                     info.WriteInt16LEToBytes( ref idx, Convert.ToInt16( arg ) );
                     break;
                  case CustomAttributeArgumentTypeSimpleKind.U2:
                     info.WriteUInt16LEToBytes( ref idx, Convert.ToUInt16( arg ) );
                     break;
                  case CustomAttributeArgumentTypeSimpleKind.I4:
                     info.WriteInt32LEToBytes( ref idx, Convert.ToInt32( arg ) );
                     break;
                  case CustomAttributeArgumentTypeSimpleKind.U4:
                     info.WriteUInt32LEToBytes( ref idx, Convert.ToUInt32( arg ) );
                     break;
                  case CustomAttributeArgumentTypeSimpleKind.I8:
                     info.WriteInt64LEToBytes( ref idx, Convert.ToInt64( arg ) );
                     break;
                  case CustomAttributeArgumentTypeSimpleKind.U8:
                     info.WriteUInt64LEToBytes( ref idx, Convert.ToUInt64( arg ) );
                     break;
                  case CustomAttributeArgumentTypeSimpleKind.R4:
                     info.WriteSingleLEToBytes( ref idx, Convert.ToSingle( arg ) );
                     break;
                  case CustomAttributeArgumentTypeSimpleKind.R8:
                     info.WriteDoubleLEToBytes( ref idx, Convert.ToDouble( arg ) );
                     break;
                  case CustomAttributeArgumentTypeSimpleKind.String:
                     info.AddCAString( ref idx, arg == null ? null : Convert.ToString( arg ) );
                     break;
                  case CustomAttributeArgumentTypeSimpleKind.Type:
                     String typeStr;
                     if ( arg != null )
                     {
                        if ( arg is CustomAttributeValue_TypeReference )
                        {
                           typeStr = ( (CustomAttributeValue_TypeReference) arg ).TypeString;
                        }
                        else if ( arg is Type )
                        {
                           typeStr = ( (Type) arg ).AssemblyQualifiedName;
                        }
                        else
                        {
                           typeStr = Convert.ToString( arg );
                        }
                     }
                     else
                     {
                        typeStr = null;
                     }
                     info.AddCAString( ref idx, typeStr );
                     break;
                  case CustomAttributeArgumentTypeSimpleKind.Object:
                     if ( arg == null )
                     {
                        // Nulls are serialized as null strings
                        if ( !CustomAttributeArgumentTypeSimple.String.Equals( argType ) )
                        {
                           argType = CustomAttributeArgumentTypeSimple.String;
                        }
                     }
                     else
                     {
                        argType = ResolveBoxedCAType( arg );
                     }
                     info
                        .WriteCustomAttributeFieldOrPropType( ref idx, ref argType, ref arg )
                        .WriteCustomAttributeFixedArg( ref idx, argType, arg );
                     break;
               }
               break;
            case CustomAttributeArgumentTypeKind.TypeString:
               // TODO check for invalid types (bool, char, single, double, string, any other non-primitive)
               var valueToWrite = arg is CustomAttributeValue_EnumReference ? ( (CustomAttributeValue_EnumReference) arg ).EnumValue : arg;
               if ( valueToWrite == null )
               {
                  throw new InvalidOperationException( "Tried to serialize null as enum." );
               }
               info.WriteConstantValueNotNull( ref idx, valueToWrite );
               break;
         }

         return info;
      }

      private static CustomAttributeArgumentType ResolveBoxedCAType( Object arg, Boolean isWithinArray = false )
      {
         var argType = arg.GetType();
         if ( argType.IsEnum )
         {
            return new CustomAttributeArgumentTypeEnum()
            {
               TypeString = argType.AssemblyQualifiedName
            };
         }
         else
         {
            switch ( Type.GetTypeCode( argType ) )
            {
               case TypeCode.Boolean:
                  return CustomAttributeArgumentTypeSimple.Boolean;
               case TypeCode.Char:
                  return CustomAttributeArgumentTypeSimple.Char;
               case TypeCode.SByte:
                  return CustomAttributeArgumentTypeSimple.SByte;
               case TypeCode.Byte:
                  return CustomAttributeArgumentTypeSimple.Byte;
               case TypeCode.Int16:
                  return CustomAttributeArgumentTypeSimple.Int16;
               case TypeCode.UInt16:
                  return CustomAttributeArgumentTypeSimple.UInt16;
               case TypeCode.Int32:
                  return CustomAttributeArgumentTypeSimple.Int32;
               case TypeCode.UInt32:
                  return CustomAttributeArgumentTypeSimple.UInt32;
               case TypeCode.Int64:
                  return CustomAttributeArgumentTypeSimple.Int64;
               case TypeCode.UInt64:
                  return CustomAttributeArgumentTypeSimple.UInt64;
               case TypeCode.Single:
                  return CustomAttributeArgumentTypeSimple.Single;
               case TypeCode.Double:
                  return CustomAttributeArgumentTypeSimple.Double;
               case TypeCode.String:
                  return CustomAttributeArgumentTypeSimple.String;
               case TypeCode.Object:
                  if ( argType.IsArray )
                  {
                     return isWithinArray ?
                        (CustomAttributeArgumentType) CustomAttributeArgumentTypeSimple.Object :
                        new CustomAttributeArgumentTypeArray()
                        {
                           ArrayType = ResolveBoxedCAType( argType.GetElementType(), true )
                        };
                  }
                  else
                  {
                     // Check for enum reference
                     if ( Equals( typeof( CustomAttributeValue_EnumReference ), argType ) )
                     {
                        return new CustomAttributeArgumentTypeEnum()
                        {
                           TypeString = ( (CustomAttributeValue_EnumReference) arg ).EnumType
                        };
                     }
                     // System.Type or System.Object or CustomAttributeTypeReference
                     else if ( Equals( typeof( CustomAttributeValue_TypeReference ), argType ) || Equals( typeof( Type ), argType ) )
                     {
                        return CustomAttributeArgumentTypeSimple.Type;
                     }
                     else if ( isWithinArray && Equals( typeof( Object ), argType ) )
                     {
                        return CustomAttributeArgumentTypeSimple.Object;
                     }
                     else
                     {
                        throw new InvalidOperationException( "Failed to deduce custom attribute type for " + argType + "." );
                     }
                  }
               default:
                  throw new InvalidOperationException( "Failed to deduce custom attribute type for " + argType + "." );
            }
         }
      }

      private static ResizableArray<Byte> WriteCustomAttributeNamedArg( this ResizableArray<Byte> info, ref Int32 idx, CustomAttributeNamedArgument arg )
      {
         var elem = arg.IsField ? SignatureElementTypes.CA_Field : SignatureElementTypes.CA_Property;
         var typedValueValue = arg.Value.Value;
         var caType = arg.FieldOrPropertyType;
         return info
            .AddSigByte( ref idx, elem )
            .WriteCustomAttributeFieldOrPropType( ref idx, ref caType, ref typedValueValue )
            .AddCAString( ref idx, arg.Name )
            .WriteCustomAttributeFixedArg( ref idx, caType, typedValueValue );
      }

      private static ResizableArray<Byte> WriteCustomAttributeFieldOrPropType( this ResizableArray<Byte> info, ref Int32 idx, ref CustomAttributeArgumentType type, ref Object value, Boolean processEnumTypeAndValue = true )
      {
         if ( type == null )
         {
            throw new InvalidOperationException( "Custom attribute signature typed argument type was null." );
         }

         switch ( type.ArgumentTypeKind )
         {
            case CustomAttributeArgumentTypeKind.Array:
               var arrayType = ( (CustomAttributeArgumentTypeArray) type ).ArrayType;
               Object dummy = null;
               info
                  .AddSigByte( ref idx, SignatureElementTypes.SzArray )
                  .WriteCustomAttributeFieldOrPropType( ref idx, ref arrayType, ref dummy, false );
               break;
            case CustomAttributeArgumentTypeKind.Simple:
               var sigStarter = (SignatureElementTypes) ( (CustomAttributeArgumentTypeSimple) type ).SimpleType;
               info.AddSigByte( ref idx, sigStarter );
               break;
            case CustomAttributeArgumentTypeKind.TypeString:
               info
                  .AddSigByte( ref idx, SignatureElementTypes.CA_Enum )
                  .AddCAString( ref idx, ( (CustomAttributeArgumentTypeEnum) type ).TypeString );
               if ( processEnumTypeAndValue )
               {
                  if ( value == null )
                  {
                     throw new InvalidOperationException( "Tried to serialize null as enum." );
                  }
                  else
                  {
                     if ( value is CustomAttributeValue_EnumReference )
                     {
                        value = ( (CustomAttributeValue_EnumReference) value ).EnumValue;
                     }

                     switch ( Type.GetTypeCode( value.GetType() ) )
                     {
                        //case TypeCode.Boolean:
                        //   type = CustomAttributeArgumentTypeSimple.Boolean;
                        //   break;
                        //case TypeCode.Char:
                        //   type = CustomAttributeArgumentTypeSimple.Char;
                        //   break;
                        case TypeCode.SByte:
                           type = CustomAttributeArgumentTypeSimple.SByte;
                           break;
                        case TypeCode.Byte:
                           type = CustomAttributeArgumentTypeSimple.Byte;
                           break;
                        case TypeCode.Int16:
                           type = CustomAttributeArgumentTypeSimple.Int16;
                           break;
                        case TypeCode.UInt16:
                           type = CustomAttributeArgumentTypeSimple.UInt16;
                           break;
                        case TypeCode.Int32:
                           type = CustomAttributeArgumentTypeSimple.Int32;
                           break;
                        case TypeCode.UInt32:
                           type = CustomAttributeArgumentTypeSimple.UInt32;
                           break;
                        case TypeCode.Int64:
                           type = CustomAttributeArgumentTypeSimple.Int64;
                           break;
                        case TypeCode.UInt64:
                           type = CustomAttributeArgumentTypeSimple.UInt64;
                           break;
                        //case TypeCode.Single:
                        //   type = CustomAttributeArgumentTypeSimple.Single;
                        //   break;
                        //case TypeCode.Double:
                        //   type = CustomAttributeArgumentTypeSimple.Double;
                        //   break;
                        //case TypeCode.String:
                        //   type = CustomAttributeArgumentTypeSimple.String;
                        //break;
                        default:
                           throw new NotSupportedException( "The custom attribute type was marked to be enum, but the actual value's type was: " + value.GetType() + "." );
                     }
                  }
               }
               break;
         }

         return info;
      }

      private static Boolean WriteMarshalInfo( this ResizableArray<Byte> info, ref Int32 idx, AbstractMarshalingInfo marshal )
      {
         var retVal = marshal != null;
         if ( retVal )
         {
            info.WriteByteToBytes( ref idx, (Byte) marshal.Value );
            var canWrite = true;
            Int32 tmp; String tmpString;
            switch ( marshal.MarshalingInfoKind )
            {
               case MarshalingInfoKind.Simple:
                  // Nothing else to write
                  break;
               case MarshalingInfoKind.FixedLengthString:
                  if ( IsMarshalSizeValid( ( tmp = ( (FixedLengthStringMarshalingInfo) marshal ).Size ) ) )
                  {
                     info.AddCompressedUInt32( ref idx, tmp );
                  }
                  break;
               case MarshalingInfoKind.FixedLengthArray:
                  var flArray = (FixedLengthArrayMarshalingInfo) marshal;
                  if ( CanWriteNextMarshalElement( IsMarshalSizeValid( ( tmp = flArray.Size ) ), ref canWrite ) )
                  {
                     info.AddCompressedUInt32( ref idx, tmp );
                  }
                  if ( CanWriteNextMarshalElement( IsUnmanagedTypeValid( ( tmp = (Int32) flArray.ElementType ) ), ref canWrite ) )
                  {
                     info.AddCompressedUInt32( ref idx, tmp );
                  }
                  break;
               case MarshalingInfoKind.SafeArray:
                  var sArray = (SafeArrayMarshalingInfo) marshal;
                  if ( CanWriteNextMarshalElement( ( tmp = (Int32) sArray.ElementType ) != 0, ref canWrite ) )
                  {
                     info.AddCompressedUInt32( ref idx, tmp );
                  }
                  if ( CanWriteNextMarshalElement( ( tmpString = sArray.UserDefinedType ) != null, ref canWrite ) )
                  {
                     info.AddCAString( ref idx, tmpString );
                  }
                  break;
               case MarshalingInfoKind.Array:
                  var array = (ArrayMarshalingInfo) marshal;
                  if ( CanWriteNextMarshalElement( IsUnmanagedTypeValid( ( tmp = (Int32) array.ElementType ) ), ref canWrite ) )
                  {
                     info.AddCompressedUInt32( ref idx, tmp );
                  }
                  if ( CanWriteNextMarshalElement( IsMarshalIndexValid( ( tmp = array.SizeParameterIndex ) ), ref canWrite ) )
                  {
                     info.AddCompressedUInt32( ref idx, tmp );
                  }
                  if ( CanWriteNextMarshalElement( IsMarshalSizeValid( ( tmp = array.Size ) ), ref canWrite ) )
                  {
                     info.AddCompressedUInt32( ref idx, tmp );
                  }
                  if ( CanWriteNextMarshalElement( ( tmp = array.Flags ) >= 0, ref canWrite ) )
                  {
                     info.AddCompressedUInt32( ref idx, tmp );
                  }
                  break;
               case MarshalingInfoKind.Interface:
                  if ( IsMarshalIndexValid( ( tmp = ( (InterfaceMarshalingInfo) marshal ).IIDParameterIndex ) ) )
                  {
                     info.AddCompressedUInt32( ref idx, tmp );
                  }
                  break;
               case MarshalingInfoKind.Custom:
                  var custom = (CustomMarshalingInfo) marshal;
                  info
                        .AddCAString( ref idx, custom.GUIDString ?? "" )
                        .AddCAString( ref idx, custom.NativeTypeName ?? "" )
                        .AddCAString( ref idx, custom.CustomMarshalerTypeName ?? "" )
                        .AddCAString( ref idx, custom.MarshalCookie ?? "" );
                  break;
               case MarshalingInfoKind.Raw:
                  info.WriteArray( ref idx, ( (RawMarshalingInfo) marshal ).Bytes );
                  break;
            }
         }

         return retVal;
      }

      private static Boolean CanWriteNextMarshalElement( Boolean condition, ref Boolean previousResult )
      {
         if ( previousResult )
         {
            if ( !condition )
            {
               previousResult = false;
            }
         }
         else
         {
            // TODO some sort of error reporting
         }

         return previousResult;
      }

      private static Boolean IsMarshalSizeValid( Int32 size )
      {
         return size >= 0;
      }

      private static Boolean IsMarshalIndexValid( Int32 idx )
      {
         return idx >= 0;
      }

      private static Boolean IsUnmanagedTypeValid( Int32 ut )
      {
         return ut != (Int32) UnmanagedType.NotPresent;
      }

      private static ResizableArray<Byte> WriteSecuritySignature(
         this ResizableArray<Byte> info,
         ref Int32 idx,
         List<AbstractSecurityInformation> permissions,
         ResizableArray<Byte> aux
         )
      {
         // TODO currently only newer format, .NET 1 format not supported for writing
         info
            .WriteByteToBytes( ref idx, DECL_SECURITY_HEADER )
            .AddCompressedUInt32( ref idx, permissions.Count );
         foreach ( var sec in permissions )
         {
            info.AddCAString( ref idx, sec.SecurityAttributeType );
            var secInfo = sec as SecurityInformation;
            Byte[] secInfoBLOB;
            if ( secInfo != null )
            {
               // Store arguments in separate bytes
               var auxIdx = 0;
               foreach ( var arg in secInfo.NamedArguments )
               {
                  aux.WriteCustomAttributeNamedArg( ref auxIdx, arg );
               }
               // Now write to sec blob
               secInfoBLOB = aux.Array.CreateArrayCopy( auxIdx );
               // The length of named arguments blob
               info
                  .AddCompressedUInt32( ref idx, secInfoBLOB.Length + BitUtils.GetEncodedUIntSize( secInfo.NamedArguments.Count ) )
                  // The amount of named arguments
                  .AddCompressedUInt32( ref idx, secInfo.NamedArguments.Count );
            }
            else
            {
               secInfoBLOB = ( (RawSecurityInformation) sec ).Bytes;
               info.AddCompressedUInt32( ref idx, secInfoBLOB.Length );
            }

            info.WriteArray( ref idx, secInfoBLOB );
         }

         return info;
      }

      private static ResizableArray<Byte> WriteLocalsSignature(
         this ResizableArray<Byte> info,
         ref Int32 idx,
         LocalVariablesSignature sig
         )
      {
         if ( sig != null )
         {
            var locals = sig.Locals;
            info
               .AddSigStarterByte( ref idx, SignatureStarters.LocalSignature )
               .AddCompressedUInt32( ref idx, locals.Count );
            foreach ( var local in locals )
            {
               info.WriteLocalSignature( ref idx, local );
            }
         }
         return info;
      }

      private static ResizableArray<Byte> WriteLocalSignature(
         this ResizableArray<Byte> info,
         ref Int32 idx,
         LocalSignature sig
         )
      {
         var type = sig.Type;
         if ( info.ContinueWritingParameterOrLocalType( ref idx, type ) )
         {
            info.WriteCustomModifiers( ref idx, sig.CustomModifiers );
            if ( sig.IsPinned )
            {
               info.AddSigByte( ref idx, SignatureElementTypes.Pinned );
            }

            if ( sig.IsByRef )
            {
               info.AddSigByte( ref idx, SignatureElementTypes.ByRef );
            }
            info.WriteTypeSignature( ref idx, type );
         }

         return info;
      }

      private static ResizableArray<Byte> WritePropertySignature(
         this ResizableArray<Byte> info,
         ref Int32 idx,
         PropertySignature sig
         )
      {
         if ( sig != null )
         {
            var starter = SignatureStarters.Property;
            if ( sig.HasThis )
            {
               starter |= SignatureStarters.HasThis;
            }
            info
               .AddSigStarterByte( ref idx, starter )
               .AddCompressedUInt32( ref idx, sig.Parameters.Count )
               .WriteCustomModifiers( ref idx, sig.CustomModifiers )
               .WriteTypeSignature( ref idx, sig.PropertyType );

            foreach ( var param in sig.Parameters )
            {
               info.WriteParameterSignature( ref idx, param );
            }
         }
         return info;
      }

      private static ResizableArray<Byte> WriteMethodSpecSignature(
         this ResizableArray<Byte> info,
         ref Int32 idx,
         GenericMethodSignature sig
         )
      {
         info
            .AddSigStarterByte( ref idx, SignatureStarters.MethodSpecGenericInst )
            .AddCompressedUInt32( ref idx, sig.GenericArguments.Count );
         foreach ( var gArg in sig.GenericArguments )
         {
            info.WriteTypeSignature( ref idx, gArg );
         }
         return info;
      }

      private static ResizableArray<Byte> AddSigStarterByte( this ResizableArray<Byte> info, ref Int32 idx, SignatureStarters starter )
      {
         return info.WriteByteToBytes( ref idx, (Byte) starter );
      }

      private static ResizableArray<Byte> AddSigByte( this ResizableArray<Byte> info, ref Int32 idx, SignatureElementTypes sig )
      {
         return info.WriteByteToBytes( ref idx, (Byte) sig );
      }

      private static ResizableArray<Byte> AddTDRSToken( this ResizableArray<Byte> info, ref Int32 idx, TableIndex token )
      {
         return info.AddCompressedUInt32( ref idx, token.GetOneBasedToken().EncodeTypeDefOrRefOrSpec() );
      }

      internal static ResizableArray<Byte> AddCompressedUInt32( this ResizableArray<Byte> info, ref Int32 idx, Int32 value )
      {
         info.EnsureThatCanAdd( idx, (Int32) BitUtils.GetEncodedUIntSize( value ) )
            .Array.CompressUInt32( ref idx, value );
         return info;
      }

      internal static ResizableArray<Byte> AddCompressedInt32( this ResizableArray<Byte> info, ref Int32 idx, Int32 value )
      {
         info.EnsureThatCanAdd( idx, (Int32) BitUtils.GetEncodedIntSize( value ) )
            .Array.CompressInt32( ref idx, value );
         return info;
      }

      private static ResizableArray<Byte> AddCAString( this ResizableArray<Byte> info, ref Int32 idx, String str )
      {
         if ( str == null )
         {
            info.WriteByteToBytes( ref idx, 0xFF );
         }
         else
         {
            var encoding = MetaDataConstants.SYS_STRING_ENCODING;
            var size = encoding.GetByteCount( str );
            info
               .AddCompressedUInt32( ref idx, size )
               .EnsureThatCanAdd( idx, size );
            idx += encoding.GetBytes( str, 0, str.Length, info.Array, idx );
         }
         return info;
      }

      #endregion


   }

}

public static partial class E_CILPhysical
{
   internal static SignatureElementTypes ReadSigElementType( this Byte[] array, ref Int32 idx )
   {
      return (SignatureElementTypes) array[idx++];
   }

   internal static SignatureStarters ReadSigStarter( this Byte[] array, ref Int32 idx )
   {
      return (SignatureStarters) array[idx++];
   }

   internal static UnmanagedType ReadUnmanagedType( this Byte[] array, ref Int32 idx )
   {
      return (UnmanagedType) array[idx++];
   }

   internal static Boolean TryReadExtraData( this Byte[] array, Int32 idx, Int32 max, out Byte[] extraData )
   {
      var retVal = max >= idx;
      if ( retVal )
      {
         extraData = idx < max ? array.CreateArrayCopy( idx, max - idx ) : Empty<Byte>.Array;
      }
      else
      {
         extraData = null;
      }
      return retVal;
   }
}