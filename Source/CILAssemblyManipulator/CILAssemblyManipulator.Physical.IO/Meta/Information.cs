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
extern alias CAMPhysicalR;
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;
using CAMPhysical::CILAssemblyManipulator.Physical.Meta;

using CAMPhysicalR;
using CAMPhysicalR::CILAssemblyManipulator.Physical.Resolving;

using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CILAssemblyManipulator.Physical.IO;
using TabularMetaData.Meta;

namespace CILAssemblyManipulator.Physical.Meta
{
   public class DefaultMetaDataTableInformationProvider
   {
      public static ArrayQuery<Int32?> TypeDefOrRef { get; }

      public static ArrayQuery<Int32?> HasConstant { get; }

      public static ArrayQuery<Int32?> HasCustomAttribute { get; }

      public static ArrayQuery<Int32?> HasFieldMarshal { get; }

      public static ArrayQuery<Int32?> HasSecurity { get; }

      public static ArrayQuery<Int32?> MemberRefParent { get; }

      public static ArrayQuery<Int32?> HasSemantics { get; }

      public static ArrayQuery<Int32?> MethodDefOrRef { get; }

      public static ArrayQuery<Int32?> MemberForwarded { get; }

      public static ArrayQuery<Int32?> Implementation { get; }

      public static ArrayQuery<Int32?> CustomAttributeType { get; }

      public static ArrayQuery<Int32?> ResolutionScope { get; }

      public static ArrayQuery<Int32?> TypeOrMethodDef { get; }

      static DefaultMetaDataTableInformationProvider()
      {
         var cf = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY;
         // ECMA-335, pp. 274-276
         TypeDefOrRef = cf.NewArrayProxy( new Int32?[] { (Int32) Tables.TypeDef, (Int32) Tables.TypeRef, (Int32) Tables.TypeSpec } ).CQ;
         HasConstant = cf.NewArrayProxy( new Int32?[] { (Int32) Tables.Field, (Int32) Tables.Parameter, (Int32) Tables.Property } ).CQ;
         HasCustomAttribute = cf.NewArrayProxy( new Int32?[] { ( Int32)Tables.MethodDef, ( Int32)Tables.Field, ( Int32)Tables.TypeRef, ( Int32)Tables.TypeDef, ( Int32)Tables.Parameter,
            ( Int32)Tables.InterfaceImpl, ( Int32)Tables.MemberRef, ( Int32)Tables.Module, ( Int32)Tables.DeclSecurity, ( Int32)Tables.Property, ( Int32)Tables.Event,
            ( Int32)Tables.StandaloneSignature, ( Int32)Tables.ModuleRef, ( Int32)Tables.TypeSpec, ( Int32)Tables.Assembly, ( Int32)Tables.AssemblyRef, ( Int32)Tables.File,
            ( Int32)Tables.ExportedType, ( Int32)Tables.ManifestResource, ( Int32)Tables.GenericParameter, ( Int32)Tables.GenericParameterConstraint, ( Int32)Tables.MethodSpec } ).CQ;
         HasFieldMarshal = cf.NewArrayProxy( new Int32?[] { (Int32) Tables.Field, (Int32) Tables.Parameter } ).CQ;
         HasSecurity = cf.NewArrayProxy( new Int32?[] { (Int32) Tables.TypeDef, (Int32) Tables.MethodDef, (Int32) Tables.Assembly } ).CQ;
         MemberRefParent = cf.NewArrayProxy( new Int32?[] { (Int32) Tables.TypeDef, (Int32) Tables.TypeRef, (Int32) Tables.ModuleRef, (Int32) Tables.MethodDef, (Int32) Tables.TypeSpec } ).CQ;
         HasSemantics = cf.NewArrayProxy( new Int32?[] { (Int32) Tables.Event, (Int32) Tables.Property } ).CQ;
         MethodDefOrRef = cf.NewArrayProxy( new Int32?[] { (Int32) Tables.MethodDef, (Int32) Tables.MemberRef } ).CQ;
         MemberForwarded = cf.NewArrayProxy( new Int32?[] { (Int32) Tables.Field, (Int32) Tables.MethodDef } ).CQ;
         Implementation = cf.NewArrayProxy( new Int32?[] { (Int32) Tables.File, (Int32) Tables.AssemblyRef, (Int32) Tables.ExportedType } ).CQ;
         CustomAttributeType = cf.NewArrayProxy( new Int32?[] { null, null, (Int32) Tables.MethodDef, (Int32) Tables.MemberRef, null } ).CQ;
         ResolutionScope = cf.NewArrayProxy( new Int32?[] { (Int32) Tables.Module, (Int32) Tables.ModuleRef, (Int32) Tables.AssemblyRef, (Int32) Tables.TypeRef } ).CQ;
         TypeOrMethodDef = cf.NewArrayProxy( new Int32?[] { (Int32) Tables.TypeDef, (Int32) Tables.MethodDef } ).CQ;
      }
   }
}