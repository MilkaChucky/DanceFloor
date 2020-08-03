using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace DanceFloor.Api.Models
{
    [DataContract]
    public class UserCredentials
    {
        [DataMember]
        [Required]
        public string Email { get; set; }
        
        [DataMember]
        [Required]
        public string Password { get; set; }
    }
}