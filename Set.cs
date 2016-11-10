using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace ReactiveSets
{   
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

        public bool Contains(TId id)
        {
            return _content.ContainsKey(id);
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
                observer.OnNext(delta);
            }

            observer.OnNext(Delta<TId, TPayload>.EndBulkUpdate);
        }
    }
}