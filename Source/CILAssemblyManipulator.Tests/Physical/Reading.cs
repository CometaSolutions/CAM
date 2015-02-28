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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CILAssemblyManipulator.Physical;
using NUnit.Framework;
using System.IO;

namespace CILAssemblyManipulator.Tests.Physical
{
   public class ReadingTest
   {

      [Test]
      public void TestReadingCAMAssemblies()
      {
         TestReading( typeof( CILModuleIO ).Assembly );
      }

      [Test]
      public void TestReadingMSCorLib()
      {
         TestReading( typeof( Object ).Assembly, md =>
         {
            foreach ( var ca in md.CustomAttributeDefinitions )
            {
               var sig = ca.Signature;
               Assert.IsNotNull( sig );
               Assert.IsNotInstanceOf<RawCustomAttributeSignature>( sig );
            }
         } );
         // TODO: check that all custom attribute sigs are resolved
         // check that all security declarations have non-null custom attribute named args
      }

      private void TestReading( System.Reflection.Assembly assembly, Action<CILMetaData> validationAction = null )
      {
         using ( var fs = File.OpenRead( new Uri( assembly.CodeBase ).LocalPath ) )
         {
            var lArgs = new ModuleLoadingArguments();

            var thisMD = CILModuleIO.ReadModule( lArgs, fs );

            if ( validationAction != null )
            {
               validationAction( thisMD );
            }
         }
      }
   }
}
