using Newtonsoft.Json;
using UnityEngine.Networking;
using UnityEngine;

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

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();

            if (component == null)
                component = gameObject.AddComponent<T>();

            return component;
        }

        public static T GetOrAddComponent<T>(this MonoBehaviour component) where T : Component
        {
            return GetOrAddComponent<T>(component.gameObject);
        }
    }
}