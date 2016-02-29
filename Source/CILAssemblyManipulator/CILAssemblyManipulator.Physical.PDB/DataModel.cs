/*
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
         this.Sources = new Dictionary<String, PDBSource>();
         this.Modules = new Dictionary<String, PDBModule>();
      }

      /// <summary>
      /// Gets the dictionary of <see cref="PDBSource"/> objects for this <see cref="PDBInstance"/>.
      /// </summary>
      /// <value>The dictionary of <see cref="PDBSource"/> objects for this <see cref="PDBInstance"/>.</value>
      /// <remarks>
      /// The key of the dictionary is the name of the associated <see cref="PDBSource"/>.
      /// </remarks>
      public IDictionary<String, PDBSource> Sources { get; }

      /// <summary>
      /// Gets the dictionary of <see cref="PDBModule"/> objects for this <see cref="PDBInstance"/>.
      /// </summary>
      /// <value>The dictionary of <see cref="PDBModule"/> objects for this <see cref="PDBInstance"/>.</value>
      /// <remarks>
      /// The key of the dictionary is the name of the associated <see cref="PDBModule"/>.
      /// </remarks>
      public IDictionary<String, PDBModule> Modules { get; }

      /// <summary>
      /// Gets or sets the unique identifier of this <see cref="PDBInstance"/>.
      /// </summary>
      /// <value>The unique identifier of this <see cref="PDBInstance"/>.</value>
      /// <remarks>
      /// This <see cref="Guid"/> should match the one serialized in <see cref="P:CILAssemblyManipulator.Physical.DebugInformation.DebugData"/>.
      /// </remarks>
      public Guid DebugGUID { get; set; }

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

   public sealed class PDBSource
   {
      public PDBSource()
      {
         this.DocumentType = Guid.Empty;
         this.Language = Guid.Empty;
         this.Vendor = Guid.Empty;
         this.HashAlgorithm = Guid.Empty;
      }

      public Guid DocumentType { get; set; }

      public Guid Language { get; set; }

      public Guid Vendor { get; set; }

      public Guid HashAlgorithm { get; set; }

      public Byte[] Hash { get; set; }
   }

   public sealed class PDBModule
   {

      public PDBModule()
      {
         this.Functions = new List<PDBFunction>();
      }

      public List<PDBFunction> Functions { get; }
   }

   public abstract class PDBScopeOrFunction
   {
      internal PDBScopeOrFunction()
      {
         this.Slots = new List<PDBSlot>();
         this.Scopes = new List<PDBScope>();
         this.UsedNamespaces = new List<String>();
      }

      public IList<PDBSlot> Slots { get; }

      public IList<PDBScope> Scopes { get; }

      public IList<String> UsedNamespaces { get; }

      public String Name { get; set; }

      public Int32 Length { get; set; }

   }

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


      internal PDBFunction()
      {
         this.LocalScopes = new List<PDBLocalScope>();
         this.Lines = new Dictionary<String, IList<PDBLine>>();
         this.UsingCounts = new List<UInt16>();
      }

      public IDictionary<String, IList<PDBLine>> Lines { get; }

      [CLSCompliant( false )]
      public UInt32 Token { get; set; }

      public PDBAsyncMethodInfo AsyncMethodInfo { get; set; }

      [CLSCompliant( false )]
      public UInt32 ENCID { get; set; }

      //public Byte Flags // TODO is this used *only* when serializing/deserializing?
      //{
      //   get
      //   {
      //      return this._flags;
      //   }
      //   set
      //   {
      //      this._flags = value;
      //   }
      //}

      [CLSCompliant( false )]
      public UInt32 ForwardingMethodToken { get; set; }

      public List<PDBLocalScope> LocalScopes { get; }

      public String IteratorClass { get; set; }

      [CLSCompliant( false )]
      public List<UInt16> UsingCounts { get; }

      [CLSCompliant( false )]
      public UInt32 ModuleForwardingMethodToken { get; set; }

      public override String ToString()
      {
         return "Function " + this.Name + " @" + this.Token;
      }
   }

   public sealed class PDBScope : PDBScopeOrFunction
   {

      public Int32 Offset { get; set; }

      public override String ToString()
      {
         return "Scope " + this.Name + " @" + this.Offset;
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

   public sealed class PDBSlot
   {

      public String Name { get; set; }

      public Int32 SlotIndex { get; set; }

      [CLSCompliant( false )]
      public UInt32 TypeToken { get; set; }

      public PDBSlotFlags Flags { get; set; }

      public Int32 Address { get; set; }

      public override String ToString()
      {
         return this.Name;
      }
   }

   [Flags]
   public enum PDBSlotFlags : short
   {
      IsParameter = 0x0001,
      AddressIsTaken = 0x0002,
      IsCompilerGenerated = 0x0004,
      IsAggregate = 0x0008,
      IsAggregated = 0x0010,
      IsAliased = 0x0020,
      IsAlias = 0x0040,
   }

   public sealed class PDBAsyncMethodInfo
   {
      public PDBAsyncMethodInfo()
      {
         this.SynchronizationPoints = new List<PDBSynchronizationPoint>();
      }

      [CLSCompliant( false )]
      public UInt32 KickoffMethodToken { get; set; }

      public Int32 CatchHandlerOffset { get; set; }

      public IList<PDBSynchronizationPoint> SynchronizationPoints { get; }
   }

   public sealed class PDBSynchronizationPoint
   {
      public Int32 SyncOffset { get; set; }

      [CLSCompliant( false )]
      public UInt32 ContinuationMethodToken { get; set; }
      public Int32 ContinuationOffset { get; set; }
   }

   public sealed class PDBLocalScope
   {
      public Int32 Offset { get; set; }

      public Int32 Length { get; set; }

      public override String ToString()
      {
         return String.Format( "IL_{0:X4} .. IL_{1:X4}", this.Offset, this.Offset + this.Length );
      }

   }

   public sealed class PDBLine
   {


      // IL Byte offset
      public Int32 Offset { get; set; }

      public Int32 LineStart { get; set; }

      public Int32 LineEnd { get; set; }

      [CLSCompliant( false )]
      public UInt16? ColumnStart { get; set; }

      [CLSCompliant( false )]
      public UInt16? ColumnEnd { get; set; }
      public Boolean IsStatement { get; set; }

      public override String ToString()
      {
         return String.Format( "IL_{0:X4}", this.Offset ) +
            " [" + this.LineStart + ( this.LineEnd == this.LineStart ? "" : ( "->" + this.LineEnd ) ) + "]";
      }
   }
}
