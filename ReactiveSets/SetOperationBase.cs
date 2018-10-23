using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;

namespace ReactiveSets
{

    public abstract class SetOperationBase<TIdSet, TId, TPayload> : SetOfSetsToSet<TIdSet, TId, TPayload, TPayload>
    {
        private readonly Dictionary<TId, Dictionary<TIdSet, TPayload>> _payloadBySetById;
        private readonly Dictionary<TIdSet, HashSet<TId>> _idsBySetId;

        protected SetOperationBase(IObservable<IDelta<TIdSet, IObservable<IDelta<TId, TPayload>>>> source) : base(source)
        {
            _payloadBySetById = new Dictionary<TId, Dictionary<TIdSet, TPayload>>();
            _idsBySetId = new Dictionary<TIdSet, HashSet<TId>>();
        }

        protected abstract bool ShouldIncludeItem(int setCount, int itemCount);

        protected override void OnSetSet(TIdSet id, IObservable<IDelta<TId, TPayload>> payload)
        {
            base.OnSetSet(id, payload);
            HandleSetCountChanged();
        }

        protected override void OnDeleteSet(TIdSet id)
        {
            base.OnDeleteSet(id);
            HandleSetCountChanged();
        }
        protected override void OnSetItem(TIdSet setId, TId id, TPayload payload)
        {
            var payloadBySet = _payloadBySetById.FindOrAdd(id, () => new Dictionary<TIdSet, TPayload>());

            payloadBySet[setId] = payload;
            _idsBySetId.FindOrAdd(setId, () => new HashSet<TId>()).Add(id);

            HandlePayloadBySetChangedForId(id, payloadBySet);
        }

        protected override void OnDeleteItem(TIdSet setId, TId id)
        {
            var payloadBySet = _payloadBySetById.FindOrAdd(id, () => new Dictionary<TIdSet, TPayload>());

            payloadBySet.Remove(setId);
            _idsBySetId.FindOrAdd(setId, () => new HashSet<TId>()).Remove(id);

            HandlePayloadBySetChangedForId(id, payloadBySet);
        }

        protected override void OnClear(TIdSet setId)
        {
            var idsOfSet = _idsBySetId[setId].ToArray();
            foreach(var id in idsOfSet)
            {
                OnDeleteItem(setId, id);
            }
        }

        private void HandleSetCountChanged()
        {
            foreach(var kvp in _payloadBySetById)
            {
                HandlePayloadBySetChangedForId(kvp.Key, kvp.Value);
            }
        }

        private void HandlePayloadBySetChangedForId(TId id, IReadOnlyDictionary<TIdSet, TPayload> payloadBySetId)
        {
            var shouldInclude = ShouldIncludeItem(GetSetCount(), payloadBySetId.Count());
            var isIncluded = _content.ContainsKey(id);

            if(shouldInclude && !isIncluded)
            {
                _content.SetItem(id, payloadBySetId.First().Value);
            }
            else if(!shouldInclude && isIncluded)
            {
                _content.DeleteItem(id);
            }
        }
    }
}