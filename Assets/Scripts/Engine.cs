using UnityEngine;
using UnityEngine.Networking;
using Task = System.Threading.Tasks.Task;

namespace Yle.Fi
{
    public sealed class Engine : MonoBehaviour
    {
        internal const int REQUEST_LIMIT = 10;

        private const string URL_TEMPLATE = "https://external.api.yle.fi/v1/programs/items.json?q={0}&limit={1}&offset={2}&app_id={3}&app_key={4}&availability=ondemand";

        private const string APP_ID = "dace39cd";
        private const string APP_KEY = "41d5031aabfc3f94c7eb54c7f987ab90";

        [SerializeField] private ScrollerPanel _scrollerPanel;

        private BindableList<ContentData> _tvProgramDatas;

        private int _requestOffset;
        private string _currentUrl;
        private string _searchQuery;
        private bool _requesting;

        private void Awake()
        {
            _tvProgramDatas = new BindableList<ContentData>();

            _scrollerPanel.Show(_tvProgramDatas);

            _scrollerPanel.RequestNewData += RequestNewDataHandler;
            _scrollerPanel.RequestNextData += RequestNextDataHandler;
        }

        private async void RequestNewDataHandler(string searchQuery)
        {
            _searchQuery = searchQuery;

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

            var url = string.Format(URL_TEMPLATE, _searchQuery, REQUEST_LIMIT, _requestOffset, APP_ID, APP_KEY);

            if (url == _currentUrl)
                return;

            _requesting = true;
            _scrollerPanel.SetRequestingStatus(_requesting);

            if (clearInfo)
            {
                _requestOffset = 0;
                _tvProgramDatas.Clear();
            }

            _currentUrl = url;

            await Send(url);

            _requesting = false;
            _scrollerPanel.SetRequestingStatus(_requesting);
        }

        private async Task Send(string url)
        {
            var request = UnityWebRequest.Get(url);

            Debug.Log($">>> sending to\n{url}");

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var handlerText = request.downloadHandler.text;
                Debug.Log(handlerText);

                var result = handlerText.JsonTo<TVProgramData>();

                _tvProgramDatas.AddRange(result.Data);
                _scrollerPanel.DisplayError(string.Empty);
            }
            else
            {
                Debug.LogError($"Request error [{request.error}]");

                _tvProgramDatas.Clear();
                _scrollerPanel.DisplayError(request.error);
            }

            request.Dispose();
        }

        private void OnDestroy()
        {
            _scrollerPanel.RequestNewData -= RequestNewDataHandler;
            _scrollerPanel.RequestNextData -= RequestNextDataHandler;
        }
    }
}