using System;
using System.Collections.Generic;

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

        public void Dispose()
        {
            foreach (var destroy in _destroys)
                destroy();

            _destroys.Clear();
        }
    }
}