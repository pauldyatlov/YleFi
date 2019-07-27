using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yle.Fi
{
    public class UIElement : MonoBehaviour, IDisposable
    {
        private readonly List<Action> _destroys = new List<Action>();

        public T AddDisposable<T>(T disposable)
            where T : IDisposable
        {
            _destroys.Add(disposable.Dispose);
            return disposable;
        }

        public void Display()
        {
            ShowGameObject();
        }

        public void AddDisposable(Action destroy)
        {
            _destroys.Add(destroy);
        }

        public void Dispose()
        {
            foreach (var destroy in _destroys)
                destroy();

            _destroys.Clear();

            HideGameObject();
        }

        protected void ShowGameObject()
        {
            gameObject.SetActive(true);
        }

        private void HideGameObject()
        {
            gameObject.SetActive(false);
        }
    }
}