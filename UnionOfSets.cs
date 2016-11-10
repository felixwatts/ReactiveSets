using System;
using System.Collections.Generic;

namespace ReactiveSets
{
    public class UnionOfSets<TIdSet, TId, TPayload> : SetOfSetsToSet<TIdSet, TId, TPayload>
    {
        private Dictionary<TIdSet, HashSet<TId>> _idsBySet;
        private Dictionary<TId, uint> _countById;

        public UnionOfSets(IObservable<Delta<TIdSet, IObservable<Delta<TId, TPayload>>>> source) : base(source)
        {
            _idsBySet = new Dictionary<TIdSet, HashSet<TId>>();
            _countById = new Dictionary<TId, uint>();
        }

        protected override void OnClearSets()
        {
            base.OnClearSets();
            _idsBySet.Clear();
            _countById.Clear();        
        }

        protected override void OnSetItem(TIdSet setId, TId id, TPayload payload)
        {
            var incrementCount = _idsBySet.FindOrAdd(setId, () => new HashSet<TId>()).Add(id);
            if(incrementCount)
            {
                uint count = 0;
                _countById.TryGetValue(id, out count);
                count++;
                _countById[id] = count;
            }

            _content.SetItem(id, payload);
        }

        protected override void OnDeleteItem(TIdSet setId, TId id)
        {
            _idsBySet[setId].Remove(id);
            _countById[id] --;

            if(_countById[id] == 0)
                _content.DeleteItem(id);            
        }

        protected override void OnClear(TIdSet setId)
        {
            foreach(var id in _idsBySet[setId])
            {
                OnDeleteItem(setId, id);
            }

            _idsBySet.Remove(setId);
        }
    }
}