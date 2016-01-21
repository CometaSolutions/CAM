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
   /// <summary>
   /// This enumeration tells what type instance of <see cref="AbstractSignature"/> really is.
   /// </summary>
   public enum SignatureKind
   {
      /// <summary>
      /// The <see cref="AbstractSignature"/> is of type <see cref="MethodDefinitionSignature"/>.
      /// </summary>
      MethodDefinition,

      /// <summary>
      /// The <see cref="AbstractSignature"/> is of type <see cref="MethodReferenceSignature"/>.
      /// </summary>
      MethodReference,

      /// <summary>
      /// The <see cref="AbstractSignature"/> is of type <see cref="FieldSignature"/>.
      /// </summary>
      Field,

      /// <summary>
      /// The <see cref="AbstractSignature"/> is of type <see cref="PropertySignature"/>.
      /// </summary>
      Property,

      /// <summary>
      /// The <see cref="AbstractSignature"/> is of type <see cref="LocalVariablesSignature"/>.
      /// </summary>
      LocalVariables,

      /// <summary>
      /// The <see cref="AbstractSignature"/> is of type <see cref="TypeSignature"/>.
      /// Use the <see cref="TypeSignatureKind"/> enumeration returned by <see cref="TypeSignature.TypeSignatureKind"/> to find out the exact type of the <see cref="TypeSignature"/>.
      /// </summary>
      Type,

      /// <summary>
      /// The <see cref="AbstractSignature"/> is of type <see cref="GenericMethodSignature"/>.
      /// </summary>
      GenericMethodInstantiation,

      /// <summary>
      /// The <see cref="AbstractSignature"/> is of type <see cref="RawSignature"/>.
      /// </summary>
      Raw
   }

   /// <summary>
   /// This is base class for all signatures in CAM.Physical framework.
   /// </summary>
   /// <remarks>
   /// The instances of this class can not be directly instantiated.
   /// Instead, use one of the following for non-type signatures:
   /// <list type="bullet">
   /// <item><description><see cref="MethodDefinitionSignature"/>,</description></item>
   /// <item><description><see cref="MethodReferenceSignature"/>,</description></item>
   /// <item><description><see cref="FieldSignature"/>,</description></item>
   /// <item><description><see cref="PropertySignature"/>,</description></item>
   /// <item><description><see cref="LocalVariablesSignature"/>,</description></item>
   /// <item><description><see cref="GenericMethodSignature"/>, or</description></item>
   /// <item><description><see cref="RawSignature"/>.</description></item>
   /// </list>
   /// For type signatures, use one of the following:
   /// <list type="bullet">
   /// <item><description><see cref="SimpleTypeSignature"/>,</description></item>
   /// <item><description><see cref="ClassOrValueTypeSignature"/>,</description></item>
   /// <item><description><see cref="SimpleArrayTypeSignature"/>,</description></item>
   /// <item><description><see cref="GenericParameterTypeSignature"/>,</description></item>
   /// <item><description><see cref="ComplexArrayTypeSignature"/>,</description></item>
   /// <item><description><see cref="PointerTypeSignature"/>, or</description></item>
   /// <item><description><see cref="FunctionPointerTypeSignature"/>.</description></item>
   /// </list>
   /// </remarks>
   public abstract class AbstractSignature
   {
      // Disable inheritance to other assemblies
      internal AbstractSignature()
      {

      }

      /// <summary>
      /// Gets the <see cref="Physical.SignatureKind"/> enumeration descripting the actual type of this <see cref="AbstractSignature"/>.
      /// </summary>
      /// <value>The <see cref="Physical.SignatureKind"/> enumeration descripting the actual type of this <see cref="AbstractSignature"/>.</value>
      /// <seealso cref="Physical.SignatureKind"/>
      public abstract SignatureKind SignatureKind { get; }
   }

   /// <summary>
   /// This signature contains signature data as byte array.
   /// </summary>
   public sealed class RawSignature : AbstractSignature
   {
      /// <summary>
      /// Creates a new instance of <see cref="RawSignature"/>, with <see cref="Bytes"/> set to <c>null</c>.
      /// </summary>
      public RawSignature()
      {

      }

      /// <summary>
      /// Gets or sets the byte array acting as signature data for this <see cref="RawSignature"/>.
      /// </summary>
      /// <value>The byte array acting as signature data for this <see cref="RawSignature"/>.</value>
      public Byte[] Bytes { get; set; }

      /// <summary>
      /// Returns the <see cref="SignatureKind.Raw"/>.
      /// </summary>
      /// <value>The <see cref="SignatureKind.Raw"/>.</value>
      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.Raw;
         }
      }
   }

   /// <summary>
   /// This is base class for all signatures that are not <see cref="RawSignature"/>.
   /// It exposes extra data alongside actual signature when (de)serializing.
   /// </summary>
   /// <remarks>
   /// Instances of this class can not be instantiated directly.
   /// Instead, use one of the following for non-type signatures:
   /// <list type="bullet">
   /// <item><description><see cref="MethodDefinitionSignature"/>,</description></item>
   /// <item><description><see cref="MethodReferenceSignature"/>,</description></item>
   /// <item><description><see cref="FieldSignature"/>,</description></item>
   /// <item><description><see cref="PropertySignature"/>,</description></item>
   /// <item><description><see cref="LocalVariablesSignature"/>,</description></item>
   /// <item><description><see cref="GenericMethodSignature"/>, or</description></item>
   /// </list>
   /// For type signatures, use one of the following:
   /// <list type="bullet">
   /// <item><description><see cref="SimpleTypeSignature"/>,</description></item>
   /// <item><description><see cref="ClassOrValueTypeSignature"/>,</description></item>
   /// <item><description><see cref="SimpleArrayTypeSignature"/>,</description></item>
   /// <item><description><see cref="GenericParameterTypeSignature"/>,</description></item>
   /// <item><description><see cref="ComplexArrayTypeSignature"/>,</description></item>
   /// <item><description><see cref="PointerTypeSignature"/>, or</description></item>
   /// <item><description><see cref="FunctionPointerTypeSignature"/>.</description></item>
   /// </list>
   /// </remarks>
   public abstract class AbstractNotRawSignature : AbstractSignature
   {
      /// <summary>
      /// Gets or sets the extra data for this <see cref="AbstractNotRawSignature"/>.
      /// </summary>
      /// <value>The extra data for this <see cref="AbstractNotRawSignature"/>.</value>
      public Byte[] ExtraData { get; set; }

   }

   /// <summary>
   /// This is base class for <see cref="MethodDefinitionSignature"/> and <see cref="MethodReferenceSignature"/>.
   /// </summary>
   /// <remarks>
   /// Instances of this class can not be instantiated directly.
   /// Instead, use one of the following:
   /// <list type="bullet">
   /// <item><description><see cref="MethodDefinitionSignature"/>, or</description></item>
   /// <item><description><see cref="MethodReferenceSignature"/>.</description></item>
   /// </list>
   /// </remarks>
   public abstract class AbstractMethodSignature : AbstractNotRawSignature
   {
      // Disable inheritance to other assemblies
      internal AbstractMethodSignature( Int32 parameterCount )
      {
         this.Parameters = new List<ParameterSignature>( Math.Max( 0, parameterCount ) );
      }

      /// <summary>
      /// Gets or sets the <see cref="Physical.MethodSignatureInformation"/> of this <see cref="AbstractMethodSignature"/>, containing the calling conventions and other flags.
      /// </summary>
      /// <value>The <see cref="Physical.MethodSignatureInformation"/> of this <see cref="AbstractMethodSignature"/>, containing the calling conventions and other flags.</value>
      public MethodSignatureInformation MethodSignatureInformation { get; set; }

      /// <summary>
      /// Gets or sets the amount of generic arguments the method represented by this <see cref="AbstractMethodSignature"/> has.
      /// </summary>
      /// <value>The amount of generic arguments the method represented by this <see cref="AbstractMethodSignature"/> has.</value>
      public Int32 GenericArgumentCount { get; set; }

      /// <summary>
      /// Gets or sets the return type information for this <see cref="AbstractMethodSignature"/>.
      /// </summary>
      /// <value>The return type for this <see cref="AbstractMethodSignature"/>.</value>
      /// <seealso cref="ParameterSignature"/>
      public ParameterSignature ReturnType { get; set; }

      /// <summary>
      /// Gets the list of all parameter information for this <see cref="AbstractMethodSignature"/>.
      /// </summary>
      /// <value>The list of all parameter information for this <see cref="AbstractMethodSignature"/>.</value>
      /// <seealso cref="ParameterSignature"/>
      public List<ParameterSignature> Parameters { get; }
   }

   /// <summary>
   /// This class is a signature for a <see cref="MethodDefinition"/>, accessible by <see cref="MethodDefinition.Signature"/> property.
   /// </summary>
   /// <remarks>
   /// The difference between this class and <see cref="MethodReferenceSignature"/> is that <see cref="MethodReferenceSignature"/> can contain varargs parameters, capturing its on-site information.
   /// </remarks>
   public sealed class MethodDefinitionSignature : AbstractMethodSignature
   {
      /// <summary>
      /// Creates a new instance of <see cref="MethodDefinitionSignature"/> with given initial capacity for <see cref="AbstractMethodSignature.Parameters"/> list.
      /// </summary>
      /// <param name="parameterCount">The initial capacity for <see cref="AbstractMethodSignature.Parameters"/> list.</param>
      public MethodDefinitionSignature( Int32 parameterCount = 0 )
         : base( parameterCount )
      {
      }

      /// <summary>
      /// Returns the <see cref="SignatureKind.MethodDefinition"/>.
      /// </summary>
      /// <value>The <see cref="SignatureKind.MethodDefinition"/>.</value>
      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.MethodDefinition;
         }
      }
   }

   /// <summary>
   /// This class represents a signature for method references, which may appear in metadata tables or be referenced by IL byte code.
   /// </summary>
   public sealed class MethodReferenceSignature : AbstractMethodSignature
   {
      /// <summary>
      /// Creates a new instance of <see cref="MethodReferenceSignature"/> with given initial capacities for <see cref="AbstractMethodSignature.Parameters"/> and <see cref="VarArgsParameters"/> lists.
      /// </summary>
      /// <param name="parameterCount">The initial capacity for <see cref="AbstractMethodSignature.Parameters"/> list.</param>
      /// <param name="varArgsParameterCount">The initial capacity for <see cref="VarArgsParameters"/> list.</param>
      public MethodReferenceSignature( Int32 parameterCount = 0, Int32 varArgsParameterCount = 0 )
         : base( parameterCount )
      {
         this.VarArgsParameters = new List<ParameterSignature>( Math.Max( 0, varArgsParameterCount ) );
      }

      /// <summary>
      /// Gets the list of all vararg parameter information for this <see cref="MethodReferenceSignature"/>.
      /// </summary>
      /// <value>The list of all vararg parameter information for this <see cref="MethodReferenceSignature"/>.</value>
      /// <seealso cref="ParameterSignature"/>
      public List<ParameterSignature> VarArgsParameters { get; }

      /// <summary>
      /// Returns the <see cref="SignatureKind.MethodReference"/>.
      /// </summary>
      /// <value>The <see cref="SignatureKind.MethodReference"/>.</value>
      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.MethodReference;
         }
      }

   }

   /// <summary>
   /// This is base class for all signatures, that can have custom modifiers.
   /// </summary>
   /// <remarks>
   /// Instances of this class can not be instantiated directly.
   /// Instead, use one of the following:
   /// <list type="bullet">
   /// <item><description><see cref="FieldSignature"/>,</description></item>
   /// <item><description><see cref="PropertySignature"/>,</description></item>
   /// </list>
   /// </remarks>
   /// <seealso cref="CustomModifierSignature"/>
   public abstract class AbstractSignatureWithCustomMods : AbstractNotRawSignature
   {

      // Disable inheritance to other assemblies
      internal AbstractSignatureWithCustomMods( Int32 customModCount )
      {
         this.CustomModifiers = new List<CustomModifierSignature>( Math.Max( 0, customModCount ) );
      }

      /// <summary>
      /// Gets the list of all <see cref="CustomModifierSignature"/>s that this <see cref="AbstractSignatureWithCustomMods"/> contains.
      /// </summary>
      /// <value>The list of all <see cref="CustomModifierSignature"/>s that this <see cref="AbstractSignatureWithCustomMods"/> contains.</value>
      /// <see cref="CustomModifierSignature"/>
      public List<CustomModifierSignature> CustomModifiers { get; }
   }

   /// <summary>
   /// This class represents a signature for field definitions and references, which may appear in metadata tables or be referenced by IL byte code.
   /// </summary>
   public sealed class FieldSignature : AbstractSignatureWithCustomMods
   {
      /// <summary>
      /// Creates a new instance of <see cref="FieldSignature"/> with given initial capacity for <see cref="AbstractSignatureWithCustomMods.CustomModifiers"/> list.
      /// </summary>
      /// <param name="customModCount">Initial capacity for <see cref="AbstractSignatureWithCustomMods.CustomModifiers"/> list.</param>
      public FieldSignature( Int32 customModCount = 0 )
         : base( customModCount )
      {
      }

      /// <summary>
      /// Returns the <see cref="SignatureKind.Field"/>.
      /// </summary>
      /// <value>The <see cref="SignatureKind.Field"/>.</value>
      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.Field;
         }
      }

      /// <summary>
      /// Gets or sets the type for this <see cref="FieldSignature"/>.
      /// </summary>
      /// <value>The type for this <see cref="FieldSignature"/>.</value>
      /// <seealso cref="TypeSignature"/>
      public TypeSignature Type { get; set; }
   }

   /// <summary>
   /// This class represents a signature for a property, accessible by <see cref="PropertyDefinition.Signature"/> property.
   /// </summary>
   public sealed class PropertySignature : AbstractSignatureWithCustomMods
   {
      /// <summary>
      /// Creates a new instance of <see cref="PropertySignature"/>, with given initial capacities for <see cref="AbstractSignatureWithCustomMods.CustomModifiers"/> and <see cref="Parameters"/> lists.
      /// </summary>
      /// <param name="customModCount">The initial capacity for <see cref="AbstractSignatureWithCustomMods.CustomModifiers"/> list.</param>
      /// <param name="parameterCount">The initial capacity for <see cref="Parameters"/> list.</param>
      public PropertySignature( Int32 customModCount = 0, Int32 parameterCount = 0 )
         : base( customModCount )
      {
         this.Parameters = new List<ParameterSignature>( Math.Max( 0, parameterCount ) );
      }

      /// <summary>
      /// Returns the <see cref="SignatureKind.Property"/>.
      /// </summary>
      /// <value>The <see cref="SignatureKind.Property"/>.</value>
      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.Property;
         }
      }

      /// <summary>
      /// Gets or sets the value indicating whether this property signature is for static or instance property.
      /// </summary>
      /// <value>The value indicating whether this property signature is for static or instance property.</value>
      /// <remarks>
      /// Set to <c>true</c> for instance properties, and to <c>false</c> for static properties.
      /// </remarks>
      public Boolean HasThis { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="TypeSignature"/> for this <see cref="PropertySignature"/>.
      /// </summary>
      /// <value>The <see cref="TypeSignature"/> for this <see cref="PropertySignature"/>.</value>
      /// <seealso cref="TypeSignature"/>
      public TypeSignature PropertyType { get; set; }

      /// <summary>
      /// Gets the list of all index parameters for this <see cref="PropertySignature"/>.
      /// </summary>
      /// <value>The list of all index parameters for this <see cref="PropertySignature"/>.</value>
      /// <seealso cref="ParameterSignature"/>
      public List<ParameterSignature> Parameters { get; }
   }

   /// <summary>
   /// This class represents the signature of local variables of a method, and is accessible through <see cref="StandaloneSignature.Signature"/> property.
   /// </summary>
   public sealed class LocalVariablesSignature : AbstractNotRawSignature
   {
      /// <summary>
      /// Creates a new instance of <see cref="LocalVariablesSignature"/> with given initial capacity for <see cref="Locals"/> list.
      /// </summary>
      /// <param name="localsCount">The initial capacity for <see cref="Locals"/> list.</param>
      public LocalVariablesSignature( Int32 localsCount = 0 )
      {
         this.Locals = new List<LocalSignature>( Math.Max( 0, localsCount ) );
      }

      /// <summary>
      /// Returns the <see cref="SignatureKind.LocalVariables"/>.
      /// </summary>
      /// <value>The <see cref="SignatureKind.LocalVariables"/>.</value>
      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.LocalVariables;
         }
      }

      /// <summary>
      /// Gets the list of all <see cref="LocalSignature"/>s of this <see cref="LocalVariablesSignature"/>.
      /// </summary>
      /// <value>The list of all <see cref="LocalSignature"/>s of this <see cref="LocalVariablesSignature"/>.</value>
      /// <seealso cref="LocalSignature"/>
      public List<LocalSignature> Locals { get; }
   }

   /// <summary>
   /// This class is abstract class for parameter or local variable signature.
   /// </summary>
   /// <remarks>
   /// Instances of this class can not be instantiated directly.
   /// Instead, use one of the following:
   /// <list type="bullet">
   /// <item><description><see cref="ParameterSignature"/>, or</description></item>
   /// <item><description><see cref="LocalSignature"/>.</description></item>
   /// </list>
   /// </remarks>
   public abstract class ParameterOrLocalSignature
   {

      // Disable inheritance to other assemblies
      internal ParameterOrLocalSignature( Int32 customModCount )
      {
         this.CustomModifiers = new List<CustomModifierSignature>( Math.Max( 0, customModCount ) );
      }

      /// <summary>
      /// Gets the list of all <see cref="CustomModifierSignature"/>s that this <see cref="ParameterOrLocalSignature"/> contains.
      /// </summary>
      /// <value>The list of all <see cref="CustomModifierSignature"/>s that this <see cref="ParameterOrLocalSignature"/> contains.</value>
      /// <see cref="CustomModifierSignature"/>
      public List<CustomModifierSignature> CustomModifiers { get; }

      /// <summary>
      /// Gets or sets the value indicating whether this <see cref="ParameterOrLocalSignature"/> is by-ref type.
      /// </summary>
      /// <value>The value indicating whether this <see cref="ParameterOrLocalSignature"/> is by-ref type.</value>
      public Boolean IsByRef { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="TypeSignature"/> of this <see cref="ParameterOrLocalSignature"/>.
      /// </summary>
      /// <value>The <see cref="TypeSignature"/> of this <see cref="ParameterOrLocalSignature"/>.</value>
      /// <seealso cref="TypeSignature"/>
      public TypeSignature Type { get; set; }
   }

   /// <summary>
   /// This class represents a signature for a single local in <see cref="LocalVariablesSignature"/>.
   /// </summary>
   public sealed class LocalSignature : ParameterOrLocalSignature
   {
      /// <summary>
      /// Creates a new instance of <see cref="LocalSignature"/> with given initial capacity for <see cref="ParameterOrLocalSignature.CustomModifiers"/>.
      /// </summary>
      /// <param name="customModCount">The initial capacity for <see cref="ParameterOrLocalSignature.CustomModifiers"/>.</param>
      public LocalSignature( Int32 customModCount = 0 )
         : base( customModCount )
      {

      }

      /// <summary>
      /// Gets or sets the value indicating whether this <see cref="LocalSignature"/> is pinned.
      /// </summary>
      /// <value>The value indicating whether this <see cref="LocalSignature"/> is pinned.</value>
      public Boolean IsPinned { get; set; }

   }

   /// <summary>
   /// This class represents a single parameter in <see cref="AbstractMethodSignature.Parameters"/>, <see cref="MethodReferenceSignature.VarArgsParameters"/>, and <see cref="PropertySignature.Parameters"/>.
   /// </summary>
   public sealed class ParameterSignature : ParameterOrLocalSignature
   {
      /// <summary>
      /// Creates a new instance of <see cref="ParameterSignature"/> with given initial capacity for <see cref="ParameterOrLocalSignature.CustomModifiers"/>.
      /// </summary>
      /// <param name="customModCount">The initial capacity for <see cref="ParameterOrLocalSignature.CustomModifiers"/>.</param>
      public ParameterSignature( Int32 customModCount = 0 )
         : base( customModCount )
      {

      }
   }

   /// <summary>
   /// This class represents a single custom modifier in <see cref="AbstractSignatureWithCustomMods.CustomModifiers"/>, <see cref="ParameterOrLocalSignature.CustomModifiers"/>, <see cref="PointerTypeSignature.CustomModifiers"/>, and <see cref="SimpleArrayTypeSignature.CustomModifiers"/> lists.
   /// </summary>
   public sealed class CustomModifierSignature
   {
      /// <summary>
      /// Gets or sets the optionality of this <see cref="CustomModifierSignature"/>.
      /// </summary>
      /// <value>The optionality of this <see cref="CustomModifierSignature"/>.</value>
      /// <seealso cref="CustomModifierSignatureOptionality"/>
      public CustomModifierSignatureOptionality Optionality { get; set; }

      /// <summary>
      /// Gets or sets the type reference of this <see cref="CustomModifierSignature"/>.
      /// </summary>
      /// <value>The type reference of this <see cref="CustomModifierSignature"/>.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this <see cref="TableIndex"/> should be one of the following:
      /// <list type="bullet">
      /// <item><description><see cref="Tables.TypeDef"/>,</description></item>
      /// <item><description><see cref="Tables.TypeRef"/>, or</description></item>
      /// <item><description><see cref="Tables.TypeSpec"/>.</description></item>
      /// </list>
      /// </remarks>
      public TableIndex CustomModifierType { get; set; }
   }

   /// <summary>
   /// This enumeration contains possible values for <see cref="CustomModifierSignature.Optionality"/>.
   /// </summary>
   /// <remarks>
   /// The values of this enumeration are safe to be casted to <see cref="T:CILAssemblyManipulator.Physical.IO.SignatureElementTypes"/>.
   /// </remarks>
   public enum CustomModifierSignatureOptionality : byte
   {
      /// <summary>
      /// The custom modifier is deemed to be required as specified in ECMA-335 standard.
      /// </summary>
      Required = 0x1F, // Same as SignatureElementTypes.CModReqd

      /// <summary>
      /// The custom modifier is deemed to be optional as specified in ECMA-335 standard.
      /// </summary>
      Optional = 0x20,
   }

   /// <summary>
   /// This is base class for all type signatures in CAM.Physical framework.
   /// The type signatures are accessible through <see cref="TypeSpecification.Signature"/> property.
   /// </summary>
   /// <remarks>
   /// Instances of this class can not be instantiated directly.
   /// Instead, use one of the following:
   /// <list type="bullet">
   /// <item><description><see cref="SimpleTypeSignature"/>,</description></item>
   /// <item><description><see cref="ClassOrValueTypeSignature"/>,</description></item>
   /// <item><description><see cref="SimpleArrayTypeSignature"/>,</description></item>
   /// <item><description><see cref="GenericParameterTypeSignature"/>,</description></item>
   /// <item><description><see cref="ComplexArrayTypeSignature"/>,</description></item>
   /// <item><description><see cref="PointerTypeSignature"/>, or</description></item>
   /// <item><description><see cref="FunctionPointerTypeSignature"/>.</description></item>
   /// </list>
   /// </remarks>
   public abstract class TypeSignature : AbstractNotRawSignature
   {
      // Disable inheritance to other assemblies
      internal TypeSignature()
      {

      }

      /// <summary>
      /// Returns the <see cref="SignatureKind.Type"/>.
      /// </summary>
      /// <value>The <see cref="SignatureKind.Type"/>.</value>
      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.Type;
         }
      }

      /// <summary>
      /// Gets the <see cref="Physical.TypeSignatureKind"/> enumeration descripting the actual type of this <see cref="TypeSignature"/>.
      /// </summary>
      /// <value>The <see cref="Physical.TypeSignatureKind"/> enumeration descripting the actual type of this <see cref="TypeSignature"/>.</value>
      /// <seealso cref="Physical.TypeSignatureKind"/>
      public abstract TypeSignatureKind TypeSignatureKind { get; }

   }

   /// <summary>
   /// This enumeration tells what type instance of <see cref="TypeSignature"/> really is.
   /// </summary>
   public enum TypeSignatureKind : byte
   {
      /// <summary>
      /// The <see cref="TypeSignature"/> is of type <see cref="SimpleTypeSignature"/>.
      /// </summary>
      Simple,

      /// <summary>
      /// The <see cref="TypeSignature"/> is of type <see cref="ComplexArrayTypeSignature"/>.
      /// </summary>
      ComplexArray,

      /// <summary>
      /// The <see cref="TypeSignature"/> is of type <see cref="ClassOrValueTypeSignature"/>.
      /// </summary>
      ClassOrValue,

      /// <summary>
      /// The <see cref="TypeSignature"/> is of type <see cref="GenericParameterTypeSignature"/>.
      /// </summary>
      GenericParameter,

      /// <summary>
      /// The <see cref="TypeSignature"/> is of type <see cref="FunctionPointerTypeSignature"/>.
      /// </summary>
      FunctionPointer,

      /// <summary>
      /// The <see cref="TypeSignature"/> is of type <see cref="PointerTypeSignature"/>.
      /// </summary>
      Pointer,

      /// <summary>
      /// The <see cref="TypeSignature"/> is of type <see cref="SimpleArrayTypeSignature"/>.
      /// </summary>
      SimpleArray,
   }

   /// <summary>
   /// This class represents type signature for built-in type, such as <see cref="Int32"/>, <see cref="String"/>, or <see cref="Void"/>.
   /// The built-in types should always appear in signatures instead of their counterparts expressable with <see cref="ClassOrValueTypeSignature"/>.
   /// </summary>
   /// <remarks>
   /// The instances of this class should be obtained through <see cref="Meta.SignatureProvider.GetSimpleTypeSignatureOrNull"/> or <see cref="E_CILPhysical.GetSimpleTypeSignature"/> methods.
   /// This is to save memory - no need to allocate duplicate <see cref="SimpleTypeSignature"/> objects with identical state (assuming <see cref="Meta.SignatureProvider"/> caches the <see cref="SimpleTypeSignature"/>s).
   /// </remarks>
   public sealed class SimpleTypeSignature : TypeSignature
   {
      /// <summary>
      /// Creates a new instance of <see cref="SimpleTypeSignature"/> with specified <see cref="SimpleTypeSignatureKind"/>.
      /// </summary>
      /// <param name="type">The <see cref="SimpleTypeSignatureKind"/>.</param>
      /// <remarks>
      /// The only place where this constructor should be used is by types implementing <see cref="Meta.SignatureProvider"/>.
      /// </remarks>
      public SimpleTypeSignature( SimpleTypeSignatureKind type )
      {
         this.SimpleType = type;
      }

      /// <summary>
      /// Returns the <see cref="TypeSignatureKind.Simple"/>.
      /// </summary>
      /// <value>The <see cref="TypeSignatureKind.Simple"/>.</value>
      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.Simple;
         }
      }

      /// <summary>
      /// Gets the simple type value of this <see cref="SimpleTypeSignature"/>.
      /// </summary>
      /// <value>The simple type value of this <see cref="SimpleTypeSignature"/>.</value>
      /// <seealso cref="SimpleTypeSignatureKind"/>
      public SimpleTypeSignatureKind SimpleType { get; }
   }

   /// <summary>
   /// This enumeration represents the kind of <see cref="SimpleTypeSignature"/>.
   /// </summary>
   /// <remarks>
   /// The values of this enumeration are safe to be casted to <see cref="T:CILAssemblyManipulator.Physical.IO.SignatureElementTypes"/>.
   /// </remarks>
   public enum SimpleTypeSignatureKind : byte
   {
      /// <summary>
      /// This is type signature for <see cref="Void"/>.
      /// </summary>
      Void = 0x01, // Same as SignatureElementTypes.Void

      /// <summary>
      /// This is type signature for <see cref="Boolean"/>.
      /// </summary>
      Boolean,

      /// <summary>
      /// This is type signature for <see cref="Char"/>.
      /// </summary>
      Char,

      /// <summary>
      /// This is type signature for <see cref="SByte"/>.
      /// </summary>
      I1,

      /// <summary>
      /// This is type signature for <see cref="Byte"/>.
      /// </summary>
      U1,

      /// <summary>
      /// This is type signature for <see cref="Int16"/>.
      /// </summary>
      I2,

      /// <summary>
      /// This is type signature for <see cref="UInt16"/>.
      /// </summary>
      U2,

      /// <summary>
      /// This is type signature for <see cref="Int32"/>.
      /// </summary>
      I4,

      /// <summary>
      /// This is type signature for <see cref="UInt32"/>.
      /// </summary>
      U4,

      /// <summary>
      /// This is type signature for <see cref="Int64"/>.
      /// </summary>
      I8,

      /// <summary>
      /// This is type signature for <see cref="UInt64"/>.
      /// </summary>
      U8,

      /// <summary>
      /// This is type signature for <see cref="Single"/>.
      /// </summary>
      R4,

      /// <summary>
      /// This is type signature for <see cref="Double"/>.
      /// </summary>
      R8,

      /// <summary>
      /// This is type signature for <see cref="String"/>.
      /// </summary>
      String,

      /// <summary>
      /// This is type signature for <see cref="T:System.TypedByReference"/>.
      /// </summary>
      TypedByRef = 0x16, // Same as SignatureElementTypes.TypedByRef

      /// <summary>
      /// This is type signature for <see cref="IntPtr"/>.
      /// </summary>
      I = 0x18, // Same as SignatureElementTypes.I

      /// <summary>
      /// This is type signature for <see cref="UIntPtr"/>.
      /// </summary>
      U,

      /// <summary>
      /// This is type signature for <see cref="Object"/>.
      /// </summary>
      Object = 0x1C, // Same as SignatureElementTypes.Object
   }

   /// <summary>
   /// This class represents a signature for a direct reference to type which is not a built-in type.
   /// </summary>
   public sealed class ClassOrValueTypeSignature : TypeSignature
   {
      /// <summary>
      /// Creates a new instance of <see cref="ClassOrValueTypeSignature"/> with given initial capacity for <see cref="GenericArguments"/> list.
      /// </summary>
      /// <param name="genericArgumentsCount">The initial capacity for <see cref="GenericArguments"/> list.</param>
      public ClassOrValueTypeSignature( Int32 genericArgumentsCount = 0 )
      {
         this.GenericArguments = new List<TypeSignature>( Math.Max( 0, genericArgumentsCount ) );
      }

      /// <summary>
      /// Returns the <see cref="TypeSignatureKind.ClassOrValue"/>.
      /// </summary>
      /// <value>The <see cref="TypeSignatureKind.ClassOrValue"/>.</value>
      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.ClassOrValue;
         }
      }

      /// <summary>
      /// Gets or sets the <see cref="Physical.TypeReferenceKind"/> of this <see cref="ClassOrValueTypeSignature"/>.
      /// </summary>
      /// <value>
      /// The <see cref="Physical.TypeReferenceKind"/> of this <see cref="ClassOrValueTypeSignature"/>.
      /// </value>
      /// <seealso cref="Physical.TypeReferenceKind"/>
      public TypeReferenceKind TypeReferenceKind { get; set; }

      /// <summary>
      /// Gets or sets the type reference of this <see cref="ClassOrValueTypeSignature"/>.
      /// </summary>
      /// <value>The type reference of this <see cref="ClassOrValueTypeSignature"/>.</value>
      /// <remarks>
      /// The <see cref="TableIndex.Table"/> property of this <see cref="TableIndex"/> should be one of the following:
      /// <list type="bullet">
      /// <item><description><see cref="Tables.TypeDef"/>,</description></item>
      /// <item><description><see cref="Tables.TypeRef"/>, or</description></item>
      /// <item><description><see cref="Tables.TypeSpec"/>.</description></item>
      /// </list>
      /// </remarks>
      public TableIndex Type { get; set; }

      /// <summary>
      /// Gets the list of generic arguments for this <see cref="ClassOrValueTypeSignature"/>.
      /// </summary>
      /// <value>The list of generic arguments for this <see cref="ClassOrValueTypeSignature"/>.</value>
      public List<TypeSignature> GenericArguments { get; }
   }

   /// <summary>
   /// This enumeration tells what kind of type is in question for <see cref="ClassOrValueTypeSignature"/>.
   /// </summary>
   /// <remarks>
   /// The values of this enumeration are safe to be casted to <see cref="T:CILAssemblyManipulator.Physical.IO.SignatureElementTypes"/>.
   /// </remarks>
   public enum TypeReferenceKind : byte
   {
      /// <summary>
      /// The type is value type.
      /// </summary>
      ValueType = 0x11,

      /// <summary>
      /// The type is reference type.
      /// </summary>
      Class = 0x12,
   }

   /// <summary>
   /// This class represents a type signature which is a reference to a type or method generic parameter.
   /// </summary>
   public sealed class GenericParameterTypeSignature : TypeSignature
   {

      /// <summary>
      /// Returns the <see cref="TypeSignatureKind.GenericParameter"/>.
      /// </summary>
      /// <value>The <see cref="TypeSignatureKind.GenericParameter"/>.</value>
      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.GenericParameter;
         }
      }

      /// <summary>
      /// Gets or sets the <see cref="Physical.GenericParameterKind"/> of this <see cref="GenericParameterTypeSignature"/>.
      /// </summary>
      /// <value>The <see cref="Physical.GenericParameterKind"/> of this <see cref="GenericParameterTypeSignature"/>.</value>
      /// <seealso cref="Physical.GenericParameterKind"/>
      public GenericParameterKind GenericParameterKind { get; set; }

      /// <summary>
      /// Gets or sets the zero-based index of the referenced generic parameter.
      /// </summary>
      /// <value>The zero-based index of the referenced generic parameter.</value>
      public Int32 GenericParameterIndex { get; set; }
   }

   /// <summary>
   /// This enumeration tells what kind of generic parameter is in question for <see cref="GenericParameterTypeSignature"/>.
   /// </summary>
   /// <remarks>
   /// The values of this enumeration are safe to be casted to <see cref="T:CILAssemblyManipulator.Physical.IO.SignatureElementTypes"/>.
   /// </remarks>
   public enum GenericParameterKind : byte
   {
      /// <summary>
      /// The referenced generic parameter is type generic parameter.
      /// </summary>
      Type = 0x13,

      /// <summary>
      /// The referenced generic parameter is method generic parameter.
      /// </summary>
      Method = 0x1E,
   }

   /// <summary>
   /// This class represents a type signature which is a pointer to a function with certain signature.
   /// </summary>
   public sealed class FunctionPointerTypeSignature : TypeSignature
   {

      /// <summary>
      /// Returns the <see cref="TypeSignatureKind.FunctionPointer"/>.
      /// </summary>
      /// <value>The <see cref="TypeSignatureKind.FunctionPointer"/>.</value>
      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.FunctionPointer;
         }
      }

      /// <summary>
      /// Gets or sets the <see cref="MethodReferenceSignature"/> of this <see cref="FunctionPointerTypeSignature"/>.
      /// </summary>
      /// <value>The <see cref="MethodReferenceSignature"/> of this <see cref="FunctionPointerTypeSignature"/>.</value>
      public MethodReferenceSignature MethodSignature { get; set; }
   }

   /// <summary>
   /// This class represents a type signature which is a pointer to a certain type (not a function).
   /// </summary>
   public sealed class PointerTypeSignature : TypeSignature
   {

      /// <summary>
      /// Creates a new instance of <see cref="PointerTypeSignature"/> with given initial capacity for <see cref="CustomModifiers"/> list.
      /// </summary>
      /// <param name="customModCount">The initial capacity for <see cref="CustomModifiers"/> list.</param>
      public PointerTypeSignature( Int32 customModCount = 0 )
      {
         this.CustomModifiers = new List<CustomModifierSignature>( Math.Max( 0, customModCount ) );
      }

      /// <summary>
      /// Returns the <see cref="TypeSignatureKind.Pointer"/>.
      /// </summary>
      /// <value>The <see cref="TypeSignatureKind.Pointer"/>.</value>
      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.Pointer;
         }
      }

      /// <summary>
      /// Gets the list of all <see cref="CustomModifierSignature"/>s of this <see cref="PointerTypeSignature"/>.
      /// </summary>
      /// <value>The list of all <see cref="CustomModifierSignature"/>s of this <see cref="PointerTypeSignature"/>.</value>
      public List<CustomModifierSignature> CustomModifiers { get; }

      /// <summary>
      /// Gets or sets the <see cref="TypeSignature"/> of this <see cref="PointerTypeSignature"/>.
      /// </summary>
      /// <value>The <see cref="TypeSignature"/> of this <see cref="PointerTypeSignature"/>.</value>
      public TypeSignature PointerType { get; set; }
   }

   /// <summary>
   /// This class is abstract base class for simple and complex array type signatures.
   /// </summary>
   /// <remarks>
   /// Instances of this class can not be instantiated directly.
   /// Instead, use one of the following:
   /// <list type="bullet">
   /// <item><description><see cref="SimpleArrayTypeSignature"/>, or</description></item>
   /// <item><description><see cref="ComplexArrayTypeSignature"/>.</description></item>
   /// </list>
   /// </remarks>
   public abstract class AbstractArrayTypeSignature : TypeSignature
   {
      // Disable inheritance to other assemblies
      internal AbstractArrayTypeSignature()
      {

      }

      /// <summary>
      /// Gets or sets the <see cref="TypeSignature"/> of this <see cref="AbstractArrayTypeSignature"/>.
      /// </summary>
      /// <value>The <see cref="TypeSignature"/> of this <see cref="AbstractArrayTypeSignature"/>.</value>
      public TypeSignature ArrayType { get; set; }
   }

   /// <summary>
   /// This class represents a multidimensional array type signature with customizable lower bounds and sizes.
   /// </summary>
   public sealed class ComplexArrayTypeSignature : AbstractArrayTypeSignature
   {
      /// <summary>
      /// Creates a new instance of <see cref="ComplexArrayTypeSignature"/> with given initial capacities for <see cref="Sizes"/> and <see cref="LowerBounds"/> lists.
      /// </summary>
      /// <param name="sizesCount">The initial capacity for <see cref="Sizes"/> list.</param>
      /// <param name="lowerBoundsCount">The initial capacity for <see cref="LowerBounds"/> list.</param>
      public ComplexArrayTypeSignature( Int32 sizesCount = 0, Int32 lowerBoundsCount = 0 )
      {
         this.Sizes = new List<Int32>( Math.Max( 0, sizesCount ) );
         this.LowerBounds = new List<Int32>( Math.Max( 0, lowerBoundsCount ) );
      }

      /// <summary>
      /// Returns the <see cref="TypeSignatureKind.ComplexArray"/>.
      /// </summary>
      /// <value>The <see cref="TypeSignatureKind.ComplexArray"/>.</value>
      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.ComplexArray;
         }
      }

      /// <summary>
      /// Gets or sets the rank (number of dimensions) for this <see cref="ComplexArrayTypeSignature"/>.
      /// </summary>
      /// <value>The rank (number of dimensions) for this <see cref="ComplexArrayTypeSignature"/>.</value>
      public Int32 Rank { get; set; }

      /// <summary>
      /// Gets the list of sizes of dimensions for this <see cref="ComplexArrayTypeSignature"/>.
      /// </summary>
      /// <value>The list for sizes of dimensions for this <see cref="ComplexArrayTypeSignature"/>.</value>
      public List<Int32> Sizes { get; }

      /// <summary>
      /// Gets the list of lower bounds of dimensions for this <see cref="ComplexArrayTypeSignature"/>.
      /// </summary>
      /// <value>The list of lower bounds of dimensions for this <see cref="ComplexArrayTypeSignature"/>.</value>
      public List<Int32> LowerBounds { get; }
   }

   /// <summary>
   /// This class represents a one-dimensional array type signature with no lower bound nor size constraints.
   /// </summary>
   public sealed class SimpleArrayTypeSignature : AbstractArrayTypeSignature
   {

      /// <summary>
      /// Creates a new instance of <see cref="SimpleArrayTypeSignature"/> with given initial capacity for <see cref="CustomModifiers"/> list.
      /// </summary>
      /// <param name="customModCount">The initial capacity for <see cref="CustomModifiers"/> list.</param>
      public SimpleArrayTypeSignature( Int32 customModCount = 0 )
      {
         this.CustomModifiers = new List<CustomModifierSignature>( customModCount );
      }

      /// <summary>
      /// Returns the <see cref="TypeSignatureKind.SimpleArray"/>.
      /// </summary>
      /// <value>The <see cref="TypeSignatureKind.SimpleArray"/>.</value>
      public override TypeSignatureKind TypeSignatureKind
      {
         get
         {
            return TypeSignatureKind.SimpleArray;
         }
      }

      /// <summary>
      /// Gets the list of all <see cref="CustomModifierSignature"/>s for this <see cref="SimpleArrayTypeSignature"/>.
      /// </summary>
      /// <value>The list of all <see cref="CustomModifierSignature"/>s for this <see cref="SimpleArrayTypeSignature"/>.</value>
      public List<CustomModifierSignature> CustomModifiers { get; }
   }

   /// <summary>
   /// This class represents a signature for generic method instantiation.
   /// </summary>
   public sealed class GenericMethodSignature : AbstractNotRawSignature
   {

      /// <summary>
      /// Creates a new instance of <see cref="GenericMethodSignature"/> with given initial capacity for <see cref="GenericArguments"/> list.
      /// </summary>
      /// <param name="genericArgumentsCount">The initial capacity for <see cref="GenericArguments"/> list.</param>
      public GenericMethodSignature( Int32 genericArgumentsCount = 0 )
      {
         this.GenericArguments = new List<TypeSignature>( Math.Max( 0, genericArgumentsCount ) );
      }

      /// <summary>
      /// Returns the <see cref="SignatureKind.GenericMethodInstantiation"/>.
      /// </summary>
      /// <value>The <see cref="SignatureKind.GenericMethodInstantiation"/>.</value>
      public override SignatureKind SignatureKind
      {
         get
         {
            return SignatureKind.GenericMethodInstantiation;
         }
      }

      /// <summary>
      /// Gets the list of all generic arguments of this <see cref="GenericMethodSignature"/>.
      /// </summary>
      /// <value>The list of all generic arguments of this <see cref="GenericMethodSignature"/>.</value>
      /// <seealso cref="TypeSignature"/>
      public List<TypeSignature> GenericArguments { get; }
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
      // Disable inheritance to other assemblies
      internal AbstractMarshalingInfo()
      {
      }

      /// <summary>
      /// Gets the <see cref="UnmanagedType" /> value the data is to be marshaled as.
      /// </summary>
      /// <value>The <see cref="UnmanagedType" /> value the data is to be marshaled as.</value>
      public UnmanagedType Value { get; set; }

      /// <summary>
      /// Gets the <see cref="Physical.MarshalingInfoKind"/> enumeration descripting the actual type of this <see cref="AbstractMarshalingInfo"/>.
      /// </summary>
      /// <value>The <see cref="Physical.MarshalingInfoKind"/> enumeration descripting the actual type of this <see cref="AbstractMarshalingInfo"/>.</value>
      /// <seealso cref="Physical.MarshalingInfoKind"/>
      public abstract MarshalingInfoKind MarshalingInfoKind { get; }

      /// <summary>
      /// Creates a new <see cref="AbstractMarshalingInfo"/> from the information from <see cref="T:System.Runtime.InteropServices.MarshalAsAttribute"/> attribute type.
      /// Since the attribute type is not included in all portable profiles, this method does not take the attribute as parameter directly.
      /// </summary>
      /// <param name="ut">The value of <see cref="P:System.Runtime.InteropServices.MarshalAsAttribute.Value"/>.</param>
      /// <param name="sizeConst">The value of <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.SizeConst"/>.</param>
      /// <param name="iidParameterIndex">The value of <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.IidParameterIndex"/>.</param>
      /// <param name="sizeParameterIndex">The value of <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.SizeParamIndex"/>.</param>
      /// <param name="arraySubType">The value of <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.ArraySubType"/>.</param>
      /// <param name="safeArraySubType">The value of <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.SafeArraySubType"/>.</param>
      /// <param name="safeArrayUserDefinedSubType">The value of <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.SafeArrayUserDefinedSubType"/>.</param>
      /// <param name="customMarshalType">The value of <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.MarshalType"/>.</param>
      /// <param name="customMarshalCookie">The value of <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.MarshalCookie"/>.</param>
      /// <returns>A new <see cref="AbstractMarshalingInfo"/> with given information.</returns>
      public static AbstractMarshalingInfo FromAttributeInfo(
         UnmanagedType ut,
         Int32 sizeConst,
         Int32 iidParameterIndex,
         Int16 sizeParameterIndex,
         UnmanagedType arraySubType,
         VarEnum safeArraySubType,
         Type safeArrayUserDefinedSubType,
         String customMarshalType,
         String customMarshalCookie
         )
      {
         AbstractMarshalingInfo result;
         switch ( ut )
         {
            case UnmanagedType.ByValTStr:
               result = new FixedLengthStringMarshalingInfo()
               {
                  Value = ut,
                  Size = sizeConst
               };
               break;
            case UnmanagedType.IUnknown:
            case UnmanagedType.IDispatch:
            case UnmanagedType.Interface:
               result = new InterfaceMarshalingInfo()
               {
                  Value = ut,
                  IIDParameterIndex = iidParameterIndex
               };
               break;
            case UnmanagedType.SafeArray:
               result = new SafeArrayMarshalingInfo()
               {
                  Value = ut,
                  ElementType = safeArraySubType,
                  UserDefinedType = safeArrayUserDefinedSubType?.AssemblyQualifiedName
               };
               break;
            case UnmanagedType.ByValArray:
               result = new FixedLengthArrayMarshalingInfo()
               {
                  Value = ut,
                  Size = sizeConst,
                  ElementType = arraySubType
               };
               break;
            case UnmanagedType.LPArray:
               result = new ArrayMarshalingInfo()
               {
                  Value = ut,
                  ElementType = arraySubType,
                  SizeParameterIndex = sizeParameterIndex,
                  Size = sizeConst,
                  Flags = -1
               };
               break;
            case UnmanagedType.CustomMarshaler:
               result = new CustomMarshalingInfo()
               {
                  Value = ut,
                  GUIDString = null,
                  NativeTypeName = null,
                  CustomMarshalerTypeName = customMarshalType,
                  MarshalCookie = customMarshalCookie
               };
               break;
            default:
               result = new SimpleMarshalingInfo()
               {
                  Value = ut
               };
               break;
         }

         return result;
      }


#if !CAM_PHYSICAL_IS_PORTABLE

      /// <summary>
      /// Creates <see cref="AbstractMarshalingInfo"/> with all information specified in <see cref="System.Runtime.InteropServices.MarshalAsAttribute"/>.
      /// </summary>
      /// <param name="attr">The <see cref="System.Runtime.InteropServices.MarshalAsAttribute"/>. If <c>null</c>, then the result will be <c>null</c> as well.</param>
      /// <returns>A new <see cref="AbstractMarshalingInfo"/> with given information, or <c>null</c> if <paramref name="attr"/> is <c>null</c>.</returns>
      /// <remarks>
      /// This is a wrapper around <see cref="FromAttributeInfo"/> method.
      /// </remarks>
      public static AbstractMarshalingInfo FromAttribute( System.Runtime.InteropServices.MarshalAsAttribute attr )
      {
         return attr == null ?
            null :
            FromAttributeInfo( (UnmanagedType) attr.Value, attr.SizeConst, attr.IidParameterIndex, attr.SizeParamIndex, (UnmanagedType) attr.ArraySubType, (VarEnum) attr.SafeArraySubType, attr.SafeArrayUserDefinedSubType, attr.MarshalType, attr.MarshalCookie );
      }

#endif

   }

   /// <summary>
   /// This enumeration tells what type instance of <see cref="AbstractMarshalingInfo"/> really is.
   /// </summary>
   public enum MarshalingInfoKind
   {
      /// <summary>
      /// The <see cref="AbstractMarshalingInfo"/> is of type <see cref="SimpleMarshalingInfo"/>.
      /// </summary>
      Simple,

      /// <summary>
      /// The <see cref="AbstractMarshalingInfo"/> is of type <see cref="FixedLengthStringMarshalingInfo"/>.
      /// </summary>
      FixedLengthString,

      /// <summary>
      /// The <see cref="AbstractMarshalingInfo"/> is of type <see cref="FixedLengthArrayMarshalingInfo"/>.
      /// </summary>
      FixedLengthArray,

      /// <summary>
      /// The <see cref="AbstractMarshalingInfo"/> is of type <see cref="SafeArrayMarshalingInfo"/>.
      /// </summary>
      SafeArray,

      /// <summary>
      /// The <see cref="AbstractMarshalingInfo"/> is of type <see cref="ArrayMarshalingInfo"/>.
      /// </summary>
      Array,

      /// <summary>
      /// The <see cref="AbstractMarshalingInfo"/> is of type <see cref="InterfaceMarshalingInfo"/>.
      /// </summary>
      Interface,

      /// <summary>
      /// The <see cref="AbstractMarshalingInfo"/> is of type <see cref="CustomMarshalingInfo"/>.
      /// </summary>
      Custom,

      /// <summary>
      /// The <see cref="AbstractMarshalingInfo"/> is of type <see cref="RawMarshalingInfo"/>.
      /// </summary>
      Raw
   }

   /// <summary>
   /// This class represents marshaling information for simple built-in types, that are handled the same way both managed and unmanaged.
   /// </summary>
   public sealed class SimpleMarshalingInfo : AbstractMarshalingInfo
   {
      /// <summary>
      /// Returns the <see cref="MarshalingInfoKind.Simple"/>.
      /// </summary>
      /// <value>The <see cref="MarshalingInfoKind.Simple"/>.</value>
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
      public static readonly CustomAttributeArgumentTypeSimple Boolean = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.Boolean );
      public static readonly CustomAttributeArgumentTypeSimple Char = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.Char );
      public static readonly CustomAttributeArgumentTypeSimple SByte = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.I1 );
      public static readonly CustomAttributeArgumentTypeSimple Byte = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.U1 );
      public static readonly CustomAttributeArgumentTypeSimple Int16 = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.I2 );
      public static readonly CustomAttributeArgumentTypeSimple UInt16 = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.U2 );
      public static readonly CustomAttributeArgumentTypeSimple Int32 = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.I4 );
      public static readonly CustomAttributeArgumentTypeSimple UInt32 = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.U4 );
      public static readonly CustomAttributeArgumentTypeSimple Int64 = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.I8 );
      public static readonly CustomAttributeArgumentTypeSimple UInt64 = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.U8 );
      public static readonly CustomAttributeArgumentTypeSimple Single = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.R4 );
      public static readonly CustomAttributeArgumentTypeSimple Double = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.R8 );
      public static readonly CustomAttributeArgumentTypeSimple String = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.String );
      public static readonly CustomAttributeArgumentTypeSimple Type = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.Type );
      public static readonly CustomAttributeArgumentTypeSimple Object = new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.Object );

      private CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind kind )
      {
         this.SimpleType = kind;
      }

      public override CustomAttributeArgumentTypeKind ArgumentTypeKind
      {
         get
         {
            return CustomAttributeArgumentTypeKind.Simple;
         }
      }

      public CustomAttributeArgumentTypeSimpleKind SimpleType { get; }

      public static CustomAttributeArgumentTypeSimple GetByKind( CustomAttributeArgumentTypeSimpleKind kind )
      {
         CustomAttributeArgumentTypeSimple retVal;
         if ( !TryGetByKind( kind, out retVal ) )
         {
            throw new ArgumentException( "Unrecognized CA argument simple type kind: " + kind + "." );
         }
         return retVal;
      }

      public static Boolean TryGetByKind( CustomAttributeArgumentTypeSimpleKind kind, out CustomAttributeArgumentTypeSimple caArgType )
      {
         switch ( kind )
         {
            case CustomAttributeArgumentTypeSimpleKind.Boolean:
               caArgType = Boolean;
               break;
            case CustomAttributeArgumentTypeSimpleKind.Char:
               caArgType = Char;
               break;
            case CustomAttributeArgumentTypeSimpleKind.I1:
               caArgType = SByte;
               break;
            case CustomAttributeArgumentTypeSimpleKind.U1:
               caArgType = Byte;
               break;
            case CustomAttributeArgumentTypeSimpleKind.I2:
               caArgType = Int16;
               break;
            case CustomAttributeArgumentTypeSimpleKind.U2:
               caArgType = UInt16;
               break;
            case CustomAttributeArgumentTypeSimpleKind.I4:
               caArgType = Int32;
               break;
            case CustomAttributeArgumentTypeSimpleKind.U4:
               caArgType = UInt32;
               break;
            case CustomAttributeArgumentTypeSimpleKind.I8:
               caArgType = Int64;
               break;
            case CustomAttributeArgumentTypeSimpleKind.U8:
               caArgType = UInt64;
               break;
            case CustomAttributeArgumentTypeSimpleKind.R4:
               caArgType = Single;
               break;
            case CustomAttributeArgumentTypeSimpleKind.R8:
               caArgType = Double;
               break;
            case CustomAttributeArgumentTypeSimpleKind.String:
               caArgType = String;
               break;
            case CustomAttributeArgumentTypeSimpleKind.Object:
               caArgType = Object;
               break;
            case CustomAttributeArgumentTypeSimpleKind.Type:
               caArgType = Type;
               break;
            default:
               caArgType = null;
               break;
         }

         return caArgType != null;
      }
   }

   /// <summary>
   /// This enumeration represents the kind of <see cref="CustomAttributeArgumentTypeSimple"/>.
   /// </summary>
   /// <remarks>
   /// The values of this enumeration are safe to be casted to <see cref="T:CILAssemblyManipulator.Physical.IO.SignatureElementTypes"/>.
   /// </remarks>
   public enum CustomAttributeArgumentTypeSimpleKind : byte
   {
      Boolean = 0x02, // Same as SignatureElementTypes.Boolean
      Char,
      I1,
      U1,
      I2,
      U2,
      I4,
      U4,
      I8,
      U8,
      R4,
      R8,
      String,
      Type = 0x50, // Same as SignatureElementTypes.Type
      Object // Same as SignatureElementTypes.CA_Boxed
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

      public abstract SecurityInformationKind SecurityInformationKind { get; }

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
         case SignatureKind.Raw:
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
               TypeReferenceKind = clazz.TypeReferenceKind,
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
      clone.MethodSignatureInformation = original.MethodSignatureInformation;
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

   private static LocalSignature CloneLocalSignature( LocalSignature local, Func<TableIndex, TableIndex> tableIndexTranslator )
   {
      var retVal = new LocalSignature( local.CustomModifiers.Count )
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
            Optionality = cm.Optionality,
            CustomModifierType = tableIndexTranslator == null ? cm.CustomModifierType : tableIndexTranslator( cm.CustomModifierType )
         };
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
               case CustomAttributeArgumentTypeSimpleKind.Boolean:
                  return typeof( Boolean );
               case CustomAttributeArgumentTypeSimpleKind.Char:
                  return typeof( Char );
               case CustomAttributeArgumentTypeSimpleKind.I1:
                  return typeof( SByte );
               case CustomAttributeArgumentTypeSimpleKind.U1:
                  return typeof( Byte );
               case CustomAttributeArgumentTypeSimpleKind.I2:
                  return typeof( Int16 );
               case CustomAttributeArgumentTypeSimpleKind.U2:
                  return typeof( UInt16 );
               case CustomAttributeArgumentTypeSimpleKind.I4:
                  return typeof( Int32 );
               case CustomAttributeArgumentTypeSimpleKind.U4:
                  return typeof( UInt32 );
               case CustomAttributeArgumentTypeSimpleKind.I8:
                  return typeof( Int64 );
               case CustomAttributeArgumentTypeSimpleKind.U8:
                  return typeof( UInt64 );
               case CustomAttributeArgumentTypeSimpleKind.R4:
                  return typeof( Single );
               case CustomAttributeArgumentTypeSimpleKind.R8:
                  return typeof( Double );
               case CustomAttributeArgumentTypeSimpleKind.String:
                  return typeof( String );
               case CustomAttributeArgumentTypeSimpleKind.Type:
                  return typeof( CustomAttributeValue_TypeReference );
               case CustomAttributeArgumentTypeSimpleKind.Object:
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

   public static Boolean IsRequired( this CustomModifierSignature customMod )
   {
      return customMod != null && customMod.Optionality == CustomModifierSignatureOptionality.Required;
   }

   public static Boolean IsOptional( this CustomModifierSignature customMod )
   {
      return customMod != null && customMod.Optionality == CustomModifierSignatureOptionality.Optional;
   }

   public static Boolean IsClass( this TypeReferenceKind typeReferenceKind )
   {
      return typeReferenceKind == TypeReferenceKind.Class;
   }

   public static Boolean IsTypeParameter( this GenericParameterKind genericParameterKind )
   {
      return genericParameterKind == GenericParameterKind.Type;
   }
}