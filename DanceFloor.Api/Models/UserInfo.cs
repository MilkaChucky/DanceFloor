using MongoDB.Bson;

namespace DanceFloor.Api.Models
{
    public class UserInfo
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
    }
}