using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Networking;
using Task = System.Threading.Tasks.Task;

namespace Yle.Fi
{
    public sealed class Engine : MonoBehaviour
    {
        private const string URL_TEMPLATE =
            "https://external.api.yle.fi/v1/programs/items.json?limit={1}&offset={2}&app_id={3}&app_key={4}";

        private const int REQUEST_LIMIT = 10;

        private const string APP_ID = "dace39cd";
        private const string APP_KEY = "41d5031aabfc3f94c7eb54c7f987ab90";

        [SerializeField] private ScrollerPanel _uiController = default;

        private BindableList<ContentData> _tvProgramDatas;

        private int _requestOffset;
        private string _currentUrl;
        private bool _requesting;

        private void Awake()
        {
            _tvProgramDatas = new BindableList<ContentData>();

            _uiController.Show(_tvProgramDatas);

            _uiController.RequestNewData += RequestNewDataHandler;
        }

        private async void RequestNewDataHandler()
        {
            await TrySendRequest(true);
        }

        private async void RequestNextDataHandler()
        {
            await TrySendRequest(false);

            _requestOffset += REQUEST_LIMIT;
        }

        private async Task TrySendRequest(bool clearInfo)
        {
            if (_requesting)
                return;

            var url = string.Format(URL_TEMPLATE, "muumi", REQUEST_LIMIT, _requestOffset, APP_ID, APP_KEY);

            if (url == _currentUrl)
                return;

            if (clearInfo)
            {
                _requestOffset = 0;
                _tvProgramDatas.Clear();
            }

            _requesting = true;
            _uiController.SetRequestingStatus(_requesting);

            _currentUrl = url;

            await Send(url);

            _requesting = false;
            _uiController.SetRequestingStatus(_requesting);
        }

        private async Task Send(string url)
        {
            var request = UnityWebRequest.Get(url);

            Debug.Log($"Sending to\n{url}");

            await request.SendWebRequest();

            if (!request.isHttpError && !request.isNetworkError)
            {
                var handlerText = request.downloadHandler.text;
                Debug.Log(handlerText);

                var result = handlerText.JsonTo<TVProgramData>();

                _tvProgramDatas.AddRange(result.Data);
            }
            else
            {
                Debug.LogError($"Request error [{request.error}]");
            }

            request.Dispose();
        }
    }
}