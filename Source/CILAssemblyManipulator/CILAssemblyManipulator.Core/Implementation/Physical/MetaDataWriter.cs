/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
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
using System.IO;
using System.Linq;
using System.Text;
using CILAssemblyManipulator.API;
using CILAssemblyManipulator.Implementation;
using CollectionsWithRoles.API;
using CommonUtils;

namespace CILAssemblyManipulator.Implementation.Physical
{
   using TRVA = System.UInt32;

   internal class MetaDataWriter : IDisposable
   {
      private const UInt32 USER_STRING_MASK = 0x70 << 24;

      private const Int32 MD_IDX = 0;
      private const Int32 SYS_STRINGS_IDX = 1;
      private const Int32 USER_STRINGS_IDX = 2;
      private const Int32 GUID_IDX = 3;
      private const Int32 BLOB_IDX = 4;

      // ECMA-335, p. 271 
      private const UInt32 MD_SIGNATURE = 0x424A5342;
      private const UInt16 MD_MAJOR = 1;
      private const UInt16 MD_MINOR = 1;
      private const UInt32 MD_RESERVED = 0;
      private const UInt16 MD_FLAGS = 0;

      private const Int32 MD_MAX_VERSION_LENGTH = 255;

      private const Int32 TABLE_STREAM_RESERVED = 0;
      private const Byte TABLE_STREAM_RESERVED_2 = 1;

      // ECMA-335, p. 210
      //private static readonly ISet<Tables> SORTED_TABLES = new HashSet<Tables>(
      //   new Tables[] { Tables.ClassLayout, Tables.Constant, Tables.CustomAttribute, Tables.DeclSecurity, Tables.FieldLayout, Tables.FieldMarshal, Tables.FieldRVA, Tables.GenericParameter, Tables.GenericParameterConstraint, Tables.ImplMap, Tables.InterfaceImpl, Tables.MethodImpl, Tables.MethodSemantics, Tables.NestedClass }
      //   );
      private const Int64 SORTED_TABLES = 0x16003325FA00;

      private static readonly String[] STREAM_NAMES = new String[] {
         Consts.TABLE_STREAM_NAME,
         Consts.SYS_STRING_STREAM_NAME,
         Consts.USER_STRING_STREAM_NAME,
         Consts.GUID_STREAM_NAME,
         Consts.BLOB_STREAM_NAME
      };
      private static readonly ISet<String> PSEUDO_ATTRIBUTES = new HashSet<String>
      {
         "System.Reflection" + CILTypeImpl.NAMESPACE_SEPARATOR + "AssemblyAlgorithmIdAttribute",
         "System.Reflection" + CILTypeImpl.NAMESPACE_SEPARATOR + "AssemblyFlagsAttribute",
         "System.Runtime.InteropServices" + CILTypeImpl.NAMESPACE_SEPARATOR + "DllImportAttribute",
         "System.Runtime.InteropServices" + CILTypeImpl.NAMESPACE_SEPARATOR + "FieldOffsetAttribute",
         "System.Runtime.InteropServices" + CILTypeImpl.NAMESPACE_SEPARATOR + "InAttribute",
         "System.Runtime.InteropServices" + CILTypeImpl.NAMESPACE_SEPARATOR + "MarshalAsAttribute",
         "System.Runtime.CompilerServices" + CILTypeImpl.NAMESPACE_SEPARATOR + "MethodImplAttribute",
         "System.Runtime.CompilerServices" + CILTypeImpl.NAMESPACE_SEPARATOR + "TypeForwardedToAttribute",
         "System.Runtime.InteropServices" + CILTypeImpl.NAMESPACE_SEPARATOR + "OutAttribute",
         "System.Runtime.InteropServices"+ CILTypeImpl.NAMESPACE_SEPARATOR + "StructLayoutAttribute",
         "System.Runtime.InteropServices"+ CILTypeImpl.NAMESPACE_SEPARATOR + "OptionalAttribute"
      };

      private static readonly IEqualityComparer<LocalBuilder> LOCAL_BUILDER_EQ_COMP = ComparerFromFunctions.NewEqualityComparer<LocalBuilder>(
         ( l1, l2 ) =>
         {
            return Object.ReferenceEquals( l1, l2 ) || ( l1 != null && l2 != null && l1.LocalIndex == l2.LocalIndex && Object.Equals( l1.LocalType, l2.LocalType ) && l1.IsPinned == l2.IsPinned );
         }, l => l.LocalType.GetHashCode() );
      private static readonly IEqualityComparer<LocalBuilder[]> LOCAL_BUILDERS_EQ_COMP = ArrayEqualityComparer<LocalBuilder>.NewArrayEqualityComparer( LOCAL_BUILDER_EQ_COMP );


      private readonly CILReflectionContextImpl _context;
      private readonly CILModule _module;

      private readonly TableEmittingInfoWithList<CILType> _typeDef;
      private readonly TableEmittingInfoWithList<CILMethodBase> _methodDef;
      private readonly TableEmittingInfoWithList<CILField> _fieldDef;
      private readonly TableEmittingInfoWithList<CILParameter> _paramDef;
      private readonly TableEmittingInfoWithList<CILTypeParameter> _gParamDef;
      private readonly TableEmittingInfoWithList<Tuple<Int32, String, String>> _typeRef;
      private readonly TableEmittingInfoWithSignature<CILTypeBase> _typeSpec;
      private readonly TableEmittingInfoWithSignatureAndForeignKey<CILMethod, Int32> _methodSpec;
      private readonly TableEmittingInfoWithSignatureAndForeignKey<CILElementOwnedByType, Tuple<Int32, String>> _memberRef; // TODO when adding methods/fields to type, the methods can't share fields' names and vice versa!
      private readonly TableEmittingInfoWithSignature<Object> _standaloneSig;
      private readonly TableEmittingInfoWithList<CILAssemblyName> _assembly;
      private readonly IList<Tuple<Int32, Int32>> _interfaceImpl;
      private readonly IList<Tuple<Int32, Int32>> _eventMap;
      private readonly TableEmittingInfoWithList<CILEvent> _eventDef;
      private readonly IList<Tuple<Int32, Int32>> _propertyMap;
      private readonly TableEmittingInfoWithList<CILProperty> _propertyDef;
      private readonly IList<Tuple<Int32, Int32>> _nestedClass;
      private readonly TableEmittingInfoWithList<CILModule> _moduleDef;
      private readonly IList<Tuple<Int32, Int32>> _gParamConstraints;
      private readonly IList<Tuple<Int16, Int32, UInt32>> _constants;
      private readonly IList<Tuple<Int32, Int32, UInt32>> _customAttributes;
      private readonly IList<Tuple<UInt16, Int32, Int32>> _methodSemantics;
      private readonly IList<Tuple<Int32, Int32, Int32>> _methodImpl;
      private readonly IList<Tuple<PInvokeAttributes, Int32, UInt32, Int32>> _implMap;
      private readonly TableEmittingInfoWithList<String> _moduleRef;
      private readonly IList<CILAssemblyName> _assemblyRef;
      private readonly IList<Tuple<Int32, Int32>> _fieldLayout;
      private readonly IList<Tuple<Int16, Int32, Int32>> _classLayout;
      private readonly IList<Tuple<Int32, UInt32>> _marshals;
      private readonly IDictionary<String, Tuple<Int32, FileAttributes, UInt32, UInt32>> _files;
      private readonly IList<Tuple<TypeAttributes, Int32, String, String, Int32>> _exportedTypes;
      private readonly IList<Tuple<UInt16, Int32, UInt32>> _declSecurity;
      private readonly IList<Guid> _guids;
      private readonly Int32[] _methodLocalSigs;

      private readonly IDictionary<CILAssemblyName, Tuple<CILAssemblyName, Int32>> _assRefDic;
      private readonly IList<UInt32> _aRefHashes;

      private readonly StringHeapEmittingInfo _userStrings;
      private readonly StringHeapEmittingInfo _sysStrings;
      private readonly BLOBContainer _blob;

      private readonly EmittingArguments _emittingArgs;

      private readonly HashStreamLoadEventArgs _hashStreamArgs;
      private readonly CILAssemblyName _assemblyName;

      private readonly Encoding _mdStringEncoding;

      private readonly IList<Tuple<Int32, Int32>> _typeDefFieldAndMethodIndices;
      private readonly IList<Int32> _methodDefParamIndices;
      private readonly IList<Int32> _gParamOwners;
      private readonly IList<Int32> _fieldsWithRVA;

      private readonly EmittingAssemblyMapper _assemblyMapper;

      internal MetaDataWriter( EmittingArguments eArgs, CILReflectionContextImpl ctx, CILModule module, EmittingAssemblyMapper assemblyMapper, out IList<CILMethodBase> allMethodDefs, out CILAssemblyName an )
      {
         this._context = ctx;
         this._module = module;
         this._mdStringEncoding = new UTF8Encoding( false, true );
         this._assemblyMapper = assemblyMapper;

         this._emittingArgs = eArgs;

         this._assRefDic = new Dictionary<CILAssemblyName, Tuple<CILAssemblyName, Int32>>( ComparerFromFunctions.NewEqualityComparer<CILAssemblyName>( ( n1, n2 ) => n1.CorePropertiesEqual( n2 ), n1 => n1 == null ? 0 : n1.GetHashCode() ) );
         this._aRefHashes = new List<UInt32>();

         this._userStrings = new StringHeapEmittingInfo( MetaDataConstants.USER_STRING_ENCODING, true, ( str, array, idx ) =>
         {
            var oldIdx = idx;
            byte lastByte = 0;
            for ( var i = 0; i < str.Length; ++i )
            {
               var chr = str[i];
               array.WriteUInt16LEToBytes( ref idx, chr );
               // ECMA-335, p. 272
               if ( lastByte == 0 &&
                ( chr > 0x7E
                     || ( chr <= 0x2D
                        && ( ( chr >= 0x01 && chr <= 0x08 )
                           || ( chr >= 0x0E && chr <= 0x1F )
                           || chr == 0x27 || chr == 0x2D ) )
                     ) )
               {
                  lastByte = 1;
               }
            }
            array[idx++] = lastByte;
            return idx - oldIdx;
         } );
         this._sysStrings = new StringHeapEmittingInfo( MetaDataConstants.SYS_STRING_ENCODING, false, null );
         this._blob = new BLOBContainer();

         this._typeDef = new TableEmittingInfoWithList<CILType>( Tables.TypeDef );
         this._fieldDef = new TableEmittingInfoWithList<CILField>( Tables.Field, ( field, info ) => this.WriteFieldSignature( info, field ), this._blob );
         this._methodDef = new TableEmittingInfoWithList<CILMethodBase>( Tables.MethodDef, ( method, info ) => this.WriteMethodSignature( info, method ), this._blob );
         this._paramDef = new TableEmittingInfoWithList<CILParameter>( Tables.Parameter );
         this._gParamDef = new TableEmittingInfoWithList<CILTypeParameter>( Tables.GenericParameter );
         this._typeRef = new TableEmittingInfoWithList<Tuple<Int32, String, String>>( Tables.TypeRef );
         this._typeSpec = new TableEmittingInfoWithSignature<CILTypeBase>( Tables.TypeSpec, this._blob, ( info, additionalInfo, type ) => this.WriteTypeSignature( info, type ) );
         this._methodSpec = new TableEmittingInfoWithSignatureAndForeignKey<CILMethod, Int32>( Tables.MethodSpec, this._blob,
            assemblyMapper == null ? new Func<CILMethod, Object, Int32>( ( method, additionalInfo ) => this.GetTokenFor( method.GenericDefinition, false ) ) : ( method, additionalInfo ) => this.GetTokenFor( assemblyMapper.MapMethodBase( method.GenericDefinition ), false ),
            assemblyMapper == null ? new Action<BinaryHeapEmittingInfo, CILMethod>( ( info, method ) => this.WriteMethodSpecSignature( info, method ) ) : ( info, method ) => this.WriteMethodSpecSignature( info, (CILMethod) assemblyMapper.MapMethodBase( method ) ),
            true );
         this._memberRef = new TableEmittingInfoWithSignatureAndForeignKey<CILElementOwnedByType, Tuple<Int32, String>>( Tables.MemberRef, this._blob,
            assemblyMapper == null ? new Func<CILElementOwnedByType, Object, Tuple<Int32, String>>( ( obj, additionalInfo ) => Tuple.Create( this.GetTokenFor( obj.DeclaringType, (Boolean) additionalInfo ), obj is CILField ? ( (CILElementWithSimpleName) obj ).Name : ( (CILMethodBase) obj ).GetName() ) ) : ( obj, additionalInfo ) => Tuple.Create( this.GetTokenFor( assemblyMapper.MapTypeBase( obj.DeclaringType ), (Boolean) additionalInfo ), obj is CILField ? ( (CILElementWithSimpleName) obj ).Name : ( (CILMethodBase) obj ).GetName() ),
            ( info, obj ) =>
            {
               if ( obj.DeclaringType.IsGenericType() )
               {
                  obj = ( (CILElementOwnedByChangeableType<CILElementOwnedByType>) obj ).ChangeDeclaringType( obj.DeclaringType.GenericDefinition.GenericArguments.ToArray() );
               }
               if ( obj is CILField )
               {
                  this.WriteFieldSignature( info, (CILField) obj );
               }
               else
               {
                  this.WriteMethodSignature( info, (CILMethodBase) obj );
               }
            }, true );
         this._standaloneSig = new TableEmittingInfoWithSignature<Object>( Tables.StandaloneSignature, this._blob, ( info, additionalInfo, locals ) =>
            {
               if ( locals is LocalBuilder[] )
               {
                  this.WriteLocalsSignature( info, (LocalBuilder[]) locals );
               }
               else
               {
                  // Method sig
                  this.WriteMethodSignature( info, (CILMethodSignature) locals, (Tuple<CILCustomModifier[], CILTypeBase>[]) additionalInfo );
               }
            }, ComparerFromFunctions.NewEqualityComparer<Object>( ( o1, o2 ) =>
            {
               return LOCAL_BUILDERS_EQ_COMP.Equals( o1 as LocalBuilder[], o2 as LocalBuilder[] ) || Object.Equals( o1, o2 );
            }, o =>
            {
               return o is LocalBuilder[] ? LOCAL_BUILDERS_EQ_COMP.GetHashCode( o as LocalBuilder[] ) : ( o == null ? 0 : o.GetHashCode() );
            } ) );
         this._assembly = new TableEmittingInfoWithList<CILAssemblyName>( Tables.Assembly );
         this._interfaceImpl = new List<Tuple<Int32, Int32>>();
         this._eventMap = new List<Tuple<Int32, Int32>>();
         this._eventDef = new TableEmittingInfoWithList<CILEvent>( Tables.Event );
         this._propertyMap = new List<Tuple<Int32, Int32>>();
         this._propertyDef = new TableEmittingInfoWithList<CILProperty>( Tables.Property, ( prop, info ) => this.WritePropertySignature( info, prop ), this._blob );
         this._nestedClass = new List<Tuple<Int32, Int32>>();
         this._moduleDef = new TableEmittingInfoWithList<CILModule>( Tables.Module );
         this._gParamConstraints = new List<Tuple<Int32, Int32>>();
         this._constants = new List<Tuple<Int16, Int32, UInt32>>();
         this._customAttributes = new List<Tuple<Int32, Int32, UInt32>>();
         this._methodSemantics = new List<Tuple<UInt16, Int32, Int32>>();
         this._methodImpl = new List<Tuple<Int32, Int32, Int32>>();
         this._implMap = new List<Tuple<PInvokeAttributes, Int32, UInt32, Int32>>();
         this._moduleRef = new TableEmittingInfoWithList<String>( (Tables) 0 ); // Set table mask to zero in order for 'tokens' to be just table indices.
         this._assemblyRef = new List<CILAssemblyName>();
         this._fieldLayout = new List<Tuple<Int32, Int32>>();
         this._classLayout = new List<Tuple<Int16, Int32, Int32>>();
         this._marshals = new List<Tuple<Int32, UInt32>>();
         this._files = new Dictionary<String, Tuple<Int32, FileAttributes, UInt32, UInt32>>();
         this._exportedTypes = new List<Tuple<TypeAttributes, Int32, String, String, Int32>>();
         this._declSecurity = new List<Tuple<UInt16, Int32, UInt32>>();
         this._guids = new List<Guid>();


         // Check whether we need callback to create public key tokens
         this._assemblyName = new CILAssemblyName( module.Assembly.Name );
         an = this._assemblyName;
         if ( !eArgs.UseFullPublicKeyInAssemblyReferences )
         {
            this._hashStreamArgs = GetArgsForPublicKeyTokenComputing( ctx );
         }

         // Start by generating typeDef, methodDef, fieldDef, paramDef -tables, since they need to be ready and in specific order when emitting methods begins.
         var tlTypes = this._module.DefinedTypes
            .SelectMany( t => t.AsDepthFirstEnumerable( tt => tt.DeclaredNestedTypes ) )
            .ToArray();


         // Process type defs. Remember that enclosing type must precede any nested types
         this._typeDefFieldAndMethodIndices = new List<Tuple<Int32, Int32>>();
         this._methodDefParamIndices = new List<Int32>();
         this._gParamOwners = new List<Int32>();
         this._fieldsWithRVA = new List<Int32>();

         var mInit = this._module.ModuleInitializer;

         this.ProcessDefinedType( mInit, assemblyMapper );
         foreach ( var tlType in tlTypes )
         {
            this.ProcessDefinedType( tlType, assemblyMapper );
         }

         this.PostProcessDefinedType( mInit, assemblyMapper );
         foreach ( var tlType in tlTypes )
         {
            this.PostProcessDefinedType( tlType, assemblyMapper );
         }

         // Process generic parameters
         this.ProcessAllGParams();
         allMethodDefs = this._methodDef._list;

         this._methodLocalSigs = new Int32[allMethodDefs.Count];
         // ECMA-335, pp. 206-207
      }

      internal static HashStreamLoadEventArgs GetArgsForPublicKeyTokenComputing( CILReflectionContextImpl ctx )
      {
         var hashStreamArgs = new HashStreamLoadEventArgs( AssemblyHashAlgorithm.SHA1 );// this._assemblyName.HashAlgorithm );
         ctx.LaunchHashStreamEvent( hashStreamArgs );
         if ( hashStreamArgs.ComputeHash == null )
         {
            throw new InvalidOperationException( "The callback to compute public key tokens was not specified in " + typeof( CILReflectionContext ).GetEvent( "HashStreamLoadEvent" ) + " event." );
         }
         return hashStreamArgs;
      }

      internal Int32 GetTokenFor( CILTypeBase type, Boolean useRefForGDef )
      {
         Int32 result;
         if ( ( type.IsGenericType() && ( !useRefForGDef || !type.IsGenericTypeDefinition() ) )
            || TypeKind.Type != type.TypeKind
            || type.GetElementKind().HasValue
            )
         {
            // This must be in type-spec
            result = this._typeSpec.GetOrAddTokenFor( type );
         }
         else if ( !this._typeDef.TryGetToken( (CILType) type, out result ) )
         {
            // This must be in type-ref
            var tc = (CILType) type;
            result = this._typeRef.GetOrAddTokenFor( this.TupleForTypeRef( tc ), () =>
            {
               this._sysStrings.GetOrAddString( tc.Name, false );
               this._sysStrings.GetOrAddString( tc.Namespace, false );
            } );
         }
         return result;
      }

      private Tuple<Int32, String, String> TupleForTypeRef( CILType type )
      {
         Int32 resScopeToken;
         if ( type.DeclaringType == null )
         {
            var isWithinThisAssembly = Object.Equals( type.Module.Assembly, this._module.Assembly );
            resScopeToken = TokenUtils.EncodeToken( isWithinThisAssembly ? Tables.ModuleRef : Tables.AssemblyRef, isWithinThisAssembly ? this._moduleRef.GetOrAddTokenFor( type.Module.Name ) : this.GetAssemblyRefInfoAssemblyRef( type.Module.Assembly.Name ).Item2 + 1 );
         }
         else
         {
            resScopeToken = this.GetTokenFor( type.DeclaringType, true );
         }

         return Tuple.Create( resScopeToken, type.Name, type.Namespace );
      }

      internal Int32 GetTokenFor( CILMethodBase method, Boolean useRefForDeclTypeGDef )
      {
         Int32 result;
         var type = method.DeclaringType;
         if ( ( type.IsGenericType() && ( !useRefForDeclTypeGDef || !type.IsGenericTypeDefinition() ) )
            || !this._methodDef.TryGetToken( method, out result ) )
         {
            var rMethod = method as CILMethod;
            if ( rMethod == null
               || !rMethod.HasGenericArguments()
               || rMethod.IsGenericMethodDefinition() )
            {
               // This must be in member-ref
               result = this._memberRef.GetOrAddTokenFor( method, () => this._sysStrings.GetOrAddString( method.GetName(), false ), useRefForDeclTypeGDef );
            }
            else
            {
               // This really belongs to method-spec
               result = this._methodSpec.GetOrAddTokenFor( (CILMethod) method, useRefForDeclTypeGDef );
            }
         }
         return result;
      }

      internal Int32 GetTokenFor( CILField field, Boolean useRefForDeclTypeGDef )
      {
         Int32 result;
         var type = field.DeclaringType;
         if ( ( type.IsGenericType() && ( !useRefForDeclTypeGDef || !type.IsGenericTypeDefinition() ) )
            || !this._fieldDef.TryGetToken( field, out result ) )
         {
            // This must be in member-ref
            result = this._memberRef.GetOrAddTokenFor( field, () => this._sysStrings.GetOrAddString( field.Name, false ), useRefForDeclTypeGDef );
         }
         return result;
      }

      internal Int32 GetTokenFor( String str )
      {
         return (Int32) ( this._userStrings.GetOrAddString( str, true ) | USER_STRING_MASK );
      }

      internal Int32 GetSignatureTokenFor( CILMethodSignature method, Tuple<CILCustomModifier[], CILTypeBase>[] varArgs )
      {
         return this._standaloneSig.GetOrAddTokenFor( method, varArgs );
      }

      internal Int32 GetSignatureTokenFor( CILMethodBase method, LocalBuilder[] locals )
      {
         var retVal = this._standaloneSig.GetOrAddTokenFor( locals );
         this._methodLocalSigs[this._methodDef.GetTableIndexOf( method ) - 1] = retVal;
         return retVal;
      }

      private Int32? TokenEncoderCallback( CILElementTokenizableInILCode element )
      {
         Int32 result = 0;
         Boolean tokenExisted = false;
         switch ( element.ElementTypeKind )
         {
            case CILElementWithinILCode.Type:
               switch ( ( (CILTypeBase) element ).TypeKind )
               {
                  case TypeKind.MethodSignature:
                     tokenExisted = this._standaloneSig.TryGetToken( element, out result );
                     break;
                  case TypeKind.Type:
                     tokenExisted = this._typeDef.TryGetToken( (CILType) element, out result ) || this._typeRef.TryGetToken( this.TupleForTypeRef( (CILType) element ), out result ) || this._typeSpec.TryGetToken( (CILType) element, out result );
                     break;
                  case TypeKind.TypeParameter:
                     tokenExisted = this._typeSpec.TryGetToken( (CILTypeBase) element, out result );
                     break;
               }
               break;
            case CILElementWithinILCode.Field:
               tokenExisted = this._fieldDef.TryGetToken( (CILField) element, out result ) || this._memberRef.TryGetToken( (CILField) element, out result );
               break;
            case CILElementWithinILCode.Method:
               tokenExisted = this._methodDef.TryGetToken( (CILMethodBase) element, out result ) || this._memberRef.TryGetToken( (CILMethodBase) element, out result ) || ( element is CILMethod && this._methodSpec.TryGetToken( (CILMethod) element, out result ) );
               break;
         }
         return tokenExisted ? (Int32?) result : null;
      }

      private Int32? TokenSignatureEncoderCallback( CILMethodBase method )
      {
         Int32? retVal;
         Int32 idx;
         if ( this._methodDef._table.TryGetValue( method, out idx ) )
         {
            retVal = this._methodLocalSigs[idx - 1];
            if ( retVal == 0 )
            {
               // No method body.
               retVal = null;
            }
         }
         else
         {
            retVal = null;
         }
         return retVal;
      }

      // This assumes that sink offset is at multiple of 4.
      internal UInt32 WriteMetaData(
         Stream sink,
         TRVA currentRVA,
         EmittingArguments eArgs,
         IDictionary<CILMethodBase, TRVA> methodRVAs,
         IDictionary<String, UInt32> embeddedManifestResourceOffsets,
         out UInt32 addedToOffsetBeforeMD
         )
      {
         // First, write all the data of fields with RVA
         IList<TRVA> fRVAs;
         addedToOffsetBeforeMD = 0;
         if ( this._fieldsWithRVA.Any() )
         {
            fRVAs = new List<TRVA>( this._fieldsWithRVA.Count );
            foreach ( var fIdx in this._fieldsWithRVA )
            {
               var field = this._fieldDef._list[fIdx - 1];
               fRVAs.Add( currentRVA );
               foreach ( var bytee in field.InitialValue )
               {
                  sink.WriteByte( bytee );
                  ++currentRVA;
                  ++addedToOffsetBeforeMD;
               }
            }
            // Align data to 4
            addedToOffsetBeforeMD += sink.SkipToNextAlignment( ref currentRVA, 4 );
         }
         else
         {
            fRVAs = null;
         }

         // Actual meta-data
         var metaDataVersion = eArgs.MetaDataVersion;
         var versionStringSize = this._mdStringEncoding.GetByteCount( metaDataVersion ) + 1;
         if ( versionStringSize > MD_MAX_VERSION_LENGTH )
         {
            throw new ArgumentException( "Metadata version must be at maximum " + MD_MAX_VERSION_LENGTH + " bytes long after encoding it using " + this._mdStringEncoding + "." );
         }

         // First fill in all the missing information after emitting method bodies
         var typeDefExtends = new List<Int32>( this._typeDef.Count ); var evtTypes = new List<Int32>( this._eventDef.Count ); var moduleResRefIndices = new Dictionary<String, Int32>();
         this.FinalizeMetaData( eArgs, typeDefExtends, evtTypes, moduleResRefIndices );

         // Then write the data to the byte sink
         // ECMA-335, pp. 271-272
         var streamHeaders = new Int32[BLOB_IDX + 1];
         var streamSizes = new UInt32[BLOB_IDX + 1];

         var hasSysStrings = this._sysStrings.Accessed;
         var hasUserStrings = this._userStrings.Accessed;
         var hasGuids = this._guids.Count > 0;
         var hasBlobs = this._blob.Accessed;

         if ( hasSysStrings )
         {
            // Store offset to array to streamHeaders
            // This offset, for each stream, tells where to write first field of stream header (offset from metadata root)
            streamHeaders[SYS_STRINGS_IDX] = 8 + BitUtils.MultipleOf4( STREAM_NAMES[SYS_STRINGS_IDX].Length + 1 );
            streamSizes[SYS_STRINGS_IDX] = BitUtils.MultipleOf4( this._sysStrings.Size );
         }
         if ( hasUserStrings )
         {
            streamHeaders[USER_STRINGS_IDX] = 8 + BitUtils.MultipleOf4( STREAM_NAMES[USER_STRINGS_IDX].Length + 1 );
            streamSizes[USER_STRINGS_IDX] = BitUtils.MultipleOf4( this._userStrings.Size );
         }
         if ( hasGuids )
         {
            streamHeaders[GUID_IDX] = 8 + BitUtils.MultipleOf4( STREAM_NAMES[GUID_IDX].Length + 1 );
            streamSizes[GUID_IDX] = BitUtils.MultipleOf4( ( (UInt32) this._guids.Count ) * Consts.GUID_SIZE );
         }
         if ( hasBlobs )
         {
            streamHeaders[BLOB_IDX] = 8 + BitUtils.MultipleOf4( STREAM_NAMES[BLOB_IDX].Length + 1 );
            streamSizes[BLOB_IDX] = BitUtils.MultipleOf4( this._blob.Size );
         }

         var wideSysStringIndex = streamSizes[SYS_STRINGS_IDX] > UInt16.MaxValue;
         var wideGUIDIndex = streamSizes[GUID_IDX] > UInt16.MaxValue;
         var wideBLOBIndex = streamSizes[BLOB_IDX] > UInt16.MaxValue;
         var sysStringIndexSize = wideSysStringIndex ? 4 : 2;
         var guidIndexSize = wideGUIDIndex ? 4 : 2;
         var blobIndexSize = wideBLOBIndex ? 4 : 2;

         var tableSizes = new UInt32[TablesUtils.AMOUNT_OF_TABLES];
         tableSizes[(Int32) Tables.Module] = (UInt32) this._moduleDef.Count;
         tableSizes[(Int32) Tables.TypeRef] = (UInt32) this._typeRef.Count;
         tableSizes[(Int32) Tables.TypeDef] = (UInt32) this._typeDef.Count;
         tableSizes[(Int32) Tables.Field] = (UInt32) this._fieldDef.Count;
         tableSizes[(Int32) Tables.MethodDef] = (UInt32) this._methodDef.Count;
         tableSizes[(Int32) Tables.Parameter] = (UInt32) this._paramDef.Count;
         tableSizes[(Int32) Tables.InterfaceImpl] = (UInt32) this._interfaceImpl.Count;
         tableSizes[(Int32) Tables.MemberRef] = (UInt32) this._memberRef.Count;
         tableSizes[(Int32) Tables.Constant] = (UInt32) this._constants.Count;
         tableSizes[(Int32) Tables.CustomAttribute] = (UInt32) this._customAttributes.Count;
         tableSizes[(Int32) Tables.FieldMarshal] = (UInt32) this._marshals.Count;
         tableSizes[(Int32) Tables.DeclSecurity] = (UInt32) this._declSecurity.Count;
         tableSizes[(Int32) Tables.ClassLayout] = (UInt32) this._classLayout.Count;
         tableSizes[(Int32) Tables.FieldLayout] = (UInt32) this._fieldLayout.Count;
         tableSizes[(Int32) Tables.StandaloneSignature] = (UInt32) this._standaloneSig.Count;
         tableSizes[(Int32) Tables.EventMap] = (UInt32) this._eventMap.Count;
         tableSizes[(Int32) Tables.Event] = (UInt32) this._eventDef.Count;
         tableSizes[(Int32) Tables.PropertyMap] = (UInt32) this._propertyMap.Count;
         tableSizes[(Int32) Tables.Property] = (UInt32) this._propertyDef.Count;
         tableSizes[(Int32) Tables.MethodSemantics] = (UInt32) this._methodSemantics.Count;
         tableSizes[(Int32) Tables.MethodImpl] = (UInt32) this._methodImpl.Count;
         tableSizes[(Int32) Tables.ModuleRef] = (UInt32) this._moduleRef.Count;
         tableSizes[(Int32) Tables.TypeSpec] = (UInt32) this._typeSpec.Count;
         tableSizes[(Int32) Tables.ImplMap] = (UInt32) this._implMap.Count;
         tableSizes[(Int32) Tables.FieldRVA] = (UInt32) this._fieldsWithRVA.Count;
         tableSizes[(Int32) Tables.Assembly] = (UInt32) this._assembly.Count;
         tableSizes[(Int32) Tables.AssemblyRef] = (UInt32) this._assemblyRef.Count;
         tableSizes[(Int32) Tables.File] = (UInt32) this._files.Count;
         tableSizes[(Int32) Tables.ExportedType] = (UInt32) this._exportedTypes.Count;
         tableSizes[(Int32) Tables.ManifestResource] = (UInt32) ( embeddedManifestResourceOffsets.Count + moduleResRefIndices.Count );
         tableSizes[(Int32) Tables.NestedClass] = (UInt32) this._nestedClass.Count;
         tableSizes[(Int32) Tables.GenericParameter] = (UInt32) this._gParamDef.Count;
         tableSizes[(Int32) Tables.MethodSpec] = (UInt32) this._methodSpec.Count;
         tableSizes[(Int32) Tables.GenericParameterConstraint] = (UInt32) this._gParamConstraints.Count;

         var tableWidths = new Int32[tableSizes.Length];
         for ( var i = 0; i < tableWidths.Length; ++i )
         {
            if ( tableSizes[i] > 0 )
            {
               tableWidths[i] = MetaDataConstants.CalculateTableWidth( (Tables) i, tableSizes, sysStringIndexSize, guidIndexSize, blobIndexSize );
            }
         }

         var tRefWidths = MetaDataConstants.GetCodedTableIndexSizes( tableSizes );

         var versionStringSize4 = BitUtils.MultipleOf4( versionStringSize );
         var mdHeaderSize = 24 + 4 * (UInt32) tableSizes.Count( size => size > 0 );
         streamHeaders[MD_IDX] = 8 + BitUtils.MultipleOf4( STREAM_NAMES[MD_IDX].Length );
         var mdStreamSize = mdHeaderSize + tableSizes.Select( ( size, idx ) => (UInt32) size * (UInt32) tableWidths[idx] ).Sum();
         var mdStreamSize4 = BitUtils.MultipleOf4( mdStreamSize );
         streamSizes[MD_IDX] = mdStreamSize4;

         var anArray = new Byte[16 // Header start
            + versionStringSize4 // Version string
            + 4 // Header end
            + streamHeaders.Sum() // Stream headers
            + mdHeaderSize
            ];
         // Metadata root
         var offset = 0;
         anArray.WriteUInt32LEToBytes( ref offset, MD_SIGNATURE )
            .WriteUInt16LEToBytes( ref offset, MD_MAJOR )
            .WriteUInt16LEToBytes( ref offset, MD_MINOR )
            .WriteUInt32LEToBytes( ref offset, MD_RESERVED )
            .WriteInt32LEToBytes( ref offset, versionStringSize4 )
            .WriteStringToBytes( ref offset, this._mdStringEncoding, metaDataVersion )
            .Skip( ref offset, versionStringSize4 - versionStringSize + 1 )
            .WriteUInt16LEToBytes( ref offset, MD_FLAGS )
            .WriteUInt16LEToBytes( ref offset, (UInt16) streamHeaders.Count( stream => stream > 0 ) )
            // #~ header
            .WriteInt32LEToBytes( ref offset, offset + streamHeaders.Sum() )
            .WriteUInt32LEToBytes( ref offset, mdStreamSize4 )
            .WriteStringToBytes( ref offset, this._mdStringEncoding, STREAM_NAMES[MD_IDX] )
            .Skip( ref offset, 4 - ( offset % 4 ) );

         if ( hasSysStrings )
         {
            // #String header
            anArray.WriteUInt32LEToBytes( ref offset, (UInt32) offset + (UInt32) streamHeaders.Skip( SYS_STRINGS_IDX ).Sum() + streamSizes.Take( SYS_STRINGS_IDX ).Sum() )
               .WriteUInt32LEToBytes( ref offset, streamSizes[SYS_STRINGS_IDX] )
               .WriteStringToBytes( ref offset, this._mdStringEncoding, STREAM_NAMES[SYS_STRINGS_IDX] )
               .Skip( ref offset, 4 - ( offset % 4 ) );
         }

         if ( hasUserStrings )
         {
            // #US header
            anArray.WriteUInt32LEToBytes( ref offset, (UInt32) offset + (UInt32) streamHeaders.Skip( USER_STRINGS_IDX ).Sum() + streamSizes.Take( USER_STRINGS_IDX ).Sum() )
               .WriteUInt32LEToBytes( ref offset, streamSizes[USER_STRINGS_IDX] )
               .WriteStringToBytes( ref offset, this._mdStringEncoding, STREAM_NAMES[USER_STRINGS_IDX] )
               .Skip( ref offset, 4 - ( offset % 4 ) );
         }

         if ( hasGuids )
         {
            // #Guid header
            anArray.WriteUInt32LEToBytes( ref offset, (UInt32) offset + (UInt32) streamHeaders.Skip( GUID_IDX ).Sum() + streamSizes.Take( GUID_IDX ).Sum() )
               .WriteUInt32LEToBytes( ref offset, streamSizes[GUID_IDX] )
               .WriteStringToBytes( ref offset, this._mdStringEncoding, STREAM_NAMES[GUID_IDX] )
               .Skip( ref offset, 4 - ( offset % 4 ) );
         }

         if ( hasBlobs )
         {
            // #Blob header
            anArray.WriteUInt32LEToBytes( ref offset, (UInt32) offset + (UInt32) streamHeaders.Skip( BLOB_IDX ).Sum() + streamSizes.Take( BLOB_IDX ).Sum() )
               .WriteUInt32LEToBytes( ref offset, streamSizes[BLOB_IDX] )
               .WriteStringToBytes( ref offset, this._mdStringEncoding, STREAM_NAMES[BLOB_IDX] )
               .Skip( ref offset, 4 - ( offset % 4 ) );
         }

         // Write the end of the header
         // Header (ECMA-335, p. 273)
         var validBitvector = 0L;
         for ( var i = tableSizes.Length - 1; i >= 0; --i )
         {
            validBitvector = validBitvector << 1;
            if ( tableSizes[i] > 0 )
            {
               validBitvector |= 1;
            }
         }
         anArray.WriteInt32LEToBytes( ref offset, TABLE_STREAM_RESERVED )
            .WriteByteToBytes( ref offset, eArgs.TableHeapMajor )
            .WriteByteToBytes( ref offset, eArgs.TableHeapMinor )
            .WriteByteToBytes( ref offset, (Byte) ( Convert.ToInt32( wideSysStringIndex ) | ( Convert.ToInt32( wideGUIDIndex ) << 1 ) | ( Convert.ToInt32( wideBLOBIndex ) << 2 ) ) )
            .WriteByteToBytes( ref offset, TABLE_STREAM_RESERVED_2 )
            .WriteInt64LEToBytes( ref offset, validBitvector )
            .WriteInt64LEToBytes( ref offset, SORTED_TABLES );
         for ( var i = 0; i < tableSizes.Length; ++i )
         {
            if ( tableSizes[i] > 0 )
            {
               anArray.WriteUInt32LEToBytes( ref offset, tableSizes[i] );
            }
         }

#if DEBUG
         if ( offset != anArray.Length )
         {
            throw new Exception( "Debyyg" );
         }
#endif

         // Write the full CLI header
         sink.Write( anArray );

         // Then, write the binary representation of the tables
         // ECMA-335, p. 239
         ForEachRow( this._moduleDef, tableWidths, sink, ( array, idx, listIdx, blobIdx, module ) => array
            .WriteInt16LEToBytes( ref idx, 0 ) // Generation
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( module.Name ), wideSysStringIndex ) // Name
            .WriteHeapIndex( ref idx, 1, wideGUIDIndex ) // MvId
            .WriteHeapIndex( ref idx, 0, wideGUIDIndex ) // EncId
            .WriteHeapIndex( ref idx, 0, wideGUIDIndex ) // EncBaseId
            );
         // ECMA-335, p. 247
         // TypeRef may contain types which result in duplicate rows - avoid that
         ForEachRow( this._typeRef, tableWidths, sink, ( array, idx, listIdx, blobIdx, typeRef ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.ResolutionScope, MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.ResolutionScope, typeRef.Item1 ), tRefWidths ) // ResolutionScope
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( typeRef.Item2 ), wideSysStringIndex ) // TypeName
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( typeRef.Item3 ), wideSysStringIndex ) // TypeNamespace
            );
         // ECMA-335, p. 243
         ForEachRow( this._typeDef, tableWidths, sink, ( array, idx, listIdx, blobIdx, typeDef ) => array
            .WriteInt32LEToBytes( ref idx, (Int32) ( (CILType) typeDef ).Attributes ) // Flags
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( typeDef.Name ), wideSysStringIndex ) // TypeName
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( typeDef.Namespace ), wideSysStringIndex ) // TypeNamespace
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeDefOrRef, typeDefExtends[listIdx], tRefWidths ) // Extends
            .WriteSimpleTableIndex( ref idx, Tables.Field, this._typeDefFieldAndMethodIndices[listIdx].Item1, tableSizes ) // FieldList
            .WriteSimpleTableIndex( ref idx, Tables.MethodDef, this._typeDefFieldAndMethodIndices[listIdx].Item2, tableSizes ) // MethodList
            );
         // ECMA-335, p. 223
         ForEachRow( this._fieldDef, tableWidths, sink, ( array, idx, listIdx, blobIdx, fDef ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) fDef.Attributes ) // FieldAttributes
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( fDef.Name ), wideSysStringIndex ) // Name
            .WriteHeapIndex( ref idx, blobIdx, wideBLOBIndex ) // Signature
            );
         // ECMA-335, p. 233
         ForEachRow( this._methodDef, tableWidths, sink, ( array, idx, listIdx, blobIdx, mDef ) => array
            .WriteUInt32LEToBytes( ref idx, methodRVAs.GetOrDefault( mDef, 0u ) ) // RVA
            .WriteInt16LEToBytes( ref idx, (Int16) mDef.ImplementationAttributes ) // ImplFlags
            .WriteInt16LEToBytes( ref idx, (Int16) mDef.Attributes ) // Flags
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( mDef.GetName() ), wideSysStringIndex ) // Name
            .WriteHeapIndex( ref idx, blobIdx, wideBLOBIndex ) // Signature
            .WriteSimpleTableIndex( ref idx, Tables.Parameter, this._methodDefParamIndices[listIdx], tableSizes )
            );
         // ECMA-335, p. 240
         ForEachRow( this._paramDef, tableWidths, sink, ( array, idx, listIdx, blobIdx, pDef ) => array
            .WriteInt16LEToBytes( ref idx, (Int16) pDef.Attributes ) // Flags
            .WriteUInt16LEToBytes( ref idx, (UInt16) ( pDef.Position + 1 ) ) // Sequence
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( pDef.Name ), wideSysStringIndex ) // Name
            );
         // ECMA-335, p. 231
         ForEachElement( Tables.InterfaceImpl, this._interfaceImpl, tableWidths, sink, ( array, idx, listIdx, item ) => array
            .WriteSimpleTableIndex( ref idx, Tables.TypeDef, item.Item1, tableSizes ) // Class
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeDefOrRef, item.Item2, tRefWidths ) // Interface
            );
         // ECMA-335, p. 232
         ForEachRow( this._memberRef, tableWidths, sink, ( array, idx, listIdx, blobIdx, mRef ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MemberRefParent, MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.MemberRefParent, this._memberRef._keys[listIdx].Item1 ), tRefWidths ) // Class
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( mRef is CILElementWithSimpleName ? ( (CILElementWithSimpleName) mRef ).Name : ( (CILConstructor) mRef ).GetName() ), wideSysStringIndex ) // Name
            .WriteHeapIndex( ref idx, blobIdx, wideBLOBIndex ) // Signature
            );
         // ECMA-335, p. 216
         ForEachElement( Tables.Constant, this._constants, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteInt16LEToBytes( ref idx, tuple.Item1 )
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasConstant, tuple.Item2, tRefWidths )
            .WriteHeapIndex( ref idx, tuple.Item3, wideBLOBIndex )
            );
         // ECMA-335, p. 216
         ForEachElement( Tables.CustomAttribute, this._customAttributes, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasCustomAttribute, tuple.Item1, tRefWidths ) // Parent
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.CustomAttributeType, tuple.Item2, tRefWidths ) // Type
            .WriteHeapIndex( ref idx, tuple.Item3, wideBLOBIndex )
            );
         // ECMA-335, p.226
         ForEachElement( Tables.FieldMarshal, this._marshals, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasFieldMarshal, tuple.Item1, tRefWidths ) // Parent
            .WriteHeapIndex( ref idx, tuple.Item2, wideBLOBIndex ) // NativeType
            );
         // ECMA-335, p. 218
         ForEachElement( Tables.DeclSecurity, this._declSecurity, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteUInt16LEToBytes( ref idx, tuple.Item1 ) // Action
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasDeclSecurity, tuple.Item2, tRefWidths ) // Parent
            .WriteHeapIndex( ref idx, tuple.Item3, wideBLOBIndex ) // PermissionSet
            );
         // ECMA-335 p. 215
         ForEachElement( Tables.ClassLayout, this._classLayout, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteInt16LEToBytes( ref idx, tuple.Item1 ) // PackingSize
            .WriteInt32LEToBytes( ref idx, tuple.Item2 ) // ClassSize
            .WriteSimpleTableIndex( ref idx, Tables.TypeDef, tuple.Item3, tableSizes ) // Parent
            );
         // ECMA-335 p. 225
         ForEachElement( Tables.FieldLayout, this._fieldLayout, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteInt32LEToBytes( ref idx, tuple.Item1 ) // Offset
            .WriteSimpleTableIndex( ref idx, Tables.Field, tuple.Item2, tableSizes ) // Field
            );
         // ECMA-335 p. 243
         ForEachRow( this._standaloneSig, tableWidths, sink, ( array, idx, listIdx, blobIdx, sig ) => array
            .WriteHeapIndex( ref idx, blobIdx, wideBLOBIndex ) // Signature
            );
         // ECMA-335 p. 220
         ForEachElement( Tables.EventMap, this._eventMap, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteSimpleTableIndex( ref idx, Tables.TypeDef, tuple.Item1, tableSizes ) // Parent
            .WriteSimpleTableIndex( ref idx, Tables.Event, tuple.Item2, tableSizes ) // EventList
            );
         // ECMA-335 p. 221
         ForEachRow( this._eventDef, tableWidths, sink, ( array, idx, listIdx, blobIdx, evt ) => array
            .WriteUInt16LEToBytes( ref idx, (UInt16) evt.Attributes ) // EventFlags
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( evt.Name ), wideSysStringIndex ) // Name
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeDefOrRef, evtTypes[listIdx], tRefWidths ) // EventType
            );
         // ECMA-335 p. 242
         ForEachElement( Tables.PropertyMap, this._propertyMap, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteSimpleTableIndex( ref idx, Tables.TypeDef, tuple.Item1, tableSizes ) // Parent
            .WriteSimpleTableIndex( ref idx, Tables.Property, tuple.Item2, tableSizes ) // PropertyList
            );
         // ECMA-335 p. 242
         ForEachRow( this._propertyDef, tableWidths, sink, ( array, idx, listIdx, blobIdx, prop ) => array
            .WriteUInt16LEToBytes( ref idx, (UInt16) prop.Attributes ) // Flags
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( prop.Name ), wideSysStringIndex ) // Name
            .WriteHeapIndex( ref idx, blobIdx, wideBLOBIndex ) // Type
            );
         // ECMA-335 p. 237
         ForEachElement( Tables.MethodSemantics, this._methodSemantics, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteUInt16LEToBytes( ref idx, tuple.Item1 ) // Semantics
            .WriteSimpleTableIndex( ref idx, Tables.MethodDef, tuple.Item2, tableSizes ) // Method
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.HasSemantics, tuple.Item3, tRefWidths ) // Association
            );
         // ECMA-335 p. 237
         ForEachElement( Tables.MethodImpl, this._methodImpl, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteSimpleTableIndex( ref idx, Tables.TypeDef, tuple.Item1, tableSizes ) // Class
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MethodDefOrRef, tuple.Item2, tRefWidths ) // MethodBody
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MethodDefOrRef, tuple.Item3, tRefWidths ) // MethodDeclaration
            );
         // ECMA-335, p. 239
         ForEachElement( Tables.ModuleRef, this._moduleRef._list, tableWidths, sink, ( array, idx, listIdx, mRef ) => array
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( mRef ), wideSysStringIndex ) // Name
            );
         // ECMA-335, p. 248
         ForEachRow( this._typeSpec, tableWidths, sink, ( array, idx, listIdx, blobIdx, tSpec ) => array
            .WriteHeapIndex( ref idx, blobIdx, wideBLOBIndex ) // Signature
            );
         // ECMA-335, p. 230
         ForEachElement( Tables.ImplMap, this._implMap, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteUInt16LEToBytes( ref idx, (UInt16) tuple.Item1 ) // PInvokeAttributes
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MemberForwarded, tuple.Item2, tRefWidths ) // MemberForwarded
            .WriteHeapIndex( ref idx, tuple.Item3, wideSysStringIndex ) // Import name
            .WriteSimpleTableIndex( ref idx, Tables.ModuleRef, tuple.Item4, tableSizes ) // Import scope
            );
         // ECMA-335, p. 227
         ForEachElement( Tables.FieldRVA, this._fieldsWithRVA, tableWidths, sink, ( array, idx, listIdx, item ) => array
            .WriteUInt32LEToBytes( ref idx, fRVAs[listIdx] )
            .WriteSimpleTableIndex( ref idx, Tables.Field, this._fieldsWithRVA[listIdx], tableSizes )
            );
         // ECMA-335, p. 211
         ForEachRow( this._assembly, tableWidths, sink, ( array, idx, listIdx, blobIdx, ass ) => array
            .WriteInt32LEToBytes( ref idx, (Int32) ass.HashAlgorithm ) // HashAlgId
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.MajorVersion ) // MajorVersion
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.MinorVersion ) // MinorVersion
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.BuildNumber ) // BuildNumber
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.Revision ) // RevisionNumber
            .WriteInt32LEToBytes( ref idx, (Int32) ( ass.Flags & ~AssemblyFlags.PublicKey ) ) // Flags (Don't set public key flag)
            .WriteHeapIndex( ref idx, this._blob.GetBLOB( this._assemblyName.PublicKey ), wideBLOBIndex ) // PublicKey
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( ass.Name ), wideSysStringIndex ) // Name
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( ass.Culture ), wideSysStringIndex ) // Culture
            );
         // ECMA-335, p. 212
         ForEachElement( Tables.AssemblyRef, this._assemblyRef, tableWidths, sink, ( array, idx, listIdx, ass ) => array
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.MajorVersion ) // MajorVersion
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.MinorVersion ) // MinorVersion
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.BuildNumber ) // BuildNumber
            .WriteUInt16LEToBytes( ref idx, (UInt16) ass.Revision ) // RevisionNumber
            .WriteInt32LEToBytes( ref idx, (Int32) ass.Flags ) // Flags
            .WriteHeapIndex( ref idx, this._blob.GetBLOB( ass.PublicKey.IsNullOrEmpty() ? null : ass.PublicKey ), wideBLOBIndex ) // PublicKey
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( ass.Name ), wideSysStringIndex ) // Name
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( ass.Culture ), wideSysStringIndex ) // Culture
            .WriteHeapIndex( ref idx, this._aRefHashes[listIdx], wideBLOBIndex ) // HashValue
            );
         ForEachElement( Tables.File, this._files.Values.OrderBy( t => t.Item1 ).ToList(), tableWidths, sink, ( array, idx, listIdx, file ) => array
            .WriteUInt32LEToBytes( ref idx, (UInt32) file.Item2 )
            .WriteHeapIndex( ref idx, file.Item3, wideSysStringIndex )
            .WriteHeapIndex( ref idx, file.Item4, wideBLOBIndex )
            );
         ForEachElement( Tables.ExportedType, this._exportedTypes, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteUInt32LEToBytes( ref idx, (UInt32) tuple.Item1 ) // TypeAttributes
            .WriteInt32LEToBytes( ref idx, tuple.Item2 ) // TypeDef index in other (!) assembly
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( tuple.Item3 ), wideSysStringIndex ) // TypeName
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( tuple.Item4 ), wideSysStringIndex ) // TypeNamespace
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.Implementation, tuple.Item5, tRefWidths ) // Implementation
            );
         ForEachElement( Tables.ManifestResource, this._module.ManifestResources, tableWidths, sink, ( array, idx, listIdx, kvp ) =>
         {
            UInt32 mOffset;
            Int32 impl = 0;
            var add = embeddedManifestResourceOffsets.TryGetValue( kvp.Key, out mOffset );
            if ( !add )
            {
               add = moduleResRefIndices.ContainsKey( kvp.Key );
               if ( add )
               {
                  add = true;
                  mOffset = 0u;
                  impl = MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.Implementation, moduleResRefIndices[kvp.Key] );
               }
            }

            if ( add )
            {
               array
                  .WriteUInt32LEToBytes( ref idx, mOffset ) // Offset
                  .WriteUInt32LEToBytes( ref idx, (UInt32) kvp.Value.Attributes ) // Flags
                  .WriteHeapIndex( ref idx, this._sysStrings.GetString( kvp.Key ), wideSysStringIndex ) // Name
                  .WriteCodedTableIndex( ref idx, CodedTableIndexKind.Implementation, impl, tRefWidths ); // Implementation
            }
         } );
         // ECMA-335, p. 240
         ForEachElement( Tables.NestedClass, this._nestedClass, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteSimpleTableIndex( ref idx, Tables.TypeDef, tuple.Item1, tableSizes )
            .WriteSimpleTableIndex( ref idx, Tables.TypeDef, tuple.Item2, tableSizes )
            );
         // ECMA-335, p. 228
         ForEachRow( this._gParamDef, tableWidths, sink, ( array, idx, listIdx, blobIdx, gParam ) => array
            .WriteUInt16LEToBytes( ref idx, (UInt16) gParam.GenericParameterPosition ) // Number
            .WriteUInt16LEToBytes( ref idx, (UInt16) gParam.Attributes ) // Flags
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeOrMethodDef, this._gParamOwners[listIdx], tRefWidths ) // Owner
            .WriteHeapIndex( ref idx, this._sysStrings.GetString( gParam.Name ), wideSysStringIndex ) // Name
            );
         // ECMA-335, p. 238
         ForEachRow( this._methodSpec, tableWidths, sink, ( array, idx, listIdx, blobIdx, mSpec ) => array
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.MethodDefOrRef, MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.MethodDefOrRef, this._methodSpec._keys[listIdx] ), tRefWidths ) // Method
            .WriteHeapIndex( ref idx, blobIdx, wideBLOBIndex ) // Instantiation
            );
         // ECMA-335, p. 229
         ForEachElement( Tables.GenericParameterConstraint, this._gParamConstraints, tableWidths, sink, ( array, idx, listIdx, tuple ) => array
            .WriteSimpleTableIndex( ref idx, Tables.GenericParameter, tuple.Item1, tableSizes ) // Owner
            .WriteCodedTableIndex( ref idx, CodedTableIndexKind.TypeDefOrRef, tuple.Item2, tRefWidths ) // Constraint
            );
         // Padding
         for ( var i = mdStreamSize; i < mdStreamSize4; i++ )
         {
            sink.WriteByte( 0 );
         }

         // #String
         this._sysStrings.WriteStrings( sink );

         // #US
         this._userStrings.WriteStrings( sink );

         // #Guid
         foreach ( var guid in this._guids )
         {
            sink.Write( guid.ToByteArray() );
         }

         // #Blob
         this._blob.WriteBLOBs( sink );

         var total = (UInt32) anArray.Length + streamSizes.Sum() - mdHeaderSize;

         // Set token encoding callback
         eArgs.SetTokenFunctions( ( method, token ) =>
         {
            throw new InvalidOperationException( "Token resolving is not supported for emitting arguments used for emitting." );
         }, this.TokenEncoderCallback, ( method ) =>
         {
            throw new InvalidOperationException( "Token resolving is not supported for emitting arguments used for emitting." );
         }, this.TokenSignatureEncoderCallback );

         // Set metadata info
         eArgs.SetMDInfo( () => new EmittingMetadataInfo(
            new List<CILType>( this._typeDef._list ), // Create copy so that possibly modifications won't interfer with token resolved callbacks
            new List<CILMethodBase>( this._methodDef._list ),
            new List<CILParameter>( this._paramDef._list ),
            new List<CILField>( this._fieldDef._list ),
            new List<CILProperty>( this._propertyDef._list ),
            new List<CILEvent>( this._eventDef._list ),
            new List<UInt32>( this._methodDef._list.Select( m =>
            {
               UInt32 rVal;
               return methodRVAs.TryGetValue( m, out rVal ) ? rVal : 0u;
            } ).ToList() )
            ) );
         return total;
      }

      private void FinalizeMetaData(
         EmittingArguments eArgs,
         IList<Int32> typeDefExtends,
         //IList<Int32> memberRefParents,
         IList<Int32> evtTypes,
         IDictionary<String, Int32> moduleResRefIndices
         )
      {
         // Module
         this.ProcessModuleTable( eArgs.ModuleID );

         // Assembly
         this.ProcessAssemblyTable( eArgs.ModuleKind );

         // InterfaceImpl                             (SortBy Class, Interface)
         this.ProcessInterfaceImplTable();

         // EventMap
         // Event
         this.ProcessEventMapAndEventTables( evtTypes );

         // Property
         // PropertyMap
         this.ProcessPropertyMapAndPropertyTables();

         // MethodSemantics                           (SortBy Association)
         this.ProcessMethodSemanticsTable();

         // TypeDef
         this.ProcessTypeDefTable( typeDefExtends );

         // FieldMarshal
         this.ProcessFieldMarshalTable();

         // NestedClass                               (SortBy NestedClass)
         this.ProcessNestedClassTable();

         // GenericParamConstraint                    (SortBy Owner)
         this.ProcessGenericParamConstraintTable();

         // Constant                                  (SortBy Parent)
         this.ProcessConstantsTable();


         // CustomAttribute                           (SortBy Parent)
         this.ProcessCustomAttributeTable();

         // MethodImpl                                (SortBy Class)
         // might reference member ref
         this.ProcessMethodImplTable();

         // MethodImpl
         // ModuleDef
         this.ProcessMethodImplAndModuleDefTables();

         // ManifestResource
         foreach ( var kvp in this._module.ManifestResources )
         {
            this._sysStrings.GetOrAddString( kvp.Key, false );
            var modRes = kvp.Value as ModuleManifestResource;
            if ( modRes != null && modRes.Module != null )
            {
               if ( Object.Equals( this._module.Assembly, modRes.Module.Assembly ) )
               {
                  var tuple = this._emittingArgs.OtherModules[modRes.Module.Name];
                  moduleResRefIndices.Add( kvp.Key, this.GetFileToken( tuple.Item1, tuple.Item2, FileAttributes.ContainsMetadata ) );
               }
               else
               {
                  moduleResRefIndices.Add( kvp.Key, TokenUtils.EncodeToken( Tables.AssemblyRef, this.GetAssemblyRefInfoAssemblyRef( modRes.Module.Assembly.Name ).Item2 + 1 ) );
               }
            }
            else
            {
               var fRes = kvp.Value as FileManifestResource;
               if ( fRes != null && fRes.FileName != null )
               {
                  var hash = fRes.Hash;
                  if ( hash == null )
                  {
                     using ( var fs = this._emittingArgs.FileStreamOpener( this._module, fRes.FileName ) )
                     {
                        Func<Stream> hashStream; Func<Byte[]> hashGetter; IDisposable transform;
                        this._context.LaunchHashStreamEvent( this._assemblyName.HashAlgorithm, out hashStream, out hashGetter, out transform );
                        using ( var tf = transform )
                        {
                           using ( var cryptoStream = hashStream() )
                           {
                              var buf = new Byte[MetaDataConstants.STREAM_COPY_BUFFER_SIZE];
                              fs.CopyStream( cryptoStream, buf );
                           }
                           hash = hashGetter();
                        }
                     }
                  }

                  moduleResRefIndices.Add( kvp.Key, this.GetFileToken( fRes.FileName, hash, FileAttributes.ContainsNoMetadata ) );
               }
            }
         }

         // DeclSecurity
         this.ProcessDeclSecurityTable();
      }

      private void ProcessDefinedType( CILType type, EmittingAssemblyMapper mapper )
      {
         if ( mapper != null )
         {
            type = (CILType) mapper.MapTypeBase( type );
         }
         // Add row to typedef
         this._typeDef.AddItem( type );
         this._sysStrings.GetOrAddString( type.Name, false );
         this._sysStrings.GetOrAddString( type.Namespace, false );
      }

      private void PostProcessDefinedType( CILType type, EmittingAssemblyMapper mapper )
      {
         if ( mapper != null )
         {
            type = (CILType) mapper.MapTypeBase( type );
         }
         this._typeDefFieldAndMethodIndices.Add( Tuple.Create( this._fieldDef.Count + 1, this._methodDef.Count + 1 ) );

         // Add rows to methodDef
         foreach ( var method in ( (IEnumerable<CILMethodBase>) type.Constructors ).Concat( type.DeclaredMethods ) )
         {
            var curMethod = method;
            if ( mapper != null )
            {
               curMethod = mapper.MapMethodBase( curMethod );
            }
            this._methodDef.AddItem( curMethod );
            this._sysStrings.GetOrAddString( curMethod.GetName(), false );

            this._methodDefParamIndices.Add( this._paramDef.Count + 1 );

            if ( curMethod is CILMethod )
            {
               var rParam = ( (CILMethod) curMethod ).ReturnParameter;
               if ( rParam.CustomAttributeData.Any() )
               {
                  this._paramDef.AddItem( rParam );
                  this._sysStrings.GetOrAddString( rParam.Name, false );
               }
            }
            foreach ( var param in curMethod.Parameters )
            {
               this._paramDef.AddItem( param );
               this._sysStrings.GetOrAddString( param.Name, false );
            }
         }

         // Add fields to fieldDef
         var isExplicit = type.Attributes.IsExplicitLayout();
         foreach ( var field in type.DeclaredFields )
         {
            var curField = field;
            if ( mapper != null )
            {
               curField = mapper.MapField( curField );
            }
            this._fieldDef.AddItem( curField );
            this._sysStrings.GetOrAddString( curField.Name, false );
            if ( curField.Attributes.HasRVA() )
            {
               this._fieldsWithRVA.Add( this._fieldDef.GetTableIndexOf( curField ) );
            }
            if ( isExplicit )
            {
               this._fieldLayout.Add( Tuple.Create( curField.FieldOffset, this._fieldDef.Count ) );
            }
         }
      }

      private void ProcessAllGParams()
      {
         var max = Math.Max( this._typeDef.Count, this._methodDef.Count );
         var i = 0;
         while ( i < max )
         {
            if ( i < this._typeDef.Count )
            {
               var type = (CILType) this._typeDef._list[i];
               this.ProcessGParams( type.GenericArguments, this._typeDef.GetTokenFor( type ) );
            }
            if ( i < this._methodDef.Count )
            {
               var method = this._methodDef._list[i];
               if ( MethodKind.Method == method.MethodKind )
               {
                  this.ProcessGParams( ( (CILMethod) method ).GenericArguments, this._methodDef.GetTokenFor( method ) );
               }
            }
            ++i;
         }
      }

      private void ProcessGParams( ListQuery<CILTypeBase> gArgs, Int32 ownerToken )
      {
         for ( var j = 0; j < gArgs.Count; ++j )
         {
            this._gParamDef.AddItem( (CILTypeParameter) gArgs[j] );
            this._gParamOwners.Add( MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.TypeOrMethodDef, ownerToken ) );
            this._sysStrings.GetOrAddString( ( (CILTypeParameter) gArgs[j] ).Name, true );
         }
      }

      private void ProcessModuleTable( Guid moduleID )
      {
         this._moduleDef.AddItem( this._module );
         this._sysStrings.GetOrAddString( this._module.Name, false ); // TODO throw if empty
         this._guids.Add( moduleID );
      }

      private void ProcessAssemblyTable( ModuleKind moduleKind )
      {
         if ( ModuleKind.NetModule != moduleKind && Object.Equals( this._module, this._module.Assembly.MainModule ) )
         {
            this._assembly.AddItem( this._assemblyName );
            this._sysStrings.GetOrAddString( this._assemblyName.Name, false ); // TODO throw if empty
            this._sysStrings.GetOrAddString( this._assemblyName.Culture, false );
            this._blob.GetOrAddBLOB( this._assemblyName.PublicKey ); // TODO store BLOB index ?

            // Process other modules and exported types
            foreach ( var kvp in this._emittingArgs.OtherModules )
            {
               if ( !String.Equals( kvp.Key, this._module.Name ) )
               {
                  var tuple = kvp.Value;
                  var mod = this._module.Assembly.Modules.FirstOrDefault( m => String.Equals( m.Name, kvp.Key ) );
                  if ( mod != null )
                  {
                     var modToken = this.GetFileToken( tuple.Item1, tuple.Item2, FileAttributes.ContainsMetadata );
                     foreach ( var t in mod.DefinedTypes )
                     {
                        this.ProcessOtherModuleType( modToken, t, 0 );
                     }
                  }
               }
            }

            // Process type forwarders
            var tfDic = new Dictionary<Tuple<String, String>, Int32>();
            var tfInfos = this._module.Assembly.ForwardedTypeInfos;
            foreach ( var kvp in tfInfos )
            {
               this.ProcessTypeForwarder( tfInfos, kvp.Key, tfDic );
            }
         }
      }

      private void ProcessTypeForwarder( DictionaryQuery<Tuple<String, String>, TypeForwardingInfo> assTFDic, Tuple<String, String> curKey, IDictionary<Tuple<String, String>, Int32> tfDic )
      {
         TypeForwardingInfo tf;
         if ( assTFDic.TryGetValue( curKey, out tf ) )
         {
            if ( !tfDic.ContainsKey( curKey ) )
            {
               // Record strings
               this._sysStrings.GetOrAddString( tf.Name, false );
               this._sysStrings.GetOrAddString( tf.Namespace, false );

               Int32 refToken;
               if ( tf.DeclaringTypeName != null )
               {
                  var nKey = Tuple.Create( tf.DeclaringTypeName, tf.DeclaringTypeNamespace );
                  if ( !tfDic.TryGetValue( nKey, out refToken ) )
                  {
                     // Avoid infinite loop if for some reason declaring type is same as this type
                     if ( String.Equals( tf.DeclaringTypeName, tf.Name ) && String.Equals( tf.DeclaringTypeNamespace, tf.Namespace ) )
                     {
                        // Row is referencing itself
                        refToken = TokenUtils.EncodeToken( Tables.ExportedType, this._exportedTypes.Count );
                     }
                     else
                     {
                        this.ProcessTypeForwarder( assTFDic, nKey, tfDic );
                        // tfDic is guaranteed to have value for nKey now
                        refToken = tfDic[nKey];
                     }
                  }
               }
               else
               {
                  refToken = TokenUtils.EncodeToken( Tables.AssemblyRef, this.GetAssemblyRefInfoAssemblyRef( tf.AssemblyName ).Item2 + 1 );
               }
               // Add row
               this._exportedTypes.Add( Tuple.Create( tf.Attributes, 0, tf.Name, tf.Namespace, MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.Implementation, refToken ) ) );
               tfDic.Add( curKey, TokenUtils.EncodeToken( Tables.ExportedType, this._exportedTypes.Count ) );
            }
         }
         else
         {
            throw new InvalidOperationException( "The type forwarder declaring type " + curKey + " was not found." );
         }
      }

      private void ProcessOtherModuleType( Int32 moduleToken, CILType type, Int32 declTypeIdx )
      {
         var attrs = type.Attributes;
         if ( attrs.IsPublic() || attrs.IsNestedPublic() )
         {
            var tn = type.Name;
            var tns = type.Namespace;
            // Record strings
            this._sysStrings.GetOrAddString( tn, false );
            this._sysStrings.GetOrAddString( tns, false );
            // Add row
            this._exportedTypes.Add( Tuple.Create( attrs, 0, tn, tns, MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.Implementation, declTypeIdx == 0 ? moduleToken : TokenUtils.EncodeToken( Tables.ExportedType, declTypeIdx ) ) ) );

            // Process nested types
            declTypeIdx = this._exportedTypes.Count;
            foreach ( var dt in type.DeclaredNestedTypes )
            {
               this.ProcessOtherModuleType( moduleToken, dt, declTypeIdx );
            }
         }
      }

      private Int32 GetFileToken( String fileName, Byte[] hashValue, FileAttributes attrs )
      {
         return this._files.GetOrAdd_NotThreadSafe( fileName, fn => Tuple.Create( TokenUtils.EncodeToken( Tables.File, this._files.Count + 1 ), attrs, this._sysStrings.GetOrAddString( fn, false ), this._blob.GetOrAddBLOB( hashValue ) ) ).Item1;
      }

      private void ProcessInterfaceImplTable()
      {
         var tableIdx = 1;
         foreach ( CILType tDef in this._typeDef._list )
         {
            foreach ( var iFace in tDef.DeclaredInterfaces ) //.AsDepthFirstEnumerable( t => t.DeclaredInterfaces ).Skip( 1 ).Distinct() )
            {
               var curIFace = this._assemblyMapper.TryMapType( iFace );
               this._interfaceImpl.Add( Tuple.Create( tableIdx, MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.TypeDefOrRef, this.GetTokenFor( curIFace, false ) ) ) );
            }
            ++tableIdx;
         }
      }

      private void ProcessEventMapAndEventTables( IList<Int32> evtTypes )
      {
         for ( var i = 0; i < this._typeDef._list.Count; ++i )
         {
            var tDef = (CILType) this._typeDef._list[i];
            if ( tDef.DeclaredEvents.Count > 0 )
            {
               this._eventMap.Add( Tuple.Create( i + 1, this._eventDef.Count + 1 ) );
               foreach ( var evt in tDef.DeclaredEvents )
               {
                  var hType = evt.EventHandlerType == null ? null : this._assemblyMapper.TryMapTypeBase( evt.EventHandlerType );
                  this._eventDef.AddItem( evt );
                  evtTypes.Add( hType == null ? 0 : MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.TypeDefOrRef, this.GetTokenFor( hType, true ) ) );
                  this._sysStrings.GetOrAddString( evt.Name, false );
               }
            }
         }
      }

      private void ProcessPropertyMapAndPropertyTables()
      {
         for ( var i = 0; i < this._typeDef._list.Count; ++i )
         {
            var tDef = (CILType) this._typeDef._list[i];
            if ( tDef.DeclaredProperties.Count > 0 )
            {
               this._propertyMap.Add( Tuple.Create( i + 1, this._propertyDef.Count + 1 ) );
               foreach ( var prop in tDef.DeclaredProperties )
               {
                  this._propertyDef.AddItem( prop );
                  this._sysStrings.GetOrAddString( prop.Name, false );
               }
            }
         }
      }

      private void ProcessMethodSemanticsTable()
      {
         var max = Math.Max( this._propertyDef.Count, this._eventDef.Count );
         for ( var i = 0; i < max; ++i )
         {
            // Order is important, because Tables.Event < Tables.Property, and method semantics table is sorted by that table ref.
            this.AddToMethodSemantics( i, this._eventDef, E_CIL.GetSemanticMethods );
            this.AddToMethodSemantics( i, this._propertyDef, E_CIL.GetSemanticMethods );
         }
      }

      private void AddToMethodSemantics<T>( Int32 i, TableEmittingInfoWithList<T> table, Func<T, IEnumerable<Tuple<MethodSemanticsAttributes, CILMethod>>> methodGetter )
      {
         if ( i < table.Count )
         {
            var item = table._list[i];
            foreach ( var tuple in methodGetter( item ) )
            {
               this._methodSemantics.Add( Tuple.Create( (UInt16) tuple.Item1, this._methodDef.GetTableIndexOf( tuple.Item2 ), MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.HasSemantics, TokenUtils.EncodeToken( table._tableEnum, i + 1 ) ) ) );
            }
         }
      }

      private void ProcessNestedClassTable()
      {
         for ( var i = 0; i < this._typeDef._list.Count; ++i )
         {
            var tDef = (CILType) this._typeDef._list[i];
            if ( tDef.DeclaringType != null )
            {
               this._nestedClass.Add( Tuple.Create( i + 1, this._typeDef.GetTableIndexOf( tDef.DeclaringType ) ) );
            }
         }
      }

      private void ProcessTypeDefTable( IList<Int32> typeDefExtends )
      {
         foreach ( CILType tDef in this._typeDef._list )
         {
            var bType = tDef.BaseType;
            Int32 bTypeToken = 0;
            if ( bType != null )
            {
               bType = this._assemblyMapper.TryMapType( bType );
               bTypeToken = MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.TypeDefOrRef, this.GetTokenFor( bType, false ) );
            }
            typeDefExtends.Add( bTypeToken );
            var layout = tDef.Layout;
            if ( layout != null && !tDef.Attributes.IsAutoLayout() && ( layout.Value.pack != 8 || layout.Value.size != 0 ) )
            {
               this._classLayout.Add( Tuple.Create( (Int16) layout.Value.pack, layout.Value.size, this._typeDef.GetTableIndexOf( tDef ) ) );
            }
         }
      }

      private void ProcessGenericParamConstraintTable()
      {
         for ( var i = 0; i < this._gParamDef._list.Count; ++i )
         {
            var gParam = (CILTypeParameter) this._gParamDef._list[i];
            foreach ( var constraint in gParam.GenericParameterConstraints )
            {
               var curConstraint = this._assemblyMapper.TryMapTypeBase( constraint );
               this._gParamConstraints.Add( Tuple.Create( i + 1, MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.TypeDefOrRef, this.GetTokenFor( curConstraint, false ) ) ) );
            }
         }
      }

      private void ProcessConstantsTable()
      {
         var max = Math.Max( this._fieldDef.Count, Math.Max( this._paramDef.Count, this._propertyDef.Count ) );
         for ( var i = 0; i < max; ++i )
         {
            this.ProcessConstantElement<CILField, FieldAttributes>( i, this._fieldDef, E_CIL.HasDefault, field => Tuple.Create( field.FieldType, field.ConstantValue ) );
            this.ProcessConstantElement<CILParameter, ParameterAttributes>( i, this._paramDef, E_CIL.HasDefault, param => Tuple.Create( param.ParameterType, param.ConstantValue ) );
            this.ProcessConstantElement<CILProperty, PropertyAttributes>( i, this._propertyDef, E_CIL.HasDefault, prop => Tuple.Create( prop.GetPropertyType(), prop.ConstantValue ) );
         }
      }

      private void ProcessConstantElement<T, U>( Int32 i, TableEmittingInfoWithList<T> table, Func<U, Boolean> hasDefaultFunc, Func<T, Tuple<CILTypeBase, Object>> defaultValueGetter )
         where T : CILElementWithConstant, CILElementWithAttributes<U>
      {
         if ( i < table.Count )
         {
            var item = table._list[i];
            if ( hasDefaultFunc( item.Attributes ) )
            {
               var dv = defaultValueGetter( item );
               SignatureElementTypes sig;
               var type = this._assemblyMapper.TryMapTypeBase( dv.Item1 );
               CILTypeBase nullable;
               if ( type.IsNullable( out nullable ) )
               {
                  type = nullable;
               }
               var tc = type.GetTypeCode( CILTypeCode.Empty );
               var constant = dv.Item2;
               if ( !MetaDataConstants.TYPECODE_MAPPING_SIMPLE.TryGetValue( tc, out sig )
                  || ( constant != null
                     && !MetaDataConstants.TYPECODE_MAPPING_SIMPLE.TryGetValue(
#if WINDOWS_PHONE_APP
                     E_CIL.GetTypeCode
#else
                     (CILTypeCode) Type.GetTypeCode
#endif
                  (constant.GetType() ), out sig ) 
                     ) )
               {
                  if ( constant != null && dv.Item1.IsValueType() )
                  {
                     throw new InvalidOperationException( "Constant of type " + type + " is not supported." );
                  }
                  else
                  {
                     sig = SignatureElementTypes.Class;
                     tc = CILTypeCode.Object;
                  }
               }
               else if ( SignatureElementTypes.String == sig && constant == null )
               {
                  // Null strings are serialized as null class
                  sig = SignatureElementTypes.Class;
                  tc = CILTypeCode.Object;
               }
               var info = new BinaryHeapEmittingInfo();
               this.WriteConstantValue( info, tc, constant );
               this._constants.Add( Tuple.Create<Int16, Int32, UInt32>( (Int16) sig, MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.HasConstant, TokenUtils.EncodeToken( table._tableEnum, i + 1 ) ), this._blob.GetOrAddBLOB( info.CreateByteArray() ) ) );
            }
         }
      }

      private void ProcessCustomAttributeTable(
         //ref Boolean isPostProcessing
         )
      {
         //if ( !isPostProcessing )
         //{
         var max = Math.Max(
            this._typeDef.Count,
            Math.Max(
               this._methodDef.Count,
               Math.Max(
                  this._paramDef.Count,
                  Math.Max(
                     this._fieldDef.Count,
                     Math.Max(
                        this._moduleDef.Count,
                        Math.Max(
                           this._propertyDef.Count,
                           Math.Max(
                              this._eventDef.Count,
                              Math.Max(
                                 this._gParamDef.Count,
                                 this._assembly.Count
                              )
                           )
                        )
                     )
                  )
               )
            )
         );
         for ( var i = 0; i < max; ++i )
         {
            // Apparently, if this is done in different order, things get broken by Microsoft's stuff (but still work ok in e.g. ILSpy etc)
            // Actually, order is same as in ECMA-335 (p. 274)
            this.ProcessCustomAttributeElement( i, this._methodDef );
            this.ProcessCustomAttributeElement( i, this._fieldDef );
            this.ProcessCustomAttributeElement( i, this._typeDef );
            this.ProcessCustomAttributeElement( i, this._paramDef );
            this.ProcessCustomAttributeElement( i, this._moduleDef );
            this.ProcessCustomAttributeElement( i, this._propertyDef );
            this.ProcessCustomAttributeElement( i, this._eventDef );
            this.ProcessCustomAttributeElementIndirect( i, this._assembly, ass => this._module.Assembly.CustomAttributeData );
            this.ProcessCustomAttributeElement( i, this._gParamDef );
         }

         // TODO post-processing - custom attributes to member-ref etc.
         // Don't think that's possible with C#-like reflection emitting system?

      }

      private void ProcessFieldMarshalTable()
      {
         var max = Math.Max( this._fieldDef.Count, this._paramDef.Count );
         for ( var i = 0; i < max; ++i )
         {
            if ( i < this._fieldDef.Count && this._fieldDef._list[i].MarshalingInformation != null )
            {
               this._marshals.Add( Tuple.Create( MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.HasFieldMarshal, TokenUtils.EncodeToken( Tables.Field, i + 1 ) ), this.WriteMarshalInfo( this._fieldDef._list[i].MarshalingInformation ) ) );
            }
            if ( i < this._paramDef.Count && this._paramDef._list[i].MarshalingInformation != null )
            {
               this._marshals.Add( Tuple.Create( MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.HasFieldMarshal, TokenUtils.EncodeToken( Tables.Parameter, i + 1 ) ), this.WriteMarshalInfo( this._paramDef._list[i].MarshalingInformation ) ) );
            }
         }
      }

      private void ProcessCustomAttributeElement<T>( Int32 i, TableEmittingInfoWithList<T> table )
         where T : CILCustomAttributeContainer
      {
         if ( i < table.Count )
         {
            var element = table._list[i];
            foreach ( var cData in element.CustomAttributeData )
            {
               if ( !this.CheckForPseudoAttribute( cData ) )
               {
                  this.AddToCustomAttributes( i, table, cData );
               }
            }
         }
      }

      private Boolean CheckForPseudoAttribute( CILCustomAttribute ca )
      {
         var dt = this._assemblyMapper.TryMapType( ca.Constructor.DeclaringType );
         var typeStr = dt.Namespace + CILTypeImpl.NAMESPACE_SEPARATOR + dt.Name;
         var retVal = PSEUDO_ATTRIBUTES.Contains( typeStr );
         if ( !retVal )
         {
            // Check whether it is declarative security attribute
            retVal = ca.Constructor.DeclaringType.BaseTypeChain().Any( bt => String.Equals( Consts.SECURITY_ATTR, bt.GetFullName() ) && Object.Equals( this._module.AssociatedMSCorLibModule, bt.Module ) );
         }
         return retVal;
      }

      private void ProcessCustomAttributeElementIndirect<T>( Int32 i, TableEmittingInfoWithList<T> table, Func<T, ListQuery<CILCustomAttribute>> attrGetter )
      {
         if ( i < table.Count )
         {
            var element = table._list[i];
            foreach ( var cData in attrGetter( element ) )
            {
               this.AddToCustomAttributes( i, table, cData );
            }
         }
      }

      private void AddToCustomAttributes<T>( Int32 i, TableEmittingInfoWithList<T> table, CILCustomAttribute cData )
      {
         var info = new BinaryHeapEmittingInfo();
         this.WriteCustomAttributeSignature( info, cData );
         var ctor = this._assemblyMapper.TryMapConstructor( cData.Constructor );
         this._customAttributes.Add( Tuple.Create( MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.HasCustomAttribute, TokenUtils.EncodeToken( table._tableEnum, i + 1 ) ), MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.CustomAttributeType, this.GetTokenFor( ctor, false ) ), this._blob.GetOrAddBLOB( info.CreateByteArray() ) ) );
      }

      private void ProcessMethodImplTable()
      {
         for ( var i = 0; i < this._typeDef.Count; ++i )
         {
            foreach ( var method in ( (CILType) this._typeDef._list[i] ).DeclaredMethods )
            {
               // TODO shall we check base types? Since the MethodBody index is also MethodDef or MemberRef. And standard says:
               // (either a method body within C, or a method body implemented by a base class of C)

               foreach ( var curMethod in method.OverriddenMethods )
               {
                  this._methodImpl.Add( Tuple.Create( i + 1, MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.MethodDefOrRef, this.GetTokenFor( method, true ) ), MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.MethodDefOrRef, this.GetTokenFor( curMethod, false ) ) ) );
               }
            }
         }
      }

      private void ProcessMethodImplAndModuleDefTables()
      {
         // Populate ImplMap table
         foreach ( var mDef in this._methodDef._list )
         {
            if ( MethodKind.Method == mDef.MethodKind )
            {
               var m = mDef as CILMethod;
               if ( m.PlatformInvokeModuleName != null && m.PlatformInvokeName != null )
               {
                  this._implMap.Add( Tuple.Create( m.PlatformInvokeAttributes, MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.MemberForwarded, this._methodDef.GetTokenFor( mDef ) ), this._sysStrings.GetOrAddString( m.PlatformInvokeName, true ), this._moduleRef.GetOrAddTokenFor( m.PlatformInvokeModuleName ) ) );
               }
            }
         }

         // Postprocess ModuleDef table.
         foreach ( var mRef in this._moduleRef._list )
         {
            this._sysStrings.GetOrAddString( mRef, false );
         }
      }

      private void ProcessDeclSecurityTable()
      {
         var max = Math.Max( this._typeDef.Count, this._methodDef.Count );
         for ( var i = 0; i < max; ++i )
         {
            // Order is important - first typedef, then methoddef. Like defined in ECMA-335, p. 275
            this.ProcessDeclSecurityElement( i, this._typeDef );
            this.ProcessDeclSecurityElement( i, this._methodDef );
         }
      }

      private void ProcessDeclSecurityElement<T>( Int32 i, TableEmittingInfoWithList<T> table )
         where T : CILElementWithSecurityInformation
      {
         if ( i < table._list.Count )
         {
            foreach ( var kvp in table._list[i].DeclarativeSecurity )
            {
               var info = new BinaryHeapEmittingInfo();
               info.AddByte( MetaDataConstants.DECL_SECURITY_HEADER );
               info.AddCompressedUInt32( kvp.Value.Count );
               foreach ( var sec in kvp.Value )
               {
                  info.AddTypeString( this._module, sec.SecurityAttributeType );
                  // For some silly reason, the amount of bytes taken to serialize named attributes is stored at this point. Sigh...
                  var info2 = new BinaryHeapEmittingInfo();
                  foreach ( var arg in sec.NamedArguments )
                  {
                     this.WriteCustomAttributeNamedArg( info2, arg );
                  }
                  // Now write to sec blob
                  var argsBlob = info2.CreateByteArray();
                  // The length of named arguments blob
                  info.AddCompressedUInt32( argsBlob.Length + (Int32) BitUtils.GetEncodedUIntSize( sec.NamedArguments.Count ) );
                  // The amount of named arguments
                  info.AddCompressedUInt32( sec.NamedArguments.Count );
                  // The named arguments
                  info.AddBytes( argsBlob );
               }
               this._declSecurity.Add( Tuple.Create( (UInt16) kvp.Key, MetaDataConstants.GetCodedTableIndex( CodedTableIndexKind.HasDeclSecurity, TokenUtils.EncodeToken( table._tableEnum, i + 1 ) ), this._blob.GetOrAddBLOB( info.CreateByteArray() ) ) );
            }
         }
      }

      private Tuple<CILAssemblyName, Int32> GetAssemblyRefInfoAssemblyRef( CILAssemblyName ass )
      {
         Tuple<CILAssemblyName, Int32> name;
         if ( !this._assRefDic.TryGetValue( ass, out name ) )
         {
            var state = new CILAssemblyName( ass );
            state.Flags = AssemblyFlags.None;
            name = Tuple.Create( (CILAssemblyName) state, this._assemblyRef.Count );
            if ( ass.Flags.IsFullPublicKey() )
            {
               state.Flags |= AssemblyFlags.PublicKey;
            }

            var aRefProcessor = this._emittingArgs.AssemblyRefEventValue;
            if ( aRefProcessor != null )
            {
               aRefProcessor( state );
            }

            if ( state.Flags.IsFullPublicKey() && state.PublicKey != null && this._hashStreamArgs != null )
            {
               // Have to compute public key token
               state.PublicKey = this._hashStreamArgs.ComputeHash( state.PublicKey );
               state.Flags &= ~AssemblyFlags.PublicKey;
            }

            if ( !state.Flags.IsRetargetable() && String.Equals( state.Name, this._emittingArgs.CorLibName, StringComparison.Ordinal ) ) // TODO was assembly name comparison case-insensitive?
            {
               state.MajorVersion = this._emittingArgs.CorLibMajor;
               state.MinorVersion = this._emittingArgs.CorLibMinor;
               state.BuildNumber = this._emittingArgs.CorLibBuild;
               state.Revision = this._emittingArgs.CorLibRevision;
            }


            var oldCount = this._assemblyRef.Count;
            Int32 idx;
            // Don't add duplicates (sometimes this happens when e.g. merging assemblies from multiple different PCLs)
            if ( !IndexOf( this._assemblyRef, state, ( curState, itemState ) =>
               String.Equals( curState.Name, itemState.Name )
                  //&& ( state.Flags.IsRetargetable()
                  //|| (
                  && curState.MajorVersion == itemState.MajorVersion
                  && curState.MinorVersion == itemState.MinorVersion
                  && curState.BuildNumber == itemState.BuildNumber
                  && curState.Revision == itemState.Revision
                  && (
                  ( curState.Flags.IsFullPublicKey() && itemState.Flags.IsFullPublicKey() && curState.PublicKey.StructArrayEquals( itemState.PublicKey ) )
                     || ( true ) // TODO ComputePublicKeyToken -event to CILReflectionContext.
                  )
                  && String.Equals( curState.Culture, itemState.Culture ) //) )
               , out idx ) )
            {
               this._assemblyRef.Add( name.Item1 );
               this._emittingArgs.AssemblyRefs.Add( state );

               this._sysStrings.GetOrAddString( state.Name, false ); // TODO throw if empty
               this._sysStrings.GetOrAddString( state.Culture, false );
               this._blob.GetOrAddBLOB( state.PublicKey ); // TODO store BLOB index ?
               this._aRefHashes.Add( 0 ); // TODO hash value
            }
            if ( oldCount == this._assemblyRef.Count )
            {
               name = Tuple.Create( (CILAssemblyName) state, idx );
            }

            this._assRefDic.Add( ass, name );
         }
         return name;
      }

      private void WriteMethodSignature( BinaryHeapEmittingInfo info, CILMethodBase method )
      {
         method = this._assemblyMapper.TryMapMethodBase( method );

         var hasGArgs = MethodKind.Method == method.MethodKind && ( (CILMethod) method ).HasGenericArguments();
         this.WriteMethodSignature(
            info,
            method.CallingConvention.GetSignatureStarter( method.Attributes.IsStatic(), hasGArgs ),
            hasGArgs ? ( (CILMethod) method ).GenericArguments.Count : 0,
            method,
            ( CILParameter p ) => p.ParameterType,
            null );
      }

      private void WriteMethodSignature( BinaryHeapEmittingInfo info, CILMethodSignature methodSig, Tuple<CILCustomModifier[], CILTypeBase>[] varArgs )
      {
         this.WriteMethodSignature(
            info,
            (SignatureStarters) methodSig.CallingConvention,
            0,
            methodSig,
            ( CILParameterSignature p ) => p.ParameterType,
            varArgs
            );
      }

      private void WriteMethodSignature<TMethod, TParam>( BinaryHeapEmittingInfo info, SignatureStarters starter, Int32 gArgsCount, TMethod method, Func<TParam, CILTypeBase> pTypeExtractor, Tuple<CILCustomModifier[], CILTypeBase>[] varArgs )
         where TMethod : class, CILMethodOrSignature<TParam>
         where TParam : class, CILParameterBase<TMethod>
      {
         info.AddSigStarterByte( starter );
         if ( gArgsCount > 0 )
         {
            info.AddCompressedUInt32( gArgsCount );
         }

         info.AddCompressedUInt32( method.Parameters.Count );

         // RetType
         var retParam = method is CILMethodWithReturnParameter<TParam> ? ( (CILMethodWithReturnParameter<TParam>) method ).ReturnParameter : null;
         this.WriteCustomModifiers( info, retParam == null ? null : retParam.CustomModifiers );
         if ( retParam == null )
         {
            // Ctor
            info.AddSigByte( SignatureElementTypes.Void );
         }
         else
         {
            this.WriteTypeSignature( info, pTypeExtractor( retParam ) );
         }

         // Param
         foreach ( var param in method.Parameters )
         {
            this.WriteCustomModifiers( info, param.CustomModifiers );
            this.WriteTypeSignature( info, pTypeExtractor( param ) );
         }

         if ( varArgs != null && varArgs.Length > 0 )
         {
            info.AddSigByte( SignatureElementTypes.Sentinel );
            foreach ( var v in varArgs )
            {
               this.WriteCustomModifiers( info, v.Item1 );
               this.WriteTypeSignature( info, v.Item2 );
            }
         }
      }

      private void WriteFieldSignature( BinaryHeapEmittingInfo info, CILField field )
      {
         field = this._assemblyMapper.TryMapField( field );
         info.AddSigStarterByte( SignatureStarters.Field );
         this.WriteCustomModifiers( info, field.CustomModifiers );
         this.WriteTypeSignature( info, field.FieldType );
      }

      private void WritePropertySignature( BinaryHeapEmittingInfo info, CILProperty prop )
      {
         var starter = SignatureStarters.Property;
         var getter = prop.GetMethod;
         var setter = prop.SetMethod;
         if ( ( getter != null && !getter.Attributes.IsStatic() ) || ( setter != null && !setter.Attributes.IsStatic() ) )
         {
            starter |= SignatureStarters.HasThis;
         }

         info.AddSigStarterByte( starter );
         info.AddCompressedUInt32( prop.GetIndexTypeCount() );
         this.WriteCustomModifiers( info, prop.CustomModifiers );
         this.WriteTypeSignature( info, prop.GetPropertyType() );
         foreach ( var pInfo in prop.GetIndexParameters() )
         {
            this.WriteCustomModifiers( info, pInfo.CustomModifiers );
            this.WriteTypeSignature( info, pInfo.ParameterType );
         }
      }

      private void WriteLocalsSignature( BinaryHeapEmittingInfo info, LocalBuilder[] locals )
      {
         info.AddSigStarterByte( SignatureStarters.LocalSignature );
         info.AddCompressedUInt32( locals.Length );
         foreach ( var local in locals )
         {
            this.WriteCustomModifiers( info, null ); // TODO
            if ( local.IsPinned )
            {
               info.AddSigByte( SignatureElementTypes.Pinned );
            }
            this.WriteTypeSignature( info, local.LocalType );
         }
      }

      private void WriteCustomAttributeSignature( BinaryHeapEmittingInfo info, CILCustomAttribute attrData )
      {
         // Prolog
         info.AddByte( 1 );
         info.AddByte( 0 );

         // Fixed args
         for ( var i = 0; i < attrData.ConstructorArguments.Count; ++i )
         {
            var arg = attrData.ConstructorArguments[i];
            this.WriteCustomAttributeFixedArg( info, (CILType) attrData.Constructor.Parameters[i].ParameterType, arg.ArgumentType, arg.Value );
         }

         // Named args
         info.AddUncompressedUInt16( (UInt16) attrData.NamedArguments.Count );
         foreach ( var arg in attrData.NamedArguments )
         {
            this.WriteCustomAttributeNamedArg( info, arg );
         }
      }

      private void WriteCustomAttributeFixedArg( BinaryHeapEmittingInfo info, CILType paramType, CILType argType, Object arg )
      {
         ArgumentValidator.ValidateNotNull( "Custom attribute argument parameter type", paramType );
         ArgumentValidator.ValidateNotNull( "Custom attribute argument type", argType );

         paramType = this._assemblyMapper.TryMapType( paramType );
         //if ( paramType.IsEnum() )
         //{
         //   this.WriteCustomAttributeFixedArg( info, (CILType) paramType.GetEnumValueField().FieldType, (CILType) argType.GetEnumValueField().FieldType, arg );
         //}
         //else 
         if ( paramType.ElementKind == ElementKind.Array )
         {
            if ( arg == null )
            {
               info.AddUncompressedInt32( unchecked( (Int32) 0xFFFFFFFF ) );
            }
            else
            {
               info.AddUncompressedInt32( ( (Array) arg ).Length );
               paramType = (CILType) paramType.ElementType;
               argType = (CILType) argType.ElementType;
               foreach ( var elem in (Array) arg )
               {
                  this.WriteCustomAttributeFixedArg( info, paramType, argType, elem );
               }
            }
         }
         else
         {
            var tc = paramType.TypeCode;
            switch ( tc )
            {
               case CILTypeCode.Boolean:
                  info.AddByte( Convert.ToBoolean( arg ) ? (Byte) 1 : (Byte) 0 );
                  break;
               case CILTypeCode.SByte:
                  info.AddSByte( Convert.ToSByte( arg ) );
                  break;
               case CILTypeCode.Byte:
                  info.AddByte( Convert.ToByte( arg ) );
                  break;
               case CILTypeCode.Char:
                  info.AddUncompressedUInt16( Convert.ToUInt16( Convert.ToChar( arg ) ) );
                  break;
               case CILTypeCode.Int16:
                  info.AddUncompressedInt16( Convert.ToInt16( arg ) );
                  break;
               case CILTypeCode.UInt16:
                  info.AddUncompressedUInt16( Convert.ToUInt16( arg ) );
                  break;
               case CILTypeCode.Int32:
                  info.AddUncompressedInt32( Convert.ToInt32( arg ) );
                  break;
               case CILTypeCode.UInt32:
                  info.AddUncompressedUInt32( Convert.ToUInt32( arg ) );
                  break;
               case CILTypeCode.Int64:
                  info.AddUncompressedInt64( Convert.ToInt64( arg ) );
                  break;
               case CILTypeCode.UInt64:
                  info.AddUncompressedUInt64( Convert.ToUInt64( arg ) );
                  break;
               case CILTypeCode.Single:
                  info.AddUncompressedSingle( Convert.ToSingle( arg ) );
                  break;
               case CILTypeCode.Double:
                  info.AddUncompressedDouble( Convert.ToDouble( arg ) );
                  break;
               case CILTypeCode.String:
                  info.AddCAString( Convert.ToString( arg ) );
                  break;
               case CILTypeCode.Type:
                  CILType argAsType;
                  if ( arg is Type )
                  {
                     argAsType = ( (Type) arg ).NewWrapperAsType( this._context );
                  }
                  else
                  {
                     argAsType = (CILType) arg;
                  }
                  info.AddTypeString( this._module, this._assemblyMapper.TryMapType( argAsType ) );
                  break;
               case CILTypeCode.SystemObject:
                  if ( arg == null )
                  {
                     // Nulls are serialized as null strings
                     if ( CILTypeCode.String != argType.TypeCode )
                     {
                        argType = this._module.AssociatedMSCorLibModule.GetTypeByName( Consts.STRING );
                     }
                  }
                  else if ( arg is CILType || arg is Type )
                  {
                     // Capture both native Type and CAM CILType
                     if ( CILTypeCode.Type != argType.TypeCode )
                     {
                        argType = this._module.AssociatedMSCorLibModule.GetTypeByName( Consts.TYPE );
                     }
                  }
                  this.WriteCustomAttributeFieldOrPropType( info, argType );
                  this.WriteCustomAttributeFixedArg( info, argType, argType, arg );
                  break;
               default:
                  throw new ArgumentException( "Unsupported type for custom attribute signature element: " + paramType + "." );
            }
         }
      }


      private void WriteCustomAttributeFieldOrPropType( BinaryHeapEmittingInfo info, CILType type )
      {
         if ( type.IsEnum() )
         {
            info.AddSigByte( SignatureElementTypes.CA_Enum );
            info.AddTypeString( this._module, type );
         }
         else if ( type.IsArray() )
         {
            info.AddSigByte( SignatureElementTypes.SzArray );
            this.WriteCustomAttributeFieldOrPropType( info, (CILType) type.ElementType );
         }
         else
         {
            var tc = type.TypeCode;
            SignatureElementTypes sigType;
            if ( !MetaDataConstants.TYPECODE_MAPPING_SIMPLE.TryGetValue( tc, out sigType ) )
            {
               // Either System.Type or System.Object
               switch ( tc )
               {
                  case CILTypeCode.SystemObject:
                     sigType = SignatureElementTypes.CA_Boxed;
                     break;
                  case CILTypeCode.Type:
                     sigType = SignatureElementTypes.Type;
                     break;
                  default:
                     throw new ArgumentException( "Unsupported type for custom attribute element: " + type + "." );
               }
            }
            info.AddSigByte( sigType );
         }
      }

      private void WriteConstantValue( BinaryHeapEmittingInfo info, CILTypeCode tc, Object arg )
      {
         switch ( tc )
         {
            case CILTypeCode.Boolean:
               info.AddByte( Convert.ToBoolean( arg ) ? (Byte) 1 : (Byte) 0 );
               break;
            case CILTypeCode.SByte:
               info.AddSByte( Convert.ToSByte( arg ) );
               break;
            case CILTypeCode.Byte:
               info.AddByte( Convert.ToByte( arg ) );
               break;
            case CILTypeCode.Char:
               info.AddUncompressedUInt16( Convert.ToUInt16( Convert.ToChar( arg ) ) );
               break;
            case CILTypeCode.Int16:
               info.AddUncompressedInt16( Convert.ToInt16( arg ) );
               break;
            case CILTypeCode.UInt16:
               info.AddUncompressedUInt16( Convert.ToUInt16( arg ) );
               break;
            case CILTypeCode.Int32:
               info.AddUncompressedInt32( Convert.ToInt32( arg ) );
               break;
            case CILTypeCode.UInt32:
               info.AddUncompressedUInt32( Convert.ToUInt32( arg ) );
               break;
            case CILTypeCode.Int64:
               info.AddUncompressedInt64( Convert.ToInt64( arg ) );
               break;
            case CILTypeCode.UInt64:
               info.AddUncompressedUInt64( Convert.ToUInt64( arg ) );
               break;
            case CILTypeCode.Single:
               info.AddUncompressedSingle( Convert.ToSingle( arg ) );
               break;
            case CILTypeCode.Double:
               info.AddUncompressedDouble( Convert.ToDouble( arg ) );
               break;
            case CILTypeCode.String:
               info.AddNormalString( Convert.ToString( arg ) );
               break;
            default:
               info.AddUncompressedInt32( 0 );
               break;
         }
      }

      private void WriteCustomAttributeNamedArg( BinaryHeapEmittingInfo info, CILCustomAttributeNamedArgument arg )
      {
         var elem = arg.NamedMember is CILField ? SignatureElementTypes.CA_Field : SignatureElementTypes.CA_Property;
         info.AddSigByte( elem );
         var fOrPType = (CILType) ( elem == SignatureElementTypes.CA_Field ? ( (CILField) arg.NamedMember ).FieldType : ( (CILProperty) arg.NamedMember ).GetPropertyType() );
         this.WriteCustomAttributeFieldOrPropType( info, fOrPType );
         info.AddCAString( arg.NamedMember.Name );
         this.WriteCustomAttributeFixedArg( info, fOrPType, arg.TypedValue.ArgumentType, arg.TypedValue.Value );
      }

      private void WriteTypeSignature( BinaryHeapEmittingInfo info, CILTypeBase type )
      {
         type = this._assemblyMapper.TryMapTypeBase( type );
         SignatureElementTypes simple;
         if ( !type.IsEnum() && MetaDataConstants.TYPECODE_MAPPING_FULL.TryGetValue( type.GetTypeCode( CILTypeCode.Empty ), out simple ) )
         {
            info.AddSigByte( simple );
         }
         else
         {
            if ( TypeKind.Type == type.TypeKind )
            {
               var rType = (CILType) type;
               var eKind = rType.ElementKind;
               if ( eKind.HasValue )
               {
                  if ( ElementKind.Array == eKind )
                  {
                     var arrayInfo = rType.ArrayInformation;
                     var isVector = arrayInfo == null;
                     info.AddSigByte( isVector ? SignatureElementTypes.SzArray : SignatureElementTypes.Array );
                     if ( isVector )
                     {
                        this.WriteCustomModifiers( info, null ); // TODO
                     }
                     this.WriteTypeSignature( info, rType.ElementType );
                     if ( !isVector )
                     {
                        info.AddCompressedUInt32( arrayInfo.Rank );

                        info.AddCompressedUInt32( arrayInfo.Sizes.Count );
                        arrayInfo.Sizes.ForEach( size => info.AddCompressedUInt32( size ) );

                        info.AddCompressedUInt32( arrayInfo.LowerBounds.Count );
                        arrayInfo.LowerBounds.ForEach( lobo => info.AddCompressedInt32( lobo ) );
                     }
                  }
                  else if ( ElementKind.Pointer == eKind )
                  {
                     info.AddSigByte( SignatureElementTypes.Ptr );
                     this.WriteCustomModifiers( info, null ); // TODO
                     this.WriteTypeSignature( info, rType.ElementType );
                  }
                  else
                  {
                     info.AddSigByte( SignatureElementTypes.ByRef );
                     this.WriteTypeSignature( info, rType.ElementType );
                  }
               }
               else
               {
                  var isGenericInstance = rType.IsGenericType(); // && ( ( typeContext != null && Object.ReferenceEquals( typeContext, rType.GenericDefinition ) ) || !rType.IsGenericTypeDefinition() );
                  if ( isGenericInstance )
                  {
                     info.AddSigByte( SignatureElementTypes.GenericInst );
                  }
                  info.AddSigByte( !rType.IsValueType() || rType.TypeCode == CILTypeCode.Enum ? SignatureElementTypes.Class : SignatureElementTypes.ValueType ); // this.IsEnum this.IsValueType
                  info.AddTDRSToken( this.GetTokenFor( isGenericInstance ? rType.GenericDefinition : rType, true ) );

                  if ( isGenericInstance )
                  {
                     var gArgs = rType.GenericArguments;
                     info.AddCompressedUInt32( gArgs.Count );
                     foreach ( var gArg in gArgs )
                     {
                        this.WriteTypeSignature( info, gArg );
                     }
                  }
               }
            }
            else if ( TypeKind.TypeParameter == type.TypeKind )
            {
               info.AddSigByte( ( (CILTypeParameter) type ).DeclaringMethod == null ? SignatureElementTypes.Var : SignatureElementTypes.MVar );
               info.AddCompressedUInt32( ( (CILTypeParameter) type ).GenericParameterPosition );
            }
            else
            {
               info.AddSigByte( SignatureElementTypes.FnPtr );
               this.WriteMethodSignature( info, (CILMethodSignature) type, null );
            }
         }
      }

      //private Boolean IsValueType( CILType type )
      //{
      //   return ( this._assemblyMapper == null ? type.BaseTypeChain() : type.BaseTypeChain().Select( t => (CILType) this._assemblyMapper.MapTypeBase( t ) ) ).Contains( this._valueTypeType );
      //}

      //private Boolean IsEnum( CILTypeBase type )
      //{
      //   return TypeKind.Type == type.TypeKind && ( (CILType) type ).BaseType != null && this._enumType.Equals( ( this._assemblyMapper == null ? ( (CILType) type ).BaseType : (CILType) this._assemblyMapper.MapTypeBase( ( (CILType) type ).BaseType ) ) );
      //}

      private void WriteMethodSpecSignature( BinaryHeapEmittingInfo info, CILMethod method )
      {
         info.AddSigStarterByte( SignatureStarters.MethodSpecGenericInst );
         info.AddCompressedUInt32( method.GenericArguments.Count );
         foreach ( var gArg in method.GenericArguments )
         {
            this.WriteTypeSignature( info, gArg );
         }
      }

      private void WriteCustomModifiers( BinaryHeapEmittingInfo info, IEnumerable<CILCustomModifier> mods )
      {
         if ( mods != null )
         {
            foreach ( var mod in mods )
            {
               info.AddSigByte( mod.Optionality.IsOptional() ? SignatureElementTypes.CModOpt : SignatureElementTypes.CModReqd );
               info.AddTDRSToken( this.GetTokenFor( mod.Modifier, true ) );
            }
         }
      }

      private UInt32 WriteMarshalInfo( MarshalingInfo info )
      {
         var emitter = new BinaryHeapEmittingInfo();
         emitter.AddCompressedUInt32( (Int32) info.Value );
         if ( !info.Value.IsNativeInstric() )
         {
            // Apparently Microsoft's implementation differs from ECMA-335 standard:
            // there the index of first parameter is 1, here all indices are zero-based.
            switch ( (UnmanagedType) info.Value )
            {
               case UnmanagedType.ByValTStr:
                  emitter.AddCompressedUInt32( info.ConstSize );
                  break;
               case UnmanagedType.IUnknown:
               case UnmanagedType.IDispatch:
                  if ( info.IIDParameterIndex >= 0 )
                  {
                     emitter.AddCompressedUInt32( info.IIDParameterIndex );
                  }
                  break;
               case UnmanagedType.SafeArray:
                  if ( info.SafeArrayType != CILAssemblyManipulator.API.VarEnum.VT_EMPTY )
                  {
                     emitter.AddCompressedUInt32( (Int32) info.SafeArrayType );
                     if ( CILAssemblyManipulator.API.VarEnum.VT_USERDEFINED == info.SafeArrayType )
                     {
                        emitter.AddTypeString( this._module, info.SafeArrayUserDefinedType );
                     }
                  }
                  break;
               case UnmanagedType.ByValArray:
                  emitter.AddCompressedUInt32( info.ConstSize );
                  if ( info.ArrayType != MarshalingInfo.NATIVE_TYPE_MAX )
                  {
                     emitter.AddCompressedUInt32( (Int32) info.ArrayType );
                  }
                  break;
               case UnmanagedType.LPArray:
                  emitter.AddCompressedUInt32( (Int32) info.ArrayType );
                  var hasSize = info.SizeParameterIndex != MarshalingInfo.NO_INDEX;
                  emitter.AddCompressedUInt32( hasSize ? info.SizeParameterIndex : 0 );
                  if ( info.ConstSize != MarshalingInfo.NO_INDEX )
                  {
                     emitter.AddCompressedUInt32( info.ConstSize );
                     emitter.AddCompressedUInt32( hasSize ? 1 : 0 ); // Indicate whether size-parameter was specified
                  }
                  break;
               case UnmanagedType.CustomMarshaler:
                  // For some reason, there are two compressed ints at this point
                  emitter.AddCompressedUInt32( 0 );
                  emitter.AddCompressedUInt32( 0 );
                  if ( info.MarshalType != null )
                  {
                     emitter.AddCAString( info.MarshalType );
                  }
                  else
                  {
                     emitter.AddTypeString( this._module, info.MarshalTypeRef );
                  }
                  emitter.AddCAString( info.MarshalCookie ?? "" );
                  break;
               default:
                  break;
            }
         }

         return this._blob.GetOrAddBLOB( emitter.CreateByteArray() );
      }

      private static void ForEachRow<T>( TableEmittingInfoWithList<T> tableEmittingInfo, Int32[] tableWidths, Stream sink, Action<Byte[], Int32, Int32, UInt32, T> writeAction )
      {
         var usingBlobIndex = tableEmittingInfo._blobIndexList != null;
         ForEachElement( tableEmittingInfo._tableEnum, tableEmittingInfo._list, tableWidths, sink, ( array, idx, listIdx, item ) => writeAction( array, idx, listIdx, usingBlobIndex ? tableEmittingInfo._blobIndexList[listIdx] : 0, item ) );
      }

      private static void ForEachElement<T>( Tables table, ICollection<T> list, Int32[] tableWidths, Stream sink, Action<Byte[], Int32, Int32, T> writeAction )
      {
         ForEachElementInEnumerable( table, list, tableWidths, sink, writeAction, list.Count );
      }

      private static void ForEachElement<T>( Tables table, ListProxy<T> list, Int32[] tableWidths, Stream sink, Action<Byte[], Int32, Int32, T> writeAction )
      {
         ForEachElementInEnumerable( table, list.CQ, tableWidths, sink, writeAction, list.CQ.Count );
      }

      private static void ForEachElementInEnumerable<T>( Tables table, IEnumerable<T> list, Int32[] tableWidths, Stream sink, Action<Byte[], Int32, Int32, T> writeAction, Int32 count )
      {
         if ( count > 0 )
         {

            var width = tableWidths[(Int32) table];
            var array = new Byte[width * count];
            var idx = 0;
            var listIdx = 0;
            foreach ( var item in list )
            {
               writeAction( array, idx, listIdx, item );
               idx += width;
               ++listIdx;
            }
            sink.Write( array );
#if DEBUG
            if ( idx != array.Length )
            {
               throw new Exception( "Something went wrong when emitting metadata array: emitted " + idx + " instead of expected " + array.Length + " bytes." );
            }
#endif
         }
      }

      private static Boolean IndexOf<T>( IList<T> list, T item, Func<T, T, Boolean> comparer, out Int32 idx )
      {
         idx = list.TakeWhile( t => !comparer( item, t ) ).Count();
         return idx < list.Count;
      }

      public void Dispose()
      {
         if ( this._hashStreamArgs != null )
         {
            var trans = this._hashStreamArgs.Transform;
            if ( trans != null )
            {
               this._hashStreamArgs.Transform.Dispose();
            }
         }
      }
   }

   internal class TableEmittingInfo<T>
   {
      internal readonly IDictionary<T, Int32> _table;
      internal readonly Int32 _tableMask;
      internal readonly Tables _tableEnum;

      internal TableEmittingInfo( Tables table, IEqualityComparer<T> eq )
      {
         this._tableMask = TokenUtils.EncodeToken( table, 0 );
         this._table = new Dictionary<T, Int32>( eq );
         this._tableEnum = table;
      }

      internal TableEmittingInfo( Tables table )
         : this( table, null )
      {
      }

      internal virtual Int32 GetOrAddTokenFor( T item, Object additionalInfo = null )
      {
         if ( this._table.ContainsKey( item ) )
         {
            return this._table[item] | this._tableMask;
         }
         else
         {
            return this.AddItem( item ) | this._tableMask;
         }
      }

      internal virtual Int32 GetOrAddTokenFor( T item, Action additionAction, Object additionalInfo = null )
      {
         if ( this._table.ContainsKey( item ) )
         {
            return this._table[item] | this._tableMask;
         }
         else
         {
            var result = this.AddItem( item ) | this._tableMask;
            additionAction();
            return result;
         }
      }

      internal virtual Boolean TryGetToken( T item, out Int32 token )
      {
         var result = this._table.TryGetValue( item, out token );
         token |= this._tableMask;
         return result;
      }

      internal virtual Int32 GetTokenFor( T item )
      {
         return this._table[item] | this._tableMask;
      }

      internal virtual Int32 AddItem( T item )
      {
         this._table.Add( item, this._table.Count + 1 );
         return this._table.Count;
      }

      internal Int32 GetTableIndexOf( T item )
      {
         return this._table[item];
      }

      internal virtual Boolean Contains( T item )
      {
         return this._table.ContainsKey( item );
      }

      internal Int32 Count
      {
         get
         {
            return this._table.Count;
         }
      }
   }

   internal class TableEmittingInfoWithList<T> : TableEmittingInfo<T>
   {
      protected static readonly IEqualityComparer<Byte[]> BYTE_ARRAY_EQ = new ByteArrayEqualityComparer();

      internal readonly Action<T, BinaryHeapEmittingInfo> _blobIndexFunc;
      internal readonly IList<T> _list;
      internal readonly IList<UInt32> _blobIndexList;
      internal readonly BLOBContainer _blob;
      private readonly Func<T, Int32?> _customAdditionAction;

      protected TableEmittingInfoWithList( Tables table, IEqualityComparer<T> eq, Action<T, BinaryHeapEmittingInfo> blobIndexFunc, BLOBContainer blob, Boolean useBlob )
         : base( table, eq )
      {
         this._list = new List<T>();
         this._blobIndexFunc = blobIndexFunc;
         this._blob = blob;
         if ( useBlob )
         {
            this._blobIndexList = new List<UInt32>();
         }
      }

      internal TableEmittingInfoWithList( Tables table, Func<T, Int32?> customAdditionAction = null )
         : this( table, null, null, null, false )
      {
         this._customAdditionAction = customAdditionAction;
      }

      internal TableEmittingInfoWithList( Tables table, Action<T, BinaryHeapEmittingInfo> blobIndexFunc, BLOBContainer blob )
         : this( table, null, blobIndexFunc, blob, blobIndexFunc != null )
      {

      }

      internal override int AddItem( T item )
      {
         return this.AddItem( item, true );
      }

      protected Int32 AddItem( T item, Boolean checkBlobIndexFunc )
      {
         var shouldAdd = this._customAdditionAction == null;
         Int32 retVal = -1;
         if ( !shouldAdd )
         {
            var customResult = this._customAdditionAction( item );
            if ( customResult.HasValue )
            {
               retVal = customResult.Value;
            }
            else
            {
               shouldAdd = true;
            }
         }

         if ( shouldAdd )
         {
            this._list.Add( item );
            if ( checkBlobIndexFunc && this._blobIndexFunc != null )
            {
               var info = new BinaryHeapEmittingInfo();
               this._blobIndexFunc( item, info );
               this._blobIndexList.Add( this._blob.GetOrAddBLOB( info.CreateByteArray() ) );
            }

            retVal = base.AddItem( item );
         }
         return retVal;
      }

   }

   internal class TableEmittingInfoWithBLOB<T> : TableEmittingInfoWithList<T>
   {
      internal readonly IDictionary<T, Byte[]> _sigCache;

      internal TableEmittingInfoWithBLOB( Tables table, BLOBContainer blob, IEqualityComparer<T> eq = null )
         : base( table, eq, null, blob, true )
      {
         this._sigCache = new Dictionary<T, Byte[]>();
      }

      protected Int32 AddItem( T item, Byte[] array )
      {
         this._blobIndexList.Add( this._blob.GetOrAddBLOB( array ) );
         return base.AddItem( item, false );
      }

      internal override bool Contains( T item )
      {
         return base.Contains( item ) || this._sigCache.ContainsKey( item );
      }

      internal override int AddItem( T item )
      {
         throw new NotSupportedException( "Don't use this method" );
      }
   }

   internal class TableEmittingInfoWithSignature<T> : TableEmittingInfoWithBLOB<T>
   {
      internal readonly Action<BinaryHeapEmittingInfo, Object, T> _signatureBuilder;
      private readonly IDictionary<Byte[], Int32> _sigIndices;

      internal TableEmittingInfoWithSignature( Tables table, BLOBContainer blob, Action<BinaryHeapEmittingInfo, Object, T> sigBuilder, IEqualityComparer<T> eq = null )
         : base( table, blob, eq )
      {
         this._signatureBuilder = sigBuilder;
         this._sigIndices = new Dictionary<Byte[], Int32>( BYTE_ARRAY_EQ );
      }

      internal override Int32 GetOrAddTokenFor( T item, Object additionalInfo = null )
      {
         Int32 result;
         if ( !this._table.TryGetValue( item, out result ) )
         {
            Byte[] array = this._sigCache.GetOrAdd_NotThreadSafe( item, () =>
            {
               var info = new BinaryHeapEmittingInfo();
               this._signatureBuilder( info, additionalInfo, item );
               return info.CreateByteArray();
            } );

            result = this._sigIndices.GetOrAdd_NotThreadSafe( array, () => base.AddItem( item, array ) );
         }
         return result | this._tableMask;
      }

      internal override Int32 GetTokenFor( T item )
      {
         Int32 result;
         if ( !this._table.TryGetValue( item, out result ) )
         {
            result = this._sigIndices[this._sigCache[item]];
         }
         return result | this._tableMask;
      }

      internal override Int32 AddItem( T item )
      {
         throw new NotSupportedException( "Use addition method with signature." );
      }
   }

   internal class TableEmittingInfoWithSignatureAndForeignKey<T, TKey> : TableEmittingInfoWithBLOB<T>
   {
      internal readonly IDictionary<TKey, IDictionary<Byte[], Int32>> _fkCache;
      private readonly Func<T, Object, TKey> _fkProcessor;
      private readonly Action<BinaryHeapEmittingInfo, T> _signatureBuilder;
      internal readonly IList<TKey> _keys;

      internal TableEmittingInfoWithSignatureAndForeignKey( Tables table, BLOBContainer blob, Func<T, Object, TKey> fkProcessor, Action<BinaryHeapEmittingInfo, T> sigBuilder, Boolean saveKeys )
         : base( table, blob )
      {
         this._fkCache = new Dictionary<TKey, IDictionary<Byte[], Int32>>();
         this._fkProcessor = fkProcessor;
         this._signatureBuilder = sigBuilder;
         if ( saveKeys )
         {
            this._keys = new List<TKey>();
         }
      }

      internal override Int32 GetOrAddTokenFor( T item, Object additionalInfo = null )
      {
         return this.GetOrAddTokenFor( item, null, additionalInfo );
      }

      internal override int GetOrAddTokenFor( T item, Action additionAction, Object additionalInfo = null )
      {
         Int32 result;
         if ( !this._table.TryGetValue( item, out result ) )
         {
            Byte[] array = this._sigCache.GetOrAdd_NotThreadSafe( item, () =>
            {
               var info = new BinaryHeapEmittingInfo();
               this._signatureBuilder( info, item );
               return info.CreateByteArray();
            } );

            var fkKey = this._fkProcessor( item, additionalInfo );
            var cache = this._fkCache.GetOrAdd_NotThreadSafe( fkKey, () => new Dictionary<Byte[], Int32>( BYTE_ARRAY_EQ ) );

            result = cache.GetOrAdd_NotThreadSafe( array, () =>
            {
               if ( additionAction != null )
               {
                  additionAction();
               }
               if ( this._keys != null )
               {
                  this._keys.Add( fkKey );
               }
               return base.AddItem( item, array );
            } );
         }
         return result | this._tableMask;
      }

      internal override Int32 GetTokenFor( T item )
      {
         throw new NotSupportedException();
      }

      internal override Int32 AddItem( T item )
      {
         throw new NotSupportedException( "Use addition method with signature." );
      }
   }

   internal class StringHeapEmittingInfo
   {
      private readonly IDictionary<String, Tuple<UInt32, Int32>> _strings;
      private readonly Boolean _saveSize;
      private readonly Encoding _encoding;
      private readonly Func<String, Byte[], Int32, Int32> _serializeFunc;
      private UInt32 _curIndex;
      private Boolean _notAccessed;

      internal StringHeapEmittingInfo( Encoding encoding, Boolean saveSize, Func<String, Byte[], Int32, Int32> serializeFunc )
      {
         this._encoding = encoding;
         this._saveSize = saveSize;
         this._serializeFunc = serializeFunc;
         this._strings = new Dictionary<String, Tuple<UInt32, Int32>>();
         this._curIndex = 1;
         this._notAccessed = true;
      }

      internal UInt32 GetOrAddString( String str, Boolean acceptNonEmpty )
      {
         if ( this._notAccessed )
         {
            this._notAccessed = false;
         }

         UInt32 result;
         Tuple<UInt32, Int32> strInfo;
         if ( str == null || ( str.Length == 0 && !acceptNonEmpty ) )
         {
            result = 0;
         }
         else if ( this._strings.TryGetValue( str, out strInfo ) )
         {
            result = strInfo.Item1;
         }
         else
         {
            result = this._curIndex;
            this.AddString( str );
         }
         return result;
      }

      internal UInt32 GetString( String str )
      {
         Tuple<UInt32, Int32> result;
         if ( str == null || !this._strings.TryGetValue( str, out result ) )
         {
            return 0;
         }
         return result.Item1;
      }

      internal UInt32 Size
      {
         get
         {
            return this._curIndex;
         }
      }

      internal Boolean Accessed
      {
         get
         {
            return !this._notAccessed;
         }
      }

      internal void WriteStrings( Stream sink )
      {
         if ( !this._notAccessed )
         {
            sink.WriteByte( 0 );
            if ( this._strings.Count > 0 )
            {
               var array = new Byte[BitUtils.MultipleOf4( this._curIndex ) - 1];
               var i = 0;
               foreach ( var kvp in this._strings )
               {
                  if ( this._saveSize )
                  {
                     array.CompressUInt32( ref i, kvp.Value.Item2 );
                  }
                  i += this._serializeFunc == null ? this._encoding.GetBytes( kvp.Key, 0, kvp.Key.Length, array, i ) : this._serializeFunc( kvp.Key, array, i );
                  if ( !this._saveSize )
                  {
                     array[i++] = 0;
                  }
               }
               sink.Write( array );
            }
         }
      }

      private void AddString( String str )
      {
         var byteCount = this._encoding.GetByteCount( str ) + 1;
         this._strings.Add( str, Tuple.Create( this._curIndex, byteCount ) );
         this._curIndex += (UInt32) byteCount;
         if ( this._saveSize )
         {
            this._curIndex += BitUtils.GetEncodedUIntSize( byteCount );
         }
      }
   }

   // TODO instead of always creating binary heap emitting infos, make them acquirable from instance pool (e.g. ConcurrentBag)
   internal class BinaryHeapEmittingInfo
   {

      private const Int32 DEFAULT_BLOCK_SIZE = 512;

      private readonly Int32 _blockSize;
      private readonly IList<Byte[]> _prevBlocks;
      private readonly IList<Int32> _prevBlockSizes;
      private readonly Encoding _stringEncoding;
      private Byte[] _curBlock;
      private Int32 _curCount;

      internal BinaryHeapEmittingInfo()
         : this( DEFAULT_BLOCK_SIZE )
      {

      }

      internal BinaryHeapEmittingInfo( Int32 blockSize )
      {
         this._blockSize = Math.Max( blockSize, DEFAULT_BLOCK_SIZE );
         this._prevBlocks = new List<Byte[]>();
         this._prevBlockSizes = new List<Int32>();
         this._curCount = 0;
         this._curBlock = new Byte[this._blockSize];
         this._stringEncoding = new UTF8Encoding( false, true );
      }

      internal void AddByte( Byte aByte )
      {
         this.EnsureSize( 1 );
         this._curBlock[this._curCount++] = aByte;
      }

      internal void AddSByte( SByte sByte )
      {
         this.EnsureSize( 1 );
         this._curBlock[this._curCount++] = (Byte) sByte;
      }

      internal void AddUncompressedInt16( Int16 val )
      {
         this.EnsureSize( 2 );
         this._curBlock.WriteInt16LEToBytes( ref this._curCount, val );
      }

      internal void AddUncompressedUInt16( UInt16 val )
      {
         this.EnsureSize( 2 );
         this._curBlock.WriteUInt16LEToBytes( ref this._curCount, val );
      }

      internal void AddUncompressedInt32( Int32 value )
      {
         this.EnsureSize( 4 );
         this._curBlock.WriteInt32LEToBytes( ref this._curCount, value );
      }

      internal void AddUncompressedUInt32( UInt32 value )
      {
         this.EnsureSize( 4 );
         this._curBlock.WriteUInt32LEToBytes( ref this._curCount, value );
      }

      internal void AddUncompressedInt64( Int64 value )
      {
         this.EnsureSize( 8 );
         this._curBlock.WriteInt64LEToBytes( ref this._curCount, value );
      }

      internal void AddUncompressedUInt64( UInt64 value )
      {
         this.EnsureSize( 8 );
         this._curBlock.WriteUInt64LEToBytes( ref this._curCount, value );
      }

      internal void AddUncompressedSingle( Single value )
      {
         this.EnsureSize( 4 );
         this._curBlock.WriteSingleLEToBytes( ref this._curCount, value );
      }

      internal void AddUncompressedDouble( Double value )
      {
         this.EnsureSize( 8 );
         this._curBlock.WriteDoubleLEToBytes( ref this._curCount, value );
      }

      internal void AddCAString( String str )
      {
         if ( str == null )
         {
            this.AddByte( 0xFF );
         }
         else
         {
            var size = this._stringEncoding.GetByteCount( str );
            this.AddCompressedUInt32( size );
            if ( this._curCount + size > this._blockSize )
            {
               this.AddBytes( this._stringEncoding.GetBytes( str ) );
            }
            else
            {
               this._curCount += this._stringEncoding.GetBytes( str, 0, str.Length, this._curBlock, this._curCount );
            }
         }
      }

      internal void AddTypeString( CILModule moduleBeingEmitted, CILType type )
      {
         this.AddCAString( Utils.CreateTypeString( type, moduleBeingEmitted, true ) );
      }

      internal void AddNormalString( String str )
      {
         // This is used only when storing string in constants table. The string must be stored using user string encoding (UTF16).
         if ( str == null )
         {
            this.AddByte( 0x00 );
         }
         else
         {
            var size = MetaDataConstants.USER_STRING_ENCODING.GetByteCount( str );
            if ( this._curCount + size > this._blockSize )
            {
               this.AddBytes( MetaDataConstants.USER_STRING_ENCODING.GetBytes( str ) );
            }
            else
            {
               this._curCount += MetaDataConstants.USER_STRING_ENCODING.GetBytes( str, 0, str.Length, this._curBlock, this._curCount );
            }
         }
      }

      internal void AddBytes( Byte[] bytes )
      {
         if ( bytes != null )
         {
            var written = 0;
            while ( written < bytes.Length )
            {
               if ( this._curCount == this._blockSize )
               {
                  this.EnsureSize( 1u );
               }
               var amountToWrite = Math.Min( bytes.Length - written, this._blockSize - this._curCount );
               this.EnsureSize( (UInt32) amountToWrite );
               Array.Copy( bytes, written, this._curBlock, this._curCount, amountToWrite );
               written += amountToWrite;
               this._curCount += amountToWrite;
            }
         }
      }

      internal void AddTDRSToken( Int32 token )
      {
         this.AddCompressedUInt32( TokenUtils.EncodeTypeDefOrRefOrSpec( token ) );
      }

      internal void AddCompressedUInt32( Int32 value )
      {
         this.EnsureSize( BitUtils.GetEncodedUIntSize( value ) );
         this._curBlock.CompressUInt32( ref this._curCount, value );
      }

      internal void AddCompressedInt32( Int32 value )
      {
         this.EnsureSize( BitUtils.GetEncodedIntSize( value ) );
         this._curBlock.CompressInt32( ref this._curCount, value );
      }

      internal void AddSigByte( SignatureElementTypes sigType )
      {
         this.AddByte( (Byte) sigType );
      }

      internal void AddSigStarterByte( SignatureStarters sigStarter )
      {
         this.AddByte( (Byte) sigStarter );
      }

      internal Byte[] CreateByteArray()
      {
         var result = new Byte[this._prevBlockSizes.Sum() + this._curCount];
         var curIdx = 0;
         for ( var i = 0; i < this._prevBlocks.Count; ++i )
         {
            Array.Copy( this._prevBlocks[i], 0, result, curIdx, this._prevBlockSizes[i] );
            curIdx += this._prevBlockSizes[i];
         }
         Array.Copy( this._curBlock, 0, result, curIdx, this._curCount );
         return result;
      }

      private void EnsureSize( UInt32 size )
      {
         if ( this._blockSize < this._curCount + size )
         {
            this._prevBlocks.Add( this._curBlock );
            this._prevBlockSizes.Add( this._curCount );
            this._curBlock = new Byte[this._blockSize];
            this._curCount = 0;
         }
      }

   }

   internal class ByteArrayEqualityComparer : IEqualityComparer<Byte[]>
   {
      #region IEqualityComparer<byte[]> Members

      public Boolean Equals( Byte[] x, Byte[] y )
      {
         var result = x.Length == y.Length;
         if ( result )
         {
            for ( var i = 0; i < x.Length; ++i )
            {
               if ( x[i] != y[i] )
               {
                  return false;
               }
            }
         }
         return result;
      }

      public Int32 GetHashCode( Byte[] obj )
      {
         var result = 0;
         for ( var i = 0; i < obj.Length; ++i )
         {
            result += obj[i];
         }
         return result + obj.Length;
      }

      #endregion
   }

   internal class BLOBContainer
   {
      private readonly IDictionary<Byte[], UInt32> _blobIndices;
      private readonly IList<Byte[]> _blobs;
      private UInt32 _curIdx;
      private Boolean _notAccessed;

      internal BLOBContainer()
      {
         this._blobIndices = new Dictionary<Byte[], UInt32>( new ByteArrayEqualityComparer() );
         this._blobs = new List<Byte[]>();
         this._curIdx = 1;
         this._notAccessed = true;
      }

      internal UInt32 GetOrAddBLOB( Byte[] blob )
      {
         if ( this._notAccessed )
         {
            this._notAccessed = false;
         }
         if ( blob == null )
         {
            return 0;
         }
         else
         {
            UInt32 result;
            if ( !this._blobIndices.TryGetValue( blob, out result ) )
            {
               result = this._curIdx;
               this._blobIndices.Add( blob, result );
               this._blobs.Add( blob );
               this._curIdx += (UInt32) blob.Length + BitUtils.GetEncodedUIntSize( blob.Length );
            }
            return result;
         }
      }

      internal UInt32 GetBLOB( Byte[] blob )
      {
         return blob == null ? 0 : this._blobIndices[blob];
      }

      internal UInt32 Size
      {
         get
         {
            return this._curIdx;
         }
      }

      internal Boolean Accessed
      {
         get
         {
            return !this._notAccessed;
         }
      }

      internal void WriteBLOBs( Stream sink )
      {
         if ( !this._notAccessed )
         {
            sink.WriteByte( 0 );
            var byteCount = 1u;
            if ( this._blobs.Count > 0 )
            {
               var tmpArray = new Byte[4];
               foreach ( var blob in this._blobs )
               {
                  var i = 0;
                  tmpArray.CompressUInt32( ref i, blob.Length );
                  sink.Write( tmpArray, 0, i );
                  sink.Write( blob );
                  byteCount += (UInt32) i + (UInt32) blob.Length;
               }
            }
            sink.SkipToNextAlignment( ref byteCount, 4 );
         }
      }
   }

}