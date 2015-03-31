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
using CILAssemblyManipulator.Physical;
using NUnit.Framework;
using CommonUtils;

namespace CILAssemblyManipulator.Tests.Physical
{
   public class OrderingTest : AbstractCAMTest
   {

      [Test]
      public void TestNestedClassOrdering()
      {
         const String NS = "TestNamespace";
         const String NESTED_CLASS_NAME = "NestedType";
         const String ENCLOSING_CLASS_NAME = "EnclosingType";
         var md = CILMetaDataFactory.NewMetaData();

         // Create some types
         md.TypeDefinitions.Add( new TypeDefinition() { Namespace = NS, Name = NESTED_CLASS_NAME } );
         md.TypeDefinitions.Add( new TypeDefinition() { Namespace = NS, Name = ENCLOSING_CLASS_NAME } );

         // Add wrong nested-class definition (enclosing type is greater than nested type)
         md.NestedClassDefinitions.Add( new NestedClassDefinition()
         {
            NestedClass = new TableIndex( Tables.TypeDef, 0 ),
            EnclosingClass = new TableIndex( Tables.TypeDef, 1 )
         } );

         ReOrderAndValidate( md );

         Assert.AreEqual( 1, md.NestedClassDefinitions.Count );
         Assert.AreEqual( 2, md.TypeDefinitions.Count );
         Assert.AreEqual( NESTED_CLASS_NAME, md.TypeDefinitions[md.NestedClassDefinitions[0].NestedClass.Index].Name );
         Assert.AreEqual( ENCLOSING_CLASS_NAME, md.TypeDefinitions[md.NestedClassDefinitions[0].EnclosingClass.Index].Name );
      }

      [Test]
      public void TestMSCorLibOrdering()
      {
         var md = ReadFromFile( MSCorLibLocation );
         ReOrderAndValidate( md.MetaData );
      }


      private static void ReOrderAndValidate( CILMetaData md )
      {
         var oldInfo = new ModuleLogicalInfo( md );
         md.OrderTablesAndUpdateSignatures();
         /////////////////////// Order

         // 1. TypeDef - enclosing class definition must precede nested class definition
         foreach ( var nc in md.NestedClassDefinitions )
         {
            Assert.Less( nc.EnclosingClass.Index, nc.NestedClass.Index );
         }

         // NestedClass - sorted by NestedClass column
         AssertOrderBySingleSimpleColumn( md.NestedClassDefinitions, nc =>
         {
            Assert.AreEqual( nc.NestedClass.Table, Tables.TypeDef );
            Assert.AreEqual( nc.EnclosingClass.Table, Tables.TypeDef );
            return nc.NestedClass.Index;
         } );

         //////////////////////// Integrity
         var newInfo = new ModuleLogicalInfo( md );
         Assert.AreEqual( oldInfo, newInfo );
      }

      private static void AssertOrderBySingleSimpleColumn<T>( List<T> table, Func<T, Int32> pkExtractor )
      {
         for ( var i = 1; i < table.Count; ++i )
         {
            Assert.Less( pkExtractor( table[i - 1] ), pkExtractor( table[i] ) );
         }
      }
   }

   internal sealed class ModuleLogicalInfo : IEquatable<ModuleLogicalInfo>
   {
      private readonly String _name;
      private readonly ISet<TypeLogicalInfo> _types;

      internal ModuleLogicalInfo( CILMetaData md )
      {
         this._types = new HashSet<TypeLogicalInfo>( md.TypeDefinitions.Select( ( td, idx ) => new TypeLogicalInfo( md, idx ) ) );
      }

      public override Boolean Equals( Object obj )
      {
         return this.Equals( obj as ModuleLogicalInfo );
      }

      public override Int32 GetHashCode()
      {
         return this._types.Count * 23;
      }

      public Boolean Equals( ModuleLogicalInfo other )
      {
         var retVal = ReferenceEquals( this, other )
         || (
            other != null
            && this._types.SetEquals( other._types )
            );
         if ( !retVal )
         {

         }

         return retVal;
      }
   }

   internal sealed class TypeLogicalInfo : IEquatable<TypeLogicalInfo>
   {
      private readonly TypeDefinition _type;
      private readonly ISet<MethodLogicalInfo> _methods;
      private readonly ISet<FieldDefinition> _fields;

      internal TypeLogicalInfo( CILMetaData md, Int32 typeDefIndex )
      {
         this._type = md.TypeDefinitions[typeDefIndex];
         this._methods = new HashSet<MethodLogicalInfo>( md.GetTypeMethodIndices( typeDefIndex ).Select( idx => new MethodLogicalInfo( md, idx ) ) );
         this._fields = new HashSet<FieldDefinition>( md.GetTypeFields( typeDefIndex ), ReferenceEqualityComparer<FieldDefinition>.ReferenceBasedComparer );
      }

      public override Boolean Equals( Object obj )
      {
         return this.Equals( obj as TypeLogicalInfo );
      }

      public override Int32 GetHashCode()
      {
         return Comparers.TypeDefinitionEqualityComparer.GetHashCode( this._type );
      }

      public Boolean Equals( TypeLogicalInfo other )
      {
         var retVal = ReferenceEquals( this, other )
         || (
            other != null
            && ReferenceEquals( other._type, this._type )
            && this._methods.SetEquals( other._methods )
            && this._fields.SetEquals( other._fields )
            );
         if ( !retVal )
         {
            var refEquals = ReferenceEquals( other._type, this._type );
            if ( refEquals )
            {
               var methodsEqual = this._methods.SetEquals( other._methods );
               var fieldsEqual = this._fields.SetEquals( other._fields );
            }
         }
         return retVal;
      }
   }

   internal sealed class MethodLogicalInfo : IEquatable<MethodLogicalInfo>
   {
      private readonly MethodDefinition _method;
      private readonly IList<ParameterDefinition> _parameters;

      internal MethodLogicalInfo( CILMetaData md, Int32 methodDefIndex )
      {
         this._method = md.MethodDefinitions[methodDefIndex];
         this._parameters = md.GetMethodParameterIndices( methodDefIndex ).Select( idx => md.ParameterDefinitions[idx] ).ToList();
      }

      public override Boolean Equals( Object obj )
      {
         return this.Equals( obj as MethodLogicalInfo );
      }

      public override Int32 GetHashCode()
      {
         return Comparers.MethodDefinitionEqualityComparer.GetHashCode( this._method );
      }

      public Boolean Equals( MethodLogicalInfo other )
      {
         var retVal = ReferenceEquals( this, other )
            || (
            other != null
            && ReferenceEquals( this._method, other._method )
            && ListEqualityComparer<IList<ParameterDefinition>, ParameterDefinition>.Equals( this._parameters, other._parameters, ReferenceEqualityComparer<ParameterDefinition>.ReferenceBasedComparer )
            );
         if ( !retVal )
         {

         }
         return retVal;
      }
   }
}
