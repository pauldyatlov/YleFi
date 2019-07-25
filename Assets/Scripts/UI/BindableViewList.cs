using UnityEngine;
using System;
using System.Collections.Generic;

namespace Yle.Fi
{
    public interface IBindableList<out TItem> : IEnumerable<TItem>
    {
        event Action<TItem> ItemAdded;
        event Action<TItem> ItemRemoved;

        event Action<IEnumerable<TItem>> ItemsAdded;
        event Action<IEnumerable<TItem>> ItemsRemoved;
    }

    public sealed class BindableListView<TItem, TView> : ViewList<TItem, TView>
        where TView : UIElement
    {
        private readonly IBindableList<TItem> _items;

        public event Action<TItem> OnRemove;

        public BindableListView(IBindableList<TItem> items,
            Func<TItem, TView> template,
            Transform container,
            Action<TItem, TView> showAction)
            : base(items, template, container, showAction)
        {
            _items = items;

            _items.ItemAdded += ItemAddedHandler;
            _items.ItemRemoved += ItemRemovedHandler;

            _items.ItemsAdded += ItemsAddedHandler;
            _items.ItemsRemoved += ItemsRemovedHandler;
        }

        public BindableListView(IBindableList<TItem> items,
            TView template,
            Transform container,
            Action<TItem, TView> showAction)
            : this(items, arg => template, container, showAction)
        {
        }

        private void ItemAddedHandler(TItem item)
        {
            Add(item);
        }

        private void ItemRemovedHandler(TItem item)
        {
            Remove(item);

            OnRemove?.Invoke(item);
        }

        private void ItemsAddedHandler(IEnumerable<TItem> items)
        {
            foreach (var item in items)
                Add(item);
        }

        private void ItemsRemovedHandler(IEnumerable<TItem> items)
        {
            foreach (var item in items)
                Remove(item);
        }

        public override void Dispose()
        {
            _items.ItemAdded -= ItemAddedHandler;
            _items.ItemRemoved -= ItemRemovedHandler;

            _items.ItemsAdded -= ItemsAddedHandler;
            _items.ItemsRemoved -= ItemsRemovedHandler;

            base.Dispose();
        }
    }
}