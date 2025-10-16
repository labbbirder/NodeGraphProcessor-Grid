using System;
using System.Collections.Generic;
using System.Threading;

namespace BBBirder
{
    /// <summary>
    /// CancallationTokenSource that can be reused until Cancel() called.
    /// </summary>
    public class PoolableCancellationTokenSource : IDisposable
    {
        public static Stack<PoolableCancellationTokenSource> s_pool = new();

        public static PoolableCancellationTokenSource Get()
        {
#if DEBUG
            if (Thread.CurrentThread.ManagedThreadId != 1)
            {
                throw new("PooledCancallationTokenSource can only be used in main thread.");
            }
#endif

            if (s_pool.TryPop(out var inst))
            {
                inst._tokenAcquired = false;
                inst._disposed = false;
                return inst;
            }

            return new();
        }

        private CancellationTokenSource _tokenSource;
        private bool _disposed;
        private bool _tokenAcquired;

        public bool IsDisposed => _disposed;
        public CancellationToken Token
        {
            get
            {
                _tokenAcquired = true;
                return _tokenSource.Token;
            }
        }

        private PoolableCancellationTokenSource()
        {
            _tokenSource = new();
        }

        public void Cancel()
        {
            if (_tokenAcquired)
            {
                _tokenSource.Cancel();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            if (_tokenSource.IsCancellationRequested || _tokenAcquired)
            {
                _tokenSource.Dispose();
                _tokenSource = null;
            }
            else
            {
                s_pool.Push(this);
            }
        }
    }
}
