using System;
using System.Runtime.Serialization;
using DanceFloor.Api.Converters;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

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
        public ObjectId Id { get; set; }
        
        [DataMember]
        [BsonElement("teacher")]
        public ObjectId Teacher { get; set; }
        
        [DataMember]
        [BsonElement("starts_at")]
        public TimeSpan StartsAt { get; set; }
        
        [DataMember]
        [BsonElement("ends_at")]
        public TimeSpan EndsAt { get; set; }
    }
}