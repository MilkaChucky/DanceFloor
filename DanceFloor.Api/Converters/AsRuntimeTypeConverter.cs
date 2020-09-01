using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanceFloor.Api.Converters
{
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