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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
   public sealed class ModuleDefinition
   {
      public Int16 Generation { get; set; }
      public String Name { get; set; }
      public Guid? ModuleGUID { get; set; }
      public Guid? EditAndContinueGUID { get; set; }
      public Guid? EditAndContinueBaseGUID { get; set; }
   }

   public sealed class TypeReference
   {
      public TableIndex? ResolutionScope { get; set; }
      public String Name { get; set; }
      public String Namespace { get; set; }
   }

   public sealed class TypeDefinition
   {
      public TypeAttributes Attributes { get; set; }
      public String Name { get; set; }
      public String Namespace { get; set; }
      public TableIndex? BaseType { get; set; }
      public TableIndex FieldList { get; set; }
      public TableIndex MethodList { get; set; }
   }

   public sealed class FieldDefinition
   {
      public FieldAttributes Attributes { get; set; }
      public String Name { get; set; }
      public Byte[] Signature { get; set; }
   }

   public sealed class MethodDefinition
   {
      public MethodILDefinition IL { get; set; }
      public MethodImplAttributes ImplementationAttributes { get; set; }
      public MethodAttributes Attributes { get; set; }
      public String Name { get; set; }
      public Byte[] Signature { get; set; }
      public TableIndex ParameterList { get; set; }
   }

   public sealed class MethodILDefinition
   {

   }

   public sealed class ParameterDefinition
   {
      public ParameterAttributes Attributes { get; set; }
      public Int16 Sequence { get; set; }
      public String Name { get; set; }
   }

   public sealed class InterfaceImplementation
   {
      public TableIndex Class { get; set; }
      public TableIndex Interface { get; set; }
   }

   public sealed class MemberReference
   {
      public TableIndex DeclaringType { get; set; }
      public String Name { get; set; }
      public Byte[] Signature { get; set; }
   }

   public sealed class ConstantDefinition
   {
      public SignatureElementTypes Type { get; set; }
      public TableIndex Parent { get; set; }
      public Byte[] Value { get; set; }
   }

   public sealed class CustomAttributeDefinition
   {
      public TableIndex Parent { get; set; }
      public TableIndex Type { get; set; }
      public Byte[] Value { get; set; }
   }
   public sealed class FieldMarshal
   {
      public TableIndex Parent { get; set; }
      public Byte[] NativeType { get; set; }
   }

   public sealed class SecurityDefinition
   {
      public Int16 Action { get; set; }
      public TableIndex Parent { get; set; }
      public Byte[] PermissionSet { get; set; }
   }

   public sealed class ClassLayout
   {
      public Int16 PackingSize { get; set; }
      public Int32 ClassSize { get; set; }
      public TableIndex Parent { get; set; }
   }

   public sealed class FieldLayout
   {
      public Int32 Offset { get; set; }
      public TableIndex Field { get; set; }
   }

   public sealed class StandaloneSignature
   {
      public Byte[] Signature { get; set; }
   }

   public sealed class EventMap
   {
      public TableIndex Parent { get; set; }
      public TableIndex EventList { get; set; }
   }

   public sealed class EventDefinition
   {
      public EventAttributes Attributes { get; set; }
      public String Name { get; set; }
      public TableIndex EventType { get; set; }
   }

   public sealed class PropertyMap
   {
      public TableIndex Parent { get; set; }
      public TableIndex PropertyList { get; set; }
   }

   public sealed class PropertyDefinition
   {
      public PropertyAttributes Attributes { get; set; }
      public String Name { get; set; }
      public Byte[] Signature { get; set; }
   }

   public sealed class MethodSemantics
   {
      public MethodSemanticsAttributes Attributes { get; set; }
      public TableIndex Method { get; set; }
      public TableIndex Associaton { get; set; }
   }

   public sealed class MethodImplementation
   {
      public TableIndex Class { get; set; }
      public TableIndex MethodBody { get; set; }
      public TableIndex MethodDeclaration { get; set; }
   }

   public sealed class ModuleReference
   {
      public String ModuleRefeference { get; set; }
   }

   public sealed class TypeSpecification
   {
      public Byte[] Signature { get; set; }
   }

   public sealed class MethodImplementationMap
   {
      public PInvokeAttributes Attributes { get; set; }
      public TableIndex MemberForwarded { get; set; }
      public String ImportName { get; set; }
      public TableIndex ImportScope { get; set; }
   }

   public sealed class FieldRVA
   {
      public Byte[] Data { get; set; }
      public TableIndex Field { get; set; }
   }

   public sealed class AssemblyDefinition
   {
      public AssemblyHashAlgorithm HashAlgorithm { get; set; }
      public Int32 VersionMajor { get; set; }
      public Int32 VersionMinor { get; set; }
      public Int32 VersionBuild { get; set; }
      public Int32 VersionRevision { get; set; }
      public AssemblyFlags Attributes { get; set; }
      public Byte[] PublicKey { get; set; }
      public String Name { get; set; }
      public String Culture { get; set; }
   }

   public sealed class AssemblyReference
   {
      public Int32 VersionMajor { get; set; }
      public Int32 VersionMinor { get; set; }
      public Int32 VersionBuild { get; set; }
      public Int32 VersionRevision { get; set; }
      public AssemblyFlags Attributes { get; set; }
      public Byte[] PublicKeyOrToken { get; set; }
      public String Name { get; set; }
      public String Culture { get; set; }
      public Byte[] HashValue { get; set; }
   }

   public sealed class FileReference
   {
      FileAttributes Attributes { get; set; }
      String Name { get; set; }
      Byte[] HashValue { get; set; }
   }

   public sealed class ExportedTypes
   {
      TypeAttributes Attributes { get; set; }
      Int32 TypeDefinitionIndex { get; set; }
      String Name { get; set; }
      String Namespace { get; set; }
      TableIndex Implementation { get; set; }
   }

   public sealed class ManifestResource
   {
      Int64 Offset { get; set; }
      ManifestResourceAttributes Attributes { get; set; }
      String Name { get; set; }
      TableIndex? Implementation { get; set; }
   }

   public sealed class NestedClassDefinition
   {
      TableIndex NestedClass { get; set; }
      TableIndex EnclosingClass { get; set; }
   }

   public sealed class GenericParameterDefinition
   {
      Int16 GenericParameterIndex { get; set; }
      GenericParameterAttributes Attributes { get; set; }
      TableIndex Owner { get; set; }
      String Name { get; set; }
   }

   public sealed class MethodSpecification
   {
      TableIndex Method { get; set; }
      Byte[] Signature { get; set; }
   }

   public sealed class GenericParameterConstraintDefinition
   {
      TableIndex Owner { get; set; }
      TableIndex Constraint { get; set; }
   }

   public struct TableIndex
   {
      internal readonly Tables table;
      internal readonly Int32 idx; // Zero-based
      internal TableIndex( Tables aTable, Int32 anIdx )
      {
         if ( anIdx < 0 )
         {
            throw new BadImageFormatException( "Simple index to table " + aTable + " was null." );
         }
         this.table = aTable;
         this.idx = anIdx;
      }

      internal TableIndex( Int32 token )
      {
         TokenUtils.DecodeTokenZeroBased( token, out this.table, out this.idx );
         if ( idx < 0 )
         {
            throw new BadImageFormatException( "Token had zero as index (" + this + ")." );
         }
      }

      public override string ToString()
      {
         return this.table + "[" + this.idx + "]";
      }
   }

   public enum Tables : byte
   {
      Assembly = 0x20,
      AssemblyOS = 0x22,
      AssemblyProcessor = 0x21,
      AssemblyRef = 0x23,
      AssemblyRefOS = 0x25,
      AssemblyRefProcessor = 0x24,
      ClassLayout = 0x0F,
      Constant = 0x0B,
      CustomAttribute = 0x0C,
      DeclSecurity = 0x0E,
      EncLog = 0x1E,
      EncMap = 0x1F,
      EventMap = 0x12,
      Event = 0x14,
      EventPtr = 0x13,
      ExportedType = 0x27,
      Field = 0x04,
      FieldLayout = 0x10,
      FieldMarshal = 0x0D,
      FieldPtr = 0x03,
      FieldRVA = 0x1D,
      File = 0x26,
      GenericParameter = 0x2A,
      GenericParameterConstraint = 0x2C,
      ImplMap = 0x1C,
      InterfaceImpl = 0x09,
      ManifestResource = 0x28,
      MemberRef = 0x0A,
      MethodDef = 0x06,
      MethodImpl = 0x19,
      MethodPtr = 0x05,
      MethodSemantics = 0x18,
      MethodSpec = 0x2B,
      Module = 0x00,
      ModuleRef = 0x1A,
      NestedClass = 0x29,
      Parameter = 0x08,
      ParameterPtr = 0x07,
      Property = 0x17,
      PropertyPtr = 0x16,
      PropertyMap = 0x15,
      StandaloneSignature = 0x11,
      TypeDef = 0x02,
      TypeRef = 0x01,
      TypeSpec = 0x1B
   }

   public enum SignatureKind
   {
      MethodDefinition,
      MethodReference,
      Field,
      Property,
      LocalVariable,
      Type
   }

   public abstract class AbstractSignature
   {
      public abstract SignatureKind SignatureKind { get; }
   }

   public abstract class AbstractMethodSignature : AbstractSignature
   {
      private readonly IList<ParameterSignature> _parameters;
      public AbstractMethodSignature( Int32 parameterCount = 0 )
      {
         this._parameters = new List<ParameterSignature>( parameterCount );
      }

      public SignatureStarters SignatureStarter { get; set; }
      public Boolean GenericArgumentCount { get; set; }
      public ParameterSignature ReturnType { get; set; }
      public IList<ParameterSignature> Parameters
      {
         get
         {
            return this._parameters;
         }
      }
   }

   public sealed class MethodDefinitionSignature : AbstractMethodSignature
   {

      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.MethodDefinition;
         }
      }
   }

   public sealed class MethodReferenceSignature : AbstractMethodSignature
   {
      private readonly IList<ParameterSignature> _varArgsParameters;

      public MethodReferenceSignature( Int32 parameterCount = 0, Int32 varArgsParameterCount = 0 )
         : base( parameterCount )
      {
         this._varArgsParameters = new List<ParameterSignature>( varArgsParameterCount );
      }

      public IList<ParameterSignature> VarArgsParameters
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
   }

   public abstract class AbstractSignatureWithCustomMods : AbstractSignature
   {
      private readonly IList<CustomModifierSignature> _customMods;

      protected AbstractSignatureWithCustomMods( Int32 customModCount )
      {
         this._customMods = new List<CustomModifierSignature>( customModCount );
      }

      public IList<CustomModifierSignature> CustomModifiers
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
   }

   public sealed class PropertySignature : AbstractSignatureWithCustomMods
   {
      private readonly IList<ParameterSignature> _parameters;

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
      public IList<ParameterSignature> Parameters
      {
         get
         {
            return this._parameters;
         }
      }
   }

   public sealed class LocalVariablesSignature : AbstractSignature
   {
      private readonly IList<LocalVariableSignature> _locals;

      public LocalVariablesSignature( Int32 localsCount = 0 )
      {
         this._locals = new List<LocalVariableSignature>();
      }

      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.LocalVariable;
         }
      }

      public IList<LocalVariableSignature> Locals
      {
         get
         {
            return this._locals;
         }
      }
   }

   public sealed class LocalVariableSignature : ParameterOrLocalVariableSignature
   {
      public LocalVariableSignature( Int32 customModCount = 0 )
         : base( customModCount )
      {

      }

      public Boolean IsPinned { get; set; }
   }

   public abstract class ParameterOrLocalVariableSignature
   {
      private readonly IList<CustomModifierSignature> _customMods;

      protected ParameterOrLocalVariableSignature( Int32 customModCount )
      {
         this._customMods = new List<CustomModifierSignature>( customModCount );
      }

      public IList<CustomModifierSignature> CustomModifiers
      {
         get
         {
            return this._customMods;
         }
      }

      public Boolean IsByRef { get; set; }
      public TypeSignature Type { get; set; }
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
   }

   public abstract class TypeSignature : AbstractSignature
   {


      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.Type;
         }
      }

      public abstract TypeSignatureKind TypeSignatureKind { get; }
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
      public static readonly SimpleTypeSignature UIn64 = new SimpleTypeSignature( SignatureElementTypes.U8 );
      public static readonly SimpleTypeSignature Single = new SimpleTypeSignature( SignatureElementTypes.R4 );
      public static readonly SimpleTypeSignature Double = new SimpleTypeSignature( SignatureElementTypes.R8 );
      public static readonly SimpleTypeSignature IntPtr = new SimpleTypeSignature( SignatureElementTypes.I );
      public static readonly SimpleTypeSignature UIntPtr = new SimpleTypeSignature( SignatureElementTypes.U );
      public static readonly SimpleTypeSignature Object = new SimpleTypeSignature( SignatureElementTypes.Object );
      public static readonly SimpleTypeSignature String = new SimpleTypeSignature( SignatureElementTypes.String );
      public static readonly SimpleTypeSignature Void = new SimpleTypeSignature( SignatureElementTypes.Void );

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
   }

   public sealed class ClassOrValueTypeSignature : TypeSignature
   {
      private readonly IList<TypeSignature> _genericArguments;

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
      public IList<TypeSignature> GenericArguments
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

      public AbstractMethodSignature MethodSignature { get; set; }
   }

   public sealed class PointerTypeSignature : TypeSignature
   {
      private readonly IList<CustomModifierSignature> _customMods;

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

      public IList<CustomModifierSignature> CustomModifiers
      {
         get
         {
            return this._customMods;
         }
      }

      public TypeSignature Type { get; set; }
   }

   public abstract class AbstractArrayTypeSignature : TypeSignature
   {
      public TypeSignature ArrayType { get; set; }
   }

   public sealed class ComplexArrayTypeSignature : AbstractArrayTypeSignature
   {
      private readonly IList<Int32> _sizes;
      private readonly IList<Int32> _lowerBounds;

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
      public IList<Int32> Sizes
      {
         get
         {
            return this._sizes;
         }
      }
      public IList<Int32> LowerBounds
      {
         get
         {
            return this._lowerBounds;
         }
      }
   }

   public sealed class SimpleArrayTypeSignature : AbstractArrayTypeSignature
   {
      private readonly IList<CustomModifierSignature> _customMods;

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

      public IList<CustomModifierSignature> CustomModifiers
      {
         get
         {
            return this._customMods;
         }
      }
   }
}

public static partial class E_CILPhysical
{
   public static Boolean IsHasThis( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.HasThis ) != 0;
   }

   public static Boolean IsExplicitThis( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.ExplicitThis ) != 0;
   }

   public static Boolean IsDefault( this SignatureStarters starter )
   {
      return starter == 0;
   }

   public static Boolean IsVarArg( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.VarArgs ) != 0;
   }

   public static Boolean IsGeneric( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.Generic ) != 0;
   }
}
