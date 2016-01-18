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
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Physical
{
   /// <summary>
   /// This enumeration holds values for assembly hash algorithm. See ECMA specification (and .NET 4.5 lib for SHA256+) for more information.
   /// </summary>
   public enum AssemblyHashAlgorithm
   {
      /// <summary>
      /// The assembly contents are not hashed.
      /// </summary>
      None = 0x0000,
      /// <summary>
      /// The assembly hash is calculated using <c>MD5</c> algorithm.
      /// </summary>
      MD5 = 0x8003,
      /// <summary>
      /// The assembly hash is calculated using <c>SHA1</c> algorithm.
      /// </summary>
      SHA1 = 0x8004,
      /// <summary>
      /// The assembly hash is calculated using <c>SHA256</c> algorithm.
      /// </summary>
      SHA256 = 0x800C,
      /// <summary>
      /// The assembly hash is calculated using <c>SHA384</c> algorithm.
      /// </summary>
      SHA384 = 0x800D,
      /// <summary>
      /// The assembly hash is calculated using <c>SHA512</c> algorithm.
      /// </summary>
      SHA512 = 0x800E
   }

   /// <summary>
   /// This enumeration holds values for assembly flags. See ECMA specification for more information.
   /// </summary>
   [Flags]
   public enum AssemblyFlags
   {
      /// <summary>
      /// The assembly has no flags.
      /// </summary>
      None = 0x0000,
      /// <summary>
      /// The assembly name holds full (unhashed) public key. 
      /// </summary>
      PublicKey = 0x0001,
      /// <summary>
      /// The assembly version information may be different at compile time than at runtime.
      /// </summary>
      Retargetable = 0x0100,
      /// <summary>
      /// Reserved (use at own caution).
      /// </summary>
      DisableJITCompileOptimizer = 0x4000,
      /// <summary>
      /// Reserved (use at own caution).
      /// </summary>
      EnableJITCompileTracking = 0x8000
   }

   /// <summary>
   /// This enumeration holds values for file attributes for files referenced by assembly. See ECMA specification for more information.
   /// </summary>
   public enum FileAttributes
   {
      /// <summary>
      /// The file is not a resource file.
      /// </summary>
      ContainsMetadata = 0x0000,
      /// <summary>
      /// The file is a resource file or other non-metadata-containing file.
      /// </summary>
      ContainsNoMetadata = 0x0001
   }

   /// <summary>
   /// This enumeration holds values of PInvoke attributes associated with PInvoke methods/fields. See ECMA specification for more information.
   /// </summary>
   [Flags]
   public enum PInvokeAttributes
   {
      /// <summary>
      /// PInvoke is to use member name as specified.
      /// </summary>
      NoMangle = 0x0001,
      /// <summary>
      /// The mask value for charsets (2 bits).
      /// </summary>
      CharsetMask = 0x0006,
      /// <summary>
      /// Charset is not specified.
      /// </summary>
      CharsetNotSpec = 0x0000,
      /// <summary>
      /// ANSI charset should be used.
      /// </summary>
      CharsetAnsi = 0x0002,
      /// <summary>
      /// Unicode charset should be used.
      /// </summary>
      CharsetUnicode = 0x0004,
      /// <summary>
      /// Charset should be deduced automatically.
      /// </summary>
      CharsetAuto = 0x0006,
      /// <summary>
      /// Enables best fit mapping when converting Unicode characters to ANSI.
      /// </summary>
      BestFitMapping = 0x0010,
      /// <summary>
      /// Information about target function. Not relevant for fields.
      /// </summary>
      SupportsLastError = 0x0040,
      /// <summary>
      /// The mask value for call conventions of methods (3 bits).
      /// </summary>
      CallConvMask = 0x0700,
      /// <summary>
      /// Platform-specific call convention.
      /// </summary>
      CallConvPlatformapi = 0x0100,
      /// <summary>
      /// C-style call convention.
      /// </summary>
      CallConvCDecl = 0x0200,
      /// <summary>
      /// Standard call convention.
      /// </summary>
      CallConvStdcall = 0x0300,
      /// <summary>
      /// This -call convention.
      /// </summary>
      CallConvThiscall = 0x0400,
      /// <summary>
      /// Fast call convention.
      /// </summary>
      CallConvFastcall = 0x0500,
      /// <summary>
      /// Specifies whether to throw on unmappable Unicode character when converting to ANSI.
      /// </summary>
      ThrowOnUnmappableChar = 0x1000
   }

   /// <summary>
   /// This enumeration holds values for resource files of the assembly. See ECMA specification for more information.
   /// </summary>
   [Flags]
   public enum ManifestResourceAttributes
   {
      /// <summary>
      /// The mask for the visibility of the resource file (3 bits).
      /// </summary>
      VisibilityMask = 0x0007,
      /// <summary>
      /// The resource is exported from the assembly.
      /// </summary>
      Public = 0x0001,
      /// <summary>
      /// The resource is private to the assembly.
      /// </summary>
      Private = 0x0002
   }

   /// <summary>
   /// This enumeration holds values for method implementation information. See ECMA specification for more information.
   /// </summary>
   [Flags]
   public enum MethodImplAttributes
   {
      /// <summary>
      /// The mask for the code type of the method implementation (2 bits).
      /// </summary>
      CodeTypeMask = 0x0003,
      /// <summary>
      /// The method implementation is in CIL language.
      /// </summary>
      IL = 0x0000,
      /// <summary>
      /// The method implementation is in native language.
      /// </summary>
      Native = 0x0001,
      /// <summary>
      /// Reserved.
      /// </summary>
      OPTIL = 0x0002,
      /// <summary>
      /// The method implementation is provided by the runtime.
      /// </summary>
      Runtime = 0x0003,
      /// <summary>
      /// The mask for the managed status of the method code (1 bit).
      /// </summary>
      ManagedMask = 0x0004,
      /// <summary>
      /// The method implementation is unmanaged.
      /// </summary>
      Unmanaged = 0x0004,
      /// <summary>
      /// The method implementation is managed.
      /// </summary>
      Managed = 0x0000,
      /// <summary>
      /// Indicates method is defined; used primarily in merge scenarios. 
      /// </summary>
      ForwardRef = 0x0010,
      /// <summary>
      /// The method signature should be exported exactly as defined.
      /// </summary>
      PreserveSig = 0x0080,
      /// <summary>
      /// Internal call.
      /// </summary>
      InternalCall = 0x1000,
      /// <summary>
      /// Method is single threaded through the body.
      /// </summary>
      Synchronized = 0x0020,
      /// <summary>
      /// Method can not be inlined.
      /// </summary>
      NoInlining = 0x0008,
      /// <summary>
      /// Method should be aggresively inlined.
      /// </summary>
      AggressiveInlining = 0x0100,
      /// <summary>
      /// Method will not be optimized when generating native code.
      /// </summary>
      NoOptimization = 0x0040
   }


   /// <summary>
   /// This enumeration holds values for the associated semantics of a method. See ECMA specification for more information.
   /// </summary>
   public enum MethodSemanticsAttributes
   {
      /// <summary>
      /// The method is a setter for property.
      /// </summary>
      Setter = 0x0001,
      /// <summary>
      /// The method is a getter for property.
      /// </summary>
      Getter = 0x0002,
      /// <summary>
      /// The method serves other function for property or event.
      /// </summary>
      Other = 0x0004,
      /// <summary>
      /// The method serves as required addition method for event.
      /// </summary>
      AddOn = 0x0008,
      /// <summary>
      /// The method serves as required removal method for event.
      /// </summary>
      RemoveOn = 0x0010,
      /// <summary>
      /// The method serves as optional event raise method.
      /// </summary>
      Fire = 0x0020
   }

   /// <summary>
   /// This enumeration holds values for attributes of types. See ECMA speicification for more information.
   /// </summary>
   /// <seealso cref="System.Reflection.TypeAttributes"/>
   [Flags]
   public enum TypeAttributes
   {
      /// <summary>
      /// The mask for visibility of type (3 bits).
      /// </summary>
      VisibilityMask = 0x00000007,
      /// <summary>
      /// Type is not public.
      /// </summary>
      NotPublic = 0x00000000,
      /// <summary>
      /// Type is public.
      /// </summary>
      Public = 0x00000001,
      /// <summary>
      /// Type is nested with public visibility.
      /// </summary>
      NestedPublic = 0x00000002,
      /// <summary>
      /// Type is nested with private visibility.
      /// </summary>
      NestedPrivate = 0x00000003,
      /// <summary>
      /// Type is nested with family visibility.
      /// </summary>
      NestedFamily = 0x00000004,
      /// <summary>
      /// Type is nested with assembly visiblity.
      /// </summary>
      NestedAssembly = 0x00000005,
      /// <summary>
      /// Type is nested with family and assembly visiblity, meaning that the accessor must be both family and in same assembly.
      /// </summary>
      NestedFamANDAssem = 0x00000006,
      /// <summary>
      /// Type is nested with family or assembly visibility, meaning that the accessor must be either family or in same assembly.
      /// </summary>
      NestedFamORAssem = 0x00000007,

      /// <summary>
      /// The mask for Type layout (2 bits).
      /// </summary>
      LayoutMask = 0x00000018,
      /// <summary>
      /// Type fields are automatically laid out.
      /// </summary>
      AutoLayout = 0x00000000,
      /// <summary>
      /// Type fields are laid out sequentially.
      /// </summary>
      SequentialLayout = 0x00000008,
      /// <summary>
      /// Layout for Type fields is supplied explicitly.
      /// </summary>
      ExplicitLayout = 0x000000010,

      /// <summary>
      /// The mask for Type semantics (1bit).
      /// </summary>
      ClassSemanticsMask = 0x00000020,
      /// <summary>
      /// Type is a class.
      /// </summary>
      Class = 0x00000000,
      /// <summary>
      /// Type is an interface.
      /// </summary>
      Interface = 0x00000020,

      /// <summary>
      /// Type is abstract.
      /// </summary>
      Abstract = 0x00000080,
      /// <summary>
      /// Type is sealed.
      /// </summary>
      Sealed = 0x00000100,
      /// <summary>
      /// Type name is special. The name denotes in what way the type is special.
      /// </summary>
      SpecialName = 0x00000400,

      /// <summary>
      /// Type is imported.
      /// </summary>
      Import = 0x00001000,
      /// <summary>
      /// Type can be serialized.
      /// </summary>
      Serializable = 0x00002000,
      /// <summary>
      /// TODO WinRT-related?
      /// </summary>
      WindowsRuntime = 0x00004000,

      /// <summary>
      /// The mask about string information for native interoperability (2 bits).
      /// </summary>
      StringFormatMask = 0x00030000,
      /// <summary>
      /// LPSTR is interpreted as ANSI.
      /// </summary>
      AnsiClass = 0x00000000,
      /// <summary>
      /// LPSTR is interpreted as Unicode.
      /// </summary>
      UnicodeClass = 0x00010000,
      /// <summary>
      /// LPSTR is interpreted automatically.
      /// </summary>
      AutoClass = 0x00020000,
      /// <summary>
      /// Non-standard encoding for LPSTR.
      /// </summary>
      CustomFormatClass = 0x00030000,
      /// <summary>
      /// The mask for get non-standard string encoding for native interoperability (2bits). The meaning is not specified.
      /// </summary>
      CustomFormatMask = 0x00C00000,

      /// <summary>
      /// Type will be initialized before first static field access. Thus calling static methods will not cause type initialization.
      /// </summary>
      BeforeFieldInit = 0x00100000,

      /// <summary>
      /// Type is special in CIL context. The name denotes in what way type is special.
      /// </summary>
      RTSpecialName = 0x00000800,
      /// <summary>
      /// Type has security information with it.
      /// </summary>
      HasSecurity = 0x00040000,
      /// <summary>
      /// The type is exported.
      /// </summary>
      IsTypeForwarder = 0x00200000,
   }

   /// <summary>
   /// This enumeration holds values for method attributes. See ECMA specification for more information.
   /// </summary>
   /// <seealso cref="System.Reflection.MethodAttributes"/>
   [Flags]
   public enum MethodAttributes
   {
      /// <summary>
      /// The mask to get method access (3 bits).
      /// </summary>
      MemberAccessMask = 0x0007,
      /// <summary>
      /// Method is not accessible from code.
      /// </summary>
      CompilerControlled = 0x0000,
      /// <summary>
      /// Method is accessible only by its declaring type.
      /// </summary>
      Private = 0x0001,
      /// <summary>
      /// Method is accessible only by declaring type and its sub-types in this assembly.
      /// </summary>
      FamANDAssem = 0x0002,
      /// <summary>
      /// Method is accessible in this assembly.
      /// </summary>
      Assembly = 0x0003,
      /// <summary>
      /// Method is accessible only by declaring type and its sub-types.
      /// </summary>
      Family = 0x0004,
      /// <summary>
      /// Method is accessible by declaring type and its sub-types, and additionally in this assembly.
      /// </summary>
      FamORAssem = 0x0005,
      /// <summary>
      /// Method is accessible to anyone who has visibility to this scope.
      /// </summary>
      Public = 0x0006,

      /// <summary>
      /// No instance of declaring type is required to invoke method.
      /// </summary>
      Static = 0x0010,
      /// <summary>
      /// Method can not be overridden.
      /// </summary>
      Final = 0x0020,
      /// <summary>
      /// Method is virtual.
      /// </summary>
      Virtual = 0x0040,
      /// <summary>
      /// Method hids by name and signature, otherwise just by name.
      /// </summary>
      HideBySig = 0x0080,

      /// <summary>
      /// The mask to get vtable attributes (1 bit).
      /// </summary>
      VtableLayoutMask = 0x0100,
      /// <summary>
      /// Method reuses existing slot in vtable.
      /// </summary>
      ReuseSlot = 0x0000,
      /// <summary>
      /// Method always gets a new slot in vtable.
      /// </summary>
      NewSlot = 0x0100,

      /// <summary>
      /// Method can only be overridden if also accessible.
      /// </summary>
      Strict = 0x0200,
      /// <summary>
      /// Method does not provide an implementation.
      /// </summary>
      Abstract = 0x0400,
      /// <summary>
      /// Method is special. The name denotes in what way it is special.
      /// </summary>
      SpecialName = 0x0800,

      /// <summary>
      /// The method implementation is forwarded through PInvoke.
      /// </summary>
      PinvokeImpl = 0x2000,
      /// <summary>
      /// The managed method is exported by thunk to unmanaged code.
      /// </summary>
      UnmanagedExport = 0x0008,

      /// <summary>
      /// Method is special in CIL context. The name denotes in what way type is special.
      /// </summary>
      RTSpecialName = 0x1000,
      /// <summary>
      /// The method has security associated with it.
      /// </summary>
      HasSecurity = 0x4000,
      /// <summary>
      /// Method calls another method containing security code.
      /// </summary>
      RequireSecObject = 0x8000
   }

   ///// <summary>
   ///// Class containing useful things about <see cref="MethodAttributes"/>.
   ///// </summary>
   //public static class MethodAttributesUtils
   //{
   //   /// <summary>
   //   /// Method attributes for explicitly implementating methods.
   //   /// </summary>
   //   public const MethodAttributes EXPLICIT_IMPLEMENTATION_ATTRIBUTES = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
   //}

   /// <summary>
   /// This enumeration provides values for calling convention of unmanaged methods. See ECMA specification about StandAloneMethodSig for more info.
   /// </summary>
   [Flags]
   public enum UnmanagedCallingConventions
   {
      /// <summary>
      /// The default calling convention.
      /// </summary>
      Default = 0x00,

      /// <summary>
      /// Calling convention used by Standard C.
      /// </summary>
      C = 0x01,

      /// <summary>
      /// Calling convention used by standard C++.
      /// </summary>
      StdCall = 0x02,

      /// <summary>
      /// C++ call that passes this pointer to the method.
      /// </summary>
      ThisCall = 0x03,

      /// <summary>
      /// Special optimized C++ calling convention.
      /// </summary>
      FastCall = 0x04,

      /// <summary>
      /// The calling convention to use for methods with variable arguments.
      /// </summary>
      VarArg = 0x05,

      /// <summary>
      /// The calling convention to use for instance or virtual methods.
      /// </summary>
      HasThis = 0x20,

      /// <summary>
      /// The calling convention to use for function-pointer signatures.
      /// </summary>
      ExplicitThis = 0x40
   }

   /// <summary>
   /// This enumeration contains values for field attributes. See ECMA specification for more information.
   /// </summary>
   /// <seealso cref="System.Reflection.FieldAttributes"/>
   [Flags]
   public enum FieldAttributes
   {
      /// <summary>
      /// The mask to get field access (3 bits).
      /// </summary>
      FieldAccessMask = 0x0007,
      /// <summary>
      /// The field is not accessible from the code.
      /// </summary>
      CompilerControlled = 0x0000,
      /// <summary>
      /// The field is accessible only by its declaring type.
      /// </summary>
      Private = 0x0001,
      /// <summary>
      /// The field is accessible only by declaring type and its sub-types in this assembly.
      /// </summary>
      FamANDAssem = 0x0002,
      /// <summary>
      /// The field is accessible in this assembly.
      /// </summary>
      Assembly = 0x0003,
      /// <summary>
      /// The field is accessible only by declaring type and its sub-types.
      /// </summary>
      Family = 0x0004,
      /// <summary>
      /// The field is accessible by declaring type and its sub-types, and additionally in this assembly.
      /// </summary>
      FamORAssem = 0x0005,
      /// <summary>
      /// The field is accessible to anyone who has visibility to this scope.
      /// </summary>
      Public = 0x0006,

      /// <summary>
      /// No instance of declaring type is required to access fields.
      /// </summary>
      Static = 0x0010,
      /// <summary>
      /// Field can be written only during initialization (ie. in constructor).
      /// </summary>
      InitOnly = 0x0020,
      /// <summary>
      /// Field value is a compile time constant.
      /// </summary>
      Literal = 0x0040,
      /// <summary>
      /// Field should not be serialized when type is remoted.
      /// </summary>
      NotSerialized = 0x0080,
      /// <summary>
      /// The field is special. The name denotes in what way it is special.
      /// </summary>
      SpecialName = 0x0200,

      /// <summary>
      /// Implementation is forwarded through PInvoke.
      /// </summary>
      PinvokeImpl = 0x2000,

      /// <summary>
      /// The field is special in CIL context. The name denotes in what way type is special.
      /// </summary>
      RTSpecialName = 0x0400,
      /// <summary>
      /// Field has marshalling information.
      /// </summary>
      HasFieldMarshal = 0x1000,
      /// <summary>
      /// Field has default value.
      /// </summary>
      HasDefault = 0x8000,
      /// <summary>
      /// Field has RVA for its value.
      /// </summary>
      HasFieldRVA = 0x0100
   }

   /// <summary>
   /// This enumeration provides values for parameter attributes. See ECMA specification for more information.
   /// </summary>
   /// <seealso cref="System.Reflection.ParameterAttributes"/>
   [Flags]
   public enum ParameterAttributes
   {
      /// <summary>
      /// Parameter has no attributes.
      /// </summary>
      None = 0x0000,
      /// <summary>
      /// Parameter is an input parameter.
      /// </summary>
      In = 0x0001,
      /// <summary>
      /// Parameter is an output parameter.
      /// </summary>
      Out = 0x0002,
      /// <summary>
      /// Parameter is a local identifier (lcid).
      /// </summary>
      Lcid = 0x0004,
      /// <summary>
      /// Parameter is a return value.
      /// </summary>
      Retval = 0x0008,
      /// <summary>
      /// Parameter is optional.
      /// </summary>
      Optional = 0x0010,
      /// <summary>
      /// Parameter has a default value.
      /// </summary>
      HasDefault = 0x1000,
      /// <summary>
      /// Parameter has field marshalling information.
      /// </summary>
      HasFieldMarshal = 0x2000
   }

   /// <summary>
   /// This enumeration provides values for property attributes. See ECMA specification for more information.
   /// </summary>
   /// <seealso cref="System.Reflection.PropertyAttributes"/>
   [Flags]
   public enum PropertyAttributes
   {
      /// <summary>
      /// The property has no attributes.
      /// </summary>
      None = 0,
      /// <summary>
      /// The property is special. The name denotes in what way it is special.
      /// </summary>
      SpecialName = 0x0200,
      /// <summary>
      /// The property is special in CIL context. The name denotes in what way type is special.
      /// </summary>
      RTSpecialName = 0x0400,
      /// <summary>
      /// Property has default value.
      /// </summary>
      HasDefault = 0x1000
   }

   /// <summary>
   /// This enumeration provides values for event attributes. See ECMA specification for more information.
   /// </summary>
   /// <seealso cref="System.Reflection.EventAttributes"/>
   [Flags]
   public enum EventAttributes
   {
      /// <summary>
      /// The event has no attributes.
      /// </summary>
      None = 0,
      /// <summary>
      /// The event is special. The name denotes in what way it is special.
      /// </summary>
      SpecialName = 0x0200,
      /// <summary>
      /// The event is special in CIL context. The name denotes in what way type is special.
      /// </summary>
      RTSpecialName = 0x0400
   }

   /// <summary>
   /// This enumeration provides values for generic parameter attributes. See ECMA specification for more information.
   /// </summary>
   /// <seealso cref="System.Reflection.GenericParameterAttributes"/>
   [Flags]
   public enum GenericParameterAttributes
   {
      /// <summary>
      /// The mask for generic parameter variance (2 bits).
      /// </summary>
      VarianceMask = 0x0003,
      /// <summary>
      /// No variance.
      /// </summary>
      None = 0x0000,
      /// <summary>
      /// The generic parameter is covariant.
      /// </summary>
      Covariant = 0x0001,
      /// <summary>
      /// The generic parameter is contravariant.
      /// </summary>
      Contravariant = 0x0002,
      /// <summary>
      /// The mask for generic parameter special constraints (3 bits).
      /// </summary>
      SpecialConstraintMask = 0x001C,
      /// <summary>
      /// The generic parameter has <c>class</c> constraint.
      /// </summary>
      ReferenceTypeConstraint = 0x0004,
      /// <summary>
      /// The generic parameter has <c>struct</c> constraint.
      /// </summary>
      NotNullableValueTypeConstraint = 0x0008,
      /// <summary>
      /// The generic parameter has <c>new()</c> constraint.
      /// </summary>
      DefaultConstructorConstraint = 0x0010
   }

   ///// <summary>
   ///// The kind of the module being loaded or emitted.
   ///// </summary>
   //public enum ModuleKind
   //{
   //   /// <summary>
   //   /// The module is a class library (.dll) with assembly manifest.
   //   /// </summary>
   //   Dll,
   //   /// <summary>
   //   /// The module is a console application (.exe) with assembly manifest.
   //   /// </summary>
   //   Console,
   //   /// <summary>
   //   /// The module is a windows application (.exe) with assembly manifest.
   //   /// </summary>
   //   Windows,
   //   /// <summary>
   //   /// The module is a netmodule (.netmodule) without assembly manifest.
   //   /// </summary>
   //   NetModule,
   //}

   /// <summary>
   /// Specifies the security actions (ECMA-335 pp. 218-219), minus the ones that were obsoleted by .NET 4 release.
   /// </summary>
   /// <seealso href="http://msdn.microsoft.com/en-us/library/system.security.permissions.securityaction.aspx"/>
   /// <seealso href="http://msdn.microsoft.com/en-us/library/ee191568%28VS.100%29.aspx"/>
   public enum SecurityAction
   {
      /// <summary>
      /// Checks that all the callers in the call chain have been granted a permission to a specified resource.
      /// </summary>
      Demand = 2,

      /// <summary>
      /// Checks that the caller has been granted a permission to a specified resource.
      /// </summary>
      Assert = 3,

      /// <summary>
      /// Limits access to specific set of resources, even if the code could access other resources.
      /// </summary>
      PermitOnly = 5,

      /// <summary>
      /// The derived class, either inheriting a class or overriding a method, is required to have been granted a permission to a specified resource.
      /// </summary>
      InheritanceDemand = 7,
   }

   ///// <summary>
   ///// The target .NET runtime for the emitted module.
   ///// </summary>
   ///// <remarks>This controls things as metadata version string, mscorlib name and version, cli and tableheap versions.</remarks>
   ///// <seealso cref="EmittingArguments"/>.
   //public enum TargetRuntime
   //{
   //   /// <summary>
   //   /// The .NET 1.0 runtime.
   //   /// </summary>
   //   Net_1_0,
   //   /// <summary>
   //   /// The .NET 1.1 runtime.
   //   /// </summary>
   //   Net_1_1,
   //   /// <summary>
   //   /// The .NET 2.0, 3.0 and 3.5 runtimes
   //   /// </summary>
   //   Net_2_0,
   //   /// <summary>
   //   /// The .NET 4.0 and 4.5 runtimes.
   //   /// </summary>
   //   Net_4_0,
   //}

   ///// <summary>
   ///// Helper enumeration to separate custom modifiers into required and optional ones. See ECMA specification for more information about custom modifiers and their separation.
   ///// </summary>
   //public enum CILCustomModifierOptionality
   //{
   //   /// <summary>
   //   /// The custom modifier is required to be processed.
   //   /// </summary>
   //   Required,
   //   /// <summary>
   //   /// The custom modifier is not required to be processed.
   //   /// </summary>
   //   Optional
   //}

   /// <summary>
   /// This enumeration corresponds to <see cref="T:System.Runtime.InteropServices.VarEnum"/>.
   /// The values from this enumeration and <see cref="T:System.Runtime.InteropServices.VarEnum"/> should be directly castable between each other.
   /// </summary>
   public enum VarEnum
   {

      /// <summary>
      /// Indicates that a value was not specified.
      /// </summary>
      VT_EMPTY = 0,
      /// <summary>
      /// Indicates a null value, similar to a null value in SQL.
      /// </summary>
      VT_NULL = 1,
      /// <summary>
      /// Indicates a short integer.
      /// </summary>
      VT_I2 = 2,
      /// <summary>
      /// Indicates a long integer.
      /// </summary>
      VT_I4 = 3,
      /// <summary>
      /// Indicates a float value.
      /// </summary>
      VT_R4 = 4,
      /// <summary>
      /// Indicates a double value.
      /// </summary>
      VT_R8 = 5,
      /// <summary>
      /// Indicates a currency value.
      /// </summary>
      VT_CY = 6,
      /// <summary>
      /// Indicates a DATE value.
      /// </summary>
      VT_DATE = 7,
      /// <summary>
      /// Indicates a BSTR string.
      /// </summary>
      VT_BSTR = 8,
      /// <summary>
      /// Indicates an IDispatch pointer.
      /// </summary>
      VT_DISPATCH = 9,
      /// <summary>
      /// Indicates an SCODE.
      /// </summary>
      VT_ERROR = 10,
      /// <summary>
      /// Indicates a Boolean value.
      /// </summary>
      VT_BOOL = 11,
      /// <summary>
      /// Indicates a VARIANT far pointer.
      /// </summary>
      VT_VARIANT = 12,
      /// <summary>
      /// Indicates an IUnknown pointer.
      /// </summary>
      VT_UNKNOWN = 13,
      /// <summary>
      /// Indicates a decimal value.
      /// </summary>
      VT_DECIMAL = 14,
      /// <summary>
      /// Indicates a char value.
      /// </summary>
      VT_I1 = 16,
      /// <summary>
      /// Indicates a byte.
      /// </summary>
      VT_UI1 = 17,
      /// <summary>
      /// Indicates an unsignedshort.
      /// </summary>
      VT_UI2 = 18,
      /// <summary>
      /// Indicates an unsignedlong.
      /// </summary>
      VT_UI4 = 19,
      /// <summary>
      /// Indicates a 64-bit integer.
      /// </summary>
      VT_I8 = 20,
      /// <summary>
      /// Indicates an 64-bit unsigned integer.
      /// </summary>
      VT_UI8 = 21,
      /// <summary>
      /// Indicates an integer value.
      /// </summary>
      VT_INT = 22,
      /// <summary>
      /// Indicates an unsigned integer value.
      /// </summary>
      VT_UINT = 23,
      /// <summary>
      /// Indicates a C style void.
      /// </summary>
      VT_VOID = 24,
      /// <summary>
      /// Indicates an HRESULT.
      /// </summary>
      VT_HRESULT = 25,
      /// <summary>
      /// Indicates a pointer type.
      /// </summary>
      VT_PTR = 26,
      /// <summary>
      /// Indicates a SAFEARRAY. Not valid in a VARIANT.
      /// </summary>
      VT_SAFEARRAY = 27,
      /// <summary>
      /// Indicates a C style array.
      /// </summary>
      VT_CARRAY = 28,
      /// <summary>
      /// Indicates a user defined type.
      /// </summary>
      VT_USERDEFINED = 29,
      /// <summary>
      /// Indicates a null-terminated string.
      /// </summary>
      VT_LPSTR = 30,
      /// <summary>
      /// Indicates a wide string terminated by null.
      /// </summary>
      VT_LPWSTR = 31,
      /// <summary>
      /// Indicates a user defined type.
      /// </summary>
      VT_RECORD = 36,
      /// <summary>
      /// Indicates a FILETIME value.
      /// </summary>
      VT_FILETIME = 64,
      /// <summary>
      /// Indicates length prefixed bytes.
      /// </summary>
      VT_BLOB = 65,
      /// <summary>
      /// Indicates that the name of a stream follows.
      /// </summary>
      VT_STREAM = 66,
      /// <summary>
      /// Indicates that the name of a storage follows.
      /// </summary>
      VT_STORAGE = 67,
      /// <summary>
      /// Indicates that a stream contains an object.
      /// </summary>
      VT_STREAMED_OBJECT = 68,
      /// <summary>
      /// Indicates that a storage contains an object.
      /// </summary>
      VT_STORED_OBJECT = 69,
      /// <summary>
      /// Indicates that a blob contains an object.
      /// </summary>
      VT_BLOB_OBJECT = 70,
      /// <summary>
      /// Indicates the clipboard format.
      /// </summary>
      VT_CF = 71,
      /// <summary>
      /// Indicates a class ID.
      /// </summary>
      VT_CLSID = 72,
      /// <summary>
      /// Indicates a simple, counted array.
      /// </summary>
      VT_VECTOR = 4096,
      /// <summary>
      /// Indicates a SAFEARRAY pointer.
      /// </summary>
      VT_ARRAY = 8192,
      /// <summary>
      /// Indicates that a value is a reference.
      /// </summary>
      VT_BYREF = 16384,
      /// <summary>
      /// Indicates that the marshalling type is not specified.
      /// </summary>
      VT_NOT_SPECIFIED = -1
   }

   /// <summary>
   /// This enumeration corresponds to <see cref="T:System.Runtime.InteropServices.UnmanagedType"/>.
   /// /// The values from this enumeration and <see cref="T:System.Runtime.InteropServices.UnmanagedType"/> should be directly castable between each other.
   /// </summary>
   public enum UnmanagedType : int
   {
      /// <summary>
      /// A 4-byte Boolean value (true != 0, false = 0). This is the Win32 BOOL type.
      /// </summary>
      Bool = 2,
      /// <summary>
      /// A 1-byte signed integer. You can use this member to transform a Boolean value into a 1-byte, C-style bool (true = 1, false = 0).
      /// </summary>
      I1,
      /// <summary>
      /// A 1-byte unsigned integer.
      /// </summary>
      U1,
      /// <summary>
      /// A 2-byte signed integer.
      /// </summary>
      I2,
      /// <summary>
      /// A 2-byte unsigned integer.
      /// </summary>
      U2,
      /// <summary>
      /// A 4-byte signed integer.
      /// </summary>
      I4,
      /// <summary>
      /// A 4-byte unsigned integer.
      /// </summary>
      U4,
      /// <summary>
      /// An 8-byte signed integer.
      /// </summary>
      I8,
      /// <summary>
      /// An 8-byte unsigned integer.
      /// </summary>
      U8,
      /// <summary>
      /// A 4-byte floating-point number.
      /// </summary>
      R4,
      /// <summary>
      /// An 8-byte floating-point number.
      /// </summary>
      R8,
      /// <summary>
      /// A currency type. Used on a <see cref="T:System.Decimal" /> to marshal the decimal value as a COM currency type instead of as a Decimal.
      /// </summary>
      Currency = 15,
      /// <summary>
      /// A Unicode character string that is a length-prefixed double byte. You can use this member, which is the default string in COM, on the <see cref="T:System.String" /> data type.
      /// </summary>
      BStr = 19,
      /// <summary>
      /// A single byte, null-terminated ANSI character string. You can use this member on the <see cref="T:System.String" /> and <see cref="T:System.Text.StringBuilder" /> data types.
      /// </summary>
      LPStr,
      /// <summary>
      /// A 2-byte, null-terminated Unicode character string.
      /// </summary>
      LPWStr,
      /// <summary>
      /// A platform-dependent character string: ANSI on Windows 98, and Unicode on Windows NT and Windows XP. This value is supported only for platform invoke and not for COM interop, because exporting a string of type LPTStr is not supported.
      /// </summary>
      LPTStr,
      /// <summary>
      /// Used for in-line, fixed-length character arrays that appear within a structure. The character type used with <see cref="F:System.Runtime.InteropServices.UnmanagedType.ByValTStr" /> is determined by the <see cref="T:System.Runtime.InteropServices.CharSet" /> argument of the <see cref="T:System.Runtime.InteropServices.StructLayoutAttribute" /> attribute applied to the containing structure. Always use the <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.SizeConst" /> field to indicate the size of the array.
      /// </summary>
      ByValTStr,
      /// <summary>
      /// A COM IUnknown pointer. You can use this member on the <see cref="T:System.Object" /> data type.
      /// </summary>
      IUnknown = 25,
      /// <summary>
      /// A COM IDispatch pointer (Object in Microsoft Visual Basic 6.0).
      /// </summary>
      IDispatch,
      /// <summary>
      /// A VARIANT, which is used to marshal managed formatted classes and value types.
      /// </summary>
      Struct,
      /// <summary>
      /// A COM interface pointer. The <see cref="T:System.Guid" /> of the interface is obtained from the class metadata. Use this member to specify the exact interface type or the default interface type if you apply it to a class. This member produces the same behavior as <see cref="F:System.Runtime.InteropServices.UnmanagedType.IUnknown" /> when you apply it to the <see cref="T:System.Object" /> data type.
      /// </summary>
      Interface,
      /// <summary>
      /// A SafeArray, which is a self-describing array that carries the type, rank, and bounds of the associated array data. You can use this member with the <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.SafeArraySubType" /> field to override the default element type.
      /// </summary>
      SafeArray,
      /// <summary>
      /// When the <see cref="P:System.Runtime.InteropServices.MarshalAsAttribute.Value" /> property is set to ByValArray, the <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.SizeConst" /> field must be set to indicate the number of elements in the array. The <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.ArraySubType" /> field can optionally contain the <see cref="T:System.Runtime.InteropServices.UnmanagedType" /> of the array elements when it is necessary to differentiate among string types. You can use this <see cref="T:System.Runtime.InteropServices.UnmanagedType" /> only on an array that whose elements appear as fields in a structure.
      /// </summary>
      ByValArray,
      /// <summary>
      /// A platform-dependent, signed integer: 4 bytes on 32-bit Windows, 8 bytes on 64-bit Windows.
      /// </summary>
      SysInt,
      /// <summary>
      /// A platform-dependent, unsigned integer: 4 bytes on 32-bit Windows, 8 bytes on 64-bit Windows.
      /// </summary>
      SysUInt,
      /// <summary>
      /// A value that enables Visual Basic to change a string in unmanaged code and have the results reflected in managed code. This value is only supported for platform invoke.
      /// </summary>
      VBByRefStr = 34,
      /// <summary>
      /// An ANSI character string that is a length-prefixed single byte. You can use this member on the <see cref="T:System.String" /> data type.
      /// </summary>
      AnsiBStr,
      /// <summary>
      /// A length-prefixed, platform-dependent char string: ANSI on Windows 98, Unicode on Windows NT. You rarely use this BSTR-like member.
      /// </summary>
      TBStr,
      /// <summary>
      /// A 2-byte, OLE-defined VARIANT_BOOL type (true = -1, false = 0)
      /// .</summary>
      VariantBool,
      /// <summary>
      /// An integer that can be used as a C-style function pointer. You can use this member on a <see cref="T:System.Delegate" /> data type or on a type that inherits from a <see cref="T:System.Delegate" />.
      /// </summary>
      FunctionPtr,
      /// <summary>
      /// A dynamic type that determines the type of an object at run time and marshals the object as that type. This member is valid for platform invoke methods only.
      /// </summary>
      AsAny = 40,
      /// <summary>
      /// A pointer to the first element of a C-style array. When marshaling from managed to unmanaged code, the length of the array is determined by the length of the managed array. When marshaling from unmanaged to managed code, the length of the array is determined from the <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.SizeConst" /> and <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.SizeParamIndex" /> fields, optionally followed by the unmanaged type of the elements within the array when it is necessary to differentiate among string types.
      /// </summary>
      LPArray = 42,
      /// <summary>
      /// A pointer to a C-style structure that you use to marshal managed formatted classes. This member is valid for platform invoke methods only.
      /// </summary>
      LPStruct,
      /// <summary>
      /// Specifies the custom marshaler class when used with the System.Runtime.InteropServices.MarshalAsAttribute.MarshalType or System.Runtime.InteropServices.MarshalAsAttribute.MarshalTypeRef field.
      /// The System.Runtime.InteropServices.MarshalAsAttribute.MarshalCookie field can be used to pass additional information to the custom marshaler.
      /// You can use this member on any reference type.
      /// </summary>
      CustomMarshaler,
      /// <summary>
      /// A native type that is associated with an <see cref="F:System.Runtime.InteropServices.UnmanagedType.I4" /> or an <see cref="F:System.Runtime.InteropServices.UnmanagedType.U4" /> and that causes the parameter to be exported as an HRESULT in the exported type library.
      /// </summary>
      Error,
      /// <summary>
      /// A Windows Runtime interface pointer. You can use this member on the <see cref="T:System.Object" /> data type.
      /// </summary>
      IInspectable,
      /// <summary>
      /// A Windows Runtime string. You can use this member on the <see cref="T:System.String" /> data type.
      /// </summary>
      HString,
      Max = 80,
      NotPresent = -1,
   }

   /// <summary>
   /// This is enumeration of various values that the <see cref="AbstractSignature"/>s (excluding <see cref="TypeSignature"/>) can start with.
   /// </summary>  
   public enum SignatureStarters : byte
   {
      // TODO this enum should be in IO project. It is here only because of the AbstractMethodSignature.SignatureStarter property!
      // Probably the AbstractMethodSignature.SignatureStarter property should be refactored into CallingConventions and something else?
      Default = 0x00,
      C = 0x01,
      StandardCall = 0x02,
      ThisCall = 0x03,
      FastCall = 0x04,
      VarArgs = 0x05,
      Field = 0x06,
      LocalSignature = 0x07,
      Property = 0x08,
      Unmanaged = 0x09,
      MethodSpecGenericInst = 0x0A,
      NativeVarArgs = 0x0B,
      Generic = 0x10,
      HasThis = 0x20,
      ExplicitThis = 0x40,
      Reserved = 0x80,
   }

   public enum SignatureElementTypes : byte
   {
      End = 0x00,
      Void = 0x01,
      Boolean = 0x02,
      Char = 0x03,
      I1 = 0x04,
      U1 = 0x05,
      I2 = 0x06,
      U2 = 0x07,
      I4 = 0x08,
      U4 = 0x09,
      I8 = 0x0A,
      U8 = 0x0B,
      R4 = 0x0C,
      R8 = 0x0D,
      String = 0x0E,
      Ptr = 0x0F,
      ByRef = 0x10,
      ValueType = 0x11,
      Class = 0x12,
      Var = 0x13,
      Array = 0x14,
      GenericInst = 0x15,
      TypedByRef = 0x16,
      I = 0x18,
      U = 0x19,
      FnPtr = 0x1B,
      Object = 0x1C,
      SzArray = 0x1D,
      MVar = 0x1E,
      CModReqd = 0x1F,
      CModOpt = 0x20,
      Internal = 0x21,
      Modifier = 0x40,
      Sentinel = 0x41,
      Pinned = 0x45,
      Type = 0x50,
      CA_Boxed = 0x51,
      Reserved = 0x52,
      CA_Field = 0x53,
      CA_Property = 0x54,
      CA_Enum = 0x55
   }

   /// <summary>
   /// This enumeration contains all values for possible exception block types of IL.
   /// </summary>
   public enum ExceptionBlockType
   {
      /// <summary>
      /// The exception block type is try-catch statement.
      /// </summary>
      Exception = 0x0000,
      /// <summary>
      /// The exception block type is filter clause.
      /// </summary>
      Filter = 0x0001,
      /// <summary>
      /// The exception block type is try-finally statement.
      /// </summary>
      Finally = 0x0002,
      /// <summary>
      /// The exception block type is fault.
      /// </summary>
      Fault = 0x0004
   }
}

/// <summary>
/// This class contains extensions methods for types defined in this assembly.
/// </summary>
public static partial class E_CILPhysical
{
   /// <summary>
   /// Returns the textual representation of the <paramref name="algorithm"/>. In non-portable environment, this value can be used to set algorithm for strong name signature creation.
   /// </summary>
   /// <param name="algorithm">The algorithm.</param>
   /// <returns>The textual representation of the <paramref name="algorithm"/>.</returns>
   /// <exception cref="ArgumentException">If <paramref name="algorithm"/> is not one of the ones specified in <see cref="AssemblyHashAlgorithm"/> enumeration.</exception>
   public static String GetAlgorithmName( this AssemblyHashAlgorithm algorithm )
   {
      switch ( algorithm )
      {
         case AssemblyHashAlgorithm.None:
            return null;
         case AssemblyHashAlgorithm.MD5:
            return "MD5";
         case AssemblyHashAlgorithm.SHA1:
            return "SHA1";
         case AssemblyHashAlgorithm.SHA256:
            return "SHA256";
         case AssemblyHashAlgorithm.SHA384:
            return "SHA384";
         case AssemblyHashAlgorithm.SHA512:
            return "SHA512";
         default:
            throw new ArgumentException( "Unknown algorithm: " + algorithm );
      }
   }
   /// <summary>
   /// Returns <c>true</c> if <paramref name="flags"/> specify that the assembly reference holds full, unhashed public key.
   /// </summary>
   /// <param name="flags">The <see cref="AssemblyFlags"/></param>
   /// <returns><c>true</c> if <paramref name="flags"/> specify that the assembly reference holds full, unhashed public key; <c>false</c> otherwise.</returns>
   /// <seealso cref="AssemblyFlags.PublicKey"/>
   public static Boolean IsFullPublicKey( this AssemblyFlags flags )
   {
      return ( flags & AssemblyFlags.PublicKey ) != 0;
   }

   /// <summary>
   /// Returns <c>true</c> if <paramref name="flags"/> specify that the assembly reference is retargetable.
   /// </summary>
   /// <param name="flags">The <see cref="AssemblyFlags"/></param>
   /// <returns><c>true</c> if <paramref name="flags"/> specify that the assembly reference is retargetable; <c>false</c> otherwise.</returns>
   /// <seealso cref="AssemblyFlags.Retargetable"/>
   public static Boolean IsRetargetable( this AssemblyFlags flags )
   {
      return ( flags & AssemblyFlags.Retargetable ) != 0;
   }

   /// <summary>
   /// Returns <c>true</c> if <paramref name="attrs"/> indicates that method implementation is in CIL.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodImplAttributes"/></param>
   /// <returns><c>true</c> if <paramref name="attrs"/> indicates that method implementation is in CIL; <c>false</c> otherwise.</returns>
   /// <seealso cref="MethodImplAttributes.IL"/>
   public static Boolean IsIL( this MethodImplAttributes attrs )
   {
      return ( attrs & MethodImplAttributes.CodeTypeMask ) == MethodImplAttributes.IL && !attrs.IsInternalCall();
   }

   /// <summary>
   /// Returns <c>true</c> if <paramref name="attrs"/> indicates that method implementation is native code.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodImplAttributes"/></param>
   /// <returns><c>true</c> if <paramref name="attrs"/> indicates that method implementation is native code; <c>false</c> otherwise.</returns>
   /// <seealso cref="MethodImplAttributes.Native"/>
   public static Boolean IsNative( this MethodImplAttributes attrs )
   {
      return ( attrs & MethodImplAttributes.CodeTypeMask ) == MethodImplAttributes.Native;
   }

   /// <summary>
   /// Returns <c>true</c> if <paramref name="attrs"/> indicates that method implementation is provided by the runtime.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodImplAttributes"/></param>
   /// <returns><c>true</c> if <paramref name="attrs"/> indicates that method implementation is provided by the runtime; <c>false</c> otherwise.</returns>
   /// <seealso cref="MethodImplAttributes.Runtime"/>
   public static Boolean IsProvidedByRuntime( this MethodImplAttributes attrs )
   {
      return ( attrs & MethodImplAttributes.CodeTypeMask ) == MethodImplAttributes.Runtime;
   }

   /// <summary>
   /// Returns <c>true</c> if <paramref name="attrs"/> indicates that the method implementation is an internal call.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodImplAttributes"/></param>
   /// <returns><c>true</c> if <paramref name="attrs"/> indicates that the method implementation is an internal call; <c>false</c> otherwise.</returns>
   /// <seealso cref="MethodImplAttributes.InternalCall"/>
   public static Boolean IsInternalCall( this MethodImplAttributes attrs )
   {
      return ( attrs & MethodImplAttributes.InternalCall ) != 0;
   }

   /// <summary>
   /// Checks whether type attribute represents a public type.
   /// </summary>
   /// <param name="attrs">The <see cref="TypeAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a public type; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Type.IsPublic"/>
   public static Boolean IsPublic( this TypeAttributes attrs )
   {
      return ( attrs & TypeAttributes.VisibilityMask ) == TypeAttributes.Public;
   }

   /// <summary>
   /// Checks whether type attribute represents non-public type.
   /// </summary>
   /// <param name="attrs">The <see cref="TypeAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents non-public type; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Type.IsNotPublic"/>
   public static Boolean IsNotPublic( this TypeAttributes attrs )
   {
      return ( attrs & TypeAttributes.VisibilityMask ) == TypeAttributes.NotPublic;
   }

   /// <summary>
   /// Checks whether type attribute represents a class or a value type.
   /// </summary>
   /// <param name="attrs">The <see cref="TypeAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a class or a value type; <c>false</c> otherwise.</returns>
   /// <remarks>This differs from <see cref="System.Type.IsClass"/> property in such way that this method does not perform value type check.</remarks>
   /// <seealso cref="TypeAttributes.Class"/>
   public static Boolean IsClass( this TypeAttributes attrs )
   {
      return ( attrs & TypeAttributes.ClassSemanticsMask ) == TypeAttributes.Class;
   }

   /// <summary>
   /// Checks whether type attribute represents a sealed class.
   /// </summary>
   /// <param name="attrs">The <see cref="TypeAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a sealed class; <c>false</c> otherwise.</returns>
   public static Boolean IsSealed( this TypeAttributes attrs )
   {
      return ( attrs & TypeAttributes.Sealed ) != 0;
   }

   /// <summary>
   /// Checks whether type attribute represents an interface.
   /// </summary>
   /// <param name="attrs">The <see cref="TypeAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents an interface; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Type.IsInterface"/>
   /// <seealso cref="TypeAttributes.Interface"/>
   public static Boolean IsInterface( this TypeAttributes attrs )
   {
      return ( attrs & TypeAttributes.ClassSemanticsMask ) == TypeAttributes.Interface;
   }

   /// <summary>
   /// Checks whether type attribute represents an abstract type.
   /// </summary>
   /// <param name="attrs">The <see cref="TypeAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents an abstract type; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Type.IsAbstract"/>
   /// <seealso cref="TypeAttributes.Abstract"/>
   public static Boolean IsAbstract( this TypeAttributes attrs )
   {
      return ( attrs & TypeAttributes.Abstract ) != 0;
   }

   /// <summary>
   /// Checks whether type attribute represents an automatically laid out type.
   /// </summary>
   /// <param name="attrs">The <see cref="TypeAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents an automatically laid out type; <c>false</c> otherwise.</returns>
   /// <seealso cref="TypeAttributes.AutoLayout"/>
   public static Boolean IsAutoLayout( this TypeAttributes attrs )
   {
      return ( attrs & TypeAttributes.LayoutMask ) == TypeAttributes.AutoLayout;
   }

   /// <summary>
   /// Checks whether type attribute represents a sequentially laid out type.
   /// </summary>
   /// <param name="attrs">The <see cref="TypeAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a sequentially laid out type; <c>false</c> otherwise.</returns>
   /// <seealso cref="TypeAttributes.SequentialLayout"/>
   public static Boolean IsSequentialLayout( this TypeAttributes attrs )
   {
      return ( attrs & TypeAttributes.LayoutMask ) == TypeAttributes.SequentialLayout;
   }

   /// <summary>
   /// Checks whether type attribute represents an explicitly laid out type.
   /// </summary>
   /// <param name="attrs">The <see cref="TypeAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents an explicitly laid out type; <c>false</c> otherwise.</returns>
   /// <seealso cref="TypeAttributes.ExplicitLayout"/>
   public static Boolean IsExplicitLayout( this TypeAttributes attrs )
   {
      return ( attrs & TypeAttributes.LayoutMask ) == TypeAttributes.ExplicitLayout;
   }

   /// <summary>
   /// Checks whether type attribute represents type forwarder. See ECMA specification for more information about forwarded types.
   /// </summary>
   /// <param name="attrs">The <see cref="TypeAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents type forwarder; <c>false</c> otherwise.</returns>
   /// <remarks>Normal <see cref="System.Type"/> objects never represent type forwarders. However, <see cref="CILAssembly.ForwardedTypeInfos"/> property returns all such types for the assembly in question.</remarks>
   /// <seealso cref="TypeAttributes.IsTypeForwarder"/>
   public static Boolean IsTypeForwarder( this TypeAttributes attrs )
   {
      return ( attrs & TypeAttributes.IsTypeForwarder ) != 0;
   }

   /// <summary>
   /// Checks whether type attribute has <see cref="TypeAttributes.NestedPublic"/> value set within its <see cref="TypeAttributes.VisibilityMask"/>.
   /// </summary>
   /// <param name="attrs">The <see cref="TypeAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> has <see cref="TypeAttributes.NestedPublic"/> value set within its <see cref="TypeAttributes.VisibilityMask"/>.</returns>
   public static Boolean IsNestedPublic( this TypeAttributes attrs )
   {
      return ( attrs & TypeAttributes.VisibilityMask ) == TypeAttributes.NestedPublic;
   }

   /// <summary>
   /// Checks whether type attribute has <see cref="TypeAttributes.NestedPrivate"/> value set within its <see cref="TypeAttributes.VisibilityMask"/>.
   /// </summary>
   /// <param name="attrs">The <see cref="TypeAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> has <see cref="TypeAttributes.NestedPrivate"/> value set within its <see cref="TypeAttributes.VisibilityMask"/>.</returns>
   public static Boolean IsNestedPrivate( this TypeAttributes attrs )
   {
      return ( attrs & TypeAttributes.VisibilityMask ) == TypeAttributes.NestedPrivate;
   }

   /// <summary>
   /// Checks whether type attribute has <see cref="TypeAttributes.NestedFamily"/> value set within its <see cref="TypeAttributes.VisibilityMask"/>.
   /// </summary>
   /// <param name="attrs">The <see cref="TypeAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> has <see cref="TypeAttributes.NestedFamily"/> value set within its <see cref="TypeAttributes.VisibilityMask"/>.</returns>
   public static Boolean IsNestedFamily( this TypeAttributes attrs )
   {
      return ( attrs & TypeAttributes.VisibilityMask ) == TypeAttributes.NestedFamily;
   }

   /// <summary>
   /// Checks whether type attribute has <see cref="TypeAttributes.NestedAssembly"/> value set within its <see cref="TypeAttributes.VisibilityMask"/>.
   /// </summary>
   /// <param name="attrs">The <see cref="TypeAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> has <see cref="TypeAttributes.NestedAssembly"/> value set within its <see cref="TypeAttributes.VisibilityMask"/>.</returns>
   public static Boolean IsNestedAssembly( this TypeAttributes attrs )
   {
      return ( attrs & TypeAttributes.VisibilityMask ) == TypeAttributes.NestedAssembly;
   }

   /// <summary>
   /// Checks whether type attribute has <see cref="TypeAttributes.NestedFamANDAssem"/> value set within its <see cref="TypeAttributes.VisibilityMask"/>.
   /// </summary>
   /// <param name="attrs">The <see cref="TypeAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> has <see cref="TypeAttributes.NestedFamANDAssem"/> value set within its <see cref="TypeAttributes.VisibilityMask"/>.</returns>
   public static Boolean IsNestedFamilyANDAssembly( this TypeAttributes attrs )
   {
      return ( attrs & TypeAttributes.VisibilityMask ) == TypeAttributes.NestedFamANDAssem;
   }

   /// <summary>
   /// Checks whether type attribute has <see cref="TypeAttributes.NestedFamORAssem"/> value set within its <see cref="TypeAttributes.VisibilityMask"/>.
   /// </summary>
   /// <param name="attrs">The <see cref="TypeAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> has <see cref="TypeAttributes.NestedFamORAssem"/> value set within its <see cref="TypeAttributes.VisibilityMask"/>.</returns>
   public static Boolean IsNestedFamilyORAssembly( this TypeAttributes attrs )
   {
      return ( attrs & TypeAttributes.VisibilityMask ) == TypeAttributes.NestedFamORAssem;
   }

   /// <summary>
   /// Checks whether given <see cref="TypeAttributes"/> signify that the type may be accessed from other assembly, that is, the visibility is one of <see cref="TypeAttributes.Public"/>, <see cref="TypeAttributes.NestedPublic"/>, <see cref="TypeAttributes.NestedFamily"/> or <see cref="TypeAttributes.NestedFamORAssem"/>.
   /// This check does not take into account any <see cref="System.Runtime.CompilerServices.InternalsVisibleToAttribute"/>d applied to the assembly.
   /// </summary>
   /// <param name="attrs">The <see cref="TypeAttributes"/>.</param>
   /// <returns><c>true</c> if the type with given <see cref="TypeAttributes"/> may be accessed to other assembly without the use of <see cref="System.Runtime.CompilerServices.InternalsVisibleToAttribute"/>; <c>false</c> otherwise.</returns>
   public static Boolean IsVisibleToOutsideOfDefinedAssembly( this TypeAttributes attrs )
   {
      switch ( ( attrs & TypeAttributes.VisibilityMask ) )
      {
         case TypeAttributes.Public:
         case TypeAttributes.NestedPublic:
         case TypeAttributes.NestedFamily:
         case TypeAttributes.NestedFamORAssem:
            return true;
         default:
            return false;
      }
   }

   /// <summary>
   /// Checks whether method attribute represents a method visible in assembly scope.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a method visible in assembly scope; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.MethodBase.IsAssembly"/>
   /// <seealso cref="MethodAttributes.Assembly"/>
   public static Boolean IsAssembly( this MethodAttributes attrs )
   {
      return ( attrs & MethodAttributes.MemberAccessMask ) == MethodAttributes.Assembly;
   }

   /// <summary>
   /// Checks whether method attribute represents a method visible in declaring type and its sub-types.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a method visible in declaring type and its sub-types; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.MethodBase.IsFamily"/>
   /// <seealso cref="MethodAttributes.Family"/>
   public static Boolean IsFamily( this MethodAttributes attrs )
   {
      return ( attrs & MethodAttributes.MemberAccessMask ) == MethodAttributes.Family;
   }

   /// <summary>
   /// Checks whether method attribute represents a method visible in declaring type and its sub-types which are in the same assembly.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a method visible in declaring type and its sub-types which are in the same assembly; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.MethodBase.IsFamilyAndAssembly"/>
   /// <seealso cref="MethodAttributes.FamANDAssem"/>
   public static Boolean IsFamilyAndAssembly( this MethodAttributes attrs )
   {
      return ( attrs & MethodAttributes.MemberAccessMask ) == MethodAttributes.FamANDAssem;
   }

   /// <summary>
   /// Checks whether method attribute represents a method visible in declaring type and its sub-types in addition to the types that are in the same assembly.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a method visible in declaring type and its sub-types in addition to the types that are in the same assembly; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.MethodBase.IsFamilyOrAssembly"/>
   /// <seealso cref="MethodAttributes.FamORAssem"/>
   public static Boolean IsFamilyOrAssembly( this MethodAttributes attrs )
   {
      return ( attrs & MethodAttributes.MemberAccessMask ) == MethodAttributes.FamORAssem;
   }

   /// <summary>
   /// Checks whether method attribute represents a method visible in public scope.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a method visible in public scope; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.MethodBase.IsPublic"/>
   /// <seealso cref="MethodAttributes.Public"/>
   public static Boolean IsPublic( this MethodAttributes attrs )
   {
      return ( attrs & MethodAttributes.MemberAccessMask ) == MethodAttributes.Public;
   }

   /// <summary>
   /// Checks whether method attribute represents a method visible in private scope.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a method visible in private scope; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.MethodBase.IsPrivate"/>
   /// <seealso cref="MethodAttributes.Private"/>
   public static Boolean IsPrivate( this MethodAttributes attrs )
   {
      return ( attrs & MethodAttributes.MemberAccessMask ) == MethodAttributes.Private;
   }

   /// <summary>
   /// Checks whether method attribute represents a method not accessible from anywhere in the code.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a method not accessible from anywhere in the code; <c>false</c> otherwise.</returns>
   /// <seealso cref="MethodAttributes.CompilerControlled"/>
   public static Boolean IsCompilerControlled( this MethodAttributes attrs )
   {
      return ( attrs & MethodAttributes.MemberAccessMask ) == MethodAttributes.CompilerControlled;
   }

   /// <summary>
   /// Checks whether method attribute represents a static method.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a static method; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.MethodBase.IsStatic"/>
   /// <seealso cref="MethodAttributes.Static"/>
   public static Boolean IsStatic( this MethodAttributes attrs )
   {
      return ( attrs & MethodAttributes.Static ) != 0;
   }

   /// <summary>
   /// Checks whether method attribute represents a final method.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a final method; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.MethodBase.IsFinal"/>
   /// <seealso cref="MethodAttributes.Final"/>
   public static Boolean IsFinal( this MethodAttributes attrs )
   {
      return ( attrs & MethodAttributes.Final ) != 0;
   }

   /// <summary>
   /// Checks whether method attribute represents a virtual method.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a virtual method; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.MethodBase.IsVirtual"/>
   /// <seealso cref="MethodAttributes.Virtual"/>
   public static Boolean IsVirtual( this MethodAttributes attrs )
   {
      return ( attrs & MethodAttributes.Virtual ) != 0;
   }

   /// <summary>
   /// Checks whether method attribute represents a method hiding by the signature instead of just name.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a method hiding by the signature instead of just name; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.MethodBase.IsHideBySig"/>
   /// <seealso cref="MethodAttributes.HideBySig"/>
   public static Boolean IsHideBySig( this MethodAttributes attrs )
   {
      return ( attrs & MethodAttributes.HideBySig ) != 0;
   }

   /// <summary>
   /// Checks whether method attribute represents a method which forces overriden methods to have same visibility.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a method which forces overriden methods to have same visibility; <c>false</c> otherwise.</returns>
   /// <seealso cref="MethodAttributes.Strict"/>
   public static Boolean IsStrict( this MethodAttributes attrs )
   {
      return ( attrs & MethodAttributes.Strict ) != 0;
   }

   /// <summary>
   /// Checks whether method attribute represents a method which reuses vtable slot.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a method which reuses vtable slot; <c>false</c> otherwise.</returns>
   /// <seealso cref="MethodAttributes.ReuseSlot"/>
   public static Boolean IsReuseSlot( this MethodAttributes attrs )
   {
      return ( attrs & MethodAttributes.VtableLayoutMask ) == MethodAttributes.ReuseSlot;
   }

   /// <summary>
   /// Checks whether method attribute represents a method which always gets a new vtable slot.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a method which always gets a new vtable slot; <c>false</c> otherwise.</returns>
   /// <seealso cref="MethodAttributes.ReuseSlot"/>
   public static Boolean IsNewSlot( this MethodAttributes attrs )
   {
      return ( attrs & MethodAttributes.VtableLayoutMask ) == MethodAttributes.NewSlot;
   }

   /// <summary>
   /// Checks whether method attribute represents an abstract method.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents an abstract method; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.MethodBase.IsAbstract"/>
   /// <seealso cref="MethodAttributes.Abstract"/>
   public static Boolean IsAbstract( this MethodAttributes attrs )
   {
      return ( attrs & MethodAttributes.Abstract ) != 0;
   }

   /// <summary>
   /// Checks whether method attribute represents a method with special name.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a method with special name; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.MethodBase.IsSpecialName"/>
   /// <seealso cref="MethodAttributes.SpecialName"/>
   public static Boolean IsSpecialName( this MethodAttributes attrs )
   {
      return ( attrs & MethodAttributes.SpecialName ) != 0;
   }

   /// <summary>
   /// Checks whether method attribute represents a method forwarding its implementation through PInvoke.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a method forwarding its implementation through PInvoke; <c>false</c> otherwise.</returns>
   /// <seealso cref="MethodAttributes.PinvokeImpl"/>
   public static Boolean IsPInvokeImpl( this MethodAttributes attrs )
   {
      return ( attrs & MethodAttributes.PinvokeImpl ) != 0;
   }

   /// <summary>
   /// Checks whether method attribute represents a method capable of having IL bytecode implementation.
   /// </summary>
   /// <param name="attrs">The <see cref="MethodAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a method capable of having IL bytecode implementation; <c>false</c> otherwise.</returns>
   /// <seealso cref="MethodAttributes.Abstract"/>
   /// <seealso cref="MethodAttributes.PinvokeImpl"/>
   public static Boolean CanEmitIL( this MethodAttributes attrs )
   {
      return !attrs.IsAbstract() && !attrs.IsPInvokeImpl();
   }

   /// <summary>
   /// Checks whether the <see cref="UnmanagedCallingConventions.ExplicitThis"/> is set for given <see cref="UnmanagedCallingConventions"/>.
   /// </summary>
   /// <param name="conv">The <see cref="UnmanagedCallingConventions"/>.</param>
   /// <returns><c>true</c> if <see cref="UnmanagedCallingConventions.ExplicitThis"/> is set for <paramref name="conv"/>; <c>false</c> otherwise.</returns>
   public static Boolean IsExplicitThis( this UnmanagedCallingConventions conv )
   {
      return ( conv & UnmanagedCallingConventions.ExplicitThis ) != 0;
   }

   /// <summary>
   /// Checks whether the <see cref="UnmanagedCallingConventions.HasThis"/> is set for given <see cref="UnmanagedCallingConventions"/>.
   /// </summary>
   /// <param name="conv">The <see cref="UnmanagedCallingConventions"/>.</param>
   /// <returns><c>true</c> if <see cref="UnmanagedCallingConventions.HasThis"/> is set for <paramref name="conv"/>; <c>false</c> otherwise.</returns>
   public static Boolean IsThis( this UnmanagedCallingConventions conv )
   {
      return ( conv & UnmanagedCallingConventions.HasThis ) != 0;
   }

   /// <summary>
   /// Checks whether the <see cref="UnmanagedCallingConventions.C"/> is set for given <see cref="UnmanagedCallingConventions"/>.
   /// </summary>
   /// <param name="conv">The <see cref="UnmanagedCallingConventions"/>.</param>
   /// <returns><c>true</c> if <see cref="UnmanagedCallingConventions.C"/> is set for <paramref name="conv"/>; <c>false</c> otherwise.</returns>
   public static Boolean IsCCall( this UnmanagedCallingConventions conv )
   {
      return ( conv & UnmanagedCallingConventions.C ) != 0;
   }

   /// <summary>
   /// Checks whether the <see cref="UnmanagedCallingConventions.StdCall"/> is set for given <see cref="UnmanagedCallingConventions"/>.
   /// </summary>
   /// <param name="conv">The <see cref="UnmanagedCallingConventions"/>.</param>
   /// <returns><c>true</c> if <see cref="UnmanagedCallingConventions.StdCall"/> is set for <paramref name="conv"/>; <c>false</c> otherwise.</returns>
   public static Boolean IsStdCall( this UnmanagedCallingConventions conv )
   {
      return ( conv & UnmanagedCallingConventions.StdCall ) != 0;
   }

   /// <summary>
   /// Checks whether the <see cref="UnmanagedCallingConventions.ThisCall"/> is set for given <see cref="UnmanagedCallingConventions"/>.
   /// </summary>
   /// <param name="conv">The <see cref="UnmanagedCallingConventions"/>.</param>
   /// <returns><c>true</c> if <see cref="UnmanagedCallingConventions.ThisCall"/> is set for <paramref name="conv"/>; <c>false</c> otherwise.</returns>
   public static Boolean IsThisCall( this UnmanagedCallingConventions conv )
   {
      return ( conv & UnmanagedCallingConventions.ThisCall ) != 0;
   }

   /// <summary>
   /// Checks whether the <see cref="UnmanagedCallingConventions.FastCall"/> is set for given <see cref="UnmanagedCallingConventions"/>.
   /// </summary>
   /// <param name="conv">The <see cref="UnmanagedCallingConventions"/>.</param>
   /// <returns><c>true</c> if <see cref="UnmanagedCallingConventions.FastCall"/> is set for <paramref name="conv"/>; <c>false</c> otherwise.</returns>
   public static Boolean IsFastCall( this UnmanagedCallingConventions conv )
   {
      return ( conv & UnmanagedCallingConventions.FastCall ) != 0;
   }

   /// <summary>
   /// Checks whether the <see cref="UnmanagedCallingConventions.VarArg"/> is set for given <see cref="UnmanagedCallingConventions"/>.
   /// </summary>
   /// <param name="conv">The <see cref="UnmanagedCallingConventions"/>.</param>
   /// <returns><c>true</c> if <see cref="UnmanagedCallingConventions.VarArg"/> is set for <paramref name="conv"/>; <c>false</c> otherwise.</returns>
   public static Boolean IsVarArg( this UnmanagedCallingConventions conv )
   {
      return ( conv & UnmanagedCallingConventions.VarArg ) != 0;
   }

   /// <summary>
   /// Checks whether field attributes represent a field not accessible from the code.
   /// </summary>
   /// <param name="attrs">The <see cref="FieldAttributes"/></param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a field not accessible from the code; <c>false</c> otherwise.</returns>
   /// <seealso cref="FieldAttributes.CompilerControlled"/>
   public static Boolean IsCompilerControlled( this FieldAttributes attrs )
   {
      return ( attrs & FieldAttributes.FieldAccessMask ) == FieldAttributes.CompilerControlled;
   }

   /// <summary>
   /// Checks whether field attributes represent a public field.
   /// </summary>
   /// <param name="attrs">The <see cref="FieldAttributes"/></param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a public field; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.FieldInfo.IsPublic"/>
   /// <seealso cref="FieldAttributes.Public"/>
   public static Boolean IsPublic( this FieldAttributes attrs )
   {
      return ( attrs & FieldAttributes.FieldAccessMask ) == FieldAttributes.Public;
   }

   /// <summary>
   /// Checks whether field attributes represent a private field.
   /// </summary>
   /// <param name="attrs">The <see cref="FieldAttributes"/></param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a private field; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.FieldInfo.IsPrivate"/>
   /// <seealso cref="FieldAttributes.Private"/>
   public static Boolean IsPrivate( this FieldAttributes attrs )
   {
      return ( attrs & FieldAttributes.FieldAccessMask ) == FieldAttributes.Private;
   }

   /// <summary>
   /// Checks whether field attributes represent a field accessible by declaring type and its sub-types.
   /// </summary>
   /// <param name="attrs">The <see cref="FieldAttributes"/></param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a field accessible by declaring type and its sub-types; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.FieldInfo.IsFamily"/>
   /// <seealso cref="FieldAttributes.Family"/>
   public static Boolean IsFamily( this FieldAttributes attrs )
   {
      return ( attrs & FieldAttributes.FieldAccessMask ) == FieldAttributes.Family;
   }

   /// <summary>
   /// Checks whether field attributes represent a field accessible by declaring type and its sub-types which are in the same assembly.
   /// </summary>
   /// <param name="attrs">The <see cref="FieldAttributes"/></param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a field accessible by declaring type and its sub-types which are in the same assembly; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.FieldInfo.IsFamilyAndAssembly"/>
   /// <seealso cref="FieldAttributes.FamANDAssem"/>
   public static Boolean IsFamilyAndAssembly( this FieldAttributes attrs )
   {
      return ( attrs & FieldAttributes.FieldAccessMask ) == FieldAttributes.FamANDAssem;
   }

   /// <summary>
   /// Checks whether field attributes represent a field accessible within same assembly.
   /// </summary>
   /// <param name="attrs">The <see cref="FieldAttributes"/></param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a field accessible within same assembly; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.FieldInfo.IsAssembly"/>
   /// <seealso cref="FieldAttributes.Assembly"/>
   public static Boolean IsAssembly( this FieldAttributes attrs )
   {
      return ( attrs & FieldAttributes.FieldAccessMask ) == FieldAttributes.Assembly;
   }

   /// <summary>
   /// Checks whether field attributes represent a field accessible by declaring type and its sub-types in addition to the types in the same assembly.
   /// </summary>
   /// <param name="attrs">The <see cref="FieldAttributes"/></param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a field accessible by declaring type and its sub-types in addition to the types in the same assembly; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.FieldInfo.IsFamilyOrAssembly"/>
   /// <seealso cref="FieldAttributes.FamORAssem"/>
   public static Boolean IsFamilyOrAssembly( this FieldAttributes attrs )
   {
      return ( attrs & FieldAttributes.FieldAccessMask ) == FieldAttributes.FamORAssem;
   }

   /// <summary>
   /// Checks whether field attributes represent a static field.
   /// </summary>
   /// <param name="attrs">The <see cref="FieldAttributes"/></param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a static field; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.FieldInfo.IsStatic"/>
   /// <seealso cref="FieldAttributes.Static"/>
   public static Boolean IsStatic( this FieldAttributes attrs )
   {
      return ( attrs & FieldAttributes.Static ) != 0;
   }

   /// <summary>
   /// Checks whether field attributes represent a compile time constant.
   /// </summary>
   /// <param name="attrs">The <see cref="FieldAttributes"/></param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a compile time constant; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.FieldInfo.IsLiteral"/>
   /// <seealso cref="FieldAttributes.Literal"/>
   public static Boolean IsLiteral( this FieldAttributes attrs )
   {
      return ( attrs & FieldAttributes.Literal ) != 0;
   }

   /// <summary>
   /// Checks whether field attributes represent a field with default value.
   /// </summary>
   /// <param name="attrs">The <see cref="FieldAttributes"/></param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a field with default value; <c>false</c> otherwise.</returns>
   /// <seealso cref="FieldAttributes.HasDefault"/>
   public static Boolean HasDefault( this FieldAttributes attrs )
   {
      return ( attrs & FieldAttributes.HasDefault ) != 0;
   }

   /// <summary>
   /// Checks whether field attributes represent a field with RVA of the field value.
   /// </summary>
   /// <param name="attrs">The <see cref="FieldAttributes"/></param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a field with RVA of the field value; <c>false</c> otherwise.</returns>
   /// <seealso cref="FieldAttributes.HasFieldRVA"/>
   public static Boolean HasRVA( this FieldAttributes attrs )
   {
      return ( attrs & FieldAttributes.HasFieldRVA ) != 0;
   }

   /// <summary>
   /// Checks whether field attributes represent a field which can only be set in constructor.
   /// </summary>
   /// <param name="attrs">The <see cref="FieldAttributes"/></param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a field which can only be set in constructor; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.FieldInfo.IsInitOnly"/>
   /// <seealso cref="FieldAttributes.InitOnly"/>
   public static Boolean IsInitOnly( this FieldAttributes attrs )
   {
      return ( attrs & FieldAttributes.InitOnly ) != 0;
   }

   /// <summary>
   /// Checks whether field attributes represent a field with special name.
   /// </summary>
   /// <param name="attrs">The <see cref="FieldAttributes"/></param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a field with special name; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.FieldInfo.IsSpecialName"/>
   /// <seealso cref="FieldAttributes.SpecialName"/>
   public static Boolean IsSpecialName( this FieldAttributes attrs )
   {
      return ( attrs & FieldAttributes.SpecialName ) != 0;
   }

   /// <summary>
   /// Checks whether parameter attributes represent an input parameter.
   /// </summary>
   /// <param name="attrs">The <see cref="ParameterAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents an input parameter; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.ParameterInfo.IsIn"/>
   /// <seealso cref="ParameterAttributes.In"/>
   public static Boolean IsIn( this ParameterAttributes attrs )
   {
      return ( attrs & ParameterAttributes.In ) != 0;
   }

   /// <summary>
   /// Checks whether parameter attributes represent an output parameter.
   /// </summary>
   /// <param name="attrs">The <see cref="ParameterAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents an output parameter; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.ParameterInfo.IsOut"/>
   /// <seealso cref="ParameterAttributes.Out"/>
   public static Boolean IsOut( this ParameterAttributes attrs )
   {
      return ( attrs & ParameterAttributes.Out ) != 0;
   }

   /// <summary>
   /// Checks whether parameter attributes represent an optional parameter.
   /// </summary>
   /// <param name="attrs">The <see cref="ParameterAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents an optional parameter; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.ParameterInfo.IsOptional"/>
   /// <seealso cref="ParameterAttributes.Optional"/>
   public static Boolean IsOptional( this ParameterAttributes attrs )
   {
      return ( attrs & ParameterAttributes.Optional ) != 0;
   }

   /// <summary>
   /// Checks whether parameter attributes represent a parameter with default value.
   /// </summary>
   /// <param name="attrs">The <see cref="ParameterAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a parameter with default value; <c>false</c> otherwise.</returns>
   /// <seealso cref="ParameterAttributes.HasDefault"/>
   public static Boolean HasDefault( this ParameterAttributes attrs )
   {
      return ( attrs & ParameterAttributes.HasDefault ) != 0;
   }

   /// <summary>
   /// Checks whether parameter attributes represent a parameter with marshalling information.
   /// </summary>
   /// <param name="attrs">The <see cref="ParameterAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a parameter with marshalling information; <c>false</c> otherwise.</returns>
   /// <seealso cref="ParameterAttributes.HasFieldMarshal"/>
   public static Boolean HasFieldMarshal( this ParameterAttributes attrs )
   {
      return ( attrs & ParameterAttributes.HasFieldMarshal ) != 0;
   }

   /// <summary>
   /// Checks whether property attributes represent a property with special name.
   /// </summary>
   /// <param name="attrs">The <see cref="PropertyAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a property with special name; <c>false</c> otherwise.</returns>
   /// <seealso cref="System.Reflection.PropertyInfo.IsSpecialName"/>
   /// <seealso cref="PropertyAttributes.SpecialName"/>
   public static Boolean IsSpecialName( this PropertyAttributes attrs )
   {
      return ( attrs & PropertyAttributes.SpecialName ) == PropertyAttributes.SpecialName;
   }

   /// <summary>
   /// Checks whether property attributes represent a property with special name in CIL context.
   /// </summary>
   /// <param name="attrs">The <see cref="PropertyAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a property with special name in CIL context; <c>false</c> otherwise.</returns>
   /// <seealso cref="PropertyAttributes.RTSpecialName"/>
   public static Boolean IsRTSpecialName( this PropertyAttributes attrs )
   {
      return ( attrs & PropertyAttributes.RTSpecialName ) == PropertyAttributes.RTSpecialName;
   }

   /// <summary>
   /// Checks whether property attributes represent a property with default value.
   /// </summary>
   /// <param name="attrs">The <see cref="PropertyAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents a property with default value; <c>false</c> otherwise.</returns>
   /// <seealso cref="PropertyAttributes.HasDefault"/>
   public static Boolean HasDefault( this PropertyAttributes attrs )
   {
      return ( attrs & PropertyAttributes.HasDefault ) == PropertyAttributes.HasDefault;
   }

   ///// <summary>
   ///// Checks whether given module kind requires emitted module to have DLL characteristics.
   ///// </summary>
   ///// <param name="kind">The <see cref="ModuleKind"/>.</param>
   ///// <returns><c>true</c> if <paramref name="kind"/> represents a module kind which requires emitted module to have DLL characteristics; <c>false</c> otherwise.</returns>
   //public static Boolean IsDLL( this ModuleKind kind )
   //{
   //   return ModuleKind.Dll == kind || ModuleKind.NetModule == kind;
   //}


   ///// <summary>
   ///// Checks whether custom modifier optionality represents optional custom modifier.
   ///// </summary>
   ///// <param name="optionality">The <see cref="CILCustomModifierOptionality"/>.</param>
   ///// <returns><c>true</c> if <paramref name="optionality"/> represents optional custom modifier; <c>false</c> otherwise.</returns>
   ///// <seealso cref="CILCustomModifierOptionality.Optional"/>
   //public static Boolean IsOptional( this CILCustomModifierOptionality optionality )
   //{
   //   return CILCustomModifierOptionality.Optional == optionality;
   //}

   ///// <summary>
   ///// Checks whether custom modifier optionality represents required custom modifier.
   ///// </summary>
   ///// <param name="optionality">The <see cref="CILCustomModifierOptionality"/>.</param>
   ///// <returns><c>true</c> if <paramref name="optionality"/> represents required custom modifier; <c>false</c> otherwise.</returns>
   ///// <seealso cref="CILCustomModifierOptionality.Required"/>
   //public static Boolean IsRequired( this CILCustomModifierOptionality optionality )
   //{
   //   return CILCustomModifierOptionality.Required == optionality;
   //}

   /// <summary>
   /// Checks whether file attributes are marked to contain metadata.
   /// </summary>
   /// <param name="attrs">The <see cref="FileAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> is <see cref="FileAttributes.ContainsMetadata"/>; <c>false</c> otherwise.</returns>
   public static Boolean ContainsMetadata( this FileAttributes attrs )
   {
      return attrs == FileAttributes.ContainsMetadata;
   }

   /// <summary>
   /// Checks whether given <see cref="UnmanagedType"/> can be present on its own without any extra information.
   /// </summary>
   /// <param name="ut">The <see cref="UnmanagedType"/>.</param>
   /// <returns><c>true</c> if <paramref name="ut"/> can be present on its own without any extra information; <c>false</c> otherwise.</returns>
   /// <remarks>
   /// This method returns <c>true</c> when <paramref name="ut"/> is one of
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
   /// <item><description><see cref="UnmanagedType.Interface"/>,</description></item>
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
   public static Boolean IsNativeInstric( this CILAssemblyManipulator.Physical.UnmanagedType ut )
   {
      switch ( ut )
      {
         case UnmanagedType.Bool:
         case UnmanagedType.I1:
         case UnmanagedType.U1:
         case UnmanagedType.I2:
         case UnmanagedType.U2:
         case UnmanagedType.I4:
         case UnmanagedType.U4:
         case UnmanagedType.I8:
         case UnmanagedType.U8:
         case UnmanagedType.R4:
         case UnmanagedType.R8:
         case UnmanagedType.LPStr:
         case UnmanagedType.LPWStr:
         case UnmanagedType.SysInt:
         case UnmanagedType.SysUInt:
         case UnmanagedType.FunctionPtr:
         case UnmanagedType.Currency:
         case UnmanagedType.BStr:
         case UnmanagedType.Struct:
         case UnmanagedType.VBByRefStr:
         case UnmanagedType.AnsiBStr:
         case UnmanagedType.TBStr:
         case UnmanagedType.VariantBool:
         case UnmanagedType.AsAny:
         case UnmanagedType.LPStruct:
         case UnmanagedType.Error:
         case UnmanagedType.IInspectable: // IInspectable
         case UnmanagedType.HString: // HString
            return true;
         default:
            return false;
      }
   }

   /// <summary>
   /// Checks whether generic parameter attributes represent a covariant (<c>out</c>) type parameter.
   /// </summary>
   /// <param name="attrs">The <see cref="GenericParameterAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents covariant (<c>out</c>) type parameter; <c>false</c> otherwise.</returns>
   /// <seealso cref="GenericParameterAttributes"/>
   /// <seealso cref="GenericParameterAttributes.Covariant"/>
   public static Boolean IsCovariant( this GenericParameterAttributes attrs )
   {
      return ( attrs & GenericParameterAttributes.Covariant ) != 0;
   }

   /// <summary>
   /// Checks whether generic parameter attributes represent a contravariant (<c>in</c>) type parameter.
   /// </summary>
   /// <param name="attrs">The <see cref="GenericParameterAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> represents contravariant (<c>in</c>) type parameter; <c>false</c> otherwise.</returns>
   /// <seealso cref="GenericParameterAttributes"/>
   /// <seealso cref="GenericParameterAttributes.Contravariant"/>
   public static Boolean IsContravariant( this GenericParameterAttributes attrs )
   {
      return ( attrs & GenericParameterAttributes.Contravariant ) != 0;
   }

   /// <summary>
   /// Checks whether generic parameter attributes have a reference type constraint (<c>class</c>).
   /// </summary>
   /// <param name="attrs">The <see cref="GenericParameterAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> has a reference type constraint (<c>class</c>); <c>false</c> otherwise.</returns>
   /// <seealso cref="GenericParameterAttributes"/>
   /// <seealso cref="GenericParameterAttributes.Covariant"/>
   public static Boolean HasReferenceConstraint( this GenericParameterAttributes attrs )
   {
      return ( attrs & GenericParameterAttributes.ReferenceTypeConstraint ) != 0;
   }

   /// <summary>
   /// Checks whether generic parameter attributes have a non-null type constraint (<c>struct</c>).
   /// </summary>
   /// <param name="attrs">The <see cref="GenericParameterAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> has a non-null type constraint (<c>struct</c>); <c>false</c> otherwise.</returns>
   /// <seealso cref="GenericParameterAttributes"/>
   /// <seealso cref="GenericParameterAttributes.Covariant"/>
   public static Boolean HasNotNullConstraint( this GenericParameterAttributes attrs )
   {
      return ( attrs & GenericParameterAttributes.NotNullableValueTypeConstraint ) != 0;
   }

   /// <summary>
   /// Checks whether generic parameter attributes have a default constructor constraint (<c>new()</c>).
   /// </summary>
   /// <param name="attrs">The <see cref="GenericParameterAttributes"/>.</param>
   /// <returns><c>true</c> if <paramref name="attrs"/> has a default constructor constraint (<c>new()</c>); <c>false</c> otherwise.</returns>
   /// <seealso cref="GenericParameterAttributes"/>
   /// <seealso cref="GenericParameterAttributes.Covariant"/>
   public static Boolean HasDefaultConstructorConstraint( this GenericParameterAttributes attrs )
   {
      return ( attrs & GenericParameterAttributes.DefaultConstructorConstraint ) != 0;
   }

}