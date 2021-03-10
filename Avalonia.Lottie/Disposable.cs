using System;

namespace Avalonia.Lottie
{
    public class Disposable : IDisposable
    {
        private readonly Action _action;
        private bool _isDisposed;

        public Disposable(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _action?.Invoke();
            _isDisposed = true;
        }
    }
}