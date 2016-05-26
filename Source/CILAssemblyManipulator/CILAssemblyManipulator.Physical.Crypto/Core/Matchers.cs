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
using CILAssemblyManipulator.Physical.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILAssemblyManipulator.Physical
{
#pragma warning disable 1591
   public sealed class AssemblyReferenceMatcherExact : IEqualityComparer<AssemblyReference>, IEqualityComparer<AssemblyDefinition>
   {

      private static AssemblyReferenceMatcherExact Instance { get; }

      public static IEqualityComparer<AssemblyReference> AssemblyRefInstance
      {
         get
         {
            return Instance;
         }
      }

      public static IEqualityComparer<AssemblyDefinition> AssemblyDefInstance
      {
         get
         {
            return Instance;
         }
      }

      static AssemblyReferenceMatcherExact()
      {
         Instance = new AssemblyReferenceMatcherExact();
      }

      private AssemblyReferenceMatcherExact()
      {
      }

      Boolean IEqualityComparer<AssemblyReference>.Equals( AssemblyReference x, AssemblyReference y )
      {
         return Match( x.AssemblyInformation, x.Attributes, y.AssemblyInformation, y.Attributes );
      }

      Int32 IEqualityComparer<AssemblyReference>.GetHashCode( AssemblyReference obj )
      {
         return obj?.AssemblyInformation?.GetHashCode() ?? 0;
      }

      Boolean IEqualityComparer<AssemblyDefinition>.Equals( AssemblyDefinition x, AssemblyDefinition y )
      {
         return Match( x.AssemblyInformation, x.Attributes, y.AssemblyInformation, y.Attributes );
      }

      Int32 IEqualityComparer<AssemblyDefinition>.GetHashCode( AssemblyDefinition obj )
      {
         return obj?.AssemblyInformation?.GetHashCode() ?? 0;
      }

      public static Boolean Match( AssemblyInformation x, AssemblyFlags xFlags, AssemblyInformation y, AssemblyFlags yFlags )
      {
         Boolean retVal;
         if ( xFlags.IsFullPublicKey() == yFlags.IsFullPublicKey() )
         {
            retVal = x.Equals( y );
         }
         else
         {
            retVal = x.Equals( y, false );
            if ( retVal && x.PublicKeyOrToken.IsNullOrEmpty() == y.PublicKeyOrToken.IsNullOrEmpty() )
            {
               if ( !x.PublicKeyOrToken.IsNullOrEmpty() )
               {
                  IEnumerable<Byte> xBytes, yBytes;
                  if ( xFlags.IsFullPublicKey() )
                  {
                     // Create public key token for x and compare with y
                     xBytes = HashAlgorithmPool.SHA1.EnumeratePublicKeyToken( x.PublicKeyOrToken );
                     yBytes = y.PublicKeyOrToken;
                  }
                  else
                  {
                     // Create public key token for y and compare with x
                     xBytes = x.PublicKeyOrToken;
                     yBytes = HashAlgorithmPool.SHA1.EnumeratePublicKeyToken( y.PublicKeyOrToken );
                  }
                  retVal = xBytes.SequenceEqual( yBytes );
               }
            }
            else
            {
               retVal = false;
            }
         }
         return retVal;
      }

      public static Boolean Match( AssemblyDefinition assemblyDef, AssemblyReference assemblyRef )
      {
         return ( assemblyDef == null && assemblyRef == null ) || ( assemblyDef != null && assemblyRef != null && Match( assemblyDef.AssemblyInformation, AssemblyFlags.PublicKey, assemblyRef.AssemblyInformation, assemblyRef.Attributes ) );
      }

      public static Boolean Match( AssemblyDefinition assemblyDef, AssemblyInformation assemblyRefInfo, AssemblyFlags assemblyRefFlags )
      {
         return ( assemblyDef == null && assemblyRefInfo == null ) || ( assemblyDef != null && assemblyRefInfo != null && Match( assemblyDef.AssemblyInformation, AssemblyFlags.PublicKey, assemblyRefInfo, assemblyRefFlags ) );
      }
   }

   public sealed class AssemblyReferenceMatcherRuntime : IEqualityComparer<AssemblyReference>, IEqualityComparer<AssemblyDefinition>
   {
      private static AssemblyReferenceMatcherRuntime Instance { get; }

      public static IEqualityComparer<AssemblyReference> AssemblyRefInstance
      {
         get
         {
            return Instance;
         }
      }

      public static IEqualityComparer<AssemblyDefinition> AssemblyDefInstance
      {
         get
         {
            return Instance;
         }
      }

      static AssemblyReferenceMatcherRuntime()
      {
         Instance = new AssemblyReferenceMatcherRuntime();
      }

      private AssemblyReferenceMatcherRuntime()
      {

      }

      Boolean IEqualityComparer<AssemblyReference>.Equals( AssemblyReference x, AssemblyReference y )
      {
         return Match( x.AssemblyInformation, x.Attributes, y.AssemblyInformation, y.Attributes );
      }

      Int32 IEqualityComparer<AssemblyReference>.GetHashCode( AssemblyReference obj )
      {
         return obj?.AssemblyInformation?.GetHashCode() ?? 0;
      }

      Boolean IEqualityComparer<AssemblyDefinition>.Equals( AssemblyDefinition x, AssemblyDefinition y )
      {
         return Match( x.AssemblyInformation, x.Attributes, y.AssemblyInformation, y.Attributes );
      }

      Int32 IEqualityComparer<AssemblyDefinition>.GetHashCode( AssemblyDefinition obj )
      {
         return obj?.AssemblyInformation?.GetHashCode() ?? 0;
      }

      public static Boolean Match( AssemblyInformation x, AssemblyFlags xFlags, AssemblyInformation y, AssemblyFlags yFlags )
      {
         return xFlags.IsRetargetable() || yFlags.IsRetargetable() ?
            // Simple name match
            String.Equals( x.Name, y.Name ) :
            // Exact match since both are not retargetable
            AssemblyReferenceMatcherExact.Match( x, xFlags, y, yFlags );
      }

      public static Boolean Match( AssemblyDefinition assemblyDef, AssemblyReference assemblyRef )
      {
         return ( assemblyDef == null && assemblyRef == null ) || ( assemblyDef != null && assemblyRef != null && Match( assemblyDef.AssemblyInformation, AssemblyFlags.PublicKey, assemblyRef.AssemblyInformation, assemblyRef.Attributes ) );
      }

      public static Boolean Match( AssemblyDefinition assemblyDef, AssemblyInformation assemblyRefInfo, AssemblyFlags assemblyRefFlags )
      {
         return ( assemblyDef == null && assemblyRefInfo == null ) || ( assemblyDef != null && assemblyRefInfo != null && Match( assemblyDef.AssemblyInformation, AssemblyFlags.PublicKey, assemblyRefInfo, assemblyRefFlags ) );
      }
   }

#pragma warning restore 1591
}
