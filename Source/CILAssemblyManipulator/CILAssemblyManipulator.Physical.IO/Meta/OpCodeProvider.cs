/*
 * Copyright 2016 Stanislav Muhametsin. All rights Reserved.
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
extern alias CAMPhysical;

using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;
using UtilPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilPack.Extension;
using UtilPack.Visiting;

using TOpCodeEqualityAcceptorSetup = UtilPack.Visiting.AutomaticTransitionAcceptor_WithContext<UtilPack.Visiting.AcceptorWithContext<CAMPhysical::CILAssemblyManipulator.Physical.IOpCodeInfo, CAMPhysical::CILAssemblyManipulator.Physical.IOpCodeInfo>, CAMPhysical::CILAssemblyManipulator.Physical.IOpCodeInfo, System.Int32, UtilPack.Visiting.ObjectGraphEqualityContext<CAMPhysical::CILAssemblyManipulator.Physical.IOpCodeInfo>>;
using TOpCodeEqualityAcceptor = UtilPack.Visiting.AcceptorWithContext<CAMPhysical::CILAssemblyManipulator.Physical.IOpCodeInfo, CAMPhysical::CILAssemblyManipulator.Physical.IOpCodeInfo>;
using TOpCodeHashCodeAcceptorSetup = UtilPack.Visiting.ManualTransitionAcceptor_WithReturnValue<UtilPack.Visiting.AcceptorWithReturnValue<CAMPhysical::CILAssemblyManipulator.Physical.IOpCodeInfo, System.Int32>, CAMPhysical::CILAssemblyManipulator.Physical.IOpCodeInfo, System.Int32>;
using TOpCodeHashCodeAcceptor = UtilPack.Visiting.AcceptorWithReturnValue<CAMPhysical::CILAssemblyManipulator.Physical.IOpCodeInfo, System.Int32>;
using TOpCodeCloningAcceptor = UtilPack.Visiting.AcceptorWithContextAndReturnValue<CAMPhysical::CILAssemblyManipulator.Physical.IOpCodeInfo, CAMPhysical::CILAssemblyManipulator.Physical.Meta.CopyingArgs, CAMPhysical::CILAssemblyManipulator.Physical.IOpCodeInfo>;
using TabularMetaData;
using UtilPack.CollectionsWithRoles;

namespace CILAssemblyManipulator.Physical.Meta
{
   // This interface will be merged with corresponding interface of CAM.Physical Core project.
#pragma warning disable 1591
   public interface OpCodeProvider
#pragma warning restore 1591
   {
      /// <summary>
      /// Tries to get <see cref="OpCodeSerializationInfo" /> for given <see cref="OpCodeID"/>.
      /// </summary>
      /// <param name="codeID">The <see cref="OpCodeID"/>.</param>
      /// <returns><see cref="OpCodeSerializationInfo"/> for given <paramref name="codeID"/>, or <c>null</c>.</returns>
      OpCodeSerializationInfo GetSerializationInfoOrNull( OpCode codeID );

      ///// <summary>
      ///// Tries to read <see cref="OpCodeID"/> from serialized form in given byte array.
      ///// </summary>
      ///// <param name="array">The byte array read from.</param>
      ///// <param name="index">The index in <paramref name="array"/>.</param>
      ///// <param name="info">This parameter will have the <see cref="OpCodeSerializationInfo"/> of the op code serialized at given index in given byte array.</param>
      ///// <returns><c>true</c> if this <see cref="OpCodeProvider"/> knew how to deserialize the op code; <c>false</c> otherwise.</returns>
      //Boolean TryReadOpCode( Byte[] array, Int32 index, out OpCodeSerializationInfo info );

      /// <summary>
      /// Creates instance of <see cref="SerializationFunctionality{TSerializationSink, TValue}"/> to be used to serialize op codes supported by this <see cref="OpCodeProvider"/>.
      /// </summary>
      /// <param name="args">The arguments encapsulating required information in order to create serializer.</param>
      /// <returns>Instance of <see cref="SerializationFunctionality{TSerializationSink, TValue}"/> to be used to serialize op codes.</returns>
      SerializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateSerializer( OpCodeSerializationArguments args );

      /// <summary>
      /// Creates instance of <see cref="DeserializationFunctionality{TDeserializationSource, TValue}"/> to be used to deserialize op codes supported by this <see cref="OpCodeProvider"/>.
      /// </summary>
      /// <param name="args">The arguments encapsulating required information in order to create deserializer.</param>
      /// <returns>Instance of <see cref="DeserializationFunctionality{TDeserializationSource, TValue}"/> to be used to deserialize op codes.</returns>
      DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateDeserializer( OpCodeDeserializationArguments args );

      /// <summary>
      /// Creates a type into given <see cref="CILMetaData"/> which implements this interface, and has a optimized version of object returned by <see cref="CreateDeserializer"/>.
      /// </summary>
      /// <param name="emittingHelper">The module to create type to.</param>
      /// <returns>Index into <see cref="CILMetaData.TypeDefinitions"/> table where created type resides.</returns>
      Int32 CreateOptimizedOpCodeProvider( EmittingNativeHelper emittingHelper );

      /// <summary>
      /// After the assembly that is processed by <see cref="CreateOptimizedOpCodeProvider"/> method has been serialized and loaded into CLR runtime, this method can be used to instantiate the type generated by <see cref="CreateOptimizedOpCodeProvider"/>.
      /// </summary>
      /// <param name="generatedType">The type generated by <see cref="CreateOptimizedOpCodeProvider"/> </param>
      /// <returns>An instance of optimized <see cref="OpCodeProvider"/>.</returns>
      CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider InstantiateOptimizedOpCodeProvider( Type generatedType );
   }

#pragma warning disable 1591

   /// <summary>
   /// This class encapsulates required information about single <see cref="OpCodeID"/> for the <see cref="DefaultOpCodeProvider"/>.
   /// </summary>
   public abstract class OpCodeSerializationInfo
   {

      protected abstract class AbstractOpCodeSerializer : SerializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo>
      {
         public AbstractOpCodeSerializer( OpCodeSerializationInfo info )
         {
            this.Info = ArgumentValidator.ValidateNotNull( "Info", info );
         }


         public Int32 Serialize( ArrayIndex<Byte> sink, IOpCodeInfo value )
         {
            var idx = sink.Index;
            var array = sink.Array;
            this.SerializeValue( array, idx );
            idx += this.Info.CodeSize;
            this.SerializeOperand( array, ref idx, value );
            return idx - sink.Index;
         }

         protected void SerializeValue( Byte[] array, Int32 index )
         {
            var info = this.Info;
            if ( info.CodeSize == 1 )
            {
               array.WriteByteToBytes( ref index, (Byte) info.SerializedValue );
            }
            else
            {
               // N.B.! Big-endian! Everywhere else everything is little-endian.
               array.WriteUInt16BEToBytes( ref index, (UInt16) info.SerializedValue );
            }
         }

         protected abstract void SerializeOperand( Byte[] array, ref Int32 index, IOpCodeInfo value );

         protected OpCodeSerializationInfo Info { get; }
      }

      protected abstract class AbstractOpCodeDeserializer : DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo>
      {

         public AbstractOpCodeDeserializer( OpCodeSerializationInfo info )
         {
            this.Info = ArgumentValidator.ValidateNotNull( "Info", info );
         }

         public abstract IOpCodeInfo Deserialize( ArrayIndex<Byte> source, out Int32 unitsProcessed );

         protected OpCodeSerializationInfo Info { get; }
      }


      /// <summary>
      /// Creates new instance of <see cref="OpCodeSerializationInfo"/> with given parameters.
      /// </summary>
      /// <param name="code">The <see cref="OpCode"/>.</param>
      /// <param name="codeSize">The size of the code, in bytes.</param>
      /// <param name="operandSize">The size of the operand, in bytes.</param>
      /// <param name="serializedValue">The serialized value of the code, as short.</param>
      /// <param name="statelessInstance">Optional stateless instance for this <paramref name="code"/>.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="code"/> is <c>null</c>.</exception>
      public OpCodeSerializationInfo( OpCode code, Byte codeSize, Byte operandSize, Int16 serializedValue, IOpCodeInfo statelessInstance = null )
      {
         this.Code = ArgumentValidator.ValidateNotNull( "Op code", code );
         this.CodeSize = codeSize;
         this.FixedOperandSize = operandSize;
         this.SerializedValue = serializedValue;
         this.StatelessInstance = statelessInstance;
      }

      /// <summary>
      /// Gets the related <see cref="OpCode"/>.
      /// </summary>
      /// <value>The related <see cref="OpCode"/>.</value>
      public OpCode Code { get; }

      /// <summary>
      /// Gets the byte size of the code, without operand.
      /// </summary>
      /// <value>The byte size of the code, without operand.</value>
      public Byte CodeSize { get; }

      /// <summary>
      /// Gets the fixed byte size of the operand, without the code.
      /// </summary>
      /// <value>The fixed byte size of the operand, without the code.</value>
      public Byte FixedOperandSize { get; }

      /// <summary>
      /// Gets the serialized value of the code, as short.
      /// </summary>
      /// <value>The serialized value of the code, as short.</value>
      public Int16 SerializedValue { get; }

      /// <summary>
      /// Gets the dynamic byte size taken by a single <see cref="IOpCodeInfo"/>.
      /// </summary>
      /// <param name="instance">The <see cref="IOpCodeInfo"/>.</param>
      /// <returns>The dynamic byte size of given <see cref="IOpCodeInfo"/>.</returns>
      /// <remarks>
      /// Currently the only <see cref="IOpCodeInfo"/>s with dynamic byte size are <see cref="IOpCodeInfoWithOperand{TOperand}"/>s which have list of integers as their operand type (e.g. for <see cref="OpCodes.Switch"/> instruction).
      /// </remarks>
      public abstract Int32 GetDynamicSize( IOpCodeInfo instance );

      public abstract SerializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateSerializer( OpCodeSerializationArguments args );

      public abstract DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateDeserializer( OpCodeDeserializationArguments args );

      public abstract void GenerateCodeForOptimizedDeserialization( OptimizedCodeGenerationArgs args );

      public IOpCodeInfo StatelessInstance { get; }


      // TODO I am thinking of removing OpCode.OpCodeID property.
      // Would make comparing OpCodes (and caching their textual values) hard tho.
      ///// <summary>
      ///// Gets the <see cref="CAMPhysical::CILAssemblyManipulator.Physical.OpCodeID"/> for this <see cref="OpCodeProviderInfo"/>.
      ///// </summary>
      ///// <value>The <see cref="CAMPhysical::CILAssemblyManipulator.Physical.OpCodeID"/> for this <see cref="OpCodeProviderInfo"/>.</value>
      //public OpCodeID OpCodeID { get; }

      protected static Byte GetDefaultOpCodeSize( Int32 serializedValue )
      {
         const Int32 MAX_ONE_BYTE_INSTRUCTION = 0xFE;
         return (Byte) ( ( (UInt32) serializedValue ) >= MAX_ONE_BYTE_INSTRUCTION ? 2 : 1 );
      }
   }

   public class OptimizedCodeGenerationArgs
   {
      private readonly Lazy<ArrayQuery<OpCodeInfo>> _storeToResult;

      public OptimizedCodeGenerationArgs(
         CILMetaData md,
         OpCodeProvider opc,
         List<OpCodeInfo> opCodes,
         List<OpCodeInfo> constructorOpCodes_BeforeForLoop,
         List<OpCodeInfo> constructorOpCodes_InsideForLoop,
         List<OpCodeInfo> constructorOpCodes_AfterForLoop,
         Func<Type, TypeSignature> typeSignatureGetter,
         Func<Type, TableIndex> typeRefGetter,
         Func<System.Reflection.MethodBase, TableIndex> methodRefGetter,
         ArrayQuery<OpCodeInfo> loadArray,
         ArrayQuery<OpCodeInfo> loadArrayIndex,
         ArrayQuery<OpCodeInfo> storeArrayIndex,
         ArrayQuery<OpCodeInfo> loadArrayIndexRef,
         ArrayQuery<OpCodeInfo> loadCodeID,
         ArrayQuery<OpCodeInfo> loadCode,
         ArrayQuery<OpCodeInfo> loadSerializationArgs,
         ArrayQuery<OpCodeInfo> storeToResult,
         ArrayQuery<OpCodeInfo> loadResult,
         ArrayQuery<OpCodeInfo> ctor_LoadSerializationArrayLength,
         ArrayQuery<OpCodeInfo> ctor_InForLoop_LoadCurrentSerializationID,
         ArrayQuery<OpCodeInfo> ctor_InForLoop_LoadCurrentSerializationInfo,
         Func<LocalSignature, Int32> localCreator,
         Func<LocalSignature, Int32> ctorLocalCreator
         )
      {
         this.StoredToResult = false;

         this.MD = ArgumentValidator.ValidateNotNull( "Meta data", md );
         this.OPC = ArgumentValidator.ValidateNotNull( "Op Code Provider", opc );
         this.OpCodes = ArgumentValidator.ValidateNotNull( "Op codes", opCodes );
         this.ConstructorOpCodes_BeforeForLoop = ArgumentValidator.ValidateNotNull( "Constructor op codes before for-loop", constructorOpCodes_BeforeForLoop );
         this.ConstructorOpCodes_InsideForLoop = ArgumentValidator.ValidateNotNull( "Constructor op codes inside for-loop", constructorOpCodes_InsideForLoop );
         this.ConstructorOpCodes_AfterForLoop = ArgumentValidator.ValidateNotNull( "Constructor op codes after for-loop", constructorOpCodes_AfterForLoop );
         this.TypeSignatureGetter = ArgumentValidator.ValidateNotNull( "Type signature getter", typeSignatureGetter );
         this.TypeRefGetter = ArgumentValidator.ValidateNotNull( "Type ref getter", typeRefGetter );
         this.MethodRefGetter = ArgumentValidator.ValidateNotNull( "Method ref getter", methodRefGetter );
         this.LoadArray = ArgumentValidator.ValidateNotNull( "Load array codes", loadArray );
         this.LoadArrayIndex = ArgumentValidator.ValidateNotNull( "Load array index codes", loadArrayIndex );
         this.StoreArrayIndex = ArgumentValidator.ValidateNotNull( "Store array index codes", storeArrayIndex );
         this.LoadArrayIndexRef = ArgumentValidator.ValidateNotNull( "Load array index ref codes", loadArrayIndexRef );
         this.LoadCodeID = ArgumentValidator.ValidateNotNull( "Load code ID codes", loadCodeID );
         this.LoadCode = ArgumentValidator.ValidateNotNull( "Load code codes", loadCode );
         this.LoadSerializationArgs = ArgumentValidator.ValidateNotNull( "Load serialization args codes", loadSerializationArgs );
         ArgumentValidator.ValidateNotNull( "Store to result codes", storeToResult );
         this._storeToResult = new Lazy<ArrayQuery<OpCodeInfo>>( () =>
         {
            this.StoredToResult = true;
            return storeToResult;
         }, System.Threading.LazyThreadSafetyMode.None );
         this.LoadResult = ArgumentValidator.ValidateNotNull( "Load result codes", loadResult );
         this.Ctor_LoadSerializationArrayLength = ArgumentValidator.ValidateNotNull( "Load serialization array in constructor", ctor_LoadSerializationArrayLength );
         this.Ctor_InForLoop_LoadCurrentSerializationID = ArgumentValidator.ValidateNotNull( "Load serialization id in constructor for loop", ctor_InForLoop_LoadCurrentSerializationID );
         this.Ctor_InForLoop_LoadCurrentSerializationInfo = ArgumentValidator.ValidateNotNull( "Load serialization info in constructor for loop", ctor_InForLoop_LoadCurrentSerializationInfo );
         this.CtorLocalCreator = ArgumentValidator.ValidateNotNull( "Local creator callback for constructor IL", ctorLocalCreator );
         this.LocalCreator = ArgumentValidator.ValidateNotNull( "Local creator", localCreator );
      }

      public CILMetaData MD { get; }

      public OpCodeProvider OPC { get; }

      public List<OpCodeInfo> OpCodes { get; }

      public List<OpCodeInfo> ConstructorOpCodes_BeforeForLoop { get; }

      public List<OpCodeInfo> ConstructorOpCodes_InsideForLoop { get; }

      public List<OpCodeInfo> ConstructorOpCodes_AfterForLoop { get; }

      public Func<Type, TypeSignature> TypeSignatureGetter { get; }

      public Func<Type, TableIndex> TypeRefGetter { get; }

      public Func<System.Reflection.MethodBase, TableIndex> MethodRefGetter { get; }

      public ArrayQuery<OpCodeInfo> LoadArray { get; }

      public ArrayQuery<OpCodeInfo> LoadArrayIndex { get; }

      public ArrayQuery<OpCodeInfo> StoreArrayIndex { get; }

      public ArrayQuery<OpCodeInfo> LoadArrayIndexRef { get; }

      public ArrayQuery<OpCodeInfo> LoadCodeID { get; }

      public ArrayQuery<OpCodeInfo> LoadCode { get; }

      public ArrayQuery<OpCodeInfo> LoadSerializationArgs { get; }

      public ArrayQuery<OpCodeInfo> Ctor_LoadSerializationArrayLength { get; }

      public ArrayQuery<OpCodeInfo> Ctor_InForLoop_LoadCurrentSerializationID { get; }

      public ArrayQuery<OpCodeInfo> Ctor_InForLoop_LoadCurrentSerializationInfo { get; }

      public Func<LocalSignature, Int32> CtorLocalCreator { get; }

      public Func<LocalSignature, Int32> LocalCreator { get; }

      public Boolean StoredToResult { get; private set; }

      public ArrayQuery<OpCodeInfo> StoreToResult
      {
         get
         {
            return this._storeToResult.Value;
         }
      }

      public ArrayQuery<OpCodeInfo> LoadResult { get; }
   }

   public class OpCodeSerializationArguments
   {
      public OpCodeSerializationArguments( Func<IOpCodeInfoWithOperand<String>, Int32> stringIndexGetter )
      {
         this.StringIndexGetter = ArgumentValidator.ValidateNotNull( "String index getter", stringIndexGetter );
      }

      public Func<IOpCodeInfoWithOperand<String>, Int32> StringIndexGetter { get; }
   }

   public class OpCodeDeserializationArguments
   {
      public OpCodeDeserializationArguments( Func<Int32, String> stringGetter )
      {
         this.StringGetter = ArgumentValidator.ValidateNotNull( "String getter", stringGetter );
      }

      public Func<Int32, String> StringGetter { get; }
   }

   public sealed class OpCodeSerializationInfo_Operandless : OpCodeSerializationInfo
   {

      private sealed class Serializer : AbstractOpCodeSerializer
      {
         public Serializer( OpCodeSerializationInfo_Operandless info )
            : base( info )
         {
         }

         protected sealed override void SerializeOperand( Byte[] array, ref Int32 index, IOpCodeInfo value )
         {
            // Nothing to do.
         }
      }

      private sealed class Deserializer : AbstractOpCodeDeserializer
      {
         public Deserializer( OpCodeSerializationInfo_Operandless info ) :
            base( info )
         {
         }

         public override IOpCodeInfo Deserialize( ArrayIndex<Byte> source, out Int32 unitsProcessed )
         {
            unitsProcessed = 0;
            return Info.StatelessInstance;
         }
      }

      private readonly Serializer _serializer;
      private readonly Deserializer _deserializer;

      public OpCodeSerializationInfo_Operandless( OpCode code, Int32 serializedValue )
         : this( code, GetDefaultOpCodeSize( serializedValue ), (Int16) serializedValue )
      {

      }

      public OpCodeSerializationInfo_Operandless( OpCode code, Byte codeSize, Int16 serializedValue )
         : base( code, codeSize, 0, serializedValue, new OpCodeInfoWithNoOperand( code ) )
      {
         this._serializer = new Serializer( this );
         this._deserializer = new Deserializer( this );
      }

      public override SerializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateSerializer( OpCodeSerializationArguments args )
      {
         return this._serializer;
      }

      public override DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateDeserializer( OpCodeDeserializationArguments args )
      {
         return this._deserializer;
      }

      public override void GenerateCodeForOptimizedDeserialization( OptimizedCodeGenerationArgs args )
      {
         // Add field to store stateless op code infos
         var statelessField = args.MD.FieldDefinitions.AddRow( new FieldDefinition()
         {
            Attributes = FieldAttributes.Private | FieldAttributes.InitOnly,
            Name = "_stateless",
            Signature = new FieldSignature()
            {
               Type = args.TypeSignatureGetter( typeof( IOpCodeInfo[] ) )
            }
         } );

         var opc = (CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider) args.OPC;

         // Add IL code to constructor
         // var stateless = new IOpCodeInfo[arrayLen];
         var statelessArrayIdx = args.CtorLocalCreator( new LocalSignature()
         {
            Type = args.TypeSignatureGetter( typeof( IOpCodeInfo[] ) )
         } );
         args.ConstructorOpCodes_BeforeForLoop.AddRange(
            args.Ctor_LoadSerializationArrayLength.Concat( new OpCodeInfo[]
            {
               new OpCodeInfoWithOperand<TableIndex>(OpCodes.Newarr, args.TypeRefGetter(typeof(IOpCodeInfo))),
               new OpCodeInfoWithOperand<Int32>(OpCodes.Stloc_S, statelessArrayIdx)
            } )
            );

         // stateless[idx] = info.StatelessInstance
         args.ConstructorOpCodes_InsideForLoop.AddRange( new OpCodeInfo[]
         {
            new OpCodeInfoWithOperand<Int32>(OpCodes.Ldloc_S, statelessArrayIdx)
         }.Concat( args.Ctor_InForLoop_LoadCurrentSerializationID )
         .Concat( args.Ctor_InForLoop_LoadCurrentSerializationInfo )
         .Concat( new OpCodeInfo[]
         {
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Callvirt, args.MethodRefGetter(typeof(OpCodeSerializationInfo).GetProperty(nameof(OpCodeSerializationInfo.StatelessInstance)).GetGetMethod())),
            opc.GetOperandlessInfoFor(OpCodes.Stelem_Ref)
         } )
         );

         // this._stateless = stateless;
         args.ConstructorOpCodes_AfterForLoop.AddRange( new OpCodeInfo[]
         {
            opc.GetOperandlessInfoFor(OpCodes.Ldarg_0),
            new OpCodeInfoWithOperand<Int32>(OpCodes.Ldloc_S, statelessArrayIdx),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Stfld, statelessField)
         } );

         // Add IL code to current switch statement
         // this._stateless[opCodeID]
         args.OpCodes.AddRange( new OpCodeInfo[]
         {
            opc.GetOperandlessInfoFor(OpCodes.Ldarg_0),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Ldfld, statelessField),
         } );
         args.OpCodes.AddRange( args.LoadCodeID );
         args.OpCodes.Add( opc.GetOperandlessInfoFor( OpCodes.Ldelem_Ref ) );
      }

      public override Int32 GetDynamicSize( IOpCodeInfo instance )
      {
         return 0;
      }
   }

   public abstract class OpCodeSerializationInfo_Fixed<TOperand> : OpCodeSerializationInfo
   {
      protected abstract class AbstractOpCodeSerializer_Fixed : AbstractOpCodeSerializer
      {
         public AbstractOpCodeSerializer_Fixed( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected sealed override void SerializeOperand( Byte[] array, ref Int32 index, IOpCodeInfo value )
         {
            this.WriteOperand( (IOpCodeInfoWithOperand<TOperand>) value, array, ref index );
         }

         protected abstract void WriteOperand( IOpCodeInfoWithOperand<TOperand> instance, Byte[] array, ref Int32 index );
      }

      protected abstract class AbstractOpCodeDeserializer_Fixed : AbstractOpCodeDeserializer
      {
         public AbstractOpCodeDeserializer_Fixed( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         public sealed override IOpCodeInfo Deserialize( ArrayIndex<Byte> source, out Int32 unitsProcessed )
         {
            var info = this.Info;
            unitsProcessed = info.FixedOperandSize;
            return new OpCodeInfoWithOperand<TOperand>( info.Code, this.ReadOperand( source.Array, source.Index ) );
         }

         protected abstract TOperand ReadOperand( Byte[] array, Int32 index );
      }

      public OpCodeSerializationInfo_Fixed( OpCode code, Byte codeSize, Byte operandSize, Int16 serializedValue )
         : base( code, codeSize, operandSize, serializedValue )
      {
      }

      public sealed override Int32 GetDynamicSize( IOpCodeInfo instance )
      {
         return 0;
      }

   }

   public sealed class OpCodeSerializationInfo_Byte : OpCodeSerializationInfo_Fixed<Int32>
   {
      private sealed class Serializer : AbstractOpCodeSerializer_Fixed
      {
         public Serializer( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected override void WriteOperand( IOpCodeInfoWithOperand<Int32> instance, Byte[] array, ref Int32 index )
         {
            array.WriteByteToBytes( ref index, (Byte) instance.Operand );
         }
      }

      private sealed class Deserializer : AbstractOpCodeDeserializer_Fixed
      {
         public Deserializer( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected override Int32 ReadOperand( Byte[] array, Int32 index )
         {
            return array.ReadByteFromBytes( ref index );
         }
      }

      private readonly Serializer _serializer;
      private readonly Deserializer _deserializer;

      public OpCodeSerializationInfo_Byte( OpCode code, Int32 serializedValue )
         : this( code, GetDefaultOpCodeSize( serializedValue ), (Int16) serializedValue )
      {

      }

      public OpCodeSerializationInfo_Byte( OpCode code, Byte codeSize, Int16 serializedValue ) :
         base( code, codeSize, sizeof( Byte ), serializedValue )
      {
         this._serializer = new Serializer( this );
         this._deserializer = new Deserializer( this );
      }

      public override SerializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateSerializer( OpCodeSerializationArguments args )
      {
         return this._serializer;
      }

      public override DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateDeserializer( OpCodeDeserializationArguments args )
      {
         return this._deserializer;
      }

      public override void GenerateCodeForOptimizedDeserialization( OptimizedCodeGenerationArgs args )
      {
         // new OpCodeInfoWithOperand<Int32>(code, array[index++]);
         var codes = args.OpCodes;
         // code
         codes.AddRange( args.LoadCode );
         // array[index++]
         codes.AddRange( args.LoadArray );
         codes.AddRange( args.LoadArrayIndex );
         var opc = (CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider) args.OPC;
         codes.AddRange( new OpCodeInfo[]
         {
            opc.GetOperandlessInfoFor(OpCodes.Dup),
            opc.GetOperandlessInfoFor(OpCodes.Ldc_I4_1),
            opc.GetOperandlessInfoFor(OpCodes.Add_Ovf)
         } );
         codes.AddRange( args.StoreArrayIndex );
         codes.AddRange( new OpCodeInfo[]
         {
            opc.GetOperandlessInfoFor(OpCodes.Ldelem_U1),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Newobj, args.MethodRefGetter(typeof(OpCodeInfoWithOperand<Int32>).GetConstructor(new Type[] { typeof(OpCode), typeof( Int32 ) })))
         } );
      }
   }

   public sealed class OpCodeSerializationInfo_SByte : OpCodeSerializationInfo_Fixed<Int32>
   {
      private sealed class Serializer : AbstractOpCodeSerializer_Fixed
      {
         public Serializer( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected override void WriteOperand( IOpCodeInfoWithOperand<Int32> instance, Byte[] array, ref Int32 index )
         {
            array.WriteSByteToBytes( ref index, (SByte) instance.Operand );
         }
      }

      private sealed class Deserializer : AbstractOpCodeDeserializer_Fixed
      {
         public Deserializer( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected override Int32 ReadOperand( Byte[] array, Int32 index )
         {
            return array.ReadSByteFromBytes( ref index );
         }
      }

      private readonly Serializer _serializer;
      private readonly Deserializer _deserializer;

      public OpCodeSerializationInfo_SByte( OpCode code, Int32 serializedValue )
         : this( code, GetDefaultOpCodeSize( serializedValue ), (Int16) serializedValue )
      {

      }

      public OpCodeSerializationInfo_SByte( OpCode code, Byte codeSize, Int16 serializedValue ) :
         base( code, codeSize, sizeof( SByte ), serializedValue )
      {
         this._serializer = new Serializer( this );
         this._deserializer = new Deserializer( this );
      }

      public override SerializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateSerializer( OpCodeSerializationArguments args )
      {
         return this._serializer;
      }

      public override DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateDeserializer( OpCodeDeserializationArguments args )
      {
         return this._deserializer;
      }

      public override void GenerateCodeForOptimizedDeserialization( OptimizedCodeGenerationArgs args )
      {
         // new OpCodeInfoWithOperand<Int32>(code, unchecked((SByte)array[index++]));
         var codes = args.OpCodes;
         // code
         codes.AddRange( args.LoadCode );
         // array[index++]
         codes.AddRange( args.LoadArray );
         codes.AddRange( args.LoadArrayIndex );
         var opc = (CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider) args.OPC;
         codes.AddRange( new OpCodeInfo[]
         {
            opc.GetOperandlessInfoFor(OpCodes.Dup),
            opc.GetOperandlessInfoFor(OpCodes.Ldc_I4_1),
            opc.GetOperandlessInfoFor(OpCodes.Add_Ovf)
         } );
         codes.AddRange( args.StoreArrayIndex );
         codes.AddRange( new OpCodeInfo[]
         {
            opc.GetOperandlessInfoFor(OpCodes.Ldelem_U1),
            opc.GetOperandlessInfoFor(OpCodes.Conv_I1),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Newobj, args.MethodRefGetter(typeof(OpCodeInfoWithOperand<Int32>).GetConstructor(new Type[] { typeof(OpCode), typeof( Int32 ) })))
         } );
      }
   }

   public sealed class OpCodeSerializationInfo_Int16 : OpCodeSerializationInfo_Fixed<Int32>
   {
      private sealed class Serializer : AbstractOpCodeSerializer_Fixed
      {
         public Serializer( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected override void WriteOperand( IOpCodeInfoWithOperand<Int32> instance, Byte[] array, ref Int32 index )
         {
            array.WriteUInt16LEToBytes( ref index, (UInt16) instance.Operand );
         }
      }

      private sealed class Deserializer : AbstractOpCodeDeserializer_Fixed
      {
         public Deserializer( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected override Int32 ReadOperand( Byte[] array, Int32 index )
         {
            return array.ReadUInt16LEFromBytes( ref index );
         }
      }

      private readonly Serializer _serializer;
      private readonly Deserializer _deserializer;

      public OpCodeSerializationInfo_Int16( OpCode code, Int32 serializedValue )
         : this( code, GetDefaultOpCodeSize( serializedValue ), (Int16) serializedValue )
      {

      }

      public OpCodeSerializationInfo_Int16( OpCode code, Byte codeSize, Int16 serializedValue ) :
         base( code, codeSize, sizeof( UInt16 ), serializedValue )
      {
         this._serializer = new Serializer( this );
         this._deserializer = new Deserializer( this );
      }

      public override SerializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateSerializer( OpCodeSerializationArguments args )
      {
         return this._serializer;
      }

      public override DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateDeserializer( OpCodeDeserializationArguments args )
      {
         return this._deserializer;
      }

      public override void GenerateCodeForOptimizedDeserialization( OptimizedCodeGenerationArgs args )
      {
         // new OpCodeInfoWithOperand<Int32>(code, array.ReadUInt16LEFromBytes( ref index ));
         var codes = args.OpCodes;
         codes.AddRange( args.LoadCode );
         codes.AddRange( args.LoadArray );
         codes.AddRange( args.LoadArrayIndexRef );
         // TODO add aliases for various UP projects to get rid of string constants, which might become wrong ones in refactorings, within code
         var e_up = typeof( BinaryUtils ).Assembly.GetType( "E_UtilPack" );
         codes.AddRange( new OpCodeInfo[]
         {
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, args.MethodRefGetter(e_up.GetMethod("ReadUInt16LEFromBytes", new Type[] { typeof(Byte[]), typeof(Int32).MakeByRefType() } ))),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Newobj, args.MethodRefGetter(typeof(OpCodeInfoWithOperand<Int32>).GetConstructor(new Type[] { typeof(OpCode), typeof(Int32)})))
         } );
      }
   }

   public sealed class OpCodeSerializationInfo_Int32 : OpCodeSerializationInfo_Fixed<Int32>
   {
      private sealed class Serializer : AbstractOpCodeSerializer_Fixed
      {
         public Serializer( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected override void WriteOperand( IOpCodeInfoWithOperand<Int32> instance, Byte[] array, ref Int32 index )
         {
            array.WriteInt32LEToBytes( ref index, instance.Operand );
         }
      }

      private sealed class Deserializer : AbstractOpCodeDeserializer_Fixed
      {
         public Deserializer( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected override Int32 ReadOperand( Byte[] array, Int32 index )
         {
            return array.ReadInt32LEFromBytes( ref index );
         }
      }

      private readonly Serializer _serializer;
      private readonly Deserializer _deserializer;

      public OpCodeSerializationInfo_Int32( OpCode code, Int32 serializedValue )
         : this( code, GetDefaultOpCodeSize( serializedValue ), (Int16) serializedValue )
      {

      }

      public OpCodeSerializationInfo_Int32( OpCode code, Byte codeSize, Int16 serializedValue ) :
         base( code, codeSize, sizeof( Int32 ), serializedValue )
      {
         this._serializer = new Serializer( this );
         this._deserializer = new Deserializer( this );
      }

      public override SerializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateSerializer( OpCodeSerializationArguments args )
      {
         return this._serializer;
      }

      public override DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateDeserializer( OpCodeDeserializationArguments args )
      {
         return this._deserializer;
      }

      public override void GenerateCodeForOptimizedDeserialization( OptimizedCodeGenerationArgs args )
      {
         // new OpCodeInfoWithOperand<Int32>(code, array.ReadInt32LEFromBytes( ref index ));
         var codes = args.OpCodes;
         codes.AddRange( args.LoadCode );
         codes.AddRange( args.LoadArray );
         codes.AddRange( args.LoadArrayIndexRef );
         // TODO add aliases for various UP projects to get rid of string constants, which might become wrong ones in refactorings, within code
         var e_up = typeof( BinaryUtils ).Assembly.GetType( "E_UtilPack" );
         codes.AddRange( new OpCodeInfo[]
         {
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, args.MethodRefGetter(e_up.GetMethod("ReadInt32LEFromBytes", new Type[] { typeof(Byte[]), typeof(Int32).MakeByRefType() } ))),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Newobj, args.MethodRefGetter(typeof(OpCodeInfoWithOperand<Int32>).GetConstructor(new Type[] { typeof(OpCode), typeof(Int32)})))
         } );
      }
   }

   public sealed class OpCodeSerializationInfo_Int64 : OpCodeSerializationInfo_Fixed<Int64>
   {
      private sealed class Serializer : AbstractOpCodeSerializer_Fixed
      {
         public Serializer( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected override void WriteOperand( IOpCodeInfoWithOperand<Int64> instance, Byte[] array, ref Int32 index )
         {
            array.WriteInt64LEToBytes( ref index, instance.Operand );
         }
      }

      private sealed class Deserializer : AbstractOpCodeDeserializer_Fixed
      {
         public Deserializer( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected override Int64 ReadOperand( Byte[] array, Int32 index )
         {
            return array.ReadInt64LEFromBytes( ref index );
         }
      }

      private readonly Serializer _serializer;
      private readonly Deserializer _deserializer;

      public OpCodeSerializationInfo_Int64( OpCode code, Int32 serializedValue )
         : this( code, GetDefaultOpCodeSize( serializedValue ), (Int16) serializedValue )
      {

      }

      public OpCodeSerializationInfo_Int64( OpCode code, Byte codeSize, Int16 serializedValue ) :
         base( code, codeSize, sizeof( Int64 ), serializedValue )
      {
         this._serializer = new Serializer( this );
         this._deserializer = new Deserializer( this );
      }

      public override SerializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateSerializer( OpCodeSerializationArguments args )
      {
         return this._serializer;
      }

      public override DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateDeserializer( OpCodeDeserializationArguments args )
      {
         return this._deserializer;
      }

      public override void GenerateCodeForOptimizedDeserialization( OptimizedCodeGenerationArgs args )
      {
         // new OpCodeInfoWithOperand<Int64>(code, array.ReadInt64LEFromBytes( ref index ));
         var codes = args.OpCodes;
         codes.AddRange( args.LoadCode );
         codes.AddRange( args.LoadArray );
         codes.AddRange( args.LoadArrayIndexRef );
         // TODO add aliases for various UP projects to get rid of string constants, which might become wrong ones in refactorings, within code
         var e_up = typeof( BinaryUtils ).Assembly.GetType( "E_UtilPack" );
         codes.AddRange( new OpCodeInfo[]
         {
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, args.MethodRefGetter(e_up.GetMethod("ReadInt64LEFromBytes", new Type[] { typeof(Byte[]), typeof(Int32).MakeByRefType() } ))),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Newobj, args.MethodRefGetter(typeof(OpCodeInfoWithOperand<Int64>).GetConstructor(new Type[] { typeof(OpCode), typeof(Int64)})))
         } );
      }
   }

   public sealed class OpCodeSerializationInfo_Single : OpCodeSerializationInfo_Fixed<Single>
   {
      private sealed class Serializer : AbstractOpCodeSerializer_Fixed
      {
         public Serializer( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected override void WriteOperand( IOpCodeInfoWithOperand<Single> instance, Byte[] array, ref Int32 index )
         {
            array.WriteSingleLEToBytes( ref index, instance.Operand );
         }
      }

      private sealed class Deserializer : AbstractOpCodeDeserializer_Fixed
      {
         public Deserializer( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected override Single ReadOperand( Byte[] array, Int32 index )
         {
            return array.ReadSingleLEFromBytes( ref index );
         }
      }

      private readonly Serializer _serializer;
      private readonly Deserializer _deserializer;

      public OpCodeSerializationInfo_Single( OpCode code, Int32 serializedValue )
         : this( code, GetDefaultOpCodeSize( serializedValue ), (Int16) serializedValue )
      {

      }

      public OpCodeSerializationInfo_Single( OpCode code, Byte codeSize, Int16 serializedValue ) :
         base( code, codeSize, sizeof( Single ), serializedValue )
      {
         this._serializer = new Serializer( this );
         this._deserializer = new Deserializer( this );
      }

      public override SerializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateSerializer( OpCodeSerializationArguments args )
      {
         return this._serializer;
      }

      public override DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateDeserializer( OpCodeDeserializationArguments args )
      {
         return this._deserializer;
      }

      public override void GenerateCodeForOptimizedDeserialization( OptimizedCodeGenerationArgs args )
      {
         // new OpCodeInfoWithOperand<Single>(code, array.ReadSingleLEFromBytes( ref index ));
         var codes = args.OpCodes;
         codes.AddRange( args.LoadCode );
         codes.AddRange( args.LoadArray );
         codes.AddRange( args.LoadArrayIndexRef );
         // TODO add aliases for various UP projects to get rid of string constants, which might become wrong ones in refactorings, within code
         var e_up = typeof( BinaryUtils ).Assembly.GetType( "E_UtilPack" );
         codes.AddRange( new OpCodeInfo[]
         {
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, args.MethodRefGetter(e_up.GetMethod("ReadSingleLEFromBytes", new Type[] { typeof(Byte[]), typeof(Int32).MakeByRefType() } ))),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Newobj, args.MethodRefGetter(typeof(OpCodeInfoWithOperand<Single>).GetConstructor(new Type[] { typeof(OpCode), typeof(Single)})))
         } );
      }
   }

   public sealed class OpCodeSerializationInfo_Double : OpCodeSerializationInfo_Fixed<Double>
   {
      private sealed class Serializer : AbstractOpCodeSerializer_Fixed
      {
         public Serializer( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected override void WriteOperand( IOpCodeInfoWithOperand<Double> instance, Byte[] array, ref Int32 index )
         {
            array.WriteDoubleLEToBytes( ref index, instance.Operand );
         }
      }

      private sealed class Deserializer : AbstractOpCodeDeserializer_Fixed
      {
         public Deserializer( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected override Double ReadOperand( Byte[] array, Int32 index )
         {
            return array.ReadDoubleLEFromBytes( ref index );
         }
      }

      private readonly Serializer _serializer;
      private readonly Deserializer _deserializer;

      public OpCodeSerializationInfo_Double( OpCode code, Int32 serializedValue )
         : this( code, GetDefaultOpCodeSize( serializedValue ), (Int16) serializedValue )
      {

      }

      public OpCodeSerializationInfo_Double( OpCode code, Byte codeSize, Int16 serializedValue ) :
         base( code, codeSize, sizeof( Double ), serializedValue )
      {
         this._serializer = new Serializer( this );
         this._deserializer = new Deserializer( this );
      }

      public override SerializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateSerializer( OpCodeSerializationArguments args )
      {
         return this._serializer;
      }

      public override DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateDeserializer( OpCodeDeserializationArguments args )
      {
         return this._deserializer;
      }

      public override void GenerateCodeForOptimizedDeserialization( OptimizedCodeGenerationArgs args )
      {
         // new OpCodeInfoWithOperand<Single>(code, array.ReadDoubleLEFromBytes( ref index ));
         var codes = args.OpCodes;
         codes.AddRange( args.LoadCode );
         codes.AddRange( args.LoadArray );
         codes.AddRange( args.LoadArrayIndexRef );
         // TODO add aliases for various UP projects to get rid of string constants, which might become wrong ones in refactorings, within code
         var e_up = typeof( BinaryUtils ).Assembly.GetType( "E_UtilPack" );
         codes.AddRange( new OpCodeInfo[]
         {
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, args.MethodRefGetter(e_up.GetMethod("ReadDoubleLEFromBytes", new Type[] { typeof(Byte[]), typeof(Int32).MakeByRefType() } ))),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Newobj, args.MethodRefGetter(typeof(OpCodeInfoWithOperand<Double>).GetConstructor(new Type[] { typeof(OpCode), typeof(Double)})))
         } );
      }
   }

   public sealed class OpCodeSerializationInfo_TableIndex : OpCodeSerializationInfo_Fixed<TableIndex>
   {
      private sealed class Serializer : AbstractOpCodeSerializer_Fixed
      {
         public Serializer( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected override void WriteOperand( IOpCodeInfoWithOperand<TableIndex> instance, Byte[] array, ref Int32 index )
         {
            array.WriteInt32LEToBytes( ref index, instance.Operand.GetOneBasedToken() );
         }
      }

      private sealed class Deserializer : AbstractOpCodeDeserializer_Fixed
      {
         public Deserializer( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected override TableIndex ReadOperand( Byte[] array, Int32 index )
         {
            return TableIndex.FromOneBasedToken( array.ReadInt32LEFromBytes( ref index ) );
         }
      }

      private readonly Serializer _serializer;
      private readonly Deserializer _deserializer;

      public OpCodeSerializationInfo_TableIndex( OpCode code, Int32 serializedValue )
         : this( code, GetDefaultOpCodeSize( serializedValue ), (Int16) serializedValue )
      {

      }

      public OpCodeSerializationInfo_TableIndex( OpCode code, Byte codeSize, Int16 serializedValue ) :
         base( code, codeSize, sizeof( Int32 ), serializedValue )
      {
         this._serializer = new Serializer( this );
         this._deserializer = new Deserializer( this );
      }

      public override SerializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateSerializer( OpCodeSerializationArguments args )
      {
         return this._serializer;
      }

      public override DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateDeserializer( OpCodeDeserializationArguments args )
      {
         return this._deserializer;
      }

      public override void GenerateCodeForOptimizedDeserialization( OptimizedCodeGenerationArgs args )
      {
         // new OpCodeInfoWithOperand<TableIndex>( code, TableIndex.FromOneBasedToken( array.ReadSingleLEFromBytes( ref index ) ) );
         var codes = args.OpCodes;
         codes.AddRange( args.LoadCode );
         codes.AddRange( args.LoadArray );
         codes.AddRange( args.LoadArrayIndexRef );
         // TODO add aliases for various UP projects to get rid of string constants, which might become wrong ones in refactorings, within code
         var e_up = typeof( BinaryUtils ).Assembly.GetType( "E_UtilPack" );
         codes.AddRange( new OpCodeInfo[]
         {
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, args.MethodRefGetter(e_up.GetMethod("ReadInt32LEFromBytes", new Type[] { typeof(Byte[]), typeof(Int32).MakeByRefType() } ))),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, args.MethodRefGetter(typeof(TableIndex).GetMethod(nameof(TableIndex.FromOneBasedToken)))),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Newobj, args.MethodRefGetter(typeof(OpCodeInfoWithOperand<TableIndex>).GetConstructor(new Type[] { typeof(OpCode), typeof(TableIndex)})))
         } );
      }
   }

   public sealed class OpCodeSerializationInfo_String : OpCodeSerializationInfo_Fixed<String>
   {
      private sealed class Serializer : AbstractOpCodeSerializer_Fixed
      {
         private readonly Func<IOpCodeInfoWithOperand<String>, Int32> _stringIndexGetter;

         public Serializer( OpCodeSerializationInfo info, Func<IOpCodeInfoWithOperand<String>, Int32> stringIndexGetter )
            : base( info )
         {
            this._stringIndexGetter = ArgumentValidator.ValidateNotNull( "String index getter callback", stringIndexGetter );
         }

         protected override void WriteOperand( IOpCodeInfoWithOperand<String> instance, Byte[] array, ref Int32 index )
         {
            array.WriteInt32LEToBytes( ref index, this._stringIndexGetter( instance ) );
         }
      }

      private sealed class Deserializer : AbstractOpCodeDeserializer_Fixed
      {
         private readonly Func<Int32, String> _stringGetter;

         public Deserializer( OpCodeSerializationInfo info, Func<Int32, String> stringGetter )
            : base( info )
         {
            this._stringGetter = ArgumentValidator.ValidateNotNull( "String getter", stringGetter );
         }

         protected override String ReadOperand( Byte[] array, Int32 index )
         {
            return this._stringGetter( array.ReadInt32LEFromBytes( ref index ) );
         }
      }

      public OpCodeSerializationInfo_String( OpCode code, Int32 serializedValue )
         : this( code, GetDefaultOpCodeSize( serializedValue ), (Int16) serializedValue )
      {

      }

      public OpCodeSerializationInfo_String( OpCode code, Byte codeSize, Int16 serializedValue )
         : base( code, codeSize, sizeof( Int32 ), serializedValue )
      {
      }

      public override SerializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateSerializer( OpCodeSerializationArguments args )
      {
         return new Serializer( this, args.StringIndexGetter );
      }

      public override DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateDeserializer( OpCodeDeserializationArguments args )
      {
         return new Deserializer( this, args.StringGetter );
      }

      public override void GenerateCodeForOptimizedDeserialization( OptimizedCodeGenerationArgs args )
      {
         // new OpCodeInfoWithOperand<String>( code, this._args.StringGetter( array.ReadSingleLEFromBytes( ref index ) ) );
         var codes = args.OpCodes;
         codes.AddRange( args.LoadCode );
         codes.AddRange( args.LoadSerializationArgs );
         codes.Add( new OpCodeInfoWithOperand<TableIndex>( OpCodes.Callvirt, args.MethodRefGetter( typeof( OpCodeDeserializationArguments ).GetProperty( nameof( OpCodeDeserializationArguments.StringGetter ) ).GetGetMethod() ) ) );
         codes.AddRange( args.LoadArray );
         codes.AddRange( args.LoadArrayIndexRef );
         // TODO add aliases for various UP projects to get rid of string constants, which might become wrong ones in refactorings, within code
         var e_up = typeof( BinaryUtils ).Assembly.GetType( "E_UtilPack" );
         codes.AddRange( new OpCodeInfo[]
         {
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, args.MethodRefGetter(e_up.GetMethod("ReadInt32LEFromBytes", new Type[] { typeof(Byte[]), typeof(Int32).MakeByRefType() } ))),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Callvirt, args.MethodRefGetter( typeof(Func<Int32, String>).GetMethod(nameof(Func<Int32, String>.Invoke) ) ) ),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Newobj, args.MethodRefGetter(typeof(OpCodeInfoWithOperand<String>).GetConstructor(new Type[] { typeof(OpCode), typeof(String)})))
         } );
      }
   }

   public abstract class OpCodeSerializationInfo_Dynamic<TOpCode> : OpCodeSerializationInfo
      where TOpCode : IOpCodeInfo
   {
      protected abstract class AbstractOpCodeSerializer_Dynamic : AbstractOpCodeSerializer
      {
         public AbstractOpCodeSerializer_Dynamic( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected sealed override void SerializeOperand( Byte[] array, ref Int32 index, IOpCodeInfo value )
         {
            this.WriteOperand( (TOpCode) value, array, ref index );
         }

         protected abstract void WriteOperand( TOpCode instance, Byte[] array, ref Int32 index );
      }

      protected abstract class AbstractOpCodeDeserializer_Dynamic : AbstractOpCodeDeserializer
      {
         public AbstractOpCodeDeserializer_Dynamic( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         public sealed override IOpCodeInfo Deserialize( ArrayIndex<Byte> source, out Int32 unitsProcessed )
         {
            var retVal = this.Deserialize( source.Array, source.Index );
            unitsProcessed = this.Info.GetTotalByteCount( retVal ) - this.Info.CodeSize;
            return retVal;
         }

         protected abstract TOpCode Deserialize( Byte[] array, Int32 index );

      }

      public OpCodeSerializationInfo_Dynamic( OpCode code, Byte codeSize, Byte operandSize, Int16 serializedValue )
         : base( code, codeSize, operandSize, serializedValue )
      {
      }

      public override Int32 GetDynamicSize( IOpCodeInfo instance )
      {
         return instance == null ? 0 : this.DoGetDynamicSize( (TOpCode) instance );
      }

      protected abstract Int32 DoGetDynamicSize( TOpCode instance );
   }

   public sealed class OpCodeSerializationInfo_Int32List : OpCodeSerializationInfo_Dynamic<IOpCodeInfoWithOperand<IList<Int32>>>
   {
      private sealed class Serializer : AbstractOpCodeSerializer_Dynamic
      {
         public Serializer( OpCodeSerializationInfo info ) : base( info )
         {
         }

         protected override void WriteOperand( IOpCodeInfoWithOperand<IList<Int32>> instance, Byte[] array, ref Int32 index )
         {
            var list = instance.Operand;
            array.WriteInt32LEToBytes( ref index, list.Count );

            foreach ( var i32 in instance.Operand )
            {
               array.WriteInt32LEToBytes( ref index, i32 );
            }
         }
      }

      private sealed class Deserializer : AbstractOpCodeDeserializer_Dynamic
      {
         public Deserializer( OpCodeSerializationInfo info )
            : base( info )
         {
         }

         protected override IOpCodeInfoWithOperand<IList<Int32>> Deserialize( Byte[] array, Int32 index )
         {
            var cap = array.ReadInt32LEFromBytes( ref index );
            var retVal = new OpCodeInfoWithList<Int32>( this.Info.Code, cap );
            var list = retVal.Operand;
            for ( var i = 0; i < cap; ++i )
            {
               list.Add( array.ReadInt32LEFromBytes( ref index ) );
            }
            return retVal;
         }
      }

      private readonly Serializer _serializer;
      private readonly Deserializer _deserializer;

      public OpCodeSerializationInfo_Int32List( OpCode code, Int32 serializedValue )
         : this( code, GetDefaultOpCodeSize( serializedValue ), (Int16) serializedValue )
      {

      }

      public OpCodeSerializationInfo_Int32List( OpCode code, Byte codeSize, Int16 serializedValue )
         : base( code, codeSize, sizeof( Int32 ), serializedValue )
      {
         this._serializer = new Serializer( this );
         this._deserializer = new Deserializer( this );
      }

      public override SerializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateSerializer( OpCodeSerializationArguments args )
      {
         return this._serializer;
      }

      public override DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateDeserializer( OpCodeDeserializationArguments args )
      {
         return this._deserializer;
      }

      protected override Int32 DoGetDynamicSize( IOpCodeInfoWithOperand<IList<Int32>> instance )
      {
         return instance.Operand.Count * sizeof( Int32 );
      }

      public override void GenerateCodeForOptimizedDeserialization( OptimizedCodeGenerationArgs args )
      {
         // var cap = array.ReadInt32LEFromBytes( ref index );
         // retVal = new OpCodeInfoWithList<Int32>( this.Info.Code, cap );
         // var list = retVal.Operand;
         // for ( var i = 0; i < cap; ++i )
         // {
         //   list.Add( array.ReadInt32LEFromBytes( ref index ) );
         // }
         var codes = args.OpCodes;
         var capacityLocalIndex = args.LocalCreator( new LocalSignature()
         {
            Type = args.TypeSignatureGetter( typeof( Int32 ) )
         } );


         // var cap = array.ReadInt32LEFromBytes( ref index );
         codes.AddRange( args.LoadArray );
         codes.AddRange( args.LoadArrayIndexRef );
         // TODO add aliases for various UP projects to get rid of string constants, which might become wrong ones in refactorings, within code
         var e_up = typeof( BinaryUtils ).Assembly.GetType( "E_UtilPack" );
         codes.AddRange( new OpCodeInfo[]
         {
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, args.MethodRefGetter(e_up.GetMethod("ReadInt32LEFromBytes", new Type[] { typeof(Byte[]), typeof(Int32).MakeByRefType() } ))),
            new OpCodeInfoWithOperand<Int32>( OpCodes.Stloc_S, capacityLocalIndex ),
         } );

         // retVal = new OpCodeInfoWithList<Int32>( code, cap );
         var loadCap = new OpCodeInfoWithOperand<Int32>( OpCodes.Ldloc_S, capacityLocalIndex );
         codes.AddRange( args.LoadCode );
         codes.Add( loadCap );
         codes.AddRange( new OpCodeInfo[]
         {
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Newobj, args.MethodRefGetter(typeof(OpCodeInfoWithList<Int32>).GetConstructor(new Type[] { typeof(OpCode), typeof(Int32)})))
         } );
         codes.AddRange( args.StoreToResult );

         // var list = retVal.Operand;
         var listLocalIndex = args.LocalCreator( new LocalSignature()
         {
            Type = args.TypeSignatureGetter( typeof( List<Int32> ) )
         } );
         codes.AddRange( args.LoadResult );
         codes.AddRange( new OpCodeInfo[]
         {
            new OpCodeInfoWithOperand<TableIndex>( OpCodes.Castclass, args.TypeRefGetter(typeof(OpCodeInfoWithOperandGetter<List<Int32>>))),
            new OpCodeInfoWithOperand<TableIndex>( OpCodes.Callvirt, args.MethodRefGetter( typeof( OpCodeInfoWithOperandGetter<List<Int32>> ).GetProperty( nameof( OpCodeInfoWithOperandGetter<List<Int32>>.Operand ) ).GetGetMethod() ) ),
            new OpCodeInfoWithOperand<Int32>(OpCodes.Stloc_S, listLocalIndex)
         } );

         // for ( var i = 0; i < cap; ++i )
         // {
         //   list.Add( array.ReadInt32LEFromBytes( ref index ) );
         // }
         var loopVariableIndex = args.LocalCreator( new LocalSignature()
         {
            Type = args.TypeSignatureGetter( typeof( Int32 ) )
         } );
         var loadLoopVariable = new OpCodeInfoWithOperand<Int32>( OpCodes.Ldloc_S, loopVariableIndex );
         var storeLoopVariable = new OpCodeInfoWithOperand<Int32>( OpCodes.Stloc_S, loopVariableIndex );
         var opc = (CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider) args.OPC;
         opc.EmitForLoop(
            codes,
            new OpCodeInfo[]
            {
               // i = 0;
               opc.GetOperandlessInfoFor(OpCodes.Ldc_I4_0),
               storeLoopVariable,
            },
            new OpCodeInfo[]
            {
               // i < cap
               loadLoopVariable,
               loadCap,
            },
            OpCodes.Blt_S,
            OpCodes.Blt,
            new OpCodeInfo[]
            {
               // ++i
               loadLoopVariable,
               opc.GetOperandlessInfoFor(OpCodes.Ldc_I4_1),
               opc.GetOperandlessInfoFor(OpCodes.Add),
               storeLoopVariable,
            },
            new OpCodeInfo[]
            {
               // list.Add( array.ReadInt32LEFromBytes( ref index ) );
               new OpCodeInfoWithOperand<Int32>( OpCodes.Ldloc_S, listLocalIndex )
            }
            .Concat( args.LoadArray )
            .Concat( args.LoadArrayIndexRef )
            .Concat( new OpCodeInfo[]
            {
               new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, args.MethodRefGetter( e_up.GetMethod("ReadInt32LEFromBytes", new Type[] { typeof(Byte[]), typeof(Int32).MakeByRefType() } ))),
               new OpCodeInfoWithOperand<TableIndex>(OpCodes.Callvirt, args.MethodRefGetter(typeof(List<Int32>).GetMethod(nameof(List<Int32>.Add)))),
            } ) );
      }
   }

   public abstract class AbstractDefaultOpCodeProvider : DefaultSelfDescribingExtensionByCompositionProvider<Object>, OpCodeProvider, CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider
   {
      /// <summary>
      /// 
      /// </summary>
      public class OpCodeElementTypeInfo
      {
         /// <summary>
         /// 
         /// </summary>
         /// <param name="opCodeType"></param>
         /// <param name="registerEdgesForVisitor"></param>
         /// <param name="registerEquality"></param>
         /// <param name="registerHashCode"></param>
         /// <param name="cloningFunctionality"></param>
         public OpCodeElementTypeInfo(
            Type opCodeType,
            Action<VisitorVertexInfoFactory<IOpCodeInfo, Int32>, TOpCodeEqualityAcceptorSetup> registerEdgesForVisitor,
            Action<TOpCodeEqualityAcceptorSetup> registerEquality,
            Action<TOpCodeHashCodeAcceptorSetup> registerHashCode,
            AcceptVertexExplicitWithResultDelegate<IOpCodeInfo, CAMPhysical::CILAssemblyManipulator.Physical.Meta.CAMCopyingContext, IOpCodeInfo> cloningFunctionality
            )
         {
            this.OpCodeType = ArgumentValidator.ValidateNotNull( "Op code element type", opCodeType );
            if ( this.OpCodeType.ContainsGenericParameters )
            {
               throw new ArgumentException( "Op code type must not contain unreplaced generic arguments." );
            }
            else if ( !typeof( IOpCodeInfo ).IsAssignableFrom( opCodeType ) )
            {
               throw new ArgumentException( "Op code type must be sub-type of " + typeof( IOpCodeInfo ) + "." );
            }

            this.RegisterEdgesForVisitor = registerEdgesForVisitor;
            this.RegisterEquality = ArgumentValidator.ValidateNotNull( "Equality register callback", registerEquality );
            this.RegisterHashCode = registerHashCode;
            this.CloningFunctionality = cloningFunctionality;
         }

         /// <summary>
         /// Gets the type of the signature element.
         /// </summary>
         /// <value>The type of the signature element.</value>
         public Type OpCodeType { get; }

         /// <summary>
         /// Gets the callback to register edges to <see cref="AutomaticTypeBasedVisitor{TElement, TEdgeInfo}"/>.
         /// </summary>
         /// <value>The callback to register edges to <see cref="AutomaticTypeBasedVisitor{TElement, TEdgeInfo}"/>.</value>
         public Action<VisitorVertexInfoFactory<IOpCodeInfo, Int32>, TOpCodeEqualityAcceptorSetup> RegisterEdgesForVisitor { get; }

         /// <summary>
         /// Gets the callback to register non-deep equality comparison for the op code type.
         /// </summary>
         /// <value>The callback to register non-deep equality comparison for the op code type.</value>
         public Action<TOpCodeEqualityAcceptorSetup> RegisterEquality { get; }

         /// <summary>
         /// Gets the callback to register hash code computation for the op code type.
         /// </summary>
         /// <value>The callback to register hash code computation for the op code type.</value>
         public Action<TOpCodeHashCodeAcceptorSetup> RegisterHashCode { get; }

         /// <summary>
         /// Gets the callback to create a copy of <see cref="IOpCodeInfo"/> for the op code type.
         /// </summary>
         /// <value>The callback to create a copy of <see cref="IOpCodeInfo"/> for the op code type.</value>
         public AcceptVertexExplicitWithResultDelegate<IOpCodeInfo, CAMPhysical::CILAssemblyManipulator.Physical.Meta.CAMCopyingContext, IOpCodeInfo> CloningFunctionality { get; }

         /// <summary>
         /// 
         /// </summary>
         /// <typeparam name="TOpCode"></typeparam>
         /// <param name="equality"></param>
         /// <param name="hashCode"></param>
         /// <param name="cloningFunctionality"></param>
         /// <returns></returns>
         public static OpCodeElementTypeInfo NewInfo<TOpCode>(
            //Action<VisitorVertexInfoFactory<IOpCodeInfo, Int32>, TOpCodeEqualityAcceptorSetup> registerEdgesForVisitor,
            Equality<TOpCode> equality,
            HashCode<TOpCode> hashCode,
            AcceptVertexExplicitWithResultDelegateTyped<IOpCodeInfo, CAMPhysical::CILAssemblyManipulator.Physical.Meta.CAMCopyingContext, IOpCodeInfo, TOpCode> cloningFunctionality
            )
            where TOpCode : class, IOpCodeInfo
         {
            return new OpCodeElementTypeInfo(
               typeof( TOpCode ),
               null, //registerEdgesForVisitor,
               equalityAcceptor => equalityAcceptor.RegisterEqualityAcceptor( equality ?? new Equality<TOpCode>( ( x, y ) => true ) ),
               hashCode == null ? (Action<TOpCodeHashCodeAcceptorSetup>) null : hashCodeAcceptor => hashCodeAcceptor.RegisterHashCodeComputer( hashCode ),
               cloningFunctionality == null ? (AcceptVertexExplicitWithResultDelegate<IOpCodeInfo, CAMPhysical::CILAssemblyManipulator.Physical.Meta.CAMCopyingContext, IOpCodeInfo>) null : ( el, ctx, cb ) => cloningFunctionality( (TOpCode) el, ctx, cb )
               );
         }
      }

      private readonly Dictionary<OpCode, OpCodeSerializationInfo> _infosDictionary;

      /// <summary>
      /// Initializes a new instance of <see cref="AbstractDefaultOpCodeProvider"/> with support for given op code set.
      /// </summary>
      /// <param name="typeInfos">The type information about possible types of op code instances.</param>
      /// <param name="serializationInfos">The <see cref="OpCodeSerializationInfo"/>s to support.</param>
      public AbstractDefaultOpCodeProvider(
         IEnumerable<OpCodeElementTypeInfo> typeInfos,
         IEnumerable<OpCodeSerializationInfo> serializationInfos
         )
      {
         ArgumentValidator.ValidateNotNull( "Type infos", typeInfos );
         ArgumentValidator.ValidateNotNull( "Serialization infos", serializationInfos );

         var visitor = new AutomaticTypeBasedVisitor<IOpCodeInfo, Int32>();

         var equality = AcceptorFactory.NewEqualityComparisonAcceptor( visitor );
         var hashCode = AcceptorFactory.NewHashCodeComputationAcceptor( visitor );
         var cloner = AcceptorFactory.NewCopyingAcceptor<IOpCodeInfo, CAMPhysical::CILAssemblyManipulator.Physical.Meta.CAMCopyingContext, CAMPhysical::CILAssemblyManipulator.Physical.Meta.CopyingArgs>( visitor, () => new CAMPhysical::CILAssemblyManipulator.Physical.Meta.CAMCopyingContext(), ( el, ctx, arg ) =>
         {
            ctx.IsDeepCopy = arg.IsDeep;
            ctx.TableIndexTransformer = arg.TableIndexTransformer;
         } );

         foreach ( var typeInfo in typeInfos )
         {
            var codeType = typeInfo.OpCodeType;
            // Edges
            using ( var factory = visitor.CreateVertexInfoFactory( codeType ) )
            {
               typeInfo.RegisterEdgesForVisitor?.Invoke( factory, equality );
            }

            // Equality
            typeInfo.RegisterEquality( equality );

            // Hash code
            typeInfo.RegisterHashCode?.Invoke( hashCode );

            // Cloning
            var cloningFunc = typeInfo.CloningFunctionality;
            if ( cloningFunc != null )
            {
               cloner.RegisterVertexAcceptor( codeType, cloningFunc );
            }
         }

         // Expose visitor and acceptors via types.
         this.RegisterFunctionalityDirect( visitor );
         this.RegisterFunctionalityDirect( equality.Acceptor );
         this.RegisterFunctionalityDirect( hashCode.Acceptor );
         this.RegisterFunctionalityDirect( cloner.Acceptor );


         this.SerializationInfos = serializationInfos.ToArrayProxy().CQ;
         this._infosDictionary = this.SerializationInfos.ToDictionary( info => info.Code, info => info, ReferenceEqualityComparer<OpCode>.ReferenceBasedComparer );
      }

      protected AbstractDefaultOpCodeProvider( AbstractDefaultOpCodeProvider another )
      {
         ArgumentValidator.ValidateNotNull( "Another op code provider", another );

         this.RegisterFunctionalityDirect( another.GetFunctionality<AutomaticTypeBasedVisitor<IOpCodeInfo, Int32>>() );
         this.RegisterFunctionalityDirect( another.GetFunctionality<TOpCodeEqualityAcceptor>() );
         this.RegisterFunctionalityDirect( another.GetFunctionality<TOpCodeHashCodeAcceptor>() );
         this.RegisterFunctionalityDirect( another.GetFunctionality<TOpCodeCloningAcceptor>() );

         this.SerializationInfos = another.SerializationInfos;
         this._infosDictionary = another._infosDictionary;
      }

      public OpCodeInfoWithNoOperand GetOperandlessInfoOrNull( OpCode codeID )
      {
         OpCodeSerializationInfo retVal;
         return this._infosDictionary.TryGetValue( codeID, out retVal ) ? (OpCodeInfoWithNoOperand) retVal.StatelessInstance : null;
      }

      /// <inheritdoc />
      public OpCodeSerializationInfo GetSerializationInfoOrNull( OpCode codeID )
      {
         OpCodeSerializationInfo info;
         return this._infosDictionary.TryGetValue( codeID, out info ) ? info : null;
      }

      /// <inheritdoc />
      public SerializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateSerializer( OpCodeSerializationArguments args )
      {
         // Op codes do not require recursive functionality, so just use basic key-based serializer
         // Also, the keys are not System.Types - they are OpCode instances
         return new KeyBasedSerializer<ArrayIndex<Byte>, IOpCodeInfo, OpCode>(
            this.SerializationInfos.ToDictionary(
               info => info.Code,
               info => info.CreateSerializer( args ),
               ReferenceEqualityComparer<OpCode>.ReferenceBasedComparer
            ),
            instance => instance.OpCodeID
            );
      }

      /// <inheritdoc />
      public abstract DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateDeserializer( OpCodeDeserializationArguments args );

      /// <inheritdoc />
      public abstract Int32 CreateOptimizedOpCodeProvider( EmittingNativeHelper emittingHelper );

      /// <inheritdoc />
      public abstract CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider InstantiateOptimizedOpCodeProvider( Type generatedType );

      public ArrayQuery<OpCodeSerializationInfo> SerializationInfos { get; }
   }

#pragma warning restore 1591


   /// <summary>
   /// This class provides default implementation for <see cref="OpCodeProvider"/>.
   /// It caches all the <see cref="OpCode"/>s, and also caches all <see cref="OpCodeInfoWithNoOperand"/>s based on their <see cref="OpCodeID"/>.
   /// </summary>
   public class DefaultOpCodeProvider : AbstractDefaultOpCodeProvider
   {
      /// <summary>
      /// Contains the maximum value for <see cref="OpCodeID"/> which would fit into one byte.
      /// </summary>
      /// <remarks>This value is <c>0xFE</c>.</remarks>
      private const Int32 MAX_ONE_BYTE_INSTRUCTION = 0xFE;


      /// <summary>
      /// Gets the default instance of <see cref="DefaultOpCodeProvider"/>.
      /// It has support for codes returned by <see cref="GetDefaultOpCodes"/>.
      /// </summary>
      public static CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider DefaultInstance { get; }



      static DefaultOpCodeProvider()
      {
         DefaultInstance = new DefaultOpCodeProvider();
      }

      private const Int32 INFOS_ARRAY_SIZE = Byte.MaxValue;

      private readonly OpCodeSerializationInfo[] _infosBySerializedValue_Byte1;
      private readonly OpCodeSerializationInfo[] _infosBySerializedValue_Byte2;

      /// <summary>
      /// Creates a new instance of <see cref="DefaultOpCodeProvider"/> with support for given op code set.
      /// </summary>
      /// <param name="typeInfos">The type information about possible types of op code instances. If <c>null</c>, the return value of <see cref="GetDefaultTypeInfos"/> will be used.</param>
      /// <param name="codes">The <see cref="OpCodeSerializationInfo"/>s to support. If <c>null</c>, the return value of <see cref="GetDefaultOpCodes"/> will be used.</param>
      public DefaultOpCodeProvider(
         IEnumerable<OpCodeElementTypeInfo> typeInfos = null,
         IEnumerable<OpCodeSerializationInfo> codes = null
         ) : base( typeInfos ?? GetDefaultTypeInfos(), codes ?? GetDefaultOpCodes() )
      {

         var allInfos = this.SerializationInfos; // this.GetAllSerializationInfos(); // this._infosArray.Where( info => info != null ).Concat( tooBigCodes.Values );
         this._infosBySerializedValue_Byte1 = new OpCodeSerializationInfo[Byte.MaxValue];
         allInfos
            .Where( info => info.CodeSize == 1 )
            .ToArray_SelfIndexing(
               info => info.SerializedValue,
               CollectionOverwriteStrategy.Throw,
               arrayFactory: len => this._infosBySerializedValue_Byte1
               );
         this._infosBySerializedValue_Byte2 = new OpCodeSerializationInfo[Byte.MaxValue];
         allInfos
            .Where( info => info.CodeSize == 2 )
            .ToArray_SelfIndexing(
               info => info.SerializedValue & Byte.MaxValue,
               CollectionOverwriteStrategy.Throw,
               arrayFactory: len => this._infosBySerializedValue_Byte2
               );

         // TEMP CODE FOR IL INSPECTION
         var lel = this.SerializationInfos
            .OfType<OpCodeSerializationInfo_Operandless>()
            .ToArray_SelfIndexing_Throw(
               info =>
               {
                  var serValue = (Int32) unchecked((UInt16) info.SerializedValue);
                  if ( serValue > MAX_ONE_BYTE_INSTRUCTION )
                  {
                     serValue = Byte.MaxValue + ( serValue & Byte.MaxValue );
                  }
                  return serValue;
               },
               newSize => new OpCodeSerializationInfo_Operandless[Math.Max( newSize, Byte.MaxValue * 2 )]
            );
      }


      ///// <inheritdoc />
      //public Boolean TryReadOpCode( Byte[] array, Int32 index, out OpCodeSerializationInfo info )
      //{
      //   var startIdx = index;
      //   var b = array[index];
      //   info = b == MAX_ONE_BYTE_INSTRUCTION ?
      //      this._infosBySerializedValue_Byte2[array[index + 1]] :
      //      info = this._infosBySerializedValue_Byte1[b];
      //   return info != null;
      //}


      /// <inheritdoc />
      public override DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateDeserializer( OpCodeDeserializationArguments args )
      {
         return new NumericIDBasedDeserializer<ArrayIndex<Byte>, IOpCodeInfo>(
            this.SerializationInfos.ToDictionary( info => (Int32) info.SerializedValue, info => info.CreateDeserializer( args ) ),
            ReadOpCodeID,
            this._infosBySerializedValue_Byte1.Concat( this._infosBySerializedValue_Byte2 ).Select( i => i?.CreateDeserializer( args ) ).ToArray(),
            TransformArrayIndexToDictionaryIndex
            );
      }

      /// <inheritdoc />
      public override Int32 CreateOptimizedOpCodeProvider( EmittingNativeHelper emittingHelper )
      {
         var module = emittingHelper.Module;
         var fields = module.FieldDefinitions.TableContents;
         var methods = module.MethodDefinitions;
         var paramz = module.ParameterDefinitions.TableContents;

         // Create type def
         var opCodeProviderIdx = module.TypeDefinitions.AddRow( new TypeDefinition( fields.Count, methods.TableContents.Count )
         {
            Attributes = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class,
            BaseType = emittingHelper.GetTypeRefOrSpec( typeof( AbstractDefaultOpCodeProvider ) ),
            Name = "OptimizedOpCodeProvider"
         } );

         // Create constructor
         var ctorSig = new MethodDefinitionSignature()
         {
            MethodSignatureInformation = MethodSignatureInformation.HasThis,
            ReturnType = new ParameterSignature()
            {
               Type = new SimpleTypeSignature( SimpleTypeSignatureKind.Void )
            }
         };
         ctorSig.Parameters.Add( new ParameterSignature()
         {
            Type = emittingHelper.GetTypeSignature( typeof( AbstractDefaultOpCodeProvider ) )
         } );
         var ctor = new MethodDefinition( paramz.Count )
         {
            Attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            ImplementationAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed,
            Name = Miscellaneous.INSTANCE_CTOR_NAME,
            Signature = ctorSig
         };
         var ctorIL = new MethodILDefinition()
         {
            InitLocals = true
         };
         ctor.IL = ctorIL;
         ctorIL.OpCodes.AddRange( new OpCodeInfo[]
         {
            this.GetOperandlessInfoFor(OpCodes.Ldarg_0),
            this.GetOperandlessInfoFor(OpCodes.Ldarg_1),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, emittingHelper.GetMemberRefOrSpec(typeof(AbstractDefaultOpCodeProvider).GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).First( curCtor => curCtor.GetParameters().Length == 1 && Equals(curCtor.GetParameters()[0].ParameterType, typeof( AbstractDefaultOpCodeProvider ))))),
            this.GetOperandlessInfoFor(OpCodes.Ret)
         } );
         var ctorIndex = methods.AddRow( ctor );
         ctorIL.MaxStackSize = module.CalculateStackSize( ctorIndex.Index );
         paramz.Add( new ParameterDefinition()
         {
            Attributes = ParameterAttributes.None,
            Name = "another",
            Sequence = 1
         } );

         // Implement this method
         var thisMethodSignature = new MethodDefinitionSignature( 1 )
         {
            MethodSignatureInformation = MethodSignatureInformation.HasThis,
            ReturnType = new ParameterSignature()
            {
               Type = emittingHelper.GetTypeSignature( typeof( Int32 ) )
            }
         };
         thisMethodSignature.Parameters.Add( new ParameterSignature()
         {
            Type = emittingHelper.GetTypeSignature( typeof( EmittingNativeHelper ) )
         } );
         var thisMethodIL = new MethodILDefinition()
         {
            InitLocals = true
         };
         thisMethodIL.OpCodes.AddRange( new OpCodeInfo[]
         {
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Newobj, emittingHelper.GetMemberRefOrSpec( typeof(NotSupportedException).GetConstructor(Empty<Type>.Array))),
            this.GetOperandlessInfoFor(OpCodes.Throw)
         } );
         methods.AddRow( new MethodDefinition( paramz.Count )
         {
            Attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Final,
            Name = nameof( OpCodeProvider.CreateOptimizedOpCodeProvider ),
            Signature = thisMethodSignature,
            IL = thisMethodIL
         } );
         paramz.Add( new ParameterDefinition()
         {
            Attributes = ParameterAttributes.None,
            Name = "module",
            Sequence = 1
         } );

         // Implement instantiation method
         var instantiationMethodSignature = new MethodDefinitionSignature( 1 )
         {
            MethodSignatureInformation = MethodSignatureInformation.HasThis,
            ReturnType = new ParameterSignature()
            {
               Type = emittingHelper.GetTypeSignature( typeof( CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider ) )
            }
         };
         instantiationMethodSignature.Parameters.Add( new ParameterSignature()
         {
            Type = emittingHelper.GetTypeSignature( typeof( Type ) )
         } );
         methods.AddRow( new MethodDefinition( paramz.Count )
         {
            Attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Final,
            Name = nameof( OpCodeProvider.InstantiateOptimizedOpCodeProvider ),
            Signature = instantiationMethodSignature,
            IL = thisMethodIL
         } );
         paramz.Add( new ParameterDefinition()
         {
            Attributes = ParameterAttributes.None,
            Name = "type",
            Sequence = 1
         } );


         // Deserializator creation method
         var locals = new LocalVariablesSignature();
         var methodIL = new MethodILDefinition()
         {
            InitLocals = true,
            LocalsSignatureIndex = module.StandaloneSignatures.AddRow( new StandaloneSignature()
            {
               Signature = locals
            } )
         };

         // ArrayIndex<Byte>
         var arrayIndexSig = new ClassOrValueTypeSignature( 1 )
         {
            TypeReferenceKind = TypeReferenceKind.ValueType,
            Type = emittingHelper.GetTypeRefOrSpec( typeof( ArrayIndex<> ) )
         };
         arrayIndexSig.GenericArguments.Add( new SimpleTypeSignature( SimpleTypeSignatureKind.U1 ) );

         // DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo>
         var deserializerSig = new ClassOrValueTypeSignature( 2 )
         {
            TypeReferenceKind = TypeReferenceKind.Class,
            Type = emittingHelper.GetTypeRefOrSpec( typeof( DeserializationFunctionality<,> ) )
         };
         deserializerSig.GenericArguments.Add( arrayIndexSig );
         deserializerSig.GenericArguments.Add( new ClassOrValueTypeSignature()
         {
            TypeReferenceKind = TypeReferenceKind.Class,
            Type = emittingHelper.GetTypeRefOrSpec( typeof( IOpCodeInfo ) )
         } );

         // DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> CreateDeserializer( OpCodeDeserializationArguments args )
         var methodSig = new MethodDefinitionSignature( 1 )
         {
            MethodSignatureInformation = MethodSignatureInformation.HasThis
         };
         methodSig.ReturnType = new ParameterSignature()
         {
            Type = deserializerSig
         };
         methodSig.Parameters.Add( new ParameterSignature()
         {
            Type = new ClassOrValueTypeSignature()
            {
               TypeReferenceKind = TypeReferenceKind.Class,
               Type = emittingHelper.GetTypeRefOrSpec( typeof( OpCodeDeserializationArguments ) )
            }
         } );

         var creationIndex = methods.AddRow( new MethodDefinition( paramz.Count )
         {
            Attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
            ImplementationAttributes = MethodImplAttributes.Managed | MethodImplAttributes.IL,
            Name = nameof( OpCodeProvider.CreateDeserializer ),
            Signature = methodSig,
            IL = methodIL
         } );
         paramz.Add( new ParameterDefinition()
         {
            Attributes = ParameterAttributes.None,
            Name = "args",
            Sequence = 1
         } );

         // Create optimized serializer type
         var deserializerCreationCode = this.CreateOptimizedDeserializer( emittingHelper, opCodeProviderIdx, deserializerSig );
         deserializerCreationCode( methodIL.OpCodes );
         methodIL.OpCodes.Add( this.GetOperandlessInfoFor( OpCodes.Ret ) );
         methodIL.MaxStackSize = module.CalculateStackSize( creationIndex.Index );


         return opCodeProviderIdx.Index;
      }

      /// <inheritdoc />
      public override CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider InstantiateOptimizedOpCodeProvider( Type generatedType )
      {
         return (CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider) generatedType
            .GetConstructor( new Type[] { typeof( AbstractDefaultOpCodeProvider ) } )
            .Invoke( new Object[] { this } );
      }

      private Action<List<OpCodeInfo>> CreateOptimizedDeserializer( EmittingNativeHelper emittingHelper, TableIndex providerTypeDef, ClassOrValueTypeSignature deserializerSignature )
      {
         var module = emittingHelper.Module;
         var fields = module.FieldDefinitions;
         var methods = module.MethodDefinitions;
         var paramz = module.ParameterDefinitions.TableContents;

         // Add new nested type
         var deserializerIndex = module.TypeDefinitions.AddRow( new TypeDefinition( fields.TableContents.Count, methods.TableContents.Count )
         {
            Attributes = TypeAttributes.NestedPrivate | TypeAttributes.Sealed | TypeAttributes.Class | TypeAttributes.BeforeFieldInit,
            Name = "OptimizedDeserializer",
            BaseType = emittingHelper.GetTypeRefOrSpec( typeof( Object ) )
         } );
         module.NestedClassDefinitions.AddRow( new NestedClassDefinition( deserializerIndex.Index, providerTypeDef.Index ) );

         // Add interface implementation
         module.InterfaceImplementations.AddRow( new InterfaceImplementation( deserializerIndex.Index )
         {
            Interface = emittingHelper.GetTypeRefOrSpec( typeof( DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo> ) )
         } );

         // Add fields
         var codesField = fields.AddRow( new FieldDefinition()
         {
            Attributes = FieldAttributes.InitOnly | FieldAttributes.Private,
            Name = "_codes",
            Signature = new FieldSignature()
            {
               Type = emittingHelper.GetTypeSignature( typeof( OpCode[] ) )
            }
         } );
         var serializationArgsField = fields.AddRow( new FieldDefinition()
         {
            Attributes = FieldAttributes.InitOnly | FieldAttributes.Private,
            Name = "_args",
            Signature = new FieldSignature()
            {
               Type = emittingHelper.GetTypeSignature( typeof( OpCodeDeserializationArguments ) )
            }
         } );
         var codeTypeIdsField = fields.AddRow( new FieldDefinition()
         {
            Attributes = FieldAttributes.InitOnly | FieldAttributes.Private,
            Name = "_typeIDs",
            Signature = new FieldSignature()
            {
               Type = emittingHelper.GetTypeSignature( typeof( Byte[] ) )
            }
         } );


         // Add constructor
         var ctorSig = new MethodDefinitionSignature()
         {
            MethodSignatureInformation = MethodSignatureInformation.HasThis,
            ReturnType = new ParameterSignature()
            {
               Type = new SimpleTypeSignature( SimpleTypeSignatureKind.Void )
            }
         };
         ctorSig.Parameters.AddRange( new ParameterSignature[]
         {
            new ParameterSignature()
            {
               Type = emittingHelper.GetTypeSignature( typeof(AbstractDefaultOpCodeProvider))
            },
            new ParameterSignature()
            {
               Type = emittingHelper.GetTypeSignature( typeof(OpCodeDeserializationArguments))
            }
         } );
         var ctor = new MethodDefinition( paramz.Count )
         {
            Attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            ImplementationAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed,
            Name = Miscellaneous.INSTANCE_CTOR_NAME,
            Signature = ctorSig
         };
         var ctorLocals = new LocalVariablesSignature();
         // local: codeIDArrayLen
         ctorLocals.Locals.Add( new LocalSignature()
         {
            Type = emittingHelper.GetTypeSignature( typeof( Int32 ) )
         } );
         // local: idx
         ctorLocals.Locals.Add( new LocalSignature()
         {
            Type = emittingHelper.GetTypeSignature( typeof( Int32 ) )
         } );
         // local: info
         ctorLocals.Locals.Add( new LocalSignature()
         {
            Type = emittingHelper.GetTypeSignature( typeof( OpCodeSerializationInfo ) )
         } );
         // local: codes
         ctorLocals.Locals.Add( new LocalSignature()
         {
            Type = emittingHelper.GetTypeSignature( typeof( OpCode[] ) )
         } );
         // local: enumerator for foreach loop
         ctorLocals.Locals.Add( new LocalSignature()
         {
            Type = emittingHelper.GetTypeSignature( typeof( Dictionary<Int32, OpCodeSerializationInfo>.Enumerator ) )
         } );
         // local: key-value-pair for foreach loop
         ctorLocals.Locals.Add( new LocalSignature()
         {
            Type = emittingHelper.GetTypeSignature( typeof( KeyValuePair<Int32, OpCodeSerializationInfo> ) )
         } );

         var ctorIL = new MethodILDefinition()
         {
            InitLocals = true,
            LocalsSignatureIndex = module.StandaloneSignatures.AddRow( new StandaloneSignature()
            {
               Signature = ctorLocals
            } )
         };
         ctor.IL = ctorIL;

         ctorIL.OpCodes.AddRange( new OpCodeInfo[]
         {
            // base();
            this.GetOperandlessInfoFor(OpCodes.Ldarg_0),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, emittingHelper.GetMemberRefOrSpec( typeof(Object).GetConstructor(Empty<Type>.Array))),

            // this._args = args;
            this.GetOperandlessInfoFor(OpCodes.Ldarg_0),
            this.GetOperandlessInfoFor(OpCodes.Ldarg_2),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Stfld, serializationArgsField),

            // var dictionary = opc.SerializationInfos.ToDictionary( GetOpCodeSerializationID, Identity<OpCodeSerializationInfo>.Function ); 
            // (save on stack instead of local)
            this.GetOperandlessInfoFor(OpCodes.Ldarg_1),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Callvirt, emittingHelper.GetMemberRefOrSpec( typeof(AbstractDefaultOpCodeProvider).GetProperty(nameof(DefaultOpCodeProvider.SerializationInfos), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetGetMethod(true))),
            this.GetOperandlessInfoFor(OpCodes.Ldnull),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Ldftn, emittingHelper.GetMemberRefOrSpec( typeof(DefaultOpCodeProvider).GetMethod(nameof(DefaultOpCodeProvider.GetOpCodeSerializationID), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public))),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Newobj, emittingHelper.GetMemberRefOrSpec( typeof(Func<OpCodeSerializationInfo, Int32>).GetConstructor(new Type[] { typeof(Object), typeof(System.IntPtr) }))),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, emittingHelper.GetMemberRefOrSpec( typeof(Identity<OpCodeSerializationInfo>).GetProperty(nameof(Identity<OpCodeSerializationInfo>.Function)).GetGetMethod())),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, emittingHelper.GetMemberRefOrSpec( typeof( Enumerable ).GetMethods()
               .First( m => String.Equals( m.Name, nameof( Enumerable.ToDictionary ) ) && m.GetParameters().Length == 3 && m.GetGenericArguments().Length == 3 )
               .MakeGenericMethod( typeof( OpCodeSerializationInfo ), typeof( Int32 ), typeof( OpCodeSerializationInfo ) ) ) ),

            // var codeIDArrayLen = dic.Keys.Max() + 1;
            this.GetOperandlessInfoFor(OpCodes.Dup),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Callvirt, emittingHelper.GetMemberRefOrSpec( typeof(Dictionary<Int32, OpCodeSerializationInfo>).GetProperty(nameof(Dictionary<Int32, OpCodeSerializationInfo>.Keys)).GetGetMethod())),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, emittingHelper.GetMemberRefOrSpec( typeof(Enumerable).GetMethod("Max", new Type[] { typeof(IEnumerable<Int32>) }))),
            this.GetOperandlessInfoFor(OpCodes.Ldc_I4_1),
            this.GetOperandlessInfoFor(OpCodes.Add),
            this.GetOperandlessInfoFor(OpCodes.Stloc_0),

            // var codes = new OpCode[codeIDArrayLen];
            this.GetOperandlessInfoFor(OpCodes.Ldloc_0),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Newarr, emittingHelper.GetTypeRefOrSpec( typeof(OpCode))),
            this.GetOperandlessInfoFor(OpCodes.Stloc_3),

            // this._typeIDs = new Int32[codeIDarrayLen];
            this.GetOperandlessInfoFor(OpCodes.Ldarg_0),
            this.GetOperandlessInfoFor(OpCodes.Ldloc_0),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Newarr, emittingHelper.GetTypeRefOrSpec( typeof(Byte))),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Stfld, codeTypeIdsField)
         } );
         var ctorIndex = methods.AddRow( ctor );

         paramz.AddRange( new ParameterDefinition[]
         {
            new ParameterDefinition()
            {
               Attributes = ParameterAttributes.None,
               Name = "opc",
               Sequence = 1
            },
            new ParameterDefinition()
            {
               Attributes = ParameterAttributes.None,
               Name = "args",
               Sequence = 2
            }
         } );

         // Add method implementation
         var locals = new LocalVariablesSignature( 5 );
         // local: array
         locals.Locals.Add( new LocalSignature()
         {
            Type = emittingHelper.GetTypeSignature( typeof( Byte[] ) )
         } );
         // local: index
         locals.Locals.Add( new LocalSignature()
         {
            Type = emittingHelper.GetTypeSignature( typeof( Int32 ) )
         } );
         // local: codeID
         locals.Locals.Add( new LocalSignature()
         {
            Type = emittingHelper.GetTypeSignature( typeof( Int32 ) )
         } );
         // local: retVal
         locals.Locals.Add( new LocalSignature()
         {
            Type = emittingHelper.GetTypeSignature( typeof( IOpCodeInfo ) )
         } );
         // local: originalIndex
         locals.Locals.Add( new LocalSignature()
         {
            Type = emittingHelper.GetTypeSignature( typeof( Int32 ) )
         } );

         // Add op codes
         var il = new MethodILDefinition()
         {
            InitLocals = true,
            LocalsSignatureIndex = module.StandaloneSignatures.AddRow( new StandaloneSignature()
            {
               Signature = locals
            } )
         };
         var codes = il.OpCodes;
         codes.AddRange( new OpCodeInfo[]
         {
            // var array = source.Array;
            new OpCodeInfoWithOperand<Int32>( OpCodes.Ldarga_S, 1 ),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, emittingHelper.GetMemberRefOrSpec( typeof(ArrayIndex<Byte>).GetProperty(nameof(ArrayIndex<Byte>.Array)).GetGetMethod())),
            this.GetOperandlessInfoFor(OpCodes.Stloc_0),

            // var index = source.Index
            new OpCodeInfoWithOperand<Int32>(OpCodes.Ldarga_S, 1 ),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, emittingHelper.GetMemberRefOrSpec( typeof(ArrayIndex<Byte>).GetProperty(nameof(ArrayIndex<Byte>.Index)).GetGetMethod())),
            this.GetOperandlessInfoFor(OpCodes.Stloc_1),

            // var originalIndex = index
            this.GetOperandlessInfoFor(OpCodes.Ldloc_1),
            new OpCodeInfoWithOperand<Int32>(OpCodes.Stloc_S, 4),

            // Int32 codeID = array[index++];
            this.GetOperandlessInfoFor(OpCodes.Ldloc_0),
            this.GetOperandlessInfoFor(OpCodes.Ldloc_1),
            this.GetOperandlessInfoFor(OpCodes.Dup),
            this.GetOperandlessInfoFor(OpCodes.Ldc_I4_1),
            this.GetOperandlessInfoFor(OpCodes.Add),
            this.GetOperandlessInfoFor(OpCodes.Stloc_1),
            this.GetOperandlessInfoFor(OpCodes.Ldelem_U1),
            this.GetOperandlessInfoFor(OpCodes.Stloc_2),

            // if ( codeID == 0xFE )
            this.GetOperandlessInfoFor(OpCodes.Ldloc_2),
            new OpCodeInfoWithOperand<Int32>(OpCodes.Ldc_I4, MAX_ONE_BYTE_INSTRUCTION),
            new OpCodeInfoWithOperand<Int32>(OpCodes.Bne_Un_S, 0x0E),

            // {
            //   codeID = Byte.MaxValue + array[index++];
            // }
            new OpCodeInfoWithOperand<Int32>(OpCodes.Ldc_I4, Byte.MaxValue),
            this.GetOperandlessInfoFor(OpCodes.Ldloc_0),
            this.GetOperandlessInfoFor(OpCodes.Ldloc_1),
            this.GetOperandlessInfoFor(OpCodes.Dup),
            this.GetOperandlessInfoFor(OpCodes.Ldc_I4_1),
            this.GetOperandlessInfoFor(OpCodes.Add),
            this.GetOperandlessInfoFor(OpCodes.Stloc_1),
            this.GetOperandlessInfoFor(OpCodes.Ldelem_U1),
            this.GetOperandlessInfoFor(OpCodes.Add),
            this.GetOperandlessInfoFor(OpCodes.Stloc_2),

            // Load argument for switch
            // this._typeIDs[codeID]
            this.GetOperandlessInfoFor(OpCodes.Ldarg_0),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Ldfld, codeTypeIdsField),
            this.GetOperandlessInfoFor(OpCodes.Ldloc_2),
            this.GetOperandlessInfoFor(OpCodes.Ldelem_I1)
         } );

         // Generate switch statement
         var infosCodeIDs = this.SerializationInfos.ToDictionary(
            Identity<OpCodeSerializationInfo>.Function,
            GetOpCodeSerializationID,
            ReferenceEqualityComparer<OpCodeSerializationInfo>.ReferenceBasedComparer
            );
         var uniqueSerializationInfosByType = this.SerializationInfos
            .GroupBy( info => info.GetType() )
            .Select( group => group.First() ) //.ToArray() )
            .ToArray();
         //.ToDictionary( group => group.Key, group =>
         //{
         //   var arr = group.ToArray();
         //   Array.Sort( arr, ( info1, info2 ) => info1.SerializedValue.CompareTo( info2.SerializedValue ) );
         //   return arr;
         //} );
         var switchMax = uniqueSerializationInfosByType.Length; // infosCodeIDs.Values.Max() + 1;
         var switchCode = new OpCodeInfoWithList<Int32>( OpCodes.Switch, switchMax );
         switchCode.Operand.AddRange( Enumerable.Repeat( 0, switchMax ) );
         var branchToDefaultCode = new OpCodeInfoWithOperand<Int32>( OpCodes.Br, 0 );

         var curByteCount = codes.Aggregate( 0, ( cur, oc ) => cur + this.GetTotalByteCount( oc ) );
         var switchAndBranch = new OpCodeInfo[]
         {
            switchCode,
            branchToDefaultCode
         };
         codes.AddRange( switchAndBranch );
         var switchStartByteCount = curByteCount + this.GetTotalByteCount( switchCode );
         curByteCount = switchStartByteCount + this.GetTotalByteCount( branchToDefaultCode );
         var branchToDefaultStart = curByteCount;
         var branchesToOutside = new List<Tuple<OpCodeInfoWithOperand<Int32>, Int32>>();
         var ctorCodesBeforeForLoop = new List<OpCodeInfo>();
         var ctorCodesInsideForLoop = new List<OpCodeInfo>();
         var ctorCodesAfterForLoop = new List<OpCodeInfo>();


         for ( var i = 0; i < uniqueSerializationInfosByType.Length; ++i )
         {
            var infoArgs = new OptimizedCodeGenerationArgs(
            module,
            this,
            codes,
            ctorCodesBeforeForLoop,
            ctorCodesInsideForLoop,
            ctorCodesAfterForLoop,
            nType => emittingHelper.GetTypeSignature( nType ),
            nType => emittingHelper.GetTypeRefOrSpec( nType ),
            nMethod => emittingHelper.GetMemberRefOrSpec( nMethod ),
            new OpCodeInfo[] { this.GetOperandlessInfoFor( OpCodes.Ldloc_0 ) }.ToArrayProxy().CQ,
            new OpCodeInfo[] { this.GetOperandlessInfoFor( OpCodes.Ldloc_1 ) }.ToArrayProxy().CQ,
            new OpCodeInfo[] { this.GetOperandlessInfoFor( OpCodes.Stloc_1 ) }.ToArrayProxy().CQ,
            new OpCodeInfo[] { new OpCodeInfoWithOperand<Int32>( OpCodes.Ldloca_S, 1 ) }.ToArrayProxy().CQ,
            new OpCodeInfo[] { this.GetOperandlessInfoFor( OpCodes.Ldloc_2 ) }.ToArrayProxy().CQ,
            new OpCodeInfo[] {
               this.GetOperandlessInfoFor(OpCodes.Ldarg_0),
               new OpCodeInfoWithOperand<TableIndex>(OpCodes.Ldfld, codesField),
               this.GetOperandlessInfoFor(OpCodes.Ldloc_2),
               this.GetOperandlessInfoFor(OpCodes.Ldelem_Ref),
            }.ToArrayProxy().CQ,
            new OpCodeInfo[] { this.GetOperandlessInfoFor( OpCodes.Ldarg_0 ), new OpCodeInfoWithOperand<TableIndex>( OpCodes.Ldfld, serializationArgsField ) }.ToArrayProxy().CQ,
            new OpCodeInfo[] { this.GetOperandlessInfoFor( OpCodes.Stloc_3 ) }.ToArrayProxy().CQ,
            new OpCodeInfo[] { this.GetOperandlessInfoFor( OpCodes.Ldloc_3 ) }.ToArrayProxy().CQ,
            // OpCodeInfos that should go to constructor
            new OpCodeInfo[] { this.GetOperandlessInfoFor( OpCodes.Ldloc_0 ) }.ToArrayProxy().CQ,
            new OpCodeInfo[] { this.GetOperandlessInfoFor( OpCodes.Ldloc_1 ) }.ToArrayProxy().CQ,
            new OpCodeInfo[] { this.GetOperandlessInfoFor( OpCodes.Ldloc_2 ) }.ToArrayProxy().CQ,
            lSig =>
            {
               var lIdx = locals.Locals.Count;
               locals.Locals.Add( lSig );
               return lIdx;
            },
            lSig =>
            {
               var lIdx = ctorLocals.Locals.Count;
               ctorLocals.Locals.Add( lSig );
               return lIdx;
            } );

            switchCode.Operand[i] = curByteCount - switchStartByteCount;

            //// All codes of this type with various IDs will branch to here
            //foreach ( var info in kvp.Value )
            //{
            //   var thisCodeID = infosCodeIDs[info];
            //   switchCode.Operand[thisCodeID] = curByteCount - switchStartByteCount;
            //}

            var first = uniqueSerializationInfosByType[i]; // kvp.Value[0];
            var curOpCodeCount = codes.Count;
            first.GenerateCodeForOptimizedDeserialization( infoArgs );
            if ( !infoArgs.StoredToResult )
            {
               codes.AddRange( infoArgs.StoreToResult );
            }
            // Branch outside
            var outsideBranch = new OpCodeInfoWithOperand<Int32>( OpCodes.Br, 0 );
            codes.Add( outsideBranch );

            for ( var j = curOpCodeCount; j < codes.Count; ++j )
            {
               curByteCount += this.GetTotalByteCount( codes[j] );
            }


            branchesToOutside.Add( Tuple.Create( outsideBranch, curByteCount ) );
         }
         branchToDefaultCode.Operand = curByteCount - branchToDefaultStart;
         var defaultBranchCountInSwitch = curByteCount - switchStartByteCount;
         // Set all zeroes of switch case to branch to default
         var switchList = switchCode.Operand;
         for ( var i = 0; i < switchList.Count; ++i )
         {
            if ( switchList[i] == 0 )
            {
               switchList[i] = defaultBranchCountInSwitch;
            }
         }

         // Emit default branch
         var defaultBranchCodes = new OpCodeInfo[]
         {
            // retVal = null;
            this.GetOperandlessInfoFor(OpCodes.Ldnull),
            this.GetOperandlessInfoFor(OpCodes.Stloc_3)
         };
         codes.AddRange( defaultBranchCodes );
         curByteCount = defaultBranchCodes.Aggregate( curByteCount, ( cur, oc ) => cur + this.GetTotalByteCount( oc ) );

         // Set branch values for all outside-branching instructions
         foreach ( var outsideBranchInfo in branchesToOutside )
         {
            outsideBranchInfo.Item1.Operand = curByteCount - outsideBranchInfo.Item2;
         }

         codes.AddRange( new OpCodeInfo[]
         {
            // unitsProcessed = index - originalIndex;
            this.GetOperandlessInfoFor(OpCodes.Ldarg_2),
            this.GetOperandlessInfoFor(OpCodes.Ldloc_1),
            new OpCodeInfoWithOperand<Int32>(OpCodes.Ldloc_S, 4),
            this.GetOperandlessInfoFor(OpCodes.Sub),
            this.GetOperandlessInfoFor(OpCodes.Stind_I4),

            // return retVal;
            this.GetOperandlessInfoFor(OpCodes.Ldloc_3),
            this.GetOperandlessInfoFor(OpCodes.Ret)
         } );

         // We are done, add infrastructure info
         var deserializerMethodSignature = new MethodDefinitionSignature( 2 )
         {
            MethodSignatureInformation = MethodSignatureInformation.HasThis,
            ReturnType = new ParameterSignature()
            {
               Type = emittingHelper.GetTypeSignature( typeof( IOpCodeInfo ) )
            }
         };
         deserializerMethodSignature.Parameters.AddRange( new ParameterSignature[]
         {
            new ParameterSignature()
            {
               Type = emittingHelper.GetTypeSignature( typeof(ArrayIndex<Byte>))
            },
            new ParameterSignature()
            {
               IsByRef = true,
               Type = emittingHelper.GetTypeSignature( typeof(Int32))
            }
         } );
         var deserializationMethodIndex = methods.AddRow( new MethodDefinition( paramz.Count )
         {
            Attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
            Name = nameof( DeserializationFunctionality<ArrayIndex<Byte>, IOpCodeInfo>.Deserialize ),
            Signature = deserializerMethodSignature,
            IL = il
         } );
         paramz.AddRange( new ParameterDefinition[]
         {
            new ParameterDefinition()
            {
               Attributes = ParameterAttributes.None,
               Name = "source",
               Sequence = 1
            },
            new ParameterDefinition()
            {
               Attributes = ParameterAttributes.Out,
               Name = "unitsProcessed",
               Sequence = 2
            }
         } );

         il.MaxStackSize = module.CalculateStackSize( deserializationMethodIndex.Index );

         // Postprocess constructor IL
         ctorIL.OpCodes.AddRange( ctorCodesBeforeForLoop );

         // Foreach-loop
         Int32 tryStartByteCount, finallyStartByteCount;
         this.EmitForLoop(
            ctorIL.OpCodes,
            new OpCodeInfo[]
            {
               // enumerator = dictionary.GetEnumerator()
               new OpCodeInfoWithOperand<TableIndex>(OpCodes.Callvirt, emittingHelper.GetMemberRefOrSpec( typeof(Dictionary<Int32, OpCodeSerializationInfo>).GetMethod(nameof(Dictionary<Int32, OpCodeSerializationInfo>.GetEnumerator)))),
               new OpCodeInfoWithOperand<Int32>(OpCodes.Stloc_S, 4),
            },
            new OpCodeInfo[]
            {
               // enumerator.MoveNext();
               new OpCodeInfoWithOperand<Int32>(OpCodes.Ldloca_S, 4),
               new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, emittingHelper.GetMemberRefOrSpec( typeof(Dictionary<Int32, OpCodeSerializationInfo>.Enumerator).GetMethod(nameof(Dictionary<Int32, OpCodeSerializationInfo>.Enumerator.MoveNext)))),
            },
            OpCodes.Brtrue_S,
            OpCodes.Brtrue,
            null,
            new OpCodeInfo[]
            {
               // var kvp = enumerator.Current;
               new OpCodeInfoWithOperand<Int32>(OpCodes.Ldloca_S, 4),
               new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, emittingHelper.GetMemberRefOrSpec( typeof(Dictionary<Int32, OpCodeSerializationInfo>.Enumerator).GetProperty(nameof(Dictionary<Int32, OpCodeSerializationInfo>.Enumerator.Current)).GetGetMethod())),
               new OpCodeInfoWithOperand<Int32>(OpCodes.Stloc_S, 5),

               // var idx = kvp.Key
               new OpCodeInfoWithOperand<Int32>(OpCodes.Ldloca_S, 5),
               new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, emittingHelper.GetMemberRefOrSpec( typeof(KeyValuePair<Int32, OpCodeSerializationInfo>).GetProperty(nameof(KeyValuePair<Int32, OpCodeSerializationInfo>.Key)).GetGetMethod())),
               this.GetOperandlessInfoFor(OpCodes.Stloc_1),

               // var info = kvp.Value;
               new OpCodeInfoWithOperand<Int32>(OpCodes.Ldloca_S, 5),
               new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, emittingHelper.GetMemberRefOrSpec( typeof(KeyValuePair<Int32, OpCodeSerializationInfo>).GetProperty(nameof(KeyValuePair<Int32, OpCodeSerializationInfo>.Value)).GetGetMethod())),
               this.GetOperandlessInfoFor(OpCodes.Stloc_2),

               // codes[idx] = info.Code
               this.GetOperandlessInfoFor(OpCodes.Ldloc_3),
               this.GetOperandlessInfoFor(OpCodes.Ldloc_1),
               this.GetOperandlessInfoFor(OpCodes.Ldloc_2),
               new OpCodeInfoWithOperand<TableIndex>(OpCodes.Callvirt, emittingHelper.GetMemberRefOrSpec( typeof(OpCodeSerializationInfo).GetProperty(nameof(OpCodeSerializationInfo.Code)).GetGetMethod())),
               this.GetOperandlessInfoFor(OpCodes.Stelem_Ref)
            }.Concat( ctorCodesInsideForLoop ),
            out tryStartByteCount,
            out finallyStartByteCount
            );

         // Add Leave instruction
         var leaveInstruction = new OpCodeInfoWithOperand<Int32>( OpCodes.Leave_S, 0 );
         ctorIL.OpCodes.Add( leaveInstruction );
         finallyStartByteCount += this.GetTotalByteCount( leaveInstruction );

         // Add finally-block instructions
         var finallyInstructions = new OpCodeInfo[]
         {
            new OpCodeInfoWithOperand<Int32>(OpCodes.Ldloca_S, 4),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Constrained_, emittingHelper.GetTypeRefOrSpec( typeof(Dictionary<Int32, OpCodeSerializationInfo>.Enumerator))),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Callvirt, emittingHelper.GetMemberRefOrSpec( typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose)))),
            this.GetOperandlessInfoFor(OpCodes.Endfinally)
         };
         ctorIL.OpCodes.AddRange( finallyInstructions );
         var finallyLength = finallyInstructions.Aggregate( 0, ( cur, oc ) => cur + this.GetTotalByteCount( oc ) );
         ctorIL.ExceptionBlocks.Add( new MethodExceptionBlock()
         {
            BlockType = ExceptionBlockType.Finally,
            TryOffset = tryStartByteCount,
            TryLength = finallyStartByteCount - tryStartByteCount,
            HandlerOffset = finallyStartByteCount,
            HandlerLength = finallyLength
         } );

         // Update leave instruction offset
         leaveInstruction.Operand = finallyLength;

         // After for-loop
         ctorIL.OpCodes.AddRange( new OpCodeInfo[]
         {
            // this._codes = codes;
            this.GetOperandlessInfoFor(OpCodes.Ldarg_0),
            this.GetOperandlessInfoFor(OpCodes.Ldloc_3),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Stfld, codesField),

            // DefaultOpCodeProvider.PopulateTypeIDArray(this._typeIDs, provider);
            this.GetOperandlessInfoFor(OpCodes.Ldarg_0),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Ldfld, codeTypeIdsField),
            this.GetOperandlessInfoFor(OpCodes.Ldarg_1),
            new OpCodeInfoWithOperand<TableIndex>(OpCodes.Call, emittingHelper.GetMemberRefOrSpec( typeof(DefaultOpCodeProvider).GetMethod(nameof(DefaultOpCodeProvider.PopulateTypeIDArray)))),
         } );
         ctorIL.OpCodes.AddRange( ctorCodesAfterForLoop );


         // Remember 'Ret' for ctor IL here - since ctor IL might be modified by infos
         ctorIL.OpCodes.Add( this.GetOperandlessInfoFor( OpCodes.Ret ) );
         ctorIL.MaxStackSize = module.CalculateStackSize( ctorIndex.Index );

         // Return callback which will emit instrutions to instantiate deserializer
         return creationCodes =>
         {
            creationCodes.AddRange( new OpCodeInfo[]
            {
               this.GetOperandlessInfoFor(OpCodes.Ldarg_0),
               this.GetOperandlessInfoFor(OpCodes.Ldarg_1),
               new OpCodeInfoWithOperand<TableIndex>( OpCodes.Newobj, ctorIndex )
            } );
         };
      }


      /// <summary>
      /// 
      /// </summary>
      /// <param name="typeIDs"></param>
      /// <param name="provider"></param>
      public static void PopulateTypeIDArray( Byte[] typeIDs, AbstractDefaultOpCodeProvider provider )
      {
         var curID = 0;
         var typeDic = provider.SerializationInfos
            .GroupBy( i => i.GetType() )
            .ToDictionary( group => group.Key, group =>
             {
                var thisID = curID;
                ++curID;
                return thisID;
             } );
         foreach ( var info in provider.SerializationInfos )
         {
            typeIDs[GetOpCodeSerializationID( info )] = checked((Byte) typeDic[info.GetType()]);
         }
      }


      /// <summary>
      /// Gets the ID suitable to be used as array index for given <see cref="OpCodeSerializationInfo"/>.
      /// </summary>
      /// <param name="info">The <see cref="OpCodeSerializationInfo"/>.</param>
      /// <returns>Serialization ID for <paramref name="info"/>.</returns>
      public static Int32 GetOpCodeSerializationID( OpCodeSerializationInfo info )
      {
         Int32 codeID = unchecked((UInt16) info.SerializedValue);
         if ( info.CodeSize > 1 )
         {
            codeID = Byte.MaxValue + ( codeID & Byte.MaxValue );
         }
         return codeID;
      }



      private static Int32 ReadOpCodeID( ArrayIndex<Byte> source, out Int32 unitsProcessed, out ArrayIndex<Byte> newSource )
      {
         var array = source.Array;
         var idx = source.Index;
         Int32 firstByte = array[idx];
         if ( firstByte == MAX_ONE_BYTE_INSTRUCTION )
         {
            firstByte = Byte.MaxValue + array[idx + 1];
            unitsProcessed = 2;
         }
         else
         {
            unitsProcessed = 1;
         }
         newSource = new ArrayIndex<Byte>( array, idx + unitsProcessed );
         return firstByte;
      }


      private static Int32 TransformArrayIndexToDictionaryIndex( Int32 arrayIndex )
      {
         if ( arrayIndex >= Byte.MaxValue )
         {
            arrayIndex = ( MAX_ONE_BYTE_INSTRUCTION << 8 ) & ( arrayIndex >> 8 );
         }
         return arrayIndex;
      }

      //private IEnumerable<OpCodeSerializationInfo> GetAllSerializationInfos()
      //{
      //   return this._infosArray
      //      .Where( info => info != null )
      //      .Concat( this._infosDictionary.Values );
      //}

      /// <summary>
      /// Returns the <see cref="AbstractDefaultOpCodeProvider.OpCodeElementTypeInfo"/> instances for types deriving from <see cref="IOpCodeInfo"/> which are supported in CAM by default.
      /// </summary>
      /// <returns>The <see cref="AbstractDefaultOpCodeProvider.OpCodeElementTypeInfo"/> instances for default types deriving from <see cref="IOpCodeInfo"/>.</returns>
      public static IEnumerable<OpCodeElementTypeInfo> GetDefaultTypeInfos()
      {
         // Operandless
         yield return OpCodeElementTypeInfo.NewInfo<OpCodeInfoWithNoOperand>(
            ( x, y ) => true,
            x => unchecked(x.OpCodeID.GetHashCode() * 31), // ReferenceEqualityComparer<OpCodeInfoWithNoOperand>.ReferenceBasedComparer.GetHashCode( x )
            ( el, ctx, cb ) => el
            );

         // With settable operand: Int32
         yield return OpCodeElementTypeInfo.NewInfo<OpCodeInfoWithOperand<Int32>>(
            ( x, y ) => x.Operand == y.Operand,
            x => unchecked(x.Operand * 37),
            ( el, ctx, cb ) => new OpCodeInfoWithOperand<Int32>( el.OpCodeID, el.Operand )
            );

         // With settable operand: Int64
         yield return OpCodeElementTypeInfo.NewInfo<OpCodeInfoWithOperand<Int64>>(
            ( x, y ) => x.Operand == y.Operand,
            x => unchecked((Int32) x.Operand * 41),
            ( el, ctx, cb ) => new OpCodeInfoWithOperand<Int64>( el.OpCodeID, el.Operand )
            );

         // With settable operand: Single
         yield return OpCodeElementTypeInfo.NewInfo<OpCodeInfoWithOperand<Single>>(
            ( x, y ) => x.Operand.Equals( y.Operand ), // Use .Equals in order for NaN's to work more intuitively
            x => unchecked(x.Operand.GetHashCode() * 43),
            ( el, ctx, cb ) => new OpCodeInfoWithOperand<Single>( el.OpCodeID, el.Operand )
            );

         // With settable operand: Double
         yield return OpCodeElementTypeInfo.NewInfo<OpCodeInfoWithOperand<Double>>(
            ( x, y ) => x.Operand.Equals( y.Operand ), // Use .Equals in order for NaN's to work more intuitively
            x => unchecked(x.Operand.GetHashCode() * 47),
            ( el, ctx, cb ) => new OpCodeInfoWithOperand<Double>( el.OpCodeID, el.Operand )
            );

         // With settable operand: String
         yield return OpCodeElementTypeInfo.NewInfo<OpCodeInfoWithOperand<String>>(
            ( x, y ) => String.Equals( x.Operand, y.Operand ),
            x => x.Operand?.GetHashCode() ?? 0,
            ( el, ctx, cb ) => new OpCodeInfoWithOperand<String>( el.OpCodeID, el.Operand )
            );

         // With settable operand: TableIndex
         yield return OpCodeElementTypeInfo.NewInfo<OpCodeInfoWithOperand<TableIndex>>(
            ( x, y ) => x.Operand == y.Operand,
            x => x.Operand.GetHashCode(),
            ( el, ctx, cb ) => new OpCodeInfoWithOperand<TableIndex>( el.OpCodeID, ctx.TableIndexTransformer?.Invoke( el.Operand ) ?? el.Operand )
            );

         // With readonly operand: List<Int32>
         yield return OpCodeElementTypeInfo.NewInfo<OpCodeInfoWithList<Int32>>(
            ( x, y ) => ListEqualityComparer<List<Int32>, Int32>.ListEquality( x.Operand, y.Operand ),
            x => ListEqualityComparer<List<Int32>, Int32>.ListHashCode( x.Operand ),
            ( el, ctx, cb ) =>
            {
               var list = el.Operand;
               var retVal = new OpCodeInfoWithList<Int32>( el.OpCodeID, list.Count );
               retVal.Operand.AddRange( list );
               return retVal;
            } );
      }


      /// <summary>
      /// Gets all of the op codes in <see cref="OpCodes"/> class.
      /// </summary>
      /// <returns>An enumerable to iterate all codes in <see cref="OpCodes"/> class.</returns>
      public static IEnumerable<OpCodeSerializationInfo> GetDefaultOpCodes()
      {
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Nop, 0x0000 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Break, 0x0001 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldarg_0, 0x0002 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldarg_1, 0x0003 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldarg_2, 0x0004 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldarg_3, 0x0005 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldloc_0, 0x0006 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldloc_1, 0x0007 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldloc_2, 0x0008 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldloc_3, 0x0009 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stloc_0, 0x000A );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stloc_1, 0x000B );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stloc_2, 0x000C );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stloc_3, 0x000D );
         yield return new OpCodeSerializationInfo_Byte( OpCodes.Ldarg_S, 0x000E );
         yield return new OpCodeSerializationInfo_Byte( OpCodes.Ldarga_S, 0x000F );
         yield return new OpCodeSerializationInfo_Byte( OpCodes.Starg_S, 0x0010 );
         yield return new OpCodeSerializationInfo_Byte( OpCodes.Ldloc_S, 0x0011 );
         yield return new OpCodeSerializationInfo_Byte( OpCodes.Ldloca_S, 0x0012 );
         yield return new OpCodeSerializationInfo_Byte( OpCodes.Stloc_S, 0x0013 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldnull, 0x0014 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldc_I4_M1, 0x0015 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldc_I4_0, 0x0016 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldc_I4_1, 0x0017 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldc_I4_2, 0x0018 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldc_I4_3, 0x0019 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldc_I4_4, 0x001A );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldc_I4_5, 0x001B );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldc_I4_6, 0x001C );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldc_I4_7, 0x001D );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldc_I4_8, 0x001E );
         yield return new OpCodeSerializationInfo_SByte( OpCodes.Ldc_I4_S, 0x001F );
         yield return new OpCodeSerializationInfo_Int32( OpCodes.Ldc_I4, 0x0020 );
         yield return new OpCodeSerializationInfo_Int64( OpCodes.Ldc_I8, 0x0021 );
         yield return new OpCodeSerializationInfo_Single( OpCodes.Ldc_R4, 0x0022 );
         yield return new OpCodeSerializationInfo_Double( OpCodes.Ldc_R8, 0x0023 );
         // 0x0024 is missing
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Dup, 0x0025 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Pop, 0x0026 );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Jmp, 0x0027 );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Call, 0x0028 );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Calli, 0x0029 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ret, 0x002A );
         yield return new OpCodeSerializationInfo_SByte( OpCodes.Br_S, 0x002B );
         yield return new OpCodeSerializationInfo_SByte( OpCodes.Brfalse_S, 0x002C );
         yield return new OpCodeSerializationInfo_SByte( OpCodes.Brtrue_S, 0x002D );
         yield return new OpCodeSerializationInfo_SByte( OpCodes.Beq_S, 0x002E );
         yield return new OpCodeSerializationInfo_SByte( OpCodes.Bge_S, 0x002F );
         yield return new OpCodeSerializationInfo_SByte( OpCodes.Bgt_S, 0x0030 );
         yield return new OpCodeSerializationInfo_SByte( OpCodes.Ble_S, 0x0031 );
         yield return new OpCodeSerializationInfo_SByte( OpCodes.Blt_S, 0x0032 );
         yield return new OpCodeSerializationInfo_SByte( OpCodes.Bne_Un_S, 0x0033 );
         yield return new OpCodeSerializationInfo_SByte( OpCodes.Bge_Un_S, 0x0034 );
         yield return new OpCodeSerializationInfo_SByte( OpCodes.Bgt_Un_S, 0x0035 );
         yield return new OpCodeSerializationInfo_SByte( OpCodes.Ble_Un_S, 0x0036 );
         yield return new OpCodeSerializationInfo_SByte( OpCodes.Blt_Un_S, 0x0037 );
         yield return new OpCodeSerializationInfo_Int32( OpCodes.Br, 0x0038 );
         yield return new OpCodeSerializationInfo_Int32( OpCodes.Brfalse, 0x0039 );
         yield return new OpCodeSerializationInfo_Int32( OpCodes.Brtrue, 0x003A );
         yield return new OpCodeSerializationInfo_Int32( OpCodes.Beq, 0x003B );
         yield return new OpCodeSerializationInfo_Int32( OpCodes.Bge, 0x003C );
         yield return new OpCodeSerializationInfo_Int32( OpCodes.Bgt, 0x003D );
         yield return new OpCodeSerializationInfo_Int32( OpCodes.Ble, 0x003E );
         yield return new OpCodeSerializationInfo_Int32( OpCodes.Blt, 0x003F );
         yield return new OpCodeSerializationInfo_Int32( OpCodes.Bne_Un, 0x0040 );
         yield return new OpCodeSerializationInfo_Int32( OpCodes.Bge_Un, 0x0041 );
         yield return new OpCodeSerializationInfo_Int32( OpCodes.Bgt_Un, 0x0042 );
         yield return new OpCodeSerializationInfo_Int32( OpCodes.Ble_Un, 0x0043 );
         yield return new OpCodeSerializationInfo_Int32( OpCodes.Blt_Un, 0x0044 );
         yield return new OpCodeSerializationInfo_Int32List( OpCodes.Switch, 0x0045 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldind_I1, 0x0046 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldind_U1, 0x0047 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldind_I2, 0x0048 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldind_U2, 0x0049 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldind_I4, 0x004A );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldind_U4, 0x004B );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldind_I8, 0x004C );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldind_I, 0x004D );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldind_R4, 0x004E );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldind_R8, 0x004F );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldind_Ref, 0x0050 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stind_Ref, 0x0051 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stind_I1, 0x0052 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stind_I2, 0x0053 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stind_I4, 0x0054 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stind_I8, 0x0055 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stind_R4, 0x0056 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stind_R8, 0x0057 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Add, 0x0058 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Sub, 0x0059 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Mul, 0x005A );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Div, 0x005B );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Div_Un, 0x005C );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Rem, 0x005D );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Rem_Un, 0x005E );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.And, 0x005F );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Or, 0x0060 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Xor, 0x0061 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Shl, 0x0062 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Shr, 0x0063 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Shr_Un, 0x0064 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Neg, 0x0065 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Not, 0x0066 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_I1, 0x0067 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_I2, 0x0068 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_I4, 0x0069 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_I8, 0x006A );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_R4, 0x006B );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_R8, 0x006C );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_U4, 0x006D );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_U8, 0x006E );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Callvirt, 0x006F );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Cpobj, 0x0070 );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Ldobj, 0x0071 );
         yield return new OpCodeSerializationInfo_String( OpCodes.Ldstr, 0x0072 );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Newobj, 0x0073 );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Castclass, 0x0074 );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Isinst, 0x0075 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_R_Un, 0x0076 );
         // 0x0077, and 0x0078 are missing
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Unbox, 0x0079 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Throw, 0x007A );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Ldfld, 0x007B );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Ldflda, 0x007C );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Stfld, 0x007D );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Ldsfld, 0x007E );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Ldsflda, 0x007F );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Stsfld, 0x0080 );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Stobj, 0x0081 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_I1_Un, 0x0082 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_I2_Un, 0x0083 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_I4_Un, 0x0084 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_I8_Un, 0x0085 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_U1_Un, 0x0086 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_U2_Un, 0x0087 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_U4_Un, 0x0088 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_U8_Un, 0x0089 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_I_Un, 0x008A );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_U_Un, 0x008B );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Box, 0x008C );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Newarr, 0x008D );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldlen, 0x008E );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Ldelema, 0x008F );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldelem_I1, 0x0090 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldelem_U1, 0x0091 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldelem_I2, 0x0092 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldelem_U2, 0x0093 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldelem_I4, 0x0094 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldelem_U4, 0x0095 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldelem_I8, 0x0096 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldelem_I, 0x0097 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldelem_R4, 0x0098 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldelem_R8, 0x0099 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ldelem_Ref, 0x009A );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stelem_I, 0x009B );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stelem_I1, 0x009C );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stelem_I2, 0x009D );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stelem_I4, 0x009E );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stelem_I8, 0x009F );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stelem_R4, 0x00A0 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stelem_R8, 0x00A1 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stelem_Ref, 0x00A2 );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Ldelem, 0x00A3 );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Stelem, 0x00A4 );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Unbox_Any, 0x00A5 );
         // 0x00A6-0x00B2 are missing
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_I1, 0x00B3 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_U1, 0x00B4 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_I2, 0x00B5 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_U2, 0x00B6 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_I4, 0x00B7 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_U4, 0x00B8 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_I8, 0x00B9 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_U8, 0x00BA );
         // 0x00BB-0x00C1 are missing
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Refanyval, 0x00C2 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ckfinite, 0x00C3 );
         // 0x00C4 and 0x00C5 are missing
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Mkrefany, 0x00C6 );
         // 0x00C7-0x00CF are missing
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Ldtoken, 0x00D0 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_U2, 0x00D1 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_U1, 0x00D2 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_I, 0x00D3 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_I, 0x00D4 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_Ovf_U, 0x00D5 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Add_Ovf, 0x00D6 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Add_Ovf_Un, 0x00D7 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Mul_Ovf, 0x00D8 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Mul_Ovf_Un, 0x00D9 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Sub_Ovf, 0x00DA );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Sub_Ovf_Un, 0x00DB );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Endfinally, 0x00DC );
         yield return new OpCodeSerializationInfo_Int32( OpCodes.Leave, 0x00DD );
         yield return new OpCodeSerializationInfo_SByte( OpCodes.Leave_S, 0x00DE );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Stind_I, 0x00DF );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Conv_U, 0x00E0 );
         // 0x00E1-0x00FD are missing
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Arglist, 0xFE00 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Ceq, 0xFE01 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Cgt, 0xFE02 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Cgt_Un, 0xFE03 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Clt, 0xFE04 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Clt_Un, 0xFE05 );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Ldftn, 0xFE06 );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Ldvirtftn, 0xFE07 );
         // 0xFE08 is missing
         yield return new OpCodeSerializationInfo_Int16( OpCodes.Ldarg, 0xFE09 );
         yield return new OpCodeSerializationInfo_Int16( OpCodes.Ldarga, 0xFE0A );
         yield return new OpCodeSerializationInfo_Int16( OpCodes.Starg, 0xFE0B );
         yield return new OpCodeSerializationInfo_Int16( OpCodes.Ldloc, 0xFE0C );
         yield return new OpCodeSerializationInfo_Int16( OpCodes.Ldloca, 0xFE0D );
         yield return new OpCodeSerializationInfo_Int16( OpCodes.Stloc, 0xFE0E );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Localloc, 0xFE0F );
         // 0xFE10 is missing
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Endfilter, 0xFE11 );
         yield return new OpCodeSerializationInfo_SByte( OpCodes.Unaligned_, 0xFE12 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Volatile_, 0xFE13 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Tail_, 0xFE14 );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Initobj, 0xFE15 );
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Constrained_, 0xFE16 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Cpblk, 0xFE17 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Initblk, 0xFE18 );
         yield return new OpCodeSerializationInfo_SByte( OpCodes.No_, 0xFE19 );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Rethrow, 0xFE1A );
         // 0xFE1B is missing
         yield return new OpCodeSerializationInfo_TableIndex( OpCodes.Sizeof, 0xFE1C );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Refanytype, 0xFE1D );
         yield return new OpCodeSerializationInfo_Operandless( OpCodes.Readonly_, 0xFE1E );
      }
   }
}

#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{
   ///// <summary>
   ///// Gets the code for given <see cref="OpCodeID"/>, or throws an exception if no code found.
   ///// </summary>
   ///// <param name="opCodeProvider">The <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeProvider"/>.</param>
   ///// <param name="codeID">The <see cref="OpCode"/></param>
   ///// <returns>The <see cref="OpCode"/> for given <paramref name="codeID"/></returns>
   ///// <exception cref="NullReferenceException">If this <paramref name="opCodeProvider"/> is <c>null</c>.</exception>
   ///// <exception cref="ArgumentException">If no suitable <see cref="OpCode"/> is found.</exception>
   ///// <seealso cref="OpCode"/>
   //public static OpCode GetCodeFor( this CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider opCodeProvider, OpCode codeID )
   //{
   //   return opCodeProvider.GetInfoFor( codeID ).Code;
   //}

   /// <summary>
   /// Gets the <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo"/> for given <see cref="OpCodeID"/>, or throws an exception if no info found.
   /// </summary>
   /// <param name="opCodeProvider">The <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeProvider"/>.</param>
   /// <param name="codeID">The <see cref="OpCode"/></param>
   /// <returns>The <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo"/> for given <paramref name="codeID"/></returns>
   /// <exception cref="NullReferenceException">If this <paramref name="opCodeProvider"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If no suitable <see cref="OpCode"/> is found.</exception>
   public static CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo GetInfoFor( this CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider opCodeProvider, OpCode codeID )
   {
      var info = ( (CILAssemblyManipulator.Physical.Meta.OpCodeProvider) opCodeProvider ).GetSerializationInfoOrNull( codeID );
      if ( info == null )
      {
         throw new ArgumentException( "Op code " + codeID + " is invalid or not supported by this op code provider." );
      }
      return info;
   }

   /// <summary>
   /// Calculates the total fixed byte count for a specific <see cref="OpCodeID"/>.
   /// This is the sum of <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo.CodeSize"/> and <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo.FixedOperandSize"/>.
   /// </summary>
   /// <param name="opCodeProvider">The <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeProvider"/>.</param>
   /// <param name="codeID">The <see cref="OpCode"/>.</param>
   /// <returns>The total fixed byte count for a specific <see cref="OpCode"/>.</returns>
   /// <exception cref="NullReferenceException">If </exception>
   /// <remarks>
   /// One should use <see cref="GetTotalByteCount(CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider, IOpCodeInfo)"/> extension method when calculating byte sizes when writing or reading IL bytecode.
   /// This is because switch instruction (<see cref="OpCodeID.Switch"/>) has additional offset array, the length of which is determined by the fixed operand of switch instruction.
   /// </remarks>
   public static Int32 GetFixedByteCount( this CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider opCodeProvider, OpCode codeID )
   {
      return opCodeProvider.GetInfoFor( codeID ).GetFixedByteCount();
   }

   /// <summary>
   /// Helper method to calculate the fixed byte count for specific <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo"/>
   /// </summary>
   /// <param name="info">The <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo"/>.</param>
   /// <returns>The sum of <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo.CodeSize"/> and <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo.FixedOperandSize"/>.</returns>
   public static Int32 GetFixedByteCount( this CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo info )
   {
      return info.CodeSize + info.FixedOperandSize;
   }

   /// <summary>
   /// Gets the total byte count that a single <see cref="OpCodeInfo"/> takes.
   /// </summary>
   /// <param name="opCodeProvider">The <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeProvider"/> to use.</param>
   /// <param name="info">The single <see cref="OpCodeInfo"/>.</param>
   /// <returns>The total byte count of a single <see cref="OpCodeInfo"/>.</returns>
   /// <remarks>
   /// The total byte count is the size of op code of <see cref="OpCodeInfo"/> added with <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo.GetDynamicSize"/>.
   /// </remarks>
   /// <exception cref="NullReferenceException">If <paramref name="opCodeProvider"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="info"/> is <c>null</c>.</exception>
   public static Int32 GetTotalByteCount( this CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider opCodeProvider, IOpCodeInfo info )
   {
      // Get NullReferenceException *before* ArgumentNullException
      return ArgumentValidator.ValidateNotNullReference( opCodeProvider ).GetInfoFor( info.OpCodeID ).GetTotalByteCount( info );// .GetFixedByteCount( info.OpCodeID ) + info.DynamicOperandByteSize;
   }

   /// <summary>
   /// Calculates the total byte count which will be taken by given <see cref="IOpCodeInfo"/> when it is written to byte stream.
   /// </summary>
   /// <param name="info">The <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo"/>.</param>
   /// <param name="instance">The <see cref="IOpCodeInfo"/>.</param>
   /// <returns>The total byte count which will be taken by given <see cref="IOpCodeInfo"/> when it is written to byte stream.</returns>
   /// <remarks>
   /// The return byte count includes count for op code, its operand, and any additional dynamic operand size.
   /// </remarks>
   /// <exception cref="NullReferenceException">If this <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="instance"/> is <c>null</c>.</exception>
   public static Int32 GetTotalByteCount( this CILAssemblyManipulator.Physical.Meta.OpCodeSerializationInfo info, IOpCodeInfo instance )
   {
      return info.GetFixedByteCount() + info.GetDynamicSize( instance );
   }

   /// <summary>
   /// Calculates the total byte count for IL of given op codes.
   /// </summary>
   /// <param name="ocp">The <see cref="CILAssemblyManipulator.Physical.Meta.OpCodeProvider" />.</param>
   /// <param name="opCodes">The op codes.</param>
   /// <returns>The total byte count for IL of given op codes.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="ocp"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="opCodes"/> is <c>null</c>.</exception>
   public static Int32 GetILByteCount( this CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider ocp, IEnumerable<OpCodeInfo> opCodes )
   {
      ArgumentValidator.ValidateNotNullReference( ocp );

      return ArgumentValidator.ValidateNotNull( "Op codes", opCodes ).Sum( oci => ocp.GetTotalByteCount( oci ) );
   }

   /// <summary>
   /// This method will emit a for-loop with given arguments.
   /// </summary>
   /// <param name="ocp">This <see cref="CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider"/>.</param>
   /// <param name="codes">The list of op codes where to emit the for loop.</param>
   /// <param name="initialization">The op codes constituting the initialization part of for-loop.</param>
   /// <param name="condition">The op codes constituting the condition part of for-loop, up until branch back to loop start.</param>
   /// <param name="conditionBranchShortForm">The short form of <see cref="OpCode"/> that will branch to loop start.</param>
   /// <param name="conditionBranchLongForm">The long form of <see cref="OpCode"/> that will branch to loop start.</param>
   /// <param name="increment">The op codes constituting the increment part of for-loop.</param>
   /// <param name="body">The op codes constituting the body of for-loop.</param>
   public static void EmitForLoop(
      this CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider ocp,
      List<OpCodeInfo> codes,
      IEnumerable<OpCodeInfo> initialization,
      IEnumerable<OpCodeInfo> condition,
      OpCode conditionBranchShortForm,
      OpCode conditionBranchLongForm,
      IEnumerable<OpCodeInfo> increment,
      IEnumerable<OpCodeInfo> body
      )
   {
      Int32 dummy1, dummy2;
      ocp.EmitForLoop( codes, initialization, condition, conditionBranchShortForm, conditionBranchLongForm, increment, body, out dummy1, out dummy2 );
   }

   /// <summary>
   /// This method will emit a for-loop with given arguments.
   /// </summary>
   /// <param name="ocp">This <see cref="CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider"/>.</param>
   /// <param name="codes">The list of op codes where to emit the for loop.</param>
   /// <param name="initialization">The op codes constituting the initialization part of for-loop.</param>
   /// <param name="condition">The op codes constituting the condition part of for-loop, up until branch back to loop start.</param>
   /// <param name="conditionBranchShortForm">The short form of <see cref="OpCode"/> that will branch to loop start.</param>
   /// <param name="conditionBranchLongForm">The long form of <see cref="OpCode"/> that will branch to loop start.</param>
   /// <param name="increment">The op codes constituting the increment part of for-loop.</param>
   /// <param name="body">The op codes constituting the body of for-loop.</param>
   /// <param name="beforeUnconditionalBranchByteCount">This parameter will hold the byte count of all op codes up until unconditional branch to for-loop condition check.</param>
   /// <param name="loopEndByteCount">This parameter will hold the byte count of all op codes in <paramref name="codes"/> after this method completes.</param>
   public static void EmitForLoop(
      this CAMPhysical::CILAssemblyManipulator.Physical.Meta.OpCodeProvider ocp,
      List<OpCodeInfo> codes,
      IEnumerable<OpCodeInfo> initialization,
      IEnumerable<OpCodeInfo> condition,
      OpCode conditionBranchShortForm,
      OpCode conditionBranchLongForm,
      IEnumerable<OpCodeInfo> increment,
      IEnumerable<OpCodeInfo> body,
      out Int32 beforeUnconditionalBranchByteCount,
      out Int32 loopEndByteCount
      )
   {
      // Typically this is "i = 0;"
      codes.AddRange( initialization );
      var afterInitializationByteCount = codes.Aggregate( 0, ( cur, oc ) => cur + ocp.GetTotalByteCount( oc ) );
      beforeUnconditionalBranchByteCount = afterInitializationByteCount;

      // This will be our jump
      var branchToConditionCheckIndex = codes.Count;
      codes.Add( null );

      // Now add op codes that are a body and increment of the loop
      var conditionStartByteCount = body.Concat( increment ?? Empty<OpCodeInfo>.Enumerable ).Aggregate( afterInitializationByteCount, ( cur, oc ) =>
      {
         codes.Add( oc );
         return cur + ocp.GetTotalByteCount( oc );
      } );

      var jumpToConditionAmount = conditionStartByteCount - afterInitializationByteCount;
      // Set the jump after initialization
      var branchToConditionCheck = new OpCodeInfoWithOperand<Int32>( jumpToConditionAmount <= SByte.MaxValue ? OpCodes.Br_S : OpCodes.Br, jumpToConditionAmount );
      codes[branchToConditionCheckIndex] = branchToConditionCheck;
      var branchToConditionCheckSize = ocp.GetTotalByteCount( branchToConditionCheck );
      afterInitializationByteCount += branchToConditionCheckSize;
      conditionStartByteCount += branchToConditionCheckSize;

      // Add code to leave condition on stack
      var curByteCount = condition.Aggregate( conditionStartByteCount, ( cur, oc ) =>
      {
         codes.Add( oc );
         return cur + ocp.GetTotalByteCount( oc );
      } );

      // Try short form first
      var branchToLoopStartSize = ocp.GetFixedByteCount( conditionBranchShortForm );
      var branchToLoopStartAmount = -( curByteCount + branchToLoopStartSize - afterInitializationByteCount );
      OpCode jumpToLoopStartCode;
      if ( conditionBranchLongForm == null || conditionBranchShortForm == conditionBranchLongForm || branchToLoopStartAmount >= SByte.MinValue )
      {
         // Short form is fine
         jumpToLoopStartCode = conditionBranchShortForm;
      }
      else
      {
         jumpToLoopStartCode = conditionBranchLongForm;
         var difference = ocp.GetFixedByteCount( conditionBranchLongForm ) - branchToLoopStartSize;
         branchToLoopStartAmount -= difference;
         branchToLoopStartSize += difference;
      }
      codes.Add( new OpCodeInfoWithOperand<Int32>( jumpToLoopStartCode, branchToLoopStartAmount ) );
      loopEndByteCount = curByteCount + branchToLoopStartSize;
   }
}