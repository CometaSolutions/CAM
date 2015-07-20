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
using CILAssemblyManipulator.Physical;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
   public class TargetFrameworkMapper
   {
      private readonly IDictionary<CILMetaData, IDictionary<String, Boolean>> _mdTopLevelTypes;

      public TargetFrameworkMapper()
      {
         this._mdTopLevelTypes = new Dictionary<CILMetaData, IDictionary<String, Boolean>>();
      }

      private Boolean IsTopLevelTypePresent( CILMetaData md, String ns, String tn )
      {
         return this.IsTypePresent( md, Miscellaneous.CombineNamespaceAndType( ns, tn ) );
      }

      private Boolean IsTypePresent( CILMetaData md, String typeName )
      {
         throw new NotImplementedException();
      }

      internal CILMetaData GetActualMDForTopLevelType( CILMetaData md, CILMetaDataLoaderWithCallbacks loader, String ns, String tn, TargetFrameworkInfo newTargetFW )
      {
         var retVal = md;
         if ( !this.IsTopLevelTypePresent( md, ns, tn ) )
         {
            // The type is in other assembly, have to check which assembly *does* have the type
            // Walk all assemblies in target fw dir, and check whether we have the type...
            var cb = loader.LoaderCallbacks;
            foreach ( var res in cb.GetAssemblyResourcesForFramework( newTargetFW ) )
            {

            }
         }
         return retVal;
      }
   }
}

public static partial class E_CILPhysical
{
   public static void ChangeTargetFramework( this TargetFrameworkMapper mapper, CILMetaData md, CILMetaDataLoaderWithCallbacks loader, TargetFrameworkInfo newTargetFW )
   {
      var cb = loader.LoaderCallbacks;
      var newTargetFWPath = cb.GetTargetFrameworkPathForFrameworkInfo( newTargetFW );
      var aRefs = md.AssemblyReferences.TableContents;

      var aRefPaths = new Dictionary<AssemblyReference, String>( ReferenceEqualityComparer<AssemblyReference>.ReferenceBasedComparer );

      // First, type refs
      foreach ( var tRef in md.TypeReferences.TableContents.Where( tr => tr.ResolutionScope.HasValue && tr.ResolutionScope.Value.Table == Tables.AssemblyRef ) )
      {
         var aRef = aRefs[tRef.ResolutionScope.Value.Index];
         var targetFWAssemblyPath = aRefPaths.GetOrAdd_NotThreadSafe(
            aRef,
            aReff => cb.GetPossibleResourcesForAssemblyReference( loader.GetResourceFor( md ), md, new AssemblyInformationForResolving( aReff.AssemblyInformation, aReff.Attributes.IsFullPublicKey() ), null )
               .Where( res => cb.IsValidResource( res ) )
               .FirstOrDefault( res => res.StartsWith( newTargetFWPath ) )
            );
         if ( !String.IsNullOrEmpty( targetFWAssemblyPath ) )
         {
            var targetFWAssembly = loader.GetOrLoadMetaData( targetFWAssemblyPath );
            var actualTargetFWAssembly = mapper.GetActualMDForTopLevelType( targetFWAssembly, loader, tRef.Namespace, tRef.Name, newTargetFW );
            if ( !ReferenceEquals( targetFWAssembly, actualTargetFWAssembly ) )
            {
               // Type was in another assembly
            }


         }
      }

      // Then, all type strings (sec blobs, custom attrs)
   }
}
