using Library . API . Services;
using System;
using System . Collections . Generic;
using System . Linq;
using System . Linq . Dynamic . Core;

namespace Library . API . Helpers
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T> ( this IQueryable<T> source , string orderBy , Dictionary<string , PropertyMappingValue> mappingDictionary )
        {
            if ( source == null )
            {
                throw new ArgumentNullException ( "source" );
            }

            if ( mappingDictionary == null )
            {
                throw new ArgumentNullException ( "mappingDictionary" );
            }

            if ( string . IsNullOrWhiteSpace ( orderBy ) )
            {
                return source;
            }

            //order by string is separated by ",", so we split it
            var orderByAfterSplit = orderBy.Split(',');

            //apply each orderby clause in reverse order - otherwise, the
            //IQueryable will be ordered in wrong order
            foreach ( var orderByClause in orderByAfterSplit . Reverse ( ) )
            {
                //trim the orderByClause, as it might contain leading
                //or trailing spaces. Can't trim in foreach,
                //so we use another var
                var trimmedOrderByClause = orderByClause.Trim();

                //if the trimmed clause ends with desc, we order
                //descending, otherwise ascending
                var orderDescending = trimmedOrderByClause.EndsWith(" desc");

                //remove " asc" or " desc" from order by clause, so we
                //get the property name to look for in mapping dictionary
                var indexOfFirstspace = trimmedOrderByClause.IndexOf(" ");
                var propertyName = indexOfFirstspace == -1 ? trimmedOrderByClause :trimmedOrderByClause.Remove(indexOfFirstspace);

                //find the matching property
                if ( !mappingDictionary . ContainsKey ( propertyName ) )
                {
                    throw new ArgumentException ( $"Key mapping for {propertyName} is missing." );
                }

                //get the property mapping value
                var propertyMappingValue = mappingDictionary[propertyName];

                if(propertyMappingValue == null )
                {
                    throw new ArgumentNullException ( nameof ( propertyMappingValue ) );
                }

                //run through the property names in reverse
                //so order by clause are applied in correct order
                foreach(var destinationProperty in propertyMappingValue . DestinationProperties . Reverse ( ) )
                {
                    //revert sort order if necessary
                    if ( propertyMappingValue . Revert )
                    {
                        orderDescending = !orderDescending;
                    }

                    source = source . OrderBy ( destinationProperty + ( orderDescending ? " descending" : " ascending" ) );
                }
            }
            return source;
        }
    }
}
