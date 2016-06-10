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
using UtilPack;
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
   /// This interface represents any object part of <see cref="AbstractSignature"/>. All signatures are <see cref="SignatureElement"/>s, but also some object not extending <see cref="AbstractSignature"/> can be such elements, e.g. <see cref="CustomModifierSignature"/>.
   /// </summary>
   public interface SignatureElement
   {
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
   public abstract class AbstractSignature : SignatureElement
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
   public abstract class ParameterOrLocalSignature : SignatureElement
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
   public sealed class CustomModifierSignature : SignatureElement
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
      /// Gets or sets the <see cref="Physical.ComplexArrayInfo"/> object containing information about rank, lower bounds, and sizes.
      /// </summary>
      /// <value>The <see cref="Physical.ComplexArrayInfo"/> object containing information about rank, lower bounds, and sizes.</value>
      public ComplexArrayInfo ComplexArrayInfo { get; set; }

   }

   /// <summary>
   /// This type encapsulates information that is related to <see cref="ComplexArrayTypeSignature"/>.
   /// </summary>
   public sealed class ComplexArrayInfo // : IEquatable<ComplexArrayInfo>
   {
      /// <summary>
      /// Creates a new instance of <see cref="ComplexArrayInfo"/> with given initial capacities for <see cref="Sizes"/> and <see cref="LowerBounds"/> lists.
      /// </summary>
      /// <param name="sizesCount">The initial capacity for <see cref="Sizes"/> list.</param>
      /// <param name="lowerBoundsCount">The initial capacity for <see cref="LowerBounds"/> list.</param>
      public ComplexArrayInfo( Int32 sizesCount = 0, Int32 lowerBoundsCount = 0 )
      {
         this.Sizes = new List<Int32>( Math.Max( 0, sizesCount ) );
         this.LowerBounds = new List<Int32>( Math.Max( 0, lowerBoundsCount ) );
      }

      /// <summary>
      /// Creates a new instance of <see cref="ComplexArrayInfo"/> with the values copied from given <see cref="ComplexArrayInfo"/>.
      /// </summary>
      /// <param name="other">The other <see cref="ComplexArrayInfo"/>. May be <c>null</c>.</param>
      public ComplexArrayInfo( ComplexArrayInfo other )
         : this( other?.Sizes.Count ?? 0, other?.LowerBounds.Count ?? 0 )
      {
         this.Rank = other?.Rank ?? 0;
         if ( other != null )
         {
            this.Sizes.AddRange( other.Sizes );
            this.LowerBounds.AddRange( other.LowerBounds );
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

      ///// <inheritdoc/>
      //public override Boolean Equals( Object obj )
      //{
      //   return this.Equals( obj as ComplexArrayInfo );
      //}

      ///// <inheritdoc/>
      //public override Int32 GetHashCode()
      //{
      //   return ( ( 17 * 23 + this.Rank ) * 23 + ListEqualityComparer<List<Int32>, Int32>.GetHashCode( this.Sizes ) ) * 23;
      //}

      ///// <inheritdoc/>
      //public Boolean Equals( ComplexArrayInfo other )
      //{
      //   return ReferenceEquals(this, other)
      //      || (other != null &&this.Rank == other.Rank
      //      && ListEqualityComparer<List<Int32>, Int32>.ListEquality( this.Sizes, other.Sizes )
      //      && ListEqualityComparer<List<Int32>, Int32>.ListEquality( this.LowerBounds, other.LowerBounds )
      //      );
      //}
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
   /// <remarks>
   /// The instances of this class can not be directly instantiated.
   /// Instead, use one of the following:
   /// <list type="bullet">
   /// <item><description><see cref="SimpleMarshalingInfo"/>,</description></item>
   /// <item><description><see cref="FixedLengthStringMarshalingInfo"/>,</description></item>
   /// <item><description><see cref="InterfaceMarshalingInfo"/>,</description></item>
   /// <item><description><see cref="SafeArrayMarshalingInfo"/>,</description></item>
   /// <item><description><see cref="FixedLengthArrayMarshalingInfo"/>,</description></item>
   /// <item><description><see cref="ArrayMarshalingInfo"/>,</description></item>
   /// <item><description><see cref="CustomMarshalingInfo"/>, or</description></item>
   /// <item><description><see cref="RawMarshalingInfo"/>.</description></item>
   /// </list>
   /// </remarks>
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
                  SizeParameterMultiplier = -1
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
   /// <remarks>
   /// The <see cref="AbstractMarshalingInfo.Value"/> should be one of the following:
   /// <list type="bullet">
   /// <item><description><see cref="UnmanagedType.Bool"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.I1"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.U1"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.I2"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.U2"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.I4"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.U4"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.I8"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.U8"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.R4"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.R8"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.LPStr"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.LPWStr"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.SysInt"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.SysUInt"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.FunctionPtr"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.Currency"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.BStr"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.Struct"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.VBByRefStr"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.AnsiBStr"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.TBStr"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.VariantBool"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.AsAny"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.LPStruct"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.Error"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.IInspectable"/> or</description></item>
   /// <item><description><see cref="UnmanagedType.HString"/>.</description></item>
   /// </list>
   /// </remarks>
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

   /// <summary>
   /// This class represents marshaling parameter or field as fixed-length string.
   /// </summary>
   /// <remarks>
   /// The <see cref="AbstractMarshalingInfo.Value"/> should be <see cref="UnmanagedType.ByValTStr"/>.
   /// </remarks>
   public sealed class FixedLengthStringMarshalingInfo : AbstractMarshalingInfo
   {
      /// <summary>
      /// Returns the <see cref="MarshalingInfoKind.FixedLengthString"/>.
      /// </summary>
      /// <value>The <see cref="MarshalingInfoKind.FixedLengthString"/>.</value>
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

   /// <summary>
   /// This class represents marshaling parameter or field as (COM) interface.
   /// </summary>
   /// <remarks>
   /// The <see cref="AbstractMarshalingInfo.Value"/> should be one of the following:
   /// <list type="bullet">
   /// <item><description><see cref="UnmanagedType.IUnknown"/>,</description></item>
   /// <item><description><see cref="UnmanagedType.IDispatch"/>, or</description></item>
   /// <item><description><see cref="UnmanagedType.Interface"/>.</description></item>
   /// </list>
   /// </remarks>
   public sealed class InterfaceMarshalingInfo : AbstractMarshalingInfo
   {
      /// <summary>
      /// Returns the <see cref="MarshalingInfoKind.Interface"/>.
      /// </summary>
      /// <value>The <see cref="MarshalingInfoKind.Interface"/>.</value>
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

   /// <summary>
   /// This class represents marshaling parameter or field as safe array.
   /// </summary>
   /// <remarks>
   /// The <see cref="AbstractMarshalingInfo.Value"/> should be <see cref="UnmanagedType.SafeArray"/>.
   /// </remarks>
   public sealed class SafeArrayMarshalingInfo : AbstractMarshalingInfo
   {
      /// <summary>
      /// Returns the <see cref="MarshalingInfoKind.SafeArray"/>.
      /// </summary>
      /// <value>The <see cref="MarshalingInfoKind.SafeArray"/>.</value>
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

   /// <summary>
   /// This class represents marshaling parameter or field as fixed-length array.
   /// </summary>
   /// <remarks>
   /// The <see cref="AbstractMarshalingInfo.Value"/> should be <see cref="UnmanagedType.ByValArray"/>.
   /// </remarks>
   public sealed class FixedLengthArrayMarshalingInfo : AbstractMarshalingInfo
   {
      /// <summary>
      /// Returns the <see cref="MarshalingInfoKind.FixedLengthArray"/>.
      /// </summary>
      /// <value>The <see cref="MarshalingInfoKind.FixedLengthArray"/>.</value>
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

   /// <summary>
   /// This class represents marshaling parameter or field as variable-length array.
   /// </summary>
   /// <remarks>
   /// The <see cref="AbstractMarshalingInfo.Value"/> should be <see cref="UnmanagedType.LPArray"/>.
   /// </remarks>
   public sealed class ArrayMarshalingInfo : AbstractMarshalingInfo
   {
      /// <summary>
      /// Returns the <see cref="MarshalingInfoKind.Array"/>.
      /// </summary>
      /// <value>The <see cref="MarshalingInfoKind.Array"/>.</value>
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
      /// Gets or sets the size parameter multiplier for this marshaling info.
      /// </summary>
      /// <value>The the size parameter multiplier for this marshaling info.</value>
      public Int32 SizeParameterMultiplier { get; set; }
   }

   /// <summary>
   /// This class represents marshaling parameter or field with customized marshaling callbacks.
   /// </summary>
   /// <remarks>
   /// The <see cref="AbstractMarshalingInfo.Value"/> should be <see cref="UnmanagedType.CustomMarshaler"/>.
   /// </remarks>
   public sealed class CustomMarshalingInfo : AbstractMarshalingInfo
   {
      /// <summary>
      /// Returns the <see cref="MarshalingInfoKind.Custom"/>.
      /// </summary>
      /// <value>The <see cref="MarshalingInfoKind.Custom"/>.</value>
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
      /// <remarks>
      /// The type should implement the <see cref="T:System.Runtime.InteropServices.ICustomMarshaler"/> interface.
      /// </remarks>
      public String CustomMarshalerTypeName { get; set; }

      /// <summary>
      /// Gets or sets the additional information for custom marshaler.
      /// </summary>
      /// <value>The additional information for custom marshaler.</value>
      public String MarshalCookie { get; set; }
   }

   /// <summary>
   /// This class represents marshaling parameter or field as unstructured information.
   /// </summary>
   /// <remarks>
   /// This class is typically used when the marshaling blob is unexpressable with this framework.
   /// </remarks>
   public sealed class RawMarshalingInfo : AbstractMarshalingInfo
   {
      /// <summary>
      /// Returns the <see cref="MarshalingInfoKind.Raw"/>.
      /// </summary>
      /// <value>The <see cref="MarshalingInfoKind.Raw"/>.</value>
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

   /// <summary>
   /// This is base class for custom attribute data (signature).
   /// </summary>
   /// <remarks>
   /// The instances of this class can not be directly instantiated.
   /// Instead, use one of the following:
   /// <list type="bullet">
   /// <item><description><see cref="ResolvedCustomAttributeSignature"/>, or</description></item>
   /// <item><description><see cref="RawCustomAttributeSignature"/>.</description></item>
   /// </list>
   /// </remarks>
   public abstract class AbstractCustomAttributeSignature
   {
      // Disable inheritance to other assemblies
      internal AbstractCustomAttributeSignature()
      {

      }

      /// <summary>
      /// Gets the <see cref="Physical.CustomAttributeSignatureKind"/> enumeration descripting the actual type of this <see cref="AbstractCustomAttributeSignature"/>.
      /// </summary>
      /// <value>The <see cref="Physical.CustomAttributeSignatureKind"/> enumeration descripting the actual type of this <see cref="AbstractCustomAttributeSignature"/>.</value>
      /// <seealso cref="Physical.CustomAttributeSignatureKind"/>
      public abstract CustomAttributeSignatureKind CustomAttributeSignatureKind { get; }
   }

   /// <summary>
   /// This enumeration tells what type instance of <see cref="AbstractCustomAttributeSignature"/> really is.
   /// </summary>
   public enum CustomAttributeSignatureKind
   {
      /// <summary>
      /// The <see cref="AbstractCustomAttributeSignature"/> is of type <see cref="RawCustomAttributeSignature"/>.
      /// </summary>
      Raw,

      /// <summary>
      /// The <see cref="AbstractCustomAttributeSignature"/> is of type <see cref="ResolvedCustomAttributeSignature"/>.
      /// </summary>
      Resolved
   }

   /// <summary>
   /// This class represents unresolved custom attribute signature, where all data is represented as byte array.
   /// </summary>
   public sealed class RawCustomAttributeSignature : AbstractCustomAttributeSignature
   {
      /// <summary>
      /// Gets or sets the binary data for this <see cref="RawCustomAttributeSignature"/>.
      /// </summary>
      /// <value>The binary data for this <see cref="RawCustomAttributeSignature"/>.</value>
      public Byte[] Bytes { get; set; }

      /// <summary>
      /// Returns the <see cref="CustomAttributeSignatureKind.Raw"/>.
      /// </summary>
      /// <value>The <see cref="CustomAttributeSignatureKind.Raw"/>.</value>
      public override CustomAttributeSignatureKind CustomAttributeSignatureKind
      {
         get
         {
            return CustomAttributeSignatureKind.Raw;
         }
      }
   }

   /// <summary>
   /// This class represents resolved custom attribute signature, where all data is accessible through list of <see cref="CustomAttributeTypedArgument"/>s and <see cref="CustomAttributeNamedArgument"/>s.
   /// </summary>
   public sealed class ResolvedCustomAttributeSignature : AbstractCustomAttributeSignature
   {

      /// <summary>
      /// Creates a new instance of <see cref="ResolvedCustomAttributeSignature"/>, with given initial capacities for <see cref="TypedArguments"/> and <see cref="NamedArguments"/> lists.
      /// </summary>
      /// <param name="typedArgsCount">The initial capacity for <see cref="TypedArguments"/> list.</param>
      /// <param name="namedArgsCount">The initial capacity for <see cref="NamedArguments"/> list.</param>
      public ResolvedCustomAttributeSignature( Int32 typedArgsCount = 0, Int32 namedArgsCount = 0 )
      {
         this.TypedArguments = new List<CustomAttributeTypedArgument>( Math.Max( 0, typedArgsCount ) );
         this.NamedArguments = new List<CustomAttributeNamedArgument>( Math.Max( 0, namedArgsCount ) );
      }

      /// <summary>
      /// Returns the <see cref="CustomAttributeSignatureKind.Resolved"/>.
      /// </summary>
      /// <value>The <see cref="CustomAttributeSignatureKind.Resolved"/>.</value>
      public override CustomAttributeSignatureKind CustomAttributeSignatureKind
      {
         get
         {
            return CustomAttributeSignatureKind.Resolved;
         }
      }

      /// <summary>
      /// Gets the list of all <see cref="CustomAttributeTypedArgument"/>s for this <see cref="ResolvedCustomAttributeSignature"/>.
      /// </summary>
      /// <value>The list of all <see cref="CustomAttributeTypedArgument"/>s for this <see cref="ResolvedCustomAttributeSignature"/>.</value>
      /// <seealso cref="CustomAttributeTypedArgument"/>
      public List<CustomAttributeTypedArgument> TypedArguments { get; }

      /// <summary>
      /// Gets the list of all <see cref="CustomAttributeNamedArgument"/>s for this <see cref="ResolvedCustomAttributeSignature"/>.
      /// </summary>
      /// <value>The list of all <see cref="CustomAttributeNamedArgument"/>s for this <see cref="ResolvedCustomAttributeSignature"/>.</value>
      /// <seealso cref="CustomAttributeNamedArgument"/>
      public List<CustomAttributeNamedArgument> NamedArguments { get; }
   }

   /// <summary>
   /// This class represents an implicitly-typed argument for a constructor in custom attribute signature.
   /// </summary>
   public sealed class CustomAttributeTypedArgument
   {
      /// <summary>
      /// Gets or sets the value for this <see cref="CustomAttributeTypedArgument"/>.
      /// </summary>
      /// <value>The value for this <see cref="CustomAttributeTypedArgument"/>.</value>
      /// <remarks>
      /// The type for non-null value should be one of the following:
      /// <list type="bullet">
      /// <item><description><see cref="Boolean"/> for <c>bool</c> values,</description></item>
      /// <item><description><see cref="Char"/> for <c>char</c> values,</description></item>
      /// <item><description><see cref="SByte"/> for <c>sbyte</c> values,</description></item>
      /// <item><description><see cref="Byte"/> for <c>byte</c> values,</description></item>
      /// <item><description><see cref="Int16"/> for <c>short</c> values,</description></item>
      /// <item><description><see cref="UInt16"/> for <c>ushort</c> values,</description></item>
      /// <item><description><see cref="Int32"/> for <c>int</c> values,</description></item>
      /// <item><description><see cref="UInt32"/> for <c>uint</c> values,</description></item>
      /// <item><description><see cref="Int64"/> for <c>long</c> values,</description></item>
      /// <item><description><see cref="UInt64"/> for <c>ulong</c> values,</description></item>
      /// <item><description><see cref="Single"/> for <c>float</c> values,</description></item>
      /// <item><description><see cref="Double"/> for <c>double</c> values,</description></item>
      /// <item><description><see cref="String"/> for <c>string</c> values,</description></item>
      /// <item><description><see cref="CustomAttributeValue_TypeReference"/> for references to other types,</description></item>
      /// <item><description><see cref="CustomAttributeValue_EnumReference"/> for references to enumeration values, or</description></item>
      /// <item><description><see cref="CustomAttributeValue_Array"/> for arrays.</description></item>
      /// </list>
      /// </remarks>
      public Object Value { get; set; }
   }

   /// <summary>
   /// This enumeration tells what type instance of <see cref="CustomAttributeTypedArgumentValueComplex"/> really is.
   /// </summary>
   public enum CustomAttributeTypedArgumentValueKind
   {
      /// <summary>
      /// The <see cref="CustomAttributeTypedArgumentValueComplex"/> is of type <see cref="CustomAttributeValue_TypeReference"/>.
      /// </summary>
      Type,

      /// <summary>
      /// The <see cref="CustomAttributeTypedArgumentValueComplex"/> is of type <see cref="CustomAttributeValue_EnumReference"/>.
      /// </summary>
      Enum,

      /// <summary>
      /// The <see cref="CustomAttributeTypedArgumentValueComplex"/> is of type <see cref="CustomAttributeValue_Array"/>.
      /// </summary>
      Array
   }

   /// <summary>
   /// This is common interface for non-primitive value storable in <see cref="CustomAttributeTypedArgument.Value"/>.
   /// </summary>
   /// <remarks>
   /// The following three structs implement this interface:
   /// <list type="bullet">
   /// <item><description><see cref="CustomAttributeValue_TypeReference"/>,</description></item>
   /// <item><description><see cref="CustomAttributeValue_EnumReference"/>, and</description></item>
   /// <item><description><see cref="CustomAttributeValue_Array"/>.</description></item>
   /// </list>
   /// </remarks>
   public interface CustomAttributeTypedArgumentValueComplex
   {
      // TODO investigate whether it is truly necessary for implementing types to be structs.

      /// <summary>
      /// Gets the <see cref="Physical.CustomAttributeTypedArgumentValueKind"/> enumeration descripting the actual type of this <see cref="CustomAttributeTypedArgumentValueComplex"/>.
      /// </summary>
      /// <value>The <see cref="Physical.CustomAttributeTypedArgumentValueKind"/> enumeration descripting the actual type of this <see cref="CustomAttributeTypedArgumentValueComplex"/>.</value>
      /// <seealso cref="Physical.CustomAttributeTypedArgumentValueKind"/>
      CustomAttributeTypedArgumentValueKind CustomAttributeTypedArgumentValueKind { get; }
   }

   /// <summary>
   /// This struct should be using when storing type references as custom attribute values in <see cref="CustomAttributeTypedArgument.Value"/>.
   /// </summary>
   public struct CustomAttributeValue_TypeReference : IEquatable<CustomAttributeValue_TypeReference>, CustomAttributeTypedArgumentValueComplex
   {
      /// <summary>
      /// Creates a new instance of <see cref="CustomAttributeValue_TypeReference"/> with given type string.
      /// </summary>
      /// <param name="typeString">The full type string.</param>
      public CustomAttributeValue_TypeReference( String typeString )
      {
         this.TypeString = typeString;
      }

      /// <summary>
      /// Returns the <see cref="CustomAttributeTypedArgumentValueKind.Type"/>.
      /// </summary>
      /// <value>The <see cref="CustomAttributeTypedArgumentValueKind.Type"/>.</value>
      public CustomAttributeTypedArgumentValueKind CustomAttributeTypedArgumentValueKind
      {
         get
         {
            return CustomAttributeTypedArgumentValueKind.Type;
         }
      }

      /// <summary>
      /// Gets the type string of this <see cref="CustomAttributeValue_TypeReference"/>.
      /// </summary>
      public String TypeString { get; }

      /// <summary>
      /// Checks that given object is of type <see cref="CustomAttributeValue_TypeReference"/> and that their data equals.
      /// </summary>
      /// <param name="obj">The object to check.</param>
      /// <returns><c>true</c> if <paramref name="obj"/> is of type <see cref="CustomAttributeValue_TypeReference"/> and its data matches data of this object; <c>false</c> otherwise.</returns>
      public override Boolean Equals( Object obj )
      {
         return obj is CustomAttributeValue_TypeReference && this.Equals( (CustomAttributeValue_TypeReference) obj );
      }

      /// <summary>
      /// Computes the hash code of this <see cref="CustomAttributeValue_TypeReference"/>.
      /// </summary>
      /// <returns>The hash code of this <see cref="CustomAttributeValue_TypeReference"/>.</returns>
      public override Int32 GetHashCode()
      {
         return this.TypeString.GetHashCodeSafe();
      }

      /// <summary>
      /// Checks that this <see cref="CustomAttributeValue_TypeReference"/>s and others data matches.
      /// </summary>
      /// <param name="other">The other <see cref="CustomAttributeValue_TypeReference"/>.</param>
      /// <returns><c>true</c> if data matches; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// The data is considered to match when the <see cref="TypeString"/> of both matches exactly and case-sensitively.
      /// </remarks>
      public Boolean Equals( CustomAttributeValue_TypeReference other )
      {
         return String.Equals( this.TypeString, other.TypeString );
      }
   }

   /// <summary>
   /// This struct should be using when storing enum type values as custom attribute values in <see cref="CustomAttributeTypedArgument.Value"/>.
   /// </summary>
   public struct CustomAttributeValue_EnumReference : IEquatable<CustomAttributeValue_EnumReference>, CustomAttributeTypedArgumentValueComplex
   {

      /// <summary>
      /// Creates a new instance of <see cref="CustomAttributeValue_EnumReference"/> with given enum type string and value.
      /// </summary>
      /// <param name="enumType">The full type string of the enum type.</param>
      /// <param name="enumValue">The enum value.</param>
      public CustomAttributeValue_EnumReference( String enumType, Object enumValue )
      {
         this.EnumType = enumType;
         this.EnumValue = enumValue;
      }

      /// <summary>
      /// Returns the <see cref="CustomAttributeTypedArgumentValueKind.Enum"/>.
      /// </summary>
      /// <value>The <see cref="CustomAttributeTypedArgumentValueKind.Enum"/>.</value>
      public CustomAttributeTypedArgumentValueKind CustomAttributeTypedArgumentValueKind
      {
         get
         {
            return CustomAttributeTypedArgumentValueKind.Enum;
         }
      }

      /// <summary>
      /// Gets the enum type string for this <see cref="CustomAttributeValue_EnumReference"/>.
      /// </summary>
      /// <value>The enum type string for this <see cref="CustomAttributeValue_EnumReference"/>.</value>
      public String EnumType { get; }

      /// <summary>
      /// Gets the enum value for this <see cref="CustomAttributeValue_EnumReference"/>.
      /// </summary>
      /// <value>The enum value for this <see cref="CustomAttributeValue_EnumReference"/>.</value>
      public Object EnumValue { get; }

      /// <summary>
      /// Checks that given object is of type <see cref="CustomAttributeValue_EnumReference"/> and that their data equals.
      /// </summary>
      /// <param name="obj">The object to check.</param>
      /// <returns><c>true</c> if <paramref name="obj"/> is of type <see cref="CustomAttributeValue_EnumReference"/> and its data matches data of this object; <c>false</c> otherwise.</returns>
      public override Boolean Equals( Object obj )
      {
         return obj is CustomAttributeValue_EnumReference && this.Equals( (CustomAttributeValue_EnumReference) obj );
      }

      /// <summary>
      /// Computes the hash code for this <see cref="CustomAttributeValue_EnumReference"/>.
      /// </summary>
      /// <returns>The hash code for this <see cref="CustomAttributeValue_EnumReference"/>.</returns>
      public override Int32 GetHashCode()
      {
         return ( 17 * 23 + this.EnumType.GetHashCodeSafe() ) * 23 + this.EnumValue.GetHashCodeSafe();
      }

      /// <summary>
      /// Checks that this <see cref="CustomAttributeValue_EnumReference"/>s and others data matches.
      /// </summary>
      /// <param name="other">The other <see cref="CustomAttributeValue_EnumReference"/>.</param>
      /// <returns><c>true</c> if data matches; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// The data is considered to match when all of the following condition become true:
      /// <list type="bullet">
      /// <item><description>The <see cref="EnumType"/> strings match exactly and case-sensitively, and</description></item>
      /// <item><description>The <see cref="EnumValue"/> objects equal using their <see cref="Equals(object)"/> method.</description></item>
      /// </list>
      /// </remarks>
      public Boolean Equals( CustomAttributeValue_EnumReference other )
      {
         return String.Equals( this.EnumType, other.EnumType )
            && Equals( this.EnumValue, other.EnumValue );
      }
   }

   /// <summary>
   /// This struct should be using when storing arrays as custom attribute values in <see cref="CustomAttributeTypedArgument.Value"/>.
   /// </summary>
   public struct CustomAttributeValue_Array : IEquatable<CustomAttributeValue_Array>, CustomAttributeTypedArgumentValueComplex
   {
      /// <summary>
      /// Creates a new instance of <see cref="CustomAttributeValue_Array"/> with given array and array element type.
      /// </summary>
      /// <param name="array">The array of values.</param>
      /// <param name="arrayElementType">The array element type.</param>
      /// <seealso cref="CustomAttributeArgumentType"/>
      public CustomAttributeValue_Array( Array array, CustomAttributeArgumentType arrayElementType )
      {
         this.Array = array;
         this.ArrayElementType = arrayElementType;
      }

      /// <summary>
      /// Creates a new instance of <see cref="CustomAttributeValue_Array"/> with given array, and tries to deduce the array element type automatically.
      /// </summary>
      /// <param name="array">The array of values.</param>
      /// <param name="sigProvider">The <see cref="Meta.SignatureProvider"/> to use to deduce the array element type automatically.</param>
      /// <seealso cref="E_CILPhysical.ResolveCAArgumentTypeFromObject"/>
      public CustomAttributeValue_Array( Array array, Meta.SignatureProvider sigProvider )
      {
         this.Array = array;
         this.ArrayElementType = sigProvider.ResolveCAArgumentTypeFromObject( array );
      }

      /// <summary>
      /// Returns the <see cref="CustomAttributeTypedArgumentValueKind.Enum"/>.
      /// </summary>
      /// <value>The <see cref="CustomAttributeTypedArgumentValueKind.Enum"/>.</value>
      public CustomAttributeTypedArgumentValueKind CustomAttributeTypedArgumentValueKind
      {
         get
         {
            return CustomAttributeTypedArgumentValueKind.Array;
         }
      }

      /// <summary>
      /// Gets the array of this <see cref="CustomAttributeValue_Array"/>.
      /// </summary>
      /// <value>The array of this <see cref="CustomAttributeValue_Array"/>.</value>
      public Array Array { get; }

      /// <summary>
      /// Gets the array element type, as <see cref="CustomAttributeArgumentType"/>.
      /// </summary>
      /// <value>The array element type, as <see cref="CustomAttributeArgumentType"/>.</value>
      public CustomAttributeArgumentType ArrayElementType { get; }

      /// <summary>
      /// Checks that given object is of type <see cref="CustomAttributeValue_Array"/> and that their data equals.
      /// </summary>
      /// <param name="obj">The object to check.</param>
      /// <returns><c>true</c> if <paramref name="obj"/> is of type <see cref="CustomAttributeValue_Array"/> and its data matches data of this object; <c>false</c> otherwise.</returns>
      public override Boolean Equals( Object obj )
      {
         return obj is CustomAttributeValue_Array && this.Equals( (CustomAttributeValue_Array) obj );
      }

      /// <summary>
      /// Computes the hash code for this <see cref="CustomAttributeValue_Array"/>.
      /// </summary>
      /// <returns>The hash code for this <see cref="CustomAttributeValue_Array"/>.</returns>
      public override Int32 GetHashCode()
      {
         return ( 17 * 23 + this.ArrayElementType.GetHashCodeSafe() ) * 23 + SequenceEqualityComparer<IEnumerable<Object>, Object>.SequenceHashCode( this.Array?.Cast<Object>() );
      }

      /// <summary>
      /// Checks that this <see cref="CustomAttributeValue_EnumReference"/>s and others data matches.
      /// </summary>
      /// <param name="other">The other <see cref="CustomAttributeValue_EnumReference"/>.</param>
      /// <returns><c>true</c> if data matches; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// The data is considered to match when all of the following condition become true:
      /// <list type="bullet">
      /// <item><description>The <see cref="ArrayElementType"/> are equal</description></item>
      /// <item><description>The <see cref="Array"/> objects equal using <see cref="Equals(object)"/> on their elements.</description></item>
      /// </list>
      /// </remarks>
      public Boolean Equals( CustomAttributeValue_Array other )
      {
         return this.ArrayElementType.EqualsTyped( other.ArrayElementType )
            && SequenceEqualityComparer<IEnumerable<Object>, Object>.SequenceEquality( this.Array.Cast<Object>(), other.Array.Cast<Object>() );
      }

   }

   /// <summary>
   /// This class represents a named argument (field or property) in custom attribute signature.
   /// </summary>
   public sealed class CustomAttributeNamedArgument
   {
      /// <summary>
      /// Gets or sets the <see cref="CustomAttributeTypedArgument"/> holding the value for this <see cref="CustomAttributeNamedArgument"/>.
      /// </summary>
      /// <value>The <see cref="CustomAttributeTypedArgument"/> holding the value for this <see cref="CustomAttributeNamedArgument"/>.</value>
      /// <seealso cref="CustomAttributeTypedArgument"/>
      public CustomAttributeTypedArgument Value { get; set; }

      /// <summary>
      /// Gets or sets the type describing the target field or property, as <see cref="CustomAttributeArgumentType"/>.
      /// </summary>
      /// <value>The type describing the target field or property, as <see cref="CustomAttributeArgumentType"/>.</value>
      /// <seealso cref="CustomAttributeArgumentType"/>
      public CustomAttributeArgumentType FieldOrPropertyType { get; set; }

      /// <summary>
      /// Gets or sets the name of the target field or property.
      /// </summary>
      /// <value>The name of the target field or property.</value>
      public String Name { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="CustomAttributeNamedArgumentTarget"/> of this <see cref="CustomAttributeNamedArgument"/>.
      /// </summary>
      /// <value>The <see cref="CustomAttributeNamedArgumentTarget"/> of this <see cref="CustomAttributeNamedArgument"/>.</value>
      /// <seealso cref="CustomAttributeNamedArgumentTarget"/>
      public CustomAttributeNamedArgumentTarget TargetKind { get; set; }
   }

   /// <summary>
   /// This enumeration contains possible values for <see cref="CustomAttributeNamedArgument.TargetKind"/>.
   /// </summary>
   /// <remarks>
   /// The values of this enumeration are safe to be casted to <see cref="T:CILAssemblyManipulator.Physical.IO.SignatureElementTypes"/>.
   /// </remarks>
   public enum CustomAttributeNamedArgumentTarget : byte
   {
      /// <summary>
      /// The target element is a field.
      /// </summary>
      Field = 0x53,

      /// <summary>
      /// The target element is a property.
      /// </summary>
      Property = 0x54
   }

   /// <summary>
   /// This enumeration tells what type instance of <see cref="CustomAttributeArgumentType"/> really is.
   /// </summary>
   public enum CustomAttributeArgumentTypeKind
   {
      /// <summary>
      /// The <see cref="CustomAttributeArgumentType"/> is of type <see cref="CustomAttributeArgumentTypeSimple"/>.
      /// </summary>
      Simple,

      /// <summary>
      /// The <see cref="CustomAttributeArgumentType"/> is of type <see cref="CustomAttributeArgumentTypeEnum"/>.
      /// </summary>
      Enum,

      /// <summary>
      /// The <see cref="CustomAttributeArgumentType"/> is of type <see cref="CustomAttributeArgumentTypeArray"/>.
      /// </summary>
      Array
   }

   /// <summary>
   /// This is base class for type information about values in custom attribute signatures.
   /// Primarily used by <see cref="CustomAttributeNamedArgument.FieldOrPropertyType"/> and <see cref="CustomAttributeValue_Array.ArrayElementType"/>.
   /// </summary>
   /// <remarks>
   /// The instances of this class can not be directly instantiated.
   /// Instead, use one of the following:
   /// <list type="bullet">
   /// <item><description><see cref="CustomAttributeArgumentTypeSimple"/>,</description></item>
   /// <item><description><see cref="CustomAttributeArgumentTypeEnum"/>, or</description></item>
   /// <item><description><see cref="CustomAttributeArgumentTypeArray"/>.</description></item>
   /// </list>
   /// </remarks>
   public abstract class CustomAttributeArgumentType
   {
      // Disable inheritance to other assemblies
      internal CustomAttributeArgumentType()
      {

      }

      /// <summary>
      /// Gets the <see cref="Physical.CustomAttributeArgumentTypeKind"/> enumeration descripting the actual type of this <see cref="CustomAttributeArgumentType"/>.
      /// </summary>
      /// <value>The <see cref="Physical.CustomAttributeArgumentTypeKind"/> enumeration descripting the actual type of this <see cref="CustomAttributeArgumentType"/>.</value>
      /// <seealso cref="Physical.CustomAttributeArgumentTypeKind"/>
      public abstract CustomAttributeArgumentTypeKind ArgumentTypeKind { get; }



   }

   /// <summary>
   /// This is class to represent a simple type in <see cref="CustomAttributeNamedArgument.FieldOrPropertyType"/> and <see cref="CustomAttributeValue_Array.ArrayElementType"/>.
   /// </summary>
   /// <remarks>
   /// The instances of this class should be obtained through <see cref="Meta.SignatureProvider.GetSimpleCATypeOrNull"/> or <see cref="E_CILPhysical.GetSimpleTypeSignature"/> methods.
   /// This is to save memory - no need to allocate duplicate <see cref="CustomAttributeArgumentTypeSimple"/> objects with identical state (assuming <see cref="Meta.SignatureProvider"/> caches the <see cref="CustomAttributeArgumentTypeSimple"/>s).
   /// </remarks>
   public sealed class CustomAttributeArgumentTypeSimple : CustomAttributeArgumentType
   {
      /// <summary>
      /// Creates a new instance of <see cref="CustomAttributeArgumentTypeSimple"/> with specified <see cref="CustomAttributeArgumentTypeSimpleKind"/>.
      /// </summary>
      /// <param name="kind">The <see cref="CustomAttributeArgumentTypeSimpleKind"/>.</param>
      /// <remarks>
      /// The only place where this constructor should be used is by types implementing <see cref="Meta.SignatureProvider"/>.
      /// </remarks>
      public CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind kind )
      {
         this.SimpleType = kind;
      }

      /// <summary>
      /// Returns the <see cref="CustomAttributeArgumentTypeKind.Simple"/>.
      /// </summary>
      /// <value>The <see cref="CustomAttributeArgumentTypeKind.Simple"/>.</value>
      public override CustomAttributeArgumentTypeKind ArgumentTypeKind
      {
         get
         {
            return CustomAttributeArgumentTypeKind.Simple;
         }
      }

      /// <summary>
      /// Gets the <see cref="CustomAttributeArgumentTypeSimpleKind"/> of this <see cref="CustomAttributeArgumentTypeSimple"/>.
      /// </summary>
      /// <value>The <see cref="CustomAttributeArgumentTypeSimpleKind"/> of this <see cref="CustomAttributeArgumentTypeSimple"/>.</value>
      public CustomAttributeArgumentTypeSimpleKind SimpleType { get; }

   }

   /// <summary>
   /// This enumeration represents the kind of <see cref="CustomAttributeArgumentTypeSimple"/>.
   /// </summary>
   /// <remarks>
   /// The values of this enumeration are safe to be casted to <see cref="T:CILAssemblyManipulator.Physical.IO.SignatureElementTypes"/>.
   /// </remarks>
   public enum CustomAttributeArgumentTypeSimpleKind : byte
   {
      /// <summary>
      /// This is type for <c>bool</c> values.
      /// </summary>
      Boolean = 0x02, // Same as SignatureElementTypes.Boolean

      /// <summary>
      /// This is type for <c>char</c> values.
      /// </summary>
      Char,

      /// <summary>
      /// This is type for <c>sbyte</c> values.
      /// </summary>
      I1,

      /// <summary>
      /// This is type for <c>byte</c> values.
      /// </summary>
      U1,

      /// <summary>
      /// This is type for <c>short</c> values.
      /// </summary>
      I2,

      /// <summary>
      /// This is type for <c>ushort</c> values.
      /// </summary>
      U2,

      /// <summary>
      /// This is type for <c>int</c> values.
      /// </summary>
      I4,

      /// <summary>
      /// This is type for <c>uint</c> values.
      /// </summary>
      U4,

      /// <summary>
      /// This is type for <c>long</c> values.
      /// </summary>
      I8,

      /// <summary>
      /// This is type for <c>ulong</c> values.
      /// </summary>
      U8,

      /// <summary>
      /// This is type for <c>float</c> values.
      /// </summary>
      R4,

      /// <summary>
      /// This is type for <c>double</c> values.
      /// </summary>
      R8,

      /// <summary>
      /// This is type for <c>string</c> values.
      /// </summary>
      String,

      /// <summary>
      /// This is type for <see cref="Type"/> values.
      /// </summary>
      Type = 0x50, // Same as SignatureElementTypes.Type

      /// <summary>
      /// This is type for boxed values.
      /// </summary>
      Object // Same as SignatureElementTypes.CA_Boxed
   }

   /// <summary>
   /// This is class to represent a enum type in <see cref="CustomAttributeNamedArgument.FieldOrPropertyType"/> and <see cref="CustomAttributeValue_Array.ArrayElementType"/>.
   /// </summary>
   public sealed class CustomAttributeArgumentTypeEnum : CustomAttributeArgumentType
   {
      /// <summary>
      /// Returns the <see cref="CustomAttributeArgumentTypeKind.Enum"/>.
      /// </summary>
      /// <value>The <see cref="CustomAttributeArgumentTypeKind.Enum"/>.</value>
      public override CustomAttributeArgumentTypeKind ArgumentTypeKind
      {
         get
         {
            return CustomAttributeArgumentTypeKind.Enum;
         }
      }

      /// <summary>
      /// Gets or sets the full type string of target enum type.
      /// </summary>
      /// <value>The full type string of target enum type.</value>
      public String TypeString { get; set; }
   }

   /// <summary>
   /// This is class to represent a simple array type in <see cref="CustomAttributeNamedArgument.FieldOrPropertyType"/> and <see cref="CustomAttributeValue_Array.ArrayElementType"/>.
   /// </summary>
   public sealed class CustomAttributeArgumentTypeArray : CustomAttributeArgumentType
   {
      /// <summary>
      /// Returns the <see cref="CustomAttributeArgumentTypeKind.Array"/>.
      /// </summary>
      /// <value>The <see cref="CustomAttributeArgumentTypeKind.Array"/>.</value>
      public override CustomAttributeArgumentTypeKind ArgumentTypeKind
      {
         get
         {
            return CustomAttributeArgumentTypeKind.Array;
         }
      }

      /// <summary>
      /// Gets or sets the element type for this array type.
      /// </summary>
      /// <value>The element type for this array type.</value>
      /// <seealso cref="CustomAttributeArgumentType"/>
      public CustomAttributeArgumentType ArrayType { get; set; }
   }

   /// <summary>
   /// This is base class for security informations present in <see cref="SecurityDefinition.PermissionSets"/> list.
   /// </summary>
   /// <remarks>
   /// The instances of this class can not be directly instantiated.
   /// Instead, use one of the following:
   /// <list type="bullet">
   /// <item><description><see cref="RawSecurityInformation"/>, or</description></item>
   /// <item><description><see cref="SecurityInformation"/>.</description></item>
   /// </list>
   /// </remarks>
   public abstract class AbstractSecurityInformation
   {
      // Disable inheritance to other assemblies
      internal AbstractSecurityInformation()
      {

      }

      /// <summary>
      /// Gets or sets the full type string of the security attribute.
      /// </summary>
      /// <value>The full type string of the security attribute.</value>
      public String SecurityAttributeType { get; set; }

      /// <summary>
      /// Gets the <see cref="Physical.SecurityInformationKind"/> enumeration descripting the actual type of this <see cref="AbstractSecurityInformation"/>.
      /// </summary>
      /// <value>The <see cref="Physical.SecurityInformationKind"/> enumeration descripting the actual type of this <see cref="AbstractSecurityInformation"/>.</value>
      /// <seealso cref="Physical.SecurityInformationKind"/>
      public abstract SecurityInformationKind SecurityInformationKind { get; }

   }

   /// <summary>
   /// This enumeration tells what type instance of <see cref="AbstractSecurityInformation"/> really is.
   /// </summary>
   public enum SecurityInformationKind
   {
      /// <summary>
      /// The <see cref="AbstractSecurityInformation"/> is of type <see cref="SecurityInformation"/>.
      /// </summary>
      Resolved,

      /// <summary>
      /// The <see cref="AbstractSecurityInformation"/> is of type <see cref="RawSecurityInformation"/>.
      /// </summary>
      Raw
   }

   /// <summary>
   /// This class represents the security information data as a byte array.
   /// </summary>
   public sealed class RawSecurityInformation : AbstractSecurityInformation
   {
      /// <summary>
      /// Gets or sets the amount of named <see cref="CustomAttributeNamedArgument"/>s the data of this <see cref="RawSecurityInformation"/> holds.
      /// </summary>
      /// <value>The amount of named <see cref="CustomAttributeNamedArgument"/>s the data of this <see cref="RawSecurityInformation"/> holds.</value>
      public Int32 ArgumentCount { get; set; }

      /// <summary>
      /// Gets or sets the raw data of this <see cref="RawSecurityInformation"/>.
      /// </summary>
      /// <value>The raw data of this <see cref="RawSecurityInformation"/>.</value>
      public Byte[] Bytes { get; set; }

      /// <summary>
      /// Returns the <see cref="SecurityInformationKind.Raw"/>.
      /// </summary>
      /// <value>The <see cref="SecurityInformationKind.Raw"/>.</value>
      public override SecurityInformationKind SecurityInformationKind
      {
         get
         {
            return SecurityInformationKind.Raw;
         }
      }
   }

   /// <summary>
   /// This class represents resolved security information, where data is available through list of <see cref="CustomAttributeNamedArgument"/>s.
   /// </summary>
   /// <seealso cref="CustomAttributeNamedArgument"/>
   public sealed class SecurityInformation : AbstractSecurityInformation
   {

      /// <summary>
      /// Creates a new instance of <see cref="SecurityInformation"/> with given initial capacity for <see cref="NamedArguments"/> list.
      /// </summary>
      /// <param name="namedArgumentsCount">The initial capacity for <see cref="NamedArguments"/> list.</param>
      public SecurityInformation( Int32 namedArgumentsCount = 0 )
      {
         this.NamedArguments = new List<CustomAttributeNamedArgument>( Math.Max( 0, namedArgumentsCount ) );
      }

      /// <summary>
      /// Gets the list of all <see cref="CustomAttributeNamedArgument"/>s of this security attribute declaration.
      /// </summary>
      /// <value>The list of all <see cref="CustomAttributeNamedArgument"/>s of this security attribute declaration.</value>
      /// <seealso cref="CustomAttributeNamedArgument"/>
      public List<CustomAttributeNamedArgument> NamedArguments { get; }

      /// <summary>
      /// Returns the <see cref="SecurityInformationKind.Resolved"/>.
      /// </summary>
      /// <value>The <see cref="SecurityInformationKind.Resolved"/>.</value>
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

   /// <summary>
   /// Tries to get the native <see cref="Type"/> for this <see cref="CustomAttributeArgumentType"/>.
   /// </summary>
   /// <param name="elemType">The <see cref="CustomAttributeArgumentType"/>.</param>
   /// <returns>The native type for given <see cref="CustomAttributeArgumentType"/>, where enums are represented with <see cref="CustomAttributeValue_EnumReference"/> and types with <see cref="CustomAttributeValue_TypeReference"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="elemType"/> is <c>null</c>.</exception>
   public static Type GetNativeTypeForCAArrayType( this CustomAttributeArgumentType elemType )
   {
      // TODO decide and document what to do with arrays.
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
         case CustomAttributeArgumentTypeKind.Enum:
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

   /// <summary>
   /// Creates a new instance of marshal info, which will contain a deep copy of this marshal info.
   /// </summary>
   /// <param name="marshal">The <see cref="AbstractMarshalingInfo"/>.</param>
   /// <returns>A new instance of the same type as given <see cref="AbstractMarshalingInfo"/>, which has all of its contents deeply copied from given signature.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="marshal"/> is <c>null.</c></exception>
   /// <exception cref="NotSupportedException">If <see cref="AbstractMarshalingInfo.MarshalingInfoKind"/> returns any other value than what the <see cref="MarshalingInfoKind"/> enumeration has.</exception>
   public static AbstractMarshalingInfo CreateDeepCopy( this AbstractMarshalingInfo marshal )
   {
      AbstractMarshalingInfo retVal;
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
               SizeParameterMultiplier = array.SizeParameterMultiplier
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
            throw new NotSupportedException( "Unrecognized marshal kind: " + mKind + "." );
      }


      return retVal;
   }

   /// <summary>
   /// Checks whether the given <see cref="CustomModifierSignatureOptionality"/> is <see cref="CustomModifierSignatureOptionality.Required"/>.
   /// </summary>
   /// <param name="customMod">The <see cref="CustomModifierSignatureOptionality"/>.</param>
   /// <returns><c>true</c> if <paramref name="customMod"/> is <see cref="CustomModifierSignatureOptionality.Required"/>; <c>false</c> otherwise.</returns>
   public static Boolean IsRequired( this CustomModifierSignatureOptionality customMod )
   {
      return customMod == CustomModifierSignatureOptionality.Required;
   }

   /// <summary>
   /// Checks whether the given <see cref="CustomModifierSignatureOptionality"/> is <see cref="CustomModifierSignatureOptionality.Optional"/>.
   /// </summary>
   /// <param name="customMod">The <see cref="CustomModifierSignatureOptionality"/>.</param>
   /// <returns><c>true</c> if <paramref name="customMod"/> is <see cref="CustomModifierSignatureOptionality.Optional"/>; <c>false</c> otherwise.</returns>
   public static Boolean IsOptional( this CustomModifierSignatureOptionality customMod )
   {
      return customMod == CustomModifierSignatureOptionality.Optional;
   }

   /// <summary>
   /// Checks whether the given <see cref="TypeReferenceKind"/> is <see cref="TypeReferenceKind.Class"/>.
   /// </summary>
   /// <param name="typeReferenceKind">The <see cref="TypeReferenceKind"/>.</param>
   /// <returns><c>true</c> if <paramref name="typeReferenceKind"/> is <see cref="TypeReferenceKind.Class"/>; <c>false</c> otherwise.</returns>
   public static Boolean IsClass( this TypeReferenceKind typeReferenceKind )
   {
      return typeReferenceKind == TypeReferenceKind.Class;
   }

   /// <summary>
   /// Checks whether the given <see cref="GenericParameterKind"/> is <see cref="GenericParameterKind.Type"/>.
   /// </summary>
   /// <param name="genericParameterKind">The <see cref="GenericParameterKind"/>.</param>
   /// <returns><c>true</c> if <paramref name="genericParameterKind"/> is <see cref="GenericParameterKind.Type"/>; <c>false</c> otherwise.</returns>
   public static Boolean IsTypeParameter( this GenericParameterKind genericParameterKind )
   {
      return genericParameterKind == GenericParameterKind.Type;
   }

   /// <summary>
   /// Checks whether the given <see cref="CustomAttributeNamedArgumentTarget"/> is <see cref="CustomAttributeNamedArgumentTarget.Field"/>.
   /// </summary>
   /// <param name="targetKind">The <see cref="CustomAttributeNamedArgumentTarget"/>.</param>
   /// <returns><c>true</c> if <paramref name="targetKind"/> is <see cref="CustomAttributeNamedArgumentTarget.Field"/>; <c>false</c> otherwise.</returns>
   public static Boolean IsField( this CustomAttributeNamedArgumentTarget targetKind )
   {
      return targetKind == CustomAttributeNamedArgumentTarget.Field;
   }

   /// <summary>
   /// Given a <see cref="TypeSignature"/> located in a given <see cref="CILMetaData"/>, tries to create a new <see cref="CustomAttributeArgumentType"/> which would logically match the given <see cref="TypeSignature"/>.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/> holding the type signature.</param>
   /// <param name="type">The <see cref="TypeSignature"/>.</param>
   /// <param name="enumFactory">The callback to create <see cref="CustomAttributeArgumentTypeEnum"/> from a given <see cref="TableIndex"/>.</param>
   /// <returns>An object of some type extending <see cref="CustomAttributeArgumentType"/>, or <c>null</c> if transformation was not successful.</returns>
   public static CustomAttributeArgumentType TypeSignatureToCustomAttributeArgumentType( this CILMetaData md, TypeSignature type, Func<TableIndex, CustomAttributeArgumentTypeEnum> enumFactory )
   {
      return md.TypeSignatureToCustomAttributeArgumentType( type, enumFactory, true );
   }


   private static CustomAttributeArgumentType TypeSignatureToCustomAttributeArgumentType( this CILMetaData md, TypeSignature type, Func<TableIndex, CustomAttributeArgumentTypeEnum> enumFactory, Boolean acceptArray )
   {
      var sigProvider = md.GetSignatureProvider();
      switch ( type.TypeSignatureKind )
      {
         case TypeSignatureKind.Simple:
            return sigProvider.GetSimpleCATypeOrNull( (CustomAttributeArgumentTypeSimpleKind) ( (SimpleTypeSignature) type ).SimpleType );
         case TypeSignatureKind.ClassOrValue:
            var clazz = (ClassOrValueTypeSignature) type;
            if ( clazz.GenericArguments.Count <= 0 )
            {
               if ( clazz.TypeReferenceKind.IsClass() )
               {
                  // Either type or System.Object or System.Type are allowed here
                  if ( md.IsTypeType( clazz.Type ) )
                  {
                     return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.Type );
                  }
                  else if ( md.IsSystemObjectType( clazz.Type ) )
                  {
                     return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.Object );
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
            if ( acceptArray )
            {
               var retVal = acceptArray ?
               new CustomAttributeArgumentTypeArray()
               {
                  ArrayType = md.TypeSignatureToCustomAttributeArgumentType( ( (SimpleArrayTypeSignature) type ).ArrayType, enumFactory, false )
               } :
               null;
               return retVal == null || retVal.ArrayType == null ? null : retVal;
            }
            else
            {
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.Object );
            }
         default:
            return null;
      }
   }

}