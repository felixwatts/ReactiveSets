using System;
using System.Collections.Generic;
using System.Linq;

namespace ReactiveSets
{
    internal abstract class DynamicToSet<TId, TPayloadIn, TDynamic, TPayloadOut> : SetToSet<TId, TPayloadIn, TId, TPayloadOut>
    {
        private readonly Func<TPayloadIn, IObservable<TDynamic>> _payloadToObservable; 
        private readonly Dictionary<TId, IDisposable> _subscriptionById;

        public DynamicToSet(
            IObservable<IDelta<TId, TPayloadIn>> source, 
            Func<TPayloadIn, IObservable<TDynamic>> payloadToObservable) 
            : base(source)
        {
            _payloadToObservable = payloadToObservable;
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

            if(_content.ContainsKey(id))
                _content.DeleteItem(id);
        }

        protected override void OnSetItem(TId id, TPayloadIn payload)
        {
            _subscriptionById.TryGetValue(id, out var existingSubscription);
            existingSubscription?.Dispose();

            var observable = _payloadToObservable(payload);

            var newSubscription = observable.Subscribe(next => OnDynamicNext(id, next, payload));
            _subscriptionById[id] = newSubscription;
        }

        protected override void Reset()
        {
            _subscriptionById.Values.DisposeAll();
            _subscriptionById.Clear();
        }

        protected abstract void OnDynamicNext(TId id, TDynamic next, TPayloadIn payload);
    }
}