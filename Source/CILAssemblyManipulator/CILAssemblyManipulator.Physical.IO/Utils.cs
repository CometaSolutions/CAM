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

namespace CILAssemblyManipulator.Physical.IO
{
   internal static class BitUtils
   {
      private enum EncodingForcePolicy : byte { DontForce, Force2Byte, Force4Byte }

      //private static readonly Byte[] ZeroArray = new Byte[0x10];

      //internal static String ReadLenPrefixedUTF8String( this StreamHelper caBLOB )
      //{
      //   Int32 len;
      //   // DecompressUInt32 will return false for value '0xFF' when 'acceptErraneous' parameter is set to 'false'.
      //   return caBLOB.DecompressUInt32( out len, false ) ?
      //      IO.Defaults.MetaDataConstants.SYS_STRING_ENCODING.GetString( caBLOB.ReadAndCreateArray( len ) ) :
      //      null;
      //}

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

      private const UInt32 UINT_FOUR_BYTES_MASK = 0xC0;
      private const UInt32 UINT_TWO_BYTES_MASK = 0x80;
      private const Int32 UINT_TWO_BYTES_DECODE_MASK = 0x3F;
      private const Int32 UINT_FOUR_BYTES_DECODE_MASK = 0x1F;

      private const Int32 UINT_ONE_BYTE_MAX = 0x7F;
      private const Int32 UINT_TWO_BYTES_MAX = 0x3FFF;
      private const Int32 UINT_FOUR_BYTES_MAX = 0x1FFFFFFF;

      private const Int32 INT_ONE_BYTE_MIN = -0x40;
      private const Int32 INT_ONE_BYTE_MAX = 0x3F;
      private const Int32 INT_TWO_BYTES_MIN = -0x2000;
      private const Int32 INT_TWO_BYTES_MAX = 0x1FFF;
      private const Int32 INT_FOUR_BYTES_MIN = -0x10000000;
      private const Int32 INT_FOUR_BYTES_MAX = 0xFFFFFFF;

      private const Int32 INT_ONE_BYTE_ENCODE_MASK = 0x40;
      private const Int32 INT_TWO_BYTES_ENCODE_MASK = 0x2000;
      private const Int32 INT_FOUR_BYTES_ENCODE_MASK = 0x10000000;

      private const UInt32 ONE = 1;
      private const Int32 COMPLEMENT_MASK_ONE_BYTE = unchecked((Int32) 0xFFFFFFC0);
      private const Int32 COMPLEMENT_MASK_TWO_BYTES = unchecked((Int32) 0xFFFFE000);
      private const Int32 COMPLEMENT_MASK_FOUR_BYTES = unchecked((Int32) 0xF0000000);

      internal static void CompressUInt32( this Byte[] array, ref Int32 offset, Int32 value )
      {
         EncodeUInt32( array, ref offset, value, EncodingForcePolicy.DontForce );
      }

      internal static void CompressInt32( this Byte[] array, ref Int32 offset, Int32 value )
      {
         Int32 uValue = value;

         EncodingForcePolicy policy;
         if ( value >= INT_ONE_BYTE_MIN && value <= INT_ONE_BYTE_MAX )
         {
            policy = EncodingForcePolicy.DontForce;
            if ( value < 0 )
            {
               // Represent the value as 7-bit 2-complement number
               uValue = ( INT_ONE_BYTE_ENCODE_MASK | ( uValue & INT_ONE_BYTE_MAX ) );
            }
            // Rotate value 1 bit left
            uValue = ( ( uValue << 1 ) | ( uValue >> 6 ) ) & UINT_ONE_BYTE_MAX;
         }
         else if ( value >= INT_TWO_BYTES_MIN && value <= INT_TWO_BYTES_MAX )
         {
            policy = EncodingForcePolicy.Force2Byte;
            if ( value < 0 )
            {
               // Represent the value as 14-bit 2-complement number
               uValue = ( INT_TWO_BYTES_ENCODE_MASK | ( uValue & INT_TWO_BYTES_MAX ) );
            }
            // Rotate value 1 bit left
            uValue = ( ( uValue << 1 ) | ( uValue >> 13 ) ) & UINT_TWO_BYTES_MAX;
         }
         else if ( value >= INT_FOUR_BYTES_MIN || value <= INT_FOUR_BYTES_MAX )
         {
            policy = EncodingForcePolicy.Force4Byte;
            if ( value < 0 )
            {
               // Represent the value as 28-bit 2-complement number
               uValue = ( INT_FOUR_BYTES_ENCODE_MASK | ( uValue & INT_FOUR_BYTES_MAX ) );
            }

            // Rotate value 1 bit left
            uValue = ( ( uValue << 1 ) | ( uValue >> 28 ) ) & UINT_FOUR_BYTES_MAX;
         }
         else
         {
            throw new ArgumentException( "Int32 value " + value + "was too big or too small to be encoded (should be between " + INT_FOUR_BYTES_MIN + " and " + INT_FOUR_BYTES_MAX + ")." );
         }
         EncodeUInt32( array, ref offset, uValue, policy );
      }

      private static void EncodeUInt32( Byte[] array, ref Int32 offset, Int32 value, EncodingForcePolicy forcePolicy )
      {
         if ( forcePolicy == EncodingForcePolicy.DontForce && value <= UINT_ONE_BYTE_MAX )
         {
            array[offset++] = (Byte) value;
         }
         else if ( forcePolicy != EncodingForcePolicy.Force4Byte && value <= UINT_TWO_BYTES_MAX )
         {
            array[offset++] = (Byte) ( ( value >> 8 ) | UINT_TWO_BYTES_MASK );
            array[offset++] = (Byte) ( value & Byte.MaxValue );
         }
         else if ( value <= UINT_FOUR_BYTES_MAX )
         {
            array[offset++] = (Byte) ( ( value >> 24 ) | UINT_FOUR_BYTES_MASK );
            array[offset++] = (Byte) ( value >> 16 );
            array[offset++] = (Byte) ( value >> 8 );
            array[offset++] = (Byte) ( value & Byte.MaxValue );
         }
         else
         {
            throw new ArgumentException( "UInt32 value " + value + " was too big to be encoded (max value is " + UINT_FOUR_BYTES_MAX + ")." );
         }
      }

      internal static Int32 GetEncodedIntSize( Int32 value )
      {
         if ( value >= INT_ONE_BYTE_MIN && value <= INT_ONE_BYTE_MAX )
         {
            return 1;
         }
         else if ( value >= INT_TWO_BYTES_MIN && value <= INT_TWO_BYTES_MAX )
         {
            return 2;
         }
         else if ( value >= INT_FOUR_BYTES_MIN || value <= INT_FOUR_BYTES_MAX )
         {
            return 4;
         }
         else
         {
            throw new ArgumentException( "Int32 value " + value + "was too big or too small to be encoded (should be between " + INT_FOUR_BYTES_MIN + " and " + INT_FOUR_BYTES_MAX + ")." );
         }
      }

      internal static Int32 GetEncodedUIntSize( Int32 value )
      {
         if ( value < 0 )
         {
            throw new ArgumentException( "Number " + value + " is not UInt32." );
         }

         if ( value <= UINT_ONE_BYTE_MAX )
         {
            return 1;
         }
         else if ( value <= UINT_TWO_BYTES_MAX )
         {
            return 2;
         }
         else if ( value <= UINT_FOUR_BYTES_MAX )
         {
            return 4;
         }
         else
         {
            throw new ArgumentException( "Int32 value " + value + "was too big or too small to be encoded (should be between " + INT_FOUR_BYTES_MIN + " and " + INT_FOUR_BYTES_MAX + ")." );
         }
      }

      private const Int32 TYPE_DEF = 0;
      private const Int32 TYPE_REF = 1;
      private const Int32 TYPE_SPEC = 2;
      private const Int32 TDRS_TABLE_EXTRACT_MASK = 0x3;
      private const Int32 TYPE_DEF_MASK = ( (Byte) Tables.TypeDef ) << 24; // 0x2000000;
      private const Int32 TYPE_REF_MASK = ( (Byte) Tables.TypeRef ) << 24; // 0x1000000;
      private const Int32 TYPE_SPEC_MASK = ( (Byte) Tables.TypeSpec ) << 24; // 0x1B000000;

      internal static Int32 DecodeTypeDefOrRefOrSpec( this Byte[] array, ref Int32 idx )
      {
         var token = array.DecompressUInt32( ref idx );
         switch ( token & TDRS_TABLE_EXTRACT_MASK )
         {
            case TYPE_DEF:
               token = TYPE_DEF_MASK | ( token >> 2 );
               break;
            case TYPE_REF:
               token = TYPE_REF_MASK | ( token >> 2 );
               break;
            case TYPE_SPEC:
               token = TYPE_SPEC_MASK | ( token >> 2 );
               break;
            default:
               throw new InvalidOperationException( "Invalid TDRS token: " + token );
         }

         return token;
      }

      internal static Int32 EncodeTypeDefOrRefOrSpec( this Int32 token )
      {
         Int32 encodedValue;
         switch ( unchecked((UInt32) token) >> 24 )
         {
            case (UInt32) Tables.TypeDef:
               encodedValue = ( ( CAMCoreInternals.INDEX_MASK & token ) << 2 ) | TYPE_DEF;
               break;
            case (UInt32) Tables.TypeRef:
               encodedValue = ( ( CAMCoreInternals.INDEX_MASK & token ) << 2 ) | TYPE_REF;
               break;
            case (UInt32) Tables.TypeSpec:
               encodedValue = ( ( CAMCoreInternals.INDEX_MASK & token ) << 2 ) | TYPE_SPEC;
               break;
            default:
               throw new ArgumentException( "Token must reference one of the following tables: " + String.Join( ", ", Tables.TypeDef, Tables.TypeRef, Tables.TypeSpec ) + "." );
         }
         return encodedValue;
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

#if CAM_PHYSICAL_IS_PORTABLE
      // For some reason, this method is missing from PCL
      internal static Int32 FindIndex<T>( this IList<T> list, Predicate<T> match )
      {
         var max = list.Count;
         for ( var i = 0; i < max; ++i )
         {
            if ( match( list[i] ) )
            {
               return i;
            }
         }
         return -1;
      }
#endif



      //internal static IEnumerable<Int32> GetReferencingRowsFromOrdered<T>( this IList<T> array, Tables targetTable, Int32 targetIndex, Func<T, TableIndex> fullIndexExtractor )
      //{
      //   return array.GetReferencingRowsFromOrderedWithIndex( targetTable, targetIndex, idx => fullIndexExtractor( array[idx] ) );
      //}

      //internal static IEnumerable<Int32> GetReferencingRowsFromOrderedWithIndex<T>( this IList<T> array, Tables targetTable, Int32 targetIndex, Func<Int32, TableIndex> fullIndexExtractor )
      //{
      //   // Use binary search to find first one
      //   // Use the deferred equality detection version in order to find the smallest index matching the target index
      //   var max = array.Count - 1;
      //   var min = 0;
      //   while ( min < max )
      //   {
      //      var mid = ( min + max ) >> 1; // We can safely add before shifting, since table indices are supposed to be max 3 bytes long anyway.
      //      if ( fullIndexExtractor( mid ).Index < targetIndex )
      //      {
      //         min = mid + 1;
      //      }
      //      else
      //      {
      //         max = mid;
      //      }
      //   }

      //   // By calling explicit ReturnWhile method, calculating index using binary search will not be done when re-enumerating the enumerable.
      //   return min == max && fullIndexExtractor( min ).Index == targetIndex ?
      //      ReturnWhile( array, targetTable, targetIndex, fullIndexExtractor, min ) :
      //      Empty<Int32>.Enumerable;
      //}

      //private static IEnumerable<Int32> ReturnWhile<T>( IList<T> array, Tables targetTable, Int32 targetIndex, Func<Int32, TableIndex> fullIndexExtractor, Int32 idx )
      //{
      //   do
      //   {
      //      if ( fullIndexExtractor( idx ).Table == targetTable )
      //      {
      //         yield return idx;
      //      }
      //      ++idx;
      //   } while ( idx < array.Count && fullIndexExtractor( idx ).Index == targetIndex );
      //}

      //internal static String CreateTypeString( this TypeSignature type, CILMetaData moduleBeingEmitted, Boolean appendGArgs )
      //{

      //   String typeString;
      //   if ( type == null )
      //   {
      //      typeString = null;
      //   }
      //   else
      //   {
      //      // TODO probably should forbid whitespace characters to be within type and assembly names?
      //      var builder = new StringBuilder();
      //      CreateTypeString( type, moduleBeingEmitted, builder, appendGArgs );
      //      typeString = builder.ToString();
      //   }
      //   return typeString;
      //}

      //private static void CreateTypeString( TypeSignature type, CILMetaData md, StringBuilder builder, Boolean appendGArgs )
      //{
      //   var declTypeInfo = new Lazy<IDictionary<Int32, Int32>>( () =>
      //   {
      //      var dic = new Dictionary<Int32, Int32>();
      //      foreach ( var nc in md.NestedClassDefinitions )
      //      {
      //         dic[nc.NestedClass.Index] = nc.EnclosingClass.Index;
      //      }
      //      return dic;
      //   }, System.Threading.LazyThreadSafetyMode.None );

      //   var otherAssemblyRef = CreateTypeStringCore( type, md, builder, appendGArgs, declTypeInfo );
      //   if ( otherAssemblyRef != null )
      //   {
      //      builder
      //         .Insert( 0, "[" )
      //         .Append( ']' )
      //         .Append( TYPE_ASSEMBLY_SEPARATOR )
      //         .Append( otherAssemblyRef.ToStringForTypeName() ); // Assembly name will be escaped.

      //   }
      //}

      //private static AssemblyReference CreateTypeStringCore( TypeSignature type, CILMetaData md, StringBuilder builder, Boolean appendGArgs, Lazy<IDictionary<Int32, Int32>> declTypeInfo )
      //{
      //   AssemblyReference retVal = null;
      //   switch ( type.TypeSignatureKind )
      //   {
      //      case TypeSignatureKind.ComplexArray:
      //         var cArray = (ComplexArrayTypeSignature) type;
      //         retVal = CreateTypeStringCore( cArray.ArrayType, md, builder, appendGArgs, declTypeInfo );
      //         CreateArrayString( cArray, builder );
      //         break;
      //      case TypeSignatureKind.SimpleArray:
      //         retVal = CreateTypeStringCore( ( (AbstractArrayTypeSignature) type ).ArrayType, md, builder, appendGArgs, declTypeInfo );
      //         CreateArrayString( null, builder );
      //         break;
      //      case TypeSignatureKind.Pointer:
      //         retVal = CreateTypeStringCore( ( (PointerTypeSignature) type ).PointerType, md, builder, appendGArgs, declTypeInfo );
      //         builder.Append( '*' );
      //         break;
      //      case TypeSignatureKind.ClassOrValue:
      //         retVal = CreateTypeStringFromTableIndex( md, ( (ClassOrValueTypeSignature) type ).Type, builder, appendGArgs, declTypeInfo );
      //         break;
      //      case TypeSignatureKind.GenericParameter:
      //         throw new NotImplementedException();
      //      case TypeSignatureKind.FunctionPointer:
      //         throw new NotImplementedException();
      //   }

      //   return retVal;
      //}

      //private static AssemblyReference CreateTypeStringFromTableIndex( this CILMetaData md, TableIndex tIndex, StringBuilder builder, Boolean appendGArgs, Lazy<IDictionary<Int32, Int32>> declTypeInfo )
      //{
      //   var index = tIndex.Index;
      //   AssemblyReference retVal = null;
      //   switch ( tIndex.Table )
      //   {
      //      case Tables.TypeDef:
      //         if ( index < md.TypeDefinitions.Count )
      //         {
      //            var tDef = md.TypeDefinitions[index];
      //            Int32 declTypeRow;
      //            if ( declTypeInfo.Value.TryGetValue( index, out declTypeRow ) )
      //            {
      //               if ( declTypeRow < md.NestedClassDefinitions.Count )
      //               {
      //                  CreateTypeStringFromTableIndex( md, md.NestedClassDefinitions[declTypeRow].EnclosingClass, builder, false, declTypeInfo );
      //                  builder.Append( '+' );
      //               }
      //            }
      //            else
      //            {
      //               var ns = tDef.Namespace;
      //               if ( !String.IsNullOrEmpty( ns ) )
      //               {
      //                  builder.Append( EscapeSomeString( ns ) ).Append( '.' );
      //               }
      //            }
      //            builder.Append( EscapeSomeString( tDef.Name ) );
      //         }
      //         break;
      //      case Tables.TypeRef:
      //         if ( index < md.TypeReferences.Count )
      //         {
      //            var tRef = md.TypeReferences[index];
      //            var resScope = tRef.ResolutionScope;
      //            if ( resScope.HasValue && resScope.Value.Table == Tables.TypeRef )
      //            {
      //               CreateTypeStringFromTableIndex( md, resScope.Value, builder, false, declTypeInfo );
      //               builder.Append( '+' );
      //            }
      //            else
      //            {
      //               var ns = tRef.Namespace;
      //               if ( !String.IsNullOrEmpty( ns ) )
      //               {
      //                  builder.Append( EscapeSomeString( ns ) ).Append( '.' );
      //               }
      //            }
      //            builder.Append( EscapeSomeString( tRef.Name ) );
      //            if ( resScope.HasValue && resScope.Value.Table == Tables.AssemblyRef && resScope.Value.Index < md.AssemblyReferences.Count )
      //            {
      //               retVal = md.AssemblyReferences[resScope.Value.Index];
      //            }
      //         }
      //         break;
      //      case Tables.TypeSpec:
      //         if ( index < md.TypeSpecifications.Count )
      //         {
      //            var tSig = md.TypeSpecifications[index].Signature as ClassOrValueTypeSignature;
      //            if ( tSig != null )
      //            {
      //               retVal = CreateTypeStringFromTableIndex( md, tSig.Type, builder, false, declTypeInfo );

      //               if ( appendGArgs )
      //               {
      //                  var gArgs = tSig.GenericArguments;
      //                  builder.Append( '[' );
      //                  for ( var i = 0; i < gArgs.Count; ++i )
      //                  {
      //                     builder.Append( '[' );
      //                     CreateTypeString( gArgs[i], md, true );
      //                     builder.Append( ']' );
      //                     if ( i < gArgs.Count - 1 )
      //                     {
      //                        builder.Append( ',' );
      //                     }
      //                  }
      //                  builder.Append( ']' );
      //               }
      //            }
      //         }
      //         break;
      //   }

      //   return retVal;
      //}

      //private static String GetFullNameFromTypeName( this String typeName, String ns )
      //{
      //   return String.IsNullOrEmpty( ns ) ? typeName : ( ns + "." + typeName );
      //}

      //private static void CreateArrayString( ComplexArrayTypeSignature complexArray, StringBuilder builder )
      //{
      //   builder.Append( '[' );
      //   if ( complexArray != null )
      //   {
      //      if ( complexArray.Rank == 1 && complexArray.Sizes.Count == 0 && complexArray.LowerBounds.Count == 0 )
      //      {
      //         // Special case
      //         builder.Append( '*' );
      //      }
      //      else
      //      {
      //         for ( var i = 0; i < complexArray.Rank; ++i )
      //         {
      //            var appendLoBound = i < complexArray.LowerBounds.Count;
      //            if ( appendLoBound )
      //            {
      //               var loBound = complexArray.LowerBounds[i];
      //               appendLoBound = loBound != 0;
      //               if ( appendLoBound )
      //               {
      //                  builder.Append( loBound ).Append( ".." );
      //               }
      //            }
      //            if ( i < complexArray.Sizes.Count )
      //            {
      //               builder.Append( complexArray.Sizes[i] );
      //            }
      //            else if ( appendLoBound )
      //            {
      //               builder.Append( '.' );
      //            }

      //            if ( i < complexArray.Rank - 1 )
      //            {
      //               builder.Append( ',' );
      //            }
      //         }
      //      }
      //   }
      //   builder.Append( ']' );

      //}

      //private static String ToStringForTypeName( this AssemblyReference aRef )
      //{
      //   // mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
      //   var aInfo = aRef.AssemblyInformation;
      //   return EscapeSomeString( aInfo.Name ) +
      //      Consts.ASSEMBLY_NAME_ELEMENTS_SEPARATOR + ' ' + Consts.VERSION + Consts.ASSEMBLY_NAME_ELEMENT_VALUE_SEPARATOR + aInfo.VersionMajor + Consts.VERSION_SEPARATOR + aInfo.VersionMinor + Consts.VERSION_SEPARATOR + aInfo.VersionBuild + Consts.VERSION_SEPARATOR + aInfo.VersionRevision +
      //      Consts.ASSEMBLY_NAME_ELEMENTS_SEPARATOR + ' ' + Consts.CULTURE + Consts.ASSEMBLY_NAME_ELEMENT_VALUE_SEPARATOR + ( aInfo.Culture == null || aInfo.Culture.Trim().Length == 0 ? Consts.NEUTRAL_CULTURE : aInfo.Culture ) +
      //      ( aInfo.PublicKeyOrToken.IsNullOrEmpty() ? "" :
      //      ( Consts.ASSEMBLY_NAME_ELEMENTS_SEPARATOR + ' ' + ( ( (AssemblyFlags) aRef.Attributes ).IsFullPublicKey() ? Consts.PUBLIC_KEY : Consts.PUBLIC_KEY_TOKEN ) + Consts.ASSEMBLY_NAME_ELEMENT_VALUE_SEPARATOR + StringConversions.ByteArray2HexStr( aInfo.PublicKeyOrToken, 0, aInfo.PublicKeyOrToken.Length, false ) )
      //         );
      //}
   }
}
