using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DanceFloor.Api.Models
{
    [DataContract]
    public class BallroomDanceLesson : Lesson
    {
        [DataMember]
        [BsonElement("pairs")]
        public List<List<string>> Pairs { get; set; }
    }
}