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

   public abstract class AbstractMethodSignature : AbstractSignature
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

      protected static void ReadFromBytes(
         Byte[] sig,
         ref Int32 idx,
         out SignatureStarters elementType,
         out Int32 genericCount,
         out ParameterSignature returnParameter,
         out ParameterSignature[] parameters,
         out Int32 sentinelMark
         )
      {
         elementType = (SignatureStarters) sig.ReadByteFromBytes( ref idx );
         genericCount = 0;
         if ( elementType.IsGeneric() )
         {
            genericCount = sig.DecompressUInt32( ref idx );
         }

         var amountOfParams = sig.DecompressUInt32( ref idx );
         returnParameter = ReadParameter( sig, ref idx );
         sentinelMark = -1;
         if ( amountOfParams > 0 )
         {
            parameters = new ParameterSignature[amountOfParams];
            for ( var i = 0; i < amountOfParams; ++i )
            {
               if ( sig[idx] == (Byte) SignatureElementTypes.Sentinel )
               {
                  sentinelMark = i;
               }
               parameters[i] = ReadParameter( sig, ref idx );
            }
         }
         else
         {
            parameters = Empty<ParameterSignature>.Array;
         }

      }

      internal static ParameterSignature ReadParameter( Byte[] sig, ref Int32 idx )
      {
         var retVal = new ParameterSignature();
         CustomModifierSignature.AddFromBytes( sig, ref idx, retVal.CustomModifiers );
         var elementType = (SignatureElementTypes) sig.ReadByteFromBytes( ref idx );
         TypeSignature type;
         if ( elementType == SignatureElementTypes.TypedByRef )
         {
            type = SimpleTypeSignature.TypedByRef;
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
            type = TypeSignature.ReadFromBytesWithRef( sig, ref idx );
         }
         retVal.Type = type;
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

      public static MethodDefinitionSignature ReadFromBytes( Byte[] sig, Int32 idx )
      {
         return ReadFromBytesWithRef( sig, ref idx );
      }

      public static MethodDefinitionSignature ReadFromBytesWithRef( Byte[] sig, ref Int32 idx )
      {
         SignatureStarters elementType;
         Int32 genericCount;
         ParameterSignature returnParameter;
         ParameterSignature[] parameters;
         Int32 sentinelMark;
         ReadFromBytes( sig, ref idx, out elementType, out genericCount, out returnParameter, out parameters, out sentinelMark );
         var retVal = new MethodDefinitionSignature( parameters.Length )
         {
            GenericArgumentCount = genericCount,
            ReturnType = returnParameter,
            SignatureStarter = elementType,
         };
         foreach ( var p in parameters )
         {
            retVal.Parameters.Add( p );
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

      public static MethodReferenceSignature ReadFromBytes( Byte[] sig, ref Int32 idx )
      {
         SignatureStarters elementType;
         Int32 genericCount;
         ParameterSignature returnParameter;
         ParameterSignature[] parameters;
         Int32 sentinelMark;
         ReadFromBytes( sig, ref idx, out elementType, out genericCount, out returnParameter, out parameters, out sentinelMark );
         var pLength = sentinelMark == -1 ? parameters.Length : sentinelMark;
         var vLength = sentinelMark == -1 ? 0 : ( parameters.Length - sentinelMark );
         var retVal = new MethodReferenceSignature( pLength, vLength )
         {
            GenericArgumentCount = genericCount,
            ReturnType = returnParameter,
            SignatureStarter = elementType,
         };
         for ( var i = 0; i < pLength; ++i )
         {
            retVal.Parameters.Add( parameters[i] );
         }
         for ( var i = 0; i < vLength; ++i )
         {
            retVal.VarArgsParameters.Add( parameters[i + pLength] );
         }
         return retVal;
      }
   }

   public abstract class AbstractSignatureWithCustomMods : AbstractSignature
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

      public static FieldSignature ReadFromBytes( Byte[] sig, Int32 idx )
      {
         return ReadFromBytesWithRef( sig, ref idx );
      }

      public static FieldSignature ReadFromBytesWithRef( Byte[] sig, ref Int32 idx )
      {
         var starter = (SignatureStarters) sig.ReadByteFromBytes( ref idx );
         FieldSignature retVal;
         if ( starter == SignatureStarters.Field )
         {
            retVal = new FieldSignature();
            CustomModifierSignature.AddFromBytes( sig, ref idx, retVal.CustomModifiers );
            retVal.Type = TypeSignature.ReadFromBytesWithRef( sig, ref idx );
         }
         else
         {
            retVal = null;
         }
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

      public static PropertySignature ReadFromBytes( Byte[] sig, Int32 idx )
      {
         return ReadFromBytesWithRef( sig, ref idx );
      }

      public static PropertySignature ReadFromBytesWithRef( Byte[] sig, ref Int32 idx )
      {
         var starter = (SignatureStarters) sig.ReadByteFromBytes( ref idx );
         PropertySignature retVal;
         if ( starter.IsProperty() )
         {
            var paramCount = sig.DecompressUInt32( ref idx );
            retVal = new PropertySignature( parameterCount: paramCount );
            CustomModifierSignature.AddFromBytes( sig, ref idx, retVal.CustomModifiers );
            retVal.PropertyType = TypeSignature.ReadFromBytesWithRef( sig, ref idx );
            for ( var i = 0; i < paramCount; ++i )
            {
               retVal.Parameters.Add( AbstractMethodSignature.ReadParameter( sig, ref idx ) );
            }
         }
         else
         {
            retVal = null;
         }

         return retVal;
      }
   }

   public sealed class LocalVariablesSignature : AbstractSignature
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

      public static LocalVariablesSignature ReadFromBytes( Byte[] sig, ref Int32 idx )
      {
         var starter = (SignatureStarters) sig.ReadByteFromBytes( ref idx );
         LocalVariablesSignature retVal;
         if ( starter == SignatureStarters.LocalSignature )
         {
            var lCount = sig.DecompressUInt32( ref idx );
            retVal = new LocalVariablesSignature( lCount );
            for ( var i = 0; i < lCount; ++i )
            {
               var local = new LocalVariableSignature();
               var elementType = (SignatureElementTypes) sig[idx];
               if ( elementType == SignatureElementTypes.TypedByRef )
               {
                  local.Type = SimpleTypeSignature.TypedByRef;
                  ++idx; // Mark this byte read
               }
               else
               {
                  CustomModifierSignature.AddFromBytes( sig, ref idx, local.CustomModifiers );
                  elementType = (SignatureElementTypes) sig[idx];
                  if ( elementType == SignatureElementTypes.Pinned )
                  {
                     local.IsPinned = true;
                     ++idx;
                     elementType = (SignatureElementTypes) sig[idx];
                  }
                  if ( elementType == SignatureElementTypes.ByRef )
                  {
                     local.IsByRef = true;
                     ++idx;
                  }
                  local.Type = TypeSignature.ReadFromBytesWithRef( sig, ref idx );
               }
               retVal.Locals.Add( local );
            }
         }
         else
         {
            retVal = null;
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

      public static CustomModifierSignature ReadFromBytes( Byte[] sig, ref Int32 idx )
      {
         var curByte = sig[idx];
         CustomModifierSignature retVal;
         if ( curByte == (Byte) SignatureElementTypes.CModOpt || curByte == (Byte) SignatureElementTypes.CModReqd )
         {
            ++idx;
            retVal = new CustomModifierSignature()
            {
               CustomModifierType = new TableIndex( TokenUtils.DecodeTypeDefOrRefOrSpec( sig, ref idx ) ),
               IsOptional = curByte == (Byte) SignatureElementTypes.CModOpt
            };
         }
         else
         {
            retVal = null;
         }
         return retVal;
      }

      public static void AddFromBytes( Byte[] sig, ref Int32 idx, IList<CustomModifierSignature> customMods )
      {
         CustomModifierSignature curMod;
         do
         {
            curMod = ReadFromBytes( sig, ref idx );
            if ( curMod != null )
            {
               customMods.Add( curMod );
            }
         } while ( curMod != null );
      }
   }

   public abstract class TypeSignature : AbstractSignature
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

      public static TypeSignature ReadFromBytes( Byte[] sig, Int32 idx )
      {
         return ReadFromBytesWithRef( sig, ref idx );
      }

      public static TypeSignature ReadFromBytesWithRef( Byte[] sig, ref Int32 idx )
      {
         Int32 auxiliary;
         var elementType = (SignatureElementTypes) sig[idx++];
         switch ( elementType )
         {
            case SignatureElementTypes.End:
               return null;
            case SignatureElementTypes.Boolean:
               return SimpleTypeSignature.Boolean;
            case SignatureElementTypes.Char:
               return SimpleTypeSignature.Char;
            case SignatureElementTypes.I1:
               return SimpleTypeSignature.SByte;
            case SignatureElementTypes.U1:
               return SimpleTypeSignature.Byte;
            case SignatureElementTypes.I2:
               return SimpleTypeSignature.Int16;
            case SignatureElementTypes.U2:
               return SimpleTypeSignature.UInt16;
            case SignatureElementTypes.I4:
               return SimpleTypeSignature.Int32;
            case SignatureElementTypes.U4:
               return SimpleTypeSignature.UInt32;
            case SignatureElementTypes.I8:
               return SimpleTypeSignature.Int64;
            case SignatureElementTypes.U8:
               return SimpleTypeSignature.UInt64;
            case SignatureElementTypes.R4:
               return SimpleTypeSignature.Single;
            case SignatureElementTypes.R8:
               return SimpleTypeSignature.Double;
            case SignatureElementTypes.I:
               return SimpleTypeSignature.IntPtr;
            case SignatureElementTypes.U:
               return SimpleTypeSignature.UIntPtr;
            case SignatureElementTypes.String:
               return SimpleTypeSignature.String;
            case SignatureElementTypes.Object:
               return SimpleTypeSignature.Object;
            case SignatureElementTypes.Void:
               return SimpleTypeSignature.Void;
            case SignatureElementTypes.Array:
               var arrayType = ReadFromBytesWithRef( sig, ref idx );
               var arraySig = ReadArrayInfo( sig, ref idx );
               arraySig.ArrayType = arrayType;
               return arraySig;
            case SignatureElementTypes.Class:
            case SignatureElementTypes.ValueType:
            case SignatureElementTypes.GenericInst:
               var isGeneric = elementType == SignatureElementTypes.GenericInst;
               TableIndex actualType;
               if ( isGeneric )
               {
                  elementType = (SignatureElementTypes) sig[idx++];
               }
               actualType = new TableIndex( TokenUtils.DecodeTypeDefOrRefOrSpec( sig, ref idx ) );
               auxiliary = isGeneric ? sig.DecompressUInt32( ref idx ) : 0;
               var classOrValue = new ClassOrValueTypeSignature( auxiliary )
               {
                  IsClass = elementType == SignatureElementTypes.Class,
                  Type = actualType
               };
               if ( auxiliary > 0 )
               {
                  for ( var i = 0; i < auxiliary; ++i )
                  {
                     classOrValue.GenericArguments.Add( ReadFromBytesWithRef( sig, ref idx ) );
                  }
               }
               return classOrValue;
            case SignatureElementTypes.FnPtr:
               return new FunctionPointerTypeSignature()
               {
                  MethodSignature = MethodReferenceSignature.ReadFromBytes( sig, ref idx )
               };
            case SignatureElementTypes.MVar:
            case SignatureElementTypes.Var:
               return new GenericParameterTypeSignature()
               {
                  GenericParameterIndex = sig.DecompressUInt32( ref idx ),
                  IsTypeParameter = elementType == SignatureElementTypes.Var
               };
            case SignatureElementTypes.Ptr:
               var ptr = new PointerTypeSignature();
               CustomModifierSignature.AddFromBytes( sig, ref idx, ptr.CustomModifiers );
               ptr.PointerType = ReadFromBytesWithRef( sig, ref idx );
               return ptr;
            case SignatureElementTypes.SzArray:
               var szArr = new SimpleArrayTypeSignature();
               CustomModifierSignature.AddFromBytes( sig, ref idx, szArr.CustomModifiers );
               szArr.ArrayType = ReadFromBytesWithRef( sig, ref idx );
               return szArr;
            default:
               return null;
         }

      }

      private static ComplexArrayTypeSignature ReadArrayInfo( Byte[] sig, ref Int32 idx )
      {
         var rank = sig.DecompressUInt32( ref idx );
         var curSize = sig.DecompressUInt32( ref idx );
         var retVal = new ComplexArrayTypeSignature( sizesCount: curSize ); // TODO skip thru sizes and detect lower bound count
         retVal.Rank = rank;
         while ( curSize > 0 )
         {
            retVal.Sizes.Add( sig.DecompressUInt32( ref idx ) );
            --curSize;
         }
         curSize = sig.DecompressUInt32( ref idx );
         var loBounds = curSize > 0 ?
            new Int32[curSize] :
            null;
         while ( curSize > 0 )
         {
            retVal.LowerBounds.Add( sig.DecompressInt32( ref idx ) );
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

   public sealed class GenericMethodSignature : AbstractSignature
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

      public static GenericMethodSignature ReadFromBytes( Byte[] sig, Int32 idx )
      {
         return ReadFromBytesWithRef( sig, ref idx );
      }

      public static GenericMethodSignature ReadFromBytesWithRef( Byte[] sig, ref Int32 idx )
      {
         var elementType = (SignatureStarters) sig.ReadByteFromBytes( ref idx );
         GenericMethodSignature retVal;
         if ( elementType == SignatureStarters.MethodSpecGenericInst )
         {

            var genericArgumentsCount = sig.DecompressUInt32( ref idx );
            retVal = new GenericMethodSignature( genericArgumentsCount );
            for ( var i = 0; i < genericArgumentsCount; ++i )
            {
               retVal.GenericArguments.Add( TypeSignature.ReadFromBytesWithRef( sig, ref idx ) );
            }
         }
         else
         {
            retVal = null;
         }
         return retVal;
      }
   }

   /// <summary>
   /// This class represents marshalling information used for CIL parameters and fields.
   /// The instances of this class are created via static methods in this class.
   /// </summary>
   /// <seealso cref="CILElementWithMarshalingInfo"/>
   /// <seealso cref="CILField"/>
   /// <seealso cref="CILParameter"/>
   public sealed class MarshalingInfo
   {
      internal const UnmanagedType NATIVE_TYPE_MAX = (UnmanagedType) 0x50;
      internal const Int32 NO_INDEX = -1;

      private readonly UnmanagedType _ut;

      private readonly VarEnum _safeArrayType;
      private readonly String _safeArrayUserDefinedType;
      private readonly Int32 _iidParameterIndex;
      private readonly UnmanagedType _arrayType;
      private readonly Int32 _sizeParamIndex;
      private readonly Int32 _constSize;
      private readonly String _marshalType;
      private readonly String _marshalCookie;

      /// <summary>
      /// Creates a new instance of <see cref="MarshalingInfo"/> with all values as specified.
      /// </summary>
      /// <param name="ut">The <see cref="UnmanagedType"/>.</param>
      /// <param name="safeArrayType">The <see cref="VarEnum"/> of safe array elements.</param>
      /// <param name="safeArrayUDType">The user-defined type of safe array elements.</param>
      /// <param name="iidParamIdx">The zero-based index of <c>iid_is</c> parameter in COM interop.</param>
      /// <param name="arrayType">The <see cref="UnmanagedType"/> of array elements.</param>
      /// <param name="sizeParamIdx">The zero-based index of array size parameter.</param>
      /// <param name="constSize">The size of additional array elements.</param>
      /// <param name="marshalType">The type name of custom marshaler.</param>
      /// <param name="marshalCookie">The cookie for custom marshaler.</param>
      public MarshalingInfo(
         UnmanagedType ut,
         VarEnum safeArrayType,
         String safeArrayUDType,
         Int32 iidParamIdx,
         UnmanagedType arrayType,
         Int32 sizeParamIdx,
         Int32 constSize,
         String marshalType,
         String marshalCookie
         )
      {
         this._ut = ut;
         this._safeArrayType = safeArrayType;
         this._safeArrayUserDefinedType = safeArrayUDType;
         this._iidParameterIndex = Math.Max( NO_INDEX, iidParamIdx );
         this._arrayType = arrayType == (UnmanagedType) 0 ? NATIVE_TYPE_MAX : arrayType;
         this._sizeParamIndex = Math.Max( NO_INDEX, sizeParamIdx );
         this._constSize = Math.Max( NO_INDEX, constSize );
         this._marshalType = marshalType;
         this._marshalCookie = marshalCookie;
      }

      /// <summary>
      /// Gets the <see cref="UnmanagedType" /> value the data is to be marshaled as.
      /// </summary>
      /// <value>The <see cref="UnmanagedType" /> value the data is to be marshaled as.</value>
      public UnmanagedType Value
      {
         get
         {
            return this._ut;
         }
      }

      /// <summary>
      /// Gets the element type for <see cref="UnmanagedType.SafeArray"/>.
      /// </summary>
      /// <value>The element type for <see cref="UnmanagedType.SafeArray"/>.</value>
      public VarEnum SafeArrayType
      {
         get
         {
            return this._safeArrayType;
         }
      }

      /// <summary>
      /// Gets the element type for <see cref="UnmanagedType.SafeArray"/> when <see cref="SafeArrayType"/> is <see cref="VarEnum.VT_USERDEFINED"/>.
      /// </summary>
      /// <value>The element type for <see cref="UnmanagedType.SafeArray"/> when <see cref="SafeArrayType"/> is <see cref="VarEnum.VT_USERDEFINED"/>.</value>
      public String SafeArrayUserDefinedType
      {
         get
         {
            return this._safeArrayUserDefinedType;
         }
      }

      /// <summary>
      /// Gets the zero-based parameter index of the unmanaged iid_is attribute used by COM.
      /// </summary>
      /// <value>The parameter index of the unmanaged iid_is attribute used by COM.</value>
      public Int32 IIDParameterIndex
      {
         get
         {
            return this._iidParameterIndex;
         }
      }

      /// <summary>
      /// Gets the type of the array for <see cref="UnmanagedType.ByValArray"/> or <see cref="UnmanagedType.LPArray"/>.
      /// </summary>
      /// <value>The type of the array for <see cref="UnmanagedType.ByValArray"/> or <see cref="UnmanagedType.LPArray"/>.</value>
      public UnmanagedType ArrayType
      {
         get
         {
            return this._arrayType;
         }
      }

      /// <summary>
      /// Gets the zero-based index of the parameter containing the count of the array elements.
      /// </summary>
      /// <value>The zero-based index of the parameter containing the count of the array elements.</value>
      public Int32 SizeParameterIndex
      {
         get
         {
            return this._sizeParamIndex;
         }
      }

      /// <summary>
      /// Gets the number of elements of fixed-length array or the number of character (not bytes) in a string.
      /// </summary>
      /// <value>The number of elements of fixed-length array or the number of character (not bytes) in a string.</value>
      public Int32 ConstSize
      {
         get
         {
            return this._constSize;
         }
      }

      /// <summary>
      /// Gets the fully-qualified name of a custom marshaler.
      /// </summary>
      /// <value>The fully-qualified name of a custom marshaler.</value>
      public String MarshalType
      {
         get
         {
            return this._marshalType;
         }
      }

      /// <summary>
      /// Gets the additional information for custom marshaler.
      /// </summary>
      /// <value>The additional information for custom marshaler.</value>
      public String MarshalCookie
      {
         get
         {
            return this._marshalCookie;
         }
      }

      /// <summary>
      /// Marshals field or parameter as native instric type.
      /// </summary>
      /// <param name="ut">The native instric type.</param>
      /// <returns>A new <see cref="MarshalingInfo"/> with given information.</returns>
      /// <exception cref="ArgumentException">If <see cref="E_CIL.IsNativeInstric(UnmanagedType)"/> returns <c>false</c> for <paramref name="ut"/>.</exception>
      public static MarshalingInfo MarshalAs( UnmanagedType ut )
      {
         if ( ut.IsNativeInstric() )
         {
            return new MarshalingInfo( ut, VarEnum.VT_EMPTY, null, NO_INDEX, NATIVE_TYPE_MAX, NO_INDEX, NO_INDEX, null, null );
         }
         else
         {
            throw new ArgumentException( "This method may be used only on native instrict unmanaged types." );
         }
      }

      /// <summary>
      /// Marshals field or parameter as <see cref="UnmanagedType.ByValTStr"/>.
      /// </summary>
      /// <param name="size">The size of the string.</param>
      /// <returns>A new <see cref="MarshalingInfo"/> with given information.</returns>
      /// <exception cref="ArgumentOutOfRangeException">If <paramref name="size"/> is less than zero.</exception>
      public static MarshalingInfo MarshalAsByValTStr( Int32 size )
      {
         if ( size < 0 )
         {
            throw new ArgumentOutOfRangeException( "The size for in-line character array must be at least zero." );
         }
         return new MarshalingInfo( UnmanagedType.ByValTStr, VarEnum.VT_EMPTY, null, NO_INDEX, NATIVE_TYPE_MAX, NO_INDEX, size, null, null );
      }

      /// <summary>
      /// Marshals field or parameter as <see cref="UnmanagedType.IUnknown"/>.
      /// </summary>
      /// <param name="iidParamIndex">The zero-based index for <c>iid_is</c> parameter.</param>
      /// <returns>A new <see cref="MarshalingInfo"/> with given information.</returns>
      public static MarshalingInfo MarshalAsIUnknown( Int32 iidParamIndex = NO_INDEX )
      {
         return MarshalAsIUnknownOrIDispatch( true, iidParamIndex );
      }

      /// <summary>
      /// Marshals field or parameter as <see cref="UnmanagedType.IDispatch"/>.
      /// </summary>
      /// <param name="iidParamIndex">The zero-based index for <c>iid_is</c> parameter.</param>
      /// <returns>A new <see cref="MarshalingInfo"/> with given information.</returns>
      public static MarshalingInfo MarshalAsIDispatch( Int32 iidParamIndex = NO_INDEX )
      {
         return MarshalAsIUnknownOrIDispatch( false, iidParamIndex );
      }

      private static MarshalingInfo MarshalAsIUnknownOrIDispatch( Boolean unknown, Int32 iidParamIndex )
      {
         return new MarshalingInfo( unknown ? UnmanagedType.IUnknown : UnmanagedType.IDispatch, VarEnum.VT_EMPTY, null, iidParamIndex, NATIVE_TYPE_MAX, NO_INDEX, NO_INDEX, null, null );
      }

      /// <summary>
      /// Marshals field or parameter as <see cref="UnmanagedType.SafeArray"/> without any further information.
      /// </summary>
      /// <returns>A new <see cref="MarshalingInfo"/> with given information.</returns>
      public static MarshalingInfo MarshalAsSafeArray()
      {
         return MarshalAsSafeArray( VarEnum.VT_EMPTY, null );
      }

      /// <summary>
      /// Marshals field or parameter as <see cref="UnmanagedType.SafeArray"/> with specified array element type.
      /// </summary>
      /// <param name="elementType">The type of array elements.</param>
      /// <returns>A new <see cref="MarshalingInfo"/> with given information.</returns>
      /// <exception cref="ArgumentException">If <paramref name="elementType"/> is <see cref="VarEnum.VT_USERDEFINED"/>. Use <see cref="MarshalAsSafeArray(CILType)"/> method to specify safe arrays with user-defined types.</exception>
      /// <seealso cref="VarEnum"/>
      public static MarshalingInfo MarshalAsSafeArray( VarEnum elementType )
      {
         if ( VarEnum.VT_USERDEFINED == elementType )
         {
            throw new ArgumentException( "Use other method for userdefined safe array types." );
         }
         return MarshalAsSafeArray( elementType, null );
      }

      /// <summary>
      /// Marshals field or parameter as <see cref="UnmanagedType.SafeArray"/> with user-defined array element type.
      /// </summary>
      /// <param name="elementType">The type of array elements. May be <c>null</c>, then no type information is included.</param>
      /// <returns>A new <see cref="MarshalingInfo"/> with given information.</returns>
      public static MarshalingInfo MarshalAsSafeArray( String elementType )
      {
         return MarshalAsSafeArray( elementType == null ? VarEnum.VT_EMPTY : VarEnum.VT_USERDEFINED, elementType );
      }

      private static MarshalingInfo MarshalAsSafeArray( VarEnum elementType, String udType )
      {
         return new MarshalingInfo( UnmanagedType.SafeArray, elementType, udType, NO_INDEX, NATIVE_TYPE_MAX, NO_INDEX, NO_INDEX, null, null );
      }

      /// <summary>
      /// Marshals field or parameter as <see cref="UnmanagedType.ByValArray"/>.
      /// </summary>
      /// <param name="size">The size of the array.</param>
      /// <param name="elementType">The optional type information about array elements.</param>
      /// <returns>A new <see cref="MarshalingInfo"/> with given information.</returns>
      /// <exception cref="ArgumentOutOfRangeException">If <paramref name="size"/> is less than zero.</exception>
      public static MarshalingInfo MarshalAsByValArray( Int32 size, UnmanagedType elementType = NATIVE_TYPE_MAX )
      {
         if ( size < 0 )
         {
            throw new ArgumentOutOfRangeException( "The size for by-val array must be at least zero." );
         }
         return new MarshalingInfo( UnmanagedType.ByValArray, VarEnum.VT_EMPTY, null, NO_INDEX, elementType, NO_INDEX, size, null, null );
      }

      /// <summary>
      /// Marshals field or parameter as <see cref="UnmanagedType.LPArray"/>.
      /// </summary>
      /// <param name="sizeParamIdx">The zero-based index for parameter containing array size.</param>
      /// <param name="constSize">The size of additional elements.</param>
      /// <param name="elementType">The optional type information about array elements.</param>
      /// <returns>A new <see cref="MarshalingInfo"/> with given information.</returns>
      public static MarshalingInfo MarshalAsLPArray( Int32 sizeParamIdx = NO_INDEX, Int32 constSize = 0, UnmanagedType elementType = NATIVE_TYPE_MAX )
      {
         return new MarshalingInfo( UnmanagedType.LPArray, VarEnum.VT_EMPTY, null, NO_INDEX, elementType, sizeParamIdx, constSize, null, null );
      }

      /// <summary>
      /// Marshals field or parameter using custom marshalling.
      /// </summary>
      /// <param name="customMarshalerTypeName">The fully qualified type name of the custom marshaler.Must implement <see cref="T:System.Runtime.InteropServices.ICustomMarshaler"/>.</param>
      /// <param name="marshalCookie">The string information to pass for marshaler creation function.</param>
      /// <returns>A new <see cref="MarshalingInfo"/> with given information.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="customMarshalerTypeName"/> is <c>null</c>.</exception>
      /// <seealso href="http://msdn.microsoft.com/en-us/library/system.runtime.interopservices.icustommarshaler.aspx"/>
      public static MarshalingInfo MarshalAsCustom( String customMarshalerTypeName, String marshalCookie = null )
      {
         ArgumentValidator.ValidateNotNull( "Custom marshaler typename", customMarshalerTypeName );
         return new MarshalingInfo( UnmanagedType.CustomMarshaler, VarEnum.VT_EMPTY, null, NO_INDEX, NATIVE_TYPE_MAX, NO_INDEX, NO_INDEX, customMarshalerTypeName, marshalCookie );
      }

      public static MarshalingInfo ReadFromBytes( Byte[] sig, Int32 idx )
      {
         return ReadFromBytesWithRef( sig, ref idx );
      }

      public static MarshalingInfo ReadFromBytesWithRef( Byte[] sig, ref Int32 idx )
      {
         var sIdx = 0;
         var ut = (UnmanagedType) sig[sIdx++];
         MarshalingInfo result;
         if ( ut.IsNativeInstric() )
         {
            result = MarshalingInfo.MarshalAs( ut );
         }
         else
         {
            Int32 constSize, paramIdx;
            UnmanagedType arrElementType;
            switch ( ut )
            {
               case UnmanagedType.ByValTStr:
                  result = MarshalingInfo.MarshalAsByValTStr( sig.DecompressUInt32( ref sIdx ) );
                  break;
               case UnmanagedType.IUnknown:
                  result = MarshalingInfo.MarshalAsIUnknown( sIdx < sig.Length ? sig.DecompressUInt32( ref sIdx ) : MarshalingInfo.NO_INDEX );
                  break;
               case UnmanagedType.IDispatch:
                  result = MarshalingInfo.MarshalAsIDispatch( sIdx < sig.Length ? sig.DecompressUInt32( ref sIdx ) : MarshalingInfo.NO_INDEX );
                  break;
               case UnmanagedType.SafeArray:
                  if ( sIdx < sig.Length )
                  {
                     var ve = (VarEnum) sig.DecompressUInt32( ref sIdx );
                     if ( VarEnum.VT_USERDEFINED == ve )
                     {
                        if ( sIdx < sig.Length )
                        {
                           result = MarshalingInfo.MarshalAsSafeArray( sig.ReadLenPrefixedUTF8String( ref sIdx ) );
                        }
                        else
                        {
                           // Fallback in erroneus blob - just plain safe array
                           result = MarshalingInfo.MarshalAsSafeArray();
                        }
                     }
                     else
                     {
                        result = MarshalingInfo.MarshalAsSafeArray( ve );
                     }
                  }
                  else
                  {
                     result = MarshalingInfo.MarshalAsSafeArray();
                  }
                  break;
               case UnmanagedType.ByValArray:
                  constSize = sig.DecompressUInt32( ref sIdx );
                  result = MarshalingInfo.MarshalAsByValArray(
                     constSize,
                     sIdx < sig.Length ?
                        (UnmanagedType) sig.DecompressUInt32( ref sIdx ) :
                        MarshalingInfo.NATIVE_TYPE_MAX );
                  break;
               case UnmanagedType.LPArray:
                  arrElementType = (UnmanagedType) sig[sIdx++];
                  paramIdx = MarshalingInfo.NO_INDEX;
                  constSize = MarshalingInfo.NO_INDEX;
                  if ( sIdx < sig.Length )
                  {
                     paramIdx = sig.DecompressUInt32( ref sIdx );
                     if ( sIdx < sig.Length )
                     {
                        constSize = sig.DecompressUInt32( ref sIdx );
                        if ( sIdx < sig.Length && sig.DecompressUInt32( ref sIdx ) == 0 )
                        {
                           paramIdx = MarshalingInfo.NO_INDEX; // No size parameter index was specified
                        }
                     }
                  }
                  result = MarshalingInfo.MarshalAsLPArray( paramIdx, constSize, arrElementType );
                  break;
               case UnmanagedType.CustomMarshaler:
                  // For some reason, there are two compressed ints at this point
                  sig.DecompressUInt32( ref sIdx );
                  sig.DecompressUInt32( ref sIdx );

                  var mTypeStr = sig.ReadLenPrefixedUTF8String( ref sIdx );
                  var mCookie = sig.ReadLenPrefixedUTF8String( ref sIdx );
                  result = MarshalingInfo.MarshalAsCustom( mTypeStr, mCookie );
                  break;
               default:
                  result = null;
                  break;
            }
         }
         return result;
      }

   }

   public abstract class AbstractCustomAttributeSignature
   {
      // Disable inheritance to other assemblies
      internal AbstractCustomAttributeSignature()
      {

      }
   }

   public sealed class RawCustomAttributeSignature : AbstractCustomAttributeSignature
   {
      public Byte[] Bytes { get; set; }
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

   public sealed class CustomAttributeTypedArgument
   {
      // Note: enums will be deserialized as their underlying enum types
      public Object Value { get; set; }
      public CustomAttributeArgumentType Type { get; set; }
   }

   public sealed class CustomAttributeNamedArgument
   {
      public CustomAttributeTypedArgument Value { get; set; }
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

   public sealed class CustomAttributeArgumentSimple : CustomAttributeArgumentType
   {
      public static readonly CustomAttributeArgumentSimple Boolean = new CustomAttributeArgumentSimple( SignatureElementTypes.Boolean );
      public static readonly CustomAttributeArgumentSimple Char = new CustomAttributeArgumentSimple( SignatureElementTypes.Char );
      public static readonly CustomAttributeArgumentSimple SByte = new CustomAttributeArgumentSimple( SignatureElementTypes.I1 );
      public static readonly CustomAttributeArgumentSimple Byte = new CustomAttributeArgumentSimple( SignatureElementTypes.U1 );
      public static readonly CustomAttributeArgumentSimple Int16 = new CustomAttributeArgumentSimple( SignatureElementTypes.I2 );
      public static readonly CustomAttributeArgumentSimple UInt16 = new CustomAttributeArgumentSimple( SignatureElementTypes.U2 );
      public static readonly CustomAttributeArgumentSimple Int32 = new CustomAttributeArgumentSimple( SignatureElementTypes.I4 );
      public static readonly CustomAttributeArgumentSimple UInt32 = new CustomAttributeArgumentSimple( SignatureElementTypes.U4 );
      public static readonly CustomAttributeArgumentSimple Int64 = new CustomAttributeArgumentSimple( SignatureElementTypes.I8 );
      public static readonly CustomAttributeArgumentSimple UInt64 = new CustomAttributeArgumentSimple( SignatureElementTypes.U8 );
      public static readonly CustomAttributeArgumentSimple Single = new CustomAttributeArgumentSimple( SignatureElementTypes.R4 );
      public static readonly CustomAttributeArgumentSimple Double = new CustomAttributeArgumentSimple( SignatureElementTypes.R8 );
      public static readonly CustomAttributeArgumentSimple String = new CustomAttributeArgumentSimple( SignatureElementTypes.String );
      public static readonly CustomAttributeArgumentSimple Type = new CustomAttributeArgumentSimple( SignatureElementTypes.Type );
      internal static readonly CustomAttributeArgumentSimple Object = new CustomAttributeArgumentSimple( SignatureElementTypes.Object );

      private SignatureElementTypes _kind;

      private CustomAttributeArgumentSimple( SignatureElementTypes kind )
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
      // Disable inheritance to other assemblies
      internal AbstractSecurityInformation()
      {

      }

      /// <summary>
      /// Gets or sets the type of the security attribute.
      /// </summary>
      /// <value>The type of the security attribute.</value>
      public String SecurityAttributeType { get; set; }
   }

   public sealed class RawSecurityInformation : AbstractSecurityInformation
   {
      public Int32 ArgumentCount { get; set; }
      public Byte[] Bytes { get; set; }
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

      internal SecurityInformation( Int32 namedArgumentsCount = 0 )
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
   }
}

public static partial class E_CILPhysical
{
   public static TSignature CreateDeepCopy<TSignature>( this TSignature sig )
      where TSignature : AbstractSignature
   {
      switch ( sig.SignatureKind )
      {
         case SignatureKind.Field:
            return CloneFieldSignature( sig as FieldSignature ) as TSignature;
         case SignatureKind.GenericMethodInstantiation:
            return CloneGenericMethodSignature( sig as GenericMethodSignature ) as TSignature;
         case SignatureKind.LocalVariables:
            return CloneLocalsSignature( sig as LocalVariablesSignature ) as TSignature;
         case SignatureKind.MethodDefinition:
            return CloneMethodDefSignature( sig as MethodDefinitionSignature ) as TSignature;
         case SignatureKind.MethodReference:
            return CloneMethodRefSignature( sig as MethodReferenceSignature ) as TSignature;
         case SignatureKind.Type:
            return CloneTypeSignature( sig as TypeSignature ) as TSignature;
         default:
            throw new NotSupportedException( "Invalid signature kind: " + sig.SignatureKind + "." );
      }
   }

   private static TypeSignature CloneTypeSignature( TypeSignature sig )
   {
      TypeSignature retVal;
      switch ( sig.TypeSignatureKind )
      {
         case TypeSignatureKind.ClassOrValue:
            var clazz = (ClassOrValueTypeSignature) sig;
            var clazzClone = new ClassOrValueTypeSignature( clazz.GenericArguments.Count )
            {
               IsClass = clazz.IsClass,
               Type = clazz.Type
            };
            clazzClone.GenericArguments.AddRange( clazz.GenericArguments.Select( gArg => CloneTypeSignature( gArg ) ) );
            retVal = clazzClone;
            break;
         case TypeSignatureKind.ComplexArray:
            var cArray = (ComplexArrayTypeSignature) sig;
            var cClone = new ComplexArrayTypeSignature( cArray.Sizes.Count, cArray.LowerBounds.Count )
            {
               Rank = cArray.Rank,
               ArrayType = CloneTypeSignature( cArray.ArrayType )
            };
            cClone.LowerBounds.AddRange( cArray.LowerBounds );
            cClone.Sizes.AddRange( cArray.Sizes );
            retVal = cClone;
            break;
         case TypeSignatureKind.FunctionPointer:
            retVal = new FunctionPointerTypeSignature()
            {
               MethodSignature = CloneMethodRefSignature( ( (FunctionPointerTypeSignature) sig ).MethodSignature )
            };
            break;
         case TypeSignatureKind.Pointer:
            var ptr = (PointerTypeSignature) sig;
            var ptrClone = new PointerTypeSignature( ptr.CustomModifiers.Count )
            {
               PointerType = CloneTypeSignature( ptr.PointerType )
            };
            ptrClone.CustomModifiers.AddRange( ptr.CustomModifiers );
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
               ArrayType = CloneTypeSignature( array.ArrayType )
            };
            clone.CustomModifiers.AddRange( array.CustomModifiers );
            retVal = clone;
            break;
         default:
            throw new NotSupportedException( "Invalid type signature kind: " + sig.TypeSignatureKind );
      }

      return retVal;
   }

   private static void PopulateAbstractMethodSignature( AbstractMethodSignature original, AbstractMethodSignature clone )
   {
      clone.GenericArgumentCount = original.GenericArgumentCount;
      clone.SignatureStarter = original.SignatureStarter;
      clone.ReturnType = CloneParameterSignature( original.ReturnType );
      clone.Parameters.AddRange( original.Parameters.Select( p => CloneParameterSignature( p ) ) );
   }

   private static MethodReferenceSignature CloneMethodRefSignature( MethodReferenceSignature methodRef )
   {
      var retVal = new MethodReferenceSignature( methodRef.Parameters.Count, methodRef.VarArgsParameters.Count );
      PopulateAbstractMethodSignature( methodRef, retVal );
      retVal.VarArgsParameters.AddRange( methodRef.VarArgsParameters.Select( p => CloneParameterSignature( p ) ) );
      return retVal;
   }

   private static MethodDefinitionSignature CloneMethodDefSignature( MethodDefinitionSignature methodDef )
   {
      var retVal = new MethodDefinitionSignature( methodDef.Parameters.Count );
      PopulateAbstractMethodSignature( methodDef, retVal );
      return retVal;
   }

   private static ParameterSignature CloneParameterSignature( ParameterSignature paramSig )
   {
      var retVal = new ParameterSignature( paramSig.CustomModifiers.Count )
      {
         IsByRef = paramSig.IsByRef,
         Type = CloneTypeSignature( paramSig.Type )
      };
      retVal.CustomModifiers.AddRange( paramSig.CustomModifiers );
      return retVal;
   }

   private static GenericMethodSignature CloneGenericMethodSignature( GenericMethodSignature gSig )
   {
      var retVal = new GenericMethodSignature( gSig.GenericArguments.Count );
      retVal.GenericArguments.AddRange( gSig.GenericArguments.Select( gArg => CloneTypeSignature( gArg ) ) );
      return retVal;
   }

   private static FieldSignature CloneFieldSignature( FieldSignature sig )
   {
      var retVal = new FieldSignature( sig.CustomModifiers.Count );
      retVal.Type = CloneTypeSignature( sig.Type );
      retVal.CustomModifiers.AddRange( sig.CustomModifiers );
      return retVal;
   }

   private static LocalVariablesSignature CloneLocalsSignature( LocalVariablesSignature locals )
   {
      var retVal = new LocalVariablesSignature( locals.Locals.Count );
      retVal.Locals.AddRange( locals.Locals.Select( l => CloneLocalSignature( l ) ) );
      return retVal;
   }

   private static LocalVariableSignature CloneLocalSignature( LocalVariableSignature local )
   {
      var retVal = new LocalVariableSignature( local.CustomModifiers.Count )
      {
         IsByRef = local.IsByRef,
         IsPinned = local.IsPinned,
         Type = CloneTypeSignature( local.Type )
      };
      retVal.CustomModifiers.AddRange( local.CustomModifiers );
      return retVal;
   }
}