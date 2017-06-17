using Microsoft . AspNetCore . Mvc . ModelBinding;
using System;
using System . Collections . Generic;
using System . ComponentModel;
using System . Linq;
using System . Threading . Tasks;

namespace Library.API.Helpers
{
    public class ArrayModelBinder : IModelBinder
    {
        public Task BindModelAsync ( ModelBindingContext bindingContext )
        {
            //As our binding only works on IEnumrable type
            if ( !bindingContext . ModelMetadata . IsEnumerableType )
            {
                bindingContext . Result = ModelBindingResult . Failed();
                return Task . CompletedTask;
            }

            //Get the inputted value through value provider
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ToString();

            //If that value is whitespace or null, we return null
            if ( string . IsNullOrWhiteSpace ( value ) )
            {
                bindingContext . Result = ModelBindingResult . Success ( null );
                return Task . CompletedTask;
            }

            //The value isn't null or whitespace,
            //and the type of model is enumerable.
            //Get the enumerabe's type, and a convertor
            var elementType = bindingContext.ModelType.GetType().GenericTypeArguments[0];
            var convertor = TypeDescriptor.GetConverter(elementType);

            //Convert each item in the value list to enumerable type
            var values = value
                         .Split(new[] {","},StringSplitOptions.RemoveEmptyEntries)
                         .Select(x => convertor.ConvertFromString(x.Trim()))
                         .ToArray();

            //Create an array of that type, and set it as Model value
            var typedValues = Array.CreateInstance(elementType,values.Length);
            values . CopyTo ( typedValues , 0 );
            bindingContext . Model = typedValues;

            //return a successful result passing the Model
            bindingContext . Result = ModelBindingResult . Success ( typedValues );
            return Task . CompletedTask;
                            
        }
    }
}
