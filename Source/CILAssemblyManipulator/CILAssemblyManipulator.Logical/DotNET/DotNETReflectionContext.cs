/*
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
#if CAM_LOGICAL_IS_DOT_NET
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices;
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Logical
{
   /// <summary>
   /// This class implements <see cref="CILReflectionContextWrapperCallbacks"/> in .NET environment.
   /// </summary>
   public class CILReflectionContextWrapperCallbacksDotNET : CILReflectionContextWrapperCallbacks
   {
      /// <inheritdoc />
      public virtual IEnumerable<Type> GetTopLevelDefinedTypes( System.Reflection.Module module )
      {
         return module.GetTypes().Where( t => t.DeclaringType == null ); ;
      }

      /// <inheritdoc />
      public virtual System.Reflection.Module GetModuleOfType( Type type )
      {
         return type.Module;
      }

      /// <inheritdoc />
      public virtual IEnumerable<Object> GetCustomAttributesDataFor( System.Reflection.MemberInfo member )
      {
         return member.GetCustomAttributesData();
      }

      /// <inheritdoc />
      public virtual IEnumerable<Object> GetCustomAttributesDataFor( System.Reflection.ParameterInfo parameter )
      {
         return parameter.GetCustomAttributesData();
      }

      /// <inheritdoc />
      public virtual IEnumerable<Object> GetCustomAttributesDataFor( System.Reflection.Assembly assembly )
      {
         return assembly.GetCustomAttributesData();
      }

      /// <inheritdoc />
      public virtual IEnumerable<Object> GetCustomAttributesDataFor( System.Reflection.Module module )
      {
         return module.GetCustomAttributesData();
      }

      /// <inheritdoc />
      public virtual CILCustomAttribute GetCILCustomAttributeFromNative( CILCustomAttributeContainer container, Object caData )
      {
         var ctx = container.ReflectionContext;
         var attr = (System.Reflection.CustomAttributeData) caData;
         return CILCustomAttributeFactory.NewAttribute(
            container,
            ctx.NewWrapper( attr.Constructor ),
            attr.ConstructorArguments.Select( cArg => CILCustomAttributeFactory.NewTypedArgument( ( ctx.NewWrapperAsType( cArg.ArgumentType ) ), this.ExtractValue( ctx, cArg ) ) ),
            attr.NamedArguments.Select( nArg => CILCustomAttributeFactory.NewNamedArgument(
               ( nArg.MemberInfo is System.Reflection.PropertyInfo ? (CILElementForNamedCustomAttribute) ctx.NewWrapper( (System.Reflection.PropertyInfo) nArg.MemberInfo ) : ctx.NewWrapper( (System.Reflection.FieldInfo) nArg.MemberInfo ) ),
               CILCustomAttributeFactory.NewTypedArgument( ctx.NewWrapperAsType( nArg.TypedValue.ArgumentType ), this.ExtractValue( ctx, nArg.TypedValue ) ) ) )
            );
      }

      private Object ExtractValue( CILReflectionContext ctx, System.Reflection.CustomAttributeTypedArgument typedArg )
      {
         var retVal = typedArg.Value;
         var array = retVal as System.Collections.ObjectModel.ReadOnlyCollection<System.Reflection.CustomAttributeTypedArgument>;
         if ( array != null )
         {
            retVal = array
               .Select( arg => CILCustomAttributeFactory.NewTypedArgument( ctx.NewWrapperAsType( arg.ArgumentType ), this.ExtractValue( ctx, arg ) ) )
               .ToList();
         }
         return retVal;
      }

      /// <inheritdoc />
      public virtual IEnumerable<System.Reflection.MethodInfo> GetEventOtherMethods( System.Reflection.EventInfo evt )
      {
         return evt.GetOtherMethods( true );
      }

      /// <inheritdoc />
      public virtual Object GetConstantValueFor( System.Reflection.PropertyInfo property )
      {
         Object result = null;
         try
         {
            result = property.GetRawConstantValue();
         }
         catch
         {
            // Ignore - unmanaged stuff sometimes throws here
         }
         return result;
      }

      /// <inheritdoc />
      public virtual Object GetConstantValueFor( System.Reflection.FieldInfo field )
      {
         Object result = null;
         try
         {
            result = field.GetRawConstantValue();
         }
         catch
         {
            // Ignore - unmanaged stuff sometimes throws here
         }
         return result;
      }

      /// <inheritdoc />
      public virtual Object GetConstantValueFor( System.Reflection.ParameterInfo parameter )
      {
         Object result = null;
         try
         {
            result = parameter.RawDefaultValue;
         }
         catch
         {
            // Ignore - unmanaged stuff sometimes throws here
         }
         return result;
      }

      /// <inheritdoc />
      public virtual IDictionary<System.Reflection.MethodInfo, System.Reflection.MethodInfo[]> GetExplicitlyImplementedMethods( Type type )
      {
         return type.GetInterfaces()
            .Select( iFace => type.GetInterfaceMap( iFace ) )
            .SelectMany( map => map.TargetMethods.Select( ( tMethod, idx ) => Tuple.Create( tMethod, map.InterfaceMethods[idx] ) ) )
            .GroupBy( tuple => tuple.Item1 )
            .ToDictionary( grp => grp.Key, grp => grp.Select( tuple => tuple.Item2 ).ToArray() );
      }

      /// <inheritdoc />
      public virtual void GetMethodBodyData( System.Reflection.MethodBase method, out IEnumerable<Tuple<Boolean, Type>> locals, out Boolean initLocals, out IEnumerable<Tuple<ExceptionBlockType, int, int, int, int, Type, int>> exceptionBlocks, out byte[] ilBytes )
      {
         var body = method.GetMethodBody();
         locals = body.LocalVariables
            .OrderBy( local => local.LocalIndex )
            .Select( local => Tuple.Create( local.IsPinned, local.LocalType ) );
         initLocals = body.InitLocals;
         exceptionBlocks = body.ExceptionHandlingClauses
            .Select( clause => Tuple.Create( (ExceptionBlockType) clause.Flags, clause.TryOffset, clause.TryLength, clause.HandlerOffset, clause.HandlerLength, clause.Flags == System.Reflection.ExceptionHandlingClauseOptions.Clause ? clause.CatchType : null, clause.Flags == System.Reflection.ExceptionHandlingClauseOptions.Filter ? clause.FilterOffset : -1 ) );
         ilBytes = body.GetILAsByteArray();
      }

      /// <inheritdoc />
      public virtual System.Reflection.MemberInfo ResolveTypeOrMember( System.Reflection.Module module, Int32 token, Type[] typeGenericArguments, Type[] methodGenericArguments )
      {
         return module.ResolveMember( token, typeGenericArguments, methodGenericArguments );
      }

      /// <inheritdoc />
      public virtual String ResolveString( System.Reflection.Module module, Int32 token )
      {
         return module.ResolveString( token );
      }

      /// <inheritdoc />
      public virtual Byte[] ResolveSignature( System.Reflection.Module module, Int32 token )
      {
         return module.ResolveSignature( token );
      }

      /// <inheritdoc />
      public virtual MethodImplAttributes GetMethodImplementationAttributes( System.Reflection.MethodBase method )
      {
         return (MethodImplAttributes) method.GetMethodImplementationFlags();
      }

      /// <inheritdoc />
      public virtual StructLayoutAttribute GetStructLayoutAttribute( Type type )
      {
         return type.StructLayoutAttribute;
      }

      /// <inheritdoc />
      public virtual void GetAssemblyNameInformation( System.Reflection.Assembly assembly, out AssemblyHashAlgorithm hashAlgorithm, out AssemblyFlags flags, out Byte[] publicKey )
      {
         var name = assembly.GetName();
         hashAlgorithm = (AssemblyHashAlgorithm) name.HashAlgorithm;
         flags = (AssemblyFlags) name.Flags;
         publicKey = name.GetPublicKey();
      }

      /// <inheritdoc />
      public virtual void GetCustomModifiersFor( System.Reflection.FieldInfo field, out Type[] optionalModifiers, out Type[] requiredModifiers )
      {
         optionalModifiers = field.GetOptionalCustomModifiers();
         requiredModifiers = field.GetRequiredCustomModifiers();
      }

      /// <inheritdoc />
      public virtual void GetCustomModifiersFor( System.Reflection.PropertyInfo property, out Type[] optionalModifiers, out Type[] requiredModifiers )
      {
         optionalModifiers = property.GetOptionalCustomModifiers();
         requiredModifiers = property.GetRequiredCustomModifiers();
      }

      /// <inheritdoc />
      public virtual void GetCustomModifiersFor( System.Reflection.ParameterInfo parameter, out Type[] optionalModifiers, out Type[] requiredModifiers )
      {
         optionalModifiers = parameter.GetOptionalCustomModifiers();
         requiredModifiers = parameter.GetRequiredCustomModifiers();
      }
   }

}
#endif
