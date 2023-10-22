using System;

namespace uPools
{
    public sealed class ObjectPool<T> : ObjectPoolBase<T>
        where T : class
    {
        public ObjectPool(Func<T> createFunc, Action<T> onRent = null, Action<T> onReturn = null, Action<T> onDestroy = null)
        {
            if (createFunc == null) throw new ArgumentException(nameof(createFunc));

            this.createFunc = createFunc;
            this.onRent = onRent;
            this.onReturn = onReturn;
            this.onDestroy = onDestroy;
        }

        readonly Func<T> createFunc;
        readonly Action<T> onRent;
        readonly Action<T> onReturn;
        readonly Action<T> onDestroy;

        protected override T CreateInstance()
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