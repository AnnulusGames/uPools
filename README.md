# uPools
 Object Pooling for Unity

[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)

[日本語版READMEはこちら](README_JP.md)

## Overview

uPools is a library that provides object pooling functionality for Unity. It includes various classes for object pooling, such as a generic `ObjectPool` and a specialized `GameObjectPool` for GameObjects. You can also create your custom object pools by inheriting from the `ObjectPoolBase` class. Additionally, it offers the convenience of object pooling using `SharedGameObjectPool`.

Furthermore, it provides support for asynchronous object pooling using UniTask and object pooling with Addressables.

## Features

* Numerous classes for object pooling in Unity
* Generic object pooling with `ObjectPool`
* Pooling for GameObjects with `GameObjectPool`
* `SharedGameObjectPool` that can replace `Instantiate` and `Destroy`
* Callback handling with `IPoolCallbackReceiver`
* Asynchronous object pooling with UniTask
* Object pooling with Addressables using `AddressableGameObjectPool`

## Setup

### Requirements

* Unity 2019.4 or newer

### Installation

1. Open the Package Manager from Window > Package Manager.
2. Click the "+" button, then select "Add package from git URL."
3. Enter the following URL:

```
https://github.com/AnnulusGames/uPools.git?path=/Assets/uPools
```

Alternatively, open the Packages/manifest.json file and add the following to the dependencies block:

```json
{
    "dependencies": {
        "com.annulusgames.u-pools" : "https://github.com/AnnulusGames/uPools.git?path=/Assets/uPools"
    }
}
```

## Quick Start

You can easily implement object pooling by replacing `GameObject.Instantiate()` with `SharedGameObjectPool.Rent()`.

```cs
using UnityEngine;
using uPools;

public class Example : MonoBehaviour
{
    [SerializeField] GameObject prefab;

    void Start()
    {
        // Pre-warm objects in advance (additional objects are created automatically)
        SharedGameObjectPool.Prewarm(prefab, 10);

        // Retrieve objects from the pool
        var instance = SharedGameObjectPool.Rent(prefab);

        // Return the object to the pool after use
        SharedGameObjectPool.Return(instance);
    }
}
```

This is the simplest way to implement object pooling. If you need more fine-grained control, you can explore the following methods.

## Pooling Regular Classes

To pool regular classes, you can create an object pool using `ObjectPool<T>`.

```cs
class PooledObject { }

var pool = new ObjectPool<PooledObject>(
    createFunc: () => new PooledObject(), // provide object creation using a Func<T>
    onRent: instance => { }, // actions on Rent (optional)
    onReturn: instance => { }, // actions on Return  (optional)
    onDestroy: instance => { } // actions when the pool is destroyed (optional)
)

// Pre-warm the pool with objects
pool.Prewarm(10);

// Use Rent() to retrieve an object, and Return() to return it to the pool
var instance = pool.Rent();
pool.Return(instance);

// Get the number of objects in the pool
var count = pool.Count;

// Clear all objects in the pool
pool.Clear();

// Dispose of the pool with Dispose()
pool.Dispose();
```

> **Warning**
> Note that object pools are not thread-safe.

## Pooling GameObjects

For pooling GameObjects, a dedicated `GameObjectPool` is provided.

```cs
// GameObject prefab
GameObject original;

var pool = new GameObjectPool(original);

// Use Rent() to retrieve an object
var instance1 = pool.Rent();

// You can specify position, rotation, and parent Transform when retrieving
Transform parent;
var instance2 = pool.Rent(new Vector3(1f, 2f, 3f), Quaternion.identity, parent);

// Use Return() to return the object
pool.Return(instance1);
pool.Return(instance2);

// Dispose() to destroy the pool and all GameObjects
pool.Dispose();
```

GameObject instances are activated when retrieved from the pool and deactivated when returned.

## Creating Custom Object Pools

You can create your custom object pool by inheriting from `ObjectPoolBase<T>`.

```cs
class PooledObject { }

public sealed class CustomObjectPool : ObjectPoolBase<PooledObject>
{
    protected override PooledObject CreateInstance()
    {
        return new PooledObject();
    }

    protected override void OnDestroy(PooledObject instance)
    {
        // Add actions when the object is destroyed in Clear or Dispose
    }

    protected override void OnRent(PooledObject instance)
    {
        // Add actions when rented
    }

    protected override void OnReturn(PooledObject instance)
    {
        // Add actions when returned
    }
}
```

Additionally, an interface `IObjectPool<T>` is provided, which allows you to implement your own object pool by implementing it.

## Callbacks

You can insert custom actions on Rent and Return by implementing `IPoolCallbackReceiver`.

```cs
public class CallbackExample : MonoBehaviour, IPoolCallbackReceiver
{
    public void OnRent()
    {
        Debug.Log("Rented");
    }

    public void OnReturn()
    {
        Debug.Log("Returned");
    }
}
```

In the case of `GameObjectPool` or `SharedGameObjectPool`, this component will be retrieved from the object and its child objects, and the callbacks will be invoked accordingly. For other object pools like `ObjectPool<T>` or pools that inherit from `ObjectPoolBase<T>`, the callbacks are invoked for objects that implement `IPoolCallbackReceiver`.

If you create your own object pool by implementing `IObjectPool<T`, you will need to handle the `IPoolCallbackReceiver` calls yourself. Implement the necessary logic to invoke these callbacks as needed.

## UniTask

uPools supports asynchronous object pooling using UniTask. When you add UniTask to your project, you can use `AsyncObjectPool<T>`, `AsyncObjectPoolBase<T>`, and `IAsyncObjectPool<T>` for asynchronous object pooling. These pools provide asynchronous versions of Rent, Prewarm, and CreateInstance while behaving like regular `ObjectPool<T>` in other aspects.

## Addressables

When using Addressables to generate GameObjects, you need to manage the resources of the loaded Prefabs. uPools offers `AddressableGameObjectPool` for this purpose, which can be used similarly to `GameObjectPool`.

```cs
// Address of the Prefab
var key = "Address";
var pool = new AddressableGameObjectPool(key);

// Usage is the same as GameObjectPool
var instance1 = pool.Rent();
var instance2 = pool.Rent(new Vector3(1f, 2f, 3f), Quaternion.identity);

pool.Return(instance1);
pool.Return(instance2);

pool.Dispose();
```

You can also use the asynchronous version `AsyncAddressableGameObjectPool` by introducing UniTask.

## License

[MIT License](LICENSE)