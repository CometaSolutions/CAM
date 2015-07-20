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
      private readonly IDictionary<CILMetaData, ISet<String>> _mdTopLevelTypes;
      private readonly IDictionary<TargetFrameworkInfo, String[]> _targetFWAssemblies;
      private readonly IDictionary<Tuple<CILMetaData, String>, CILMetaData> _resolvedTargetFWAssemblies;

      public TargetFrameworkMapper()
      {
         this._mdTopLevelTypes = new Dictionary<CILMetaData, ISet<String>>();
         this._targetFWAssemblies = new Dictionary<TargetFrameworkInfo, String[]>();
      }

      internal CILMetaData GetActualMDForTopLevelType( CILMetaData targetFWAssembly, CILMetaDataLoaderWithCallbacks loader, String ns, String tn, TargetFrameworkInfo newTargetFW )
      {
         var fullType = Miscellaneous.CombineNamespaceAndType( ns, tn );
         return this._resolvedTargetFWAssemblies.GetOrAdd_NotThreadSafe(
            Tuple.Create( targetFWAssembly, fullType ),
            tuple => this.GetSuitableMDsForTopLevelType( tuple.Item1, loader, newTargetFW )
               .FirstOrDefault( md => this.IsTypePresent( md, tuple.Item2 ) )
            );
      }

      internal Boolean ProcessTypeString( ref String typeString )
      {
         String typeName, assemblyName;
         if ( typeString.ParseAssemblyQualifiedTypeString( out typeName, out assemblyName ) )
         {

         }
      }

      private Boolean IsTypePresent( CILMetaData metaData, String typeName )
      {
         return this._mdTopLevelTypes.GetOrAdd_NotThreadSafe( metaData, md =>
         {
            throw new NotImplementedException();
         } )
         .Contains( typeName );
      }

      private IEnumerable<CILMetaData> GetSuitableMDsForTopLevelType( CILMetaData md, CILMetaDataLoaderWithCallbacks loader, TargetFrameworkInfo targetFW )
      {
         // Always try current library at first
         yield return md;

         // Then start enumerating all the rest of the assemblies in target framework directory
         foreach ( var res in this._targetFWAssemblies.GetOrAdd_NotThreadSafe( targetFW, tfw => loader.LoaderCallbacks.GetAssemblyResourcesForFramework( tfw ).ToArray() ) )
         {
            var current = loader.GetOrLoadMetaData( res );
            if ( !ReferenceEquals( md, current ) )
            {
               yield return current;
            }
         }
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
      var aRefDic = new Dictionary<AssemblyReference, Int32>( Comparers.AssemblyReferenceEqualityComparer );
      // TODO .ToDictionary_Overwrite and .ToDictionary_Preserve extension methods to UtilPack
      for ( var i = 0; i < aRefs.Count; ++i )
      {
         aRefDic[aRefs[i]] = i;
      }

      // First, type refs
      foreach ( var tRef in md.TypeReferences.TableContents.Where( tr => tr.ResolutionScope.HasValue && tr.ResolutionScope.Value.Table == Tables.AssemblyRef ) )
      {
         var aRefIdx = tRef.ResolutionScope.Value;
         var aRef = aRefs[aRefIdx.Index];
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

            if ( actualTargetFWAssembly == null )
            {
               throw new InvalidOperationException( "Failed to map type " + Miscellaneous.CombineNamespaceAndType( tRef.Namespace, tRef.Name ) + " in " + targetFWAssemblyPath + " to target framework " + newTargetFW + "." );
            }
            else if ( !ReferenceEquals( targetFWAssembly, actualTargetFWAssembly ) )
            {
               // Type was in another assembly
               var newARef = actualTargetFWAssembly.AssemblyDefinitions.TableContents[0].AsAssemblyReference();
               Int32 aRefNewIdx;
               if ( !aRefDic.TryGetValue( newARef, out aRefNewIdx ) )
               {
                  aRefNewIdx = aRefs.Count;
                  aRefs.Add( newARef );
               }

               tRef.ResolutionScope = aRefIdx.ChangeIndex( aRefNewIdx );
            }
         }
      }

      // Then, all type strings (sec blobs, custom attrs, marshal infos)

   }
}
