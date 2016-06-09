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
// The class would look something like this in C#:
//
//public class DelegateMultiplexer<TKey, TDelegate> : Multiplexer<TKey, TDelegate>
//   where TDelegate : Delegate
//{

//   public DelegateMultiplexer( IEqualityComparer<TKey> equalityComparer = null )
//      : base( equalityComparer )
//   {
//   }

//   protected override TDelegate Combine( TDelegate existing, TDelegate newValue )
//   {
//      return (TDelegate) Delegate.Combine( existing, newValue );
//   }

//   protected override TDelegate Remove( TDelegate existing, TDelegate removable )
//   {
//      return (TDelegate) Delegate.Remove( existing, removable );
//   }
//}

.class public beforefieldinit UtilPack.DelegateMultiplexer`2<TKey, ([$DELEGATE_ASSEMBLY$]System.Delegate) TDelegate>
    extends class UtilPack.Multiplexer`2<!TKey, !TDelegate>
{


   // Constructors
   .method public hidebysig specialname rtspecialname 
      instance void .ctor (
         [opt] class [$EQUALITY_COMPARER_ASSEMBLY$]System.Collections.Generic.IEqualityComparer`1<!TKey> equalityComparer
      ) cil managed 
   {
      .param [1] = nullref
      .maxstack 2

      IL_0000: ldarg.0
      IL_0001: ldarg.1
      IL_0002: call instance void class UtilPack.Multiplexer`2<!TKey, !TDelegate>::.ctor(class [$EQUALITY_COMPARER_ASSEMBLY$]System.Collections.Generic.IEqualityComparer`1<!0>)
      IL_0007: ret
   }

   // Methods
   .method family hidebysig virtual 
      instance !TDelegate Combine (
         !TDelegate existing,
         !TDelegate newValue
      ) cil managed 
   {
      .maxstack 2

      IL_0000: ldarg.1
      IL_0001: box !TDelegate
      IL_0006: ldarg.2
      IL_0007: box !TDelegate
      IL_000c: call class [$DELEGATE_ASSEMBLY$]System.Delegate [$DELEGATE_ASSEMBLY$]System.Delegate::Combine(class [$DELEGATE_ASSEMBLY$]System.Delegate, class [$DELEGATE_ASSEMBLY$]System.Delegate)
      IL_0011: unbox.any !TDelegate
      IL_0016: ret
   }

   .method family hidebysig virtual 
      instance !TDelegate Remove (
         !TDelegate existing,
         !TDelegate removable
      ) cil managed 
   {
      .maxstack 2

      IL_0000: ldarg.1
      IL_0001: box !TDelegate
      IL_0006: ldarg.2
      IL_0007: box !TDelegate
      IL_000c: call class [$DELEGATE_ASSEMBLY$]System.Delegate [$DELEGATE_ASSEMBLY$]System.Delegate::Remove(class [$DELEGATE_ASSEMBLY$]System.Delegate, class [$DELEGATE_ASSEMBLY$]System.Delegate)
      IL_0011: unbox.any !TDelegate
      IL_0016: ret
   }
}
