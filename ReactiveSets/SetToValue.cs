using System;
using System.Reactive.Subjects;

namespace ReactiveSets
{
    internal abstract class SetToValue<TIdIn, TPayloadIn, TPayloadOut> : IValue<TPayloadOut>, IObserver<Delta<TIdIn, TPayloadIn>>
    {
        protected readonly Value<TPayloadOut> _value;
        private readonly IObservable<Delta<TIdIn, TPayloadIn>> _source;
        protected uint _bulkUpdateNestDepth;

        public TPayloadOut Current => _value.Current;

        protected SetToValue(IObservable<Delta<TIdIn, TPayloadIn>> source, TPayloadOut initialValue)
        {
            _value = new Value<TPayloadOut>(initialValue, SubscribeToSource);
            _source = source;
        }

        public virtual void OnCompleted()
        {
            _value.OnCompleted();
        }

        public virtual void OnError(Exception error)
        {
            _value.OnError(error);
        }

        public virtual void OnNext(Delta<TIdIn, TPayloadIn> next)
        {
            switch(next.Type)
            {
                case DeltaType.BeginBulkUpdate:
                    OnBeginBulkUpdate();
                    break;
                case DeltaType.EndBulkUpdate:
                    OnEndBulkUpdate();
                    break;
                case DeltaType.SetItem:
                    OnSetItem(next.Id, next.Payload);
                    break;
                case DeltaType.DeleteItem:
                    OnDeleteItem(next.Id);
                    break;
                case DeltaType.Clear:
                    OnClear();
                    break;
            }
        }

        protected abstract void OnClear();

        protected abstract void OnDeleteItem(TIdIn id);

        protected abstract void OnSetItem(TIdIn id, TPayloadIn payload);

        protected virtual void OnEndBulkUpdate()
        {
            _bulkUpdateNestDepth--;
        }

        protected virtual void OnBeginBulkUpdate()
        {
            _bulkUpdateNestDepth++;
        }

        public IDisposable Subscribe(IObserver<TPayloadOut> observer)
        {
            return _value.Subscribe(observer);
        }

        private IDisposable SubscribeToSource()
        {
            return _source.Subscribe(this);
        }
    }

}