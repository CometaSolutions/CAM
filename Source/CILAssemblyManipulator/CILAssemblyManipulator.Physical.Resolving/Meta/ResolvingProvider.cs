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
extern alias CAMPhysical;
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;
using CAMPhysical::CILAssemblyManipulator.Physical.Meta;

//using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.Meta;
using CILAssemblyManipulator.Physical.Resolving;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TabularMetaData;
using TabularMetaData.Meta;

namespace CILAssemblyManipulator.Physical.Meta
{
   /// <summary>
   /// This class provides creation of <see cref="ResolvingProvider"/> through a callback.
   /// </summary>
   public sealed class ResolvingProviderProvider
   {
      private readonly Func<CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData, ResolvingProvider> _callback;

      /// <summary>
      /// Creates a new instance of <see cref="ResolvingProviderProvider"/> with given callback.
      /// </summary>
      /// <param name="callback">The callback to create <see cref="ResolvingProvider"/>.</param>
      public ResolvingProviderProvider( Func<CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData, ResolvingProvider> callback )
      {
         ArgumentValidator.ValidateNotNull( "Callback", callback );

         this._callback = callback;
      }

      /// <summary>
      /// Creates a new <see cref="ResolvingProvider"/> for a given <see cref="CILMetaData"/>.
      /// </summary>
      /// <param name="thisMD">The <see cref="CILMetaData"/>.</param>
      /// <returns>A possibly new instance of <see cref="ResolvingProvider"/> for this <see cref="CILMetaData"/>.</returns>
      /// <remarks>
      /// The returned <see cref="ResolvingProvider"/> will be accessible via <see cref="CILMetaData.ResolvingProvider"/> property.
      /// </remarks>
      public ResolvingProvider CreateResolvingProvider( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData thisMD )
      {
         return this._callback( thisMD );
      }
   }

   /// <summary>
   /// The sole purpose of this interface is to provide resolving capability of <see cref="RawCustomAttributeSignature"/>s and <see cref="RawSecurityInformation"/>s into <see cref="ResolvedCustomAttributeSignature"/>s and <see cref="SecurityInformation"/>, respectively.
   /// This interface provides the resolving capability in such way that adding new tables with columns that require resolving is easy.
   /// </summary>
   /// <remarks>
   /// <para>
   /// This class is rarely used directly, as e.g. <see cref="T:CILAssemblyManipulator.Physical.IO.CILMetaDataLoader"/> will use this by default in <see cref="T:CILAssemblyManipulator.Physical.IO.CILMetaDataLoader.ResolveMetaData()"/> method.
   /// In order to fully utilize this class, one should register to <see cref="MetaDataResolver.AssemblyReferenceResolveEvent"/> and <see cref="MetaDataResolver.ModuleReferenceResolveEvent"/> events.
   /// </para>
   /// <para>
   /// The custom attribute signatures are serialized in meta data (see ECMA-335 spec for more info) in such way that enum values have their type names present, but the underlying enum value type (e.g. integer) is not present.
   /// Therefore, the custom attribute signatures, and security signatures (which share some serialization functionality with custom attribute signatures) require dynamic resolving of what is the underlying enum value type.
   /// This class encapsulates this resolving process, which may be complicated and involve loading of several dependant assemblies.
   /// </para>
   /// </remarks>
   public interface ResolvingProvider
   {
      /// <summary>
      /// Gets the <see cref="MetaDataResolver"/> for this <see cref="ResolvingProvider"/>.
      /// </summary>
      /// <value>The <see cref="MetaDataResolver"/> for this <see cref="ResolvingProvider"/>.</value>
      MetaDataResolver Resolver { get; }

      /// <summary>
      /// Uses this <see cref="Resolver"/> to resolve a column value at given table, row, and column.
      /// </summary>
      /// <param name="table">The <see cref="Tables"/> containing the row.</param>
      /// <param name="rowIndex">The index of the row.</param>
      /// <param name="columnIndex">The index of the column.</param>
      /// <returns><c>true</c> if unresolved column value was converted to resolved value; <c>false</c> otherwise (including situation when the value already was resolved).</returns>
      Boolean Resolve( Tables table, Int32 rowIndex, Int32 columnIndex );

      /// <summary>
      /// Gets value indicating how many resolvable columns are in given <see cref="Tables"/> table.
      /// </summary>
      /// <param name="table">The <see cref="Tables"/>.</param>
      /// <returns>The amount of resolvable columns in given <see cref="Tables"/>.</returns>
      Int32 GetResolvableColumnsCount( Tables table );
   }

   /// <summary>
   /// This class provides default implementation for <see cref="ResolvingProvider"/>.
   /// </summary>
   public class DefaultResolvingProvider : ResolvingProvider
   {

      private readonly IDictionary<Tables, List<Tuple<MetaDataColumnInformationWithResolvingCapability, Object>>> _columnSpecificCaches;

      private readonly CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData _md;

      /// <summary>
      /// Creates a new instance of <see cref="DefaultResolvingProvider"/> with given <see cref="CILMetaData"/>.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/>.</param>
      /// <param name="resolvableColumns">All of the resolvable columns.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="md"/> is <c>null</c>.</exception>
      public DefaultResolvingProvider(
         CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData md,
         IEnumerable<Tuple<Tables, MetaDataColumnInformationWithResolvingCapability>> resolvableColumns
         )
      {
         ArgumentValidator.ValidateNotNull( "Meta data", md );

         this._md = md;

         this.Resolver = new DefaultMetaDataResolver();
         var cols = new Dictionary<Tables, List<Tuple<MetaDataColumnInformationWithResolvingCapability, Object>>>();
         foreach ( var resolvableColumn in ( resolvableColumns ?? Empty<Tuple<Tables, MetaDataColumnInformationWithResolvingCapability>>.Enumerable )
            .Where( rc => rc != null ) )
         {
            cols.GetOrAdd_NotThreadSafe( resolvableColumn.Item1, t => new List<Tuple<MetaDataColumnInformationWithResolvingCapability, Object>>() )
               .Add( Tuple.Create( resolvableColumn.Item2, resolvableColumn.Item2.CreateCache() ) );
         }
         this._columnSpecificCaches = cols;
      }

      /// <inheritdoc />
      public MetaDataResolver Resolver { get; }

      /// <inheritdoc />
      public Boolean Resolve( Tables table, Int32 rowIndex, Int32 columnIndex )
      {
         var tuple = this._columnSpecificCaches[table][columnIndex];
         return tuple.Item1.Resolve( this._md, rowIndex, this.Resolver, tuple.Item2 );
      }

      /// <inheritdoc />
      public Int32 GetResolvableColumnsCount( Tables table )
      {
         return this._columnSpecificCaches.GetOrDefault( table )?.Count ?? 0;
      }
   }

   /// <summary>
   /// This interface is used by <see cref="DefaultResolvingProvider"/> to delegate the resolving functionality of <see cref="ResolvingProvider.Resolve"/>.
   /// </summary>
   public interface MetaDataColumnInformationWithResolvingCapability
   {
      /// <summary>
      /// This should create a new cache object specific for this column.
      /// </summary>
      /// <returns>A new cache object specific for this column.</returns>
      Object CreateCache();

      /// <summary>
      /// Performs the resolving.
      /// </summary>
      /// <param name="md">The <see cref="CILMetaData"/>.</param>
      /// <param name="rowIndex">The row index.</param>
      /// <param name="resolver">The <see cref="MetaDataResolver"/>.</param>
      /// <param name="cache">The cache created by <see cref="CreateCache"/>.</param>
      /// <returns><c>true</c> if column value was successfully converter from unresolved value to resolved value; <c>false</c> otherwise.</returns>
      Boolean Resolve( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData md, Int32 rowIndex, MetaDataResolver resolver, Object cache );
   }

}

#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{
   /// <summary>
   /// Helper method to resolve all custom attributes of given <see cref="CILMetaData"/> using <see cref="ResolveCustomAttributeSignature"/>.
   /// </summary>
   /// <param name="md">This <see cref="CILMetaData"/>.</param>
   /// <exception cref="NullReferenceException">If this <see cref="CILMetaData"/> is <c>null</c>.</exception>
   public static void ResolveAllCustomAttributes( this CILMetaData md )
   {
      md.UseResolver( md.CustomAttributeDefinitions, ( m, i ) => m.ResolveCustomAttributeSignature( i ) );
   }

   /// <summary>
   /// Helper method to resolve all security signatures of given <see cref="CILMetaData"/> using <see cref="ResolveSecurityDeclaration"/>.
   /// </summary>
   /// <param name="md">This <see cref="CILMetaData"/>.</param>
   /// <exception cref="NullReferenceException">If this <see cref="CILMetaData"/> is <c>null</c>.</exception>
   public static void ResolveAllSecurityInformation( this CILMetaData md )
   {
      md.UseResolver( md.SecurityDefinitions, ( m, i ) => m.ResolveSecurityDeclaration( i ) );
   }

   /// <summary>
   /// Resolves a <see cref="CustomAttributeDefinition.Signature"/> at given row index.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/> containing the <see cref="CustomAttributeDefinition"/>.</param>
   /// <param name="rowIndex">The row index.</param>
   /// <returns><c>true</c> if signature successfully resolved; <c>false</c> otherwise.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="CILMetaData"/> is <c>null</c>.</exception>
   public static Boolean ResolveCustomAttributeSignature( this CILMetaData md, Int32 rowIndex )
   {
      return ( (CILAssemblyManipulator.Physical.CILMetaData) md ).ResolvingProvider.Resolve( Tables.CustomAttribute, rowIndex, 0 );
   }

   /// <summary>
   /// Resolves a <see cref="SecurityDefinition.PermissionSets"/> at given row index.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/> containing the <see cref="SecurityDefinition"/>.</param>
   /// <param name="rowIndex">The row index.</param>
   /// <returns><c>true</c> if permission sets successfully resolved; <c>false</c> otherwise.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="CILMetaData"/> is <c>null</c>.</exception>
   public static Boolean ResolveSecurityDeclaration( this CILMetaData md, Int32 rowIndex )
   {
      return ( (CILAssemblyManipulator.Physical.CILMetaData) md ).ResolvingProvider.Resolve( Tables.DeclSecurity, rowIndex, 0 );
   }

   /// <summary>
   /// Helper method to resolve custom attribute signatures and security signatures in given <see cref="CILMetaData"/>.
   /// </summary>
   /// <param name="md">This <see cref="CILMetaData"/>.</param>
   /// <exception cref="NullReferenceException">If this <see cref="CILMetaData"/> is <c>null</c>.</exception>
   public static void ResolveEverything( this CILMetaData md )
   {
      var resolver = ( (CILAssemblyManipulator.Physical.CILMetaData) md ).ResolvingProvider;
      foreach ( var table in md.GetAllTables() )
      {
         var tableEnum = (Tables) table.GetTableIndex();
         var colCount = resolver.GetResolvableColumnsCount( tableEnum );
         if ( colCount > 0 )
         {
            var info = table.TableInformationNotGeneric;
            for ( var i = 0; i < table.TableContentsNotGeneric.Count; ++i )
            {
               for ( var j = 0; j < colCount; ++j )
               {
                  resolver.Resolve( tableEnum, i, j );
               }
            }
         }
      }
   }

   private static void UseResolver<T>(
      this CILMetaData md,
      MetaDataTable<T> list,
      Action<CILMetaData, Int32> action
      )
      where T : class
   {

      var max = list.GetRowCount();
      for ( var i = 0; i < max; ++i )
      {
         action( md, i );
      }
   }

   /// <summary>
   /// This is helper method to search for custom attribute of type <see cref="System.Runtime.Versioning.TargetFrameworkAttribute"/> attribute applied to the assembly, and creates a <see cref="TargetFrameworkInfo"/> based on the information in the custom attribute signature.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <param name="fwInfo">This parameter will contain the <see cref="TargetFrameworkInfo"/> created based on the information in the assembly.</param>
   /// <returns><c>true</c> if suitable attribute is found, and the information in the signature is enough to create <see cref="TargetFrameworkInfo"/>; <c>false</c> otherwise.</returns>
   /// <remarks>
   /// <para>
   /// In case of multiple matching custom attributes, the first one in <see cref="CILMetaData.CustomAttributeDefinitions"/> table is used.
   /// </para>
   /// <para>
   /// The assemblies in target framework directory usually don't have the <see cref="System.Runtime.Versioning.TargetFrameworkAttribute"/> on them.
   /// </para>
   /// </remarks>
   /// <exception cref="NullReferenceException">If <paramref name="md"/> is <c>null</c>.</exception>
   public static Boolean TryGetTargetFrameworkInformation( this CILMetaData md, out TargetFrameworkInfo fwInfo )
   {
      fwInfo = md.CustomAttributeDefinitions.TableContents
         .Where( ( ca, caIdx ) =>
         {
            var isTargetFWAttribute = false;
            if ( ca.Parent.Table == Tables.Assembly
            && md.AssemblyDefinitions.GetOrNull( ca.Parent.Index ) != null
            && ca.Type.Table == Tables.MemberRef ) // Remember that framework assemblies don't have TargetFrameworkAttribute defined
            {
               var memberRef = md.MemberReferences.GetOrNull( ca.Type.Index );
               if ( memberRef != null
                  && memberRef?.Signature?.SignatureKind == SignatureKind.MethodReference
                  && memberRef.DeclaringType.Table == Tables.TypeRef
                  && String.Equals( memberRef.Name, Miscellaneous.INSTANCE_CTOR_NAME )
                  )
               {
                  var typeRef = md.TypeReferences.GetOrNull( memberRef.DeclaringType.Index );
                  if ( typeRef != null
                     && typeRef.ResolutionScope.HasValue
                     && typeRef.ResolutionScope.Value.Table == Tables.AssemblyRef
                     && String.Equals( typeRef.Namespace, "System.Runtime.Versioning" )
                     && String.Equals( typeRef.Name, "TargetFrameworkAttribute" )
                     )
                  {
                     if ( ca.Signature is RawCustomAttributeSignature )
                     {
                        // Use resolver with no events, so nothing additional will be loaded (and is not required, as both arguments are strings
                        md.ResolveCustomAttributeSignature( caIdx );
                     }

                     var caSig = ca.Signature as ResolvedCustomAttributeSignature;
                     if ( caSig != null
                        && caSig.TypedArguments.Count > 0
                        )
                     {
                        // Resolving succeeded
                        isTargetFWAttribute = true;
                     }
#if DEBUG
                     else
                     {
                        // Breakpoint (resolving failed, even though it should have succeeded
                     }
#endif
                  }
               }
            }
            return isTargetFWAttribute;
         } )
         .Select( ca =>
         {

            var fwInfoString = ( (ResolvedCustomAttributeSignature) ca.Signature ).TypedArguments[0].Value.ToStringSafe( null );
            //var displayName = caSig.NamedArguments.Count > 0
            //   && String.Equals( caSig.NamedArguments[0].Name, "FrameworkDisplayName" )
            //   && caSig.NamedArguments[0].Value.Type.IsSimpleTypeOfKind( SignatureElementTypes.String ) ?
            //   caSig.NamedArguments[0].Value.Value.ToStringSafe( null ) :
            //   null;
            TargetFrameworkInfo thisFWInfo;
            return TargetFrameworkInfo.TryParse( fwInfoString, out thisFWInfo ) ? thisFWInfo : null;

         } )
         .FirstOrDefault();

      return fwInfo != null;
   }

   /// <summary>
   /// Wrapper around <see cref="TryGetTargetFrameworkInformation"/>, that will always return <see cref="TargetFrameworkInfo"/>, but it will be <c>null</c> if <see cref="TryGetTargetFrameworkInformation"/> will return <c>false</c>.
   /// </summary>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <returns>The parsed <see cref="TargetFrameworkInfo"/> object, or <c>null</c> if such information could not be found from <paramref name="md"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="md"/> is <c>null</c>.</exception>
   public static TargetFrameworkInfo GetTargetFrameworkInformationOrNull( this CILMetaData md )
   {
      TargetFrameworkInfo retVal;
      return md.TryGetTargetFrameworkInformation( out retVal ) ?
         retVal :
         null;
   }

   /// <summary>
   /// Gets or creates a new <see cref="ResolvingProvider"/>.
   /// </summary>
   /// <param name="provider">The <see cref="MetaDataTableInformationProvider"/>.</param>
   /// <param name="md">The <see cref="CILMetaData"/>.</param>
   /// <returns>A <see cref="ResolvingProvider"/> supported by this <see cref="MetaDataTableInformationProvider"/>.</returns>
   /// <seealso cref="ResolvingProvider"/>
   public static ResolvingProvider CreateResolvingProvider( this MetaDataTableInformationProvider provider, CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData md )
   {
      return provider.GetFunctionality<ResolvingProviderProvider>()?.CreateResolvingProvider( md );
   }
}