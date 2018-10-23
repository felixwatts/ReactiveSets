using System;

namespace ReactiveSets
{
    public class UnionOfSets<TIdSet, TId, TPayload> : SetOperationBase<TIdSet, TId, TPayload>
    {
        public UnionOfSets(IObservable<IDelta<TIdSet, IObservable<IDelta<TId, TPayload>>>> source) : base(source)
        {
        }

        protected override bool ShouldIncludeItem(int setCount, int itemCount)
        {
            return itemCount > 0;
        }
    }
}