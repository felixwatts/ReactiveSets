using System;

namespace ReactiveSets
{
    public class DifferenceOfSets<TIdSet, TId, TPayload> : SetOperationBase<TIdSet, TId, TPayload>
    {
        public DifferenceOfSets(IObservable<IDelta<TIdSet, IObservable<IDelta<TId, TPayload>>>> source) : base(source)
        {
        }

        protected override bool ShouldIncludeItem(int setCount, int itemCount)
        {
            return itemCount == 1;
        }
    }
}