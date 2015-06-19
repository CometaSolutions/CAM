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

namespace CILAssemblyManipulator.Logical
{
   /// <summary>
   /// This exception is thrown when <see cref="CILReflectionContext.CustomAttributeDataLoadEvent"/> fails to provide custom attribute data for native reflection elements.
   /// </summary>
   public class CustomAttributeDataLoadException : Exception
   {
      private readonly CustomAttributeDataEventArgs _args;

      internal CustomAttributeDataLoadException( CustomAttributeDataEventArgs args )
         : base( "Reflection context could not load custom attribute data." )
      {
         this._args = args;
      }

      /// <summary>
      /// Gets the arguments for <see cref="CILReflectionContext.CustomAttributeDataLoadEvent"/> which failed to provide custom attribute data.
      /// </summary>
      /// <value>The arguments for <see cref="CILReflectionContext.CustomAttributeDataLoadEvent"/> which failed to provide custom attribute data.</value>
      public CustomAttributeDataEventArgs Args
      {
         get
         {
            return this._args;
         }
      }
   }

   /// <summary>
   /// This exception is thrown when <see cref="CILReflectionContext.ModuleTypesLoadEvent"/> fails to provide the defined types of the native <see cref="System.Reflection.Module"/>.
   /// </summary>
   public class TypesLoadException : Exception
   {
      private readonly ModuleTypesEventArgs _args;
      internal TypesLoadException( ModuleTypesEventArgs args )
         : base( "Reflection context could not load types from " + args.Module + "." )
      {
         this._args = args;
      }

      /// <summary>
      /// Gets the arguments for <see cref="CILReflectionContext.ModuleTypesLoadEvent"/> which failed to provide the defined types of the native <see cref="System.Reflection.Module"/>.
      /// </summary>
      /// <value>The arguments for <see cref="CILReflectionContext.ModuleTypesLoadEvent"/> which failed to provide the defined types of the native <see cref="System.Reflection.Module"/>.</value>
      public ModuleTypesEventArgs Args
      {
         get
         {
            return this._args;
         }
      }
   }

   /// <summary>
   /// This exception is thrown when <see cref="CILReflectionContext.TypeModuleLoadEvent"/> fails to provide module for native <see cref="System.Type"/>.
   /// </summary>
   public class ModuleLoadException : Exception
   {
      private readonly TypeModuleEventArgs _args;

      internal ModuleLoadException( TypeModuleEventArgs args )
         : base( "Reflection context could not load module from " + args.Type + "." )
      {
         this._args = args;
      }

      /// <summary>
      /// Gets the arguments given to <see cref="CILReflectionContext.TypeModuleLoadEvent"/> which failed to provide module for native <see cref="System.Type"/>.
      /// </summary>
      /// <value>The arguments given to <see cref="CILReflectionContext.TypeModuleLoadEvent"/> which failed to provide module for native <see cref="System.Type"/>.</value>
      public TypeModuleEventArgs Args
      {
         get
         {
            return this._args;
         }
      }
   }

   ///// <summary>
   ///// This exception is thrown whenever something goes wrong when emitting a strong-signed module.
   ///// </summary>
   //public class CryptographicException : Exception
   //{
   //   internal CryptographicException( String msg )
   //      : this( msg, null )
   //   {

   //   }

   //   internal CryptographicException( String msg, Exception inner )
   //      : base( msg, inner )
   //   {

   //   }
   //}
}