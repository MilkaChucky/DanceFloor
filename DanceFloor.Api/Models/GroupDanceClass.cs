using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DanceFloor.Api.Models
{
    [DataContract]
    public class GroupDanceClass : Class
    {
        [DataMember]
        [BsonElement("dancers")]
        public List<ObjectId> Dancers { get; set; }
    }
}