using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Linq.AI.OpenAI
{

    public class StructuredSchemaGenerator
    {
        public static JObject FromType<Type>()
        {
            return FromType(typeof(Type));
        }

        public static JObject FromType(Type type)
        {
            return GetSchema(type);
        }

        private static JObject GetSchema(Type type)
        {
            var schema = new JObject();

            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
                type = nullableType;

            if (type == typeof(string))
            {
                schema["type"] = "string";
            }
            else if (type == typeof(byte) || type == typeof(sbyte) ||
                     type == typeof(Int16) || type == typeof(UInt16) ||
                     type == typeof(Int32) || type == typeof(UInt32) ||
                     type == typeof(Int64) || type == typeof(UInt64))
            {
                schema["type"] = "integer";
            }
            else if (type == typeof(float) || type == typeof(double))
            {
                schema["type"] = "number";
            }
            else if (type == typeof(bool))
            {
                schema["type"] = "boolean";
            }
            else if (type == typeof(DateOnly))
            {
                schema["type"] = "string";
                //schema["format"] = "date";
            }
            else if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            {
                schema["type"] = "string";
                //schema["format"] = "date-time";
            }
            else if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                schema["type"] = "array";
                var itemType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
                schema["items"] = GetSchema(itemType!);
            }
            else
            {

                var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                schema["type"] = "object";
                schema["additionalProperties"] = false;
                schema["required"] = JArray.FromObject(props.Select(s => s.Name));
                var propertiesSchema = new JObject();
                foreach (var prop in props)
                {
                    propertiesSchema[prop.Name] = GetSchema(prop.PropertyType);
                }
                schema["properties"] = propertiesSchema;

            }

            return schema;
        }
    }

}
