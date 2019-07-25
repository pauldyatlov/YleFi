using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace Yle.Fi
{
    public class WebRequestAwaiter : INotifyCompletion
    {
        private readonly UnityWebRequestAsyncOperation _asyncOp;
        private Action _continuation;

        public WebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp)
        {
            _asyncOp = asyncOp;
            asyncOp.completed += OnRequestCompleted;
        }

        public bool IsCompleted => _asyncOp.isDone;

        public void GetResult()
        {
        }

        public void OnCompleted(Action continuation)
        {
            _continuation = continuation;
        }

        private void OnRequestCompleted(AsyncOperation obj)
        {
            _continuation?.Invoke();
        }
    }
}