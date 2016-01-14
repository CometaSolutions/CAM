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
   public sealed class PDBInstance
   {
      private readonly IDictionary<String, PDBSource> _sources;
      private readonly IDictionary<String, PDBModule> _modules;
      private Guid _guid;
      private UInt32 _age;
      private String _sourceServer;

      public PDBInstance()
      {
         this._sources = new Dictionary<String, PDBSource>();
         this._modules = new Dictionary<String, PDBModule>();
      }

      public IEnumerable<PDBSource> Sources
      {
         get
         {
            return this._sources.Values;
         }
      }

      public IEnumerable<PDBModule> Modules
      {
         get
         {
            return this._modules.Values;
         }
      }

      public PDBSource GetOrAddSource( String sourceName )
      {
         return this._sources.GetOrAdd_NotThreadSafe( sourceName, srcN => new PDBSource( srcN ) );
      }

      public Boolean TryGetSource( String sourceName, out PDBSource source )
      {
         return this._sources.TryGetValue( sourceName, out source );
      }

      public Boolean TryAddSource( String sourceName, PDBSource source )
      {
         ArgumentValidator.ValidateNotNull( "Source", source );
         var result = !this._sources.ContainsKey( sourceName );
         if ( result )
         {
            this._sources.Add( sourceName, source );
         }
         return result;
      }

      public void AddSource( PDBSource source )
      {
         ArgumentValidator.ValidateNotNull( "Source", source );
         this._sources.Add( source.Name, source );
      }

      public PDBModule GetOrAddModule( String moduleName )
      {
         return this._modules.GetOrAdd_NotThreadSafe( moduleName, srcN => new PDBModule( srcN ) );
      }

      public Boolean TryGetModule( String moduleName, out PDBModule module )
      {
         return this._modules.TryGetValue( moduleName, out module );
      }

      public Boolean TryAddModule( PDBModule module )
      {
         ArgumentValidator.ValidateNotNull( "Module", module );
         var moduleName = module.Name;
         var result = !this._sources.ContainsKey( moduleName );
         if ( result )
         {
            this._modules.Add( moduleName, module );
         }
         return result;
      }

      public void AddModule( PDBModule module )
      {
         ArgumentValidator.ValidateNotNull( "Module", module );
         this._modules.Add( module.Name, module );
      }

      public Guid DebugGUID
      {
         get
         {
            return this._guid;
         }
         set
         {
            this._guid = value;
         }
      }

      [CLSCompliant( false )]
      public UInt32 Age
      {
         get
         {
            return this._age;
         }
         set
         {
            this._age = value;
         }
      }

      public String SourceServer
      {
         get
         {
            return this._sourceServer;
         }
         set
         {
            this._sourceServer = value;
         }
      }
   }

   public sealed class PDBSource
   {
      private readonly String _name;
      private Guid _documentType;
      private Guid _language;
      private Guid _vendor;
      private Guid _hashAlgorithm;
      private Byte[] _hash;

      internal PDBSource( String aName )
      {
         ArgumentValidator.ValidateNotNull( "Source name", aName );

         this._name = aName;
         this._documentType = Guid.Empty;
         this._language = Guid.Empty;
         this._vendor = Guid.Empty;
         this._hashAlgorithm = Guid.Empty;
      }

      public String Name
      {
         get
         {
            return this._name;
         }
      }

      public Guid DocumentType
      {
         get
         {
            return this._documentType;
         }
         set
         {
            this._documentType = value;
         }
      }

      public Guid Language
      {
         get
         {
            return this._language;
         }
         set
         {
            this._language = value;
         }
      }

      public Guid Vendor
      {
         get
         {
            return this._vendor;
         }
         set
         {
            this._vendor = value;
         }
      }

      public Guid HashAlgorithm
      {
         get
         {
            return this._hashAlgorithm;
         }
         set
         {
            this._hashAlgorithm = value;
         }
      }

      public Byte[] Hash
      {
         get
         {
            return this._hash;
         }
         set
         {
            this._hash = value;
         }
      }

      public override String ToString()
      {
         return this._name;
      }

      //public override Boolean Equals( Object obj )
      //{
      //   return Object.ReferenceEquals( this, obj ) || this.DoesEqual( obj as PDBSource );
      //}

      //private Boolean DoesEqual( PDBSource other )
      //{
      //   return other != null && String.Equals( this._name, other._name );
      //}

      //public override Int32 GetHashCode()
      //{
      //   return this._name == null ? 0 : this._name.GetHashCode();
      //}
   }

   public sealed class PDBModule
   {
      private readonly String _name;
      private readonly IList<PDBFunction> _functions;

      internal PDBModule( String name )
      {
         ArgumentValidator.ValidateNotNull( "Name", name );

         this._name = name;
         this._functions = new List<PDBFunction>();
      }

      public IList<PDBFunction> Functions
      {
         get
         {
            return this._functions;
         }
      }

      public String Name
      {
         get
         {
            return this._name;
         }
      }

      public override String ToString()
      {
         return this._name;
      }
   }

   public abstract class PDBScopeOrFunction
   {
      //private readonly IList<PDBConstant> constants;
      private readonly IList<PDBSlot> slots;
      private readonly IList<PDBScope> scopes;
      private readonly IList<String> usedNamespaces;
      private String name;
      private Int32 _length;

      internal PDBScopeOrFunction()
         : this( /*new List<PDBConstant>(),*/ new List<PDBSlot>(), new List<PDBScope>(), new List<String>() )
      {

      }

      private PDBScopeOrFunction( /*IList<PDBConstant> consts, */IList<PDBSlot> sls, IList<PDBScope> scps, IList<String> un )
      {
         //this.constants = consts;
         this.slots = sls;
         this.scopes = scps;
         this.usedNamespaces = un;
      }

      //public IList<PDBConstant> Constants
      //{
      //   get
      //   {
      //      return this.constants;
      //   }
      //}

      public IList<PDBSlot> Slots
      {
         get
         {
            return this.slots;
         }
      }

      public IList<PDBScope> Scopes
      {
         get
         {
            return this.scopes;
         }
      }

      public IList<String> UsedNamespaces
      {
         get
         {
            return this.usedNamespaces;
         }
      }

      public String Name
      {
         get
         {
            return this.name;
         }
         set
         {
            this.name = value;
         }
      }

      public Int32 Length
      {
         get
         {
            return this._length;
         }
         set
         {
            this._length = value;
         }
      }

      public override String ToString()
      {
         return this.name;
      }
   }

   public sealed class PDBFunction : PDBScopeOrFunction
   {



      //internal Int32 parent;
      //internal Int32 end;
      //internal Int32 next;

      //internal Int32 debugStart;
      //internal Int32 debugEnd;
      private UInt32 _token;
      //internal Int32 _offset;
      //internal UInt16 _segment;
      //private Byte _flags;
      //internal UInt16 returnReg;

      private List<UInt16> _usingCounts;
      private UInt32 _forwardingMethodToken;
      private UInt32 _moduleForwardingMethodToken;
      private readonly List<PDBLocalScope> _localScopes;
      private String _iteratorClass;
      private UInt32 _encID;

      private readonly IDictionary<String, IList<PDBLine>> _lineInfo;

      private PDBAsyncMethodInfo _asyncMethodInfo;

      internal PDBFunction()
      {
         this._localScopes = new List<PDBLocalScope>();
         this._lineInfo = new Dictionary<String, IList<PDBLine>>();
         this._usingCounts = new List<UInt16>();
      }

      public IDictionary<String, IList<PDBLine>> Lines
      {
         get
         {
            return this._lineInfo;
         }
      }

      [CLSCompliant( false )]
      public UInt32 Token
      {
         get
         {
            return this._token;
         }
         set
         {
            this._token = value;
         }
      }

      public PDBAsyncMethodInfo AsyncMethodInfo
      {
         get
         {
            return this._asyncMethodInfo;
         }
         set
         {
            this._asyncMethodInfo = value;
         }
      }

      [CLSCompliant( false )]
      public UInt32 ENCID // TODO the heck is this...
      {
         get
         {
            return this._encID;
         }
         set
         {
            this._encID = value;
         }
      }

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
      public UInt32 ForwardingMethodToken
      {
         get
         {
            return this._forwardingMethodToken;
         }
         set
         {
            this._forwardingMethodToken = value;
         }
      }

      public List<PDBLocalScope> LocalScopes
      {
         get
         {
            return this._localScopes;
         }
      }

      public String IteratorClass
      {
         get
         {
            return this._iteratorClass;
         }
         set
         {
            this._iteratorClass = value;
         }
      }

      [CLSCompliant( false )]
      public List<UInt16> UsingCounts
      {
         get
         {
            return this._usingCounts;
         }
      }

      [CLSCompliant( false )]
      public UInt32 ModuleForwardingMethodToken
      {
         get
         {
            return this._moduleForwardingMethodToken;
         }
         set
         {
            this._moduleForwardingMethodToken = value;
         }
      }
   }

   public sealed class PDBScope : PDBScopeOrFunction
   {
      private Int32 _offset;

      public PDBScope( String name )
      {
         this.Name = name ?? String.Empty;
      }

      //internal PDBScope( PDBFunction scope )
      //   : base( scope.constants, scope.slots, scope.scopes, scope.usedNamespaces )
      //{

      //}

      public Int32 Offset
      {
         get
         {
            return this._offset;
         }
         set
         {
            this._offset = value;
         }
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
      private Int32 _slot;
      private UInt32 _typeToken;
      private String _name;
      private PDBSlotFlags _flags;
      private Int32 _address;

      public PDBSlot()
      {

      }

      public override String ToString()
      {
         return this._name;
      }

      public String Name
      {
         get
         {
            return this._name;
         }
         set
         {
            this._name = value;
         }
      }

      public Int32 SlotIndex
      {
         get
         {
            return this._slot;
         }
         set
         {
            this._slot = value;
         }
      }

      [CLSCompliant( false )]
      public UInt32 TypeToken
      {
         get
         {
            return this._typeToken;
         }
         set
         {
            this._typeToken = value;
         }
      }

      [CLSCompliant( false )]
      public PDBSlotFlags Flags
      {
         get
         {
            return this._flags;
         }
         set
         {
            this._flags = value;
         }
      }

      public Int32 Address
      {
         get
         {
            return this._address;
         }
         set
         {
            this._address = value;
         }
      }
   }

   [Flags, CLSCompliant( false )]
   public enum PDBSlotFlags : ushort
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
      private UInt32 _kickoffMethodToken;
      private Int32 _catchHandlerOffset;
      private IList<PDBSynchronizationPoint> _syncPoints;

      [CLSCompliant( false )]
      public PDBAsyncMethodInfo( UInt32 kickoffMethodToken, Int32 catchHandlerOffset )
      {
         this._kickoffMethodToken = kickoffMethodToken;
         this._catchHandlerOffset = catchHandlerOffset;
         this._syncPoints = new List<PDBSynchronizationPoint>();
      }

      [CLSCompliant( false )]
      public UInt32 KickoffMethodToken
      {
         get
         {
            return this._kickoffMethodToken;
         }
         set
         {
            this._kickoffMethodToken = value;
         }
      }

      public Int32 CatchHandlerOffset
      {
         get
         {
            return this._catchHandlerOffset;
         }
         set
         {
            this._catchHandlerOffset = value;
         }
      }

      public IList<PDBSynchronizationPoint> SynchronizationPoints
      {
         get
         {
            return this._syncPoints;
         }
      }
   }

   public sealed class PDBSynchronizationPoint
   {
      private Int32 _syncOffset;
      private UInt32 _continuationMethodToken;
      private Int32 _continuationOffset;

      [CLSCompliant( false )]
      public PDBSynchronizationPoint( Int32 syncOffset, UInt32 continuationMethodToken, Int32 continuationOffset )
      {
         this._syncOffset = syncOffset;
         this._continuationMethodToken = continuationMethodToken;
         this._continuationOffset = continuationOffset;
      }

      public Int32 SyncOffset
      {
         get
         {
            return this._syncOffset;
         }
         set
         {
            this._syncOffset = value;
         }
      }

      [CLSCompliant( false )]
      public UInt32 ContinuationMethodToken
      {
         get
         {
            return this._continuationMethodToken;
         }
         set
         {
            this._continuationMethodToken = value;
         }
      }

      public Int32 ContinuationOffset
      {
         get
         {
            return this._continuationOffset;
         }
         set
         {
            this._continuationOffset = value;
         }
      }
   }

   public sealed class PDBLocalScope
   {
      private Int32 _offset;
      private Int32 _length;

      public PDBLocalScope( Int32 offset, Int32 length )
      {
         this._offset = offset;
         this._length = length;
      }

      public override String ToString()
      {
         return String.Format( "IL_{0:X4} .. IL_{1:X4}", this._offset, this._offset + this._length );
      }

      public Int32 Offset
      {
         get
         {
            return this._offset;
         }
         set
         {
            this._offset = value;
         }
      }

      public Int32 Length
      {
         get
         {
            return this._length;
         }
         set
         {
            this._length = value;
         }
      }
   }

   public sealed class PDBLine
   {
      private readonly Int32 _offset;
      private Int32 _lineStart;
      private Int32 _lineEnd;
      private UInt16? _colStart;
      private UInt16? _colEnd;
      private Boolean _isStatement;

      public PDBLine( Int32 anOffset )
      {
         this._offset = anOffset;
      }

      //public override Boolean Equals( Object obj )
      //{
      //   return Object.ReferenceEquals( this, obj ) || this.DoesEqual( obj as PDBLine );
      //}

      //private Boolean DoesEqual( PDBLine line )
      //{
      //   return line != null
      //      && this._offset == line._offset
      //      && this._lineStart == line._lineStart
      //      && this._lineEnd == line._lineEnd
      //      && this._colStart == line._colStart
      //      && this._colEnd == line._colEnd;
      //}

      //public override Int32 GetHashCode()
      //{
      //   return this._offset.GetHashCode();
      //}

      public override String ToString()
      {
         return String.Format( "IL_{0:X4}", this._offset ) +
            " [" + this._lineStart + ( this._lineEnd == this._lineStart ? "" : ( "->" + this._lineEnd ) ) + "]";
      }

      public Int32 Offset
      {
         get
         {
            return this._offset;
         }
      }

      public Int32 LineStart
      {
         get
         {
            return this._lineStart;
         }
         set
         {
            this._lineStart = value;
         }
      }

      public Int32 LineEnd
      {
         get
         {
            return this._lineEnd;
         }
         set
         {
            this._lineEnd = value;
         }
      }

      [CLSCompliant( false )]
      public UInt16? ColumnStart
      {
         get
         {
            return this._colStart;
         }
         set
         {
            this._colStart = value;
         }
      }

      [CLSCompliant( false )]
      public UInt16? ColumnEnd
      {
         get
         {
            return this._colEnd;
         }
         set
         {
            this._colEnd = value;
         }
      }

      public Boolean IsStatement
      {
         get
         {
            return this._isStatement;
         }
         set
         {
            this._isStatement = value;
         }
      }
   }
}
