///*
// * Copyright 2015 Stanislav Muhametsin. All rights Reserved.
// *
// * Licensed  under the  Apache License,  Version 2.0  (the "License");
// * you may not use  this file  except in  compliance with the License.
// * You may obtain a copy of the License at
// *
// *   http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed  under the  License is distributed on an "AS IS" BASIS,
// * WITHOUT  WARRANTIES OR CONDITIONS  OF ANY KIND, either  express  or
// * implied.
// *
// * See the License for the specific language governing permissions and
// * limitations under the License. 
// */
//using CILAssemblyManipulator.Logical;
//using CILAssemblyManipulator.Physical;
//using CILAssemblyManipulator.Structural;
//using CommonUtils;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//public static partial class E_CILLogical
//{
//   private sealed class LogicalFromStructuralState
//   {
//      public IDictionary<TypeDefinitionStructure, CILType> TypeDefs
//      {
//         get
//         {
//            throw new NotImplementedException();
//         }
//      }

//      public IDictionary<FieldStructure, CILField> FieldDefs
//      {
//         get
//         {
//            throw new NotImplementedException();
//         }
//      }

//      public IDictionary<MethodStructure, CILMethodBase> MethodDefs
//      {
//         get
//         {
//            throw new NotImplementedException();
//         }
//      }

//      public IDictionary<ParameterStructure, CILParameter> ParamDefs
//      {
//         get
//         {
//            throw new NotImplementedException();
//         }
//      }

//      public IDictionary<AbstractTypeStructure, CILTypeBase> TypeDefsOrRefsOrSpecs
//      {
//         get
//         {
//            throw new NotImplementedException();
//         }
//      }
//   }

//   public static CILAssembly CreateLogicalRepresentation( this CILReflectionContext ctx, AssemblyStructure assembly )
//   {
//      // TODO - this might not be worth after all
//      // The process is almost the same as creating logical from physical
//      // So structural -> logical conversion would just add extra performance hit when converting physical -> structural
//      // Most likely best thing is to just have physical -> logical conversion, and no structural -> logical.
//   }

//   private static CILModule CreateLogicalRepresentation( this CILAssembly assembly, ModuleStructure module )
//   {
//      var moduleL = assembly.AddModule( module.Name );

//      var state = new LogicalFromStructuralState();

//      var moduleInit = module.TopLevelTypeDefinitions.FirstOrDefault( t => String.IsNullOrEmpty( t.Namespace ) && String.Equals( t.Name, "<Module>" ) );
//      if ( moduleInit != null )
//      {
//         state.ProcessNewlyCreatedType( moduleInit, moduleL.ModuleInitializer );
//      }

//      foreach ( var type in module.TopLevelTypeDefinitions.Where( t => !ReferenceEquals( moduleInit, t ) ) )
//      {
//         state.CreateLogicalType( moduleL, type );
//      }


//   }

//   private static void CreateLogicalType( this LogicalFromStructuralState state, CILElementCapableOfDefiningType parent, TypeDefinitionStructure type )
//   {
//      CILTypeCode tc;
//      FieldStructure enumField;
//      if ( type.IsEnum() && type.TryResolveEnumValueField( out enumField ) )
//      {
//         tc = enumField.Signature.ResolveTypeCodeFromEnumFieldSignature();
//      }
//      else
//      {
//         tc = ResolveTypeCodeTextual( type.Namespace, type.Name );
//      }

//      var typeL = parent.AddType( type.Name, type.Attributes, tc );
//      typeL.Namespace = type.Namespace;
//      state.ProcessNewlyCreatedType( type, typeL );
//   }

//   private static void ProcessNewlyCreatedType( this LogicalFromStructuralState state, TypeDefinitionStructure type, CILType typeL )
//   {
//      state.TypeDefs.Add( type, typeL );
//      state.TypeDefsOrRefsOrSpecs.Add( type, typeL );

//      // Fields
//      foreach ( var field in type.Fields )
//      {
//         state.FieldDefs.Add( field, typeL.AddField( field.Name, null, field.Attributes ) );
//      }

//      // Methods

//   }

//   private static Boolean IsEnum( this TypeDefinitionStructure type )
//   {
//      var bt = type.BaseType;
//      if ( bt == null )
//      {
//         return false;
//      }
//      else
//      {
//         switch ( bt.TypeStructureKind )
//         {
//            case TypeStructureKind.TypeDef:
//               var tDef = (TypeDefinitionStructure) bt;
//               return String.Equals( Consts.SYSTEM_NS, tDef.Namespace )
//                  && String.Equals( "Enum", tDef.Name );
//            case TypeStructureKind.TypeRef:
//               var tRef = (TypeReferenceStructure) bt;
//               return String.Equals( Consts.SYSTEM_NS, tRef.Namespace )
//                  && String.Equals( "Enum", tRef.Name );
//            default:
//               return false;
//         }
//      }
//   }

//   private static Boolean TryResolveEnumValueField( this TypeDefinitionStructure type, out FieldStructure field )
//   {
//      field = type.Fields.FirstOrDefault( f => !f.Attributes.IsStatic() );
//      return field != null;
//   }

//   private static CILTypeCode ResolveTypeCodeFromEnumFieldSignature( this FieldStructureSignature fSig )
//   {
//      TypeStructureSignature typeSig;
//      if ( fSig != null
//         && ( typeSig = fSig.Type ) != null
//         && typeSig.TypeSignatureKind == TypeStructureSignatureKind.Simple )
//      {
//         switch ( ( (SimpleTypeStructureSignature) typeSig ).SimpleType )
//         {
//            case SignatureElementTypes.Char:
//               return CILTypeCode.Char;
//            case SignatureElementTypes.I1:
//               return CILTypeCode.SByte;
//            case SignatureElementTypes.U1:
//               return CILTypeCode.Byte;
//            case SignatureElementTypes.I2:
//               return CILTypeCode.Int16;
//            case SignatureElementTypes.U2:
//               return CILTypeCode.UInt16;
//            case SignatureElementTypes.I4:
//               return CILTypeCode.Int32;
//            case SignatureElementTypes.U4:
//               return CILTypeCode.UInt64;
//            case SignatureElementTypes.I8:
//               return CILTypeCode.Int64;
//            case SignatureElementTypes.U8:
//               return CILTypeCode.UInt64;
//            default:
//               return CILTypeCode.Object;
//         }
//      }
//      else
//      {
//         return CILTypeCode.Object;
//      }
//   }

//}