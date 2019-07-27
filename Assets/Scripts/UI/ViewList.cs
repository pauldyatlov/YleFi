using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Yle.Fi
{
    public class ViewList<TItem, TView> : IDisposable, IEnumerable<KeyValuePair<TItem, TView>>
        where TView : UIElement
    {
        private readonly Dictionary<TItem, TView> _views = new Dictionary<TItem, TView>();
        private readonly Func<TItem, TView> _template;
        private readonly Transform _container;
        private readonly Action<TItem, TView> _showAction;

        protected ViewList(IEnumerable<TItem> items,
            Func<TItem, TView> template,
            Transform container,
            Action<TItem, TView> showAction)
        {
            _template = template;
            _container = container;
            _showAction = showAction;

            foreach (var item in items)
                CreateView(item);
        }

        private void CreateView(TItem item)
        {
            if (_views.ContainsKey(item))
            {
                Debug.LogError("element " + item + " is already in list");
                return;
            }

            var view = UnityEngine.Object.Instantiate(_template(item), _container, false);
            var castedView = view.GetComponent<TView>();

            _showAction(item, castedView);
            _views[item] = castedView;
        }

        protected void Add(TItem item)
        {
            CreateView(item);
        }

        protected void Remove(TItem item)
        {
            KillView(_views[item]);
            _views.Remove(item);
        }

        private void KillView(TView view)
        {
            view.Dispose();
            UnityEngine.Object.DestroyImmediate(view.gameObject);
        }

        public virtual void Dispose()
        {
            foreach (var view in _views.Values)
                KillView(view);

            _views.Clear();
        }

        public IEnumerator<KeyValuePair<TItem, TView>> GetEnumerator() =>
            ((IEnumerable<KeyValuePair<TItem, TView>>)_views).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _views.GetEnumerator();
    }
}