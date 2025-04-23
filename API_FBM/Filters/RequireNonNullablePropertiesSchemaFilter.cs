using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace API_FBM.Filters
{
    public class RequireNonNullablePropertiesSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties == null || context.Type == null)
                return;

            foreach (var property in context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propertyName = char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);
                if (!schema.Properties.ContainsKey(propertyName))
                    propertyName = property.Name;

                if (schema.Properties.ContainsKey(propertyName) && 
                    property.PropertyType.IsValueType && 
                    !IsNullable(property.PropertyType))
                {
                    // Помечаем значимые типы, не допускающие null, как обязательные
                    schema.Required.Add(propertyName);
                }
            }
        }

        private static bool IsNullable(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }
    }
} 