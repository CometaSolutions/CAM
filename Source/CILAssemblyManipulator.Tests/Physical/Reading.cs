﻿/*
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
   public class ReadingTest : AbstractCAMTest
   {

      //[Test]
      public void TestReadingCAMAssemblies()
      {
         TestReading( typeof( CILModuleIO ).Assembly );
      }

      //[Test]
      public void TestReadingMSCorLib()
      {
         TestReading( MSCorLib, md =>
         {
            for ( var i = 0; i < md.CustomAttributeDefinitions.Count; ++i )
            {
               var ca = md.CustomAttributeDefinitions[i];
               var sig = ca.Signature;
               Assert.IsNotNull( sig );
               Assert.IsNotInstanceOf<RawCustomAttributeSignature>( sig );
            }

            for ( var i = 0; i < md.SecurityDefinitions.Count; ++i )
            {
               var sec = md.SecurityDefinitions[i];
               foreach ( var permission in sec.PermissionSets )
               {
                  Assert.IsNotNull( permission );
                  Assert.IsNotInstanceOf<RawSecurityInformation>( permission );
                  foreach ( var arg in ( (SecurityInformation) permission ).NamedArguments )
                  {
                     Assert.IsNotNull( arg );
                  }
               }
            }
         } );
         // TODO: check that all custom attribute sigs are resolved
         // check that all security declarations have non-null custom attribute named args
      }

      private void TestReading( System.Reflection.Assembly assembly, Action<CILMetaData> validationAction = null )
      {
         var resolver = new MetaDataResolver();
         using ( var fs = File.OpenRead( new Uri( assembly.CodeBase ).LocalPath ) )
         {
            var md = CILModuleIO.ReadModule( fs );

            resolver.ResolveEverything( md.MetaData );

            if ( validationAction != null )
            {
               validationAction( md.MetaData );
            }
         }
      }
   }
}
