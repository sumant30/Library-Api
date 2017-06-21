using System;
using System . Collections . Generic;
using System . Dynamic;
using System . Linq;
using System . Reflection;
using System . Threading . Tasks;

namespace Library.API.Helpers
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<ExpandoObject> ShapeData<TSource> ( this IEnumerable<TSource> source , string fields )
        {
            if ( source == null )
            {
                throw new ArgumentNullException ( nameof ( source ) );
            }

            //create a list to hold our ExpandoObjects
            var expandoObjectList = new List<ExpandoObject>();

            //create a list with PropertyInfo objects on TSource. Reflection is
            //expensive, so rather than doing it for each object in list, we do
            //it once and resuse the results. after all part of reflection is on
            //type of object (TSource), not on instance.
            var propertyInfoList = new List<PropertyInfo>();

            if ( string . IsNullOrWhiteSpace ( fields ) )
            {
                var propertyInfos = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                propertyInfoList . AddRange ( propertyInfos );
            }
            else
            {
                //only public properties that match fields should be
                //in the ExpandoObject

                var fieldsAfterSplit = fields.Split(',');

                foreach ( var field in fieldsAfterSplit )
                {
                    var propertyName = field.Trim();

                    var propertyInfos = typeof(TSource).GetProperty(propertyName,BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if(propertyInfos == null )
                    {
                        throw new Exception ( $"Property {propertyName} wasn't found on {typeof ( TSource )}" );
                    }

                    propertyInfoList . Add ( propertyInfos );
                }
            }

            //run through source objects
            foreach (TSource sourceObject in source)
            {
                //create an exapndo object that will hold
                //selected properties & values
                var dataShapedObject = new ExpandoObject();

                //Get the value of each property we have to return. For that,
                //we run through the list
                foreach (var propertyInfo in propertyInfoList)
                {
                    //get value returns the value of the property on the source object
                    var propertyValue = propertyInfo.GetValue(propertyInfo);

                    //ad the field to exapndo object
                    ( ( IDictionary<string , object> ) dataShapedObject ) . Add ( propertyInfo . Name , propertyValue );
                }

                expandoObjectList . Add ( dataShapedObject );
            }
            return expandoObjectList;
        }
    }
}
