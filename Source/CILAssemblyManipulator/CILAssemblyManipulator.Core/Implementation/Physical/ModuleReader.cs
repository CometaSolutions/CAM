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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using CILAssemblyManipulator.API;
using CILAssemblyManipulator.Implementation;
using CollectionsWithRoles.API;
using CommonUtils;
using TExceptionBlockInfo = System.Tuple<CILAssemblyManipulator.API.ExceptionBlockType, System.UInt32, System.UInt32, System.UInt32, System.UInt32, System.UInt32, System.UInt32>;
using TFieldSig = System.Tuple<CollectionsWithRoles.API.ListProxy<CILAssemblyManipulator.API.CILCustomModifier>, CILAssemblyManipulator.API.CILTypeBase>;
using TMethodCodeInfo = System.Tuple<System.Boolean, CILAssemblyManipulator.Implementation.Physical.TableIndex?, System.Byte[], System.Tuple<CILAssemblyManipulator.API.ExceptionBlockType, System.UInt32, System.UInt32, System.UInt32, System.UInt32, System.UInt32, System.UInt32>[]>;
using TMethodDefOrRefSig = System.Tuple<System.Tuple<CollectionsWithRoles.API.ListProxy<CILAssemblyManipulator.API.CILCustomModifier>, CILAssemblyManipulator.API.CILTypeBase>, System.Collections.Generic.IList<System.Tuple<CollectionsWithRoles.API.ListProxy<CILAssemblyManipulator.API.CILCustomModifier>, CILAssemblyManipulator.API.CILTypeBase>>, System.Collections.Generic.IList<System.Tuple<CollectionsWithRoles.API.ListProxy<CILAssemblyManipulator.API.CILCustomModifier>, CILAssemblyManipulator.API.CILTypeBase>>>;
using TRVA = System.UInt32;
using TSection = System.Tuple<System.String, System.UInt32, System.UInt32, System.UInt32, System.UInt32>;

namespace CILAssemblyManipulator.Implementation.Physical
{
   internal class ModuleReader
   {


      private struct DataDir
      {
         internal UInt32 rva;
         internal UInt32 size;

         internal DataDir( Stream stream, Byte[] tmpArray )
         {
            this.rva = stream.ReadU32( tmpArray );
            this.size = stream.ReadU32( tmpArray );
         }
      }

      internal static readonly Type TARGET_FRAMEWORK_TYPE = typeof( TargetFrameworkAttribute );
      internal static readonly System.Reflection.ConstructorInfo TARGET_FRAMEWORK_ATTRIBUTE_CTOR = typeof( TargetFrameworkAttribute ).LoadConstructorOrThrow( new Type[] { typeof( String ) } );
      internal static readonly System.Reflection.PropertyInfo TARGET_FRAMEWORK_ATTRIBUTE_NAMED_PROPERTY = typeof( TargetFrameworkAttribute ).LoadPropertyOrThrow( "FrameworkDisplayName" );

      // MemberRef cache doesn't seem to have any actual impact on performance.
      //private static readonly IEqualityComparer<Tuple<Int32, CILTypeBase[], CILTypeBase[], ILResolveKind>> MEMBER_REF_CACHE_EQUALITY_COMPARER = ComparerFromFunctions.NewEqualityComparer<Tuple<Int32, CILTypeBase[], CILTypeBase[], ILResolveKind>>(
      //   ( m1, m2 ) => m1.Item1 == m2.Item1 && m1.Item4 == m2.Item4 && ArrayEqualityComparer<CILTypeBase>.DefaultArrayEqualityComparer.Equals( m1.Item2, m2.Item2 ) && ArrayEqualityComparer<CILTypeBase>.DefaultArrayEqualityComparer.Equals( m1.Item3, m2.Item3 ),
      //   m => ( m.Item1 << 24 ) | ( ( ArrayEqualityComparer<CILTypeBase>.DefaultArrayEqualityComparer.GetHashCode( m.Item2 ) ^ ArrayEqualityComparer<CILTypeBase>.DefaultArrayEqualityComparer.GetHashCode( m.Item3 ) ) >> 8 ) );

      private static readonly IList<Int32> EMPTY_INDICES = new List<Int32>( 0 );

      private readonly CILReflectionContextImpl _ctx;
      private readonly TSection[] _sections;
      private readonly MetaDataReader _md;
      private readonly CILType[] _typeDef;
      private readonly CILMethodBase[] _methods;
      private readonly CILField[] _fields;
      private readonly IDictionary<CILElementWithGenericArguments<Object>, CILTypeBase[]> _gParams;

      private readonly Lazy<CILType>[] _typeRef;
      private readonly TMethodCodeInfo[] _methodDefRVAContents;
      private readonly Byte[][] _fieldRVAContents;
      private readonly Lazy<Boolean> _thisIsMSCorLib;
      private readonly IDictionary<SignatureElementTypes, Lazy<CILType>> _coreTypes;
      private readonly Lazy<CILType> _enumType;
      private readonly ConcurrentDictionary<String, CILType> _typeStringResolutions;
      //private readonly ConcurrentDictionary<Tuple<Int32, CILTypeBase[], CILTypeBase[], ILResolveKind>, CILElementTokenizableInILCode> _memberRefCache;

      private readonly CILAssembly[] _resolvedAssemblyRefs;
      private readonly Object _resolvedAssemblyRefsLock;
      internal readonly Lazy<CILModule> _mscorLibRef;

      private readonly Func<CILModule, CILAssemblyName, CILAssembly> _customAssemblyRefLoader;

      private readonly Int32 _epToken;
      private readonly EmittingArguments _eArgs;

      private CILModuleImpl _thisModule;

      internal ModuleReader(
         CILReflectionContextImpl ctx,
         Stream stream,
         EmittingArguments eArgs,
         out TargetRuntime targetRuntime,
         out DLLFlags dllFlags,
         out MetaDataReader md,
         out IDictionary<String, ManifestResource> manifestResources
         )
      {
         this._ctx = ctx;
         this._eArgs = eArgs;

         Byte[] tmpArray = new Byte[8];

         // DOS header, skip to lfa new
         stream.SeekFromBegin( 60 );

         // PE file header
         // Skip to PE file header, and skip magic
         var suuka = stream.ReadU32( tmpArray );
         stream.SeekFromBegin( suuka + 4 );

         // Architecture
         var architecture = (ImageFileMachine) stream.ReadU16( tmpArray );

         eArgs.Machine = architecture;
         // Amount of sections
         var amountOfSections = stream.ReadU16( tmpArray );

         // Skip timestamp, symbol table pointer, number of symbols
         stream.SeekFromCurrent( 12 );

         // Optional header size
         stream.ReadU16( tmpArray );

         // Characteristics
         var characteristics = stream.ReadU16( tmpArray );

         // PE Optional header
         // Skip standard fields and all NT-specific fields until subsystem
         stream.SeekFromCurrent( 68 ); // Value is the same for both pe32 & pe64, since BaseOfData is lacking from pe64

         // Subsystem
         var subsystem = stream.ReadU16( tmpArray );

         // DLL flags
         dllFlags = (DLLFlags) stream.ReadU16( tmpArray );

         // Skip to debug header
         stream.SeekFromCurrent( architecture.RequiresPE64() ? 88 : 72 ); // PE64 requires 8 bytes for stack reserve & commit sizes, and heap reserve & commit sizes
         var debugDD = new DataDir( stream, tmpArray );

         // Skip to CLI header
         stream.SeekFromCurrent( 56 );

         // CLI header
         var cliDD = new DataDir( stream, tmpArray );

         // Reserved
         stream.SeekFromCurrent( 8 );

         // Read sections
         this._sections = new TSection[amountOfSections];
         for ( var i = 0u; i < amountOfSections; ++i )
         {
            // VS2012 evaluates positional arguments from left to right, so creating Tuple should work correctly
            // This is not so in VS2010 ( see http://msdn.microsoft.com/en-us/library/hh678682.aspx )
            stream.ReadWholeArray( tmpArray ); // tmpArray is 8 bytes long
            this._sections[i] = Tuple.Create(
               tmpArray.ReadZeroTerminatedASCIIStringFromBytes(), // Section name
               stream.ReadU32( tmpArray ), // Virtual size
               stream.ReadU32( tmpArray ), // Virtual address
               stream.ReadU32( tmpArray ), // Raw data size
               stream.ReadU32( tmpArray ) // Raw data pointer
               );
            // Skip number of relocation & line numbers, and section characteristics
            stream.SeekFromCurrent( 16 );
         }

         // CLI header, skip magic and runtime versions
         stream.SeekFromBegin( ResolveRVA( cliDD.rva ) + 8 );

         // Metadata datadirectory
         var mdDD = new DataDir( stream, tmpArray );

         // Module flags
         var moduleFlags = (ModuleFlags) stream.ReadU32( tmpArray );
         eArgs.ModuleFlags = moduleFlags;

         // Entrypoint token
         this._epToken = (Int32) stream.ReadU32( tmpArray );

         // Resources data directory
         var rsrcDD = new DataDir( stream, tmpArray );

         // Strong name
         //var snDD = new DataDir( stream, tmpArray );

         // Skip code manager table, virtual table fixups, export address table jumps, and managed native header data directories
         //stream.SeekFromCurrent( 32 );

         // Metadata
         stream.SeekFromBegin( ResolveRVA( mdDD.rva ) );
         String mdVersion;
         md = new MetaDataReader( stream, out mdVersion );
         targetRuntime = Utils.GetTargetRuntimeBasedOnMetadataVersion( mdVersion );
         eArgs.MetaDataVersion = mdVersion;
         var mGUID = md.module[0].Item3;
         if ( mGUID.HasValue )
         {
            eArgs.ModuleID = mGUID.Value;
         }
         this._md = md;

         var moduleKind = GetModuleKind( characteristics, subsystem, md.assembly.Length );
         eArgs.ModuleKind = moduleKind;

         // Put this behind lazy so that users of emitting arguments can set CorLibName after loading and this would still work.
         this._thisIsMSCorLib = new Lazy<Boolean>( () => this._md.assembly.Length == 1 && String.Equals( this._md.assembly[0].Item8, eArgs.CorLibName ), LazyThreadSafetyMode.ExecutionAndPublication );

         // Read RVAs
         this._methodDefRVAContents = new TMethodCodeInfo[md.methodDef.Length];
         for ( var i = 0; i < md.methodDef.Length; ++i )
         {
            var mRow = md.methodDef[i];
            if ( mRow.Item1 != 0 && mRow.Item2.IsIL() )
            {
               var curRVA = mRow.Item1;
               stream.SeekFromBegin( this.ResolveRVA( curRVA ) );
               this._methodDefRVAContents[i] = ReadMethodBytes( stream );
            }
         }
         this._fieldRVAContents = new Byte[md.fieldRVA.Length][];
         for ( var i = 0; i < md.fieldRVA.Length; ++i )
         {
            var curRVA = md.fieldRVA[i].Item1;
            try
            {
               var array = new Byte[CalculateFieldTypeSize( md.field[md.fieldRVA[i].Item2.idx].Item3 )];
               // Field RVA should be non-zero
               stream.SeekFromBegin( this.ResolveRVA( curRVA ) );
               stream.ReadWholeArray( array );
               this._fieldRVAContents[i] = array;
            }
            catch
            {
               // Sometimes, field RVA points outside the file (some C++ thing?)
               // TODO report exception... or maybe add to some kind of list in EmittingArguments?
               //this._fieldRVAContents[i] = null;
            }
         }

         // Read debug directory
         if ( debugDD.rva != 0 && debugDD.size >= MetaDataConstants.DEBUG_DD_SIZE )
         {
            var array = new Byte[MetaDataConstants.DEBUG_DD_SIZE];
            stream.SeekFromBegin( this.ResolveRVA( debugDD.rva ) );
            stream.ReadWholeArray( array );
            var dbgInfo = new EmittingDebugInformation();
            var idx = 0;
            dbgInfo.Characteristics = array.ReadInt32LEFromBytes( ref idx );
            dbgInfo.Timestamp = array.ReadInt32LEFromBytes( ref idx );
            dbgInfo.VersionMajor = array.ReadInt16LEFromBytes( ref idx );
            dbgInfo.VersionMinor = array.ReadInt16LEFromBytes( ref idx );
            dbgInfo.DebugType = array.ReadInt32LEFromBytes( ref idx );
            var dataSize = array.ReadInt32LEFromBytes( ref idx );
            var dbgPos = this.ResolveRVA( array.ReadUInt32LEFromBytes( ref idx ) );
            if ( dbgPos == array.ReadUInt32LEFromBytes( ref idx ) )
            {
               stream.SeekFromBegin( dbgPos );
               dbgInfo.DebugData = new Byte[dataSize];
               stream.ReadWholeArray( dbgInfo.DebugData );
            }
            eArgs.DebugInformation = dbgInfo;
         }

         // Read resource directory
         var hasManifestResources = rsrcDD.rva > 0 && rsrcDD.size > 0;
         manifestResources = new Dictionary<String, ManifestResource>();
         foreach ( var mres in md.manifestResource )
         {
            if ( !manifestResources.ContainsKey( mres.Item3 ) )
            {
               ManifestResource res = null;
               if ( mres.Item4.HasValue )
               {
                  var refTable = mres.Item4.Value.table;
                  var refIdx = mres.Item4.Value.idx;
                  if ( refTable == Tables.File && refIdx < this._md.file.Length && this._md.file[refIdx].Item1 == FileAttributes.ContainsNoMetadata )
                  {
                     res = new FileManifestResource( mres.Item2, this._md.file[refIdx].Item2, this._md.file[refIdx].Item3 );
                  }
                  else
                  {
                     res = new ModuleManifestResource( mres.Item2, () =>
                     {
                        CILModule mModule = null;
                        if ( refTable == Tables.File && refIdx < this._md.file.Length )
                        {
                           mModule = this._thisModule.Assembly.Modules.FirstOrDefault( m => String.Equals( m.Name, this._md.file[refIdx].Item2 ) );
                        }
                        else if ( refTable == Tables.AssemblyRef && refIdx < this._md.assemblyRef.Length )
                        {
                           var ass = this.TryResolveAssemblyRef( refIdx + 1 );
                           if ( ass != null )
                           {
                              mModule = ass.MainModule;
                           }
                        }
                        if ( mModule == null )
                        {
                           throw new BadImageFormatException( "The row for manifest resource " + mres.Item3 + " contained invalid data." );
                        }
                        return mModule;
                     } );
                  }
               }
               else
               {
                  if ( hasManifestResources && mres.Item1 < rsrcDD.size )
                  {
                     // Read embedded resource
                     stream.SeekFromBegin( this.ResolveRVA( rsrcDD.rva ) + mres.Item1 );
                     var length = stream.ReadU32( tmpArray );
                     var data = new Byte[length];
                     stream.ReadWholeArray( data );
                     res = new EmbeddedManifestResource( mres.Item2, data );
                  }
               }
               // TODO maybe throw exception if res is null? or just not add?
               manifestResources.Add( mres.Item3, res );
            }
         }

         // Prepare logical structures
         this._typeDef = new CILType[md.typeDef.Length];
         this._methods = new CILMethodBase[md.methodDef.Length];
         this._fields = new CILField[md.field.Length];
         this._gParams = new Dictionary<CILElementWithGenericArguments<Object>, CILTypeBase[]>( md.genericParam.Length );
         this._typeRef = new Lazy<CILType>[md.typeRef.Length];
         for ( var i = 0; i < this._typeRef.Length; ++i )
         {
            // Don't use loop variable in lambda
            var trIdx = i;
            this._typeRef[i] = new Lazy<CILType>( () => this.ResolveTypeRef( trIdx ), LazyThreadSafetyMode.ExecutionAndPublication );
         }

         this._coreTypes = new Dictionary<SignatureElementTypes, Lazy<CILType>>()
         {
            { SignatureElementTypes.Boolean, new Lazy<CILType>( () => this.GetCoreType( typeof( Boolean ) ), LazyThreadSafetyMode.ExecutionAndPublication) },
            { SignatureElementTypes.Char, new Lazy<CILType>( () => this.GetCoreType( typeof( Char ) ) , LazyThreadSafetyMode.ExecutionAndPublication) },
            { SignatureElementTypes.I1, new Lazy<CILType>( () => this.GetCoreType( typeof( SByte ) ) , LazyThreadSafetyMode.ExecutionAndPublication) },
            { SignatureElementTypes.U1, new Lazy<CILType>( () => this.GetCoreType( typeof( Byte ) ) , LazyThreadSafetyMode.ExecutionAndPublication) },
            { SignatureElementTypes.I2, new Lazy<CILType>( () => this.GetCoreType( typeof( Int16 ) ) , LazyThreadSafetyMode.ExecutionAndPublication) },
            { SignatureElementTypes.U2, new Lazy<CILType>( () => this.GetCoreType( typeof( UInt16 ) ) , LazyThreadSafetyMode.ExecutionAndPublication) },
            { SignatureElementTypes.I4, new Lazy<CILType>( () => this.GetCoreType( typeof( Int32 ) ) , LazyThreadSafetyMode.ExecutionAndPublication) },
            { SignatureElementTypes.U4, new Lazy<CILType>( () => this.GetCoreType( typeof( UInt32 ) ) , LazyThreadSafetyMode.ExecutionAndPublication) },
            { SignatureElementTypes.I8, new Lazy<CILType>( () => this.GetCoreType( typeof( Int64 ) ) , LazyThreadSafetyMode.ExecutionAndPublication) },
            { SignatureElementTypes.U8, new Lazy<CILType>( () => this.GetCoreType( typeof( UInt64 ) ) , LazyThreadSafetyMode.ExecutionAndPublication) },
            { SignatureElementTypes.I, new Lazy<CILType>( () => this.GetCoreType( typeof( IntPtr ) ) , LazyThreadSafetyMode.ExecutionAndPublication) },
            { SignatureElementTypes.U, new Lazy<CILType>( () => this.GetCoreType( typeof( UIntPtr ) ) , LazyThreadSafetyMode.ExecutionAndPublication) },
            { SignatureElementTypes.R4, new Lazy<CILType>( () => this.GetCoreType( typeof( Single ) ) , LazyThreadSafetyMode.ExecutionAndPublication) },
            { SignatureElementTypes.R8, new Lazy<CILType>( () => this.GetCoreType( typeof( Double ) ) , LazyThreadSafetyMode.ExecutionAndPublication) },
            { SignatureElementTypes.String, new Lazy<CILType>( () => this.GetCoreType( typeof( String ) ) , LazyThreadSafetyMode.ExecutionAndPublication) },
            { SignatureElementTypes.Void, new Lazy<CILType>( () => this.GetCoreType( typeof( void ) ) , LazyThreadSafetyMode.ExecutionAndPublication) },
            { SignatureElementTypes.Object, new Lazy<CILType>( () => this.GetCoreType( typeof( Object ) ) , LazyThreadSafetyMode.ExecutionAndPublication) },
            { SignatureElementTypes.TypedByRef, new Lazy<CILType>( () => this.GetCoreType( null, "System", "TypedReference" ) , LazyThreadSafetyMode.ExecutionAndPublication) },
            { SignatureElementTypes.Type, new Lazy<CILType>( () => this.GetCoreType( typeof( Type ) ) , LazyThreadSafetyMode.ExecutionAndPublication) }
         };

         this._enumType = new Lazy<CILType>( () => this.GetCoreType( typeof( Enum ) ), LazyThreadSafetyMode.ExecutionAndPublication );

         this._typeStringResolutions = new ConcurrentDictionary<String, CILType>();
         //this._memberRefCache = new ConcurrentDictionary<Tuple<Int32, CILTypeBase[], CILTypeBase[], ILResolveKind>, CILElementTokenizableInILCode>( MEMBER_REF_CACHE_EQUALITY_COMPARER );

         this._customAssemblyRefLoader = eArgs.AssemblyRefLoader;
         this._resolvedAssemblyRefs = new CILAssembly[md.assemblyRef.Length + 1];
         this._resolvedAssemblyRefsLock = new Object();
         this._mscorLibRef = new Lazy<CILModule>( this.ResolveMSCorLib, LazyThreadSafetyMode.ExecutionAndPublication );

         // Peek custom attribute data for TargetFramework attribute
         try
         {
            foreach ( var cRowIdx in GetReferencingRowsFromOrdered( md.customAttribute, Tables.Assembly, 0, cr => cr.Item1 ) )
            {
               var cRow = md.customAttribute[cRowIdx];
               Boolean isTargetFW = false;
               var tIdx = cRow.Item2;
               switch ( tIdx.table )
               {
                  //case Tables.MethodDef:
                  //   // Find decl type
                  //   if ( this._thisIsMSCorLib )
                  //   {
                  //      for ( var ti = 0; ti < md.typeDef.Length; ++ti )
                  //      {
                  //         if ( md.typeDef[ti].Item6.idx > tIdx.idx && ti > 0 )
                  //         {
                  //            isTargetFW = String.Equals( TARGET_FRAMEWORK_TYPE.Name, md.typeDef[ti - 1].Item2, StringComparison.CurrentCulture )
                  //               && String.Equals( TARGET_FRAMEWORK_TYPE.Namespace, md.typeDef[ti - 1].Item3, StringComparison.CurrentCulture );
                  //            break;
                  //         }
                  //      }
                  //   }
                  //   break;
                  case Tables.MemberRef:
                     var mr = md.memberRef[tIdx.idx];
                     if ( mr.Item1.table == Tables.TypeRef )
                     {
                        var tr = md.typeRef[mr.Item1.idx];
                        isTargetFW = String.Equals( TARGET_FRAMEWORK_TYPE.Name, tr.Item2, StringComparison.CurrentCulture ) && String.Equals( TARGET_FRAMEWORK_TYPE.Namespace, tr.Item3, StringComparison.CurrentCulture );
                     }
                     break;
               }
               if ( isTargetFW )
               {
                  // First argument is always the type string
                  Int32 idx = 2; // Skip prolog
                  String fwName, fwVersion, fwProfile;
                  ParseFWStr( cRow.Item3.ReadLenPrefixedUTF8String( ref idx ), out fwName, out fwVersion, out fwProfile );
                  eArgs.FrameworkVersion = fwVersion;
                  eArgs.FrameworkName = fwName;
                  eArgs.FrameworkProfile = fwProfile;
                  break;
               }
            }
         }
         catch
         {
            // Just ignore.
         }

         // Set token resolving callbacks.
         eArgs.SetTokenFunctions( this.TokenResolverCallback, el =>
         {
            throw new InvalidOperationException( "Token encoding is not support for emitting arguments used for loading." );
         }, this.TokenSignatureResolverCallback, locals =>
         {
            throw new InvalidOperationException( "Token encoding is not support for emitting arguments used for loading." );
         } );

         // Set metadata info
         // Note - if subsequently type structure is modified, before accessing the metadata info, then this information won't be consistent.
         eArgs.SetMDInfo( () => new EmittingMetadataInfo(
            new List<CILType>( this._typeDef ),
            new List<CILMethodBase>( this._methods ),
            this._md.methodDef.SelectMany( ( t, i ) =>
            {
               var max = i + 1 < this._md.methodDef.Length ?
                  this._md.methodDef[i + 1].Item6.idx :
                  this._md.param.Length;
               var arr = new CILParameter[max - t.Item6.idx];
               var start = this._md.methodDef[i].Item6.idx;
               var m = this._methods[i];
               for ( var j = 0; j < arr.Length; ++j )
               {
                  var pIdx = this._md.param[start + j].Item2;
                  arr[j] = pIdx == 0 ? ( (CILMethod) m ).ReturnParameter : m.Parameters[pIdx - 1];
               }
               return arr;
            } ).ToList(),
            this._typeDef.SelectMany( t => t.DeclaredFields ).ToList(),
            this._md.methodSemantics
               .Where( t => t.Item3.table == Tables.Property )
               .Select( t => this._methods[t.Item2.idx].DeclaringType.DeclaredProperties.FirstOrDefault( p => p.GetSemanticMethods().Select( tt => tt.Item2 ).Contains( (CILMethod) this._methods[t.Item2.idx] ) ) )
               .Distinct()
               .ToList(),
            this._md.methodSemantics
               .Where( t => t.Item3.table == Tables.Event )
               .Select( t => this._methods[t.Item2.idx].DeclaringType.DeclaredEvents.FirstOrDefault( e => e.GetSemanticMethods().Select( tt => tt.Item2 ).Contains( (CILMethod) this._methods[t.Item2.idx] ) ) )
               .Distinct()
               .ToList(),
            this._md.methodDef.Select( t => t.Item1 ).ToList()
            ) );
      }

      private CILElementTokenizableInILCode TokenResolverCallback( CILMethodBase method, Int32 token )
      {
         // Make sure to initialize logical structure
         var dummy = this._thisModule.DefinedTypes;

         // Proceed with token resolving
         Tables table;
         Int32 idx;
         TokenUtils.DecodeTokenZeroBased( token, out table, out idx );
         CILElementTokenizableInILCode result = null;
         if ( idx >= 0 )
         {
            var tIdx = new TableIndex( table, idx );
            // Prepare gArgs
            var typeGArgs = method == null ? null : method.DeclaringType.GenericArguments.ToArray();
            var methodGArgs = method == null || method.MethodKind == MethodKind.Constructor ? null : ( (CILMethod) method ).GenericArguments.ToArray();
            switch ( table )
            {
               case Tables.TypeDef:
                  if ( idx < this._typeDef.Length )
                  {
                     result = this.ResolveType( tIdx, typeGArgs, methodGArgs );
                  }
                  break;
               case Tables.TypeRef:
                  if ( idx < this._typeRef.Length )
                  {
                     result = this.ResolveType( tIdx, typeGArgs, methodGArgs );
                  }
                  break;
               case Tables.TypeSpec:
                  if ( idx < this._md.typeSpec.Length )
                  {
                     result = this.ResolveType( tIdx, typeGArgs, methodGArgs );
                  }
                  break;
               case Tables.StandaloneSignature:
                  if ( idx < this._md.standaloneSig.Length && this._md.standaloneSig[idx].Length > 0 && this._md.standaloneSig[idx][0] != (Byte) SignatureStarters.LocalSignature )
                  {
                     result = this.ResolveMethodSigFromTableIndex( idx, typeGArgs, methodGArgs ).Item1;
                  }
                  break;
               case Tables.Field:
                  if ( idx < this._fields.Length )
                  {
                     result = this._fields[idx];
                  }
                  break;
               case Tables.MethodDef:
                  if ( idx < this._methods.Length )
                  {
                     result = this._methods[idx];
                  }
                  break;
               case Tables.MemberRef:
                  if ( idx < this._md.memberRef.Length )
                  {
                     result = this.ResolveMemberRef( idx, typeGArgs, methodGArgs, ILResolveKind.MethodBaseOrField );
                  }
                  break;
               case Tables.MethodSpec:
                  if ( idx < this._md.methodSpec.Length )
                  {
                     result = this.ResolveMethodSpec( idx, typeGArgs, methodGArgs );
                  }
                  break;
            }
         }
         return result;
      }

      private IEnumerable<CILMethodBase> TokenSignatureResolverCallback( Int32 token )
      {
         Tables table;
         Int32 idx;
         TokenUtils.DecodeTokenZeroBased( token, out table, out idx );
         return Tables.StandaloneSignature == table ?
            this._methodDefRVAContents
               .Select( ( tuple, mIdx ) => Tuple.Create( tuple, mIdx ) )
               .Where( tuple => tuple.Item1 != null && tuple.Item1.Item2.HasValue && tuple.Item1.Item2.Value.idx == idx )
               .Select( tuple => this._methods[tuple.Item2] ) :
            Empty<CILMethodBase>.Enumerable;
      }

      private TRVA ResolveRVA( TRVA rva )
      {
         for ( var i = 0; i < this._sections.Length; ++i )
         {
            var sec = this._sections[i];
            if ( sec.Item3 <= rva && (Int64) rva < (Int64) ( sec.Item3 + sec.Item2 ) )
            {
               return sec.Item5 + ( rva - sec.Item3 );
            }
         }
         throw new ArgumentException( "Could not resolve RVA " + rva + "." );
      }

      internal CILType GetModuleInitializer()
      {
         return this._typeDef[0];
      }

      internal Boolean HasEntryPoint()
      {
         return this._epToken != 0;
      }

      internal CILMethod GetEntryPoint()
      {
         var idx = TokenUtils.DecodeToken( this._epToken ).Item2 - 1;
         return idx >= 0 && idx < this._methods.Length ? this._methods[idx] as CILMethod : null;
      }

      internal void SetThisModule( CILModule module )
      {
         Interlocked.CompareExchange( ref this._thisModule, (CILModuleImpl) module, null );
      }

      internal ListProxy<CILType> CreateLogicalStructure()
      {
         var md = this._md;

         var ctx = this._ctx;
         var typeDef = this._typeDef;
         var methodDef = this._methods;

         for ( var curIdx = 0; curIdx < md.typeDef.Length; ++curIdx )
         {
            var i = curIdx; // Don't use loop variable in lambda
            var tdRow = md.typeDef[i];
            var thisRowType = ctx.Cache.NewType( id => new CILTypeImpl(
               ctx,
               id,
               this.ReadCustomAttributes( Tables.TypeDef, i, CILElementKind.Type, id ),
               () => ResolveTypeCode( typeDef[i] ),
               new SettableValueForClasses<String>( tdRow.Item2 ),
               new SettableValueForClasses<String>( tdRow.Item3 ),
               () => this._thisModule,
               () =>
               {
                  var declTypeRows = GetReferencingRowsFromOrdered( md.nestedClass, Tables.TypeDef, i, row => row.Item1 );
                  return declTypeRows.Count == 0 ? null : typeDef[md.nestedClass[declTypeRows[0]].Item2.idx];
               },
               () =>
               {
                  var btRow = tdRow.Item4;
                  return btRow.HasValue ? (CILType) this.ResolveType( btRow.Value, this.GArgsOrNull( typeDef[i] ), null ) : null;
               },
               () => ctx.CollectionsFactory.NewListProxy(
                  GetReferencingRowsFromOrdered( md.interfaceImpl, Tables.TypeDef, i, row => row.Item1 )
                  .Select( rowIdx => ResolveType( md.interfaceImpl[rowIdx].Item2, this.GArgsOrNull( typeDef[i] ), null ) )
                  .Cast<CILType>()
                  .ToArray()
                  .OnlyBottomTypes()
                  .ToList() ),
               new SettableValueForEnums<TypeAttributes>( tdRow.Item1 ),
               null,
               null,
               () => ctx.CollectionsFactory.NewListProxy(
                  GetReferencingRowsFromOrdered( md.genericParam, Tables.TypeDef, i, row => row.Item3 )
                  .Select( rowIdx => ResolveGenericParam( rowIdx, typeDef[i], null ) )
                  .ToList<CILTypeBase>() ),
               () =>
               {
                  return GetReferencingRowsFromOrdered( md.genericParam, Tables.TypeDef, i, row => row.Item3 ).Any() ? typeDef[i] : null;
               }, new LazyWithLock<ListProxy<CILType>>( () => ctx.CollectionsFactory.NewListProxy(
                  md.nestedClass
                  .SkipWhile( row => row.Item1.idx <= i )
                  .Where( row => row.Item2.idx == i )
                  .Select( row => typeDef[row.Item1.idx] )
                  .ToList()
               ) ),
               () => ctx.CollectionsFactory.NewListProxy( this.GetListFromRangeReference( md.typeDef, i, this._fields, row => row.Item5.idx, f => f ) ),
               () => null,
               () => this._ctx.CollectionsFactory.NewListProxy( this.GetListFromRangeReference( md.typeDef, i, methodDef, row => row.Item6.idx, m => m as CILMethod ) ),
               () => this._ctx.CollectionsFactory.NewListProxy( this.GetListFromRangeReference( md.typeDef, i, methodDef, row => row.Item6.idx, m => m as CILConstructor ) ),
               () =>
               {
                  var tuple = this.GetListFromRangeReferenceViaMap( md.typeDef, i, md.propertyMap, md.property, row => row.Item1.idx, row => row.Item2.idx );
                  return this._ctx.CollectionsFactory.NewListProxy( tuple.Item1.Select( ( pRow, pIdx ) =>
                  {
                     pIdx += tuple.Item2;
                     var pMethods = GetReferencingRowsFromOrdered( md.methodSemantics, Tables.Property, pIdx, sRow => sRow.Item3 ).Select( sRowIdx => md.methodSemantics[sRowIdx] ).ToArray();
                     return this._ctx.Cache.NewProperty( pID => new CILPropertyImpl(
                        this._ctx,
                        pID,
                        this.ReadCustomAttributes( Tables.Property, pIdx, CILElementKind.Property, pID ),
                        new SettableValueForClasses<String>( pRow.Item2 ),
                        new SettableValueForEnums<PropertyAttributes>( pRow.Item1 ),
                        () => pMethods.Where( sRow => sRow.Item1 == MethodSemanticsAttributes.Setter ).Select( sRow => (CILMethod) methodDef[sRow.Item2.idx] ).FirstOrDefault(),
                        () => pMethods.Where( sRow => sRow.Item1 == MethodSemanticsAttributes.Getter ).Select( sRow => (CILMethod) methodDef[sRow.Item2.idx] ).FirstOrDefault(),
                        () => typeDef[i],
                        this.ReadConstantValue( Tables.Property, pIdx ),
                        new LazyWithLock<ListProxy<CILCustomModifier>>( () =>
                        {
                           var cmIdx = 1; // Skip first byte
                           pRow.Item3.DecompressUInt32( ref cmIdx ); // Skip param count
                           return this._ctx.CollectionsFactory.NewListProxy( this.ReadCustomMods( pRow.Item3, ref cmIdx, this.GArgsOrNull( typeDef[i] ), null ) );
                        } ),
                        true ) );
                  } ).ToList() );
               },
               () =>
               {
                  var tuple = this.GetListFromRangeReferenceViaMap( md.typeDef, i, md.eventMap, md.events, row => row.Item1.idx, row => row.Item2.idx );
                  return this._ctx.CollectionsFactory.NewListProxy( tuple.Item1.Select( ( eRow, eIdx ) =>
                  {
                     eIdx += tuple.Item2;
                     var eMethods = GetReferencingRowsFromOrdered( md.methodSemantics, Tables.Event, eIdx, sRow => sRow.Item3 ).Select( sRowIdx => md.methodSemantics[sRowIdx] ).ToArray();
                     return this._ctx.Cache.NewEvent( eID => new CILEventImpl(
                        this._ctx,
                        eID,
                        this.ReadCustomAttributes( Tables.Event, eIdx, CILElementKind.Event, eID ),
                        new SettableValueForClasses<string>( eRow.Item2 ),
                        new SettableValueForEnums<EventAttributes>( eRow.Item1 ),
                        () => this.ResolveType( eRow.Item3, typeDef[i].GenericArguments.ToArray(), null ),
                        () => eMethods.Where( sRow => sRow.Item1 == MethodSemanticsAttributes.AddOn ).Select( sRow => (CILMethod) methodDef[sRow.Item2.idx] ).FirstOrDefault(),
                        () => eMethods.Where( sRow => sRow.Item1 == MethodSemanticsAttributes.RemoveOn ).Select( sRow => (CILMethod) methodDef[sRow.Item2.idx] ).FirstOrDefault(),
                        () => eMethods.Where( sRow => sRow.Item1 == MethodSemanticsAttributes.Fire ).Select( sRow => (CILMethod) methodDef[sRow.Item2.idx] ).FirstOrDefault(),
                        () => this._ctx.CollectionsFactory.NewListProxy( eMethods.Where( sRow => sRow.Item1 == MethodSemanticsAttributes.Other ).Select( sRow => (CILMethod) methodDef[sRow.Item2.idx] ).ToList() ),
                        () => typeDef[i],
                        true
                        ) );
                  } ).ToList() );
               },
               new SettableLazy<ClassLayout?>( () =>
               {
                  var layoutRows = GetReferencingRowsFromOrdered( md.classLayout, Tables.TypeDef, i, row => row.Item3 );
                  return layoutRows.Count == 0 ? (ClassLayout?) null : new ClassLayout() { pack = md.classLayout[layoutRows[0]].Item1, size = (Int32) md.classLayout[layoutRows[0]].Item2 };
               } ),
               this.GetSecurityInfo( Tables.TypeDef, i ),
               true
               )
            );
            this._typeDef[i] = thisRowType;
            this._thisModule.TypeNameCache.TryAdd( this.ConstructTypeString( i ), thisRowType );
         }

         var curTDIdxTuple = md.typeDef.Select( ( tdr, idx ) => Tuple.Create( tdr, idx ) ).LastOrDefault( tuple => tuple.Item1.Item5.idx == 0 );
         var curTDIdx = curTDIdxTuple == null ? 0 : curTDIdxTuple.Item2;
         for ( var curIdx = 0; curIdx < md.field.Length; ++curIdx )
         {
            var fRow = md.field[curIdx];
            var sig = this.ReadFieldSignature( fRow.Item3, curTDIdx );
            var rvaList = GetReferencingRowsFromOrdered( md.fieldRVA, Tables.Field, curIdx, row => row.Item2 );
            Byte[] initialValue;
            if ( rvaList.Count > 0 )
            {
               initialValue = this._fieldRVAContents[rvaList[0]];
            }
            else
            {
               initialValue = null;
            }

            // Don't use loop-wide variables in lambda
            var i = curIdx;
            var curTDIdxLambda = curTDIdx;

            this._fields[curIdx] = ctx.Cache.NewField( fID => new CILFieldImpl(
               ctx,
               fID,
               this.ReadCustomAttributes( Tables.Field, i, CILElementKind.Field, fID ),
               new SettableValueForEnums<FieldAttributes>( fRow.Item1 ),
               new SettableValueForClasses<String>( fRow.Item2 ),
               () => typeDef[curTDIdxLambda],
               () => sig.Value.Item2,
               this.ReadConstantValue( Tables.Field, i ),
               new SettableValueForClasses<Byte[]>( initialValue ),
               new LazyWithLock<ListProxy<CILCustomModifier>>( () => sig.Value.Item1 ),
               new SettableLazy<Int32>( () =>
               {
                  var oList = GetReferencingRowsFromOrdered( md.fieldLayout, Tables.Field, i, row => row.Item2 );
                  return oList.Count > 0 ? (Int32) md.fieldLayout[oList[0]].Item1 : CILFieldImpl.NO_OFFSET;
               } ),
               new SettableLazy<MarshalingInfo>( () =>
               {
                  var fMInfo = GetReferencingRowsFromOrdered( this._md.fieldMarshal, Tables.Field, i, row => row.Item1 );
                  return fMInfo.Count > 0 ? this.ReadMarshalInfo( fMInfo[0] ) : null;
               } ),
               true ) );
            while ( curTDIdx + 1 < md.typeDef.Length && md.typeDef[curTDIdx + 1].Item5.idx == curIdx + 1 )
            {
               ++curTDIdx;
            }
         }
         // Methods        
         curTDIdxTuple = md.typeDef.Select( ( tdr, idx ) => Tuple.Create( tdr, idx ) ).LastOrDefault( tuple => tuple.Item1.Item6.idx == 0 );
         curTDIdx = curTDIdxTuple == null ? 0 : curTDIdxTuple.Item2;
         for ( var curIdx = 0; curIdx < md.methodDef.Length; ++curIdx )
         {
            var mRow = md.methodDef[curIdx];

            // Don't use loop-wide variables in lambda
            var i = curIdx;
            var curTDIdxLambda = curTDIdx;
            var sig = this.ReadMethodSignature( mRow.Item5, curTDIdx, curIdx );
            CILMethodBase curMethod;
            if ( mRow.Item4 == CILConstructorImpl.INSTANCE_CTOR_NAME || mRow.Item4 == CILConstructorImpl.STATIC_CTOR_NAME )
            {
               // Constructor
               curMethod = ctx.Cache.NewConstructor( id => new CILConstructorImpl(
                  ctx,
                  id,
                  this.ReadCustomAttributes( Tables.MethodDef, i, CILElementKind.Method, id ),
                  new SettableValueForEnums<CallingConventions>( ReadCallingConventionFromSignature( mRow.Item5 ) ),
                  new SettableValueForEnums<MethodAttributes>( mRow.Item3 ),
                  () => typeDef[curTDIdxLambda],
                  () => this.CreateMethodParameters( sig.Value, i, mRow.Item6.idx ),
                  () => this.NewMethodIL( curTDIdxLambda, i ),
                  new SettableLazy<MethodImplAttributes>( () => mRow.Item2 ),
                  this.GetSecurityInfo( Tables.MethodDef, i ),
                  true ) );
            }
            else
            {
               // Method
               var pInvokeRowIdxs = GetReferencingRowsFromOrdered( md.implMap, Tables.MethodDef, i, t => t.Item2 );
               var pInvokeRow = pInvokeRowIdxs.Count > 0 ? md.implMap[pInvokeRowIdxs[0]] : null;
               curMethod = ctx.Cache.NewMethod( id => new CILMethodImpl(
                  ctx,
                  id,
                  this.ReadCustomAttributes( Tables.MethodDef, i, CILElementKind.Method, id ),
                  new SettableValueForEnums<CallingConventions>( ReadCallingConventionFromSignature( mRow.Item5 ) ),
                  new SettableValueForEnums<MethodAttributes>( mRow.Item3 ),
                  () => typeDef[curTDIdxLambda],
                  () => this.CreateMethodParameters( sig.Value, i, mRow.Item6.idx ),
                  () => this.NewMethodIL( curTDIdxLambda, i ),
                  new SettableLazy<MethodImplAttributes>( () => mRow.Item2 ),
                  this.GetSecurityInfo( Tables.MethodDef, i ),
                  new SettableValueForClasses<string>( mRow.Item4 ),
                  () => this.CreateMethodReturnParameter( sig.Value, i, mRow.Item6.idx ),
                  () => ctx.CollectionsFactory.NewListProxy(
                  GetReferencingRowsFromOrdered( md.genericParam, Tables.MethodDef, i, row => row.Item3 )
                     .Select( rowIdx => ResolveGenericParam( rowIdx, typeDef[curTDIdxLambda], (CILMethod) methodDef[i] ) )
                     .ToList<CILTypeBase>() ),
                  () =>
                  {
                     return GetReferencingRowsFromOrdered( md.genericParam, Tables.MethodDef, i, row => row.Item3 ).Any() ? (CILMethod) methodDef[i] : null;
                  },
                  () => this._ctx.CollectionsFactory.NewListProxy(
                     GetReferencingRowsFromOrdered( md.methodImpl, Tables.TypeDef, curTDIdxLambda, iRow => iRow.Item1 )
                        .Select( iRowIdx => md.methodImpl[iRowIdx] )
                        .Where( iRow => iRow.Item2.table == Tables.MethodDef && iRow.Item2.idx == i )
                        .Select( iRow =>
                        {
                           switch ( iRow.Item3.table )
                           {
                              case Tables.MethodDef:
                                 return (CILMethod) methodDef[iRow.Item3.idx];
                              case Tables.MemberRef:
                                 // TODO what if explicitly implementing generic method??
                                 return (CILMethod) this.ResolveMemberRef( iRow.Item3.idx, this.GArgsOrNull( typeDef[curTDIdxLambda] ), this.GArgsOrNull( methodDef[i] ), ILResolveKind.MethodBase );
                              default:
                                 throw new BadImageFormatException( "Found unsupported table index in method impl table: " + iRow.Item3 + "." );
                           }
                        } ).ToList() ),
                   pInvokeRow == null ? null : new SettableValueForEnums<PInvokeAttributes>( pInvokeRow.Item1 ),
                   pInvokeRow == null ? null : new SettableValueForClasses<String>( pInvokeRow.Item3 ),
                   pInvokeRow == null ? null : new SettableValueForClasses<String>( md.moduleRef[pInvokeRow.Item4.idx] ),
                  true ) );
            }
            methodDef[curIdx] = curMethod;
            while ( curTDIdx + 1 < md.typeDef.Length && md.typeDef[curTDIdx + 1].Item6.idx == curIdx + 1 )
            {
               ++curTDIdx;
            }
         }

         // Generic params
         for ( var curIdx = 0; curIdx < md.genericParam.Length; )
         {
            var owner = md.genericParam[curIdx].Item3;
            var cilOwner = owner.table == Tables.TypeDef ? (CILElementWithGenericArguments<Object>) typeDef[owner.idx] : (CILElementWithGenericArguments<Object>) methodDef[owner.idx];
            var max = curIdx;
            while ( max < md.genericParam.Length && md.genericParam[max].Item3.Equals( owner ) )
            {
               ++max;
            }
            var declType = cilOwner is CILType ? (CILType) cilOwner : ( (CILMethod) cilOwner ).DeclaringType;
            var declMethod = cilOwner is CILType ? null : (CILMethod) cilOwner;
            var gParamz = new CILTypeParameter[max - curIdx];
            for ( var inner = curIdx; inner < max; ++inner )
            {
               // Don't use loop variable in lambda
               var innerLambda = inner;
               var gRow = md.genericParam[inner];
               gParamz[inner - curIdx] = ctx.Cache.NewTypeParameter( id => new CILTypeParameterImpl(
                  ctx,
                  id,
                  this.ReadCustomAttributes( Tables.GenericParameter, inner, CILElementKind.Type, id ),
                  gRow.Item2,
                  declType,
                  declMethod,
                  gRow.Item4,
                  gRow.Item1,
                  () => ctx.CollectionsFactory.NewListProxy(
                     GetReferencingRowsFromOrdered( md.genericParamConstraint, Tables.GenericParameter, innerLambda, cRow => cRow.Item1 )
                        .Select( cIdx => this.ResolveType( this._md.genericParamConstraint[cIdx].Item2, this.GArgsOrNull( declType ), this.GArgsOrNull( declMethod ) ) ).ToList() )
                  ) );
            }
            curIdx += gParamz.Length;
            this._gParams.Add( cilOwner, gParamz );
         }

         var returnList = new List<CILType>( md.typeDef.Length - md.nestedClass.Length );
         var curNIdx = 0;
         // Skip module initializer
         for ( var curIdx = 1; curIdx < md.typeDef.Length; ++curIdx )
         {
            var addToList = curNIdx >= md.nestedClass.Length;
            if ( !addToList )
            {
               var curNRef = md.nestedClass[curNIdx].Item1.idx;
               // NestedType table is ordered by first index
               if ( curNRef < curIdx && curNIdx < md.nestedClass.Length - 1 )
               {
                  curNRef = md.nestedClass[++curNIdx].Item1.idx;
               }
               addToList = curNRef != curIdx;
            }
            if ( addToList )
            {
               returnList.Add( this._typeDef[curIdx] );
            }
         }
         return this._ctx.CollectionsFactory.NewListProxy( returnList );
      }

      private String ConstructTypeString( Int32 tDefIdx )
      {
         var dtRows = GetReferencingRowsFromOrdered( this._md.nestedClass, Tables.TypeDef, tDefIdx, row => row.Item1 );
         var tRow = this._md.typeDef[tDefIdx];
         if ( dtRows.Count > 0 )
         {
            return this.ConstructTypeString( this._md.nestedClass[dtRows[0]].Item2.idx ) + CILTypeImpl.NESTED_TYPENAME_SEPARATOR + tRow.Item2;
         }
         else
         {
            return Utils.NamespaceAndTypeName( tRow.Item3, tRow.Item2 );
         }
      }

      private CILType ResolveExportedTypeAsCILType( TableIndex tIdx, String ns, String name )
      {
         switch ( tIdx.table )
         {
            case Tables.AssemblyRef:
               return this.LoadTypeFromAssemblyRef( tIdx.idx + 1, null, ns, name, false );
            case Tables.ExportedType:
               var row = this._md.exportedType[tIdx.idx];
               return ResolveExportedTypeAsCILType( row.Item5, row.Item4, row.Item3 ).DeclaredNestedTypes.First( dt => String.Equals( dt.Name, name ) );
            case Tables.File:
               throw new NotImplementedException();
            default:
               throw new BadImageFormatException( "Exported type row contained invalid table index: " + tIdx + "." );
         }
      }

      internal TypeForwardingInfo ResolveExportedType( TableIndex tIdx, TypeAttributes attrs, String ns, String name )
      {
         switch ( tIdx.table )
         {
            case Tables.AssemblyRef:
               var aRow = this._md.assemblyRef[tIdx.idx];
               return new TypeForwardingInfo( attrs, name, ns, new CILAssemblyName( aRow.Item7, aRow.Item1, aRow.Item2, aRow.Item3, aRow.Item4, AssemblyHashAlgorithm.None, aRow.Item5, aRow.Item6, aRow.Item8 ) );
            case Tables.ExportedType:
               var row = this._md.exportedType[tIdx.idx];
               var other = this.ResolveExportedType( row.Item5, row.Item1, row.Item4, row.Item3 );
               return new TypeForwardingInfo( attrs, name, ns, other.Name, other.Namespace, other.AssemblyName );
            case Tables.File:
               throw new NotImplementedException();
            default:
               throw new BadImageFormatException( "Exported type row contained invalid table index: " + tIdx + "." );
         }
      }

      private Lazy<DictionaryWithRoles<SecurityAction, ListProxy<SecurityInformation>, ListProxyQuery<SecurityInformation>, ListQuery<SecurityInformation>>> GetSecurityInfo( Tables table, Int32 idx )
      {
         return new Lazy<DictionaryWithRoles<SecurityAction, ListProxy<SecurityInformation>, ListProxyQuery<SecurityInformation>, ListQuery<SecurityInformation>>>( () =>
         {
            var secList = new List<SecurityInformation>();
            foreach ( var i in GetReferencingRowsFromOrdered( this._md.declSecurity, table, idx, row => row.Item2 ) )
            {
               var row = this._md.declSecurity[i];
               var blob = row.Item3;
               var bIdx = 0;
               if ( blob.Length > 0 )
               {

                  if ( blob[0] == MetaDataConstants.DECL_SECURITY_HEADER )
                  {
                     // New (.NET 2.0+) security spec
                     ++bIdx;
                     // Amount of security attributes
                     var attrCount = blob.DecompressUInt32( ref bIdx );
                     for ( var j = 0; j < attrCount; ++j )
                     {
                        var secType = this.ResolveTypeString( blob.ReadLenPrefixedUTF8String( ref bIdx ) );
                        // For some reason, there is a amount of remaining bytes here
                        blob.DecompressUInt32( ref bIdx );
                        // Now, amount of named args
                        var argCount = blob.DecompressUInt32( ref bIdx );
                        var secArgs = new List<CILCustomAttributeNamedArgument>( argCount );
                        // Read named args
                        for ( var k = 0; k < argCount; ++k )
                        {
                           secArgs.Add( this.ReadCANamedArgument( blob, ref bIdx, secType ) );
                        }
                        secList.Add( new SecurityInformation( (SecurityAction) row.Item1, secType, secArgs ) );
                     }
                  }
                  else
                  {
                     // Old (.NET 1.x) security spec
                     // Create a single SecurityInformation with PermissionSetAttribute type and XML property argument containing the XML of the blob
                     var corLibAN = new CILAssemblyName( this._eArgs.CorLibName, this._eArgs.CorLibMajor, this._eArgs.CorLibMinor, this._eArgs.CorLibBuild, this._eArgs.CorLibRevision );
                     var secType = this.ResolveTypeString( Consts.PERMISSION_SET + ", " + corLibAN );
                     var prop = secType.DeclaredProperties.FirstOrDefault( p => String.Equals( Consts.PERMISSION_SET_XML_PROP, p.Name ) );
                     var secArgs = new List<CILCustomAttributeNamedArgument>( prop == null ? 0 : 1 );
                     if ( prop != null )
                     {
                        secArgs.Add( CILCustomAttributeFactory.NewNamedArgument( prop, CILCustomAttributeFactory.NewTypedArgument( this._coreTypes[SignatureElementTypes.String].Value, MetaDataConstants.USER_STRING_ENCODING.GetString( blob, bIdx, blob.Length ) ) ) );
                     }
                     secList.Add( new SecurityInformation( (SecurityAction) row.Item1, secType, secArgs ) );
                  }
               }
            }
            return this._ctx.CollectionsFactory.NewDictionary<SecurityAction, ListProxy<SecurityInformation>, ListProxyQuery<SecurityInformation>, ListQuery<SecurityInformation>>( secList
               .GroupBy( s => s.SecurityAction )
               .ToDictionary( s => s.Key, s => this._ctx.CollectionsFactory.NewListProxy( s.ToList() ) )
               );
         }, LazyThreadSafetyMode.ExecutionAndPublication );
      }

      private MarshalingInfo ReadMarshalInfo( Int32 idx )
      {
         var sig = this._md.fieldMarshal[idx].Item2;
         var sIdx = 0;
         var ut = (UnmanagedType) sig[sIdx++];
         MarshalingInfo result;
         if ( ut.IsNativeInstric() )
         {
            result = MarshalingInfo.MarshalAs( ut );
         }
         else
         {
            Int32 constSize, paramIdx;
            UnmanagedType arrElementType;
            switch ( ut )
            {
               case UnmanagedType.ByValTStr:
                  result = MarshalingInfo.MarshalAsByValTStr( sig.DecompressUInt32( ref sIdx ) );
                  break;
               case UnmanagedType.IUnknown:
                  result = MarshalingInfo.MarshalAsIUnknown( sIdx < sig.Length ? sig.DecompressUInt32( ref sIdx ) : MarshalingInfo.NO_INDEX );
                  break;
               case UnmanagedType.IDispatch:
                  result = MarshalingInfo.MarshalAsIDispatch( sIdx < sig.Length ? sig.DecompressUInt32( ref sIdx ) : MarshalingInfo.NO_INDEX );
                  break;
               case UnmanagedType.SafeArray:
                  if ( sIdx < sig.Length )
                  {
                     var ve = (CILAssemblyManipulator.API.VarEnum) sig.DecompressUInt32( ref sIdx );
                     if ( CILAssemblyManipulator.API.VarEnum.VT_USERDEFINED == ve )
                     {
                        if ( sIdx < sig.Length )
                        {
                           result = MarshalingInfo.MarshalAsSafeArray( this.ResolveTypeString( sig.ReadLenPrefixedUTF8String( ref sIdx ) ) );
                        }
                        else
                        {
                           // Fallback in erroneus blob - just plain safe array
                           result = MarshalingInfo.MarshalAsSafeArray();
                        }
                     }
                     else
                     {
                        result = MarshalingInfo.MarshalAsSafeArray( ve );
                     }
                  }
                  else
                  {
                     result = MarshalingInfo.MarshalAsSafeArray();
                  }
                  break;
               case UnmanagedType.ByValArray:
                  constSize = sig.DecompressUInt32( ref sIdx );
                  result = MarshalingInfo.MarshalAsByValArray(
                     constSize,
                     sIdx < sig.Length ?
                        (UnmanagedType) sig.DecompressUInt32( ref sIdx ) :
                        MarshalingInfo.NATIVE_TYPE_MAX );
                  break;
               case UnmanagedType.LPArray:
                  arrElementType = (UnmanagedType) sig[sIdx++];
                  paramIdx = MarshalingInfo.NO_INDEX;
                  constSize = MarshalingInfo.NO_INDEX;
                  if ( sIdx < sig.Length )
                  {
                     paramIdx = sig.DecompressUInt32( ref sIdx );
                     if ( sIdx < sig.Length )
                     {
                        constSize = sig.DecompressUInt32( ref sIdx );
                        if ( sIdx < sig.Length && sig.DecompressUInt32( ref sIdx ) == 0 )
                        {
                           paramIdx = MarshalingInfo.NO_INDEX; // No size parameter index was specified
                        }
                     }
                  }
                  result = MarshalingInfo.MarshalAsLPArray( paramIdx, constSize, arrElementType );
                  break;
               case UnmanagedType.CustomMarshaler:
                  // For some reason, there are two compressed ints at this point
                  sig.DecompressUInt32( ref sIdx );
                  sig.DecompressUInt32( ref sIdx );

                  var mTypeStr = sig.ReadLenPrefixedUTF8String( ref sIdx );
                  var mCookie = sig.ReadLenPrefixedUTF8String( ref sIdx );
                  result = MarshalingInfo.MarshalAsCustom( mTypeStr, str =>
                  {
                     CILType mType = null;
                     if ( str != null )
                     {
                        try
                        {
                           mType = this.ResolveTypeString( str );
                        }
                        catch
                        {
                           // Ignore
                        }
                     }
                     return mType;
                  }, mCookie );
                  break;
               default:
                  result = null;
                  break;
            }
         }
         return result;
      }

      private MethodIL NewMethodIL( Int32 typeIdx, Int32 methodIdx )
      {
         var ilInfo = this._methodDefRVAContents[methodIdx];
         var thisMethod = this._methods[methodIdx];
         var row = this._md.methodDef[methodIdx];
         MethodIL result;
         if ( row.Item1 != 0 && row.Item2.IsIL() )
         {
            CILTypeBase[] typeGArgs;
            this._gParams.TryGetValue( this._typeDef[typeIdx], out typeGArgs );
            CILTypeBase[] methodGArgs = null;
            if ( thisMethod is CILMethod )
            {
               this._gParams.TryGetValue( (CILMethod) thisMethod, out methodGArgs );
            }
            result = new MethodILImpl(
               this._thisModule,
               ilInfo.Item1,
               ilInfo.Item3,
               ilInfo.Item2.HasValue ? this.ReadLocalVarSignature( this._md.standaloneSig[ilInfo.Item2.Value.idx], typeGArgs, methodGArgs ) : EMPTY_LOCAL_VARS,
               ilInfo.Item4.Select( excInfo => Tuple.Create( excInfo.Item1, (Int32) excInfo.Item2, (Int32) excInfo.Item3, (Int32) excInfo.Item4, (Int32) excInfo.Item5, excInfo.Item6 == 0 || excInfo.Item1 != ExceptionBlockType.Exception ? null : (CILType) this.ResolveType( new TableIndex( (Int32) excInfo.Item6 ), typeGArgs, methodGArgs ), (Int32) excInfo.Item7 ) ).ToArray(),
               ( token, rKind ) =>
               {
                  switch ( rKind )
                  {
                     case ILResolveKind.String:
                        return this._md.userStrings.GetString( token & TokenUtils.INDEX_MASK );
                     case ILResolveKind.Type:
                        return this.ResolveType( new TableIndex( token ), typeGArgs, methodGArgs );
                     case ILResolveKind.Signature:
                        return this.ResolveMethodSigFromTableIndex( new TableIndex( token ).idx, typeGArgs, methodGArgs );
                     default:
                        switch ( TokenUtils.DecodeTokenTable( token ) )
                        {
                           case Tables.MemberRef:
                              return this.ResolveMemberRef( new TableIndex( token ).idx, typeGArgs, methodGArgs, rKind );
                           case Tables.Field:
                              return this._fields[new TableIndex( token ).idx];
                           case Tables.MethodDef:
                              return this._methods[new TableIndex( token ).idx];
                           case Tables.MethodSpec:
                              return this.ResolveMethodSpec( new TableIndex( token ).idx, typeGArgs, methodGArgs );
                           default:
                              throw new BadImageFormatException( "Unsupported token in IL: " + token + "." );
                        }
                  }
               } );
         }
         else
         {
            result = new MethodILImpl( this._thisModule );
         }
         return result;
      }

      private Tuple<CILMethodSignature, Tuple<CILCustomModifier[], CILTypeBase>[]> ResolveMethodSigFromTableIndex( Int32 idx, CILTypeBase[] typeGArgs, CILTypeBase[] methodGArgs )
      {
         var sig = this._md.standaloneSig[idx];
         var sigIdx = 0;
         var mResult = this.DoReadMethodSignature( sig, ref sigIdx, typeGArgs, methodGArgs );
         return Tuple.Create<CILMethodSignature, Tuple<CILCustomModifier[], CILTypeBase>[]>(
            new CILMethodSignatureImpl( this._ctx, this._thisModule, (UnmanagedCallingConventions) sig[0], mResult.Item1.Item1, mResult.Item1.Item2, mResult.Item2, null ),
            mResult.Item3 == null ? null : mResult.Item3.Select( i => Tuple.Create( i.Item1.CQ.ToArray(), i.Item2 ) ).ToArray()
            );
      }

      private CILMethod ResolveMethodSpec( Int32 tIdx, CILTypeBase[] typeGArgs, CILTypeBase[] methodGArgs )
      {
         var mRow = this._md.methodSpec[tIdx];
         var gDef = mRow.Item1.table == Tables.MethodDef ? (CILMethod) this._methods[mRow.Item1.idx] : (CILMethod) this.ResolveMemberRef( mRow.Item1.idx, typeGArgs, methodGArgs, ILResolveKind.MethodBase );
         var sig = mRow.Item2;
         var idx = 1; // Skip GenericInst
         var amount = sig.DecompressUInt32( ref idx );
         if ( amount != gDef.GenericArguments.Count )
         {
            throw new BadImageFormatException( "Method spec contained " + amount + " generic arguments, while actual method contained " + gDef.GenericArguments.Count + " (spec index: " + tIdx + ")." );
         }
         var gArgs = new CILTypeBase[amount];
         for ( var i = 0; i < amount; ++i )
         {
            gArgs[i] = this.ReadTypeFromSig( sig, ref idx, typeGArgs, methodGArgs );
         }
         return gDef.MakeGenericMethod( gArgs );
      }

      private CILTypeBase[] GArgsOrNull( Object obj )
      {
         CILTypeBase[] result = null;
         if ( obj is CILElementWithGenericArguments<Object> )
         {
            this._gParams.TryGetValue( obj as CILElementWithGenericArguments<Object>, out result );
         }
         return result;
      }

      internal ListProxy<CILCustomAttribute> ReadAssemblyCustomAttributes( Int32 assemblyID )
      {
         return this.ReadCustomAttributes( Tables.Assembly, 0, CILElementKind.Assembly, assemblyID ).Value;
      }

      internal ListProxy<CILCustomAttribute> ReadModuleCustomAttributes( Int32 moduleID )
      {
         return this.ReadCustomAttributes( Tables.Module, 0, CILElementKind.Module, moduleID ).Value;
      }

      private IList<V> GetListFromRangeReference<T, U, V>( T[] thisArray, Int32 thisArrayIdx, U[] targetArray, Func<T, Int32> thisArrayIndexExtractor, Func<U, V> caster )
         where U : class
         where V : class
      {
         var max = CalculateMaxForRangeRef( thisArray, thisArrayIdx, targetArray, thisArrayIndexExtractor );
         var cur = thisArrayIndexExtractor( thisArray[thisArrayIdx] );
         var list = new List<V>( max - cur );
         while ( cur < max )
         {
            var curItem = caster( targetArray[cur] );
            if ( curItem != null )
            {
               list.Add( curItem );
            }
            ++cur;
         }
         return list;
      }

      private static Int32 CalculateMaxForRangeRef<T, U>( T[] thisArray, Int32 thisArrayIdx, U[] targetArray, Func<T, Int32> thisArrayIndexExtractor )
      {
         return thisArrayIdx + 1 == thisArray.Length ? targetArray.Length : thisArrayIndexExtractor( thisArray[thisArrayIdx + 1] );
      }

      private Tuple<IList<V>, Int32> GetListFromRangeReferenceViaMap<T, U, V>( T[] thisArray, Int32 thisArrayIdx, U[] mapArray, V[] targetArray, Func<U, Int32> mapThisArrayIndexExtractor, Func<U, Int32> mapTargetArrayIndexExtractor )
         where V : class
      {
         Tuple<IList<V>, Int32> result = null;
         for ( var j = 0; j < mapArray.Length; ++j )
         {
            var pRow = mapArray[j];
            if ( mapThisArrayIndexExtractor( pRow ) == thisArrayIdx )
            {
               result = Tuple.Create( this.GetListFromRangeReference( mapArray, j, targetArray, mapTargetArrayIndexExtractor, p => p ), mapTargetArrayIndexExtractor( pRow ) );
               break;
            }
         }
         return result ?? Tuple.Create<IList<V>, Int32>( new List<V>(), -1 );
      }

      private static IList<Int32> GetReferencingRowsFromOrdered<T>( T[] array, Tables targetTable, Int32 targetIndex, Func<T, TableIndex> fullIndexExtractor )
      {
         // Use binary search to find first one
         // Use the deferred equality detection version in order to find the smallest index matching the target index
         var max = array.Length - 1;
         var min = 0;
         while ( min < max )
         {
            var mid = ( min + max ) >> 1; // We can safely add before shifting, since table indices are supposed to be max 3 bytes long anyway.
            if ( fullIndexExtractor( array[mid] ).idx < targetIndex )
            {
               min = mid + 1;
            }
            else
            {
               max = mid;
            }
         }
         IList<Int32> result;
         if ( ( min == max ) && fullIndexExtractor( array[min] ).idx == targetIndex )
         {
            result = new List<Int32>();
            do
            {
               if ( fullIndexExtractor( array[min] ).table == targetTable )
               {
                  result.Add( min );
               }
               ++min;
            } while ( min < array.Length && fullIndexExtractor( array[min] ).idx == targetIndex );
         }
         else
         {
            result = EMPTY_INDICES;
         }
         return result;
      }

      private SettableLazy<Object> ReadConstantValue( Tables thisTable, Int32 thisIdx )
      {
         return new SettableLazy<Object>( () =>
         {
            if ( thisIdx >= 0 )
            {
               var list = GetReferencingRowsFromOrdered( this._md.constant, thisTable, thisIdx, row => row.Item2 );
               if ( list.Any() )
               {
                  var row = this._md.constant[list[0]];
                  return this.ReadConstantValue( row.Item3, (SignatureElementTypes) row.Item1 );
               }
               else
               {
                  return null;
               }
            }
            else
            {
               return null;
            }
         } );
      }

      private CILCustomAttribute ReadCustomAttribute( Int32 caIdx, CILCustomAttributeContainer container )
      {
         var row = this._md.customAttribute[caIdx];
         CILConstructor ctor;
         switch ( row.Item2.table )
         {
            case Tables.MethodDef:
               ctor = (CILConstructor) this._methods[row.Item2.idx];
               break;
            case Tables.MemberRef:
               ctor = (CILConstructor) this.ResolveMemberRef( this._md.customAttribute[caIdx].Item2.idx, null, null, ILResolveKind.MethodBase );
               break;
            default:
               throw new BadImageFormatException( "Unknown custom attribute constructor ref: " + row.Item2 + "." );
         }

         var idx = 2; // Skip prolog
         return CILCustomAttributeFactory.NewAttribute(
            container,
            ctor,
            row.Item3.Length == 0 ? null : this.ReadCATypedArguments( row.Item3, ref idx, ctor ),
            row.Item3.Length == 0 ? null : this.ReadCANamedArguments( row.Item3, ref idx, ctor )
            );
      }

      private CILCustomAttributeTypedArgument[] ReadCATypedArguments( Byte[] caBLOB, ref Int32 idx, CILConstructor ctor )
      {
         var args = new CILCustomAttributeTypedArgument[ctor.Parameters.Count];
         for ( var i = 0; i < args.Length; ++i )
         {
            var tuple = this.ReadCAFixedArgument( caBLOB, ref idx, (CILType) ctor.Parameters[i].ParameterType );
            args[i] = CILCustomAttributeFactory.NewTypedArgument( tuple.Item2, tuple.Item1 );
         }
         return args;
      }

      private Tuple<Object, CILType> ReadCAFixedArgument( Byte[] caBLOB, ref Int32 idx, CILType type )
      {
         Object result;
         if ( type.IsArray() )
         {
            var amount = caBLOB.ReadInt32LEFromBytes( ref idx );
            if ( ( (UInt32) amount ) == 0xFFFFFFFF )
            {
               result = null;
            }
            else
            {
               var array = new Object[amount];
               var elemType = (CILType) type.ElementType;
               for ( var i = 0; i < amount; ++i )
               {
                  array[i] = this.ReadCAFixedArgument( caBLOB, ref idx, elemType ).Item1;
               }
               result = array;
            }
         }
         else
         {
            switch ( type.TypeCode )
            {
               case CILTypeCode.Boolean:
                  result = caBLOB.ReadByteFromBytes( ref idx ) == 1;
                  break;
               case CILTypeCode.Char:
                  result = Convert.ToChar( caBLOB.ReadUInt16LEFromBytes( ref idx ) );
                  break;
               case CILTypeCode.SByte:
                  result = caBLOB.ReadSByteFromBytes( ref idx );
                  break;
               case CILTypeCode.Byte:
                  result = caBLOB.ReadByteFromBytes( ref idx );
                  break;
               case CILTypeCode.Int16:
                  result = caBLOB.ReadInt16LEFromBytes( ref idx );
                  break;
               case CILTypeCode.UInt16:
                  result = caBLOB.ReadUInt32LEFromBytes( ref idx );
                  break;
               case CILTypeCode.Int32:
                  result = caBLOB.ReadInt32LEFromBytes( ref idx );
                  break;
               case CILTypeCode.UInt32:
                  result = caBLOB.ReadUInt32LEFromBytes( ref idx );
                  break;
               case CILTypeCode.Int64:
                  result = caBLOB.ReadInt64LEFromBytes( ref idx );
                  break;
               case CILTypeCode.UInt64:
                  result = caBLOB.ReadUInt64LEFromBytes( ref idx );
                  break;
               case CILTypeCode.Single:
                  result = caBLOB.ReadSingleLEFromBytes( ref idx );
                  break;
               case CILTypeCode.Double:
                  result = caBLOB.ReadDoubleLEFromBytes( ref idx );
                  break;
               case CILTypeCode.String:
                  result = caBLOB.ReadLenPrefixedUTF8String( ref idx );
                  break;
               case CILTypeCode.SystemObject:
                  type = this.ReadCAFieldOrPropType( caBLOB, ref idx );
                  result = this.ReadCAFixedArgument( caBLOB, ref idx, type ).Item1;
                  break;
               case CILTypeCode.Type:
                  result = this.ResolveTypeString( caBLOB.ReadLenPrefixedUTF8String( ref idx ) );
                  break;
               default:
                  throw new BadImageFormatException( "Invalid type for custom attribute argument: " + type );
            }
         }
         return Tuple.Create( result, type );
      }

      private CILType ReadCAFieldOrPropType( Byte[] array, ref Int32 idx )
      {
         var sigType = (SignatureElementTypes) array.ReadByteFromBytes( ref idx );
         switch ( sigType )
         {
            case SignatureElementTypes.CA_Enum:
               return this.ResolveTypeString( array.ReadLenPrefixedUTF8String( ref idx ) );
            case SignatureElementTypes.SzArray:
               return this.ReadCAFieldOrPropType( array, ref idx );
            case SignatureElementTypes.CA_Boxed:
               return this._coreTypes[SignatureElementTypes.Object].Value;
            case SignatureElementTypes.Boolean:
            case SignatureElementTypes.Char:
            case SignatureElementTypes.I1:
            case SignatureElementTypes.U1:
            case SignatureElementTypes.I2:
            case SignatureElementTypes.U2:
            case SignatureElementTypes.I4:
            case SignatureElementTypes.U4:
            case SignatureElementTypes.I8:
            case SignatureElementTypes.U8:
            case SignatureElementTypes.R4:
            case SignatureElementTypes.R8:
            case SignatureElementTypes.String:
            case SignatureElementTypes.Type:
               return this._coreTypes[sigType].Value;
            default:
               throw new BadImageFormatException( "Invalid type for custom attribute argument type: " + sigType );
         }
      }

      private CILCustomAttributeNamedArgument[] ReadCANamedArguments( Byte[] caBLOB, ref Int32 idx, CILConstructor ctor )
      {
         var amount = caBLOB.ReadUInt16LEFromBytes( ref idx );
         var args = new CILCustomAttributeNamedArgument[amount];
         for ( var i = 0; i < amount; ++i )
         {
            args[i] = this.ReadCANamedArgument( caBLOB, ref idx, ctor.DeclaringType );
         }
         return args;
      }

      private CILCustomAttributeNamedArgument ReadCANamedArgument( Byte[] caBLOB, ref Int32 idx, CILType declType )
      {
         Boolean isField;
         switch ( (SignatureElementTypes) caBLOB.ReadByteFromBytes( ref idx ) )
         {
            case SignatureElementTypes.CA_Field:
               isField = true;
               break;
            case SignatureElementTypes.CA_Property:
               isField = false;
               break;
            default:
               throw new BadImageFormatException( "Unknown custom attribute named argument kind: " + (SignatureElementTypes) caBLOB[idx - 1] + "." );
         }
         var type = this.ReadCAFieldOrPropType( caBLOB, ref idx );
         var name = caBLOB.ReadLenPrefixedUTF8String( ref idx );
         var valueTuple = this.ReadCAFixedArgument( caBLOB, ref idx, type );
         return CILCustomAttributeFactory.NewNamedArgument(
            declType.BaseTypeChain().SelectMany( t => isField ? (IEnumerable<CILElementForNamedCustomAttribute>) t.DeclaredFields : t.DeclaredProperties ).First( e => String.Equals( e.Name, name ) ),
            CILCustomAttributeFactory.NewTypedArgument( valueTuple.Item2, valueTuple.Item1 )
            );
      }

      private Object ReadConstantValue( Byte[] blob, SignatureElementTypes constType )
      {
         Int32 idx = 0;
         switch ( constType )
         {
            case SignatureElementTypes.Boolean:
               return blob.ReadByteFromBytes( ref idx ) == 1;
            case SignatureElementTypes.Char:
               return Convert.ToChar( blob.ReadUInt16LEFromBytes( ref idx ) );
            case SignatureElementTypes.I1:
               return blob.ReadSByteFromBytes( ref idx );
            case SignatureElementTypes.U1:
               return blob.ReadByteFromBytes( ref idx );
            case SignatureElementTypes.I2:
               return blob.ReadInt16LEFromBytes( ref idx );
            case SignatureElementTypes.U2:
               return blob.ReadUInt16LEFromBytes( ref idx );
            case SignatureElementTypes.I4:
               return blob.ReadInt32LEFromBytes( ref idx );
            case SignatureElementTypes.U4:
               return blob.ReadUInt32LEFromBytes( ref idx );
            case SignatureElementTypes.I8:
               return blob.ReadInt64LEFromBytes( ref idx );
            case SignatureElementTypes.U8:
               return blob.ReadUInt64LEFromBytes( ref idx );
            case SignatureElementTypes.R4:
               return blob.ReadSingleLEFromBytes( ref idx );
            case SignatureElementTypes.R8:
               return blob.ReadDoubleLEFromBytes( ref idx );
            case SignatureElementTypes.String:
               return MetaDataConstants.USER_STRING_ENCODING.GetString( blob, 0, blob.Length );
            default:
               idx = blob.ReadInt32LEFromBytesNoRef( 0 );
               if ( idx != 0 )
               {
                  throw new BadImageFormatException( "Other const types than primitives should be serialized as zero int32's." );
               }
               return null;
         }
      }

      private CILTypeBase ResolveType( TableIndex typeDefOrRef, CILTypeBase[] typeGArgs, CILTypeBase[] methodGArgs )
      {
         switch ( typeDefOrRef.table )
         {
            case Tables.TypeDef:
               return this._typeDef[typeDefOrRef.idx];
            case Tables.TypeRef:
               return this._typeRef[typeDefOrRef.idx].Value;
            case Tables.TypeSpec:
               return this.ResolveTypeSpec( typeDefOrRef.idx, typeGArgs, methodGArgs );
            default:
               throw new ArgumentException( "Could not resolve type from " + typeDefOrRef + "." );
         }
      }

      private CILType ResolveTypeRef( Int32 idx )
      {
         var row = this._md.typeRef[idx];
         CILType result;
         if ( row.Item1.HasValue )
         {
            var tIdx = row.Item1.Value;
            switch ( tIdx.table )
            {
               case Tables.TypeRef:
                  result = this.ResolveTypeRef( tIdx.idx ).DeclaredNestedTypes.First( nt => nt.Name == row.Item2 );
                  break;
               case Tables.ModuleRef:
                  result = this.LoadTypeFromAssemblyRef( 0, this._md.moduleRef[tIdx.idx], row.Item3, row.Item2, false );
                  break;
               case Tables.Module:
                  result = this._typeDef[tIdx.idx];
                  break;
               case Tables.AssemblyRef:
                  result = this.LoadTypeFromAssemblyRef( tIdx.idx + 1, null, row.Item3, row.Item2, false );
                  break;
               default:
                  throw new BadImageFormatException( "Unknown table index in TypeRef table at " + idx + " (" + tIdx + ")" );
            }
         }
         else
         {
            var eRow = this._md.exportedType.FirstOrDefault( e => e.Item3 == row.Item2 && e.Item4 == row.Item3 );
            if ( eRow == null )
            {
               throw new BadImageFormatException( "TypeRef with index " + idx + " had its resolution scope as null, but no corresponding row found in ExportedType." );
            }
            else
            {
               result = this.ResolveExportedTypeAsCILType( eRow.Item5, eRow.Item4, eRow.Item3 );
            }
         }
         return result;
      }

      private CILTypeBase ResolveTypeSpec( Int32 idx, CILTypeBase[] typeGArgs, CILTypeBase[] methodGArgs )
      {
         var sigIdx = 0;
         // TODO add one more parameter to ReadTypeFromSig so it would throw on invalid type specs
         return this.ReadTypeFromSig( this._md.typeSpec[idx], ref sigIdx, typeGArgs, methodGArgs );
      }

      private CILElementTokenizableInILCode ResolveMemberRef( Int32 pIdx, CILTypeBase[] pTypeGArgs, CILTypeBase[] pMethodGArgs, ILResolveKind pRKind )
      {
         //return this._memberRefCache.GetOrAdd( Tuple.Create( pIdx, pTypeGArgs, pMethodGArgs, pRKind ), tuple =>
         //{
         //   var row = this._md.memberRef[tuple.Item1];
         //   CILType thisType;
         //   switch ( row.Item1.table )
         //   {
         //      case Tables.TypeRef:
         //         thisType = this._typeRef[row.Item1.idx].Value;
         //         return ResolveActualMember( thisType, row.Item2, row.Item3, tuple.Item4 );
         //      case Tables.ModuleRef:
         //         throw new NotImplementedException();
         //      case Tables.MethodDef:
         //         // TODO
         //         throw new NotImplementedException();
         //      case Tables.TypeSpec:
         //         thisType = (CILType) this.ResolveTypeSpec( row.Item1.idx, tuple.Item2, tuple.Item3 );
         //         return ResolveActualMember( thisType, row.Item2, row.Item3, tuple.Item4 );
         //      default:
         //         throw new BadImageFormatException( "Member ref contained invalid Class index: " + row.Item1 );
         //   }
         //} );

         var row = this._md.memberRef[pIdx];
         CILType thisType;
         CILElementTokenizableInILCode retVal;
         switch ( row.Item1.table )
         {
            case Tables.TypeRef:
               thisType = this._typeRef[row.Item1.idx].Value;
               retVal = ResolveActualMember( thisType, row.Item2, row.Item3, pRKind );
               break;
            case Tables.ModuleRef:
               throw new NotImplementedException();
            case Tables.MethodDef:
               // TODO
               throw new NotImplementedException();
            case Tables.TypeSpec:
               thisType = (CILType) this.ResolveTypeSpec( row.Item1.idx, pTypeGArgs, pMethodGArgs );
               retVal = ResolveActualMember( thisType, row.Item2, row.Item3, pRKind );
               break;
            default:
               throw new BadImageFormatException( "Member ref contained invalid Class index: " + row.Item1 );
         }

         if ( retVal == null )
         {
            throw new InvalidOperationException( "Failed to resolve referenced member \"" + row.Item2 + "\" (" + pRKind + ") in " + thisType + "; signature: " + CommonUtils.StringConversions.ByteArray2HexStr( row.Item3 ) + "; assembly: " + thisType.Module.Assembly.Name + "; memberRef index: " + pIdx + "." );
         }

         return retVal;
      }

      private CILElementTokenizableInILCode ResolveActualMember( CILType declType, String name, Byte[] sig, ILResolveKind rKind )
      {
         var typeGArgs = declType.GenericArguments.Any() ? declType.GenericArguments.ToArray() : null;

         switch ( rKind )
         {
            case ILResolveKind.Field:
               return declType.DeclaredFields.FirstOrDefault( f => String.Equals( f.Name, name ) && this.MatchFieldToSignature( f, sig, typeGArgs ) );
            case ILResolveKind.MethodBase:
               return String.Equals( name, CILConstructorImpl.STATIC_CTOR_NAME ) ?
                  declType.Constructors.FirstOrDefault( ctor => ctor.Attributes.IsStatic() ) :
                  ( String.Equals( name, CILConstructorImpl.INSTANCE_CTOR_NAME ) ?
                     (CILMethodBase) declType.Constructors.FirstOrDefault( ctor => !ctor.Attributes.IsStatic() && this.MatchMethodToSignature( ctor, sig, typeGArgs ) ) :
                     declType.DeclaredMethods.FirstOrDefault( m => String.Equals( m.Name, name ) && this.MatchMethodToSignature( m, sig, typeGArgs ) )
                     );
            case ILResolveKind.MethodBaseOrField:
               return String.Equals( name, CILConstructorImpl.STATIC_CTOR_NAME ) ?
                  declType.Constructors.FirstOrDefault( ctor => ctor.Attributes.IsStatic() ) :
                  ( String.Equals( name, CILConstructorImpl.INSTANCE_CTOR_NAME ) ?
                     (CILElementTokenizableInILCode) declType.Constructors.FirstOrDefault( ctor => !ctor.Attributes.IsStatic() && this.MatchMethodToSignature( ctor, sig, typeGArgs ) ) :
                     (CILElementTokenizableInILCode) declType.DeclaredMethods.Cast<CILElementWithSimpleName>().Concat( declType.DeclaredFields ).FirstOrDefault( e => String.Equals( e.Name, name ) && ( e is CILField ? this.MatchFieldToSignature( (CILField) e, sig, typeGArgs ) : this.MatchMethodToSignature( (CILMethod) e, sig, typeGArgs ) ) )
                     );
            default:
               throw new ArgumentException( "Unrecognized resolve kind when resolving member ref; the resolve kind is " + rKind );
         }
      }

      private CILTypeParameter ResolveGenericParam( Int32 paramIdx, CILType declType, CILMethod declMethod )
      {
         return (CILTypeParameter) this._gParams[declMethod == null ? (CILElementWithGenericArguments<Object>) declType : declMethod][this._md.genericParam[paramIdx].Item1];
      }

      private LazyWithLock<ListProxy<CILCustomAttribute>> ReadCustomAttributes( Tables targetTable, Int32 idx, CILElementKind cliKind, Int32 contextID )
      {
         return new LazyWithLock<ListProxy<CILCustomAttribute>>( () => this._ctx.CollectionsFactory.NewListProxy(
                  GetReferencingRowsFromOrdered( this._md.customAttribute, targetTable, idx, row => row.Item1 )
                  .Select( rowIdx => ReadCustomAttribute( rowIdx, this._ctx.Cache.ResolveAnyID( cliKind, contextID ) ) )
                  .ToList() )
               );
      }

      // Item1 - custom mods
      // Item2 - param type
      private Lazy<TFieldSig> ReadFieldSignature( Byte[] sig, Int32 thisTypeIdx )
      {
         return new Lazy<TFieldSig>( () =>
         {
            var idx = 1;
            CILTypeBase[] tArgs = null;
            if ( thisTypeIdx != -1 )
            {
               this._gParams.TryGetValue( this._typeDef[thisTypeIdx], out tArgs );
            }
            return this.DoReadFieldSignature( sig, ref idx, tArgs, null );
         } );
      }

      private Lazy<TMethodDefOrRefSig> ReadMethodSignature( Byte[] sig, Int32 thisTypeIdx, Int32 thisMethodIdx )
      {
         return new Lazy<TMethodDefOrRefSig>( () =>
         {
            CILTypeBase[] tArgs = null, mArgs = null;
            if ( thisTypeIdx != -1 )
            {
               this._gParams.TryGetValue( this._typeDef[thisTypeIdx], out tArgs );
            }
            if ( thisMethodIdx != -1 && this._methods[thisMethodIdx] is CILMethod )
            {
               this._gParams.TryGetValue( this._methods[thisMethodIdx] as CILMethod, out mArgs );
            }
            Int32 idx = 0;
            return this.DoReadMethodSignature( sig, ref idx, tArgs, mArgs );
         }, LazyThreadSafetyMode.ExecutionAndPublication );
      }

      // Item1 - return param
      // Item2 - method params
      // Item3 - method varargs
      private TMethodDefOrRefSig DoReadMethodSignature( Byte[] sig, ref Int32 idx, CILTypeBase[] typeGArgs, CILTypeBase[] methodGArgs )
      {
         if ( ( ( (SignatureStarters) sig.ReadByteFromBytes( ref idx ) ) & SignatureStarters.Generic ) != 0 )
         {
            // Skip generic argument count
            sig.DecompressUInt32( ref idx );
         }
         var amountOfParams = sig.DecompressUInt32( ref idx );
         var rParam = this.DoReadFieldSignature( sig, ref idx, typeGArgs, methodGArgs );
         IList<TFieldSig> iParams = new List<TFieldSig>( amountOfParams );
         IList<TFieldSig> vParams = null;
         var curPList = iParams;
         for ( var i = 0; i < amountOfParams; ++i )
         {
            if ( sig[idx] == (Byte) SignatureElementTypes.Sentinel )
            {
               vParams = new List<TFieldSig>();
               curPList = vParams;
            }
            curPList.Add( this.DoReadFieldSignature( sig, ref idx, typeGArgs, methodGArgs ) );
         }
         return Tuple.Create( rParam, iParams, vParams );
      }

      private TFieldSig DoReadFieldSignature( Byte[] sig, ref Int32 idx, CILTypeBase[] typeGArgs, CILTypeBase[] methodGArgs )
      {
         return Tuple.Create( this._ctx.CollectionsFactory.NewListProxy( this.ReadCustomMods( sig, ref idx, typeGArgs, methodGArgs ) ), this.ReadTypeFromSig( sig, ref idx, typeGArgs, methodGArgs ) );
      }

      private IList<CILCustomModifier> ReadCustomMods( Byte[] sig, ref Int32 idx, CILTypeBase[] typeGArgs, CILTypeBase[] methodGArgs )
      {
         var curByte = sig.ReadByteFromBytes( ref idx );
         var cMods = new List<CILCustomModifier>();
         while ( curByte == (Byte) SignatureElementTypes.CModOpt || curByte == (Byte) SignatureElementTypes.CModReqd )
         {
            // Custom mod
            var ctIdx = new TableIndex( TokenUtils.DecodeTypeDefOrRefOrSpec( sig, ref idx ) );
            cMods.Add( new CILCustomModifierImpl( curByte == (Byte) SignatureElementTypes.CModOpt ? CILCustomModifierOptionality.Optional : CILCustomModifierOptionality.Required, (CILType) this.ResolveType( ctIdx, typeGArgs, methodGArgs ) ) );
            curByte = sig.ReadByteFromBytes( ref idx );
         }
         // Go back one step in order for this.ReadTypeFromSig to work
         --idx;
         return cMods;
      }

      private static void SkipCustomMods( Byte[] sig, ref Int32 idx )
      {
         var curByte = sig.ReadByteFromBytes( ref idx );
         while ( curByte == (Byte) SignatureElementTypes.CModOpt || curByte == (Byte) SignatureElementTypes.CModReqd )
         {
            // Skip custom mods
            sig.DecompressUInt32( ref idx );
            curByte = sig.ReadByteFromBytes( ref idx );
         }
         --idx;
      }

      private CILParameter CreateMethodReturnParameter( TMethodDefOrRefSig methodSig, Int32 curMethodIdx, Int32 curParamIdxInMethodRow )
      {
         var max = CalculateMaxForRangeRef( this._md.methodDef, curMethodIdx, this._md.param, row => row.Item6.idx );

         // Parameters have secondary ordering by their position, so checking first one only is ok
         curParamIdxInMethodRow = max > curParamIdxInMethodRow && this._md.param[curParamIdxInMethodRow].Item2 == 0 ? curParamIdxInMethodRow : -1;
         return this.CreateParameter( curMethodIdx, methodSig.Item1, curParamIdxInMethodRow, E_CIL.RETURN_PARAMETER_POSITION );
      }

      private ListProxy<CILParameter> CreateMethodParameters( TMethodDefOrRefSig methodSig, Int32 curMethodIdx, Int32 curParamIdx )
      {
         var max = CalculateMaxForRangeRef( this._md.methodDef, curMethodIdx, this._md.param, row => row.Item6.idx );

         return this._ctx.CollectionsFactory.NewListProxy( methodSig.Item2.Select( ( sig, idx ) =>
         {
            ++idx;
            while ( curParamIdx < max && this._md.param[curParamIdx].Item2 < idx )
            {
               ++curParamIdx;
            }
            // Check if there exists a row in parameter table corresponding to this parameter
            var paramRowIdxToGive = curParamIdx < max && this._md.param[curParamIdx].Item2 == idx ? curParamIdx : -1;
            return this.CreateParameter( curMethodIdx, sig, paramRowIdxToGive, idx - 1 );
         } ).ToList() );
      }

      private CILParameter CreateParameter( Int32 curMethodIdx, TFieldSig paramSig, Int32 paramRowIdx, Int32 paramPosition )
      {
         var paramRow = paramRowIdx < 0 ? null : this._md.param[paramRowIdx];

         return this._ctx.Cache.NewParameter( id => new CILParameterImpl(
            this._ctx,
            id,
            paramRow == null ? new LazyWithLock<ListProxy<CILCustomAttribute>>( () => this._ctx.CollectionsFactory.NewListProxy( new List<CILCustomAttribute>() ) ) : this.ReadCustomAttributes( Tables.Parameter, paramRowIdx, CILElementKind.Parameter, id ),
            new SettableValueForEnums<ParameterAttributes>( paramRow == null ? ParameterAttributes.None : paramRow.Item1 ),
            paramRow == null ? paramPosition : paramRow.Item2 - 1,
            new SettableValueForClasses<String>( paramRow == null ? null : paramRow.Item3 ),
            () => this._methods[curMethodIdx],
            () => paramSig.Item2,
            this.ReadConstantValue( Tables.Parameter, paramRowIdx ),
            new LazyWithLock<ListProxy<CILCustomModifier>>( () => paramSig.Item1 ),
            new SettableLazy<MarshalingInfo>( () =>
            {
               var mRows = GetReferencingRowsFromOrdered( this._md.fieldMarshal, Tables.Parameter, paramRowIdx, row => row.Item1 );
               return mRows.Count > 0 ? this.ReadMarshalInfo( mRows[0] ) : null;
            } ),
            true
            ) );
      }

      private static CallingConventions ReadCallingConventionFromSignature( Byte[] sig, Int32 idx = 0 )
      {
         var starter = (SignatureStarters) sig[idx];
         CallingConventions result = 0;
         if ( ( starter & SignatureStarters.HasThis ) != 0 )
         {
            result = CallingConventions.HasThis;
         }
         if ( ( starter & SignatureStarters.ExplicitThis ) != 0 )
         {
            result |= CallingConventions.ExplicitThis;
         }
         if ( ( starter & SignatureStarters.VarArgs ) != 0 )
         {
            result |= CallingConventions.VarArgs;
         }
         else if ( ( starter & SignatureStarters.Generic ) == 0 )
         {
            result |= CallingConventions.Standard;
         }
         return result;
      }

      private UInt32 CalculateFieldTypeSize( Byte[] fieldSig )
      {
         var idx = 1; // Skip first byte
         SkipCustomMods( fieldSig, ref idx );
         UInt32 result;
         switch ( (SignatureElementTypes) fieldSig[idx++] )
         {
            case SignatureElementTypes.Boolean:
               result = sizeof( Boolean ); // TODO is this actually 1 or 4?
               break;
            case SignatureElementTypes.I1:
            case SignatureElementTypes.U1:
               result = 1;
               break;
            case SignatureElementTypes.I2:
            case SignatureElementTypes.U2:
            case SignatureElementTypes.Char:
               result = 2;
               break;
            case SignatureElementTypes.I4:
            case SignatureElementTypes.U4:
            case SignatureElementTypes.R4:
            case SignatureElementTypes.FnPtr:
            case SignatureElementTypes.Ptr: // I am not 100% sure of this.
               result = 4;
               break;
            case SignatureElementTypes.I8:
            case SignatureElementTypes.U8:
            case SignatureElementTypes.R8:
               result = 8;
               break;
            case SignatureElementTypes.ValueType:
               var fieldType = new TableIndex( TokenUtils.DecodeTypeDefOrRefOrSpec( fieldSig, ref idx ) );
               if ( fieldType.table != Tables.TypeDef )
               {
                  throw new NotImplementedException( "Found field with RVA, and field type was in other module/assembly, what now?" );
               }
               var extendInfo = this._md.typeDef[fieldType.idx].Item4;
               var isEnum = extendInfo.HasValue
                  && extendInfo.Value.table != Tables.TypeSpec
                  && this.IsEnum( extendInfo );
               if ( isEnum )
               {
                  var fieldStartIdx = this._md.typeDef[fieldType.idx].Item5.idx;
                  var fieldEndIdx = fieldType.idx + 1 >= this._md.typeDef.Length ?
                     this._md.field.Length :
                     this._md.typeDef[fieldType.idx + 1].Item5.idx;
                  var fIdx = -1;
                  for ( var i = fieldStartIdx; i < fieldEndIdx; ++i )
                  {
                     if ( !this._md.field[i].Item1.IsStatic() )
                     {
                        fIdx = i;
                        break;
                     }
                  }
                  if ( fIdx == -1 )
                  {
                     throw new BadImageFormatException( "Could not find instance field for enum " + GetReadableTypeName( this._md.typeDef[fieldType.idx].Item2, this._md.typeDef[fieldType.idx].Item3 ) + "." );
                  }
                  else
                  {
                     result = CalculateFieldTypeSize( this._md.field[fIdx].Item3 );
                  }
               }
               else
               {
                  var cilIdx = GetReferencingRowsFromOrdered( this._md.classLayout, Tables.TypeDef, fieldType.idx, row => row.Item3 );
                  result = this._md.classLayout[cilIdx.First()].Item2;
               }
               break;
            default:
               throw new BadImageFormatException( "Unknown field type kind for field with RVA: " + (SignatureElementTypes) fieldSig[idx - 1] + "." );
         }
         return result;
      }

      private Boolean IsEnum( TableIndex? tIdx )
      {
         var result = tIdx.HasValue && tIdx.Value.table != Tables.TypeSpec;
         if ( result )
         {
            var idx = tIdx.Value.idx;
            String tn, ns;
            if ( tIdx.Value.table == Tables.TypeDef )
            {
               tn = this._md.typeDef[idx].Item2;
               ns = this._md.typeDef[idx].Item3;
            }
            else
            {
               tn = this._md.typeRef[idx].Item2;
               ns = this._md.typeRef[idx].Item3;
            }

            result = tn == "Enum" && ns == "System";
         }
         return result;
      }

      private static String GetReadableTypeName( String tn, String ns )
      {
         return ns + ( ns == null ? "" : "." ) + tn;
      }

      private CILTypeBase ReadTypeFromSig( Byte[] sig, ref Int32 idx, CILTypeBase[] typeGArgs, CILTypeBase[] methodGArgs )
      {
         Int32 gIdx;
         SignatureElementTypes sigType = (SignatureElementTypes) sig[idx++];
         switch ( sigType )
         {
            case SignatureElementTypes.Boolean:
            case SignatureElementTypes.Char:
            case SignatureElementTypes.I1:
            case SignatureElementTypes.U1:
            case SignatureElementTypes.I2:
            case SignatureElementTypes.U2:
            case SignatureElementTypes.I4:
            case SignatureElementTypes.U4:
            case SignatureElementTypes.I8:
            case SignatureElementTypes.U8:
            case SignatureElementTypes.R4:
            case SignatureElementTypes.R8:
            case SignatureElementTypes.I:
            case SignatureElementTypes.U:
            case SignatureElementTypes.String:
            case SignatureElementTypes.Object:
            case SignatureElementTypes.Void:
            case SignatureElementTypes.TypedByRef:
               return this._coreTypes[sigType].Value;
            case SignatureElementTypes.Array:
               return this.ReadTypeFromSig( sig, ref idx, typeGArgs, methodGArgs ).MakeElementType( ElementKind.Array, this.ReadArrayInfo( sig, ref idx ) );
            case SignatureElementTypes.Class:
            case SignatureElementTypes.ValueType:
               return this.ResolveType( new TableIndex( TokenUtils.DecodeTypeDefOrRefOrSpec( sig, ref idx ) ), typeGArgs, methodGArgs );
            case SignatureElementTypes.FnPtr:
               var cconv = (UnmanagedCallingConventions) sig[idx];
               var mSig = this.DoReadMethodSignature( sig, ref idx, typeGArgs, methodGArgs );
               return new CILMethodSignatureImpl( this._ctx, this._thisModule, cconv, mSig.Item1.Item1, mSig.Item1.Item2, mSig.Item2, null );
            case SignatureElementTypes.GenericInst:
               //var isValType = sig.ReadByteFromBytes( ref idx ) == (Byte) SignatureElementTypes.ValueType;
               ++idx;
               var gDef = (CILType) this.ResolveType( new TableIndex( TokenUtils.DecodeTypeDefOrRefOrSpec( sig, ref idx ) ), typeGArgs, methodGArgs );
               var gArgs = new CILTypeBase[sig.DecompressUInt32( ref idx )];
               for ( var i = 0; i < gArgs.Length; ++i )
               {
                  gArgs[i] = this.ReadTypeFromSig( sig, ref idx, typeGArgs, methodGArgs );
               }
               return gDef.MakeGenericType( gArgs );
            case SignatureElementTypes.MVar:

               if ( methodGArgs == null )
               {
                  throw new BadImageFormatException( "Type signature contained reference to method generic argument, but it is not used from within generic method context." );
               }
               gIdx = sig.DecompressUInt32( ref idx );
               if ( gIdx < 0 || gIdx >= methodGArgs.Length )
               {
                  throw new BadImageFormatException( "Type signature contained reference to method generic argument number " + gIdx + " but it only has " + methodGArgs.Length + " generic arguments." );
               }
               return methodGArgs[gIdx];
            case SignatureElementTypes.Ptr:
               SkipCustomMods( sig, ref idx );
               return this.ReadTypeFromSig( sig, ref idx, typeGArgs, methodGArgs ).MakePointerType();
            case SignatureElementTypes.SzArray:
               SkipCustomMods( sig, ref idx );
               return this.ReadTypeFromSig( sig, ref idx, typeGArgs, methodGArgs ).MakeArrayType();
            case SignatureElementTypes.Var:
               if ( typeGArgs == null )
               {
                  throw new BadImageFormatException( "Type signature contained reference to type generic argument, but it is not used from within generic type context." );
               }
               gIdx = sig.DecompressUInt32( ref idx );
               if ( gIdx < 0 || gIdx >= typeGArgs.Length )
               {
                  throw new BadImageFormatException( "Type signature contained reference to method generic argument number " + gIdx + " but it only has " + typeGArgs.Length + " generic arguments." );
               }
               return typeGArgs[gIdx];
            case SignatureElementTypes.ByRef:
               return this.ReadTypeFromSig( sig, ref idx, typeGArgs, methodGArgs ).MakeByRefType();
            default:
               throw new BadImageFormatException( "Unknown type starter: " + (SignatureElementTypes) sig[idx - 1] );
         }
      }

      private GeneralArrayInfo ReadArrayInfo( Byte[] sig, ref Int32 idx )
      {
         var rank = sig.DecompressUInt32( ref idx );
         var curSize = sig.DecompressUInt32( ref idx );
         var sizes = curSize > 0 ?
            new Int32[curSize] :
            null;
         while ( curSize > 0 )
         {
            sizes[sizes.Length - curSize] = sig.DecompressUInt32( ref idx );
            --curSize;
         }
         curSize = sig.DecompressUInt32( ref idx );
         var loBounds = curSize > 0 ?
            new Int32[curSize] :
            null;
         while ( curSize > 0 )
         {
            loBounds[loBounds.Length - curSize] = sig.DecompressInt32( ref idx );
            --curSize;
         }
         return new GeneralArrayInfo( rank, sizes, loBounds );
      }

      private CILType GetCoreType( Type type, String actualNS = null, String actualName = null )
      {
         actualNS = type == null ? actualNS : type.Namespace;
         actualName = type == null ? actualName : type.Name;

         var mod = this._mscorLibRef.Value;
         return mod.GetTypeByName( ( actualNS == null ? "" : ( actualNS + "." ) ) + actualName );
      }

      private CILModule ResolveMSCorLib()
      {
         CILModule result;
         if ( this._thisIsMSCorLib.Value )
         {
            result = this._thisModule;
         }
         else
         {
            var aRefIDx = this.FindNewestAssemblyRefIdx( this._eArgs.CorLibName ) + 1;
            CILAssembly ass;
            if ( aRefIDx < 0 )
            {
               var an = new CILAssemblyName( this._eArgs.CorLibName, this._eArgs.CorLibMajor, this._eArgs.CorLibMinor, this._eArgs.CorLibBuild, this._eArgs.CorLibRevision );
               ass = this.TryResolveAssemblyRef( an );
            }
            else
            {
               ass = this.TryResolveAssemblyRef( aRefIDx );
            }
            if ( ass == null )
            {
               throw new InvalidOperationException( "Failed to resolve 'mscorlib' reference. The one given in emitting arguments is '" + this._eArgs.CorLibName + "', but no reference exists to it." );
            }
            result = ass.MainModule;
            if ( result == null )
            {
               throw new InvalidOperationException( "Failed to resolve 'mscorlib' reference. The referenced assembly contained no modules." );
            }
         }
         return result;
      }

      private CILType ResolveTypeString( String typeString )
      {
         return typeString == null ? null : this._typeStringResolutions.GetOrAdd( typeString, tsArg => this.DoResolveTypeString( tsArg ) );
      }

      private CILType DoResolveTypeString( String typeString )
      {
         var parsingResult = Utils.ParseTypeString( typeString );
         return this.DoResolveTypeString( typeString, parsingResult );
      }

      private CILType DoResolveTypeString( String typeString, Utils.TypeParseResult parseResult )
      {
         var targetAssembly = parseResult.assemblyName;
         var isInThisAssemblyForSure = this._thisModule.Assembly.Name.CorePropertiesEqual( targetAssembly );

         var assToUse = targetAssembly == null || isInThisAssemblyForSure ? this._thisModule.Assembly : this.TryResolveAssemblyRef( targetAssembly );
         CILType result = null;
         if ( assToUse != null )
         {
            // Type must be in this or mscorlib assembly
            // Check this assembly first
            var tn = Utils.NamespaceAndTypeName( parseResult.nameSpace, parseResult.typeName );
            result = assToUse.Modules.Select( mod => mod.GetTypeByName( tn, false ) ).Where( t => t != null ).FirstOrDefault();
         }

         // Then check mscorlib if needed
         if ( result == null && !isInThisAssemblyForSure )
         {
            result = this.LoadTypeFromAssemblyRef( this.FindNewestAssemblyRefIdx( this._eArgs.CorLibName ) + 1, null, parseResult.nameSpace, parseResult.typeName, true );
         }

         // Process nested type information, element type information, and generic argument information
         if ( result != null && parseResult.nestedTypes != null )
         {
            foreach ( var nt in parseResult.nestedTypes )
            {
               result = result == null ? null : result.DeclaredNestedTypes.FirstOrDefault( dt => String.Equals( dt.Name, nt ) );
            }
         }

         if ( result == null )
         {
            throw new BadImageFormatException( "Could not resolve type string " + typeString + " located in " + ( targetAssembly == null ? targetAssembly.ToString() : "this assembly or mscorlib" ) + "." );
         }

         // Element type info
         if ( parseResult.elementInfo != null )
         {
            foreach ( var ei in parseResult.elementInfo )
            {
               result = result.MakeElementType( ei.Item1, ei.Item2 );
            }
         }

         // Generic arguments
         if ( parseResult.genericArguments != null )
         {
            var gArgs = new CILType[parseResult.genericArguments.Count];
            for ( var i = 0; i < gArgs.Length; ++i )
            {
               gArgs[i] = this.ResolveTypeString( parseResult.genericArguments[i] );
            }
         }

         return result;
      }

      internal Int32 FindNewestAssemblyRefIdx( String aName )
      {
         UInt16 major = 0, minor = 0, build = 0, rev = 0;
         var idx = Int32.MinValue;
         for ( var i = 0; i < this._md.assemblyRef.Length; ++i )
         {
            var aRef = this._md.assemblyRef[i];
            if ( String.Equals( aRef.Item7, aName ) )
            {
               if ( aRef.Item1 > major )
               {
                  idx = i;
                  major = aRef.Item1;
                  minor = aRef.Item2;
                  build = aRef.Item3;
                  rev = aRef.Item4;
               }
               else if ( aRef.Item1 == major )
               {
                  if ( aRef.Item2 > minor )
                  {
                     idx = i;
                     minor = aRef.Item2;
                     build = aRef.Item3;
                     rev = aRef.Item4;
                  }
                  else if ( aRef.Item2 == minor )
                  {
                     if ( aRef.Item3 > build )
                     {
                        idx = i;
                        build = aRef.Item3;
                        rev = aRef.Item4;
                     }
                     else if ( aRef.Item3 == build )
                     {
                        if ( aRef.Item4 > rev )
                        {
                           idx = i;
                           rev = aRef.Item4;
                        }
                     }
                  }
               }
            }
         }
         return idx;
      }

      private CILTypeCode ResolveTypeCode( CILType type )
      {
         if ( Object.Equals( type.BaseType, this._enumType.Value ) )
         {
            return ( (CILTypeOrTypeParameter) type.GetEnumValueField().FieldType ).TypeCode;
         }
         else if ( this._thisIsMSCorLib.Value )
         {
            return this.ResolveTypeCodeTextual( type.Namespace, type.Name );
         }
         else
         {
            return CILTypeCode.Object;
         }
      }

      private CILTypeCode ResolveTypeCodeTextual( String ns, String tn )
      {
         if ( String.Equals( ns, "System" ) )
         {
            switch ( tn )
            {
               case "Boolean":
                  return CILTypeCode.Boolean;
               case "Char":
                  return CILTypeCode.Char;
               case "SByte":
                  return CILTypeCode.SByte;
               case "Byte":
                  return CILTypeCode.Byte;
               case "Int16":
                  return CILTypeCode.Int16;
               case "UInt16":
                  return CILTypeCode.UInt16;
               case "Int32":
                  return CILTypeCode.Int32;
               case "UInt32":
                  return CILTypeCode.UInt32;
               case "Int64":
                  return CILTypeCode.Int64;
               case "UInt64":
                  return CILTypeCode.UInt64;
               case "Single":
                  return CILTypeCode.Single;
               case "Double":
                  return CILTypeCode.Double;
               case "String":
                  return CILTypeCode.String;
               case "Decimal":
                  return CILTypeCode.Decimal;
               case "DateTime":
                  return CILTypeCode.DateTime;
               case "Void":
                  return CILTypeCode.Void;
               case "ValueType":
                  return CILTypeCode.Value;
               case "Enum":
                  return CILTypeCode.Enum;
               case "TypedReference":
                  return CILTypeCode.TypedByRef;
               case "IntPtr":
                  return CILTypeCode.IntPtr;
               case "UIntPtr":
                  return CILTypeCode.UIntPtr;
               case "Object":
                  return CILTypeCode.SystemObject;
               case "Type":
                  return CILTypeCode.Type;
               default:
                  return CILTypeCode.Object;
            }
         }
         else
         {
            return CILTypeCode.Object;
         }
      }
      private CILType LoadTypeFromAssemblyRef( Int32 aRefIdx, String modName, String ns, String name, Boolean invalidARefIsMsCorLib )
      {
         if ( aRefIdx < 0 )
         {
            if ( invalidARefIsMsCorLib )
            {
               var result = this._mscorLibRef.Value.GetTypeByName( Utils.NamespaceAndTypeName( ns, name ), false );
               if ( result == null )
               {
                  throw new InvalidOperationException( "Could not resolve type " + ns + CILTypeImpl.NAMESPACE_SEPARATOR + name + "." );
               }
               return result;
            }
            else
            {
               throw new BadImageFormatException( "Could not resolve type reference: " + ns + CILTypeImpl.NAMESPACE_SEPARATOR + name + "." );
            }
         }
         else
         {
            return this.LoadTypeFromAssemblyRefNoAnonMSCorLib( aRefIdx, modName, ns, name );
         }
      }

      private CILType LoadTypeFromAssemblyRefNoAnonMSCorLib( Int32 aRefIdx, String modName, String ns, String name )
      {
         var resolvedAss = this.TryResolveAssemblyRef( aRefIdx );
         var mod = modName == null ? resolvedAss.MainModule : resolvedAss.Modules.First( m => String.Equals( modName, m.Name ) );
         var result = mod.GetTypeByName( Utils.NamespaceAndTypeName( ns, name ), false );
         if ( result == null )
         {
            // Try searching type forwarders
            TypeForwardingInfo tf;
            if ( resolvedAss.TryGetTypeForwarder( name, ns, out tf ) )
            {
               result = this.TryResolveAssemblyRef( tf.AssemblyName, resolvedAss.MainModule ).GetAllDefinedTypes().FirstOrDefault( t => String.Equals( name, t.Name ) && String.Equals( ns, t.Namespace ) );
            }
            if ( result == null )
            {
               throw new InvalidOperationException( "Could not resolve type " + ns + CILTypeImpl.NAMESPACE_SEPARATOR + name + "." );
            }
         }
         return result;
      }

      internal CILAssembly TryResolveAssemblyRef( Int32 aRefIdx, CILModule overrideThisModule = null )
      {
         lock ( this._resolvedAssemblyRefsLock )
         {
            var result = this._resolvedAssemblyRefs[aRefIdx];
            if ( this._resolvedAssemblyRefs[aRefIdx] == null )
            {
               if ( aRefIdx == 0 )
               {
                  result = this._thisModule.Assembly;
               }
               else
               {
                  var aRow = this._md.assemblyRef[aRefIdx - 1];
                  var aRefName = new CILAssemblyName( aRow.Item7, aRow.Item1, aRow.Item2, aRow.Item3, aRow.Item4, AssemblyHashAlgorithm.None, aRow.Item5, aRow.Item6, aRow.Item8 );
                  result = this.TryResolveAssemblyRef( aRefName, overrideThisModule );
               }
               if ( result == null )
               {
                  // In order to prevent resolving over and over and over again in weird error situations
                  throw new Exception( "Something weird happened - either this module's assembly was null or assembly reference load event launcher was faulty." );
               }

               this._resolvedAssemblyRefs[aRefIdx] = result;
            }
            return result;
         }

      }

      internal CILAssembly TryResolveAssemblyRef( CILAssemblyName aName, CILModule overrideThisModule = null )
      {
         CILAssembly resolvedAss = null;
         if ( this._customAssemblyRefLoader != null )
         {
            var thisModule = this._thisModule ?? overrideThisModule;
            resolvedAss = this._customAssemblyRefLoader( thisModule, aName );
         }
         if ( resolvedAss == null )
         {
            resolvedAss = this._ctx.LaunchAssemblyRefResolveEvent( new AssemblyRefResolveFromLoadedAssemblyEventArgs( aName, this._ctx ) );
         }
         return resolvedAss;
      }

      private static Tuple<Boolean, CILTypeBase>[] EMPTY_LOCAL_VARS = new Tuple<Boolean, CILTypeBase>[0];

      private Tuple<Boolean, CILTypeBase>[] ReadLocalVarSignature( Byte[] sig, CILTypeBase[] typeGArgs, CILTypeBase[] methodGArgs )
      {
         var idx = 1; // Skip intro
         var count = sig.DecompressUInt32( ref idx );
         Tuple<Boolean, CILTypeBase>[] result;
         if ( count > 0 )
         {
            result = new Tuple<Boolean, CILTypeBase>[count];
            for ( var i = 0; i < count; ++i )
            {
               switch ( (SignatureElementTypes) sig[idx] )
               {
                  case SignatureElementTypes.TypedByRef:
                     result[i] = Tuple.Create<Boolean, CILTypeBase>( false, this._coreTypes[SignatureElementTypes.TypedByRef].Value );
                     ++idx;
                     break;
                  default:
                     SkipCustomMods( sig, ref idx );
                     var pinned = (SignatureElementTypes) sig[idx] == SignatureElementTypes.Pinned;
                     if ( pinned )
                     {
                        ++idx;
                     }
                     result[i] = Tuple.Create( pinned, this.ReadTypeFromSig( sig, ref idx, typeGArgs, methodGArgs ) );
                     break;
               }
            }
         }
         else
         {
            result = EMPTY_LOCAL_VARS;
         }
         return result;
      }

      private Boolean MatchFieldToSignature( CILField field, Byte[] sig, CILTypeBase[] typeGArgs )
      {
         if ( field.DeclaringType.GenericArguments.Any() )
         {
            field = field.ChangeDeclaringType( field.DeclaringType.GenericDefinition.GenericArguments.ToArray() );
         }
         var idx = 1; // Skip first byte
         SkipCustomMods( sig, ref idx );
         return this.MatchTypeToSignature( field.FieldType, sig, ref idx, typeGArgs, null );
      }

      private Boolean MatchMethodToSignature( CILMethodBase method, Byte[] sig, CILTypeBase[] typeGArgs )
      {
         var methodGArgs = MethodKind.Method == method.MethodKind && ( (CILMethod) method ).HasGenericArguments() ? ( (CILMethod) method ).GenericArguments.ToArray() : null;
         if ( method.DeclaringType.GenericArguments.Any() )
         {
            method = method.ChangeDeclaringTypeUT( method.DeclaringType.GenericDefinition.GenericArguments.ToArray() );
         }
         var idx = 0;
         return this.MatchMethodOrSigToSignature<CILMethodBase, CILParameter>( method, sig, method.CallingConvention.GetSignatureStarter( method.Attributes.IsStatic(), methodGArgs != null ), typeGArgs, ref idx, methodGArgs, method is CILConstructor ? this._coreTypes[SignatureElementTypes.Void].Value : ( (CILMethod) method ).ReturnParameter.ParameterType, p => p.ParameterType );
      }

      private Boolean MatchMethodSigToSignature( CILMethodSignature methodSig, Byte[] binarySig, CILTypeBase[] typeGArgs, ref Int32 idx )
      {
         return this.MatchMethodOrSigToSignature<CILMethodSignature, CILParameterSignature>( methodSig, binarySig, (SignatureStarters) methodSig.CallingConvention, typeGArgs, ref idx, null, methodSig.ReturnParameter.ParameterType, p => p.ParameterType );
      }

      private Boolean MatchMethodOrSigToSignature<TMethod, TParam>(
         TMethod method,
         Byte[] sig,
         SignatureStarters requiredStarter,
         CILTypeBase[] typeGArgs,
         ref Int32 idx,
         CILTypeBase[] methodGArgs,
         CILTypeBase returnParamType,
         Func<TParam, CILTypeBase> paramExtractor
         )
         where TMethod : class, CILMethodOrSignature<TParam>
         where TParam : class, CILParameterBase<TMethod>
      {
         var result = sig[idx++] == (Byte) requiredStarter;
         if ( result )
         {
            if ( methodGArgs != null )
            {
               result = methodGArgs.Length == sig.DecompressUInt32( ref idx );
            }

            if ( result )
            {
               var pCount = sig.DecompressUInt32( ref idx );
               result = method.Parameters.Count == pCount;
               if ( result )
               {
                  result = this.MatchParamTypeToSignature( false, returnParamType, sig, ref idx, typeGArgs, methodGArgs );
                  if ( result )
                  {
                     var i = 0;
                     while ( result && i < pCount )
                     {
                        result = this.MatchParamTypeToSignature( true, paramExtractor( method.Parameters[i] ), sig, ref idx, typeGArgs, methodGArgs );
                        ++i;
                     }
                  }
               }
            }
         }
         return result;
      }

      private Boolean MatchParamTypeToSignature( Boolean acceptByRef, CILTypeBase type, Byte[] sig, ref Int32 idx, CILTypeBase[] typeGArgs, CILTypeBase[] methodGArgs )
      {
         SkipCustomMods( sig, ref idx );
         return this.MatchTypeToSignature( type, sig, ref idx, typeGArgs, methodGArgs );
      }

      private Boolean MatchTypeToSignature( CILTypeBase type, Byte[] sig, ref Int32 idx, CILTypeBase[] typeGArgs, CILTypeBase[] methodGArgs )
      {
         if ( TypeKind.Type == type.TypeKind && type.IsEnum() )
         {
            return this.MatchTypeToSignatureNoTypeCode( true, type, sig, ref idx, typeGArgs, methodGArgs );
         }
         else
         {
            switch ( type.GetTypeCode( CILTypeCode.Empty ) )
            {
               case CILTypeCode.Boolean:
                  return sig[idx++] == (Byte) SignatureElementTypes.Boolean;
               case CILTypeCode.Char:
                  return sig[idx++] == (Byte) SignatureElementTypes.Char;
               case CILTypeCode.SByte:
                  return sig[idx++] == (Byte) SignatureElementTypes.I1;
               case CILTypeCode.Byte:
                  return sig[idx++] == (Byte) SignatureElementTypes.U1;
               case CILTypeCode.Int16:
                  return sig[idx++] == (Byte) SignatureElementTypes.I2;
               case CILTypeCode.UInt16:
                  return sig[idx++] == (Byte) SignatureElementTypes.U2;
               case CILTypeCode.Int32:
                  return sig[idx++] == (Byte) SignatureElementTypes.I4;
               case CILTypeCode.UInt32:
                  return sig[idx++] == (Byte) SignatureElementTypes.U4;
               case CILTypeCode.Int64:
                  return sig[idx++] == (Byte) SignatureElementTypes.I8;
               case CILTypeCode.UInt64:
                  return sig[idx++] == (Byte) SignatureElementTypes.U8;
               case CILTypeCode.Single:
                  return sig[idx++] == (Byte) SignatureElementTypes.R4;
               case CILTypeCode.Double:
                  return sig[idx++] == (Byte) SignatureElementTypes.R8;
               case CILTypeCode.String:
                  return sig[idx++] == (Byte) SignatureElementTypes.String;
               case CILTypeCode.Void:
                  return sig[idx++] == (Byte) SignatureElementTypes.Void;
               case CILTypeCode.TypedByRef:
                  return sig[idx++] == (Byte) SignatureElementTypes.TypedByRef;
               case CILTypeCode.IntPtr:
                  return sig[idx++] == (Byte) SignatureElementTypes.I;
               case CILTypeCode.UIntPtr:
                  return sig[idx++] == (Byte) SignatureElementTypes.U;
               case CILTypeCode.SystemObject:
                  return sig[idx++] == (Byte) SignatureElementTypes.Object;
               default:
                  return this.MatchTypeToSignatureNoTypeCode( false, type, sig, ref idx, typeGArgs, methodGArgs );
            }
         }
      }

      private Boolean MatchTypeToSignatureNoTypeCode( Boolean isEnum, CILTypeBase type, Byte[] sig, ref Int32 idx, CILTypeBase[] typeGArgs, CILTypeBase[] methodGArgs )
      {
         Boolean result;
         if ( isEnum || TypeKind.Type == type.TypeKind )
         {
            var tt = (CILType) type;
            if ( tt.IsGenericType() )
            {
               result = sig[idx++] == (Byte) SignatureElementTypes.GenericInst;
               if ( result )
               {
                  result = this.MatchClassOrValueTypeToSignature( isEnum, tt.GenericDefinition, sig, ref idx, typeGArgs, methodGArgs );
                  if ( result )
                  {
                     result = sig.DecompressUInt32( ref idx ) == tt.GenericArguments.Count;
                     if ( result )
                     {
                        var i = 0;
                        while ( result && i < tt.GenericArguments.Count )
                        {
                           result = this.MatchTypeToSignature( tt.GenericArguments[i], sig, ref idx, typeGArgs, methodGArgs );
                           ++i;
                        }
                        result = i == tt.GenericArguments.Count;
                     }
                  }
               }
            }
            else
            {
               var ek = tt.ElementKind;
               if ( ek.HasValue )
               {
                  switch ( ek.Value )
                  {
                     case ElementKind.Array:
                        result = this.MatchArrayTypeToSignature( tt, sig, ref idx, typeGArgs, methodGArgs );
                        break;
                     case ElementKind.Pointer:
                        result = sig[idx++] == (Byte) SignatureElementTypes.Ptr;
                        if ( result )
                        {
                           SkipCustomMods( sig, ref idx );
                           result = this.MatchTypeToSignature( tt.ElementType, sig, ref idx, typeGArgs, methodGArgs );
                        }
                        break;
                     case ElementKind.Reference:
                        result = sig[idx++] == (Byte) SignatureElementTypes.ByRef;
                        if ( result )
                        {
                           result = this.MatchTypeToSignature( tt.ElementType, sig, ref idx, typeGArgs, methodGArgs );
                        }
                        break;
                     default:
                        throw new Exception( "Shouldn't be possible." );
                  }
               }
               else
               {
                  result = this.MatchClassOrValueTypeToSignature( isEnum, tt, sig, ref idx, typeGArgs, methodGArgs );
               }
            }
         }
         else if ( TypeKind.TypeParameter == type.TypeKind )
         {
            result = ( (CILTypeParameter) type ).DeclaringMethod == null ?
               sig[idx++] == (Byte) SignatureElementTypes.Var :
               sig[idx++] == (Byte) SignatureElementTypes.MVar;
            if ( result )
            {
               result = ( (CILTypeParameter) type ).GenericParameterPosition == sig.DecompressUInt32( ref idx );
            }
         }
         else // TypeKind.MethodSignature
         {
            result = sig[idx++] == (Byte) SignatureElementTypes.FnPtr;
            if ( result )
            {
               result = this.MatchMethodSigToSignature( ( (CILMethodSignature) type ), sig, typeGArgs, ref idx );
            }
         }
         return result;
      }

      private Boolean MatchArrayTypeToSignature( CILType type, Byte[] sig, ref Int32 idx, CILTypeBase[] typeGArgs, CILTypeBase[] methodGArgs )
      {
         Boolean result;
         if ( type.IsVectorArray() )
         {
            result = sig[idx++] == (Byte) SignatureElementTypes.SzArray;
            if ( result )
            {
               SkipCustomMods( sig, ref idx );
               result = this.MatchTypeToSignature( type.ElementType, sig, ref idx, typeGArgs, methodGArgs );
            }
         }
         else
         {
            result = sig[idx++] == (Byte) SignatureElementTypes.Array;
            if ( result )
            {
               var arrayInfo = type.ArrayInformation;
               result = this.MatchTypeToSignature( type.ElementType, sig, ref idx, typeGArgs, methodGArgs )
                  && arrayInfo.Rank == sig.DecompressUInt32( ref idx );
               if ( result )
               {
                  // Skip the rest of the info
                  // TODO implement array sizes & min boundaries
                  var cur = sig.DecompressUInt32( ref idx );
                  result = arrayInfo.Sizes.Count == cur;
                  for ( var i = 0; result && i < cur; ++i )
                  {
                     result = arrayInfo.Sizes[i] == sig.DecompressUInt32( ref idx );
                  }
                  if ( result )
                  {
                     cur = sig.DecompressUInt32( ref idx );
                     result = arrayInfo.LowerBounds.Count == cur;
                     for ( var i = 0; result && i < cur; ++i )
                     {
                        result = arrayInfo.LowerBounds[i] == sig.DecompressInt32( ref idx );
                     }
                  }
               }
            }
         }
         return result;
      }

      private Boolean MatchClassOrValueTypeToSignature( Boolean isEnum, CILType type, Byte[] sig, ref Int32 idx, CILTypeBase[] typeGArgs, CILTypeBase[] methodGArgs )
      {
         Boolean result;
         switch ( (SignatureElementTypes) sig[idx++] )
         {
            case SignatureElementTypes.ValueType:
               result = isEnum || type.IsValueType();
               break;
            case SignatureElementTypes.Class:
               result = true;
               break;
            default:
               result = false;
               break;
         }
         if ( result )
         {
            var tIdx = new TableIndex( TokenUtils.DecodeTypeDefOrRefOrSpec( sig, ref idx ) );
            var resolvedType = this.ResolveType( tIdx, typeGArgs, methodGArgs );

            result = type.Equals( resolvedType );
            if ( !result && tIdx.table == Tables.TypeRef && resolvedType.TypeKind == TypeKind.Type && ( (CILType) resolvedType ).IsTrueDefinition )
            {
               var rt = (CILType) resolvedType;
               var tRefRow = this._md.typeRef[tIdx.idx];
               // TODO when tRefRow.Item.HasValue is false, get aRef from exportedType
               if ( tRefRow.Item1.HasValue && tRefRow.Item1.Value.table == Tables.AssemblyRef )
               {
                  var aRef = this._md.assemblyRef[tRefRow.Item1.Value.idx];

                  // Match textually (don't match assembly name for retargetable refs since it might be different for different PCL's, like Func<T,U> is in mscorlib.dll in some PCL's and in System.Core.dll in some other PCL's.
                  result = ( aRef.Item5.IsRetargetable()
                     || String.Equals( type.Module.Assembly.Name.Name, resolvedType.Module.Assembly.Name.Name ) )
                     && String.Equals( rt.Namespace, type.Namespace )
                     && String.Equals( rt.Name, type.Name );
                  //}
               }
            }
         }
         return result;
      }

      private const Int32 FORMAT_MASK = 0x00000001;
      private const Int32 FLAG_MASK = 0x00000FFF;
      private const UInt32 SEC_SIZE_MASK = 0xFFFFFF00;
      private const UInt32 SEC_FLAG_MASK = 0x000000FF;
      private static readonly TExceptionBlockInfo[] EMPTY_EXCEPTION_BLOCKS = new TExceptionBlockInfo[0];

      internal static TMethodCodeInfo ReadMethodBytes( System.IO.Stream stream )
      {
         var b = (Int32) stream.ReadByteFromStream();
         Byte[] code;
         TableIndex? localSig;
         TExceptionBlockInfo[] exceptionBlocks;
         Boolean initLocals;
         if ( ( FORMAT_MASK & b ) == 0 )
         {
            // Tiny header - no locals, no exceptions, no extra data
            localSig = null;
            code = new Byte[b >> 2];
            stream.ReadWholeArray( code );
            exceptionBlocks = EMPTY_EXCEPTION_BLOCKS;
            initLocals = false;
         }
         else
         {
            stream.SeekFromCurrent( -1 );
            var tmpArray = new Byte[4];
            b = stream.ReadU16( tmpArray );
            var flags = (MethodHeaderFlags) ( b & FLAG_MASK );
            initLocals = ( flags & MethodHeaderFlags.InitLocals ) != 0;
            var headerSize = ( b >> 12 ) * 4; // Header size is written as amount of integers
            // Skip max stack
            stream.SeekFromCurrent( 2 );
            var codeSize = stream.ReadU32( tmpArray );
            var localSigToken = stream.ReadU32( tmpArray );
            if ( localSigToken == 0 )
            {
               localSig = null;
            }
            else
            {
               localSig = new TableIndex( (Int32) localSigToken );
            }

            if ( headerSize != 12 )
            {
               stream.SeekFromCurrent( BitUtils.MultipleOf4( headerSize - 12 ) );
            }

            // Read code
            code = new Byte[codeSize];
            stream.ReadWholeArray( code );
            stream.SeekFromCurrent( BitUtils.MultipleOf4( codeSize ) - codeSize );

            var excList = new List<TExceptionBlockInfo>();
            if ( ( flags & MethodHeaderFlags.MoreSections ) != 0 )
            {
               // Read sections
               MethodDataFlags secFlags;
               do
               {
                  var secHeader = stream.ReadU32( tmpArray );
                  secFlags = (MethodDataFlags) ( secHeader & SEC_FLAG_MASK );
                  var secByteSize = ( secHeader & SEC_SIZE_MASK ) >> 8;
                  secByteSize -= 4;
                  var isFat = ( secFlags & MethodDataFlags.FatFormat ) != 0;
                  while ( secByteSize > 0 )
                  {
                     var eType = (ExceptionBlockType) ( isFat ? stream.ReadU32( tmpArray ) : stream.ReadU16( tmpArray ) );
                     excList.Add( Tuple.Create(
                        eType,
                        isFat ? stream.ReadU32( tmpArray ) : stream.ReadU16( tmpArray ),
                        isFat ? stream.ReadU32( tmpArray ) : stream.ReadByteFromStream(),
                        isFat ? stream.ReadU32( tmpArray ) : stream.ReadU16( tmpArray ),
                        isFat ? stream.ReadU32( tmpArray ) : stream.ReadByteFromStream(),
                        eType == ExceptionBlockType.Filter ? 0u : stream.ReadU32( tmpArray ),
                        eType == ExceptionBlockType.Filter ? stream.ReadU32( tmpArray ) : 0u ) );
                     secByteSize -= ( isFat ? 24u : 12u );
                  }
               } while ( ( secFlags & MethodDataFlags.MoreSections ) != 0 );
            }
            exceptionBlocks = excList.ToArray();
         }
         return Tuple.Create( initLocals, localSig, code, exceptionBlocks );
      }

      private static ModuleKind GetModuleKind( UInt16 characteristics, UInt16 subsystem, Int32 assemblyTableRowCount )
      {
         if ( ( characteristics & HeaderFieldPossibleValues.IMAGE_FILE_DLL ) != 0 )
         {
            return assemblyTableRowCount > 0 ? ModuleKind.Dll : ModuleKind.NetModule;
         }
         else
         {
            if ( subsystem == HeaderFieldPossibleValues.IMAGE_SUBSYSTEM_WINDOWS_GUI || subsystem == HeaderFieldPossibleValues.IMAGE_SUBSYSTEM_WINDOWS_CE_GUI )
            {
               return ModuleKind.Windows;
            }
            else
            {
               return ModuleKind.Console;
            }
         }
      }

      private const String PROFILE_PREFIX = "Profile=";
      private const String VERSION_PREFIX = "Version=";
      private const Char SEPARATOR = ',';
      internal static void ParseFWStr( String str, out String fwName, out String fwVersion, out String fwProfile )
      {
         // First, framework name
         var idx = str.IndexOf( SEPARATOR );
         fwName = idx == -1 ? str : str.Substring( 0, idx );

         // Then, framework version
         idx = str.IndexOf( VERSION_PREFIX, StringComparison.Ordinal );
         var nextIdx = idx + VERSION_PREFIX.Length;
         var endIdx = str.IndexOf( SEPARATOR, nextIdx );
         if ( endIdx == -1 )
         {
            endIdx = str.Length;
         }
         fwVersion = idx != -1 && nextIdx < str.Length ? str.Substring( nextIdx, endIdx - nextIdx ) : null;

         // Then, profile
         idx = str.IndexOf( PROFILE_PREFIX, StringComparison.Ordinal );
         nextIdx = idx + PROFILE_PREFIX.Length;
         endIdx = str.IndexOf( SEPARATOR, nextIdx );
         if ( endIdx == -1 )
         {
            endIdx = str.Length;
         }
         fwProfile = idx != -1 && nextIdx < str.Length ? str.Substring( nextIdx, endIdx - nextIdx ) : null;
      }
   }
}