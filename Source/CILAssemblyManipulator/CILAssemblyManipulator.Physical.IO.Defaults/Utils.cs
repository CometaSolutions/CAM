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

using CAMPhysicalIO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO
{
   internal static class BitUtils_IOD
   {

      internal static String ReadLenPrefixedUTF8StringOrDefault( this Byte[] caBLOB, ref Int32 idx, Int32 max, String defaultString = null )
      {
         String str;
         return caBLOB.ReadLenPrefixedUTF8String( ref idx, max, out str ) ? str : defaultString;
      }

      internal static Boolean ReadLenPrefixedUTF8String( this Byte[] caBLOB, ref Int32 idx, out String str )
      {
         return caBLOB.ReadLenPrefixedUTF8String( ref idx, caBLOB.Length, out str );
      }

      internal static Boolean ReadLenPrefixedUTF8String( this Byte[] caBLOB, ref Int32 idx, Int32 max, out String str )
      {
         Int32 len;
         // DecompressUInt32 will return false for value '0xFF' when 'acceptErraneous' parameter is set to 'false'.
         var retVal = !caBLOB.TryDecompressUInt32( ref idx, max, out len, false ) || idx + len <= caBLOB.Length;
         if ( retVal )
         {
            if ( len >= 0 )
            {
               str = IO.Defaults.MetaDataConstants.SYS_STRING_ENCODING.GetString( caBLOB, idx, len );
               idx += len;
            }
            else
            {
               str = null;
               ++idx;
            }
         }
         else
         {
            str = null;
         }
         return retVal;
      }

      internal static UInt32 Sum( this IEnumerable<UInt32> enumerable )
      {
         var total = 0u;
         checked
         {
            foreach ( var item in enumerable )
            {
               total += item;
            }
         }
         return total;
      }
   }
}

#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{
   // TODO this method is only used by tests now, might remove maybe?
   /// <summary>
   /// Checks whether the <see cref="MethodDefinition"/> at given index has <see cref="MethodILDefinition"/> such that it would be serialized using tiny IL header.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="methodDefIndex">The index in <see cref="CILMetaData.MethodDefinitions"/> table.</param>
   /// <returns><c>true</c> if <see cref="MethodILDefinition"/> would be serialized using tiny IL header; <c>false</c> otherwise.</returns>
   public static Boolean IsTinyILHeader( this CILMetaData md, Int32 methodDefIndex )
   {
      Int32 ilCodeByteCount; Boolean hasAnyExceptions, allAreSmall;
      return md.IsTinyILHeader( methodDefIndex, out ilCodeByteCount, out hasAnyExceptions, out allAreSmall );
   }

   // TODO this method is not used at all, might remove maybe?
   /// <summary>
   /// Checks whether the <see cref="MethodDefinition"/> at given index has <see cref="MethodILDefinition"/> such that it would be serialized using tiny IL header.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="methodDefIndex">The index in <see cref="CILMetaData.MethodDefinitions"/> table.</param>
   /// <param name="ilCodeByteCount">This parameter will have the byte count for IL bytecode of the target <see cref="MethodILDefinition"/>.</param>
   /// <returns><c>true</c> if <see cref="MethodILDefinition"/> would be serialized using tiny IL header; <c>false</c> otherwise.</returns>
   public static Boolean IsTinyILHeader( this CILMetaData md, Int32 methodDefIndex, out Int32 ilCodeByteCount )
   {
      Boolean hasAnyExceptions, allAreSmall;
      return md.IsTinyILHeader( methodDefIndex, out ilCodeByteCount, out hasAnyExceptions, out allAreSmall );
   }

   internal static Boolean IsTinyILHeader( this CILMetaData md, Int32 methodDefIndex, out Int32 ilCodeByteCount, out Boolean hasAnyExceptions, out Boolean allAreSmall )
   {
      var il = md?.MethodDefinitions?.GetOrNull( methodDefIndex )?.IL;
      Boolean retVal;
      if ( il != null )
      {
         var ocp = md.OpCodeProvider;
         ilCodeByteCount = ocp.GetILByteCount( il.OpCodes );

         var lIdx = il.LocalsSignatureIndex;
         var localSig = lIdx.HasValue && lIdx.Value.Table == Tables.StandaloneSignature ?
            md.StandaloneSignatures.GetOrNull( lIdx.Value.Index )?.Signature as LocalVariablesSignature :
            null;


         // Then calculate the size of headers and other stuff
         var exceptionBlocks = il.ExceptionBlocks;
         // PEVerify doesn't like mixed small and fat blocks at all (however, at least Cecil understands that kind of situation)
         // Apparently, PEVerify doesn't like multiple small blocks either (Cecil still loads code fine)
         // So to use small exception blocks at all, all the blocks must be small, and there must be a limited amount of them
         allAreSmall = exceptionBlocks.Count <= CILAssemblyManipulator.Physical.IO.Defaults.SectionPartFunctionality_MethodIL.MAX_SMALL_EXC_HANDLERS_IN_ONE_SECTION
            && exceptionBlocks.All( excBlock =>
            {
               return excBlock.TryLength <= Byte.MaxValue
                  && excBlock.HandlerLength <= Byte.MaxValue
                  && excBlock.TryOffset <= UInt16.MaxValue
                  && excBlock.HandlerOffset <= UInt16.MaxValue;
            } );

         var maxStack = il.MaxStackSize;

         var excCount = exceptionBlocks.Count;
         hasAnyExceptions = excCount > 0;
         retVal = ilCodeByteCount < 64
            && !hasAnyExceptions
            && maxStack <= 8
            && ( localSig == null || localSig.Locals.Count == 0 );

      }
      else
      {
         ilCodeByteCount = 0;
         hasAnyExceptions = false;
         allAreSmall = false;
         retVal = false;
      }

      return retVal;
   }
}