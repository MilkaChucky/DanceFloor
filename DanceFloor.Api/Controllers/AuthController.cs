using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DanceFloor.Api.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DanceFloor.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IMongoCollection<User> _users;

        public AuthController(
            ILogger<AuthController> logger,
            IMongoCollection<User> users)
        {
            _logger = logger;
            _users = users;
        }

        [HttpPost("/register/social")]
        public ActionResult<SocialUser> RegisterSocial([FromBody] SocialUser socialUser)
        {
            var userExists = _users
                .OfType<SocialUser>()
                .FindSync(user => user.Id == socialUser.Id && user.Provider == socialUser.Provider)
                .Any();

            if (userExists) return BadRequest("User is already registered!");
            
            _users.InsertOne(socialUser);

            return socialUser;
        }
        
        [HttpPost("/register")]
        public ActionResult<CustomUser> Register([FromBody] UserCredentials credentials)
        {
            var userExists = _users
                .OfType<CustomUser>()
                .FindSync(user => user.Email == credentials.Email)
                .Any();

            if (userExists) return BadRequest("User is already registered!");
            
            var customUser = new CustomUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = credentials.Email,
                PasswordHash = HashPassword(credentials.Password)
            };

            _users.InsertOne(customUser);
            
            return customUser;
        }

        [HttpPost("/login")]
        public ActionResult<CustomUser> Login([FromBody] UserCredentials credentials)
        {
            var match = _users
                .OfType<CustomUser>()
                .FindSync(user => user.Email == credentials.Email && user.PasswordHash == HashPassword(credentials.Password))
                .SingleOrDefault();
            
            if (match == null) return Unauthorized("Invalid email or password! Have you registered?");

            return match;
        }

        private string HashPassword(string password)
        {
            var hashed = KeyDerivation.Pbkdf2(
                password,
                new byte[128 / 8],
                KeyDerivationPrf.HMACSHA512,
                10000,
                256 / 8);

            return Convert.ToBase64String(hashed);
        }
    }
}