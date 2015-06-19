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
      public TypeDefinition()
      {
         this.FieldList = new TableIndex( Tables.Field, 0 );
         this.MethodList = new TableIndex( Tables.MethodDef, 0 );
      }

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
      public MethodDefinition()
      {
         this.ParameterList = new TableIndex( Tables.Parameter, 0 );
      }

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
      public InterfaceImplementation()
      {
         this.Class = new TableIndex( Tables.TypeDef, 0 );
      }

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
      public ClassLayout()
      {
         this.Parent = new TableIndex( Tables.TypeDef, 0 );
      }

      public Int16 PackingSize { get; set; }
      public Int32 ClassSize { get; set; }
      public TableIndex Parent { get; set; }
   }

   public sealed class FieldLayout
   {
      public FieldLayout()
      {
         this.Field = new TableIndex( Tables.Field, 0 );
      }

      public Int32 Offset { get; set; }
      public TableIndex Field { get; set; }
   }

   public sealed class StandaloneSignature
   {
      public AbstractSignature Signature { get; set; }

      // From https://social.msdn.microsoft.com/Forums/en-US/b4252eab-7aae-4456-9829-2707c8459e13/pinned-fields-in-the-common-language-runtime?forum=netfxtoolsdev
      // After messing around further, and noticing that even the C# compiler emits Field signatures in the StandAloneSig table, the signatures seem to relate to PDB debugging symbols.
      // When you emit symbols with the Debug or Release versions of your code, I'm guessing a StandAloneSig entry is injected and referred to by the PDB file.
      // If you are in release mode and you generate no PDB info, the StandAloneSig table contains no Field signatures.
      // One such condition for the emission of such information is constants within the scope of a method body.
      // Original thread:  http://www.netframeworkdev.com/building-development-diagnostic-tools-for-net/field-signatures-in-standalonesig-table-30658.shtml
      public Boolean StoreSignatureAsFieldSignature { get; set; }

   }

   public sealed class EventMap
   {
      public EventMap()
      {
         this.Parent = new TableIndex( Tables.TypeDef, 0 );
         this.EventList = new TableIndex( Tables.Event, 0 );
      }

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
      public PropertyMap()
      {
         this.Parent = new TableIndex( Tables.TypeDef, 0 );
         this.PropertyList = new TableIndex( Tables.Property, 0 );
      }

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
      public MethodSemantics()
      {
         this.Method = new TableIndex( Tables.MethodDef, 0 );
      }

      public MethodSemanticsAttributes Attributes { get; set; }
      public TableIndex Method { get; set; }
      public TableIndex Associaton { get; set; }
   }

   public sealed class MethodImplementation
   {
      public MethodImplementation()
      {
         this.Class = new TableIndex( Tables.TypeDef, 0 );
      }

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
      public MethodImplementationMap()
      {
         this.ImportScope = new TableIndex( Tables.ModuleRef, 0 );
      }

      public PInvokeAttributes Attributes { get; set; }
      public TableIndex MemberForwarded { get; set; }
      public String ImportName { get; set; }
      public TableIndex ImportScope { get; set; }
   }

   public sealed class FieldRVA
   {
      public FieldRVA()
      {
         this.Field = new TableIndex( Tables.Field, 0 );
      }

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

      public override String ToString()
      {
         return this.AssemblyInformation.ToString( true, true );
      }
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

      public override String ToString()
      {
         return this.AssemblyInformation.ToString( true, this.Attributes.IsFullPublicKey() );
      }
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
      /// <summary>
      /// This value is interpreted as unsigned 4-byte integer.
      /// </summary>
      public Int32 Offset { get; set; }
      public ManifestResourceAttributes Attributes { get; set; }
      public String Name { get; set; }
      public TableIndex? Implementation { get; set; }

      // This will be used only if Implementation is null
      public Byte[] DataInCurrentFile { get; set; }
   }

   public sealed class NestedClassDefinition
   {
      public NestedClassDefinition()
      {
         this.NestedClass = new TableIndex( Tables.TypeDef, 0 );
         this.EnclosingClass = new TableIndex( Tables.TypeDef, 0 );
      }

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
      public GenericParameterConstraintDefinition()
      {
         this.Owner = new TableIndex( Tables.GenericParameter, 0 );
      }

      public TableIndex Owner { get; set; }
      public TableIndex Constraint { get; set; }
   }

   public struct TableIndex : IEquatable<TableIndex>, IComparable<TableIndex>, IComparable
   {
      private readonly Int32 _token;

      // index is zero-based
      public TableIndex( Tables aTable, Int32 anIdx )
         : this( ( (Int32) aTable << 24 ) | anIdx )
      {
      }

      internal TableIndex( Int32 token )
      {
         this._token = token;
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

      public Int32 ZeroBasedToken
      {
         get
         {
            return this._token;
         }
      }

      public Int32 OneBasedToken
      {
         get
         {
            return ( ( this._token & TokenUtils.INDEX_MASK ) + 1 ) | ( this._token & ~TokenUtils.INDEX_MASK );
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

      public static TableIndex? FromOneBasedTokenNullable( Int32 token )
      {
         return token == 0 ?
            (TableIndex?) null :
            FromOneBasedToken( token );
      }

      public static TableIndex FromOneBasedToken( Int32 token )
      {
         return new TableIndex( ( ( token & TokenUtils.INDEX_MASK ) - 1 ) | ( token & ~TokenUtils.INDEX_MASK ) );
      }

      public static TableIndex FromZeroBasedToken( Int32 token )
      {
         return new TableIndex( token );
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

   // System.Runtime.Versioning.FrameworkName is amazingly missing from all PCL framework assemblies.
   public sealed class TargetFrameworkInfo
   {
      private readonly String _fwName;
      private readonly String _fwVersion;
      private readonly String _fwProfile;
      private readonly Boolean _assemblyRefsRetargetable;

      public TargetFrameworkInfo( String name, String version, String profile )
      {
         this._fwName = name;
         this._fwVersion = version;
         this._fwProfile = profile;
         // TODO better
         this._assemblyRefsRetargetable = String.Equals( this._fwName, ".NETPortable" );
      }

      public String Identifier
      {
         get
         {
            return this._fwName;
         }
      }

      public String Version
      {
         get
         {
            return this._fwVersion;
         }
      }

      public String Profile
      {
         get
         {
            return this._fwProfile;
         }
      }

      public Boolean AreFrameworkAssemblyReferencesRetargetable
      {
         get
         {
            return this._assemblyRefsRetargetable;
         }
      }


      private const String PROFILE_PREFIX = "Profile=";
      private const String VERSION_PREFIX = "Version=";
      private const Char SEPARATOR = ',';

      public static Boolean TryParse( String str, out TargetFrameworkInfo fwInfo )
      {
         var retVal = !String.IsNullOrEmpty( str );
         if ( retVal )
         {
            // First, framework name
            var idx = str.IndexOf( SEPARATOR );
            var fwName = idx == -1 ? str : str.Substring( 0, idx );

            String fwVersion = null, fwProfile = null;
            if ( idx > 0 )
            {

               // Then, framework version
               idx = str.IndexOf( VERSION_PREFIX, idx, StringComparison.Ordinal );
               var nextIdx = idx + VERSION_PREFIX.Length;
               var endIdx = str.IndexOf( SEPARATOR, nextIdx );
               if ( endIdx == -1 )
               {
                  endIdx = str.Length;
               }
               fwVersion = idx != -1 && nextIdx < str.Length ? str.Substring( nextIdx, endIdx - nextIdx ) : null;

               // Then, profile
               if ( idx > 0 )
               {
                  idx = str.IndexOf( PROFILE_PREFIX, idx, StringComparison.Ordinal );
                  nextIdx = idx + PROFILE_PREFIX.Length;
                  endIdx = str.IndexOf( SEPARATOR, nextIdx );
                  if ( endIdx == -1 )
                  {
                     endIdx = str.Length;
                  }
                  fwProfile = idx != -1 && nextIdx < str.Length ? str.Substring( nextIdx, endIdx - nextIdx ) : null;
               }
            }

            fwInfo = new TargetFrameworkInfo( fwName, fwVersion, fwProfile );
         }
         else
         {
            fwInfo = null;
         }

         return retVal;
      }
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

   private sealed class SignatureReOrderState
   {
      private readonly MetaDataReOrderState _reorderState;
      private readonly Tables _table;
      private Int32 _currentMinDuplicate;
      private Int32[] _prevDuplicates;

      internal SignatureReOrderState( MetaDataReOrderState reorderState, Tables table )
      {
         this._reorderState = reorderState;
         this._table = table;
         this._currentMinDuplicate = -1;
      }

      public MetaDataReOrderState ReOrderState
      {
         get
         {
            return this._reorderState;
         }
      }

      public CILMetaData MD
      {
         get
         {
            return this._reorderState.MetaData;
         }
      }

      public void BeforeDuplicateRemoval()
      {
         if ( this._currentMinDuplicate >= 0 )
         {
            this._prevDuplicates = this._reorderState.Duplicates[this._table].Keys.ToArray();
         }
      }

      public void AfterDuplicateRemoval()
      {
         var duplicates = new HashSet<Int32>( this._reorderState.Duplicates[this._table].Keys );
         if ( this._prevDuplicates != null )
         {
            duplicates.ExceptWith( this._prevDuplicates );
         }
         this._currentMinDuplicate = duplicates.Min();
      }

      public Boolean ShouldProcess( TableIndex tIdx )
      {
         return this._currentMinDuplicate == -1 || ( this._table == tIdx.Table && this._currentMinDuplicate <= tIdx.Index );
      }

   }

   private sealed class MetaDataReOrderState
   {
      private readonly CILMetaData _md;
      private readonly IDictionary<Tables, IDictionary<Int32, Int32>> _duplicates;
      private readonly Int32[][] _finalIndices;

      internal MetaDataReOrderState( CILMetaData md )
      {
         this._md = md;
         this._duplicates = new Dictionary<Tables, IDictionary<Int32, Int32>>();
         this._finalIndices = new Int32[Consts.AMOUNT_OF_TABLES][];
      }

      public CILMetaData MetaData
      {
         get
         {
            return this._md;
         }
      }

      public IDictionary<Tables, IDictionary<Int32, Int32>> Duplicates
      {
         get
         {
            return this._duplicates;
         }
      }

      public Int32[][] FinalIndices
      {
         get
         {
            return this._finalIndices;
         }
      }

      public void MarkDuplicate( Tables table, Int32 idx, Int32 actualIndex )
      {
         var thisDuplicates = this._duplicates
            .GetOrAdd_NotThreadSafe( table, t => new Dictionary<Int32, Int32>() );
         thisDuplicates
            .Add( idx, actualIndex );
         // Update all other duplicates as well
         foreach ( var kvp in thisDuplicates.ToArray() )
         {
            if ( kvp.Value == idx )
            {
               thisDuplicates.Remove( kvp.Key );
               thisDuplicates.Add( kvp.Key, actualIndex );
            }
         }
      }

      public Boolean IsDuplicate( TableIndex index )
      {
         IDictionary<Int32, Int32> tableDuplicates;
         return this._duplicates.TryGetValue( index.Table, out tableDuplicates )
            && tableDuplicates.ContainsKey( index.Index );
      }

      public Boolean IsDuplicate( TableIndex index, out Int32 newIndex )
      {
         IDictionary<Int32, Int32> tableDuplicates;
         newIndex = -1;
         var retVal = this._duplicates.TryGetValue( index.Table, out tableDuplicates )
            && tableDuplicates.TryGetValue( index.Index, out newIndex );
         if ( !retVal )
         {
            newIndex = -1;
         }
         return retVal;
      }

      public Int32[] GetOrCreateIndexArray<T>( MetaDataTable<T> table )
         where T : class
      {
         var tIdx = (Int32) table.TableKind;
         var retVal = this._finalIndices[tIdx];
         if ( retVal == null )
         {
            var list = table.TableContents;
            retVal = new Int32[list.Count];
            for ( var i = 0; i < retVal.Length; ++i )
            {
               retVal[i] = i;
            }
            this._finalIndices[tIdx] = retVal;
         }
         return retVal;
      }

      public Int32 GetFinalIndex( TableIndex index )
      {
         return this._finalIndices[(Int32) index.Table][index.Index];
      }

      public Int32 GetFinalIndex( Tables table, Int32 index )
      {
         return this._finalIndices[(Int32) table][index];
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
            var list = md.StandaloneSignatures.TableContents;
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
      return md.GetTypeMethodIndices( typeDefIndex ).Select( idx => md.MethodDefinitions.TableContents[idx] );
   }

   public static IEnumerable<FieldDefinition> GetTypeFields( this CILMetaData md, Int32 typeDefIndex )
   {
      return md.GetTypeFieldIndices( typeDefIndex ).Select( idx => md.FieldDefinitions.TableContents[idx] );
   }

   public static IEnumerable<ParameterDefinition> GetMethodParameters( this CILMetaData md, Int32 methodDefIndex )
   {
      return md.GetMethodParameterIndices( methodDefIndex ).Select( idx => md.ParameterDefinitions.TableContents[idx] );
   }

   public static IEnumerable<Int32> GetTypeMethodIndices( this CILMetaData md, Int32 typeDefIndex )
   {
      return md.TypeDefinitions.GetTargetIndicesForAscendingReferenceListTable( md.MethodDefinitions.RowCount, typeDefIndex, td => td.MethodList.Index );
   }

   public static IEnumerable<Int32> GetTypeFieldIndices( this CILMetaData md, Int32 typeDefIndex )
   {
      return md.TypeDefinitions.GetTargetIndicesForAscendingReferenceListTable( md.FieldDefinitions.RowCount, typeDefIndex, td => td.FieldList.Index );
   }

   public static IEnumerable<Int32> GetMethodParameterIndices( this CILMetaData md, Int32 methodDefIndex )
   {
      return md.MethodDefinitions.GetTargetIndicesForAscendingReferenceListTable( md.ParameterDefinitions.RowCount, methodDefIndex, mdef => mdef.ParameterList.Index );
   }

   internal static IEnumerable<Int32> GetTargetIndicesForAscendingReferenceListTable<T>( this MetaDataTable<T> mdTableWithReferences, Int32 targetTableCount, Int32 tableWithReferencesIndex, Func<T, Int32> referenceExtractor )
      where T : class
   {
      var tableWithReferences = mdTableWithReferences.TableContents;
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

   public static Boolean TryGetByTable( this CILMetaData md, Tables tableKind, out MetaDataTable table )
   {
      switch ( tableKind )
      {
         case Tables.Module:
            table = md.ModuleDefinitions;
            break;
         case Tables.TypeRef:
            table = md.TypeReferences;
            break;
         case Tables.TypeDef:
            table = md.TypeDefinitions;
            break;
         case Tables.Field:
            table = md.FieldDefinitions;
            break;
         case Tables.MethodDef:
            table = md.MethodDefinitions;
            break;
         case Tables.Parameter:
            table = md.ParameterDefinitions;
            break;
         case Tables.InterfaceImpl:
            table = md.InterfaceImplementations;
            break;
         case Tables.MemberRef:
            table = md.MemberReferences;
            break;
         case Tables.Constant:
            table = md.ConstantDefinitions;
            break;
         case Tables.CustomAttribute:
            table = md.CustomAttributeDefinitions;
            break;
         case Tables.FieldMarshal:
            table = md.FieldMarshals;
            break;
         case Tables.DeclSecurity:
            table = md.SecurityDefinitions;
            break;
         case Tables.ClassLayout:
            table = md.ClassLayouts;
            break;
         case Tables.FieldLayout:
            table = md.FieldLayouts;
            break;
         case Tables.StandaloneSignature:
            table = md.StandaloneSignatures;
            break;
         case Tables.EventMap:
            table = md.EventMaps;
            break;
         case Tables.Event:
            table = md.EventDefinitions;
            break;
         case Tables.PropertyMap:
            table = md.PropertyMaps;
            break;
         case Tables.Property:
            table = md.PropertyDefinitions;
            break;
         case Tables.MethodSemantics:
            table = md.MethodSemantics;
            break;
         case Tables.MethodImpl:
            table = md.MethodImplementations;
            break;
         case Tables.ModuleRef:
            table = md.ModuleReferences;
            break;
         case Tables.TypeSpec:
            table = md.TypeSpecifications;
            break;
         case Tables.ImplMap:
            table = md.MethodImplementationMaps;
            break;
         case Tables.FieldRVA:
            table = md.FieldRVAs;
            break;
         case Tables.Assembly:
            table = md.AssemblyDefinitions;
            break;
         case Tables.AssemblyRef:
            table = md.AssemblyReferences;
            break;
         case Tables.File:
            table = md.FieldDefinitions;
            break;
         case Tables.ExportedType:
            table = md.ExportedTypes;
            break;
         case Tables.ManifestResource:
            table = md.ManifestResources;
            break;
         case Tables.NestedClass:
            table = md.NestedClassDefinitions;
            break;
         case Tables.GenericParameter:
            table = md.GenericParameterDefinitions;
            break;
         case Tables.MethodSpec:
            table = md.MethodSpecifications;
            break;
         case Tables.GenericParameterConstraint:
            table = md.GenericParameterConstraintDefinitions;
            break;
         //case Tables.FieldPtr:
         //case Tables.MethodPtr:
         //case Tables.ParameterPtr:
         //case Tables.EventPtr:
         //case Tables.PropertyPtr:
         //case Tables.EncLog:
         //case Tables.EncMap:
         //case Tables.AssemblyProcessor:
         //case Tables.AssemblyOS:
         //case Tables.AssemblyRefProcessor:
         //case Tables.AssemblyRefOS:
         default:
            table = null;
            break;
      }


      return table != null;
   }

   public static Boolean TryGetByTableIndex( this CILMetaData md, TableIndex index, out Object row )
   {
      MetaDataTable table;
      var retVal = md.TryGetByTable( index.Table, out table ) && index.Index <= table.RowCount;
      row = retVal ? table[index.Index] : null;
      return retVal;
   }

   // Assumes that all lists of CILMetaData have only non-null elements.
   // TypeDef and MethodDef can not have duplicate instances of same object!!
   // Assumes that MethodList, FieldList indices in TypeDef and ParameterList in MethodDef are all ordered correctly.
   // TODO check that everything works even though <Module> class is not a first row in TypeDef table
   // Duplicates *not* checked from the following tables:
   // TypeDef
   // MethodDef
   // FieldDef
   // PropertyDef
   // EventDef
   // ExportedType


   public static Int32[][] OrderTablesAndRemoveDuplicates( this CILMetaData md )
   {
      // TODO maybe just create a new CILMetaData which would be a sorted version of this??
      // Would simplify a lot of things, and possibly could be even faster (unless given md is already in order)


      //var allTableIndices = new Int32[Consts.AMOUNT_OF_TABLES][];
      var reorderState = new MetaDataReOrderState( md );

      // Start by re-ordering structural (TypeDef, MethodDef, ParamDef, Field, NestedClass) tables
      reorderState.ReOrderStructuralTables();

      // Keep updating and removing duplicates from TypeRef, TypeSpec, MemberRef, MethodSpec, StandaloneSignature and Property tables, while updating all signatures and IL code
      reorderState.UpdateSignaturesAndILWhileRemovingDuplicates();

      // Update and sort the remaining tables which don't have signatures
      reorderState.UpdateAndSortTablesWithNoSignatures();

      // Remove duplicates
      reorderState.RemoveDuplicatesAfterSorting();

      // Sort exception blocks of all ILs
      md.SortMethodILExceptionBlocks();

      return reorderState.FinalIndices;
   }

   // Re-orders TypeDef, MethodDef, ParamDef, Field, and NestedClass tables, if necessary
   private static void ReOrderStructuralTables( this MetaDataReOrderState reorderState )
   {
      var md = reorderState.MetaData;
      // No matter what, we have to remove nested class duplicates
      // Don't need to keep track of changes - nested class table is not referenced by anything
      md.NestedClassDefinitions.RemoveDuplicatesUnsortedInPlace( Comparers.NestedClassDefinitionEqualityComparer );

      var typeDef = md.TypeDefinitions;
      var methodDef = md.MethodDefinitions;
      var fieldDef = md.FieldDefinitions;
      var paramDef = md.ParameterDefinitions;
      var nestedClass = md.NestedClassDefinitions;
      var tDefCount = typeDef.RowCount;
      var mDefCount = methodDef.RowCount;
      var fDefCount = fieldDef.RowCount;
      var pDefCount = paramDef.RowCount;
      var ncCount = nestedClass.RowCount;

      var typeDefIndices = reorderState.GetOrCreateIndexArray( typeDef );
      var methodDefIndices = reorderState.GetOrCreateIndexArray( methodDef );
      var paramDefIndices = reorderState.GetOrCreateIndexArray( paramDef );
      var fDefIndices = reorderState.GetOrCreateIndexArray( fieldDef );
      var ncIndices = reorderState.GetOrCreateIndexArray( nestedClass );


      // So, start by reading nested class data into more easily accessible data structure

      // TypeDef table has special constraint - enclosing class must precede nested class.
      // In other words, for all rows in NestedClass table, the EnclosingClass index must be less than NestedClass index
      // All the tables that are handled in this method will only be needed to re-shuffle if TypeDef table changes, that is, if there are violating rows in NestedClass table.
      var typeDefOrderingChanged = nestedClass.TableContents.Any( nc => nc.NestedClass.Index < nc.EnclosingClass.Index );

      if ( typeDefOrderingChanged )
      {
         // We have to pre-calculate method and field counts for types
         // We have to do this BEFORE typedef table is re-ordered
         var methodAndFieldCounts = new Dictionary<TypeDefinition, KeyValuePair<Int32, Int32>>( tDefCount, ReferenceEqualityComparer<TypeDefinition>.ReferenceBasedComparer );
         var typeDefL = typeDef.TableContents;
         for ( var i = 0; i < tDefCount; ++i )
         {
            var curTD = typeDefL[i];
            Int32 mMax, fMax;
            if ( i + 1 < tDefCount )
            {
               var nextTD = typeDefL[i + 1];
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
         var mDefL = methodDef.TableContents;
         for ( var i = 0; i < mDefCount; ++i )
         {
            var curMD = mDefL[i];
            Int32 max;
            if ( i + 1 < mDefCount )
            {
               max = mDefL[i + 1].ParameterList.Index;
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
         foreach ( var nc in nestedClass.TableContents )
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
         var tDefCopy = typeDefL.ToArray();
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
               typeDefL[i] = tDefCopy[tDefCopyIdx];
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
                  typeDefL[++i] = tDefCopy[nested];
                  typeDefIndices[nested] = i;
               }
            }
         }

         // Update NestedClass indices and sort NestedClass
         reorderState.UpdateMDTableWithTableIndices2(
            md.NestedClassDefinitions,
            nc => nc.NestedClass,
            ( nc, ncIdx ) => nc.NestedClass = ncIdx,
            nc => nc.EnclosingClass,
            ( nc, ecIdx ) => nc.EnclosingClass = ecIdx
            );
         nestedClass.SortMDTable( ncIndices, Comparers.NestedClassDefinitionComparer );

         // Sort MethodDef table and update references in TypeDef table
         reorderState.ReOrderMDTableWithAscendingReferences(
            methodDef,
            methodDefIndices,
            typeDef,
            typeDefIndices,
            td => td.MethodList.Index,
            ( td, mIdx ) => td.MethodList = new TableIndex( Tables.MethodDef, mIdx ),
            tdIdx => methodAndFieldCounts[tdIdx].Key
            );

         // Sort ParameterDef table and update references in MethodDef table
         reorderState.ReOrderMDTableWithAscendingReferences(
            paramDef,
            paramDefIndices,
            methodDef,
            methodDefIndices,
            mDef => mDef.ParameterList.Index,
            ( mDef, pIdx ) => mDef.ParameterList = new TableIndex( Tables.Parameter, pIdx ),
            mdIdx => paramCounts[mdIdx]
            );

         // Sort FieldDef table and update references in TypeDef table
         reorderState.ReOrderMDTableWithAscendingReferences(
            fieldDef,
            fDefIndices,
            typeDef,
            typeDefIndices,
            td => td.FieldList.Index,
            ( td, fIdx ) => td.FieldList = new TableIndex( Tables.Field, fIdx ),
            tdIdx => methodAndFieldCounts[tdIdx].Value
            );
      }
   }

   private static void UpdateAndSortTablesWithNoSignatures( this MetaDataReOrderState reorderState )
   {
      var md = reorderState.MetaData;
      // Create table index arrays for tables which are untouched (but can be used by various table indices in table rows)
      reorderState.GetOrCreateIndexArray( md.AssemblyDefinitions );
      reorderState.GetOrCreateIndexArray( md.FileReferences );
      reorderState.GetOrCreateIndexArray( md.PropertyDefinitions );

      // Update TypeDef
      reorderState.UpdateMDTableIndices(
         md.TypeDefinitions,
         null,
         ( td, indices ) => reorderState.UpdateMDTableWithTableIndices1Nullable( td, t => t.BaseType, ( t, b ) => t.BaseType = b )
         );

      // Update EventDefinition
      reorderState.UpdateMDTableIndices(
         md.EventDefinitions,
         null,
         ( ed, indices ) => reorderState.UpdateMDTableWithTableIndices1( ed, e => e.EventType, ( e, t ) => e.EventType = t )
         );

      // Update EventMap
      reorderState.UpdateMDTableIndices(
         md.EventMaps,
         null,
         ( em, indices ) => reorderState.UpdateMDTableWithTableIndices2( em, e => e.Parent, ( e, p ) => e.Parent = p, e => e.EventList, ( e, l ) => e.EventList = l )
         );

      // No table indices in PropertyDefinition

      // Update PropertyMap
      reorderState.UpdateMDTableIndices(
         md.PropertyMaps,
         null,
         ( pm, indices ) => reorderState.UpdateMDTableWithTableIndices2( pm, p => p.Parent, ( p, pp ) => p.Parent = pp, p => p.PropertyList, ( p, pl ) => p.PropertyList = pl )
         );

      // Sort InterfaceImpl table ( Class, Interface)
      reorderState.UpdateMDTableIndices(
         md.InterfaceImplementations,
         Comparers.InterfaceImplementationComparer,
         ( iFaceImpl, indices ) => reorderState.UpdateMDTableWithTableIndices2( iFaceImpl, i => i.Class, ( i, c ) => i.Class = c, i => i.Interface, ( i, iface ) => i.Interface = iface )
         );

      // Sort ConstantDef table (Parent)
      reorderState.UpdateMDTableIndices(
         md.ConstantDefinitions,
         Comparers.ConstantDefinitionComparer,
         ( constant, indices ) => reorderState.UpdateMDTableWithTableIndices1( constant, c => c.Parent, ( c, p ) => c.Parent = p )
         );

      // Sort FieldMarshal table (Parent)
      reorderState.UpdateMDTableIndices(
         md.FieldMarshals,
         Comparers.FieldMarshalComparer,
         ( marshal, indices ) => reorderState.UpdateMDTableWithTableIndices1( marshal, f => f.Parent, ( f, p ) => f.Parent = p )
         );

      // Sort DeclSecurity table (Parent)
      reorderState.UpdateMDTableIndices(
         md.SecurityDefinitions,
         Comparers.SecurityDefinitionComparer,
         ( sec, indices ) => reorderState.UpdateMDTableWithTableIndices1( sec, s => s.Parent, ( s, p ) => s.Parent = p )
         );

      // Sort ClassLayout table (Parent)
      reorderState.UpdateMDTableIndices(
         md.ClassLayouts,
         Comparers.ClassLayoutComparer,
         ( clazz, indices ) => reorderState.UpdateMDTableWithTableIndices1( clazz, c => c.Parent, ( c, p ) => c.Parent = p )
         );

      // Sort FieldLayout table (Field)
      reorderState.UpdateMDTableIndices(
         md.FieldLayouts,
         Comparers.FieldLayoutComparer,
         ( fieldLayout, indices ) => reorderState.UpdateMDTableWithTableIndices1( fieldLayout, f => f.Field, ( f, p ) => f.Field = p )
         );

      // Sort MethodSemantics table (Association)
      reorderState.UpdateMDTableIndices(
         md.MethodSemantics,
         Comparers.MethodSemanticsComparer,
         ( semantics, indices ) => reorderState.UpdateMDTableWithTableIndices2( semantics, s => s.Method, ( s, m ) => s.Method = m, s => s.Associaton, ( s, a ) => s.Associaton = a )
         );

      // Sort MethodImpl table (Class)
      reorderState.UpdateMDTableIndices(
         md.MethodImplementations,
         Comparers.MethodImplementationComparer,
         ( impl, indices ) => reorderState.UpdateMDTableWithTableIndices3( impl, i => i.Class, ( i, c ) => i.Class = c, i => i.MethodBody, ( i, b ) => i.MethodBody = b, i => i.MethodDeclaration, ( i, d ) => i.MethodDeclaration = d )
         );

      // Sort ImplMap table (MemberForwarded)
      reorderState.UpdateMDTableIndices(
         md.MethodImplementationMaps,
         Comparers.MethodImplementationMapComparer,
         ( map, indices ) => reorderState.UpdateMDTableWithTableIndices2( map, m => m.MemberForwarded, ( m, mem ) => m.MemberForwarded = mem, m => m.ImportScope, ( m, i ) => m.ImportScope = i )
         );

      // Sort FieldRVA table (Field)
      reorderState.UpdateMDTableIndices(
         md.FieldRVAs,
         Comparers.FieldRVAComparer,
         ( fieldRVAs, indices ) => reorderState.UpdateMDTableWithTableIndices1( fieldRVAs, f => f.Field, ( f, field ) => f.Field = field )
         );

      // Sort GenericParamDef table (Owner, Sequence)
      reorderState.UpdateMDTableIndices(
         md.GenericParameterDefinitions,
         Comparers.GenericParameterDefinitionComparer,
         ( gDef, indices ) => reorderState.UpdateMDTableWithTableIndices1( gDef, g => g.Owner, ( g, o ) => g.Owner = o )
         );

      // Sort GenericParameterConstraint table (Owner)
      reorderState.UpdateMDTableIndices(
         md.GenericParameterConstraintDefinitions,
         Comparers.GenericParameterConstraintDefinitionComparer,
         ( gDef, indices ) => reorderState.UpdateMDTableWithTableIndices2( gDef, g => g.Owner, ( g, o ) => g.Owner = o, g => g.Constraint, ( g, c ) => g.Constraint = c )
         );

      // Update ExportedType
      reorderState.UpdateMDTableIndices(
         md.ExportedTypes,
         null,
         ( et, indices ) => reorderState.UpdateMDTableWithTableIndices1( et, e => e.Implementation, ( e, i ) => e.Implementation = i )
         );

      // Update ManifestResource
      reorderState.UpdateMDTableIndices(
         md.ManifestResources,
         null,
         ( mr, indices ) => reorderState.UpdateMDTableWithTableIndices1Nullable( mr, m => m.Implementation, ( m, i ) => m.Implementation = i )
         );

      // Sort CustomAttributeDef table (Parent) 
      reorderState.UpdateMDTableIndices(
         md.CustomAttributeDefinitions,
         Comparers.CustomAttributeDefinitionComparer,
         ( ca, indices ) => reorderState.UpdateMDTableWithTableIndices2( ca, c => c.Parent, ( c, p ) => c.Parent = p, c => c.Type, ( c, t ) => c.Type = t )
         );
   }

   private static void RemoveDuplicatesUnsortedInPlace<T>( this MetaDataTable<T> mdTable, IEqualityComparer<T> equalityComparer )
      where T : class
   {
      var count = mdTable.RowCount;
      if ( count > 1 )
      {
         var table = mdTable.TableContents;
         var set = new HashSet<T>( equalityComparer );
         for ( var i = 0; i < table.Count; )
         {
            var item = table[i];
            if ( set.Add( item ) )
            {
               ++i;
            }
            else
            {
               table.RemoveAt( i );
            }
         }
      }
   }

   private static void RemoveDuplicatesAfterSorting( this MetaDataReOrderState reorderState )
   {
      var md = reorderState.MetaData;
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
            case Tables.NestedClass:
               md.NestedClassDefinitions.RemoveDuplicatesFromTable( indices );
               break;
         }
      }
   }

   private static void RemoveDuplicatesFromTable<T>( this MetaDataTable<T> mdTable, IDictionary<Int32, Int32> indices )
      where T : class
   {
      var table = mdTable.TableContents;
      var max = table.Count;
      for ( Int32 curIdx = 0, originalIdx = 0; originalIdx < max; ++originalIdx )
      {
         if ( indices.ContainsKey( originalIdx ) )
         {
            table.RemoveAt( curIdx );
         }
         else
         {
            ++curIdx;
         }
      }
   }

   private static void UpdateMDTableIndices<T>(
      this MetaDataReOrderState reorderState,
      MetaDataTable<T> mdTable,
      IComparer<T> comparer,
      Action<MetaDataTable<T>, Int32[]> tableUpdateCallback
      )
      where T : class
   {
      var thisTableIndices = reorderState.GetOrCreateIndexArray( mdTable );
      tableUpdateCallback( mdTable, thisTableIndices );
      if ( comparer != null )
      {
         mdTable.SortMDTable( thisTableIndices, comparer );
      }
   }

   private static void UpdateMDTableWithTableIndices1<T>(
      this MetaDataReOrderState reorderState,
      MetaDataTable<T> mdTable,
      Func<T, TableIndex> tableIndexGetter1,
      Action<T, TableIndex> tableIndexSetter1,
      Func<T, Int32, TableIndex, Boolean> rowAdditionalCheck = null
      )
      where T : class
   {
      var table = mdTable.TableContents;
      for ( var i = 0; i < table.Count; ++i )
      {
         reorderState.ProcessSingleTableIndexToUpdate( table[i], i, tableIndexGetter1, tableIndexSetter1, rowAdditionalCheck );
      }
   }

   private static void UpdateMDTableWithTableIndices1Nullable<T>(
      this MetaDataReOrderState reorderState,
      MetaDataTable<T> mdTable,
      Func<T, TableIndex?> tableIndexGetter1,
      Action<T, TableIndex> tableIndexSetter1,
      Func<T, Int32, TableIndex, Boolean> rowAdditionalCheck = null
      )
      where T : class
   {
      var table = mdTable.TableContents;
      for ( var i = 0; i < table.Count; ++i )
      {
         reorderState.ProcessSingleTableIndexToUpdateNullable( table[i], i, tableIndexGetter1, tableIndexSetter1, rowAdditionalCheck );
      }
   }

   private static void UpdateMDTableWithTableIndices2<T>(
      this MetaDataReOrderState reorderState,
      MetaDataTable<T> mdTable,
      Func<T, TableIndex> tableIndexGetter1,
      Action<T, TableIndex> tableIndexSetter1,
      Func<T, TableIndex> tableIndexGetter2,
      Action<T, TableIndex> tableIndexSetter2
      )
      where T : class
   {
      var table = mdTable.TableContents;
      for ( var i = 0; i < table.Count; ++i )
      {
         var row = table[i];
         reorderState.ProcessSingleTableIndexToUpdate( row, i, tableIndexGetter1, tableIndexSetter1, null );
         reorderState.ProcessSingleTableIndexToUpdate( row, i, tableIndexGetter2, tableIndexSetter2, null );
      }
   }

   private static void UpdateMDTableWithTableIndices3<T>(
      this MetaDataReOrderState reorderState,
      MetaDataTable<T> mdTable,
      Func<T, TableIndex> tableIndexGetter1,
      Action<T, TableIndex> tableIndexSetter1,
      Func<T, TableIndex> tableIndexGetter2,
      Action<T, TableIndex> tableIndexSetter2,
      Func<T, TableIndex> tableIndexGetter3,
      Action<T, TableIndex> tableIndexSetter3
      )
      where T : class
   {
      var table = mdTable.TableContents;
      for ( var i = 0; i < table.Count; ++i )
      {
         var row = table[i];
         reorderState.ProcessSingleTableIndexToUpdate( row, i, tableIndexGetter1, tableIndexSetter1, null );
         reorderState.ProcessSingleTableIndexToUpdate( row, i, tableIndexGetter2, tableIndexSetter2, null );
         reorderState.ProcessSingleTableIndexToUpdate( row, i, tableIndexGetter3, tableIndexSetter3, null );
      }
   }

   private static void ProcessSingleTableIndexToUpdate<T>( this MetaDataReOrderState reorderState, T row, Int32 rowIndex, Func<T, TableIndex> tableIndexGetter, Action<T, TableIndex> tableIndexSetter, Func<T, Int32, TableIndex, Boolean> rowAdditionalCheck )
      where T : class
   {
      if ( row != null )
      {
         reorderState.ProcessSingleTableIndexToUpdateWithTableIndex( row, rowIndex, tableIndexGetter( row ), tableIndexSetter, rowAdditionalCheck );
      }
   }

   private static void ProcessSingleTableIndexToUpdateWithTableIndex<T>( this MetaDataReOrderState reorderState, T row, Int32 rowIndex, TableIndex tableIndex, Action<T, TableIndex> tableIndexSetter, Func<T, Int32, TableIndex, Boolean> rowAdditionalCheck )
      where T : class
   {
      var newIndex = reorderState.GetFinalIndex( tableIndex );
      if ( newIndex != tableIndex.Index && ( rowAdditionalCheck == null || rowAdditionalCheck( row, rowIndex, tableIndex ) ) )
      {
         tableIndexSetter( row, new TableIndex( tableIndex.Table, newIndex ) );
      }
   }

   private static void ProcessSingleTableIndexToUpdateNullable<T>( this MetaDataReOrderState reorderState, T row, Int32 rowIndex, Func<T, TableIndex?> tableIndexGetter, Action<T, TableIndex> tableIndexSetter, Func<T, Int32, TableIndex, Boolean> rowAdditionalCheck )
      where T : class
   {
      if ( row != null )
      {
         var tIdx = tableIndexGetter( row );
         if ( tIdx.HasValue )
         {
            reorderState.ProcessSingleTableIndexToUpdateWithTableIndex( row, rowIndex, tIdx.Value, tableIndexSetter, rowAdditionalCheck );
         }
      }
   }

   private static void SortMDTable<T>( this MetaDataTable<T> mdTable, Int32[] indices, IComparer<T> comparer )
      where T : class
   {
      // If within 'indices' array, we have value '2' at index '0', it means that within the 'table', there should be value at index '0' which is currently at index '2'
      var count = mdTable.RowCount;
      if ( count > 1 )
      {
         // 1. Make a copy of array
         var table = mdTable.TableContents;
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

   private static void SortMethodILExceptionBlocks( this CILMetaData md )
   {
      // Remember that inner exception blocks must precede outer ones
      foreach ( var il in md.MethodDefinitions.TableContents.Where( methodDef => methodDef.IL != null ).Select( methodDef => methodDef.IL ) )
      {
         il.ExceptionBlocks.Sort(
            ( item1, item2 ) =>
            {
               // Return -1 if item1 is inner block of item2, 0 if they are same, 1 if item1 is not inner block of item2
               return Object.ReferenceEquals( item1, item2 ) || ( item1.TryOffset == item2.TryOffset && item1.HandlerOffset == item2.HandlerOffset ) ? 0 :
                  ( item1.TryOffset >= item2.HandlerOffset + item2.HandlerLength
                     || ( item1.TryOffset <= item2.TryOffset && item1.HandlerOffset + item1.HandlerLength > item2.HandlerOffset + item2.HandlerLength ) ?
                  1 :
                  -1
                  );
            } );
      }
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

   private static void ReOrderMDTableWithAscendingReferences<T, U>(
      this MetaDataReOrderState reorderState,
      MetaDataTable<T> mdTable,
      Int32[] thisTableIndices,
      MetaDataTable<U> referencingMDTable,
      Int32[] referencingTableIndices,
      Func<U, Int32> referenceIndexGetter,
      Action<U, Int32> referenceIndexSetter,
      Func<U, Int32> referenceCountGetter
      )
      where T : class
      where U : class
   {
      var refTableCount = referencingMDTable.RowCount;
      var thisTableCount = mdTable.RowCount;

      if ( thisTableCount > 0 )
      {
         var table = mdTable.TableContents;
         var referencingTable = referencingMDTable.TableContents;

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

   private static Boolean CheckMDDuplicatesUnsorted<T>(
      this MetaDataReOrderState reorderState,
      MetaDataTable<T> mdTable,
      IEqualityComparer<T> comparer
      )
      where T : class
   {
      var list = mdTable.TableContents;
      var table = mdTable.TableKind;
      var foundDuplicates = false;
      var count = list.Count;
      var indices = reorderState.GetOrCreateIndexArray( mdTable );
      if ( count > 1 )
      {
         var set = new HashSet<T>( comparer );
         for ( var i = 0; i < list.Count; ++i )
         {
            var cur = list[i];
            if ( cur != null && !set.Add( cur ) )
            {
               if ( !foundDuplicates )
               {
                  foundDuplicates = true;
               }

               var actualIndex = 0;
               while ( list[actualIndex] == null || !comparer.Equals( cur, list[actualIndex] ) )
               {
                  ++actualIndex;
               }

               // Mark as duplicate - replace value with null
               reorderState.MarkDuplicate( table, i, actualIndex );
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

   //private static Int32[] CreateIndexArray( Int32 size )
   //{
   //   var retVal = new Int32[size];
   //   PopulateIndexArray( retVal );
   //   return retVal;
   //}

   //private static void PopulateIndexArray( Int32[] array )
   //{
   //   var size = array.Length;
   //   // This might get called for already-used index array
   //   // Therefore, start from 0
   //   for ( var i = 0; i < size; ++i )
   //   {
   //      array[i] = i;
   //   }
   //}

   private static void UpdateSignaturesAndILWhileRemovingDuplicates( this MetaDataReOrderState reorderState )
   {
      var md = reorderState.MetaData;

      // Remove duplicates from AssemblyRef table (since reordering of the TypeRef table will require the indices in this table to be present)
      // ECMA-335: The AssemblyRef table shall contain no duplicates (where duplicate rows are deemd  to be those having the same MajorVersion, MinorVersion, BuildNumber, RevisionNumber, PublicKeyOrToken, Name, and Culture) [WARNING] 
      reorderState.CheckMDDuplicatesUnsorted(
         md.AssemblyReferences,
         ComparerFromFunctions.NewEqualityComparer<AssemblyReference>(
            ( x, y ) => x.AssemblyInformation.Equals( y.AssemblyInformation ),
            x => x.AssemblyInformation.GetHashCode() )
         );

      // Remove duplicates from ModuleRef table (since reordering of the TypeRef table will require the indices in this table to be present)
      // ECMA-335: There should be no duplicate rows  [WARNING] 
      reorderState.CheckMDDuplicatesUnsorted(
         md.ModuleReferences,
         Comparers.ModuleReferenceEqualityComparer
         );


      // TypeRef
      // ECMA-335:  There shall be no duplicate rows, where a duplicate has the same ResolutionScope, TypeName and TypeNamespace  [ERROR] 
      // Do in a loop, since TypeRef may reference itself
      var tRefs = md.TypeReferences;
      reorderState.GetOrCreateIndexArray( tRefs );
      // Create index array for Module table, as TypeRef.ResolutionScope may reference that.
      reorderState.GetOrCreateIndexArray( md.ModuleDefinitions );
      Boolean removedDuplicates;
      var updateState = new SignatureReOrderState( reorderState, Tables.TypeRef );
      do
      {
         reorderState.UpdateMDTableWithTableIndices1Nullable(
            tRefs,
            tRef => tRef.ResolutionScope,
            ( tRef, resScope ) => tRef.ResolutionScope = resScope,
            ( tRef, tRefIdx, resScope ) => updateState.ShouldProcess( resScope )
            );
         removedDuplicates = updateState.CheckMDDuplicatesUnsortedWithSignatureReOrderState(
            tRefs,
            Comparers.TypeReferenceEqualityComparer
            );
      } while ( removedDuplicates );

      // TypeSpec
      // ECMA-335: There shall be no duplicate rows, based upon Signature  [ERROR] 
      // This is the last of three tables (TypeDef, TypeRef, TypeSpec) which may appear in signatures, so update all signatures also.
      // Furthermore, updating signatures may also cause this table to start having duplicates.
      // So remove duplicates and update signatures in a loop
      var tSpecs = md.TypeSpecifications;
      reorderState.GetOrCreateIndexArray( tSpecs );
      updateState = new SignatureReOrderState( reorderState, Tables.TypeSpec );
      do
      {
         updateState.UpdateSignatures();
         removedDuplicates = updateState.CheckMDDuplicatesUnsortedWithSignatureReOrderState(
            tSpecs,
            Comparers.TypeSpecificationEqualityComparer
            );

      } while ( removedDuplicates );

      // ECMA-335: IL tokens shall be from TypeDef, TypeRef, TypeSpec, MethodDef, FieldDef, MemberRef, MethodSpec or StandaloneSignature tables.
      // The only unprocessed tables from those are MemberRef, MethodSpec and StandaloneSignature
      // ECMA-335:  The MemberRef table shall contain no duplicates, where duplicate rows have the same Class, Name, and Signature  [WARNING] 
      var memberRefs = md.MemberReferences;
      reorderState.UpdateMDTableWithTableIndices1(
         memberRefs,
         mRef => mRef.DeclaringType,
         ( mRef, dType ) => mRef.DeclaringType = dType
         );
      reorderState.CheckMDDuplicatesUnsorted(
         memberRefs,
         Comparers.MemberReferenceEqualityComparer
         );

      // MethodSpec
      // ECMA-335: There shall be no duplicate rows based upon Method+Instantiation  [ERROR] 
      var mSpecs = md.MethodSpecifications;
      reorderState.UpdateMDTableWithTableIndices1(
         mSpecs,
         mSpec => mSpec.Method,
         ( mSpec, method ) => mSpec.Method = method
         );
      reorderState.CheckMDDuplicatesUnsorted(
         mSpecs,
         Comparers.MethodSpecificationEqualityComparer
         );

      // StandaloneSignature
      // ECMA-335: Duplicates allowed (but we will make them all unique anyway)
      var standaloneSigs = md.StandaloneSignatures;
      reorderState.CheckMDDuplicatesUnsorted(
         md.StandaloneSignatures,
         Comparers.StandaloneSignatureEqualityComparer
         );

      // Now update IL
      reorderState.UpdateIL();



      //// ECMA-335: IL tokens shall be from TypeDef, TypeRef, TypeSpec, MethodDef, FieldDef, MemberRef, MethodSpec or StandaloneSignature tables.
      //// All table indices in signatures should only ever reference TypeDef, TypeRef or TypeSpec tables.
      //// Tables with signatures are: FieldDef, MethodDef, MemberRef, CustomAttribute, DeclSecurity, StandaloneSignature, PropertyDef, TypeSpecification, MethodSpecification
      //// The ones that have rules for duplicates: MemberRef, StandaloneSignature, PropertyDef, TypeSpecification, MethodSpecification
      //// We will not handle PropertyDef (nor EventDef) duplicates
      //// The ones without duplicates will be handled later (those are CustomAttribute, DeclSecurity)
      //tableIndices[(Int32) Tables.MemberRef] = CreateIndexArray( md.MemberReferences.Count );
      //tableIndices[(Int32) Tables.MethodSpec] = CreateIndexArray( md.MethodSpecifications.Count );
      //tableIndices[(Int32) Tables.StandaloneSignature] = CreateIndexArray( md.StandaloneSignatures.Count );


      //var memberRefs = md.MemberReferences;
      //var mSpecs = md.MethodSpecifications;
      //var standAloneSigs = md.StandaloneSignatures;
      //var props = md.PropertyDefinitions;
      //Boolean removedTypeRefDuplicates, removedTSpecDuplicates, removedMemberRefDuplicates, removedMethodSpecDuplicates, removedStandaloneSigDuplicates, updatedSignatureTableIndices, updatedILTableIndices;
      //// This has to be done in loop since modifying e.g. type specs will modify signatures, and thus might result in more typeref, typespec or memberref duplicates
      //do
      //{
      //   var startingDuplicates = new HashSet<TableIndex>( reorderState.Duplicates.SelectMany( kvp => kvp.Value.Select( i => new TableIndex( kvp.Key, i ) ) ) );

      //   // ECMA-335:  There shall be no duplicate rows, where a duplicate has the same ResolutionScope, TypeName and TypeNamespace  [ERROR] 
      //   tRefs.UpdateMDTableWithTableIndices1Nullable(
      //      tableIndices,
      //      tRef => tRef.ResolutionScope,
      //      ( tRef, resScope ) => tRef.ResolutionScope = resScope,
      //      ( tRef, tRefIdx, resScope ) => updateState.CheckRowTableIndexWhenHandlingSignatures( resScope, new TableIndex( Tables.TypeRef, tRefIdx ) )
      //      );
      //   removedTypeRefDuplicates = tRefs.CheckMDDuplicatesUnsorted( tableIndices[(Int32) Tables.TypeRef], Tables.TypeRef, reorderState, Comparers.TypeReferenceEqualityComparer );

      //   // ECMA-335: There shall be no duplicate rows, based upon Signature  [ERROR] 
      //   removedTSpecDuplicates = tSpecs.CheckMDDuplicatesUnsorted( tableIndices[(Int32) Tables.TypeSpec], Tables.TypeSpec, reorderState, Comparers.TypeSpecificationEqualityComparer );

      //   // ECMA-335:  The MemberRef table shall contain no duplicates, where duplicate rows have the same Class, Name, and Signature  [WARNING] 
      //   memberRefs.UpdateMDTableWithTableIndices1(
      //      tableIndices,
      //      mRef => mRef.DeclaringType,
      //      ( mRef, dType ) => mRef.DeclaringType = dType,
      //      ( mRef, mRefIdx, dType ) => updateState.CheckRowTableIndexWhenHandlingSignatures( dType, new TableIndex( Tables.MemberRef, mRefIdx ) )
      //      );
      //   removedMemberRefDuplicates = memberRefs.CheckMDDuplicatesUnsorted( tableIndices[(Int32) Tables.MemberRef], Tables.MemberRef, reorderState, Comparers.MemberReferenceEqualityComparer );

      //   // ECMA-335: There shall be no duplicate rows based upon Method+Instantiation  [ERROR] 
      //   mSpecs.UpdateMDTableWithTableIndices1(
      //      tableIndices,
      //      mSpec => mSpec.Method,
      //      ( mSpec, method ) => mSpec.Method = method,
      //      ( mSpec, mSpecIdx, method ) => updateState.CheckRowTableIndexWhenHandlingSignatures( method, new TableIndex( Tables.MethodSpec, mSpecIdx ) )
      //      );
      //   removedMethodSpecDuplicates = mSpecs.CheckMDDuplicatesUnsorted( tableIndices[(Int32) Tables.MethodSpec], Tables.MethodSpec, reorderState, Comparers.MethodSpecificationEqualityComparer );

      //   // ECMA-335: Duplicates allowed (but we will make them all unique anyway)
      //   removedStandaloneSigDuplicates = standAloneSigs.CheckMDDuplicatesUnsorted( tableIndices[(Int32) Tables.StandaloneSignature], Tables.StandaloneSignature, reorderState, Comparers.StandaloneSignatureEqualityComparer );

      //   // Calculate the duplicates
      //   startingDuplicates.SymmetricExceptWith( reorderState.Duplicates.SelectMany( kvp => kvp.Value.Select( i => new TableIndex( kvp.Key, i ) ) ) );
      //   updateState.DuplicatesThisRound.Clear();
      //   updateState.DuplicatesThisRound.UnionWith( startingDuplicates );

      //   // Update signatures
      //   updateState.ResetChangedAny();
      //   updateState.UpdateSignatures();
      //   updatedSignatureTableIndices = updateState.ChangedAny;

      //   // Update table indices in IL
      //   updateState.ResetChangedAny();
      //   updateState.UpdateIL();
      //   updatedILTableIndices = updateState.ChangedAny;
      //} while ( updatedSignatureTableIndices || updatedILTableIndices );
   }

   private static Boolean CheckMDDuplicatesUnsortedWithSignatureReOrderState<T>(
      this SignatureReOrderState state,
      MetaDataTable<T> mdTable,
      IEqualityComparer<T> equalityComparer
      )
      where T : class
   {
      var reorderState = state.ReOrderState;
      state.BeforeDuplicateRemoval();
      var removedDuplicates = reorderState.CheckMDDuplicatesUnsorted(
         mdTable,
         equalityComparer
         );
      if ( removedDuplicates )
      {
         state.AfterDuplicateRemoval();
      }

      return removedDuplicates;
   }

   // This method updates all signature table indices and returns true if any table index was modified
   private static void UpdateSignatures( this SignatureReOrderState state )
   {
      var md = state.MD;
      // 1. FieldDef
      foreach ( var field in md.FieldDefinitions.TableContents.Where( f => f != null ) )
      {
         state.UpdateFieldSignature( field.Signature );
      }

      // 2. MethodDef
      foreach ( var method in md.MethodDefinitions.TableContents.Where( m => m != null ) )
      {
         state.UpdateMethodDefSignature( method.Signature );
      }

      // 3. MemberRef
      foreach ( var member in md.MemberReferences.TableContents.Where( m => m != null ) )
      {
         state.UpdateAbstractSignature( member.Signature );
      }

      // 4. StandaloneSignature
      foreach ( var sig in md.StandaloneSignatures.TableContents.Where( s => s != null ) )
      {
         state.UpdateAbstractSignature( sig.Signature );
      }

      // 5. PropertyDef
      foreach ( var prop in md.PropertyDefinitions.TableContents ) // No need for null check as property definition is not sorted nor checked for duplicates
      {
         state.UpdatePropertySignature( prop.Signature );
      }

      // 6. TypeSpec
      foreach ( var tSpec in md.TypeSpecifications.TableContents.Where( t => t != null ) )
      {
         state.UpdateTypeSignature( tSpec.Signature );
      }

      // 7. MethodSpecification
      foreach ( var mSpec in md.MethodSpecifications.TableContents.Where( m => m != null ) )
      {
         state.UpdateGenericMethodSignature( mSpec.Signature );
      }

      // CustomAttribute and DeclarativeSecurity signatures do not reference table indices, so they can be skipped
   }

   private static void UpdateIL( this MetaDataReOrderState state )
   {
      foreach ( var mDef in state.MetaData.MethodDefinitions.TableContents )
      {
         var il = mDef.IL;
         if ( il != null )
         {
            // Local signature
            var localIdx = il.LocalsSignatureIndex;
            if ( localIdx.HasValue )
            {
               var newIdx = state.GetFinalIndex( localIdx.Value );
               if ( newIdx != localIdx.Value.Index )
               {
                  il.LocalsSignatureIndex = localIdx.Value.ChangeIndex( newIdx );
               }
            }

            // Exception blocks
            foreach ( var block in il.ExceptionBlocks )
            {
               var excIdx = block.ExceptionType;
               if ( excIdx.HasValue )
               {
                  var newIdx = state.GetFinalIndex( excIdx.Value );
                  if ( newIdx != excIdx.Value.Index )
                  {
                     block.ExceptionType = excIdx.Value.ChangeIndex( newIdx );
                  }
               }
            }

            // Op codes
            foreach ( var code in il.OpCodes.Where( code => code.InfoKind == OpCodeOperandKind.OperandToken ) )
            {
               var codeInfo = (OpCodeInfoWithToken) code;
               var token = codeInfo.Operand;
               var newIdx = state.GetFinalIndex( token );
               if ( newIdx != token.Index )
               {
                  codeInfo.Operand = token.ChangeIndex( newIdx );
               }
            }
         }
      }
   }

   private static void UpdateAbstractSignature( this SignatureReOrderState state, AbstractSignature sig )
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

   private static void UpdateFieldSignature( this SignatureReOrderState state, FieldSignature sig )
   {
      state.UpdateSignatureCustomModifiers( sig.CustomModifiers );
      state.UpdateTypeSignature( sig.Type );
   }

   private static void UpdateGenericMethodSignature( this SignatureReOrderState state, GenericMethodSignature sig )
   {
      foreach ( var gArg in sig.GenericArguments )
      {
         state.UpdateTypeSignature( gArg );
      }
   }

   private static void UpdateLocalVariablesSignature( this SignatureReOrderState state, LocalVariablesSignature sig )
   {
      state.UpdateParameterSignatures( sig.Locals );
   }

   private static void UpdatePropertySignature( this SignatureReOrderState state, PropertySignature sig )
   {
      state.UpdateSignatureCustomModifiers( sig.CustomModifiers );
      state.UpdateParameterSignatures( sig.Parameters );
      state.UpdateTypeSignature( sig.PropertyType );
   }

   private static void UpdateTypeSignature( this SignatureReOrderState state, TypeSignature sig )
   {
      switch ( sig.TypeSignatureKind )
      {
         case TypeSignatureKind.ClassOrValue:
            var clazz = (ClassOrValueTypeSignature) sig;
            var type = clazz.Type;
            if ( state.ShouldProcess( type ) )
            {
               var newIdx = state.ReOrderState.GetFinalIndex( type );
               if ( newIdx != type.Index )
               {
                  clazz.Type = type.ChangeIndex( newIdx );
               }
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

   private static void UpdateSignatureCustomModifiers( this SignatureReOrderState state, IList<CustomModifierSignature> mods )
   {
      foreach ( var mod in mods )
      {
         var idx = mod.CustomModifierType;
         if ( state.ShouldProcess( idx ) )
         {
            var newIdx = state.ReOrderState.GetFinalIndex( idx );
            if ( newIdx != idx.Index )
            {
               mod.CustomModifierType = idx.ChangeIndex( newIdx );
            }
         }
      }
   }

   private static void UpdateAbstractMethodSignature( this SignatureReOrderState state, AbstractMethodSignature sig )
   {
      state.UpdateParameterSignature( sig.ReturnType );
      state.UpdateParameterSignatures( sig.Parameters );
   }

   private static void UpdateMethodDefSignature( this SignatureReOrderState state, MethodDefinitionSignature sig )
   {
      state.UpdateAbstractMethodSignature( sig );
   }

   private static void UpdateMethodRefSignature( this SignatureReOrderState state, MethodReferenceSignature sig )
   {
      state.UpdateAbstractMethodSignature( sig );
      state.UpdateParameterSignatures( sig.VarArgsParameters );
   }

   private static void UpdateParameterSignatures<T>( this SignatureReOrderState state, List<T> parameters )
      where T : ParameterOrLocalVariableSignature
   {
      foreach ( var param in parameters )
      {
         state.UpdateParameterSignature( param );
      }
   }

   private static void UpdateParameterSignature( this SignatureReOrderState state, ParameterOrLocalVariableSignature parameter )
   {
      state.UpdateSignatureCustomModifiers( parameter.CustomModifiers );
      state.UpdateTypeSignature( parameter.Type );
   }

   //private static Boolean SignatureOrILTableIndexDiffersNullable( this SignatureReorderState state, TableIndex? index, Object parent, out Int32 newIndex )
   //{
   //   var retVal = index.HasValue;
   //   if ( retVal )
   //   {
   //      retVal = state.SignatureOrILTableIndexDiffers( index.Value, parent, out newIndex );
   //   }
   //   else
   //   {
   //      newIndex = -1;
   //   }
   //   return retVal;
   //}

   //private static Boolean SignatureOrILTableIndexDiffers( this SignatureReorderState state, TableIndex index, Object parent, out Int32 newIndex )
   //{
   //   var targetIndices = state.TableIndices[(Int32) index.Table];
   //   var retVal = ( state.IsFirstRound || targetIndices[index.Index] == null );
   //   if ( retVal )
   //   {

   //   }

   //   var retVal = state.CheckRowTableIndexWhenHandlingSignatures( index, parent );
   //   if ( retVal )
   //   {
   //      var oldIndex = index.Index;
   //      newIndex = state.TableIndices[(Int32) index.Table][oldIndex]; // TODO NullRef if not TypeDef/TypeRef/TypeSpec, ArrayIndexOutOfBounds if invalid index!
   //      retVal = oldIndex != newIndex;
   //      // Mark that we have changed an index
   //      state.ProcessedTableIndex( retVal );
   //   }
   //   else
   //   {
   //      newIndex = -1;
   //   }

   //   return retVal;
   //}

   //private static Boolean CheckRowTableIndexWhenHandlingSignatures( this SignatureReorderState state, TableIndex index, Object parent )
   //{
   //   var visitedInfo = state.VisitedInfo;
   //   var notVisited = !visitedInfo.ContainsKey( parent );// Check whether we have already visited this index
   //   // Even if we have already visited this, have to check here whether target is null
   //   // If target is null, that means that after visiting, the target became duplicate and was removed
   //   // So we need to update it again
   //   var retVal = notVisited || state.DuplicatesThisRound.Contains( index );
   //   if ( notVisited )
   //   {
   //      visitedInfo.Add( parent, parent );
   //   }

   //   return retVal;
   //}

   //private static IEnumerable<Int32> GetDeclaringTypeChain( this Int32 typeDefIndex, IList<NestedClassDefinition> sortedNestedClass )
   //{

   //   return typeDefIndex.AsSingleBranchEnumerable( cur =>
   //      {
   //         var nIdx = sortedNestedClass.FindRowFromSortedNestedClass( cur );
   //         if ( nIdx != -1 )
   //         {
   //            nIdx = sortedNestedClass[nIdx].EnclosingClass.Index;
   //         }
   //         return nIdx;
   //      }, cur => cur == -1 || cur == typeDefIndex, false ); // Stop also when we hit the same index again (illegal situation but possible), and don't include itself
   //}

   //private static Int32 FindRowFromSortedNestedClass( this IList<NestedClassDefinition> sortedNestedClass, Int32 currentTypeDefIndex )
   //{
   //   using ( var xDeclTypeRows = sortedNestedClass.GetReferencingRowsFromOrderedWithIndex( Tables.TypeDef, currentTypeDefIndex, nIdx =>
   //   {
   //      while ( sortedNestedClass[nIdx] == null )
   //      {
   //         --nIdx;
   //      }
   //      return sortedNestedClass[nIdx].NestedClass;
   //   } ).GetEnumerator() )
   //   {
   //      return xDeclTypeRows.MoveNext() ?
   //         xDeclTypeRows.Current : // has declaring type. 
   //         -1; // does not have declaring type. 
   //   }
   //}

   // Returns token with 1-based indexing, or zero if tableIdx has no value
   internal static Int32 GetOneBasedToken( this TableIndex? tableIdx )
   {
      return tableIdx.HasValue ?
         tableIdx.Value.OneBasedToken :
         0;
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
         var isNewObj = code.Value != OpCodeEncoding.Newobj;
         if ( sig.SignatureStarter.IsHasThis() && !isNewObj )
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

         if ( code.Value == OpCodeEncoding.Calli )
         {
            // Pop function pointer
            --curStacksize;
         }

         var rType = sig.ReturnType.Type;

         // TODO we could check here for stack underflow!

         if ( isNewObj
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
      if ( jump >= 0 )
      {
         var idx = state.NextCodeByteOffset + jump;
         state.StackSizes[idx] = Math.Max( state.StackSizes[idx], stackSize );
      }
   }

   public static Boolean TryGetTargetFrameworkInformation( this CILMetaData md, out TargetFrameworkInfo fwInfo, MetaDataResolver resolverToUse = null )
   {
      fwInfo = md.CustomAttributeDefinitions.TableContents
         .Where( ( ca, caIdx ) =>
         {
            var isTargetFWAttribute = false;
            if ( ca.Parent.Table == Tables.Assembly
            && md.AssemblyDefinitions.GetOrNull( ca.Parent.Index ) != null
            && ca.Type.Table == Tables.MemberRef ) // Remember that framework assemblies don't have TargetFrameworkAttribute defined
            {
               var memberRef = md.MemberReferences.GetOrNull( ca.Type.Index );
               if ( memberRef != null
                  && memberRef.Signature.SignatureKind == SignatureKind.MethodReference
                  && memberRef.DeclaringType.Table == Tables.TypeRef
                  && String.Equals( memberRef.Name, ".ctor" )
                  )
               {
                  var typeRef = md.TypeReferences.GetOrNull( memberRef.DeclaringType.Index );
                  if ( typeRef != null
                     && typeRef.ResolutionScope.HasValue
                     && typeRef.ResolutionScope.Value.Table == Tables.AssemblyRef
                     && String.Equals( typeRef.Namespace, "System.Runtime.Versioning" )
                     && String.Equals( typeRef.Name, "TargetFrameworkAttribute" )
                     )
                  {
                     if ( ca.Signature is RawCustomAttributeSignature )
                     {
                        // Use resolver with no events, so nothing additional will be loaded (and is not required, as both arguments are strings
                        ( resolverToUse ?? new MetaDataResolver() ).ResolveCustomAttributeSignature( md, caIdx );
                     }

                     var caSig = ca.Signature as CustomAttributeSignature;
                     if ( caSig != null
                        && caSig.TypedArguments.Count > 0
                        && caSig.TypedArguments[0].Type.IsSimpleTypeOfKind( SignatureElementTypes.String )
                        )
                     {
                        // Resolving succeeded
                        isTargetFWAttribute = true;
                     }
#if DEBUG
                     else
                     {
                        // Breakpoint (resolving failed, even though it should have succeeded
                     }
#endif
                  }
               }
            }
            return isTargetFWAttribute;
         } )
         .Select( ca =>
         {

            var fwInfoString = ( (CustomAttributeSignature) ca.Signature ).TypedArguments[0].Value.ToStringSafe( null );
            //var displayName = caSig.NamedArguments.Count > 0
            //   && String.Equals( caSig.NamedArguments[0].Name, "FrameworkDisplayName" )
            //   && caSig.NamedArguments[0].Value.Type.IsSimpleTypeOfKind( SignatureElementTypes.String ) ?
            //   caSig.NamedArguments[0].Value.Value.ToStringSafe( null ) :
            //   null;
            TargetFrameworkInfo thisFWInfo;
            return TargetFrameworkInfo.TryParse( fwInfoString, out thisFWInfo ) ? thisFWInfo : null;

         } )
         .FirstOrDefault();

      return fwInfo != null;
   }

   public static TargetFrameworkInfo GetTargetFrameworkInformationOrNull( this CILMetaData md, MetaDataResolver resolverToUse = null )
   {
      TargetFrameworkInfo retVal;
      return md.TryGetTargetFrameworkInformation( out retVal, resolverToUse ) ?
         retVal :
         null;
   }


   public static Boolean IsSimpleTypeOfKind( this CustomAttributeArgumentType caType, SignatureElementTypes typeKind )
   {
      return caType.ArgumentTypeKind == CustomAttributeArgumentTypeKind.Simple
         && ( (CustomAttributeArgumentTypeSimple) caType ).SimpleType == typeKind;
   }

   public static Boolean CanBeReferencedFromIL( this Tables table )
   {
      switch ( table )
      {
         case Tables.TypeDef:
         case Tables.TypeRef:
         case Tables.TypeSpec:
         case Tables.MethodDef:
         case Tables.Field:
         case Tables.MemberRef:
         case Tables.MethodSpec:
         case Tables.StandaloneSignature:
            return true;
         default:
            return false;
      }
   }

   public static Boolean IsEmbeddedResource( this ManifestResource resource )
   {
      return !resource.Implementation.HasValue;
   }

   public static IEnumerable<FileReference> GetModuleFileReferences( this CILMetaData md )
   {
      return md.FileReferences.TableContents.Where( f => f.Attributes.ContainsMetadata() );
   }
}
