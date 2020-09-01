using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson;

namespace DanceFloor.Api.Converters
{
    public class ObjectIdConverter : JsonConverter<ObjectId>
    {
        public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new Exception($"Unexpected token parsing ObjectId. Expected String, got {reader.TokenType}.");

            var value = reader.GetString();
            return string.IsNullOrEmpty(value) ? ObjectId.Empty : new ObjectId(value);
        }

        public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value == ObjectId.Empty ? string.Empty : value.ToString());
        }
    }
}