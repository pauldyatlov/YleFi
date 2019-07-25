using UnityEngine;
using System;
using System.Collections.Generic;

namespace Yle.Fi
{
    public sealed class BindableViewList<TItem, TView> : ViewList<TItem, TView>
        where TView : UIElement
    {
        private readonly BindableList<TItem> _items;

        public BindableViewList(BindableList<TItem> items,
            TView template,
            Transform container,
            Action<TItem, TView> showAction) : base(items, func => template, container, showAction)
        {
            _items = items;

            _items.ItemAdded += ItemAddedHandler;
            _items.ItemRemoved += ItemRemovedHandler;

            _items.ItemsAdded += ItemsAddedHandler;
            _items.ItemsRemoved += ItemsRemovedHandler;
        }

        private void ItemAddedHandler(TItem item)
        {
            Add(item);
        }

        private void ItemRemovedHandler(TItem item)
        {
            Remove(item);
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