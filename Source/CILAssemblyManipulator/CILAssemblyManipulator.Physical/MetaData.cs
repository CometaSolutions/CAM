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
      private readonly ISet<TableIndex> _duplicatesThisRound;
      private readonly Int32[][] _tableIndices;
      private readonly IDictionary<Object, Object> _visitedInfo;
      private Boolean _updatedAny;

      internal SignatureReorderState( CILMetaData md, Int32[][] tableIndices )
      {
         this._md = md;
         this._duplicatesThisRound = new HashSet<TableIndex>();
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

      public ISet<TableIndex> DuplicatesThisRound
      {
         get
         {
            return this._duplicatesThisRound;
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

   private sealed class MetaDataReOrderState
   {
      private readonly IDictionary<Tables, ISet<Int32>> _duplicates;

      internal MetaDataReOrderState()
      {
         this._duplicates = new Dictionary<Tables, ISet<Int32>>();
      }

      public IDictionary<Tables, ISet<Int32>> Duplicates
      {
         get
         {
            return this._duplicates;
         }
      }

      public void MarkDuplicate( Tables table, Int32 idx )
      {
         this._duplicates
            .GetOrAdd_NotThreadSafe( table, t => new HashSet<Int32>() )
            .Add( idx );
      }

      //public Boolean IsDuplicate( Tables table, Int32 idx )
      //{
      //   ISet<Int32> set;
      //   return this._duplicates.TryGetValue( table, out set )
      //      && set.Contains( idx );
      //}
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
      if ( method == null || method.IL == null )
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

   public static IEnumerable<MethodDefinition> GetTypeMethods( this CILMetaData md, Int32 typeDefIndex )
   {
      return md.GetTypeMethodIndices( typeDefIndex ).Select( idx => md.MethodDefinitions[idx] );
   }

   public static IEnumerable<FieldDefinition> GetTypeFields( this CILMetaData md, Int32 typeDefIndex )
   {
      return md.GetTypeFieldIndices( typeDefIndex ).Select( idx => md.FieldDefinitions[idx] );
   }

   public static IEnumerable<ParameterDefinition> GetMethodParameters( this CILMetaData md, Int32 methodDefIndex )
   {
      return md.GetMethodParameterIndices( methodDefIndex ).Select( idx => md.ParameterDefinitions[idx] );
   }

   public static IEnumerable<Int32> GetTypeMethodIndices( this CILMetaData md, Int32 typeDefIndex )
   {
      return md.TypeDefinitions.GetTargetIndicesForAscendingReferenceListTable( md.MethodDefinitions.Count, typeDefIndex, td => td.MethodList.Index );
   }

   public static IEnumerable<Int32> GetTypeFieldIndices( this CILMetaData md, Int32 typeDefIndex )
   {
      return md.TypeDefinitions.GetTargetIndicesForAscendingReferenceListTable( md.FieldDefinitions.Count, typeDefIndex, td => td.FieldList.Index );
   }

   public static IEnumerable<Int32> GetMethodParameterIndices( this CILMetaData md, Int32 methodDefIndex )
   {
      return md.MethodDefinitions.GetTargetIndicesForAscendingReferenceListTable( md.ParameterDefinitions.Count, methodDefIndex, mdef => mdef.ParameterList.Index );
   }

   internal static IEnumerable<Int32> GetTargetIndicesForAscendingReferenceListTable<T>( this List<T> tableWithReferences, Int32 targetTableCount, Int32 tableWithReferencesIndex, Func<T, Int32> referenceExtractor )
   {
      if ( tableWithReferencesIndex < 0 || tableWithReferencesIndex >= tableWithReferences.Count )
      {
         throw new ArgumentOutOfRangeException( "Table index." );
      }

      var min = referenceExtractor( tableWithReferences[tableWithReferencesIndex] );
      var max = tableWithReferencesIndex < tableWithReferences.Count - 1 ?
         referenceExtractor( tableWithReferences[tableWithReferencesIndex + 1] ) :
         targetTableCount;
      while ( min < max )
      {
         yield return min;
         ++min;
      }
   }

   public static TableIndex ChangeIndex( this TableIndex index, Int32 newIndex )
   {
      return new TableIndex( index.Table, newIndex );
   }

   public static Object GetByTableIndex( this CILMetaData md, TableIndex index )
   {
      Object retVal;
      if ( !md.TryGetByTableIndex( index, out retVal ) )
      {
         switch ( index.Table )
         {
            case Tables.Module:
            case Tables.TypeRef:
            case Tables.TypeDef:
            case Tables.Field:
            case Tables.MethodDef:
            case Tables.Parameter:
            case Tables.InterfaceImpl:
            case Tables.MemberRef:
            case Tables.Constant:
            case Tables.CustomAttribute:
            case Tables.FieldMarshal:
            case Tables.DeclSecurity:
            case Tables.ClassLayout:
            case Tables.FieldLayout:
            case Tables.StandaloneSignature:
            case Tables.EventMap:
            case Tables.Event:
            case Tables.PropertyMap:
            case Tables.Property:
            case Tables.MethodSemantics:
            case Tables.MethodImpl:
            case Tables.ModuleRef:
            case Tables.TypeSpec:
            case Tables.ImplMap:
            case Tables.FieldRVA:
            case Tables.Assembly:
            case Tables.AssemblyRef:
            case Tables.File:
            case Tables.ExportedType:
            case Tables.ManifestResource:
            case Tables.NestedClass:
            case Tables.GenericParameter:
            case Tables.MethodSpec:
            case Tables.GenericParameterConstraint:
               throw new ArgumentOutOfRangeException( "Table index " + index + " was out of range." );
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
               throw new InvalidOperationException( "The table " + index.Table + " does not have representation in this framework." );
         }
      }

      return retVal;
   }

   public static Boolean TryGetByTableIndex( this CILMetaData md, TableIndex index, out Object row )
   {
      // TODO check table size!

      var retVal = index.Index >= 0;
      row = null;
      if ( retVal )
      {
         switch ( index.Table )
         {
            case Tables.Module:
               if ( index.Index == 0 )
               {
                  row = md;
               }
               else
               {
                  retVal = false;
               }
               break;
            case Tables.TypeRef:
               row = md.TypeReferences[index.Index];
               break;
            case Tables.TypeDef:
               row = md.TypeDefinitions[index.Index];
               break;
            case Tables.Field:
               row = md.FieldDefinitions[index.Index];
               break;
            case Tables.MethodDef:
               row = md.MethodDefinitions[index.Index];
               break;
            case Tables.Parameter:
               row = md.ParameterDefinitions[index.Index];
               break;
            case Tables.InterfaceImpl:
               row = md.InterfaceImplementations[index.Index];
               break;
            case Tables.MemberRef:
               row = md.MemberReferences[index.Index];
               break;
            case Tables.Constant:
               row = md.ConstantDefinitions[index.Index];
               break;
            case Tables.CustomAttribute:
               row = md.CustomAttributeDefinitions[index.Index];
               break;
            case Tables.FieldMarshal:
               row = md.FieldMarshals[index.Index];
               break;
            case Tables.DeclSecurity:
               row = md.SecurityDefinitions[index.Index];
               break;
            case Tables.ClassLayout:
               row = md.ClassLayouts[index.Index];
               break;
            case Tables.FieldLayout:
               row = md.FieldLayouts[index.Index];
               break;
            case Tables.StandaloneSignature:
               row = md.StandaloneSignatures[index.Index];
               break;
            case Tables.EventMap:
               row = md.EventMaps[index.Index];
               break;
            case Tables.Event:
               row = md.EventDefinitions[index.Index];
               break;
            case Tables.PropertyMap:
               row = md.PropertyMaps[index.Index];
               break;
            case Tables.Property:
               row = md.PropertyDefinitions[index.Index];
               break;
            case Tables.MethodSemantics:
               row = md.MethodSemantics[index.Index];
               break;
            case Tables.MethodImpl:
               row = md.MethodImplementations[index.Index];
               break;
            case Tables.ModuleRef:
               row = md.ModuleReferences[index.Index];
               break;
            case Tables.TypeSpec:
               row = md.TypeSpecifications[index.Index];
               break;
            case Tables.ImplMap:
               row = md.MethodImplementationMaps[index.Index];
               break;
            case Tables.FieldRVA:
               row = md.FieldRVAs[index.Index];
               break;
            case Tables.Assembly:
               row = md.AssemblyDefinitions[index.Index];
               break;
            case Tables.AssemblyRef:
               row = md.AssemblyReferences[index.Index];
               break;
            case Tables.File:
               row = md.FieldDefinitions[index.Index];
               break;
            case Tables.ExportedType:
               row = md.ExportedTypes[index.Index];
               break;
            case Tables.ManifestResource:
               row = md.ManifestResources[index.Index];
               break;
            case Tables.NestedClass:
               row = md.NestedClassDefinitions[index.Index];
               break;
            case Tables.GenericParameter:
               row = md.GenericParameterDefinitions[index.Index];
               break;
            case Tables.MethodSpec:
               row = md.MethodSpecifications[index.Index];
               break;
            case Tables.GenericParameterConstraint:
               row = md.GenericParameterConstraintDefinitions[index.Index];
               break;
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
               retVal = false;
               break;
         }
      }

      return retVal;
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
   // NestedClass

   // TypeDef and MethodDef can not have duplicate instances of same object!!
   public static Int32[][] OrderTablesAndUpdateSignatures( this CILMetaData md )
   {
      var allTableIndices = new Int32[Consts.AMOUNT_OF_TABLES][];

      // Start by re-ordering structural (TypeDef, MethodDef, ParamDef, Field, NestedClass) tables
      md.ReOrderStructuralTables( allTableIndices );

      // Keep updating and removing duplicates from TypeRef, TypeSpec, MemberRef, MethodSpec, StandaloneSignature and Property tables, while updating all signatures and IL code
      var reorderState = new MetaDataReOrderState();
      md.UpdateSignaturesAndILWhileRemovingDuplicates( allTableIndices, reorderState );

      // Update and sort the remaining tables which don't have signatures
      md.UpdateAndSortTablesWithNoSignatures( allTableIndices );

      // Remove duplicates
      md.RemoveDuplicatesAfterSorting( reorderState );
      return allTableIndices;
   }

   // Re-orders TypeDef, MethodDef, ParamDef, Field, and NestedClass tables, if necessary
   private static void ReOrderStructuralTables( this CILMetaData md, Int32[][] allTableIndices )
   {
      var typeDef = md.TypeDefinitions;
      var methodDef = md.MethodDefinitions;
      var fieldDef = md.FieldDefinitions;
      var paramDef = md.ParameterDefinitions;
      var nestedClass = md.NestedClassDefinitions;
      var tDefCount = typeDef.Count;
      var mDefCount = methodDef.Count;
      var fDefCount = fieldDef.Count;
      var pDefCount = paramDef.Count;
      var ncCount = nestedClass.Count;

      var typeDefIndices = CreateIndexArray( tDefCount );
      var methodDefIndices = CreateIndexArray( mDefCount );
      var paramDefIndices = CreateIndexArray( pDefCount );
      var fDefIndices = CreateIndexArray( fDefCount );
      var ncIndices = CreateIndexArray( ncCount );

      // Set table indices
      allTableIndices[(Int32) Tables.TypeDef] = typeDefIndices;
      allTableIndices[(Int32) Tables.MethodDef] = methodDefIndices;
      allTableIndices[(Int32) Tables.Field] = fDefIndices;
      allTableIndices[(Int32) Tables.Parameter] = paramDefIndices;
      allTableIndices[(Int32) Tables.NestedClass] = ncIndices;

      // So, start by reading nested class data into more easily accessible data structure

      // TypeDef table has special constraint - enclosing class must precede nested class.
      // In other words, for all rows in NestedClass table, the EnclosingClass index must be less than NestedClass index
      // All the tables that are handled in this method will only be needed to re-shuffle if TypeDef table changes, that is, if there are violating rows in NestedClass table.
      var typeDefOrderingChanged = true; // nestedClass.Any( nc => nc.NestedClass.Index < nc.EnclosingClass.Index );

      if ( typeDefOrderingChanged )
      {
         // We have to pre-calculate method and field counts for types
         // We have to do this BEFORE typedef table is re-ordered
         var methodAndFieldCounts = new Dictionary<TypeDefinition, KeyValuePair<Int32, Int32>>( tDefCount, ReferenceEqualityComparer<TypeDefinition>.ReferenceBasedComparer );
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
            methodAndFieldCounts.Add( curTD, new KeyValuePair<Int32, Int32>( mMax - curTD.MethodList.Index, fMax - curTD.FieldList.Index ) );
         }

         // We have to pre-calculate param count for methods
         // We have to do this BEFORE methoddef table is re-ordered
         var paramCounts = new Dictionary<MethodDefinition, Int32>( mDefCount, ReferenceEqualityComparer<MethodDefinition>.ReferenceBasedComparer );
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
            paramCounts.Add( curMD, max - curMD.ParameterList.Index );
         }

         // Create data structure
         var nestedClassInfo = new Dictionary<Int32, List<Int32>>(); // Key - enclosing type which is lower in TypeDef table than its nested type, Value: list of nested types higher in TypeDef table
         var nestedTypeIndices = new HashSet<Int32>();
         // Populate data structure
         foreach ( var nc in nestedClass )
         {
            var enclosing = nc.EnclosingClass.Index;
            var nested = nc.NestedClass.Index;
            nestedClassInfo
                  .GetOrAdd_NotThreadSafe( enclosing, i => new List<Int32>( 1 ) )
                  .Add( nested );
            nestedTypeIndices.Add( nested );
         }
         // Now we can sort TypeDef table

         // Probably most simple and efficient way is to just add nested types right after enclosing types, in BFS style and update typeDefIndices as we go.
         var tDefCopy = typeDef.ToArray();
         for ( Int32 i = 0, tDefCopyIdx = 0; i < tDefCount; ++i, ++tDefCopyIdx )
         {
            // If we encounter nested type HERE, it means that this nested type is above of enclosing type in the table, skip that
            while ( nestedTypeIndices.Contains( tDefCopyIdx ) )
            {
               ++tDefCopyIdx;
            }

            // Type at index 'tDefCopyIdx' is guaranteed now to be top-level type
            if ( i != tDefCopyIdx )
            {
               typeDef[i] = tDefCopy[tDefCopyIdx];
               typeDefIndices[tDefCopyIdx] = i;
            }

            // Does this type has nested types
            if ( nestedClassInfo.ContainsKey( tDefCopyIdx ) )
            {
               // Iterate all nested types with BFS
               foreach ( var nested in tDefCopyIdx.AsBreadthFirstEnumerable( cur =>
               {
                  List<Int32> nestedTypes;
                  return nestedClassInfo.TryGetValue( cur, out nestedTypes ) ?
                     nestedTypes :
                     Empty<Int32>.Enumerable;
               }, false ) // Skip this type
               .EndOnFirstLoop() ) // Detect loops to avoid infite enumerable
               {
                  typeDef[++i] = tDefCopy[nested];
                  typeDefIndices[nested] = i;
               }
            }
         }
         // Update NestedClass indices and sort NestedClass
         nestedClass.UpdateMDTableWithTableIndices2(
            allTableIndices,
            nc => nc.NestedClass,
            ( nc, ncIdx ) => nc.NestedClass = ncIdx,
            nc => nc.EnclosingClass,
            ( nc, ecIdx ) => nc.EnclosingClass = ecIdx
            );
         nestedClass.SortMDTable( ncIndices, Comparers.NestedClassDefinitionComparer );

         // Sort MethodDef table and update references in TypeDef table
         methodDef.ReOrderMDTableWithAscendingReferences(
            methodDefIndices,
            typeDef,
            typeDefIndices,
            td => td.MethodList.Index,
            ( td, mIdx ) => td.MethodList = new TableIndex( Tables.MethodDef, mIdx ),
            tdIdx => methodAndFieldCounts[tdIdx].Key
            );

         // Sort ParameterDef table and update references in MethodDef table
         paramDef.ReOrderMDTableWithAscendingReferences(
            paramDefIndices,
            methodDef,
            methodDefIndices,
            mDef => mDef.ParameterList.Index,
            ( mDef, pIdx ) => mDef.ParameterList = new TableIndex( Tables.Parameter, pIdx ),
            mdIdx => paramCounts[mdIdx]
            );

         // Sort FieldDef table and update references in TypeDef table
         fieldDef.ReOrderMDTableWithAscendingReferences(
            fDefIndices,
            typeDef,
            typeDefIndices,
            td => td.FieldList.Index,
            ( td, fIdx ) => td.FieldList = new TableIndex( Tables.Field, fIdx ),
            tdIdx => methodAndFieldCounts[tdIdx].Value
            );
      }
   }

   private static void UpdateAndSortTablesWithNoSignatures( this CILMetaData md, Int32[][] allTableIndices )
   {
      // Create table index arrays for tables which are untouched (but can be used by various table indices in table rows)
      allTableIndices[(Int32) Tables.Assembly] = CreateIndexArray( md.AssemblyDefinitions.Count );
      allTableIndices[(Int32) Tables.File] = CreateIndexArray( md.FileReferences.Count );
      allTableIndices[(Int32) Tables.Property] = CreateIndexArray( md.PropertyDefinitions.Count );

      // Update TypeDef
      md.TypeDefinitions.UpdateMDTableIndices(
         Tables.TypeDef,
         allTableIndices,
         null,
         ( td, indices ) => td.UpdateMDTableWithTableIndices1Nullable( allTableIndices, t => t.BaseType, ( t, b ) => t.BaseType = b )
         );

      // Update EventDefinition
      md.EventDefinitions.UpdateMDTableIndices(
         Tables.Event,
         allTableIndices,
         null,
         ( ed, indices ) => ed.UpdateMDTableWithTableIndices1( indices, e => e.EventType, ( e, t ) => e.EventType = t )
         );

      // Update EventMap
      md.EventMaps.UpdateMDTableIndices(
         Tables.EventMap,
         allTableIndices,
         null,
         ( em, indices ) => em.UpdateMDTableWithTableIndices2( indices, e => e.Parent, ( e, p ) => e.Parent = p, e => e.EventList, ( e, l ) => e.EventList = l )
         );

      // No table indices in PropertyDefinition

      // Update PropertyMap
      md.PropertyMaps.UpdateMDTableIndices(
         Tables.PropertyMap,
         allTableIndices,
         null,
         ( pm, indices ) => pm.UpdateMDTableWithTableIndices2( indices, p => p.Parent, ( p, pp ) => p.Parent = pp, p => p.PropertyList, ( p, pl ) => p.PropertyList = pl )
         );

      // Sort InterfaceImpl table ( Class, Interface)
      md.InterfaceImplementations.UpdateMDTableIndices(
         Tables.InterfaceImpl,
         allTableIndices,
         Comparers.InterfaceImplementationComparer,
         ( iFaceImpl, indices ) => iFaceImpl.UpdateMDTableWithTableIndices2( indices, i => i.Class, ( i, c ) => i.Class = c, i => i.Interface, ( i, iface ) => i.Interface = iface )
         );

      // Sort ConstantDef table (Parent)
      md.ConstantDefinitions.UpdateMDTableIndices(
         Tables.Constant,
         allTableIndices,
         Comparers.ConstantDefinitionComparer,
         ( constant, indices ) => constant.UpdateMDTableWithTableIndices1( indices, c => c.Parent, ( c, p ) => c.Parent = p )
         );

      // Sort FieldMarshal table (Parent)
      md.FieldMarshals.UpdateMDTableIndices(
         Tables.FieldMarshal,
         allTableIndices,
         Comparers.FieldMarshalComparer,
         ( marshal, indices ) => marshal.UpdateMDTableWithTableIndices1( indices, f => f.Parent, ( f, p ) => f.Parent = p )
         );

      // Sort DeclSecurity table (Parent)
      md.SecurityDefinitions.UpdateMDTableIndices(
         Tables.DeclSecurity,
         allTableIndices,
         Comparers.SecurityDefinitionComparer,
         ( sec, indices ) => sec.UpdateMDTableWithTableIndices1( indices, s => s.Parent, ( s, p ) => s.Parent = p )
         );

      // Sort ClassLayout table (Parent)
      md.ClassLayouts.UpdateMDTableIndices(
         Tables.ClassLayout,
         allTableIndices,
         Comparers.ClassLayoutComparer,
         ( clazz, indices ) => clazz.UpdateMDTableWithTableIndices1( indices, c => c.Parent, ( c, p ) => c.Parent = p )
         );

      // Sort FieldLayout table (Field)
      md.FieldLayouts.UpdateMDTableIndices(
         Tables.FieldLayout,
         allTableIndices,
         Comparers.FieldLayoutComparer,
         ( fieldLayout, indices ) => fieldLayout.UpdateMDTableWithTableIndices1( indices, f => f.Field, ( f, p ) => f.Field = p )
         );

      // Sort MethodSemantics table (Association)
      md.MethodSemantics.UpdateMDTableIndices(
         Tables.MethodSemantics,
         allTableIndices,
         Comparers.MethodSemanticsComparer,
         ( semantics, indices ) => semantics.UpdateMDTableWithTableIndices2( indices, s => s.Method, ( s, m ) => s.Method = m, s => s.Associaton, ( s, a ) => s.Associaton = a )
         );

      // Sort MethodImpl table (Class)
      md.MethodImplementations.UpdateMDTableIndices(
         Tables.MethodImpl,
         allTableIndices,
         Comparers.MethodImplementationComparer,
         ( impl, indices ) => impl.UpdateMDTableWithTableIndices3( indices, i => i.Class, ( i, c ) => i.Class = c, i => i.MethodBody, ( i, b ) => i.MethodBody = b, i => i.MethodDeclaration, ( i, d ) => i.MethodDeclaration = d )
         );

      // Sort ImplMap table (MemberForwarded)
      md.MethodImplementationMaps.UpdateMDTableIndices(
         Tables.ImplMap,
         allTableIndices,
         Comparers.MethodImplementationMapComparer,
         ( map, indices ) => map.UpdateMDTableWithTableIndices2( indices, m => m.MemberForwarded, ( m, mem ) => m.MemberForwarded = mem, m => m.ImportScope, ( m, i ) => m.ImportScope = i )
         );

      // Sort FieldRVA table (Field)
      md.FieldRVAs.UpdateMDTableIndices(
         Tables.FieldRVA,
         allTableIndices,
         Comparers.FieldRVAComparer,
         ( fieldRVAs, indices ) => fieldRVAs.UpdateMDTableWithTableIndices1( indices, f => f.Field, ( f, field ) => f.Field = field )
         );

      // Sort GenericParamDef table (Owner, Sequence)
      md.GenericParameterDefinitions.UpdateMDTableIndices(
         Tables.GenericParameter,
         allTableIndices,
         Comparers.GenericParameterDefinitionComparer,
         ( gDef, indices ) => gDef.UpdateMDTableWithTableIndices1( indices, g => g.Owner, ( g, o ) => g.Owner = o )
         );

      // Sort GenericParameterConstraint table (Owner)
      md.GenericParameterConstraintDefinitions.UpdateMDTableIndices(
         Tables.GenericParameterConstraint,
         allTableIndices,
         Comparers.GenericParameterConstraintDefinitionComparer,
         ( gDef, indices ) => gDef.UpdateMDTableWithTableIndices2( indices, g => g.Owner, ( g, o ) => g.Owner = o, g => g.Constraint, ( g, c ) => g.Constraint = c )
         );

      // Update ExportedType
      md.ExportedTypes.UpdateMDTableIndices(
         Tables.ExportedType,
         allTableIndices,
         null,
         ( et, indices ) => et.UpdateMDTableWithTableIndices1( indices, e => e.Implementation, ( e, i ) => e.Implementation = i )
         );

      // Update ManifestResource
      md.ManifestResources.UpdateMDTableIndices(
         Tables.ManifestResource,
         allTableIndices,
         null,
         ( mr, indices ) => mr.UpdateMDTableWithTableIndices1Nullable( indices, m => m.Implementation, ( m, i ) => m.Implementation = i )
         );

      // Sort CustomAttributeDef table (Parent) 
      md.CustomAttributeDefinitions.UpdateMDTableIndices(
         Tables.CustomAttribute,
         allTableIndices,
         Comparers.CustomAttributeDefinitionComparer,
         ( ca, indices ) => ca.UpdateMDTableWithTableIndices2( indices, c => c.Parent, ( c, p ) => c.Parent = p, c => c.Type, ( c, t ) => c.Type = t )
         );
   }

   private static void RemoveDuplicatesAfterSorting( this CILMetaData md, MetaDataReOrderState reorderState )
   {
      foreach ( var kvp in reorderState.Duplicates )
      {
         var table = kvp.Key;
         var indices = kvp.Value;
         switch ( table )
         {
            case Tables.AssemblyRef:
               md.AssemblyReferences.RemoveDuplicatesFromTable( indices );
               break;
            case Tables.ModuleRef:
               md.ModuleReferences.RemoveDuplicatesFromTable( indices );
               break;
            case Tables.TypeSpec:
               md.TypeSpecifications.RemoveDuplicatesFromTable( indices );
               break;
            case Tables.TypeRef:
               md.TypeReferences.RemoveDuplicatesFromTable( indices );
               break;
            case Tables.MemberRef:
               md.MemberReferences.RemoveDuplicatesFromTable( indices );
               break;
            case Tables.MethodSpec:
               md.MethodSpecifications.RemoveDuplicatesFromTable( indices );
               break;
            case Tables.StandaloneSignature:
               md.StandaloneSignatures.RemoveDuplicatesFromTable( indices );
               break;
         }
      }
   }

   private static void RemoveDuplicatesFromTable<T>( this List<T> table, ISet<Int32> indices )
   {
      var max = table.Count;
      for ( Int32 curIdx = 0, originalIdx = 0; originalIdx < max; ++originalIdx )
      {
         if ( indices.Contains( originalIdx ) )
         {
            table.RemoveAt( curIdx );
         }
         else
         {
            ++curIdx;
         }
      }
   }

   private static void UpdateMDTableIndices<T>( this List<T> table, Tables thisTable, Int32[][] allTableIndices, IComparer<T> comparer, Action<List<T>, Int32[][]> tableUpdateCallback )
      where T : class
   {
      var thisTableIndices = allTableIndices[(Int32) thisTable];
      if ( thisTableIndices == null )
      {
         thisTableIndices = CreateIndexArray( table.Count );
         allTableIndices[(Int32) thisTable] = thisTableIndices;
      }

      tableUpdateCallback( table, allTableIndices );
      if ( comparer != null )
      {
         table.SortMDTable( thisTableIndices, comparer );
      }
   }

   private static void UpdateMDTableWithTableIndices1<T>( this List<T> table, Int32[][] tableIndices, Func<T, TableIndex> tableIndexGetter1, Action<T, TableIndex> tableIndexSetter1, Func<T, Int32, TableIndex, Boolean> rowAdditionalCheck = null )
      where T : class
   {
      for ( var i = 0; i < table.Count; ++i )
      {
         var row = table[i];
         row.ProcessSingleTableIndexToUpdate( i, tableIndices, tableIndexGetter1, tableIndexSetter1, rowAdditionalCheck );
      }
   }

   private static void UpdateMDTableWithTableIndices1Nullable<T>( this List<T> table, Int32[][] tableIndices, Func<T, TableIndex?> tableIndexGetter1, Action<T, TableIndex> tableIndexSetter1, Func<T, Int32, TableIndex, Boolean> rowAdditionalCheck = null )
      where T : class
   {
      for ( var i = 0; i < table.Count; ++i )
      {
         var row = table[i];
         row.ProcessSingleTableIndexToUpdateNullable( i, tableIndices, tableIndexGetter1, tableIndexSetter1, rowAdditionalCheck );
      }
   }

   private static void UpdateMDTableWithTableIndices2<T>( this List<T> table, Int32[][] tableIndices, Func<T, TableIndex> tableIndexGetter1, Action<T, TableIndex> tableIndexSetter1, Func<T, TableIndex> tableIndexGetter2, Action<T, TableIndex> tableIndexSetter2 )
      where T : class
   {
      for ( var i = 0; i < table.Count; ++i )
      {
         var row = table[i];
         row.ProcessSingleTableIndexToUpdate( i, tableIndices, tableIndexGetter1, tableIndexSetter1, null );
         row.ProcessSingleTableIndexToUpdate( i, tableIndices, tableIndexGetter2, tableIndexSetter2, null );
      }
   }

   private static void UpdateMDTableWithTableIndices3<T>( this List<T> table, Int32[][] tableIndices, Func<T, TableIndex> tableIndexGetter1, Action<T, TableIndex> tableIndexSetter1, Func<T, TableIndex> tableIndexGetter2, Action<T, TableIndex> tableIndexSetter2, Func<T, TableIndex> tableIndexGetter3, Action<T, TableIndex> tableIndexSetter3 )
      where T : class
   {
      for ( var i = 0; i < table.Count; ++i )
      {
         var row = table[i];
         row.ProcessSingleTableIndexToUpdate( i, tableIndices, tableIndexGetter1, tableIndexSetter1, null );
         row.ProcessSingleTableIndexToUpdate( i, tableIndices, tableIndexGetter2, tableIndexSetter2, null );
         row.ProcessSingleTableIndexToUpdate( i, tableIndices, tableIndexGetter3, tableIndexSetter3, null );
      }
   }

   private static void ProcessSingleTableIndexToUpdate<T>( this T row, Int32 rowIndex, Int32[][] tableIndices, Func<T, TableIndex> tableIndexGetter, Action<T, TableIndex> tableIndexSetter, Func<T, Int32, TableIndex, Boolean> rowAdditionalCheck )
      where T : class
   {
      if ( row != null )
      {
         row.ProcessSingleTableIndexToUpdateWithTableIndex( rowIndex, tableIndices, tableIndexGetter( row ), tableIndexSetter, rowAdditionalCheck );
      }
   }

   private static void ProcessSingleTableIndexToUpdateWithTableIndex<T>( this T row, Int32 rowIndex, Int32[][] tableIndices, TableIndex tableIndex, Action<T, TableIndex> tableIndexSetter, Func<T, Int32, TableIndex, Boolean> rowAdditionalCheck )
      where T : class
   {
      var table = tableIndex.Table;
      var newIndex = tableIndices[(Int32) table][tableIndex.Index];
      if ( newIndex != tableIndex.Index && ( rowAdditionalCheck == null || rowAdditionalCheck( row, rowIndex, tableIndex ) ) )
      {
         tableIndexSetter( row, new TableIndex( table, newIndex ) );
      }
   }

   private static void ProcessSingleTableIndexToUpdateNullable<T>( this T row, Int32 rowIndex, Int32[][] tableIndices, Func<T, TableIndex?> tableIndexGetter, Action<T, TableIndex> tableIndexSetter, Func<T, Int32, TableIndex, Boolean> rowAdditionalCheck )
      where T : class
   {
      if ( row != null )
      {
         var tIdx = tableIndexGetter( row );
         if ( tIdx.HasValue )
         {
            row.ProcessSingleTableIndexToUpdateWithTableIndex( rowIndex, tableIndices, tIdx.Value, tableIndexSetter, rowAdditionalCheck );
         }
      }
   }

   private static void SortMDTable<T>( this List<T> table, Int32[] indices, IComparer<T> comparer )
      where T : class
   {
      // If within 'indices' array, we have value '2' at index '0', it means that within the 'table', there should be value at index '0' which is currently at index '2'
      var count = table.Count;
      if ( count > 1 )
      {
         // 1. Make a copy of array
         var copy = table.ToArray();

         // 2. Sort original array
         table.Sort( comparer );

         // 3. For each element, do a binary search to find where it is now after sorting
         for ( var i = 0; i < count; ++i )
         {
            var idx = table.BinarySearchDeferredEqualityDetection( copy[i], comparer );
            while ( !ReferenceEquals( copy[i], table[idx] ) )
            {
               ++idx;
            }
            indices[i] = idx;
         }
      }



      //table.SortMDTableWithInt32Comparison( indices, ( x, y ) => comparer.Compare( table[x], table[y] ) );
   }

   //private static void SortMDTable<T>( this List<T> table, Int32[] indices, Comparison<T> comparison )
   //{
   //   table.SortMDTableWithInt32Comparison( indices, ( x, y ) => comparison( table[x], table[y] ) );
   //}

   // This was wrong. This gives us lookup: "given new index X, what is old index Y?", which is exactly opposite from what we want it to be.
   //private static void SortMDTableWithInt32Comparison<T>( this List<T> table, Int32[] indices, Comparison<Int32> comparison )
   //{
   //   // If within 'indices' array, we have value '2' at index '0', it means that within the 'table', there should be value at index '0' which is currently at index '2'
   //   var count = table.Count;
   //   if ( count > 1 )
   //   {
   //      // Sort in such way that we know how indices are shuffled
   //      Array.Sort( indices, ( x, y ) => comparison( x, y ) );

   //      // Reshuffle according to indices
   //      // List.ToArray() is close to constant time because of Array.Copy being close to constant time
   //      // The only loss is somewhat bigger memory allocation
   //      var copy = table.ToArray();
   //      for ( var i = 0; i < count; ++i )
   //      {
   //         table[indices[i]] = copy[i];
   //      }
   //   }
   //}

   private static void ReOrderMDTableWithAscendingReferences<T, U>( this List<T> table, Int32[] thisTableIndices, List<U> referencingTable, Int32[] referencingTableIndices, Func<U, Int32> referenceIndexGetter, Action<U, Int32> referenceIndexSetter, Func<U, Int32> referenceCountGetter )
   {
      var refTableCount = referencingTable.Count;
      var thisTableCount = table.Count;

      if ( thisTableCount > 0 )
      {
         var originalTable = table.ToArray();

         // Comments talk about typedefs and methoddefs but this method is generalized to handle any two tables with ascending reference pattern
         // This loop walks one typedef at a time, updating methoddef index and re-ordering methoddef array as needed
         for ( Int32 tIdx = 0, mIdx = 0; tIdx < refTableCount; ++tIdx )
         {
            var curTD = referencingTable[tIdx];

            // Inclusive min (the method where current typedef points to)
            var originalMin = referenceIndexGetter( curTD );

            // The count must be pre-calculated - we can't use typedef table to calculate that, as this for loop modifies the reference (e.g. MethodList property of TypeDefinition)
            var blockCount = referenceCountGetter( curTD );

            if ( blockCount > 0 )
            {
               var min = thisTableIndices[originalMin];

               for ( var i = 0; i < blockCount; ++i )
               {
                  var thisMethodIndex = mIdx + i;
                  var originalIndex = min + i;
                  table[thisMethodIndex] = originalTable[originalIndex];
                  thisTableIndices[originalIndex] = thisMethodIndex;
               }

               mIdx += blockCount;
            }

            // Set methoddef index for this typedef
            referenceIndexSetter( curTD, mIdx - blockCount );
         }
      }
   }

   //// Assumes list is sorted
   //private static Boolean CheckMDDuplicatesSorted<T>( List<T> list, Int32[] indices, Func<T, T, Boolean> duplicateComparer )
   //   where T : class
   //{
   //   var foundDuplicates = false;
   //   var count = list.Count;
   //   if ( count > 1 )
   //   {
   //      var prevNotNullIndex = 0;
   //      for ( var i = 1; i < count; ++i )
   //      {
   //         if ( duplicateComparer( list[i], list[prevNotNullIndex] ) )
   //         {
   //            if ( !foundDuplicates )
   //            {
   //               foundDuplicates = true;
   //            }

   //            list.AfterFindingDuplicate( indices, i, prevNotNullIndex );
   //         }
   //         else
   //         {
   //            prevNotNullIndex = i;
   //         }
   //      }
   //   }

   //   return foundDuplicates;
   //}

   private static Boolean CheckMDDuplicatesUnsorted<T>( this List<T> list, Int32[] indices, Tables table, MetaDataReOrderState reorderState, IEqualityComparer<T> comparer )
      where T : class
   {
      return list.CheckMDDuplicatesUnsorted( indices, table, reorderState, ( x, y ) => comparer.Equals( list[x], list[y] ), x => comparer.GetHashCode( list[x] ) );
   }

   private static Boolean CheckMDDuplicatesUnsorted<T>( this List<T> list, Int32[] indices, Tables table, MetaDataReOrderState reorderState, Func<Int32, Int32, Boolean> duplicateComparer, Func<Int32, Int32> hashCode )
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
               reorderState.MarkDuplicate( table, i );

               if ( !foundDuplicates )
               {
                  foundDuplicates = true;
               }

               var actualIndex = 0;
               while ( list[actualIndex] == null || !duplicateComparer( i, actualIndex ) )
               {
                  ++actualIndex;
               }

               // Mark as duplicate - replace value with null
               list[i] = null;

               list.AfterFindingDuplicate( indices, indices[i], indices[actualIndex] );
            }

         }
      }

      return foundDuplicates;
   }

   private static void AfterFindingDuplicate<T>( this List<T> list, Int32[] indices, Int32 current, Int32 prevNotNullIndex )
      where T : class
   {
      // Update index which point to this to point to previous instead
      for ( var j = 0; j < indices.Length; ++j )
      {
         if ( indices[j] == current )
         {
            indices[j] = prevNotNullIndex;
         }
         else if ( indices[j] > current )
         {
            --indices[j];
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

   private static void UpdateSignaturesAndILWhileRemovingDuplicates( this CILMetaData md, Int32[][] tableIndices, MetaDataReOrderState reorderState )
   {
      // Remove duplicates from AssemblyRef table (since reordering of the TypeRef table will require the indices in this table to be present)
      // ECMA-335: The AssemblyRef table shall contain no duplicates (where duplicate rows are deemd  to be those having the same MajorVersion, MinorVersion, BuildNumber, RevisionNumber, PublicKeyOrToken, Name, and Culture) [WARNING] 
      var aRefs = md.AssemblyReferences;
      var aRefIndices = CreateIndexArray( aRefs.Count );
      aRefs.CheckMDDuplicatesUnsorted( aRefIndices, Tables.AssemblyRef, reorderState, ( x, y ) =>
      {
         var xRef = aRefs[x];
         var yRef = aRefs[y];
         return xRef.AssemblyInformation.Equals( yRef.AssemblyInformation );
      }, x => aRefs[x].AssemblyInformation.GetHashCode() );

      // Remove duplicates from ModuleRef table (since reordering of the TypeRef table will require the indices in this table to be present)
      // ECMA-335: There should be no duplicate rows  [WARNING] 
      var mRefs = md.ModuleReferences;
      var mRefIndices = CreateIndexArray( mRefs.Count );
      mRefs.CheckMDDuplicatesUnsorted( mRefIndices, Tables.ModuleRef, reorderState, ( x, y ) => String.Equals( mRefs[x].ModuleName, mRefs[y].ModuleName ), x => mRefs[x].ModuleName.GetHashCodeSafe() );

      // ECMA-335: IL tokens shall be from TypeDef, TypeRef, TypeSpec, MethodDef, FieldDef, MemberRef, MethodSpec or StandaloneSignature tables.
      // All table indices in signatures should only ever reference TypeDef, TypeRef or TypeSpec tables.
      // Tables with signatures are: FieldDef, MethodDef, MemberRef, CustomAttribute, DeclSecurity, StandaloneSignature, PropertyDef, TypeSpecification, MethodSpecification
      // The ones that have rules for duplicates: MemberRef, StandaloneSignature, PropertyDef, TypeSpecification, MethodSpecification
      // We will not handle PropertyDef (nor EventDef) duplicates
      // The ones without duplicates will be handled later (those are CustomAttribute, DeclSecurity)
      tableIndices[(Int32) Tables.TypeRef] = CreateIndexArray( md.TypeReferences.Count );
      tableIndices[(Int32) Tables.TypeSpec] = CreateIndexArray( md.TypeSpecifications.Count );
      tableIndices[(Int32) Tables.MemberRef] = CreateIndexArray( md.MemberReferences.Count );
      tableIndices[(Int32) Tables.MethodSpec] = CreateIndexArray( md.MethodSpecifications.Count );
      tableIndices[(Int32) Tables.StandaloneSignature] = CreateIndexArray( md.StandaloneSignatures.Count );
      tableIndices[(Int32) Tables.AssemblyRef] = aRefIndices;
      tableIndices[(Int32) Tables.ModuleRef] = mRefIndices;
      tableIndices[(Int32) Tables.Module] = CreateIndexArray( md.ModuleDefinitions.Count );

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
         var startingDuplicates = new HashSet<TableIndex>( reorderState.Duplicates.SelectMany( kvp => kvp.Value.Select( i => new TableIndex( kvp.Key, i ) ) ) );

         // ECMA-335:  There shall be no duplicate rows, where a duplicate has the same ResolutionScope, TypeName and TypeNamespace  [ERROR] 
         tRefs.UpdateMDTableWithTableIndices1Nullable(
            tableIndices,
            tRef => tRef.ResolutionScope,
            ( tRef, resScope ) => tRef.ResolutionScope = resScope,
            ( tRef, tRefIdx, resScope ) => updateState.CheckRowTableIndexWhenHandlingSignatures( resScope, new TableIndex( Tables.TypeRef, tRefIdx ) )
            );
         removedTypeRefDuplicates = tRefs.CheckMDDuplicatesUnsorted( tableIndices[(Int32) Tables.TypeRef], Tables.TypeRef, reorderState, Comparers.TypeReferenceEqualityComparer );

         // ECMA-335: There shall be no duplicate rows, based upon Signature  [ERROR] 
         removedTSpecDuplicates = tSpecs.CheckMDDuplicatesUnsorted( tableIndices[(Int32) Tables.TypeSpec], Tables.TypeSpec, reorderState, Comparers.TypeSpecificationEqualityComparer );

         // ECMA-335:  The MemberRef table shall contain no duplicates, where duplicate rows have the same Class, Name, and Signature  [WARNING] 
         memberRefs.UpdateMDTableWithTableIndices1(
            tableIndices,
            mRef => mRef.DeclaringType,
            ( mRef, dType ) => mRef.DeclaringType = dType,
            ( mRef, mRefIdx, dType ) => updateState.CheckRowTableIndexWhenHandlingSignatures( dType, new TableIndex( Tables.MemberRef, mRefIdx ) )
            );
         removedMemberRefDuplicates = memberRefs.CheckMDDuplicatesUnsorted( tableIndices[(Int32) Tables.MemberRef], Tables.MemberRef, reorderState, Comparers.MemberReferenceEqualityComparer );

         // ECMA-335: There shall be no duplicate rows based upon Method+Instantiation  [ERROR] 
         mSpecs.UpdateMDTableWithTableIndices1(
            tableIndices,
            mSpec => mSpec.Method,
            ( mSpec, method ) => mSpec.Method = method,
            ( mSpec, mSpecIdx, method ) => updateState.CheckRowTableIndexWhenHandlingSignatures( method, new TableIndex( Tables.MethodSpec, mSpecIdx ) )
            );
         removedMethodSpecDuplicates = mSpecs.CheckMDDuplicatesUnsorted( tableIndices[(Int32) Tables.MethodSpec], Tables.MethodSpec, reorderState, Comparers.MethodSpecificationEqualityComparer );

         // ECMA-335: Duplicates allowed (but we will make them all unique anyway)
         removedStandaloneSigDuplicates = standAloneSigs.CheckMDDuplicatesUnsorted( tableIndices[(Int32) Tables.StandaloneSignature], Tables.StandaloneSignature, reorderState, Comparers.StandaloneSignatureEqualityComparer );

         // Calculate the duplicates
         startingDuplicates.SymmetricExceptWith( reorderState.Duplicates.SelectMany( kvp => kvp.Value.Select( i => new TableIndex( kvp.Key, i ) ) ) );
         updateState.DuplicatesThisRound.Clear();
         updateState.DuplicatesThisRound.UnionWith( startingDuplicates );

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

   private static Boolean SignatureOrILTableIndexDiffersNullable( this SignatureReorderState state, TableIndex? index, Object parent, out Int32 newIndex )
   {
      var retVal = index.HasValue;
      if ( retVal )
      {
         retVal = state.SignatureOrILTableIndexDiffers( index.Value, parent, out newIndex );
      }
      else
      {
         newIndex = -1;
      }
      return retVal;
   }

   private static Boolean SignatureOrILTableIndexDiffers( this SignatureReorderState state, TableIndex index, Object parent, out Int32 newIndex )
   {
      var retVal = state.CheckRowTableIndexWhenHandlingSignatures( index, parent );
      if ( retVal )
      {
         var oldIndex = index.Index;
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

   private static Boolean CheckRowTableIndexWhenHandlingSignatures( this SignatureReorderState state, TableIndex index, Object parent )
   {
      var visitedInfo = state.VisitedInfo;
      var notVisited = !visitedInfo.ContainsKey( parent );// Check whether we have already visited this index
      // Even if we have already visited this, have to check here whether target is null
      // If target is null, that means that after visiting, the target became duplicate and was removed
      // So we need to update it again
      var retVal = notVisited || state.DuplicatesThisRound.Contains( index );
      if ( notVisited )
      {
         visitedInfo.Add( parent, parent );
      }

      return retVal;
   }

   private static IEnumerable<Int32> GetDeclaringTypeChain( this Int32 typeDefIndex, IList<NestedClassDefinition> sortedNestedClass )
   {

      return typeDefIndex.AsSingleBranchEnumerable( cur =>
         {
            var nIdx = sortedNestedClass.FindRowFromSortedNestedClass( cur );
            if ( nIdx != -1 )
            {
               nIdx = sortedNestedClass[nIdx].EnclosingClass.Index;
            }
            return nIdx;
         }, cur => cur == -1 || cur == typeDefIndex, false ); // Stop also when we hit the same index again (illegal situation but possible), and don't include itself
   }

   private static Int32 FindRowFromSortedNestedClass( this IList<NestedClassDefinition> sortedNestedClass, Int32 currentTypeDefIndex )
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
