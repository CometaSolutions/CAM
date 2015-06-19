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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;
using CommonUtils;

namespace CILAssemblyManipulator.Logical.Implementation
{
   internal class CILCustomAttributeImpl : CILCustomAttribute
   {
      private static readonly ListProxy<CILCustomAttributeTypedArgument> EMPTY_CTOR_ARGS = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<CILCustomAttributeTypedArgument>();
      private static readonly ListProxy<CILCustomAttributeNamedArgument> EMPTY_NAMED_ARGS = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<CILCustomAttributeNamedArgument>();

      private readonly CILCustomAttributeContainer container;
      private Tuple<CILConstructor, ListProxy<CILCustomAttributeTypedArgument>, ListProxy<CILCustomAttributeNamedArgument>> state;

      internal CILCustomAttributeImpl( CILCustomAttributeContainer container )
      {
         ArgumentValidator.ValidateNotNull( "Custom attribute container", container );

         this.container = container;
      }

      #region CILCustomAttribute Members

      public void SetCustomAttributeData( CILConstructor ctor, IEnumerable<CILCustomAttributeTypedArgument> ctorArgs, IEnumerable<CILCustomAttributeNamedArgument> namedArgs )
      {
         ArgumentValidator.ValidateNotNull( "Constructor", ctor );

         ListProxy<CILCustomAttributeTypedArgument> ctorList;
         if ( ctorArgs == null )
         {
            ctorList = EMPTY_CTOR_ARGS;
         }
         else
         {
            ctorList = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<CILCustomAttributeTypedArgument>( ctorArgs.ToList() );
         }

         ListProxy<CILCustomAttributeNamedArgument> namedList;
         if ( namedArgs == null )
         {
            namedList = EMPTY_NAMED_ARGS;
         }
         else
         {
            namedList = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<CILCustomAttributeNamedArgument>( namedArgs.ToList() );
         }

         Interlocked.Exchange( ref this.state, Tuple.Create( ctor, ctorList, namedList ) );
      }

      #endregion

      #region CILCustomAttribute Members


      public ListQuery<CILCustomAttributeTypedArgument> ConstructorArguments
      {
         get
         {
            return this.state.Item2.CQ;
         }
      }

      public ListQuery<CILCustomAttributeNamedArgument> NamedArguments
      {
         get
         {
            return this.state.Item3.CQ;
         }
      }

      public CILConstructor Constructor
      {
         get
         {
            return this.state.Item1;
         }
      }

      public CILCustomAttributeContainer Container
      {
         get
         {
            return this.container;
         }
      }

      #endregion
   }

   internal class CILCustomAttributeTypedArgumentImpl : CILCustomAttributeTypedArgument
   {
      private CILType argumentType;
      private Object value;

      internal CILCustomAttributeTypedArgumentImpl( CILType argumentType, Object value )
      {
         ArgumentValidator.ValidateNotNull( "Argument type", argumentType );

         this.argumentType = argumentType;
         this.value = value;
      }

      #region CILCustomAttributeTypedArgument Members

      public CILType ArgumentType
      {
         get
         {
            return this.argumentType;
         }
         set
         {
            Interlocked.Exchange( ref this.argumentType, value );
         }
      }

      public Object Value
      {
         get
         {
            return this.value;
         }
         set
         {
            Interlocked.Exchange( ref this.value, value );
         }
      }

      #endregion
   }

   internal class CILCustomAttributeNamedArgumentImpl : CILCustomAttributeNamedArgument
   {
      internal CILCustomAttributeTypedArgument typedValue;
      internal CILElementForNamedCustomAttribute namedMember;

      internal CILCustomAttributeNamedArgumentImpl( CILCustomAttributeTypedArgument typedValue, CILElementForNamedCustomAttribute namedMember )
      {
         ArgumentValidator.ValidateNotNull( "Typed value", typedValue );
         ArgumentValidator.ValidateNotNull( "Named member", namedMember );

         this.TypedValue = typedValue;
         this.NamedMember = namedMember;
      }

      #region CILCustomAttributeNamedArgument Members

      public CILCustomAttributeTypedArgument TypedValue
      {
         get
         {
            return this.typedValue;
         }
         set
         {
            Interlocked.Exchange( ref this.typedValue, value );
         }
      }

      public CILElementForNamedCustomAttribute NamedMember
      {
         get
         {
            return this.namedMember;
         }
         set
         {
            Interlocked.Exchange( ref this.namedMember, value );
         }
      }

      #endregion
   }

}