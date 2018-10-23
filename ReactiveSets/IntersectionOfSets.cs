using System;

namespace ReactiveSets
{
    public class IntersectionOfSets<TIdSet, TId, TPayload> : SetOperationBase<TIdSet, TId, TPayload>
    {
        public IntersectionOfSets(IObservable<IDelta<TIdSet, IObservable<IDelta<TId, TPayload>>>> source) : base(source)
        {
        }

        protected override bool ShouldIncludeItem(int setCount, int itemCount)
        {
            return itemCount == setCount;
        }
    }
}