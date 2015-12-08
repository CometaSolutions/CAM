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
using CILAssemblyManipulator.Physical.IO;
using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
   public static partial class CILMetaDataIO
   {
      public static CILMetaData ReadModule( this Stream stream, ReadingArguments rArgs = null )
      {
         ImageInformation imageInfo;
         var md = stream.ReadMetaDataFromStream( rArgs?.ReaderFunctionalityProvider, rArgs?.TableInformationProvider, out imageInfo );
         if ( rArgs != null )
         {
            rArgs.ImageInformation = imageInfo;
         }
         return md;
      }
   }

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

      public ImageInformation ImageInformation { get; set; }

   }

   /// <summary>
   /// Class containing information specific to reading the module.
   /// </summary>
   public sealed class ReadingArguments : IOArguments
   {

      public ReaderFunctionalityProvider ReaderFunctionalityProvider { get; set; }

      public Meta.MetaDataTableInformationProvider TableInformationProvider { get; set; }
   }

   /// <summary>
   /// Class containing information specific to writing the module.
   /// </summary>
   public sealed class WritingArguments : IOArguments
   {

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

      public WriterFunctionalityProvider WriterFunctionalityProvider { get; set; }

      public WritingOptions WritingOptions { get; set; }

   }

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

   /// <summary>
   /// This will be thrown by <see cref="CILMetaDataIO.ReadModule"/> method if the target file does not contain a managed assembly.
   /// </summary>
   public class NotAManagedModuleException : Exception
   {
      internal NotAManagedModuleException()
      {

      }
   }

   //[AttributeUsage( AttributeTargets.Property | AttributeTargets.ReturnValue | AttributeTargets.Parameter, AllowMultiple = false )]
   //public sealed class InterpretAsUnsignedAttribute : Attribute
   //{

   //}

}

public static partial class E_CILPhysical
{
   public static void WriteModule( this CILMetaData md, Stream stream, WritingArguments eArgs = null )
   {
      if ( eArgs == null )
      {
         eArgs = new WritingArguments();
      }

      eArgs.ImageInformation = stream.WriteMetaDataToStream(
         md,
         eArgs.WriterFunctionalityProvider,
         eArgs.WritingOptions,
         eArgs.StrongName,
         eArgs.DelaySign,
         eArgs.CryptoCallbacks,
         eArgs.SigningAlgorithm
         );

   }
}