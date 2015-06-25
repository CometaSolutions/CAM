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
using System.Linq;
using CommonUtils;
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Logical.Implementation
{
   internal class MethodILImpl : MethodIL
   {
      internal const Int32 NO_OFFSET = -1;

      private readonly CILReflectionContextImpl context;
      private readonly CILModule _module;
      private readonly IList<Int32> _labelOffsets;
      private readonly IList<LogicalOpCodeInfo> _opCodes;
      private readonly IList<ExceptionBlockInfo> _allExceptionBlocks;
      private readonly Stack<ExceptionBlockInfo> _currentExceptionBlocks;
      private readonly IList<LocalBuilder> _locals;
      private Boolean _initLocals;

      internal MethodILImpl( CILModule module )
      {
         ArgumentValidator.ValidateNotNull( "Module", module );

         this._module = module;
         this.context = (CILReflectionContextImpl) module.ReflectionContext;
         this._opCodes = new List<LogicalOpCodeInfo>();
         this._labelOffsets = new List<Int32>();
         this._allExceptionBlocks = new List<ExceptionBlockInfo>();
         this._currentExceptionBlocks = new Stack<ExceptionBlockInfo>();
         this._locals = new List<LocalBuilder>();
         this._initLocals = true; // Maybe set to false by default?
      }

      internal MethodILImpl( CILModule module, MethodBodyLoadArgs args )
         : this( module, args.InitLocals, args.IL, GetLocalsFromArgs( (CILReflectionContextImpl) module.ReflectionContext, args ), args.ExceptionInfos.Select( tuple => Tuple.Create( tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6 == null ? null : tuple.Item6.NewWrapperAsType( (CILReflectionContextImpl) module.ReflectionContext ), tuple.Item7 ) ).ToArray(), TokenResolverFromArgs( (CILReflectionContextImpl) module.ReflectionContext, args ) )
      {
      }

      private static Tuple<Boolean, CILTypeBase>[] GetLocalsFromArgs( CILReflectionContextImpl ctx, MethodBodyLoadArgs args )
      {
         var result = new Tuple<Boolean, CILTypeBase>[args.Locals.Count];
         for ( var i = 0; i < result.Length; ++i )
         {
            result[i] = Tuple.Create( args.Locals[i].Item1, args.Locals[i].Item2.NewWrapper( ctx ) );
         }
         return result;
      }

      private static Func<Int32, ILResolveKind, Object> TokenResolverFromArgs( CILReflectionContextImpl ctx, MethodBodyLoadArgs args )
      {
         var module = new Lazy<System.Reflection.Module>( () => ctx.LaunchTypeModuleLoadEvent( new TypeModuleEventArgs( args.Method.DeclaringType ) ), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );

         var dType = args.Method.DeclaringType;
         var gArgs = dType
#if WINDOWS_PHONE_APP
            .GetTypeInfo()
#endif
.IsGenericType ? dType.GetGenericTypeDefinition().GetGenericArguments() : null;
         var mgArgs = args.Method is System.Reflection.MethodInfo && args.Method.GetGenericArguments().Any() ? ( (System.Reflection.MethodInfo) args.Method ).GetGenericMethodDefinition().GetGenericArguments() : null;

         return ( token, rKind ) =>
         {
            var tArgs = new TokenResolveArgs( module.Value, token, rKind == ILResolveKind.String ? TokenResolveArgs.ResolveKinds.String : ( rKind == ILResolveKind.Signature ? TokenResolveArgs.ResolveKinds.Signature : TokenResolveArgs.ResolveKinds.Member ), gArgs, mgArgs );
            ctx.LaunchTokenResolveEvent( tArgs );
            switch ( rKind )
            {
               case ILResolveKind.String:
                  return tArgs.ResolvedString;
               case ILResolveKind.Signature:
                  // TODO basically same thing as in ModuleReadingContext, except use this method to resolve tokens.
                  // Maybe could make static methods to ModuleReadingContext (ReadFieldSignature, ReadMethodSignature, ReadType) that would use callbacks to resolve tokens.
                  throw new NotImplementedException( "Implement creating method signature + var args from byte array (at this point)." );
               default:
                  return tArgs.ResolvedMember is Type ?
                     ctx.Cache.GetOrAdd( (Type) tArgs.ResolvedMember ) :
                        ( tArgs.ResolvedMember is System.Reflection.FieldInfo ?
                           ctx.Cache.GetOrAdd( (System.Reflection.FieldInfo) tArgs.ResolvedMember ) :
                              ( tArgs.ResolvedMember is System.Reflection.MethodInfo ?
                                 (Object) ctx.Cache.GetOrAdd( (System.Reflection.MethodInfo) tArgs.ResolvedMember ) :
                                 ctx.Cache.GetOrAdd( (System.Reflection.ConstructorInfo) tArgs.ResolvedMember )
                                 )
                            );
            }
         };
      }

      internal MethodILImpl( CILModule module, Boolean initLocals, Byte[] il, Tuple<Boolean, CILTypeBase>[] locals, Tuple<ExceptionBlockType, Int32, Int32, Int32, Int32, CILType, Int32>[] excBlocks, Func<Int32, ILResolveKind, Object> tokenResolver )
         : this( module )
      {
         this._initLocals = initLocals;
         foreach ( var lInfo in locals )
         {
            this.DeclareLocal( lInfo.Item2, lInfo.Item1 );
         }
         var ilOffset = 0;
         var labelsDic = new Dictionary<Int32, ILLabel>();
         var codeInfoILOffsets = new Dictionary<Int32, Int32>();
         Func<Int32, ILLabel> ilLabelDefiner = i => this.DefineLabel();
         while ( ilOffset < il.Length )
         {
            codeInfoILOffsets.Add( ilOffset, this._opCodes.Count );

            var physical = OpCodeInfo.ReadFromBytes( il, ref ilOffset, t => (String) tokenResolver( t, ILResolveKind.String ) );
            LogicalOpCodeInfo logical;
            Int32 int32;
            var code = physical.OpCode;
            switch ( code.OperandType )
            {
               case OperandType.InlineNone:
                  logical = LogicalOpCodeInfoWithNoOperand.GetInstanceFor( code );
                  break;
               case OperandType.ShortInlineBrTarget:
               case OperandType.InlineBrTarget:
                  int32 = ilOffset + ( (OpCodeInfoWithInt32) physical ).Operand;
                  logical = new LogicalOpCodeInfoForFixedBranchOrLeave( code, labelsDic.GetOrAdd_NotThreadSafe( int32, ilLabelDefiner ) );
                  break;
               case OperandType.ShortInlineI:
               case OperandType.InlineI:
                  logical = new LogicalOpCodeInfoWithFixedSizeOperandInt32( code, ( (OpCodeInfoWithInt32) physical ).Operand );
                  break;
               case OperandType.ShortInlineVar:
               case OperandType.InlineVar:
                  logical = new LogicalOpCodeInfoWithFixedSizeOperandUInt16( code, (Int16) ( (OpCodeInfoWithInt32) physical ).Operand );
                  break;
               case OperandType.ShortInlineR:
                  logical = new LogicalOpCodeInfoWithFixedSizeOperandSingle( code, ( (OpCodeInfoWithSingle) physical ).Operand );
                  break;
               case OperandType.InlineR:
                  logical = new LogicalOpCodeInfoWithFixedSizeOperandDouble( code, ( (OpCodeInfoWithDouble) physical ).Operand );
                  break;
               case OperandType.InlineI8:
                  logical = new LogicalOpCodeInfoWithFixedSizeOperandInt64( code, ( (OpCodeInfoWithInt64) physical ).Operand );
                  break;
               case OperandType.InlineString:
                  logical = new LogicalOpCodeInfoWithFixedSizeOperandString( code, ( (OpCodeInfoWithString) physical ).Operand );
                  break;
               case OperandType.InlineField:
               case OperandType.InlineMethod:
               case OperandType.InlineType:
               case OperandType.InlineTok:
                  var tableIndex = ( (OpCodeInfoWithToken) physical ).Operand;
                  ILResolveKind resolveKind;
                  var table = tableIndex.Table;
                  switch ( table )
                  {
                     case Tables.Field:
                        resolveKind = ILResolveKind.Field;
                        break;
                     case Tables.MethodDef:
                     case Tables.MethodSpec:
                        resolveKind = ILResolveKind.MethodBase;
                        break;
                     case Tables.TypeDef:
                     case Tables.TypeRef:
                     case Tables.TypeSpec:
                        resolveKind = ILResolveKind.Type;
                        break;
                     case Tables.MemberRef:
                        resolveKind = ILResolveKind.MethodBaseOrField;
                        break;
                     default:
                        throw new BadImageFormatException( "Unknown inline token in IL at offset " + ilOffset + " (" + tableIndex + ")." );
                  }
                  var resolved = tokenResolver( tableIndex.OneBasedToken, resolveKind );
                  switch ( ( (CILCustomAttributeContainerImpl) resolved ).cilKind )
                  {
                     case CILElementKind.Type:
                        logical = new LogicalOpCodeInfoWithTypeToken( code, (CILTypeBase) resolved, Tables.TypeDef == table || Tables.TypeRef == table );
                        break;
                     case CILElementKind.Method:
                        // TODO last parameter properly
                        logical = new LogicalOpCodeInfoWithMethodToken( code, (CILMethod) resolved, Tables.MemberRef == table || Tables.MethodDef == table );
                        break;
                     case CILElementKind.Field:
                        // TODO last parameter properly
                        logical = new LogicalOpCodeInfoWithFieldToken( code, (CILField) resolved, code.OperandType != OperandType.InlineField );
                        break;
                     case CILElementKind.Constructor:
                        // TODO last parameter properly
                        logical = new LogicalOpCodeInfoWithCtorToken( code, (CILConstructor) resolved, code.OperandType != OperandType.InlineMethod );
                        break;
                     default:
                        throw new BadImageFormatException( "Token resolver resolved unsupported CIL element kind: " + ( (CILCustomAttributeContainerImpl) resolved ).cilKind + "." );
                  }
                  break;
               case OperandType.InlineSwitch:
                  var sw = (OpCodeInfoWithSwitch) physical;
                  logical = new LogicalOpCodeInfoForSwitch( sw.Offsets
                     .Select( ( o, sIdx ) => labelsDic.GetOrAdd_NotThreadSafe( ilOffset + o, ilLabelDefiner ) )
                     .ToArray() );
                  break;
               case OperandType.InlineSig:
                  var methodSig = (Tuple<CILMethodSignature, Tuple<CILCustomModifier[], CILTypeBase>[]>) tokenResolver( ( (OpCodeInfoWithToken) physical ).Operand.OneBasedToken, ILResolveKind.Signature );
                  logical = new LogicalOpCodeInfoWithMethodSig( methodSig.Item1, methodSig.Item2 );
                  break;
               default:
                  throw new ArgumentException( "Unknown operand type: " + code.OperandType + " for " + code + "." );
            }

            this._opCodes.Add( logical );
         }

         // Then, process labels
         foreach ( var kvp in labelsDic )
         {
            this.MarkLabel( kvp.Value, codeInfoILOffsets[kvp.Key] );
         }

         // And exception blocks
         foreach ( var block in excBlocks )
         {
            var info = new ExceptionBlockInfo( codeInfoILOffsets[block.Item2], labelsDic.GetOrAdd_NotThreadSafe( block.Item4 + block.Item5, () =>
            {
               var result = this.DefineLabel();
               this.MarkLabel( result, codeInfoILOffsets[block.Item4 + block.Item5] );
               return result;
            } ) );
            info._blockType = block.Item1;
            info._tryLength = codeInfoILOffsets[block.Item2 + block.Item3] - info._tryOffset;
            info._handlerOffset = codeInfoILOffsets[block.Item4];
            info._handlerLength = codeInfoILOffsets[block.Item4 + block.Item5] - info._handlerOffset;
            var excType = block.Item6;
            if ( excType != null )
            {
               info._exceptionType = excType;
            }
            if ( codeInfoILOffsets.ContainsKey( block.Item7 ) )
            {
               info._filterOffset = block.Item7;
            }
            this._allExceptionBlocks.Add( info );
         }
      }

      #region MethodIL Members

      public MethodIL Add( LogicalOpCodeInfo opCodeInfo )
      {
         ArgumentValidator.ValidateNotNull( "Op code info", opCodeInfo );
         this._opCodes.Add( opCodeInfo );
         return this;
      }

      public Int32 OpCodeCount
      {
         get
         {
            return this._opCodes.Count;
         }
      }

      public ILLabel DefineLabel()
      {
         var result = new ILLabel( this._labelOffsets.Count );
         this._labelOffsets.Add( NO_OFFSET );
         return result;
      }

      public MethodIL MarkLabel( ILLabel label, Int32 idx )
      {
         this.CheckLabel( label );
         this._labelOffsets[label.LabelIndex] = idx;
         return this;
      }

      public LocalBuilder DeclareLocal( CILTypeBase type, Boolean pinned = false )
      {
         LogicalUtils.CheckTypeForMethodSig( this._module, ref type );
         var result = new LocalBuilderImpl( this._locals.Count, type, pinned );
         this._locals.Add( result );
         return result;
      }

      public Boolean InitLocals
      {
         get
         {
            return this._initLocals;
         }
         set
         {
            this._initLocals = value;
         }
      }

      public IEnumerable<LogicalOpCodeInfo> OpCodeInfos
      {
         get
         {
            return this._opCodes.Skip( 0 );
         }
      }

      public Int32 LabelCount
      {
         get
         {
            return this._labelOffsets.Count;
         }
      }

      public Int32 GetLabelOffset( ILLabel label )
      {
         return label.LabelIndex < this._labelOffsets.Count ?
            this._labelOffsets[label.LabelIndex] :
            -1;
      }

      public IEnumerable<LocalBuilder> Locals
      {
         get
         {
            return this._locals.Skip( 0 );
         }
      }

      public IEnumerable<ExceptionBlockInfo> ExceptionBlocks
      {
         get
         {
            return this._allExceptionBlocks.Skip( 0 );
         }
      }

      public void AddExceptionBlockInfo( ExceptionBlockInfo block )
      {
         ArgumentValidator.ValidateNotNull( "Block", block );
         this._allExceptionBlocks.Add( block );
      }

      #endregion

      internal CILModule OwningModule
      {
         get
         {
            return this._module;
         }
      }

      internal ILLabel BeginExceptionBlock()
      {
         var label = this.DefineLabel();
         var info = new ExceptionBlockInfo( this._opCodes.Count, label );
         this._allExceptionBlocks.Add( info );
         this._currentExceptionBlocks.Push( info );
         return label;
      }

      internal void BeginCatchBlock( CILTypeBase exceptionType )
      {
         var info = this._currentExceptionBlocks.Peek();
         info._exceptionType = exceptionType;
         info.HandlerBegun( ExceptionBlockType.Exception, this._opCodes.Count );
      }

      internal void BeginFinallyBlock()
      {
         this._currentExceptionBlocks.Peek().HandlerBegun( ExceptionBlockType.Finally, this._opCodes.Count );
      }

      internal void EndExceptionBlock()
      {
         var info = this._currentExceptionBlocks.Pop();
         if ( ExceptionBlockType.Finally == info._blockType || ExceptionBlockType.Fault == info._blockType )
         {
            this.Add( LogicalOpCodeInfoWithNoOperand.GetInstanceFor( OpCodeEncoding.Endfinally ) );
         }
         //else if ( ExceptionBlockType.Exception == info.blockType )
         //{
         //   // Let the caller decide whether to leave or rethrow
         //}
         this.MarkLabel( info._endLabel );
         info.HandlerEnded( this._opCodes.Count );
      }

      private void CheckLabel( ILLabel label )
      {
         var idx = label.LabelIndex;
         if ( idx < 0 || idx >= this._labelOffsets.Count )
         {
            throw new ArgumentException( "Invalid label with index " + idx );
         }
         if ( this._labelOffsets[idx] != NO_OFFSET )
         {
            throw new ArgumentException( "Label with index " + idx + " has already been defined at offset " + this._labelOffsets[idx] );
         }
      }
   }

   internal class LocalBuilderImpl : LocalBuilder
   {
      private readonly Int32 _idx;
      private readonly CILTypeBase _localType;
      private readonly Boolean _pinned;

      internal LocalBuilderImpl( Int32 idx, CILTypeBase type, Boolean pinned )
      {
         ArgumentValidator.ValidateNotNull( "Local type", type );
         this._idx = idx;
         this._localType = type;
         this._pinned = pinned;
      }

      #region LocalBuilder Members

      public Int32 LocalIndex
      {
         get
         {
            return this._idx;
         }
      }

      public CILTypeBase LocalType
      {
         get
         {
            return this._localType;
         }
      }

      public Boolean IsPinned
      {
         get
         {
            return this._pinned;
         }
      }

      #endregion

   }

   internal enum ILResolveKind
   {
      String,
      MethodBase,
      Type,
      Field,
      MethodBaseOrField,
      Signature
   }
}