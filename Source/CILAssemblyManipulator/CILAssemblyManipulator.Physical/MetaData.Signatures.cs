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
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Physical
{
   public enum SignatureKind
   {
      MethodDefinition,
      MethodReference,
      Field,
      Property,
      LocalVariables,
      Type,
      GenericMethodInstantiation,
      RawSignature
   }

   public abstract class AbstractSignature
   {
      // Disable inheritance to other assemblies
      internal AbstractSignature()
      {

      }

      public abstract SignatureKind SignatureKind { get; }
   }

   public sealed class RawSignature : AbstractSignature
   {
      public RawSignature()
      {

      }

      public Byte[] Bytes { get; set; }
      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.RawSignature;
         }
      }
   }

   public abstract class AbstractNotRawSignature : AbstractSignature
   {
      public Byte[] ExtraData { get; set; }

      public static AbstractNotRawSignature ReadNonTypeSignature(
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
                  locals.Locals.Add( LocalVariableSignature.ReadLocalVariableSignature( array, ref idx ) );
                  retVal = locals;
                  fieldSigTransformedToLocalsSig = true;
               }
               else
               {
                  retVal = FieldSignature.ReadFieldSignatureAfterStarter( array, ref idx );
               }
               break;
            case SignatureStarters.LocalSignature:
               retVal = LocalVariablesSignature.ReadLocalVariablesSignatureAfterStarter( array, ref idx );
               break;
            case SignatureStarters.Property:
            case SignatureStarters.Property | SignatureStarters.HasThis:
               retVal = PropertySignature.ReadPropertySignatureAfterStarter( array, ref idx, starter );
               break;
            case SignatureStarters.MethodSpecGenericInst:
               retVal = GenericMethodSignature.ReadGenericMethodSignatureAfterStarter( array, ref idx );
               break;
            default:
               --idx;
               retVal = methodSigIsDefinition ?
                  (AbstractNotRawSignature) MethodDefinitionSignature.ReadMethodSignature( array, ref idx ) :
                  MethodReferenceSignature.ReadMethodSignature( array, ref idx );
               break;
         }

         Byte[] extraData;
         if ( retVal != null && array.TryReadExtraData( idx, max, out extraData ) )
         {
            retVal.ExtraData = extraData;
         }

         return retVal;
      }
   }

   public abstract class AbstractMethodSignature : AbstractNotRawSignature
   {
      private readonly List<ParameterSignature> _parameters;

      // Disable inheritance to other assemblies
      internal AbstractMethodSignature( Int32 parameterCount )
      {
         this._parameters = new List<ParameterSignature>( parameterCount );
      }

      public SignatureStarters SignatureStarter { get; set; }
      public Int32 GenericArgumentCount { get; set; }
      public ParameterSignature ReturnType { get; set; }
      public List<ParameterSignature> Parameters
      {
         get
         {
            return this._parameters;
         }
      }

      protected static Boolean ReadFromBytes(
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
            returnParameter = ReadParameter( array, ref idx );
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
                     parameters[i] = ReadParameter( array, ref idx );
                  }
               }
               else
               {
                  for ( i = 0; i < amountOfParams; ++i )
                  {
                     parameters[i] = ReadParameter( array, ref idx );
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
         Byte[] array,
         ref Int32 idx
         )
      {
         var retVal = new ParameterSignature();
         CustomModifierSignature.AddFromBytes( array, ref idx, retVal.CustomModifiers );
         var elementType = array.ReadSigElementType( ref idx );
         if ( elementType == SignatureElementTypes.TypedByRef )
         {
            retVal.Type = SimpleTypeSignature.TypedByRef;
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
            retVal.Type = TypeSignature.ReadTypeSignature( array, ref idx );
         }
         return retVal;
      }
   }

   public sealed class MethodDefinitionSignature : AbstractMethodSignature
   {
      public MethodDefinitionSignature( Int32 parameterCount = 0 )
         : base( parameterCount )
      {
      }

      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.MethodDefinition;
         }
      }


      public static MethodDefinitionSignature ReadMethodSignature( Byte[] array, ref Int32 idx )
      {
         SignatureStarters elementType;
         Int32 genericCount;
         ParameterSignature returnParameter;
         ParameterSignature[] parameters;
         Int32 sentinelMark;
         MethodDefinitionSignature retVal;
         if ( ReadFromBytes( array, ref idx, false, out elementType, out genericCount, out returnParameter, out parameters, out sentinelMark ) )
         {
            retVal = new MethodDefinitionSignature( parameters.Length )
            {
               GenericArgumentCount = genericCount,
               ReturnType = returnParameter,
               SignatureStarter = elementType,
            };
            retVal.Parameters.AddRange( parameters );
         }
         else
         {
            retVal = null;
         }
         return retVal;
      }
   }

   public sealed class MethodReferenceSignature : AbstractMethodSignature
   {
      private readonly List<ParameterSignature> _varArgsParameters;

      public MethodReferenceSignature( Int32 parameterCount = 0, Int32 varArgsParameterCount = 0 )
         : base( parameterCount )
      {
         this._varArgsParameters = new List<ParameterSignature>( varArgsParameterCount );
      }

      public List<ParameterSignature> VarArgsParameters
      {
         get
         {
            return this._varArgsParameters;
         }
      }

      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.MethodReference;
         }
      }

      public static MethodReferenceSignature ReadMethodSignature( Byte[] array, ref Int32 idx )
      {
         SignatureStarters elementType;
         Int32 genericCount;
         ParameterSignature returnParameter;
         ParameterSignature[] parameters;
         Int32 sentinelMark;
         MethodReferenceSignature retVal;
         if ( ReadFromBytes( array, ref idx, true, out elementType, out genericCount, out returnParameter, out parameters, out sentinelMark ) )
         {
            var pLength = sentinelMark == -1 ? parameters.Length : sentinelMark;
            var vLength = sentinelMark == -1 ? 0 : ( parameters.Length - sentinelMark );
            retVal = new MethodReferenceSignature( pLength, vLength )
            {
               GenericArgumentCount = genericCount,
               ReturnType = returnParameter,
               SignatureStarter = elementType,
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
   }

   public abstract class AbstractSignatureWithCustomMods : AbstractNotRawSignature
   {
      private readonly List<CustomModifierSignature> _customMods;

      // Disable inheritance to other assemblies
      internal AbstractSignatureWithCustomMods( Int32 customModCount )
      {
         this._customMods = new List<CustomModifierSignature>( customModCount );
      }

      public List<CustomModifierSignature> CustomModifiers
      {
         get
         {
            return this._customMods;
         }
      }
   }

   public sealed class FieldSignature : AbstractSignatureWithCustomMods
   {
      public FieldSignature( Int32 customModCount = 0 )
         : base( customModCount )
      {
      }

      public TypeSignature Type { get; set; }

      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.Field;
         }
      }

      public static FieldSignature ReadFieldSignature( Byte[] array, ref Int32 idx )
      {
         SignatureStarters starter;
         return ( starter = array.ReadSigStarter( ref idx ) ) == SignatureStarters.Field ?
            ReadFieldSignatureAfterStarter( array, ref idx ) :
            null;
      }

      internal static FieldSignature ReadFieldSignatureAfterStarter( Byte[] array, ref Int32 idx )
      {
         var retVal = new FieldSignature();
         CustomModifierSignature.AddFromBytes( array, ref idx, retVal.CustomModifiers );
         retVal.Type = TypeSignature.ReadTypeSignature( array, ref idx );
         return retVal;
      }

   }

   public sealed class PropertySignature : AbstractSignatureWithCustomMods
   {
      private readonly List<ParameterSignature> _parameters;

      public PropertySignature( Int32 customModCount = 0, Int32 parameterCount = 0 )
         : base( customModCount )
      {
         this._parameters = new List<ParameterSignature>( parameterCount );
      }

      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.Property;
         }
      }

      public Boolean HasThis { get; set; }
      public TypeSignature PropertyType { get; set; }
      public List<ParameterSignature> Parameters
      {
         get
         {
            return this._parameters;
         }
      }

      public static PropertySignature ReadPropertySignature(
         Byte[] array,
         ref Int32 idx
         )
      {
         SignatureStarters starter;
         return ( starter = array.ReadSigStarter( ref idx ) ).IsProperty() ?
            ReadPropertySignatureAfterStarter( array, ref idx, starter ) :
            null;
      }

      internal static PropertySignature ReadPropertySignatureAfterStarter(
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
            CustomModifierSignature.AddFromBytes( array, ref idx, retVal.CustomModifiers );
            retVal.PropertyType = TypeSignature.ReadTypeSignature( array, ref idx );

            for ( var i = 0; i < paramCount; ++i )
            {
               retVal.Parameters.Add( AbstractMethodSignature.ReadParameter( array, ref idx ) );
            }
         }
         else
         {
            retVal = null;
         }
         return retVal;
      }
   }

   public sealed class LocalVariablesSignature : AbstractNotRawSignature
   {
      private readonly List<LocalVariableSignature> _locals;

      public LocalVariablesSignature( Int32 localsCount = 0 )
      {
         this._locals = new List<LocalVariableSignature>();
      }

      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.LocalVariables;
         }
      }

      public List<LocalVariableSignature> Locals
      {
         get
         {
            return this._locals;
         }
      }

      public static LocalVariablesSignature ReadLocalVariablesSignature(
         Byte[] array,
         ref Int32 idx
         )
      {
         SignatureStarters starter;
         return ( starter = array.ReadSigStarter( ref idx ) ) == SignatureStarters.LocalSignature ?
            ReadLocalVariablesSignatureAfterStarter( array, ref idx ) :
            null;
      }

      internal static LocalVariablesSignature ReadLocalVariablesSignatureAfterStarter(
         Byte[] array,
         ref Int32 idx
         )
      {
         var localsCount = array.DecompressUInt32( ref idx );
         var retVal = new LocalVariablesSignature( localsCount );
         for ( var i = 0; i < localsCount; ++i )
         {
            retVal.Locals.Add( LocalVariableSignature.ReadLocalVariableSignature( array, ref idx ) );
         }
         return retVal;
      }
   }

   public abstract class ParameterOrLocalVariableSignature
   {
      private readonly List<CustomModifierSignature> _customMods;

      // Disable inheritance to other assemblies
      internal ParameterOrLocalVariableSignature( Int32 customModCount )
      {
         this._customMods = new List<CustomModifierSignature>( customModCount );
      }

      public List<CustomModifierSignature> CustomModifiers
      {
         get
         {
            return this._customMods;
         }
      }

      public Boolean IsByRef { get; set; }
      public TypeSignature Type { get; set; }
   }

   public sealed class LocalVariableSignature : ParameterOrLocalVariableSignature
   {
      public LocalVariableSignature( Int32 customModCount = 0 )
         : base( customModCount )
      {

      }

      public Boolean IsPinned { get; set; }

      public static LocalVariableSignature ReadLocalVariableSignature(
         Byte[] array,
         ref Int32 idx
         )
      {
         var retVal = new LocalVariableSignature();
         if ( array[idx] == (Byte) SignatureElementTypes.TypedByRef )
         {
            retVal.Type = SimpleTypeSignature.TypedByRef;
         }
         else
         {
            CustomModifierSignature.AddFromBytes( array, ref idx, retVal.CustomModifiers );
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

            retVal.Type = TypeSignature.ReadTypeSignature( array, ref idx );
         }

         return retVal;
      }
   }

   public sealed class ParameterSignature : ParameterOrLocalVariableSignature
   {
      public ParameterSignature( Int32 customModCount = 0 )
         : base( customModCount )
      {

      }
   }

   public sealed class CustomModifierSignature
   {
      public Boolean IsOptional { get; set; }
      public TableIndex CustomModifierType { get; set; }

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
                  CustomModifierType = TableIndex.FromOneBasedToken( TableIndex.DecodeTypeDefOrRefOrSpec( array, ref idx ) ),
                  IsOptional = sigType == SignatureElementTypes.CModOpt
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
   }

   public abstract class TypeSignature : AbstractNotRawSignature
   {
      // Disable inheritance to other assemblies
      internal TypeSignature()
      {

      }

      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.Type;
         }
      }

      public abstract TypeSignatureKind TypeSignatureKind { get; }


      public static TypeSignature ReadTypeSignature( Byte[] array, ref Int32 idx, Int32 max = -1 )
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
               retVal = SimpleTypeSignature.GetByElement( elementType );
               break;
            case SignatureElementTypes.Array:
               var arrayType = ReadTypeSignature( array, ref idx );
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

               var actualType = TableIndex.FromOneBasedToken( TableIndex.DecodeTypeDefOrRefOrSpec( array, ref idx ) );
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
                        var curGArg = ReadTypeSignature( array, ref idx );
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
                  MethodSignature = MethodReferenceSignature.ReadMethodSignature( array, ref idx )
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
               CustomModifierSignature.AddFromBytes( array, ref idx, ptr.CustomModifiers );
               ptr.PointerType = ReadTypeSignature( array, ref idx );
               retVal = ptr;
               break;
            case SignatureElementTypes.SzArray:
               var szArr = new SimpleArrayTypeSignature();
               CustomModifierSignature.AddFromBytes( array, ref idx, szArr.CustomModifiers );
               szArr.ArrayType = ReadTypeSignature( array, ref idx );
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

      private static ComplexArrayTypeSignature ReadArrayInfo( Byte[] array, ref Int32 idx )
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

   }

   public enum TypeSignatureKind : byte
   {
      Simple,
      ComplexArray,
      ClassOrValue,
      GenericParameter,
      FunctionPointer,
      Pointer,
      SimpleArray,
   }

   public sealed class SimpleTypeSignature : TypeSignature
   {
      public static readonly SimpleTypeSignature Boolean = new SimpleTypeSignature( SignatureElementTypes.Boolean );
      public static readonly SimpleTypeSignature Char = new SimpleTypeSignature( SignatureElementTypes.Char );
      public static readonly SimpleTypeSignature SByte = new SimpleTypeSignature( SignatureElementTypes.I1 );
      public static readonly SimpleTypeSignature Byte = new SimpleTypeSignature( SignatureElementTypes.U1 );
      public static readonly SimpleTypeSignature Int16 = new SimpleTypeSignature( SignatureElementTypes.I2 );
      public static readonly SimpleTypeSignature UInt16 = new SimpleTypeSignature( SignatureElementTypes.U2 );
      public static readonly SimpleTypeSignature Int32 = new SimpleTypeSignature( SignatureElementTypes.I4 );
      public static readonly SimpleTypeSignature UInt32 = new SimpleTypeSignature( SignatureElementTypes.U4 );
      public static readonly SimpleTypeSignature Int64 = new SimpleTypeSignature( SignatureElementTypes.I8 );
      public static readonly SimpleTypeSignature UInt64 = new SimpleTypeSignature( SignatureElementTypes.U8 );
      public static readonly SimpleTypeSignature Single = new SimpleTypeSignature( SignatureElementTypes.R4 );
      public static readonly SimpleTypeSignature Double = new SimpleTypeSignature( SignatureElementTypes.R8 );
      public static readonly SimpleTypeSignature IntPtr = new SimpleTypeSignature( SignatureElementTypes.I );
      public static readonly SimpleTypeSignature UIntPtr = new SimpleTypeSignature( SignatureElementTypes.U );
      public static readonly SimpleTypeSignature Object = new SimpleTypeSignature( SignatureElementTypes.Object );
      public static readonly SimpleTypeSignature String = new SimpleTypeSignature( SignatureElementTypes.String );
      public static readonly SimpleTypeSignature Void = new SimpleTypeSignature( SignatureElementTypes.Void );
      public static readonly SimpleTypeSignature TypedByRef = new SimpleTypeSignature( SignatureElementTypes.TypedByRef );

      private readonly SignatureElementTypes _type;

      private SimpleTypeSignature( SignatureElementTypes type )
      {
         this._type = type;
      }

      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.Simple;
         }
      }

      public SignatureElementTypes SimpleType
      {
         get
         {
            return this._type;
         }
      }

      public static SimpleTypeSignature GetByElement( SignatureElementTypes element )
      {
         switch ( element )
         {
            case SignatureElementTypes.Boolean:
               return Boolean;
            case SignatureElementTypes.Char:
               return Char;
            case SignatureElementTypes.I1:
               return SByte;
            case SignatureElementTypes.U1:
               return Byte;
            case SignatureElementTypes.I2:
               return Int16;
            case SignatureElementTypes.U2:
               return UInt16;
            case SignatureElementTypes.I4:
               return Int32;
            case SignatureElementTypes.U4:
               return UInt32;
            case SignatureElementTypes.I8:
               return Int64;
            case SignatureElementTypes.U8:
               return UInt64;
            case SignatureElementTypes.R4:
               return Single;
            case SignatureElementTypes.R8:
               return Double;
            case SignatureElementTypes.I:
               return IntPtr;
            case SignatureElementTypes.U:
               return UIntPtr;
            case SignatureElementTypes.Object:
               return Object;
            case SignatureElementTypes.String:
               return String;
            case SignatureElementTypes.Void:
               return Void;
            case SignatureElementTypes.TypedByRef:
               return TypedByRef;
            default:
               throw new InvalidOperationException( "Element " + element + " does not represent simple type." );
         }
      }
   }

   public sealed class ClassOrValueTypeSignature : TypeSignature
   {
      private readonly List<TypeSignature> _genericArguments;

      public ClassOrValueTypeSignature( Int32 genericArgumentsCount = 0 )
      {
         this._genericArguments = new List<TypeSignature>( genericArgumentsCount );
      }

      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.ClassOrValue;
         }
      }

      public Boolean IsClass { get; set; }
      public TableIndex Type { get; set; }
      public List<TypeSignature> GenericArguments
      {
         get
         {
            return this._genericArguments;
         }
      }
   }

   public sealed class GenericParameterTypeSignature : TypeSignature
   {

      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.GenericParameter;
         }
      }

      public Boolean IsTypeParameter { get; set; }
      public Int32 GenericParameterIndex { get; set; }
   }

   public sealed class FunctionPointerTypeSignature : TypeSignature
   {

      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.FunctionPointer;
         }
      }

      public MethodReferenceSignature MethodSignature { get; set; }
   }

   public sealed class PointerTypeSignature : TypeSignature
   {
      private readonly List<CustomModifierSignature> _customMods;

      public PointerTypeSignature( Int32 customModCount = 0 )
      {
         this._customMods = new List<CustomModifierSignature>( customModCount );
      }

      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.Pointer;
         }
      }

      public List<CustomModifierSignature> CustomModifiers
      {
         get
         {
            return this._customMods;
         }
      }

      public TypeSignature PointerType { get; set; }
   }

   public abstract class AbstractArrayTypeSignature : TypeSignature
   {
      // Disable inheritance to other assemblies
      internal AbstractArrayTypeSignature()
      {

      }

      public TypeSignature ArrayType { get; set; }
   }

   public sealed class ComplexArrayTypeSignature : AbstractArrayTypeSignature
   {
      private readonly List<Int32> _sizes;
      private readonly List<Int32> _lowerBounds;

      public ComplexArrayTypeSignature( Int32 sizesCount = 0, Int32 lowerBoundsCount = 0 )
      {
         this._sizes = new List<Int32>( sizesCount );
         this._lowerBounds = new List<Int32>( lowerBoundsCount );
      }

      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.ComplexArray;
         }
      }

      public Int32 Rank { get; set; }
      public List<Int32> Sizes
      {
         get
         {
            return this._sizes;
         }
      }
      public List<Int32> LowerBounds
      {
         get
         {
            return this._lowerBounds;
         }
      }
   }

   public sealed class SimpleArrayTypeSignature : AbstractArrayTypeSignature
   {
      private readonly List<CustomModifierSignature> _customMods;

      public SimpleArrayTypeSignature( Int32 customModCount = 0 )
      {
         this._customMods = new List<CustomModifierSignature>( customModCount );
      }

      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.SimpleArray;
         }
      }

      public List<CustomModifierSignature> CustomModifiers
      {
         get
         {
            return this._customMods;
         }
      }
   }

   public sealed class GenericMethodSignature : AbstractNotRawSignature
   {
      private readonly List<TypeSignature> _genericArguments;

      public GenericMethodSignature( Int32 genericArgumentsCount = 0 )
      {
         this._genericArguments = new List<TypeSignature>( genericArgumentsCount );
      }

      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.GenericMethodInstantiation;
         }
      }

      public List<TypeSignature> GenericArguments
      {
         get
         {
            return this._genericArguments;
         }
      }

      public static GenericMethodSignature ReadGenericMethodSignature(
         Byte[] array,
         ref Int32 idx
         )
      {
         SignatureStarters starter;
         return ( starter = array.ReadSigStarter( ref idx ) ) == SignatureStarters.MethodSpecGenericInst ?
            ReadGenericMethodSignatureAfterStarter( array, ref idx ) :
            null;
      }

      public static GenericMethodSignature ReadGenericMethodSignatureAfterStarter(
         Byte[] array,
         ref Int32 idx
         )
      {
         var gArgsCount = array.DecompressUInt32( ref idx );
         var retVal = new GenericMethodSignature( gArgsCount );
         for ( var i = 0; i < gArgsCount; ++i )
         {
            retVal.GenericArguments.Add( TypeSignature.ReadTypeSignature( array, ref idx ) );
         }
         return retVal;
      }
   }

   /// <summary>
   /// This class represents marshalling information used for CIL parameters and fields.
   /// Subclasses of this class further define what kind of marshaling info is in question.
   /// </summary>
   /// <seealso cref="CILElementWithMarshalingInfo"/>
   /// <seealso cref="CILField"/>
   /// <seealso cref="CILParameter"/>
   public abstract class AbstractMarshalingInfo
   {
      internal const Int32 NO_INDEX = -1;

      // Disable inheritance to other assemblies
      internal AbstractMarshalingInfo()
      {
      }

      /// <summary>
      /// Gets the <see cref="UnmanagedType" /> value the data is to be marshaled as.
      /// </summary>
      /// <value>The <see cref="UnmanagedType" /> value the data is to be marshaled as.</value>
      public UnmanagedType Value { get; set; }

      public abstract MarshalingInfoKind MarshalingInfoKind { get; }

#if !CAM_PHYSICAL_IS_PORTABLE

      /// <summary>
      /// Creates <see cref="AbstractMarshalingInfo"/> with all information specified in <see cref="System.Runtime.InteropServices.MarshalAsAttribute"/>.
      /// </summary>
      /// <param name="attr">The <see cref="System.Runtime.InteropServices.MarshalAsAttribute"/>. If <c>null</c>, then the result will be <c>null</c> as well.</param>
      /// <returns>A new <see cref="AbstractMarshalingInfo"/> with given information.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="attr"/> has non-<c>null</c> <see cref="System.Runtime.InteropServices.MarshalAsAttribute.SafeArrayUserDefinedSubType"/>, <see cref="System.Runtime.InteropServices.MarshalAsAttribute.MarshalType"/> or <see cref="System.Runtime.InteropServices.MarshalAsAttribute.MarshalTypeRef"/> fields, and <paramref name="ctx"/> is <c>null</c>.</exception>
      public static AbstractMarshalingInfo FromAttribute( System.Runtime.InteropServices.MarshalAsAttribute attr )
      {
         AbstractMarshalingInfo result;
         if ( attr == null )
         {
            result = null;
         }
         else
         {
            var ut = (UnmanagedType) attr.Value;
            switch ( ut )
            {
               case UnmanagedType.ByValTStr:
                  result = new FixedLengthStringMarshalingInfo()
                  {
                     Value = ut,
                     Size = attr.SizeConst
                  };
                  break;
               case UnmanagedType.IUnknown:
               case UnmanagedType.IDispatch:
               case UnmanagedType.Interface:
                  result = new InterfaceMarshalingInfo()
                  {
                     Value = ut,
                     IIDParameterIndex = attr.IidParameterIndex
                  };
                  break;
               case UnmanagedType.SafeArray:
                  result = new SafeArrayMarshalingInfo()
                  {
                     Value = ut,
                     ElementType = (VarEnum) attr.SafeArraySubType,
                     UserDefinedType = attr.SafeArrayUserDefinedSubType?.AssemblyQualifiedName
                  };
                  break;
               case UnmanagedType.ByValArray:
                  result = new FixedLengthArrayMarshalingInfo()
                  {
                     Value = ut,
                     Size = attr.SizeConst,
                     ElementType = (UnmanagedType) attr.ArraySubType
                  };
                  break;
               case UnmanagedType.LPArray:
                  result = new ArrayMarshalingInfo()
                  {
                     Value = ut,
                     ElementType = (UnmanagedType) attr.ArraySubType,
                     SizeParameterIndex = attr.SizeParamIndex,
                     Size = attr.SizeConst,
                     Flags = -1
                  };
                  break;
               case UnmanagedType.CustomMarshaler:
                  result = new CustomMarshalingInfo()
                  {
                     Value = ut,
                     GUIDString = null,
                     NativeTypeName = null,
                     CustomMarshalerTypeName = attr.MarshalType,
                     MarshalCookie = attr.MarshalCookie
                  };
                  break;
               default:
                  result = new SimpleMarshalingInfo()
                  {
                     Value = ut
                  };
                  break;
            }
         }
         return result;
      }

#endif

      public static AbstractMarshalingInfo ReadFromBytes(
         Byte[] array,
         ref Int32 idx,
         Int32 max
         )
      {
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

   }

   public enum MarshalingInfoKind
   {
      Simple,
      FixedLengthString,
      FixedLengthArray,
      SafeArray,
      Array,
      Interface,
      Custom,
      Raw
   }

   public sealed class SimpleMarshalingInfo : AbstractMarshalingInfo
   {
      public override MarshalingInfoKind MarshalingInfoKind
      {
         get
         {
            return MarshalingInfoKind.Simple;
         }
      }
   }

   public sealed class FixedLengthStringMarshalingInfo : AbstractMarshalingInfo
   {
      public override MarshalingInfoKind MarshalingInfoKind
      {
         get
         {
            return MarshalingInfoKind.FixedLengthString;
         }
      }

      /// <summary>
      /// Gets or sets the number of characters (not bytes) in a string.
      /// </summary>
      /// <value>The number of characters (not bytes) in a string.</value>
      public Int32 Size { get; set; }
   }

   public sealed class InterfaceMarshalingInfo : AbstractMarshalingInfo
   {
      public override MarshalingInfoKind MarshalingInfoKind
      {
         get
         {
            return MarshalingInfoKind.Interface;
         }
      }

      /// <summary>
      /// Gets or sets the zero-based parameter index of the unmanaged iid_is attribute used by COM.
      /// </summary>
      /// <value>The parameter index of the unmanaged iid_is attribute used by COM.</value>
      public Int32 IIDParameterIndex { get; set; }
   }

   public sealed class SafeArrayMarshalingInfo : AbstractMarshalingInfo
   {
      public override MarshalingInfoKind MarshalingInfoKind
      {
         get
         {
            return MarshalingInfoKind.SafeArray;
         }
      }

      /// <summary>
      /// Gets or sets the type for array elements.
      /// </summary>
      /// <value>The type for array elements.</value>
      public VarEnum ElementType { get; set; }

      /// <summary>
      /// Gets or sets the element type string.
      /// </summary>
      /// <value>The element type string.</value>
      public String UserDefinedType { get; set; }
   }

   public sealed class FixedLengthArrayMarshalingInfo : AbstractMarshalingInfo
   {
      public override MarshalingInfoKind MarshalingInfoKind
      {
         get
         {
            return MarshalingInfoKind.FixedLengthArray;
         }
      }

      /// <summary>
      /// Gets or sets the number of elements in an array.
      /// </summary>
      /// <value>The number of elements in an array.</value>
      public Int32 Size { get; set; }

      /// <summary>
      /// Gets or sets the type for array elements.
      /// </summary>
      /// <value>The type for array elements.</value>
      public UnmanagedType ElementType { get; set; }
   }

   public sealed class ArrayMarshalingInfo : AbstractMarshalingInfo
   {
      public override MarshalingInfoKind MarshalingInfoKind
      {
         get
         {
            return MarshalingInfoKind.Array;
         }
      }

      /// <summary>
      /// Gets or sets the type for array elements.
      /// </summary>
      /// <value>The type for array elements.</value>
      public UnmanagedType ElementType { get; set; }

      /// <summary>
      /// Gets the zero-based index of the parameter containing the count of the array elements.
      /// </summary>
      /// <value>The zero-based index of the parameter containing the count of the array elements.</value>
      public Int32 SizeParameterIndex { get; set; }

      /// <summary>
      /// Gets or sets the number of elements in an array.
      /// </summary>
      /// <value>The number of elements in an array.</value>
      public Int32 Size { get; set; }

      /// <summary>
      /// Gets or sets flags for this marshaling info.
      /// </summary>
      /// <value>The flags for this marshaling info.</value>
      public Int32 Flags { get; set; }
   }

   public sealed class CustomMarshalingInfo : AbstractMarshalingInfo
   {
      public override MarshalingInfoKind MarshalingInfoKind
      {
         get
         {
            return MarshalingInfoKind.Custom;
         }
      }

      /// <summary>
      /// Gets or sets the COM GUID string for this marshaling info.
      /// </summary>
      /// <value>The COM GUID string for this marshaling info.</value>
      public String GUIDString { get; set; }

      /// <summary>
      /// Gets or sets the native type name for this marshaling info.
      /// </summary>
      /// <value>The native type name for this marshaling info.</value>
      public String NativeTypeName { get; set; }

      /// <summary>
      /// Gets or sets the custom marshaler type name for this marshaling info.
      /// </summary>
      /// <value>The custom marshaler type name for this marshaling info.</value>
      public String CustomMarshalerTypeName { get; set; }

      /// <summary>
      /// Gets or sets the additional information for custom marshaler.
      /// </summary>
      /// <value>The additional information for custom marshaler.</value>
      public String MarshalCookie { get; set; }
   }

   public sealed class RawMarshalingInfo : AbstractMarshalingInfo
   {
      public override MarshalingInfoKind MarshalingInfoKind
      {
         get
         {
            return MarshalingInfoKind.Raw;
         }
      }

      /// <summary>
      /// Gets or sets the raw binary marshaling info, except for the starting byte (the value of <see cref="AbstractMarshalingInfo.Value"/> property).
      /// </summary>
      /// <value>The raw binary marshaling info.</value>
      public Byte[] Bytes { get; set; }
   }

   public abstract class AbstractCustomAttributeSignature
   {
      // Disable inheritance to other assemblies
      internal AbstractCustomAttributeSignature()
      {

      }

      public abstract CustomAttributeSignatureKind CustomAttributeSignatureKind { get; }
   }

   public enum CustomAttributeSignatureKind
   {
      Raw,
      Resolved
   }

   public sealed class RawCustomAttributeSignature : AbstractCustomAttributeSignature
   {
      public Byte[] Bytes { get; set; }
      public override CustomAttributeSignatureKind CustomAttributeSignatureKind
      {
         get
         {
            return CustomAttributeSignatureKind.Raw;
         }
      }
   }

   public sealed class CustomAttributeSignature : AbstractCustomAttributeSignature
   {
      private readonly List<CustomAttributeTypedArgument> _typedArgs;
      private readonly List<CustomAttributeNamedArgument> _namedArgs;

      public CustomAttributeSignature( Int32 typedArgsCount = 0, Int32 namedArgsCount = 0 )
      {
         this._typedArgs = new List<CustomAttributeTypedArgument>( typedArgsCount );
         this._namedArgs = new List<CustomAttributeNamedArgument>( namedArgsCount );
      }

      public override CustomAttributeSignatureKind CustomAttributeSignatureKind
      {
         get
         {
            return CustomAttributeSignatureKind.Resolved;
         }
      }

      public List<CustomAttributeTypedArgument> TypedArguments
      {
         get
         {
            return this._typedArgs;
         }
      }

      public List<CustomAttributeNamedArgument> NamedArguments
      {
         get
         {
            return this._namedArgs;
         }
      }
   }

   /// <summary>
   /// TODO: modification is easier if there is only one class for typed arguments, i.e. just use Value setter instead of creating new TypedArgument object and set value.
   /// </summary>
   public sealed class CustomAttributeTypedArgument
   {
      // Note: Enum values should be CustomAttributeValue_EnumReferences
      // Note: Type values should be CustomAttributeValue_TypeReferences
      // Note: Arrays should be CustomAttributeValue_Arrays
      public Object Value { get; set; }
   }

   public enum CustomAttributeTypedArgumentValueKind
   {
      Type,
      Enum,
      Array
   }

   public interface CustomAttributeTypedArgumentValueComplex
   {
      CustomAttributeTypedArgumentValueKind CustomAttributeTypedArgumentValueKind { get; }
   }

   public struct CustomAttributeValue_TypeReference : IEquatable<CustomAttributeValue_TypeReference>, CustomAttributeTypedArgumentValueComplex
   {
      private readonly String _typeString;

      public CustomAttributeValue_TypeReference( String typeString )
      {
         this._typeString = typeString;
      }

      public CustomAttributeTypedArgumentValueKind CustomAttributeTypedArgumentValueKind
      {
         get
         {
            return CustomAttributeTypedArgumentValueKind.Type;
         }
      }

      public String TypeString
      {
         get
         {
            return this._typeString;
         }
      }

      public override Boolean Equals( Object obj )
      {
         return obj is CustomAttributeValue_TypeReference && this.Equals( (CustomAttributeValue_TypeReference) obj );
      }

      public override Int32 GetHashCode()
      {
         return this._typeString.GetHashCodeSafe();
      }

      public Boolean Equals( CustomAttributeValue_TypeReference other )
      {
         return String.Equals( this._typeString, other._typeString );
      }


   }

   public struct CustomAttributeValue_EnumReference : IEquatable<CustomAttributeValue_EnumReference>, CustomAttributeTypedArgumentValueComplex
   {
      private readonly String _enumType;
      private readonly Object _enumValue;

      public CustomAttributeValue_EnumReference( String enumType, Object enumValue )
      {
         this._enumType = enumType;
         this._enumValue = enumValue;
      }

      public CustomAttributeTypedArgumentValueKind CustomAttributeTypedArgumentValueKind
      {
         get
         {
            return CustomAttributeTypedArgumentValueKind.Enum;
         }
      }

      public String EnumType
      {
         get
         {
            return this._enumType;
         }
      }

      public Object EnumValue
      {
         get
         {
            return this._enumValue;
         }
      }

      public override Boolean Equals( Object obj )
      {
         return obj is CustomAttributeValue_EnumReference && this.Equals( (CustomAttributeValue_EnumReference) obj );
      }

      public override Int32 GetHashCode()
      {
         return ( 17 * 23 + this._enumType.GetHashCodeSafe() ) * 23 + this._enumValue.GetHashCodeSafe();
      }

      public Boolean Equals( CustomAttributeValue_EnumReference other )
      {
         return String.Equals( this._enumType, other._enumType )
            && Equals( this._enumValue, other._enumValue );
      }
   }

   public struct CustomAttributeValue_Array : IEquatable<CustomAttributeValue_Array>, CustomAttributeTypedArgumentValueComplex
   {
      private readonly Array _array;
      private readonly CustomAttributeArgumentType _arrayElementType;

      public CustomAttributeValue_Array( Array array, CustomAttributeArgumentType arrayElementTypeString )
      {
         this._array = array;
         this._arrayElementType = arrayElementTypeString;
      }

      public CustomAttributeTypedArgumentValueKind CustomAttributeTypedArgumentValueKind
      {
         get
         {
            return CustomAttributeTypedArgumentValueKind.Array;
         }
      }

      public Array Array
      {
         get
         {
            return this._array;
         }
      }

      public CustomAttributeArgumentType ArrayElementType
      {
         get
         {
            return this._arrayElementType;
         }
      }

      public override Boolean Equals( Object obj )
      {
         return obj is CustomAttributeValue_Array && this.Equals( (CustomAttributeValue_Array) obj );
      }

      public override Int32 GetHashCode()
      {
         return ( 17 * 23 + this._arrayElementType.GetHashCodeSafe() ) * 23 + SequenceEqualityComparer<IEnumerable<Object>, Object>.SequenceHashCode( this.Array.Cast<Object>() );
      }

      public Boolean Equals( CustomAttributeValue_Array other )
      {
         return this._arrayElementType.EqualsTyped( other._arrayElementType )
            && SequenceEqualityComparer<IEnumerable<Object>, Object>.SequenceEquality( this._array.Cast<Object>(), other._array.Cast<Object>() );
      }
   }

   public sealed class CustomAttributeNamedArgument
   {
      public CustomAttributeTypedArgument Value { get; set; }
      public CustomAttributeArgumentType FieldOrPropertyType { get; set; }
      public String Name { get; set; }
      public Boolean IsField { get; set; }
   }

   public enum CustomAttributeArgumentTypeKind
   {
      Simple,
      TypeString,
      Array
   }

   public abstract class CustomAttributeArgumentType
   {
      // Disable inheritance to other assemblies
      internal CustomAttributeArgumentType()
      {

      }

      public abstract CustomAttributeArgumentTypeKind ArgumentTypeKind { get; }
   }

   public sealed class CustomAttributeArgumentTypeSimple : CustomAttributeArgumentType
   {
      public static readonly CustomAttributeArgumentTypeSimple Boolean = new CustomAttributeArgumentTypeSimple( SignatureElementTypes.Boolean );
      public static readonly CustomAttributeArgumentTypeSimple Char = new CustomAttributeArgumentTypeSimple( SignatureElementTypes.Char );
      public static readonly CustomAttributeArgumentTypeSimple SByte = new CustomAttributeArgumentTypeSimple( SignatureElementTypes.I1 );
      public static readonly CustomAttributeArgumentTypeSimple Byte = new CustomAttributeArgumentTypeSimple( SignatureElementTypes.U1 );
      public static readonly CustomAttributeArgumentTypeSimple Int16 = new CustomAttributeArgumentTypeSimple( SignatureElementTypes.I2 );
      public static readonly CustomAttributeArgumentTypeSimple UInt16 = new CustomAttributeArgumentTypeSimple( SignatureElementTypes.U2 );
      public static readonly CustomAttributeArgumentTypeSimple Int32 = new CustomAttributeArgumentTypeSimple( SignatureElementTypes.I4 );
      public static readonly CustomAttributeArgumentTypeSimple UInt32 = new CustomAttributeArgumentTypeSimple( SignatureElementTypes.U4 );
      public static readonly CustomAttributeArgumentTypeSimple Int64 = new CustomAttributeArgumentTypeSimple( SignatureElementTypes.I8 );
      public static readonly CustomAttributeArgumentTypeSimple UInt64 = new CustomAttributeArgumentTypeSimple( SignatureElementTypes.U8 );
      public static readonly CustomAttributeArgumentTypeSimple Single = new CustomAttributeArgumentTypeSimple( SignatureElementTypes.R4 );
      public static readonly CustomAttributeArgumentTypeSimple Double = new CustomAttributeArgumentTypeSimple( SignatureElementTypes.R8 );
      public static readonly CustomAttributeArgumentTypeSimple String = new CustomAttributeArgumentTypeSimple( SignatureElementTypes.String );
      public static readonly CustomAttributeArgumentTypeSimple Type = new CustomAttributeArgumentTypeSimple( SignatureElementTypes.Type );
      public static readonly CustomAttributeArgumentTypeSimple Object = new CustomAttributeArgumentTypeSimple( SignatureElementTypes.Object );

      private SignatureElementTypes _kind;

      private CustomAttributeArgumentTypeSimple( SignatureElementTypes kind )
      {
         this._kind = kind;
      }

      public override CustomAttributeArgumentTypeKind ArgumentTypeKind
      {
         get
         {
            return CustomAttributeArgumentTypeKind.Simple;
         }
      }

      public SignatureElementTypes SimpleType
      {
         get
         {
            return this._kind;
         }
      }
   }

   public sealed class CustomAttributeArgumentTypeEnum : CustomAttributeArgumentType
   {
      public override CustomAttributeArgumentTypeKind ArgumentTypeKind
      {
         get
         {
            return CustomAttributeArgumentTypeKind.TypeString;
         }
      }

      public String TypeString { get; set; }
   }

   public sealed class CustomAttributeArgumentTypeArray : CustomAttributeArgumentType
   {

      public override CustomAttributeArgumentTypeKind ArgumentTypeKind
      {
         get
         {
            return CustomAttributeArgumentTypeKind.Array;
         }
      }

      public CustomAttributeArgumentType ArrayType { get; set; }
   }

   public abstract class AbstractSecurityInformation
   {
      private const String PERMISSION_SET = "System.Security.Permissions.PermissionSetAttribute";
      private const String PERMISSION_SET_XML_PROP = "XML";
      internal const Byte DECL_SECURITY_HEADER = 0x2E; // '.'

      // Disable inheritance to other assemblies
      internal AbstractSecurityInformation()
      {

      }

      /// <summary>
      /// Gets or sets the type of the security attribute.
      /// </summary>
      /// <value>The type of the security attribute.</value>
      public String SecurityAttributeType { get; set; }

      public abstract SecurityInformationKind SecurityInformationKind { get; }

      public static void ReadSecurityInformation(
         Byte[] array,
         ref Int32 idx,
         Int32 max,
         List<AbstractSecurityInformation> secInfos
         )
      {
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
   }

   public enum SecurityInformationKind
   {
      Resolved,
      Raw
   }

   public sealed class RawSecurityInformation : AbstractSecurityInformation
   {
      public Int32 ArgumentCount { get; set; }
      public Byte[] Bytes { get; set; }
      public override SecurityInformationKind SecurityInformationKind
      {
         get
         {
            return SecurityInformationKind.Raw;
         }
      }
   }

   /// <summary>
   /// This class represents a single security attribute declaration.
   /// Instances of this class are created via <see cref="CILElementWithSecurityInformation.AddDeclarativeSecurity(API.SecurityAction, CILType)"/> method.
   /// </summary>
   /// <seealso cref="CILElementWithSecurityInformation"/>
   /// <seealso cref="CILElementWithSecurityInformation.AddDeclarativeSecurity(API.SecurityAction, CILType)"/>
   public sealed class SecurityInformation : AbstractSecurityInformation
   {
      private readonly List<CustomAttributeNamedArgument> _namedArguments;

      public SecurityInformation( Int32 namedArgumentsCount = 0 )
      {
         this._namedArguments = new List<CustomAttributeNamedArgument>( namedArgumentsCount );
      }

      /// <summary>
      /// Gets the <see cref="CILCustomAttributeNamedArgument"/>s of this security attribute declaration.
      /// </summary>
      /// <value>The <see cref="CILCustomAttributeNamedArgument"/>s of this security attribute declaration.</value>
      public List<CustomAttributeNamedArgument> NamedArguments
      {
         get
         {
            return this._namedArguments;
         }
      }

      public override SecurityInformationKind SecurityInformationKind
      {
         get
         {
            return SecurityInformationKind.Resolved;
         }
      }
   }
}

public static partial class E_CILPhysical
{
   public static TSignature CreateDeepCopy<TSignature>( this TSignature sig, Func<TableIndex, TableIndex> tableIndexTranslator = null )
      where TSignature : AbstractSignature
   {
      switch ( sig.SignatureKind )
      {
         case SignatureKind.Field:
            return CloneFieldSignature( sig as FieldSignature, tableIndexTranslator ) as TSignature;
         case SignatureKind.GenericMethodInstantiation:
            return CloneGenericMethodSignature( sig as GenericMethodSignature, tableIndexTranslator ) as TSignature;
         case SignatureKind.LocalVariables:
            return CloneLocalsSignature( sig as LocalVariablesSignature, tableIndexTranslator ) as TSignature;
         case SignatureKind.MethodDefinition:
            return CloneMethodDefSignature( sig as MethodDefinitionSignature, tableIndexTranslator ) as TSignature;
         case SignatureKind.MethodReference:
            return CloneMethodRefSignature( sig as MethodReferenceSignature, tableIndexTranslator ) as TSignature;
         case SignatureKind.Type:
            return CloneTypeSignature( sig as TypeSignature, tableIndexTranslator ) as TSignature;
         case SignatureKind.RawSignature:
            return CloneRawSignature( sig as RawSignature, tableIndexTranslator ) as TSignature;
         case SignatureKind.Property:
            return ClonePropertySignature( sig as PropertySignature, tableIndexTranslator ) as TSignature;
         default:
            throw new NotSupportedException( "Invalid signature kind: " + sig.SignatureKind + "." );
      }
   }

   private static RawSignature CloneRawSignature( RawSignature sig, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var idx = 0;
      var bytes = sig.Bytes;
      return new RawSignature() { Bytes = bytes.CreateAndBlockCopyTo( ref idx, bytes.Length ) };
   }

   private static TypeSignature CloneTypeSignature( TypeSignature sig, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      TypeSignature retVal;
      switch ( sig.TypeSignatureKind )
      {
         case TypeSignatureKind.ClassOrValue:
            var clazz = (ClassOrValueTypeSignature) sig;
            var clazzClone = new ClassOrValueTypeSignature( clazz.GenericArguments.Count )
            {
               IsClass = clazz.IsClass,
               Type = tableIndexTranslator == null ? clazz.Type : tableIndexTranslator( clazz.Type )
            };
            clazzClone.GenericArguments.AddRange( clazz.GenericArguments.Select( gArg => CloneTypeSignature( gArg, tableIndexTranslator ) ) );
            retVal = clazzClone;
            break;
         case TypeSignatureKind.ComplexArray:
            var cArray = (ComplexArrayTypeSignature) sig;
            var cClone = new ComplexArrayTypeSignature( cArray.Sizes.Count, cArray.LowerBounds.Count )
            {
               Rank = cArray.Rank,
               ArrayType = CloneTypeSignature( cArray.ArrayType, tableIndexTranslator )
            };
            cClone.LowerBounds.AddRange( cArray.LowerBounds );
            cClone.Sizes.AddRange( cArray.Sizes );
            retVal = cClone;
            break;
         case TypeSignatureKind.FunctionPointer:
            retVal = new FunctionPointerTypeSignature()
            {
               MethodSignature = CloneMethodRefSignature( ( (FunctionPointerTypeSignature) sig ).MethodSignature, tableIndexTranslator )
            };
            break;
         case TypeSignatureKind.Pointer:
            var ptr = (PointerTypeSignature) sig;
            var ptrClone = new PointerTypeSignature( ptr.CustomModifiers.Count )
            {
               PointerType = CloneTypeSignature( ptr.PointerType, tableIndexTranslator )
            };
            ptrClone.CustomModifiers.AddRange( ptr.CustomModifiers.CloneCustomMods( tableIndexTranslator ) );
            retVal = ptrClone;
            break;
         case TypeSignatureKind.GenericParameter:
         case TypeSignatureKind.Simple:
            retVal = sig;
            break;
         case TypeSignatureKind.SimpleArray:
            var array = (SimpleArrayTypeSignature) sig;
            var clone = new SimpleArrayTypeSignature( array.CustomModifiers.Count )
            {
               ArrayType = CloneTypeSignature( array.ArrayType, tableIndexTranslator )
            };
            clone.CustomModifiers.AddRange( array.CustomModifiers.CloneCustomMods( tableIndexTranslator ) );
            retVal = clone;
            break;
         default:
            throw new NotSupportedException( "Invalid type signature kind: " + sig.TypeSignatureKind );
      }

      return retVal;
   }

   private static void PopulateAbstractMethodSignature( AbstractMethodSignature original, AbstractMethodSignature clone, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      clone.GenericArgumentCount = original.GenericArgumentCount;
      clone.SignatureStarter = original.SignatureStarter;
      clone.ReturnType = CloneParameterSignature( original.ReturnType, tableIndexTranslator );
      clone.Parameters.AddRange( original.Parameters.Select( p => CloneParameterSignature( p, tableIndexTranslator ) ) );
   }

   private static MethodReferenceSignature CloneMethodRefSignature( MethodReferenceSignature methodRef, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var retVal = new MethodReferenceSignature( methodRef.Parameters.Count, methodRef.VarArgsParameters.Count );
      PopulateAbstractMethodSignature( methodRef, retVal, tableIndexTranslator );
      retVal.VarArgsParameters.AddRange( methodRef.VarArgsParameters.Select( p => CloneParameterSignature( p, tableIndexTranslator ) ) );
      return retVal;
   }

   private static MethodDefinitionSignature CloneMethodDefSignature( MethodDefinitionSignature methodDef, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var retVal = new MethodDefinitionSignature( methodDef.Parameters.Count );
      PopulateAbstractMethodSignature( methodDef, retVal, tableIndexTranslator );
      return retVal;
   }

   private static ParameterSignature CloneParameterSignature( ParameterSignature paramSig, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var retVal = new ParameterSignature( paramSig.CustomModifiers.Count )
      {
         IsByRef = paramSig.IsByRef,
         Type = CloneTypeSignature( paramSig.Type, tableIndexTranslator )
      };
      retVal.CustomModifiers.AddRange( paramSig.CustomModifiers.CloneCustomMods( tableIndexTranslator ) );
      return retVal;
   }

   private static GenericMethodSignature CloneGenericMethodSignature( GenericMethodSignature gSig, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var retVal = new GenericMethodSignature( gSig.GenericArguments.Count );
      retVal.GenericArguments.AddRange( gSig.GenericArguments.Select( gArg => CloneTypeSignature( gArg, tableIndexTranslator ) ) );
      return retVal;
   }

   private static FieldSignature CloneFieldSignature( FieldSignature sig, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var retVal = new FieldSignature( sig.CustomModifiers.Count );
      retVal.Type = CloneTypeSignature( sig.Type, tableIndexTranslator );
      retVal.CustomModifiers.AddRange( sig.CustomModifiers.CloneCustomMods( tableIndexTranslator ) );
      return retVal;
   }

   private static LocalVariablesSignature CloneLocalsSignature( LocalVariablesSignature locals, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var retVal = new LocalVariablesSignature( locals.Locals.Count );
      retVal.Locals.AddRange( locals.Locals.Select( l => CloneLocalSignature( l, tableIndexTranslator ) ) );
      return retVal;
   }

   private static LocalVariableSignature CloneLocalSignature( LocalVariableSignature local, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var retVal = new LocalVariableSignature( local.CustomModifiers.Count )
      {
         IsByRef = local.IsByRef,
         IsPinned = local.IsPinned,
         Type = CloneTypeSignature( local.Type, tableIndexTranslator )
      };
      retVal.CustomModifiers.AddRange( local.CustomModifiers.CloneCustomMods( tableIndexTranslator ) );
      return retVal;
   }

   private static PropertySignature ClonePropertySignature( PropertySignature sig, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var retVal = new PropertySignature( sig.CustomModifiers.Count, sig.Parameters.Count )
      {
         HasThis = sig.HasThis,
         PropertyType = CloneTypeSignature( sig.PropertyType, tableIndexTranslator )
      };
      retVal.CustomModifiers.AddRange( sig.CustomModifiers.CloneCustomMods( tableIndexTranslator ) );
      retVal.Parameters.AddRange( sig.Parameters.Select( p => CloneParameterSignature( p, tableIndexTranslator ) ) );
      return retVal;
   }

   private static IEnumerable<CustomModifierSignature> CloneCustomMods( this List<CustomModifierSignature> original, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      foreach ( var cm in original )
      {
         yield return new CustomModifierSignature()
         {
            IsOptional = cm.IsOptional,
            CustomModifierType = tableIndexTranslator == null ? cm.CustomModifierType : tableIndexTranslator( cm.CustomModifierType )
         };
      }
   }

   internal static CustomAttributeArgumentType ResolveCACtorType( this CILMetaData md, TypeSignature type, Func<TableIndex, CustomAttributeArgumentType> enumFactory )
   {
      return md.ResolveCACtorType( type, enumFactory, true );
   }


   private static CustomAttributeArgumentType ResolveCACtorType( this CILMetaData md, TypeSignature type, Func<TableIndex, CustomAttributeArgumentType> enumFactory, Boolean acceptArray )
   {
      switch ( type.TypeSignatureKind )
      {
         case TypeSignatureKind.Simple:
            switch ( ( (SimpleTypeSignature) type ).SimpleType )
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
               default:
                  return null;
            }
         case TypeSignatureKind.ClassOrValue:
            var clazz = (ClassOrValueTypeSignature) type;
            if ( clazz.GenericArguments.Count <= 0 )
            {
               if ( clazz.IsClass )
               {
                  // Either type or System.Object or System.Type are allowed here
                  if ( md.IsTypeType( clazz.Type ) )
                  {
                     return CustomAttributeArgumentTypeSimple.Type;
                  }
                  else if ( md.IsSystemObjectType( clazz.Type ) )
                  {
                     return CustomAttributeArgumentTypeSimple.Object;
                  }
                  else
                  {
                     return null;
                  }
               }
               else
               {
                  return enumFactory( clazz.Type );
               }
            }
            else
            {
               return null;
            }
         case TypeSignatureKind.SimpleArray:
            var retVal = acceptArray ?
               new CustomAttributeArgumentTypeArray()
               {
                  ArrayType = md.ResolveCACtorType( ( (SimpleArrayTypeSignature) type ).ArrayType, enumFactory, false )
               } :
               null;
            return retVal == null || retVal.ArrayType == null ? null : retVal;
         default:
            return null;
      }
   }

   public static Type GetNativeTypeForCAArrayType( this CustomAttributeArgumentType elemType )
   {
      switch ( elemType.ArgumentTypeKind )
      {
         case CustomAttributeArgumentTypeKind.Array:
            // Shouldn't be possible...
            return null;
         case CustomAttributeArgumentTypeKind.Simple:
            switch ( ( (CustomAttributeArgumentTypeSimple) elemType ).SimpleType )
            {
               case SignatureElementTypes.Boolean:
                  return typeof( Boolean );
               case SignatureElementTypes.Char:
                  return typeof( Char );
               case SignatureElementTypes.I1:
                  return typeof( SByte );
               case SignatureElementTypes.U1:
                  return typeof( Byte );
               case SignatureElementTypes.I2:
                  return typeof( Int16 );
               case SignatureElementTypes.U2:
                  return typeof( UInt16 );
               case SignatureElementTypes.I4:
                  return typeof( Int32 );
               case SignatureElementTypes.U4:
                  return typeof( UInt32 );
               case SignatureElementTypes.I8:
                  return typeof( Int64 );
               case SignatureElementTypes.U8:
                  return typeof( UInt64 );
               case SignatureElementTypes.R4:
                  return typeof( Single );
               case SignatureElementTypes.R8:
                  return typeof( Double );
               case SignatureElementTypes.String:
                  return typeof( String );
               case SignatureElementTypes.Type:
                  return typeof( CustomAttributeValue_TypeReference );
               case SignatureElementTypes.Object:
                  return typeof( Object );
               default:
                  return null;
            }
         case CustomAttributeArgumentTypeKind.TypeString:
            return typeof( CustomAttributeValue_EnumReference );
         default:
            return null;
      }
   }

   //internal static Boolean TryReadSigElementType( this Byte[] array, ref Int32 idx, out SignatureElementTypes sig )
   //{
   //   var retVal = idx < array.Length;
   //   sig = retVal ? (SignatureElementTypes) array[idx++] : SignatureElementTypes.End;
   //   return retVal;
   //}

   //internal static Boolean TryReadSigStarter( this Byte[] array, ref Int32 idx, out SignatureStarters sig )
   //{
   //   var retVal = idx < array.Length;
   //   sig = retVal ? (SignatureStarters) array[idx++] : (SignatureStarters) 0;
   //   return retVal;
   //}

   //internal static Boolean TryReadUnmanagedType( this Byte[] array, ref Int32 idx, out UnmanagedType ut )
   //{
   //   var retVal = idx < array.Length;
   //   ut = retVal ? (UnmanagedType) array[idx++] : (UnmanagedType) 0;
   //   return retVal;
   //}

   //internal static Boolean TryReadByte( this Byte[] array, ref Int32 idx, out Byte b )
   //{
   //   var retVal = idx < array.Length;
   //   b = retVal ? array[idx++] : (Byte) 0;
   //   return retVal;
   //}

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

   public static AbstractMarshalingInfo CreateDeepCopy( this AbstractMarshalingInfo marshal )
   {
      AbstractMarshalingInfo retVal;
      if ( marshal == null )
      {
         retVal = null;
      }
      else
      {
         var mKind = marshal.MarshalingInfoKind;
         switch ( mKind )
         {
            case MarshalingInfoKind.Simple:
               retVal = new SimpleMarshalingInfo()
               {
                  Value = marshal.Value
               };
               break;
            case MarshalingInfoKind.FixedLengthString:
               retVal = new FixedLengthStringMarshalingInfo()
               {
                  Value = marshal.Value,
                  Size = ( (FixedLengthStringMarshalingInfo) marshal ).Size
               };
               break;
            case MarshalingInfoKind.FixedLengthArray:
               var flArray = (FixedLengthArrayMarshalingInfo) marshal;
               retVal = new FixedLengthArrayMarshalingInfo()
               {
                  Value = marshal.Value,
                  Size = flArray.Size,
                  ElementType = flArray.ElementType
               };
               break;
            case MarshalingInfoKind.SafeArray:
               var safeArray = (SafeArrayMarshalingInfo) marshal;
               retVal = new SafeArrayMarshalingInfo()
               {
                  Value = marshal.Value,
                  ElementType = safeArray.ElementType,
                  UserDefinedType = safeArray.UserDefinedType
               };
               break;
            case MarshalingInfoKind.Array:
               var array = (ArrayMarshalingInfo) marshal;
               retVal = new ArrayMarshalingInfo()
               {
                  Value = marshal.Value,
                  ElementType = array.ElementType,
                  SizeParameterIndex = array.SizeParameterIndex,
                  Size = array.Size,
                  Flags = array.Flags
               };
               break;
            case MarshalingInfoKind.Interface:
               retVal = new InterfaceMarshalingInfo()
               {
                  Value = marshal.Value,
                  IIDParameterIndex = ( (InterfaceMarshalingInfo) marshal ).IIDParameterIndex
               };
               break;
            case MarshalingInfoKind.Custom:
               var custom = (CustomMarshalingInfo) marshal;
               retVal = new CustomMarshalingInfo()
               {
                  Value = marshal.Value,
                  GUIDString = custom.GUIDString,
                  NativeTypeName = custom.NativeTypeName,
                  CustomMarshalerTypeName = custom.CustomMarshalerTypeName,
                  MarshalCookie = custom.MarshalCookie
               };
               break;
            case MarshalingInfoKind.Raw:
               retVal = new RawMarshalingInfo()
               {
                  Value = marshal.Value,
                  Bytes = ( (RawMarshalingInfo) marshal ).Bytes.CreateArrayCopy()
               };
               break;
            default:
               throw new InvalidOperationException( "Unrecognized marshal kind: " + mKind + "." );
         }
      }

      return retVal;
   }
}