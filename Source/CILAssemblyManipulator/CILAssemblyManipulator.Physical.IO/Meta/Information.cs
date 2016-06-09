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

using UtilPack.CollectionsWithRoles;
using UtilPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CILAssemblyManipulator.Physical.IO;
using TabularMetaData.Meta;

namespace CILAssemblyManipulator.Physical.Meta
{
#pragma warning disable 1591
   // This class will be documented in IO.Defaults project.
   public class DefaultMetaDataTableInformationProvider
#pragma warning restore 1591
   {
      /// <summary>
      /// Gets the possible tables, as nullable integers, for <c>TypeDefOrRef</c> kind of table index schema.
      /// </summary>
      /// <value>The possible tables, as nullable integers, for <c>TypeDefOrRef</c> kind of table index schema.</value>
      /// <remarks>
      /// <para>
      /// The schema means that the <see cref="TableIndex"/> of the value of the row object in question should only have its <see cref="TableIndex.Table"/> from one of the possible tables of this schema.
      /// The order of the tables matters, since the <see cref="TableIndex"/> objects with same <see cref="TableIndex.Index"/> property will be indexed in same order as the tables in this schema.
      /// </para>
      /// <para>
      /// This table schema, <c>TypeDefOrRef</c>, allows the <see cref="TableIndex.Table"/> property to be one of the following:
      /// <list type="number">
      /// <item><description><see cref="Tables.TypeDef"/>,</description></item>
      /// <item><description><see cref="Tables.TypeRef"/>, and</description></item>
      /// <item><description><see cref="Tables.TypeSpec"/>.</description></item>
      /// </list>
      /// </para>
      /// <para>
      /// This table schema is used by the following properties:
      /// <list type="bullet">
      /// <item><description><see cref="TypeDefinition.BaseType"/>,</description></item>
      /// <item><description><see cref="InterfaceImplementation.Interface"/>,</description></item>
      /// <item><description><see cref="EventDefinition.EventType"/>, and</description></item>
      /// <item><description><see cref="GenericParameterConstraintDefinition.Constraint"/>.</description></item>
      /// </list>
      /// </para>
      /// </remarks>
      public static ArrayQuery<Int32?> TypeDefOrRef { get; }

      /// <summary>
      /// Gets the possible tables, as nullable integers, for <c>HasConstant</c> kind of table index schema.
      /// </summary>
      /// <value>The possible tables, as nullable integers, for <c>HasConstant</c> kind of table index schema.</value>
      /// <remarks>
      /// <para>
      /// The schema means that the <see cref="TableIndex"/> of the value of the row object in question should only have its <see cref="TableIndex.Table"/> from one of the possible tables of this schema.
      /// The order of the tables matters, since the <see cref="TableIndex"/> objects with same <see cref="TableIndex.Index"/> property will be indexed in same order as the tables in this schema.
      /// </para>
      /// <para>
      /// This table schema, <c>HasConstant</c>, allows the <see cref="TableIndex.Table"/> property to be one of the following:
      /// <list type="number">
      /// <item><description><see cref="Tables.Field"/>,</description></item>
      /// <item><description><see cref="Tables.Parameter"/>, and</description></item>
      /// <item><description><see cref="Tables.Property"/>.</description></item>
      /// </list>
      /// </para>
      /// <para>
      /// This table schema is used by the following properties:
      /// <list type="bullet">
      /// <item><description><see cref="ConstantDefinition.Parent"/>.</description></item>
      /// </list>
      /// </para>
      /// </remarks>
      public static ArrayQuery<Int32?> HasConstant { get; }

      /// <summary>
      /// Gets the possible tables, as nullable integers, for <c>HasCustomAttribute</c> kind of table index schema.
      /// </summary>
      /// <value>The possible tables, as nullable integers, for <c>HasCustomAttribute</c> kind of table index schema.</value>
      /// <remarks>
      /// <para>
      /// The schema means that the <see cref="TableIndex"/> of the value of the row object in question should only have its <see cref="TableIndex.Table"/> from one of the possible tables of this schema.
      /// The order of the tables matters, since the <see cref="TableIndex"/> objects with same <see cref="TableIndex.Index"/> property will be indexed in same order as the tables in this schema.
      /// </para>
      /// <para>
      /// This table schema, <c>HasCustomAttribute</c>, allows the <see cref="TableIndex.Table"/> property to be one of the following:
      /// <list type="number">
      /// <item><description><see cref="Tables.MethodDef"/>,</description></item>
      /// <item><description><see cref="Tables.Field"/>,</description></item>
      /// <item><description><see cref="Tables.TypeRef"/>,</description></item>
      /// <item><description><see cref="Tables.TypeDef"/>,</description></item>
      /// <item><description><see cref="Tables.Parameter"/>,</description></item>
      /// <item><description><see cref="Tables.InterfaceImpl"/>,</description></item>
      /// <item><description><see cref="Tables.MemberRef"/>,</description></item>
      /// <item><description><see cref="Tables.Module"/>,</description></item>
      /// <item><description><see cref="Tables.DeclSecurity"/>,</description></item>
      /// <item><description><see cref="Tables.Property"/>,</description></item>
      /// <item><description><see cref="Tables.Event"/>,</description></item>
      /// <item><description><see cref="Tables.StandaloneSignature"/>,</description></item>
      /// <item><description><see cref="Tables.ModuleRef"/>,</description></item>
      /// <item><description><see cref="Tables.TypeSpec"/>,</description></item>
      /// <item><description><see cref="Tables.Assembly"/>,</description></item>
      /// <item><description><see cref="Tables.AssemblyRef"/>,</description></item>
      /// <item><description><see cref="Tables.File"/>,</description></item>
      /// <item><description><see cref="Tables.ExportedType"/>,</description></item>
      /// <item><description><see cref="Tables.ManifestResource"/>,</description></item>
      /// <item><description><see cref="Tables.GenericParameter"/>, and</description></item>
      /// <item><description><see cref="Tables.GenericParameterConstraint"/>, and</description></item>
      /// <item><description><see cref="Tables.MethodSpec"/>.</description></item>
      /// </list>
      /// </para>
      /// <para>
      /// This table schema is used by the following properties:
      /// <list type="bullet">
      /// <item><description><see cref="CustomAttributeDefinition.Parent"/>.</description></item>
      /// </list>
      /// </para>
      /// </remarks>
      public static ArrayQuery<Int32?> HasCustomAttribute { get; }

      /// <summary>
      /// Gets the possible tables, as nullable integers, for <c>HasFieldMarshal</c> kind of table index schema.
      /// </summary>
      /// <value>The possible tables, as nullable integers, for <c>HasFieldMarshal</c> kind of table index schema.</value>
      /// <remarks>
      /// <para>
      /// The schema means that the <see cref="TableIndex"/> of the value of the row object in question should only have its <see cref="TableIndex.Table"/> from one of the possible tables of this schema.
      /// The order of the tables matters, since the <see cref="TableIndex"/> objects with same <see cref="TableIndex.Index"/> property will be indexed in same order as the tables in this schema.
      /// </para>
      /// <para>
      /// This table schema, <c>HasFieldMarshal</c>, allows the <see cref="TableIndex.Table"/> property to be one of the following:
      /// <list type="number">
      /// <item><description><see cref="Tables.Field"/>, and</description></item>
      /// <item><description><see cref="Tables.Parameter"/>.</description></item>
      /// </list>
      /// </para>
      /// <para>
      /// This table schema is used by the following properties:
      /// <list type="bullet">
      /// <item><description><see cref="FieldMarshal.Parent"/>.</description></item>
      /// </list>
      /// </para>
      /// </remarks>
      public static ArrayQuery<Int32?> HasFieldMarshal { get; }

      /// <summary>
      /// Gets the possible tables, as nullable integers, for <c>HasSecurity</c> kind of table index schema.
      /// </summary>
      /// <value>The possible tables, as nullable integers, for <c>HasSecurity</c> kind of table index schema.</value>
      /// <remarks>
      /// <para>
      /// The schema means that the <see cref="TableIndex"/> of the value of the row object in question should only have its <see cref="TableIndex.Table"/> from one of the possible tables of this schema.
      /// The order of the tables matters, since the <see cref="TableIndex"/> objects with same <see cref="TableIndex.Index"/> property will be indexed in same order as the tables in this schema.
      /// </para>
      /// <para>
      /// This table schema, <c>HasSecurity</c>, allows the <see cref="TableIndex.Table"/> property to be one of the following:
      /// <list type="number">
      /// <item><description><see cref="Tables.TypeDef"/>,</description></item>
      /// <item><description><see cref="Tables.MethodDef"/>, and</description></item>
      /// <item><description><see cref="Tables.Assembly"/>.</description></item>
      /// </list>
      /// </para>
      /// <para>
      /// This table schema is used by the following properties:
      /// <list type="bullet">
      /// <item><description><see cref="SecurityDefinition.Parent"/>.</description></item>
      /// </list>
      /// </para>
      /// </remarks>
      public static ArrayQuery<Int32?> HasSecurity { get; }

      /// <summary>
      /// Gets the possible tables, as nullable integers, for <c>MemberRefParent</c> kind of table index schema.
      /// </summary>
      /// <value>The possible tables, as nullable integers, for <c>MemberRefParent</c> kind of table index schema.</value>
      /// <remarks>
      /// <para>
      /// The schema means that the <see cref="TableIndex"/> of the value of the row object in question should only have its <see cref="TableIndex.Table"/> from one of the possible tables of this schema.
      /// The order of the tables matters, since the <see cref="TableIndex"/> objects with same <see cref="TableIndex.Index"/> property will be indexed in same order as the tables in this schema.
      /// </para>
      /// <para>
      /// This table schema, <c>MemberRefParent</c>, allows the <see cref="TableIndex.Table"/> property to be one of the following:
      /// <list type="number">
      /// <item><description><see cref="Tables.TypeDef"/>,</description></item>
      /// <item><description><see cref="Tables.TypeRef"/>,</description></item>
      /// <item><description><see cref="Tables.ModuleRef"/>,</description></item>
      /// <item><description><see cref="Tables.MethodDef"/>, and</description></item>
      /// <item><description><see cref="Tables.TypeSpec"/>.</description></item>
      /// </list>
      /// </para>
      /// <para>
      /// This table schema is used by the following properties:
      /// <list type="bullet">
      /// <item><description><see cref="MemberReference.DeclaringType"/>.</description></item>
      /// </list>
      /// </para>
      /// </remarks>
      public static ArrayQuery<Int32?> MemberRefParent { get; }

      /// <summary>
      /// Gets the possible tables, as nullable integers, for <c>HasSemantics</c> kind of table index schema.
      /// </summary>
      /// <value>The possible tables, as nullable integers, for <c>HasSemantics</c> kind of table index schema.</value>
      /// <remarks>
      /// <para>
      /// The schema means that the <see cref="TableIndex"/> of the value of the row object in question should only have its <see cref="TableIndex.Table"/> from one of the possible tables of this schema.
      /// The order of the tables matters, since the <see cref="TableIndex"/> objects with same <see cref="TableIndex.Index"/> property will be indexed in same order as the tables in this schema.
      /// </para>
      /// <para>
      /// This table schema, <c>HasSemantics</c>, allows the <see cref="TableIndex.Table"/> property to be one of the following:
      /// <list type="number">
      /// <item><description><see cref="Tables.Event"/>, and</description></item>
      /// <item><description><see cref="Tables.Property"/>.</description></item>
      /// </list>
      /// </para>
      /// </remarks>
      /// <para>
      /// This table schema is used by the following properties:
      /// <list type="bullet">
      /// <item><description><see cref="MethodSemantics.Associaton"/>.</description></item>
      /// </list>
      /// </para>
      public static ArrayQuery<Int32?> HasSemantics { get; }

      /// <summary>
      /// Gets the possible tables, as nullable integers, for <c>MethodDefOrRef</c> kind of table index schema.
      /// </summary>
      /// <value>The possible tables, as nullable integers, for <c>MethodDefOrRef</c> kind of table index schema.</value>
      /// <remarks>
      /// <para>
      /// The schema means that the <see cref="TableIndex"/> of the value of the row object in question should only have its <see cref="TableIndex.Table"/> from one of the possible tables of this schema.
      /// The order of the tables matters, since the <see cref="TableIndex"/> objects with same <see cref="TableIndex.Index"/> property will be indexed in same order as the tables in this schema.
      /// </para>
      /// <para>
      /// This table schema, <c>MethodDefOrRef</c>, allows the <see cref="TableIndex.Table"/> property to be one of the following:
      /// <list type="number">
      /// <item><description><see cref="Tables.MethodDef"/>, and</description></item>
      /// <item><description><see cref="Tables.MemberRef"/>.</description></item>
      /// </list>
      /// </para>
      /// <para>
      /// This table schema is used by the following properties:
      /// <list type="bullet">
      /// <item><description><see cref="MethodImplementation.MethodBody"/>,</description></item>
      /// <item><description><see cref="MethodImplementation.MethodDeclaration"/>, and</description></item>
      /// <item><description><see cref="MethodSpecification.Method"/>.</description></item>
      /// </list>
      /// </para>
      /// </remarks>
      public static ArrayQuery<Int32?> MethodDefOrRef { get; }

      /// <summary>
      /// Gets the possible tables, as nullable integers, for <c>MemberForwarded</c> kind of table index schema.
      /// </summary>
      /// <value>The possible tables, as nullable integers, for <c>MemberForwarded</c> kind of table index schema.</value>
      /// <remarks>
      /// <para>
      /// The schema means that the <see cref="TableIndex"/> of the value of the row object in question should only have its <see cref="TableIndex.Table"/> from one of the possible tables of this schema.
      /// The order of the tables matters, since the <see cref="TableIndex"/> objects with same <see cref="TableIndex.Index"/> property will be indexed in same order as the tables in this schema.
      /// </para>
      /// <para>
      /// This table schema, <c>MemberForwarded</c>, allows the <see cref="TableIndex.Table"/> property to be one of the following:
      /// <list type="number">
      /// <item><description><see cref="Tables.Field"/>, and</description></item>
      /// <item><description><see cref="Tables.MethodDef"/>.</description></item>
      /// </list>
      /// </para>
      /// <para>
      /// This table schema is used by the following properties:
      /// <list type="bullet">
      /// <item><description><see cref="MethodImplementationMap.MemberForwarded"/>.</description></item>
      /// </list>
      /// </para>
      /// </remarks>
      public static ArrayQuery<Int32?> MemberForwarded { get; }

      /// <summary>
      /// Gets the possible tables, as nullable integers, for <c>Implementation</c> kind of table index schema.
      /// </summary>
      /// <value>The possible tables, as nullable integers, for <c>Implementation</c> kind of table index schema.</value>
      /// <remarks>
      /// <para>
      /// The schema means that the <see cref="TableIndex"/> of the value of the row object in question should only have its <see cref="TableIndex.Table"/> from one of the possible tables of this schema.
      /// The order of the tables matters, since the <see cref="TableIndex"/> objects with same <see cref="TableIndex.Index"/> property will be indexed in same order as the tables in this schema.
      /// </para>
      /// <para>
      /// This table schema, <c>Implementation</c>, allows the <see cref="TableIndex.Table"/> property to be one of the following:
      /// <list type="number">
      /// <item><description><see cref="Tables.File"/>,</description></item>
      /// <item><description><see cref="Tables.AssemblyRef"/>, and</description></item>
      /// <item><description><see cref="Tables.ExportedType"/>.</description></item>
      /// </list>
      /// </para>
      /// <para>
      /// This table schema is used by the following properties:
      /// <list type="bullet">
      /// <item><description><see cref="ExportedType.Implementation"/>, and</description></item>
      /// <item><description><see cref="ManifestResource.Implementation"/>.</description></item>
      /// </list>
      /// </para>
      /// </remarks>
      public static ArrayQuery<Int32?> Implementation { get; }

      /// <summary>
      /// Gets the possible tables, as nullable integers, for <c>CustomAttributeType</c> kind of table index schema.
      /// </summary>
      /// <value>The possible tables, as nullable integers, for <c>CustomAttributeType</c> kind of table index schema.</value>
      /// <remarks>
      /// <para>
      /// The schema means that the <see cref="TableIndex"/> of the value of the row object in question should only have its <see cref="TableIndex.Table"/> from one of the possible tables of this schema.
      /// The order of the tables matters, since the <see cref="TableIndex"/> objects with same <see cref="TableIndex.Index"/> property will be indexed in same order as the tables in this schema.
      /// </para>
      /// <para>
      /// This table schema, <c>CustomAttributeType</c>, allows the <see cref="TableIndex.Table"/> property to be one of the following:
      /// <list type="number">
      /// <item><description>unused value,</description></item>
      /// <item><description>unused value,</description></item>
      /// <item><description><see cref="Tables.MethodDef"/>,</description></item>
      /// <item><description><see cref="Tables.MemberRef"/>, and</description></item>
      /// <item><description>unused value.</description></item>
      /// </list>
      /// </para>
      /// <para>
      /// This table schema is used by the following properties:
      /// <list type="bullet">
      /// <item><description><see cref="CustomAttributeDefinition.Type"/>.</description></item>
      /// </list>
      /// </para>
      /// </remarks>
      public static ArrayQuery<Int32?> CustomAttributeType { get; }

      /// <summary>
      /// Gets the possible tables, as nullable integers, for <c>ResolutionScope</c> kind of table index schema.
      /// </summary>
      /// <value>The possible tables, as nullable integers, for <c>ResolutionScope</c> kind of table index schema.</value>
      /// <remarks>
      /// <para>
      /// The schema means that the <see cref="TableIndex"/> of the value of the row object in question should only have its <see cref="TableIndex.Table"/> from one of the possible tables of this schema.
      /// The order of the tables matters, since the <see cref="TableIndex"/> objects with same <see cref="TableIndex.Index"/> property will be indexed in same order as the tables in this schema.
      /// </para>
      /// <para>
      /// This table schema, <c>ResolutionScope</c>, allows the <see cref="TableIndex.Table"/> property to be one of the following:
      /// <list type="number">
      /// <item><description><see cref="Tables.Module"/>,</description></item>
      /// <item><description><see cref="Tables.ModuleRef"/>,</description></item>
      /// <item><description><see cref="Tables.AssemblyRef"/>, and</description></item>
      /// <item><description><see cref="Tables.TypeRef"/>.</description></item>
      /// </list>
      /// </para>
      /// <para>
      /// This table schema is used by the following properties:
      /// <list type="bullet">
      /// <item><description><see cref="TypeReference.ResolutionScope"/>.</description></item>
      /// </list>
      /// </para>
      /// </remarks>
      public static ArrayQuery<Int32?> ResolutionScope { get; }

      /// <summary>
      /// Gets the possible tables, as nullable integers, for <c>TypeOrMethodDef</c> kind of table index schema.
      /// </summary>
      /// <value>The possible tables, as nullable integers, for <c>TypeOrMethodDef</c> kind of table index schema.</value>
      /// <remarks>
      /// <para>
      /// The schema means that the <see cref="TableIndex"/> of the value of the row object in question should only have its <see cref="TableIndex.Table"/> from one of the possible tables of this schema.
      /// The order of the tables matters, since the <see cref="TableIndex"/> objects with same <see cref="TableIndex.Index"/> property will be indexed in same order as the tables in this schema.
      /// </para>
      /// <para>
      /// This table schema, <c>TypeOrMethodDef</c>, allows the <see cref="TableIndex.Table"/> property to be one of the following:
      /// <list type="number">
      /// <item><description><see cref="Tables.TypeDef"/>, and</description></item>
      /// <item><description><see cref="Tables.MethodDef"/>.</description></item>
      /// </list>
      /// </para>
      /// <para>
      /// This table schema is used by the following properties:
      /// <list type="bullet">
      /// <item><description><see cref="GenericParameterDefinition.Owner"/>.</description></item>
      /// </list>
      /// </para>
      /// </remarks>
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