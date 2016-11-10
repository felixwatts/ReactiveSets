using System;
using System.Collections.Generic;
using System.Linq;

namespace ReactiveSets
{
    internal class DynamicSubsetter<TId, TPayload, TDynamic> : SetToSet<TId, TPayload, TId, TPayload>
    {
        private readonly Func<TPayload, IObservable<TDynamic>> _payloadToObservable; 
        private readonly Predicate<TDynamic> _condition;
        private readonly Dictionary<TId, IDisposable> _subscriptionById;

        public DynamicSubsetter(IObservable<Delta<TId, TPayload>> source, Func<TPayload, IObservable<TDynamic>> payloadToObservable, Predicate<TDynamic> condition) 
            : base(source)
        {
            _payloadToObservable = payloadToObservable;
            _condition = condition;
            _subscriptionById = new Dictionary<TId, IDisposable>();
        }

        protected override void OnClear()
        {        
            _subscriptionById.Values.DisposeAll(); 
            _subscriptionById.Clear();          
            base.OnClear();
        }

        protected override void OnDeleteItem(TId id)
        {
            _subscriptionById[id].Dispose();
            _subscriptionById.Remove(id);

            if(_content.Contains(id))
                _content.DeleteItem(id);
        }

        protected override void OnSetItem(TId id, TPayload payload)
        {
            IDisposable existingSubscription = null;
            _subscriptionById.TryGetValue(id, out existingSubscription);
            existingSubscription?.Dispose();

            var newSubscription = _payloadToObservable(payload).Subscribe(next => OnDynamicNext(id, next, payload));
            _subscriptionById[id] = newSubscription;
        }

        private void OnDynamicNext(TId id, TDynamic next, TPayload payload)
        {
            var shouldInclude = _condition(next);
            var isIncluded = _content.Contains(id);

            if(shouldInclude == isIncluded) return;

            if(shouldInclude)
                _content.SetItem(id, payload);
            else _content.DeleteItem(id);
        }
    }
} 