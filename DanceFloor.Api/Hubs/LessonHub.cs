using System.Threading.Tasks;
using DanceFloor.Api.Models;
using Microsoft.AspNetCore.SignalR;

namespace DanceFloor.Api.Hubs
{
    public class LessonHub : Hub<ILessonHubClient>
    {
    }

    public interface ILessonHubClient
    {
        public void UpdateLesson(Lesson lesson);
    }
}