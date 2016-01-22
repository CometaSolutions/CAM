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
using CAMPhysical;
using CAMPhysical::CILAssemblyManipulator.Physical;

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.IO;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.IO
{
   public interface TargetFrameworkMapper
   {
      Boolean TryReMapReference(
         CILMetaData thisMD,
         AssemblyInformationForResolving aRef,
         String fullType,
         CILMetaDataLoaderWithCallbacks loader,
         TargetFrameworkInfoWithRetargetabilityInformation targetFW,
         out AssemblyReference newRef,
         out Boolean wasTargetFW
         );

      Boolean ProcessTypeString(
         CILMetaData thisMD,
         CILMetaDataLoaderWithCallbacks loader,
         TargetFrameworkInfoWithRetargetabilityInformation targetFW,
         ref String typeString
         );
   }

   public sealed class TargetFrameworkInfoWithRetargetabilityInformation
   {
      public TargetFrameworkInfoWithRetargetabilityInformation(
         TargetFrameworkInfo targetFramework,
         Boolean assemblyReferencesRetargetable
         )
      {
         ArgumentValidator.ValidateNotNull( "Target framework information", targetFramework );

         this.TargetFrameworkInfo = targetFramework;
         this.AreFrameworkAssemblyReferencesRetargetable = assemblyReferencesRetargetable;
      }

      public TargetFrameworkInfo TargetFrameworkInfo { get; }

      public Boolean AreFrameworkAssemblyReferencesRetargetable { get; }
   }

   public abstract class AbstractTargetFrameworkMapper<
      TTypesDic,
      TTargetFWAssembliesDic,
      TResolvedTargetFWAssembliesDic,
      TResolvedTargetFWAssembliesDicInner,
      TAssemblyReferenceDic,
      TAssemblyReferenceDicInner
      > : TargetFrameworkMapper
      where TTypesDic : class, IDictionary<CILMetaData, ISet<String>>
      where TTargetFWAssembliesDic : class, IDictionary<TargetFrameworkInfo, String[]>
      where TResolvedTargetFWAssembliesDic : class, IDictionary<CILMetaData, TResolvedTargetFWAssembliesDicInner>
      where TResolvedTargetFWAssembliesDicInner : class, IDictionary<String, CILMetaData>
      where TAssemblyReferenceDic : class, IDictionary<CILMetaData, TAssemblyReferenceDicInner>
      where TAssemblyReferenceDicInner : class, IDictionary<AssemblyInformationForResolving, CILMetaData>
   {
      private readonly TTypesDic _mdTypes;
      private readonly TTargetFWAssembliesDic _targetFWAssemblies;
      private readonly TResolvedTargetFWAssembliesDic _resolvedTargetFWAssemblies;
      private readonly TAssemblyReferenceDic _assemblyReferenceInfo;

      private readonly Func<CILMetaData, TResolvedTargetFWAssembliesDicInner> _resolvedInnerFactory;
      private readonly Func<CILMetaData, TAssemblyReferenceDicInner> _assemblyReferenceInnerFactory;

      internal AbstractTargetFrameworkMapper(
         TTypesDic mdTypes,
         TTargetFWAssembliesDic targetFWAssemblies,
         TResolvedTargetFWAssembliesDic resolvedTargetFWAssemblies,
         TAssemblyReferenceDic assemblyReferences,
         Func<CILMetaData, TResolvedTargetFWAssembliesDicInner> resolvedInnerFactory,
         Func<CILMetaData, TAssemblyReferenceDicInner> assemblyReferenceInnerFactory
         )
      {
         ArgumentValidator.ValidateNotNull( "Meta data type dictionary", mdTypes );
         ArgumentValidator.ValidateNotNull( "Target framework assemblies dictionary", targetFWAssemblies );
         ArgumentValidator.ValidateNotNull( "Resolved target framework assemblies dictionary", resolvedTargetFWAssemblies );
         ArgumentValidator.ValidateNotNull( "Assembly reference dictionary", assemblyReferences );
         ArgumentValidator.ValidateNotNull( "Resolved target framework assemblies inner dictionary factory", resolvedInnerFactory );
         ArgumentValidator.ValidateNotNull( "Assembly reference inner dictionary factory", assemblyReferenceInnerFactory );

         this._mdTypes = mdTypes;
         this._targetFWAssemblies = targetFWAssemblies;
         this._resolvedTargetFWAssemblies = resolvedTargetFWAssemblies;
         this._assemblyReferenceInfo = assemblyReferences;
         this._resolvedInnerFactory = resolvedInnerFactory;
         this._assemblyReferenceInnerFactory = assemblyReferenceInnerFactory;
      }

      public Boolean TryReMapReference(
         CILMetaData thisMD,
         AssemblyInformationForResolving aRef,
         String fullType,
         CILMetaDataLoaderWithCallbacks loader,
         TargetFrameworkInfoWithRetargetabilityInformation targetFW,
         out AssemblyReference newRef,
         out Boolean wasTargetFW
         )
      {
         newRef = null;

         var targetFWAssembly = this.ResolveTargetFWReferenceOrNull( thisMD, aRef, loader, targetFW );
         var retVal = targetFWAssembly != null;
         wasTargetFW = retVal;
         if ( retVal )
         {
            var actualTargetFWAssembly = this.GetActualMDForType( targetFWAssembly, loader, fullType, targetFW.TargetFrameworkInfo );

            if ( actualTargetFWAssembly == null )
            {
               throw new InvalidOperationException( "Failed to map type " + fullType + " in " + loader.GetResourceFor( targetFWAssembly ) + " to target framework " + targetFW + "." );
            }
            else
            {
               retVal = !ReferenceEquals( targetFWAssembly, actualTargetFWAssembly );
               if ( retVal )
               {
                  // Type was in another assembly
                  newRef = actualTargetFWAssembly.AssemblyDefinitions.TableContents[0].AsAssemblyReference();
               }
            }
         }

         return retVal;
      }

      public Boolean ProcessTypeString(
         CILMetaData thisMD,
         CILMetaDataLoaderWithCallbacks loader,
         TargetFrameworkInfoWithRetargetabilityInformation targetFW,
         ref String typeString
         )
      {
         String typeName, assemblyName;
         AssemblyInformation assemblyInfo;
         Boolean isFullPublicKey;
         AssemblyReference newRef = null;
         Boolean wasTargetFW;
         var retVal = typeString.ParseAssemblyQualifiedTypeString( out typeName, out assemblyName )
            && AssemblyInformation.TryParse( assemblyName, out assemblyInfo, out isFullPublicKey )
            && this.TryReMapReference( thisMD, new AssemblyInformationForResolving( assemblyInfo, isFullPublicKey ), typeName, loader, targetFW, out newRef, out wasTargetFW );

         if ( retVal )
         {
            assemblyName = newRef.ToString();
            typeString = Miscellaneous.CombineAssemblyAndType( assemblyName, typeName );
         }

         return retVal;
      }

      private CILMetaData GetActualMDForType(
         CILMetaData targetFWAssembly,
         CILMetaDataLoaderWithCallbacks loader,
         String fullType,
         TargetFrameworkInfo newTargetFW
         )
      {
         return this.GetOrAdd_ResolvedTargetFWAssembliesInner(
            this.GetOrAdd_ResolvedTargetFWAssemblies( this._resolvedTargetFWAssemblies, targetFWAssembly, this._resolvedInnerFactory ),
            fullType,
            typeStr =>
               this.GetSuitableMDsForTargetFW( targetFWAssembly, loader, newTargetFW, true )
                  .FirstOrDefault( md => this.IsTypePresent( md, typeStr ) )
            );
      }

      private CILMetaData ResolveTargetFWReferenceOrNull(
         CILMetaData thisMD,
         AssemblyInformationForResolving assemblyRef,
         CILMetaDataLoaderWithCallbacks loader,
         TargetFrameworkInfoWithRetargetabilityInformation targetFW
         )
      {
         return this.GetOrAdd_AssemblyReferencesInner(
            this.GetOrAdd_AssemblyReferences( this._assemblyReferenceInfo, thisMD, this._assemblyReferenceInnerFactory ),
            assemblyRef,
            aRef =>
            {
               var cb = loader.LoaderCallbacks;
               var validResource = cb
                  .GetPossibleResourcesForAssemblyReference( loader.GetResourceFor( thisMD ), thisMD, aRef, null )
                  .Where( res => cb.IsValidResource( res ) )
                  .FirstOrDefault();
               CILMetaData retVal;
               if ( validResource == null )
               {
                  // Most likely this metadata didn't have target framework info attribute
                  retVal = this.GetSuitableMDsForTargetFW( thisMD, loader, targetFW.TargetFrameworkInfo, false )
                     .FirstOrDefault( md => md.AssemblyDefinitions.GetOrNull( 0 ).IsMatch( assemblyRef, targetFW.AreFrameworkAssemblyReferencesRetargetable, loader.PublicKeyComputer ) );
               }
               else if ( validResource.StartsWith( cb.GetTargetFrameworkPathForFrameworkInfo( targetFW.TargetFrameworkInfo ) ) ) // Check whether resolved reference is located in target framework path
               {
                  retVal = loader.GetOrLoadMetaData( validResource );
               }
               else
               {
                  retVal = null;
               }
               return retVal;
            } );
      }

      private Boolean IsTypePresent( CILMetaData metaData, String typeName )
      {
         return this.GetOrAdd_MDTypes( this._mdTypes, metaData, md => new HashSet<String>( md.GetTypeDefinitionsFullNames() ) )
            .Contains( typeName );
      }

      private IEnumerable<CILMetaData> GetSuitableMDsForTargetFW(
         CILMetaData md,
         CILMetaDataLoaderWithCallbacks loader,
         TargetFrameworkInfo targetFW,
         Boolean returnThis
         )
      {
         if ( returnThis )
         {
            // Always try current library at first
            yield return md;
         }

         // Then start enumerating all the rest of the assemblies in target framework directory
         foreach ( var res in this.GetTargetFWAssemblies( targetFW, loader ).Where( r => !this.IsRecordedNotManagedAssembly( r ) ) )
         {
            CILMetaData current;
            try
            {
               current = loader.GetOrLoadMetaData( res );
            }
            catch ( MetaDataLoadException e )
            {
               if ( e.InnerException is NotAManagedModuleException )
               {
                  current = null;
                  this.RecordNotManagedAssembly( res );
               }
               else
               {
                  throw;
               }
            }

            if ( current != null && !ReferenceEquals( md, current ) )
            {
               yield return current;
            }
         }
      }

      private String[] GetTargetFWAssemblies( TargetFrameworkInfo targetFW, CILMetaDataLoaderWithCallbacks loader )
      {
         return this.GetOrAdd_TargetFWAssemblies( this._targetFWAssemblies, targetFW, tfw => loader.LoaderCallbacks.GetAssemblyResourcesForFramework( tfw ).ToArray() );
      }

      protected abstract String[] GetOrAdd_TargetFWAssemblies( TTargetFWAssembliesDic dic, TargetFrameworkInfo key, Func<TargetFrameworkInfo, String[]> factory );
      protected abstract ISet<String> GetOrAdd_MDTypes( TTypesDic dic, CILMetaData key, Func<CILMetaData, ISet<String>> factory );
      protected abstract TResolvedTargetFWAssembliesDicInner GetOrAdd_ResolvedTargetFWAssemblies( TResolvedTargetFWAssembliesDic dic, CILMetaData key, Func<CILMetaData, TResolvedTargetFWAssembliesDicInner> factory );
      protected abstract CILMetaData GetOrAdd_ResolvedTargetFWAssembliesInner( TResolvedTargetFWAssembliesDicInner dic, String key, Func<String, CILMetaData> factory );
      protected abstract TAssemblyReferenceDicInner GetOrAdd_AssemblyReferences( TAssemblyReferenceDic dic, CILMetaData key, Func<CILMetaData, TAssemblyReferenceDicInner> factory );
      protected abstract CILMetaData GetOrAdd_AssemblyReferencesInner( TAssemblyReferenceDicInner dic, AssemblyInformationForResolving key, Func<AssemblyInformationForResolving, CILMetaData> factory );
      protected abstract void RecordNotManagedAssembly( String resource );
      protected abstract Boolean IsRecordedNotManagedAssembly( String resource );

   }


   public class TargetFrameworkMapperNotThreadSafe : AbstractTargetFrameworkMapper<
      Dictionary<CILMetaData, ISet<String>>,
      Dictionary<TargetFrameworkInfo, String[]>,
      Dictionary<CILMetaData, Dictionary<String, CILMetaData>>,
      Dictionary<String, CILMetaData>,
      Dictionary<CILMetaData, Dictionary<AssemblyInformationForResolving, CILMetaData>>,
      Dictionary<AssemblyInformationForResolving, CILMetaData>
      >
   {
      private readonly HashSet<String> _notManagedAssemblies;

      public TargetFrameworkMapperNotThreadSafe()
         : base(
         new Dictionary<CILMetaData, ISet<String>>(),
         new Dictionary<TargetFrameworkInfo, String[]>(),
         new Dictionary<CILMetaData, Dictionary<String, CILMetaData>>(),
         new Dictionary<CILMetaData, Dictionary<AssemblyInformationForResolving, CILMetaData>>(),
         md => new Dictionary<String, CILMetaData>(),
         md => new Dictionary<AssemblyInformationForResolving, CILMetaData>()
         )
      {
         this._notManagedAssemblies = new HashSet<String>();
      }

      protected override String[] GetOrAdd_TargetFWAssemblies( Dictionary<TargetFrameworkInfo, String[]> dic, TargetFrameworkInfo key, Func<TargetFrameworkInfo, String[]> factory )
      {
         return dic.GetOrAdd_NotThreadSafe( key, factory );
      }

      protected override ISet<String> GetOrAdd_MDTypes( Dictionary<CILMetaData, ISet<String>> dic, CILMetaData key, Func<CILMetaData, ISet<String>> factory )
      {
         return dic.GetOrAdd_NotThreadSafe( key, factory );
      }

      protected override Dictionary<String, CILMetaData> GetOrAdd_ResolvedTargetFWAssemblies( Dictionary<CILMetaData, Dictionary<String, CILMetaData>> dic, CILMetaData key, Func<CILMetaData, Dictionary<String, CILMetaData>> factory )
      {
         return dic.GetOrAdd_NotThreadSafe( key, factory );
      }

      protected override CILMetaData GetOrAdd_ResolvedTargetFWAssembliesInner( Dictionary<String, CILMetaData> dic, String key, Func<String, CILMetaData> factory )
      {
         return dic.GetOrAdd_NotThreadSafe( key, factory );
      }

      protected override Dictionary<AssemblyInformationForResolving, CILMetaData> GetOrAdd_AssemblyReferences( Dictionary<CILMetaData, Dictionary<AssemblyInformationForResolving, CILMetaData>> dic, CILMetaData key, Func<CILMetaData, Dictionary<AssemblyInformationForResolving, CILMetaData>> factory )
      {
         return dic.GetOrAdd_NotThreadSafe( key, factory );
      }

      protected override CILMetaData GetOrAdd_AssemblyReferencesInner( Dictionary<AssemblyInformationForResolving, CILMetaData> dic, AssemblyInformationForResolving key, Func<AssemblyInformationForResolving, CILMetaData> factory )
      {
         return dic.GetOrAdd_NotThreadSafe( key, factory );
      }

      protected override void RecordNotManagedAssembly( String resource )
      {
         this._notManagedAssemblies.Add( resource );
      }

      protected override bool IsRecordedNotManagedAssembly( String resource )
      {
         return this._notManagedAssemblies.Contains( resource );
      }
   }
}

public static partial class E_CILPhysical
{
   public static void ChangeTargetFramework(
      this TargetFrameworkMapper mapper,
      CILMetaData md,
      CILMetaDataLoaderWithCallbacks loader,
      TargetFrameworkInfoWithRetargetabilityInformation newTargetFW
      )
   {
      var cb = loader.LoaderCallbacks;
      var newTargetFWPath = cb.GetTargetFrameworkPathForFrameworkInfo( newTargetFW.TargetFrameworkInfo );
      var aRefsTable = md.AssemblyReferences;
      var aRefs = aRefsTable.TableContents;

      var aRefPaths = new Dictionary<AssemblyReference, String>( ReferenceEqualityComparer<AssemblyReference>.ReferenceBasedComparer );
      var aRefDic = Enumerable.Range( 0, aRefs.Count )
         .ToDictionary_Overwrite( aRefIdx => aRefs[aRefIdx], aRefIdx => aRefIdx, aRefsTable.TableInformation.EqualityComparer );

      // First, type refs
      foreach ( var tRef in md.TypeReferences.TableContents.Where( tr => tr.ResolutionScope.HasValue && tr.ResolutionScope.Value.Table == Tables.AssemblyRef ) )
      {
         var aRefIdx = tRef.ResolutionScope.Value;
         var aRef = aRefs[aRefIdx.Index];

         AssemblyReference newRef;
         Boolean wasTargetFW;
         if ( mapper.TryReMapReference( md, aRef.NewInformationForResolving(), Miscellaneous.CombineNamespaceAndType( tRef.Namespace, tRef.Name ), loader, newTargetFW, out newRef, out wasTargetFW ) )
         {
            Int32 aRefNewIdx;
            if ( !aRefDic.TryGetValue( newRef, out aRefNewIdx ) )
            {
               aRefNewIdx = aRefs.Count;
               aRefs.Add( newRef );
            }

            tRef.ResolutionScope = aRefIdx.ChangeIndex( aRefNewIdx );
         }

         if ( wasTargetFW && newTargetFW.AreFrameworkAssemblyReferencesRetargetable )
         {
            ( newRef ?? aRef ).Attributes |= AssemblyFlags.Retargetable;
         }
      }

      // Then, all type strings (sec blobs, custom attrs, marshal infos)
      foreach ( var marshal in md.FieldMarshals.TableContents )
      {
         mapper.ProcessMarshalInfo( md, loader, newTargetFW, marshal.NativeType );
      }

      foreach ( var sec in md.SecurityDefinitions.TableContents )
      {
         foreach ( var permSet in sec.PermissionSets.OfType<SecurityInformation>() )
         {
            var typeStr = permSet.SecurityAttributeType;
            if ( mapper.ProcessTypeString( md, loader, newTargetFW, ref typeStr ) )
            {
               permSet.SecurityAttributeType = typeStr;
            }
            foreach ( var namedArg in permSet.NamedArguments )
            {
               mapper.ProcessCASignatureNamed( md, loader, newTargetFW, namedArg );
            }
         }
      }

      foreach ( var ca in md.CustomAttributeDefinitions.TableContents )
      {
         mapper.ProcessCASignature( md, loader, newTargetFW, ca.Signature );
      }
   }

   private static void ProcessMarshalInfo(
      this TargetFrameworkMapper mapper,
      CILMetaData md,
      CILMetaDataLoaderWithCallbacks loader,
      TargetFrameworkInfoWithRetargetabilityInformation newTargetFW,
      AbstractMarshalingInfo marshal
      )
   {
      String typeStr;
      switch ( marshal?.MarshalingInfoKind )
      {
         case MarshalingInfoKind.SafeArray:
            var safeArray = (SafeArrayMarshalingInfo) marshal;
            typeStr = safeArray.UserDefinedType;
            if ( mapper.ProcessTypeString( md, loader, newTargetFW, ref typeStr ) )
            {
               safeArray.UserDefinedType = typeStr;
            }
            break;
         case MarshalingInfoKind.Custom:
            var custom = (CustomMarshalingInfo) marshal;
            typeStr = custom.CustomMarshalerTypeName;
            if ( mapper.ProcessTypeString( md, loader, newTargetFW, ref typeStr ) )
            {
               custom.CustomMarshalerTypeName = typeStr;
            }
            break;
      }

   }

   private static void ProcessCASignature(
      this TargetFrameworkMapper mapper,
      CILMetaData md,
      CILMetaDataLoaderWithCallbacks loader,
      TargetFrameworkInfoWithRetargetabilityInformation newTargetFW,
      AbstractCustomAttributeSignature sig
      )
   {
      if ( sig != null && sig.CustomAttributeSignatureKind == CustomAttributeSignatureKind.Resolved )
      {
         var sigg = (ResolvedCustomAttributeSignature) sig;
         foreach ( var typed in sigg.TypedArguments )
         {
            mapper.ProcessCASignatureTyped( md, loader, newTargetFW, typed );
         }

         foreach ( var named in sigg.NamedArguments )
         {
            mapper.ProcessCASignatureNamed( md, loader, newTargetFW, named );
         }
      }
   }

   private static void ProcessCASignatureTyped(
      this TargetFrameworkMapper mapper,
      CILMetaData md,
      CILMetaDataLoaderWithCallbacks loader,
      TargetFrameworkInfoWithRetargetabilityInformation newTargetFW,
      CustomAttributeTypedArgument arg
      )
   {
      if ( arg != null )
      {
         var value = arg.Value;
         if ( mapper.ProcessCASignatureTypedValue( md, loader, newTargetFW, ref value ) )
         {
            arg.Value = value;
         }
      }
   }

   private static Boolean ProcessCASignatureTypedValue(
      this TargetFrameworkMapper mapper,
      CILMetaData md,
      CILMetaDataLoaderWithCallbacks loader,
      TargetFrameworkInfoWithRetargetabilityInformation newTargetFW,
      ref Object value
      )
   {
      var retVal = false;
      if ( value != null )
      {
         var complex = value as CustomAttributeTypedArgumentValueComplex;
         if ( complex != null )
         {
            String typeString;
            switch ( complex.CustomAttributeTypedArgumentValueKind )
            {
               case CustomAttributeTypedArgumentValueKind.Type:
                  typeString = ( (CustomAttributeValue_TypeReference) complex ).TypeString;
                  break;
               case CustomAttributeTypedArgumentValueKind.Enum:
                  typeString = ( (CustomAttributeValue_EnumReference) complex ).EnumType;
                  break;
               case CustomAttributeTypedArgumentValueKind.Array:
                  var arrayValue = (CustomAttributeValue_Array) complex;
                  var elType = arrayValue.ArrayElementType;
                  typeString = elType != null && elType.ArgumentTypeKind == CustomAttributeArgumentTypeKind.Enum ?
                     ( (CustomAttributeArgumentTypeEnum) elType ).TypeString :
                     null;
                  var array = arrayValue.Array;
                  if ( array != null )
                  {
                     for ( var i = 0; i < array.Length; ++i )
                     {
                        var cur = array.GetValue( i );
                        if ( mapper.ProcessCASignatureTypedValue( md, loader, newTargetFW, ref cur ) )
                        {
                           array.SetValue( cur, i );
                        }
                     }
                  }
                  break;
               default:
                  typeString = null;
                  break;
            }

            retVal = typeString != null && mapper.ProcessTypeString( md, loader, newTargetFW, ref typeString );
            if ( retVal )
            {
               switch ( complex.CustomAttributeTypedArgumentValueKind )
               {
                  case CustomAttributeTypedArgumentValueKind.Type:
                     value = new CustomAttributeValue_TypeReference( typeString );
                     break;
                  case CustomAttributeTypedArgumentValueKind.Enum:
                     value = new CustomAttributeValue_EnumReference( typeString, ( (CustomAttributeValue_EnumReference) complex ).EnumValue );
                     break;
                  case CustomAttributeTypedArgumentValueKind.Array:
                     value = new CustomAttributeValue_Array( ( (CustomAttributeValue_Array) complex ).Array, new CustomAttributeArgumentTypeEnum() { TypeString = typeString } );
                     break;
               }
            }
         }
      }

      return retVal;
   }

   private static void ProcessCASignatureNamed(
      this TargetFrameworkMapper mapper,
      CILMetaData md,
      CILMetaDataLoaderWithCallbacks loader,
      TargetFrameworkInfoWithRetargetabilityInformation newTargetFW,
      CustomAttributeNamedArgument arg
      )
   {
      if ( arg != null )
      {
         var type = arg.FieldOrPropertyType;
         if ( type != null && type.ArgumentTypeKind == CustomAttributeArgumentTypeKind.Enum )
         {
            var typeStrArg = (CustomAttributeArgumentTypeEnum) type;
            var typeString = typeStrArg.TypeString;
            if ( mapper.ProcessTypeString( md, loader, newTargetFW, ref typeString ) )
            {
               typeStrArg.TypeString = typeString;
            }
         }
         mapper.ProcessCASignatureTyped( md, loader, newTargetFW, arg.Value );
      }
   }
}
