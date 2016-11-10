using System;
using System.Collections.Generic;

namespace ReactiveSets
{

    internal class Aggregator<TIdIn, TPayloadIn, TPayloadOut> : SetToValue<TIdIn, TPayloadIn, TPayloadOut>
    {
        private readonly Func<IEnumerable<TPayloadIn>, TPayloadOut> _aggregate;
        private readonly Dictionary<TIdIn, TPayloadIn> _content;

        public Aggregator(IObservable<Delta<TIdIn, TPayloadIn>> source, Func<IEnumerable<TPayloadIn>, TPayloadOut> aggregate) : base(source)
        {
            _aggregate = aggregate;
            _content = new Dictionary<TIdIn, TPayloadIn>();
        }
    
        protected override void OnEndBulkUpdate()
        {
            base.OnEndBulkUpdate();

            if(_bulkUpdateNestDepth == 0)
                ReaggregateAndPublish();
        }

        protected override void OnClear()
        {
            _value.OnNext(default(TPayloadOut));
        }

        protected override void OnDeleteItem(TIdIn id)
        {
            _content.Remove(id);
            ReaggregateAndPublish();
        }

        protected override void OnSetItem(TIdIn id, TPayloadIn payload)
        {
            _content.Add(id, payload);
            ReaggregateAndPublish();
        }

        private void ReaggregateAndPublish()
        {
            if(_bulkUpdateNestDepth > 0) return;

            _value.OnNext(_aggregate(_content.Values));
        }
    }
}