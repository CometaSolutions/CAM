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
using CommonUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Tests.Logical
{
   [Category( "CAM.Logical" )]
   public class WrappersTest : AbstractCAMTest
   {
      [Test]
      public void TestVectorArrayType()
      {
         PerformElementTypeTest( typeof( Object[] ) );
      }

      [Test]
      public void TestMultiDimensionalArrayType()
      {
         PerformElementTypeTest( typeof( Object[,] ) );
      }

      [Test]
      public void TestPointerType()
      {
         PerformElementTypeTest( typeof( Object ).MakePointerType() );
      }

      [Test]
      public void TestByReferenceType()
      {
         PerformElementTypeTest( typeof( Object ).MakeByRefType() );
      }

      private static void PerformElementTypeTest( Type typeNative )
      {
         using ( var ctx = DotNETReflectionContext.CreateDotNETContext() )
         {
            var typeWrapper = ctx.NewWrapperAsType( typeNative );

            var nativeInterfaces = typeNative.GetInterfaces();
            var nativeFields = typeNative.GetFields( System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance );
            var nativeMethods = typeNative.GetMethods( System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance );
            var nativeCtors = typeNative.GetConstructors( System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance );
            var nativeProps = typeNative.GetProperties( System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance );
            var nativeEvts = typeNative.GetEvents( System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance );
            var nativeNested = typeNative.GetNestedTypes( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic );

            var wrapperInterfaces = typeWrapper.DeclaredInterfaces.ToArray();
            var wrapperFields = typeWrapper.DeclaredFields.ToArray();
            var wrapperMethods = typeWrapper.DeclaredMethods.ToArray();
            var wrapperCtors = typeWrapper.Constructors.ToArray();
            var wrapperProps = typeWrapper.DeclaredProperties.ToArray();
            var wrapperEvts = typeWrapper.DeclaredEvents.ToArray();
            var wrapperNested = typeWrapper.DeclaredNestedTypes.ToArray();

            Assert.AreEqual(
               ctx.NewWrapperAsType( typeNative.BaseType ),
               typeWrapper.BaseType,
               "Element base type comparison failed; native type is " + typeNative.BaseType + ", wrapper type is " + typeWrapper.BaseType + "."
               );
            Assert.AreEqual(
               (TypeAttributes) typeNative.Attributes,
               typeWrapper.Attributes,
               "Element type attributes comparison failed; native type attributes are " + typeNative.Attributes + ", wrapper type attributes are " + typeWrapper.Attributes + "."
               );
            Assert.IsTrue( ArrayEqualityComparer<CILType>.IsPermutation( nativeInterfaces.Select( i => ctx.NewWrapperAsType( i ) ).OnlyBottomTypes().ToArray(), wrapperInterfaces ),
               "Element type implemented interfaces comparison failed; native type interfaces are " + String.Join( ", ", nativeInterfaces.Select( i => ctx.NewWrapperAsType( i ) ).OnlyBottomTypes() ) + ", wrapper type interfaces are " + String.Join( ", ", (Object[]) wrapperInterfaces ) + "."
               );
            Assert.IsTrue( ArrayEqualityComparer<CILField>.IsPermutation( nativeFields.Select( f => ctx.NewWrapper( f ) ).ToArray(), wrapperFields ),
               "Element type fields comparison failed; native type fields are " + String.Join( ", ", (Object[]) nativeFields ) + ", wrapper type fields are " + String.Join( ", ", (Object[]) wrapperFields ) + "."
               );
            Assert.IsTrue( ArrayEqualityComparer<CILMethod>.ArrayEquality( nativeMethods.Select( m => ctx.NewWrapper( m ) ).ToArray(), wrapperMethods ),
               "Element type methods comparison failed; native type methods are " + String.Join( ", ", (Object[]) nativeMethods ) + ", wrapper type methods are " + String.Join( ", ", (Object[]) wrapperMethods ) + "."
               );
            Assert.IsTrue( ArrayEqualityComparer<CILConstructor>.IsPermutation( nativeCtors.Select( c => ctx.NewWrapper( c ) ).ToArray(), wrapperCtors ),
               "Element type constructors comparison failed; native type constructors are " + String.Join( ", ", (Object[]) nativeCtors ) + ", wrapper type constructors are " + String.Join( ", ", (Object[]) wrapperCtors ) + "."
               );
            Assert.IsTrue( ArrayEqualityComparer<CILProperty>.IsPermutation( nativeProps.Select( p => ctx.NewWrapper( p ) ).ToArray(), wrapperProps ),
               "Element type properties comparison failed; native type properties are " + String.Join( ", ", (Object[]) nativeProps ) + ", wrapper type properties are " + String.Join( ", ", (Object[]) wrapperProps ) + "."
               );
            Assert.IsTrue( ArrayEqualityComparer<CILEvent>.IsPermutation( nativeEvts.Select( e => ctx.NewWrapper( e ) ).ToArray(), wrapperEvts ),
               "Element type events comparison failed; native type events are " + String.Join( ", ", (Object[]) nativeEvts ) + ", wrapper type events are " + String.Join( ", ", (Object[]) wrapperEvts ) + "."
               );
            Assert.IsTrue( ArrayEqualityComparer<CILType>.IsPermutation( nativeNested.Select( n => ctx.NewWrapperAsType( n ) ).ToArray(), wrapperNested ),
               "Element type nested types comparison failed; native type nested types are " + String.Join( ", ", (Object[]) nativeNested ) + ", wrapper type nested types are " + String.Join( ", ", (Object[]) wrapperNested ) + "."
               );
         }
      }
   }
}
