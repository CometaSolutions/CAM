/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
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
using System;
using CILAssemblyManipulator.Logical;
using CILAssemblyManipulator.Logical.Implementation;
using UtilPack;
using CILAssemblyManipulator.Physical;

namespace CILAssemblyManipulator.Logical
{
   /// <summary>
   /// This interface represents a field in CIL environment. The interface roughly corresponds to <see cref="System.Reflection.FieldInfo"/>. See ECMA specification for more information about CIL fields.
   /// </summary>
   public interface CILField :
      CILCustomAttributeContainer,
      CILElementWithSimpleName,
      CILElementOwnedByChangeableType<CILField>,
      CILElementForNamedCustomAttribute,
      CILElementWithAttributes<FieldAttributes>,
      CILElementWithConstant,
      CILElementInstantiable,
      CILElementWithCustomModifiers,
      CILElementWithContext,
      CILElementWithMarshalingInfo,
      CILElementTokenizableInILCode
   {
      /// <summary>
      /// Gets or sets the type of this field.
      /// </summary>
      /// <value>The type of this field.</value>
      /// <exception cref="NotSupportedException">For setter only. The exception is thrown when <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c>, meaning the <see cref="CILElementOwnedByType.DeclaringType"/> is a generic type but not generic type definition.</exception>
      /// <seealso cref="System.Reflection.FieldInfo.FieldType"/>
      CILTypeBase FieldType { get; set; }

      /// <summary>
      /// Get or sets the initial value of this field, as byte array. If field has no initial value, this property is <c>null</c>.
      /// </summary>
      /// <value>The initial value of this field.</value>
      /// <exception cref="NotSupportedException">For setter only. The exception is thrown when <see cref="CILElementInstantiable.IsTrueDefinition"/> returns <c>false</c>, meaning the <see cref="CILElementOwnedByType.DeclaringType"/> is a generic type but not generic type definition.</exception>
      /// <remarks>Setting this to non-null causes field to have Relative Virtual Address (RVA) during emitting. Make sure that field attributes have <see cref="FieldAttributes.HasFieldRVA"/> set. See ECMA specification for more information about fields with RVA.</remarks>
      Byte[] InitialValue { get; set; }

      /// <summary>
      /// Gets or sets the offet of this field. For more information about field offsets, see ECMA specification.
      /// </summary>
      /// <value>The offset of this field, in bytes.</value>
      Int32 FieldOffset { get; set; }
   }
}