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
            var isIncluded = _content.ContainsKey(id);

            if(shouldInclude == isIncluded) return;

            if(shouldInclude)
                _content.SetItem(id, payload);
            else _content.DeleteItem(id);
        }
    }
}