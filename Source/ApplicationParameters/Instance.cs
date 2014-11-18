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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ApplicationParameters;

namespace ApplicationParameters
{
   public class ApplicationParameters
   {
      private readonly IList<AbstractApplicationParameter> _parameters;
      private readonly IList<ValidationError> _errors;
      private readonly ApplicationParametersModel _model;

      public ApplicationParameters( ApplicationParametersModel model, String[] args, Func<IList<AbstractApplicationParameter>, IList<ValidationError>> additionalValidator = null )
      {
         this._model = model;
         var list = new List<AbstractApplicationParameter>( model.Parameters.Count );
         Int32 idx = 0;
         var argsList = args.ToList();
         var argsListReadOnly = new ReadOnlyCollection<String>( argsList );
         var errors = new List<ValidationError>();
         var i = 0;
         var curParamCount = 0;
         var latestAddedParamModelIdx = -1;
         while ( i < model.Parameters.Count && idx < argsList.Count )
         {
            var pModel = model.Parameters[i];
            var instance = ProcessParameterModel( pModel, model, argsList, argsListReadOnly, errors, ref idx );
            if ( instance != null )
            {
               ++curParamCount;
            }

            var noInstanceOrTooBigIndex = instance == null || idx >= argsList.Count;
            if ( noInstanceOrTooBigIndex || ( pModel.MaxOccurrences >= 0 && curParamCount > pModel.MaxOccurrences ) )
            {
               ++i; // Move to next parameter
               if ( noInstanceOrTooBigIndex && curParamCount < pModel.MinOccurrences )
               {
                  errors.Add( new ValidationError( null, "Parameter " + pModel.Name + " occurred too few times." ) );

               }
               else if ( !noInstanceOrTooBigIndex )
               {
                  errors.Add( new ValidationError( null, "Parameter " + pModel.Name + " occurred too many times." ) );
               }
               else if ( instance != null )
               {
                  list.Add( instance );
               }
               curParamCount = 0;
            }
            else
            {
               list.Add( instance );
               latestAddedParamModelIdx = i;
            }
         }
         ++latestAddedParamModelIdx;

         if ( latestAddedParamModelIdx < model.Parameters.Count )
         {
            errors.AddRange( model.Parameters.Skip( i ).Where( p => p.MinOccurrences > 0 ).Select( p => new ValidationError( null, "Parameter " + p.Name + " occurred too few times." ) ) );
         }

         this._parameters = new ReadOnlyCollection<AbstractApplicationParameter>( list );

         if ( additionalValidator != null )
         {
            var aErrors = additionalValidator( this._parameters );
            if ( aErrors != null )
            {
               errors.AddRange( aErrors );
            }
         }

         this._errors = new ReadOnlyCollection<ValidationError>( errors );
      }

      public IList<AbstractApplicationParameter> Parameters
      {
         get
         {
            return this._parameters;
         }
      }

      public IList<ValidationError> Errors
      {
         get
         {
            return this._errors;
         }
      }

      public ApplicationParametersModel Model
      {
         get
         {
            return this._model;
         }
      }

      private static AbstractApplicationParameter ProcessParameterModel( ApplicationParameterModel p, ApplicationParametersModel paramsModel, IList<String> args, IList<String> argsListReadOnly, IList<ValidationError> errors, ref Int32 idx )
      {
         var arg = args[idx];
         if ( p.Expandable )
         {
            var eArgs = TryExpandArgumentsRecursively( paramsModel.ParameterParser, arg );
            if ( eArgs != null )
            {
               // Expanded successfully
               args.RemoveAt( idx );
               for ( var i = eArgs.Count - 1; i >= 0; --i )
               {
                  args.Insert( idx, eArgs[i] );
               }
               // Read-only list should reflect the changed list
            }
         }

         AbstractApplicationParameter pInstance = null;
         if ( idx < args.Count )
         {
            arg = args[idx];
            foreach ( var optionModel in p.PossibleOptions )
            {
               String on, ov, errorMsg;
               Object ovt;
               var oldIdx = idx;
               idx += paramsModel.ParameterParser.TryParseOption( argsListReadOnly, idx, optionModel, out on, out ov, out ovt, out errorMsg );
               if ( idx > oldIdx )
               {
                  pInstance = new ApplicationOptionParameter( p, optionModel, on, ov, ovt );
                  if ( errorMsg != null )
                  {
                     errors.Add( new ValidationError( on, errorMsg ) );
                  }
                  break;
               }
            }

            if ( pInstance == null && p.CanBeNonOption )
            {
               pInstance = new ApplicationValueParameter( p, arg );
               idx += 1;
            }
         }

         return pInstance;
      }

      private static IList<String> TryExpandArgumentsRecursively( AbstractOptionParser parser, String arg )
      {
         List<String> result = null;
         var array = parser.TryExpandArgument( arg );
         if ( array != null )
         {
            result = new List<String>( array );
            foreach ( var str in array )
            {
               var list = TryExpandArgumentsRecursively( parser, str );
               if ( list != null )
               {
                  result.AddRange( list );
               }
            }
         }
         return result;
      }
   }

   public abstract class AbstractApplicationParameter
   {
      private readonly ApplicationParameterModel _parameterModel;

      public AbstractApplicationParameter( ApplicationParameterModel parameterModel )
      {
         this._parameterModel = parameterModel;
      }

      public ApplicationParameterModel ParameterModel
      {
         get
         {
            return this._parameterModel;
         }
      }
   }

   public class ApplicationOptionParameter : AbstractApplicationParameter
   {
      private readonly OptionModel _model;
      private readonly String _optionName;
      private readonly String _optionValue;
      private readonly Object _optionValueTyped;

      public ApplicationOptionParameter( ApplicationParameterModel pModel, OptionModel model, String optionName, String optionValue, Object optionValueTyped )
         : base( pModel )
      {
         this._model = model;
         this._optionName = optionName ?? "";
         this._optionValue = optionValue ?? "";
         this._optionValueTyped = optionValueTyped;
      }

      public OptionModel Model
      {
         get
         {
            return this._model;
         }
      }

      public String OptionName
      {
         get
         {
            return this._optionName;
         }
      }

      public String OptionValueAsString
      {
         get
         {
            return this._optionValue;
         }
      }

      public Object OptionValueTyped
      {
         get
         {
            return this._optionValueTyped;
         }
      }

      public override String ToString()
      {
         return this._optionName + "=" + this._optionValueTyped;
      }
   }

   public class ApplicationValueParameter : AbstractApplicationParameter
   {
      private readonly String _value;

      internal ApplicationValueParameter( ApplicationParameterModel parameterModel, String value )
         : base( parameterModel )
      {
         this._value = value;
      }

      public String Value
      {
         get
         {
            return this._value;
         }
      }

      public override string ToString()
      {
         return this._value;
      }
   }

   public struct ValidationError
   {
      private readonly String _optionName;
      private readonly String _errorMessage;

      public ValidationError( String optionName, String errorMessage )
      {
         this._optionName = optionName;
         this._errorMessage = errorMessage;
      }

      public String OptionName
      {
         get
         {
            return this._optionName;
         }
      }

      public String ErrorMessage
      {
         get
         {
            return this._errorMessage;
         }
      }

      public override String ToString()
      {
         return this._optionName == null ?
            this._errorMessage :
            ( "Option: " + this._optionName + ", message: " + this._errorMessage );
      }
   }

   public class SimpleApplicationParameters : ApplicationParameters
   {
      private readonly IList<ApplicationOptionParameter> EMPTY_PARAM = new ReadOnlyCollection<ApplicationOptionParameter>( new List<ApplicationOptionParameter>() );

      private readonly IDictionary<String, IList<ApplicationOptionParameter>> _options;
      private readonly IList<String> _values;
      private readonly ApplicationOptionParameter _separatorParameter;
      private readonly Boolean _helpOptionPresent;

      public SimpleApplicationParameters( SimpleApplicationParametersModel model, String[] args )
         : base( model, args, pList =>
            {
               var options = pList.Where( p => Object.ReferenceEquals( p.ParameterModel, model.OptionModel ) ).ToArray();
               var optionNames = options.Select( p => ( (ApplicationOptionParameter) p ).OptionName );
               var optionNameSet = new HashSet<String>( optionNames, StringComparer.OrdinalIgnoreCase );
               ValidationError[] errors = null;
               if ( options.Length != optionNameSet.Count )
               {
                  // Duplicates occurred
                  var optionNameList = new List<String>( optionNames );
                  // Find the option names that were duplicated
                  foreach ( var on in optionNameSet )
                  {
                     optionNameList.Remove( on );
                  }
                  // Remove the ones which are allowed to have duplicates
                  foreach ( var d in model.OptionsWithDuplicatesAllowed )
                  {
                     optionNameList.RemoveAll( str => String.Equals( str, d, StringComparison.OrdinalIgnoreCase ) );
                  }
                  // If there are any actual duplicates left, create error
                  if ( optionNameList.Count > 0 )
                  {
                     errors = new[] { new ValidationError( null, "Duplicate option names: " + String.Join( ", ", optionNameList ) ) };
                  }
               }
               return errors;
            } )
      {
         var optionParams = this.Parameters
            .Where( p => Object.ReferenceEquals( p.ParameterModel, model.OptionModel ) )
            .Cast<ApplicationOptionParameter>();
         this._options = optionParams
            .GroupBy( p => p.OptionName )
            .ToDictionary( g => g.Key, g => (IList<ApplicationOptionParameter>) new ReadOnlyCollection<ApplicationOptionParameter>( g.ToList() ) );
         this._values = new ReadOnlyCollection<String>( this.Parameters
            .Where( p => Object.ReferenceEquals( p.ParameterModel, model.ValueModel ) )
            .Cast<ApplicationValueParameter>()
            .Select( p => p.Value )
            .ToList() );
         this._separatorParameter = this.Parameters
            .Where( p => Object.ReferenceEquals( p.ParameterModel, model.OptionAndValueSeparator ) )
            .Cast<ApplicationOptionParameter>()
            .FirstOrDefault();
         this._helpOptionPresent = optionParams
            .Where( p => Object.ReferenceEquals( p.Model, model.HelpOption ) )
            .Any();
      }

      public ApplicationOptionParameter GetSingleOptionOrNull( String name )
      {
         IList<ApplicationOptionParameter> param;
         if ( this._options.TryGetValue( name, out param ) && param.Count > 0 )
         {
            return param[0];
         }
         else
         {
            return null;
         }
      }

      public IList<ApplicationOptionParameter> GetMultipleOptionsOrEmpty( String name )
      {
         IList<ApplicationOptionParameter> param;
         if ( this._options.TryGetValue( name, out param ) )
         {
            return param;
         }
         else
         {
            return EMPTY_PARAM;
         }
      }

      public IList<String> Values
      {
         get
         {
            return this._values;
         }
      }

      public ApplicationOptionParameter SeparatorOption
      {
         get
         {
            return this._separatorParameter;
         }
      }

      public Boolean HelpOptionPresent
      {
         get
         {
            return this._helpOptionPresent;
         }
      }
   }
}

public static partial class E_ApplicationParameters
{
   public static T GetOrDefault<T>( this ApplicationOptionParameter param, T defaultValue = default(T) )
   {
      return param == null ? defaultValue : (T) param.OptionValueTyped;
   }
}
