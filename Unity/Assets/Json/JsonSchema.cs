#nullable enable

using System.Collections.Generic;
using Newtonsoft.Json;

namespace NeuroSdk.Json
{
    public sealed class JsonSchema
    {
        [JsonIgnore]
        public Dictionary<string, JsonSchema> Properties
        {
            get => _properties ??= new Dictionary<string, JsonSchema>();
            set => _properties = value;
        }

        [JsonIgnore]
        public JsonSchemaType Type
        {
            get
            {
                return _type switch
                {
                    "string" => JsonSchemaType.String,
                    "number" => JsonSchemaType.Float,
                    "integer" => JsonSchemaType.Integer,
                    "boolean" => JsonSchemaType.Boolean,
                    "object" => JsonSchemaType.Object,
                    "array" => JsonSchemaType.Array,
                    "null" => JsonSchemaType.Null,
                    _ => JsonSchemaType.None
                };
            }
            set
            {
                _type = value switch
                {
                    JsonSchemaType.String => "string",
                    JsonSchemaType.Float => "number",
                    JsonSchemaType.Integer => "integer",
                    JsonSchemaType.Boolean => "boolean",
                    JsonSchemaType.Object => "object",
                    JsonSchemaType.Array => "array",
                    JsonSchemaType.Null => "null",
                    _ => null
                };
            }
        }

        [JsonIgnore]
        public List<object> Enum
        {
            get => _enum ??= new List<object>();
            set => _enum = value;
        }

        [JsonIgnore]
        public List<string> Required
        {
            get => _required ??= new List<string>();
            set => _required = value;
        }

        #region Keywords

        [JsonProperty("properties")]
        private Dictionary<string, JsonSchema>? _properties;

        [JsonProperty("items")]
        public JsonSchema? Items { get; set; }

        [JsonProperty("type")]
        private string? _type;

        [JsonProperty("enum")]
        private List<object>? _enum;

        [JsonProperty("const")]
        public object? Const { get; set; }

        [JsonProperty("minLength")]
        public int? MinLength { get; set; }

        [JsonProperty("pattern")]
        public string? Pattern { get; set; }

        [JsonProperty("maxLength")]
        public int? MaxLength { get; set; }

        [JsonProperty("maximum")]
        public float? Maximum { get; set; }

        [JsonProperty("exclusiveMinimum")]
        public float? ExclusiveMinimum { get; set; }

        [JsonProperty("exclusiveMaximum")]
        public float? ExclusiveMaximum { get; set; }

        [JsonProperty("minimum")]
        public float? Minimum { get; set; }

        [JsonProperty("required")]
        private List<string>? _required;

        [JsonProperty("minItems")]
        public int? MinItems { get; set; }

        [JsonProperty("maxItems")]
        public int? MaxItems { get; set; }

        [JsonProperty("uniqueItems")]
        public bool? UniqueItems { get; set; }

        [JsonProperty("format")]
        public string? Format { get; set; }

        #endregion
    }
}
