using System;
using System.Reactive;

namespace ReactiveSets
{   
    public abstract class SetToSet<TIdIn, TPayloadIn, TIdOut, TPayloadOut> : ISet<TIdOut, TPayloadOut>, IObserver<Delta<TIdIn, TPayloadIn>>
    {
        protected readonly Set<TIdOut, TPayloadOut> _content;

        protected SetToSet(IObservable<Delta<TIdIn, TPayloadIn>> source)
        {
            _content = new Set<TIdOut, TPayloadOut>(() => source.Subscribe(this));
        }

        public virtual void OnCompleted()
        {
            _content.OnCompleted();
        }

        public virtual void OnError(Exception error)
        {
            _content.OnError(error);
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
                case DeltaType.Clear:
                    OnClear();
                    break;
                case DeltaType.SetItem:
                    OnSetItem(next.Id, next.Payload);
                    break;
                case DeltaType.DeleteItem:
                    OnDeleteItem(next.Id);
                    break;
            }
        }

        protected virtual void OnBeginBulkUpdate()
        {
            _content.BeginBulkUpdate();
        }

        protected virtual void OnEndBulkUpdate()
        {
            _content.EndBulkUpdate();
        }

        protected virtual void OnClear()
        {
            _content.Clear();
        }

        protected abstract void OnSetItem(TIdIn id, TPayloadIn payload);
        protected abstract void OnDeleteItem(TIdIn id);

        public IDisposable Subscribe(IObserver<Delta<TIdOut, TPayloadOut>> observer)
        {
            return _content.Subscribe(observer);
        }
    } 
}