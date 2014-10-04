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

namespace CILAssemblyManipulator.Implementation.Physical
{
   internal static class TokenUtils
   {
      internal const Int32 INDEX_MASK = 0x00FFFFF;

      private const Int32 TYPE_DEF = 0;
      private const Int32 TYPE_REF = 1;
      private const Int32 TYPE_SPEC = 2;
      private const Int32 TABLE_EXTRACTOR_MASK = 0x3;
      private const Int32 TYPE_DEF_MASK = ( (Byte) Tables.TypeDef ) << 24; // 0x2000000;
      private const Int32 TYPE_REF_MASK = ( (Byte) Tables.TypeRef ) << 24; // 0x1000000;
      private const Int32 TYPE_SPEC_MASK = ( (Byte) Tables.TypeSpec ) << 24; // 0x1B000000;

      internal static Tuple<Tables, Int32> DecodeToken( Int32 token )
      {
         Tables table;
         Int32 index;
         DecodeToken( token, out table, out index );
         return Tuple.Create( table, index );
      }

      internal static Tables DecodeTokenTable( Int32 token )
      {
         return (Tables) ( unchecked( (UInt32) token ) >> 24 );
      }

      internal static void DecodeToken( Int32 token, out Tables table, out Int32 index )
      {
         table = (Tables) ( unchecked( (UInt32) token ) >> 24 );
         index = token & INDEX_MASK;
      }

      internal static void DecodeTokenZeroBased( Int32 token, out Tables table, out Int32 index )
      {
         table = (Tables) ( unchecked( (UInt32) token ) >> 24 );
         index = ( token & INDEX_MASK ) - 1;
      }

      internal static Int32 EncodeToken( Tuple<Tables, Int32> tableRef )
      {
         return EncodeToken( tableRef.Item1, tableRef.Item2 );
      }

      internal static Int32 EncodeToken( Tables table, Int32 index )
      {
         return EncodeToken( (Int32) table, index );
      }

      internal static Int32 EncodeToken( Int32 table, Int32 index )
      {
         return ( ( table ) << 24 ) | index;
      }

      internal static Int32 DecodeTypeDefOrRefOrSpec( Byte[] array, ref Int32 offset )
      {
         var decodedValue = array.DecompressUInt32( ref offset );
         switch ( decodedValue & TABLE_EXTRACTOR_MASK )
         {
            case TYPE_DEF:
               decodedValue = TYPE_DEF_MASK | ( decodedValue >> 2 );
               break;
            case TYPE_REF:
               decodedValue = TYPE_REF_MASK | ( decodedValue >> 2 );
               break;
            case TYPE_SPEC:
               decodedValue = TYPE_SPEC_MASK | ( decodedValue >> 2 );
               break;
            default:
               throw new ArgumentException( "Token table resolved to not supported: " + (Tables) ( decodedValue & TABLE_EXTRACTOR_MASK ) + "." );
         }
         return decodedValue;
      }

      internal static Int32 EncodeTypeDefOrRefOrSpec( Int32 token )
      {
         Int32 encodedValue;
         switch ( unchecked( (UInt32) token ) >> 24 )
         {
            case (UInt32) Tables.TypeDef:
               encodedValue = ( ( INDEX_MASK & token ) << 2 ) | TYPE_DEF;
               break;
            case (UInt32) Tables.TypeRef:
               encodedValue = ( ( INDEX_MASK & token ) << 2 ) | TYPE_REF;
               break;
            case (UInt32) Tables.TypeSpec:
               encodedValue = ( ( INDEX_MASK & token ) << 2 ) | TYPE_SPEC;
               break;
            default:
               throw new ArgumentException( "Token must reference one of the following tables: " + String.Join( ", ", Tables.TypeDef, Tables.TypeRef, Tables.TypeSpec ) + "." );
         }
         return encodedValue;
      }
   }
}