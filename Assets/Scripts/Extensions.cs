using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Yle.Fi
{
    public static class Extensions
    {
        public static WebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
        {
            return new WebRequestAwaiter(asyncOp);
        }

        public static string ToJson<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T JsonTo<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}