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
using System.Text.RegularExpressions;

namespace ApplicationParameters
{
   public class ApplicationParametersModel
   {
      private static readonly AbstractOptionParser DEFAULT_PARSER = new MSStyleParser( null );

      private readonly IList<ApplicationParameterModel> _parameters;
      private readonly AbstractOptionParser _parser;

      public ApplicationParametersModel( params ApplicationParameterModel[] parameters )
         : this( null, parameters )
      {

      }
      public ApplicationParametersModel( AbstractOptionParser parser, params ApplicationParameterModel[] parameters )
      {
         this._parser = parser ?? DEFAULT_PARSER;
         this._parameters = new ReadOnlyCollection<ApplicationParameterModel>( parameters.FilterNulls() );
      }

      public IList<ApplicationParameterModel> Parameters
      {
         get
         {
            return this._parameters;
         }
      }

      public AbstractOptionParser ParameterParser
      {
         get
         {
            return this._parser;
         }
      }
   }

   public class ApplicationParameterModel
   {
      public const Int32 MAX_UNLIMITED = -1;

      private readonly String _name;
      private readonly Boolean _canBeNonOption;
      private readonly IList<OptionModel> _possibleOptions;
      private readonly Int32 _minOccurrences;
      private readonly Int32 _maxOccurrences;

      public ApplicationParameterModel( String name, Boolean canBeNonOption, Int32 maxOccurrences, params OptionModel[] possibleOptions )
         : this( name, canBeNonOption, 0, maxOccurrences, possibleOptions )
      {

      }

      public ApplicationParameterModel( String name, Boolean canBeNonOption, Int32 minOccurrences, Int32 maxOccurrences, params OptionModel[] possibleOptions )
      {
         this._name = name ?? "";
         this._canBeNonOption = canBeNonOption;
         this._minOccurrences = Math.Max( 0, minOccurrences );
         this._maxOccurrences = Math.Max( -1, maxOccurrences );
         this._possibleOptions = new ReadOnlyCollection<OptionModel>( possibleOptions.FilterNulls() );
      }

      public String Name
      {
         get
         {
            return this._name;
         }
      }

      public Boolean CanBeNonOption
      {
         get
         {
            return this._canBeNonOption;
         }
      }

      public IList<OptionModel> PossibleOptions
      {
         get
         {
            return this._possibleOptions;
         }
      }

      public Boolean Expandable
      {
         get
         {
            return this.PossibleOptions.Count > 0;
         }
      }

      public Int32 MinOccurrences
      {
         get
         {
            return this._minOccurrences;
         }
      }

      public Int32 MaxOccurrences
      {
         get
         {
            return this._maxOccurrences;
         }
      }

      public override String ToString()
      {
         return this.Name;
      }
   }

   public class OptionModel
   {
      private readonly String _primaryName;
      private readonly IList<String> _aliases;
      private readonly Type _optionValueType;

      public OptionModel( String primaryName, params String[] aliases )
         : this( typeof( String ), primaryName, aliases )
      {

      }

      public OptionModel( Type optionValueType, String primaryName, params String[] aliases )
      {
         this._primaryName = primaryName ?? "";
         this._optionValueType = optionValueType ?? typeof( String );
         this._aliases = new ReadOnlyCollection<String>( aliases.FilterNulls() );
      }

      public String PrimaryName
      {
         get
         {
            return this._primaryName;
         }
      }

      public Type OptionValueType
      {
         get
         {
            return this._optionValueType;
         }
      }

      public IList<String> Aliases
      {
         get
         {
            return this._aliases;
         }
      }

      public virtual void TryParseToTyped( String value, ref Object valueTyped, ref String errorMessage )
      {
         switch ( Type.GetTypeCode( this._optionValueType ) )
         {
            case TypeCode.Boolean:
               valueTyped = String.Equals( "+", value );
               if ( !( (Boolean) valueTyped ) && !String.Equals( "-", value ) )
               {
                  valueTyped = null;
                  errorMessage = "Either \"+\" for true and \"-\" for false are accepted.";
               }
               break;
            case TypeCode.Byte:
               Byte b;
               if ( Byte.TryParse( value, out b ) )
               {
                  valueTyped = b;
               }
               else
               {
                  errorMessage = "Acceptable range is [" + Byte.MinValue + ".." + Byte.MaxValue + "]";
               }
               break;
            case TypeCode.Char:
               if ( value.Length > 0 )
               {
                  valueTyped = value[0];
               }
               else
               {
                  errorMessage = "Acceptable value must have at least one character.";
               }
               break;
            case TypeCode.Decimal:
               Decimal d;
               if ( Decimal.TryParse( value, out d ) )
               {
                  valueTyped = d;
               }
               else
               {
                  errorMessage = "Acceptable values are decimal values.";
               }
               break;
            case TypeCode.Double:
               Double dd;
               if ( Double.TryParse( value, out dd ) )
               {
                  valueTyped = dd;
               }
               else
               {
                  errorMessage = "Acceptable range is [" + Double.MinValue + ".." + Double.MaxValue + "]";
               }
               break;
            case TypeCode.Int16:
               Int16 i16;
               if ( Int16.TryParse( value, out i16 ) )
               {
                  valueTyped = i16;
               }
               else
               {
                  errorMessage = "Acceptable range is [" + Int16.MinValue + ".." + Int16.MaxValue + "]";
               }
               break;
            case TypeCode.Int32:
               Int32 i32;
               if ( Int32.TryParse( value, out i32 ) )
               {
                  valueTyped = i32;
               }
               else
               {
                  errorMessage = "Acceptable range is [" + Int32.MinValue + ".." + Int32.MaxValue + "]";
               }
               break;
            case TypeCode.Int64:
               Int64 i64;
               if ( Int64.TryParse( value, out i64 ) )
               {
                  valueTyped = i64;
               }
               else
               {
                  errorMessage = "Acceptable range is [" + Int64.MinValue + ".." + Int64.MaxValue + "]";
               }
               break;
            case TypeCode.SByte:
               SByte sb;
               if ( SByte.TryParse( value, out sb ) )
               {
                  valueTyped = sb;
               }
               else
               {
                  errorMessage = "Acceptable range is [" + SByte.MinValue + ".." + SByte.MaxValue + "]";
               }
               break;
            case TypeCode.Single:
               Single s;
               if ( Single.TryParse( value, out s ) )
               {
                  valueTyped = s;
               }
               else
               {
                  errorMessage = "Acceptable range is [" + Single.MinValue + ".." + Single.MaxValue + "]";
               }
               break;
            case TypeCode.String:
               valueTyped = value;
               break;
            case TypeCode.UInt16:
               UInt16 u16;
               if ( UInt16.TryParse( value, out u16 ) )
               {
                  valueTyped = u16;
               }
               else
               {
                  errorMessage = "Acceptable range is [" + UInt16.MinValue + ".." + UInt16.MaxValue + "]";
               }
               break;
            case TypeCode.UInt32:
               UInt32 u32;
               if ( UInt32.TryParse( value, out u32 ) )
               {
                  valueTyped = u32;
               }
               else
               {
                  errorMessage = "Acceptable range is [" + UInt32.MinValue + ".." + UInt32.MaxValue + "]";
               }
               break;
            case TypeCode.UInt64:
               UInt64 u64;
               if ( UInt64.TryParse( value, out u64 ) )
               {
                  valueTyped = u64;
               }
               else
               {
                  errorMessage = "Acceptable range is [" + UInt64.MinValue + ".." + UInt64.MaxValue + "]";
               }
               break;
            default:
               this.TryParseToTypedFallback( value, ref valueTyped, ref errorMessage );
               break;
         }

         if ( errorMessage != null )
         {
            this.PostProcessTypedValue( value, valueTyped, ref errorMessage );
         }
      }

      protected virtual void TryParseToTypedFallback( String value, ref Object valueTyped, ref String errorMessage )
      {
         errorMessage = "Internal error: do not know how to parse option of type " + this._optionValueType;
      }

      protected virtual void PostProcessTypedValue( String valueStr, Object valueTyped, ref String errorMessage )
      {
         // Do nothing by default
      }
   }

   public class StringOptionModel : OptionModel
   {
      private readonly Regex _regex;

      public StringOptionModel( String primaryName, params String[] aliases )
         : this( primaryName, null, aliases )
      {

      }

      public StringOptionModel( String primaryName, Regex matchingRegex, params String[] aliases )
         : base( typeof( String ), primaryName, aliases )
      {
         this._regex = matchingRegex;
      }

      protected override void PostProcessTypedValue( string valueStr, object valueTyped, ref string errorMessage )
      {
         if ( this._regex != null && !this._regex.IsMatch( valueStr ) )
         {
            errorMessage = "Option value must match regex: " + this._regex;
         }
      }
   }

   public class BooleanOptionModel : OptionModel
   {
      public BooleanOptionModel( String primaryName, params String[] aliases )
         : base( typeof( Boolean ), primaryName, aliases )
      {

      }
   }

   public class SwitchOptionModel : OptionModel
   {
      public SwitchOptionModel( String primaryName, params String[] aliases )
         : base( typeof( Boolean ), primaryName, aliases )
      {

      }

      public override void TryParseToTyped( String value, ref Object valueTyped, ref String errorMessage )
      {
         if ( value.Trim().Length > 0 )
         {
            errorMessage = "Option should not have any value.";
         }
         else
         {
            valueTyped = true;
         }
      }
   }

   public class EnumOptionModel<TEnum> : OptionModel
      where TEnum : struct
   {
      private readonly Boolean _ignoreCase;
      private readonly Func<String, String> _preProcessor;

      public EnumOptionModel( String primaryName, Boolean ignoreCase, params String[] aliases )
         : this( primaryName, ignoreCase, (Func<String, String>) null )
      {

      }

      public EnumOptionModel( String primaryName, Boolean ignoreCase, Func<String, String> preProcessor, params String[] aliases )
         : base( typeof( TEnum ), primaryName, aliases )
      {
         this._preProcessor = preProcessor;
         this._ignoreCase = ignoreCase;
      }

      public override void TryParseToTyped( string value, ref object valueTyped, ref string errorMessage )
      {
         if ( this._preProcessor != null )
         {
            value = this._preProcessor( value );
         }
         TEnum enumValue;
         if ( Enum.TryParse<TEnum>( value, this._ignoreCase, out enumValue ) )
         {
            valueTyped = enumValue;
         }
         else
         {
            base.TryParseToTyped( value, ref valueTyped, ref errorMessage );
            if ( errorMessage != null )
            {
               errorMessage = "Option value should be one of the following: " + String.Join( ", ", Enum.GetNames( typeof( TEnum ) ) ) + ", or their numeric corresponding values.";
            }
         }
      }
   }

   public class ListOptionModel : OptionModel
   {
      private readonly System.Reflection.ConstructorInfo _listCtor;
      private readonly System.Reflection.MethodInfo _listAddMethod;
      private readonly String[] _delimiters;
      private readonly OptionModel _elementModel;
      private readonly Int32 _minOccurs;
      private readonly Int32 _maxOccurs;

      public ListOptionModel( String primaryName, OptionModel elementModel, String delimiter, Int32 maxOccurs, params String[] aliases )
         : this( primaryName, elementModel, delimiter, 0, maxOccurs, aliases )
      {

      }

      public ListOptionModel( String primaryName, OptionModel elementModel, String delimiter, Int32 minOccurs, Int32 maxOccurs, params String[] aliases )
         : this( primaryName, elementModel, new[] { delimiter }, minOccurs, maxOccurs, aliases )
      {

      }

      public ListOptionModel( String primaryName, OptionModel elementModel, String[] delimiters, Int32 maxOccurs, params String[] aliases )
         : this( primaryName, elementModel, delimiters, 0, maxOccurs, aliases )
      {

      }

      public ListOptionModel( String primaryName, OptionModel elementModel, String[] delimiters, Int32 minOccurs, Int32 maxOccurs, params String[] aliases )
         : base( typeof( String ), primaryName, aliases )
      {
         if ( elementModel == null )
         {
            throw new ArgumentNullException( "Element model" );
         }
         this._listCtor = typeof( List<> ).MakeGenericType( elementModel.OptionValueType ).GetConstructor( new Type[0] );
         this._listAddMethod = typeof( List<> ).MakeGenericType( elementModel.OptionValueType ).GetMethod( "Add", new[] { elementModel.OptionValueType } );
         var delims = delimiters.FilterNulls();
         if ( delims.Count == 0 )
         {
            delims.Add( "," );
         }
         this._delimiters = delims.ToArray();
         this._elementModel = elementModel;
         this._minOccurs = Math.Max( 0, minOccurs );
         this._maxOccurs = Math.Max( -1, maxOccurs );
      }

      public override void TryParseToTyped( String value, ref Object valueTyped, ref String errorMessage )
      {
         var items = value.Split( this._delimiters, StringSplitOptions.None );
         var list = this._listCtor.Invoke( null );
         foreach ( var item in items )
         {
            Object itemTyped = null;
            this._elementModel.TryParseToTyped( item, ref itemTyped, ref errorMessage );
            if ( errorMessage == null )
            {
               this._listAddMethod.Invoke( list, new[] { itemTyped } );
            }
            else
            {
               break;
            }
         }
         if ( errorMessage == null )
         {
            if ( items.Length < this._minOccurs )
            {
               errorMessage = "Too few elements in option, required " + this._minOccurs + " but was " + items.Length + ".";
            }
            else if ( this._maxOccurs != ApplicationParameterModel.MAX_UNLIMITED && items.Length > this._maxOccurs )
            {
               errorMessage = "Too many elements in option, maximum is " + this._maxOccurs + " but was " + items.Length + ".";
            }
         }
         valueTyped = list;
      }
   }

   public class SimpleApplicationParametersModel : ApplicationParametersModel
   {
      private readonly IList<String> _optionsWithDuplicates;
      private readonly OptionModel _helpOption;

      public SimpleApplicationParametersModel( String valuesModelName, OptionModel helpOption, OptionModel optionAndValueSeparator, OptionModel[] options, Int32 minValues, params String[] optionsWithDuplicatesAllowed )
         : this( null, valuesModelName, helpOption, optionAndValueSeparator, options, minValues, optionsWithDuplicatesAllowed )
      {

      }

      public SimpleApplicationParametersModel( String valuesModelName, OptionModel helpOption, OptionModel optionAndValueSeparator, OptionModel[] options, params String[] optionsWithDuplicatesAllowed )
         : this( null, valuesModelName, helpOption, optionAndValueSeparator, options, optionsWithDuplicatesAllowed )
      {

      }

      public SimpleApplicationParametersModel( String valuesModelName, OptionModel helpOption, OptionModel[] options, params String[] optionsWithDuplicatesAllowed )
         : this( null, valuesModelName, helpOption, null, options, optionsWithDuplicatesAllowed )
      {

      }
      public SimpleApplicationParametersModel( String valuesModelName, OptionModel helpOption, params OptionModel[] options )
         : this( null, valuesModelName, helpOption, null, options, null )
      {

      }

      public SimpleApplicationParametersModel( AbstractOptionParser parser, String valuesModelName, OptionModel helpOption, OptionModel optionAndValueSeparator, OptionModel[] options, params String[] optionsWithDuplicatesAllowed )
         : this( parser, valuesModelName, helpOption, optionAndValueSeparator, options, 1, optionsWithDuplicatesAllowed )
      {

      }

      public SimpleApplicationParametersModel( AbstractOptionParser parser, String valuesModelName, OptionModel helpOption, OptionModel optionAndValueSeparator, OptionModel[] options, Int32 minValues, params String[] optionsWithDuplicatesAllowed )
         : base(
         parser,
         new ApplicationParameterModel( "Options", false, ApplicationParameterModel.MAX_UNLIMITED, options.Concat( new[] { helpOption } ).ToArray() ),
         optionAndValueSeparator == null ? null : new ApplicationParameterModel( optionAndValueSeparator.PrimaryName, false, 1, 1, optionAndValueSeparator ),
         new ApplicationParameterModel( valuesModelName, true, Math.Max( 1, minValues ), ApplicationParameterModel.MAX_UNLIMITED ) )
      {
         this._optionsWithDuplicates = new ReadOnlyCollection<String>( optionsWithDuplicatesAllowed.FilterNulls() );
         this._helpOption = helpOption;
      }

      public IList<String> OptionsWithDuplicatesAllowed
      {
         get
         {
            return this._optionsWithDuplicates;
         }
      }

      public ApplicationParameterModel OptionAndValueSeparator
      {
         get
         {
            return this.Parameters.Count > 2 ? this.Parameters[1] : null;
         }
      }

      public ApplicationParameterModel OptionModel
      {
         get
         {
            return this.Parameters[0];
         }
      }

      public ApplicationParameterModel ValueModel
      {
         get
         {
            return this.Parameters[this.Parameters.Count - 1];
         }
      }

      public OptionModel HelpOption
      {
         get
         {
            return this._helpOption;
         }
      }
   }
}

public static partial class E_ApplicationParameters
{
   public static IList<T> FilterNulls<T>( this T[] array )
      where T : class
   {
      return array == null ? new List<T>( 0 ) { } : array.Where( t => t != null ).ToList();
   }
}