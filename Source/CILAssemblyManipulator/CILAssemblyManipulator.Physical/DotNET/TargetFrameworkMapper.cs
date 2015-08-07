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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
   public class TargetFrameworkMapperConcurrent : AbstractTargetFrameworkMapper<
      ConcurrentDictionary<CILMetaData, ISet<String>>,
      ConcurrentDictionary<TargetFrameworkInfo, String[]>,
      ConcurrentDictionary<CILMetaData, ConcurrentDictionary<String, CILMetaData>>,
      ConcurrentDictionary<String, CILMetaData>,
      ConcurrentDictionary<CILMetaData, ConcurrentDictionary<AssemblyInformationForResolving, CILMetaData>>,
      ConcurrentDictionary<AssemblyInformationForResolving, CILMetaData>
      >
   {

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
      }

      protected override String[] GetOrAdd_TargetFWAssemblies( ConcurrentDictionary<TargetFrameworkInfo, String[]> dic, TargetFrameworkInfo key, Func<TargetFrameworkInfo, String[]> factory )
      {
         return dic.GetOrAdd( key, factory );
      }

      protected override ISet<String> GetOrAdd_MDTypes( ConcurrentDictionary<CILMetaData, ISet<String>> dic, CILMetaData key, Func<CILMetaData, ISet<String>> factory )
      {
         return dic.GetOrAdd( key, factory );
      }

      protected override ConcurrentDictionary<String, CILMetaData> GetOrAdd_ResolvedTargetFWAssemblies( ConcurrentDictionary<CILMetaData, ConcurrentDictionary<String, CILMetaData>> dic, CILMetaData key, Func<CILMetaData, ConcurrentDictionary<String, CILMetaData>> factory )
      {
         return dic.GetOrAdd( key, factory );
      }

      protected override CILMetaData GetOrAdd_ResolvedTargetFWAssembliesInner( ConcurrentDictionary<String, CILMetaData> dic, String key, Func<String, CILMetaData> factory )
      {
         return dic.GetOrAdd( key, factory );
      }

      protected override ConcurrentDictionary<AssemblyInformationForResolving, CILMetaData> GetOrAdd_AssemblyReferences( ConcurrentDictionary<CILMetaData, ConcurrentDictionary<AssemblyInformationForResolving, CILMetaData>> dic, CILMetaData key, Func<CILMetaData, ConcurrentDictionary<AssemblyInformationForResolving, CILMetaData>> factory )
      {
         return dic.GetOrAdd( key, factory );
      }

      protected override CILMetaData GetOrAdd_AssemblyReferencesInner( ConcurrentDictionary<AssemblyInformationForResolving, CILMetaData> dic, AssemblyInformationForResolving key, Func<AssemblyInformationForResolving, CILMetaData> factory )
      {
         return dic.GetOrAdd( key, factory );
      }
   }
}
#endif