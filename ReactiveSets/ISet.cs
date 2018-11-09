using System;

namespace ReactiveSets
{
    public interface ISet<TId, out TPayload> : IObservable<IDelta<TId, TPayload>>
    {
        IDisposable Subscribe(TId id, IObserver<IDelta<TId, TPayload>> subscriber);
    }
}