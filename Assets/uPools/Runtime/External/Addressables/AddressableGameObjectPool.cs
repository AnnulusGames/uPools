#if UPOOLS_ADDRESSABLES_SUPPORT
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace uPools
{
    public sealed class AddressableGameObjectPool : IObjectPool<GameObject>
    {
        public AddressableGameObjectPool(object key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            this.key = key;
        }

        public AddressableGameObjectPool(AssetReferenceGameObject reference)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            this.key = reference.RuntimeKey;
        }

        readonly object key;
        readonly Stack<GameObject> stack = new(32);
        bool isDisposed;

        public int Count => stack.Count;
        public bool IsDisposed => isDisposed;

        public GameObject Rent()
        {
            ThrowIfDisposed();

            if (!stack.TryPop(out var obj))
            {
                obj = Addressables.InstantiateAsync(key).WaitForCompletion();
            }
            else
            {
                obj.SetActive(true);
            }

            PoolCallbackHelper.InvokeOnRent(obj);
            return obj;
        }

        public GameObject Rent(Transform parent)
        {
            ThrowIfDisposed();

            if (!stack.TryPop(out var obj))
            {
                obj = Addressables.InstantiateAsync(key, parent).WaitForCompletion();
            }
            else
            {
                obj.transform.SetParent(parent);
                obj.SetActive(true);
            }

            PoolCallbackHelper.InvokeOnRent(obj);
            return obj;
        }

        public GameObject Rent(Vector3 position, Quaternion rotation)
        {
            ThrowIfDisposed();

            if (!stack.TryPop(out var obj))
            {
                obj = Addressables.InstantiateAsync(key, position, rotation).WaitForCompletion();
            }
            else
            {
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.SetActive(true);
            }

            PoolCallbackHelper.InvokeOnRent(obj);
            return obj;
        }

        public GameObject Rent(Vector3 position, Quaternion rotation, Transform parent)
        {
            ThrowIfDisposed();

            if (!stack.TryPop(out var obj))
            {
                obj = Addressables.InstantiateAsync(key, position, rotation, parent).WaitForCompletion();
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

        public void Prewarm(int count)
        {
            ThrowIfDisposed();

            for (int i = 0; i < count; i++)
            {
                var obj = Addressables.InstantiateAsync(key).WaitForCompletion();

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