using JetBrains.Annotations;
using UnityEngine;

namespace Yle.Fi.EnhancedScroller
{
    public class SmallList<T>
    {
        public T[] Data = { };
        public int Count;

        public T this[int i]
        {
            get { return Data[i]; }
            set { Data[i] = value; }
        }

        private void ResizeArray()
        {
            var newData = Data != null
                ? new T[Mathf.Max(Data.Length << 1, 64)]
                : new T[64];

            if (Data != null && Count > 0)
                Data.CopyTo(newData, 0);

            Data = newData;
        }

        public void Clear()
        {
            Count = 0;
        }

        [CanBeNull] public T First()
        {
            if (Data == null || Count == 0)
                return default(T);

            return Data[0];
        }

        [CanBeNull] public T Last()
        {
            if (Data == null || Count == 0)
                return default(T);

            return Data[Count - 1];
        }

        public void Add(T item)
        {
            if (Data == null || Count == Data.Length)
                ResizeArray();

            Data[Count] = item;
            Count++;
        }

        public void AddStart(T item)
        {
            Insert(0, item);
        }

        public void Insert(int index, T item)
        {
            if (Data == null || Count == Data.Length)
                ResizeArray();

            for (var i = Count; i > index; i--)
                Data[i] = Data[i - 1];

            Data[index] = item;
            Count++;
        }

        [CanBeNull] public T RemoveAt(int index)
        {
            if (Data == null || Count == 0)
                return default(T);

            var val = Data[index];

            for (var i = index; i < Count - 1; i++)
            {
                Data[i] = Data[i + 1];
            }

            Count--;
            Data[Count] = default(T);
            return val;
        }

        [CanBeNull] public T Remove(T item)
        {
            if (Data == null || Count == 0)
                return default(T);

            for (var i = 0; i < Count; i++)
                if (Data[i].Equals(item))
                    return RemoveAt(i);

            return default(T);
        }

        public bool Contains(T item)
        {
            if (Data == null)
                return false;

            for (var i = 0; i < Count; i++)
                if (Data[i].Equals(item))
                    return true;

            return false;
        }
    }
}