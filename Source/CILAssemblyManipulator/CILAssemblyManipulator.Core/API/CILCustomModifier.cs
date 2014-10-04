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
using CILAssemblyManipulator.Implementation;

namespace CILAssemblyManipulator.API
{
   /// <summary>
   /// This interface represents custom modifier in CIL. See ECMA specification for more information about custom modifiers.
   /// </summary>
   /// <see cref="CILElementWithCustomModifiers"/>.
   public interface CILCustomModifier
   {
      /// <summary>
      /// Gets or sets optionality of this custom modifier.
      /// </summary>
      /// <value>The optionality of this custom modifier.</value>
      /// <seealso cref="CILCustomModifierOptionality"/>
      CILCustomModifierOptionality Optionality { get; set; }

      /// <summary>
      /// Gets or sets the modifier type of this custom modifier.
      /// </summary>
      /// <value>Modifier type of this custom modifier.</value>
      CILType Modifier { get; set; }
   }

   /// <summary>
   /// Factory class for creating <see cref="CILCustomModifier"/>s.
   /// </summary>
   public static class CILCustomModifierFactory
   {
      /// <summary>
      /// Creates a new instance of <see cref="CILCustomModifier"/> as required modifier with specified modifier type.
      /// </summary>
      /// <param name="modifier">The modifier type.</param>
      /// <returns>A new instance of <see cref="CILCustomModifier"/> as required modifier with specified modifier type.</returns>
      public static CILCustomModifier CreateRequiredModifier( CILType modifier )
      {
         return CreateModifier( CILCustomModifierOptionality.Required, modifier );
      }

      /// <summary>
      /// Creates a new instance of <see cref="CILCustomModifier"/> as optional modifier with specified modifier type.
      /// </summary>
      /// <param name="modifier">The modifier type.</param>
      /// <returns>A new instance of <see cref="CILCustomModifier"/> as optional modifier with specified modifier type.</returns>
      public static CILCustomModifier CreateOptionalModifier( CILType modifier )
      {
         return CreateModifier( CILCustomModifierOptionality.Optional, modifier );
      }

      /// <summary>
      /// Creates a new instance of <see cref="CILCustomModifier"/> with specified optionality and modifier type.
      /// </summary>
      /// <param name="optionality">The optionality of custom modifier.</param>
      /// <param name="modifier">The type of custom modifier.</param>
      /// <returns>A new instance of <see cref="CILCustomModifier"/> with specified optionality and modifier.</returns>
      public static CILCustomModifier CreateModifier( CILCustomModifierOptionality optionality, CILType modifier )
      {
         return new CILCustomModifierImpl( optionality, modifier );
      }
   }
}