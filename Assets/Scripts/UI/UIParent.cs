using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yle.Fi
{
    public sealed class UIParent : IDisposable
    {
        private readonly List<Action> _destroys = new List<Action>();

        public T AddDisposable<T>(T disposable)
            where T : IDisposable
        {
            _destroys.Add(disposable.Dispose);
            return disposable;
        }

        public void AddDisposable(Action destroy)
        {
            _destroys.Add(destroy);
        }

        public ViewList<TItem, TView> AddViewList<TItem, TView>(IEnumerable<TItem> items,
            Func<TItem, TView> template,
            Transform container,
            Action<TItem, TView> showAction)
            where TView : UIElement
        {
            return AddDisposable(new ViewList<TItem, TView>(items, template, container, showAction));
        }

        public ViewList<TItem, TView> AddViewList<TItem, TView>(IEnumerable<TItem> items,
            TView template,
            Transform container,
            Action<TItem, TView> showAction)
            where TView : UIElement
        {
            return AddDisposable(new ViewList<TItem, TView>(items, arg => template, container, showAction));
        }

        public void Dispose()
        {
            foreach (var destroy in _destroys)
                destroy();

            _destroys.Clear();
        }
    }
}