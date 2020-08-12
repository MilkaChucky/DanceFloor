using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DanceFloor.Api.Models
{
    [DataContract]
    public class DanceHall
    {
        [DataMember]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [DataMember]
        [Required]
        [BsonElement("address")]
        public string Address { get; set; }
        
        [DataMember]
        [BsonElement("room")]
        public string Room { get; set; }
        
        [DataMember]
        [BsonElement("lessons")]
        public List<Lesson> Lessons { get; set; }
    }
}