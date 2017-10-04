using System;

namespace ReactiveSets
{
    internal class Subsetter<TId, TPayload> : SetToSet<TId, TPayload, TId, TPayload>
    {
        private readonly Func<TId, TPayload, bool> _condition;

        public Subsetter(IObservable<Delta<TId, TPayload>> source, Func<TId, TPayload, bool> condition)
            : base(source)
        {
            _condition = condition;
        }

        protected override void OnDeleteItem(TId id)
        {
            if(_content.ContainsKey(id))
                _content.DeleteItem(id);
        }

        protected override void OnSetItem(TId id, TPayload payload)
        {
            var shouldInclude = _condition(id, payload);            

            if(shouldInclude)
            {
                _content.SetItem(id, payload);
            }
            else
            {
                var isIncluded = _content.ContainsKey(id);
                
                if(isIncluded)
                {
                    _content.DeleteItem(id);
                }                
            }
        }

        protected override void Reset()
        {
            // no op
        }
    }
}