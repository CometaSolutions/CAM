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
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Tests.UtilPack
{
   [Category( "UtilPack" )]
   public class Collections
   {
      [Test]
      public void TestArrayFill()
      {
         const Int32 VALUE = 6;

         for ( var i = 1; i < UInt16.MaxValue; ++i )
         {
            var array = new Int32[i];
            array.Fill( VALUE );
            Assert.IsTrue( array.All( v => v == VALUE ) );
         }

      }
   }
}
