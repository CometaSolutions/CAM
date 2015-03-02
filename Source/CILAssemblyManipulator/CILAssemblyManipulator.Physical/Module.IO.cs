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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
   /// <summary>
   /// This class holds the required information when emitting strong-name assemblies. See ECMA specification for more information about strong named assemblies.
   /// </summary>
   public sealed class StrongNameKeyPair
   {
      private readonly Byte[] _keyPairArray;
      private readonly String _containerName;

      /// <summary>
      /// Creates a <see cref="StrongNameKeyPair"/> based on byte contents of the key pair.
      /// </summary>
      /// <param name="keyPairArray">The byte contents of the key pair.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="keyPairArray"/> is <c>null</c>.</exception>
      public StrongNameKeyPair( Byte[] keyPairArray )
         : this( keyPairArray, null, "Key pair array" )
      {
      }

      /// <summary>
      /// Creates a new <see cref="StrongNameKeyPair"/> based on the name of the container holding key pair.
      /// </summary>
      /// <param name="containerName">The name of the container to be used when creating RSA algorithm.</param>
      /// <remarks>Use this constructor only when you want to use the public key not easily accessible via byte array (eg when using machine key store).</remarks>
      /// <exception cref="ArgumentNullException">If <paramref name="containerName"/> is <c>null</c>.</exception>
      public StrongNameKeyPair( String containerName )
         : this( null, containerName, "Container name" )
      {
      }

      private StrongNameKeyPair( Byte[] keyPairArray, String containerName, String argumentName )
      {
         if ( keyPairArray == null && containerName == null )
         {
            throw new ArgumentNullException( argumentName + " was null." );
         }
         this._keyPairArray = keyPairArray;
         this._containerName = containerName;
      }

      /// <summary>
      /// Gets the byte contents of the key pair. Will be <c>null</c> if this <see cref="StrongNameKeyPair"/> was created based on the name of the container holding key pair.
      /// </summary>
      /// <value>The byte contents of the key pair.</value>
      public IEnumerable<Byte> KeyPair
      {
         get
         {
            return this._keyPairArray == null ? null : this._keyPairArray.Skip( 0 );
         }
      }

      /// <summary>
      /// Gets the name of the container holding the key pair. Will be <c>null</c> if this <see cref="StrongNameKeyPair"/> was created based on the byte contents of the key pair.
      /// </summary>
      /// <value>The name of the container holding the key pair.</value>
      public String ContainerName
      {
         get
         {
            return this._containerName;
         }
      }
   }

   /// <summary>
   /// Class containing information specific to writing the module.
   /// </summary>
   public sealed class EmittingArguments
   {
      /// <summary>
      /// During emitting, if the module is main module and should be strong-name signed, this <see cref="StrongNameKeyPair"/> will be used.
      /// Set <c>null</c> if the module should not be strong-name signed.
      /// This property is not used during loading.
      /// </summary>
      /// <value>The strong name of the module being emitted.</value>
      public StrongNameKeyPair StrongName { get; set; }

      /// <summary>
      /// During emitting, if the module is main module and should be strong-name signed, this property may be used to override the algorithm specified by key BLOB of <see cref="StrongName"/>.
      /// If this property does not have a value, the algorithm specified by key BLOB of <see cref="StrongName"/> will be used.
      /// If the key BLOB of <see cref="StrongName"/> does not specify an algorithm, the assembly will be signed using <see cref="AssemblyHashAlgorithm.SHA1"/>.
      /// This property is not used during loading.
      /// </summary>
      /// <value>The algorithm to compute a hash over emitted assembly data.</value>
      /// <remarks>
      /// If <see name="AssemblyHashAlgorithm.MD5"/> or <see cref="AssemblyHashAlgorithm.None"/> is specified, the <see cref="AssemblyHashAlgorithm.SHA1"/> will be used instead.
      /// </remarks>
      public AssemblyHashAlgorithm? SigningAlgorithm { get; set; }

      /// <summary>
      /// During emitting, if the module is main module and should be strong-name signed, setting this to <c>true</c> will only leave room for the hash, without actually computing it.
      /// This property is not used during loading.
      /// </summary>
      /// <value>Whether to delay signing procedure.</value>
      public Boolean DelaySign { get; set; }
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

   public static class CILModuleIO
   {

      public static ModuleReadResult ReadModule( Stream stream )
      {
         HeadersData headers;
         var md = CILAssemblyManipulator.Physical.Implementation.ModuleReader.ReadFromStream( stream, out headers );
         return new ModuleReadResult( md, headers );
      }
   }
}
