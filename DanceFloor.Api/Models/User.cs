using System;
using System.Runtime.Serialization;
using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace DanceFloor.Api.Models
{
    [DataContract]
    [CollectionName("users")]
    public class User : MongoIdentityUser<ObjectId>
    {
        public string Name { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
    }
}