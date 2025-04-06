using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Utils.Common
{
    public class JsonStringOrNumberConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt64(out long longValue))
                {
                    return longValue.ToString();
                }
                else if (reader.TryGetDouble(out double doubleValue))
                {
                    return doubleValue.ToString();
                }
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString() ?? string.Empty;
            }

            if (reader.TokenType == JsonTokenType.Null)
            {
                return string.Empty;
            }

            throw new JsonException($"Cannot convert {reader.TokenType} to {typeToConvert}");
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}