using System;
using System.Collections.Generic;

namespace uPools
{
    public abstract class ObjectPoolBase<T> : IObjectPool<T>
        where T : class
    {
        protected readonly Stack<T> stack = new(32);
        bool isDisposed;

        protected abstract T CreateInstance();
        protected virtual void OnDestroy(T instance) { }
        protected virtual void OnRent(T instance) { }
        protected virtual void OnReturn(T instance) { }

        public T Rent()
        {
            ThrowIfDisposed();
            if (stack.TryPop(out var obj))
            {
                OnRent(obj);
                if (obj is IPoolCallbackReceiver receiver) receiver.OnRent();
                return obj;
            }

            return CreateInstance();
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

        public void Prewarm(int count)
        {
            ThrowIfDisposed();
            for (int i = 0; i < count; i++)
            {
                var instance = CreateInstance();
                Return(instance);
            }
        }

        public int Count => stack.Count;
        public bool IsDisposed => isDisposed;

        public void Dispose()
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