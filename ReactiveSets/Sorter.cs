using System;
using System.Collections.Generic;

namespace ReactiveSets
{
    internal class Sorter<TId, TPayload> : SetToSet<TId, TPayload, int, TPayload>
    {
        private readonly Func<TPayload, TPayload, int> _comparison;
        private readonly Dictionary<TId, TPayload> _payloadById;

        public Sorter(IObservable<IDelta<TId, TPayload>> source, Func<TPayload, TPayload, int> comparison) 
            : base(source)
        {
            _comparison = comparison;
            _payloadById = new Dictionary<TId, TPayload>();
        }

        protected override void Reset()
        {
            _payloadById.Clear();
        }

        protected override void OnSetItem(TId id, TPayload payload)
        {
            var newIndex = FindIndex(payload);
            var oldIndex = _content.Count;

            if(_payloadById.TryGetValue(id, out var oldPayload))            
                oldIndex = FindIndex(oldPayload);
            
            _payloadById[id] = payload;

            if(oldIndex == newIndex)
            {
                _content.SetItem(newIndex, payload);
            }
            else if(oldIndex > newIndex)
            {
                _content.BeginBulkUpdate();
                for(int i = oldIndex - 1; i >= newIndex; i--)                
                    _content.SetItem(i+1, _content[i]); 
                _content.SetItem(newIndex, payload);
                _content.EndBulkUpdate();
            }
            else
            {
                _content.BeginBulkUpdate();
                for(int i = oldIndex + 1; i <= newIndex; i++)                
                    _content.SetItem(i-1, _content[i]);                
                _content.SetItem(newIndex, payload);
                _content.EndBulkUpdate();
            }
        }

        protected override void OnDeleteItem(TId id)
        {
            var oldPayload = _payloadById[id];            
            _payloadById.Remove(id);
            var oldIndex = FindIndex(oldPayload);
            var end = _content.Count;

            _content.BeginBulkUpdate();
            for(int i = oldIndex + 1; i < end; i++)                
                _content.SetItem(i-1, _content[i]);
            _content.EndBulkUpdate();
        }

        private int FindIndex(TPayload payload)
        {
            return FindIndexInSubArray(0, _content.Count);

            int FindIndexInSubArray(int start, int end)
            {
                if(start == end) return start;
                
                int middle = (end - start) / 2;
                var middlePayload = _content[middle];

                var comparison = _comparison(payload, middlePayload);
                if(comparison > 0)
                    return FindIndexInSubArray(middle+1, end);
                else if(comparison < 0)
                    return FindIndexInSubArray(start, middle);
                else return middle;
            }            
        }
    }
}