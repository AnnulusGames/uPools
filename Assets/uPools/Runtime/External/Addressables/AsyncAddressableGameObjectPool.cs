#if UPOOLS_ADDRESSABLES_SUPPORT && UPOOLS_UNITASK_SUPPORT
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace uPools
{
    public sealed class AsyncAddressableGameObjectPool : IAsyncObjectPool<GameObject>
    {
        public AsyncAddressableGameObjectPool(object key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            this.key = key;
        }
        
        public AsyncAddressableGameObjectPool(AssetReferenceGameObject reference)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            this.key = reference.RuntimeKey;
        }

        readonly object key;
        readonly Stack<GameObject> stack = new(32);
        bool isDisposed;

        public int Count => stack.Count;
        public bool IsDisposed => isDisposed;

        public async UniTask<GameObject> RentAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (!stack.TryPop(out var obj))
            {
                obj = await Addressables.InstantiateAsync(key).ToUniTask(cancellationToken: cancellationToken);
            }
            else
            {
                obj.SetActive(true);
            }

            PoolCallbackHelper.InvokeOnRent(obj);
            return obj;
        }

        public async UniTask<GameObject> RentAsync(Transform parent, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (!stack.TryPop(out var obj))
            {
                obj = await Addressables.InstantiateAsync(key, parent).ToUniTask(cancellationToken: cancellationToken);
            }
            else
            {
                obj.transform.SetParent(parent);
                obj.SetActive(true);
            }

            PoolCallbackHelper.InvokeOnRent(obj);
            return obj;
        }

        public async UniTask<GameObject> RentAsync(Vector3 position, Quaternion rotation, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (!stack.TryPop(out var obj))
            {
                obj = await Addressables.InstantiateAsync(key, position, rotation).ToUniTask(cancellationToken: cancellationToken);
            }
            else
            {
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.SetActive(true);
            }

            PoolCallbackHelper.InvokeOnRent(obj);
            return obj;
        }

        public async UniTask<GameObject> RentAsync(Vector3 position, Quaternion rotation, Transform parent)
        {
            ThrowIfDisposed();

            if (!stack.TryPop(out var obj))
            {
                obj = await Addressables.InstantiateAsync(key, position, rotation, parent);
            }
            else
            {
                obj.transform.SetParent(parent);
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.SetActive(true);
            }

            PoolCallbackHelper.InvokeOnRent(obj);
            return obj;
        }

        public void Return(GameObject obj)
        {
            ThrowIfDisposed();

            stack.Push(obj);
            obj.SetActive(false);

            PoolCallbackHelper.InvokeOnReturn(obj);
        }

        public void Clear()
        {
            ThrowIfDisposed();

            while (stack.TryPop(out var obj))
            {
                Addressables.ReleaseInstance(obj);
            }
        }

        public async UniTask PrewarmAsync(int count, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            for (int i = 0; i < count; i++)
            {
                var obj = await Addressables.InstantiateAsync(key).ToUniTask(cancellationToken: cancellationToken);

                stack.Push(obj);
                obj.SetActive(false);

                PoolCallbackHelper.InvokeOnReturn(obj);
            }
        }

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
#endif