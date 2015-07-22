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
      private readonly IDictionary<CILMetaData, ISet<String>> _mdTypes;
      private readonly IDictionary<TargetFrameworkInfo, String[]> _targetFWAssemblies;
      private readonly IDictionary<CILMetaData, IDictionary<String, CILMetaData>> _resolvedTargetFWAssemblies;
      private readonly IDictionary<CILMetaData, IDictionary<AssemblyInformationForResolving, CILMetaData>> _assemblyReferenceInfo;

      public TargetFrameworkMapper()
      {
         this._mdTypes = new Dictionary<CILMetaData, ISet<String>>();
         this._targetFWAssemblies = new Dictionary<TargetFrameworkInfo, String[]>();
         this._resolvedTargetFWAssemblies = new Dictionary<CILMetaData, IDictionary<String, CILMetaData>>();
         this._assemblyReferenceInfo = new Dictionary<CILMetaData, IDictionary<AssemblyInformationForResolving, CILMetaData>>();
      }

      internal Boolean TryReMapReference( CILMetaData thisMD, AssemblyInformationForResolving aRef, String fullType, CILMetaDataLoaderWithCallbacks loader, TargetFrameworkInfo targetFW, out AssemblyReference newRef )
      {
         newRef = null;

         var targetFWAssembly = this.ResolveTargetFWReferenceOrNull( thisMD, aRef, loader, targetFW );
         var retVal = targetFWAssembly != null;
         if ( retVal )
         {
            var actualTargetFWAssembly = this.GetActualMDForType( targetFWAssembly, loader, fullType, targetFW );

            if ( actualTargetFWAssembly == null )
            {
               throw new InvalidOperationException( "Failed to map type " + fullType + " in " + loader.GetResourceFor( targetFWAssembly ) + " to target framework " + targetFW + "." );
            }
            else
            {
               retVal = !ReferenceEquals( targetFWAssembly, actualTargetFWAssembly );
               if ( retVal )
               {
                  // Type was in another assembly
                  newRef = actualTargetFWAssembly.AssemblyDefinitions.TableContents[0].AsAssemblyReference();
               }
            }
         }

         return retVal;
      }

      internal Boolean ProcessTypeString( CILMetaData thisMD, CILMetaDataLoaderWithCallbacks loader, TargetFrameworkInfo targetFW, ref String typeString )
      {
         String typeName, assemblyName;
         AssemblyInformation assemblyInfo;
         Boolean isFullPublicKey;
         AssemblyReference newRef = null;
         var retVal = typeString.ParseAssemblyQualifiedTypeString( out typeName, out assemblyName )
            && AssemblyInformation.TryParse( assemblyName, out assemblyInfo, out isFullPublicKey )
            && this.TryReMapReference( thisMD, new AssemblyInformationForResolving( assemblyInfo, isFullPublicKey ), typeName, loader, targetFW, out newRef );

         if ( retVal )
         {
            assemblyName = newRef.ToString();
            typeString = Miscellaneous.CombineAssemblyAndType( assemblyName, typeName );
         }

         return retVal;
      }

      private CILMetaData GetActualMDForType( CILMetaData targetFWAssembly, CILMetaDataLoaderWithCallbacks loader, String fullType, TargetFrameworkInfo newTargetFW )
      {
         return this._resolvedTargetFWAssemblies
            .GetOrAdd_NotThreadSafe( targetFWAssembly, tfwa => new Dictionary<String, CILMetaData>() )
            .GetOrAdd_NotThreadSafe( fullType, typeStr =>
               this.GetSuitableMDsForTargetFW( targetFWAssembly, loader, newTargetFW )
                  .FirstOrDefault( md => this.IsTypePresent( md, typeStr ) )
            );
      }

      private CILMetaData ResolveTargetFWReferenceOrNull( CILMetaData thisMD, AssemblyInformationForResolving assemblyRef, CILMetaDataLoaderWithCallbacks loader, TargetFrameworkInfo targetFW )
      {
         return this._assemblyReferenceInfo
            .GetOrAdd_NotThreadSafe( thisMD, md => new Dictionary<AssemblyInformationForResolving, CILMetaData>() )
            .GetOrAdd_NotThreadSafe( assemblyRef, aRef =>
         {
            var cb = loader.LoaderCallbacks;
            var validResource = cb.GetPossibleResourcesForAssemblyReference( loader.GetResourceFor( thisMD ), thisMD, aRef, null )
               .Where( res => cb.IsValidResource( res ) )
               .FirstOrDefault();
            CILMetaData retVal;
            if ( validResource == null )
            {
               // Most likely this metadata didn't have target framework info attribute
               // TODO match public key too.
               retVal = this.GetTargetFWAssemblies( targetFW, loader )
                  .Select( res => loader.GetOrLoadMetaData( res ) )
                  .FirstOrDefault( md => md.AssemblyDefinitions.RowCount > 0 && String.Equals( md.AssemblyDefinitions.TableContents[0].AssemblyInformation.Name, aRef.AssemblyInformation.Name ) );
            }
            else if ( validResource.StartsWith( cb.GetTargetFrameworkPathForFrameworkInfo( targetFW ) ) ) // Check whether resolved reference is located in target framework path
            {
               retVal = loader.GetOrLoadMetaData( validResource );
            }
            else
            {
               retVal = null;
            }
            return retVal;
         } );
      }

      private Boolean IsTypePresent( CILMetaData metaData, String typeName )
      {
         return this._mdTypes
            .GetOrAdd_NotThreadSafe( metaData, md => new HashSet<String>( md.GetTypeDefinitionsFullNames() ) )
            .Contains( typeName );
      }

      private IEnumerable<CILMetaData> GetSuitableMDsForTargetFW( CILMetaData md, CILMetaDataLoaderWithCallbacks loader, TargetFrameworkInfo targetFW )
      {
         // Always try current library at first
         yield return md;

         // Then start enumerating all the rest of the assemblies in target framework directory
         foreach ( var res in this.GetTargetFWAssemblies( targetFW, loader ) )
         {
            var current = loader.GetOrLoadMetaData( res );
            if ( !ReferenceEquals( md, current ) )
            {
               yield return current;
            }
         }
      }

      private String[] GetTargetFWAssemblies( TargetFrameworkInfo targetFW, CILMetaDataLoaderWithCallbacks loader )
      {
         return this._targetFWAssemblies.GetOrAdd_NotThreadSafe( targetFW, tfw => loader.LoaderCallbacks.GetAssemblyResourcesForFramework( tfw ).ToArray() );
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

         AssemblyReference newRef;
         if ( mapper.TryReMapReference( md, new AssemblyInformationForResolving( aRef.AssemblyInformation, aRef.Attributes.IsFullPublicKey() ), Miscellaneous.CombineNamespaceAndType( tRef.Namespace, tRef.Name ), loader, newTargetFW, out newRef ) )
         {
            Int32 aRefNewIdx;
            if ( !aRefDic.TryGetValue( newRef, out aRefNewIdx ) )
            {
               aRefNewIdx = aRefs.Count;
               aRefs.Add( newRef );
            }

            tRef.ResolutionScope = aRefIdx.ChangeIndex( aRefNewIdx );
         }
      }

      // Then, all type strings (sec blobs, custom attrs, marshal infos)
      foreach ( var marshal in md.FieldMarshals.TableContents )
      {
         mapper.ProcessMarshalInfo( md, loader, newTargetFW, marshal.NativeType );
      }

      foreach ( var sec in md.SecurityDefinitions.TableContents )
      {
         foreach ( var permSet in sec.PermissionSets.OfType<SecurityInformation>() )
         {
            var typeStr = permSet.SecurityAttributeType;
            if ( mapper.ProcessTypeString( md, loader, newTargetFW, ref typeStr ) )
            {
               permSet.SecurityAttributeType = typeStr;
            }
            foreach ( var namedArg in permSet.NamedArguments )
            {
               mapper.ProcessCASignatureNamed( md, loader, newTargetFW, namedArg );
            }
         }
      }

      foreach ( var ca in md.CustomAttributeDefinitions.TableContents )
      {
         mapper.ProcessCASignature( md, loader, newTargetFW, ca.Signature );
      }
   }

   private static void ProcessMarshalInfo( this TargetFrameworkMapper mapper, CILMetaData md, CILMetaDataLoaderWithCallbacks loader, TargetFrameworkInfo newTargetFW, MarshalingInfo marshal )
   {
      if ( marshal != null )
      {
         var typeStr = marshal.SafeArrayUserDefinedType;
         if ( mapper.ProcessTypeString( md, loader, newTargetFW, ref typeStr ) )
         {
            marshal.SafeArrayUserDefinedType = typeStr;
         }
         typeStr = marshal.MarshalType;
         if ( mapper.ProcessTypeString( md, loader, newTargetFW, ref typeStr ) )
         {
            marshal.MarshalType = typeStr;
         }
      }
   }

   private static void ProcessCASignature( this TargetFrameworkMapper mapper, CILMetaData md, CILMetaDataLoaderWithCallbacks loader, TargetFrameworkInfo newTargetFW, AbstractCustomAttributeSignature sig )
   {
      if ( sig != null && sig.CustomAttributeSignatureKind == CustomAttributeSignatureKind.Resolved )
      {
         var sigg = (CustomAttributeSignature) sig;
         foreach ( var typed in sigg.TypedArguments )
         {
            mapper.ProcessCASignatureTyped( md, loader, newTargetFW, typed );
         }

         foreach ( var named in sigg.NamedArguments )
         {
            mapper.ProcessCASignatureNamed( md, loader, newTargetFW, named );
         }
      }
   }

   private static void ProcessCASignatureTyped( this TargetFrameworkMapper mapper, CILMetaData md, CILMetaDataLoaderWithCallbacks loader, TargetFrameworkInfo newTargetFW, CustomAttributeTypedArgument arg )
   {
      if ( arg != null )
      {
         var type = arg.Type;
         if ( type != null && type.ArgumentTypeKind == CustomAttributeArgumentTypeKind.TypeString )
         {
            var typeStrArg = (CustomAttributeArgumentTypeEnum) type;
            var typeString = typeStrArg.TypeString;
            if ( mapper.ProcessTypeString( md, loader, newTargetFW, ref typeString ) )
            {
               typeStrArg.TypeString = typeString;
            }
         }
      }
   }

   private static void ProcessCASignatureNamed( this TargetFrameworkMapper mapper, CILMetaData md, CILMetaDataLoaderWithCallbacks loader, TargetFrameworkInfo newTargetFW, CustomAttributeNamedArgument arg )
   {
      if ( arg != null )
      {
         mapper.ProcessCASignatureTyped( md, loader, newTargetFW, arg.Value );
      }
   }
}
