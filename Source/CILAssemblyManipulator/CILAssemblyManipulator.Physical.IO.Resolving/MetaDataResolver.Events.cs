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

using CILAssemblyManipulator.Physical;
using CILAssemblyManipulator.Physical.Resolving;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TabularMetaData;

namespace CILAssemblyManipulator.Physical.Resolving
{
   /// <summary>
   /// This is common base class for <see cref="ModuleReferenceResolveEventArgs"/> and <see cref="AssemblyReferenceResolveEventArgs"/>.
   /// It encapsulates some common properties when resolving module or assembly references with <see cref="MetaDataResolver.ModuleReferenceResolveEvent"/> and <see cref="MetaDataResolver.AssemblyReferenceResolveEvent"/> events, respectively.
   /// </summary>
   public abstract class AssemblyOrModuleReferenceResolveEventArgs : EventArgs
   {

      internal AssemblyOrModuleReferenceResolveEventArgs( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData thisMD )
      {
         ArgumentValidator.ValidateNotNull( "This metadata", thisMD );

         this.ThisMetaData = thisMD;
      }

      /// <summary>
      /// Gets the metadata which contained the module or assembly reference.
      /// </summary>
      /// <value>The metadata which contained the module or assembly reference.</value>
      public CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData ThisMetaData { get; }

      /// <summary>
      /// The event handlers of <see cref="MetaDataResolver.ModuleReferenceResolveEvent"/> and <see cref="MetaDataResolver.AssemblyReferenceResolveEvent"/> events should set this property to the <see cref="CILMetaData"/> corresponding to the module or assembly reference being resolved.
      /// </summary>
      public CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData ResolvedMetaData { get; set; }
   }

   /// <summary>
   /// This is arguments class for <see cref="MetaDataResolver.ModuleReferenceResolveEvent"/> event.
   /// </summary>
   public sealed class ModuleReferenceResolveEventArgs : AssemblyOrModuleReferenceResolveEventArgs
   {

      internal ModuleReferenceResolveEventArgs( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData thisMD, String moduleName )
         : base( thisMD )
      {
         this.ModuleName = moduleName;
      }

      /// <summary>
      /// Gets the name of the module being resolved.
      /// </summary>
      /// <value>The name of the module being resolved.</value>
      public String ModuleName { get; }
   }

   /// <summary>
   /// This is arguments class for <see cref="MetaDataResolver.AssemblyReferenceResolveEvent"/> event.
   /// </summary>
   public sealed class AssemblyReferenceResolveEventArgs : AssemblyOrModuleReferenceResolveEventArgs
   {

      internal AssemblyReferenceResolveEventArgs( CAMPhysical::CILAssemblyManipulator.Physical.CILMetaData thisMD, String assemblyName, AssemblyInformationForResolving assemblyInfo ) //, Boolean isRetargetable )
         : base( thisMD )
      {

         this.UnparsedAssemblyName = assemblyName;
         this.AssemblyInformation = assemblyInfo;
      }

      /// <summary>
      /// Gets the unparsed assembly name in case parsing failed.
      /// </summary>
      /// <value>The unparsed assembly name in case parsing failed.</value>
      /// <remarks>
      /// This will be <c>null</c> when the assembly reference was not in textual format, or when the assembly name was successfully parsed with <see cref="AssemblyInformation.TryParse(string, out AssemblyInformation, out bool)"/> method.
      /// In such case, the <see cref="AssemblyInformation"/> will be non-<c>null</c>.
      /// </remarks>
      public String UnparsedAssemblyName { get; }

      /// <summary>
      /// Gets the <see cref="AssemblyInformationForResolving"/> in case of assembly references via <see cref="AssemblyReference"/> or successfully parsed assembly string.
      /// </summary>
      /// <value>The <see cref="AssemblyInformationForResolving"/> in case of assembly references via <see cref="AssemblyReference"/> or successfully parsed assembly string.</value>
      /// <remarks>
      /// This will be <c>null</c> if assembly reference was in textual format and parsing the assembly name failed.
      /// In such case, the <see cref="UnparsedAssemblyName"/> will be set to the textual assembly name.
      /// </remarks>
      public AssemblyInformationForResolving AssemblyInformation { get; }


   }

   /// <summary>
   /// This class encapsulates information which is required when resolving assembly references with <see cref="MetaDataResolver"/>, and more specifically, its <see cref="MetaDataResolver.AssemblyReferenceResolveEvent"/> event.
   /// </summary>
   public sealed class AssemblyInformationForResolving : IEquatable<AssemblyInformationForResolving>
   {

      /// <summary>
      /// Creates a new instance of <see cref="AssemblyInformationForResolving"/> with required information gathered from given <see cref="AssemblyReference"/> row.
      /// </summary>
      /// <param name="aRef">The <see cref="AssemblyReference"/> row.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="aRef"/> is <c>null</c>.</exception>
      public AssemblyInformationForResolving( AssemblyReference aRef )
         : this( ArgumentValidator.ValidateNotNullAndReturn( "Assembly reference", aRef ).AssemblyInformation.CreateDeepCopy(), aRef.Attributes.IsFullPublicKey() )
      {

      }

      /// <summary>
      /// Creates a new instance of <see cref="AssemblyInformationForResolving"/> with required information specified in a <see cref="CAMPhysical::CILAssemblyManipulator.Physical.AssemblyInformation"/> object, and separate boolean indicating whether the reference uses full public key, or public key token.
      /// </summary>
      /// <param name="information">The <see cref="CAMPhysical::CILAssemblyManipulator.Physical.AssemblyInformation"/> with required information.</param>
      /// <param name="isFullPublicKey"><c>true</c> if this assembly reference uses full public key; <c>false</c> if it uses public key token.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="information"/> is <c>null</c>.</exception>
      public AssemblyInformationForResolving( AssemblyInformation information, Boolean isFullPublicKey )
      {
         ArgumentValidator.ValidateNotNull( "Assembly information", information );

         this.AssemblyInformation = information;
         this.IsFullPublicKey = isFullPublicKey;
      }

      /// <summary>
      /// Gets the <see cref="CAMPhysical::CILAssemblyManipulator.Physical.AssemblyInformation"/> containing the name, culture, version, and public key information of this assembly reference.
      /// </summary>
      /// <value>The <see cref="CAMPhysical::CILAssemblyManipulator.Physical.AssemblyInformation"/> containing the name, culture, version, and public key information of this assembly reference.</value>
      public AssemblyInformation AssemblyInformation { get; }

      /// <summary>
      /// Gets the value indicating whether the possible public key in <see cref="AssemblyInformation"/> is full public key, or public key token.
      /// </summary>
      /// <value>The value indicating whether the possible public key in <see cref="AssemblyInformation"/> is full public key, or public key token.</value>
      public Boolean IsFullPublicKey { get; }

      /// <summary>
      /// Checks whether given object is of type <see cref="AssemblyInformationForResolving"/> and that its data content is same as data content of this.
      /// </summary>
      /// <param name="obj">The object to check.</param>
      /// <returns><c>true</c> if <paramref name="obj"/> is of type <see cref="AssemblyInformationForResolving"/> and its data content match to data content of this.</returns>
      public override Boolean Equals( Object obj )
      {
         return this.Equals( obj as AssemblyInformationForResolving );
      }

      /// <summary>
      /// Computes the hash code for this <see cref="AssemblyInformationForResolving"/>.
      /// </summary>
      /// <returns>The hash code for this <see cref="AssemblyInformationForResolving"/>.</returns>
      public override Int32 GetHashCode()
      {
         return this.AssemblyInformation.Name.GetHashCodeSafe();
      }

      /// <summary>
      /// Checks whether this <see cref="AssemblyInformationForResolving"/> and given <see cref="AssemblyInformationForResolving"/> equal or have same data contents.
      /// </summary>
      /// <param name="other">Other <see cref="AssemblyInformationForResolving"/>.</param>
      /// <returns><c>true</c> if <paramref name="other"/> is this or if its data content matches to this; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// The following properites are checked when matching data content:
      /// <list type="bullet">
      /// <item><description><see cref="AssemblyInformation"/> (using <see cref="Comparers.AssemblyInformationEqualityComparer"/>), and</description></item>
      /// <item><description><see cref="IsFullPublicKey"/>.</description></item>
      /// </list>
      /// </remarks>
      public Boolean Equals( AssemblyInformationForResolving other )
      {
         return ReferenceEquals( this, other ) ||
            ( other != null
            && this.IsFullPublicKey == other.IsFullPublicKey
            && Comparers.AssemblyInformationEqualityComparer.Equals( this.AssemblyInformation, other.AssemblyInformation )
            );
      }
   }


#if CAM_PHYSICAL_IS_PORTABLE
   internal static class CollectionExtensions
   {
      // For some reason, this method is missing from PCL
      internal static Int32 FindIndex<T>( this IList<T> list, Predicate<T> match )
      {
         var max = list.Count;
         for ( var i = 0; i < max; ++i )
         {
            if ( match( list[i] ) )
            {
               return i;
            }
         }
         return -1;
      }

   }
#endif

}

#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{
   /// <summary>
   /// Checks whether this <see cref="AssemblyDefinition"/> matches the given <see cref="AssemblyReference"/>, 
   /// </summary>
   /// <param name="aDef">The <see cref="AssemblyDefinition"/>.</param>
   /// <param name="aRef">The optional <see cref="AssemblyReference"/>.</param>
   /// <param name="publicKeyTokenComputer">The callback to use, if public key token computation is required.</param>
   /// <returns><c>true</c> if <paramref name="aRef"/> is not <c>null</c> and matches this <see cref="AssemblyDefinition"/>, taking into account that <paramref name="aRef"/> might have public key token instead of full public key; <c>false</c> otherwise.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="AssemblyDefinition"/> is <c>null</c>.</exception>
   /// <seealso cref="T:CILAssemblyManipulator.Physical.Crypto.HashStreamInfo.HashComputer"/>
   public static Boolean IsMatch( this AssemblyDefinition aDef, AssemblyReference aRef, Func<Byte[], Byte[]> publicKeyTokenComputer )
   {
      return aDef.IsMatch( aRef == null ? null : new AssemblyInformationForResolving( aRef ), aRef?.Attributes.IsRetargetable() ?? false, publicKeyTokenComputer );
   }

   /// <summary>
   /// Checks whether this <see cref="AssemblyDefinition"/> matches the given <see cref="AssemblyInformationForResolving"/>.
   /// </summary>
   /// <param name="aDef">The <see cref="AssemblyDefinition"/>.</param>
   /// <param name="aRef">The optional <see cref="AssemblyInformationForResolving"/>.</param>
   /// <param name="isRetargetable">Whether the <paramref name="aRef"/> is retargetable.</param>
   /// <param name="publicKeyTokenComputer">The callback to use, if public key token computation is required.</param>
   /// <returns><c>true</c> if <paramref name="aRef"/> is not <c>null</c> and matches this <see cref="AssemblyDefinition"/>, taking into account that <paramref name="aRef"/> might have public key token instead of full public key; <c>false</c> otherwise.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="AssemblyDefinition"/> is <c>null</c>.</exception>
   /// <seealso cref="T:CILAssemblyManipulator.Physical.Crypto.HashStreamInfo.HashComputer"/>
   public static Boolean IsMatch( this AssemblyDefinition aDef, AssemblyInformationForResolving aRef, Boolean isRetargetable, Func<Byte[], Byte[]> publicKeyTokenComputer )
   {
      var defInfo = aDef.AssemblyInformation;
      var retVal = aRef != null;
      if ( retVal )
      {
         var refInfo = aRef.AssemblyInformation;
         retVal = String.Equals( defInfo.Name, refInfo.Name );
         if ( retVal && !isRetargetable )
         {
            var defPK = defInfo.PublicKeyOrToken;
            var refPK = refInfo.PublicKeyOrToken;
            retVal = defPK.IsNullOrEmpty() == refPK.IsNullOrEmpty()
               && defInfo.Equals( refInfo, aRef.IsFullPublicKey )
               && ( aRef.IsFullPublicKey || ArrayEqualityComparer<Byte>.ArrayEquality( publicKeyTokenComputer?.Invoke( defPK ), refPK ) );
         }
      }

      return retVal;
   }
}