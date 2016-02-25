/*
 * Copyright 2015 Stanislav Muhametsin. All rights Reserved.
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
extern alias CAMPhysicalIOD;
extern alias CAMPhysicalIO;

using CAMPhysicalIOD;
using CAMPhysicalIOD::CILAssemblyManipulator.Physical;
using CAMPhysicalIOD::CILAssemblyManipulator.Physical.Meta;
using CAMPhysicalIOD::CILAssemblyManipulator.Physical.IO;

using CAMPhysicalIO::CILAssemblyManipulator.Physical.IO;

using CILAssemblyManipulator.Physical;
using CommonUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TabularMetaData;
using TabularMetaData.Meta;

namespace CILAssemblyManipulator.Tests.Physical
{
   [Category( "CAM.Physical" )]
   public class AdditionalTablesTest : AbstractCAMTest
   {
      private const Int32 ADDITIONAL_TABLE = (Int32) ( Tables.GenericParameterConstraint + 2 );

      [Test]
      public void TestBorderLineCases()
      {
         var md = CILMetaDataFactory.NewBlankMetaData();
         MetaDataTable tbl;
         Assert.IsFalse( md.TryGetByTable( (Int32) Tables.GenericParameterConstraint + 1, out tbl ) );
         Assert.IsFalse( md.TryGetByTable( (Int32) Byte.MaxValue, out tbl ) );
      }

      [Test]
      public void TestGettingTables()
      {
         var md = CILMetaDataFactory.NewBlankMetaData();
         var tablesReturned = md.GetAllTables().ToArray();
         var tablesOrdered = tablesReturned.OrderBy( t => t.GetTableIndex() ).ToArray();
         Assert.IsTrue( tablesReturned.SequenceEqual( tablesOrdered ) );
         Assert.AreEqual( (Int32) Tables.GenericParameterConstraint + 1, tablesReturned.Length );
      }

      [Test]
      public void TestAdditionalTables()
      {
         var md = CreateMDWithAdditionalTables();
         var additionalTable = md.GetByTable( ADDITIONAL_TABLE );
         Assert.IsNotNull( additionalTable );

         var additionalTableTyped = (MetaDataTable<MyAdditionalTableRow>) additionalTable;
         additionalTableTyped.TableContents.Add( new MyAdditionalTableRow()
         {
            IntValue = 50,
            StringValue = "Testing"
         } );

         var info = additionalTableTyped.TableInformation;
         Assert.AreEqual( 2, info.ColumnsInformation.Count );
         //Assert.AreEqual( MetaDataColumnInformationKind.FixedSizeConstant, ( (MetaDataColumnInformationWithDataMeaning) info.ColumnsInformation[0] ).DataInformation.ColumnKind );
         //Assert.AreEqual( MetaDataColumnInformationKind.HeapIndex, ( (MetaDataColumnInformationWithDataMeaning) info.ColumnsInformation[1] ).DataInformation.ColumnKind );
      }

      [Test]
      public void TestAdditionalTablesSerialization()
      {
         var md = CreateMDWithAdditionalTables();
         var additionalTable = (MetaDataTable<MyAdditionalTableRow>) md.GetByTable( ADDITIONAL_TABLE );
         additionalTable.TableContents.Add( new MyAdditionalTableRow()
         {
            IntValue = 50,
            StringValue = "Testing"
         } );

         Byte[] serializedMD;
         using ( var stream = new MemoryStream() )
         {
            md.WriteModule( stream );

            serializedMD = stream.ToArray();
         }

         CILMetaData md2;
         using ( var stream = new MemoryStream( serializedMD ) )
         {
            md2 = stream.ReadModule( new ReadingArguments()
            {
               TableInformationProvider = CreateTableProvider()
            } );
         }

         var additionalTable2 = (MetaDataTable<MyAdditionalTableRow>) md2.GetByTable( ADDITIONAL_TABLE );

         Assert.IsTrue( Comparers.MetaDataEqualityComparer.Equals( md, md2 ) );
         Assert.IsTrue( ListEqualityComparer<List<MyAdditionalTableRow>, MyAdditionalTableRow>.ListEquality(
            additionalTable.TableContents,
            additionalTable2.TableContents,
            MyAdditionalTableRowEquals
            ) );

         additionalTable2.TableContents.Clear();
         Assert.IsFalse( Comparers.MetaDataEqualityComparer.Equals( md, md2 ) );

      }

      private static CILMetaData CreateMDWithAdditionalTables()
      {
         return CILMetaDataFactory.CreateMinimalAssembly(
            "Test_Assembly",
            null,
            createModuleType: true,
            tableInfoProvider: CreateTableProvider()
            );
      }

      private static DefaultMetaDataTableInformationProvider CreateTableProvider()
      {
         return DefaultMetaDataTableInformationProvider.CreateWithAdditionalTables( CreateAdditionalTableInfo() );
      }


      private static IEnumerable<MetaDataTableInformation> CreateAdditionalTableInfo()
      {
         yield return new MetaDataTableInformation<MyAdditionalTableRow, RawMyAdditionalTableRow>(
            (Tables) ADDITIONAL_TABLE,
            new MyAdditionalTableRowEqualityComparer(),
            null,
            () => new MyAdditionalTableRow(),
            CreateAdditionalTableColumnInfo(),
            () => new RawMyAdditionalTableRow(),
            false
            );
      }

      private static IEnumerable<MetaDataColumnInformation<MyAdditionalTableRow>> CreateAdditionalTableColumnInfo()
      {
         yield return MetaDataColumnInformationFactory.Number32<MyAdditionalTableRow, RawMyAdditionalTableRow>(
            row => row.IntValue,
            ( row, value ) => { row.IntValue = value; return true; },
            ( rawRow, value ) => rawRow.IntValue = value
            );

         yield return MetaDataColumnInformationFactory.SystemString<MyAdditionalTableRow, RawMyAdditionalTableRow>(
            row => row.StringValue,
            ( row, value ) => { row.StringValue = value; return true; },
            ( rawRow, value ) => rawRow.StringValue = value
            );
      }

      public class MyAdditionalTableRow
      {
         public Int32 IntValue { get; set; }

         public String StringValue { get; set; }
      }

      public class RawMyAdditionalTableRow
      {
         public Int32 IntValue { get; set; }

         public Int32 StringValue { get; set; }
      }

      public class MyAdditionalTableRowEqualityComparer : IEqualityComparer<MyAdditionalTableRow>
      {
         public Boolean Equals( MyAdditionalTableRow x, MyAdditionalTableRow y )
         {
            return MyAdditionalTableRowEquals( x, y );
         }

         public Int32 GetHashCode( MyAdditionalTableRow obj )
         {
            return obj == null ? 0 : ( ( 17 * 23 + obj.IntValue ) * 23 + obj.StringValue.GetHashCode() );
         }
      }

      private static Boolean MyAdditionalTableRowEquals( MyAdditionalTableRow x, MyAdditionalTableRow y )
      {
         return ReferenceEquals( x, y ) ||
            ( x != null && y != null
            && x.IntValue == y.IntValue
            && x.StringValue == y.StringValue
            );
      }
   }
}
