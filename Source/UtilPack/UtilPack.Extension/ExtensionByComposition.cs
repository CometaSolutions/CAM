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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilPack.Extension;
using UtilPack.CollectionsWithRoles;

namespace UtilPack.Extension
{
   /// <summary>
   /// This is common interface for objects providing extensionability through composition (instead of inheritance).
   /// </summary>
   /// <typeparam name="TFunctionality">The base type for all extensions that this type supports.</typeparam>
   public interface SelfDescribingExtensionByCompositionProvider<TFunctionality>
      where TFunctionality : class
   {
      /// <summary>
      /// Registers a certain type of functionality for this <see cref="SelfDescribingExtensionByCompositionProvider{TFunctionality}"/>, with lazy initialization of functionality.
      /// </summary>
      /// <typeparam name="TThisFunctionality">The type of the functionality.</typeparam>
      /// <param name="functionality">The callback to create an instance of functionality.</param>
      /// <returns><c>true</c> if <paramref name="functionality"/> was not <c>null</c> and registered; <c>false</c> otherwise.</returns>
      Boolean RegisterFunctionality<TThisFunctionality>( Func<TThisFunctionality> functionality )
         where TThisFunctionality : class, TFunctionality;

      /// <summary>
      /// Gets all the functionalities for this <see cref="SelfDescribingExtensionByCompositionProvider{TFunctionality}"/>.
      /// </summary>
      /// <value>All the functionalities for this <see cref="SelfDescribingExtensionByCompositionProvider{TFunctionality}"/>.</value>
      DictionaryQuery<Type, Lazy<TFunctionality>> Functionalities { get; }
   }

   /// <summary>
   /// This class provides default implementation of <see cref="SelfDescribingExtensionByCompositionProvider{TFunctionality}"/>.
   /// </summary>
   /// <typeparam name="TFunctionality">The base type for all functionalities.</typeparam>
   public class DefaultSelfDescribingExtensionByCompositionProvider<TFunctionality> : SelfDescribingExtensionByCompositionProvider<TFunctionality>
      where TFunctionality : class
   {
      private readonly DictionaryProxy<Type, Lazy<TFunctionality>> _functionalities;
      private readonly System.Threading.LazyThreadSafetyMode _lazyThreadSafety;

      /// <summary>
      /// Creates a new instance of <see cref="DefaultSelfDescribingExtensionByCompositionProvider{TFunctionality}"/>.
      /// <param name="lazyThreadSafety">The lazy thread safety for the <see cref="Lazy{T}"/> object.</param>
      /// </summary>
      public DefaultSelfDescribingExtensionByCompositionProvider(
         System.Threading.LazyThreadSafetyMode lazyThreadSafety = System.Threading.LazyThreadSafetyMode.ExecutionAndPublication
         )
      {
         this._lazyThreadSafety = lazyThreadSafety;
         this._functionalities = new Dictionary<Type, Lazy<TFunctionality>>().ToDictionaryProxy();
      }

      /// <inheritdoc />
      public DictionaryQuery<Type, Lazy<TFunctionality>> Functionalities
      {
         get
         {
            return this._functionalities.CQ;
         }
      }

      /// <inheritdoc />
      public Boolean RegisterFunctionality<TThisFunctionality>( Func<TThisFunctionality> functionality )
          where TThisFunctionality : class, TFunctionality
      {
         var retVal = functionality != null;
         if ( retVal )
         {
            this._functionalities[typeof( TThisFunctionality )] = new Lazy<TFunctionality>( () => functionality(), this._lazyThreadSafety );
         }
         return retVal;
      }
   }
}

#pragma warning disable 1591
public static partial class E_UtilPack
#pragma warning restore 1591
{
   /// <summary>
   /// Registers a certain type of functionality for this <see cref="SelfDescribingExtensionByCompositionProvider{TFunctionality}"/>, when the functionality is already created.
   /// </summary>
   /// <typeparam name="TFunctionality">The base type of the functionality.</typeparam>
   /// <typeparam name="TThisFunctionality">The type of this functionality.</typeparam>
   /// <param name="provider">The <see cref="SelfDescribingExtensionByCompositionProvider{TFunctionality}"/>.</param>
   /// <param name="functionality">The instance of functionality.</param>
   /// <returns><c>true</c> if <paramref name="functionality"/> was not <c>null</c> and registered; <c>false</c> otherwise.</returns>
   public static Boolean RegisterFunctionalityDirect<TFunctionality, TThisFunctionality>( this SelfDescribingExtensionByCompositionProvider<TFunctionality> provider, TThisFunctionality functionality )
      where TFunctionality : class
      where TThisFunctionality : class, TFunctionality
   {
      return functionality != null && provider.RegisterFunctionality( () => functionality );
   }

   /// <summary>
   /// Registers a certain type of functionality for this <see cref="SelfDescribingExtensionByCompositionProvider{TFunctionality}"/>, when the functionality is already created.
   /// </summary>
   /// <typeparam name="TThisFunctionality">The type of this functionality.</typeparam>
   /// <param name="provider">The <see cref="SelfDescribingExtensionByCompositionProvider{TFunctionality}"/>.</param>
   /// <param name="functionality">The instance of functionality.</param>
   /// <returns><c>true</c> if <paramref name="functionality"/> was not <c>null</c> and registered; <c>false</c> otherwise.</returns>
   public static Boolean RegisterFunctionalityDirect<TThisFunctionality>( this SelfDescribingExtensionByCompositionProvider<Object> provider, TThisFunctionality functionality )
      where TThisFunctionality : class
   {
      return functionality != null && provider.RegisterFunctionality( () => functionality );
   }

   /// <summary>
   /// Helper method to get functionality when the type of functionality is known at compile time.
   /// </summary>
   /// <typeparam name="TFunctionality">The base type of the functionality.</typeparam>
   /// <typeparam name="TThisFunctionality">The type of this functionality.</typeparam>
   /// <param name="provider">The <see cref="SelfDescribingExtensionByCompositionProvider{TFunctionality}"/>.</param>
   /// <returns>The functionality, or <c>null</c> if functionality is not found.</returns>
   /// <exception cref="NullReferenceException">If the <see cref="SelfDescribingExtensionByCompositionProvider{TFunctionality}"/> is <c>null</c>.</exception>
   public static TThisFunctionality GetFunctionality<TFunctionality, TThisFunctionality>( this SelfDescribingExtensionByCompositionProvider<TFunctionality> provider )
      where TFunctionality : class
      where TThisFunctionality : class, TFunctionality
   {
      return provider.GetFunctionality( typeof( TThisFunctionality ) ) as TThisFunctionality;
   }

   /// <summary>
   /// Helper method to get functionality when the type of functionality is known at compile time.
   /// </summary>
   /// <typeparam name="TThisFunctionality">The type of this functionality.</typeparam>
   /// <param name="provider">The <see cref="SelfDescribingExtensionByCompositionProvider{TFunctionality}"/>.</param>
   /// <returns>The functionality, or <c>null</c> if functionality is not found.</returns>
   /// <exception cref="NullReferenceException">If the <see cref="SelfDescribingExtensionByCompositionProvider{TFunctionality}"/> is <c>null</c>.</exception>
   public static TThisFunctionality GetFunctionality<TThisFunctionality>( this SelfDescribingExtensionByCompositionProvider<Object> provider )
      where TThisFunctionality : class
   {
      return provider.GetFunctionality( typeof( TThisFunctionality ) ) as TThisFunctionality;
   }

   /// <summary>
   /// Helpe rmethod to get functionality when the type of functionality is not known at compile time.
   /// </summary>
   /// <typeparam name="TFunctionality">The base type of the functionality.</typeparam>
   /// <param name="provider">The <see cref="SelfDescribingExtensionByCompositionProvider{TFunctionality}"/>.</param>
   /// <param name="functionalityType">The type of the functionality.</param>
   /// <returns>The functionality, or <c>null</c> if functionality is not found.</returns>
   /// <exception cref="NullReferenceException">If the <see cref="SelfDescribingExtensionByCompositionProvider{TFunctionality}"/> is <c>null</c>.</exception>
   public static Object GetFunctionality<TFunctionality>( this SelfDescribingExtensionByCompositionProvider<TFunctionality> provider, Type functionalityType )
      where TFunctionality : class
   {
      Boolean success;
      return functionalityType == null ? null : provider.Functionalities.TryGetValue( functionalityType, out success )?.Value;
      //Lazy<TFunctionality> retVal; Boolean contained;
      //return functionalityType != null && ( retVal = provider?.Functionalities.TryGetValue( functionalityType, out contained )) ?
      //   retVal.Value :
      //   null;
   }

   /// <summary>
   /// Helpe rmethod to get functionality when the type of functionality is not known at compile time.
   /// </summary>
   /// <param name="provider">The <see cref="SelfDescribingExtensionByCompositionProvider{TFunctionality}"/>.</param>
   /// <param name="functionalityType">The type of the functionality.</param>
   /// <returns>The functionality, or <c>null</c> if functionality is not found.</returns>
   /// <exception cref="NullReferenceException">If the <see cref="SelfDescribingExtensionByCompositionProvider{TFunctionality}"/> is <c>null</c>.</exception>
   public static Object GetFunctionality( this SelfDescribingExtensionByCompositionProvider<Object> provider, Type functionalityType )
   {
      Boolean success;
      return functionalityType == null ? null : provider.Functionalities.TryGetValue( functionalityType, out success )?.Value;
      //Lazy<Object> retVal;
      //return functionalityType != null && provider.Functionalities.TryGetValue( functionalityType, out retVal ) ?
      //   retVal.Value :
      //   null;
   }
}