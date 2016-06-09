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
using CAMPhysical::CILAssemblyManipulator.Physical.Meta;

using CILAssemblyManipulator.Physical;
using UtilPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TabularMetaData;

namespace CILAssemblyManipulator.Physical.Resolving
{

   /// <summary>
   /// This interface provides methods for resolving type names and <see cref="TableIndex"/> objects, in order to properly process structural information about types in different assemblies.
   /// </summary>
   public interface MetaDataResolver
   {
      /// <summary>
      /// Tries to resolve given <see cref="TableIndex"/> pointing to <see cref="Tables.TypeDef"/>, <see cref="Tables.TypeRef"/> or <see cref="Tables.TypeSpec"/> table, into a full type string.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/> containing the <see cref="TableIndex"/>.</param>
      /// <param name="index">The <see cref="TableIndex"/>.</param>
      /// <returns>Resolved type string, or <c>null</c> if resolving failed.</returns>
      String ResolveTypeNameFromTypeDefOrRefOrSpec( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData md, TableIndex index );

      /// <summary>
      /// Tries to resolve a given full type string within given <see cref="CILMetaData"/> into a index to <see cref="Tables.TypeDef"/> table within another <see cref="CILMetaData"/>.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/> containing the type string.</param>
      /// <param name="fullTypeString">The full, possibly assembly-qualified type string.</param>
      /// <param name="otherMD">If succeeded, this parameter will hold the instance of <see cref="CILMetaData"/> where the <see cref="TypeDefinition"/> of the given <paramref name="fullTypeString"/> is.</param>
      /// <param name="typeDefIndex">If succeeded, this parameter will hold the index to <see cref="CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData.TypeDefinitions"/> table where the <see cref="TypeDefinition"/> of the given <paramref name="fullTypeString"/> is.</param>
      /// <returns><c>true</c> if resolving succeeds; <c>false</c> otherwise.</returns>
      Boolean TryResolveTypeString( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData md, String fullTypeString, out CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData otherMD, out Int32 typeDefIndex );

      /// <summary>
      /// Tries to resolve given <see cref="TableIndex"/> pointing to <see cref="Tables.TypeDef"/>, <see cref="Tables.TypeRef"/> or <see cref="Tables.TypeSpec"/> table, into a <see cref="Tables.TypeDef"/> index into other meta data.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/> containing the <see cref="TableIndex"/>.</param>
      /// <param name="index">The <see cref="TableIndex"/>.</param>
      /// <param name="otherMD">If successful, this parameter will be <see cref="CILMetaData"/> that contains <see cref="TypeDefinition"/> corresponding to given <paramref name="index"/>.</param>
      /// <param name="typeDefIndex">If successful, this parameter will be the index to <see cref="CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData.TypeDefinitions"/> table containing the <see cref="TypeDefinition"/> corresponding to given <paramref name="index"/>.</param>
      /// <returns><c>true</c> if resolving succeeds; <c>False</c> otherwise.</returns>
      Boolean TryResolveTypeDefOrRefOrSpec( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData md, TableIndex index, out CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData otherMD, out Int32 typeDefIndex );

      /// <summary>
      /// This event will be fired when an assembly reference will need to be resolved.
      /// </summary>
      /// <seealso cref="AssemblyReferenceResolveEventArgs"/>
      event EventHandler<AssemblyReferenceResolveEventArgs> AssemblyReferenceResolveEvent;

      /// <summary>
      /// This event will be fired when a module reference will need to be resolved.
      /// </summary>
      /// <seealso cref="ModuleReferenceResolveEventArgs"/>
      event EventHandler<ModuleReferenceResolveEventArgs> ModuleReferenceResolveEvent;
   }


}

