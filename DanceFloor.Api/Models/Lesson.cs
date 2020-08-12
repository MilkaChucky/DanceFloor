using System;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DanceFloor.Api.Models
{
    [DataContract]
    [BsonKnownTypes(typeof(DanceLesson), typeof(BallroomDanceLesson))]
    [System.Text.Json.Serialization.JsonConverter(typeof(AsRuntimeTypeConverter<Lesson>))]
    [BsonIgnoreExtraElements]
    public abstract class Lesson
    {
        [DataMember]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [DataMember]
        [BsonElement("teacher")]
        public string Teacher { get; set; }

        [DataMember]
        [BsonElement("starts_at")]
        public TimeSpan StartsAt { get; set; }
        
        [DataMember]
        [BsonElement("ends_at")]
        public TimeSpan EndsAt { get; set; }
    }

    public class AsRuntimeTypeConverter<T> : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<T>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var valueType = value?.GetType() ?? typeof(object);
            
            if (typeof(T) == valueType)
            {
                // var dict =  Newtonsoft.Json.JsonConvert.SerializeObject(value))
                //
                // writer.WriteStartObject();
                // foreach ( var property in contract.Properties )
                // {
                //     writer.WritePropertyName( property.PropertyName );
                //     writer.WriteValue( property.ValueProvider.GetValue(value));
                // }
                // writer.WriteEndObject();
                
                return;
            }
            
            JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(object), options);
        }
    }
}