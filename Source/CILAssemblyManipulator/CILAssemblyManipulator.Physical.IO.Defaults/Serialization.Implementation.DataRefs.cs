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
extern alias CAMPhysicalIO;

using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;
using CAMPhysical::CILAssemblyManipulator.Physical.Meta;

using CAMPhysicalIO;
using CAMPhysicalIO::CILAssemblyManipulator.Physical.IO;

using CILAssemblyManipulator.Physical.IO;
using CollectionsWithRoles.API;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{
   /// <summary>
   /// Using this <see cref="OpCodeProvider"/>, deserializes <see cref="MethodILDefinition"/> from a given stream.
   /// </summary>
   /// <param name="opCodeProvider">This <see cref="OpCodeProvider"/>.</param>
   /// <param name="stream">The stream to deserialzie <see cref="MethodILDefinition"/>, as <see cref="StreamHelper"/>.</param>
   /// <param name="array">The auxiliary array to use when deserializing.</param>
   /// <param name="userStrings">The <see cref="ReaderStringStreamHandler"/> containing strings that may be reference by op codes.</param>
   /// <returns>A new instance of <see cref="MethodILDefinition"/> with its data deserialized from given <paramref name="stream"/>.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="OpCodeProvider"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If any of the <paramref name="stream"/>, <paramref name="array"/>, or <paramref name="userStrings"/> is <c>null</c>.</exception>
   /// <exception cref="EndOfStreamException">If stream ends unexpectedly.</exception>
   public static MethodILDefinition DeserializeIL(
      this OpCodeProvider opCodeProvider,
      StreamHelper stream,
      ResizableArray<Byte> array,
      ReaderStringStreamHandler userStrings
      )
   {
      const Int32 FORMAT_MASK = 0x00000001;
      const Int32 FLAG_MASK = 0x00000FFF;
      const Int32 SEC_SIZE_MASK = unchecked((Int32) 0xFFFFFF00);
      const Int32 SEC_FLAG_MASK = 0x000000FF;

      ArgumentValidator.ValidateNotNullReference( opCodeProvider );
      ArgumentValidator.ValidateNotNull( "Stream", stream );
      ArgumentValidator.ValidateNotNull( "Array", array );

      var retVal = new MethodILDefinition();

      Byte b;
      Int32 codeSize;
      if ( stream.TryReadByteFromBytes( out b ) )
      {
         Byte b2;
         if ( ( FORMAT_MASK & b ) == 0 )
         {
            // Tiny header - no locals, no exceptions, no extra data
            if ( CreateOpCodes( opCodeProvider, retVal, stream.Stream, array, b >> 2, userStrings ) )
            {
               // Max stack is 8
               retVal.MaxStackSize = 8;
               retVal.InitLocals = false;
            }
            else
            {
               retVal = null;
            }
         }
         else if ( stream.TryReadByteFromBytes( out b2 ) )
         {
            var starter = ( b2 << 8 ) | b;
            var flags = (MethodHeaderFlags) ( starter & FLAG_MASK );
            retVal.InitLocals = ( flags & MethodHeaderFlags.InitLocals ) != 0;
            var headerSize = ( starter >> 12 ) * 4; // Header size is written as amount of integers
                                                    // Read max stack
            var bytes = array.ReadIntoResizableArray( stream.Stream, headerSize - 2 );
            var idx = 0;
            retVal.MaxStackSize = bytes.ReadUInt16LEFromBytes( ref idx );
            codeSize = bytes.ReadInt32LEFromBytes( ref idx );
            retVal.LocalsSignatureIndex = TableIndex.FromOneBasedTokenNullable( bytes.ReadInt32LEFromBytes( ref idx ) );

            // Read code
            if ( CreateOpCodes( opCodeProvider, retVal, stream.Stream, array, codeSize, userStrings )
               && ( flags & MethodHeaderFlags.MoreSections ) != 0 )
            {

               stream.SkipToNextAlignmentInt32();
               // Read sections
               MethodDataFlags secFlags;
               do
               {
                  var secHeader = stream.ReadInt32LEFromBytes();
                  secFlags = (MethodDataFlags) ( secHeader & SEC_FLAG_MASK );
                  var secByteSize = ( secHeader & SEC_SIZE_MASK ) >> 8;
                  secByteSize -= 4;
                  var isFat = ( secFlags & MethodDataFlags.FatFormat ) != 0;
                  bytes = array.ReadIntoResizableArray( stream.Stream, secByteSize );
                  idx = 0;
                  while ( secByteSize > 0 )
                  {
                     var eType = (ExceptionBlockType) ( isFat ? bytes.ReadInt32LEFromBytes( ref idx ) : bytes.ReadUInt16LEFromBytes( ref idx ) );
                     retVal.ExceptionBlocks.Add( new MethodExceptionBlock()
                     {
                        BlockType = eType,
                        TryOffset = isFat ? bytes.ReadInt32LEFromBytes( ref idx ) : bytes.ReadUInt16LEFromBytes( ref idx ),
                        TryLength = isFat ? bytes.ReadInt32LEFromBytes( ref idx ) : bytes.ReadByteFromBytes( ref idx ),
                        HandlerOffset = isFat ? bytes.ReadInt32LEFromBytes( ref idx ) : bytes.ReadUInt16LEFromBytes( ref idx ),
                        HandlerLength = isFat ? bytes.ReadInt32LEFromBytes( ref idx ) : bytes.ReadByteFromBytes( ref idx ),
                        ExceptionType = eType == ExceptionBlockType.Filter ? (TableIndex?) null : TableIndex.FromOneBasedTokenNullable( bytes.ReadInt32LEFromBytes( ref idx ) ),
                        FilterOffset = eType == ExceptionBlockType.Filter ? bytes.ReadInt32LEFromBytes( ref idx ) : 0
                     } );
                     secByteSize -= ( isFat ? 24 : 12 );
                  }
               } while ( ( secFlags & MethodDataFlags.MoreSections ) != 0 );

            }
         }
         else
         {
            retVal = null;
         }
      }

      return retVal;
   }

   private static Boolean CreateOpCodes(
      this OpCodeProvider opCodeProvider,
      MethodILDefinition methodIL,
      Stream stream,
      ResizableArray<Byte> array,
      Int32 codeSize,
      ReaderStringStreamHandler userStrings
      )
   {

      var success = codeSize >= 0;
      if ( codeSize > 0 )
      {
         var opCodes = methodIL.OpCodes;
         var idx = 0;
         var bytes = array.ReadIntoResizableArray( stream, codeSize );
         while ( idx < codeSize && success )
         {
            var curCodeInfo = opCodeProvider.TryReadOpCode(
               bytes,
               ref idx,
               strToken => userStrings.GetString( TableIndex.FromZeroBasedToken( strToken ).Index )
               );

            if ( curCodeInfo == null )
            {
               success = false;
            }
            else
            {
               opCodes.Add( curCodeInfo );
            }
         }
      }

      return success;
   }


   /// <summary>
   /// Using this <see cref="OpCodeProvider"/>, tries to read one <see cref="OpCodeInfo"/> from given byte array.
   /// </summary>
   /// <param name="opCodeProvider">This <see cref="OpCodeProvider"/>.</param>
   /// <param name="bytes">The byte array.</param>
   /// <param name="idx">The index in <paramref name="bytes"/> where to start reading.</param>
   /// <param name="stringGetter">The callback to get a string for <see cref="OpCodeInfoWithString"/>.</param>
   /// <returns>Deserialized <see cref="OpCodeInfo"/>, or <c>null</c> if deserialization failed.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="OpCodeProvider"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If any of the <paramref name="bytes"/> or <paramref name="stringGetter"/> is <c>null</c>.</exception>
   public static OpCodeInfo TryReadOpCode(
     this OpCodeProvider opCodeProvider,
     Byte[] bytes,
     ref Int32 idx,
     Func<Int32, String> stringGetter
     )
   {
      ArgumentValidator.ValidateNotNullReference( opCodeProvider );
      ArgumentValidator.ValidateNotNull( "Byte array", bytes );

      CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.OpCodeProviderInfo ocpInfo;
      OpCodeInfo info;
      if ( ( (CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.OpCodeProvider) opCodeProvider ).TryReadOpCode( bytes, idx, out ocpInfo ) )
      {
         idx += ocpInfo.Size;
         var codeID = ocpInfo.Code.OpCodeID;
         switch ( ocpInfo.Code.OperandType )
         {
            case OperandType.InlineNone:
               info = opCodeProvider.GetOperandlessInfoFor( codeID );
               break;
            case OperandType.ShortInlineBrTarget:
            case OperandType.ShortInlineI:
               info = new OpCodeInfoWithInt32( codeID, (Int32) ( bytes.ReadSByteFromBytes( ref idx ) ) );
               break;
            case OperandType.ShortInlineVar:
               info = new OpCodeInfoWithInt32( codeID, bytes.ReadByteFromBytes( ref idx ) );
               break;
            case OperandType.ShortInlineR:
               info = new OpCodeInfoWithSingle( codeID, bytes.ReadSingleLEFromBytes( ref idx ) );
               break;
            case OperandType.InlineBrTarget:
            case OperandType.InlineI:
               info = new OpCodeInfoWithInt32( codeID, bytes.ReadInt32LEFromBytes( ref idx ) );
               break;
            case OperandType.InlineVar:
               info = new OpCodeInfoWithInt32( codeID, bytes.ReadUInt16LEFromBytes( ref idx ) );
               break;
            case OperandType.InlineR:
               info = new OpCodeInfoWithDouble( codeID, bytes.ReadDoubleLEFromBytes( ref idx ) );
               break;
            case OperandType.InlineI8:
               info = new OpCodeInfoWithInt64( codeID, bytes.ReadInt64LEFromBytes( ref idx ) );
               break;
            case OperandType.InlineString:
               ArgumentValidator.ValidateNotNull( "String getter", stringGetter );
               info = new OpCodeInfoWithString( codeID, stringGetter( bytes.ReadInt32LEFromBytes( ref idx ) ) );
               break;
            case OperandType.InlineField:
            case OperandType.InlineMethod:
            case OperandType.InlineType:
            case OperandType.InlineToken:
            case OperandType.InlineSignature:
               info = new OpCodeInfoWithTableIndex( codeID, TableIndex.FromOneBasedToken( bytes.ReadInt32LEFromBytes( ref idx ) ) );
               break;
            case OperandType.InlineSwitch:
               var count = bytes.ReadInt32LEFromBytes( ref idx );
               var sInfo = new OpCodeInfoWithIntegers( codeID, count );
               for ( var i = 0; i < count; ++i )
               {
                  sInfo.Operand.Add( bytes.ReadInt32LEFromBytes( ref idx ) );
               }
               info = sInfo;
               break;
            default:
               info = null;
               break;
         }
      }
      else
      {
         info = null;
      }

      return info;

   }

   /// <summary>
   /// This helper method tries to calculate the size of the type of the given field, in bytes.
   /// </summary>
   /// <param name="md">This <see cref="CILMetaData"/>.</param>
   /// <param name="layoutInfo">The helper dictionary, containing <see cref="ClassLayout"/> for given <see cref="Tables.TypeDef"/> indices.</param>
   /// <param name="fieldIdx">The index in <see cref="CILMetaData.FieldDefinitions"/> table where to search for field.</param>
   /// <param name="size">This parameter will contain the calculated byte count for field type, if this method is successful.</param>
   /// <returns><c>true</c> if the size calculation succeeded; <c>false</c> otherwise.</returns>
   public static Boolean TryCalculateFieldTypeSize(
      this CILMetaData md,
      DictionaryQuery<Int32, ClassLayout> layoutInfo,
      Int32 fieldIdx,
      out Int32 size
      )
   {
      return md.TryCalculateFieldTypeSize( layoutInfo, fieldIdx, out size, false );
   }

   private static Boolean TryCalculateFieldTypeSize(
      this CILMetaData md,
      DictionaryQuery<Int32, ClassLayout> layoutInfo,
      Int32 fieldIdx,
      out Int32 size,
      Boolean onlySimpleTypeValid
      )
   {
      var fDef = md.FieldDefinitions.TableContents;
      size = 0;
      if ( fieldIdx < fDef.Count )
      {
         var type = fDef[fieldIdx]?.Signature?.Type;
         if ( type != null )
         {
            switch ( type.TypeSignatureKind )
            {
               case TypeSignatureKind.Simple:
                  switch ( ( (SimpleTypeSignature) type ).SimpleType )
                  {
                     case SimpleTypeSignatureKind.Boolean:
                        size = sizeof( Boolean ); // TODO is this actually 1 or 4?
                        break;
                     case SimpleTypeSignatureKind.I1:
                     case SimpleTypeSignatureKind.U1:
                        size = 1;
                        break;
                     case SimpleTypeSignatureKind.I2:
                     case SimpleTypeSignatureKind.U2:
                     case SimpleTypeSignatureKind.Char:
                        size = 2;
                        break;
                     case SimpleTypeSignatureKind.I4:
                     case SimpleTypeSignatureKind.U4:
                     case SimpleTypeSignatureKind.R4:
                        size = 4;
                        break;
                     case SimpleTypeSignatureKind.I8:
                     case SimpleTypeSignatureKind.U8:
                     case SimpleTypeSignatureKind.R8:
                        size = 8;
                        break;
                  }
                  break;
               case TypeSignatureKind.ClassOrValue:
                  if ( !onlySimpleTypeValid )
                  {
                     var c = (ClassOrValueTypeSignature) type;

                     var typeIdx = c.Type;
                     if ( typeIdx.Table == Tables.TypeDef )
                     {
                        // Only possible for types defined in this module
                        Int32 enumValueFieldIndex;
                        if ( md.TryGetEnumValueFieldIndex( typeIdx.Index, out enumValueFieldIndex ) )
                        {
                           md.TryCalculateFieldTypeSize( layoutInfo, enumValueFieldIndex, out size, true ); // Last parameter true to prevent possible infinite recursion in case of malformed metadata
                        }
                        else
                        {
                           ClassLayout layout;
                           if ( layoutInfo.TryGetValue( typeIdx.Index, out layout ) )
                           {
                              size = layout.ClassSize;
                           }
                        }

                     }
                  }
                  break;
               case TypeSignatureKind.Pointer:
               case TypeSignatureKind.FunctionPointer:
                  size = 4; // I am not 100% sure of this.
                  break;
            }
         }
      }
      return size != 0;
   }
}