using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DanceFloor.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace DanceFloor.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserManager<User> _userManager;

        public UserController(
            ILogger<UserController> logger,
            UserManager<User> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet("/userinfo")]
        public ActionResult GetUserInfo([FromQuery] IEnumerable<ObjectId> userIds)
        {
            try
            {
                var userInfos = _userManager.Users
                    .Where(user => userIds.Contains(user.Id))
                    .Select(user => new UserInfo
                    {
                        Id = user.Id,
                        Name = user.Name
                    })
                    .ToList();

                if (userIds.Count() != userInfos.Count)
                    return BadRequest(new {Reason = "Some of the user identifiers are not valid!"});

                return Ok(userInfos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserInfo exception");
                throw;
            }
        }
    }
}