using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace DanceFloor.Api.Models
{
    [DataContract]
    public class CustomUser : User
    {
        [DataMember]
        [BsonElement("email")]
        public override string Email { get; set; }
        
        [JsonIgnore]
        [BsonElement("passwordHash")]
        public override string PasswordHash { get; set; }
    }
}