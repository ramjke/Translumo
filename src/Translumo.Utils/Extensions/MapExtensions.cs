using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Translumo.Utils.Extensions
{
    public static class MapExtensions
    {
        /// <summary>
        /// Copy all matched properties to new instance of TDestination object. It uses default type converter for type casting
        /// </summary>
        /// <typeparam name="TSource">Source class type</typeparam>
        /// <typeparam name="TDestination">Destination class type</typeparam>
        /// <param name="source">Source object</param>
        /// <returns></returns>
        public static TDestination MapTo<TSource, TDestination>(this TSource source)
            where TDestination : class
            where TSource : class
        {
            var sourceProperties = typeof(TSource)
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            var destinationProperties = typeof(TDestination)
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

            var resultObject = Activator.CreateInstance<TDestination>();

            foreach (PropertyInfo sourceProperty in sourceProperties)
            {
                var destinationProperty = destinationProperties.FirstOrDefault(pr => pr.Name == sourceProperty.Name);
                object? value = sourceProperty.GetValue(source);
                if (destinationProperty == null || value == null || destinationProperty.GetSetMethod() == null)
                {
                    continue;
                }

                if (sourceProperty.PropertyType == destinationProperty.PropertyType)
                {
                    destinationProperty.SetValue(resultObject, value);
                }
                else
                {
                    var converter = TypeDescriptor.GetConverter(sourceProperty.PropertyType);
                    if (converter.CanConvertTo(destinationProperty.PropertyType))
                    {
                        destinationProperty.SetValue(resultObject, converter.ConvertTo(value, destinationProperty.PropertyType));
                    }
                    else
                    {
                        converter = TypeDescriptor.GetConverter(destinationProperty.PropertyType);
                        destinationProperty.SetValue(resultObject, converter.ConvertFrom(value));
                    }
                }
            }

            return resultObject;
        }

        /// <summary>
        /// Copy all matched properties to destination object. It uses default type converter for type casting
        /// </summary>
        /// <typeparam name="TEntity">Source class type</typeparam>
        /// <param name="source">Source object</param>
        /// <param name="destination">Destination object</param>
        public static void MapTo<TEntity>(this TEntity source, TEntity destination)
            where TEntity : class
        {
            var sourceProperties = source.GetType()
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo propertyInfo in sourceProperties)
            {
                object? value = propertyInfo.GetValue(source);
                if (propertyInfo.GetCustomAttribute<MapInternalAttribute>() != null)
                {
                    var destinationValue = propertyInfo.GetValue(destination);
                    if (value != null && destinationValue != null)
                    {
                        value.MapTo(destinationValue);
                        continue;
                    }
                }
                if (propertyInfo.GetSetMethod() == null)
                {
                    continue;
                }

                propertyInfo.SetValue(destination, value);
            }
        }
    }
}
