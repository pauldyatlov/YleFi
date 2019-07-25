﻿using UnityEngine;
using UnityEngine.Networking;

namespace Yle.Fi
{
    public sealed class Engine : MonoBehaviour
    {
        private const string URL_TEMPLATE =
            "https://external.api.yle.fi/v1/programs/items.json?q={0}&limit={1}&offset={2}&app_id={3}&app_key={4}";

        private const int REQUEST_LIMIT = 10;

        private const string APP_ID = "dace39cd";
        private const string APP_KEY = "41d5031aabfc3f94c7eb54c7f987ab90";

        [SerializeField] private UIController _uiController = default;

        private int _requestOffset;

        private void Awake()
        {
            _uiController.Show(SendRequest);
        }

        private async void SendRequest()
        {
            var url = string.Format(URL_TEMPLATE, "muumi", REQUEST_LIMIT, _requestOffset, APP_ID, APP_KEY);
            var request = UnityWebRequest.Get(url);

            Debug.Log($"Sending to [{url}]");

            await request.SendWebRequest();

            if (!request.isHttpError && !request.isNetworkError)
            {
                Debug.Log(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"Request error [{request.error}]");
            }

            request.Dispose();

            _requestOffset += REQUEST_LIMIT;
        }
    }
}