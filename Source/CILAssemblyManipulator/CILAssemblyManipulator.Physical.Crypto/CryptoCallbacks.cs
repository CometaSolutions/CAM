﻿/*
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
using CILAssemblyManipulator.Physical.Crypto;
using UtilPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UtilPack.Numerics;

namespace CILAssemblyManipulator.Physical.Crypto
{
   /// <summary>
   /// This interface encapsulates all the cryptographical methods needed by CAM.Physical.
   /// </summary>
   public interface CryptoCallbacks : IDisposable
   {
      /// <summary>
      /// Given a container name, extracts public key bytes from it.
      /// </summary>
      /// <param name="containerName">The container name.</param>
      /// <returns>The public key bytes for given container name.</returns>
      /// <exception cref="NotSupportedException">If container names are not supported on this platform.</exception>
      Byte[] ExtractPublicKeyFromCSPContainer( String containerName ); // TODO maybe something like KeyBLOBParsingResult TryParseCSPContainer(String containerName) ?

      /// <summary>
      /// Given an <see cref="AssemblyHashAlgorithm"/>, creates a new <see cref="BlockDigestAlgorithm"/> struct to be used for cryptographic purposes (e.g. computing public key token, or calculating storng name signature).
      /// </summary>
      /// <param name="algorithm">The <see cref="AssemblyHashAlgorithm"/> enumeration.</param>
      /// <returns>A <see cref="BlockDigestAlgorithm"/> for given <paramref name="algorithm"/>.</returns>
      /// <seealso cref="BlockDigestAlgorithm"/>
      BlockDigestAlgorithm CreateHashAlgorithm( AssemblyHashAlgorithm algorithm );

      /// <summary>
      /// Computes a signature from a hash bytes, using given algorithm.
      /// </summary>
      /// <param name="contentsHash">The binary contents of the content from which to create signature from.</param>
      /// <param name="parsingResult">The parsing result from <see cref="TryParseKeyBLOB"/>.</param>
      /// <param name="containerName">The container name. Use <c>null</c> or empty string to use <paramref name="parsingResult"/> for RSA.</param>
      /// <returns>The cryptographic signature created from <paramref name="contentsHash"/>.</returns>
      Byte[] CreateSignature( Byte[] contentsHash, KeyBLOBParsingResult parsingResult, String containerName );

      /// <summary>
      /// Enumerates public key token computed from given full public key.
      /// </summary>
      /// <param name="fullPublicKey">The full public key.</param>
      /// <returns>Enumerable of the public key token. Will be empty if <paramref name="fullPublicKey"/> is <c>null</c> or empty.</returns>
      IEnumerable<Byte> EnumeratePublicKeyToken( Byte[] fullPublicKey );

      /// <summary>
      /// Tries to parse public and/or private key information from given key BLOB.
      /// </summary>
      /// <param name="keyBLOB">The key BLOB.</param>
      /// <param name="hashAlgorithmOverride">The optional override to the hash algorithm specified in <paramref name="keyBLOB"/>.</param>
      /// <returns>Instance of <see cref="KeyBLOBParsingResult"/> if parsing was successul; <c>null</c> otherwise.</returns>
      KeyBLOBParsingResult TryParseKeyBLOB( Byte[] keyBLOB, AssemblyHashAlgorithm? hashAlgorithmOverride );
   }

   /// <summary>
   /// This class represents the result of parsing a cryptographic key(-pair) BLOB.
   /// </summary>
   public class KeyBLOBParsingResult
   {

      /// <summary>
      /// Creates a new instance of <see cref="KeyBLOBParsingResult"/> with given parameters.
      /// </summary>
      /// <param name="snSize">The size of the strong-name signature in the image, in bytes.</param>
      /// <param name="publicKey">The public key to use in emitting.</param>
      /// <param name="hashAlgorithm">The hash algorithm to use.</param>
      /// <param name="errorMessage">The error message, if key BLOB parsing failed.</param>
      public KeyBLOBParsingResult( Int32 snSize, Byte[] publicKey, AssemblyHashAlgorithm hashAlgorithm, String errorMessage )
      {
         this.StrongNameSize = snSize;
         this.PublicKey = publicKey;
         this.ErrorMessage = errorMessage;
         this.HashAlgorithm = hashAlgorithm;
      }

      /// <summary>
      /// Gets the size of the strong-name signature in the image, in bytes.
      /// </summary>
      /// <value>The size of the strong-name signature in the image, in bytes.</value>
      public Int32 StrongNameSize { get; }

      /// <summary>
      /// Gets the public key to use in emitting.
      /// </summary>
      /// <value>The public key to use in emitting.</value>
      public Byte[] PublicKey { get; }

      /// <summary>
      /// Gets the error message if key BLOB parsing failed.
      /// </summary>
      /// <value>The error message if key BLOB parsing failed.</value>
      public String ErrorMessage { get; }

      /// <summary>
      /// Gets the hash algorithm to use when computing hash.
      /// </summary>
      /// <value>The hash algorithm to use when computing hash.</value>
      public AssemblyHashAlgorithm HashAlgorithm { get; }
   }

}

// This class will get its documentation from CAM.Physical Core.
#pragma warning disable 1591
public static partial class E_CILPhysical
#pragma warning restore 1591
{

   /// <summary>
   /// Helper method to call <see cref="CryptoCallbacks.ExtractPublicKeyFromCSPContainer"/> method and check that return value is not <c>null</c> or empty.
   /// </summary>
   /// <param name="callbacks">This <see cref="CryptoCallbacks"/>.</param>
   /// <param name="containerName">The container name.</param>
   /// <returns>The non-<c>null</c> and non-empty public key for given container name.</returns>
   /// <exception cref="ArgumentException">If the return value of <see cref="CryptoCallbacks.ExtractPublicKeyFromCSPContainer"/> method is <c>null</c> or empty.</exception>
   /// <exception cref="NullReferenceException">If this <see cref="CryptoCallbacks"/> is <c>null</c>.</exception>
   public static Byte[] ExtractPublicKeyFromCSPContainerAndCheck( this CryptoCallbacks callbacks, String containerName )
   {
      var retVal = callbacks.ExtractPublicKeyFromCSPContainer( containerName );
      if ( retVal.IsNullOrEmpty() )
      {
         throw new ArgumentException( "The public key of CSP \"" + containerName + "\" could not be resolved." );
      }
      return retVal;
   }

   /// <summary>
   /// Helper method to check whether <see cref="KeyBLOBParsingResult"/> represents succeeded key BLOB parsing operation.
   /// </summary>
   /// <param name="result">The <see cref="KeyBLOBParsingResult"/>.</param>
   /// <returns><c>true</c> if the <see cref="KeyBLOBParsingResult"/> is not <c>null</c> and its <see cref="KeyBLOBParsingResult.ErrorMessage"/> is not <c>null</c>; <c>false</c> otherwise.</returns>
   public static Boolean ParsingSucceeded( this KeyBLOBParsingResult result )
   {
      return result != null && result.ErrorMessage == null;
   }

   /// <summary>
   /// This method will compute public key token for a given full public key.
   /// </summary>
   /// <param name="cryptoCallbacks">The <see cref="CryptoCallbacks"/>.</param>
   /// <param name="fullPublicKey">The full public key.</param>
   /// <returns>The value of <paramref name="fullPublicKey"/>, if <paramref name="fullPublicKey"/> is <c>null</c> or empty, or the public key token of the <paramref name="fullPublicKey"/>.</returns>
   public static Byte[] ComputePublicKeyToken( this CryptoCallbacks cryptoCallbacks, Byte[] fullPublicKey )
   {
      return cryptoCallbacks.EnumeratePublicKeyToken( fullPublicKey ).ToArray();
   }
}