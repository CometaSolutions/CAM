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
   public class EmittingTest : AbstractCAMTest
   {

      [Test]
      public void TestEmittingModuleWithTypeButNoMethods()
      {
         var md = CreateMinimalAssembly( "SimpleTestAssembly1" );

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
            Name = "TestType",
            Namespace = "TestNamespace"
         };
         md.TypeDefinitions.Add( testType );

         TestRuntimeAssembly( md,
            //bytes =>
            //{
            //   File.WriteAllBytes( "SimpleTestAssembly1.dll", bytes );
            //},
            assembly =>
            {
               var typez = assembly.GetTypes();
               Assert.AreEqual( typez.Length, 1 );
            } );

      }

      private static CILMetaData CreateMinimalAssembly( String assemblyName )
      {
         var md = CreateMinimalModule( assemblyName + ".dll" );
         var aDef = new AssemblyDefinition();
         aDef.AssemblyInformation.Name = "SimpleTestAssembly1";
         aDef.HashAlgorithm = AssemblyHashAlgorithm.SHA1;
         md.AssemblyDefinitions.Add( aDef );

         return md;
      }

      private static CILMetaData CreateMinimalModule( String moduleName )
      {
         var md = CILMetaDataFactory.NewMetaData();

         // Module definition
         md.ModuleDefinitions.Add( new ModuleDefinition()
         {
            Name = moduleName,
            ModuleGUID = Guid.NewGuid()
         } );

         // Module type
         md.TypeDefinitions.Add( new TypeDefinition()
         {
            Name = "<Module>"
         } );

         return md;
      }

      private static void TestRuntimeAssembly(
         CILMetaData md,
         /*Action<Byte[]> arrayAction,*/
         Action<System.Reflection.Assembly> action,
         HeadersData headers = null,
         EmittingArguments eArgs = null
         )
      {
         Byte[] bytez;
         using ( var ms = new MemoryStream() )
         {
            md.WriteModule( ms, headers, eArgs );
            bytez = ms.ToArray();
         }
         //arrayAction( bytez );
         action( System.Reflection.Assembly.Load( bytez ) );
      }
   }
}
