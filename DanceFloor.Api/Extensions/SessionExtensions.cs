using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace DanceFloor.Api.Extensions
{
    public static class SessionExtensions
    {
        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return JsonConvert.DeserializeObject<T>(value);
        }

        public static void Set<T>(this ISession session, string key, T value)
        {
            var serialized = JsonConvert.SerializeObject(value);
            session.SetString(key, serialized);
        }
    }
}