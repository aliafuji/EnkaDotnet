using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnkaDotNet.Utils.Common
{
    public class NewtonsoftStringOrNumberConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
            {
                return reader.Value.ToString();
            }
            if (reader.TokenType == JsonToken.Float)
            {
                return reader.Value.ToString();
            }
            if (reader.TokenType == JsonToken.String)
            {
                return reader.Value;
            }
            if (reader.TokenType == JsonToken.Null)
            {
                return string.Empty;
            }

            throw new JsonSerializationException($"Unexpected token type: {reader.TokenType}");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue((string)value);
        }

        public override bool CanWrite => true;
    }
}