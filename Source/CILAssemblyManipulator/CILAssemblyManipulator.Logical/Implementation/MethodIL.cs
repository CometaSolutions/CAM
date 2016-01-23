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
#if !NO_ALIASES
extern alias CAMPhysical;
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical.IO.Defaults;
#else
using CILAssemblyManipulator.Physical.IO.Defaults;
#endif

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

      private readonly CILModule _module;
      private readonly IList<Int32> _labelOffsets;
      private readonly IList<LogicalOpCodeInfo> _opCodes;
      private readonly IList<ExceptionBlockInfo> _allExceptionBlocks;
      private readonly Stack<Tuple<ExceptionBlockInfo, ILLabel>> _currentExceptionBlocks;
      private readonly IList<LocalBuilder> _locals;
      private Boolean _initLocals;

      internal MethodILImpl( CILModule module )
      {
         ArgumentValidator.ValidateNotNull( "Module", module );

         this._module = module;
         this._opCodes = new List<LogicalOpCodeInfo>();
         this._labelOffsets = new List<Int32>();
         this._allExceptionBlocks = new List<ExceptionBlockInfo>();
         this._currentExceptionBlocks = new Stack<Tuple<ExceptionBlockInfo, ILLabel>>();
         this._locals = new List<LocalBuilder>();
         this._initLocals = true; // Maybe set to false by default?
      }

      internal MethodILImpl( CILMethodBase thisMethod, Boolean initLocals, Byte[] il, Tuple<Boolean, CILTypeBase>[] locals, Tuple<ExceptionBlockType, Int32, Int32, Int32, Int32, CILType, Int32>[] excBlocks, Func<Int32, ILResolveKind, Object> tokenResolver )
         : this( thisMethod.DeclaringType.Module )
      {
         var thisType = thisMethod.DeclaringType;
         this._initLocals = initLocals;
         foreach ( var lInfo in locals )
         {
            this.DeclareLocal( lInfo.Item2, lInfo.Item1 );
         }
         var ilOffset = 0;
         var labelsDic = new Dictionary<Int32, ILLabel>();
         var codeInfoILOffsets = new Dictionary<Int32, Int32>();
         Func<Int32, ILLabel> ilLabelDefiner = i => this.DefineLabel();
         var ocp = CILAssemblyManipulator.Physical.Meta.DefaultOpCodeProvider.DefaultInstance;
         while ( ilOffset < il.Length )
         {
            codeInfoILOffsets.Add( ilOffset, this._opCodes.Count );

            var physical = ILSerialization.TryReadOpCode( il, ref ilOffset, t => (String) tokenResolver( t, ILResolveKind.String ), ocp );
            LogicalOpCodeInfo logical;
            Int32 int32;
            var codeID = physical.OpCodeID;
            var code = ocp.GetCodeFor( codeID );
            switch ( code.OperandType )
            {
               case OperandType.InlineNone:
                  logical = this._module.GetOperandlessOpCode( code.OpCodeID );
                  break;
               case OperandType.ShortInlineBrTarget:
               case OperandType.InlineBrTarget:
                  int32 = ilOffset + ( (OpCodeInfoWithInt32) physical ).Operand;
                  logical = new LogicalOpCodeInfoForFixedBranchOrLeave( codeID, labelsDic.GetOrAdd_NotThreadSafe( int32, ilLabelDefiner ) );
                  break;
               case OperandType.ShortInlineI:
               case OperandType.InlineI:
                  logical = new LogicalOpCodeInfoWithFixedSizeOperandInt32( codeID, ( (OpCodeInfoWithInt32) physical ).Operand );
                  break;
               case OperandType.ShortInlineVar:
               case OperandType.InlineVar:
                  logical = new LogicalOpCodeInfoWithFixedSizeOperandUInt16( codeID, (Int16) ( (OpCodeInfoWithInt32) physical ).Operand );
                  break;
               case OperandType.ShortInlineR:
                  logical = new LogicalOpCodeInfoWithFixedSizeOperandSingle( codeID, ( (OpCodeInfoWithSingle) physical ).Operand );
                  break;
               case OperandType.InlineR:
                  logical = new LogicalOpCodeInfoWithFixedSizeOperandDouble( codeID, ( (OpCodeInfoWithDouble) physical ).Operand );
                  break;
               case OperandType.InlineI8:
                  logical = new LogicalOpCodeInfoWithFixedSizeOperandInt64( codeID, ( (OpCodeInfoWithInt64) physical ).Operand );
                  break;
               case OperandType.InlineString:
                  logical = new LogicalOpCodeInfoWithFixedSizeOperandString( codeID, ( (OpCodeInfoWithString) physical ).Operand );
                  break;
               case OperandType.InlineField:
               case OperandType.InlineMethod:
               case OperandType.InlineType:
               case OperandType.InlineToken:
                  var tableIndex = ( (OpCodeInfoWithTableIndex) physical ).Operand;
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
                  var resolved = tokenResolver( tableIndex.GetOneBasedToken(), resolveKind );
                  switch ( ( (CILCustomAttributeContainerImpl) resolved ).cilKind )
                  {
                     case CILElementKind.Type:
                        logical = new LogicalOpCodeInfoWithTypeToken( codeID, (CILTypeBase) resolved, thisType.GetTypeTokenKind( resolved as CILType, table, Tables.TypeDef ) );
                        break;
                     case CILElementKind.Method:
                        var method = (CILMethod) resolved;
                        // Some loss of information here, but in order to extract TypeTokenKind precisely, we must be able to get method spec signature using System.Reflection API... not possible in PCL at least.
                        logical = new LogicalOpCodeInfoWithMethodToken( codeID, method, thisType.GetTypeTokenKind( method.DeclaringType, table, Tables.MethodDef ), thisMethod.GetMethodTokenKind( method, table ) );
                        break;
                     case CILElementKind.Field:
                        var field = (CILField) resolved;
                        logical = new LogicalOpCodeInfoWithFieldToken( codeID, field, thisType.GetTypeTokenKind( field.DeclaringType, table, Tables.Field ) );
                        break;
                     case CILElementKind.Constructor:
                        var ctor = (CILConstructor) resolved;
                        logical = new LogicalOpCodeInfoWithCtorToken( codeID, (CILConstructor) resolved, thisType.GetTypeTokenKind( ctor.DeclaringType, table, Tables.MethodDef ) );
                        break;
                     default:
                        throw new BadImageFormatException( "Token resolver resolved unsupported CIL element kind: " + ( (CILCustomAttributeContainerImpl) resolved ).cilKind + "." );
                  }
                  break;
               case OperandType.InlineSwitch:
                  var sw = (OpCodeInfoWithIntegers) physical;
                  logical = new LogicalOpCodeInfoForSwitch( sw.Operand
                     .Select( ( o, sIdx ) => labelsDic.GetOrAdd_NotThreadSafe( ilOffset + o, ilLabelDefiner ) )
                     .ToArray() );
                  break;
               case OperandType.InlineSignature:
                  var methodSig = (Tuple<CILMethodSignature, VarArgInstance[]>) tokenResolver( ( (OpCodeInfoWithTableIndex) physical ).Operand.GetOneBasedToken(), ILResolveKind.Signature );
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
            var info = new ExceptionBlockInfo( codeInfoILOffsets[block.Item2] );
            info.BlockType = block.Item1;
            info.TryLength = codeInfoILOffsets[block.Item2 + block.Item3] - info.TryOffset;
            info.HandlerOffset = codeInfoILOffsets[block.Item4];
            info.HandlerLength = codeInfoILOffsets[block.Item4 + block.Item5] - info.HandlerOffset;
            var excType = block.Item6;
            if ( excType != null )
            {
               info.ExceptionType = excType;
            }
            if ( codeInfoILOffsets.ContainsKey( block.Item7 ) )
            {
               info.FilterOffset = block.Item7;
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

      public LogicalOpCodeInfo GetOpCodeInfo( Int32 index )
      {
         return this._opCodes[index];
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

      public CILModule Module
      {
         get
         {
            return this._module;
         }
      }

      #endregion

      internal IList<LocalBuilder> LocalsList
      {
         get
         {
            return this._locals;
         }
      }

      internal ILLabel BeginExceptionBlock()
      {
         var info = new ExceptionBlockInfo( this._opCodes.Count );
         this._allExceptionBlocks.Add( info );

         var label = this.DefineLabel();
         this._currentExceptionBlocks.Push( Tuple.Create( info, label ) );
         return label;
      }

      internal void BeginCatchBlock( CILTypeBase exceptionType )
      {
         var info = this._currentExceptionBlocks.Peek().Item1;
         info.ExceptionType = exceptionType;
         info.HandlerBegun( ExceptionBlockType.Exception, this._opCodes.Count );
      }

      internal void BeginFinallyBlock()
      {
         this._currentExceptionBlocks.Peek().Item1.HandlerBegun( ExceptionBlockType.Finally, this._opCodes.Count );
      }

      internal void EndExceptionBlock()
      {
         var tuple = this._currentExceptionBlocks.Pop();
         var info = tuple.Item1;
         if ( ExceptionBlockType.Finally == info.BlockType || ExceptionBlockType.Fault == info.BlockType )
         {
            this.Add( this._module.GetOperandlessOpCode( OpCodeID.Endfinally ) );
         }

         this.MarkLabel( tuple.Item2 );
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