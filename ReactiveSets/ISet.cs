using System;

namespace ReactiveSets
{
    public interface ISet<TId, TPayload> : IObservable<Delta<TId, TPayload>>{}
}