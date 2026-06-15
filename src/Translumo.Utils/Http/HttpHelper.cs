using System;
using System.Reflection;
using System.Text;

namespace Translumo.Utils.Http
{
    public static class HttpHelper
    {
        public static string BuildFormData<TEntity>(TEntity bodyEntity)
            where TEntity : class
        {
            var result = new StringBuilder();
            foreach (var propertyInfo in typeof(TEntity).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
            {
                var value = propertyInfo.GetValue(bodyEntity)?.ToString();
                if (value != null)
                {
                    result.Append($"{GetFormDataPropertyName(propertyInfo.Name)}={Uri.EscapeDataString(value)}");
                    result.Append("&");
                }
            }

            if (result.Length > 0)
            {
                result.Remove(result.Length - 1, 1);
            }

            return result.ToString();
        }

        public static string BuildQueryString<TEntity>(TEntity queryParamsEntity)
            where TEntity : class
        {
            return "?" + BuildFormData(queryParamsEntity);
        }

        private static string GetFormDataPropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || Char.IsLower(propertyName[0]))
            {
                return propertyName;
            }

            return char.ToLower(propertyName[0]) + (propertyName.Length > 1 ? propertyName.Substring(1) : string.Empty);
        }
    }
}
