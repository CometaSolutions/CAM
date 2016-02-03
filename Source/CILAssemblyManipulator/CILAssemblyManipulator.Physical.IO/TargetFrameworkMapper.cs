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
extern alias CAMPhysical;
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.IO;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO
{
   /// <summary>
   /// This interface provides core methods required when mapping assembly references to possibly other target frameworks than assembly's current target framework.
   /// </summary>
   public interface TargetFrameworkMapper
   {
      /// <summary>
      /// Tries to map type referenced in the given <see cref="CILMetaData"/> into given target framework represented by <see cref="TargetFrameworkInfoWithRetargetabilityInformation"/>.
      /// </summary>
      /// <param name="thisMD">The <see cref="CILMetaData"/> containing the assembly reference.</param>
      /// <param name="aRef">The assembly reference, as <see cref="AssemblyInformationForResolving"/>.</param>
      /// <param name="fullType">The full type name (containing namespace, possible enclosing type names, and the type name) as string.</param>
      /// <param name="loader">The <see cref="CILMetaDataLoaderWithCallbacks"/> to use when performing on-demand loading of the assemblies in given target framework.</param>
      /// <param name="targetFW">The <see cref="TargetFrameworkInfoWithRetargetabilityInformation"/> representing the target framework that this type reference is being mapped to.</param>
      /// <param name="newRef">If the type reference is found in the target framework represented by <paramref name="targetFW"/> parameter, then this will hold the <see cref="AssemblyInformationForResolving"/> object describing the new assembly reference.</param>
      /// <returns>One of the values in <see cref="RemapResult"/>. If the value is <see cref="RemapResult.Success"/>, then <paramref name="newRef"/> will always be non-<c>null</c>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="loader"/> or <paramref name="targetFW"/> is <c>null</c>.</exception>
      /// <seealso cref="RemapResult"/>
      RemapResult TryRemapReference(
         CILMetaData thisMD,
         AssemblyInformationForResolving aRef,
         String fullType,
         CILMetaDataLoaderWithCallbacks loader,
         TargetFrameworkInfoWithRetargetabilityInformation targetFW,
         out AssemblyInformationForResolving newRef
         );
   }

   /// <summary>
   /// This enumeration represents possible remapping results when changing type reference from one target framework into another, by <see cref="TargetFrameworkMapper.TryRemapReference"/> method.
   /// </summary>
   public enum RemapResult
   {
      /// <summary>
      /// The type reference was successfully mapped into other assembly than original, and thus it has been changed.
      /// Sometimes only assembly version information changes, but other times also assembly name.
      /// </summary>
      Success,

      /// <summary>
      /// The type reference does not represent a reference into type located in target framework assemblies.
      /// </summary>
      NotATargetFrameworkReference,

      /// <summary>
      /// The type reference was reference to type in one of the target framework assemblies, but it was already the correct one.
      /// </summary>
      AlreadyMapped,

      /// <summary>
      /// The type reference was reference to type in one of the target framework assemblies, but the new target framework did not have any assembly which would have the type with the same name.
      /// </summary>
      NotPresentInGivenTargetFramework
   }

   /// <summary>
   /// This class encapsulates the information about target framework information (<see cref="CAMPhysical::CILAssemblyManipulator.Physical.TargetFrameworkInfo"/>) and whether the assembly references to that framework should be marked with <see cref="AssemblyFlags.Retargetable"/> flag.
   /// </summary>
   public sealed class TargetFrameworkInfoWithRetargetabilityInformation
   {
      /// <summary>
      /// Creates a new instance of <see cref="TargetFrameworkInfoWithRetargetabilityInformation"/> with given <see cref="CAMPhysical::CILAssemblyManipulator.Physical.TargetFrameworkInfo"/> and whether the assembly references to this target framework are retargetable.
      /// </summary>
      /// <param name="targetFramework">The <see cref="CAMPhysical::CILAssemblyManipulator.Physical.TargetFrameworkInfo"/> representing target framework information.</param>
      /// <param name="assemblyReferencesRetargetable">Whether the assembly references to <paramref name="targetFramework"/> should be tagged with <see cref="AssemblyFlags.Retargetable"/> flag.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="targetFramework"/> is <c>null</c>.</exception>
      public TargetFrameworkInfoWithRetargetabilityInformation(
         TargetFrameworkInfo targetFramework,
         Boolean assemblyReferencesRetargetable
         )
      {
         ArgumentValidator.ValidateNotNull( "Target framework information", targetFramework );

         this.TargetFrameworkInfo = targetFramework;
         this.AreFrameworkAssemblyReferencesRetargetable = assemblyReferencesRetargetable;
      }

      /// <summary>
      /// Gets the <see cref="CAMPhysical::CILAssemblyManipulator.Physical.TargetFrameworkInfo"/> for represented target framework.
      /// </summary>
      /// <value>The <see cref="CAMPhysical::CILAssemblyManipulator.Physical.TargetFrameworkInfo"/> for represented target framework.</value>
      public TargetFrameworkInfo TargetFrameworkInfo { get; }

      /// <summary>
      /// Gets the value indicating whether assembly references to represented target framework should be tagged with <see cref="AssemblyFlags.Retargetable"/> flag.
      /// </summary>
      /// <value>The value indicating whether assembly references to represented target framework should be tagged with <see cref="AssemblyFlags.Retargetable"/> flag.</value>
      public Boolean AreFrameworkAssemblyReferencesRetargetable { get; }
   }

   /// <summary>
   /// This is abstract class implementing <see cref="TargetFrameworkMapper"/> in such way that the types of various dictionaries are parametrized via type parameters.
   /// Thus the normal dictionaries, or concurrent dictionaries, may be used, depending on which one is required.
   /// </summary>
   /// <typeparam name="TTypesDic">The type of dictionary caching type strings.</typeparam>
   /// <typeparam name="TTargetFWAssembliesDic">The type of dictionary caching target framework assemblies resources (paths).</typeparam>
   /// <typeparam name="TResolvedTargetFWAssembliesDic">The type of dictionary caching target framework assembly references.</typeparam>
   /// <typeparam name="TResolvedTargetFWAssembliesDicInner">The inner dictionary type for <typeparamref name="TResolvedTargetFWAssembliesDic"/>.</typeparam>
   /// <typeparam name="TAssemblyReferenceDic">The of dictionary caching assembly references.</typeparam>
   /// <typeparam name="TAssemblyReferenceDicInner">The inner dictionary type for <typeparamref name="TAssemblyReferenceDic"/>.</typeparam>
   /// <remarks>
   /// This class is not directly instanceable, instead use <see cref="TargetFrameworkMapperNotThreadSafe"/>
#if CAM_PHYSICAL_IS_PORTABLE
   /// class.
#else
   /// or <see cref="TargetFrameworkMapperConcurrent"/> class.
#endif
   /// </remarks>
   public abstract class AbstractTargetFrameworkMapper<
      TTypesDic,
      TTargetFWAssembliesDic,
      TResolvedTargetFWAssembliesDic,
      TResolvedTargetFWAssembliesDicInner,
      TAssemblyReferenceDic,
      TAssemblyReferenceDicInner
      > : TargetFrameworkMapper
      where TTypesDic : class, IDictionary<CILMetaData, ISet<String>>
      where TTargetFWAssembliesDic : class, IDictionary<TargetFrameworkInfo, String[]>
      where TResolvedTargetFWAssembliesDic : class, IDictionary<CILMetaData, TResolvedTargetFWAssembliesDicInner>
      where TResolvedTargetFWAssembliesDicInner : class, IDictionary<String, CILMetaData>
      where TAssemblyReferenceDic : class, IDictionary<CILMetaData, TAssemblyReferenceDicInner>
      where TAssemblyReferenceDicInner : class, IDictionary<AssemblyInformationForResolving, CILMetaData>
   {
      private readonly TTypesDic _mdTypes;
      private readonly TTargetFWAssembliesDic _targetFWAssemblies;
      private readonly TResolvedTargetFWAssembliesDic _resolvedTargetFWAssemblies;
      private readonly TAssemblyReferenceDic _assemblyReferenceInfo;

      private readonly Func<CILMetaData, TResolvedTargetFWAssembliesDicInner> _resolvedInnerFactory;
      private readonly Func<CILMetaData, TAssemblyReferenceDicInner> _assemblyReferenceInnerFactory;

      internal AbstractTargetFrameworkMapper(
         TTypesDic mdTypes,
         TTargetFWAssembliesDic targetFWAssemblies,
         TResolvedTargetFWAssembliesDic resolvedTargetFWAssemblies,
         TAssemblyReferenceDic assemblyReferences,
         Func<CILMetaData, TResolvedTargetFWAssembliesDicInner> resolvedInnerFactory,
         Func<CILMetaData, TAssemblyReferenceDicInner> assemblyReferenceInnerFactory
         )
      {
         ArgumentValidator.ValidateNotNull( "Meta data type dictionary", mdTypes );
         ArgumentValidator.ValidateNotNull( "Target framework assemblies dictionary", targetFWAssemblies );
         ArgumentValidator.ValidateNotNull( "Resolved target framework assemblies dictionary", resolvedTargetFWAssemblies );
         ArgumentValidator.ValidateNotNull( "Assembly reference dictionary", assemblyReferences );
         ArgumentValidator.ValidateNotNull( "Resolved target framework assemblies inner dictionary factory", resolvedInnerFactory );
         ArgumentValidator.ValidateNotNull( "Assembly reference inner dictionary factory", assemblyReferenceInnerFactory );

         this._mdTypes = mdTypes;
         this._targetFWAssemblies = targetFWAssemblies;
         this._resolvedTargetFWAssemblies = resolvedTargetFWAssemblies;
         this._assemblyReferenceInfo = assemblyReferences;
         this._resolvedInnerFactory = resolvedInnerFactory;
         this._assemblyReferenceInnerFactory = assemblyReferenceInnerFactory;
      }

      /// <inheritdoc />
      public RemapResult TryRemapReference(
         CILMetaData thisMD,
         AssemblyInformationForResolving aRef,
         String fullType,
         CILMetaDataLoaderWithCallbacks loader,
         TargetFrameworkInfoWithRetargetabilityInformation targetFW,
         out AssemblyInformationForResolving newRef
         )
      {
         ArgumentValidator.ValidateNotNull( "Loader", loader );
         ArgumentValidator.ValidateNotNull( "New target framework information", targetFW );

         newRef = null;

         var targetFWAssembly = this.ResolveTargetFWReferenceOrNull( thisMD, aRef, loader, targetFW );
         RemapResult retVal;
         if ( targetFWAssembly != null )
         {
            var actualTargetFWAssembly = this.GetActualMDForType( targetFWAssembly, loader, fullType, targetFW.TargetFrameworkInfo );
            if ( actualTargetFWAssembly != null )
            {
               if ( !ReferenceEquals( targetFWAssembly, actualTargetFWAssembly ) )
               {
                  // Type was in another assembly
                  newRef = new AssemblyInformationForResolving( actualTargetFWAssembly.AssemblyDefinitions.TableContents[0].AssemblyInformation, true );
                  retVal = RemapResult.Success;
               }
               else
               {
                  retVal = RemapResult.AlreadyMapped;
               }
            }
            else
            {
               retVal = RemapResult.NotPresentInGivenTargetFramework;
            }
         }
         else
         {
            retVal = RemapResult.NotATargetFrameworkReference;
         }

         return retVal;
      }

      private CILMetaData GetActualMDForType(
         CILMetaData targetFWAssembly,
         CILMetaDataLoaderWithCallbacks loader,
         String fullType,
         TargetFrameworkInfo newTargetFW
         )
      {
         return this.GetOrAdd_ResolvedTargetFWAssembliesInner(
            this.GetOrAdd_ResolvedTargetFWAssemblies( this._resolvedTargetFWAssemblies, targetFWAssembly, this._resolvedInnerFactory ),
            fullType,
            typeStr =>
               this.GetSuitableMDsForTargetFW( targetFWAssembly, loader, newTargetFW, true )
                  .FirstOrDefault( md => this.IsTypePresent( md, typeStr ) )
            );
      }

      private CILMetaData ResolveTargetFWReferenceOrNull(
         CILMetaData thisMD,
         AssemblyInformationForResolving assemblyRef,
         CILMetaDataLoaderWithCallbacks loader,
         TargetFrameworkInfoWithRetargetabilityInformation targetFW
         )
      {
         return assemblyRef == null ? null : this.GetOrAdd_AssemblyReferencesInner(
            this.GetOrAdd_AssemblyReferences( this._assemblyReferenceInfo, thisMD, this._assemblyReferenceInnerFactory ),
            assemblyRef,
            aRef =>
            {
               var cb = loader.LoaderCallbacks;
               var validResource = cb
                  .GetPossibleResourcesForAssemblyReference( loader.GetResourceFor( thisMD ), thisMD, aRef, null )
                  .Where( res => cb.IsValidResource( res ) )
                  .FirstOrDefault();
               CILMetaData retVal;
               if ( validResource == null )
               {
                  // Most likely this metadata didn't have target framework info attribute
                  retVal = this.GetSuitableMDsForTargetFW( thisMD, loader, targetFW.TargetFrameworkInfo, false )
                     .FirstOrDefault( md => md.AssemblyDefinitions.GetOrNull( 0 )?.IsMatch( assemblyRef, targetFW.AreFrameworkAssemblyReferencesRetargetable, loader.ComputePublicKeyTokenOrNull ) ?? false );
               }
               else if ( validResource.StartsWith( cb.GetTargetFrameworkPathForFrameworkInfo( targetFW.TargetFrameworkInfo ) ) ) // Check whether resolved reference is located in target framework path
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
         return this.GetOrAdd_MDTypes( this._mdTypes, metaData, md => new HashSet<String>( md.GetTypeDefinitionsFullNames() ) )
            .Contains( typeName );
      }

      private IEnumerable<CILMetaData> GetSuitableMDsForTargetFW(
         CILMetaData md,
         CILMetaDataLoaderWithCallbacks loader,
         TargetFrameworkInfo targetFW,
         Boolean returnThis
         )
      {
         if ( returnThis )
         {
            // Always try current library at first
            yield return md;
         }

         // Then start enumerating all the rest of the assemblies in target framework directory
         foreach ( var res in this.GetTargetFWAssemblies( targetFW, loader ).Where( r => !this.IsRecordedNotManagedAssembly( r ) ) )
         {
            CILMetaData current;
            try
            {
               current = loader.GetOrLoadMetaData( res );
            }
            catch ( MetaDataLoadException e )
            {
               if ( e.InnerException is NotAManagedModuleException )
               {
                  current = null;
                  this.RecordNotManagedAssembly( res );
               }
               else
               {
                  throw;
               }
            }

            if ( current != null && !ReferenceEquals( md, current ) )
            {
               yield return current;
            }
         }
      }

      private String[] GetTargetFWAssemblies( TargetFrameworkInfo targetFW, CILMetaDataLoaderWithCallbacks loader )
      {
         return this.GetOrAdd_TargetFWAssemblies( this._targetFWAssemblies, targetFW, tfw => loader.LoaderCallbacks.GetAssemblyResourcesForFramework( tfw ).ToArray() );
      }

      /// <summary>
      /// Callback-method to get or add value to dictionary of type <typeparamref name="TTargetFWAssembliesDic"/>.
      /// </summary>
      /// <param name="dic">The dictionary.</param>
      /// <param name="key">The key to dictionary.</param>
      /// <param name="factory">The factory callback, if the value is not present for given <paramref name="key"/>.</param>
      /// <returns>The existing or created value.</returns>
      protected abstract String[] GetOrAdd_TargetFWAssemblies( TTargetFWAssembliesDic dic, TargetFrameworkInfo key, Func<TargetFrameworkInfo, String[]> factory );

      /// <summary>
      /// Callback-method to get or add value to dictionary of type <typeparamref name="TTypesDic"/>.
      /// </summary>
      /// <param name="dic">The dictionary.</param>
      /// <param name="key">The key to dictionary.</param>
      /// <param name="factory">The factory callback, if the value is not present for given <paramref name="key"/>.</param>
      /// <returns>The existing or created value.</returns>
      protected abstract ISet<String> GetOrAdd_MDTypes( TTypesDic dic, CILMetaData key, Func<CILMetaData, ISet<String>> factory );

      /// <summary>
      /// Callback-method to get or add value to dictionary of type <typeparamref name="TResolvedTargetFWAssembliesDic"/>.
      /// </summary>
      /// <param name="dic">The dictionary.</param>
      /// <param name="key">The key to dictionary.</param>
      /// <param name="factory">The factory callback, if the value is not present for given <paramref name="key"/>.</param>
      /// <returns>The existing or created value.</returns>
      protected abstract TResolvedTargetFWAssembliesDicInner GetOrAdd_ResolvedTargetFWAssemblies( TResolvedTargetFWAssembliesDic dic, CILMetaData key, Func<CILMetaData, TResolvedTargetFWAssembliesDicInner> factory );

      /// <summary>
      /// Callback-method to get or add value to dictionary of type <typeparamref name="TResolvedTargetFWAssembliesDicInner"/>.
      /// </summary>
      /// <param name="dic">The dictionary.</param>
      /// <param name="key">The key to dictionary.</param>
      /// <param name="factory">The factory callback, if the value is not present for given <paramref name="key"/>.</param>
      /// <returns>The existing or created value.</returns>
      protected abstract CILMetaData GetOrAdd_ResolvedTargetFWAssembliesInner( TResolvedTargetFWAssembliesDicInner dic, String key, Func<String, CILMetaData> factory );

      /// <summary>
      /// Callback-method to get or add value to dictionary of type <typeparamref name="TAssemblyReferenceDic"/>.
      /// </summary>
      /// <param name="dic">The dictionary.</param>
      /// <param name="key">The key to dictionary.</param>
      /// <param name="factory">The factory callback, if the value is not present for given <paramref name="key"/>.</param>
      /// <returns>The existing or created value.</returns>
      protected abstract TAssemblyReferenceDicInner GetOrAdd_AssemblyReferences( TAssemblyReferenceDic dic, CILMetaData key, Func<CILMetaData, TAssemblyReferenceDicInner> factory );

      /// <summary>
      /// Callback-method to get or add value to dictionary of type <typeparamref name="TAssemblyReferenceDicInner"/>.
      /// </summary>
      /// <param name="dic">The dictionary.</param>
      /// <param name="key">The key to dictionary.</param>
      /// <param name="factory">The factory callback, if the value is not present for given <paramref name="key"/>.</param>
      /// <returns>The existing or created value.</returns>
      protected abstract CILMetaData GetOrAdd_AssemblyReferencesInner( TAssemblyReferenceDicInner dic, AssemblyInformationForResolving key, Func<AssemblyInformationForResolving, CILMetaData> factory );

      /// <summary>
      /// Callback-method to record resource which is not a managed assembly.
      /// </summary>
      /// <param name="resource">The resource (path) for the assembly which was detected to be unmanaged assembly.</param>
      protected abstract void RecordNotManagedAssembly( String resource );

      /// <summary>
      /// Callback-method to check whether the resource represents previously recorded (via <see cref="RecordNotManagedAssembly"/> method) unmanaged assembly.
      /// </summary>
      /// <param name="resource">The resource (path) to check.</param>
      /// <returns><c>true</c> if the assembly at given <paramref name="resource"/> was previously recorded to be as unmanaged assembly; <c>false</c> otherwise.</returns>
      protected abstract Boolean IsRecordedNotManagedAssembly( String resource );

   }

   /// <summary>
   /// This class extends <see cref="AbstractTargetFrameworkMapper{TTypesDic, TTargetFWAssembliesDic, TResolvedTargetFWAssembliesDic, TResolvedTargetFWAssembliesDicInner, TAssemblyReferenceDic, TAssemblyReferenceDicInner}"/> (and thus implements <see cref="TargetFrameworkMapper"/>) to be used in non-threadsafe scenarios.
   /// </summary>
   public class TargetFrameworkMapperNotThreadSafe : AbstractTargetFrameworkMapper<
      Dictionary<CILMetaData, ISet<String>>,
      Dictionary<TargetFrameworkInfo, String[]>,
      Dictionary<CILMetaData, Dictionary<String, CILMetaData>>,
      Dictionary<String, CILMetaData>,
      Dictionary<CILMetaData, Dictionary<AssemblyInformationForResolving, CILMetaData>>,
      Dictionary<AssemblyInformationForResolving, CILMetaData>
      >
   {
      private readonly HashSet<String> _notManagedAssemblies;

      /// <summary>
      /// Creates a new instance of <see cref="TargetFrameworkMapperNotThreadSafe"/>.
      /// </summary>
      public TargetFrameworkMapperNotThreadSafe()
         : base(
         new Dictionary<CILMetaData, ISet<String>>(),
         new Dictionary<TargetFrameworkInfo, String[]>(),
         new Dictionary<CILMetaData, Dictionary<String, CILMetaData>>(),
         new Dictionary<CILMetaData, Dictionary<AssemblyInformationForResolving, CILMetaData>>(),
         md => new Dictionary<String, CILMetaData>(),
         md => new Dictionary<AssemblyInformationForResolving, CILMetaData>()
         )
      {
         this._notManagedAssemblies = new HashSet<String>();
      }

      /// <summary>
      /// Implements <see cref="AbstractTargetFrameworkMapper{TTypesDic, TTargetFWAssembliesDic, TResolvedTargetFWAssembliesDic, TResolvedTargetFWAssembliesDicInner, TAssemblyReferenceDic, TAssemblyReferenceDicInner}.GetOrAdd_TargetFWAssemblies"/> by calling <see cref="E_CommonUtils.GetOrAdd_NotThreadSafe{TKey, TValue}(IDictionary{TKey, TValue}, TKey, Func{TKey, TValue})"/>.
      /// </summary>
      /// <param name="dic">The dictionary.</param>
      /// <param name="key">The key to dictionary.</param>
      /// <param name="factory">The factory callback, if the value is not present for given <paramref name="key"/>.</param>
      /// <returns>The existing or created value.</returns>
      protected override String[] GetOrAdd_TargetFWAssemblies( Dictionary<TargetFrameworkInfo, String[]> dic, TargetFrameworkInfo key, Func<TargetFrameworkInfo, String[]> factory )
      {
         return dic.GetOrAdd_NotThreadSafe( key, factory );
      }

      /// <summary>
      /// Implements <see cref="AbstractTargetFrameworkMapper{TTypesDic, TTargetFWAssembliesDic, TResolvedTargetFWAssembliesDic, TResolvedTargetFWAssembliesDicInner, TAssemblyReferenceDic, TAssemblyReferenceDicInner}.GetOrAdd_MDTypes"/> by calling <see cref="E_CommonUtils.GetOrAdd_NotThreadSafe{TKey, TValue}(IDictionary{TKey, TValue}, TKey, Func{TKey, TValue})"/>.
      /// </summary>
      /// <param name="dic">The dictionary.</param>
      /// <param name="key">The key to dictionary.</param>
      /// <param name="factory">The factory callback, if the value is not present for given <paramref name="key"/>.</param>
      /// <returns>The existing or created value.</returns>
      protected override ISet<String> GetOrAdd_MDTypes( Dictionary<CILMetaData, ISet<String>> dic, CILMetaData key, Func<CILMetaData, ISet<String>> factory )
      {
         return dic.GetOrAdd_NotThreadSafe( key, factory );
      }

      /// <summary>
      /// Implements <see cref="AbstractTargetFrameworkMapper{TTypesDic, TTargetFWAssembliesDic, TResolvedTargetFWAssembliesDic, TResolvedTargetFWAssembliesDicInner, TAssemblyReferenceDic, TAssemblyReferenceDicInner}.GetOrAdd_ResolvedTargetFWAssemblies"/> by calling <see cref="E_CommonUtils.GetOrAdd_NotThreadSafe{TKey, TValue}(IDictionary{TKey, TValue}, TKey, Func{TKey, TValue})"/>.
      /// </summary>
      /// <param name="dic">The dictionary.</param>
      /// <param name="key">The key to dictionary.</param>
      /// <param name="factory">The factory callback, if the value is not present for given <paramref name="key"/>.</param>
      /// <returns>The existing or created value.</returns>
      protected override Dictionary<String, CILMetaData> GetOrAdd_ResolvedTargetFWAssemblies( Dictionary<CILMetaData, Dictionary<String, CILMetaData>> dic, CILMetaData key, Func<CILMetaData, Dictionary<String, CILMetaData>> factory )
      {
         return dic.GetOrAdd_NotThreadSafe( key, factory );
      }

      /// <summary>
      /// Implements <see cref="AbstractTargetFrameworkMapper{TTypesDic, TTargetFWAssembliesDic, TResolvedTargetFWAssembliesDic, TResolvedTargetFWAssembliesDicInner, TAssemblyReferenceDic, TAssemblyReferenceDicInner}.GetOrAdd_ResolvedTargetFWAssembliesInner"/> by calling <see cref="E_CommonUtils.GetOrAdd_NotThreadSafe{TKey, TValue}(IDictionary{TKey, TValue}, TKey, Func{TKey, TValue})"/>.
      /// </summary>
      /// <param name="dic">The dictionary.</param>
      /// <param name="key">The key to dictionary.</param>
      /// <param name="factory">The factory callback, if the value is not present for given <paramref name="key"/>.</param>
      /// <returns>The existing or created value.</returns>
      protected override CILMetaData GetOrAdd_ResolvedTargetFWAssembliesInner( Dictionary<String, CILMetaData> dic, String key, Func<String, CILMetaData> factory )
      {
         return dic.GetOrAdd_NotThreadSafe( key, factory );
      }

      /// <summary>
      /// Implements <see cref="AbstractTargetFrameworkMapper{TTypesDic, TTargetFWAssembliesDic, TResolvedTargetFWAssembliesDic, TResolvedTargetFWAssembliesDicInner, TAssemblyReferenceDic, TAssemblyReferenceDicInner}.GetOrAdd_AssemblyReferences"/> by calling <see cref="E_CommonUtils.GetOrAdd_NotThreadSafe{TKey, TValue}(IDictionary{TKey, TValue}, TKey, Func{TKey, TValue})"/>.
      /// </summary>
      /// <param name="dic">The dictionary.</param>
      /// <param name="key">The key to dictionary.</param>
      /// <param name="factory">The factory callback, if the value is not present for given <paramref name="key"/>.</param>
      /// <returns>The existing or created value.</returns>
      protected override Dictionary<AssemblyInformationForResolving, CILMetaData> GetOrAdd_AssemblyReferences( Dictionary<CILMetaData, Dictionary<AssemblyInformationForResolving, CILMetaData>> dic, CILMetaData key, Func<CILMetaData, Dictionary<AssemblyInformationForResolving, CILMetaData>> factory )
      {
         return dic.GetOrAdd_NotThreadSafe( key, factory );
      }

      /// <summary>
      /// Implements <see cref="AbstractTargetFrameworkMapper{TTypesDic, TTargetFWAssembliesDic, TResolvedTargetFWAssembliesDic, TResolvedTargetFWAssembliesDicInner, TAssemblyReferenceDic, TAssemblyReferenceDicInner}.GetOrAdd_AssemblyReferencesInner"/> by calling <see cref="E_CommonUtils.GetOrAdd_NotThreadSafe{TKey, TValue}(IDictionary{TKey, TValue}, TKey, Func{TKey, TValue})"/>.
      /// </summary>
      /// <param name="dic">The dictionary.</param>
      /// <param name="key">The key to dictionary.</param>
      /// <param name="factory">The factory callback, if the value is not present for given <paramref name="key"/>.</param>
      /// <returns>The existing or created value.</returns>
      protected override CILMetaData GetOrAdd_AssemblyReferencesInner( Dictionary<AssemblyInformationForResolving, CILMetaData> dic, AssemblyInformationForResolving key, Func<AssemblyInformationForResolving, CILMetaData> factory )
      {
         return dic.GetOrAdd_NotThreadSafe( key, factory );
      }

      /// <summary>
      /// Implements <see cref="AbstractTargetFrameworkMapper{TTypesDic, TTargetFWAssembliesDic, TResolvedTargetFWAssembliesDic, TResolvedTargetFWAssembliesDicInner, TAssemblyReferenceDic, TAssemblyReferenceDicInner}.RecordNotManagedAssembly"/> by using basic <see cref="HashSet{T}"/>.
      /// </summary>
      /// <param name="resource">The resource (path) for the assembly which was detected to be unmanaged assembly.</param>
      protected override void RecordNotManagedAssembly( String resource )
      {
         this._notManagedAssemblies.Add( resource );
      }

      /// <summary>
      /// Implements <see cref="AbstractTargetFrameworkMapper{TTypesDic, TTargetFWAssembliesDic, TResolvedTargetFWAssembliesDic, TResolvedTargetFWAssembliesDicInner, TAssemblyReferenceDic, TAssemblyReferenceDicInner}.IsRecordedNotManagedAssembly"/> by using basic <see cref="HashSet{T}"/>.
      /// </summary>
      /// <param name="resource">The resource (path) to check.</param>
      /// <returns><c>true</c> if the assembly at given <paramref name="resource"/> was previously recorded to be as unmanaged assembly; <c>false</c> otherwise.</returns>
      protected override bool IsRecordedNotManagedAssembly( String resource )
      {
         return this._notManagedAssemblies.Contains( resource );
      }
   }

   /// <summary>
   /// This exception is thrown by extension methods of <see cref="TargetFrameworkMapper"/>.
   /// For example, <see cref="E_CILPhysical.RemapReference"/> and <see cref="E_CILPhysical.RemapTypeString"/> will throw this exception when the return value of <see cref="TargetFrameworkMapper.TryRemapReference"/> is <see cref="RemapResult.NotPresentInGivenTargetFramework"/>.
   /// </summary>
   public class TargetFrameworkRemapException : Exception
   {
      /// <summary>
      /// Creates a new instance of <see cref="TargetFrameworkRemapException"/> with given message and optional inner exception.
      /// </summary>
      /// <param name="msg">The message.</param>
      /// <param name="inner">The optional inner exception.</param>
      public TargetFrameworkRemapException( String msg, Exception inner = null )
         : base( msg, inner )
      {

      }
   }
}

public static partial class E_CILPhysical
{
   /// <summary>
   /// Helper method to try to remap type reference, and throw <see cref="TargetFrameworkRemapException"/> if the type was in original set of target framework assemblies, but not present in the new set of target framework assemblies.
   /// </summary>
   /// <param name="mapper">This <see cref="TargetFrameworkMapper"/>.</param>
   /// <param name="thisMD">The <see cref="CILMetaData"/> containing the assembly reference.</param>
   /// <param name="aRef">The assembly reference, as <see cref="AssemblyInformationForResolving"/>.</param>
   /// <param name="fullType">The full type name (containing namespace, possible enclosing type names, and the type name) as string.</param>
   /// <param name="loader">The <see cref="CILMetaDataLoaderWithCallbacks"/> to use when performing on-demand loading of the assemblies in given target framework.</param>
   /// <param name="targetFW">The <see cref="TargetFrameworkInfoWithRetargetabilityInformation"/> representing the target framework that this type reference is being mapped to.</param>
   /// <param name="newRef">If the type reference is found in the target framework represented by <paramref name="targetFW"/> parameter, then this will hold the <see cref="AssemblyInformationForResolving"/> object describing the new assembly reference.</param>
   /// <returns>One of the values in <see cref="RemapResult"/>. If the value is <see cref="RemapResult.Success"/>, then <paramref name="newRef"/> will always be non-<c>null</c>.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="TargetFrameworkMapper"/> is <c>null</c>.</exception>
   /// <exception cref="TargetFrameworkRemapException">If the return value of <see cref="TargetFrameworkMapper.TryRemapReference"/> is <see cref="RemapResult.NotPresentInGivenTargetFramework"/>.</exception>
   public static RemapResult RemapReference(
      this TargetFrameworkMapper mapper,
      CILMetaData thisMD,
      AssemblyInformationForResolving aRef,
      String fullType,
      CILMetaDataLoaderWithCallbacks loader,
      TargetFrameworkInfoWithRetargetabilityInformation targetFW,
      out AssemblyInformationForResolving newRef
      )
   {
      var retVal = mapper.TryRemapReference( thisMD, aRef, fullType, loader, targetFW, out newRef );
      if ( retVal == RemapResult.NotPresentInGivenTargetFramework )
      {
         throw new TargetFrameworkRemapException( "The type reference " + fullType + " located in " + loader.GetResourceFor( thisMD ) + " is not present in the " + targetFW + "." );
      }
      return retVal;
   }

   /// <summary>
   /// Helper method to try to remap textual type reference using this <see cref="TargetFrameworkMapper"/>.
   /// </summary>
   /// <param name="mapper">This <see cref="TargetFrameworkMapper"/>.</param>
   /// <param name="thisMD">The <see cref="CILMetaData"/> containing the assembly reference.</param>
   /// <param name="loader">The <see cref="CILMetaDataLoaderWithCallbacks"/> to use when performing on-demand loading of the assemblies in given target framework.</param>
   /// <param name="targetFW">The <see cref="TargetFrameworkInfoWithRetargetabilityInformation"/> representing the target framework that this type reference is being mapped to.</param>
   /// <param name="typeString">The full type string including assembly name.</param>
   /// <returns>If the <paramref name="typeString"/> did not contain assembly name, or if the assembly name could not be parsed using <see cref="AssemblyInformation.TryParse"/> method, returns <c>null</c>. Otherwise, returns the same value as <see cref="TargetFrameworkMapper.TryRemapReference"/>.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="TargetFrameworkMapper"/> is <c>null</c>.</exception>
   public static RemapResult? TryRemapTypeString(
      this TargetFrameworkMapper mapper,
      CILMetaData thisMD,
      CILMetaDataLoaderWithCallbacks loader,
      TargetFrameworkInfoWithRetargetabilityInformation targetFW,
      ref String typeString
      )
   {
      String typeName, assemblyName;
      AssemblyInformation assemblyInfo;
      Boolean isFullPublicKey;

      RemapResult? retVal;
      if ( typeString.ParseAssemblyQualifiedTypeString( out typeName, out assemblyName )
         && AssemblyInformation.TryParse( assemblyName, out assemblyInfo, out isFullPublicKey ) )
      {
         AssemblyInformationForResolving newRef = null;
         retVal = mapper.TryRemapReference( thisMD, new AssemblyInformationForResolving( assemblyInfo, isFullPublicKey ), typeName, loader, targetFW, out newRef );

         if ( retVal == RemapResult.Success )
         {
            assemblyName = newRef.AssemblyInformation.ToString( true, true );
            typeString = Miscellaneous.CombineAssemblyAndType( assemblyName, typeName );
         }
      }
      else
      {
         retVal = null;
      }

      return retVal;
   }

   /// <summary>
   /// Helper method to try to remap textual type reference using this <see cref="TargetFrameworkMapper"/>, and throw <see cref="TargetFrameworkRemapException"/> if type string is parsed successfully and the type was in original set of target framework assemblies, but not present in the new set of target framework assemblies.
   /// </summary>
   /// <param name="mapper">This <see cref="TargetFrameworkMapper"/>.</param>
   /// <param name="thisMD">The <see cref="CILMetaData"/> containing the assembly reference.</param>
   /// <param name="loader">The <see cref="CILMetaDataLoaderWithCallbacks"/> to use when performing on-demand loading of the assemblies in given target framework.</param>
   /// <param name="targetFW">The <see cref="TargetFrameworkInfoWithRetargetabilityInformation"/> representing the target framework that this type reference is being mapped to.</param>
   /// <param name="typeString">The full type string including assembly name.</param>
   /// <returns>If the <paramref name="typeString"/> did not contain assembly name, or if the assembly name could not be parsed using <see cref="AssemblyInformation.TryParse"/> method, returns <c>null</c>. Otherwise, returns the same value as <see cref="TargetFrameworkMapper.TryRemapReference"/>.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="TargetFrameworkMapper"/> is <c>null</c>.</exception>
   /// <exception cref="TargetFrameworkRemapException">If the return value of <see cref="TryRemapTypeString"/> is <see cref="RemapResult.NotPresentInGivenTargetFramework"/>.</exception>
   public static RemapResult? RemapTypeString(
      this TargetFrameworkMapper mapper,
      CILMetaData thisMD,
      CILMetaDataLoaderWithCallbacks loader,
      TargetFrameworkInfoWithRetargetabilityInformation targetFW,
      ref String typeString
      )
   {
      var retVal = mapper.TryRemapTypeString( thisMD, loader, targetFW, ref typeString );
      if ( retVal.HasValue && retVal.Value == RemapResult.NotPresentInGivenTargetFramework )
      {
         throw new TargetFrameworkRemapException( "The type string " + typeString + " located in " + loader.GetResourceFor( thisMD ) + " is not present in the " + targetFW + "." );
      }
      return retVal;
   }

   /// <summary>
   /// This method will remap all type references (including <see cref="CILMetaData.TypeReferences"/> table, and textual type strings in various signatures) to given target framework.
   /// </summary>
   /// <param name="mapper">This <see cref="TargetFrameworkMapper"/>.</param>
   /// <param name="md">The <see cref="CILMetaData"/> to process.</param>
   /// <param name="loader">The <see cref="CILMetaDataLoaderWithCallbacks"/> to use when performing on-demand loading of the assemblies in given target framework.</param>
   /// <param name="targetFW">The <see cref="TargetFrameworkInfoWithRetargetabilityInformation"/> representing the target framework that this type reference is being mapped to.</param>
   /// <exception cref="NullReferenceException">If this <see cref="TargetFrameworkMapper"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="loader"/> or <paramref name="targetFW"/> is <c>null</c>.</exception>
   /// <exception cref="TargetFrameworkRemapException">If at least one resolved type reference is not present in the target framework represented by <paramref name="targetFW"/>.</exception>
   public static void ChangeTargetFramework(
      this TargetFrameworkMapper mapper,
      CILMetaData md,
      CILMetaDataLoaderWithCallbacks loader,
      TargetFrameworkInfoWithRetargetabilityInformation targetFW
      )
   {
      ArgumentValidator.ValidateNotNull( "Loader", loader );
      ArgumentValidator.ValidateNotNull( "New target framework information", targetFW );


      var cb = loader.LoaderCallbacks;
      var newTargetFWPath = cb.GetTargetFrameworkPathForFrameworkInfo( targetFW.TargetFrameworkInfo );
      var aRefsTable = md.AssemblyReferences;
      var aRefs = aRefsTable.TableContents;

      var aRefPaths = new Dictionary<AssemblyReference, String>( ReferenceEqualityComparer<AssemblyReference>.ReferenceBasedComparer );
      var aRefDic = Enumerable.Range( 0, aRefs.Count )
         .ToDictionary_Overwrite( aRefIdx => aRefs[aRefIdx], aRefIdx => aRefIdx, aRefsTable.TableInformation.EqualityComparer );

      // First, type refs
      foreach ( var tRef in md.TypeReferences.TableContents.Where( tr => tr.ResolutionScope.HasValue && tr.ResolutionScope.Value.Table == Tables.AssemblyRef ) )
      {
         var aRefIdx = tRef.ResolutionScope.Value;
         var aRef = aRefs[aRefIdx.Index];

         AssemblyInformationForResolving newRefInfo;
         var remapResult = mapper.RemapReference( md, new AssemblyInformationForResolving( aRef ), Miscellaneous.CombineNamespaceAndType( tRef.Namespace, tRef.Name ), loader, targetFW, out newRefInfo );
         var wasTargetFW = remapResult != RemapResult.NotATargetFrameworkReference;

         AssemblyReference newRef;
         if ( remapResult == RemapResult.Success )
         {
            newRef = newRefInfo.AssemblyInformation.AsAssemblyReference( newRefInfo.IsFullPublicKey );
            Int32 aRefNewIdx;
            if ( !aRefDic.TryGetValue( newRef, out aRefNewIdx ) )
            {
               aRefNewIdx = aRefs.Count;
               aRefs.Add( newRef );
            }

            tRef.ResolutionScope = aRefIdx.ChangeIndex( aRefNewIdx );
         }
         else
         {
            newRef = aRef;
         }

         if ( wasTargetFW && targetFW.AreFrameworkAssemblyReferencesRetargetable )
         {
            newRef.Attributes |= AssemblyFlags.Retargetable;
         }
      }

      // Then, all type strings (sec blobs, custom attrs, marshal infos)
      foreach ( var marshal in md.FieldMarshals.TableContents )
      {
         mapper.ProcessMarshalInfo( md, loader, targetFW, marshal.NativeType );
      }

      foreach ( var sec in md.SecurityDefinitions.TableContents )
      {
         foreach ( var permSet in sec.PermissionSets.OfType<SecurityInformation>() )
         {
            var typeStr = permSet.SecurityAttributeType;
            if ( mapper.RemapTypeString( md, loader, targetFW, ref typeStr ).IsSuccess() )
            {
               permSet.SecurityAttributeType = typeStr;
            }
            foreach ( var namedArg in permSet.NamedArguments )
            {
               mapper.ProcessCASignatureNamed( md, loader, targetFW, namedArg );
            }
         }
      }

      foreach ( var ca in md.CustomAttributeDefinitions.TableContents )
      {
         mapper.ProcessCASignature( md, loader, targetFW, ca.Signature );
      }

      // TODO Extra tables!
   }

   private static void ProcessMarshalInfo(
      this TargetFrameworkMapper mapper,
      CILMetaData md,
      CILMetaDataLoaderWithCallbacks loader,
      TargetFrameworkInfoWithRetargetabilityInformation newTargetFW,
      AbstractMarshalingInfo marshal
      )
   {
      String typeStr;
      switch ( marshal?.MarshalingInfoKind )
      {
         case MarshalingInfoKind.SafeArray:
            var safeArray = (SafeArrayMarshalingInfo) marshal;
            typeStr = safeArray.UserDefinedType;
            if ( mapper.RemapTypeString( md, loader, newTargetFW, ref typeStr ).IsSuccess() )
            {
               safeArray.UserDefinedType = typeStr;
            }
            break;
         case MarshalingInfoKind.Custom:
            var custom = (CustomMarshalingInfo) marshal;
            typeStr = custom.CustomMarshalerTypeName;
            if ( mapper.RemapTypeString( md, loader, newTargetFW, ref typeStr ).IsSuccess() )
            {
               custom.CustomMarshalerTypeName = typeStr;
            }
            break;
      }

   }

   private static void ProcessCASignature(
      this TargetFrameworkMapper mapper,
      CILMetaData md,
      CILMetaDataLoaderWithCallbacks loader,
      TargetFrameworkInfoWithRetargetabilityInformation newTargetFW,
      AbstractCustomAttributeSignature sig
      )
   {
      if ( sig != null && sig.CustomAttributeSignatureKind == CustomAttributeSignatureKind.Resolved )
      {
         var sigg = (ResolvedCustomAttributeSignature) sig;
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

   private static void ProcessCASignatureTyped(
      this TargetFrameworkMapper mapper,
      CILMetaData md,
      CILMetaDataLoaderWithCallbacks loader,
      TargetFrameworkInfoWithRetargetabilityInformation newTargetFW,
      CustomAttributeTypedArgument arg
      )
   {
      if ( arg != null )
      {
         var value = arg.Value;
         if ( mapper.ProcessCASignatureTypedValue( md, loader, newTargetFW, ref value ) )
         {
            arg.Value = value;
         }
      }
   }

   private static Boolean ProcessCASignatureTypedValue(
      this TargetFrameworkMapper mapper,
      CILMetaData md,
      CILMetaDataLoaderWithCallbacks loader,
      TargetFrameworkInfoWithRetargetabilityInformation newTargetFW,
      ref Object value
      )
   {
      var retVal = false;
      if ( value != null )
      {
         var complex = value as CustomAttributeTypedArgumentValueComplex;
         if ( complex != null )
         {
            String typeString;
            switch ( complex.CustomAttributeTypedArgumentValueKind )
            {
               case CustomAttributeTypedArgumentValueKind.Type:
                  typeString = ( (CustomAttributeValue_TypeReference) complex ).TypeString;
                  break;
               case CustomAttributeTypedArgumentValueKind.Enum:
                  typeString = ( (CustomAttributeValue_EnumReference) complex ).EnumType;
                  break;
               case CustomAttributeTypedArgumentValueKind.Array:
                  var arrayValue = (CustomAttributeValue_Array) complex;
                  var elType = arrayValue.ArrayElementType;
                  typeString = elType != null && elType.ArgumentTypeKind == CustomAttributeArgumentTypeKind.Enum ?
                     ( (CustomAttributeArgumentTypeEnum) elType ).TypeString :
                     null;
                  var array = arrayValue.Array;
                  if ( array != null )
                  {
                     for ( var i = 0; i < array.Length; ++i )
                     {
                        var cur = array.GetValue( i );
                        if ( mapper.ProcessCASignatureTypedValue( md, loader, newTargetFW, ref cur ) )
                        {
                           array.SetValue( cur, i );
                        }
                     }
                  }
                  break;
               default:
                  typeString = null;
                  break;
            }

            retVal = typeString != null && mapper.RemapTypeString( md, loader, newTargetFW, ref typeString ).IsSuccess();
            if ( retVal )
            {
               switch ( complex.CustomAttributeTypedArgumentValueKind )
               {
                  case CustomAttributeTypedArgumentValueKind.Type:
                     value = new CustomAttributeValue_TypeReference( typeString );
                     break;
                  case CustomAttributeTypedArgumentValueKind.Enum:
                     value = new CustomAttributeValue_EnumReference( typeString, ( (CustomAttributeValue_EnumReference) complex ).EnumValue );
                     break;
                  case CustomAttributeTypedArgumentValueKind.Array:
                     value = new CustomAttributeValue_Array( ( (CustomAttributeValue_Array) complex ).Array, new CustomAttributeArgumentTypeEnum() { TypeString = typeString } );
                     break;
               }
            }
         }
      }

      return retVal;
   }

   private static void ProcessCASignatureNamed(
      this TargetFrameworkMapper mapper,
      CILMetaData md,
      CILMetaDataLoaderWithCallbacks loader,
      TargetFrameworkInfoWithRetargetabilityInformation newTargetFW,
      CustomAttributeNamedArgument arg
      )
   {
      if ( arg != null )
      {
         var type = arg.FieldOrPropertyType;
         if ( type != null && type.ArgumentTypeKind == CustomAttributeArgumentTypeKind.Enum )
         {
            var typeStrArg = (CustomAttributeArgumentTypeEnum) type;
            var typeString = typeStrArg.TypeString;
            if ( mapper.RemapTypeString( md, loader, newTargetFW, ref typeString ).IsSuccess() )
            {
               typeStrArg.TypeString = typeString;
            }
         }
         mapper.ProcessCASignatureTyped( md, loader, newTargetFW, arg.Value );
      }
   }

   private static Boolean IsSuccess( this RemapResult? res )
   {
      return res.HasValue && res.Value == RemapResult.Success;
   }
}
