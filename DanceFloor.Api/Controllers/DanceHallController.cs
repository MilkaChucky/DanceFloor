using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DanceFloor.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DanceFloor.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class DanceHallController : ControllerBase
    {
        private readonly ILogger<DanceHallController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IMongoCollection<DanceHall> _danceHalls;
        
        public DanceHallController(
            ILogger<DanceHallController> logger,
            IMongoCollection<DanceHall> danceHalls, UserManager<User> userManager)
        {
            _logger = logger;
            _danceHalls = danceHalls;
            _userManager = userManager;
        }

        // [AllowAnonymous]
        [HttpGet("/dance_halls")]
        public async Task<ActionResult<IEnumerable<DanceHall>>> GetAll()
        {
            var user = await _userManager.GetUserAsync(User);
            
            var result = _danceHalls.AsQueryable()
                // .ToList()
                // .Select(dh =>
                // {
                //     var lessons = dh.Lessons.Select(lesson => new Lesson {Id = lesson.Id, Teacher = lesson.Teacher, StartsAt = lesson.StartsAt, EndsAt = lesson.EndsAt}).ToList();
                //
                //     dh.Lessons = lessons;
                //     
                //     return dh;
                // })
                .ToList();

            return result;
        }
    }
}