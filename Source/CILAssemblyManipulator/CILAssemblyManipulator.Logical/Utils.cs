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
using System.Linq;
using CILAssemblyManipulator.Logical.Implementation;
using System.Text;
using System.Collections.Generic;
using CILAssemblyManipulator.Physical;
using CollectionsWithRoles.API;
using System.Runtime.InteropServices;

namespace CILAssemblyManipulator.Logical
{
   /// <summary>
   /// This class contains miscellaneous utility methods related to CIL assembly emitting.
   /// </summary>
   public static class LogicalUtils
   {
      //      internal const String MD_NET_1_0 = "v1.0.3705";
      //      internal const String MD_NET_1_1 = "v1.1.4322";
      //      internal const String MD_NET_2_0 = "v2.0.50727";
      //      internal const String MD_NET_4_0 = "v4.0.30319";

      internal static readonly System.Reflection.Assembly NATIVE_MSCORLIB = typeof( Object )
#if WINDOWS_PHONE_APP
      .GetTypeInfo()
#endif
.Assembly;

      //      private const Char ESCAPE_CHAR = '\\';

      //      private static readonly Char[] ESCAPABLE_CHARS_WITHIN_TYPESTRING = { ESCAPE_CHAR, ',', '+', '&', '*', '[', ']' };
      //      private static readonly Char[] CHARS_ENDING_SIMPLE_TYPENAME = { '&', '*', '[' };
      //      private const String TYPE_ASSEMBLY_SEPARATOR = ", ";


      /// <summary>
      /// Returns <see cref="ElementKind"/> of the given native type.
      /// </summary>
      /// <param name="type">The native type.</param>
      /// <returns><c>null</c> if <paramref name="type"/> is not element type, otherwise <see cref="ElementKind"/> of the <paramref name="type"/>.</returns>
      public static ElementKind? GetElementKind( this Type type )
      {
         if ( type.IsArray )
         {
            return ElementKind.Array;
         }
         else if ( type.IsPointer )
         {
            return ElementKind.Pointer;
         }
         else if ( type.IsByRef )
         {
            return ElementKind.Reference;
         }
         else
         {
            return null;
         }
      }

      /// <summary>
      /// Checks whether <paramref name="type"/> is not <c>null</c> and is vector array type.
      /// </summary>
      /// <param name="type">The type to check.</param>
      /// <returns><c>true</c> if <paramref name="type"/> is not <c>null</c> and is vector array type; <c>false</c> otherwise.</returns>
      public static Boolean IsVectorArray( this Type type )
      {
         return type != null && type.IsArray && type.Name.EndsWith( "[]" );
      }

      /// <summary>
      /// Checks whether <paramref name="type"/> is not <c>null</c> and is multi-dimensional array type.
      /// </summary>
      /// <param name="type">The type to check.</param>
      /// <returns><c>true</c> if <paramref name="type"/> is not <c>null</c> and is multi-dimensional array type; <c>false</c> otherwise.</returns>
      public static Boolean IsMultiDimensionalArray( this Type type )
      {
         return type != null && type.IsArray && type.Name.EndsWith( "]" ) && type.Name[type.Name.Length - 2] != '[';
      }

      //      /// <summary>
      //      /// Helper method to extact full public key from a private key CAPI BLOB.
      //      /// </summary>
      //      /// <param name="privateOrPublicKeyBLOB">The private or public key BLOB in CAPI format (basically a .snk file).</param>
      //      /// <param name="signingAlgorithm">The signing algorithm to use.</param>
      //      /// <returns>The full public key of the assembly that is signed with <paramref name="privateOrPublicKeyBLOB"/> using <paramref name="signingAlgorithm"/> as signing algorithm.</returns>
      //      public static Byte[] ExtractPublicKey( Byte[] privateOrPublicKeyBLOB, AssemblyHashAlgorithm signingAlgorithm )
      //      {
      //         Byte[] pk;
      //         AssemblyHashAlgorithm dummy;
      //         CILAssemblyManipulator.Logical.Implementation.Physical.CryptoUtils.CreateSigningInformationFromKeyBLOB( privateOrPublicKeyBLOB, signingAlgorithm, out pk, out dummy );
      //         return pk;
      //      }

      //      internal struct TypeParseResult
      //      {
      //         internal readonly CILAssemblyName assemblyName;
      //         internal readonly String nameSpace;
      //         internal readonly String typeName;
      //         internal readonly IList<String> genericArguments;
      //         internal readonly IList<Tuple<ElementKind, GeneralArrayInfo>> elementInfo;
      //         internal readonly IList<String> nestedTypes;

      //         internal TypeParseResult( CILAssemblyName an, String ns, String tn, IList<String> gArgs, IList<Tuple<ElementKind, GeneralArrayInfo>> elInfo, IList<String> nt )
      //         {
      //            this.assemblyName = an;
      //            this.nameSpace = ns;
      //            this.typeName = tn;
      //            this.genericArguments = gArgs;
      //            this.elementInfo = elInfo;
      //            this.nestedTypes = nt;
      //         }
      //      }

      //      internal static TypeParseResult ParseTypeString( String typeStr )
      //      {
      //         // If there is an assembly name, there will be ", " but not "\, " in type string.
      //         var an = ParseAssemblyNameFromTypeString( ref typeStr );

      //         if ( String.IsNullOrEmpty( typeStr ) )
      //         {
      //            // Assembly name but no type name?
      //            throw new ArgumentException( "The string \"" + typeStr + "\" does not contain a type name." );
      //         }

      //         // Find next escapable but not escaped character: '&' (by-ref), '*' (pointer), '[' (array or generic type)
      //         List<Tuple<ElementKind, GeneralArrayInfo>> elInfo = null;
      //         List<String> gArgs = null;
      //         var curIdx = IndexOfNonEscaped( typeStr, 0, CHARS_ENDING_SIMPLE_TYPENAME );
      //         if ( curIdx >= 0 )
      //         {
      //            // Element kind or generic parameters present
      //            var tmp = curIdx;
      //            while ( tmp >= 0 )
      //            {
      //               switch ( typeStr[tmp] )
      //               {
      //                  case '&':
      //                     if ( elInfo == null )
      //                     {
      //                        elInfo = new List<Tuple<ElementKind, GeneralArrayInfo>>();
      //                     }
      //                     elInfo.Add( Tuple.Create<ElementKind, GeneralArrayInfo>( ElementKind.Reference, null ) );
      //                     break;
      //                  case '*':
      //                     if ( elInfo == null )
      //                     {
      //                        elInfo = new List<Tuple<ElementKind, GeneralArrayInfo>>();
      //                     }
      //                     elInfo.Add( Tuple.Create<ElementKind, GeneralArrayInfo>( ElementKind.Pointer, null ) );
      //                     break;
      //                  case '[':
      //                     // Either array or generic type.
      //                     Int32 arrayRank;
      //                     var tmp2 = tmp;
      //                     var isArray = IsArray( typeStr, ref tmp2, out arrayRank );
      //                     if ( isArray )
      //                     {
      //                        if ( elInfo == null )
      //                        {
      //                           elInfo = new List<Tuple<ElementKind, GeneralArrayInfo>>();
      //                        }
      //                        // TODO ArrayInfo
      //                        elInfo.Add( Tuple.Create( ElementKind.Array, ReadArrayInfo( typeStr, tmp, tmp2, arrayRank ) ) );
      //                        tmp = tmp2;
      //                     }
      //                     else
      //                     {
      //                        tmp2 = tmp + 1;
      //                        // Generic type. Iterate through, remember [] -depth, and read generic argument strings
      //                        if ( gArgs != null )
      //                        {
      //                           // Specifying generic types x2
      //                           throw new ArgumentException( "Invalid type string \"" + typeStr + "\". Generic arguments were specified more than once." );
      //                        }
      //                        gArgs = new List<String>();
      //                        var curBracketDepth = 0;
      //                        while ( ++tmp < typeStr.Length && curBracketDepth >= 0 )
      //                        {
      //                           // Skip escaped characters
      //                           if ( typeStr[tmp - 1] != '\\' )
      //                           {
      //                              var ch = typeStr[tmp];
      //                              if ( ch == '[' )
      //                              {
      //                                 ++curBracketDepth;
      //                              }
      //                              else if ( ch == ']' )
      //                              {
      //                                 --curBracketDepth;
      //                                 if ( curBracketDepth < 0 )
      //                                 {
      //                                    // Add last argument
      //                                    gArgs.Add( UnescapeSomeString( typeStr.Substring( tmp2, tmp - tmp2 ) ) );
      //                                 }
      //                              }
      //                              else if ( ch == ',' && curBracketDepth == 0 )
      //                              {
      //                                 // "Top-level" comma, meaning generic argument separator.
      //                                 gArgs.Add( UnescapeSomeString( typeStr.Substring( tmp2, tmp - tmp2 ) ) );
      //                                 tmp2 = tmp + 1;
      //                              }
      //                           }
      //                        }
      //                        if ( curBracketDepth >= 0 )
      //                        {
      //                           throw new ArgumentException( "Invalid type string \"" + typeStr + "\". String ended before all generic arguments were specified." );
      //                        }
      //                        if ( gArgs.Count == 0 )
      //                        {
      //                           throw new Exception( "Internal error in type parsing, no generic arguments added when should have to." );
      //                        }
      //                     }
      //                     break;
      //               }
      //               tmp = IndexOfNonEscaped( typeStr, tmp + 1, CHARS_ENDING_SIMPLE_TYPENAME );
      //            }
      //            typeStr = typeStr.Substring( 0, curIdx );
      //         }

      //         // Construct nested type information
      //         String ns, tn;
      //         List<String> nt = null;
      //         var nSep = IndexOfNonEscaped( typeStr, 0, '+' );
      //         if ( nSep >= 0 && nSep < typeStr.Length - 1 )
      //         {
      //            // Nested type
      //            nt = new List<String>();
      //            var min = nSep + 1;
      //            Int32 max;
      //            do
      //            {
      //               max = IndexOfNonEscaped( typeStr, min, '+' );
      //               if ( max < 0 )
      //               {
      //                  max = typeStr.Length;
      //               }
      //               nt.Add( UnescapeSomeString( typeStr.Substring( min, max - min ) ) );
      //               min = max + 1;
      //            } while ( min < typeStr.Length );
      //            typeStr = typeStr.Substring( 0, nSep );
      //         }

      //         // Parse namespace information
      //         nSep = typeStr.LastIndexOf( '.' );
      //         ns = nSep == -1 ? null : typeStr.Substring( 0, nSep );
      //         tn = nSep == -1 ? typeStr : typeStr.Substring( nSep + 1 );
      //         return new TypeParseResult( an, UnescapeSomeString( ns ), UnescapeSomeString( tn ), gArgs, elInfo, nt );
      //      }

      //      internal static String NamespaceAndTypeName( String ns, String tn )
      //      {
      //         return String.IsNullOrEmpty( ns ) ? tn : ( ns + '.' + tn );
      //      }

      //      private static Boolean IsArray( String typeStr, ref Int32 idx, out Int32 rank )
      //      {
      //         // Array type will either have ']' as next character, or numbers, commas, and dots before ']', nothing else.
      //         var max = IndexOfNonEscaped( typeStr, idx, ']' );
      //         var isArray = true;
      //         rank = 1;
      //         for ( var i = idx; i < max; ++i )
      //         {
      //            var ch = typeStr[i];
      //            if ( !Char.IsDigit( ch ) || ch != '.' || ch != ',' || ch != '*' )
      //            {
      //               isArray = false;
      //               break;
      //            }
      //            else if ( ch == ',' )
      //            {
      //               ++rank;
      //            }
      //         }
      //         idx = max;
      //         return isArray;
      //      }

      //      // Start idx = index of '[', End idx = index of ']'
      //      private static GeneralArrayInfo ReadArrayInfo( String typeStr, Int32 startIdx, Int32 endIdx, Int32 rank )
      //      {
      //         GeneralArrayInfo result;
      //         if ( startIdx == endIdx - 1 )
      //         {
      //            // Vector array
      //            result = null;
      //         }
      //         else
      //         {
      //            // General array
      //            ++startIdx;
      //            var sizes = new Int32[rank];
      //            var sizeIdx = 0;
      //            var loBounds = new Int32[rank];
      //            var loBoundIdx = 0;
      //            // Check for special case - rank 1, no sizes, no lower bounds (is '[*]').
      //            if ( startIdx != endIdx - 1 || typeStr[startIdx] != '*' )
      //            {
      //               var curIdx = startIdx;

      //               while ( curIdx <= endIdx )
      //               {
      //                  switch ( typeStr[curIdx] )
      //                  {
      //                     case ',':
      //                     case ']':
      //                        // End of dimension
      //                        if ( startIdx != curIdx )
      //                        {
      //                           // Size specified
      //                           // Implicitly add zero lower bound if needed
      //                           if ( sizeIdx == loBoundIdx )
      //                           {
      //                              loBounds[loBoundIdx++] = 0;
      //                           }
      //                           // Then save size
      //                           sizes[sizeIdx++] = Int32.Parse( typeStr.Substring( startIdx, curIdx - startIdx ) );
      //                        }
      //                        startIdx = curIdx + 1;
      //                        break;
      //                     case '.':
      //                        if ( startIdx != curIdx )
      //                        {
      //                           // End of lower bound
      //                           loBounds[loBoundIdx++] = Int32.Parse( typeStr.Substring( startIdx, curIdx - startIdx ) );
      //                        }
      //                        startIdx = curIdx + 1;
      //                        break;
      //                  }
      //                  ++curIdx;
      //               }
      //            }
      //            // Create actual size and lower bounds arrays
      //            var actualSizes = new Int32[sizeIdx];
      //            Array.Copy( sizes, actualSizes, sizeIdx );
      //            var actualLoBounds = new Int32[loBoundIdx];
      //            Array.Copy( loBounds, actualLoBounds, loBoundIdx );
      //            result = new GeneralArrayInfo( rank, actualSizes, actualLoBounds );
      //         }
      //         return result;
      //      }

      //      private static CILAssemblyName ParseAssemblyNameFromTypeString( ref String str )
      //      {
      //         var typeStrMax = 0;
      //         var curIdx = -1;
      //         while ( typeStrMax < str.Length
      //            && ( curIdx = str.IndexOf( TYPE_ASSEMBLY_SEPARATOR, typeStrMax ) ) > 0
      //            && str[curIdx - 1] == ESCAPE_CHAR )
      //         {
      //            typeStrMax = curIdx + TYPE_ASSEMBLY_SEPARATOR.Length;
      //         }

      //         CILAssemblyName an;
      //         if ( curIdx < 0 || typeStrMax >= str.Length )
      //         {
      //            // No assembly name present
      //            an = null;
      //         }
      //         else
      //         {
      //            // Parse assembly name and set type string to hold actual type string
      //            an = CILAssemblyName.Parse( str.Substring( curIdx + TYPE_ASSEMBLY_SEPARATOR.Length ) );
      //            str = str.Substring( 0, curIdx );
      //         }
      //         return an;
      //      }

      //      private static Int32 IndexOfNonEscaped( String str, Int32 startIdx, Char[] chars )
      //      {
      //         var chIdx = -1;
      //         while ( startIdx < str.Length
      //            && ( chIdx = str.IndexOfAny( chars, startIdx ) ) > 0
      //            && str[chIdx - 1] == ESCAPE_CHAR )
      //         {
      //            startIdx = chIdx + 1;
      //         }
      //         return chIdx < 0 || startIdx >= str.Length ? -1 : chIdx;
      //      }

      //      private static Int32 IndexOfNonEscaped( String str, Int32 startIdx, Char ch )
      //      {
      //         var chIdx = -1;
      //         while ( startIdx < str.Length
      //            && ( chIdx = str.IndexOf( ch, startIdx ) ) > 0
      //            && str[chIdx - 1] == ESCAPE_CHAR )
      //         {
      //            startIdx = chIdx + 1;
      //         }
      //         return chIdx < 0 || startIdx >= str.Length ? -1 : chIdx;
      //      }

      internal static String CreateTypeString( CILType type, Boolean appendGArgs )
      {
         String typeString;
         if ( type == null )
         {
            typeString = null;
         }
         else
         {
            var builder = new StringBuilder();
            CreateTypeStringCore( type, type.Module, builder, appendGArgs );
            typeString = builder.ToString();
         }
         return typeString;
      }

      //private static void CreateTypeString( CILTypeBase type, CILModule moduleBeingEmitted, StringBuilder builder, Boolean appendGArgs, Boolean isGParam )
      //{
      //   var needsAssembly = moduleBeingEmitted != null && !moduleBeingEmitted.Assembly.Equals( type.Module.Assembly );
      //   if ( isGParam && needsAssembly )
      //   {
      //      builder.Append( '[' );
      //   }
      //   CreateTypeStringCore( type, moduleBeingEmitted, builder, appendGArgs );
      //   if ( needsAssembly )
      //   {
      //      builder
      //         .Append( TYPE_ASSEMBLY_SEPARATOR )
      //         .Append( type.Module.Assembly.Name.ToString() ); // Assembly name will be escaped.
      //      if ( isGParam )
      //      {
      //         builder.Append( ']' );
      //      }
      //   }
      //}

      private static void CreateTypeStringCore( CILTypeBase type, CILModule moduleBeingEmitted, StringBuilder builder, Boolean appendGArgs )
      {
         var eKind = type.GetElementKind();
         if ( eKind.HasValue )
         {
            CreateTypeStringCore( type.GetElementType(), moduleBeingEmitted, builder, appendGArgs );
            CreateElementKindString( eKind.Value, ( (CILType) type ).ArrayInformation, builder );
         }
         else if ( appendGArgs && type.IsGenericType() && !type.ContainsGenericParameters() )
         {
            var typee = (CILType) type;
            CreateTypeStringCore( typee.GenericDefinition, moduleBeingEmitted, builder, false );
            builder.Append( '[' );
            var gArgs = typee.GenericArguments;
            for ( var i = 0; i < gArgs.Count; ++i )
            {
               var gArg = gArgs[i];
               var needsAssembly = moduleBeingEmitted != null
                  && gArg.TypeKind != TypeKind.MethodSignature
                  && !moduleBeingEmitted.Assembly.Equals( gArg.Module.Assembly );
               if ( needsAssembly )
               {
                  builder.Append( '[' );
               }

               CreateTypeStringCore( gArg, moduleBeingEmitted, builder, true );
               if ( needsAssembly )
               {
                  builder
                     .AppendAssemblyNameToTypeString( gArg.Module.Assembly.Name.ToString() ) // Assembly name will be escaped.
                     .Append( ']' );
               }
               if ( i < gArgs.Count - 1 )
               {
                  builder.Append( ',' );
               }
            }
            builder.Append( ']' );
         }
         else
         {
            switch ( type.TypeKind )
            {
               case TypeKind.Type:
                  var typee = (CILType) type;
                  var dt = typee.DeclaringType;
                  if ( dt == null )
                  {
                     var ns = typee.Namespace;
                     if ( !String.IsNullOrEmpty( ns ) )
                     {
                        builder.Append( ns.EscapeCILTypeString() ).Append( Miscellaneous.NAMESPACE_SEPARATOR );
                     }
                  }
                  else
                  {
                     CreateTypeStringCore( dt, moduleBeingEmitted, builder, false );
                     builder.Append( Miscellaneous.NESTED_TYPE_SEPARATOR );
                  }
                  builder.Append( typee.Name.EscapeCILTypeString() );
                  break;
               case TypeKind.MethodSignature:
                  builder.Append( type.ToString() );
                  break;
               case TypeKind.TypeParameter:
                  builder.Append( ( (CILTypeParameter) type ).Name );
                  break;
            }
         }
      }

      internal static String CreateElementKindString( ElementKind elementKind, GeneralArrayInfo arrayInfo )
      {
         var builder = new StringBuilder();
         CreateElementKindString( elementKind, arrayInfo, builder );
         return builder.ToString();
      }

      private static void CreateElementKindString( ElementKind elementKind, GeneralArrayInfo arrayInfo, StringBuilder builder )
      {
         switch ( elementKind )
         {
            case ElementKind.Array:
               builder.Append( '[' );
               if ( arrayInfo != null )
               {
                  if ( arrayInfo.Rank == 1 && arrayInfo.Sizes.Count == 0 && arrayInfo.LowerBounds.Count == 0 )
                  {
                     // Special case
                     builder.Append( '*' );
                  }
                  else
                  {
                     for ( var i = 0; i < arrayInfo.Rank; ++i )
                     {
                        var appendLoBound = i < arrayInfo.LowerBounds.Count;
                        if ( appendLoBound )
                        {
                           var loBound = arrayInfo.LowerBounds[i];
                           appendLoBound = loBound != 0;
                           if ( appendLoBound )
                           {
                              builder.Append( loBound ).Append( ".." );
                           }
                        }
                        if ( i < arrayInfo.Sizes.Count )
                        {
                           builder.Append( arrayInfo.Sizes[i] );
                        }
                        else if ( appendLoBound )
                        {
                           builder.Append( '.' );
                        }

                        if ( i < arrayInfo.Rank - 1 )
                        {
                           builder.Append( ',' );
                        }
                     }
                  }
               }
               builder.Append( ']' );
               break;
            case ElementKind.Pointer:
               builder.Append( '*' );
               break;
            case ElementKind.Reference:
               builder.Append( '&' );
               break;
         }
      }

      internal static void ThrowIfDeclaringTypeGenericButNotGDef( CILElementOwnedByType element )
      {
         var gDef = element.DeclaringType.GenericDefinition;
         if ( gDef != null && !Object.ReferenceEquals( gDef, element.DeclaringType ) )
         {
            throw new InvalidOperationException( "This method can not be used on generic types, which are not generic type definitions." );
         }
      }

      internal static void ThrowIfDeclaringTypeNotGeneric( CILElementOwnedByType element, CILTypeBase[] gArgs )
      {
         var gDef = element.DeclaringType.GenericDefinition;
         if ( gDef == null && gArgs != null && gArgs.Length != 0 )
         {
            throw new InvalidOperationException( "This method can only be used on elements declared in generic types." );
         }
      }

      //internal static void CheckCyclity( this IEnumerable<CILTypeBase> graph, Object thisType )
      //{
      //   if ( graph.Any( i => Object.ReferenceEquals( thisType, i ) ) )
      //   {
      //      throw new ArgumentException( "Cyclity detected between " + thisType + " and " + graph.First( i => Object.ReferenceEquals( thisType, i ) ) + "." );
      //   }
      //}

      internal static void CheckWhenDefiningGArgs( ListProxy<CILTypeBase> currentGArgs, String[] names )
      {
         if ( currentGArgs.MQ.Count > 0 )
         {
            throw new InvalidOperationException( "Generic arguments have already been defined." );
         }
      }

      internal static T AddToResettableLazyList<T>( this ResettableLazy<ListProxy<T>> lazy, T value )
      {
         lazy.Value.Add( value );
         return value;
      }

      internal static void CheckMethodAttributesForOverriddenMethods( SettableValueForEnums<MethodAttributes> attrs, ListProxy<CILMethod> overriddenMethods )
      {
         if ( overriddenMethods.CQ.Any() )
         {
            attrs.Value = ( attrs.Value & ( ~MethodAttributes.MemberAccessMask ) ) | MethodAttributes.Private;
         }
      }

      internal static void ThrowIfNotTrueDefinition( this CILCustomAttributeContainer element )
      {
         if ( element != null && !( (CILElementInstantiable) element ).IsTrueDefinition )
         {
            throw new ArgumentException( "Given argument is not true definition." );
         }
      }

      internal static Boolean IsGenericDefinition<T>( this CILElementWithGenericArguments<T> element )
         where T : class
      {
         return Object.ReferenceEquals( element, element.GenericDefinition );
      }

      internal static SignatureStarters GetSignatureStarter( this CallingConventions convs, Boolean isStatic, Boolean isGeneric )
      {
         var starter = SignatureStarters.Default;
         if ( !isStatic )
         {
            starter |= SignatureStarters.HasThis;
         }
         if ( convs.IsExplicitThis() )
         {
            starter |= SignatureStarters.ExplicitThis;
         }

         if ( isGeneric )
         {
            starter |= SignatureStarters.Generic;
         }
         else if ( convs.IsVarArgs() )
         {
            starter |= SignatureStarters.VarArgs;
         }
         return starter;
      }

      internal static CallingConventions GetCallingConventionFromSignature( this SignatureStarters starter )
      {
         // TODO is this method correct?
         CallingConventions result = 0;
         if ( starter.IsHasThis() )
         {
            result = CallingConventions.HasThis;
         }
         if ( starter.IsExplicitThis() )
         {
            result |= CallingConventions.ExplicitThis;
         }
         if ( starter.IsVarArg() )
         {
            result |= CallingConventions.VarArgs;
         }
         else if ( starter.IsGeneric() )
         {
            result |= CallingConventions.Standard;
         }
         return result;
      }

      internal static void CheckTypeForMethodSig( CILModule thisModule, ref CILTypeBase type )
      {
         if ( TypeKind.MethodSignature == type.TypeKind && !Object.Equals( thisModule, type.Module ) )
         {
            type = ( (CILMethodSignature) type ).CopyToOtherModule( thisModule );
         }
      }

      // Mofidied from http://stackoverflow.com/questions/1068541/how-to-convert-a-value-type-to-byte-in-c
      internal static Byte[] ObjectToByteArray( Object value )
      {
         // TODO SL (get thru context?)
#if CAM_LOGICAL_IS_SL
         return null;
#else
         var rawsize = Marshal.SizeOf( value );
         var rawdata = new Byte[rawsize];
         var handle =
             GCHandle.Alloc( rawdata,
             GCHandleType.Pinned );
         try
         {
            Marshal.StructureToPtr( value,
                handle.AddrOfPinnedObject(),
                false );
         }
         finally
         {
            handle.Free();
         }
         return rawdata;
#endif
      }

   }
}