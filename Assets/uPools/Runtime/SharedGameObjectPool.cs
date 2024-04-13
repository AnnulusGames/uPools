using System;
using System.Collections.Generic;
using UnityEngine;

namespace uPools
{
    public static class SharedGameObjectPool
    {
        static readonly Dictionary<GameObject, Stack<GameObject>> pools = new();
        static readonly Dictionary<GameObject, Stack<GameObject>> cloneReferences = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            pools.Clear();
            cloneReferences.Clear();
        }

        public static GameObject Rent(GameObject original)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);

            GameObject obj;
            while (true)
            {
                if (!pool.TryPop(out obj))
                {
                    obj = UnityEngine.Object.Instantiate(original);
                    break;
                }
                else if (obj != null)
                {
                    obj.SetActive(true);
                    break;
                }
            }

            cloneReferences.Add(obj, pool);

            PoolCallbackHelper.InvokeOnRent(obj);
            
            return obj;
        }

        public static GameObject Rent(GameObject original, Transform parent)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);

            GameObject obj;
            while (true)
            {
                if (!pool.TryPop(out obj))
                {
                    obj = UnityEngine.Object.Instantiate(original, parent);
                    break;
                }
                else if (obj != null)
                {
                    obj.transform.SetParent(parent);
                    obj.SetActive(true);
                    break;
                }
            }

            cloneReferences.Add(obj, pool);

            PoolCallbackHelper.InvokeOnRent(obj);

            return obj;
        }

        public static GameObject Rent(GameObject original, Vector3 position, Quaternion rotation)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);

            GameObject obj;
            while (true)
            {
                if (!pool.TryPop(out obj))
                {
                    obj = UnityEngine.Object.Instantiate(original, position, rotation);
                    break;
                }
                else if (obj != null)
                {
                    obj.transform.SetPositionAndRotation(position, rotation);
                    obj.SetActive(true);
                    break;
                }
            }

            cloneReferences.Add(obj, pool);

            PoolCallbackHelper.InvokeOnRent(obj);

            return obj;
        }

        public static GameObject Rent(GameObject original, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);

            GameObject obj;
            while (true)
            {
                if (!pool.TryPop(out obj))
                {
                    obj = UnityEngine.Object.Instantiate(original, position, rotation, parent);
                    break;
                }
                else if (obj != null)
                {
                    obj.transform.SetParent(parent);
                    obj.transform.SetPositionAndRotation(position, rotation);
                    obj.SetActive(true);
                    break;
                }
            }

            cloneReferences.Add(obj, pool);

            PoolCallbackHelper.InvokeOnRent(obj);

            return obj;
        }

        public static TComponent Rent<TComponent>(TComponent original) where TComponent : Component
        {
            return Rent(original.gameObject).GetComponent<TComponent>();
        }

        public static TComponent Rent<TComponent>(TComponent original, Vector3 position, Quaternion rotation, Transform parent) where TComponent : Component
        {
            return Rent(original.gameObject, position, rotation, parent).GetComponent<TComponent>();
        }

        public static TComponent Rent<TComponent>(TComponent original, Vector3 position, Quaternion rotation) where TComponent : Component
        {
            return Rent(original.gameObject, position, rotation).GetComponent<TComponent>();
        }

        public static TComponent Rent<TComponent>(TComponent original, Transform parent) where TComponent : Component
        {
            return Rent(original.gameObject, parent).GetComponent<TComponent>();
        }

        public static void Return(GameObject instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var pool = cloneReferences[instance];
            instance.SetActive(false);
            pool.Push(instance);
            cloneReferences.Remove(instance);

            PoolCallbackHelper.InvokeOnReturn(instance);
        }

        public static void Prewarm(GameObject original, int count)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);

            for (int i = 0; i < count; i++)
            {
                var obj = UnityEngine.Object.Instantiate(original);
                obj.SetActive(false);
                pool.Push(obj);

                PoolCallbackHelper.InvokeOnReturn(obj);
            }
        }

        static Stack<GameObject> GetOrCreatePool(GameObject original)
        {
            if (!pools.TryGetValue(original, out var pool))
            {
                pool = new Stack<GameObject>();
                pools.Add(original, pool);
            }
            return pool;
        }
    }
}