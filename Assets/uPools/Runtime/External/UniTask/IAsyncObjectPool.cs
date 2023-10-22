#if UPOOLS_UNITASK_SUPPORT
using System.Threading;
using Cysharp.Threading.Tasks;

namespace uPools
{
    public interface IAsyncObjectPool<T>
    {
        UniTask<T> RentAsync(CancellationToken cancellationToken);
        void Return(T instance);
    }
}
#endif