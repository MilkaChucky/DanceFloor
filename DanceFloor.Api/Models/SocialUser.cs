using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DanceFloor.Api.Models
{
    [DataContract]
    public class SocialUser : User
    {
        [DataMember]
        [Required]
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public new string Id { get; set; }
        
        [DataMember]
        [Required]
        [BsonElement("provider")]
        public string Provider { get; set; }
    }
}