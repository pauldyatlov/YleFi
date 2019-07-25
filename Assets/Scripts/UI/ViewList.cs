using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using System.Collections;

namespace Yle.Fi
{
    public class ViewList<TItem, TView> : IDisposable, IEnumerable<KeyValuePair<TItem, TView>>
        where TView : UIElement
    {
        [CanBeNull] public TView this[TItem item] => _views[item];

        public int Count => _views.Count;

        private readonly Dictionary<TItem, TView> _views = new Dictionary<TItem, TView>();
        private readonly Func<TItem, TView> _template;
        private readonly Transform _container;
        private readonly Action<TItem, TView> _showAction;

        public ViewList(IEnumerable<TItem> items,
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

        public ViewList(IEnumerable<TItem> items,
            TView template,
            Transform container,
            Action<TItem, TView> showAction) : this(items, arg => template, container, showAction)
        {
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

        public void UpdateItems(Action<TView> updateAction)
        {
            foreach (var view in _views)
                updateAction(view.Value);
        }

        public void OrderByDescending<TKey>(Func<TItem, TKey> order)
        {
            foreach (var view in _views.OrderByDescending(x => order(x.Key)))
                view.Value.transform.SetAsLastSibling();
        }

        public void OrderBy<TKey>(Func<TItem, TKey> order, IComparer<TKey> comparer)
        {
            foreach (var view in _views.OrderBy(x => order(x.Key), comparer))
                view.Value.transform.SetAsLastSibling();
        }

        public void SetOrder(IEnumerable<TItem> orderedItems)
        {
            foreach (var item in orderedItems)
                _views[item].transform.SetAsLastSibling();
        }

        public void FilterBy(Func<TItem, bool> condition)
        {
            foreach (var view in _views)
                view.Value.gameObject.SetActive(condition(view.Key));
        }

        public bool Any()
        {
            return _views.Any();
        }

        public bool Contains(TItem key)
        {
            return _views.ContainsKey(key);
        }

        public KeyValuePair<TItem, TView> Last()
        {
            return _views.Last();
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

        private void SoftKill(TView view)
        {
            view.Dispose();
            UnityEngine.Object.Destroy(view.gameObject);
        }

        public void SoftDispose()
        {
            foreach (var view in _views.Values)
                SoftKill(view);

            _views.Clear();
        }

        public IEnumerator<KeyValuePair<TItem, TView>> GetEnumerator()
        {
            foreach (var view in _views)
                yield return view;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var view in _views)
                yield return view;
        }
    }
}