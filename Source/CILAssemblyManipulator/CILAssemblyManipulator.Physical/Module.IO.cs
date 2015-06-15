using CILAssemblyManipulator.Physical;
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
using CommonUtils;
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

   public abstract class IOArguments
   {
      private readonly List<Int32> _methodRVAs;
      private readonly List<Int32> _fieldRVAs;
      private readonly List<Int32?> _embeddedManifestResourceOffsets;

      public IOArguments()
      {
         this._methodRVAs = new List<Int32>();
         this._fieldRVAs = new List<Int32>();
         this._embeddedManifestResourceOffsets = new List<Int32?>();
      }

      /// <summary>
      /// The values are interpreted as unsigned 4-byte integers.
      /// </summary>
      public List<Int32> MethodRVAs
      {
         get
         {
            return this._methodRVAs;
         }
      }

      /// <summary>
      /// The values are interpreted as unsigned 4-byte integers.
      /// </summary>
      public List<Int32> FieldRVAs
      {
         get
         {
            return this._fieldRVAs;
         }
      }

      /// <summary>
      /// If has value, then <see cref="ManifestResource"/> at this index is embedded in the module, and is located at given offset. Otherwise, the <see cref="ManifestResource"/> is located at another file.
      /// </summary>
      public List<Int32?> EmbeddedManifestResourceOffsets
      {
         get
         {
            return this._embeddedManifestResourceOffsets;
         }
      }

      public HeadersData Headers { get; set; }

      public Byte[] StrongNameHashValue { get; set; }
   }

   /// <summary>
   /// Class containing information specific to reading the module.
   /// </summary>
   public sealed class ReadingArguments : IOArguments
   {

   }

   /// <summary>
   /// Class containing information specific to writing the module.
   /// </summary>
   public sealed class EmittingArguments : IOArguments
   {

      ///// <summary>
      ///// This event occurs when a public key is being exported from a named cryptographic service provider.
      ///// </summary>
      //event EventHandler<CSPPublicKeyEventArgs> CSPPublicKeyEvent;

      ///// <summary>
      ///// This event occurs when a <see cref="CILMetaData"/> is emitted with a strong name. The event handler should set the <see cref="HashStreamLoadEventArgs.CryptoStream"/>, <see cref="HashStreamLoadEventArgs.HashGetter"/> and <see cref="HashStreamLoadEventArgs.Transform"/> properties. This assembly can not do this since many security and cryptographic functions are not present in this portable profile.
      ///// </summary>
      //event EventHandler<HashStreamLoadEventArgs> HashStreamLoadEvent;

      ///// <summary>
      ///// This event occurs when a <see cref="CILMetaData"/> is emitted with a strong name. The event handler should set the <see cref="RSACreationEventArgs.RSA"/> property. This assembly can not do this since many security and cryptographic functions are not present in this portable profile.
      ///// </summary>
      //event EventHandler<RSACreationEventArgs> RSACreationEvent;

      ///// <summary>
      ///// This event occurs when a <see cref="CILMetaData"/> is emitted with a strong name. The event handler should set the <see cref="RSASignatureCreationEventArgs.Signature"/> property. This assembly can not do this since many security and cryptographic functions are not present in this portable profile.
      ///// </summary>
      //event EventHandler<RSASignatureCreationEventArgs> RSASignatureCreationEvent;

      /// <summary>
      /// During emitting, if the module is main module and should be strong-name signed, this <see cref="StrongNameKeyPair"/> will be used.
      /// Set to <c>null</c> if the module should not be strong-name signed.
      /// </summary>
      /// <value>The strong name of the module being emitted.</value>
      public StrongNameKeyPair StrongName { get; set; }

      /// <summary>
      /// During emitting, if the module is main module and should be strong-name signed, this property may be used to override the algorithm specified by key BLOB of <see cref="StrongName"/>.
      /// If this property does not have a value, the algorithm specified by key BLOB of <see cref="StrongName"/> will be used.
      /// If the key BLOB of <see cref="StrongName"/> does not specify an algorithm, the assembly will be signed using <see cref="AssemblyHashAlgorithm.SHA1"/>.
      /// </summary>
      /// <value>The algorithm to compute a hash over emitted assembly data.</value>
      /// <remarks>
      /// If <see name="AssemblyHashAlgorithm.MD5"/> or <see cref="AssemblyHashAlgorithm.None"/> is specified, the <see cref="AssemblyHashAlgorithm.SHA1"/> will be used instead.
      /// </remarks>
      public AssemblyHashAlgorithm? SigningAlgorithm { get; set; }

      /// <summary>
      /// During emitting, if the module is main module and should be strong-name signed, setting this to <c>true</c> will only leave room for the hash, without actually computing it.
      /// </summary>
      /// <value>Whether to delay signing procedure.</value>
      public Boolean DelaySign { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="ModuleKind"/> of the module.
      /// </summary>
      /// <value>The <see cref="ModuleKind"/> of the module being emitted or loaded.</value>
      public ModuleKind ModuleKind { get; set; }

      public CryptoCallbacks CryptoCallbacks { get; set; }

   }

   ///// <summary>
   ///// This is event argument class for <see cref="CILReflectionContext.CSPPublicKeyEvent"/> event.
   ///// </summary>
   //public sealed class CSPPublicKeyEventArgs : EventArgs
   //{
   //   private readonly String _cspName;

   //   internal CSPPublicKeyEventArgs( String cspName )
   //   {
   //      ArgumentValidator.ValidateNotNull( "Cryptographic service provider name", cspName );

   //      this._cspName = cspName;
   //   }

   //   /// <summary>
   //   /// Gets the name of the cryptographic service provider.
   //   /// </summary>
   //   /// <value>The name of the cryptographic service provider.</value>
   //   public String CSPName
   //   {
   //      get
   //      {
   //         return this._cspName;
   //      }
   //   }

   //   /// <summary>
   //   /// Gets or sets the public key of the named cryptographic service provider.
   //   /// </summary>
   //   /// <value>The public key of the named cryptographic service provider.</value>
   //   public Byte[] PublicKey { get; set; }
   //}

   ///// <summary>
   ///// The event argument class used by <see cref="CILReflectionContext.RSACreationEvent"/> event.
   ///// </summary>
   //public sealed class RSACreationEventArgs : EventArgs
   //{
   //   private readonly String _keyPairContainer;
   //   private readonly RSAParameters? _rsaParams;

   //   internal RSACreationEventArgs( String containerName )
   //      : this( containerName, null )
   //   {
   //   }

   //   internal RSACreationEventArgs( RSAParameters rsaParams )
   //      : this( null, rsaParams )
   //   {
   //   }

   //   private RSACreationEventArgs( String containerName, RSAParameters? rsaParams )
   //   {
   //      this._keyPairContainer = containerName;
   //      this._rsaParams = rsaParams;
   //   }

   //   /// <summary>
   //   /// Gets the key-pair container name to use when creating <see cref="RSA"/>. May be <c>null</c> if no named key-pair container should be used.
   //   /// </summary>
   //   /// <value>Key-pair container name to use when creating <see cref="RSA"/>. May be <c>null</c> if no named key-pair container should be used.</value>
   //   /// <remarks>One of the properties <see cref="KeyPairContainer"/> and <see cref="RSAParameters"/> is always non-<c>null</c>.</remarks>
   //   public String KeyPairContainer
   //   {
   //      get
   //      {
   //         return this._keyPairContainer;
   //      }
   //   }

   //   /// <summary>
   //   /// Gets the RSA parameters for the resulting <see cref="RSA"/>. May be <c>null</c> if named container should be used for creating <see cref="RSA"/>.
   //   /// </summary>
   //   /// <value>The RSA parameters for the resulting <see cref="RSA"/>. May be <c>null</c> if named container should be used for creating <see cref="RSA"/>.</value>
   //   public RSAParameters? RSAParameters
   //   {
   //      get
   //      {
   //         return this._rsaParams;
   //      }
   //   }

   //   /// <summary>
   //   /// Gets or sets the RSA algorithm to use based on <see cref="KeyPairContainer"/> or <see cref="RSAParameters"/> property.
   //   /// </summary>
   //   /// <value>The RSA algorithm to use based on <see cref="KeyPairContainer"/> or <see cref="RSAParameters"/> property.</value>
   //   /// <remarks>Event handlers should set this property.</remarks>
   //   public IDisposable RSA { get; set; }
   //}

   ///// <summary>
   ///// The event argument class used by <see cref="CILReflectionContext.RSASignatureCreationEvent"/> event.
   ///// </summary>
   //public sealed class RSASignatureCreationEventArgs : EventArgs
   //{
   //   private readonly IDisposable _rsa;
   //   private readonly String _hashAlgorithm;
   //   private readonly Byte[] _contentsHash;

   //   internal RSASignatureCreationEventArgs( IDisposable rsa, AssemblyHashAlgorithm algorithm, Byte[] contentsHash )
   //   {
   //      this._rsa = rsa;
   //      this._hashAlgorithm = algorithm.GetAlgorithmName();
   //      this._contentsHash = contentsHash;
   //   }

   //   /// <summary>
   //   /// Gets the RSA algorithm to use.
   //   /// </summary>
   //   /// <value>The RSA algorithm to use.</value>
   //   public IDisposable RSA
   //   {
   //      get
   //      {
   //         return this._rsa;
   //      }
   //   }

   //   /// <summary>
   //   /// Gets the hash algorithm to use.
   //   /// </summary>
   //   /// <value>The hash algorithm to use.</value>
   //   public String HashAlgorithm
   //   {
   //      get
   //      {
   //         return this._hashAlgorithm;
   //      }
   //   }

   //   /// <summary>
   //   /// Gets the hash of the file contents.
   //   /// </summary>
   //   /// <value>The hash of the file contents.</value>
   //   public Byte[] ContentsHash
   //   {
   //      get
   //      {
   //         return this._contentsHash;
   //      }
   //   }

   //   /// <summary>
   //   /// Gets or sets the signature calculated using <see cref="RSA"/>, with the hash algorithm <see cref="HashAlgorithm"/> and given <see cref="ContentsHash"/>.
   //   /// </summary>
   //   /// <value>The signature calculated using <see cref="RSA"/>, with the hash algorithm <see cref="HashAlgorithm"/> and given <see cref="ContentsHash"/>.</value>
   //   /// <remarks>Event handlers should set this property.</remarks>
   //   public Byte[] Signature { get; set; }
   //}

   ///// <summary>
   ///// The event argument class used by <see cref="CILReflectionContext.HashStreamLoadEvent"/> event.
   ///// </summary>
   //public sealed class HashStreamLoadEventArgs : EventArgs
   //{
   //   private readonly AssemblyHashAlgorithm _algorithm;

   //   internal HashStreamLoadEventArgs( AssemblyHashAlgorithm algo )
   //   {
   //      this._algorithm = algo;
   //   }

   //   /// <summary>
   //   /// Gets the hash algorithm for the hash stream.
   //   /// </summary>
   //   /// <value>The hash algorithm for the hash stream.</value>
   //   public AssemblyHashAlgorithm Algorithm
   //   {
   //      get
   //      {
   //         return this._algorithm;
   //      }
   //   }

   //   /// <summary>
   //   /// Gets or sets the resulting hash stream creator callback.
   //   /// </summary>
   //   /// <value>The resulting hash stream creator callback.</value>
   //   /// <remarks>
   //   /// Event handlers should set this property.
   //   /// This should be set to the callback creating cryptographic stream.
   //   /// Typically the callback will just invoke the <see cref="M:System.Security.Cryptography.CryptoStream#ctor(System.IO.Stream, System.Security.Cryptography.ICryptoTransform, System.Security.Cryptography.CryptoStreamMode)"/> constructor, and pass <see cref="Stream.Null"/> as first parameter, the resulting <see cref="Transform"/> as second parameter, and <see cref="F:System.Security.Cryptography.CryptoStreamMode.Write"/> as third parameter.
   //   /// </remarks>
   //   public Func<Stream> CryptoStream { set; get; }

   //   /// <summary>
   //   /// Gets or sets the callback to get hash from the transform.
   //   /// </summary>
   //   /// <value>The callback to get hash from the transform.</value>
   //   /// <remarks>
   //   /// Event handlers should set this property.
   //   /// This callback will be used to get the hash after the copying file contents to crypto stream.
   //   /// Typically the callback will just cast the parameter to <see cref="T:System.Security.Cryptography.HashAlgorithm"/> and return its <see cref="P:System.Security.Cryptography.HashAlgorithm.Hash"/> property.
   //   /// </remarks>
   //   public Func<Byte[]> HashGetter { get; set; }

   //   /// <summary>
   //   /// Gets or sets the callback to compute hash from byte array using the transform.
   //   /// </summary>
   //   /// <value>The callback to compute hash from byte array.</value>
   //   /// <remarks>
   //   /// Event handlers should set this property.
   //   /// This callback will be used to compute public key tokens.
   //   /// </remarks>
   //   public Func<Byte[], Byte[]> ComputeHash { get; set; }

   //   /// <summary>
   //   /// Gets or sets the cryptographic transform object.
   //   /// </summary>
   //   /// <value>The cryptographic transform object.</value>
   //   /// <remarks>
   //   /// Event handlers should set this propery.
   //   /// Once the transform is not needed, the <see cref="IDisposable.Dispose"/> method will be called for it.
   //   /// </remarks>
   //   public IDisposable Transform { get; set; }
   //}

   /// <summary>
   /// This exception is thrown whenever something goes wrong when emitting a strong-signed module.
   /// </summary>
   public class CryptographicException : Exception
   {
      internal CryptographicException( String msg )
         : this( msg, null )
      {

      }

      internal CryptographicException( String msg, Exception inner )
         : base( msg, inner )
      {

      }
   }

   public static class CILModuleIO
   {

      public static CILMetaData ReadModule( this Stream stream, ReadingArguments rArgs = null )
      {
         if ( rArgs == null )
         {
            rArgs = new ReadingArguments();
         }

         if ( rArgs.Headers == null )
         {
            rArgs.Headers = new HeadersData(false);
         }

         return CILAssemblyManipulator.Physical.Implementation.ModuleReader.ReadFromStream( stream, rArgs );
      }

      public static void WriteModule( this CILMetaData md, Stream stream, EmittingArguments eArgs = null )
      {
         if ( eArgs == null )
         {
            eArgs = new EmittingArguments();
         }

         if ( eArgs.Headers == null )
         {
            eArgs.Headers = new HeadersData();
         }

         CILAssemblyManipulator.Physical.Implementation.ModuleWriter.WriteModule( md, eArgs, stream );
      }
   }
}