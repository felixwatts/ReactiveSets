using System;
using System.Reactive;
using System.Reactive.Disposables;

namespace ReactiveSets
{   
    public abstract class SetToSet<TIdIn, TPayloadIn, TIdOut, TPayloadOut> : ISet<TIdOut, TPayloadOut>, IObserver<IDelta<TIdIn, TPayloadIn>>
    {
        protected readonly Set<TIdOut, TPayloadOut> _content;
        private readonly IObservable<IDelta<TIdIn, TPayloadIn>> _source;

        protected SetToSet(IObservable<IDelta<TIdIn, TPayloadIn>> source)
        {
            _source = source;
            _content = new Set<TIdOut, TPayloadOut>(SubscribeToSource);
        }

        public IDisposable Subscribe(IObserver<IDelta<TIdOut, TPayloadOut>> observer)
        {
            return _content.Subscribe(observer);
        }

        public virtual void OnCompleted()
        {
            _content.OnCompleted();
        }

        public virtual void OnError(Exception error)
        {
            _content.OnError(error);
        }

        public virtual void OnNext(IDelta<TIdIn, TPayloadIn> next)
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
        protected abstract void Reset();  

        private IDisposable SubscribeToSource()
        {
            var sub = _source.Subscribe(this);
            return Disposable.Create(() =>
            {
                sub.Dispose();
                Reset();
            });
        }      
    } 
}