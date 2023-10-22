# uPools
 Object Pooling for Unity

<img width="90%" src="https://github.com/AnnulusGames/uPools/blob/main/Assets/uPools/Documentation~/Header.png">

[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)

[English README is here](README.md)

## 概要

uPoolsはUnity向けにオブジェクトプール用の機能を提供するライブラリです。汎用的な`ObjectPool`やGameObjectのプールに特化した`GameObjectPool`などを追加するほか、`ObjectPoolBase`クラスを継承して独自のオブジェクトプールを作成することが可能になります。また`SharedGameObjectPool`を使用することで手軽にプーリングを行うこともできます。

さらに、UniTaskを用いた非同期オブジェクトプールやAddressablesに対応したオブジェクトプールも用意されています。

## 特徴

* Unity向けのオブジェクトプール用のクラスを多数追加
* 汎用的なオブジェクトプールの機能を提供する`ObjectPool`
* GameObjectに特化した`GameObjectPool`
* Instantiate/Destroyをそのまま置き換え可能にする`SharedGameObjectPool`
* `IPoolCallbackReceiver`を用いてコールバックを取得
* UniTaskを用いた非同期オブジェクトプール
* Addressablesに対応した`AddressableGameObjectPool`

## セットアップ

### 要件

* Unity 2019.4 以上

### インストール

1. Window > Package ManagerからPackage Managerを開く
2. 「+」ボタン > Add package from git URL
3. 以下のURLを入力する

```
https://github.com/AnnulusGames/uPools.git?path=/Assets/uPools
```

あるいはPackages/manifest.jsonを開き、dependenciesブロックに以下を追記

```json
{
    "dependencies": {
        "com.annulusgames.u-pools" : "https://github.com/AnnulusGames/uPools.git?path=/Assets/uPools"
    }
}
```

## クイックスタート

`GameObject.Instantiate()`を`SharedGameObjectPool.Rent()`を置き換えるだけでオブジェクトプーリングを行うことができます。

```cs
using UnityEngine;
using uPools;

public class Example : MonoBehaviour
{
    [SerializeField] GameObject prefab;

    void Start()
    {
        // 事前にオブジェクトを生成する　(不足分は自動で生成される)
        SharedGameObjectPool.Prewarm(prefab, 10);

        // プールからオブジェクトを取得する
        var instance = SharedGameObjectPool.Rent(prefab);

        // 使用後はプールにオブジェクトを返す
        SharedGameObjectPool.Return(instance);
    }
}
```

この方法が最も簡単にプーリングを行えますが、細かく挙動を調整したい場合には以降の方法を使用してください。

## 通常のclassをプーリングする

通常のclassをプーリングするには、`ObjectPool<T>`を用いてオブジェクトプールを作成します。

```cs
class PooledObject { }

var pool = new ObjectPool<PooledObject>(
    createFunc: () => new PooledObject(), // オブジェクトの作成をFunc<T>で渡す
    onRent: instance => { }, // Rent時の処理 (オプション)
    onReturn: instance => { }, // Return時の処理 (オプション)
    onDestroy: instance => { } // プールが破棄された時の処理 (オプション)
)

// 事前に生成を行う
pool.Prewarm(10);

// Rent()でオブジェクトを取得、Return()でオブジェクトをプールに返す
var instance = pool.Rent();
pool.Return(instance);

// プール内のオブジェクトの個数を取得
var count = pool.Count;

// プールの中身を全て消す
pool.Clear();

// Dispose()でプールを破棄する
pool.Dispose();
```

> **Warning**
> オブジェクトプールはスレッドセーフではない点に注意してください。

## GameObjectをプーリングする

GameObjectをプーリングする場合には、専用の`GameObjectPool`が用意されています。

```cs
// PrefabのGameObject
GameObject original;

var pool = new GameObjectPool(original);

// Rent()でオブジェクトを取得
var instance1 = pool.Rent();

// 取得時に位置や回転、親のTransformを指定できる
Transform parent;
var instance2 = pool.Rent(new Vector3(1f, 2f, 3f), Quaternion.identity, parent);

// Return()でオブジェクトを返却
pool.Return(instance1);
pool.Return(instance2);

// Dispose()でプールを破棄し、GameObjectを全てDestroyする
pool.Dispose();
```

GameObjectはプールから取り出される際にアクティブ化され、返却される際に非アクティブ化されます。

## 独自のObjectPoolを作成する

`ObjectPoolBase<T>`を継承することで独自のオブジェクトプールを作成することができます。

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
        // ClearやDisposeでオブジェクトが破棄された際の処理を記述
    }

    protected override void OnRent(PooledObject instance)
    {
        // Rent時の処理を記述
    }

    protected override void OnReturn(PooledObject instance)
    {
        // Return時の処理を記述
    }
}
```

また、interfaceとして`IObjectPool<T>`が提供されており、こちらを実装してオブジェクトプールを作成することも可能です。

## コールバック

`IPoolCallbackReceiver`を実装することでRent/Return時に処理を挿入することが可能です。

```cs
public class CallbackExample : MonoBehaviour, IPoolCallbackReceiver
{
    public void OnRent()
    {
        Debug.Log("rented");
    }

    public void OnReturn()
    {
        Debug.Log("returned");
    }
}
```

`GameObjectPool`または`SharedGameObjectPool`の場合、対象のオブジェクトおよび子オブジェクトが持つ`IPoolCallbackReceiver`を実装したComponentを取得し、それぞれコールバックの呼び出しを行います。

それ以外の`ObjectPool<T>`や`ObjectPoolBase<T>`を継承したプールなどは、オブジェクトが`IPoolCallbackReceiver`を実装している場合にコールバックの呼び出しを行います。

`IObjectPool<T>`を独自に実装したオブジェクトプールの場合、`IPoolCallbackReceiver`の扱いは実装側の責任になります。必要に応じてこれらのコールバックを呼び出す処理を実装してください。

## UniTask

uPoolsはUniTaskを用いた非同期のオブジェクトプールに対応しています。UniTaskをプロジェクトに追加すると`AsyncObjectPool<T>`、`AsyncObjectPoolBase<T>`、`IAsyncObjectPool<T>`が利用可能になります。

これらのプールはRentやPrewarm、CreateInstanceが非同期となっており、それ以外は通常の`ObjectPool<T>`等と同じです。

## Addressables

Addressablesを使用してGameObjectを生成する場合、読み込んだPrefabのリソースを管理する必要があります。uPoolsはこれに対応した`AddressableGameObjectPool`を提供します。基本的な使い方は`GameObjectPool`と同じです。

```cs
// Prefabのアドレス
var key = "Address";
var pool = new AddressableGameObjectPool(key);

// 使用方法はGameObjectPoolと同じ
var instance1 = pool.Rent();
var instance2 = pool.Rent(new Vector3(1f, 2f, 3f), Quaternion.identity);

pool.Return(instance1);
pool.Return(instance2);

pool.Dispose();
```

また、UniTaskを導入することで非同期版の`AsyncAddressableGameObjectPool`を使用することも可能です。

## ライセンス

[MIT License](LICENSE)