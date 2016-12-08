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
using NUnit.Framework;
using CILAssemblyManipulator.Logical;
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Tests.Logical
{
   [Category( "CAM.Logical" )]
   public class ILTests : AbstractCAMTest
   {
      [Test]
      public void TestBranchCalculation1()
      {
         using ( var ctx = CILReflectionContextFactory.NewContext() )
         {
            var assembly = ctx.NewBlankAssembly( "Testing" );
            var mod = assembly.AddModule( "Testing" );
            var objType = ctx.NewWrapperAsType( typeof( Object ) );
            var objMethod = objType.DeclaredMethods[0];
            var objCtor = objType.Constructors[0];

            mod.AssociatedMSCorLibModule = objType.Module;

            var type = mod.AddType( "Testing", TypeAttributes.AutoClass );
            var method = type.AddMethodWithReturnType( "Testing", MethodAttributes.Public, CallingConventions.HasThis, mod.GetTypeForTypeCode( CILTypeCode.Void ) );
            var field = type.AddField( "Testingg", objType, FieldAttributes.Private );
            var il = method.MethodIL;
            var local0 = il.DeclareLocal( objType );
            var local1 = il.DeclareLocal( objType );
            var local2 = il.DeclareLocal( objType );

            il.EmitLoadThis()
               .EmitStoreLocal( local0 )
               .EmitLoadLocal( local0 )
               .EmitReflectionObjectOf( objType )
               .EmitCall( objMethod )
               .EmitStoreLocal( local2 )
               .EmitLoadLocal( local2 )
               .EmitSwitch( 1, ( il2, cases, defaultLabel, endLabel ) =>
               {
                  il2.MarkLabel( cases[0] );
                  var label1 = il2.DefineLabel();
                  var label2 = il2.DefineLabel();
                  il2.EmitNewObject( objCtor )
                     .EmitStoreLocal( local1 )
                     .EmitLoadLocal( local1 )
                     .EmitLoadLocal( local1 )
                     .EmitNewObject( objCtor )
                     .EmitCall( objMethod )
                     .EmitReflectionObjectOf( field )
                     .EmitLoadLocal( local1 )
                     .EmitCall( objMethod )
                     .EmitLoadLocal( local0 )
                     .EmitCall( objMethod )
                     .EmitCall( objMethod )
                     .EmitCall( objMethod )
                     .EmitLoadLocal( local0 )
                     .EmitLoadLocal( local0 )
                     .EmitCall( objMethod )
                     .EmitCall( objMethod )
                     .EmitCall( objMethod )
                     .EmitLoadInt32( 0 )
                     .EmitCall( objMethod )
                     .EmitReflectionObjectOf( objType )
                     .EmitCall( objMethod )
                     .EmitDup()
                     .EmitBranch( BranchType.ALWAYS, label1 )
                     .EmitBranch( BranchType.ALWAYS, label2 )
                     .MarkLabel( label1 )
                     .EmitPop()
                     .EmitLoadString( "" )
                     .EmitLoadLocal( local0 )
                     .EmitCall( objMethod )
                     .EmitCall( objMethod )
                     .EmitCall( objMethod )
                     .EmitLoadInt32( 0 )
                     .EmitCall( objMethod )
                     .EmitLoadString( "" )
                     .EmitCall( objMethod )
                     .EmitReflectionObjectOf( objType )
                     .EmitNewObject( objCtor )
                     .EmitThrow()
                     .MarkLabel( label2 )
                     .EmitCall( objMethod )
                     .EmitBranch( BranchType.ALWAYS, endLabel );
               }, ( il2, endLabel ) =>
               {
                  il2.EmitLoadString( "" )
                     .EmitLoadLocal( local2 )
                     .Add( new LogicalOpCodeInfoWithTypeToken( OpCodes.Box, objType ) )
                     .EmitLoadString( "" )
                     .EmitCall( objMethod )
                     .EmitReflectionObjectOf( objType )
                     .EmitLoadString( "" )
                     .EmitCall( objMethod )
                     .EmitNewObject( objCtor )
                     .EmitThrow();
               } );

            il.EmitLoadLocal( local1 )
               .EmitReturn();


            Assert.DoesNotThrow( () => assembly.MainModule.CreatePhysicalRepresentation( orderAndRemoveDuplicates: false ) );
         }
      }

      [Test]
      public void TestReflectionObjectOf()
      {
         using ( var ctx = CILReflectionContextFactory.NewContext() )
         {
            var assembly = ctx.NewBlankAssembly( "Testing" );
            var mod = assembly.AddModule( "Testing" );
            var objType = ctx.NewWrapperAsType( typeof( Object ) );
            var objMethod = objType.DeclaredMethods[0];
            var objCtor = objType.Constructors[0];

            mod.AssociatedMSCorLibModule = objType.Module;

            var methodRef = ctx.NewWrapper( typeof( Enum ).LoadMethodGDefinitionOrThrow( "TryParse" ) );

            var type = mod.AddType( "Testing", TypeAttributes.AutoClass );
            var method = type.AddMethodWithReturnType( "Testing", MethodAttributes.Public, CallingConventions.HasThis, mod.GetTypeForTypeCode( CILTypeCode.Void ) );
            var il = method.MethodIL;

            il.EmitReflectionObjectOf( methodRef, TypeTokenKind.GenericDefinition, MethodTokenKind.GenericDefinition, false );
            il.EmitReflectionObjectOf( methodRef, TypeTokenKind.GenericDefinition, MethodTokenKind.GenericInstantiation );
            il.EmitReflectionObjectOf( methodRef, TypeTokenKind.GenericInstantiation, MethodTokenKind.GenericDefinition, false );
            il.EmitReflectionObjectOf( methodRef, TypeTokenKind.GenericInstantiation, MethodTokenKind.GenericInstantiation );

            var phys = mod.CreatePhysicalRepresentation();

            var physIL = phys.MethodDefinitions.TableContents[0].IL;

            var curCode = physIL.OpCodes[0];
            Assert.IsInstanceOf<OpCodeInfoWithOperand<TableIndex>>( curCode );
            Assert.IsTrue( ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand.Table == Tables.MemberRef );
            curCode = physIL.OpCodes[4];
            Assert.IsInstanceOf<OpCodeInfoWithOperand<TableIndex>>( curCode );
            Assert.IsTrue( ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand.Table == Tables.MethodSpec );
            curCode = physIL.OpCodes[8];
            Assert.IsInstanceOf<OpCodeInfoWithOperand<TableIndex>>( curCode );
            Assert.IsTrue( ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand.Table == Tables.MemberRef );
            curCode = physIL.OpCodes[12];
            Assert.IsInstanceOf<OpCodeInfoWithOperand<TableIndex>>( curCode );
            Assert.IsTrue( ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand.Table == Tables.MethodSpec );
         }
      }

      [Test]
      public void TestReflectionObjectOf2()
      {
         using ( var ctx = CILReflectionContextFactory.NewContext() )
         {
            var assembly = ctx.NewBlankAssembly( "Testing" );
            var mod = assembly.AddModule( "Testing" );
            mod.AssociatedMSCorLibModule = ctx.NewWrapperAsType( typeof( Object ) ).Module;

            var type = mod.AddType( "Testing", TypeAttributes.AutoClass );
            var method = type.AddMethodWithReturnType( "Testing", MethodAttributes.Public, CallingConventions.HasThis, mod.GetTypeForTypeCode( CILTypeCode.Void ) );
            var il = method.MethodIL;

            il.EmitReflectionObjectOf( method, TypeTokenKind.GenericDefinition, MethodTokenKind.GenericDefinition, false );
            il.EmitReflectionObjectOf( method, TypeTokenKind.GenericDefinition, MethodTokenKind.GenericInstantiation );
            il.EmitReflectionObjectOf( method, TypeTokenKind.GenericInstantiation, MethodTokenKind.GenericDefinition, false );
            il.EmitReflectionObjectOf( method, TypeTokenKind.GenericInstantiation, MethodTokenKind.GenericInstantiation );

            var phys = mod.CreatePhysicalRepresentation();

            var physIL = phys.MethodDefinitions.TableContents[0].IL;

            var curCode = physIL.OpCodes[0];
            Assert.IsInstanceOf<OpCodeInfoWithOperand<TableIndex>>( curCode );
            var token = ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand;
            Assert.IsTrue( token.Table == Tables.MethodDef );

            curCode = physIL.OpCodes[4];
            Assert.IsInstanceOf<OpCodeInfoWithOperand<TableIndex>>( curCode );
            token = ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand;
            Assert.IsTrue( token.Table == Tables.MethodDef );

            curCode = physIL.OpCodes[8];
            Assert.IsInstanceOf<OpCodeInfoWithOperand<TableIndex>>( curCode );
            token = ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand;
            Assert.IsTrue( ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand.Table == Tables.MethodDef );

            curCode = physIL.OpCodes[12];
            Assert.IsInstanceOf<OpCodeInfoWithOperand<TableIndex>>( curCode );
            token = ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand;
            Assert.IsTrue( ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand.Table == Tables.MethodDef );
         }
      }

      [Test]
      public void TestReflectionObjectOf3()
      {
         using ( var ctx = CILReflectionContextFactory.NewContext() )
         {
            var assembly = ctx.NewBlankAssembly( "Testing" );
            var mod = assembly.AddModule( "Testing" );
            mod.AssociatedMSCorLibModule = ctx.NewWrapperAsType( typeof( Object ) ).Module;

            var type = mod.AddType( "Testing", TypeAttributes.AutoClass );
            type.DefineGenericParameters( "T" );
            var method = type.AddMethodWithReturnType( "Testing", MethodAttributes.Public, CallingConventions.HasThis, mod.GetTypeForTypeCode( CILTypeCode.Void ) );
            method.DefineGenericParameters( "U" );
            var il = method.MethodIL;

            il.EmitReflectionObjectOf( method, TypeTokenKind.GenericDefinition, MethodTokenKind.GenericDefinition, false );
            il.EmitReflectionObjectOf( method, TypeTokenKind.GenericDefinition, MethodTokenKind.GenericInstantiation );
            il.EmitReflectionObjectOf( method, TypeTokenKind.GenericInstantiation, MethodTokenKind.GenericDefinition, false );
            il.EmitReflectionObjectOf( method, TypeTokenKind.GenericInstantiation, MethodTokenKind.GenericInstantiation );

            var phys = mod.CreatePhysicalRepresentation();

            var physIL = phys.MethodDefinitions.TableContents[0].IL;

            var curCode = physIL.OpCodes[0];
            Assert.IsInstanceOf<OpCodeInfoWithOperand<TableIndex>>( curCode );
            var token = ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand;
            Assert.IsTrue( token.Table == Tables.MethodDef );

            curCode = physIL.OpCodes[4];
            Assert.IsInstanceOf<OpCodeInfoWithOperand<TableIndex>>( curCode );
            token = ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand;
            Assert.IsTrue( token.Table == Tables.MethodSpec );
            token = phys.MethodSpecifications.TableContents[token.Index].Method;
            Assert.IsTrue( token.Table == Tables.MethodDef );

            curCode = physIL.OpCodes[8];
            Assert.IsInstanceOf<OpCodeInfoWithOperand<TableIndex>>( curCode );
            token = ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand;
            Assert.IsTrue( ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand.Table == Tables.MemberRef );

            curCode = physIL.OpCodes[12];
            Assert.IsInstanceOf<OpCodeInfoWithOperand<TableIndex>>( curCode );
            token = ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand;
            Assert.IsTrue( ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand.Table == Tables.MethodSpec );
            token = phys.MethodSpecifications.TableContents[token.Index].Method;
            Assert.IsTrue( token.Table == Tables.MemberRef );
         }
      }

      [Test]
      public void TestReflectionObjectOf4()
      {
         using ( var ctx = CILReflectionContextFactory.NewContext() )
         {
            var assembly = ctx.NewBlankAssembly( "Testing" );
            var mod = assembly.AddModule( "Testing" );
            var objType = ctx.NewWrapperAsType( typeof( Object ) );
            var listType = ctx.NewWrapperAsType( typeof( IList<> ) );
            var listStringType = ctx.NewWrapperAsType( typeof( IList<String> ) );
            mod.AssociatedMSCorLibModule = objType.Module;

            var type = mod.AddType( "Testing", TypeAttributes.AutoClass );
            type.DefineGenericParameters( "T" );
            var method = type.AddMethodWithReturnType( "Testing", MethodAttributes.Public, CallingConventions.HasThis, mod.GetTypeForTypeCode( CILTypeCode.Void ) );
            method.DefineGenericParameters( "U" );
            var il = method.MethodIL;


            il.EmitReflectionObjectOf( type, TypeTokenKind.GenericDefinition );
            il.EmitReflectionObjectOf( type, TypeTokenKind.GenericInstantiation );
            il.EmitReflectionObjectOf( listType, TypeTokenKind.GenericDefinition );
            il.EmitReflectionObjectOf( listType, TypeTokenKind.GenericInstantiation );
            il.EmitReflectionObjectOf( listStringType, TypeTokenKind.GenericDefinition );
            il.EmitReflectionObjectOf( listStringType, TypeTokenKind.GenericInstantiation );


            var phys = mod.CreatePhysicalRepresentation();

            var physIL = phys.MethodDefinitions.TableContents[0].IL;

            var curCode = physIL.OpCodes[0];
            Assert.IsInstanceOf<OpCodeInfoWithOperand<TableIndex>>( curCode );
            var token = ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand;
            Assert.IsTrue( token.Table == Tables.TypeDef );

            curCode = physIL.OpCodes[2];
            Assert.IsInstanceOf<OpCodeInfoWithOperand<TableIndex>>( curCode );
            token = ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand;
            Assert.IsTrue( token.Table == Tables.TypeSpec );

            curCode = physIL.OpCodes[4];
            Assert.IsInstanceOf<OpCodeInfoWithOperand<TableIndex>>( curCode );
            token = ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand;
            Assert.IsTrue( token.Table == Tables.TypeRef );

            curCode = physIL.OpCodes[6];
            Assert.IsInstanceOf<OpCodeInfoWithOperand<TableIndex>>( curCode );
            token = ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand;
            Assert.IsTrue( token.Table == Tables.TypeRef );

            curCode = physIL.OpCodes[8];
            Assert.IsInstanceOf<OpCodeInfoWithOperand<TableIndex>>( curCode );
            token = ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand;
            Assert.IsTrue( token.Table == Tables.TypeSpec );

            curCode = physIL.OpCodes[10];
            Assert.IsInstanceOf<OpCodeInfoWithOperand<TableIndex>>( curCode );
            token = ( (OpCodeInfoWithOperand<TableIndex>) curCode ).Operand;
            Assert.IsTrue( token.Table == Tables.TypeSpec );

         }
      }

   }
}
