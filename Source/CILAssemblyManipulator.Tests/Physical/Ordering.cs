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
using CILAssemblyManipulator.Tests.Physical;

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
         var md = ReadFromFile( MSCorLibLocation, null );
         ReOrderAndValidate( md );
      }

      [Test]
      public void TestCAMOrdering()
      {
         var md = ReadFromFile( CAMLocation, null );
         ReOrderAndValidate( md );
      }

      [Test]
      public void TestDuplicateRemovingWithOneDuplicate()
      {
         var md = CILMetaDataFactory.NewMetaData();
         md.TypeDefinitions.Add( new TypeDefinition() { Namespace = "TestNS", Name = "TestType" } );
         var method = new MethodDefinition() { Name = "TestMethod", IL = new MethodILDefinition(), Signature = new MethodDefinitionSignature( 1 ) };
         md.MethodDefinitions.Add( method );
         var typeSpec = new ClassOrValueTypeSignature() { Type = new TableIndex( Tables.TypeDef, 0 ), IsClass = false };

         AddDuplicateRowToMD( md, method, typeSpec );

         ReOrderAndValidate( md );

         Assert.AreEqual( 1, md.TypeSpecifications.Count );
         Assert.AreEqual( new TableIndex( Tables.TypeSpec, 0 ), ( (ClassOrValueTypeSignature) md.MethodDefinitions[0].Signature.Parameters[0].Type ).Type );
         Assert.AreEqual( new TableIndex( Tables.TypeSpec, 0 ), ( (OpCodeInfoWithToken) md.MethodDefinitions[0].IL.OpCodes[0] ).Operand );
      }

      [Test]
      public void TestDuplicateRemovingWithTwoDuplicates()
      {
         var md = CILMetaDataFactory.NewMetaData();
         md.TypeDefinitions.Add( new TypeDefinition() { Namespace = "TestNS", Name = "TestType" } );
         var method = new MethodDefinition() { Name = "TestMethod", IL = new MethodILDefinition(), Signature = new MethodDefinitionSignature( 1 ) };
         md.MethodDefinitions.Add( method );
         var typeSpec = new ClassOrValueTypeSignature() { Type = new TableIndex( Tables.TypeDef, 0 ), IsClass = false };

         var method2 = new MethodDefinition() { Name = "TestMethod2", IL = new MethodILDefinition(), Signature = new MethodDefinitionSignature( 1 ) };
         md.MethodDefinitions.Add( method2 );
         var typeSpec2 = new ClassOrValueTypeSignature( 1 ) { Type = new TableIndex( Tables.TypeDef, 0 ), IsClass = true };

         AddDuplicateRowToMD( md, method, typeSpec );
         AddDuplicateRowToMD( md, method2, typeSpec2 );


         ReOrderAndValidate( md );

         Assert.AreEqual( 2, md.TypeSpecifications.Count );
         Assert.AreEqual( new TableIndex( Tables.TypeSpec, 0 ), ( (ClassOrValueTypeSignature) md.MethodDefinitions[0].Signature.Parameters[0].Type ).Type );
         Assert.AreEqual( new TableIndex( Tables.TypeSpec, 0 ), ( (OpCodeInfoWithToken) md.MethodDefinitions[0].IL.OpCodes[0] ).Operand );
         Assert.AreEqual( new TableIndex( Tables.TypeSpec, 1 ), ( (ClassOrValueTypeSignature) md.MethodDefinitions[1].Signature.Parameters[0].Type ).Type );
         Assert.AreEqual( new TableIndex( Tables.TypeSpec, 1 ), ( (OpCodeInfoWithToken) md.MethodDefinitions[1].IL.OpCodes[0] ).Operand );
      }

      private static void AddDuplicateRowToMD( CILMetaData md, MethodDefinition method, TypeSignature typeSpec )
      {
         var typeSpecIndex = md.TypeSpecifications.Count;
         var type = new ClassOrValueTypeSignature() { Type = new TableIndex( Tables.TypeSpec, typeSpecIndex ) };
         method.Signature.Parameters.Add( new ParameterSignature() { Type = type } );
         method.Signature.ReturnType = new ParameterSignature() { Type = SimpleTypeSignature.Void };
         method.IL.OpCodes.Add( new OpCodeInfoWithToken( OpCodes.Ldtoken, new TableIndex( Tables.TypeSpec, typeSpecIndex + 1 ) ) );

         var typeSpecRow = new TypeSpecification() { Signature = typeSpec };
         md.TypeSpecifications.Add( typeSpecRow );
         md.TypeSpecifications.Add( typeSpecRow );
      }

      private static void ReOrderAndValidate( CILMetaData md )
      {
         var logicalInfo = new ModuleLogicalInfo( md );
         // Create args BEFORE sorting
         var matchArgs = new MatchArgs( md );

         // Perform Sort
         var tableIndexTranslationInfo = md.OrderTablesAndUpdateSignatures();
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

         // TODO all other tables as well...

         //////////////////////// Integrity
         Assert.IsTrue( logicalInfo.IsMatch( matchArgs ) );
      }

      private static void AssertOrderBySingleSimpleColumn<T>( List<T> table, Func<T, Int32> pkExtractor )
      {
         for ( var i = 1; i < table.Count; ++i )
         {
            Assert.Less( pkExtractor( table[i - 1] ), pkExtractor( table[i] ) );
         }
      }
   }

   internal sealed class MatchArgs
   {
      private readonly CILMetaData _md;
      //private Int32[][] _tableIndexTranslationInfo;
      private readonly TypeDefinition[] _typeDefs;
      private readonly AbstractTypeRefInfo[] _originalTypeRefs;
      private readonly SignatureInfo<TypeSignature>[] _originalTypeSpecs;

      //private AbstractTypeRefInfo[] _newTypeRefs;
      //private SignatureInfo<TypeSignature>[] _newTypeSpecs;

      internal MatchArgs( CILMetaData md )
      {
         this._md = md;
         this._typeDefs = md.TypeDefinitions.ToArray();
         this._originalTypeRefs = md.TypeReferences
            .Select( ( tRef, idx ) => (AbstractTypeRefInfo) MethodLogicalInfo.CreateHelperObjectFromToken( md, new TableIndex( Tables.TypeRef, idx ) ) )
            .ToArray();
         this._originalTypeSpecs = md.TypeSpecifications
            .Select( tSpec => new SignatureInfo<TypeSignature>( tSpec.Signature ) )
            .ToArray();
      }

      public CILMetaData MD
      {
         get
         {
            return this._md;
         }
      }

      //public Int32 GetNewTableIndex( TableIndex oldIndex )
      //{
      //   return this._tableIndexTranslationInfo[(Int32) oldIndex.Table][oldIndex.Index];
      //}

      //public void SetTableIndexTranslationInfo( Int32[][] tableIndexTranslationInfo )
      //{
      //   this._tableIndexTranslationInfo = tableIndexTranslationInfo;
      //   //this._newTypeRefs = this._md.TypeReferences
      //   //   .Select( ( tRef, idx ) => (AbstractTypeRefInfo) MethodLogicalInfo.CreateHelperObjectFromToken( this._md, new TableIndex( Tables.TypeRef, idx ) ) )
      //   //   .ToArray();
      //   this._newTypeSpecs = this._md.TypeSpecifications
      //      .Select( tSpec => new SignatureInfo<TypeSignature>( tSpec.Signature ) )
      //      .ToArray();
      //}

      public TypeDefinition[] OriginalTypeDefs
      {
         get
         {
            return this._typeDefs;
         }
      }

      public AbstractTypeRefInfo[] OriginalTypeRefs
      {
         get
         {
            return this._originalTypeRefs;
         }
      }

      public SignatureInfo<TypeSignature>[] OriginalTypeSpecs
      {
         get
         {
            return this._originalTypeSpecs;
         }
      }

      //public AbstractTypeRefInfo[] ModifiedTypeRefs
      //{
      //   get
      //   {
      //      return this._newTypeRefs;
      //   }
      //}

      //public SignatureInfo<TypeSignature>[] ModifiedTypeSpecs
      //{
      //   get
      //   {
      //      return this._newTypeSpecs;
      //   }
      //}

   }

   internal sealed class ModuleLogicalInfo
   {
      private readonly String _name;
      private readonly ISet<TypeLogicalInfo> _types;

      internal ModuleLogicalInfo( CILMetaData md )
      {
         this._types = new HashSet<TypeLogicalInfo>( md.TypeDefinitions.Select( ( td, idx ) => new TypeLogicalInfo( md, idx ) ) );
      }

      public bool IsMatch( MatchArgs args )
      {
         var otherTypeDefs = args.MD.TypeDefinitions;
         var retVal = this._types.Count == otherTypeDefs.Count;
         var matched = new HashSet<TypeDefinition>( ReferenceEqualityComparer<TypeDefinition>.ReferenceBasedComparer );
         if ( retVal )
         {
            foreach ( var type in this._types )
            {
               var matches = otherTypeDefs.Where( ( t, idx ) => type.IsMatch( t, idx, args ) ).ToArray();
               if ( matches.Length != 1 )
               {
                  retVal = false;
                  break;
               }
               else if ( !matched.Add( matches[0] ) )
               {
                  retVal = false;
               }
            }
         }

         return retVal;
      }
   }

   internal sealed class TypeLogicalInfo
   {
      private readonly TypeDefinition _type;
      private readonly List<MethodLogicalInfo> _methods;
      private readonly List<FieldDefinition> _fields;
      private readonly Object _baseType;

      internal TypeLogicalInfo( CILMetaData md, Int32 typeDefIndex )
      {
         this._type = md.TypeDefinitions[typeDefIndex];
         this._methods = new List<MethodLogicalInfo>( md.GetTypeMethodIndices( typeDefIndex ).Select( idx => new MethodLogicalInfo( md, idx ) ) );
         this._fields = new List<FieldDefinition>( md.GetTypeFields( typeDefIndex ) );
         this._baseType = this._type.BaseType.HasValue ?
            MethodLogicalInfo.CreateHelperObjectFromToken( md, this._type.BaseType.Value ) :
            null;
      }

      public bool IsMatch( TypeDefinition other, Int32 typeDefIndex, MatchArgs args )
      {
         var retVal = ReferenceEquals( this._type, other );
         if ( retVal )
         {
            var md = args.MD;
            var curMethods = md.GetTypeMethods( typeDefIndex ).ToArray();
            var curFields = md.GetTypeFields( typeDefIndex ).ToArray();
            retVal = ( ( this._baseType == null && !other.BaseType.HasValue ) || ( this._baseType != null && other.BaseType.HasValue && MethodLogicalInfo.MatchHelperObject( this._baseType, md, other.BaseType.Value, args ) ) )
               && this._methods.Where( ( m, idx ) => m.IsMatch( curMethods[idx], other.MethodList.Index + idx, args ) ).Count() == this._methods.Count
               && this._fields.Where( ( f, idx ) => ReferenceEquals( f, curFields[idx] ) ).Count() == this._fields.Count;
            if ( !retVal )
            {
            }
         }
         return retVal;
      }
   }

   internal abstract class AbstractTypeRefInfo
   {
      public String Name { get; set; }
      public String Namespace { get; set; }

      public bool IsMatch( TypeReference obj, MatchArgs args )
      {
         return
            String.Equals( this.Name, obj.Name )
            && String.Equals( this.Namespace, obj.Namespace )
            && this.DoesEqual( obj, args );
      }

      protected abstract Boolean DoesEqual( TypeReference other, MatchArgs args );
   }

   internal sealed class NestedTypeRefInfo : AbstractTypeRefInfo
   {
      public AbstractTypeRefInfo EnclosingInfo { get; set; }

      protected override bool DoesEqual( TypeReference other, MatchArgs args )
      {
         return other.ResolutionScope.HasValue
            && other.ResolutionScope.Value.Table == Tables.TypeRef
            && this.EnclosingInfo.IsMatch( args.MD.TypeReferences[other.ResolutionScope.Value.Index], args );
      }
   }

   internal sealed class TopLevelTypeRefInfo : AbstractTypeRefInfo
   {
      public TableIndex? ResolutionScopeIndex { get; set; }
      public Object ResolutionScope { get; set; }

      protected override bool DoesEqual( TypeReference other, MatchArgs args )
      {
         var resScopeNullable = this.ResolutionScopeIndex;
         var otherRSNullable = other.ResolutionScope;
         var retVal = resScopeNullable.HasValue == otherRSNullable.HasValue;
         if ( retVal && resScopeNullable.HasValue )
         {
            var resScope = resScopeNullable.Value;
            var otherResCope = other.ResolutionScope.Value;
            retVal = resScopeNullable.Value.Table == otherRSNullable.Value.Table;
            if ( retVal )
            {
               var newIndex = otherResCope.Index;
               var md = args.MD;
               switch ( resScope.Table )
               {
                  case Tables.ModuleRef:
                     retVal = Comparers.ModuleReferenceEqualityComparer.Equals( (ModuleReference) this.ResolutionScope, md.ModuleReferences[newIndex] );
                     break;
                  case Tables.Module:
                     retVal = Comparers.ModuleDefinitionEqualityComparer.Equals( (ModuleDefinition) this.ResolutionScope, md.ModuleDefinitions[newIndex] );
                     break;
                  case Tables.AssemblyRef:
                     retVal = Comparers.AssemblyReferenceEqualityComparer.Equals( (AssemblyReference) this.ResolutionScope, md.AssemblyReferences[newIndex] );
                     break;
                  default:
                     throw new NotSupportedException( "Resolution scope (" + resScope + ")" );
               }
            }
         }
         return retVal;
      }
   }

   internal sealed class SignatureInfo<TSignature>
      where TSignature : AbstractSignature
   {
      private readonly TSignature _signature;

      internal SignatureInfo( TSignature signature )
      {
         this._signature = signature.CreateDeepCopy();
      }

      public TSignature Signature
      {
         get
         {
            return this._signature;
         }
      }

      // We can't use normal signature comparers here since they would return true only for exactly same table indices
      public Boolean IsMatch( TSignature other, MatchArgs args )
      {
         return IsMatch( this.Signature, other, args );
      }

      private static Boolean IsMatch( AbstractSignature original, AbstractSignature modified, MatchArgs args )
      {
         var retVal = original.SignatureKind == modified.SignatureKind;
         if ( retVal )
         {
            switch ( original.SignatureKind )
            {
               case SignatureKind.Field:
                  var originalF = (FieldSignature) original;
                  var modifiedF = (FieldSignature) modified;
                  retVal = CustomModsMatch( originalF.CustomModifiers, modifiedF.CustomModifiers, args )
                     && IsMatch( originalF.Type, modifiedF.Type, args );
                  break;
               case SignatureKind.GenericMethodInstantiation:
                  retVal = ListEqualityComparer<List<TypeSignature>, TypeSignature>.Equals(
                     ( (GenericMethodSignature) original ).GenericArguments,
                     ( (GenericMethodSignature) modified ).GenericArguments,
                     ComparerFromFunctions.NewEqualityComparer<TypeSignature>( ( og, mg ) => IsMatch( og, mg, args ), g => { throw new NotSupportedException(); } )
                     );
                  break;
               case SignatureKind.LocalVariables:
                  retVal = ListEqualityComparer<List<LocalVariableSignature>, LocalVariableSignature>.Equals(
                     ( (LocalVariablesSignature) original ).Locals,
                     ( (LocalVariablesSignature) modified ).Locals,
                     ComparerFromFunctions.NewEqualityComparer<LocalVariableSignature>( ( ol, ml ) =>
                        ol.IsPinned == ml.IsPinned
                        && ParametersMatch( ol, ml, args )
                     , l => { throw new NotSupportedException(); } )
                     );
                  break;
               case SignatureKind.MethodDefinition:
                  retVal = MethodCommonPartsMatch( (MethodDefinitionSignature) original, (MethodDefinitionSignature) modified, args );
                  break;
               case SignatureKind.MethodReference:
                  retVal = MethodRefMatch( (MethodReferenceSignature) original, (MethodReferenceSignature) modified, args );
                  break;
               case SignatureKind.Type:
                  retVal = TypesMatch( (TypeSignature) original, (TypeSignature) modified, args );
                  break;
               default:
                  throw new NotSupportedException( "Unknown signature kind: " + original.SignatureKind + "." );
            }
         }

         return retVal;
      }

      private static Boolean MethodCommonPartsMatch( AbstractMethodSignature original, AbstractMethodSignature modified, MatchArgs args )
      {
         return original.GenericArgumentCount == modified.GenericArgumentCount
            && original.SignatureStarter == modified.SignatureStarter
            && ParametersMatch( original.ReturnType, modified.ReturnType, args )
            && ListEqualityComparer<List<ParameterSignature>, ParameterSignature>.Equals( original.Parameters, modified.Parameters, ComparerFromFunctions.NewEqualityComparer<ParameterSignature>( ( op, mp ) => ParametersMatch( op, mp, args ), p => { throw new NotSupportedException(); } ) );
      }

      private static Boolean MethodRefMatch( MethodReferenceSignature original, MethodReferenceSignature modified, MatchArgs args )
      {
         return MethodCommonPartsMatch( original, modified, args )
            && ListEqualityComparer<List<ParameterSignature>, ParameterSignature>.Equals( original.VarArgsParameters, modified.VarArgsParameters, ComparerFromFunctions.NewEqualityComparer<ParameterSignature>( ( op, mp ) => ParametersMatch( op, mp, args ), p => { throw new NotSupportedException(); } ) );
      }

      private static Boolean ParametersMatch( ParameterOrLocalVariableSignature original, ParameterOrLocalVariableSignature modified, MatchArgs args )
      {
         return original.IsByRef == modified.IsByRef
            && CustomModsMatch( original.CustomModifiers, modified.CustomModifiers, args )
            && IsMatch( original.Type, modified.Type, args );
      }

      private static Boolean CustomModsMatch( List<CustomModifierSignature> original, List<CustomModifierSignature> modified, MatchArgs args )
      {
         return ListEqualityComparer<List<CustomModifierSignature>, CustomModifierSignature>.Equals(
            original,
            modified,
            ComparerFromFunctions.NewEqualityComparer<CustomModifierSignature>( ( cOriginal, cModified ) => SignatureTypeDefOrRefOrSpecEquals( cOriginal.CustomModifierType, cModified.CustomModifierType, args ) && cOriginal.IsOptional == cModified.IsOptional, c => { throw new NotSupportedException(); } )
            );
      }

      private static Boolean TypesMatch( TypeSignature original, TypeSignature modified, MatchArgs args )
      {
         var retVal = original.TypeSignatureKind == modified.TypeSignatureKind;
         if ( retVal )
         {
            switch ( original.TypeSignatureKind )
            {
               case TypeSignatureKind.ClassOrValue:
                  var originalC = (ClassOrValueTypeSignature) original;
                  var modifiedC = (ClassOrValueTypeSignature) modified;
                  retVal = originalC.IsClass == modifiedC.IsClass
                     && SignatureTypeDefOrRefOrSpecEquals( originalC.Type, modifiedC.Type, args )
                     && ListEqualityComparer<List<TypeSignature>, TypeSignature>.Equals( originalC.GenericArguments, modifiedC.GenericArguments, ComparerFromFunctions.NewEqualityComparer<TypeSignature>( ( o, m ) => TypesMatch( o, m, args ), t => { throw new NotSupportedException(); } ) );
                  break;
               case TypeSignatureKind.ComplexArray:
                  var originalAC = (ComplexArrayTypeSignature) original;
                  var modifiedAC = (ComplexArrayTypeSignature) modified;
                  retVal = originalAC.Rank == modifiedAC.Rank
                     && ListEqualityComparer<List<Int32>, Int32>.DefaultListEqualityComparer.Equals( originalAC.LowerBounds, modifiedAC.LowerBounds )
                     && ListEqualityComparer<List<Int32>, Int32>.DefaultListEqualityComparer.Equals( originalAC.Sizes, modifiedAC.Sizes )
                     && TypesMatch( originalAC.ArrayType, modifiedAC.ArrayType, args );
                  break;
               case TypeSignatureKind.FunctionPointer:
                  retVal = MethodRefMatch( ( (FunctionPointerTypeSignature) original ).MethodSignature, ( (FunctionPointerTypeSignature) modified ).MethodSignature, args );
                  break;
               case TypeSignatureKind.GenericParameter:
                  var originalG = (GenericParameterTypeSignature) original;
                  var modifiedG = (GenericParameterTypeSignature) modified;
                  retVal = originalG.GenericParameterIndex == modifiedG.GenericParameterIndex
                     && originalG.IsTypeParameter == modifiedG.IsTypeParameter;
                  break;
               case TypeSignatureKind.Pointer:
                  var originalP = (PointerTypeSignature) original;
                  var modifiedP = (PointerTypeSignature) modified;
                  retVal = CustomModsMatch( originalP.CustomModifiers, modifiedP.CustomModifiers, args )
                     && TypesMatch( originalP.PointerType, modifiedP.PointerType, args );
                  break;
               case TypeSignatureKind.Simple:
                  retVal = ( (SimpleTypeSignature) original ).SimpleType == ( (SimpleTypeSignature) modified ).SimpleType;
                  break;
               case TypeSignatureKind.SimpleArray:
                  var originalA = (SimpleArrayTypeSignature) original;
                  var modifiedA = (SimpleArrayTypeSignature) modified;
                  retVal = CustomModsMatch( originalA.CustomModifiers, modifiedA.CustomModifiers, args )
                     && TypesMatch( originalA.ArrayType, modifiedA.ArrayType, args );
                  break;
               default:
                  throw new NotSupportedException( "Unknown type signature kind: " + original.TypeSignatureKind + "." );
            }
         }

         return retVal;
      }

      private static Boolean SignatureTypeDefOrRefOrSpecEquals( TableIndex original, TableIndex modified, MatchArgs args )
      {
         var retVal = original.Table == modified.Table;
         if ( retVal )
         {
            switch ( original.Table )
            {
               case Tables.TypeDef:
                  retVal = ReferenceEquals( args.OriginalTypeDefs[original.Index], args.MD.TypeDefinitions[modified.Index] );
                  break;
               case Tables.TypeRef:
                  retVal = args.OriginalTypeRefs[original.Index].IsMatch( args.MD.TypeReferences[modified.Index], args );
                  break;
               case Tables.TypeSpec:
                  retVal = args.OriginalTypeSpecs[original.Index].IsMatch( args.MD.TypeSpecifications[modified.Index].Signature, args );
                  break;
               default:
                  throw new InvalidOperationException( "Signature had " + original + " as table index." );
            }
         }

         return retVal;
      }
   }

   internal sealed class MethodLogicalInfo
   {

      private sealed class MemberRefInfo
      {
         public MemberReference MemberRef { get; set; }
         public TableIndex DeclaringTypeIndex { get; set; }
         public Object DeclaringType { get; set; }
         public SignatureInfo<AbstractSignature> Signature { get; set; }

         public bool IsMatch( MemberReference obj, MatchArgs args )
         {
            var md = args.MD;
            var declType = this.DeclaringTypeIndex;
            var newDeclType = obj.DeclaringType;
            var retVal =
               obj != null
               && String.Equals( this.MemberRef.Name, obj.Name )
               && this.Signature.IsMatch( obj.Signature, args )
               && declType.Table == newDeclType.Table;
            if ( retVal )
            {
               switch ( declType.Table )
               {
                  case Tables.TypeDef:
                     retVal = ReferenceEquals( this.DeclaringType, md.TypeDefinitions[newDeclType.Index] );
                     break;
                  case Tables.MethodDef:
                     retVal = ReferenceEquals( this.DeclaringType, md.MethodDefinitions[newDeclType.Index] );
                     break;
                  case Tables.TypeRef:
                     retVal = ( (AbstractTypeRefInfo) this.DeclaringType ).IsMatch( md.TypeReferences[newDeclType.Index], args );
                     break;
                  case Tables.TypeSpec:
                     retVal = ( (SignatureInfo<TypeSignature>) this.DeclaringType ).IsMatch( md.TypeSpecifications[newDeclType.Index].Signature, args );
                     break;
                  case Tables.ModuleRef:
                     retVal = Comparers.ModuleReferenceEqualityComparer.Equals( (ModuleReference) this.DeclaringType, md.ModuleReferences[newDeclType.Index] );
                     break;
                  default:
                     throw new NotSupportedException( "Declaring type (" + declType + ")" );
               }
            }

            return retVal;
         }
      }

      private sealed class MethodSpecInfo
      {
         public MethodSpecification MethodSpec { get; set; }
         public SignatureInfo<GenericMethodSignature> Signature { get; set; }
         public TableIndex MethodIndex { get; set; }
         public Object Method { get; set; }

         public bool IsMatch( MethodSpecification obj, MatchArgs args )
         {
            var method = this.MethodIndex;
            var newMethod = obj.Method;

            var retVal = method.Table == newMethod.Table;

            if ( retVal )
            {
               switch ( method.Table )
               {
                  case Tables.MethodDef:
                     retVal = ReferenceEquals( this.Method, args.MD.MethodDefinitions[newMethod.Index] );
                     break;
                  case Tables.MemberRef:
                     retVal = ( (MemberRefInfo) this.Method ).IsMatch( args.MD.MemberReferences[newMethod.Index], args );
                     break;
                  default:
                     throw new NotSupportedException( "Method (" + method + ")" );
               }
            }

            return retVal && this.Signature.IsMatch( obj.Signature, args );
         }
      }



      private readonly MethodDefinition _method;
      private readonly IList<ParameterDefinition> _parameters;
      private readonly SignatureInfo<LocalVariablesSignature> _locals;
      private readonly IDictionary<Int32, Object> _ilTableInfos;
      private readonly IDictionary<Int32, Object> _exceptionClasses;

      internal MethodLogicalInfo( CILMetaData md, Int32 methodDefIndex )
      {
         this._method = md.MethodDefinitions[methodDefIndex];
         this._parameters = md.GetMethodParameterIndices( methodDefIndex ).Select( idx => md.ParameterDefinitions[idx] ).ToList();
         var locals = md.GetLocalsSignatureForMethodOrNull( methodDefIndex );
         if ( locals != null )
         {
            this._locals = new SignatureInfo<LocalVariablesSignature>( locals );
         }
         var il = this._method.IL;
         if ( il != null )
         {
            this._ilTableInfos = il.OpCodes
               .Select( ( c, i ) => Tuple.Create( c, i ) )
               .Where( t => t.Item1.InfoKind == OpCodeOperandKind.OperandToken )
               .Select( t => Tuple.Create( t.Item2, CreateHelperObjectFromToken( md, ( (OpCodeInfoWithToken) t.Item1 ).Operand ) ) )
               .ToDictionary( t => t.Item1, t => t.Item2 );
            this._exceptionClasses = il.ExceptionBlocks
               .Select( ( b, i ) => Tuple.Create( b, i ) )
               .Where( t => t.Item1.ExceptionType.HasValue )
               .Select( t => Tuple.Create( t.Item2, CreateHelperObjectFromToken( md, t.Item1.ExceptionType.Value ) ) )
               .ToDictionary( t => t.Item1, t => t.Item2 );
         }

      }

      public bool IsMatch( MethodDefinition other, Int32 methodDefIndex, MatchArgs args )
      {
         var retVal = ReferenceEquals( this._method, other );

         if ( retVal )
         {
            var md = args.MD;
            var curParams = md.GetMethodParameters( methodDefIndex ).ToArray();
            var curLocals = md.GetLocalsSignatureForMethodOrNull( methodDefIndex );

            retVal = ListEqualityComparer<IList<ParameterDefinition>, ParameterDefinition>.Equals( this._parameters, curParams, ReferenceEqualityComparer<ParameterDefinition>.ReferenceBasedComparer )
            && ( ( this._locals == null && curLocals == null ) || ( this._locals != null && curLocals != null && this._locals.IsMatch( curLocals, args ) ) )
            && (
               ( this._ilTableInfos == null && this._exceptionClasses == null && other.IL == null )
               ||
                ( this._ilTableInfos != null && this._exceptionClasses != null && other.IL != null
                && this._ilTableInfos.All( kvp => MatchHelperObject( kvp.Value, md, ( (OpCodeInfoWithToken) other.IL.OpCodes[kvp.Key] ).Operand, args ) )
                && this._exceptionClasses.All( kvp => MatchHelperObject( kvp.Value, md, other.IL.ExceptionBlocks[kvp.Key].ExceptionType.Value, args ) )
                )
               );

            if ( !retVal )
            {

            }
         }

         //);
         return retVal;
      }

      internal static Object CreateHelperObjectFromToken( CILMetaData md, TableIndex token )
      {
         switch ( token.Table )
         {
            case Tables.TypeDef:
            case Tables.MethodDef:
            case Tables.Field:
               return md.GetByTableIndex( token );
            case Tables.TypeRef:
               var tRef = md.TypeReferences[token.Index];
               return tRef.ResolutionScope.HasValue && tRef.ResolutionScope.Value.Table == Tables.TypeRef ?
                  (AbstractTypeRefInfo) new NestedTypeRefInfo()
                  {
                     Name = tRef.Name,
                     Namespace = tRef.Namespace,
                     EnclosingInfo = (AbstractTypeRefInfo) CreateHelperObjectFromToken( md, tRef.ResolutionScope.Value )
                  } :
                  new TopLevelTypeRefInfo()
                  {
                     Name = tRef.Name,
                     Namespace = tRef.Namespace,
                     ResolutionScopeIndex = tRef.ResolutionScope,
                     ResolutionScope = tRef.ResolutionScope.HasValue ? md.GetByTableIndex( tRef.ResolutionScope.Value ) : null
                  };
            case Tables.MemberRef:
               var mRef = md.MemberReferences[token.Index];
               return new MemberRefInfo()
               {
                  MemberRef = mRef,
                  DeclaringTypeIndex = mRef.DeclaringType,
                  Signature = new SignatureInfo<AbstractSignature>( mRef.Signature ),
                  DeclaringType = mRef.DeclaringType.Table == Tables.ModuleRef ? md.GetByTableIndex( mRef.DeclaringType ) : CreateHelperObjectFromToken( md, mRef.DeclaringType )
               };
            case Tables.MethodSpec:
               var mSpec = md.MethodSpecifications[token.Index];
               return new MethodSpecInfo()
               {
                  MethodSpec = mSpec,
                  MethodIndex = mSpec.Method,
                  Signature = new SignatureInfo<GenericMethodSignature>( mSpec.Signature ),
                  Method = CreateHelperObjectFromToken( md, mSpec.Method )
               };
            case Tables.TypeSpec:
               return new SignatureInfo<TypeSignature>( md.TypeSpecifications[token.Index].Signature );
            case Tables.StandaloneSignature:
               return new SignatureInfo<MethodReferenceSignature>( (MethodReferenceSignature) md.StandaloneSignatures[token.Index].Signature );
            default:
               throw new InvalidOperationException( "Unrecognized token: " + token + "." );
         }
      }

      internal static Boolean MatchHelperObject( Object originalObject, CILMetaData md, TableIndex newIndex, MatchArgs args )
      {
         switch ( newIndex.Table )
         {
            case Tables.TypeDef:
            case Tables.MethodDef:
            case Tables.Field:
               return ReferenceEquals( originalObject, md.GetByTableIndex( newIndex ) );
            case Tables.TypeRef:
               return ( (AbstractTypeRefInfo) originalObject ).IsMatch( md.TypeReferences[newIndex.Index], args );
            case Tables.MemberRef:
               return ( (MemberRefInfo) originalObject ).IsMatch( md.MemberReferences[newIndex.Index], args );
            case Tables.MethodSpec:
               return ( (MethodSpecInfo) originalObject ).IsMatch( md.MethodSpecifications[newIndex.Index], args );
            case Tables.TypeSpec:
               return ( (SignatureInfo<TypeSignature>) originalObject ).IsMatch( md.TypeSpecifications[newIndex.Index].Signature, args );
            case Tables.StandaloneSignature:
               return ( (SignatureInfo<MethodReferenceSignature>) originalObject ).IsMatch( (MethodReferenceSignature) md.StandaloneSignatures[newIndex.Index].Signature, args );
            default:
               throw new InvalidOperationException( "Unrecognized token: " + newIndex + "." );
         }
      }
   }
}