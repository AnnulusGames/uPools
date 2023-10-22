using System;

namespace uPools
{
    public interface IObjectPool<T> : IDisposable
    {
        T Rent();
        void Return(T obj);
    }
}