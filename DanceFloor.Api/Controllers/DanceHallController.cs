using System.Collections.Generic;
using System.Linq;
using DanceFloor.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace DanceFloor.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class DanceHallController : ControllerBase
    {
        private readonly ILogger<DanceHallController> _logger;
        private readonly IMongoCollection<DanceHall> _danceHalls;
        
        public DanceHallController(
            ILogger<DanceHallController> logger,
            IMongoCollection<DanceHall> danceHalls)
        {
            _logger = logger;
            _danceHalls = danceHalls;
        }
        
        [HttpGet("/dance_halls")]
        public ActionResult<IEnumerable<DanceHall>> GetAll()
        {
            return _danceHalls.AsQueryable().ToList();
        }
    }
}