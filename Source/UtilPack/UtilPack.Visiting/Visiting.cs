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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilPack.CollectionsWithRoles;
using UtilPack.Visiting;
using UtilPack;

namespace UtilPack.Visiting
{
#pragma warning disable 1591


   /// <summary>
   /// This delegate contains signature required for callbacks which perform visiting of hierarchically contained items according to visitor-pattern.
   /// </summary>
   /// <typeparam name="TElement">The common type for elements.</typeparam>
   /// <typeparam name="TEdgeInfo">The type for edge information (e.g. list index).</typeparam>
   /// <param name="element">The current element.</param>
   /// <param name="callback">The callback to call for all other elements hierarchically contained by given <paramref name="element"/>.</param>
   /// <returns><c>true</c> if should continue visiting; <c>false</c> otherwise.</returns>
   public delegate Boolean VisitElementDelegate<TElement, TEdgeInfo>( TElement element, VisitElementCallbackDelegate<TElement, TEdgeInfo> callback );

   /// <summary>
   /// This is helper delegate for callbacks which want automatic casting to be performed.
   /// </summary>
   /// <typeparam name="TElement">The common type for elements.</typeparam>
   /// <typeparam name="TEdgeInfo">The type for edge information (e.g. list index).</typeparam>
   /// <typeparam name="TActualElement">The casted type for elements..</typeparam>
   /// <param name="element">The current element.</param>
   /// <param name="callback">The callback to call for all other elements hierarchically contained by given <paramref name="element"/>.</param>
   /// <returns><c>true</c> if should continue visiting; <c>false</c> otherwise.</returns>
   /// <seealso cref="M:E_UtilPack.AsVisitElementDelegate"/>
   public delegate Boolean VisitElementDelegateTyped<TElement, TEdgeInfo, in TActualElement>( TActualElement element, VisitElementCallbackDelegate<TElement, TEdgeInfo> callback )
      where TActualElement : TElement;

   /// <summary>
   /// This is delegate that should be called by methods called by <see cref="VisitElementDelegate{TElement, TEdgeInfo}"/>. It will take care to visit the given element according to the visitor pattern.
   /// </summary>
   /// <typeparam name="TElement">The common type for elements.</typeparam>
   /// <typeparam name="TEdgeInfo">The type for edge information (e.g. list index).</typeparam>
   /// <param name="element">The element that should be visited.</param>
   /// <param name="edgeID">The <see cref="VisitorEdgeInfo{TElement, TEdgeInfo}.ID"/> of the edge.</param>
   /// <param name="edgeInfo">The edge-specific info object (e.g. list index).</param>
   /// <param name="overrideType">If parameter is supplied, then this type is used to lookup the visitor functionality for given <paramref name="element"/>, instead of <see cref="Object.GetType"/>.</param>
   /// <returns><c>true</c> if should continue visiting; <c>false</c> otherwise.</returns>
   /// <seealso cref="TypeBasedVisitor{TElement, TEdgeInfo}"/>
   public delegate Boolean VisitElementCallbackDelegate<in TElement, in TEdgeInfo>( TElement element, Int32 edgeID, TEdgeInfo edgeInfo, Type overrideType = null );


   public class VisitorVertexInfo<TElement, TEdgeInfo>
   {
      internal VisitorVertexInfo( Type type, IEnumerable<VisitorEdgeInfo<TElement, TEdgeInfo>> edges )
      {
         this.Type = ArgumentValidator.ValidateNotNull( "Type", type );
         this.Edges = ( edges ?? Empty<VisitorEdgeInfo<TElement, TEdgeInfo>>.Enumerable ).Where( e => e != null ).ToArrayProxy().CQ;
      }
      public Type Type { get; }

      public ArrayQuery<VisitorEdgeInfo<TElement, TEdgeInfo>> Edges { get; }
   }

   public class VisitorEdgeInfo<TElement, TEdgeInfo>
   {

      internal VisitorEdgeInfo( Int32 id, Type type, String name, VisitElementDelegate<TElement, TEdgeInfo> del )
      {
         this.ID = id;
         this.Type = ArgumentValidator.ValidateNotNull( "Type", type );
         this.Name = name;
         this.Delegate = ArgumentValidator.ValidateNotNull( "Delegate", del );
      }

      public Int32 ID { get; }

      public Type Type { get; }

      public String Name { get; }

      public VisitElementDelegate<TElement, TEdgeInfo> Delegate { get; }


   }

   public class VisitorVertexInfoFactory<TElement, TEdgeInfo> : AbstractDisposable
   {
      private readonly TypeBasedVisitor<TElement, TEdgeInfo> _visitor;
      private readonly List<VisitorEdgeInfo<TElement, TEdgeInfo>> _edges;
      private readonly Type _vertexType;

      public VisitorVertexInfoFactory( TypeBasedVisitor<TElement, TEdgeInfo> visitor, Type vertexType )
      {
         this._visitor = ArgumentValidator.ValidateNotNull( "Visitor", visitor );
         this._vertexType = ArgumentValidator.ValidateNotNull( "Vertex type", vertexType );
         this._edges = new List<VisitorEdgeInfo<TElement, TEdgeInfo>>();
      }

      public VisitorEdgeInfo<TElement, TEdgeInfo> AddEdge( Type type, String name, Func<Int32, VisitElementDelegate<TElement, TEdgeInfo>> delegateCreator )
      {
         var id = this._visitor.GetNextEdgeID();
         var retVal = new VisitorEdgeInfo<TElement, TEdgeInfo>( id, type, name, delegateCreator( id ) );
         this._edges.Add( retVal );
         return retVal;
      }

      protected override void Dispose( Boolean disposing )
      {
         if ( disposing )
         {
            this._visitor.RegisterVisitor( new VisitorVertexInfo<TElement, TEdgeInfo>( this._vertexType, this._edges ) );
         }
      }
   }

   public abstract class ExplicitTypeBasedVisitor<TElement>
   {
      public ExplicitVisitorInformation<TElement, TContext> CreateExplicitVisitorInfo<TContext>( ExplicitAcceptorInformation<TElement, TContext> acceptorInfo, TContext context )
      {
         return new ExplicitVisitorInformation<TElement, TContext>( this.CreateCallback( acceptorInfo, context ), acceptorInfo, context );
      }

      public Boolean VisitExplicit<TContext>(
         TElement element,
         ExplicitVisitorInformation<TElement, TContext> visitorInfo
         )
      {
         return this.VisitElementWithContext( element, visitorInfo.AcceptorInformation, visitorInfo.Context, visitorInfo.Callback, null );
      }


      // TODO parametrize what to return when vertex type not found.
      private Boolean VisitElementWithContext<TContext>(
         TElement element,
         ExplicitAcceptorInformation<TElement, TContext> acceptorInfo,
         TContext context,
         AcceptVertexExplicitCallbackDelegate<TElement, TContext> acceptorCallback,
         Type overrideType
         )
      {
         //AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext> edgeDelegateInfo = null;
         AcceptVertexExplicitDelegate<TElement, TContext> vertexAcceptor;
         Boolean hadAcceptor;
         return element != null
            //&& ( ( edgeDelegateInfo = acceptorInfo.EdgeAcceptors.GetElementOrDefault( edgeID ) )?.Entry?.Invoke( element, edgeInfo, context ) ?? true )
            && this.CheckForTopMostTypeStrategy( element, acceptorInfo, context, overrideType, acceptorCallback )
            && ( ( vertexAcceptor = acceptorInfo.VertexAcceptors.TryGetValue( overrideType ?? element.GetType(), out hadAcceptor ) ) == null || vertexAcceptor( element, context, acceptorCallback ) ) //( ( ( vertexAcceptor = acceptors.VertexAcceptors.TryGetValue( overrideType ?? element.GetType(), out hadAcceptor ) ) != null && vertexAcceptor( element, context, acceptorCallback ) ) || ( vertexAcceptor == null && acceptors.ContinueOnMissingVertex ) )

            // && this.VisitEdges( element, overrideType ?? element.GetType(), callback )
            //&& ( edgeDelegateInfo?.Exit( element, edgeInfo, context ) ?? true )
            ;
      }

      private Boolean CheckForTopMostTypeStrategy<TContext>(
         TElement element,
         ExplicitAcceptorInformation<TElement, TContext> info,
         TContext context,
         Type overrideType,
         AcceptVertexExplicitCallbackDelegate<TElement, TContext> callback
         )
      {
         Boolean hadAcceptor;
         AcceptVertexExplicitDelegate<TElement, TContext> acceptor;
         var acceptorDictionary = info.VertexAcceptors;
         switch ( info.TopMostVisitingStrategy )
         {
            case TopMostTypeVisitingStrategy.IfNotOverridingType:
               return overrideType != null
                  || ( acceptor = acceptorDictionary.TryGetValue( typeof( TElement ), out hadAcceptor ) ) == null
                  || acceptor( element, context, callback );

            case TopMostTypeVisitingStrategy.Always:
               return ( acceptor = acceptorDictionary.TryGetValue( typeof( TElement ), out hadAcceptor ) ) == null
                  || acceptor( element, context, callback );
            default:
               return true;
         }
      }

      private AcceptVertexExplicitCallbackDelegate<TElement, TContext> CreateCallback<TContext>( ExplicitAcceptorInformation<TElement, TContext> acceptorInfo, TContext context )
      {
         AcceptVertexExplicitCallbackDelegate<TElement, TContext> callback = null;
         callback = ( el, ctx ) => el == null || this.VisitElementWithContext( el, acceptorInfo, context, callback, null );
         return callback;
      }
   }

   public class TypeBasedVisitor<TElement, TEdgeInfo> : ExplicitTypeBasedVisitor<TElement>
   {

      private Int32 _currentEdgeID;

      public TypeBasedVisitor()
      {
         this._currentEdgeID = 0;
         this.VerticeInfos = new Dictionary<Type, VisitorVertexInfo<TElement, TEdgeInfo>>();
         this.EdgeInfoLookup = new Dictionary<Type, Dictionary<String, VisitorEdgeInfo<TElement, TEdgeInfo>>>();
      }

      private Dictionary<Type, VisitorVertexInfo<TElement, TEdgeInfo>> VerticeInfos { get; }

      private Dictionary<Type, Dictionary<String, VisitorEdgeInfo<TElement, TEdgeInfo>>> EdgeInfoLookup { get; }

      public VisitorEdgeInfo<TElement, TEdgeInfo> TryGetEdgeInfo( Type type, String name, out Boolean success )
      {
         Dictionary<String, VisitorEdgeInfo<TElement, TEdgeInfo>> inner;
         VisitorEdgeInfo<TElement, TEdgeInfo> info = null;
         success = this.EdgeInfoLookup.TryGetValue( type, out inner )
            && inner.TryGetValue( name, out info );
         return info;
      }

      public VisitorVertexInfoFactory<TElement, TEdgeInfo> CreateVertexInfoFactory( Type vertexType )
      {
         return new VisitorVertexInfoFactory<TElement, TEdgeInfo>( this, vertexType );
      }


      internal void RegisterVisitor( VisitorVertexInfo<TElement, TEdgeInfo> visitor )
      {
         this.VerticeInfos[visitor.Type] = ArgumentValidator.ValidateNotNull( "Visitor", visitor );
         foreach ( var edge in visitor.Edges )
         {
            // Skip parent-type edges.
            if ( edge.Name != null )
            {
               this.EdgeInfoLookup
                  .GetOrAdd_NotThreadSafe( edge.Type, t => new Dictionary<String, VisitorEdgeInfo<TElement, TEdgeInfo>>() )
                  .Add( edge.Name, edge );
            }
         }
      }

      internal Int32 GetNextEdgeID()
      {
         return System.Threading.Interlocked.Increment( ref this._currentEdgeID ) - 1;
      }

      internal VisitorInformation<TElement, TEdgeInfo> CreateVisitorInfo( AcceptorInformation<TElement, TEdgeInfo> acceptorInfo )
      {
         return new VisitorInformation<TElement, TEdgeInfo>( this.CreateCallback( acceptorInfo ), acceptorInfo );
      }

      internal VisitorInformation<TElement, TEdgeInfo, TContext> CreateVisitorInfo<TContext>( AcceptorInformation<TElement, TEdgeInfo, TContext> acceptorInfo, TContext context )
      {
         return new VisitorInformation<TElement, TEdgeInfo, TContext>( this.CreateCallback( acceptorInfo, context ), acceptorInfo, context );
      }

      private VisitElementCallbackDelegate<TElement, TEdgeInfo> CreateCallback<TContext>( AcceptorInformation<TElement, TEdgeInfo, TContext> acceptorInfo, TContext context )
      {
         VisitElementCallbackDelegate<TElement, TEdgeInfo> callback = null;
         callback = ( el, edgeID, edgeInfo, type ) => this.VisitElementWithContext( el, acceptorInfo, context, callback, edgeID, edgeInfo, type );
         return callback;
      }

      private VisitElementCallbackDelegate<TElement, TEdgeInfo> CreateCallback( AcceptorInformation<TElement, TEdgeInfo> acceptorInfo )
      {
         VisitElementCallbackDelegate<TElement, TEdgeInfo> callback = null;
         callback = ( el, edgeID, edgeInfo, type ) => this.VisitElementWithNoContext( el, acceptorInfo, callback, edgeID, edgeInfo, type );
         return callback;
      }

      public Boolean Visit(
         TElement element,
         VisitorInformation<TElement, TEdgeInfo> visitorInfo
         )
      {
         return this.VisitElementWithNoContext( element, visitorInfo.AcceptorInformation, visitorInfo.Callback, -1, default( TEdgeInfo ), null );
      }

      public Boolean Visit<TContext>(
         TElement element,
         VisitorInformation<TElement, TEdgeInfo, TContext> visitorInfo
         )
      {
         return this.VisitElementWithContext( element, visitorInfo.AcceptorInformation, visitorInfo.Context, visitorInfo.Callback, -1, default( TEdgeInfo ), null );
      }



      private Boolean VisitElementWithNoContext(
         TElement element,
         AcceptorInformation<TElement, TEdgeInfo> acceptorInfo,
         VisitElementCallbackDelegate<TElement, TEdgeInfo> callback,
         Int32 edgeID,
         TEdgeInfo edgeInfo,
         Type overrideType
         )
      {
         AcceptEdgeDelegateInformation<TElement, TEdgeInfo> edgeDelegateInfo = null;
         AcceptVertexDelegate<TElement> vertexAcceptor;
         Boolean hadAcceptor;
         return element != null
            && ( ( edgeDelegateInfo = acceptorInfo.EdgeAcceptors.GetElementOrDefault( edgeID ) )?.Entry?.Invoke( element, edgeInfo ) ?? true )
            && this.CheckForTopMostTypeStrategy( element, acceptorInfo, overrideType )
            && ( ( ( vertexAcceptor = acceptorInfo.VertexAcceptors.TryGetValue( overrideType ?? element.GetType(), out hadAcceptor ) ) != null && vertexAcceptor( element ) ) || ( vertexAcceptor == null && acceptorInfo.ContinueOnMissingVertex ) )
            && this.VisitEdges( element, overrideType ?? element.GetType(), callback )
            && ( edgeDelegateInfo?.Exit( element, edgeInfo ) ?? true );
      }

      private Boolean VisitElementWithContext<TContext>(
         TElement element,
         AcceptorInformation<TElement, TEdgeInfo, TContext> acceptorInfo,
         TContext context,
         VisitElementCallbackDelegate<TElement, TEdgeInfo> callback,
         Int32 edgeID,
         TEdgeInfo edgeInfo,
         Type overrideType
         )
      {
         AcceptEdgeDelegateInformation<TElement, TEdgeInfo, TContext> edgeDelegateInfo = null;
         AcceptVertexDelegate<TElement, TContext> vertexAcceptor;
         Boolean hadAcceptor;
         return element != null
            && ( ( edgeDelegateInfo = acceptorInfo.EdgeAcceptors.GetElementOrDefault( edgeID ) )?.Entry?.Invoke( element, edgeInfo, context ) ?? true )
            && this.CheckForTopMostTypeStrategy( element, acceptorInfo, context, overrideType )
            && ( ( vertexAcceptor = acceptorInfo.VertexAcceptors.TryGetValue( overrideType ?? element.GetType(), out hadAcceptor ) ) == null || vertexAcceptor( element, context ) ) //( ( ( vertexAcceptor = acceptors.VertexAcceptors.TryGetValue( overrideType ?? element.GetType(), out hadAcceptor ) ) != null && vertexAcceptor( element, context ) ) || ( vertexAcceptor == null && acceptors.ContinueOnMissingVertex ) )
            && this.VisitEdges( element, overrideType ?? element.GetType(), callback )
            && ( edgeDelegateInfo?.Exit( element, edgeInfo, context ) ?? true );
      }

      private Boolean VisitEdges( TElement element, Type type, VisitElementCallbackDelegate<TElement, TEdgeInfo> callback )
      {
         VisitorVertexInfo<TElement, TEdgeInfo> info;
         var retVal = true;
         if ( this.VerticeInfos.TryGetValue( type, out info ) )
         {
            var edges = info.Edges;
            for ( var i = 0; i < edges.Count && retVal; ++i )
            {
               retVal = edges[i].Delegate( element, callback );
            }
         }
         return retVal;
      }

      private Boolean CheckForTopMostTypeStrategy(
         TElement element,
         AcceptorInformation<TElement, TEdgeInfo> info,
         Type overrideType
         )
      {
         Boolean hadAcceptor;
         AcceptVertexDelegate<TElement> acceptor;
         var acceptorDictionary = info.VertexAcceptors;
         switch ( info.TopMostVisitingStrategy )
         {
            case TopMostTypeVisitingStrategy.IfNotOverridingType:
               return overrideType != null
                  || ( acceptor = acceptorDictionary.TryGetValue( typeof( TElement ), out hadAcceptor ) ) == null
                  || acceptor( element );

            case TopMostTypeVisitingStrategy.Always:
               return ( acceptor = acceptorDictionary.TryGetValue( typeof( TElement ), out hadAcceptor ) ) == null
                  || acceptor( element );
            default:
               return true;
         }
      }

      private Boolean CheckForTopMostTypeStrategy<TContext>(
         TElement element,
         AcceptorInformation<TElement, TEdgeInfo, TContext> info,
         TContext context,
         Type overrideType
         )
      {
         Boolean hadAcceptor;
         AcceptVertexDelegate<TElement, TContext> acceptor;
         var acceptorDictionary = info.VertexAcceptors;
         switch ( info.TopMostVisitingStrategy )
         {
            case TopMostTypeVisitingStrategy.IfNotOverridingType:
               return overrideType != null
                  || ( acceptor = acceptorDictionary.TryGetValue( typeof( TElement ), out hadAcceptor ) ) == null
                  || acceptor( element, context );

            case TopMostTypeVisitingStrategy.Always:
               return ( acceptor = acceptorDictionary.TryGetValue( typeof( TElement ), out hadAcceptor ) ) == null
                  || acceptor( element, context );
            default:
               return true;
         }
      }
   }

   public abstract class AbstractVisitorInformation<TDelegate, TAcceptorInfo>
      where TDelegate : class
      where TAcceptorInfo : class
   {
      public AbstractVisitorInformation( TDelegate callback, TAcceptorInfo acceptorInfo )
      {
         this.Callback = ArgumentValidator.ValidateNotNull( "Callback", callback );
         this.AcceptorInformation = ArgumentValidator.ValidateNotNull( "Acceptor info", acceptorInfo );
      }

      public TDelegate Callback { get; }

      public TAcceptorInfo AcceptorInformation { get; }
   }

   public abstract class AbstractVisitorInformation<TDelegate, TAcceptorInfo, TContext> : AbstractVisitorInformation<TDelegate, TAcceptorInfo>
      where TDelegate : class
      where TAcceptorInfo : class
   {

      public AbstractVisitorInformation( TDelegate callback, TAcceptorInfo acceptorInfo, TContext context )
         : base( callback, acceptorInfo )
      {
         this.Context = context;
      }

      public TContext Context { get; }

   }

   public class VisitorInformation<TElement, TEdgeInfo> : AbstractVisitorInformation<VisitElementCallbackDelegate<TElement, TEdgeInfo>, AcceptorInformation<TElement, TEdgeInfo>>
   {
      public VisitorInformation( VisitElementCallbackDelegate<TElement, TEdgeInfo> callback, AcceptorInformation<TElement, TEdgeInfo> acceptorInfo )
         : base( callback, acceptorInfo )
      {
      }


   }

   public class VisitorInformation<TElement, TEdgeInfo, TContext> : AbstractVisitorInformation<VisitElementCallbackDelegate<TElement, TEdgeInfo>, AcceptorInformation<TElement, TEdgeInfo, TContext>, TContext>
   {
      public VisitorInformation( VisitElementCallbackDelegate<TElement, TEdgeInfo> callback, AcceptorInformation<TElement, TEdgeInfo, TContext> acceptorInfo, TContext context )
         : base( callback, acceptorInfo, context )
      {
      }
   }

   public class ExplicitVisitorInformation<TElement, TContext> : AbstractVisitorInformation<AcceptVertexExplicitCallbackDelegate<TElement, TContext>, ExplicitAcceptorInformation<TElement, TContext>, TContext>
   {
      public ExplicitVisitorInformation( AcceptVertexExplicitCallbackDelegate<TElement, TContext> callback, ExplicitAcceptorInformation<TElement, TContext> acceptorInfo, TContext context )
         : base( callback, acceptorInfo, context )
      {
      }

   }

}

public static partial class E_UtilPack
{
   public static VisitElementDelegate<TElement, TEdgeInfo> AsVisitElementDelegate<TElement, TEdgeInfo, TActualElement>( this VisitElementDelegateTyped<TElement, TEdgeInfo, TActualElement> typed )
      where TActualElement : TElement
   {
      return ( el, cb ) => typed( (TActualElement) el, cb );
   }

   public static Boolean VisitBaseType<TElement, TEdgeInfo>( this VisitElementCallbackDelegate<TElement, TEdgeInfo> callback, TElement element, Type baseType )
   {
      return callback( element, -1, default( TEdgeInfo ), baseType );
   }

   public static Boolean VisitSimpleEdge<TElement, TEdgeInfo>( this VisitElementCallbackDelegate<TElement, TEdgeInfo> callback, TElement element, Int32 edgeID )
   {
      return callback( element, edgeID, default( TEdgeInfo ) );
   }

   public static Boolean VisitCollectionEdge<TElement>( this VisitElementCallbackDelegate<TElement, Int32> callback, TElement element, Int32 edgeID, Int32 index )
   {
      return callback( element, edgeID, index );
   }

   public static Boolean VisitListEdge<TElement, TEdgeElement>( this VisitElementCallbackDelegate<TElement, Int32> callback, Int32 edgeID, List<TEdgeElement> list )
      where TEdgeElement : TElement
   {
      var retVal = true;
      var max = list.Count;
      for ( var i = 0; i < max && retVal; ++i )
      {
         retVal = callback.VisitCollectionEdge( list[i], edgeID, i );
      }
      return retVal;
   }

   public static VisitorEdgeInfo<TElement, TEdgeInfo> CreatePropertyEdge<TElement, TEdgeInfo, TActualElement>( this VisitorVertexInfoFactory<TElement, TEdgeInfo> factory, String name, Func<Int32, VisitElementDelegateTyped<TElement, TEdgeInfo, TActualElement>> typedDelegate )
      where TActualElement : TElement
   {
      return factory.AddEdge( typeof( TActualElement ), name, id => typedDelegate( id ).AsVisitElementDelegate() );
   }

   public static VisitorEdgeInfo<TElement, TEdgeInfo> CreateBaseTypeEdge<TElement, TEdgeInfo, TActualElement, TBaseType>( this VisitorVertexInfoFactory<TElement, TEdgeInfo> factory )
      where TActualElement : TBaseType
      where TBaseType : TElement
   {
      return factory.AddEdge( typeof( TBaseType ), null, id => ( sig, cb ) => cb.VisitBaseType( sig, typeof( TBaseType ) ) );
   }

   public static Int32 GetEdgeIDOrNegative<TElement, TEdgeInfo>( this TypeBasedVisitor<TElement, TEdgeInfo> visitor, Type type, String name )
   {
      Boolean success;
      return visitor.TryGetEdgeInfo( type, name, out success )?.ID ?? -1;
   }

   public static Int32 GetEdgeIDOrThrow<TElement, TEdgeInfo>( this TypeBasedVisitor<TElement, TEdgeInfo> visitor, Type type, String name )
   {
      var edgeID = visitor.GetEdgeIDOrNegative( type, name );
      if ( edgeID < 0 )
      {
         throw new ArgumentException( "No edge information found for " + type + "." + name + "." );
      }
      return edgeID;
   }


}