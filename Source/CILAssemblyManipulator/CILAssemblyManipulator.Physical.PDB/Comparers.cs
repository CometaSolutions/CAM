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
using CILAssemblyManipulator.Physical.PDB;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
#pragma warning disable 1591
   public static class Comparers
#pragma warning restore 1591
   {
      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="PDBInstance"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="PDBInstance"/> are equal.</value>
      /// <remarks>
      /// Two instances of <see cref="PDBInstance"/> are considered to be equal by this equality when:
      /// <list type="bullet">
      /// <item><description><see cref="PDBInstance.Age"/> are equal,</description></item>
      /// <item><description><see cref="PDBInstance.DebugGUID"/> are equal,</description></item>
      /// <item><description><see cref="PDBInstance.SourceServer"/> are equal, and</description></item>
      /// <item><description><see cref="PDBInstance.Modules"/> match exactly using <see cref="PDBModuleEqualityComparer"/>.</description></item>
      /// </list>
      /// </remarks>
      public static IEqualityComparer<PDBInstance> PDBInstanceEqualityComparer { get; }

      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="PDBModule"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="PDBModule"/> are equal.</value>
      /// <remarks>
      /// Two instances of <see cref="PDBModule"/> are considered to be equal by this equality when:
      /// <list type="bullet">
      /// <item><description><see cref="PDBModule.Name"/> are equal, and</description></item>
      /// <item><description><see cref="PDBModule.Functions"/> match exactly using <see cref="PDBFunctionEqualityComparer"/>.</description></item>
      /// </list>
      /// </remarks>
      public static IEqualityComparer<PDBModule> PDBModuleEqualityComparer { get; }

      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="PDBFunction"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="PDBFunction"/> are equal.</value>
      /// <remarks>
      /// Two instances of <see cref="PDBFunction"/> are considered to be equal by this equality when:
      /// <list type="bullet">
      /// <item><description>common properties of <see cref="PDBScopeOrFunction"/> match using <see cref="PDBScopeOrFunctionEqualityComparer"/>,</description></item>
      /// <item><description><see cref="PDBFunction.Token"/> are equal,</description></item>
      /// <item><description><see cref="PDBFunction.ENCID"/> are equal,</description></item>
      /// <item><description><see cref="PDBFunction.ForwardingMethodToken"/> are equal,</description></item>
      /// <item><description><see cref="PDBFunction.ModuleForwardingMethodToken"/> are equal,</description></item>
      /// <item><description><see cref="PDBFunction.IteratorClass"/> are equal,</description></item>
      /// <item><description><see cref="PDBFunction.LocalScopes"/> match exactly using <see cref="PDBLocalScopeEqualityComparer"/></description></item>
      /// <item><description><see cref="PDBFunction.AsyncMethodInfo"/> match exactly using <see cref="PDBAsyncMethodInfoEqualityComparer"/>, and</description></item>
      /// <item><description><see cref="PDBFunction.Lines"/> match exactly using <see cref="PDBLineEqualityComparer"/>.</description></item>
      /// </list>
      /// </remarks>
      public static IEqualityComparer<PDBFunction> PDBFunctionEqualityComparer { get; }

      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="PDBScope"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="PDBScope"/> are equal.</value>
      /// <remarks>
      /// Two instances of <see cref="PDBScope"/> are considered to be equal by this equality when:
      /// <list type="bullet">
      /// <item><description>common properties of <see cref="PDBScopeOrFunction"/> match using <see cref="PDBScopeOrFunctionEqualityComparer"/>, and</description></item>
      /// <item><description><see cref="PDBScope.Offset"/> are equal.</description></item>
      /// </list>
      /// </remarks>
      public static IEqualityComparer<PDBScope> PDBScopeEqualityComparer { get; }

      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="PDBScopeOrFunction"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="PDBScopeOrFunction"/> are equal.</value>
      /// <remarks>
      /// Two instances of <see cref="PDBScopeOrFunction"/> are considered to be equal by this equality when:
      /// <list type="bullet">
      /// <item><description><see cref="PDBScopeOrFunction.Length"/> are equal,</description></item>
      /// <item><description><see cref="PDBScopeOrFunction.Name"/> are equal,</description></item>
      /// <item><description><see cref="PDBScopeOrFunction.UsedNamespaces"/> match exactly using <see cref="String.Equals(String, String)"/>,</description></item>
      /// <item><description><see cref="PDBScopeOrFunction.Slots"/> match exactly using <see cref="PDBSlotEqualityComparer"/>,</description></item>
      /// <item><description><see cref="PDBScopeOrFunction.Scopes"/> match exactly using <see cref="PDBScopeEqualityComparer"/>, and</description></item>
      /// <item><description><see cref="PDBScopeOrFunction.Constants"/> match exactly using <see cref="PDBConstantEqualityComparer"/>.</description></item>
      /// </list>
      /// </remarks>
      public static IEqualityComparer<PDBScopeOrFunction> PDBScopeOrFunctionEqualityComparer { get; }

      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="PDBSlot"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="PDBSlot"/> are equal.</value>
      /// <remarks>
      /// Two instances of <see cref="PDBSlot"/> are considered to be equal by this equality when:
      /// <list type="bullet">
      /// <item><description><see cref="PDBSlot.SlotIndex"/> are equal,</description></item>
      /// <item><description><see cref="PDBSlot.TypeToken"/> are equal,</description></item>
      /// <item><description><see cref="PDBSlot.Name"/> are equal, and</description></item>
      /// <item><description><see cref="PDBSlot.Flags"/> are equal.</description></item>
      /// </list>
      /// </remarks>
      public static IEqualityComparer<PDBSlot> PDBSlotEqualityComparer { get; }

      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="PDBLocalScope"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="PDBLocalScope"/> are equal.</value>
      /// <remarks>
      /// Two instances of <see cref="PDBLocalScope"/> are considered to be equal by this equality when:
      /// <list type="bullet">
      /// <item><description><see cref="PDBLocalScope.Offset"/> are equal, and</description></item>
      /// <item><description><see cref="PDBLocalScope.Length"/> are equal.</description></item>
      /// </list>
      /// </remarks>
      public static IEqualityComparer<PDBLocalScope> PDBLocalScopeEqualityComparer { get; }

      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="PDBAsyncMethodInfo"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="PDBAsyncMethodInfo"/> are equal.</value>
      /// <remarks>
      /// Two instances of <see cref="PDBAsyncMethodInfo"/> are considered to be equal by this equality when:
      /// <list type="bullet">
      /// <item><description><see cref="PDBAsyncMethodInfo.KickoffMethodToken"/> are equal,</description></item>
      /// <item><description><see cref="PDBAsyncMethodInfo.CatchHandlerOffset"/> are equal, and</description></item>
      /// <item><description><see cref="PDBAsyncMethodInfo.SynchronizationPoints"/> match exactly using <see cref="PDBSynchronizationPointEqualityComparer"/>.</description></item>
      /// </list>
      /// </remarks>
      public static IEqualityComparer<PDBAsyncMethodInfo> PDBAsyncMethodInfoEqualityComparer { get; }

      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="PDBSynchronizationPoint"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="PDBSynchronizationPoint"/> are equal.</value>
      /// <remarks>
      /// Two instances of <see cref="PDBSynchronizationPoint"/> are considered to be equal by this equality when:
      /// <list type="bullet">
      /// <item><description><see cref="PDBSynchronizationPoint.ContinuationMethodToken"/> are equal,</description></item>
      /// <item><description><see cref="PDBSynchronizationPoint.ContinuationOffset"/> are equal, and</description></item>
      /// <item><description><see cref="PDBSynchronizationPoint.SyncOffset"/> are equal.</description></item>
      /// </list>
      /// </remarks>
      public static IEqualityComparer<PDBSynchronizationPoint> PDBSynchronizationPointEqualityComparer { get; }

      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="PDBLine"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="PDBLine"/> are equal.</value>
      /// <remarks>
      /// Two instances of <see cref="PDBLine"/> are considered to be equal by this equality when:
      /// <list type="bullet">
      /// <item><description><see cref="PDBLine.Offset"/> are equal,</description></item>
      /// <item><description><see cref="PDBLine.LineStart"/> are equal,</description></item>
      /// <item><description><see cref="PDBLine.LineEnd"/> are equal,</description></item>
      /// <item><description><see cref="PDBLine.Source"/> match using <see cref="PDBSourceEqualityComparer"/>,</description></item>
      /// <item><description><see cref="PDBLine.ColumnStart"/> are equal,</description></item>
      /// <item><description><see cref="PDBLine.ColumnEnd"/> are equal, and</description></item>
      /// <item><description><see cref="PDBLine.IsStatement"/> are equal.</description></item>
      /// </list>
      /// </remarks>
      public static IEqualityComparer<PDBLine> PDBLineEqualityComparer { get; }

      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="PDBSource"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two instances of <see cref="PDBSource"/> are equal.</value>
      /// <remarks>
      /// Two instances of <see cref="PDBSource"/> are considered to be equal by this equality when:
      /// <list type="bullet">
      /// <item><description><see cref="PDBSource.Name"/> are equal,</description></item>
      /// <item><description><see cref="PDBSource.DocumentType"/> are equal,</description></item>
      /// <item><description><see cref="PDBSource.Language"/> are equal,</description></item>
      /// <item><description><see cref="PDBSource.HashAlgorithm"/> are equal,</description></item>
      /// <item><description><see cref="PDBSource.Vendor"/> are equal, and</description></item>
      /// <item><description><see cref="PDBSource.Hash"/> match exactly.</description></item>
      /// </list>
      /// </remarks>
      public static IEqualityComparer<PDBSource> PDBSourceEqualityComparer { get; }

      /// <summary>
      /// Gets the equality comparer to check whether two instances of <see cref="PDBConstant"/> are equal.
      /// </summary>
      /// <value>The equality compare to check whether two instances of <see cref="PDBConstant"/> are equal.</value>
      /// <remarks>
      /// Two instances of <see cref="PDBConstant"/> are considered to be equaly by this equality comparer when:
      /// <list type="bullet">
      /// <item><description><see cref="PDBConstant.Token"/> are equal,</description></item>
      /// <item><description><see cref="PDBConstant.Name"/> are equal, and</description></item>
      /// <item><description><see cref="PDBConstant.Value"/>s are equal using <see cref="PDBConstantValueEqualityComparer"/>.</description></item>
      /// </list>
      /// </remarks>
      public static IEqualityComparer<PDBConstant> PDBConstantEqualityComparer { get; }

      /// <summary>
      /// Gets the equality comparer to check whether two values of <see cref="PDBConstant.Value"/> are equal.
      /// </summary>
      /// <value>The equality comparer to check whether two values of <see cref="PDBConstant.Value"/> are equal.</value>
      /// <remarks>
      /// The values are considered to be equal when:
      /// <list type="bullet">
      /// <item><description>both values are numerical values and equal ignoring their type, or</description></item>
      /// <item><description>both values are strings and are equal with <see cref="string.Equals(string, string)"/> method, or</description></item>
      /// <item><description>both values are <c>null</c>.</description></item>
      /// </list>
      /// </remarks>
      public static IEqualityComparer<Object> PDBConstantValueEqualityComparer { get; }

      static Comparers()
      {
         PDBInstanceEqualityComparer = ComparerFromFunctions.NewEqualityComparer<PDBInstance>( Equality_PDBInstance, HashCode_PDBInstance );
         PDBModuleEqualityComparer = ComparerFromFunctions.NewEqualityComparer<PDBModule>( Equality_PDBModule, HashCode_PDBModule );
         PDBFunctionEqualityComparer = ComparerFromFunctions.NewEqualityComparer<PDBFunction>( Equality_PDBFunction, HashCode_PDBFunction );
         PDBScopeEqualityComparer = ComparerFromFunctions.NewEqualityComparer<PDBScope>( Equality_PDBScope, HashCode_PDBScope );
         PDBScopeOrFunctionEqualityComparer = ComparerFromFunctions.NewEqualityComparer<PDBScopeOrFunction>( Equality_PDBScopeOrFunction, HashCode_PDBScopeOrFunction );
         PDBSlotEqualityComparer = ComparerFromFunctions.NewEqualityComparer<PDBSlot>( Equality_PDBSlot, HashCode_PDBSlot );
         PDBLocalScopeEqualityComparer = ComparerFromFunctions.NewEqualityComparer<PDBLocalScope>( Equality_PDBLocalScope, HashCode_PDBLocalScope );
         PDBAsyncMethodInfoEqualityComparer = ComparerFromFunctions.NewEqualityComparer<PDBAsyncMethodInfo>( Equality_PDBAsyncMethodInfo, HashCode_PDBAsyncMethodInfo );
         PDBSynchronizationPointEqualityComparer = ComparerFromFunctions.NewEqualityComparer<PDBSynchronizationPoint>( Equality_PDBSynchronizationPoint, HashCode_PDBSynchronizationPoint );
         PDBLineEqualityComparer = ComparerFromFunctions.NewEqualityComparer<PDBLine>( Equality_PDBLine, HashCode_PDBLine );
         PDBSourceEqualityComparer = ComparerFromFunctions.NewEqualityComparer<PDBSource>( Equality_PDBSource, HashCode_PDBSource );
         PDBConstantEqualityComparer = ComparerFromFunctions.NewEqualityComparer<PDBConstant>( Equality_PDBConstant, HashCode_PDBConstant );
         PDBConstantValueEqualityComparer = ComparerFromFunctions.NewEqualityComparer<Object>( Equality_PDBConstantValue, HashCode_PDBConstantValue );
      }


      private static Boolean Equality_PDBInstance( PDBInstance x, PDBInstance y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Age == y.Age
            && x.Timestamp == y.Timestamp
            && x.DebugGUID == y.DebugGUID
            && String.Equals( x.SourceServer, y.SourceServer )
            && ListEqualityComparer<List<PDBModule>, PDBModule>.ListEquality( x.Modules, y.Modules, Equality_PDBModule )
            );
      }

      private static Boolean Equality_PDBModule( PDBModule x, PDBModule y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && ListEqualityComparer<List<PDBFunction>, PDBFunction>.ListEquality( x.Functions, y.Functions, Equality_PDBFunction )
            );
      }

      private static Boolean Equality_PDBFunction( PDBFunction x, PDBFunction y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && Equality_PDBScopeOrFunction_NoReferenceOrNullCheck( x, y )
            && x.Token == y.Token
            && x.ENCID == y.ENCID
            && x.ForwardingMethodToken == y.ForwardingMethodToken
            && x.ModuleForwardingMethodToken == y.ModuleForwardingMethodToken
            && String.Equals( x.IteratorClass, y.IteratorClass )
            && ListEqualityComparer<List<PDBLocalScope>, PDBLocalScope>.ListEquality( x.LocalScopes, y.LocalScopes, Equality_PDBLocalScope )
            && Equality_PDBAsyncMethodInfo( x.AsyncMethodInfo, y.AsyncMethodInfo )
            && ListEqualityComparer<List<PDBLine>, PDBLine>.ListEquality( x.Lines, y.Lines, Equality_PDBLine )
            );
      }

      private static Boolean Equality_PDBScope( PDBScope x, PDBScope y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && Equality_PDBScopeOrFunction_NoReferenceOrNullCheck( x, y )
            && x.Offset == y.Offset
            );
      }

      private static Boolean Equality_PDBScopeOrFunction( PDBScopeOrFunction x, PDBScopeOrFunction y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && Equality_PDBScopeOrFunction_NoReferenceOrNullCheck( x, y )
            );
      }

      private static Boolean Equality_PDBScopeOrFunction_NoReferenceOrNullCheck( PDBScopeOrFunction x, PDBScopeOrFunction y )
      {
         var retVal = x.Length == y.Length
            && String.Equals( x.Name, y.Name )
            && ListEqualityComparer<List<String>, String>.ListEquality( x.UsedNamespaces, y.UsedNamespaces )
            && ListEqualityComparer<List<PDBSlot>, PDBSlot>.ListEquality( x.Slots, y.Slots, Equality_PDBSlot )
            && ListEqualityComparer<List<PDBScope>, PDBScope>.ListEquality( x.Scopes, y.Scopes, Equality_PDBScope )
            && ListEqualityComparer<List<PDBConstant>, PDBConstant>.ListEquality( x.Constants, y.Constants, Equality_PDBConstant );
         if ( !retVal )
         {

         }

         return retVal;
      }

      private static Boolean Equality_PDBSlot( PDBSlot x, PDBSlot y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.SlotIndex == y.SlotIndex
            && x.TypeToken == y.TypeToken
            && String.Equals( x.Name, y.Name )
            && x.Flags == y.Flags
            );
      }

      private static Boolean Equality_PDBLocalScope( PDBLocalScope x, PDBLocalScope y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Offset == y.Offset
            && x.Length == y.Length
            );
      }

      private static Boolean Equality_PDBAsyncMethodInfo( PDBAsyncMethodInfo x, PDBAsyncMethodInfo y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.KickoffMethodToken == y.KickoffMethodToken
            && x.CatchHandlerOffset == y.CatchHandlerOffset
            && ListEqualityComparer<List<PDBSynchronizationPoint>, PDBSynchronizationPoint>.ListEquality( x.SynchronizationPoints, y.SynchronizationPoints, Equality_PDBSynchronizationPoint )
            );
      }

      private static Boolean Equality_PDBSynchronizationPoint( PDBSynchronizationPoint x, PDBSynchronizationPoint y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.ContinuationMethodToken == y.ContinuationMethodToken
            && x.ContinuationOffset == y.ContinuationOffset
            && x.SyncOffset == y.SyncOffset
            );
      }

      private static Boolean Equality_PDBLine( PDBLine x, PDBLine y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Offset == y.Offset
            && x.LineStart == y.LineStart
            && x.LineEnd == y.LineEnd
            && Equality_PDBSource( x.Source, y.Source )
            && x.ColumnStart == y.ColumnStart
            && x.ColumnEnd == y.ColumnEnd
            && x.IsStatement == y.IsStatement
            );
      }

      private static Boolean Equality_PDBSource( PDBSource x, PDBSource y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && String.Equals( x.Name, y.Name )
            && x.DocumentType == y.DocumentType
            && x.Language == y.Language
            && x.HashAlgorithm == y.HashAlgorithm
            && x.Vendor == y.Vendor
            && ArrayEqualityComparer<Byte>.ArrayEquality( x.Hash, y.Hash )
            );
      }

      private static Boolean Equality_PDBConstant( PDBConstant x, PDBConstant y )
      {
         var retVal = ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.Token == y.Token
            && String.Equals( x.Name, y.Name )
            && Equality_PDBConstantValue( x.Value, y.Value )
            );
         if ( !retVal )
         {

         }
         return retVal;
      }

      private static Boolean Equality_PDBConstantValue( Object x, Object y )
      {
         var retVal = ReferenceEquals( x, y );
         if ( !retVal && x != null && y != null )
         {
            // Anything that has same numerical value should be the same
            Object numX, numY;
            TypeCode tcX, tcY;
            if ( TryGetNumericalValue( x, out numX )
               && TryGetNumericalValue( x, out numY )
               && (
                  ( tcX = Type.GetTypeCode( numX.GetType() ) ) == ( tcY = Type.GetTypeCode( numY.GetType() ) )
                  || ( tcX == TypeCode.Int64 && tcY == TypeCode.UInt64 )
                  || ( tcX == TypeCode.UInt64 && tcY == TypeCode.Int64 )
                  )
               )
            {
               switch ( tcX )
               {
                  case TypeCode.Int64:
                     retVal = tcY == TypeCode.Int64 ? ( (Int64) numX == (Int64) numY ) : ( (Int64) numX == unchecked((Int64) (UInt64) numY) );
                     break;
                  case TypeCode.UInt64:
                     retVal = tcY == TypeCode.UInt64 ? ( (UInt64) numX == (UInt64) numY ) : ( (UInt64) numX == unchecked((UInt64) (Int64) numY) );
                     break;
                  case TypeCode.Single:
                     retVal = (Single) numX == (Single) numY;
                     break;
                  case TypeCode.Double:
                     retVal = (Double) numX == (Double) numY;
                     break;
                  case TypeCode.Decimal:
                     retVal = (Decimal) numX == (Decimal) numY;
                     break;
               }

            }
            else
            {
               retVal = String.Equals( x as String, y as String );
            }
         }
         return retVal;
      }

      // num will be either Int64, UInt64, Single, Double, or Decimal, if this returns true
      private static Boolean TryGetNumericalValue( Object x, out Object num )
      {
         switch ( Type.GetTypeCode( x.GetType() ) )
         {
            case TypeCode.Boolean:
               num = (Boolean) x ? 1UL : 0UL;
               break;
            case TypeCode.Char:
               num = (UInt64) (Char) x;
               break;
            case TypeCode.SByte:
               num = (Int64) (SByte) x;
               break;
            case TypeCode.Byte:
               num = (UInt64) (Byte) x;
               break;
            case TypeCode.Int16:
               num = (Int64) (Int16) x;
               break;
            case TypeCode.UInt16:
               num = (UInt64) (UInt16) x;
               break;
            case TypeCode.Int32:
               num = (Int64) (Int32) x;
               break;
            case TypeCode.UInt32:
               num = (UInt64) (UInt32) x;
               break;
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
               num = x;
               break;
            default:
               num = null;
               break;
         }

         return num != null;
      }

      private static Int32 HashCode_PDBInstance( PDBInstance x )
      {
         return x == null ? 0 : x.DebugGUID.GetHashCode();
      }

      private static Int32 HashCode_PDBModule( PDBModule x )
      {
         return x == null ? 0 : x.Name.GetHashCodeSafe( 1 );
      }

      private static Int32 HashCode_PDBFunction( PDBFunction x )
      {
         return x == null ? 0 : ( ( 17 * 23 + HashCode_PDBScopeOrFunction_NoNullCheck( x ) ) * 23 + (Int32) x.Token );
      }

      private static Int32 HashCode_PDBScopeOrFunction( PDBScopeOrFunction x )
      {
         return x == null ? 0 : HashCode_PDBScopeOrFunction_NoNullCheck( x );
      }

      private static Int32 HashCode_PDBScopeOrFunction_NoNullCheck( PDBScopeOrFunction x )
      {
         return ( 17 * 23 + x.Name.GetHashCodeSafe() ) * 23 + x.Length;
      }

      private static Int32 HashCode_PDBScope( PDBScope x )
      {
         return x == null ? 0 : ( ( 17 * 23 + HashCode_PDBScopeOrFunction_NoNullCheck( x ) ) * 23 + x.Offset );
      }

      private static Int32 HashCode_PDBSlot( PDBSlot x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Name.GetHashCodeSafe() ) * 23 + x.SlotIndex );
      }

      private static Int32 HashCode_PDBLocalScope( PDBLocalScope x )
      {
         return x == null ? 0 : ( ( 17 * 23 + x.Offset ) * 23 + x.Length );
      }

      private static Int32 HashCode_PDBAsyncMethodInfo( PDBAsyncMethodInfo x )
      {
         return x == null ? 0 : ( ( 17 * 23 + (Int32) x.KickoffMethodToken ) * 23 + x.CatchHandlerOffset );
      }

      private static Int32 HashCode_PDBSynchronizationPoint( PDBSynchronizationPoint x )
      {
         return x == null ? 0 : ( ( 17 * 23 + (Int32) x.ContinuationMethodToken ) * 23 + x.SyncOffset );
      }

      private static Int32 HashCode_PDBLine( PDBLine x )
      {
         return x == null ? 0 : ( ( ( 17 * 23 + x.Offset ) * 23 + x.LineStart ) * 23 + x.LineEnd );
      }

      private static Int32 HashCode_PDBSource( PDBSource x )
      {
         return x == null ? 0 : ( 17 * 23 + x.Name.GetHashCodeSafe( 1 ) );
      }

      private static Int32 HashCode_PDBConstant( PDBConstant x )
      {
         return x == null ? 0 : ( ( 17 * 23 + unchecked((Int32) x.Token) ) * 23 + HashCode_PDBConstantValue( x.Value ) );
      }

      private static Int32 HashCode_PDBConstantValue( Object x )
      {
         return x.GetHashCodeSafe( 1 );
      }
   }
}