//
// Copyright 2012 Stanislav Muhametsin. All rights Reserved.
//
// Licensed  under the  Apache License,  Version 2.0  (the "License");
// you may not use  this file  except in  compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed  under the  License is distributed on an "AS IS" BASIS,
// WITHOUT  WARRANTIES OR CONDITIONS  OF ANY KIND, either  express  or
// implied.
//
// See the License for the specific language governing permissions and
// limitations under the License. 
//
// This file uses IL code order to have System.Delegate as generic constraint.
// The methods would look something like this in C#:

   //public static Boolean InvokeEventIfNotNull<TDelegate>( this TDelegate evt, Action<TDelegate> invoker )
   //   where TDelegate : class
   //{
   //   var result = evt != null;
   //   if ( result )
   //   {
   //      invoker( evt );
   //   }
   //   return result;
   //}

   //public static Boolean InvokeAllEventHandlers<TDelegate>( this TDelegate evt, Action<TDelegate> invoker, Boolean throwExceptions = true )
   //   where TDelegate : class
   //{
   //   LinkedList<Exception> exceptions = null;
   //   var result = evt != null;
   //   if ( result )
   //   {
   //      var invocationList = ( (Delegate) (Object) evt ).GetInvocationList();
   //      for ( var i = 0; i < invocationList.Length; ++i )
   //      {
   //         try
   //         {
   //            invoker( (TDelegate) (Object) invocationList[i] );
   //         }
   //         catch ( Exception exc )
   //         {
   //            if ( throwExceptions )
   //            {
   //               if ( exceptions == null )
   //               {
   //                  // Just re-throw if this is last handler and first exception
   //                  if ( i == invocationList.Length - 1 )
   //                  {
   //                     throw;
   //                  }
   //                  else
   //                  {
   //                     exceptions = new LinkedList<Exception>();
   //                  }
   //               }
   //               exceptions.AddLast( exc );
   //            }
   //         }
   //      }
   //   }

   //   if ( exceptions != null )
   //   {
   //      throw new AggregateException( exceptions.ToArray() );
   //   }

   //   return result;
   //}

   //public static Boolean InvokeAllEventHandlers<TDelegate>( this TDelegate evt, Action<TDelegate> invoker, out Exception[] occurredExceptions )
   //   where TDelegate : class
   //{
   //   LinkedList<Exception> exceptions = null;
   //   var result = evt != null;
   //   if ( result )
   //   {
   //      foreach ( var handler in ( (Delegate) (Object) evt ).GetInvocationList() )
   //      {
   //         try
   //         {
   //            invoker( (TDelegate) (Object) handler );
   //         }
   //         catch ( Exception exc )
   //         {
   //            if ( exceptions == null )
   //            {
   //               exceptions = new LinkedList<Exception>();
   //            }
   //            exceptions.AddLast( exc );
   //         }
   //      }
   //   }
   //   if ( exceptions != null )
   //   {
   //      occurredExceptions = exceptions.ToArray();
   //   }
   //   else
   //   {
   //      occurredExceptions = null;
   //   }
   //   return result;
   //}

.class public auto ansi abstract sealed beforefieldinit E_CommonUtils
   extends [$OBJECT_ASSEMBLY$]System.Object
{
.method public hidebysig static 
   bool InvokeEventIfNotNull<([$DELEGATE_ASSEMBLY$]System.Delegate) TDelegate> (
      !!TDelegate del,
      class [$ACTION_ASSEMBLY$]System.Action`1<!!TDelegate> invoker
   ) cil managed 
{
   .custom instance void [$EXTENSION_ASSEMBLY$]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = (
      01 00 00 00
   )
   .custom instance void [$DELEGATE_ASSEMBLY$]System.ObsoleteAttribute::.ctor(string, bool) = (
      01 00 3e 54 68 69 73 20 6d 65 74 68 6f 64 20 68
      61 73 20 62 65 65 6e 20 6d 61 64 65 20 6f 62 73
      6f 6c 65 74 65 20 62 79 20 22 3f 2e 22 2d 6f 70
      65 72 61 74 6f 72 20 69 6e 20 43 23 20 36 2e 30
      2e 00 00 00
   )
   // Method begins at RVA 0x2c48
   // Code size 25 (0x19)
   .maxstack 2
   .locals init (
      [0] bool
   )

   IL_0000: ldarg.0
   IL_0001: box !!TDelegate
   IL_0006: ldnull
   IL_0007: ceq
   IL_0009: ldc.i4.0
   IL_000a: ceq
   IL_000c: stloc.0
   IL_000d: ldloc.0
   IL_000e: brfalse.s IL_0017

   IL_0010: ldarg.1
   IL_0011: ldarg.0
   IL_0012: callvirt instance void class [$ACTION_ASSEMBLY$]System.Action`1<!!TDelegate>::Invoke(!0)

   IL_0017: ldloc.0
   IL_0018: ret
}

.method public hidebysig static 
   bool InvokeAllEventHandlers<([$DELEGATE_ASSEMBLY$]System.Delegate) TDelegate> (
      !!TDelegate del,
      class [$ACTION_ASSEMBLY$]System.Action`1<!!TDelegate> invoker,
      [opt] bool throwExceptions
   ) cil managed 
{
   .custom instance void [$EXTENSION_ASSEMBLY$]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = (
      01 00 00 00
   )
   .param [3] = bool(true)
   // Method begins at RVA 0x2060
   // Code size 114 (0x72)
   .maxstack 3
   .locals init (
      [0] class [$LINKED_LIST_ASSEMBLY$]System.Collections.Generic.LinkedList`1<class [$EXCEPTION_ASSEMBLY$]System.Exception> exceptions,
      [1] bool result,
      [2] class [$DELEGATE_ASSEMBLY$]System.Delegate[] invocationList,
      [3] int32 i,
      [4] class [$EXCEPTION_ASSEMBLY$]System.Exception exc
   )

   IL_0000: ldnull
   IL_0001: stloc.0
   IL_0002: ldarg.0
   IL_0003: box !!TDelegate
   IL_0008: ldnull
   IL_0009: ceq
   IL_000b: ldc.i4.0
   IL_000c: ceq
   IL_000e: stloc.1
   IL_000f: ldloc.1
   IL_0010: brfalse.s IL_0061

   IL_0012: ldarga.s del
   IL_0014: constrained. !!TDelegate
   IL_001a: callvirt instance class [$DELEGATE_ASSEMBLY$]System.Delegate[] [$DELEGATE_ASSEMBLY$]System.Delegate::GetInvocationList()
   IL_001f: stloc.2
   IL_0020: ldc.i4.0
   IL_0021: stloc.3
   IL_0022: br.s IL_005b
   // loop start (head: IL_005b)
      .try
      {
         IL_0024: ldarg.1
         IL_0025: ldloc.2
         IL_0026: ldloc.3
         IL_0027: ldelem.ref
         IL_0028: unbox.any !!TDelegate
         IL_002d: callvirt instance void class [$ACTION_ASSEMBLY$]System.Action`1<!!TDelegate>::Invoke(!0)
         IL_0032: leave.s IL_0057
      } // end .try
      catch [$EXCEPTION_ASSEMBLY$]System.Exception
      {
         IL_0034: stloc.s exc
         IL_0036: ldarg.2
         IL_0037: brfalse.s IL_0055

         IL_0039: ldloc.0
         IL_003a: brtrue.s IL_004c

         IL_003c: ldloc.3
         IL_003d: ldloc.2
         IL_003e: ldlen
         IL_003f: conv.i4
         IL_0040: ldc.i4.1
         IL_0041: sub
         IL_0042: bne.un.s IL_0046

         IL_0044: rethrow

         IL_0046: newobj instance void class [$LINKED_LIST_ASSEMBLY$]System.Collections.Generic.LinkedList`1<class [$EXCEPTION_ASSEMBLY$]System.Exception>::.ctor()
         IL_004b: stloc.0

         IL_004c: ldloc.0
         IL_004d: ldloc.s exc
         IL_004f: callvirt instance class [$LINKED_LIST_ASSEMBLY$]System.Collections.Generic.LinkedListNode`1<!0> class [$LINKED_LIST_ASSEMBLY$]System.Collections.Generic.LinkedList`1<class [$EXCEPTION_ASSEMBLY$]System.Exception>::AddLast(!0)
         IL_0054: pop

         IL_0055: leave.s IL_0057
      } // end handler

      IL_0057: ldloc.3
      IL_0058: ldc.i4.1
      IL_0059: add
      IL_005a: stloc.3

      IL_005b: ldloc.3
      IL_005c: ldloc.2
      IL_005d: ldlen
      IL_005e: conv.i4
      IL_005f: blt.s IL_0024
   // end loop

   IL_0061: ldloc.0
   IL_0062: brfalse.s IL_0070

   IL_0064: ldloc.0
   IL_0065: call !!0[] [$ENUMERABLE_ASSEMBLY$]System.Linq.Enumerable::ToArray<class [$EXCEPTION_ASSEMBLY$]System.Exception>(class [$IENUMERABLE_ASSEMBLY$]System.Collections.Generic.IEnumerable`1<!!0>)
   IL_006a: newobj instance void [$AGGREGATE_EXCEPTION_ASSEMBLY$]System.AggregateException::.ctor(class [$EXCEPTION_ASSEMBLY$]System.Exception[])
   IL_006f: throw

   IL_0070: ldloc.1
   IL_0071: ret
}


.method public hidebysig static 
   bool InvokeAllEventHandlers<([$DELEGATE_ASSEMBLY$]System.Delegate) TDelegate> (
      !!TDelegate del,
      class [$ACTION_ASSEMBLY$]System.Action`1<!!TDelegate> invoker,
      [out] class [$EXCEPTION_ASSEMBLY$]System.Exception[]& occurredExceptions
   ) cil managed 
{
   .custom instance void [$EXTENSION_ASSEMBLY$]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = (
      01 00 00 00
   )
   // Method begins at RVA 0x20f0
   // Code size 110 (0x6e)
   .maxstack 2
   .locals init (
      [0] class [$LINKED_LIST_ASSEMBLY$]System.Collections.Generic.LinkedList`1<class [$EXCEPTION_ASSEMBLY$]System.Exception> exceptions,
      [1] bool result,
      [2] class [$DELEGATE_ASSEMBLY$]System.Delegate 'handler',
      [3] class [$EXCEPTION_ASSEMBLY$]System.Exception exc,
      [4] class [$DELEGATE_ASSEMBLY$]System.Delegate[] CS$6$0000,
      [5] int32 CS$7$0001
   )

   IL_0000: ldnull
   IL_0001: stloc.0
   IL_0002: ldarg.0
   IL_0003: box !!TDelegate
   IL_0008: ldnull
   IL_0009: ceq
   IL_000b: ldc.i4.0
   IL_000c: ceq
   IL_000e: stloc.1
   IL_000f: ldloc.1
   IL_0010: brfalse.s IL_005c

   IL_0012: ldarga.s del
   IL_0014: constrained. !!TDelegate
   IL_001a: callvirt instance class [$DELEGATE_ASSEMBLY$]System.Delegate[] [$DELEGATE_ASSEMBLY$]System.Delegate::GetInvocationList()
   IL_001f: stloc.s CS$6$0000
   IL_0021: ldc.i4.0
   IL_0022: stloc.s CS$7$0001
   IL_0024: br.s IL_0054
   // loop start (head: IL_0054)
      IL_0026: ldloc.s CS$6$0000
      IL_0028: ldloc.s CS$7$0001
      IL_002a: ldelem.ref
      IL_002b: stloc.2
      .try
      {
         IL_002c: ldarg.1
         IL_002d: ldloc.2
         IL_002e: unbox.any !!TDelegate
         IL_0033: callvirt instance void class [$ACTION_ASSEMBLY$]System.Action`1<!!TDelegate>::Invoke(!0)
         IL_0038: leave.s IL_004e
      } // end .try
      catch [$EXCEPTION_ASSEMBLY$]System.Exception
      {
         IL_003a: stloc.3
         IL_003b: ldloc.0
         IL_003c: brtrue.s IL_0044

         IL_003e: newobj instance void class [$LINKED_LIST_ASSEMBLY$]System.Collections.Generic.LinkedList`1<class [$EXCEPTION_ASSEMBLY$]System.Exception>::.ctor()
         IL_0043: stloc.0

         IL_0044: ldloc.0
         IL_0045: ldloc.3
         IL_0046: callvirt instance class [$LINKED_LIST_ASSEMBLY$]System.Collections.Generic.LinkedListNode`1<!0> class [$LINKED_LIST_ASSEMBLY$]System.Collections.Generic.LinkedList`1<class [$EXCEPTION_ASSEMBLY$]System.Exception>::AddLast(!0)
         IL_004b: pop
         IL_004c: leave.s IL_004e
      } // end handler

      IL_004e: ldloc.s CS$7$0001
      IL_0050: ldc.i4.1
      IL_0051: add
      IL_0052: stloc.s CS$7$0001

      IL_0054: ldloc.s CS$7$0001
      IL_0056: ldloc.s CS$6$0000
      IL_0058: ldlen
      IL_0059: conv.i4
      IL_005a: blt.s IL_0026
   // end loop

   IL_005c: ldloc.0
   IL_005d: brfalse.s IL_0069

   IL_005f: ldarg.2
   IL_0060: ldloc.0
   IL_0061: call !!0[] [$ENUMERABLE_ASSEMBLY$]System.Linq.Enumerable::ToArray<class [$EXCEPTION_ASSEMBLY$]System.Exception>(class [$IENUMERABLE_ASSEMBLY$]System.Collections.Generic.IEnumerable`1<!!0>)
   IL_0066: stind.ref
   IL_0067: br.s IL_006c

   IL_0069: ldarg.2
   IL_006a: ldnull
   IL_006b: stind.ref

   IL_006c: ldloc.1
   IL_006d: ret
}


}