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

        public static T JsonTo<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string SubstringIfNecessary(this string @string, int count)
        {
            if (string.IsNullOrEmpty(@string))
                return string.Empty;

            return @string.Length > count ? @string.Substring(0, count - 2) + "..." : @string;
        }
    }
}