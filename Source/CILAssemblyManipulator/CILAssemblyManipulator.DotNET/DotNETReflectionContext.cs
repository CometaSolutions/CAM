﻿/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
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
#if !LOAD_ONLY
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CILAssemblyManipulator.API;
using System.Runtime.InteropServices;

namespace CILAssemblyManipulator.DotNET
{
   /// <summary>
   /// This class provides out of the box functionality to use <see cref="CILReflectionContext"/>.
   /// It registers callbacks for all required events of <see cref="CILReflectionContext"/> to provide functionality missing in Portable Class Library profile the <see cref="CILReflectionContext"/> is defined in.
   /// </summary>
   public static class DotNETReflectionContext
   {
      /// <summary>
      /// This is helper method to provide well-known defaults for some parameters of <see cref="FrameworkMonikerInfo.ReadAssemblyInformationFromRedistXMLFile"/> method.
      /// </summary>
      /// <param name="redistXMLFilePath">The location of the <c>FrameworkList.xml</c> file.</param>
      /// <param name="msCorLibName">This will hold the detected assembly name that acts as <c>mscorlib</c> assembly of the framework-</param>
      /// <param name="frameworkDisplayName">This will hold the detected display name of the framework.</param>
      /// <param name="targetFWDir">This will hold the detected full path to the target framework assemblies.</param>
      /// <param name="defaultTargetFWPath">The default target framework path, if the XML file does not define target framework path. If none provider, a directory up one level from the given XML path will be used.</param>
      /// <returns>The result of <see cref="FrameworkMonikerInfo.ReadAssemblyInformationFromRedistXMLFile"/> method.</returns>
      public static IDictionary<String, Tuple<Version, Byte[]>> ReadAssemblyInformationFromRedistXMLFile(
         String redistXMLFilePath,
         out String msCorLibName,
         out String frameworkDisplayName,
         out String targetFWDir,
         String defaultTargetFWPath = null
         )
      {
         var redistListDir = Path.GetDirectoryName( redistXMLFilePath );
         if ( defaultTargetFWPath == null )
         {
            defaultTargetFWPath = Directory.GetParent( redistListDir ).FullName;
         }

         IDictionary<String, Tuple<Version, Byte[]>> retVal;
         using ( var stream = File.Open( redistXMLFilePath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
         {
            retVal = FrameworkMonikerInfo.ReadAssemblyInformationFromRedistXMLFile(
                                 defaultTargetFWPath,
                                 stream,
                                 targetFWPathArg =>
                                 {
                                    var targetFWDirInner = ProcessTargetFWDir( redistListDir, targetFWPathArg );
                                    return Directory.EnumerateFiles( targetFWDirInner, "*.dll" ).Select( curFN => Path.Combine( targetFWDirInner, curFN ) );
                                 },
                                 () => DotNETReflectionContext.CreateDotNETContext(),
                                 assFN => File.Open( assFN, FileMode.Open, FileAccess.Read, FileShare.Read ),
                                 ( targetFWPathArg, simpleAssemblyName ) =>
                                 {
                                    return File.Exists( Path.Combine( targetFWPathArg, simpleAssemblyName + ".dll" ) );
                                 },
                                 out msCorLibName,
                                 out frameworkDisplayName,
                                 out targetFWDir
                                 );
         }
         targetFWDir = ProcessTargetFWDir( redistListDir, targetFWDir );
         return retVal;
      }

      /// <summary>
      /// Gets the default reference assembly path on current OS.
      /// </summary>
      /// <returns>
      /// <list type="bullet">
      /// <item><description>If <see cref="Environment.OSVersion" /> has property <see cref="OperatingSystem.Platform"/> of <see cref="PlatformID.Unix" />, then <see cref="FrameworkMonikerInfo.DEFAULT_REFERENCE_ASSEMBLY_DIR_UNIX" /> is returned.</description></item>
      /// <item><description>If <see cref="Environment.OSVersion" /> has property <see cref="OperatingSystem.Platform"/> of <see cref="PlatformID.MacOSX" />, then <see cref="FrameworkMonikerInfo.DEFAULT_REFERENCE_ASSEMBLY_DIR_OSX" /> is returned.</description></item>
      /// <item><description>Otherwise <see cref="FrameworkMonikerInfo.DEFAULT_REFERENCE_ASSEMBLY_DIR_WINDOWS" /> is returned. </description></item>
      /// </list>
      /// </returns>
      public static String GetDefaultReferenceAssemblyPath()
      {
         switch ( Environment.OSVersion.Platform )
         {
            case PlatformID.Unix:
               return FrameworkMonikerInfo.DEFAULT_REFERENCE_ASSEMBLY_DIR_UNIX;
            case PlatformID.MacOSX:
               return FrameworkMonikerInfo.DEFAULT_REFERENCE_ASSEMBLY_DIR_OSX;
            default:
               return FrameworkMonikerInfo.DEFAULT_REFERENCE_ASSEMBLY_DIR_WINDOWS;
         }
      }

      private static String ProcessTargetFWDir( String redistListDir, String targetFWDirAttribute )
      {
         String retVal;
         if ( String.IsNullOrEmpty( targetFWDirAttribute ) )
         {
            retVal = targetFWDirAttribute;
         }
         else
         {
            if ( Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX )
            {
               // Even mono unix distribution seems to have '\'
               targetFWDirAttribute = targetFWDirAttribute.Replace( '\\', Path.DirectorySeparatorChar );
            }
            retVal = Path.GetFullPath( Path.IsPathRooted( targetFWDirAttribute ) ? targetFWDirAttribute : Path.Combine( redistListDir, targetFWDirAttribute ) );
         }
         return retVal;
      }

      private static Boolean _canUseManagedCryptoAlgorithms = true;
      private static Boolean _canUseCNGCryptoAlgorithms = true;

      /// <summary>
      /// Helper function to create a <see cref="CILReflectionContext"/>, register to all required events of the resulting <see cref="CILReflectionContext"/>, invoke a given callback to use the <see cref="CILReflectionContext"/>, and then dispose it.
      /// </summary>
      /// <param name="action">The callback to use <see cref="CILReflectionContext"/>.</param>
      /// <param name="registerCryptoRelatedEvents">
      /// Whether to register to crypto-related events as well.
      /// If <c>true</c>, the callbacks are added to <see cref="CILReflectionContext.HashStreamLoadEvent"/>, <see cref="CILReflectionContext.RSACreationEvent"/> and <see cref="CILReflectionContext.RSASignatureCreationEvent"/> events.
      /// Otherwise the callbacks are not added to those events.
      /// </param>
      /// <remarks>
      /// <para>The following events of <see cref="CILReflectionContext"/> will have a correctly working handler:
      /// <list type="bullet">
      /// <item><description><see cref="CILReflectionContext.ModuleTypesLoadEvent"/>,</description></item>
      /// <item><description><see cref="CILReflectionContext.TypeModuleLoadEvent"/>,</description></item>
      /// <item><description><see cref="CILReflectionContext.CustomAttributeDataLoadEvent"/>,</description></item>
      /// <item><description><see cref="CILReflectionContext.EventOtherMethodsLoadEvent"/>,</description></item>
      /// <item><description><see cref="CILReflectionContext.ConstantValueLoadEvent"/>,</description></item>
      /// <item><description><see cref="CILReflectionContext.ExplicitMethodImplementationLoadEvent"/>,</description></item>
      /// <item><description><see cref="CILReflectionContext.MethodBodyLoadEvent"/>,</description></item>
      /// <item><description><see cref="CILReflectionContext.TokenResolveEvent"/>,</description></item>
      /// <item><description><see cref="CILReflectionContext.MethodImplementationAttributesLoadEvent"/>,</description></item>
      /// <item><description><see cref="CILReflectionContext.TypeLayoutLoadEvent"/>,</description></item>
      /// <item><description><see cref="CILReflectionContext.AssemblyNameLoadEvent"/>,</description></item>
      /// <item><description><see cref="CILReflectionContext.CustomModifierLoadEvent"/>,</description></item>
      /// <item><description><see cref="CILReflectionContext.HashStreamLoadEvent"/>, if <paramref name="registerCryptoRelatedEvents"/> is <c>true</c>,</description></item>
      /// <item><description><see cref="CILReflectionContext.RSACreationEvent"/>, if <paramref name="registerCryptoRelatedEvents"/> is <c>true</c>,</description></item>
      /// <item><description>and <see cref="CILReflectionContext.RSASignatureCreationEvent"/>, if <paramref name="registerCryptoRelatedEvents"/> is <c>true</c>.</description></item>
      /// </list>
      /// </para>
      /// </remarks>
      /// <seealso cref="CreateDotNETContext(Boolean)"/>
      public static void UseDotNETContext( Action<CILReflectionContext> action, Boolean registerCryptoRelatedEvents = true )
      {
         ///// <para>
         ///// The purpose of this method is to provide a helper which would make the GC less intrusive for the time during usage of <see cref="CILReflectionContext"/>, as it typically will allocate a lot of memory.
         ///// After the <paramref name="action"/> is completed, this method will restore the GC mode.
         ///// </para>

         // From http://stackoverflow.com/questions/6005865/prevent-net-garbage-collection-for-short-period-of-time
         //var oldMode = System.Runtime.GCSettings.LatencyMode;
         //System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions();
         //try
         //{
         //   System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency;
         using ( var ctx = CreateDotNETContext( registerCryptoRelatedEvents ) )
         {
            action( ctx );
         }
         //}
         //finally
         //{
         //   System.Runtime.GCSettings.LatencyMode = oldMode;
         //}
      }

      /// <summary>
      /// <inheritdoc cref="UseDotNETContext(Action{CILReflectionContext}, Boolean)"/>
      /// </summary>
      /// <param name="func">The callback to use <see cref="CILReflectionContext"/>.</param>
      /// <param name="registerCryptoRelatedEvents"><inheritdoc cref="UseDotNETContext(Action{CILReflectionContext}, Boolean)" select="/param[@name='registerCryptoRelatedEvents']/node()"/></param>
      /// <returns>The result of <paramref name="func"/>.</returns>
      /// <remarks>
      /// <inheritdoc cref="UseDotNETContext(Action{CILReflectionContext}, Boolean)" />
      /// </remarks>
      public static T UseDotNETContext<T>( Func<CILReflectionContext, T> func, Boolean registerCryptoRelatedEvents = true )
      {
         // From http://stackoverflow.com/questions/6005865/prevent-net-garbage-collection-for-short-period-of-time
         //var oldMode = System.Runtime.GCSettings.LatencyMode;
         //System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions();
         //try
         //{
         //   System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency;
         using ( var ctx = CreateDotNETContext( registerCryptoRelatedEvents ) )
         {
            return func( ctx );
         }
         //}
         //finally
         //{
         //   System.Runtime.GCSettings.LatencyMode = oldMode;
         //}
      }

      /// <summary>
      /// If the lifecycle management of <see cref="UseDotNETContext(Action{CILReflectionContext}, Boolean)"/> method is not enough, this method can be used to create instance of <see cref="CILReflectionContext"/> and dispose it whenever needed.
      /// </summary>
      /// <param name="registerCryptoRelatedEvents"><inheritdoc cref="UseDotNETContext(Action{CILReflectionContext}, Boolean)"/></param>
      /// <returns>
      /// An instance of <see cref="CILReflectionContext"/> with the required events having a correctly functioning handler.
      /// </returns>
      /// <remarks>
      /// <inheritdoc cref="UseDotNETContext(Action{CILReflectionContext}, Boolean)"/>
      /// </remarks>
      public static CILReflectionContext CreateDotNETContext( Boolean registerCryptoRelatedEvents = true )
      {
         var ctx = CILReflectionContextFactory.NewContext( new Type[]
         {
            typeof(IList<>),
            typeof(Object).Assembly.GetType("System.Collections.Generic.IReadOnlyList`1", false) // .NET 4.5 addition
         }, null );
         ctx.ModuleTypesLoadEvent += ctx_ModuleTypesLoadEvent;
         ctx.TypeModuleLoadEvent += ctx_TypeModuleLoadEvent;
         ctx.CustomAttributeDataLoadEvent += ctx_CustomAttributeDataLoadEvent;
         ctx.EventOtherMethodsLoadEvent += ctx_EventOtherMethodsLoadEvent;
         ctx.ConstantValueLoadEvent += ctx_ConstantValueLoadEvent;
         ctx.ExplicitMethodImplementationLoadEvent += ctx_InterfaceMappingLoadEvent;
         ctx.MethodBodyLoadEvent += ctx_MethodBodyLoadEvent;
         ctx.TokenResolveEvent += ctx_TokenResolveEvent;
         ctx.MethodImplementationAttributesLoadEvent += ctx_MethodImplementationAttributesLoadEvent;
         ctx.TypeLayoutLoadEvent += ctx_TypeLayoutLoadEvent;
         ctx.AssemblyNameLoadEvent += ctx_AssemblyNameLoadEvent;
         ctx.CustomModifierLoadEvent += ctx_CustomModifierLoadEvent;
         ctx.AssemblyReferenceResolveFromLoadedAssemblyEvent += ctx_AssemblyReferenceResolveFromLoadedAssemblyEvent;

         if ( registerCryptoRelatedEvents )
         {
            ctx.HashStreamLoadEvent += ctx_HashStreamLoadEvent;
            ctx.RSACreationEvent += ctx_RSACreationEvent;
            ctx.RSASignatureCreationEvent += ctx_RSASignatureCreationEvent;
#if !MONO
            ctx.CSPPublicKeyEvent += ctx_CSPPublicKeyEvent;
#endif
         }

         return ctx;
      }

      private static void ctx_AssemblyReferenceResolveFromLoadedAssemblyEvent( object sender, AssemblyRefResolveFromLoadedAssemblyEventArgs e )
      {
         try
         {
            e.ResolvedAssembly = System.Reflection.Assembly.Load( e.AssemblyName.ToString() ).NewWrapper( e.ReflectionContext );
         }
         catch
         {
            // Ignore
         }
      }

      private static void ctx_CustomModifierLoadEvent( object sender, CustomModifierEventLoadArgs e )
      {
         if ( e.ParameterInfo == null )
         {
            if ( e.MemberInfo is System.Reflection.FieldInfo )
            {
               e.OptionalModifiers = ( (System.Reflection.FieldInfo) e.MemberInfo ).GetOptionalCustomModifiers();
               e.RequiredModifiers = ( (System.Reflection.FieldInfo) e.MemberInfo ).GetRequiredCustomModifiers();
            }
            else
            {
               e.OptionalModifiers = ( (System.Reflection.PropertyInfo) e.MemberInfo ).GetOptionalCustomModifiers();
               e.RequiredModifiers = ( (System.Reflection.PropertyInfo) e.MemberInfo ).GetRequiredCustomModifiers();
            }
         }
         else
         {
            e.OptionalModifiers = e.ParameterInfo.GetOptionalCustomModifiers();
            e.RequiredModifiers = e.ParameterInfo.GetRequiredCustomModifiers();
         }
      }

      private static void ctx_RSASignatureCreationEvent( object sender, RSASignatureCreationEventArgs e )
      {
         var formatter = new RSAPKCS1SignatureFormatter( (AsymmetricAlgorithm) e.RSA );
         formatter.SetHashAlgorithm( e.HashAlgorithm );
         e.Signature = formatter.CreateSignature( e.ContentsHash );
      }

      private static void ctx_RSACreationEvent( Object sender, RSACreationEventArgs e )
      {
         RSA result = null;
         if ( e.RSAParameters == null )
         {
            result = CreateRSAServiceProvider( e.KeyPairContainer );
         }
         else
         {
            var erParams = e.RSAParameters.Value;
            var rParams = new System.Security.Cryptography.RSAParameters()
            {
               D = erParams.D,
               DP = erParams.DP,
               DQ = erParams.DQ,
               Exponent = erParams.Exponent,
               InverseQ = erParams.InverseQ,
               Modulus = erParams.Modulus,
               P = erParams.P,
               Q = erParams.Q
            };
            try
            {
               result = RSA.Create();
               result.ImportParameters( rParams );
            }
            catch ( System.Security.Cryptography.CryptographicException )
            {
               var success = false;
               try
               {
                  // Try SP without key container name instead
                  result = CreateRSAServiceProvider( null );
                  result.ImportParameters( rParams );
                  success = true;
               }
               catch
               {
                  // Ignore
               }

               if ( !success )
               {
                  throw;
               }
            }
         }
         e.RSA = result;
      }

      private static void ctx_HashStreamLoadEvent( object sender, HashStreamLoadEventArgs e )
      {
         HashAlgorithm transform;
         switch ( e.Algorithm )
         {
            case AssemblyHashAlgorithm.MD5:
               transform = GetTransform( null, () => new MD5Cng(), () => new MD5CryptoServiceProvider() );
               break;
            case AssemblyHashAlgorithm.SHA1:
               transform = GetTransform( () => new SHA1Managed(), () => new SHA1Cng(), () => new SHA1CryptoServiceProvider() );
               break;
            case AssemblyHashAlgorithm.SHA256:
               transform = GetTransform( () => new SHA256Managed(), () => new SHA256Cng(), () => new SHA256CryptoServiceProvider() );
               break;
            case AssemblyHashAlgorithm.SHA384:
               transform = GetTransform( () => new SHA384Managed(), () => new SHA384Cng(), () => new SHA384CryptoServiceProvider() );
               break;
            case AssemblyHashAlgorithm.SHA512:
               transform = GetTransform( () => new SHA512Managed(), () => new SHA512Cng(), () => new SHA512CryptoServiceProvider() );
               break;
            case AssemblyHashAlgorithm.None:
               throw new InvalidOperationException( "Tried to create hash stream with no hash algorithm" );
            default:
               throw new ArgumentException( "Unknown hash algorithm: " + e.Algorithm + "." );
         }
         e.CryptoStream = () => new CryptoStream( Stream.Null, transform, CryptoStreamMode.Write );
         e.HashGetter = () => transform.Hash;
         e.ComputeHash = bytes => transform.ComputeHash( bytes );
         e.Transform = transform;
      }

      private static void ctx_AssemblyNameLoadEvent( object sender, AssemblyNameEventArgs e )
      {
         var name = e.Assembly.GetName();
         e.AssemblyNameInfo = Tuple.Create( (AssemblyHashAlgorithm) name.HashAlgorithm, (AssemblyFlags) name.Flags, name.GetPublicKey() );
      }

      private static void ctx_TypeLayoutLoadEvent( object sender, TypeLayoutEventArgs e )
      {
         e.Layout = e.Type.StructLayoutAttribute;
      }

      private static void ctx_MethodImplementationAttributesLoadEvent( object sender, MethodImplAttributesEventArgs e )
      {
         e.MethodImplementationAttributes = (MethodImplAttributes) e.Method.GetMethodImplementationFlags();
      }

      private static void ctx_TokenResolveEvent( object sender, TokenResolveArgs e )
      {
         var module = e.Module;
         var token = e.Token;
         switch ( e.ResolveKind )
         {
            case TokenResolveArgs.ResolveKinds.String:
               e.ResolvedString = module.ResolveString( token );
               break;
            case TokenResolveArgs.ResolveKinds.Signature:
               e.ResolvedSignature = module.ResolveSignature( token );
               break;
            case TokenResolveArgs.ResolveKinds.Member:
               e.ResolvedMember = module.ResolveMember( token, e.TypeGenericArguments, e.MethodGenericArguments );
               break;
         }
      }

      private static void ctx_MethodBodyLoadEvent( object sender, MethodBodyLoadArgs e )
      {
         var body = e.Method.GetMethodBody();
         //if ( body != null )
         //{
         e.InitLocals = body.InitLocals;
         e.Locals = body.LocalVariables
            .OrderBy( local => local.LocalIndex )
            .Select( local => Tuple.Create( local.IsPinned, local.LocalType ) )
            .ToList();
         e.IL = body.GetILAsByteArray();
         e.ExceptionInfos = body.ExceptionHandlingClauses
            .Select( clause => Tuple.Create( (ExceptionBlockType) clause.Flags, clause.TryOffset, clause.TryLength, clause.HandlerOffset, clause.HandlerLength, clause.Flags == System.Reflection.ExceptionHandlingClauseOptions.Clause ? clause.CatchType : null, clause.Flags == System.Reflection.ExceptionHandlingClauseOptions.Filter ? clause.FilterOffset : -1 ) )
            .ToList();
         //}
      }

      private static void ctx_InterfaceMappingLoadEvent( object sender, ExplicitMethodImplementationLoadArgs e )
      {
         e.ExplicitlyImplementedMethods = e.Type.GetInterfaces()
            .Select( iFace => e.Type.GetInterfaceMap( iFace ) )
            .SelectMany( map => map.TargetMethods.Select( ( tMethod, idx ) => Tuple.Create( tMethod, map.InterfaceMethods[idx] ) ) )
            .GroupBy( tuple => tuple.Item1 )
            .ToDictionary( grp => grp.Key, grp => grp.Select( tuple => tuple.Item2 ).ToArray() );
      }

      private static void ctx_ConstantValueLoadEvent( object sender, ConstantValueLoadArgs e )
      {
         Object result = null;
         try
         {
            if ( e.Property != null )
            {
               result = e.Property.GetRawConstantValue();
            }
            else if ( e.Field != null )
            {
               result = e.Field.GetRawConstantValue();
            }
            else if ( e.Parameter != null )
            {
               result = e.Parameter.RawDefaultValue;
            }
         }
         catch
         {
            // Ignore - unmanaged stuff sometimes throws here
         }
         e.ConstantValue = result;
      }

      private static void ctx_EventOtherMethodsLoadEvent( object sender, EventOtherMethodsEventArgs e )
      {
         e.OtherMethods = e.Event.GetOtherMethods( true );
      }

      private static void ctx_TypeModuleLoadEvent( object sender, TypeModuleEventArgs e )
      {
         e.Module = e.Type.Module;
      }

      private static void ctx_CustomAttributeDataLoadEvent( object sender, CustomAttributeDataEventArgs e )
      {
         IEnumerable<System.Reflection.CustomAttributeData> attrs;
         if ( e.Member != null )
         {
            attrs = e.Member.GetCustomAttributesData();
         }
         else if ( e.Type != null )
         {
            attrs = e.Type.GetCustomAttributesData();
         }
         else if ( e.Parameter != null )
         {
            attrs = e.Parameter.GetCustomAttributesData();
         }
         else if ( e.Assembly != null )
         {
            attrs = e.Assembly.GetCustomAttributesData();
         }
         else if ( e.Module != null )
         {
            attrs = e.Module.GetCustomAttributesData();
         }
         else
         {
            throw new ArgumentException( "Custom attribute data event with no native member?" );
         }

         e.CustomAttributeData = attrs.Select( attr => Tuple.Create(
            attr.Constructor.NewWrapper( e.Context ),
            attr.ConstructorArguments.Select( cArg => CILCustomAttributeFactory.NewTypedArgument( ( cArg.ArgumentType.NewWrapperAsType( e.Context ) ), cArg.Value ) ),
            attr.NamedArguments.Select( nArg => CILCustomAttributeFactory.NewNamedArgument( ( nArg.MemberInfo is System.Reflection.PropertyInfo ? (CILElementForNamedCustomAttribute) ( (System.Reflection.PropertyInfo) nArg.MemberInfo ).NewWrapper( e.Context ) : ( (System.Reflection.FieldInfo) nArg.MemberInfo ).NewWrapper( e.Context ) ), CILCustomAttributeFactory.NewTypedArgument( nArg.TypedValue.ArgumentType.NewWrapperAsType( e.Context ), nArg.TypedValue.Value ) ) )
            ) );
      }

      private static void ctx_ModuleTypesLoadEvent( object sender, ModuleTypesEventArgs e )
      {
         e.DefinedTypes = e.Module.GetTypes();
      }

      private static HashAlgorithm GetTransform( Func<HashAlgorithm> managedVersion, Func<HashAlgorithm> cngVersion, Func<HashAlgorithm> spVersion )
      {
         if ( _canUseManagedCryptoAlgorithms && managedVersion != null )
         {
            try
            {
               return managedVersion();
            }
            catch
            {
               _canUseManagedCryptoAlgorithms = false;
            }
         }
         if ( _canUseCNGCryptoAlgorithms )
         {
            try
            {
               return cngVersion();
            }
            catch
            {
               _canUseCNGCryptoAlgorithms = false;
            }
         }
         return spVersion();
      }

      private static RSA CreateRSAServiceProvider( String keyContainerName )
      {
         var csp = new CspParameters { Flags = CspProviderFlags.UseMachineKeyStore };
         if ( keyContainerName != null )
         {
            csp.KeyContainerName = keyContainerName;
            csp.KeyNumber = 2;
         }
         return new RSACryptoServiceProvider( csp );
      }

#if !MONO

      private static void ctx_CSPPublicKeyEvent( Object sender, CSPPublicKeyEventArgs e )
      {
         var cspName = e.CSPName;
         Byte[] pk;
         Int32 winError1, winError2;
         if ( TryExportCSPPublicKey( cspName, 0, out pk, out winError1 ) // Try user-specific key first
            || TryExportCSPPublicKey( cspName, 32u, out pk, out winError2 ) ) // Then try machine-specific key (32u = CRYPT_MACHINE_KEYSET )
         {
            e.PublicKey = pk;
         }
         else
         {
            throw new InvalidOperationException( "Error when using user keystore: " + GetWin32ErrorString( winError1 ) + "\nError when using machine keystore: " + GetWin32ErrorString( winError2 ) );
         }
      }

      private static String GetWin32ErrorString( Int32 errorCode )
      {
         return new System.ComponentModel.Win32Exception( errorCode ).Message + " ( Win32 error code: 0x" + Convert.ToString( errorCode, 16 ) + ").";
      }

      // Much of this code is adapted by disassembling KeyPal.exe, available at http://www.jensign.com/KeyPal/index.html
      private static Boolean TryExportCSPPublicKey( String cspName, UInt32 keyType, out Byte[] pk, out Int32 winError )
      {
         var dwProvType = 1u;
         var ctxPtr = IntPtr.Zero;
         winError = 0;
         pk = null;
         var retVal = false;
         try
         {
            if ( Win32.CryptAcquireContext( out ctxPtr, cspName, "Microsoft Base Cryptographic Provider v1.0", dwProvType, keyType )
               || Win32.CryptAcquireContext( out ctxPtr, cspName, "Microsoft Strong Cryptographic Provider", dwProvType, keyType )
               || Win32.CryptAcquireContext( out ctxPtr, cspName, "Microsoft Enhanced Cryptographic Provider v1.0", dwProvType, keyType ) )
            {
               IntPtr keyPtr = IntPtr.Zero;
               try
               {
                  if ( Win32.CryptGetUserKey( ctxPtr, 2u, out keyPtr ) ) // 2 = AT_SIGNATURE
                  {
                     IntPtr expKeyPtr = IntPtr.Zero; // When exporting public key, this is zero
                     var arraySize = 0u;
                     if ( Win32.CryptExportKey( keyPtr, expKeyPtr, 6u, 0u, null, ref arraySize ) ) // 6 = PublicKey
                     {
                        pk = new Byte[arraySize];
                        if ( Win32.CryptExportKey( keyPtr, expKeyPtr, 6u, 0u, pk, ref arraySize ) )
                        {
                           retVal = true;
                        }
                        else
                        {
                           winError = Marshal.GetLastWin32Error();
                        }
                     }
                     else
                     {
                        winError = Marshal.GetLastWin32Error();
                     }
                  }
                  else
                  {
                     winError = Marshal.GetLastWin32Error();
                  }
               }
               finally
               {
                  if ( keyPtr != IntPtr.Zero )
                  {
                     Win32.CryptDestroyKey( keyPtr );
                  }
               }
            }
            else
            {
               winError = Marshal.GetLastWin32Error();
            }
         }
         finally
         {
            if ( ctxPtr != IntPtr.Zero )
            {
               Win32.CryptReleaseContext( ctxPtr, 0u );
            }
         }
         return retVal;
      }

      private static void ThrowFromLastWin32Error()
      {
         throw new System.ComponentModel.Win32Exception( Marshal.GetLastWin32Error() );
      }

   }

   internal static class Win32
   {
      // http://msdn.microsoft.com/en-us/library/windows/desktop/aa379886%28v=vs.85%29.aspx
      [DllImport( "advapi32.dll", CharSet = CharSet.Auto, SetLastError = true )]
      internal static extern Boolean CryptAcquireContext(
         [Out] out IntPtr hProv,
         [In, System.Runtime.InteropServices.MarshalAs( System.Runtime.InteropServices.UnmanagedType.LPWStr )] String pszContainer,
         [In, System.Runtime.InteropServices.MarshalAs( System.Runtime.InteropServices.UnmanagedType.LPWStr )] String pszProvider,
         [In] UInt32 dwProvType,
         [In] UInt32 dwFlags );

      // http://msdn.microsoft.com/en-us/library/windows/desktop/aa380268%28v=vs.85%29.aspx
      [DllImport( "advapi32.dll" )]
      internal static extern Boolean CryptReleaseContext(
         [In] IntPtr hProv,
         [In] UInt32 dwFlags
         );

      // http://msdn.microsoft.com/en-us/library/windows/desktop/aa380199%28v=vs.85%29.aspx
      [DllImport( "advapi32.dll" )]
      internal static extern Boolean CryptGetUserKey(
         [In] IntPtr hProv,
         [In] UInt32 dwKeySpec,
         [Out] out IntPtr hKey
         );

      // http://msdn.microsoft.com/en-us/library/windows/desktop/aa379918%28v=vs.85%29.aspx
      [DllImport( "advapi32.dll" )]
      internal static extern Boolean CryptDestroyKey( [In] IntPtr hKey );

      // http://msdn.microsoft.com/en-us/library/windows/desktop/aa379931%28v=vs.85%29.aspx
      [DllImport( "advapi32.dll", SetLastError = true )]
      internal static extern Boolean CryptExportKey(
         [In] IntPtr hKey,
         [In] IntPtr hExpKey,
         [In] UInt32 dwBlobType,
         [In] UInt32 dwFlags,
         [In] Byte[] pbData,
         [In, Out] ref UInt32 dwDataLen );
   }
#endif
}
#endif
