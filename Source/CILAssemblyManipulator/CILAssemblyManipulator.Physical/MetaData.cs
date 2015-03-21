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
using CommonUtils;

namespace CILAssemblyManipulator.Physical
{
   public sealed class ModuleDefinition
   {
      public Int16 Generation { get; set; }
      public String Name { get; set; }
      public Guid? ModuleGUID { get; set; }
      public Guid? EditAndContinueGUID { get; set; }
      public Guid? EditAndContinueBaseGUID { get; set; }
   }

   public sealed class TypeReference
   {
      public TableIndex? ResolutionScope { get; set; }
      public String Name { get; set; }
      public String Namespace { get; set; }
   }

   public sealed class TypeDefinition
   {
      public TypeAttributes Attributes { get; set; }
      public String Name { get; set; }
      public String Namespace { get; set; }
      public TableIndex? BaseType { get; set; }
      public TableIndex FieldList { get; set; }
      public TableIndex MethodList { get; set; }
   }

   public sealed class FieldDefinition
   {
      public FieldAttributes Attributes { get; set; }
      public String Name { get; set; }
      public FieldSignature Signature { get; set; }
   }

   public sealed class MethodDefinition
   {
      public MethodILDefinition IL { get; set; }
      public MethodImplAttributes ImplementationAttributes { get; set; }
      public MethodAttributes Attributes { get; set; }
      public String Name { get; set; }
      public MethodDefinitionSignature Signature { get; set; }
      public TableIndex ParameterList { get; set; }
   }

   public sealed class ParameterDefinition
   {
      public ParameterAttributes Attributes { get; set; }
      public Int32 Sequence { get; set; }
      public String Name { get; set; }
   }

   public sealed class InterfaceImplementation
   {
      public TableIndex Class { get; set; }
      public TableIndex Interface { get; set; }
   }

   public sealed class MemberReference
   {
      public TableIndex DeclaringType { get; set; }
      public String Name { get; set; }
      public AbstractSignature Signature { get; set; }
   }

   public sealed class ConstantDefinition
   {
      public SignatureElementTypes Type { get; set; }
      public TableIndex Parent { get; set; }
      public Object Value { get; set; }
   }

   public sealed class CustomAttributeDefinition
   {
      public TableIndex Parent { get; set; }
      public TableIndex Type { get; set; }
      public AbstractCustomAttributeSignature Signature { get; set; }
   }

   public sealed class FieldMarshal
   {
      public TableIndex Parent { get; set; }
      public MarshalingInfo NativeType { get; set; }
   }

   public sealed class SecurityDefinition
   {
      private readonly List<AbstractSecurityInformation> _permissionSets;

      public SecurityDefinition( Int32 permissionSetsCount = 0 )
      {
         this._permissionSets = new List<AbstractSecurityInformation>( permissionSetsCount );
      }

      /// <summary>
      /// Gets or sets the <see cref="SecurityAction"/> associated with this security attribute declaration.
      /// </summary>
      /// <value>The <see cref="SecurityAction"/> associated with this security attribute declaration.</value>
      public SecurityAction Action { get; set; }

      public TableIndex Parent { get; set; }

      public List<AbstractSecurityInformation> PermissionSets
      {
         get
         {
            return this._permissionSets;
         }
      }
   }

   public sealed class ClassLayout
   {
      public Int16 PackingSize { get; set; }
      public Int32 ClassSize { get; set; }
      public TableIndex Parent { get; set; }
   }

   public sealed class FieldLayout
   {
      public Int32 Offset { get; set; }
      public TableIndex Field { get; set; }
   }

   public sealed class StandaloneSignature
   {
      public AbstractSignature Signature { get; set; }
   }

   public sealed class EventMap
   {
      public TableIndex Parent { get; set; }
      public TableIndex EventList { get; set; }
   }

   public sealed class EventDefinition
   {
      public EventAttributes Attributes { get; set; }
      public String Name { get; set; }
      public TableIndex EventType { get; set; }
   }

   public sealed class PropertyMap
   {
      public TableIndex Parent { get; set; }
      public TableIndex PropertyList { get; set; }
   }

   public sealed class PropertyDefinition
   {
      public PropertyAttributes Attributes { get; set; }
      public String Name { get; set; }
      public PropertySignature Signature { get; set; }
   }

   public sealed class MethodSemantics
   {
      public MethodSemanticsAttributes Attributes { get; set; }
      public TableIndex Method { get; set; }
      public TableIndex Associaton { get; set; }
   }

   public sealed class MethodImplementation
   {
      public TableIndex Class { get; set; }
      public TableIndex MethodBody { get; set; }
      public TableIndex MethodDeclaration { get; set; }
   }

   public sealed class ModuleReference
   {
      public String ModuleName { get; set; }
   }

   public sealed class TypeSpecification
   {
      public TypeSignature Signature { get; set; }
   }

   public sealed class MethodImplementationMap
   {
      public PInvokeAttributes Attributes { get; set; }
      public TableIndex MemberForwarded { get; set; }
      public String ImportName { get; set; }
      public TableIndex ImportScope { get; set; }
   }

   public sealed class FieldRVA
   {
      public Byte[] Data { get; set; }
      public TableIndex Field { get; set; }
   }

   public sealed class AssemblyDefinition
   {
      private readonly AssemblyInformation _assemblyInfo;

      public AssemblyDefinition()
      {
         this._assemblyInfo = new AssemblyInformation();
      }

      public AssemblyFlags Attributes { get; set; }
      public AssemblyInformation AssemblyInformation
      {
         get
         {
            return this._assemblyInfo;
         }
      }
      public AssemblyHashAlgorithm HashAlgorithm { get; set; }
   }

   public sealed class AssemblyReference
   {
      private readonly AssemblyInformation _assemblyInfo;

      public AssemblyReference()
      {
         this._assemblyInfo = new AssemblyInformation();
      }

      public AssemblyFlags Attributes { get; set; }
      public AssemblyInformation AssemblyInformation
      {
         get
         {
            return this._assemblyInfo;
         }
      }
      public Byte[] HashValue { get; set; }
   }

   public sealed class FileReference
   {
      public FileAttributes Attributes { get; set; }
      public String Name { get; set; }
      public Byte[] HashValue { get; set; }
   }

   public sealed class ExportedType
   {
      public TypeAttributes Attributes { get; set; }
      public Int32 TypeDefinitionIndex { get; set; }
      public String Name { get; set; }
      public String Namespace { get; set; }
      public TableIndex Implementation { get; set; }
   }

   public sealed class ManifestResource
   {
      public Int64 Offset { get; set; }
      public ManifestResourceAttributes Attributes { get; set; }
      public String Name { get; set; }
      public TableIndex? Implementation { get; set; }

      // This will be used only if Implementation is null
      public Byte[] DataInCurrentFile { get; set; }
   }

   public sealed class NestedClassDefinition
   {
      public TableIndex NestedClass { get; set; }
      public TableIndex EnclosingClass { get; set; }
   }

   public sealed class GenericParameterDefinition
   {
      public Int16 GenericParameterIndex { get; set; }
      public GenericParameterAttributes Attributes { get; set; }
      public TableIndex Owner { get; set; }
      public String Name { get; set; }
   }

   public sealed class MethodSpecification
   {
      public TableIndex Method { get; set; }
      public GenericMethodSignature Signature { get; set; }
   }

   public sealed class GenericParameterConstraintDefinition
   {
      public TableIndex Owner { get; set; }
      public TableIndex Constraint { get; set; }
   }

   public struct TableIndex : IEquatable<TableIndex>, IComparable<TableIndex>, IComparable
   {
      private readonly Int32 _token;

      // index is zero-based
      public TableIndex( Tables aTable, Int32 anIdx )
      {
         this._token = ( (Int32) aTable << 24 ) | anIdx;
      }

      internal TableIndex( Int32 token )
      {
         // Index is zero-based in CAM
         this._token = ( ( token & TokenUtils.INDEX_MASK ) - 1 ) | ( token & ~TokenUtils.INDEX_MASK );
      }

      public Tables Table
      {
         get
         {
            return (Tables) ( this._token >> 24 );
         }
      }

      /// <summary>
      /// This index is zero-based.
      /// </summary>
      public Int32 Index
      {
         get
         {
            return this._token & TokenUtils.INDEX_MASK;
         }
      }

      internal Int32 ZeroBasedToken
      {
         get
         {
            return this._token;
         }
      }

      public override Boolean Equals( Object obj )
      {
         return obj is TableIndex && this.Equals( (TableIndex) obj );
      }

      public override Int32 GetHashCode()
      {
         return this._token;
      }

      public Boolean Equals( TableIndex other )
      {
         return this._token == other._token;
      }

      public override String ToString()
      {
         return this.Table + "[" + this.Index + "]";
      }

      public Int32 CompareTo( TableIndex other )
      {
         var retVal = this.Table.CompareTo( other.Table );
         if ( retVal == 0 )
         {
            retVal = this.Index.CompareTo( other.Index );
         }

         return retVal;
      }

      internal Int32 CompareTo( TableIndex other, Int32[] tableOrderArray )
      {
         var retVal = this.Index.CompareTo( other.Index );
         if ( retVal == 0 )
         {
            retVal = tableOrderArray[(Int32) this.Table].CompareTo( tableOrderArray[(Int32) other.Table] );
         }
         return retVal;
      }

      int IComparable.CompareTo( Object obj )
      {
         if ( obj == null )
         {
            // This is always 'greater' than null
            return 1;
         }
         else if ( obj is TableIndex )
         {
            return this.CompareTo( (TableIndex) obj );
         }
         else
         {
            throw new ArgumentException( "Given object must be of type " + this.GetType() + " or null." );
         }
      }

      public static Boolean operator ==( TableIndex x, TableIndex y )
      {
         return x.Equals( y );
      }

      public static Boolean operator !=( TableIndex x, TableIndex y )
      {
         return !( x == y );
      }

      public static Boolean operator <( TableIndex x, TableIndex y )
      {
         return x.CompareTo( y ) < 0;
      }

      public static Boolean operator >( TableIndex x, TableIndex y )
      {
         return x.CompareTo( y ) > 0;
      }

      public static Boolean operator <=( TableIndex x, TableIndex y )
      {
         return !( x > y );
      }

      public static Boolean operator >=( TableIndex x, TableIndex y )
      {
         return !( x < y );
      }
   }

   public enum Tables : byte
   {
      Assembly = 0x20,
      AssemblyOS = 0x22,
      AssemblyProcessor = 0x21,
      AssemblyRef = 0x23,
      AssemblyRefOS = 0x25,
      AssemblyRefProcessor = 0x24,
      ClassLayout = 0x0F,
      Constant = 0x0B,
      CustomAttribute = 0x0C,
      DeclSecurity = 0x0E,
      EncLog = 0x1E,
      EncMap = 0x1F,
      EventMap = 0x12,
      Event = 0x14,
      EventPtr = 0x13,
      ExportedType = 0x27,
      Field = 0x04,
      FieldLayout = 0x10,
      FieldMarshal = 0x0D,
      FieldPtr = 0x03,
      FieldRVA = 0x1D,
      File = 0x26,
      GenericParameter = 0x2A,
      GenericParameterConstraint = 0x2C,
      ImplMap = 0x1C,
      InterfaceImpl = 0x09,
      ManifestResource = 0x28,
      MemberRef = 0x0A,
      MethodDef = 0x06,
      MethodImpl = 0x19,
      MethodPtr = 0x05,
      MethodSemantics = 0x18,
      MethodSpec = 0x2B,
      Module = 0x00,
      ModuleRef = 0x1A,
      NestedClass = 0x29,
      Parameter = 0x08,
      ParameterPtr = 0x07,
      Property = 0x17,
      PropertyPtr = 0x16,
      PropertyMap = 0x15,
      StandaloneSignature = 0x11,
      TypeDef = 0x02,
      TypeRef = 0x01,
      TypeSpec = 0x1B
   }


}

public static partial class E_CILPhysical
{
   private sealed class StackCalculationState
   {
      private Int32 _maxStack;
      private readonly Int32[] _stackSizes;
      private readonly CILMetaData _md;

      internal StackCalculationState( CILMetaData md, Int32 ilByteCount )
      {
         this._md = md;
         this._stackSizes = new Int32[ilByteCount];
         this._maxStack = 0;
      }

      public Int32 CurrentStack { get; set; }
      public Int32 CurrentCodeByteOffset { get; set; }
      public Int32 NextCodeByteOffset { get; set; }
      public Int32 MaxStack
      {
         get
         {
            return this._maxStack;
         }
      }
      public CILMetaData MD
      {
         get
         {
            return this._md;
         }
      }
      public Int32[] StackSizes
      {
         get
         {
            return this._stackSizes;
         }
      }

      public void UpdateMaxStack( Int32 newMaxStack )
      {
         if ( this._maxStack < newMaxStack )
         {
            this._maxStack = newMaxStack;
         }
      }
   }

   private sealed class SignatureReorderState
   {
      private readonly CILMetaData _md;
      private readonly Int32[][] _tableIndices;
      private readonly IDictionary<Object, Object> _visitedInfo;
      private Boolean _updatedAny;

      internal SignatureReorderState( CILMetaData md, Int32[][] tableIndices )
      {
         this._md = md;
         this._tableIndices = tableIndices;
         this._visitedInfo = new Dictionary<Object, Object>();
         this._updatedAny = false;
      }

      public CILMetaData MD
      {
         get
         {
            return this._md;
         }
      }

      public Int32[][] TableIndices
      {
         get
         {
            return this._tableIndices;
         }
      }

      public IDictionary<Object, Object> VisitedInfo
      {
         get
         {
            return this._visitedInfo;
         }
      }

      public Boolean ChangedAny
      {
         get
         {
            return this._updatedAny;
         }
      }

      public void ResetChangedAny()
      {
         this._updatedAny = false;
      }

      public void ProcessedTableIndex( Boolean changed )
      {
         if ( changed && !this._updatedAny )
         {
            this._updatedAny = true;
         }
      }
   }

   public static Boolean IsHasThis( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.HasThis ) != 0;
   }

   public static Boolean IsExplicitThis( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.ExplicitThis ) != 0;
   }

   public static Boolean IsDefault( this SignatureStarters starter )
   {
      return starter == 0;
   }

   public static Boolean IsVarArg( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.VarArgs ) != 0;
   }

   public static Boolean IsGeneric( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.Generic ) != 0;
   }

   public static Boolean IsProperty( this SignatureStarters starter )
   {
      return ( starter & SignatureStarters.Property ) != 0;
   }

   public static LocalVariablesSignature GetLocalsSignatureForMethodOrNull( this CILMetaData md, Int32 methodDefIndex )
   {
      var method = md.MethodDefinitions.GetOrNull( methodDefIndex );
      LocalVariablesSignature retVal;
      if ( method == null )
      {
         retVal = null;
      }
      else
      {
         var il = method.IL;
         var tIdx = il.LocalsSignatureIndex;
         if ( tIdx.HasValue )
         {
            var idx = tIdx.Value.Index;
            var list = md.StandaloneSignatures;
            retVal = idx >= 0 && idx < list.Count ?
               list[idx].Signature as LocalVariablesSignature :
               null;
         }
         else
         {
            retVal = null;
         }
      }

      return retVal;
   }

   /// <summary>
   /// Checks whether the method is eligible to have method body. See ECMA specification (condition 33 for MethodDef table) for exact condition of methods having method bodies. In addition to that, the <see cref="E_CIL.IsIL"/> must return <c>true</c>.
   /// </summary>
   /// <param name="method">The method to check.</param>
   /// <returns><c>true</c> if the <paramref name="method"/> is non-<c>null</c> and can have IL method body; <c>false</c> otherwise.</returns>
   /// <seealso cref="E_CIL.IsIL"/>
   /// <seealso cref="E_CIL.CanEmitIL"/>
   public static Boolean ShouldHaveMethodBody( this MethodDefinition method )
   {
      return method != null && method.Attributes.CanEmitIL() && method.ImplementationAttributes.IsIL();
   }

   public static TableIndex ChangeIndex( this TableIndex index, Int32 newIndex )
   {
      return new TableIndex( index.Table, newIndex );
   }

   public static Object GetByTableIndex( this CILMetaData md, TableIndex index )
   {
      switch ( index.Table )
      {
         case Tables.Module:
            if ( index.Index == 0 )
            {
               return md;
            }
            else
            {
               throw new ArgumentOutOfRangeException( "Module table index should always be zero." );
            }
         case Tables.TypeRef:
            return md.TypeReferences[index.Index];
         case Tables.TypeDef:
            return md.TypeDefinitions[index.Index];
         case Tables.Field:
            return md.FieldDefinitions[index.Index];
         case Tables.MethodDef:
            return md.MethodDefinitions[index.Index];
         case Tables.Parameter:
            return md.ParameterDefinitions[index.Index];
         case Tables.InterfaceImpl:
            return md.InterfaceImplementations[index.Index];
         case Tables.MemberRef:
            return md.MemberReferences[index.Index];
         case Tables.Constant:
            return md.ConstantDefinitions[index.Index];
         case Tables.CustomAttribute:
            return md.CustomAttributeDefinitions[index.Index];
         case Tables.FieldMarshal:
            return md.FieldMarshals[index.Index];
         case Tables.DeclSecurity:
            return md.SecurityDefinitions[index.Index];
         case Tables.ClassLayout:
            return md.ClassLayouts[index.Index];
         case Tables.FieldLayout:
            return md.FieldLayouts[index.Index];
         case Tables.StandaloneSignature:
            return md.StandaloneSignatures[index.Index];
         case Tables.EventMap:
            return md.EventMaps[index.Index];
         case Tables.Event:
            return md.EventDefinitions[index.Index];
         case Tables.PropertyMap:
            return md.PropertyMaps[index.Index];
         case Tables.Property:
            return md.PropertyDefinitions[index.Index];
         case Tables.MethodSemantics:
            return md.MethodSemantics[index.Index];
         case Tables.MethodImpl:
            return md.MethodImplementations[index.Index];
         case Tables.ModuleRef:
            return md.ModuleReferences[index.Index];
         case Tables.TypeSpec:
            return md.TypeSpecifications[index.Index];
         case Tables.ImplMap:
            return md.MethodImplementationMaps[index.Index];
         case Tables.FieldRVA:
            return md.FieldRVAs[index.Index];
         case Tables.Assembly:
            return md.AssemblyDefinitions[index.Index];
         case Tables.AssemblyRef:
            return md.AssemblyReferences[index.Index];
         case Tables.File:
            return md.FieldDefinitions[index.Index];
         case Tables.ExportedType:
            return md.ExportedTypes[index.Index];
         case Tables.ManifestResource:
            return md.ManifestResources[index.Index];
         case Tables.NestedClass:
            return md.NestedClassDefinitions[index.Index];
         case Tables.GenericParameter:
            return md.GenericParameterDefinitions[index.Index];
         case Tables.MethodSpec:
            return md.MethodSpecifications[index.Index];
         case Tables.GenericParameterConstraint:
            return md.GenericParameterConstraintDefinitions[index.Index];
         case Tables.FieldPtr:
         case Tables.MethodPtr:
         case Tables.ParameterPtr:
         case Tables.EventPtr:
         case Tables.PropertyPtr:
         case Tables.EncLog:
         case Tables.EncMap:
         case Tables.AssemblyProcessor:
         case Tables.AssemblyOS:
         case Tables.AssemblyRefProcessor:
         case Tables.AssemblyRefOS:
         default:
            throw new ArgumentOutOfRangeException( "The table " + index.Table + " does not have representation in this framework." );
      }
   }

   // Assumes that all lists of CILMetaData have only non-null elements.
   // Assumes that MethodList, FieldList indices in TypeDef and ParameterList in MethodDef are all ordered correctly.
   // TODO check that everything works even though <Module> class is not a first row in TypeDef table
   // Duplicates not checked from the following tables:
   // TypeDef
   // MethodDef
   // FieldDef
   // PropertyDef
   // EventDef
   public static void OrderTablesAndUpdateSignatures( this CILMetaData md )
   {
      // 1. Create dictionary <Typedef, Int32> with reference-equality-comparer, which has the original index of each type-def
      //var originalTDefIndices = md.TypeDefinitions
      //   .Select( ( tDef, tDefIdx ) => new KeyValuePair<TypeDefinition, Int32>( tDef, tDefIdx ) )
      //   .ToDictionary( kvp => kvp.Key, kvp => kvp.Value, ReferenceEqualityComparer<TypeDefinition>.ReferenceBasedComparer );

      // 2. Sort NestedClass table (NestedClass) and remove duplicates
      // Start with this because sorting TypeDef requires accessing NestedClass table and therefore it is quicker to first sort NestedClass table
      var nestedClass = md.NestedClassDefinitions;
      var nestedClassIndices = CreateIndexArray( nestedClass.Count );
      nestedClass.SortMDTable( nestedClassIndices, Comparers.NestedClassDefinitionComparer );
      CheckMDDuplicatesSorted( nestedClass, nestedClassIndices, ( x, y ) => x.NestedClass == y.NestedClass );

      var typeDef = md.TypeDefinitions;
      var methodDef = md.MethodDefinitions;
      var fieldDef = md.FieldDefinitions;
      var paramDef = md.ParameterDefinitions;
      var tDefCount = typeDef.Count;
      var mDefCount = methodDef.Count;
      var fDefCount = fieldDef.Count;
      var pDefCount = paramDef.Count;
      var typeDefIndices = CreateIndexArray( tDefCount );

      // We have to pre-calculate method and field counts for types
      // We have to do this BEFORE typedef table is re-ordered
      var methodAndFieldCounts = new KeyValuePair<Int32, Int32>[tDefCount];
      for ( var i = 0; i < tDefCount; ++i )
      {
         var curTD = typeDef[i];
         Int32 mMax, fMax;
         if ( i + 1 < tDefCount )
         {
            var nextTD = typeDef[i + 1];
            mMax = nextTD.MethodList.Index;
            fMax = nextTD.FieldList.Index;
         }
         else
         {
            mMax = mDefCount;
            fMax = fDefCount;
         }
         methodAndFieldCounts[i] = new KeyValuePair<Int32, Int32>( mMax - curTD.MethodList.Index, fMax - curTD.FieldList.Index );
      }

      // We have to pre-calculate param count for methods
      // We have to do this BEFORE methoddef table is re-ordered
      var paramCounts = new Int32[mDefCount];
      for ( var i = 0; i < mDefCount; ++i )
      {
         var curMD = methodDef[i];
         Int32 max;
         if ( i + 1 < mDefCount )
         {
            max = methodDef[i + 1].ParameterList.Index;
         }
         else
         {
            max = pDefCount;
         }
         paramCounts[i] = max - curMD.ParameterList.Index;
      }

      // 3. Sort TypeDef, use dictionary created in step 1 to get original index, and then get nested-type index from sorted NestedClass with binary search
      //    When checking whether type x is enclosing type of type y, need to walk through whole enclosing-type chain (i.e. x may be enclosed type of z, which may be enclosed type of y)
      typeDef.SortMDTableWithInt32Comparison( typeDefIndices, ( x, y ) =>
      {
         // If x is greater than y, that means that typedef at index x has y in its declaring type chain
         // If y is greater than x, that means that typedef at index y has x in its declaring type chain
         // Otherwise, the order doesn't matter so we can consider them the same
         return x.GetDeclaringTypeChain( nestedClass ).Contains( y ) ?
            1 : ( y.GetDeclaringTypeChain( nestedClass ).Contains( x ) ?
               -1 :
               0 );
      } );
      // ECMA-335:
      // There shall be no duplicate rows in the TypeDef table, based on 
      // TypeNamespace+TypeName (unless this is a nested type - see below)  [ERROR] 
      // If this is a nested type, there shall be no duplicate row in the  TypeDef table, based 
      // upon TypeNamespace+TypeName+OwnerRowInNestedClassTable  [ERROR] 
      //typeDef.CheckMDDuplicatesUnsorted( typeDefIndices, ( x, y ) =>
      //{
      //   // return true only if both non-null
      //   var xTD = typeDef[x];
      //   var yTD = typeDef[y];
      //   var retVal = xTD != null && yTD != null;
      //   if ( retVal )
      //   {
      //      var xDecl = x.GetDeclaringTypeChain( nestedClass ).FirstOrDefaultCustom( -1 );
      //      var yDecl = y.GetDeclaringTypeChain( nestedClass ).FirstOrDefaultCustom( -1 );
      //      retVal = String.Equals( xTD.Name, yTD.Name )
      //         && String.Equals( xTD.Namespace, yTD.Namespace )
      //         && xDecl == yDecl;
      //   }
      //   return retVal;
      //}, x => typeDef[x].Name.GetHashCodeSafe() );

      // 4. Update NestedClass indices, sort NestedClass again, and remove duplicates again
      TableIndex aux;
      for ( var i = 0; i < nestedClass.Count; ++i )
      {
         var nc = nestedClass[i];
         if ( nc != null )
         {
            if ( nc.NestedClass.TryUpdateTableIndex( typeDefIndices, out aux ) )
            {
               nc.NestedClass = aux;
            }
            if ( nc.EnclosingClass.TryUpdateTableIndex( typeDefIndices, out aux ) )
            {
               nc.EnclosingClass = aux;
            }
         }
      }
      PopulateIndexArray( nestedClassIndices );
      nestedClass.SortMDTable( nestedClassIndices, Comparers.NestedClassDefinitionComparer );
      CheckMDDuplicatesSorted( nestedClass, nestedClassIndices, ( x, y ) => x.NestedClass == y.NestedClass );

      // 5. Sort MethodDef table and update references in TypeDef table
      var methodDefIndices = methodDef.ReOrderMDTableWithAscendingReferences(
         typeDef,
         typeDefIndices,
         td => td.MethodList.Index,
         ( td, mIdx ) => td.MethodList = new TableIndex( Tables.MethodDef, mIdx ),
         tdIdx => methodAndFieldCounts[tdIdx].Key
         );

      // 6. Sort ParameterDef table and update references in MethodDef table
      var paramDefIndices = paramDef.ReOrderMDTableWithAscendingReferences(
         methodDef,
         methodDefIndices,
         mDef => mDef.ParameterList.Index,
         ( mDef, pIdx ) => mDef.ParameterList = new TableIndex( Tables.Parameter, pIdx ),
         mdIdx => paramCounts[mdIdx]
         );

      // 7. Sort FieldDef table and update references in TypeDef table
      var fDefIndices = fieldDef.ReOrderMDTableWithAscendingReferences(
         typeDef,
         typeDefIndices,
         td => td.FieldList.Index,
         ( td, fIdx ) => td.FieldList = new TableIndex( Tables.Field, fIdx ),
         tdIdx => methodAndFieldCounts[tdIdx].Value
         );

      // 8. Remove duplicates from AssemblyRef table (since reordering of the TypeRef table will require the indices in this table to be present)
      // ECMA-335: The AssemblyRef table shall contain no duplicates (where duplicate rows are deemd  to be those having the same MajorVersion, MinorVersion, BuildNumber, RevisionNumber, PublicKeyOrToken, Name, and Culture) [WARNING] 
      var aRefs = md.AssemblyReferences;
      var aRefIndices = CreateIndexArray( aRefs.Count );
      aRefs.CheckMDDuplicatesUnsorted( aRefIndices, ( x, y ) =>
      {
         var xRef = aRefs[x];
         var yRef = aRefs[y];
         return xRef.AssemblyInformation.Equals( yRef.AssemblyInformation );
      }, x => aRefs[x].AssemblyInformation.GetHashCode() );

      // 9. Remove duplicates from ModuleRef table (since reordering of the TypeRef table will require the indices in this table to be present)
      // ECMA-335: There should be no duplicate rows  [WARNING] 
      var mRefs = md.ModuleReferences;
      var mRefIndices = CreateIndexArray( mRefs.Count );
      mRefs.CheckMDDuplicatesUnsorted( mRefIndices, ( x, y ) => String.Equals( mRefs[x].ModuleName, mRefs[y].ModuleName ), x => mRefs[x].ModuleName.GetHashCodeSafe() );

      var allTableIndices = new Int32[Consts.AMOUNT_OF_TABLES][];
      // ECMA-335: IL tokens shall be from TypeDef, TypeRef, TypeSpec, MethodDef, FieldDef, MemberRef or MethodSpec tables.
      // All table indices in signatures should only ever reference TypeDef, TypeRef or TypeSpec tables.
      // Tables with signatures are: FieldDef, MethodDef, MemberRef, CustomAttribute, DeclSecurity, StandaloneSignature, PropertyDef, TypeSpecification, MethodSpecification
      // The ones that have rules for duplicates: MemberRef, StandaloneSignature, PropertyDef, TypeSpecification, MethodSpecification
      // We will not handle PropertyDef (nor EventDef) duplicates
      // The ones without duplicates will be handled later (those are CustomAttribute, DeclSecurity)
      allTableIndices[(Int32) Tables.TypeDef] = typeDefIndices;
      allTableIndices[(Int32) Tables.TypeRef] = CreateIndexArray( md.TypeReferences.Count );
      allTableIndices[(Int32) Tables.TypeSpec] = CreateIndexArray( md.TypeSpecifications.Count );
      allTableIndices[(Int32) Tables.MethodDef] = methodDefIndices;
      allTableIndices[(Int32) Tables.Field] = fDefIndices;
      allTableIndices[(Int32) Tables.MemberRef] = CreateIndexArray( md.MemberReferences.Count );
      allTableIndices[(Int32) Tables.MethodSpec] = CreateIndexArray( md.MethodSpecifications.Count );
      allTableIndices[(Int32) Tables.StandaloneSignature] = CreateIndexArray( md.StandaloneSignatures.Count );
      allTableIndices[(Int32) Tables.AssemblyRef] = aRefIndices;
      allTableIndices[(Int32) Tables.ModuleRef] = mRefIndices;

      // Keep updating and removing duplicates from TypeRef, TypeSpec, MemberRef, MethodSpec, StandaloneSignature and Property tables, while updating all signatures and IL code
      md.UpdateSignaturesAndILWhileRemovingDuplicates( allTableIndices );

      // Now begins the part that mostly updates and sorts the table indices to tables
      allTableIndices[(Int32) Tables.Parameter] = paramDefIndices;

      // 8. Sort InterfaceImpl table ( Class, Interface)
      md.InterfaceImplementations.UpdateMDTableIndicesAndSort(
         Tables.InterfaceImpl,
         allTableIndices,
         Comparers.InterfaceImplementationComparer,
         ( iFaceImpl, indices ) => iFaceImpl.UpdateMDTableWithTableIndices2( indices, i => i.Class, ( i, c ) => i.Class = c, i => i.Interface, ( i, iface ) => i.Interface = iface )
         );

      // 9. Sort ConstantDef table (Parent)
      md.ConstantDefinitions.UpdateMDTableIndicesAndSort(
         Tables.Constant,
         allTableIndices,
         Comparers.ConstantDefinitionComparer,
         ( constant, indices ) => constant.UpdateMDTableWithTableIndices1( indices, c => c.Parent, ( c, p ) => c.Parent = p )
         );

      // 10. Sort FieldMarshal table (Parent)

      // 12. Sort ClassLayout table (Parent)

      // 13. Sort FieldLayout table (Field)

      // 14. Sort MethodSemantics table (Association)

      // 15. Sort MethodImpl table (Class)

      // 16. Sort ImplMap table (MemberForwarded)

      // 17. Sort FieldRVA table (Field)

      // 18. Sort GenericParamDef table (Owner, Sequence)

      // 19. Sort GenericParameterConstraint table (Owner)

      // 20. Sort CustomAttributeDef table (Parent)

      // Update table indices for:
      // EventMap
      // EventDefinition
      // PropertyMap
      // PropertyDefinition

   }

   private static void UpdateMDTableIndicesAndSort<T>( this List<T> table, Tables thisTable, Int32[][] allTableIndices, IComparer<T> comparer, Action<List<T>, Int32[][]> tableUpdateCallback )
   {
      var thisTableIndices = CreateIndexArray( table.Count );
      tableUpdateCallback( table, allTableIndices );
      table.SortMDTable( thisTableIndices, comparer );
      allTableIndices[(Int32) thisTable] = thisTableIndices;
   }

   private static void UpdateMDTableWithTableIndices1<T>( this List<T> table, Int32[][] tableIndices, Func<T, TableIndex> tableIndexGetter1, Action<T, TableIndex> tableIndexSetter1 )
   {
      foreach ( var row in table )
      {
         row.ProcessSingleTableIndexToUpdate( tableIndices, tableIndexGetter1, tableIndexSetter1 );
      }
   }

   private static void UpdateMDTableWithTableIndices1Nullable<T>( this List<T> table, Int32[][] tableIndices, Func<T, TableIndex?> tableIndexGetter1, Action<T, TableIndex> tableIndexSetter1 )
   {
      foreach ( var row in table )
      {
         row.ProcessSingleTableIndexToUpdateNullable( tableIndices, tableIndexGetter1, tableIndexSetter1 );
      }
   }

   private static void UpdateMDTableWithTableIndices2<T>( this List<T> table, Int32[][] tableIndices, Func<T, TableIndex> tableIndexGetter1, Action<T, TableIndex> tableIndexSetter1, Func<T, TableIndex> tableIndexGetter2, Action<T, TableIndex> tableIndexSetter2 )
   {
      foreach ( var row in table )
      {
         row.ProcessSingleTableIndexToUpdate( tableIndices, tableIndexGetter1, tableIndexSetter1 );
         row.ProcessSingleTableIndexToUpdate( tableIndices, tableIndexGetter2, tableIndexSetter2 );
      }
   }

   private static void ProcessSingleTableIndexToUpdate<T>( this T row, Int32[][] tableIndices, Func<T, TableIndex> tableIndexGetter, Action<T, TableIndex> tableIndexSetter )
   {
      row.ProcessSingleTableIndexToUpdateWithTableIndex( tableIndices, tableIndexGetter( row ), tableIndexSetter );
   }

   private static void ProcessSingleTableIndexToUpdateWithTableIndex<T>( this T row, Int32[][] tableIndices, TableIndex tableIndex, Action<T, TableIndex> tableIndexSetter )
   {
      var table = tableIndex.Table;
      var newIndex = tableIndices[(Int32) table][tableIndex.Index];
      if ( newIndex != tableIndex.Index )
      {
         tableIndexSetter( row, new TableIndex( table, newIndex ) );
      }
   }

   private static void ProcessSingleTableIndexToUpdateNullable<T>( this T row, Int32[][] tableIndices, Func<T, TableIndex?> tableIndexGetter, Action<T, TableIndex> tableIndexSetter )
   {
      var tIdx = tableIndexGetter( row );
      if ( tIdx.HasValue )
      {
         row.ProcessSingleTableIndexToUpdateWithTableIndex( tableIndices, tIdx.Value, tableIndexSetter );
      }
   }

   private static Boolean TryUpdateTableIndex( this TableIndex current, Int32[] targetTableIndices, out TableIndex newIndex )
   {
      var curIdx = current.Index;
      var newIdx = targetTableIndices[curIdx];
      var retVal = curIdx != newIdx;
      newIndex = retVal ?
         new TableIndex( current.Table, newIdx ) :
         current;
      return retVal;
   }

   private static void SortMDTable<T>( this List<T> table, Int32[] indices, IComparer<T> comparer )
   {
      table.SortMDTableWithInt32Comparison( indices, ( x, y ) => comparer.Compare( table[x], table[y] ) );
   }

   private static void SortMDTable<T>( this List<T> table, Int32[] indices, Comparison<T> comparison )
   {
      table.SortMDTableWithInt32Comparison( indices, ( x, y ) => comparison( table[x], table[y] ) );
   }

   private static void SortMDTableWithInt32Comparison<T>( this List<T> table, Int32[] indices, Comparison<Int32> comparison )
   {
      // If within 'indices' array, we have value '2' at index '0', it means that within the 'table', there should be value at index '0' which is currently at index '2'
      var count = table.Count;
      if ( count > 1 )
      {
         // Sort in such way that we know how indices are shuffled
         Array.Sort( indices, ( x, y ) => comparison( x, y ) );

         // Reshuffle according to indices
         // List.ToArray() is close to constant time because of Array.Copy being close to constant time
         // The only loss is somewhat bigger memory allocation
         var copy = table.ToArray();
         for ( var i = 0; i < count; ++i )
         {
            table[indices[i]] = copy[i];
         }
      }
   }

   private static Int32[] ReOrderMDTableWithAscendingReferences<T, U>( this List<T> table, List<U> referencingTable, Int32[] referencingTableIndices, Func<U, Int32> referenceIndexGetter, Action<U, Int32> referenceIndexSetter, Func<Int32, Int32> referenceCountGetter )
   {
      var refTableCount = referencingTable.Count;
      var thisTableIndices = CreateIndexArray( table.Count );

      // Comments talk about typedefs and methoddefs but this method is generalized to handle any two tables with ascending reference pattern
      // This loop walks one typedef at a time, updating methoddef index and re-ordering methoddef array as needed
      for ( Int32 tIdx = 0, mIdx = 0; tIdx < refTableCount; ++tIdx )
      {
         var curTD = referencingTable[tIdx];

         // Inclusive min (the method where current typedef points to)
         var min = referenceIndexGetter( curTD );

         // The count must be pre-calculated - we can't use typedef table to calculate that, as this for loop modifies the reference (e.g. MethodList property of TypeDefinition)
         var blockCount = referenceCountGetter( referencingTableIndices[tIdx] );

         if ( min != mIdx )
         {
            if ( blockCount > 0 )
            {
               // At least one element
               var array = new T[blockCount];

               // Save old elements into array
               table.CopyTo( mIdx, array, 0, blockCount );

               // Overwrite old elements in list with the this type's method list, and update method def indices
               for ( var i = 0; i < blockCount; ++i )
               {
                  var mDefIdx = thisTableIndices[i + min];
                  table[i + mIdx] = table[mDefIdx];
                  thisTableIndices[mDefIdx] = i + mIdx;
               }

               // Use elements in array to overwite elements we just read into current section
               for ( var i = 0; i < blockCount; ++i )
               {
                  table[i + min] = array[i];
                  thisTableIndices[i + mIdx] = i + min;
               }
            }

            // Set methoddef index for this typedef
            referenceIndexSetter( curTD, mIdx );
         }

         mIdx += blockCount;
      }

      return thisTableIndices;
   }

   // Assumes list is sorted
   private static Boolean CheckMDDuplicatesSorted<T>( List<T> list, Int32[] indices, Func<T, T, Boolean> duplicateComparer )
      where T : class
   {
      var foundDuplicates = false;
      var count = list.Count;
      if ( count > 1 )
      {
         var prevNotNullIndex = 0;
         for ( var i = 1; i < count; ++i )
         {
            if ( duplicateComparer( list[i], list[prevNotNullIndex] ) )
            {
               if ( !foundDuplicates )
               {
                  foundDuplicates = true;
               }

               list.AfterFindingDuplicate( indices, i, prevNotNullIndex );
            }
            else
            {
               prevNotNullIndex = i;
            }
         }
      }

      return foundDuplicates;
   }

   private static Boolean CheckMDDuplicatesUnsorted<T>( this List<T> list, Int32[] indices, IEqualityComparer<T> comparer )
      where T : class
   {
      return list.CheckMDDuplicatesUnsorted( indices, ( x, y ) => comparer.Equals( list[x], list[y] ), x => comparer.GetHashCode( list[x] ) );
   }

   private static Boolean CheckMDDuplicatesUnsorted<T>( this List<T> list, Int32[] indices, Func<Int32, Int32, Boolean> duplicateComparer, Func<Int32, Int32> hashCode )
      where T : class
   {
      var foundDuplicates = false;
      var count = list.Count;
      if ( count > 1 )
      {
         var set = new HashSet<Int32>( ComparerFromFunctions.NewEqualityComparer( duplicateComparer, hashCode ) );
         for ( var i = 0; i < list.Count; ++i )
         {
            if ( list[i] != null && !set.Add( i ) )
            {
               if ( !foundDuplicates )
               {
                  foundDuplicates = true;
               }

               var actualIndex = 0;
               while ( list[actualIndex] == null || !duplicateComparer( i, actualIndex ) )
               {
                  ++actualIndex;
               }

               list.AfterFindingDuplicate( indices, i, actualIndex );
            }
         }
      }

      return foundDuplicates;
   }

   private static void AfterFindingDuplicate<T>( this List<T> list, Int32[] indices, Int32 current, Int32 prevNotNullIndex )
      where T : class
   {
      // Mark as duplicate - replace value with null
      list[current] = null;

      // Update index which point to this to point to previous instead
      for ( var j = 0; j < indices.Length; ++j )
      {
         if ( indices[j] == current )
         {
            indices[j] = prevNotNullIndex;
            break;
         }
      }
   }

   private static Int32[] CreateIndexArray( Int32 size )
   {
      var retVal = new Int32[size];
      PopulateIndexArray( retVal );
      return retVal;
   }

   private static void PopulateIndexArray( Int32[] array )
   {
      var size = array.Length;
      // This might get called for already-used index array
      // Therefore, start from 0
      for ( var i = 0; i < size; ++i )
      {
         array[i] = i;
      }
   }

   private static void UpdateSignaturesAndILWhileRemovingDuplicates( this CILMetaData md, Int32[][] tableIndices )
   {
      var tRefs = md.TypeReferences;
      var tSpecs = md.TypeSpecifications;
      var memberRefs = md.MemberReferences;
      var mSpecs = md.MethodSpecifications;
      var standAloneSigs = md.StandaloneSignatures;
      var props = md.PropertyDefinitions;
      var updateState = new SignatureReorderState( md, tableIndices );
      Boolean removedTypeRefDuplicates, removedTSpecDuplicates, removedMemberRefDuplicates, removedMethodSpecDuplicates, removedStandaloneSigDuplicates, updatedSignatureTableIndices, updatedILTableIndices;
      // This has to be done in loop since modifying e.g. type specs will modify signatures, and thus might result in more typeref, typespec or memberref duplicates
      do
      {
         // ECMA-335:  There shall be no duplicate rows, where a duplicate has the same ResolutionScope, TypeName and TypeNamespace  [ERROR] 
         tRefs.UpdateMDTableWithTableIndices1Nullable( tableIndices, tRef => tRef.ResolutionScope, ( tRef, resScope ) => tRef.ResolutionScope = resScope );
         removedTypeRefDuplicates = tRefs.CheckMDDuplicatesUnsorted( tableIndices[(Int32) Tables.TypeRef], Comparers.TypeReferenceEqualityComparer );

         // ECMA-335: There shall be no duplicate rows, based upon Signature  [ERROR] 
         removedTSpecDuplicates = tSpecs.CheckMDDuplicatesUnsorted( tableIndices[(Int32) Tables.TypeSpec], Comparers.TypeSpecificationEqualityComparer );

         // ECMA-335:  The MemberRef table shall contain no duplicates, where duplicate rows have the same Class, Name, and Signature  [WARNING] 
         memberRefs.UpdateMDTableWithTableIndices1( tableIndices, mRef => mRef.DeclaringType, ( mRef, dType ) => mRef.DeclaringType = dType );
         removedMemberRefDuplicates = memberRefs.CheckMDDuplicatesUnsorted( tableIndices[(Int32) Tables.MemberRef], Comparers.MemberReferenceEqualityComparer );

         // ECMA-335: There shall be no duplicate rows based upon Method+Instantiation  [ERROR] 
         mSpecs.UpdateMDTableWithTableIndices1( tableIndices, mSpec => mSpec.Method, ( mSpec, method ) => mSpec.Method = method );
         removedMethodSpecDuplicates = mSpecs.CheckMDDuplicatesUnsorted( tableIndices[(Int32) Tables.MethodSpec], Comparers.MethodSpecificationEqualityComparer );

         // ECMA-335: Duplicates allowed (but we will make them all unique anyway)
         removedStandaloneSigDuplicates = standAloneSigs.CheckMDDuplicatesUnsorted( tableIndices[(Int32) Tables.StandaloneSignature], Comparers.StandaloneSignatureEqualityComparer );

         // We shall not check for property duplicates as they don't really bother this algorithm and we don't know generic enough case to handle them

         // Update signatures
         updateState.ResetChangedAny();
         updateState.UpdateSignatures();
         updatedSignatureTableIndices = updateState.ChangedAny;

         // Update table indices in IL
         updateState.ResetChangedAny();
         updateState.UpdateIL();
         updatedILTableIndices = updateState.ChangedAny;
      } while ( updatedSignatureTableIndices || updatedILTableIndices );
   }

   // This method updates all signature table indices and returns true if any table index was modified
   private static void UpdateSignatures( this SignatureReorderState state )
   {
      var md = state.MD;
      // 1. FieldDef
      foreach ( var field in md.FieldDefinitions.Where( f => f != null ) )
      {
         state.UpdateFieldSignature( field.Signature );
      }

      // 2. MethodDef
      foreach ( var method in md.MethodDefinitions.Where( m => m != null ) )
      {
         state.UpdateMethodDefSignature( method.Signature );
      }

      // 3. MemberRef
      foreach ( var member in md.MemberReferences.Where( m => m != null ) )
      {
         state.UpdateAbstractSignature( member.Signature );
      }

      // 4. StandaloneSignature
      foreach ( var sig in md.StandaloneSignatures.Where( s => s != null ) )
      {
         state.UpdateAbstractSignature( sig.Signature );
      }

      // 5. PropertyDef
      foreach ( var prop in md.PropertyDefinitions ) // No need for null check as property definition is not sorted nor checked for duplicates
      {
         state.UpdatePropertySignature( prop.Signature );
      }

      // 6. TypeSpec
      foreach ( var tSpec in md.TypeSpecifications.Where( t => t != null ) )
      {
         state.UpdateTypeSignature( tSpec.Signature );
      }

      // 7. MethodSpecification
      foreach ( var mSpec in md.MethodSpecifications.Where( m => m != null ) )
      {
         state.UpdateGenericMethodSignature( mSpec.Signature );
      }

      // CustomAttribute and DeclarativeSecurity signatures do not reference table indices, so they can be skipped
   }

   private static void UpdateIL( this SignatureReorderState state )
   {
      foreach ( var mDef in state.MD.MethodDefinitions )
      {
         var il = mDef.IL;
         if ( il != null )
         {
            // Local signature
            var localIdx = il.LocalsSignatureIndex;
            Int32 newIdx;
            if ( state.SignatureOrILTableIndexDiffersNullable( localIdx, il, out newIdx ) )
            {
               il.LocalsSignatureIndex = localIdx.Value.ChangeIndex( newIdx );
            }

            // Exception blocks
            foreach ( var block in il.ExceptionBlocks )
            {
               var excIdx = block.ExceptionType;
               if ( state.SignatureOrILTableIndexDiffersNullable( excIdx, block, out newIdx ) )
               {
                  block.ExceptionType = excIdx.Value.ChangeIndex( newIdx );
               }
            }

            // Op codes
            foreach ( var code in il.OpCodes.Where( code => code.InfoKind == OpCodeOperandKind.OperandToken ) )
            {
               var codeInfo = (OpCodeInfoWithToken) code;
               var token = codeInfo.Operand;
               if ( state.SignatureOrILTableIndexDiffers( token, code, out newIdx ) )
               {
                  codeInfo.Operand = token.ChangeIndex( newIdx );
               }
            }
         }
      }
   }

   private static void UpdateAbstractSignature( this SignatureReorderState state, AbstractSignature sig )
   {
      switch ( sig.SignatureKind )
      {
         case SignatureKind.Field:
            state.UpdateFieldSignature( (FieldSignature) sig );
            break;
         case SignatureKind.GenericMethodInstantiation:
            state.UpdateGenericMethodSignature( (GenericMethodSignature) sig );
            break;
         case SignatureKind.LocalVariables:
            state.UpdateLocalVariablesSignature( (LocalVariablesSignature) sig );
            break;
         case SignatureKind.MethodDefinition:
            state.UpdateMethodDefSignature( (MethodDefinitionSignature) sig );
            break;
         case SignatureKind.MethodReference:
            state.UpdateMethodRefSignature( (MethodReferenceSignature) sig );
            break;
         case SignatureKind.Property:
            state.UpdatePropertySignature( (PropertySignature) sig );
            break;
         case SignatureKind.Type:
            state.UpdateTypeSignature( (TypeSignature) sig );
            break;
      }
   }

   private static void UpdateFieldSignature( this SignatureReorderState state, FieldSignature sig )
   {
      state.UpdateSignatureCustomModifiers( sig.CustomModifiers );
      state.UpdateTypeSignature( sig.Type );
   }

   private static void UpdateGenericMethodSignature( this SignatureReorderState state, GenericMethodSignature sig )
   {
      foreach ( var gArg in sig.GenericArguments )
      {
         state.UpdateTypeSignature( gArg );
      }
   }

   private static void UpdateLocalVariablesSignature( this SignatureReorderState state, LocalVariablesSignature sig )
   {
      state.UpdateParameterSignatures( sig.Locals );
   }

   private static void UpdatePropertySignature( this SignatureReorderState state, PropertySignature sig )
   {
      state.UpdateSignatureCustomModifiers( sig.CustomModifiers );
      state.UpdateParameterSignatures( sig.Parameters );
      state.UpdateTypeSignature( sig.PropertyType );
   }

   private static void UpdateTypeSignature( this SignatureReorderState state, TypeSignature sig )
   {
      switch ( sig.TypeSignatureKind )
      {
         case TypeSignatureKind.ClassOrValue:
            var clazz = (ClassOrValueTypeSignature) sig;
            Int32 newIdx;
            var type = clazz.Type;
            if ( state.SignatureOrILTableIndexDiffers( type, clazz, out newIdx ) )
            {
               clazz.Type = type.ChangeIndex( newIdx );
            }
            foreach ( var gArg in clazz.GenericArguments )
            {
               state.UpdateTypeSignature( gArg );
            }
            break;
         case TypeSignatureKind.ComplexArray:
            state.UpdateTypeSignature( ( (ComplexArrayTypeSignature) sig ).ArrayType );
            break;
         case TypeSignatureKind.FunctionPointer:
            state.UpdateMethodRefSignature( ( (FunctionPointerTypeSignature) sig ).MethodSignature );
            break;
         case TypeSignatureKind.Pointer:
            var ptr = (PointerTypeSignature) sig;
            state.UpdateSignatureCustomModifiers( ptr.CustomModifiers );
            state.UpdateTypeSignature( ptr.PointerType );
            break;
         case TypeSignatureKind.SimpleArray:
            var array = (SimpleArrayTypeSignature) sig;
            state.UpdateSignatureCustomModifiers( array.CustomModifiers );
            state.UpdateTypeSignature( array.ArrayType );
            break;
      }
   }

   private static void UpdateSignatureCustomModifiers( this SignatureReorderState state, IList<CustomModifierSignature> mods )
   {
      Int32 newIndex;
      foreach ( var mod in mods )
      {
         var idx = mod.CustomModifierType;
         if ( state.SignatureOrILTableIndexDiffers( idx, mod, out newIndex ) )
         {
            mod.CustomModifierType = idx.ChangeIndex( newIndex );
         }
      }
   }

   private static void UpdateAbstractMethodSignature( this SignatureReorderState state, AbstractMethodSignature sig )
   {
      state.UpdateParameterSignature( sig.ReturnType );
      state.UpdateParameterSignatures( sig.Parameters );
   }

   private static void UpdateMethodDefSignature( this SignatureReorderState state, MethodDefinitionSignature sig )
   {
      state.UpdateAbstractMethodSignature( sig );
   }

   private static void UpdateMethodRefSignature( this SignatureReorderState state, MethodReferenceSignature sig )
   {
      state.UpdateAbstractMethodSignature( sig );
      state.UpdateParameterSignatures( sig.VarArgsParameters );
   }

   private static void UpdateParameterSignatures<T>( this SignatureReorderState state, List<T> parameters )
      where T : ParameterOrLocalVariableSignature
   {
      foreach ( var param in parameters )
      {
         state.UpdateParameterSignature( param );
      }
   }

   private static void UpdateParameterSignature( this SignatureReorderState state, ParameterOrLocalVariableSignature parameter )
   {
      state.UpdateSignatureCustomModifiers( parameter.CustomModifiers );
      state.UpdateTypeSignature( parameter.Type );
   }

   private static Boolean SignatureOrILTableIndexDiffersNullable( this SignatureReorderState state, this TableIndex? index, Object parent, out Int32 newIndex )
   {
      var retVal = index.HasValue;
      if ( retVal )
      {
         state.SignatureOrILTableIndexDiffers( index.Value, parent, out newIndex );
      }
      else
      {
         newIndex = -1;
      }
      return retVal;
   }

   private static Boolean SignatureOrILTableIndexDiffers( this SignatureReorderState state, TableIndex index, Object parent, out Int32 newIndex )
   {
      var oldIndex = index.Index;
      var visitedDic = state.VisitedInfo;
      var retVal = !visitedDic.ContainsKey( parent ); // Check whether we have already visited this index
      // Even if we have already visited this, have to check here whether target is null
      // If target is null, that means that after visiting, the target became duplicate and was removed
      // So we need to update it again
      if ( retVal || state.MD.GetByTableIndex( index ) == null )
      {
         if ( retVal )
         {
            visitedDic.Add( parent, parent );
         }
         newIndex = state.TableIndices[(Int32) index.Table][oldIndex]; // TODO NullRef if not TypeDef/TypeRef/TypeSpec, ArrayIndexOutOfBounds if invalid index!
         retVal = oldIndex != newIndex;
         // Mark that we have changed an index
         state.ProcessedTableIndex( retVal );
      }
      else
      {
         newIndex = -1;
      }
      return retVal;
   }

   private static IEnumerable<Int32> GetDeclaringTypeChain( this Int32 typeDefIndex, IList<NestedClassDefinition> sortedNestedClass )
   {
      return typeDefIndex.AsSingleBranchEnumerable( cur =>
         {
            var nIdx = sortedNestedClass.FindDeclaringTypeIndexFromSortedNestedClass( cur );
            if ( nIdx != -1 )
            {
               nIdx = sortedNestedClass[nIdx].EnclosingClass.Index;
            }
            return nIdx;
         }, false, cur => cur == -1 || cur == typeDefIndex ); // Stop also when we hit the same index again (illegal situation but possible), and don't include itself
   }

   private static Int32 FindDeclaringTypeIndexFromSortedNestedClass( this IList<NestedClassDefinition> sortedNestedClass, Int32 currentTypeDefIndex )
   {
      using ( var xDeclTypeRows = sortedNestedClass.GetReferencingRowsFromOrderedWithIndex( Tables.TypeDef, currentTypeDefIndex, nIdx =>
      {
         while ( sortedNestedClass[nIdx] == null )
         {
            --nIdx;
         }
         return sortedNestedClass[nIdx].NestedClass;
      } ).GetEnumerator() )
      {
         return xDeclTypeRows.MoveNext() ?
            xDeclTypeRows.Current : // has declaring type. 
            -1; // does not have declaring type. 
      }
   }

   // Returns token with 1-based indexing, or zero if tableIdx has no value
   internal static Int32 CreateTokenForEmittingOptionalTableIndex( this TableIndex? tableIdx )
   {
      return tableIdx.HasValue ?
         ZeroBasedTokenToOneBasedToken( tableIdx.Value.ZeroBasedToken ) :
         0;
   }

   // Returns token with 1-based indexing
   internal static Int32 CreateTokenForEmittingMandatoryTableIndex( this TableIndex tableIdx )
   {
      return ZeroBasedTokenToOneBasedToken( tableIdx.ZeroBasedToken );
   }

   private static Int32 ZeroBasedTokenToOneBasedToken( Int32 token )
   {
      return ( ( token & TokenUtils.INDEX_MASK ) + 1 ) | ( token & ~TokenUtils.INDEX_MASK );
   }

   public static Int32 CalculateStackSize( this CILMetaData md, Int32 methodIndex )
   {
      var mDef = md.MethodDefinitions.GetOrNull( methodIndex );
      var retVal = -1;
      if ( mDef != null )
      {
         var il = mDef.IL;
         if ( il != null )
         {
            var state = new StackCalculationState( md, il.OpCodes.Sum( oc => oc.ByteSize ) );

            // Setup exception block stack sizes
            foreach ( var block in il.ExceptionBlocks )
            {
               switch ( block.BlockType )
               {
                  case ExceptionBlockType.Exception:
                     state.StackSizes[block.HandlerOffset] = 1;
                     break;
                  case ExceptionBlockType.Filter:
                     state.StackSizes[block.HandlerOffset] = 1;
                     state.StackSizes[block.FilterOffset] = 1;
                     break;
               }
            }

            // Calculate actual max stack
            foreach ( var codeInfo in il.OpCodes )
            {
               state.CurrentCodeByteOffset += codeInfo.OpCode.Size;
               state.NextCodeByteOffset += codeInfo.ByteSize;
               UpdateStackSize( state, codeInfo );
            }

            retVal = state.MaxStack;
         }
      }

      return retVal;
   }

   private static void UpdateStackSize(
      StackCalculationState state,
      OpCodeInfo codeInfo
      )
   {
      var code = codeInfo.OpCode;
      var curStacksize = Math.Max( state.CurrentStack, state.StackSizes[state.CurrentCodeByteOffset] );
      if ( FlowControl.Call == code.FlowControl )
      {
         curStacksize = UpdateStackSizeForMethod( state, code, ( (OpCodeInfoWithToken) codeInfo ).Operand, curStacksize );
      }
      else
      {
         curStacksize += code.StackChange;
      }

      // Save max stack here
      state.UpdateMaxStack( curStacksize );

      // Copy branch stack size
      if ( curStacksize > 0 )
      {
         switch ( code.OperandType )
         {
            case OperandType.InlineBrTarget:
               UpdateStackSizeAtBranchTarget( state, ( (OpCodeInfoWithInt32) codeInfo ).Operand, curStacksize );
               break;
            case OperandType.ShortInlineBrTarget:
               UpdateStackSizeAtBranchTarget( state, ( (OpCodeInfoWithInt32) codeInfo ).Operand, curStacksize );
               break;
            case OperandType.InlineSwitch:
               var offsets = ( (OpCodeInfoWithSwitch) codeInfo ).Offsets;
               for ( var i = 0; i < offsets.Count; ++i )
               {
                  UpdateStackSizeAtBranchTarget( state, offsets[i], curStacksize );
               }
               break;
         }
      }

      // Set stack to zero if required
      if ( code.UnconditionallyEndsBulkOfCode )
      {
         curStacksize = 0;
      }

      // Save current size for next iteration
      state.CurrentStack = curStacksize;
   }

   private static Int32 UpdateStackSizeForMethod(
      StackCalculationState state,
      OpCode code,
      TableIndex method,
      Int32 curStacksize
      )
   {
      var sig = ResolveSignatureFromTableIndex( state, method );

      if ( sig != null )
      {
         if ( sig.SignatureStarter.IsHasThis() && OpCodes.Newobj != code )
         {
            // Pop 'this'
            --curStacksize;
         }

         // Pop parameters
         curStacksize -= sig.Parameters.Count;
         var refSig = sig as MethodReferenceSignature;
         if ( refSig != null )
         {
            curStacksize -= refSig.VarArgsParameters.Count;
         }

         if ( OpCodes.Calli == code )
         {
            // Pop function pointer
            --curStacksize;
         }

         var rType = sig.ReturnType.Type;

         // TODO we could check here for stack underflow!

         if ( OpCodes.Newobj == code
            || rType.TypeSignatureKind != TypeSignatureKind.Simple
            || ( (SimpleTypeSignature) rType ).SimpleType != SignatureElementTypes.Void
            )
         {
            // Push return value
            ++curStacksize;
         }
      }

      return curStacksize;
   }

   private static AbstractMethodSignature ResolveSignatureFromTableIndex(
      StackCalculationState state,
      TableIndex method
      )
   {
      var mIdx = method.Index;
      switch ( method.Table )
      {
         case Tables.MethodDef:
            var mDef = state.MD.MethodDefinitions.GetOrNull( mIdx );
            return mDef == null ? null : mDef.Signature;
         case Tables.MemberRef:
            var mRef = state.MD.MemberReferences.GetOrNull( mIdx );
            return mRef == null ? null : mRef.Signature as AbstractMethodSignature;
         case Tables.StandaloneSignature:
            var sig = state.MD.StandaloneSignatures.GetOrNull( mIdx );
            return sig == null ? null : sig.Signature as AbstractMethodSignature;
         case Tables.MethodSpec:
            var mSpec = state.MD.MethodSpecifications.GetOrNull( mIdx );
            return mSpec == null ? null : ResolveSignatureFromTableIndex( state, mSpec.Method );
         default:
            return null;
      }
   }

   private static void UpdateStackSizeAtBranchTarget(
      StackCalculationState state,
      Int32 jump,
      Int32 stackSize
      )
   {
      var idx = state.NextCodeByteOffset + jump;
      state.StackSizes[idx] = Math.Max( state.StackSizes[idx], stackSize );
   }
}
