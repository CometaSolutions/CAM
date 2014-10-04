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
 * See the License for the specific language governing permissions and
 * limitations under the License. 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApplicationParameters
{

   /// <summary>
   /// This is abstract class implenting common functionality for all option parsers.
   /// </summary>
   public abstract class AbstractOptionParser
   {
      /// <summary>
      /// Tries to parses the option at index <paramref name="currentIndex"/>.
      /// Sets the out parameters indicating option name and option value, and returns amount of how many arguments have been processed.
      /// </summary>
      /// <param name="args">All command-line options.</param>
      /// <param name="currentIndex">The current index for <paramref name="args"/>.</param>
      /// <param name="parsedOptions">The parsed options from the command line argument.</param>
      /// <returns>
      /// How many arguments processed in <paramref name="args"/>.
      /// Value of <c>0</c> means that current argument is not an option.
      /// Values less than <c>0</c> are considered to be <c>0</c>.
      /// </returns>
      public Int32 TryParseOption( IList<String> args, Int32 currentIndex, OptionModel optionModel, out String optionName, out String optionValueStr, out Object optionValueTyped, out String errorMessage )
      {
         var arg = args[currentIndex];
         Int32 result;
         errorMessage = null;
         optionValueTyped = null;
         if ( !String.IsNullOrEmpty( arg ) && this.IsOption( args, currentIndex, arg ) )
         {
            result = this.ParseOption( args, currentIndex, arg, out optionName, out optionValueStr );
            if ( result > 0 )
            {
               var on = optionName;
               if ( String.Equals( optionModel.PrimaryName, optionName, StringComparison.OrdinalIgnoreCase ) || optionModel.Aliases.Any( alias => String.Equals( alias, on, StringComparison.OrdinalIgnoreCase ) ) )
               {
                  optionModel.TryParseToTyped( optionValueStr, ref optionValueTyped, ref errorMessage );
               }
               else
               {
                  result = 0;
               }
            }
         }
         else
         {
            result = 0;
            optionName = null;
            optionValueStr = null;
         }
         return result;
      }

      public String[] TryExpandArgument( String arg )
      {
         String[] result;
         if ( !String.IsNullOrEmpty( arg ) && this.IsExpandable( arg ) )
         {
            result = this.ExpandArgument( arg );
         }
         else
         {
            result = null;
         }
         return result;
      }


      /// <summary>
      /// Gets the option kind from the command line argument.
      /// The supplied command line argument will always be non-<c>null</c> and not empty.
      /// </summary>
      /// <param name="arg">The command line argument.</param>
      /// <param name="optionKind">The option kind.</param>
      /// <returns><c>true</c> if the <paramref name="arg"/> is recognized as option; <c>false</c> otherwise.</returns>
      protected abstract Boolean IsOption( IList<String> args, Int32 currentIndex, String arg );

      protected abstract Boolean IsExpandable( String arg );

      /// <summary>
      /// This method is called when the argument in <paramref name="args"/> at index <paramref name="currentIndex"/> is not <c>null</c> and not empty.
      /// </summary>
      /// <param name="args"><inheritdoc/></param>
      /// <param name="currentIndex"><inheritdoc/></param>
      /// <param name="currentArg">The current command line option.</param>
      /// <param name="optionName"><inheritdoc/></param>
      /// <param name="optionValue"><inheritdoc/></param>
      /// <returns><inheritdoc/></returns>
      protected abstract Int32 ParseOption( IList<String> args, Int32 currentIndex, String currentArg, out String optionName, out String optionValue );

      protected abstract String[] ExpandArgument( String arg );

   }

   /// <summary>
   /// This class implements the <c>"&lt;option name&gt;:&lt;option value&gt;"</c> parsing style.
   /// </summary>
   public sealed class MSStyleParser : AbstractOptionParser
   {

      private Func<String, String[]> _fileReader;

      public MSStyleParser( Func<String, String[]> fileReader )
      {
         this._fileReader = fileReader;
      }

      protected override Boolean IsOption( IList<String> args, Int32 currentIndex, String arg )
      {
         return arg.Length > 1 && arg[0] == '/' || arg[0] == '-';
      }

      protected override Boolean IsExpandable( string arg )
      {
         return arg[0] == '@';
      }

      protected override Int32 ParseOption( IList<String> options, Int32 currentIndex, String currentArg, out String optionName, out String optionValue )
      {
         var index = 1;
         while ( index < currentArg.Length && currentArg[index] != ':' )
         {
            index++;
         }
         optionName = currentArg.Substring( 1, index - 1 );
         optionValue = currentArg.Substring( Math.Min( currentArg.Length, index + 1 ) );
         // Each argument is option-value pair.
         return 1;
      }

      protected override String[] ExpandArgument( String arg )
      {
         return this._fileReader( arg );
      }
   }
}
