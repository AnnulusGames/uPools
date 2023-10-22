#if UPOOLS_UNITASK_SUPPORT
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace uPools
{
    public abstract class AsyncObjectPoolBase<T> : IAsyncObjectPool<T>
        where T : class
    {
        protected readonly Stack<T> stack = new(32);
        bool isDisposed;

        protected abstract UniTask<T> CreateInstanceAsync(CancellationToken cancellationToken);
        protected virtual void OnDestroy(T instance) { }
        protected virtual void OnRent(T instance) { }
        protected virtual void OnReturn(T instance) { }

        public UniTask<T> RentAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            if (stack.TryPop(out var obj))
            {
                OnRent(obj);
                if (obj is IPoolCallbackReceiver receiver) receiver.OnRent();
                return new UniTask<T>(obj);
            }

            return CreateInstanceAsync(cancellationToken);
        }

        public void Return(T obj)
        {
            ThrowIfDisposed();
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            OnReturn(obj);
            if (obj is IPoolCallbackReceiver receiver) receiver.OnReturn();
            stack.Push(obj);
        }

        public void Clear()
        {
            ThrowIfDisposed();
            while (stack.TryPop(out var obj))
            {
                OnDestroy(obj);
            }
        }

        public async UniTask PrewarmAsync(int count, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            for (int i = 0; i < count; i++)
            {
                var instance = await CreateInstanceAsync(cancellationToken);
                Return(instance);
            }
        }

        public int Count => stack.Count;
        public bool IsDisposed => isDisposed;

        public virtual void Dispose()
        {
            ThrowIfDisposed();
            Clear();
            isDisposed = true;
        }

        void ThrowIfDisposed()
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().Name);
        }
    }
}
#endif