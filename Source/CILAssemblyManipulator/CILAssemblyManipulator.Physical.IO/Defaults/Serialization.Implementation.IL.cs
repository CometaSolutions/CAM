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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO.Defaults
{
   public static class ILSerialization
   {
      public static OpCodeInfo TryReadOpCode(
        Byte[] bytes,
        ref Int32 idx,
        Func<Int32, String> stringGetter
        )
      {
         OpCodeInfo info;
         var b = bytes[idx++];
         var encoding = (OpCodeEncoding) ( b == OpCode.MAX_ONE_BYTE_INSTRUCTION ?
            ( ( b << 8 ) | bytes[idx++] ) :
            b );

         OpCode code;
         if ( OpCodes.TryGetCodeFor( encoding, out code ) )
         {
            switch ( code.OperandType )
            {
               case OperandType.InlineNone:
                  info = OpCodeInfoWithNoOperand.GetInstanceFor( encoding );
                  break;
               case OperandType.ShortInlineBrTarget:
               case OperandType.ShortInlineI:
                  info = new OpCodeInfoWithInt32( code, (Int32) ( bytes.ReadSByteFromBytes( ref idx ) ) );
                  break;
               case OperandType.ShortInlineVar:
                  info = new OpCodeInfoWithInt32( code, bytes.ReadByteFromBytes( ref idx ) );
                  break;
               case OperandType.ShortInlineR:
                  info = new OpCodeInfoWithSingle( code, bytes.ReadSingleLEFromBytes( ref idx ) );
                  break;
               case OperandType.InlineBrTarget:
               case OperandType.InlineI:
                  info = new OpCodeInfoWithInt32( code, bytes.ReadInt32LEFromBytes( ref idx ) );
                  break;
               case OperandType.InlineVar:
                  info = new OpCodeInfoWithInt32( code, bytes.ReadUInt16LEFromBytes( ref idx ) );
                  break;
               case OperandType.InlineR:
                  info = new OpCodeInfoWithDouble( code, bytes.ReadDoubleLEFromBytes( ref idx ) );
                  break;
               case OperandType.InlineI8:
                  info = new OpCodeInfoWithInt64( code, bytes.ReadInt64LEFromBytes( ref idx ) );
                  break;
               case OperandType.InlineString:
                  info = new OpCodeInfoWithString( code, stringGetter( bytes.ReadInt32LEFromBytes( ref idx ) ) );
                  break;
               case OperandType.InlineField:
               case OperandType.InlineMethod:
               case OperandType.InlineType:
               case OperandType.InlineTok:
               case OperandType.InlineSig:
                  info = new OpCodeInfoWithToken( code, TableIndex.FromOneBasedToken( bytes.ReadInt32LEFromBytes( ref idx ) ) );
                  break;
               case OperandType.InlineSwitch:
                  var count = bytes.ReadInt32LEFromBytes( ref idx );
                  var sInfo = new OpCodeInfoWithSwitch( code, count );
                  for ( var i = 0; i < count; ++i )
                  {
                     sInfo.Offsets.Add( bytes.ReadInt32LEFromBytes( ref idx ) );
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
   }
}
