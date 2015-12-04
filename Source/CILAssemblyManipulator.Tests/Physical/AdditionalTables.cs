using CILAssemblyManipulator.Physical;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Tests.Physical
{
   [Category( "CAM.Physical" )]
   public class AdditionalTablesTest : AbstractCAMTest
   {
      [Test]
      public void TestBorderLineCases()
      {
         var md = CILMetaDataFactory.NewBlankMetaData();

         Assert.IsNull( md.GetAdditionalTable( (Int32) Tables.GenericParameterConstraint + 1 ) );
         Assert.IsNull( md.GetAdditionalTable( (Int32) Byte.MaxValue ) );
      }

      [Test]
      public void TestGettingTables()
      {
         var md = CILMetaDataFactory.NewBlankMetaData();
         var tablesReturned = md.GetAllTables().ToArray();
         var tablesOrdered = tablesReturned.OrderBy( t => (Int32) t.TableKind ).ToArray();
         Assert.IsTrue( tablesReturned.SequenceEqual( tablesOrdered ) );
         Assert.AreEqual( (Int32) Tables.GenericParameterConstraint + 1, tablesReturned.Length );
      }
   }
}
