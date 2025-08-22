#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace NeuroSdk.Json
{
    /// <summary>
    /// Utility class for generating quick JSON schemas
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class QJS
    {
        private static JsonSchema Const<T>(T value)
        {
            return new JsonSchema
            {
                Const = value
            };
        }
        private static JsonSchema Enum<T>(IEnumerable<T> values)
        {
            return new JsonSchema
            {
                Enum = values.Cast<object>().ToList()
            };
        }

        public static JsonSchema Const(string value) => Const<string>(value);
        public static JsonSchema Const(int value) => Const<int>(value);
        public static JsonSchema Const(float value) => Const<float>(value);
        public static JsonSchema Const(bool value) => Const<bool>(value);

        public static JsonSchema Const(IEnumerable<string> values) => Const<IEnumerable<string>>(values);
        public static JsonSchema Const(IEnumerable<int> values) => Const<IEnumerable<int>>(values);
        public static JsonSchema Const(IEnumerable<float> values) => Const<IEnumerable<float>>(values);
        public static JsonSchema Const(IEnumerable<bool> values) => Const<IEnumerable<bool>>(values);

        public static JsonSchema ConstEmptyArray => Const(Array.Empty<object>());
        public static JsonSchema ConstNull => Enum(new object?[] { null });

        public static JsonSchema Enum(IEnumerable<string> values) => Enum<string>(values);
        public static JsonSchema Enum(IEnumerable<int> values) => Enum<int>(values);
        public static JsonSchema Enum(IEnumerable<float> values) => Enum<float>(values);

        public static JsonSchema Type(JsonSchemaType type)
        {
            return new JsonSchema
            {
                Type = type
            };
        }

        public static JsonSchema WrapObject(IReadOnlyDictionary<string, JsonSchema> properties, bool makePropertiesRequired = true)
        {
            JsonSchema result = new()
            {
                Type = JsonSchemaType.Object,
                Properties = properties.ToDictionary(x => x.Key, x => x.Value)
            };

            if (makePropertiesRequired) result.Required = properties.Keys.ToList();

            return result;
        }
    }
}
