using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Yle.Fi
{
    public sealed class BindableList<T> : IEnumerable<T>
    {
        public event Action<IEnumerable<T>> ItemsAdded;
        public event Action<IEnumerable<T>> ItemsRemoved;

        private readonly List<T> _list;

        public BindableList()
        {
            _list = new List<T>();
        }

        public void AddRange(IEnumerable<T> list)
        {
            var newItems = list.ToArray().Where(x => !_list.Contains(x)).ToArray();

            _list.InsertRange(0, newItems);

            ItemsAdded?.Invoke(newItems);
        }

        public void Clear()
        {
            ItemsRemoved?.Invoke(_list);

            _list.Clear();
        }

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}