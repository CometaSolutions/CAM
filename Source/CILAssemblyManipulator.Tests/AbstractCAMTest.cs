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
using CAMPhysical::CILAssemblyManipulator.Physical.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CILAssemblyManipulator.Physical;
using NUnit.Framework;
using System.IO;
using CILMerge;
using System.Diagnostics;
using Microsoft.Win32;
using CILAssemblyManipulator.Logical;
using CILAssemblyManipulator.Structural;

namespace CILAssemblyManipulator.Tests
{
   public class AbstractCAMTest
   {


      private static readonly System.Reflection.Assembly _msCorLib = typeof( Object ).Assembly;
      private static readonly String _msCorLibLocation = new Uri( _msCorLib.CodeBase ).LocalPath;

      private static readonly System.Reflection.Assembly _camPhysical = typeof( CILMetaData ).Assembly;
      private static readonly String _camPhysicalLocation = new Uri( _camPhysical.CodeBase ).LocalPath;

      private static readonly System.Reflection.Assembly _camStructural = typeof( AssemblyStructure ).Assembly;
      private static readonly String _camStructuralLocation = new Uri( _camStructural.CodeBase ).LocalPath;

      private static readonly System.Reflection.Assembly _camLogical = typeof( CILModule ).Assembly;
      private static readonly String _camLogicalLocation = new Uri( _camLogical.CodeBase ).LocalPath;

      private static readonly System.Reflection.Assembly _merge = typeof( CILMerger ).Assembly;
      private static readonly String _mergeLocation = new Uri( _merge.CodeBase ).LocalPath;

      protected static System.Reflection.Assembly MSCorLib
      {
         get
         {
            return _msCorLib;
         }
      }

      protected static String MSCorLibLocation
      {
         get
         {
            return _msCorLibLocation;
         }
      }

      protected static System.Reflection.Assembly CAMPhysical
      {
         get
         {
            return _camPhysical;
         }
      }

      protected static String CAMPhysicalLocation
      {
         get
         {
            return _camPhysicalLocation;
         }
      }

      protected static System.Reflection.Assembly CAMStructural
      {
         get
         {
            return _camStructural;
         }
      }

      protected static String CAMStructuralLocation
      {
         get
         {
            return _camStructuralLocation;
         }
      }

      protected static System.Reflection.Assembly CAMLogical
      {
         get
         {
            return _camLogical;
         }
      }

      protected static String CAMLogicalLocation
      {
         get
         {
            return _camLogicalLocation;
         }
      }

      protected static System.Reflection.Assembly CILMerge
      {
         get
         {
            return _merge;
         }
      }

      protected static String CILMergeLocation
      {
         get
         {
            return _mergeLocation;
         }
      }

      public static void ValidateAllIsResolved( CILMetaData md )
      {
         for ( var i = 0; i < md.CustomAttributeDefinitions.TableContents.Count; ++i )
         {
            var ca = md.CustomAttributeDefinitions.TableContents[i];
            var sig = ca.Signature;
            Assert.IsNotNull( sig );
            Assert.IsNotInstanceOf<RawCustomAttributeSignature>( sig );
         }

         for ( var i = 0; i < md.SecurityDefinitions.TableContents.Count; ++i )
         {
            var sec = md.SecurityDefinitions.TableContents[i];
            foreach ( var permission in sec.PermissionSets )
            {
               Assert.IsNotNull( permission );
               Assert.IsNotInstanceOf<RawSecurityInformation>( permission );
               foreach ( var arg in ( (SecurityInformation) permission ).NamedArguments )
               {
                  Assert.IsNotNull( arg );
               }
            }
         }

         for ( var i = 0; i < md.FieldMarshals.TableContents.Count; ++i )
         {
            var marshal = md.FieldMarshals.TableContents[i];
            var info = marshal.NativeType;
            Assert.IsNotNull( info );
            Assert.IsNotInstanceOf<RawMarshalingInfo>( info );
         }
      }

      public static CILMetaData ReadFromAssembly( System.Reflection.Assembly assembly, ReadingArguments rArgs )
      {
         return CILMetaDataIO.ReadModuleFrom( new Uri( assembly.CodeBase ).LocalPath, rArgs );
      }

      protected static void RunPEVerify( String fileName, Boolean verifyStrongName )
      {
         Verification.RunPEVerify( fileName, verifyStrongName );
      }

   }
}
