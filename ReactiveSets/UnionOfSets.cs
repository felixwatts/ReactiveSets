using System;
using System.Collections.Generic;
using System.Linq;

namespace ReactiveSets
{
    public class UnionOfSets<TIdSet, TId, TPayload> : SetOfSetsToSet<TIdSet, TId, TPayload, TPayload>
    {
        private Dictionary<TIdSet, HashSet<TId>> _idsBySet;
        private Dictionary<TId, uint> _countById;

        public UnionOfSets(IObservable<IDelta<TIdSet, IObservable<IDelta<TId, TPayload>>>> source) : base(source)
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

        protected override void OnSetSet(TIdSet id, IObservable<IDelta<TId, TPayload>> payload)
        {
            _idsBySet.Add(id, new HashSet<TId>());
            base.OnSetSet(id, payload);            
        }

        protected override void OnDeleteSet(TIdSet setId)
        {
            base.OnDeleteSet(setId);

            foreach(var id in _idsBySet[setId].ToArray())
            {
                OnDeleteItem(setId, id);
            }

            _idsBySet.Remove(setId);
        }

        protected override void OnSetItem(TIdSet setId, TId id, TPayload payload)
        {
            var incrementCount = _idsBySet[setId].Add(id);
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
            foreach(var id in _idsBySet[setId].ToArray())
            {
                OnDeleteItem(setId, id);
            }
        }
    }
}