/*
 * Copyright 2016 Stanislav Muhametsin. All rights Reserved.
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
using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.Meta;
using UtilPack.CollectionsWithRoles;
using UtilPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TabularMetaData.Meta;
using UtilPack.Extension;
using UtilPack.Visiting;

namespace CILAssemblyManipulator.Physical.Meta
{
   /// <summary>
   /// This interface captures some of the operations crucial for interacting with signatures in CAM.Physical framework.
   /// </summary>
   /// <remarks>
   /// Unless specifically desired, instead of directly implementing this interface, a <see cref="DefaultSignatureProvider"/> should be used direclty, or by subclassing.
   /// </remarks>
   public interface SignatureProvider : SelfDescribingExtensionByCompositionProvider<Object> // TODO document which functionalities are available via SelfDescribingExtensionByCompositionProvider<TFunctionality>
   {
      /// <summary>
      /// Gets an instance of <see cref="SimpleTypeSignature"/> for a given <see cref="SimpleTypeSignatureKind"/>, or returns <c>null</c> on failure.
      /// </summary>
      /// <param name="kind">The <see cref="SimpleTypeSignatureKind"/></param>
      /// <returns>An instance of <see cref="SimpleTypeSignature"/> for a given <paramref name="kind"/>, or <c>null</c> if the <paramref name="kind"/> was unrecognized.</returns>
      /// <seealso cref="SimpleTypeSignature"/>
      SimpleTypeSignature GetSimpleTypeSignatureOrNull( SimpleTypeSignatureKind kind );

      /// <summary>
      /// Gets an instance of <see cref="CustomAttributeArgumentTypeSimple"/> for a given <see cref="CustomAttributeArgumentTypeSimpleKind"/>, or returns <c>null</c> on failure.
      /// </summary>
      /// <param name="kind">The <see cref="CustomAttributeArgumentTypeSimpleKind"/></param>
      /// <returns>An instance of <see cref="CustomAttributeArgumentTypeSimple"/> for a given <paramref name="kind"/>, or <c>null</c> if the <paramref name="kind"/> was unrecognized.</returns>
      /// <seealso cref="CustomAttributeArgumentTypeSimple"/>
      CustomAttributeArgumentTypeSimple GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind kind );

   }

   /// <summary>
   /// This type encapsulates callbacks needed for comparing signatures using <see cref="E_CILPhysical.MatchSignatures"/> method.
   /// </summary>
   public struct SignatureMatcher
   {
      /// <summary>
      /// Creates a new instance of <see cref="SignatureMatcher"/> with given parameters.
      /// </summary>
      /// <param name="typeDefOrRefMatcher">The value for <see cref="TypeDefOrRefMatcher"/>.</param>
      /// <param name="resolutionScopeMatcher">The value for <see cref="ResolutionScopeMatcher"/>.</param>
      /// <exception cref="ArgumentNullException">If either of <paramref name="typeDefOrRefMatcher"/> or <paramref name="resolutionScopeMatcher"/> is <c>null</c>.</exception>
      public SignatureMatcher( SignatureMatcherCallback<TableIndex> typeDefOrRefMatcher, SignatureMatcherCallback<TableIndex?> resolutionScopeMatcher )
      {
         this.TypeDefOrRefMatcher = ArgumentValidator.ValidateNotNull( "Type def or ref matcher", typeDefOrRefMatcher );
         this.ResolutionScopeMatcher = ArgumentValidator.ValidateNotNull( "Resolution scope matcher", resolutionScopeMatcher );
      }

      /// <summary>
      /// This callback will be called when one <see cref="TableIndex"/> is pointing to <see cref="Tables.TypeDef"/> table, and another <see cref="TableIndex"/> is pointing to <see cref="Tables.TypeRef"/>, or both are pointing to <see cref="Tables.TypeDef"/>.
      /// </summary>
      /// <value>The callback to compare <see cref="TableIndex"/>es pointing to <see cref="Tables.TypeDef"/> and <see cref="Tables.TypeRef"/> such that both are never <see cref="Tables.TypeRef"/>.</value>
      public SignatureMatcherCallback<TableIndex> TypeDefOrRefMatcher { get; }

      /// <summary>
      /// This callback will be called to match <see cref="TypeReference.ResolutionScope"/>s of top-level <see cref="TypeReference"/>s.
      /// </summary>
      /// <value>The callback to match <see cref="TypeReference.ResolutionScope"/>s of top-level <see cref="TypeReference"/>s.</value>
      public SignatureMatcherCallback<TableIndex?> ResolutionScopeMatcher { get; }
   }

   /// <summary>
   /// This delegate contains signature for callbacks used by <see cref="SignatureMatcher"/>.
   /// </summary>
   /// <typeparam name="TItem">The type of the first item index.</typeparam>
   /// <param name="firstMD">The <see cref="CILMetaData"/> passed as first to <see cref="E_CILPhysical.MatchSignatures"/> method.</param>
   /// <param name="firstIndex">The item within the <see cref="AbstractSignature"/> passed as first to <see cref="E_CILPhysical.MatchSignatures"/> method.</param>
   /// <param name="secondMD">The <see cref="CILMetaData"/> passed as second to <see cref="E_CILPhysical.MatchSignatures"/> method.</param>
   /// <param name="secondIndex">The item within the <see cref="AbstractSignature"/> passed as second to <see cref="E_CILPhysical.MatchSignatures"/> method.</param>
   /// <returns>Whether the <paramref name="firstIndex"/> and <paramref name="secondIndex"/> match.</returns>
   public delegate Boolean SignatureMatcherCallback<TItem>( CILMetaData firstMD, TItem firstIndex, CILMetaData secondMD, TItem secondIndex );



   /// <summary>
   /// This type represents information about a single <see cref="Physical.TableIndex"/> reference located in <see cref="SignatureElement"/>.
   /// </summary>
   public struct SignatureTableIndexInfo
   {
      /// <summary>
      /// Creates a new instance of <see cref="SignatureTableIndexInfo"/> with given parameters.
      /// </summary>
      /// <param name="tableIndex">The <see cref="Physical.TableIndex"/>.</param>
      /// <param name="setter">The callback to set a new table index for the owner of <paramref name="tableIndex"/>.</param>
      public SignatureTableIndexInfo( TableIndex tableIndex, Action<TableIndex> setter )
      {
         this.TableIndex = tableIndex;
         this.TableIndexSetter = ArgumentValidator.ValidateNotNull( "Setter", setter );
      }

      /// <summary>
      /// Gets the <see cref="Physical.TableIndex"/> that is referenced by the signature element.
      /// </summary>
      /// <value>The <see cref="Physical.TableIndex"/> that is referenced by the signature element.</value>
      public TableIndex TableIndex { get; }

      /// <summary>
      /// Gets the callback to set a new <see cref="Physical.TableIndex"/> value to the <see cref="SignatureElement"/>.
      /// </summary>
      /// <value>The callback to set a new <see cref="Physical.TableIndex"/> value to the <see cref="SignatureElement"/>.</value>
      public Action<TableIndex> TableIndexSetter { get; }
   }

   /// <summary>
   /// This class provides default implementation for <see cref="SignatureProvider"/>.
   /// It caches all the <see cref="SimpleTypeSignature"/>s based on their <see cref="SimpleTypeSignature.SimpleType"/>, and also caches all <see cref="CustomAttributeArgumentTypeSimple"/>s based on their <see cref="CustomAttributeArgumentTypeSimple.SimpleType"/>.
   /// </summary>
   public class DefaultSignatureProvider : DefaultSelfDescribingExtensionByCompositionProvider<Object>, SignatureProvider
   {
      /// <summary>
      /// This class contains information to implement visitor logic and various acceptor logic for signatures.
      /// </summary>
      /// <typeparam name="TFunctionality">The type of functionality</typeparam>
      public class SignatureTypeInfo<TFunctionality>
      {
         /// <summary>
         /// Creates a new instance of <see cref="SignatureTypeInfo{TFunctionality}"/> with given parameters.
         /// </summary>
         /// <param name="signatureElementType">The type of signature element.</param>
         /// <param name="factory">The callback to create <see cref="SignatureTableIndexInfo"/>s from signature element.</param>
         /// <exception cref="ArgumentNullException">If <paramref name="signatureElementType"/> is <c>null</c>.</exception>
         /// <exception cref="ArgumentException">If <paramref name="signatureElementType"/> is generic type or it is not assignable from <see cref="SignatureElement"/>.</exception>
         public SignatureTypeInfo( Type signatureElementType, TFunctionality factory )
         {
            this.SignatureElementType = ArgumentValidator.ValidateNotNull( "Signature element type", signatureElementType );
            if ( this.SignatureElementType.GetGenericArguments().Length > 0 )
            {
               throw new ArgumentException( "Signature element type must not be generic type." );
            }
            else if ( !typeof( SignatureElement ).IsAssignableFrom( signatureElementType ) )
            {
               throw new ArgumentException( "Signature element type must be sub-type of " + typeof( SignatureElement ) + "." );
            }

            this.Functionality = factory;
         }

         /// <summary>
         /// Gets the type of the signature element.
         /// </summary>
         /// <value>The type of the signature element.</value>
         public Type SignatureElementType { get; }

         /// <summary>
         /// Gets the factory callback to create enumerable of <see cref="SignatureTableIndexInfo"/>s from single <see cref="SignatureElement"/>.
         /// </summary>
         /// <value>The factory callback to create enumerable of <see cref="SignatureTableIndexInfo"/>s from single <see cref="SignatureElement"/>.</value>
         public TFunctionality Functionality { get; }
      }

      /// <summary>
      /// Gets the default instance of <see cref="DefaultSignatureProvider"/>.
      /// It has support for simple type signatures returned by <see cref="GetDefaultSimpleTypeSignatures"/> method, and custom attribute simple types returned by <see cref="GetDefaultSimpleCATypes"/> method.
      /// </summary>
      public static SignatureProvider DefaultInstance { get; }

      static DefaultSignatureProvider()
      {
         DefaultInstance = new DefaultSignatureProvider();
      }

      private readonly IDictionary<SimpleTypeSignatureKind, SimpleTypeSignature> _simpleTypeSignatures;
      private readonly IDictionary<CustomAttributeArgumentTypeSimpleKind, CustomAttributeArgumentTypeSimple> _simpleCATypes;



      /// <summary>
      /// Creates a new instance of <see cref="DefaultSignatureProvider"/> with given supported <see cref="SimpleTypeSignature"/>s and <see cref="CustomAttributeArgumentTypeSimple"/>s.
      /// </summary>
      /// <param name="simpleTypeSignatures">The supported <see cref="SimpleTypeSignature"/>. If <c>null</c>, the return value of <see cref="GetDefaultSimpleTypeSignatures"/> will be used.</param>
      /// <param name="simpleCATypes">The supported <see cref="CustomAttributeArgumentTypeSimple"/>. If <c>null</c>, the return value of <see cref="GetDefaultSimpleCATypes"/> will be used.</param>
      /// <param name="signatureTableIndexInfoProviders">The <see cref="SignatureTypeInfo{TFunctionality}"/> functionality. If <c>null</c>, the return value of <see cref="GetDefaultSignatureTableIndexInfoCollectors"/> will be used.</param>
      /// <param name="signatureVisitors">The enumerable of signature visitors.</param>
      /// <param name="matchers">The enumerable of signature matchers.</param>
      /// <param name="cloners">The enumerable of signature cloners.</param>
      public DefaultSignatureProvider(
         IEnumerable<SimpleTypeSignature> simpleTypeSignatures = null,
         IEnumerable<CustomAttributeArgumentTypeSimple> simpleCATypes = null,
         IEnumerable<SignatureTypeInfo<Func<VisitorVertexInfoFactory<SignatureElement, Int32>, IEnumerable<VisitorEdgeInfo<SignatureElement, Int32>>>>> signatureVisitors = null,
         IEnumerable<SignatureTypeInfo<AcceptVertexDelegate<SignatureElement, TableIndexCollectorContext>>> signatureTableIndexInfoProviders = null,
         IEnumerable<SignatureTypeInfo<SignatureVertexMatchingFunctionality>> matchers = null,
         IEnumerable<SignatureTypeInfo<AcceptVertexExplicitDelegate<SignatureElement, CopyingContext>>> cloners = null
         )
      {
         this._simpleTypeSignatures = ( simpleTypeSignatures ?? GetDefaultSimpleTypeSignatures() )
            .Where( s => s != null )
            .ToDictionary_Overwrite( s => s.SimpleType, s => s );
         this._simpleCATypes = ( simpleCATypes ?? GetDefaultSimpleCATypes() )
            .Where( s => s != null )
            .ToDictionary_Overwrite( s => s.SimpleType, s => s );

         // Signature visitor
         var visitor = new TypeBasedVisitor<SignatureElement, Int32>();
         foreach ( var visitorInfo in signatureVisitors ?? GetDefaultSignatureVisitors( visitor ) )
         {
            using ( var factory = visitor.CreateVertexInfoFactory( visitorInfo.SignatureElementType ) )
            {
               visitorInfo.Functionality( factory );
            }
         }
         this.RegisterFunctionalityDirect( visitor );

         // Signature acceptor: decomposing
         var decomposer = new TypeBasedAcceptor<SignatureElement, Int32, DecomposeSignatureContext>( visitor, TopMostTypeVisitingStrategy.IfNotOverridingType, true );
         decomposer.RegisterSignatureDecomposer<SignatureElement>( ( el, ctx ) => ctx.AddElement( el ) );
         this.RegisterFunctionalityDirect( decomposer );

         // Signature acceptor: table index collector
         var tableIndexCollector = new TypeBasedAcceptor<SignatureElement, Int32, TableIndexCollectorContext>( visitor, TopMostTypeVisitingStrategy.Never, true );
         foreach ( var tableIndexCollectorInfo in signatureTableIndexInfoProviders ?? GetDefaultSignatureTableIndexInfoCollectors() )
         {
            tableIndexCollector.RegisterVertexAcceptor( tableIndexCollectorInfo.SignatureElementType, tableIndexCollectorInfo.Functionality );
         }
         this.RegisterFunctionalityDirect( tableIndexCollector );

         // Signature matching
         var matcher = new TypeBasedAcceptor<SignatureElement, Int32, SignatureMatchingContext>( visitor, TopMostTypeVisitingStrategy.Never, false );
         foreach ( var matcherInfo in matchers ?? GetDefaultMatchingFunctionality() )
         {
            var type = matcherInfo.SignatureElementType;
            var info = matcherInfo.Functionality;
            matcher.RegisterVertexAcceptor( type, info.VertexAcceptor );
            foreach ( var edge in info.Edges )
            {
               matcher.RegisterEdgeAcceptor( type, edge.EdgeName, edge.Enter, edge.Exit );
            }
         }
         this.RegisterFunctionalityDirect( matcher );

         // Signature cloning
         var cloner = new TypeBasedAcceptor<SignatureElement, Int32, CopyingContext>( visitor, TopMostTypeVisitingStrategy.Never, false );
         foreach ( var clonerInfo in cloners ?? GetDefaultCopyingFunctionality() )
         {
            cloner.RegisterExplicitVertexAcceptor( clonerInfo.SignatureElementType, clonerInfo.Functionality );
         }
         this.RegisterFunctionalityDirect( cloner );
      }

      /// <inheritdoc />
      public SimpleTypeSignature GetSimpleTypeSignatureOrNull( SimpleTypeSignatureKind kind )
      {
         SimpleTypeSignature sig;
         return this._simpleTypeSignatures.TryGetValue( kind, out sig ) ? sig : null;
      }

      /// <inheritdoc />
      public CustomAttributeArgumentTypeSimple GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind kind )
      {
         CustomAttributeArgumentTypeSimple retVal;
         return this._simpleCATypes.TryGetValue( kind, out retVal ) ? retVal : null;
      }

      private static Boolean MatchTypeDefOrRefOrSpec( SignatureMatchingContext ctx, TableIndex defIdx, TableIndex refIdx )
      {
         var defModule = ctx.FirstMD;
         var refModule = ctx.SecondMD;
         var matcher = ctx.SignatureMatcher;
         switch ( defIdx.Table )
         {
            case Tables.TypeDef:
               return ( ReferenceEquals( defModule, refModule )
                  && refIdx.Table == Tables.TypeDef
                  && matcher.TypeDefOrRefMatcher( defModule, defIdx, refModule, refIdx )
                  ) || ( !ReferenceEquals( defModule, refModule )
                  && refIdx.Table == Tables.TypeRef
                  && matcher.TypeDefOrRefMatcher( defModule, defIdx, refModule, refIdx )
                  );
            case Tables.TypeRef:
               return ( !ReferenceEquals( defModule, refModule )
                  && refIdx.Table == Tables.TypeDef
                  && matcher.TypeDefOrRefMatcher( defModule, defIdx, refModule, refIdx )
                  ) || ( refIdx.Table == Tables.TypeRef
                  && MatchTypeRefs( ctx, defIdx.Index, refIdx.Index ) );
            case Tables.TypeSpec:
               return refIdx.Table == Tables.TypeSpec && defModule.GetSignatureProvider().MatchSignatures( defModule, defModule.TypeSpecifications.TableContents[defIdx.Index].Signature, refModule, refModule.TypeSpecifications.TableContents[refIdx.Index].Signature, matcher );
            default:
               return false;
         }
      }

      private static Boolean MatchTypeRefs( SignatureMatchingContext ctx, Int32 defIdx, Int32 refIdx )
      {
         var firstMD = ctx.FirstMD;
         var secondMD = ctx.SecondMD;
         var defTypeRef = firstMD.TypeReferences.TableContents[defIdx];
         var refTypeRef = secondMD.TypeReferences.TableContents[refIdx];
         var retVal = String.Equals( defTypeRef.Name, refTypeRef.Name )
            && String.Equals( defTypeRef.Namespace, refTypeRef.Namespace );
         if ( retVal )
         {
            var defResScopeNullable = defTypeRef.ResolutionScope;
            var refResScopeNullable = refTypeRef.ResolutionScope;
            retVal = ( defResScopeNullable.HasValue
               && refResScopeNullable.HasValue
               && defResScopeNullable.Value.Table == Tables.TypeRef
               && refResScopeNullable.Value.Table == Tables.TypeRef
               && MatchTypeRefs( ctx, defResScopeNullable.Value.Index, refResScopeNullable.Value.Index )
               ) || ( ( !defResScopeNullable.HasValue || defResScopeNullable.Value.Table != Tables.TypeRef )
               && ( !refResScopeNullable.HasValue || refResScopeNullable.Value.Table != Tables.TypeRef )
               && ctx.SignatureMatcher.ResolutionScopeMatcher( firstMD, defResScopeNullable, secondMD, refResScopeNullable )
               );
         }

         return retVal;
      }

      /// <summary>
      /// Gets an instance of <see cref="SimpleTypeSignature"/> for every value of <see cref="SimpleTypeSignatureKind"/> enumeration.
      /// </summary>
      /// <returns>An enumerable to iterate instances of <see cref="SimpleTypeSignature"/>s for every value of <see cref="SimpleTypeSignatureKind"/> enumeration.</returns>
      public static IEnumerable<SimpleTypeSignature> GetDefaultSimpleTypeSignatures()
      {
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.Boolean );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.Char );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.I1 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.U1 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.I2 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.U2 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.I4 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.U4 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.I8 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.U8 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.R4 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.R8 );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.I );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.U );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.Object );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.String );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.Void );
         yield return new SimpleTypeSignature( SimpleTypeSignatureKind.TypedByRef );
      }

      /// <summary>
      /// Gets an instance of <see cref="CustomAttributeArgumentTypeSimple"/> for every value of <see cref="CustomAttributeArgumentTypeSimpleKind"/> enumeration.
      /// </summary>
      /// <returns>An enumerable to iterate instances of <see cref="CustomAttributeArgumentTypeSimple"/>s for every value of <see cref="CustomAttributeArgumentTypeSimpleKind"/> enumeration.</returns>

      public static IEnumerable<CustomAttributeArgumentTypeSimple> GetDefaultSimpleCATypes()
      {
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.Boolean );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.Char );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.I1 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.U1 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.I2 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.U2 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.I4 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.U4 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.I8 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.U8 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.R4 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.R8 );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.String );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.Type );
         yield return new CustomAttributeArgumentTypeSimple( CustomAttributeArgumentTypeSimpleKind.Object );
      }

      /// <summary>
      /// Returns <see cref="SignatureTypeInfo{TFunctionality}"/> functionality for <see cref="ClassOrValueTypeSignature"/> and <see cref="CustomModifierSignature"/>.
      /// </summary>
      /// <returns>Default <see cref="SignatureTypeInfo{TFunctionality}"/> functionality for <see cref="ClassOrValueTypeSignature"/> and <see cref="CustomModifierSignature"/>.</returns>
      public static IEnumerable<SignatureTypeInfo<AcceptVertexDelegate<SignatureElement, TableIndexCollectorContext>>> GetDefaultSignatureTableIndexInfoCollectors()
      {
         yield return NewAcceptFunctionality<ClassOrValueTypeSignature, TableIndexCollectorContext>( ( sig, ctx ) => ctx.AddElement( new SignatureTableIndexInfo( sig.Type, tIdx => sig.Type = tIdx ) ) );
         yield return NewAcceptFunctionality<CustomModifierSignature, TableIndexCollectorContext>( ( sig, ctx ) => ctx.AddElement( new SignatureTableIndexInfo( sig.CustomModifierType, tIdx => sig.CustomModifierType = tIdx ) ) );
      }

      /// <summary>
      /// Gets enumerable of visitor functionalities for default signatures.
      /// </summary>
      /// <returns>The enumerable of visitor functionalities for default signatures.</returns>
      public static IEnumerable<SignatureTypeInfo<Func<VisitorVertexInfoFactory<SignatureElement, Int32>, IEnumerable<VisitorEdgeInfo<SignatureElement, Int32>>>>> GetDefaultSignatureVisitors( TypeBasedVisitor<SignatureElement, Int32> visitor )
      {
         yield return NewVisitFunctionality<ParameterOrLocalSignature>( factory =>
         {
            return new[]
            {
               factory.CreatePropertyEdge<SignatureElement, Int32, ParameterOrLocalSignature>( nameof( ParameterOrLocalSignature.CustomModifiers ), edgeID => ( sig, cb ) => cb.VisitListEdge( edgeID, sig.CustomModifiers ) ),
               factory.CreatePropertyEdge<SignatureElement, Int32, ParameterOrLocalSignature>( nameof( ParameterOrLocalSignature.Type ), edgeID => ( sig, cb ) => cb.VisitSimpleEdge(  sig.Type, edgeID ) ),
             };
         } );

         yield return NewVisitFunctionality<ParameterSignature>( factory =>
         {
            return new[]
            {
               factory.CreateBaseTypeEdge<SignatureElement, Int32, ParameterSignature, ParameterOrLocalSignature>()
            };
         } );

         yield return NewVisitFunctionality<LocalSignature>( factory =>
         {
            return new[]
            {
               factory.CreateBaseTypeEdge<SignatureElement, Int32, LocalSignature, ParameterOrLocalSignature>()
            };
         } );

         yield return NewVisitFunctionality<FieldSignature>( factory =>
         {
            return new[]
            {
               factory.CreatePropertyEdge<SignatureElement, Int32, FieldSignature>( nameof( FieldSignature.CustomModifiers ), edgeID => ( sig, cb ) => cb.VisitListEdge(edgeID, sig.CustomModifiers ) ),
               factory.CreatePropertyEdge<SignatureElement, Int32, FieldSignature>( nameof( FieldSignature.Type ), edgeID => ( sig, cb ) => cb.VisitSimpleEdge( sig.Type, edgeID ) )
            };
         } );

         yield return NewVisitFunctionality<AbstractMethodSignature>( factory =>
         {
            return new[]
            {
               factory.CreatePropertyEdge<SignatureElement, Int32, AbstractMethodSignature>( nameof( AbstractMethodSignature.ReturnType ), edgeID => ( sig, cb ) => cb.VisitSimpleEdge( sig.ReturnType, edgeID ) ),
               factory.CreatePropertyEdge<SignatureElement, Int32, AbstractMethodSignature>( nameof( AbstractMethodSignature.Parameters ), edgeID => ( sig, cb ) => cb.VisitListEdge( edgeID, sig.Parameters ) )
            };
         } );

         yield return NewVisitFunctionality<MethodDefinitionSignature>( factory =>
         {
            return new[]
            {
               factory.CreateBaseTypeEdge<SignatureElement, Int32, MethodDefinitionSignature, AbstractMethodSignature>()
            };
         } );

         yield return NewVisitFunctionality<MethodReferenceSignature>( factory =>
         {
            return new[]
            {
               factory.CreateBaseTypeEdge<SignatureElement, Int32, MethodDefinitionSignature, AbstractMethodSignature>(),
               factory.CreatePropertyEdge<SignatureElement, Int32, MethodReferenceSignature>( nameof( MethodReferenceSignature.VarArgsParameters ), edgeID => ( sig, cb ) => cb.VisitListEdge( edgeID, sig.VarArgsParameters ) )
            };
         } );

         yield return NewVisitFunctionality<PropertySignature>( factory =>
         {
            return new[]
            {
               factory.CreatePropertyEdge<SignatureElement, Int32, PropertySignature>( nameof( PropertySignature.CustomModifiers ), edgeID => ( sig, cb ) => cb.VisitListEdge( edgeID, sig.CustomModifiers ) ),
               factory.CreatePropertyEdge<SignatureElement, Int32, PropertySignature>( nameof( PropertySignature.PropertyType ), edgeID => ( sig, cb ) => cb.VisitSimpleEdge( sig.PropertyType, edgeID ) ),
               factory.CreatePropertyEdge<SignatureElement, Int32, PropertySignature>( nameof( PropertySignature.Parameters ), edgeID => ( sig, cb ) => cb.VisitListEdge( edgeID, sig.Parameters ) )
            };
         } );

         yield return NewVisitFunctionality<LocalVariablesSignature>( factory =>
         {
            return new[]
            {
               factory.CreatePropertyEdge<SignatureElement, Int32, LocalVariablesSignature>( nameof( LocalVariablesSignature.Locals ), edgeID => ( sig, cb ) => cb.VisitListEdge( edgeID, sig.Locals ) )
            };
         } );

         yield return NewVisitFunctionality<GenericMethodSignature>( factory =>
         {
            return new[]
            {
               factory.CreatePropertyEdge<SignatureElement, Int32, GenericMethodSignature>( nameof( GenericMethodSignature.GenericArguments ), edgeID => ( sig, cb ) => cb.VisitListEdge( edgeID, sig.GenericArguments ) )
            };
         } );

         yield return NewVisitFunctionality<AbstractArrayTypeSignature>( factory =>
         {
            return new[]
            {
               factory.CreatePropertyEdge<SignatureElement, Int32, AbstractArrayTypeSignature>( nameof( AbstractArrayTypeSignature.ArrayType ), edgeID => ( sig, cb ) => cb.VisitSimpleEdge( sig.ArrayType, edgeID ) )
            };
         } );

         yield return NewVisitFunctionality<ComplexArrayTypeSignature>( factory =>
         {
            return new[]
            {
               factory.CreateBaseTypeEdge<SignatureElement, Int32, ComplexArrayTypeSignature, AbstractArrayTypeSignature>()
            };
         } );

         yield return NewVisitFunctionality<SimpleArrayTypeSignature>( factory =>
         {
            return new[]
            {
               factory.CreatePropertyEdge<SignatureElement, Int32, SimpleArrayTypeSignature>( nameof( SimpleArrayTypeSignature.CustomModifiers ), edgeID => ( sig, cb ) => cb.VisitListEdge( edgeID, sig.CustomModifiers ) ),
               factory.CreateBaseTypeEdge<SignatureElement, Int32, SimpleArrayTypeSignature, AbstractArrayTypeSignature>()
            };
         } );

         yield return NewVisitFunctionality<ClassOrValueTypeSignature>( factory =>
         {
            return new[]
            {
               factory.CreatePropertyEdge<SignatureElement, Int32, ClassOrValueTypeSignature>( nameof( ClassOrValueTypeSignature.GenericArguments ), edgeID => ( sig, cb ) => cb.VisitListEdge( edgeID, sig.GenericArguments ) )
            };
         } );

         yield return NewVisitFunctionality<FunctionPointerTypeSignature>( factory =>
         {
            return new[]
            {
               factory.CreatePropertyEdge<SignatureElement, Int32, FunctionPointerTypeSignature>( nameof( FunctionPointerTypeSignature.MethodSignature ), edgeID => ( sig, cb ) => cb.VisitSimpleEdge( sig.MethodSignature, edgeID ) )
            };
         } );

         yield return NewVisitFunctionality<PointerTypeSignature>( factory =>
         {
            return new[]
            {
               factory.CreatePropertyEdge<SignatureElement, Int32, PointerTypeSignature>( nameof( PointerTypeSignature.CustomModifiers ), edgeID => ( sig, cb ) => cb.VisitListEdge( edgeID, sig.CustomModifiers ) ),
               factory.CreatePropertyEdge<SignatureElement, Int32, PointerTypeSignature>( nameof( PointerTypeSignature.PointerType ), edgeID => ( sig, cb ) => cb.VisitSimpleEdge( sig.PointerType, edgeID ) )
            };
         } );
      }

      /// <summary>
      /// Gets the default functionality about performing signature matching.
      /// </summary>
      /// <returns>The default functionality about performing signature matching.</returns>
      public static IEnumerable<SignatureTypeInfo<SignatureVertexMatchingFunctionality>> GetDefaultMatchingFunctionality()
      {
         yield return NewMatchingFunctionality<CustomModifierSignature>(
            ( x, y, ctx ) => x.Optionality == y.Optionality && MatchTypeDefOrRefOrSpec( ctx, x.CustomModifierType, y.CustomModifierType )
            );
         yield return NewMatchingFunctionality<ParameterOrLocalSignature>(
            ( x, y, ctx ) => x.CustomModifiers.Count == y.CustomModifiers.Count && x.IsByRef == y.IsByRef,
            SignatureEdgeMatchingFunctionality.NewFunctionalityForListEdge<ParameterOrLocalSignature, CustomModifierSignature>( nameof( ParameterOrLocalSignature.CustomModifiers ), sig => sig.CustomModifiers ),
            SignatureEdgeMatchingFunctionality.NewFunctionalityForSimpleEdge<ParameterOrLocalSignature>( nameof( ParameterOrLocalSignature.Type ), sig => sig.Type )
            );
         yield return NewMatchingFunctionality<ParameterSignature>(
            ( x, y, ctx ) => true
            );
         yield return NewMatchingFunctionality<LocalSignature>(
            ( x, y, ctx ) => x.IsPinned == y.IsPinned
            );
         yield return NewMatchingFunctionality<FieldSignature>(
            ( x, y, ctx ) => x.CustomModifiers.Count == y.CustomModifiers.Count,
            SignatureEdgeMatchingFunctionality.NewFunctionalityForListEdge<FieldSignature, CustomModifierSignature>( nameof( FieldSignature.CustomModifiers ), sig => sig.CustomModifiers ),
            SignatureEdgeMatchingFunctionality.NewFunctionalityForSimpleEdge<FieldSignature>( nameof( FieldSignature.Type ), sig => sig.Type )
            );
         yield return NewMatchingFunctionality<AbstractMethodSignature>(
            ( x, y, ctx ) => x.MethodSignatureInformation == y.MethodSignatureInformation && x.Parameters.Count == y.Parameters.Count && x.GenericArgumentCount == y.GenericArgumentCount,
            SignatureEdgeMatchingFunctionality.NewFunctionalityForSimpleEdge<AbstractMethodSignature>( nameof( AbstractMethodSignature.ReturnType ), sig => sig.ReturnType ),
            SignatureEdgeMatchingFunctionality.NewFunctionalityForListEdge<AbstractMethodSignature, ParameterSignature>( nameof( AbstractMethodSignature.Parameters ), sig => sig.Parameters )
            );
         yield return NewMatchingFunctionality<MethodDefinitionSignature>(
            null
            );
         yield return NewMatchingFunctionality<MethodReferenceSignature>(
            null
            );
         yield return NewMatchingFunctionality<PropertySignature>(
            ( x, y, ctx ) => x.CustomModifiers.Count == y.CustomModifiers.Count && x.HasThis == y.HasThis && x.Parameters.Count == y.Parameters.Count,
            SignatureEdgeMatchingFunctionality.NewFunctionalityForSimpleEdge<PropertySignature>( nameof( PropertySignature.PropertyType ), sig => sig.PropertyType ),
            SignatureEdgeMatchingFunctionality.NewFunctionalityForListEdge<PropertySignature, ParameterSignature>( nameof( PropertySignature.Parameters ), sig => sig.Parameters )
            );
         yield return NewMatchingFunctionality<LocalVariablesSignature>(
            ( x, y, ctx ) => x.Locals.Count == y.Locals.Count,
            SignatureEdgeMatchingFunctionality.NewFunctionalityForListEdge<LocalVariablesSignature, LocalSignature>( nameof( LocalVariablesSignature.Locals ), sig => sig.Locals )
            );
         yield return NewMatchingFunctionality<GenericMethodSignature>(
            ( x, y, ctx ) => x.GenericArguments.Count == y.GenericArguments.Count,
            SignatureEdgeMatchingFunctionality.NewFunctionalityForListEdge<GenericMethodSignature, TypeSignature>( nameof( GenericMethodSignature.GenericArguments ), sig => sig.GenericArguments )
            );
         yield return NewMatchingFunctionality<RawSignature>(
            ( x, y, ctx ) => ArrayEqualityComparer<Byte>.ArrayEquality( x.Bytes, y.Bytes )
            );
         yield return NewMatchingFunctionality<AbstractArrayTypeSignature>(
            ( x, y, ctx ) => true,
            SignatureEdgeMatchingFunctionality.NewFunctionalityForSimpleEdge<AbstractArrayTypeSignature>( nameof( AbstractArrayTypeSignature.ArrayType ), sig => sig.ArrayType )
            );
         yield return NewMatchingFunctionality<ComplexArrayTypeSignature>(
            ( x, y, ctx ) => Comparers.ComplexArrayInfoEqualityComparer.Equals( x.ComplexArrayInfo, y.ComplexArrayInfo )
            );
         yield return NewMatchingFunctionality<SimpleArrayTypeSignature>(
            ( x, y, ctx ) => x.CustomModifiers.Count == y.CustomModifiers.Count,
            SignatureEdgeMatchingFunctionality.NewFunctionalityForListEdge<SimpleArrayTypeSignature, CustomModifierSignature>( nameof( SimpleArrayTypeSignature.CustomModifiers ), sig => sig.CustomModifiers )
            );
         yield return NewMatchingFunctionality<ClassOrValueTypeSignature>(
            ( x, y, ctx ) => x.GenericArguments.Count == y.GenericArguments.Count && x.TypeReferenceKind == y.TypeReferenceKind && MatchTypeDefOrRefOrSpec( ctx, x.Type, y.Type ),
            SignatureEdgeMatchingFunctionality.NewFunctionalityForListEdge<ClassOrValueTypeSignature, TypeSignature>( nameof( ClassOrValueTypeSignature.GenericArguments ), sig => sig.GenericArguments )
            );
         yield return NewMatchingFunctionality<GenericParameterTypeSignature>(
            ( x, y, ctx ) => x.GenericParameterIndex == y.GenericParameterIndex && x.GenericParameterKind == y.GenericParameterKind
            );
         yield return NewMatchingFunctionality<PointerTypeSignature>(
            ( x, y, ctx ) => x.CustomModifiers.Count == y.CustomModifiers.Count,
            SignatureEdgeMatchingFunctionality.NewFunctionalityForListEdge<PointerTypeSignature, CustomModifierSignature>( nameof( PointerTypeSignature.CustomModifiers ), sig => sig.CustomModifiers )
            );
         yield return NewMatchingFunctionality<FunctionPointerTypeSignature>(
            ( x, y, ctx ) => true,
            SignatureEdgeMatchingFunctionality.NewFunctionalityForSimpleEdge<FunctionPointerTypeSignature>( nameof( FunctionPointerTypeSignature.MethodSignature ), sig => sig.MethodSignature )
            );
         yield return NewMatchingFunctionality<SimpleTypeSignature>(
            ( x, y, ctx ) => x.SimpleType == y.SimpleType
            );
      }

      /// <summary>
      /// Returns cloning functionality for default signature types.
      /// </summary>
      /// <returns>The cloning functionality for default signature types.</returns>
      public static IEnumerable<SignatureTypeInfo<AcceptVertexExplicitDelegate<SignatureElement, CopyingContext>>> GetDefaultCopyingFunctionality()
      {
         yield return NewCopyingFunctionality<CustomModifierSignature>( ( sig, ctx, cb ) => new CustomModifierSignature()
         {
            Optionality = sig.Optionality,
            CustomModifierType = ctx.TableIndexTransformer?.Invoke( sig.CustomModifierType ) ?? sig.CustomModifierType
         } );
         yield return NewCopyingFunctionality<ParameterSignature>( ( sig, ctx, cb ) =>
         {
            var retVal = new ParameterSignature( sig.CustomModifiers.Count )
            {
               IsByRef = sig.IsByRef,
               Type = ctx.CloneSingle( sig.Type, cb )
            };
            ctx.CloneList( retVal.CustomModifiers, sig.CustomModifiers, cb );
            return retVal;
         } );
         yield return NewCopyingFunctionality<LocalSignature>( ( sig, ctx, cb ) =>
         {
            var retVal = new LocalSignature( sig.CustomModifiers.Count )
            {
               IsByRef = sig.IsByRef,
               Type = ctx.CloneSingle( sig.Type, cb ),
               IsPinned = sig.IsPinned
            };
            ctx.CloneList( retVal.CustomModifiers, sig.CustomModifiers, cb );
            return retVal;
         } );
         yield return NewCopyingFunctionality<FieldSignature>( ( sig, ctx, cb ) =>
         {
            var retVal = new FieldSignature( sig.CustomModifiers.Count )
            {
               ExtraData = sig.ExtraData.CreateBlockCopy(),
               Type = ctx.CloneSingle( sig.Type, cb )
            };
            ctx.CloneList( retVal.CustomModifiers, sig.CustomModifiers, cb );
            return retVal;
         } );
         yield return NewCopyingFunctionality<MethodDefinitionSignature>( ( sig, ctx, cb ) =>
         {
            var retVal = new MethodDefinitionSignature( sig.Parameters.Count )
            {
               ExtraData = sig.ExtraData.CreateBlockCopy(),
               GenericArgumentCount = sig.GenericArgumentCount,
               MethodSignatureInformation = sig.MethodSignatureInformation,
               ReturnType = ctx.CloneSingle( sig.ReturnType, cb )
            };
            ctx.CloneList( retVal.Parameters, sig.Parameters, cb );
            return retVal;
         } );
         yield return NewCopyingFunctionality<MethodReferenceSignature>( ( sig, ctx, cb ) =>
         {
            var retVal = new MethodReferenceSignature( sig.Parameters.Count, sig.VarArgsParameters.Count )
            {
               ExtraData = sig.ExtraData.CreateBlockCopy(),
               GenericArgumentCount = sig.GenericArgumentCount,
               MethodSignatureInformation = sig.MethodSignatureInformation,
               ReturnType = ctx.CloneSingle( sig.ReturnType, cb )
            };
            ctx.CloneList( retVal.Parameters, sig.Parameters, cb );
            ctx.CloneList( retVal.VarArgsParameters, sig.VarArgsParameters, cb );
            return retVal;
         } );
         yield return NewCopyingFunctionality<PropertySignature>( ( sig, ctx, cb ) =>
         {
            var retVal = new PropertySignature( sig.CustomModifiers.Count, sig.Parameters.Count )
            {
               ExtraData = sig.ExtraData.CreateBlockCopy(),
               HasThis = sig.HasThis,
               PropertyType = ctx.CloneSingle( sig.PropertyType, cb ),
            };
            ctx.CloneList( retVal.CustomModifiers, sig.CustomModifiers, cb );
            ctx.CloneList( retVal.Parameters, sig.Parameters, cb );
            return retVal;
         } );
         yield return NewCopyingFunctionality<LocalVariablesSignature>( ( sig, ctx, cb ) =>
         {
            var retVal = new LocalVariablesSignature( sig.Locals.Count )
            {
               ExtraData = sig.ExtraData.CreateBlockCopy()
            };
            ctx.CloneList( retVal.Locals, sig.Locals, cb );
            return retVal;
         } );
         yield return NewCopyingFunctionality<GenericMethodSignature>( ( sig, ctx, cb ) =>
         {
            var retVal = new GenericMethodSignature( sig.GenericArguments.Count )
            {
               ExtraData = sig.ExtraData.CreateBlockCopy()
            };
            ctx.CloneList( retVal.GenericArguments, sig.GenericArguments, cb );
            return retVal;
         } );
         yield return NewCopyingFunctionality<RawSignature>( ( sig, ctx, cb ) => new RawSignature()
         {
            Bytes = sig.Bytes.CreateBlockCopy()
         } );
         yield return NewCopyingFunctionality<ComplexArrayTypeSignature>( ( sig, ctx, cb ) => new ComplexArrayTypeSignature()
         {
            ExtraData = sig.ExtraData.CreateBlockCopy(),
            ArrayType = ctx.CloneSingle( sig.ArrayType, cb ),
            ComplexArrayInfo = new ComplexArrayInfo( sig.ComplexArrayInfo )
         } );
         yield return NewCopyingFunctionality<SimpleArrayTypeSignature>( ( sig, ctx, cb ) =>
         {
            var retVal = new SimpleArrayTypeSignature( sig.CustomModifiers.Count )
            {
               ExtraData = sig.ExtraData.CreateBlockCopy(),
               ArrayType = ctx.CloneSingle( sig.ArrayType, cb )
            };
            ctx.CloneList( retVal.CustomModifiers, sig.CustomModifiers, cb );
            return retVal;
         } );
         yield return NewCopyingFunctionality<ClassOrValueTypeSignature>( ( sig, ctx, cb ) =>
         {
            var retVal = new ClassOrValueTypeSignature( sig.GenericArguments.Count )
            {
               ExtraData = sig.ExtraData.CreateBlockCopy(),
               Type = ctx.TableIndexTransformer?.Invoke( sig.Type ) ?? sig.Type,
               TypeReferenceKind = sig.TypeReferenceKind
            };
            ctx.CloneList( retVal.GenericArguments, sig.GenericArguments, cb );
            return retVal;
         } );
         yield return NewCopyingFunctionality<GenericParameterTypeSignature>( ( sig, ctx, cb ) => new GenericParameterTypeSignature()
         {
            ExtraData = sig.ExtraData.CreateBlockCopy(),
            GenericParameterIndex = sig.GenericParameterIndex,
            GenericParameterKind = sig.GenericParameterKind
         } );
         yield return NewCopyingFunctionality<FunctionPointerTypeSignature>( ( sig, ctx, cb ) => new FunctionPointerTypeSignature()
         {
            ExtraData = sig.ExtraData.CreateBlockCopy(),
            MethodSignature = ctx.CloneSingle( sig.MethodSignature, cb )
         } );
         yield return NewCopyingFunctionality<PointerTypeSignature>( ( sig, ctx, cb ) =>
         {
            var retVal = new PointerTypeSignature()
            {
               ExtraData = sig.ExtraData.CreateBlockCopy(),
               PointerType = ctx.CloneSingle( sig.PointerType, cb )
            };
            ctx.CloneList( retVal.CustomModifiers, sig.CustomModifiers, cb );
            return retVal;
         } );
         yield return NewCopyingFunctionality<SimpleTypeSignature>( ( sig, ctx, cb ) => sig );
      }


      /// <summary>
      /// Creates a new <see cref="SignatureTypeInfo{TFunctionality}"/> with functionality for visiting signatures according to visitor pattern.
      /// </summary>
      /// <typeparam name="TSignature">The type of the signature element.</typeparam>
      /// <param name="functionality">The visitor functionality.</param>
      /// <returns>A new instance of <see cref="SignatureTypeInfo{TFunctionality}"/>.</returns>
      public static SignatureTypeInfo<Func<VisitorVertexInfoFactory<SignatureElement, Int32>, IEnumerable<VisitorEdgeInfo<SignatureElement, Int32>>>> NewVisitFunctionality<TSignature>(
         Func<VisitorVertexInfoFactory<SignatureElement, Int32>, IEnumerable<VisitorEdgeInfo<SignatureElement, Int32>>> functionality
         )
         where TSignature : SignatureElement
      {
         return new SignatureTypeInfo<Func<VisitorVertexInfoFactory<SignatureElement, Int32>, IEnumerable<VisitorEdgeInfo<SignatureElement, Int32>>>>( typeof( TSignature ), functionality );
      }

      /// <summary>
      /// Creates a new <see cref="SignatureTypeInfo{TFunctionality}"/> with functionality for accepting signatures according to visitor pattern.
      /// </summary>
      /// <typeparam name="TSignature">The type of the signature element.</typeparam>
      /// <typeparam name="TContext">The type of context.</typeparam>
      /// <param name="acceptor">The acceptor functionality.</param>
      /// <returns>A new instance of <see cref="SignatureTypeInfo{TFunctionality}"/>.</returns>
      public static SignatureTypeInfo<AcceptVertexDelegate<SignatureElement, TContext>> NewAcceptFunctionality<TSignature, TContext>( AcceptVertexDelegate<TSignature, TContext> acceptor )
         where TSignature : SignatureElement
      {
         return new SignatureTypeInfo<AcceptVertexDelegate<SignatureElement, TContext>>( typeof( TSignature ), ( el, ctx ) => acceptor( (TSignature) el, ctx ) );
      }

      internal static SignatureTypeInfo<SignatureVertexMatchingFunctionality> NewMatchingFunctionality<TSignature>(
         EqualityWithContext<TSignature, SignatureMatchingContext> vertexEquality,
         params SignatureEdgeMatchingFunctionality[] edges
         )
         where TSignature : class, SignatureElement
      {
         return new SignatureTypeInfo<SignatureVertexMatchingFunctionality>( typeof( TSignature ), SignatureVertexMatchingFunctionality.NewFunctionality( vertexEquality, edges ) );
      }

      internal static SignatureTypeInfo<AcceptVertexExplicitDelegate<SignatureElement, CopyingContext>> NewCopyingFunctionality<TSignature>(
         CopySignatureDelegate<TSignature> functionality
         )
         where TSignature : class, SignatureElement
      {
         return new SignatureTypeInfo<AcceptVertexExplicitDelegate<SignatureElement, CopyingContext>>( typeof( TSignature ), ( el, ctx, cb ) =>
         {
            var result = functionality( (TSignature) el, ctx, cb );
            ctx.CurrentObject = result;
            return true;
         } );
      }

   }

   /// <summary>
   /// This class holds callbacks to match signatures.
   /// </summary>
   public class SignatureVertexMatchingFunctionality
   {
      /// <summary>
      /// Creates a new instance of <see cref="SignatureVertexMatchingFunctionality"/>.
      /// </summary>
      /// <param name="vertexEquality">The callback to match simple properties of </param>
      /// <param name="edges">The edges.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="vertexEquality"/> is <c>null</c>.</exception>
      public SignatureVertexMatchingFunctionality( AcceptVertexDelegate<SignatureElement, SignatureMatchingContext> vertexEquality, params SignatureEdgeMatchingFunctionality[] edges )
      {
         this.VertexAcceptor = ArgumentValidator.ValidateNotNull( "Vertex equality", vertexEquality );
         this.Edges = ( edges ?? Empty<SignatureEdgeMatchingFunctionality>.Array ).ToArrayProxy().CQ;
      }

      /// <summary>
      /// Gets the callback used to match signature element vertices.
      /// </summary>
      /// <value>The callback used to match signature element vertices.</value>
      public AcceptVertexDelegate<SignatureElement, SignatureMatchingContext> VertexAcceptor { get; }

      /// <summary>
      /// Gets the array of <see cref="SignatureEdgeMatchingFunctionality"/> objects.
      /// </summary>
      /// <value>The array of <see cref="SignatureEdgeMatchingFunctionality"/> objects.</value>
      public ArrayQuery<SignatureEdgeMatchingFunctionality> Edges { get; }

      internal static SignatureVertexMatchingFunctionality NewFunctionality<TSignature>(
         EqualityWithContext<TSignature, SignatureMatchingContext> vertexEquality,
         params SignatureEdgeMatchingFunctionality[] edges
         )
         where TSignature : class, SignatureElement
      {
         return new SignatureVertexMatchingFunctionality(
            ( el, ctx ) =>
            {
               var retVal = vertexEquality == null;
               if ( !retVal )
               {
                  var fromCtx = ctx.GetCurrentElement();
                  TSignature fromCtxTyped;
                  retVal = ReferenceEquals( el, fromCtx )
                  || ( el != null && ( fromCtxTyped = fromCtx as TSignature ) != null
                  && vertexEquality( (TSignature) el, fromCtxTyped, ctx ) );
               }
               return retVal;
            },
            edges
            );
      }
   }

   /// <summary>
   /// This class holds callbacks to match the signature element edges.
   /// </summary>
   public class SignatureEdgeMatchingFunctionality
   {
      /// <summary>
      /// Creates new instance of <see cref="SignatureEdgeMatchingFunctionality"/>.
      /// </summary>
      /// <param name="edgeName">The name of the edge.</param>
      /// <param name="enter">The callback when entering the edge.</param>
      /// <param name="exit">The callback when exiting the edge.</param>
      /// <exception cref="ArgumentNullException">If any of <paramref name="edgeName"/>, <paramref name="enter"/>, or <paramref name="exit"/> is <c>null</c>.</exception>
      public SignatureEdgeMatchingFunctionality( String edgeName, AcceptEdgeDelegate<SignatureElement, Int32, SignatureMatchingContext> enter, AcceptEdgeDelegate<SignatureElement, Int32, SignatureMatchingContext> exit )
      {
         this.EdgeName = ArgumentValidator.ValidateNotNull( "Edge name", edgeName );
         this.Enter = ArgumentValidator.ValidateNotNull( "Enter callback", enter );
         this.Exit = exit ?? ( new AcceptEdgeDelegate<SignatureElement, Int32, SignatureMatchingContext>( ( el, info, ctx ) => { ctx.CurrentElementStack.Pop(); return true; } ) );
      }

      /// <summary>
      /// Gets the name of the edge.
      /// </summary>
      /// <value>The name of the edge.</value>
      /// <remarks>
      /// Typically the name is the name of the property, using the <c>nameof</c> operator.
      /// </remarks>
      public String EdgeName { get; }

      /// <summary>
      /// Gets the callback which is executed when entering the edge.
      /// </summary>
      /// <value>The callback which is executed when entering the edge.</value>
      public AcceptEdgeDelegate<SignatureElement, Int32, SignatureMatchingContext> Enter { get; }

      /// <summary>
      /// Gets the callback which is executed when exiting the edge.
      /// </summary>
      /// <value>The callback which is executed when exiting the edge.</value>
      public AcceptEdgeDelegate<SignatureElement, Int32, SignatureMatchingContext> Exit { get; }

      private static Boolean DefaultEdgeExit( SignatureElement element, Int32 info, SignatureMatchingContext context )
      {
         context.CurrentElementStack.Pop();
         return true;
      }

      internal static SignatureEdgeMatchingFunctionality NewFunctionalityForSimpleEdge<TSignature>( String propertyName, Func<TSignature, SignatureElement> getter )
      {
         return new SignatureEdgeMatchingFunctionality(
            propertyName,
            ( el, info, ctx ) =>
            {
               ctx.CurrentElementStack.Push( getter( (TSignature) ctx.GetCurrentElement() ) );
               return true;
            },
            DefaultEdgeExit
            );
      }

      internal static SignatureEdgeMatchingFunctionality NewFunctionalityForListEdge<TSignature, TListElement>( String propertyName, Func<TSignature, List<TListElement>> listGetter )
         where TListElement : SignatureElement
      {
         return new SignatureEdgeMatchingFunctionality(
            propertyName,
            ( el, info, ctx ) =>
            {
               var list = listGetter( (TSignature) ctx.GetCurrentElement() );
               ctx.CurrentElementStack.Push( list[(Int32) info] );
               return true;
            },
            DefaultEdgeExit
            );
      }
   }

   /// <summary>
   /// This is abstract base class for functionalities that collect something from hierarchical structures.
   /// </summary>
   /// <typeparam name="TElement">The type of elements to collect.</typeparam>
   public abstract class ElementCollectorState<TElement>
   {
      private readonly ListProxy<TElement> _elements;
      private readonly Boolean _nullsAllowed;

      /// <summary>
      /// Creates new instance of <see cref="ElementCollectorState{TElement}"/>.
      /// </summary>
      /// <param name="nullsAllowed">Whether <c>null</c> values are allowed.</param>
      public ElementCollectorState( Boolean nullsAllowed = false )
      {
         this._nullsAllowed = nullsAllowed;
         this._elements = new List<TElement>().ToListProxy();
      }

      /// <summary>
      /// Adds an element to the current list of elements.
      /// </summary>
      /// <param name="element">The element to add.</param>
      /// <returns>Always returns <c>true</c>.</returns>
      public Boolean AddElement( TElement element )
      {
         if ( this._nullsAllowed || element != null )
         {
            this._elements.Add( element );
         }
         return true;
      }

      /// <summary>
      /// Gets the added elements.
      /// </summary>
      /// <value>The added elements.</value>
      public ListQuery<TElement> Elements
      {
         get
         {
            return this._elements.CQ;
         }
      }
   }

   internal class DecomposeSignatureContext : ElementCollectorState<SignatureElement>
   {
   }

   /// <summary>
   /// This state is used in <see cref="E_CILPhysical.GetSignatureTableIndexInfos"/> method to perform visitor walking in order to extract <see cref="SignatureTableIndexInfo"/>s.
   /// </summary>
   public class TableIndexCollectorContext : ElementCollectorState<SignatureTableIndexInfo>
   {

   }

   /// <summary>
   /// This is base class for code using visitor pattern to test whether object hierarchies are equal.
   /// </summary>
   /// <typeparam name="TElement">The top-level type of the elements.</typeparam>
   public class ObjectGraphEqualityContext<TElement>
   {
      /// <summary>
      /// Creates a new instance of <see cref="ObjectGraphEqualityContext{TElement}"/> with given starting element.
      /// </summary>
      public ObjectGraphEqualityContext()
      {
         this.CurrentElementStack = new Stack<TElement>();
      }

      /// <summary>
      /// Gets the current stack of the objects.
      /// </summary>
      /// <value>The current stack of the objects.</value>
      /// <remarks>
      /// Whenever visiting other object, on-enter-edge acceptor should push to this stack, and on-exit-edge acceptor should pop from this stack.
      /// </remarks>
      public Stack<TElement> CurrentElementStack { get; }
   }

   /// <summary>
   /// This class captures context when matching two signatures.
   /// </summary>
   public class SignatureMatchingContext : ObjectGraphEqualityContext<SignatureElement>
   {
      /// <summary>
      /// Creates a new instance of <see cref="SignatureMatchingContext"/>.
      /// </summary>
      /// <param name="startingElement">The starting element, not part of the visited signature.</param>
      /// <param name="firstMD">The <see cref="CILMetaData"/> holding the visited signature.</param>
      /// <param name="secondMD">The <see cref="CILMetaData"/> holding the <paramref name="startingElement"/>.</param>
      /// <param name="signatureMatcher">The <see cref="Meta.SignatureMatcher"/> holding callbacks to match table indices.</param>
      public SignatureMatchingContext( SignatureElement startingElement, CILMetaData firstMD, CILMetaData secondMD, SignatureMatcher signatureMatcher )
         : base()
      {
         this.CurrentElementStack.Push( startingElement );

         this.FirstMD = ArgumentValidator.ValidateNotNull( "First meta data", firstMD );
         this.SecondMD = ArgumentValidator.ValidateNotNull( "Second meta data", secondMD );

         this.SignatureMatcher = signatureMatcher;
      }

      /// <summary>
      /// Gets the <see cref="CILMetaData"/> holding the signature being visited.
      /// </summary>
      /// <value>The <see cref="CILMetaData"/> holding the signature being visited.</value>
      public CILMetaData FirstMD { get; }

      /// <summary>
      /// Gets the <see cref="CILMetaData"/> holding the signature in <see cref="ObjectGraphEqualityContext{TElement}.CurrentElementStack"/>.
      /// </summary>
      /// <value>The <see cref="CILMetaData"/> holding the signature being visited.</value>
      public CILMetaData SecondMD { get; }

      /// <summary>
      /// Gets the <see cref="Meta.SignatureMatcher"/> holding callbacks to match table indices.
      /// </summary>
      /// <value>The <see cref="Meta.SignatureMatcher"/> holding callbacks to match table indices.</value>
      public SignatureMatcher SignatureMatcher { get; }
   }

   /// <summary>
   /// This delegate represents signature for methods that check for equality for two objects with given context.
   /// </summary>
   /// <typeparam name="TItem">The type of objects to check equality for.</typeparam>
   /// <typeparam name="TContext">The type of context.</typeparam>
   /// <param name="x">The first object.</param>
   /// <param name="y">The second object.</param>
   /// <param name="context">The context.</param>
   /// <returns><c>true</c> if <paramref name="x"/> is considered to be equal to <paramref name="y"/>; <c>false</c> otherwise.</returns>
   public delegate Boolean EqualityWithContext<TItem, TContext>( TItem x, TItem y, TContext context );

#pragma warning disable 1591
   public class CopyingContext
   {
      public CopyingContext( Boolean isDeep, Func<TableIndex, TableIndex> tableIndexTransformer )
      {
         this.IsDeepCopy = isDeep;
         this.TableIndexTransformer = tableIndexTransformer;
      }

      public Object CurrentObject { get; set; }

      public Boolean IsDeepCopy { get; }

      public Func<TableIndex, TableIndex> TableIndexTransformer { get; }
   }

   internal delegate TSignature CopySignatureDelegate<TSignature>( TSignature signature, CopyingContext context, AcceptVertexExplicitCallbackDelegate<SignatureElement, CopyingContext> callback );
}

public static partial class E_CILPhysical
{
   /// <summary>
   /// Gets simple type signature for given <see cref="SimpleTypeSignatureKind"/>, or throws an exception if no suitable signature found.
   /// </summary>
   /// <param name="sigProvider">The <see cref="SignatureProvider"/>.</param>
   /// <param name="kind">The <see cref="SimpleTypeSignatureKind"/></param>
   /// <returns>The <see cref="SimpleTypeSignature"/> for given <paramref name="kind"/></returns>
   /// <exception cref="NullReferenceException">If this <paramref name="sigProvider"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If no suitable <see cref="SimpleTypeSignature"/> is found.</exception>
   /// <seealso cref="SimpleTypeSignature"/>
   /// <seealso cref="SignatureProvider.GetSimpleTypeSignatureOrNull"/>
   public static SimpleTypeSignature GetSimpleTypeSignature( this SignatureProvider sigProvider, SimpleTypeSignatureKind kind )
   {
      var retVal = sigProvider.GetSimpleTypeSignatureOrNull( kind );
      if ( retVal == null )
      {
         throw new ArgumentException( "The type signature kind " + kind + " is not simple." );
      }
      return retVal;
   }

   /// <summary>
   /// Gets simple custom attribute type for given <see cref="CustomAttributeArgumentTypeSimpleKind"/>, or throws an exception if no suitable signature found.
   /// </summary>
   /// <param name="sigProvider">The <see cref="SignatureProvider"/>.</param>
   /// <param name="kind">The <see cref="CustomAttributeArgumentTypeSimpleKind"/></param>
   /// <returns>The <see cref="CustomAttributeArgumentTypeSimple"/> for given <paramref name="kind"/></returns>
   /// <exception cref="NullReferenceException">If this <paramref name="sigProvider"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If no suitable <see cref="CustomAttributeArgumentTypeSimple"/> is found.</exception>
   /// <seealso cref="CustomAttributeArgumentTypeSimple"/>
   /// <seealso cref="SignatureProvider.GetSimpleCATypeOrNull"/>
   public static CustomAttributeArgumentTypeSimple GetSimpleCAType( this SignatureProvider sigProvider, CustomAttributeArgumentTypeSimpleKind kind )
   {
      var retVal = sigProvider.GetSimpleCATypeOrNull( kind );
      if ( retVal == null )
      {
         throw new ArgumentException( "Unrecognized CA argument simple type kind: " + kind + "." );
      }
      return retVal;
   }

   /// <summary>
   /// Creates a new instance of <see cref="CustomAttributeArgumentType"/> based on a native type.
   /// </summary>
   /// <param name="sigProvider">The <see cref="SignatureProvider"/>.</param>
   /// <param name="type">The native type.</param>
   /// <param name="enumTypeStringFactory">The callback to get textual type string when type is <see cref="CustomAttributeValue_EnumReference"/>.</param>
   /// <returns>A new instance of <see cref="CustomAttributeArgumentType"/>, or <c>null</c> if <paramref name="type"/> is <c>null</c> or if the type could not be resolved.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="sigProvider"/> is <c>null</c></exception>
   public static CustomAttributeArgumentType ResolveCAArgumentTypeFromType( this SignatureProvider sigProvider, Type type, Func<Type, Boolean, String> enumTypeStringFactory )
   {
      return type == null ? null : sigProvider.ResolveCAArgumentTypeOrNull( type, enumTypeStringFactory );
   }

   /// <summary>
   /// Creates a new instance of <see cref="CustomAttributeArgumentType"/> based on a existing value.
   /// </summary>
   /// <param name="sigProvider">The <see cref="SignatureProvider"/>.</param>
   /// <param name="obj">The existing value.</param>
   /// <returns>A new instance of <see cref="CustomAttributeArgumentType"/>, or <c>null</c> if <paramref name="obj"/> is <c>null</c> or if the type could not be resolved.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="sigProvider"/> is <c>null</c></exception>
   public static CustomAttributeArgumentType ResolveCAArgumentTypeFromObject( this SignatureProvider sigProvider, Object obj )
   {
      return obj == null ? null : sigProvider.ResolveCAArgumentTypeOrNull( obj.GetType(), ( t, isWithinArray ) =>
      {
         String enumType = null;
         if ( isWithinArray )
         {
            var array = (Array) obj;
            if ( array.Length > 0 )
            {
               var elem = array.GetValue( 0 );
               if ( elem is CustomAttributeValue_EnumReference )
               {
                  enumType = ( (CustomAttributeValue_EnumReference) elem ).EnumType;
               }
            }
         }
         else
         {
            enumType = ( (CustomAttributeValue_EnumReference) obj ).EnumType;
         }
         return enumType;
      } );
   }

   private static CustomAttributeArgumentType ResolveCAArgumentTypeOrNull(
      this SignatureProvider sigProvider,
      Type argType,
      Func<Type, Boolean, String> enumTypeStringFactory,
      Boolean isWithinArray = false
      )
   {
      if ( argType.IsEnum )
      {
         return new CustomAttributeArgumentTypeEnum()
         {
            TypeString = argType.AssemblyQualifiedName
         };
      }
      else
      {
         switch ( Type.GetTypeCode( argType ) )
         {
            case TypeCode.Boolean:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.Boolean );
            case TypeCode.Char:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.Char );
            case TypeCode.SByte:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.I1 );
            case TypeCode.Byte:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.U1 );
            case TypeCode.Int16:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.I2 );
            case TypeCode.UInt16:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.U2 );
            case TypeCode.Int32:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.I4 );
            case TypeCode.UInt32:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.U4 );
            case TypeCode.Int64:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.I8 );
            case TypeCode.UInt64:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.U8 );
            case TypeCode.Single:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.R4 );
            case TypeCode.Double:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.R8 );
            case TypeCode.String:
               return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.String );
            case TypeCode.Object:
               if ( argType.IsArray )
               {
                  if ( isWithinArray )
                  {
                     return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.Object );
                  }
                  else
                  {
                     var arrayType = sigProvider.ResolveCAArgumentTypeOrNull( argType.GetElementType(), enumTypeStringFactory, true );
                     return arrayType == null ?
                        null :
                        new CustomAttributeArgumentTypeArray()
                        {
                           ArrayType = arrayType
                        };
                  }
               }
               else
               {
                  // Check for enum reference
                  if ( Equals( typeof( CustomAttributeValue_EnumReference ), argType ) )
                  {
                     var enumTypeStr = enumTypeStringFactory( argType, isWithinArray );
                     return enumTypeStr == null ? null : new CustomAttributeArgumentTypeEnum()
                     {
                        TypeString = enumTypeStr
                     };
                  }
                  // System.Type or System.Object or CustomAttributeTypeReference
                  else if ( Equals( typeof( CustomAttributeValue_TypeReference ), argType ) || Equals( typeof( Type ), argType ) )
                  {
                     return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.Type );
                  }
                  else if ( isWithinArray && Equals( typeof( Object ), argType ) )
                  {
                     return sigProvider.GetSimpleCATypeOrNull( CustomAttributeArgumentTypeSimpleKind.Object );
                  }
                  else
                  {
                     return null;
                     //throw new InvalidOperationException( "Failed to deduce custom attribute type for " + argType + "." );
                  }
               }
            default:
               return null;
               //throw new InvalidOperationException( "Failed to deduce custom attribute type for " + argType + "." );
         }
      }
   }

   /// <summary>
   /// Gets or creates a new <see cref="SignatureProvider"/>.
   /// </summary>
   /// <param name="provider">The <see cref="MetaDataTableInformationProvider"/>.</param>
   /// <returns>A <see cref="SignatureProvider"/> supported by this <see cref="MetaDataTableInformationProvider"/>.</returns>
   /// <seealso cref="SignatureProvider"/>
   public static SignatureProvider CreateSignatureProvider( this MetaDataTableInformationProvider provider )
   {
      return provider.GetFunctionality<SignatureProvider>();
   }

   /// <summary>
   /// Returns enumerable of leaf signature elements of given <see cref="AbstractSignature"/>.
   /// </summary>
   /// <param name="provider"></param>
   /// <param name="signature">The <see cref="AbstractSignature"/> to decompose.</param>
   /// <returns>The recursive enumerable of all <see cref="SignatureElement"/>s of <paramref name="signature"/>. Will be empty if <paramref name="signature"/> is <c>null</c>.</returns>

   public static IEnumerable<SignatureElement> DecomposeSignature( this SignatureProvider provider, AbstractSignature signature )
   {
      var decomposer = provider.GetFunctionality<TypeBasedAcceptor<SignatureElement, Int32, DecomposeSignatureContext>>();
      var decomposeContext = new DecomposeSignatureContext();
      decomposer.Accept( signature, decomposeContext );
      return decomposeContext.Elements;
   }

   /// <summary>
   /// Performs structural match on two signatures.
   /// </summary>
   /// <param name="provider">The <see cref="SignatureProvider"/>.</param>
   /// <param name="firstMD">The <see cref="CILMetaData"/> containing the <paramref name="firstSignature"/>.</param>
   /// <param name="firstSignature">The first <see cref="AbstractSignature"/>.</param>
   /// <param name="secondMD">The <see cref="CILMetaData"/> containing the <paramref name="secondSignature"/>.</param>
   /// <param name="secondSignature">the second <see cref="AbstractSignature"/>.</param>
   /// <param name="matcher">The object capturing callbacks to perform non-structural compare.</param>
   /// <returns>Whether <paramref name="firstSignature"/> and <paramref name="secondSignature"/> match structurally and using the given <paramref name="matcher"/>.</returns>
   public static Boolean MatchSignatures( this SignatureProvider provider, CILMetaData firstMD, AbstractSignature firstSignature, CILMetaData secondMD, AbstractSignature secondSignature, SignatureMatcher matcher )
   {
      return provider.GetFunctionality<TypeBasedAcceptor<SignatureElement, Int32, SignatureMatchingContext>>()
         .Accept( firstSignature, new SignatureMatchingContext( secondSignature, firstMD, secondMD, matcher ) );
   }


   /// <summary>
   /// Extracts all <see cref="SignatureTableIndexInfo"/> related to a single signature.
   /// </summary>
   /// <param name="provider">The <see cref="SignatureProvider"/>.</param>
   /// <param name="signature">The <see cref="AbstractSignature"/>.</param>
   /// <returns>A list of all <see cref="SignatureTableIndexInfo"/> related to a single signature. Will be empty if <paramref name="signature"/> is <c>null</c>.</returns>
   public static IEnumerable<SignatureTableIndexInfo> GetSignatureTableIndexInfos( this SignatureProvider provider, AbstractSignature signature )
   {
      var collector = provider.GetFunctionality<TypeBasedAcceptor<SignatureElement, Int32, TableIndexCollectorContext>>();
      var collectorContext = new TableIndexCollectorContext();
      collector.Accept( signature, collectorContext );
      return collectorContext.Elements;
   }

   /// <summary>
   /// Creates a new instance of signature of given type, which will contain a shallow or deep copy of this signature.
   /// </summary>
   /// <typeparam name="TSignature">The type of signature reference.</typeparam>
   /// <param name="provider">The <see cref="SignatureProvider"/>.</param>
   /// <param name="sig">The <see cref="AbstractSignature"/>.</param>
   /// <param name="tableIndexTranslator">Optional callback to translate table indices of <see cref="CustomModifierSignature.CustomModifierType"/> and <see cref="ClassOrValueTypeSignature.Type"/> properties while copying.</param>
   /// <param name="isDeep">Whether the copy is deep.</param>
   /// <returns>A new instance of <typeparamref name="TSignature"/>, which has all of its contents deeply copied from given signature.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="sig"/> is <c>null</c>.</exception>
   /// <exception cref="NotSupportedException">If <see cref="AbstractSignature.SignatureKind"/> returns any other value than what the <see cref="SignatureKind"/> enumeration has.</exception>
   public static TSignature CreateCopy<TSignature>( this SignatureProvider provider, TSignature sig, Boolean isDeep, Func<TableIndex, TableIndex> tableIndexTranslator = null )
      where TSignature : AbstractSignature
   {
      TSignature retVal;
      if ( sig == null )
      {
         retVal = sig;
      }
      else
      {
         var acceptor = provider.GetFunctionality<TypeBasedAcceptor<SignatureElement, Int32, CopyingContext>>();
         var ctx = new CopyingContext( isDeep, tableIndexTranslator );
         if ( !acceptor.AcceptExplicit( sig, ctx ) )
         {
            throw new NotSupportedException( "Could not find functionality to copy signature or part of it." );
         }
         retVal = (TSignature) ctx.CurrentObject;
      }
      return retVal;
   }

   //internal static void RegisterVisitor<TElement, TActualElement>( this TypeBasedVisitor<TElement> visitorAggregator, VisitElementDelegateTyped<TElement, TActualElement> visitor )
   //   where TActualElement : TElement
   //{
   //   visitorAggregator.RegisterVisitor( typeof( TActualElement ), ( el, cb ) => visitor( (TActualElement) el, cb ) );
   //}

   //internal static void RegisterSignatureVisitor<TSignature>( this TypeBasedVisitor<SignatureElement> visitorAggregator, VisitElementDelegateTyped<SignatureElement, TSignature> visitor )
   //   where TSignature : SignatureElement
   //{
   //   visitorAggregator.RegisterVisitor( visitor );
   //}


   internal static void RegisterSignatureDecomposer<TSignature>( this TypeBasedAcceptor<SignatureElement, Int32, DecomposeSignatureContext> acceptorAggregator, AcceptVertexDelegate<TSignature, DecomposeSignatureContext> acceptor )
      where TSignature : SignatureElement
   {
      acceptorAggregator.RegisterAcceptor( acceptor );
   }

   internal static void RegisterSignatureMatcher<TSignature>( this TypeBasedAcceptor<SignatureElement, Int32, SignatureMatchingContext> acceptorAggregator, EqualityWithContext<TSignature, SignatureMatchingContext> acceptor )
      where TSignature : class, SignatureElement
   {
      acceptorAggregator.RegisterVertexAcceptor( typeof( TSignature ), ( el, ctx ) =>
      {
         var fromCtx = ctx.GetCurrentElement();
         TSignature fromCtxTyped;
         return ReferenceEquals( el, fromCtx )
         || ( el != null && ( fromCtxTyped = fromCtx as TSignature ) != null
         && acceptor( (TSignature) el, fromCtxTyped, ctx ) );
      } );
   }

   internal static void RegisterSignatureMatcherTransitionForSimple<TSignature>( this TypeBasedAcceptor<SignatureElement, Int32, SignatureMatchingContext> acceptorAggregator, String propertyName, Func<TSignature, SignatureElement> getter )
   {
      acceptorAggregator.RegisterEdgeAcceptor(
         typeof( TSignature ),
         propertyName,
         ( el, info, ctx ) =>
         {
            ctx.CurrentElementStack.Push( getter( (TSignature) ctx.GetCurrentElement() ) );
            return true;
         },
         ( el, info, ctx ) =>
         {
            ctx.CurrentElementStack.Pop();
            return true;
         } );
   }

   internal static void RegisterSignatureMatcherTransitionForLists<TSignature, TListElement>( this TypeBasedAcceptor<SignatureElement, Int32, SignatureMatchingContext> acceptorAggregator, String propertyName, Func<TSignature, List<TListElement>> listGetter )
      where TListElement : SignatureElement
   {
      acceptorAggregator.RegisterEdgeAcceptor(
         typeof( TSignature ),
         propertyName,
         ( el, info, ctx ) =>
         {
            var list = listGetter( (TSignature) ctx.GetCurrentElement() );
            ctx.CurrentElementStack.Push( list[(Int32) info] );
            return true;
         },
         ( el, info, ctx ) =>
         {
            ctx.CurrentElementStack.Pop();
            return true;
         } );
   }


   //internal static void RegisterSignatureElementDecomposer<TElement>( this TypeBasedAcceptor<AbstractSignature, DecomposeSignatureContext> acceptorAggregator, AcceptElementDelegateWithContext<TElement, DefaultSignatureProvider.DecomposeSignatureContext> acceptor )
   //   where TElement : SignatureElement
   //{
   //   acceptorAggregator.RegisterNonConformingAcceptor( typeof( TElement ), ( el, nc, ctx ) => acceptor( (TElement) el, nc, ctx ) );
   //}

   internal static SignatureElement GetCurrentElement( this SignatureMatchingContext ctx )
   {
      return ctx.CurrentElementStack.Peek();
   }



   internal static TSignature CloneSingle<TSignature>( this CopyingContext context, TSignature signature, AcceptVertexExplicitCallbackDelegate<SignatureElement, CopyingContext> callback )
      where TSignature : class, SignatureElement
   {
      if ( signature != null && context.IsDeepCopy )
      {
         callback( signature, context );
         return (TSignature) context.CurrentObject;
      }
      else
      {
         return signature;
      }
   }

   internal static void CloneList<TSignature>( this CopyingContext context, List<TSignature> listToWrite, List<TSignature> listToRead, AcceptVertexExplicitCallbackDelegate<SignatureElement, CopyingContext> callback )
      where TSignature : class, SignatureElement
   {
      if ( context.IsDeepCopy )
      {
         var max = listToRead.Count;
         for ( var i = 0; i < max; ++i )
         {
            var cur = listToRead[i];
            var curCopy = context.CloneSingle( cur, callback );
            listToWrite.Add( curCopy );
         }
      }
      else
      {
         listToWrite.AddRange( listToRead );
      }
   }

   internal static VisitorVertexInfoFactory<SignatureElement, Int32> CreateSignatureVertexInfoFactory<TSignature>( this TypeBasedVisitor<SignatureElement, Int32> visitor )
      where TSignature : SignatureElement
   {
      return visitor.CreateVertexInfoFactory( typeof( TSignature ) );
   }

}
