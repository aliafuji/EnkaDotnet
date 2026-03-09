using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Serialization
{
    /// <summary>
    /// Accepts both JSON strings and JSON numbers, always deserializes as <see cref="string"/>.
    /// This handles API responses where fields like <c>setName</c> or <c>name</c> can be
    /// returned as either a quoted string or a bare integer
    /// </summary>
    internal sealed class StringOrNumberConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString();
                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out long l))
                        return l.ToString();
                    if (reader.TryGetDouble(out double d))
                        return d.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    return string.Empty;
                case JsonTokenType.Null:
                    return null;
                default:
                    reader.Skip();
                    return string.Empty;
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
