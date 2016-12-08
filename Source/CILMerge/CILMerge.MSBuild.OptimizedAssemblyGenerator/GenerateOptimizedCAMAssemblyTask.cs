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
extern alias CAMPhysicalIO;
extern alias CAMPhysicalIOD;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CILAssemblyManipulator.Physical;
using CAMPhysicalIOD::CILAssemblyManipulator.Physical;
using CAMPhysicalIO::CILAssemblyManipulator.Physical.IO;
using System.IO;

namespace CILMerge.MSBuild.OptimizedAssemblyGenerator
{
   public class GenerateOptimizedCAMAssemblyTask : Task
   {
      public override Boolean Execute()
      {
         var ocProvider = CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.DefaultOpCodeProvider.DefaultInstance;
         var sigProvider = CILAssemblyManipulator.Physical.Meta.DefaultSignatureProvider.DefaultInstance;

         // Create optimized versions of providers
         var fullPath = this.OutputFile;
         var assemblyName = Path.GetFileNameWithoutExtension( fullPath );
         var md = CILMetaDataFactory.CreateMinimalAssembly( assemblyName, null );
         var helper = new CAMPhysicalIO::CILAssemblyManipulator.Physical.EmittingNativeHelper( md );
         var optimizedIndex = ( (CAMPhysicalIO::CILAssemblyManipulator.Physical.Meta.OpCodeProvider) ocProvider ).CreateOptimizedOpCodeProvider( helper );

         // Add custom attribute to assembly that signals the presence of optimized type
         var fullNames = md.GetTypeDefinitionsFullNames().ToArray();
         var caSig = new ResolvedCustomAttributeSignature( 1, 0 );
         caSig.TypedArguments.Add( new CustomAttributeTypedArgument()
         {
            Value = new CustomAttributeValue_TypeReference( fullNames[optimizedIndex] )
         } );
         md.CustomAttributeDefinitions.AddRow( new CustomAttributeDefinition()
         {
            Parent = new TableIndex( Tables.Assembly, 0 ),
            Type = helper.GetMemberRefOrSpec( typeof( OptimizedOpCodeProviderTypeAttribute ).GetConstructor( new Type[] { typeof( Type ) } ) ),
            Signature = caSig
         } );

         using ( var stream = File.Open( fullPath, FileMode.Create, FileAccess.Write, FileShare.Read ) )
         {
            md.WriteModule( stream, new WritingArguments() );
         }

         return true;
      }

      [Required]
      public String OutputFile { get; set; }
   }
}
