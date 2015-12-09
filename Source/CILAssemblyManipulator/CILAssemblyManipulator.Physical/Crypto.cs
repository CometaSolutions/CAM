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
using CILAssemblyManipulator.Physical;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
   public interface CryptoCallbacks
   {
      Byte[] ExtractPublicKeyFromCSPContainer( String containterName );
      HashStreamInfo CreateHashStream( AssemblyHashAlgorithm algorithm );
      IDisposable CreateRSAFromCSPContainer( String containerName );
      IDisposable CreateRSAFromParameters( RSAParameters parameters );
      Byte[] CreateRSASignature( IDisposable rsa, String hashAlgorithmName, Byte[] contentsHash );
   }

   /// <summary>
   /// The event argument class used by <see cref="CILReflectionContext.HashStreamLoadEvent"/> event.
   /// </summary>
   public struct HashStreamInfo
   {
      private readonly AssemblyHashAlgorithm _algorithm;
      private readonly Func<Stream> _cryptoStream;
      private readonly Func<Byte[]> _hashGetter;
      private readonly Func<Byte[], Byte[]> _hashComputer;
      private readonly IDisposable _transform;

      public HashStreamInfo( AssemblyHashAlgorithm algorithm, Func<Stream> cryptoStream, Func<Byte[]> hashGetter, Func<Byte[], Byte[]> hashComputer, IDisposable transform )
      {
         this._algorithm = algorithm;
         this._cryptoStream = cryptoStream;
         this._hashGetter = hashGetter;
         this._hashComputer = hashComputer;
         this._transform = transform;
      }

      public AssemblyHashAlgorithm Algorithm
      {
         get
         {
            return this._algorithm;
         }
      }

      public Func<Stream> CryptoStream
      {
         get
         {
            return this._cryptoStream;
         }
      }

      public Func<Byte[]> HashGetter
      {
         get
         {
            return this._hashGetter;
         }
      }

      public Func<Byte[], Byte[]> ComputeHash
      {
         get
         {
            return this._hashComputer;
         }
      }

      public IDisposable Transform
      {
         get
         {
            return this._transform;
         }
      }
   }

   /// <summary>
   /// This is identical to System.Security.Cryptography.RSAParameters struct.
   /// </summary>
   public struct RSAParameters
   {

      /// <summary>
      /// Represents the <c>D</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] D;

      /// <summary>
      /// Represents the <c>DP</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] DP;

      /// <summary>
      /// Represents the <c>DQ</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] DQ;

      /// <summary>
      /// Represents the <c>Exponent</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] Exponent;

      /// <summary>
      /// Represents the <c>InverseQ</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] InverseQ;

      /// <summary>
      /// Represents the <c>Modulus</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] Modulus;

      /// <summary>
      /// Represents the <c>P</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] P;

      /// <summary>
      /// Represents the <c>Q</c> parameter for the RSA algorithm.
      /// </summary>
      public Byte[] Q;
   }
}

public static partial class E_CILPhysical
{
   public static HashStreamInfo CreateHashStreamAndCheck(
      this CryptoCallbacks callbacks,
      AssemblyHashAlgorithm algorithm,
      Boolean checkCryptoStream,
      Boolean checkHashGetter,
      Boolean checkComputeHash,
      Boolean checkTransform
      )
   {
      var retVal = callbacks.CreateHashStream( algorithm );
      if ( checkCryptoStream )
      {
         ArgumentValidator.ValidateNotNull( "Crypto stream", retVal.CryptoStream );
      }
      if ( checkHashGetter )
      {
         ArgumentValidator.ValidateNotNull( "Hash getter", retVal.HashGetter );
      }
      if ( checkComputeHash )
      {
         ArgumentValidator.ValidateNotNull( "Hash computer", retVal.ComputeHash );
      }
      if ( checkTransform )
      {
         ArgumentValidator.ValidateNotNull( "Transform", retVal.Transform );
      }
      return retVal;
   }

   public static Byte[] ExtractPublicKeyFromCSPContainerAndCheck( this CryptoCallbacks callbacks, String containerName )
   {
      var retVal = callbacks.ExtractPublicKeyFromCSPContainer( containerName );
      if ( retVal.IsNullOrEmpty() )
      {
         throw new InvalidOperationException( "The public key of CSP \"" + containerName + "\" could not be resolved." );
      }
      return retVal;
   }

   public static Byte[] CreateRSASignatureAndCheck( this CryptoCallbacks callbacks, IDisposable rsa, String algorithmName, Byte[] contentsHash )
   {
      var retVal = callbacks.CreateRSASignature( rsa, algorithmName, contentsHash );
      ArgumentValidator.ValidateNotNull( "RSA signature", retVal );
      return retVal;
   }

   public static Byte[] ComputePublicKeyToken( this HashStreamInfo streamInfo, Byte[] fullPublicKey )
   {
      Byte[] retVal;
      if ( fullPublicKey.IsNullOrEmpty() )
      {
         retVal = fullPublicKey;
      }
      else
      {
         if ( streamInfo.Algorithm != AssemblyHashAlgorithm.SHA1 )
         {
            throw new InvalidOperationException( "Hash algorithm must be " + AssemblyHashAlgorithm.SHA1 + "." );
         }
         ArgumentValidator.ValidateNotNull( "Transform", streamInfo.Transform );
         ArgumentValidator.ValidateNotNull( "Hash computer", streamInfo.ComputeHash );

         retVal = streamInfo.ComputeHash( fullPublicKey );
         retVal = retVal.Skip( retVal.Length - 8 ).Reverse().ToArray();
      }
      return retVal;
   }

   public static Byte[] CreatePublicKeyFromStrongName( this CryptoCallbacks eArgs, StrongNameKeyPair strongName, AssemblyHashAlgorithm? algorithmOverride = null )
   {
      Byte[] retVal;
      if ( strongName == null )
      {
         retVal = null;
      }
      else
      {
         var container = strongName.ContainerName;
         if ( container == null )
         {
            String errorString;
            AssemblyHashAlgorithm signingAlgorithm;
            RSAParameters rParams;
            CILAssemblyManipulator.Physical.Implementation.CryptoUtils.TryCreateSigningInformationFromKeyBLOB(
               strongName.KeyPair.ToArray(),
               algorithmOverride,
               out retVal,
               out signingAlgorithm,
               out rParams,
               out errorString
               );
         }
         else
         {
            retVal = eArgs.ExtractPublicKeyFromCSPContainer( container );
         }
      }

      return retVal;
   }

   public static Boolean IsMatch( this AssemblyDefinition aDef, AssemblyReference aRef, HashStreamInfo? hashStreamInfo )
   {
      return aDef.IsMatch( new AssemblyInformationForResolving( aRef ), aRef.Attributes.IsRetargetable(), hashStreamInfo );
   }

   public static Boolean IsMatch( this AssemblyDefinition aDef, AssemblyInformationForResolving? aRef, Boolean isRetargetable, HashStreamInfo? hashStreamInfo )
   {
      var retVal = aDef != null
         && aRef != null;
      if ( retVal )
      {
         var aReff = aRef.Value;
         var defInfo = aDef.AssemblyInformation;
         var refInfo = aReff.AssemblyInformation;
         if ( isRetargetable )
         {
            retVal = String.Equals( defInfo.Name, refInfo.Name );
         }
         else
         {
            var defPK = defInfo.PublicKeyOrToken;
            var refPK = refInfo.PublicKeyOrToken;
            retVal = defPK.IsNullOrEmpty() == refPK.IsNullOrEmpty()
               && defInfo.Equals( refInfo, aReff.IsFullPublicKey )
               && ( aReff.IsFullPublicKey || ( hashStreamInfo.HasValue && ArrayEqualityComparer<Byte>.ArrayEquality( hashStreamInfo.Value.ComputePublicKeyToken( defPK ), refPK ) ) );
         }
      }

      return retVal;
   }
}