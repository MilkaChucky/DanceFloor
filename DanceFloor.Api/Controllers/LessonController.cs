using System.Collections.Generic;
using System.Linq;
using DanceFloor.Api.Extensions;
using DanceFloor.Api.Hubs;
using DanceFloor.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace DanceFloor.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class LessonController : ControllerBase
    {
        private readonly ILogger<LessonController> _logger;
        private readonly IMongoCollection<DanceHall> _danceHalls;
        private readonly IHubContext<LessonHub, ILessonHubClient> _lessonHub;

        public LessonController(
            ILogger<LessonController> logger,
            IMongoCollection<DanceHall> danceHalls,
            IHubContext<LessonHub, ILessonHubClient> lessonHub)
        {
            _logger = logger;
            _danceHalls = danceHalls;
            _lessonHub = lessonHub;
        }

        // [AllowAnonymous]
        // [HttpPost("/apply/lesson/{lessonId}")]
        // public ActionResult<Lesson> ApplyForDanceLesson(string lessonId)
        // {
        //     var danceHall = _danceHalls
        //         .FindSync(dh => dh.Lessons != null && dh.Lessons.Any(l => l.Id == lessonId))
        //         .FirstOrDefault();
        //
        //     if (danceHall == null) return NotFound(new {Reason = "There is no lesson with the specified Id!"});
        //     
        //     var lesson = danceHall.Lessons
        //         .FirstOrDefault(l => l.Id == lessonId);
        //
        //     var user = HttpContext.Session.Get<User>("user");
        //     
        //     switch (lesson)
        //     {
        //         case DanceLesson danceLesson when !danceLesson.Dancers.Contains(user.Id):
        //             danceLesson.Dancers.Add(user.Id);
        //             break;
        //         case BallroomDanceLesson danceLesson when !danceLesson.Pairs.Any(p => p.Contains(user.Id)):
        //             danceLesson.Pairs.Add(new List<string> { user.Id });
        //             break;
        //         default:
        //             return NotFound(new {Reason = "You already applied for this lesson!"});
        //     }
        //     
        //     _lessonHub.Clients.All.UpdateLesson(lesson);
        //     
        //     return lesson;
        // }
        //
        // [AllowAnonymous]
        // [HttpPost("/apply/lesson/{lessonId}/{pairId}")]
        // public ActionResult<BallroomDanceLesson> ApplyForDanceLesson(string lessonId, string pairId)
        // {
        //     var danceHall = _danceHalls
        //         .FindSync(dh => dh.Lessons.Any(l => l.Id == lessonId))
        //         .FirstOrDefault();
        //
        //     var lessons = danceHall.Lessons
        //         .OfType<BallroomDanceLesson>()
        //         .ToList();
        //
        //     var user = HttpContext.Session.Get<User>("user");
        //
        //     if (lessons.Any(l => l.Id == lessonId && l.Pairs.SelectMany(p => p).Contains(user.Id)))
        //         return BadRequest(new {Reason = "You already applied for this lesson!"});
        //     
        //     var lesson = lessons
        //         .FirstOrDefault(l 
        //             => l.Id == lessonId &&
        //                l.Pairs != null &&
        //                l.Pairs.Any(p => p.Count == 1 && p.Contains(pairId)));
        //
        //     if (lesson == null) return BadRequest(new {Reason = ""});
        //
        //     lesson.Pairs.FirstOrDefault()?.Add(user.Id);
        //     _lessonHub.Clients.All.UpdateLesson(lesson);
        //     
        //     return lesson;
        // }
    }
}