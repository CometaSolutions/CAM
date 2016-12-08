/*
* Copyright 2016 Stanislav Muhametsin. All rights Reserved.
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TabularMetaData;
using UtilPack;

namespace CILAssemblyManipulator.Physical
{
#pragma warning disable 1591

   public class EmittingNativeHelper
   {
      private readonly Dictionary<Type, TableIndex> _typeRefOrSpecs;
      private readonly Dictionary<System.Reflection.MethodBase, TableIndex> _memberRefOrSpecs;

      public EmittingNativeHelper( CILMetaData module )
      {
         this.Module = ArgumentValidator.ValidateNotNull( "Module", module );
         this._typeRefOrSpecs = new Dictionary<Type, TableIndex>();
         this._memberRefOrSpecs = new Dictionary<System.Reflection.MethodBase, TableIndex>();
      }

      public CILMetaData Module { get; }

      public TableIndex GetTypeRefOrSpec( Type nativeType )
      {
         return this._typeRefOrSpecs.GetOrAdd_NotThreadSafe( nativeType, ( nType ) =>
         {
            var assemblyRef = new AssemblyReference();
            Boolean wasFullPublicKey;
            AssemblyInformation.Parse( nType.Assembly.FullName, out wasFullPublicKey ).DeepCopyContentsTo( assemblyRef.AssemblyInformation );
            if ( wasFullPublicKey )
            {
               assemblyRef.Attributes |= AssemblyFlags.PublicKey;
            }
            var module = this.Module;
            var aRefs = module.AssemblyReferences;
            var aRefEquality = aRefs.TableInformation.EqualityComparer;

            var aRefNumericIdx = aRefs.TableContents.FindIndex( aRef => aRefEquality.Equals( aRef, assemblyRef ) );
            TableIndex aRefIdx;
            if ( aRefNumericIdx >= 0 )
            {
               aRefIdx = new TableIndex( Tables.AssemblyRef, aRefNumericIdx );
            }
            else
            {
               aRefIdx = aRefs.AddRow( assemblyRef );
            }

            return this.GetTypeRefOrSpec( nType, aRefIdx );

         } );
      }

      private TableIndex GetTypeRefOrSpec( Type nativeType, TableIndex assemblyRef )
      {
         TableIndex retVal;
         Type[] gArgs;
         if ( ( gArgs = nativeType.GetGenericArguments() ).Length == 0 || nativeType.IsGenericTypeDefinition )
         {
            // This will be TypeRef
            var typeRef = new TypeReference()
            {
               Name = nativeType.Name
            };
            var declaringType = nativeType.DeclaringType;
            if ( declaringType == null )
            {
               typeRef.Namespace = nativeType.Namespace;
               typeRef.ResolutionScope = assemblyRef;
            }
            else
            {
               typeRef.ResolutionScope = this.GetTypeRefOrSpec( declaringType, assemblyRef );
            }
            var typeRefs = this.Module.TypeReferences;

            var tRefEquality = typeRefs.TableInformation.EqualityComparer;
            var tRefNumericIdx = typeRefs.TableContents.FindIndex( tRef => tRefEquality.Equals( tRef, typeRef ) );
            retVal = tRefNumericIdx >= 0 ?
               new TableIndex( Tables.TypeRef, tRefNumericIdx ) :
               typeRefs.AddRow( typeRef );
         }
         else
         {
            // This will be TypeSpec
            var typeSpec = new TypeSpecification()
            {
               Signature = this.GetTypeSignature( nativeType )
            };

            var typeSpecs = this.Module.TypeSpecifications;
            var tSpecEquality = typeSpecs.TableInformation.EqualityComparer;
            var tSpecNumericIdx = typeSpecs.TableContents.FindIndex( tSpec => tSpecEquality.Equals( tSpec, typeSpec ) );
            retVal = tSpecNumericIdx >= 0 ?
               new TableIndex( Tables.TypeSpec, tSpecNumericIdx ) :
               typeSpecs.AddRow( typeSpec );
         }

         return retVal;
      }

      public TableIndex GetMemberRefOrSpec( System.Reflection.MethodBase methodArg )
      {
         return this._memberRefOrSpecs.GetOrAdd_NotThreadSafe( methodArg, method =>
         {
            var declaringType = method.DeclaringType;
            var methodToUseForSignature = method;
            if ( declaringType.IsGenericType && !declaringType.IsGenericTypeDefinition )
            {
               methodToUseForSignature = System.Reflection.MethodBase.GetMethodFromHandle( method.MethodHandle, declaringType.GetGenericTypeDefinition().TypeHandle );
            }
            var methodAsMethod = methodToUseForSignature as System.Reflection.MethodInfo;

            TableIndex retVal;
            Type[] gArgs;
            if ( methodAsMethod != null && ( gArgs = methodAsMethod.GetGenericArguments() ).Length > 0 && !methodAsMethod.IsGenericMethodDefinition )
            {
               // Generic method
               var sig = new GenericMethodSignature( gArgs.Length );
               sig.GenericArguments.AddRange( gArgs.Select( gArg => this.GetTypeSignature( gArg ) ) );
               var methodSpec = new MethodSpecification()
               {
                  Method = this.GetMemberRefOrSpec( methodAsMethod.GetGenericMethodDefinition() ),
                  Signature = sig
               };

               var mSpecs = this.Module.MethodSpecifications;
               var mSpecEquality = mSpecs.TableInformation.EqualityComparer;
               var mSpecNumericIdex = mSpecs.TableContents.FindIndex( mSpec => mSpecEquality.Equals( mSpec, methodSpec ) );
               retVal = mSpecNumericIdex >= 0 ?
                  new TableIndex( Tables.MethodSpec, mSpecNumericIdex ) :
                  mSpecs.AddRow( methodSpec );
            }
            else
            {
               var paramz = methodToUseForSignature.GetParameters();
               var sig = new MethodReferenceSignature( paramz.Length )
               {
                  MethodSignatureInformation = methodToUseForSignature.IsStatic ? MethodSignatureInformation.Default : MethodSignatureInformation.HasThis,
                  ReturnType = methodAsMethod == null ?
                     new ParameterSignature() { Type = new SimpleTypeSignature( SimpleTypeSignatureKind.Void ) } :
                     this.GetParameterSignature( methodAsMethod.ReturnType )
               };
               if ( method.IsGenericMethodDefinition )
               {
                  sig.MethodSignatureInformation |= MethodSignatureInformation.Generic;
                  sig.GenericArgumentCount = method.GetGenericArguments().Length;
               }
               sig.Parameters.AddRange( paramz.Select( param => this.GetParameterSignature( param.ParameterType ) ) );

               var memberRef = new MemberReference()
               {
                  DeclaringType = this.GetTypeRefOrSpec( declaringType ),
                  Name = method.Name,
                  Signature = sig
               };

               var mRefs = this.Module.MemberReferences;
               var mRefEquality = mRefs.TableInformation.EqualityComparer;
               var mRefNumericIdx = mRefs.TableContents.FindIndex( mRef => mRefEquality.Equals( mRef, memberRef ) );
               retVal = mRefNumericIdx >= 0 ?
                  new TableIndex( Tables.MemberRef, mRefNumericIdx ) :
                  mRefs.AddRow( memberRef );
            }

            return retVal;
         } );
      }


      public ParameterSignature GetParameterSignature( Type parameterType )
      {
         var retVal = new ParameterSignature();
         if ( parameterType.IsByRef )
         {
            retVal.IsByRef = true;
            parameterType = parameterType.GetElementType();
         }
         retVal.Type = this.GetTypeSignature( parameterType );
         return retVal;
      }

      public TypeSignature GetTypeSignature( Type nativeType )
      {
         SimpleTypeSignatureKind? simple;
         switch ( Type.GetTypeCode( nativeType ) )
         {
            case TypeCode.Boolean:
               simple = SimpleTypeSignatureKind.Boolean;
               break;
            case TypeCode.Byte:
               simple = SimpleTypeSignatureKind.U1;
               break;
            case TypeCode.Char:
               simple = SimpleTypeSignatureKind.Char;
               break;
            case TypeCode.Double:
               simple = SimpleTypeSignatureKind.R8;
               break;
            case TypeCode.Int16:
               simple = SimpleTypeSignatureKind.I2;
               break;
            case TypeCode.Int32:
               simple = SimpleTypeSignatureKind.I4;
               break;
            case TypeCode.Int64:
               simple = SimpleTypeSignatureKind.I8;
               break;
            case TypeCode.SByte:
               simple = SimpleTypeSignatureKind.I1;
               break;
            case TypeCode.Single:
               simple = SimpleTypeSignatureKind.R4;
               break;
            case TypeCode.String:
               simple = SimpleTypeSignatureKind.String;
               break;
            case TypeCode.UInt16:
               simple = SimpleTypeSignatureKind.U2;
               break;
            case TypeCode.UInt32:
               simple = SimpleTypeSignatureKind.U4;
               break;
            case TypeCode.UInt64:
               simple = SimpleTypeSignatureKind.U8;
               break;
            default:
               if ( Equals( typeof( Object ), nativeType ) )
               {
                  simple = SimpleTypeSignatureKind.Object;
               }
               else if ( String.Equals( "System.Void", nativeType.FullName ) && nativeType.Assembly == typeof( Object ).Assembly )
               {
                  simple = SimpleTypeSignatureKind.Void;
               }
               else if ( Equals( typeof( IntPtr ), nativeType ) )
               {
                  simple = SimpleTypeSignatureKind.I;
               }
               else if ( Equals( typeof( UIntPtr ), nativeType ) )
               {
                  simple = SimpleTypeSignatureKind.U;
               }
               else
               {
                  simple = null;
               }
               break;
         }

         TypeSignature retVal;
         if ( simple.HasValue )
         {
            retVal = new SimpleTypeSignature( simple.Value );
         }
         else
         {
            if ( nativeType.IsArray )
            {
               // TODO complex array types
               retVal = new SimpleArrayTypeSignature()
               {
                  ArrayType = this.GetTypeSignature( nativeType.GetElementType() )
               };
            }
            else if ( nativeType.IsPointer )
            {
               retVal = new PointerTypeSignature()
               {
                  PointerType = this.GetTypeSignature( nativeType.GetElementType() )
               };
            }
            else if ( nativeType.IsGenericParameter )
            {
               retVal = new GenericParameterTypeSignature()
               {
                  GenericParameterIndex = nativeType.GenericParameterPosition,
                  GenericParameterKind = nativeType.DeclaringMethod == null ? GenericParameterKind.Type : GenericParameterKind.Method
               };
            }
            else
            {
               var gArgs = nativeType.GetGenericArguments();
               retVal = new ClassOrValueTypeSignature( gArgs.Length )
               {
                  TypeReferenceKind = nativeType.IsValueType ? TypeReferenceKind.ValueType : TypeReferenceKind.Class,
                  Type = this.GetTypeRefOrSpec( nativeType.GetGenericDefinitionIfGenericType() )
               };
               if ( gArgs.Length > 0 )
               {
                  var list = ( (ClassOrValueTypeSignature) retVal ).GenericArguments;
                  foreach ( var gArg in gArgs )
                  {
                     list.Add( this.GetTypeSignature( gArg ) );
                  }
               }
            }
         }

         return retVal;
      }
   }
}
