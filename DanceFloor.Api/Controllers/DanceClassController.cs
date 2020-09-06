using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DanceFloor.Api.Hubs;
using DanceFloor.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DanceFloor.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class DanceClassController : ControllerBase
    {
        private readonly ILogger<DanceClassController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IMongoCollection<DanceHall> _danceHalls;
        private readonly IHubContext<DanceClassHub, IDanceClassHubClient> _danceClassHub;

        public DanceClassController(
            ILogger<DanceClassController> logger,
            UserManager<User> userManager,
            IMongoCollection<DanceHall> danceHalls,
            IHubContext<DanceClassHub, IDanceClassHubClient> danceClassHub)
        {
            _logger = logger;
            _userManager = userManager;
            _danceHalls = danceHalls;
            _danceClassHub = danceClassHub;
        }

        [HttpGet("/classes")]
        public ActionResult GetClasses([FromQuery] ObjectId danceHall)
        {
            var classes = _danceHalls.AsQueryable()
                .FirstOrDefault(dh => dh.Id == danceHall)
                ?.Classes;

            if (classes == null)
                return BadRequest(new { Reason = "The dance hall does not exist!" });

            return Ok(classes);
        }
        
        [HttpPost("/classes/{classId}/join")]
        public async Task<ActionResult> ApplyForClass([FromRoute] ObjectId classId, ObjectId? pairId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                    return Unauthorized();

                // var filterDef = Builders<DanceHall>.Filter
                //     .ElemMatch(x => x.Classes, x => x.Id == classId);

                var cursor = await _danceHalls
                    .FindAsync(dh => dh.Classes.Any(c => c.Id == classId));
                    // .Find(filterDef)
                    // .FirstOrDefault();

                var danceHall = await cursor.FirstOrDefaultAsync();

                if (danceHall == null)
                    return BadRequest(new { Reason = "There is no class with the given ID!" });

                var danceClass = danceHall.Classes.First(c => c.Id == classId);

                switch (danceClass)
                {
                    case GroupDanceClass groupDanceClass:
                        if (groupDanceClass.Dancers.Contains(user.Id))
                            return BadRequest(new { Reason = "You already applied for this class!" });
                        
                        groupDanceClass.Dancers.Add(user.Id);
                        break;
                    case BallroomDanceClass ballroomDanceClass:
                        if (ballroomDanceClass.Pairs.Any(p => p.Contains(user.Id)))
                            return BadRequest(new { Reason = "You already applied for this class!" });
                        
                        if (pairId.HasValue)
                        {
                            var pair = ballroomDanceClass.Pairs.FirstOrDefault(p => p.Contains(pairId.Value) && p.Count == 1);

                            if (pair == null)
                                return BadRequest(new { Reason = "The user either does not exist or already has a pair!" });

                            pair.Add(user.Id);
                        }
                        else
                        {
                            ballroomDanceClass.Pairs.Add(new List<ObjectId> { user.Id });
                        }

                        break;
                }

                var updateDef = Builders<DanceHall>.Update
                    .Set(hall => hall.Classes, danceHall.Classes);

                await _danceHalls.FindOneAndUpdateAsync(dh => dh.Id == danceHall.Id, updateDef);

                var userInfo = new UserInfo
                {
                    Id = user.Id,
                    Name = user.Name
                };
                
                await _danceClassHub.Clients.All.ReceiveApplication(classId, userInfo, pairId);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApplyApplication exception");
                throw;
            }
        }
        
        [HttpPost("/classes/{classId}/leave")]
        public async Task<ActionResult> CancelApplication([FromRoute] ObjectId classId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                    return Unauthorized();
            
                // var filterDef = Builders<DanceHall>.Filter
                //     .ElemMatch(x => x.Classes, x => x.Id == classId);

                var cursor = await _danceHalls
                    .FindAsync(dh => dh.Classes.Any(c => c.Id == classId));
                
                var danceHall = await cursor.FirstOrDefaultAsync();

                if (danceHall == null)
                    return BadRequest(new { Reason = "There is no class with the given ID!" });
            
                var danceClass = danceHall.Classes.First(c => c.Id == classId);
                var found = false;
            
                switch (danceClass)
                {
                    case GroupDanceClass groupDanceClass:
                        found = groupDanceClass.Dancers.Remove(user.Id);
                        break;
                    case BallroomDanceClass ballroomDanceClass:
                        found = ballroomDanceClass.Pairs
                            .FirstOrDefault(p => p.Contains(user.Id))
                            ?.Remove(user.Id) ?? false;

                        if (found)
                            ballroomDanceClass.Pairs = ballroomDanceClass.Pairs
                                .Where(pair => pair.Any())
                                .ToList();
                        break;
                }
            
                if (!found)
                    return BadRequest(new { Reason = "You haven't event even applied for this class!" });
            
                var updateDef = Builders<DanceHall>.Update
                    .Set(hall => hall.Classes, danceHall.Classes);
            
                await _danceHalls.FindOneAndUpdateAsync(dh => dh.Id == danceHall.Id, updateDef);
            
                await _danceClassHub.Clients.All.ReceiveCancellation(classId, user.Id);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CancelApplication exception");
                throw;
            }
        }
    }
}