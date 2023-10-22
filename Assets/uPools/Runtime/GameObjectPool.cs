using System;
using System.Collections.Generic;
using UnityEngine;

namespace uPools
{
    public sealed class GameObjectPool : IObjectPool<GameObject>
    {
        public GameObjectPool(GameObject original)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            this.original = original;
        }

        readonly GameObject original;
        readonly Stack<GameObject> stack = new(32);
        bool isDisposed;

        public int Count => stack.Count;
        public bool IsDisposed => isDisposed;

        public GameObject Rent()
        {
            ThrowIfDisposed();

            if (!stack.TryPop(out var obj))
            {
                obj = UnityEngine.Object.Instantiate(original);
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
                obj = UnityEngine.Object.Instantiate(original, parent);
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
                obj = UnityEngine.Object.Instantiate(original, position, rotation);
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
                obj = UnityEngine.Object.Instantiate(original, position, rotation, parent);
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
                UnityEngine.Object.Destroy(obj);
            }
        }

        public void Prewarm(int count)
        {
            ThrowIfDisposed();

            for (int i = 0; i < count; i++)
            {
                var obj = UnityEngine.Object.Instantiate(original);

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