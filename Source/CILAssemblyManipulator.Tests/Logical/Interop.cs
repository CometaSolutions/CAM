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
using CILAssemblyManipulator.Logical;
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Structural;
using CILAssemblyManipulator.Tests.Logical;
using CommonUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CILAssemblyManipulator.Tests.Logical
{
   [Category( "CAM.Logical" )]
   public class LogicalInteropTest : AbstractCAMTest
   {

      [Test]
      public void TestPhysicalInteropCAMPhysicalAssembly()
      {
         PerformRoundtripTest( CAMPhysicalLocation );
      }

      [Test]
      public void TestPhysicalInteropCAMStructuralAssembly()
      {
         PerformRoundtripTest( CAMStructuralLocation );
      }

      [Test]
      public void TestPhysicalInteropCAMLogicalAssembly()
      {
         PerformRoundtripTest( CAMLogicalLocation );
      }

      [Test]
      public void TestPhysicalInteropMSCorLibAssembly()
      {
         PerformRoundtripTest( MSCorLibLocation );
      }

      //[Test]
      //public void TestCustomAttributeTypeArray_Simple()
      //{
      //   var nType = typeof( Object );
      //   PerformTestCustomAttributeTypeArrayTest( null, new[] { nType, nType }, ctx => new[] { ctx.NewWrapperAsType( nType ), ctx.NewWrapperAsType( nType ) } );
      //}

      //[Test]
      //public void TestCustomAttributeTypeArray_Jagged()
      //{

      //   var nType = typeof( Object );
      //   PerformTestCustomAttributeTypeArrayTest(
      //      null,
      //      new[]
      //      {
      //         new[]
      //         {
      //            new[]
      //            {
      //               nType,
      //               nType
      //            },
      //            new[]
      //            {
      //               nType,
      //               nType
      //            }
      //         },
      //         new[]
      //         {
      //            new[]
      //            {
      //               nType
      //            }
      //         },
      //         new[]
      //         {
      //            new[]
      //            {
      //               nType,
      //               nType,
      //               nType
      //            }
      //         }
      //      },
      //      ctx =>
      //      new[]
      //      {
      //         new[]
      //         {
      //            new[]
      //            {
      //               ctx.NewWrapperAsType( nType),
      //               ctx.NewWrapperAsType(nType)
      //            },
      //            new[]
      //            {
      //               ctx.NewWrapperAsType(nType),
      //               ctx.NewWrapperAsType(nType)
      //            }
      //         },
      //         new[]
      //         {
      //            new[]
      //            {
      //               ctx.NewWrapperAsType(nType)
      //            }
      //         },
      //         new[]
      //         {
      //            new[]
      //            {
      //               ctx.NewWrapperAsType(nType),
      //               ctx.NewWrapperAsType(nType),
      //               ctx.NewWrapperAsType(nType)
      //            }
      //         }
      //      } );
      //}

      //private static void PerformTestCustomAttributeTypeArrayTest( Type nativeTypeArrayType, Array nativeTypeArray, Func<CILReflectionContext, Array> cilTypeArray )
      //{
      //   if ( nativeTypeArrayType == null )
      //   {
      //      nativeTypeArrayType = nativeTypeArray.GetType();
      //   }

      //   PerformTest( ctx =>
      //   {
      //      var assembly = ctx.NewBlankAssembly( "Testing" );
      //      var mod = assembly.AddModule( "Testing" );
      //      var objType = ctx.NewWrapperAsType( typeof( Object ) );
      //      mod.AssociatedMSCorLibModule = objType.Module;


      //      var type = mod.AddType( "TestCustomAttribute", TypeAttributes.AutoClass );
      //      type.BaseType = ctx.NewWrapperAsType( typeof( Attribute ) );

      //      // Add property
      //      CILField propField;
      //      var nativeTypeArrayTypeWrapper = ctx.NewWrapperAsType( nativeTypeArrayType );
      //      var prop = type.AddAutoProperty( "Types", objType, true, true, out propField );
      //      var caCtor = type.AddDefaultConstructor( MethodAttributes.Public );

      //      assembly.AddCustomAttribute(
      //         caCtor,
      //         null,
      //         new[]
      //         {
      //            CILCustomAttributeFactory.NewNamedArgument(
      //               prop,
      //               CILCustomAttributeFactory.NewTypedArgument( nativeTypeArrayTypeWrapper, cilTypeArray( ctx ) )
      //            )
      //         } );

      //      var phys = mod.CreatePhysicalRepresentation();

      //      var caSig = (CustomAttributeSignature) phys.CustomAttributeDefinitions.TableContents[0].Signature;
      //      var caValue = caSig.NamedArguments[0].Value;
      //      var physicalValueType = typeof( CustomAttributeTypeReference );
      //      do
      //      {
      //         physicalValueType = physicalValueType.MakeArrayType();
      //         nativeTypeArrayType = nativeTypeArrayType.GetElementType();
      //      } while ( nativeTypeArrayType.IsArray );

      //      if ( nativeTypeArray != null )
      //      {
      //         var valType = caValue.Value.GetType();
      //         Assert.AreEqual( physicalValueType, valType );
      //      }

      //      System.Reflection.Assembly nativeAssembly;
      //      using ( var strm = new MemoryStream() )
      //      {
      //         phys.WriteModule( strm );
      //         nativeAssembly = System.Reflection.Assembly.Load( strm.ToArray() );
      //      }

      //      var nativeTypedValue = nativeAssembly.GetCustomAttributesData()[0].NamedArguments[0].TypedValue;
      //      //Assert.AreEqual( nativeTypeArrayType, nativeTypedValue.ArgumentType );
      //      if ( nativeTypeArray == null )
      //      {
      //         Assert.IsNull( nativeTypedValue.Value );
      //      }
      //      else
      //      {

      //         var emittedTypes = nativeTypedValue.AsDepthFirstEnumerable( a => a.ArgumentType.IsArray ? (IEnumerable<System.Reflection.CustomAttributeTypedArgument>) a.Value : Empty<System.Reflection.CustomAttributeTypedArgument>.Enumerable )
      //            .Select( a => a.Value )
      //            .OfType<Type>()
      //            .ToArray();

      //         var givenTypes = nativeTypeArray.AsDepthFirstEnumerable<Object>( a => a is Array ? (IEnumerable<Object>) a : Empty<Object>.Enumerable )
      //            .OfType<Type>()
      //            .ToArray();
      //         Assert.IsTrue( ArrayEqualityComparer<Type>.ArrayEquality( emittedTypes, givenTypes ) );
      //      }

      //   } );
      //}

      private static void PerformRoundtripTest( String mdLocation )
      {
         CILMetaData md;
         using ( var fs = File.OpenRead( mdLocation ) )
         {
            md = fs.ReadModule();
         }
         PerformTest( ctx =>
         {
            var mdLoader = new CILMetaDataLoaderNotThreadSafeForFiles();
            var loader = new CILAssemblyLoaderNotThreadSafe( ctx, mdLoader );
            var logical = loader.LoadAssemblyFrom( mdLocation );
            var physicalLoaded = mdLoader.GetOrLoadMetaData( mdLocation );
            var physicalCreated = logical.MainModule.CreatePhysicalRepresentation();

            var structure1 = physicalLoaded.CreateStructuralRepresentation();
            structure1.SecurityInfo.Clear(); // CAM.Logical doesn't support security attributes on assemblies.
            var structure2 = physicalCreated.CreateStructuralRepresentation();
            using ( var comparer = new AssemblyEquivalenceComparerTokenMatch( ctx.DefaultCryptoCallbacks ) )
            {
               Assert.IsTrue( comparer.Equals( structure1, structure2 ) );
            }
         } );
      }

      private static void PerformTest( Action<CILReflectionContext> test )
      {
         using ( var ctx = CILReflectionContextFactory.NewContext() )
         {
            test( ctx );
         }
      }
   }
}

