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
using CILAssemblyManipulator.Physical;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Structural
{
   public enum StructureSignatureKind
   {
      MethodDefinition,
      MethodReference,
      Field,
      Property,
      LocalVariables,
      Type,
      GenericMethodInstantiation
   }

   public abstract class AbstractStructureSignature
   {
      // Disable inheritance to other assemblies
      internal AbstractStructureSignature()
      {

      }

      public abstract StructureSignatureKind SignatureKind { get; }
   }

   public abstract class AbstractMethodStructureSignature : AbstractStructureSignature
   {
      private readonly List<ParameterStructureSignature> _parameters;

      // Disable inheritance to other assemblies
      internal AbstractMethodStructureSignature( Int32 parameterCount )
      {
         this._parameters = new List<ParameterStructureSignature>( parameterCount );
      }

      public SignatureStarters SignatureStarter { get; set; }
      public Int32 GenericArgumentCount { get; set; }
      public ParameterStructureSignature ReturnType { get; set; }
      public List<ParameterStructureSignature> Parameters
      {
         get
         {
            return this._parameters;
         }
      }
   }


   public sealed class MethodDefinitionStructureSignature : AbstractMethodStructureSignature
   {
      public MethodDefinitionStructureSignature( Int32 parameterCount = 0 )
         : base( parameterCount )
      {
      }

      public override StructureSignatureKind SignatureKind
      {
         get
         {
            return StructureSignatureKind.MethodDefinition;
         }
      }
   }

   public sealed class MethodReferenceStructureSignature : AbstractMethodStructureSignature
   {
      private readonly List<ParameterStructureSignature> _varArgsParameters;

      public MethodReferenceStructureSignature( Int32 parameterCount = 0, Int32 varArgsParameterCount = 0 )
         : base( parameterCount )
      {
         this._varArgsParameters = new List<ParameterStructureSignature>( varArgsParameterCount );
      }

      public List<ParameterStructureSignature> VarArgsParameters
      {
         get
         {
            return this._varArgsParameters;
         }
      }

      public override StructureSignatureKind SignatureKind
      {
         get
         {
            return StructureSignatureKind.MethodReference;
         }
      }
   }

   public abstract class AbstractStructureSignatureWithCustomMods : AbstractStructureSignature
   {
      private readonly List<CustomModifierStructureSignature> _customMods;

      // Disable inheritance to other assemblies
      internal AbstractStructureSignatureWithCustomMods( Int32 customModCount )
      {
         this._customMods = new List<CustomModifierStructureSignature>( customModCount );
      }

      public List<CustomModifierStructureSignature> CustomModifiers
      {
         get
         {
            return this._customMods;
         }
      }
   }

   public sealed class FieldStructureSignature : AbstractStructureSignatureWithCustomMods
   {
      public FieldStructureSignature( Int32 customModCount = 0 )
         : base( customModCount )
      {
      }

      public TypeStructureSignature Type { get; set; }

      public override StructureSignatureKind SignatureKind
      {
         get
         {
            return StructureSignatureKind.Field;
         }
      }
   }

   public sealed class PropertyStructureSignature : AbstractStructureSignatureWithCustomMods
   {
      private readonly List<ParameterStructureSignature> _parameters;

      public PropertyStructureSignature( Int32 customModCount = 0, Int32 parameterCount = 0 )
         : base( customModCount )
      {
         this._parameters = new List<ParameterStructureSignature>( parameterCount );
      }

      public override StructureSignatureKind SignatureKind
      {
         get
         {
            return StructureSignatureKind.Property;
         }
      }

      public Boolean HasThis { get; set; }
      public TypeStructureSignature PropertyType { get; set; }
      public List<ParameterStructureSignature> Parameters
      {
         get
         {
            return this._parameters;
         }
      }
   }

   public sealed class LocalVariablesStructureSignature : AbstractStructureSignature
   {
      private readonly List<LocalVariableStructureSignature> _locals;

      public LocalVariablesStructureSignature( Int32 localsCount = 0 )
      {
         this._locals = new List<LocalVariableStructureSignature>();
      }

      public override StructureSignatureKind SignatureKind
      {
         get
         {
            return StructureSignatureKind.LocalVariables;
         }
      }

      public List<LocalVariableStructureSignature> Locals
      {
         get
         {
            return this._locals;
         }
      }
   }

   public abstract class ParameterOrLocalVariableStructureSignature
   {
      private readonly List<CustomModifierStructureSignature> _customMods;

      // Disable inheritance to other assemblies
      internal ParameterOrLocalVariableStructureSignature( Int32 customModCount )
      {
         this._customMods = new List<CustomModifierStructureSignature>( customModCount );
      }

      public List<CustomModifierStructureSignature> CustomModifiers
      {
         get
         {
            return this._customMods;
         }
      }

      public Boolean IsByRef { get; set; }
      public TypeStructureSignature Type { get; set; }
   }

   public sealed class LocalVariableStructureSignature : ParameterOrLocalVariableStructureSignature
   {
      public LocalVariableStructureSignature( Int32 customModCount = 0 )
         : base( customModCount )
      {

      }

      public Boolean IsPinned { get; set; }
   }

   public sealed class ParameterStructureSignature : ParameterOrLocalVariableStructureSignature
   {
      public ParameterStructureSignature( Int32 customModCount = 0 )
         : base( customModCount )
      {

      }
   }

   public sealed class CustomModifierStructureSignature
   {
      public Boolean IsOptional { get; set; }
      public AbstractTypeDescription CustomModifierType { get; set; }
   }

   public abstract class TypeStructureSignature : AbstractStructureSignature
   {
      // Disable inheritance to other assemblies
      internal TypeStructureSignature()
      {

      }

      public override StructureSignatureKind SignatureKind
      {
         get
         {
            return StructureSignatureKind.Type;
         }
      }

      public abstract TypeStructureSignatureKind TypeSignatureKind { get; }
   }

   public enum TypeStructureSignatureKind : byte
   {
      Simple,
      ComplexArray,
      ClassOrValue,
      GenericParameter,
      FunctionPointer,
      Pointer,
      SimpleArray,
   }

   public sealed class SimpleTypeStructureSignature : TypeStructureSignature
   {
      public static readonly SimpleTypeStructureSignature Boolean = new SimpleTypeStructureSignature( SignatureElementTypes.Boolean );
      public static readonly SimpleTypeStructureSignature Char = new SimpleTypeStructureSignature( SignatureElementTypes.Char );
      public static readonly SimpleTypeStructureSignature SByte = new SimpleTypeStructureSignature( SignatureElementTypes.I1 );
      public static readonly SimpleTypeStructureSignature Byte = new SimpleTypeStructureSignature( SignatureElementTypes.U1 );
      public static readonly SimpleTypeStructureSignature Int16 = new SimpleTypeStructureSignature( SignatureElementTypes.I2 );
      public static readonly SimpleTypeStructureSignature UInt16 = new SimpleTypeStructureSignature( SignatureElementTypes.U2 );
      public static readonly SimpleTypeStructureSignature Int32 = new SimpleTypeStructureSignature( SignatureElementTypes.I4 );
      public static readonly SimpleTypeStructureSignature UInt32 = new SimpleTypeStructureSignature( SignatureElementTypes.U4 );
      public static readonly SimpleTypeStructureSignature Int64 = new SimpleTypeStructureSignature( SignatureElementTypes.I8 );
      public static readonly SimpleTypeStructureSignature UInt64 = new SimpleTypeStructureSignature( SignatureElementTypes.U8 );
      public static readonly SimpleTypeStructureSignature Single = new SimpleTypeStructureSignature( SignatureElementTypes.R4 );
      public static readonly SimpleTypeStructureSignature Double = new SimpleTypeStructureSignature( SignatureElementTypes.R8 );
      public static readonly SimpleTypeStructureSignature IntPtr = new SimpleTypeStructureSignature( SignatureElementTypes.I );
      public static readonly SimpleTypeStructureSignature UIntPtr = new SimpleTypeStructureSignature( SignatureElementTypes.U );
      public static readonly SimpleTypeStructureSignature Object = new SimpleTypeStructureSignature( SignatureElementTypes.Object );
      public static readonly SimpleTypeStructureSignature String = new SimpleTypeStructureSignature( SignatureElementTypes.String );
      public static readonly SimpleTypeStructureSignature Void = new SimpleTypeStructureSignature( SignatureElementTypes.Void );
      public static readonly SimpleTypeStructureSignature TypedByRef = new SimpleTypeStructureSignature( SignatureElementTypes.TypedByRef );

      private readonly SignatureElementTypes _type;

      private SimpleTypeStructureSignature( SignatureElementTypes type )
      {
         this._type = type;
      }

      public override TypeStructureSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeStructureSignatureKind.Simple;
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

   public sealed class ClassOrValueTypeStructureSignature : TypeStructureSignature
   {
      private readonly List<TypeStructureSignature> _genericArguments;

      public ClassOrValueTypeStructureSignature( Int32 genericArgumentsCount = 0 )
      {
         this._genericArguments = new List<TypeStructureSignature>( genericArgumentsCount );
      }

      public override TypeStructureSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeStructureSignatureKind.ClassOrValue;
         }
      }

      public Boolean IsClass { get; set; }
      public AbstractTypeDescription Type { get; set; }
      public List<TypeStructureSignature> GenericArguments
      {
         get
         {
            return this._genericArguments;
         }
      }
   }

   public sealed class GenericParameterTypeStructureSignature : TypeStructureSignature
   {

      public override TypeStructureSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeStructureSignatureKind.GenericParameter;
         }
      }

      public Boolean IsTypeParameter { get; set; }
      public Int32 GenericParameterIndex { get; set; }
   }

   public sealed class FunctionPointerTypeStructureSignature : TypeStructureSignature
   {

      public override TypeStructureSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeStructureSignatureKind.FunctionPointer;
         }
      }

      public MethodReferenceStructureSignature MethodSignature { get; set; }
   }

   public sealed class PointerTypeStructureSignature : TypeStructureSignature
   {
      private readonly List<CustomModifierStructureSignature> _customMods;

      public PointerTypeStructureSignature( Int32 customModCount = 0 )
      {
         this._customMods = new List<CustomModifierStructureSignature>( customModCount );
      }

      public override TypeStructureSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeStructureSignatureKind.Pointer;
         }
      }

      public List<CustomModifierStructureSignature> CustomModifiers
      {
         get
         {
            return this._customMods;
         }
      }

      public TypeStructureSignature PointerType { get; set; }
   }

   public abstract class AbstractArrayTypeStructureSignature : TypeStructureSignature
   {
      // Disable inheritance to other assemblies
      internal AbstractArrayTypeStructureSignature()
      {

      }

      public TypeStructureSignature ArrayType { get; set; }
   }

   public sealed class ComplexArrayTypeStructureSignature : AbstractArrayTypeStructureSignature
   {
      private readonly List<Int32> _sizes;
      private readonly List<Int32> _lowerBounds;

      public ComplexArrayTypeStructureSignature( Int32 sizesCount = 0, Int32 lowerBoundsCount = 0 )
      {
         this._sizes = new List<Int32>( sizesCount );
         this._lowerBounds = new List<Int32>( lowerBoundsCount );
      }

      public override TypeStructureSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeStructureSignatureKind.ComplexArray;
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

   public sealed class SimpleArrayTypeStructureSignature : AbstractArrayTypeStructureSignature
   {
      private readonly List<CustomModifierStructureSignature> _customMods;

      public SimpleArrayTypeStructureSignature( Int32 customModCount = 0 )
      {
         this._customMods = new List<CustomModifierStructureSignature>( customModCount );
      }

      public override TypeStructureSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeStructureSignatureKind.SimpleArray;
         }
      }

      public List<CustomModifierStructureSignature> CustomModifiers
      {
         get
         {
            return this._customMods;
         }
      }
   }

   public sealed class GenericMethodStructureSignature : AbstractStructureSignature
   {
      private readonly List<TypeStructureSignature> _genericArguments;

      public GenericMethodStructureSignature( Int32 genericArgumentsCount = 0 )
      {
         this._genericArguments = new List<TypeStructureSignature>( genericArgumentsCount );
      }

      public override StructureSignatureKind SignatureKind
      {
         get
         {
            return StructureSignatureKind.GenericMethodInstantiation;
         }
      }

      public List<TypeStructureSignature> GenericArguments
      {
         get
         {
            return this._genericArguments;
         }
      }
   }
}
