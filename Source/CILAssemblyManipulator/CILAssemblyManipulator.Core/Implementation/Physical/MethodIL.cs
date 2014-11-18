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
using CILAssemblyManipulator.API;
using CILAssemblyManipulator.Implementation;
using CommonUtils;

namespace CILAssemblyManipulator.Implementation.Physical
{
   internal class MethodILImpl : MethodIL
   {
      internal const Int32 NO_OFFSET = -1;

      internal readonly CILReflectionContextImpl context;
      internal readonly CILModule _module;
      internal readonly IList<Int32> _labelOffsets;
      internal readonly IList<OpCodeInfo> _opCodes;
      internal readonly IList<ExceptionBlockInfo> _allExceptionBlocks;
      internal readonly Stack<ExceptionBlockInfo> _currentExceptionBlocks;
      internal readonly IList<LocalBuilder> _locals;
      internal Int32 _branchTargetsCount;
      private Boolean _initLocals;

      internal MethodILImpl( CILModule module )
      {
         ArgumentValidator.ValidateNotNull( "Module", module );

         this._module = module;
         this.context = (CILReflectionContextImpl) module.ReflectionContext;
         this._opCodes = new List<OpCodeInfo>();
         this._labelOffsets = new List<Int32>();
         this._allExceptionBlocks = new List<ExceptionBlockInfo>();
         this._currentExceptionBlocks = new Stack<ExceptionBlockInfo>();
         this._locals = new List<LocalBuilder>();
         this._branchTargetsCount = 0;
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
         Func<ILLabel> ilLabelDefiner = () => this.DefineLabel();
         while ( ilOffset < il.Length )
         {
            codeInfoILOffsets.Add( ilOffset, this._opCodes.Count );

            var curInstruction = (Int32) il[ilOffset++];
            if ( curInstruction == OpCode.MAX_ONE_BYTE_INSTRUCTION )
            {
               curInstruction = ( curInstruction << 8 ) | (Int32) il[ilOffset++];
            }

            var code = OpCodes.Codes[(OpCodeEncoding) curInstruction];
            Int32 int32;
            ILLabel label;
            Object resolvedToken;
            switch ( code.OperandType )
            {
               case OperandType.InlineNone:
                  this._opCodes.Add( OpCodes.CodeInfosWithNoOperand[(OpCodeEncoding) curInstruction] );
                  break;
               case OperandType.ShortInlineBrTarget:
                  int32 = ilOffset + 1 + (Int32) ( (SByte) il[ilOffset] );
                  ++ilOffset;
                  label = labelsDic.GetOrAdd_NotThreadSafe( int32, ilLabelDefiner );
                  this._opCodes.Add( new OpCodeInfoForFixedBranchOrLeave( code, label ) );//  OpCodes.Leave_S == code ? (OpCodeInfo) new OpCodeInfoForLeave( label ) : new OpCodeInfoForBranch( code, true, label ) );
                  ++this._branchTargetsCount;
                  break;
               case OperandType.ShortInlineI:
                  this._opCodes.Add( new OpCodeInfoWithFixedSizeOperandInt32( code, (SByte) il[ilOffset++] ) );
                  break;
               case OperandType.ShortInlineVar:
                  this._opCodes.Add( new OpCodeInfoWithFixedSizeOperandUInt16( code, il[ilOffset++] ) );
                  break;
               case OperandType.ShortInlineR:
                  this._opCodes.Add( new OpCodeInfoWithFixedSizeOperandSingle( code, il.ReadSingleLEFromBytes( ref ilOffset ) ) );
                  break;
               case OperandType.InlineBrTarget:
                  int32 = il.ReadInt32LEFromBytes( ref ilOffset );
                  int32 += ilOffset;
                  label = labelsDic.GetOrAdd_NotThreadSafe( int32, ilLabelDefiner );
                  this._opCodes.Add( new OpCodeInfoForFixedBranchOrLeave( code, label ) );// OpCodes.Leave == code ? (OpCodeInfo) new OpCodeInfoForLeave( label ) : new OpCodeInfoForBranch( code, false, label ) );
                  ++this._branchTargetsCount;
                  break;
               case OperandType.InlineI:
                  this._opCodes.Add( new OpCodeInfoWithFixedSizeOperandInt32( code, il.ReadInt32LEFromBytes( ref ilOffset ) ) );
                  break;
               case OperandType.InlineVar:
                  this._opCodes.Add( new OpCodeInfoWithFixedSizeOperandUInt16( code, il.ReadInt16LEFromBytes( ref ilOffset ) ) );
                  break;
               case OperandType.InlineR:
                  this._opCodes.Add( new OpCodeInfoWithFixedSizeOperandDouble( code, il.ReadDoubleLEFromBytes( ref ilOffset ) ) );
                  break;
               case OperandType.InlineI8:
                  this._opCodes.Add( new OpCodeInfoWithFixedSizeOperandInt64( code, il.ReadInt64LEFromBytes( ref ilOffset ) ) );
                  break;
               case OperandType.InlineString:
                  this._opCodes.Add( new OpCodeInfoWithFixedSizeOperandString( (String) tokenResolver( il.ReadInt32LEFromBytes( ref ilOffset ), ILResolveKind.String ) ) );
                  break;
               case OperandType.InlineField:
                  this._opCodes.Add( new OpCodeInfoWithFieldToken( code, (CILField) tokenResolver( il.ReadInt32LEFromBytes( ref ilOffset ), ILResolveKind.Field ) ) );
                  break;
               case OperandType.InlineMethod:
                  resolvedToken = tokenResolver( il.ReadInt32LEFromBytes( ref ilOffset ), ILResolveKind.MethodBase );
                  // Because of base.X() calls to virtual methods, we can not use Call/Callvirt deduction here
                  this._opCodes.Add( resolvedToken is CILMethod ?
                     (OpCodeInfo) new OpCodeInfoWithMethodToken( code, (CILMethod) resolvedToken ) :
                     new OpCodeInfoWithCtorToken( code, (CILConstructor) resolvedToken ) );
                  //var isMethod = resolvedToken is CILMethod;
                  //var canEmitNormalOrVirtual = isMethod && OpCodes.Callvirt.Equals( code );
                  //var isCallOrVirt = canEmitNormalOrVirtual;
                  //if ( canEmitNormalOrVirtual )
                  //{
                  //   canEmitNormalOrVirtual = this._opCodes.Count == 0;
                  //   if ( canEmitNormalOrVirtual )
                  //   {
                  //      var last = this._opCodes[this._opCodes.Count - 1];
                  //      canEmitNormalOrVirtual = !( last is OpCodeInfoWithTypeToken ) || !OpCodes.Constrained_.Equals( ( (OpCodeInfoWithTypeToken) last ).Code );
                  //   }
                  //}
                  //else if ( isMethod )
                  //{
                  //   isCallOrVirt = OpCodes.Call.Equals( code );
                  //   canEmitNormalOrVirtual = isCallOrVirt || OpCodes.Ldftn.Equals( code ) || OpCodes.Ldvirtftn.Equals( code );
                  //}

                  //this._opCodes.Add(
                  //   canEmitNormalOrVirtual ?
                  //      ( isCallOrVirt ?
                  //         OpCodeInfoForNormalOrVirtual.OpCodeInfoForCall( (CILMethod) resolvedToken ) :
                  //         OpCodeInfoForNormalOrVirtual.OpCodeInfoForLdFtn( (CILMethod) resolvedToken ) ) :
                  //      ( isMethod ?
                  //         (OpCodeInfo) new OpCodeInfoWithMethodToken( code, (CILMethod) resolvedToken ) :
                  //         new OpCodeInfoWithCtorToken( code, (CILConstructor) resolvedToken ) )
                  //         );
                  break;
               case OperandType.InlineType:
                  this._opCodes.Add( new OpCodeInfoWithTypeToken( code, (CILTypeBase) tokenResolver( il.ReadInt32LEFromBytes( ref ilOffset ), ILResolveKind.Type ) ) );
                  break;
               case OperandType.InlineTok:
                  int32 = il.ReadInt32LEFromBytes( ref ilOffset );
                  Tables refTable; Int32 dummy;
                  TokenUtils.DecodeToken( int32, out refTable, out dummy );
                  ILResolveKind resolveKind;
                  switch ( refTable )
                  {
                     case Tables.TypeDef:
                     case Tables.TypeRef:
                     case Tables.TypeSpec:
                        resolveKind = ILResolveKind.Type;
                        break;
                     case Tables.Field:
                        resolveKind = ILResolveKind.Field;
                        break;
                     case Tables.MethodDef:
                     case Tables.MethodSpec:
                        resolveKind = ILResolveKind.MethodBase;
                        break;
                     case Tables.MemberRef:
                        resolveKind = ILResolveKind.MethodBaseOrField;
                        break;
                     default:
                        throw new BadImageFormatException( "Unknown inline token in IL at offset " + ilOffset + " (" + new TableIndex( int32 ) + ")" );
                  }
                  resolvedToken = tokenResolver( int32, resolveKind );
                  this._opCodes.Add( resolvedToken is CILTypeBase ?
                     new OpCodeInfoWithTypeToken( code, (CILTypeBase) resolvedToken, ( Tables.TypeDef == refTable || Tables.TypeRef == refTable ) ) :
                     ( resolvedToken is CILField ?
                        new OpCodeInfoWithFieldToken( code, (CILField) resolvedToken, true ) : // TODO the last boolean parameter properly (how?)
                        ( resolvedToken is CILMethod ?
                           (OpCodeInfo) new OpCodeInfoWithMethodToken( code, (CILMethod) resolvedToken, ( Tables.MemberRef == refTable || Tables.MethodDef == refTable ) ) : // TODO last boolean parameter propertly (how?)
                           new OpCodeInfoWithCtorToken( code, (CILConstructor) resolvedToken, true ) // TODO last boolean parameter propertly (how?)
                        )
                     )
                     );
                  break;
               case OperandType.InlineSwitch:
                  int32 = il.ReadInt32LEFromBytes( ref ilOffset );
                  ILLabel[] labels = new ILLabel[int32];
                  for ( var i = 0; i < int32; ++i )
                  {
                     var lTarget = il.ReadInt32LEFromBytes( ref ilOffset );
                     lTarget += ilOffset + ( int32 - i - 1 ) * 4;
                     labels[i] = labelsDic.GetOrAdd_NotThreadSafe( lTarget, () => this.DefineLabel() );
                  }
                  this._opCodes.Add( new OpCodeInfoForSwitch( labels ) );
                  this._branchTargetsCount += int32;
                  break;
               case OperandType.InlineSig:
                  int32 = il.ReadInt32LEFromBytes( ref ilOffset );
                  var methodSig = (Tuple<CILMethodSignature, Tuple<CILCustomModifier[], CILTypeBase>[]>) tokenResolver( int32, ILResolveKind.Signature );
                  this._opCodes.Add( new OpCodeInfoWithMethodSig( methodSig.Item1, methodSig.Item2 ) );
                  break;
               default:
                  throw new ArgumentException( "Unknown operand type: " + code.OperandType + " for " + code + "." );
            }
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

      public MethodIL Add( OpCodeInfo opCodeInfo )
      {
         ArgumentValidator.ValidateNotNull( "Op code info", opCodeInfo );
         this._opCodes.Add( opCodeInfo );
         this._branchTargetsCount += opCodeInfo.BranchTargetCount;
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
         this._labelOffsets[label.labelIdx] = idx;
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

      public IEnumerable<OpCodeInfo> OpCodeInfos
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

      public IEnumerable<Int32> LabelOffsets
      {
         get
         {
            return this._labelOffsets.Skip( 0 );
         }
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
            this.Add( OpCodes.CodeInfosWithNoOperand[OpCodeEncoding.Endfinally] );
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
         var idx = label.labelIdx;
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