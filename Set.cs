using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace ReactiveSets
{
    public abstract class SetToSet<TIdIn, TPayloadIn, TIdOut, TPayloadOut> : ISet<TIdOut, TPayloadOut>, IObserver<Delta<TIdIn, TPayloadIn>>
    {
        protected readonly Set<TIdOut, TPayloadOut> _content;

        public SetToSet(ISet<TIdIn, TPayloadIn> source)
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

    public class Mapper<TIdIn, TPayloadIn, TIdOut, TPayloadOut> : SetToSet<TIdIn, TPayloadIn, TIdOut, TPayloadOut>
    {
        private readonly Func<TIdIn, TIdOut> _idMapping;
        private readonly Func<TIdIn, TPayloadIn, TPayloadOut> _payloadMapping;
        private readonly bool _disposeOnDelete;

        public Mapper(ISet<TIdIn, TPayloadIn> source, Func<TIdIn, TIdOut> idMapping, Func<TIdIn, TPayloadIn, TPayloadOut> payloadMapping, bool disposeOnDelete = true)
            : base(source)
        {
            _idMapping = idMapping;
            _payloadMapping = payloadMapping;
            _disposeOnDelete = disposeOnDelete;
        }

        protected override void OnSetItem(TIdIn id, TPayloadIn payload)
        {
            var idOut = _idMapping(id);
            var payloadOut = _payloadMapping(id, payload);

            _content.SetItem(idOut, payloadOut);
        }

        protected override void OnDeleteItem(TIdIn id)
        {
            var idOut = _idMapping(id);  

            var oldItem = _content[idOut];    

            _content.DeleteItem(idOut);

            if(_disposeOnDelete)
            {                
                (oldItem as IDisposable)?.Dispose();
            }    
        }
    } 

    public class Set<TId, TPayload> : ISet<TId, TPayload>, IObserver<Delta<TId, TPayload>>
    {
        private readonly ISubject<Delta<TId, TPayload>> _subscribers;
        private readonly Dictionary<TId, TPayload> _content;
        private uint _bulkUpdateNestDepth;

        public Set(Func<IDisposable> subscribeToSource = null)
        {
            if(subscribeToSource == null)
            {
                _subscribers = new FastSubject<Delta<TId, TPayload>>();
            }
            else
            {
                subscribeToSource = () =>
                {
                    var sub = subscribeToSource();
                    return Disposable.Create(() =>
                    {
                        sub.Dispose();
                        _content.Clear();
                        _bulkUpdateNestDepth = 0;
                    });
                };

                _subscribers = new ReferenceCountingSubject<Delta<TId, TPayload>>(subscribeToSource);
            }
        }

        public TPayload this[TId id]
        {
            get{ return _content[id]; }
        }

        public void SetItem(TId id, TPayload payload)
        {
            _content[id] = payload;
            var delta = Delta<TId, TPayload>.SetItem(id, payload);
            _subscribers.OnNext(delta);
        }

        public void DeleteItem(TId id)
        {
            if(!_content.ContainsKey(id)) throw new InvalidOperationException("Cannot remove non-existent item");

            _content.Remove(id);
            var delta = Delta<TId, TPayload>.DeleteItem(id);
            _subscribers.OnNext(delta);
        }

        public void BeginBulkUpdate()
        {
            _bulkUpdateNestDepth++;
            _subscribers.OnNext(Delta<TId, TPayload>.BeginBulkUpdate);
        }

        public void EndBulkUpdate()
        {
            if(_bulkUpdateNestDepth == 0) throw new InvalidOperationException("Bulk update nest depth < 0");

            _bulkUpdateNestDepth--;
            _subscribers.OnNext(Delta<TId, TPayload>.EndBulkUpdate);
        }

        public void Clear()
        {
            _content.Clear();
            _subscribers.OnNext(Delta<TId, TPayload>.Clear);
        }        

        public void OnCompleted()
        {
            _subscribers.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _subscribers.OnError(error);
        }

        public void OnNext(Delta<TId, TPayload> value)
        {
            switch(value.Type)
            {
                case DeltaType.SetItem:
                    SetItem(value.Id, value.Payload);
                    break;
                case DeltaType.DeleteItem:
                    DeleteItem(value.Id);
                    break;
                case DeltaType.BeginBulkUpdate:
                    BeginBulkUpdate();
                    break;
                case DeltaType.EndBulkUpdate:
                    EndBulkUpdate();
                    break;
                case DeltaType.Clear:
                    Clear();
                    break;
            }
        }

        public IDisposable Subscribe(IObserver<Delta<TId, TPayload>> observer)
        {
            SendCurrentContentToSubscriber(observer);
            return _subscribers.Subscribe(observer);
        }

        private void SendCurrentContentToSubscriber(IObserver<Delta<TId, TPayload>> observer)
        {
            for(int n = 0; n < _bulkUpdateNestDepth; n++)
                observer.OnNext(Delta<TId, TPayload>.BeginBulkUpdate);

            observer.OnNext(Delta<TId, TPayload>.BeginBulkUpdate);

            foreach(var kvp in _content)
            {
                var delta = Delta<TId, TPayload>.SetItem(kvp.Key, kvp.Value);
            }

            observer.OnNext(Delta<TId, TPayload>.EndBulkUpdate);
        }
    }
}