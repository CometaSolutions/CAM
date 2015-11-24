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
using CILAssemblyManipulator.Physical;
using System.IO;

namespace CILAssemblyManipulator.Tests.Physical
{
   [Category( "CAM.Physical" )]
   public class EmittingTest : AbstractCAMTest
   {

      [Test]
      public void TestEmittingModuleWithTypeButNoMethods()
      {
         const String NAME = "TestType";
         const String NS = "TestNamespace";
         const String ASSEMBLY = "SimpleTestAssembly1";

         var md = CILMetaDataFactory.CreateMinimalAssembly( ASSEMBLY, ASSEMBLY + ".dll" );

         // mscorlib-reference
         //var mscorLib = new AssemblyReference();
         //mscorLib.AssemblyInformation.Name = "mscorlib";
         //mscorLib.Attributes |= AssemblyFlags.Retargetable;
         //md.AssemblyReferences.Add(mscorLib);

         //// System.Object reference
         //var sysObj = new TypeReference()
         //{
         //   Name = "Object",
         //   Namespace = "System",
         //   ResolutionScope = new TableIndex(Tables.AssemblyRef, 0)
         //};
         //md.TypeReferences.Add(sysObj);

         var testType = new TypeDefinition()
         {
            Attributes = TypeAttributes.Interface | TypeAttributes.Abstract,
            //BaseType = new TableIndex( Tables.TypeRef, 0 ),
            Name = NAME,
            Namespace = NS
         };
         md.TypeDefinitions.TableContents.Add( testType );

         TestRuntimeAssembly( md,
            //bytes =>
            //{
            //   File.WriteAllBytes( ASSEMBLY + ".dll", bytes );
            //},
            assembly =>
            {
               var typez = assembly.GetTypes();
               Assert.AreEqual( typez.Length, 1 );
               var type = typez[0];
               Assert.AreEqual( NAME, type.Name );
               Assert.AreEqual( NS, type.Namespace );
               Assert.AreEqual( assembly.GetName().Name, ASSEMBLY );
               Assert.AreEqual( TypeAttributes.Interface | TypeAttributes.Abstract, (TypeAttributes) type.Attributes );
            } );

      }

      private static void TestRuntimeAssembly(
         CILMetaData md,
         //Action<Byte[]> arrayAction,
         Action<System.Reflection.Assembly> action,
         EmittingArguments eArgs = null
         )
      {
         Byte[] bytez;
         using ( var ms = new MemoryStream() )
         {
            md.WriteModule( ms, eArgs );
            bytez = ms.ToArray();
         }
         File.WriteAllBytes( "SimpleTestAssembly1.dll", bytez );
         using ( var ms = new MemoryStream( bytez ) )
         {
            var md2 = ms.ReadModule( null );
            Assert.IsTrue( Comparers.MetaDataComparer.Equals( md, md2 ) );
         }
         //arrayAction( bytez );
         action( System.Reflection.Assembly.Load( bytez ) );
      }
   }
}
