using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Yle.Fi
{
    internal static class Extensions
    {
        internal static WebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
        {
            return new WebRequestAwaiter(asyncOp);
        }

        [CanBeNull]
        internal static T JsonTo<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        internal static string SubstringIfNecessary([CanBeNull] this string value, int count)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Length > count ? value.Substring(0, count - 2) + "..." : value;
        }
    }
}