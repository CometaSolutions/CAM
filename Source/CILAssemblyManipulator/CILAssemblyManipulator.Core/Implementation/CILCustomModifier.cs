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
using System.Threading;
using CILAssemblyManipulator.API;
using CommonUtils;

namespace CILAssemblyManipulator.Implementation
{
   internal class CILCustomModifierImpl : CILCustomModifier
   {
      private Int32 optionality;
      private CILType modifier;

      internal CILCustomModifierImpl( CILCustomModifierOptionality optionality, CILType modifier )
      {
         this.Optionality = optionality;
         this.Modifier = modifier;
      }

      #region CILCustomModifier Members

      public CILCustomModifierOptionality Optionality
      {
         set
         {
            Interlocked.Exchange( ref this.optionality, (Int32) value );
         }
         get
         {
            return (CILCustomModifierOptionality) this.optionality;
         }
      }

      public CILType Modifier
      {
         set
         {
            ArgumentValidator.ValidateNotNull( "Modifier", value );
            Interlocked.Exchange( ref this.modifier, value );
         }
         get
         {
            return this.modifier;
         }
      }

      #endregion

      public override Boolean Equals( Object obj )
      {
         return Object.ReferenceEquals( obj, this ) ||
            this.Equals( obj as CILCustomModifier );
      }

      public override Int32 GetHashCode()
      {
         return ( this.optionality << 31 ) | this.modifier.GetHashCode();
      }

      private Boolean Equals( CILCustomModifier mod )
      {
         return mod != null && this.Optionality == mod.Optionality && this.modifier.Equals( mod.Modifier );
      }

      public override String ToString()
      {
         return ( CILCustomModifierOptionality.Optional == (CILCustomModifierOptionality) this.optionality ?
            "modopt" :
            "modreq" ) + "(" + this.modifier + ")";
      }
   }
}