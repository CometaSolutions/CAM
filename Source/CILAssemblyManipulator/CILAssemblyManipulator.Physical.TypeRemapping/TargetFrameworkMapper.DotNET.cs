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
#if !CAM_PHYSICAL_IS_PORTABLE
extern alias CAMPhysicalR;
using CAMPhysicalR;
using CAMPhysicalR::CILAssemblyManipulator.Physical.Resolving;

using CILAssemblyManipulator.Physical.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.TypeRemapping
{
   /// <summary>
   /// This class implements <see cref="AbstractTargetFrameworkMapper{TTypesDic, TTargetFWAssembliesDic, TResolvedTargetFWAssembliesDic, TResolvedTargetFWAssembliesDicInner, TAssemblyReferenceDic, TAssemblyReferenceDicInner}"/> using <see cref="ConcurrentDictionary{TKey, TValue}"/> objects as caches.
   /// Thus, it is safe to use in concurrent scenarios.
   /// </summary>
   public class TargetFrameworkMapperConcurrent : AbstractTargetFrameworkMapper<
      ConcurrentDictionary<CILMetaData, ISet<String>>,
      ConcurrentDictionary<TargetFrameworkInfo, String[]>,
      ConcurrentDictionary<CILMetaData, ConcurrentDictionary<String, CILMetaData>>,
      ConcurrentDictionary<String, CILMetaData>,
      ConcurrentDictionary<CILMetaData, ConcurrentDictionary<AssemblyInformationForResolving, CILMetaData>>,
      ConcurrentDictionary<AssemblyInformationForResolving, CILMetaData>
      >
   {
      private readonly ConcurrentDictionary<String, String> _notManagedAssemblies;

      /// <summary>
      /// Creates a new instance of <see cref="TargetFrameworkMapperConcurrent"/>.
      /// </summary>
      public TargetFrameworkMapperConcurrent()
         : base(
         new ConcurrentDictionary<CILMetaData, ISet<String>>(),
         new ConcurrentDictionary<TargetFrameworkInfo, String[]>(),
         new ConcurrentDictionary<CILMetaData, ConcurrentDictionary<String, CILMetaData>>(),
         new ConcurrentDictionary<CILMetaData, ConcurrentDictionary<AssemblyInformationForResolving, CILMetaData>>(),
         md => new ConcurrentDictionary<String, CILMetaData>(),
         md => new ConcurrentDictionary<AssemblyInformationForResolving, CILMetaData>()
         )
      {
         this._notManagedAssemblies = new ConcurrentDictionary<String, String>();
      }

      /// <summary>
      /// Implements <see cref="AbstractTargetFrameworkMapper{TTypesDic, TTargetFWAssembliesDic, TResolvedTargetFWAssembliesDic, TResolvedTargetFWAssembliesDicInner, TAssemblyReferenceDic, TAssemblyReferenceDicInner}.GetOrAdd_TargetFWAssemblies"/> by calling <see cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, Func{TKey, TValue})"/>.
      /// </summary>
      /// <param name="dic">The dictionary.</param>
      /// <param name="key">The key to dictionary.</param>
      /// <param name="factory">The factory callback, if the value is not present for given <paramref name="key"/>.</param>
      /// <returns>The existing or created value.</returns>
      protected override String[] GetOrAdd_TargetFWAssemblies( ConcurrentDictionary<TargetFrameworkInfo, String[]> dic, TargetFrameworkInfo key, Func<TargetFrameworkInfo, String[]> factory )
      {
         return dic.GetOrAdd( key, factory );
      }

      /// <summary>
      /// Implements <see cref="AbstractTargetFrameworkMapper{TTypesDic, TTargetFWAssembliesDic, TResolvedTargetFWAssembliesDic, TResolvedTargetFWAssembliesDicInner, TAssemblyReferenceDic, TAssemblyReferenceDicInner}.GetOrAdd_MDTypes"/> by calling <see cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, Func{TKey, TValue})"/>.
      /// </summary>
      /// <param name="dic">The dictionary.</param>
      /// <param name="key">The key to dictionary.</param>
      /// <param name="factory">The factory callback, if the value is not present for given <paramref name="key"/>.</param>
      /// <returns>The existing or created value.</returns>
      protected override ISet<String> GetOrAdd_MDTypes( ConcurrentDictionary<CILMetaData, ISet<String>> dic, CILMetaData key, Func<CILMetaData, ISet<String>> factory )
      {
         return dic.GetOrAdd( key, factory );
      }

      /// <summary>
      /// Implements <see cref="AbstractTargetFrameworkMapper{TTypesDic, TTargetFWAssembliesDic, TResolvedTargetFWAssembliesDic, TResolvedTargetFWAssembliesDicInner, TAssemblyReferenceDic, TAssemblyReferenceDicInner}.GetOrAdd_ResolvedTargetFWAssemblies"/> by calling <see cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, Func{TKey, TValue})"/>.
      /// </summary>
      /// <param name="dic">The dictionary.</param>
      /// <param name="key">The key to dictionary.</param>
      /// <param name="factory">The factory callback, if the value is not present for given <paramref name="key"/>.</param>
      /// <returns>The existing or created value.</returns>
      protected override ConcurrentDictionary<String, CILMetaData> GetOrAdd_ResolvedTargetFWAssemblies( ConcurrentDictionary<CILMetaData, ConcurrentDictionary<String, CILMetaData>> dic, CILMetaData key, Func<CILMetaData, ConcurrentDictionary<String, CILMetaData>> factory )
      {
         return dic.GetOrAdd( key, factory );
      }

      /// <summary>
      /// Implements <see cref="AbstractTargetFrameworkMapper{TTypesDic, TTargetFWAssembliesDic, TResolvedTargetFWAssembliesDic, TResolvedTargetFWAssembliesDicInner, TAssemblyReferenceDic, TAssemblyReferenceDicInner}.GetOrAdd_ResolvedTargetFWAssembliesInner"/> by calling <see cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, Func{TKey, TValue})"/>.
      /// </summary>
      /// <param name="dic">The dictionary.</param>
      /// <param name="key">The key to dictionary.</param>
      /// <param name="factory">The factory callback, if the value is not present for given <paramref name="key"/>.</param>
      /// <returns>The existing or created value.</returns>
      protected override CILMetaData GetOrAdd_ResolvedTargetFWAssembliesInner( ConcurrentDictionary<String, CILMetaData> dic, String key, Func<String, CILMetaData> factory )
      {
         return dic.GetOrAdd( key, factory );
      }

      /// <summary>
      /// Implements <see cref="AbstractTargetFrameworkMapper{TTypesDic, TTargetFWAssembliesDic, TResolvedTargetFWAssembliesDic, TResolvedTargetFWAssembliesDicInner, TAssemblyReferenceDic, TAssemblyReferenceDicInner}.GetOrAdd_AssemblyReferences"/> by calling <see cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, Func{TKey, TValue})"/>.
      /// </summary>
      /// <param name="dic">The dictionary.</param>
      /// <param name="key">The key to dictionary.</param>
      /// <param name="factory">The factory callback, if the value is not present for given <paramref name="key"/>.</param>
      /// <returns>The existing or created value.</returns>
      protected override ConcurrentDictionary<AssemblyInformationForResolving, CILMetaData> GetOrAdd_AssemblyReferences( ConcurrentDictionary<CILMetaData, ConcurrentDictionary<AssemblyInformationForResolving, CILMetaData>> dic, CILMetaData key, Func<CILMetaData, ConcurrentDictionary<AssemblyInformationForResolving, CILMetaData>> factory )
      {
         return dic.GetOrAdd( key, factory );
      }

      /// <summary>
      /// Implements <see cref="AbstractTargetFrameworkMapper{TTypesDic, TTargetFWAssembliesDic, TResolvedTargetFWAssembliesDic, TResolvedTargetFWAssembliesDicInner, TAssemblyReferenceDic, TAssemblyReferenceDicInner}.GetOrAdd_AssemblyReferencesInner"/> by calling <see cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, Func{TKey, TValue})"/>.
      /// </summary>
      /// <param name="dic">The dictionary.</param>
      /// <param name="key">The key to dictionary.</param>
      /// <param name="factory">The factory callback, if the value is not present for given <paramref name="key"/>.</param>
      /// <returns>The existing or created value.</returns>
      protected override CILMetaData GetOrAdd_AssemblyReferencesInner( ConcurrentDictionary<AssemblyInformationForResolving, CILMetaData> dic, AssemblyInformationForResolving key, Func<AssemblyInformationForResolving, CILMetaData> factory )
      {
         return dic.GetOrAdd( key, factory );
      }

      /// <summary>
      /// Implements <see cref="AbstractTargetFrameworkMapper{TTypesDic, TTargetFWAssembliesDic, TResolvedTargetFWAssembliesDic, TResolvedTargetFWAssembliesDicInner, TAssemblyReferenceDic, TAssemblyReferenceDicInner}.RecordNotManagedAssembly"/> by using <see cref="ConcurrentDictionary{TKey, TValue}"/> as a concurrent set (same key and value type, and same object acting as both key and value).
      /// </summary>
      /// <param name="resource">The resource (path) for the assembly which was detected to be unmanaged assembly.</param>
      protected override void RecordNotManagedAssembly( String resource )
      {
         this._notManagedAssemblies.TryAdd( resource, resource );
      }

      /// <summary>
      /// Implements <see cref="AbstractTargetFrameworkMapper{TTypesDic, TTargetFWAssembliesDic, TResolvedTargetFWAssembliesDic, TResolvedTargetFWAssembliesDicInner, TAssemblyReferenceDic, TAssemblyReferenceDicInner}.IsRecordedNotManagedAssembly"/> by using <see cref="ConcurrentDictionary{TKey, TValue}"/> as a concurrent set (same key and value type, and same object acting as both key and value).
      /// </summary>
      /// <param name="resource">The resource (path) to check.</param>
      /// <returns><c>true</c> if the assembly at given <paramref name="resource"/> was previously recorded to be as unmanaged assembly; <c>false</c> otherwise.</returns>
      protected override bool IsRecordedNotManagedAssembly( String resource )
      {
         return this._notManagedAssemblies.ContainsKey( resource );
      }
   }
}
#endif