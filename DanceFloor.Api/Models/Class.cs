using System;
using System.Runtime.Serialization;
using DanceFloor.Api.Converters;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DanceFloor.Api.Models
{
    [DataContract]
    [BsonKnownTypes(typeof(GroupDanceClass), typeof(BallroomDanceClass))]
    [System.Text.Json.Serialization.JsonConverter(typeof(AsRuntimeTypeConverter<Class>))]
    [BsonIgnoreExtraElements]
    public abstract class Class
    {
        [DataMember]
        [BsonId]
        public ObjectId Id { get; set; }
        
        [DataMember]
        [BsonElement("name")]
        public string Name { get; set; }
        
        [DataMember]
        [BsonElement("teacher")]
        public ObjectId Teacher { get; set; }
        
        [DataMember]
        [BsonElement("day_of_week")]
        public DayOfWeek DayOfWeek { get; set; }
        
        [DataMember]
        [BsonElement("starts_at")]
        public TimeSpan StartsAt { get; set; }
        
        [DataMember]
        [BsonElement("ends_at")]
        public TimeSpan EndsAt { get; set; }
    }
}