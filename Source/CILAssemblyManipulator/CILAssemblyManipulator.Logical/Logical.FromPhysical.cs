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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CILAssemblyManipulator.Logical
{
   /// <summary>
   /// This class represents the "promise of" Logical layer <see cref="CILAssembly"/> created from Physical layer <see cref="CILMetaData"/>.
   /// Access the <see cref="LogicalAssemblyCreationResult.Assembly"/> property to acquire actual <see cref="CILAssembly"/>.
   /// </summary>
   public sealed class LogicalAssemblyCreationResult
   {
      private readonly CILAssembly _assemblyInstance;
      private readonly Lazy<CILAssembly> _assembly;
      private readonly IDictionary<String, CILType> _typeDefs;
      private readonly IDictionary<CILType, KeyValuePair<String, Int32>> _typeDefInfos;

      // TODO list: method signatures, type spec signatures
      private readonly IDictionary<CILField, FieldSignature> _fieldSignatures;
      private readonly IDictionary<CILMethodBase, MethodDefinitionSignature> _methodDefSignatures;

      private readonly IDictionary<Int32, Int32> _declaringTypes;
      private readonly TypeDefinition[] _typeDefArray;
      private readonly TypeReference[] _typeRefs;
      private readonly TypeSignature[] _typeSpecSignatures;
      private readonly AssemblyReference[] _assemblyRefs;
      private readonly String[] _moduleRefs;

      private readonly Object _lock;
      private Action _basicStructurePopulator;
      private Action _complexStructurePopulator;

      internal LogicalAssemblyCreationResult(
         CILMetaData md,
         CILAssembly assembly,
         Action basicStructurePopulator,
         Action complexStructurePopulator
         )
      {
         ArgumentValidator.ValidateNotNull( "MetaData", md );
         ArgumentValidator.ValidateNotNull( "Assembly", assembly );
         ArgumentValidator.ValidateNotNull( "Basic structure populator", basicStructurePopulator );
         ArgumentValidator.ValidateNotNull( "Complex structure populator", complexStructurePopulator );

         this._assemblyInstance = assembly;
         this._assembly = new Lazy<CILAssembly>( () =>
         {
            this.PopulateComplexStructure();
            return this._assemblyInstance;
         }, assembly.ReflectionContext.GetLazyThreadSafetyMode() );
         this._basicStructurePopulator = basicStructurePopulator;
         this._complexStructurePopulator = complexStructurePopulator;
         this._lock = new Object();
         this._typeDefs = new Dictionary<String, CILType>();
         this._typeDefInfos = new Dictionary<CILType, KeyValuePair<String, Int32>>();

         this._declaringTypes = md.NestedClassDefinitions.TableContents.ToDictionary_Overwrite( nc => nc.NestedClass.Index, nc => nc.EnclosingClass.Index );
         this._fieldSignatures = new Dictionary<CILField, FieldSignature>();
         this._methodDefSignatures = new Dictionary<CILMethodBase, MethodDefinitionSignature>();
         // TODO .ToArray<T,U>(Func<T,U> selector) to UtilPack
         this._typeDefArray = md.TypeDefinitions.TableContents.ToArray();
         this._typeRefs = md.TypeReferences.TableContents.ToArray();
         this._typeSpecSignatures = md.TypeSpecifications.TableContents.Select( t => t.Signature ).ToArray();
         this._assemblyRefs = md.AssemblyReferences.TableContents.ToArray();
         this._moduleRefs = md.ModuleReferences.TableContents.Select( m => m.ModuleName ).ToArray();
      }

      /// <summary>
      /// Lazily initializes and gets the <see cref="CILAssembly"/> 
      /// </summary>
      public CILAssembly Assembly
      {
         get
         {
            return this._assembly.Value;
         }
      }

      internal CILType ResolveTopLevelType( String ns, String tn, Boolean throwOnError )
      {
         return this.ResolveTypeString( Miscellaneous.CombineNamespaceAndType( ns, tn ), throwOnError );
      }

      internal CILType ResolveTypeString( String typeString, Boolean throwOnError )
      {
         // TODO maybe use exported types here too?
         CILType retVal;
         if ( !this._typeDefs.TryGetValue( typeString, out retVal ) && throwOnError )
         {
            throw new InvalidOperationException( "Failed to resolve type \"" + typeString + "\"." );
         }
         return retVal;
      }

      internal String ResolveType( CILType type )
      {
         return this.ResolveTypeInfo( type ).Key;
      }

      internal KeyValuePair<String, Int32> ResolveTypeInfo( CILType type )
      {
         KeyValuePair<String, Int32> retVal;
         if ( !this._typeDefInfos.TryGetValue( type, out retVal ) )
         {
            throw new InvalidOperationException( "Failed to get type string for type " );
         }
         return retVal;
      }

      internal void RecordTypeDef(
         CILType type,
         String typeString,
         CILMetaData md,
         Int32 tDefIndex
         )
      {
         var tDef = md.TypeDefinitions.TableContents[tDefIndex];
         this._typeDefs.Add( typeString, type );
         this._typeDefInfos.Add( type, new KeyValuePair<String, Int32>( typeString, tDefIndex ) );
      }

      internal void RecordFieldDef( CILField field, FieldSignature sig )
      {
         this._fieldSignatures.Add( field, sig );
      }

      internal void RecordMethodDef( CILMethodBase method, MethodDefinitionSignature sig )
      {
         this._methodDefSignatures.Add( method, sig );
      }

      internal FieldSignature GetFieldSignature( CILField field )
      {
         return this._fieldSignatures[field];
      }

      internal MethodDefinitionSignature GetMethodSignature( CILMethodBase method )
      {
         return this._methodDefSignatures[method];
      }

      internal TypeSignature GetTypeSpecSignature( Int32 index )
      {
         return this._typeSpecSignatures[index];
      }

      internal TypeReference GetTypeReference( Int32 index )
      {
         return this._typeRefs[index];
      }

      internal AssemblyReference GetAssemblyReference( Int32 index )
      {
         return this._assemblyRefs[index];
      }

      internal String GetModuleReference( Int32 index )
      {
         return this._moduleRefs[index];
      }

      internal Boolean TryGetDeclaringType( Int32 nestedIndex, out Int32 declaringIndex )
      {
         return this._declaringTypes.TryGetValue( nestedIndex, out declaringIndex );
      }

      internal TypeDefinition GetAnyTypeDef( Int32 index )
      {
         return this._typeDefArray[index];
      }

      internal CILAssembly AssemblyInstance
      {
         get
         {
            return this._assemblyInstance;
         }
      }

      internal void PopulateBasicStructure()
      {
         this.RunActionWithLock( ref this._basicStructurePopulator );
      }

      internal void PopulateComplexStructure()
      {
         this.PopulateBasicStructure();
         this.RunActionWithLock( ref this._complexStructurePopulator );
      }

      private void RunActionWithLock( ref Action action )
      {
         if ( action != null )
         {
            lock ( this._lock )
            {
               if ( action != null )
               {
                  var actionVar = action;
                  Interlocked.Exchange( ref action, null );
                  actionVar();
               }
            }
         }
      }
   }
}

public static partial class E_CILLogical
{
   private sealed class LogicalCreationState
   {
      private struct TypeSpecInfo : IEquatable<TypeSpecInfo>
      {
         private readonly Int32 _idx;
         private readonly CILType _contextType;
         private readonly CILMethodBase _contextMethod;
         private readonly Boolean _populateAssemblyRefSimpleStructure;

         public TypeSpecInfo( Int32 idx, CILType contextType, CILMethodBase contextMethod, Boolean populateAssemblyRefSimpleStructure )
         {
            this._idx = idx;
            this._contextType = contextType;
            this._contextMethod = contextMethod;
            this._populateAssemblyRefSimpleStructure = populateAssemblyRefSimpleStructure;
         }

         public override Boolean Equals( Object obj )
         {
            return obj is TypeSpecInfo && this.Equals( (TypeSpecInfo) obj );
         }

         public override Int32 GetHashCode()
         {
            return ( ( 17 * 23 + this._idx ) * 23 + this._contextType.GetHashCodeSafe() ) * 23 + this._contextMethod.GetHashCodeSafe();
         }

         public Boolean Equals( TypeSpecInfo other )
         {
            return this._idx == other._idx
               && Equals( this._contextType, other._contextType )
               && Equals( this._contextMethod, other._contextMethod )
               && this._populateAssemblyRefSimpleStructure == other._populateAssemblyRefSimpleStructure;
         }

         public Int32 Index
         {
            get
            {
               return this._idx;
            }
         }

         public CILType ContextType
         {
            get
            {
               return this._contextType;
            }
         }

         public CILMethodBase ContextMethod
         {
            get
            {
               return this._contextMethod;
            }
         }

         public Boolean PopulateAssemblyRefSimpleStructure
         {
            get
            {
               return this._populateAssemblyRefSimpleStructure;
            }
         }
      }

      private struct ContextualInfo : IEquatable<ContextualInfo>
      {
         private readonly Int32 _idx;
         private readonly CILType _contextType;
         private readonly CILMethodBase _contextMethod;

         public ContextualInfo( Int32 idx, CILType contextType, CILMethodBase contextMethod )
         {
            this._idx = idx;
            this._contextType = contextType;
            this._contextMethod = contextMethod;
         }

         public override Boolean Equals( Object obj )
         {
            return obj is ContextualInfo && this.Equals( (ContextualInfo) obj );
         }

         public override Int32 GetHashCode()
         {
            return ( ( 17 * 23 + this._idx ) * 23 + this._contextType.GetHashCodeSafe() ) * 23 + this._contextMethod.GetHashCodeSafe();
         }

         public Boolean Equals( ContextualInfo other )
         {
            return this._idx == other._idx
               && Equals( this._contextType, other._contextType )
               && Equals( this._contextMethod, other._contextMethod );
         }

         public Int32 Index
         {
            get
            {
               return this._idx;
            }
         }

         public CILType ContextType
         {
            get
            {
               return this._contextType;
            }
         }

         public CILMethodBase ContextMethod
         {
            get
            {
               return this._contextMethod;
            }
         }
      }

      private readonly IDictionary<String, LogicalCreationState> _allModuleStates;
      private readonly LogicalAssemblyCreationResult _creationResult;
      private readonly CILModule _module;
      private readonly CILMetaData _md;
      private readonly Func<CILAssemblyName, LogicalAssemblyCreationResult> _assemblyRefResolver;

      private readonly CILType[] _typeDefs;
      private readonly CILField[] _fieldDefs;
      private readonly CILMethodBase[] _methodDefs;
      private readonly CILParameter[] _parameterDefs;
      private readonly CILTypeParameter[] _typeParameters;
      private readonly CILProperty[] _properties;
      private readonly CILEvent[] _events;
      private readonly CILAssemblyName[] _assemblyRefs;
      private readonly IDictionary<CILAssemblyName, LogicalAssemblyCreationResult> _assemblyRefsByName;

      private readonly IDictionary<Int32, ISet<Int32>> _nestedTypes;

      private readonly IDictionary<SignatureElementTypes, CILType> _simpleTypes;
      private readonly Tuple<CILType, LogicalAssemblyCreationResult>[] _typeRefs; // 1: resolved type, 2: AssemblyRef or null
      private readonly IDictionary<TypeSpecInfo, CILTypeBase> _typeSpecs;
      private readonly IDictionary<ContextualInfo, IEnumerable<Tuple<CILTypeBase, Boolean>>> _locals;
      private readonly IDictionary<ContextualInfo, CILMethod> _methodSpecs;

      private readonly Lazy<LogicalAssemblyCreationResult> _associatedMSCorLib;

      internal LogicalCreationState(
         IDictionary<String, LogicalCreationState> allModuleStates,
         LogicalAssemblyCreationResult creationResult,
         CILModule module,
         CILMetaData md,
         Func<CILAssemblyName, LogicalAssemblyCreationResult> assemblyRefResolver,
         LogicalAssemblyCreationResult msCorLibOverride
         )
      {
         this._allModuleStates = allModuleStates;
         this._creationResult = creationResult;
         this._module = module;
         this._md = md;
         this._assemblyRefResolver = assemblyRefResolver;

         var tDefCount = md.TypeDefinitions.RowCount;

         this._typeDefs = PopulateWithNulls<CILType>( tDefCount );
         this._fieldDefs = PopulateWithNulls<CILField>( md.FieldDefinitions.RowCount );
         this._methodDefs = PopulateWithNulls<CILMethodBase>( md.MethodDefinitions.RowCount );
         this._parameterDefs = PopulateWithNulls<CILParameter>( md.ParameterDefinitions.RowCount );
         this._typeParameters = PopulateWithNulls<CILTypeParameter>( md.GenericParameterDefinitions.RowCount );
         this._typeRefs = PopulateWithNulls<Tuple<CILType, LogicalAssemblyCreationResult>>( md.TypeReferences.RowCount );
         this._properties = PopulateWithNulls<CILProperty>( md.PropertyDefinitions.RowCount );
         this._events = PopulateWithNulls<CILEvent>( md.EventDefinitions.RowCount );
         this._assemblyRefs = PopulateWithNulls<CILAssemblyName>( md.AssemblyReferences.RowCount );

         this._assemblyRefsByName = new Dictionary<CILAssemblyName, LogicalAssemblyCreationResult>( module.ReflectionContext.DefaultAssemblyNameComparer );

         var nestedTypes = new Dictionary<Int32, ISet<Int32>>();
         foreach ( var nc in md.NestedClassDefinitions.TableContents )
         {
            nestedTypes
               .GetOrAdd_NotThreadSafe( nc.EnclosingClass.Index, i => new HashSet<Int32>() )
               .Add( nc.NestedClass.Index );
         }
         this._nestedTypes = nestedTypes;

         this._simpleTypes = new Dictionary<SignatureElementTypes, CILType>();
         this._typeSpecs = new Dictionary<TypeSpecInfo, CILTypeBase>();
         this._locals = new Dictionary<ContextualInfo, IEnumerable<Tuple<CILTypeBase, Boolean>>>();
         this._methodSpecs = new Dictionary<ContextualInfo, CILMethod>();

         this._associatedMSCorLib = new Lazy<LogicalAssemblyCreationResult>( () => this.ResolveMSCorLibModule( msCorLibOverride ), LazyThreadSafetyMode.None );
      }

      public CILModule Module
      {
         get
         {
            return this._module;
         }
      }

      public CILMetaData MetaData
      {
         get
         {
            return this._md;
         }
      }

      public IDictionary<Int32, ISet<Int32>> NestedTypes
      {
         get
         {
            return this._nestedTypes;
         }
      }

      public LogicalAssemblyCreationResult AssociatedMSCorLibModule
      {
         get
         {
            return this._associatedMSCorLib.Value;
         }
      }

      public LogicalAssemblyCreationResult CreationResult
      {
         get
         {
            return this._creationResult;
         }
      }

      internal CILType GetTypeDef( Int32 typeDefIndex )
      {
         return this._typeDefs[typeDefIndex];
      }

      internal CILMethodBase GetMethodDef( Int32 methodDefIndex )
      {
         return this._methodDefs[methodDefIndex];
      }

      internal CILParameter GetParameterDef( Int32 parameterIndex )
      {
         return this._parameterDefs[parameterIndex];
      }

      internal CILField GetFieldDef( Int32 fieldDefIndex )
      {
         return this._fieldDefs[fieldDefIndex];
      }

      internal CILTypeParameter GetTypeParameter( Int32 tParamIndex )
      {
         return this._typeParameters[tParamIndex];
      }

      internal CILProperty GetProperty( Int32 propDefIndex )
      {
         return this._properties[propDefIndex];
      }

      internal CILEvent GetEvent( Int32 eventDefIndex )
      {
         return this._events[eventDefIndex];
      }

      internal void RecordTypeDef(
         CILType type,
         Int32 typeDefIndex
         )
      {
         this._typeDefs[typeDefIndex] = type;

         var typeString = this._creationResult.IsTopLevelType( typeDefIndex ) ?
            Miscellaneous.CombineNamespaceAndType( type.Namespace, type.Name ) :
            Miscellaneous.CombineEnclosingAndNestedType( this._creationResult.ResolveType( type.DeclaringType ), type.Name );
         this._creationResult.RecordTypeDef( type, typeString, this._md, typeDefIndex );
      }

      internal void RecordFieldDef( CILField field, Int32 fieldDefIndex )
      {
         this._fieldDefs[fieldDefIndex] = field;
         this._creationResult.RecordFieldDef( field, this._md.FieldDefinitions.TableContents[fieldDefIndex].Signature );
      }

      internal void RecordMethodDef( CILMethodBase method, Int32 methodDefIndex )
      {
         this._methodDefs[methodDefIndex] = method;
         this._creationResult.RecordMethodDef( method, this._md.MethodDefinitions.TableContents[methodDefIndex].Signature );
      }

      internal void RecordParameter( CILParameter parameter, Int32 paramDefIndex )
      {
         this._parameterDefs[paramDefIndex] = parameter;
      }

      internal void RecordTypeParameter( CILTypeParameter parameter, Int32 gParamIndex )
      {
         this._typeParameters[gParamIndex] = parameter;
      }

      internal void RecordProperty( CILProperty property, Int32 propDefIndex )
      {
         this._properties[propDefIndex] = property;
      }

      internal void RecordEvent( CILEvent evt, Int32 evtDefIndex )
      {
         this._events[evtDefIndex] = evt;
      }

      internal CILTypeBase ResolveTypeDefOrRefOrSpec( TableIndex index, CILType contextType, CILMethodBase contextMethod, Boolean populateAssemblyRefStructure = false )
      {
         switch ( index.Table )
         {
            case Tables.TypeDef:
               return this._typeDefs[index.Index];
            case Tables.TypeRef:
               var retVal = this._typeRefs.GetOrAdd_NotThreadSafe( index.Index, i => this.ResolveTypeRef( i, populateAssemblyRefStructure ) );
               if ( populateAssemblyRefStructure && retVal.Item2 != null )
               {
                  // In case on first time we resolved this type ref, the populate parameter was false.
                  retVal.Item2.PopulateBasicStructure();
               }
               return retVal.Item1;
            case Tables.TypeSpec:
               return this.ResolveTypeSpec( index.Index, contextType, contextMethod, populateAssemblyRefStructure );
            default:
               throw new InvalidOperationException( "Unexpected TypeDef/Ref/Spec: " + index + "." );
         }
      }

      internal CILTypeBase ResolveTypeSignature( TypeSignature sig, CILType contextType, CILMethodBase contextMethod, Boolean populateAssemblyRefStructure = false )
      {
         switch ( sig.TypeSignatureKind )
         {
            case TypeSignatureKind.ClassOrValue:
               var clazz = (ClassOrValueTypeSignature) sig;
               var cilClazz = this.ResolveTypeDefOrRefOrSpec( clazz.Type, contextType, contextMethod, populateAssemblyRefStructure );
               if ( clazz.GenericArguments.Count > 0 )
               {
                  cilClazz = ( (CILType) cilClazz ).MakeGenericType( clazz.GenericArguments
                     .Select( arg => this.ResolveTypeSignature( arg, contextType, contextMethod, populateAssemblyRefStructure ) )
                     .ToArray()
                     );
               }
               return cilClazz;
            case TypeSignatureKind.ComplexArray:
               var array = (ComplexArrayTypeSignature) sig;
               return this.ResolveTypeSignature( array.ArrayType, contextType, contextMethod, populateAssemblyRefStructure )
                  .MakeArrayType( array.Rank, array.Sizes.ToArray(), array.LowerBounds.ToArray() );
            case TypeSignatureKind.FunctionPointer:
               var fn = ( (FunctionPointerTypeSignature) sig ).MethodSignature;
               return this._module.ReflectionContext.NewMethodSignature(
                  this._module,
                  (UnmanagedCallingConventions) fn.SignatureStarter,
                  this.ResolveParamSignature( fn.ReturnType, contextType, contextMethod, populateAssemblyRefStructure ),
                  fn.ReturnType.CustomModifiers.Select( cm => CILCustomModifierFactory.CreateModifier( cm.IsOptional, (CILType) this.ResolveTypeDefOrRefOrSpec( cm.CustomModifierType, contextType, contextMethod, populateAssemblyRefStructure ) ) ).ToArray(),
                  fn.Parameters.Select( p => Tuple.Create( p.CustomModifiers.Select( cm => CILCustomModifierFactory.CreateModifier( cm.IsOptional, (CILType) this.ResolveTypeDefOrRefOrSpec( cm.CustomModifierType, contextType, contextMethod, populateAssemblyRefStructure ) ) ).ToArray(), this.ResolveParamSignature( p, contextType, contextMethod ) ) ).ToArray()
                  );
            case TypeSignatureKind.GenericParameter:
               var gSig = (GenericParameterTypeSignature) sig;
               if ( gSig.IsTypeParameter )
               {
                  if ( contextType == null )
                  {
                     throw new InvalidOperationException( "Type generic parameter signature present, but no type is currently in context." );
                  }
                  else
                  {
                     return contextType.GenericArguments[gSig.GenericParameterIndex];
                  }
               }
               else
               {
                  if ( contextMethod == null )
                  {
                     throw new InvalidOperationException( "Method generic parameter signature present, but no method is currently in context." );
                  }
                  else
                  {
                     var m = contextMethod as CILMethod;
                     if ( m == null )
                     {
                        throw new InvalidOperationException( "Method generic parameter signature present, but method does not have generic parameters." );
                     }
                     else
                     {
                        return m.GenericArguments[gSig.GenericParameterIndex];
                     }
                  }
               }
            case TypeSignatureKind.Pointer:
               return this.ResolveTypeSignature( ( (PointerTypeSignature) sig ).PointerType, contextType, contextMethod, populateAssemblyRefStructure ).MakePointerType();
            case TypeSignatureKind.Simple:
               return this.ResolveSimpleType( ( (SimpleTypeSignature) sig ).SimpleType );
            case TypeSignatureKind.SimpleArray:
               return this.ResolveTypeSignature( ( (SimpleArrayTypeSignature) sig ).ArrayType, contextType, contextMethod, populateAssemblyRefStructure ).MakeArrayType();
            default:
               throw new InvalidOperationException( "Unrecognized type signature kind: " + sig.TypeSignatureKind + "." );
         }
      }

      internal CILTypeBase ResolveParamSignature( ParameterOrLocalVariableSignature sig, CILType contextType, CILMethodBase contextMethod, Boolean populateAssemblyRefStructure = false )
      {
         var retVal = this.ResolveTypeSignature( sig.Type, contextType, contextMethod, populateAssemblyRefStructure );
         if ( sig.IsByRef )
         {
            retVal = retVal.MakeByRefType();
         }

         return retVal;
      }

      internal CILType ResolveTypeString( String typeString, Boolean populateTargetStructure )
      {
         CILType retVal;
         if ( String.IsNullOrEmpty( typeString ) )
         {
            retVal = null;
         }
         else
         {
            String type, assembly;
            LogicalAssemblyCreationResult targetAssembly;
            if ( typeString.ParseAssemblyQualifiedTypeString( out type, out assembly ) )
            {
               CILAssemblyName aName;
               if ( CILAssemblyName.TryParse( assembly, out aName ) )
               {
                  targetAssembly = this.ResolveAssemblyRef( aName );
                  if ( populateTargetStructure )
                  {
                     targetAssembly.PopulateBasicStructure();
                  }
               }
               else
               {
                  throw new InvalidOperationException( "Unparseable assembly name: \"" + assembly + "\"." );
               }
            }
            else
            {
               targetAssembly = this._creationResult;
            }

            // TODO maybe use exported types here too?
            retVal = targetAssembly.ResolveTypeString( type.UnescapeCILTypeString(), true );
         }

         return retVal;
      }

      internal void AddCustomModifiers( CILElementWithCustomModifiers element, List<CustomModifierSignature> mods, CILType contextType, CILMethodBase contextMethod )
      {
         foreach ( var mod in mods )
         {
            element.AddCustomModifier( this.ResolveTypeDefOrRefOrSpec( mod.CustomModifierType, contextType, contextMethod ) as CILType, mod.IsOptional );
         }
      }

      private LogicalAssemblyCreationResult ResolveAssemblyCreationResult( TableIndex typeDefOrRefOrSpec )
      {
         switch ( typeDefOrRefOrSpec.Table )
         {
            case Tables.TypeDef:
               return this._creationResult;
            case Tables.TypeRef:
               return this._typeRefs[typeDefOrRefOrSpec.Index].Item2;
            case Tables.TypeSpec:
               return this.GetCreationResultFromTypeSignature( this._md.TypeSpecifications.TableContents[typeDefOrRefOrSpec.Index].Signature );
            default:
               throw new InvalidOperationException( "Unsupported table for TypeDefOrRefOrSpec: " + typeDefOrRefOrSpec.Table + "." );
         }
      }

      private LogicalAssemblyCreationResult GetCreationResultFromTypeSignature( TypeSignature typeSig )
      {
         switch ( typeSig.TypeSignatureKind )
         {
            case TypeSignatureKind.ClassOrValue:
               return this.ResolveAssemblyCreationResult( ( (ClassOrValueTypeSignature) typeSig ).Type );
            case TypeSignatureKind.ComplexArray:
               return this.GetCreationResultFromTypeSignature( ( (ComplexArrayTypeSignature) typeSig ).ArrayType );
            case TypeSignatureKind.FunctionPointer:
            case TypeSignatureKind.GenericParameter:
            case TypeSignatureKind.Simple:
               return this._creationResult;
            case TypeSignatureKind.Pointer:
               return this.GetCreationResultFromTypeSignature( ( (PointerTypeSignature) typeSig ).PointerType );
            case TypeSignatureKind.SimpleArray:
               return this.GetCreationResultFromTypeSignature( ( (SimpleArrayTypeSignature) typeSig ).ArrayType );
            default:
               throw new InvalidOperationException( "Invalid type signature kind: " + typeSig.TypeSignatureKind + "." );
         }
      }

      internal CILElementTokenizableInILCode ResolveMemberRef( Int32 index, CILType contextType, CILMethodBase contextMethod, Boolean? shouldBeMethod = null )
      {
         var mRef = this._md.MemberReferences.TableContents[index];
         var declType = mRef.DeclaringType;
         CILType cilDeclType;
         LogicalAssemblyCreationResult declTypeCreationResult;
         switch ( declType.Table )
         {
            case Tables.TypeDef:
            case Tables.TypeRef:
            case Tables.TypeSpec:
               cilDeclType = (CILType) this.ResolveTypeDefOrRefOrSpec( declType, contextType, contextMethod, true );
               declTypeCreationResult = this.ResolveAssemblyCreationResult( declType );
               break;
            case Tables.MethodDef:
               throw new NotImplementedException( "References to global methods/fields in other modules." );
            case Tables.ModuleRef:
               var resolvedModule = this.ResolveModuleRef( declType.Index );
               cilDeclType = resolvedModule.Module.ModuleInitializer;
               declTypeCreationResult = resolvedModule.CreationResult;
               break;
            default:
               throw new InvalidOperationException( "Unsupported member ref declaring type: " + declType + "." );
         }

         var sig = mRef.Signature;
         Boolean wasMethod;
         CILElementTokenizableInILCode retVal;
         var declIsGeneric = cilDeclType.IsGenericType();
         var declTypeToUse = declIsGeneric ? cilDeclType.GenericDefinition : cilDeclType;
         //var declTypeIndex = declTypeCreationResult.ResolveTypeString
         var isSameModule = ReferenceEquals( this._creationResult, declTypeCreationResult );
         switch ( sig.SignatureKind )
         {
            case SignatureKind.Field:
               wasMethod = true;
               var fSig = (FieldSignature) sig;
               var suitableFields = declTypeToUse.DeclaredFields.Where( f =>
                  String.Equals( f.Name, mRef.Name ) );
               suitableFields = isSameModule ?
                  suitableFields.Where( f => Comparers.FieldSignatureEqualityComparer.Equals( fSig, declTypeCreationResult.GetFieldSignature( f ) ) ) :
                  suitableFields.Where( f => this.MatchFieldSignatures( fSig, declTypeCreationResult, declTypeCreationResult.GetFieldSignature( f ) ) );

               var field = suitableFields.FirstOrDefault();
               if ( field != null && declIsGeneric )
               {
                  field = field.ChangeDeclaringType( cilDeclType.GenericArguments.ToArray() );
               }
               retVal = field;
               break;
            case SignatureKind.MethodDefinition:
            case SignatureKind.MethodReference:
               wasMethod = true;
               var mSig = (MethodReferenceSignature) mRef.Signature;
               var isCtor = String.Equals( mRef.Name, Miscellaneous.CLASS_CTOR_NAME ) || String.Equals( mRef.Name, Miscellaneous.INSTANCE_CTOR_NAME );
               var suitableMethods = ( isCtor ? (IEnumerable<CILMethodBase>) declTypeToUse.Constructors : declTypeToUse.DeclaredMethods.Where( m => String.Equals( mRef.Name, m.Name ) ) );

               suitableMethods = declTypeToUse.ElementKind.HasValue ?
                  suitableMethods.Where( m => m.Parameters.Count == mSig.Parameters.Count ) : // We would need to create a physical signature here for perfect match - a bit too complicated, especially since each method has unique name, and all constructors have variable amount of parameters
                  ( isSameModule ?
                     suitableMethods.Where( m => Comparers.AbstractMethodSignatureEqualityComparer_IgnoreKind.Equals( mSig, declTypeCreationResult.GetMethodSignature( m ) ) ) :
                     suitableMethods.Where( m => this.MatchMethodSignatures( mSig, declTypeCreationResult, declTypeCreationResult.GetMethodSignature( m ) ) )
                  );

               var method = suitableMethods.FirstOrDefault();
               if ( method != null && declIsGeneric )
               {
                  method = method.ChangeDeclaringTypeUT( cilDeclType.GenericArguments.ToArray() );
               }
               retVal = method;
               break;
            default:
               throw new InvalidOperationException( "Unsupported member ref signature: " + sig.SignatureKind + "." );
         }

         if ( retVal == null )
         {
            throw new InvalidOperationException( "Failed to resolve member ref at zero-based index " + index + "." );
         }
         else if ( shouldBeMethod.IsTrue() && !wasMethod )
         {
            throw new InvalidOperationException( "Expected method from member reference at zero-based index " + index + "." );
         }

         return retVal;
      }

      internal CILMethodBase ResolveMethodDefOrMemberRef( TableIndex index, CILType contextType, CILMethodBase contextMethod )
      {
         switch ( index.Table )
         {
            case Tables.MethodDef:
               return this.GetMethodDef( index.Index );
            case Tables.MemberRef:
               return (CILMethodBase) this.ResolveMemberRef( index.Index, contextType, contextMethod, true );
            default:
               throw new InvalidOperationException( "Unsupported method def or member ref index: " + index + "." );
         }
      }

      internal CILType ResolveSimpleType( SignatureElementTypes sigType )
      {
         return this._simpleTypes.GetOrAdd_NotThreadSafe( sigType, st =>
         {
            String typeStr;
            switch ( sigType )
            {
               case SignatureElementTypes.Boolean:
                  typeStr = Consts.BOOLEAN;
                  break;
               case SignatureElementTypes.Char:
                  typeStr = Consts.CHAR;
                  break;
               case SignatureElementTypes.I1:
                  typeStr = Consts.SBYTE;
                  break;
               case SignatureElementTypes.U1:
                  typeStr = Consts.BYTE;
                  break;
               case SignatureElementTypes.I2:
                  typeStr = Consts.INT16;
                  break;
               case SignatureElementTypes.U2:
                  typeStr = Consts.UINT16;
                  break;
               case SignatureElementTypes.I4:
                  typeStr = Consts.INT32;
                  break;
               case SignatureElementTypes.U4:
                  typeStr = Consts.UINT32;
                  break;
               case SignatureElementTypes.I8:
                  typeStr = Consts.INT64;
                  break;
               case SignatureElementTypes.U8:
                  typeStr = Consts.UINT64;
                  break;
               case SignatureElementTypes.I:
                  typeStr = Consts.INT_PTR;
                  break;
               case SignatureElementTypes.U:
                  typeStr = Consts.UINT_PTR;
                  break;
               case SignatureElementTypes.R4:
                  typeStr = Consts.SINGLE;
                  break;
               case SignatureElementTypes.R8:
                  typeStr = Consts.DOUBLE;
                  break;
               case SignatureElementTypes.String:
                  typeStr = Consts.STRING;
                  break;
               case SignatureElementTypes.Void:
                  typeStr = Consts.VOID;
                  break;
               case SignatureElementTypes.Object:
                  typeStr = Consts.OBJECT;
                  break;
               case SignatureElementTypes.TypedByRef:
                  typeStr = Consts.TYPED_BY_REF;
                  break;
               case SignatureElementTypes.Type:
                  typeStr = Consts.TYPE;
                  break;
               default:
                  throw new InvalidOperationException( "Unsupported primitive type: " + sigType + "." );
            }

            return this._associatedMSCorLib.Value.ResolveTypeString( typeStr, true );
         } );
      }

      internal CILElementTokenizableInILCode ResolveToken( TableIndex index, CILType contextType, CILMethodBase contextMethod )
      {
         switch ( index.Table )
         {
            case Tables.TypeDef:
            case Tables.TypeRef:
            case Tables.TypeSpec:
               return this.ResolveTypeDefOrRefOrSpec( index, contextType, contextMethod );
            case Tables.MethodDef:
               return this.GetMethodDef( index.Index );
            case Tables.Field:
               return this.GetFieldDef( index.Index );
            case Tables.MemberRef:
               return this.ResolveMemberRef( index.Index, contextType, contextMethod );
            case Tables.MethodSpec:
               return this._methodSpecs.GetOrAdd_NotThreadSafe( new ContextualInfo( index.Index, contextType, contextMethod ), i =>
               {
                  var mSpec = this._md.MethodSpecifications.TableContents[i.Index];
                  var retVal = this.ResolveMethodDefOrMemberRef( mSpec.Method, i.ContextType, i.ContextMethod ) as CILMethod;
                  if ( retVal == null )
                  {
                     throw new InvalidOperationException( "Token resolved to constructor with generic arguments (" + index + ")." );
                  }
                  else
                  {
                     retVal = retVal.MakeGenericMethod( mSpec.Signature.GenericArguments.Select( arg => this.ResolveTypeSignature( arg, i.ContextType, i.ContextMethod ) ).ToArray() );
                  }
                  return retVal;
               } );
            case Tables.StandaloneSignature:
               throw new NotImplementedException( "StandaloneSignature as token." );
            default:
               throw new InvalidOperationException( "Unsupported token: " + index + "." );
         }
      }

      internal IEnumerable<Tuple<CILTypeBase, Boolean>> ResolveLocalsSignature( Int32 index, CILType contextType, CILMethodBase contextMethod )
      {
         return this._locals.GetOrAdd_NotThreadSafe( new ContextualInfo( index, contextType, contextMethod ), i =>
         {
            var sig = this._md.StandaloneSignatures.TableContents[i.Index].Signature;
            switch ( sig.SignatureKind )
            {
               case SignatureKind.LocalVariables:
                  return this.DoResolveLocalsSignature( i.Index, i.ContextType, i.ContextMethod, (LocalVariablesSignature) sig );
               default:
                  throw new InvalidOperationException( "Unsupported local variable signature: " + sig.SignatureKind + "." );
            }
         } );
      }

      internal LogicalAssemblyCreationResult ResolveAssemblyRef( Int32 index )
      {
         var aName = this._assemblyRefs.GetOrAdd_NotThreadSafe( index, i =>
         {
            var aRef = this._md.AssemblyReferences.TableContents[i];
            return new CILAssemblyName( aRef.AssemblyInformation, aRef.Attributes.IsFullPublicKey() );
         } );

         return this.ResolveAssemblyRef( aName );
      }

      private LogicalAssemblyCreationResult ResolveAssemblyRef( CILAssemblyName name )
      {
         return this._assemblyRefsByName.GetOrAdd_NotThreadSafe( name, an =>
         {
            return this._assemblyRefResolver( an );
         } );
      }

      private IEnumerable<Tuple<CILTypeBase, Boolean>> DoResolveLocalsSignature( Int32 index, CILType contextType, CILMethodBase contextMethod, LocalVariablesSignature sig )
      {
         foreach ( var local in sig.Locals )
         {
            yield return Tuple.Create( this.ResolveParamSignature( local, contextType, contextMethod ), local.IsPinned );
         }
      }

      private CILTypeBase ResolveTypeSpec( Int32 tSpecIdx, CILType contextType, CILMethodBase contextMethod, Boolean populateAssemblyRefStructure = false )
      {
         return this._typeSpecs.GetOrAdd_NotThreadSafe(
            new TypeSpecInfo( tSpecIdx, contextType, contextMethod, populateAssemblyRefStructure ),
            info => this.ResolveTypeSignature( this._md.TypeSpecifications.TableContents[info.Index].Signature, info.ContextType, info.ContextMethod, info.PopulateAssemblyRefSimpleStructure ) );
      }


      private Tuple<CILType, LogicalAssemblyCreationResult> ResolveTypeRef( Int32 tRefIdx, Boolean populateAssemblyRefStructure )
      {
         var tRef = this._md.TypeReferences.TableContents[tRefIdx];
         var resScopeNullable = tRef.ResolutionScope;
         LogicalAssemblyCreationResult aRefCreationResult = null;
         CILType retVal;
         if ( resScopeNullable.HasValue )
         {
            var resScope = resScopeNullable.Value;
            switch ( resScope.Table )
            {
               case Tables.TypeRef:
                  var tuple = this.ResolveTypeRef( resScope.Index, populateAssemblyRefStructure );
                  aRefCreationResult = tuple.Item2;
                  retVal = tuple.Item1
                     .DeclaredNestedTypes
                     .FirstOrDefault( n => String.Equals( n.Namespace, tRef.Namespace ) && String.Equals( n.Name, tRef.Name ) );
                  break;
               case Tables.AssemblyRef:
                  // TODO type forwarding!!
                  aRefCreationResult = this.ResolveAssemblyRef( resScope.Index );
                  if ( populateAssemblyRefStructure )
                  {
                     aRefCreationResult.PopulateBasicStructure();
                  }
                  retVal = aRefCreationResult.ResolveTopLevelType( tRef.Namespace, tRef.Name, true );
                  break;
               case Tables.Module:
                  retVal = this._creationResult.ResolveTopLevelType( tRef.Namespace, tRef.Name, true );
                  break;
               case Tables.ModuleRef:
                  retVal = this.ResolveModuleRef( resScope.Index ).CreationResult.ResolveTopLevelType( tRef.Namespace, tRef.Name, true );
                  break;
               default:
                  throw new InvalidOperationException( "Unexpected TypeRef resolution scope: " + resScope + "." );
            }
         }
         else
         {
            throw new NotImplementedException( "Null resolution scope." );
         }

         return Tuple.Create( retVal, aRefCreationResult );
      }

      private LogicalCreationState ResolveModuleRef( Int32 idx )
      {
         var modName = this._md.ModuleReferences.TableContents[idx].ModuleName;
         LogicalCreationState retModule;
         if ( !this._allModuleStates.TryGetValue( modName, out retModule ) )
         {
            throw new InvalidOperationException( "No module named \"" + modName + "\" exists in this assembly." );
         }
         return retModule;
      }

      private Boolean MatchFieldSignatures( FieldSignature thisSignature, LogicalAssemblyCreationResult declaringTypeCreationResult, FieldSignature declaringTypeSignature )
      {
         return this.MatchTypeSignatures( thisSignature.Type, declaringTypeCreationResult, declaringTypeSignature.Type )
            && this.MatchCustomModifiers( thisSignature.CustomModifiers, declaringTypeCreationResult, declaringTypeSignature.CustomModifiers );
      }

      private Boolean MatchMethodSignatures( AbstractMethodSignature thisSignature, LogicalAssemblyCreationResult declaringTypeCreationResult, AbstractMethodSignature declaringTypeSignature )
      {
         return thisSignature.SignatureStarter == declaringTypeSignature.SignatureStarter
            && thisSignature.GenericArgumentCount == declaringTypeSignature.GenericArgumentCount
            && ListEqualityComparer<List<ParameterSignature>, ParameterSignature>.ListEquality( thisSignature.Parameters, declaringTypeSignature.Parameters, ( t, d ) => this.MatchParameterSignatures( t, declaringTypeCreationResult, d ) )
            && this.MatchParameterSignatures( thisSignature.ReturnType, declaringTypeCreationResult, declaringTypeSignature.ReturnType );
      }

      private Boolean MatchParameterSignatures( ParameterSignature thisSignature, LogicalAssemblyCreationResult declaringTypeCreationResult, ParameterSignature declaringTypeSignature )
      {
         return thisSignature.IsByRef == declaringTypeSignature.IsByRef
            && this.MatchTypeSignatures( thisSignature.Type, declaringTypeCreationResult, declaringTypeSignature.Type )
            && this.MatchCustomModifiers( thisSignature.CustomModifiers, declaringTypeCreationResult, declaringTypeSignature.CustomModifiers );
      }

      private Boolean MatchTypeSignatures( TypeSignature thisSignature, LogicalAssemblyCreationResult declaringTypeCreationResult, TypeSignature declaringTypeSignature )
      {
         var retVal = thisSignature.TypeSignatureKind == declaringTypeSignature.TypeSignatureKind;
         if ( retVal )
         {
            switch ( thisSignature.TypeSignatureKind )
            {
               case TypeSignatureKind.ClassOrValue:
                  var thisClass = (ClassOrValueTypeSignature) thisSignature;
                  var declaringClass = (ClassOrValueTypeSignature) declaringTypeSignature;
                  retVal = thisClass.IsClass == declaringClass.IsClass
                     && this.MatchSignatureTableIndices( thisClass.Type, declaringTypeCreationResult, declaringClass.Type )
                     && ListEqualityComparer<List<TypeSignature>, TypeSignature>.ListEquality( thisClass.GenericArguments, declaringClass.GenericArguments, ( t, d ) => this.MatchTypeSignatures( t, declaringTypeCreationResult, d ) );
                  break;
               case TypeSignatureKind.ComplexArray:
                  var thisArray = (ComplexArrayTypeSignature) thisSignature;
                  var declaringArray = (ComplexArrayTypeSignature) declaringTypeSignature;
                  retVal = thisArray.Rank == declaringArray.Rank
                     && ListEqualityComparer<List<Int32>, Int32>.ListEquality( thisArray.LowerBounds, declaringArray.LowerBounds )
                     && ListEqualityComparer<List<Int32>, Int32>.ListEquality( thisArray.Sizes, declaringArray.Sizes )
                     && this.MatchTypeSignatures( thisArray.ArrayType, declaringTypeCreationResult, declaringArray.ArrayType );
                  break;
               case TypeSignatureKind.FunctionPointer:
                  retVal = this.MatchMethodSignatures( ( (FunctionPointerTypeSignature) thisSignature ).MethodSignature, declaringTypeCreationResult, ( (FunctionPointerTypeSignature) declaringTypeSignature ).MethodSignature );
                  break;
               case TypeSignatureKind.GenericParameter:
                  var thisGParam = (GenericParameterTypeSignature) thisSignature;
                  var declaringGParam = (GenericParameterTypeSignature) declaringTypeSignature;
                  retVal = thisGParam.GenericParameterIndex == declaringGParam.GenericParameterIndex
                     && thisGParam.IsTypeParameter == declaringGParam.IsTypeParameter;
                  break;
               case TypeSignatureKind.Pointer:
                  var thisPtr = (PointerTypeSignature) thisSignature;
                  var declaringPtr = (PointerTypeSignature) declaringTypeSignature;
                  retVal = this.MatchTypeSignatures( thisPtr.PointerType, declaringTypeCreationResult, declaringPtr.PointerType )
                     && this.MatchCustomModifiers( thisPtr.CustomModifiers, declaringTypeCreationResult, declaringPtr.CustomModifiers );
                  break;
               case TypeSignatureKind.Simple:
                  retVal = ( (SimpleTypeSignature) thisSignature ).SimpleType == ( (SimpleTypeSignature) declaringTypeSignature ).SimpleType;
                  break;
               case TypeSignatureKind.SimpleArray:
                  var thisSimple = (SimpleArrayTypeSignature) thisSignature;
                  var declaringSimple = (SimpleArrayTypeSignature) declaringTypeSignature;
                  retVal = this.MatchTypeSignatures( thisSimple.ArrayType, declaringTypeCreationResult, declaringSimple.ArrayType )
                     && this.MatchCustomModifiers( thisSimple.CustomModifiers, declaringTypeCreationResult, declaringSimple.CustomModifiers );
                  break;
               default:
                  retVal = false;
                  break;
            }
         }

         return retVal;
      }

      private Boolean MatchCustomModifiers( List<CustomModifierSignature> thisMods, LogicalAssemblyCreationResult declaringTypeCreationResult, List<CustomModifierSignature> declaringTypeMods )
      {
         return ListEqualityComparer<List<CustomModifierSignature>, CustomModifierSignature>.ListEquality( thisMods, declaringTypeMods, ( x, y ) => x.IsOptional == y.IsOptional && this.MatchSignatureTableIndices( x.CustomModifierType, declaringTypeCreationResult, y.CustomModifierType ) );
      }

      private Boolean MatchSignatureTableIndices( TableIndex thisIndex, LogicalAssemblyCreationResult declaringTypeCreationResult, TableIndex declaringTypeIndex )
      {
         Boolean retVal;
         var dTable = declaringTypeIndex.Table;
         switch ( thisIndex.Table )
         {
            case Tables.TypeDef:
               // It is possible to have this scenario e.g. when there are circular references between assemblies
               retVal = dTable == Tables.TypeRef && this.MatchTypeDefAndTypeRef( this._creationResult, thisIndex.Index, declaringTypeCreationResult, declaringTypeIndex.Index );
               break;
            case Tables.TypeRef:
               // This assembly has reference to another assembly/module, match that
               switch ( dTable )
               {
                  case Tables.TypeDef:
                     // Match this type as reference to type defined in declaring type module
                     retVal = this.MatchTypeDefAndTypeRef( declaringTypeCreationResult, declaringTypeIndex.Index, this._creationResult, thisIndex.Index );
                     break;
                  case Tables.TypeRef:
                     // Match this type as reference to type defined in assembly/module referenced by both this and declaring type module
                     retVal = this.MatchTypeRefAndTypeRef( this._creationResult, thisIndex.Index, declaringTypeCreationResult, declaringTypeIndex.Index );
                     break;
                  default:
                     retVal = false;
                     break;
               }
               break;
            case Tables.TypeSpec:
               retVal = dTable == Tables.TypeSpec
                  && this.MatchTypeSignatures( this._md.TypeSpecifications.TableContents[thisIndex.Index].Signature, declaringTypeCreationResult, declaringTypeCreationResult.GetTypeSpecSignature( declaringTypeIndex.Index ) );
               break;
            default:
               retVal = false;
               break;
         }

         return retVal;
      }

      private static Boolean MatchNSAndName( String xNS, String xName, String yNS, String yName )
      {
         return String.Equals( xName, yName ) && String.Equals( xNS, yNS );
      }

      private Boolean MatchTypeDefAndTypeRef( LogicalAssemblyCreationResult tDefResult, Int32 tDefIndex, LogicalAssemblyCreationResult tRefResult, Int32 tRefIndex )
      {
         var tDef = tDefResult.GetAnyTypeDef( tDefIndex );
         var tRef = tRefResult.GetTypeReference( tRefIndex );
         var retVal = MatchNSAndName( tRef.Namespace, tRef.Name, tDef.Namespace, tDef.Name );
         if ( retVal )
         {
            var rScopeNullable = tRef.ResolutionScope;
            Int32 declTypeIndex;
            if ( tDefResult.TryGetDeclaringType( tDefIndex, out declTypeIndex ) )
            {
               retVal = rScopeNullable.HasValue
                  && rScopeNullable.Value.Table == Tables.TypeRef
                  && this.MatchTypeDefAndTypeRef( tDefResult, declTypeIndex, tRefResult, rScopeNullable.Value.Index );
            }
            else
            {
               if ( rScopeNullable.HasValue )
               {
                  var rScope = rScopeNullable.Value;
                  switch ( rScope.Table )
                  {
                     case Tables.AssemblyRef:
                        var aInstance = tDefResult.AssemblyInstance;
                        var aRef = tRefResult.GetAssemblyReference( rScope.Index );
                        retVal = aRef.Attributes.IsRetargetable() ?
                           String.Equals( aRef.AssemblyInformation.Name, aInstance.Name ) :
                           aInstance.ReflectionContext.DefaultAssemblyNameComparer.Equals( aInstance.Name, new CILAssemblyName( aRef.AssemblyInformation, aRef.Attributes.IsFullPublicKey() ) );
                        break;
                     case Tables.ModuleRef:
                        LogicalCreationState otherState;
                        retVal = tRefResult.AssemblyInstance.Equals( tDefResult.AssemblyInstance )
                           && this._allModuleStates.TryGetValue( tRefResult.GetModuleReference( rScope.Index ), out otherState )
                           && ReferenceEquals( this, otherState );
                        break;
                     default:
                        retVal = false;
                        break;
                  }
               }
               else
               {
                  throw new NotImplementedException( "Null resolution scope (2)." );
               }
            }
         }

         return retVal;
      }

      private Boolean MatchTypeRefAndTypeRef( LogicalAssemblyCreationResult xResult, Int32 xIndex, LogicalAssemblyCreationResult yResult, Int32 yIndex )
      {
         var x = xResult.GetTypeReference( xIndex );
         var y = yResult.GetTypeReference( yIndex );
         var retVal = MatchNSAndName( x.Namespace, x.Name, y.Namespace, y.Name );
         if ( retVal )
         {
            var xResScopeNullable = x.ResolutionScope;
            var yResScopeNullable = y.ResolutionScope;
            if ( xResScopeNullable.HasValue && yResScopeNullable.HasValue )
            {
               var xResScope = xResScopeNullable.Value;
               var yResScope = yResScopeNullable.Value;
               var yTable = yResScope.Table;
               switch ( xResScope.Table )
               {
                  case Tables.AssemblyRef:
                     retVal = yTable == Tables.AssemblyRef
                        && this.MatchAssemblyRefs( xResult.GetAssemblyReference( xResScope.Index ), yResult.GetAssemblyReference( yResScope.Index ) );
                     break;
                  case Tables.TypeRef:
                     retVal = yTable == Tables.TypeRef
                        && this.MatchTypeDefAndTypeRef( xResult, xResScope.Index, yResult, yResScope.Index );
                     break;
                  case Tables.ModuleRef:
                     retVal = xResult.AssemblyInstance.Equals( yResult.AssemblyInstance )
                        && ( ( yTable == Tables.ModuleRef && String.Equals( xResult.GetModuleReference( xResScope.Index ), yResult.GetModuleReference( yResScope.Index ) ) )
                           || ( yTable == Tables.Module && this.MatchTypeDefAndTypeRef( yResult, yResult.ResolveTypeInfo( yResult.ResolveTopLevelType( y.Namespace, y.Name, true ) ).Value, xResult, xIndex ) )
                           );
                     break;
                  case Tables.Module:
                     retVal = ( yTable == Tables.ModuleRef || yTable == Tables.AssemblyRef )
                        && this.MatchTypeDefAndTypeRef( xResult, xResult.ResolveTypeInfo( xResult.ResolveTopLevelType( x.Namespace, x.Name, true ) ).Value, yResult, yIndex );
                     break;
                  default:
                     retVal = false;
                     break;
               }
            }
            else
            {
               throw new NotImplementedException( "Null resolution scope (3)." );
            }
         }

         return retVal;
      }

      private Boolean MatchAssemblyRefs( AssemblyReference x, AssemblyReference y )
      {
         var xInfo = x.AssemblyInformation;
         var yInfo = y.AssemblyInformation;
         return x.Attributes.IsRetargetable() || y.Attributes.IsRetargetable() ?
            String.Equals( xInfo.Name, yInfo.Name ) :
            this._module.ReflectionContext.DefaultAssemblyNameComparer.Equals( new CILAssemblyName( xInfo, x.Attributes.IsFullPublicKey() ), new CILAssemblyName( yInfo, y.Attributes.IsFullPublicKey() ) );
      }

      private LogicalAssemblyCreationResult ResolveMSCorLibModule( LogicalAssemblyCreationResult msCorLibOverride )
      {
         // Try find "System.Object"
         var testTypeString = Consts.OBJECT;

         var suitableModule = this.GetSuitableMSCorLibModules( msCorLibOverride ).FirstOrDefault( m =>
         {
            var testType = m.ResolveTypeString( testTypeString, false );
            var retVal = testType != null;
            if ( retVal )
            {
               // Perform additional checks
               var tDef = m.LogicalToPhysical( testType );
               retVal = tDef.Attributes.IsClass() && !tDef.BaseType.HasValue;
            }
            return retVal;
         } );

         if ( suitableModule == null )
         {
            throw new InvalidOperationException( "Failed to resolve mscorlib module." );
         }

         return suitableModule;
      }

      private IEnumerable<LogicalAssemblyCreationResult> GetSuitableMSCorLibModules( LogicalAssemblyCreationResult msCorLibOverride )
      {
         if ( msCorLibOverride != null )
         {
            yield return msCorLibOverride;
         }

         yield return this._creationResult;

         var aRefs = this._md.AssemblyReferences.TableContents;
         var aRefList = new List<Int32>( Enumerable.Range( 0, aRefs.Count )
            .Where( aRef => aRefs[aRef] != null )
            );

         aRefList.Sort( ( xIdx, yIdx ) =>
         {
            var x = aRefs[xIdx].AssemblyInformation;
            var y = aRefs[yIdx].AssemblyInformation;

            Int32 retVal;
            if ( x.Equals( y ) )
            {
               retVal = 0;
            }
            else
            {
               retVal = StringComparer.Ordinal.Compare( x.Name, y.Name );
               if ( retVal == 0 )
               {
                  // Compare by-version, newest first
                  retVal = x.VersionMajor.CompareTo( y.VersionMajor );
                  if ( retVal == 0 )
                  {
                     retVal = x.VersionMinor.CompareTo( y.VersionMinor );
                     if ( retVal == 0 )
                     {
                        retVal = x.VersionBuild.CompareTo( y.VersionBuild );
                        if ( retVal == 0 )
                        {
                           retVal = x.VersionRevision.CompareTo( y.VersionRevision );
                        }
                     }
                  }
               }
               else if ( !String.IsNullOrEmpty( x.Name ) )
               {
                  // MSCorLib < System.Runtime < anything else
                  if ( retVal > 0 )
                  {
                     switch ( x.Name )
                     {
                        case Consts.MSCORLIB_NAME:
                        case Consts.NEW_MSCORLIB_NAME:
                           retVal = -1;
                           break;
                     }
                  }
                  else
                  {
                     switch ( y.Name )
                     {
                        case Consts.MSCORLIB_NAME:
                        case Consts.NEW_MSCORLIB_NAME:
                           retVal = 1;
                           break;
                     }
                  }
               }
            }

            return retVal;
         } );

         foreach ( var aRefIdx in aRefList )
         {
            yield return this.ResolveAssemblyRef( aRefIdx );
         }
      }
   }

   /// <summary>
   /// Creates a <see cref="LogicalAssemblyCreationResult"/> representing Logical layer <see cref="CILAssembly"/> with contents built from given Physical layer <see cref="CILMetaData"/>.
   /// </summary>
   /// <param name="ctx">The <see cref="CILReflectionContext"/>.</param>
   /// <param name="metaData">The Physical layer <see cref="CILMetaData"/>.</param>
   /// <param name="moduleResolver">The callback to resolve other modules of the assembly.</param>
   /// <param name="assemblyReferenceResolver">The callback to resolve other assemblies referenced by this assembly.</param>
   /// <param name="msCorLibOverride">The optional override of system library holding types like <see cref="Object"/>, <see cref="Array"/>, <see cref="Enum"/>, etc.</param>
   /// <returns>The <see cref="LogicalAssemblyCreationResult"/> for given Physical layer <see cref="CILMetaData"/>.</returns>
   /// <remarks>
   /// This method should rarely be used directly.
   /// Instead, one should utilize <see cref="CILAssemblyLoaderNotThreadSafe"/> or <see cref="CILAssemblyLoaderThreadSafeSimple"/> classes to load <see cref="CILAssembly"/> from files and cache results.
   /// </remarks>
   /// <exception cref="InvalidOperationException">If <paramref name="metaData"/> does not contain assembly or module information (i.e., the <see cref="CILMetaData.AssemblyDefinitions"/> or <see cref="CILMetaData.ModuleDefinitions"/> is empty).</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="metaData"/>, <paramref name="moduleResolver"/>, or <paramref name="assemblyReferenceResolver"/> is <c>null</c>.</exception>
   public static LogicalAssemblyCreationResult CreateLogicalRepresentation(
      this CILReflectionContext ctx,
      CILMetaData metaData,
      Func<String, CILMetaData> moduleResolver,
      Func<CILMetaData, CILAssemblyName, LogicalAssemblyCreationResult> assemblyReferenceResolver,
      LogicalAssemblyCreationResult msCorLibOverride = null
      )
   {
      ArgumentValidator.ValidateNotNull( "Physical metadata", metaData );
      ArgumentValidator.ValidateNotNull( "Module resolver", moduleResolver );
      ArgumentValidator.ValidateNotNull( "Assembly reference resolver", assemblyReferenceResolver );

      var aDefList = metaData.AssemblyDefinitions.TableContents;
      if ( aDefList.Count <= 0 )
      {
         throw new InvalidOperationException( "The physical metadata does not contain assembly information." );
      }

      var modList = metaData.ModuleDefinitions.TableContents;
      if ( modList.Count <= 0 )
      {
         throw new InvalidOperationException( "The physical metadata does not contain module information." );
      }

      var aDef = aDefList[0];
      var assembly = ctx.NewBlankAssembly( aDef.AssemblyInformation.Name );
      var an = assembly.Name;
      aDef.AssemblyInformation.DeepCopyContentsTo( an.AssemblyInformation );
      an.HashAlgorithm = aDef.HashAlgorithm;
      an.Flags = aDef.Attributes;

      var allModuleStates = new Dictionary<String, LogicalCreationState>();
      var retVal = new LogicalAssemblyCreationResult(
         metaData,
         assembly,
         () =>
         {
            foreach ( var state in allModuleStates.Values )
            {
               state.Module.AssociatedMSCorLibModule = state.AssociatedMSCorLibModule.AssemblyInstance.MainModule;
               state.CreateBasicStructure();
            }
         }, () =>
         {
            foreach ( var state in allModuleStates.Values )
            {
               state.CreateComplexStructure();
            }
         } );

      // Adding first module will make it main module (TODO this is a bit un-intuitive...)
      var mainModuleState = retVal.CreateLogicalCreationState(
         allModuleStates,
         assembly.AddModule( modList[0].Name ),
         metaData,
         assemblyReferenceResolver,
         msCorLibOverride
         );
      allModuleStates[mainModuleState.Module.Name] = mainModuleState;

      foreach ( var module in metaData.FileReferences.TableContents.Where( f => f.Attributes.ContainsMetadata() ) )
      {
         var name = module.Name;
         var moduleMD = moduleResolver( name );
         if ( moduleMD == null )
         {
            // TODO change exception type and document this situation.
            throw new InvalidOperationException( "Failed to resolve module \"" + moduleMD + "\"." );
         }

         var cilModule = assembly.AddModule( name );
         allModuleStates[name] = retVal.CreateLogicalCreationState(
            allModuleStates,
            cilModule,
            moduleMD,
            assemblyReferenceResolver,
            msCorLibOverride
            );
      }

      return retVal;
   }

   private static LogicalCreationState CreateLogicalCreationState(
      this LogicalAssemblyCreationResult creationResult,
      IDictionary<String, LogicalCreationState> allModuleStates,
      CILModule module,
      CILMetaData metaData,
      Func<CILMetaData, CILAssemblyName, LogicalAssemblyCreationResult> customAssemblyResolver,
      LogicalAssemblyCreationResult msCorLibOverride
      )
   {
      var anEQComparer = module.ReflectionContext.DefaultAssemblyNameComparer;
      var assemblyNameCache = new Dictionary<CILAssemblyName, LogicalAssemblyCreationResult>( anEQComparer );
      var thisAN = creationResult.AssemblyInstance.Name;
      var state = new LogicalCreationState(
         allModuleStates,
         creationResult,
         module,
         metaData,
         aName => assemblyNameCache.GetOrAdd_NotThreadSafe( aName, an =>
         {
            LogicalAssemblyCreationResult retVal;
            // Check for self-reference...
            if ( anEQComparer.Equals( thisAN, an ) )
            {
               retVal = creationResult;
            }
            else
            {
               if ( customAssemblyResolver == null )
               {
                  throw new InvalidOperationException( "Custom assembly resolver is null." );
               }
               else
               {
                  retVal = customAssemblyResolver( metaData, an );
               }
            }

            return retVal;
         } ),
         msCorLibOverride
         );


      module.AssociatedMSCorLibModule = null;

      var tDefs = metaData.TypeDefinitions.TableContents;
      if ( tDefs.Count > 0 )
      {
         state.ProcessNewlyCreatedType( 0, module.ModuleInitializer );

         for ( var i = 1; i < tDefs.Count; ++i )
         {
            if ( state.CreationResult.IsTopLevelType( i ) )
            {
               state.CreateLogicalType( module, i );
            }
         }
      }

      state.ProcessGenericParameters();

      return state;
   }

   private static void CreateLogicalType( this LogicalCreationState state, CILElementCapableOfDefiningType owner, Int32 typeDefIndex )
   {

      var md = state.MetaData;
      var tDef = md.TypeDefinitions.TableContents[typeDefIndex];

      CILTypeCode tc;

      Int32 enumValueFieldIndex;
      if ( md.TryGetEnumValueFieldIndex( typeDefIndex, out enumValueFieldIndex ) )
      {
         tc = ResolveTypeCodeFromEnumType( md.FieldDefinitions.TableContents[enumValueFieldIndex].Signature.Type );
      }
      else
      {
         tc = ResolveTypeCodeTextual( tDef.Namespace, tDef.Name );
      }

      var retVal = state.GetTypeDef( typeDefIndex );
      if ( retVal == null )
      {
         retVal = owner.AddType( tDef.Name, tDef.Attributes, tc );
         retVal.Namespace = tDef.Namespace;
         state.ProcessNewlyCreatedType( typeDefIndex, retVal );
      }
   }

   private static void ProcessNewlyCreatedType( this LogicalCreationState state, Int32 typeDefIndex, CILType typeDef )
   {

      state.RecordTypeDef( typeDef, typeDefIndex );

      // Fields
      var md = state.MetaData;
      var fieldDefs = md.FieldDefinitions.TableContents;
      foreach ( var fIdx in md.GetTypeFieldIndices( typeDefIndex ) )
      {
         var field = fieldDefs[fIdx];
         state.RecordFieldDef( typeDef.AddField( field.Name, null, field.Attributes ), fIdx );
      }

      // Methods and parameters
      var methodDefs = md.MethodDefinitions.TableContents;
      var paramDefs = md.ParameterDefinitions.TableContents;
      foreach ( var mIdx in md.GetTypeMethodIndices( typeDefIndex ) )
      {
         var method = methodDefs[mIdx];
         var name = method.Name;
         var isCtor = String.Equals( name, Miscellaneous.INSTANCE_CTOR_NAME ) || String.Equals( name, Miscellaneous.CLASS_CTOR_NAME );
         var cilMethod = isCtor ?
            (CILMethodBase) typeDef.AddConstructor( method.Attributes, method.Signature.SignatureStarter.GetCallingConventionFromSignature() ) :
            typeDef.AddMethod( name, method.Attributes, method.Signature.SignatureStarter.GetCallingConventionFromSignature() );
         state.RecordMethodDef( cilMethod, mIdx );
         cilMethod.ImplementationAttributes = method.ImplementationAttributes;

         foreach ( var pIdx in md.GetMethodParameterIndices( mIdx ).OrderBy( p => paramDefs[p].Sequence ) )
         {
            var param = paramDefs[pIdx];
            CILParameter cilParam;
            if ( param.Sequence > 0 )
            {
               cilParam = cilMethod.AddParameter( param.Name, param.Attributes, null );
            }
            else
            {
               cilParam = isCtor ? null : ( (CILMethod) cilMethod ).ReturnParameter;
            }
            state.RecordParameter( cilParam, pIdx );
         }
      }

      // Nested types
      ISet<Int32> nestedList;
      if ( state.NestedTypes.TryGetValue( typeDefIndex, out nestedList ) )
      {
         foreach ( var nested in nestedList )
         {
            state.CreateLogicalType( typeDef, nested );
         }
      }
   }

   private static CILTypeCode ResolveTypeCodeFromEnumType( TypeSignature typeSig )
   {
      if ( typeSig.TypeSignatureKind == TypeSignatureKind.Simple )
      {
         switch ( ( (SimpleTypeSignature) typeSig ).SimpleType )
         {
            case SignatureElementTypes.Char:
               return CILTypeCode.Char;
            case SignatureElementTypes.I1:
               return CILTypeCode.SByte;
            case SignatureElementTypes.U1:
               return CILTypeCode.Byte;
            case SignatureElementTypes.I2:
               return CILTypeCode.Int16;
            case SignatureElementTypes.U2:
               return CILTypeCode.UInt16;
            case SignatureElementTypes.I4:
               return CILTypeCode.Int32;
            case SignatureElementTypes.U4:
               return CILTypeCode.UInt64;
            case SignatureElementTypes.I8:
               return CILTypeCode.Int64;
            case SignatureElementTypes.U8:
               return CILTypeCode.UInt64;
            default:
               return CILTypeCode.Object;
         }
      }
      else
      {
         return CILTypeCode.Object;
      }
   }

   private static CILTypeCode ResolveTypeCodeTextual( String ns, String tn )
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

   private static void ProcessGenericParameters( this LogicalCreationState state )
   {
      var md = state.MetaData;

      var gParamInfos = new Dictionary<CILElementWithGenericArguments<Object>, List<Int32>>();
      var gParamTable = md.GenericParameterDefinitions.TableContents;
      for ( var i = 0; i < gParamTable.Count; ++i )
      {
         var gParam = gParamTable[i];
         var parent = gParam.Owner;
         CILElementWithGenericArguments<Object> cilParent;
         switch ( parent.Table )
         {
            case Tables.TypeDef:
               cilParent = state.GetTypeDef( parent.Index );
               break;
            case Tables.MethodDef:
               cilParent = state.GetMethodDef( parent.Index ) as CILMethod;
               break;
            default:
               throw new InvalidOperationException( "Unrecognized generic parameter owner: " + parent + "." );
         }
         if ( cilParent != null )
         {
            gParamInfos
               .GetOrAdd_NotThreadSafe( cilParent, p => new List<Int32>() )
               .Add( i );
         }
      }

      foreach ( var gParamInfo in gParamInfos )
      {
         var parent = gParamInfo.Key;
         var gParams = gParamInfo.Value;
         gParams.Sort( ( x, y ) => gParamTable[x].GenericParameterIndex.CompareTo( gParamTable[y].GenericParameterIndex ) );
         var cilGParams = parent.DefineGenericParameters( gParams.Select( g => gParamTable[g].Name ).ToArray() );
         for ( var i = 0; i < cilGParams.Length; ++i )
         {
            var cilGParam = cilGParams[i];
            var gParam = gParams[i];
            state.RecordTypeParameter( cilGParam, gParam );
            cilGParam.Attributes = gParamTable[gParam].Attributes;
         }
      }
   }


   private static void CreateBasicStructure( this LogicalCreationState state )
   {
      var md = state.MetaData;
      // TypeDef table
      var tDefs = md.TypeDefinitions.TableContents;
      for ( var i = 0; i < tDefs.Count; ++i )
      {
         var tDef = tDefs[i];
         var cilType = state.GetTypeDef( i );

         // Base type
         if ( tDef.BaseType.HasValue )
         {
            cilType.BaseType = state.ResolveTypeDefOrRefOrSpec( tDef.BaseType.Value, cilType, null, true ) as CILType;
         }
         else
         {
            cilType.BaseType = null;
         }

         // Field types and custom modifiers
         foreach ( var fIdx in md.GetTypeFieldIndices( i ) )
         {
            var fSig = md.FieldDefinitions.TableContents[fIdx].Signature;
            var field = state.GetFieldDef( fIdx );
            field.FieldType = state.ResolveTypeSignature( fSig.Type, cilType, null );
            state.AddCustomModifiers( field, fSig.CustomModifiers, cilType, null );
         }

         // Method parameter types and custom modifiers
         foreach ( var mIdx in md.GetTypeMethodIndices( i ) )
         {
            var mSig = md.MethodDefinitions.TableContents[mIdx].Signature;
            var methodBase = state.GetMethodDef( mIdx );
            for ( var pIdx = 0; pIdx < mSig.Parameters.Count; ++pIdx )
            {
               var pSig = mSig.Parameters[pIdx];
               var cilParam = methodBase.Parameters[pIdx];
               cilParam.ParameterType = state.ResolveParamSignature( pSig, cilType, methodBase );
               state.AddCustomModifiers( cilParam, pSig.CustomModifiers, cilType, methodBase );
            }
            var method = methodBase as CILMethod;
            if ( method != null )
            {
               var pSig = mSig.ReturnType;
               var cilParam = method.ReturnParameter;
               cilParam.ParameterType = state.ResolveParamSignature( pSig, cilType, methodBase );
               state.AddCustomModifiers( cilParam, pSig.CustomModifiers, cilType, methodBase );
            }
         }
      }

      // Properties
      var pMaps = md.PropertyMaps.TableContents;
      for ( var i = 0; i < pMaps.Count; ++i )
      {
         var type = state.GetTypeDef( pMaps[i].Parent.Index );
         foreach ( var propIdx in md.GetTypePropertyIndices( i ) )
         {
            var prop = md.PropertyDefinitions.TableContents[propIdx];
            var cilProp = type.AddProperty( prop.Name, prop.Attributes );
            state.AddCustomModifiers( cilProp, prop.Signature.CustomModifiers, type, null );
            state.RecordProperty( cilProp, propIdx );
         }
      }

      // Events
      var eMaps = md.EventMaps.TableContents;
      for ( var i = 0; i < eMaps.Count; ++i )
      {
         var type = state.GetTypeDef( eMaps[i].Parent.Index );
         foreach ( var evtIdx in md.GetTypeEventIndices( i ) )
         {
            var evt = md.EventDefinitions.TableContents[evtIdx];
            var cilEvt = type.AddEvent( evt.Name, evt.Attributes, state.ResolveTypeDefOrRefOrSpec( evt.EventType, type, null ) );
            state.RecordEvent( cilEvt, evtIdx );
         }
      }

      // Method semantics (custom attribute creation will require properties to have their setters/getters ready)
      foreach ( var semantics in md.MethodSemantics.TableContents )
      {
         var method = (CILMethod) state.GetMethodDef( semantics.Method.Index );
         var asso = semantics.Associaton;
         switch ( semantics.Attributes )
         {
            case MethodSemanticsAttributes.Getter:
               state.GetProperty( asso.Index ).GetMethod = method;
               break;
            case MethodSemanticsAttributes.Setter:
               state.GetProperty( asso.Index ).SetMethod = method;
               break;
            case MethodSemanticsAttributes.AddOn:
               state.GetEvent( asso.Index ).AddMethod = method;
               break;
            case MethodSemanticsAttributes.RemoveOn:
               state.GetEvent( asso.Index ).RemoveMethod = method;
               break;
            case MethodSemanticsAttributes.Fire:
               state.GetEvent( asso.Index ).RaiseMethod = method;
               break;
            case MethodSemanticsAttributes.Other:
               state.GetEvent( asso.Index ).AddOtherMethods( method );
               break;
            default:
               throw new InvalidOperationException( "Unrecognized semantics attributes: " + semantics.Attributes + "." );
         }
      }
   }

   private static void CreateComplexStructure( this LogicalCreationState state )
   {
      var md = state.MetaData;
      // Generic parameter constraints
      foreach ( var gParamConstraint in md.GenericParameterConstraintDefinitions.TableContents )
      {
         var typeParam = state
            .GetTypeParameter( gParamConstraint.Owner.Index );
         typeParam.AddGenericParameterConstraints( state.ResolveTypeDefOrRefOrSpec( gParamConstraint.Constraint, typeParam.DeclaringType, typeParam.DeclaringMethod ) );
      }

      // Class layout table
      foreach ( var layout in md.ClassLayouts.TableContents )
      {
         state.GetTypeDef( layout.Parent.Index ).Layout = new LogicalClassLayout( layout.ClassSize, layout.PackingSize );
      }

      // Constants
      foreach ( var constant in md.ConstantDefinitions.TableContents )
      {
         CILElementWithConstant element;
         var parent = constant.Parent;
         switch ( parent.Table )
         {
            case Tables.Parameter:
               element = state.GetParameterDef( parent.Index );
               break;
            case Tables.Field:
               element = state.GetFieldDef( parent.Index );
               break;
            case Tables.Property:
               element = state.GetProperty( parent.Index );
               break;
            default:
               throw new InvalidOperationException( "Unrecognized constant parent: " + parent + "." );
         }

         if ( element != null )
         {
            element.ConstantValue = constant.Value;
         }
      }

      // Field layouts
      foreach ( var layout in md.FieldLayouts.TableContents )
      {
         state.GetFieldDef( layout.Field.Index ).FieldOffset = layout.Offset;
      }

      // Field marshals
      foreach ( var marshal in md.FieldMarshals.TableContents )
      {
         CILElementWithMarshalingInfo element;
         var parent = marshal.Parent;
         switch ( parent.Table )
         {
            case Tables.Field:
               element = state.GetFieldDef( parent.Index );
               break;
            case Tables.Parameter:
               element = state.GetParameterDef( parent.Index );
               break;
            default:
               throw new InvalidOperationException( "Unrecognized field marshal parent: " + parent + "." );
         }

         if ( element != null )
         {
            var marshalInfo = marshal.NativeType;
            element.MarshalingInformation = new LogicalMarshalingInfo(
               marshalInfo.Value,
               marshalInfo.SafeArrayType,
               state.ResolveTypeString( marshalInfo.SafeArrayUserDefinedType, false ),
               marshalInfo.IIDParameterIndex,
               marshalInfo.ArrayType,
               marshalInfo.SizeParameterIndex,
               marshalInfo.ConstSize,
               marshalInfo.MarshalType,
               s => state.ResolveTypeString( s, false ),
               marshalInfo.MarshalCookie
               );
         }
      }

      // Field RVAs
      foreach ( var rva in md.FieldRVAs.TableContents )
      {
         state.GetFieldDef( rva.Field.Index ).InitialValue = rva.Data;
      }

      // Implementation Maps
      foreach ( var impl in md.MethodImplementationMaps.TableContents )
      {
         var member = impl.MemberForwarded;
         CILMethod method;
         switch ( member.Table )
         {
            case Tables.MethodDef:
               method = state.GetMethodDef( member.Index ) as CILMethod;
               break;
            case Tables.Field:
               // Not implemented
               method = null;
               break;
            default:
               throw new InvalidOperationException( "Unrecognized implementation map member: " + member + "." );
         }

         if ( method != null )
         {
            method.PlatformInvokeAttributes = impl.Attributes;
            method.PlatformInvokeName = impl.ImportName;
            method.PlatformInvokeModuleName = md.ModuleReferences.TableContents[impl.ImportScope.Index].ModuleName;
         }
      }

      // InterfaceImpl
      foreach ( var impl in md.InterfaceImplementations.TableContents )
      {
         var type = state.GetTypeDef( impl.Class.Index );
         var iFace = state.ResolveTypeDefOrRefOrSpec( impl.Interface, type, null ) as CILType;
         if ( iFace != null )
         {
            type.AddDeclaredInterfaces( iFace );
         }
      }

      // MethodImpl
      foreach ( var impl in md.MethodImplementations.TableContents )
      {
         var methodBody = impl.MethodBody;
         var type = state.GetTypeDef( impl.Class.Index );
         var cilMethodBody = state.ResolveMethodDefOrMemberRef( methodBody, type, null ) as CILMethod;
         if ( cilMethodBody != null )
         {
            var cilMethodDecl = state.ResolveMethodDefOrMemberRef( impl.MethodDeclaration, type, cilMethodBody ) as CILMethod;
            if ( cilMethodDecl != null )
            {
               type.AddExplicitMethodImplementation( cilMethodBody, cilMethodDecl );
            }
         }

      }

      // DeclSecurity
      var secDefs = md.SecurityDefinitions.TableContents;
      for ( var i = 0; i < secDefs.Count; ++i )
      {
         var sec = secDefs[i];
         var parent = sec.Parent;
         CILElementWithSecurityInformation cilParent;
         switch ( parent.Table )
         {
            case Tables.TypeDef:
               cilParent = state.GetTypeDef( parent.Index );
               break;
            case Tables.MethodDef:
               cilParent = state.GetMethodDef( parent.Index );
               break;
            case Tables.Assembly:
               cilParent = null;
               break;
            default:
               throw new InvalidOperationException( "Unsupported security information parent: " + parent + "." );
         }

         if ( cilParent != null )
         {
            var secIdx = new TableIndex( Tables.DeclSecurity, i );
            foreach ( var permission in sec.PermissionSets )
            {
               IList<CustomAttributeNamedArgument> namedArgs;
               switch ( permission.SecurityInformationKind )
               {
                  case SecurityInformationKind.Resolved:
                     namedArgs = ( (SecurityInformation) permission ).NamedArguments;
                     break;
                  case SecurityInformationKind.Raw:
                     throw new InvalidOperationException( "Unresolved security information at " + secIdx + "." );
                  default:
                     throw new InvalidOperationException( "Unsupported security information kind: " + permission.SecurityInformationKind + "." );
               }

               // Populate target
               var declType = state.ResolveTypeString( permission.SecurityAttributeType, true );
               var cilPermission = cilParent.AddDeclarativeSecurity( sec.Action, declType );
               cilPermission.NamedArguments.AddRange( namedArgs.Select( arg => state.CreateCANamedArg( declType, secIdx, arg ) ) );
            }
         }
      }

      // Manifest resources
      foreach ( var mResource in md.ManifestResources.TableContents )
      {
         var name = mResource.Name;
         if ( state.Module.ManifestResources.ContainsKey( name ) )
         {
            throw new InvalidOperationException( "Duplicate manifest resource: \"" + name + "\"." );
         }
         else
         {
            var implNullable = mResource.Implementation;
            AbstractLogicalManifestResource resource;
            if ( implNullable.HasValue )
            {
               var impl = implNullable.Value;
               switch ( impl.Table )
               {
                  case Tables.File:
                     var file = md.FileReferences.TableContents[impl.Index];
                     resource = new FileManifestResource( mResource.Attributes, file.Name, file.HashValue.CreateBlockCopy() );
                     break;
                  case Tables.AssemblyRef:
                     resource = new AssemblyManifestResource( mResource.Attributes, state.ResolveAssemblyRef( impl.Index ).AssemblyInstance );
                     break;
                  default:
                     throw new InvalidOperationException( "Unsupported manifest resource implementation: " + impl + "." );
               }
            }
            else
            {
               resource = new EmbeddedManifestResource( mResource.Attributes, mResource.DataInCurrentFile );
            }

            state.Module.ManifestResources.Add( name, resource );
         }
      }

      // Custom attributes
      var customAttrs = md.CustomAttributeDefinitions.TableContents;
      for ( var i = 0; i < customAttrs.Count; ++i )
      {
         var ca = customAttrs[i];
         var owner = ca.Parent;
         CILCustomAttributeContainer cilOwner;
         switch ( owner.Table )
         {
            case Tables.TypeDef:
               cilOwner = state.GetTypeDef( owner.Index );
               break;
            case Tables.MethodDef:
               cilOwner = state.GetMethodDef( owner.Index );
               break;
            case Tables.Field:
               cilOwner = state.GetFieldDef( owner.Index );
               break;
            case Tables.Parameter:
               cilOwner = state.GetParameterDef( owner.Index );
               break;
            case Tables.Module:
               cilOwner = state.Module;
               break;
            case Tables.Property:
               cilOwner = state.GetProperty( owner.Index );
               break;
            case Tables.Event:
               cilOwner = state.GetEvent( owner.Index );
               break;
            case Tables.Assembly:
               cilOwner = state.Module.Assembly;
               break;
            case Tables.GenericParameter:
               cilOwner = state.GetTypeParameter( owner.Index );
               break;
            case Tables.TypeRef:
            case Tables.InterfaceImpl:
            case Tables.MemberRef:
            case Tables.DeclSecurity:
            case Tables.StandaloneSignature:
            case Tables.ModuleRef:
            case Tables.TypeSpec:
            case Tables.AssemblyRef:
            case Tables.File:
            case Tables.ExportedType:
            case Tables.ManifestResource:
            case Tables.GenericParameterConstraint:
            case Tables.MethodSpec:
               // CAM.Logical does not expose adding custom attributes to these
               cilOwner = null;
               break;
            default:
               throw new InvalidOperationException( "Unrecognized custom attribute parent: " + owner + "." );
         }

         if ( cilOwner != null )
         {
            state.AddCustomAttributeTo( cilOwner, i );
         }
      }

      // Create method IL
      var max = md.MethodDefinitions.TableContents.Count;
      for ( var i = 0; i < max; ++i )
      {
         state.CreateLogicalIL( i );
      }

      // TODO ExportedType

   }

   private static void AddCustomAttributeTo( this LogicalCreationState state, CILCustomAttributeContainer owner, Int32 caIdx )
   {
      var ca = state.MetaData.CustomAttributeDefinitions.TableContents[caIdx];
      var ctor = state.ResolveMethodDefOrMemberRef( ca.Type, null, null ) as CILConstructor;
      if ( ctor != null )
      {
         var sig = ca.Signature;
         if ( sig.CustomAttributeSignatureKind == CustomAttributeSignatureKind.Resolved )
         {
            var caSig = (CustomAttributeSignature) sig;
            if ( ctor.Parameters.Count != caSig.TypedArguments.Count )
            {
               throw new InvalidOperationException( "Constructor parameter count mismatch ( custom attribute index " + caIdx + ", constuctor parameter count: " + ctor.Parameters.Count + ", signature parameter count: " + caSig.TypedArguments.Count + "." );
            }

            var declType = ctor.DeclaringType;
            var caTableIdx = new TableIndex( Tables.CustomAttribute, caIdx );
            owner.AddCustomAttribute( ctor, caSig.TypedArguments.Select( ( arg, idx ) => state.CreateCATypedArg( ctor.Parameters[idx].ParameterType as CILType, arg.Value ) ), caSig.NamedArguments.Select( arg => state.CreateCANamedArg( declType, caTableIdx, arg ) ) );
         }
         else
         {
            throw new InvalidOperationException( "Custom attribute signature at zero-based index " + caIdx + " must be resolved." );
         }
      }
   }

   private static CILCustomAttributeTypedArgument CreateCATypedArg( this LogicalCreationState state, CILTypeBase currentParamTypeBase, Object currentValue )
   {
      var currentParamType = currentParamTypeBase as CILType;
      if ( currentParamType == null )
      {
         throw new InvalidOperationException( "Custom attribute typed argument type was not type or null." );
      }

      if ( currentValue != null )
      {
         var complex = currentValue as CustomAttributeTypedArgumentValueComplex;
         if ( complex != null )
         {
            // Enum, type, or array
            switch ( complex.CustomAttributeTypedArgumentValueKind )
            {
               case CustomAttributeTypedArgumentValueKind.Type:
                  currentParamType = state.Module.GetTypeForTypeCode( CILTypeCode.Type );
                  currentValue = state.ResolveTypeString( ( (CustomAttributeValue_TypeReference) complex ).TypeString, false );
                  break;
               case CustomAttributeTypedArgumentValueKind.Enum:
                  var enumValue = (CustomAttributeValue_EnumReference) complex;
                  if ( !currentParamType.IsEnum() ) // E.g. System.Object
                  {
                     currentParamType = state.ResolveTypeString( enumValue.EnumType, false );
                  }
                  currentValue = enumValue.EnumValue;
                  break;
               case CustomAttributeTypedArgumentValueKind.Array:
                  var array = (CustomAttributeValue_Array) complex;
                  currentParamType = state.ResolveCAType( array.ArrayElementType );
                  currentValue = array.Array.Cast<Object>().Select( o => state.CreateCATypedArg( currentParamType, o ) ).ToList();
                  currentParamType = currentParamType.MakeArrayType();
                  break;
               default:
                  throw new InvalidOperationException( "Unrecognized complex CA typed argument value kind: " + complex.CustomAttributeTypedArgumentValueKind + "." );
            }
         }
         else if ( currentParamType.TypeCode == CILTypeCode.Type )
         {
            // Type stored as System.Type/String
            currentValue = currentValue is Type ?
               state.Module.ReflectionContext.NewWrapperAsType( (Type) currentValue ) :
               state.ResolveTypeString( currentValue.ToString(), false );
         }
      }

      return CILCustomAttributeFactory.NewTypedArgument( currentParamType, currentValue );
   }

   private static CILCustomAttributeNamedArgument CreateCANamedArg( this LogicalCreationState state, CILType declType, TableIndex caIdx, CustomAttributeNamedArgument sig )
   {
      var isField = sig.IsField;
      var namedElement = isField ?
         (CILElementForNamedCustomAttribute) declType.GetBaseTypeChain().SelectMany( t => t.DeclaredFields ).FirstOrDefault( f => String.Equals( f.Name, sig.Name ) ) :
         declType.GetBaseTypeChain().SelectMany( t => t.DeclaredProperties ).FirstOrDefault( p => String.Equals( p.Name, sig.Name ) );
      if ( namedElement == null )
      {
         throw new InvalidOperationException( "Failed to resolve " + ( sig.IsField ? "field" : "property" ) + " named \"" + sig.Name + "\" at " + caIdx + "." );
      }

      return CILCustomAttributeFactory.NewNamedArgument( namedElement, state.CreateCATypedArg( isField ? ( (CILField) namedElement ).FieldType : ( (CILProperty) namedElement ).GetPropertyType(), sig.Value.Value ) );
   }

   private static CILType ResolveCAType( this LogicalCreationState state, CustomAttributeArgumentType sigType )
   {
      switch ( sigType.ArgumentTypeKind )
      {
         case CustomAttributeArgumentTypeKind.Simple:
            return state.ResolveSimpleType( ( (CustomAttributeArgumentTypeSimple) sigType ).SimpleType );
         case CustomAttributeArgumentTypeKind.Array:
            return state.ResolveCAType( ( (CustomAttributeArgumentTypeArray) sigType ).ArrayType ).MakeArrayType();
         case CustomAttributeArgumentTypeKind.TypeString:
            return state.ResolveTypeString( ( (CustomAttributeArgumentTypeEnum) sigType ).TypeString, false );
         default:
            throw new InvalidOperationException( "Unsupported custom attribute argument type: " + sigType.ArgumentTypeKind + "." );
      }
   }

   private static void CreateLogicalIL( this LogicalCreationState state, Int32 mDefIdx )
   {
      var physicalIL = state.MetaData.MethodDefinitions.TableContents[mDefIdx].IL;
      if ( physicalIL != null )
      {
         var method = state.GetMethodDef( mDefIdx );
         var contextType = method.DeclaringType;
         var retVal = method.MethodIL;
         var physOpCodes = physicalIL.OpCodes;
         var logicalByteOffsets = new Dictionary<Int32, Int32>( physOpCodes.Count );
         var labelByteOffsets = new Dictionary<ILLabel, Int32>();
         var curByteOffset = 0;
         var md = state.MetaData;
         var methodSpecs = md.MethodSpecifications.TableContents;

         for ( var codeIdx = 0; codeIdx < physOpCodes.Count; ++codeIdx )
         {
            var code = physOpCodes[codeIdx];
            logicalByteOffsets.Add( curByteOffset, codeIdx );
            curByteOffset += code.GetTotalByteCount();
            LogicalOpCodeInfo logicalCode;
            switch ( code.InfoKind )
            {
               case OpCodeOperandKind.OperandInteger:
                  var physIntCode = (OpCodeInfoWithInt32) code;
                  switch ( code.OpCode.OperandType )
                  {
                     case OperandType.InlineBrTarget:
                     case OperandType.ShortInlineBrTarget:
                        var label = retVal.DefineLabel();
                        logicalCode = new LogicalOpCodeInfoForFixedBranchOrLeave( code.OpCode, label );
                        labelByteOffsets.Add( label, curByteOffset + physIntCode.Operand );
                        break;
                     case OperandType.InlineI:
                     case OperandType.InlineVar:
                        logicalCode = new LogicalOpCodeInfoWithFixedSizeOperandInt32( code.OpCode, physIntCode.Operand );
                        break;
                     case OperandType.ShortInlineI:
                     case OperandType.ShortInlineVar:
                        logicalCode = new LogicalOpCodeInfoWithFixedSizeOperandUInt16( code.OpCode, (Int16) physIntCode.Operand );
                        break;
                     default:
                        throw new InvalidOperationException( "Unsupported op code for physical op code with integer: " + code.OpCode + "." );
                  }
                  break;
               case OpCodeOperandKind.OperandInteger64:
                  logicalCode = new LogicalOpCodeInfoWithFixedSizeOperandInt64( code.OpCode, ( (OpCodeInfoWithInt64) code ).Operand );
                  break;
               case OpCodeOperandKind.OperandNone:
                  logicalCode = LogicalOpCodeInfoWithNoOperand.GetInstanceFor( code.OpCode );
                  break;
               case OpCodeOperandKind.OperandR4:
                  logicalCode = new LogicalOpCodeInfoWithFixedSizeOperandSingle( code.OpCode, ( (OpCodeInfoWithSingle) code ).Operand );
                  break;
               case OpCodeOperandKind.OperandR8:
                  logicalCode = new LogicalOpCodeInfoWithFixedSizeOperandDouble( code.OpCode, ( (OpCodeInfoWithDouble) code ).Operand );
                  break;
               case OpCodeOperandKind.OperandString:
                  logicalCode = new LogicalOpCodeInfoWithFixedSizeOperandString( code.OpCode, ( (OpCodeInfoWithString) code ).Operand );
                  break;
               case OpCodeOperandKind.OperandSwitch:
                  var physSwitch = (OpCodeInfoWithSwitch) code;
                  var labels = retVal.DefineLabels( physSwitch.Offsets.Count );
                  logicalCode = new LogicalOpCodeInfoForSwitch( labels );
                  for ( var i = 0; i < labels.Length; ++i )
                  {
                     labelByteOffsets.Add( labels[i], curByteOffset + physSwitch.Offsets[i] );
                  }
                  break;
               case OpCodeOperandKind.OperandToken:
                  var token = ( (OpCodeInfoWithToken) code ).Operand;
                  var resolved = state.ResolveToken( token, contextType, method );
                  switch ( resolved.ElementTypeKind )
                  {
                     case CILElementWithinILCode.Field:
                        var field = (CILField) resolved;
                        logicalCode = new LogicalOpCodeInfoWithFieldToken( code.OpCode, field, contextType.GetTypeTokenKind( field.DeclaringType, token.Table, Tables.Field ) );
                        break;
                     case CILElementWithinILCode.Method:
                        if ( resolved is CILMethod )
                        {
                           var cilMethod = (CILMethod) resolved;
                           var typeTable = state.GetTypeTableFromMethodDefOrMemberRef( token.Table == Tables.MethodSpec ? methodSpecs[token.Index].Method : token );
                           logicalCode = new LogicalOpCodeInfoWithMethodToken( code.OpCode, cilMethod, contextType.GetTypeTokenKind( cilMethod.DeclaringType, typeTable, Tables.TypeDef ), method.GetMethodTokenKind( cilMethod, token.Table ) );
                        }
                        else
                        {
                           var ctor = (CILConstructor) resolved;
                           logicalCode = new LogicalOpCodeInfoWithCtorToken( code.OpCode, ctor, contextType.GetTypeTokenKind( ctor.DeclaringType, token.Table, Tables.MethodDef ) );
                        }
                        break;
                     case CILElementWithinILCode.Type:
                        logicalCode = new LogicalOpCodeInfoWithTypeToken( code.OpCode, (CILTypeBase) resolved, contextType.GetTypeTokenKind( resolved as CILType, token.Table, Tables.TypeDef ) );
                        break;
                     default:
                        throw new InvalidOperationException( "Unrecognized tokenizable element kind: " + resolved.ElementTypeKind + "." );
                  }
                  break;
               default:
                  throw new InvalidOperationException( "Unsupported physical op code kind: " + code.InfoKind + "." );
            }

            retVal.Add( logicalCode );
         }

         // Mark labels
         foreach ( var kvp in labelByteOffsets )
         {
            var codeOffset = MapByteOffsetToCodeOffset( logicalByteOffsets, kvp.Value );
            retVal.MarkLabel( kvp.Key, codeOffset );
         }

         // Create locals
         var localsIdx = physicalIL.LocalsSignatureIndex;
         if ( localsIdx.HasValue )
         {
            foreach ( var local in state.ResolveLocalsSignature( physicalIL.LocalsSignatureIndex.Value.Index, contextType, method ) )
            {
               retVal.DeclareLocal( local.Item1, local.Item2 );
            }
         }

         retVal.InitLocals = physicalIL.InitLocals;
         // Create exception blocks
         foreach ( var excBlock in physicalIL.ExceptionBlocks )
         {
            var tryOffset = MapByteOffsetToCodeOffset( logicalByteOffsets, excBlock.TryOffset );
            var handlerOffset = MapByteOffsetToCodeOffset( logicalByteOffsets, excBlock.HandlerOffset );
            retVal.AddExceptionBlockInfo( new ExceptionBlockInfo(
               tryOffset,
               MapByteOffsetToCodeOffset( logicalByteOffsets, excBlock.TryOffset + excBlock.TryLength ) - tryOffset,
               handlerOffset,
               MapByteOffsetToCodeOffset( logicalByteOffsets, excBlock.HandlerOffset + excBlock.HandlerLength ) - handlerOffset,
               excBlock.ExceptionType.HasValue ? state.ResolveTypeDefOrRefOrSpec( excBlock.ExceptionType.Value, contextType, method ) : null,
               MapByteOffsetToCodeOffset( logicalByteOffsets, excBlock.FilterOffset ),
               excBlock.BlockType
               ) );
         }
      }
   }

   private static Tables GetTypeTableFromMethodDefOrMemberRef( this LogicalCreationState state, TableIndex token )
   {
      switch ( token.Table )
      {
         case Tables.MethodDef:
            return Tables.TypeDef;
         case Tables.MemberRef:
            return state.MetaData.MemberReferences.TableContents[token.Index].DeclaringType.Table;
         default:
            throw new InvalidOperationException( "Given table index was not method def or member ref." );
      }
   }

   private static Int32 MapByteOffsetToCodeOffset( IDictionary<Int32, Int32> logicalByteOffsets, Int32 byteOffset )
   {
      Int32 codeOffset;
      if ( !logicalByteOffsets.TryGetValue( byteOffset, out codeOffset ) )
      {
         //throw new InvalidOperationException( "Branch offset does not point to the start of the op code." );
         // Instead of throwing, just set to -1. Because stuff in reference assemblies folder tends to have branches which go outside the actual code...
         codeOffset = -1;
      }
      return codeOffset;
   }

   private static T GetOrAdd_NotThreadSafe<T>( this T[] array, Int32 index, Func<Int32, T> factory )
      where T : class
   {
      var retVal = array[index];
      if ( retVal == null )
      {
         retVal = factory( index );
         array[index] = retVal;
      }
      return retVal;
   }

   private static T[] PopulateWithNulls<T>( Int32 count )
      where T : class
   {
      var retVal = new T[count];
      retVal.FillWithNulls();
      return retVal;
   }

   internal static MethodTokenKind GetMethodTokenKind( this CILMethodBase thisMethod, CILMethod method, Tables resolvedTable )
   {
      return Equals( thisMethod, method ) && method.IsGenericMethodDefinition() && Tables.MethodDef == resolvedTable ? MethodTokenKind.GenericDefinition : MethodTokenKind.GenericInstantiation;
   }

   internal static TypeTokenKind GetTypeTokenKind( this CILType thisType, CILType otherType, Tables resolvedTable, Tables gDefTable )
   {
      return thisType.CanBeTypeTokenKind_GenericDefinition( otherType ) && resolvedTable == gDefTable ? TypeTokenKind.GenericDefinition : TypeTokenKind.GenericInstantiation;
   }

   private static Boolean CanBeTypeTokenKind_GenericDefinition( this CILType thisType, CILTypeBase otherType )
   {
      return Equals( otherType, thisType ) && ( (CILType) otherType ).IsGenericTypeDefinition();
   }

   private static Boolean IsTopLevelType( this LogicalAssemblyCreationResult creationResult, Int32 tDefIndex )
   {
      Int32 declTypeIdx;
      return !creationResult.TryGetDeclaringType( tDefIndex, out declTypeIdx );
   }

   internal static String ResolveType( this LogicalAssemblyCreationResult creationResult, CILType type )
   {
      return creationResult.ResolveTypeInfo( type ).Key;
   }

   internal static TypeDefinition LogicalToPhysical( this LogicalAssemblyCreationResult creationResult, CILType type )
   {
      return creationResult.GetAnyTypeDef( creationResult.ResolveTypeInfo( type ).Value );
   }
}
