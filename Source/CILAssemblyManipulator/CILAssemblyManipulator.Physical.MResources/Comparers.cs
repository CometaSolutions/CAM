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
using CILAssemblyManipulator.Physical.MResources;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
#pragma warning disable 1591
   public static class Comparers
#pragma warning restore 1591
   {
      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="ResourceManagerEntry"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="ResourceManagerEntry"/> are equal.</value>
      /// <seealso cref="PreDefinedResourceManagerEntryEqualityComparer"/>
      /// <seealso cref="UserDefinedResourceManagerEntryEqualityComparer"/>
      public static IEqualityComparer<ResourceManagerEntry> ResourceManagerEntryEqualityComparer { get; }

      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="PreDefinedResourceManagerEntry"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="PreDefinedResourceManagerEntry"/> are equal.</value>
      /// <remarks>
      /// Two instances of <see cref="PreDefinedResourceManagerEntry"/> are considered to be equal by this equality when:
      /// <list type="bullet">
      /// <item><description><see cref="PreDefinedResourceManagerEntry.Value"/> match using <see cref="Object.Equals(Object, Object)"/> method.</description></item>
      /// </list>
      /// </remarks>
      public static IEqualityComparer<PreDefinedResourceManagerEntry> PreDefinedResourceManagerEntryEqualityComparer { get; }

      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="UserDefinedResourceManagerEntry"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="UserDefinedResourceManagerEntry"/> are equal.</value>
      /// <remarks>
      /// Two instances of <see cref="UserDefinedResourceManagerEntry"/> are considered to be equal by this equality when:
      /// <list type="bullet">
      /// <item><description><see cref="UserDefinedResourceManagerEntry.UserDefinedType"/> match exactly, using <see cref="String.Equals(String, String)"/> method, and</description></item>
      /// <item><description><see cref="UserDefinedResourceManagerEntry.Contents"/> match exactly with elements being compared using <see cref="AbstractRecordEqualityComparer"/>.</description></item>
      /// </list>
      /// </remarks>
      /// <seealso cref="AbstractRecordEqualityComparer"/>
      public static IEqualityComparer<UserDefinedResourceManagerEntry> UserDefinedResourceManagerEntryEqualityComparer { get; }


      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="AbstractRecord"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="AbstractRecord"/> are equal.</value>
      /// <seealso cref="ClassRecordEqualityComparer"/>
      /// <seealso cref="ArrayRecordEqualityComparer"/>
      public static IEqualityComparer<AbstractRecord> AbstractRecordEqualityComparer { get; }

      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="ClassRecord"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="ClassRecord"/> are equal.</value>
      /// <remarks>
      /// Two instances of <see cref="ClassRecord"/> are considered to be equal by this equality when:
      /// <list type="bullet">
      /// <item><description><see cref="ClassRecord.TypeName"/> match exactly, with <see cref="String.Equals(String, String)"/> method, and</description></item>
      /// <item><description><see cref="ClassRecord.AssemblyName"/> match exactly, with <see cref="String.Equals(String, String)"/> method, and</description></item>
      /// <item><description><see cref="ClassRecord.Members"/> match using <see cref="ListEqualityComparer{T, U}.IsPermutation"/> method (i.e. the order of <see cref="ClassRecordMember"/>s does not matter), with elements being compared using <see cref="ClassRecordMemberEqualityComparer"/>.</description></item>
      /// </list>
      /// </remarks>
      /// <seealso cref="ClassRecordMemberEqualityComparer"/>
      public static IEqualityComparer<ClassRecord> ClassRecordEqualityComparer { get; }

      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="ArrayRecord"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="ArrayRecord"/> are equal.</value>
      /// <remarks>
      /// Two instances of <see cref="ArrayRecord"/> are considered to be equal by this equality when:
      /// <list type="bullet">
      /// <item><description><see cref="ArrayRecord.TypeName"/> match exactly, with <see cref="String.Equals(String, String)"/> method, and</description></item>
      /// <item><description><see cref="ArrayRecord.AssemblyName"/> match exactly, with <see cref="String.Equals(String, String)"/> method, and</description></item>
      /// <item><description><see cref="ArrayRecord.ArrayKind"/> match exactly, and</description></item>
      /// <item><description><see cref="ArrayRecord.Rank"/> match exactly, and</description></item>
      /// <item><description><see cref="ArrayRecord.Lengths"/> match exactly, and</description></item>
      /// <item><description><see cref="ArrayRecord.LowerBounds"/> match exactly, and</description></item>
      /// <item><description><see cref="ArrayRecord.ValuesAsVector"/> match exactly with elements being compared using <see cref="ArrayElementOrMemberValueEqualityComparer"/>.</description></item>
      /// </list>
      /// </remarks>
      /// <seealso cref="ArrayElementOrMemberValueEqualityComparer"/>
      public static IEqualityComparer<ArrayRecord> ArrayRecordEqualityComparer { get; }

      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="ClassRecordMember"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="ClassRecordMember"/> are equal.</value>
      /// <remarks>
      /// Two instances of <see cref="ClassRecordMember"/> are considered to be equal by this equality comparer when:
      /// <list type="bullet">
      /// <item><description><see cref="ClassRecordMember.Name"/> match exactly, with <see cref="String.Equals(String, String)"/> method, and</description></item>
      /// <item><description><see cref="ClassRecordMember.TypeName"/> match exactly, with <see cref="String.Equals(String, String)"/> method, and</description></item>
      /// <item><description><see cref="ClassRecordMember.AssemblyName"/> match exactly, with <see cref="String.Equals(String, String)"/> method, and</description></item>
      /// <item><description><see cref="ClassRecordMember.Value"/> match using <see cref="ArrayElementOrMemberValueEqualityComparer"/>.</description></item>
      /// </list>
      /// </remarks>
      /// <seealso cref="ArrayElementOrMemberValueEqualityComparer"/>
      public static IEqualityComparer<ClassRecordMember> ClassRecordMemberEqualityComparer { get; }

      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="ClassRecordMember.Value"/> value, or elements belonging to <see cref="ArrayRecord.ValuesAsVector"/> list, are considered to be equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="ClassRecordMember.Value"/> value, or elements belonging to <see cref="ArrayRecord.ValuesAsVector"/> list, are considered to be equal.</value>
      /// <remarks>
      /// Two instances of <see cref="Object"/>s belonging to <see cref="ClassRecordMember.Value"/> or <see cref="ArrayRecord.ValuesAsVector"/> are considered to be equal by this equality when:
      /// <list type="bullet">
      /// <item><description>both are instance of <see cref="AbstractRecord"/> and <see cref="AbstractRecordEqualityComparer"/> returns <c>true</c> for the equality comparison, or</description></item>
      /// <item><description>neither are instance of <see cref="AbstractRecord"/> and <see cref="Object.Equals(Object, Object)"/> returns <c>true</c> for the equality comparison.</description></item>
      /// </list>
      /// </remarks>
      public static IEqualityComparer<Object> ArrayElementOrMemberValueEqualityComparer { get; }

      static Comparers()
      {
         ResourceManagerEntryEqualityComparer = ComparerFromFunctions.NewEqualityComparer<ResourceManagerEntry>( Equality_ResourceManagerEntry, HashCode_ResourceManagerEntry );
         PreDefinedResourceManagerEntryEqualityComparer = ComparerFromFunctions.NewEqualityComparer<PreDefinedResourceManagerEntry>( Equality_ResourceManagerEntry_PreDefined, HashCode_ResourceManagerEntry_PreDefined );
         UserDefinedResourceManagerEntryEqualityComparer = ComparerFromFunctions.NewEqualityComparer<UserDefinedResourceManagerEntry>( Equality_ResourceManagerEntry_UserDefined, HashCode_ResourceManagerEntry_UserDefined );
         AbstractRecordEqualityComparer = ComparerFromFunctions.NewEqualityComparer<AbstractRecord>( Equality_AbstractRecord, HashCode_AbstractRecord );
         ClassRecordEqualityComparer = ComparerFromFunctions.NewEqualityComparer<ClassRecord>( Equality_ClassRecord, HashCode_ClassRecord );
         ArrayRecordEqualityComparer = ComparerFromFunctions.NewEqualityComparer<ArrayRecord>( Equality_ArrayRecord, HashCode_ArrayRecord );
         ClassRecordMemberEqualityComparer = ComparerFromFunctions.NewEqualityComparer<ClassRecordMember>( Equality_ClassRecordMember, HashCode_ClassRecordMember );
         ArrayElementOrMemberValueEqualityComparer = ComparerFromFunctions.NewEqualityComparer<Object>( Equality_ClassRecordMemberValue, HashCode_ClassRecordMemberValue );
      }

      private static Boolean Equality_ResourceManagerEntry( ResourceManagerEntry x, ResourceManagerEntry y )
      {
         var retVal = ReferenceEquals( x, y );
         if ( !retVal && x != null && y != null && x.ResourceManagerEntryKind == y.ResourceManagerEntryKind )
         {
            switch ( x.ResourceManagerEntryKind )
            {
               case ResourceManagerEntryKind.PreDefined:
                  retVal = Equality_ResourceManagerEntry_PreDefined( x as PreDefinedResourceManagerEntry, y as PreDefinedResourceManagerEntry );
                  break;
               case ResourceManagerEntryKind.UserDefined:
                  retVal = Equality_ResourceManagerEntry_UserDefined( x as UserDefinedResourceManagerEntry, y as UserDefinedResourceManagerEntry );
                  break;
            }
         }
         return retVal;
      }

      private static Boolean Equality_ResourceManagerEntry_PreDefined( PreDefinedResourceManagerEntry x, PreDefinedResourceManagerEntry y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && Equals( x.Value, y.Value )
            );
      }

      private static Boolean Equality_ResourceManagerEntry_UserDefined( UserDefinedResourceManagerEntry x, UserDefinedResourceManagerEntry y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.UserDefinedType, y.UserDefinedType )
            && ListEqualityComparer<List<AbstractRecord>, AbstractRecord>.ListEquality( x.Contents, y.Contents, Equality_AbstractRecord )
            );
      }

      private static Boolean Equality_AbstractRecord( AbstractRecord x, AbstractRecord y )
      {
         var retVal = ReferenceEquals( x, y );
         if ( !retVal && x != null && y != null && x.RecordKind == y.RecordKind )
         {
            switch ( x.RecordKind )
            {
               case RecordKind.Class:
                  retVal = Equality_ClassRecord( x as ClassRecord, y as ClassRecord );
                  break;
               case RecordKind.Array:
                  retVal = Equality_ArrayRecord( x as ArrayRecord, y as ArrayRecord );
                  break;
            }
         }
         return retVal;
      }

      private static Boolean Equality_ClassRecord( ClassRecord x, ClassRecord y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.TypeName, y.TypeName )
            && String.Equals( x.AssemblyName, y.AssemblyName )
            && ListEqualityComparer<List<ClassRecordMember>, ClassRecordMember>.IsPermutation( x.Members, y.Members, ClassRecordMemberEqualityComparer )
            );
      }

      private static Boolean Equality_ArrayRecord( ArrayRecord x, ArrayRecord y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.ArrayKind == y.ArrayKind
            && String.Equals( x.TypeName, y.TypeName )
            && String.Equals( x.AssemblyName, y.AssemblyName )
            && x.Rank == y.Rank
            && ListEqualityComparer<List<Int32>, Int32>.ListEquality( x.Lengths, y.Lengths )
            && ListEqualityComparer<List<Int32>, Int32>.ListEquality( x.LowerBounds, y.LowerBounds )
            && ListEqualityComparer<List<Object>, Object>.ListEquality( x.ValuesAsVector, y.ValuesAsVector, Equality_ClassRecordMemberValue )
            );
      }

      private static Boolean Equality_ClassRecordMember( ClassRecordMember x, ClassRecordMember y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && String.Equals( x.TypeName, y.TypeName )
            && String.Equals( x.AssemblyName, y.AssemblyName )
            && Equality_ClassRecordMemberValue( x.Value, y.Value )
            );
      }

      private static Boolean Equality_ClassRecordMemberValue( Object x, Object y )
      {

         var retVal = ReferenceEquals( x, y );
         if ( !retVal )
         {
            var xRecord = x as AbstractRecord;
            var yRecord = y as AbstractRecord;
            retVal = Equality_AbstractRecord( xRecord, yRecord )
               || ( xRecord == null && yRecord == null && Object.Equals( x, y ) );
         }

         return retVal;
      }

      private static Int32 HashCode_ResourceManagerEntry( ResourceManagerEntry x )
      {
         if ( x == null )
         {
            return 0;
         }
         else
         {
            switch ( x.ResourceManagerEntryKind )
            {
               case ResourceManagerEntryKind.PreDefined:
                  return HashCode_ResourceManagerEntry_PreDefined( x as PreDefinedResourceManagerEntry );
               case ResourceManagerEntryKind.UserDefined:
                  return HashCode_ResourceManagerEntry_UserDefined( x as UserDefinedResourceManagerEntry );
               default:
                  return 0;
            }
         }
      }

      private static Int32 HashCode_ResourceManagerEntry_PreDefined( PreDefinedResourceManagerEntry x )
      {
         return x == null ? 0 : x.Value.GetHashCodeSafe( 1 );
      }

      private static Int32 HashCode_ResourceManagerEntry_UserDefined( UserDefinedResourceManagerEntry x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.UserDefinedType.GetHashCodeSafe( 1 ) ) * 23 + ListEqualityComparer<List<AbstractRecord>, AbstractRecord>.ListHashCode( x.Contents, HashCode_AbstractRecord ) );
      }

      private static Int32 HashCode_AbstractRecord( AbstractRecord x )
      {
         if ( x == null )
         {
            return 0;
         }
         else
         {
            switch ( x.RecordKind )
            {
               case RecordKind.Class:
                  return HashCode_ClassRecord( x as ClassRecord );
               case RecordKind.Array:
                  return HashCode_ArrayRecord( x as ArrayRecord );
               default:
                  return 0;
            }
         }
      }

      private static Int32 HashCode_ClassRecord( ClassRecord x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.TypeName.GetHashCodeSafe( 1 ) ) * 23 + ListEqualityComparer<List<ClassRecordMember>, ClassRecordMember>.ListHashCode( x.Members, HashCode_ClassRecordMember ) );
      }

      private static Int32 HashCode_ArrayRecord( ArrayRecord x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.TypeName.GetHashCodeSafe() ) * 23 + ListEqualityComparer<List<Object>, Object>.ListHashCode( x.ValuesAsVector, HashCode_ClassRecordMemberValue ) );
      }

      private static Int32 HashCode_ClassRecordMember( ClassRecordMember x )
      {
         return x == null ? 0 : ( ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + x.TypeName.GetHashCodeSafe( 2 ) ) * 23 + HashCode_ClassRecordMemberValue( x.Value ) );
      }

      private static Int32 HashCode_ClassRecordMemberValue( Object x )
      {
         AbstractRecord xRecord;
         return x == null ? 0 : (
            ( xRecord = x as AbstractRecord ) == null ?
               x.GetHashCode() :
               HashCode_AbstractRecord_NoRecursion( xRecord )
            );
      }

      private static Int32 HashCode_AbstractRecord_NoRecursion( AbstractRecord x )
      {
         if ( x == null )
         {
            return 0;
         }
         else
         {
            switch ( x.RecordKind )
            {
               case RecordKind.Class:
                  return HashCode_ClassRecord_NoRecursion( x as ClassRecord );
               case RecordKind.Array:
                  return HashCode_ArrayRecord_NoRecursion( x as ArrayRecord );
               default:
                  return 0;
            }
         }
      }

      private static Int32 HashCode_ClassRecord_NoRecursion( ClassRecord x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.TypeName.GetHashCodeSafe( 1 ) ) * 23 + ListEqualityComparer<List<ClassRecordMember>, ClassRecordMember>.ListHashCode( x.Members, HashCode_ClassRecordMember_NoRecursion ) );
      }

      private static Int32 HashCode_ArrayRecord_NoRecursion( ArrayRecord x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.TypeName.GetHashCodeSafe() ) * 23 + x.ValuesAsVector.Count * 37 );
      }

      private static Int32 HashCode_ClassRecordMember_NoRecursion( ClassRecordMember x )
      {
         return x == null ? 0 : ( ( ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) ) * 23 + x.TypeName.GetHashCodeSafe( 2 ) ) * 23 );
      }

      private static Int32 HashCode_ClassRecordMemberValue_NoRecursion( Object x )
      {
         AbstractRecord xRecord;
         return x == null ? 0 : (
            ( xRecord = x as AbstractRecord ) == null ?
               x.GetHashCode() :
               HashCode_AbstractRecord_NoRecursion( xRecord )
            );
      }
   }
}
