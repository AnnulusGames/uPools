#if UPOOLS_UNITASK_SUPPORT
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace uPools
{
    public sealed class AsyncObjectPool<T> : AsyncObjectPoolBase<T>
        where T : class
    {
        public AsyncObjectPool(Func<UniTask<T>> createFunc, Action<T> onRent = null, Action<T> onReturn = null, Action<T> onDestroy = null)
        {
            if (createFunc == null) throw new ArgumentException(nameof(createFunc));

            this.createFunc = createFunc;
            this.onRent = onRent;
            this.onReturn = onReturn;
            this.onDestroy = onDestroy;
        }

        readonly Func<UniTask<T>> createFunc;
        readonly Action<T> onRent;
        readonly Action<T> onReturn;
        readonly Action<T> onDestroy;

        protected override UniTask<T> CreateInstanceAsync(CancellationToken cancellationToken)
        {
            return createFunc();
        }

        protected override void OnDestroy(T instance)
        {
            onDestroy?.Invoke(instance);
        }

        protected override void OnRent(T instance)
        {
            onRent?.Invoke(instance);
        }

        protected override void OnReturn(T instance)
        {
            onReturn?.Invoke(instance);
        }
    }
}
#endif