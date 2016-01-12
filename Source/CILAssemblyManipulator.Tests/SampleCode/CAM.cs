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
extern alias CAMPhysical;
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical.Meta;
using CAMPhysical::CILAssemblyManipulator.Physical.IO;

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Logical;
using CILAssemblyManipulator.Structural;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Tests.SampleCode
{
   public static class CAMPhysical
   {
      private static void SampleCode()
      {
         // This creates otherwise empty metadata, but its AssemblyDefinition, ModuleDefinition, and TypeDefinition tables will have 1 row each
         // The first argument is assembly name, and second is optional module name
         // When assembly name is supplied, and module name is not, the module name will be assembly name appended by ".dll"
         // The TypeDefinition table will have the module type definition (to hold global methods) as its only row.
         var md = CAMPhysical::CILAssemblyManipulator.Physical.CILMetaDataFactory.CreateMinimalAssembly( "MyAssembly", null );

         // Let's add a type
         md.TypeDefinitions.TableContents.Add( new TypeDefinition()
         {
            Namespace = "MyNamespace",
            Name = "MyType",
            Attributes = TypeAttributes.Class
         } );

         // Let's add an attribute to our newly created type
         // For brevity, our custom attribute will have only one constructor argument, and no named arguments
         // The constructor argument will have its value to be '5'
         var caSig = new CustomAttributeSignature();
         caSig.TypedArguments.Add( new CustomAttributeTypedArgument()
         {
            Value = 5
         } );
         md.CustomAttributeDefinitions.TableContents.Add( new CustomAttributeDefinition()
         {
            Parent = new TableIndex( Tables.TypeDef, 1 ), // Zero-based indexing 
            Type = new TableIndex( Tables.MemberRef, 0 ), // The target does not need to be existing at addition time
            Signature = caSig
         } );

         // In order to write this metadata to disk (replace File.Open with some other way of acquiring some stream relevant to your application):
         using ( var fs = File.Open( "MyAssembly.dll", FileMode.Create, FileAccess.ReadWrite, FileShare.None ) )
         {
            // When nothing else but stream is supplied, the module will be emitted as AnyCPU dll
            md.WriteModule( fs );
         }
         // In .NET environment, this method will do exactly the same as above using-statement
         md.WriteModuleTo( "MyAssembly.dll" );

         // In order to read the module from disk (replace File.OpenRead with some other way of acquiring some stream relevant to your application):
         CILMetaData mdFromDisk;
         using ( var fs = File.OpenRead( "MyAssembly.dll" ) )
         {
            // Need to add using CILAssemblyManipulator.Physical in order to use this extension method
            mdFromDisk = fs.ReadModule();
         }
         // In .NET environment, this method will do exactly the same as above using-statement
         mdFromDisk = CILMetaDataIO.ReadModuleFrom( "MyAssembly.dll" );
      }
   }

   public static class CAMStructural
   {
      private static void SampleCode()
      {
         // Create fresh assembly with a module
         var assembly = new AssemblyStructure();
         assembly.AssemblyInfo.Name = "MyAssembly";
         var module = new ModuleStructure()
         {
            Name = "MyAssembly.dll",
            IsMainModule = true
         };
         assembly.Modules.Add( module );

         // Add a type with custom attribute, like in CAM.Physical example
         var myType = new TypeDefinitionStructure()
         {
            Namespace = "MyNamespace",
            Name = "MyType",
            Attributes = TypeAttributes.Class
         };
         module.TopLevelTypeDefinitions.Add( myType );

         var caSig = new CustomAttributeSignature();
         caSig.TypedArguments.Add( new CustomAttributeTypedArgument()
         {
            Value = 5
         } );
         // Instead of having custom attribute table on module, custom attributes may be directly added to their targets
         myType.CustomAttributes.Add( new CustomAttributeStructure()
         {
            Constructor = null, // We can set this later, no checks are performed on addition time
            Signature = caSig
         } );

         // Convert structural assembly into physical assembly
         CILMetaData physical = assembly.CreatePhysicalRepresentationOfMainModule();
         // The physical metadata can be now written to disk, if needed

         // Convert physical back to structural
         var assembly2 = physical.CreateStructuralRepresentation();

         // This will return true, since both assemblies are structurally equal
         AssemblyEquivalenceComparerExact.ExactEqualityComparer.Equals( assembly, assembly2 );

      }
   }

   public static class CAMLogical
   {
      public static void SampleCode()
      {
         // CAM.Logical has a concept of reflection context, so everything has to be done within it
         using ( var ctx = CILReflectionContextFactory.NewContext() )
         {
            // Create fresh assembly with a module
            var assembly = ctx.NewBlankAssembly( "MyAssembly" );
            var module = assembly.AddModule( "MyAssembly.dll" );

            // Add a type with custom attribute, a bit in CAM.Logical and CAM.Physical example
            // Now we must to supply the constructor to the custom attribute data
            // To save time, we use the feature of CAM.Logical: ability to wrap native reflection elements into CAM.Logical reflection elements
            var myType = module.AddType( "MyType", TypeAttributes.Class );
            myType.AddNewCustomAttributeTypedParams( ctx.NewWrapper( typeof( CLSCompliantAttribute ).GetConstructors()[0] ), CILCustomAttributeFactory.NewTypedArgument( true, ctx ) );

            // Convert logical assembly into physical assembly
            CILMetaData physical = module.CreatePhysicalRepresentation();
            // The physical metadata can be now written to disk, if needed
         }
      }
   }
}
