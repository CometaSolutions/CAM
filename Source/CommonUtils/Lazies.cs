/*
 * Copyright 2014 Stanislav Muhametsin. All rights Reserved.
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

namespace CommonUtils
{
   // TODO:
   // LazyFactory to create lazies

   // Lazy aspects:
   // Read-only (same as current System.Lazy<T>, except it should set factory to null after value has been successfully loaded)
   // Resettable read-only
   // Writeable (factory to null after setting)
   // Resettable writeable
   // Also thread-safety for all of those
}
