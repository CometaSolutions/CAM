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
   }
}
