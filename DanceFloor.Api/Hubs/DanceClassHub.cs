using System.Threading.Tasks;
using DanceFloor.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;

namespace DanceFloor.Api.Hubs
{
    [Authorize]
    public class DanceClassHub : Hub<IDanceClassHubClient> { }

    public interface IDanceClassHubClient
    {
        public Task ReceiveApplication(ObjectId classId, UserInfo userInfo, ObjectId? pairId);
        public Task ReceiveCancellation(ObjectId classId, ObjectId userId);
    }
}