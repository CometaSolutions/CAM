using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical.Implementation
{
   // Most info from http://msdn.microsoft.com/en-us/library/cc250013.aspx 
   internal static class CryptoUtils
   {
      private const UInt32 RSA1 = 0x31415352;
      private const UInt32 RSA2 = 0x32415352;
      private const Int32 CAPI_HEADER_SIZE = 12;
      private const Byte PRIVATE_KEY = 0x07;
      private const Byte PUBLIC_KEY = 0x06;

      internal static Boolean TryCreateRSAParametersFromCapiBLOB( Byte[] blob, Int32 offset, out Int32 pkLen, out Int32 algID, out RSAParameters result, out String errorString )
      {
         pkLen = 0;
         algID = 0;
         var startOffset = offset;
         errorString = null;
         var retVal = false;
         result = new RSAParameters();
         if ( startOffset + CAPI_HEADER_SIZE < blob.Length )
         {
            try
            {
               var firstByte = blob[offset++];
               if ( ( firstByte == PRIVATE_KEY || firstByte == PUBLIC_KEY ) // Check whether this is private or public key blob
                   && blob.ReadByteFromBytes( ref offset ) == 0x02 // Version (0x02)
                  )
               {
                  blob.Skip( ref offset, 2 ); // Skip reserved (short, should be zero)
                  algID = blob.ReadInt32LEFromBytes( ref offset ); // alg-id (should be either 0x0000a400 for RSA_KEYX or 0x00002400 for RSA_SIGN)
                  var keyType = blob.ReadUInt32LEFromBytes( ref offset );
                  if ( keyType == RSA2 // RSA2
                      || keyType == RSA1 ) // RSA1
                  {
                     var isPrivateKey = firstByte == PRIVATE_KEY;


                     var bitLength = blob.ReadInt32LEFromBytes( ref offset );
                     var byteLength = bitLength >> 3;
                     var byteHalfLength = byteLength >> 1;

                     // Exponent
                     var exp = blob.CreateAndBlockCopyTo( ref offset, 4 );
                     //if ( BitConverter.IsLittleEndian )
                     //{
                     Array.Reverse( exp );
                     //}
                     var tmp = 0;
                     // Trim exponent
                     while ( exp[tmp] == 0 )
                     {
                        ++tmp;
                     }
                     if ( tmp != 0 )
                     {
                        exp = exp.Skip( tmp ).ToArray();
                     }
                     result.Exponent = exp;

                     // Modulus
                     result.Modulus = blob.CreateAndBlockCopyTo( ref offset, byteLength );
                     pkLen = offset - startOffset;

                     if ( isPrivateKey )
                     {

                        // prime1
                        result.P = blob.CreateAndBlockCopyTo( ref offset, byteHalfLength );

                        // prime2
                        result.Q = blob.CreateAndBlockCopyTo( ref offset, byteHalfLength );

                        // exponent1
                        result.DP = blob.CreateAndBlockCopyTo( ref offset, byteHalfLength );

                        // exponent2
                        result.DQ = blob.CreateAndBlockCopyTo( ref offset, byteHalfLength );

                        // coefficient
                        result.InverseQ = blob.CreateAndBlockCopyTo( ref offset, byteHalfLength );

                        // private exponent
                        result.D = blob.CreateAndBlockCopyTo( ref offset, byteLength );
                     }

                     //if ( BitConverter.IsLittleEndian )
                     //{
                     // Reverse arrays, since they are stored in big-endian format in BLOB
                     Array.Reverse( result.Modulus );
                     //if ( isPrivateKey )
                     //{
                     Array.Reverse( result.P );
                     Array.Reverse( result.Q );
                     Array.Reverse( result.DP );
                     Array.Reverse( result.DQ );
                     Array.Reverse( result.InverseQ );
                     Array.Reverse( result.D );
                     //}
                     //}

                     retVal = true;
                  }
                  else
                  {
                     errorString = "Invalid key type: " + keyType;
                  }
               }
               else
               {
                  errorString = "Invalid BLOB header.";
               }
            }
            catch
            {
               errorString = "Invalid BLOB";
            }
         }

         return retVal;
      }

      internal static Boolean TryCreateSigningInformationFromKeyBLOB( Byte[] blob, AssemblyHashAlgorithm? signingAlgorithm, out Byte[] publicKey, out AssemblyHashAlgorithm actualSigningAlgorithm, out RSAParameters rsaParameters, out String errorString )
      {
         // There might be actual key after a header, if first byte is zero.
         var hasHeader = blob[0] == 0x00;
         Int32 pkLen, algID;
         var retVal = TryCreateRSAParametersFromCapiBLOB( blob, hasHeader ? CAPI_HEADER_SIZE : 0, out pkLen, out algID, out rsaParameters, out errorString );

         if ( retVal )
         {

            publicKey = new Byte[pkLen + CAPI_HEADER_SIZE];
            var idx = 0;
            if ( hasHeader )
            {
               // Just copy from blob
               publicKey.BlockCopyFrom( ref idx, blob, 0, publicKey.Length );
               idx = 4;
               if ( signingAlgorithm.HasValue )
               {
                  actualSigningAlgorithm = signingAlgorithm.Value;
                  publicKey.WriteInt32LEToBytes( ref idx, (Int32) actualSigningAlgorithm );
               }
               else
               {
                  actualSigningAlgorithm = (AssemblyHashAlgorithm) publicKey.ReadInt32LEFromBytes( ref idx );
               }
            }
            else
            {
               // Write public key, including header
               // Write header explicitly. ALG-ID, followed by AssemblyHashAlgorithmID, followed by the size of the PK
               actualSigningAlgorithm = signingAlgorithm ?? AssemblyHashAlgorithm.SHA1;// Defaults to SHA1
               publicKey
                  .WriteInt32LEToBytes( ref idx, algID )
                  .WriteInt32LEToBytes( ref idx, (Int32) actualSigningAlgorithm )
                  .WriteInt32LEToBytes( ref idx, pkLen )
                  .BlockCopyFrom( ref idx, blob, 0, pkLen );
            }
            // Mark PK actually being PK
            publicKey[CAPI_HEADER_SIZE] = PUBLIC_KEY;

            // Set public key algorithm to RSA1
            idx = 20;
            publicKey.WriteUInt32LEToBytes( ref idx, RSA1 );
         }
         else
         {
            publicKey = null;
            actualSigningAlgorithm = AssemblyHashAlgorithm.SHA1;
         }

         return retVal;
      }
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
}
