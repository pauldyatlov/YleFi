using UnityEngine.Networking;

namespace Yle.Fi
{
    public static class ExtensionMethods
    {
        public static WebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
        {
            return new WebRequestAwaiter(asyncOp);
        }
    }
}