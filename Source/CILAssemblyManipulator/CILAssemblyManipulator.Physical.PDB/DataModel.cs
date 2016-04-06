﻿/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
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
 * See the License for the specific _language governing permissions and
 * limitations under the License. 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtils;

namespace CILAssemblyManipulator.Physical.PDB
{
   /// <summary>
   /// This is root class to obtain all PDB information related to the single PDB file.
   /// </summary>
   public sealed class PDBInstance
   {

      /// <summary>
      /// Creates new instance of <see cref="PDBInstance"/>.
      /// </summary>
      public PDBInstance()
      {
         //this.Sources = new Dictionary<String, PDBSource>();
         this.Modules = new List<PDBModule>();
      }

      /// <summary>
      /// Gets the list of <see cref="PDBModule"/> objects for this <see cref="PDBInstance"/>.
      /// </summary>
      /// <value>The list of <see cref="PDBModule"/> objects for this <see cref="PDBInstance"/>.</value>
      public List<PDBModule> Modules { get; }

      /// <summary>
      /// Gets or sets the unique identifier of this <see cref="PDBInstance"/>.
      /// </summary>
      /// <value>The unique identifier of this <see cref="PDBInstance"/>.</value>
      /// <remarks>
      /// This <see cref="Guid"/> should match the one serialized in <see cref="P:CILAssemblyManipulator.Physical.DebugInformation.DebugData"/>.
      /// </remarks>
      public Guid DebugGUID { get; set; }

      /// <summary>
      /// Gets or sets the timestamp of this <see cref="PDBInstance"/> as integer value.
      /// </summary>
      /// <value>The timestamp of this <see cref="PDBInstance"/> as integer value.</value>
      /// <remarks>
      /// This <see cref="Guid"/> should match the one serialized in <see cref="P:CILAssemblyManipulator.Physical.DebugInformation.Timestamp"/>.
      /// </remarks>
      [CLSCompliant( false )]
      public UInt32 Timestamp { get; set; }

      /// <summary>
      /// Gets or sets the age of this <see cref="PDBInstance"/>.
      /// </summary>
      /// <value>The age of this <see cref="PDBInstance"/>.</value>
      /// <remarks>
      /// This integer should match the one serialized in <see cref="P:CILAssemblyManipulator.Physical.DebugInformation.DebugData"/>.
      /// </remarks>
      [CLSCompliant( false )]
      public UInt32 Age { get; set; }

      /// <summary>
      /// Gets or sets the server for the sources of this <see cref="PDBInstance"/>, as string.
      /// </summary>
      /// <value>The server for the sources of this <see cref="PDBInstance"/>, as string.</value>
      public String SourceServer { get; set; }
   }

   /// <summary>
   /// This class represents a single source in PDB, typically meaning a single file.
   /// </summary>
   public sealed class PDBSource
   {

      /// <summary>
      /// Creates a new instance of <see cref="PDBSource"/>.
      /// </summary>
      public PDBSource()
      {
         this.DocumentType = Guid.Empty;
         this.Language = Guid.Empty;
         this.Vendor = Guid.Empty;
         this.HashAlgorithm = Guid.Empty;
      }

      /// <summary>
      /// Gets or sets the <see cref="Guid"/> for the type of the document represented by this <see cref="PDBSource"/>.
      /// </summary>
      /// <value>The <see cref="Guid"/> for the type of the document represented by this <see cref="PDBSource"/>.</value>
      public Guid DocumentType { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="Guid"/> for the type of language of the document represented by this <see cref="PDBSource"/>.
      /// </summary>
      /// <value>The <see cref="Guid"/> for the type of language of the document represented by this <see cref="PDBSource"/>.</value>
      public Guid Language { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="Guid"/> for the vendor of the document represented by this <see cref="PDBSource"/>.
      /// </summary>
      /// <value>The <see cref="Guid"/> for the vendor of the document represented by this <see cref="PDBSource"/>.</value>
      public Guid Vendor { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="Guid"/> for the hash algorithm used to compute <see cref="Hash"/> for this <see cref="PDBSource"/>.
      /// </summary>
      /// <value>The <see cref="Guid"/> for the hash algorithm used to compute <see cref="Hash"/> for this <see cref="PDBSource"/>.</value>
      public Guid HashAlgorithm { get; set; }

      /// <summary>
      /// Gets or sets the hash computed for the document represented by this <see cref="PDBSource"/>, as byte array.
      /// </summary>
      /// <value>The hash computed for the document represented by this <see cref="PDBSource"/>, as byte array.</value>
      public Byte[] Hash { get; set; }

      /// <summary>
      /// Gets or sets the name of this <see cref="PDBSource"/>.
      /// </summary>
      /// <value>The name of this <see cref="PDBSource"/>.</value>
      /// <remarks>
      /// Typically, the name is full file path to the document represented by this <see cref="PDBSource"/>.
      /// </remarks>
      public String Name { get; set; }

      /// <summary>
      /// Creates the textual representation of this <see cref="PDBSource"/>.
      /// </summary>
      /// <returns>The textual representation of this <see cref="PDBSource"/>.</returns>
      /// <remarks>
      /// The textual representation of this <see cref="PDBSource"/> is the value of the <see cref="Name"/> property.
      /// </remarks>
      public override String ToString()
      {
         return this.Name;
      }
   }

   /// <summary>
   /// This class represents a single module of the PDB, holding a list of <see cref="PDBFunction"/>s.
   /// </summary>
   /// <remarks>
   /// Typically, one module is created for each class.
   /// </remarks>
   public sealed class PDBModule
   {
      /// <summary>
      /// Creates a new instance of <see cref="PDBModule"/>.
      /// </summary>
      public PDBModule()
      {
         this.Functions = new List<PDBFunction>();
      }

      /// <summary>
      /// Gets the list of all <see cref="PDBFunction"/>s that this <see cref="PDBModule"/> holds.
      /// </summary>
      /// <value>The list of all <see cref="PDBFunction"/>s that this <see cref="PDBModule"/> holds.</value>
      /// <seealso cref="PDBFunction"/>
      public List<PDBFunction> Functions { get; }

      /// <summary>
      /// Gets or sets the name of this module.
      /// </summary>
      /// <value>The name of this module.</value>
      public String Name { get; set; }

      /// <summary>
      /// Creates the textual representation of this <see cref="PDBModule"/>.
      /// </summary>
      /// <returns>The textual representation of this <see cref="PDBModule"/>.</returns>
      /// <remarks>
      /// The textual representation of this <see cref="PDBModule"/> is the value of the <see cref="Name"/> property.
      /// </remarks>
      public override String ToString()
      {
         return this.Name;
      }
   }

   /// <summary>
   /// This abstract class exposes common properties of <see cref="PDBScope"/> and <see cref="PDBFunction"/>.
   /// </summary>
   /// <remarks>
   /// This class can not be instantiated directly, instead use <see cref="PDBScope"/> or <see cref="PDBFunction"/>.
   /// </remarks>
   public abstract class PDBScopeOrFunction
   {
      internal PDBScopeOrFunction()
      {
         this.Slots = new List<PDBSlot>();
         this.Scopes = new List<PDBScope>();
         this.UsedNamespaces = new List<String>();
      }

      /// <summary>
      /// Gets the list of all <see cref="PDBSlot"/>s of this <see cref="PDBScopeOrFunction"/>.
      /// </summary>
      /// <value>The list of all <see cref="PDBSlot"/>s of this <see cref="PDBScopeOrFunction"/>.</value>
      /// <remarks>
      /// Typically, one <see cref="PDBSlot"/> is created for each local variable.
      /// </remarks>
      /// <seealso cref="PDBSlot"/>
      public List<PDBSlot> Slots { get; }

      /// <summary>
      /// Gets the list of sub-scopes of this <see cref="PDBScopeOrFunction"/>.
      /// </summary>
      /// <value>The list of sub-scopes of this <see cref="PDBScopeOrFunction"/>.</value>
      /// <seealso cref="PDBScope"/>
      public List<PDBScope> Scopes { get; }

      /// <summary>
      /// Gets the list of all namespaces for 'using' declarations.
      /// </summary>
      /// <value>The list of all namespaces for 'using' declarations.</value>
      public List<String> UsedNamespaces { get; }

      /// <summary>
      /// Gets or sets the name of this <see cref="PDBScopeOrFunction"/>.
      /// </summary>
      /// <value>The name of this <see cref="PDBScopeOrFunction"/>.</value>
      public String Name { get; set; }

      /// <summary>
      /// Gets or sets the length of this <see cref="PDBScopeOrFunction"/>, in bytes.
      /// </summary>
      /// <value>The length of this <see cref="PDBScopeOrFunction"/>, in bytes.</value>
      public Int32 Length { get; set; }

   }


   /// <summary>
   /// This class represents a single function in PDB.
   /// </summary>
   /// <remarks>
   /// Typically, one instance of <see cref="PDBFunction"/> is created for every method with IL code.
   /// </remarks>
   public sealed class PDBFunction : PDBScopeOrFunction
   {



      //internal Int32 parent;
      //internal Int32 end;
      //internal Int32 next;

      //internal Int32 debugStart;
      //internal Int32 debugEnd;
      //internal Int32 _offset;
      //internal UInt16 _segment;
      //private Byte _flags;
      //internal UInt16 returnReg;

      /// <summary>
      /// Creates a new instance of <see cref="PDBFunction"/>.
      /// </summary>
      public PDBFunction()
      {
         this.LocalScopes = new List<PDBLocalScope>();
         this.Lines = new List<PDBLine>();
      }

      /// <summary>
      /// Gets the list of all <see cref="PDBLine"/>s with source information about the code of this <see cref="PDBFunction"/>.
      /// </summary>
      /// <value>The list of all <see cref="PDBLine"/>s with source information about the code of this <see cref="PDBFunction"/>.</value>
      /// <seealso cref="PDBLine"/>
      public List<PDBLine> Lines { get; }

      /// <summary>
      /// Gets or sets the token of this <see cref="PDBFunction"/>.
      /// </summary>
      /// <value>The token of this <see cref="PDBFunction"/>.</value>
      /// <remarks>
      /// The token should be transformable into <see cref="T:CILAssemblyManipulator.Physical.TableIndex"/> by <see cref="M:CILAssemblyManipulator.Physical.TableIndex.FromOneBasedToken(System.Int32)"/> method.
      /// </remarks>
      [CLSCompliant( false )]
      public UInt32 Token { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="PDBAsyncMethodInfo"/> associated with this <see cref="PDBFunction"/>.
      /// </summary>
      /// <value>The <see cref="PDBAsyncMethodInfo"/> associated with this <see cref="PDBFunction"/>.</value>
      public PDBAsyncMethodInfo AsyncMethodInfo { get; set; }

      /// <summary>
      /// Gets or sets the edit and continue -ID of this <see cref="PDBFunction"/>.
      /// </summary>
      /// <value>The edit and continue -ID of this <see cref="PDBFunction"/>.</value>
      [CLSCompliant( false )]
      public UInt32 ENCID { get; set; }

      /// <summary>
      /// Gets or sets the token of forwarding method for this <see cref="PDBFunction"/>.
      /// </summary>
      /// <value>The token of forwarding method for this <see cref="PDBFunction"/>.</value>
      [CLSCompliant( false )]
      public UInt32 ForwardingMethodToken { get; set; }

      /// <summary>
      /// Gets the list of all local scopes of this <see cref="PDBFunction"/>.
      /// </summary>
      /// <value>The list of all local scopes of this <see cref="PDBFunction"/>.</value>
      public List<PDBLocalScope> LocalScopes { get; }

      /// <summary>
      /// Gets or sets the name of the iterator class, if this <see cref="PDBFunction"/> represents iterator method.
      /// </summary>
      /// <value>The name of the iterator class, if this <see cref="PDBFunction"/> represents iterator method.</value>
      public String IteratorClass { get; set; }

      /// <summary>
      /// Gets or sets the oken of forwarding method on module scope for this <see cref="PDBFunction"/>.
      /// </summary>
      /// <value>The oken of forwarding method on module scope for this <see cref="PDBFunction"/>.</value>
      [CLSCompliant( false )]
      public UInt32 ModuleForwardingMethodToken { get; set; }

      /// <summary>
      /// Creates the textual representation of this <see cref="PDBFunction"/>.
      /// </summary>
      /// <returns>The textual representation of this <see cref="PDBFunction"/>.</returns>
      /// <remarks>
      /// The textual representation of this <see cref="PDBFunction"/> includes the values of the <see cref="PDBScopeOrFunction.Name"/> and <see cref="Token"/> properties.
      /// </remarks>
      public override String ToString()
      {
         return "Function " + this.Name + " @" + String.Format( "{0:X8}", this.Token );
      }
   }

   /// <summary>
   /// This class represents a single lexical scope within a <see cref="PDBFunction"/>.
   /// </summary>
   /// <seealso cref="PDBScopeOrFunction.Scopes"/>
   public sealed class PDBScope : PDBScopeOrFunction
   {

      /// <summary>
      /// Gets or sets the offset, in bytes, where this <see cref="PDBScope"/> begins.
      /// </summary>
      /// <value>The offset, in bytes, where this <see cref="PDBScope"/> begins.</value>
      /// <remarks>
      /// The length of this scope is determined by <see cref="PDBScopeOrFunction.Length"/> property.
      /// </remarks>
      public Int32 Offset { get; set; }

      /// <summary>
      /// Creates the textual representation of this <see cref="PDBScope"/>.
      /// </summary>
      /// <returns>The textual representation of this <see cref="PDBScope"/>.</returns>
      /// <remarks>
      /// The textual representation of this <see cref="PDBScope"/> includes the values of the <see cref="PDBScopeOrFunction.Name"/> and <see cref="Offset"/> properties.
      /// </remarks>
      public override String ToString()
      {
         return "Scope " + this.Name + " @" + String.Format( "{0:X8}", this.Offset );
      }
   }

   //public sealed class PDBConstant
   //{
   //   private UInt32 _token;
   //   private String _name;
   //   private Object _value;

   //   internal PDBConstant( Byte[] array, ref Int32 idx )
   //   {
   //      this._token = array.ReadUInt32LEFromBytes( ref idx );
   //      var valueKindOrByte = array.ReadByteFromBytes( ref idx );
   //      var valueOrZero = array.ReadByteFromBytes( ref idx );
   //      if ( valueOrZero == 0 )
   //      {
   //         this._value = valueKindOrByte;
   //      }
   //      else if ( valueOrZero == 0x80 )
   //      {
   //         switch ( valueKindOrByte )
   //         {
   //            case 0x00: // LF_NUMERIC
   //               this._value = array.ReadSByteFromBytes( ref idx );
   //               break;
   //            case 0x01: // LF_SHORT
   //               this._value = array.ReadInt16LEFromBytes( ref idx );
   //               break;
   //            case 0x02: // LF_USHORT
   //               this._value = array.ReadUInt16LEFromBytes( ref idx );
   //               break;
   //            case 0x03: // LF_LONG
   //               this._value = array.ReadInt32LEFromBytes( ref idx );
   //               break;
   //            case 0x04: // LF_ULONG
   //               this._value = array.ReadUInt32LEFromBytes( ref idx );
   //               break;
   //            case 0x05: // LF_REAL32
   //               this._value = array.ReadSingleLEFromBytes( ref idx );
   //               break;
   //            case 0x06: // LF_REAL64
   //               this._value = array.ReadDoubleLEFromBytes( ref idx );
   //               break;
   //            case 0x09: // LF_QUADWORD
   //               this._value = array.ReadInt64LEFromBytes( ref idx );
   //               break;
   //            case 0x0a: // LF_UQUADWORD
   //               this._value = array.ReadUInt64LEFromBytes( ref idx );
   //               break;
   //            case 0x10: // LF_VARSTRING
   //               this._value = array.ReadShortLenghtPrefixedString( ref idx, PDBIO.NAME_ENCODING );
   //               break;
   //            case 0x19: // LF_DECIMAL
   //               var bits = array.ReadInt32ArrayLEFromBytes( ref idx, 4 );
   //               // For some weird reason, the order is: _flags, hi, lo, mid...
   //               this._value = new Decimal( bits[2], bits[3], bits[1], bits[0] < 0, (Byte) ( ( bits[0] & 0x00FF0000 ) >> 16 ) );
   //               break;
   //            default:
   //               // TODO real80, real128, complex32, complex64, complex80, complex128, ocword, uoctword, date, utf8string
   //               break;
   //         }
   //      }
   //      else
   //      {
   //         // Unknown stuff?
   //      }
   //      this._name = array.ReadZeroTerminatedStringFromBytes( ref idx, PDBIO.NAME_ENCODING );
   //   }

   //   [CLSCompliant( false )]
   //   public PDBConstant( String _name, UInt32 _token, Object value )
   //   {
   //      this._name = _name;
   //      this._token = _token;
   //      this._value = value;
   //   }

   //   public String Name
   //   {
   //      get
   //      {
   //         return this._name;
   //      }
   //      set
   //      {
   //         this._name = value;
   //      }
   //   }

   //   [CLSCompliant( false )]
   //   public UInt32 Token
   //   {
   //      get
   //      {
   //         return this._token;
   //      }
   //      set
   //      {
   //         this._token = value;
   //      }
   //   }

   //   public override String ToString()
   //   {
   //      return "[" + this._token + "]: " + this._name + " = " + this._value;
   //   }
   //}

   /// <summary>
   /// This class represents a single data slot of the <see cref="PDBFunction"/>.
   /// </summary>
   /// <remarks>
   /// Typically, one instance of <see cref="PDBSlot"/> is created for every local variable of method represented by <see cref="PDBFunction"/>.
   /// </remarks>
   /// <seealso cref="PDBFunction"/>
   /// <seealso cref="PDBScopeOrFunction.Slots"/>
   public sealed class PDBSlot
   {

      /// <summary>
      /// Gets or sets the name of this <see cref="PDBSlot"/>.
      /// </summary>
      /// <value>The name of this <see cref="PDBSlot"/>.</value>
      public String Name { get; set; }

      /// <summary>
      /// Gets or sets the index of this <see cref="PDBSlot"/> within all slots of <see cref="PDBScopeOrFunction"/>.
      /// </summary>
      /// <value>The index of this <see cref="PDBSlot"/> within all slots of <see cref="PDBScopeOrFunction"/>.</value>
      public Int32 SlotIndex { get; set; }

      /// <summary>
      /// Gets or sets the token which has the information about the type of the local variable represented by this <see cref="PDBSlot"/>.
      /// </summary>
      /// <value>The token which has the information about the type of the local variable represented by this <see cref="PDBSlot"/>.</value>
      /// <remarks>
      /// The token should be transformable into <see cref="T:CILAssemblyManipulator.Physical.TableIndex"/> by <see cref="M:CILAssemblyManipulator.Physical.TableIndex.FromOneBasedToken(System.Int32)"/> method.
      /// </remarks>
      [CLSCompliant( false )]
      public UInt32 TypeToken { get; set; }

      /// <summary>
      /// Gets or sets the <see cref="PDBSlotFlags"/> of this <see cref="PDBSlot"/>.
      /// </summary>
      /// <value>The <see cref="PDBSlotFlags"/> of this <see cref="PDBSlot"/>.</value>
      public PDBSlotFlags Flags { get; set; }

      //public Int32 Address { get; set; }

      /// <summary>
      /// Creates the textual representation of this <see cref="PDBSlot"/>.
      /// </summary>
      /// <returns>The textual representation of this <see cref="PDBSlot"/>.</returns>
      /// <remarks>
      /// The textual representation of this <see cref="PDBSlot"/> includes the values of the <see cref="PDBSlot.Name"/> and <see cref="TypeToken"/> properties.
      /// </remarks>
      public override String ToString()
      {
         return this.Name + " @" + String.Format( "{0:X8}", this.TypeToken );
      }
   }

   /// <summary>
   /// This enumeration holds flags for <see cref="PDBSlot"/>.
   /// </summary>
   [Flags]
   public enum PDBSlotFlags : short
   {
      /// <summary>
      /// TOOD documentation.
      /// </summary>
      IsParameter = 0x0001,

      /// <summary>
      /// TOOD documentation.
      /// </summary>
      AddressIsTaken = 0x0002,

      /// <summary>
      /// TOOD documentation.
      /// </summary>
      IsCompilerGenerated = 0x0004,

      /// <summary>
      /// TOOD documentation.
      /// </summary>
      IsAggregate = 0x0008,

      /// <summary>
      /// TOOD documentation.
      /// </summary>
      IsAggregated = 0x0010,

      /// <summary>
      /// TOOD documentation.
      /// </summary>
      IsAliased = 0x0020,

      /// <summary>
      /// TOOD documentation.
      /// </summary>
      IsAlias = 0x0040,
   }

   /// <summary>
   /// This class represents information about the <c>async</c> method, represented by <see cref="PDBFunction"/>.
   /// </summary>
   /// <seealso cref="PDBFunction"/>
   /// <seealso cref="PDBFunction.AsyncMethodInfo"/>
   public sealed class PDBAsyncMethodInfo
   {
      /// <summary>
      /// Creates a new instance of <see cref="PDBAsyncMethodInfo"/>.
      /// </summary>
      public PDBAsyncMethodInfo()
      {
         this.SynchronizationPoints = new List<PDBSynchronizationPoint>();
      }

      /// <summary>
      /// Gets or sets the token of the kick-off method.
      /// </summary>
      /// <value>The token of the kick-off method.</value>
      /// <remarks>
      /// The token should be transformable into <see cref="T:CILAssemblyManipulator.Physical.TableIndex"/> by <see cref="M:CILAssemblyManipulator.Physical.TableIndex.FromOneBasedToken(System.Int32)"/> method.
      /// </remarks>
      [CLSCompliant( false )]
      public UInt32 KickoffMethodToken { get; set; }

      /// <summary>
      /// Gets or sets the offset of the catch handler, in bytes.
      /// </summary>
      /// <value>The offset of the catch handler, in bytes.</value>
      public Int32 CatchHandlerOffset { get; set; }

      /// <summary>
      /// Gets the list of <see cref="PDBSynchronizationPoint"/>s of this <see cref="PDBAsyncMethodInfo"/>.
      /// </summary>
      /// <value>The list of <see cref="PDBSynchronizationPoint"/>s of this <see cref="PDBAsyncMethodInfo"/>.</value>
      public List<PDBSynchronizationPoint> SynchronizationPoints { get; }
   }

   /// <summary>
   /// The class contains information about single synchronization point of <see cref="PDBAsyncMethodInfo"/>.
   /// </summary>
   /// <seealso cref="PDBAsyncMethodInfo"/>
   /// <seealso cref="PDBAsyncMethodInfo.SynchronizationPoints"/>
   public sealed class PDBSynchronizationPoint
   {
      /// <summary>
      /// Gets or sets the offset for the synchronization point, in bytes.
      /// </summary>
      /// <value>The offset for the synchronization point, in bytes.</value>
      public Int32 SyncOffset { get; set; }

      /// <summary>
      /// Gets or sets the token of the continuation method.
      /// </summary>
      /// <value>The token of the continuation method.</value>
      /// <remarks>
      /// The token should be transformable into <see cref="T:CILAssemblyManipulator.Physical.TableIndex"/> by <see cref="M:CILAssemblyManipulator.Physical.TableIndex.FromOneBasedToken(System.Int32)"/> method.
      /// </remarks>
      [CLSCompliant( false )]
      public UInt32 ContinuationMethodToken { get; set; }

      /// <summary>
      /// Gets or sets the offset in the continuation method, of something.
      /// </summary>
      /// <value>The offset in the continuation method, of something.</value>
      public Int32 ContinuationOffset { get; set; }
   }

   /// <summary>
   /// This class represents a lexical scope of local variables of method.
   /// </summary>
   public sealed class PDBLocalScope
   {
      /// <summary>
      /// Gets or sets the offset, in bytes, where this <see cref="PDBLocalScope"/> starts.
      /// </summary>
      /// <value>The offset, in bytes, where this <see cref="PDBLocalScope"/> starts.</value>
      public Int32 Offset { get; set; }

      /// <summary>
      /// Gets or sets the length, in bytes, of this <see cref="PDBLocalScope"/>.
      /// </summary>
      /// <value>The length, in bytes, of this <see cref="PDBLocalScope"/>.</value>
      public Int32 Length { get; set; }

      /// <summary>
      /// Creates the textual representation of this <see cref="PDBLocalScope"/>.
      /// </summary>
      /// <returns>The textual representation of this <see cref="PDBLocalScope"/>.</returns>
      /// <remarks>
      /// The textual representation of this <see cref="PDBLocalScope"/> includes the values of the <see cref="Offset"/> and <see cref="Length"/> properties.
      /// </remarks>
      public override String ToString()
      {
         return String.Format( "IL_{0:X4} .. IL_{1:X4}", this.Offset, this.Offset + this.Length );
      }

   }

   /// <summary>
   /// This class represents information about source code fragment, related to some <see cref="PDBFunction"/>.
   /// </summary>
   public sealed class PDBLine
   {
      /// <summary>
      /// Gets or sets the <see cref="PDBSource"/> of this <see cref="PDBLine"/>.
      /// </summary>
      /// <value>The <see cref="PDBSource"/> of this <see cref="PDBLine"/>.</value>
      /// <remarks>The <see cref="PDBSource"/> basically represents a file where this line belongs to.</remarks>
      public PDBSource Source { get; set; }

      /// <summary>
      /// Gets or sets the IL byte offset this <see cref="PDBLine"/> is related to.
      /// </summary>
      /// <value>The IL byte offset this <see cref="PDBLine"/> is related to.</value>
      public Int32 Offset { get; set; }

      /// <summary>
      /// Gets or sets the inclusive line number where the source fragment starts.
      /// </summary>
      /// <value>The inclusive line number where the source fragment starts.</value>
      public Int32 LineStart { get; set; }

      /// <summary>
      /// Gets or sets the inclusive line number where the source fragment ends.
      /// </summary>
      /// <value>The inclusive line number where the source fragment ends.</value>
      public Int32 LineEnd { get; set; }

      /// <summary>
      /// Gets or sets the optional inclusive column number where the source fragment starts.
      /// </summary>
      /// <value>The optional inclusive column number where the source fragment starts.</value>
      [CLSCompliant( false )]
      public UInt16? ColumnStart { get; set; }

      /// <summary>
      /// Gets or sets the optional exclusive column number where the source fragment ends.
      /// </summary>
      /// <value>The optional exclusive column number where the source fragment ends.</value>
      [CLSCompliant( false )]
      public UInt16? ColumnEnd { get; set; }

      /// <summary>
      /// Gets or sets the value indicating whether source fragment is a statement.
      /// </summary>
      /// <value>The value indicating whether source fragment is a statement.</value>
      public Boolean IsStatement { get; set; }


      /// <summary>
      /// Creates the textual representation of this <see cref="PDBLine"/>.
      /// </summary>
      /// <returns>The textual representation of this <see cref="PDBLine"/>.</returns>
      /// <remarks>
      /// The textual representation of this <see cref="PDBLine"/> includes the values of the <see cref="Offset"/>, <see cref="Source"/>, <see cref="LineStart"/>, and <see cref="LineEnd"/> properties.
      /// </remarks>
      public override String ToString()
      {
         return String.Format( "IL_{0:X4}", this.Offset ) +
            " " + this.Source + ": " + " [" + this.LineStart + ( this.LineEnd == this.LineStart ? "" : ( "->" + this.LineEnd ) ) + "]";
      }
   }
}
