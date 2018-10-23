using System;

namespace ReactiveSets
{
    public interface ISet<out TId, out TPayload> : IObservable<IDelta<TId, TPayload>>{}
}