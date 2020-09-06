using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DanceFloor.Api.Models
{
    [DataContract]
    public class DanceHall
    {
        [DataMember]
        [BsonId]
        public ObjectId Id { get; set; }
        
        [DataMember]
        [Required]
        [BsonElement("address")]
        public string Address { get; set; }
        
        [DataMember]
        [BsonElement("room")]
        public string Room { get; set; }
        
        [DataMember]
        [BsonElement("classes")]
        public List<Class> Classes { get; set; }
    }
}